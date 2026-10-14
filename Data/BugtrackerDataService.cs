using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
namespace InfyPOS.Data

{
    public class BugtrackerDataService : SAASData
    {
        const string SQL_Search = @"select id, companyid, locationid, typeid, priorityid, statusid, requestedby, requestedon, contactno, bugdetails, attachment, assignedby, assignedon, resolvedby, resolvedon, isactive, version, modifiedby, createdby, createdon, modifiedon, bugno, comments from bugtracker where id=@id";


        const string SQL_Insert = @"INSERT INTO public.bugtracker(companyid, locationid, typeid, priorityid, statusid, requestedby, requestedon, contactno, bugdetails, attachment, assignedby, assignedon, resolvedby, resolvedon, isactive, version, modifiedby, createdby, createdon, modifiedon, bugno, comments) VALUES(@companyid, @locationid, @typeid, @priorityid, @statusid, @requestedby, @requestedon, @contactno, @bugdetails, @attachment, @assignedby, @assignedon, @resolvedby, @resolvedon, @isactive, @version, @modifiedby, @createdby, @createdon, @modifiedon, @bugno, @comments) RETURNING id";


        const string SQL_Update = @"UPDATE public.bugtracker SET companyid=@companyid, locationid=@locationid, typeid=@typeid, priorityid=@priorityid, statusid=@statusid, requestedby=@requestedby, requestedon=@requestedon, contactno=@contactno, bugdetails=@bugdetails, attachment=@attachment, assignedby=@assignedby, assignedon=@assignedon, resolvedby=@resolvedby, resolvedon=@resolvedon, version=@version+1, modifiedby=@modifiedby, modifiedon=@modifiedon, bugno=@bugno, comments=@comments where id=@id and version=@version RETURNING version";

        const string SQL_Delete = @"update public.bugtracker set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";

        public List<Bugtracker> Search(BugtrackerCreteria _bugtracker)
        {
            List<Bugtracker> lst = new List<Bugtracker>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_bugtracker.id != 0)
                {
                    command = new NpgsqlCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _bugtracker.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("isactive", true, "=");

                    if (!string.IsNullOrEmpty(_bugtracker.bugno))
                        qb.AddParameter("bugno", _bugtracker.bugno, "=");
                    if (_bugtracker.companyid > 0)
                        qb.AddParameter("companyid", _bugtracker.companyid, "=");
                    if (_bugtracker.statusid > 0)
                        qb.AddParameter("statusid", _bugtracker.statusid, "=");
                    if (_bugtracker.priorityid > 0)
                        qb.AddParameter("priorityid", _bugtracker.priorityid, "=");
                    if (_bugtracker.typeid > 0)
                        qb.AddParameter("typeid", _bugtracker.typeid, "=");
                    if (_bugtracker.assignedby > 0)
                        qb.AddParameter("assignedby", _bugtracker.assignedby, "=");
                    if (_bugtracker.resolvedby > 0)
                        qb.AddParameter("resolvedby", _bugtracker.resolvedby, "=");
                    if (_bugtracker.fromdate != DateTime.MinValue && _bugtracker.todate != DateTime.MinValue)
                    {
                        qb.AddParameter("requestedon", "datefrom", _bugtracker.fromdate.Date, ">=");
                        qb.AddParameter("requestedon", "dateto", _bugtracker.todate.Date.AddDays(1), "<");
                    }

                    qb.SortField = "id desc";
                    qb.Limit = 1000;
                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Bugtracker obugtracker = new Bugtracker();
                        obugtracker.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        obugtracker.companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"]);
                        obugtracker.locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                        obugtracker.typeid = reader["typeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["typeid"]);
                        obugtracker.priorityid = reader["priorityid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["priorityid"]);
                        obugtracker.statusid = reader["statusid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["statusid"]);
                        obugtracker.requestedby = reader["requestedby"] == DBNull.Value ? "" : reader["requestedby"].ToString();
                        obugtracker.requestedon = reader["requestedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["requestedon"]);
                        obugtracker.contactno = reader["contactno"] == DBNull.Value ? "" : reader["contactno"].ToString();
                        obugtracker.bugdetails = reader["bugdetails"] == DBNull.Value ? "" : reader["bugdetails"].ToString();
                        obugtracker.attachment_json = reader["attachment"] == DBNull.Value ? null : reader["attachment"].ToString();
                        obugtracker.assignedby = reader["assignedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["assignedby"]);
                        obugtracker.assignedon = reader["assignedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["assignedon"]);
                        obugtracker.resolvedby = reader["resolvedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["resolvedby"]);
                        obugtracker.resolvedon = reader["resolvedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["resolvedon"]);
                        obugtracker.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        obugtracker.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        obugtracker.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        obugtracker.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        obugtracker.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        obugtracker.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        obugtracker.bugno = reader["bugno"] == DBNull.Value ? "" : reader["bugno"].ToString();
                        obugtracker.comments = reader["comments"] == DBNull.Value ? "" : reader["comments"].ToString();
                        lst.Add(obugtracker);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _bugtracker);
            }
            return lst;
        }

        public Bugtracker Insert(Bugtracker oBugtracker)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                var sequence = SequenceManager.Instance(Context.OrganizationID).Get("bugtracker.bugno", 0);
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);
                    oBugtracker.bugno = sequence.GetNextNumber(Context,conn, transaction);

                    Insert(transaction, conn, oBugtracker);
                    transaction.Commit();
                    conn.Close();

                }

                if (!string.IsNullOrEmpty(oBugtracker.contactno) && oBugtracker.contactno.Length >= 10)
                {
                    if (oBugtracker.statusid == 2)
                    {
                        string msg = string.Format(@"Dear Customer, Your Service Request no #{0} is resolved and closed. 
Thank you for contacting with us. 
- Team Quanto", oBugtracker.bugno);
                        InfyPOS.Services.SMSManager.Instance.SendSMS(0,oBugtracker.contactno, msg);

                    }
                    else
                    {
                        string msg = string.Format(@"Dear Customer, Your Service Request no is #{0}. 
Our Service Eng will contact you to sort out the problems. Thank you for contacting with us. 
- Team Quanto", oBugtracker.bugno);
                        InfyPOS.Services.SMSManager.Instance.SendSMS(0,oBugtracker.contactno, msg);
                    }
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oBugtracker);
            }

            return oBugtracker;
        }

        public Bugtracker Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Bugtracker oBugtracker)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBugtracker.companyid;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBugtracker.locationid;
            command.Parameters.Add("typeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBugtracker.typeid;
            command.Parameters.Add("priorityid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBugtracker.priorityid;
            command.Parameters.Add("statusid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBugtracker.statusid;
            command.Parameters.Add("requestedby", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBugtracker.requestedby == null ? "" : oBugtracker.requestedby;
            command.Parameters.Add("requestedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBugtracker.requestedon;
            command.Parameters.Add("contactno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBugtracker.contactno == null ? "" : oBugtracker.contactno;
            command.Parameters.Add("bugdetails", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBugtracker.bugdetails == null ? "" : oBugtracker.bugdetails;
            command.Parameters.Add("attachment", NpgsqlTypes.NpgsqlDbType.Json).Value = oBugtracker.attachment_json == null ? "null" : oBugtracker.attachment_json;
            command.Parameters.Add("assignedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBugtracker.assignedby;
            command.Parameters.Add("assignedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBugtracker.assignedon;
            command.Parameters.Add("resolvedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBugtracker.resolvedby;
            command.Parameters.Add("resolvedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBugtracker.resolvedon;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBugtracker.isactive;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBugtracker.version;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("bugno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBugtracker.bugno == null ? "" : oBugtracker.bugno;
            command.Parameters.Add("comments", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBugtracker.comments == null ? "" : oBugtracker.comments;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oBugtracker.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            return oBugtracker;
        }

        public Bugtracker Update(Bugtracker oBugtracker)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oBugtracker);
                    transaction.Commit();
                    conn.Close();
                }

                if (!string.IsNullOrEmpty(oBugtracker.contactno) && oBugtracker.contactno.Length >= 10 
                    && oBugtracker.statusid == 2)
                {
                    string msg = string.Format(@"Dear Customer, Your Service Request no #{0} is resolved and closed. 
Thank you for contacting with us. 
- Team Quanto", oBugtracker.bugno);
                    InfyPOS.Services.SMSManager.Instance.SendSMS(0,oBugtracker.contactno, msg);
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oBugtracker);
            }

            return oBugtracker;
        }

        public Bugtracker Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Bugtracker oBugtracker)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBugtracker.id;
            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBugtracker.companyid;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBugtracker.locationid;
            command.Parameters.Add("typeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBugtracker.typeid;
            command.Parameters.Add("priorityid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBugtracker.priorityid;
            command.Parameters.Add("statusid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBugtracker.statusid;
            command.Parameters.Add("requestedby", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBugtracker.requestedby == null ? "" : oBugtracker.requestedby;
            command.Parameters.Add("requestedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBugtracker.requestedon;
            command.Parameters.Add("contactno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBugtracker.contactno == null ? "" : oBugtracker.contactno;
            command.Parameters.Add("bugdetails", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBugtracker.bugdetails == null ? "" : oBugtracker.bugdetails;
            command.Parameters.Add("attachment", NpgsqlTypes.NpgsqlDbType.Json).Value = oBugtracker.attachment_json == null ? "null" : oBugtracker.attachment_json;
            command.Parameters.Add("assignedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBugtracker.assignedby;
            command.Parameters.Add("assignedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBugtracker.assignedon;
            command.Parameters.Add("resolvedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBugtracker.resolvedby;
            command.Parameters.Add("resolvedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBugtracker.resolvedon;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBugtracker.isactive;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBugtracker.version;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBugtracker.createdby;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBugtracker.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("bugno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBugtracker.bugno == null ? "" : oBugtracker.bugno;
            command.Parameters.Add("comments", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBugtracker.comments == null ? "" : oBugtracker.comments;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oBugtracker.version = Convert.ToInt32(reader[0].ToString());
                }
            }

            return oBugtracker;
        }


        public bool Delete(Bugtracker oBugtracker)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oBugtracker);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oBugtracker);
                
            }
            return hasSuccessCount;

        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Bugtracker oBugtracker)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBugtracker.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBugtracker.modifiedby;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBugtracker.modifiedon;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBugtracker.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBugtracker.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

    }

}
