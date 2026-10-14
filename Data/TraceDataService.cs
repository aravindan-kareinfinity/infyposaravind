using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
namespace InfyPOS.Data

{
    public class TraceDataService : SAASData
    {
        const string SQL_Search = @"select type, loggedat, methodName, info, requesturl from trace where id=@id";


        const string SQL_Insert = @"INSERT INTO public.trace(type, loggedat, methodName, info, requesturl) VALUES(@type, @loggedat, @methodName, @info, @requesturl)";


        const string SQL_Update = @"UPDATE public.trace SET type=@type, loggedat=@loggedat, methodName=@methodName, info=@info, requesturl=@requesturl where id=@id";

        const string SQL_Delete = @"update public.trace set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";

        public List<Trace> Search(TraceCreteria _trace)
        {
            string SQL_TraceSearch = @"select type, loggedat, methodName, info, requesturl from trace where 1=1 ";

            List<Trace> lst = new List<Trace>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                QueryBuilder qb = new QueryBuilder();

                if (_trace.loggedat != DateTime.MinValue)
                    SQL_TraceSearch += " AND loggedat::date = '" + _trace.loggedat.Date.ToString("yyyy-MM-dd") + "'";

                if (_trace.fromdate != DateTime.MinValue)
                    SQL_TraceSearch += " AND loggedat::date>='" + _trace.fromdate.ToString("yyyy-MM-dd") + "'";

                if (_trace.todate != DateTime.MinValue)
                    SQL_TraceSearch += " AND loggedat::date<'" + _trace.todate.AddDays(1).ToString("yyyy-MM-dd") + "'";

                if (!string.IsNullOrEmpty(_trace.type))
                    SQL_TraceSearch += " AND upper(type) ilike '%" + WebAPI.Data.Utility.SafeSQL(_trace.type.ToUpper()) + "%'";
                if (!string.IsNullOrEmpty(_trace.info))
                    SQL_TraceSearch += " AND info::text ilike '%" + _trace.info +   "%'::text ";
                if (!string.IsNullOrEmpty(_trace.requesturl))
                    SQL_TraceSearch += " AND requesturl::text ilike '%" + _trace.requesturl + "%'::text ";
                qb.CurrentQuery = SQL_TraceSearch.Replace("methodName", "\"methodName\"");
                qb.SortField = "loggedat desc";
                qb.Limit = 250;
                command = qb.GetCommand(conn);

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Trace otrace = new Trace();
                        otrace.type = reader["type"] == DBNull.Value ? "" : reader["type"].ToString();
                        otrace.loggedat = reader["loggedat"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["loggedat"]);
                        otrace.methodName = reader["methodName"] == DBNull.Value ? null : reader["methodName"].ToString();
                        try
                        {
                            if(otrace.methodName != null && otrace.methodName.Length>5)
                                otrace.exception = Newtonsoft.Json.JsonConvert.DeserializeObject<WebAPI.Common.Logger.InfyException>(otrace.methodName).Exception;
                        }catch(Exception exp)
                        {

                        }
                        otrace.info = reader["info"] == DBNull.Value ? null : reader["info"].ToString();
                        otrace.requesturl = reader["requesturl"] == DBNull.Value ? null : reader["requesturl"].ToString();
                        lst.Add(otrace);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _trace);
            }
            return lst;
        }

        public Trace Insert(Trace oTrace)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);

                    Insert(transaction, conn, oTrace);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oTrace);
            }

            return oTrace;
        }

        public Trace Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Trace oTrace)
        {

            //NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);
            //command.Parameters.Add("type", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oTrace.type == null ? "" : oTrace.type;
            //command.Parameters.Add("loggedat", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oTrace.loggedat;
            //command.Parameters.Add("methodName", NpgsqlTypes.NpgsqlDbType.Json).Value = oTrace.methodName_json == null ? "null" : oTrace.methodName_json;
            //command.Parameters.Add("info", NpgsqlTypes.NpgsqlDbType.Json).Value = oTrace.info_json == null ? "null" : oTrace.info_json;
            //command.Parameters.Add("requesturl", NpgsqlTypes.NpgsqlDbType.Json).Value = oTrace.requesturl_json == null ? "null" : oTrace.requesturl_json;
            //command.ExecuteReader();
            return oTrace;
        }

        public Trace Update(Trace oTrace)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oTrace);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oTrace);
                
            }

            return oTrace;
        }

        public Trace Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Trace oTrace)
        {

            //NpgsqlCommand command = GetCommand(SQL_Update, conn, transaction);
            //command.Parameters.Add("type", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oTrace.type == null ? "" : oTrace.type;
            //command.Parameters.Add("loggedat", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oTrace.loggedat;
            //command.Parameters.Add("methodName", NpgsqlTypes.NpgsqlDbType.Json).Value = oTrace.methodName_json == null ? "null" : oTrace.methodName_json;
            //command.Parameters.Add("info", NpgsqlTypes.NpgsqlDbType.Json).Value = oTrace.info_json == null ? "null" : oTrace.info_json;
            //command.Parameters.Add("requesturl", NpgsqlTypes.NpgsqlDbType.Json).Value = oTrace.requesturl_json == null ? "null" : oTrace.requesturl_json;
            //command.ExecuteReader();
            return oTrace;
        }


        public bool Delete(Trace oTrace)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oTrace);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oTrace);

            }
            return hasSuccessCount;

        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Trace oTrace)
        {

            //NpgsqlCommand command = GetCommand(SQL_Delete, conn, transaction);
            //command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oTrace.isactive;
            //command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTrace.modifiedby;
            //command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oTrace.modifiedon;
            //command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oTrace.version;
            //command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTrace.id;
            //int successCount = command.ExecuteNonQuery();

            return true;
        }

    }

}
