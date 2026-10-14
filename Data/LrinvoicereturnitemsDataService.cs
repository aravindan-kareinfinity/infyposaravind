using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;

namespace InfyPOS.Data
{
    public class LrinvoicereturnitemsDataService : SAASData
    {
        const string SQL_Search = @"select productid, id, isactive, createdby, modifiedby, version, createdon, modifiedon, itemid, qty, 
            barcode, stockid, lrinvoiceitemsid, taxid,imageid, rate, tax, discount, lrinvoicereturnid,designid,lrinvoicereturnitems.taxpercentage,
            lrinvoicereturnitems.amount,lrinvoicereturnitems.salablegoodsid,discountpercentage from lrinvoicereturnitems where id=@id";

        const string SQL_SearchAll = @"select productid, id, isactive, createdby, modifiedby, version, createdon, modifiedon, itemid, 
            qty, barcode, stockid, lrinvoiceitemsid, taxid,imageid, rate, tax, discount, discountpercentage, lrinvoicereturnid,designid,
            lrinvoicereturnitems.taxpercentage,lrinvoicereturnitems.amount, lrinvoicereturnitems.salablegoodsid,lrinvoicereturnitems.uan 
            from lrinvoicereturnitems where lrinvoicereturnid=@lrinvoicereturnid and isactive=true";

        const string SQL_SearchAllByType = @"
select productid, lrinvoicereturnitems.id, lrinvoicereturnitems.isactive, 
lrinvoicereturnitems.createdby, lrinvoicereturnitems.modifiedby, lrinvoicereturnitems.version, 
lrinvoicereturnitems.createdon, lrinvoicereturnitems.modifiedon, lrinvoicereturnitems.itemid, 
lrinvoicereturnitems.qty, lrinvoicereturnitems.barcode, lrinvoicereturnitems.stockid, 
lrinvoicereturnitems.lrinvoiceitemsid, lrinvoicereturnitems.taxid,imageid, lrinvoicereturnitems.rate, 
lrinvoicereturnitems.tax, lrinvoicereturnitems.discount, lrinvoicereturnitems.lrinvoicereturnid,
lrinvoicereturnitems.designid,lrinvoicereturnitems.taxpercentage,lrinvoicereturnitems.amount,lrinvoicereturnitems.uan,
lrinvoicereturnitems.salablegoodsid,lrinvoicereturn.returnno, lrinvoicereturn.returndate, lrinvoicereturnitems.discountpercentage 
from lrinvoicereturnitems 
inner join lrinvoicereturn on lrinvoicereturn.id = lrinvoicereturnid
where lrinvoiceitemsid=@lrinvoiceitemsid and 
lrinvoicereturn.ReturnType = @returntype and
lrinvoicereturnitems.isactive=true and lrinvoicereturn.isactive=true";

        const string SQL_SearchAllWithTax = @"select lrinvoicereturnitems.productid, lrinvoicereturnitems.id, lrinvoicereturnitems.isactive, lrinvoicereturnitems.createdby, lrinvoicereturnitems.modifiedby, lrinvoicereturnitems.version, 
                                    lrinvoicereturnitems.createdon, lrinvoicereturnitems.modifiedon, lrinvoicereturnitems.itemid, lrinvoicereturnitems.qty, 
									lrinvoicereturnitems.barcode, stockid, lrinvoiceitemsid, lrinvoice.invoiceno,lrinvoice.invoicedate,
				                    lrinvoicereturnitems.taxid,lrinvoicereturnitems.imageid, rate, tax, products.name as printingname, lrinvoicereturnitems.discount, lrinvoicereturnitems.discountpercentage, 
                                    lrinvoicereturnid,taxsplit,tax.name as taxname,lrinvoiceitems.hsncode, stock.hsncode stockhsncode,lrinvoicereturnitems.designid,lrinvoicereturnitems.taxpercentage,lrinvoicereturnitems.amount,
                                    lrinvoicereturnitems.salablegoodsid,lrinvoiceitems.sno invoiceserialno,lrentry.lrentryno,stock.serialno,
                                    brand.name brandname, size.name as sizename, style.name as stylename,lrinvoicereturnitems.uan from lrinvoicereturnitems
                                    left outer join lrinvoiceitems on lrinvoiceitems.id = lrinvoiceitemsid and lrinvoiceitems.isactive=true
                                    left outer join stock on stock.id = lrinvoicereturnitems.stockid
                                    left outer join lrinvoice on lrinvoice.id = lrinvoiceitems.lrinvoiceid  and lrinvoice.isactive = true
                                    left outer join lrentry on lrentry.id = lrinvoice.lrid  and lrentry.isactive = true
                                    left outer join products on products.id = lrinvoicereturnitems.productid
                                    left outer join tax on tax.id = lrinvoicereturnitems.taxid
                                    left outer join items on items.id = lrinvoiceitems.itemid
                                    left outer join brand on brand.id = items.brandid
                                    left outer join referencelist size on size.id = items.sizeid 
                                    left outer join referencelist style on style.id = items.styleid
                                    where lrinvoicereturnid=@lrinvoicereturnid and lrinvoicereturnitems.isactive=true";

        const string SQL_Insert = @"INSERT INTO public.lrinvoicereturnitems(productid, isactive, createdby, modifiedby, version, createdon, modifiedon, itemid, qty, barcode, stockid, lrinvoiceitemsid, taxid,imageid, 
                    rate, tax, discount, lrinvoicereturnid,designid,taxpercentage,amount,salablegoodsid,discountpercentage,uan) VALUES(@productid, @isactive, @createdby, @modifiedby, @version, 
                    @createdon, @modifiedon, @itemid, @qty, @barcode, @stockid, @lrinvoiceitemsid, @taxid,@imageid, @rate, @tax, @discount, @lrinvoicereturnid,@designid,@taxpercentage,@amount,
                    @salablegoodsid,@discountpercentage,@uan) RETURNING id";

        const string SQL_UpdateReturnGoods = @"Update stock set transfered=transfered-@qty,purchasereturnqty=COALESCE(purchasereturnqty,0)-@qty where id=@stockid;
                                               Update salablegoods set iqty=iqty-@qty,rqty=COALESCE(rqty,0)+@qty where id=@salablegoodsid RETURNING salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty),barcode; ";

        const string SQL_UpdateReturnStock = @"Update lrinvoiceitems set returnqty=returnqty+@qty where id=@lrinvoiceitemsid;
                                               Update Stock set returnqty=returnqty+@qty,purchasereturnqty=COALESCE(purchasereturnqty,0)+@qty where id=@stockid RETURNING qty-(damagedqty+journalqty+returnqty+transfered),barcode; ";

        const string SQL_UpdateExcessStock = @"Update lrinvoiceitems set returnqty=returnqty+@qty,actualqty=buyingqty+(@qty*-1) where id=@lrinvoiceitemsid;";

        const string SQL_UpdateReturnGoodsRollback = @"Update salablegoods set iqty=iqty+@qty,rqty=COALESCE(rqty,0)-@qty where id=@salablegoodsid;
                                               Update stock set transfered=transfered+@qty,purchasereturnqty=COALESCE(purchasereturnqty,0)-@qty where id=@stockid;";

        const string SQL_UpdateReturnStockRollback = @"Update Stock set returnqty=returnqty-@qty,purchasereturnqty=COALESCE(purchasereturnqty,0)-@qty  where id=@stockid;
                                               Update lrinvoiceitems set returnqty=returnqty-@qty where id=@lrinvoiceitemsid; ";

        const string SQL_Update = @"UPDATE public.lrinvoicereturnitems SET productid=@productid, modifiedby=@modifiedby, version=@version+1, modifiedon=@modifiedon, itemid=@itemid, qty=@qty, barcode=@barcode, stockid=@stockid, lrinvoiceitemsid=@lrinvoiceitemsid,
                                    taxid=@taxid,designid=@designid,imageid=@imageid, rate=@rate, tax=@tax, discount=@discount,salablegoodsid=@salablegoodsid, lrinvoicereturnid=@lrinvoicereturnid,taxpercentage=@taxpercentage,amount=@amount,discountpercentage=@discountpercentage
                                    where id=@id and version=@version RETURNING version";

        const string SQL_Delete = @"update public.lrinvoicereturnitems set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version  RETURNING version";
        const string SQL_DeleteAll = @"update public.lrinvoicereturnitems set isactive=false,modifiedby=@modifiedby,modifiedon=@modifiedon where lrinvoicereturnid=@lrinvoicereturnid";
        public List<Lrinvoicereturnitems> Search(LrinvoicereturnitemsCreteria _lrinvoicereturnitems)
        {
            List<Lrinvoicereturnitems> lst = new List<Lrinvoicereturnitems>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_lrinvoicereturnitems.id != 0)
                {
                    command = GetCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _lrinvoicereturnitems.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("isactive", true, "=");

                    //if (!string.IsNullOrEmpty(_lrinvoicereturnitems.Code))
                    //    qb.AddParameter("code", _lrinvoicereturnitems.Code, "=");

                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Lrinvoicereturnitems olrinvoicereturnitems = new Lrinvoicereturnitems();
                        olrinvoicereturnitems.productid = reader["productid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productid"].ToString());
                        olrinvoicereturnitems.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        olrinvoicereturnitems.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        olrinvoicereturnitems.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        olrinvoicereturnitems.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        olrinvoicereturnitems.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"].ToString());
                        olrinvoicereturnitems.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        olrinvoicereturnitems.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        olrinvoicereturnitems.itemid = reader["itemid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["itemid"].ToString());
                        olrinvoicereturnitems.salablegoodsid = reader["salablegoodsid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["salablegoodsid"].ToString());
                        olrinvoicereturnitems.qty = reader["qty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["qty"].ToString());
                        olrinvoicereturnitems.designid = reader["designid"] == DBNull.Value ? "" : reader["designid"].ToString();
                        olrinvoicereturnitems.imageid = reader["imageid"] == DBNull.Value ? "" : reader["imageid"].ToString();
                        olrinvoicereturnitems.barcode = reader["barcode"] == DBNull.Value ? "" : reader["barcode"].ToString();
                        olrinvoicereturnitems.uan = reader["uan"] == DBNull.Value ? "" : reader["uan"].ToString();
                        olrinvoicereturnitems.stockid = reader["stockid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["stockid"].ToString());
                        olrinvoicereturnitems.lrinvoiceitemsid = reader["lrinvoiceitemsid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["lrinvoiceitemsid"].ToString());
                        olrinvoicereturnitems.taxid = reader["taxid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["taxid"].ToString());
                        olrinvoicereturnitems.rate = reader["rate"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["rate"].ToString());
                        olrinvoicereturnitems.tax = reader["tax"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["tax"].ToString());
                        olrinvoicereturnitems.amount = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"].ToString());
                        olrinvoicereturnitems.taxpercentage = reader["taxpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["taxpercentage"].ToString());
                        olrinvoicereturnitems.discount = reader["discount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discount"].ToString());
                        olrinvoicereturnitems.discountpercentage = reader["discountpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discountpercentage"].ToString());
                        olrinvoicereturnitems.lrinvoicereturnid = reader["lrinvoicereturnid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["lrinvoicereturnid"].ToString());
                        lst.Add(olrinvoicereturnitems);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _lrinvoicereturnitems);
            }
            return lst;
        }

        public List<Lrinvoicereturnitemdetail> SearchAllByReturnType(long _lrinvoicereturnitemid, int returntype)
        {
            List<Lrinvoicereturnitemdetail> lst = new List<Lrinvoicereturnitemdetail>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = GetCommand(SQL_SearchAllByType, conn);
                command.Parameters.Add("lrinvoiceitemsid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _lrinvoicereturnitemid;
                command.Parameters.Add("returntype", NpgsqlTypes.NpgsqlDbType.Integer).Value = returntype;
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Lrinvoicereturnitemdetail olrinvoicereturnitems = new Lrinvoicereturnitemdetail();
                        olrinvoicereturnitems.returnno = reader["returnno"] == DBNull.Value ? "" : reader["returnno"].ToString();
                        olrinvoicereturnitems.returndate = reader["returndate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["returndate"]);

                        olrinvoicereturnitems.productid = reader["productid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productid"].ToString());
                        olrinvoicereturnitems.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        olrinvoicereturnitems.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        olrinvoicereturnitems.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        olrinvoicereturnitems.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        olrinvoicereturnitems.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"].ToString());
                        olrinvoicereturnitems.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        olrinvoicereturnitems.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        olrinvoicereturnitems.itemid = reader["itemid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["itemid"].ToString());
                        olrinvoicereturnitems.barcode = reader["barcode"] == DBNull.Value ? "" : reader["barcode"].ToString();
                        olrinvoicereturnitems.uan = reader["uan"] == DBNull.Value ? "" : reader["uan"].ToString();
                        olrinvoicereturnitems.designid = reader["designid"] == DBNull.Value ? "" : reader["designid"].ToString();
                        olrinvoicereturnitems.salablegoodsid = reader["salablegoodsid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["salablegoodsid"].ToString());
                        olrinvoicereturnitems.stockid = reader["stockid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["stockid"].ToString());
                        olrinvoicereturnitems.lrinvoiceitemsid = reader["lrinvoiceitemsid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["lrinvoiceitemsid"].ToString());
                        olrinvoicereturnitems.taxid = reader["taxid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["taxid"].ToString());
                        olrinvoicereturnitems.imageid = reader["imageid"] == DBNull.Value ? "" : reader["imageid"].ToString();
                        olrinvoicereturnitems.lrinvoicereturnid = reader["lrinvoicereturnid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["lrinvoicereturnid"].ToString());
                        olrinvoicereturnitems.qty = reader["qty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["qty"].ToString());
                        olrinvoicereturnitems.rate = reader["rate"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["rate"].ToString());
                        olrinvoicereturnitems.tax = reader["tax"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["tax"].ToString());
                        olrinvoicereturnitems.amount = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"].ToString());
                        olrinvoicereturnitems.taxpercentage = reader["taxpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["taxpercentage"].ToString());
                        olrinvoicereturnitems.discount = reader["discount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discount"].ToString());
                        olrinvoicereturnitems.discountpercentage = reader["discountpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discountpercentage"].ToString());
                        olrinvoicereturnitems.taxsplit = new TaxSplit();
                        olrinvoicereturnitems.taxsplit.totalamount = olrinvoicereturnitems.taxableamount;

                        lst.Add(olrinvoicereturnitems);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, lst);
            }
            return lst;
        }

        public List<Lrinvoicereturnitems> SearchAll(long _lrinvoicereturnid,
            bool includetaxinformation,
            bool isinterstate)
        {
            List<Lrinvoicereturnitems> lst = new List<Lrinvoicereturnitems>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = GetCommand(includetaxinformation ? SQL_SearchAllWithTax : SQL_SearchAll, conn);
                command.Parameters.Add("lrinvoicereturnid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _lrinvoicereturnid;
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Lrinvoicereturnitems olrinvoicereturnitems = new Lrinvoicereturnitems();
                        olrinvoicereturnitems.productid = reader["productid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productid"].ToString());
                        olrinvoicereturnitems.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        olrinvoicereturnitems.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        olrinvoicereturnitems.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        olrinvoicereturnitems.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        olrinvoicereturnitems.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"].ToString());
                        olrinvoicereturnitems.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        olrinvoicereturnitems.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        olrinvoicereturnitems.itemid = reader["itemid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["itemid"].ToString());
                        olrinvoicereturnitems.barcode = reader["barcode"] == DBNull.Value ? "" : reader["barcode"].ToString();
                        olrinvoicereturnitems.uan = reader["uan"] == DBNull.Value ? "" : reader["uan"].ToString();
                        olrinvoicereturnitems.designid = reader["designid"] == DBNull.Value ? "" : reader["designid"].ToString();
                        olrinvoicereturnitems.salablegoodsid = reader["salablegoodsid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["salablegoodsid"].ToString());
                        olrinvoicereturnitems.stockid = reader["stockid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["stockid"].ToString());
                        olrinvoicereturnitems.lrinvoiceitemsid = reader["lrinvoiceitemsid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["lrinvoiceitemsid"].ToString());
                        olrinvoicereturnitems.taxid = reader["taxid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["taxid"].ToString());
                        olrinvoicereturnitems.imageid = reader["imageid"] == DBNull.Value ? "" : reader["imageid"].ToString();
                        olrinvoicereturnitems.lrinvoicereturnid = reader["lrinvoicereturnid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["lrinvoicereturnid"].ToString());
                        olrinvoicereturnitems.qty = reader["qty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["qty"].ToString());
                        olrinvoicereturnitems.rate = reader["rate"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["rate"].ToString());
                        olrinvoicereturnitems.tax = reader["tax"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["tax"].ToString());
                        olrinvoicereturnitems.amount = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"].ToString());
                        olrinvoicereturnitems.taxpercentage = reader["taxpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["taxpercentage"].ToString());
                        olrinvoicereturnitems.discount = reader["discount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discount"].ToString());
                        olrinvoicereturnitems.discountpercentage = reader["discountpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discountpercentage"].ToString());
                        olrinvoicereturnitems.taxsplit = new TaxSplit();
                        olrinvoicereturnitems.taxsplit.totalamount = olrinvoicereturnitems.taxableamount;
                        olrinvoicereturnitems.isinterstatesale = isinterstate;
                        if (includetaxinformation)
                        {
                            olrinvoicereturnitems.serialno = reader["serialno"] == DBNull.Value ? "" : reader["serialno"].ToString();
                            olrinvoicereturnitems.invoiceserialno = reader["invoiceserialno"] == DBNull.Value ? "" : reader["invoiceserialno"].ToString();
                            olrinvoicereturnitems.printingname = reader["printingname"] == DBNull.Value ? "" : reader["printingname"].ToString();
                            olrinvoicereturnitems.invoiceno = reader["invoiceno"] == DBNull.Value ? "" : reader["invoiceno"].ToString();
                            olrinvoicereturnitems.invoicedate = reader["invoicedate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["invoicedate"]);
                            olrinvoicereturnitems.taxsplit.name = reader["taxname"] == DBNull.Value ? "" : reader["taxname"].ToString();
                            olrinvoicereturnitems.hsncode = reader["hsncode"] == DBNull.Value ? "" : reader["hsncode"].ToString();
                            olrinvoicereturnitems.lrentryno = reader["lrentryno"] == DBNull.Value ? "" : reader["lrentryno"].ToString();
                            olrinvoicereturnitems.brandname = reader["brandname"] == DBNull.Value ? "" : reader["brandname"].ToString();
                            olrinvoicereturnitems.sizename = reader["sizename"] == DBNull.Value ? "" : reader["sizename"].ToString();
                            olrinvoicereturnitems.stylename = reader["stylename"] == DBNull.Value ? "" : reader["stylename"].ToString();

                            if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_fixed_barcodes
                                && string.IsNullOrEmpty(olrinvoicereturnitems.serialno))
                                olrinvoicereturnitems.serialno = olrinvoicereturnitems.barcode;

                            if (string.IsNullOrEmpty(olrinvoicereturnitems.hsncode))
                                olrinvoicereturnitems.hsncode = reader["stockhsncode"] == DBNull.Value ? "" : reader["stockhsncode"].ToString();

                            if (isinterstate)
                            {
                                olrinvoicereturnitems.taxsplit.secondtax = "IGST";
                            }
                            else
                            {
                                olrinvoicereturnitems.taxsplit.secondtax = "SGST";
                            }

                            if (reader["taxsplit"] != DBNull.Value)
                            {
                                var taxsplit = Newtonsoft.Json.JsonConvert.DeserializeObject<Tax.TaxSplit>(reader["taxsplit"].ToString());

                                if (taxsplit != null)
                                {
                                    var value = ((olrinvoicereturnitems.rate * olrinvoicereturnitems.qty) - olrinvoicereturnitems.discount);
                                    if (olrinvoicereturnitems.qty > 0 && value > 0)
                                        value = value / olrinvoicereturnitems.qty;
                                    var taxconfig = taxsplit.Find(value);
                                    if (taxconfig.taxpercent != olrinvoicereturnitems.taxpercentage)
                                    {
                                        if (taxsplit.items.Exists(e => e.taxpercent == olrinvoicereturnitems.taxpercentage))
                                            taxconfig = taxsplit.items.First(e => e.taxpercent == olrinvoicereturnitems.taxpercentage);
                                        else
                                        {
                                            taxconfig = new Tax.TaxSplit.Item()
                                            {
                                                cgst = olrinvoicereturnitems.taxpercentage / 2,
                                                sgst = olrinvoicereturnitems.taxpercentage / 2
                                            };
                                        }
                                    }
                                    if (isinterstate)
                                    {
                                        olrinvoicereturnitems.taxsplit.sgsttaxpercentage = 0;
                                        olrinvoicereturnitems.taxsplit.sgsttaxamount = 0;
                                        olrinvoicereturnitems.taxsplit.cgsttaxpercentage = taxconfig.sgst + taxconfig.cgst;
                                        olrinvoicereturnitems.taxsplit.cgsttaxamount = olrinvoicereturnitems.tax;
                                    }
                                    else
                                    {
                                        olrinvoicereturnitems.taxsplit.sgsttaxpercentage = taxconfig.sgst;
                                        olrinvoicereturnitems.taxsplit.cgsttaxpercentage = taxconfig.cgst;
                                        if (olrinvoicereturnitems.tax > 0
                                            && olrinvoicereturnitems.taxsplit.cgsttaxpercentage + olrinvoicereturnitems.taxsplit.sgsttaxpercentage > 0)
                                        {
                                            olrinvoicereturnitems.taxsplit.sgsttaxamount = Math.Round(olrinvoicereturnitems.tax /
                                                (olrinvoicereturnitems.taxsplit.cgsttaxpercentage + olrinvoicereturnitems.taxsplit.sgsttaxpercentage)
                                                * olrinvoicereturnitems.taxsplit.sgsttaxpercentage, 4);
                                        }
                                        olrinvoicereturnitems.taxsplit.cgsttaxamount = olrinvoicereturnitems.taxsplit.sgsttaxamount;
                                    }
                                }
                                else
                                {
                                    if (isinterstate)
                                    {
                                        olrinvoicereturnitems.taxsplit.sgsttaxpercentage = 0;
                                        olrinvoicereturnitems.taxsplit.sgsttaxamount = 0;
                                        olrinvoicereturnitems.taxsplit.cgsttaxpercentage = olrinvoicereturnitems.taxpercentage;
                                        olrinvoicereturnitems.taxsplit.cgsttaxamount = olrinvoicereturnitems.tax;
                                    }
                                    else
                                    {
                                        if (olrinvoicereturnitems.tax > 0 && olrinvoicereturnitems.taxpercentage > 0)
                                        {
                                            olrinvoicereturnitems.taxsplit.sgsttaxpercentage = olrinvoicereturnitems.taxpercentage / 2;
                                            olrinvoicereturnitems.taxsplit.cgsttaxpercentage = olrinvoicereturnitems.taxpercentage / 2;
                                            olrinvoicereturnitems.taxsplit.sgsttaxamount = Math.Round(olrinvoicereturnitems.tax / (olrinvoicereturnitems.taxsplit.cgsttaxpercentage + olrinvoicereturnitems.taxsplit.sgsttaxpercentage) * olrinvoicereturnitems.taxsplit.sgsttaxpercentage, 4);
                                            olrinvoicereturnitems.taxsplit.cgsttaxamount = olrinvoicereturnitems.taxsplit.sgsttaxamount;
                                        }
                                    }
                                }
                            }
                            else if (olrinvoicereturnitems.tax != 0 &&
                                olrinvoicereturnitems.taxpercentage != 0)
                            {
                                if (isinterstate)
                                {
                                    olrinvoicereturnitems.taxsplit.sgsttaxpercentage = 0;
                                    olrinvoicereturnitems.taxsplit.sgsttaxamount = 0;
                                    olrinvoicereturnitems.taxsplit.cgsttaxpercentage = olrinvoicereturnitems.taxpercentage;
                                    olrinvoicereturnitems.taxsplit.cgsttaxamount = olrinvoicereturnitems.tax;
                                }
                                else
                                {
                                    olrinvoicereturnitems.taxsplit.sgsttaxpercentage = olrinvoicereturnitems.taxpercentage / 2;
                                    olrinvoicereturnitems.taxsplit.cgsttaxpercentage = olrinvoicereturnitems.taxpercentage / 2;
                                    olrinvoicereturnitems.taxsplit.sgsttaxamount = Math.Round(olrinvoicereturnitems.tax / (olrinvoicereturnitems.taxsplit.cgsttaxpercentage + olrinvoicereturnitems.taxsplit.sgsttaxpercentage) * olrinvoicereturnitems.taxsplit.sgsttaxpercentage, 4);
                                    olrinvoicereturnitems.taxsplit.cgsttaxamount = olrinvoicereturnitems.taxsplit.sgsttaxamount;
                                }
                            }
                            if (olrinvoicereturnitems.taxsplit != null)
                                olrinvoicereturnitems.taxsplit.isinterstatesale = isinterstate;
                        }
                        olrinvoicereturnitems.sno = lst.Count + 1;
                        lst.Add(olrinvoicereturnitems);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, lst);
            }
            return lst;
        }

        public Lrinvoicereturnitems Insert(Lrinvoicereturn lrinvoicereturn, Lrinvoicereturnitems oLrinvoicereturnitems)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);

                    Insert(transaction, conn, lrinvoicereturn, oLrinvoicereturnitems);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oLrinvoicereturnitems);
            }
            return oLrinvoicereturnitems;
        }

        public bool Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Lrinvoicereturn lrinvoicereturn, Lrinvoicereturnitems oLrinvoicereturnitems)
        {
            NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("productid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrinvoicereturnitems.productid;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oLrinvoicereturnitems.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oLrinvoicereturnitems.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("itemid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrinvoicereturnitems.itemid;
            command.Parameters.Add("qty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrinvoicereturnitems.qty;
            command.Parameters.Add("uan", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oLrinvoicereturnitems.uan == null ? "" : oLrinvoicereturnitems.uan;
            command.Parameters.Add("barcode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oLrinvoicereturnitems.barcode == null ? "" : oLrinvoicereturnitems.barcode;
            command.Parameters.Add("designid", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oLrinvoicereturnitems.designid == null ? "" : oLrinvoicereturnitems.designid;
            command.Parameters.Add("imageid", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oLrinvoicereturnitems.imageid == null ? "" : oLrinvoicereturnitems.imageid;
            command.Parameters.Add("stockid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrinvoicereturnitems.stockid;
            command.Parameters.Add("lrinvoiceitemsid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrinvoicereturnitems.lrinvoiceitemsid;
            command.Parameters.Add("salablegoodsid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrinvoicereturnitems.salablegoodsid;
            command.Parameters.Add("taxid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrinvoicereturnitems.taxid;
            command.Parameters.Add("rate", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrinvoicereturnitems.rate;
            command.Parameters.Add("tax", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrinvoicereturnitems.tax;
            command.Parameters.Add("taxpercentage", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrinvoicereturnitems.taxpercentage;
            command.Parameters.Add("discount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrinvoicereturnitems.discount;
            command.Parameters.Add("amount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrinvoicereturnitems.amount;
            command.Parameters.Add("lrinvoicereturnid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrinvoicereturnitems.lrinvoicereturnid;
            command.Parameters.Add("discountpercentage", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrinvoicereturnitems.discountpercentage;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oLrinvoicereturnitems.id = Convert.ToInt64(reader[0].ToString());
                }
            }

            if (lrinvoicereturn.ReturnType == Lrinvoicereturn.ReturnTypes.DebitNote)
            {
                bool hasStockError = false;
                string errorString = "";
                if (oLrinvoicereturnitems.salablegoodsid > 0)
                {
                    command = GetCommand(SQL_UpdateReturnGoods, conn, transaction);
                    command.Parameters.Add("qty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrinvoicereturnitems.qty;
                    command.Parameters.Add("stockid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrinvoicereturnitems.stockid;
                    command.Parameters.Add("lrinvoiceitemsid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrinvoicereturnitems.lrinvoiceitemsid;
                    command.Parameters.Add("salablegoodsid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrinvoicereturnitems.salablegoodsid;
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            if (reader.GetDecimal(0) < 0)
                            {
                                hasStockError = true;
                                errorString = reader.GetString(1) + " dont have enogh stock to return - (" + reader.GetDecimal(0) + ")";
                                //throw new Exception(reader.GetString(1) + " dont have enogh stock to return - (" + reader.GetDecimal(0) + ")");
                            }
                        }
                    }
                    if (hasStockError)
                    {
                        if (!string.IsNullOrEmpty(oLrinvoicereturnitems.serialno))
                        {
                            command = GetCommand(SQL_UpdateReturnGoods, conn, transaction);
                            command.Parameters.Add("qty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrinvoicereturnitems.qty * -1;
                            command.Parameters.Add("stockid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrinvoicereturnitems.stockid;
                            command.Parameters.Add("lrinvoiceitemsid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrinvoicereturnitems.lrinvoiceitemsid;
                            command.Parameters.Add("salablegoodsid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrinvoicereturnitems.salablegoodsid;
                            command.ExecuteNonQuery();
                            return false;
                        }
                        else
                        {
                            throw new Exception(errorString);
                        }
                    }
                }

                command = GetCommand(SQL_UpdateReturnStock, conn, transaction);
                command.Parameters.Add("qty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrinvoicereturnitems.qty;
                command.Parameters.Add("stockid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrinvoicereturnitems.stockid;
                command.Parameters.Add("lrinvoiceitemsid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrinvoicereturnitems.lrinvoiceitemsid;
                command.Parameters.Add("salablegoodsid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrinvoicereturnitems.salablegoodsid;
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        if (reader.GetDecimal(0) < 0)
                        {
                            hasStockError = true;
                            errorString = reader.GetString(1) + "Either warehouse stock is in-correct or dont have enogh stock to return in warehouse - (" + reader.GetDecimal(0) + ")";
                            //throw new Exception(reader.GetString(1) + " dont have enogh stock to return - (" + reader.GetDecimal(0) + ")");
                        }
                    }
                }
                if (hasStockError)
                {
                    if (!string.IsNullOrEmpty(oLrinvoicereturnitems.serialno))
                    {
                        command = GetCommand(SQL_UpdateReturnStock, conn, transaction);
                        command.Parameters.Add("qty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrinvoicereturnitems.qty * -1;
                        command.Parameters.Add("stockid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrinvoicereturnitems.stockid;
                        command.Parameters.Add("lrinvoiceitemsid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrinvoicereturnitems.lrinvoiceitemsid;
                        command.Parameters.Add("salablegoodsid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrinvoicereturnitems.salablegoodsid;
                        command.ExecuteNonQuery();

                        if (oLrinvoicereturnitems.salablegoodsid > 0)
                        {
                            command = GetCommand(SQL_UpdateReturnGoods, conn, transaction);
                            command.Parameters.Add("qty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrinvoicereturnitems.qty * -1;
                            command.Parameters.Add("stockid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrinvoicereturnitems.stockid;
                            command.Parameters.Add("lrinvoiceitemsid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrinvoicereturnitems.lrinvoiceitemsid;
                            command.Parameters.Add("salablegoodsid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrinvoicereturnitems.salablegoodsid;
                            command.ExecuteNonQuery();
                        }
                        return false;
                    }
                    else
                    {
                        throw new Exception(errorString);
                    }
                }
            }

            if (lrinvoicereturn.ReturnType == Lrinvoicereturn.ReturnTypes.ExcessCreditNote)
            {
                command = GetCommand(SQL_UpdateExcessStock, conn, transaction);
                command.Parameters.Add("qty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrinvoicereturnitems.qty * -1;
                command.Parameters.Add("lrinvoiceitemsid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrinvoicereturnitems.lrinvoiceitemsid;
                command.ExecuteNonQuery();
            }
            return true;
        }


        public Lrinvoicereturnitems Update(Lrinvoicereturn lrinvoicereturn, Lrinvoicereturnitems oLrinvoicereturnitems)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, lrinvoicereturn, oLrinvoicereturnitems);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oLrinvoicereturnitems);
            }
            return oLrinvoicereturnitems;
        }

        public Lrinvoicereturnitems Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Lrinvoicereturn lrinvoicereturn, Lrinvoicereturnitems oLrinvoicereturnitems)
        {

            NpgsqlCommand command = GetCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("productid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrinvoicereturnitems.productid;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrinvoicereturnitems.id;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oLrinvoicereturnitems.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrinvoicereturnitems.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oLrinvoicereturnitems.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oLrinvoicereturnitems.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("itemid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrinvoicereturnitems.itemid;
            command.Parameters.Add("qty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrinvoicereturnitems.qty;
            command.Parameters.Add("barcode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oLrinvoicereturnitems.barcode == null ? "" : oLrinvoicereturnitems.barcode;
            command.Parameters.Add("designid", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oLrinvoicereturnitems.designid == null ? "" : oLrinvoicereturnitems.designid;
            command.Parameters.Add("imageid", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oLrinvoicereturnitems.imageid == null ? "" : oLrinvoicereturnitems.imageid;
            command.Parameters.Add("stockid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrinvoicereturnitems.stockid;
            command.Parameters.Add("lrinvoiceitemsid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrinvoicereturnitems.lrinvoiceitemsid;
            command.Parameters.Add("taxid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrinvoicereturnitems.taxid;
            command.Parameters.Add("rate", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrinvoicereturnitems.rate;
            command.Parameters.Add("tax", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrinvoicereturnitems.tax;
            command.Parameters.Add("discount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrinvoicereturnitems.discount;
            command.Parameters.Add("lrinvoicereturnid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrinvoicereturnitems.lrinvoicereturnid;
            command.Parameters.Add("taxpercentage", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrinvoicereturnitems.taxpercentage;
            command.Parameters.Add("amount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrinvoicereturnitems.amount;
            command.Parameters.Add("salablegoodsid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrinvoicereturnitems.salablegoodsid;
            command.Parameters.Add("discountpercentage", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrinvoicereturnitems.discountpercentage;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oLrinvoicereturnitems.version = Convert.ToInt32(reader[0]);
                }
            }

            if (lrinvoicereturn.ReturnType == Lrinvoicereturn.ReturnTypes.DebitNote)
            {
                command = GetCommand(oLrinvoicereturnitems.salablegoodsid > 0 ? (SQL_UpdateReturnGoods + "\r\n" + SQL_UpdateReturnStock) :
                    SQL_UpdateReturnStock, conn, transaction);
                command.Parameters.Add("qty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrinvoicereturnitems.qty;
                command.Parameters.Add("stockid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrinvoicereturnitems.stockid;
                command.Parameters.Add("lrinvoiceitemsid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrinvoicereturnitems.lrinvoiceitemsid;
                command.Parameters.Add("salablegoodsid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrinvoicereturnitems.salablegoodsid;
                command.ExecuteNonQuery();
            }

            if (lrinvoicereturn.ReturnType == Lrinvoicereturn.ReturnTypes.ExcessCreditNote)
            {
                command = GetCommand(SQL_UpdateExcessStock, conn, transaction);
                command.Parameters.Add("qty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrinvoicereturnitems.qty * -1;
                command.Parameters.Add("lrinvoiceitemsid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrinvoicereturnitems.lrinvoiceitemsid;
                command.ExecuteNonQuery();
            }
            return oLrinvoicereturnitems;
        }
        public bool UpdateForItemReturn(NpgsqlTransaction transaction, NpgsqlConnection conn, Lrinvoicereturn lrinvoicereturn, List<Lrinvoicereturnitems> list, long id)
        {
            foreach (var item in list)
            {
                if (item.id > 0)
                {
                    if (item.isactive == false)
                        Delete(transaction, conn, lrinvoicereturn, item);
                    else
                        Update(transaction, conn, lrinvoicereturn, item);
                }
                else
                {
                    item.lrinvoicereturnid = id;
                    Insert(transaction, conn, lrinvoicereturn, item);
                }
            }
            return true;

        }
        public bool Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Lrinvoicereturn lrinvoicereturn, List<Lrinvoicereturnitems> list, long id)
        {
            var existingItems = SearchAll(id, false, false);
            if (existingItems != null && existingItems.Count > 0)
            {
                var deletedItems = existingItems.FindAll(e => !list.Exists(f => f.id == e.id));
                foreach (var item in deletedItems)
                {
                    Delete(transaction, conn, lrinvoicereturn, item);
                    item.qty = 0;
                }

                if (lrinvoicereturn.ReturnType == Lrinvoicereturn.ReturnTypes.DebitNote)
                {
                    foreach (var item in existingItems.Where(e => e.qty > 0 && (e.stockid > 0 || e.lrinvoiceitemsid > 0)))
                    {
                        NpgsqlCommand commandRollback = GetCommand(item.salablegoodsid > 0 ? (SQL_UpdateReturnGoodsRollback + "\r\n" + SQL_UpdateReturnStockRollback) : SQL_UpdateReturnStockRollback, conn, transaction);
                        commandRollback.Parameters.Add("qty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = item.qty;
                        commandRollback.Parameters.Add("stockid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.stockid;
                        commandRollback.Parameters.Add("lrinvoiceitemsid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.lrinvoiceitemsid;
                        commandRollback.Parameters.Add("salablegoodsid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.salablegoodsid;
                        commandRollback.ExecuteNonQuery();
                    }
                }

                if (lrinvoicereturn.ReturnType == Lrinvoicereturn.ReturnTypes.ExcessCreditNote)
                {
                    foreach (var item in existingItems.Where(e => e.qty > 0 && (e.stockid > 0 || e.lrinvoiceitemsid > 0)))
                    {
                        NpgsqlCommand commandRollback = GetCommand(item.salablegoodsid > 0 ? (SQL_UpdateReturnGoodsRollback + "\r\n" + SQL_UpdateReturnStockRollback) : SQL_UpdateReturnStockRollback, conn, transaction);
                        commandRollback.Parameters.Add("qty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = item.qty * -1;
                        commandRollback.Parameters.Add("stockid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.stockid;
                        commandRollback.Parameters.Add("lrinvoiceitemsid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.lrinvoiceitemsid;
                        commandRollback.Parameters.Add("salablegoodsid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.salablegoodsid;
                        commandRollback.ExecuteNonQuery();
                    }
                }
            }

            foreach (var item in list)
            {
                if (item.id > 0)
                {
                    Update(transaction, conn, lrinvoicereturn, item);
                }
                else
                {
                    item.lrinvoicereturnid = id;
                    Insert(transaction, conn, lrinvoicereturn, item);
                }
            }
            return true;
        }

        public bool Delete(Lrinvoicereturnitems oLrinvoicereturnitems, Lrinvoicereturn lrinvoicereturn)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, lrinvoicereturn, oLrinvoicereturnitems);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oLrinvoicereturnitems);
            }
            return hasSuccessCount;

        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Lrinvoicereturn lrinvoicereturn, Lrinvoicereturnitems oLrinvoicereturnitems)
        {

            NpgsqlCommand command = GetCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oLrinvoicereturnitems.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oLrinvoicereturnitems.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrinvoicereturnitems.id;
            int successCount = command.ExecuteNonQuery();

            if (lrinvoicereturn.ReturnType == Lrinvoicereturn.ReturnTypes.DebitNote)
            {
                command = GetCommand(oLrinvoicereturnitems.salablegoodsid > 0 ? (SQL_UpdateReturnGoodsRollback + "\r\n" + SQL_UpdateReturnStockRollback) : SQL_UpdateReturnStockRollback, conn, transaction);
                command.Parameters.Add("qty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrinvoicereturnitems.qty;
                command.Parameters.Add("stockid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrinvoicereturnitems.stockid;
                command.Parameters.Add("lrinvoiceitemsid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrinvoicereturnitems.lrinvoiceitemsid;
                command.Parameters.Add("salablegoodsid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrinvoicereturnitems.salablegoodsid;
                command.ExecuteNonQuery();
            }

            if (lrinvoicereturn.ReturnType == Lrinvoicereturn.ReturnTypes.ExcessCreditNote)
            {
                NpgsqlCommand commandRollback = GetCommand(oLrinvoicereturnitems.salablegoodsid > 0 ? (SQL_UpdateReturnGoodsRollback + "\r\n" + SQL_UpdateReturnStockRollback) : SQL_UpdateReturnStockRollback, conn, transaction);
                commandRollback.Parameters.Add("qty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrinvoicereturnitems.qty * -1;
                commandRollback.Parameters.Add("stockid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrinvoicereturnitems.stockid;
                commandRollback.Parameters.Add("lrinvoiceitemsid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrinvoicereturnitems.lrinvoiceitemsid;
                commandRollback.Parameters.Add("salablegoodsid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrinvoicereturnitems.salablegoodsid;
                commandRollback.ExecuteNonQuery();
            }
            return successCount > 0;
        }

        public bool DeleteAll(Lrinvoicereturn lrinvoicereturn, long userid, NpgsqlConnection conn, NpgsqlTransaction transaction)
        {
            if (lrinvoicereturn.ReturnType == Lrinvoicereturn.ReturnTypes.DebitNote)
            {
                var list = SearchAll(lrinvoicereturn.id, false, false);
                foreach (var item in list)
                {
                    var commandRollback = GetCommand(item.salablegoodsid > 0 ? (SQL_UpdateReturnGoodsRollback + "\r\n" + SQL_UpdateReturnStockRollback) : SQL_UpdateReturnStockRollback, conn, transaction);
                    commandRollback.Parameters.Add("qty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = item.qty;
                    commandRollback.Parameters.Add("stockid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.stockid;
                    commandRollback.Parameters.Add("lrinvoiceitemsid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.lrinvoiceitemsid;
                    commandRollback.Parameters.Add("salablegoodsid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.salablegoodsid;
                    commandRollback.ExecuteNonQuery();
                }
            }
            int successCount = 0;
            NpgsqlCommand command = GetCommand(SQL_DeleteAll, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = false;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("lrinvoicereturnid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = lrinvoicereturn.id;
            successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

        public class ConflictUAN
        {
            public string barcode { get; set; }
            public decimal qty { get; set; }
            public long stockid { get; set; }
            public long salablegoodsid { get; set; }
        }

        internal List<Lrinvoicereturnitems> SyncGLNStock(Lrinvoicereturn lrinvoicereturn,
            List<Lrinvoicereturnitems> stockFailedItems,
            NpgsqlConnection conn, NpgsqlTransaction transaction)
        {
            List<Lrinvoicereturnitems> response = new List<Lrinvoicereturnitems>();
            foreach (var item in stockFailedItems.Where(x => x.salablegoodsid > 0))
            {
                string querystockuan = "select stock.id,salablegoods.id,salablegoods.barcode,iqty-(oqty+jqty+dqty) from stock " +
                                                          "inner join salablegoods on salablegoods.stockid = stock.id " +
                                                          "where stock.serialno = @serialno and iqty>(oqty+jqty+dqty) " +
                                                          "and salablegoods.locationid = @locationid " +
                                                          "order by iqty-(oqty+jqty+dqty) desc";
                List<ConflictUAN> listofstock = new List<ConflictUAN>();
                SortedList<string, object> param = new SortedList<string, object>();
                param.Add("@serialno", item.serialno);
                param.Add("@locationid", lrinvoicereturn.locationid);
                param.Add("@id", item.id);
                using (var reader = ExecuteReader(querystockuan, param, conn, transaction))
                {
                    while (reader.Read())
                    {
                        listofstock.Add(new ConflictUAN()
                        {
                            stockid = reader.GetInt64(0),
                            salablegoodsid = reader.GetInt64(1),
                            barcode = reader.GetString(2),
                            qty = reader.GetDecimal(3)
                        });

                        if (listofstock.Sum(e => e.qty) >= item.qty)
                        {
                            var additionalqty = listofstock.Sum(e => e.qty) - item.qty;
                            listofstock.OrderByDescending(x => x.qty).First().qty -= additionalqty;
                            break;
                        }
                    }
                }
                if (listofstock.Sum(e => e.qty) >= item.qty)
                {
                    ExecuteQuery("delete from Lrinvoicereturnitems where id=@id", param, conn, transaction);
                    item.id = 0;
                    foreach (var nitem in listofstock)
                    {
                        Lrinvoicereturnitems newreturnitem = Newtonsoft.Json.JsonConvert.DeserializeObject<Lrinvoicereturnitems>(Newtonsoft.Json.JsonConvert.SerializeObject(item));
                        newreturnitem.salablegoodsid = nitem.salablegoodsid;
                        newreturnitem.stockid = nitem.stockid;
                        newreturnitem.barcode = nitem.barcode;
                        newreturnitem.qty = nitem.qty;
                        if (!Insert(transaction, conn, lrinvoicereturn, newreturnitem))
                        {
                            throw new Exception(item.serialno + " don't have enough stock");
                        }
                        else
                        {
                            response.Add(newreturnitem);
                        }
                    }
                }
                else
                {
                    throw new Exception(item.serialno + " don't have enough stock");
                }
            }


            foreach (var item in stockFailedItems.Where(x => x.salablegoodsid <= 0))
            {
                string querystockuan = "select stock.id,stock.barcode,qty-(damagedqty+journalqty+returnqty+transfered) from stock " +
                                                          "where stock.serialno = @serialno and qty-(damagedqty+journalqty+returnqty+transfered)>0 " +
                                                          "order by qty-(damagedqty+journalqty+returnqty+transfered) desc";
                List<ConflictUAN> listofstock = new List<ConflictUAN>();
                SortedList<string, object> param = new SortedList<string, object>();
                param.Add("@serialno", item.serialno);
                param.Add("@locationid", lrinvoicereturn.locationid);
                param.Add("@id", item.id);
                using (var reader = ExecuteReader(querystockuan, param, conn, transaction))
                {
                    while (reader.Read())
                    {
                        listofstock.Add(new ConflictUAN()
                        {
                            stockid = reader.GetInt64(0),
                            barcode = reader.GetString(1),
                            qty = reader.GetDecimal(2)
                        });

                        if (listofstock.Sum(e => e.qty) >= item.qty)
                        {
                            var additionalqty = listofstock.Sum(e => e.qty) - item.qty;
                            listofstock.OrderByDescending(x => x.qty).First().qty -= additionalqty;
                            break;
                        }
                    }
                }
                if (listofstock.Sum(e => e.qty) >= item.qty)
                {
                    ExecuteQuery("delete from Lrinvoicereturnitems where id=@id", param, conn, transaction);
                    item.id = 0;
                    foreach (var nitem in listofstock)
                    {
                        Lrinvoicereturnitems newreturnitem = Newtonsoft.Json.JsonConvert.DeserializeObject<Lrinvoicereturnitems>(Newtonsoft.Json.JsonConvert.SerializeObject(item));
                        newreturnitem.stockid = nitem.stockid;
                        newreturnitem.barcode = nitem.barcode;
                        newreturnitem.qty = nitem.qty;
                        if (!Insert(transaction, conn, lrinvoicereturn, newreturnitem))
                        {
                            throw new Exception(item.serialno + " don't have enough stock");
                        }
                        else
                        {
                            response.Add(newreturnitem);
                        }
                    }
                }
                else
                {
                    throw new Exception(item.serialno + " don't have enough stock");
                }
            }
            return response;
        }
    }

}
