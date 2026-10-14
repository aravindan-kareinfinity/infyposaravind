using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace InfyPOS.Common
{

    public class EnableETag : ActionFilterAttribute
    {
        Mode mode;
        public enum Mode
        {
            ReferenceList
        }
        public EnableETag(Mode mode):base()
        {
            this.mode = mode;
        }

        public DateTime GetLastModified(long organizationid,string table)
        {
            var datetime = DateTime.Now.AddYears(-1).Date;
            Npgsql.NpgsqlConnection connection = null;
            try
            {
                BaseData baseData = new BaseData();
                if (baseData.IsSAAS) baseData.Context = new UserClaim() { OrganizationID = organizationid };
                string query = "Select modifiedon from " + table + " order by modifiedon desc limit 1";
                
                using (connection = baseData.CreateConnection())
                {
                    using (var reader = baseData.ExecuteReader(query, null))
                    {
                        if (reader.Read() && !reader.IsDBNull(0))
                            datetime = reader.GetDateTime(0);
                    }
                    connection.Close();
                }
            }catch(Exception exp)
            {
                if (connection != null && connection.State == System.Data.ConnectionState.Open)
                    connection.Close();
            }
            return datetime;
        }

        public DateTime GetLastModifiedByUser(long organizationid,string table,long userid)
        {
            DateTime dateTime = DateTime.MinValue;
            DateTime dateTimeuser = DateTime.MinValue;
            Npgsql.NpgsqlConnection connection = null;
            try
            {
                BaseData baseData = new BaseData();
                if (baseData.IsSAAS) baseData.Context = new UserClaim() { OrganizationID = organizationid };
                string query = "Select modifiedon from " + table + " order by modifiedon desc limit 1";
                using (connection = baseData.CreateConnection())
                {
                    using (var reader = baseData.ExecuteReader(query, null))
                    {
                        if (reader.Read() && !reader.IsDBNull(0))
                            dateTime = reader.GetDateTime(0);
                        else
                            dateTime = DateTime.Now.AddYears(-1).Date;
                    }
                    connection.Close();
                }

                query = "Select modifiedon from users where id=@id";
                var param = new SortedList<string, object>();
                param.Add("id", userid);
                using (connection = baseData.CreateConnection())
                {
                    using (var reader = baseData.ExecuteReader(query, param))
                    {
                        if (reader.Read() && !reader.IsDBNull(0))
                            dateTimeuser = reader.GetDateTime(0);
                        else
                            dateTimeuser = DateTime.Now.AddYears(-1).Date;
                    }
                    connection.Close();
                }
            }catch(Exception exp)
            {
                if (connection != null && connection.State == System.Data.ConnectionState.Open)
                    connection.Close();
            }
            if (dateTimeuser > dateTime)
                return dateTimeuser;

            return dateTime;
        }

        public DateTime? GetLastModified(long organizationid, string id, string productgroup)
        {
            if (id == "AUTONUMBER")
            {
                return GetLastModified(organizationid, "Autonumber");
                //select locationid, type, prefix, filler, nolength from autonumber
            }
            if (id == "BILLKEY")
            {
                return GetLastModified(organizationid, "Referencelist where type in ('BULKSALEREASON','SEASONS','AREA')");
            }
            if (id == "SIZE" || id == "VSIZE")
            {
                return GetLastModified(organizationid, "Referencelist where type in ('SIZE')");
            }
            if (id == "PROMOTIONS")
            {
                return GetLastModified(organizationid, "Promotions");
            }
            if (id == "RECHARGESCHEMES")
            {
                return GetLastModified(organizationid, "Rechargescheme");
            }
            if (id == "PURCHASETAX" || id == "SALESTAX" || id == "ALLTAX" || id == "CESSTAX")
            {
                return GetLastModified(organizationid, "Tax");
            }
            if (id == "SIZEGROUPLIST")
            {
                return GetLastModified(organizationid, "SIZEGROUP");
            }
            if (id == "PRODUCTGROUP")
            {
                return GetLastModified(organizationid, "Productgroup");
            }
            if (id == "SUPPLIER" || id == "SUPPLIERLEDGER" || id == "LIST-SUPPLIER" || id == "SUPPLIERWITHCITY")
            {
                return GetLastModified(organizationid, "Supplier");
            }
            if (id == "TRANSPORT" || id == "TRANSPORTWITHCHARGE")
            {
                return GetLastModified(organizationid, "Transport");
            }
            if (id == "SUPPLIERCOMPANY" || id == "LIST-SUPPLIERCOMPANY")
            {
                return GetLastModified(organizationid, "Suppliercompany");
            }
            if (id == "BUYINGCOMPANY" || id == "SELLINGCOMPANY" || id == "COMPANY")
            {
                return GetLastModified(organizationid, "Company");
            }
            if (id == "NONUAN")
            {
                return GetLastModified(organizationid, "Items where nonuan=true");
            }
            if (id == "USERLIST")
            {
                return GetLastModified(organizationid, "Users");
            }
            if (id == "PRODUCTS")
            {
                return GetLastModified(organizationid, "Products");
            }
            if (id == "MFGWOFLOW")
            {
                return GetLastModified(organizationid, "Mfgworkfloworchestration");
            }
            if (id == "SETPRODUCTS")
            {
                return GetLastModified(organizationid, "mfgdesign");
            }
            if (id == "FABRIC" || id== "BOM" || id == "BOMLIST")
            {
                return GetLastModified(organizationid, "bommaster");
            }
            if (id == "FABRIC")
            {
                return GetLastModified(organizationid, "bommaster");
            }
            if (id == "ALLPRODUCTS")
            {
                return GetLastModified(organizationid, "Products");
            }
            if (id == "AGENT")
            {
                return GetLastModified(organizationid, "Agent");
            }
            if (id == "STRUCTURE" || id == "FULL-STRUCTURE")
            {
                return GetLastModified(organizationid, "Employeesalarystructure");  
            }
            if (id == "LEDGERS" || id == "TDSGROUP" || id == "EXPENSELEDGER" || id == "TDSRULES" || id == "LEDGERHEADERS") 
            {
                return GetLastModified(organizationid, "ledger");
            }
            if (id == "ITEMS")
            {
                return GetLastModified(organizationid, "items where allowsearch=true");
            }
            if (id == "EMPLOY" || id == "EMPLOYONLY" || id == "IEMPLOY" ||id == "EMPLOYEELEDGER" || id == "SALESMEN" || id == "PURCHASEMANAGER")
            {
                return GetLastModified(organizationid, "Employee");
            }
            if (id == "TAILORS")
            {
                return GetLastModified(organizationid, "TAILOR");
            }
            if (id == "SCHEMES")
            {
                return GetLastModified(organizationid, "Buybackscheme");
            }
            if (id == "AGREEMENTPO")
            {
                return GetLastModified(organizationid, "purchaseorder");
            }

            if (id == "BRAND")
            {
                return GetLastModified(organizationid, "Brand");
            }
            if (id == "REFILLBOX")
            {
                return GetLastModified(organizationid, "RefillBox");
            }
            if (id == "PRICEDESIGN")
            {
                if (Services.ConfigManager.Instance(organizationid).Config.system_ismfgapplication)
                {
                    var mfgdeate =  GetLastModified(organizationid, "mfgdesign");
                    var cusdate = GetLastModified(organizationid, "Customerprice");
                    if (mfgdeate > cusdate) return mfgdeate;
                    return cusdate;
                }
                else
                {
                    return GetLastModified(organizationid, "Customerprice");
                }
            }
            if (id == "CUSTOMER" || id == "RETAILCUSTOMER" || id == "CUSTOMERLEDGER" || id == "RETAILCUSTOMERLEDGER" 
                || id == "LIST-CUSTOMER" || id == "CUSTOMERLEDGERUNAPPROVED" || id == "CUSTOMERONLY" || id== "FRANCHISECUSTOMER")
            {
                return GetLastModified(organizationid, "Customer");
            }
            if (id == "MFGSAMPLES" || id == "MFGPENDINGSO" || id == "BARCODEFILE")
                return null;
            if (id == "MYLOCATION")
            {
                if (System.Web.HttpContext.Current != null)
                {
                    var Context = new UserClaim(System.Web.HttpContext.Current.Request.GetOwinContext().Authentication);
                    if (Context != null && Context.UserID > 0)
                    {
                        return GetLastModifiedByUser(organizationid, "referencelist where type = 'LOCATION'", Context.UserID);
                    }
                }
                return null;
            }
            if (id == "WHLOCATION" || id == "LOCATION" 
                || id == "RETAILLOCATION" || id.EndsWith("-LOCATION")
                || id == "ALLLOCATION")
            {
                return GetLastModified(organizationid, "referencelist where type = 'LOCATION'");
            }
            if (id.StartsWith("DIRECT-REORDER"))
            {
                return null;
            }
            if (id.StartsWith("DIRECT-"))
            {
                return GetLastModified(organizationid, "referencelist where type = '" + WebAPI.Data.Utility.SafeSQL(id.Substring(7)) + "'");
            }
            if (id.StartsWith("LIST-"))
            {
                return GetLastModified(organizationid, "referencelist where type = '" + WebAPI.Data.Utility.SafeSQL(id.Substring(5)) + "'");
            }
            if (id.StartsWith("FULL-"))
            {
                return GetLastModified(organizationid, "referencelist where type = '" + WebAPI.Data.Utility.SafeSQL(id.Substring(5)) + "'");
            }
            return GetLastModified(organizationid, "referencelist where type = '" + WebAPI.Data.Utility.SafeSQL(id) + "'");
        }

        public static Guid GenerateTimeBasedGuid(DateTime dateTime)
        {
            if (dateTime.Year < 2000) dateTime = DateTime.Now.AddYears(-1).Date;
            return GuidGenerator.GenerateTimeBasedGuid(dateTime);
        }

        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            //Get the request
            var request = actionContext.Request;
            if (request.Method == HttpMethod.Get)
            {
                //Get if If-None match header
                var clientEtags = request.Headers.IfNoneMatch;

                if (clientEtags.Count > 0)
                {
                    EntityTagHeaderValue etag = GeteTag(actionContext.Request);
                    if (etag != null && clientEtags.Any(t => t.Tag == etag.Tag))
                    {
                        actionContext.Response = new HttpResponseMessage(HttpStatusCode.NotModified);
                        SetCacheControl(actionContext.Response);
                    }
                }
            }
        }

        private void SetCacheControl(HttpResponseMessage httpResponseMessage)
        {
            httpResponseMessage.Headers.CacheControl = new CacheControlHeaderValue()
            {
                MaxAge = TimeSpan.FromSeconds(3),
                MustRevalidate = true,
                Private = true
            };
        }

        private string GetKey(HttpRequestMessage request)
        {
            return request.RequestUri.ToString();
        }

        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            if (actionExecutedContext.Response != null &&
                actionExecutedContext.Response.Headers != null)
            {
                actionExecutedContext.Response.Headers.ETag = GeteTag(actionExecutedContext.Request);
                SetCacheControl(actionExecutedContext.Response);
            }
        }

        public EntityTagHeaderValue GeteTag(HttpRequestMessage request)
        {
            UserClaim Context = null;
            if (System.Web.HttpContext.Current != null)
            {
                Context = new UserClaim(System.Web.HttpContext.Current.Request.GetOwinContext().Authentication);
            }
            DateTime dt = DateTime.Now.AddYears(-1).Date;
            if (mode == Mode.ReferenceList)
            {
                var array = request.RequestUri.LocalPath.Split(new string[] { "ReferencelistService/" }, StringSplitOptions.RemoveEmptyEntries);
                array = array[1].Split('/');
                string id = array.Length > 0 ? array[0] : "";
                string productgroup = array.Length > 1 ? array[1] : "";
                DateTime? mdate = GetLastModified(Context != null ? Context.OrganizationID : 0 ,id, productgroup);
                if (!mdate.HasValue)
                    return null;
                dt = mdate.Value;
            }
            if (Context != null)
            {
                if (Context.UserID > 0)
                {
                    return new EntityTagHeaderValue("\"" + GenerateTimeBasedGuid(dt).ToString()+"-"+ Context.UserID + "\"");
                }
            }
            return new EntityTagHeaderValue("\"" + GenerateTimeBasedGuid(dt).ToString() + "\"");
        }

        public static partial class GuidGenerator
        {
            // number of bytes in guid
            public const int ByteArraySize = 16;

            // multiplex variant info
            public const int VariantByte = 8;
            public const int VariantByteMask = 0x3f;
            public const int VariantByteShift = 0x80;

            // multiplex version info
            public const int VersionByte = 7;
            public const int VersionByteMask = 0x0f;
            public const int VersionByteShift = 4;

            // indexes within the uuid array for certain boundaries
            private const byte TimestampByte = 0;
            private const byte GuidClockSequenceByte = 8;
            private const byte NodeByte = 10;

            // offset to move from 1/1/0001, which is 0-time for .NET, to gregorian 0-time of 10/15/1582
            private static readonly DateTimeOffset GregorianCalendarStart = new DateTimeOffset(1582, 10, 15, 0, 0, 0, TimeSpan.Zero);

            // random clock sequence and node
            public static byte[] DefaultClockSequence { get; set; }
            public static byte[] DefaultNode { get; set; }

            static GuidGenerator()
            {
                DefaultClockSequence = new byte[2];
                DefaultNode = new byte[6];

                var random = new Random();
                random.NextBytes(DefaultClockSequence);
                random.NextBytes(DefaultNode);
            }

            //public static GuidVersion GetVersion(this Guid guid)
            //{
            //    byte[] bytes = guid.ToByteArray();
            //    return (GuidVersion)((bytes[VersionByte] & 0xFF) >> VersionByteShift);
            //}

            public static DateTimeOffset GetDateTimeOffset(Guid guid)
            {
                byte[] bytes = guid.ToByteArray();

                // reverse the version
                bytes[VersionByte] &= (byte)VersionByteMask;
                bytes[VersionByte] |= (byte)((byte)GuidVersion.TimeBased >> VersionByteShift);

                byte[] timestampBytes = new byte[8];
                Array.Copy(bytes, TimestampByte, timestampBytes, 0, 8);

                long timestamp = BitConverter.ToInt64(timestampBytes, 0);
                long ticks = timestamp + GregorianCalendarStart.Ticks;

                return new DateTimeOffset(ticks, TimeSpan.Zero);
            }

            public static DateTime GetDateTime(Guid guid)
            {
                return GetDateTimeOffset(guid).DateTime;
            }

            public static DateTime GetLocalDateTime(Guid guid)
            {
                return GetDateTimeOffset(guid).LocalDateTime;
            }

            public static DateTime GetUtcDateTime(Guid guid)
            {
                return GetDateTimeOffset(guid).UtcDateTime;
            }

            public static Guid GenerateTimeBasedGuid()
            {
                return GenerateTimeBasedGuid(DateTimeOffset.UtcNow, DefaultClockSequence, DefaultNode);
            }

            public static Guid GenerateTimeBasedGuid(DateTime dateTime)
            {
                return GenerateTimeBasedGuid(dateTime, DefaultClockSequence, DefaultNode);
            }

            public static Guid GenerateTimeBasedGuid(DateTimeOffset dateTime)
            {
                return GenerateTimeBasedGuid(dateTime, DefaultClockSequence, DefaultNode);
            }

            public static Guid GenerateTimeBasedGuid(DateTime dateTime, byte[] clockSequence, byte[] node)
            {
                return GenerateTimeBasedGuid(new DateTimeOffset(dateTime), clockSequence, node);
            }

            public static Guid GenerateTimeBasedGuid(DateTimeOffset dateTime, byte[] clockSequence, byte[] node)
            {
                if (clockSequence == null)
                    throw new ArgumentNullException("clockSequence");

                if (node == null)
                    throw new ArgumentNullException("node");

                if (clockSequence.Length != 2)
                    throw new ArgumentOutOfRangeException("clockSequence", "The clockSequence must be 2 bytes.");

                if (node.Length != 6)
                    throw new ArgumentOutOfRangeException("node", "The node must be 6 bytes.");

                long ticks = (dateTime - GregorianCalendarStart).Ticks;
                byte[] guid = new byte[ByteArraySize];
                byte[] timestamp = BitConverter.GetBytes(ticks);

                // copy node
                Array.Copy(node, 0, guid, NodeByte, Math.Min(6, node.Length));

                // copy clock sequence
                Array.Copy(clockSequence, 0, guid, GuidClockSequenceByte, Math.Min(2, clockSequence.Length));

                // copy timestamp
                Array.Copy(timestamp, 0, guid, TimestampByte, Math.Min(8, timestamp.Length));

                // set the variant
                guid[VariantByte] &= (byte)VariantByteMask;
                guid[VariantByte] |= (byte)VariantByteShift;

                // set the version
                guid[VersionByte] &= (byte)VersionByteMask;
                guid[VersionByte] |= (byte)((byte)GuidVersion.TimeBased << VersionByteShift);

                return new Guid(guid);
            }

            public enum GuidVersion
            {
                TimeBased = 0x01,
                Reserved = 0x02,
                NameBased = 0x03,
                Random = 0x04
            }
        }
    }
}