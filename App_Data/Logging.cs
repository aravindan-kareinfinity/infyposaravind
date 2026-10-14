using InfyPOS.Common;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using WebAPI.Data;
using WebAPI.Data.Sql;

namespace WebAPI.Common
{
    public class Logger : InfyPOS.Common.BaseData,IDisposable
    {

        private long organizationid;
        public Logger(long organizationid):this()
        {
            this.organizationid = organizationid;
            this.Context = new InfyPOS.Common.UserClaim()
            {
                OrganizationID = organizationid
            };
        }

        private static System.Collections.Concurrent.ConcurrentDictionary<long, Logger> instanceList = new System.Collections.Concurrent.ConcurrentDictionary<long, Logger>();
        public new static Logger Instance(long organizationid)
        {
            Logger instance = null;
            if (instanceList.TryGetValue(organizationid, out instance))
                return instance;

            instance = new Logger(organizationid);
            if (!instanceList.TryAdd(organizationid, instance))
                instanceList.TryGetValue(organizationid, out instance);
            return instance;

        }


        private static bool? logenabled = null;
        private static bool? logall = null;
        public bool LogEnabled()
        {
            if (!logenabled.HasValue)
            {
                logenabled = System.Configuration.ConfigurationManager.AppSettings["LogEnabled"] != null &&
                    System.Configuration.ConfigurationManager.AppSettings["LogEnabled"].ToString().ToLower() == "true";
            }
            return logenabled.Value;
        }

        public bool LogAll()
        {
            if (!logall.HasValue)
            {
                logall = System.Configuration.ConfigurationManager.AppSettings["LogAll"] != null &&
                    System.Configuration.ConfigurationManager.AppSettings["LogAll"].ToString().ToLower() == "true";
            }
            return logall.Value;
        }

        System.Timers.Timer logtimer;
        public void SetLogEnable(bool enabled)
        {
            logenabled = enabled;
            if (enabled)
            {
                if (logtimer == null)
                {
                    logtimer = new System.Timers.Timer(new TimeSpan(0, 30, 0).TotalMilliseconds);
                    logtimer.Elapsed += Logtimer_Elapsed;
                    logtimer.Enabled = true;
                    logtimer.Start();
                }
            }
            else if (logtimer != null && !logtimer.Enabled)
            {
                logtimer.Stop();
                logtimer.Enabled = false;
            }
        }

        private void Logtimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            logenabled = false;
            logtimer.Stop();
        }

        public enum LogType
        {
            Request,
            Response,
            Error,
            Info,
            Sync
        }
        private static readonly log4net.ILog FileLog = log4net.LogManager.GetLogger(typeof(Logger));
        //static Logger l = null;
        //static Logger()
        //{
        //    l = new Logger();
        //}
        bool isdbAvailable = false;
        private Logger()
        {
            //connection = CreateConnection();
            try
            {
                //connection.Open();
                using (NpgsqlConnection connection = CreateConnection())
                {
                    connection.Open();
                    isdbAvailable = true;
                    connection.Close();
                }
            }
            catch (Exception exception)
            {
                isdbAvailable = false;
                FileLog.Error(exception);
            }

        }

        public void DirectLog(Exception information)
        {
            FileLog.Error(information);
        }

        public void DirectLog(string information)
        {
            FileLog.Info(information);
        }
        /*internal NpgsqlConnection CreateConnection()
        {
            string connstring = System.Configuration.ConfigurationManager.ConnectionStrings["InfyPOS"].ConnectionString;
            return new NpgsqlConnection(connstring);
        }*/


        public void Log(Exception exception, LogType type, object data)
        {
            Log(exception, type, Environment.StackTrace, data);
            if(type == LogType.Error && data != null && data.GetType() != typeof(String))
                throw exception;
        }

        public void Log(string exception, LogType type, object data)
        {
            Log(exception, type, Environment.StackTrace, data);
        }
        public void Log(Exception exception, LogType type, string requesturl, object data)
        {
            if (!LogEnabled() && (type != LogType.Error && type != LogType.Sync && LogAll() == false)) return;

            if (!isdbAvailable)
            {
                FileLog.Error(exception);
                return;
            }
            try
            {
                using (NpgsqlConnection connection = CreateConnection())
                {
                    connection.Open();
                    NpgsqlCommand command = new NpgsqlCommand(@"INSERT INTO public.trace(
                            type, loggedat, info,requesturl, ""methodName"") VALUES(@type, @loggedat, @info,@requesturl, @methodName)", connection);

                    command.Parameters.Add("type", NpgsqlTypes.NpgsqlDbType.Varchar).Value = type.ToString();
                    command.Parameters.Add("methodName", NpgsqlTypes.NpgsqlDbType.Json).Value = new InfyException(exception).ToJSON();
                    command.Parameters.Add("requesturl", NpgsqlTypes.NpgsqlDbType.Json).Value = requesturl.ToJSON();
                    command.Parameters.Add("info", NpgsqlTypes.NpgsqlDbType.Json).Value = data.ToJSON();
                    command.Parameters.Add("loggedat", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
                    command.ExecuteNonQuery();
                    connection.Close();
                }
            }
            catch (Exception exp)
            {
                FileLog.Error(exception);
            }
        }

        public void DBLog(string log)
        {
            try
            {
                using (NpgsqlConnection connection = CreateConnection())
                {
                    connection.Open();
                    NpgsqlCommand command = new NpgsqlCommand(@"INSERT INTO public.trace(
                            type, loggedat, info) VALUES(@type, @loggedat, @info,@requesturl, @methodName,@userid,@locationid)", connection);

                    command.Parameters.Add("type", NpgsqlTypes.NpgsqlDbType.Varchar).Value = "DB";
                    command.Parameters.Add("info", NpgsqlTypes.NpgsqlDbType.Json).Value = log.ToJSON();
                    command.Parameters.Add("loggedat", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
                    command.ExecuteNonQuery();
                    connection.Close();
                }
            }
            catch (Exception exp)
            {

            }
        }

        public void PerformanceLog(string title, LogType type, string requesturl, object data, long userid, long locationid)
        {
            try
            {
                using (NpgsqlConnection connection = CreateConnection())
                {
                    connection.Open();
                    NpgsqlCommand command = new NpgsqlCommand(@"INSERT INTO public.trace(
                            type, loggedat, info,requesturl, ""methodName"",userid,locationid) VALUES(@type, @loggedat, @info,@requesturl, @methodName,@userid,@locationid)", connection);

                    command.Parameters.Add("type", NpgsqlTypes.NpgsqlDbType.Varchar).Value = type.ToString();
                    command.Parameters.Add("methodName", NpgsqlTypes.NpgsqlDbType.Json).Value = title.ToJSON();
                    command.Parameters.Add("requesturl", NpgsqlTypes.NpgsqlDbType.Json).Value = requesturl == null?"null": requesturl.ToJSON();
                    command.Parameters.Add("info", NpgsqlTypes.NpgsqlDbType.Json).Value = data == null? "null": data.ToJSON();
                    command.Parameters.Add("loggedat", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
                    command.Parameters.Add("userid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = userid;
                    command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = locationid;
                    command.ExecuteNonQuery();
                    connection.Close();
                }
            }
            catch (Exception exp)
            {
                
            }
        }
        public void Log(Exception exception, LogType type, string requesturl, object data,long userid,long locationid)
        {
            if (!LogEnabled() && (type != LogType.Error && type != LogType.Sync && LogAll() == false)) return;

            if (!isdbAvailable)
            {
                FileLog.Error(exception);
                return;
            }
            try
            {
                using (NpgsqlConnection connection = CreateConnection())
                {
                    connection.Open();
                    NpgsqlCommand command = new NpgsqlCommand(@"INSERT INTO public.trace(
                            type, loggedat, info,requesturl, ""methodName"",userid,locationid) VALUES(@type, @loggedat, @info,@requesturl, @methodName,@userid,@locationid)", connection);

                    command.Parameters.Add("type", NpgsqlTypes.NpgsqlDbType.Varchar).Value = type.ToString();
                    command.Parameters.Add("methodName", NpgsqlTypes.NpgsqlDbType.Json).Value = new InfyException(exception).ToJSON();
                    command.Parameters.Add("requesturl", NpgsqlTypes.NpgsqlDbType.Json).Value = requesturl.ToJSON();
                    command.Parameters.Add("info", NpgsqlTypes.NpgsqlDbType.Json).Value = data.ToJSON();
                    command.Parameters.Add("loggedat", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
                    command.Parameters.Add("userid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = userid;
                    command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = locationid;
                    command.ExecuteNonQuery();
                    connection.Close();
                }
            }
            catch (Exception exp)
            {
                FileLog.Error(exception);
            }
        }

        public class InfyException
        {
            public string InnerException { get; set; }
            public string StackTrace { get; set; }
            public string Exception { get; set; }
            public InfyException()
            {

            }

            public InfyException(Exception exp)
            {
                InnerException = exp.InnerException != null ? exp.InnerException.Message : "";
                Exception = exp.Message;
                StackTrace = exp.StackTrace;
            }
        }
        public void Log(string methodName, LogType type, string requesturl, object data)
        {
            if (!LogEnabled() && type != LogType.Error && type != LogType.Sync) return;

            if (!isdbAvailable)
            {
                return;
            }
            try
            {
                using (NpgsqlConnection connection = CreateConnection())
                {
                    connection.Open();
                    NpgsqlCommand command = new NpgsqlCommand(@"INSERT INTO public.trace(
                            type, loggedat, info,requesturl, ""methodName"") VALUES(@type, @loggedat, @info,@requesturl, @methodName)", connection);

                    command.Parameters.Add("type", NpgsqlTypes.NpgsqlDbType.Varchar).Value = type.ToString();
                    command.Parameters.Add("methodName", NpgsqlTypes.NpgsqlDbType.Json).Value = methodName.ToJSON();
                    command.Parameters.Add("requesturl", NpgsqlTypes.NpgsqlDbType.Json).Value = requesturl.ToJSON();
                    command.Parameters.Add("info", NpgsqlTypes.NpgsqlDbType.Json).Value = data.ToJSON();
                    command.Parameters.Add("loggedat", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
                    command.ExecuteNonQuery();
                    connection.Close();
                }
            }
            catch (Exception exception)
            {
                FileLog.Error(exception);
            }
        }

        public void Dispose()
        {
            //if (connection != null && connection.State == System.Data.ConnectionState.Open)
            //{
            //    connection.Close();
            //}
        }

        internal void Log(Exception exception, LogType type, Object data, long userID, long locationid)
        {
            Log(exception, type, Environment.StackTrace, data, userID, locationid);
        }
    }


    public class AuditLogger : InfyPOS.Common.BaseData,IDisposable
    {
        public enum Types
        {
            agent,
            agentpayment,
            bankchequetransaction,
            bill,
            billcreditnote,
            billinbox,
            billsettlement,
            bombundles,
            bomconsumption,
            bominvoice,
            bominvoicereturn,
            bomlrentry,
            bommaster,
            bomrequest,
            brand,
            bulkbill,
            bundles,
            buyback,
            cash,
            company,
            componentpo,
            configuration,
            coupons,
            courier,
            customer,
            customerdivert,
            customerorder,
            customerpayment,
            dailyprice,
            deathonarrival,
            delivery,
            discountscheme,
            duplicate,
            employee,
            employeeinout,
            employeeleave,
            employeesalary,
            employeesalaryschedule,
            employeesalarystructure,
            employeetarget,
            holiday,
            inbox,
            incentiveconfig,
            incentives,
            intray,
            items,
            jobworkbill,
            jobworkbillgrn,
            jobworksamples,
            leadmanagement,
            leaverequest,
            ledger,
            loansettlement,
            lrentry,
            lrinvoice,
            lrinvoicereturn,
            lrissue,
            lrreceived,
            mfg_samples,
            mfg_swatches,
            mfg_swatches_folder,
            mfg_swatches_history,
            mfgbinstorage,
            mfgdesign,
            mfgjoborder,
            mfgtailoring,
            mfgworkfloworchestration,
            mfgworkorder,
            mfgworkordergrn,
            mfgworkorderinbox,
            missingstock,
            organisation,
            packingslip,
            payments,
            pricechange,
            pricecontrol,
            pricedrop,
            productgroup,
            productrequestform,
            products,
            promotions,
            purchaseorder,
            recharge,
            referencelist,
            refillbox,
            salesorder,
            schedule,
            supplier,
            supplierpayment,
            suppliertradeagreement,
            tailoringorder,
            tailoringpayment,
            tax,
            transport,
            transportpayment,
            users,
            usersession,
            voucher,
            vouchers
        }

        private long organizationid;
        public AuditLogger(long organizationid):this()
        {
            this.organizationid = organizationid;
            this.Context = new InfyPOS.Common.UserClaim()
            {
                OrganizationID = organizationid
            };
        }

        private static System.Collections.Concurrent.ConcurrentDictionary<long, AuditLogger> instanceList = new System.Collections.Concurrent.ConcurrentDictionary<long, AuditLogger>();
        public new static AuditLogger Instance(long organizationid)
        {
            AuditLogger instance = null;
            if (instanceList.TryGetValue(organizationid, out instance))
                return instance;

            instance = new AuditLogger(organizationid);
            if (!instanceList.TryAdd(organizationid, instance))
                instanceList.TryGetValue(organizationid, out instance);
            return instance;
        }

        private static bool? logenabled = null;
        private static bool? logall = null;
        public bool LogEnabled()
        {
            if (!logenabled.HasValue)
            {
                logenabled = System.Configuration.ConfigurationManager.AppSettings["AuditEnabled"] != null &&
                    System.Configuration.ConfigurationManager.AppSettings["AuditEnabled"].ToString().ToLower() == "true";
            }
            return logenabled.Value;
        }



        bool isdbAvailable = false;
        private AuditLogger()
        {
            //connection = CreateConnection();
            try
            {
                //connection.Open();
                using (NpgsqlConnection connection = CreateConnection())
                {
                    connection.Open();
                    isdbAvailable = true;
                    connection.Close();
                }
            }
            catch (Exception exception)
            {
                isdbAvailable = false;
            }

        }


        public void Log(Types type,UserClaim context,string reference,long referenceid, object data)
        {
            using (NpgsqlConnection connection = CreateConnection())
            {
                connection.Open();
                NpgsqlCommand command = new NpgsqlCommand(@"INSERT INTO public.audittrial(
                            altype,refid,reference,logdata,userid,empid,trialon) 
                            VALUES(@altype,@refid,@reference,@logdata,@userid,@empid,@trialon)", connection);

                command.Parameters.Add("altype", NpgsqlTypes.NpgsqlDbType.Varchar).Value = type.ToString();
                command.Parameters.Add("refid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = referenceid;
                command.Parameters.Add("reference", NpgsqlTypes.NpgsqlDbType.Varchar).Value = string.IsNullOrEmpty(reference) ? "" : reference;
                command.Parameters.Add("logdata", NpgsqlTypes.NpgsqlDbType.Json).Value = data.ToJSON();
                command.Parameters.Add("trialon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
                command.ExecuteNonQuery();
                connection.Close();
            }
        }

        public void Dispose()
        {
            //if (connection != null && connection.State == System.Data.ConnectionState.Open)
            //{
            //    connection.Close();
            //}
        }


    }
    public abstract class MessageHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            byte[] responseMessage;
            var requestInfo = "";
            var corrId = "";
            try
            {
                if (request.RequestUri.AbsoluteUri.Contains(@"/signalr/"))
                    return await base.SendAsync(request, cancellationToken);

                if (cancellationToken.IsCancellationRequested)
                    return new HttpResponseMessage(System.Net.HttpStatusCode.Unused);

                corrId = string.Format("{0}_{1}", DateTime.Now.Ticks, Thread.CurrentThread.ManagedThreadId);
                requestInfo = string.Format("{0} {1}", request.Method, request.RequestUri);

                var requestMessage = await request.Content.ReadAsByteArrayAsync();

                if (cancellationToken.IsCancellationRequested)
                    return new HttpResponseMessage(System.Net.HttpStatusCode.Unused);

                await IncommingMessageAsync(corrId, requestInfo, requestMessage);

                if (cancellationToken.IsCancellationRequested)
                    return new HttpResponseMessage(System.Net.HttpStatusCode.Unused);

                HttpResponseMessage response = null;

                if (!cancellationToken.IsCancellationRequested)
                {
                    
                    response = await base.SendAsync(request, cancellationToken);
                    if (response.IsSuccessStatusCode)
                        responseMessage = await response.Content.ReadAsByteArrayAsync();
                    else if(response.Content != null && response.StatusCode == (System.Net.HttpStatusCode)600)
                        responseMessage = await response.Content.ReadAsByteArrayAsync();
                    else if (response.Content != null && response.StatusCode == (System.Net.HttpStatusCode)601)
                        responseMessage = await response.Content.ReadAsByteArrayAsync();
                    else
                        responseMessage = System.Text.Encoding.UTF8.GetBytes(response.ReasonPhrase);

                    await OutgoingMessageAsync(corrId, requestInfo, responseMessage);
                }
                return response;
            }
            catch (Exception exp)
            {
                Logger.Instance(0).Log(exp, Logger.LogType.Error, requestInfo, "");
                return new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError)
                {
                    ReasonPhrase = exp.Message + "\r\n" + exp.StackTrace
                };
            }
        }


        protected abstract Task IncommingMessageAsync(string correlationId, string requestInfo, byte[] message);
        protected abstract Task OutgoingMessageAsync(string correlationId, string requestInfo, byte[] message);
    }

    public class CancelledTaskBugWorkaroundMessageHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            try
            {
                HttpResponseMessage response = await base.SendAsync(request, cancellationToken);

                // Try to suppress response content when the cancellation token has fired; ASP.NET will log to the Application event log if there's content in this case.
                if (cancellationToken.IsCancellationRequested)
                {
                    return new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError);
                }

                return response;
            }catch(Exception exp)
            {
                return new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError);
            }
        }
    }

    public class AllowOptionsHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            HttpResponseMessage response = null;
            if (request.Method == HttpMethod.Options)
            {
                response = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
            }
            else
            {
                response = await base.SendAsync(request, cancellationToken);
            }
            return response;
        }
    }

    public class MessageLoggingHandler : MessageHandler
    {

        protected override async Task IncommingMessageAsync(string correlationId, string requestInfo, byte[] message)
        {
            if (string.IsNullOrEmpty(requestInfo) || requestInfo.Contains(".js") || requestInfo.Contains(".htm")) return;
            await Task.Run(() =>
            Logger.Instance(0).Log("Request", Logger.LogType.Info, requestInfo, Encoding.UTF8.GetString(message)));
        }

        protected override async Task OutgoingMessageAsync(string correlationId, string requestInfo, byte[] message)
        {
            if (string.IsNullOrEmpty(requestInfo) || requestInfo.Contains(".js") || requestInfo.Contains(".htm")) return;
            await Task.Run(() =>
                Logger.Instance(0).Log("Response", Logger.LogType.Info, requestInfo, Encoding.UTF8.GetString(message)));
        }
    }
}