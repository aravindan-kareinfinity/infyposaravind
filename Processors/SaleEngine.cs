using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;

namespace InfyPOS.Processors
{
    /*select billitems.*
into UNLOGGED archieve_billitems
from billitems
where  billid<1000000

select bill.*
into UNLOGGED archieve_bill
from bill
where  billid<1000000

select billsettlement.*
into UNLOGGED archieve_billsettlement
from billsettlement
where  billid<1000000

select billsettlementmaster.*
into UNLOGGED archieve_billsettlementmaster
from billsettlementmaster
where id<=663953


select stock.*
into UNLOGGED archieve_stock
from stock
left outer join salablegoods on salablegoods.stockid = stock.id 
and salablegoods.hasstock=true and stockid<500000
left outer join billitems on billitems.salablegoodsid = salablegoods.id
where stock.id<500000
and salablegoods.id is null and billitems.id is null

select salablegoods.*
into UNLOGGED archieve_salablegoods
from salablegoods
inner join archieve_stock on archieve_stock.id =salablegoods.stockid


delete from salablegoods where id in (select id from archieve_salablegoods)

delete from stock where id in (select id from archieve_stock)*/
    public class SaleEngine : InfyPOS.Common.BaseData
    {
        public string sql_sale = @"select stock.supplierid,items.brandid,items.sizeid,items.colourid,items.styleid,items.patternid,
                                ceiling(EXTRACT(day from bill.billdate - stock.stockdate)/@days) days,
                                sum(billitems.receivable) value,sum(billitems.qty) qty from billitems 
                                inner join bill on bill.id = billitems.id
                                inner join salablegoods on salablegoods.id = billitems.salablegoodsid
                                inner join stock on stock.id = salablegoods.stockid
                                inner join products on products.id = stock.productid
                                inner join items on items.id = stock.itemid
                                where bill.billdate>=@fromdate and bill.billdate<@todate {0}
                                group by stock.supplierid,stock.productid,items.brandid,items.sizeid,items.colourid,items.styleid,items.patternid,ceiling(EXTRACT(day from bill.billdate - stock.stockdate)/@days)";

        public string sql_stock = @"select stock.supplierid,items.brandid,items.sizeid,items.colourid,items.styleid,items.patternid,ceiling(EXTRACT(day from current_date - stock.stockdate)/@days) days,
                                sum(stock.buyingprice::numeric*(salablegoods.iqty-(salablegoods.dqty+salablegoods.jqty+salablegoods.oqty))) value,sum(salablegoods.iqty-(salablegoods.dqty+salablegoods.jqty+salablegoods.oqty)) qty from salablegoods 
                                inner join stock on stock.id = salablegoods.stockid
                                inner join products on products.id = stock.productid
                                inner join items on items.id = stock.itemid
                                where salablegoods.iqty-(salablegoods.dqty+salablegoods.jqty+salablegoods.oqty)>0 and stock.stockdate<@todate {1}
                                group by stock.supplierid,stock.productid,items.brandid,items.sizeid,items.colourid,items.styleid,items.patternid,ceiling(EXTRACT(day from current_date- stock.stockdate)/@days)";

        public class Rule
        {
            public long productid { get; set; }
            public long productgroupid { get; set; }
            public long sectionid { get; set; }
            public int days { get; set; }
            public DateTime fromdate { get; set; }
            public DateTime todate { get; set; }
        }

        public class SaleData
        {
            public long supplierid;
            public long brandid;
            public long sizeid;
            public long colourid;
            public long styleid;
            public long patternid;
            public int days;
            public decimal value;
            public decimal qty;
        }

        public class ResultData
        {
            public long primaryid { get; set; }
            public string display { get; set; }
            public decimal stockvalue { get; set; }
            public decimal stockqty { get; set; }
            public decimal salevalue { get; set; }
            public decimal saleqty { get; set; }
            public int days { get; set; }
            public decimal stockratio { get;  set; }
        }

        public List<ResultData> Analys(Rule rule)
        {
            var parma = new SortedList<string, object>();
            parma.Add("days", rule.days);
            parma.Add("fromdate", rule.fromdate);
            parma.Add("todate", rule.todate.AddDays(1));
            System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();
            if (rule.productgroupid > 0)
            {
                stringBuilder.Append(" and products.productgroupid=@productgroupid");
                parma.Add("productgroupid", rule.productgroupid);
            }
            if (rule.productid > 0)
            {
                stringBuilder.Append(" and products.id=@productid");
                parma.Add("productid", rule.productid);
            }
            if (rule.sectionid > 0)
            {
                stringBuilder.Append(" and products.sectionid=@sectionid");
                parma.Add("sectionid", rule.sectionid);
            }
            var resultset = ExecuteDataSet(string.Format(sql_sale, stringBuilder.ToString()), parma);

            List<SaleData> QuantoSales = (from row in resultset.Tables[0].AsEnumerable()
                                             select new SaleData
                                             {
                                                 supplierid = row.Field<long>("supplierid"),
                                                 brandid = row.Field<long>("brandid"),
                                                 sizeid = row.Field<long>("sizeid"),
                                                 colourid = row.Field<long>("colourid"),
                                                 styleid = row.Field<long>("styleid"),
                                                 patternid = row.Field<long>("patternid"),
                                                 value = row.Field<decimal>("value"),
                                                 qty = row.Field<decimal>("qty"),
                                                 days = row.Field<int>("days")
                                             }).ToList();


            List<SaleData> QuantoStock = (from row in resultset.Tables[0].AsEnumerable()
                                          select new SaleData
                                          {
                                              supplierid = row.Field<long>("supplierid"),
                                              brandid = row.Field<long>("brandid"),
                                              sizeid = row.Field<long>("sizeid"),
                                              colourid = row.Field<long>("colourid"),
                                              styleid = row.Field<long>("styleid"),
                                              patternid = row.Field<long>("patternid"),
                                              value = row.Field<decimal>("value"),
                                              qty = row.Field<decimal>("qty"),
                                              days = row.Field<int>("days")
                                          }).ToList();

            var stockbysupplier = QuantoStock
             .GroupBy(l => new { l.supplierid, l.days })
             .Select(cl => new ResultData
             {
                 primaryid = cl.Key.supplierid,
                 days = cl.Key.days,
                 stockqty = cl.Sum(e=>e.qty),
                 stockvalue = cl.Sum(e => e.value)
             }).ToList();

            var salesbysupplier = QuantoSales
             .GroupBy(l => new { l.supplierid, l.days })
             .Select(cl => new ResultData
             {
                 primaryid = cl.Key.supplierid,
                 days = cl.Key.days,
                 saleqty = cl.Sum(e => e.qty),
                 salevalue = cl.Sum(e => e.value)
             }).ToList();

            stockbysupplier.ToList().ForEach(e =>
            {
                var index = salesbysupplier.FindIndex(x => x.primaryid == e.primaryid && x.days == e.days);
                if (index < 0)
                {
                    e.stockratio = 100;
                    stockbysupplier.Add(e);
                }
                else
                {
                    stockbysupplier[index].saleqty = e.saleqty;
                    stockbysupplier[index].salevalue = e.salevalue;
                    stockbysupplier[index].stockratio = (salesbysupplier[index].stockvalue + salesbysupplier[index].salevalue / e.salevalue) * 100;
                }
            });

            List<ResultData> resultDatas = new List<ResultData>();
            return resultDatas;
        }
    }
}
