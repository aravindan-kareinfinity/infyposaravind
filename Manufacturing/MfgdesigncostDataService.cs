using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
namespace InfyPOS.Data

{
    public class MfgdesigncostDataService : BaseData
    {
        const string SQL_Search = @"select id, mfgdesignid,mfgcolourid, createdby, modifiedby, createdon, modifiedon, isactive, version, sizeid, colourid, workflowid, stageid, type, mode, bomid, categoryid, uomid, qty, amount, chargeid, sizegroupid, uniqueid from mfgdesigncost where id=@id";
        const string SQL_SearchAll = @"select id, mfgdesignid,mfgcolourid, createdby, modifiedby, createdon, modifiedon, isactive, version, sizeid, colourid, workflowid, stageid, type, mode, bomid, categoryid, uomid, qty, amount, chargeid, sizegroupid, uniqueid from mfgdesigncost where mfgdesignid=@mfgdesignid and isactive=true";


        const string SQL_Insert = @"INSERT INTO public.mfgdesigncost(mfgdesignid,mfgcolourid, createdby, modifiedby, createdon, modifiedon, isactive, version, sizeid, colourid, workflowid, stageid, type, mode, bomid, categoryid, uomid, qty, amount, chargeid, sizegroupid, uniqueid) VALUES(@mfgdesignid,@mfgcolourid, @createdby, @modifiedby, @createdon, @modifiedon, @isactive, @version, @sizeid, @colourid, @workflowid, @stageid, @type, @mode, @bomid, @categoryid, @uomid, @qty, @amount, @chargeid, @sizegroupid, @uniqueid) RETURNING id";


        const string SQL_Update = @"UPDATE public.mfgdesigncost SET mfgdesignid=@mfgdesignid, mfgcolourid=@mfgcolourid,modifiedby=@modifiedby, modifiedon=@modifiedon, version=@version+1, sizeid=@sizeid, colourid=@colourid, workflowid=@workflowid, stageid=@stageid, type=@type, mode=@mode, bomid=@bomid, categoryid=@categoryid, uomid=@uomid, qty=@qty, amount=@amount, chargeid=@chargeid, sizegroupid=@sizegroupid, uniqueid=@uniqueid where id=@id and version=@version RETURNING version";

        const string SQL_Delete = @"update public.mfgdesigncost set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";
        const string SQL_DeleteAll = @"update public.mfgdesigncost set isactive=false,modifiedby=@modifiedby,modifiedon=@modifiedon where mfgdesignid=@mfgdesignid";
        public List<Mfgdesigncost> Search(MfgdesigncostCreteria _mfgdesigncost)
        {
            List<Mfgdesigncost> lst = new List<Mfgdesigncost>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_mfgdesigncost.id != 0)
                {
                    command = new NpgsqlCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _mfgdesigncost.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("isactive", true, "=");
                    if(_mfgdesigncost.mode>0)
                        qb.AddParameter("mode", _mfgdesigncost.mode, "=");
                    if (_mfgdesigncost.mfgdesignids != null &&
                        _mfgdesigncost.mfgdesignids.Count>0)
                    {
                        qb.AddInParamer("mfgdesignid", String.Join(",", _mfgdesigncost.mfgdesignids));
                    }
                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Mfgdesigncost omfgdesigncost = new Mfgdesigncost();
                        omfgdesigncost.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        omfgdesigncost.mfgdesignid = reader["mfgdesignid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["mfgdesignid"]);
                        omfgdesigncost.mfgcolourid = reader["mfgcolourid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["mfgcolourid"]);
                        omfgdesigncost.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        omfgdesigncost.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        omfgdesigncost.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        omfgdesigncost.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        omfgdesigncost.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        omfgdesigncost.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        omfgdesigncost.sizeid = reader["sizeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sizeid"]);
                        omfgdesigncost.colourid = reader["colourid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["colourid"]);
                        omfgdesigncost.workflowid = reader["workflowid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["workflowid"]);
                        omfgdesigncost.stageid = reader["stageid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["stageid"]);
                        omfgdesigncost.type = reader["type"] == DBNull.Value ? 0 : Convert.ToInt32(reader["type"]);
                        omfgdesigncost.mode = reader["mode"] == DBNull.Value ? 0 : Convert.ToInt32(reader["mode"]);
                        omfgdesigncost.bomid = reader["bomid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["bomid"]);
                        omfgdesigncost.categoryid = reader["categoryid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["categoryid"]);
                        omfgdesigncost.uomid = reader["uomid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["uomid"]);
                        omfgdesigncost.qty = reader["qty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["qty"]);
                        omfgdesigncost.amount = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"]);
                        omfgdesigncost.chargeid = reader["chargeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["chargeid"]);
                        omfgdesigncost.sizegroupid = reader["sizegroupid"] == DBNull.Value ? "" : reader["sizegroupid"].ToString();
                        omfgdesigncost.uniqueid = reader["uniqueid"] == DBNull.Value ? "" : reader["uniqueid"].ToString();
                        lst.Add(omfgdesigncost);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _mfgdesigncost);
            }
            return lst;
        }

        public List<Mfgdesigncost> SearchAll(long _mfgdesignid)
        {
            List<Mfgdesigncost> lst = new List<Mfgdesigncost>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = new NpgsqlCommand(SQL_SearchAll, conn);
                command.Parameters.Add("mfgdesignid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _mfgdesignid;
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Mfgdesigncost omfgdesigncost = new Mfgdesigncost();
                        omfgdesigncost.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        omfgdesigncost.mfgdesignid = reader["mfgdesignid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["mfgdesignid"]);
                        omfgdesigncost.mfgcolourid = reader["mfgcolourid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["mfgcolourid"]);
                        omfgdesigncost.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        omfgdesigncost.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        omfgdesigncost.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        omfgdesigncost.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        omfgdesigncost.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        omfgdesigncost.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        omfgdesigncost.sizeid = reader["sizeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sizeid"]);
                        omfgdesigncost.colourid = reader["colourid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["colourid"]);
                        omfgdesigncost.workflowid = reader["workflowid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["workflowid"]);
                        omfgdesigncost.stageid = reader["stageid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["stageid"]);
                        omfgdesigncost.type = reader["type"] == DBNull.Value ? 0 : Convert.ToInt32(reader["type"]);
                        omfgdesigncost.mode = reader["mode"] == DBNull.Value ? 0 : Convert.ToInt32(reader["mode"]);
                        omfgdesigncost.bomid = reader["bomid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["bomid"]);
                        omfgdesigncost.categoryid = reader["categoryid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["categoryid"]);
                        omfgdesigncost.uomid = reader["uomid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["uomid"]);
                        omfgdesigncost.qty = reader["qty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["qty"]);
                        omfgdesigncost.amount = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"]);
                        omfgdesigncost.chargeid = reader["chargeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["chargeid"]);
                        omfgdesigncost.sizegroupid = reader["sizegroupid"] == DBNull.Value ? "" : reader["sizegroupid"].ToString();
                        omfgdesigncost.uniqueid = reader["uniqueid"] == DBNull.Value ? "" : reader["uniqueid"].ToString();
                        lst.Add(omfgdesigncost);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _mfgdesignid);
            }
            return lst;
        }

        public List<Mfgdesigncost> Save(List<Mfgdesigncost> oMfgdesigncost)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    var existing = oMfgdesigncost.ConvertAll(x => x.id).Distinct().ToList();
                    var param = new SortedList<string, object>();
                    param.Add("mfgdesignid", oMfgdesigncost.First().mfgdesignid);
                    ExecuteQuery("update mfgdesigncost set isactive=false where mfgdesignid=@mfgdesignid and id not in (" + String.Join(",", existing)+")", param);
                    foreach(var item in oMfgdesigncost)
                    {
                        if (item.id > 0)
                            Update(transaction, conn, item);
                        else
                            Insert(transaction, conn, item);
                    }
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oMfgdesigncost);
            }

            return oMfgdesigncost;
        }

        public Mfgdesigncost Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Mfgdesigncost oMfgdesigncost)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("mfgdesignid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgdesigncost.mfgdesignid;
            command.Parameters.Add("mfgcolourid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgdesigncost.mfgcolourid;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgdesigncost.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgdesigncost.modifiedby;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgdesigncost.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgdesigncost.modifiedon;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oMfgdesigncost.isactive;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgdesigncost.version;
            command.Parameters.Add("sizeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgdesigncost.sizeid;
            command.Parameters.Add("colourid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgdesigncost.colourid;
            command.Parameters.Add("workflowid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgdesigncost.workflowid;
            command.Parameters.Add("stageid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgdesigncost.stageid;
            command.Parameters.Add("type", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgdesigncost.type;
            command.Parameters.Add("mode", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgdesigncost.mode;
            command.Parameters.Add("bomid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgdesigncost.bomid;
            command.Parameters.Add("categoryid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgdesigncost.categoryid;
            command.Parameters.Add("uomid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgdesigncost.uomid;
            command.Parameters.Add("qty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgdesigncost.qty;
            command.Parameters.Add("amount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgdesigncost.amount;
            command.Parameters.Add("chargeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgdesigncost.chargeid;
            command.Parameters.Add("sizegroupid", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfgdesigncost.sizegroupid == null ? "" : oMfgdesigncost.sizegroupid;
            command.Parameters.Add("uniqueid", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfgdesigncost.uniqueid == null ? "" : oMfgdesigncost.uniqueid;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oMfgdesigncost.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            return oMfgdesigncost;
        }

        public Mfgdesigncost Update(Mfgdesigncost oMfgdesigncost)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oMfgdesigncost);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oMfgdesigncost);
            }

            return oMfgdesigncost;
        }

        public Mfgdesigncost Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Mfgdesigncost oMfgdesigncost)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgdesigncost.id;
            command.Parameters.Add("mfgdesignid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgdesigncost.mfgdesignid;
            command.Parameters.Add("mfgcolourid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgdesigncost.mfgcolourid;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgdesigncost.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgdesigncost.modifiedby;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgdesigncost.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgdesigncost.modifiedon;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oMfgdesigncost.isactive;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgdesigncost.version;
            command.Parameters.Add("sizeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgdesigncost.sizeid;
            command.Parameters.Add("colourid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgdesigncost.colourid;
            command.Parameters.Add("workflowid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgdesigncost.workflowid;
            command.Parameters.Add("stageid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgdesigncost.stageid;
            command.Parameters.Add("type", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgdesigncost.type;
            command.Parameters.Add("mode", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgdesigncost.mode;
            command.Parameters.Add("bomid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgdesigncost.bomid;
            command.Parameters.Add("categoryid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgdesigncost.categoryid;
            command.Parameters.Add("uomid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgdesigncost.uomid;
            command.Parameters.Add("qty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgdesigncost.qty;
            command.Parameters.Add("amount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgdesigncost.amount;
            command.Parameters.Add("chargeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgdesigncost.chargeid;
            command.Parameters.Add("sizegroupid", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfgdesigncost.sizegroupid == null ? "" : oMfgdesigncost.sizegroupid;
            command.Parameters.Add("uniqueid", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfgdesigncost.uniqueid == null ? "" : oMfgdesigncost.uniqueid;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oMfgdesigncost.version = Convert.ToInt32(reader[0].ToString());
                }
            }

            return oMfgdesigncost;
        }

        public bool Update(NpgsqlTransaction transaction, NpgsqlConnection conn, List<Mfgdesigncost> list, long id)
        {
            var existingItems = SearchAll(id);
            if (existingItems != null && existingItems.Count > 0)
            {
                var deletedItems = existingItems.FindAll(e => !list.Exists(f => f.id == e.id));
                foreach (var item in deletedItems)
                    Delete(transaction, conn, item);
            }

            foreach (var item in list)
            {
                if (item.id > 0)
                {
                    Update(transaction, conn, item);
                }
                else
                {
                    item.mfgdesignid = id;
                    Insert(transaction, conn, item);
                }
            }
            return true;
        }

        public bool Delete(Mfgdesigncost oMfgdesigncost)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oMfgdesigncost);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oMfgdesigncost);

            }
            return hasSuccessCount;

        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Mfgdesigncost oMfgdesigncost)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oMfgdesigncost.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgdesigncost.modifiedby;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.TimestampTZ).Value = oMfgdesigncost.modifiedon;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgdesigncost.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgdesigncost.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

        public bool DeleteAll(long mfgdesignid, long userid, NpgsqlConnection conn, NpgsqlTransaction transaction)
        {

            int successCount = 0;
            NpgsqlCommand command = new NpgsqlCommand(SQL_DeleteAll, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = false;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = userid;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.TimestampTZ).Value = DateTime.Now;
            command.Parameters.Add("mfgdesignid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = mfgdesignid;
            successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

    }

}
