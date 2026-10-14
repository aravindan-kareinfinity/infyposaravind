using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace InfyPOS.Analytical
{
    public class AnalyticalCubesOLD : Common.BaseData
    {
        public string Query_MonthlySales = @"select stock.productid,stock.supplierid,sum(billitems.receivable) as amount,
        sum(billitems.qty) as qty,sum(stock.ibuyingprice::numeric*billitems.qty::numeric) as cost,
EXTRACT(month from billdate) as quarter,EXTRACT(year from billdate) as year 
from bill inner join billitems on billitems.billid = bill.id and billitems.isactive=true
inner join stock on stock.id = stockid 
where bill.billtype != 9 and bill.isactive = true and bill.billdate<'2020-02-01' 
group by EXTRACT(month from billdate),EXTRACT(year from billdate),stock.productid,stock.supplierid";
        public AnalyticalCubesOLD()
        {

        }

        public List<AnalyticalCubes.ProductInsightItem> ProductSummary()
        {
            string query = @"/*select matviewname from pg_matviews 
select * from mv_stock_monthly_prd limit 10
select * from mv_sales_monthly_prd limit 10
select * from mv_stock_prd_sup limit 10
select * from mv_sales_prd_sup limit 10
*/

with prdstock
as
(
select sum(sp) stkcogs,sum(cp) stkcog,
sum(case when MV_Stock_Prd_Sup.period ='30 - 60 days' then 2*MV_Stock_Prd_Sup.cp/100
when MV_Stock_Prd_Sup.period ='60 - 90 days' then 3*MV_Stock_Prd_Sup.cp/100
when MV_Stock_Prd_Sup.period ='90 - 120 days' then 4*MV_Stock_Prd_Sup.cp/100
when MV_Stock_Prd_Sup.period ='120 - 150 days' then 5*MV_Stock_Prd_Sup.cp/100
when MV_Stock_Prd_Sup.period ='150 - 180 days' then 6*MV_Stock_Prd_Sup.cp/100
when MV_Stock_Prd_Sup.period ='above 180 days' then 7*MV_Stock_Prd_Sup.cp/100
else MV_Stock_Prd_Sup.cp/100 end) stkloss,
	productid from mv_stock_prd_sup
group by productid
),prdsale
as
(
select sum(cogs) salcogs,sum(cog) salcog,
sum(case 
when mv_sales_prd_sup.period ='30 - 60 days' then 2*mv_sales_prd_sup.cog/100
when mv_sales_prd_sup.period ='60 - 90 days' then 3*mv_sales_prd_sup.cog/100
when mv_sales_prd_sup.period ='90 - 120 days' then 4*mv_sales_prd_sup.cog/100
when mv_sales_prd_sup.period ='120 - 150 days' then 5*mv_sales_prd_sup.cog/100
when mv_sales_prd_sup.period ='150 - 180 days' then 6*mv_sales_prd_sup.cog/100
when mv_sales_prd_sup.period ='above 180 days' then 7*mv_sales_prd_sup.cog/100
else mv_sales_prd_sup.cog/100 end) salloss,
productid from mv_sales_prd_sup
group by productid
),trend as
(
	select coalesce(prdstock.productid,prdsale.productid) productid,
	prdstock.stkcogs,stkcog,salcogs,salcog,salloss,stkloss
	from prdstock	
 full outer join prdsale on prdsale.productid =  prdstock.productid
)
select products.name,trend.* from trend
inner join products on products.id = trend.productid";


            var table = ExecuteDataSet(query, null).Tables[0];
            var lst = table.AsEnumerable().Select(x => new AnalyticalCubes.ProductInsightItem()
            {
                exmarginpercentage = 0,
                totalstock = 0,
                profit_margin = 0,
                salstockratio = 0,
                marginpercentage = 0,

                sale_loss = x["salloss"] == DBNull.Value ? 0 : Convert.ToDecimal(x["salloss"]),
                stock_loss = x["stkloss"] == DBNull.Value ? 0 : Convert.ToDecimal(x["stkloss"]),
                sold_cost = x["salcog"] == DBNull.Value ? 0 : Convert.ToDecimal(x["salcog"]),
                sold_value = x["salcogs"] == DBNull.Value ? 0 : Convert.ToDecimal(x["salcogs"]),
                stock_cost = x["stkcog"] == DBNull.Value ? 0 : Convert.ToDecimal(x["stkcog"]),
                stock_value = x["stkcogs"] == DBNull.Value ? 0 : Convert.ToDecimal(x["stkcogs"]),
                name = Convert.ToString(x["name"]),
                id = Convert.ToInt64(x["productid"])
            });
            return lst.ToList();
        }
        public AnalyticalCubes.ProductInsight Product(long id, DateTime fromdate, DateTime todate)
        {
            AnalyticalCubes.ProductInsight productInsight = new AnalyticalCubes.ProductInsight();
            var param = new SortedList<string, object>();
            param.Add("fromdate", fromdate);
            param.Add("todate", todate);
            param.Add("productid", id);
            var table = ExecuteDataSet(@"select sum(cost) as cog,month,year from mv_stock_monthly_prd where productid=@productid
            and stockdate>=@fromdate and stockdate<@todate", param).Tables[0];
            productInsight.MonthlyPurchase = table.AsEnumerable().Select(x => new AnalyticalCubes.ProductInsight.monthly()
            {
                cog = Convert.ToDecimal(x["cog"]),
                month = Convert.ToInt32(x["month"]),
                year = Convert.ToInt32(x["year"])
            }).ToList();


            table = ExecuteDataSet(@"select month,year,sum(cog) cog,sum(cogs) cogs,sum(qty) qty from mv_sales_monthly_prd  where productid=3
                    and cast(year||'-'||month||'-01' as timestamp)>=@fromdate and cast(year||'-'||month||'-01' as timestamp)<@todate
                    group by month,year", param).Tables[0];
            productInsight.MonthlySales = table.AsEnumerable().Select(x => new AnalyticalCubes.ProductInsight.monthly()
            {
                qty = Convert.ToDecimal(x["qty"]),
                cog = Convert.ToDecimal(x["cog"]),
                cogs = Convert.ToDecimal(x["cogs"]),
                month = Convert.ToInt32(x["month"]),
                year = Convert.ToInt32(x["year"])
            }).ToList();

            return productInsight;
        }


        static System.Threading.AutoResetEvent autoResetEvent = new System.Threading.AutoResetEvent(true);
        public void Refresh()
        {
            if (autoResetEvent.WaitOne())
            {
                if (NeedRefresh())
                {
                    ExecuteQuery("REFRESH MATERIALIZED VIEW MonthlySales", null);
                }
                autoResetEvent.Set();
            }
        }

        public bool NeedRefresh()
        {
            var param = new SortedList<string, object>();
            string existquery = "select refereshed from monthlysales refereshed limit 1";
            DateTime currentbegin = DateTime.MinValue;
            using (var reader = ExecuteReader(existquery, param))
            {
                if (reader.Read())
                {
                    currentbegin = reader.GetDateTime(0);
                }
                reader.Close();
            }
            return DateTimeNow.Date != currentbegin.Date;
        }

        public List<MobileInsight.Insight.SalesItem> GetProductCurrentYearSales(long productid)
        {
            if (!NeedRefresh())
            {
                Refresh();
            }
            var param = new SortedList<string, object>();
            var dateend = GetQuarterLast(DateTimeNow.Date).AddDays(1);
            var datebegin = dateend.AddYears(-1);
            param.Add("datebegin", datebegin);
            param.Add("dateend", dateend.AddDays(1));
            param.Add("productid", productid);
            var list = new List<MobileInsight.Insight.SalesItem>();
            using (var reader = ExecuteReader(@"Select productid,year,quarter,sum(amount) as amount,sum(qty) as qty,sum(cost) as cost from MonthlySales
                                where datebegin >= @datebegin and datebegin < @dateend and productid=@productid
                                group by productid, year, quarter", param))
            {
                while (reader.Read())
                {
                    list.Add(
                    new MobileInsight.Insight.SalesItem()
                    {

                        Amount = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"]),
                        Qty = reader["qty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["qty"]),
                        Cost = reader["cost"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["cost"]),
                        Year = reader["year"] == DBNull.Value ? 0 : Convert.ToInt32(reader["year"]),
                        Quarter = reader["quarter"] == DBNull.Value ? 0 : Convert.ToInt32(reader["quarter"])
                    });
                }
                reader.Close();
            }


            string currentmonth = @"select sum(billitems.receivable) as amount,
sum(billitems.qty) as qty,
sum(stock.ibuyingprice::numeric*billitems.qty::numeric) as cost,
EXTRACT(quarter from billdate) as quarter,
EXTRACT(year from billdate) as year from bill
inner join billitems on billitems.billid = bill.id and billitems.isactive=true
inner join stock on stock.id = stockid and stock.productid = @productid
where bill.billdate>=@monthfromdate and bill.billdate<@monthtodate and bill.billtype != 9 and bill.isactive = true
group by EXTRACT(quarter from billdate),EXTRACT(year from billdate);";
            param.Add("monthfromdate", new DateTime(DateTimeNow.Year, DateTimeNow.Month, 1));
            param.Add("monthtodate", new DateTime(DateTimeNow.Year, DateTimeNow.Month, 1).AddMonths(1));
            using (var reader = ExecuteReader(currentmonth, param))
            {
                while (reader.Read())
                {
                    var ci = new MobileInsight.Insight.SalesItem()
                    {

                        Amount = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"]),
                        Qty = reader["qty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["qty"]),
                        Cost = reader["cost"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["cost"]),
                        Year = reader["year"] == DBNull.Value ? 0 : Convert.ToInt32(reader["year"]),
                        Quarter = reader["quarter"] == DBNull.Value ? 0 : Convert.ToInt32(reader["quarter"])
                    };
                    if (list.Exists(e => e.Year == ci.Year && e.Quarter == ci.Quarter))
                    {
                        var existci = list.Find(e => e.Year == ci.Year && e.Quarter == ci.Quarter);
                        existci.Amount = existci.Amount + ci.Amount;
                        existci.Cost = existci.Cost + ci.Cost;
                        existci.Qty = existci.Qty + ci.Qty;
                    }
                    else
                    {
                        list.Add(ci);
                    }
                }
                reader.Close();
            }
            list.ForEach(e =>
            {
                e.Quarter -= 1;
                if (e.Quarter == 0)
                {
                    e.Quarter = 4;
                    e.Year = e.Year - 1;
                }
            });
            return list;
        }
        public List<MobileInsight.Insight.SalesItem> GetSupplierCurrentYearSales(long supplierid)
        {
            if (!NeedRefresh())
            {
                Refresh();
            }
            var param = new SortedList<string, object>();
            var dateend = GetQuarterLast(DateTimeNow.Date).AddDays(1);
            var datebegin = dateend.AddYears(-1).AddDays(-1);
            param.Add("datebegin", datebegin);
            param.Add("dateend", dateend.AddDays(1));
            param.Add("supplierid", supplierid);
            var list = new List<MobileInsight.Insight.SalesItem>();
            using (var reader = ExecuteReader(@"Select supplierid,year,quarter,sum(amount) as amount,sum(qty) as qty,sum(cost) as cost from MonthlySales
                                where datebegin >= @datebegin and datebegin < @dateend and supplierid=@supplierid
                                group by supplierid, year, quarter", param))
            {
                while (reader.Read())
                {
                    list.Add(
                    new MobileInsight.Insight.SalesItem()
                    {
                        Amount = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"]),
                        Qty = reader["qty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["qty"]),
                        Cost = reader["cost"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["cost"]),
                        Year = reader["year"] == DBNull.Value ? 0 : Convert.ToInt32(reader["year"]),
                        Quarter = reader["quarter"] == DBNull.Value ? 0 : Convert.ToInt32(reader["quarter"])
                    });
                }
                reader.Close();
            }


            string currentmonth = @"select sum(billitems.receivable) as amount,
sum(billitems.qty) as qty,
sum(stock.ibuyingprice::numeric*billitems.qty::numeric) as cost,
EXTRACT(quarter from billdate) as quarter,
EXTRACT(year from billdate) as year from bill
inner join billitems on billitems.billid = bill.id and billitems.isactive=true
inner join stock on stock.id = stockid and stock.supplierid = @supplierid
where bill.billdate>=@monthfromdate and bill.billdate<@monthtodate and bill.billtype != 9 and bill.isactive = true
group by EXTRACT(quarter from billdate),EXTRACT(year from billdate);";
            param.Add("monthfromdate", new DateTime(DateTimeNow.Year, DateTimeNow.Month, 1));
            param.Add("monthtodate", new DateTime(DateTimeNow.Year, DateTimeNow.Month, 1).AddMonths(1));
            using (var reader = ExecuteReader(currentmonth, param))
            {
                while (reader.Read())
                {
                    var ci = new MobileInsight.Insight.SalesItem()
                    {

                        Amount = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"]),
                        Qty = reader["qty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["qty"]),
                        Cost = reader["cost"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["cost"]),
                        Year = reader["year"] == DBNull.Value ? 0 : Convert.ToInt32(reader["year"]),
                        Quarter = reader["quarter"] == DBNull.Value ? 0 : Convert.ToInt32(reader["quarter"])
                    };
                    if (list.Exists(e => e.Year == ci.Year && e.Quarter == ci.Quarter))
                    {
                        var existci = list.Find(e => e.Year == ci.Year && e.Quarter == ci.Quarter);
                        existci.Amount = existci.Amount + ci.Amount;
                        existci.Cost = existci.Cost + ci.Cost;
                        existci.Qty = existci.Qty + ci.Qty;
                    }
                    else
                    {
                        list.Add(ci);
                    }
                }
                reader.Close();
            }
            list.ForEach(e =>
            {
                e.Quarter -= 1;
                if (e.Quarter == 0)
                {
                    e.Quarter = 4;
                    e.Year = e.Year - 1;
                }
            });
            return list;
        }
        static int[] quarters = new int[] { 4, 4, 4, 1, 1, 1, 2, 2, 2, 3, 3, 3 };
        public static DateTime GetQuarterLast(DateTime date)
        {
            var quarter = quarters[date.Month - 1];
            switch (quarter)
            {
                case 1:
                    return new DateTime(date.Year, 6, 30);
                case 2:
                    return new DateTime(date.Year, 9, 30);
                case 3:
                    return new DateTime(date.Year, 12, 31);
            }
            return new DateTime(date.Year, 3, 31);
        }


    }
}