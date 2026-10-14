using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
namespace InfyPOS.Data

{
    public class CustomerdivertDataService : SAASData
    {
        const string SQL_Search = @"select id,diverton,ccinchargeid,sectionid,customertypeid,reason,sectioninchargeid,sectionfeedback,actiontakenbyid,actionstatus,actioncomments,isactive,createdby,modifiedby,version,createdon,modifiedon,customername,mobileno from customerdivert where id=@id";

        const string SQL_Insert = @"INSERT INTO public.customerdivert(diverton,ccinchargeid,sectionid,customertypeid,reason,sectioninchargeid,sectionfeedback,actiontakenbyid,actionstatus,actioncomments,isactive,createdby,modifiedby,version,createdon,modifiedon,customername,mobileno) 
        VALUES(@diverton,@ccinchargeid,@sectionid,@customertypeid,@reason,@sectioninchargeid,@sectionfeedback,@actiontakenbyid,@actionstatus,@actioncomments,@isactive,@createdby,@modifiedby,@version,@createdon,@modifiedon,@customername,@mobileno) RETURNING id";

        const string SQL_Update = @"UPDATE public.customerdivert SET diverton=@diverton,ccinchargeid=@ccinchargeid,sectionid=@sectionid,customertypeid=@customertypeid,reason=@reason,sectioninchargeid=@sectioninchargeid,sectionfeedback=@sectionfeedback,actiontakenbyid=@actiontakenbyid,
                                    actionstatus=@actionstatus,actioncomments=@actioncomments,isactive=@isactive,createdby=@createdby,modifiedby=@modifiedby,version=@version,createdon=@createdon,modifiedon=@modifiedon,customername=@customername,mobileno=@mobileno where id=@id and version=@version RETURNING version";

        const string SQL_Delete = @"update public.customerdivert set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";

        public List<Customerdivert> Search(CustomerdivertCreteria _customerdivert)
        {
            List<Customerdivert> lst = new List<Customerdivert>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_customerdivert.id != 0)
                {
                    command = GetCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _customerdivert.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("customerdivert.isactive", true, "=");

                    if (_customerdivert.sectionid > 0)
                        qb.AddParameter("customerdivert.sectionid", _customerdivert.sectionid, "=");

                    if (_customerdivert.ccinchargeid > 0)
                        qb.AddParameter("customerdivert.ccinchargeid", _customerdivert.ccinchargeid, "=");
                    if (_customerdivert.actionstatus > 0)
                        qb.AddParameter("customerdivert.actionstatus", _customerdivert.actionstatus, "=");
                    if (_customerdivert.fromdate != DateTime.MinValue && _customerdivert.todate != DateTime.MinValue)
                    {
                        qb.AddParameter("customerdivert.diverton", "fromdate", _customerdivert.fromdate.Date, ">=");
                        qb.AddParameter("customerdivert.diverton", "todate", _customerdivert.todate.Date.AddDays(1), "<");
                    }

                    qb.SortField = "diverton desc";
                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Customerdivert ocustomerdivert = new Customerdivert();
                        ocustomerdivert.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        ocustomerdivert.diverton = reader["diverton"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["diverton"]);
                        ocustomerdivert.ccinchargeid = reader["ccinchargeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["ccinchargeid"]);
                        ocustomerdivert.sectionid = reader["sectionid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sectionid"]);
                        ocustomerdivert.customertypeid = reader["customertypeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["customertypeid"]);
                        ocustomerdivert.reason = reader["reason"] == DBNull.Value ? "" : reader["reason"].ToString();
                        ocustomerdivert.customername = reader["customername"] == DBNull.Value ? "" : reader["customername"].ToString();
                        ocustomerdivert.mobileno = reader["mobileno"] == DBNull.Value ? "" : reader["mobileno"].ToString();
                        ocustomerdivert.sectioninchargeid = reader["sectioninchargeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sectioninchargeid"]);
                        ocustomerdivert.sectionfeedback = reader["sectionfeedback"] == DBNull.Value ? "" : reader["sectionfeedback"].ToString();
                        ocustomerdivert.actiontakenbyid = reader["actiontakenbyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["actiontakenbyid"]);
                        ocustomerdivert.actionstatus = reader["actionstatus"] == DBNull.Value ? 0 : Convert.ToInt64(reader["actionstatus"]);
                        ocustomerdivert.actioncomments = reader["actioncomments"] == DBNull.Value ? "" : reader["actioncomments"].ToString();
                        ocustomerdivert.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        ocustomerdivert.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        ocustomerdivert.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        ocustomerdivert.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        ocustomerdivert.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        ocustomerdivert.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        lst.Add(ocustomerdivert);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _customerdivert);
            }
            return lst;
        }

        public Customerdivert Insert(Customerdivert ocustomerdivert)
        {
            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);

                    Insert(transaction, conn, ocustomerdivert);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, ocustomerdivert);
            }

            return ocustomerdivert;
        }

        public Customerdivert Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Customerdivert ocustomerdivert)
        {

            NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("diverton", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = ocustomerdivert.diverton;
            command.Parameters.Add("ccinchargeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = ocustomerdivert.ccinchargeid;
            command.Parameters.Add("sectionid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = ocustomerdivert.sectionid;
            command.Parameters.Add("customertypeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = ocustomerdivert.customertypeid;
            command.Parameters.Add("customertypeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = ocustomerdivert.customertypeid;
            command.Parameters.Add("reason", NpgsqlTypes.NpgsqlDbType.Varchar).Value = ocustomerdivert.reason == null ? "" : ocustomerdivert.reason;
            command.Parameters.Add("customername", NpgsqlTypes.NpgsqlDbType.Varchar).Value = ocustomerdivert.customername == null ? "" : ocustomerdivert.customername;
            command.Parameters.Add("mobileno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = ocustomerdivert.mobileno == null ? "" : ocustomerdivert.mobileno;
            command.Parameters.Add("sectioninchargeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = ocustomerdivert.sectioninchargeid;
            command.Parameters.Add("sectionfeedback", NpgsqlTypes.NpgsqlDbType.Varchar).Value = ocustomerdivert.sectionfeedback == null ? "" : ocustomerdivert.sectionfeedback;
            command.Parameters.Add("actiontakenbyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = ocustomerdivert.actiontakenbyid;
            command.Parameters.Add("actionstatus", NpgsqlTypes.NpgsqlDbType.Bigint).Value = ocustomerdivert.actionstatus;
            command.Parameters.Add("actioncomments", NpgsqlTypes.NpgsqlDbType.Varchar).Value = ocustomerdivert.actioncomments == null ? "" : ocustomerdivert.actioncomments;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = true;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = ocustomerdivert.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    ocustomerdivert.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            return ocustomerdivert;
        }

        public Customerdivert Update(Customerdivert ocustomerdivert)
        {
            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, ocustomerdivert);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, ocustomerdivert);
            }

            return ocustomerdivert;
        }

        public Customerdivert Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Customerdivert ocustomerdivert)
        {

            NpgsqlCommand command = GetCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = ocustomerdivert.id;
            command.Parameters.Add("diverton", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = ocustomerdivert.diverton;
            command.Parameters.Add("ccinchargeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = ocustomerdivert.ccinchargeid;
            command.Parameters.Add("sectionid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = ocustomerdivert.sectionid;
            command.Parameters.Add("customertypeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = ocustomerdivert.customertypeid;
            command.Parameters.Add("customertypeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = ocustomerdivert.customertypeid;
            command.Parameters.Add("reason", NpgsqlTypes.NpgsqlDbType.Varchar).Value = ocustomerdivert.reason == null ? "" : ocustomerdivert.reason;
            command.Parameters.Add("customername", NpgsqlTypes.NpgsqlDbType.Varchar).Value = ocustomerdivert.customername == null ? "" : ocustomerdivert.customername;
            command.Parameters.Add("mobileno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = ocustomerdivert.mobileno == null ? "" : ocustomerdivert.mobileno;
            command.Parameters.Add("sectioninchargeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = ocustomerdivert.sectioninchargeid;
            command.Parameters.Add("sectionfeedback", NpgsqlTypes.NpgsqlDbType.Varchar).Value = ocustomerdivert.sectionfeedback == null ? "" : ocustomerdivert.sectionfeedback;
            command.Parameters.Add("actiontakenbyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = ocustomerdivert.actiontakenbyid;
            command.Parameters.Add("actionstatus", NpgsqlTypes.NpgsqlDbType.Bigint).Value = ocustomerdivert.actionstatus;
            command.Parameters.Add("actioncomments", NpgsqlTypes.NpgsqlDbType.Varchar).Value = ocustomerdivert.actioncomments == null ? "" : ocustomerdivert.actioncomments;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = ocustomerdivert.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = ocustomerdivert.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = ocustomerdivert.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = ocustomerdivert.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    ocustomerdivert.version = Convert.ToInt32(reader[0].ToString());
                }
            }

            return ocustomerdivert;
        }


        public bool Delete(Customerdivert ocustomerdivert)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, ocustomerdivert);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, ocustomerdivert);
            }
            return hasSuccessCount;
        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Customerdivert ocustomerdivert)
        {

            NpgsqlCommand command = GetCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = ocustomerdivert.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = ocustomerdivert.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = ocustomerdivert.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

    }

}
