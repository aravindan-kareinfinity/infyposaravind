using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
namespace InfyPOS.Data
{
    public class MfgworkordergrnitemsizesDataService : SAASData
    {
        const string SQL_Search = @"select id, mfgworkordergrnid, mfgworkorderitemsgrnid,mfgworkorderitemsizesid,mfgworkorderid,mfgworkorderitemsid, isactive, createdby, modifiedby, version, createdon, modifiedon, colourid, itemid,uid, sleeveid, sizeid, sizeindex, qty,sqty,cqty, delivered, excess, shortage, designid,cost,wsrate,dealerprice,salerate,aqty,eqty,dqty from mfgworkordergrnitemsizes where id=@id";
        const string SQL_SearchAll = @"select id, mfgworkordergrnid, mfgworkorderitemsgrnid,mfgworkorderitemsizesid,mfgworkorderid,mfgworkorderitemsid, isactive, createdby, modifiedby, version, createdon, modifiedon, colourid, itemid,uid, sleeveid, sizeid, sizeindex, qty,sqty,cqty, delivered, excess, shortage, designid,cost,wsrate,dealerprice,salerate,aqty,eqty,dqty from mfgworkordergrnitemsizes where mfgworkordergrnid=@mfgworkordergrnid";

        const string SQL_Insert = @"
update mfgworkorderinbox set inqty=mfgworkorderinbox.inqty+@qty+@sqty from mfgworkorderitems
inner join mfgworkorderitemsizes on mfgworkorderitemsizes.mfgworkorderitemsid = mfgworkorderitems.id
where mfgworkorderitemsizes.id = @mfgworkorderitemsizesid 
and mfgworkorderitems.mfgworkorderid = mfgworkorderinbox.mfgworkorderid 
and mfgworkorderinbox.mfgjoborderid=0;

Update mfgworkorderitemsizes set delivered=delivered+@qty+@sqty where id=@mfgworkorderitemsizesid;

Update mfgworkorderitems set received=received+@qty+@sqty from mfgworkorderitemsizes
where mfgworkorderitemsizes.id = @mfgworkorderitemsizesid and mfgworkorderitems.id = mfgworkorderitemsizes.mfgworkorderitemsid;

INSERT INTO public.mfgworkordergrnitemsizes(mfgworkordergrnid, mfgworkorderitemsgrnid, mfgworkorderitemsizesid,mfgworkorderid,mfgworkorderitemsid, isactive, createdby, modifiedby, version, createdon,
modifiedon, colourid, itemid,uid, sleeveid, sizeid, sizeindex, qty, delivered, excess, shortage, designid,cost,wsrate,dealerprice,salerate,sqty,cqty,aqty,eqty,dqty) 
VALUES(@mfgworkordergrnid, @mfgworkorderitemsgrnid,@mfgworkorderitemsizesid,@mfgworkorderid,@mfgworkorderitemsid, @isactive, @createdby, @modifiedby, @version, @createdon, @modifiedon, @colourid, 
@itemid,@uid, @sleeveid, @sizeid, @sizeindex, @qty, @delivered, @excess, @shortage, @designid,@cost,@wsrate,@dealerprice,@salerate,@sqty,@cqty,@aqty,@eqty,@dqty) RETURNING id";

        const string SQL_Update = @"
update mfgworkorderinbox set inqty=mfgworkorderinbox.inqty-(mfgworkorderitemsizes.qty+mfgworkordergrnitemsizes.sqty)+@qty+@sqty from mfgworkorderitems
inner join mfgworkorderitemsizes on mfgworkorderitemsizes.mfgworkorderitemsid = mfgworkorderitems.id
inner join mfgworkordergrnitemsizes on mfgworkordergrnitemsizes.mfgworkorderitemsizesid = mfgworkorderitemsizes.id
where mfgworkorderitemsizes.id = @mfgworkorderitemsizesid 
and mfgworkorderitems.mfgworkorderid = mfgworkorderinbox.mfgworkorderid
and mfgworkorderinbox.mfgjoborderid=0;

Update mfgworkorderitems set received=received-mfgworkorderitemsizes.delivered+@qty+@sqty from mfgworkorderitemsizes
where mfgworkorderitemsizes.id = @mfgworkorderitemsizesid and mfgworkorderitems.id = mfgworkorderitemsizes.mfgworkorderitemsid;

Update mfgworkorderitemsizes set delivered=mfgworkorderitemsizes.delivered-mfgworkordergrnitemsizes.qty+@qty+@sqty from mfgworkordergrnitemsizes
where mfgworkordergrnitemsizes.id=@id and mfgworkorderitemsizes.id=mfgworkorderitemsizesid;

UPDATE public.mfgworkordergrnitemsizes SET mfgworkordergrnid=@mfgworkordergrnid, 
mfgworkorderitemsgrnid=@mfgworkorderitemsgrnid,aqty=@aqty,eqty=@eqty,dqty=@dqty,
mfgworkorderid=@mfgworkorderid,mfgworkorderitemsid=@mfgworkorderitemsid,
mfgworkorderitemsizesid=@mfgworkorderitemsizesid, modifiedby=@modifiedby, 
version=@version+1, modifiedon=@modifiedon, colourid=@colourid, itemid=@itemid,uid=@uid, sleeveid=@sleeveid, sizeid=@sizeid, sizeindex=@sizeindex, qty=@qty, 
sqty=@sqty,cqty=@cqty,delivered=@delivered, excess=@excess, shortage=@shortage, designid=@designid where id=@id and version=@version RETURNING version";

        const string SQL_Delete = @"

update mfgworkorderinbox set inqty=mfgworkorderinbox.inqty-mfgworkordergrnitemsizes.qty from mfgworkorderitems
inner join mfgworkorderitemsizes on mfgworkorderitemsizes.mfgworkorderitemsid = mfgworkorderitems.id
inner join mfgworkordergrnitemsizes on mfgworkordergrnitemsizes.mfgworkorderitemsizesid = mfgworkorderitemsizes.id
where mfgworkordergrnitemsizes.id = @id 
and mfgworkorderitems.mfgworkorderid = mfgworkorderinbox.mfgworkorderid
and mfgworkorderinbox.mfgjoborderid=0;

Update mfgworkorderitems set received=received-mfgworkordergrnitemsizes.qty 
from mfgworkordergrnitemsizes
inner join mfgworkorderitemsizes on mfgworkorderitemsizes.id=mfgworkorderitemsizesid 
where mfgworkordergrnitemsizes.id=@id  
and mfgworkorderitems.id = mfgworkorderitemsizes.mfgworkorderitemsid

Update mfgworkorderitemsizes set delivered=mfgworkorderitemsizes.delivered-mfgworkordergrnitemsizes.qty 
from mfgworkordergrnitemsizes
where mfgworkordergrnitemsizes.id=@id and mfgworkorderitemsizes.id=mfgworkorderitemsizesid;

update public.mfgworkordergrnitemsizes set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";

        const string SQL_DeleteAll = @"
Update mfgworkorderitems set received=received-mfgworkordergrnitemsizes.qty from mfgworkordergrnitemsizes
inner join mfgworkorderitemsizes on mfgworkorderitemsizes.id=mfgworkorderitemsizesid
where mfgworkordergrnitemsizes.id=@mfgworkorderitemsgrnid and mfgworkorderitemsizes.id=mfgworkorderitemsizesid
and mfgworkorderitems.id = mfgworkorderitemsizes.mfgworkorderitemsid;

Update mfgworkorderitemsizes set delivered=mfgworkorderitemsizes.delivered-mfgworkordergrnitemsizes.qty from mfgworkordergrnitemsizes
where mfgworkordergrnitemsizes.id=@mfgworkorderitemsgrnid and mfgworkorderitemsizes.id=mfgworkorderitemsizesid;

update public.mfgworkordergrnitemsizes set isactive=false,modifiedby=@modifiedby,modifiedon=@modifiedon where mfgworkorderitemsgrnid=@mfgworkorderitemsgrnid";
        public List<mfgworkordergrnitemsizes> Search(MfgworkordergrnitemsizesCreteria _mfgworkordergrnitemsizes)
        {
            List<mfgworkordergrnitemsizes> lst = new List<mfgworkordergrnitemsizes>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_mfgworkordergrnitemsizes.id != 0)
                {
                    command = new NpgsqlCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _mfgworkordergrnitemsizes.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("isactive", true, "=");

                    //if (!string.IsNullOrEmpty(_mfgworkordergrnitemsizes.Code))
                    //    qb.AddParameter("code", _mfgworkordergrnitemsizes.Code, "=");

                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        mfgworkordergrnitemsizes omfgworkordergrnitemsizes = new mfgworkordergrnitemsizes();
                        omfgworkordergrnitemsizes.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        omfgworkordergrnitemsizes.mfgworkordergrnid = reader["mfgworkordergrnid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["mfgworkordergrnid"]);
                        omfgworkordergrnitemsizes.mfgworkorderitemsgrnid = reader["mfgworkorderitemsgrnid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["mfgworkorderitemsgrnid"]);
                        omfgworkordergrnitemsizes.mfgworkorderitemsizesid = reader["mfgworkorderitemsizesid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["mfgworkorderitemsizesid"]);

                        omfgworkordergrnitemsizes.mfgworkorderid = reader["mfgworkorderitemsizesid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["mfgworkorderid"]);
                        omfgworkordergrnitemsizes.mfgworkorderitemsid = reader["mfgworkorderitemsizesid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["mfgworkorderitemsid"]);

                        
                        omfgworkordergrnitemsizes.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        omfgworkordergrnitemsizes.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        omfgworkordergrnitemsizes.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        omfgworkordergrnitemsizes.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        omfgworkordergrnitemsizes.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        omfgworkordergrnitemsizes.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        omfgworkordergrnitemsizes.colourid = reader["colourid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["colourid"]);
                        omfgworkordergrnitemsizes.itemid = reader["itemid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["itemid"]);
                        omfgworkordergrnitemsizes.uid = reader["uid"] == DBNull.Value ? "" : Convert.ToString(reader["uid"]);
                        omfgworkordergrnitemsizes.sleeveid = reader["sleeveid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sleeveid"]);
                        omfgworkordergrnitemsizes.sizeid = reader["sizeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sizeid"]);
                        omfgworkordergrnitemsizes.sizeindex = reader["sizeindex"] == DBNull.Value ? 0 : Convert.ToInt32(reader["sizeindex"]);
                        omfgworkordergrnitemsizes.qty = reader["qty"] == DBNull.Value ? 0 : Convert.ToInt32(reader["qty"]);
                        omfgworkordergrnitemsizes.sqty = reader["sqty"] == DBNull.Value ? 0 : Convert.ToInt32(reader["sqty"]);
                        omfgworkordergrnitemsizes.cqty = reader["cqty"] == DBNull.Value ? 0 : Convert.ToInt32(reader["cqty"]);
                        omfgworkordergrnitemsizes.delivered = reader["delivered"] == DBNull.Value ? 0 : Convert.ToInt32(reader["delivered"]);
                        omfgworkordergrnitemsizes.excess = reader["excess"] == DBNull.Value ? 0 : Convert.ToInt32(reader["excess"]);
                        omfgworkordergrnitemsizes.shortage = reader["shortage"] == DBNull.Value ? 0 : Convert.ToInt32(reader["shortage"]);
                        omfgworkordergrnitemsizes.designid = reader["designid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["designid"]);
                        omfgworkordergrnitemsizes.cost = reader["cost"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["cost"]);
                        omfgworkordergrnitemsizes.salerate = reader["salerate"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["salerate"]);
                        omfgworkordergrnitemsizes.wsrate = reader["wsrate"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["wsrate"]);
                        omfgworkordergrnitemsizes.dealerprice = reader["dealerprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["dealerprice"]);
                        omfgworkordergrnitemsizes.eqty = reader["eqty"] == DBNull.Value ? 0 : Convert.ToInt32(reader["eqty"]);
                        omfgworkordergrnitemsizes.aqty = reader["aqty"] == DBNull.Value ? 0 : Convert.ToInt32(reader["aqty"]);
                        omfgworkordergrnitemsizes.dqty = reader["dqty"] == DBNull.Value ? 0 : Convert.ToInt32(reader["dqty"]);
                        lst.Add(omfgworkordergrnitemsizes);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _mfgworkordergrnitemsizes);
            }
            return lst;
        }

        public List<mfgworkordergrnitemsizes> SearchAll(long mfgworkordergrnid)
        {
            List<mfgworkordergrnitemsizes> lst = new List<mfgworkordergrnitemsizes>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = new NpgsqlCommand(SQL_SearchAll, conn);
                command.Parameters.Add("mfgworkordergrnid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = mfgworkordergrnid;
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        mfgworkordergrnitemsizes omfgworkordergrnitemsizes = new mfgworkordergrnitemsizes();
                        omfgworkordergrnitemsizes.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        omfgworkordergrnitemsizes.mfgworkordergrnid = reader["mfgworkordergrnid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["mfgworkordergrnid"]);
                        omfgworkordergrnitemsizes.mfgworkorderitemsgrnid = reader["mfgworkorderitemsgrnid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["mfgworkorderitemsgrnid"]);
                        omfgworkordergrnitemsizes.mfgworkorderitemsizesid = reader["mfgworkorderitemsizesid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["mfgworkorderitemsizesid"]);
                        omfgworkordergrnitemsizes.mfgworkorderid = reader["mfgworkorderitemsizesid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["mfgworkorderid"]);
                        omfgworkordergrnitemsizes.mfgworkorderitemsid = reader["mfgworkorderitemsizesid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["mfgworkorderitemsid"]);
                        omfgworkordergrnitemsizes.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        omfgworkordergrnitemsizes.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        omfgworkordergrnitemsizes.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        omfgworkordergrnitemsizes.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        omfgworkordergrnitemsizes.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        omfgworkordergrnitemsizes.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        omfgworkordergrnitemsizes.colourid = reader["colourid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["colourid"]);
                        omfgworkordergrnitemsizes.itemid = reader["itemid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["itemid"]);
                        omfgworkordergrnitemsizes.uid = reader["uid"] == DBNull.Value ? "" : Convert.ToString(reader["uid"]);
                        omfgworkordergrnitemsizes.sleeveid = reader["sleeveid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sleeveid"]);
                        omfgworkordergrnitemsizes.sizeid = reader["sizeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sizeid"]);
                        omfgworkordergrnitemsizes.sizeindex = reader["sizeindex"] == DBNull.Value ? 0 : Convert.ToInt32(reader["sizeindex"]);
                        omfgworkordergrnitemsizes.qty = reader["qty"] == DBNull.Value ? 0 : Convert.ToInt32(reader["qty"]);
                        omfgworkordergrnitemsizes.sqty = reader["sqty"] == DBNull.Value ? 0 : Convert.ToInt32(reader["sqty"]);
                        omfgworkordergrnitemsizes.cqty = reader["cqty"] == DBNull.Value ? 0 : Convert.ToInt32(reader["cqty"]);

                        omfgworkordergrnitemsizes.eqty = reader["eqty"] == DBNull.Value ? 0 : Convert.ToInt32(reader["eqty"]);
                        omfgworkordergrnitemsizes.aqty = reader["aqty"] == DBNull.Value ? 0 : Convert.ToInt32(reader["aqty"]);
                        omfgworkordergrnitemsizes.dqty = reader["dqty"] == DBNull.Value ? 0 : Convert.ToInt32(reader["dqty"]);

                        omfgworkordergrnitemsizes.delivered = reader["delivered"] == DBNull.Value ? 0 : Convert.ToInt32(reader["delivered"]);
                        omfgworkordergrnitemsizes.excess = reader["excess"] == DBNull.Value ? 0 : Convert.ToInt32(reader["excess"]);
                        omfgworkordergrnitemsizes.shortage = reader["shortage"] == DBNull.Value ? 0 : Convert.ToInt32(reader["shortage"]);
                        omfgworkordergrnitemsizes.designid = reader["designid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["designid"]);
                        omfgworkordergrnitemsizes.cost = reader["cost"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["cost"]);
                        omfgworkordergrnitemsizes.salerate = reader["salerate"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["salerate"]);
                        omfgworkordergrnitemsizes.wsrate = reader["wsrate"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["wsrate"]);
                        omfgworkordergrnitemsizes.dealerprice = reader["dealerprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["dealerprice"]);
                        lst.Add(omfgworkordergrnitemsizes);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, mfgworkordergrnid);
            }
            return lst;
        }

        public mfgworkordergrnitemsizes Insert(mfgworkordergrnitemsizes oMfgworkordergrnitemsizes)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);

                    Insert(transaction, conn, oMfgworkordergrnitemsizes);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oMfgworkordergrnitemsizes);
                throw exp;
            }

            return oMfgworkordergrnitemsizes;
        }

        public mfgworkordergrnitemsizes Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, mfgworkordergrnitemsizes oMfgworkordergrnitemsizes)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("mfgworkordergrnid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkordergrnitemsizes.mfgworkordergrnid;
            command.Parameters.Add("mfgworkorderitemsgrnid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkordergrnitemsizes.mfgworkorderitemsgrnid;
            command.Parameters.Add("mfgworkorderitemsizesid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkordergrnitemsizes.mfgworkorderitemsizesid;

            command.Parameters.Add("mfgworkorderid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkordergrnitemsizes.mfgworkorderid;
            command.Parameters.Add("mfgworkorderitemsid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkordergrnitemsizes.mfgworkorderitemsid;

            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oMfgworkordergrnitemsizes.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkordergrnitemsizes.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkordergrnitemsizes.modifiedby;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgworkordergrnitemsizes.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("colourid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkordergrnitemsizes.colourid;
            command.Parameters.Add("itemid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkordergrnitemsizes.itemid;
            command.Parameters.Add("uid", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfgworkordergrnitemsizes.uid;
            command.Parameters.Add("sleeveid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkordergrnitemsizes.sleeveid;
            command.Parameters.Add("sizeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkordergrnitemsizes.sizeid;
            command.Parameters.Add("sizeindex", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgworkordergrnitemsizes.sizeindex;
            command.Parameters.Add("qty", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgworkordergrnitemsizes.qty;

            command.Parameters.Add("sqty", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgworkordergrnitemsizes.sqty;
            command.Parameters.Add("cqty", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgworkordergrnitemsizes.cqty;

            command.Parameters.Add("eqty", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgworkordergrnitemsizes.eqty;
            command.Parameters.Add("aqty", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgworkordergrnitemsizes.aqty;
            command.Parameters.Add("dqty", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgworkordergrnitemsizes.dqty;

            command.Parameters.Add("delivered", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgworkordergrnitemsizes.delivered;
            command.Parameters.Add("excess", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgworkordergrnitemsizes.excess;
            command.Parameters.Add("shortage", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgworkordergrnitemsizes.shortage;
            command.Parameters.Add("designid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkordergrnitemsizes.designid;

            command.Parameters.Add("cost", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgworkordergrnitemsizes.cost;
            command.Parameters.Add("salerate", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgworkordergrnitemsizes.salerate;
            command.Parameters.Add("wsrate", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgworkordergrnitemsizes.wsrate;
            command.Parameters.Add("dealerprice", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgworkordergrnitemsizes.dealerprice;

            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oMfgworkordergrnitemsizes.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            return oMfgworkordergrnitemsizes;
        }

        public mfgworkordergrnitemsizes Update(mfgworkordergrnitemsizes oMfgworkordergrnitemsizes)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oMfgworkordergrnitemsizes);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oMfgworkordergrnitemsizes);
                throw exp;
            }

            return oMfgworkordergrnitemsizes;
        }

        public mfgworkordergrnitemsizes Update(NpgsqlTransaction transaction, NpgsqlConnection conn, mfgworkordergrnitemsizes oMfgworkordergrnitemsizes)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkordergrnitemsizes.id;
            command.Parameters.Add("mfgworkordergrnid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkordergrnitemsizes.mfgworkordergrnid;
            command.Parameters.Add("mfgworkorderitemsgrnid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkordergrnitemsizes.mfgworkorderitemsgrnid;
            command.Parameters.Add("mfgworkorderitemsizesid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkordergrnitemsizes.mfgworkorderitemsizesid;
            command.Parameters.Add("mfgworkorderid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkordergrnitemsizes.mfgworkorderid;
            command.Parameters.Add("mfgworkorderitemsid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkordergrnitemsizes.mfgworkorderitemsid;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oMfgworkordergrnitemsizes.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkordergrnitemsizes.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkordergrnitemsizes.modifiedby;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgworkordergrnitemsizes.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgworkordergrnitemsizes.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgworkordergrnitemsizes.modifiedon;
            command.Parameters.Add("colourid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkordergrnitemsizes.colourid;
            command.Parameters.Add("itemid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkordergrnitemsizes.itemid;
            command.Parameters.Add("uid", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfgworkordergrnitemsizes.uid;

            command.Parameters.Add("sleeveid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkordergrnitemsizes.sleeveid;
            command.Parameters.Add("sizeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkordergrnitemsizes.sizeid;
            command.Parameters.Add("sizeindex", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgworkordergrnitemsizes.sizeindex;
            command.Parameters.Add("eqty", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgworkordergrnitemsizes.eqty;
            command.Parameters.Add("aqty", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgworkordergrnitemsizes.aqty;
            command.Parameters.Add("dqty", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgworkordergrnitemsizes.dqty;
            command.Parameters.Add("qty", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgworkordergrnitemsizes.qty;
            command.Parameters.Add("sqty", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgworkordergrnitemsizes.sqty;
            command.Parameters.Add("cqty", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgworkordergrnitemsizes.cqty;
            command.Parameters.Add("delivered", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgworkordergrnitemsizes.delivered;
            command.Parameters.Add("excess", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgworkordergrnitemsizes.excess;
            command.Parameters.Add("shortage", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgworkordergrnitemsizes.shortage;
            command.Parameters.Add("designid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkordergrnitemsizes.designid;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oMfgworkordergrnitemsizes.version = Convert.ToInt32(reader[0].ToString());
                }
            }

            return oMfgworkordergrnitemsizes;
        }

        public bool Update(NpgsqlTransaction transaction, NpgsqlConnection conn, List<mfgworkordergrnitemsizes> list, long id)
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
                    item.mfgworkorderitemsgrnid = id;
                    Insert(transaction, conn, item);
                }
            }
            return true;
        }

        public bool Delete(mfgworkordergrnitemsizes oMfgworkordergrnitemsizes)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oMfgworkordergrnitemsizes);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oMfgworkordergrnitemsizes);

                throw exp;
            }
            return hasSuccessCount;

        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, mfgworkordergrnitemsizes oMfgworkordergrnitemsizes)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oMfgworkordergrnitemsizes.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkordergrnitemsizes.modifiedby;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgworkordergrnitemsizes.modifiedon;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgworkordergrnitemsizes.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkordergrnitemsizes.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

        public bool DeleteAll(long mfgworkorderitemsgrnid, long userid, NpgsqlConnection conn, NpgsqlTransaction transaction)
        {


            int successCount = 0;
            NpgsqlCommand command = new NpgsqlCommand(SQL_DeleteAll, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = false;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = userid;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("mfgworkorderitemsgrnid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = mfgworkorderitemsgrnid;
            successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

    }

}
