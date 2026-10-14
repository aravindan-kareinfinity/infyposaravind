using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
using System;

namespace InfyPOS.Data
{
    public class MfgjoborderitemsDataService : SAASData
    {
        const string SQL_Search = @"select mfgjoborderitems.id, bomid, oqty, rqty,brqty,jobreasonid,jobchargeid,sqty, cqty, cost, marginpercentage,bominwardcode,bominvoiceitemid,bominvoiceitemimage,
        sizeid,mfgworkorderitemsid,mfgworkorderitemsizesid, materialtype, sellingprice, taxpercent, mfgjoborderitems.tax, amount, taxid, 
		mfgjoborderitems.isactive,sno, mfgjoborderitems.createdby, 
        mfgjoborderitems.modifiedby, mfgjoborderitems.version, mfgjoborderid, mfgjoborderitems.createdon, mfgjoborderitems.modifiedon, 
		bomcategoryid,colourid,productid,dynamicbom,typedisplay,itemdisplay,isbom,supportbominvoice,width,
        isfabric,sourcejoborderitemid,mfgjoborderitems.corestageid,mfgjoborderitems.inwardgoods,mfgjoborderitems.stageid from mfgjoborderitems
        inner join mfgjoborder on mfgjoborder.id =  mfgjoborderitems.mfgjoborderid 
        where mfgjoborderitems.id=@id";

        const string SQL_SearchAllJobworkbased = @"select mfgjoborderitems.id, bomid, oqty, rqty,brqty,jobreasonid,jobchargeid,sqty, cqty, cost, marginpercentage,bominwardcode,bominvoiceitemid,bominvoiceitemimage,sizeid,mfgworkorderitemsid,mfgworkorderitemsizesid, materialtype, 
sellingprice, taxpercent, mfgjoborderitems.tax, amount, taxid, mfgjoborderitems.isactive,sno, mfgjoborderitems.createdby, mfgjoborderitems.modifiedby, mfgjoborderitems.version, mfgjoborderid, 
mfgjoborderitems.createdon, mfgjoborderitems.modifiedon, bomcategoryid,colourid,productid,dynamicbom,typedisplay,mfgjoborder.inwardjob,
itemdisplay,isbom,supportbominvoice,width,isfabric,sourcejoborderitemid,mfgjoborderitems.corestageid,mfgjoborderitems.inwardgoods,mfgjoborder.stageid
from mfgjoborderitems
inner join mfgjoborder on mfgjoborder.id =  mfgjoborderitems.mfgjoborderid 
where mfgjoborderitems.mfgjoborderid=@mfgjoborderid";

        const string SQL_SearchAllWorkOrderbased = @"select mfgjoborderitems.id, bomid, oqty, rqty,brqty,jobreasonid,jobchargeid,sqty, cqty, cost, marginpercentage,bominwardcode,bominvoiceitemid,bominvoiceitemimage,sizeid,mfgworkorderitemsid,mfgworkorderitemsizesid, materialtype, 
sellingprice, taxpercent, mfgjoborderitems.tax, amount, taxid, mfgjoborderitems.isactive,sno, mfgjoborderitems.createdby, mfgjoborderitems.modifiedby, mfgjoborderitems.version, mfgjoborderid, 
mfgjoborderitems.createdon, mfgjoborderitems.modifiedon, bomcategoryid,colourid,productid,dynamicbom,typedisplay,mfgjoborder.inwardjob,
itemdisplay,isbom,supportbominvoice,width,isfabric,sourcejoborderitemid,mfgjoborderitems.corestageid,mfgjoborderitems.inwardgoods,mfgjoborder.stageid
from mfgjoborderitems
inner join mfgjoborder on mfgjoborder.id =  mfgjoborderitems.mfgjoborderid 
where mfgjoborderitems.mfgworkorderid=@mfgworkorderid";

        const string SQL_InsertInbox = @"update mfgworkorderinbox set inqty=mfgworkorderinbox.inqty+@rqty,brqty=@brqty,jobreasonid=@jobreasonid,jobchargeid=@jobchargeid,sqty=@sqty,outqty=mfgworkorderinbox.outqty+@oqty from mfgjoborder
        where @isbom = false and mfgjoborder.id=@mfgjoborderid and mfgjoborder.sourcejobid>0 and mfgjoborder=mfgjoborder.sourcejobid and mfgworkorderinbox.mfgworkorderid=mfgjoborder.mfgworkorderid RETURNING outqty,inqty";

        const string SQL_Insert = @"
        update mfgjoborderitems set rqty=rqty+@rqty,brqty=brqty+@brqty,cqty=cqty+@cqty where id=@sourcejoborderitemid and isbom=false;

        INSERT INTO public.mfgjoborderitems(bomid, oqty, rqty,brqty,jobreasonid,jobchargeid,sqty,cqty, cost, marginpercentage,bominwardcode,bominvoiceitemid,bominvoiceitemimage,sizeid,mfgworkorderitemsid,mfgworkorderitemsizesid, materialtype,sellingprice, taxpercent, tax, amount, taxid, isactive,sno, createdby, modifiedby, version, mfgjoborderid, createdon, modifiedon, bomcategoryid,colourid,productid,dynamicbom,typedisplay,itemdisplay,isbom,supportbominvoice,width,isfabric,sourcejoborderitemid,corestageid,stageid,inwardgoods,mfgworkorderid) 
        VALUES(@bomid, @oqty, @rqty,@brqty,@jobreasonid,@jobchargeid,@sqty, @cqty, @cost, @marginpercentage,@bominwardcode,@bominvoiceitemid,@bominvoiceitemimage,@sizeid,@mfgworkorderitemsid,@mfgworkorderitemsizesid,@materialtype , @sellingprice, @taxpercent, @tax, @amount, @taxid, @isactive,@sno, @createdby, @modifiedby, @version, @mfgjoborderid, @createdon, @modifiedon, @bomcategoryid,@colourid,@productid,@dynamicbom,@typedisplay,@itemdisplay,@isbom,@supportbominvoice,@width,@isfabric,@sourcejoborderitemid,@corestageid,@stageid,@inwardgoods,@mfgworkorderid) RETURNING id";

        const string SQL_Update = @"
        update mfgjoborderitems set rqty=mfgjoborderitems.rqty-currentjobitem.rqty+@rqty,cqty=mfgjoborderitems.cqty-currentjobitem.cqty+@cqty,brqty=mfgjoborderitems.brqty-currentjobitem.brqty+@brqty
        from mfgjoborderitems currentjobitem
        where currentjobitem.id=@id and  mfgjoborderitems.id = currentjobitem.sourcejoborderitemid;

        UPDATE public.mfgjoborderitems SET bomid=@bomid, oqty=@oqty, rqty=@rqty,brqty=@brqty,sqty=@sqty, cqty=@cqty, cost=@cost,jobreasonid=@jobreasonid,jobchargeid=@jobchargeid, marginpercentage=@marginpercentage,materialtype=@materialtype, sno=@sno,
        bominwardcode=@bominwardcode,bominvoiceitemid=@bominvoiceitemid,bominvoiceitemimage=@bominvoiceitemimage,sizeid=@sizeid,mfgworkorderitemsid=@mfgworkorderitemsid,   
        mfgworkorderitemsizesid=@mfgworkorderitemsizesid,isfabric=@isfabric,sellingprice=@sellingprice, taxpercent=@taxpercent, tax=@tax, amount=@amount, taxid=@taxid, 
        modifiedby=@modifiedby, version=@version+1, mfgjoborderid=@mfgjoborderid, modifiedon=@modifiedon,isbom=@isbom,supportbominvoice=@supportbominvoice,sourcejoborderitemid=@sourcejoborderitemid,width=@width,
        bomcategoryid=@bomcategoryid,colourid=@colourid,productid=@productid,dynamicbom=@dynamicbom,typedisplay=@typedisplay,itemdisplay=@itemdisplay where id=@id and version=@version RETURNING version";

        const string SQL_Delete = @"
        update mfgjoborderitems set rqty=mfgjoborderitems.rqty-currentjobitem.rqty,cqty=mfgjoborderitems.cqty-currentjobitem.cqty,brqty=mfgjoborderitems.brqty-currentjobitem.brqty
        from mfgjoborderitems currentjobitem
        where currentjobitem.id=@id and  mfgjoborderitems.id = currentjobitem.sourcejoborderitemid;

        update public.mfgjoborderitems set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";

        const string SQL_DeleteAll = @"
        update mfgjoborderitems set rqty=mfgjoborderitems.rqty-currentjobitem.rqty,cqty=mfgjoborderitems.cqty-currentjobitem.cqty,brqty=mfgjoborderitems.brqty-currentjobitem.brqty
        from mfgjoborderitems currentjobitem
        where currentjobitem.mfgjoborderid = @mfgjoborderid and  mfgjoborderitems.id = currentjobitem.sourcejoborderitemid;

        update public.mfgjoborderitems set isactive=false,modifiedby=@modifiedby,modifiedon=@modifiedon where mfgjoborderid=@mfgjoborderid";
        public List<Mfgjoborderitems> Search(MfgjoborderitemsCreteria _mfgjoborderitems)
        {
            List<Mfgjoborderitems> lst = new List<Mfgjoborderitems>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_mfgjoborderitems.id != 0)
                {
                    command = new NpgsqlCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _mfgjoborderitems.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("mfgjoborderitems.isactive", true, "=");

                    if (_mfgjoborderitems.corestageid > 0)
                        qb.AddParameter("mfgjoborderitems.corestageid", _mfgjoborderitems.corestageid, "=");

                    if(_mfgjoborderitems.inwardjob)
                        qb.AddParameter("mfgjoborder.inwardjob", _mfgjoborderitems.inwardjob, "=");

                    if (_mfgjoborderitems.mfgjoborderid>0)
                    {
                        qb.AddParameter("mfgjoborderitems.mfgjoborderid", _mfgjoborderitems.mfgjoborderid, "=");
                    }

                    if (_mfgjoborderitems.mfgworkorderid > 0)
                    {
                        qb.AddParameter("mfgjoborderitems.mfgworkorderid", _mfgjoborderitems.mfgworkorderid, "=");
                    }

                    if (_mfgjoborderitems.sourcejoborderitemids !=null &&
                        _mfgjoborderitems.sourcejoborderitemids.Count  > 0)
                    {
                        qb.AddInParamer("sourcejoborderitemid",string.Join(",", _mfgjoborderitems.sourcejoborderitemids));
                    }
                    //if (!string.IsNullOrEmpty(_mfgjoborderitems.Code))
                    //    qb.AddParameter("code", _mfgjoborderitems.Code, "=");

                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Mfgjoborderitems omfgjoborderitems = new Mfgjoborderitems();
                        omfgjoborderitems.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        omfgjoborderitems.bomid = reader["bomid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["bomid"]);

                        omfgjoborderitems.sizeid = reader["sizeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sizeid"]);
                        omfgjoborderitems.mfgworkorderitemsid = reader["mfgworkorderitemsid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["mfgworkorderitemsid"]);
                        omfgjoborderitems.mfgworkorderitemsizesid = reader["mfgworkorderitemsizesid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["mfgworkorderitemsizesid"]);

                        omfgjoborderitems.oqty = reader["oqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["oqty"]);
                        omfgjoborderitems.sqty = reader["sqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["sqty"]);
                        omfgjoborderitems.rqty = reader["rqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["rqty"]);
                        omfgjoborderitems.brqty = reader["brqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["brqty"]);
                        omfgjoborderitems.jobreasonid = reader["jobreasonid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["jobreasonid"]);
                        omfgjoborderitems.jobchargeid = reader["jobchargeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["jobchargeid"]);
                        omfgjoborderitems.cqty = reader["cqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["cqty"]);
                        omfgjoborderitems.cost = reader["cost"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["cost"]);
                        omfgjoborderitems.materialtype = reader["materialtype"] == DBNull.Value ? 0 : Convert.ToInt32(reader["materialtype"]);
                        
                        omfgjoborderitems.sno = reader["sno"] == DBNull.Value ? 0 : Convert.ToInt32(reader["sno"]);

                        omfgjoborderitems.bominwardcode = reader["bominwardcode"] == DBNull.Value ? "" : Convert.ToString(reader["bominwardcode"]);
                        omfgjoborderitems.bominvoiceitemimage = reader["bominvoiceitemimage"] == DBNull.Value ? "" : Convert.ToString(reader["bominvoiceitemimage"]);
                        omfgjoborderitems.bominvoiceitemid = reader["bominvoiceitemid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["bominvoiceitemid"]);

                        omfgjoborderitems.marginpercentage = reader["marginpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["marginpercentage"]);
                        omfgjoborderitems.sellingprice = reader["sellingprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["sellingprice"]);
                        omfgjoborderitems.taxpercentage = reader["taxpercent"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["taxpercent"]);
                        omfgjoborderitems.tax = reader["tax"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["tax"]);
                        omfgjoborderitems.amount = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"]);
                        omfgjoborderitems.taxid = reader["taxid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["taxid"]);
                        omfgjoborderitems.sourcejoborderitemid = reader["sourcejoborderitemid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sourcejoborderitemid"]);

                        omfgjoborderitems.corestageid = reader["corestageid"] == DBNull.Value ? 0 : Convert.ToInt32(reader["corestageid"]);
                        omfgjoborderitems.stageid = reader["stageid"] == DBNull.Value ? 0 : Convert.ToInt32(reader["stageid"]);
                        omfgjoborderitems.inwardgoods = reader["inwardgoods"] == DBNull.Value ? false : Convert.ToBoolean(reader["inwardgoods"]);
                        
                        omfgjoborderitems.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        omfgjoborderitems.isbom = reader["isbom"] == DBNull.Value ? false : Convert.ToBoolean(reader["isbom"]);
                        omfgjoborderitems.supportbominvoice = reader["supportbominvoice"] == DBNull.Value ? false : Convert.ToBoolean(reader["supportbominvoice"]);
                        omfgjoborderitems.width = reader["width"] == DBNull.Value ? "" : Convert.ToString(reader["width"]);

                        omfgjoborderitems.isfabric = reader["isfabric"] == DBNull.Value ? false : Convert.ToBoolean(reader["isfabric"]);
                        omfgjoborderitems.dynamicbom = reader["dynamicbom"] == DBNull.Value ? false : Convert.ToBoolean(reader["dynamicbom"]);
                        omfgjoborderitems.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        omfgjoborderitems.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        omfgjoborderitems.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        omfgjoborderitems.mfgjoborderid = reader["mfgjoborderid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["mfgjoborderid"]);
                        omfgjoborderitems.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        omfgjoborderitems.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        omfgjoborderitems.bomcategoryid = reader["bomcategoryid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["bomcategoryid"]);
                        omfgjoborderitems.itemdisplay = reader["itemdisplay"] == DBNull.Value ? "" : Convert.ToString(reader["itemdisplay"]);
                        omfgjoborderitems.typedisplay = reader["typedisplay"] == DBNull.Value ? "" : Convert.ToString(reader["typedisplay"]);

                        omfgjoborderitems.productid = reader["productid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productid"]);
                        omfgjoborderitems.colourid = reader["colourid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["colourid"]);

                        lst.Add(omfgjoborderitems);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _mfgjoborderitems);
            }
            return lst;
        }

        public List<Mfgjoborderitems> SearchAll(long _mfgjoborderid, long _mfgworkorderid)
        {
            List<Mfgjoborderitems> lst = new List<Mfgjoborderitems>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                NpgsqlCommand command = new NpgsqlCommand(_mfgjoborderid>0 ? SQL_SearchAllJobworkbased : SQL_SearchAllWorkOrderbased, conn);
                command.Parameters.Add("mfgjoborderid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _mfgjoborderid;
                command.Parameters.Add("mfgworkorderid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _mfgworkorderid;
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Mfgjoborderitems omfgjoborderitems = new Mfgjoborderitems();
                        omfgjoborderitems.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        omfgjoborderitems.bomid = reader["bomid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["bomid"]);
                        omfgjoborderitems.cost = reader["cost"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["cost"]);
                        omfgjoborderitems.marginpercentage = reader["marginpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["marginpercentage"]);
                        omfgjoborderitems.bominwardcode = reader["bominwardcode"] == DBNull.Value ? "" : Convert.ToString(reader["bominwardcode"]);
                        omfgjoborderitems.sizeid = reader["sizeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sizeid"]);
                        omfgjoborderitems.mfgworkorderitemsid = reader["mfgworkorderitemsid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["mfgworkorderitemsid"]);
                        omfgjoborderitems.mfgworkorderitemsizesid = reader["mfgworkorderitemsizesid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["mfgworkorderitemsizesid"]);
                        omfgjoborderitems.bominvoiceitemimage = reader["bominvoiceitemimage"] == DBNull.Value ? "" : Convert.ToString(reader["bominvoiceitemimage"]);
                        omfgjoborderitems.bominvoiceitemid = reader["bominvoiceitemid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["bominvoiceitemid"]);
                        omfgjoborderitems.jobreasonid = reader["jobreasonid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["jobreasonid"]);
                        omfgjoborderitems.jobchargeid = reader["jobchargeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["jobchargeid"]);
                        omfgjoborderitems.materialtype = reader["materialtype"] == DBNull.Value ? 0 : Convert.ToInt32(reader["materialtype"]);
                        omfgjoborderitems.dynamicbom = reader["dynamicbom"] == DBNull.Value ? false : Convert.ToBoolean(reader["dynamicbom"]);
                        omfgjoborderitems.sellingprice = reader["sellingprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["sellingprice"]);
                        omfgjoborderitems.taxpercentage = reader["taxpercent"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["taxpercent"]);
                        omfgjoborderitems.tax = reader["tax"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["tax"]);
                        omfgjoborderitems.oqty = reader["oqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["oqty"]);
                        omfgjoborderitems.rqty = reader["rqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["rqty"]);
                        omfgjoborderitems.brqty = reader["brqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["brqty"]);
                        omfgjoborderitems.sqty = reader["sqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["sqty"]);
                        omfgjoborderitems.cqty = reader["cqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["cqty"]);
                        omfgjoborderitems.amount = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"]);
                        omfgjoborderitems.taxid = reader["taxid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["taxid"]);
                        omfgjoborderitems.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        omfgjoborderitems.isbom = reader["isbom"] == DBNull.Value ? false : Convert.ToBoolean(reader["isbom"]);
                        omfgjoborderitems.supportbominvoice = reader["supportbominvoice"] == DBNull.Value ? false : Convert.ToBoolean(reader["supportbominvoice"]);
                        omfgjoborderitems.width = reader["width"] == DBNull.Value ? "" : Convert.ToString(reader["width"]);
                        omfgjoborderitems.isfabric = reader["isfabric"] == DBNull.Value ? false : Convert.ToBoolean(reader["isfabric"]);
                        omfgjoborderitems.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        omfgjoborderitems.corestageid = reader["corestageid"] == DBNull.Value ? 0 : Convert.ToInt32(reader["corestageid"]);
                        omfgjoborderitems.stageid = reader["stageid"] == DBNull.Value ? 0 : Convert.ToInt32(reader["stageid"]);
                        omfgjoborderitems.inwardgoods = reader["inwardgoods"] == DBNull.Value ? false : Convert.ToBoolean(reader["inwardgoods"]);
                        omfgjoborderitems.inwardjob = reader["inwardjob"] == DBNull.Value ? false : Convert.ToBoolean(reader["inwardjob"]);
                        
                        omfgjoborderitems.sourcejoborderitemid = reader["sourcejoborderitemid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sourcejoborderitemid"]);
                        omfgjoborderitems.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        omfgjoborderitems.productid = reader["productid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productid"]);
                        omfgjoborderitems.colourid = reader["colourid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["colourid"]);
                        omfgjoborderitems.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        omfgjoborderitems.mfgjoborderid = reader["mfgjoborderid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["mfgjoborderid"]);
                        omfgjoborderitems.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        omfgjoborderitems.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        omfgjoborderitems.bomcategoryid = reader["bomcategoryid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["bomcategoryid"]);
                        omfgjoborderitems.sno = reader["sno"] == DBNull.Value ? 0 : Convert.ToInt32(reader["sno"]);
                        omfgjoborderitems.itemdisplay = reader["itemdisplay"] == DBNull.Value ? "" : Convert.ToString(reader["itemdisplay"]);
                        omfgjoborderitems.typedisplay = reader["typedisplay"] == DBNull.Value ? "" : Convert.ToString(reader["typedisplay"]);

                        omfgjoborderitems.productid = reader["productid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productid"]);
                        omfgjoborderitems.colourid = reader["colourid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["colourid"]);

                        lst.Add(omfgjoborderitems);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _mfgjoborderid);
            }
            return lst;
        }

        public Mfgjoborderitems Insert(Mfgjoborderitems oMfgjoborderitems)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);

                    Insert(transaction, conn, oMfgjoborderitems);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oMfgjoborderitems);
                throw exp;
            }

            return oMfgjoborderitems;
        }

        public Mfgjoborderitems Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Mfgjoborderitems oMfgjoborderitems)
        {
            if (!oMfgjoborderitems.isbom && ( oMfgjoborderitems.mfgworkorderid <= 0 || oMfgjoborderitems.mfgworkorderitemsid <= 0))
                throw new Exception("Work order item id missing.");

            NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("bomid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgjoborderitems.bomid;
            command.Parameters.Add("jobreasonid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgjoborderitems.jobreasonid;
            command.Parameters.Add("jobchargeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgjoborderitems.jobchargeid;
            command.Parameters.Add("oqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgjoborderitems.oqty;
            command.Parameters.Add("rqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgjoborderitems.rqty;
            command.Parameters.Add("brqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgjoborderitems.brqty;
            command.Parameters.Add("sqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgjoborderitems.sqty;
            command.Parameters.Add("cqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgjoborderitems.cqty;
            command.Parameters.Add("cost", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgjoborderitems.cost;
            command.Parameters.Add("materialtype", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgjoborderitems.materialtype;
            command.Parameters.Add("marginpercentage", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgjoborderitems.marginpercentage;
            command.Parameters.Add("sellingprice", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgjoborderitems.sellingprice;
            command.Parameters.Add("taxpercent", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgjoborderitems.taxpercentage;
            command.Parameters.Add("tax", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgjoborderitems.tax;
            command.Parameters.Add("amount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgjoborderitems.amount;
            command.Parameters.Add("taxid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgjoborderitems.taxid;
            command.Parameters.Add("corestageid", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgjoborderitems.corestageid;
            command.Parameters.Add("stageid", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgjoborderitems.stageid;
            command.Parameters.Add("inwardgoods", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oMfgjoborderitems.inwardgoods;
            command.Parameters.Add("sourcejoborderitemid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgjoborderitems.sourcejoborderitemid;
            command.Parameters.Add("productid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgjoborderitems.productid;
            command.Parameters.Add("colourid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgjoborderitems.colourid;
            command.Parameters.Add("isbom", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oMfgjoborderitems.isbom;
            command.Parameters.Add("supportbominvoice", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oMfgjoborderitems.supportbominvoice;
            command.Parameters.Add("width", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfgjoborderitems.width == null ? "" : oMfgjoborderitems.width;
            command.Parameters.Add("isfabric", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oMfgjoborderitems.isfabric;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oMfgjoborderitems.isactive;
            command.Parameters.Add("sno", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgjoborderitems.sno;
            command.Parameters.Add("dynamicbom", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oMfgjoborderitems.dynamicbom;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgjoborderitems.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgjoborderitems.modifiedby;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgjoborderitems.version;
            command.Parameters.Add("mfgjoborderid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgjoborderitems.mfgjoborderid;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgjoborderitems.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgjoborderitems.modifiedon;
            command.Parameters.Add("bomcategoryid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgjoborderitems.bomcategoryid;
            command.Parameters.Add("mfgworkorderid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgjoborderitems.mfgworkorderid;
            command.Parameters.Add("mfgworkorderitemsid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgjoborderitems.mfgworkorderitemsid;
            command.Parameters.Add("mfgworkorderitemsizesid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgjoborderitems.mfgworkorderitemsizesid;

            command.Parameters.Add("sizeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgjoborderitems.sizeid;

            command.Parameters.Add("typedisplay", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfgjoborderitems.typedisplay == null ? "" : oMfgjoborderitems.typedisplay;
            command.Parameters.Add("itemdisplay", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfgjoborderitems.itemdisplay == null ? "" : oMfgjoborderitems.itemdisplay;

            command.Parameters.Add("bominwardcode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfgjoborderitems.bominwardcode == null ? "" : oMfgjoborderitems.bominwardcode;
            command.Parameters.Add("bominvoiceitemimage", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfgjoborderitems.bominvoiceitemimage == null ? "" : oMfgjoborderitems.bominvoiceitemimage;
            command.Parameters.Add("bominvoiceitemid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgjoborderitems.bominvoiceitemid;

            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oMfgjoborderitems.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            return oMfgjoborderitems;
        }

        public Mfgjoborderitems Update(Mfgjoborderitems oMfgjoborderitems)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oMfgjoborderitems);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oMfgjoborderitems);
                throw exp;
            }

            return oMfgjoborderitems;
        }

        public Mfgjoborderitems Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Mfgjoborderitems oMfgjoborderitems)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgjoborderitems.id;
            command.Parameters.Add("jobreasonid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgjoborderitems.jobreasonid;
            command.Parameters.Add("jobchargeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgjoborderitems.jobchargeid;
            command.Parameters.Add("bomid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgjoborderitems.bomid;
            command.Parameters.Add("oqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgjoborderitems.oqty;
            command.Parameters.Add("rqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgjoborderitems.rqty;
            command.Parameters.Add("brqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgjoborderitems.brqty;
            command.Parameters.Add("sqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgjoborderitems.sqty;
            command.Parameters.Add("cqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgjoborderitems.cqty;
            command.Parameters.Add("cost", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgjoborderitems.cost;
            command.Parameters.Add("marginpercentage", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgjoborderitems.marginpercentage;
            command.Parameters.Add("bominwardcode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfgjoborderitems.bominwardcode == null ? "" : oMfgjoborderitems.bominwardcode;
            command.Parameters.Add("bominvoiceitemimage", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfgjoborderitems.bominvoiceitemimage == null ? "" : oMfgjoborderitems.bominvoiceitemimage;
            command.Parameters.Add("bominvoiceitemid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgjoborderitems.bominvoiceitemid;
            command.Parameters.Add("sizeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgjoborderitems.sizeid;
            command.Parameters.Add("mfgworkorderitemsid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgjoborderitems.mfgworkorderitemsid;
            command.Parameters.Add("mfgworkorderitemsizesid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgjoborderitems.mfgworkorderitemsizesid;
            command.Parameters.Add("materialtype", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgjoborderitems.materialtype;
            command.Parameters.Add("dynamicbom", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oMfgjoborderitems.dynamicbom;
            command.Parameters.Add("sellingprice", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgjoborderitems.sellingprice;
            command.Parameters.Add("taxpercent", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgjoborderitems.taxpercentage;
            command.Parameters.Add("tax", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgjoborderitems.tax;
            command.Parameters.Add("amount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgjoborderitems.amount;
            command.Parameters.Add("taxid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgjoborderitems.taxid;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oMfgjoborderitems.isactive;
            command.Parameters.Add("sno", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgjoborderitems.sno;
            command.Parameters.Add("isbom", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oMfgjoborderitems.isbom;
            command.Parameters.Add("supportbominvoice", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oMfgjoborderitems.supportbominvoice;
            command.Parameters.Add("width", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfgjoborderitems.width == null ? "" : oMfgjoborderitems.width;
            command.Parameters.Add("isfabric", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oMfgjoborderitems.isfabric;
            command.Parameters.Add("sourcejoborderitemid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgjoborderitems.sourcejoborderitemid;
            command.Parameters.Add("stageid", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgjoborderitems.corestageid;
            command.Parameters.Add("corestageid", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgjoborderitems.stageid;
            command.Parameters.Add("inwardgoods", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oMfgjoborderitems.inwardgoods;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgjoborderitems.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgjoborderitems.modifiedby;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgjoborderitems.version;
            command.Parameters.Add("mfgjoborderid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgjoborderitems.mfgjoborderid;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgjoborderitems.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgjoborderitems.modifiedon;
            command.Parameters.Add("bomcategoryid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgjoborderitems.bomcategoryid;
            command.Parameters.Add("productid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgjoborderitems.productid;
            command.Parameters.Add("colourid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgjoborderitems.colourid;

            command.Parameters.Add("typedisplay", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfgjoborderitems.typedisplay == null ? "" : oMfgjoborderitems.typedisplay;
            command.Parameters.Add("itemdisplay", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfgjoborderitems.itemdisplay == null ? "" : oMfgjoborderitems.itemdisplay;

            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oMfgjoborderitems.version = Convert.ToInt32(reader[0].ToString());
                }
            }

            return oMfgjoborderitems;
        }

        public bool Update(NpgsqlTransaction transaction, NpgsqlConnection conn, List<Mfgjoborderitems> list, Mfgjoborder mfgjoborder)
        {
            var existingItems = SearchAll(mfgjoborder.id, 0);
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
                    item.mfgjoborderid = mfgjoborder.id;
                    item.mfgworkorderid = mfgjoborder.mfgworkorderid;
                    item.corestageid = mfgjoborder.corestageid;
                    item.stageid = mfgjoborder.stageid;
                    Insert(transaction, conn, item);
                }
            }
            return true;
        }

        public bool Delete(Mfgjoborderitems oMfgjoborderitems)
        {
            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oMfgjoborderitems);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oMfgjoborderitems);

                throw exp;
            }
            return hasSuccessCount;

        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Mfgjoborderitems oMfgjoborderitems)
        {
            NpgsqlCommand command = new NpgsqlCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oMfgjoborderitems.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgjoborderitems.modifiedby;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgjoborderitems.modifiedon;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgjoborderitems.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgjoborderitems.id;
            int successCount = command.ExecuteNonQuery();
            return successCount > 0;
        }

        public bool DeleteAll(long mfgjoborderid, long userid, NpgsqlConnection conn, NpgsqlTransaction transaction)
        {
            int successCount = 0;
            NpgsqlCommand command = new NpgsqlCommand(SQL_DeleteAll, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = false;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = userid;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("mfgjoborderid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = mfgjoborderid;
            successCount = command.ExecuteNonQuery();
            return successCount > 0;
        }

    }

}
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                   