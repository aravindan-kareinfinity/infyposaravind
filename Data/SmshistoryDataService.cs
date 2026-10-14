using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
using System.Security.Cryptography;
using System.Text;

namespace InfyPOS.Data

{
    public class SmshistoryDataService : SAASData
    {
        const string SQL_Search = @"select scheduleid, mobileno, message, response, actionon, configjson from smshistory where id=@id";

        const string SQL_Insert = @"INSERT INTO public.smshistory(scheduleid, mobileno, message, response, actionon, configjson,hash) VALUES(@scheduleid, @mobileno, @message, @response, @actionon, @configjson,@hash)";

        public static string Hash(string value) => string.IsNullOrEmpty(value) ? value : SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(value)).Aggregate(new StringBuilder(), (sb, b) => sb.Append(b.ToString("X2"))).ToString();

        public List<Smshistory> Search(SmshistoryCreteria _smshistory)
        {
            List<Smshistory> lst = new List<Smshistory>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;

                QueryBuilder qb = new QueryBuilder();
                qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                if (!string.IsNullOrEmpty(_smshistory.mobileno))
                    qb.AddParameter("mobileno", _smshistory.mobileno, "=");

                if (_smshistory.actionon.Year>2000)
                    qb.AddParameter("actionon", _smshistory.actionon, "=");

                if (!string.IsNullOrEmpty(_smshistory.config))
                    qb.AddParameter("config", _smshistory.config, "=");

                if (_smshistory.scheduleid>0)
                    qb.AddParameter("scheduleid", _smshistory.scheduleid, "=");

                command = qb.GetCommand(conn);


                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Smshistory osmshistory = new Smshistory();
                        osmshistory.scheduleid = reader["scheduleid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["scheduleid"]);
                        osmshistory.mobileno = reader["mobileno"] == DBNull.Value ? "" : reader["mobileno"].ToString();
                        osmshistory.message = reader["message"] == DBNull.Value ? "" : reader["message"].ToString();
                        osmshistory.response = reader["response"] == DBNull.Value ? "" : reader["response"].ToString();
                        osmshistory.actionon = reader["actionon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["actionon"]);
                        osmshistory.config = reader["configjson"] == DBNull.Value ? "" : reader["configjson"].ToString();
                        lst.Add(osmshistory);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _smshistory);
            }
            return lst;
        }

        public Smshistory Insert(Smshistory oSmshistory)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);

                    Insert(transaction, conn, oSmshistory);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oSmshistory);
                throw exp;
            }

            return oSmshistory;
        }

        public bool HasSent(Smshistory oSmshistory)
        {
            var param = new SortedList<string, object>();
            param.Add("actionon", oSmshistory.actionon);
            param.Add("hash", Hash(oSmshistory.message));
            param.Add("mobileno", oSmshistory.mobileno);
            using (var reader = ExecuteReader("select hash from smshistory where actionon=@actionon and mobileno=@mobileno and hash=@hash", param))
            {
                return reader.Read();
            }
        }
        public bool Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Smshistory oSmshistory)
        {
            if (oSmshistory.HasDuplicate) return false;
            NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("scheduleid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSmshistory.scheduleid;
            command.Parameters.Add("mobileno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSmshistory.mobileno == null ? "" : oSmshistory.mobileno;
            command.Parameters.Add("message", NpgsqlTypes.NpgsqlDbType.Json).Value = string.IsNullOrEmpty(oSmshistory.message) ? "null" : Newtonsoft.Json.JsonConvert.SerializeObject(oSmshistory.message);
            command.Parameters.Add("hash", NpgsqlTypes.NpgsqlDbType.Varchar).Value = string.IsNullOrEmpty(oSmshistory.message) ? "" : Hash(oSmshistory.message);
            command.Parameters.Add("response", NpgsqlTypes.NpgsqlDbType.Json).Value = string.IsNullOrEmpty(oSmshistory.response) ? "null" : Newtonsoft.Json.JsonConvert.SerializeObject(oSmshistory.response);
            command.Parameters.Add("actionon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oSmshistory.actionon;
            command.Parameters.Add("configjson", NpgsqlTypes.NpgsqlDbType.Json).Value = oSmshistory.config == null ? "null" : Newtonsoft.Json.JsonConvert.SerializeObject(oSmshistory.config);
            return command.ExecuteNonQuery() > 0;
        }
        internal bool SendWithLog(long organizationid, string mobile, string msg, string configname)
        {
            bool responsemsg = InfyPOS.Services.SMSManager.Instance.SendSMS(organizationid, mobile, msg);
            Insert(new InfyPOS.Models.Smshistory()
            {
                response = responsemsg.ToString(),
                actionon = DateTimeNow,
                config = configname,
                mobileno = mobile,
                message = msg,
                scheduleid = 0
            });
            return true;
        }
        internal void InsertAll(List<Smshistory> smshistories)
        {
            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);
                    foreach(var item in smshistories)
                        Insert(transaction, conn, item);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, "oSmshistory");
            }

        }
    }

}
