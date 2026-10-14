
using InfyPOS.Common;
using InfyPOS.Data;
using InfyPOS.Models;
using Newtonsoft.Json;
using Npgsql;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using WebAPI.Data;

namespace InfyPOS.Analytical
{
    public class MobileInsight : Common.BaseData
    {
        private string GetFinanceYear(int year)
        {
            return year.ToString("N0").Substring(3, 2) + "-" + (year + 1).ToString("N0").Substring(3, 2);
        }
        public List<Insight.StockItem> Location()
        {
            string fullquery = @"select case when EXTRACT(day from @current_date - stock.stockdate)<30 then 'below 30 days'
	when EXTRACT(day from @current_date - stock.stockdate)<61 then '30 - 60 days'
	when EXTRACT(day from @current_date - stock.stockdate)<91 then '60 - 90 days'
	when EXTRACT(day from @current_date - stock.stockdate)<121 then '90 - 120 days'
    when EXTRACT(day from @current_date - stock.stockdate)<151 then '120 - 150 days'
    when EXTRACT(day from @current_date - stock.stockdate)<181 then '150 - 180 days'
	else 'above 180 days' end as 
	period,sum(salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty)) as qty, 
sum((salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty))*stock.ibuyingprice::numeric) as value from salablegoods
inner join stock on stock.id = stockid
where salablegoods.hasstock = true and salablegoods.isactive = true and salablegoods.locationid=@locationid
group by case when EXTRACT(day from @current_date - stock.stockdate)<30 then 'below 30 days'
	when EXTRACT(day from @current_date - stock.stockdate)<61 then '30 - 60 days'
	when EXTRACT(day from @current_date - stock.stockdate)<91 then '60 - 90 days'
	when EXTRACT(day from @current_date - stock.stockdate)<121 then '90 - 120 days'
    when EXTRACT(day from @current_date - stock.stockdate)<151 then '120 - 150 days'
    when EXTRACT(day from @current_date - stock.stockdate)<181 then '150 - 180 days'
	else 'above 180 days' end;

select case when EXTRACT(day from bill.billdate - stock.stockdate)<30 then 'below 30 days'
	when EXTRACT(day from bill.billdate - stock.stockdate)<61 then '30 - 60 days'
	when EXTRACT(day from bill.billdate - stock.stockdate)<91 then '60 - 90 days'
	when EXTRACT(day from bill.billdate - stock.stockdate)<121 then '90 - 120 days'
    when EXTRACT(day from bill.billdate - stock.stockdate)<151 then '120 - 150 days'
    when EXTRACT(day from bill.billdate - stock.stockdate)<181 then '150 - 180 days'
	else 'above 180 days' end as 
	period,sum(billitems.qty) as qty from bill
inner join billitems on billitems.billid = bill.id and billitems.isactive=true
inner join stock on stock.id = stockid
where bill.billdate>=@todayfromdate and bill.billdate<@todaytodate and bill.billtype != 9 
and bill.locationid = @locationid and bill.isactive = true
group by case when EXTRACT(day from bill.billdate - stock.stockdate)<30 then 'below 30 days'
	when EXTRACT(day from bill.billdate - stock.stockdate)<61 then '30 - 60 days'
	when EXTRACT(day from bill.billdate - stock.stockdate)<91 then '60 - 90 days'
	when EXTRACT(day from bill.billdate - stock.stockdate)<121 then '90 - 120 days'
    when EXTRACT(day from bill.billdate - stock.stockdate)<151 then '120 - 150 days'
    when EXTRACT(day from bill.billdate - stock.stockdate)<181 then '150 - 180 days'
	else 'above 180 days' end;

select case when EXTRACT(day from bill.billdate - stock.stockdate)<30 then 'below 30 days'
	when EXTRACT(day from bill.billdate - stock.stockdate)<61 then '30 - 60 days'
	when EXTRACT(day from bill.billdate - stock.stockdate)<91 then '60 - 90 days'
	when EXTRACT(day from bill.billdate - stock.stockdate)<121 then '90 - 120 days'
    when EXTRACT(day from bill.billdate - stock.stockdate)<151 then '120 - 150 days'
    when EXTRACT(day from bill.billdate - stock.stockdate)<181 then '150 - 180 days'
	else 'above 180 days' end as 
	period,sum(billitems.qty) as qty from bill
inner join billitems on billitems.billid = bill.id and billitems.isactive=true
inner join stock on stock.id = stockid
where bill.billdate>=@threemonthfromdate and bill.billdate<@todaytodate and bill.billtype != 9 
and bill.locationid = @locationid and bill.isactive = true
group by case when EXTRACT(day from bill.billdate - stock.stockdate)<30 then 'below 30 days'
	when EXTRACT(day from bill.billdate - stock.stockdate)<61 then '30 - 60 days'
	when EXTRACT(day from bill.billdate - stock.stockdate)<91 then '60 - 90 days'
	when EXTRACT(day from bill.billdate - stock.stockdate)<121 then '90 - 120 days'
    when EXTRACT(day from bill.billdate - stock.stockdate)<151 then '120 - 150 days'
    when EXTRACT(day from bill.billdate - stock.stockdate)<181 then '150 - 180 days'
	else 'above 180 days' end;
";

            var result = new Insight();
            var todate = GetQuarterLast(DateTimeNow.Date).AddDays(1);
            var baseData = new Common.BaseData() { Context = this.Context };
            using (var connection = baseData.CreateConnection())
            {


                try
                {
                    connection.Open();
                    fullquery = fullquery.Replace("and salablegoods.locationid=@locationid", "").Replace("and bill.locationid = @locationid", "");

                    if (Services.ConfigManager.Instance(Context.OrganizationID).Config.analyser_stock_removeaqty)
                        fullquery = fullquery.Replace("salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty)", "salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty+salablegoods.aqty)");

                    NpgsqlCommand command = new NpgsqlCommand(fullquery, connection);
                    command.Parameters.Add("current_date", NpgsqlTypes.NpgsqlDbType.Date).Value = DateTimeNow.Date;
                    command.Parameters.Add("fromdate", NpgsqlTypes.NpgsqlDbType.Date).Value = todate.AddYears(-1);
                    command.Parameters.Add("quaterfrom", NpgsqlTypes.NpgsqlDbType.Date).Value = new DateTime(todate.AddYears(-1).Year, 4, 1);
                    command.Parameters.Add("todate", NpgsqlTypes.NpgsqlDbType.Date).Value = todate;
                    command.Parameters.Add("current_date", NpgsqlTypes.NpgsqlDbType.Date).Value = DateTimeNow.Date;
                    command.Parameters.Add("todayfromdate", NpgsqlTypes.NpgsqlDbType.Date).Value = DateTimeNow.Date;
                    command.Parameters.Add("todaytodate", NpgsqlTypes.NpgsqlDbType.Date).Value = DateTimeNow.Date.AddDays(1);
                    command.Parameters.Add("threemonthfromdate", NpgsqlTypes.NpgsqlDbType.Date).Value = DateTimeNow.Date.AddMonths(-3);

                    command.CommandTimeout = 10000;
                    using (var reader = command.ExecuteReader())
                    {

                        result.StockData = new List<Insight.StockItem>();
                        while (reader.Read())
                        {
                            result.StockData.Add(new Insight.StockItem()
                            {
                                Period = reader["period"] != DBNull.Value ? Convert.ToString(reader["period"]) : "",
                                Qty = reader["qty"] != DBNull.Value ? Convert.ToDecimal(reader["qty"]) : 0,
                                Value = reader["value"] != DBNull.Value ? Convert.ToDecimal(reader["value"]) : 0,
                                Index = IndexOfDays(reader["period"] != DBNull.Value ? Convert.ToString(reader["period"]) : "")
                            });
                        }

                        if (reader.NextResult())
                        {
                            while (reader.Read())
                            {
                                var item = new Insight.StockItem()
                                {
                                    Period = reader["period"] != DBNull.Value ? Convert.ToString(reader["period"]) : "",
                                    Sold = reader["qty"] != DBNull.Value ? Convert.ToDecimal(reader["qty"]) : 0,
                                    Index = IndexOfDays(reader["period"] != DBNull.Value ? Convert.ToString(reader["period"]) : "")
                                };
                                if (result.StockData.Exists(e => e.Period == item.Period))
                                {
                                    result.StockData.Find(e => e.Period == item.Period).Sold = item.Sold;
                                }
                                else
                                {
                                    result.StockData.Add(item);
                                }
                            }
                        }

                        if (reader.NextResult())
                        {
                            while (reader.Read())
                            {
                                var item = new Insight.StockItem()
                                {
                                    Period = reader["period"] != DBNull.Value ? Convert.ToString(reader["period"]) : "",
                                    TSold = reader["qty"] != DBNull.Value ? Convert.ToDecimal(reader["qty"]) : 0,
                                    Index = IndexOfDays(reader["period"] != DBNull.Value ? Convert.ToString(reader["period"]) : "")
                                };
                                if (result.StockData.Exists(e => e.Period == item.Period))
                                {
                                    result.StockData.Find(e => e.Period == item.Period).TSold = item.TSold;
                                }
                                else
                                {
                                    result.StockData.Add(item);
                                }
                            }
                        }
                    }

                    var totalcost = result.StockData.Sum(x => x.Value);
                    result.StockData.ForEach(x =>
                    {
                        if (totalcost > 0 && x.Value > 0)
                            x.pt = Math.Round(x.Value / totalcost * 100);
                    });
                }
                catch (Exception exp)
                {
                    WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, "MobileInsight.Location Summary");
                }
                finally
                {
                    connection.Close();
                }
            }

            result.StockData.Sort(delegate (Insight.StockItem a, Insight.StockItem b)
            {
                return a.Index.CompareTo(b.Index);
            });

            return result.StockData;
        }
        public int GetQuarter(int quarter)
        {
            if (quarter + 1 > 4) return 1;
            return quarter;
        }
        private static ConcurrentDictionary<string, bool> locationLocks = new ConcurrentDictionary<string, bool>();

        public Insight Location(long locationid)
        {
            if (locationLocks.TryAdd("Location" + Context.UserID.ToString() + "-" + locationid, true))
            {
                try
                {
                    return InternalLocation(locationid);
                }
                catch (Exception exp)
                {
                    throw exp;
                }
                finally
                {
                    bool bl;
                    locationLocks.TryRemove("Location" + Context.UserID.ToString() + "-" + locationid, out bl);
                }
            }
            else
            {
                throw new Exception("Already running");
            }
        }
        public Insight InternalLocation(long locationid)
        {
            string fullquery = @"select case when EXTRACT(day from @current_date - stock.stockdate)<30 then 'below 30 days'
	when EXTRACT(day from @current_date - stock.stockdate)<61 then '30 - 60 days'
	when EXTRACT(day from @current_date - stock.stockdate)<91 then '60 - 90 days'
	when EXTRACT(day from @current_date - stock.stockdate)<121 then '90 - 120 days'
    when EXTRACT(day from @current_date - stock.stockdate)<151 then '120 - 150 days'
    when EXTRACT(day from @current_date - stock.stockdate)<181 then '150 - 180 days'
	else 'above 180 days' end as 
	period,sum(salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty)) as qty, 
sum((salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty))*stock.ibuyingprice::numeric) as value from salablegoods
inner join stock on stock.id = stockid
where salablegoods.hasstock = true and salablegoods.isactive = true and salablegoods.locationid=@locationid
group by case when EXTRACT(day from @current_date - stock.stockdate)<30 then 'below 30 days'
	when EXTRACT(day from @current_date - stock.stockdate)<61 then '30 - 60 days'
	when EXTRACT(day from @current_date - stock.stockdate)<91 then '60 - 90 days'
	when EXTRACT(day from @current_date - stock.stockdate)<121 then '90 - 120 days'
    when EXTRACT(day from @current_date - stock.stockdate)<151 then '120 - 150 days'
    when EXTRACT(day from @current_date - stock.stockdate)<181 then '150 - 180 days'
	else 'above 180 days' end;

select case when EXTRACT(day from bill.billdate - stock.stockdate)<30 then 'below 30 days'
	when EXTRACT(day from bill.billdate - stock.stockdate)<61 then '30 - 60 days'
	when EXTRACT(day from bill.billdate - stock.stockdate)<91 then '60 - 90 days'
	when EXTRACT(day from bill.billdate - stock.stockdate)<121 then '90 - 120 days'
    when EXTRACT(day from bill.billdate - stock.stockdate)<151 then '120 - 150 days'
    when EXTRACT(day from bill.billdate - stock.stockdate)<181 then '150 - 180 days'
	else 'above 180 days' end as 
	period,sum(billitems.qty) as qty from bill
inner join billitems on billitems.billid = bill.id and billitems.isactive=true
inner join stock on stock.id = stockid
where bill.billdate>=@todayfromdate and bill.billdate<@todaytodate and bill.billtype != 9 
and bill.locationid = @locationid and bill.isactive = true
group by case when EXTRACT(day from bill.billdate - stock.stockdate)<30 then 'below 30 days'
	when EXTRACT(day from bill.billdate - stock.stockdate)<61 then '30 - 60 days'
	when EXTRACT(day from bill.billdate - stock.stockdate)<91 then '60 - 90 days'
	when EXTRACT(day from bill.billdate - stock.stockdate)<121 then '90 - 120 days'
    when EXTRACT(day from bill.billdate - stock.stockdate)<151 then '120 - 150 days'
    when EXTRACT(day from bill.billdate - stock.stockdate)<181 then '150 - 180 days'
	else 'above 180 days' end;";

            fullquery = @"select case when (@current_date - stock.stockdate)<30 then 'below 30 days'
when (@current_date - stock.stockdate)<61 then '30 - 60 days'
when (@current_date - stock.stockdate)<91 then '60 - 90 days'
when (@current_date - stock.stockdate)<121 then '90 - 120 days'
when (@current_date - stock.stockdate)<151 then '120 - 150 days'
when (@current_date - stock.stockdate)<181 then '150 - 180 days'
else 'above 180 days' end as 
period,sum(stockqty) as qty, 
sum(productprice*stockqty) as value from mw_dailstockdetails stock
where stock.locationid = @locationid
group by period;

select case when EXTRACT(day from bill.billdate - stock.stockdate)<30 then 'below 30 days'
	when EXTRACT(day from bill.billdate - stock.stockdate)<61 then '30 - 60 days'
	when EXTRACT(day from bill.billdate - stock.stockdate)<91 then '60 - 90 days'
	when EXTRACT(day from bill.billdate - stock.stockdate)<121 then '90 - 120 days'
    when EXTRACT(day from bill.billdate - stock.stockdate)<151 then '120 - 150 days'
    when EXTRACT(day from bill.billdate - stock.stockdate)<181 then '150 - 180 days'
	else 'above 180 days' end as 
	period,sum(billitems.qty) as qty from bill
inner join billitems on billitems.billid = bill.id and billitems.isactive=true
inner join stock on stock.id = stockid
where bill.billdate>=@todayfromdate and bill.billdate<@todaytodate and bill.billtype != 9 
and bill.locationid = @locationid and bill.isactive = true
group by period;";

            var noncacheQuery = @"
select sum(bill.receivable+bill.rounding) as amount,
EXTRACT(quarter from billdate) as quarter,
EXTRACT(year from billdate) as year from bill
where bill.billdate>=@quaterfrom and bill.billdate<@todate and bill.billtype != 9 and bill.locationid = @locationid and bill.isactive = true
group by EXTRACT(quarter from billdate),EXTRACT(year from billdate);

select sum((iqty-dqty-jqty)::numeric*stock.ibuyingprice::numeric) as cost,
EXTRACT(quarter from stock.stockdate) as quarter,EXTRACT(year from stock.stockdate) as year from stock
inner join salablegoods on stockid = stock.id and salablegoods.locationid=@locationid
where stock.stockdate>=@quaterfrom and stock.stockdate<@todate
group by EXTRACT(quarter from stock.stockdate),EXTRACT(year from stock.stockdate);";

            noncacheQuery = @"
select sum(amount) as amount,
EXTRACT(quarter from billdate) as quarter,
EXTRACT(year from billdate) as year from mv_salesmonthly bill
where bill.billdate>=@quaterfrom and bill.billdate<@todate and bill.locationid = @locationid
group by EXTRACT(quarter from billdate),EXTRACT(year from billdate);

select sum(cost) as cost,
EXTRACT(quarter from stock.stockdate) as quarter,EXTRACT(year from stock.stockdate) as year from  mv_stockmonthly stock
where stock.stockdate>=@quaterfrom and stock.stockdate<@todate
group by EXTRACT(quarter from stock.stockdate),EXTRACT(year from stock.stockdate);";

            var result = new Insight();
            var todate = GetQuarterLast(DateTimeNow.Date).AddDays(1);
            var baseData = new Common.BaseData() { Context = this.Context };
            using (var connection = baseData.CreateConnection())
            {


                try
                {
                    connection.Open();
                    if (locationid <= 0)
                        fullquery = fullquery.Replace("and salablegoods.locationid=@locationid", "").Replace("and bill.locationid = @locationid", "");

                    if (Services.ConfigManager.Instance(Context.OrganizationID).Config.analyser_stock_removeaqty)
                        fullquery = fullquery.Replace("salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty)", "salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty+salablegoods.aqty)");

                    if (!Services.ConfigManager.Instance(Context.OrganizationID).Config.analyser_data_cache)
                        fullquery += noncacheQuery;

                    NpgsqlCommand command = new NpgsqlCommand(fullquery, connection);
                    command.Parameters.Add("current_date", NpgsqlTypes.NpgsqlDbType.Date).Value = DateTimeNow.Date;
                    command.Parameters.Add("fromdate", NpgsqlTypes.NpgsqlDbType.Date).Value = todate.AddYears(-1);
                    command.Parameters.Add("quaterfrom", NpgsqlTypes.NpgsqlDbType.Date).Value = new DateTime(todate.AddYears(-1).Year, 4, 1);
                    command.Parameters.Add("todate", NpgsqlTypes.NpgsqlDbType.Date).Value = todate;
                    command.Parameters.Add("current_date", NpgsqlTypes.NpgsqlDbType.Date).Value = DateTimeNow.Date;
                    command.Parameters.Add("todayfromdate", NpgsqlTypes.NpgsqlDbType.Date).Value = DateTimeNow.Date;
                    command.Parameters.Add("todaytodate", NpgsqlTypes.NpgsqlDbType.Date).Value = DateTimeNow.Date.AddDays(1);
                    command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = locationid;
                    command.CommandTimeout = 10000;
                    using (var reader = command.ExecuteReader())
                    {

                        result.StockData = new List<Insight.StockItem>();
                        while (reader.Read())
                        {
                            result.StockData.Add(new Insight.StockItem()
                            {
                                Period = reader["period"] != DBNull.Value ? Convert.ToString(reader["period"]) : "",
                                Qty = reader["qty"] != DBNull.Value ? Convert.ToDecimal(reader["qty"]) : 0,
                                Value = reader["value"] != DBNull.Value ? Convert.ToDecimal(reader["value"]) : 0,
                                Index = IndexOfDays(reader["period"] != DBNull.Value ? Convert.ToString(reader["period"]) : "")
                            });
                        }
                        result.StockDataTotalQty = result.StockData.Sum(e => e.Qty);
                        result.StockDataTotalValue = result.StockData.Sum(e => e.Value);

                        if (reader.NextResult())
                        {
                            while (reader.Read())
                            {
                                var item = new Insight.StockItem()
                                {
                                    Period = reader["period"] != DBNull.Value ? Convert.ToString(reader["period"]) : "",
                                    Sold = reader["qty"] != DBNull.Value ? Convert.ToDecimal(reader["qty"]) : 0,
                                    Index = IndexOfDays(reader["period"] != DBNull.Value ? Convert.ToString(reader["period"]) : "")
                                };
                                if (result.StockData.Exists(e => e.Period == item.Period))
                                {
                                    result.StockData.Find(e => e.Period == item.Period).Sold = item.Sold;
                                }
                                else
                                {
                                    result.StockData.Add(item);
                                }
                            }
                            result.StockDataTotalSold = result.StockData.Sum(e => e.Sold);
                        }

                        if (!Services.ConfigManager.Instance(Context.OrganizationID).Config.analyser_data_cache)
                        {
                            if (reader.NextResult())
                            {
                                result.SalesData = new List<Insight.SalesItem>();
                                while (reader.Read())
                                {
                                    result.SalesData.Add(new Insight.SalesItem()
                                    {
                                        Amount = reader["Amount"] != DBNull.Value ? Convert.ToDecimal(reader["Amount"]) : 0,
                                        Quarter = reader["Quarter"] != DBNull.Value ? Convert.ToInt32(reader["Quarter"]) : 0,
                                        Year = reader["Year"] != DBNull.Value ? Convert.ToInt32(reader["Year"]) : 0,
                                    });
                                }
                                result.SalesData.ForEach(e =>
                                {
                                    e.Quarter -= 1;
                                    if (e.Quarter == 0)
                                    {
                                        e.Quarter = 4;
                                        e.Year = e.Year - 1;
                                    }
                                });
                                result.SalesData = result.SalesData.OrderBy(e => e.Year * 12 + e.Quarter * 4).ToList();
                            }
                            if (reader.NextResult())
                            {
                                result.PurchaseData = new List<Insight.SalesItem>();
                                while (reader.Read())
                                {
                                    result.PurchaseData.Add(new Insight.SalesItem()
                                    {
                                        Cost = reader["Cost"] != DBNull.Value ? Convert.ToDecimal(reader["Cost"]) : 0,
                                        Quarter = reader["Quarter"] != DBNull.Value ? Convert.ToInt32(reader["Quarter"]) : 0,
                                        Year = reader["Year"] != DBNull.Value ? Convert.ToInt32(reader["Year"]) : 0,
                                    });
                                }
                                result.PurchaseData.ForEach(e =>
                                {
                                    e.Quarter -= 1;
                                    if (e.Quarter == 0)
                                    {
                                        e.Quarter = 4;
                                        e.Year = e.Year - 1;
                                    }
                                });
                                result.PurchaseData = result.PurchaseData.OrderBy(e => e.Year * 12 + e.Quarter * 4).ToList();
                            }
                        }
                    }

                    if (Services.ConfigManager.Instance(Context.OrganizationID).Config.analyser_data_cache)
                    {
                        fullquery = @"select sum(amount) Amount,EXTRACT(quarter from billdate) as quarter,
                        EXTRACT(year from billdate) as year from MV_SalesMonthly
                        where locationid = @locationid and billdate>=@fromdate and billdate<@todate
                        group by EXTRACT(quarter from billdate),
                        EXTRACT(year from billdate);

                        select sum(cost) cost,
                        EXTRACT(quarter from stockdate) as quarter,EXTRACT(year from stockdate) as year
                        from MV_StockMonthly
                        where stockdate>=@fromdate and stockdate<@todate and locationid = @locationid
                        group by EXTRACT(quarter from stockdate),EXTRACT(year from stockdate)";
                        command = new NpgsqlCommand(fullquery, connection);
                        command.Parameters.Add("current_date", NpgsqlTypes.NpgsqlDbType.Date).Value = DateTimeNow.Date;
                        command.Parameters.Add("fromdate", NpgsqlTypes.NpgsqlDbType.Date).Value = todate.AddYears(-1);
                        command.Parameters.Add("quaterfrom", NpgsqlTypes.NpgsqlDbType.Date).Value = new DateTime(todate.AddYears(-1).Year, 4, 1);
                        command.Parameters.Add("todate", NpgsqlTypes.NpgsqlDbType.Date).Value = todate;
                        command.Parameters.Add("current_date", NpgsqlTypes.NpgsqlDbType.Date).Value = DateTimeNow.Date;
                        command.Parameters.Add("todayfromdate", NpgsqlTypes.NpgsqlDbType.Date).Value = DateTimeNow.Date;
                        command.Parameters.Add("todaytodate", NpgsqlTypes.NpgsqlDbType.Date).Value = DateTimeNow.Date.AddDays(1);
                        command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = locationid;
                        command.CommandTimeout = 10000;
                        using (var reader = command.ExecuteReader())
                        {
                            result.SalesData = new List<Insight.SalesItem>();
                            while (reader.Read())
                            {
                                result.SalesData.Add(new Insight.SalesItem()
                                {
                                    Amount = reader["Amount"] != DBNull.Value ? Convert.ToDecimal(reader["Amount"]) : 0,
                                    Quarter = reader["Quarter"] != DBNull.Value ? Convert.ToInt32(reader["Quarter"]) : 0,
                                    Year = reader["Year"] != DBNull.Value ? Convert.ToInt32(reader["Year"]) : 0,
                                });
                            }
                            result.SalesData.ForEach(e =>
                            {
                                e.Quarter -= 1;
                                if (e.Quarter == 0)
                                {
                                    e.Quarter = 4;
                                    e.Year = e.Year - 1;
                                }
                            });
                            result.SalesData = result.SalesData.OrderBy(e => e.Year * 12 + e.Quarter * 4).ToList();

                            if (reader.NextResult())
                            {
                                result.PurchaseData = new List<Insight.SalesItem>();
                                while (reader.Read())
                                {
                                    result.PurchaseData.Add(new Insight.SalesItem()
                                    {
                                        Cost = reader["Cost"] != DBNull.Value ? Convert.ToDecimal(reader["Cost"]) : 0,
                                        Quarter = reader["Quarter"] != DBNull.Value ? Convert.ToInt32(reader["Quarter"]) : 0,
                                        Year = reader["Year"] != DBNull.Value ? Convert.ToInt32(reader["Year"]) : 0,
                                    });
                                }
                                result.PurchaseData.ForEach(e =>
                                {
                                    e.Quarter -= 1;
                                    if (e.Quarter == 0)
                                    {
                                        e.Quarter = 4;
                                        e.Year = e.Year - 1;
                                    }
                                });
                                result.PurchaseData = result.PurchaseData.OrderBy(e => e.Year * 12 + e.Quarter * 4).ToList();
                            }
                        }


                        DateTime quaterfrom = new DateTime(todate.AddYears(-1).Year, 4, 1);
                        InfyPOS.Processors.DBViewCache dBViewCache = new Processors.DBViewCache();
                        var cachehistory = dBViewCache.CacheHistory();

                        if (cachehistory.ContainsKey("mv_stockmonthly"))
                            quaterfrom = cachehistory["mv_stockmonthly"];
                        command = new NpgsqlCommand(noncacheQuery, connection);
                        command.Parameters.Add("current_date", NpgsqlTypes.NpgsqlDbType.Date).Value = DateTimeNow.Date;
                        command.Parameters.Add("fromdate", NpgsqlTypes.NpgsqlDbType.Date).Value = todate.AddYears(-1);
                        command.Parameters.Add("quaterfrom", NpgsqlTypes.NpgsqlDbType.Date).Value = quaterfrom;
                        command.Parameters.Add("todate", NpgsqlTypes.NpgsqlDbType.Date).Value = todate;
                        command.Parameters.Add("current_date", NpgsqlTypes.NpgsqlDbType.Date).Value = DateTimeNow.Date;
                        command.Parameters.Add("todayfromdate", NpgsqlTypes.NpgsqlDbType.Date).Value = DateTimeNow.Date;
                        command.Parameters.Add("todaytodate", NpgsqlTypes.NpgsqlDbType.Date).Value = DateTimeNow.Date.AddDays(1);
                        command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = locationid;
                        command.CommandTimeout = 10000;
                        using (var reader = command.ExecuteReader())
                        {
                            var SalesData = new List<Insight.SalesItem>();
                            var PurchaseData = new List<Insight.SalesItem>();

                            while (reader.Read())
                            {
                                SalesData.Add(new Insight.SalesItem()
                                {
                                    Amount = reader["Amount"] != DBNull.Value ? Convert.ToDecimal(reader["Amount"]) : 0,
                                    Quarter = reader["Quarter"] != DBNull.Value ? Convert.ToInt32(reader["Quarter"]) : 0,
                                    Year = reader["Year"] != DBNull.Value ? Convert.ToInt32(reader["Year"]) : 0,
                                });
                            }
                            SalesData.ForEach(e =>
                            {
                                e.Quarter -= 1;
                                if (e.Quarter == 0)
                                {
                                    e.Quarter = 4;
                                    e.Year = e.Year - 1;
                                }
                            });

                            SalesData.ForEach(x =>
                            {
                                if (result.SalesData.Exists(y => y.Year == x.Year && y.Quarter == x.Quarter))
                                {
                                    result.SalesData.Find(y => y.Year == x.Year && y.Quarter == x.Quarter).Amount = x.Amount;
                                }
                                else
                                {
                                    result.SalesData.Add(x);
                                }
                            });
                            if (reader.NextResult())
                            {
                                while (reader.Read())
                                {
                                    PurchaseData.Add(new Insight.SalesItem()
                                    {
                                        Cost = reader["Cost"] != DBNull.Value ? Convert.ToDecimal(reader["Cost"]) : 0,
                                        Quarter = reader["Quarter"] != DBNull.Value ? Convert.ToInt32(reader["Quarter"]) : 0,
                                        Year = reader["Year"] != DBNull.Value ? Convert.ToInt32(reader["Year"]) : 0,
                                    });
                                }
                                PurchaseData.ForEach(e =>
                                {
                                    e.Quarter -= 1;
                                    if (e.Quarter == 0)
                                    {
                                        e.Quarter = 4;
                                        e.Year = e.Year - 1;
                                    }
                                });
                            }
                            PurchaseData.ForEach(x =>
                            {
                                if (result.PurchaseData.Exists(y => y.Year == x.Year && y.Quarter == x.Quarter))
                                {
                                    result.PurchaseData.Find(y => y.Year == x.Year && y.Quarter == x.Quarter).Cost = x.Cost;
                                }
                                else
                                {
                                    result.PurchaseData.Add(x);
                                }
                            });

                            result.PurchaseData = result.PurchaseData.OrderBy(e => e.Year * 12 + e.Quarter * 4).ToList();
                            result.SalesData = result.SalesData.OrderBy(e => e.Year * 12 + e.Quarter * 4).ToList();

                        }
                    }

                }
                catch (Exception exp)
                {
                    WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, "MobileInsight.LocationID" + locationid.ToString());
                }
                finally
                {
                    connection.Close();
                }
            }

            result.InsightData = new List<Insight.DataItem>();
            result.PurchaseData.ForEach(e =>
            {
                result.InsightData.Add(new Insight.DataItem()
                {
                    Purchase = e.Cost,
                    PurchaseQty = e.Qty,
                    Quarter = e.Quarter,
                    Year = e.Year,
                    Display = string.Format("{0}({1})", GetFinanceYear(e.Year), quaterdisplay[e.Quarter - 1])
                });
            });

            result.SalesData.ForEach(e =>
            {
                if (result.InsightData.Exists(x => x.Year == e.Year && x.Quarter == e.Quarter))
                {
                    var edata = result.InsightData.Find(x => x.Year == e.Year && x.Quarter == e.Quarter);
                    edata.Sales = e.Amount;
                    edata.SalesQty = e.Qty;
                    edata.Profit = e.Amount - e.Cost;
                    edata.Display = string.Format("{0}({1})", GetFinanceYear(e.Year), quaterdisplay[e.Quarter - 1]);
                }
                else
                {
                    result.InsightData.Add(new Insight.DataItem()
                    {
                        Sales = e.Cost,
                        SalesQty = e.Qty,
                        Quarter = e.Quarter,
                        Year = e.Year,
                        Profit = e.Amount - e.Cost,
                        Display = string.Format("{0}{1}({1})", GetFinanceYear(e.Year), quaterdisplay[e.Quarter - 1])
                    });
                }
            });

            result.display = "Apr'" + todate.AddYears(-1).Year.ToString("N0").Substring(3, 2) + " - " + "Mar'" + todate.Year.ToString("N0").Substring(3, 2);

            result.StockData.Sort(delegate (Insight.StockItem a, Insight.StockItem b)
            {
                return a.Index.CompareTo(b.Index);
            });

            result.ClosingAmount = result.StockData.Sum(e => e.Value);
            result.Closing = result.ClosingAmount;
            result.ClosingQty = result.StockData.Sum(e => e.Qty);
            result.SalesCost = result.SalesData.Sum(e => e.Cost);
            result.SalesQty = result.SalesData.Sum(e => e.Qty);
            result.SalesAmount = result.SalesData.Sum(e => e.Amount);
            result.PurchaseCost = result.PurchaseData.Sum(e => e.Cost);
            result.PurchaseQty = result.PurchaseData.Sum(e => e.Qty);
            result.OpeningCost = result.ClosingAmount - result.PurchaseCost + result.SalesCost;
            result.OpeningQty = result.ClosingQty - result.PurchaseQty + result.SalesQty;
            result.PriceData = LocationPriceRanges(locationid, DateTimeNow.AddMonths(-1), DateTimeNow.Date);

            result.PriceData.Sort(delegate (Insight.PriceRangeItem a, Insight.PriceRangeItem b)
            {
                return a.sortindex.CompareTo(b.sortindex);
            });
            result.SalesData.Sort(delegate (Insight.SalesItem a, Insight.SalesItem b)
            {
                return (a.Year * 12 + a.Quarter).CompareTo(b.Year * 12 + b.Quarter);
            });

            return result;
        }

        private string[] quaterdisplay = new string[] { "APR-JUN", "JUL-SEP", "OCT-DEC", "JAN-MAR" };

        public StoreViewEOD GetStoreViewEOD(StoreViewEOD_Request request)
        {

            string query = @"
select sum(case when billtype != 8 and billitems.receivable>0 then billitems.receivable else 0 end) salevalue,
sum(case when billtype != 8 and billitems.receivable<0 then billitems.receivable*-1::int else 0 end) exchangevalue,
sum(case when billtype = 8  then billitems.receivable*-1 else 0 end) returnvalue,
sum(case 
when customer.id is not null and customer.createdon>=@fromdate and customer.createdon<@todate then 1::int else 0::int
end) newcustomer,
sum(case 
when customer.id is not null and customer.createdon<@todate then 1::int else 0::int
end) repeatcustomer,
sum(case 
when customer.id is null then 1::int else 0::int
end) unregisterdcustomer
from bill 
left outer join customer on customer.id = bill.customerid
inner join billitems on billitems.billid = bill.id and billitems.isactive=true
where billdate>=@fromdate and billdate<@todate and billtype != 9
and bill.isb2b=false and bill.isactive = true and bill.locationid=@locationid and bill.companyid=@companyid;

select sum(case when billtype != 9 and billtype != 8 then 1 else 0 end) salecount,
sum(case when billtype != 9 then additionaldiscount else 0 end) additionaldiscount,
sum(cast(billattributes->>'rewardamount' as numeric)+
cast(billattributes->>'manualrewardamount' as numeric)) as rewards,
sum(case when billtype=9 and returnbillno like '~%' then 1 else 0 end) exchangecount,
sum(case when billtype=8 then 1 else 0 end) returncount,
sum(case when billtype=9 and returnbillno != '' and returnbillno not like '~%' then 1 else 0 end) cancelcount,
sum(case when billtype=9 and returnbillno != '' and returnbillno not like '~%' then receivable else 0 end) cancelvalue,
sum(case 
when bill.billtype != 9 and  customer.id is not null and customer.createdon>=bill.billdate::date and customer.createdon<@todate then 1::int else 0::int
end) newcustomer,
sum(case 
when bill.billtype != 9 and  customer.id is not null and customer.createdon<bill.billdate::date then 1::int else 0::int
end) repeatcustomer,
sum(case 
when bill.billtype != 9 and  customer.id is null then 1::int else 0::int
end) unregisterdcustomer,
sum(case when billtype != 9 then bill.rounding else 0 end) rounding,
sum(case when billtype != 9 then bill.receivable+bill.rounding else 0 end) billamount
from bill 
left outer join customer on customer.id = bill.customerid
where billdate>=@fromdate  and billdate<@todate
and bill.isb2b=false and bill.isactive = true and bill.locationid=@locationid and bill.companyid=@companyid;

select sum(paid_cash) cash,
sum(paid_card) card,
sum(paid_upi) upi,
sum(paid_credit) credit,
sum(paid_voucher) voucher, 
sum(loan_paid) loan,
sum(discount) discount,
sum(case when billsettlementmaster.paidamount<0 then cast(paymentinfo->>'cash_return' as numeric) else 0 end) refund,
sum(case when billsettlementmaster.paidamount<0 and cast(paymentinfo->>'cash_return' as numeric)>0 then 1::int else 0::int end) refundcount,
sum(case when paid_card>0 then 1::int else 0::int end) cardcount,
sum(case when paid_upi>0 then 1::int else 0::int end) upicount,
sum(case when paid_credit>0 then 1::int else 0::int end) creditcount,
sum(case when loan_paid>0 then 1::int else 0::int end) loancount,
sum(case when paid_voucher>0 then 1::int else 0::int end) vouchercount,
sum(case when paid_cash>0 then 1::int else 0::int end) cashcount,
sum(case when discount>0 then 1::int else 0::int end) discountcount
from billsettlementmaster 
where settlementon>=@fromdate and settlementon<@todate and billsettlementmaster.isactive = true
and billsettlementmaster.locationid=@locationid and billsettlementmaster.companyid=@companyid;

select sum(paid_cash+paid_card+paid_upi) cancelvalue,
sum(case when paid_cash+paid_card>0 then 1::int else 0::int end) cancelcount
from billsettlementmaster 
where settlementon>=@fromdate and settlementon<@todate and billsettlementmaster.isactive = true
and billsettlementmaster.locationid=@locationid and billsettlementmaster.companyid=@companyid;

select dledger.displayname, sum(voucheritems.amount) as amount from voucher
inner join voucheritems on voucheritems.voucherid = voucher.id  and voucheritems.isactive = true
inner join ledger cledger on cledger.id = adjustmentledgerid
inner join ledger dledger on dledger.id = voucheritems.ledgerid 
where voucherdate>=@fromdate 
and voucherdate <@todate and voucher.isactive = true
and voucher.locationid=@locationid and voucher.companyid=@companyid 
and vouchertype in (0,13) 
group by dledger.displayname;

select
case when vouchertype=9 then 'Cash Deposit'
when vouchertype=10 then 'Cash Widhrawal'
when vouchertype=11 then 'Main Cash to Petty Cash'
when vouchertype=12 then 'Petty Cash to Main Cash' 
when vouchertype=4 then 'Sales'
when cledger.typeid in (6,13) and voucheritems.iscredit=false then 'Expenses'
when cledger.typeid in (6,13) and voucheritems.iscredit=true then 'Income'
else 'Journal' end as display,sum(voucheritems.amount) as amount,dledger.displayname,voucheritems.iscredit from voucher
inner join voucheritems on voucheritems.voucherid = voucher.id and voucheritems.isactive = true
inner join ledger cledger on cledger.id = adjustmentledgerid
inner join ledger dledger on dledger.id = voucheritems.ledgerid 
where (cledger.typeid in (6,13) or dledger.typeid in (6,13)) and vouchertype >0
and voucherdate>=@fromdate and voucherdate <@todate 
and voucher.isactive = true
and voucher.locationid=@locationid and voucher.companyid=@companyid 
group by case when vouchertype=9 then 'Cash Deposit'
when vouchertype=10 then 'Cash Widhrawal'
when vouchertype=11 then 'Main Cash to Petty Cash'
when vouchertype=12 then 'Petty Cash to Main Cash' 
when vouchertype=4 then 'Sales'
when cledger.typeid in (6,13) and voucheritems.iscredit=false then 'Expenses'
when cledger.typeid in (6,13) and voucheritems.iscredit=true then 'Income'
else 'Journal' end,dledger.displayname,voucheritems.iscredit;

with sectionsales
as
(
select sum(case when billtype != 8 and billitems.receivable>0 then billitems.receivable else 0 end) salevalue,
sum(case when billtype != 8 and billitems.receivable<0 then billitems.receivable*-1::int else 0 end) exchangevalue,
sum(case when billtype = 8  then billitems.receivable*-1 else 0 end) returnvalue,
products.salescounterid
from bill 
inner join billitems on billitems.billid = bill.id and billitems.isactive=true
inner join stock on stock.id = billitems.stockid
inner join products on products.id = stock.productid
where billdate>=@fromdate and billdate<@todate and billtype != 9
and bill.isb2b=false and bill.isactive = true and bill.locationid=@locationid and bill.companyid=@companyid
group by products.salescounterid
)
select sectionsales.*,referencelist.name from sectionsales
inner join referencelist on referencelist.id = sectionsales.salescounterid;

with sectionsales
as
(
select sum(case when billtype != 8 and billitems.receivable>0 then billitems.receivable else 0 end) salevalue,
sum(case when billtype != 8 and billitems.receivable<0 then billitems.receivable*-1::int else 0 end) exchangevalue,
sum(case when billtype = 8  then billitems.receivable*-1 else 0 end) returnvalue,
products.barcodeid
from bill 
inner join billitems on billitems.billid = bill.id and billitems.isactive=true
inner join stock on stock.id = billitems.stockid
inner join products on products.id = stock.productid
where billdate>=@fromdate and billdate<@todate and billtype != 9
and bill.isb2b=false and bill.isactive = true and bill.locationid=@locationid and bill.companyid=@companyid
group by products.barcodeid
)
select sectionsales.*,referencelist.name from sectionsales
inner join referencelist on referencelist.id = sectionsales.barcodeid;

with productsales
as
(
select sum(case when billtype != 8 and billitems.receivable>0 then billitems.receivable else 0 end) salevalue,
sum(case when billtype != 8 and billitems.receivable<0 then billitems.receivable*-1::int else 0 end) exchangevalue,
sum(case when billtype = 8  then billitems.receivable*-1 else 0 end) returnvalue,
	sum(COALESCE(stock.buyingprice*billitems.qty::numeric)) buyingcost,
	sum(COALESCE(billitems.receivable-billitems.tax)) sellingvalue,
stock.productid
from bill 
inner join billitems on billitems.billid = bill.id and billitems.isactive=true
left outer join stock on stock.id = billitems.stockid
where billdate>=@fromdate and billdate<@todate and billtype != 9
and bill.isb2b=false and bill.isactive = true and bill.locationid=@locationid and bill.companyid=@companyid
group by stock.productid
)
select productsales.*,products.name,referencelist.name groupname,referencelist.id groupid from productsales
inner join products on products.id = productsales.productid
inner join referencelist on referencelist.id = products.productgroup;

with sectionsales
as
(
    select sum(case when billtype != 8 and billitems.receivable>0 then billitems.receivable else 0 end) salevalue,
    sum(case when billtype != 8 and billitems.receivable<0 then billitems.receivable*-1::int else 0 end) exchangevalue,
    sum(case when billtype = 8  then billitems.receivable*-1 else 0 end) returnvalue,
    billitems.salesmanid
    from bill 
    inner join billitems on billitems.billid = bill.id and billitems.isactive=true
    where billdate>=@fromdate and billdate<@todate and billtype != 9
    and bill.isb2b=false and bill.isactive = true and bill.locationid=@locationid and bill.companyid=@companyid
    group by billitems.salesmanid
)
select sectionsales.*,employee.name from sectionsales
inner join employee on employee.id = sectionsales.salesmanid;

select case when EXTRACT(day from @current_date - stock.stockdate)<30 then 'below 30'
	when EXTRACT(day from @current_date - stock.stockdate)<61 then '30 - 60'
	when EXTRACT(day from @current_date - stock.stockdate)<91 then '60 - 90'
	when EXTRACT(day from @current_date - stock.stockdate)<121 then '90 - 120'
    when EXTRACT(day from @current_date - stock.stockdate)<151 then '120 - 150'
    when EXTRACT(day from @current_date - stock.stockdate)<181 then '150 - 180'
	else 'above 180' end as 
	period,sum(salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty)) as qty, 
sum((salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty))*stock.ibuyingprice::numeric) as value from salablegoods
inner join stock on stock.id = stockid
where salablegoods.hasstock = true and salablegoods.isactive = true and salablegoods.locationid=@locationid
group by case when EXTRACT(day from @current_date - stock.stockdate)<30 then 'below 30'
	when EXTRACT(day from @current_date - stock.stockdate)<61 then '30 - 60'
	when EXTRACT(day from @current_date - stock.stockdate)<91 then '60 - 90'
	when EXTRACT(day from @current_date - stock.stockdate)<121 then '90 - 120'
    when EXTRACT(day from @current_date - stock.stockdate)<151 then '120 - 150'
    when EXTRACT(day from @current_date - stock.stockdate)<181 then '150 - 180'
	else 'above 180' end;

select case when EXTRACT(day from bill.billdate - stock.stockdate)<30 then 'below 30'
	when EXTRACT(day from bill.billdate - stock.stockdate)<61 then '30 - 60'
	when EXTRACT(day from bill.billdate - stock.stockdate)<91 then '60 - 90'
	when EXTRACT(day from bill.billdate - stock.stockdate)<121 then '90 - 120'
    when EXTRACT(day from bill.billdate - stock.stockdate)<151 then '120 - 150'
    when EXTRACT(day from bill.billdate - stock.stockdate)<181 then '150 - 180'
	else 'above 180' end as 
	period,sum(billitems.qty) as qty,
sum(billitems.receivable) as value from bill
inner join billitems on billitems.billid = bill.id and billitems.isactive=true
inner join stock on stock.id = stockid
where bill.billdate>=@fromdate and bill.billdate<@todate and bill.billtype != 9 
and bill.locationid=@locationid and bill.isactive = true
group by case when EXTRACT(day from bill.billdate - stock.stockdate)<30 then 'below 30'
	when EXTRACT(day from bill.billdate - stock.stockdate)<61 then '30 - 60'
	when EXTRACT(day from bill.billdate - stock.stockdate)<91 then '60 - 90'
	when EXTRACT(day from bill.billdate - stock.stockdate)<121 then '90 - 120'
    when EXTRACT(day from bill.billdate - stock.stockdate)<151 then '120 - 150'
    when EXTRACT(day from bill.billdate - stock.stockdate)<181 then '150 - 180'
	else 'above 180' end;";


            using (NpgsqlConnection conn = CreateConnection())
            {

                if (request.companyid <= 0)
                {
                    query = query.Replace("and voucher.companyid=@companyid", "");
                    query = query.Replace("and billsettlementmaster.companyid=@companyid", "");
                    query = query.Replace("and bill.companyid=@companyid", "");
                }

                if (request.locationid <= 0)
                {
                    query = query.Replace("and voucher.locationid=@locationid", "");
                    query = query.Replace("and billsettlementmaster.locationid=@locationid", "");
                    query = query.Replace("and bill.locationid=@locationid", "");
                    query = query.Replace("and salablegoods.locationid=@locationid", "");
                    query = query.Replace("where salablegoods.locationid = @locationid", "");
                }

                if (Services.ConfigManager.Instance(Context.OrganizationID).Config.analyser_stock_removeaqty)
                    query = query.Replace("salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty)", "salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty+salablegoods.aqty)");

                StoreViewEOD result = new StoreViewEOD();

                try
                {
                    DateTime date = request.date.AddDays(request.plus).AddDays(request.minus * -1).Date;
                    result.Date = date;
                    Data.CustomerwalkinsDataService customerwalkinsDataService = new Data.CustomerwalkinsDataService() { Context = this.Context };
                    var walkincount = customerwalkinsDataService.GetWalkinCount(date);

                    Data.VoucherDataService voucherDataService = new Data.VoucherDataService() { Context = this.Context };
                    var pettycashopening = voucherDataService.CashClosing(new Data.SaleRetailCreteria()
                    {
                        locationid = request.locationid,
                        companyid = request.companyid,
                        fromdate = date
                    }, 0, Data.VoucherDataService.CashMode.PettyCash);

                    var maincashopening = voucherDataService.CashClosing(new Data.SaleRetailCreteria()
                    {
                        locationid = request.locationid,
                        companyid = request.companyid,
                        fromdate = date
                    }, 0, Data.VoucherDataService.CashMode.MainCash);

                    var maincashclosing = voucherDataService.CashClosing(new Data.SaleRetailCreteria()
                    {
                        locationid = request.locationid,
                        companyid = request.companyid,
                        fromdate = date.AddDays(1)
                    }, 0, Data.VoucherDataService.CashMode.Both);

                    result.CashBook.Add(new StoreViewEOD_CashItem()
                    {
                        display = "Opening Petty Cash",
                        value = pettycashopening
                    });

                    result.CashBook.Add(new StoreViewEOD_CashItem()
                    {
                        display = "Opening Main Cash",
                        value = maincashopening
                    });

                    var newcustomer = 0;
                    var repeatcustomer = 0;
                    var unregisterdcustomer = 0;
                    conn.Open();

                    if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_isb2bapplication)
                        query = query.Replace("and bill.isb2b=false", "");

                    var command = GetCommand(query, conn);
                    command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = request.locationid;
                    command.Parameters.Add("fromdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = date;
                    command.Parameters.Add("current_date", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow.Date;
                    command.Parameters.Add("todate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = date.AddDays(1).Date;
                    using (System.Data.IDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var salevalue = reader["salevalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["salevalue"]);
                            var exchangevalue = reader["exchangevalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["exchangevalue"]);
                            var returnvalue = reader["returnvalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["returnvalue"]);

                            result.SaleItem.Add(new StoreViewEOD_SaleItem()
                            {
                                display = "Sale Value",
                                value = salevalue
                            });
                            result.SaleItem.Add(new StoreViewEOD_SaleItem()
                            {
                                display = "Exchange Value",
                                value = exchangevalue
                            });
                            result.SaleItem.Add(new StoreViewEOD_SaleItem()
                            {
                                display = "Return Value",
                                value = returnvalue
                            });

                        }

                        if (reader.NextResult() && reader.Read())
                        {
                            var salecount = reader["salecount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["salecount"]);
                            var additionaldiscount = reader["additionaldiscount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["additionaldiscount"]);
                            var rewards = reader["rewards"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["rewards"]);
                            var exchangecount = reader["exchangecount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["exchangecount"]);
                            var returncount = reader["returncount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["returncount"]);
                            var cancelcount = reader["cancelcount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["cancelcount"]);
                            var cancelvalue = reader["cancelvalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["cancelvalue"]);
                            var rounding = reader["rounding"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["rounding"]);
                            var billamount = reader["billamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["billamount"]);

                            newcustomer = reader["newcustomer"] == DBNull.Value ? 0 : Convert.ToInt32(reader["newcustomer"]);
                            repeatcustomer = reader["repeatcustomer"] == DBNull.Value ? 0 : Convert.ToInt32(reader["repeatcustomer"]);
                            unregisterdcustomer = reader["unregisterdcustomer"] == DBNull.Value ? 0 : Convert.ToInt32(reader["unregisterdcustomer"]);

                            result.SaleItem.Add(new StoreViewEOD_SaleItem()
                            {
                                display = "Rounding",
                                value = rounding
                            });


                            result.SaleItem.Add(new StoreViewEOD_SaleItem()
                            {
                                display = "Total Sale",
                                value = billamount
                            });

                            result.SaleItem.Add(new StoreViewEOD_SaleItem()
                            {
                                display = "Cancel Value",
                                value = cancelvalue
                            });

                            result.SaleItem.Add(new StoreViewEOD_SaleItem()
                            {
                                display = "Addnl Discount",
                                value = additionaldiscount
                            });
                            result.SaleItem.Add(new StoreViewEOD_SaleItem()
                            {
                                display = "Rewards",
                                value = rewards
                            });

                            result.SaleItem.Add(new StoreViewEOD_SaleItem()
                            {
                                display = "Sale Bill Count",
                                value = salecount
                            });
                            result.SaleItem.Add(new StoreViewEOD_SaleItem()
                            {
                                display = "Sale Return Count",
                                value = returncount
                            });
                            result.SaleItem.Add(new StoreViewEOD_SaleItem()
                            {
                                display = "Exchange Count",
                                value = exchangecount
                            });

                            result.SaleItem.Add(new StoreViewEOD_SaleItem()
                            {
                                display = "Cancel Count",
                                value = cancelcount
                            });
                        }

                        result.SaleItem.Add(new StoreViewEOD_SaleItem()
                        {
                            display = "Footfall",
                            value = walkincount
                        });

                        result.SaleItem.Add(new StoreViewEOD_SaleItem()
                        {
                            display = "New customer Bill Count",
                            value = newcustomer
                        });
                        result.SaleItem.Add(new StoreViewEOD_SaleItem()
                        {
                            display = "Repeat customer Bill Count",
                            value = repeatcustomer
                        });
                        result.SaleItem.Add(new StoreViewEOD_SaleItem()
                        {
                            display = "Unregisterd customer Bill Count",
                            value = unregisterdcustomer
                        });
                        if (reader.NextResult() && reader.Read())
                        {
                            var cash = reader["cash"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["cash"]);
                            var card = reader["card"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["card"]);
                            var upi = reader["upi"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["upi"]);
                            var credit = reader["credit"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["credit"]);
                            var voucher = reader["voucher"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["voucher"]);
                            var loan = reader["loan"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["loan"]);
                            var discount = reader["discount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discount"]);
                            var refund = reader["refund"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["refund"]);
                            var refundcount = reader["refundcount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["refundcount"]);
                            var cardcount = reader["cardcount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["cardcount"]);
                            var creditcount = reader["creditcount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["creditcount"]);
                            var upicount = reader["upicount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["upicount"]);
                            var vouchercount = reader["vouchercount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["vouchercount"]);
                            var loancount = reader["loancount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["loancount"]);
                            var cashcount = reader["cashcount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["cashcount"]);
                            var discountcount = reader["discountcount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discountcount"]);


                            maincashclosing += cash - refund;
                            result.CashBook.Add(new StoreViewEOD_CashItem()
                            {
                                display = "Cash Sales",
                                value = cash
                            });

                            result.CashBook.Add(new StoreViewEOD_CashItem()
                            {
                                display = "Refund",
                                value = refund
                            });

                            result.SettlementItem.Add(new StoreViewEOD_Settlement()
                            {
                                display = "Cash",
                                count = cashcount,
                                value = cash
                            });

                            result.SettlementItem.Add(new StoreViewEOD_Settlement()
                            {
                                display = "Refund",
                                count = refundcount,
                                value = refund
                            });

                            result.SettlementItem.Add(new StoreViewEOD_Settlement()
                            {
                                display = "Card",
                                count = cardcount,
                                value = card
                            });

                            result.SettlementItem.Add(new StoreViewEOD_Settlement()
                            {
                                display = "UPI",
                                count = upicount,
                                value = upi
                            });

                            result.SettlementItem.Add(new StoreViewEOD_Settlement()
                            {
                                display = "Credit",
                                count = creditcount,
                                value = credit
                            });


                            result.SettlementItem.Add(new StoreViewEOD_Settlement()
                            {
                                display = "EMI",
                                count = loancount,
                                value = loan
                            });

                            result.SettlementItem.Add(new StoreViewEOD_Settlement()
                            {
                                display = "Voucher",
                                count = vouchercount,
                                value = voucher
                            });

                            result.SettlementItem.Add(new StoreViewEOD_Settlement()
                            {
                                display = "Discount",
                                count = discountcount,
                                value = discount
                            });
                        }

                        if (reader.NextResult())
                        {
                            if (reader.Read())
                            {
                                var cancelcount = reader["cancelcount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["cancelcount"]);
                                var cancelvalue = reader["cancelvalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["cancelvalue"]);

                                result.SettlementItem.Add(new StoreViewEOD_Settlement()
                                {
                                    display = "Settlement Cancel",
                                    count = cancelcount,
                                    value = cancelvalue
                                });
                            }
                        }

                        if (reader.NextResult())
                        {
                            while (reader.Read())
                            {
                                result.PettyCashExpense.Add(new StoreViewEOD_CashItem()
                                {
                                    display = reader["displayname"].ToString(),
                                    value = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"])
                                });
                            }
                        }

                        if (reader.NextResult())
                        {
                            while (reader.Read())
                            {
                                result.Vochers.Add(new StoreViewEOD_CashItem()
                                {
                                    display = reader["display"].ToString(),
                                    parentdisplay = reader["displayname"].ToString(),
                                    symbol = Convert.ToBoolean(reader["iscredit"]) ? "+" : "-",
                                    value = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"])
                                });
                            }

                            result.CashBook.Add(new StoreViewEOD_CashItem()
                            {
                                display = "Cash Deposit",
                                value = result.Vochers.Sum(e => e.display == "Cash Deposit" ? e.value : 0)
                            });

                            result.CashBook.Add(new StoreViewEOD_CashItem()
                            {
                                display = "Closing",
                                value = maincashclosing
                            });

                            List<StoreViewEOD_CashItem> cashresult = new List<StoreViewEOD_CashItem>();
                            foreach (var citem in result.Vochers.GroupBy(e => e.parentdisplay))
                            {
                                cashresult.Add(new StoreViewEOD_CashItem()
                                {
                                    display = citem.Key,
                                    value = citem.Sum(e => e.value)
                                });
                                foreach (var item in citem)
                                {
                                    cashresult.Add(new StoreViewEOD_CashItem()
                                    {
                                        display = "  " + item.display,
                                        childvalue = item.value,
                                        parentdisplay = item.parentdisplay,
                                        symbol = item.symbol
                                    });
                                }
                            }
                            result.Vochers = cashresult;
                        }

                        if (reader.NextResult())
                        {
                            while (reader.Read())
                            {
                                var salevalue = reader["salevalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["salevalue"]);
                                var exchangevalue = reader["exchangevalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["exchangevalue"]);
                                var returnvalue = reader["returnvalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["returnvalue"]);
                                result.SectionSale.Add(new StoreViewEOD_SectionSaleItem()
                                {
                                    display = reader["name"].ToString(),
                                    soldvalue = salevalue,
                                    exchangevalue = exchangevalue,
                                    returnvalue = returnvalue,
                                    id = reader["salescounterid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["salescounterid"])
                                });
                            }
                            result.SectionSale = result.SectionSale.OrderByDescending(x => x.soldvalue).ToList();
                        }

                        if (reader.NextResult())
                        {
                            result.BarcodeSale = new List<StoreViewEOD_SectionSaleItem>();
                            while (reader.Read())
                            {
                                var salevalue = reader["salevalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["salevalue"]);
                                var exchangevalue = reader["exchangevalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["exchangevalue"]);
                                var returnvalue = reader["returnvalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["returnvalue"]);
                                result.BarcodeSale.Add(new StoreViewEOD_SectionSaleItem()
                                {
                                    display = reader["name"].ToString(),
                                    soldvalue = salevalue,
                                    exchangevalue = exchangevalue,
                                    returnvalue = returnvalue,
                                    id = reader["barcodeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["barcodeid"])
                                });
                            }
                            result.BarcodeSale = result.BarcodeSale.OrderByDescending(x => x.soldvalue).ToList();
                            if (result.BarcodeSale.Count == 1)
                                result.BarcodeSale = null;
                        }

                        //if (reader.NextResult())
                        //{
                        //    while (reader.Read())
                        //    {
                        //        var salevalue = reader["salevalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["salevalue"]);
                        //        var exchangevalue = reader["exchangevalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["exchangevalue"]);
                        //        var returnvalue = reader["returnvalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["returnvalue"]);
                        //        result.SectionSale.Add(new StoreViewEOD_SectionSaleItem()
                        //        {
                        //            display = reader["name"].ToString(),
                        //            soldvalue = salevalue,
                        //            exchangevalue = exchangevalue,
                        //            returnvalue = returnvalue,
                        //            id = reader["salescounterid"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["salescounterid"])
                        //        });
                        //    }
                        //    result.SectionSale = result.SectionSale.OrderByDescending(x => x.soldvalue).ToList();
                        //}

                        if (reader.NextResult())
                        {
                            result.ProductSale = new List<StoreViewEOD_SaleProductItem>();
                            while (reader.Read())
                            {
                                var salevalue = reader["salevalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["salevalue"]);
                                var exchangevalue = reader["exchangevalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["exchangevalue"]);
                                var returnvalue = reader["returnvalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["returnvalue"]);
                                var buyingcost = reader["buyingcost"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["buyingcost"]);
                                var sellingvalue = reader["sellingvalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["sellingvalue"]);
                                result.ProductSale.Add(new StoreViewEOD_SaleProductItem()
                                {
                                    display = reader["name"].ToString(),
                                    group = reader["groupname"].ToString(),
                                    soldvalue = salevalue,
                                    exchangevalue = exchangevalue,
                                    returnvalue = returnvalue,
                                    cost = buyingcost,
                                    id = reader["productid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productid"]),
                                    groupid = reader["groupid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["groupid"]),
                                    margin = sellingvalue - buyingcost
                                });
                            }
                            result.ProductSale = result.ProductSale.OrderByDescending(x => x.soldvalue).ToList();
                            result.ProductGroupSale = result.ProductSale.GroupBy(x => new { x.group, x.groupid }).ToList().ConvertAll(x => new StoreViewEOD_SaleProductItem()
                            {
                                display = x.Key.group,
                                soldvalue = x.Sum(y => y.soldvalue),
                                exchangevalue = x.Sum(y => y.exchangevalue),
                                returnvalue = x.Sum(y => y.returnvalue),
                                cost = x.Sum(y => y.cost),
                                margin = x.Sum(y => y.margin),
                                id = x.Key.groupid
                            });
                        }

                        if (reader.NextResult())
                        {
                            result.SalesmenSale = new List<StoreViewEOD_SectionSaleItem>();
                            while (reader.Read())
                            {
                                var salevalue = reader["salevalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["salevalue"]);
                                var exchangevalue = reader["exchangevalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["exchangevalue"]);
                                var returnvalue = reader["returnvalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["returnvalue"]);
                                var id = reader["salesmanid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["salesmanid"]);
                                result.SalesmenSale.Add(new StoreViewEOD_SectionSaleItem()
                                {
                                    display = reader["name"].ToString(),
                                    soldvalue = salevalue,
                                    exchangevalue = exchangevalue,
                                    returnvalue = returnvalue,
                                    id = id
                                });
                            }
                            result.SalesmenSale = result.SalesmenSale.OrderByDescending(x => x.soldvalue).ToList();
                        }


                        if (reader.NextResult())
                        {
                            while (reader.Read())
                            {
                                result.AgeStock.Add(new StoreViewEOD_AgeStock()
                                {
                                    age = reader["period"].ToString(),
                                    stockvalue = reader["value"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["value"]),
                                    stockqty = reader["qty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["qty"]),
                                    index = IndexOfDays(reader["period"] != DBNull.Value ? Convert.ToString(reader["period"]) : "")
                                });
                            }
                        }

                        if (reader.NextResult())
                        {
                            while (reader.Read())
                            {
                                var item = new StoreViewEOD_AgeStock()
                                {
                                    age = reader["period"].ToString(),
                                    soldvalue = reader["value"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["value"]),
                                    soldqty = reader["qty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["qty"]),
                                    index = IndexOfDays(reader["period"] != DBNull.Value ? Convert.ToString(reader["period"]) : "")
                                };

                                if (result.AgeStock.Exists(e => e.age == item.age))
                                {
                                    result.AgeStock.Find(e => e.age == item.age).soldqty = item.soldqty;
                                    result.AgeStock.Find(e => e.age == item.age).soldvalue = item.soldvalue;
                                }
                                else
                                {
                                    result.AgeStock.Add(item);
                                }
                            }
                        }
                        result.AgeStock.Sort(delegate (StoreViewEOD_AgeStock a, StoreViewEOD_AgeStock b)
                        {
                            return a.index.CompareTo(b.index);
                        });

                    }
                    conn.Close();
                }
                catch (Exception exp)
                {
                    if (conn != null)
                        conn.Close();
                }
                Data.RetailReportDataService IntrayData = new Data.RetailReportDataService() { Context = this.Context };
                result.CreditHistory = IntrayData.SearchCreditHistory(new Data.RetailReportCreteria()
                {
                    locationid = request.locationid,
                    fromdate = request.date.Date,
                    todate = request.date.Date
                });
                return result;
            }
            //System.IO.File.WriteAllText(@"e:\json.txt", Newtonsoft.Json.JsonConvert.SerializeObject(result));

        }
        public LocationItemSummary LocationSummary()
        {
            if (locationLocks.TryAdd("LocationSummary" + Context.UserID.ToString(), true))
            {
                try
                {
                    return InternalLocationSummary();
                }
                catch (Exception exp)
                {
                    throw exp;
                }
                finally
                {
                    bool bl;
                    locationLocks.TryRemove("LocationSummary" + Context.UserID.ToString(), out bl);
                }
            }
            else
            {
                throw new Exception("Already running");
            }
        }

        internal LocationItemSummary InternalLocationSummary()
        {
            LocationItemSummary locationItemSummary = new LocationItemSummary();

            InfyPOS.Data.RetailReportDataService retailReport = new Data.RetailReportDataService() { Context = this.Context };
            retailReport.Context = this.Context;
            var stockhistory = retailReport.GetLocationView();

            if (Context != null &&
                Context.accessiblelocation != null &&
                !Context.accessiblelocation.Contains(-1))
                stockhistory.RemoveAll(e => e.id != 0 && !Context.accessiblelocation.Contains(e.id));

            List<InfyPOS.Analytical.MobileInsight.LocationItem> result = stockhistory.ConvertAll(e =>
                new LocationItem()
                {
                    id = e.id,
                    name = e.name,
                    stock = e.stockcostvalue,
                    currentmonth = e.monthsales,
                    todaysales = e.todaysales,
                    todayinward = e.bundlevalue,
                    pendinginward = e.pendinginward,
                    monthbillcount = e.monthbillcount,
                    todaybillcount = e.todaybillcount,
                    monthsalesqty = e.monthsalesqty,
                    todaysalesqty = e.todaysalesqty
                });

            locationItemSummary.currentmonth = DateTimeNow.ToString("MMM-yyyy");
            locationItemSummary.items = result;


            if (Services.ConfigManager.Instance(Context.OrganizationID).Config.analyser_data_cache)
            {
                string query = "select locationid,sum(amount) amount from MV_SalesMonthly where month=@month and year=@year group by locationid";
                bool hasdata = false;
                var param = new SortedList<string, object>();
                param.Add("month", DateTimeNow.Month);
                param.Add("year", DateTimeNow.Year - 1);
                using (var reader = ExecuteReader(query, param))
                {
                    while (reader.Read())
                    {
                        hasdata = true;
                        decimal amount = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"]);
                        long locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                        if (result.Exists(x => x.id == locationid))
                        {
                            result.Find(x => x.id == locationid).lastyearmonth = amount;
                        }
                    }
                }
                if (!hasdata)
                {
                    int month = DateTimeNow.Month - 1;
                    int year = DateTimeNow.Year;
                    if (month == 0)
                    {
                        year -= 1;
                        month = 12;
                    }
                    param = new SortedList<string, object>();
                    param.Add("month", month);
                    param.Add("year", year);
                    using (var reader = ExecuteReader(query, param))
                    {
                        while (reader.Read())
                        {
                            hasdata = true;
                            decimal amount = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"]);
                            long locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                            if (result.Exists(x => x.id == locationid))
                            {
                                result.Find(x => x.id == locationid).lastyearmonth = amount;
                            }
                        }
                    }
                }
            }
            else
            {
                MonthlySummary lastmonthlySummary = BuildMonthlyLocationSummary(new SummaryRequst()
                {
                    attribute = "location",
                    month = DateTimeNow.Month,
                    year = DateTimeNow.Year - 1
                });
                locationItemSummary.lastyearmonth = new DateTime(DateTimeNow.Year - 1, DateTimeNow.Month, 1).ToString("MMM-yyyy");
                if (lastmonthlySummary.items.Sum(e => e.amount) == 0)
                {
                    int month = DateTimeNow.Month - 1;
                    int year = DateTimeNow.Year;
                    if (month == 0)
                    {
                        year -= 1;
                        month = 12;
                    }
                    locationItemSummary.lastyearmonth = new DateTime(year, month, 1).ToString("MMM-yyyy");
                    lastmonthlySummary = BuildMonthlyLocationSummary(new SummaryRequst()
                    {
                        attribute = "location",
                        month = month,
                        year = year
                    });
                }
                lastmonthlySummary.items.ForEach(y =>
                {
                    if (result.Exists(x => x.id == y.id))
                    {
                        result.Find(x => x.id == y.id).lastyearmonth = y.amount;
                    }
                });
            }

            result.RemoveAll(e => e.currentmonth == 0 && e.stock == 0);
            if (result.Exists(e => e.name == "All Store"))
                result.Find(e => e.name == "All Store").lastyearmonth = result.Sum(e => e.lastyearmonth);

            locationItemSummary.items.ForEach(x =>
            {
                x.todayinward = Math.Round(x.todayinward);
            });
            return locationItemSummary;
        }

        public class SummaryRequst : Common.BaseEntity
        {
            [JsonIgnore]
            public DateTime date { get; set; }
            public DateTime? fromdate { get; set; }
            public DateTime? todate { get; set; }
            public long supplierid { get; set; }
            public long productid { get; set; }
            public long locationid { get; set; }
            public int month { get; set; }
            public int year { get; set; }
            public string attribute { get; set; }
            public RequestType requestType { get; set; }
            public enum RequestType
            {
                Monthly,
                Daily
            }
        }

        public DailySummary BuildDailySummary(SummaryRequst request)
        {
            DailySummary monthlySummary = new DailySummary();

            DateTime fromdate = request.date;
            DateTime todate = fromdate.AddMonths(1).AddDays(1);

            string productsummary = @"select sum(billitems.receivable) as amount,
sum(billitems.qty) as qty,billdate::date as day from bill
inner join billitems on billitems.billid = bill.id and billitems.isactive=true
inner join stock on stock.id = stockid 
where bill.billdate >= @fromdate and bill.billdate < @todate and bill.billtype != 9 
and bill.isactive = true and bill.locationid = @locationid
group by billdate::date
order by billdate::date";

            string brandsummary = @"
select sum(billitems.receivable) as amount,
sum(billitems.qty) as qty,billdate::date as day from bill
inner join billitems on billitems.billid = bill.id and billitems.isactive=true
inner join stock on stock.id = stockid 
left outer join items on items.id = stock.itemid
where bill.billdate >= @fromdate and bill.billdate < @todate and bill.billtype != 9 
and bill.isactive = true and bill.locationid = @locationid
group by billdate::date
order by billdate::date";

            string locationsummary = @"select sum(billitems.receivable) as amount,
sum(billitems.qty) as qty,billdate::date as day  from bill
inner join billitems on billitems.billid = bill.id and billitems.isactive=true
where bill.billdate >= @fromdate and bill.billdate < @todate and bill.billtype != 9 
and bill.isactive = true and bill.locationid = @locationid
group by billdate::date
order by billdate::date";

            var fullquery = productsummary;
            if (request.attribute == "location")
                fullquery = locationsummary;
            else if (request.attribute == "brand")
                fullquery = brandsummary;
            if (request.locationid <= 0)
                fullquery = fullquery.Replace("and salablegoods.locationid=@locationid", "").Replace("and bill.locationid = @locationid", "");

            var baseData = new Common.BaseData() { Context = this.Context };
            using (var connection = baseData.CreateConnection())
            {
                connection.Open();
                NpgsqlCommand command = new NpgsqlCommand(fullquery, connection);
                command.Parameters.Add("current_date", NpgsqlTypes.NpgsqlDbType.Date).Value = DateTimeNow.Date;
                command.Parameters.Add("fromdate", NpgsqlTypes.NpgsqlDbType.Date).Value = fromdate;
                command.Parameters.Add("todate", NpgsqlTypes.NpgsqlDbType.Date).Value = todate;
                command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = request.locationid;
                command.Parameters.Add("productid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = request.productid;
                command.Parameters.Add("supplierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = request.supplierid;
                command.CommandTimeout = 10000;

                using (var reader = command.ExecuteReader())
                {
                    monthlySummary.items = new List<DailySaleData>();
                    while (reader.Read())
                    {
                        monthlySummary.items.Add(new DailySaleData()
                        {
                            date = reader["day"] != DBNull.Value ? Convert.ToDateTime(reader["day"]) : DateTime.MinValue,
                            qty = reader["qty"] != DBNull.Value ? Convert.ToDecimal(reader["qty"]) : 0,
                            amount = reader["amount"] != DBNull.Value ? Convert.ToDecimal(reader["amount"]) : 0,
                        });
                    }
                }
                connection.Close();
            }
            return monthlySummary;
        }


        public class FinanceSummary
        {
            public List<NameAndValue> PurchaseValues { get; set; }
            public List<NameAndValue> PurchaseReturn { get; set; }
            public List<NameAndValue> SaleValues { get; set; }
            public List<AgeByValue> SupplierPaid { get; set; }
            public List<AgeByValue> SupplierPending { get; set; }
            public List<NameAndValue> Summary { get; set; }
            public List<NameAndValue> DebitNotes { get; set; }
            public List<NameAndValue> BankPayments { get; set; }
            public List<NameAndValue> Expenses { get; set; }
            public List<NameAndValue> PettyCashExpenses { get; set; }
            public List<NameAndValue> Stock { get; internal set; }

            public void Process()
            {
                Summary = new List<NameAndValue>();
                Summary.Add(new NameAndValue()
                {
                    name = "Purchase",
                    value = PurchaseValues.Sum(x => x.value)
                });
                Summary.Add(new NameAndValue()
                {
                    name = "Purchase Return",
                    value = PurchaseReturn.Sum(x => x.value)
                });
                Summary.Add(new NameAndValue()
                {
                    name = "Sales",
                    value = SaleValues.Sum(x => x.value)
                });
                Summary.Add(new NameAndValue()
                {
                    name = "Stock",
                    value = Stock.Sum(x => x.value)
                });
                Summary.Add(new NameAndValue()
                {
                    name = "Supplier Paid",
                    value = SupplierPaid.Sum(x => x.amount_30d + x.amount_90d + x.amount_180d + x.amount_180plus)
                });
                Summary.Add(new NameAndValue()
                {
                    name = "Supplier Pending",
                    value = SupplierPending.Sum(x => x.amount_30d + x.amount_90d + x.amount_180d + x.amount_180plus)
                });
                Summary.Add(new NameAndValue()
                {
                    name = "Supplier Pending",
                    value = SupplierPending.Sum(x => x.amount_30d + x.amount_90d + x.amount_180d + x.amount_180plus)
                });
                Summary.Add(new NameAndValue()
                {
                    name = "Debit Notes",
                    value = DebitNotes.Sum(x => x.value)
                });
                Summary.Add(new NameAndValue()
                {
                    name = "Bank Payments",
                    value = BankPayments.Sum(x => x.value)
                });
                Summary.Add(new NameAndValue()
                {
                    name = "Expenses",
                    value = Expenses.Sum(x => x.value)
                });
                Summary.Add(new NameAndValue()
                {
                    name = "PettyCash Expenses",
                    value = PettyCashExpenses.Sum(x => x.value)
                });
            }
            public class AgeByValue
            {
                public string name { get; set; }
                public decimal amount_30d { get; set; }
                public decimal amount_90d { get; set; }
                public decimal amount_180d { get; set; }
                public decimal amount_180plus { get; set; }
            }
            public class NameAndValue
            {
                public string name { get; set; }
                public decimal value { get; set; }
            }
            public class InternalValue
            {
                public string suppliername { get; set; }
                public string age { get; set; }
                public string sectionname { get; set; }
                public string bsectionname { get; set; }
                public decimal amount { get; set; }
            }
        }

        public FinanceSummary BuildInvetorySummary(long locationid, DateTime from, DateTime to)
        {
            DateTime aviladate = DateTime.MinValue;
            using (var reader = ExecuteReader("select lastDateValue from Cache_View where viewname='mv_purchaseproductdaily' limit 1", null))
            {
                if (reader.Read() && !reader.IsDBNull(0))
                    aviladate = reader.GetDateTime(0);
            }
            if (to <= aviladate)
            {
                return BuildInvetorySummaryCache(locationid, from, to);
            }
            else if (from > aviladate)
            {
                return BuildInvetorySummaryDirect(locationid, from, to);
            }
            else
            {
                var fromcache = BuildInvetorySummaryCache(locationid, from, aviladate);
                var fromdirect = BuildInvetorySummaryDirect(locationid, aviladate.AddDays(1), to);
                fromdirect.PurchaseValues.ForEach(x =>
                {
                    if (fromcache.PurchaseValues.Exists(y => y.name == x.name))
                    {
                        fromcache.PurchaseValues.Find(y => y.name == x.name).value += x.value;
                    }
                    else
                    {
                        fromcache.PurchaseValues.Add(x);
                    }
                });
                return fromcache;
            }
        }
        public FinanceSummary BuildInvetorySummaryCache(long locationid, DateTime fromdate, DateTime todate)
        {
            FinanceSummary response = new FinanceSummary();

            string fullquery = @"
select sum(purchase.amount) amount,referencelist.name sectionname,barcodesection.name barcodesection from mv_purchaseproductdaily purchase 
inner join products on products.id = purchase.productid
left outer join referencelist on referencelist.id = salescounterid
left outer join referencelist barcodesection on barcodesection.id = products.barcodeid
where lrentrydate>=@fromdate and lrentrydate<@todate
group by referencelist.name,barcodesection.name;

select sum(sales.salevalue) amount,referencelist.name sectionname,barcodesection.name barcodesection from mv_salesdailybyproduct sales
inner join products on products.id = sales.productid
left outer join referencelist on referencelist.id = salescounterid
left outer join referencelist barcodesection on barcodesection.id = products.barcodeid
where billdate>=@fromdate and billdate<@todate and sales.locationid=@locationid
group by referencelist.name,barcodesection.name;";

            if (locationid == 0)
            {
                fullquery = fullquery.Replace("and sales.locationid=@locationid", "");
                fullquery = fullquery.Replace("and purchase.locationid=@locationid", "");
            }

            var baseData = new Common.BaseData() { Context = this.Context };


            using (var connection = baseData.CreateConnection())
            {
                connection.Open();
                NpgsqlCommand command = new NpgsqlCommand(fullquery, connection);
                command.Parameters.Add("current_date", NpgsqlTypes.NpgsqlDbType.Date).Value = DateTimeNow.Date;
                command.Parameters.Add("fromdate", NpgsqlTypes.NpgsqlDbType.Date).Value = fromdate;
                command.Parameters.Add("todate", NpgsqlTypes.NpgsqlDbType.Date).Value = todate.AddDays(1);
                command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = locationid;
                command.CommandTimeout = 10000;
                using (var reader = command.ExecuteReader())
                {
                    List<FinanceSummary.InternalValue> internalValues = new List<FinanceSummary.InternalValue>();
                    while (reader.Read())
                    {
                        internalValues.Add(new FinanceSummary.InternalValue()
                        {
                            sectionname = reader["sectionname"] != DBNull.Value ? Convert.ToString(reader["sectionname"]) : "",
                            bsectionname = reader["barcodesection"] != DBNull.Value ? Convert.ToString(reader["barcodesection"]) : "",
                            amount = reader["amount"] != DBNull.Value ? Convert.ToDecimal(reader["amount"]) : 0
                        });
                    }
                    if (internalValues.Exists(x => string.IsNullOrEmpty(x.bsectionname)))
                    {
                        response.PurchaseValues = internalValues.GroupBy(x => x.bsectionname).ToList().ConvertAll(y => new FinanceSummary.NameAndValue()
                        {
                            name = y.Key,
                            value = y.Sum(z => z.amount)
                        });
                    }
                    else
                    {
                        response.PurchaseValues = internalValues.GroupBy(x => x.sectionname).ToList().ConvertAll(y => new FinanceSummary.NameAndValue()
                        {
                            name = y.Key,
                            value = y.Sum(z => z.amount)
                        });
                    }
                    if (reader.NextResult())
                    {
                        internalValues.Clear();
                        while (reader.Read())
                        {
                            internalValues.Add(new FinanceSummary.InternalValue()
                            {
                                sectionname = reader["sectionname"] != DBNull.Value ? Convert.ToString(reader["sectionname"]) : "",
                                bsectionname = reader["barcodesection"] != DBNull.Value ? Convert.ToString(reader["barcodesection"]) : "",
                                amount = reader["amount"] != DBNull.Value ? Convert.ToDecimal(reader["amount"]) : 0
                            });
                        }
                        if (internalValues.Exists(x => string.IsNullOrEmpty(x.bsectionname)))
                        {
                            response.SaleValues = internalValues.GroupBy(x => x.bsectionname).ToList().ConvertAll(y => new FinanceSummary.NameAndValue()
                            {
                                name = y.Key,
                                value = y.Sum(z => z.amount)
                            });
                        }
                        else
                        {
                            response.SaleValues = internalValues.GroupBy(x => x.sectionname).ToList().ConvertAll(y => new FinanceSummary.NameAndValue()
                            {
                                name = y.Key,
                                value = y.Sum(z => z.amount)
                            });
                        }
                    }
                }
                connection.Close();
            }
            return response;
        }
        public FinanceSummary BuildInvetorySummaryDirect(long locationid, DateTime fromdate, DateTime todate)
        {
            FinanceSummary response = new FinanceSummary();

            string fullquery = @"--purchase
with purchase as
(
select sum(lrinvoiceitems.amount) amount,
lrinvoiceitems.productid
from lrinvoice  
inner join lrinvoiceitems on lrinvoiceitems.lrinvoiceid = lrinvoice.id and lrinvoiceitems.isactive=true
left outer join lrentry on lrentry.id = lrinvoice.lrid
left outer join referencelist on referencelist.id = lrsectionid
where lrinvoice.entrydate>=@fromdate and lrinvoice.entrydate<@todate and lrinvoice.isactive=true
group by lrinvoiceitems.productid
)
select sum(purchase.amount) amount,referencelist.name sectionname,barcodesection.name barcodesection from purchase
inner join products on products.id = purchase.productid
left outer join referencelist on referencelist.id = salescounterid
left outer join referencelist barcodesection on barcodesection.id = products.barcodeid
group by referencelist.name,barcodesection.name;

--sales
with sales as
(
select sum(billitems.receivable) salevalue,billitems.productid from bill
inner join billitems on billitems.billid = bill.id and billitems.isactive=true
where bill.billdate>=@fromdate and bill.billdate<@todate
and billtype != 9 and bill.isactive=true
and bill.locationid = @locationid
group by billitems.productid
)
select sum(sales.salevalue) amount,referencelist.name sectionname,barcodesection.name barcodesection from sales
inner join products on products.id = sales.productid
left outer join referencelist on referencelist.id = salescounterid
left outer join referencelist barcodesection on barcodesection.id = products.barcodeid
group by referencelist.name,barcodesection.name;";

            if (locationid == 0)
            {
                fullquery = fullquery.Replace("and supplierpayment.locationid=@locationid", "");
                fullquery = fullquery.Replace("and lrinvoicereturn.locationid=@locationid", "");
                fullquery = fullquery.Replace("and voucher.locationid=@locationid", "");
                fullquery = fullquery.Replace("and bill.locationid = @locationid", "");
                fullquery = fullquery.Replace("and lrentry.entrylocationid=@locationid", "");
            }

            var baseData = new Common.BaseData() { Context = this.Context };

            using (var connection = baseData.CreateConnection())
            {
                connection.Open();
                NpgsqlCommand command = new NpgsqlCommand(fullquery, connection);
                command.Parameters.Add("current_date", NpgsqlTypes.NpgsqlDbType.Date).Value = DateTimeNow.Date;
                command.Parameters.Add("fromdate", NpgsqlTypes.NpgsqlDbType.Date).Value = fromdate;
                command.Parameters.Add("todate", NpgsqlTypes.NpgsqlDbType.Date).Value = todate;
                command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = locationid;
                command.CommandTimeout = 10000;
                using (var reader = command.ExecuteReader())
                {
                    List<FinanceSummary.InternalValue> internalValues = new List<FinanceSummary.InternalValue>();
                    while (reader.Read())
                    {
                        internalValues.Add(new FinanceSummary.InternalValue()
                        {
                            sectionname = reader["sectionname"] != DBNull.Value ? Convert.ToString(reader["sectionname"]) : "",
                            bsectionname = reader["barcodesection"] != DBNull.Value ? Convert.ToString(reader["barcodesection"]) : "",
                            amount = reader["amount"] != DBNull.Value ? Convert.ToDecimal(reader["amount"]) : 0
                        });
                    }
                    if (internalValues.Exists(x => string.IsNullOrEmpty(x.bsectionname)))
                    {
                        response.PurchaseValues = internalValues.GroupBy(x => x.bsectionname).ToList().ConvertAll(y => new FinanceSummary.NameAndValue()
                        {
                            name = y.Key,
                            value = y.Sum(z => z.amount)
                        });
                    }
                    else
                    {
                        response.PurchaseValues = internalValues.GroupBy(x => x.sectionname).ToList().ConvertAll(y => new FinanceSummary.NameAndValue()
                        {
                            name = y.Key,
                            value = y.Sum(z => z.amount)
                        });
                    }
                    if (reader.NextResult())
                    {
                        internalValues.Clear();
                        while (reader.Read())
                        {
                            internalValues.Add(new FinanceSummary.InternalValue()
                            {
                                sectionname = reader["sectionname"] != DBNull.Value ? Convert.ToString(reader["sectionname"]) : "",
                                bsectionname = reader["barcodesection"] != DBNull.Value ? Convert.ToString(reader["barcodesection"]) : "",
                                amount = reader["amount"] != DBNull.Value ? Convert.ToDecimal(reader["amount"]) : 0
                            });
                        }
                        if (internalValues.Exists(x => string.IsNullOrEmpty(x.bsectionname)))
                        {
                            response.SaleValues = internalValues.GroupBy(x => x.bsectionname).ToList().ConvertAll(y => new FinanceSummary.NameAndValue()
                            {
                                name = y.Key,
                                value = y.Sum(z => z.amount)
                            });
                        }
                        else
                        {
                            response.SaleValues = internalValues.GroupBy(x => x.sectionname).ToList().ConvertAll(y => new FinanceSummary.NameAndValue()
                            {
                                name = y.Key,
                                value = y.Sum(z => z.amount)
                            });
                        }
                    }
                }
                connection.Close();
            }
            return response;
        }

        public FinanceSummary BuildFinanceSummary(SummaryRequst requst)
        {
            FinanceSummary response = BuildInvetorySummary(requst.locationid, requst.fromdate.Value, requst.todate.Value);
            requst.fromdate = requst.fromdate.Value.Date;
            requst.todate = requst.todate.Value.AddDays(1);

            string fullquery = @"
--stock
select barcodesection.name,sum(stockqty*productprice) value  from mw_dailstockdetails 
inner join products on products.id = mw_dailstockdetails.productid
left outer join referencelist barcodesection on barcodesection.id = products.barcodeid
group by  barcodesection.name;

/*--purchase
 select sum(lrinvoice.totalamount) amount,
 referencelist.name sectionname,
 barcodesection.name barcodesection
 from lrinvoice  
 left outer join lrentry on lrentry.id = lrinvoice.lrid
 left outer join referencelist on referencelist.id = lrsectionid
 left outer join referencelist barcodesection on barcodesection.id = referencelist.productgroup
 where lrinvoice.entrydate>=@fromdate and lrinvoice.entrydate<@todate and lrinvoice.isactive=true
 and lrentry.entrylocationid = @locationid
 group by referencelist.name,barcodesection.name;
 
 --sales
 with sales as
 (
 select sum(billitems.receivable) salevalue,billitems.productid from bill
 inner join billitems on billitems.billid = bill.id and billitems.isactive=true
 where bill.billdate>=@fromdate and bill.billdate<@todate
 and billtype != 9 and bill.isactive=true
 and bill.locationid = @locationid
 group by billitems.productid
 )
 select sum(sales.salevalue) amount,referencelist.name sectionname,barcodesection.name barcodesection from sales
 inner join products on products.id = sales.productid
 left outer join referencelist on referencelist.id = salescounterid
 left outer join referencelist barcodesection on barcodesection.id = referencelist.productgroup
 group by referencelist.name,barcodesection.name;
*/

--purchasereturn
select 
referencelist.name sectionname,barcodesection.name barcodesection,
sum(lrinvoicereturnitems.amount) amount
from lrinvoicereturn 
inner join lrinvoicereturnitems on lrinvoicereturnitems.lrinvoicereturnid = lrinvoicereturn.id
inner join products on products.id = lrinvoicereturnitems.productid
left outer join referencelist on referencelist.id = salescounterid
left outer join referencelist barcodesection on barcodesection.id = products.barcodeid
where lrinvoicereturn.isactive = true 
and lrinvoicereturn.returndate>=@fromdate and lrinvoicereturn.returndate<@todate
and lrinvoicereturn.locationid=@locationid
group by referencelist.name ,barcodesection.name;

--bank payments
with bankpayments
as
(
select voucher.id,ledger.displayname,amount from voucher
inner join ledger on ledger.id = adjustmentledgerid and ledger.typeid=7
where voucherdate>=@fromdate and voucherdate<@todate
and voucher.locationid=@locationid
and vouchertype=1
and adjustmentcredit=true
and voucher.isactive=true
)
select ledger.displayname,sum(voucheritems.amount) amount from bankpayments
inner join voucheritems on voucheritems.voucherid = bankpayments.id 
and voucheritems.isactive=true and voucheritems.iscredit =false
inner join ledger on ledger.id = voucheritems.ledgerid
where ledger.typeid=12
group by ledger.displayname;

--expense
select ledger.displayname,sum(voucheritems.amount) amount from voucher 
inner join voucheritems on voucheritems.voucherid = voucher.id and voucheritems.isactive=true
inner join ledger on ledger.id = voucheritems.ledgerid and ledger.typeid!=16
and ledger.typeid!=30
where voucherdate>=@fromdate and voucherdate<@todate
and voucher.locationid=@locationid
and voucher.vouchertype=3
group by ledger.displayname;
 
--petty cash 
select ledger.displayname,sum(voucheritems.amount) amount from voucher 
inner join voucheritems on voucheritems.voucherid = voucher.id and voucheritems.isactive=true
inner join ledger on ledger.id = voucheritems.ledgerid and ledger.typeid!=16
where voucherdate>=@fromdate and voucherdate<@todate
and voucher.locationid=@locationid
and voucher.vouchertype=15
group by ledger.displayname;

--debit notes
select 
suppliercompany.name name,sum(lrinvoicereturn.total) total,
sum(paidinvoice.paidamount) paidamount
from lrinvoicereturn 
left outer join (select sum(supplierpaymentinvoice.paidamount) paidamount,invoiceid
from supplierpaymentinvoice 
inner join supplierpayment on supplierpayment.id = supplierpaymentinvoice.supplierpaymentid  where supplierpaymentinvoice.isactive = true 
and supplierpaymentinvoice.hascancel=false 
and (islrinvoicereturn = true or iscreditnote = true)
group by invoiceid) paidinvoice on paidinvoice.invoiceid = lrinvoicereturn.id
inner join suppliercompany on suppliercompany.id = lrinvoicereturn.suppliercompanyid
where lrinvoicereturn.isactive = true 
and lrinvoicereturn.settled = false
and lrinvoicereturn.returndate>=@fromdate and lrinvoicereturn.returndate<@todate
and lrinvoicereturn.locationid=@locationid
and lrinvoicereturn.total-abs(paidinvoice.paidamount)>0
group by suppliercompany.name;

--supplierpayments
with supplierpaymentinvoices
as
(
select sum(supplierpaymentinvoice.paidamount) paidamount,
invoiceid
from supplierpayment
inner join supplierpaymentinvoice on supplierpaymentinvoice.supplierpaymentid = supplierpayment.id
and islrinvoicereturn = false and iscreditnote = false and supplierpaymentinvoice.isactive = true
where supplierpayment.paymentdate>=@fromdate and supplierpayment.paymentdate<@todate
and supplierpayment.locationid=@locationid
group by invoiceid
),paidlist as
(
select supplierpaymentinvoices.paidamount,
lrinvoice.suppliercompanyid,
case 
when EXTRACT(day from current_date - lrinvoice.entrydate)<30 then '30D'
when EXTRACT(day from current_date - lrinvoice.entrydate)<90 then '90D'
when EXTRACT(day from current_date - lrinvoice.entrydate)<180 then '180D'
else '180D+'
end Age,
lrentry.lrsectionid
from supplierpaymentinvoices
inner join lrinvoice  on lrinvoice.id = supplierpaymentinvoices.invoiceid
left outer join lrentry on lrentry.id = lrinvoice.lrid
)
select paidlist.paidamount,Age,
suppliercompany.name suppliername, 
referencelist.name sectionname,barcodesection.name barcodesection 
from paidlist
left outer join suppliercompany on suppliercompany.id = paidlist.suppliercompanyid								
left outer join referencelist on referencelist.id = lrsectionid
left outer join referencelist barcodesection on barcodesection.id = referencelist.productgroup;

--supplier pending
with supplierpending 
as
(
select 
lrinvoice.suppliercompanyid,
case 
when EXTRACT(day from current_date - lrinvoice.entrydate)<30 then '30D'
when EXTRACT(day from current_date - lrinvoice.entrydate)<90 then '90D'
when EXTRACT(day from current_date - lrinvoice.entrydate)<180 then '180D'
else '180D+'
end Age,
lrentry.lrsectionid,
sum(lrinvoice.totalamount-tdsamount-coalesce(lrinvoice.presettled,0)-coalesce(paidinvoice.paidamount,0)) balance
from lrinvoice 
left outer join lrentry on lrentry.id = lrinvoice.lrid
left outer join (select sum(paidamount) paidamount,invoiceid
from supplierpaymentinvoice where 
islrinvoicereturn = false and
iscreditnote = false
and isactive = true
group by invoiceid) paidinvoice on paidinvoice.invoiceid = lrinvoice.id
where lrinvoice.isactive = true and lrinvoice.settled = false
and lrinvoice.totalamount-lrinvoice.presettled-(coalesce(paidinvoice.paidamount,0))>0
and lrinvoice.entrydate>=@fromdate and lrinvoice.entrydate<@todate
and lrentry.entrylocationid=@locationid
group by suppliercompanyid,lrentry.lrsectionid,
case 
when EXTRACT(day from current_date - lrinvoice.entrydate)<30 then '30D'
when EXTRACT(day from current_date - lrinvoice.entrydate)<90 then '90D'
when EXTRACT(day from current_date - lrinvoice.entrydate)<180 then '180D'
else '180D+'
end
)
select supplierpending.*,suppliercompany.name suppliername,referencelist.name sectionname,barcodesection.name barcodesection from supplierpending
left outer join suppliercompany on suppliercompany.id = supplierpending.suppliercompanyid
left outer join referencelist on referencelist.id = lrsectionid
left outer join referencelist barcodesection on barcodesection.id = referencelist.productgroup
order by suppliercompany.name,referencelist.name";

            if (requst.locationid == 0)
            {
                fullquery = fullquery.Replace("and supplierpayment.locationid=@locationid", "");
                fullquery = fullquery.Replace("and lrinvoicereturn.locationid=@locationid", "");
                fullquery = fullquery.Replace("and voucher.locationid=@locationid", "");
                fullquery = fullquery.Replace("and bill.locationid = @locationid", "");
                fullquery = fullquery.Replace("and lrentry.entrylocationid=@locationid", "");
            }

            var baseData = new Common.BaseData() { Context = this.Context };
            using (var connection = baseData.CreateConnection())
            {
                connection.Open();
                NpgsqlCommand command = new NpgsqlCommand(fullquery, connection);
                command.Parameters.Add("current_date", NpgsqlTypes.NpgsqlDbType.Date).Value = DateTimeNow.Date;
                command.Parameters.Add("fromdate", NpgsqlTypes.NpgsqlDbType.Date).Value = requst.fromdate;
                command.Parameters.Add("todate", NpgsqlTypes.NpgsqlDbType.Date).Value = requst.todate;
                command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = requst.locationid;
                command.CommandTimeout = 10000;
                using (var reader = command.ExecuteReader())
                {
                    response.Stock = new List<FinanceSummary.NameAndValue>();
                    while (reader.Read())
                    {
                        response.Stock.Add(new FinanceSummary.NameAndValue()
                        {
                            name = reader["name"] != DBNull.Value ? Convert.ToString(reader["name"]) : "",
                            value = reader["value"] != DBNull.Value ? Convert.ToDecimal(reader["value"]) : 0
                        });
                    }
                    List<FinanceSummary.InternalValue> internalValues = new List<FinanceSummary.InternalValue>();
                    //if (reader.NextResult())
                    //{
                    //    internalValues.Clear();
                    //    while (reader.Read())
                    //    {
                    //        internalValues.Add(new FinanceSummary.InternalValue()
                    //        {
                    //            sectionname = reader["sectionname"] != DBNull.Value ? Convert.ToString(reader["sectionname"]) : "",
                    //            bsectionname = reader["barcodesection"] != DBNull.Value ? Convert.ToString(reader["barcodesection"]) : "",
                    //            amount = reader["amount"] != DBNull.Value ? Convert.ToDecimal(reader["amount"]) : 0
                    //        });
                    //    }
                    //    if (internalValues.Exists(x => string.IsNullOrEmpty(x.bsectionname)))
                    //    {
                    //        response.PurchaseValues = internalValues.GroupBy(x => x.bsectionname).ToList().ConvertAll(y => new FinanceSummary.NameAndValue()
                    //        {
                    //            name = y.Key,
                    //            value = y.Sum(z => z.amount)
                    //        });
                    //    }
                    //    else
                    //    {
                    //        response.PurchaseValues = internalValues.GroupBy(x => x.sectionname).ToList().ConvertAll(y => new FinanceSummary.NameAndValue()
                    //        {
                    //            name = y.Key,
                    //            value = y.Sum(z => z.amount)
                    //        });
                    //    }
                    //}

                   
                    //if (reader.NextResult())
                    //{
                    //    internalValues.Clear();
                    //    while (reader.Read())
                    //    {
                    //        internalValues.Add(new FinanceSummary.InternalValue()
                    //        {
                    //            sectionname = reader["sectionname"] != DBNull.Value ? Convert.ToString(reader["sectionname"]) : "",
                    //            bsectionname = reader["barcodesection"] != DBNull.Value ? Convert.ToString(reader["barcodesection"]) : "",
                    //            amount = reader["amount"] != DBNull.Value ? Convert.ToDecimal(reader["amount"]) : 0
                    //        });
                    //    }
                    //    if (internalValues.Exists(x => string.IsNullOrEmpty(x.bsectionname)))
                    //    {
                    //        response.SaleValues = internalValues.GroupBy(x => x.bsectionname).ToList().ConvertAll(y => new FinanceSummary.NameAndValue()
                    //        {
                    //            name = y.Key,
                    //            value = y.Sum(z => z.amount)
                    //        });
                    //    }
                    //    else
                    //    {
                    //        response.SaleValues = internalValues.GroupBy(x => x.sectionname).ToList().ConvertAll(y => new FinanceSummary.NameAndValue()
                    //        {
                    //            name = y.Key,
                    //            value = y.Sum(z => z.amount)
                    //        });
                    //    }
                    //}

                    if (reader.NextResult())
                    {
                        response.PurchaseReturn = new List<FinanceSummary.NameAndValue>();

                        internalValues.Clear();
                        while (reader.Read())
                        {
                            internalValues.Add(new FinanceSummary.InternalValue()
                            {
                                sectionname = reader["sectionname"] != DBNull.Value ? Convert.ToString(reader["sectionname"]) : "",
                                bsectionname = reader["barcodesection"] != DBNull.Value ? Convert.ToString(reader["barcodesection"]) : "",
                                amount = reader["amount"] != DBNull.Value ? Convert.ToDecimal(reader["amount"]) : 0
                            });
                        }

                        if (internalValues.Exists(x => string.IsNullOrEmpty(x.bsectionname)))
                        {
                            response.PurchaseReturn = internalValues.GroupBy(x => x.bsectionname).ToList().ConvertAll(y => new FinanceSummary.NameAndValue()
                            {
                                name = y.Key,
                                value = y.Sum(z => z.amount)
                            });
                        }
                        else
                        {
                            response.PurchaseReturn = internalValues.GroupBy(x => x.sectionname).ToList().ConvertAll(y => new FinanceSummary.NameAndValue()
                            {
                                name = y.Key,
                                value = y.Sum(z => z.amount)
                            });
                        }
                    }

                    if (reader.NextResult())
                    {
                        response.BankPayments = new List<FinanceSummary.NameAndValue>();
                        while (reader.Read())
                        {
                            response.BankPayments.Add(new FinanceSummary.NameAndValue()
                            {
                                name = reader["displayname"] != DBNull.Value ? Convert.ToString(reader["displayname"]) : "",
                                value = reader["amount"] != DBNull.Value ? Convert.ToDecimal(reader["amount"]) : 0
                            });
                        }
                    }

                    if (reader.NextResult())
                    {
                        response.Expenses = new List<FinanceSummary.NameAndValue>();
                        while (reader.Read())
                        {
                            response.Expenses.Add(new FinanceSummary.NameAndValue()
                            {
                                name = reader["displayname"] != DBNull.Value ? Convert.ToString(reader["displayname"]) : "",
                                value = reader["amount"] != DBNull.Value ? Convert.ToDecimal(reader["amount"]) : 0
                            });
                        }
                    }

                    if (reader.NextResult())
                    {
                        response.PettyCashExpenses = new List<FinanceSummary.NameAndValue>();
                        while (reader.Read())
                        {
                            response.PettyCashExpenses.Add(new FinanceSummary.NameAndValue()
                            {
                                name = reader["displayname"] != DBNull.Value ? Convert.ToString(reader["displayname"]) : "",
                                value = reader["amount"] != DBNull.Value ? Convert.ToDecimal(reader["amount"]) : 0
                            });
                        }
                    }

                    if (reader.NextResult())
                    {
                        response.DebitNotes = new List<FinanceSummary.NameAndValue>();
                        while (reader.Read())
                        {
                            var t = reader["total"] != DBNull.Value ? Convert.ToDecimal(reader["total"]) : 0;
                            var p = reader["paidamount"] != DBNull.Value ? Convert.ToDecimal(reader["paidamount"]) : 0;
                            response.DebitNotes.Add(new FinanceSummary.NameAndValue()
                            {
                                name = reader["name"] != DBNull.Value ? Convert.ToString(reader["name"]) : "",
                                value = t - Math.Abs(p)
                            });
                        }
                    }

                    if (reader.NextResult())
                    {
                        internalValues.Clear();
                        while (reader.Read())
                        {
                            internalValues.Add(new FinanceSummary.InternalValue()
                            {
                                age = reader["age"] != DBNull.Value ? Convert.ToString(reader["age"]) : "",
                                suppliername = reader["suppliername"] != DBNull.Value ? Convert.ToString(reader["suppliername"]) : "",
                                sectionname = reader["sectionname"] != DBNull.Value ? Convert.ToString(reader["sectionname"]) : "",
                                bsectionname = reader["barcodesection"] != DBNull.Value ? Convert.ToString(reader["barcodesection"]) : "",
                                amount = reader["paidamount"] != DBNull.Value ? Convert.ToDecimal(reader["paidamount"]) : 0
                            });
                        }
                        //if (internalValues.Exists(x => string.IsNullOrEmpty(x.bsectionname)))
                        //{
                        //    response.SupplierPaid = internalValues.GroupBy(x => x.bsectionname).ToList().ConvertAll(y => new FinanceSummary.AgeByValue()
                        //    {
                        //        name = y.Key,
                        //        amount_30d = y.Sum(z=>z.age == "30D"?z.amount:0),
                        //        amount_90d = y.Sum(z => z.age == "90D" ? z.amount : 0),
                        //        amount_180d = y.Sum(z => z.age == "180D" ? z.amount : 0),
                        //        amount_180plus = y.Sum(z => z.age == "180D+" ? z.amount : 0),
                        //    });
                        //}
                        //else
                        //{
                        response.SupplierPaid = internalValues.GroupBy(x => x.sectionname).ToList().ConvertAll(y => new FinanceSummary.AgeByValue()
                        {
                            name = y.Key,
                            amount_30d = y.Sum(z => z.age == "30D" ? z.amount : 0),
                            amount_90d = y.Sum(z => z.age == "90D" ? z.amount : 0),
                            amount_180d = y.Sum(z => z.age == "180D" ? z.amount : 0),
                            amount_180plus = y.Sum(z => z.age == "180D+" ? z.amount : 0),
                        });
                        //}
                    }

                    if (reader.NextResult())
                    {
                        internalValues.Clear();
                        while (reader.Read())
                        {
                            internalValues.Add(new FinanceSummary.InternalValue()
                            {
                                age = reader["age"] != DBNull.Value ? Convert.ToString(reader["age"]) : "",
                                suppliername = reader["suppliername"] != DBNull.Value ? Convert.ToString(reader["suppliername"]) : "",
                                sectionname = reader["sectionname"] != DBNull.Value ? Convert.ToString(reader["sectionname"]) : "",
                                bsectionname = reader["barcodesection"] != DBNull.Value ? Convert.ToString(reader["barcodesection"]) : "",
                                amount = reader["balance"] != DBNull.Value ? Convert.ToDecimal(reader["balance"]) : 0
                            });
                        }
                        //if (internalValues.Exists(x => string.IsNullOrEmpty(x.bsectionname)))
                        //{
                        //    response.SupplierPending = internalValues.GroupBy(x => x.bsectionname).ToList().ConvertAll(y => new FinanceSummary.AgeByValue()
                        //    {
                        //        name = y.Key,
                        //        amount_30d = y.Sum(z => z.age == "30D" ? z.amount : 0),
                        //        amount_90d = y.Sum(z => z.age == "90D" ? z.amount : 0),
                        //        amount_180d = y.Sum(z => z.age == "180D" ? z.amount : 0),
                        //        amount_180plus = y.Sum(z => z.age == "180D+" ? z.amount : 0),
                        //    });
                        //}
                        //else
                        //{
                        response.SupplierPending = internalValues.GroupBy(x => x.sectionname).ToList().ConvertAll(y => new FinanceSummary.AgeByValue()
                        {
                            name = y.Key,
                            amount_30d = y.Sum(z => z.age == "30D" ? z.amount : 0),
                            amount_90d = y.Sum(z => z.age == "90D" ? z.amount : 0),
                            amount_180d = y.Sum(z => z.age == "180D" ? z.amount : 0),
                            amount_180plus = y.Sum(z => z.age == "180D+" ? z.amount : 0),
                        });
                        //}
                    }
                }
                connection.Close();
            }
            response.Process();
            return response;
        }

        public MonthlySummary BuildMonthlyPurchaseSummaryCached(SummaryRequst request, DateTime fromdate, DateTime todate)
        {
            MonthlySummary monthlySummary = new MonthlySummary();

            string productsummary = @"
with source as 
(select stock.productid as id,
sum(stock.qty) as qty,
sum(cost) as amount
from mv_stockmonthly stock 
where stock.stockdate >= @fromdate and stock.stockdate < @todate and stock.supplierid = @supplierid
group by stock.productid
)
select products.name,source.* from source
left outer join products on products.id = source.id
order by products.name";

            string suppliersummary = @"
with source as 
(select stock.productid as id,
sum(stock.qty) as qty,
sum(cost) as amount
from mv_stockmonthly stock 
where stock.stockdate >= @fromdate and stock.stockdate < @todate and stock.supplierid = @supplierid
group by stock.productid
)
select products.name,source.* from source
left outer join products on products.id = source.id
order by products.name";

            string sectionsummary = @"
with source as 
(select stock.productid as id,
sum(stock.qty) as qty,
sum(cost) as amount
from mv_stockmonthly stock 
where stock.stockdate >= @fromdate and stock.stockdate < @todate and stock.supplierid = @supplierid
group by stock.productid
)
select referencelist.id,referencelist.name,sum(source.amount) as amount,sum(source.qty) as qty from source
left outer join products on products.id = source.id
left outer join referencelist on referencelist.id = products.salescounterid
group by referencelist.id,referencelist.name";

            string productgroupsummary = @"
with source as 
(select stock.productid as id,
sum(stock.qty) as qty,
sum(cost) as amount
from mv_stockmonthly stock 
where stock.stockdate >= @fromdate and stock.stockdate < @todate and stock.supplierid = @supplierid
group by stock.productid
)
select referencelist.id,referencelist.name,sum(source.amount) as amount,sum(source.qty) as qty from source
left outer join products on products.id = source.id
left outer join referencelist on referencelist.id = products.productgroup
group by referencelist.id,referencelist.name";

            string barcodegroupsummary = @"
with source as 
(select stock.productid as id,
sum(stock.qty) as qty,
sum(cost) as amount
from mv_stockmonthly stock 
where stock.stockdate >= @fromdate and stock.stockdate < @todate and stock.supplierid = @supplierid
group by stock.productid
)
select referencelist.id,referencelist.name,sum(source.amount) as amount,sum(source.qty) as qty from source
left outer join products on products.id = source.id
left outer join referencelist on referencelist.id = products.barcodeid
group by referencelist.id,referencelist.name";

            string designsizesummary = @"
with source as 
(select items.sizeid as id,stock.designid,sum(stock.qty - (stock.returnqty+stock.damagedqty+stock.journalqty)) as qty,
sum((stock.qty - (stock.returnqty+stock.damagedqty+stock.journalqty))::numeric*stock.ibuyingprice::numeric) as amount from stock 
left outer join items on items.id = stock.itemid
where stock.stockdate >= @fromdate and stock.stockdate < @todate and stock.productid = @productid and stock.supplierid = @supplierid
group by items.sizeid,stock.designid
)
select size.name||COALESCE(source.designid,'') as name,source.* from source
left outer join referencelist size on size.id = source.id
order by size.name||COALESCE(source.designid,'')";

            string brandsummary = @"
with source as 
(select items.brandid as id,
sum(stock.qty - (stock.returnqty+stock.damagedqty+stock.journalqty)) as qty,
sum((stock.qty - (stock.returnqty+stock.damagedqty+stock.journalqty))::numeric*stock.ibuyingprice::numeric) as amount from stock 
left outer join items on items.id = stock.itemid
where stock.stockdate >= @fromdate and stock.stockdate < @todate and stock.productid = @productid and stock.supplierid = @supplierid
group by items.brandid
)
select brand.name,source.* from source
left outer join brand on brand.id = source.id
order by brand.name";

            var fullquery = productsummary;
            if (request.attribute == "location")
                return new MonthlySummary();
            else if (request.attribute == "brand")
                fullquery = brandsummary;
            else if (request.attribute == "section")
                fullquery = sectionsummary;
            else if (request.attribute == "devision")
                fullquery = barcodegroupsummary;
            else if (request.attribute == "productgroup")
                fullquery = productgroupsummary;
            else if (request.attribute == "supplier" || request.attribute == "supllier")
                fullquery = suppliersummary;
            else if (request.attribute == "sizedesign")
                fullquery = designsizesummary;
            if (request.supplierid <= 0)
                fullquery = fullquery.Replace("and stock.supplierid = @supplierid", "");

            if (request.productid <= 0)
                fullquery = fullquery.Replace("and stock.productid = @productid", "");

            var baseData = new Common.BaseData() { Context = this.Context };
            using (var connection = baseData.CreateConnection())
            {
                connection.Open();
                NpgsqlCommand command = new NpgsqlCommand(fullquery, connection);
                command.Parameters.Add("current_date", NpgsqlTypes.NpgsqlDbType.Date).Value = DateTimeNow.Date;
                command.Parameters.Add("fromdate", NpgsqlTypes.NpgsqlDbType.Date).Value = fromdate;
                command.Parameters.Add("todate", NpgsqlTypes.NpgsqlDbType.Date).Value = todate;
                command.Parameters.Add("productid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = request.productid;
                command.Parameters.Add("supplierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = request.supplierid;
                command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = request.locationid;
                command.CommandTimeout = 10000;
                using (var reader = command.ExecuteReader())
                {
                    monthlySummary.items = new List<MonthlySaleData>();
                    while (reader.Read())
                    {
                        monthlySummary.items.Add(new MonthlySaleData()
                        {
                            id = reader["id"] != DBNull.Value ? Convert.ToInt64(reader["id"]) : 0,
                            display = reader["name"] != DBNull.Value ? Convert.ToString(reader["name"]) : "",
                            qty = reader["qty"] != DBNull.Value ? Convert.ToDecimal(reader["qty"]) : 0,
                            amount = reader["amount"] != DBNull.Value ? Convert.ToDecimal(reader["amount"]) : 0,
                        });
                    }
                }
                connection.Close();
            }
            return monthlySummary;
        }
        public MonthlySummary BuildMonthlyPurchaseSummaryDirect(SummaryRequst request, DateTime fromdate, DateTime todate)
        {
            MonthlySummary monthlySummary = new MonthlySummary();


            string productsummary = @"
with source as 
(select stock.productid as id,
sum(stock.qty - (stock.returnqty+stock.damagedqty+stock.journalqty)) as qty,
sum((stock.qty - (stock.returnqty+stock.damagedqty+stock.journalqty))::numeric*stock.ibuyingprice::numeric) as amount
from stock 
where stock.stockdate >= @fromdate and stock.stockdate < @todate and stock.isactive = true and stock.supplierid = @supplierid
group by stock.productid
)
select products.name,source.* from source
left outer join products on products.id = source.id
order by products.name";

            string suppliersummary = @"
with source as 
(select stock.productid as id,
sum(stock.qty - (stock.returnqty+stock.damagedqty+stock.journalqty)) as qty,
sum((stock.qty - (stock.returnqty+stock.damagedqty+stock.journalqty))::numeric*stock.ibuyingprice::numeric) as amount
from stock 
where stock.stockdate >= @fromdate and stock.stockdate < @todate and stock.supplierid = @supplierid
group by stock.productid
)
select products.name,source.* from source
left outer join products on products.id = source.id
order by products.name";

            string sectionsummary = @"
with source as 
(select stock.productid as id,
sum(stock.qty - (stock.returnqty+stock.damagedqty+stock.journalqty)) as qty,
sum((stock.qty - (stock.returnqty+stock.damagedqty+stock.journalqty))::numeric*stock.ibuyingprice::numeric) as amount
from stock 
where stock.stockdate >= @fromdate and stock.stockdate < @todate and stock.supplierid = @supplierid
group by stock.productid
)
select referencelist.id,referencelist.name,sum(source.amount) as amount,sum(source.qty) as qty from source
left outer join products on products.id = source.id
left outer join referencelist on referencelist.id = products.salescounterid
group by referencelist.id,referencelist.name";

            string productgroupsummary = @"
with source as 
(select stock.productid as id,
sum(stock.qty - (stock.returnqty+stock.damagedqty+stock.journalqty)) as qty,
sum((stock.qty - (stock.returnqty+stock.damagedqty+stock.journalqty))::numeric*stock.ibuyingprice::numeric) as amount
from stock 
where stock.stockdate >= @fromdate and stock.stockdate < @todate and stock.supplierid = @supplierid
group by stock.productid
)
select referencelist.id,referencelist.name,sum(source.amount) as amount,sum(source.qty) as qty from source
left outer join products on products.id = source.id
left outer join referencelist on referencelist.id = products.productgroup
group by referencelist.id,referencelist.name";

            string barcodegroupsummary = @"
with source as 
(select stock.productid as id,
sum(stock.qty - (stock.returnqty+stock.damagedqty+stock.journalqty)) as qty,
sum((stock.qty - (stock.returnqty+stock.damagedqty+stock.journalqty))::numeric*stock.ibuyingprice::numeric) as amount
from stock 
where stock.stockdate >= @fromdate and stock.stockdate < @todate and stock.supplierid = @supplierid
group by stock.productid
)
select referencelist.id,referencelist.name,sum(source.amount) as amount,sum(source.qty) as qty from source
left outer join products on products.id = source.id
left outer join referencelist on referencelist.id = products.barcodeid
group by referencelist.id,referencelist.name";

            string designsizesummary = @"
with source as 
(select items.sizeid as id,stock.designid,sum(stock.qty - (stock.returnqty+stock.damagedqty+stock.journalqty)) as qty,
sum((stock.qty - (stock.returnqty+stock.damagedqty+stock.journalqty))::numeric*stock.ibuyingprice::numeric) as amount from stock 
left outer join items on items.id = stock.itemid
where stock.stockdate >= @fromdate and stock.stockdate < @todate and stock.productid = @productid and stock.supplierid = @supplierid
group by items.sizeid,stock.designid
)
select string_agg(size.name,',') sizename,COALESCE(source.designid,'') as name,sum(amount) amount,sum(qty) qty from source
left outer join referencelist size on size.id = source.id
group by COALESCE(source.designid,'')";

            string brandsummary = @"
with source as 
(select items.brandid as id,sum(stock.qty - (stock.returnqty+stock.damagedqty+stock.journalqty)) as qty,
sum((stock.qty - (stock.returnqty+stock.damagedqty+stock.journalqty))::numeric*stock.ibuyingprice::numeric) as amount from stock 
left outer join items on items.id = stock.itemid
where stock.stockdate >= @fromdate and stock.stockdate < @todate and stock.productid = @productid and stock.supplierid = @supplierid
group by items.brandid
)
select brand.name,source.* from source
left outer join brand on brand.id = source.id
order by brand.name";

            var fullquery = productsummary;
            if (request.attribute == "location")
                return new MonthlySummary();
            else if (request.attribute == "brand")
                fullquery = brandsummary;
            else if (request.attribute == "section")
                fullquery = sectionsummary;
            else if (request.attribute == "devision")
                fullquery = barcodegroupsummary;
            else if (request.attribute == "productgroup")
                fullquery = productgroupsummary;
            else if (request.attribute == "supplier" || request.attribute == "supllier")
                fullquery = suppliersummary;
            else if (request.attribute == "sizedesign")
                fullquery = designsizesummary;
            if (request.supplierid <= 0)
                fullquery = fullquery.Replace("and stock.supplierid = @supplierid", "");

            if (request.productid <= 0)
                fullquery = fullquery.Replace("and stock.productid = @productid", "");

            var baseData = new Common.BaseData() { Context = this.Context };
            using (var connection = baseData.CreateConnection())
            {
                connection.Open();
                NpgsqlCommand command = new NpgsqlCommand(fullquery, connection);
                command.Parameters.Add("current_date", NpgsqlTypes.NpgsqlDbType.Date).Value = DateTimeNow.Date;
                command.Parameters.Add("fromdate", NpgsqlTypes.NpgsqlDbType.Date).Value = fromdate;
                command.Parameters.Add("todate", NpgsqlTypes.NpgsqlDbType.Date).Value = todate;
                command.Parameters.Add("productid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = request.productid;
                command.Parameters.Add("supplierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = request.supplierid;
                command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = request.locationid;
                command.CommandTimeout = 10000;
                using (var reader = command.ExecuteReader())
                {
                    monthlySummary.items = new List<MonthlySaleData>();
                    while (reader.Read())
                    {
                       
                        if (request.attribute == "sizedesign" || request.attribute == "SIZE DESIGN")
                        {
                            var ms = new MonthlySaleData()
                            {
                                display = reader["name"] != DBNull.Value ? Convert.ToString(reader["name"]) : "",
                                qty = reader["qty"] != DBNull.Value ? Convert.ToDecimal(reader["qty"]) : 0,
                                amount = reader["amount"] != DBNull.Value ? Convert.ToDecimal(reader["amount"]) : 0,
                            };
                            if (reader["sizename"] != DBNull.Value)
                                ms.display += "(" + Convert.ToString(reader["sizename"]) + ")";
                        }
                        else
                        {
                            var ms = new MonthlySaleData()
                            {
                                id = reader["id"] != DBNull.Value ? Convert.ToInt64(reader["id"]) : 0,
                                display = reader["name"] != DBNull.Value ? Convert.ToString(reader["name"]) : "",
                                qty = reader["qty"] != DBNull.Value ? Convert.ToDecimal(reader["qty"]) : 0,
                                amount = reader["amount"] != DBNull.Value ? Convert.ToDecimal(reader["amount"]) : 0,
                            };
                            monthlySummary.items.Add(ms);
                        }
                        
                    }
                }
                connection.Close();
            }
            return monthlySummary;
        }
        public MonthlySummary BuildMonthlySalesSummaryCache(SummaryRequst request, DateTime fromdate, DateTime todate)
        {
            MonthlySummary monthlySummary = new MonthlySummary();

            string productsummary = @"
with source as 
(
select productid as id,sum(salevalue) as amount,
sum(qty) as qty,count(*) billcount from mv_salesdailybyproduct bill
where bill.billdate >= @fromdate and bill.billdate < @todate 
and bill.locationid = @locationid
group by productid
)
select products.name,source.* from source
left outer join products on products.id = source.id
order by products.name";

            string suppliersummary = @"
with source as 
(select productid as id,sum(salevalue) as amount,
sum(qty) as qty,count(*) billcount from mv_salesdailybyproduct bill
where bill.billdate >= @fromdate and bill.billdate < @todate 
and bill.locationid = @locationid and bill.supplierid = @supplierid
group by productid
)
select products.name,source.* from source
left outer join products on products.id = source.id
order by products.name";

            string sectionsummary = @"
with source as 
(
select productid as id,sum(salevalue) as amount,
sum(qty) as qty,count(*) billcount from mv_salesdailybyproduct bill
where bill.billdate >= @fromdate and bill.billdate < @todate and bill.locationid = @locationid
group by productid
)
select referencelist.id,referencelist.name,sum(source.amount) as amount,sum(source.qty) as qty,sum(billcount) billcount from source
left outer join products on products.id = source.id
left outer join referencelist on referencelist.id = products.salescounterid
group by referencelist.id,referencelist.name";

            string devisionsummary = @"
with source as 
(
select productid as id,sum(salevalue) as amount,
sum(qty) as qty,count(*) billcount from mv_salesdailybyproduct bill
where bill.billdate >= @fromdate and bill.billdate < @todate and bill.locationid = @locationid
group by productid
)
select referencelist.id,referencelist.name,sum(source.amount) as amount,sum(source.qty) as qty,sum(billcount) billcount from source
left outer join products on products.id = source.id
left outer join referencelist on referencelist.id = products.barcodeid
group by referencelist.id,referencelist.name";

            string prdgrpsummary = @"
with source as 
(
select productid as id,sum(salevalue) as amount,
sum(qty) as qty,count(*) billcount from mv_salesdailybyproduct bill
where bill.billdate >= @fromdate and bill.billdate < @todate 
and bill.locationid = @locationid
group by productid
)
select referencelist.id,referencelist.name,sum(source.amount) as amount,sum(source.qty) as qty,sum(billcount) billcount from source
left outer join products on products.id = source.id
left outer join referencelist on referencelist.id = products.productgroup
group by referencelist.id,referencelist.name";


            string locationsummary = @"
with source as 
(select locationid as id,sum(salevalue) as amount,
sum(qty) as qty,count(*) billcount from mv_salesdailybyproduct bill
where bill.billdate >= @fromdate and bill.billdate < @todate 
and bill.locationid = @locationid
group by bill.locationid
)
select referencelist.name,source.* from source
left outer join referencelist on referencelist.id = source.id
order by referencelist.name";

            var fullquery = productsummary;
            if (request.attribute == "location")
                fullquery = locationsummary;
            else if (request.attribute == "section")
                fullquery = sectionsummary;
            else if (request.attribute == "devision")
                fullquery = devisionsummary;
            else if (request.attribute == "productgroup")
                fullquery = prdgrpsummary;
            else if (request.attribute == "supplier" || request.attribute == "supllier")
                fullquery = suppliersummary;

            if (request.locationid <= 0)
                fullquery = fullquery.Replace("and bill.locationid = @locationid", "");

            if (request.supplierid <= 0)
                fullquery = fullquery.Replace("and bill.supplierid = @supplierid", "");

            if (request.productid <= 0)
                fullquery = fullquery.Replace("and bill.productid = @productid", "");

            monthlySummary.items = new List<MonthlySaleData>();
            var baseData = new Common.BaseData() { Context = this.Context };
            using (var connection = baseData.CreateConnection())
            {
                connection.Open();
                NpgsqlCommand command = new NpgsqlCommand(fullquery, connection);
                command.Parameters.Add("current_date", NpgsqlTypes.NpgsqlDbType.Date).Value = DateTimeNow.Date;
                command.Parameters.Add("fromdate", NpgsqlTypes.NpgsqlDbType.Date).Value = fromdate;
                command.Parameters.Add("todate", NpgsqlTypes.NpgsqlDbType.Date).Value = todate;
                command.Parameters.Add("productid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = request.productid;
                command.Parameters.Add("supplierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = request.supplierid;
                command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = request.locationid;
                command.CommandTimeout = 10000;
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        monthlySummary.items.Add(new MonthlySaleData()
                        {
                            id = reader["id"] != DBNull.Value ? Convert.ToInt64(reader["id"]) : 0,
                            display = reader["name"] != DBNull.Value ? Convert.ToString(reader["name"]) : "",
                            qty = reader["qty"] != DBNull.Value ? Convert.ToDecimal(reader["qty"]) : 0,
                            amount = reader["amount"] != DBNull.Value ? Convert.ToDecimal(reader["amount"]) : 0,
                            count = reader["billcount"] != DBNull.Value ? Convert.ToDecimal(reader["billcount"]) : 0
                        });
                    }
                }
                connection.Close();
            }
            return monthlySummary;
        }
        public MonthlySummary BuildMonthlySalesSummaryDirect(SummaryRequst request, DateTime fromdate, DateTime todate)
        {
            MonthlySummary monthlySummary = new MonthlySummary();
            string productsummary = @"
with source as 
(select stock.productid as id,sum(billitems.receivable) as amount,
sum(billitems.qty) as qty,count(*) billcount from bill
inner join billitems on billitems.billid = bill.id and billitems.isactive=true
inner join stock on stock.id = stockid and stock.supplierid = @supplierid
where bill.billdate >= @fromdate and bill.billdate < @todate and bill.billtype != 9 
and bill.isactive = true and bill.locationid = @locationid
group by stock.productid
)
select products.name,source.* from source
left outer join products on products.id = source.id
order by products.name";

            string suppliersummary = @"
with source as 
(select stock.productid as id,sum(billitems.receivable) as amount,
sum(billitems.qty) as qty,count(*) billcount from bill
inner join billitems on billitems.billid = bill.id and billitems.isactive=true
inner join stock on stock.id = stockid and stock.productid = @productid
where bill.billdate >= @fromdate and bill.billdate < @todate and bill.billtype != 9 
and bill.isactive = true and bill.locationid = @locationid and stock.supplierid = @supplierid
group by stock.productid
)
select products.name,source.* from source
left outer join products on products.id = source.id
order by products.name";

            string sectionsummary = @"
with source as 
(select stock.productid as id,sum(billitems.receivable) as amount,
sum(billitems.qty) as qty,count(*) billcount from bill
inner join billitems on billitems.billid = bill.id and billitems.isactive=true
inner join stock on stock.id = stockid and stock.supplierid = @supplierid
where bill.billdate >= @fromdate and bill.billdate < @todate and bill.billtype != 9 
and bill.isactive = true and bill.locationid = @locationid and stock.productid = @productid
group by stock.productid
)
select referencelist.id,referencelist.name,sum(source.amount) as amount,sum(source.qty) as qty,sum(billcount) billcount from source
left outer join products on products.id = source.id
left outer join referencelist on referencelist.id = products.salescounterid
group by referencelist.id,referencelist.name";

            string devisionsummary = @"
with source as 
(select stock.productid as id,sum(billitems.receivable) as amount,
sum(billitems.qty) as qty,count(*) billcount from bill
inner join billitems on billitems.billid = bill.id and billitems.isactive=true
inner join stock on stock.id = stockid and stock.supplierid = @supplierid
where bill.billdate >= @fromdate and bill.billdate < @todate and bill.billtype != 9 
and bill.isactive = true and bill.locationid = @locationid and stock.productid = @productid
group by stock.productid
)
select referencelist.id,referencelist.name,sum(source.amount) as amount,sum(source.qty) as qty,sum(billcount) billcount from source
left outer join products on products.id = source.id
left outer join referencelist on referencelist.id = products.barcodeid
group by referencelist.id,referencelist.name";

            string prdgrpsummary = @"
with source as 
(select stock.productid as id,sum(billitems.receivable) as amount,
sum(billitems.qty) as qty,count(*) billcount from bill
inner join billitems on billitems.billid = bill.id and billitems.isactive=true
inner join stock on stock.id = stockid and stock.supplierid = @supplierid
where bill.billdate >= @fromdate and bill.billdate < @todate and bill.billtype != 9 
and bill.isactive = true and bill.locationid = @locationid and stock.productid = @productid
group by stock.productid
)
select referencelist.id,referencelist.name,sum(source.amount) as amount,sum(source.qty) as qty,sum(billcount) billcount from source
left outer join products on products.id = source.id
left outer join referencelist on referencelist.id = products.productgroup
group by referencelist.id,referencelist.name";

            string brandsummary = @"
with source as 
(select items.brandid as id,sum(billitems.receivable) as amount,
sum(billitems.qty) as qty,count(*) billcount from bill
inner join billitems on billitems.billid = bill.id and billitems.isactive=true
inner join stock on stock.id = stockid and stock.productid = @productid and stock.supplierid = @supplierid
left outer join items on items.id = stock.itemid
where bill.billdate >= @fromdate and bill.billdate < @todate and bill.billtype != 9 
and bill.isactive = true and bill.locationid = @locationid and stock.productid = @productid
group by items.brandid
)
select brand.name,source.* from source
left outer join brand on brand.id = source.id
order by brand.name";

            string designsizesummary = @"
with source as 
(select items.sizeid as id,stock.designid as designid,sum(billitems.receivable) as amount,
sum(billitems.qty) as qty,count(*) billcount from bill
inner join billitems on billitems.billid = bill.id and billitems.isactive=true
inner join stock on stock.id = stockid  
left outer join items on items.id = stock.itemid
where bill.billdate >= @fromdate and bill.billdate < @todate and bill.billtype != 9 and stock.productid = @productid
and bill.isactive = true and bill.locationid = @locationid and stock.supplierid = @supplierid
group by items.sizeid,stock.designid
)
select string_agg(size.name,',') sizename,COALESCE(source.designid,'') as name,
sum(amount) amount,sum(qty) qty,sum(billcount) billcount from source
left outer join referencelist size on size.id = source.id
group by COALESCE(source.designid,'')
";

            string locationsummary = @"
with source as 
(select bill.locationid as id,sum(bill.receivable+bill.rounding) as amount,
sum(bill.qty) as qty,count(*) billcount from bill
where bill.billdate >= @fromdate and bill.billdate < @todate and bill.billtype != 9 
and bill.isactive = true and bill.locationid = @locationid
group by bill.locationid
)
select referencelist.name,source.* from source
left outer join referencelist on referencelist.id = source.id
order by referencelist.name";

            var fullquery = productsummary;
            if (request.attribute == "location")
                fullquery = locationsummary;
            else if (request.attribute == "brand")
                fullquery = brandsummary;
            else if (request.attribute == "section")
                fullquery = sectionsummary;
            else if (request.attribute == "devision")
                fullquery = devisionsummary;
            else if (request.attribute == "productgroup")
                fullquery = prdgrpsummary;
            else if (request.attribute == "supplier" || request.attribute == "supllier")
                fullquery = suppliersummary;
            else if (request.attribute == "sizedesign" || request.attribute == "SIZE DESIGN")
                fullquery = designsizesummary;

            if (request.locationid <= 0)
                fullquery = fullquery.Replace("and salablegoods.locationid=@locationid", "").Replace("and bill.locationid = @locationid", "");

            if (request.supplierid <= 0)
                fullquery = fullquery.Replace("and stock.supplierid = @supplierid", "");

            if (request.productid <= 0)
                fullquery = fullquery.Replace("and stock.productid = @productid", "");

            monthlySummary.items = new List<MonthlySaleData>();
            var baseData = new Common.BaseData() { Context = this.Context };
            using (var connection = baseData.CreateConnection())
            {
                connection.Open();
                NpgsqlCommand command = new NpgsqlCommand(fullquery, connection);
                command.Parameters.Add("current_date", NpgsqlTypes.NpgsqlDbType.Date).Value = DateTimeNow.Date;
                command.Parameters.Add("fromdate", NpgsqlTypes.NpgsqlDbType.Date).Value = fromdate;
                command.Parameters.Add("todate", NpgsqlTypes.NpgsqlDbType.Date).Value = todate;
                command.Parameters.Add("productid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = request.productid;
                command.Parameters.Add("supplierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = request.supplierid;
                command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = request.locationid;
                command.CommandTimeout = 10000;
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        MonthlySaleData ms = null;
                        if (request.attribute == "sizedesign" || request.attribute == "SIZE DESIGN")
                        {
                            ms = new MonthlySaleData()
                            {
                                display = reader["name"] != DBNull.Value ? Convert.ToString(reader["name"]) : "",
                                qty = reader["qty"] != DBNull.Value ? Convert.ToDecimal(reader["qty"]) : 0,
                                amount = reader["amount"] != DBNull.Value ? Convert.ToDecimal(reader["amount"]) : 0,
                                count = reader["billcount"] != DBNull.Value ? Convert.ToDecimal(reader["billcount"]) : 0
                            };
                            if (reader["sizename"] != DBNull.Value)
                                ms.display += "(" + Convert.ToString(reader["sizename"]) + ")";
                        }
                        else
                        {
                            ms = new MonthlySaleData()
                            {
                                id = reader["id"] != DBNull.Value ? Convert.ToInt64(reader["id"]) : 0,
                                display = reader["name"] != DBNull.Value ? Convert.ToString(reader["name"]) : "",
                                qty = reader["qty"] != DBNull.Value ? Convert.ToDecimal(reader["qty"]) : 0,
                                amount = reader["amount"] != DBNull.Value ? Convert.ToDecimal(reader["amount"]) : 0,
                                count = reader["billcount"] != DBNull.Value ? Convert.ToDecimal(reader["billcount"]) : 0
                            };
                        }
                        monthlySummary.items.Add(ms);
                    }
                }
                connection.Close();
            }
            return monthlySummary;
        }

        public MonthlySummary BuildMonthlySummary(SummaryRequst request)
        {
            DateTime to = new DateTime(request.year, request.month, 1).AddMonths(1);
            DateTime from = to.AddMonths(-1);
            MonthlySummary monthlySummary = null;
            if (request.attribute == "brand" || request.attribute == "sizedesign" || request.attribute == "SIZE DESIGN")
            {
                monthlySummary = BuildMonthlySalesSummaryDirect(request, from, to);
                var fromdirect = BuildMonthlyPurchaseSummaryDirect(request, from, to);
                if (fromdirect != null && fromdirect.items != null)
                {
                    fromdirect.items.ForEach(e =>
                    {
                        if (monthlySummary.items.Exists(x => x.id == e.id))
                        {
                            var sitem = monthlySummary.items.Find(x => x.id == e.id);
                            sitem.pamount = e.amount;
                            sitem.pqty = e.qty;
                        }
                        else
                        {
                            monthlySummary.items.Add(new MonthlySaleData()
                            {
                                id = e.id,
                                display = e.display,
                                pamount = e.amount,
                                pqty = e.qty
                            });
                        }
                    });
                }
            }
            else
            {
                DateTime aviladate = DateTime.MinValue;
                using (var reader = ExecuteReader("select lastDateValue from Cache_View where  viewname='mv_salesdailybyproduct' limit 1", null))
                {
                    if (reader.Read() && !reader.IsDBNull(0))
                        aviladate = reader.GetDateTime(0);
                }
                if (to <= aviladate)
                {
                    monthlySummary = BuildMonthlySalesSummaryCache(request, from, to);
                }
                else if (from > aviladate)
                {
                    monthlySummary = BuildMonthlySalesSummaryDirect(request, from, to);
                }
                else
                {
                    monthlySummary = BuildMonthlySalesSummaryCache(request, from, aviladate);
                    var fromdirect = BuildMonthlySalesSummaryDirect(request, aviladate.AddDays(1), to);
                    monthlySummary.Merge(fromdirect);
                }

                MonthlySummary purchaseSummary = null;
                aviladate = DateTime.MinValue;
                using (var reader = ExecuteReader("select lastDateValue from Cache_View where  viewname='mv_stockmonthly' limit 1", null))
                {
                    if (reader.Read() && !reader.IsDBNull(0))
                        aviladate = reader.GetDateTime(0);
                }
                if (to <= aviladate)
                {
                    purchaseSummary = BuildMonthlyPurchaseSummaryCached(request, from, to);
                }
                else if (from > aviladate)
                {
                    purchaseSummary = BuildMonthlyPurchaseSummaryDirect(request, from, to);
                }
                else
                {
                    purchaseSummary = BuildMonthlyPurchaseSummaryCached(request, from, aviladate);
                    var fromdirect = BuildMonthlyPurchaseSummaryDirect(request, aviladate.AddDays(1), to);
                    purchaseSummary.Merge(fromdirect);
                }
                if (purchaseSummary != null && purchaseSummary.items != null)
                {
                    purchaseSummary.items.ForEach(e =>
                    {
                        if (monthlySummary.items.Exists(x => x.id == e.id))
                        {
                            var sitem = monthlySummary.items.Find(x => x.id == e.id);
                            sitem.pamount = e.amount;
                            sitem.pqty = e.qty;
                        }
                        else
                        {
                            monthlySummary.items.Add(new MonthlySaleData()
                            {
                                id = e.id,
                                display = e.display,
                                pamount = e.amount,
                                pqty = e.qty
                            });
                        }
                    });
                }
            }
            return monthlySummary;
        }
        public List<Insight.SaleStockItem> StockSalesSummary()
        {
            var result = new List<Insight.SaleStockItem>();
            var startdate = DateTime.Now.Date.AddMonths(-3);
            string query = @"with stockdata
as
(
select case when stock.stockdate>@startdate 
	then 'NEW'
	else 'OLD' end daymode,stock.productid,
	sum(salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty)) as qty, 
	sum((salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty))*stock.ibuyingprice::numeric) as value 
	from salablegoods
inner join stock on stock.id = stockid
where salablegoods.hasstock = true and salablegoods.isactive = true 
group by case when stock.stockdate>@startdate 
	then 'NEW'
	else 'OLD' end,stock.productid
),salesdata as
(select case when stock.stockdate>@startdate 
	then 'NEW'
	else 'OLD' end daymode,
	stock.productid,sum(billitems.qty) as qty,
sum(billitems.receivable) as value from bill
inner join billitems on billitems.billid = bill.id and billitems.isactive=true
inner join stock on stock.id = stockid
where bill.billdate>=@startdate and bill.billtype != 9 
and bill.isactive = true
group by case when stock.stockdate>@startdate 
	then 'NEW'
	else 'OLD' end,stock.productid)
	
select id,name,Nstockdata.qty Nstockqty,Nstockdata.value Nstockvalue,
Nsalesdata.qty Nsalesqty,Nsalesdata.value Nsalesvalue,
Ostockdata.qty Ostockqty,Ostockdata.value Ostockvalue,
Osalesdata.qty Osalesqty,Osalesdata.value Osalesvalue from products
left outer join stockdata Nstockdata on Nstockdata.productid = products.id and Nstockdata.daymode = 'NEW'
left outer join salesdata Nsalesdata on Nsalesdata.productid = products.id and Nsalesdata.daymode = 'NEW'
left outer join stockdata Ostockdata on Ostockdata.productid = products.id and Ostockdata.daymode = 'OLD'
left outer join salesdata Osalesdata on Osalesdata.productid = products.id and Osalesdata.daymode = 'OLD'";

            var baseData = new Common.BaseData() { Context = this.Context };
            using (var connection = baseData.CreateConnection())
            {
                connection.Open();
                NpgsqlCommand command = new NpgsqlCommand(query, connection);
                command.Parameters.Add("current_date", NpgsqlTypes.NpgsqlDbType.Date).Value = DateTimeNow.Date;
                command.Parameters.Add("startdate", NpgsqlTypes.NpgsqlDbType.Date).Value = startdate;
                command.CommandTimeout = 10000;
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Insight.SaleStockItem saleStockItem = new Insight.SaleStockItem();
                        saleStockItem.name = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
                        saleStockItem.ostockvalue = reader["ostockvalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["ostockvalue"]);
                        saleStockItem.ostockqty = reader["ostockqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["ostockqty"]);
                        saleStockItem.osalesvalue = reader["osalesvalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["osalesvalue"]);
                        saleStockItem.osalesqty = reader["osalesqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["osalesqty"]);

                        saleStockItem.nstockvalue = reader["nstockvalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["nstockvalue"]);
                        saleStockItem.nstockqty = reader["nstockqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["nstockqty"]);
                        saleStockItem.nsalesvalue = reader["nsalesvalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["nsalesvalue"]);
                        saleStockItem.nsalesqty = reader["nsalesqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["nsalesqty"]);

                        saleStockItem.openingqty = saleStockItem.ostockqty + saleStockItem.osalesqty;
                        saleStockItem.openingvalue = saleStockItem.ostockvalue + saleStockItem.osalesvalue;

                        saleStockItem.closingqty = saleStockItem.ostockqty + saleStockItem.nstockqty;
                        saleStockItem.closingvalue = saleStockItem.ostockvalue + saleStockItem.nstockvalue;

                        //var sold = (saleStockItem.nsalesqty + saleStockItem.osalesqty);
                        //var purchased = (saleStockItem.nstockqty + saleStockItem.nsalesqty);
                        //var soldvalue = (saleStockItem.nsalesvalue + saleStockItem.osalesvalue);
                        //var purchasedvalue = (saleStockItem.nstockvalue + saleStockItem.nsalesvalue);
                        //var cogs = (saleStockItem.openingvalue + purchasedvalue) - saleStockItem.closingvalue;
                        //var avginventory = (saleStockItem.openingvalue +saleStockItem.closingvalue)/2;
                        //var stockturn = Math.Round(cogs / avginventory, 2);
                        //var sellthrogh = Math.Round((sold / purchased) * 100, 2);

                        result.Add(saleStockItem);
                    }
                }
            }
            result.RemoveAll(x => x.openingqty == 0 && x.closingqty == 0 && x.soldvalue == 0);
            var totalstockvalue = result.Sum(x => x.closingvalue);
            result.ForEach(x =>
            {
                if (x.closingvalue != 0)
                    x.clratio = x.closingvalue / totalstockvalue * 100;
            });
            //result.RemoveAll(x => x.clratio<5);
            result = result.OrderByDescending(x => x.sellthrogh).ThenByDescending(y => y.stockturn).ToList();
            return result;
        }
        public MonthlySummary BuildMonthlyLocationSummary(SummaryRequst request)
        {
            MonthlySummary monthlySummary = new MonthlySummary();
            DateTime todate = new DateTime(request.year, request.month, 1).AddMonths(1);
            DateTime fromdate = todate.AddMonths(-1);

            string locationsummary = @"
with source as 
(
select bill.locationid as id,sum(bill.receivable+bill.rounding) as amount,
sum(bill.qty) qty,count(*) count
from bill
where bill.billdate >= @fromdate and bill.billdate < @todate and bill.billtype != 9 
and bill.isactive = true and bill.locationid = @locationid
group by bill.locationid
)
select referencelist.name,source.* from source
left outer join referencelist on referencelist.id = source.id
order by referencelist.name";

            var fullquery = locationsummary;

            if (request.locationid <= 0)
                fullquery = fullquery.Replace("and salablegoods.locationid=@locationid", "").Replace("and bill.locationid = @locationid", "");

            var baseData = new Common.BaseData() { Context = this.Context };
            using (var connection = baseData.CreateConnection())
            {
                connection.Open();
                NpgsqlCommand command = new NpgsqlCommand(fullquery, connection);
                command.Parameters.Add("current_date", NpgsqlTypes.NpgsqlDbType.Date).Value = DateTimeNow.Date;
                command.Parameters.Add("fromdate", NpgsqlTypes.NpgsqlDbType.Date).Value = fromdate;
                command.Parameters.Add("todate", NpgsqlTypes.NpgsqlDbType.Date).Value = todate;
                command.Parameters.Add("productid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = request.productid;
                command.Parameters.Add("supplierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = request.supplierid;
                command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = request.locationid;
                command.CommandTimeout = 10000;
                using (var reader = command.ExecuteReader())
                {
                    monthlySummary.items = new List<MonthlySaleData>();
                    while (reader.Read())
                    {
                        monthlySummary.items.Add(new MonthlySaleData()
                        {
                            id = reader["id"] != DBNull.Value ? Convert.ToInt64(reader["id"]) : 0,
                            display = reader["name"] != DBNull.Value ? Convert.ToString(reader["name"]) : "",
                            amount = reader["amount"] != DBNull.Value ? Convert.ToDecimal(reader["amount"]) : 0,
                            qty = reader["qty"] != DBNull.Value ? Convert.ToDecimal(reader["qty"]) : 0,
                            count = reader["count"] != DBNull.Value ? Convert.ToDecimal(reader["count"]) : 0,
                        });
                    }
                }
                connection.Close();
            }
            monthlySummary.items.Sort(delegate (MonthlySaleData a, MonthlySaleData b)
            {
                return b.amount.CompareTo(a.amount);
            });
            return monthlySummary;
        }
        public List<Analytical.MobileInsight.SaleView_Response> SaleView_Product(Analytical.MobileInsight.SaleView_Request request)
        {
            string query = @"with salesdetails as
            (
                select stock.productid,sum(billitems.qty*stock.ibuyingprice) produtcost,sum(billitems.qty) as salqty,sum(billitems.receivable) as salval from bill
                inner join billitems on billitems.billid = bill.id and billitems.isactive=true and billitems.salesmanid = @salesmenid
                inner join stock on stock.id = stockid
                where bill.billdate>=@fromdate and bill.billdate<@todate 
                and bill.billtype != 9 and bill.isactive = true
                and bill.locationid = @locationid
                group by stock.productid
            )
            select salesdetails.*,products.name from salesdetails
            left outer join products on products.id = salesdetails.productid";

            List<Analytical.MobileInsight.SaleView_Response> response = new List<SaleView_Response>();
            var baseData = new Common.BaseData() { Context = this.Context };
            using (var connection = baseData.CreateConnection())
            {

                try
                {

                    if (request.fromdate.Date == DateTime.MinValue)
                        request.fromdate = request.date;
                    connection.Open();
                    NpgsqlCommand command = new NpgsqlCommand(query, connection);
                    command.Parameters.Add("fromdate", NpgsqlTypes.NpgsqlDbType.Date).Value = request.fromdate.Date;
                    command.Parameters.Add("todate", NpgsqlTypes.NpgsqlDbType.Date).Value = request.date.Date.AddDays(1);
                    command.Parameters.Add("productid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = request.productid;
                    command.Parameters.Add("salesmenid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = request.salesmenid;
                    command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = request.locationid;
                    command.Parameters.Add("sectionid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = request.sectionid;
                    command.CommandTimeout = 10000;
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            response.Add(new SaleView_Response()
                            {
                                id = reader["productid"] != DBNull.Value ? Convert.ToInt64(reader["productid"]) : 0,
                                display = reader["name"] != DBNull.Value ? Convert.ToString(reader["name"]) : "",
                                salqty = reader["salqty"] != DBNull.Value ? Convert.ToDecimal(reader["salqty"]) : 0,
                                salval = reader["salval"] != DBNull.Value ? Convert.ToDecimal(reader["salval"]) : 0,
                                stkval = reader["produtcost"] != DBNull.Value ? Convert.ToDecimal(reader["produtcost"]) : 0,
                            });
                        }
                    }
                }
                catch (Exception exp)
                {
                    WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, "MobileInsight.SaleView." + request.ToJSON());
                }
                finally
                {
                    connection.Close();
                }
            }
            return response.OrderByDescending(x => x.salval).ToList();
        }

        public List<Analytical.MobileInsight.SaleView_Response> SaleView_Supplier(Analytical.MobileInsight.SaleView_Request request)
        {
            string query = @"with salesdetails as
            (
                select stock.supplierid,sum(billitems.qty) as salqty,sum(billitems.receivable) as salval from bill
                inner join billitems on billitems.billid = bill.id and billitems.isactive=true
                inner join stock on stock.id = stockid and stock.productid = @productid
                where bill.billdate>=@fromdate and bill.billdate<@todate and bill.billtype != 9 and bill.isactive = true
                and bill.locationid = @locationid
                group by stock.supplierid
            )
            select salesdetails.*,supplier.name from salesdetails
            left outer join supplier on supplier.id = salesdetails.supplierid;

            with stockdetails as
            (
                select stock.supplierid,
                sum(salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty)) as stkqty, 
                sum((salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty))*stock.ibuyingprice::numeric) as stkval
                from salablegoods
                inner join stock on stock.id = salablegoods.stockid and stock.productid = @productid
                where salablegoods.hasstock =true and salablegoods.isactive=true and salablegoods.locationid = @locationid
                group by stock.supplierid
            )
            select stockdetails.*,supplier.name from stockdetails
            left outer join supplier on supplier.id = stockdetails.supplierid;";

            if (request.sectionid > 0)
            {
                query = @"with salesdetails as
                    (
                        select stock.supplierid,sum(billitems.qty) as salqty,sum(billitems.receivable) as salval from bill
                        inner join billitems on billitems.billid = bill.id and billitems.isactive=true
                        inner join stock on stock.id = stockid 
                        inner join products on products.id = stock.productid and products.salescounterid = @sectionid
                        where bill.billdate>=@fromdate and bill.billdate<@todate and bill.billtype != 9 and bill.isactive = true
                        and bill.locationid = @locationid
                        group by stock.supplierid
                    )
                    select salesdetails.*,supplier.name from salesdetails
                    left outer join supplier on supplier.id = salesdetails.supplierid;

                    with stockdetails as
                    (
                        select stock.supplierid,
                        sum(salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty)) as stkqty, 
                        sum((salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty))*stock.ibuyingprice::numeric) as stkval
                        from salablegoods
                        inner join stock on stock.id = salablegoods.stockid 
                        inner join products on products.id = stock.productid and products.salescounterid = @sectionid
                        where salablegoods.hasstock =true and salablegoods.isactive=true and salablegoods.locationid = @locationid
                        group by stock.supplierid
                    )
                    select stockdetails.*,supplier.name from stockdetails
                    left outer join supplier on supplier.id = stockdetails.supplierid;";
            }

            if (request.divisionid > 0)
            {
                query = @"with salesdetails as
                    (
                        select stock.supplierid,sum(billitems.qty) as salqty,sum(billitems.receivable) as salval from bill
                        inner join billitems on billitems.billid = bill.id and billitems.isactive=true
                        inner join stock on stock.id = stockid 
                        inner join products on products.id = stock.productid and products.barcodeid = @divisionid
                        where bill.billdate>=@fromdate and bill.billdate<@todate and bill.billtype != 9 and bill.isactive = true
                        and bill.locationid = @locationid
                        group by stock.supplierid
                    )
                    select salesdetails.*,supplier.name from salesdetails
                    left outer join supplier on supplier.id = salesdetails.supplierid;

                    with stockdetails as
                    (
                        select stock.supplierid,
                        sum(salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty)) as stkqty, 
                        sum((salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty))*stock.ibuyingprice::numeric) as stkval
                        from salablegoods
                        inner join stock on stock.id = salablegoods.stockid 
                        inner join products on products.id = stock.productid and products.barcodeid = @divisionid
                        where salablegoods.hasstock =true and salablegoods.isactive=true and salablegoods.locationid = @locationid
                        group by stock.supplierid
                    )
                    select stockdetails.*,supplier.name from stockdetails
                    left outer join supplier on supplier.id = stockdetails.supplierid;";
            }

            List<Analytical.MobileInsight.SaleView_Response> response = new List<SaleView_Response>();
            var baseData = new Common.BaseData() { Context = this.Context };
            using (var connection = baseData.CreateConnection())
            {

                try
                {

                    if (request.fromdate.Date == DateTime.MinValue)
                        request.fromdate = request.date;
                    connection.Open();
                    NpgsqlCommand command = new NpgsqlCommand(query, connection);
                    command.Parameters.Add("fromdate", NpgsqlTypes.NpgsqlDbType.Date).Value = request.fromdate.Date;
                    command.Parameters.Add("todate", NpgsqlTypes.NpgsqlDbType.Date).Value = request.date.Date.AddDays(1);
                    command.Parameters.Add("productid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = request.productid;
                    command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = request.locationid;
                    command.Parameters.Add("divisionid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = request.divisionid;
                    command.Parameters.Add("sectionid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = request.sectionid;
                    command.CommandTimeout = 10000;
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            response.Add(new SaleView_Response()
                            {
                                id = reader["supplierid"] != DBNull.Value ? Convert.ToInt64(reader["supplierid"]) : 0,
                                display = reader["name"] != DBNull.Value ? Convert.ToString(reader["name"]) : "",
                                salqty = reader["salqty"] != DBNull.Value ? Convert.ToDecimal(reader["salqty"]) : 0,
                                salval = reader["salval"] != DBNull.Value ? Convert.ToDecimal(reader["salval"]) : 0,
                            });
                        }
                        if (reader.NextResult())
                        {
                            while (reader.Read())
                            {
                                var id = reader["supplierid"] != DBNull.Value ? Convert.ToInt64(reader["supplierid"]) : 0;
                                if (response.Exists(x => x.id == id))
                                {
                                    var exist = response.Find(x => x.id == id);
                                    exist.stkval = reader["stkval"] != DBNull.Value ? Convert.ToDecimal(reader["stkval"]) : 0;
                                    exist.stkqty = reader["stkqty"] != DBNull.Value ? Convert.ToDecimal(reader["stkqty"]) : 0;
                                }
                                else
                                {
                                    response.Add(new SaleView_Response()
                                    {
                                        id = reader["supplierid"] != DBNull.Value ? Convert.ToInt64(reader["supplierid"]) : 0,
                                        display = reader["name"] != DBNull.Value ? Convert.ToString(reader["name"]) : "",
                                        stkval = reader["stkval"] != DBNull.Value ? Convert.ToDecimal(reader["stkval"]) : 0,
                                        stkqty = reader["stkqty"] != DBNull.Value ? Convert.ToDecimal(reader["stkqty"]) : 0,
                                    });
                                }
                            }
                        }
                    }
                }
                catch (Exception exp)
                {
                    WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, "MobileInsight.SaleView." + request.ToJSON());
                }
                finally
                {
                    connection.Close();
                }
            }
            return response.OrderByDescending(x => x.salval).ToList();
        }
        public Insight Supplier(long supplierid, string locationid)
        {
            if (!Services.ConfigManager.Instance(Context.OrganizationID).Config.analyser_data_cache)
                return new MobileInsightOLD().Supplier(supplierid, locationid);

            long locid = 0;
            long.TryParse(locationid, out locid);

            string fullquery = @"select name,0::int as stockholdingperiod from Supplier where id=@supplierid;

select case when EXTRACT(day from @current_date - stock.stockdate) <30 then 'below 30 days'
	when EXTRACT(day from @current_date - stock.stockdate)<61 then '30 - 60 days'
	when EXTRACT(day from @current_date - stock.stockdate)<91 then '60 - 90 days'
	when EXTRACT(day from @current_date - stock.stockdate)<121 then '90 - 120 days'
    when EXTRACT(day from @current_date - stock.stockdate)<151 then '120 - 150 days'
    when EXTRACT(day from @current_date - stock.stockdate)<181 then '150 - 180 days'
	else 'above 180 days' end as 
	period,sum(salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty)) as qty, 
sum((salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty))*stock.ibuyingprice::numeric) as value from salablegoods
inner join stock on stock.id = stockid and stock.supplierid = @supplierid
where salablegoods.hasstock = true and salablegoods.isactive = true
and salablegoods.locationid=@locationid
group by period;

select case when EXTRACT(day from bill.billdate - stock.stockdate)<30 then 'below 30 days'
	when EXTRACT(day from bill.billdate - stock.stockdate)<61 then '30 - 60 days'
	when EXTRACT(day from bill.billdate - stock.stockdate)<91 then '60 - 90 days'
	when EXTRACT(day from bill.billdate - stock.stockdate)<121 then '90 - 120 days'
    when EXTRACT(day from bill.billdate - stock.stockdate)<151 then '120 - 150 days'
    when EXTRACT(day from bill.billdate - stock.stockdate)<181 then '150 - 180 days'
	else 'above 180 days' end as 
	period,sum(billitems.qty) as qty from bill
inner join billitems on billitems.billid = bill.id and billitems.isactive=true
inner join stock on stock.id = stockid and stock.supplierid = @supplierid
where bill.billdate>=@todayfromdate and bill.billdate<@todaytodate and bill.billtype != 9 and bill.isactive = true
and bill.locationid=@locationid
group by period;";

            if (Services.ConfigManager.Instance(Context.OrganizationID).Config.analyser_stock_removeaqty)
                fullquery = fullquery.Replace("salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty)", "salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty+salablegoods.aqty)");

            if (locid <= 0)
                fullquery = fullquery.Replace("and salablegoods.locationid=@locationid", "").Replace("and bill.locationid=@locationid", "");

            var result = new Insight();
            if (Services.ConfigManager.Instance(Context.OrganizationID).Config.analyser_data_cache)
                result.SalesData = new AnalyticalCubes().GetSupplierCurrentYearSales(supplierid);
            else
                result.SalesData = new AnalyticalCubesOLD().GetSupplierCurrentYearSales(supplierid);

            var todate = GetQuarterLast(DateTimeNow.Date).AddDays(1);
            var baseData = new Common.BaseData() { Context = this.Context };
            using (var connection = baseData.CreateConnection())
            {

                try
                {
                    connection.Open();
                    NpgsqlCommand command = new NpgsqlCommand(fullquery, connection);
                    command.Parameters.Add("fromdate", NpgsqlTypes.NpgsqlDbType.Date).Value = todate.AddYears(-1);
                    command.Parameters.Add("todate", NpgsqlTypes.NpgsqlDbType.Date).Value = todate;
                    command.Parameters.Add("current_date", NpgsqlTypes.NpgsqlDbType.Date).Value = DateTimeNow.Date;
                    command.Parameters.Add("supplierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = supplierid;
                    command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = locid;
                    command.Parameters.Add("todayfromdate", NpgsqlTypes.NpgsqlDbType.Date).Value = DateTimeNow.Date;
                    command.Parameters.Add("todaytodate", NpgsqlTypes.NpgsqlDbType.Date).Value = DateTimeNow.Date.AddDays(1);
                    command.Parameters.Add("quarterfromdate", NpgsqlTypes.NpgsqlDbType.Date).Value = new DateTime(DateTimeNow.Year, DateTimeNow.Month, 1).AddMonths(-3);
                    command.Parameters.Add("quartertodate", NpgsqlTypes.NpgsqlDbType.Date).Value = DateTimeNow.Date.AddDays(1);
                    command.CommandTimeout = 10000;
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            result.display = reader.GetString(0);
                            result.stockholding = reader.IsDBNull(1) ? 0 : reader.GetInt32(1);
                        }

                        if (reader.NextResult())
                        {
                            result.StockData = new List<Insight.StockItem>();
                            while (reader.Read())
                            {
                                result.StockData.Add(new Insight.StockItem()
                                {
                                    Period = reader["period"] != DBNull.Value ? Convert.ToString(reader["period"]) : "",
                                    Qty = reader["qty"] != DBNull.Value ? Convert.ToDecimal(reader["qty"]) : 0,
                                    Value = reader["value"] != DBNull.Value ? Convert.ToDecimal(reader["value"]) : 0,
                                    Index = IndexOfDays(reader["period"] != DBNull.Value ? Convert.ToString(reader["period"]) : "")
                                });
                            }
                            result.StockDataTotalQty = result.StockData.Sum(e => e.Qty);
                            result.StockDataTotalValue = result.StockData.Sum(e => e.Value);
                        }

                        if (reader.NextResult())
                        {
                            while (reader.Read())
                            {
                                var item = new Insight.StockItem()
                                {
                                    Period = reader["period"] != DBNull.Value ? Convert.ToString(reader["period"]) : "",
                                    Sold = reader["qty"] != DBNull.Value ? Convert.ToDecimal(reader["qty"]) : 0,
                                    Index = IndexOfDays(reader["period"] != DBNull.Value ? Convert.ToString(reader["period"]) : "")
                                };
                                if (result.StockData.Exists(e => e.Period == item.Period))
                                {
                                    result.StockData.Find(e => e.Period == item.Period).Sold = item.Sold;
                                }
                                else
                                {
                                    result.StockData.Add(item);
                                }
                            }
                            result.StockDataTotalSold = result.StockData.Sum(e => e.Sold);
                        }
                    }

                    result.PurchaseData = new List<Insight.SalesItem>();

                    DateTime aviladate = DateTime.MinValue;
                    using (var reader = ExecuteReader("select lastDateValue from Cache_View where viewname='mv_purchaseproductdaily' limit 1", null))
                    {
                        if (reader.Read() && !reader.IsDBNull(0))
                            aviladate = reader.GetDateTime(0);
                    }

                    var param = new SortedList<string, object>();
                    param.Add("fromdate", todate.AddYears(-1));
                    param.Add("todate", todate);
                    param.Add("supplierid", supplierid);
                    using (var reader = ExecuteReader(@"select quarter,year,sum(cost) cost,sum(qty) qty from mv_montlypurchasebyproduct 
                                        where supplierid=@supplierid and datebegin>=@fromdate and datebegin<@todate
                                        group by quarter,year", param))
                    {
                        while (reader.Read())
                        {
                            result.PurchaseData.Add(new Insight.SalesItem()
                            {
                                Qty = reader["qty"] != DBNull.Value ? Convert.ToDecimal(reader["qty"]) : 0,
                                Cost = reader["Cost"] != DBNull.Value ? Convert.ToDecimal(reader["Cost"]) : 0,
                                Quarter = reader["Quarter"] != DBNull.Value ? Convert.ToInt32(reader["Quarter"]) : 0,
                                Year = reader["Year"] != DBNull.Value ? Convert.ToInt32(reader["Year"]) : 0,
                            });
                        }
                    }

                    if (todate > aviladate)
                    {
                        param = new SortedList<string, object>();
                        param.Add("fromdate", aviladate);
                        param.Add("todate", todate);
                        param.Add("supplierid", supplierid);
                        using (var reader = ExecuteReader(@"select sum(stock.qty - (stock.returnqty+stock.damagedqty+stock.journalqty)) as qty,
                        sum((stock.qty - (stock.returnqty+stock.damagedqty+stock.journalqty))::numeric*stock.ibuyingprice::numeric) as cost,
                        EXTRACT(quarter from stock.stockdate) as quarter,EXTRACT(year from stock.stockdate) as year from stock
                        where stock.supplierid = @supplierid and stock.stockdate>=@fromdate and stock.stockdate<@todate
                        group by EXTRACT(quarter from stock.stockdate),EXTRACT(year from stock.stockdate);", param))
                        {
                            while (reader.Read())
                            {
                                var pd = new Insight.SalesItem()
                                {
                                    Qty = reader["qty"] != DBNull.Value ? Convert.ToDecimal(reader["qty"]) : 0,
                                    Cost = reader["Cost"] != DBNull.Value ? Convert.ToDecimal(reader["Cost"]) : 0,
                                    Quarter = reader["Quarter"] != DBNull.Value ? Convert.ToInt32(reader["Quarter"]) : 0,
                                    Year = reader["Year"] != DBNull.Value ? Convert.ToInt32(reader["Year"]) : 0,
                                };
                                if (result.PurchaseData.Exists(x => x.Quarter == pd.Quarter &&
                                x.Year == pd.Year))
                                {
                                    var xpd = result.PurchaseData.Find(x => x.Quarter == pd.Quarter &&
                                              x.Year == pd.Year);
                                    xpd.Cost += pd.Cost;
                                    xpd.Qty += pd.Qty;
                                }
                                else
                                {
                                    result.PurchaseData.Add(pd);
                                }
                            }
                        }
                    }

                    result.PurchaseData.ForEach(e =>
                    {
                        e.Quarter -= 1;
                        if (e.Quarter == 0)
                        {
                            e.Quarter = 4;
                            e.Year = e.Year - 1;
                        }
                    });
                }
                catch (Exception exp)
                {
                    WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, "MobileInsight.Supplier." + supplierid.ToString("F0"));
                }
                finally
                {
                    connection.Close();
                }
            }

            result.InsightData = new List<Insight.DataItem>();
            if (result.PurchaseData == null) result.PurchaseData = new List<Insight.SalesItem>();
            result.PurchaseData.ForEach(e =>
            {
                result.InsightData.Add(new Insight.DataItem()
                {
                    Purchase = e.Cost,
                    PurchaseQty = e.Qty,
                    Quarter = e.Quarter,
                    Year = e.Year,
                    Display = string.Format("{0}({1})", GetFinanceYear(e.Year), quaterdisplay[e.Quarter - 1])
                });
            });

            if (result.SalesData == null) result.SalesData = new List<Insight.SalesItem>();
            result.SalesData.ForEach(e =>
            {
                if (result.InsightData.Exists(x => x.Year == e.Year && x.Quarter == e.Quarter))
                {
                    var edata = result.InsightData.Find(x => x.Year == e.Year && x.Quarter == e.Quarter);
                    edata.Sales = e.Amount;
                    edata.SalesQty = e.Qty;
                    edata.Profit = e.Amount - e.Cost;
                }
                else
                {
                    result.InsightData.Add(new Insight.DataItem()
                    {
                        Sales = e.Cost,
                        SalesQty = e.Qty,
                        Quarter = e.Quarter,
                        Year = e.Year,
                        Profit = e.Amount - e.Cost,
                        Display = string.Format("{0}({1})", GetFinanceYear(e.Year), quaterdisplay[e.Quarter - 1])
                    });
                }
            });

            if (result.StockData == null) result.StockData = new List<Insight.StockItem>();
            result.StockData.Sort(delegate (Insight.StockItem a, Insight.StockItem b)
            {
                return a.Index.CompareTo(b.Index);
            });

            result.ClosingAmount = result.StockData.Sum(e => e.Value);
            result.ClosingQty = result.StockData.Sum(e => e.Qty);
            result.Closing = result.ClosingAmount;
            result.SalesCost = result.SalesData.Sum(e => e.Cost);
            result.SalesQty = result.SalesData.Sum(e => e.Qty);
            result.SalesAmount = result.SalesData.Sum(e => e.Amount);
            result.PurchaseCost = result.PurchaseData.Sum(e => e.Cost);
            result.PurchaseQty = result.PurchaseData.Sum(e => e.Qty);
            result.OpeningCost = result.ClosingAmount - result.PurchaseCost + result.SalesCost;
            result.OpeningQty = result.ClosingQty - result.PurchaseQty + result.SalesQty;
            result.PriceData = SupplierPriceRangesCache(supplierid, " stock.supplierid = " + supplierid.ToString("F0"), DateTimeNow.AddMonths(-3), DateTimeNow.Date);

            result.PriceData.Sort(delegate (Insight.PriceRangeItem a, Insight.PriceRangeItem b)
            {
                return a.sortindex.CompareTo(b.sortindex);
            });

            result.SalesData.Sort(delegate (Insight.SalesItem a, Insight.SalesItem b)
            {
                return (a.Year * 12 + a.Quarter).CompareTo(b.Year * 12 + b.Quarter);
            });
            result.PurchaseData.Sort(delegate (Insight.SalesItem a, Insight.SalesItem b)
            {
                return (a.Year * 12 + a.Quarter).CompareTo(b.Year * 12 + b.Quarter);
            });
            return result;
        }

        public class DailySaleData
        {
            public int day { get; set; }
            public int cday { get; set; }
            public int lday { get; set; }
            public decimal currentyear { get; set; }
            public decimal lastyear { get; set; }
            [Newtonsoft.Json.JsonIgnore]
            public decimal qty { get; set; }
            [Newtonsoft.Json.JsonIgnore]
            public decimal amount { get; set; }
            public decimal result { get; set; }
            public DayOfWeek currentyearday { get; set; }
            public DayOfWeek lastyearday { get; set; }
            [JsonIgnore]
            public DateTime date { get; set; }
            [JsonIgnore]
            public DateTime cdate { get; set; }
            [JsonIgnore]
            public DateTime ldate { get; set; }
        }

        public class MonthlySaleData
        {
            public string display { get; set; }
            public decimal qty { get; set; }
            public decimal amount { get; set; }
            public decimal count { get; set; }
            public decimal pqty { get; set; }
            public decimal pamount { get; set; }
            public long id { get; set; }
        }

        public class MonthlySummary
        {
            public string description { get; set; }
            public List<MonthlySaleData> items { get; set; }
            public decimal totalqty { get; set; }
            public decimal totalamount { get; set; }
            public decimal totalpamount { get; set; }
            public decimal totalpqty { get; set; }

            internal void Merge(MonthlySummary fromdirect)
            {
                fromdirect.items.ForEach(x =>
                {
                    if (items.Exists(y => y.id == x.id))
                    {
                        var itm = items.Find(y => y.id == x.id);
                        itm.qty += x.qty;
                        itm.amount += x.amount;
                        itm.count += x.count;
                    }
                    else
                    {
                        items.Add(x);
                    }
                });
            }
        }
        public class DailySummary
        {
            public bool datebased { get; set; }
            public string currentyear { get; set; }
            public string lastyear { get; set; }
            public decimal currentyeartotal { get; set; }
            public decimal lastyeartotal { get; set; }
            public List<DailySaleData> items { get; set; }
            public decimal result { get; set; }
        }
        public class PriceItem
        {
            public int index { get; set; }
            public decimal from { get; set; }
            public decimal price { get; set; }
            public decimal qty { get; set; }
            public decimal total { get; set; }
            public decimal percentage { get; set; }
        }

        public List<Insight.PriceRangeItem> SupplierPriceRangesCache(long supplierid, string condition, DateTime fromdate, DateTime todate)
        {
            string query = string.Format(@"
            select case when costprice>0 and qty>0 then floor(costprice/qty) else 0 end as price,
            sum(qty) qty from mw_dailsalesdetails
            where supplierid=@supplierid and bldate>=@fromdate and bldate<@todate
            group by price", condition);

            var baseData = new InfyPOS.Common.BaseData() { Context = this.Context };
            List<PriceItem> priceitems = new List<PriceItem>();
            int index = 0;
            decimal total = 0;
            using (var connection = baseData.CreateConnection())
            {
                connection.Open();
                NpgsqlCommand command = new NpgsqlCommand(query, connection);
                command.Parameters.Add("fromdate", NpgsqlTypes.NpgsqlDbType.Date).Value = todate.AddDays(-7);
                command.Parameters.Add("todate", NpgsqlTypes.NpgsqlDbType.Date).Value = todate;
                command.Parameters.Add("supplierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = supplierid;
                command.CommandTimeout = 10000;

                using (var reader = command.ExecuteReader())
                {
                    decimal from = 0;
                    while (reader.Read())
                    {
                        total += reader["qty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["qty"]);
                        priceitems.Add(new PriceItem()
                        {
                            from = from,
                            price = reader["price"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["price"]),
                            qty = reader["qty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["qty"]),
                            index = index++,
                            total = total
                        });
                        from = priceitems.Last().price;
                    }
                }
                connection.Close();
            }
            var result = new List<Insight.PriceRangeItem>();
            int percentposition = 10;
            List<decimal> pricebrackets = new List<decimal>();
            decimal diffValue = 0;
            foreach (var item in priceitems.OrderBy(x => x.price))
            {
                if (item.total != 0 && total != 0)
                {
                    item.percentage = Math.Round(item.total / total * 100, 2);
                    var price = item.price;
                    if (item.percentage > percentposition)
                    {
                        if (priceitems.IndexOf(item) + 1 < priceitems.Count &&
                            priceitems[priceitems.IndexOf(item) + 1].price != item.price)
                        {
                            if (pricebrackets.Count > 0)
                                diffValue += item.price - pricebrackets.Last();
                            pricebrackets.Add(item.price);
                            percentposition += 10;
                        }
                    }
                }
            }
            if (pricebrackets.Count > 0 && diffValue > 0)
            {
                var newpricerackets = new List<decimal>();
                var avgprice = Math.Ceiling((diffValue / pricebrackets.Count) / 10) * 10;

                for (int i = 0; i < pricebrackets.Count; i++)
                {
                    var newValue = avgprice > 0 ? Math.Ceiling(pricebrackets[i] / avgprice) * avgprice : 0;
                    if (newpricerackets.Count == 0 || newpricerackets.Max() < newValue)
                        newpricerackets.Add(newValue);
                }
                newpricerackets.Reverse();

                var pricerangequery = @"
with currentstock as
(
 select case {1} end as price,sum(stockqty) as cqty, 
 sum(stockvalue) cstock from mw_dailstockdetails
 where supplierid=@supplierid
 group by price
),currentsales as
(
  select case {2} end as price,sum(costprice) costval,
  sum(qty) qty,sum(qty*productprice) salesvalue from mw_dailsalesdetails
  where bldate>=@fromdate and bldate<@todate and supplierid=@supplierid
  group by price
),currentpurchase as
(
 select case {3} end as purprice,sum(qty) as pqty, 
 sum(amount) pstock 
 from mv_purchaseproductdaily
 where 
 lrentrydate>=@fromdate and lrentrydate<@todate and supplierid=@supplierid
 group by purprice
)

select COALESCE(currentstock.price, currentsales.price,'UNKNOWN') as price,
COALESCE(currentstock.cqty,0) stockqty,COALESCE(currentstock.cstock,0) stockvalue,
COALESCE(currentsales.costval,0) soldcost,COALESCE(currentsales.qty,0) saleqty,
COALESCE(currentsales.salesvalue,0) salesvalue,
COALESCE(currentpurchase.pqty,0) purchaseqty,
COALESCE(currentpurchase.pstock,0) purchasevalue
from currentstock
full outer join currentsales on currentsales.price = currentstock.price
full outer join currentpurchase on currentpurchase.purprice = currentstock.price";

                System.Text.StringBuilder billpricelist = new System.Text.StringBuilder();
                System.Text.StringBuilder stockpricelist = new System.Text.StringBuilder();
                System.Text.StringBuilder purchasepricelist = new System.Text.StringBuilder();
                decimal tovalue = 0;
                foreach (var rng in newpricerackets)
                {
                    var caption = tovalue == 0 ? "Above " + rng.ToString("F0") : (rng.ToString("F0") + " - " + tovalue.ToString("F0"));
                    stockpricelist.AppendLine("when productprice>" + rng.ToString("F0") + " then '" + caption + "'");
                    billpricelist.AppendLine("when productprice>" + rng.ToString("F0") + " then '" + caption + "'");
                    purchasepricelist.AppendLine("when price>" + rng.ToString("F0") + " then '" + caption + "'");
                    tovalue = rng;
                }
                billpricelist.AppendLine(" else 'Below " + tovalue.ToString("F0") + "'");
                stockpricelist.AppendLine(" else 'Below " + tovalue.ToString("F0") + "'");
                purchasepricelist.AppendLine(" else 'Below " + tovalue.ToString("F0") + "'");

                query = string.Format(pricerangequery, condition, stockpricelist.ToString(), billpricelist.ToString(), purchasepricelist.ToString());

                using (var connection = baseData.CreateConnection())
                {
                    if (Services.ConfigManager.Instance(Context.OrganizationID).Config.analyser_stock_removeaqty)
                        query = query.Replace("salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty)", "salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty+salablegoods.aqty)");

                    connection.Open();
                    NpgsqlCommand command = new NpgsqlCommand(query, connection);
                    command.Parameters.Add("fromdate", NpgsqlTypes.NpgsqlDbType.Date).Value = fromdate;
                    command.Parameters.Add("todate", NpgsqlTypes.NpgsqlDbType.Date).Value = todate;
                    command.Parameters.Add("supplierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = supplierid;
                    command.CommandTimeout = 10000;

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var pindex = 0;
                            var price = reader["price"] == DBNull.Value ? "Unknown" : Convert.ToString(reader["price"]);
                            if (price != "Unknown")
                            {
                                var lastitem = price.Split(new char[] { ' ', '-' }).Last();
                                int.TryParse(lastitem, out pindex);
                            }
                            result.Add(new Insight.PriceRangeItem()
                            {
                                sortindex = pindex,
                                price = reader["price"] == DBNull.Value ? "Unknown" : Convert.ToString(reader["price"]),
                                saleqty = reader["saleqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["saleqty"]),
                                soldcost = reader["soldcost"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["soldcost"]),
                                stockqty = reader["stockqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["stockqty"]),
                                stockvalue = reader["stockvalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["stockvalue"]),
                                salesvalue = reader["salesvalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["salesvalue"]),
                                purchaseqty = reader["purchaseqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["purchaseqty"]),
                                purchasevalue = reader["purchasevalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["purchasevalue"])
                            }); ;
                        }
                    }
                    connection.Close();
                }
            }
            var totalqty = result.Sum(x => x.stockqty);
            result.ForEach(x =>
            {
                if (totalqty > 0)
                    x.pt = Math.Round(x.stockqty / totalqty * 100, 2);
                if (x.saleqty != 0 && x.purchaseqty != 0)
                    x.rt = x.saleqty / x.purchaseqty * 4;
            });
            return result;
        }

        public List<Insight.PriceRangeItem> SupplierPriceRanges(string condition, DateTime fromdate, DateTime todate)
        {

            string query = string.Format(@"
select floor((billitems.rate+billitems.itemtax)::numeric) price,
sum(billitems.qty) qty from bill
inner join billitems on billitems.billid = bill.id and billitems.isactive=true
inner join stock on stock.id = stockid and {0}
where bill.billdate>=@fromdate and bill.billdate<@todate and bill.billtype != 9 and bill.isactive = true
group by floor((billitems.rate+billitems.itemtax)::numeric)", condition);

            var baseData = new InfyPOS.Common.BaseData() { Context = this.Context };
            List<PriceItem> priceitems = new List<PriceItem>();
            int index = 0;
            decimal total = 0;
            using (var connection = baseData.CreateConnection())
            {
                connection.Open();
                NpgsqlCommand command = new NpgsqlCommand(query, connection);
                command.Parameters.Add("fromdate", NpgsqlTypes.NpgsqlDbType.Date).Value = todate.AddDays(-7);
                command.Parameters.Add("todate", NpgsqlTypes.NpgsqlDbType.Date).Value = todate;
                command.CommandTimeout = 10000;

                using (var reader = command.ExecuteReader())
                {
                    decimal from = 0;
                    while (reader.Read())
                    {
                        total += reader["qty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["qty"]);
                        priceitems.Add(new PriceItem()
                        {
                            from = from,
                            price = reader["price"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["price"]),
                            qty = reader["qty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["qty"]),
                            index = index++,
                            total = total
                        });
                        from = priceitems.Last().price;
                    }
                }
                connection.Close();
            }
            var result = new List<Insight.PriceRangeItem>();
            int percentposition = 10;
            List<decimal> pricebrackets = new List<decimal>();
            decimal diffValue = 0;
            foreach (var item in priceitems.OrderBy(x => x.price))
            {
                item.percentage = Math.Round(item.total / total * 100, 2);
                var price = item.price;
                if (item.percentage > percentposition)
                {
                    if (priceitems.IndexOf(item) + 1 < priceitems.Count &&
                        priceitems[priceitems.IndexOf(item) + 1].price != item.price)
                    {
                        if (pricebrackets.Count > 0)
                            diffValue += item.price - pricebrackets.Last();
                        pricebrackets.Add(item.price);
                        percentposition += 10;
                    }
                }
            }
            if (pricebrackets.Count > 0 && diffValue > 0)
            {
                var newpricerackets = new List<decimal>();
                var avgprice = Math.Ceiling((diffValue / pricebrackets.Count) / 10) * 10;

                for (int i = 0; i < pricebrackets.Count; i++)
                {
                    var newValue = avgprice > 0 ? Math.Ceiling(pricebrackets[i] / avgprice) * avgprice : 0;
                    if (newpricerackets.Count == 0 || newpricerackets.Max() < newValue)
                        newpricerackets.Add(newValue);
                }
                newpricerackets.Reverse();

                var pricerangequery = @"
with currentstock as
(
 select case {1} end as price,sum(salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty)) as cqty, sum((salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty))* stock.ibuyingprice::numeric) cstock from salablegoods
 inner join stock on stock.id = stockid and {0}
 where salablegoods.hasstock = true and salablegoods.isactive = true
 group by case {1} end
),currentpurchase as
(
 select case {3} end as price,sum(stock.qty-stock.returnqty) as pqty, 
 sum((stock.qty-stock.returnqty)* stock.ibuyingprice::numeric) pstock from lrentry
 inner join lrinvoice on lrinvoice.lrid = lrentry.id
 inner join lrinvoiceitems on lrinvoiceitems.lrinvoiceid = lrinvoice.id
 inner join stock on stock.invoiceitemid = lrinvoiceitems.id and {0}
 where stock.isactive = true
 and lrentry.lrentrydate>=@fromdate and lrentry.lrentrydate<@todate and lrentry.isactive = true
 group by case {3} end
),
currentsales as
(
  select case {2} end as price,sum((billitems.qty) * stock.ibuyingprice::numeric) costval,sum(billitems.qty) qty,sum(billitems.receivable) salesvalue from bill
  inner join billitems on billitems.billid = bill.id and billitems.isactive=true
  inner join stock on stock.id = stockid and {0}
  where bill.billdate>=@fromdate and bill.billdate<@todate and bill.billtype != 9 and bill.isactive = true
  group by case {2} end
)

select COALESCE(currentstock.price, currentsales.price,'UNKNOWN') as price,
COALESCE(currentstock.cqty,0) stockqty,COALESCE(currentstock.cstock,0) stockvalue,
COALESCE(currentsales.costval,0) soldcost,COALESCE(currentsales.qty,0) saleqty,
COALESCE(currentsales.salesvalue,0) salesvalue,
COALESCE(currentpurchase.pqty,0) purchaseqty,
COALESCE(currentpurchase.pstock,0) purchasevalue
from currentstock
full outer join currentsales on currentsales.price = currentstock.price
full outer join currentpurchase on currentpurchase.price = currentstock.price";

                System.Text.StringBuilder billpricelist = new System.Text.StringBuilder();
                System.Text.StringBuilder stockpricelist = new System.Text.StringBuilder();
                System.Text.StringBuilder purchasepricelist = new System.Text.StringBuilder();
                decimal tovalue = 0;
                foreach (var rng in newpricerackets)
                {
                    var caption = tovalue == 0 ? "Above " + rng.ToString("F0") : (rng.ToString("F0") + " - " + tovalue.ToString("F0"));
                    billpricelist.AppendLine("when (billitems.rate+billitems.itemtax)::numeric>" + rng.ToString("F0") + " then '" + caption + "'");
                    stockpricelist.AppendLine("when salablegoods.price>" + rng.ToString("F0") + " then '" + caption + "'");
                    purchasepricelist.AppendLine("when stock.price>" + rng.ToString("F0") + " then '" + caption + "'");
                    tovalue = rng;
                }
                billpricelist.AppendLine(" else 'Below " + tovalue.ToString("F0") + "'");
                stockpricelist.AppendLine(" else 'Below " + tovalue.ToString("F0") + "'");
                purchasepricelist.AppendLine(" else 'Below " + tovalue.ToString("F0") + "'");

                query = string.Format(pricerangequery, condition, stockpricelist.ToString(), billpricelist.ToString(), purchasepricelist.ToString());

                using (var connection = baseData.CreateConnection())
                {
                    if (Services.ConfigManager.Instance(Context.OrganizationID).Config.analyser_stock_removeaqty)
                        query = query.Replace("salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty)", "salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty+salablegoods.aqty)");

                    connection.Open();
                    NpgsqlCommand command = new NpgsqlCommand(query, connection);
                    command.Parameters.Add("fromdate", NpgsqlTypes.NpgsqlDbType.Date).Value = fromdate;
                    command.Parameters.Add("todate", NpgsqlTypes.NpgsqlDbType.Date).Value = todate;
                    command.CommandTimeout = 10000;

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var pindex = 0;
                            var price = reader["price"] == DBNull.Value ? "Unknown" : Convert.ToString(reader["price"]);
                            if (price != "Unknown")
                            {
                                var lastitem = price.Split(new char[] { ' ', '-' }).Last();
                                int.TryParse(lastitem, out pindex);
                            }
                            result.Add(new Insight.PriceRangeItem()
                            {
                                sortindex = pindex,
                                price = reader["price"] == DBNull.Value ? "Unknown" : Convert.ToString(reader["price"]),
                                saleqty = reader["saleqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["saleqty"]),
                                soldcost = reader["soldcost"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["soldcost"]),
                                stockqty = reader["stockqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["stockqty"]),
                                stockvalue = reader["stockvalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["stockvalue"]),
                                salesvalue = reader["salesvalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["salesvalue"]),
                                purchaseqty = reader["purchaseqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["purchaseqty"]),
                                purchasevalue = reader["purchasevalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["purchasevalue"])
                            }); ;
                        }
                    }
                    connection.Close();
                }
            }
            var totalqty = result.Sum(x => x.stockqty);
            result.ForEach(x =>
            {
                if (totalqty > 0)
                    x.pt = Math.Round(x.stockqty / totalqty * 100, 2);
                if (x.saleqty != 0 && x.purchaseqty != 0)
                    x.rt = x.saleqty / x.purchaseqty * 4;
            });
            return result;
        }

        public List<Insight.PriceRangeItem> ProductPriceRangesCache(long productid, string condition, DateTime fromdate, DateTime todate)
        {
            string query = string.Format(@"
            select floor(productprice) price,
            sum(qty) qty from mw_dailsalesdetails
            where productid=@productid and bldate>=@fromdate and bldate<@todate
            group by floor(productprice)", condition);

            var baseData = new InfyPOS.Common.BaseData() { Context = this.Context };
            List<PriceItem> priceitems = new List<PriceItem>();
            int index = 0;
            decimal total = 0;
            using (var connection = baseData.CreateConnection())
            {
                connection.Open();
                NpgsqlCommand command = new NpgsqlCommand(query, connection);
                command.Parameters.Add("fromdate", NpgsqlTypes.NpgsqlDbType.Date).Value = todate.AddDays(-7);
                command.Parameters.Add("todate", NpgsqlTypes.NpgsqlDbType.Date).Value = todate;
                command.Parameters.Add("productid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = productid;
                command.CommandTimeout = 10000;

                using (var reader = command.ExecuteReader())
                {
                    decimal from = 0;
                    while (reader.Read())
                    {
                        total += reader["qty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["qty"]);
                        priceitems.Add(new PriceItem()
                        {
                            from = from,
                            price = reader["price"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["price"]),
                            qty = reader["qty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["qty"]),
                            index = index++,
                            total = total
                        });
                        from = priceitems.Last().price;
                    }
                }
                connection.Close();
            }
            var result = new List<Insight.PriceRangeItem>();
            int percentposition = 10;
            List<decimal> pricebrackets = new List<decimal>();
            decimal diffValue = 0;
            foreach (var item in priceitems.OrderBy(x => x.price))
            {
                item.percentage = Math.Round(item.total / total * 100, 2);
                var price = item.price;
                if (item.percentage > percentposition)
                {
                    if (priceitems.IndexOf(item) + 1 < priceitems.Count &&
                        priceitems[priceitems.IndexOf(item) + 1].price != item.price)
                    {
                        if (pricebrackets.Count > 0)
                            diffValue += item.price - pricebrackets.Last();
                        pricebrackets.Add(item.price);
                        percentposition += 10;
                    }
                }
            }
            if (pricebrackets.Count > 0 && diffValue > 0)
            {
                var newpricerackets = new List<decimal>();
                var avgprice = Math.Ceiling((diffValue / pricebrackets.Count) / 10) * 10;

                for (int i = 0; i < pricebrackets.Count; i++)
                {
                    var newValue = avgprice > 0 ? Math.Ceiling(pricebrackets[i] / avgprice) * avgprice : 0;
                    if (newpricerackets.Count == 0 || newpricerackets.Max() < newValue)
                        newpricerackets.Add(newValue);
                }
                newpricerackets.Reverse();

                var pricerangequery = @"
with currentstock as
(
 select case {1} end as price,sum(stockqty) as cqty, 
 sum(stockvalue) cstock from mw_dailstockdetails
 where productid=@productid
 group by price
),currentsales as
(
  select case {2} end as price,sum(costprice) costval,
  sum(qty) qty,sum(qty*productprice) salesvalue from mw_dailsalesdetails
  where bldate>=@fromdate and bldate<@todate and productid=@productid
  group by price
),currentpurchase as
(
 select case {3} end as purprice,sum(qty) as pqty, 
 sum(amount) pstock 
 from mv_purchaseproductdaily
 where 
 lrentrydate>=@fromdate and lrentrydate<@todate and productid=@productid
 group by purprice
)

select COALESCE(currentstock.price, currentsales.price,'UNKNOWN') as price,
COALESCE(currentstock.cqty,0) stockqty,COALESCE(currentstock.cstock,0) stockvalue,
COALESCE(currentsales.costval,0) soldcost,COALESCE(currentsales.qty,0) saleqty,
COALESCE(currentsales.salesvalue,0) salesvalue,
COALESCE(currentpurchase.pqty,0) purchaseqty,
COALESCE(currentpurchase.pstock,0) purchasevalue
from currentstock
full outer join currentsales on currentsales.price = currentstock.price
full outer join currentpurchase on currentpurchase.purprice = currentstock.price";

                System.Text.StringBuilder billpricelist = new System.Text.StringBuilder();
                System.Text.StringBuilder stockpricelist = new System.Text.StringBuilder();
                System.Text.StringBuilder purchasepricelist = new System.Text.StringBuilder();
                decimal tovalue = 0;
                foreach (var rng in newpricerackets)
                {
                    var caption = tovalue == 0 ? "Above " + rng.ToString("F0") : (rng.ToString("F0") + " - " + tovalue.ToString("F0"));
                    stockpricelist.AppendLine("when productprice>" + rng.ToString("F0") + " then '" + caption + "'");
                    billpricelist.AppendLine("when productprice>" + rng.ToString("F0") + " then '" + caption + "'");
                    purchasepricelist.AppendLine("when price>" + rng.ToString("F0") + " then '" + caption + "'");
                    tovalue = rng;
                }
                billpricelist.AppendLine(" else 'Below " + tovalue.ToString("F0") + "'");
                stockpricelist.AppendLine(" else 'Below " + tovalue.ToString("F0") + "'");
                purchasepricelist.AppendLine(" else 'Below " + tovalue.ToString("F0") + "'");

                query = string.Format(pricerangequery, condition, stockpricelist.ToString(), billpricelist.ToString(), purchasepricelist.ToString());

                using (var connection = baseData.CreateConnection())
                {
                    if (Services.ConfigManager.Instance(Context.OrganizationID).Config.analyser_stock_removeaqty)
                        query = query.Replace("salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty)", "salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty+salablegoods.aqty)");

                    connection.Open();
                    NpgsqlCommand command = new NpgsqlCommand(query, connection);
                    command.Parameters.Add("fromdate", NpgsqlTypes.NpgsqlDbType.Date).Value = fromdate;
                    command.Parameters.Add("todate", NpgsqlTypes.NpgsqlDbType.Date).Value = todate;
                    command.Parameters.Add("productid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = productid;
                    command.CommandTimeout = 10000;

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var pindex = 0;
                            var price = reader["price"] == DBNull.Value ? "Unknown" : Convert.ToString(reader["price"]);
                            if (price != "Unknown")
                            {
                                var lastitem = price.Split(new char[] { ' ', '-' }).Last();
                                int.TryParse(lastitem, out pindex);
                            }
                            result.Add(new Insight.PriceRangeItem()
                            {
                                sortindex = pindex,
                                price = reader["price"] == DBNull.Value ? "Unknown" : Convert.ToString(reader["price"]),
                                saleqty = reader["saleqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["saleqty"]),
                                soldcost = reader["soldcost"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["soldcost"]),
                                stockqty = reader["stockqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["stockqty"]),
                                stockvalue = reader["stockvalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["stockvalue"]),
                                salesvalue = reader["salesvalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["salesvalue"]),
                                purchaseqty = reader["purchaseqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["purchaseqty"]),
                                purchasevalue = reader["purchasevalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["purchasevalue"])
                            }); ;
                        }
                    }
                    connection.Close();
                }
            }
            var totalqty = result.Sum(x => x.stockqty);
            result.ForEach(x =>
            {
                if (totalqty > 0)
                    x.pt = Math.Round(x.stockqty / totalqty * 100, 2);
                if (x.saleqty != 0 && x.purchaseqty != 0)
                    x.rt = x.saleqty / x.purchaseqty * 4;
            });
            return result;
        }

        public List<Insight.PriceRangeItem> ProductPriceRanges(long productid, string condition, DateTime fromdate, DateTime todate)
        {
            string query = string.Format(@"
            select floor((billitems.rate+billitems.itemtax)::numeric) price,
            sum(billitems.qty) qty from bill
            inner join billitems on billitems.billid = bill.id and billitems.isactive=true and billitems.productid=@productid
            where bill.billdate>=@fromdate and bill.billdate<@todate and bill.billtype != 9 and bill.isactive = true
            group by floor((billitems.rate+billitems.itemtax)::numeric)", condition);

            var baseData = new InfyPOS.Common.BaseData() { Context = this.Context };
            List<PriceItem> priceitems = new List<PriceItem>();
            int index = 0;
            decimal total = 0;
            using (var connection = baseData.CreateConnection())
            {
                connection.Open();
                NpgsqlCommand command = new NpgsqlCommand(query, connection);
                command.Parameters.Add("fromdate", NpgsqlTypes.NpgsqlDbType.Date).Value = todate.AddDays(-7);
                command.Parameters.Add("todate", NpgsqlTypes.NpgsqlDbType.Date).Value = todate;
                command.Parameters.Add("productid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = productid;
                command.CommandTimeout = 10000;

                using (var reader = command.ExecuteReader())
                {
                    decimal from = 0;
                    while (reader.Read())
                    {
                        total += reader["qty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["qty"]);
                        priceitems.Add(new PriceItem()
                        {
                            from = from,
                            price = reader["price"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["price"]),
                            qty = reader["qty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["qty"]),
                            index = index++,
                            total = total
                        });
                        from = priceitems.Last().price;
                    }
                }
                connection.Close();
            }
            var result = new List<Insight.PriceRangeItem>();
            int percentposition = 10;
            List<decimal> pricebrackets = new List<decimal>();
            decimal diffValue = 0;
            foreach (var item in priceitems.OrderBy(x => x.price))
            {
                item.percentage = Math.Round(item.total / total * 100, 2);
                var price = item.price;
                if (item.percentage > percentposition)
                {
                    if (priceitems.IndexOf(item) + 1 < priceitems.Count &&
                        priceitems[priceitems.IndexOf(item) + 1].price != item.price)
                    {
                        if (pricebrackets.Count > 0)
                            diffValue += item.price - pricebrackets.Last();
                        pricebrackets.Add(item.price);
                        percentposition += 10;
                    }
                }
            }
            if (pricebrackets.Count > 0 && diffValue > 0)
            {
                var newpricerackets = new List<decimal>();
                var avgprice = Math.Ceiling((diffValue / pricebrackets.Count) / 10) * 10;

                for (int i = 0; i < pricebrackets.Count; i++)
                {
                    var newValue = avgprice > 0 ? Math.Ceiling(pricebrackets[i] / avgprice) * avgprice : 0;
                    if (newpricerackets.Count == 0 || newpricerackets.Max() < newValue)
                        newpricerackets.Add(newValue);
                }
                newpricerackets.Reverse();

                var pricerangequery = @"
with currentstock as
(
 select case {1} end as price,sum(salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty)) as cqty, sum((salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty))* stock.ibuyingprice::numeric) cstock from salablegoods
 inner join stock on stock.id = stockid and {0}
 where salablegoods.hasstock = true and salablegoods.isactive = true
 group by case {1} end
),currentpurchase as
(
 select case {3} end as price,sum(stock.qty-stock.returnqty) as pqty, 
 sum((stock.qty-stock.returnqty)* stock.ibuyingprice::numeric) pstock from lrentry
 inner join lrinvoice on lrinvoice.lrid = lrentry.id
 inner join lrinvoiceitems on lrinvoiceitems.lrinvoiceid = lrinvoice.id
 inner join stock on stock.invoiceitemid = lrinvoiceitems.id and {0}
 where stock.isactive = true
 and lrentry.lrentrydate>=@fromdate and lrentry.lrentrydate<@todate and lrentry.isactive = true
 group by case {3} end
),
currentsales as
(
  select case {2} end as price,sum((billitems.qty) * stock.ibuyingprice::numeric) costval,sum(billitems.qty) qty,sum(billitems.receivable) salesvalue from bill
  inner join billitems on billitems.billid = bill.id and billitems.isactive=true
  inner join stock on stock.id = stockid and {0}
  where bill.billdate>=@fromdate and bill.billdate<@todate and bill.billtype != 9 and bill.isactive = true
  group by case {2} end
)

select COALESCE(currentstock.price, currentsales.price,'UNKNOWN') as price,
COALESCE(currentstock.cqty,0) stockqty,COALESCE(currentstock.cstock,0) stockvalue,
COALESCE(currentsales.costval,0) soldcost,COALESCE(currentsales.qty,0) saleqty,
COALESCE(currentsales.salesvalue,0) salesvalue,
COALESCE(currentpurchase.pqty,0) purchaseqty,
COALESCE(currentpurchase.pstock,0) purchasevalue
from currentstock
full outer join currentsales on currentsales.price = currentstock.price
full outer join currentpurchase on currentpurchase.price = currentstock.price";

                System.Text.StringBuilder billpricelist = new System.Text.StringBuilder();
                System.Text.StringBuilder stockpricelist = new System.Text.StringBuilder();
                System.Text.StringBuilder purchasepricelist = new System.Text.StringBuilder();
                decimal tovalue = 0;
                foreach (var rng in newpricerackets)
                {
                    var caption = tovalue == 0 ? "Above " + rng.ToString("F0") : (rng.ToString("F0") + " - " + tovalue.ToString("F0"));
                    billpricelist.AppendLine("when (billitems.rate+billitems.itemtax)::numeric>" + rng.ToString("F0") + " then '" + caption + "'");
                    stockpricelist.AppendLine("when salablegoods.price>" + rng.ToString("F0") + " then '" + caption + "'");
                    purchasepricelist.AppendLine("when stock.price>" + rng.ToString("F0") + " then '" + caption + "'");
                    tovalue = rng;
                }
                billpricelist.AppendLine(" else 'Below " + tovalue.ToString("F0") + "'");
                stockpricelist.AppendLine(" else 'Below " + tovalue.ToString("F0") + "'");
                purchasepricelist.AppendLine(" else 'Below " + tovalue.ToString("F0") + "'");

                query = string.Format(pricerangequery, condition, stockpricelist.ToString(), billpricelist.ToString(), purchasepricelist.ToString());

                using (var connection = baseData.CreateConnection())
                {
                    if (Services.ConfigManager.Instance(Context.OrganizationID).Config.analyser_stock_removeaqty)
                        query = query.Replace("salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty)", "salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty+salablegoods.aqty)");

                    connection.Open();
                    NpgsqlCommand command = new NpgsqlCommand(query, connection);
                    command.Parameters.Add("fromdate", NpgsqlTypes.NpgsqlDbType.Date).Value = fromdate;
                    command.Parameters.Add("todate", NpgsqlTypes.NpgsqlDbType.Date).Value = todate;
                    command.CommandTimeout = 10000;

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var pindex = 0;
                            var price = reader["price"] == DBNull.Value ? "Unknown" : Convert.ToString(reader["price"]);
                            if (price != "Unknown")
                            {
                                var lastitem = price.Split(new char[] { ' ', '-' }).Last();
                                int.TryParse(lastitem, out pindex);
                            }
                            result.Add(new Insight.PriceRangeItem()
                            {
                                sortindex = pindex,
                                price = reader["price"] == DBNull.Value ? "Unknown" : Convert.ToString(reader["price"]),
                                saleqty = reader["saleqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["saleqty"]),
                                soldcost = reader["soldcost"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["soldcost"]),
                                stockqty = reader["stockqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["stockqty"]),
                                stockvalue = reader["stockvalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["stockvalue"]),
                                salesvalue = reader["salesvalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["salesvalue"]),
                                purchaseqty = reader["purchaseqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["purchaseqty"]),
                                purchasevalue = reader["purchasevalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["purchasevalue"])
                            }); ;
                        }
                    }
                    connection.Close();
                }
            }
            var totalqty = result.Sum(x => x.stockqty);
            result.ForEach(x =>
            {
                if (totalqty > 0)
                    x.pt = Math.Round(x.stockqty / totalqty * 100, 2);
                if (x.saleqty != 0 && x.purchaseqty != 0)
                    x.rt = x.saleqty / x.purchaseqty * 4;
            });
            return result;
        }

        public List<Insight.PriceRangeItem> LocationPriceRanges(long locationid, DateTime fromdate, DateTime todate)
        {


            var result = new List<Insight.PriceRangeItem>();
            List<decimal> newpricerackets = new List<decimal>();
            newpricerackets.AddRange(new decimal[] { 50, 100, 150, 200, 250, 350, 500, 750, 1000, 1500, 2000, 3000, 4000, 5000, 10000, 15000, 20000, 25000, 40000, 50000, 75000, 100000 });
            newpricerackets.Reverse();

            var pricerangequery = @"
with currentstock as
(
 select case {1} end as price,sum(salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty)) as cqty, sum((salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty))* stock.ibuyingprice::numeric) cstock from salablegoods
 inner join stock on stock.id = stockid
 where salablegoods.hasstock = true and salablegoods.isactive = true and salablegoods.locationid=@locationid
 group by case {1} end
),currentpurchase as
(
 select case {3} end as price,sum(salablegoods.iqty) as pqty, 
 sum((salablegoods.iqty)* stock.ibuyingprice::numeric) pstock from lrentry
 inner join lrinvoice on lrinvoice.lrid = lrentry.id
 inner join lrinvoiceitems on lrinvoiceitems.lrinvoiceid = lrinvoice.id
 inner join stock on stock.invoiceitemid = lrinvoiceitems.id
 inner join salablegoods on salablegoods.stockid = stock.id and salablegoods.locationid=@locationid {0}
 where stock.isactive = true
 and lrentry.lrentrydate>=@fromdate and lrentry.lrentrydate<@todate and lrentry.isactive = true
 group by case {3} end
),
currentsales as
(
  select case {2} end as price,sum((billitems.qty) * stock.ibuyingprice::numeric) costval,sum(billitems.qty) qty,sum(billitems.receivable) salesvalue from bill
  inner join billitems on billitems.billid = bill.id and billitems.isactive=true
  inner join stock on stock.id = stockid
  where bill.billdate>=@fromdate and bill.billdate<@todate and bill.billtype != 9 and bill.isactive = true and bill.locationid=@locationid
  group by case {2} end
)

select COALESCE(currentstock.price, currentsales.price,'UNKNOWN') as price,
COALESCE(currentstock.cqty,0) stockqty,COALESCE(currentstock.cstock,0) stockvalue,
COALESCE(currentsales.costval,0) soldcost,COALESCE(currentsales.qty,0) saleqty,
COALESCE(currentsales.salesvalue,0) salesvalue,
COALESCE(currentpurchase.pqty,0) purchaseqty,
COALESCE(currentpurchase.pstock,0) purchasevalue
from currentstock
full outer join currentsales on currentsales.price = currentstock.price
full outer join currentpurchase on currentpurchase.price = currentstock.price";

            if (locationid <= 0)
                pricerangequery = pricerangequery.Replace("and bill.locationid=@locationid", "").Replace("and salablegoods.locationid=@locationid", "");
            System.Text.StringBuilder billpricelist = new System.Text.StringBuilder();
            System.Text.StringBuilder stockpricelist = new System.Text.StringBuilder();
            System.Text.StringBuilder purchasepricelist = new System.Text.StringBuilder();
            decimal tovalue = 0;
            foreach (var rng in newpricerackets)
            {
                var caption = tovalue == 0 ? "Above " + rng.ToString("F0") : (rng.ToString("F0") + " - " + tovalue.ToString("F0"));
                billpricelist.AppendLine("when (billitems.rate+billitems.itemtax)::numeric>" + rng.ToString("F0") + " then '" + caption + "'");
                stockpricelist.AppendLine("when salablegoods.price>" + rng.ToString("F0") + " then '" + caption + "'");
                purchasepricelist.AppendLine("when stock.price>" + rng.ToString("F0") + " then '" + caption + "'");
                tovalue = rng;
            }
            billpricelist.AppendLine(" else 'Below " + tovalue.ToString("F0") + "'");
            stockpricelist.AppendLine(" else 'Below " + tovalue.ToString("F0") + "'");
            purchasepricelist.AppendLine(" else 'Below " + tovalue.ToString("F0") + "'");

            var query = string.Format(pricerangequery, "", stockpricelist.ToString(), billpricelist.ToString(), purchasepricelist.ToString());
            var baseData = new Common.BaseData() { Context = this.Context };
            using (var connection = baseData.CreateConnection())
            {
                connection.Open();
                if (Services.ConfigManager.Instance(Context.OrganizationID).Config.analyser_stock_removeaqty)
                    query = query.Replace("salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty)", "salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty+salablegoods.aqty)");

                NpgsqlCommand command = new NpgsqlCommand(query, connection);
                command.Parameters.Add("fromdate", NpgsqlTypes.NpgsqlDbType.Date).Value = fromdate;
                command.Parameters.Add("todate", NpgsqlTypes.NpgsqlDbType.Date).Value = todate;
                command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = locationid;
                command.CommandTimeout = 10000;

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var pindex = 0;
                        var price = reader["price"] == DBNull.Value ? "Unknown" : Convert.ToString(reader["price"]);
                        if (price != "Unknown")
                        {
                            var lastitem = price.Split(new char[] { ' ', '-' }).Last();
                            int.TryParse(lastitem, out pindex);
                        }
                        result.Add(new Insight.PriceRangeItem()
                        {
                            sortindex = pindex,
                            price = reader["price"] == DBNull.Value ? "Unknown" : Convert.ToString(reader["price"]),
                            saleqty = reader["saleqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["saleqty"]),
                            soldcost = reader["soldcost"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["soldcost"]),
                            stockqty = reader["stockqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["stockqty"]),
                            stockvalue = reader["stockvalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["stockvalue"]),
                            salesvalue = reader["salesvalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["salesvalue"]),
                            purchaseqty = reader["purchaseqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["purchaseqty"]),
                            purchasevalue = reader["purchasevalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["purchasevalue"])
                        });
                    }
                }
                connection.Close();
            }
            //result.ForEach(x => x.rt = (x.saleqty>0 && x.stockqty>0) ? (x.saleqty / x.stockqty * 100 / 100 * 4):0);
            var totalqty = result.Sum(x => x.stockqty);
            result.ForEach(x =>
            {
                if (totalqty > 0 && x.stockqty > 0)
                    x.pt = Math.Round(x.stockqty / totalqty * 100, 2);

                if (x.saleqty > 0 && x.purchaseqty > 0)
                    x.rt = x.saleqty / x.purchaseqty * 4;
            });
            return result.OrderBy(x => x.sortindex).ToList();
        }

        public Insight Product(long productid, string locationid)
        {
            if (!Services.ConfigManager.Instance(Context.OrganizationID).Config.analyser_data_cache)
                return new MobileInsightOLD().Product(productid, locationid);

                long locid;
            long.TryParse(locationid, out locid);
            string fullquery = @"select name,stockholdingperiod from products where id=@productid;

select case when EXTRACT(day from @current_date - stock.stockdate)<30 then 'below 30 days'
when EXTRACT(day from @current_date - stock.stockdate)<61 then '30 - 60 days'
when EXTRACT(day from @current_date - stock.stockdate)<91 then '60 - 90 days'
when EXTRACT(day from @current_date - stock.stockdate)<121 then '90 - 120 days'
when EXTRACT(day from @current_date - stock.stockdate)<151 then '120 - 150 days'
when EXTRACT(day from @current_date - stock.stockdate)<181 then '150 - 180 days'
else 'above 180 days' end as 
period,sum(salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty)) as qty, 
sum((salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty))*stock.ibuyingprice::numeric) as value from salablegoods
from stock
inner join salablegoods on stock.id = stockid and salablegoods.hasstock = true
and salablegoods.isactive = true and salablegoods.locationid = @locationid
inner join stock on stock.id = stockid and stock.productid = @productid
where stock.productid = @productid
group by period;

select case when EXTRACT(day from bill.billdate - stock.stockdate)<30 then 'below 30 days'
	when EXTRACT(day from bill.billdate - stock.stockdate)<61 then '30 - 60 days'
	when EXTRACT(day from bill.billdate - stock.stockdate)<91 then '60 - 90 days'
	when EXTRACT(day from bill.billdate - stock.stockdate)<121 then '90 - 120 days'
    when EXTRACT(day from bill.billdate - stock.stockdate)<151 then '120 - 150 days'
    when EXTRACT(day from bill.billdate - stock.stockdate)<181 then '150 - 180 days'
	else 'above 180 days' end as 
	period,sum(billitems.qty) as qty from bill
inner join billitems on billitems.billid = bill.id and billitems.isactive=true
inner join stock on stock.id = stockid and stock.productid = @productid
where bill.billdate>=@todayfromdate and bill.billdate<@todaytodate and bill.billtype != 9 and bill.isactive = true
and bill.locationid = @locationid
group by period;";

            fullquery = @"select name,stockholdingperiod from products where id=@productid;

select case when (@current_date - stock.stockdate)<30 then 'below 30 days'
when (@current_date - stock.stockdate)<61 then '30 - 60 days'
when (@current_date - stock.stockdate)<91 then '60 - 90 days'
when (@current_date - stock.stockdate)<121 then '90 - 120 days'
when (@current_date - stock.stockdate)<151 then '120 - 150 days'
when (@current_date - stock.stockdate)<181 then '150 - 180 days'
else 'above 180 days' end as 
period,sum(stockqty) as qty, 
sum(productprice*stockqty) as value from mw_dailstockdetails stock
where stock.productid = @productid and stock.locationid = @locationid
group by period;

select case when EXTRACT(day from bill.billdate - stock.stockdate)<30 then 'below 30 days'
	when EXTRACT(day from bill.billdate - stock.stockdate)<61 then '30 - 60 days'
	when EXTRACT(day from bill.billdate - stock.stockdate)<91 then '60 - 90 days'
	when EXTRACT(day from bill.billdate - stock.stockdate)<121 then '90 - 120 days'
    when EXTRACT(day from bill.billdate - stock.stockdate)<151 then '120 - 150 days'
    when EXTRACT(day from bill.billdate - stock.stockdate)<181 then '150 - 180 days'
	else 'above 180 days' end as 
	period,sum(billitems.qty) as qty from bill
inner join billitems on billitems.billid = bill.id and billitems.isactive=true
inner join stock on stock.id = stockid and stock.productid = @productid
where bill.billdate>=@todayfromdate and bill.billdate<@todaytodate and bill.billtype != 9 and bill.isactive = true
and bill.locationid = @locationid
group by period;";

            var result = new Insight();

            if(Services.ConfigManager.Instance(Context.OrganizationID).Config.analyser_data_cache)
                result.SalesData = new AnalyticalCubes().GetProductCurrentYearSales(productid);
            else
                result.SalesData = new AnalyticalCubesOLD().GetProductCurrentYearSales(productid);

            var todate = GetQuarterLast(DateTimeNow.Date).AddDays(1);
            var baseData = new Common.BaseData() { Context = this.Context };
            using (var connection = baseData.CreateConnection())
            {
                if (Services.ConfigManager.Instance(Context.OrganizationID).Config.analyser_stock_removeaqty)
                    fullquery = fullquery.Replace("salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty)", "salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty+salablegoods.aqty)");

                if (locid <= 0)
                {
                    fullquery = fullquery.Replace("and salablegoods.locationid = @locationid", "").Replace("and bill.locationid = @locationid", "").Replace("and stock.locationid = @locationid", "");
                }

                try
                {
                    connection.Open();
                    NpgsqlCommand command = new NpgsqlCommand(fullquery, connection);
                    command.Parameters.Add("fromdate", NpgsqlTypes.NpgsqlDbType.Date).Value = todate.AddYears(-1);
                    command.Parameters.Add("todate", NpgsqlTypes.NpgsqlDbType.Date).Value = todate;
                    command.Parameters.Add("current_date", NpgsqlTypes.NpgsqlDbType.Date).Value = DateTimeNow.Date;
                    command.Parameters.Add("todayfromdate", NpgsqlTypes.NpgsqlDbType.Date).Value = DateTimeNow.Date;
                    command.Parameters.Add("todaytodate", NpgsqlTypes.NpgsqlDbType.Date).Value = DateTimeNow.Date.AddDays(1);
                    command.Parameters.Add("quarterfromdate", NpgsqlTypes.NpgsqlDbType.Date).Value = new DateTime(DateTimeNow.Year, DateTimeNow.Month, 1).AddMonths(-3);
                    command.Parameters.Add("quartertodate", NpgsqlTypes.NpgsqlDbType.Date).Value = DateTimeNow.Date.AddDays(1);
                    command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = locid;
                    command.Parameters.Add("productid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = productid;
                    command.CommandTimeout = 10000;
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            result.display = reader.GetString(0);
                            result.stockholding = reader.IsDBNull(1) ? 0 : reader.GetInt32(1);
                        }

                        if (reader.NextResult())
                        {
                            result.StockData = new List<Insight.StockItem>();
                            while (reader.Read())
                            {
                                result.StockData.Add(new Insight.StockItem()
                                {
                                    Period = reader["period"] != DBNull.Value ? Convert.ToString(reader["period"]) : "",
                                    Qty = reader["qty"] != DBNull.Value ? Convert.ToDecimal(reader["qty"]) : 0,
                                    Value = reader["value"] != DBNull.Value ? Convert.ToDecimal(reader["value"]) : 0,
                                    Index = IndexOfDays(reader["period"] != DBNull.Value ? Convert.ToString(reader["period"]) : "")
                                });
                            }
                            result.StockDataTotalQty = result.StockData.Sum(e => e.Qty);
                            result.StockDataTotalValue = result.StockData.Sum(e => e.Value);
                        }
                        if (reader.NextResult())
                        {
                            while (reader.Read())
                            {
                                var item = new Insight.StockItem()
                                {
                                    Period = reader["period"] != DBNull.Value ? Convert.ToString(reader["period"]) : "",
                                    Sold = reader["qty"] != DBNull.Value ? Convert.ToDecimal(reader["qty"]) : 0,
                                    Index = IndexOfDays(reader["period"] != DBNull.Value ? Convert.ToString(reader["period"]) : "")
                                };
                                if (result.StockData.Exists(e => e.Period == item.Period))
                                {
                                    result.StockData.Find(e => e.Period == item.Period).Sold = item.Sold;
                                }
                                else
                                {
                                    result.StockData.Add(item);
                                }
                            }
                            result.StockDataTotalSold = result.StockData.Sum(e => e.Sold);
                        }
                    }

                    result.PurchaseData = new List<Insight.SalesItem>();

                    DateTime aviladate = DateTime.MinValue;
                    using (var reader = ExecuteReader("select lastDateValue from Cache_View where viewname='mv_purchaseproductdaily' limit 1", null))
                    {
                        if (reader.Read() && !reader.IsDBNull(0))
                            aviladate = reader.GetDateTime(0);
                    }

                    var param = new SortedList<string, object>();
                    param.Add("fromdate", todate.AddYears(-1));
                    param.Add("todate", todate);
                    param.Add("productid", productid);
                    using (var reader = ExecuteReader(@"select quarter,year,sum(cost) cost,sum(qty) qty from mv_montlypurchasebyproduct 
                                        where productid=@productid and datebegin>=@fromdate and datebegin<@todate
                                        group by quarter,year", param))
                    {
                        while (reader.Read())
                        {
                            result.PurchaseData.Add(new Insight.SalesItem()
                            {
                                Qty = reader["qty"] != DBNull.Value ? Convert.ToDecimal(reader["qty"]) : 0,
                                Cost = reader["Cost"] != DBNull.Value ? Convert.ToDecimal(reader["Cost"]) : 0,
                                Quarter = reader["Quarter"] != DBNull.Value ? Convert.ToInt32(reader["Quarter"]) : 0,
                                Year = reader["Year"] != DBNull.Value ? Convert.ToInt32(reader["Year"]) : 0,
                            });
                        }
                    }

                    if (todate > aviladate)
                    {
                        param = new SortedList<string, object>();
                        param.Add("fromdate", aviladate);
                        param.Add("todate", todate);
                        param.Add("productid", productid);
                        using (var reader = ExecuteReader(@"select sum(stock.qty - (stock.returnqty+stock.damagedqty+stock.journalqty)) as qty,
                        sum((stock.qty - (stock.returnqty+stock.damagedqty+stock.journalqty))::numeric*stock.ibuyingprice::numeric) as cost,
                        EXTRACT(quarter from stock.stockdate) as quarter,EXTRACT(year from stock.stockdate) as year from stock
                        where stock.productid = @productid and stock.stockdate>=@fromdate and stock.stockdate<@todate
                        group by EXTRACT(quarter from stock.stockdate),EXTRACT(year from stock.stockdate);", param))
                        {
                            while (reader.Read())
                            {
                                var pd = new Insight.SalesItem()
                                {
                                    Qty = reader["qty"] != DBNull.Value ? Convert.ToDecimal(reader["qty"]) : 0,
                                    Cost = reader["Cost"] != DBNull.Value ? Convert.ToDecimal(reader["Cost"]) : 0,
                                    Quarter = reader["Quarter"] != DBNull.Value ? Convert.ToInt32(reader["Quarter"]) : 0,
                                    Year = reader["Year"] != DBNull.Value ? Convert.ToInt32(reader["Year"]) : 0,
                                };
                                if (result.PurchaseData.Exists(x => x.Quarter == pd.Quarter &&
                                x.Year == pd.Year))
                                {
                                    var xpd = result.PurchaseData.Find(x => x.Quarter == pd.Quarter &&
                                              x.Year == pd.Year);
                                    xpd.Cost += pd.Cost;
                                    xpd.Qty += pd.Qty;
                                }
                                else
                                {
                                    result.PurchaseData.Add(pd);
                                }
                            }
                        }
                    }

                    result.PurchaseData.ForEach(e =>
                    {
                        e.Quarter -= 1;
                        if (e.Quarter == 0)
                        {
                            e.Quarter = 4;
                            e.Year = e.Year - 1;
                        }
                    });
                }
                catch (Exception exp)
                {
                    WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, "MobileInsight.Supplier." + productid.ToString("F0"));
                }
                finally
                {
                    connection.Close();
                }
            }


            result.InsightData = new List<Insight.DataItem>();
            result.PurchaseData.ForEach(e =>
            {
                result.InsightData.Add(new Insight.DataItem()
                {
                    Purchase = e.Cost,
                    PurchaseQty = e.Qty,
                    Quarter = e.Quarter,
                    Year = e.Year,
                    Display = string.Format("{0}({1})", GetFinanceYear(e.Year), quaterdisplay[e.Quarter - 1])
                });
            });

            result.SalesData.ForEach(e =>
            {
                if (result.InsightData.Exists(x => x.Year == e.Year && x.Quarter == e.Quarter))
                {
                    var edata = result.InsightData.Find(x => x.Year == e.Year && x.Quarter == e.Quarter);
                    edata.Sales = e.Amount;
                    edata.SalesQty = e.Qty;
                    edata.Profit = e.Amount - e.Cost;
                    edata.Display = string.Format("{0}({1})", GetFinanceYear(e.Year), quaterdisplay[e.Quarter - 1]);
                }
                else
                {
                    result.InsightData.Add(new Insight.DataItem()
                    {
                        Sales = e.Cost,
                        SalesQty = e.Qty,
                        Quarter = e.Quarter,
                        Year = e.Year,
                        Profit = e.Amount - e.Cost,
                        Display = string.Format("{0}({1})", GetFinanceYear(e.Year), quaterdisplay[e.Quarter - 1])
                    });
                }
            });

            result.StockData.Sort(delegate (Insight.StockItem a, Insight.StockItem b)
            {
                return a.Index.CompareTo(b.Index);
            });
            result.SalesData.Sort(delegate (Insight.SalesItem a, Insight.SalesItem b)
            {
                return (a.Year * 12 + a.Quarter).CompareTo(b.Year * 12 + b.Quarter);
            });
            result.PurchaseData.Sort(delegate (Insight.SalesItem a, Insight.SalesItem b)
            {
                return (a.Year * 12 + a.Quarter).CompareTo(b.Year * 12 + b.Quarter);
            });
            result.ClosingAmount = result.StockData.Sum(e => e.Value);
            result.ClosingQty = result.StockData.Sum(e => e.Qty);
            result.Closing = result.ClosingAmount;
            result.SalesCost = result.SalesData.Sum(e => e.Cost);
            result.SalesQty = result.SalesData.Sum(e => e.Qty);
            result.SalesAmount = result.SalesData.Sum(e => e.Amount);
            result.PurchaseCost = result.PurchaseData.Sum(e => e.Cost);
            result.PurchaseQty = result.PurchaseData.Sum(e => e.Qty);
            result.OpeningCost = result.ClosingAmount - result.PurchaseCost + result.SalesCost;
            result.OpeningQty = result.ClosingQty - result.PurchaseQty + result.SalesQty;
            result.PriceData = ProductPriceRangesCache(productid, " stock.productid = " + productid.ToString("F0"), DateTimeNow.AddMonths(-3), DateTimeNow.Date);
            result.PriceData.Sort(delegate (Insight.PriceRangeItem a, Insight.PriceRangeItem b)
            {
                return a.sortindex.CompareTo(b.sortindex);
            });


            return result;
        }


        static int[] quarters = new int[] { 4, 4, 4, 1, 1, 1, 2, 2, 2, 3, 3, 3 };
        public static int GetQuarter(DateTime date)
        {
            return quarters[date.Month - 1];
        }

        public int IndexOfDays(string source)
        {

            if (source.StartsWith("below 30")) return 1;
            if (source.StartsWith("30 - 60")) return 2;
            if (source.StartsWith("60 - 90")) return 3;
            if (source.StartsWith("90 - 120")) return 4;
            if (source.StartsWith("120 - 150")) return 5;
            if (source.StartsWith("150 - 180")) return 6;

            return 8;
        }

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

        public class DetailShowResponse
        {
            public string[] modes { get; set; }
            public List<DisplayItem> products { get; set; }
            public List<DisplayItem> sections { get; set; }
            public List<DisplayItem> suppliers { get; set; }

            public class DetailItem : DisplayItem
            {
                public string product { get; set; }
                public string supplier { get; set; }
                public string section { get; set; }
            }
            public class DisplayItem
            {
                public string display { get; set; }
                public decimal stockqty { get; set; }
                public decimal stockvalue { get; set; }
                public decimal soldqty { get; set; }
                public decimal soldvalue { get; set; }
                public decimal soldcost { get; set; }
                public decimal purchaseqty { get; set; }
                public decimal purchasevalue { get; set; }
            }

        }
        public DetailShowResponse DetailView(DetailShowRequest item)
        {
            item.todate = DateTime.Now.Date.AddDays(1);
            item.fromdate = item.todate.AddMonths(-3);
            if (!string.IsNullOrEmpty(item.days))
            {
                if (item.days.Contains("below"))
                {
                    item.todays = 31;
                }
                else if (item.days.Contains("above"))
                {
                    item.fromdays = 180;
                    item.todays = 50000;
                }
                else
                {
                    var days = item.days.Replace(" days", "").Split('-');
                    item.fromdays = int.Parse(days[0].Trim());
                    item.todays = int.Parse(days[1].Trim());
                }
            }

            if (!string.IsNullOrEmpty(item.price))
            {
                if (item.price.Contains("Below"))
                {
                    item.priceto = int.Parse(item.price.Replace("Below", "").Trim());
                }
                else if (item.price.Contains("Above"))
                {
                    item.pricefrom = int.Parse(item.price.Replace("Above", "").Trim());
                    item.priceto += item.priceto;
                }
                else
                {
                    var days = item.price.Split('-');
                    item.pricefrom = int.Parse(days[0].Trim());
                    item.priceto = int.Parse(days[1].Trim()) + 1;
                }
                item.priceto++;
            }

            string agequery = @"
            select  
            products.name productname,supplier.name suppliername,section.name sectoinname,
            sum(salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty)) as stockqty, 
            sum((salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty))*stock.ibuyingprice::numeric) as stockvalue
            from salablegoods
            inner join stock on stock.id = stockid
            left outer join products on products.id = stock.productid
            left outer join supplier on supplier.id = stock.supplierid
            left outer join referencelist section on section.id = products.salescounterid
            where salablegoods.hasstock = true and salablegoods.isactive = true 
            and salablegoods.locationid = @locationid
            and EXTRACT(day from @current_date - stock.stockdate)>=@fromdays
            and EXTRACT(day from @current_date - stock.stockdate)<@todays
            and salablegoods.locationid=@locationid
            and stock.productid = @productid
            and stock.supplierid = @supplierid
            group by productname,suppliername,sectoinname;

            select 
            sum((billitems.qty) * stock.ibuyingprice::numeric) soldcost,
            sum(billitems.qty) as soldqty,
            sum(billitems.receivable) as soldvalue,
            products.name productname,supplier.name suppliername,section.name sectoinname
            from bill
            inner join billitems on billitems.billid = bill.id and billitems.isactive=true
            inner join stock on stock.id = stockid
            left outer join products on products.id = stock.productid
            left outer join supplier on supplier.id = stock.supplierid
            left outer join referencelist section on section.id = products.salescounterid
            where bill.billdate>=@fromdate and bill.billdate<@todate and bill.billtype != 9 
            and bill.isactive = true
            and bill.locationid = @locationid 
            and EXTRACT(day from bill.billdate - stock.stockdate)>=@fromdays
            and EXTRACT(day from bill.billdate - stock.stockdate)<@todays
            and bill.locationid = @locationid
            and stock.productid = @productid
            and stock.supplierid = @supplierid
            group by productname,suppliername,sectoinname";


            if (item.productid == 0)
                agequery = agequery.Replace("and stock.productid = @productid", "");
            if (item.supplierid == 0)
                agequery = agequery.Replace("and stock.supplierid = @supplierid", "");
            if (item.locationid == 0)
            {
                agequery = agequery.Replace("and lrentry.retaillocationid = @locationid", "");
                agequery = agequery.Replace("and bill.locationid = @locationid", "");
                agequery = agequery.Replace("and salablegoods.locationid = @locationid", "");
            }

            string pricequery = @"select 
            products.name productname,supplier.name suppliername,section.name sectoinname,
            sum(salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty)) as stockqty, 
            sum((salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty))* stock.ibuyingprice::numeric) stockvalue from salablegoods
            inner join stock on stock.id = stockid 
            left outer join products on products.id = stock.productid
            left outer join supplier on supplier.id = stock.supplierid
            left outer join referencelist section on section.id = products.salescounterid	
            where salablegoods.hasstock = true and salablegoods.isactive = true
            and (salablegoods.price>@fromprice and salablegoods.price<@toprice)
            and salablegoods.locationid=@locationid
            and stock.productid = @productid
            and stock.supplierid = @supplierid
            group by productname,suppliername,sectoinname;

            select products.name productname,supplier.name suppliername,section.name sectoinname,
            sum((billitems.qty) * stock.ibuyingprice::numeric) soldcost,
            sum(billitems.qty) soldqty,
            sum(billitems.receivable) soldvalue from bill
            inner join billitems on billitems.billid = bill.id and billitems.isactive=true
            inner join stock on stock.id = stockid
            left outer join products on products.id = stock.productid
            left outer join supplier on supplier.id = stock.supplierid
            left outer join referencelist section on section.id = products.salescounterid		  
            where bill.billdate>=@fromdate and bill.billdate<@todate 
            and bill.billtype != 9 and bill.isactive = true
            and bill.locationid = @locationid
            and stock.productid = @productid
            and stock.supplierid = @supplierid
            group by productname,suppliername,sectoinname;

            select products.name productname,supplier.name suppliername,section.name sectoinname,
            sum(stock.qty-stock.returnqty) as purchaseqty, 
            sum((stock.qty-stock.returnqty)* stock.ibuyingprice::numeric) purchasevalue from lrentry
            inner join lrinvoice on lrinvoice.lrid = lrentry.id
            inner join lrinvoiceitems on lrinvoiceitems.lrinvoiceid = lrinvoice.id
            inner join stock on stock.invoiceitemid = lrinvoiceitems.id 
            left outer join products on products.id = stock.productid
            left outer join supplier on supplier.id = stock.supplierid
            left outer join referencelist section on section.id = products.salescounterid		
            where stock.isactive = true and stock.price>@fromprice and stock.price<@toprice
            and lrentry.lrentrydate>=@fromdate and lrentry.lrentrydate<@todate
            and lrentry.isactive = true 
            and lrentry.retaillocationid = @locationid
            and stock.productid = @productid
            and stock.supplierid = @supplierid
            group by productname,suppliername,sectoinname;";

            if (item.productid == 0)
                pricequery = pricequery.Replace("and stock.productid = @productid", "");
            if (item.supplierid == 0)
                pricequery = pricequery.Replace("and stock.supplierid = @supplierid", "");
            if (item.locationid == 0)
            {
                pricequery = pricequery.Replace("and lrentry.retaillocationid = @locationid", "");
                pricequery = pricequery.Replace("and bill.locationid = @locationid", "");
                pricequery = pricequery.Replace("and salablegoods.locationid = @locationid", "");
            }

            List<DetailShowResponse.DetailItem> items = new List<DetailShowResponse.DetailItem>();
            if (item.mode == DetailShowRequest.Modes.Age)
            {
                var param = new SortedList<string, object>();
                param.Add("@locationid", item.locationid);
                param.Add("@fromprice", item.pricefrom);
                param.Add("@toprice", item.priceto);
                param.Add("@fromdays", item.fromdays);
                param.Add("@todays", item.todays);
                param.Add("@fromdate", item.fromdate);
                param.Add("@current_date", DateTime.Now.Date);
                param.Add("@todate", item.todate);
                using (var reader = ExecuteReader(agequery, param))
                {
                    while (reader.Read())
                    {
                        DetailShowResponse.DetailItem ditem = new DetailShowResponse.DetailItem();
                        ditem.product = reader["productname"] == DBNull.Value ? "" : reader["productname"].ToString();
                        ditem.supplier = reader["suppliername"] == DBNull.Value ? "" : reader["suppliername"].ToString();
                        ditem.section = reader["sectoinname"] == DBNull.Value ? "" : reader["sectoinname"].ToString();
                        ditem.stockqty = reader["stockqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["stockqty"]);
                        ditem.stockvalue = reader["stockvalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["stockvalue"]);
                        items.Add(ditem);
                    }
                    if (reader.NextResult())
                    {
                        while (reader.Read())
                        {
                            DetailShowResponse.DetailItem ditem = new DetailShowResponse.DetailItem();
                            ditem.product = reader["productname"] == DBNull.Value ? "" : reader["productname"].ToString();
                            ditem.supplier = reader["suppliername"] == DBNull.Value ? "" : reader["suppliername"].ToString();
                            ditem.section = reader["sectoinname"] == DBNull.Value ? "" : reader["sectoinname"].ToString();
                            ditem.soldcost = reader["soldcost"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["soldcost"]);
                            ditem.soldqty = reader["soldqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["soldqty"]);
                            ditem.soldvalue = reader["soldvalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["soldvalue"]);
                            var idx = items.FindIndex(x => x.product == ditem.product &&
                                x.supplier == ditem.supplier && x.section == ditem.section);
                            if (idx >= 0)
                            {
                                items[idx].soldcost = ditem.soldcost;
                                items[idx].soldqty = ditem.soldqty;
                                items[idx].soldvalue = ditem.soldvalue;
                            }
                            else
                            {
                                items.Add(ditem);
                            }
                        }
                    }
                }
            }

            if (item.mode == DetailShowRequest.Modes.Price)
            {

                var param = new SortedList<string, object>();
                param.Add("@locationid", item.locationid);
                param.Add("@fromprice", item.pricefrom);
                param.Add("@toprice", item.priceto);
                param.Add("@fromdays", item.fromdays);
                param.Add("@todays", item.todays);
                param.Add("@fromdate", item.fromdate);
                param.Add("@current_date", DateTime.Now.Date);
                param.Add("@todate", item.todate);
                using (var reader = ExecuteReader(pricequery, param))
                {
                    while (reader.Read())
                    {
                        DetailShowResponse.DetailItem ditem = new DetailShowResponse.DetailItem();
                        ditem.product = reader["productname"] == DBNull.Value ? "" : reader["productname"].ToString();
                        ditem.supplier = reader["suppliername"] == DBNull.Value ? "" : reader["suppliername"].ToString();
                        ditem.section = reader["sectoinname"] == DBNull.Value ? "" : reader["sectoinname"].ToString();
                        ditem.stockqty = reader["stockqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["stockqty"]);
                        ditem.stockvalue = reader["stockvalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["stockvalue"]);
                        items.Add(ditem);
                    }
                    if (reader.NextResult())
                    {
                        while (reader.Read())
                        {
                            DetailShowResponse.DetailItem ditem = new DetailShowResponse.DetailItem();
                            ditem.product = reader["productname"] == DBNull.Value ? "" : reader["productname"].ToString();
                            ditem.supplier = reader["suppliername"] == DBNull.Value ? "" : reader["suppliername"].ToString();
                            ditem.section = reader["sectoinname"] == DBNull.Value ? "" : reader["sectoinname"].ToString();
                            ditem.soldcost = reader["soldcost"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["soldcost"]);
                            ditem.soldqty = reader["soldqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["soldqty"]);
                            ditem.soldvalue = reader["soldvalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["soldvalue"]);
                            var idx = items.FindIndex(x => x.product == ditem.product &&
                                x.supplier == ditem.supplier && x.section == ditem.section);
                            if (idx >= 0)
                            {
                                items[idx].soldcost = ditem.soldcost;
                                items[idx].soldqty = ditem.soldqty;
                                items[idx].soldvalue = ditem.soldvalue;
                            }
                            else
                            {
                                items.Add(ditem);
                            }
                        }
                    }
                    if (reader.NextResult())
                    {
                        while (reader.Read())
                        {
                            DetailShowResponse.DetailItem ditem = new DetailShowResponse.DetailItem();
                            ditem.product = reader["productname"] == DBNull.Value ? "" : reader["productname"].ToString();
                            ditem.supplier = reader["suppliername"] == DBNull.Value ? "" : reader["suppliername"].ToString();
                            ditem.section = reader["sectoinname"] == DBNull.Value ? "" : reader["sectoinname"].ToString();
                            ditem.purchasevalue = reader["purchasevalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["purchasevalue"]);
                            ditem.purchaseqty = reader["purchaseqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["purchaseqty"]);
                            var idx = items.FindIndex(x => x.product == ditem.product &&
                                x.supplier == ditem.supplier && x.section == ditem.section);
                            if (idx >= 0)
                            {
                                items[idx].purchasevalue = ditem.purchasevalue;
                                items[idx].purchaseqty = ditem.purchaseqty;
                            }
                            else
                            {
                                items.Add(ditem);
                            }
                        }
                    }
                }
            }

            DetailShowResponse detailShowResponse = new DetailShowResponse();

            detailShowResponse.products = items.GroupBy(x => x.product).ToList().ConvertAll(x =>
            new DetailShowResponse.DisplayItem()
            {
                display = x.Key,
                soldcost = x.Sum(y => y.soldcost),
                soldqty = x.Sum(y => y.soldqty),
                soldvalue = x.Sum(y => y.soldvalue),
                purchaseqty = x.Sum(y => y.purchaseqty),
                purchasevalue = x.Sum(y => y.purchasevalue),
                stockqty = x.Sum(y => y.stockqty),
                stockvalue = x.Sum(y => y.stockvalue),
            });

            detailShowResponse.suppliers = items.GroupBy(x => x.supplier).ToList().ConvertAll(x =>
            new DetailShowResponse.DisplayItem()
            {
                display = x.Key,
                soldcost = x.Sum(y => y.soldcost),
                soldqty = x.Sum(y => y.soldqty),
                soldvalue = x.Sum(y => y.soldvalue),
                purchaseqty = x.Sum(y => y.purchaseqty),
                purchasevalue = x.Sum(y => y.purchasevalue),
                stockqty = x.Sum(y => y.stockqty),
                stockvalue = x.Sum(y => y.stockvalue),
            });

            detailShowResponse.sections = items.GroupBy(x => x.section).ToList().ConvertAll(x =>
            new DetailShowResponse.DisplayItem()
            {
                display = x.Key,
                soldcost = x.Sum(y => y.soldcost),
                soldqty = x.Sum(y => y.soldqty),
                soldvalue = x.Sum(y => y.soldvalue),
                purchaseqty = x.Sum(y => y.purchaseqty),
                purchasevalue = x.Sum(y => y.purchasevalue),
                stockqty = x.Sum(y => y.stockqty),
                stockvalue = x.Sum(y => y.stockvalue),
            });

            return detailShowResponse;
        }

        internal List<DesignShowResponse> DesignView(DetailShowRequest item)
        {
            string agequery = @"
            select  
            stock.supplierid,items.colourid,
            stock.prodcutid,supplier.name suppliername,designid,colour.name colour,size.name size,
            sum(salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty)) as stockqty, 
            sum((salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty))*stock.ibuyingprice::numeric) as stockvalue
            from salablegoods
            inner join stock on stock.id = stockid
            inner join items on items.id = stock.itemid
            left outer join supplier on supplier.id = stock.supplierid
            left outer join referencelist colour on colour.id = items.colourid
            left outer join referencelist size on size.id = items.sizeid
            where salablegoods.hasstock = true and salablegoods.isactive = true 
            and salablegoods.locationid = @locationid
            and stock.productid = @productid
            and EXTRACT(day from @current_date - stock.stockdate)>=@fromdays
            and EXTRACT(day from @current_date - stock.stockdate)<@todays
            group by supplier.name,designid,colour.name,size.name,items.colourid;

            select 
            stock.supplierid,items.colourid,
            stock.prodcutid,supplier.name suppliername,designid,colour.name colour,size.name size,
            sum((billitems.qty) * stock.ibuyingprice::numeric) soldcost,
            sum(billitems.qty) as soldqty,
            sum(billitems.receivable) as soldvalue
            from bill
            inner join billitems on billitems.billid = bill.id and billitems.isactive=true
            inner join stock on stock.id = stockid
            inner join items on items.id = stock.itemid
            left outer join supplier on supplier.id = stock.supplierid
            left outer join referencelist colour on colour.id = items.colourid
            left outer join referencelist size on size.id = items.sizeid
            where 
            bill.billdate>=@fromdate and bill.billdate<@todate and bill.billtype != 9 
            and bill.isactive = true
            and bill.locationid = @locationid 
            and EXTRACT(day from bill.billdate - stock.stockdate)>=@fromdays
            and EXTRACT(day from bill.billdate - stock.stockdate)<@todays
            and stock.productid = @productid
            group by supplier.name,designid,colour.name,size.name,items.colourid";


            List<DesignShowResponse.DisplayItem> items = new List<DesignShowResponse.DisplayItem>();
            if (item.mode == DetailShowRequest.Modes.Age)
            {
                var param = new SortedList<string, object>();
                param.Add("locationid", item.locationid);
                param.Add("supplierid", item.supplierid);
                param.Add("productid", item.productid);
                param.Add("@fromdays", item.fromdays);
                param.Add("@todays", item.todays);
                param.Add("@fromdate", item.fromdate);
                param.Add("@todate", item.todate);
                using (var reader = ExecuteReader(agequery, param))
                {
                    while (reader.Read())
                    {
                        DesignShowResponse.DisplayItem ditem = new DesignShowResponse.DisplayItem();
                        ditem.supplierid = reader["supplierid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["supplierid"]);
                        ditem.productid = reader["productid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productid"]);
                        ditem.colourid = reader["colourid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["colourid"]);
                        ditem.stockqty = reader["stockqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["stockqty"]);
                        ditem.supplier = reader["suppliername"] == DBNull.Value ? "" : reader["suppliername"].ToString();
                        ditem.designid = reader["designid"] == DBNull.Value ? "" : reader["designid"].ToString();
                        ditem.size = reader["size"] == DBNull.Value ? "" : reader["size"].ToString();
                        ditem.colour = reader["colour"] == DBNull.Value ? "" : reader["colour"].ToString();
                        ditem.stockqty = reader["stockqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["stockqty"]);
                        ditem.stockvalue = reader["stockvalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["stockvalue"]);
                        items.Add(ditem);
                    }
                    if (reader.NextResult())
                    {
                        while (reader.Read())
                        {
                            DesignShowResponse.DisplayItem ditem = new DesignShowResponse.DisplayItem();
                            ditem.supplierid = reader["supplierid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["supplierid"]);
                            ditem.productid = reader["productid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productid"]);
                            ditem.colourid = reader["colourid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["colourid"]);
                            ditem.soldcost = reader["soldcost"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["soldcost"]);
                            ditem.soldqty = reader["soldqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["soldqty"]);
                            ditem.soldvalue = reader["soldvalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["soldvalue"]);
                            ditem.designid = reader["designid"] == DBNull.Value ? "" : reader["designid"].ToString();
                            ditem.size = reader["size"] == DBNull.Value ? "" : reader["size"].ToString();
                            ditem.colour = reader["colour"] == DBNull.Value ? "" : reader["colour"].ToString();
                            var idx = items.FindIndex(x => x.supplierid == ditem.supplierid &&
                                x.productid == ditem.productid && x.designid == ditem.designid);
                            if (idx >= 0)
                            {
                                items[idx].soldcost = ditem.soldcost;
                                items[idx].soldqty = ditem.soldqty;
                                items[idx].soldvalue = ditem.soldvalue;
                            }
                            else
                            {
                                items.Add(ditem);
                            }
                        }
                    }
                }
            }
            var lst = items.GroupBy(x => new { x.supplierid, x.designid, x.productid }).Select(y => new { y.Key.designid, y.Key.supplierid, y.Key.productid, items = y.ToList() }).ToList();

            string query = string.Format(@"select productid,supplierid,designid,colours from productdesign
            INNER JOIN LATERAL
            json_array_elements ('{0}'::json) schemelist ON 
            cast(schemelist->>'productid' as bigint) = productdesign.productid AND
            cast(schemelist->>'supplierid' as bigint) = productdesign.supplierid AND
            schemelist->>'designid' = productdesign.designid
            where productdesign.isactive=true", lst.ToJSON());

            using (var reader = ExecuteReader(query, null))
            {
                while (reader.Read())
                {
                    var supplierid = reader["supplierid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["supplierid"]);
                    var productid = reader["productid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productid"]);
                    var designid = reader["designid"] == DBNull.Value ? "" : reader["designid"].ToString();
                    var fitems = items.FindAll(x => x.supplierid == supplierid && x.productid == productid && x.designid == designid);
                    if (fitems.Count > 0)
                    {
                        var colours = Newtonsoft.Json.JsonConvert.DeserializeObject<List<DesignShowResponse.Colours>>(reader["colours"].ToString());
                        if (colours != null && colours.Count > 0)
                        {
                            foreach (var fitem in fitems)
                            {
                                if (colours.Exists(x => x.id == fitem.colourid))
                                {
                                    fitem.image = colours.Find(x => x.id == fitem.colourid).image;
                                }
                            }
                        }
                    }
                }
            }
            return DesignShowResponse.Build(items);
        }

        public class InvoicePendingItem
        {
            public string no { get; set; }
            public DateTime date { get; set; }
            public decimal value { get; set; }
            public decimal soldpercent { get; set; }
            public bool isinvoice { get; set; }
            public long invoiceid { get; set; }
        }
        public class LocationItemSummary
        {
            public string currentmonth { get; set; }
            public string lastyearmonth { get; set; }
            public List<LocationItem> items { get; set; }
        }
        public class LocationItem
        {
            public string name { get; set; }
            public long id { get; set; }
            public decimal stock { get; set; }
            public decimal currentmonth { get; set; }
            public decimal lastyearmonth { get; set; }
            public decimal todaysales { get; set; }
            public decimal todayinward { get; set; }
            public int pendinginward { get; set; }
            public decimal lastyearmonthqty { get; set; }
            public decimal lastyearmonthcount { get; set; }
            public decimal monthbillcount { get; set; }
            public decimal todaybillcount { get; set; }
            public decimal monthsalesqty { get; set; }
            public decimal todaysalesqty { get; set; }

        }

        public class Insight
        {
            public enum InsightType
            {
                Product,
                Supplier
            }
            public int stockholding { get; set; }
            public string display { get; set; }
            public InsightType type { get; set; }

            public List<DataItem> InsightData { get; set; }
            public List<StockItem> StockData { get; set; }
            public List<SalesItem> SalesData { get; set; }
            public List<SalesItem> PurchaseData { get; set; }
            public List<PriceRangeItem> PriceData { get; set; }
            public decimal Opening { get; set; }
            public decimal Closing { get; set; }
            public decimal ClosingAmount { get; set; }
            public decimal ClosingQty { get; set; }
            public decimal SalesCost { get; set; }
            public decimal SalesAmount { get; set; }
            public decimal SalesQty { get; set; }
            public decimal PurchaseCost { get; set; }
            public decimal PurchaseQty { get; set; }
            public decimal OpeningCost { get; set; }
            public decimal OpeningQty { get; set; }
            public decimal StockDataTotalQty { get; set; }
            public decimal StockDataTotalValue { get; set; }
            public decimal StockDataTotalSold { get; set; }

            public class StockItem
            {
                public int Index { get; set; }
                public string Period { get; set; }
                public decimal Qty { get; set; }
                public decimal Value { get; set; }
                public decimal Sold { get; set; }
                public decimal TSold { get; set; }
                public decimal pt { get; set; }
            }
            public class SalesItem
            {
                public int Quarter { get; set; }
                public int Year { get; set; }
                public decimal Qty { get; set; }
                public decimal Cost { get; set; }
                public decimal Amount { get; set; }
                public int Month { get; set; }
                public string display { get; set; }
            }

            public class SaleStockItem
            {
                public string name { get; set; }
                public decimal ostockqty { get; set; }
                public decimal ostockvalue { get; set; }
                public decimal osalesqty { get; set; }
                public decimal osalesvalue { get; set; }

                public decimal nstockqty { get; set; }
                public decimal nstockvalue { get; set; }
                public decimal nsalesqty { get; set; }
                public decimal nsalesvalue { get; set; }

                public decimal openingqty { get; set; }
                public decimal openingvalue { get; set; }
                public decimal closingqty { get; set; }
                public decimal closingvalue { get; set; }
                public decimal clratio { get; set; }
                public decimal soldqty
                {
                    get
                    {
                        return nsalesqty + osalesqty;
                    }
                }
                public decimal soldvalue
                {
                    get
                    {
                        return nsalesvalue + osalesvalue;
                    }
                }
                public decimal purchaseqty
                {
                    get
                    {
                        return nstockqty + nsalesqty;
                    }
                }
                public decimal purchasevalue
                {
                    get
                    {
                        return nstockvalue + nsalesvalue;
                    }
                }

                public decimal cogs
                {
                    get
                    {
                        return openingvalue + purchasevalue - closingvalue;
                    }
                }

                public decimal avginventory
                {
                    get
                    {
                        return Math.Round((openingvalue + closingvalue) / 2, 2);
                    }
                }

                public decimal sellthrogh
                {
                    get
                    {
                        return purchasevalue == 0 ? 0 : Math.Round((soldvalue / purchasevalue) * 100, 2);
                    }
                }
                public decimal stockturn
                {
                    get
                    {
                        return avginventory == 0 ? 0 : Math.Round(cogs / avginventory, 2);
                    }
                }
            }
            public class PriceRangeItem
            {
                public string price { get; set; }
                public decimal stockvalue { get; set; }
                public decimal stockqty { get; set; }
                public decimal soldcost { get; set; }
                public decimal saleqty { get; set; }
                public decimal salesvalue { get; set; }
                public int sortindex { get; set; }
                public decimal rt { get; set; }
                public decimal pt { get; set; }
                public decimal purchaseqty { get; set; }
                public decimal purchasevalue { get; set; }
            }
            public class DataItem
            {
                public int Quarter { get; set; }
                public int Year { get; set; }
                public decimal Sales { get; set; }
                public decimal Purchase { get; set; }
                public decimal SalesQty { get; set; }
                public decimal Profit { get; set; }
                public decimal PurchaseQty { get; set; }
                public string Display { get; set; }
            }
        }

        public class SaleView_Request : Common.BaseEntity
        {
            public long locationid { get; set; }
            public long sectionid { get; set; }
            public long divisionid { get; set; }
            public long productid { get; set; }
            public long salesmenid { get; set; }
            public string mode { get; set; }
            public DateTime date { get; set; }
            public DateTime fromdate { get; set; }
        }
        public class SaleView_Response : Common.BaseEntity
        {
            public long id { get; set; }
            public string display { get; set; }
            public decimal salqty { get; set; }
            public decimal salval { get; set; }
            public decimal stkval { get; set; }
            public decimal stkqty { get; set; }
        }
        public class StoreViewEOD_Request : Common.BaseEntity
        {
            public long companyid { get; set; }
            public long locationid { get; set; }
            public DateTime date { get; set; }
            public int plus { get; set; }
            public int minus { get; set; }
        }
        public class StoreViewEOD_SaleItem
        {
            public string display { get; set; }
            public decimal value { get; set; }
        }
        public class StoreViewEOD_SectionSaleItem
        {
            [JsonIgnore]
            public long groupid { get; set; }
            [JsonIgnore]
            public string group { get; set; }
            public string display { get; set; }
            public decimal soldvalue { get; set; }
            public decimal exchangevalue { get; set; }
            public decimal returnvalue { get; set; }
            public long id { get; set; }
        }
        public class StoreViewEOD_SaleProductItem : StoreViewEOD_SectionSaleItem
        {
            public decimal cost { get; set; }
            public decimal margin { get; set; }
        }

        public class StoreViewEOD_Settlement
        {
            public string display { get; set; }
            public decimal value { get; set; }

            public decimal count { get; set; }
        }


        public class StoreViewEOD_AgeStock
        {
            public string age { get; set; }
            public decimal stockqty { get; set; }
            public decimal stockvalue { get; set; }
            public decimal soldqty { get; set; }
            public decimal soldvalue { get; set; }
            public int index { get; set; }
        }

        public class IntrayItem
        {
            public string status { get; set; }
            public string typename { get; set; }
            public string description { get; set; }
            public DateTime date { get; set; }
            public bool can { get; set; }
            public string durl { get; set; }
            public long id { get; set; }
        }
        public class StoreViewEOD
        {
            public StoreViewEOD()
            {
                Vochers = new List<StoreViewEOD_CashItem>();
                PettyCashExpense = new List<StoreViewEOD_CashItem>();
                AgeStock = new List<StoreViewEOD_AgeStock>();
                SettlementItem = new List<StoreViewEOD_Settlement>();
                SaleItem = new List<StoreViewEOD_SaleItem>();
                SectionSale = new List<StoreViewEOD_SectionSaleItem>();
                SalesmenSale = new List<StoreViewEOD_SectionSaleItem>();
                ProductGroupSale = new List<StoreViewEOD_SaleProductItem>();
                CashBook = new List<StoreViewEOD_CashItem>();
            }
            public List<StoreViewEOD_SaleProductItem> ProductSale { get; set; }
            public List<StoreViewEOD_SaleProductItem> ProductGroupSale { get; set; }
            public List<StoreViewEOD_CashItem> CashBook { get; set; }
            public List<StoreViewEOD_CashItem> PettyCashExpense { get; set; }
            public List<StoreViewEOD_CashItem> Vochers { get; set; }
            public List<StoreViewEOD_AgeStock> AgeStock { get; set; }
            public List<StoreViewEOD_SaleItem> SaleItem { get; set; }
            public List<StoreViewEOD_Settlement> SettlementItem { get; set; }
            public List<StoreViewEOD_SectionSaleItem> SectionSale { get; set; }
            public List<StoreViewEOD_SectionSaleItem> BarcodeSale { get; set; }
            public List<StoreViewEOD_SectionSaleItem> SalesmenSale { get; set; }
            public DateTime Date { get; set; }
            public List<CreditHistory> CreditHistory { get; set; }
        }
        public class StoreViewEOD_CashItem
        {
            public string parentdisplay { get; set; }
            public string display { get; set; }
            public decimal childvalue { get; set; }
            public decimal value { get; set; }
            public string symbol { get; set; }
        }

        public class DetailShowRequest : BaseEntity
        {
            public string days { get; set; }
            public string price { get; set; }
            public int fromdays { get; set; }
            public int todays { get; set; }
            public DateTime fromdate { get; set; }
            public DateTime todate { get; set; }
            public int pricefrom { get; set; }
            public int priceto { get; set; }
            public long productid { get; set; }
            public long supplierid { get; set; }
            public long locationid { get; set; }
            public string design { get; set; }
            public Modes mode { get; set; }
            public string resultmode { get; set; }
            public enum Modes
            {
                Age,
                Price,
                Design
            }
        }

        public class DesignShowResponse
        {

            public static List<DesignShowResponse> Build(List<DisplayItem> items)
            {
                var response = new List<DesignShowResponse>();
                var lst = items.GroupBy(x => new { x.supplierid, x.designid, x.productid }).Select(y => new { y.Key.designid, y.Key.supplierid, y.Key.productid, items = y.ToList() }).ToList();
                foreach (var itm in lst)
                {
                    DesignShowResponse designShowResponse = new DesignShowResponse();
                    designShowResponse.stock = new List<ColourStock>();
                    designShowResponse.productid = itm.productid;
                    designShowResponse.supplierid = itm.supplierid;
                    designShowResponse.designid = itm.designid;
                    designShowResponse.sizelist = itm.items.ConvertAll(x => x.size);
                    foreach (var citm in itm.items.GroupBy(x => x.colour))
                    {
                        ColourStock colour = new ColourStock(); ;
                        colour.colour = citm.Key;
                        colour.imageid = citm.First().image;
                        colour.stock = citm.ToList().ConvertAll(x => new ColourStock.SizeStock()
                        {
                            size = x.size,
                            soldqty = x.soldqty,
                            soldvalue = x.soldvalue,
                            soldcost = x.soldcost,
                            stockqty = x.stockqty,
                            stockvalue = x.stockvalue,
                            purchaseqty = x.purchaseqty,
                            purchasevalue = x.purchasevalue
                        });
                    }
                }
                return response;
            }

            public class Colours
            {
                public long id { get; set; }
                public string name { get; set; }
                public string image { get; set; }
            }

            public class DisplayItem
            {
                public long colourid { get; set; }
                public long productid { get; set; }
                public long supplierid { get; set; }
                public string supplier { get; set; }
                public string designid { get; set; }
                public string size { get; set; }
                public string colour { get; set; }
                public decimal stockqty { get; set; }
                public decimal stockvalue { get; set; }
                public decimal soldqty { get; set; }
                public decimal soldvalue { get; set; }
                public decimal soldcost { get; set; }
                public decimal purchaseqty { get; set; }
                public decimal purchasevalue { get; set; }
                public string image { get; internal set; }
            }


            public List<ColourStock> stock { get; set; }
            public List<string> sizelist { get; set; }
            public long productid { get; set; }
            public long supplierid { get; set; }
            public string supplier { get; set; }
            public string designid { get; set; }

            public class ColourStock
            {
                public string colour { get; set; }
                public string imageid { get; set; }
                public List<SizeStock> stock { get; set; }
                public class SizeStock
                {
                    public string size { get; set; }
                    public decimal stockqty { get; set; }
                    public decimal stockvalue { get; set; }
                    public decimal soldqty { get; set; }
                    public decimal soldvalue { get; set; }
                    public decimal soldcost { get; set; }
                    public decimal purchaseqty { get; set; }
                    public decimal purchasevalue { get; set; }
                }
            }

        }

    }
}