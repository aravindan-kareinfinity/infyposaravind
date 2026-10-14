using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace InfyPOS.Analytical
{
    public class PurchaseAnalyser : Common.BaseData
    {

        public InsightInfo PurchaseOrder(long orderid)
        {
            
            SortedList<string, object> param = new SortedList<string, object>();
            param.Add("id", orderid);
            var baseitems = Search(poBaseItem, param);
            InsightInfo insightInfo = GetInsightInfo(baseitems.First().supplierid, baseitems.First().date);
            insightInfo.items = BuildResult(baseitems, insightInfo);
            return insightInfo;
        }
        public InsightInfo LrEntry(long lrid)
        {
            SortedList<string, object> param = new SortedList<string, object>();
            param.Add("id", lrid);
            var baseitems = Search(lrBaseItem, param);
            InsightInfo insightInfo = GetInsightInfo(baseitems.First().supplierid, baseitems.First().date);
            insightInfo.items = BuildResult(baseitems, insightInfo);
            return insightInfo;
        }
        public List<PurchaseInsightItem> BuildResult(List<BaseItem> baseitems, InsightInfo insightInfo)
        {
            var products = baseitems.ConvertAll(e => e.productid).Distinct().ToList();
            var brands = baseitems.ConvertAll(e => e.brandid).Distinct().ToList();
            var sellingprice = baseitems.ConvertAll(e => e.sellingprice.ToString("F0") + "").Distinct().ToList();
            var designid = baseitems.ConvertAll(e => "'" + e.designid + "'").Distinct().ToList();
            designid.RemoveAll(e => e == "''");
            sellingprice.RemoveAll(e => e == "0");
            brands.RemoveAll(e => e == 0);

            SortedDictionary<string, string> keyValuePairs = new SortedDictionary<string, string>();
            keyValuePairs.Add("@productid", string.Join(",", products));
            keyValuePairs.Add("@brandid", string.Join(",", brands));
            keyValuePairs.Add("@sellingprice", string.Join(",", sellingprice));
            keyValuePairs.Add("@designid", string.Join(",", designid));
            keyValuePairs.Add("@supplierid", insightInfo.supplierid>0 ? insightInfo.supplierid.ToString("F0") : "");
            
            var querylist = prodanalys.ToList();
            for (var itm = 0; itm < querylist.Count; itm++)
            {
                foreach (var ritm in keyValuePairs)
                {
                    if (querylist[itm].IndexOf(ritm.Key) > 0)
                    {
                        if (string.IsNullOrEmpty(ritm.Value))
                        {
                            querylist[itm] = "";
                        }
                        else
                        {
                            querylist[itm] = querylist[itm].Replace(ritm.Key, ritm.Value);
                        }
                    }
                }
            }

            SortedDictionary<long, string> brandnames = new SortedDictionary<long, string>();
            if (brands.Count > 0)
                brandnames = SearchKeyValue("select id,name from brand where id in (" + keyValuePairs["@brandid"] + ")");
            else
                brandnames.Add(0, "");

            SortedDictionary<long, string> productnames = SearchKeyValue("select id,name from products where id in (" + keyValuePairs["@productid"] + ")");
            var results = SearchValues(string.Join("\r\n", querylist));

            var fullresult = new List<PurchaseInsightItem>();
            foreach (var item in baseitems)
            {
                PurchaseInsightItem purchaseInsightItem = new PurchaseInsightItem();
                fullresult.Add(purchaseInsightItem);
                purchaseInsightItem.brand = brandnames[item.brandid];
                purchaseInsightItem.product = productnames[item.productid];
                purchaseInsightItem.SellingPrice = item.sellingprice;
                purchaseInsightItem.BuyingPrice = item.buyingprice;
                purchaseInsightItem.BuyingQty = item.buyingqty;
                purchaseInsightItem.design = item.designid;

                purchaseInsightItem.BrandData = new PurchaseInsightItem.Detail();
                purchaseInsightItem.BrandData.stock = results.Sum(e =>
                e.type == "stock" &&
                e.role == "brand" &&
                e.productid == item.productid &&
                e.brandid == item.brandid ? e.qty : 0);

                purchaseInsightItem.BrandData.stockvalue = results.Sum(e =>
                e.type == "stock" &&
                e.role == "brand" &&
                e.productid == item.productid &&
                e.brandid == item.brandid ? e.value : 0);

                purchaseInsightItem.BrandData.stock90 = results.Sum(e =>
                e.type == "stock" &&
                e.role == "brand" &&
                e.productid == item.productid &&
                e.brandid == item.brandid ? e.stock90 : 0);

                purchaseInsightItem.BrandData.sales = results.Sum(e =>
                e.type == "sales" &&
                e.role == "brand" &&
                e.productid == item.productid &&
                e.brandid == item.brandid ? e.value : 0);

                purchaseInsightItem.ProductData = new PurchaseInsightItem.Detail();

                purchaseInsightItem.ProductData.stock = results.Sum(e =>
                e.type == "stock" &&
                e.role == "product" &&
                e.productid == item.productid ? e.qty : 0);

                purchaseInsightItem.ProductData.stockvalue = results.Sum(e =>
                e.type == "stock" &&
                e.role == "product" &&
                e.productid == item.productid ? e.value : 0);

                purchaseInsightItem.ProductData.stock90 = results.Sum(e =>
                e.type == "stock" &&
                e.role == "product" &&
                e.productid == item.productid ? e.stock90 : 0);

                purchaseInsightItem.ProductData.sales = results.Sum(e =>
                e.type == "sales" &&
                e.role == "product" &&
                e.productid == item.productid ? e.value : 0);


                purchaseInsightItem.SalesPriceData = new PurchaseInsightItem.Detail();

                purchaseInsightItem.SalesPriceData.stock = results.Sum(e =>
                e.type == "stock" &&
                e.role == "sellingprice" &&
                e.productid == item.productid &&
                e.sellingprice == item.sellingprice ? e.qty : 0);

                purchaseInsightItem.SalesPriceData.stockvalue = results.Sum(e =>
                e.type == "stock" &&
                e.role == "sellingprice" &&
                e.productid == item.productid &&
                e.sellingprice == item.sellingprice ? e.value : 0);

                purchaseInsightItem.SalesPriceData.stock90 = results.Sum(e =>
                e.type == "stock" &&
                e.role == "sellingprice" &&
                e.productid == item.productid &&
                e.sellingprice == item.sellingprice ? e.stock90 : 0);

                purchaseInsightItem.SalesPriceData.sales = results.Sum(e =>
                e.type == "sales" &&
                e.role == "sellingprice" &&
                e.productid == item.productid &&
                e.sellingprice == item.sellingprice ? e.value : 0);


                purchaseInsightItem.DesignData = new PurchaseInsightItem.Detail();
                purchaseInsightItem.DesignData.stock = results.Sum(e =>
                e.type == "stock" &&
                e.role == "designid" &&
                e.productid == item.productid &&
                e.designid == item.designid ? e.qty : 0);

                purchaseInsightItem.DesignData.stockvalue = results.Sum(e =>
                e.type == "stock" &&
                e.role == "designid" &&
                e.productid == item.productid &&
                e.designid == item.designid ? e.value : 0);

                purchaseInsightItem.DesignData.stock90 = results.Sum(e =>
                e.type == "stock" &&
                e.role == "designid" &&
                e.productid == item.productid &&
                e.designid == item.designid ? e.stock90 : 0);

                purchaseInsightItem.DesignData.sales = results.Sum(e =>
                e.type == "sales" &&
                e.role == "designid" &&
                e.productid == item.productid &&
                e.designid == item.designid ? e.value : 0);


                purchaseInsightItem.SupplierData = new PurchaseInsightItem.Detail();
                purchaseInsightItem.SupplierData.stock = results.Sum(e =>
                e.type == "stock" &&
                e.role == "supplier"? e.qty : 0);

                purchaseInsightItem.SupplierData.stockvalue = results.Sum(e =>
                e.type == "stock" &&
                e.role == "supplier"? e.value : 0);

                purchaseInsightItem.SupplierData.stock90 = results.Sum(e =>
                e.type == "stock" &&
                e.role == "supplier"? e.stock90 : 0);

                purchaseInsightItem.SupplierData.sales = results.Sum(e =>
                e.type == "sales" &&
                e.role == "supplier"? e.value : 0);

            }
            return fullresult;
        }


        string[] prodanalys = new string[] { @"select 'stock' as type,
'product' as role,0 as sellingprice,'' designid,0::bigint brandid,
sum(salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty)) as qty,  
stock.productid as productid,
sum(((salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty))*stock.ibuyingprice::numeric) as value,
sum(case when EXTRACT(day from @current_date - stock.stockdate)<91 then salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty) else 0 end) as stock90
from salablegoods
inner join stock on stock.id = stockid and stock.productid in (@productid)
where salablegoods.hasstock = true and salablegoods.isactive = true
group by stock.productid;",

@"select 'stock' as type,
'sellingprice' as role,'' designid,0::bigint brandid,
sum(salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty)) as qty,  stock.productid,
sum(((salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty))*stock.ibuyingprice::numeric) as value,salablegoods.price as sellingprice,
sum(case when EXTRACT(day from @current_date - stock.stockdate)<91 then salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty) else 0 end) as stock90
from salablegoods
inner join stock on stock.id = stockid and stock.productid in (@productid)
where salablegoods.hasstock = true and salablegoods.isactive = true and salablegoods.price in (@sellingprice)
group by stock.productid,salablegoods.price;",

@"select 'stock' as type,
'brand' as role,0 as sellingprice,'' designid,
sum(salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty)) as qty, stock.productid,items.brandid,
sum((salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty))*stock.ibuyingprice::numeric) as value,
sum(case when EXTRACT(day from @current_date - stock.stockdate)<91 then salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty) else 0 end) as stock90
from salablegoods
inner join stock on stock.id = stockid and stock.productid in (@productid)
inner join items on items.id = stock.itemid and items.brandid in (@brandid)
where salablegoods.hasstock = true and salablegoods.isactive = true
group by stock.productid,items.brandid;",

@"select 'stock' as type,
'designid' as role,0 as sellingprice,stock.designid,0::bigint brandid,
sum(salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty)) as qty, stock.productid,
sum((salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty))*stock.ibuyingprice::numeric) as value,
sum(case when EXTRACT(day from @current_date - stock.stockdate)<91 then salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty) else 0 end) as stock90
from salablegoods
inner join stock on stock.id = stockid and stock.productid in (@productid)
where salablegoods.hasstock = true and salablegoods.isactive = true and stock.designid in (@designid)
group by stock.productid,stock.designid;",


@"select 'stock' as type,
'supplier' as role,0 as sellingprice,'' as designid,0::bigint brandid,
sum(salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty)) as qty, stock.productid,
sum((salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty))*stock.ibuyingprice::numeric) as value,
sum(case when EXTRACT(day from @current_date - stock.stockdate)<91 then salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty) else 0 end) as stock90
from salablegoods
inner join stock on stock.id = stockid and stock.productid in (@productid)
where salablegoods.hasstock = true and salablegoods.isactive = true and stock.supplierid = @supplierid
group by stock.productid;",

@"select  'sale' as type,
'product' as role,0 as sellingprice,'' as designid,0::bigint brandid,0 as stock90,
sum(billitems.qty) as qty,stock.productid, 
sum(billitems.qty * stock.ibuyingprice::numeric) as value
from billitems
inner join bill on bill.id = billid
inner join salablegoods on salablegoods.id = salablegoodsid
inner join stock on stock.id = salablegoods.stockid and stock.productid in (@productid)
where EXTRACT(day from @current_date - bill.billdate)<91
group by stock.productid;",

@"select  'sellingprice' as type,
'product' as role,'' as designid,0::bigint brandid,
sum(billitems.qty) as qty,stock.productid, salablegoods.price as sellingprice,0 as stock90,
sum(billitems.qty * stock.ibuyingprice::numeric) as value
from billitems
inner join bill on bill.id = billid
inner join salablegoods on salablegoods.id = salablegoodsid
inner join stock on stock.id = salablegoods.stockid and stock.productid in (@productid)
where EXTRACT(day from @current_date - bill.billdate)<91 and salablegoods.price in (@sellingprice)
group by stock.productid,salablegoods.price;",

        @"select  'sale' as type,
'brand' as role,0 as sellingprice,'' as designid,
sum(billitems.qty) as qty, stock.productid,items.brandid,0 as stock90,
sum(billitems.qty * stock.ibuyingprice::numeric) as value
from billitems
inner join bill on bill.id = billid
inner join salablegoods on salablegoods.id = salablegoodsid
inner join stock on stock.id = salablegoods.stockid and stock.productid in (@productid)
inner join items on items.id = stock.itemid and items.brandid in (@brandid)
where EXTRACT(day from @current_date - bill.billdate)<91
group by stock.productid,items.brandid;",

        @"select  'sale' as type,stock.productid,
'designid' as role,0 as sellingprice,stock.designid,0::bigint brandid,
sum(billitems.qty) as qty, 0 as stock90,
sum(billitems.qty * stock.ibuyingprice::numeric) as value
from billitems
inner join bill on bill.id = billid
inner join salablegoods on salablegoods.id = salablegoodsid
inner join stock on stock.id = salablegoods.stockid and stock.productid in (@productid)
where EXTRACT(day from @current_date - bill.billdate)<91 and stock.designid in (@designid)
group by stock.productid,stock.designid;",

        @"select  'sale' as type,stock.productid,
'supplier' as role,0 as sellingprice,'' as designid,0::bigint brandid,
sum(billitems.qty) as qty, 0 as stock90,
sum(billitems.qty * stock.ibuyingprice::numeric) as value
from billitems
inner join bill on bill.id = billid
inner join salablegoods on salablegoods.id = salablegoodsid
inner join stock on stock.id = salablegoods.stockid and stock.productid in (@productid)
where EXTRACT(day from @current_date - bill.billdate)<91 and stock.supplierid = @supplierid
group by stock.productid;"};


        string poBaseItem = @"select purchaseorderitems.id,purchaseorderitems.productid,0::bigint ibrandid,
                            items.brandid,brand.name as brandname,
                            designid,oqty buyingqty,price buyingprice,
                            purchaseorder.supplierid,purchaseorder.podate date,
                            0 as sellingprice from purchaseorderitems 
                            inner join purchaseorder on purchaseorder.id = purchaseorderid
                            left outer join items on items.id = itemid
                            left outer join brand on brand.id = items.brandid
                            where purchaseorderitems.purchaseorderid=@id;";

        string lrBaseItem = @"select lrinvoiceitems.id,lrinvoiceitems.productid,
                            cast(itemdetail->>'brandid' as bigint) ibrandid,items.brandid,
                            designid,lrinvoiceitems.buyingqty,lrinvoiceitems.buyingprice,
                            sellingprice,COALESCE(ibrand.name,lbrand.name) as brandname,
                            lrentry.supplierid,lrinvoice.invoicedate date
                            from lrinvoiceitems 
                            inner join lrinvoice on lrinvoice.id = lrinvoiceid
                            inner join lrentry on lrentry.id = lrid
                            left outer join items on items.id = itemid
                            left outer join brand ibrand on ibrand.id = items.brandid
                            left outer join brand lbrand on lbrand.id = cast(itemdetail->>'brandid' as bigint)
                            where lrid = @id";

        string lrinfo = @"select supplier.name,lrinvoice.invoiceno,lrinvoice.invoicedate from lrinvoice
                            inner join supplier on supplier.id = lrsupplierid
                            where lrinvoice.invoicedate<@invoicedate and lrinvoice.lrsupplierid = @supplierid
                            order by lrinvoice.id desc 
                            limit 1";

        public InsightInfo GetInsightInfo(long supplier,DateTime invoicedate)
        {
            InsightInfo insightInfo = new InsightInfo() { supplierid = supplier };
            var basedata = new Common.BaseData() { Context = this.Context };
            var param = new SortedList<string, object>();
            param.Add("supplierid", supplier);
            param.Add("invoicedate", invoicedate);
            param.Add("current_date", basedata.DateTimeNow.Date);
            
            using (var reader = basedata.ExecuteReader(lrinfo, param))
            {
                if (reader.Read())
                {
                    insightInfo.supplier = reader.IsDBNull(0) ? "" : reader.GetString(0);
                    insightInfo.lastinvoiceno = reader.IsDBNull(1) ? "" : reader.GetString(1);
                    insightInfo.lastpurchase = reader.IsDBNull(2) ? DateTime.MinValue : reader.GetDateTime(2);
                }
            }
            return insightInfo;
        }
        public enum ResultType
        {
            Product,
            Brand,
            Design,
            Price
        }

        public SortedDictionary<long, string> SearchKeyValue(string query)
        {
            var result = new SortedDictionary<long, string>();
            var basedata = new Common.BaseData() { Context = this.Context };
            using (var reader = basedata.ExecuteReader(query, null))
            {
                while (reader.Read())
                {
                    result.Add(reader.GetInt64(0), reader.IsDBNull(1) ? "" : reader.GetString(1));
                }
            }
            return result;
        }

        public List<StockItem> SearchValues(string query)
        {
            var result = new List<StockItem>();
            var basedata = new Common.BaseData() { Context = this.Context };
            using (var reader = basedata.ExecuteReader(query, null))
            {
                do
                {
                    while (reader.Read())
                    {
                        var bitem = new StockItem();
                        bitem.value = reader["value"] != DBNull.Value ? Convert.ToDecimal(reader["value"]) : 0;
                        bitem.qty = reader["qty"] != DBNull.Value ? Convert.ToDecimal(reader["qty"]) : 0;
                        bitem.stock90 = reader["stock90"] != DBNull.Value ? Convert.ToDecimal(reader["stock90"]) : 0;
                        bitem.sellingprice = reader["sellingprice"] != DBNull.Value ? Convert.ToDecimal(reader["sellingprice"]) : 0;
                        bitem.productid = reader["productid"] != DBNull.Value ? Convert.ToInt64(reader["productid"]) : 0;
                        bitem.brandid = reader["brandid"] != DBNull.Value ? Convert.ToInt64(reader["brandid"]) : 0;
                        bitem.designid = reader["designid"] != DBNull.Value ? Convert.ToString(reader["designid"]) : "";
                        bitem.type = reader["type"] != DBNull.Value ? Convert.ToString(reader["type"]) : "";
                        bitem.role = reader["role"] != DBNull.Value ? Convert.ToString(reader["role"]) : "";

                        result.Add(bitem);
                    }
                } while (reader.NextResult());
            }
            return result;
        }
        public List<BaseItem> Search(string query, SortedList<string, object> param)
        {
            var basedata = new Common.BaseData() { Context = this.Context };
            if (!param.ContainsKey("current_date"))
                param.Add("current_date", basedata.DateTimeNow.Date);
            var result = new List<BaseItem>();
            
            using (var reader = basedata.ExecuteReader(query, param))
            {
                while (reader.Read())
                {
                    var bitem = new BaseItem();
                    bitem.id = reader["id"] != DBNull.Value ? Convert.ToInt64(reader["id"]) : 0;

                    bitem.supplierid = reader["supplierid"] != DBNull.Value ? Convert.ToInt64(reader["supplierid"]) : 0;
                    bitem.date = reader["date"] != DBNull.Value ? Convert.ToDateTime(reader["date"]) : DateTime.MinValue;

                    bitem.buyingqty = reader["buyingqty"] != DBNull.Value ? Convert.ToDecimal(reader["buyingqty"]) : 0;
                    bitem.sellingprice = reader["sellingprice"] != DBNull.Value ? Convert.ToDecimal(reader["sellingprice"]) : 0;
                    bitem.buyingprice = reader["buyingprice"] != DBNull.Value ? Convert.ToDecimal(reader["buyingprice"]) : 0;
                    bitem.designid = reader["designid"] != DBNull.Value ? Convert.ToString(reader["designid"]) : "";
                    bitem.brandname = reader["brandname"] != DBNull.Value ? Convert.ToString(reader["brandname"]) : "";
                    bitem.productid = reader["productid"] != DBNull.Value ? Convert.ToInt64(reader["productid"]) : 0;
                    bitem.brandid = reader["brandid"] != DBNull.Value ? Convert.ToInt64(reader["brandid"]) : 0;
                    if (bitem.brandid == 0)
                        bitem.brandid = reader["ibrandid"] != DBNull.Value ? Convert.ToInt64(reader["ibrandid"]) : 0;
                    result.Add(bitem);

                    if(bitem.brandname.IndexOfAny("0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz".ToCharArray())<0)
                    {
                        bitem.brandname = "";
                    }
                    if (bitem.designid.IndexOfAny("0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz".ToCharArray()) < 0)
                    {
                        bitem.designid = "";
                    }
                }
            }
            var fresult = new List<BaseItem>();
            foreach (var gitem in result.GroupBy(e=>new {e.brandid,e.brandname,e.buyingprice,e.designid,e.productid,e.sellingprice}))
            {
                fresult.Add(new BaseItem()
                {
                    brandid = gitem.Key.brandid,
                    brandname = gitem.Key.brandname,
                    buyingprice = gitem.Key.buyingprice,
                    sellingprice = gitem.Key.sellingprice,
                    productid = gitem.Key.productid,
                    designid = gitem.Key.designid,
                    buyingqty = gitem.Sum(e => e.buyingqty),
                    supplierid = gitem.First().supplierid,
                    date = gitem.First().date
                });
            }
            return fresult;
        }
        public class BaseItem
        {
            public long id { get; set; }
            public long productid { get; set; }
            public long brandid { get; set; }
            public string designid { get; set; }
            public decimal buyingqty { get; set; }
            public decimal sellingprice { get; set; }
            public decimal buyingprice { get;  set; }
            public string brandname { get;  set; }
            public long supplierid { get;  set; }
            public DateTime date { get;  set; }
        }

        public class StockItem
        {
            public long productid { get; set; }
            public long brandid { get; set; }
            public string designid { get; set; }
            public string type { get; set; }
            public string role { get; set; }
            public decimal sellingprice { get; set; }
            public decimal qty { get; set; }
            public decimal value { get; set; }
            public decimal stock90 { get; set; }
        }

        public class InsightInfo
        {
            public long supplierid { get; set; }
            public string supplier { get; set; }
            public DateTime lastpurchase { get; set; }
            public string lastinvoiceno { get; set; }

            public List<PurchaseInsightItem> items { get; set; }
        }
        public class PurchaseInsightItem
        {
            public decimal BuyingQty { get; set; }
            public decimal BuyingPrice { get; set; }
            public decimal SellingPrice { get; set; }
            public string product { get; set; }
            public string brand { get; set; }
            public string design { get; set; }
            public Detail ProductData { get; set; }
            public Detail BrandData { get; set; }
            public Detail DesignData { get; set; }
            public Detail SalesPriceData { get; set; }

            public Detail SupplierData { get; set; }

            public class Detail
            {
                public decimal stock { get; set; }
                public decimal stock90 { get; set; }
                public decimal sales { get; set; }
                public decimal stockvalue { get; set; }
            }

        }
    }
}