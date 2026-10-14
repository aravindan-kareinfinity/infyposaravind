using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
namespace InfyPOS.Data

{
    public class DiscountqueueDataService : SAASData
    {
        const string SQL_Search = @"select isactive, createdby, modifiedby, version, id, createdon, modifiedon, billid, requestedby, status, actionedby, notes, actionreason, requestedamount, requestedpercentage, 
                                    approvedamount, approvedpercentage, requestedon, actionedon,billno,hasaccepted,billamount,companyid from discountqueue 
                                    where id=@id";


        const string SQL_Insert = @"INSERT INTO public.discountqueue(isactive, createdby, modifiedby, version, createdon, modifiedon, billid, requestedby, status, actionedby, notes, actionreason, requestedamount, 
                                    requestedpercentage, approvedamount, approvedpercentage, requestedon, actionedon,billno,hasaccepted,billamount,companyid) VALUES(@isactive, @createdby, @modifiedby, @version, @createdon, @modifiedon,
                                    @billid, @requestedby, @status, @actionedby, @notes, @actionreason, @requestedamount, @requestedpercentage, @approvedamount, @approvedpercentage,
                                    @requestedon, @actionedon,@billno,@hasaccepted,@billamount,@companyid) RETURNING id";

        const string SQL_Update = @"UPDATE public.discountqueue SET modifiedby=@modifiedby, version=@version+1, modifiedon=@modifiedon, billid=@billid, hasaccepted=@hasaccepted, requestedby=@requestedby, 
status=@status, actionedby=@actionedby, notes=@notes, actionreason=@actionreason, requestedamount=@requestedamount, requestedpercentage=@requestedpercentage, approvedamount=@approvedamount, 
approvedpercentage=@approvedpercentage, requestedon=@requestedon, actionedon=@actionedon where id=@id and version=@version RETURNING version;";


        const string SQL_UpdateBIllStatus = @"UPDATE public.bill SET discountstatus=@discountstatus where id=@id";


        const string SQL_Delete = @"update public.discountqueue set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";

        public List<Discountqueue> Search(DiscountqueueCreteria _discountqueue)
        {
            List<Discountqueue> lst = new List<Discountqueue>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_discountqueue.id != 0)
                {
                    command = GetCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _discountqueue.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("isactive", true, "=");

                    if (_discountqueue.mode != DiscountqueueCreteria.RequestMode.Requester)
                    {
                        if (_discountqueue.billid > 0)
                            qb.AddParameter("billid", _discountqueue.billid, "=");

                        if (_discountqueue.status == Discountqueue.DiscountStatus.Requested)
                        {
                            qb.AddParameter("status", (int)Discountqueue.DiscountStatus.Requested, "=");
                        }

                        if (_discountqueue.requestedby > 0)
                            qb.AddParameter("requestedby", _discountqueue.requestedby, "=");
                        else
                            qb.AddParameter("actionedby", 0, "=");
                    }
                    else if (_discountqueue.mode == DiscountqueueCreteria.RequestMode.Requester)
                    {
                        qb.AddParameter("requestedby", _discountqueue.requestedby, "=");
                        qb.AddParameter("hasaccepted", false, "=");
                    }
                    else if (_discountqueue.billid > 0)
                    {
                        qb.AddParameter("billid", _discountqueue.billid, "=");
                    }
                    else
                    {
                        qb.AddParameter("status", (int)Discountqueue.DiscountStatus.Requested, "=");
                    }

                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Discountqueue odiscountqueue = new Discountqueue();
                        odiscountqueue.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        odiscountqueue.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        odiscountqueue.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        odiscountqueue.companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"]);
                        odiscountqueue.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        odiscountqueue.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        odiscountqueue.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        odiscountqueue.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        odiscountqueue.billid = reader["billid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["billid"]);
                        odiscountqueue.requestedby = reader["requestedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["requestedby"]);
                        odiscountqueue.status = reader["status"] == DBNull.Value ? Discountqueue.DiscountStatus.NoDiscount : (Discountqueue.DiscountStatus)Convert.ToInt32(reader["status"]);
                        odiscountqueue.actionedby = reader["actionedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["actionedby"]);
                        odiscountqueue.notes = reader["notes"] == DBNull.Value ? "" : reader["notes"].ToString();
                        odiscountqueue.actionreason = reader["actionreason"] == DBNull.Value ? "" : reader["actionreason"].ToString();
                        odiscountqueue.requestedamount = reader["requestedamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["requestedamount"]);
                        odiscountqueue.requestedpercentage = reader["requestedpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["requestedpercentage"]);
                        odiscountqueue.approvedamount = reader["approvedamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["approvedamount"]);
                        odiscountqueue.approvedpercentage = reader["approvedpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["approvedpercentage"]);
                        odiscountqueue.billamount = reader["billamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["billamount"]);
                        odiscountqueue.billno = reader["billno"] == DBNull.Value ? "" : reader["billno"].ToString();
                        odiscountqueue.hasaccepted = reader["hasaccepted"] == DBNull.Value ? false : Convert.ToBoolean(reader["hasaccepted"]);
                        odiscountqueue.requestedon = reader["requestedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["requestedon"]);
                        odiscountqueue.actionedon = reader["actionedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["actionedon"]);
                        lst.Add(odiscountqueue);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _discountqueue);
            }
            return lst;
        }

        public Discountqueue Insert(Discountqueue oDiscountqueue)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);

                    Insert(transaction, conn, oDiscountqueue);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oDiscountqueue);
                
                    
            }

            return oDiscountqueue;
        }

        public Discountqueue Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Discountqueue oDiscountqueue)
        {

            NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oDiscountqueue.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oDiscountqueue.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("billid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oDiscountqueue.billid;
            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oDiscountqueue.companyid;
            command.Parameters.Add("requestedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oDiscountqueue.requestedby;
            command.Parameters.Add("status", NpgsqlTypes.NpgsqlDbType.Integer).Value =(int) oDiscountqueue.status;
            command.Parameters.Add("actionedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oDiscountqueue.actionedby;
            command.Parameters.Add("notes", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oDiscountqueue.notes == null ? "" : oDiscountqueue.notes;
            command.Parameters.Add("actionreason", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oDiscountqueue.actionreason == null ? "" : oDiscountqueue.actionreason;
            command.Parameters.Add("requestedamount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oDiscountqueue.requestedamount;
            command.Parameters.Add("requestedpercentage", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oDiscountqueue.requestedpercentage;
            command.Parameters.Add("approvedamount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oDiscountqueue.approvedamount;
            command.Parameters.Add("approvedpercentage", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oDiscountqueue.billamount;
            command.Parameters.Add("billamount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oDiscountqueue.approvedpercentage;

            command.Parameters.Add("hasaccepted", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oDiscountqueue.hasaccepted;
            command.Parameters.Add("billno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oDiscountqueue.billno;

            command.Parameters.Add("requestedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oDiscountqueue.requestedon;
            command.Parameters.Add("actionedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oDiscountqueue.actionedon;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oDiscountqueue.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            return oDiscountqueue;
        }

        public Discountqueue Update(Discountqueue oDiscountqueue)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                var existing = Search(new DiscountqueueCreteria()
                {
                    id = oDiscountqueue.id
                });

                if (oDiscountqueue.hasaccepted)
                {
                    existing[0].hasaccepted = true;
                }
                else if (oDiscountqueue.status == Discountqueue.DiscountStatus.Rejected)
                {
                    existing[0].status = Discountqueue.DiscountStatus.Rejected;
                    existing[0].actionreason = oDiscountqueue.actionreason;
                }
                else if (oDiscountqueue.status == Discountqueue.DiscountStatus.Approved)
                {
                    existing[0].status = Discountqueue.DiscountStatus.Approved;
                }

                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, existing[0]);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oDiscountqueue);
                
                    
            }

            return oDiscountqueue;
        }

        public Discountqueue Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Discountqueue oDiscountqueue)
        {

            NpgsqlCommand command = GetCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("hasaccepted", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oDiscountqueue.hasaccepted;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oDiscountqueue.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oDiscountqueue.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oDiscountqueue.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oDiscountqueue.id;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oDiscountqueue.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("billid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oDiscountqueue.billid;
            command.Parameters.Add("requestedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oDiscountqueue.requestedby;
            command.Parameters.Add("status", NpgsqlTypes.NpgsqlDbType.Integer).Value = oDiscountqueue.status;
            command.Parameters.Add("actionedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oDiscountqueue.actionedby;
            command.Parameters.Add("notes", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oDiscountqueue.notes == null ? "" : oDiscountqueue.notes;
            command.Parameters.Add("actionreason", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oDiscountqueue.actionreason == null ? "" : oDiscountqueue.actionreason;
            command.Parameters.Add("requestedamount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oDiscountqueue.requestedamount;
            command.Parameters.Add("requestedpercentage", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oDiscountqueue.requestedpercentage;
            command.Parameters.Add("approvedamount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oDiscountqueue.approvedamount;
            command.Parameters.Add("approvedpercentage", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oDiscountqueue.approvedpercentage;
            command.Parameters.Add("requestedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oDiscountqueue.requestedon;
            command.Parameters.Add("actionedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oDiscountqueue.actionedon;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oDiscountqueue.version = Convert.ToInt32(reader[0].ToString());
                }
            }

            command = GetCommand(SQL_UpdateBIllStatus, conn, transaction);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oDiscountqueue.billid;
            command.Parameters.Add("discountstatus", NpgsqlTypes.NpgsqlDbType.Integer).Value = oDiscountqueue.status;
            command.ExecuteNonQuery();
            return oDiscountqueue;
        }


        public bool Delete(Discountqueue oDiscountqueue)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oDiscountqueue);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oDiscountqueue);

                
                    
            }
            return hasSuccessCount;

        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Discountqueue oDiscountqueue)
        {

            NpgsqlCommand command = GetCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oDiscountqueue.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oDiscountqueue.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oDiscountqueue.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

    }

}
