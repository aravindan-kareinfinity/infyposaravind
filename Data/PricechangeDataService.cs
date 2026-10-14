using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
namespace InfyPOS.Data
{
    public class PricechangeDataService : SAASData
    {
        const string SQL_Search = @"select id, approvedby, reason, changedon, itemlist, isactive, createdby, modifiedby, version, createdon, modifiedon, companyid,iswarehouse from pricechange where id=@id";

        const string SQL_UpdateSalableGoods = @"WITH OLD AS (
                                                            SELECT price,discount FROM salablegoods WHERE id = @id
                                                            )
                                                            update salablegoods set price=@price,discount=@discount,version=version+1 where id=@id
                                                            RETURNING (SELECT json_build_object('price',price::numeric,'discount',discount::numeric) FROM OLD);";

        const string SQL_UpdateStock = @"WITH OLD AS (
                                                     SELECT price,discount FROM Stock WHERE id = @id
                                                     )
                                                     update Stock set price=@price,discount=@discount,version=version+1 where id=@id
                                                     RETURNING (SELECT json_build_object('price',price::numeric,'discount',discount::numeric) FROM OLD);";


        //const string SQL_UpdateStockDealerPrice = @"
        //                                             update lrinvoiceitems set dealerprice = @dealerprice,version=lrinvoiceitems.version+1 from stock
        //                                             where lrinvoiceitems.id = stock.invoiceitemid and stock.id = @id;
        //                                             WITH OLD AS (
        //                                                SELECT dealerprice,discount FROM Stock WHERE id = @id
        //                                             )
        //                                             update Stock set dealerprice=@dealerprice,version=version+1 where id=@id
        //                                             RETURNING (SELECT json_build_object('dealerprice',dealerprice::numeric,'discount',discount::numeric) FROM OLD);";

        const string SQL_UpdateStockDealerPrice = @"WITH OLD AS (
                                                        SELECT dealerprice,discount FROM Stock WHERE id = @id
                                                     )
                                                     update Stock set dealerprice=@dealerprice,version=version+1 where id=@id
                                                     RETURNING (SELECT json_build_object('dealerprice',dealerprice::numeric,'discount',discount::numeric) FROM OLD);";

        const string SQL_UpdateDealerPrice = @"WITH OLD AS (
                                                     SELECT lrinvoiceitems.sellingprice,lrinvoiceitems.dealerprice FROM stock
                                                     inner join lrinvoiceitems on lrinvoiceitems.id = stock.invoiceitemid
                                                     WHERE stock.id = @id
                                                     )
                                                     update lrinvoiceitems set dealerprice = @price,version=lrinvoiceitems.version+1 from stock
                                                     where lrinvoiceitems.id = stock.invoiceitemid and stock.id = @id
                                                     RETURNING (SELECT json_build_object('price',sellingprice::numeric,'discount',dealerprice::numeric) FROM OLD);";

        const string SQL_SearchStock = @"select stock.id,stock.price,stock.discount,products.name printingname,pricetag.name pricetagname,stock.barcode,stock.buyingprice buyingprice, stock.ibuyingprice ibuyingprice,
                                        stock.qty-(stock.transfered+stock.journalqty+stock.damagedqty+stock.returnqty) stock,lrinvoiceitems.mrp,stock.serialno,stock.dealerprice,
                                        (case when tax.type != 2 then tax.taxpercentage when (stock.price-stock.discount)>1045 then 12 else 5 end) taxpercentage
                                        from stock 
                                        inner join items on items.id = stock.itemid
                                        inner join products on products.id =  items.productid
                                        inner join tax on tax.id = products.salestaxid
                                        left outer join lrinvoiceitems on lrinvoiceitems.id = stock.invoiceitemid  and lrinvoiceitems.isactive = true
                                        left outer join lrinvoice on lrinvoiceitems.lrinvoiceid = lrinvoice.id and lrinvoice.isactive = true
                                        left outer join lrentry on lrentry.id=lrinvoice.lrid and lrentry.isactive = true
                                        left outer join referencelist pricetag on pricetag.id = stock.pricetagid
                                        where stock.isactive=true and (stock.qty-(stock.transfered+stock.journalqty+stock.damagedqty+stock.returnqty)>0)";

        const string SQL_SearchSalableStock = @"select salablegoods.id,salablegoods.price,salablegoods.discount,pricetag.name pricetagname,products.name printingname,stock.buyingprice buyingprice,
                                        stock.ibuyingprice ibuyingprice,lrinvoiceitems.mrp,stock.serialno,stock.dealerprice,
                                        (case when tax.type != 2 then tax.taxpercentage when (salablegoods.price-salablegoods.discount)>1045 then 12 else 5 end) taxpercentage,
                                        salablegoods.iqty-(salablegoods.oqty+salablegoods.jqty+salablegoods.dqty) stock,salablegoods.barcode 
                                        from salablegoods 
                                        inner join stock on stock.id=salablegoods.stockid
                                        inner join items on items.id = stock.itemid
                                        inner join products on products.id =  items.productid
                                        inner join tax on tax.id = products.salestaxid
                                        left outer join lrinvoiceitems on lrinvoiceitems.id = stock.invoiceitemid  and lrinvoiceitems.isactive = true
                                        left outer join lrinvoice on lrinvoiceitems.lrinvoiceid = lrinvoice.id and lrinvoice.isactive = true
                                        left outer join lrentry on lrentry.id=lrinvoice.lrid and lrentry.isactive = true
                                        left outer join referencelist pricetag on pricetag.id = stock.pricetagid
                                        where salablegoods.isactive=true and (salablegoods.iqty-(salablegoods.oqty+salablegoods.jqty+salablegoods.dqty)>0)";

        const string SQL_Insert = @"INSERT INTO public.pricechange(approvedby, reason, changedon, itemlist, isactive, createdby, modifiedby, version, createdon, modifiedon, companyid,iswarehouse,locationid) 
                                    VALUES(@approvedby, @reason, @changedon, @itemlist, @isactive, @createdby, @modifiedby, @version, @createdon, @modifiedon, @companyid,@iswarehouse,@locationid) RETURNING id";

        const string SQL_SearchItems_Stock = @"select stock.id,stock.price,stock.discount,stock.printingname,stock.barcode,
                                        stock.qty-(stock.transfered+stock.journalqty+stock.damagedqty+stock.returnqty) stockqty,
                                        (schemelist ->> 'oprice') :: money as oprice,(schemelist ->> 'odiscount') :: money as odiscount,
                                        (schemelist ->> 'nprice') :: money as nprice,(schemelist ->> 'ndiscount') :: money as ndiscount
                                        from pricechange LEFT JOIN LATERAL
                                        json_array_elements (pricechange.itemlist :: json) schemelist ON true
                                        inner join stock ON stock.id = (schemelist ->> 'id') :: BIGINT
                                        where 
                                        pricechange.id = @id and stock.isactive=true";

        const string SQL_SearchItems_Salable = @"select stock.id,stock.price,stock.discount,stock.printingname,stock.barcode,
                                        stock.qty-(stock.transfered+stock.journalqty+stock.damagedqty+stock.returnqty) stockqty,
                                        (schemelist ->> 'oprice') :: money as oprice,(schemelist ->> 'odiscount') :: money as odiscount,
                                        (schemelist ->> 'nprice') :: money as nprice,(schemelist ->> 'ndiscount') :: money as ndiscount
                                        from pricechange LEFT JOIN LATERAL
                                        json_array_elements (pricechange.itemlist :: json) schemelist ON true
                                        inner join salablegoods on salablegoods.id = (schemelist ->> 'id') :: BIGINT
                                        inner join stock ON stock.id = salablegoods.stockid
                                        where 
                                        pricechange.id = @id and salablegoods.isactive=true";

        const string SQL_SalablegoodsPriceHistory = @"select changedon,pricechange.locationid,pricechange.createdby,
                                        (schemelist ->> 'oprice') :: money as oprice,(schemelist ->> 'odiscount') :: money as odiscount,
                                        (schemelist ->> 'nprice') :: money as nprice,(schemelist ->> 'ndiscount') :: money as ndiscount
                                        from pricechange 
										LEFT JOIN LATERAL json_array_elements (pricechange.itemlist :: json) schemelist ON true
                                        inner join salablegoods ON salablegoods.id = (schemelist ->> 'salablegoodsid') :: BIGINT
                                        where pricechange.isactive = true and salablegoods.barcode = @barcode";


        public List<Pricechange.ItemDetail> SearchItems(Discountscheme.FilterCreteria request)
        {

            if (!string.IsNullOrEmpty(InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_barcodeparser))
            {
                if (!string.IsNullOrEmpty(request.barcode))
                {
                    request.barcode = InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.POSParser.UpdateBarcode(request.barcode);
                }
                if (request.barcodes != null)
                {
                    request.barcodes = request.barcodes.ConvertAll(x => InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.POSParser.UpdateBarcode(x));
                }
            }

            if (request.changedfromdate.Year>2000 && request.changedtodate.Year>2000)
            {
                if (request.barcodes == null)
                    request.barcodes = new List<string>();
                var existing = Search(new PricechangeCreteria()
                {
                    fromdate = request.fromdate,
                    todate = request.todate,
                    iswarehouse = request.iswarehouse
                });
                existing.ForEach(x => request.barcodes.AddRange(x.itemlist.ConvertAll(y => y.barcode)));
            }

            List<Pricechange.ItemDetail> lst = new List<Pricechange.ItemDetail>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                NpgsqlCommand command = null;
                QueryBuilder qb = new QueryBuilder();
                bool hasbarcodeSpecific = !string.IsNullOrEmpty(request.barcode) || (request.barcodes != null && request.barcodes.Count>0);
                
                int _index = 1;
                if (request.dosort && request.barcodes != null && request.barcodes.Count > 0)
                {
                    request.indexedbarcodes = new List<Discountscheme.FilterCreteria.BarcodewithIndex>();
                    request.barcodes.ForEach(e =>
                        request.indexedbarcodes.Add(new Discountscheme.FilterCreteria.BarcodewithIndex()
                        {
                            itemindex = _index++,
                            barcode = e
                        })
                    );
                }
                bool includezerostock = InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.analyser_stock_withzerostock;                
                if (request.iswarehouse)
                {
                    qb.CurrentQuery = hasbarcodeSpecific || includezerostock ? SQL_SearchStock.Replace("and (stock.qty-(stock.transfered+stock.journalqty+stock.damagedqty+stock.returnqty)>0)", "") : SQL_SearchStock ;
                    if (!string.IsNullOrEmpty(request.barcode))
                        qb.AddParameter("stock.barcode", "barcode", request.barcode, "=");
                    if (request.supplierid > 0)
                        qb.AddParameter("lrentry.supplierid", "supplierid", request.supplierid, "=");
                    if (request.taxid > 0)
                        qb.AddParameter("products.salestaxid", "salestaxid", request.taxid, "=");
                    if (request.taxpercentage > 0)
                        qb.Add2Query(@"(case when tax.type != 2 then tax.taxpercentage when (salablegoods.price-salablegoods.discount)>1045 then 12 else 5 end)=" + request.taxpercentage);
                }
                else
                {
                    qb.CurrentQuery = hasbarcodeSpecific || includezerostock ? SQL_SearchSalableStock.Replace("and (salablegoods.iqty-(salablegoods.oqty+salablegoods.jqty+salablegoods.dqty)>0)", "") : SQL_SearchSalableStock;
                    if (!string.IsNullOrEmpty(request.barcode))
                        qb.AddParameter("salablegoods.barcode", "barcode", request.barcode, "=");
                    qb.AddParameter("salablegoods.locationid", "locationid", request.locationid, "=");
                    if (request.supplierid > 0)
                        qb.AddParameter("lrentry.supplierid", "supplierid", request.supplierid, "=");
                    if (request.taxid > 0)
                        qb.AddParameter("products.salestaxid", "salestaxid", request.taxid, "=");
                    if (request.taxpercentage > 0)
                        qb.Add2Query(@"(case when tax.type != 2 then tax.taxpercentage when (stock.price-stock.discount)>1045 then 12 else 5 end)=" + request.taxpercentage);
                }
                if (request.productid > 0)
                    qb.AddParameter("items.productid", "productid", request.productid, "=");
                if (request.brandid > 0)
                    qb.AddParameter("items.brandid", "brandid", request.brandid, "=");
                if (request.colourid > 0)
                    qb.AddParameter("items.colourid", "colourid", request.colourid, "=");
                if (request.materialid > 0)
                    qb.AddParameter("items.materialid", "materialid", request.materialid, "=");
                if (request.patternid > 0)
                    qb.AddParameter("items.patternid", "patternid", request.patternid, "=");
                if (request.typeid > 0)
                    qb.AddParameter("items.typeid", "typeid", request.typeid, "=");
                if (request.sizeid > 0)
                    qb.AddParameter("items.sizeid", "sizeid", request.sizeid, "=");
                if (request.styleid > 0)
                    qb.AddParameter("items.styleid", "styleid", request.styleid, "=");
                if (request.pricetagid > 0)
                    qb.AddParameter("stock.pricetagid", request.pricetagid, "=");
                if (!string.IsNullOrEmpty(request.designid))
                    qb.AddParameter("stock.designid", "designid", request.designid, "=");

                if (request.fromdate != DateTime.MinValue && request.todate != DateTime.MinValue)
                {
                    qb.AddParameter("lrinvoice.entrydate", "datefrom", request.fromdate.Date, ">=");
                    qb.AddParameter("lrinvoice.entrydate", "dateto", request.todate.Date.AddDays(1), "<");
                }

                if(request.barcodes != null && request.barcodes.Count>0)
                {
                    qb.AddInParamer("stock.barcode", string.Join(",", request.barcodes.ConvertAll(e => "'" + e + "'")));
                }

                //qb.Limit = 500;
                command = qb.GetCommand(conn);
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Pricechange.ItemDetail odiscountscheme = new Pricechange.ItemDetail();
                        if(request.iswarehouse)
                            odiscountscheme.stockid = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        else
                            odiscountscheme.salablegoodsid = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        odiscountscheme.stock = reader["stock"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["stock"]);
                        odiscountscheme.taxpercentage = reader["taxpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["taxpercentage"]);
                        odiscountscheme.price = reader["price"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["price"]);
                        odiscountscheme.dealerprice = reader["dealerprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["dealerprice"]);
                        odiscountscheme.buyingprice = reader["buyingprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["buyingprice"]);
                        odiscountscheme.ibuyingprice = reader["ibuyingprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["ibuyingprice"]);
                        odiscountscheme.serialno = reader["serialno"] == DBNull.Value ? "" : reader["serialno"].ToString();
                        if (odiscountscheme.ibuyingprice < odiscountscheme.buyingprice)
                            odiscountscheme.ibuyingprice = reader["buyingprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["buyingprice"]);
                        odiscountscheme.mrp = reader["mrp"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["mrp"]);
                        odiscountscheme.discount = reader["discount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discount"]);
                        odiscountscheme.printingname = reader["printingname"] == DBNull.Value ? "" : reader["printingname"].ToString();
                        odiscountscheme.barcode = reader["barcode"] == DBNull.Value ? "" : reader["barcode"].ToString();
                        odiscountscheme.pricetagname = reader["pricetagname"] == DBNull.Value ? "" : reader["pricetagname"].ToString();

                        if (odiscountscheme.ibuyingprice > 0)
                            odiscountscheme.marginpercentage = Math.Round(((odiscountscheme.price - odiscountscheme.discount - odiscountscheme.ibuyingprice) / odiscountscheme.ibuyingprice) * 100, 2);
                        
                        lst.Add(odiscountscheme);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, request);
            }

            if (request.dosort && lst.Count > 0)
            {
                foreach(var item in lst)
                {
                    if(request.indexedbarcodes.Exists(ex => ex.barcode == item.barcode))
                    {
                        item._itemindex = request.indexedbarcodes.Find(ex => ex.barcode == item.barcode).itemindex;
                    }
                }
                //lst.ForEach(e => e._itemindex = request.indexedbarcodes.Find(ex => ex.barcode == e.barcode).itemindex);
                lst = lst.OrderBy(e => e._itemindex).ToList();
            }
            return lst;
        }

        const string SQL_Update = @"UPDATE public.pricechange SET approvedby=@approvedby, reason=@reason, changedon=@changedon, itemlist=@itemlist, modifiedby=@modifiedby, version=@version+1, modifiedon=@modifiedon, companyid=@companyid where id=@id and version=@version RETURNING version";

        const string SQL_Delete = @"update public.pricechange set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";

        public List<Pricechange> Search(PricechangeCreteria _pricechange)
        {
            List<Pricechange> lst = new List<Pricechange>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_pricechange.id != 0)
                {
                    command = GetCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _pricechange.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("isactive", true, "=");

                    if (_pricechange.iswarehouse)
                        qb.AddParameter("iswarehouse", true, "=");
                    else
                        qb.AddParameter("iswarehouse", false, "=");

                    if (_pricechange.fromdate != DateTime.MinValue && _pricechange.todate != DateTime.MinValue)
                    {
                        qb.AddParameter("changedon", "changedonfrom", _pricechange.fromdate.Date, ">=");
                        qb.AddParameter("changedon", "changedonto", _pricechange.todate.AddDays(1).Date, "<");
                    }

                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Pricechange opricechange = new Pricechange();
                        opricechange.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        opricechange.approvedby = reader["approvedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["approvedby"]);
                        opricechange.reason = reader["reason"] == DBNull.Value ? "" : reader["reason"].ToString();
                        opricechange.changedon = reader["changedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["changedon"]);
                        opricechange.itemlist_json = reader["itemlist"] == DBNull.Value ? null : reader["itemlist"].ToString();
                        opricechange.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        opricechange.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        opricechange.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        opricechange.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        opricechange.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        opricechange.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        opricechange.companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"]);
                        opricechange.iswarehouse = reader["iswarehouse"] == DBNull.Value ? false : Convert.ToBoolean(reader["iswarehouse"]);
                        lst.Add(opricechange);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _pricechange);
            }

            if(lst.Count ==1 && _pricechange.id>0)
            {
                lst[0].itemlist = SearchItems(_pricechange.id, lst[0].iswarehouse);
            }
            return lst;
        }

        public class PriceInfo
        {
            public decimal price { get; set; }
            public decimal discount { get; set; }
            public decimal dealerprice { get; set; }
        }
        public Pricechange Insert(PricechangeCreteria oPricechange)
        {
            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = null;
                    NpgsqlCommand taxpricecommand = null;
                    if (oPricechange.changedealerprice)
                    {
                        if(oPricechange.mode == 3)
                        {
                            taxpricecommand = GetCommand(@"update Stock set tax_intermediate_dealerprice=dealerprice where id=@id and tax_intermediate_dealerprice=0", conn, transaction);
                        }
                        command = GetCommand(SQL_UpdateStockDealerPrice, conn, transaction);
                        oPricechange.itemlist.RemoveAll(e => e.stockid == 0);
                    }
                    else
                    {
                        if(!oPricechange.iswarehouse)
                            oPricechange.itemlist.RemoveAll(e => e.salablegoodsid == 0);
                        else
                            oPricechange.itemlist.RemoveAll(e => e.stockid == 0);
                        command = GetCommand(oPricechange.iswarehouse ? SQL_UpdateStock : SQL_UpdateSalableGoods, conn, transaction);
                        if (oPricechange.mode == 3)
                        {
                            if(oPricechange.iswarehouse)
                                taxpricecommand = GetCommand(@"update Stock set tax_intermediate_dealerprice=dealerprice where id=@id and tax_intermediate_dealerprice=0", conn, transaction);
                            else
                                taxpricecommand = GetCommand(@"update salablegoods set tax_intermediate_price=price where id=@id and tax_intermediate_price=0", conn, transaction);
                        }
                    }
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = 0;
                    command.Parameters.Add("price", NpgsqlTypes.NpgsqlDbType.Numeric).Value = 0;
                    command.Parameters.Add("discount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = 0;
                    command.Parameters.Add("dealerprice", NpgsqlTypes.NpgsqlDbType.Numeric).Value = 0;
                    if(taxpricecommand != null)
                        taxpricecommand.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = 0;

                    foreach (var item in oPricechange.itemlist)
                    {
                        if (taxpricecommand != null)
                        {
                            taxpricecommand.Parameters["id"].Value = oPricechange.iswarehouse || oPricechange.changedealerprice ? item.stockid : item.salablegoodsid;
                            int rows = taxpricecommand.ExecuteNonQuery();
                        }

                        command.Parameters["id"].Value = oPricechange.iswarehouse || oPricechange.changedealerprice ? item.stockid : item.salablegoodsid;
                        command.Parameters["price"].Value = item.nprice;
                        command.Parameters["discount"].Value = item.ndiscount;
                        command.Parameters["dealerprice"].Value = item.ndealerprice;
                        using (System.Data.IDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string json = (reader[0].ToString());
                                PriceInfo priceinfo = Newtonsoft.Json.JsonConvert.DeserializeObject<PriceInfo>(json);
                                item.odiscount = priceinfo.discount;
                                item.oprice = priceinfo.price;
                                item.odealerprice = priceinfo.dealerprice;
                            }
                        }
                    }
                    Insert(transaction, conn, oPricechange);

                    if (oPricechange.iswarehouse)
                    {
                        var stockids = oPricechange.itemlist.ConvertAll(x => x.stockid);

                        if (Services.ConfigManager.Instance(Context.OrganizationID).Config.system_syncshowroomprice)
                        {
                            command = GetCommand(@"update salablegoods set price=@price,discount=@discount,modifiedby=@modifiedby,modifiedon=@modifiedon  from stock
                                                            where salablegoods.stockid = stock.id and stock.id = @id;", conn, transaction);
                            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = 0;
                            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
                            command.Parameters.Add("price", NpgsqlTypes.NpgsqlDbType.Numeric).Value = 0;
                            command.Parameters.Add("discount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = 0;
                          
                            foreach (var item in oPricechange.itemlist)
                            {
                                command.Parameters["id"].Value = oPricechange.iswarehouse ? item.stockid : item.salablegoodsid;
                                command.Parameters["price"].Value = item.nprice;
                                command.Parameters["discount"].Value = item.ndiscount;
                                command.ExecuteNonQuery();
                            }
                        }
                        ExecuteQuery(String.Format(@"with computedtax as
                        (
                        select id,type,cast(cast((taxsplit->>'items')::json->0->'sgst' as text) as numeric)+cast(cast((taxsplit->>'items')::json->0->'cgst' as text) as numeric) taxpercentage from tax
                        ),
                        stocktax
                        as
                        (
                        select stock.id,tax.type,
	                        case when tax.type != 2 then tax.taxpercentage when price-discount>1045 then 12 else 5 end taxpercentage from stock
                        inner join products on products.id = stock.productid
                        inner join computedtax tax on tax.id = products.salestaxid
                        where stock.id in ({0})
                        )
                        update stock set eprice = (price-discount)/(100+taxpercentage)*100 from stocktax
                        where stocktax.id = stock.id;",
                        string.Join(",", stockids)), null, conn, transaction);
                    }
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oPricechange);
            }
            return oPricechange;
        }

        public Pricechange Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Pricechange oPricechange)
        {

            NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("approvedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPricechange.approvedby;
            command.Parameters.Add("reason", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oPricechange.reason == null ? "" : oPricechange.reason;
            command.Parameters.Add("changedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("itemlist", NpgsqlTypes.NpgsqlDbType.Jsonb).Value = oPricechange.itemlist_json == null ? "null" : oPricechange.itemlist_json;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oPricechange.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oPricechange.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPricechange.companyid;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPricechange.locationid;
            command.Parameters.Add("iswarehouse", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oPricechange.iswarehouse;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oPricechange.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            return oPricechange;
        }

        public List<Pricechange.ItemDetail> SearchItems(long id,bool iswarehouse)
        {
            List<Pricechange.ItemDetail> lst = new List<Pricechange.ItemDetail>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                NpgsqlCommand command = GetCommand(iswarehouse?SQL_SearchItems_Stock : SQL_SearchItems_Salable, conn);
                command.Parameters.Add("pricechangeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = id;
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Pricechange.ItemDetail odiscountscheme = new Pricechange.ItemDetail();
                        if(iswarehouse)
                            odiscountscheme.stockid = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        else
                            odiscountscheme.salablegoodsid = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        odiscountscheme.price = reader["price"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["price"]);
                        odiscountscheme.discount = reader["discount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discount"]);
                        odiscountscheme.printingname = reader["printingname"] == DBNull.Value ? "" : reader["printingname"].ToString();
                        odiscountscheme.stock = reader["stockqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["stockqty"]);
                        odiscountscheme.nprice = reader["nprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["nprice"]);
                        odiscountscheme.oprice = reader["oprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["oprice"]);
                        odiscountscheme.price = reader["price"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["price"]);
                        odiscountscheme.ndiscount = reader["ndiscount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["ndiscount"]);
                        odiscountscheme.odiscount = reader["odiscount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["odiscount"]);
                        odiscountscheme.discount = reader["discount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discount"]);

                        odiscountscheme.barcode = reader["barcode"] == DBNull.Value ? "" : reader["barcode"].ToString();
                        
                        lst.Add(odiscountscheme);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, id);
            }
            return lst;
        }

        public Pricechange Update(Pricechange oPricechange)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                var search = Search(new PricechangeCreteria()
                {
                    id = oPricechange.id
                });
                List<PricechangeCreteria.ItemDetail> oldItems = null;
                if (search.Count == 1)
                    oldItems = search[0].itemlist;

                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    var removeditems = oPricechange.iswarehouse ? oldItems.FindAll(e => !oPricechange.itemlist.Exists(ex => ex.stockid == e.stockid)) :
                        oldItems.FindAll(e => !oPricechange.itemlist.Exists(ex => ex.salablegoodsid == e.salablegoodsid));
                    NpgsqlCommand command = GetCommand(oPricechange.iswarehouse ? SQL_UpdateStock : SQL_UpdateSalableGoods, conn, transaction);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = 0;
                    command.Parameters.Add("price", NpgsqlTypes.NpgsqlDbType.Numeric).Value = 0;
                    command.Parameters.Add("discount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = 0;
                    
                    foreach (var item in removeditems)
                    {
                        command.Parameters["id"].Value = oPricechange.iswarehouse ? item.stockid : item.salablegoodsid;
                        command.Parameters["price"].Value = item.oprice;
                        command.Parameters["discount"].Value = item.odiscount;
                        command.ExecuteNonQuery();
                    }

                    foreach (var item in oPricechange.itemlist)
                    {
                        command.Parameters["id"].Value = oPricechange.iswarehouse ? item.stockid : item.salablegoodsid;
                        command.Parameters["price"].Value = item.nprice;
                        command.Parameters["discount"].Value = item.ndiscount;
                        using (System.Data.IDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string json = (reader[0].ToString());
                                PriceInfo priceinfo = Newtonsoft.Json.JsonConvert.DeserializeObject<PriceInfo>(json);
                                item.odiscount = priceinfo.discount;
                                item.oprice = priceinfo.price;
                            }
                        }
                    }

                    if (oPricechange.iswarehouse)
                    {
                        var stockids = removeditems.ConvertAll(x => x.stockid);
                        stockids.AddRange(oPricechange.itemlist.ConvertAll(x => x.stockid));
                        
                        if (Services.ConfigManager.Instance(Context.OrganizationID).Config.system_syncshowroomprice)
                        {
                            command = GetCommand(@"update salablegoods set price=@price,discount=@discount,modifiedby=@modifiedby,modifiedon=@modifiedon  from stock
                                                            where salablegoods.stockid = stock.id and stock.id = @id;", conn, transaction);
                            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = 0;
                            command.Parameters.Add("price", NpgsqlTypes.NpgsqlDbType.Numeric).Value = 0;
                            command.Parameters.Add("discount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = 0;
                            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
                            foreach (var item in removeditems)
                            {
                                command.Parameters["id"].Value = oPricechange.iswarehouse ? item.stockid : item.salablegoodsid;
                                command.Parameters["price"].Value = item.oprice;
                                command.Parameters["discount"].Value = item.odiscount;
                                command.ExecuteNonQuery();
                            }

                            foreach (var item in oPricechange.itemlist)
                            {
                                command.Parameters["id"].Value = oPricechange.iswarehouse ? item.stockid : item.salablegoodsid;
                                command.Parameters["price"].Value = item.nprice;
                                command.Parameters["discount"].Value = item.ndiscount;
                                command.ExecuteNonQuery();
                            }
                        }

                        ExecuteQuery(String.Format(@"with computedtax as
                        (
                        select id,type,cast(cast((taxsplit->>'items')::json->0->'sgst' as text) as numeric)+cast(cast((taxsplit->>'items')::json->0->'cgst' as text) as numeric) taxpercentage from tax
                        ),
                        stocktax
                        as
                        (
                        select stock.id,tax.type,
	                        case when tax.type != 2 then tax.taxpercentage when price-discount>1045 then 12 else 5 end taxpercentage from stock
                        inner join products on products.id = stock.productid
                        inner join computedtax tax on tax.id = products.salestaxid
                        where stock.id in ({0})
                        )
                        update stock set eprice = (price-discount)/(100+taxpercentage)*100 from stocktax
                        where stocktax.id = stock.id;",
                        string.Join(",", stockids)), null, conn, transaction);
                    }
                    Update(transaction, conn, oPricechange);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oPricechange);
                
                    
            }

            return oPricechange;
        }

        public Pricechange Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Pricechange oPricechange)
        {

            NpgsqlCommand command = GetCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPricechange.id;
            command.Parameters.Add("approvedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPricechange.approvedby;
            command.Parameters.Add("reason", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oPricechange.reason == null ? "" : oPricechange.reason;
            command.Parameters.Add("changedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("itemlist", NpgsqlTypes.NpgsqlDbType.Jsonb).Value = oPricechange.itemlist_json == null ? "null" : oPricechange.itemlist_json;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oPricechange.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPricechange.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oPricechange.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oPricechange.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPricechange.companyid;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oPricechange.version = Convert.ToInt32(reader[0].ToString());
                }
            }

            return oPricechange;
        }


        public bool Delete(Pricechange oPricechange)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    var price = Search(new PricechangeCreteria()
                    {
                        id = oPricechange.id
                    });

                    if(price.Count == 1)
                    {
                        NpgsqlCommand command = GetCommand(oPricechange.iswarehouse ? SQL_UpdateStock : SQL_UpdateSalableGoods, conn, transaction);
                        command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = 0;
                        command.Parameters.Add("price", NpgsqlTypes.NpgsqlDbType.Numeric).Value = 0;
                        command.Parameters.Add("discount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = 0;

                        foreach (var item in price[0].itemlist)
                        {
                            command.Parameters["id"].Value = oPricechange.iswarehouse ? item.stockid : item.salablegoodsid;
                            command.Parameters["price"].Value = item.oprice;
                            command.Parameters["discount"].Value = item.odiscount;
                            command.ExecuteNonQuery();
                        }
                    }
                    hasSuccessCount = Delete(transaction, conn, oPricechange);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oPricechange);
            }
            return hasSuccessCount;
        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Pricechange oPricechange)
        {

            NpgsqlCommand command = GetCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oPricechange.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oPricechange.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPricechange.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

    }

}
