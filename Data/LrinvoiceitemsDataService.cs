using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
using InfyPOS.Services;
using System.Data;
using ClosedXML.Excel;

namespace InfyPOS.Data

{
    public class LrinvoiceitemsDataService : SAASData
    {
        const string SQL_LastTrans = @"select productid, pricingid, buyingqty,freeqty, buyingprice, mrp,rowexpense, rowexpensevalue, sellingprice, purchasetotaldiscount,
                                    lrinvoiceitems.id, lrinvoiceitems.isactive,isseconds, lrinvoiceitems.createdby, lrinvoiceitems.modifiedby, lrinvoiceitems.version, 
                                    lrinvoiceitems.createdon, lrinvoiceitems.modifiedon, itemid, damagedqty, lrinvoiceitems.amount,wsrate,dealerprice,
                                    sellableqty,lrinvoiceitems.basebuyingprice, lrinvoiceid,stocklocationid, uan, purchasediscountpercentage,taxid,purchasetax,barcodes,designid,itemdetail,sno,purchasediscount,
                                    hsncode,salediscount,imgid,weight,workingcharge,wastage,weightrate,cesstaxid,cesstax,cesspercentage,
                                    salediscountpercentage,marginpercentage,salestaxpercentage,purchasetaxpercentage,salestax,ptaxid,poinfo,priceinfo
                                    from lrinvoiceitems
									inner join lrinvoice on lrinvoice.id = lrinvoiceitems.lrinvoiceid and lrinvoice.isactive = true";

        const string SQL_SearchByDesign = @"select productid, pricingid, buyingqty,freeqty, buyingprice, mrp,rowexpense, rowexpensevalue, purchasetotaldiscount, sellingprice, lrinvoiceitems.id, 
                                    lrinvoiceitems.isactive, lrinvoiceitems.isseconds, lrinvoiceitems.createdby, lrinvoiceitems.modifiedby, lrinvoiceitems.version, lrinvoiceitems.createdon, lrinvoiceitems.amount, 
                                    lrinvoiceitems.modifiedon, lrinvoiceitems.itemid, damagedqty, sellableqty, lrinvoiceitems.basebuyingprice,ptaxid,wsrate,dealerprice,
                                    lrinvoiceid,stocklocationid,uan, lrinvoiceitems.purchasediscountpercentage,taxid,purchasetax,barcodes,designid,itemdetail,sno,purchasediscount,
                                    hsncode,salediscount,imgid,weight,workingcharge,wastage,weightrate,cesstaxid,cesstax,cesspercentage,
                                    salediscountpercentage,marginpercentage,salestaxpercentage,purchasetaxpercentage,salestax,poinfo,priceinfo from lrinvoiceitems
                                    inner join lrinvoice on lrinvoice.id = lrinvoiceid  and lrinvoice.isactive = true
                                    where designid=@designid and suppliercompanyid=@suppliercompanyid and lrinvoiceitems.productid=@productid and lrinvoiceitems.isactive=true
                                    order by id desc
                                    limit 1";

        const string SQL_SearchByDesignList = @"select distinct lrinvoiceitems.designid from lrinvoiceitems
                                    inner join lrinvoice on lrinvoice.id = lrinvoiceid  and lrinvoice.isactive = true
                                    where suppliercompanyid=@suppliercompanyid and lrinvoiceitems.productid=@productid 
                                    and lrinvoiceitems.isactive=true";

        public bool ispricing { get; set; }
        internal List<BarcodeImageResponse> SearchBarcodeImages(BarcodeImageRequest item)
        {
            List<BarcodeImageResponse> lst = new List<BarcodeImageResponse>();
            var conn = CreateConnection();
            try
            {
                conn.Open();
                QueryBuilder qb = new QueryBuilder();
                qb.CurrentQuery = @"select lrinvoiceitems.id,barcode,imgid from lrinvoiceitems 
                                left outer join stock on stock.invoiceitemid = lrinvoiceitems.id";
                qb.AddInParamer("lrinvoiceitems.id", string.Join<long>(",", item.ids));

                var command = qb.GetCommand(conn);

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        BarcodeImageResponse olrinvoiceitems = new BarcodeImageResponse();
                        olrinvoiceitems.id = reader.GetInt64(0);
                        olrinvoiceitems.barcode = reader.IsDBNull(1) ? "" : reader.GetString(1);
                        olrinvoiceitems.imgid = reader.IsDBNull(2) ? "" : reader.GetString(2);
                        lst.Add(olrinvoiceitems);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                conn.Close();
            }
            return lst;
        }

        internal bool ChangeStockLocation(List<LrinvoiceItemDetail> items)
        {

            var conn = CreateConnection();
            try
            {
                conn.Open();
                var transaction = conn.BeginTransaction();
                string query = @"update lrinvoiceitems set stocklocationid=@stocklocationid,modifiedon=@modifiedon,modifiedby=@modifiedby where id=@invoiceitemid;";

                string stquery = @"update lrinvoiceitems set stocklocationid=@stocklocationid,modifiedon=@modifiedon,modifiedby=@modifiedby where id=@invoiceitemid;
                                   update stock set retaillocationid = @stocklocationid where invoiceitemid=@invoiceitemid
                                   update stockuan set retaillocationid = @stocklocationid where invoiceitemid=@invoiceitemid";

                NpgsqlCommand command = GetCommand(query, conn, transaction);
                command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
                command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                command.Parameters.Add("invoiceitemid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = 0;
                command.Parameters.Add("stocklocationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = 0;
                foreach (var item in items)
                {
                    if (item.sellableqty > 0)
                        command.CommandText = stquery;
                    else
                        command.CommandText = query;
                    command.Parameters["invoiceitemid"].Value = item.id;
                    command.Parameters["stocklocationid"].Value = item.stocklocationid;
                    command.ExecuteNonQuery();
                }

                transaction.Commit();
                conn.Close();
                return true;
            }
            catch (Exception exp)
            {
                if (conn != null)
                    conn.Close();

                return false;
            }
        }


        /*

        select lrinvoice.CREATEDON,suppliercompany.name supplier,lrentry.lrno,lrentry.lrentryno,lrinvoice.invoicedate,
        lrinvoice.invoiceno,lrinvoice.pono,
        case when lrinvoice.settled then 'OLD STOCK' else 'NEW GRN' end from lrinvoice
        inner join suppliercompany on suppliercompany.id = lrinvoice.suppliercompanyid
        inner join lrentry on lrentry.id = lrid
        where lrinvoice.isactive=true and lrentry.isactive=true


        with missingpoid 
        as
        (
        select cast(schemelist->>'pono' as varchar) pono,lrinvoice.id,purchaseorderitems.purchaseorderid from lrinvoiceitems
        LEFT OUTER JOIN LATERAL json_array_elements (case when lrinvoiceitems.poinfo is null 
        or lrinvoiceitems.poinfo::text ='null' then '[]'::json else lrinvoiceitems.poinfo::json end) schemelist ON true
        inner join lrinvoice on lrinvoice.id = lrinvoiceitems.lrinvoiceid
        inner join purchaseorderitems on purchaseorderitems.id = cast(schemelist->>'purchaseorderitemsid' as bigint)
        where lrinvoice.poid = 0 and poinfo is not null and poinfo->0 is not null
        )
        update lrinvoice set poid = purchaseorderid,pono=missingpoid.pono from missingpoid
        where missingpoid.id =lrinvoice.id

        */

        const string SQL_UpdatePOItems = @"
                                    with polist as
                                    (select lrinvoiceitems.id,cast(schemelist ->> 'purchaseorderitemsid' as bigint) purchaseorderitemsid,
                                    cast(schemelist ->> 'aqty' as numeric) aqty from lrinvoiceitems
                                    LEFT OUTER JOIN LATERAL json_array_elements (case when lrinvoiceitems.poinfo is null 
																				or lrinvoiceitems.poinfo::text ='null' then '[]'::json else lrinvoiceitems.poinfo::json end) schemelist ON true
                                    where lrinvoiceitems.id = @id and cast(schemelist ->> 'purchaseorderitemsid' as bigint) is not null)

                                    update purchaseorderitems set rqty = rqty{0}polist.aqty from polist
                                    where polist.purchaseorderitemsid = purchaseorderitems.id";


        internal bool DeleteBarcode(List<long> items)
        {
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                var transaction = conn.BeginTransaction();

                var lrinvoiceitems = Search(new LrinvoiceitemsCreteria() { ids = items });

                string query = @"update lrinvoiceitems set barcodes='null',barcodedon=null,sellableqty=0,modifiedon=@modifiedon,modifiedby=@modifiedby where id=@invoiceitemid;
                                delete from salablegoods using stock where salablegoods.stockid = stock.id and invoiceitemid=@invoiceitemid;
                                delete from stock where invoiceitemid=@invoiceitemid;
                                delete from sellablebarcodes using stock
                                where sellablebarcodes.stockid = stock.id and invoiceitemid=@invoiceitemid;";

                if (!InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_ismobileapplication &&
                    !InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_ismfgapplication)
                {
                    query = @"update lrinvoiceitems set barcodes='null',barcodedon=null,sellableqty=0,modifiedon=@modifiedon,modifiedby=@modifiedby where id=@invoiceitemid;
                                update salablegoods set isactive = false,modifiedon=@modifiedon,modifiedby=@modifiedby from stock where salablegoods.stockid = stock.id and invoiceitemid=@invoiceitemid;
                                update stock set isactive =false,modifiedon=@modifiedon,modifiedby=@modifiedby where invoiceitemid=@invoiceitemid;
                                update sellablebarcodes set isactive = true,modifiedon=@modifiedon,modifiedby=@modifiedby from stock
                                where sellablebarcodes.stockid = stock.id and invoiceitemid=@invoiceitemid;";
                }

                string dquery = @"select sum(oqty) from salablegoods 
                                inner join stock on stock.id = salablegoods.stockid
                                where invoiceitemid=@invoiceitemid;";

                var command = GetCommand(query, conn, transaction);
                command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
                command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                command.Parameters.Add("invoiceitemid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = 0;

                foreach (var item in items)
                {
                    string serialno = "";
                    using (var reader = ExecuteReader("select itemdetail->>'serialno' from lrinvoiceitems where id=" + item, null, conn, transaction))
                    {
                        if (reader.Read())
                        {
                            serialno = reader[0] == DBNull.Value ? "" : reader.GetString(0);
                        }
                    }

                    if (!string.IsNullOrEmpty(serialno) &&
                        InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_hyper_uan)
                    {
                        StockUANDataService stockUANDataService = new StockUANDataService() { Context = this.Context };
                        var uanstocks = stockUANDataService.Search(new StockuanCreteria()
                        {
                            invoiceitemid = item
                        });

                        var inventoryqty = uanstocks.Sum(x => x.qty);
                        var transferedqty = uanstocks.Sum(x => x.transfered);
                        if (uanstocks.Count > 0)
                        {
                            long stockid = uanstocks.First().stockid;
                            //if (transferedqty != 0 && inventoryqty > transferedqty)
                            //{
                            //    throw new Exception("Can't remove the barcodes which are contains partial transfers or UAN stock to retail location (" + serialno + ")");
                            //}
                            if (uanstocks.Count > 0)
                            {
                                var uanstock = uanstocks.Sum(x => x.qty);
                                var warehouseqty = 0;
                                var warehousetransfered = 0;
                                using (var reader = ExecuteReader("select qty,transfered from stock where id=" + stockid + " and isactive=true", null, conn, transaction))
                                {
                                    if (reader.Read())
                                    {
                                        warehouseqty = reader[0] == DBNull.Value ? 0 : reader.GetInt32(0);
                                        warehousetransfered = reader[1] == DBNull.Value ? 0 : reader.GetInt32(1);
                                    }
                                }
                                if (inventoryqty <= (warehouseqty - warehousetransfered))
                                {
                                    var uquery = @"update lrinvoiceitems set barcodes='null',barcodedon=null,sellableqty=0,modifiedon=@modifiedon,modifiedby=@modifiedby,version=version+1 where id=@invoiceitemid;
                                update stockuan set isactive=false,modifiedon=@modifiedon,modifiedby=@modifiedby,version=version+1 where invoiceitemid=@invoiceitemid;
                                update stock set qty=qty-@qty,modifiedon=@modifiedon,modifiedby=@modifiedby,version=version+1 where id=@stockid;";
                                    var ucommand = GetCommand(uquery, conn, transaction);
                                    ucommand.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
                                    ucommand.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                                    ucommand.Parameters.Add("invoiceitemid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item;
                                    ucommand.Parameters.Add("stockid", NpgsqlTypes.NpgsqlDbType.Numeric).Value = stockid;
                                    ucommand.Parameters.Add("qty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = inventoryqty;
                                    ucommand.ExecuteNonQuery();
                                    continue;
                                }
                                throw new Exception("Can't remove the barcodes which are contains partial transfers or UAN stock to retail location (" + serialno + ")");
                            }
                            //else if(transferedqty > 0)
                            //{
                            //    var retailstock = 0;
                            //    var retailcount = 0;
                            //    using (var reader = ExecuteReader("select count(*),sum(iqty) from salablegoods where stockid=" + uanstocks.First().stockid + " and isactive=true", null, conn, transaction))
                            //    {
                            //        if (reader.Read())
                            //        {
                            //            retailcount = reader[0] == DBNull.Value ? 0 : reader.GetInt32(0);
                            //            retailstock = reader[1] == DBNull.Value ? 0 : reader.GetInt32(1);
                            //        }
                            //    }
                            //    if(retailcount==1 && retailstock>=inventoryqty)
                            //    {
                            //        var uquery = @"update lrinvoiceitems set barcodes='null',sellableqty=0,modifiedon=@modifiedon,modifiedby=@modifiedby where id=@invoiceitemid;
                            //                       update stock set qty=qty-@qty,transfered=transfered-@transfered where id=@id;
                            //                       update salablegoods set iqty=iqty-@iqty where salablegoods.stockid = @stockid;";
                            //        var ucommand = GetCommand(uquery, conn, transaction);
                            //        ucommand.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
                            //        ucommand.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                            //        ucommand.Parameters.Add("invoiceitemid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item;
                            //        ucommand.Parameters.Add("invoiceitemid", NpgsqlTypes.NpgsqlDbType.Numeric).Value = inventoryqty;
                            //        ucommand.ExecuteNonQuery();
                            //        continue;
                            //    }
                            //}
                            throw new Exception("Can't remove the UAN barcodes it has conflict, try to adjust stock qty on audit.");
                        }

                    }
                    else
                    {

                        command.Parameters["invoiceitemid"].Value = item;
                        command.CommandText = dquery;
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                if (!reader.IsDBNull(0) && Convert.ToInt32(reader[0]) > 0)
                                {
                                    throw new Exception("Can't remove the barcodes which are sold already.");
                                }
                            }
                        }

                        command.CommandText = query;
                        command.ExecuteNonQuery();
                        if (!string.IsNullOrEmpty(serialno))
                        {
                            var ucommand = GetCommand("Update stockuan set isactive=false,modifiedon=@modifiedon,modifiedby=@modifiedby where invoiceitemid=@invoiceitemid", conn, transaction);
                            ucommand.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
                            ucommand.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                            ucommand.Parameters.Add("invoiceitemid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item;
                            ucommand.ExecuteNonQuery();
                        }

                    }
                }

                StocktransactionDataService stocktransactionDataServicev = new StocktransactionDataService() { Context = this.Context };
                stocktransactionDataServicev.DeleteInvoiceItemBarcode(items, transaction, conn);
                transaction.Commit();
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, items);
                return false;
            }
            return true;
        }

        internal List<LrinvoiceItemDetail> UpdatePrice(List<LrinvoiceItemDetail> items)
        {
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                QueryBuilder qb = new QueryBuilder();
                qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                qb.AddParameter("isactive", true, "=");
                qb.AddInParamer("id", string.Join<long>(",", items.ConvertAll(e => e.id)));
                command = qb.GetCommand(conn);
                List<LrinvoiceItemDetail> lst = new List<LrinvoiceItemDetail>();
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        LrinvoiceItemDetail olrinvoiceitems = new LrinvoiceItemDetail();
                        olrinvoiceitems.productid = reader["productid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productid"].ToString());
                        olrinvoiceitems.buyingqty = reader["buyingqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["buyingqty"].ToString());
                        olrinvoiceitems.freeqty = reader["freeqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["freeqty"].ToString());
                        olrinvoiceitems.buyingprice = reader["buyingprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["buyingprice"].ToString());
                        olrinvoiceitems.mrp = reader["mrp"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["mrp"].ToString());
                        olrinvoiceitems.rowexpense = reader["rowexpense"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["rowexpense"].ToString());
                        olrinvoiceitems.rowexpensevalue = reader["rowexpensevalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["rowexpensevalue"].ToString());
                        olrinvoiceitems.purchasetotaldiscount = reader["purchasetotaldiscount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["purchasetotaldiscount"].ToString());
                        olrinvoiceitems.purchasediscount = reader["purchasediscount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["purchasediscount"].ToString());
                        olrinvoiceitems.salediscountpercentage = reader["salediscountpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["salediscountpercentage"].ToString());
                        olrinvoiceitems.marginpercentage = reader["marginpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["marginpercentage"].ToString());
                        olrinvoiceitems.purchasediscountpercentage = reader["purchasediscountpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["purchasediscountpercentage"].ToString());
                        olrinvoiceitems.salediscount = reader["salediscount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["salediscount"].ToString());
                        olrinvoiceitems.salestax = reader["salestax"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["salestax"].ToString());
                        olrinvoiceitems.purchasetaxpercentage = reader["purchasetaxpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["purchasetaxpercentage"].ToString());
                        olrinvoiceitems.salestaxpercentage = reader["salestaxpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["salestaxpercentage"].ToString());


                        olrinvoiceitems.sellingprice = reader["sellingprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["sellingprice"].ToString());
                        olrinvoiceitems.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        olrinvoiceitems.sno = reader["sno"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["sno"]);
                        olrinvoiceitems.uan = reader["uan"] == DBNull.Value ? "" : Convert.ToString(reader["uan"]);

                        olrinvoiceitems.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        olrinvoiceitems.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"].ToString());
                        olrinvoiceitems.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        olrinvoiceitems.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        olrinvoiceitems.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        olrinvoiceitems.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        olrinvoiceitems.itemid = reader["itemid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["itemid"].ToString());
                        olrinvoiceitems.damagedqty = reader["damagedqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["damagedqty"].ToString());
                        olrinvoiceitems.sellableqty = reader["sellableqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["sellableqty"].ToString());
                        olrinvoiceitems.basebuyingprice = reader["basebuyingprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["basebuyingprice"].ToString());
                        olrinvoiceitems.lrinvoiceid = reader["lrinvoiceid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["lrinvoiceid"].ToString());
                        olrinvoiceitems.stocklocationid = reader["stocklocationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["stocklocationid"].ToString());
                        olrinvoiceitems.purchasetax = reader["purchasetax"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["purchasetax"].ToString());
                        olrinvoiceitems.taxid = reader["taxid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["taxid"].ToString());
                        olrinvoiceitems.ptaxid = reader["ptaxid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["ptaxid"].ToString());
                        if (olrinvoiceitems.ptaxid == 0 && olrinvoiceitems.taxid > 0)
                            olrinvoiceitems.ptaxid = olrinvoiceitems.taxid;
                        olrinvoiceitems.barcodes = reader["barcodes"] == DBNull.Value ? "null" : reader["barcodes"].ToString();
                        olrinvoiceitems.designid = reader["designid"] == DBNull.Value ? "" : reader["designid"].ToString();
                        olrinvoiceitems.hsncode = reader["hsncode"] == DBNull.Value ? "" : reader["hsncode"].ToString();
                        olrinvoiceitems.imgid = reader["imgid"] == DBNull.Value ? "" : reader["imgid"].ToString();
                        olrinvoiceitems.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"].ToString());
                        olrinvoiceitems.itemdetail = reader["itemdetail"] == DBNull.Value ? "null" : reader["itemdetail"].ToString();
                        olrinvoiceitems.POItemInfo = reader["poinfo"] == DBNull.Value ? "null" : reader["poinfo"].ToString();
                        olrinvoiceitems.PriceItemInfo = reader["priceinfo"] == DBNull.Value ? "null" : reader["priceinfo"].ToString();
                        olrinvoiceitems.amount = reader["amount"] == DBNull.Value ? ((olrinvoiceitems.buyingprice * olrinvoiceitems.buyingqty - olrinvoiceitems.purchasediscount)) : Convert.ToDecimal(reader["amount"].ToString());
                        lst.Add(olrinvoiceitems);
                    }
                }

                if (Services.ConfigManager.Instance(Context.OrganizationID).Config.system_disableupdateonbarcode)
                {
                    if (lst.Exists(e => e.sellableqty > 0)) throw new Exception("Updatation disabled after barcode generation.");
                }

                string priceChangeQuery = SQL_UpdatePrice;
                if (Services.ConfigManager.Instance(Context.OrganizationID).Config.system_syncshowroomprice)
                {
                    priceChangeQuery = SQL_UpdatePrice + @";update salablegoods set price=@price,discount=@discount,modifiedby=@modifiedby,modifiedon=@modifiedon  from stock
                                                            where salablegoods.stockid = stock.id and stock.invoiceitemid = @invoiceitemid;";
                }
                var transaction = conn.BeginTransaction();
                foreach (var item in lst)
                {
                    var priceitem = items.Find(e => e.id == item.id);
                    item.mrp = priceitem.mrp;
                    item.salediscount = priceitem.salediscount;
                    item.salediscountpercentage = priceitem.salediscountpercentage;
                    item.salestax = priceitem.salestax;
                    item.salestaxpercentage = priceitem.salestaxpercentage;
                    item.sellingprice = priceitem.sellingprice;
                    item.marginpercentage = priceitem.marginpercentage;
                    Update(transaction, conn, item);
                    if (!string.IsNullOrEmpty(item.barcodes))
                    {
                        command = GetCommand(priceChangeQuery, conn, transaction);
                        command.Parameters.Add("discount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = item.salediscount;
                        command.Parameters.Add("price", NpgsqlTypes.NpgsqlDbType.Numeric).Value = item.sellingprice;
                        command.Parameters.Add("displaymrp", NpgsqlTypes.NpgsqlDbType.Numeric).Value = item.mrp;
                        command.Parameters.Add("invoiceitemid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = priceitem.id;
                        command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                        command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
                        command.ExecuteNonQuery();
                    }
                }
                transaction.Commit();
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, items);
            }
            return items;
        }

        internal bool SwapBarcode(SwapBarcode item)
        {
            decimal fqty = 0;
            decimal fiqty = 0;
            decimal fstockqty = 0;
            decimal fprice = 0;
            long finvoiceitemid = 0;
            long fstockid = 0;
            long fid = 0;

            decimal tqty = 0;
            decimal tiqty = 0;
            decimal tstockqty = 0;
            decimal tprice = 0;
            long tinvoiceitemid = 0;
            long tstockid = 0;
            long tid = 0;

            var param = new SortedList<string, object>();
            string query = @"select stock.qty,salablegoods.iqty,stock.id stockid,salablegoods.id,salablegoods.price-salablegoods.discount price,
                            salablegoods.iqty-(salablegoods.oqty+salablegoods.jqty+salablegoods.dqty) stockqty,invoiceitemid from salablegoods
                            inner join stock on stock.id = stockid
                            where salablegoods.barcode=@barcode and salablegoods.locationid=@locationid";
            param.Add("barcode", item.frombarcode.barcode);
            param.Add("locationid", item.locationid);
            using (var reader = ExecuteReader(query, param))
            {
                if (reader.Read())
                {
                    fqty = reader["qty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["qty"]);
                    fiqty = reader["iqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["iqty"]);
                    fstockqty = reader["stockqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["stockqty"]);
                    fprice = reader["price"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["price"]);
                    finvoiceitemid = reader["invoiceitemid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["invoiceitemid"]);
                    fstockid = reader["stockid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["stockid"]);
                    fid = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                }
            }
            param.Clear();
            param.Add("barcode", item.tobarcode.barcode);
            param.Add("locationid", item.locationid);
            using (var reader = ExecuteReader(query, param))
            {
                if (reader.Read())
                {
                    tqty = reader["qty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["qty"]);
                    tiqty = reader["iqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["iqty"]);
                    tstockqty = reader["stockqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["stockqty"]);
                    tprice = reader["price"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["price"]);
                    tinvoiceitemid = reader["invoiceitemid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["invoiceitemid"]);
                    tstockid = reader["stockid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["stockid"]);
                    tid = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                }
            }
            if (fqty != 1)
                throw new Exception(item.frombarcode.barcode + " contains " + fqty + " it should not exceed 1");

            if (tqty != 1)
                throw new Exception(item.tobarcode.barcode + " contains " + tqty + " it should not exceed 1");

            if (tprice != fprice)
                throw new Exception("Both barcode should be same price");

            using (NpgsqlConnection conn = CreateConnection())
            {
                conn.Open();
                var transaction = conn.BeginTransaction();

                query = @"update lrinvoiceitems set barcodes=@frombarcodes,isseconds=true where id=@frominvoiceitemid;
                    update lrinvoiceitems set barcodes=@tobarcodes,isseconds=true where id=@toinvoiceitemid;
                    update stock set barcode=@tempfromcode where id=@fromstockid;
                    update stock set barcode=@temptocode where id=@tostockid;
                    update stock set barcode=@fromcode where id=@fromstockid;
                    update stock set barcode=@tocode where id=@tostockid;
                    update salablegoods set barcode=@tempfromcode where id=@fromid;
                    update salablegoods set barcode=@temptocode where id=@toid;
                    update salablegoods set barcode=@fromcode where id=@fromid;
                    update salablegoods set barcode=@tocode where id=@toid;";
                NpgsqlCommand command = GetCommand(query, conn, transaction);
                command.Parameters.Add("tobarcodes", NpgsqlTypes.NpgsqlDbType.Json).Value = Newtonsoft.Json.JsonConvert.SerializeObject(new List<string>(new string[] { item.frombarcode.barcode }));
                command.Parameters.Add("frombarcodes", NpgsqlTypes.NpgsqlDbType.Json).Value = Newtonsoft.Json.JsonConvert.SerializeObject(new List<string>(new string[] { item.tobarcode.barcode }));
                command.Parameters.Add("frominvoiceitemid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = finvoiceitemid;
                command.Parameters.Add("toinvoiceitemid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = tinvoiceitemid;
                command.Parameters.Add("fromid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = fid;
                command.Parameters.Add("toid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = tid;
                command.Parameters.Add("fromstockid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = fstockid;
                command.Parameters.Add("tostockid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = tstockid;
                command.Parameters.Add("tocode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = item.frombarcode.barcode;
                command.Parameters.Add("fromcode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = item.tobarcode.barcode;
                command.Parameters.Add("temptocode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = "#" + item.frombarcode.barcode;
                command.Parameters.Add("tempfromcode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = "#" + item.tobarcode.barcode;
                command.ExecuteNonQuery();

                transaction.Commit();
                conn.Close();
            }
            return true;
        }

        internal LrinvoiceItemDetail UpdateDamagedQty(LrinvoiceItemDetail item)
        {
            var existingitem = Search(new LrinvoiceitemsCreteria()
            {
                id = item.id
            });
            if (existingitem.Count == 1)
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    var transaction = conn.BeginTransaction();
                    string query = "Update lrinvoiceitems set actualqty=buyingqty-@damagedqty where id=@id";

                    NpgsqlCommand command = GetCommand(query, conn, transaction);
                    command.Parameters.Add("damagedqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = item.damagedquantity;
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.id;
                    command.ExecuteNonQuery();

                    InfyPOS.Services.IntrayManager.Instance(Context.OrganizationID).Add(Context.OrganizationID, Services.IntrayManager.EntryType.Notification,
                    item.lrinvoiceid, "INVOICE QTY SHORTAGE - " + item.Details.productname + " QTY " + item.damagedquantity.ToString()
                    + "/" + item.buyingqty.ToString() + " ON " + item.invoiceinformation,
                    Context.UserID, 0, transaction, Context);

                    transaction.Commit();
                    conn.Close();
                }

                LrinvoicereturnDataService lrinvoicereturnDataService = new LrinvoicereturnDataService() { Context = this.Context };
                lrinvoicereturnDataService.Context = this.Context;
                lrinvoicereturnDataService.AddDirectDebitNote(item, item.damagedquantity, "Shortage qty received");
                return existingitem[0];
            }
            return null;
        }

        internal SortedDictionary<long, decimal> SearchAllDiscount(List<long> lrinvoiceitemsids)
        {
            SortedDictionary<long, decimal> result = new SortedDictionary<long, decimal>();
            if (lrinvoiceitemsids == null || lrinvoiceitemsids.Count == 0)
                return result;

            string query = string.Format(@"select sum(lrinvoicereturnitems.rate) discount,lrinvoiceitemsid 
                            from lrinvoicereturnitems
                            inner join lrinvoicereturn on lrinvoicereturn.id = lrinvoicereturnitems.lrinvoicereturnid
                            where lrinvoicereturnitems.isactive = true and lrinvoicereturn.isactive = true and lrinvoiceitemsid in ({0}) and lrinvoicereturn.returntype = 5 and lrinvoicereturnitems.qty>0 and lrinvoicereturnitems.rate>0
                            group by lrinvoiceitemsid", string.Join(",", lrinvoiceitemsids));
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

        internal LrinvoiceItemDetail UpdateActualQty(LrinvoiceItemDetail item)
        {
            var existingitem = Search(new LrinvoiceitemsCreteria()
            {
                id = item.id
            });
            if (existingitem.Count == 1)
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    var transaction = conn.BeginTransaction();
                    string query = "Update lrinvoiceitems set actualqty = @actualqty where id=@id";

                    NpgsqlCommand command = GetCommand(query, conn, transaction);
                    command.Parameters.Add("actualqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = item.actualqty;
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.id;
                    command.ExecuteNonQuery();

                    InfyPOS.Services.IntrayManager.Instance(Context.OrganizationID).Add(Context.OrganizationID, Services.IntrayManager.EntryType.Notification,
                    item.lrinvoiceid, "INVOICE QTY MISMATCH - " + item.Details.productname + " RECEIVED " + item.actualqty.ToString()
                    + "/" + item.buyingqty.ToString() + " ON " + item.invoiceinformation,
                    Context.UserID, 0, transaction, Context);
                    transaction.Commit();
                    conn.Close();
                }
                if (existingitem[0].buyingqty - item.actualqty > 0)
                {
                    LrinvoicereturnDataService lrinvoicereturnDataService = new LrinvoicereturnDataService() { Context = this.Context };
                    lrinvoicereturnDataService.Context = this.Context;
                    lrinvoicereturnDataService.AddDirectDebitNote(item, existingitem[0].buyingqty - item.actualqty, "Received different qty");
                }
                return existingitem[0];
            }
            return null;
        }

        internal List<SplitBarcode> ConvertItem(List<SplitBarcode> obarcodereport)
        {
            if (obarcodereport.Exists(e => e.mode == 5))
            {
                foreach (var item in obarcodereport)
                {
                    item.newbarcodelist = item.barcode;
                    item.barcodes = item.barcode;
                    item.barcode = "";
                    item.splitqty = 1;
                    item.splitpiece = 1;
                    if (item.retailstock != null && item.retailstock.First().locationid > 0)
                        item.locationid = item.retailstock.First().locationid;
                }
            }
            else if (obarcodereport.Exists(e => e.mode == 7))
            {
                Items items = null;
                List<long> itemid = obarcodereport.ConvertAll(e => e.itemid);
                if (itemid.Count > 0)
                {
                    ItemsDataService itemsDataService = new ItemsDataService() { Context = this.Context };
                    var itemslist = itemsDataService.Search(new ItemsCreteria()
                    {
                        id = itemid[0]
                    });
                    if (itemslist.Count > 0)
                        items = itemslist[0];
                }

                ReferencelistDataService referencelistDataService = new ReferencelistDataService() { Context = this.Context };
                long sizeid = 0;
                var param = new SortedList<string, object>();
                param.Add("name", obarcodereport[0].splitqty.ToString("F2"));
                using (var reader = ExecuteReader(@"select id from referencelist where type ='SIZE'
                                    and name=@name", param))
                {
                    if (reader.Read())
                    {
                        sizeid = reader.GetInt64(0);
                    }
                }
                if (sizeid == 0)
                {
                    sizeid = referencelistDataService.Insert(new Referencelist()
                    {
                        createdby = Context.UserID,
                        modifiedby = Context.UserID,
                        createdon = DateTimeNow,
                        modifiedon = DateTimeNow,
                        isactive = true,
                        type = "SIZE",
                        code = obarcodereport[0].splitqty.ToString("F2"),
                        name = obarcodereport[0].splitqty.ToString("F2"),
                        additionalinfo = new Referencelist.Attribute()
                        {
                            issales = true
                        }
                    }).id;
                }

                foreach (BarcodeReport splititem in obarcodereport)
                {
                    if (splititem.locationid == 0 && splititem.wstock < (splititem.splitqty * splititem.splitpiece))
                        throw new Exception("There is no sufficient qty in warehouse");
                    if (splititem.locationid > 0 && splititem.rstock < (splititem.splitqty * splititem.splitpiece))
                        throw new Exception("There is no sufficient qty in retail showroom");

                    ProductsDataService productdataservice = new ProductsDataService() { Context = this.Context };
                    var products = productdataservice.Search(new ProductsCreteria()
                    {
                        id = splititem.changeproductid
                    });

                    {
                        items.productid = splititem.productid;
                        items.sizeid = sizeid;
                        LRInvoiceItemBitChange item = new LRInvoiceItemBitChange();
                        item.Core = new LrinvoiceItemDetail();
                        item.Core.id = splititem.lrinvoiceitemid;
                        item.Core.productid = splititem.productid;
                        item.Core.Details = new Items()
                        {
                            productid = splititem.productid,
                            sizeid = sizeid
                        };
                        item.barcode = splititem.barcode;
                        item.locationid = splititem.locationid;
                        item.companyid = splititem.companyid;
                        item.IsBit = true;
                        item.items = new List<LrinvoiceItemDetail>();
                        if (splititem.splitpiece == 0)
                            splititem.splitpiece = 1;

                        if (products[0].sellingmode == 1)
                        {
                            for (int i = 1; i <= splititem.splitpiece; i++)
                            {
                                item.items.Add(new LrinvoiceItemDetail()
                                {
                                    barcode = splititem.barcode,
                                    sellableqty = 1,
                                    sizeinmeter = splititem.splitqty,
                                    convertableqty = splititem.splitqty * 1,
                                    sellingprice = splititem.nprice,
                                    salediscount = splititem.ndiscount,
                                    salediscountpercentage = splititem.npdiscount,
                                    productid = splititem.changeproductid,
                                    productname = splititem.productname,
                                    Details = items
                                });
                            }
                            splititem.splitpiece = 1;
                        }
                        else
                        {

                            item.items.Add(new LrinvoiceItemDetail()
                            {
                                barcode = splititem.barcode,
                                sellableqty = splititem.splitpiece,
                                sizeinmeter = splititem.splitqty,
                                convertableqty = splititem.splitqty * splititem.splitpiece,
                                sellingprice = splititem.nprice,
                                salediscount = splititem.ndiscount,
                                salediscountpercentage = splititem.npdiscount,
                                productid = splititem.changeproductid,
                                productname = splititem.productname,
                                Details = items
                            });
                        }

                        List<Stock> lststock = ConvertItem(item);
                        if (lststock.Count > 0)
                        {
                            splititem.barcodes = lststock[0].newbarcode;
                            splititem.newbarcodelist = string.Join(",", lststock.ConvertAll(e => e.barcode));
                            splititem.newbarcode = lststock.OrderBy(e => e.id).Last().barcode;
                        }
                    }
                }

                SplitbarcodehistoryDataService splitbarcodehistoryDataService = new SplitbarcodehistoryDataService() { Context = this.Context };
                splitbarcodehistoryDataService.Context = this.Context;
                splitbarcodehistoryDataService.Save(obarcodereport);
            }
            else
            {
                Items items = null;
                List<long> itemid = obarcodereport.FindAll(e => e.changeproductid > 0).ConvertAll(e => e.itemid);
                if (itemid.Count > 0)
                {
                    ItemsDataService itemsDataService = new ItemsDataService() { Context = this.Context };
                    var itemslist = itemsDataService.Search(new ItemsCreteria()
                    {
                        id = itemid[0]
                    });
                    if (itemslist.Count > 0)
                        items = itemslist[0];
                }

                foreach (BarcodeReport splititem in obarcodereport)
                {
                    if (splititem.locationid == 0 && splititem.wstock < (splititem.splitqty * splititem.splitpiece))
                        throw new Exception("There is no sufficient qty in warehouse");
                    if (splititem.locationid > 0 && splititem.rstock < (splititem.splitqty * splititem.splitpiece))
                        throw new Exception("There is no sufficient qty in retail showroom");

                    {
                        LRInvoiceItemBitChange item = new LRInvoiceItemBitChange();
                        item.Core = new LrinvoiceItemDetail();
                        item.Core.id = splititem.lrinvoiceitemid;
                        item.Core.productid = splititem.productid;
                        item.barcode = splititem.barcode;
                        item.locationid = splititem.locationid;
                        item.companyid = splititem.companyid;
                        item.IsBit = splititem.isbit;
                        item.items = new List<LrinvoiceItemDetail>();
                        if (splititem.splitpiece == 0)
                            splititem.splitpiece = 1;
                        for (int i = 0; i < splititem.splitpiece; i++)
                        {
                            item.items.Add(new LrinvoiceItemDetail()
                            {
                                barcode = splititem.barcode,
                                sellableqty = splititem.splitqty,
                                sellingprice = splititem.nprice,
                                salediscount = splititem.ndiscount,
                                salediscountpercentage = splititem.npdiscount,
                                productid = splititem.changeproductid > 0 ? splititem.changeproductid : splititem.productid,
                                productname = splititem.productname,
                                Details = items
                            });
                        }
                        List<Stock> lststock = ConvertItem(item);
                        if (lststock.Count > 0)
                        {
                            splititem.barcodes = lststock[0].newbarcode;
                            splititem.newbarcodelist = string.Join(",", lststock.ConvertAll(e => e.barcode));
                            splititem.newbarcode = lststock.OrderBy(e => e.id).Last().barcode;
                        }
                    }
                }

                SplitbarcodehistoryDataService splitbarcodehistoryDataService = new SplitbarcodehistoryDataService() { Context = this.Context };
                splitbarcodehistoryDataService.Context = this.Context;
                splitbarcodehistoryDataService.Save(obarcodereport);
            }

            if (obarcodereport.Exists(e => e.mode == 5 || (e.mode == 4 && e.assignmarker)))
            {
                StockmarkerDataService stockmarkerDataService = new StockmarkerDataService() { Context = this.Context };
                stockmarkerDataService.Associate(obarcodereport);
            }
            else if (obarcodereport.Exists(e => e.locationid > 0))
            {
                StockmarkerDataService stockmarkerDataService = new StockmarkerDataService() { Context = this.Context };
                List<Stockmarkerdetail> lstmarker = stockmarkerDataService.Search(new StockmarkerCreteria()
                {
                    locationid = obarcodereport[0].locationid,
                    barcode = obarcodereport[0].barcode,
                    mode = 2
                });
                if (lstmarker != null && lstmarker.Count > 0)
                {
                    obarcodereport[0].stockmarkerid = lstmarker[0].markerid;
                    stockmarkerDataService.Associate(obarcodereport);
                }
            }
            return obarcodereport;
        }
        internal List<Stock2BulkSplit.Item> ConvertItem(Stock2BulkSplit obarcodereport)
        {
            if(obarcodereport.items.Exists(x=>x.barcode.Contains("-")))
            {
                throw new Exception("Can't split second time, try single item split - "+String.Join(",", obarcodereport.items.ConvertAll(x=>x.barcode)));
            }
            ItemsDataService itemsDataService = new ItemsDataService() { Context = this.Context };
            ReferencelistDataService referencelistDataService = new ReferencelistDataService() { Context = this.Context };
            LrinvoiceitemsDataService lrinvoiceitemsDataService = new LrinvoiceitemsDataService() { Context = this.Context };

            using (var reader = lrinvoiceitemsDataService.ExecuteReader(string.Format(@"select stock.barcode,stock.id,stock.itemid,invoiceitemid,stock.productid,salablegoods.id salid from stock 
                left outer join lrinvoiceitems on lrinvoiceitems.id = invoiceitemid
                left outer join salablegoods on salablegoods.stockid = stock.id and salablegoods.locationid={1}
                where stock.barcode in ({0})", string.Join(",", obarcodereport.items.ConvertAll(x =>"'"+x.barcode+"'")),obarcodereport.locationid), null))
            {
                while (reader.Read())
                {
                    string barcode = reader["barcode"] == DBNull.Value ? "" : reader["barcode"].ToString();
                    long id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                    long salid = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["salid"]); 
                    long prdid = reader["productid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productid"]);
                    long itmid = reader["itemid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["itemid"]);
                    long invoiceitemid = reader["invoiceitemid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["invoiceitemid"]);
                    obarcodereport.items.FindAll(x => x.barcode == barcode).ForEach(y =>
                    {
                        y.stockid = id;
                        y.productid = prdid;
                        y.invoiceitemid = invoiceitemid;
                        y.itemid = itmid;
                        y.salid = salid;
                    });
                }
            }

            if(obarcodereport.items.Exists(x=>x.stockid == 0))
            {
                var mbarcodes = obarcodereport.items.FindAll(x => x.stockid == 0).ConvertAll(x => x.barcode);
                throw new Exception("Can't find these barcode in the system " + String.Join(",", mbarcodes));
            }

            if(obarcodereport.locationid>0 &&
               obarcodereport.items.Exists(x => x.salid == 0))
            {
                var mbarcodes = obarcodereport.items.FindAll(x => x.salid == 0).ConvertAll(x => x.barcode);
                throw new Exception("Can't find the stock on given location - " + String.Join(",", mbarcodes));
            }

            var allProducts = obarcodereport.items.ConvertAll(x => x.product.Replace("'","")).Distinct();
            var allBrands = obarcodereport.items.ConvertAll(x => x.brand.Replace("'", "")).Distinct();
            var allPattern = obarcodereport.items.ConvertAll(x => x.pattern.Replace("'", "")).Distinct();
            var allColour = obarcodereport.items.ConvertAll(x => x.colour.Replace("'", "")).Distinct();
            var allMaterial = obarcodereport.items.ConvertAll(x => x.material.Replace("'", "")).Distinct();
            var allSleeve = obarcodereport.items.ConvertAll(x => x.sleeve.Replace("'", "")).Distinct();
            var allSize = obarcodereport.items.ConvertAll(x => x.size.Replace("'", "")).Distinct();
            var allStyle = obarcodereport.items.ConvertAll(x => x.style.Replace("'", "")).Distinct();

            ProductsDataService productsDataService = new ProductsDataService() { Context = this.Context };
            BrandDataService brandsDataService = new BrandDataService() { Context = this.Context };
            StockDataService stockDataService = new StockDataService() { Context = this.Context };
            SalablegoodsDataService salablegoodsDataService = new SalablegoodsDataService() { Context = this.Context };
            var products = productsDataService.SearchList(new ProductsCreteria());
            var missingproduts = allProducts.ToList().FindAll(x =>!products.Exists(y => y.name.ToUpper() == x.ToUpper()));
            missingproduts.ForEach(x =>
            {
                if(!string.IsNullOrEmpty(x))
                products.Add(productsDataService.Insert(new Products()
                {
                    isactive = true,
                    name = x.ToUpper()
                }));
            });
            var brands = brandsDataService.Search(new BrandCreteria());
            var missingbrands = allBrands.ToList().FindAll(x => !brands.Exists(y => y.name.ToUpper() == x.ToUpper()));
            missingbrands.ForEach(x =>
            {
                if (!string.IsNullOrEmpty(x))
                    brands.Add(brandsDataService.Insert(new Brand()
                {
                    isactive = true,
                    name = x.ToUpper()
                }));
            });

            var patterns = referencelistDataService.SearchList(new ReferencelistCreteria() { addmissingnames =true, type = "PATTERN", names = allPattern.ToList() });
            var colours = referencelistDataService.SearchList(new ReferencelistCreteria() { addmissingnames = true, type = "COLOUR", names = allColour.ToList() });
            var sizes = referencelistDataService.SearchList(new ReferencelistCreteria() { addmissingnames = true, type = "SIZE", names = allSize.ToList() });
            var materials = referencelistDataService.SearchList(new ReferencelistCreteria() { addmissingnames = true, type = "MATERIAL", names = allMaterial.ToList() });
            var sleeves = referencelistDataService.SearchList(new ReferencelistCreteria() { addmissingnames = true, type = "SLEEVE", names = allSleeve.ToList() });
            var styles = referencelistDataService.SearchList(new ReferencelistCreteria() { addmissingnames = true, type = "STYLE", names = allStyle.ToList() });

            foreach(var bitem in obarcodereport.items)
            {
                if (!string.IsNullOrEmpty(bitem.product) &&
                    products.Exists(x => x.name.ToUpper() == bitem.product.ToUpper()))
                {
                    bitem.productid = products.Find(x => x.name.ToUpper() == bitem.product.ToUpper()).id;
                }
                else if (bitem.productid > 0)
                {
                    bitem.product = products.Find(x => x.id == bitem.productid).name;
                }

                if (!string.IsNullOrEmpty(bitem.brand) &&
                    brands.Exists(x => x.name.ToUpper() == bitem.brand.ToUpper()))
                    bitem.brandid = brands.Find(x => x.name.ToUpper() == bitem.brand.ToUpper()).id;
                
                if (!string.IsNullOrEmpty(bitem.pattern) &&
                    patterns.Exists(x => x.name.ToUpper() == bitem.pattern.ToUpper()))
                    bitem.patternid = patterns.Find(x => x.name.ToUpper() == bitem.pattern.ToUpper()).id;

                if (!string.IsNullOrEmpty(bitem.colour) &&
                    colours.Exists(x => x.name.ToUpper() == bitem.colour.ToUpper()))
                    bitem.colourid = colours.Find(x => x.name.ToUpper() == bitem.colour.ToUpper()).id;

                if (!string.IsNullOrEmpty(bitem.size) &&
                    sizes.Exists(x => x.name.ToUpper() == bitem.size.ToUpper()))
                    bitem.sizeid = sizes.Find(x => x.name.ToUpper() == bitem.size.ToUpper()).id;

                if (!string.IsNullOrEmpty(bitem.material) &&
                    materials.Exists(x => x.name.ToUpper() == bitem.material.ToUpper()))
                    bitem.materialid = materials.Find(x => x.name.ToUpper() == bitem.material.ToUpper()).id;

                if (!string.IsNullOrEmpty(bitem.sleeve) &&
                    sleeves.Exists(x => x.name.ToUpper() == bitem.sleeve.ToUpper()))
                    bitem.sleeveid = sleeves.Find(x => x.name.ToUpper() == bitem.sleeve.ToUpper()).id;

                if (!string.IsNullOrEmpty(bitem.style) &&
                    styles.Exists(x => x.name.ToUpper() == bitem.style.ToUpper()))
                    bitem.styleid = styles.Find(x => x.name.ToUpper() == bitem.style.ToUpper()).id;
            }

            foreach(var itm in obarcodereport.items.GroupBy(x=>new { x.productid,x.brandid,x.patternid,x.colourid,x.sizeid,x.materialid,x.sleeveid,x.styleid }))
            {
                var newitem = new ItemsCreteria()
                {
                    productid = itm.Key.productid,
                    brandid = itm.Key.brandid,
                    patternid = itm.Key.patternid,
                    colourid = itm.Key.colourid,
                    sizeid = itm.Key.sizeid,
                    materialid = itm.Key.materialid,
                    sleeveid = itm.Key.sleeveid,
                    styleid = itm.Key.styleid,
                };
                var avlitems = itemsDataService.Search(newitem);
                if(avlitems.Count == 0)
                {
                    newitem.sellingname = products.Find(x => x.id == itm.Key.productid).name;
                    newitem.productname = newitem.sellingname;
                    newitem.createdby = Context.UserID;
                    newitem.modifiedby = Context.UserID;
                    newitem.createdon = DateTimeNow;
                    newitem.modifiedon = DateTimeNow;
                    newitem.isactive = true;
                    avlitems.Add(itemsDataService.Insert(newitem));
                }
                itm.ForEach(y => y.itemid = avlitems[0].id);
            }

            foreach(var bgrp in obarcodereport.items.GroupBy(x=>x.barcode))
            {
                var newitems = bgrp.ToList();
                int id = 1;
                newitems.ForEach(x => x.nbarcode = x.barcode + "-" + (id++).ToString("F0"));
                while (newitems.Count>0)
                {
                    string barcodes = string.Join(",", newitems.ConvertAll(x => "'" + x.nbarcode + "'"));
                    newitems.ForEach(x => x.nstockid = 0);
                    using (var reader = ExecuteReader("Select id,barcode from stock where barcode in (" + barcodes + ")", null))
                    {
                        while (reader.Read())
                        {
                            long newstockid = reader.GetInt64(0);
                            string barcode = reader.GetString(1);
                            newitems.Find(x => x.nbarcode == barcode).nstockid = newstockid;
                        }
                    }
                    if (newitems.Exists(x => x.nstockid > 0))
                    {
                        newitems.RemoveAll(x => x.nstockid == 0);
                        newitems.ForEach(x => x.nbarcode = x.barcode + "-" + (id++).ToString("F0"));
                    }
                    else
                    {
                        newitems.Clear();
                    }
                }
                newitems = bgrp.ToList();
                var stock = stockDataService.Search(0, bgrp.Key);
                List<Salablegoods> salablegood = obarcodereport.locationid>0 ? salablegoodsDataService.Search(new SalablegoodsCreteria() { id = bgrp.First().salid }):null;
                stock.transfered -= newitems.Sum(x => x.qty);
                stock.qty -= newitems.Sum(x => x.qty);
                if (stock.transfered < 0) stock.transfered = 0;
                if (stock.qty < 0) stock.qty = 0;
                long sourcestockid = stock.id;

                using (var conn = CreateConnection())
                {
                    conn.Open();
                    var trans = conn.BeginTransaction();
                    var param = new SortedList<string, object>();
                    param.Add("id", stock.id);
                    param.Add("transfered", stock.transfered);
                    param.Add("qty", stock.qty);
                    stockDataService.ExecuteQuery("update stock set transfered=@transfered,qty=@qty where id=@id", param, conn, trans);
                    if (obarcodereport.locationid > 0)
                    {
                        salablegood[0].iqty -= newitems.Sum(x => x.qty);
                        if (salablegood[0].iqty < 0)
                        {
                            salablegood[0].jqty += Math.Abs(salablegood[0].iqty);
                            salablegood[0].iqty = 0;
                        }
                        param = new SortedList<string, object>();
                        param.Add("id", salablegood[0].id);
                        param.Add("iqty", salablegood[0].iqty);
                        param.Add("jqty", salablegood[0].jqty);
                        stockDataService.ExecuteQuery("update Salablegoods set iqty=@iqty,jqty=@jqty where id=@id", param, conn, trans);
                    }

                    List<string> barcodes = new List<string>();
                    foreach (var stk in newitems)
                    {
                        stock.id = 0;
                        stock.barcode = stk.nbarcode;
                        stock.productid = stk.productid;
                        stock.itemid = stk.itemid;
                        stock.createdby = Context.UserID;
                        stock.modifiedby = Context.UserID;
                        stock.createdon = DateTimeNow;
                        stock.modifiedon = DateTimeNow;
                        stock.version = 1;
                        stock.returnqty = 0;
                        //stock.buyingqty = stk.qty;
                        stock.qty = stk.qty;
                        stock.transfered = obarcodereport.locationid > 0 ? stk.qty:0;
                        stockDataService.IsMigration = true;
                        stockDataService.Insert(trans, conn, stock);
                        if (obarcodereport.locationid>0)
                        {
                            var salgood = salablegood[0];
                            salgood.id = 0;
                            salgood.barcode = stk.nbarcode;
                            salgood.itemid = stk.itemid;
                            salgood.stockid = stock.id;
                            salgood.createdby = Context.UserID;
                            salgood.modifiedby = Context.UserID;
                            salgood.createdon = DateTimeNow;
                            salgood.modifiedon = DateTimeNow;
                            salgood.version = 1;
                            salgood.iqty = stk.qty;
                            salgood.oqty = 0;
                            salgood.tiqty = 0;
                            salgood.toqty = 0;
                            salgood.dqty = 0;
                            salgood.aqty = 0;
                            salgood.jqty = 0;
                            salablegoodsDataService.isMigration = true;
                            salablegoodsDataService.Insert(trans, conn, salgood);
                        }

                        //stk.barcode = stock.barcode;
                        stk.stockid = stock.id;
                        barcodes.Add(stock.barcode);
                    }
                    if (stock.invoiceitemid > 0)
                    {
                        var invoiceitem = lrinvoiceitemsDataService.Search(new LrinvoiceitemsCreteria() { id = stock.invoiceitemid });
                        if (invoiceitem[0].barcodeList == null)
                            invoiceitem[0].barcodeList = new List<string>();
                        invoiceitem[0].barcodeList.AddRange(barcodes);
                        invoiceitem[0].barcodeList = invoiceitem[0].barcodeList;
                        lrinvoiceitemsDataService.UpdateBarcode(trans, conn, invoiceitem[0]);
                    }
                    SplitbarcodehistoryDataService splitbarcodehistoryDataService = new SplitbarcodehistoryDataService() { Context = this.Context };
                    splitbarcodehistoryDataService.Context = this.Context;

                    var sequence = SequenceManager.Instance(Context.OrganizationID).Get("splitbarcode.code", 0, 0);
                    var code = sequence.GetNextNumber(Context, conn, trans);
                    var splitBarcode = new Splitbarcodehistory()
                    {
                        barcode = bgrp.Key,
                        code = code,
                        barcodelist = barcodes,
                        companyid = obarcodereport.companyid,
                        createdby = Context.UserID,
                        createdon = DateTimeNow,
                        isactive = true,
                        locationid = obarcodereport.locationid,
                        modifiedby = Context.UserID,
                        modifiedon = DateTimeNow,
                        stockid = sourcestockid,
                        splitqty = newitems.Sum(x => x.qty),
                        noofnewbarcode = Convert.ToInt32(newitems.Sum(x => x.qty)),
                        version = 1
                    };
                    splitbarcodehistoryDataService.Insert(trans, conn, splitBarcode);
                    trans.Commit();
                    conn.Close();
                }
            }
            return obarcodereport.items;
        }
        internal List<Stock> ConvertItem(LRInvoiceItemBitChange item)
        {
            NpgsqlTransaction transaction = null;
            List<Stock> updatableStock = new List<Stock>();
            string newbarcode = "";
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    List<LrinvoiceItemDetail> existing = new List<LrinvoiceItemDetail>();

                    if (item.Core.id > 0)
                    {
                        existing = Search(new LrinvoiceitemsCreteria()
                        {
                            id = item.Core.id
                        });
                    }
                    else
                    {
                        existing.Add(new LrinvoiceItemDetail() { productid = item.Core.productid, barcodes = "null" });
                    }
                    ProductsDataService productdataservice = new ProductsDataService() { Context = this.Context };
                    var products = productdataservice.Search(new ProductsCreteria()
                    {
                        id = (item.items != null && item.items[0].productid != existing[0].productid) ? item.items[0].productid : existing[0].productid
                    });

                    if (string.IsNullOrEmpty(existing[0].barcodes) || existing[0].barcodes == "null")
                        existing[0].barcodes = "['" + item.barcode + "']";


                    var barcodeConfig = InfyPOS.Processors.BarcodeManager.Instance(Context.OrganizationID).Get(products[0].barcodemode, item.companyid);
                    List<string> jsonbarcode = Newtonsoft.Json.JsonConvert.DeserializeObject<List<string>>(existing[0].barcodes);
                    if(jsonbarcode == null || jsonbarcode.Count == 0)
                    {
                        jsonbarcode = new List<string>();
                        jsonbarcode.Add(item.barcode);
                    }
                    if (jsonbarcode.Count >= 1)
                    {
                        //item.items.ForEach(e => e.barcode = jsonbarcode[0]);

                        List<string> jsonbarcodenew = new List<string>();
                        jsonbarcodenew.AddRange(jsonbarcode);
                        LrinvoiceItemDetail currentItem = Newtonsoft.Json.JsonConvert.DeserializeObject<LrinvoiceItemDetail>(Newtonsoft.Json.JsonConvert.SerializeObject(existing[0]));
                        StockDataService stockdataservice = new StockDataService() { Context = this.Context };
                        stockdataservice.Context = this.Context;
                        SalablegoodsDataService salablegoodsdataservice = new SalablegoodsDataService() { Context = this.Context };
                        salablegoodsdataservice.Context = this.Context;
                        if (string.IsNullOrEmpty(item.barcode)) item.barcode = jsonbarcode[0];

                        var existingsalable = salablegoodsdataservice.Search(new SalablegoodsCreteria()
                        {
                            barcode = item.barcode
                        });

                        var exstingstock = stockdataservice.Search(new StockCreteria()
                        {
                            barcode = item.barcode
                        });

                        var adjustSalableQty = false;
                        var transferqty = existingsalable.Sum(e => e.oqty + e.dqty);
                        var qty = exstingstock[0].qty - item.items.Sum(e => e.convertableqty != 0 ? e.convertableqty : e.sellableqty);
                        if (item.locationid == 0)
                        {
                            var availWhstock = exstingstock.Sum(e => e.qty - (e.damagedqty + e.transfered + e.journalqty));
                            var convertqty = item.items.Sum(e => e.convertableqty != 0 ? e.convertableqty : e.sellableqty);

                            if (availWhstock >= convertqty)
                            {
                                transferqty = exstingstock.Sum(e => e.transfered);
                            }
                            else
                            {
                                foreach (var salablegoods in existingsalable)
                                {
                                    salablegoods.iqty = salablegoods.oqty + salablegoods.dqty + salablegoods.jqty;
                                    salablegoodsdataservice.UpdateInwardQty(transaction, conn, salablegoods);
                                }
                            }
                        }
                        else
                        {
                            transferqty = exstingstock[0].transfered - item.items.Sum(e => e.convertableqty != 0 ? e.convertableqty : e.sellableqty);
                            qty = exstingstock[0].qty - item.items.Sum(e => e.convertableqty != 0 ? e.convertableqty : e.sellableqty);
                            existingsalable = salablegoodsdataservice.Search(new SalablegoodsCreteria()
                            {
                                barcode = item.barcode,
                                locationid = item.locationid
                            });
                            var availRetailstock = existingsalable.Sum(e => e.iqty - (e.dqty + e.jqty + e.oqty));
                            var convertqty = item.items.Sum(e => e.convertableqty != 0 ? e.convertableqty : e.sellableqty);
                            if (availRetailstock < convertqty)
                                throw new Exception("Stock not available to convert");
                            adjustSalableQty = true;
                            existingsalable[0].iqty = existingsalable[0].iqty - convertqty;
                            if (existingsalable[0].pqty > 0 && existingsalable[0].pqty > convertqty)
                                existingsalable[0].pqty = existingsalable[0].pqty - convertqty;
                            salablegoodsdataservice.UpdateInwardQty(transaction, conn, existingsalable[0]);
                        }

                        List<InfyPOS.Processors.BarcodeManager.NewBarcodeID> listOfBarcodes;
                        if (Services.ConfigManager.Instance(Context.OrganizationID).Config.system_locationbasedbarcodes && item.locationid > 0)
                        {
                            listOfBarcodes = stockdataservice.CreateBarcode(barcodeConfig.Type,
                                item.items.Count, item.companyid, conn, transaction, item.locationid,
                                new MissingBarcode()
                                {
                                    barcode = item.items[0].barcode,
                                    productid = item.items[0].productid,
                                    sellingprice = item.items[0].sellingprice
                                },
                                null);
                        }
                        else
                            listOfBarcodes = stockdataservice.CreateBarcode(barcodeConfig.Type, item.items.Count, item.companyid, conn, transaction);

                        var currentStockItem = Newtonsoft.Json.JsonConvert.DeserializeObject<Stock>(Newtonsoft.Json.JsonConvert.SerializeObject(exstingstock[0]));
                        currentStockItem.qty = qty;
                        // Commented Not to change the source barcode as bit item even if new split barcode is bit.
                        //currentStockItem.isbit = item.IsBit;  
                        currentStockItem.transfered = transferqty;
                        currentStockItem.modifiedon = DateTimeNow;

                        updatableStock.Add(currentStockItem);
                        for (int i = 0; i < item.items.Count; i++)
                        {
                            currentStockItem = Newtonsoft.Json.JsonConvert.DeserializeObject<Stock>(Newtonsoft.Json.JsonConvert.SerializeObject(exstingstock[0]));
                            currentStockItem.price = item.items[i].sellingprice;
                            currentStockItem.discount = item.items[i].salediscount;
                            currentStockItem.qty = item.items[i].sellableqty;
                            currentStockItem.designid = string.IsNullOrEmpty(item.items[i].designid) ? currentStockItem.designid : item.items[i].designid;
                            currentStockItem.isbit = item.IsBit;

                            //if (currentStockItem.isbit && currentStockItem.displaymrp > 0 && item.items[0].convertableqty == 0)
                            //    currentStockItem.displaymrp = currentStockItem.displaymrp * currentStockItem.qty;

                            currentStockItem.batch = item.items[i].Details != null ? item.items[i].Details.batch : "";
                            currentStockItem.serialno = item.items[i].Details != null ? item.items[i].Details.serialno : "";
                            currentStockItem.expiry = item.items[i].Details != null ? item.items[i].Details.expiry : DateTime.MinValue;

                            if (item.items[i].convertableqty != 0)
                            {
                                currentStockItem.buyingprice = item.items[i].convertableqty / item.items[i].sellableqty * currentStockItem.buyingprice;
                                currentStockItem.ibuyingprice = item.items[i].convertableqty / item.items[i].sellableqty * currentStockItem.ibuyingprice;
                                currentStockItem.isbit = false;
                                currentStockItem.sizeinmeter = item.items[i].sizeinmeter;
                            }

                            currentStockItem.id = 0;
                            currentStockItem.transfered = 0;
                            currentStockItem.damagedqty = 0;
                            currentStockItem.journalqty = 0;
                            currentStockItem.returnqty = 0;
                            if (item.Core.productid != item.items[i].productid)
                            {
                                var newproducts = productdataservice.Search(new ProductsCreteria()
                                {
                                    id = item.items[i].productid
                                });
                                currentStockItem.hsncode = newproducts != null && newproducts.Count > 0 ? newproducts[0].hsncode : currentStockItem.hsncode;
                                currentStockItem.productid = item.items[i].productid;
                                currentStockItem.printingname = item.items[i].productname;
                                ItemsDataService itemsDataService = new ItemsDataService() { Context = this.Context };
                                var items = itemsDataService.Search(new ItemsCreteria()
                                {
                                    productid = item.items[i].productid,
                                    colourid = item.items[i].Details.colourid,
                                    sizeid = item.items[i].Details.sizeid,
                                    styleid = item.items[i].Details.styleid,
                                    patternid = item.items[i].Details.patternid,
                                    materialid = item.items[i].Details.materialid,
                                    brandid = item.items[i].Details.brandid,
                                    typeid = item.items[i].Details.typeid,
                                    agmtcode = item.items[0].itbuscode,
                                    productmap = true
                                });
                                if (items.Count == 1)
                                {
                                    item.items[i].Details.id = items[0].id;
                                    currentStockItem.itemid = items[0].id;
                                }
                                else
                                {
                                    item.items[i].Details.productid = item.items[i].productid;
                                    itemsDataService.Insert(item.items[i].Details);
                                    currentStockItem.itemid = item.items[i].Details.id;
                                    item.items[i].Details.id = item.items[i].Details.id;
                                }
                            }
                            currentStockItem.barcode = listOfBarcodes[i].code;
                            currentStockItem.barcodeid = listOfBarcodes[i].id;
                            UpdateEPrice(currentStockItem);
                            updatableStock.Add(currentStockItem);
                        }
                        foreach (var stockitem in updatableStock)
                        {
                            if (stockitem.id > 0)
                            {
                                stockdataservice.Update(transaction, conn, stockitem);
                            }
                            else
                            {
                                stockdataservice.Insert(transaction, conn, stockitem);
                                if (adjustSalableQty)
                                {
                                    var currentRetailItem = Newtonsoft.Json.JsonConvert.DeserializeObject<Salablegoods>(Newtonsoft.Json.JsonConvert.SerializeObject(existingsalable[0]));
                                    currentRetailItem.id = 0;
                                    currentRetailItem.iqty = stockitem.qty;
                                    currentRetailItem.oqty = 0;
                                    currentRetailItem.dqty = 0;
                                    currentRetailItem.jqty = 0;
                                    currentRetailItem.tiqty = 0;
                                    currentRetailItem.toqty = 0;
                                    if (currentRetailItem.pqty > 0)
                                        currentRetailItem.pqty = stockitem.qty;
                                    currentRetailItem.price = stockitem.price;
                                    currentRetailItem.discount = stockitem.discount;
                                    currentRetailItem.stockid = stockitem.id;
                                    currentRetailItem.barcode = stockitem.barcode;
                                    currentRetailItem.itemid = stockitem.itemid;
                                    currentRetailItem.costprice = stockitem.buyingprice;
                                    currentRetailItem.icostprice = stockitem.ibuyingprice;
                                    salablegoodsdataservice.Insert(transaction, conn, currentRetailItem);
                                }
                            }
                            if (stockitem.sizeinmeter != 0)
                            {
                                var param = new SortedList<string, object>();
                                param.Add("id", stockitem.id);
                                param.Add("sizeinmeter", stockitem.sizeinmeter);
                                stockdataservice.ExecuteQuery("update stock set sizeinmeter=@sizeinmeter where id=@id", param, conn, transaction);
                            }
                        }
                        //jsonbarcodenew.AddRange(listOfBarcodes);
                        currentItem.barcodes = barcodeConfig.CompresCode(listOfBarcodes, jsonbarcodenew);
                        currentItem.modifiedon = DateTimeNow;
                        //Update(transaction, conn, currentItem);
                        UpdateBarcode(transaction, conn, currentItem);
                        newbarcode = currentItem.barcodes;
                    }
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, item);
                return null;
            }
            if (updatableStock.Count > 0)
                updatableStock[0].newbarcode = newbarcode;
            return updatableStock;
        }

        private void UpdateEPrice(Stock currentStockItem)
        {
            ProductsDataService productdataservice = new ProductsDataService() { Context = this.Context };
            TaxDataService taxdata = new TaxDataService() { Context = this.Context };
            decimal taxpercentage = 0;
            decimal salestaxpercentage = 0;
            var products = productdataservice.Search(new ProductsCreteria()
            {
                id = currentStockItem.productid
            });

            var taxlist = taxdata.Search(new TaxCreteria() { id = products[0].salestaxid });
            if (taxlist != null && taxlist.Count > 0)
            {
                if (taxlist[0].type == 1)
                {
                    taxpercentage = taxlist[0].taxpercentage;
                    salestaxpercentage = taxpercentage;
                }
                else if (taxlist[0].type == 2 && taxlist[0].taxsplit != null && taxlist[0].taxsplit.items.Count > 1)
                {
                    if (currentStockItem.price - currentStockItem.discount > taxlist[0].taxsplit.items[0].fromamount
                        && currentStockItem.price - currentStockItem.discount <= taxlist[0].taxsplit.items[0].toamount)
                    {
                        taxpercentage = (taxlist[0].taxsplit.items[0].cgst * 2);
                    }
                    else
                    {
                        taxpercentage = (taxlist[0].taxsplit.items[1].cgst * 2);
                    }

                    if (currentStockItem.price - currentStockItem.discount > taxlist[0].taxsplit.items[0].fromamount
                        && currentStockItem.price - currentStockItem.discount <= taxlist[0].taxsplit.items[0].toamount)
                    {
                        salestaxpercentage = taxpercentage;
                    }
                    else
                    {
                        salestaxpercentage = taxpercentage;
                    }
                }
            }
            if (salestaxpercentage > 0)
                currentStockItem.eprice = (currentStockItem.price - currentStockItem.discount) / (salestaxpercentage + 100) * 100;
            else
                currentStockItem.eprice = (currentStockItem.price - currentStockItem.discount);
        }

        internal List<LrinvoiceItemDetail> SaveMultiple(LrinvoiceItems requestitem)
        {
            List<LrinvoiceItemDetail> lst = new List<LrinvoiceItemDetail>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                QueryBuilder qb = new QueryBuilder();
                qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                qb.AddParameter("isactive", true, "=");
                qb.AddInParamer("id", string.Join<long>(",", requestitem.itemids));
                command = qb.GetCommand(conn);

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        LrinvoiceItemDetail olrinvoiceitems = new LrinvoiceItemDetail();
                        olrinvoiceitems.productid = reader["productid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productid"].ToString());
                        olrinvoiceitems.buyingqty = reader["buyingqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["buyingqty"].ToString());
                        olrinvoiceitems.freeqty = reader["freeqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["freeqty"].ToString());
                        olrinvoiceitems.buyingprice = reader["buyingprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["buyingprice"].ToString());
                        olrinvoiceitems.mrp = reader["mrp"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["mrp"].ToString());
                        olrinvoiceitems.rowexpense = reader["rowexpense"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["rowexpense"].ToString());
                        olrinvoiceitems.rowexpensevalue = reader["rowexpensevalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["rowexpensevalue"].ToString());
                        olrinvoiceitems.purchasediscount = reader["purchasediscount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["purchasediscount"].ToString());
                        olrinvoiceitems.purchasetotaldiscount = reader["purchasetotaldiscount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["purchasetotaldiscount"].ToString());
                        olrinvoiceitems.salediscountpercentage = reader["salediscountpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["salediscountpercentage"].ToString());
                        olrinvoiceitems.marginpercentage = reader["marginpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["marginpercentage"].ToString());
                        olrinvoiceitems.purchasediscountpercentage = reader["purchasediscountpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["purchasediscountpercentage"].ToString());
                        olrinvoiceitems.salediscount = reader["salediscount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["salediscount"].ToString());
                        olrinvoiceitems.salestax = reader["salestax"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["salestax"].ToString());
                        olrinvoiceitems.purchasetaxpercentage = reader["purchasetaxpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["purchasetaxpercentage"].ToString());
                        olrinvoiceitems.salestaxpercentage = reader["salestaxpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["salestaxpercentage"].ToString());


                        olrinvoiceitems.sellingprice = reader["sellingprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["sellingprice"].ToString());
                        olrinvoiceitems.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        olrinvoiceitems.sno = reader["sno"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["sno"]);
                        olrinvoiceitems.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        olrinvoiceitems.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"].ToString());
                        olrinvoiceitems.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        olrinvoiceitems.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        olrinvoiceitems.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        olrinvoiceitems.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        olrinvoiceitems.itemid = reader["itemid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["itemid"].ToString());
                        olrinvoiceitems.damagedqty = reader["damagedqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["damagedqty"].ToString());
                        olrinvoiceitems.sellableqty = reader["sellableqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["sellableqty"].ToString());
                        olrinvoiceitems.basebuyingprice = reader["basebuyingprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["basebuyingprice"].ToString());
                        olrinvoiceitems.lrinvoiceid = reader["lrinvoiceid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["lrinvoiceid"].ToString());
                        olrinvoiceitems.stocklocationid = reader["stocklocationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["stocklocationid"].ToString());
                        olrinvoiceitems.uan = reader["uan"] == DBNull.Value ? "" : Convert.ToString(reader["uan"]);
                        olrinvoiceitems.purchasetax = reader["purchasetax"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["purchasetax"].ToString());
                        olrinvoiceitems.taxid = reader["taxid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["taxid"].ToString());
                        olrinvoiceitems.ptaxid = reader["ptaxid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["ptaxid"].ToString());
                        olrinvoiceitems.barcodes = reader["barcodes"] == DBNull.Value ? "null" : reader["barcodes"].ToString();
                        olrinvoiceitems.designid = reader["designid"] == DBNull.Value ? "" : reader["designid"].ToString();
                        olrinvoiceitems.hsncode = reader["hsncode"] == DBNull.Value ? "" : reader["hsncode"].ToString();
                        olrinvoiceitems.imgid = reader["imgid"] == DBNull.Value ? "" : reader["imgid"].ToString();
                        olrinvoiceitems.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"].ToString());
                        olrinvoiceitems.itemdetail = reader["itemdetail"] == DBNull.Value ? "null" : reader["itemdetail"].ToString();
                        olrinvoiceitems.POItemInfo = reader["poinfo"] == DBNull.Value ? "null" : reader["poinfo"].ToString();
                        olrinvoiceitems.PriceItemInfo = reader["priceinfo"] == DBNull.Value ? "null" : reader["priceinfo"].ToString();
                        olrinvoiceitems.amount = reader["amount"] == DBNull.Value ? ((olrinvoiceitems.buyingprice - olrinvoiceitems.purchasediscount) * olrinvoiceitems.buyingqty) : Convert.ToDecimal(reader["amount"].ToString());
                        lst.Add(olrinvoiceitems);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, requestitem);
            }

            if (lst.Count > 0)
            {
                conn = CreateConnection();
                conn.Open();
                var transaction = conn.BeginTransaction();
                try
                {
                    ItemsDataService itemservice = new ItemsDataService() { Context = this.Context };
                    itemservice.Context = this.Context;
                    foreach (var item in lst)
                    {
                        if (requestitem.Details.brandid > 0)
                        {
                            item.Details.brandid = requestitem.Details.brandid;
                            item.Details.brandname = requestitem.Details.brandname;
                        }

                        if (requestitem.Details.colourid > 0)
                        {
                            item.Details.colourid = requestitem.Details.colourid;
                            item.Details.colourname = requestitem.Details.colourname;
                        }

                        if (requestitem.Details.materialid > 0)
                        {
                            item.Details.materialid = requestitem.Details.materialid;
                            item.Details.materialname = requestitem.Details.materialname;
                        }
                        if (requestitem.Details.patternid > 0)
                        {
                            item.Details.patternid = requestitem.Details.patternid;
                            item.Details.patternname = requestitem.Details.patternname;
                        }

                        if (requestitem.Details.productid > 0)
                        {
                            item.productid = requestitem.Details.productid;
                            item.Details.productid = requestitem.Details.productid;
                            item.Details.productname = requestitem.Details.productname;
                            item.Details.sellingname = requestitem.Details.sellingname;
                        }

                        if (requestitem.Details.styleid > 0)
                        {
                            item.Details.styleid = requestitem.Details.styleid;
                            item.Details.stylename = requestitem.Details.stylename;
                        }

                        if (requestitem.Details.sizeid > 0)
                        {
                            item.Details.sizeid = requestitem.Details.sizeid;
                            item.Details.sizename = requestitem.Details.sizename;
                        }

                        if (requestitem.Details.typeid > 0)
                        {
                            item.Details.typeid = requestitem.Details.typeid;
                            item.Details.typename = requestitem.Details.typename;
                        }

                        if (requestitem.Details.fitid > 0)
                        {
                            item.Details.fitid = requestitem.Details.fitid;
                            item.Details.fitname = requestitem.Details.fitname;
                        }

                        if (requestitem.Details.sleeveid > 0)
                        {
                            item.Details.sleeveid = requestitem.Details.sleeveid;
                            item.Details.sleevename = requestitem.Details.sleevename;
                        }

                        var iteminfo = itemservice.Search(new ItemsCreteria()
                        {
                            brandid = item.Details.brandid,
                            colourid = item.Details.colourid,
                            materialid = item.Details.materialid,
                            patternid = item.Details.patternid,
                            typeid = item.Details.typeid,
                            sizeid = item.Details.sizeid,
                            styleid = item.Details.styleid,
                            fitid = item.Details.fitid,
                            sleeveid = item.Details.sleeveid,
                            productid = item.productid,
                            agmtcode = item.itbuscode,
                            productmap = true
                        });
                        if (iteminfo.Count == 1)
                        {
                            item.itemid = iteminfo[0].id;
                        }
                        else
                        {
                            var newitem = item.Details;
                            newitem.isactive = true;
                            newitem.salemode = 1;
                            newitem.version = 1;
                            newitem = itemservice.Insert(newitem);
                            item.itemid = newitem.id;
                        }

                        if (!string.IsNullOrEmpty(requestitem.designid))
                            item.designid = requestitem.designid;
                        Update(transaction, conn, item);
                    }
                    UpdateItemID(lst, conn, transaction);
                    transaction.Commit();
                    conn.Close();
                }
                catch (Exception exp)
                {
                    transaction.Rollback();
                    if (conn.State != System.Data.ConnectionState.Closed)
                        conn.Close();
                    WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, requestitem);
                }
            }
            return lst;
        }

        private void UpdateItemID(List<LrinvoiceItemDetail> lst, Npgsql.NpgsqlConnection conn, NpgsqlTransaction transaction)
        {
            foreach (var item in lst)
            {
                NpgsqlCommand command = GetCommand(SQL_UpdateItemID, conn, transaction);
                command.Parameters.Add("itemid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.itemid;
                command.Parameters.Add("invoiceitemid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.id;
                command.ExecuteNonQuery();
            }
        }

        const string SQL_UpdateItemID = @"Update lrinvoiceitems set itemid=@itemid where id=@invoiceitemid;
                                          Update public.stock SET itemid=@itemid where invoiceitemid=@invoiceitemid";

        const string SQL_UpdateStockPrice = @"Update public.stock SET price=@price,displaymrp=@displaymrp,dealerprice=@dealerprice,productid=@productid,printingname=@printingname,discount=@discount,itemid=@itemid,modifiedon=@modifiedon,modifiedby=@modifiedby,eprice=@eprice where invoiceitemid=@invoiceitemid";

        const string SQL_Search = @"select productid, pricingid, buyingqty,freeqty, buyingprice, mrp,rowexpense,rowexpensevalue,purchasetotaldiscount,amount, sellingprice, id, isactive, createdby, modifiedby, version, createdon, modifiedon,basebuyingprice, 
        itemid, damagedqty, sellableqty, lrinvoiceid,stocklocationid,uan, purchasediscountpercentage,taxid,purchasetax,barcodes,designid,itemdetail,sno,purchasediscount,hsncode,imgid,salediscount,salediscountpercentage,wsrate,dealerprice,
        marginpercentage,salestaxpercentage,purchasetaxpercentage,salestax,ptaxid,poinfo,priceinfo,jobworkgrnitemid,buyingitemid,weight,workingcharge,wastage,weightrate,isseconds,sourcesupplierid,itbuscode,cesstaxid,cesstax,cesspercentage from lrinvoiceitems where id=@id";

        const string SQL_SearchAll = @"select lrinvoiceitems.productid, pricingid, lrinvoiceitems.buyingqty,lrinvoiceitems.freeqty, buyingprice, lrinvoiceitems.mrp,rowexpense,rowexpensevalue,purchasetotaldiscount,amount, sellingprice,
        lrinvoiceitems.id, lrinvoiceitems.isactive,lrinvoiceitems.createdby, lrinvoiceitems.modifiedby, lrinvoiceitems.version,
        lrinvoiceitems.createdon, lrinvoiceitems.modifiedon,basebuyingprice,wsrate,lrinvoiceitems.dealerprice,lrinvoiceitems.sourcesupplierid,itbuscode,
        itemid, damagedqty, sellableqty, lrinvoiceid,stocklocationid, uan, purchasediscountpercentage,taxid,purchasetax,barcodes,designid,
        itemdetail,returnqty,sno,purchasediscount,lrinvoiceitems.hsncode,imgid,salediscount,salediscountpercentage,(lrinvoiceitems.sellingprice-lrinvoiceitems.salediscount) finalprice,
        marginpercentage,salestaxpercentage,purchasetaxpercentage,salestax,ptaxid,poinfo,actualqty,priceinfo,actualprice,
		(itemdetail->>'sizeid')::bigint sizeid,weight,workingcharge,wastage,lrinvoiceitems.isseconds,weightrate,lrinvoiceitems.cesstaxid,cesstax,cesspercentage,
        sizelist.name as sizename,colourlist.name as colourname, typelist.name as typename, stylelist.name stylename, 
        materiallist.name as materialname,brand.name brandname, patternlist.name patternname, sleevelist.name sleevename, fitlist.name fitname, products.sellingmode,createdemployee.name createduser, 
        modifiedemployee.name modifieduser,items.sizeid isizeid,items.colourid,items.patternid,items.typeid,items.materialid,items.styleid,items.brandid,items.sleeveid,items.fitid
        {0}
		from lrinvoiceitems
        left outer join items on items.id = lrinvoiceitems.itemid
        left outer join referencelist sizelist on sizelist.id = items.sizeid
        left outer join referencelist colourlist on colourlist.id = items.colourid 
        left outer join referencelist patternlist on patternlist.id = items.patternid
        left outer join referencelist typelist on typelist.id = items.typeid 
        left outer join referencelist materiallist on materiallist.id = items.materialid
        left outer join referencelist stylelist on stylelist.id = items.styleid 
        left outer join referencelist sleevelist on sleevelist.id = items.sleeveid
        left outer join referencelist fitlist on fitlist.id = items.fitid
        left outer join brand on brand.id = items.brandid
        left outer join products on products.id = lrinvoiceitems.productid
        left outer join users createduser on createduser.id = lrinvoiceitems.createdby
        left outer join employee createdemployee on createdemployee.id = createduser.employeeid
        left outer join users modifieduser on modifieduser.id = lrinvoiceitems.modifiedby
        left outer join employee modifiedemployee on modifiedemployee.id = modifieduser.employeeid
        where lrinvoiceid=@lrinvoiceid and lrinvoiceitems.isactive=true";

        const string SQL_Insert = @"INSERT INTO public.lrinvoiceitems(productid, pricingid, buyingqty,freeqty, buyingprice, mrp,rowexpense,rowexpensevalue,purchasetotaldiscount,amount, sellingprice, isactive,isseconds, createdby, modifiedby, version, createdon, modifiedon, itemid, damagedqty,
        sellableqty, lrinvoiceid,stocklocationid, uan, purchasediscountpercentage,taxid,purchasetax,designid,itemdetail,returnqty,sno,purchasediscount,hsncode,imgid,basebuyingprice,salediscount,
        salediscountpercentage,marginpercentage,salestaxpercentage,purchasetaxpercentage,salestax,ptaxid,poinfo,priceinfo,sourceid,wsrate,dealerprice,weight,workingcharge,wastage,weightrate,sourcesupplierid,itbuscode,pentrydate,
        cesstaxid,cesstax,cesspercentage {0}) 
        VALUES(@productid, @pricingid, @buyingqty,@freeqty, @buyingprice, @mrp,@rowexpense, @rowexpensevalue, @purchasetotaldiscount, @amount, @sellingprice, @isactive,@isseconds, @createdby, @modifiedby, 
        @version, @createdon, @modifiedon, @itemid, @damagedqty, @sellableqty, @lrinvoiceid, @stocklocationid, @uan, @purchasediscountpercentage,@taxid,@purchasetax,@designid,@itemdetail,@returnqty,
        @sno,@purchasediscount,@hsncode,@imgid,@basebuyingprice,@salediscount,@salediscountpercentage,@marginpercentage,@salestaxpercentage,@purchasetaxpercentage,@salestax,@ptaxid,@poinfo,@priceinfo,@sourceid,@wsrate,@dealerprice,
        @weight,@workingcharge,@wastage,@weightrate,@sourcesupplierid,@itbuscode,@pentrydate,
        @cesstaxid,@cesstax,@cesspercentage {1}) RETURNING id,barcodes";

        const string SQL_Insert_Migration = @"INSERT INTO public.lrinvoiceitems(productid, pricingid, buyingqty,freeqty, buyingprice, mrp,rowexpense,rowexpensevalue,purchasetotaldiscount,amount, sellingprice, isactive, createdby, modifiedby, version, createdon, modifiedon, itemid, damagedqty,
        sellableqty, lrinvoiceid,stocklocationid,uan, purchasediscountpercentage,taxid,purchasetax,designid,itemdetail,returnqty,sno,purchasediscount,hsncode,imgid,basebuyingprice,salediscount,
        salediscountpercentage,marginpercentage,salestaxpercentage,purchasetaxpercentage,salestax,ptaxid,poinfo,priceinfo,sourceid,wsrate,dealerprice,barcodes,weight,workingcharge,wastage,weightrate,sourcesupplierid,itbuscode,cesstaxid,cesstax,cesspercentage) 
        VALUES(@productid, @pricingid, @buyingqty,@freeqty, @buyingprice, @mrp,@rowexpense, @rowexpensevalue, @purchasetotaldiscount, @amount, @sellingprice, @isactive, @createdby, @modifiedby, 
        @version, @createdon, @modifiedon, @itemid, @damagedqty, @sellableqty, @lrinvoiceid, @stocklocationid,@uan, @purchasediscountpercentage,@taxid,@purchasetax,@designid,@itemdetail,@returnqty,
        @sno,@purchasediscount,@hsncode,@imgid,@basebuyingprice,@salediscount,@salediscountpercentage,@marginpercentage,@salestaxpercentage,@purchasetaxpercentage,@salestax,@ptaxid,@poinfo,@priceinfo,@sourceid,@wsrate,@dealerprice,@barcodes,@weight,
        @workingcharge,@wastage,@weightrate,@sourcesupplierid,@itbuscode,@cesstaxid,@cesstax,@cesspercentage) RETURNING id,barcodes";


        const string SQL_Update = @"UPDATE public.lrinvoiceitems SET productid=@productid, pricingid=@pricingid, buyingqty=@buyingqty, buyingprice=@buyingprice,basebuyingprice=@basebuyingprice, mrp=@mrp, rowexpense=@rowexpense, 
        rowexpensevalue=@rowexpensevalue, purchasetotaldiscount=@purchasetotaldiscount, amount=@amount, uan=@uan,freeqty=@freeqty,
        sellingprice=@sellingprice, modifiedby=@modifiedby, version=@version+1, modifiedon=@modifiedon, itemid=@itemid, damagedqty=@damagedqty, sellableqty=@sellableqty, lrinvoiceid=@lrinvoiceid, stocklocationid=@stocklocationid,
        salediscountpercentage=@salediscountpercentage,marginpercentage=@marginpercentage,salestaxpercentage=@salestaxpercentage,purchasetaxpercentage=@purchasetaxpercentage,salestax=@salestax,wsrate=@wsrate,dealerprice=@dealerprice,
        weight=@weight,workingcharge=@workingcharge,wastage=@wastage,pentrydate=@pentrydate,weightrate=@weightrate,cesstaxid=@cesstaxid,cesstax=@cesstax,cesspercentage=@cesspercentage,
        purchasediscountpercentage=@purchasediscountpercentage,taxid=@taxid,purchasetax=@purchasetax,ptaxid=@ptaxid,designid=@designid,itemdetail=@itemdetail,sno=@sno,purchasediscount=@purchasediscount,hsncode=@hsncode,imgid=@imgid,
        salediscount=@salediscount,poinfo=@poinfo,returnqty=@returnqty,sourcesupplierid=@sourcesupplierid,itbuscode=@itbuscode {0} 
        where id=@id and version=@version and sellableqty<=(@buyingqty+@freeqty) RETURNING version,barcodes";

        //const string SQL_Delete = @"update public.lrinvoiceitems set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version  RETURNING version";

        const string SQL_UpdatePrice = @"update public.stock set discount=@discount,price=@price,displaymrp=@displaymrp,modifiedby=@modifiedby,modifiedon=@modifiedon  where invoiceitemid=@invoiceitemid";
        const string SQL_UpdateBarcode = @"update public.lrinvoiceitems set barcodes=@barcodes where id=@id";


        const string SQL_Delete = @"update public.lrinvoiceitems set isactive=false where id=@id and version=@version;

                                    update stock set isactive=false where invoiceitemid=@id;

                                    update public.salablegoods set isactive=false from stock
                                    where salablegoods.stockid = stock.id and stock.invoiceitemid = @id;";

        const string SQL_DeleteAll = @"update public.lrinvoiceitems set isactive=false,modifiedby=@modifiedby,modifiedon=@modifiedon where lrinvoiceid=@lrinvoiceid;

                                       update public.stock set isactive=false from lrinvoiceitems
                                       where lrinvoiceitems.id = stock.invoiceitemid and lrinvoiceitems.lrinvoiceid = @lrinvoiceid;

                                       update public.salablegoods set isactive=false from stock
                                       inner join lrinvoiceitems on lrinvoiceitems.id = stock.invoiceitemid 
                                       where salablegoods.stockid = stock.id and lrinvoiceitems.id = stock.invoiceitemid and lrinvoiceitems.lrinvoiceid  = @lrinvoiceid;";
        public List<String> SearchDesignList(LrinvoiceitemsCreteria _lrinvoiceitems)
        {
            List<String> lst = new List<string>();
            NpgsqlConnection connection = CreateConnection();
            try
            {
                connection.Open();
                var command = GetCommand(SQL_SearchByDesignList, connection);
                command.Parameters.Add("productid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _lrinvoiceitems.productid;
                command.Parameters.Add("suppliercompanyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _lrinvoiceitems.suppliercompanyid;
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (reader["designid"] != DBNull.Value)
                        {
                            lst.Add(reader["designid"].ToString());
                        }
                    }
                }
                connection.Close();
            }
            catch (Exception exp)
            {
                if (connection != null) connection.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _lrinvoiceitems);
            }
            return lst;
        }

        public List<LrinvoiceItemDetail> Search(NpgsqlConnection connection,
            NpgsqlTransaction transaction, LrinvoiceitemsCreteria _lrinvoiceitems)
        {
            List<LrinvoiceItemDetail> lst = new List<LrinvoiceItemDetail>();
            NpgsqlCommand command = null;
            if (_lrinvoiceitems.id != 0)
            {
                command = (transaction != null) ? GetCommand(SQL_Search, connection, transaction) : GetCommand(SQL_Search, connection);
                command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _lrinvoiceitems.id;
            }
            else if (_lrinvoiceitems.fetchprice)
            {
                QueryBuilder qb = new QueryBuilder();
                qb.CurrentQuery = SQL_LastTrans;
                qb.AddParameter("lrinvoiceitems.isactive", "isactive", true, " = ");
                if (_lrinvoiceitems.suppliercompanyid > 0)
                    qb.AddParameter("lrinvoice.suppliercompanyid", "suppliercompanyid", _lrinvoiceitems.suppliercompanyid, " = ");
                if (!string.IsNullOrEmpty(_lrinvoiceitems.designid))
                    qb.AddParameter("designid", "designid", _lrinvoiceitems.designid, " = ");
                if (_lrinvoiceitems.itemid > 0)
                    qb.AddParameter("itemid", "itemid", _lrinvoiceitems.itemid, " = ");
                else if (_lrinvoiceitems.productid > 0)
                    qb.AddParameter("productid", "productid", _lrinvoiceitems.productid, " = ");

                qb.SortField = "lrinvoiceitems.id desc";
                qb.Limit = 5;
                command = connection == null ? qb.GetCommand(connection) : qb.GetCommand(connection, transaction);
            }
            else if (_lrinvoiceitems.fetchdesign)
            {
                //designid=@designid and supplierid=@supplierid and productid=@productid
                command = GetCommand(SQL_SearchByDesign, connection);
                command.Parameters.Add("designid", NpgsqlTypes.NpgsqlDbType.Varchar).Value = string.IsNullOrEmpty(_lrinvoiceitems.designid) ? "" : _lrinvoiceitems.designid;
                command.Parameters.Add("productid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _lrinvoiceitems.productid;
                command.Parameters.Add("suppliercompanyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _lrinvoiceitems.suppliercompanyid;
            }
            else
            {
                QueryBuilder qb = new QueryBuilder();
                qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                qb.AddParameter("isactive", true, "=");

                if (_lrinvoiceitems.ids != null && _lrinvoiceitems.ids.Count > 0)
                    qb.AddInParamer("id", string.Join(",", _lrinvoiceitems.ids));

                if (_lrinvoiceitems.lrinvoiceid > 0)
                    qb.AddParameter("lrinvoiceid", _lrinvoiceitems.lrinvoiceid, "=");
                qb.Limit = 5000;
                command = qb.GetCommand(connection);
            }

            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    LrinvoiceItemDetail olrinvoiceitems = new LrinvoiceItemDetail();
                    olrinvoiceitems.productid = reader["productid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productid"].ToString());
                    olrinvoiceitems.buyingqty = reader["buyingqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["buyingqty"].ToString());
                    olrinvoiceitems.freeqty = reader["freeqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["freeqty"].ToString());
                    olrinvoiceitems.buyingprice = reader["buyingprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["buyingprice"].ToString());
                    olrinvoiceitems.mrp = reader["mrp"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["mrp"].ToString());
                    olrinvoiceitems.rowexpense = reader["rowexpense"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["rowexpense"].ToString());
                    olrinvoiceitems.rowexpensevalue = reader["rowexpensevalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["rowexpensevalue"].ToString());
                    olrinvoiceitems.purchasetotaldiscount = reader["purchasetotaldiscount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["purchasetotaldiscount"].ToString());

                    olrinvoiceitems.purchasediscount = reader["purchasediscount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["purchasediscount"].ToString());
                    olrinvoiceitems.salediscountpercentage = reader["salediscountpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["salediscountpercentage"].ToString());
                    olrinvoiceitems.marginpercentage = reader["marginpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["marginpercentage"].ToString());
                    olrinvoiceitems.purchasediscountpercentage = reader["purchasediscountpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["purchasediscountpercentage"].ToString());
                    olrinvoiceitems.salediscount = reader["salediscount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["salediscount"].ToString());
                    olrinvoiceitems.salestax = reader["salestax"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["salestax"].ToString());
                    olrinvoiceitems.purchasetaxpercentage = reader["purchasetaxpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["purchasetaxpercentage"].ToString());
                    olrinvoiceitems.salestaxpercentage = reader["salestaxpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["salestaxpercentage"].ToString());

                    olrinvoiceitems.wsrate = reader["wsrate"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["wsrate"].ToString());
                    olrinvoiceitems.dealerprice = reader["dealerprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["dealerprice"].ToString());

                    olrinvoiceitems.weight = reader["weight"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["weight"].ToString());
                    olrinvoiceitems.workingcharge = reader["workingcharge"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["workingcharge"].ToString());
                    olrinvoiceitems.wastage = reader["wastage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["wastage"].ToString());
                    olrinvoiceitems.weightrate = reader["weightrate"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["weightrate"].ToString());

                    olrinvoiceitems.cesspercentage = reader["cesspercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["cesspercentage"].ToString());
                    olrinvoiceitems.cesstax = reader["cesstax"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["cesstax"].ToString());
                    olrinvoiceitems.cesstaxid = reader["cesstaxid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["cesstaxid"]);

                    olrinvoiceitems.sellingprice = reader["sellingprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["sellingprice"].ToString());
                    olrinvoiceitems.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                    olrinvoiceitems.sno = reader["sno"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["sno"]);
                    olrinvoiceitems.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                    olrinvoiceitems.isseconds = reader["isseconds"] == DBNull.Value ? false : Convert.ToBoolean(reader["isseconds"]);
                    olrinvoiceitems.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"].ToString());
                    olrinvoiceitems.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                    olrinvoiceitems.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                    olrinvoiceitems.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                    olrinvoiceitems.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                    olrinvoiceitems.itemid = reader["itemid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["itemid"].ToString());
                    olrinvoiceitems.damagedqty = reader["damagedqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["damagedqty"].ToString());
                    olrinvoiceitems.sellableqty = reader["sellableqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["sellableqty"].ToString());
                    olrinvoiceitems.basebuyingprice = reader["basebuyingprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["basebuyingprice"].ToString());
                    olrinvoiceitems.lrinvoiceid = reader["lrinvoiceid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["lrinvoiceid"].ToString());
                    olrinvoiceitems.stocklocationid = reader["stocklocationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["stocklocationid"].ToString());
                    olrinvoiceitems.uan = reader["uan"] == DBNull.Value ? "" : Convert.ToString(reader["uan"]);
                    olrinvoiceitems.purchasetax = reader["purchasetax"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["purchasetax"].ToString());
                    olrinvoiceitems.taxid = reader["taxid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["taxid"].ToString());
                    olrinvoiceitems.ptaxid = reader["ptaxid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["ptaxid"].ToString());
                    if (olrinvoiceitems.ptaxid == 0 && olrinvoiceitems.taxid > 0)
                        olrinvoiceitems.ptaxid = olrinvoiceitems.taxid;
                    olrinvoiceitems.barcodes = reader["barcodes"] == DBNull.Value ? "null" : reader["barcodes"].ToString();
                    olrinvoiceitems.designid = reader["designid"] == DBNull.Value ? "" : reader["designid"].ToString();
                    olrinvoiceitems.hsncode = reader["hsncode"] == DBNull.Value ? "" : reader["hsncode"].ToString();
                    olrinvoiceitems.imgid = reader["imgid"] == DBNull.Value ? "" : reader["imgid"].ToString();
                    olrinvoiceitems.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"].ToString());
                    olrinvoiceitems.itemdetail = reader["itemdetail"] == DBNull.Value ? "null" : reader["itemdetail"].ToString();
                    olrinvoiceitems.POItemInfo = reader["poinfo"] == DBNull.Value ? "null" : reader["poinfo"].ToString();
                    olrinvoiceitems.amount = reader["amount"] == DBNull.Value ? ((olrinvoiceitems.buyingprice - olrinvoiceitems.purchasediscount) * olrinvoiceitems.buyingqty) : Convert.ToDecimal(reader["amount"].ToString());

                    if (_lrinvoiceitems.id != 0)
                    {
                        olrinvoiceitems.jobworkgrnitemid = reader["jobworkgrnitemid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["jobworkgrnitemid"].ToString());
                        olrinvoiceitems.buyingitemid = reader["buyingitemid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["buyingitemid"].ToString());

                    }
                    lst.Add(olrinvoiceitems);
                }
            }
            return lst;
        }
        public List<LrinvoiceItemDetail> Search(LrinvoiceitemsCreteria _lrinvoiceitems)
        {
            List<LrinvoiceItemDetail> lst = null;
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                lst = Search(conn, null, _lrinvoiceitems);
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _lrinvoiceitems);
            }
            return lst;
        }



        public bool HasInvoiceItems(long _lrinvoiceid)
        {
            NpgsqlConnection conn = CreateConnection();
            long itemcount = 0;
            try
            {
                conn.Open();

                NpgsqlCommand command = GetCommand("select count(id) items from lrinvoiceitems where lrinvoiceid=@lrinvoiceid and lrinvoiceitems.isactive=true", conn);
                command.Parameters.Add("lrinvoiceid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _lrinvoiceid;
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        itemcount = reader["items"] == DBNull.Value ? 0 : Convert.ToInt64(reader["items"].ToString());
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _lrinvoiceid);
            }
            return itemcount > 0;
        }
        public List<LrinvoiceItemDetail> SearchAllDetails(long _lrinvoiceid)
        {
            List<LrinvoiceItemDetail> lst = new List<LrinvoiceItemDetail>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                NpgsqlCommand command = null;
                if (ConfigManager.Instance(Context.OrganizationID).Config.system_customfields &&
            CustomFieldsManager.Instance(Context.OrganizationID).Config != null &&
            CustomFieldsManager.Instance(Context.OrganizationID).Config.Count > 0)
                {
                    command = GetCommand(string.Format(SQL_SearchAll, "," + string.Join(",", CustomFieldsManager.Instance(Context.OrganizationID).Config.ConvertAll(x => x.cfname))), conn);
                }
                else
                {
                    command = GetCommand(string.Format(SQL_SearchAll, ""), conn);
                }
                command.Parameters.Add("lrinvoiceid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _lrinvoiceid;
                List<long> ids = new List<long>();
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        LrinvoiceItemDetail olrinvoiceitems = new LrinvoiceItemDetail();
                        olrinvoiceitems.productid = reader["productid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productid"].ToString());
                        olrinvoiceitems.buyingqty = reader["buyingqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["buyingqty"].ToString());
                        olrinvoiceitems.freeqty = reader["freeqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["freeqty"].ToString());
                        olrinvoiceitems.actualqty = reader["actualqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["actualqty"].ToString());
                        olrinvoiceitems.buyingprice = reader["buyingprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["buyingprice"].ToString());
                        olrinvoiceitems.mrp = reader["mrp"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["mrp"].ToString());
                        olrinvoiceitems.rowexpense = reader["rowexpense"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["rowexpense"].ToString());
                        olrinvoiceitems.rowexpensevalue = reader["rowexpensevalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["rowexpensevalue"].ToString());
                        olrinvoiceitems.purchasetotaldiscount = reader["purchasetotaldiscount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["purchasetotaldiscount"].ToString());
                        olrinvoiceitems.sellingprice = reader["sellingprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["sellingprice"].ToString());
                        olrinvoiceitems.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        olrinvoiceitems.sno = reader["sno"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["sno"]);
                        olrinvoiceitems.imgid = reader["imgid"] == DBNull.Value ? "" : Convert.ToString(reader["imgid"]);
                        olrinvoiceitems.wsrate = reader["wsrate"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["wsrate"].ToString());
                        olrinvoiceitems.dealerprice = reader["dealerprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["dealerprice"].ToString());
                        olrinvoiceitems.weight = reader["weight"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["weight"].ToString());
                        olrinvoiceitems.workingcharge = reader["workingcharge"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["workingcharge"].ToString());
                        olrinvoiceitems.wastage = reader["wastage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["wastage"].ToString());
                        olrinvoiceitems.weightrate = reader["weightrate"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["weightrate"].ToString());
                        olrinvoiceitems.actualprice = reader["actualprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["actualprice"].ToString());
                        olrinvoiceitems.cesspercentage = reader["cesspercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["cesspercentage"].ToString());
                        olrinvoiceitems.cesstax = reader["cesstax"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["cesstax"].ToString());
                        olrinvoiceitems.cesstaxid = reader["cesstaxid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["cesstaxid"]);


                        olrinvoiceitems.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        olrinvoiceitems.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        olrinvoiceitems.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        olrinvoiceitems.basebuyingprice = reader["basebuyingprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["basebuyingprice"].ToString());
                        olrinvoiceitems.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        olrinvoiceitems.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        olrinvoiceitems.itemid = reader["itemid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["itemid"].ToString());
                        olrinvoiceitems.damagedqty = reader["damagedqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["damagedqty"].ToString());
                        olrinvoiceitems.sellableqty = reader["sellableqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["sellableqty"].ToString());
                        olrinvoiceitems.returnqty = reader["returnqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["returnqty"].ToString());
                        olrinvoiceitems.lrinvoiceid = reader["lrinvoiceid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["lrinvoiceid"].ToString());
                        olrinvoiceitems.stocklocationid = reader["stocklocationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["stocklocationid"].ToString());
                        olrinvoiceitems.uan = reader["uan"] == DBNull.Value ? "" : Convert.ToString(reader["uan"]);
                        olrinvoiceitems.purchasetax = reader["purchasetax"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["purchasetax"].ToString());
                        olrinvoiceitems.taxid = reader["taxid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["taxid"].ToString());
                        olrinvoiceitems.ptaxid = reader["ptaxid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["ptaxid"].ToString());
                        if (olrinvoiceitems.ptaxid == 0 && olrinvoiceitems.taxid > 0)
                            olrinvoiceitems.ptaxid = olrinvoiceitems.taxid;
                        olrinvoiceitems.barcodes = reader["barcodes"] == DBNull.Value ? "null" : reader["barcodes"].ToString();
                        olrinvoiceitems.designid = reader["designid"] == DBNull.Value ? "" : reader["designid"].ToString();
                        olrinvoiceitems.hsncode = reader["hsncode"] == DBNull.Value ? "" : reader["hsncode"].ToString();
                        olrinvoiceitems.imgid = reader["imgid"] == DBNull.Value ? "" : reader["imgid"].ToString();
                        olrinvoiceitems.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"].ToString());
                        olrinvoiceitems.itemdetail = reader["itemdetail"] == DBNull.Value ? "null" : reader["itemdetail"].ToString();
                        olrinvoiceitems.isseconds = reader["isseconds"] == DBNull.Value ? false : Convert.ToBoolean(reader["isseconds"]);

                        if (olrinvoiceitems.Details == null) olrinvoiceitems.Details = new Items();
                        olrinvoiceitems.purchasediscount = reader["purchasediscount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["purchasediscount"].ToString());
                        olrinvoiceitems.salediscountpercentage = reader["salediscountpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["salediscountpercentage"].ToString());
                        olrinvoiceitems.marginpercentage = reader["marginpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["marginpercentage"].ToString());
                        olrinvoiceitems.purchasediscountpercentage = reader["purchasediscountpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["purchasediscountpercentage"].ToString());
                        olrinvoiceitems.salediscount = reader["salediscount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["salediscount"].ToString());
                        olrinvoiceitems.salestax = reader["salestax"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["salestax"].ToString());
                        olrinvoiceitems.purchasetaxpercentage = reader["purchasetaxpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["purchasetaxpercentage"].ToString());
                        olrinvoiceitems.salestaxpercentage = reader["salestaxpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["salestaxpercentage"].ToString());
                        if (!InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_ismfgapplication && olrinvoiceitems.actualqty == 0)
                        {
                            olrinvoiceitems.actualqty = olrinvoiceitems.buyingqty - olrinvoiceitems.returnqty;
                        }
                        olrinvoiceitems.finalprice = reader["finalprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["finalprice"].ToString());
                        olrinvoiceitems.Details.sizename = reader["sizename"] == DBNull.Value ? "" : reader["sizename"].ToString();
                        olrinvoiceitems.Details.sizeid = reader["sizeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sizeid"].ToString());
                        olrinvoiceitems.Details.typename = reader["typename"] == DBNull.Value ? "" : reader["typename"].ToString();
                        olrinvoiceitems.Details.colourname = reader["colourname"] == DBNull.Value ? "" : reader["colourname"].ToString();
                        olrinvoiceitems.Details.materialname = reader["materialname"] == DBNull.Value ? "" : reader["materialname"].ToString();
                        olrinvoiceitems.Details.stylename = reader["stylename"] == DBNull.Value ? "" : reader["stylename"].ToString();
                        olrinvoiceitems.Details.brandname = reader["brandname"] == DBNull.Value ? "" : reader["brandname"].ToString();
                        olrinvoiceitems.Details.patternname = reader["patternname"] == DBNull.Value ? "" : reader["patternname"].ToString();
                        olrinvoiceitems.Details.sleevename = reader["sleevename"] == DBNull.Value ? "" : reader["sleevename"].ToString();
                        olrinvoiceitems.Details.fitname = reader["fitname"] == DBNull.Value ? "" : reader["fitname"].ToString();
                        olrinvoiceitems.sellingmode = reader["sellingmode"] == DBNull.Value ? Products.sellingMode.None : (Products.sellingMode)reader["sellingmode"];
                        olrinvoiceitems.createduser = reader["createduser"] == DBNull.Value ? "" : reader["createduser"].ToString();
                        olrinvoiceitems.modifieduser = reader["modifieduser"] == DBNull.Value ? "" : reader["modifieduser"].ToString();
                        olrinvoiceitems.itbuscode = reader["itbuscode"] == DBNull.Value ? "" : reader["itbuscode"].ToString();
                        olrinvoiceitems.srcsupplierid = reader["sourcesupplierid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sourcesupplierid"].ToString());

                        if (!IsMigration)
                        {
                            string barcode = "";
                            olrinvoiceitems.barcodecount = GetBarcodeCount(olrinvoiceitems.barcodes, out barcode);
                            olrinvoiceitems.barcode = barcode;
                        }
                        if (olrinvoiceitems.Details == null && olrinvoiceitems.itemid > 0 && !ids.Contains(olrinvoiceitems.itemid))
                        {
                            ids.Add(olrinvoiceitems.itemid);
                        }
                        else
                        {
                            if (olrinvoiceitems.Details.sizeid == 0 && reader["isizeid"] != DBNull.Value)
                                olrinvoiceitems.Details.sizeid = Convert.ToInt64(reader["isizeid"]);
                            if (olrinvoiceitems.Details.typeid == 0 && reader["typeid"] != DBNull.Value)
                                olrinvoiceitems.Details.typeid = Convert.ToInt64(reader["typeid"]);
                            if (olrinvoiceitems.Details.materialid == 0 && reader["materialid"] != DBNull.Value)
                                olrinvoiceitems.Details.materialid = Convert.ToInt64(reader["materialid"]);
                            if (olrinvoiceitems.Details.patternid == 0 && reader["patternid"] != DBNull.Value)
                                olrinvoiceitems.Details.patternid = Convert.ToInt64(reader["patternid"]);
                            if (olrinvoiceitems.Details.styleid == 0 && reader["styleid"] != DBNull.Value)
                                olrinvoiceitems.Details.styleid = Convert.ToInt64(reader["styleid"]);
                            if (olrinvoiceitems.Details.brandid == 0 && reader["brandid"] != DBNull.Value)
                                olrinvoiceitems.Details.brandid = Convert.ToInt64(reader["brandid"]);
                            if (olrinvoiceitems.Details.colourid == 0 && reader["colourid"] != DBNull.Value)
                                olrinvoiceitems.Details.colourid = Convert.ToInt64(reader["colourid"]);
                            if (olrinvoiceitems.Details.sleeveid == 0 && reader["sleeveid"] != DBNull.Value)
                                olrinvoiceitems.Details.sleeveid = Convert.ToInt64(reader["sleeveid"]);
                            if (olrinvoiceitems.Details.fitid == 0 && reader["fitid"] != DBNull.Value)
                                olrinvoiceitems.Details.fitid = Convert.ToInt64(reader["fitid"]);


                        }
                        olrinvoiceitems.POItemInfo = reader["poinfo"] == DBNull.Value ? "null" : reader["poinfo"].ToString();
                        olrinvoiceitems.amount = reader["amount"] == DBNull.Value ? ((olrinvoiceitems.buyingprice - olrinvoiceitems.purchasediscount) * olrinvoiceitems.buyingqty) : Convert.ToDecimal(reader["amount"].ToString());

                        if (ConfigManager.Instance(Context.OrganizationID).Config.system_customfields &&
            CustomFieldsManager.Instance(Context.OrganizationID).Config != null &&
            CustomFieldsManager.Instance(Context.OrganizationID).Config.Count > 0)
                        {
                            ReadCustomFields(olrinvoiceitems, reader);
                        }
                        lst.Add(olrinvoiceitems);
                    }
                }

                conn.Close();
                if (ids.Count > 0)
                {
                    ItemsDataService items = new ItemsDataService() { Context = this.Context };
                    items.Context = this.Context;
                    foreach (var item in items.Search(new ItemsCreteria(), ids))
                    {
                        lst.FindAll(e => e.Details == null && e.itemid == item.id).ForEach(ex => ex.Details = item);
                    }
                }


            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _lrinvoiceid);
            }
            return lst.OrderBy(e => e.sno).ToList();
        }


        public int GetBarcodeCount(string code, out string barcodes)
        {
            try
            {
                if (string.IsNullOrEmpty(code) || code == "null")
                {
                    barcodes = "";
                    return 0;
                }

                List<string> jsonbarcode = Newtonsoft.Json.JsonConvert.DeserializeObject<List<string>>(code);
                int result = 0;
                barcodes = string.Join(",", jsonbarcode);
                foreach (var bitem in jsonbarcode)
                {
                    var codes = bitem.Split('-');
                    if (codes.Length == 1)
                    {
                        result++;
                    }
                    else
                    {
                        bool completed = false;
                        try
                        {
                            var from = codes[0].IntFromBase62();
                            var to = codes[1].IntFromBase62();
                            result += (to - from);
                        }
                        catch (Exception exp)
                        {
                            try
                            {
                                var from = codes[0].IntFromBase36();
                                var to = codes[1].IntFromBase36();
                                result += (to - from);
                            }
                            catch (Exception exp62)
                            {

                            }
                        }
                    }
                }
                return result;
            }
            catch (Exception exp)
            {
                barcodes = "";
                return 0;
            }
        }


        public List<Lrinvoiceitems> SearchAll(long _lrinvoiceid)
        {
            List<Lrinvoiceitems> lst = new List<Lrinvoiceitems>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = GetCommand(string.Format(SQL_SearchAll, ""), conn);
                command.Parameters.Add("lrinvoiceid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _lrinvoiceid;
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Lrinvoiceitems olrinvoiceitems = new Lrinvoiceitems();
                        olrinvoiceitems.productid = reader["productid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productid"].ToString());
                        olrinvoiceitems.pricingid = reader["pricingid"] == DBNull.Value ? 0 : Convert.ToInt32(reader["pricingid"].ToString());
                        olrinvoiceitems.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        olrinvoiceitems.wsrate = reader["wsrate"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["wsrate"].ToString());
                        olrinvoiceitems.dealerprice = reader["dealerprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["dealerprice"].ToString());
                        olrinvoiceitems.weight = reader["weight"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["weight"].ToString());
                        olrinvoiceitems.workingcharge = reader["workingcharge"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["workingcharge"].ToString());
                        olrinvoiceitems.wastage = reader["wastage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["wastage"].ToString());
                        olrinvoiceitems.weightrate = reader["weightrate"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["weightrate"].ToString());

                        olrinvoiceitems.cesspercentage = reader["cesspercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["cesspercentage"].ToString());
                        olrinvoiceitems.cesstax = reader["cesstax"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["cesstax"].ToString());
                        olrinvoiceitems.cesstaxid = reader["cesstaxid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["cesstaxid"]);


                        olrinvoiceitems.sno = reader["sno"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["sno"]);
                        olrinvoiceitems.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        olrinvoiceitems.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        olrinvoiceitems.basebuyingprice = reader["basebuyingprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["basebuyingprice"].ToString());
                        olrinvoiceitems.buyingprice = reader["buyingprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["buyingprice"].ToString());
                        olrinvoiceitems.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        olrinvoiceitems.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"].ToString());
                        olrinvoiceitems.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        olrinvoiceitems.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        olrinvoiceitems.itemid = reader["itemid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["itemid"].ToString());
                        olrinvoiceitems.lrinvoiceid = reader["lrinvoiceid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["lrinvoiceid"].ToString());
                        olrinvoiceitems.stocklocationid = reader["stocklocationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["stocklocationid"].ToString());
                        olrinvoiceitems.uan = reader["uan"] == DBNull.Value ? "" : Convert.ToString(reader["uan"]);
                        olrinvoiceitems.purchasetax = reader["purchasetax"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["purchasetax"].ToString());
                        olrinvoiceitems.taxid = reader["taxid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["taxid"].ToString());
                        olrinvoiceitems.ptaxid = reader["ptaxid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["ptaxid"].ToString());
                        if (olrinvoiceitems.ptaxid == 0 && olrinvoiceitems.taxid > 0)
                            olrinvoiceitems.ptaxid = olrinvoiceitems.taxid;
                        olrinvoiceitems.designid = reader["designid"] == DBNull.Value ? "" : reader["designid"].ToString();
                        olrinvoiceitems.hsncode = reader["hsncode"] == DBNull.Value ? "" : reader["hsncode"].ToString();
                        olrinvoiceitems.imgid = reader["imgid"] == DBNull.Value ? "" : reader["imgid"].ToString();
                        olrinvoiceitems.purchasediscount = reader["purchasediscount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["purchasediscount"].ToString());
                        olrinvoiceitems.salediscountpercentage = reader["salediscountpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["salediscountpercentage"].ToString());
                        olrinvoiceitems.marginpercentage = reader["marginpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["marginpercentage"].ToString());
                        olrinvoiceitems.purchasediscountpercentage = reader["purchasediscountpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["purchasediscountpercentage"].ToString());
                        olrinvoiceitems.salediscount = reader["salediscount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["salediscount"].ToString());
                        olrinvoiceitems.sellableqty = reader["sellableqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["sellableqty"].ToString());
                        olrinvoiceitems.returnqty = reader["returnqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["returnqty"].ToString());
                        olrinvoiceitems.buyingqty = reader["buyingqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["buyingqty"].ToString());
                        olrinvoiceitems.freeqty = reader["freeqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["freeqty"].ToString());
                        olrinvoiceitems.salestax = reader["salestax"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["salestax"].ToString());
                        olrinvoiceitems.purchasetaxpercentage = reader["purchasetaxpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["purchasetaxpercentage"].ToString());
                        olrinvoiceitems.salestaxpercentage = reader["salestaxpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["salestaxpercentage"].ToString());
                        olrinvoiceitems.POItemInfo = reader["poinfo"] == DBNull.Value ? "null" : reader["poinfo"].ToString();
                        olrinvoiceitems.isseconds = reader["isseconds"] == DBNull.Value ? false : Convert.ToBoolean(reader["isseconds"]);
                        olrinvoiceitems.sellingmode = reader["sellingmode"] == DBNull.Value ? Products.sellingMode.None : (Products.sellingMode)reader["sellingmode"];
                        olrinvoiceitems.srcsupplierid = reader["sourcesupplierid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sourcesupplierid"].ToString());
                        olrinvoiceitems.itbuscode = reader["itbuscode"] == DBNull.Value ? "" : reader["itbuscode"].ToString();
                        olrinvoiceitems.finalprice = reader["finalprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["finalprice"].ToString());

                        lst.Add(olrinvoiceitems);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _lrinvoiceid);
            }
            return lst;
        }

        public List<LrinvoiceItemDetail> UpdateAll(List<LrinvoiceItemDetail> oLrinvoiceitems)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    foreach (var item in oLrinvoiceitems)
                    {
                        Update(transaction, conn, item);
                    }

                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oLrinvoiceitems);
            }

            return oLrinvoiceitems;
        }


        public LrinvoiceItemDetail Insert(LrinvoiceItemDetail oLrinvoiceitems)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    Insert(transaction, conn, oLrinvoiceitems);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oLrinvoiceitems);


            }
            return oLrinvoiceitems;
        }

        public bool UpdatePOItemQty(NpgsqlTransaction transaction, NpgsqlConnection conn, long id, bool isAdd)
        {

            NpgsqlCommand command = GetCommand(string.Format(SQL_UpdatePOItems, isAdd ? "+" : "-"), conn, transaction);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = id;
            return command.ExecuteNonQuery() > 0;
        }

        public Lrinvoiceitems Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, LrinvoiceItemDetail oLrinvoiceitems)
        {

            if (!IsMigration && oLrinvoiceitems.productid <= 0)
                throw new Exception("Lr Invoice Items can't create without product information - lrinvoice id : " + oLrinvoiceitems.lrinvoiceid);

            if (!IsMigration && oLrinvoiceitems.sellableqty > 0 && oLrinvoiceitems.sellingprice == 0)
                throw new Exception("Barcode Can't be generated with Zero Selling price.");

            if (!IsMigration && oLrinvoiceitems.buyingqty < 0)
                throw new Exception("Buying qty can't be lesser than 0");

            if (!IsMigration && Services.ConfigManager.Instance(Context.OrganizationID).Config.system_supportpricingflow
                && !Services.ConfigManager.Instance(Context.OrganizationID).Config.system_supportpricingflowproducts
                && oLrinvoiceitems.buyingprice > 0 && oLrinvoiceitems.sellingprice > 0
                && oLrinvoiceitems.marginpercentage == 0
                && oLrinvoiceitems.sellingprice > (oLrinvoiceitems.buyingprice + ((oLrinvoiceitems.buyingprice * (2 + oLrinvoiceitems.salestaxpercentage)) / 100)))
            {
                throw new Exception("Margin Percentage Not Set, Please check.");
            }

            if (!IsMigration && Services.ConfigManager.Instance(Context.OrganizationID).Config.system_supportpricingflow
                && !Services.ConfigManager.Instance(Context.OrganizationID).Config.system_supportpricingflowproducts
                && oLrinvoiceitems.buyingprice > 0 && oLrinvoiceitems.sellingprice > 0
                && oLrinvoiceitems.marginpercentage <= 0
                && oLrinvoiceitems.sellingprice < oLrinvoiceitems.buyingprice)
            {
                throw new Exception("Sell Price is lower than the Buying Price, Please check.");
            }



            if (oLrinvoiceitems.Details != null && !string.IsNullOrWhiteSpace(oLrinvoiceitems.Details.serialno))
                oLrinvoiceitems.uan = oLrinvoiceitems.Details.serialno;
            else if (oLrinvoiceitems.Details != null && !string.IsNullOrWhiteSpace(oLrinvoiceitems.Details.gln))
                oLrinvoiceitems.uan = oLrinvoiceitems.Details.gln;
            else
                oLrinvoiceitems.uan = "";


            if (Services.ConfigManager.Instance(Context.OrganizationID).Config.system_ismfgapplication &&
                Services.ConfigManager.Instance(Context.OrganizationID).Config.mfg_mode == 5 &&
                string.IsNullOrEmpty(oLrinvoiceitems.uan))
            {
                throw new Exception("UAN Can't be empty for an invoice item " + oLrinvoiceitems.designid);
            }

            NpgsqlCommand command = null;
            if (IsMigration)
                command = GetCommand(SQL_Insert_Migration, conn, transaction);
            else if (ConfigManager.Instance(Context.OrganizationID).Config.system_customfields &&
                CustomFieldsManager.Instance(Context.OrganizationID).Config != null &&
                CustomFieldsManager.Instance(Context.OrganizationID).Config.Count > 0)
            {
                var flist = CustomFieldsManager.Instance(Context.OrganizationID).Config.ConvertAll(x => x.cfname);
                var vlist = CustomFieldsManager.Instance(Context.OrganizationID).Config.ConvertAll(x => "@" + x.cfname);
                var query = string.Format(SQL_Insert, "," + string.Join(",", flist), "," + string.Join(",", vlist));
                command = GetCommand(query, conn, transaction);
                AddCustomFields(oLrinvoiceitems, command);
            }
            else
            {
                command = GetCommand(string.Format(SQL_Insert, "", ""), conn, transaction);
            }
            command.Parameters.Add("barcodes", NpgsqlTypes.NpgsqlDbType.Json).Value = string.IsNullOrEmpty(oLrinvoiceitems.barcodes) ? "null" : oLrinvoiceitems.barcodes;
            command.Parameters.Add("productid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrinvoiceitems.productid;
            command.Parameters.Add("sourceid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrinvoiceitems.sourceid;
            command.Parameters.Add("sourcesupplierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrinvoiceitems.srcsupplierid;
            command.Parameters.Add("itbuscode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = string.IsNullOrEmpty(oLrinvoiceitems.itbuscode) ? "" : oLrinvoiceitems.itbuscode;

            command.Parameters.Add("pricingid", NpgsqlTypes.NpgsqlDbType.Integer).Value = oLrinvoiceitems.pricingid;
            command.Parameters.Add("buyingqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrinvoiceitems.buyingqty;
            command.Parameters.Add("freeqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrinvoiceitems.freeqty;
            command.Parameters.Add("returnqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrinvoiceitems.returnqty;
            command.Parameters.Add("wsrate", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrinvoiceitems.wsrate;
            command.Parameters.Add("dealerprice", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrinvoiceitems.dealerprice;

            command.Parameters.Add("wastage", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrinvoiceitems.wastage;
            command.Parameters.Add("weightrate", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrinvoiceitems.weightrate;

            command.Parameters.Add("cesspercentage", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrinvoiceitems.weightrate;
            command.Parameters.Add("cesstax", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrinvoiceitems.cesstax;
            command.Parameters.Add("cesstaxid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrinvoiceitems.cesstaxid;


            command.Parameters.Add("weight", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrinvoiceitems.weight;
            command.Parameters.Add("workingcharge", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrinvoiceitems.workingcharge;

            command.Parameters.Add("buyingprice", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrinvoiceitems.buyingprice;
            command.Parameters.Add("basebuyingprice", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrinvoiceitems.basebuyingprice;
            command.Parameters.Add("mrp", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrinvoiceitems.mrp;
            command.Parameters.Add("rowexpense", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrinvoiceitems.rowexpense;
            command.Parameters.Add("rowexpensevalue", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrinvoiceitems.rowexpensevalue;
            command.Parameters.Add("purchasetotaldiscount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrinvoiceitems.purchasetotaldiscount;
            command.Parameters.Add("amount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrinvoiceitems.amount;
            command.Parameters.Add("sellingprice", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrinvoiceitems.sellingprice;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = true;
            command.Parameters.Add("isseconds", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oLrinvoiceitems.isseconds;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("sno", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrinvoiceitems.sno;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oLrinvoiceitems.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("pentrydate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = ispricing ? DateTime.MinValue : DateTimeNow;
            command.Parameters.Add("itemid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrinvoiceitems.itemid;
            command.Parameters.Add("damagedqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrinvoiceitems.damagedqty;
            command.Parameters.Add("sellableqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrinvoiceitems.sellableqty;
            command.Parameters.Add("lrinvoiceid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrinvoiceitems.lrinvoiceid;
            command.Parameters.Add("stocklocationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrinvoiceitems.stocklocationid;
            command.Parameters.Add("purchasetax", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrinvoiceitems.purchasetax;
            command.Parameters.Add("taxid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrinvoiceitems.taxid;
            command.Parameters.Add("ptaxid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrinvoiceitems.ptaxid;
            command.Parameters.Add("designid", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oLrinvoiceitems.designid == null ? "" : oLrinvoiceitems.designid;
            command.Parameters.Add("uan", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oLrinvoiceitems.uan == null ? "" : oLrinvoiceitems.uan;
            command.Parameters.Add("hsncode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = string.IsNullOrEmpty(oLrinvoiceitems.hsncode) ? "" : oLrinvoiceitems.hsncode;
            command.Parameters.Add("imgid", NpgsqlTypes.NpgsqlDbType.Varchar).Value = string.IsNullOrEmpty(oLrinvoiceitems.imgid) ? "" : oLrinvoiceitems.imgid;
            command.Parameters.Add("itemdetail", NpgsqlTypes.NpgsqlDbType.Json).Value = oLrinvoiceitems.Details == null ? "null" : oLrinvoiceitems.itemdetail;

            command.Parameters.Add("poinfo", NpgsqlTypes.NpgsqlDbType.Json).Value = oLrinvoiceitems.poinfo == null ? "null" : oLrinvoiceitems.POItemInfo;
            command.Parameters.Add("priceinfo", NpgsqlTypes.NpgsqlDbType.Json).Value = oLrinvoiceitems.priceinfo == null ? "null" : oLrinvoiceitems.PriceItemInfo;

            command.Parameters.Add("purchasediscount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrinvoiceitems.purchasediscount;
            command.Parameters.Add("salediscountpercentage", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrinvoiceitems.salediscountpercentage;
            command.Parameters.Add("marginpercentage", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrinvoiceitems.marginpercentage;
            command.Parameters.Add("purchasediscountpercentage", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrinvoiceitems.purchasediscountpercentage;
            command.Parameters.Add("salediscount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrinvoiceitems.salediscount;


            command.Parameters.Add("salestax", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrinvoiceitems.salestax;
            command.Parameters.Add("purchasetaxpercentage", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrinvoiceitems.purchasetaxpercentage;
            command.Parameters.Add("salestaxpercentage", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrinvoiceitems.salestaxpercentage;

            bool updatebarcode = false;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oLrinvoiceitems.id = Convert.ToInt64(reader[0]);
                    string barcode = reader.IsDBNull(1) ? "" : reader.GetString(1);
                    updatebarcode = oLrinvoiceitems.barcodes != barcode;
                }
            }
            if (updatebarcode && oLrinvoiceitems.barcodes != null)
                UpdateBarcode(transaction, conn, oLrinvoiceitems);

            if (oLrinvoiceitems.poinfo != null && oLrinvoiceitems.poinfo.Count > 0)
                UpdatePOItemQty(transaction, conn, oLrinvoiceitems.id, true);
            return oLrinvoiceitems;
        }

        private void ReadCustomFields(LrinvoiceItemDetail oLrinvoiceitems,
            IDataReader reader)
        {
            oLrinvoiceitems.CustomFields = new List<LrinvoiceItemDetail.CustomFieldValue>();
            foreach (var field in CustomFieldsManager.Instance(Context.OrganizationID).Config)
            {
                if (reader[field.cfname] == DBNull.Value)
                {
                    oLrinvoiceitems.CustomFields.Add(new LrinvoiceItemDetail.CustomFieldValue()
                    {
                        key = field.cfname,
                        value = null
                    });
                }
                else if (field.datatype == 1)
                {
                    oLrinvoiceitems.CustomFields.Add(new LrinvoiceItemDetail.CustomFieldValue()
                    {
                        key = field.cfname,
                        value = reader[field.cfname].ToString()
                    });
                }
                else if (field.datatype == 2)
                {
                    oLrinvoiceitems.CustomFields.Add(new LrinvoiceItemDetail.CustomFieldValue()
                    {
                        key = field.cfname,
                        value = Convert.ToDecimal(reader[field.cfname]).ToString()
                    });
                }
                else if (field.datatype == 3)
                {
                    oLrinvoiceitems.CustomFields.Add(new LrinvoiceItemDetail.CustomFieldValue()
                    {
                        key = field.cfname,
                        value = Convert.ToDateTime(reader[field.cfname]).ToString()
                    });
                }
                else
                {
                    oLrinvoiceitems.CustomFields.Add(new LrinvoiceItemDetail.CustomFieldValue()
                    {
                        key = field.cfname,
                        value = Convert.ToInt64(reader[field.cfname]).ToString()
                    });
                }
            }
        }
        private void AddCustomFields(LrinvoiceItemDetail oLrinvoiceitems,
            NpgsqlCommand command)
        {
            foreach (var field in CustomFieldsManager.Instance(Context.OrganizationID).Config)
            {
                var hasvalue = (oLrinvoiceitems.CustomFields != null && oLrinvoiceitems.CustomFields.Exists(x => x.key == field.cfname));
                LrinvoiceItemDetail.CustomFieldValue cfvalue = null;
                if (hasvalue)
                {
                    cfvalue = oLrinvoiceitems.CustomFields.Find(x => x.key == field.cfname);
                }
                if (field.datatype == 1)
                {
                    command.Parameters.Add("@" + field.cfname,
                        NpgsqlTypes.NpgsqlDbType.Varchar).Value =
                        !hasvalue ? "" : (string.IsNullOrEmpty(cfvalue.value) ? "" : cfvalue.value);
                }
                else if (field.datatype == 2)
                {
                    decimal value = 0;
                    if (hasvalue)
                        decimal.TryParse(cfvalue.value, out value);
                    command.Parameters.Add("@" + field.cfname,
                        NpgsqlTypes.NpgsqlDbType.Numeric).Value = !hasvalue ? 0 : value;
                }
                else if (field.datatype == 3)
                {
                    DateTime value = DateTime.MinValue;
                    if (hasvalue)
                        DateTime.TryParse(cfvalue.value, out value);
                    command.Parameters.Add("@" + field.cfname,
                        NpgsqlTypes.NpgsqlDbType.Timestamp).Value = value;
                }
                else
                {
                    long value = 0;
                    if (hasvalue)
                        long.TryParse(cfvalue.value, out value);
                    command.Parameters.Add("@" + field.cfname,
                        NpgsqlTypes.NpgsqlDbType.Bigint).Value = value;
                }
            }
        }

        public List<LrinvoiceItemDetail> Change2Bit(LRInvoiceItemSizeChange item)
        {
            NpgsqlTransaction transaction = null;
            List<LrinvoiceItemDetail> retValue = new List<LrinvoiceItemDetail>();
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    string query = "Update stock set isbit=true where barcode in (" + String.Join(",", item.items.ConvertAll(e => "'" + WebAPI.Data.Utility.SafeSQL(e.code) + "'")) + ");";
                    NpgsqlCommand command = GetCommand(query, conn, transaction);
                    command.ExecuteNonQuery();
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, item);
                return null;
            }
            return retValue;

        }
        public List<LrinvoiceItemDetail> UpdateQty(LRInvoiceItemSizeChange item)
        {
            bool _isstocktransfered = true;
            if (!InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_warehousenotrequired)
            {
                string _query = string.Format(@"select sum(transfered) tranqty from stock where isactive=true and invoiceitemid = {0} ", item.Core.id);
                using (var reader = ExecuteReader(_query, new SortedList<string, object>()))
                {
                    if (reader.Read() && (reader["tranqty"] == DBNull.Value ? 0 : Convert.ToInt64(reader["tranqty"])) > 0)
                    {
                        throw new Exception("Qty updation supported only for Single location with no Warehouse support mode Or Qty not transfered to store location.");
                    }
                    _isstocktransfered = false;
                }
            }
            NpgsqlTransaction transaction = null;
            List<LrinvoiceItemDetail> retValue = new List<LrinvoiceItemDetail>();
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    retValue = Search(new LrinvoiceitemsCreteria()
                    {
                        id = item.Core.id
                    });

                    if (retValue != null && retValue.Count > 0)
                    {
                        decimal newqty = item.items.Sum(e => e.qty);
                        if (retValue[0].buyingqty != newqty)
                        {
                            NpgsqlCommand command = null;
                            if (retValue[0].sellableqty > 0)
                                command = GetCommand("Update lrinvoiceitems set buyingqty=@buyingqty,sellableqty=@buyingqty,actualqty=@buyingqty,version=version+1,modifiedon=@modifiedon,modifiedby=@modifiedby where id=@invoiceitemid", conn, transaction);
                            else
                                command = GetCommand("Update lrinvoiceitems set buyingqty=@buyingqty,version=version+1,modifiedon=@modifiedon,modifiedby=@modifiedby where id=@invoiceitemid", conn, transaction);

                            command.Parameters.Add("buyingqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = newqty;
                            command.Parameters.Add("invoiceitemid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = retValue[0].id;
                            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
                            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                            command.Parameters.Add("barcode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = "";
                            command.Parameters.Add("qty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = 0;

                            var existingcount = command.ExecuteNonQuery();
                            if (existingcount == 1 && retValue[0].sellableqty > 0)
                            {
                                foreach (var stockitem in item.items)
                                {
                                    if (_isstocktransfered)
                                    {
                                        command.CommandText = @"update salablegoods set iqty=@qty,pqty=@qty,version=version+1,modifiedon=@modifiedon,modifiedby=@modifiedby  where barcode=@barcode;
                                                            update stock set qty=@qty, transfered=@qty,version=version+1,modifiedon=@modifiedon,modifiedby=@modifiedby  where barcode=@barcode ";
                                    }
                                    else
                                        command.CommandText = @"update stock set qty=@qty,version=version+1,modifiedon=@modifiedon,modifiedby=@modifiedby  where barcode=@barcode ";

                                    command.Parameters["barcode"].Value = stockitem.code;
                                    command.Parameters["qty"].Value = stockitem.qty;

                                    var affected = command.ExecuteNonQuery();
                                    if (affected > 2) return null;
                                }
                            }
                        }
                    }
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, item);
                return null;
            }
            return retValue;

        }

        public List<LrinvoiceItemDetail> ChangeSize(LRInvoiceItemSizeChange item)
        {
            NpgsqlTransaction transaction = null;
            List<LrinvoiceItemDetail> retValue = new List<LrinvoiceItemDetail>();
            bool updateStockWithNewID = false;

            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    retValue = Search(new LrinvoiceitemsCreteria()
                    {
                        id = item.Core.id
                    });

                    if (retValue != null && retValue.Count > 0)
                    {
                        ItemsDataService itemservice = new ItemsDataService() { Context = this.Context };
                        var existingitem = itemservice.Search(new ItemsCreteria()
                        {
                            id = retValue[0].itemid
                        });

                        foreach (var barcodeitem in item.items)
                        {
                            var finditem = itemservice.Search(new ItemsCreteria()
                            {
                                brandid = existingitem[0].brandid,
                                colourid = existingitem[0].colourid,
                                materialid = existingitem[0].materialid,
                                patternid = existingitem[0].patternid,
                                typeid = existingitem[0].typeid,
                                sizeid = barcodeitem.sizeid,
                                styleid = existingitem[0].styleid,
                                productid = existingitem[0].productid,
                                fitid = existingitem[0].fitid,
                                sleeveid = existingitem[0].sleeveid,
                                agmtcode = existingitem[0].agmtcode,
                                productmap = true
                            });
                            if (finditem.Count == 1)
                            {
                                retValue[0].itemid = finditem[0].id;
                                retValue[0].Details.sizeid = barcodeitem.sizeid;
                                retValue[0].Details.sizename = barcodeitem.sizename;
                                updateStockWithNewID = true;
                            }
                            else
                            {
                                var newitem = new Items()
                                {
                                    brandid = existingitem[0].brandid,
                                    colourid = existingitem[0].colourid,
                                    materialid = existingitem[0].materialid,
                                    patternid = existingitem[0].patternid,
                                    typeid = existingitem[0].typeid,
                                    sizeid = barcodeitem.sizeid,
                                    styleid = existingitem[0].styleid,
                                    productid = existingitem[0].productid,
                                    fitid = existingitem[0].fitid,
                                    sleeveid = existingitem[0].sleeveid
                                };

                                newitem.isactive = true;
                                newitem.salemode = 1;
                                newitem.version = 1;
                                newitem = itemservice.Insert(newitem);
                                retValue[0].itemid = newitem.id;
                                retValue[0].Details.sizeid = barcodeitem.sizeid;
                                retValue[0].Details.sizename = barcodeitem.sizename;
                                updateStockWithNewID = true;
                            }

                        }

                        NpgsqlCommand command = null;

                        string _SQL = @"Update lrinvoiceitems set itemid=@itemid,itemdetail=@itemdetail,version=version+1,modifiedon=@modifiedon,modifiedby=@modifiedby where id=@id;
                                            update stock set itemid=@itemid, designid=@designid,version=version+1,modifiedon=@modifiedon,modifiedby=@modifiedby  where invoiceitemid=@invoiceitemid;";

                        command = GetCommand(_SQL, conn, transaction);
                        command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = retValue[0].id;
                        command.Parameters.Add("itemid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = retValue[0].itemid;
                        command.Parameters.Add("itemdetail", NpgsqlTypes.NpgsqlDbType.Json).Value = retValue[0].Details == null ? "null" : retValue[0].itemdetail;
                        command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
                        command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;

                        command.Parameters.Add("invoiceitemid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = retValue[0].id;
                        command.Parameters.Add("designid", NpgsqlTypes.NpgsqlDbType.Varchar).Value = string.IsNullOrEmpty(retValue[0].designid) ? "" : retValue[0].designid;

                        var existingcount = command.ExecuteNonQuery();

                        transaction.Commit();
                        conn.Close();
                    }
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, item);
                return null;
            }
            return retValue;

        }

        public LrinvoiceItemDetail Update(LrinvoiceItemDetail oLrinvoiceitems)
        {
            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    //if (!string.IsNullOrEmpty(oLrinvoiceitems.barcode))
                    //{
                    //    try
                    //    {
                    //        List<string> jsonbarcodenew = new List<string>();
                    //        bool exist = false;;
                    //        List<string> jsonbarcode = Newtonsoft.Json.JsonConvert.DeserializeObject<List<string>>(oLrinvoiceitems.barcodes);
                    //        if (jsonbarcode.Contains(oLrinvoiceitems.barcode))
                    //        {
                    //            jsonbarcode.Remove(oLrinvoiceitems.barcode);
                    //            jsonbarcodenew.AddRange(jsonbarcode);
                    //            exist = true;
                    //        }
                    //        else
                    //        {
                    //            int bvalue = oLrinvoiceitems.barcode.IntFromBase62();
                    //            foreach (var bitem in jsonbarcode)
                    //            {
                    //                var codes = bitem.Split('-');
                    //                if (codes.Length == 2)
                    //                {
                    //                    List<string> bvalues = new List<string>();
                    //                    bool usenewbarcode = false;
                    //                    for (int id = codes[0].IntFromBase62(); id <= codes[1].IntFromBase62(); id++)
                    //                    {
                    //                        if (id != bvalue)
                    //                        {
                    //                            bvalues.Add(id.ToBase62());
                    //                        }
                    //                        else
                    //                        {
                    //                            usenewbarcode = true;
                    //                            exist = true;
                    //                        }
                    //                    }
                    //                    if (usenewbarcode)
                    //                        jsonbarcodenew.AddRange(bvalues);
                    //                    else
                    //                        jsonbarcodenew.Add(bitem); ;
                    //                }
                    //                else
                    //                {
                    //                    jsonbarcodenew.Add(bitem);
                    //                }

                    //            }
                    //        }

                    //        if (exist)
                    //        {
                    //            StockDataService stockdataservice = new StockDataService() { Context = this.Context };
                    //            var currentstock = stockdataservice.Search(new StockCreteria()
                    //            {
                    //                invoiceitemid = oLrinvoiceitems.id,
                    //                barcode = oLrinvoiceitems.barcode
                    //            });

                    //            if (currentstock.Count == 1)
                    //            {
                    //                var barcodenew = new List<string>();
                    //                barcodenew.Add(oLrinvoiceitems.barcode);
                    //                LrinvoiceItemDetail newInvoiceitem = Newtonsoft.Json.JsonConvert.DeserializeObject<LrinvoiceItemDetail>(Newtonsoft.Json.JsonConvert.SerializeObject(oLrinvoiceitems));
                    //                newInvoiceitem.barcodes = Newtonsoft.Json.JsonConvert.SerializeObject(barcodenew);
                    //                newInvoiceitem.buyingqty = 1;
                    //                newInvoiceitem.sellableqty = 1;

                    //                oLrinvoiceitems.buyingqty -= 1;
                    //                oLrinvoiceitems.sellableqty -= 1;
                    //                if (newInvoiceitem.sellableqty > 1)
                    //                {
                    //                    newInvoiceitem.sellableqty = 1;
                    //                    oLrinvoiceitems.sellableqty -= 1;
                    //                }
                    //                Insert(transaction, conn, newInvoiceitem);
                    //                currentstock[0].invoiceitemid = newInvoiceitem.id;
                    //                stockdataservice.Update(transaction, conn, currentstock[0]);
                    //                if (jsonbarcodenew.Count>0)
                    //                    oLrinvoiceitems.barcodes = CompressCode(jsonbarcodenew);
                    //                else
                    //                    oLrinvoiceitems.barcodes = CompressCode(barcodenew);
                    //            }
                    //        }
                    //    }
                    //    catch (Exception exp)
                    //    {
                    //        WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oLrinvoiceitems);
                    //    }
                    //}
                    Update(transaction, conn, oLrinvoiceitems);
                    InboxManager.Instance(Context.OrganizationID).UpdateLRStatus(oLrinvoiceitems.lrid,
                        Inbox.StatusList.Barcode_Generating, transaction, conn, Context);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oLrinvoiceitems);
            }
            return oLrinvoiceitems;
        }


        public Lrinvoiceitems Update(NpgsqlTransaction transaction, NpgsqlConnection conn, LrinvoiceItemDetail oLrinvoiceitems)
        {
            if (oLrinvoiceitems.productid <= 0)
                throw new Exception("Lr Invoice Items can't update without product information - lrinvoice id : " + oLrinvoiceitems.lrinvoiceid);

            if (oLrinvoiceitems.sellableqty > 0 && oLrinvoiceitems.sellingprice == 0)
                throw new Exception("Barcode Can't be generated with Zero Selling price.");

            if (!IsMigration && Services.ConfigManager.Instance(Context.OrganizationID).Config.system_supportpricingflow
                && !Services.ConfigManager.Instance(Context.OrganizationID).Config.system_supportpricingflowproducts
                && oLrinvoiceitems.buyingprice > 0 && oLrinvoiceitems.sellingprice > 0
                && oLrinvoiceitems.marginpercentage == 0
                && oLrinvoiceitems.sellingprice > (oLrinvoiceitems.buyingprice + ((oLrinvoiceitems.buyingprice * (2 + oLrinvoiceitems.salestaxpercentage)) / 100)))
            {
                throw new Exception("Margin Percentage Not Set, Please check.");
            }

            if (!IsMigration && Services.ConfigManager.Instance(Context.OrganizationID).Config.system_supportpricingflow
                && !Services.ConfigManager.Instance(Context.OrganizationID).Config.system_supportpricingflowproducts
                && oLrinvoiceitems.buyingprice > 0 && oLrinvoiceitems.sellingprice > 0
                && oLrinvoiceitems.marginpercentage <= 0
                && oLrinvoiceitems.sellingprice < oLrinvoiceitems.buyingprice)
            {
                throw new Exception("Sell Price is lower than the Buying Price, Please check.");
            }

            if (oLrinvoiceitems.Details != null && !string.IsNullOrWhiteSpace(oLrinvoiceitems.Details.serialno))
                oLrinvoiceitems.uan = oLrinvoiceitems.Details.serialno;
            else if (oLrinvoiceitems.Details != null && !string.IsNullOrWhiteSpace(oLrinvoiceitems.Details.gln))
                oLrinvoiceitems.uan = oLrinvoiceitems.Details.gln;
            else
                oLrinvoiceitems.uan = "";

            UpdatePOItemQty(transaction, conn, oLrinvoiceitems.id, false);
            NpgsqlCommand command = null;
            if (ConfigManager.Instance(Context.OrganizationID).Config.system_customfields &&
                CustomFieldsManager.Instance(Context.OrganizationID).Config != null &&
                CustomFieldsManager.Instance(Context.OrganizationID).Config.Count > 0)
            {
                var flist = CustomFieldsManager.Instance(Context.OrganizationID).Config.ConvertAll(x => x.cfname + "=@" + x.cfname);
                var query = string.Format(SQL_Update, "," + string.Join(",", flist));
                command = GetCommand(query, conn, transaction);
                AddCustomFields(oLrinvoiceitems, command);
            }
            else
            {
                command = GetCommand(string.Format(SQL_Update, ""), conn, transaction);
            }
            command.Parameters.Add("productid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrinvoiceitems.productid;
            command.Parameters.Add("pricingid", NpgsqlTypes.NpgsqlDbType.Integer).Value = oLrinvoiceitems.pricingid;
            command.Parameters.Add("buyingqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrinvoiceitems.buyingqty;
            command.Parameters.Add("freeqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrinvoiceitems.freeqty;
            command.Parameters.Add("returnqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrinvoiceitems.returnqty;
            command.Parameters.Add("buyingprice", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrinvoiceitems.buyingprice;
            command.Parameters.Add("mrp", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrinvoiceitems.mrp;
            command.Parameters.Add("rowexpense", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrinvoiceitems.rowexpense;
            command.Parameters.Add("rowexpensevalue", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrinvoiceitems.rowexpensevalue;
            command.Parameters.Add("purchasetotaldiscount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrinvoiceitems.purchasetotaldiscount;
            command.Parameters.Add("amount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrinvoiceitems.amount;
            command.Parameters.Add("sellingprice", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrinvoiceitems.sellingprice;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrinvoiceitems.id;
            command.Parameters.Add("sno", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrinvoiceitems.sno;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = true;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrinvoiceitems.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oLrinvoiceitems.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oLrinvoiceitems.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("itemid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrinvoiceitems.itemid;
            command.Parameters.Add("damagedqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrinvoiceitems.damagedqty;
            command.Parameters.Add("sellableqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrinvoiceitems.sellableqty;
            command.Parameters.Add("lrinvoiceid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrinvoiceitems.lrinvoiceid;
            command.Parameters.Add("stocklocationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrinvoiceitems.stocklocationid;
            command.Parameters.Add("uan", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oLrinvoiceitems.uan == null ? "" : oLrinvoiceitems.uan;
            command.Parameters.Add("wsrate", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrinvoiceitems.wsrate;
            command.Parameters.Add("dealerprice", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrinvoiceitems.dealerprice;
            command.Parameters.Add("wastage", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrinvoiceitems.wastage;
            command.Parameters.Add("weightrate", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrinvoiceitems.weightrate;
            command.Parameters.Add("cesspercentage", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrinvoiceitems.weightrate;
            command.Parameters.Add("cesstax", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrinvoiceitems.cesstax;
            command.Parameters.Add("cesstaxid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrinvoiceitems.cesstaxid;
            command.Parameters.Add("weight", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrinvoiceitems.weight;
            command.Parameters.Add("workingcharge", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrinvoiceitems.workingcharge;
            command.Parameters.Add("pentrydate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = ispricing ? DateTime.MinValue : DateTimeNow;
            command.Parameters.Add("purchasetax", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrinvoiceitems.purchasetax;
            command.Parameters.Add("taxid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrinvoiceitems.taxid;
            command.Parameters.Add("ptaxid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrinvoiceitems.ptaxid;
            command.Parameters.Add("designid", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oLrinvoiceitems.designid == null ? "" : oLrinvoiceitems.designid;
            command.Parameters.Add("hsncode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oLrinvoiceitems.hsncode;
            command.Parameters.Add("imgid", NpgsqlTypes.NpgsqlDbType.Varchar).Value = string.IsNullOrEmpty(oLrinvoiceitems.imgid) ? "" : oLrinvoiceitems.imgid;
            command.Parameters.Add("basebuyingprice", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrinvoiceitems.basebuyingprice;
            command.Parameters.Add("itemdetail", NpgsqlTypes.NpgsqlDbType.Json).Value = oLrinvoiceitems.Details == null ? "null" : oLrinvoiceitems.itemdetail;
            command.Parameters.Add("purchasediscount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrinvoiceitems.purchasediscount;
            command.Parameters.Add("salediscountpercentage", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrinvoiceitems.salediscountpercentage;
            command.Parameters.Add("marginpercentage", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrinvoiceitems.marginpercentage;
            command.Parameters.Add("purchasediscountpercentage", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrinvoiceitems.purchasediscountpercentage;
            command.Parameters.Add("salediscount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrinvoiceitems.salediscount;
            command.Parameters.Add("salestax", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrinvoiceitems.salestax;
            command.Parameters.Add("purchasetaxpercentage", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrinvoiceitems.purchasetaxpercentage;
            command.Parameters.Add("salestaxpercentage", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrinvoiceitems.salestaxpercentage;
            command.Parameters.Add("poinfo", NpgsqlTypes.NpgsqlDbType.Json).Value = oLrinvoiceitems.poinfo == null ? "null" : oLrinvoiceitems.POItemInfo;
            command.Parameters.Add("priceinfo", NpgsqlTypes.NpgsqlDbType.Json).Value = oLrinvoiceitems.priceinfo == null ? "null" : oLrinvoiceitems.PriceItemInfo;
            command.Parameters.Add("sourcesupplierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrinvoiceitems.srcsupplierid;
            command.Parameters.Add("itbuscode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = string.IsNullOrEmpty(oLrinvoiceitems.itbuscode) ? "" : oLrinvoiceitems.itbuscode;

            bool updatebarcode = false;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oLrinvoiceitems.version = Convert.ToInt32(reader[0]);
                    string barcode = reader.IsDBNull(1) ? "" : reader.GetString(1);
                    if (oLrinvoiceitems.barcodes != "NOT GENERATED")
                        updatebarcode = oLrinvoiceitems.barcodes != barcode;
                }
            }

            if (updatebarcode)
                UpdateBarcode(transaction, conn, oLrinvoiceitems);

            if (Services.ConfigManager.Instance(Context.OrganizationID).Config.system_disableupdateonbarcode)
            {
                if (oLrinvoiceitems.sellableqty > 0) throw new Exception("Updatation disabled after barcode generation.");
            }

            if (!IsMigration)
            {
                string priceChangeQuery = SQL_UpdateStockPrice;
                if (Services.ConfigManager.Instance(Context.OrganizationID).Config.system_syncshowroomprice)
                {
                    priceChangeQuery = SQL_UpdateStockPrice + @";update salablegoods set price=@price,discount=@discount,modifiedby=@modifiedby,modifiedon=@modifiedon  from stock
                                                            where salablegoods.stockid = stock.id and stock.invoiceitemid = @invoiceitemid;";
                }
                if (oLrinvoiceitems.sellableqty > 0)
                {
                    decimal _eprice = oLrinvoiceitems.sellingprice / (oLrinvoiceitems.salestaxpercentage + 100) * 100;
                    command = GetCommand(priceChangeQuery, conn, transaction);
                    command.Parameters.Add("productid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrinvoiceitems.productid;
                    command.Parameters.Add("price", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrinvoiceitems.sellingprice;
                    command.Parameters.Add("dealerprice", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrinvoiceitems.dealerprice;
                    command.Parameters.Add("invoiceitemid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrinvoiceitems.id;
                    command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                    command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
                    command.Parameters.Add("itemid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrinvoiceitems.itemid;
                    command.Parameters.Add("discount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrinvoiceitems.salediscount;
                    command.Parameters.Add("displaymrp", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrinvoiceitems.mrp;
                    command.Parameters.Add("eprice", NpgsqlTypes.NpgsqlDbType.Numeric).Value = _eprice;
                    if (oLrinvoiceitems.Details == null || string.IsNullOrEmpty(string.IsNullOrEmpty(oLrinvoiceitems.Details.printingname) ? oLrinvoiceitems.Details.productname : oLrinvoiceitems.Details.printingname))
                    {
                        ProductsDataService proddata = new ProductsDataService() { Context = this.Context };
                        var products = proddata.Search(new ProductsCreteria()
                        {
                            id = oLrinvoiceitems.productid
                        });
                        command.Parameters.Add("printingname", NpgsqlTypes.NpgsqlDbType.Varchar).Value = products[0].name;
                    }
                    else
                    {
                        string printingname = string.IsNullOrEmpty(oLrinvoiceitems.Details.printingname) ? oLrinvoiceitems.Details.productname : oLrinvoiceitems.Details.printingname;
                        command.Parameters.Add("printingname", NpgsqlTypes.NpgsqlDbType.Varchar).Value = printingname;
                    }
                    command.ExecuteNonQuery();
                }

                if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_ismfgapplication)
                {
                    command = GetCommand("Update lrinvoiceitems set actualqty = @actualqty where id=@id", conn, transaction);
                    command.Parameters.Add("actualqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrinvoiceitems.actualqty;
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrinvoiceitems.id;
                    command.ExecuteNonQuery();
                }
                if (oLrinvoiceitems.poinfo != null && oLrinvoiceitems.poinfo.Count > 0)
                    UpdatePOItemQty(transaction, conn, oLrinvoiceitems.id, true);
            }
            return oLrinvoiceitems;
        }

        public bool IsMigration { get; set; }
        public bool Update(NpgsqlTransaction transaction, NpgsqlConnection conn, List<LrinvoiceItemDetail> list,
            long id, bool ismerge)
        {
            List<Lrinvoiceitems> existingItems = null;
            if (!ismerge)
            {
                existingItems = SearchAll(id);
                if (existingItems != null && existingItems.Count > 0)
                {
                    var deletedItems = existingItems.FindAll(e => !list.Exists(f => f.id == e.id));
                    if (deletedItems.Count > 0)
                    {
                        if (deletedItems.Exists(e => e.sellableqty > 0 || e.returnqty > 0))
                        {
                            throw new Exception("Can't delete invoice items with barcode generated., Please remove barcodes.");
                        }
                        foreach (var item in deletedItems)
                        {
                            Delete(transaction, conn, item);
                        }
                    }

                    //var costModified = existingItems.FindAll(e => e.sellableqty > 0 && list.Exists(f => f.id == e.id && f.buyingprice != e.buyingprice));
                    var costModified = list.FindAll(e => existingItems.Exists(f => f.id == e.id && f.buyingprice != e.buyingprice && f.sellableqty > 0));
                    var qtyModified = existingItems.FindAll(e => e.sellableqty > 0 && list.Exists(f => f.id == e.id && f.buyingqty != e.buyingqty));

                    if (costModified != null && costModified.Count > 0)
                    {
                        if (!Context.HasPermission(PermissionSet.barcodegen, InfyPOS.Common.Action.Modify))
                            throw new Exception("Don't have permission to change cost price after barcode generation");
                    }
                    foreach (var row in costModified)
                    {
                        var param = new SortedList<string, object>();
                        param.Add("id", row.id);
                        param.Add("buyingprice", row.buyingprice);
                        param.Add("ibuyingprice", (row.purchasetax > 0 ? (row.purchasetax / row.buyingqty) + row.buyingprice : row.buyingprice));
                        string query = "update stock set buyingprice=@buyingprice,ibuyingprice=@ibuyingprice where invoiceitemid=@id";
                        if (row.buyingprice <= 0)
                            throw new Exception("Can't update stock with zero purchase cost.");
                        ExecuteQuery(query, param, conn, transaction);
                    }

                    foreach (var row in qtyModified)
                    {
                        if (row.sellingmode == Products.sellingMode.Piece)
                            continue;
                        var newqty = list.Find(e => e.id == row.id).buyingqty;
                        if (newqty < row.buyingqty)
                        {
                            throw new Exception(string.Join(",", row.barcodeList) + " conflict, Newly changed qty can't be lesser than old qty");
                        }
                        var param = new SortedList<string, object>();
                        param.Add("id", row.id);
                        param.Add("qty", newqty);
                        string query = "update stock set qty=@qty where invoiceitemid=@id";
                        ExecuteQuery(query, param, conn, transaction);
                    }
                }
            }
            Data.ItemsDataService itemservice = new ItemsDataService() { Context = this.Context };
            itemservice.Context = this.Context;
            List<long> availableIds = new List<long>();

            if (list.Count > 1 && list[0].poinfo != null && list.Count == list[0].poinfo.Count)
            {
                List<POItemInfo> poitems = list[0].poinfo;
                int i = 0;
                foreach (var item in list)
                {
                    item.poinfo = new List<POItemInfo>();
                    item.poinfo.Add(poitems[i]);
                    i++;
                }
            }
            foreach (var item in list)
            {
                if (item.id > 0)
                {
                    if (availableIds.Contains(item.id))
                        item.id = 0;
                    else
                        availableIds.Add(item.id);
                }
                if (item.itemid == 0)
                {
                    var ritem = itemservice.Search(new ItemsCreteria()
                    {
                        brandid = item.Details.brandid,
                        productid = item.productid,
                        sizeid = item.Details.sizeid,
                        typeid = item.Details.typeid,
                        styleid = item.Details.styleid,
                        materialid = item.Details.materialid,
                        patternid = item.Details.patternid,
                        colourid = item.Details.colourid,
                        fitid = item.Details.fitid,
                        sleeveid = item.Details.sleeveid,
                        agmtcode = item.itbuscode,
                        gln = item.uan,
                        productmap = true
                    });
                    //item.Details.productid = item.productid;
                    if (ritem != null && ritem.Count > 0)
                    {
                        item.itemid = ritem[0].id;
                    }
                    else if (item.Details.sourceitemid > 0)
                    {
                        ritem = itemservice.Search(new ItemsCreteria()
                        {
                            id = item.Details.sourceitemid
                        });
                        if (ritem.Count == 1)
                        {
                            ritem[0].productid = item.Details.productid;
                            ritem[0].productname = item.Details.productname;
                            ritem[0].sizeid = item.Details.sizeid;
                            ritem[0].sizename = item.Details.sizename;
                            ritem[0].patternid = item.Details.patternid;
                            ritem[0].patternname = item.Details.patternname;
                            ritem[0].materialid = item.Details.materialid;
                            ritem[0].materialname = item.Details.materialname;
                            ritem[0].typeid = item.Details.typeid;
                            ritem[0].typename = item.Details.typename;
                            ritem[0].colourid = item.Details.colourid;
                            ritem[0].colourname = item.Details.colourname;
                            ritem[0].brandid = item.Details.brandid;
                            ritem[0].brandname = item.Details.brandname;
                            ritem[0].styleid = item.Details.styleid;
                            ritem[0].stylename = item.Details.stylename;
                            ritem[0].fitid = item.Details.fitid;
                            ritem[0].fitname = item.Details.fitname;
                            ritem[0].sleeveid = item.Details.sleeveid;
                            ritem[0].sleevename = item.Details.sleevename;
                            ritem[0].agmtcode = item.itbuscode;
                            ritem[0].gln = item.uan;
                            ritem[0].id = 0;
                            ritem[0].createdon = DateTimeNow;
                            ritem[0].modifiedon = DateTimeNow;
                            ritem[0].createdby = Context.UserID;
                            ritem[0].modifiedby = Context.UserID;
                            itemservice.Insert(ritem[0]);
                            item.itemid = ritem[0].id;
                        }
                    }
                    else
                    {
                        var items = new Items()
                        {
                            productid = item.Details.productid,
                            productname = item.Details.productname,
                            sizeid = item.Details.sizeid,
                            sizename = item.Details.sizename,
                            patternid = item.Details.patternid,
                            patternname = item.Details.patternname,
                            materialid = item.Details.materialid,
                            materialname = item.Details.materialname,
                            typeid = item.Details.typeid,
                            typename = item.Details.typename,
                            colourid = item.Details.colourid,
                            colourname = item.Details.colourname,
                            brandid = item.Details.brandid,
                            brandname = item.Details.brandname,
                            styleid = item.Details.styleid,
                            stylename = item.Details.stylename,
                            fitid = item.Details.fitid,
                            fitname = item.Details.fitname,
                            sleeveid = item.Details.sleeveid,
                            sleevename = item.Details.sleevename,
                            printingname = string.IsNullOrEmpty(item.Details.printingname) ? item.Details.productname : item.Details.printingname,
                            sellingname = item.Details.sellingname,
                            agmtcode = item.itbuscode,
                            gln = item.uan
                        };
                        itemservice.Insert(transaction, conn, items);
                        item.itemid = items.id;
                    }
                }

                item.isactive = true;
                if (string.IsNullOrEmpty(item.hsncode))
                    item.hsncode = "-";
                if (item.id > 0)
                {
                    if (!ismerge)
                    {
                        if (item.sellingmode == Products.sellingMode.Pack
                            || item.sellingmode == Products.sellingMode.Cut)
                        {
                            if (item.sellableqty > 0)
                                item.sellableqty = item.buyingqty + item.freeqty - item.returnqty;
                        }
                        Update(transaction, conn, item);
                        if (existingItems.Exists(e => e.id == item.id))
                        {
                            var existingitem = existingItems.Find(e => e.id == item.id);
                            if (item.itemid != existingitem.itemid ||
                                item.productid != existingitem.productid ||
                                item.designid != existingitem.designid)
                            {
                                UpdateStock(transaction, conn, item);
                            }
                        }
                    }
                }
                else
                {
                    item.lrinvoiceid = id;
                    Insert(transaction, conn, item);
                }
            }
            return true;
        }

        public bool Delete(Lrinvoiceitems oLrinvoiceitems)
        {

            NpgsqlTransaction transaction = null;
            int successCount = 0;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    Delete(transaction, conn, oLrinvoiceitems);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oLrinvoiceitems);
            }
            return successCount > 0;
        }

        public bool UpdateStock(NpgsqlTransaction transaction, NpgsqlConnection conn, LrinvoiceItemDetail oLrinvoiceitems)
        {
            NpgsqlCommand command = GetCommand("Update Stock set printingname=@printingname,itemid=@itemid,productid=@productid,designid=@designid,hsncode=@hsncode where invoiceitemid=@invoiceitemid", conn, transaction);
            command.Parameters.Add("printingname", NpgsqlTypes.NpgsqlDbType.Varchar).Value =
                string.IsNullOrEmpty(oLrinvoiceitems.Details.printingname) ? (string.IsNullOrEmpty(oLrinvoiceitems.Details.productname) ? "" : oLrinvoiceitems.Details.productname) : oLrinvoiceitems.Details.printingname;
            command.Parameters.Add("itemid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrinvoiceitems.itemid;
            command.Parameters.Add("productid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrinvoiceitems.productid;
            command.Parameters.Add("designid", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oLrinvoiceitems.designid == null ? "" : oLrinvoiceitems.designid;
            command.Parameters.Add("hsncode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oLrinvoiceitems.hsncode == null ? "" : oLrinvoiceitems.hsncode;
            command.Parameters.Add("invoiceitemid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrinvoiceitems.id;
            var existingcount = command.ExecuteNonQuery();
            command.CommandText = @"update salablegoods set itemid=@itemid where stockid in (select id from stock where invoiceitemid=@invoiceitemid)";
            return command.ExecuteNonQuery() > 0 && existingcount > 0;
        }

        public bool UpdateBarcode(NpgsqlTransaction transaction, NpgsqlConnection conn, Lrinvoiceitems oLrinvoiceitems)
        {
            NpgsqlCommand command = GetCommand(SQL_UpdateBarcode, conn, transaction);
            command.Parameters.Add("barcodes", NpgsqlTypes.NpgsqlDbType.Json).Value = string.IsNullOrEmpty(oLrinvoiceitems.barcodes) ? "null" : oLrinvoiceitems.barcodes;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrinvoiceitems.id;
            return (command.ExecuteNonQuery() > 0);
        }
        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Lrinvoiceitems oLrinvoiceitems)
        {

            string candeletequery = string.Format(@"select count(*) from lrinvoiceitems
            where id = {0} and barcodes is not null and barcodes->0 is not null", oLrinvoiceitems.id);
            using (var reader = ExecuteReader(candeletequery, new SortedList<string, object>()))
            {
                if (reader.Read() && Convert.ToInt32(reader[0]) > 0)
                {
                    throw new Exception("this invoice items already barcode generated,please remove those barcode before delete.");
                }
            }

            candeletequery = string.Format(@"select count(*) from stock where invoiceitemid={0} and isactive=true", oLrinvoiceitems.id);
            using (var reader = ExecuteReader(candeletequery, new SortedList<string, object>()))
            {
                if (reader.Read() && Convert.ToInt32(reader[0]) > 0)
                {
                    throw new Exception("this invoice items already barcode generated,please remove those barcode before delete.");
                }
            }

            candeletequery = string.Format(@"select count(*) from stockuan where invoiceitemid={0} and isactive=true", oLrinvoiceitems.id);
            using (var reader = ExecuteReader(candeletequery, new SortedList<string, object>()))
            {
                if (reader.Read() && Convert.ToInt32(reader[0]) > 0)
                {
                    throw new Exception("this invoice items already barcode generated,please remove those barcode before delete.");
                }
            }

            NpgsqlCommand command = GetCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = false;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oLrinvoiceitems.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrinvoiceitems.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0 && UpdatePOItemQty(transaction, conn, oLrinvoiceitems.id, false);
        }

        public bool UploadSecondaryBarcode(Lrinvoiceitemsbarcode item)
        {
            var items = Search(new LrinvoiceitemsCreteria()
            {
                id = item.id
            });

            if (items.Count == 1)
            {
                if (items[0].Details == null)
                    items[0].Details = new Items();

                if (items[0].sellableqty == 0 && !string.IsNullOrEmpty(item.barcodes))
                {
                    var secondarybarcodeList = item.barcodeList;
                    if (secondarybarcodeList != null && secondarybarcodeList.Count > 0)
                    {
                        items[0].Details.serialno = string.Join(",", secondarybarcodeList);
                        Update(items[0]);
                        return true;
                    }
                }
            }
            return false;
        }
        public bool UploadBarcode(Lrinvoiceitemsbarcode item)
        {
            NpgsqlTransaction transaction = null;
            try
            {
                if (item.itemid == 0)
                {
                    ItemsDataService itemservice = new ItemsDataService() { Context = this.Context };
                    itemservice.Context = this.Context;
                    var iteminfo = itemservice.Search(new ItemsCreteria()
                    {
                        brandid = item.Details.brandid,
                        colourid = item.Details.colourid,
                        materialid = item.Details.materialid,
                        patternid = item.Details.patternid,
                        typeid = item.Details.typeid,
                        sizeid = item.Details.sizeid,
                        styleid = item.Details.styleid,
                        productid = item.productid,
                        sleeveid = item.Details.sleeveid,
                        fitid = item.Details.fitid,
                        agmtcode = item.itbuscode,
                        productmap = true
                    });
                    if (iteminfo.Count == 1)
                    {
                        item.itemid = iteminfo[0].id;
                    }
                    else
                    {
                        var newitem = item.Details;
                        newitem.isactive = true;
                        newitem.salemode = 1;
                        newitem.version = 1;
                        newitem = itemservice.Insert(newitem);
                        item.itemid = newitem.id;
                    }
                }

                ProductsDataService productdataservice = new ProductsDataService() { Context = this.Context };
                var products = productdataservice.Search(new ProductsCreteria()
                {
                    id = item.productid
                });

                SalablegoodsDataService salablegoodsDataService = new SalablegoodsDataService() { Context = this.Context };
                salablegoodsDataService.Context = this.Context;

                //bool isbase36 = (products.Count == 1 && products[0].barcodemode == 1);
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    long supplierid = 0;
                    decimal buyingprice = 0;
                    string lrreference = "";
                    string designid = "";
                    long locationid = 0;
                    long retaillocationid = 0;
                    Items itemdetail = null;
                    Products.sellingMode sellingmode = Products.sellingMode.None;
                    NpgsqlCommand command = GetCommand(StockDataService.SQL_SearchSupplierID, conn);
                    command.Parameters.Add("lrinvoiceitemsid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.id;
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            supplierid = reader.GetInt64(0);
                            retaillocationid = reader["retaillocationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["retaillocationid"]);
                            locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                            buyingprice = reader["buyingprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["buyingprice"].ToString());
                            decimal sno = reader["sno"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["sno"].ToString());
                            string lrentryno = reader["lrentryno"] == DBNull.Value ? "" : reader["lrentryno"].ToString();
                            designid = reader["designid"] == DBNull.Value ? "" : reader["designid"].ToString();
                            lrreference = string.Format("{0}.{1}", lrentryno, sno);
                            sellingmode = reader["sellingmode"] == DBNull.Value ? Products.sellingMode.None : (Products.sellingMode)reader["sellingmode"];
                            if (reader["itemdetail"] != DBNull.Value &&
                                !string.IsNullOrEmpty(reader["itemdetail"].ToString()) &&
                                reader["itemdetail"].ToString() != "null")
                            {
                                itemdetail = Newtonsoft.Json.JsonConvert.DeserializeObject<Items>(reader["itemdetail"].ToString());
                            }
                        }
                    }
                    transaction = conn.BeginTransaction();



                    var barcodelist = item.barcodeList;
                    barcodelist.RemoveAll(e => string.IsNullOrEmpty(e) || e == "null");

                    List<string> uniquebarcodes = new List<string>();
                    foreach (var barcodeitems in barcodelist)
                    {
                        uniquebarcodes.AddRange(barcodeitems.Split(','));
                    }

                    uniquebarcodes.RemoveAll(e => string.IsNullOrEmpty(e));
                    if (uniquebarcodes.Count == 0) return false;

                    command = GetCommand(string.Format(StockDataService.SQL_ValidateBarcode,
                        string.Join(",", uniquebarcodes.ConvertAll(e => "'" + WebAPI.Data.Utility.SafeSQL(e) + "'"))), conn, transaction);

                    var duplicatebarcodelist = new List<string>();
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                            duplicatebarcodelist.Add(reader.GetString(0));

                        if (reader.NextResult())
                            while (reader.Read())
                                duplicatebarcodelist.Add(reader.GetString(0));
                    }
                    if (duplicatebarcodelist.Count > 0)
                    {
                        WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(new Exception("Duplicate barcodes"),
                            WebAPI.Common.Logger.LogType.Error, duplicatebarcodelist);
                        return false;
                    }

                    item.stock = new List<Stock>();
                    for (int i = 0; i < barcodelist.Count; i++)
                    {
                        var secondarycodes = barcodelist[i].Split(',').ToList();
                        string barcode = secondarycodes[0];
                        var stck = item.CreateStock(barcode);
                        secondarycodes.RemoveAll(e => e == barcode);
                        stck.serialno = string.Join(",", secondarycodes);
                        stck.qty = 1;
                        item.stock.Add(stck);
                    }

                    bool countmissmatch = true;
                    int retry = 0;
                    while (countmissmatch && retry < 3)
                    {
                        string query = @"update public.lrinvoiceitems set version=@version+1,modifiedby=@modifiedby,itemid=@itemid,modifiedon=@modifiedon,barcodes=@barcodes,sellableqty=sellableqty+@sellableqty 
                                             where id=@id and version=@version and (lrinvoiceitems.buyingqty - (lrinvoiceitems.damagedqty+lrinvoiceitems.sellableqty))>=@sellableqty RETURNING version,sellableqty";
                        command = GetCommand(query, conn, transaction);
                        command.Parameters.Add("itemid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.itemid;
                        command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                        command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
                        command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = item.version;
                        command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.id;
                        command.Parameters.Add("sellableqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = Convert.ToDecimal(item.stock.Sum(e => e.qty));
                        command.Parameters.Add("barcodes", NpgsqlTypes.NpgsqlDbType.Json).Value = item.barcodes;
                        using (var dr = command.ExecuteReader())
                        {
                            if (dr.Read())
                            {
                                item.version = dr.GetInt32(0);
                                item.sellableqty = dr.GetDecimal(1);
                                countmissmatch = false;
                                break;
                            }
                        }
                        command.CommandText = "Select version,buyingqty,damagedqty,sellableqty from lrinvoiceitems where id=@id";
                        using (var dr = command.ExecuteReader())
                        {
                            if (dr.Read())
                            {
                                var version = dr.GetInt32(0);
                                var buyingqty = dr.GetDecimal(1);
                                var damagedqty = dr.GetDecimal(1);
                                var sellableqty = dr.GetDecimal(1);
                            }
                        }
                        retry++;
                    }

                    if (!countmissmatch)
                    {
                        StockDataService stockDataService = new StockDataService() { Context = this.Context };

                        SellablebarcodesDataService sellablebarcodesDataService = new SellablebarcodesDataService() { Context = this.Context };
                        foreach (var stock in item.stock)
                        {
                            stock.supplierid = supplierid;
                            stock.buyingprice = buyingprice;
                            stock.lrreference = lrreference;
                            stock.designid = designid;
                            stock.stockdate = DateTimeNow;
                            stock.locationid = locationid;
                            if (itemdetail != null && string.IsNullOrWhiteSpace(itemdetail.serialno))
                            {
                                stock.batch = itemdetail.batch;
                                stock.expiry = itemdetail.expiry;
                                stock.serialno = itemdetail.serialno;
                            }
                            stock.purchaseqty = stock.qty;
                            stockDataService.Insert(transaction, conn, stock);
                            if (retaillocationid > 0)
                            {
                                var existingsalableitem = salablegoodsDataService.Search(new SalablegoodsCreteria()
                                {
                                    barcode = stock.barcode,
                                    locationid = retaillocationid
                                });

                                if (existingsalableitem.Count == 0)
                                {
                                    Salablegoods sitem = new Salablegoods();
                                    sitem.inwardon = DateTimeNow;
                                    sitem.barcode = stock.barcode;
                                    sitem.bundleid = -5;
                                    sitem.iqty = stock.qty;
                                    sitem.isactive = true;
                                    sitem.itemid = stock.itemid;
                                    sitem.locationid = retaillocationid;
                                    sitem.stockid = stock.id;
                                    sitem.version = 1;
                                    sitem.companyid = stock.companyid;
                                    sitem.supplierid = stock.supplierid;
                                    sitem.received = true;
                                    sitem.price = stock.price;
                                    sitem.discount = stock.discount;
                                    sitem.additionaldiscount = stock.additionaldiscount;
                                    salablegoodsDataService.Insert(sitem);

                                    command = GetCommand(@"update stock set transfered=transfered+" + stock.qty + " where id=" + stock.id, conn, transaction);
                                    command.ExecuteNonQuery();
                                }
                                else
                                {
                                    existingsalableitem[0].iqty += stock.qty;
                                    salablegoodsDataService.Update(existingsalableitem[0], existingsalableitem[0]);
                                    command = GetCommand(@"update stock set transfered=transfered+" + stock.qty + " where id=" + stock.id, conn, transaction);
                                    command.ExecuteNonQuery();

                                }
                            }

                            if (!string.IsNullOrEmpty(stock.serialno) && InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_fixed_barcodes &&
                                !InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_hyper_uan)
                            {
                                foreach (var secondarybarcodes in stock.serialno.Split(','))
                                {
                                    if (string.IsNullOrEmpty(secondarybarcodes)) continue;
                                    sellablebarcodesDataService.Insert(transaction, conn,
                                        new Sellablebarcodes()
                                        {
                                            barcode = secondarybarcodes,
                                            stockid = stock.id,
                                            productid = stock.productid,
                                            itemid = stock.itemid,
                                            companyid = stock.companyid,
                                            isactive = true,
                                            createdby = stock.createdby,
                                            modifiedby = stock.modifiedby,
                                            createdon = stock.createdon,
                                            modifiedon = stock.modifiedon,
                                            version = 1
                                        });
                                }
                            }
                        }
                        transaction.Commit();
                        conn.Close();
                    }
                    else
                    {
                        item = null;
                        transaction.Rollback();
                        conn.Close();
                    }
                }
                return true;
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);
                return false;
            }
        }

        public bool DeleteAll(Lrinvoice lrinvoiceid, long userid, NpgsqlConnection conn, NpgsqlTransaction transaction)
        {
            int successCount = 0;
            if (lrinvoiceid.Lrinvoiceitems != null)
            {
                foreach (var item in lrinvoiceid.Lrinvoiceitems)
                    UpdatePOItemQty(transaction, conn, item.id, false);
            }

            string candeletequery = string.Format(@"select count(*) from lrinvoiceitems
            where lrinvoiceid = {0} and barcodes is not null and barcodes->0 is not null", lrinvoiceid.id);
            using (var reader = ExecuteReader(candeletequery, new SortedList<string, object>()))
            {
                if (reader.Read() && Convert.ToInt32(reader[0]) > 0)
                {
                    throw new Exception("Some of the invoice items already barcode generated,please remove those barcode before delete.");
                }
            }

            if (lrinvoiceid.Lrinvoiceitems != null)
            {
                candeletequery = string.Format(@"select count(*) from stock where invoiceitemid in ({0}) and isactive=true", string.Join(",", lrinvoiceid.Lrinvoiceitems.ConvertAll(x => x.id)));
                using (var reader = ExecuteReader(candeletequery, new SortedList<string, object>()))
                {
                    if (reader.Read() && Convert.ToInt32(reader[0]) > 0)
                    {
                        throw new Exception("This invoice items already barcode generated,please remove those barcode before delete.");
                    }
                }

                candeletequery = string.Format(@"select count(*) from stockuan where invoiceitemid in ({0}) and isactive=true", string.Join(",", lrinvoiceid.Lrinvoiceitems.ConvertAll(x => x.id)));
                using (var reader = ExecuteReader(candeletequery, new SortedList<string, object>()))
                {
                    if (reader.Read() && Convert.ToInt32(reader[0]) > 0)
                    {
                        throw new Exception("This invoice items already barcode generated,please remove those barcode before delete.");
                    }
                }
            }

            NpgsqlCommand command = GetCommand(SQL_DeleteAll, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = false;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("lrinvoiceid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = lrinvoiceid.id;
            successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

        internal string CreateB2BSKU(BarcodeImageRequest item)
        {

            string query = string.Format(@"select sum(buyingqty - (sellableqty-returnqty)) from lrinvoiceitems 
                             where buyingqty>sellableqty-returnqty and id in ({0})",
                             string.Join(",", item.ids));
            int qty = 0;
            using (var reader = ExecuteReader(query, null))
            {
                if (reader.Read())
                {
                    if (!reader.IsDBNull(0))
                        qty = reader.GetInt32(0);
                }
            }
            if (qty > 0)
                throw new Exception("Please generate pending barcode - " + qty);

            query = string.Format(@"select stock.id stockid,lrinvoiceitems.designid,
                    suppliercompany.supplierid,items.id itemid,
                    items.productid,items.sizeid,items.brandid,items.sleeveid,
                    items.fitid,items.patternid,items.materialid,items.styleid,
                    items.colourid,stock.dealerprice,stock.displaymrp,
                    stock.buyingprice,stock.price
                    from lrinvoiceitems 
                    inner join lrinvoice on lrinvoice.id = lrinvoiceid
                    inner join suppliercompany on suppliercompany.id = lrinvoice.suppliercompanyid
                    inner join stock on stock.invoiceitemid =lrinvoiceitems.id
                    inner join items on items.id = stock.itemid
                    where lrinvoiceitems.id in ({0}) 
                    and lrinvoiceitems.designid is not null and  lrinvoiceitems.designid!=''",
                    string.Join(",", item.ids));

            List<SKUStock> stocklist = new List<SKUStock>();
            using (var reader = ExecuteReader(query, null))
            {
                while (reader.Read())
                {
                    SKUStock sKUStock = new SKUStock();
                    stocklist.Add(sKUStock);
                    sKUStock.productid = reader["productid"] != DBNull.Value ? Convert.ToInt64(reader["productid"]) : 0;
                    sKUStock.stockid = reader["stockid"] != DBNull.Value ? Convert.ToInt64(reader["stockid"]) : 0;
                    sKUStock.supplierid = reader["supplierid"] != DBNull.Value ? Convert.ToInt64(reader["supplierid"]) : 0;
                    sKUStock.materialid = reader["materialid"] != DBNull.Value ? Convert.ToInt64(reader["materialid"]) : 0;
                    sKUStock.brandid = reader["brandid"] != DBNull.Value ? Convert.ToInt64(reader["brandid"]) : 0;
                    sKUStock.colourid = reader["colourid"] != DBNull.Value ? Convert.ToInt64(reader["colourid"]) : 0;
                    sKUStock.fitid = reader["fitid"] != DBNull.Value ? Convert.ToInt64(reader["fitid"]) : 0;
                    sKUStock.itemid = reader["itemid"] != DBNull.Value ? Convert.ToInt64(reader["itemid"]) : 0;
                    sKUStock.patternid = reader["patternid"] != DBNull.Value ? Convert.ToInt64(reader["patternid"]) : 0;
                    sKUStock.sizeid = reader["sizeid"] != DBNull.Value ? Convert.ToInt64(reader["sizeid"]) : 0;
                    sKUStock.sleeveid = reader["sleeveid"] != DBNull.Value ? Convert.ToInt64(reader["sleeveid"]) : 0;
                    sKUStock.styleid = reader["styleid"] != DBNull.Value ? Convert.ToInt64(reader["styleid"]) : 0;
                    sKUStock.dealerprice = reader["dealerprice"] != DBNull.Value ? Convert.ToDecimal(reader["dealerprice"]) : 0;
                    sKUStock.mrp = reader["displaymrp"] != DBNull.Value ? Convert.ToDecimal(reader["displaymrp"]) : 0;
                    sKUStock.buyingprice = reader["buyingprice"] != DBNull.Value ? Convert.ToDecimal(reader["buyingprice"]) : 0;
                    sKUStock.price = reader["price"] != DBNull.Value ? Convert.ToDecimal(reader["price"]) : 0;
                    sKUStock.designid = reader["designid"] != DBNull.Value ? Convert.ToString(reader["designid"]) : "";
                    sKUStock.designname = reader["designid"] != DBNull.Value ? Convert.ToString(reader["designid"]) : "";
                }
            }
            return SyncB2BDesign(stocklist);
        }

        public string SyncB2BDesign()
        {
            string query = @"select stock.id stockid,lrinvoiceitems.designid,
                    suppliercompany.supplierid,items.id itemid,
                    items.productid,items.sizeid,items.brandid,items.sleeveid,
                    items.fitid,items.patternid,items.materialid,items.styleid,
                    items.colourid,stock.dealerprice,stock.displaymrp,
                    stock.buyingprice,stock.price
                    from lrinvoiceitems 
                    inner join lrinvoice on lrinvoice.id = lrinvoiceid
					inner join suppliercompany on suppliercompany.id = lrinvoice.suppliercompanyid
                    inner join stock on stock.invoiceitemid =lrinvoiceitems.id
                    inner join items on items.id = stock.itemid
                    where  
                     lrinvoiceitems.designid is not null and  lrinvoiceitems.designid!=''
					 and lrinvoiceitems.id in (
						 select invoiceitemid from stock 
						 inner join salablegoods on salablegoods.stockid = stock.id
						 where (serialno = '' or serialno is null) and 
					 	iqty-(oqty+dqty+jqty)>0)";
            List<SKUStock> stocklist = new List<SKUStock>();
            using (var reader = ExecuteReader(query, null))
            {
                while (reader.Read())
                {
                    SKUStock sKUStock = new SKUStock();
                    stocklist.Add(sKUStock);
                    sKUStock.productid = reader["productid"] != DBNull.Value ? Convert.ToInt64(reader["productid"]) : 0;
                    sKUStock.stockid = reader["stockid"] != DBNull.Value ? Convert.ToInt64(reader["stockid"]) : 0;
                    sKUStock.supplierid = reader["supplierid"] != DBNull.Value ? Convert.ToInt64(reader["supplierid"]) : 0;
                    sKUStock.materialid = reader["materialid"] != DBNull.Value ? Convert.ToInt64(reader["materialid"]) : 0;
                    sKUStock.brandid = reader["brandid"] != DBNull.Value ? Convert.ToInt64(reader["brandid"]) : 0;
                    sKUStock.colourid = reader["colourid"] != DBNull.Value ? Convert.ToInt64(reader["colourid"]) : 0;
                    sKUStock.fitid = reader["fitid"] != DBNull.Value ? Convert.ToInt64(reader["fitid"]) : 0;
                    sKUStock.itemid = reader["itemid"] != DBNull.Value ? Convert.ToInt64(reader["itemid"]) : 0;
                    sKUStock.patternid = reader["patternid"] != DBNull.Value ? Convert.ToInt64(reader["patternid"]) : 0;
                    sKUStock.sizeid = reader["sizeid"] != DBNull.Value ? Convert.ToInt64(reader["sizeid"]) : 0;
                    sKUStock.sleeveid = reader["sleeveid"] != DBNull.Value ? Convert.ToInt64(reader["sleeveid"]) : 0;
                    sKUStock.styleid = reader["styleid"] != DBNull.Value ? Convert.ToInt64(reader["styleid"]) : 0;
                    sKUStock.dealerprice = reader["dealerprice"] != DBNull.Value ? Convert.ToDecimal(reader["dealerprice"]) : 0;
                    sKUStock.mrp = reader["displaymrp"] != DBNull.Value ? Convert.ToDecimal(reader["displaymrp"]) : 0;
                    sKUStock.buyingprice = reader["buyingprice"] != DBNull.Value ? Convert.ToDecimal(reader["buyingprice"]) : 0;
                    sKUStock.price = reader["price"] != DBNull.Value ? Convert.ToDecimal(reader["price"]) : 0;
                    sKUStock.designid = reader["designid"] != DBNull.Value ? Convert.ToString(reader["designid"]) : "";
                    sKUStock.designname = reader["designid"] != DBNull.Value ? Convert.ToString(reader["designid"]) : "";
                }
            }

            foreach (var dgroup in stocklist.GroupBy(x => x.designid))
            {
                var suppliergroups = dgroup.GroupBy(x => new { x.supplierid, x.productid }).ToList();
                if (suppliergroups.Count > 1)
                {
                    for (var index = 1; index < suppliergroups.Count; index++)
                    {
                        suppliergroups.ForEach(x =>
                        {
                            x.ForEach(y => y.designid = y.designid + "-" + index.ToString("F0"));
                        });
                    }
                }
            }
            return SyncB2BDesign(stocklist);
        }

        public string SyncB2BDesign(List<SKUStock> stocklist)
        {
            int newlycreateddesigns = 0;
            int newlycreateduid = 0;

            MfgdesignDataService mfgdesignDataService = new MfgdesignDataService() { Context = this.Context };
            var mfglist = mfgdesignDataService.Search(new MfgdesignCreteria()
            {
                codes = stocklist.ConvertAll(x => x.designid)
            });

            using (var conn = CreateConnection())
            {
                conn.Open();
                var transaction = conn.BeginTransaction();
                foreach (var stockgrp in stocklist.GroupBy(x => x.designid))
                {
                    var mfgdesign = mfglist.Find(x => x.code == stockgrp.Key);
                    if (mfgdesign == null)
                    {
                        newlycreateddesigns++;
                        var first = stockgrp.First();
                        mfgdesign = new Mfgdesign();
                        mfgdesign.code = stockgrp.Key;
                        mfgdesign.name = stockgrp.Key;
                        mfgdesign.displayname = first.designname;
                        mfgdesign.isactive = true;
                        mfgdesign.version = 1;
                        mfgdesign.createdby = Context.UserID;
                        mfgdesign.modifiedby = Context.UserID;
                        mfgdesign.sizegroup = new Mfgdesign.Sizegroup() { items = new List<Mfgdesign.Sizegroup.Item>() };
                        mfgdesign.productid = first.productid;
                        mfgdesign.supplierid = first.supplierid;
                        mfgdesign.brandid = first.brandid;
                        mfgdesign.styleid = first.styleid;
                        mfgdesign.sleeveid = first.sleeveid;
                        mfgdesign.fitid = first.fitid;
                        mfgdesign.materialid = first.materialid;
                        mfgdesign.patternid = first.patternid;
                        var colourid = 0;
                        foreach (var sizeitems in stockgrp.GroupBy(x => x.colourid))
                        {
                            colourid++;
                            var sequence = SequenceManager.Instance(Context.OrganizationID).Get("mfgdesign.autonumber", 0);
                            var result = sequence.GetNextNumber(Context, conn, transaction);
                            var uid = 0;
                            foreach (var sizegrpitem in sizeitems.GroupBy(x => x.sizeid))
                            {
                                newlycreateduid++;
                                uid++;
                                var sizeitem = sizegrpitem.First();
                                var sizegroupitem = new Mfgdesign.Sizegroup.Item();
                                sizegroupitem.colourid = sizeitem.colourid;
                                sizegroupitem.itemid = sizeitem.itemid;
                                sizegroupitem.code = mfgdesign.code + "/" + colourid.ToString("F0");
                                sizegroupitem.uid = result + "-" + uid.ToString("F0");
                                sizegroupitem.sizeid = sizeitem.sizeid;
                                sizegroupitem.sleeveid = sizeitem.sleeveid;
                                sizegroupitem.wsrate = sizeitem.dealerprice;
                                sizegroupitem.mrp = sizeitem.mrp;
                                sizegroupitem.cost = sizeitem.buyingprice;
                                sizegroupitem.salerate = sizeitem.price;
                                mfgdesign.sizegroup.items.Add(sizegroupitem);
                                sizegrpitem.ForEach(x => x.uid = sizegroupitem.uid);
                            }
                        }
                        mfgdesignDataService.Insert(transaction, conn, mfgdesign);
                    }
                    else
                    {
                        if (mfgdesign.supplierid != stockgrp.First().supplierid)
                        {
                            throw new Exception(stockgrp.Key + " Design already associated with another supplier, try to add some prefix/suffix to the design");
                        }

                        if (mfgdesign.productid != stockgrp.First().productid)
                        {
                            throw new Exception(stockgrp.Key + " Product already associated with another product, try to add some prefix/suffix to the design");
                        }

                        var maxcolourid = mfgdesign.sizegroup.items.ConvertAll(x => int.Parse(x.code.Split('/')[0])).ToList().OrderByDescending(x => x).First();

                        foreach (var sizeitems in stockgrp.GroupBy(x => x.colourid))
                        {
                            int colourid = 0;
                            var uid = 0;
                            var existcolour = mfgdesign.sizegroup.items.FindAll(x => x.colourid == sizeitems.Key);
                            if (existcolour != null && existcolour.Count > 0)
                            {
                                colourid = int.Parse(existcolour[0].code.Split('/')[1]);
                                uid = existcolour.ConvertAll(x => int.Parse(x.uid.Split('-')[0])).ToList().OrderByDescending(x => x).First();
                            }
                            else
                            {
                                maxcolourid++;
                                colourid = maxcolourid;
                            }
                            var sequence = SequenceManager.Instance(Context.OrganizationID).Get("mfgdesign.autonumber", 0);
                            var result = sequence.GetNextNumber(Context, conn, transaction);

                            foreach (var sizegrpitem in sizeitems.GroupBy(x => x.sizeid))
                            {
                                if (existcolour.Exists(x => x.sizeid == sizegrpitem.Key))
                                {
                                    var sizeuid = existcolour.Find(x => x.sizeid == sizegrpitem.Key).uid;
                                    sizegrpitem.ForEach(x => x.uid = sizeuid);
                                    continue;
                                }
                                var sizeitem = sizegrpitem.First();
                                uid++;
                                newlycreateduid++;
                                var sizegroupitem = new Mfgdesign.Sizegroup.Item();
                                sizegroupitem.colourid = sizeitem.colourid;
                                sizegroupitem.itemid = sizeitem.itemid;
                                sizegroupitem.code = mfgdesign.code + "/" + colourid.ToString("F0");
                                sizegroupitem.uid = result + "-" + uid.ToString("F0");
                                sizegroupitem.sizeid = sizeitem.sizeid;
                                sizegroupitem.sleeveid = sizeitem.sleeveid;
                                sizegroupitem.wsrate = sizeitem.dealerprice;
                                sizegroupitem.mrp = sizeitem.mrp;
                                sizegroupitem.cost = sizeitem.buyingprice;
                                sizegroupitem.salerate = sizeitem.price;
                                mfgdesign.sizegroup.items.Add(sizegroupitem);

                                sizegrpitem.ForEach(x => x.uid = sizegroupitem.uid);
                            }
                        }

                        mfgdesignDataService.Update(transaction, conn, mfgdesign, null);
                    }
                }
                foreach (var stockitem in stocklist)
                {
                    var param = new SortedList<string, object>();
                    param.Add("serialno", stockitem.uid);
                    param.Add("stockid", stockitem.stockid);
                    var query = "Update stock set serialno=@serialno where id=@stockid";
                    ExecuteQuery(query, param, conn, transaction);
                }
                transaction.Commit();
                conn.Close();
            }
            return "Design Crated - " + newlycreateddesigns + ", UID Created " + newlycreateduid;
        }

        internal void ChangeNewBuyingQty(LrinvoiceItemDetail item)
        {
            var oldqty = item.buyingqty;
            item.amount = item.amount / item.buyingqty * item.newbuyingqty;
            if (item.purchasediscount > 0)
                item.purchasediscount = item.purchasediscount / item.buyingqty * item.newbuyingqty;
            if (item.purchasetax > 0)
                item.purchasetax = item.purchasetax / item.buyingqty * item.newbuyingqty;
            if (item.salestax > 0)
                item.salestax = item.salestax / item.buyingqty * item.newbuyingqty;
            item.buyingqty = item.newbuyingqty;

            string query = @"update stock set qty=@qty,
modifiedby=@modifiedby,modifiedon=@modifiedon,version=stock.version+1 
where invoiceitemid=@id and qty=@oldqty;

update stock set transfered=@qty where invoiceitemid=@id and transfered=@oldqty;

update salablegoods set iqty=@qty,
modifiedby=@modifiedby,modifiedon=@modifiedon,version=salablegoods.version+1 from stock
where stock.invoiceitemid=@id and stock.id = salablegoods.stockid and iqty=@oldqty and oqty<=@qty;

update lrinvoiceitems set amount=@amount,purchasediscount=@purchasediscount,
purchasetax=@purchasetax,salestax=@salestax,buyingqty=@qty,
modifiedby=@modifiedby,modifiedon=@modifiedon,version=version+1 where id=@id;";

            try
            {
                using (var conn = CreateConnection())
                {
                    conn.Open();
                    var transaction = conn.BeginTransaction();
                    NpgsqlCommand command = GetCommand(query, conn, transaction);
                    command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = false;
                    command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                    command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.id;
                    command.Parameters.Add("oldqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oldqty;
                    command.Parameters.Add("qty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = item.buyingqty;
                    command.Parameters.Add("amount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = item.amount;
                    command.Parameters.Add("purchasediscount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = item.purchasediscount;
                    command.Parameters.Add("purchasetax", NpgsqlTypes.NpgsqlDbType.Numeric).Value = item.purchasetax;
                    command.Parameters.Add("salestax", NpgsqlTypes.NpgsqlDbType.Numeric).Value = item.salestax;
                    var affectedrecords = command.ExecuteNonQuery();
                    if (affectedrecords != 4)
                    {
                        transaction.Rollback();
                        throw new Exception("Can't change the quantity,please do it in product screen");
                    }
                    else
                    {
                        transaction.Commit();
                    }
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                throw exp;
            }
        }

        internal void SyncUANSerial(NpgsqlConnection conn, NpgsqlTransaction transaction, List<Lrinvoiceitemsbarcode> uanmissmatches)
        {
            foreach (var item in uanmissmatches)
            {
                if (!string.IsNullOrEmpty(item.uan))
                    item.Details.serialno = item.uan;
                else
                    item.uan = item.Details.serialno;
                NpgsqlCommand command = GetCommand("Update lrinvoiceitems set itemdetail=@itemdetail,uan=@uan where id=@id", conn, transaction);
                command.Parameters.Add("uan", NpgsqlTypes.NpgsqlDbType.Varchar).Value = item.uan == null ? "" : item.uan;
                command.Parameters.Add("itemdetail", NpgsqlTypes.NpgsqlDbType.Json).Value = item.Details == null ? "null" : item.itemdetail;
                command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.id;
                command.ExecuteNonQuery();
            }
        }

        public class SKUStock
        {
            public long stockid { get; set; }
            public string designid { get; set; }
            public long supplierid { get; set; }
            public long itemid { get; set; }
            public long productid { get; set; }
            public string designname { get; set; }
            public long sizeid { get; set; }
            public long brandid { get; set; }
            public long sleeveid { get; set; }
            public long fitid { get; set; }
            public long patternid { get; set; }
            public long materialid { get; set; }
            public long styleid { get; set; }
            public long colourid { get; set; }
            public decimal dealerprice { get; set; }
            public decimal mrp { get; set; }
            public decimal buyingprice { get; set; }
            public decimal price { get; set; }
            public string uid { get; set; }
        }
    }

}
