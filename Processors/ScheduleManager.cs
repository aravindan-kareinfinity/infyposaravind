using InfyPOS.Common;
using InfyPOS.Controllers;
using InfyPOS.Data;
using InfyPOS.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Net.Mime;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using WebAPI.Data;

namespace InfyPOS.Processors
{
    public class ScheduleManager
    {
        public class ReportResult
        {
            public Report Source { get; set; }
            public List<ReportResultItem> reportResultItems { get; set; }
            public string messagetemplate { get; set; }
            public string title { get; set; }
        }

        public class ReportResultItem
        {
            public List<ItemValue> Values { get; set; }
            public class ItemValue
            {
                public string name { get; set; }
                public string value { get; set; }
                public decimal dvalue { get; set; }
            }
        }
        public abstract class IReportExecutor
        {
            public class CommonRoot
            {
                public CommonRoot()
                {
                    currenttime = DateTime.Now;
                }
                public string title { get; set; }
                public DateTime fromdate { get; set; }
                public DateTime todate { get; set; }
                public string company { get; set; }
                public string location { get; set; }
                public DateTime currenttime { get; set; }
            }
            public class ReportSource
            {
                public object RootObject { get; set; }
                public object itemList { get; set; }
                public Byte[] file { get; set; }
                public List<SMSDetails> SMSList { get; set; }
            }
            public class SMSDetails
            {
                public string mailid { get; set; }
                public string mobileno { get; set; }
                public string content { get; set; }
                public string coupon { get; set; }
                public object Entity { get; set; }

                public SortedDictionary<string, string> GetReplacements()
                {
                    SortedDictionary<string, string> smsparameters = new SortedDictionary<string, string>();
                    smsparameters.Add("[MAILID]", mailid);
                    smsparameters.Add("[CONTENT]", content);
                    smsparameters.Add("[COUPON]", coupon);

                    if (Entity != null && Entity.GetType() == typeof(Models.Employee))
                    {
                        Employee emp = Entity as Models.Employee;
                        smsparameters.Add("[NAME]", emp.name);
                        smsparameters.Add("[EMPLOYEENAME]", emp.name);

                        if (!string.IsNullOrEmpty(emp.GetMobileNo()))
                        {
                            smsparameters.Add("[MOBILENO]", emp.GetMobileNo());
                            smsparameters.Add("[MOBILE]", emp.GetMobileNo());
                        }
                    }
                    if (Entity != null && Entity.GetType() == typeof(Models.Customer))
                    {
                        Customer emp = Entity as Models.Customer;
                        smsparameters.Add("[NAME]", emp.name);
                        smsparameters.Add("[CUSTOMERNAME]", emp.name);
                        if (!string.IsNullOrEmpty(emp.GetMobileNo()))
                        {
                            smsparameters.Add("[MOBILENO]", emp.GetMobileNo());
                            smsparameters.Add("[MOBILE]", emp.GetMobileNo());
                        }
                    }
                    if (Entity != null && Entity.GetType() == typeof(Models.Customerinvoice))
                    {
                        Customerinvoice emp = Entity as Models.Customerinvoice;
                        smsparameters.Add("[AMOUNT]", emp.receivable.ToString("F0"));
                        smsparameters.Add("[QTY]", emp.qty.ToString("F0"));
                        smsparameters.Add("[NAME]", emp.customername);
                        smsparameters.Add("[CUSTOMERNAME]", emp.customername);
                        if (!string.IsNullOrEmpty(emp.GetMobileNo()))
                        {
                            smsparameters.Add("[MOBILENO]", emp.GetMobileNo());
                            smsparameters.Add("[MOBILE]", emp.GetMobileNo());
                        }
                    }
                    if (!smsparameters.ContainsKey("[MOBILENO]"))
                    {
                        smsparameters.Add("[MOBILENO]", mobileno);
                    }
                    if (!smsparameters.ContainsKey("[MOBILE]"))
                    {
                        smsparameters.Add("[MOBILE]", mobileno);
                    }
                    return smsparameters;
                }
                internal string Convert2JSON(string source)
                {
                    source = source.Replace("[MAILID]", mailid);
                    source = source.Replace("[CONTENT]", content);
                    source = source.Replace("[COUPON]", coupon);
                    if (Entity != null && Entity.GetType() == typeof(Models.Employee))
                    {
                        Employee emp = Entity as Models.Employee;
                        source = source.Replace("[NAME]", emp.name);
                        source = source.Replace("[EMPLOYEENAME]", emp.name);
                        if (!string.IsNullOrEmpty(emp.GetMobileNo()))
                            source = source.Replace("[MOBILENO]", emp.GetMobileNo());
                    }
                    if (Entity != null && Entity.GetType() == typeof(Models.Customer))
                    {
                        Customer emp = Entity as Models.Customer;
                        source = source.Replace("[NAME]", emp.name);
                        source = source.Replace("[CUSTOMERNAME]", emp.name);
                        if (!string.IsNullOrEmpty(emp.GetMobileNo()))
                            source = source.Replace("[MOBILENO]", emp.GetMobileNo());
                    }
                    if (Entity != null && Entity.GetType() == typeof(Models.Customerinvoice))
                    {
                        Customerinvoice emp = Entity as Models.Customerinvoice;
                        source = source.Replace("[CUSTOMERNAME]", emp.customername);
                        source = source.Replace("[NAME]", emp.customername);
                        source = source.Replace("[AMOUNT]", emp.receivable.ToString("F0"));
                        source = source.Replace("[QTY]", emp.qty.ToString("F0"));
                        if (!string.IsNullOrEmpty(emp.GetMobileNo()))
                            source = source.Replace("[MOBILENO]", emp.GetMobileNo());
                    }
                    source = source.Replace("[MOBILENO]", mobileno);
                    return source;
                }
            }
            public abstract ReportSource Run(long organizationid, int tzoffset);
            public Models.Schedule Source { get; set; }

            public string RenderReport(long organizationid)
            {
                var reportsource = Run(organizationid, Source.config.tzoffset);
                ReportLibrary.DefaultPageSettings setting = new ReportLibrary.DefaultPageSettings();
                ReportLibrary.DotMatrixReporter labreport = new ReportLibrary.DotMatrixReporter(organizationid, new ReportLibrary.DotMatrixReporter.PrinterConfig());
                labreport.format = ReportLibrary.DosFormatParser.ParseXML(organizationid, Source.config.format);
                return labreport.format.Render(new ImageData(), reportsource.RootObject, reportsource.itemList, new SortedDictionary<string, object>());
            }

            public List<SMSDetails> RenderMultiple(long organizationid)
            {
                var reportsource = Run(organizationid, Source.config.tzoffset);
                var result = reportsource.SMSList;
                if (result != null)
                {
                    ReportLibrary.DefaultPageSettings setting = new ReportLibrary.DefaultPageSettings();
                    ReportLibrary.DotMatrixReporter labreport = new ReportLibrary.DotMatrixReporter(organizationid, new ReportLibrary.DotMatrixReporter.PrinterConfig());
                    labreport.format = ReportLibrary.DosFormatParser.ParseXML(organizationid, Source.config.format);
                    result.ForEach(e => e.content = labreport.format.Render(new ImageData(), e.Entity, null, new SortedDictionary<string, object>()));
                    if (result.Exists(e => !string.IsNullOrEmpty(e.coupon)))
                    {
                        result.ForEach(e => e.content = e.content.Replace("{COUPON}", e.coupon).Replace("[COUPON]",e.coupon));
                    }
                }
                return result;
            }

            public string RenderHTML(long organizationid)
            {
                var reportsource = Run(organizationid, Source.config.tzoffset);
                var additional = new SortedDictionary<string, object>();
                var imageData = new Common.ImageData();
                ReportLibrary.DefaultPageSettings setting = new ReportLibrary.DefaultPageSettings();
                ReportLibrary.DotMatrixReporter labreport = new ReportLibrary.DotMatrixReporter(imageData.Context.OrganizationID, new ReportLibrary.DotMatrixReporter.PrinterConfig());
                var format = ReportLibrary.DosFormatParser.ParseXML2HTML(organizationid, Source.config.format);
                var document = format.RenderBytes(new ImageData(), reportsource.RootObject, reportsource.itemList, additional, true);
                return System.Text.ASCIIEncoding.ASCII.GetString(document);
            }

            public Common.ImageData.File RenderPDFFile(long organizationid)
            {
                var reportsource = Run(organizationid, Source.config.tzoffset);
                ReportLibrary.DotMatrixReporter labreport = new ReportLibrary.DotMatrixReporter(organizationid);
                labreport.format = ReportLibrary.DosFormatParser.ParseXML(organizationid, Source.config.format);
                labreport.DefaultPageSettings = labreport.format.GetDefaultSetting();
                labreport.Additional = new SortedDictionary<string, object>();

                ReportLibrary.PDFPrinter printer = new ReportLibrary.PDFPrinter();
                printer.Report = labreport;
                if (!string.IsNullOrEmpty(Source.config.format))
                    printer.EnablePageNumber = Source.config.format.Contains("PageCount");
                labreport.DataSource = reportsource.RootObject;
                labreport.DataItems = reportsource.itemList;

                List<string> imageids = labreport.format.GetImages();
                var imageData = new Common.ImageData() { Context = new UserClaim() { OrganizationID = organizationid } };
                foreach (var imgid in imageids.FindAll(x => x.Length > 6))
                {
                    var file = imageData.GetData(imgid.LongFromBase62());
                    if (file != null && file.data != null)
                    {
                        var img = imageData.FromBytes(file.data);
                        labreport.Additional.Add("IMG-" + imgid, img);
                    }
                }

                string licensekey = LicenseManager.License.Key.Replace("-", "");
                Common.ImageData.File pdffile = new ImageData.File();
                using (System.IO.MemoryStream stream = printer.Print(labreport.format.GetDefaultSetting()))
                {
                    pdffile.ext = ".pdf";
                    pdffile.type = licensekey;
                    pdffile.filename = Source.name.Replace(" ", "") + ".pdf";
                    pdffile.data = stream.ToArray();
                }
                return pdffile;
            }

            public Common.ImageData.File RenderExcelFile(long organizationid)
            {
                var reportsource = Run(organizationid, Source.config.tzoffset);
                string licensekey = LicenseManager.License.Key.Replace("-", "");
                Common.ImageData.File pdffile = new ImageData.File();
                pdffile.data = reportsource.file;
                return pdffile;
            }

            public string RenderPDFReport(long organizationid)
            {
                var imageData = new Common.ImageData() { Context = new UserClaim() { OrganizationID = organizationid } };
                var pdffile = RenderPDFFile(organizationid);
                pdffile = imageData.Insert(pdffile);
                string licensekey = LicenseManager.License.Key.Replace("-", "");
                string serverurl = "";
                if(!string.IsNullOrEmpty(InfyPOS.Services.ConfigManager.Instance(organizationid).Config.sms_hostedserver))
                    serverurl = InfyPOS.Services.ConfigManager.Instance(organizationid).Config.sms_hostedserver
                    + (InfyPOS.Services.ConfigManager.Instance(organizationid).Config.sms_hostedserver.EndsWith("/") ? "File/PDF/" : "/File/PDF/");

                InfyPOS.Controllers.ImageController.HashDetail hashDetail = new ImageController.HashDetail(organizationid, ImageController.HashDetail.GetLongHash(licensekey), pdffile.id);
                return serverurl + licensekey + "/" + System.Web.HttpUtility.UrlEncode(hashDetail.Hash);
            }

            private static bool? multipletimezone;
            private static int? defaultutcoffset;
            public bool HasMultipleTimeZone
            {
                get
                {
                    if (multipletimezone.HasValue) return multipletimezone.Value;
                    if (string.IsNullOrEmpty(System.Configuration.ConfigurationManager.AppSettings["MultipleTimeZone"]))
                        multipletimezone = false;
                    else
                        multipletimezone = System.Configuration.ConfigurationManager.AppSettings["MultipleTimeZone"].ToLower() == "true";

                    return multipletimezone.Value;
                }
            }
            public int DefaultUTCOffset
            {
                get
                {
                    if (defaultutcoffset.HasValue) return defaultutcoffset.Value;
                    if (string.IsNullOrEmpty(System.Configuration.ConfigurationManager.AppSettings["DefaultUTCOffset"]))
                        defaultutcoffset = -1;
                    else
                        defaultutcoffset = int.Parse(System.Configuration.ConfigurationManager.AppSettings["DefaultUTCOffset"].ToLower());

                    return defaultutcoffset.Value;
                }
            }
            public int UTCOffset
            {
                get
                {
                    if (Source.config.tzoffset != -1)
                    {
                        return Source.config.tzoffset;
                    }
                    else
                    {
                        return DefaultUTCOffset;
                    }
                }
            }
            public DateTime DateTimeNow
            {
                get
                {
                    if (!HasMultipleTimeZone)
                        return DateTime.Now;

                    var utcnow = DateTime.UtcNow.AddMinutes(UTCOffset);
                    DateTime.SpecifyKind(utcnow, DateTimeKind.Unspecified);
                    return utcnow;
                }
            }
        }

        internal List<IReportExecutor.SMSDetails> ValidateSMS(long organizationid,
            List<IReportExecutor.SMSDetails> reportResult,
            IReportExecutor reportEngine)
        {
            if (reportResult == null) return null;
            string nos = string.Join(",", reportResult.ConvertAll(e => "'" + WebAPI.Data.Utility.SafeSQL(e.mobileno) + "'"));
            string query = string.Format(@"select mobileno from smshistory where scheduleid=@scheduleid
            and config=@config and actionon=@actionon and mobileno in ({0})", nos);
            var param = new SortedList<string, object>();
            param.Add("scheduleid", reportEngine.Source.id);
            param.Add("config", reportEngine.Source.config.parameter);
            param.Add("actionon", reportEngine.DateTimeNow);
            List<string> phonenos = new List<string>();
            var basedata = new BaseData() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            using (var reader = basedata.ExecuteReader(query, param))
            {
                while (reader.Read())
                {
                    phonenos.Add(reader.GetString(0));
                }
            }
            if (phonenos.Count > 0)
            {
                reportResult.RemoveAll(e => phonenos.Contains(e.mobileno));
            }
            return reportResult;
        }

        public async Task<Smshistory> Send2WebAPIAsync(Models.Schedule schedule, string json)
        {
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
            var response = new InfyPOS.Models.Smshistory()
            {
                actionon = DateTime.Now,
                config = schedule.config.parameter,
                mobileno = schedule.config.url,
                message = json,
                scheduleid = schedule.id
            };

            try
            {
                ServicePointManager.ServerCertificateValidationCallback = delegate (object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
                {
                    if (sslPolicyErrors == SslPolicyErrors.None)
                        return true;
                    if (((System.Net.HttpWebRequest)sender).Address.AbsoluteUri.Contains(@"tran.mstsms.com"))
                        return true;
                    return false;
                };
                using (var client = new HttpClient())
                {
                    if (!string.IsNullOrEmpty(schedule.config.authentication))
                    {
                        if (schedule.config.authentication.StartsWith(@"Bearer "))
                        {
                            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", schedule.config.authentication.Substring("Bearer ".Length));
                        }
                        else
                        {
                            var byteArray = Encoding.ASCII.GetBytes(schedule.config.authentication);
                            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
                        }
                    }
                    if (string.IsNullOrEmpty(schedule.config.sendmode) ||
                        schedule.config.sendmode == "BODY")
                    {

                        var jobject = Newtonsoft.Json.Linq.JObject.Parse(json);
                        if (jobject.Count == 2 &&
                            jobject["body"] != null &&
                            jobject["header"] != null)
                        {
                            foreach (JObject header in jobject["header"])
                            {
                                foreach (JProperty x in (JToken)header)
                                {
                                    client.DefaultRequestHeaders.Add(x.Name, x.Value.ToString());
                                }
                            }
                            var content = new StringContent(jobject["body"].ToString(), Encoding.UTF8, "application/json");
                            var result = Task.Run(() => client.PostAsync(schedule.config.url, content)).Result;
                            response.response = result.Content.ReadAsStringAsync().Result;
                        }
                        else
                        {
                            var content = new StringContent(json, Encoding.UTF8, "application/json");
                            var result = Task.Run(() => client.PostAsync(schedule.config.url, content)).Result;
                            response.response = result.Content.ReadAsStringAsync().Result;
                        }
                    }

                    if (schedule.config.sendmode == "FORMDATA")
                    {
                        List<KeyValuePair<string, string>> list = new List<KeyValuePair<string, string>>();
                        var jsonobject = JObject.Parse(json);
                        foreach (var item in jsonobject.Children())
                        {
                            var key = item.First().Path;
                            var value = ((Newtonsoft.Json.Linq.JValue)item.First()).Value.ToString();
                            list.Add(new KeyValuePair<string, string>(key, value));
                        }
                        var formContent = new FormUrlEncodedContent(list.ToArray());

                        var result = await Task.Run(() => client.PostAsync(schedule.config.url, formContent));
                        if (result.IsSuccessStatusCode)
                        {
                            response.response = "SUCCESS - " + result.Content.ReadAsStringAsync().Result;
                        }
                        else
                        {
                            response.response = "FAILED - " + result.Content.ReadAsStringAsync().Result;
                        }
                    }
                }
            }
            catch (Exception exp)
            {
                response.response = "FAILED - " + exp.Message;
            }
            return response;
        }

        public class Report
        {
            public string name { get; set; }
            public string display { get; set; }
            public List<Parameter> parameters { get; set; }
            public List<ReportColumn> columns { get; set; }
            public Output output { get; set; }
            public string reportformat { get; set; }
        }

        public class ReportColumn
        {
            public string name { get; set; }
            public string display { get; set; }
            public string format { get; set; }
        }
        public class Parameter
        {
            public string name { get; set; }
            public string display { get; set; }
            public Control control { get; set; }
        }
        public class Action
        {
            public string name { get; set; }
            public string display { get; set; }
            public int value { get; set; }
        }

        public enum Frequency
        {
            Daily,
            Weekly,
            Monthly,
            Yearly,
            Hourly,
            OnCreated,
            OnDeleted,
            OnChanged,
            OnApproval,
            OnPaid
        }
        public enum Output
        {
            SMS,
            EMail,
            PDF,
            PDFLink,
            Both
        }

        public class Control
        {
            public int index { get; set; }
            public string name { get; set; }
            public Controltype controltype { get; set; }
            public DefaultValue defaultvalue { get; set; }
            public string value { get; set; }
            public int width { get; set; }
            public List<ListItem> datasource { get; set; }

        }

        public enum DefaultValue
        {
            Empty,
            Zero,
            Today,
            ThisMonth,
            ThisYear
        }

        public class ListItem
        {
            public int id { get; set; }
            public string name { get; set; }
            public string display { get; set; }
        }
        public enum Controltype
        {
            Textbox,
            Number,
            Combo,
            Date,
            DateRange
        }
        public class ScheduleableTask
        {
            public string name { get; set; }
            public string displayname { get; set; }
            public List<Action> supportingactions { get; set; }
            [Newtonsoft.Json.JsonIgnore]
            public string reportclass { get; set; }
            public Output output { get; set; }
            public List<string> Parameters { get; set; }
            public bool isschedulable { get; set; }
            public string PropertyList { get; set; }
            public string defaultjson { get; set; }
            public bool multipleresult { get; set; }

            public ScheduleManager.IReportExecutor GetReportEngine()
            {
                System.Runtime.Remoting.ObjectHandle handle = Activator.CreateInstance(this.GetType().Assembly.FullName, reportclass);
                ScheduleManager.IReportExecutor report = (ScheduleManager.IReportExecutor)handle.Unwrap();
                return report;
            }
        }

        public static List<ScheduleableTask> SaleReport
        {
            get
            {
                var retvalue = new List<ScheduleableTask>();
                ScheduleableTask task = new ScheduleableTask()
                {
                    name = "SaleReport",
                    displayname = "Sales Report",
                    output = Output.Both,
                    isschedulable = true,
                    reportclass = "InfyPOS.Processors.Salesummary",
                    PropertyList = string.Join(",", typeof(InfyPOS.Processors.Salesummary.Root).GetProperties().ToList().ConvertAll(e => e.Name)),
                    Parameters = new List<string>()
                };
                task.Parameters.Add("Current Month");
                task.Parameters.Add("Current Year");
                task.Parameters.Add("Current Week");
                task.Parameters.Add("Current Day");
                retvalue.Add(task);

                task = new ScheduleableTask()
                {
                    name = "DaySummary",
                    displayname = "Day Summary",
                    output = Output.Both,
                    isschedulable = true,
                    reportclass = "InfyPOS.Processors.Daysummary",
                    PropertyList = string.Join(",", typeof(Data.DaySummary.Item).GetProperties().ToList().ConvertAll(e => e.Name)),
                    Parameters = new List<string>()
                };
                task.Parameters.Add("Current Month");
                task.Parameters.Add("Current Year");
                task.Parameters.Add("Current Week");
                task.Parameters.Add("Current Day");
                retvalue.Add(task);

                task = new ScheduleableTask()
                {
                    name = "DayClose",
                    displayname = "Day Close Summary",
                    output = Output.Both,
                    isschedulable = true,
                    reportclass = "InfyPOS.Processors.Dayclose",
                    PropertyList = string.Join(",", typeof(Data.DaySummary.Item).GetProperties().ToList().ConvertAll(e => e.Name)),
                    Parameters = new List<string>()
                };
                task.Parameters.Add("Fixed Location");
                task.Parameters.Add("All Location");
                retvalue.Add(task);

                task = new ScheduleableTask()
                {
                    name = "OnDateChange",
                    displayname = "On Date Change",
                    output = Output.Both,
                    PropertyList = string.Join(",", typeof(Data.DaySummary.Item).GetProperties().ToList().ConvertAll(e => e.Name)),
                    Parameters = new List<string>()
                };
                task.Parameters.Add("Fixed Location");
                task.Parameters.Add("All Location");
                retvalue.Add(task);

                task = new ScheduleableTask()
                {
                    name = "Reordersummary",
                    displayname = "Reorder Summary",
                    output = Output.Both,
                    isschedulable = true,
                    reportclass = "InfyPOS.Processors.ReorderSummary",
                    PropertyList = string.Join(",", typeof(Models.ReorderrulesResult).GetProperties().ToList().ConvertAll(e => e.Name))
                };
                task.Parameters = new List<string>();
                task.Parameters.Add("Reorder Config");
                task.Parameters.Add("Stock Holding Period");
                task.Parameters.Add("Item Config");
                retvalue.Add(task);

                task = new ScheduleableTask()
                {
                    name = "Inward - Pending",
                    displayname = "Inward - Pending",
                    output = Output.Both,
                    isschedulable = true,
                    reportclass = "InfyPOS.Processors.InwardDetails",
                    PropertyList = string.Join(",", typeof(Data.OutwardReport).GetProperties().ToList().ConvertAll(e => e.Name)),
                    Parameters = new List<string>()
                };
                task.Parameters.Add("Current Month");
                task.Parameters.Add("Current Week");
                task.Parameters.Add("Current Day");
                retvalue.Add(task);

                task = new ScheduleableTask()
                {
                    name = "SalesStockSummary",
                    displayname = "Sales/Stock Summary",
                    output = Output.Both,
                    isschedulable = true,
                    reportclass = "InfyPOS.Processors.SalesStockSummary",
                    PropertyList = string.Join(",", typeof(Data.SalesStockReport).GetProperties().ToList().ConvertAll(e => e.Name)) + " - Items ( " +
                    string.Join(",", typeof(Data.SalesStockReport.Item).GetProperties().ToList().ConvertAll(e => e.Name)) + " )",
                    Parameters = new List<string>()
                };
                task.Parameters.Add("Last Month");
                task.Parameters.Add("Last Week");
                task.Parameters.Add("Yesterday");
                task.Parameters.Add("Current Month");
                task.Parameters.Add("Current Week");
                task.Parameters.Add("Current Day");
                retvalue.Add(task);
                return retvalue;
            }
        }

        public static List<ScheduleableTask> SupplierTask
        {
            get
            {
                var retvalue = new List<ScheduleableTask>();
                ScheduleableTask task = new ScheduleableTask()
                {
                    name = "SupplierPayment",
                    displayname = "On Supplier Payments",
                    output = Output.Both,
                    PropertyList = string.Join(",", typeof(Models.SupplierPaymentPrint).GetProperties().ToList().ConvertAll(e => e.Name))
                };
                retvalue.Add(task);
                task = new ScheduleableTask()
                {
                    name = "SupplierDebitNote",
                    displayname = "On Purchase Return",
                    output = Output.Both,
                    PropertyList = string.Join(",", typeof(Models.Lrinvoicereturn).GetProperties().ToList().ConvertAll(e => e.Name))
                };
                retvalue.Add(task);
                task = new ScheduleableTask()
                {
                    name = "SupplierOrder",
                    displayname = "On Purchase Order",
                    output = Output.Both,
                    PropertyList = string.Join(",", typeof(Models.Purchaseorder).GetProperties().ToList().ConvertAll(e => e.Name))
                };
                retvalue.Add(task);
                task = new ScheduleableTask()
                {
                    name = "SupplierOrderPublish",
                    displayname = "On Purchase Order Publish",
                    output = Output.Both,
                    PropertyList = string.Join(",", typeof(Models.PurchaseorderPrint).GetProperties().ToList().ConvertAll(e => e.Name))
                };
                retvalue.Add(task);
                task = new ScheduleableTask()
                {
                    name = "SupplierSales",
                    displayname = "On Supplier Sales",
                    output = Output.Both,
                    isschedulable = true,
                    reportclass = "InfyPOS.Processors.SupplierTransaction",
                    PropertyList = string.Join(",", typeof(InfyPOS.QuantoAPI.SalesData).GetProperties().ToList().ConvertAll(e => e.Name))
                };
                task.Parameters = new List<string>();
                task.Parameters.Add("Last Month");
                task.Parameters.Add("Last Week");
                task.Parameters.Add("Yesterday");
                task.Parameters.Add("Current Day");
                retvalue.Add(task);
                task = new ScheduleableTask()
                {
                    name = "SupplierPurchase",
                    displayname = "On Supplier Purchase",
                    output = Output.Both,
                    isschedulable = true,
                    reportclass = "InfyPOS.Processors.SupplierTransaction",
                    PropertyList = string.Join(",", typeof(InfyPOS.QuantoAPI.StockData).GetProperties().ToList().ConvertAll(e => e.Name))
                };
                task.Parameters = new List<string>();
                task.Parameters.Add("Last Month");
                task.Parameters.Add("Last Week");
                task.Parameters.Add("Yesterday");
                task.Parameters.Add("Current Day");
                retvalue.Add(task);
                task = new ScheduleableTask()
                {
                    name = "SupplierStock",
                    displayname = "On Supplier Stock",
                    output = Output.Both,
                    isschedulable = true,
                    reportclass = "InfyPOS.Processors.SupplierTransaction",
                    PropertyList = string.Join(",", typeof(InfyPOS.QuantoAPI.StockData).GetProperties().ToList().ConvertAll(e => e.Name))
                };
                retvalue.Add(task);
                return retvalue;
            }
        }

        //public static List<ScheduleableTask> TemplateTask
        //{
        //    get
        //    {
        //        var retvalue = new List<ScheduleableTask>();
        //        ScheduleableTask task = new ScheduleableTask()
        //        {
        //            name = "customerotpfmt",
        //            displayname = "Customer OTP",
        //            PropertyList = string.Join(",", typeof(Models.Customer).GetProperties().ToList().ConvertAll(e => e.Name)),
        //            output = Output.Both
        //        };
        //        retvalue.Add(task);

        //        task = new ScheduleableTask()
        //        {
        //            name = "approvalotpfmt",
        //            displayname = "Approval OTP",
        //            output = Output.Both,
        //            PropertyList = string.Join(",", typeof(Models.Customer).GetProperties().ToList().ConvertAll(e => e.Name))
        //        };
        //        retvalue.Add(task);

        //        task = new ScheduleableTask()
        //        {
        //            name = "approvaltemplateurl",
        //            displayname = "Approval",
        //            output = Output.Both,
        //            PropertyList = string.Join(",", typeof(Models.Employee).GetProperties().ToList().ConvertAll(e => e.Name))
        //        };
        //        retvalue.Add(task);
        //        return retvalue;
        //    }
        //}
        public static List<ScheduleableTask> EmployeeTask
        {
            get
            {
                var retvalue = new List<ScheduleableTask>();
                ScheduleableTask task = new ScheduleableTask()
                {
                    name = "EmployeeCreation",
                    displayname = "On New Employee",
                    PropertyList = string.Join(",", typeof(Models.Employee).GetProperties().ToList().ConvertAll(e => e.Name)),
                    output = Output.Both
                };
                retvalue.Add(task);

                task = new ScheduleableTask()
                {
                    name = "EmployeeDOB",
                    displayname = "On Employee Birthday",
                    isschedulable = true,
                    output = Output.Both,
                    reportclass = "InfyPOS.Processors.OnDayOccure",
                    PropertyList = string.Join(",", typeof(Models.Employee).GetProperties().ToList().ConvertAll(e => e.Name)),
                    Parameters = new List<string>()
                };
                task.Parameters.Add("Employee DOB");
                retvalue.Add(task);

                task = new ScheduleableTask()
                {
                    name = "EmployeeAnniversary",
                    displayname = "On Employee Anniversary",
                    isschedulable = true,
                    output = Output.Both,
                    reportclass = "InfyPOS.Processors.OnDayOccure",
                    PropertyList = string.Join(",", typeof(Models.Employee).GetProperties().ToList().ConvertAll(e => e.Name)),
                    Parameters = new List<string>()
                };
                task.Parameters.Add("Employee Anniversary");
                retvalue.Add(task);

                //task = new ScheduleableTask()
                //{
                //    name = "EmployeeFestivalHoliday",
                //    displayname = "Employee Festival Holiday",
                //    isschedulable = true,
                //    output = Output.Both,
                //    reportclass = "InfyPOS.Processors.OnDayOccure",
                //    PropertyList = string.Join(",", typeof(Models.Employee).GetProperties().ToList().ConvertAll(e => e.Name)),
                //    Parameters = new List<string>()
                //};
                //task.Parameters.Add("Employee Festival Holiday");
                //retvalue.Add(task);
                return retvalue;
            }
        }

        public static List<ScheduleableTask> CustomerTask
        {
            get
            {
                var retvalue = new List<ScheduleableTask>();
                ScheduleableTask task = new ScheduleableTask()
                {
                    name = "CustomerCreation",
                    displayname = "On New Customer",
                    output = Output.Both,
                    PropertyList = string.Join(",", typeof(Models.Customer).GetProperties().ToList().ConvertAll(e => e.Name)),
                };
                retvalue.Add(task);

                task = new ScheduleableTask()
                {
                    name = "CustomerDOB",
                    displayname = "On Customer Birthday",
                    isschedulable = true,
                    output = Output.Both,
                    reportclass = "InfyPOS.Processors.OnDayOccure",
                    PropertyList = string.Join(",", typeof(Models.Customer).GetProperties().ToList().ConvertAll(e => e.Name)),
                    Parameters = new List<string>()
                };
                task.Parameters.Add("Customer DOB");
                retvalue.Add(task);

                task = new ScheduleableTask()
                {
                    name = "Customer Anniversary",
                    displayname = "On Customer Anniversary",
                    isschedulable = true,
                    output = Output.Both,
                    reportclass = "InfyPOS.Processors.OnDayOccure",
                    PropertyList = string.Join(",", typeof(Models.Customer).GetProperties().ToList().ConvertAll(e => e.Name)),
                    Parameters = new List<string>()
                };
                task.Parameters.Add("Customer Anniversary");
                retvalue.Add(task);

                task = new ScheduleableTask()
                {
                    name = "CustomerKidsDOB",
                    displayname = "On Customer Kids Birthday",
                    isschedulable = true,
                    output = Output.Both,
                    reportclass = "InfyPOS.Processors.OnDayOccure",
                    PropertyList = string.Join(",", typeof(Models.Customer).GetProperties().ToList().ConvertAll(e => e.Name)),
                    Parameters = new List<string>()
                };
                task.Parameters.Add("Customer Kids DOB");
                retvalue.Add(task);

                //task = new ScheduleableTask()
                //{
                //    name = "Customer Festival Holiday",
                //    displayname = "On Customer Festival Holiday",
                //    isschedulable = true,
                //    output = Output.Both,
                //    reportclass = "InfyPOS.Processors.OnDayOccure",
                //    PropertyList = string.Join(",", typeof(Models.Customer).GetProperties().ToList().ConvertAll(e => e.Name)),
                //    Parameters = new List<string>()
                //};
                //task.Parameters.Add("Customer Festival Holiday");
                //retvalue.Add(task);

                task = new ScheduleableTask()
                {
                    name = "CustomerPayment",
                    displayname = "On Customer Payments",
                    output = Output.Both,
                    PropertyList = string.Join(",", typeof(Models.Customerpayment).GetProperties().ToList().ConvertAll(e => e.Name))
                };
                retvalue.Add(task);

                task = new ScheduleableTask()
                {
                    name = "CustomerPaymentReminder",
                    displayname = "Customer Payment Reminder",
                    isschedulable = true,
                    output = Output.Both,
                    reportclass = "InfyPOS.Processors.OnDayOccure",
                    PropertyList = string.Join(",", typeof(Models.Customerinvoice).GetProperties().ToList().ConvertAll(e => e.Name)),
                    Parameters = new List<string>()
                };
                task.Parameters.Add("Customer Payment Reminder");
                retvalue.Add(task);

                task = new ScheduleableTask()
                {
                    name = "ApprovedBillReminder",
                    displayname = "Approved Bill Reminder",
                    isschedulable = true,
                    output = Output.Both,
                    reportclass = "InfyPOS.Processors.OnDayOccure",
                    PropertyList = string.Join(",", typeof(Models.Customerinvoice).GetProperties().ToList().ConvertAll(e => e.Name)),
                    Parameters = new List<string>()
                };
                task.Parameters.Add("Approved Bill Reminder");
                retvalue.Add(task);

                task = new ScheduleableTask()
                {
                    name = "SupplierPaymentReminder",
                    displayname = "Supplier Payment Reminder",
                    isschedulable = true,
                    output = Output.Both,
                    reportclass = "InfyPOS.Processors.OnDayOccure",
                    PropertyList = string.Join(",", typeof(Models.PendingLrInvoice).GetProperties().ToList().ConvertAll(e => e.Name)),
                    Parameters = new List<string>()
                };
                task.Parameters.Add("Supplier Payment Reminder");


                retvalue.Add(task);

                task = new ScheduleableTask()
                {
                    name = "CustomerSaveCRM",
                    displayname = "Customer Save - CRM",
                    isschedulable = false,
                    output = Output.Both,
                    defaultjson = JsonConvert.SerializeObject(new Models.CustomerIntegration()),
                    PropertyList = string.Join(",", typeof(Models.CustomerIntegration).GetProperties().ToList().ConvertAll(e => e.Name)),
                    Parameters = new List<string>()
                };
                retvalue.Add(task);

                task = new ScheduleableTask()
                {
                    name = "CustomerLoyaltyRequest",
                    displayname = "Customer Loyalty Request - CRM",
                    isschedulable = false,
                    output = Output.Both,
                    defaultjson = JsonConvert.SerializeObject(new Models.CustomerIntegration()),
                    PropertyList = string.Join(",", typeof(Models.CustomerIntegration).GetProperties().ToList().ConvertAll(e => e.Name)),
                    Parameters = new List<string>()
                };
                retvalue.Add(task);

                task = new ScheduleableTask()
                {
                    name = "CustomerVoucherRequest",
                    displayname = "Customer Voucher Request - CRM",
                    isschedulable = false,
                    output = Output.Both,
                    defaultjson = JsonConvert.SerializeObject(new Models.CustomerIntegration()),
                    PropertyList = string.Join(",", typeof(Models.CustomerIntegration).GetProperties().ToList().ConvertAll(e => e.Name)),
                    Parameters = new List<string>()
                };
                retvalue.Add(task);

                task = new ScheduleableTask()
                {
                    name = "CustomerCampaign",
                    displayname = "On Customer Campaign",
                    isschedulable = false,
                    output = Output.Both,
                    reportclass = "InfyPOS.Processors.OnDayOccure",
                    PropertyList = string.Join(",", typeof(Models.Customer).GetProperties().ToList().ConvertAll(e => e.Name)),
                    Parameters = new List<string>()
                };
                retvalue.Add(task);

                task = new ScheduleableTask()
                {
                    name = "AlterationReceived",
                    displayname = "On Alteration Received",
                    isschedulable = false,
                    output = Output.Both,
                    PropertyList = string.Join(",", typeof(Models.Delivery).GetProperties().ToList().ConvertAll(e => e.Name)),
                    Parameters = new List<string>()
                };
                retvalue.Add(task);

                task = new ScheduleableTask()
                {
                    name = "AlterationReady",
                    displayname = "On Alteration Ready",
                    isschedulable = false,
                    output = Output.Both,
                    PropertyList = string.Join(",", typeof(Models.Delivery).GetProperties().ToList().ConvertAll(e => e.Name)),
                    Parameters = new List<string>()
                };
                retvalue.Add(task);

                task = new ScheduleableTask()
                {
                    name = "AlterationDelivered",
                    displayname = "On Alteration Delivered",
                    isschedulable = false,
                    output = Output.Both,
                    PropertyList = string.Join(",", typeof(Models.Delivery).GetProperties().ToList().ConvertAll(e => e.Name)),
                    Parameters = new List<string>()
                };
                retvalue.Add(task);

                return retvalue;
            }
        }

        public static List<ScheduleableTask> BillTask
        {
            get
            {
                var retvalue = new List<ScheduleableTask>();
                ScheduleableTask task = new ScheduleableTask()
                {
                    name = "BillCancel",
                    displayname = "On Bill Cancellation",
                    output = Output.Both,
                    PropertyList = string.Join(",", typeof(Models.Bill).GetProperties().ToList().ConvertAll(e => e.Name))
                };
                retvalue.Add(task);
                task = new ScheduleableTask()
                {
                    name = "BillSave",
                    displayname = "On Bill Save",
                    output = Output.Both,
                    PropertyList = string.Join(",", typeof(Models.Bill).GetProperties().ToList().ConvertAll(e => e.Name))
                };
                retvalue.Add(task);
                task = new ScheduleableTask()
                {
                    name = "BillReturn",
                    displayname = "On Bill Return",
                    output = Output.Both,
                    PropertyList = string.Join(",", typeof(Models.Bill).GetProperties().ToList().ConvertAll(e => e.Name))
                };
                retvalue.Add(task);

                task = new ScheduleableTask()
                {
                    name = "BillPublish",
                    displayname = "On Bill Publish",
                    output = Output.Both,
                    PropertyList = string.Join(",", typeof(Models.Bill).GetProperties().ToList().ConvertAll(e => e.Name))
                };
                retvalue.Add(task);

                task = new ScheduleableTask()
                {
                    name = "BillCredit",
                    displayname = "On Credit Bill",
                    output = Output.Both,
                    PropertyList = string.Join(",", typeof(Models.Bill).GetProperties().ToList().ConvertAll(e => e.Name))
                };
                retvalue.Add(task);

                task = new ScheduleableTask()
                {
                    name = "BillDiscount",
                    displayname = "On Bill Discount",
                    output = Output.Both,
                    PropertyList = string.Join(",", typeof(Models.Bill).GetProperties().ToList().ConvertAll(e => e.Name)),
                    Parameters = new List<string>()
                };
                task.Parameters.Add("Discount Greater than");
                retvalue.Add(task);

                task = new ScheduleableTask()
                {
                    name = "Bill2CRM",
                    displayname = "On Bill Save to CRM",
                    output = Output.Both,
                    defaultjson = JsonConvert.SerializeObject(new Models.BillIntegration("")),
                    PropertyList = string.Join(",", typeof(Models.BillIntegration).GetProperties().ToList().ConvertAll(e => e.Name))
                };
                retvalue.Add(task);

                task = new ScheduleableTask()
                {
                    name = "Bill2ECOM",
                    displayname = "On Bill Save to E-Commerce",
                    output = Output.Both,
                    defaultjson = JsonConvert.SerializeObject(new Models.BillIntegration("")),
                    PropertyList = string.Join(",", typeof(Models.BillIntegration).GetProperties().ToList().ConvertAll(e => e.Name))
                    + ",BillItems:[" + string.Join(",", typeof(Models.BillIntegration.BillItem).GetProperties().ToList().ConvertAll(e => e.Name)) + "]"
                };
                retvalue.Add(task);

                task = new ScheduleableTask()
                {
                    name = "BillSettlement",
                    displayname = "On Bill Settlement",
                    output = Output.Both,
                    PropertyList = string.Join(",", typeof(Models.Billsettlementmaster).GetProperties().ToList().ConvertAll(e => e.Name))
                    + ",Billsettlement:[" + string.Join(",", typeof(Models.Billsettlement).GetProperties().ToList().ConvertAll(e => e.Name)) + "]"
                };
                retvalue.Add(task);

                task = new ScheduleableTask()
                {
                    name = "LoyeltyOTP",
                    displayname = "On Loyelty OTP",
                    output = Output.Both,
                    PropertyList = string.Join(",", typeof(Models.BillOTP).GetProperties().ToList().ConvertAll(e => e.Name))
                };
                retvalue.Add(task);

                task = new ScheduleableTask()
                {
                    name = "LoyeltyOTPValidate",
                    displayname = "On Loyelty OTP Validate",
                    output = Output.Both,
                    PropertyList = string.Join(",", typeof(Models.BillOTP).GetProperties().ToList().ConvertAll(e => e.Name))
                };
                retvalue.Add(task);


                task = new ScheduleableTask()
                {
                    name = "DiscountOTP",
                    displayname = "On Discount OTP",
                    output = Output.Both,
                    PropertyList = string.Join(",", typeof(Models.BillOTP).GetProperties().ToList().ConvertAll(e => e.Name))
                };
                retvalue.Add(task);

                task = new ScheduleableTask()
                {
                    name = "CreditOTP",
                    displayname = "On Credit Approval OTP",
                    output = Output.Both,
                    PropertyList = string.Join(",", typeof(Models.BillOTP).GetProperties().ToList().ConvertAll(e => e.Name))
                };
                retvalue.Add(task);

                //task = new ScheduleableTask()
                //{
                //    name = "BillCardPayment",
                //    displayname = "On Bill Card Payment",
                //    output = Output.Both
                //};
                //retvalue.Add(task);

                return retvalue;
            }
        }

        private static ScheduleManager _instance;
        static ScheduleManager()
        {
            _instance = new ScheduleManager();
        }
        public static ScheduleManager Instance
        {
            get
            {
                return _instance;
            }
        }

        public List<ScheduleableTask> TaskList()
        {
            var retvalue = new List<ScheduleableTask>();
            //retvalue.AddRange(TemplateTask);
            retvalue.AddRange(BillTask);
            retvalue.AddRange(CustomerTask);
            retvalue.AddRange(EmployeeTask);
            retvalue.AddRange(SupplierTask);
            retvalue.AddRange(SaleReport);
            return retvalue;
        }

        public void SendEmail(Models.Schedule schedule, bool ishtml, string message, string subject, string mailid)
        {
            string apikey = "2604e490-4fc7-4ba5-bc23-2c26b514dd8d";
            NameValueCollection values = new NameValueCollection();
            values.Add("apikey", apikey);
            values.Add("from", "Support@QuantoRetail.com");
            values.Add("fromName", "Support");
            values.Add("subject", subject);
            if (ishtml)
                values.Add("bodyHtml", message);
            else
                values.Add("bodyText", message);
            values.Add("to", mailid);

            //var filepath = @"C:\Users\ELAYARAJA\Downloads\5_6100403352024121380.pdf";
            //var file = File.OpenRead(filepath);

            //var filesStream = new Stream[] { file };
            //var filenames = new string[] { "5_6100403352024121380.pdf" };
            var URL = "https://api.elasticemail.com/v2/email/send";

            try
            {
                using (var client = new HttpClient())
                using (var formData = new MultipartFormDataContent())
                {
                    foreach (string key in values)
                    {
                        HttpContent stringContent = new StringContent(values[key]);
                        formData.Add(stringContent, key);
                    }

                    var response = client.PostAsync(URL, formData).Result;
                    if (!response.IsSuccessStatusCode)
                    {
                        throw new Exception(response.Content.ReadAsStringAsync().Result);
                    }

                    var result = response.Content.ReadAsStringAsync().Result;
                }
            }
            catch (Exception exp)
            {

            }
        }
        public void SendSMTPMail(Models.Schedule schedule, string message, ImageData.File file, string mailid)
        {
            MailMessage mail = new MailMessage();
            mail.To.Add(mailid);
            mail.From = new MailAddress(schedule.config.emailfrom);
            mail.Subject = schedule.config.emailsubject;
            mail.Body = message;
            mail.IsBodyHtml = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            SmtpClient smtp = new SmtpClient();
            if (file != null && file.data != null)
            {
                var memoryStream = new MemoryStream(file.data);
                System.Net.Mail.Attachment attachment = new System.Net.Mail.Attachment(memoryStream,
                    schedule.config.emailsubject);
                attachment.Name = attachment.Name.Replace(" ", "") + "."+ (string.IsNullOrEmpty(file.ext) ? "pdf": file.ext);
                mail.Attachments.Add(attachment);
            }
            smtp.Host = schedule.config.emailhost; //host name
            smtp.Port = int.Parse(schedule.config.emailport); //port number
            smtp.EnableSsl = schedule.config.emailssl;
            smtp.UseDefaultCredentials = true;
            smtp.DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.Network;
            smtp.Timeout = 20000;
            smtp.Credentials = new System.Net.NetworkCredential(schedule.config.emailusername, schedule.config.emailpassword);

            smtp.SendCompleted += (s, e) =>
            {
                if (e.Error != null)
                    WebAPI.Common.Logger.Instance(0).Log(e.Error, WebAPI.Common.Logger.LogType.Error, mail);
                else
                    WebAPI.Common.Logger.Instance(0).Log("Scheduled Email Sent to ", WebAPI.Common.Logger.LogType.Info, mail);
            };

            smtp.SendAsync(mail, null);
        }
        //public void SendEmail(Models.Schedule schedule, string message, ImageData.File file, string mailid)
        //{
        //    string apikey = "2604e490-4fc7-4ba5-bc23-2c26b514dd8d";
        //    NameValueCollection values = new NameValueCollection();
        //    values.Add("apikey", apikey);
        //    values.Add("from", "Support@QuantoRetail.com");
        //    values.Add("fromName", "Support");
        //    values.Add("subject", schedule.description);
        //    values.Add("bodyText", message);
        //    values.Add("to", mailid);
        //    MemoryStream memoryStream = null;
        //    //var filepath = @"C:\Users\ELAYARAJA\Downloads\5_6100403352024121380.pdf";
        //    //var file = File.OpenRead(filepath);

        //    //var filesStream = new Stream[] { file };
        //    //var filenames = new string[] { "5_6100403352024121380.pdf" };
        //    var URL = "https://api.elasticemail.com/v2/email/send";
        //    try
        //    {
        //        using (var client = new HttpClient())
        //        using (var formData = new MultipartFormDataContent())
        //        {
        //            foreach (string key in values)
        //            {
        //                HttpContent stringContent = new StringContent(values[key]);
        //                formData.Add(stringContent, key);
        //            }

        //            if (file != null)
        //            {
        //                memoryStream = new MemoryStream(file.data);
        //                HttpContent fileStreamContent = new StreamContent(memoryStream);
        //                formData.Add(fileStreamContent, schedule.name + ".pdf");
        //            }
        //            //for (int i = 0; i < filesStream.Length; i++)
        //            //{
        //            //    HttpContent fileStreamContent = new StreamContent(filesStream[i]);
        //            //    formData.Add(fileStreamContent, "file" + i, filenames[i]);
        //            //}

        //            var response = client.PostAsync(URL, formData).Result;
        //            if (!response.IsSuccessStatusCode)
        //            {
        //                throw new Exception(response.Content.ReadAsStringAsync().Result);
        //            }

        //            var result = response.Content.ReadAsStringAsync().Result;
        //        }
        //    }catch(Exception exp)
        //    {

        //    }
        //    if (memoryStream != null)
        //        memoryStream.Dispose();
        //}

        public bool ExecuteCampignScheduleItem(long organizationid, Models.Customer customer, Schedule scheduleconfig)
        {
            Common.BaseData baseData = new Common.BaseData() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            string request = scheduleconfig.config.format;
            request = request.Replace("[MOBILENO]", customer.mobileno);
            request = request.Replace("[NAME]", customer.name);
            request = request.Replace("[LOYELTY]", customer.loyelty.ToString("F0"));
            var response = ScheduleManager.Instance.Send2WebAPIAsync(scheduleconfig, request);
            SmshistoryDataService smshistoryDataService = new SmshistoryDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            smshistoryDataService.Insert(response.Result);
            return true;
        }
        public InfyPOS.Models.Smshistory Create(long organizationid, Models.Schedule schedule, string message,
            string mobileno, DateTime actionon)
        {
            return new InfyPOS.Models.Smshistory()
            {
                response = message,
                actionon = actionon,
                config = schedule.config.parameter,
                mobileno = mobileno,
                message = message,
                scheduleid = schedule.id
            };
        }
        public InfyPOS.Models.Smshistory SendSMS(long organizationid, long locationid, Models.Schedule schedule,
            string message, string mobileno, SortedDictionary<string, string> urlreplacements)
        {
            var response = new InfyPOS.Models.Smshistory()
            {
                actionon = DateTime.Now,
                config = schedule.config.parameter,
                mobileno = mobileno,
                message = message,
                scheduleid = schedule.id
            };

            //if (InfyPOS.Services.SMSManager.Instance.HasSent(organizationid, response))
            //{
            //    response.HasDuplicate = true;
            //    return response;
            //}

            if (string.IsNullOrEmpty(schedule.config.url))
            {

                if (message.ToUpper().StartsWith("HTTP://") ||
                    message.ToUpper().StartsWith("HTTPS://"))
                    response.response = InfyPOS.Services.SMSManager.Instance.SendSMSURLSchedule(organizationid, locationid,
                        message,
                        mobileno, "");
                else
                    response.response = InfyPOS.Services.SMSManager.Instance.SendSMSSchedule(organizationid, locationid,
                    InfyPOS.Services.ConfigManager.Instance(organizationid).Config.sms_tran_senderid,
                        mobileno, message);
            }
            else
                response.response = InfyPOS.Services.SMSManager.Instance.SendSMSURLSchedule(organizationid, locationid,
                    GetProcessedURL(schedule.config.url, urlreplacements),
                        mobileno, message);
            return response;


            //if (string.IsNullOrEmpty(InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.hostedserver))
            //{
            //    InfyPOS.Services.SMSManager.Instance.SendSMS(InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.sms_tran_senderid,
            //        mobileno, message);
            //}
            //else
            //{
            //    Schedule_Actions.QuantoSMS quantoSMS = new Schedule_Actions.QuantoSMS();
            //    quantoSMS.message = message;
            //    quantoSMS.items = new List<Schedule_Actions.QuantoSMS.Item>();
            //    quantoSMS.items.Add(new Schedule_Actions.QuantoSMS.Item()
            //    {
            //        mobile = mobileno
            //    });
            //    quantoSMS.senderid = InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.sms_tran_senderid;
            //    quantoSMS.smsserverurl = InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.sms_url;
            //    string url = InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.sms_hostedserver;

            //    using (var client = new System.Net.Http.HttpClient())
            //    {
            //        var serializedProduct = JsonConvert.SerializeObject(quantoSMS);
            //        var content = new System.Net.Http.StringContent(serializedProduct, System.Text.Encoding.UTF8, "application/json");

            //        var result = System.Threading.Tasks.Task.Run(() => client.PostAsync(url, content)).Result;
            //        if (result.IsSuccessStatusCode)
            //        {
            //            var json = result.Content.ReadAsStringAsync().Result;
            //            var actualresult = JsonConvert.DeserializeObject<bool>(json);
            //        }
            //    }
            //}
        }

        public string GetProcessedURL(string url, SortedDictionary<string, string> paramters)
        {
            if (paramters == null)
                return url;
            foreach (var key in paramters)
            {
                if(key.Key.StartsWith("[") && key.Key.EndsWith("]"))
                    url = url.Replace(key.Key, System.Web.HttpUtility.UrlEncode(key.Value));
                else if (key.Key.StartsWith("{") && key.Key.EndsWith("}"))
                    url = url.Replace(key.Key, System.Web.HttpUtility.UrlEncode(key.Value));
                else
                    url = url.Replace("[" + key.Key + "]", System.Web.HttpUtility.UrlEncode(key.Value));
            }
            return url;
        }

        internal void StartCampaign(long organizationID, long scheduleid, List<Tuple<string, string>> list)
        {
            ScheduleDataService scheduleDataService = new ScheduleDataService() { Context = new UserClaim() { OrganizationID = organizationID } };
            var schedule = scheduleDataService.Search(new ScheduleCreteria()
            {
                id = scheduleid
            }).First();
            foreach (var item in list)
            {
                Processors.Schedule_Actions.Instance.OnChange(organizationID, new Customer()
                {
                    mobileno = item.Item1,
                    name = item.Item2
                }, schedule, Processors.Schedule_Actions.Actions.CRM_Campaign);
            }
        }

        internal void SendGMail(long organizationid, long locationid, Schedule scheduleconfig,
            string result, SortedDictionary<string, byte[]> attachments, string attachmentname, string mailid)
        {

            try
            {

                MailMessage mail = new MailMessage();
                mail.To.Add(mailid);
                mail.From = new MailAddress(scheduleconfig.config.emailfrom);
                mail.Subject = scheduleconfig.config.emailsubject;
                mail.Body = string.IsNullOrEmpty(result) ? "Empty Body" : result;
                mail.IsBodyHtml = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                SmtpClient smtp = new SmtpClient();
                if (attachments != null && attachments.Count>0)
                {
                    foreach(var pdf in attachments)
                    {
                        var memoryStream = new MemoryStream(pdf.Value);
                        var filename = pdf.Key.Replace("/", "").Replace("\\", "");
                        if (!filename.ToUpper().EndsWith(".PDF"))
                            filename += ".pdf";
                        System.Net.Mail.Attachment attachment = new System.Net.Mail.Attachment(memoryStream, filename, MediaTypeNames.Application.Pdf);
                        mail.Attachments.Add(attachment);
                    }
                }
                smtp.Host = scheduleconfig.config.emailhost; //host name
                smtp.Port = int.Parse(scheduleconfig.config.emailport); //port number
                smtp.EnableSsl = scheduleconfig.config.emailssl; //whether your smtp server requires SSL
                smtp.UseDefaultCredentials = true;
                smtp.DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.Network;
                smtp.Timeout = 20000;
                smtp.Credentials = new System.Net.NetworkCredential(scheduleconfig.config.emailusername,
                    scheduleconfig.config.emailpassword);
                smtp.Send(mail);
            }
            catch (SmtpException exp)
            {
                WebAPI.Common.Logger.Instance(organizationid).Log("Failed " + scheduleconfig.name + " because of " + exp.Message, WebAPI.Common.Logger.LogType.Error, result);
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(organizationid).Log("Failed " + scheduleconfig.name + " because of " + exp.Message, WebAPI.Common.Logger.LogType.Error, result);
            }
        }
    }
}