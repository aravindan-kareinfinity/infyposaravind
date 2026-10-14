using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
using System.Text;

namespace InfyPOS.Data
{
    public class OrganisationDataService
    {
        public NpgsqlConnection CreateConnection()
        {
            string connstring = System.Configuration.ConfigurationManager.ConnectionStrings["InfyPOS"].ConnectionString;
            return new NpgsqlConnection(connstring);
        }
        public NpgsqlConnection ImageConnection()
        {
            string connstring = "";

            if (System.Configuration.ConfigurationManager.ConnectionStrings["InfyPOSImage"] != null)
            {
                connstring = System.Configuration.ConfigurationManager.ConnectionStrings["InfyPOSImage"].ConnectionString;
            }

            if (string.IsNullOrEmpty(connstring) && System.Configuration.ConfigurationManager.ConnectionStrings["InfyManager"] != null)
                connstring = System.Configuration.ConfigurationManager.ConnectionStrings["InfyManager"].ConnectionString;

            if (string.IsNullOrEmpty(connstring))
                throw new Exception("Can't find the Infy Manager configuration");

            return new NpgsqlConnection(connstring);
        }
        private static OrganisationDataService sourceData = new OrganisationDataService();
        private static System.Collections.Concurrent.ConcurrentDictionary<string, long> organizationcode = new System.Collections.Concurrent.ConcurrentDictionary<string, long>();

        internal Organisation Search(string organizationcode)
        {
            if (string.IsNullOrEmpty(organizationcode)) return null;
            var list = Search(new OrganisationCreteria() { code = organizationcode });
            if (list.Count == 1) return list.First();
            return null;
        }

        public static long GetOrganizationID(string code)
        {
            if (string.IsNullOrEmpty(code)) return 0;

            if (organizationcode.ContainsKey(code))
                return organizationcode[code];

            var rows = sourceData.Search(new OrganisationCreteria()
            {
                code = code
            });

            if (rows.Count == 1)
            {
                organizationcode.TryAdd(rows[0].code, rows[0].id);
                return rows[0].id;
            }
            return 0;
        }

        const string SQL_Search = @"select id,code, name, contactperson, contactno, emailid, address, cityid, stateid, 
        countryid, pincode, nooflocation, noofusers, lastpayment, subscriptionfees, remarks, haspaymentdue, modules, 
        isactive, createdby, modifiedby, version, createdon, modifiedon,connectionstring,dbname,dbserver,accountlockedon,
        accountlock ,noofcompany,nextdue,dbusername,dbpassword,dbstatus,quantousername,quantopassword,adminusername,
        adminpassword,appconfig from saaslicense where id=@id";

        const string SQL_Insert = @"INSERT INTO public.saaslicense(code, name, contactperson, contactno, emailid, address, cityid, stateid, countryid, pincode, nooflocation, noofusers, lastpayment, subscriptionfees, remarks, haspaymentdue, modules, isactive, createdby, modifiedby, version, createdon, 
        modifiedon,connectionstring,dbname,dbserver,accountlockedon,accountlock ,noofcompany,nextdue,dbstatus,quantousername,quantopassword,adminusername,adminpassword,appconfig) 
        VALUES(@code, @name, @contactperson, @contactno, @emailid, @address, @cityid, @stateid, @countryid, @pincode, @nooflocation, @noofusers, @lastpayment,
        @subscriptionfees, @remarks, @haspaymentdue, @modules, @isactive, @createdby, @modifiedby, @version, @createdon, @modifiedon,@connectionstring,@dbname,@dbserver,
        @accountlockedon,@accountlock ,@noofcompany,@nextdue,@dbstatus,@quantousername,@quantopassword,@adminusername,@adminpassword,@appconfig) RETURNING id";

        const string SQL_Update = @"UPDATE public.saaslicense SET code=@code, name=@name, contactperson=@contactperson, contactno=@contactno, emailid=@emailid, address=@address, cityid=@cityid, stateid=@stateid, countryid=@countryid, pincode=@pincode, nooflocation=@nooflocation, noofusers=@noofusers, lastpayment=@lastpayment, subscriptionfees=@subscriptionfees, 
        remarks=@remarks, haspaymentdue=@haspaymentdue, modules=@modules, modifiedby=@modifiedby, dbstatus=@dbstatus,
        version=@version+1, modifiedon=@modifiedon,connectionstring=@connectionstring,dbname=@dbname,dbserver=@dbserver,
        accountlockedon=@accountlockedon,accountlock=@accountlock,noofcompany=@noofcompany,nextdue=@nextdue,
        quantousername=@quantousername,quantopassword=@quantopassword,adminusername=@adminusername,adminpassword=@adminpassword,appconfig=@appconfig
        where id=@id and version=@version RETURNING version";

        const string SQL_Delete = @"update public.saaslicense set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";

        public List<Organisation> Search(OrganisationCreteria _organisation)
        {
            List<Organisation> lst = new List<Organisation>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_organisation.id != 0)
                {
                    command = new NpgsqlCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _organisation.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("isactive", true, "=");

                    //if (!string.IsNullOrEmpty(_organisation.Code))
                    //    qb.AddParameter("code", _organisation.Code, "=");

                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Organisation oorganisation = new Organisation();
                        oorganisation.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        oorganisation.code = reader["code"] == DBNull.Value ? "" : reader["code"].ToString();
                        oorganisation.name = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
                        oorganisation.contactperson = reader["contactperson"] == DBNull.Value ? "" : reader["contactperson"].ToString();
                        oorganisation.contactno = reader["contactno"] == DBNull.Value ? "" : reader["contactno"].ToString();
                        oorganisation.emailid = reader["emailid"] == DBNull.Value ? "" : reader["emailid"].ToString();
                        oorganisation.address = reader["address"] == DBNull.Value ? "" : reader["address"].ToString();
                        oorganisation.cityid = reader["cityid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["cityid"]);
                        oorganisation.stateid = reader["stateid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["stateid"]);
                        oorganisation.countryid = reader["countryid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["countryid"]);
                        oorganisation.pincode = reader["pincode"] == DBNull.Value ? "" : reader["pincode"].ToString();
                        oorganisation.nooflocation = reader["nooflocation"] == DBNull.Value ? 0 : Convert.ToInt32(reader["nooflocation"]);
                        oorganisation.noofusers = reader["noofusers"] == DBNull.Value ? 0 : Convert.ToInt32(reader["noofusers"]);
                        oorganisation.lastpayment = reader["lastpayment"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["lastpayment"]);
                        oorganisation.subscriptionfees = reader["subscriptionfees"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["subscriptionfees"]);
                        oorganisation.remarks = reader["remarks"] == DBNull.Value ? "" : reader["remarks"].ToString();

                        oorganisation.dbname = reader["dbname"] == DBNull.Value ? "" : reader["dbname"].ToString();
                        oorganisation.dbserver = reader["dbserver"] == DBNull.Value ? "" : reader["dbserver"].ToString();
                        oorganisation.accountlockedon = reader["nextdue"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["nextdue"]);
                        oorganisation.accountlock = reader["accountlock"] == DBNull.Value ? false : Convert.ToBoolean(reader["accountlock"]);
                        oorganisation.noofcompany = reader["noofcompany"] == DBNull.Value ? 0 : Convert.ToInt32(reader["noofcompany"]);
                        oorganisation.nextdue = reader["nextdue"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["nextdue"]);

                        oorganisation.quantopassword = reader["quantopassword"] == DBNull.Value ? "" : reader["quantopassword"].ToString();
                        oorganisation.quantousername = reader["quantousername"] == DBNull.Value ? "" : reader["quantousername"].ToString();
                        oorganisation.adminpassword = reader["adminpassword"] == DBNull.Value ? "" : reader["adminpassword"].ToString();
                        oorganisation.adminusername = reader["adminusername"] == DBNull.Value ? "" : reader["adminusername"].ToString();


                        oorganisation.connectionstring = reader["connectionstring"] == DBNull.Value ? "" : reader["connectionstring"].ToString();
                        oorganisation.haspaymentdue = reader["haspaymentdue"] == DBNull.Value ? false : Convert.ToBoolean(reader["haspaymentdue"]);
                        oorganisation.modules_json = reader["modules"] == DBNull.Value ? null : reader["modules"].ToString();

                        oorganisation.appconfig_json = reader["appconfig"] == DBNull.Value ? null : reader["appconfig"].ToString();

                        oorganisation.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        oorganisation.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        oorganisation.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        oorganisation.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        oorganisation.dbstatus = reader["dbstatus"] == DBNull.Value ? 0 : Convert.ToInt32(reader["dbstatus"]);
                        oorganisation.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        oorganisation.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        lst.Add(oorganisation);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(0).Log(exp, WebAPI.Common.Logger.LogType.Error, _organisation);
            }
            return lst;
        }
        private int DuplicateValidation(Organisation oOrg)
        {
            if (string.IsNullOrEmpty(oOrg.code)) return 1;

            string _Query = @"select count(id) items from organisation where isactive=true and code=@code and
                case when @id=0 then id <> 0 else id!=@id end";
            int count = 0;
            using (NpgsqlConnection conn = CreateConnection())
            {
                conn.Open();
                NpgsqlCommand command = new NpgsqlCommand(_Query, conn);
                command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oOrg.id;
                command.Parameters.Add("code", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oOrg.code;
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        count = reader["items"] == DBNull.Value ? 0 : Convert.ToInt32(reader["items"]);
                    }
                }
                conn.Close();
            }
            return count;
        }

        public bool ValidateDB(Organisation oOrganisation)
        {
            System.Configuration.ConnectionStringSettings dbconnection = null;
            foreach (System.Configuration.ConnectionStringSettings item in System.Configuration.ConfigurationManager.ConnectionStrings)
            {
                if (item.Name.ToLower() == oOrganisation.dbserver.ToLower())
                {
                    dbconnection = item;
                    break;
                }
            }

            if (dbconnection == null)
            {
                oOrganisation.ErrorMessage = "DB Server not found in the web.config file";
                return false;
            }

            string existingdbname = null;
            using (var checkconn = new NpgsqlConnection(dbconnection.ConnectionString))
            {
                checkconn.Open();
                NpgsqlCommand command = new NpgsqlCommand("SELECT datname FROM pg_catalog.pg_database where lower(datname)=@dbname", checkconn);
                command.Parameters.Add("dbname", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oOrganisation.dbname;
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                        existingdbname = reader.GetString(0);
                }
                checkconn.Close();
            }

            if (existingdbname != null)
            {
                oOrganisation.ErrorMessage = "DB " + existingdbname + " already existings, please use different db name.";
                return false;
            }

            var connectionstring = "";
            foreach (var item in dbconnection.ConnectionString.Split(';'))
            {
                if (item.Split('=')[0].ToLower() == "database")
                {
                    connectionstring += (string.IsNullOrEmpty(connectionstring) ? "" : ";") + item.Split('=')[0] + "=" + oOrganisation.dbname;
                }
                else
                {
                    connectionstring += (string.IsNullOrEmpty(connectionstring) ? "" : ";") + item;
                }
            }
            oOrganisation.connectionstring = connectionstring;
            return true;
        }
        public Organisation Insert(Organisation oOrganisation)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                if (DuplicateValidation(oOrganisation) > 0)
                {
                    oOrganisation.ErrorMessage = "Organisaction code already exists. please check.";
                    return oOrganisation;
                }

                if (!ValidateDB(oOrganisation))
                    return oOrganisation;

                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    Insert(transaction, conn, oOrganisation);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(0).Log(exp, WebAPI.Common.Logger.LogType.Error, oOrganisation);
            }

            return oOrganisation;
        }

        public Organisation Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Organisation oOrganisation)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);


            command.Parameters.Add("dbname", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oOrganisation.dbname == null ? "" : oOrganisation.dbname;
            command.Parameters.Add("dbserver", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oOrganisation.dbserver == null ? "" : oOrganisation.dbserver;
            command.Parameters.Add("accountlockedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oOrganisation.accountlockedon;
            command.Parameters.Add("accountlock", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oOrganisation.accountlock;
            command.Parameters.Add("noofcompany", NpgsqlTypes.NpgsqlDbType.Integer).Value = oOrganisation.noofcompany;
            command.Parameters.Add("nextdue", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oOrganisation.nextdue;

            command.Parameters.Add("code", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oOrganisation.code == null ? "" : oOrganisation.code;
            command.Parameters.Add("name", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oOrganisation.name == null ? "" : oOrganisation.name;
            command.Parameters.Add("contactperson", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oOrganisation.contactperson == null ? "" : oOrganisation.contactperson;
            command.Parameters.Add("contactno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oOrganisation.contactno == null ? "" : oOrganisation.contactno;
            command.Parameters.Add("emailid", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oOrganisation.emailid == null ? "" : oOrganisation.emailid;
            command.Parameters.Add("address", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oOrganisation.address == null ? "" : oOrganisation.address;
            command.Parameters.Add("cityid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oOrganisation.cityid;
            command.Parameters.Add("stateid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oOrganisation.stateid;
            command.Parameters.Add("countryid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oOrganisation.countryid;
            command.Parameters.Add("pincode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oOrganisation.pincode == null ? "" : oOrganisation.pincode;
            command.Parameters.Add("nooflocation", NpgsqlTypes.NpgsqlDbType.Integer).Value = oOrganisation.nooflocation;
            command.Parameters.Add("noofusers", NpgsqlTypes.NpgsqlDbType.Integer).Value = oOrganisation.noofusers;
            command.Parameters.Add("lastpayment", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oOrganisation.lastpayment;
            command.Parameters.Add("subscriptionfees", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oOrganisation.subscriptionfees;
            command.Parameters.Add("remarks", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oOrganisation.remarks == null ? "" : oOrganisation.remarks;
            command.Parameters.Add("connectionstring", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oOrganisation.connectionstring == null ? "" : oOrganisation.connectionstring;
            command.Parameters.Add("haspaymentdue", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oOrganisation.haspaymentdue;
            command.Parameters.Add("modules", NpgsqlTypes.NpgsqlDbType.Json).Value = oOrganisation.modules_json == null ? "null" : oOrganisation.modules_json;
            command.Parameters.Add("appconfig", NpgsqlTypes.NpgsqlDbType.Json).Value = oOrganisation.appconfig_json == null ? "null" : oOrganisation.appconfig_json;

            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oOrganisation.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oOrganisation.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oOrganisation.modifiedby;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oOrganisation.version;
            command.Parameters.Add("dbstatus", NpgsqlTypes.NpgsqlDbType.Integer).Value = oOrganisation.dbstatus;

            command.Parameters.Add("quantopassword", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oOrganisation.quantopassword == null ? "" : oOrganisation.quantopassword;
            command.Parameters.Add("quantousername", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oOrganisation.quantousername == null ? "" : oOrganisation.quantousername;
            command.Parameters.Add("adminpassword", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oOrganisation.adminpassword == null ? "" : oOrganisation.adminpassword;
            command.Parameters.Add("adminusername", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oOrganisation.adminusername == null ? "" : oOrganisation.adminusername;

            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oOrganisation.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oOrganisation.modifiedon;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oOrganisation.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            return oOrganisation;
        }

        public Organisation Update(Organisation oOrganisation)
        {
            if (DuplicateValidation(oOrganisation) > 0)
            {
                oOrganisation.ErrorMessage = "Organisaction code already exists. please check.";
                return oOrganisation;
            }

            var orglist = Search(new OrganisationCreteria()
            {
                id = oOrganisation.id
            }).First();

            if (orglist.dbname != oOrganisation.dbname || orglist.dbserver != oOrganisation.dbserver)
            {
                System.Configuration.ConnectionStringSettings dbconnection = null;
                foreach (System.Configuration.ConnectionStringSettings item in System.Configuration.ConfigurationManager.ConnectionStrings)
                {
                    if (item.Name.ToLower() == oOrganisation.dbserver.ToLower())
                    {
                        dbconnection = item;
                        break;
                    }
                }

                if (dbconnection == null)
                {
                    throw new Exception("DB Server not found in the web.config file");
                }

                string existingdbname = null;
                using (var checkconn = new NpgsqlConnection(dbconnection.ConnectionString))
                {
                    checkconn.Open();
                    NpgsqlCommand command = new NpgsqlCommand("SELECT datname FROM pg_catalog.pg_database where lower(datname)=@dbname", checkconn);
                    command.Parameters.Add("dbname", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oOrganisation.dbname;
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                            existingdbname = reader.GetString(0);
                    }
                    checkconn.Close();
                }


                var connectionstring = "";
                foreach (var item in dbconnection.ConnectionString.Split(';'))
                {
                    if (item.Split('=')[0].ToLower() == "database")
                    {
                        connectionstring += (string.IsNullOrEmpty(connectionstring) ? "" : ";") + item.Split('=')[0] + "=" + oOrganisation.dbname;
                    }
                    else
                    {
                        connectionstring += (string.IsNullOrEmpty(connectionstring) ? "" : ";") + item;
                    }
                }
                if (string.IsNullOrEmpty(existingdbname))
                    oOrganisation.dbstatus = 0;
                oOrganisation.connectionstring = connectionstring;
            }
            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oOrganisation);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(0).Log(exp, WebAPI.Common.Logger.LogType.Error, oOrganisation);
            }

            return oOrganisation;
        }

        public Organisation Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Organisation oOrganisation)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oOrganisation.id;
            command.Parameters.Add("code", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oOrganisation.code == null ? "" : oOrganisation.code;
            command.Parameters.Add("name", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oOrganisation.name == null ? "" : oOrganisation.name;
            command.Parameters.Add("contactperson", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oOrganisation.contactperson == null ? "" : oOrganisation.contactperson;
            command.Parameters.Add("contactno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oOrganisation.contactno == null ? "" : oOrganisation.contactno;
            command.Parameters.Add("emailid", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oOrganisation.emailid == null ? "" : oOrganisation.emailid;
            command.Parameters.Add("address", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oOrganisation.address == null ? "" : oOrganisation.address;
            command.Parameters.Add("cityid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oOrganisation.cityid;
            command.Parameters.Add("stateid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oOrganisation.stateid;
            command.Parameters.Add("countryid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oOrganisation.countryid;
            command.Parameters.Add("pincode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oOrganisation.pincode == null ? "" : oOrganisation.pincode;
            command.Parameters.Add("nooflocation", NpgsqlTypes.NpgsqlDbType.Integer).Value = oOrganisation.nooflocation;
            command.Parameters.Add("noofusers", NpgsqlTypes.NpgsqlDbType.Integer).Value = oOrganisation.noofusers;
            command.Parameters.Add("lastpayment", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oOrganisation.lastpayment;
            command.Parameters.Add("subscriptionfees", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oOrganisation.subscriptionfees;
            command.Parameters.Add("remarks", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oOrganisation.remarks == null ? "" : oOrganisation.remarks;
            command.Parameters.Add("haspaymentdue", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oOrganisation.haspaymentdue;
            command.Parameters.Add("connectionstring", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oOrganisation.connectionstring == null ? "" : oOrganisation.connectionstring;
            command.Parameters.Add("modules", NpgsqlTypes.NpgsqlDbType.Json).Value = oOrganisation.modules_json == null ? "null" : oOrganisation.modules_json;
            command.Parameters.Add("appconfig", NpgsqlTypes.NpgsqlDbType.Json).Value = oOrganisation.appconfig_json == null ? "null" : oOrganisation.appconfig_json;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oOrganisation.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oOrganisation.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oOrganisation.modifiedby;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oOrganisation.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oOrganisation.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oOrganisation.modifiedon;
            command.Parameters.Add("dbname", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oOrganisation.dbname == null ? "" : oOrganisation.dbname;
            command.Parameters.Add("dbserver", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oOrganisation.dbserver == null ? "" : oOrganisation.dbserver;
            command.Parameters.Add("accountlockedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oOrganisation.accountlockedon;
            command.Parameters.Add("accountlock", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oOrganisation.accountlock;
            command.Parameters.Add("noofcompany", NpgsqlTypes.NpgsqlDbType.Integer).Value = oOrganisation.noofcompany;
            command.Parameters.Add("nextdue", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oOrganisation.nextdue;
            command.Parameters.Add("dbstatus", NpgsqlTypes.NpgsqlDbType.Integer).Value = oOrganisation.dbstatus;
            command.Parameters.Add("quantopassword", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oOrganisation.quantopassword == null ? "" : oOrganisation.quantopassword;
            command.Parameters.Add("quantousername", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oOrganisation.quantousername == null ? "" : oOrganisation.quantousername;
            command.Parameters.Add("adminpassword", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oOrganisation.adminpassword == null ? "" : oOrganisation.adminpassword;
            command.Parameters.Add("adminusername", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oOrganisation.adminusername == null ? "" : oOrganisation.adminusername;

            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oOrganisation.version = Convert.ToInt32(reader[0].ToString());
                }
            }

            return oOrganisation;
        }


        public bool Delete(Organisation oOrganisation)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oOrganisation);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(0).Log(exp, WebAPI.Common.Logger.LogType.Error, oOrganisation);
            }
            return hasSuccessCount;

        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Organisation oOrganisation)
        {
            NpgsqlCommand command = new NpgsqlCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oOrganisation.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oOrganisation.modifiedby;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oOrganisation.modifiedon;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oOrganisation.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oOrganisation.id;
            int successCount = command.ExecuteNonQuery();
            return successCount > 0;
        }

        public string RunScriptOnDatabase(ScriptRunner scriptRunner)
        {
            NpgsqlTransaction transaction = null;
            StringBuilder builder = new StringBuilder();
            try
            {
                if (scriptRunner.organizatoinid == 0)
                    return "Organization id is missing";

                var organization = Search(new OrganisationCreteria()
                {
                    id = scriptRunner.organizatoinid
                }).First();

                using (NpgsqlConnection conn = new NpgsqlConnection(organization.connectionstring))
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    foreach (var script in scriptRunner.script.Split(';'))
                    {
                        NpgsqlCommand command = new NpgsqlCommand(script, conn, transaction);
                        builder.AppendLine(script);
                        int rows = command.ExecuteNonQuery();
                        builder.AppendLine("Affected :" + rows);
                    }
                    transaction.Commit();
                    conn.Close();
                }
                return "Script succesfull updated\r\n" + builder.ToString(); ;
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(0).Log(exp, WebAPI.Common.Logger.LogType.Error, scriptRunner);
                return exp.ToString() + "\r\n" + builder.ToString();
            }
        }

        public void UpdateFullRights(Users user)
        {
            user.accessablesections = new List<InfyPOS.Models.Users.ResourcePermission>();
            user.accessiblelocation = new List<InfyPOS.Models.Users.ResourcePermission>();
            user.accessiblecompany = new List<InfyPOS.Models.Users.ResourcePermission>();
            user.accessablesections.Add(new InfyPOS.Models.Users.ResourcePermission()
            {
                id = -1,
                view = true
            });
            user.accessiblelocation.Add(new InfyPOS.Models.Users.ResourcePermission()
            {
                id = -1,
                view = true
            });
            user.accessiblecompany.Add(new InfyPOS.Models.Users.ResourcePermission()
            {
                id = -1,
                view = true
            });

            user.permissions = new List<InfyPOS.Common.Permissions>();
            foreach (var item in LicenseManager.Instance(0).BuildPermissionsets(0).Permissions)
            {
                user.permissions.Add(new InfyPOS.Common.Permissions()
                {
                    code = item.code,
                    create = true,
                    delete = true,
                    modify = true,
                    print = true,
                    view = true
                });
            }
            foreach (var item in LicenseManager.Instance(0).BuildPermissionsets(0).SpecialPermissions)
            {
                user.permissions.Add(new InfyPOS.Common.Permissions()
                {
                    code = item.code,
                    create = true,
                    delete = true,
                    modify = true,
                    print = true,
                    view = true
                });
            }
        }
        public string CreateAdminUsers(long organizationid)
        {
            StringBuilder sb = new StringBuilder();
            try
            {
                var organization = Search(new OrganisationCreteria()
                {
                    id = organizationid
                }).First();
                UsersDataService usersDataService = new UsersDataService() { Context = new UserClaim() { OrganizationID = organizationid } };
                var quantouser = usersDataService.Search(new UsersCreteria()
                {
                    username = organization.quantousername
                });
                if (quantouser.Count > 0)
                {
                    quantouser[0].password = organization.quantopassword;
                    UpdateFullRights(quantouser[0]);
                    usersDataService.Update(quantouser[0]);
                    sb.AppendLine("Quanto User Updated");
                }
                else
                {
                    var user = new InfyPOS.Models.Users();
                    user.username = organization.quantousername;
                    user.password = organization.quantopassword;
                    user.isactive = true;
                    user.version = 1;
                    user.role = 999;
                    user.createdon = DateTime.Now;
                    user.modifiedon = DateTime.Now;
                    UpdateFullRights(user);
                    usersDataService.Insert(user);
                    sb.AppendLine("Quanto User Created");
                }

                quantouser = usersDataService.Search(new UsersCreteria()
                {
                    username = organization.adminusername
                });
                if (quantouser.Count > 0)
                {
                    quantouser[0].password = organization.adminpassword;
                    UpdateFullRights(quantouser[0]);
                    usersDataService.Update(quantouser[0]);
                    sb.AppendLine("Admin User Updated");
                }
                else
                {
                    var user = new InfyPOS.Models.Users();
                    user.username = organization.adminusername;
                    user.password = organization.adminpassword;
                    user.isactive = true;
                    user.version = 1;
                    user.createdon = DateTime.Now;
                    user.modifiedon = DateTime.Now;
                    UpdateFullRights(user);
                    usersDataService.Insert(user);
                    sb.AppendLine("Admin User Created");
                }
                CompanyDataService companyDataService = new CompanyDataService() { Context = new UserClaim() { OrganizationID = organizationid } };
                var listofCompany = companyDataService.Search(new CompanyCreteria());
                if (listofCompany.Count == 0)
                {
                    companyDataService.Insert(new InfyPOS.Models.Company()
                    {
                        name = organization.name,
                        code = organization.code,
                        isactive = true
                    });
                    sb.AppendLine(organization.name+" Company Created");
                }
                return sb.ToString();
            }catch(Exception exp)
            {
                return sb.ToString()+"\r\n"+exp.Message;
            }
        }
        public string CreateDB(long organizationid)
        {
            try
            {
                var organization = Search(new OrganisationCreteria()
                {
                    id = organizationid
                }).First();

                string schemafile = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "schema.json");

                if (!System.IO.File.Exists(schemafile))
                    return ("Schema file missing on " + schemafile);

                var tables = WebAPI.Data.Utility.CreateSchema(System.IO.File.ReadAllBytes(schemafile));
                StringBuilder stringBuilder = new StringBuilder();
                foreach (var table in tables)
                    stringBuilder.AppendLine(table.CreateTableScript());

                foreach (var table in tables)
                {
                    foreach (var index in table.indexes)
                        stringBuilder.AppendLine(index.script + ";");
                }

                if (tables.Exists(ex => ex.name == "Functions"))
                {
                    var newfunctions = tables.Find(ex => ex.name == "Functions").routines;
                    foreach (var ft in newfunctions)
                    {
                        stringBuilder.AppendLine(ft.description + ";");
                    }
                }

                if (tables.Exists(ex => ex.name == "MaterialView"))
                {
                    var newfunctions = tables.Find(ex => ex.name == "MaterialView").routines;
                    foreach (var ft in newfunctions)
                    {
                        stringBuilder.AppendLine(ft.description + ";");
                    }
                }

                if (tables.Exists(ex => ex.name == "Triggers"))
                {
                    var newfunctions = tables.Find(ex => ex.name == "Triggers").triggers;
                    foreach (var ft in newfunctions)
                    {
                        stringBuilder.AppendLine(ft.create + ";");
                    }
                }

                System.Configuration.ConnectionStringSettings dbconnection = null;
                foreach (System.Configuration.ConnectionStringSettings item in System.Configuration.ConfigurationManager.ConnectionStrings)
                {
                    if (item.Name.ToLower() == organization.dbserver.ToLower())
                    {
                        dbconnection = item;
                        break;
                    }
                }

                if (dbconnection == null)
                {
                    return ("DB Server not found in the web.config file");
                }

                string existingdbname = null;
                using (var checkconn = new NpgsqlConnection(dbconnection.ConnectionString))
                {
                    checkconn.Open();
                    NpgsqlCommand command = new NpgsqlCommand("SELECT datname FROM pg_catalog.pg_database where lower(datname)=@dbname", checkconn);
                    command.Parameters.Add("dbname", NpgsqlTypes.NpgsqlDbType.Varchar).Value = organization.dbname.ToLower();
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                            existingdbname = reader.GetString(0);
                    }
                    checkconn.Close();
                }

                if (!string.IsNullOrEmpty(existingdbname))
                    return ("DB Aleady exist - " + organization.dbname + " on " + organization.dbserver);

                string createDBScript = @"CREATE DATABASE ""{0}""
    WITH
    OWNER = postgres
    ENCODING = 'UTF8'
    LC_COLLATE = 'English_United States.1252'
    LC_CTYPE = 'English_United States.1252'
    TABLESPACE = pg_default
    CONNECTION LIMIT = -1;";

                using (var conn = new NpgsqlConnection(dbconnection.ConnectionString))
                {
                    conn.Open();
                    NpgsqlCommand command = new NpgsqlCommand(string.Format(createDBScript, organization.dbname), conn);
                    int rows = command.ExecuteNonQuery();
                    conn.Close();
                }

                using (var conn = new NpgsqlConnection(organization.connectionstring))
                {
                    conn.Open();
                    NpgsqlCommand command = new NpgsqlCommand(stringBuilder.ToString(), conn);
                    int rows = command.ExecuteNonQuery();
                    conn.Close();
                }

                return "Successfully " + organization.dbname + " db created on server " + organization.dbserver+"\r\n"+
                    CreateAdminUsers(organizationid);
            }
            catch (Exception exp)
            {
                return exp.ToString();
            }
        }

    }

}
