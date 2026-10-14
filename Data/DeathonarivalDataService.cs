using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
namespace InfyPOS.Data
{
    public class DeathonarrivalDataService : SAASData
    {
        const string SQL_Search = @"select registerno, modifiedon, isactive, invoiceitemid, itemid, productid, taxid, suppliercompanyid, supplierid, invoiceno, discountpercentage, taxpercentage, invoicedate, createdby, locationid, companyid, receivedemployeeid, approvedemployeeid, status, doadate, version, id, modifiedby, stockid, barcode, salablegoodsid, qty, rate, discount, tax, receivable, createdon, replacedoaid, areceivable, printingname, statusupdatedon, lrinvoicereturnid, lrinvoicereturnitemid, sellingprice from deathonarrival where id=@id";


        const string SQL_Insert = @"INSERT INTO public.deathonarrival(registerno, modifiedon, isactive, invoiceitemid, itemid, productid, taxid, suppliercompanyid, supplierid, invoiceno, discountpercentage, taxpercentage, invoicedate, createdby, locationid, companyid, receivedemployeeid, approvedemployeeid, status, doadate, version, modifiedby, stockid, barcode, salablegoodsid, qty, rate, discount, tax, receivable, createdon, replacedoaid, areceivable, printingname, statusupdatedon, lrinvoicereturnid, lrinvoicereturnitemid, sellingprice) VALUES(@registerno, @modifiedon, @isactive, @invoiceitemid, @itemid, @productid, @taxid, @suppliercompanyid, @supplierid, @invoiceno, @discountpercentage, @taxpercentage, @invoicedate, @createdby, @locationid, @companyid, @receivedemployeeid, @approvedemployeeid, @status, @doadate, @version, @modifiedby, @stockid, @barcode, @salablegoodsid, @qty, @rate, @discount, @tax, @receivable, @createdon, @replacedoaid, @areceivable, @printingname, @statusupdatedon, @lrinvoicereturnid, @lrinvoicereturnitemid, @sellingprice) RETURNING id";


        const string SQL_Update = @"UPDATE public.deathonarrival SET registerno=@registerno, modifiedon=@modifiedon, invoiceitemid=@invoiceitemid, 
    itemid=@itemid, productid=@productid, taxid=@taxid, suppliercompanyid=@suppliercompanyid, supplierid=@supplierid, invoiceno=@invoiceno, 
    discountpercentage=@discountpercentage, taxpercentage=@taxpercentage, invoicedate=@invoicedate, locationid=@locationid, companyid=@companyid, 
    receivedemployeeid=@receivedemployeeid, approvedemployeeid=@approvedemployeeid, status=@status, doadate=@doadate, version=@version+1, 
    modifiedby=@modifiedby, stockid=@stockid, barcode=@barcode, salablegoodsid=@salablegoodsid, qty=@qty, rate=@rate, discount=@discount, 
    tax=@tax, receivable=@receivable, replacedoaid=@replacedoaid, areceivable=@areceivable, printingname=@printingname, 
    statusupdatedon=@statusupdatedon, lrinvoicereturnid=@lrinvoicereturnid, lrinvoicereturnitemid=@lrinvoicereturnitemid, 
    sellingprice=@sellingprice,outcome=@outcome where id=@id and version=@version RETURNING version";

        const string SQL_Delete = @"update public.deathonarrival set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";
        const string SQL_UpdateReturnGoods = @"Update stock set transfered=transfered-@qty,purchasereturnqty=COALESCE(purchasereturnqty,0)-@qty where id=@stockid;
                                               Update salablegoods set iqty=iqty-@qty,rqty=COALESCE(rqty,0)+@qty where id=@salablegoodsid RETURNING iqty-(dqty+oqty),barcode; ";

        const string SQL_UpdateReturnStockLrinvoiceItems = @"Update lrinvoiceitems set returnqty=returnqty+@qty where id=@lrinvoiceitemsid RETURNING returnqty,buyingqty,actualqty;";

        const string SQL_UpdateReturnStock = @"Update Stock set returnqty=returnqty+@qty,purchasereturnqty=COALESCE(purchasereturnqty,0)+@qty where id=@stockid RETURNING qty-(damagedqty+returnqty+transfered),barcode; ";

        const string SQL_UpdateReturnGoodsRollback = @"Update salablegoods set iqty=iqty+@qty,rqty=COALESCE(rqty,0)-@qty where id=@salablegoodsid;
                                               Update stock set transfered=transfered+@qty,purchasereturnqty=COALESCE(purchasereturnqty,0)-@qty where id=@stockid;";

        const string SQL_UpdateReturnStockRollback = @"Update Stock set returnqty=returnqty-@qty,purchasereturnqty=COALESCE(purchasereturnqty,0)-@qty  where id=@stockid;
                                               Update lrinvoiceitems set returnqty=returnqty-@qty where id=@lrinvoiceitemsid; ";

        public List<Deathonarrival> Search(DeathonarrivalCreteria _deathonarrival)
        {
            List<Deathonarrival> lst = new List<Deathonarrival>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_deathonarrival.id != 0)
                {
                    command = new NpgsqlCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _deathonarrival.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("isactive", true, "=");

                    if (!string.IsNullOrEmpty(_deathonarrival.registerno))
                        qb.AddParameter("registerno", _deathonarrival.registerno, "=");

                    if (_deathonarrival.supplierid > 0)
                        qb.AddParameter("supplierid", _deathonarrival.supplierid, "=");

                    if (_deathonarrival.fromdate != DateTime.MinValue)
                        qb.AddParameter("doadate", "fromdate", _deathonarrival.fromdate, ">=");

                    if (_deathonarrival.todate != DateTime.MinValue)
                        qb.AddParameter("doadate", "todate", _deathonarrival.todate.AddDays(1), "<");

                    if (_deathonarrival.suppliercompanyid > 0)
                        qb.AddParameter("suppliercompanyid", _deathonarrival.suppliercompanyid, "=");

                    if (_deathonarrival.status >= 0)
                        qb.AddParameter("status", _deathonarrival.status, "=");

                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Deathonarrival odeathonarrival = new Deathonarrival();
                        odeathonarrival.registerno = reader["registerno"] == DBNull.Value ? "" : reader["registerno"].ToString();
                        odeathonarrival.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        odeathonarrival.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        odeathonarrival.invoiceitemid = reader["invoiceitemid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["invoiceitemid"]);
                        odeathonarrival.itemid = reader["itemid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["itemid"]);
                        odeathonarrival.productid = reader["productid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productid"]);
                        odeathonarrival.taxid = reader["taxid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["taxid"]);
                        odeathonarrival.suppliercompanyid = reader["suppliercompanyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["suppliercompanyid"]);
                        odeathonarrival.supplierid = reader["supplierid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["supplierid"]);
                        odeathonarrival.invoiceno = reader["invoiceno"] == DBNull.Value ? "" : reader["invoiceno"].ToString();
                        odeathonarrival.discountpercentage = reader["discountpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discountpercentage"]);
                        odeathonarrival.taxpercentage = reader["taxpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["taxpercentage"]);
                        odeathonarrival.invoicedate = reader["invoicedate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["invoicedate"]);
                        odeathonarrival.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        odeathonarrival.locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                        odeathonarrival.companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"]);
                        odeathonarrival.receivedemployeeid = reader["receivedemployeeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["receivedemployeeid"]);
                        odeathonarrival.approvedemployeeid = reader["approvedemployeeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["approvedemployeeid"]);
                        odeathonarrival.status = reader["status"] == DBNull.Value ? 0 : Convert.ToInt32(reader["status"]);
                        odeathonarrival.doadate = reader["doadate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["doadate"]);
                        odeathonarrival.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        odeathonarrival.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        odeathonarrival.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        odeathonarrival.stockid = reader["stockid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["stockid"]);
                        odeathonarrival.barcode = reader["barcode"] == DBNull.Value ? "" : reader["barcode"].ToString();
                        odeathonarrival.salablegoodsid = reader["salablegoodsid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["salablegoodsid"]);
                        odeathonarrival.qty = reader["qty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["qty"]);
                        odeathonarrival.rate = reader["rate"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["rate"]);
                        odeathonarrival.discount = reader["discount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discount"]);
                        odeathonarrival.tax = reader["tax"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["tax"]);
                        odeathonarrival.receivable = reader["receivable"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["receivable"]);
                        odeathonarrival.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        odeathonarrival.replacedoaid = reader["replacedoaid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["replacedoaid"]);
                        odeathonarrival.areceivable = reader["areceivable"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["areceivable"]);
                        odeathonarrival.printingname = reader["printingname"] == DBNull.Value ? "" : reader["printingname"].ToString();
                        odeathonarrival.statusupdatedon = reader["statusupdatedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["statusupdatedon"]);
                        odeathonarrival.lrinvoicereturnid = reader["lrinvoicereturnid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["lrinvoicereturnid"]);
                        odeathonarrival.lrinvoicereturnitemid = reader["lrinvoicereturnitemid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["lrinvoicereturnitemid"]);
                        odeathonarrival.sellingprice = reader["sellingprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["sellingprice"]);
                        lst.Add(odeathonarrival);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _deathonarrival);
            }
            return lst;
        }

        public Deathonarrival Insert(Deathonarrival oDeathonarrival)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);

                    Insert(transaction, conn, oDeathonarrival);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oDeathonarrival);
            }

            return oDeathonarrival;
        }

        public Deathonarrival Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Deathonarrival oDeathonarrival)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("registerno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oDeathonarrival.registerno == null ? "" : oDeathonarrival.registerno;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oDeathonarrival.modifiedon;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oDeathonarrival.isactive;
            command.Parameters.Add("invoiceitemid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oDeathonarrival.invoiceitemid;
            command.Parameters.Add("itemid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oDeathonarrival.itemid;
            command.Parameters.Add("productid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oDeathonarrival.productid;
            command.Parameters.Add("taxid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oDeathonarrival.taxid;
            command.Parameters.Add("suppliercompanyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oDeathonarrival.suppliercompanyid;
            command.Parameters.Add("supplierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oDeathonarrival.supplierid;
            command.Parameters.Add("invoiceno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oDeathonarrival.invoiceno == null ? "" : oDeathonarrival.invoiceno;
            command.Parameters.Add("discountpercentage", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oDeathonarrival.discountpercentage;
            command.Parameters.Add("taxpercentage", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oDeathonarrival.taxpercentage;
            command.Parameters.Add("invoicedate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oDeathonarrival.invoicedate;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oDeathonarrival.createdby;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oDeathonarrival.locationid;
            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oDeathonarrival.companyid;
            command.Parameters.Add("receivedemployeeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oDeathonarrival.receivedemployeeid;
            command.Parameters.Add("approvedemployeeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oDeathonarrival.approvedemployeeid;
            command.Parameters.Add("status", NpgsqlTypes.NpgsqlDbType.Integer).Value = oDeathonarrival.status;
            command.Parameters.Add("doadate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oDeathonarrival.doadate;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oDeathonarrival.version;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oDeathonarrival.modifiedby;
            command.Parameters.Add("stockid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oDeathonarrival.stockid;
            command.Parameters.Add("barcode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oDeathonarrival.barcode == null ? "" : oDeathonarrival.barcode;
            command.Parameters.Add("salablegoodsid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oDeathonarrival.salablegoodsid;
            command.Parameters.Add("qty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oDeathonarrival.qty;
            command.Parameters.Add("rate", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oDeathonarrival.rate;
            command.Parameters.Add("discount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oDeathonarrival.discount;
            command.Parameters.Add("tax", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oDeathonarrival.tax;
            command.Parameters.Add("receivable", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oDeathonarrival.receivable;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oDeathonarrival.createdon;
            command.Parameters.Add("replacedoaid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oDeathonarrival.replacedoaid;
            command.Parameters.Add("areceivable", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oDeathonarrival.areceivable;
            command.Parameters.Add("printingname", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oDeathonarrival.printingname == null ? "" : oDeathonarrival.printingname;
            command.Parameters.Add("statusupdatedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oDeathonarrival.statusupdatedon;
            command.Parameters.Add("lrinvoicereturnid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oDeathonarrival.lrinvoicereturnid;
            command.Parameters.Add("lrinvoicereturnitemid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oDeathonarrival.lrinvoicereturnitemid;
            command.Parameters.Add("sellingprice", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oDeathonarrival.sellingprice;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oDeathonarrival.id = Convert.ToInt64(reader[0].ToString());
                }
            }

            if (oDeathonarrival.salablegoodsid > 0)
            {
                command = GetCommand(SQL_UpdateReturnGoods, conn, transaction);
                command.Parameters.Add("qty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oDeathonarrival.qty;
                command.Parameters.Add("stockid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oDeathonarrival.stockid;
                command.Parameters.Add("lrinvoiceitemsid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oDeathonarrival.invoiceitemid;
                command.Parameters.Add("salablegoodsid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oDeathonarrival.salablegoodsid;
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        if (reader.GetDecimal(0) < 0)
                            throw new Exception(reader.GetString(1) + " dont have enogh stock to return - (" + reader.GetDecimal(0) + ")");
                    }
                }
            }
            command = GetCommand(SQL_UpdateReturnStockLrinvoiceItems, conn, transaction);
            command.Parameters.Add("qty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oDeathonarrival.qty;
            command.Parameters.Add("stockid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oDeathonarrival.stockid;
            command.Parameters.Add("lrinvoiceitemsid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oDeathonarrival.invoiceitemid;
            command.Parameters.Add("salablegoodsid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oDeathonarrival.salablegoodsid;
            using (var reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    if(!reader.IsDBNull(2))
                    {
                        var actualqty = reader.GetDecimal(2);
                        var returnqty = reader.GetDecimal(0);
                        var buyingqty = reader.GetDecimal(1);
                        if(buyingqty-actualqty<returnqty)
                            throw new Exception("returnqty higher than buyingqty-actualqty");
                    }
                    if (reader.GetDecimal(0) > reader.GetDecimal(1))
                    {
                        throw new Exception("Return qty can't higher than buying qty");
                    }    
                }
            }

            command = GetCommand(SQL_UpdateReturnStock, conn, transaction);
            command.Parameters.Add("qty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oDeathonarrival.qty;
            command.Parameters.Add("stockid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oDeathonarrival.stockid;
            command.Parameters.Add("lrinvoiceitemsid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oDeathonarrival.invoiceitemid;
            command.Parameters.Add("salablegoodsid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oDeathonarrival.salablegoodsid;
            using (var reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    if (reader.GetDecimal(0) < 0)
                        throw new Exception(reader.GetString(1) + "Either warehouse stock is in-correct or dont have enogh stock to return in warehouse - (" + reader.GetDecimal(0) + ")");
                }
            }
            return oDeathonarrival;
        }

        public void UpdateDebitNote(DOA result)
        {

        }

        internal DOA ReturnItems(DOA result)
        {
            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    foreach(var item in result.Replacements)
                    {
                        item.doastatus = Deathonarrival.DOAStatus.Return;
                        item.statusupdatedon = DateTimeNow;
                        Update(transaction, conn, item);
                    }
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                result.errormessage = exp.Message;
            }
            return result;
        }

        internal DOA Update(DOA result)
        {
            if (result.status == Deathonarrival.DOAStatus.Return)
                return ReturnItems(result);

            List<DeathonarrivalReplacement> items = result.Replacements;
            NpgsqlTransaction transaction = null;
            try
            {
                var sequence = SequenceManager.Instance(Context.OrganizationID).Get("DAO", 0, items.First().companyid);
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);
                    string seqno = sequence.GetNextNumber(Context,conn, transaction);
                    foreach (var doaitem in items)
                    {
                        if (result.status == Deathonarrival.DOAStatus.Replacement)
                        {
                            Data.LrinvoiceitemsDataService lrinvoiceitemsDataService = new LrinvoiceitemsDataService() { Context = this.Context };
                            var source = lrinvoiceitemsDataService.Search(new LrinvoiceitemsCreteria()
                            {
                                id = doaitem.invoiceitemid
                            });
                            if (source.Count == 1)
                            {
                                Data.StockDataService stockDataService = new StockDataService() { Context = this.Context };
                                if (string.IsNullOrEmpty(doaitem.replacement.barcode))
                                {
                                    Data.ProductsDataService productsDataService = new ProductsDataService() { Context = this.Context };
                                    var products = productsDataService.Search(new ProductsCreteria()
                                    { id = doaitem.replacement.productid });

                                    var barcodeConfig = InfyPOS.Processors.BarcodeManager.Instance(Context.OrganizationID).Get(products[0].barcodemode, doaitem.replacement.companyid);
                                    var listOfBarcodes = stockDataService.CreateBarcode(barcodeConfig.Type,1, doaitem.companyid, conn, transaction);
                                    doaitem.replacement.barcode = listOfBarcodes[0].code;
                                    doaitem.replacement.barcodeid = listOfBarcodes[0].id;
                                }

                                source[0].id = 0;
                                source[0].barcodeList = new List<string>();
                                source[0].barcodeList.Add(doaitem.replacement.barcode);
                                source[0].modifiedby = Context.UserID;
                                source[0].createdby = Context.UserID;
                                source[0].createdon = DateTimeNow;
                                source[0].modifiedon = DateTimeNow;
                                lrinvoiceitemsDataService.Insert(source[0]);
                                doaitem.replacement.invoiceitemid = source[0].id;

                                var stockitem = stockDataService.Search(new StockCreteria()
                                {
                                    barcode = doaitem.barcode
                                });

                                stockitem[0].id = 0;
                                stockitem[0].barcode = doaitem.replacement.barcode;
                                stockitem[0].qty = doaitem.replacement.qty;
                                stockitem[0].transfered = doaitem.replacement.qty;
                                stockitem[0].returnqty = 0;
                                stockitem[0].price = doaitem.replacement.receivable / doaitem.replacement.qty;
                                stockitem[0].modifiedby = Context.UserID;
                                stockitem[0].createdby = Context.UserID;
                                stockitem[0].createdon = DateTimeNow;
                                stockitem[0].modifiedon = DateTimeNow;
                                stockDataService.Insert(transaction, conn, stockitem[0]);
                                doaitem.replacement.stockid = stockitem[0].id;

                                Data.SalablegoodsDataService salablegoodsDataService = new SalablegoodsDataService() { Context = this.Context };
                                var salablegood = salablegoodsDataService.Search(new SalablegoodsCreteria(), conn, transaction);
                                salablegood[0].id = 0;
                                salablegood[0].barcode = doaitem.replacement.barcode;
                                salablegood[0].iqty = doaitem.replacement.qty;
                                salablegood[0].oqty = 0;
                                salablegood[0].dqty = 0;
                                salablegood[0].tiqty = 0;
                                salablegood[0].toqty = 0;
                                salablegood[0].rqty = 0;
                                salablegood[0].discount = 0;
                                salablegood[0].price = doaitem.replacement.receivable / doaitem.replacement.qty;
                                salablegood[0].modifiedby = Context.UserID;
                                salablegood[0].createdby = Context.UserID;
                                salablegood[0].createdon = DateTimeNow;
                                salablegood[0].modifiedon = DateTimeNow;
                                salablegoodsDataService.Insert(salablegood[0]);
                                doaitem.replacement.salablegoodsid = stockitem[0].id;
                            }

                            doaitem.replacement.modifiedby = Context.UserID;
                            doaitem.replacement.createdby = Context.UserID;
                            doaitem.replacement.createdon = DateTimeNow;
                            doaitem.replacement.modifiedon = DateTimeNow;
                            doaitem.replacement.receivedemployeeid = result.receivedemployeeid;
                            doaitem.replacement.doadate = result.doadate;
                            doaitem.replacement.registerno = seqno;
                            doaitem.replacement.doastatus = Deathonarrival.DOAStatus.ReplacedItem;
                            doaitem.replacement.replacedoaid = doaitem.id;
                            Insert(transaction, conn, doaitem.replacement);
                            doaitem.replacedoaid = doaitem.replacement.id;
                            doaitem.doastatus = Deathonarrival.DOAStatus.Replacement;
                            doaitem.statusupdatedon = result.doadate;
                            doaitem.outcome = doaitem.replacement.barcode;
                            Update(transaction, conn, doaitem);
                        }
                        if (result.status == Deathonarrival.DOAStatus.Return)
                        {
                            doaitem.doastatus = result.status;
                            doaitem.statusupdatedon = result.doadate;
                            Update(transaction, conn, doaitem);
                        }
                        if (result.status == Deathonarrival.DOAStatus.DebitNote || result.status == Deathonarrival.DOAStatus.CreditNote)
                        {
                            Data.LrinvoiceitemsDataService lrinvoiceitemsDataService = new LrinvoiceitemsDataService() { Context = this.Context };
                            var source = lrinvoiceitemsDataService.Search(new LrinvoiceitemsCreteria()
                            {
                                id = doaitem.invoiceitemid
                            });
                            if (source.Count == 1)
                            {
                                LrinvoicereturnDataService lrinvoicereturnDataService = new LrinvoicereturnDataService() { Context = this.Context };
                                lrinvoicereturnDataService.Context = this.Context;
                                var returnitem = lrinvoicereturnDataService.AddDirectDebitNote(
                                    result.status == Deathonarrival.DOAStatus.DebitNote ? Lrinvoicereturn.ReturnTypes.DOADebitNote :
                                    Lrinvoicereturn.ReturnTypes.DOACreditNote, source[0], doaitem.replacement);

                                doaitem.doastatus = result.status;
                                doaitem.lrinvoicereturnid = returnitem.id;
                                doaitem.lrinvoicereturnitemid = returnitem.Lrinvoicereturnitems.Count == 0 ? 0 : returnitem.Lrinvoicereturnitems.OrderBy(e => e.modifiedon).Last().id ;
                                doaitem.statusupdatedon = result.doadate;
                                doaitem.outcome = returnitem.returnno;
                                Update(transaction, conn, doaitem);
                            }
                        }
                        if (result.status == Deathonarrival.DOAStatus.Alternate)
                        {
                            Data.LrinvoiceitemsDataService lrinvoiceitemsDataService = new LrinvoiceitemsDataService() { Context = this.Context };
                            var source = lrinvoiceitemsDataService.Search(new LrinvoiceitemsCreteria()
                            {
                                id = doaitem.invoiceitemid
                            });
                            if (source.Count == 1)
                            {
                                Data.StockDataService stockDataService = new StockDataService() { Context = this.Context };
                                if (string.IsNullOrEmpty(doaitem.replacement.barcode))
                                {
                                    Data.ProductsDataService productsDataService = new ProductsDataService() { Context = this.Context };
                                    var products = productsDataService.Search(new ProductsCreteria()
                                    { id = doaitem.replacement.productid });

                                    var barcodeConfig = InfyPOS.Processors.BarcodeManager.Instance(Context.OrganizationID).Get(products[0].barcodemode, doaitem.replacement.companyid);
                                    var listOfBarcodes = stockDataService.CreateBarcode(barcodeConfig.Type, 1, doaitem.companyid, conn, transaction);
                                    doaitem.replacement.barcode = listOfBarcodes[0].code;
                                }

                                source[0].id = 0;
                                source[0].itemid = doaitem.replacement.itemid;
                                source[0].productid = doaitem.replacement.productid;
                                source[0].barcodeList = new List<string>();
                                source[0].barcodeList.Add(doaitem.replacement.barcode);
                                source[0].modifiedby = Context.UserID;
                                source[0].createdby = Context.UserID;
                                source[0].createdon = DateTimeNow;
                                source[0].modifiedon = DateTimeNow;
                                lrinvoiceitemsDataService.Insert(source[0]);
                                doaitem.replacement.invoiceitemid = source[0].id;

                                var stockitem = stockDataService.Search(new StockCreteria()
                                {
                                    barcode = doaitem.barcode
                                });

                                stockitem[0].id = 0;
                                stockitem[0].productid = doaitem.replacement.productid;
                                stockitem[0].itemid = doaitem.replacement.itemid;
                                stockitem[0].barcode = doaitem.replacement.barcode;
                                stockitem[0].qty = doaitem.replacement.qty;
                                stockitem[0].transfered = doaitem.replacement.qty;
                                stockitem[0].returnqty = 0;
                                stockitem[0].price = doaitem.replacement.receivable / doaitem.replacement.qty;
                                stockitem[0].modifiedby = Context.UserID;
                                stockitem[0].createdby = Context.UserID;
                                stockitem[0].createdon = DateTimeNow;
                                stockitem[0].modifiedon = DateTimeNow;
                                stockDataService.Insert(transaction, conn, stockitem[0]);
                                doaitem.replacement.stockid = stockitem[0].id;

                                Data.SalablegoodsDataService salablegoodsDataService = new SalablegoodsDataService() { Context = this.Context };
                                var salablegood = salablegoodsDataService.Search(new SalablegoodsCreteria(), conn, transaction);
                                salablegood[0].id = 0;
                                salablegood[0].barcode = doaitem.replacement.barcode;
                                salablegood[0].iqty = doaitem.replacement.qty;
                                salablegood[0].oqty = 0;
                                salablegood[0].dqty = 0;
                                salablegood[0].tiqty = 0;
                                salablegood[0].toqty = 0;
                                salablegood[0].rqty = 0;
                                salablegood[0].discount = 0;
                                salablegood[0].price = doaitem.replacement.receivable / doaitem.replacement.qty;
                                salablegood[0].modifiedby = Context.UserID;
                                salablegood[0].createdby = Context.UserID;
                                salablegood[0].createdon = DateTimeNow;
                                salablegood[0].modifiedon = DateTimeNow;
                                salablegoodsDataService.Insert(salablegood[0]);
                                doaitem.replacement.salablegoodsid = stockitem[0].id;
                            }

                            doaitem.replacement.modifiedby = Context.UserID;
                            doaitem.replacement.createdby = Context.UserID;
                            doaitem.replacement.createdon = DateTimeNow;
                            doaitem.replacement.modifiedon = DateTimeNow;
                            doaitem.replacement.receivedemployeeid = result.receivedemployeeid;
                            doaitem.replacement.doadate = result.doadate;
                            doaitem.replacement.registerno = seqno;
                            doaitem.replacement.doastatus = Deathonarrival.DOAStatus.ReplacedItem;
                            doaitem.replacement.replacedoaid = doaitem.id;
                            Insert(transaction, conn, doaitem.replacement);
                            doaitem.replacedoaid = doaitem.replacement.id;
                            doaitem.doastatus = Deathonarrival.DOAStatus.Alternate;
                            doaitem.statusupdatedon = result.doadate;
                            Update(transaction, conn, doaitem);
                        }
                    }
                    transaction.Commit();
                    conn.Close();
                }

                return result;
            }
            catch (Exception exp)
            {
                result.errormessage = exp.Message;
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, items);
            }
            return result;
        }

        internal DOA Insert(DOA item)
        {
            NpgsqlTransaction transaction = null;
            try
            {
                var sequence = SequenceManager.Instance(Context.OrganizationID).Get("DAO", 0, item.Items.First().companyid);
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);
                    string seqno = sequence.GetNextNumber(Context,conn, transaction);
                    foreach (var doaitem in item.Items)
                    {
                        doaitem.modifiedby = Context.UserID;
                        doaitem.createdby = Context.UserID;
                        doaitem.createdon = DateTimeNow;
                        doaitem.modifiedon = DateTimeNow;
                        doaitem.receivedemployeeid = item.receivedemployeeid;
                        doaitem.doadate = item.doadate;
                        doaitem.registerno = seqno;
                        Insert(transaction, conn, doaitem);
                    }
                    transaction.Commit();
                    conn.Close();
                }

                return item;
            }
            catch (Exception exp)
            {
                item.errormessage = exp.Message;
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, item);

            }
            return item;
        }

        public Deathonarrival Update(Deathonarrival oDeathonarrival)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oDeathonarrival);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oDeathonarrival);
            }

            return oDeathonarrival;
        }

        public Deathonarrival Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Deathonarrival oDeathonarrival)
        {
            bool rollbackstock = false;
            if(oDeathonarrival.doastatus == Deathonarrival.DOAStatus.Return)
            {
                using (var reader = ExecuteReader("Select status from Deathonarrival where id=" + oDeathonarrival.id, null))
                {
                    rollbackstock = reader.Read() && reader.GetInt32(0) != 4;
                }
            }

            NpgsqlCommand command = new NpgsqlCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("registerno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oDeathonarrival.registerno == null ? "" : oDeathonarrival.registerno;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oDeathonarrival.modifiedon;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oDeathonarrival.isactive;
            command.Parameters.Add("invoiceitemid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oDeathonarrival.invoiceitemid;
            command.Parameters.Add("itemid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oDeathonarrival.itemid;
            command.Parameters.Add("productid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oDeathonarrival.productid;
            command.Parameters.Add("taxid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oDeathonarrival.taxid;
            command.Parameters.Add("suppliercompanyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oDeathonarrival.suppliercompanyid;
            command.Parameters.Add("supplierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oDeathonarrival.supplierid;
            command.Parameters.Add("invoiceno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oDeathonarrival.invoiceno == null ? "" : oDeathonarrival.invoiceno;
            command.Parameters.Add("outcome", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oDeathonarrival.outcome == null ? "" : oDeathonarrival.outcome;
            command.Parameters.Add("discountpercentage", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oDeathonarrival.discountpercentage;
            command.Parameters.Add("taxpercentage", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oDeathonarrival.taxpercentage;
            command.Parameters.Add("invoicedate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oDeathonarrival.invoicedate;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oDeathonarrival.createdby;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oDeathonarrival.locationid;
            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oDeathonarrival.companyid;
            command.Parameters.Add("receivedemployeeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oDeathonarrival.receivedemployeeid;
            command.Parameters.Add("approvedemployeeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oDeathonarrival.approvedemployeeid;
            command.Parameters.Add("status", NpgsqlTypes.NpgsqlDbType.Integer).Value = oDeathonarrival.status;
            command.Parameters.Add("doadate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oDeathonarrival.doadate;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oDeathonarrival.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oDeathonarrival.id;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oDeathonarrival.modifiedby;
            command.Parameters.Add("stockid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oDeathonarrival.stockid;
            command.Parameters.Add("barcode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oDeathonarrival.barcode == null ? "" : oDeathonarrival.barcode;
            command.Parameters.Add("salablegoodsid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oDeathonarrival.salablegoodsid;
            command.Parameters.Add("qty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oDeathonarrival.qty;
            command.Parameters.Add("rate", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oDeathonarrival.rate;
            command.Parameters.Add("discount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oDeathonarrival.discount;
            command.Parameters.Add("tax", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oDeathonarrival.tax;
            command.Parameters.Add("receivable", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oDeathonarrival.receivable;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oDeathonarrival.createdon;
            command.Parameters.Add("replacedoaid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oDeathonarrival.replacedoaid;
            command.Parameters.Add("areceivable", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oDeathonarrival.areceivable;
            command.Parameters.Add("printingname", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oDeathonarrival.printingname == null ? "" : oDeathonarrival.printingname;
            command.Parameters.Add("statusupdatedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oDeathonarrival.statusupdatedon;
            command.Parameters.Add("lrinvoicereturnid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oDeathonarrival.lrinvoicereturnid;
            command.Parameters.Add("lrinvoicereturnitemid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oDeathonarrival.lrinvoicereturnitemid;
            command.Parameters.Add("sellingprice", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oDeathonarrival.sellingprice;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oDeathonarrival.version = Convert.ToInt32(reader[0].ToString());
                }
            }

            if (rollbackstock && oDeathonarrival.doastatus == Deathonarrival.DOAStatus.Return)
            {
                NpgsqlCommand commandRollback = GetCommand(oDeathonarrival.salablegoodsid > 0 ? (SQL_UpdateReturnGoodsRollback + "\r\n" + SQL_UpdateReturnStockRollback) : SQL_UpdateReturnStockRollback, conn, transaction);
                commandRollback.Parameters.Add("qty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oDeathonarrival.qty;
                commandRollback.Parameters.Add("stockid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oDeathonarrival.stockid;
                commandRollback.Parameters.Add("lrinvoiceitemsid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oDeathonarrival.lrinvoicereturnitemid;
                commandRollback.Parameters.Add("salablegoodsid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oDeathonarrival.salablegoodsid;
                commandRollback.ExecuteNonQuery();
            }
            return oDeathonarrival;
        }


        public bool Delete(Deathonarrival oDeathonarrival)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oDeathonarrival);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oDeathonarrival);

            }
            return hasSuccessCount;

        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Deathonarrival oDeathonarrival)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oDeathonarrival.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oDeathonarrival.modifiedby;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oDeathonarrival.modifiedon;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oDeathonarrival.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oDeathonarrival.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

    }

}
