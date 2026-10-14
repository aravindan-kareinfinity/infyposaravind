using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
namespace InfyPOS.Data

{
    public class PackbarcodeDataService : SAASData
    {
        const string SQL_Search = @"select id, isactive, createdby, modifiedby, version, createdon, modifiedon, barcodes, barcode,locationid,stockid,salablegoodsid from packbarcode where id=@id";

        const string SQL_SearchBySource = @"
                                with barcodelist
                                as 
                                (
                                select packbarcode.id from packbarcode
                                inner join LATERAL json_array_elements (packbarcode.barcodes :: json) itemlist ON true
                                where cast(itemlist->> 'barcode' as varchar(100)) = @barcode
                                )
                                select packbarcode.* from packbarcode 
                                where id in (select id from barcodelist)
                                order by id desc
                                limit 1";

        const string SQL_SearchHistory = @"SELECT packbarcode.id,packbarcode.stockid,packbarcode.salablegoodsid,packbarcode.locationid,referencelist.name locationname,
packbarcode.barcode, products.name productname,salablegoods.price,salablegoods.iqty qty,
cast(barcodelist->> 'stockid' as bigint) sourcestockid, sourcestock.buyingprice as sourcebuyingprice, sourcestock.price as sourcesellingprice,
cast(barcodelist->> 'salablegoodsid' as bigint) sourcesalablegoodsid,
cast(barcodelist->> 'barcode' as character varying(20)) sourcebarcode,
sourceproduct.name sourceproductname,
cast(barcodelist->> 'qty' as numeric) sourceqty,
packbarcode.createdon, employee.name as createdby
from packbarcode 
INNER JOIN LATERAL json_array_elements (case when packbarcode.barcodes is null or packbarcode.barcodes::text ='null' then '[]'::json else packbarcode.barcodes :: json end) barcodelist ON true
INNER JOIN stock sourcestock on sourcestock.id=cast(barcodelist->> 'stockid' as bigint) 
INNER JOIN products sourceproduct on sourceproduct.id = sourcestock.productid
INNER JOIN stock on stock.id=packbarcode.stockid
INNER JOIN products on products.id=stock.productid
INNER JOIN salablegoods on salablegoods.id=packbarcode.salablegoodsid
LEFT OUTER JOIN referencelist on referencelist.type ='LOCATION' and referencelist.id = packbarcode.locationid
LEFT OUTER JOIN users on users.id=packbarcode.createdby 
LEFT OUTER JOIN employee on employee.id=users.employeeid  ";

        const string SQL_Insert = @"INSERT INTO public.packbarcode(isactive, createdby, modifiedby, version, createdon, modifiedon, barcodes, barcode,locationid,stockid,salablegoodsid) VALUES(@isactive, @createdby, @modifiedby, @version, @createdon, @modifiedon, @barcodes, @barcode,@locationid,@stockid,@salablegoodsid) RETURNING id";

        const string SQL_Update = @"UPDATE public.packbarcode SET modifiedby=@modifiedby, version=@version+1, modifiedon=@modifiedon, barcodes=@barcodes,
                                    barcode=@barcode,locationid=@locationid,stockid=@stockid,salablegoodsid=@salablegoodsid where id=@id and version=@version RETURNING version";

        const string SQL_Delete = @"update public.packbarcode set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";

        public List<Packbarcode> Search(PackbarcodeCreteria _packbarcode)
        {
            List<Packbarcode> lst = new List<Packbarcode>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_packbarcode.id != 0)
                {
                    command = GetCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _packbarcode.id;
                }
                else if (_packbarcode.sourcesearch)
                {
                    command = GetCommand(SQL_SearchBySource, conn);
                    command.Parameters.Add("barcode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = _packbarcode.barcode;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("isactive", true, "=");
                    if (!string.IsNullOrEmpty(_packbarcode.barcode))
                        qb.AddParameter("barcode", _packbarcode.barcode, "=");
                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Packbarcode opackbarcode = new Packbarcode();
                        opackbarcode.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        opackbarcode.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        opackbarcode.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        opackbarcode.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        opackbarcode.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        opackbarcode.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        opackbarcode.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        opackbarcode.barcodes_json = reader["barcodes"] == DBNull.Value ? null : reader["barcodes"].ToString();
                        opackbarcode.barcode = reader["barcode"] == DBNull.Value ? "" : reader["barcode"].ToString();
                        opackbarcode.locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                        opackbarcode.stockid = reader["stockid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["stockid"]);
                        opackbarcode.salablegoodsid = reader["salablegoodsid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["salablegoodsid"]);

                        lst.Add(opackbarcode);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _packbarcode);
            }
            return lst;
        }

        public List<PackbarcodeHistory> SearchHistory(PackbarcodeHistory _packbarcode)
        {
            List<PackbarcodeHistory> lst = new List<PackbarcodeHistory>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_packbarcode.id != 0)
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_SearchHistory;
                    qb.AddParameter("packbarcode.id", _packbarcode.id, "=");
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_SearchHistory;
                    qb.AddParameter("packbarcode.isactive", true, "=");
                    qb.AddParameter("packbarcode.stockid", 0, ">");
                    qb.AddParameter("packbarcode.salablegoodsid", 0, ">");

                    if (_packbarcode.fromdate != DateTime.MinValue && _packbarcode.todate != DateTime.MinValue)
                    {
                        qb.AddParameter("packbarcode.createdon", "fromdate", _packbarcode.fromdate.Date, ">=");
                        qb.AddParameter("packbarcode.createdon", "todate", _packbarcode.todate.Date.AddDays(1), "<");
                    }

                    if (!string.IsNullOrEmpty(_packbarcode.barcode))
                        qb.AddParameter("packbarcode.barcode", _packbarcode.barcode, "=");

                    qb.SortField = "packbarcode.id desc";
                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        PackbarcodeHistory opackbarcode = new PackbarcodeHistory();
                        opackbarcode.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        opackbarcode.barcode = reader["barcode"] == DBNull.Value ? "" : reader["barcode"].ToString();
                        opackbarcode.productname = reader["productname"] == DBNull.Value ? "" : reader["productname"].ToString();
                        opackbarcode.qty = reader["qty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["qty"]);
                        opackbarcode.price = reader["price"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["price"]);
                        opackbarcode.locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                        opackbarcode.locationname = reader["locationname"] == DBNull.Value ? "" : reader["locationname"].ToString();
                        opackbarcode.stockid = reader["stockid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["stockid"]);
                        opackbarcode.salablegoodsid = reader["salablegoodsid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["salablegoodsid"]);
                        opackbarcode.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        opackbarcode.createdby = reader["createdby"] == DBNull.Value ? "" : reader["createdby"].ToString();

                        opackbarcode.sourcebarcode = reader["sourcebarcode"] == DBNull.Value ? "" : reader["sourcebarcode"].ToString();
                        opackbarcode.sourceproductname = reader["sourceproductname"] == DBNull.Value ? "" : reader["sourceproductname"].ToString();
                        opackbarcode.sourceqty = reader["sourceqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["sourceqty"]);
                        opackbarcode.sourcebuyingprice = reader["sourcebuyingprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["sourcebuyingprice"]);
                        opackbarcode.sourcesellingprice = reader["sourcesellingprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["sourcesellingprice"]);

                        lst.Add(opackbarcode);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _packbarcode);
            }
            return lst;
        }


        internal Packbarcode CreateLastBits(PackbarcodeCreteria item)
        {
            List<Packbarcode> packbarcodes = new List<Packbarcode>();
            Packbarcode finalpackbarcode = new Packbarcode();
            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();

                    transaction = conn.BeginTransaction();
                    StockDataService stockDataService = new StockDataService() { Context = this.Context };
                    SalablegoodsDataService salablegoodsDataService = new SalablegoodsDataService() { Context = this.Context };
                    ReferencelistDataService refdataservice = new ReferencelistDataService() { Context = this.Context };
                    var stockInfo = stockDataService.Search(new StockCreteria()
                    {
                        barcodes = item.sourcelist.ConvertAll<string>(e => "'" + WebAPI.Data.Utility.SafeSQL(e.barcode) + "'")
                    });

                    ItemsDataService itemsDataService = new ItemsDataService() { Context = this.Context };
                    List<Products> products = null;
                    ProductsDataService productdataservice = new ProductsDataService() { Context = this.Context };
                    if (item.productid > 0)
                    {
                        products = productdataservice.Search(new ProductsCreteria()
                        {
                            id = item.productid
                        });
                    }

                    long _companyid = 0;
                    var reflocationlist = refdataservice.Search(new ReferencelistCreteria() { id = item.locationid });
                    if (reflocationlist != null && reflocationlist.Count > 0)
                        _companyid = reflocationlist[0].additionalinfo.parentid;

                    if (item.companyid == 0 && _companyid > 0)
                        item.companyid = _companyid;

                    if (item.companyid == 0 && stockInfo.Count > 0)
                        item.companyid = stockInfo.First().companyid;

                    long itemid = item.sourcelist.Exists(e => e.itemid > 0) ? item.sourcelist.First(e => e.itemid > 0).itemid : 0;
                    //bool isbase36 = products.Count == 1 && products[0].barcodemode == 1;
                    var barcodeConfig = InfyPOS.Processors.BarcodeManager.Instance(Context.OrganizationID).Get(products[0].barcodemode, item.companyid);
                    if (item.productid != 0 && (!item.sourcelist.All(e => e.productid == item.productid) || itemid == 0))
                    {
                        var existingitem = new List<Items>();
                        if (itemid > 0)
                        {
                            existingitem = itemsDataService.Search(new ItemsCreteria()
                            {
                                id = itemid
                            });
                        }
                        if (existingitem.Count == 0)
                        {
                            var newitems = itemsDataService.Search(new ItemsCreteria()
                            {
                                productmap = true,
                                productid = item.productid,
                            });

                            if (newitems.Count > 0)
                            {
                                itemid = newitems.First().id;
                            }
                            else
                            {
                                var items = new Items();
                                items.id = 0;
                                items.productid = items.id;
                                itemsDataService.Insert(items);
                                itemid = items.id;
                            }
                        }
                    }


                    var stocklist = stockDataService.Search(new StockCreteria()
                    {
                        ids = item.sourcelist.ConvertAll<long>(e => e.stockid)
                    });
                    if (item.locationid > 0)
                    {

                        var salablegoodslist = salablegoodsDataService.Search(new SalablegoodsCreteria()
                        {
                            ids = item.sourcelist.ConvertAll<long>(e => e.salablegoodsid)
                        });
                        foreach (var sitem in salablegoodslist)
                        {
                            var sourceitem = item.sourcelist.Find(e => e.salablegoodsid == sitem.id);
                            sitem.dqty += (sourceitem.adjstock * -1);
                            sourceitem.buyingprice = sourceitem.buyingprice;
                            salablegoodsDataService.Update(transaction, conn, sitem, sitem);
                        }
                    }
                    else
                    {
                        foreach (var stockitem in stocklist)
                        {
                            var sourceitem = item.sourcelist.Find(e => e.stockid == stockitem.id);
                            stockitem.damagedqty += (sourceitem.adjstock * -1);
                            sourceitem.buyingprice = sourceitem.buyingprice;
                            stockDataService.Update(transaction, conn, stockitem);
                        }
                    }

                    List<InfyPOS.Processors.BarcodeManager.NewBarcodeID> barcodeList;
                    if (Services.ConfigManager.Instance(Context.OrganizationID).Config.system_locationbasedbarcodes && item.locationid > 0)
                        barcodeList = stockDataService.CreateBarcode(barcodeConfig.Type, 1, barcodeConfig.companyid, conn, transaction,
                            item.locationid,null,null);
                    else
                        barcodeList = stockDataService.CreateBarcode(barcodeConfig.Type, 1, barcodeConfig.companyid, conn, transaction,
                            0,null,null);

                    Packbarcode packbarcode = new Packbarcode();
                    packbarcode.barcode = barcodeList[0].code;
                    packbarcode.barcodeid = barcodeList[0].id;
                    packbarcode.barcodes = new List<Packbarcode.Barcodes>();
                    foreach (var sitem in item.sourcelist)
                    {
                        packbarcode.barcodes.Add(new Packbarcode.Barcodes()
                        {
                            barcode = sitem.barcode,
                            qty = sitem.qty,
                            stockid = sitem.stockid,
                            salablegoodsid = sitem.salablegoodsid
                        });
                    }
                    packbarcode.createdby = Context.UserID;
                    packbarcode.createdon = DateTimeNow;
                    packbarcode.isactive = true;
                    packbarcode.modifiedby = Context.UserID;
                    packbarcode.modifiedon = DateTimeNow;


                    Stock stock = new Stock();
                    stock.barcode = packbarcode.barcode;
                    stock.barcodeid = packbarcode.barcodeid;
                    stock.price = item.price;
                    stock.discount = item.discount;
                    stock.buyingqty = item.sourcelist.Sum(e => e.qty);
                    stock.companyid = stocklist.First().companyid;
                    stock.hsncode = item.hsncode;
                    stock.locationid = item.locationid;
                    stock.printingname = products.First().name;
                    stock.productid = item.productid;
                    stock.qty = stock.buyingqty;
                    stock.lrreference = stocklist.First().lrreference;
                    stock.sellingmode = products.First().sellingmode;
                    stock.stockdate = stocklist.First().stockdate;
                    stock.suppliercode = stocklist.First().suppliercode;
                    stock.supplierid = stocklist.First().supplierid;
                    stock.invoiceitemid = stocklist.First().invoiceitemid;
                    stock.designid = stocklist.First().designid;
                    stock.buyingprice = stocklist.First().buyingprice;
                    stock.batch = stocklist.First().batch;
                    stock.serialno = stocklist.First().serialno;
                    stock.expiry = stocklist.First().expiry;
                    stock.locationid = stocklist.First().locationid;
                    stock.retaillocationid = stocklist.First().retaillocationid;

                    stock.createdby = Context.UserID;
                    stock.createdon = DateTimeNow;
                    stock.isactive = true;
                    stock.version = 1;
                    stock.itemid = itemid;
                    stock.modifiedby = Context.UserID;
                    stock.modifiedon = DateTimeNow;
                    stock.isbit = item.isbit;
                    stockDataService.Insert(transaction, conn, stock);

                    if (item.locationid > 0)
                    {
                        Salablegoods salablegoods = new Salablegoods();
                        salablegoods.stockid = stock.id;
                        salablegoods.barcode = stock.barcode;
                        salablegoods.companyid = stock.companyid;
                        salablegoods.discount = stock.discount;
                        salablegoods.inwardon = DateTimeNow;
                        salablegoods.iqty = stock.qty;
                        salablegoods.locationid = item.locationid;
                        salablegoods.lrreference = stock.lrreference;
                        salablegoods.price = stock.price;
                        salablegoods.received = true;
                        salablegoods.sellingname = stock.printingname;
                        salablegoods.supplierid = stock.supplierid;
                        salablegoods.suppliername = stock.suppliercode;
                        salablegoods.createdby = Context.UserID;
                        salablegoods.createdon = DateTimeNow;
                        salablegoods.isactive = true;
                        salablegoods.version = 1;
                        salablegoods.modifiedby = Context.UserID;
                        salablegoods.modifiedon = DateTimeNow;
                        salablegoods.itemid = itemid;
                        salablegoods.costprice = item.price;
                        salablegoodsDataService.Insert(transaction, conn, salablegoods);
                        packbarcode.locationid = item.locationid;
                        packbarcode.stockid = stock.id;
                        packbarcode.salablegoodsid = salablegoods.id;
                        Insert(transaction, conn, packbarcode);
                    }
                    packbarcodes.Add(packbarcode);


                    finalpackbarcode.barcodes = new List<Packbarcode.Barcodes>();
                    finalpackbarcode.createdby = Context.UserID;
                    finalpackbarcode.createdon = DateTimeNow;
                    finalpackbarcode.isactive = true;
                    finalpackbarcode.modifiedby = Context.UserID;
                    finalpackbarcode.modifiedon = DateTimeNow;
                    finalpackbarcode.locationid = item.locationid;
                    finalpackbarcode.barcode = stockDataService.CreateBarcode(Processors.BarcodeManager.BarcodeType.Base36, 1, item.companyid, conn, transaction).First().code + "$";
                    foreach (var pitem in packbarcodes)
                    {
                        finalpackbarcode.barcodes.Add(new Packbarcode.Barcodes()
                        {
                            barcode = pitem.barcode,
                            qty = 1,
                            pieces = 1,
                            salablegoodsid = pitem.salablegoodsid,
                            stockid = pitem.salablegoodsid,
                            itemid = itemid
                        });
                    }
                    Insert(transaction, conn, finalpackbarcode);
                    transaction.Commit();
                    conn.Close();

                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, item);
            }

            return finalpackbarcode;
        }

        internal Packbarcode CreateBarcode(PackbarcodeCreteria item)
        {
            return Insert(new Packbarcode()
            {
                locationid = item.locationid,
                barcodes = item.sourcelist.ConvertAll(e => new Packbarcode.Barcodes()
                {
                    barcode = e.barcode,
                    qty = e.qty
                }),
                companyid = item.companyid,
                isactive = true,
                createdby = Context.UserID,
                createdon = DateTimeNow,
                modifiedby = Context.UserID,
                modifiedon = DateTimeNow,
                version = 1
            });
        }

        internal Packbarcode CreatePackcode(PackbarcodeCreteria item)
        {
            List<Packbarcode> packbarcodes = new List<Packbarcode>();
            Packbarcode finalpackbarcode = new Packbarcode();
            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    StockDataService stockDataService = new StockDataService() { Context = this.Context };
                    ProductsDataService productdataservice = new ProductsDataService() { Context = this.Context };
                    ReferencelistDataService refdataservice = new ReferencelistDataService() { Context = this.Context };
                    var products = productdataservice.Search(new ProductsCreteria()
                    {
                        id = item.productid
                    });

                    if (item.companyid == 0 && products[0].prodcompanyid > 0)
                        item.companyid = products[0].prodcompanyid;

                    //bool isbase36 = products.Count == 1 && products[0].barcodemode == 1;
                    var barcodeConfig = InfyPOS.Processors.BarcodeManager.Instance(Context.OrganizationID).Get(products[0].barcodemode, item.companyid);
                    bool ispack = products.Count == 1 && products[0].SellingMode == Products.sellingMode.Pack;
                    ItemsDataService itemsDataService = new ItemsDataService() { Context = this.Context };
                    var items = itemsDataService.Search(new ItemsCreteria()
                    {
                        productid = item.productid,
                        productmap = true,
                        sizeid = 0,
                        styleid = 0,
                        materialid = 0,
                        brandid = 0,
                        typeid = 0,
                        patternid = 0,
                        colourid = 0
                    });
                    if (items.Count == 0)
                    {
                        Items itemmaster = new Items();
                        itemmaster.printingname = products[0].name;
                        itemmaster.productid = products[0].id;
                        itemmaster.productname = itemmaster.printingname;
                        itemmaster.isactive = true;
                        itemmaster.createdby = Context.UserID;
                        itemmaster.createdon = DateTimeNow;
                        itemmaster.isactive = true;
                        itemmaster.modifiedby = Context.UserID;
                        itemmaster.modifiedon = DateTimeNow;
                        itemsDataService.Insert(transaction, conn, itemmaster);
                        items.Add(itemmaster);
                    }

                    if (item.locationid > 0)
                    {
                        long _companyid = 0;
                        var reflocationlist = refdataservice.Search(new ReferencelistCreteria() { id = item.locationid });
                        if (reflocationlist != null && reflocationlist.Count > 0)
                            _companyid = reflocationlist[0].additionalinfo.parentid;

                        SalablegoodsDataService salablegoodsDataService = new SalablegoodsDataService() { Context = this.Context };
                        var salablegoodslist = salablegoodsDataService.Search(new SalablegoodsCreteria()
                        {
                            ids = item.sourcelist.ConvertAll<long>(e => e.salablegoodsid)
                        });
                        if (salablegoodslist == null || salablegoodslist.Count == 0) return null;

                        var stocklist = stockDataService.Search(new StockCreteria()
                        {
                            ids = salablegoodslist.ConvertAll<long>(e => e.stockid)
                        });

                        if (item.companyid == 0 && _companyid > 0)
                            item.companyid = _companyid;

                        if (item.companyid == 0 && stocklist.First().companyid > 0)
                            item.companyid = stocklist.First().companyid;

                        var barcodeList = stockDataService.CreateBarcode(barcodeConfig.Type, ispack ? 1 : item.noofpack, item.companyid, conn, transaction, new MissingBarcode()
                        {
                            barcode = item.barcode,
                            productid = item.productid,
                            sellingprice = item.price
                        });
                        foreach (var sitem in salablegoodslist)
                        {
                            var sourceitem = item.sourcelist.Find(e => e.salablegoodsid == sitem.id);
                            sitem.dqty += sourceitem.qty;
                            var stockitem = stocklist.Find(e => e.id == sitem.stockid);
                            stockitem.damagedqty += sourceitem.qty;
                            sourceitem.buyingprice = stockitem.buyingprice;
                            //stockDataService.Update(transaction, conn, stockitem);
                            salablegoodsDataService.Update(transaction, conn, sitem, sitem);
                        }
                        if (ispack)
                        {
                            Packbarcode packbarcode = new Packbarcode();
                            packbarcode.barcode = barcodeList[0].code;
                            packbarcode.barcodeid = barcodeList[0].id;
                            packbarcode.barcodes = new List<Packbarcode.Barcodes>();
                            foreach (var sitem in salablegoodslist)
                            {
                                var sourceitem = item.sourcelist.Find(e => e.salablegoodsid == sitem.id);
                                packbarcode.barcodes.Add(new Packbarcode.Barcodes()
                                {
                                    barcode = sourceitem.barcode,
                                    qty = sourceitem.qty,
                                    stockid = sitem.stockid,
                                    salablegoodsid = sitem.id
                                });
                            }
                            packbarcode.createdby = Context.UserID;
                            packbarcode.createdon = DateTimeNow;
                            packbarcode.isactive = true;
                            packbarcode.modifiedby = Context.UserID;
                            packbarcode.modifiedon = DateTimeNow;


                            Stock stock = new Stock();
                            stock.barcode = packbarcode.barcode;
                            stock.barcodeid = packbarcode.barcodeid;
                            //stock.buyingprice = item.sourcelist.Sum(e => e.buyingprice * e.qty) / item.noofpack;
                            //stock.ibuyingprice = stocklist.Sum(e => e.ibuyingprice * e.qty) / item.noofpack;
                            stock.buyingprice = item.sourcelist.Sum(e => e.buyingprice);
                            stock.ibuyingprice = stocklist.Sum(e => e.ibuyingprice);
                            stock.price = item.price;
                            stock.discount = item.discount;
                            stock.displaymrp = item.displaymrp;
                            stock.buyingqty = item.noofpack;
                            stock.companyid = _companyid > 0 ? _companyid : stocklist.First().companyid;
                            stock.hsncode = item.hsncode;
                            stock.locationid = item.locationid;
                            stock.printingname = products.First().name;
                            stock.productid = item.productid;
                            stock.qty = item.noofpack;
                            stock.sellingmode = products.First().sellingmode;
                            stock.stockdate = stocklist.First().stockdate;
                            stock.suppliercode = stocklist.First().suppliercode;
                            stock.supplierid = stocklist.First().supplierid;
                            stock.createdby = Context.UserID;
                            stock.createdon = DateTimeNow;
                            stock.isactive = true;
                            stock.itemid = items[0].id;
                            stock.modifiedby = Context.UserID;
                            stock.modifiedon = DateTimeNow;
                            stockDataService.UpdateEPrice(stock);
                            stockDataService.Insert(transaction, conn, stock);

                            Salablegoods salablegoods = new Salablegoods();
                            salablegoods.stockid = stock.id;
                            salablegoods.barcode = stock.barcode;
                            salablegoods.companyid = stock.companyid;
                            salablegoods.discount = stock.discount;
                            salablegoods.inwardon = salablegoodslist.First().inwardon;
                            salablegoods.iqty = stock.qty;
                            salablegoods.locationid = item.locationid;
                            salablegoods.lrreference = salablegoodslist.First().lrreference;
                            salablegoods.price = stock.price;
                            salablegoods.received = true;
                            salablegoods.sellingname = stock.printingname;
                            salablegoods.supplierid = stock.supplierid;
                            salablegoods.suppliername = stock.suppliercode;
                            salablegoods.createdby = Context.UserID;
                            salablegoods.createdon = DateTimeNow;
                            salablegoods.isactive = true;
                            salablegoods.modifiedby = Context.UserID;
                            salablegoods.modifiedon = DateTimeNow;
                            salablegoods.itemid = items[0].id;
                            salablegoods.costprice = item.price;
                            salablegoodsDataService.Insert(transaction, conn, salablegoods);
                            packbarcode.locationid = item.locationid;
                            packbarcode.stockid = stock.id;
                            packbarcode.salablegoodsid = salablegoods.id;
                            Insert(transaction, conn, packbarcode);
                            packbarcodes.Add(packbarcode);
                        }
                        else
                        {
                            foreach (var barcode in barcodeList)
                            {
                                Packbarcode packbarcode = new Packbarcode();
                                packbarcode.barcode = barcode.code;
                                packbarcode.barcodeid = barcode.id;
                                packbarcode.barcodes = new List<Packbarcode.Barcodes>();
                                packbarcode.createdby = Context.UserID;
                                packbarcode.createdon = DateTimeNow;
                                packbarcode.isactive = true;
                                packbarcode.modifiedby = Context.UserID;
                                packbarcode.modifiedon = DateTimeNow;

                                foreach (var packinfo in item.packlist)
                                {
                                    var ratio = packinfo.available / packinfo.qty;
                                    var sourceitems = item.sourcelist.FindAll(e => e.consumed < e.qty && e.productid == packinfo.productid);
                                    PackbarcodeCreteria.source sourceitem = null;
                                    foreach (var sitem in sourceitems)
                                    {
                                        if ((sitem.qty - sitem.consumed) / ratio == Math.Round((sitem.qty - sitem.consumed) / ratio, 0))
                                        {
                                            sourceitem = sitem;
                                            break;
                                        }
                                    }
                                    if (sourceitem == null)
                                        sourceitem = sourceitems[0];

                                    sourceitem.consumed += packinfo.qty;
                                    packbarcode.barcodes.Add(new Packbarcode.Barcodes()
                                    {
                                        barcode = sourceitem.barcode,
                                        qty = packinfo.qty,
                                        stockid = salablegoodslist.Find(e => e.barcode == sourceitem.barcode).stockid,
                                        salablegoodsid = salablegoodslist.Find(e => e.barcode == sourceitem.barcode).id
                                    });
                                }


                                Stock stock = new Stock();
                                stock.barcode = packbarcode.barcode;
                                stock.barcodeid = packbarcode.barcodeid;
                                stock.buyingprice = item.sourcelist.Sum(e => e.buyingprice);
                                stock.ibuyingprice = stocklist.Sum(e => e.ibuyingprice);
                                stock.price = item.price;
                                stock.discount = item.discount;
                                stock.displaymrp = item.displaymrp;
                                stock.buyingqty = item.noofpack;
                                stock.companyid = _companyid > 0 ? _companyid : stocklist.First().companyid;
                                stock.hsncode = item.hsncode;
                                stock.itemid = items[0].id;
                                stock.locationid = item.locationid;
                                stock.printingname = products.First().name;
                                stock.productid = item.productid;
                                stock.qty = 1;
                                stock.sellingmode = products.First().sellingmode;
                                stock.stockdate = stocklist.First().stockdate;
                                stock.suppliercode = stocklist.First().suppliercode;
                                stock.supplierid = stocklist.First().supplierid;
                                stock.createdby = Context.UserID;
                                stock.createdon = DateTimeNow;
                                stock.isactive = true;
                                stock.modifiedby = Context.UserID;
                                stock.modifiedon = DateTimeNow;
                                stockDataService.UpdateEPrice(stock);
                                stockDataService.Insert(transaction, conn, stock);

                                Salablegoods salablegoods = new Salablegoods();
                                salablegoods.stockid = stock.id;
                                salablegoods.barcode = stock.barcode;
                                salablegoods.companyid = stock.companyid;
                                salablegoods.discount = stock.discount;
                                salablegoods.inwardon = salablegoodslist.First().inwardon;
                                salablegoods.iqty = stock.qty;
                                salablegoods.locationid = item.locationid;
                                salablegoods.lrreference = salablegoodslist.First().lrreference;
                                salablegoods.price = stock.price;
                                salablegoods.received = true;
                                salablegoods.itemid = items[0].id;
                                salablegoods.sellingname = stock.printingname;
                                salablegoods.supplierid = stock.supplierid;
                                salablegoods.suppliername = stock.suppliercode;
                                salablegoods.createdby = Context.UserID;
                                salablegoods.createdon = DateTimeNow;
                                salablegoods.isactive = true;
                                salablegoods.modifiedby = Context.UserID;
                                salablegoods.modifiedon = DateTimeNow;
                                salablegoodsDataService.Insert(transaction, conn, salablegoods);
                                packbarcode.stockid = stock.id;
                                packbarcode.salablegoodsid = salablegoods.id;
                                packbarcode.locationid = item.locationid;
                                Insert(transaction, conn, packbarcode);
                                packbarcodes.Add(packbarcode);
                            }
                        }
                    }
                    else
                    {

                        var stocklist = stockDataService.Search(new StockCreteria()
                        {
                            ids = item.sourcelist.ConvertAll<long>(e => e.stockid)
                        });

                        if (item.companyid == 0 && stocklist.First().companyid > 0)
                            item.companyid = stocklist.First().companyid;

                        var barcodeList = stockDataService.CreateBarcode(barcodeConfig.Type, ispack ? 1 : item.noofpack, item.companyid, conn, transaction, new MissingBarcode()
                        {
                            barcode = item.barcode,
                            productid = item.productid,
                            sellingprice = item.price
                        });
                        foreach (var sitem in stocklist)
                        {
                            var sourceitem = item.sourcelist.Find(e => e.stockid == sitem.id);
                            sitem.damagedqty += sourceitem.qty;
                            sourceitem.buyingprice = sitem.buyingprice;
                            stockDataService.Update(transaction, conn, sitem);
                        }

                        if (ispack)
                        {
                            Packbarcode packbarcode = new Packbarcode();
                            packbarcode.barcode = barcodeList[0].code;
                            packbarcode.barcodeid = barcodeList[0].id;
                            packbarcode.barcodes = new List<Packbarcode.Barcodes>();
                            foreach (var sitem in stocklist)
                            {
                                var sourceitem = item.sourcelist.Find(e => e.stockid == sitem.id);
                                packbarcode.barcodes.Add(new Packbarcode.Barcodes()
                                {
                                    barcode = sourceitem.barcode,
                                    qty = sourceitem.qty,
                                    stockid = sitem.id
                                });
                            }
                            packbarcode.createdby = Context.UserID;
                            packbarcode.createdon = DateTimeNow;
                            packbarcode.isactive = true;
                            packbarcode.modifiedby = Context.UserID;
                            packbarcode.modifiedon = DateTimeNow;


                            Stock stock = new Stock();
                            stock.barcode = packbarcode.barcode;
                            stock.barcodeid = packbarcode.barcodeid;
                            stock.buyingprice = item.sourcelist.Sum(e => e.buyingprice);
                            stock.ibuyingprice = stocklist.Sum(e => e.ibuyingprice);
                            stock.price = item.price;
                            stock.discount = item.discount;
                            stock.displaymrp = item.displaymrp;
                            stock.buyingqty = item.noofpack;
                            stock.itemid = items[0].id;
                            stock.companyid = stocklist.First().companyid;
                            stock.hsncode = item.hsncode;
                            stock.locationid = item.locationid;
                            stock.printingname = products.First().name;
                            stock.productid = item.productid;
                            stock.qty = item.noofpack;
                            stock.sellingmode = products.First().sellingmode;
                            stock.stockdate = stocklist.First().stockdate;
                            stock.suppliercode = stocklist.First().suppliercode;
                            stock.supplierid = stocklist.First().supplierid;
                            stock.createdby = Context.UserID;
                            stock.createdon = DateTimeNow;
                            stock.isactive = true;
                            stock.modifiedby = Context.UserID;
                            stock.modifiedon = DateTimeNow;
                            stockDataService.UpdateEPrice(stock);
                            stockDataService.Insert(transaction, conn, stock);
                            packbarcode.stockid = stock.id;
                            Insert(transaction, conn, packbarcode);
                            packbarcodes.Add(packbarcode);
                        }
                        else
                        {
                            foreach (var barcode in barcodeList)
                            {
                                Packbarcode packbarcode = new Packbarcode();
                                packbarcode.barcode = barcode.code;
                                packbarcode.barcodeid = barcode.id;
                                packbarcode.barcodes = new List<Packbarcode.Barcodes>();
                                packbarcode.createdby = Context.UserID;
                                packbarcode.createdon = DateTimeNow;
                                packbarcode.isactive = true;
                                packbarcode.modifiedby = Context.UserID;
                                packbarcode.modifiedon = DateTimeNow;

                                foreach (var packinfo in item.packlist)
                                {
                                    var ratio = packinfo.available / packinfo.qty;
                                    var sourceitems = item.sourcelist.FindAll(e => e.consumed < e.qty && e.productid == packinfo.productid);
                                    PackbarcodeCreteria.source sourceitem = null;
                                    foreach (var sitem in sourceitems)
                                    {
                                        if ((sitem.qty - sitem.consumed) / ratio == Math.Round((sitem.qty - sitem.consumed) / ratio, 0))
                                        {
                                            sourceitem = sitem;
                                            break;
                                        }
                                    }
                                    if (sourceitem == null)
                                        sourceitem = sourceitems[0];

                                    sourceitem.consumed += packinfo.qty;
                                    packbarcode.barcodes.Add(new Packbarcode.Barcodes()
                                    {
                                        barcode = sourceitem.barcode,
                                        qty = packinfo.qty,
                                        stockid = stocklist.Find(e => e.barcode == sourceitem.barcode).id
                                    });
                                }

                                Stock stock = new Stock();
                                stock.barcode = packbarcode.barcode;
                                stock.buyingprice = item.sourcelist.Sum(e => e.buyingprice);
                                stock.ibuyingprice = stocklist.Sum(e => e.ibuyingprice);
                                stock.price = item.price;
                                stock.discount = item.discount;
                                stock.displaymrp = item.displaymrp;
                                stock.buyingqty = item.noofpack;
                                stock.itemid = items[0].id;
                                stock.companyid = stocklist.First().companyid;
                                stock.hsncode = item.hsncode;
                                stock.locationid = item.locationid;
                                stock.printingname = products.First().name;
                                stock.productid = item.productid;
                                stock.qty = 1;
                                stock.sellingmode = products.First().sellingmode;
                                stock.stockdate = stocklist.First().stockdate;
                                stock.suppliercode = stocklist.First().suppliercode;
                                stock.supplierid = stocklist.First().supplierid;
                                stock.createdby = Context.UserID;
                                stock.createdon = DateTimeNow;
                                stock.isactive = true;
                                stock.modifiedby = Context.UserID;
                                stock.modifiedon = DateTimeNow;
                                stockDataService.UpdateEPrice(stock);
                                stockDataService.Insert(transaction, conn, stock);
                                packbarcode.stockid = stock.id;
                                Insert(transaction, conn, packbarcode);
                                packbarcodes.Add(packbarcode);
                            }
                        }
                    }


                    finalpackbarcode.barcodes = new List<Packbarcode.Barcodes>();
                    finalpackbarcode.createdby = Context.UserID;
                    finalpackbarcode.createdon = DateTimeNow;
                    finalpackbarcode.isactive = true;
                    finalpackbarcode.modifiedby = Context.UserID;
                    finalpackbarcode.modifiedon = DateTimeNow;
                    finalpackbarcode.locationid = item.locationid;
                    finalpackbarcode.barcode = stockDataService.CreateBarcode(Processors.BarcodeManager.BarcodeType.Base36, 1, item.companyid, conn, transaction).First().code + "$";
                    foreach (var pitem in packbarcodes)
                    {
                        finalpackbarcode.barcodes.Add(new Packbarcode.Barcodes()
                        {
                            barcode = pitem.barcode,
                            qty = ispack ? item.noofpack : 1,
                            pieces = ispack ? item.noofpack : 1,
                            salablegoodsid = pitem.salablegoodsid,
                            stockid = pitem.salablegoodsid
                        });
                    }
                    Insert(transaction, conn, finalpackbarcode);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, item);
                throw exp;
            }

            return finalpackbarcode;
        }


        public Packbarcode Insert(Packbarcode oPackbarcode)
        {
            NpgsqlTransaction transaction = null;
            try
            {
                StockDataService lrinvoiceitemsData = new StockDataService() { Context = this.Context };
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);
                    if (string.IsNullOrEmpty(oPackbarcode.barcode))
                        oPackbarcode.barcode = lrinvoiceitemsData.CreateBarcode(Processors.BarcodeManager.BarcodeType.Pack10, 1, oPackbarcode.companyid, conn, transaction).First().code + "$";
                    Insert(transaction, conn, oPackbarcode);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oPackbarcode);
                throw exp;
            }
            return oPackbarcode;
        }
        public Packbarcode Insert(Packbarcode oPackbarcode, StockDataService lrinvoiceitemsData)
        {
            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);
                    if (string.IsNullOrEmpty(oPackbarcode.barcode))
                        oPackbarcode.barcode = lrinvoiceitemsData.CreateBarcode(Processors.BarcodeManager.BarcodeType.Base36, 1, oPackbarcode.companyid, conn, transaction).First().code + "$";
                    Insert(transaction, conn, oPackbarcode);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oPackbarcode);
                throw exp;
            }

            return oPackbarcode;
        }

        public Packbarcode Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Packbarcode oPackbarcode)
        {

            NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oPackbarcode.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;

            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPackbarcode.locationid;
            command.Parameters.Add("stockid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPackbarcode.stockid;
            command.Parameters.Add("salablegoodsid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPackbarcode.salablegoodsid;

            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oPackbarcode.version;
            command.Parameters.Add("barcodes", NpgsqlTypes.NpgsqlDbType.Json).Value = oPackbarcode.barcodes_json == null ? "null" : oPackbarcode.barcodes_json;
            command.Parameters.Add("barcode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oPackbarcode.barcode == null ? "" : oPackbarcode.barcode;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oPackbarcode.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            return oPackbarcode;
        }

        public Packbarcode Update(Packbarcode oPackbarcode)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oPackbarcode);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oPackbarcode);
                throw exp;
            }

            return oPackbarcode;
        }

        public Packbarcode Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Packbarcode oPackbarcode)
        {

            NpgsqlCommand command = GetCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPackbarcode.id;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oPackbarcode.isactive;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oPackbarcode.version;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPackbarcode.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oPackbarcode.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPackbarcode.locationid;
            command.Parameters.Add("stockid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPackbarcode.stockid;
            command.Parameters.Add("salablegoodsid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPackbarcode.salablegoodsid;
            command.Parameters.Add("barcodes", NpgsqlTypes.NpgsqlDbType.Json).Value = oPackbarcode.barcodes_json == null ? "null" : oPackbarcode.barcodes_json;
            command.Parameters.Add("barcode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oPackbarcode.barcode == null ? "" : oPackbarcode.barcode;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oPackbarcode.version = Convert.ToInt32(reader[0].ToString());
                }
            }

            return oPackbarcode;
        }


        public bool Delete(Packbarcode oPackbarcode)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oPackbarcode);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oPackbarcode);

                throw exp;
            }
            return hasSuccessCount;

        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Packbarcode oPackbarcode)
        {

            NpgsqlCommand command = GetCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oPackbarcode.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oPackbarcode.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPackbarcode.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

    }

}
