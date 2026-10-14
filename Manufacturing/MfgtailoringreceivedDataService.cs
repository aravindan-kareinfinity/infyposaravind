using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
namespace InfyPOS.Data
{
    public class MfgtailoringreceivedDataService : BaseData
    {
        const string SQL_Search = @"select qty,cqty, id, mfgtailoringid, receivedon, createdby, createdon, version, modifiedby, employeeid, modifiedon, isactive, sizeid, sizename, sizegroupname from mfgtailoringreceived where id=@id";
        const string SQL_Insert = @"INSERT INTO public.mfgtailoringreceived(qty,cqty, mfgtailoringid, receivedon, createdby, createdon, version, modifiedby, employeeid, modifiedon, isactive, sizeid, sizename, sizegroupname) VALUES(@qty,@cqty, @mfgtailoringid, @receivedon, @createdby, @createdon, @version, @modifiedby, @employeeid, @modifiedon, @isactive, @sizeid, @sizename, @sizegroupname) RETURNING id";
        const string SQL_Update = @"UPDATE public.mfgtailoringreceived SET qty=@qty,cqty=@cqty, mfgtailoringid=@mfgtailoringid, receivedon=@receivedon, version=@version+1, modifiedby=@modifiedby, employeeid=@employeeid, modifiedon=@modifiedon, sizeid=@sizeid, sizename=@sizename, sizegroupname=@sizegroupname where id=@id and version=@version RETURNING version";
        const string SQL_Delete = @"update public.mfgtailoringreceived set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";

        public List<Mfgtailoringreceived> Search(MfgtailoringreceivedCreteria _mfgtailoringreceived)
        {
            List<Mfgtailoringreceived> lst = new List<Mfgtailoringreceived>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_mfgtailoringreceived.id != 0)
                {
                    command = new NpgsqlCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _mfgtailoringreceived.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("isactive", true, "=");

                    //if (!string.IsNullOrEmpty(_mfgtailoringreceived.Code))
                    //    qb.AddParameter("code", _mfgtailoringreceived.Code, "=");

                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Mfgtailoringreceived omfgtailoringreceived = new Mfgtailoringreceived();
                        omfgtailoringreceived.qty = reader["qty"] == DBNull.Value ? 0 : Convert.ToInt32(reader["qty"]);
                        omfgtailoringreceived.cqty = reader["cqty"] == DBNull.Value ? 0 : Convert.ToInt32(reader["cqty"]);
                        omfgtailoringreceived.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        omfgtailoringreceived.mfgtailoringid = reader["mfgtailoringid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["mfgtailoringid"]);
                        omfgtailoringreceived.receivedon = reader["receivedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["receivedon"]);
                        omfgtailoringreceived.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        omfgtailoringreceived.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        omfgtailoringreceived.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        omfgtailoringreceived.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        omfgtailoringreceived.employeeid = reader["employeeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["employeeid"]);
                        omfgtailoringreceived.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        omfgtailoringreceived.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        omfgtailoringreceived.sizeid = reader["sizeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sizeid"]);
                        omfgtailoringreceived.sizename = reader["sizename"] == DBNull.Value ? "" : reader["sizename"].ToString();
                        omfgtailoringreceived.sizegroupname = reader["sizegroupname"] == DBNull.Value ? "" : reader["sizegroupname"].ToString();
                        lst.Add(omfgtailoringreceived);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.UserID).Log(exp, WebAPI.Common.Logger.LogType.Error, _mfgtailoringreceived);
            }
            return lst;
        }

        public Mfgtailoringreceived Insert(Mfgtailoringreceived oMfgtailoringreceived)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);

                    Insert(transaction, conn, oMfgtailoringreceived);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.UserID).Log(exp, WebAPI.Common.Logger.LogType.Error, oMfgtailoringreceived);
            }

            return oMfgtailoringreceived;
        }

        public Mfgtailoringreceived Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Mfgtailoringreceived oMfgtailoringreceived)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("qty", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgtailoringreceived.qty;
            command.Parameters.Add("cqty", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgtailoringreceived.cqty;
            command.Parameters.Add("mfgtailoringid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgtailoringreceived.mfgtailoringid;
            command.Parameters.Add("receivedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgtailoringreceived.receivedon;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value =Context.UserID;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgtailoringreceived.version;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("employeeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgtailoringreceived.employeeid;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oMfgtailoringreceived.isactive;
            command.Parameters.Add("sizeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgtailoringreceived.sizeid;
            command.Parameters.Add("sizename", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfgtailoringreceived.sizename == null ? "" : oMfgtailoringreceived.sizename;
            command.Parameters.Add("sizegroupname", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfgtailoringreceived.sizegroupname == null ? "" : oMfgtailoringreceived.sizegroupname;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oMfgtailoringreceived.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            return oMfgtailoringreceived;
        }

        public Mfgtailoringreceived Update(Mfgtailoringreceived oMfgtailoringreceived)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oMfgtailoringreceived);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.UserID).Log(exp, WebAPI.Common.Logger.LogType.Error, oMfgtailoringreceived);
            }

            return oMfgtailoringreceived;
        }

        public Mfgtailoringreceived Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Mfgtailoringreceived oMfgtailoringreceived)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("qty", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgtailoringreceived.qty;
            command.Parameters.Add("cqty", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgtailoringreceived.cqty;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgtailoringreceived.id;
            command.Parameters.Add("mfgtailoringid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgtailoringreceived.mfgtailoringid;
            command.Parameters.Add("receivedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgtailoringreceived.receivedon;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgtailoringreceived.createdby;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgtailoringreceived.createdon;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgtailoringreceived.version;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("employeeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgtailoringreceived.employeeid;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oMfgtailoringreceived.isactive;
            command.Parameters.Add("sizeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgtailoringreceived.sizeid;
            command.Parameters.Add("sizename", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfgtailoringreceived.sizename == null ? "" : oMfgtailoringreceived.sizename;
            command.Parameters.Add("sizegroupname", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfgtailoringreceived.sizegroupname == null ? "" : oMfgtailoringreceived.sizegroupname;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oMfgtailoringreceived.version = Convert.ToInt32(reader[0].ToString());
                }
            }

            return oMfgtailoringreceived;
        }


        public bool Delete(Mfgtailoringreceived oMfgtailoringreceived)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oMfgtailoringreceived);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.UserID).Log(exp, WebAPI.Common.Logger.LogType.Error, oMfgtailoringreceived);
            }
            return hasSuccessCount;

        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Mfgtailoringreceived oMfgtailoringreceived)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oMfgtailoringreceived.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgtailoringreceived.modifiedby;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgtailoringreceived.modifiedon;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgtailoringreceived.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgtailoringreceived.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

    }

}
