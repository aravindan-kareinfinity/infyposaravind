using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
namespace InfyPOS.Data

{
    public class MfgworkorderitemsizesDataService : SAASData
    {
        const string SQL_Search = @"select id, mfgworkorderid, mfgworkorderitemsid, isactive, createdby, modifiedby, version, createdon, modifiedon, colourid, itemid,uid, sleeveid, sizeid, sizeindex, qty, delivered, excess, shortage, cutratio, designid,cost,wsrate,dealerprice,salerate from mfgworkorderitemsizes where id=@id";
        const string SQL_SearchAll = @"select id, mfgworkorderid, mfgworkorderitemsid, isactive, createdby, modifiedby, version, createdon, modifiedon, colourid, itemid,uid, sleeveid, sizeid, sizeindex, qty, delivered, excess, shortage, cutratio, designid,cost,wsrate,dealerprice,salerate from mfgworkorderitemsizes where mfgworkorderid=@mfgworkorderid and isactive=true";


        const string SQL_Insert = @"INSERT INTO public.mfgworkorderitemsizes(mfgworkorderid, mfgworkorderitemsid, isactive, createdby, modifiedby, version, createdon, modifiedon, colourid, itemid,uid,sleeveid, sizeid, sizeindex, qty, delivered, excess, shortage, cutratio, designid,cost,wsrate,dealerprice,salerate) 
        VALUES(@mfgworkorderid, @mfgworkorderitemsid, @isactive, @createdby, @modifiedby, @version, @createdon, @modifiedon, @colourid, @itemid,@uid, @sleeveid, @sizeid, @sizeindex, @qty, @delivered, @excess, @shortage, @cutratio, @designid,@cost,@wsrate,@dealerprice,@salerate) RETURNING id";


        const string SQL_Update = @"UPDATE public.mfgworkorderitemsizes SET mfgworkorderid=@mfgworkorderid, mfgworkorderitemsid=@mfgworkorderitemsid, modifiedby=@modifiedby, version=@version+1, modifiedon=@modifiedon, colourid=@colourid, itemid=@itemid,uid=@uid,sleeveid=@sleeveid, sizeid=@sizeid, sizeindex=@sizeindex, qty=@qty, delivered=@delivered, excess=@excess, shortage=@shortage, cutratio=@cutratio, designid=@designid where id=@id and version=@version RETURNING version";

        const string SQL_Delete = @"update public.mfgworkorderitemsizes set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";
        const string SQL_DeleteAll = @"update public.mfgworkorderitemsizes set isactive=false,modifiedby=@modifiedby,modifiedon=@modifiedon where mfgworkorderid=@mfgworkorderid";
        public List<Mfgworkorderitemsizes> Search(MfgworkorderitemsizesCreteria _mfgworkorderitemsizes)
        {
            List<Mfgworkorderitemsizes> lst = new List<Mfgworkorderitemsizes>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_mfgworkorderitemsizes.id != 0)
                {
                    command = new NpgsqlCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _mfgworkorderitemsizes.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("isactive", true, "=");

                    //if (!string.IsNullOrEmpty(_mfgworkorderitemsizes.Code))
                    //    qb.AddParameter("code", _mfgworkorderitemsizes.Code, "=");

                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Mfgworkorderitemsizes omfgworkorderitemsizes = new Mfgworkorderitemsizes();
                        omfgworkorderitemsizes.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        omfgworkorderitemsizes.mfgworkorderid = reader["mfgworkorderid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["mfgworkorderid"]);
                        omfgworkorderitemsizes.mfgworkorderitemsid = reader["mfgworkorderitemsid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["mfgworkorderitemsid"]);
                        omfgworkorderitemsizes.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        omfgworkorderitemsizes.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        omfgworkorderitemsizes.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        omfgworkorderitemsizes.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        omfgworkorderitemsizes.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        omfgworkorderitemsizes.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        omfgworkorderitemsizes.colourid = reader["colourid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["colourid"]);
                        omfgworkorderitemsizes.itemid = reader["itemid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["itemid"]);
                        omfgworkorderitemsizes.uid = reader["uid"] == DBNull.Value ? "" : Convert.ToString(reader["uid"]);
                        omfgworkorderitemsizes.sleeveid = reader["sleeveid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sleeveid"]);
                        omfgworkorderitemsizes.sizeid = reader["sizeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sizeid"]);
                        omfgworkorderitemsizes.sizeindex = reader["sizeindex"] == DBNull.Value ? 0 : Convert.ToInt32(reader["sizeindex"]);
                        omfgworkorderitemsizes.qty = reader["qty"] == DBNull.Value ? 0 : Convert.ToInt32(reader["qty"]);
                        omfgworkorderitemsizes.delivered = reader["delivered"] == DBNull.Value ? 0 : Convert.ToInt32(reader["delivered"]);
                        omfgworkorderitemsizes.excess = reader["excess"] == DBNull.Value ? 0 : Convert.ToInt32(reader["excess"]);
                        omfgworkorderitemsizes.shortage = reader["shortage"] == DBNull.Value ? 0 : Convert.ToInt32(reader["shortage"]);
                        omfgworkorderitemsizes.cutratio = reader["cutratio"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["cutratio"]);

                        omfgworkorderitemsizes.cost = reader["cost"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["cost"]);
                        omfgworkorderitemsizes.salerate = reader["salerate"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["salerate"]);
                        omfgworkorderitemsizes.wsrate = reader["wsrate"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["wsrate"]);
                        omfgworkorderitemsizes.dealerprice = reader["dealerprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["dealerprice"]);

                        omfgworkorderitemsizes.designid = reader["designid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["designid"]);
                        lst.Add(omfgworkorderitemsizes);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _mfgworkorderitemsizes);
            }
            return lst;
        }

        public List<Mfgworkorderitemsizes> SearchAll(long _mfgworkorderid)
        {
            List<Mfgworkorderitemsizes> lst = new List<Mfgworkorderitemsizes>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = new NpgsqlCommand(SQL_SearchAll, conn);
                command.Parameters.Add("mfgworkorderid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _mfgworkorderid;
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Mfgworkorderitemsizes omfgworkorderitemsizes = new Mfgworkorderitemsizes();
                        omfgworkorderitemsizes.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        omfgworkorderitemsizes.mfgworkorderid = reader["mfgworkorderid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["mfgworkorderid"]);
                        omfgworkorderitemsizes.mfgworkorderitemsid = reader["mfgworkorderitemsid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["mfgworkorderitemsid"]);
                        omfgworkorderitemsizes.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        omfgworkorderitemsizes.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        omfgworkorderitemsizes.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        omfgworkorderitemsizes.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        omfgworkorderitemsizes.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        omfgworkorderitemsizes.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        omfgworkorderitemsizes.colourid = reader["colourid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["colourid"]);
                        omfgworkorderitemsizes.itemid = reader["itemid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["itemid"]);
                        omfgworkorderitemsizes.uid = reader["uid"] == DBNull.Value ? "" : Convert.ToString(reader["uid"]);
                        omfgworkorderitemsizes.sleeveid = reader["sleeveid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sleeveid"]);
                        omfgworkorderitemsizes.sizeid = reader["sizeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sizeid"]);
                        omfgworkorderitemsizes.sizeindex = reader["sizeindex"] == DBNull.Value ? 0 : Convert.ToInt32(reader["sizeindex"]);
                        omfgworkorderitemsizes.qty = reader["qty"] == DBNull.Value ? 0 : Convert.ToInt32(reader["qty"]);
                        omfgworkorderitemsizes.delivered = reader["delivered"] == DBNull.Value ? 0 : Convert.ToInt32(reader["delivered"]);
                        omfgworkorderitemsizes.excess = reader["excess"] == DBNull.Value ? 0 : Convert.ToInt32(reader["excess"]);
                        omfgworkorderitemsizes.shortage = reader["shortage"] == DBNull.Value ? 0 : Convert.ToInt32(reader["shortage"]);
                        omfgworkorderitemsizes.cutratio = reader["cutratio"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["cutratio"]);
                        omfgworkorderitemsizes.designid = reader["designid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["designid"]);
                        omfgworkorderitemsizes.cost = reader["cost"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["cost"]);
                        omfgworkorderitemsizes.salerate = reader["salerate"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["salerate"]);
                        omfgworkorderitemsizes.wsrate = reader["wsrate"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["wsrate"]);
                        omfgworkorderitemsizes.dealerprice = reader["dealerprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["dealerprice"]);

                        lst.Add(omfgworkorderitemsizes);
                    }
                }

                List<Mfgworkorderitemsizes> removeduplicate = new List<Mfgworkorderitemsizes>();
                foreach(var gitem in lst.GroupBy(x=>new { x.mfgworkorderitemsid,x.sizeid }))
                {
                    if(gitem.Count()>1)
                    {
                        var items = gitem.ToList();
                        for (var i=1;i< items.Count; i++)
                        {
                            removeduplicate.Add(items[i]);
                        }
                    }
                }

                if (removeduplicate.Count > 0)
                {
                    command = new NpgsqlCommand("Update Mfgworkorderitemsizes set isactive=false,modifiedby=@modifiedby,modifiedon=@modifiedon where id in (" + String.Join(",", removeduplicate.ConvertAll(x=>x.id)) +")", conn);
                    command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = false;
                    command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                    command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
                    command.ExecuteNonQuery();
                    lst.RemoveAll(x => removeduplicate.Contains(x));
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _mfgworkorderid);
            }
            return lst;
        }

        public Mfgworkorderitemsizes Insert(Mfgworkorderitemsizes oMfgworkorderitemsizes)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);

                    Insert(transaction, conn, oMfgworkorderitemsizes);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oMfgworkorderitemsizes);
                throw exp;
            }
            return oMfgworkorderitemsizes;
        }

        public Mfgworkorderitemsizes Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Mfgworkorderitemsizes oMfgworkorderitemsizes)
        {
            oMfgworkorderitemsizes.isactive = true;
            oMfgworkorderitemsizes.version = 1;
            oMfgworkorderitemsizes.createdby = Context.UserID;
            oMfgworkorderitemsizes.modifiedby = Context.UserID;
            oMfgworkorderitemsizes.createdon = DateTimeNow;
            oMfgworkorderitemsizes.modifiedon = DateTimeNow;
            NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("mfgworkorderid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkorderitemsizes.mfgworkorderid;
            command.Parameters.Add("mfgworkorderitemsid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkorderitemsizes.mfgworkorderitemsid;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oMfgworkorderitemsizes.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkorderitemsizes.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkorderitemsizes.modifiedby;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgworkorderitemsizes.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgworkorderitemsizes.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgworkorderitemsizes.modifiedon;
            command.Parameters.Add("colourid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkorderitemsizes.colourid;
            command.Parameters.Add("itemid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkorderitemsizes.itemid;
            command.Parameters.Add("uid", NpgsqlTypes.NpgsqlDbType.Varchar).Value = string.IsNullOrEmpty(oMfgworkorderitemsizes.uid) ? "null" : oMfgworkorderitemsizes.uid;
            command.Parameters.Add("sleeveid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkorderitemsizes.sleeveid;
            command.Parameters.Add("sizeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkorderitemsizes.sizeid;
            command.Parameters.Add("sizeindex", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgworkorderitemsizes.sizeindex;
            command.Parameters.Add("qty", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgworkorderitemsizes.qty;
            command.Parameters.Add("delivered", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgworkorderitemsizes.delivered;
            command.Parameters.Add("excess", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgworkorderitemsizes.excess;
            command.Parameters.Add("shortage", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgworkorderitemsizes.shortage;
            command.Parameters.Add("cutratio", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgworkorderitemsizes.cutratio;

            command.Parameters.Add("cost", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgworkorderitemsizes.cost;
            command.Parameters.Add("salerate", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgworkorderitemsizes.salerate;
            command.Parameters.Add("wsrate", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgworkorderitemsizes.wsrate;
            command.Parameters.Add("dealerprice", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgworkorderitemsizes.dealerprice;

            command.Parameters.Add("designid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkorderitemsizes.designid;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oMfgworkorderitemsizes.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            return oMfgworkorderitemsizes;
        }

        public Mfgworkorderitemsizes Update(Mfgworkorderitemsizes oMfgworkorderitemsizes)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oMfgworkorderitemsizes);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oMfgworkorderitemsizes);
                throw exp;
            }

            return oMfgworkorderitemsizes;
        }

        public Mfgworkorderitemsizes Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Mfgworkorderitemsizes oMfgworkorderitemsizes)
        {

            oMfgworkorderitemsizes.modifiedby = Context.UserID;
            oMfgworkorderitemsizes.modifiedon = DateTimeNow;
            oMfgworkorderitemsizes.isactive = true;
            NpgsqlCommand command = new NpgsqlCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkorderitemsizes.id;
            command.Parameters.Add("mfgworkorderid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkorderitemsizes.mfgworkorderid;
            command.Parameters.Add("mfgworkorderitemsid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkorderitemsizes.mfgworkorderitemsid;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oMfgworkorderitemsizes.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkorderitemsizes.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkorderitemsizes.modifiedby;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgworkorderitemsizes.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgworkorderitemsizes.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgworkorderitemsizes.modifiedon;
            command.Parameters.Add("colourid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkorderitemsizes.colourid;
            command.Parameters.Add("itemid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkorderitemsizes.itemid;
            command.Parameters.Add("uid", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfgworkorderitemsizes.uid;
            command.Parameters.Add("sleeveid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkorderitemsizes.sleeveid;
            command.Parameters.Add("sizeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkorderitemsizes.sizeid;
            command.Parameters.Add("sizeindex", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgworkorderitemsizes.sizeindex;
            command.Parameters.Add("qty", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgworkorderitemsizes.qty;
            command.Parameters.Add("delivered", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgworkorderitemsizes.delivered;
            command.Parameters.Add("excess", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgworkorderitemsizes.excess;
            command.Parameters.Add("shortage", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgworkorderitemsizes.shortage;
            command.Parameters.Add("cutratio", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgworkorderitemsizes.cutratio;
            command.Parameters.Add("designid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkorderitemsizes.designid;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oMfgworkorderitemsizes.version = Convert.ToInt32(reader[0].ToString());
                }
            }

            return oMfgworkorderitemsizes;
        }

        public bool Update(NpgsqlTransaction transaction, NpgsqlConnection conn, List<Mfgworkorderitemsizes> list, long id)
        {
            foreach(var uiditem in list.GroupBy(x=>x.uid))
            {
                if(uiditem.Count()>1)
                {
                    throw new Exception(uiditem.Key + " duplicate");
                }
            }
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
                    item.mfgworkorderitemsid = id;
                    item.isactive = true;
                    Update(transaction, conn, item);
                }
                else
                {
                    item.mfgworkorderitemsid = id;
                    item.isactive = true;
                    Insert(transaction, conn, item);
                }
            }
            return true;
        }

        public bool Delete(Mfgworkorderitemsizes oMfgworkorderitemsizes)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oMfgworkorderitemsizes);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oMfgworkorderitemsizes);

                throw exp;
            }
            return hasSuccessCount;

        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Mfgworkorderitemsizes oMfgworkorderitemsizes)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oMfgworkorderitemsizes.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkorderitemsizes.modifiedby;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgworkorderitemsizes.modifiedon;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgworkorderitemsizes.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkorderitemsizes.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

        public bool DeleteAll(long mfgworkorderid, long userid, NpgsqlConnection conn, NpgsqlTransaction transaction)
        {


            int successCount = 0;
            NpgsqlCommand command = new NpgsqlCommand(SQL_DeleteAll, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = false;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = userid;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("mfgworkorderid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = mfgworkorderid;
            successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

    }

}
