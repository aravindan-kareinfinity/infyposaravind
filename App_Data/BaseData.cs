using Microsoft.Owin.Security;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Web;

namespace InfyPOS.Common
{
    public class BaseData
    {
        public HttpContext httpcontext;
        public UserClaim Context { get; set; }
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
                if (Context != null && Context.TZ != -1)
                {
                    return Context.TZ;
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
        public BaseData()
        {
            try
            {
                InitializeContext();
            }
            catch
            {
                // Absolute safety fallback
                Context = UserClaim.Default();
            }
        }

        private void InitializeContext()
        {
            HttpContext httpContext = HttpContext.Current;

            // Case 1: No HttpContext (Startup / Timer / Background)
            if (httpContext == null)
            {
                Context = UserClaim.Default();
                return;
            }

            // Case 2: HttpContext exists but NO OWIN (Startup)
            if (!httpContext.Items.Contains("owin.Environment"))
            {
                Context = UserClaim.Default();
                return;
            }

            // Case 3: Valid OWIN request
            try
            {
                var owinContext = httpContext.Request.GetOwinContext();
                IAuthenticationManager auth = owinContext.Authentication;

                Context = new UserClaim(auth);
            }
            catch
            {
                // Any unexpected failure → safe fallback
                Context = UserClaim.Default();
            }
        }
        public BaseData(UserClaim context)
        {
            this.Context = context;
        }
        public LicenseManager.License PrivateLicense()
        {
            if (IsSAAS)
            {
                var saaslicense = new LicenseManager(Context.OrganizationID);
                return saaslicense.GetLicense();
            }
            var license = new LicenseManager(0);
            return license.GetLicense();
        }
        public string GetSQL(Npgsql.NpgsqlCommand command)
        {
            string query = command.CommandText;
            foreach (NpgsqlParameter p in command.Parameters.OrderByDescending(e => e.ParameterName.Length))
            {
                if (p.Value is string || p.NpgsqlDbType == NpgsqlTypes.NpgsqlDbType.Varchar)
                    query = query.Replace("@" + p.ParameterName, "'" + p.Value.ToString() + "'");
                else if (p.Value is DateTime || p.NpgsqlDbType == NpgsqlTypes.NpgsqlDbType.TimestampTZ || p.NpgsqlDbType == NpgsqlTypes.NpgsqlDbType.Timestamp || p.NpgsqlDbType == NpgsqlTypes.NpgsqlDbType.Date)
                {
                    if (p.Value != null && p.Value != DBNull.Value)
                    {
                        if (p.Value is DateTime)
                            query = query.Replace("@" + p.ParameterName, "'" + ((DateTime)p.Value).ToString("yyyy-MM-dd") + "'");
                        else if (p.Value is DateTimeOffset)
                            query = query.Replace("@" + p.ParameterName, "'" + ((DateTimeOffset)p.Value).ToString("yyyy-MM-dd") + "'");
                    }
                }
                else
                    query = query.Replace("@" + p.ParameterName, p.Value == null ? "null" : p.Value.ToString());
            }
            return query;
        }
        public virtual NpgsqlConnection CreateConnection()
        {
            string connstring = "";
            if (this.Context.OrganizationID > 0 && IsSAAS)
            {
                connstring = InfyPOS.Services.OrganizationManager.Instance.Get(this.Context != null ? this.Context.OrganizationID : 0).connectionstring;
            }
            else
            {
                connstring = System.Configuration.ConfigurationManager.ConnectionStrings["InfyPOS"].ConnectionString;
            }
            return new NpgsqlConnection(connstring);
        }

        //public virtual NpgsqlConnection CreateSAASConnection()
        //{
        //    string connstring = System.Configuration.ConfigurationManager.ConnectionStrings["SAASPOS"].ConnectionString;
        //    return new NpgsqlConnection(connstring);
        //}

        public bool CanRunONSAAS()
        {
            if (!IsSAAS || this.Context.OrganizationID > 0) return true;
            return (Context.OrganizationCode == "QUANTO");
        }
        bool? issaas;
        public bool IsSAAS
        {
            get
            {
                if (issaas.HasValue) return issaas.Value;
                issaas = !string.IsNullOrEmpty(System.Configuration.ConfigurationManager.AppSettings["SAASMODE"]) &&
                    (System.Configuration.ConfigurationManager.AppSettings["SAASMODE"].ToLower() == "true" ||
                    System.Configuration.ConfigurationManager.AppSettings["SAASMODE"].ToLower() == "yes");

                return issaas.Value;
            }
        }
        int? saasusercount;
        public int SAASUserCount
        {
            get
            {
                if (saasusercount.HasValue) return saasusercount.Value;
                if (string.IsNullOrEmpty(System.Configuration.ConfigurationManager.AppSettings["SAASUSERS"]))
                {
                    saasusercount = -1;
                }
                int usercount = 0;
                if (int.TryParse(System.Configuration.ConfigurationManager.AppSettings["SAASUSERS"], out usercount))
                {
                    saasusercount = usercount;
                }
                else
                {
                    saasusercount = -1;
                }
                return saasusercount.Value;
            }
        }
        int? iosusercount;
        public int IOSUserCount
        {
            get
            {
                if (iosusercount.HasValue) return iosusercount.Value;
                if (string.IsNullOrEmpty(System.Configuration.ConfigurationManager.AppSettings["IOSUSERS"]))
                {
                    iosusercount = -1;
                }
                int usercount = 0;
                if (int.TryParse(System.Configuration.ConfigurationManager.AppSettings["IOSUSERS"], out usercount))
                {
                    iosusercount = usercount;
                }
                else
                {
                    iosusercount = -1;
                }
                return iosusercount.Value;
            }
        }

        int? androidusercount;
        public int AndroidUserCount
        {
            get
            {
                if (androidusercount.HasValue) return androidusercount.Value;
                if (string.IsNullOrEmpty(System.Configuration.ConfigurationManager.AppSettings["ANDROIDUSERS"]))
                {
                    androidusercount = -1;
                }
                int usercount = 0;
                if (int.TryParse(System.Configuration.ConfigurationManager.AppSettings["ANDROIDUSERS"], out usercount))
                {
                    androidusercount = usercount;
                }
                else
                {
                    androidusercount = -1;
                }
                return androidusercount.Value;
            }
        }
        public virtual NpgsqlConnection ImageConnection()
        {
            string connstring = "";
            if (IsSAAS)
            {
                connstring = InfyPOS.Services.OrganizationManager.Instance.Get(this.Context != null ? this.Context.OrganizationID : 0).connectionstring;
            }
            else
            {
                if (System.Configuration.ConfigurationManager.ConnectionStrings["InfyPOSImage"] != null)
                {
                    connstring = System.Configuration.ConfigurationManager.ConnectionStrings["InfyPOSImage"].ConnectionString;
                }

                if (string.IsNullOrEmpty(connstring) && System.Configuration.ConfigurationManager.ConnectionStrings["InfyPOS"] != null)
                    connstring = System.Configuration.ConfigurationManager.ConnectionStrings["InfyPOS"].ConnectionString;

                if (string.IsNullOrEmpty(connstring))
                    connstring = "server=localhost;Port=5432;database=InfyPOS;user id=postgres;password=abc123";
            }
            return new NpgsqlConnection(connstring);
        }

        //private static BaseData instance = new BaseData();
        //public static BaseData Instance
        //{
        //    get
        //    {
        //        return instance;
        //    }
        //}

        internal static void UpdateParameters(Npgsql.NpgsqlCommand DbCommand, SortedList<string, object> Paramters)
        {
            if (Paramters != null)
            {
                IList<string> keys = Paramters.Keys;

                for (int i = 0; i < Paramters.Count; i++)
                {
                    DbCommand.Parameters.AddWithValue(keys[i], Paramters[keys[i]]);
                }
            }
        }

        public NpgsqlCommand GetCommand(string sql, SortedList<string, object> Paramters, NpgsqlConnection conn, NpgsqlTransaction transaction)
        {
            var command = new NpgsqlCommand(sql, conn, transaction);
            if (Paramters != null)
                UpdateParameters(command, Paramters);
            return command;
        }


        public DataSet ExecuteDataSet(string Query, SortedList<string, object> Paramters)
        {
            return ExecuteDataSet(Query, Paramters, 10000);
        }
        public DataSet ExecuteDataSet(string Query, SortedList<string, object> Paramters, int timeout)
        {
            using (NpgsqlConnection conn = CreateConnection())
            {
                conn.Open();
                NpgsqlCommand command = new NpgsqlCommand(Query, conn);
                UpdateParameters(command, Paramters);
                DataSet dset = new DataSet();
                command.CommandTimeout = timeout;
                NpgsqlDataAdapter adaptoer = new NpgsqlDataAdapter(command);
                adaptoer.SelectCommand.CommandTimeout = timeout;
                adaptoer.Fill(dset);
                conn.Close();
                return dset;
            }
        }

        public DataSet ExecuteDataSet(string Query, SortedList<string, object> Paramters, NpgsqlConnection conn, NpgsqlTransaction transaction)
        {
            NpgsqlCommand command = new NpgsqlCommand(Query, conn, transaction);
            UpdateParameters(command, Paramters);
            DataSet dset = new DataSet();
            NpgsqlDataAdapter adaptoer = new NpgsqlDataAdapter(command);
            adaptoer.Fill(dset);
            return dset;
        }

        public int ExecuteQuery(string Query, SortedList<string, object> Paramters)
        {
            return ExecuteQuery(Query, Paramters, 0);
        }
        public int ExecuteQuery(string Query, SortedList<string, object> Paramters, int timeout)
        {
            using (NpgsqlConnection conn = CreateConnection())
            {
                conn.Open();
                NpgsqlCommand command = new NpgsqlCommand(Query, conn);
                UpdateParameters(command, Paramters);
                if (timeout > 0)
                    command.CommandTimeout = timeout;
                int rows = command.ExecuteNonQuery();
                conn.Close();
                return rows;
            }
        }

        public int ExecuteQuery(string Query, SortedList<string, object> Paramters, NpgsqlConnection conn, NpgsqlTransaction transaction, int timeout)
        {
            NpgsqlCommand command = new NpgsqlCommand(Query, conn, transaction);
            UpdateParameters(command, Paramters);
            if (timeout > 0)
                command.CommandTimeout = timeout;
            if (transaction == null)
                return command.ExecuteNonQuery();

            int rows = command.ExecuteNonQuery();
            return rows;
        }

        public int ExecuteQuery(string Query, SortedList<string, object> Paramters, NpgsqlConnection conn, NpgsqlTransaction transaction)
        {
            NpgsqlCommand command = new NpgsqlCommand(Query, conn, transaction);
            UpdateParameters(command, Paramters);
            if (transaction == null)
                return command.ExecuteNonQuery();

            int rows = command.ExecuteNonQuery();
            return rows;
        }

        public IDataReader ExecuteReader(string Query, SortedList<string, object> Paramters, NpgsqlConnection conn)
        {
            NpgsqlCommand command = new NpgsqlCommand(Query, conn);
            UpdateParameters(command, Paramters);
            return command.ExecuteReader(CommandBehavior.Default);
        }
        public IDataReader ExecuteReader(string Query, SortedList<string, object> Paramters, NpgsqlConnection conn, NpgsqlTransaction transaction)
        {
            NpgsqlCommand command = new NpgsqlCommand(Query, conn, transaction);
            UpdateParameters(command, Paramters);
            return command.ExecuteReader(CommandBehavior.Default);
        }

        public IDataReader ExecuteReader(string Query, SortedList<string, object> Paramters)
        {
            return ExecuteReader(Query, Paramters, 0);
        }

        public IDataReader ExecuteSequenceReader(string Query, SortedList<string, object> Paramters, int timeout,
            NpgsqlConnection conn, NpgsqlTransaction transaction)
        {
            NpgsqlCommand command = new NpgsqlCommand(Query, conn);
            if (timeout > 0)
                command.CommandTimeout = timeout;
            command.Transaction = transaction;
            if (Paramters != null)
                UpdateParameters(command, Paramters);
            try
            {

                var reader = command.ExecuteReader();
                return reader;
            }
            catch (Exception exp)
            {
                if (conn != null && conn.State != ConnectionState.Closed)
                    conn.Close();
                throw exp;
            }
        }

        public IDataReader ExecuteReader(string Query, SortedList<string, object> Paramters, int timeout)
        {
            NpgsqlConnection conn = CreateConnection();
            conn.Open();
            NpgsqlCommand command = new NpgsqlCommand(Query, conn);
            if (timeout > 0)
                command.CommandTimeout = timeout;
            if (Paramters != null)
                UpdateParameters(command, Paramters);
            try
            {
                var reader = command.ExecuteReader(CommandBehavior.CloseConnection);
                return reader;
            }
            catch (Exception exp)
            {
                if (conn != null && conn.State != ConnectionState.Closed)
                    conn.Close();
                throw exp;
            }
        }

        public virtual NpgsqlCommand GetCommand(string Query, SortedList<string, object> Paramters)
        {
            NpgsqlConnection conn = CreateConnection();
            NpgsqlCommand command = new NpgsqlCommand(Query, conn);
            if (Paramters != null)
                UpdateParameters(command, Paramters);
            return command;
        }

        public virtual NpgsqlCommand GetCommand(string sql, NpgsqlConnection conn)
        {
            var command = new NpgsqlCommand(sql, conn);
            command.Parameters.AddWithValue("orgid", Context == null ? 0 : Context.OrganizationID);
            return command;
        }
        public virtual NpgsqlCommand GetCommand(string sql, NpgsqlConnection conn, NpgsqlTransaction transaction)
        {
            var command = new NpgsqlCommand(sql, conn, transaction);
            command.Parameters.AddWithValue("orgid", Context == null ? 0 : Context.OrganizationID);
            return command;
        }

        public string GenerateUnique25CharId()
        {
            // 1. Timestamp part (to make it sortable)
            string timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmssfff"); // 17 chars

            // 2. Random part from GUID, base36 (shortens 8 bytes to ~8 chars)
            Guid guid = Guid.NewGuid();
            long guidPart = BitConverter.ToInt64(guid.ToByteArray(), 0);
            string base36 = Base36Encode(Math.Abs(guidPart)); // ~8 chars

            // 3. Combine and ensure 25 chars
            string result = (timestamp + base36).PadRight(25, 'X');
            return result.Substring(0, 25);
        }

        public string Base36Encode(long input)
        {
            const string chars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            string result = string.Empty;
            while (input > 0)
            {
                result = chars[(int)(input % 36)] + result;
                input /= 36;
            }
            return result.PadLeft(8, '0');
        }

    }

    public class SAASData : BaseData
    {
        public bool HasResctrictedAccess
        {
            get
            {
                return InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.analyser_access_restrict;
            }
        }
        public override NpgsqlConnection CreateConnection()
        {
            string connstring = "";
            if (IsSAAS)
            {
                var organizatoinid = this.Context != null ? this.Context.OrganizationID : 0;
                if (organizatoinid == 0) throw new Exception("SAAS Application can't run without organization information");
                connstring = InfyPOS.Services.OrganizationManager.Instance.Get(organizatoinid).connectionstring;
            }
            else
            {
                if (System.Configuration.ConfigurationManager.ConnectionStrings["InfyRPT"] != null &&
               !string.IsNullOrEmpty(System.Configuration.ConfigurationManager.ConnectionStrings["InfyRPT"].ConnectionString))
                    connstring = System.Configuration.ConfigurationManager.ConnectionStrings["InfyRPT"].ConnectionString;
                else
                    connstring = System.Configuration.ConfigurationManager.ConnectionStrings["InfyPOS"].ConnectionString;
            }
            return new NpgsqlConnection(connstring);
        }

        public NpgsqlConnection AdminConnection()
        {
            string connstring = "";
            if (IsSAAS)
            {
                var organizatoinid = this.Context != null ? this.Context.OrganizationID : 0;
                if (organizatoinid == 0) throw new Exception("SAAS Application can't run without organization information");
                connstring = InfyPOS.Services.OrganizationManager.Instance.Get(organizatoinid).connectionstring;
            }
            else
            {
                if (System.Configuration.ConfigurationManager.ConnectionStrings["InfyPOSAdmin"] != null)
                {
                    connstring = System.Configuration.ConfigurationManager.ConnectionStrings["InfyPOSAdmin"].ConnectionString;
                }
                else
                {
                    throw new Exception("Please configure admin connection on web.config");
                }
            }
            return new NpgsqlConnection(connstring);
        }
        public override NpgsqlConnection ImageConnection()
        {
            string connstring = "";
            if (IsSAAS)
            {
                var organizatoinid = this.Context != null ? this.Context.OrganizationID : 0;
                if (organizatoinid == 0) throw new Exception("SAAS Application can't run without organization information");
                connstring = InfyPOS.Services.OrganizationManager.Instance.Get(organizatoinid).connectionstring;
            }
            else
            {
                if (System.Configuration.ConfigurationManager.ConnectionStrings["InfyPOSImage"] != null)
                {
                    connstring = System.Configuration.ConfigurationManager.ConnectionStrings["InfyPOSImage"].ConnectionString;
                }

                if (string.IsNullOrEmpty(connstring) && System.Configuration.ConfigurationManager.ConnectionStrings["InfyPOS"] != null)
                    connstring = System.Configuration.ConfigurationManager.ConnectionStrings["InfyPOS"].ConnectionString;

                if (string.IsNullOrEmpty(connstring))
                    connstring = "server=localhost;Port=5432;database=InfyPOS;user id=postgres;password=abc123";
            }
            return new NpgsqlConnection(connstring);
        }

        public override NpgsqlCommand GetCommand(string Query, SortedList<string, object> Paramters)
        {
            NpgsqlConnection conn = CreateConnection();
            NpgsqlCommand command = new NpgsqlCommand(Query, conn);
            if (Paramters != null)
                UpdateParameters(command, Paramters);
            return command;
        }

        public override NpgsqlCommand GetCommand(string sql, NpgsqlConnection conn)
        {
            var command = new NpgsqlCommand(sql, conn);
            command.Parameters.AddWithValue("orgid", Context == null ? 0 : Context.OrganizationID);
            return command;
        }
        public override NpgsqlCommand GetCommand(string sql, NpgsqlConnection conn, NpgsqlTransaction transaction)
        {
            var command = new NpgsqlCommand(sql, conn, transaction);
            command.Parameters.AddWithValue("orgid", Context == null ? 0 : Context.OrganizationID);
            return command;
        }

    }
    public class ReportData : BaseData
    {
        public bool HasResctrictedAccess
        {
            get
            {
                return InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.analyser_access_restrict;
            }
        }
        public NpgsqlConnection CreateDirectConnection()
        {
            string connstring = "";
            if (IsSAAS)
            {
                var organizatoinid = this.Context != null ? this.Context.OrganizationID : 0;
                if (organizatoinid == 0) throw new Exception("SAAS Application can't run without organization information");
                connstring = InfyPOS.Services.OrganizationManager.Instance.Get(organizatoinid).connectionstring;
            }
            else
            {
                connstring = System.Configuration.ConfigurationManager.ConnectionStrings["InfyPOS"].ConnectionString;
            }
            return new NpgsqlConnection(connstring);
        }
        public override NpgsqlConnection CreateConnection()
        {
            string connstring = "";
            if (IsSAAS)
            {
                var organizatoinid = this.Context != null ? this.Context.OrganizationID : 0;
                if (organizatoinid == 0) throw new Exception("SAAS Application can't run without organization information");
                connstring = InfyPOS.Services.OrganizationManager.Instance.Get(organizatoinid).connectionstring;
            }
            else
            {
                if (System.Configuration.ConfigurationManager.ConnectionStrings["InfyRPT"] != null &&
               !string.IsNullOrEmpty(System.Configuration.ConfigurationManager.ConnectionStrings["InfyRPT"].ConnectionString))
                    connstring = System.Configuration.ConfigurationManager.ConnectionStrings["InfyRPT"].ConnectionString;
                else
                    connstring = System.Configuration.ConfigurationManager.ConnectionStrings["InfyPOS"].ConnectionString;
            }
            return new NpgsqlConnection(connstring);
        }

        public override NpgsqlCommand GetCommand(string sql, NpgsqlConnection conn)
        {
            var command = base.GetCommand(sql, conn);
            command.Parameters.AddWithValue("orgid", Context == null ? 0 : Context.OrganizationID);
            command.CommandTimeout = 10000;
            return command;
        }
    }

    public static class DataReaderExtensions
    {
        public static SortedDictionary<string, int> Build(this IDataRecord r)
        {
            var map = new SortedDictionary<string, int>(System.StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < r.FieldCount; i++)
            {
                var name = r.GetName(i);
                // Keep the FIRST occurrence of duplicate names (important for SequentialAccess order)
                if (!map.ContainsKey(name))
                    map.Add(name, i);
            }
            return map;
        }

        public static object GetRaw(this IDataRecord r, string name)
        {
            var v = r[name];
            return v == DBNull.Value ? null : v;
        }

        public static string GetStringOr(this IDataRecord r, int ordinal, string @default = "")
        {
            return r.IsDBNull(ordinal) ? @default : r.GetString(ordinal);
        }

        public static long GetInt64Or(this IDataRecord r, int ordinal, long @default = 0L)
        {
            return r.IsDBNull(ordinal) ? @default : r.GetInt64(ordinal);
        }

        public static int GetInt32Or(this IDataRecord r, int ordinal, int @default = 0)
        {
            return r.IsDBNull(ordinal) ? @default : r.GetInt32(ordinal);
        }

        public static decimal GetDecimalOr(this IDataRecord r, int ordinal, decimal @default = 0m)
        {
            return r.IsDBNull(ordinal) ? @default : r.GetDecimal(ordinal);
        }

        public static bool GetBooleanOr(this IDataRecord r, int ordinal, bool @default = false)
        {
            return r.IsDBNull(ordinal) ? @default : r.GetBoolean(ordinal);
        }

        public static DateTime GetDateTimeOr(this IDataRecord r, int ordinal, DateTime? @default = null)
        {
            return r.IsDBNull(ordinal) ? (@default ?? DateTime.MinValue) : r.GetDateTime(ordinal);
        }

        // Optional: for cases where the DB type is not exactly string but you still want text
        // (e.g., numeric columns you want ToString()).
        public static string GetAsStringOr(this IDataRecord r, int ordinal, string @default = "")
        {
            return r.IsDBNull(ordinal) ? @default : Convert.ToString(r.GetValue(ordinal)) ?? @default;
        }
    }
}