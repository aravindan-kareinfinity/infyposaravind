using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
using WebAPI.Data;
using InfyPOS.Controllers;

namespace InfyPOS.Data

{
    public class SmsmessageDataService : BaseData
    {
        const string SQL_Search = @"select id, message, companyid, locationid, isactive, modifiedon, createdon, createdby, modifiedby, version, publishto, smscount, result, parameter, failedcount from smsmessage where id=@id";


        const string SQL_Insert = @"INSERT INTO public.smsmessage(message, companyid, locationid, isactive, modifiedon, createdon, createdby, modifiedby, version, publishto, smscount, result, parameter, failedcount) VALUES(@message, @companyid, @locationid, @isactive, @modifiedon, @createdon, @createdby, @modifiedby, @version, @publishto, @smscount, @result, @parameter, @failedcount) RETURNING id";

        const string SQL_UpdateResult = @"UPDATE public.smsmessage SET version=version+1, result=@result, failedcount=@failedcount, modifiedon=@modifiedon, modifiedby=@modifiedby where id=@id RETURNING version";

        const string SQL_Update = @"UPDATE public.smsmessage SET message=@message, companyid=@companyid, locationid=@locationid, modifiedon=@modifiedon, modifiedby=@modifiedby, version=@version+1, publishto=@publishto, smscount=@smscount, result=@result, parameter=@parameter, failedcount=@failedcount where id=@id and version=@version RETURNING version";

        const string SQL_Delete = @"update public.smsmessage set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";

        public List<Smsmessage> Search(SmsmessageCreteria _smsmessage)
        {
            List<Smsmessage> lst = new List<Smsmessage>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_smsmessage.id != 0)
                {
                    command = GetCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _smsmessage.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("isactive", true, "=");
                    qb.AddParameter("companyid", _smsmessage.companyid, "=");
                    qb.AddParameter("locationid", _smsmessage.locationid, "=");

                    qb.AddParameter("createdon", _smsmessage.fromdate, ">=");
                    qb.AddParameter("modifiedon", _smsmessage.todate, "<");
                    qb.SortField = "id DESC";

                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Smsmessage osmsmessage = new Smsmessage();
                        osmsmessage.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        osmsmessage.message = reader["message"] == DBNull.Value ? "" : reader["message"].ToString();
                        osmsmessage.companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"]);
                        osmsmessage.locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                        osmsmessage.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        osmsmessage.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        osmsmessage.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        osmsmessage.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        osmsmessage.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        osmsmessage.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        //osmsmessage.publishto_json = reader["publishto"] == DBNull.Value ? null : reader["publishto"].ToString();
                        osmsmessage.smscount = reader["smscount"] == DBNull.Value ? 0 : Convert.ToInt32(reader["smscount"]);
                        osmsmessage.result_json = reader["result"] == DBNull.Value ? null : reader["result"].ToString();
                        osmsmessage.parameter_json = reader["parameter"] == DBNull.Value ? null : reader["parameter"].ToString();
                        osmsmessage.failedcount = reader["failedcount"] == DBNull.Value ? 0 : Convert.ToInt32(reader["failedcount"]);
                        lst.Add(osmsmessage);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance.Log(exp, WebAPI.Common.Logger.LogType.Error, _smsmessage);
            }
            return lst;
        }

        internal bool UpdateStatus(SMSQueue queue)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                List<Publishto> lstreport = new List<Publishto>();
                int failedcount = 0;
                foreach (var item in queue.items)
                {
                    if (!string.IsNullOrEmpty(item.error))
                    {
                        failedcount++;
                        lstreport.Add(new Publishto()
                        {
                            mobileno = item.mobile,
                            error = item.error
                        });
                    }
                }
                Smsmessage oSmsmessage = new Smsmessage();
                oSmsmessage.id = queue.id;
                oSmsmessage.result = lstreport;
                oSmsmessage.failedcount = failedcount;
                oSmsmessage.modifiedby = 999;
                oSmsmessage.modifiedon = DateTimeNow;

                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = GetCommand(SQL_UpdateResult, conn, transaction);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSmsmessage.id;
                    command.Parameters.Add("result", NpgsqlTypes.NpgsqlDbType.Json).Value = oSmsmessage.result_json == null ? "null" : oSmsmessage.result_json;
                    command.Parameters.Add("failedcount", NpgsqlTypes.NpgsqlDbType.Integer).Value = oSmsmessage.failedcount;
                    command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.TimestampTZ).Value = oSmsmessage.modifiedon;
                    command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSmsmessage.modifiedby;
                    using (System.Data.IDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            oSmsmessage.version = Convert.ToInt32(reader[0].ToString());
                        }
                    }
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance.Log(exp, WebAPI.Common.Logger.LogType.Error, queue);

            }
            return true;
        }
        public string StartScheduleMessage()
        {
            try
            {
                // SMS template.
                ConfigurationDataService configdata = new ConfigurationDataService() { Context = this.Context };
                configdata.Context = this.Context;
                List<Configuration> lstconfig = configdata.Search(new ConfigurationCreteria
                {
                    isscheduled = true
                });

                foreach (Configuration config in lstconfig)
                {
                    if (config.key == "BIRTHDAYWISHES")
                    {
                        //BirthdaySMS(config);
                    }
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance.DirectLog(exp);
            }
            return "true";
        }

        public Smsmessage Insert(Smsmessage oSmsmessage)
        {
            // Default sender id
            string sendercode = "QUANTO";
            oSmsmessage.smsconfig = Models.Smsconfig.Default;
            oSmsmessage.smsconfig.url = oSmsmessage.smsconfig.url.Replace("QUANTO", sendercode);

            // SMS message count calcualtion
            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);

                    Insert(transaction, conn, oSmsmessage);
                  
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance.Log(exp, WebAPI.Common.Logger.LogType.Error, oSmsmessage);

            }
            return oSmsmessage;
        }
        
        public Smsmessage Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Smsmessage oSmsmessage)
        {

            NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);

            if (!string.IsNullOrEmpty(oSmsmessage.message) && oSmsmessage.message.Length > 2000)
                oSmsmessage.message = oSmsmessage.message.Substring(0, 1999);
            command.Parameters.Add("message", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSmsmessage.message == null ? "" : oSmsmessage.message;
            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSmsmessage.companyid;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSmsmessage.locationid;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oSmsmessage.isactive;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.TimestampTZ).Value = oSmsmessage.modifiedon;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.TimestampTZ).Value = oSmsmessage.createdon;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSmsmessage.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSmsmessage.modifiedby;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oSmsmessage.version;
            command.Parameters.Add("publishto", NpgsqlTypes.NpgsqlDbType.Json).Value = oSmsmessage.publishto_json == null ? "null" : oSmsmessage.publishto_json;
            command.Parameters.Add("smscount", NpgsqlTypes.NpgsqlDbType.Integer).Value = oSmsmessage.smscount;
            command.Parameters.Add("result", NpgsqlTypes.NpgsqlDbType.Json).Value = oSmsmessage.result_json == null ? "null" : oSmsmessage.result_json;
            command.Parameters.Add("parameter", NpgsqlTypes.NpgsqlDbType.Json).Value = oSmsmessage.parameter_json == null ? "null" : oSmsmessage.parameter_json;
            command.Parameters.Add("failedcount", NpgsqlTypes.NpgsqlDbType.Integer).Value = oSmsmessage.failedcount;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oSmsmessage.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            return oSmsmessage;
        }

        public Smsmessage Update(Smsmessage oSmsmessage)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    oSmsmessage.smscount = oSmsmessage.publishto.Count;
                    Update(transaction, conn, oSmsmessage);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance.Log(exp, WebAPI.Common.Logger.LogType.Error, oSmsmessage);

            }

            return oSmsmessage;
        }

        public Smsmessage Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Smsmessage oSmsmessage)
        {

            NpgsqlCommand command = GetCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSmsmessage.id;
            command.Parameters.Add("message", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSmsmessage.message == null ? "" : oSmsmessage.message;
            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSmsmessage.companyid;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSmsmessage.locationid;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oSmsmessage.isactive;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.TimestampTZ).Value = DateTimeNow;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.TimestampTZ).Value = oSmsmessage.createdon;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oSmsmessage.version;
            command.Parameters.Add("publishto", NpgsqlTypes.NpgsqlDbType.Json).Value = oSmsmessage.publishto_json == null ? "null" : oSmsmessage.publishto_json;
            command.Parameters.Add("smscount", NpgsqlTypes.NpgsqlDbType.Integer).Value = oSmsmessage.smscount;
            command.Parameters.Add("result", NpgsqlTypes.NpgsqlDbType.Json).Value = oSmsmessage.result_json == null ? "null" : oSmsmessage.result_json;
            command.Parameters.Add("parameter", NpgsqlTypes.NpgsqlDbType.Json).Value = oSmsmessage.parameter_json == null ? "null" : oSmsmessage.parameter_json;
            command.Parameters.Add("failedcount", NpgsqlTypes.NpgsqlDbType.Integer).Value = oSmsmessage.failedcount;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oSmsmessage.version = Convert.ToInt32(reader[0].ToString());
                }
            }

            return oSmsmessage;
        }


        public bool Delete(Smsmessage oSmsmessage)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oSmsmessage);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance.Log(exp, WebAPI.Common.Logger.LogType.Error, oSmsmessage);
            }
            return hasSuccessCount;
        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Smsmessage oSmsmessage)
        {

            NpgsqlCommand command = GetCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oSmsmessage.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.TimestampTZ).Value = DateTimeNow;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oSmsmessage.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSmsmessage.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }
    }
}
