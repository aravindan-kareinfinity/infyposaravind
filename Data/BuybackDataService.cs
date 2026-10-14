using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
namespace InfyPOS.Data

{
    public class BuybackDataService : SAASData
    {
        const string SQL_Search = @"select isactive, createdby, modifiedby, version, createdon, modifiedon, id, supplierid, productid, brandid, 
    itemid, schemeid, purchaseprice, schemediscount, receivedemployeeid, approvedemployeeid, status, statuschangedon, buybackdate, hasconsumed, 
    consumedbillid, consumedbillno, consumeduserid, buybackkey, buybackno, customerid, customername, customerphoneno, schemename, locationid, 
    companyid, customeraddress,imieno,salablegoodsid,stockid from buyback where id=@id";


        const string SQL_Insert = @"INSERT INTO public.buyback(isactive, createdby, modifiedby, version, createdon, modifiedon, supplierid, productid, brandid, itemid, schemeid, purchaseprice, schemediscount, receivedemployeeid, approvedemployeeid, status, statuschangedon, buybackdate, hasconsumed, consumedbillid, consumedbillno, consumeduserid, buybackkey, buybackno, customerid, customername, customerphoneno, schemename, locationid, companyid, customeraddress,imieno,salablegoodsid,stockid) 
            VALUES(@isactive, @createdby, @modifiedby, @version, @createdon, @modifiedon, @supplierid, @productid, @brandid, @itemid, @schemeid, @purchaseprice, @schemediscount, @receivedemployeeid, @approvedemployeeid, @status, @statuschangedon, @buybackdate, @hasconsumed, @consumedbillid, @consumedbillno, @consumeduserid, @buybackkey, @buybackno, @customerid, @customername, @customerphoneno, @schemename, @locationid, @companyid, @customeraddress,@imieno,@salablegoodsid,@stockid) RETURNING id";


        const string SQL_Update = @"UPDATE public.buyback SET modifiedby=@modifiedby, version=@version+1, modifiedon=@modifiedon, supplierid=@supplierid, 
        productid=@productid, brandid=@brandid, itemid=@itemid, schemeid=@schemeid, purchaseprice=@purchaseprice, schemediscount=@schemediscount, 
        receivedemployeeid=@receivedemployeeid, approvedemployeeid=@approvedemployeeid, status=@status, statuschangedon=@statuschangedon, 
        buybackdate=@buybackdate, hasconsumed=@hasconsumed, consumedbillid=@consumedbillid, consumedbillno=@consumedbillno, 
        consumeduserid=@consumeduserid, buybackkey=@buybackkey, buybackno=@buybackno, customerid=@customerid, customername=@customername, 
        customerphoneno=@customerphoneno, schemename=@schemename, locationid=@locationid, companyid=@companyid, 
        customeraddress=@customeraddress,imieno=@imieno,salablegoodsid=@salablegoodsid,stockid=@stockid where id=@id and version=@version RETURNING version";

        internal Barcodedgoods BarcodeSearch(string locationid, string barcode)
        {
            var buybackitems = Search(new BuybackCreteria()
            {
                imieno = barcode
            });
            if (buybackitems.Count == 1)
            {
                if (buybackitems[0].Status == Buyback.BuyBackStatus.Received)
                {
                    SalablegoodsDataService stockDataService = new SalablegoodsDataService() { Context = this.Context };
                    var barcodegoods = stockDataService.BarcodeSearch(locationid, barcode, false);
                    barcodegoods.isexchange = true;
                    barcodegoods.runtimeprice = false;
                    barcodegoods.discount = 0;
                    barcodegoods.price = buybackitems[0].purchaseprice;
                    return barcodegoods;
                }
                else
                {
                    return new Barcodedgoods()
                    {
                        errormessage = "Already this item consumed in " + buybackitems[0].consumedbillno
                    };
                }
            }
            else
            {
                return new Barcodedgoods()
                {
                    errormessage = "Can't find in the buyback/exchange list " + barcode
                };
            }
        }


        const string SQL_Delete = @"update public.buyback set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon 
        where id=@id and version=@version";

        public List<Buyback> Search(BuybackCreteria _buyback)
        {
            List<Buyback> lst = new List<Buyback>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_buyback.id != 0)
                {
                    command = new NpgsqlCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _buyback.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("isactive", true, "=");

                    if (_buyback.supplierid > 0)
                        qb.AddParameter("supplierid", _buyback.supplierid, "=");

                    if (_buyback.productid > 0)
                        qb.AddParameter("productid", _buyback.productid, "=");

                    if (_buyback.brandid > 0)
                        qb.AddParameter("brandid", _buyback.brandid, "=");

                    if (_buyback.schemeid > 0)
                        qb.AddParameter("schemeid", _buyback.schemeid, "=");

                    if (_buyback.customerid > 0)
                        qb.AddParameter("customerid", _buyback.customerid, "=");

                    if (_buyback.fromdate > DateTime.MinValue)
                        qb.AddParameter("buybackdate", "fromdate", _buyback.fromdate, ">=");

                    if (_buyback.todate > DateTime.MinValue)
                        qb.AddParameter("buybackdate", "todate", _buyback.todate.AddDays(1), "<");

                    if (_buyback.stockid > 0)
                        qb.AddParameter("stockid", _buyback.stockid, "=");

                    if(_buyback.status>0)
                        qb.AddParameter("status", _buyback.status, "=");

                    if (!string.IsNullOrEmpty(_buyback.imieno))
                        qb.AddParameter("imieno", _buyback.imieno, "=");

                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Buyback obuyback = new Buyback();
                        obuyback.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        obuyback.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        obuyback.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        obuyback.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        obuyback.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        obuyback.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        obuyback.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        obuyback.supplierid = reader["supplierid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["supplierid"]);
                        obuyback.productid = reader["productid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productid"]);
                        obuyback.brandid = reader["brandid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["brandid"]);
                        obuyback.itemid = reader["itemid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["itemid"]);
                        obuyback.schemeid = reader["schemeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["schemeid"]);
                        obuyback.purchaseprice = reader["purchaseprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["purchaseprice"]);
                        obuyback.schemediscount = reader["schemediscount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["schemediscount"]);
                        obuyback.receivedemployeeid = reader["receivedemployeeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["receivedemployeeid"]);
                        obuyback.approvedemployeeid = reader["approvedemployeeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["approvedemployeeid"]);
                        obuyback.status = reader["status"] == DBNull.Value ? 0 : Convert.ToInt32(reader["status"]);
                        obuyback.statuschangedon = reader["statuschangedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["statuschangedon"]);
                        obuyback.buybackdate = reader["buybackdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["buybackdate"]);
                        obuyback.hasconsumed = reader["hasconsumed"] == DBNull.Value ? false : Convert.ToBoolean(reader["hasconsumed"]);
                        obuyback.consumedbillid = reader["consumedbillid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["consumedbillid"]);

                        obuyback.stockid = reader["stockid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["stockid"]);
                        obuyback.salablegoodsid = reader["salablegoodsid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["salablegoodsid"]);

                        obuyback.consumedbillno = reader["consumedbillno"] == DBNull.Value ? "" : reader["consumedbillno"].ToString();
                        obuyback.consumeduserid = reader["consumeduserid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["consumeduserid"]);
                        obuyback.buybackkey = reader["buybackkey"] == DBNull.Value ? "" : reader["buybackkey"].ToString();
                        obuyback.imieno = reader["imieno"] == DBNull.Value ? "" : reader["imieno"].ToString();
                        obuyback.buybackno = reader["buybackno"] == DBNull.Value ? "" : reader["buybackno"].ToString();
                        obuyback.customerid = reader["customerid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["customerid"]);
                        obuyback.customername = reader["customername"] == DBNull.Value ? "" : reader["customername"].ToString();
                        obuyback.customerphoneno = reader["customerphoneno"] == DBNull.Value ? "" : reader["customerphoneno"].ToString();
                        obuyback.schemename = reader["schemename"] == DBNull.Value ? "" : reader["schemename"].ToString();
                        obuyback.locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                        obuyback.companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"]);
                        obuyback.customeraddress = reader["customeraddress"] == DBNull.Value ? "" : reader["customeraddress"].ToString();
                        lst.Add(obuyback);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _buyback);
            }
            return lst;
        }

        public Buyback Insert(Buyback oBuyback)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                var sequence = SequenceManager.Instance(Context.OrganizationID).Get("buyback.buybackno", 0);
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    ProductsDataService productsDataService = new ProductsDataService() { Context = this.Context };
                    var product = productsDataService.Search(new ProductsCreteria()
                    {
                        id = oBuyback.productid
                    }).First();

                    ItemsDataService itemsDataService = new ItemsDataService() { Context = this.Context };
                    var item = itemsDataService.Search(new ItemsCreteria()
                    {
                        id = oBuyback.itemid
                    }).First();
                    StockDataService stockDataService = new StockDataService() { Context = this.Context };
                    var stocks = stockDataService.Search(new StockCreteria()
                    {
                        barcode = oBuyback.imieno
                    });

                    if (stocks.Count == 0)
                    {
                        Stock tstock = new Stock();
                        tstock.invoiceitemid = -1;
                        tstock.hsncode = product.hsncode;
                        tstock.isactive = true;
                        tstock.barcode = oBuyback.imieno;
                        tstock.itemid = item.id;
                        tstock.runtimeprice = true;
                        tstock.companyid = oBuyback.companyid;
                        tstock.locationid = oBuyback.locationid;
                        tstock.retaillocationid = oBuyback.locationid;
                        tstock.lrreference = "buyback";
                        tstock.price = oBuyback.purchaseprice;
                        tstock.buyingprice = oBuyback.purchaseprice;
                        tstock.productid = item.productid;
                        tstock.printingname = item.sellingname;
                        tstock.stockdate = DateTimeNow.Date;
                        tstock.buyingqty = 1;
                        tstock.qty = 1;
                        stockDataService.Insert(transaction, conn, tstock);
                        oBuyback.stockid = tstock.id;
                    }
                    else
                    {
                        throw new Exception("Given Barcode available in the system");
                        stocks[0].buyingqty++;
                        stocks[0].transfered++;
                        stockDataService.Update(transaction, conn, stocks[0]);
                    }

                    SalablegoodsDataService saldata = new SalablegoodsDataService() { Context = this.Context };
                    saldata.Context = this.Context;
                    var salableitems = saldata.Search(new SalablegoodsCreteria()
                    {
                        locationid = oBuyback.locationid,
                        barcode = oBuyback.imieno
                    });

                    if (salableitems.Count > 0)
                    {
                        salableitems[0].iqty += 1;
                        salableitems[0].price = item.purchaserate;
                        saldata.Update(transaction, conn, salableitems[0], salableitems[0]);
                        oBuyback.salablegoodsid = salableitems[0].id;
                    }
                    else
                    {
                        Salablegoods salitem = new Salablegoods();
                        salitem.iqty = 1;
                        salitem.barcode = oBuyback.imieno;
                        salitem.inwardon = DateTimeNow.Date;
                        salitem.isactive = true;
                        salitem.locationid = oBuyback.locationid;
                        salitem.lrreference = "...";
                        salitem.price = oBuyback.purchaseprice;
                        salitem.received = true;
                        salitem.itemid = item.id;
                        salitem.companyid = oBuyback.companyid;
                        salitem.isactive = true;
                        salitem.createdby = Context.UserID;
                        salitem.modifiedby = Context.UserID;
                        salitem.createdon = DateTimeNow;
                        salitem.modifiedon = DateTimeNow;
                        salitem.version = 1;
                        salitem.sellingname = item.sellingname;
                        salitem.companyid = oBuyback.companyid;
                        salitem.stockid = oBuyback.stockid;
                        salitem.costprice = oBuyback.purchaseprice;
                        saldata.Insert(transaction, conn, salitem);
                        oBuyback.salablegoodsid = salitem.id;
                    }

                    NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);
                    oBuyback.buybackno = sequence.GetNextNumber(Context,conn, transaction);
                    oBuyback.isactive = true;
                    oBuyback.version = 1;
                    oBuyback.modifiedby = Context.UserID;
                    oBuyback.createdby = Context.UserID;
                    oBuyback.createdon = DateTimeNow;
                    oBuyback.modifiedon = DateTimeNow;

                    Insert(transaction, conn, oBuyback);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oBuyback);
                throw exp;
            }

            return oBuyback;
        }

        public Buyback Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Buyback oBuyback)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBuyback.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBuyback.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBuyback.modifiedby;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBuyback.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBuyback.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBuyback.modifiedon;
            command.Parameters.Add("supplierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBuyback.supplierid;
            command.Parameters.Add("productid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBuyback.productid;
            command.Parameters.Add("brandid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBuyback.brandid;

            command.Parameters.Add("salablegoodsid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBuyback.salablegoodsid;
            command.Parameters.Add("stockid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBuyback.stockid;

            command.Parameters.Add("itemid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBuyback.itemid;
            command.Parameters.Add("schemeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBuyback.schemeid;
            command.Parameters.Add("purchaseprice", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBuyback.purchaseprice;
            command.Parameters.Add("schemediscount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBuyback.schemediscount;
            command.Parameters.Add("receivedemployeeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBuyback.receivedemployeeid;
            command.Parameters.Add("approvedemployeeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBuyback.approvedemployeeid;
            command.Parameters.Add("status", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBuyback.status;
            command.Parameters.Add("statuschangedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBuyback.statuschangedon;
            command.Parameters.Add("buybackdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBuyback.buybackdate;
            command.Parameters.Add("hasconsumed", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBuyback.hasconsumed;
            command.Parameters.Add("consumedbillid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBuyback.consumedbillid;
            command.Parameters.Add("consumedbillno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBuyback.consumedbillno == null ? "" : oBuyback.consumedbillno;
            command.Parameters.Add("imieno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBuyback.imieno == null ? "" : oBuyback.imieno;
            command.Parameters.Add("consumeduserid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBuyback.consumeduserid;
            command.Parameters.Add("buybackkey", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBuyback.buybackkey == null ? "" : oBuyback.buybackkey;
            command.Parameters.Add("buybackno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBuyback.buybackno == null ? "" : oBuyback.buybackno;
            command.Parameters.Add("customerid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBuyback.customerid;
            command.Parameters.Add("customername", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBuyback.customername == null ? "" : oBuyback.customername;
            command.Parameters.Add("customerphoneno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBuyback.customerphoneno == null ? "" : oBuyback.customerphoneno;
            command.Parameters.Add("schemename", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBuyback.schemename == null ? "" : oBuyback.schemename;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBuyback.locationid;
            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBuyback.companyid;
            command.Parameters.Add("customeraddress", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBuyback.customeraddress == null ? "" : oBuyback.customeraddress;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oBuyback.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            return oBuyback;
        }

        public Buyback Update(Buyback oBuyback)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oBuyback);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oBuyback);
            }

            return oBuyback;
        }

        public Buyback Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Buyback oBuyback)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBuyback.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBuyback.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBuyback.modifiedby;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBuyback.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBuyback.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBuyback.modifiedon;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBuyback.id;
            command.Parameters.Add("supplierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBuyback.supplierid;
            command.Parameters.Add("productid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBuyback.productid;
            command.Parameters.Add("brandid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBuyback.brandid;
            command.Parameters.Add("itemid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBuyback.itemid;
            command.Parameters.Add("schemeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBuyback.schemeid;
            command.Parameters.Add("purchaseprice", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBuyback.purchaseprice;
            command.Parameters.Add("schemediscount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBuyback.schemediscount;
            command.Parameters.Add("receivedemployeeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBuyback.receivedemployeeid;
            command.Parameters.Add("approvedemployeeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBuyback.approvedemployeeid;
            command.Parameters.Add("salablegoodsid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBuyback.salablegoodsid;
            command.Parameters.Add("stockid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBuyback.stockid;
            command.Parameters.Add("status", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBuyback.status;
            command.Parameters.Add("statuschangedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBuyback.statuschangedon;
            command.Parameters.Add("buybackdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBuyback.buybackdate;
            command.Parameters.Add("hasconsumed", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBuyback.hasconsumed;
            command.Parameters.Add("consumedbillid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBuyback.consumedbillid;
            command.Parameters.Add("imieno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBuyback.imieno == null ? "" : oBuyback.imieno;
            command.Parameters.Add("consumedbillno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBuyback.consumedbillno == null ? "" : oBuyback.consumedbillno;
            command.Parameters.Add("consumeduserid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBuyback.consumeduserid;
            command.Parameters.Add("buybackkey", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBuyback.buybackkey == null ? "" : oBuyback.buybackkey;
            command.Parameters.Add("buybackno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBuyback.buybackno == null ? "" : oBuyback.buybackno;
            command.Parameters.Add("customerid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBuyback.customerid;
            command.Parameters.Add("customername", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBuyback.customername == null ? "" : oBuyback.customername;
            command.Parameters.Add("customerphoneno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBuyback.customerphoneno == null ? "" : oBuyback.customerphoneno;
            command.Parameters.Add("schemename", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBuyback.schemename == null ? "" : oBuyback.schemename;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBuyback.locationid;
            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBuyback.companyid;
            command.Parameters.Add("customeraddress", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBuyback.customeraddress == null ? "" : oBuyback.customeraddress;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oBuyback.version = Convert.ToInt32(reader[0].ToString());
                }
            }

            return oBuyback;
        }


        public bool Delete(Buyback oBuyback)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oBuyback);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oBuyback);
            }
            return hasSuccessCount;

        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Buyback oBuyback)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBuyback.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBuyback.modifiedby;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBuyback.modifiedon;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBuyback.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBuyback.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

        internal void Sold(Bill bill, Billitems oBillitems, NpgsqlConnection conn, NpgsqlTransaction transaction)
        {
            var buybacks = Search(new BuybackCreteria()
            {
                stockid = oBillitems.stockid
            });

            if (buybacks.Count > 0)
            {
                buybacks[0].consumedbillid = bill.id;
                buybacks[0].consumedbillno = bill.billno;
                buybacks[0].consumeduserid = bill.createdby;
                buybacks[0].Status = Buyback.BuyBackStatus.Sold;
                Update(transaction, conn, buybacks[0]);
            }
        }
    }

}
