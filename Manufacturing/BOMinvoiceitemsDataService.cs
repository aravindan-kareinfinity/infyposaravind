using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
namespace InfyPOS.Data

{
    public class BominvoiceitemsDataService : SAASData
    {
        const string SQL_Search = @"select id, sno, poinfo, bomid, buyingqty, buyingprice, purchasetotaldiscount, amount, isactive, createdby, modifiedby, version, createdon, modifiedon, 
        damagedqty, sellableqty, taxid, returnqty, basebuyingprice, hsncode, purchasediscount, purchasediscountpercentage, purchasetaxpercentage, purchasetax, bomcategoryid, bominvoiceid, 
        sellingprice, marginpercentage, salestax,bominwardcode,colourid,imageid,width,fgskuid,fgproductid,dcid,dcitemid from bominvoiceitems where id=@id";
        
        
        const string SQL_SearchAll = @"select bominvoiceitems.id, sno, poinfo, bomid, buyingqty, buyingprice, purchasetotaldiscount, amount, bominvoiceitems.isactive, 
        bominvoiceitems.createdby, bominvoiceitems.modifiedby, bominvoiceitems.version, bominvoiceitems.createdon,
        bominvoiceitems.modifiedon, damagedqty, sellableqty, bominvoiceitems.taxid, returnqty, basebuyingprice, 
		bominvoiceitems.hsncode, purchasediscount, purchasediscountpercentage, purchasetaxpercentage, purchasetax, 
        bomcategoryid, bominvoiceid, sellingprice, marginpercentage, salestax,bominwardcode,colourid,
		bominvoiceitems.imageid,bominvoiceitems.width,boms.printingname bomprintingname,dcid,dcitemid,
		bomcategory.name category,boms.name bomname,colour.name colourname,fgskuid,fgproductid
		from bominvoiceitems
        left outer join referencelist bomcategory on bomcategory.id = bominvoiceitems.bomcategoryid
        left outer join bommaster boms on boms.id = bominvoiceitems.bomid
        left outer join referencelist colour on colour.id = bominvoiceitems.colourid 
        where bominvoiceitems.isactive= true and bominvoiceitems.bominvoiceid=@bominvoiceid";


        const string SQL_Insert = @"INSERT INTO public.bominvoiceitems(sno, poinfo, bomid, buyingqty, buyingprice, purchasetotaldiscount, amount, isactive, createdby, modifiedby, version, createdon, modifiedon, damagedqty, sellableqty, taxid, returnqty, basebuyingprice, 
        hsncode, purchasediscount, purchasediscountpercentage, purchasetaxpercentage, purchasetax, bomcategoryid, bominvoiceid, sellingprice, marginpercentage, salestax,bominwardcode,colourid,
        imageid,width,fgskuid,fgproductid,dcid,dcitemid) 
        VALUES(@sno, @poinfo, @bomid, @buyingqty, @buyingprice, @purchasetotaldiscount, @amount, @isactive, @createdby, @modifiedby, @version, @createdon, @modifiedon, @damagedqty, @sellableqty,
        @taxid, @returnqty, @basebuyingprice, @hsncode, @purchasediscount, @purchasediscountpercentage, @purchasetaxpercentage, @purchasetax, @bomcategoryid, @bominvoiceid, @sellingprice,
        @marginpercentage, @salestax,@bominwardcode,@colourid,@imageid,@width,@fgskuid,@fgproductid,@dcid,@dcitemid) RETURNING id";


        const string SQL_Update = @"UPDATE public.bominvoiceitems SET sno=@sno, poinfo=@poinfo, bomid=@bomid, buyingqty=@buyingqty, buyingprice=@buyingprice, purchasetotaldiscount=@purchasetotaldiscount, amount=@amount, modifiedby=@modifiedby, version=@version+1, modifiedon=@modifiedon, damagedqty=@damagedqty, sellableqty=@sellableqty, taxid=@taxid, returnqty=@returnqty, basebuyingprice=@basebuyingprice, hsncode=@hsncode, 
        purchasediscount=@purchasediscount, purchasediscountpercentage=@purchasediscountpercentage, purchasetaxpercentage=@purchasetaxpercentage, purchasetax=@purchasetax, 
        bomcategoryid=@bomcategoryid, bominvoiceid=@bominvoiceid, sellingprice=@sellingprice, marginpercentage=@marginpercentage, salestax=@salestax ,bominwardcode=@bominwardcode,
        colourid=@colourid,imageid=@imageid,width=@width,fgskuid=@fgskuid,fgproductid=@fgproductid,dcid=@dcid,dcitemid=@dcitemid
        where id=@id and version=@version RETURNING version";

        const string SQL_Delete = @"update public.bominvoiceitems set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";
        const string SQL_DeleteAll = @"update public.bominvoiceitems set isactive=false,modifiedby=@modifiedby,modifiedon=@modifiedon where bominvoiceid=@bominvoiceid";

        const string SQL_UpdatePOItems = @"with polist as
                                    (select bominvoiceitems.id,cast(schemelist ->> 'componentpoitemsid' as bigint) componentpoitemsid,
                                    cast(schemelist ->> 'aqty' as numeric) aqty from bominvoiceitems
                                    LEFT OUTER JOIN LATERAL json_array_elements (case when bominvoiceitems.poinfo is null 
																				or bominvoiceitems.poinfo::text ='null' then '[]'::json else bominvoiceitems.poinfo::json end) schemelist ON true
                                    where bominvoiceitems.id = @id and cast(schemelist ->> 'componentpoitemsid' as bigint) is not null)
							
									update ComponentPOitems set rqty = rqty{0}polist.aqty from polist
                                    where polist.componentpoitemsid = ComponentPOitems.id";

        public List<Bominvoiceitems> Search(BominvoiceitemsCreteria _bominvoiceitems)
        {
            List<Bominvoiceitems> lst = new List<Bominvoiceitems>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_bominvoiceitems.id != 0)
                {
                    command = new NpgsqlCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _bominvoiceitems.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("isactive", true, "=");

                    if (!string.IsNullOrEmpty(_bominvoiceitems.bominwardcode))
                        qb.AddParameter("bominwardcode", _bominvoiceitems.bominwardcode, "=");

                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Bominvoiceitems obominvoiceitems = new Bominvoiceitems();
                        obominvoiceitems.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        obominvoiceitems.sno = reader["sno"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["sno"]);
                        obominvoiceitems.poinfo_json = reader["poinfo"] == DBNull.Value ? null : reader["poinfo"].ToString();
                        obominvoiceitems.bomid = reader["bomid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["bomid"]);
                        obominvoiceitems.buyingqty = reader["buyingqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["buyingqty"]);
                        obominvoiceitems.buyingprice = reader["buyingprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["buyingprice"]);
                        obominvoiceitems.purchasetotaldiscount = reader["purchasetotaldiscount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["purchasetotaldiscount"]);
                        obominvoiceitems.amount = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"]);
                        obominvoiceitems.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        obominvoiceitems.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        obominvoiceitems.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        obominvoiceitems.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        obominvoiceitems.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        obominvoiceitems.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        obominvoiceitems.damagedqty = reader["damagedqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["damagedqty"]);
                        obominvoiceitems.sellableqty = reader["sellableqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["sellableqty"]);
                        obominvoiceitems.taxid = reader["taxid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["taxid"]);
                        obominvoiceitems.dcitemid = reader["dcitemid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["dcitemid"]);
                        obominvoiceitems.dcid = reader["dcid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["dcid"]);
                        
                        obominvoiceitems.returnqty = reader["returnqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["returnqty"]);
                        obominvoiceitems.basebuyingprice = reader["basebuyingprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["basebuyingprice"]);
                        obominvoiceitems.hsncode = reader["hsncode"] == DBNull.Value ? "" : reader["hsncode"].ToString();
                        obominvoiceitems.purchasediscount = reader["purchasediscount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["purchasediscount"]);
                        obominvoiceitems.purchasediscountpercentage = reader["purchasediscountpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["purchasediscountpercentage"]);
                        obominvoiceitems.purchasetaxpercentage = reader["purchasetaxpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["purchasetaxpercentage"]);
                        obominvoiceitems.purchasetax = reader["purchasetax"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["purchasetax"]);
                        obominvoiceitems.bomcategoryid = reader["bomcategoryid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["bomcategoryid"]);

                        obominvoiceitems.colourid = reader["colourid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["colourid"]);
                        obominvoiceitems.imageid = reader["imageid"] == DBNull.Value ? "" : reader["imageid"].ToString();
                        obominvoiceitems.width = reader["width"] == DBNull.Value ? "" : reader["width"].ToString();
                        obominvoiceitems.bominwardcode = reader["bominwardcode"] == DBNull.Value ? "" : reader["bominwardcode"].ToString();

                        obominvoiceitems.fgskuid = reader["fgskuid"] == DBNull.Value ? "" : reader["fgskuid"].ToString();
                        obominvoiceitems.fgproductid = reader["fgproductid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["fgproductid"]);

                        obominvoiceitems.bominvoiceid = reader["bominvoiceid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["bominvoiceid"]);
                        obominvoiceitems.sellingprice = reader["sellingprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["sellingprice"]);
                        obominvoiceitems.marginpercentage = reader["marginpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["marginpercentage"]);
                        obominvoiceitems.salestax = reader["salestax"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["salestax"]);

                        obominvoiceitems.buyingprice = obominvoiceitems.basebuyingprice;
                        if (obominvoiceitems.purchasediscount > 0)
                            obominvoiceitems.buyingprice -= obominvoiceitems.purchasediscount / obominvoiceitems.buyingqty;

                        lst.Add(obominvoiceitems);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _bominvoiceitems);
            }
            return lst;
        }

        internal Bominvoiceitems SearchCode(string code)
        {
            var stocklist = Search(new BominvoiceitemsCreteria()
            {
                bominwardcode = code
            });
            if (stocklist.Count > 0) return stocklist.First();
            return null;
        }


        public List<Bominvoiceitems> SearchAll(long _bominvoiceid)
        {
            List<Bominvoiceitems> lst = new List<Bominvoiceitems>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = new NpgsqlCommand(SQL_SearchAll, conn);
                command.Parameters.Add("bominvoiceid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _bominvoiceid;
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Bominvoiceitems obominvoiceitems = new Bominvoiceitems();
                        obominvoiceitems.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        obominvoiceitems.sno = reader["sno"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["sno"]);
                        obominvoiceitems.poinfo_json = reader["poinfo"] == DBNull.Value ? null : reader["poinfo"].ToString();
                        obominvoiceitems.bomid = reader["bomid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["bomid"]);
                        obominvoiceitems.buyingqty = reader["buyingqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["buyingqty"]);
                        obominvoiceitems.buyingprice = reader["buyingprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["buyingprice"]);
                        obominvoiceitems.purchasetotaldiscount = reader["purchasetotaldiscount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["purchasetotaldiscount"]);
                        obominvoiceitems.amount = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"]);
                        obominvoiceitems.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        obominvoiceitems.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        obominvoiceitems.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        obominvoiceitems.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        obominvoiceitems.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        obominvoiceitems.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        obominvoiceitems.damagedqty = reader["damagedqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["damagedqty"]);
                        obominvoiceitems.sellableqty = reader["sellableqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["sellableqty"]);
                        obominvoiceitems.taxid = reader["taxid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["taxid"]);
                        obominvoiceitems.dcid = reader["dcid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["dcid"]);
                        obominvoiceitems.dcitemid = reader["dcitemid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["dcitemid"]);

                        obominvoiceitems.returnqty = reader["returnqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["returnqty"]);
                        obominvoiceitems.basebuyingprice = reader["basebuyingprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["basebuyingprice"]);
                        obominvoiceitems.hsncode = reader["hsncode"] == DBNull.Value ? "" : reader["hsncode"].ToString();
                        obominvoiceitems.purchasediscount = reader["purchasediscount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["purchasediscount"]);
                        obominvoiceitems.purchasediscountpercentage = reader["purchasediscountpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["purchasediscountpercentage"]);
                        obominvoiceitems.purchasetaxpercentage = reader["purchasetaxpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["purchasetaxpercentage"]);
                        obominvoiceitems.purchasetax = reader["purchasetax"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["purchasetax"]);
                        obominvoiceitems.bomcategoryid = reader["bomcategoryid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["bomcategoryid"]);
                        obominvoiceitems.bominvoiceid = reader["bominvoiceid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["bominvoiceid"]);
                        obominvoiceitems.sellingprice = reader["sellingprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["sellingprice"]);
                        obominvoiceitems.marginpercentage = reader["marginpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["marginpercentage"]);
                        obominvoiceitems.salestax = reader["salestax"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["salestax"]);
                        obominvoiceitems.colourid = reader["colourid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["colourid"]);
                        obominvoiceitems.imageid = reader["imageid"] == DBNull.Value ? "" : reader["imageid"].ToString();
                        obominvoiceitems.width = reader["width"] == DBNull.Value ? "" : reader["width"].ToString();
                        obominvoiceitems.bominwardcode = reader["bominwardcode"] == DBNull.Value ? "" : reader["bominwardcode"].ToString();
                        obominvoiceitems.fgskuid = reader["fgskuid"] == DBNull.Value ? "" : reader["fgskuid"].ToString();
                        obominvoiceitems.fgproductid = reader["fgproductid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["fgproductid"]);
                        obominvoiceitems.categoryname = reader["category"] == DBNull.Value ? "" : reader["category"].ToString();
                        obominvoiceitems.bomname = reader["bomname"] == DBNull.Value ? "" : reader["bomname"].ToString();
                        obominvoiceitems.bomprintingname = reader["bomprintingname"] == DBNull.Value ? "" : reader["bomprintingname"].ToString();
                        if (string.IsNullOrEmpty(obominvoiceitems.bomprintingname))
                            obominvoiceitems.bomprintingname = obominvoiceitems.bomname;
                        obominvoiceitems.colourname = reader["colourname"] == DBNull.Value ? "" : reader["colourname"].ToString();

                        obominvoiceitems.buyingprice = obominvoiceitems.basebuyingprice;
                        if (obominvoiceitems.purchasediscount > 0)
                            obominvoiceitems.buyingprice -= obominvoiceitems.purchasediscount / obominvoiceitems.buyingqty;

                        lst.Add(obominvoiceitems);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _bominvoiceid);
            }
            return lst;
        }

        public Bominvoiceitems Insert(Bominvoiceitems oBominvoiceitems)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);

                    Insert(transaction, conn, oBominvoiceitems);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oBominvoiceitems);
                throw exp;
            }

            return oBominvoiceitems;
        }

        public Bominvoiceitems Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Bominvoiceitems oBominvoiceitems)
        {
            oBominvoiceitems.createdon = DateTimeNow;
            oBominvoiceitems.buyingprice = oBominvoiceitems.basebuyingprice;
            if (oBominvoiceitems.purchasediscount > 0)
                oBominvoiceitems.buyingprice -= oBominvoiceitems.purchasediscount / oBominvoiceitems.buyingqty;
            NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("sno", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBominvoiceitems.sno;
            command.Parameters.Add("poinfo", NpgsqlTypes.NpgsqlDbType.Json).Value = oBominvoiceitems.poinfo_json == null ? "null" : oBominvoiceitems.poinfo_json;
            command.Parameters.Add("bomid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBominvoiceitems.bomid;
            command.Parameters.Add("buyingqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBominvoiceitems.buyingqty;
            command.Parameters.Add("buyingprice", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBominvoiceitems.buyingprice;
            command.Parameters.Add("purchasetotaldiscount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBominvoiceitems.purchasetotaldiscount;
            command.Parameters.Add("amount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBominvoiceitems.amount;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBominvoiceitems.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBominvoiceitems.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBominvoiceitems.modifiedby;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBominvoiceitems.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBominvoiceitems.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBominvoiceitems.modifiedon;
            command.Parameters.Add("damagedqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBominvoiceitems.damagedqty;
            command.Parameters.Add("sellableqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBominvoiceitems.sellableqty;
            command.Parameters.Add("taxid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBominvoiceitems.taxid;
            command.Parameters.Add("dcid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBominvoiceitems.dcid;
            command.Parameters.Add("dcitemid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBominvoiceitems.dcitemid;
            command.Parameters.Add("returnqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBominvoiceitems.returnqty;
            command.Parameters.Add("basebuyingprice", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBominvoiceitems.basebuyingprice;
            command.Parameters.Add("hsncode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBominvoiceitems.hsncode == null ? "" : oBominvoiceitems.hsncode;

            command.Parameters.Add("fgproductid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBominvoiceitems.fgproductid;
            command.Parameters.Add("fgskuid", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBominvoiceitems.fgskuid == null ? "" : oBominvoiceitems.fgskuid;

            command.Parameters.Add("colourid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBominvoiceitems.colourid;
            command.Parameters.Add("width", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBominvoiceitems.width == null ? "" : oBominvoiceitems.width;
            command.Parameters.Add("imageid", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBominvoiceitems.imageid == null ? "" : oBominvoiceitems.imageid;
            command.Parameters.Add("bominwardcode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBominvoiceitems.bominwardcode == null ? "" : oBominvoiceitems.bominwardcode;

            command.Parameters.Add("purchasediscount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBominvoiceitems.purchasediscount;
            command.Parameters.Add("purchasediscountpercentage", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBominvoiceitems.purchasediscountpercentage;
            command.Parameters.Add("purchasetaxpercentage", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBominvoiceitems.purchasetaxpercentage;
            command.Parameters.Add("purchasetax", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBominvoiceitems.purchasetax;
            command.Parameters.Add("bomcategoryid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBominvoiceitems.bomcategoryid;
            command.Parameters.Add("bominvoiceid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBominvoiceitems.bominvoiceid;
            command.Parameters.Add("sellingprice", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBominvoiceitems.sellingprice;
            command.Parameters.Add("marginpercentage", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBominvoiceitems.marginpercentage;
            command.Parameters.Add("salestax", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBominvoiceitems.salestax;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oBominvoiceitems.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            if (oBominvoiceitems.poinfo != null && oBominvoiceitems.poinfo.Count > 0)
                UpdatePOItemQty(transaction, conn, oBominvoiceitems.id, true);
            return oBominvoiceitems;
        }

        public Bominvoiceitems Update(Bominvoiceitems oBominvoiceitems)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oBominvoiceitems);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oBominvoiceitems);
                throw exp;
            }

            return oBominvoiceitems;
        }

        public Bominvoiceitems Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Bominvoiceitems oBominvoiceitems)
        {
            UpdatePOItemQty(transaction, conn, oBominvoiceitems.id, false);
            oBominvoiceitems.modifiedon = DateTimeNow;
            NpgsqlCommand command = new NpgsqlCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBominvoiceitems.id;
            command.Parameters.Add("sno", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBominvoiceitems.sno;
            command.Parameters.Add("poinfo", NpgsqlTypes.NpgsqlDbType.Json).Value = oBominvoiceitems.poinfo_json == null ? "null" : oBominvoiceitems.poinfo_json;
            command.Parameters.Add("bomid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBominvoiceitems.bomid;
            command.Parameters.Add("buyingqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBominvoiceitems.buyingqty;
            command.Parameters.Add("buyingprice", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBominvoiceitems.buyingprice;
            command.Parameters.Add("purchasetotaldiscount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBominvoiceitems.purchasetotaldiscount;
            command.Parameters.Add("amount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBominvoiceitems.amount;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBominvoiceitems.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBominvoiceitems.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBominvoiceitems.modifiedby;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBominvoiceitems.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBominvoiceitems.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBominvoiceitems.modifiedon;
            command.Parameters.Add("damagedqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBominvoiceitems.damagedqty;
            command.Parameters.Add("sellableqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBominvoiceitems.sellableqty;
            command.Parameters.Add("taxid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBominvoiceitems.taxid;
            command.Parameters.Add("dcid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBominvoiceitems.dcid;
            command.Parameters.Add("dcitemid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBominvoiceitems.dcitemid;
            command.Parameters.Add("returnqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBominvoiceitems.returnqty;
            command.Parameters.Add("basebuyingprice", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBominvoiceitems.basebuyingprice;
            command.Parameters.Add("hsncode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBominvoiceitems.hsncode == null ? "" : oBominvoiceitems.hsncode;
            command.Parameters.Add("purchasediscount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBominvoiceitems.purchasediscount;
            command.Parameters.Add("purchasediscountpercentage", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBominvoiceitems.purchasediscountpercentage;
            command.Parameters.Add("purchasetaxpercentage", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBominvoiceitems.purchasetaxpercentage;
            command.Parameters.Add("purchasetax", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBominvoiceitems.purchasetax;
            command.Parameters.Add("bomcategoryid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBominvoiceitems.bomcategoryid;
            command.Parameters.Add("bominvoiceid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBominvoiceitems.bominvoiceid;
            command.Parameters.Add("sellingprice", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBominvoiceitems.sellingprice;
            command.Parameters.Add("marginpercentage", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBominvoiceitems.marginpercentage;
            command.Parameters.Add("salestax", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBominvoiceitems.salestax;
            command.Parameters.Add("colourid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBominvoiceitems.colourid;
            command.Parameters.Add("bominwardcode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBominvoiceitems.bominwardcode == null ? "" : oBominvoiceitems.bominwardcode;
            command.Parameters.Add("width", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBominvoiceitems.width == null ? "" : oBominvoiceitems.width;
            command.Parameters.Add("imageid", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBominvoiceitems.imageid == null ? "" : oBominvoiceitems.imageid;
            command.Parameters.Add("fgproductid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBominvoiceitems.fgproductid;
            command.Parameters.Add("fgskuid", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBominvoiceitems.fgskuid == null ? "" : oBominvoiceitems.fgskuid;

            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oBominvoiceitems.version = Convert.ToInt32(reader[0].ToString());
                }
            }

            if (oBominvoiceitems.poinfo != null && oBominvoiceitems.poinfo.Count > 0)
                UpdatePOItemQty(transaction, conn, oBominvoiceitems.id, true);
            return oBominvoiceitems;
        }

        public bool Update(NpgsqlTransaction transaction, NpgsqlConnection conn, 
            List<Bominvoiceitems> list, BOMinvoice invoice)
        {
            var existingItems = SearchAll(invoice.id);
            BomstocktransactionDataService bomstocktransactionDataService = new BomstocktransactionDataService() { Context = this.Context };
            if (existingItems != null && existingItems.Count > 0)
            {
                var deletedItems = existingItems.FindAll(e => !list.Exists(f => f.id == e.id));
                foreach (var item in deletedItems)
                {
                    Delete(transaction, conn, item);
                    
                }
                bomstocktransactionDataService.Delete(Bomstocktransaction.TransactionType.Purchase,
                        transaction, conn,invoice.id);
            }

            var sequence = SequenceManager.Instance(Context.OrganizationID).Get("bominvoiceitem.bominwardcode", 0);
            foreach (var item in list.GroupBy(e=>e.bomid))
            {
                var items = item.ToList();
                if(items.Exists(e=>string.IsNullOrEmpty(e.bominwardcode)))
                {
                    var maxid = items.Max(e =>
                    {
                        if (string.IsNullOrEmpty(e.bominwardcode)) return 0;
                        int intout = 0;
                        if (int.TryParse(e.bominwardcode.Split('-')[1], out intout)) return intout;
                        return 0;
                    });
                    items = items.FindAll(e => string.IsNullOrEmpty(e.bominwardcode));
                    var uniqecode = sequence.GetNextNumber(Context, conn, transaction);
                    for (var i = maxid; i < items.Count; i++)
                    {
                        items[i].bominwardcode = uniqecode + "-" + (i + 1);
                    }
                }
                else
                {
                    var uniqecode = sequence.GetNextNumber(Context, conn, transaction);
                    for(var i=0;i<items.Count;i++)
                    {
                        items[i].bominwardcode = uniqecode + "-" + (i + 1);
                    }
                }
            }



            var insertingrecords = list.FindAll(x => x.id <= 0);
            foreach (var item in list)
            {
                if (item.id > 0)
                {
                    Update(transaction, conn, item);
                }
                else
                {
                    item.bominvoiceid = invoice.id;
                    Insert(transaction, conn, item);
                }
            }
            bomstocktransactionDataService.Update(Bomstocktransaction.TransactionType.Purchase,
            conn, transaction, invoice, list);
            return true;
        }

        public bool Delete(Bominvoiceitems oBominvoiceitems)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oBominvoiceitems);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oBominvoiceitems);

                throw exp;
            }
            return hasSuccessCount;

        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Bominvoiceitems oBominvoiceitems)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBominvoiceitems.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBominvoiceitems.modifiedby;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBominvoiceitems.modifiedon;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBominvoiceitems.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBominvoiceitems.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0 && UpdatePOItemQty(transaction, conn, oBominvoiceitems.id, false);
        }


        internal SortedDictionary<long, decimal> SearchAllDiscount(List<long> lrinvoiceitemsids)
        {
            SortedDictionary<long, decimal> result = new SortedDictionary<long, decimal>();
            if (lrinvoiceitemsids == null || lrinvoiceitemsids.Count == 0)
                return result;

            string query = string.Format(@"select sum(bominvoicereturnitems.rate) discount,bominvoiceitemsid 
                            from bominvoicereturnitems
                            inner join bominvoicereturn on bominvoicereturn.id = bominvoicereturnitems.bominvoicereturnid
                            where bominvoicereturnitems.isactive = true and bominvoicereturn.isactive = true and bominvoiceitemsid in ({0}) and 
                            bominvoicereturn.returntype = 5 and bominvoicereturnitems.qty>0 and bominvoicereturnitems.rate>0
                            group by bominvoiceitemsid", string.Join(",", lrinvoiceitemsids));
            using (var conn = CreateConnection())
            {
                conn.Open();
                var command = GetCommand(query, conn);

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(reader.GetInt64(1), reader.GetDecimal(0));
                    }
                }
                conn.Close();
            }
            return result;
        }
        public bool UpdatePOItemQty(NpgsqlTransaction transaction, NpgsqlConnection conn, long id, bool isAdd)
        {

            NpgsqlCommand command = GetCommand(string.Format(SQL_UpdatePOItems, isAdd ? "+" : "-"), conn, transaction);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = id;
            return command.ExecuteNonQuery() > 0;
        }

        public bool DeleteAll(long bominvoiceid, long userid, NpgsqlConnection conn, NpgsqlTransaction transaction)
        {

            var lst = SearchAll(bominvoiceid);
            int successCount = 0;
            NpgsqlCommand command = new NpgsqlCommand(SQL_DeleteAll, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = false;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = userid;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("bominvoiceid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = bominvoiceid;
            successCount = command.ExecuteNonQuery();
            if(successCount>0)
            {
                foreach (var item in lst)
                    UpdatePOItemQty(transaction, conn, item.id, false);
            }
            return successCount > 0;
        }

    }

}
