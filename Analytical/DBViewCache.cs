using Amazon.Runtime.Internal.Transform;
using InfyPOS.Common;
using InfyPOS.Data;
using InfyPOS.Models;
using Microsoft.SqlServer.Server;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Web;

namespace InfyPOS.Processors
{
    public class DBViewCache : BaseData
    {

        string defaultRow = @"insert into Cache_View(viewname,lastfetch,lastfetchcompleted,lastDateValue,version)
        values('mv_salesdailybyproduct',current_date-INTERVAL '10 day',current_date-INTERVAL '10 day',current_date-INTERVAL '10 day',0);
        insert into Cache_View(viewname,lastfetch,lastfetchcompleted,lastDateValue,version)
        values('mv_salesmonthly',current_date-INTERVAL '10 day',current_date-INTERVAL '10 day',current_date-INTERVAL '10 day',0);
        insert into Cache_View(viewname,lastfetch,lastfetchcompleted,lastDateValue,version)
        values('mv_stockmonthly',current_date-INTERVAL '10 day',current_date-INTERVAL '10 day',current_date-INTERVAL '10 day',0);";


        public SortedDictionary<string,DateTime> CacheHistory()
        {
            string createcache = @"CREATE TABLE IF NOT EXISTS public.Cache_View
            (
                viewname varchar(200),
                lastfetch timestamp without time zone,
                lastfetchcompleted timestamp without time zone,
                lastDateValue timestamp without time zone,
                version integer
            )
            --create unique index idx_cacheview on Cache_View(viewname);";

            ExecuteQuery(createcache, null);

            SortedDictionary<string, DateTime> keyValuePairs = new SortedDictionary<string, DateTime>();
            using (var reader = ExecuteReader("select viewname,lastDateValue from Cache_View", null))
            {
                while(reader.Read())
                {
                    keyValuePairs.Add(reader["viewname"].ToString(), Convert.ToDateTime(reader["lastDateValue"]));
                }
            }
            return keyValuePairs;
        }
        public void ValidateView(string[] key)
        {
            var lst = CacheHistory();
            foreach (var itm in key)
            {
                if(!lst.ContainsKey(itm))
                {
                    string defaultCache = String.Format(@"insert into Cache_View(viewname,lastfetch,lastfetchcompleted,lastDateValue,version)
                values('{0}',current_date-INTERVAL '10 day',current_date-INTERVAL '10 day',current_date-INTERVAL '10 day',0);", itm);
                    ExecuteQuery(defaultCache, null);
                    lst.Add(itm, DateTimeNow);
                }
            }
        }

        public static string cacheview = "mv_stockmonthly,mv_stock_prd_sup,mv_sales_prd_sup,mv_salesmonthly,mv_fin_bill,mv_fin_billitems_tax,mv_fin_billsettlement_voucher,mv_salesproductdaily,mv_salesdailybyproduct,mv_purchaseproductdaily,mv_montlysalesbyproduct,mv_pricerangequartaly,mw_dailsalesdetails,mw_dailstockdetails,mv_fin_b2bsettlement,mv_fin_unsettled,mv_fin_additionalcharges";
        private static AutoResetEvent ars = new AutoResetEvent(true);
        public void RunAll(Migration.ProgressStatus progress)
        {
            if (ars.WaitOne(10000))
            {
                if (progress == null) progress = new Migration.ProgressStatus();
                progress.hasstarted = true;
                try
                {
                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Start();
                    progress.status = "Validating Cache";
                    
                    ValidateView(cacheview.Split(','));
                    double running = 0;
                    double total = 19;
                    Run_Prewarm();
                    progress.status = "Building Dashboard (" + (++running) + "/" + total + ")";
                    Run_Dashboard();
                    progress.percentage = Convert.ToInt32((running / total) * 100);
                    progress.status = "Running Daily Stock (" + (++running) + "/" + total + ")";
                    Run_mw_dailstockdetails();
                    progress.percentage = Convert.ToInt32((running / total) * 100);
                    progress.status = "Running Stock Monthly (" + (++running) + "/" + total + ")";
                    Run_mv_stockmonthly();
                    progress.percentage = Convert.ToInt32((running / total) * 100);
                    progress.status = "Running Sales Monthly (" + (++running) + "/" + total + ")";
                    Run_mv_salesmonthly();
                    progress.percentage = Convert.ToInt32((running / total) * 100);
                    progress.status = "Running Stock Insights (" + (++running) + "/" + total + ")";
                    Run_mv_stockinsights();
                    progress.percentage = Convert.ToInt32((running / total) * 100);
                    progress.status = "Running Sales Insights (" + (++running) + "/" + total + ")";
                    Run_mv_salesinsights();
                    progress.percentage = Convert.ToInt32((running / total) * 100);
                    progress.status = "Running Finance Bill (" + (++running) + "/" + total + ")";
                    Run_mv_finbill();
                    progress.percentage = Convert.ToInt32((running / total) * 100);
                    progress.status = "Running Bill and Tax (" + (++running) + "/" + total + ")";
                    Run_mv_finbillitemtax();
                    progress.percentage = Convert.ToInt32((running / total) * 100);
                    progress.status = "Running Settlement (" + (++running) + "/" + total + ")";
                    Run_mv_finbillsettlementvoucher();
                    progress.percentage = Convert.ToInt32((running / total) * 100);
                    progress.status = "Running Sales Daily (" + (++running) + "/" + total + ")";
                    Run_mv_salesdailybyproduct();
                    progress.percentage = Convert.ToInt32((running / total) * 100);
                    progress.status = "Running Purchase Daily (" + (++running) + "/" + total + ")";
                    Run_mv_purchaseproductdaily();
                    progress.percentage = Convert.ToInt32((running / total) * 100);
                    progress.status = "Running Sales Productwise (" + (++running) + "/" + total + ")";
                    Run_mv_salesproductdaily();
                    progress.percentage = Convert.ToInt32((running / total) * 100);
                    progress.status = "Running Purchase Monthly (" + (++running) + "/" + total + ")";
                    Run_mv_purchasemonthlybyproduct();
                    progress.percentage = Convert.ToInt32((running / total) * 100);
                    progress.status = "Running Sales Monthly (" + (++running) + "/" + total + ")";
                    Run_mv_salesmonthlybyproduct();
                    progress.percentage = Convert.ToInt32((running / total) * 100);
                    progress.status = "Running Price Range Quartaly (" + (++running) + "/" + total + ")";
                    Run_mv_pricerangequartaly();
                    progress.percentage = Convert.ToInt32((running / total) * 100);
                    progress.status = "Running Sales Daily (" + (++running) + "/" + total + ")";
                    Run_mw_dailsalesdetails();
                    progress.percentage = Convert.ToInt32((running / total) * 100);
                    progress.status = "Running Stock Daily (" + (++running) + "/" + total + ")";
                    Run_mw_dailstockdetails();

                    progress.percentage = Convert.ToInt32((running / total) * 100);
                    progress.status = "Running B2B settlement (" + (++running) + "/" + total + ")";
                    Run_mv_fin_b2bsettlement();

                    progress.percentage = Convert.ToInt32((running / total) * 100);
                    progress.status = "Running Unsettled (" + (++running) + "/" + total + ")";
                    Run_mv_fin_unsettled();

                    progress.percentage = Convert.ToInt32((running / total) * 100);
                    progress.status = "Running Additionalcharges (" + (++running) + "/" + total + ")";
                    Run_mv_fin_additionalcharges();

                    progress.status = "Completed";
                    progress.percentage = 100;
                    progress.hascompleted = true;
                    stopwatch.Stop();
                }
                catch (Exception exp)
                {
                    progress.hascompleted = true;
                    progress.status = exp.Message;
                }finally
                {
                    ars.Set();
                }
            }
            else
            {
                progress.status = "Skipped";
                progress.percentage = 100;
                progress.hascompleted = true;
            }
        }

        public bool Run_Prewarm()
        {

            string preward = @"SELECT pg_prewarm('public.referencelist', 'buffer');
                    SELECT pg_prewarm('public.items', 'buffer');
                    SELECT pg_prewarm('public.company', 'buffer');
                    SELECT pg_prewarm('public.brand', 'buffer');
                    SELECT pg_prewarm('public.supplier', 'buffer');
                    SELECT pg_prewarm('public.suppliercompany', 'buffer');
                    SELECT pg_prewarm('public.users', 'buffer');
                    SELECT pg_prewarm('public.usersession', 'buffer');";
            ExecuteQuery(preward, null);
            return true;
        }
        public bool Run_Dashboard()
        {
            StoretransactionDataService storetransactionDataService = new StoretransactionDataService();
            var cached = storetransactionDataService.Search(new StoretransactionCreteria()
            {
                type = "StkDBoard",
                actiondate = DateTimeNow.Date
            });
            if (cached.Count > 0)
                return true;
            RetailReportDataService retailReportDataService = new RetailReportDataService();
            var stocklist = retailReportDataService.SearchStockValue(new RetailReportCreteria());
            var currentstock = stocklist.Sum(x => x.stockvalue);
            retailReportDataService.BuildSalesVsStockDashBoard(currentstock, DateTimeNow.AddDays(-15).Date);
            return true;
        }
        public bool Run_mv_salesproductdaily()
        {
            var runningdate = DateTime.Now.Date;
            using (var connection = CreateConnection())
            {
                connection.Open();
                var param = new SortedList<string, object>();
                param.Add("lastDateValue", runningdate.AddDays(-1));
                param.Add("currentdate", runningdate);
                var trans = connection.BeginTransaction();
                var rows = ExecuteQuery("Update Cache_View set lastfetch=@currentdate,lastDateValue=@lastDateValue where viewname='mv_salesproductdaily' and lastfetch<@currentdate",
                    param, connection, trans, 1000);
                if (rows > 0)
                {
                    string viewExist = @"select matviewname from pg_matviews where matviewname='mv_salesproductdaily'";
                    var matviewexist = false;
                    using (var reader = ExecuteReader(viewExist, null))
                    {
                        matviewexist = (reader.Read());
                    }

                    string Query_VW_StockMonthly = @"CREATE OR REPLACE VIEW VW_SalesProductDaily AS 
                        select sum(billitems.receivable) salevalue,billitems.productid,bill.locationid,billdate::date from bill
                        inner join billitems on billitems.billid = bill.id and billitems.isactive=true
                        where bill.billdate<'{0}'
                        and billtype != 9 and bill.isactive=true
                        group by billitems.productid,bill.locationid,billdate::date;";

                    DeleteOrCreateView(string.Format(string.Format(Query_VW_StockMonthly, DateTime.Now.Date.ToString("yyyy-MM-dd")), DateTime.Now.Date.ToString("yyyy-MM-dd")), "VW_SalesProductDaily");
                    if (!matviewexist)
                    {
                        string Create_MV_SalesDailyByProduct = @"CREATE MATERIALIZED VIEW if not exists mv_salesproductdaily AS 
                                                        SELECT * FROM VW_SalesProductDaily;";
                        ExecuteQuery(Create_MV_SalesDailyByProduct, null, 100000);
                    }
                    else
                    {
                        ExecuteQuery(@"REFRESH MATERIALIZED VIEW mv_salesproductdaily", null, 100000);
                    }
                    trans.Commit();
                    connection.Close();
                    return true;
                }
                else
                {
                    trans.Rollback();
                    connection.Close();
                    return false;
                }
            }
        }

        public bool Run_mv_purchaseproductdaily()
        {
            var runningdate = DateTime.Now.Date;
            using (var connection = CreateConnection())
            {
                connection.Open();
                var param = new SortedList<string, object>();
                param.Add("lastDateValue", runningdate.AddDays(-1));
                param.Add("currentdate", runningdate);
                var trans = connection.BeginTransaction();
                var rows = ExecuteQuery("Update Cache_View set lastfetch=@currentdate,lastDateValue=@lastDateValue where viewname='mv_purchaseproductdaily' and lastfetch<@currentdate",
                    param, connection, trans, 1000);
                if (rows > 0)
                {
                    string viewExist = @"select matviewname from pg_matviews where matviewname='mv_purchaseproductdaily'";
                    var matviewexist = false;
                    using (var reader = ExecuteReader(viewExist, null))
                    {
                        matviewexist = (reader.Read());
                    }

                    string Query_VW_StockMonthly = @"CREATE OR REPLACE VIEW VW_PurchaseProductDaily AS 
                        select 
						sum(lrinvoiceitems.amount) amount,
						sum(lrinvoiceitems.buyingqty) qty,
						(div(lrinvoiceitems.sellingprice, (100)::numeric) * (100)::numeric) AS pricesegment,
                        avg(lrinvoiceitems.sellingprice) price,
                        avg(lrinvoiceitems.buyingprice) cost,
                        lrentry.lrentrydate,
                        lrinvoiceitems.productid,
						lrentry.supplierid
                        from lrinvoice  
                        inner join lrinvoiceitems on lrinvoiceitems.lrinvoiceid = lrinvoice.id and lrinvoiceitems.isactive=true
                        left outer join lrentry on lrentry.id = lrinvoice.lrid
                        where lrentry.lrentrydate<'2025-02-06' and lrinvoice.isactive=true
                        group by lrinvoiceitems.productid,lrentry.lrentrydate,
						lrentry.supplierid,pricesegment;";

                    DeleteOrCreateView(string.Format(string.Format(Query_VW_StockMonthly, DateTime.Now.Date.ToString("yyyy-MM-dd")), DateTime.Now.Date.ToString("yyyy-MM-dd")), "VW_PurchaseProductDaily");
                    if (!matviewexist)
                    {
                        string Create_MV_SalesDailyByProduct = @"CREATE MATERIALIZED VIEW if not exists mv_purchaseproductdaily AS 
                                                        SELECT * FROM VW_PurchaseProductDaily;";
                        ExecuteQuery(Create_MV_SalesDailyByProduct, null, 100000);
                    }
                    else
                    {
                        ExecuteQuery(@"REFRESH MATERIALIZED VIEW mv_purchaseproductdaily", null, 100000);
                    }
                    trans.Commit();
                    connection.Close();
                    return true;
                }
                else
                {
                    trans.Rollback();
                    connection.Close();
                    return false;
                }
            }
        }

        public bool Run_mv_stockmonthly()
        {
            var runningdate = DateTime.Now.Date;
            using (var connection = CreateConnection())
            {
                connection.Open();
                var param = new SortedList<string, object>();
                param.Add("lastDateValue", runningdate.AddDays(-1));
                param.Add("currentdate", runningdate);
                var trans = connection.BeginTransaction();
                var rows = ExecuteQuery("Update Cache_View set lastfetch=@currentdate,lastDateValue=@lastDateValue where viewname='mv_stockmonthly' and lastfetch<@currentdate",
                    param, connection, trans, 1000);
                if (rows > 0)
                {
                    string viewExist = @"select matviewname from pg_matviews where matviewname='mv_stockmonthly'";
                    var matviewexist = false;
                    using (var reader = ExecuteReader(viewExist, null))
                    {
                        matviewexist = (reader.Read());
                    }
                    
                    string Query_VW_StockMonthly = @"CREATE OR REPLACE VIEW VW_StockMonthly AS 
                        SELECT salablegoods.locationid,stock.productid,stock.supplierid,
						sum(salablegoods.iqty - salablegoods.dqty - salablegoods.jqty) qty,
                        sum((salablegoods.iqty - salablegoods.dqty - salablegoods.jqty) * (stock.ibuyingprice)::numeric) AS cost,
                        min(stock.stockdate) AS stockdate,
                        EXTRACT(month FROM stock.stockdate) AS month,
                        EXTRACT(year FROM stock.stockdate) AS year
                        FROM stock
                        INNER JOIN salablegoods ON salablegoods.stockid = stock.id
                        where stock.stockdate<'{0}' and stock.isactive=true
                        GROUP BY salablegoods.locationid,stock.productid,stock.supplierid,
                        EXTRACT(month FROM stock.stockdate), EXTRACT(year FROM stock.stockdate);";

                    DeleteOrCreateView(string.Format(string.Format(Query_VW_StockMonthly, DateTime.Now.Date.ToString("yyyy-MM-dd")), DateTime.Now.Date.ToString("yyyy-MM-dd")), "VW_StockMonthly");
                    if (!matviewexist)
                    {
                        string Create_MV_SalesDailyByProduct = @"CREATE MATERIALIZED VIEW if not exists mv_stockmonthly AS 
                                                        SELECT * FROM VW_StockMonthly;";
                        ExecuteQuery(Create_MV_SalesDailyByProduct, null, 100000);

                        ExecuteQuery("CREATE UNIQUE INDEX IDX_MV_StockMonthly ON mv_stockmonthly(month,year,productid,supplierid,locationid)", null, 100000);
                    }
                    else
                    {
                        ExecuteQuery(@"REFRESH MATERIALIZED VIEW  mv_stockmonthly", null, 100000);
                    }
                    trans.Commit();
                    connection.Close();
                    return true;
                }
                else
                {
                    trans.Rollback();
                    connection.Close();
                    return false;
                }
            }
        }

        public bool Run_mw_dailstockdetails()
        {
            var runningdate = DateTime.Now.Date;
            using (var connection = CreateConnection())
            {
                connection.Open();
                var param = new SortedList<string, object>();
                param.Add("lastDateValue", runningdate.AddDays(-1));
                param.Add("currentdate", runningdate);
                var trans = connection.BeginTransaction();
                var rows = ExecuteQuery("Update Cache_View set lastfetch=@currentdate,lastDateValue=@lastDateValue where viewname='mw_dailstockdetails' and lastfetch<@currentdate",
                    param, connection, trans, 1000);
                if (rows > 0)
                {
                    string viewExist = @"select matviewname from pg_matviews where matviewname='mw_dailstockdetails'";
                    var matviewexist = false;
                    using (var reader = ExecuteReader(viewExist, null))
                    {
                        matviewexist = (reader.Read());
                    }
                    string Query_VW_SalesMonthly = @"
                                                    CREATE OR REPLACE VIEW VW_dailstockdetails AS 
                                                    select  (div(salablegoods.price,(100)::numeric) * (100)::numeric) pricerange,
                                                    avg(salablegoods.price) productprice,salablegoods.locationid,
                                                    sum((salablegoods.iqty - ((salablegoods.dqty) + salablegoods.jqty))) purchaseqty,
                                                    sum((salablegoods.iqty - ((salablegoods.oqty + salablegoods.dqty) + salablegoods.jqty))) stockqty,
													sum((salablegoods.iqty - ((salablegoods.oqty + salablegoods.dqty) + salablegoods.jqty))*stock.buyingprice) stockvalue,
                                                    stock.productid,stock.supplierid,stock.stockdate::date
                                                    FROM salablegoods
                                                    INNER JOIN stock ON stock.id = salablegoods.stockid
                                                    where stock.stockdate<current_date and salablegoods.hasstock=true
                                                    and stock.isactive=true and salablegoods.isactive=true
                                                    group by stock.productid,salablegoods.locationid,
                                                    stockdate::date,pricerange,stock.supplierid;";

                    DeleteOrCreateView(string.Format(string.Format(Query_VW_SalesMonthly, DateTime.Now.Date.ToString("yyyy-MM-dd")), DateTime.Now.Date.ToString("yyyy-MM-dd")), "VW_dailstockdetails");
                    if (!matviewexist)
                    {
                        string Create_MV_SalesDailyByProduct = @"CREATE MATERIALIZED VIEW if not exists mw_dailstockdetails AS 
                                                        SELECT * FROM VW_dailstockdetails;";
                        ExecuteQuery(Create_MV_SalesDailyByProduct, null, 100000);
                    }
                    else
                    {
                        ExecuteQuery(@"REFRESH MATERIALIZED VIEW mw_dailstockdetails", null, 100000);
                    }
                    trans.Commit();
                    connection.Close();
                    return true;
                }
                else
                {
                    trans.Rollback();
                    connection.Close();
                    return false;
                }
            }
        }

        public bool Run_mw_dailsalesdetails()
        {
            var runningdate = DateTime.Now.Date;
            using (var connection = CreateConnection())
            {
                connection.Open();
                var param = new SortedList<string, object>();
                param.Add("lastDateValue", runningdate.AddDays(-1));
                param.Add("currentdate", runningdate);
                var trans = connection.BeginTransaction();
                var rows = ExecuteQuery("Update Cache_View set lastfetch=@currentdate,lastDateValue=@lastDateValue where viewname='mw_dailsalesdetails' and lastfetch<@currentdate",
                    param, connection, trans, 1000);
                if (rows > 0)
                {
                    string viewExist = @"select matviewname from pg_matviews where matviewname='mw_dailsalesdetails'";
                    var matviewexist = false;
                    using (var reader = ExecuteReader(viewExist, null))
                    {
                        matviewexist = (reader.Read());
                    }
                    string Query_VW_SalesMonthly = @"CREATE OR REPLACE VIEW VW_dailsalesdetails AS 
		            select bill.billdate::date bldate,bill.locationid,
		            (div(billitems.rate+billitems.itemtax,(100)::numeric) * (100)::numeric) pricerange,
		            avg(billitems.rate+billitems.itemtax) productprice,
					sum(billitems.qty*stock.buyingprice) costprice,
		            sum(billitems.qty) qty,billitems.productid,stock.supplierid,
					stock.stockdate from bill
		            inner join billitems on billitems.billid = bill.id and billitems.isactive=true
		            inner join stock on stock.id = billitems.stockid
		            where bill.billdate<'{0}'
		            and bill.billtype != 9 and bill.isactive = true
		            group by billitems.productid,bldate,pricerange,
					stock.supplierid,stock.stockdate,bill.locationid;";

                    DeleteOrCreateView(string.Format(string.Format(Query_VW_SalesMonthly, DateTime.Now.Date.ToString("yyyy-MM-dd")), DateTime.Now.Date.ToString("yyyy-MM-dd")), "VW_dailsalesdetails");
                    if (!matviewexist)
                    {
                        string Create_MV_SalesDailyByProduct = @"CREATE MATERIALIZED VIEW if not exists mw_dailsalesdetails AS 
                                                        SELECT * FROM VW_dailsalesdetails;
                                                        Create index idx_mwdailysalesdetail on mw_dailsalesdetails(bldate);";
                        ExecuteQuery(Create_MV_SalesDailyByProduct, null, 100000);
                    }
                    else
                    {
                        ExecuteQuery(@"REFRESH MATERIALIZED VIEW mw_dailsalesdetails", null, 100000);
                    }
                    trans.Commit();
                    connection.Close();
                    return true;
                }
                else
                {
                    trans.Rollback();
                    connection.Close();
                    return false;
                }
            }
        }

        public bool Run_mv_pricerangequartaly()
        {
            var runningdate = DateTime.Now.Date;
            using (var connection = CreateConnection())
            {
                connection.Open();
                var param = new SortedList<string, object>();
                param.Add("lastDateValue", runningdate.AddDays(-1));
                param.Add("currentdate", runningdate);
                var trans = connection.BeginTransaction();
                var rows = ExecuteQuery("Update Cache_View set lastfetch=@currentdate,lastDateValue=@lastDateValue where viewname='mv_salesmonthly' and lastfetch<@currentdate",
                    param, connection, trans, 1000);
                if (rows > 0)
                {
                    string viewExist = @"select matviewname from pg_matviews where matviewname='mv_pricerangequarterly'";
                    var matviewexist = false;
                    using (var reader = ExecuteReader(viewExist, null))
                    {
                        matviewexist = (reader.Read());
                    }
                    string Query_VW_SalesMonthly = @"CREATE OR REPLACE VIEW VW_Pricerangequarterly AS 
                                                    select bill.billdate::date bldate,
                                                    (div(billitems.rate+billitems.itemtax,(100)::numeric) * (100)::numeric) pricerange,
                                                    sum(billitems.qty) qty,billitems.productid from bill
                                                    inner join billitems on billitems.billid = bill.id and billitems.isactive=true
                                                    where bill.billdate>=current_date - INTERVAL '4 months'
                                                    and bill.billtype != 9 and bill.isactive = true
                                                    group by billitems.productid,bldate,pricerange;";

                    DeleteOrCreateView(string.Format(string.Format(Query_VW_SalesMonthly, DateTime.Now.Date.ToString("yyyy-MM-dd")), DateTime.Now.Date.ToString("yyyy-MM-dd")), "VW_Pricerangequarterly");
                    if (!matviewexist)
                    {
                        string Create_MV_SalesDailyByProduct = @"CREATE MATERIALIZED VIEW if not exists mv_pricerangequarterly AS 
                                                        SELECT * FROM VW_Pricerangequarterly;";
                        ExecuteQuery(Create_MV_SalesDailyByProduct, null, 100000);
                    }
                    else
                    {
                        ExecuteQuery(@"REFRESH MATERIALIZED VIEW mv_pricerangequarterly", null, 100000);
                    }
                    trans.Commit();
                    connection.Close();
                    return true;
                }
                else
                {
                    trans.Rollback();
                    connection.Close();
                    return false;
                }
            }
        }

        public bool Run_mv_salesmonthly()
        {
            var runningdate = DateTime.Now.Date;
            using (var connection = CreateConnection())
            {
                connection.Open();
                var param = new SortedList<string, object>();
                param.Add("lastDateValue", runningdate.AddDays(-1));
                param.Add("currentdate", runningdate);
                var trans = connection.BeginTransaction();
                var rows = ExecuteQuery("Update Cache_View set lastfetch=@currentdate,lastDateValue=@lastDateValue where viewname='mv_salesmonthly' and lastfetch<@currentdate",
                    param, connection, trans, 1000);
                if (rows > 0)
                {
                    string viewExist = @"select matviewname from pg_matviews where matviewname='mv_salesmonthly'";
                    var matviewexist = false;
                    using (var reader = ExecuteReader(viewExist, null))
                    {
                        matviewexist = (reader.Read());
                    }
                    string Query_VW_SalesMonthly = @"CREATE OR REPLACE VIEW VW_SalesMonthly AS 
                            SELECT bill.locationid,sum((bill.receivable + bill.rounding)) AS amount,
                            EXTRACT(month FROM bill.billdate) AS month,
                            EXTRACT(year FROM bill.billdate) AS year,
                            min(bill.billdate) AS billdate
                            FROM bill
                            WHERE (bill.billtype <> 9) AND (bill.isactive = true)  and billdate<'{0}'
                            GROUP BY bill.locationid, (EXTRACT(month FROM bill.billdate)), (EXTRACT(year FROM bill.billdate));";

                    DeleteOrCreateView(string.Format(string.Format(Query_VW_SalesMonthly, DateTime.Now.Date.ToString("yyyy-MM-dd")), DateTime.Now.Date.ToString("yyyy-MM-dd")), "VW_SalesMonthly");
                    if (!matviewexist)
                    {
                        string Create_MV_SalesDailyByProduct = @"CREATE MATERIALIZED VIEW if not exists mv_salesmonthly AS 
                                                        SELECT * FROM VW_SalesMonthly;";
                        ExecuteQuery(Create_MV_SalesDailyByProduct, null, 100000);
                        ExecuteQuery("CREATE UNIQUE INDEX IDX_MV_SalesMonthly ON mv_salesmonthly(month,year,locationid)", null, 100000);
                    }
                    else
                    {
                        ExecuteQuery(@"REFRESH MATERIALIZED VIEW CONCURRENTLY mv_salesmonthly", null, 100000);
                    }
                    trans.Commit();
                    connection.Close();
                    return true;
                }
                else
                {
                    trans.Rollback();
                    connection.Close();
                    return false;
                }
            }
        }
        public bool Run_mv_stockinsights()
        {
            var runningdate = DateTime.Now.Date;
            using (var connection = CreateConnection())
            {
                connection.Open();
                var param = new SortedList<string, object>();
                param.Add("lastDateValue", runningdate.AddDays(-1));
                param.Add("currentdate", runningdate);
                var trans = connection.BeginTransaction();
                var rows = ExecuteQuery("Update Cache_View set lastfetch=@currentdate,lastDateValue=@lastDateValue where viewname='mv_stock_prd_sup' and lastfetch<@currentdate",
                    param, connection, trans, 1000);
                if (rows > 0)
                {
                    string viewExist = @"select matviewname from pg_matviews where matviewname='mv_sales_prd_sup'";
                    var matviewexist = false;
                    using (var reader = ExecuteReader(viewExist, null))
                    {
                        matviewexist = (reader.Read());
                    }

                    String Query_VW_Sales_Prd_Sup = @"CREATE OR REPLACE VIEW VW_stock_prd_sup as 
                                     SELECT
div(EXTRACT(day FROM ((CURRENT_DATE)::timestamp without time zone - stock.stockdate)),30)*30 period,
  (div(salablegoods.price, (100)::numeric) * (100)::numeric) AS pricesegment,
  avg(salablegoods.price) avgprice,
  sum((salablegoods.iqty - ((salablegoods.oqty + salablegoods.dqty) + salablegoods.jqty))) AS qty,
  sum(((salablegoods.iqty - ((salablegoods.oqty + salablegoods.dqty) + salablegoods.jqty)) * (salablegoods.price)::numeric)) AS sp,
  sum(((salablegoods.iqty - ((salablegoods.oqty + salablegoods.dqty) + salablegoods.jqty)) * (stock.ibuyingprice)::numeric)) AS cp,
  stock.productid,
  stock.supplierid,
  stock.designid,
  items.brandid
 FROM ((salablegoods
   JOIN stock ON ((stock.id = salablegoods.stockid)))
   JOIN items ON ((items.id = stock.itemid)))
WHERE ((salablegoods.hasstock = true) AND (salablegoods.isactive = true))
GROUP BY
      period, pricesegment, stock.productid, stock.supplierid, stock.designid, items.brandid";

                    DeleteOrCreateView(string.Format(string.Format(Query_VW_Sales_Prd_Sup, (DateTime.Now.Date.AddDays(15)).ToString("yyyy-MM-dd"), DateTime.Now.Date.ToString("yyyy-MM-dd")), DateTime.Now.Date.ToString("yyyy-MM-dd")), "VW_stock_prd_sup");
                    if (!matviewexist)
                    {
                        string Create_MV_SalesDailyByProduct = @"CREATE MATERIALIZED VIEW if not exists mv_stock_prd_sup AS 
                                                        SELECT * FROM VW_stock_prd_sup;";
                        ExecuteQuery(Create_MV_SalesDailyByProduct, null, 100000);
                    }
                    else
                    {
                        ExecuteQuery(@"REFRESH MATERIALIZED VIEW mv_stock_prd_sup", null, 100000);
                    }
                    trans.Commit();
                    connection.Close();
                    return true;
                }
                else
                {
                    trans.Rollback();
                    connection.Close();
                    return false;
                }
            }
        }
        public bool Run_mv_salesinsights()
        {

            var runningdate = DateTime.Now.Date;
            using (var connection = CreateConnection())
            {
                connection.Open();
                var param = new SortedList<string, object>();
                param.Add("lastDateValue", runningdate.AddDays(-1));
                param.Add("currentdate", runningdate);
                var trans = connection.BeginTransaction();
                var rows = ExecuteQuery("Update Cache_View set lastfetch=@currentdate,lastDateValue=@lastDateValue where viewname='mv_sales_prd_sup' and lastfetch<@currentdate",
                    param, connection, trans, 1000);
                if (rows > 0)
                {
                    string viewExist = @"select matviewname from pg_matviews where matviewname='mv_sales_prd_sup'";
                    var matviewexist = false;
                    using (var reader = ExecuteReader(viewExist, null))
                    {
                        matviewexist = (reader.Read());
                    }

                    String Query_VW_Sales_Prd_Sup = @"CREATE OR REPLACE VIEW VW_Sales_Prd_Sup as 
                                    select case when EXTRACT(day from bill.billdate - stock.stockdate)<30 then 'below 30 days'
                                    when EXTRACT(day from bill.billdate - stock.stockdate)<61 then '30 - 60 days'
                                    when EXTRACT(day from bill.billdate - stock.stockdate)<91 then '60 - 90 days'
                                    when EXTRACT(day from bill.billdate - stock.stockdate)<121 then '90 - 120 days'
                                    when EXTRACT(day from bill.billdate - stock.stockdate)<151 then '120 - 150 days'
                                    when EXTRACT(day from bill.billdate - stock.stockdate)<181 then '150 - 180 days'
                                    else 'above 180 days' end as 
                                    period,div(billitems.price,100)*100 as pricesegment,
                                    sum(billitems.qty) as qty,
                                    stock.productid,stock.supplierid,stock.designid,items.brandid
                                    from bill
                                    inner join billitems on billitems.billid = bill.id and billitems.isactive=true
                                    inner join stock on stock.id = stockid 
                                    inner JOIN items ON items.id = stock.itemid
                                    where bill.billdate<'{1}' and bill.billtype != 9 and bill.isactive = true
                                    group by  period,pricesegment,
                                    stock.productid,stock.supplierid,stock.designid,items.brandid";

                    DeleteOrCreateView(string.Format(string.Format(Query_VW_Sales_Prd_Sup, (DateTime.Now.Date).ToString("yyyy-MM-dd"), DateTime.Now.Date.ToString("yyyy-MM-dd")), DateTime.Now.Date.ToString("yyyy-MM-dd")), "VW_Sales_Prd_Sup");
                    if (!matviewexist)
                    {
                        string Create_MV_SalesDailyByProduct = @"CREATE MATERIALIZED VIEW if not exists mv_sales_prd_sup AS 
                                                        SELECT * FROM VW_Sales_Prd_Sup;";
                        ExecuteQuery(Create_MV_SalesDailyByProduct, null, 100000);
                    }
                    else
                    {
                        ExecuteQuery(@"REFRESH MATERIALIZED VIEW mv_sales_prd_sup", null, 100000);
                    }
                    trans.Commit();
                    connection.Close();
                    return true;
                }
                else
                {
                    trans.Rollback();
                    connection.Close();
                    return false;
                }
            }
        }
        public bool Run_mv_finbill()
        {
            var runningdate = DateTime.Now.Date;
            using (var connection = CreateConnection())
            {
                connection.Open();
                var param = new SortedList<string, object>();
                param.Add("lastDateValue", runningdate.AddDays(-1));
                param.Add("currentdate", runningdate);
                var trans = connection.BeginTransaction();
                var rows = ExecuteQuery("Update Cache_View set lastfetch=@currentdate,lastDateValue=@lastDateValue where viewname='mv_fin_bill' and lastfetch<@currentdate",
                    param, connection, trans, 1000);
                if (rows > 0)
                {
                    string viewExist = @"select matviewname from pg_matviews where matviewname='mv_fin_bill'";
                    var matviewexist = false;
                    using (var reader = ExecuteReader(viewExist, null))
                    {
                        matviewexist = (reader.Read());
                    }
                    string Query_VW_Fin_Bill = @"CREATE OR REPLACE VIEW VW_Fin_Bill AS 
                            Select bill.companyid, bill.locationid,bill.billdate::date,bill.iscreditbill,
                            bill.counterid,bill.customerid,bill.billtype,bill.isb2b,
                            sum(bill.receivable-bill.additionalcharges) amount,
                            sum(bill.receivable) netamount,sum(rounding) rounding
                            from bill 
                            where billtype != 9 and bill.isactive=true
                            and billdate<'{0}'
                            group by bill.locationid,bill.counterid,bill.customerid,bill.billtype,
							bill.companyid,bill.iscreditbill,bill.billdate::date,bill.isb2b;";

                    DeleteOrCreateView(string.Format(string.Format(Query_VW_Fin_Bill, DateTime.Now.Date.ToString("yyyy-MM-dd")), DateTime.Now.Date.ToString("yyyy-MM-dd")), "VW_Fin_Bill");
                    if (!matviewexist)
                    {
                        string Create_MV_SalesDailyByProduct = @"CREATE MATERIALIZED VIEW if not exists mv_fin_bill AS 
                                                                 SELECT * FROM VW_Fin_Bill;";
                        ExecuteQuery(Create_MV_SalesDailyByProduct, null, 100000);
                    }
                    else
                    {
                        ExecuteQuery(@"REFRESH MATERIALIZED VIEW mv_fin_bill", null, 100000);
                    }
                    trans.Commit();
                    connection.Close();
                    return true;
                }
                else
                {
                    trans.Rollback();
                    connection.Close();
                    return false;
                }
            }
        }

        public void DeleteOrCreateView(string query, string viewname)
        {
            try
            {
                ExecuteQuery(query, null);
            }catch(Exception exp)
            {
                ExecuteQuery("Drop view " + viewname,null);
                ExecuteQuery(query, null);
            }
        }
        public bool Run_mv_finbillitemtax()
        {
            var runningdate = DateTime.Now.Date;
            using (var connection = CreateConnection())
            {
                connection.Open();
                var param = new SortedList<string, object>();
                param.Add("lastDateValue", runningdate.AddDays(-1));
                param.Add("currentdate", runningdate);
                var trans = connection.BeginTransaction();
                var rows = ExecuteQuery("Update Cache_View set lastfetch=@currentdate,lastDateValue=@lastDateValue where viewname='mv_fin_billitems_tax' and lastfetch<@currentdate",
                    param, connection, trans, 1000);
                if (rows > 0)
                {
                    string viewExist = @"select matviewname from pg_matviews where matviewname='mv_fin_billitems_tax'";
                    var matviewexist = false;
                    using (var reader = ExecuteReader(viewExist, null))
                    {
                        matviewexist = (reader.Read());
                    }
                    string Query_VW_SalesMonthly = @"CREATE OR REPLACE VIEW VW_Fin_Billitems_Tax AS 
                            Select bill.customerid,bill.iscreditbill,bill.billdate::date,bill.isb2b,
							bill.locationid,billitems.taxpercentage,billitems.taxid,sum(billitems.receivable) amount,
                            sum(billitems.tax) tax,bill.isinterstatesale,bill.billtype,bill.companyid
                            from bill 
                            inner join billitems on billitems.billid = bill.id and billitems.isactive=true
                            left outer join customer on customer.id = bill.customerid
                            where bill.isactive = true 
							and bill.billdate<'{0}'
                            group by bill.customerid,bill.locationid,billitems.taxpercentage,billitems.taxid,bill.isb2b,
							bill.isinterstatesale,bill.billtype,bill.companyid,bill.iscreditbill,bill.billdate::date;";

                    DeleteOrCreateView(string.Format(string.Format(Query_VW_SalesMonthly, DateTime.Now.Date.ToString("yyyy-MM-dd")), DateTime.Now.Date.ToString("yyyy-MM-dd")), "VW_Fin_Billitems_Tax");
                    if (!matviewexist)
                    {
                        string Create_MV_SalesDailyByProduct = @"CREATE MATERIALIZED VIEW if not exists mv_fin_billitems_tax AS 
                                                        SELECT * FROM VW_Fin_Billitems_Tax;";
                        ExecuteQuery(Create_MV_SalesDailyByProduct, null, 100000);
                    }
                    else
                    {
                        ExecuteQuery(@"REFRESH MATERIALIZED VIEW mv_fin_billitems_tax", null, 100000);
                    }
                    trans.Commit();
                    connection.Close();
                    return true;
                }
                else
                {
                    trans.Rollback();
                    connection.Close();
                    return false;
                }
            }
        }

        public bool Run_mv_fin_additionalcharges()
        {
            var runningdate = DateTime.Now.Date;
            using (var connection = CreateConnection())
            {
                connection.Open();
                var param = new SortedList<string, object>();
                param.Add("lastDateValue", runningdate.AddDays(-1));
                param.Add("currentdate", runningdate);
                var trans = connection.BeginTransaction();
                var rows = ExecuteQuery("Update Cache_View set lastfetch=@currentdate,lastDateValue=@lastDateValue where viewname='mv_fin_additionalcharges' and lastfetch<@currentdate",
                    param, connection, trans, 1000);
                if (rows > 0)
                {
                    string viewExist = @"select matviewname from pg_matviews where matviewname='mv_fin_additionalcharges'";
                    var matviewexist = false;
                    using (var reader = ExecuteReader(viewExist, null))
                    {
                        matviewexist = (reader.Read());
                    }
                    string Query_VW_SalesMonthly = @"CREATE OR REPLACE VIEW VW_Fin_additionalcharges as 
                            select bill.companyid,bill.customerid,bill.locationid,bill.counterid,
                            additionalchargestypeid,
							bill.isinterstatesale,
							billadditionalcharges.taxpercentage,
							bill.billdate::date,
							sum(billadditionalcharges.charge) charge,
                            sum(billadditionalcharges.tax) tax
							from bill
                            inner join billadditionalcharges on billadditionalcharges.billid = bill.id
                            inner join referencelist on referencelist.id = additionalchargestypeid
                            left outer join customer on customer.id = customerid
                            where 
							bill.isactive = true and 
							bill.billtype != 9 and 
							bill.isb2b=true and
                            taxonly=false
							and billdate<{0}
							group by bill.companyid,bill.customerid,
							bill.locationid,bill.counterid,
                            additionalchargestypeid,
							bill.isinterstatesale,
							billadditionalcharges.taxpercentage,
							bill.billdate::date";

                    DeleteOrCreateView(string.Format(string.Format(Query_VW_SalesMonthly, DateTime.Now.Date.ToString("yyyy-MM-dd")), DateTime.Now.Date.ToString("yyyy-MM-dd")), "VW_Fin_Billsettlement_Voucher");
                    if (!matviewexist)
                    {
                        string Create_MV_SalesDailyByProduct = @"CREATE MATERIALIZED VIEW if not exists mv_fin_additionalcharges AS 
                                                        SELECT * FROM VW_Fin_additionalcharges;";
                        ExecuteQuery(Create_MV_SalesDailyByProduct, null, 100000);
                    }
                    else
                    {
                        ExecuteQuery(@"REFRESH MATERIALIZED VIEW mv_fin_additionalcharges", null, 100000);
                    }
                    trans.Commit();
                    connection.Close();
                    return true;
                }
                else
                {
                    trans.Rollback();
                    connection.Close();
                    return false;
                }
            }
        }

        public bool Run_mv_fin_unsettled()
        {
            var runningdate = DateTime.Now.Date;
            using (var connection = CreateConnection())
            {
                connection.Open();
                var param = new SortedList<string, object>();
                param.Add("lastDateValue", runningdate.AddDays(-1));
                param.Add("currentdate", runningdate);
                var trans = connection.BeginTransaction();
                var rows = ExecuteQuery("Update Cache_View set lastfetch=@currentdate,lastDateValue=@lastDateValue where viewname='mv_fin_unsettled' and lastfetch<@currentdate",
                    param, connection, trans, 1000);
                if (rows > 0)
                {
                    string viewExist = @"select matviewname from pg_matviews where matviewname='mv_fin_unsettled'";
                    var matviewexist = false;
                    using (var reader = ExecuteReader(viewExist, null))
                    {
                        matviewexist = (reader.Read());
                    }
                    string Query_VW_SalesMonthly = @"CREATE OR REPLACE VIEW VW_Fin_unsettled as 
                        Select bill.billdate::date,
                        bill.companyid,bill.locationid,
                        sum(bill.receivable) amount
                        from bill 
                        where  bill.isactive = true 
                        and bill.isb2b =false and bill.billtype != 9 
                        and iscreditbill=false
                        and settled=false
                        and billdate<{0}
                        group by bill.billdate::date,
                        bill.companyid,bill.locationid;";

                    DeleteOrCreateView(string.Format(string.Format(Query_VW_SalesMonthly, DateTime.Now.Date.ToString("yyyy-MM-dd")), DateTime.Now.Date.ToString("yyyy-MM-dd")), "VW_Fin_Billsettlement_Voucher");
                    if (!matviewexist)
                    {
                        string Create_MV_SalesDailyByProduct = @"CREATE MATERIALIZED VIEW if not exists mv_fin_unsettled AS 
                                                        SELECT * FROM VW_Fin_unsettled;";
                        ExecuteQuery(Create_MV_SalesDailyByProduct, null, 100000);
                    }
                    else
                    {
                        ExecuteQuery(@"REFRESH MATERIALIZED VIEW mv_fin_unsettled", null, 100000);
                    }
                    trans.Commit();
                    connection.Close();
                    return true;
                }
                else
                {
                    trans.Rollback();
                    connection.Close();
                    return false;
                }
            }
        }

        public bool Run_mv_fin_b2bsettlement()
        {
            var runningdate = DateTime.Now.Date;
            using (var connection = CreateConnection())
            {
                connection.Open();
                var param = new SortedList<string, object>();
                param.Add("lastDateValue", runningdate.AddDays(-1));
                param.Add("currentdate", runningdate);
                var trans = connection.BeginTransaction();
                var rows = ExecuteQuery("Update Cache_View set lastfetch=@currentdate,lastDateValue=@lastDateValue where viewname='mv_fin_b2bsettled' and lastfetch<@currentdate",
                    param, connection, trans, 1000);
                if (rows > 0)
                {
                    string viewExist = @"select matviewname from pg_matviews where matviewname='mv_fin_b2bsettled'";
                    var matviewexist = false;
                    using (var reader = ExecuteReader(viewExist, null))
                    {
                        matviewexist = (reader.Read());
                    }
                    string Query_VW_SalesMonthly = @"CREATE OR REPLACE VIEW VW_Fin_b2bsettled as 
                                select billsettlementmaster.id,bill.companyid,bill.locationid,settlementon
                                from billsettlementmaster
							    inner join billsettlement on billsettlement.billsettlementmasterid = billsettlementmaster.id
							    inner join bill on bill.id =billid
                                where billsettlementmaster.isactive = true
                                and billsettlementmaster.settlementon < {0}
                                and bill.isb2b=true";

                    DeleteOrCreateView(string.Format(string.Format(Query_VW_SalesMonthly, DateTime.Now.Date.ToString("yyyy-MM-dd")), DateTime.Now.Date.ToString("yyyy-MM-dd")), "VW_Fin_Billsettlement_Voucher");
                    if (!matviewexist)
                    {
                        string Create_MV_SalesDailyByProduct = @"CREATE MATERIALIZED VIEW if not exists mv_fin_b2bsettled AS 
                                                        SELECT * FROM VW_Fin_b2bsettled;";
                        ExecuteQuery(Create_MV_SalesDailyByProduct, null, 100000);
                    }
                    else
                    {
                        ExecuteQuery(@"REFRESH MATERIALIZED VIEW mv_fin_b2bsettled", null, 100000);
                    }
                    trans.Commit();
                    connection.Close();
                    return true;
                }
                else
                {
                    trans.Rollback();
                    connection.Close();
                    return false;
                }
            }
        }

        public bool Run_mv_finbillsettlementvoucher()
        {
            var runningdate = DateTime.Now.Date;
            using (var connection = CreateConnection())
            {
                connection.Open();
                var param = new SortedList<string, object>();
                param.Add("lastDateValue", runningdate.AddDays(-1));
                param.Add("currentdate", runningdate);
                var trans = connection.BeginTransaction();
                var rows = ExecuteQuery("Update Cache_View set lastfetch=@currentdate,lastDateValue=@lastDateValue where viewname='mv_fin_billsettlement_voucher' and lastfetch<@currentdate",
                    param, connection, trans, 1000);
                if (rows > 0)
                {
                    string viewExist = @"select matviewname from pg_matviews where matviewname='mv_fin_billsettlement_voucher'";
                    var matviewexist = false;
                    using (var reader = ExecuteReader(viewExist, null))
                    {
                        matviewexist = (reader.Read());
                    }
                    string Query_VW_SalesMonthly = @"CREATE OR REPLACE VIEW VW_Fin_Billsettlement_Voucher AS 
                            select sum(paid_voucher) voucher,billsettlementmaster.companyid,
                            billsettlementmaster.locationid,billsettlementmaster.counterid,
							billsettlementmaster.customerid,billsettlementmaster.settlementon::date
                            from billsettlementmaster
                            where billsettlementmaster.isactive = true
                            and billsettlementmaster.settlementon <'{0}'
                            and paid_voucher!=0 
                            group by billsettlementmaster.customerid,billsettlementmaster.locationid,
							billsettlementmaster.counterid,billsettlementmaster.settlementon::date,
							billsettlementmaster.companyid";

                    DeleteOrCreateView(string.Format(string.Format(Query_VW_SalesMonthly, DateTime.Now.Date.ToString("yyyy-MM-dd")), DateTime.Now.Date.ToString("yyyy-MM-dd")), "VW_Fin_Billsettlement_Voucher");
                    if (!matviewexist)
                    {
                        string Create_MV_SalesDailyByProduct = @"CREATE MATERIALIZED VIEW if not exists mv_fin_billsettlement_voucher AS 
                                                        SELECT * FROM VW_Fin_Billsettlement_Voucher;";
                        ExecuteQuery(Create_MV_SalesDailyByProduct, null, 100000);
                    }
                    else
                    {
                        ExecuteQuery(@"REFRESH MATERIALIZED VIEW mv_fin_billsettlement_voucher", null, 100000);
                    }
                    trans.Commit();
                    connection.Close();
                    return true;
                }
                else
                {
                    trans.Rollback();
                    connection.Close();
                    return false;
                }
            }
        }
        public bool Run_mv_salesdailybyproduct()
        {
            var runningdate = DateTime.Now.Date;
            using (var connection = CreateConnection())
            {
                connection.Open();
                var param = new SortedList<string, object>();
                param.Add("lastDateValue", runningdate.AddDays(-1));
                param.Add("currentdate", runningdate);
                var trans = connection.BeginTransaction();
                var rows = ExecuteQuery("Update Cache_View set lastfetch=@currentdate,lastDateValue=@lastDateValue where viewname='mv_salesdailybyproduct' and lastfetch<@currentdate",
                    param, connection, trans, 1000);
                if (rows > 0)
                {
                    string viewExist = @"select matviewname from pg_matviews where matviewname='mv_salesdailybyproduct'";
                    var matviewexist = false;
                    using (var reader = ExecuteReader(viewExist, null))
                    {
                        matviewexist = (reader.Read());
                    }

                    string Query_VW_SalesDailyByProduct = @"CREATE OR REPLACE VIEW VW_SalesDailyByProduct AS 
                        select bill.billdate::date,sum(billitems.receivable) salevalue,billitems.productid,stock.supplierid,
                        sum(billitems.qty) as qty,sum(stock.ibuyingprice::numeric*billitems.qty::numeric) as cost,bill.locationid from bill
                        inner join billitems on billitems.billid = bill.id and billitems.isactive=true
                        inner join stock on stock.id = stockid
                        where billtype != 9 and bill.isactive=true and billdate<'{0}'
                        group by billitems.productid,bill.billdate::date,billitems.productid,stock.supplierid,bill.locationid;";
                    DeleteOrCreateView(string.Format(string.Format(Query_VW_SalesDailyByProduct, DateTime.Now.Date.ToString("yyyy-MM-dd")), DateTime.Now.Date.ToString("yyyy-MM-dd")), "VW_SalesDailyByProduct");
                    if (!matviewexist)
                    {
                        string Create_MV_SalesDailyByProduct = @"CREATE MATERIALIZED VIEW if not exists mv_salesdailybyproduct AS 
                                                        SELECT * FROM VW_SalesDailyByProduct;";
                        ExecuteQuery(Create_MV_SalesDailyByProduct, null,100000);
                        ExecuteQuery("CREATE UNIQUE INDEX IDX_MV_SalesDailyByProduct ON mv_salesdailybyproduct(billdate,productid,supplierid,locationid)", null, 100000);
                    }
                    else
                    {
                        ExecuteQuery(@"REFRESH MATERIALIZED VIEW CONCURRENTLY mv_salesdailybyproduct", null,100000);
                    }
                    trans.Commit();
                    connection.Close();
                    return true;
                }
                else
                {
                    trans.Rollback();
                    connection.Close();
                    return false;
                }
            }
        }
        public bool Run_mv_purchasemonthlybyproduct()
        {
            var runningdate = DateTime.Now.Date;
            using (var connection = CreateConnection())
            {
                connection.Open();
                var param = new SortedList<string, object>();
                param.Add("lastDateValue", runningdate.AddDays(-1));
                param.Add("currentdate", runningdate);
                var trans = connection.BeginTransaction();
                var rows = ExecuteQuery("Update Cache_View set lastfetch=@currentdate,lastDateValue=@lastDateValue where viewname='mv_montlypurchasebyproduct' and lastfetch<@currentdate",
                    param, connection, trans, 1000);
                if (rows > 0)
                {
                    string viewExist = @"select matviewname from pg_matviews where matviewname='mv_montlypurchasebyproduct'";
                    var matviewexist = false;
                    using (var reader = ExecuteReader(viewExist, null))
                    {
                        matviewexist = (reader.Read());
                    }

                    string Query_VW_SalesDailyByProduct = @"CREATE OR REPLACE VIEW VW_MonthlyPurchaseByProduct AS  
                                                            SELECT min(stock.stockdate::date) datebegin,
                                                            stock.productid,
                                                            stock.supplierid,
															sum(stock.qty - (stock.returnqty+stock.damagedqty+stock.journalqty)) as qty,															
															sum((stock.qty - (stock.returnqty+stock.damagedqty+stock.journalqty))::numeric*stock.ibuyingprice::numeric) as cost,
                                                            date_part('month'::text,stock.stockdate) AS month,
                                                            date_part('year'::text, stock.stockdate) AS year,
                                                            date_part('quarter'::text,stock.stockdate) AS quarter 
                                                            FROM stock
                                                            where stock.stockdate<'{0}'
                                                            GROUP BY (date_part('month'::text, stock.stockdate)), 
                                                            (date_part('year'::text, stock.stockdate)),
                                                            (date_part('quarter'::text, stock.stockdate)),
                                                            stock.productid, stock.supplierid;";
                    DeleteOrCreateView(string.Format(string.Format(Query_VW_SalesDailyByProduct, DateTime.Now.Date.ToString("yyyy-MM-dd")), DateTime.Now.Date.ToString("yyyy-MM-dd")), "VW_MonthlyPurchaseByProduct");
                    if (!matviewexist)
                    {
                        string Create_MV_SalesDailyByProduct = @"CREATE MATERIALIZED VIEW if not exists mv_montlypurchasebyproduct AS 
                                                        SELECT * FROM VW_MonthlyPurchaseByProduct;";
                        ExecuteQuery(Create_MV_SalesDailyByProduct, null, 100000);
                    }
                    else
                    {
                        ExecuteQuery(@"REFRESH MATERIALIZED VIEW mv_montlypurchasebyproduct", null, 100000);
                    }
                    trans.Commit();
                    connection.Close();
                    return true;
                }
                else
                {
                    trans.Rollback();
                    connection.Close();
                    return false;
                }
            }
        }
        public bool Run_mv_salesmonthlybyproduct()
        {
            var runningdate = DateTime.Now.Date;
            using (var connection = CreateConnection())
            {
                connection.Open();
                var param = new SortedList<string, object>();
                param.Add("lastDateValue", runningdate.AddDays(-1));
                param.Add("currentdate", runningdate);
                var trans = connection.BeginTransaction();
                var rows = ExecuteQuery("Update Cache_View set lastfetch=@currentdate,lastDateValue=@lastDateValue where viewname='mv_montlysalesbyproduct' and lastfetch<@currentdate",
                    param, connection, trans, 1000);
                if (rows > 0)
                {
                    string viewExist = @"select matviewname from pg_matviews where matviewname='mv_montlysalesbyproduct'";
                    var matviewexist = false;
                    using (var reader = ExecuteReader(viewExist, null))
                    {
                        matviewexist = (reader.Read());
                    }

                    string Query_VW_SalesDailyByProduct = @"CREATE OR REPLACE VIEW VW_MonthlySalesByProduct AS  
                                                            SELECT min(bill.billdate::date) datebegin,
                                                            stock.productid,
                                                            stock.supplierid,
                                                            sum(billitems.receivable) AS amount,
                                                            sum(billitems.qty) AS qty,
                                                            sum((stock.ibuyingprice * billitems.qty)) AS cost,
                                                            date_part('month'::text, bill.billdate) AS month,
                                                            date_part('year'::text, bill.billdate) AS year,
                                                            date_part('quarter'::text, bill.billdate) AS quarter 
                                                            FROM ((bill
                                                            JOIN billitems ON ((billitems.billid = bill.id)))
                                                            JOIN stock ON ((stock.id = billitems.stockid)))
                                                            WHERE ((bill.billtype <> 9) AND (bill.isactive = true))
                                                            and billdate<'{0}'
                                                            GROUP BY 
                                                            (date_part('month'::text, bill.billdate)), 
                                                            (date_part('year'::text, bill.billdate)),
                                                            (date_part('quarter'::text, bill.billdate)),
                                                            stock.productid, stock.supplierid;";
                    DeleteOrCreateView(string.Format(string.Format(Query_VW_SalesDailyByProduct, DateTime.Now.Date.ToString("yyyy-MM-dd")), DateTime.Now.Date.ToString("yyyy-MM-dd")), "VW_MonthlySalesByProduct");
                    if (!matviewexist)
                    {
                        string Create_MV_SalesDailyByProduct = @"CREATE MATERIALIZED VIEW if not exists mv_montlysalesbyproduct AS 
                                                        SELECT * FROM VW_MonthlySalesByProduct;";
                        ExecuteQuery(Create_MV_SalesDailyByProduct, null, 100000);
                    }
                    else
                    {
                        ExecuteQuery(@"REFRESH MATERIALIZED VIEW mv_montlysalesbyproduct", null, 100000);
                    }
                    trans.Commit();
                    connection.Close();
                    return true;
                }
                else
                {
                    trans.Rollback();
                    connection.Close();
                    return false;
                }
            }
        }
    }
}