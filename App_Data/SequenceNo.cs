using System;
using System.Data;
using System.Data.Common;
using System.Collections.Generic;
using System.Text;
using Npgsql;

namespace InfyPOS.Common
{
    public class SequenceManager
    {
        static System.Collections.Concurrent.ConcurrentDictionary<long, SequenceManager> instanceList;
        private System.Collections.Concurrent.ConcurrentDictionary<string, SequenceNo> list;
        static SequenceManager()
        {
            instanceList = new System.Collections.Concurrent.ConcurrentDictionary<long, SequenceManager>();
        }
        long organizationid;
        public SequenceManager(long organizationid)
        {
            this.organizationid = organizationid;
            list = new System.Collections.Concurrent.ConcurrentDictionary<string, SequenceNo>();
        }
        public static SequenceManager Instance(long organizationid)
        {
            SequenceManager instance = null;
            if (instanceList.TryGetValue(organizationid, out instance))
                return instance;

            instance = new SequenceManager(organizationid);
            if (!instanceList.TryAdd(organizationid, instance))
                instanceList.TryGetValue(organizationid, out instance);
            return instance;
        }

        public SequenceNo Get(string type, long locationid)
        {
            return Get(type, locationid, 0);
        }

        public SequenceNo Get(string type, long locationid, long companyid)
        {
            return Get(type, locationid, companyid, 0);
        }
        public SequenceNo Get(string type, long locationid, long companyid, long floorid)
        {
            if (companyid > 0 && !InfyPOS.Services.ConfigManager.Instance(organizationid).Config.system_multicompany)
                companyid = 0;

            string key = type + "-" + locationid + "-" + companyid + "-" + floorid;
            SequenceNo sn = null;
            if (list.TryGetValue(key, out sn))
                return sn;
            sn = new SequenceNo(organizationid, type, locationid, companyid, floorid);
            if (list.TryAdd(key, sn))
            {
                return sn;
            }
            list.TryGetValue(key, out sn);
            return sn;
        }
    }
    public class SequenceNo : SAASData
    {
        private SequenceNo(long organizationid)
        {
            this.organizationid = organizationid;
        }
        long organizationid;


        bool prefixexpiry { get; set; }
        DateTime prefixexpiryon { get; set; }
        bool suffixexpiry { get; set; }
        DateTime suffixexpiryon { get; set; }
        DateTime prefixvalidfrom { get; set; }
        DateTime suffixvalidfrom { get; set; }

        private long currentSlotID = -1;
        private string currentNo = string.Empty;
        private DateTime validTime = DateTime.MinValue;
        public string Description = string.Empty;
        public string Type = string.Empty;
        public long locationid { get; set; }
        public long floorid { get; set; }
        public long companyid { get; set; }
        public SequenceNo(long organizationid, string type, long locationid, long companyid)
        {
            Type = type;
            this.locationid = locationid;
            this.companyid = companyid;
            this.organizationid = organizationid;
        }
        public SequenceNo(long organizationid, string type, long locationid, long companyid, long floorid)
        {
            Type = type;
            this.locationid = locationid;
            this.floorid = floorid;
            this.companyid = companyid;
            this.organizationid = organizationid;
        }
        private List<string> validated = new List<string>();
        public bool Validate(UserClaim userClaim,string description, bool autocreate)
        {
            return Validate(userClaim, description, null, autocreate);
        }
        public bool Validate(UserClaim userClaim, string description, string prefix, bool autocreate)
        {
            string key = Type + "-" + locationid + "-" + companyid + "-" + floorid;
            if (validated.Contains(Type)) return true;
            Description = description;
            string query = "select count(*) from AutoNumber " +
                "where Type=@type and isactive = true and " +
                "COALESCE(locationid,0)=(case when @locationid =0 then COALESCE(locationid,0) else @locationid end) and " +
                "COALESCE(companyid,0)=(case when @companyid =0 then COALESCE(companyid,0) else @companyid end) and " +
                "COALESCE(floorid,0)=(case when @floorid =0 then COALESCE(floorid,0) else @floorid end) limit 1";
            SortedList<string, object> param = new SortedList<string, object>();
            param.Add("type", Type);
            param.Add("locationid", locationid);
            param.Add("companyid", companyid);
            param.Add("floorid", floorid);
            bool isavailable = true;
            Common.BaseData baseData = new Common.BaseData() { Context = new UserClaim() { OrganizationID = organizationid } };
            using (IDataReader reader = baseData.ExecuteReader(query, param))
            {
                if (reader.Read())
                {
                    int noofconfig = reader.GetInt32(0);
                    isavailable = noofconfig > 0;
                    if (noofconfig > 1)
                    {
                        if (Type == "billreturn.billno" || Type == "bulkbill.billno" ||
                            Type == "bill.billno" || Type == "customerbill.billno")
                        {
                            throw new Exception("Duplicate Sequence Number Configuration Found.");
                        }
                    }
                }
            }
            if (!autocreate)
                return isavailable;

            if (!isavailable)
            {
                query = @"INSERT INTO AutoNumber
                   (isactive, createdby, modifiedby, version, createdon, modifiedon, 
                                            identityfrom, identigyto, preincrement, timeout, description, 
                                            prefix, suffix, currentidentity, filler, nolength, type,locationid,companyid,floorid)
                    VALUES
                   ( true, 0, 0, 1, @current_date, 
                                            @current_date, 0, @maxnumber, false, 100, 
                                            @Description, @prefix, '', 0, '',25, @Type,@locationid,@companyid,@floorid)";
                if (string.IsNullOrEmpty(Description))
                    param.Add("@Description", "Auto Generated : " + Type);
                else
                    param.Add("@Description", Description);
                param.Add("@maxnumber", int.MaxValue);
                param.Add("@current_date", userClaim.DateTimeNow);
                param.Add("@prefix", string.IsNullOrEmpty(prefix) ? "" : prefix);
                ExecuteQuery(query, param);
            }
            validated.Add(Type);
            return isavailable;
        }

        public SequenceNo(string type, string description)
        {
            Type = type;
            Description = description;
        }

        //private string query;
        //SortedList<string, object> param;
        //public SequenceNo(string type, string parentColumn, string parentTable)
        //{
        //    Type = type;
        //    ParentColumn = parentColumn;
        //    ParentTable = parentTable;
        //    validateSNo = true;

        //    string query = string.Format("select top 1 {0} from {1} where {0} ilike @value", ParentColumn, ParentTable);
        //    param = new SortedList<string, object>();
        //}


        public bool IsValid()
        {
            if (currentSlotID == -1) return false;
            return (validTime > Context.DateTimeNow);
        }

        public bool SentNextNumber(long initialSlotID, long locationid, long currentSlotID,
            Npgsql.NpgsqlConnection conn, Npgsql.NpgsqlTransaction trans)
        {
            try
            {
                Npgsql.NpgsqlCommand command = new Npgsql.NpgsqlCommand(@"UPDATE autonumber a SET currentidentity=@currentSlotID
                                                where type=@type and isactive = true and currentidentity=@initialSlotID
                                                and COALESCE(locationid, 0) = (case when @locationid = 0 then COALESCE(locationid, 0) else @locationid end)
                                                and COALESCE(companyid,0)=(case when @companyid =0 then COALESCE(companyid,0) else @companyid end)
                                                and COALESCE(floorid,0)=(case when @floorid =0 then COALESCE(floorid,0) else @floorid end)
                                                RETURNING a.currentidentity,a.Prefix,a.Suffix,a.Filler,a.NoLength,a.id", conn, trans);
                command.Parameters.Add("type", NpgsqlTypes.NpgsqlDbType.Varchar).Value = Type;
                command.Parameters.Add("initialSlotID", NpgsqlTypes.NpgsqlDbType.Bigint).Value = initialSlotID;
                command.Parameters.Add("currentSlotID", NpgsqlTypes.NpgsqlDbType.Bigint).Value = currentSlotID;
                command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = locationid;
                command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = companyid;
                command.Parameters.Add("floorid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = floorid;
                using (var dr = command.ExecuteReader(CommandBehavior.SingleRow))
                {
                    if (!dr.Read())
                        return false;
                    currentSlotID = dr.GetInt64(0);
                    autonumbercurrentid = dr.GetInt64(5);
                }
                return true;
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(organizationid).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);
                return false;
            }
        }

        public long autonumbercurrentid = 0;
        public string prefix = "";
        public string Suffix = "";
        public string filler = "";
        public string autonumbertype = "";
        int length = 0;

        public bool CanResetOnDateChange()
        {
            return prefix.Contains("{dd}") || prefix.Contains("{dmy}");
        }
        public DateTime modifiedon { get; set; }
        public string GetFormatedNumber(long currentno, DateTime date)
        {
            var currentNo = currentno.ToString("F0");
            return GetFormatedValue(currentNo,null, date);
        }
        public string GetCurrentNumber(bool supportReset,DateTime actiondate, Npgsql.NpgsqlConnection conn, Npgsql.NpgsqlTransaction trans)
        {
            Npgsql.NpgsqlCommand command = new Npgsql.NpgsqlCommand(@"UPDATE autonumber a SET currentidentity=currentidentity
                                                where type=@type and isactive=true 
                                                and COALESCE(locationid, 0) = (case when @locationid = 0 then COALESCE(locationid, 0) else @locationid end)
                                                and COALESCE(companyid,0)=(case when @companyid =0 then COALESCE(companyid,0) else @companyid end)
                                                and COALESCE(floorid,0)=(case when @floorid =0 then COALESCE(floorid,0) else @floorid end)
                                                RETURNING a.currentidentity,a.Prefix,a.Suffix,a.Filler,a.NoLength,a.modifiedon", conn, trans);

            if(InfyPOS.Services.ConfigManager.Instance(organizationid).Config.system_autonumberwithexpiry)
            {
                command = new Npgsql.NpgsqlCommand(@"UPDATE autonumber a SET currentidentity=currentidentity
                                                where type=@type and isactive=true 
                                                and COALESCE(locationid, 0) = (case when @locationid = 0 then COALESCE(locationid, 0) else @locationid end)
                                                and COALESCE(companyid,0)=(case when @companyid =0 then COALESCE(companyid,0) else @companyid end)
                                                and COALESCE(floorid,0)=(case when @floorid =0 then COALESCE(floorid,0) else @floorid end)
                                                and (prefixexpiry is null or prefixexpiry = false or (@actiondate between prefixvalidfrom and prefixexpiryon))
                                                and (suffixexpiry is null or suffixexpiry = false or (@actiondate between suffixvalidfrom and suffixexpiryon))
                                                RETURNING a.currentidentity,a.Prefix,a.Suffix,a.Filler,a.NoLength,a.modifiedon", conn, trans);
            }

            command.Parameters.Add("type", NpgsqlTypes.NpgsqlDbType.Varchar).Value = Type;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = locationid;
            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = companyid;
            command.Parameters.Add("floorid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = floorid;
            command.Parameters.Add("actiondate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = actiondate;
            using (var dr = command.ExecuteReader(CommandBehavior.SingleRow))
            {
                if (dr.Read())
                {
                    currentSlotID = dr.GetInt64(0);
                    currentNo = currentSlotID.ToString("F0");
                    prefix = dr.GetString(1);
                    Suffix = dr.GetString(2);
                    filler = dr.IsDBNull(3) ? "" : dr.GetString(3);
                    length = dr.GetInt32(4);
                    modifiedon = dr.GetDateTime(5);
                    currentNo = GetFormatedValue(currentNo, null, supportReset ? modifiedon : Context.DateTimeNow);
                }
                else
                {
                    throw new Exception("No auto number configuration available.");
                }
            }
            return currentNo;
        }

        public string GetCurrentNumber(Npgsql.NpgsqlConnection conn, Npgsql.NpgsqlTransaction trans)
        {
            Npgsql.NpgsqlCommand command = new Npgsql.NpgsqlCommand(@"UPDATE autonumber a SET currentidentity=currentidentity
                                                where type=@type and isactive=true 
                                                and COALESCE(locationid, 0) = (case when @locationid = 0 then COALESCE(locationid, 0) else @locationid end)
                                                and COALESCE(companyid,0)=(case when @companyid =0 then COALESCE(companyid,0) else @companyid end)
                                                and COALESCE(floorid,0)=(case when @floorid =0 then COALESCE(floorid,0) else @floorid end)
                                                RETURNING a.currentidentity,a.Prefix,a.Suffix,a.Filler,a.NoLength,a.modifiedon", conn, trans);

            command.Parameters.Add("type", NpgsqlTypes.NpgsqlDbType.Varchar).Value = Type;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = locationid;
            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = companyid;
            command.Parameters.Add("floorid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = floorid;
            using (var dr = command.ExecuteReader(CommandBehavior.SingleRow))
            {
                if (dr.Read())
                {
                    currentSlotID = dr.GetInt64(0);
                    currentNo = currentSlotID.ToString("F0");
                    prefix = dr.GetString(1);
                    Suffix = dr.GetString(2);
                    filler = dr.IsDBNull(3) ? "" : dr.GetString(3);
                    length = dr.GetInt32(4);
                    modifiedon = dr.GetDateTime(5);
                    currentNo = GetFormatedValue(currentNo,null, Context.DateTimeNow);
                }
                else
                {
                    return null;
                }
            }
            return currentNo;
        }
        public bool HasDuplicate(DateTime actiondate, Npgsql.NpgsqlConnection conn, Npgsql.NpgsqlTransaction trans)
        {

            Npgsql.NpgsqlCommand command = new Npgsql.NpgsqlCommand(@"SELECT COUNT(*) FROM autonumber where type=@type and isactive=true 
                                                and COALESCE(locationid, 0) = (case when @locationid = 0 then COALESCE(locationid, 0) else @locationid end)
                                                and COALESCE(companyid,0)=(case when @companyid =0 then COALESCE(companyid,0) else @companyid end)
                                                and COALESCE(floorid,0)=(case when @floorid =0 then COALESCE(floorid,0) else @floorid end)
                                                and (prefixexpiry is null or prefixexpiry = false or (@actiondate between prefixvalidfrom and prefixexpiryon))
                                                and (suffixexpiry is null or suffixexpiry = false or (@actiondate between suffixvalidfrom and suffixexpiryon))", conn, trans);
            command.Parameters.Add("type", NpgsqlTypes.NpgsqlDbType.Varchar).Value = Type;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = locationid;
            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = companyid;
            command.Parameters.Add("floorid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = floorid;
            command.Parameters.Add("actiondate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = actiondate;

            using (var dr = command.ExecuteReader())
            {
                if (dr.Read())
                {
                    if (dr.GetInt32(0) > 1)
                        return true;
                }
            }
            return false;
        }
        public bool HasDuplicate(Npgsql.NpgsqlConnection conn, Npgsql.NpgsqlTransaction trans)
        {
            
            Npgsql.NpgsqlCommand command = new Npgsql.NpgsqlCommand(@"SELECT COUNT(*) FROM autonumber where type=@type and isactive=true 
                                                and COALESCE(locationid, 0) = (case when @locationid = 0 then COALESCE(locationid, 0) else @locationid end)
                                                and COALESCE(companyid,0)=(case when @companyid =0 then COALESCE(companyid,0) else @companyid end)
                                                and COALESCE(floorid,0)=(case when @floorid =0 then COALESCE(floorid,0) else @floorid end)", conn, trans);
            command.Parameters.Add("type", NpgsqlTypes.NpgsqlDbType.Varchar).Value = Type;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = locationid;
            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = companyid;
            command.Parameters.Add("floorid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = floorid;
            using (var dr = command.ExecuteReader())
            {
                if (dr.Read())
                {
                    if (dr.GetInt32(0) > 1)
                        return true;
                }
            }
            return false;
        }
        public string GetNextNumber(UserClaim Context, Npgsql.NpgsqlConnection conn, Npgsql.NpgsqlTransaction trans)
        {
            return GetNextNumber(Context,false, null, conn, trans);
        }
        public string GetNextNumber(UserClaim Context,DateTime? actiondate, Npgsql.NpgsqlConnection conn, Npgsql.NpgsqlTransaction trans)
        {
            return GetNextNumber(Context, false, actiondate, conn, trans);
        }

        private long? autonumberid { get; set; }
        public string autonumbername { get; set; }
        public string GetNextNumber(UserClaim Context, bool supportReset, DateTime? actiondate, 
            Npgsql.NpgsqlConnection conn, Npgsql.NpgsqlTransaction trans)
        {
            if (actiondate == null) actiondate = Context.DateTimeNow;
            if (!string.IsNullOrEmpty(CurrentNo) && IsValid())
            {
                return CurrentNo;
            }
            try
            {
                if (Type == "billreturn.billno" || Type == "bulkbill.billno" ||
                    Type == "bill.billno" || Type == "customerbill.billno")
                {
                    if (InfyPOS.Services.ConfigManager.Instance(organizationid).Config.system_autonumberwithexpiry
                        && actiondate.HasValue)
                    {
                        if (HasDuplicate(actiondate.Value, conn, trans)) throw new Exception(
                            string.Format("Duplicate Sequence Number Configuration Found. Type={0}, Location={1}, Company={2}, Floor={3}, MultiCompany={4}", 
                            Type, locationid, companyid, floorid, InfyPOS.Services.ConfigManager.Instance(organizationid).Config.system_multicompany));
                    }
                    else
                    {
                        if (HasDuplicate(conn, trans)) throw new Exception(
                            string.Format("Duplicate Sequence Number Configuration Found. Type={0}, Location={1}, Company={2}, Floor={3}, MultiCompany={4}", 
                            Type, locationid, companyid, floorid, InfyPOS.Services.ConfigManager.Instance(organizationid).Config.system_multicompany));
                    }
                }

                if (InfyPOS.Services.ConfigManager.Instance(organizationid).Config.system_autonumberwithexpiry)
                {
                    Npgsql.NpgsqlCommand command = null;
                    if (!autonumberid.HasValue)
                    {
                        string query = @"select id,description from autonumber where type=@type and isactive=true 
                                                and COALESCE(locationid, 0) = (case when @locationid = 0 then COALESCE(locationid, 0) else @locationid end)
                                                and COALESCE(companyid,0)=(case when @companyid =0 then COALESCE(companyid,0) else @companyid end)
                                                and COALESCE(floorid,0)=(case when @floorid =0 then COALESCE(floorid,0) else @floorid end)
                                                and (prefixexpiry is null or prefixexpiry = false or (@actiondate between prefixvalidfrom and prefixexpiryon))
                                                and (suffixexpiry is null or suffixexpiry = false or (@actiondate between suffixvalidfrom and suffixexpiryon))";

                        command = new Npgsql.NpgsqlCommand(query, conn, trans);
                        command.Parameters.Add("type", NpgsqlTypes.NpgsqlDbType.Varchar).Value = Type;
                        command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = locationid;
                        command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = companyid;
                        command.Parameters.Add("floorid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = floorid;
                        command.Parameters.Add("actiondate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = actiondate.HasValue?actiondate.Value:Context.DateTimeNow.Date;
                        using (var dr = command.ExecuteReader(CommandBehavior.SingleRow))
                        {
                            if (dr.Read())
                            {
                                autonumberid = dr.GetInt64(0);
                                autonumbername = dr.GetString(1);
                            }
                            else
                            {
                                throw new Exception("Auto number configuration Data is Null. This method or property cannot be called on Null values.");
                            }
                        }
                    }

                    command = new Npgsql.NpgsqlCommand(@"UPDATE autonumber a SET currentidentity=currentidentity+1
                                                where id=@id
                                                RETURNING a.currentidentity,a.Prefix,a.Suffix,a.Filler,a.NoLength,a.modifiedon,a.prefixexpiry,a.prefixexpiryon,
                                                a.suffixexpiry,a.suffixexpiryon,a.prefixvalidfrom,a.suffixvalidfrom,a.type", conn, trans);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = autonumberid.Value;
                    command.CommandTimeout = 500;
                    using (var dr = command.ExecuteReader(CommandBehavior.SingleRow))
                    {
                        if (dr.Read())
                        {
                            currentSlotID = dr.GetInt64(0);
                            currentNo = currentSlotID.ToString("F0");
                            prefix = dr.GetString(1);
                            Suffix = dr.GetString(2);
                            filler = dr.IsDBNull(3) ? "" : dr.GetString(3);
                            length = dr.GetInt32(4);
                            modifiedon = dr.GetDateTime(5);
                            prefixexpiry = dr.IsDBNull(6) ? false : dr.GetBoolean(6);
                            prefixexpiryon = dr.IsDBNull(7) ? DateTime.MaxValue : dr.GetDateTime(7);
                            suffixexpiry = dr.IsDBNull(8) ? false : dr.GetBoolean(8);
                            suffixexpiryon = dr.IsDBNull(9) ? DateTime.MaxValue : dr.GetDateTime(9);
                            prefixvalidfrom = dr.IsDBNull(10) ? DateTime.MaxValue : dr.GetDateTime(10);
                            suffixvalidfrom = dr.IsDBNull(11) ? DateTime.MaxValue : dr.GetDateTime(11);
                            autonumbertype = dr.IsDBNull(12) ? "" : dr.GetString(12);
                            currentNo = GetFormatedValue(currentNo, null,supportReset ? modifiedon : Context.DateTimeNow);
                        }
                    }
                    if (suffixexpiry && suffixvalidfrom.Date >= actiondate.Value && suffixexpiryon.Date <= actiondate.Value)
                    {
                        autonumberid = null;
                        throw new Exception("Auto generated number got failed (Suffix Expired - " + Type + "), please try again");
                    }
                    if (prefixexpiry && prefixvalidfrom.Date >= actiondate.Value && prefixexpiryon.Date <= actiondate.Value)
                    {
                        autonumberid = null;
                        throw new Exception("Auto generated number got failed (Prefix Expired - " + Type + "), please try again");
                    }

                }
                else
                {
                    Npgsql.NpgsqlCommand command = new Npgsql.NpgsqlCommand(@"UPDATE autonumber a SET currentidentity=currentidentity+1
                                                where type=@type and isactive=true 
                                                and COALESCE(locationid, 0) = (case when @locationid = 0 then COALESCE(locationid, 0) else @locationid end)
                                                and COALESCE(companyid,0)=(case when @companyid =0 then COALESCE(companyid,0) else @companyid end)
                                                and COALESCE(floorid,0)=(case when @floorid =0 then COALESCE(floorid,0) else @floorid end)
                                                RETURNING a.currentidentity,a.Prefix,a.Suffix,a.Filler,a.NoLength,a.modifiedon,a.prefixexpiry,a.prefixexpiryon,
                                                a.suffixexpiry,a.suffixexpiryon,a.id", conn, trans);
                    command.CommandTimeout = 500;
                    command.Parameters.Add("type", NpgsqlTypes.NpgsqlDbType.Varchar).Value = Type;
                    command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = locationid;
                    command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = companyid;
                    command.Parameters.Add("floorid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = floorid;
                    using (var dr = command.ExecuteReader(CommandBehavior.SingleRow))
                    {
                        if (dr.Read())
                        {
                            currentSlotID = dr.GetInt64(0);
                            currentNo = currentSlotID.ToString("F0");
                            prefix = dr.GetString(1);
                            Suffix = dr.GetString(2);
                            filler = dr.IsDBNull(3) ? "" : dr.GetString(3);
                            length = dr.GetInt32(4);
                            modifiedon = dr.GetDateTime(5);
                            prefixexpiry = dr.IsDBNull(6) ? false : dr.GetBoolean(6);
                            prefixexpiryon = dr.IsDBNull(7) ? DateTime.MaxValue : dr.GetDateTime(7);
                            suffixexpiry = dr.IsDBNull(8) ? false : dr.GetBoolean(8);
                            suffixexpiryon = dr.IsDBNull(9) ? DateTime.MaxValue : dr.GetDateTime(9);
                            currentNo = GetFormatedValue(currentNo, null, supportReset ? modifiedon : Context.DateTimeNow);
                            autonumbercurrentid = dr.GetInt64(10);
                        }
                        else
                        {
                            throw new Exception("Auto number configuration Data is Null. This method or property cannot be called on Null values.");
                        }
                    }
                    if (suffixexpiry && suffixexpiryon.Date < Context.DateTimeNow.Date)
                    {
                        autonumberid = null;
                        throw new Exception("Auto generated number got failed (Suffix Expired - " + Type + "), please try again");
                    }
                    if (prefixexpiry && prefixexpiryon.Date < Context.DateTimeNow.Date)
                    {
                        autonumberid = null;
                        throw new Exception("Auto generated number got failed (Prefix Expired - " + Type + "), please try again");
                    }
                }


                if (!string.IsNullOrEmpty(CurrentNo))
                    return CurrentNo;
                throw new Exception("Auto generated number got failed, please try again");
            }
            catch (Exception exp)
            {
                if (exp.Message == "Auto number configuration Data is Null. This method or property cannot be called on Null values.")
                {
                    if (Type == "billreturn.billno" || Type == "bulkbill.billno" ||
                        Type == "bill.billno" || Type == "customerbill.billno" || Type == "UAN.code")
                    {
                        throw new Exception("Auto number configuration missing!! Can't generate billsequance " + Type + "!.");
                    }

                    string sql = @"INSERT INTO public.autonumber(
                                            isactive, createdby, modifiedby, version, createdon, modifiedon, 
                                            identityfrom, identigyto, preincrement, timeout, description, 
                                            prefix, suffix, currentidentity, filler, nolength, type,locationid,companyid,floorid)
                                    VALUES ( true, 0, 0, 1, @current_date, 
                                            @current_date, 0, @maxnumber, false, 100, 
                                            @Description, '', '', 0, '',25, @Type,@locationid,@companyid,@floorid);";
                    SortedList<string, object> param = new SortedList<string, object>();
                    param.Add("@Type", Type);
                    param.Add("@locationid", locationid);
                    param.Add("@companyid", companyid);
                    param.Add("@floorid", floorid);
                    param.Add("@maxnumber", long.MaxValue);
                    param.Add("@current_date", Context.DateTimeNow);
                    if (string.IsNullOrEmpty(Description))
                        param.Add("@Description", "Auto Generated : " + Type);
                    else
                        param.Add("@Description", Description);
                    ExecuteQuery(sql, param, conn, trans);
                    return GetNextNumber(Context,supportReset, actiondate, conn, trans);
                }
                WebAPI.Common.Logger.Instance(organizationid).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);
                throw exp;
            }
        }

        internal bool ChangeCurrentNo(long newcurrentidentity,DateTime actiondate, NpgsqlConnection conn, NpgsqlTransaction trans)
        {
            Npgsql.NpgsqlCommand command = new Npgsql.NpgsqlCommand(@"UPDATE autonumber a SET currentidentity=@newcurrentidentity
                                                where type=@type and isactive=true and currentidentity=@currentidentity
                                                and COALESCE(locationid, 0) = (case when @locationid = 0 then COALESCE(locationid, 0) else @locationid end)
                                                and COALESCE(companyid,0)=(case when @companyid =0 then COALESCE(companyid,0) else @companyid end)
                                                and COALESCE(floorid,0)=(case when @floorid =0 then COALESCE(floorid,0) else @floorid end)
                                                RETURNING a.currentidentity,a.Prefix,a.Suffix,a.Filler,a.NoLength,a.modifiedon,a.prefixexpiry,a.prefixexpiryon,
                                                a.suffixexpiry,a.suffixexpiryon", conn, trans);

            if (InfyPOS.Services.ConfigManager.Instance(organizationid).Config.system_autonumberwithexpiry)
            {
                command = new Npgsql.NpgsqlCommand(@"UPDATE autonumber a SET currentidentity=@newcurrentidentity
                                                where type=@type and isactive=true and currentidentity=@currentidentity
                                                and COALESCE(locationid, 0) = (case when @locationid = 0 then COALESCE(locationid, 0) else @locationid end)
                                                and COALESCE(companyid,0)=(case when @companyid =0 then COALESCE(companyid,0) else @companyid end)
                                                and COALESCE(floorid,0)=(case when @floorid =0 then COALESCE(floorid,0) else @floorid end)
                                                and (prefixexpiry is null or prefixexpiry = false or (@actiondate between prefixvalidfrom and prefixexpiryon))
                                                and (suffixexpiry is null or suffixexpiry = false or (@actiondate between suffixvalidfrom and suffixexpiryon))
                                                RETURNING a.currentidentity,a.Prefix,a.Suffix,a.Filler,a.NoLength,a.modifiedon,a.prefixexpiry,a.prefixexpiryon,
                                                a.suffixexpiry,a.suffixexpiryon", conn, trans);
            }

            command.Parameters.Add("type", NpgsqlTypes.NpgsqlDbType.Varchar).Value = Type;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = locationid;
            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = companyid;
            command.Parameters.Add("floorid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = floorid;
            command.Parameters.Add("actiondate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = actiondate;

            command.Parameters.Add("newcurrentidentity", NpgsqlTypes.NpgsqlDbType.Bigint).Value = newcurrentidentity;
            command.Parameters.Add("currentidentity", NpgsqlTypes.NpgsqlDbType.Bigint).Value = currentSlotID;
            return command.ExecuteNonQuery() > 0;
        }



        public string GetFormatedValue(UserClaim Context, SortedDictionary<string, string> attribs,string currentNo)
        {
            return GetFormatedValue(currentNo, attribs, Context.DateTimeNow);
        }
        public string GetFormatedValue(string currentNo, DateTime sourcedate)
        {
            return GetFormatedValue(currentNo, null, sourcedate);
        }
        public string GetFormatedValue(string currentNo, SortedDictionary<string, string> attribs, DateTime sourcedate)
        {
            if(attribs != null && attribs.Count>0)
            {
                foreach(var attr in attribs)
                {
                    prefix = prefix.Replace("[" + attr.Key + "]", attr.Value);
                    Suffix = Suffix.Replace("[" + attr.Key + "]", attr.Value);
                }
            }
            if (prefix.IndexOf("{") >= 0)
                prefix = GetFormated(prefix, sourcedate);
            if (Suffix.IndexOf("{") >= 0)
                Suffix = GetFormated(Suffix, sourcedate);
            if (!string.IsNullOrEmpty(filler) && length > 0 && currentNo.Length < length)
            {
                currentNo = (new string(filler[0], length - currentNo.Length)) + currentNo;
            }
            return prefix + currentNo + Suffix;
        }

        public DateTime ValidUpto
        {
            get
            {
                return validTime;
            }
        }

        public long CurrentSlotID
        {
            get
            {
                return currentSlotID;
            }
            set
            {
                currentSlotID = value;
            }
        }

        public string CurrentNo
        {
            get
            {
                return currentNo;
            }
            set
            {
                currentNo = value;
            }
        }

        public class AmountBasedPrefix
        {
            public int from { get; set; }
            public int to { get; set; }
            public string prefix { get; set; }
        }
        private static List<AmountBasedPrefix> amountbasedprefix = null;
        static SequenceNo()
        {
            amountbasedprefix = new List<AmountBasedPrefix>();
            amountbasedprefix.Add(new AmountBasedPrefix() { from = 0, to = 1000, prefix = "A" });
            amountbasedprefix.Add(new AmountBasedPrefix() { from = 1000, to = 5000, prefix = "B" });
            amountbasedprefix.Add(new AmountBasedPrefix() { from = 5000, to = 10000, prefix = "C" });
            amountbasedprefix.Add(new AmountBasedPrefix() { from = 10000, to = 25000, prefix = "D" });
            amountbasedprefix.Add(new AmountBasedPrefix() { from = 25000, to = 50000, prefix = "E" });
            amountbasedprefix.Add(new AmountBasedPrefix() { from = 50000, to = 100000, prefix = "F" });
            amountbasedprefix.Add(new AmountBasedPrefix() { from = 100000, to = 500000, prefix = "P" });
            amountbasedprefix.Add(new AmountBasedPrefix() { from = 500000, to = 1000000, prefix = "U" });
            amountbasedprefix.Add(new AmountBasedPrefix() { from = 1000000, to = int.MaxValue, prefix = "Z" });
        }

        public string GetAmountBasedPrefix(decimal amount)
        {
            amount = Math.Abs(amount);
            if (amount > int.MaxValue)
            {
                return "X";
            }
            int amt = (int)amount;
            return amountbasedprefix.Find(e => e.from <= amt && e.to >= amt).prefix;
        }

        public string GetFormated(string value, DateTime sourcedate)
        {
            DateTime dateTime = sourcedate;
            value = value.Replace("{dd}", dateTime.Date.ToString());
            value = value.Replace("{mm}", dateTime.Month.ToString());
            value = value.Replace("{yy}", dateTime.Year.ToString().Substring(2));
            value = value.Replace("{dmy}", dateTime.ToString("ddMMyy"));
            if (value.IndexOf("{f") >= 0)
            {
                string fs = dateTime.ToString("yyyy").Substring(2);
                string fe = ((int)(dateTime.Year + 1)).ToString().Substring(2);
                if (dateTime.Month <= 3)
                {
                    fe = fs;
                    fs = ((int)(dateTime.Year - 1)).ToString().Substring(2);
                }
                value = value.Replace("{fs}", fs);
                value = value.Replace("{fe}", fe);
            }
            return value;
        }

        internal bool ResetNumber(Npgsql.NpgsqlConnection conn, Npgsql.NpgsqlTransaction trans, DateTime dateTime)
        {
            try
            {
                dateTime = dateTime.Date.Add(Context.DateTimeNow.TimeOfDay);
                Npgsql.NpgsqlCommand command = new Npgsql.NpgsqlCommand(@"UPDATE autonumber a SET currentidentity=0,modifiedon=@currentdate
                                                where type=@type and isactive = true 
                                                and COALESCE(locationid, 0) = (case when @locationid = 0 then COALESCE(locationid, 0) else @locationid end)
                                                and COALESCE(companyid,0)=(case when @companyid =0 then COALESCE(companyid,0) else @companyid end)
                                                and COALESCE(floorid,0)=(case when @floorid =0 then COALESCE(floorid,0) else @floorid end)
                                                RETURNING a.currentidentity,a.Prefix,a.Suffix,a.Filler,a.NoLength,a.modifiedon", conn, trans);

                if (InfyPOS.Services.ConfigManager.Instance(organizationid).Config.system_autonumberwithexpiry)
                {
                    command = new Npgsql.NpgsqlCommand(@"UPDATE autonumber a SET currentidentity=0,modifiedon=@currentdate
                                                where type=@type and isactive = true 
                                                and COALESCE(locationid, 0) = (case when @locationid = 0 then COALESCE(locationid, 0) else @locationid end)
                                                and COALESCE(companyid,0)=(case when @companyid =0 then COALESCE(companyid,0) else @companyid end)
                                                and COALESCE(floorid,0)=(case when @floorid =0 then COALESCE(floorid,0) else @floorid end)
                                                and (prefixexpiry is null or prefixexpiry = false or (@actiondate between prefixvalidfrom and prefixexpiryon))
                                                and (suffixexpiry is null or suffixexpiry = false or (@actiondate between suffixvalidfrom and suffixexpiryon))
                                                RETURNING a.currentidentity,a.Prefix,a.Suffix,a.Filler,a.NoLength,a.modifiedon", conn, trans);
                }

                command.Parameters.Add("type", NpgsqlTypes.NpgsqlDbType.Varchar).Value = Type;
                command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = locationid;
                command.Parameters.Add("currentdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = dateTime;
                command.Parameters.Add("actiondate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = dateTime.Date;
                command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = companyid;
                command.Parameters.Add("floorid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = floorid;
                using (var dr = command.ExecuteReader(CommandBehavior.SingleRow))
                {
                    if (!dr.Read())
                        return false;
                    currentSlotID = dr.GetInt64(0);
                    modifiedon = dr.GetDateTime(5);
                }
                return true;
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(organizationid).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);
                return false;
            }
        }
    }
}
