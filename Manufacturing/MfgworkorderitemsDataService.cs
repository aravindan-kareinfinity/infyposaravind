using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
namespace InfyPOS.Data
{
    public class MfgworkorderitemsDataService : SAASData
    {
        /*	with alldesigns
					as
					(
					select schemelist->>'name' colourcode,Mfgworkorderitems.designname,Mfgworkorderitems.id from Mfgworkorderitems
					inner join bommaster on bommaster.id = fabricid
					LEFT JOIN LATERAL
					json_array_elements (cast(colourlist as json)) schemelist ON true
					where bommaster.colourmode = 2 and bommaster.isactive=true and Mfgworkorderitems.colourid = cast(schemelist->>'id' as bigint)
					)
					
					update Mfgworkorderitems set colourcode = alldesigns.colourcode,designitemname = alldesigns.designname||'/'||alldesigns.colourcode from alldesigns
					where alldesigns.id = Mfgworkorderitems.id
					*/

        const string SQL_Search = @"select id, mfgworkorderid,salesorderid, isactive, createdby, modifiedby, version, createdon, modifiedon, received, damaged, colourid,designitemname, sleeveid,fabricid,efabricid,efabriccolourid, size, designid, inqty, outqty,length, cutratio, cost, taxamount, amount, taxid,designname,productid,productname,taxpercentage,sizecount from mfgworkorderitems where id=@id";

        const string SQL_SearchAll = @"select mfgworkorderitems.id, mfgworkorderid,salesorderid, mfgworkorderitems.isactive, mfgworkorderitems.createdby, mfgworkorderitems.modifiedby, 
        mfgworkorderitems.version, mfgworkorderitems.createdon, mfgworkorderitems.modifiedon, received, damaged, colourid,designitemname, sleeveid,fabricid,efabricid,efabriccolourid, size,fabric.categoryid,
        fabric.imageid bominvoiceitemimage ,fabric.name bominwardcode, designid, inqty, outqty, cutratio,length, cost, taxamount, amount, mfgworkorderitems.taxid,
        designname,productid,productname,taxpercentage,sizecount from mfgworkorderitems 
        left outer join bommaster fabric on fabric.id = fabricid
        left outer join bommaster efabric on efabric.id = efabricid
        where mfgworkorderid=@mfgworkorderid and mfgworkorderitems.isactive=true";

        const string SQL_Insert = @"INSERT INTO public.mfgworkorderitems(mfgworkorderid,salesorderid, isactive, createdby, modifiedby, version, createdon, modifiedon, received, damaged, colourid,designitemname, sleeveid,fabricid,efabricid,efabriccolourid, size, designid, inqty, outqty, cutratio,length, cost, taxamount, amount, taxid,designname,productid,productname,taxpercentage,sizecount) 
        VALUES(@mfgworkorderid,@salesorderid, @isactive, @createdby, @modifiedby, @version, @createdon, @modifiedon, @received, @damaged, @colourid,@designitemname, @sleeveid,@fabricid,@efabricid,@efabriccolourid, @size, @designid, @inqty, @outqty, @cutratio,@length, @cost, @taxamount, @amount, @taxid,@designname,@productid,@productname,@taxpercentage,@sizecount) RETURNING id";


        const string SQL_Update = @"UPDATE public.mfgworkorderitems SET mfgworkorderid=@mfgworkorderid,salesorderid=@salesorderid, modifiedby=@modifiedby, version=@version+1, modifiedon=@modifiedon, received=@received, damaged=@damaged,designitemname=@designitemname, colourid=@colourid,fabricid=@fabricid,efabricid=@efabricid,efabriccolourid=@efabriccolourid, sleeveid=@sleeveid, size=@size, designid=@designid, inqty=@inqty, outqty=@outqty, cutratio=@cutratio,length=@length, 
        cost=@cost, taxamount=@taxamount, amount=@amount, taxid=@taxid, designname=@designname,productid=@productid,productname=@productname,taxpercentage=@taxpercentage,sizecount=@sizecount where id=@id and version=@version RETURNING version";

        const string SQL_Delete = @"update public.mfgworkorderitems set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";
        const string SQL_DeleteAll = @"update public.mfgworkorderitems set isactive=false,modifiedby=@modifiedby,modifiedon=@modifiedon where mfgworkorderid=@mfgworkorderid";
        public List<Mfgworkorderitems> Search(MfgworkorderitemsCreteria _mfgworkorderitems)
        {
            List<Mfgworkorderitems> lst = new List<Mfgworkorderitems>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_mfgworkorderitems.id != 0)
                {
                    command = new NpgsqlCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _mfgworkorderitems.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("isactive", true, "=");

                    //if (!string.IsNullOrEmpty(_mfgworkorderitems.Code))
                    //    qb.AddParameter("code", _mfgworkorderitems.Code, "=");

                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Mfgworkorderitems omfgworkorderitems = new Mfgworkorderitems();
                        omfgworkorderitems.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        omfgworkorderitems.mfgworkorderid = reader["mfgworkorderid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["mfgworkorderid"]);
                        omfgworkorderitems.salesorderid = reader["salesorderid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["salesorderid"]);
                        omfgworkorderitems.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        omfgworkorderitems.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        omfgworkorderitems.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        omfgworkorderitems.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        omfgworkorderitems.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        omfgworkorderitems.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        omfgworkorderitems.received = reader["received"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["received"]);
                        omfgworkorderitems.damaged = reader["damaged"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["damaged"]);
                        omfgworkorderitems.colourid = reader["colourid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["colourid"]);
                        omfgworkorderitems.sleeveid = reader["sleeveid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sleeveid"]);
                        omfgworkorderitems.fabricid = reader["fabricid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["fabricid"]);
                        omfgworkorderitems.efabricid = reader["efabricid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["efabricid"]);
                        omfgworkorderitems.efabriccolourid = reader["efabriccolourid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["efabriccolourid"]);
                        omfgworkorderitems.size = reader["size"] == DBNull.Value ? "" : reader["size"].ToString();
                        omfgworkorderitems.designitemname = reader["designitemname"] == DBNull.Value ? "" : reader["designitemname"].ToString();
                        omfgworkorderitems.designid = reader["designid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["designid"]);
                        omfgworkorderitems.inqty = reader["inqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["inqty"]);
                        omfgworkorderitems.outqty = reader["outqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["outqty"]);
                        omfgworkorderitems.cutratio = reader["cutratio"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["cutratio"]);
                        omfgworkorderitems.length = reader["length"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["length"]);
                        omfgworkorderitems.cost = reader["cost"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["cost"]);
                        omfgworkorderitems.taxamount = reader["taxamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["taxamount"]);
                        omfgworkorderitems.amount = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"]);
                        omfgworkorderitems.taxid = reader["taxid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["taxid"]);
                        omfgworkorderitems.sizecount = reader["sizecount"] == DBNull.Value ? 0 : Convert.ToInt32(reader["sizecount"]);
                        omfgworkorderitems.taxpercentage = reader["taxpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["taxpercentage"]);
                        omfgworkorderitems.productid = reader["productid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productid"]);
                        omfgworkorderitems.designname = reader["designname"] == DBNull.Value ? "" : reader["designname"].ToString();

                        lst.Add(omfgworkorderitems);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _mfgworkorderitems);
            }
            return lst;
        }

        public List<Mfgworkorderitems> SearchAll(long _mfgworkorderid)
        {
            List<Mfgworkorderitems> lst = new List<Mfgworkorderitems>();
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
                        Mfgworkorderitems omfgworkorderitems = new Mfgworkorderitems();
                        omfgworkorderitems.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        omfgworkorderitems.mfgworkorderid = reader["mfgworkorderid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["mfgworkorderid"]);
                        omfgworkorderitems.salesorderid = reader["salesorderid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["salesorderid"]);
                        omfgworkorderitems.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        omfgworkorderitems.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        omfgworkorderitems.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        omfgworkorderitems.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        omfgworkorderitems.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        omfgworkorderitems.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        omfgworkorderitems.received = reader["received"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["received"]);
                        omfgworkorderitems.damaged = reader["damaged"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["damaged"]);
                        omfgworkorderitems.colourid = reader["colourid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["colourid"]);
                        omfgworkorderitems.sleeveid = reader["sleeveid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sleeveid"]);
                        omfgworkorderitems.fabricid = reader["fabricid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["fabricid"]);
                        omfgworkorderitems.efabricid = reader["efabricid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["efabricid"]);
                        omfgworkorderitems.efabriccolourid = reader["efabriccolourid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["efabriccolourid"]);
                        omfgworkorderitems.bomcategoryid = reader["categoryid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["categoryid"]);
                        omfgworkorderitems.bominvoiceitemimage = reader["bominvoiceitemimage"] == DBNull.Value ? "" : reader["bominvoiceitemimage"].ToString();
                        omfgworkorderitems.bominwardcode = reader["bominwardcode"] == DBNull.Value ? "" : reader["bominwardcode"].ToString();
                        omfgworkorderitems.designitemname = reader["designitemname"] == DBNull.Value ? "" : reader["designitemname"].ToString();
                        
                        omfgworkorderitems.size = reader["size"] == DBNull.Value ? "" : reader["size"].ToString();
                        omfgworkorderitems.designid = reader["designid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["designid"]);
                        omfgworkorderitems.inqty = reader["inqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["inqty"]);
                        omfgworkorderitems.outqty = reader["outqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["outqty"]);
                        omfgworkorderitems.cutratio = reader["cutratio"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["cutratio"]);
                        omfgworkorderitems.length = reader["length"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["length"]);
                        omfgworkorderitems.cost = reader["cost"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["cost"]);
                        omfgworkorderitems.taxamount = reader["taxamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["taxamount"]);
                        omfgworkorderitems.amount = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"]);
                        omfgworkorderitems.taxid = reader["taxid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["taxid"]);
                        omfgworkorderitems.sizecount = reader["sizecount"] == DBNull.Value ? 0 : Convert.ToInt32(reader["sizecount"]);
                        omfgworkorderitems.taxpercentage = reader["taxpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["taxpercentage"]);
                        omfgworkorderitems.productid = reader["productid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productid"]);
                        omfgworkorderitems.designname = reader["designname"] == DBNull.Value ? "" : reader["designname"].ToString();
                        

                        lst.Add(omfgworkorderitems);
                    }
                }
                conn.Close();

                MfgworkorderitemsizesDataService mfgworkorderitemsizesDataService = new MfgworkorderitemsizesDataService() { Context = this.Context };
                var workordersizeitems = mfgworkorderitemsizesDataService.SearchAll(_mfgworkorderid);
                foreach(var sizeitem in workordersizeitems.GroupBy(x=>x.mfgworkorderitemsid))
                {
                    if(lst.Exists(x => x.id == sizeitem.Key))
                        lst.Find(x => x.id == sizeitem.Key).sizelist = sizeitem.ToList();
                }
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _mfgworkorderid);
            }
            return lst;
        }

        //public Mfgworkorderitems Insert(Mfgworkorderitems oMfgworkorderitems)
        //{

        //    NpgsqlTransaction transaction = null;
        //    try
        //    {
        //        using (NpgsqlConnection conn = CreateConnection())
        //        {
        //            conn.Open();
        //            transaction = conn.BeginTransaction();
        //            NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);

        //            Insert(transaction, conn, oMfgworkorderitems);
        //            transaction.Commit();
        //            conn.Close();
        //        }
        //    }
        //    catch (Exception exp)
        //    {
        //        WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oMfgworkorderitems);
        //        throw exp;
        //    }

        //    return oMfgworkorderitems;
        //}

        public Mfgworkorderitems Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Mfgworkorder oMfgworkorder, Mfgworkorderitems oMfgworkorderitems)
        {
            if (oMfgworkorderitems.productid == 0 && oMfgworkorderitems.efabricid<=0) throw new Exception("Please enter the product information");
            oMfgworkorderitems.isactive = true;
            oMfgworkorderitems.version = 1;
            oMfgworkorderitems.createdby = Context.UserID;
            oMfgworkorderitems.modifiedby = Context.UserID;
            oMfgworkorderitems.createdon = DateTimeNow;
            oMfgworkorderitems.modifiedon = DateTimeNow;
            NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("mfgworkorderid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkorderitems.mfgworkorderid;
            command.Parameters.Add("salesorderid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkorderitems.salesorderid;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oMfgworkorderitems.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkorderitems.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkorderitems.modifiedby;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgworkorderitems.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgworkorderitems.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgworkorderitems.modifiedon;
            command.Parameters.Add("received", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgworkorderitems.received;
            command.Parameters.Add("damaged", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgworkorderitems.damaged;
            command.Parameters.Add("colourid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkorderitems.colourid;
            command.Parameters.Add("sleeveid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkorderitems.sleeveid;
            command.Parameters.Add("fabricid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkorderitems.fabricid;
            command.Parameters.Add("efabricid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkorderitems.efabricid;
            command.Parameters.Add("efabriccolourid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkorderitems.efabriccolourid;
            command.Parameters.Add("size", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfgworkorderitems.size == null ? "" : oMfgworkorderitems.size;
            command.Parameters.Add("designitemname", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfgworkorderitems.designitemname == null ? "" : oMfgworkorderitems.designitemname;
            command.Parameters.Add("designid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkorderitems.designid;
            command.Parameters.Add("inqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgworkorderitems.inqty;
            command.Parameters.Add("outqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgworkorderitems.outqty;
            command.Parameters.Add("cutratio", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgworkorderitems.cutratio;
            command.Parameters.Add("length", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgworkorderitems.length;
            command.Parameters.Add("cost", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgworkorderitems.cost;
            command.Parameters.Add("taxamount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgworkorderitems.taxamount;
            command.Parameters.Add("sizecount", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgworkorderitems.sizecount;
            command.Parameters.Add("taxpercentage", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgworkorderitems.taxpercentage;
            command.Parameters.Add("amount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgworkorderitems.amount;
            command.Parameters.Add("taxid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkorderitems.taxid;

            command.Parameters.Add("productid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkorderitems.productid;
            command.Parameters.Add("productname", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfgworkorderitems.productname == null ? "" : oMfgworkorderitems.productname;
            command.Parameters.Add("designname", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfgworkorderitems.designname == null ? "" : oMfgworkorderitems.designname;

            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oMfgworkorderitems.id = Convert.ToInt64(reader[0].ToString());
                }
            }

            if (!oMfgworkorder.onlybom)
            {
                if (oMfgworkorderitems.sizelist.GroupBy(x => x.sizeid).ToList().Exists(x => x.Count() > 1))
                {
                    throw new Exception("Duplicate size can't save the work order,please contact the software vendor!");
                }
                if (oMfgworkorderitems.id == 0)
                    throw new Exception("Can't creat items on mfgworkorderitems");

                MfgworkorderitemsizesDataService subMfgworkorderitemsizes = new MfgworkorderitemsizesDataService() { Context = this.Context };
                foreach (var item in oMfgworkorderitems.sizelist)
                {
                    item.isactive = true;
                    item.mfgworkorderitemsid = oMfgworkorderitems.id;
                    item.mfgworkorderid = oMfgworkorderitems.mfgworkorderid;
                    subMfgworkorderitemsizes.Insert(transaction, conn, item);
                }
            }
            return oMfgworkorderitems;
        }

        public Mfgworkorderitems Update(Mfgworkorderitems oMfgworkorderitems)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oMfgworkorderitems);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oMfgworkorderitems);
                throw exp;
            }

            return oMfgworkorderitems;
        }

        public Mfgworkorderitems Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Mfgworkorderitems oMfgworkorderitems)
        {
            oMfgworkorderitems.modifiedby = Context.UserID;
            oMfgworkorderitems.modifiedon = DateTimeNow;
            oMfgworkorderitems.isactive = true;
            NpgsqlCommand command = new NpgsqlCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkorderitems.id;
            command.Parameters.Add("mfgworkorderid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkorderitems.mfgworkorderid;
            command.Parameters.Add("salesorderid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkorderitems.salesorderid;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oMfgworkorderitems.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkorderitems.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkorderitems.modifiedby;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgworkorderitems.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgworkorderitems.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgworkorderitems.modifiedon;
            command.Parameters.Add("received", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgworkorderitems.received;
            command.Parameters.Add("damaged", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgworkorderitems.damaged;
            command.Parameters.Add("colourid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkorderitems.colourid;
            command.Parameters.Add("designitemname", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfgworkorderitems.designitemname == null ? "" : oMfgworkorderitems.designitemname;
            command.Parameters.Add("sleeveid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkorderitems.sleeveid;
            command.Parameters.Add("fabricid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkorderitems.fabricid;
            command.Parameters.Add("efabricid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkorderitems.efabricid;
            command.Parameters.Add("efabriccolourid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkorderitems.efabriccolourid;
            command.Parameters.Add("size", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfgworkorderitems.size == null ? "" : oMfgworkorderitems.size;
            command.Parameters.Add("designid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkorderitems.designid;
            command.Parameters.Add("inqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgworkorderitems.inqty;
            command.Parameters.Add("outqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgworkorderitems.outqty;
            command.Parameters.Add("cutratio", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgworkorderitems.cutratio;
            command.Parameters.Add("length", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgworkorderitems.length;
            command.Parameters.Add("cost", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgworkorderitems.cost;
            command.Parameters.Add("taxamount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgworkorderitems.taxamount;
            command.Parameters.Add("amount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgworkorderitems.amount;
            command.Parameters.Add("taxid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkorderitems.taxid;
            command.Parameters.Add("productid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkorderitems.productid;
            command.Parameters.Add("sizecount", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgworkorderitems.sizecount;
            command.Parameters.Add("taxpercentage", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgworkorderitems.taxpercentage;
            command.Parameters.Add("productname", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfgworkorderitems.productname == null ? "" : oMfgworkorderitems.productname;
            command.Parameters.Add("designname", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfgworkorderitems.designname == null ? "" : oMfgworkorderitems.designname;

            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oMfgworkorderitems.version = Convert.ToInt32(reader[0].ToString());
                }
            }

            return oMfgworkorderitems;
        }

        public bool Update(NpgsqlTransaction transaction, NpgsqlConnection conn, List<Mfgworkorderitems> list, Mfgworkorder workorder)
        {
            if (list.Any(x=>x.productid == 0 && x.efabricid<=0)) throw new Exception("Please enter the product information");
            var existingItems = SearchAll(workorder.id);
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
                    item.isactive = true;
                    Update(transaction, conn, item);
                    if (!workorder.onlybom)
                    {
                        MfgworkorderitemsizesDataService subMfgworkorderitemsizes = new MfgworkorderitemsizesDataService() { Context = this.Context };
                        subMfgworkorderitemsizes.Update(transaction, conn, item.sizelist, item.id);
                    }
                }
                else
                {
                    item.isactive = true;
                    item.mfgworkorderid = workorder.id;
                    Insert(transaction, conn, workorder, item);
                    //if (!workorder.onlybom)
                    //{
                    //    MfgworkorderitemsizesDataService subMfgworkorderitemsizes = new MfgworkorderitemsizesDataService() { Context = this.Context };
                    //    if (item.id == 0)
                    //        throw new Exception("Can't creat items on mfgworkorderitems");

                    //    foreach (var sizeitem in item.sizelist)
                    //    {
                    //        sizeitem.isactive = true;
                    //        sizeitem.mfgworkorderitemsid = item.id;
                    //        sizeitem.mfgworkorderid = item.mfgworkorderid;
                    //        subMfgworkorderitemsizes.Insert(transaction, conn, sizeitem);
                    //    }
                    //}
                }
            }
            return true;
        }

        public bool Delete(Mfgworkorderitems oMfgworkorderitems)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oMfgworkorderitems);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oMfgworkorderitems);

                throw exp;
            }
            return hasSuccessCount;

        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Mfgworkorderitems oMfgworkorderitems)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oMfgworkorderitems.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkorderitems.modifiedby;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgworkorderitems.modifiedon;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgworkorderitems.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkorderitems.id;
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
