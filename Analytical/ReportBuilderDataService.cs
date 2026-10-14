using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
using Newtonsoft.Json;
using System.Text;
using InfyPOS.Data;
using static InfyPOS.AnalyserDataService;
using static InfyPOS.AnalyserDataService.Condtion;
using InfyPOS.Services;

namespace InfyPOS
{
    public class ReportBuilderDataService : SAASData
    {

        string salesQueryWithStock = @"with basestock as
        (
            select salablegoods.iqty-(salablegoods.oqty+salablegoods.jqty+salablegoods.dqty) stockqty,id from salablegoods
        )
        select {0} from basestock
        left outer join billitems on billitems.salablegoodsid = basestock.id and billitems.isactive=true
        left outer join bill on bill.id = billitems.billid
        and bill.isactive = true and bill.billtype !=9 and bill.isb2b=false and bill.billdate>=@fromdate and bill.billdate<@todate {1}
        left outer join customer on customer.id = bill.customerid
        left outer join salablegoods on salablegoods.id = salablegoodsid and salablegoods.isactive = true
        left outer join stock on stock.id = salablegoods.stockid and stock.isactive = true
        left outer join products on products.id = stock.productid
        left outer join items on items.id = stock.itemid and items.isactive = true
        left outer join lrinvoiceitems on lrinvoiceitems.id = stock.invoiceitemid and lrinvoiceitems.isactive = true
        left outer join lrinvoice on lrinvoice.id = lrinvoiceitems.lrinvoiceid and lrinvoice.isactive = true
        left outer join lrentry  on lrentry.id = lrid and lrentry.isactive = true
        left outer join supplier on supplier.id = lrentry.supplierid 
        left outer join suppliercompany on suppliercompany.id = lrinvoice.suppliercompanyid
        group by {2}";

        string salesQuery = @"select {0} from bill 
inner join billitems on billitems.billid = bill.id and billitems.isactive=true
left outer join customer on customer.id = bill.customerid
left outer join salablegoods on salablegoods.id = salablegoodsid and salablegoods.isactive = true
left outer join stock on stock.id = salablegoods.stockid and stock.isactive = true
left outer join products on products.id = stock.productid
left outer join items on items.id = stock.itemid and items.isactive = true
left outer join lrinvoiceitems on lrinvoiceitems.id = stock.invoiceitemid and lrinvoiceitems.isactive = true
left outer join lrinvoice on lrinvoice.id = lrinvoiceitems.lrinvoiceid and lrinvoice.isactive = true
left outer join lrentry  on lrentry.id = lrid and lrentry.isactive = true
left outer join supplier on supplier.id = lrentry.supplierid 
left outer join suppliercompany on suppliercompany.id = lrinvoice.suppliercompanyid
where bill.isactive = true and bill.billtype !=9 and bill.billdate>=@fromdate and bill.billdate<@todate {1}
group by {2}";

        string returnQuery = @"select {0} from lrinvoicereturn 
inner join lrinvoicereturnitems on lrinvoicereturnid = lrinvoicereturn.id
left outer join lrinvoiceitems on lrinvoiceitems.id = lrinvoicereturnitems.id and lrinvoiceitems.isactive = true
left outer join products on products.id = lrinvoicereturnitems.productid
left outer join items on items.id = lrinvoiceitems.itemid and items.isactive = true
left outer join lrinvoice on lrinvoice.id = lrinvoiceitems.lrinvoiceid and lrinvoice.isactive = true
left outer join lrentry  on lrentry.id = lrid and lrentry.isactive = true
left outer join supplier on supplier.id = lrentry.supplierid 
left outer join suppliercompany on suppliercompany.id = lrinvoice.suppliercompanyid
where lrinvoicereturn.returndate>=@fromdate and lrinvoicereturn.returndate<@todate {1}
group by {2}";

        //left outer join(select stockid, sum(COALESCE(salablegoods.iqty,00)
        //-COALESCE(salablegoods.oqty,00)
        //-COALESCE(salablegoods.dqty,00)) available from salablegoods
        //where inwardon>=@fromdate
        //group by stockid) as salablestock on salablestock.stockid = stock.id

        string purchaseQuery = @"
with salesdata as
(
select lrinvoiceitems.id,sum(billitems.qty) qty,
sum(billitems.receivable) receivable,
sum(billitems.additionaldiscount) additionaldiscount,
sum(billitems.discount) discount
from bill 
inner join billitems on billitems.billid = bill.id and billitems.isactive=true
inner join salablegoods on salablegoods.id = salablegoodsid and salablegoods.isactive = true
inner join stock on stock.id = salablegoods.stockid and stock.isactive = true
left outer join products on products.id = stock.productid
left outer join items on items.id = stock.itemid and items.isactive = true
left outer join lrinvoiceitems on lrinvoiceitems.id = stock.invoiceitemid and lrinvoiceitems.isactive = true
left outer join lrinvoice on lrinvoice.id = lrinvoiceitems.lrinvoiceid and lrinvoice.isactive = true
left outer join lrentry  on lrentry.id = lrid and lrentry.isactive = true
left outer join supplier on supplier.id = lrentry.supplierid 
left outer join suppliercompany on suppliercompany.id = lrinvoice.suppliercompanyid
where bill.isactive = true and lrinvoice.isactive = true and lrinvoice.entrydate>=@fromdate and lrinvoice.entrydate<@todate {1}
group by lrinvoiceitems.id
),
returndata as
(
select lrinvoiceitems.id,sum(lrinvoicereturnitems.qty) qty,
sum(lrinvoicereturnitems.amount) receivable,
sum(lrinvoicereturnitems.discount) discount
from lrinvoicereturn 
inner join lrinvoicereturnitems on lrinvoicereturnid = lrinvoicereturn.id
inner join lrinvoiceitems on lrinvoiceitems.id = lrinvoicereturnitems.id and lrinvoiceitems.isactive = true

left outer join products on products.id = lrinvoicereturnitems.productid
left outer join items on items.id = lrinvoiceitems.itemid and items.isactive = true

left outer join lrinvoice on lrinvoice.id = lrinvoiceitems.lrinvoiceid and lrinvoice.isactive = true
left outer join lrentry  on lrentry.id = lrid and lrentry.isactive = true
left outer join supplier on supplier.id = lrentry.supplierid 
left outer join suppliercompany on suppliercompany.id = lrinvoice.suppliercompanyid
where lrinvoicereturn.isactive = true and lrinvoice.isactive = true and lrinvoice.entrydate>=@fromdate and lrinvoice.entrydate<@todate {1}
group by lrinvoiceitems.id
),
STOCKdata as
(
select lrinvoiceitems.id,sum(stock.qty) qty,
sum(stock.transfered) transfered,
sum(stock.returnqty) returnqty,
sum(stock.damagedqty) damaged,
sum(stock.journalqty) journal,
sum((stock.qty - (stock.returnqty+stock.transfered+stock.damagedqty+stock.journalqty))::numeric * stock.buyingprice::numeric) stockvalue,
sum(stock.qty - (stock.returnqty+stock.transfered+stock.damagedqty+stock.journalqty)) stockqty
from stock 
left outer join products on products.id = stock.productid
left outer join items on items.id = stock.itemid and items.isactive = true
left outer join lrinvoiceitems on lrinvoiceitems.id = stock.invoiceitemid and lrinvoiceitems.isactive = true
left outer join lrinvoice on lrinvoice.id = lrinvoiceitems.lrinvoiceid and lrinvoice.isactive = true
left outer join lrentry  on lrentry.id = lrid and lrentry.isactive = true
left outer join supplier on supplier.id = lrentry.supplierid 
left outer join suppliercompany on suppliercompany.id = lrinvoice.suppliercompanyid
where stock.isactive = true and lrinvoice.isactive = true and lrinvoice.entrydate>=@fromdate and lrinvoice.entrydate<@todate {1}
group by lrinvoiceitems.id
),
rstockdata as
(
select lrinvoiceitems.id,sum(salablegoods.iqty) iqty,
sum(salablegoods.oqty) oqty,
sum(salablegoods.dqty) dqty,
sum(salablegoods.jqty) jqty,
sum(COALESCE(salablegoods.tiqty,0)) tiqty,
sum(COALESCE(salablegoods.toqty,0)) toqty,
sum((salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty))::numeric * (stock.buyingprice+case when lrinvoiceitems.purchasetax>0 and lrinvoiceitems.returnqty>0 
	then lrinvoiceitems.returnqty * (lrinvoiceitems.purchasetax/lrinvoiceitems.buyingqty) else 0 end)::numeric) stockvalue,

sum(salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty)) stockqty
from salablegoods
left outer join stock on stock.id = salablegoods.stockid
left outer join products on products.id = stock.productid
left outer join items on items.id = stock.itemid and items.isactive = true
left outer join lrinvoiceitems on lrinvoiceitems.id = stock.invoiceitemid and lrinvoiceitems.isactive = true
left outer join lrinvoice on lrinvoice.id = lrinvoiceitems.lrinvoiceid and lrinvoice.isactive = true
left outer join lrentry  on lrentry.id = lrid and lrentry.isactive = true
left outer join supplier on supplier.id = lrentry.supplierid 
left outer join suppliercompany on suppliercompany.id = lrinvoice.suppliercompanyid
where stock.isactive = true and lrinvoice.isactive = true and lrinvoice.entrydate>=@fromdate and lrinvoice.entrydate<@todate  {1}
group by lrinvoiceitems.id
)
select {0} from lrinvoice
left outer join lrinvoiceitems on lrinvoiceitems.lrinvoiceid = lrinvoice.id and lrinvoiceitems.isactive = true
left outer join salesdata on salesdata.id = lrinvoiceitems.id
left outer join rstockdata on rstockdata.id = lrinvoiceitems.id
left outer join STOCKdata on STOCKdata.id = lrinvoiceitems.id
left outer join returndata on returndata.id = lrinvoiceitems.id
left outer join items on items.id = lrinvoiceitems.itemid and items.isactive = true
left outer join products on products.id = lrinvoiceitems.productid
left outer join lrentry  on lrentry.id = lrid and lrentry.isactive = true
left outer join supplier on supplier.id = lrentry.supplierid
left outer join suppliercompany on suppliercompany.id = lrinvoice.suppliercompanyid
where lrinvoice.isactive = true and lrinvoice.entrydate>=@fromdate and lrinvoice.entrydate<@todate {1}
group by {2}";

        string whStockQuery = @"select {0} from stock
        left outer join products on products.id = stock.productid
        left outer join items on items.id = stock.itemid and items.isactive = true
        left outer join lrinvoiceitems on lrinvoiceitems.id = stock.invoiceitemid and lrinvoiceitems.isactive = true
        left outer join lrinvoice on lrinvoice.id = lrinvoiceitems.lrinvoiceid and lrinvoice.isactive = true
        left outer join lrentry  on lrentry.id = lrid and lrentry.isactive = true
        left outer join supplier on supplier.id = lrentry.supplierid
        left outer join suppliercompany on suppliercompany.id = lrinvoice.suppliercompanyid
        where stock.isactive = true and stock.hasstock = true and stock.qty - (stock.transfered+stock.returnqty+stock.damagedqty+stock.journalqty)>0 {1}
        group by {2}";



        string branchStockQuery = @"
select {0} from salablegoods 
inner join stock on stock.id = salablegoods.stockid and stock.isactive = true
left outer join products on products.id = stock.productid
left outer join items on items.id = stock.itemid and items.isactive = true
left outer join lrinvoiceitems on lrinvoiceitems.id = stock.invoiceitemid and lrinvoiceitems.isactive = true
left outer join lrinvoice on lrinvoice.id = lrinvoiceitems.lrinvoiceid and lrinvoice.isactive = true
left outer join lrentry  on lrentry.id = lrid and lrentry.isactive = true
left outer join supplier on supplier.id = lrentry.supplierid 
left outer join suppliercompany on suppliercompany.id = lrinvoice.suppliercompanyid
where salablegoods.isactive = true and salablegoods.hasstock = true and salablegoods.iqty-(salablegoods.oqty+salablegoods.jqty+salablegoods.dqty)>0 {1}
group by {2}";


        public ReportBuilder BuildReport(ReportBuilder pconfig)
        {
            AnalyserDataService analyserDataService = new AnalyserDataService() { Context = this.Context };
            pconfig.rangelist = new List<ReportBuilder.Options>();
            StringBuilder where = new StringBuilder();
            StringBuilder groups = new StringBuilder();
            StringBuilder columns = new StringBuilder();
            List<string> columnList = new List<string>();
            List<string> groupList = new List<string>();
            int colindex = 0;
            pconfig.UpdateDateRange(pconfig.creterialist);
            foreach (var option in pconfig.columnlist)
            {
                option.index = colindex;
                option.columnname = "c_" + (colindex + 1);
                columns.Append((columns.Length > 0 ? "," : "") + (GetField(pconfig, (Options)option.id, option.customoption, null) + "  as " + option.columnname));
                option.hasreference = analyserDataService.GetReferenceQuery((Options)option.id, option.customoption) != "";

                if (!option.hasreference)
                {
                    option.hasreference = (Options)option.id == Options.SaleWeekDay ||
                        (Options)option.id == Options.SellingMode ||
                        (Options)option.id == Options.BillType ||
                        (Options)option.id == Options.PurchaseMonth ||
                        (Options)option.id == Options.SaleMonth;
                }

                groups.Append((groups.Length > 0 ? "," : "") + (GetField(pconfig, (Options)option.id, option.customoption, null)));
                colindex++;
                columnList.Add(option.columnname);
            }

            pconfig.rangelist = new List<ReportBuilder.Options>();
            colindex = 0;
            var clist = pconfig.creterialist.Where(e => e.showcolumn).ToList();

            foreach (var option in clist)
            {
                pconfig.rangelist.Add(option.option);
                option.option.index = colindex;
                option.option.columnname = "r_" + (colindex + 1);
                columns.Append((columns.Length > 0 ? "," : "") + "(" + GetField(pconfig, (Options)option.option.id, option.option.customoption, option.range) + ") as " + option.option.columnname);
                groups.Append((groups.Length > 0 ? "," : "") + (GetField(pconfig, (Options)option.option.id, option.option.customoption, option.range)));
                colindex++;
                columnList.Add(option.option.columnname);
            }

            //foreach(var grouped in groupedCreteria)
            //{
            //    var option = grouped.Value.First();
            //    pconfig.rangelist.Add(option.option);
            //    option.option.index = colindex;
            //    option.option.columnname = "r_" + (colindex + 1);
            //    columns.Append((columns.Length > 0 ? "," : "") + "(" + GetField(pconfig, (Options)option.option.id, option.range) + ") as " + option.option.columnname);
            //    groups.Append((groups.Length > 0 ? "," : "") + (GetField(pconfig, (Options)option.option.id, option.range)));
            //    colindex++;
            //}


            colindex = 0;
            foreach (var option in pconfig.aggregatelist)
            {
                option.index = colindex;
                option.columnname = "a_" + (colindex + 1);
                var aggcommand = "sum";
                if ((Options)option.id == Options.AvgSaleDays || (Options)option.id == Options.AvgStockDays)
                    aggcommand = "avg";
                groupList.Add(aggcommand + "(" + option.columnname + ") as " + option.columnname);
                columns.Append((columns.Length > 0 ? "," : "") + aggcommand + "(" + GetField(pconfig, (Options)option.id, option.customoption, null) + ") as " + option.columnname);
                colindex++;
            }

            foreach (var option in pconfig.conditionlist)
            {
                switch ((Options)option.id)
                {
                    case Options.ZeroQty:
                        if (pconfig.reportmode == "STOCK")
                        {
                            //if (where.Length > 0) where.Append(" and ");
                            where.Append(" and salablegoods.iqty-(salablegoods.oqty+salablegoods.jqty+salablegoods.dqty)=0");
                        }
                        break;
                    case Options.NegativeQty:
                        if (pconfig.reportmode == "STOCK")
                        {
                            //if (where.Length > 0) where.Append(" and ");
                            where.Append(" and salablegoods.iqty-(salablegoods.oqty+salablegoods.jqty+salablegoods.dqty)<0");
                        }
                        break;
                    case Options.B2BSales:
                        if (pconfig.reportmode == "SALES")
                        {
                            //if (where.Length > 0) where.Append(" and ");
                            where.Append(" and isb2b=true");
                        }
                        break;
                    case Options.CREDITSALES:
                        if (pconfig.reportmode == "SALES")
                        {
                            //if (where.Length > 0) where.Append(" and ");
                            where.Append(" and billtype=1");
                        }
                        break;
                    case Options.SALESRETURN:
                        if (pconfig.reportmode == "SALES")
                        {
                            //if (where.Length > 0) where.Append(" and ");
                            where.Append(" and billtype=8");
                        }
                        break;
                    case Options.APPROVEDSALES:
                        if (pconfig.reportmode == "SALES")
                        {
                            //if (where.Length > 0) where.Append(" and ");
                            where.Append(" and billtype=10");
                        }
                        break;
                }
            }
            if (pconfig.locationid > 0)
            {
                if (pconfig.reportmode == "STOCK")
                {
                    where.Append(" and salablegoods.locationid = @locationid");
                }
                if (pconfig.reportmode == "SALES")
                {
                    where.Append(" and bill.locationid = @locationid");
                }
                else if (pconfig.reportmode == "PURCHASE")
                {
                    where.Append(" and lrentry.locationid = @locationid");
                }
            }


            clist = pconfig.creterialist.ToList();
            SortedDictionary<int, List<ReportBuilder.ReportAttributes>> groupedCreteria = new SortedDictionary<int, List<ReportBuilder.ReportAttributes>>();
            foreach (var grouped in clist.GroupBy(e => e.option.id))
            {
                if (grouped.Count() > 1)
                {
                    where.Append(" and ");
                    /*               Range: 0,
                String: 1,
                Long: 2,
                Date: 3,
                Decmal: 4,
                DayRange: 5*/



                    switch (grouped.First().option.type)
                    {
                        case 1:
                            where.AppendFormat("{0} in ({1})", GetField(pconfig, (Options)grouped.First().option.id, grouped.First().option.customoption, null), string.Join(",", grouped.ToList().ConvertAll(e => "'" + WebAPI.Data.Utility.SafeSQL(e.entervalue) + "'")));
                            break;
                        case 2:
                            where.AppendFormat("{0} in ({1})", GetField(pconfig, (Options)grouped.First().option.id, grouped.First().option.customoption, null), string.Join(",", grouped.ToList().ConvertAll(e => e.selectedvalue)));
                            break;
                        case 3:
                            {
                                where.Append("(");
                                bool isfirst = true;
                                foreach (var opion in grouped)
                                {
                                    if (isfirst)
                                    {
                                        where.Append(" or ");
                                        isfirst = false;
                                    }
                                    where.Append(GetCondtion(pconfig, opion));
                                }
                                where.Append(")");
                            }
                            break;
                    }

                }
                else
                {
                    var condition = GetCondtion(pconfig, grouped.First());
                    if (!string.IsNullOrEmpty(condition))
                    {
                        where.Append(" and ");
                        where.Append(GetCondtion(pconfig, grouped.First()));
                    }
                }
            }

            string query = "";

            if (pconfig.reportmode == "SALES")
            {
                query = string.Format(salesQuery, columns.ToString(), where.ToString(), groups.ToString());
                //if(pconfig.columnlist.Exists(x=>(AnalyserDataService.Options)x.id == AnalyserDataService.Options.StockQty) ||
                //   pconfig.columnlist.Exists(x =>(AnalyserDataService.Options)x.id == AnalyserDataService.Options.StockValue) ||
                //   pconfig.columnlist.Exists(x =>(AnalyserDataService.Options)x.id == AnalyserDataService.Options.StockQty))
                //{
                //    query = string.Format(salesQueryWithStock, columns.ToString(), where.ToString(), groups.ToString());
                //}
            }
            else if (pconfig.reportmode == "PURCHASE")
                query = string.Format(purchaseQuery, columns.ToString(), where.ToString(), groups.ToString());
            else if (pconfig.reportmode == "DEBIT NOTE")
                query = string.Format(returnQuery, columns.ToString(), where.ToString(), groups.ToString());
            else if (pconfig.locationid <= 0)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(@"with totalstock as (");


                var stockquery = "";
                if (groups.Length == 0)
                    stockquery = string.Format(whStockQuery.Replace("group by {2}", ""), columns.ToString(), where.ToString(), groups.ToString());
                else
                    stockquery = string.Format(whStockQuery, columns.ToString(), where.ToString(), groups.ToString());

                stockquery = stockquery.Replace("salablegoods.iqty", "stock.qty");
                stockquery = stockquery.Replace("salablegoods.oqty", "stock.transfered");
                stockquery = stockquery.Replace("salablegoods.dqty", "stock.damagedqty");
                stockquery = stockquery.Replace("salablegoods.jqty", "stock.journalqty");
                stockquery = stockquery.Replace("salablegoods.locationid", "lrentry.locationid");
                stockquery = stockquery.Replace("salablegoods.toqty", "0");
                stockquery = stockquery.Replace("salablegoods.tiqty", "0");
                stockquery = stockquery.Replace("salablegoods.price", "stock.price");
                stockquery = stockquery.Replace("salablegoods.discount", "stock.discount");

                sb.AppendLine(stockquery);
                sb.AppendLine(@" UNION ALL ");

                var branchstock = "";
                if (groups.Length == 0)
                    branchstock = branchStockQuery.Replace("group by {2}", "");
                else
                    branchstock = branchStockQuery;

                branchstock = string.Format(branchstock,
                    pconfig.reportmode == "STOCK" && pconfig.columnlist.Exists(e => e.id == (int)Options.StockLocation) ? columns.ToString().Replace("lrentry.locationid", "salablegoods.locationid") : columns.ToString(),
                    where.ToString(),
                    pconfig.reportmode == "STOCK" && pconfig.columnlist.Exists(e => e.id == (int)Options.StockLocation) ? groups.ToString().Replace("lrentry.locationid", "salablegoods.locationid") : groups.ToString());
                branchstock = branchstock.Replace("stock.qty - (stock.transfered+stock.returnqty+stock.damagedqty+stock.journalqty)", "salablegoods.iqty-(salablegoods.oqty+salablegoods.jqty+salablegoods.dqty)");
                branchstock = branchstock.Replace("stock.qty", "salablegoods.iqty");
                branchstock = branchstock.Replace("stock.hasstock", "salablegoods.hasstock");
                branchstock = branchstock.Replace("stock.transfered", "salablegoods.oqty");
                branchstock = branchstock.Replace("stock.damagedqty", "salablegoods.dqty");
                branchstock = branchstock.Replace("stock.journalqty", "salablegoods.jqty");
                branchstock = branchstock.Replace("stock.price", "salablegoods.price");
                branchstock = branchstock.Replace("stock.returnqty", "0");
                branchstock = branchstock.Replace("stock.discount", "salablegoods.discount");

                sb.AppendLine(branchstock);
                sb.AppendLine(@")");
                sb.AppendLine(@"Select ");
                sb.Append(string.Join(",", columnList));
                if (columnList.Count > 0 && groupList.Count > 0)
                    sb.Append(",");
                sb.Append(string.Join(",", groupList));
                sb.Append(" from totalstock ");
                if (groupList.Count > 0)
                {
                    sb.Append(" group by " + string.Join(",", columnList));
                }
                query = sb.ToString();
            }
            else
            {
                //where.Append(" and salablegoods.locationid=@locationid ");
                var branchstock = string.Format(branchStockQuery, columns.ToString(), where.ToString(), groups.ToString());
                branchstock = branchstock.Replace("stock.qty - (stock.transfered+stock.returnqty+stock.damagedqty+stock.journalqty)", "salablegoods.iqty-(salablegoods.oqty+salablegoods.jqty+salablegoods.dqty)");
                branchstock = branchstock.Replace("stock.qty", "salablegoods.iqty");
                branchstock = branchstock.Replace("stock.transfered", "salablegoods.oqty");
                branchstock = branchstock.Replace("stock.hasstock", "salablegoods.hasstock");
                branchstock = branchstock.Replace("stock.damagedqty", "salablegoods.dqty");
                branchstock = branchstock.Replace("stock.journalqty", "salablegoods.jqty");
                branchstock = branchstock.Replace("stock.price", "salablegoods.price");
                branchstock = branchstock.Replace("stock.returnqty", "0");
                query = branchstock.Replace("stock.discount", "salablegoods.discount");
            }
            DateTime today = pconfig.fromdate == DateTime.MinValue ? DateTimeNow.AddDays(-365) : pconfig.fromdate.Date;
            DateTime nextdate = pconfig.todate == DateTime.MinValue ? DateTimeNow : pconfig.todate.AddDays(1).Date;

            if (pconfig.conditionlist.Exists(e => e.id == (int)Options.WithZeroandNagativeStock) ||
                pconfig.conditionlist.Exists(e => e.id == (int)Options.ZeroQty) ||
                pconfig.conditionlist.Exists(e => e.id == (int)Options.NegativeQty))
            {
                query = query.Replace("and salablegoods.hasstock = true and salablegoods.iqty-(salablegoods.oqty+salablegoods.jqty+salablegoods.dqty)>0", "");
                query = query.Replace("and stock.hasstock = true and stock.qty - (stock.transfered+stock.returnqty+stock.damagedqty+stock.journalqty)>0", "");
            }

            if (Services.ConfigManager.Instance(Context.OrganizationID).Config.analyser_stock_removeaqty)
                query = query.Replace("salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty)", "salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty+salablegoods.aqty)");

            if (query.EndsWith("group by "))
                query = query.Substring(0, query.Length - "group by ".Length);

            QueryBuilder qb = new QueryBuilder();
            qb.CurrentQuery = query;
            NpgsqlConnection conn = CreateConnection();
            List<ReportBuilder.ReportResultItems> lstitems = new List<ReportBuilder.ReportResultItems>();
            try
            {
                conn.Open();
                var command = qb.GetCommand(conn);
                command.Parameters.Add(new NpgsqlParameter("fromdate", today));
                command.Parameters.Add(new NpgsqlParameter("todate", nextdate));
                command.Parameters.Add(new NpgsqlParameter("locationid", pconfig.locationid));


                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    var totalColumn = pconfig.columnlist.Count + pconfig.aggregatelist.Count;
                    var totalconditioncolunms = pconfig.creterialist.Where(e => e.showcolumn).Count();
                    while (reader.Read())
                    {
                        ReportBuilder.ReportResultItems oitem = new ReportBuilder.ReportResultItems();
                        oitem.String = new string[totalColumn];
                        oitem.Values = new long[totalColumn];
                        oitem.RangeResult = new string[totalconditioncolunms];
                        oitem.Amount = new decimal[pconfig.aggregatelist.Count];
                        int index = 0;
                        foreach (var colmn in pconfig.columnlist)
                        {

                            if (colmn.hasreference)
                            {
                                oitem.Values[index] = reader[colmn.columnname] == DBNull.Value ? 0 : Convert.ToInt64(reader[colmn.columnname]);
                            }
                            else
                            {
                                oitem.Values[index] = 0;
                                if (colmn.type == 3)
                                    oitem.String[index] = reader[colmn.columnname] == DBNull.Value ? "" : Convert.ToDateTime(reader[colmn.columnname]).ToString("dd-MM-yyyy");
                                else if (colmn.type == 4)
                                    oitem.String[index] = reader[colmn.columnname] == DBNull.Value ? "" : Convert.ToDecimal(reader[colmn.columnname]).ToString("N2");
                                else
                                    oitem.String[index] = reader[colmn.columnname] == DBNull.Value ? "" : reader[colmn.columnname].ToString();
                            }

                            index++;
                        }
                        index = 0;
                        foreach (var colmn in pconfig.aggregatelist)
                        {
                            oitem.Amount[index] = reader[colmn.columnname] == DBNull.Value ? 0 : Convert.ToDecimal(reader[colmn.columnname]);
                            index++;
                        }
                        index = 0;
                        foreach (var colmn in pconfig.creterialist.Where(e => e.showcolumn))
                        {
                            oitem.RangeResult[index] = reader[colmn.option.columnname] == DBNull.Value ? "" : Convert.ToString(reader[colmn.option.columnname]);
                            index++;
                        }
                        lstitems.Add(oitem);
                    }
                }
                conn.Close();
                UpdateDisplay(pconfig, lstitems);
                pconfig.result = lstitems;
            }
            catch (Exception exp)
            {
                if (conn.State != System.Data.ConnectionState.Closed)
                    conn.Close();
            }

            if(pconfig.topcount>0 && !string.IsNullOrEmpty(pconfig.topfield) &&
                pconfig.result != null)
            {
                if(pconfig.columnlist != null &&
                   pconfig.columnlist.Exists(x=>x.name == pconfig.topfield))
                {
                    var index = pconfig.columnlist.Find(x => x.name == pconfig.topfield).index;
                    pconfig.result = pconfig.result.OrderBy(x => x.String[index]).ToList();
                }
                if (pconfig.aggregatelist != null &&
                pconfig.aggregatelist.Exists(x => x.name == pconfig.topfield))
                {
                    var index = pconfig.aggregatelist.Find(x => x.name == pconfig.topfield).index;
                    pconfig.result = pconfig.result.OrderByDescending(x => x.Amount[index]).ToList();
                }
                if (pconfig.rangelist != null &&
                    pconfig.rangelist.Exists(x => x.name == pconfig.topfield))
                {
                    var index = pconfig.rangelist.Find(x => x.name == pconfig.topfield).index;
                    pconfig.result = pconfig.result.OrderBy(x => x.RangeResult[index]).ToList();
                }

                if(pconfig.result.Count>pconfig.topcount)
                {
                    pconfig.result = pconfig.result.Take(pconfig.topcount).ToList();
                }
            }

            if(pconfig.widgetid>0 && pconfig.result!=null)
            {
                WidgetDataManager.Instance(Context.OrganizationID).Update(pconfig.widgetid,
                    Context.UserID, JsonConvert.SerializeObject(pconfig.result));
            }
            return pconfig;
        }

        private void UpdateDisplayList(List<ReportBuilder.ReportResultItems> source, Options option, string customoption)
        {
            if (source == null || source.Count == 0) return;
            AnalyserDataService analyserDataService = new AnalyserDataService() { Context = this.Context };
            string query = analyserDataService.GetReferenceQuery(option, customoption);
            if (query != "")
            {
                query += (query.Contains(" where ") ? " and " : " where ") + " id in (" + string.Join<long>(",", source.ConvertAll(e => (string.IsNullOrEmpty(e.id) || e.id == "null") ? 0 : long.Parse(e.id))) + ")";
                NpgsqlConnection conn = CreateConnection();
                try
                {
                    conn.Open();
                    NpgsqlCommand command = null;

                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = query;
                    command = qb.GetCommand(conn);
                    SortedDictionary<long, string> displaylist = new SortedDictionary<long, string>();
                    using (System.Data.IDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {

                            long id = reader.IsDBNull(0) ? 0 : Convert.ToInt64(reader[0]);
                            string name = reader.IsDBNull(1) ? "" : reader.GetString(1);
                            displaylist.Add(id, name);
                        }
                    }
                    conn.Close();

                    source.ForEach(e =>
                    {
                        long id;
                        e.name = "";
                        if (long.TryParse(e.id, out id))
                        {
                            if (displaylist.ContainsKey(id))
                            {
                                e.name = displaylist[id];
                            }
                        }
                    });
                }
                catch (Exception exp)
                {
                    if (conn != null) conn.Close();
                    WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, option);
                }
            }
            else if (option == Options.SaleWeekDay)
            {
                var dayList = System.Globalization.DateTimeFormatInfo.CurrentInfo.DayNames;
                source.ForEach(e =>
                {
                    long id;
                    e.name = "";
                    if (long.TryParse(e.id, out id))
                        e.name = dayList[id];
                });
            }
            else if (option == Options.SellingMode)
            {
                source.ForEach(e =>
                {
                    e.name = e.id == "1" ? "B2B" : "RETAIL";
                });
            }
            else if (option == Options.BillType)
            {
                source.ForEach(e =>
                {
                    e.name = e.id == "1" ? "Sales Return" : "Sales Invoice";
                });
            }
            else if (option == Options.PurchaseMonth || option == Options.SaleMonth)
            {
                var monthList = System.Globalization.DateTimeFormatInfo.CurrentInfo.MonthNames;
                source.ForEach(e =>
                {
                    long id;
                    e.name = "";
                    if (long.TryParse(e.id, out id))
                        e.name = monthList[id];
                });
            }
            else if (option == Options.BillDate || option == Options.InvDate || option == Options.PurchaseDate || option == Options.DebitNoteDate)
            {
                source.ForEach(e => e.name = e.id.Split(' ')[0]);
            }
            else
            {
                source.ForEach(e => e.name = e.id);
            }
        }

        private void UpdateDisplayList(List<ReportBuilder.ReportResultItems> source, int index, Options option, string customoption, bool fromid)
        {
            if (source == null || source.Count == 0) return;
            AnalyserDataService analyserDataService = new AnalyserDataService() { Context = this.Context };
            string query = analyserDataService.GetReferenceQuery(option, customoption);
            if (query != "")
            {
                if (fromid)
                    query += (query.Contains(" where ") ? " and " : " where ") + " id in (" + string.Join<long>(",", source.ConvertAll(e => (string.IsNullOrEmpty(e.id) || e.id == "null") ? 0 : long.Parse(e.id))) + ")";
                else
                    query += (query.Contains(" where ") ? " and " : " where ") + " id in (" + string.Join<long>(",", source.ConvertAll(e => e.Values[index])) + ")";
                NpgsqlConnection conn = CreateConnection();
                try
                {
                    conn.Open();
                    NpgsqlCommand command = null;

                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = query;
                    command = qb.GetCommand(conn);
                    SortedDictionary<long, string> displaylist = new SortedDictionary<long, string>();
                    using (System.Data.IDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {

                            long id = reader.IsDBNull(0) ? 0 : Convert.ToInt64(reader[0]);
                            string name = reader.IsDBNull(1) ? "" : reader.GetString(1);
                            displaylist.Add(id, name);
                        }
                    }
                    conn.Close();

                    source.ForEach(e =>
                    {
                        e.String[index] = "";
                        if (displaylist.ContainsKey(e.Values[index]))
                        {
                            e.String[index] = displaylist[e.Values[index]];
                        }
                    });
                }
                catch (Exception exp)
                {
                    if (conn != null) conn.Close();
                    WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, option);
                }
            }
            else if (option == Options.SaleWeekDay)
            {
                var dayList = System.Globalization.DateTimeFormatInfo.CurrentInfo.DayNames;
                source.ForEach(e =>
                {
                    e.String[index] = dayList[e.Values[index]];
                });
            }
            else if (option == Options.SellingMode)
            {
                source.ForEach(e =>
                {
                    e.String[index] = e.Values[index] == 1 ? "B2B" : "RETAIL";
                });
            }
            else if (option == Options.BillType)
            {
                source.ForEach(e =>
                {
                    e.String[index] = e.Values[index] == 1 ? "Sales Return" : "Sales Invoice";
                });
            }
            else if (option == Options.PurchaseMonth || option == Options.SaleMonth)
            {
                var monthList = System.Globalization.DateTimeFormatInfo.CurrentInfo.MonthNames;
                source.ForEach(e =>
                {
                    e.String[index] = monthList[e.Values[index] - 1];
                });
            }
            else
            {
                source.ForEach(e => e.name = e.id);
            }
        }

        public void UpdateDisplay(ReportBuilder _config, List<ReportBuilder.ReportResultItems> lstitems)
        {
            if (lstitems.Count == 0) return;

            foreach (var item in _config.columnlist)
            {
                if (item.hasreference)
                {
                    UpdateDisplayList(lstitems, item.index, (Options)item.id, item.customoption, false);
                }
            }

            int productIndex = _config.columnlist.FindIndex(e => e.name.ToLower() == "product");
            if (productIndex >= 0)
            {
                List<long> ids = lstitems.ConvertAll<long>(e => e.Values[productIndex]);
                Data.ProductsDataService productdata = new ProductsDataService() { Context = this.Context };
                var valuelist = productdata.Search(new ProductsCreteria()
                {
                    ids = ids
                });
                lstitems.ForEach(e =>
                {
                    var index = valuelist.FindIndex(x => x.id == e.Values[productIndex]);
                    if (index >= 0)
                    {
                        e.String[productIndex] = valuelist[index].name;
                    }
                });
            }

            productIndex = _config.columnlist.FindIndex(e => e.name.ToLower() == "brand");
            if (productIndex >= 0)
            {
                List<long> ids = lstitems.ConvertAll<long>(e => e.Values[productIndex]);
                Data.BrandDataService productdata = new BrandDataService() { Context = this.Context };
                var valuelist = productdata.Search(new BrandCreteria()
                {
                    ids = ids
                });
                lstitems.ForEach(e =>
                {
                    var index = valuelist.FindIndex(x => x.id == e.Values[productIndex]);
                    if (index >= 0)
                    {
                        e.String[productIndex] = valuelist[index].name;
                    }
                });
            }

            List<string> referecneTypes = new List<string>();
            if (_config.columnlist.FindIndex(e => e.name == "style") >= 0)
                referecneTypes.Add("'STYLE'");
            if (_config.columnlist.FindIndex(e => e.name == "pattern") >= 0)
                referecneTypes.Add("'PATTERN'");
            if (_config.columnlist.FindIndex(e => e.name == "size") >= 0)
                referecneTypes.Add("'SIZE'");
            if (_config.columnlist.FindIndex(e => e.name == "colour") >= 0)
                referecneTypes.Add("'COLOUR'");
            if (_config.columnlist.FindIndex(e => e.name == "type") >= 0)
                referecneTypes.Add("'TYPE'");
            if (_config.columnlist.FindIndex(e => e.name == "material") >= 0)
                referecneTypes.Add("'MATERIAL'");
            if (referecneTypes.Count > 0)
            {
                Data.ReferencelistDataService referencedata = new ReferencelistDataService() { Context = this.Context };
                var valuelist = referencedata.SearchList(new ReferencelistCreteria()
                {
                    types = referecneTypes
                });
                productIndex = _config.columnlist.FindIndex(e => e.name.ToLower() == "style");
                if (productIndex >= 0)
                {
                    lstitems.ForEach(e =>
                    {
                        var index = valuelist.FindIndex(x => x.id == e.Values[productIndex]);
                        if (index >= 0)
                        {
                            e.String[productIndex] = valuelist[index].name;
                        }
                    });
                }
                productIndex = _config.columnlist.FindIndex(e => e.name.ToLower() == "pattern");
                if (productIndex >= 0)
                {
                    lstitems.ForEach(e =>
                    {
                        var index = valuelist.FindIndex(x => x.id == e.Values[productIndex]);
                        if (index >= 0)
                        {
                            e.String[productIndex] = valuelist[index].name;
                        }
                    });
                }
                productIndex = _config.columnlist.FindIndex(e => e.name.ToLower() == "size");
                if (productIndex >= 0)
                {
                    lstitems.ForEach(e =>
                    {
                        var index = valuelist.FindIndex(x => x.id == e.Values[productIndex]);
                        if (index >= 0)
                        {
                            e.String[productIndex] = valuelist[index].name;
                        }
                    });
                }
                productIndex = _config.columnlist.FindIndex(e => e.name.ToLower() == "colour");
                if (productIndex >= 0)
                {
                    lstitems.ForEach(e =>
                    {
                        var index = valuelist.FindIndex(x => x.id == e.Values[productIndex]);
                        if (index >= 0)
                        {
                            e.String[productIndex] = valuelist[index].name;
                        }
                    });
                }
                productIndex = _config.columnlist.FindIndex(e => e.name.ToLower() == "type");
                if (productIndex >= 0)
                {
                    lstitems.ForEach(e =>
                    {
                        var index = valuelist.FindIndex(x => x.id == e.Values[productIndex]);
                        if (index >= 0)
                        {
                            e.String[productIndex] = valuelist[index].name;
                        }
                    });
                }
                productIndex = _config.columnlist.FindIndex(e => e.name.ToLower() == "material");
                if (productIndex >= 0)
                {
                    lstitems.ForEach(e =>
                    {
                        var index = valuelist.FindIndex(x => x.id == e.Values[productIndex]);
                        if (index >= 0)
                        {
                            e.String[productIndex] = valuelist[index].name;
                        }
                    });
                }
            }
        }


        private string GetCondtion(ReportBuilder pconfig, ReportBuilder.ReportAttributes option)
        {
            switch ((Options)option.option.id)
            {
                case Options.Barcode:
                    return "stock.barcode = '" + option.entervalue + "'";
                case Options.GLNNO:
                    return "stock.serialno = '" + option.entervalue + "'";
                case Options.BillDate:
                    return "bill.billdate::date >='" + option.fromdate.ToString("yyyy-MM-dd") + "' and bill.billdate::date <" + option.todate.AddDays(1).ToString("yyyy-MM-dd") + "'";
                case Options.BillNo:
                    return "bill.billno  ='" + option.entervalue + "'";
                case Options.Brand:
                    return "items.brandid =" + option.selectedvalue;
                case Options.Colour:
                    return "items.colourid=" + option.selectedvalue;

                case Options.Fit:
                    return "items.fitid =" + option.selectedvalue;
                case Options.Sleeve:
                    return "items.sleeveid =" + option.selectedvalue;

                case Options.Design:
                    return "stock.designid  = '" + option.entervalue + "'";
                case Options.DiscountRate:
                    if (pconfig.reportmode == "STOCK" || pconfig.reportmode == "PURCHASE")
                        return "(stock.price-stock.discount) >=" + option.fromvalue + " and (stock.price-stock.discount) <=" + option.tovalue + "";
                    return "(salablegoods.price-salablegoods.discount) >=" + option.fromvalue + " and (salablegoods.price-salablegoods.discount) <=" + option.tovalue + "";
                case Options.DiscountRate_Range:
                    if (option.fromvalue == 0 && option.tovalue == 0) return "";
                    if (pconfig.reportmode == "STOCK" || pconfig.reportmode == "PURCHASE")
                        return "(stock.price-stock.discount)>=" + option.fromvalue.ToString() + " and (stock.price-stock.discount)<=" + option.tovalue.ToString() + "";
                    return "(salablegoods.price-salablegoods.discount)>=" + option.fromvalue.ToString() + " and (salablegoods.price-salablegoods.discount)<=" + option.tovalue.ToString() + "";
                case Options.InvDate:
                    return "lrinvoice.invoicedate >='" + option.fromdate.ToString("yyyy-MM-dd") + "' and lrinvoice.invoicedate <" + option.todate.AddDays(1).ToString("yyyy-MM-dd") + "'";
                case Options.PurchaseDate:
                    return "lrinvoice.entrydate >='" + option.fromdate.ToString("yyyy-MM-dd") + "' and lrinvoice.entrydate <" + option.todate.AddDays(1).ToString("yyyy-MM-dd") + "'";
                case Options.InvoiceNo:
                    return "lrinvoice.invoiceno = '" + option.entervalue + "'";
                case Options.LREntryNo:
                    return "lrentry.lrno  = '" + option.entervalue + "'";
                case Options.Material:
                    return "items.materialid  =" + option.selectedvalue;
                case Options.Pattern:
                    return "items.patternid  =" + option.selectedvalue;
                case Options.Product:
                    return "products.id  =" + option.selectedvalue;
                case Options.ProductGroup:
                    return "products.productgroup  =" + option.selectedvalue;
                case Options.PurchaseMonth:
                    return "date_part('month',lrinvoice.entrydate)  =" + option.selectedvalue;
                case Options.PurchaseRate:
                case Options.PurchaseRate_Range:
                    if (option.fromvalue == 0 && option.tovalue == 0) return "";
                    return "(lrinvoiceitems.buyingprice)>=" + option.fromvalue + " and (lrinvoiceitems.buyingprice)<=" + option.tovalue + "";
                case Options.PurchaseYear:
                    return "date_part('year',lrinvoice.entrydate) =" + option.entervalue;
                case Options.SaleLocation:
                    return "bill.locationid=" + option.selectedvalue;
                case Options.SaleMonth:
                    return "(date_part('month',bill.billdate))>=" + option.fromvalue.ToString("F0") + "::integer and (date_part('month',bill.billdate))<=" + option.tovalue.ToString("F0") + "::integer";

                case Options.TaxPercentage:
                    if (pconfig.reportmode == "STOCK" || pconfig.reportmode == "PURCHASE")
                        return @"round(case when stock.ibuyingprice>stock.buyingprice and 
ibuyingprice>0 and stock.buyingprice>0 then ((ibuyingprice-stock.buyingprice)/stock.buyingprice*100)::numeric
 else
0::numeric
end,0)>=" + option.fromvalue.ToString() + @"::numeric and round(case when stock.ibuyingprice>stock.buyingprice and 
ibuyingprice>0 and stock.buyingprice>0 then ((ibuyingprice-stock.buyingprice)/stock.buyingprice*100)::numeric
 else
0::numeric
end,0)<=" + option.tovalue.ToString() + "::numeric";
                    return "(billitems.taxpercentage)>=" + option.fromvalue.ToString() + " and (billitems.taxpercentage)<=" + option.tovalue.ToString() + "";

                case Options.SaleRate:
                case Options.SaleRate_Range:
                    if (option.fromvalue == 0 && option.tovalue == 0) return "";
                    if (pconfig.reportmode == "STOCK" || pconfig.reportmode == "PURCHASE")
                        return "(stock.price-stock.discount)>=" + option.fromvalue.ToString() + " and (stock.price-stock.discount)<=" + option.tovalue.ToString() + "";
                    return "(salablegoods.price-salablegoods.discount)>=" + option.fromvalue.ToString() + " and (salablegoods.price-salablegoods.discount)<=" + option.tovalue.ToString() + "";
                case Options.SaleMargin_Range:
                    if (option.fromvalue == 0 && option.tovalue == 0) return "";
                    if (pconfig.reportmode == "STOCK" || pconfig.reportmode == "PURCHASE")
                        return "(round(((stock.price-stock.discount)-stock.buyingprice)/stock.buyingprice*100))>=" + option.fromvalue.ToString() + " and (round(((stock.price-stock.discount)-stock.buyingprice)/stock.buyingprice*100))<=" + option.tovalue.ToString() + "";

                    return "(round(((salablegoods.price-salablegoods.discount)-stock.buyingprice)/stock.buyingprice*100))>=" + option.fromvalue.ToString() + " and (round(((salablegoods.price-salablegoods.discount)-stock.buyingprice)/stock.buyingprice*100))<=" + option.tovalue.ToString() + "";
                case Options.RetailMargin_Range:
                    if (option.fromvalue == 0 && option.tovalue == 0) return "";
                    if (pconfig.reportmode == "STOCK" || pconfig.reportmode == "PURCHASE")
                        return "(round(((stock.price-stock.discount)-stock.buyingprice)/stock.buyingprice*100))>=" + option.fromvalue.ToString() + " and (round(((stock.price-stock.discount)-stock.buyingprice)/stock.buyingprice*100))<=" + option.tovalue.ToString() + "";

                    return "(round(((salablegoods.price-salablegoods.discount)-stock.buyingprice)/stock.buyingprice*100))>=" + option.fromvalue.ToString() + " and (round(((salablegoods.price-salablegoods.discount)-stock.buyingprice)/stock.buyingprice*100))<=" + option.tovalue.ToString() + "";
                case Options.StockAge_Range:
                    if (option.tovalue == 0 && option.fromvalue == 0)
                        return "";
                    if (pconfig.reportmode == "STOCK" || pconfig.reportmode == "PURCHASE")
                        return "(current_date - lrinvoice.entrydate::date)>=" + option.fromvalue.ToString("F0") + "::integer and (current_date - lrinvoice.entrydate::date)<=" + option.tovalue.ToString("F0") + "::integer";
                    return "(bill.billdate::date - lrinvoice.entrydate::date)>=" + option.fromvalue.ToString("F0") + "::integer and (bill.billdate::date - lrinvoice.entrydate::date)<=" + option.tovalue.ToString("F0") + "::integer";
                case Options.ShelfAge_Range:
                    if (option.tovalue == 0 && option.fromvalue == 0)
                        return "";
                    if (pconfig.reportmode == "STOCK" || pconfig.reportmode == "PURCHASE")
                        return "(current_date - salablegoods.inwardon::date)>=" + option.fromvalue.ToString("F0") + "::integer and (current_date - lrinvoice.entrydate::date)<=" + option.tovalue.ToString("F0") + "::integer";
                    return "(bill.billdate::date - salablegoods.inwardon::date)>=" + option.fromvalue.ToString("F0") + "::integer and (bill.billdate::date - lrinvoice.entrydate::date)<=" + option.tovalue.ToString("F0") + "::integer";
                case Options.SalesmanName:
                    return "billitems.salesmanid  =" + option.selectedvalue;
                case Options.SaleYear:
                    return "date_part('year',bill.billdate) = " + option.entervalue;
                case Options.Size:
                    return "items.sizeid =" + option.selectedvalue;
                case Options.Item:
                    return "items.id =" + option.selectedvalue;
                case Options.Agent:
                    return "lrentry.agentid =" + option.selectedvalue;
                case Options.Section:
                    return "products.salescounterid =" + option.selectedvalue;
                case Options.SoldRate:
                    return "billitems.salerate >=" + option.fromvalue + " and billitems.salerate <=" + option.tovalue + "";
                case Options.SoldRate_Range:
                    if (option.fromvalue == 0 && option.tovalue == 0) return "";
                    return "(billitems.salerate)>=" + option.fromvalue.ToString() + " and (billitems.salerate)<=" + option.tovalue.ToString() + "";
                case Options.StockLocation:
                    if (pconfig.reportmode == "PURCHASE")
                        return "lrentry.entrylocationid  =" + option.selectedvalue;
                    if (pconfig.reportmode == "STOCK")
                        return "lrentry.locationid  =" + option.selectedvalue;
                    return "salablegoods.locationid  =" + option.selectedvalue;
                case Options.Style:
                    return "items.styleid  =" + option.selectedvalue;
                case Options.Supplier:
                    return "supplier.id =" + option.selectedvalue;
                case Options.Supplier_City:
                    return "suppliercompany.cityid  =" + option.selectedvalue;
                case Options.Type:
                    return "items.typeid  =" + option.selectedvalue;
                case Options.Tax:
                    return "products.salestaxid in  =" + option.selectedvalue;
                case Options.SellingMode:
                    return "bill.isb2b =" + option.selectedvalue.ToString() == "8" ? "true" : "false";
                case Options.BillType:
                    if (option.selectedvalue.ToString() == "1")
                        return "bill.billtype = 8";
                    return "bill.billtype != 8";
                case Options.SaleWeekDay:
                    return "(date_part('dow',bill.billdate))>=" + option.fromvalue.ToString("F0") + "::integer and (date_part('dow',bill.billdate))<=" + option.tovalue.ToString("F0") + "::integer";
                case Options.SaleMonthDay:
                    return "(date_part('d',bill.billdate))>=" + option.fromvalue.ToString("F0") + "::integer and (billitems.salerate)<=" + option.tovalue.ToString("F0") + "::integer";

                case Options.HSNCode:
                    return "stock.hsncode = '" + option.entervalue + "'";
            }
            return option.option.name + "=" + option.option.name;
        }


        private string getDayRangeCondtion(Condtion condition, string field, string typeCast)
        {
            var rangelist = condition.range.getList();
            List<string> conditionlist = new List<string>();
            foreach (var item in condition.stringList)
            {
                string[] parts = item.Replace(" ", "").Split('-');
                decimal start = 0; decimal end = 0;
                if (parts.Length == 1)
                {
                    if (decimal.TryParse(parts[0], out start))
                    {
                        if (rangelist[0].end == start)
                        {
                            conditionlist.Add(field + "<" + rangelist[0].end + typeCast);
                        }
                        else
                        {
                            conditionlist.Add(field + ">" + rangelist[0].start + typeCast);
                        }
                    }
                }
                else if (decimal.TryParse(parts[0], out start) && decimal.TryParse(parts[1], out end))
                {
                    if (rangelist.Exists(e => e.start == start && e.end == end))
                    {
                        var rng = rangelist.Find(e => e.start == start && e.end == end);
                        conditionlist.Add(field + " BETWEEN " + rng.start + " AND " + rng.end + typeCast);
                    }
                }
            }
            return "(" + string.Join(" OR ", conditionlist) + ")";
        }

        private string getRangeCondtion(Condtion condition, string field, string typeCast)
        {
            var rangelist = condition.range.getList();
            List<string> conditionlist = new List<string>();
            foreach (var item in condition.stringList)
            {
                string[] parts = item.Replace(" ", "").Split('-');
                decimal start = 0; decimal end = 0;
                if (parts.Length == 1)
                {
                    if (decimal.TryParse(parts[0], out start))
                    {
                        if (rangelist[0].end == start)
                        {
                            conditionlist.Add(field + "<" + rangelist[0].end + typeCast);
                        }
                        else
                        {
                            conditionlist.Add(field + ">" + rangelist[0].start + typeCast);
                        }
                    }
                }
                else if (decimal.TryParse(parts[0], out start) && decimal.TryParse(parts[1], out end))
                {
                    if (rangelist.Exists(e => e.start == start && e.end == end))
                    {
                        var rng = rangelist.Find(e => e.start == start && e.end == end);
                        conditionlist.Add(field + " BETWEEN " + rng.start + typeCast + " AND " + rng.end + typeCast);
                    }
                }
            }
            return "(" + string.Join(" OR ", conditionlist) + ")";
        }

        string GetField(ReportBuilder config, AnalyserDataService.Options option,
            string coption, Range range)
        {
            switch (option)
            {
                case Options.Barcode:
                    if (config.reportmode == "DEBIT NOTE")
                        return "lrinvoicereturnitems.barcode";
                    return "stock.barcode";
                case Options.GLNNO:
                    if (config.reportmode == "DEBIT NOTE")
                        return "itemdetail->>'serialno'";
                    return "stock.serialno";
                case Options.BillDate:
                    return "bill.billdate::date";
                case Options.BillNo:
                    return "bill.billno";
                case Options.Brand:
                    return "items.brandid";
                case Options.Colour:
                    return "items.colourid";

                case Options.Sleeve:
                    return "items.sleeveid";
                case Options.Fit:
                    return "items.fitid";
                case Options.Design:
                    if (config.reportmode == "PURCHASE")
                        return "lrinvoiceitems.designid";
                    if(config.reportmode == "DEBIT NOTE")
                        return "lrinvoicereturnitems.designid";
                    return "stock.designid";
                case Options.DiscountRate:
                    if (config.reportmode == "STOCK" || config.reportmode == "PURCHASE")
                        return "(stock.price-stock.discount)";
                    return "(salablegoods.price-salablegoods.discount)";
                case Options.DiscountRate_Range:
                    if (config.reportmode == "STOCK" || config.reportmode == "PURCHASE")
                        return getRangeCase(range, "(stock.price-stock.discount)", "");
                    return getRangeCase(range, "(salablegoods.price-salablegoods.discount)", "");
                case Options.PurchaseDate:
                    return "lrinvoice.entrydate";
                case Options.Company:
                    return "lrinvoice.companyid";
                case Options.InvDate:
                    return "lrinvoice.invoicedate";

                case Options.InvoiceNo:
                    return "lrinvoice.invoiceno";
                case Options.LREntryNo:
                    return "lrentry.lrentryno";
                case Options.Material:
                    return "items.materialid";
                case Options.Pattern:
                    return "items.patternid";
                case Options.Product:
                    return "products.id";
                case Options.ProductGroup:
                    return "products.productgroup";
                case Options.PurchaseMonth:
                    return "date_part('month',lrinvoice.entrydate)";
                case Options.PurchaseRate:
                    return "lrinvoiceitems.buyingprice";
                case Options.PurchaseYear:
                    return "date_part('year',lrinvoice.entrydate)";
                case Options.SaleLocation:
                    return "bill.locationid";
                case Options.SaleMonth:
                    return "date_part('month',bill.billdate)";
                case Options.SaleRate:
                    if (config.reportmode == "STOCK" || config.reportmode == "PURCHASE")
                        return "(stock.price::numeric-stock.discount::numeric)";
                    return "(salablegoods.price::numeric-salablegoods.discount::numeric)";

                case Options.TaxPercentage:
                    if (config.reportmode == "STOCK" || config.reportmode == "PURCHASE")
                        return @"round(case when stock.ibuyingprice>stock.buyingprice and 
                                ibuyingprice>0 and stock.buyingprice>0 then ((ibuyingprice-stock.buyingprice)/stock.buyingprice*100)::numeric
                                 else
                                0::numeric
                                end,0)";
                    return "(billitems.taxpercentage)";
                case Options.SaleRate_Range:
                    if (config.reportmode == "STOCK" || config.reportmode == "PURCHASE")
                        return getRangeCase(range, "(stock.price-stock.discount)", "");
                    return getRangeCase(range, "(salablegoods.price-salablegoods.discount)", "");
                case Options.SaleMargin_Range:
                    return getRangeCase(range, "(round(((stock.price-stock.discount)-stock.buyingprice)/stock.buyingprice*100))", "::numeric");
                case Options.RetailMargin_Range:
                    if (config.reportmode == "STOCK" || config.reportmode == "PURCHASE")
                        return getRangeCase(range, "(round(((stock.price-stock.discount)-stock.buyingprice)/stock.buyingprice*100))", "::numeric");
                    return getRangeCase(range, "(round(((salablegoods.price-salablegoods.discount)-stock.buyingprice)/stock.buyingprice*100))", "::numeric");
                case Options.StockAge_Range:
                    if (config.reportmode == "STOCK" || config.reportmode == "PURCHASE")
                        return getRangeCase(range, "(current_date - lrinvoice.entrydate::date)", "::integer");
                    return getRangeCase(range, "(bill.billdate::date - lrinvoice.entrydate::date)", "::integer");
                case Options.ShelfAge_Range:
                    if (config.reportmode == "STOCK" || config.reportmode == "PURCHASE")
                        return getRangeCase(range, "(@current_date::date - salablegoods.inwardon::date)", "::integer");
                    return getRangeCase(range, "(bill.billdate::date - salablegoods.inwardon::date)", "::integer");
                case Options.PurchaseRate_Range:
                    return getRangeCase(range, "lrinvoiceitems.basebuyingprice", "");
                case Options.SaleWeekDay:
                    return "date_part('dow',bill.billdate)";
                case Options.SellingMode:
                    return "bill.isb2b::integer";
                case Options.SaleReason:
                    return "Bill.salereasonid";
                case Options.Floor:
                    return "Bill.floorid";
                case Options.Area:
                    return "Bill.areaid";
                case Options.Season:
                    return "Bill.seasonid";
                case Options.Counter:
                    return "bill.counterid";
                case Options.BillType:
                    return "case when bill.billtype=8 then 1::integer else 0::integer end";
                case Options.SaleMonthDay:
                    return getDayRangeCase(range, "date_part('d',bill.billdate)", "::integer");
                case Options.SalesmanName:
                case Options.SalesmanCode:
                    return "billitems.salesmanid";
                case Options.SaleYear:
                    return "date_part('year',bill.billdate)";
                case Options.Section:
                    return "products.salescounterid";
                case Options.Agent:
                    return "lrentry.agentid";
                case Options.Size:
                    return "items.sizeid";
                case Options.SoldRate:
                    return "billitems.salerate";
                case Options.SoldRate_Range:
                    return getRangeCase(range, "billitems.salerate", "");
                case Options.StockLocation:
                    if (config.reportmode == "PURCHASE")
                        return "lrentry.entrylocationid";
                    if (config.reportmode != "SALES")
                        return "lrentry.locationid";
                    return "salablegoods.locationid";
                case Options.Style:
                    return "items.styleid";
                case Options.Supplier:
                    return "supplier.id";
                case Options.Customer_Name:
                    return "customer.id";
                case Options.Supplier_City:
                    return "suppliercompany.cityid";
                case Options.Type:
                    return "items.typeid";
                case Options.Item:
                    return "items.id";
                case Options.HSNCode:
                    return "stock.hsncode";
                case Options.Tax:
                    return "products.salestaxid";
                case Options.PurchaseQty:
                    return "lrinvoiceitems.buyingqty";
                case Options.SaleQty:
                    if (config.reportmode == "PURCHASE")
                        return "salesdata.qty";
                    if (config.reportmode == "SALES")
                        return "billitems.qty";
                    return "COALESCE(salablegoods.oqty,0)";
                case Options.PurchaseValue:
                    return "(lrinvoiceitems.buyingqty::numeric * lrinvoiceitems.buyingprice::numeric)+lrinvoiceitems.purchasetax::numeric";
                case Options.SaleValue:
                    if (config.reportmode == "PURCHASE")
                        return "salesdata.receivable";
                    if (config.reportmode == "SALES")
                        return "billitems.receivable";
                    return "salablegoods.oqty::numeric*(salablegoods.price-salablegoods.discount)::numeric";
                case Options.AddDiscountRate:
                    return "billitems.additionaldiscount";
                case Options.ReturnQty:
                    if (config.reportmode == "PURCHASE")
                        return "lrinvoiceitems.returnqty";
                    if (config.reportmode == "DEBIT NOTE")
                        return "lrinvoicereturnitems.qty";
                    if (config.reportmode == "STOCK")
                        return "COALESCE(stock.returnqty,0)";
                    return "COALESCE(salablegoods.rqty,0)";
                case Options.DebitNoteQty:
                    if (config.reportmode == "DEBIT NOTE")
                        return "lrinvoicereturnitems.qty";
                    return "returndata.qty";
                case Options.DebitNoteValue:
                    if (config.reportmode == "DEBIT NOTE")
                        return "lrinvoicereturnitems.amount+lrinvoicereturnitems.tax";
                    return "returndata.qty::numeric*lrinvoiceitems.buyingprice::numeric";
                case Options.JournalQty:
                    if (config.reportmode == "PURCHASE")
                        return "(STOCKdata.damaged+rstockdata.dqty)";
                    else
                        return "(stock.damagedqty)";
                case Options.JournalValue:
                    if (config.reportmode == "PURCHASE")
                        return "(STOCKdata.damaged+rstockdata.dqty)::numeric*lrinvoiceitems.buyingprice::numeric";
                    else
                        return "(stock.damagedqty)::numeric*lrinvoiceitems.buyingprice::numeric";
                case Options.ReturnValue:
                    if (config.reportmode == "PURCHASE")
                        return "lrinvoiceitems.returnqty*lrinvoiceitems.buyingprice::numeric";
                    if (config.reportmode == "STOCK")
                        return "COALESCE(stock.returnqty,0)*stock.buyingprice::numeric";
                    if (config.reportmode == "DEBIT NOTE")
                        return "COALESCE(lrinvoicereturnitems.amount,0)*COALESCE(lrinvoicereturnitems.tax,0)";
                    return "COALESCE(salablegoods.rqty,0)*stock.buyingprice::numeric";
                case Options.EntryLocation:
                    return "lrentry.entrylocationid";
                case Options.LRNo:
                    return "lrentry.lrno";
                case Options.LRDate:
                    return "lrentry.lrdate";
                case Options.DebitNoteDate:
                    return "lrinvoicereturn.returndate";
                case Options.DebitNoteNo:
                    return "lrinvoicereturn.returnno";
                case Options.LREntryDate:
                    return "lrentry.lrentrydate";
                case Options.SchemeDiscount:
                    return "billitems.schemediscount";
                case Options.SaleTax:
                    return "billitems.tax";
                case Options.PurchaseDiscount:
                    return "lrinvoiceitems.purchasediscount";
                case Options.Promotion:
                    return "billitems.schemeid";
                case Options.PurchaseTax:
                    return "round(case when lrinvoiceitems.buyingqty!=0 then round(lrinvoiceitems.purchasetax/lrinvoiceitems.buyingqty) else 0 end)";
                case Options.Margin:
                    if (config.reportmode == "PURCHASE")
                        return "(round(((lrinvoiceitems.sellingprice-lrinvoiceitems.salediscount)-lrinvoiceitems.buyingprice)/lrinvoiceitems.buyingprice*100))";
                    if (config.reportmode == "STOCK")
                        return "(round(((stock.price-stock.discount)-stock.buyingprice)/stock.buyingprice*100))";
                    return "(round(((salablegoods.price-salablegoods.discount)-stock.buyingprice)/stock.buyingprice*100))";
                case Options.StockQty:
                    if (config.reportmode == "PURCHASE")
                        return "rstockdata.stockqty+STOCKdata.stockqty";
                    if (config.reportmode == "SALES")
                        return "basestock.stockqty";
                    if (config.reportmode == "STOCK" || config.reportmode == "PURCHASE")
                        return "stock.qty - (stock.transfered+stock.returnqty+stock.damagedqty+stock.journalqty)";
                    return "salablegoods.iqty - (salablegoods.oqty +salablegoods.jqty + salablegoods.dqty)";
                case Options.StockValue:
                    if (config.reportmode == "PURCHASE")
                        return "rstockdata.stockvalue+STOCKdata.stockvalue";
                    if (config.reportmode == "SALES")
                        return "basestock.stockqty*stock.buyingprice::numeric";
                    if (config.reportmode == "STOCK")
                        return "stock.buyingprice::numeric * (stock.qty - (stock.transfered+stock.returnqty+stock.damagedqty+stock.journalqty))";
                    return "stock.buyingprice::numeric * (salablegoods.iqty - (salablegoods.oqty +salablegoods.jqty + salablegoods.dqty))";
                case Options.IStockValue:
                    if (config.reportmode == "PURCHASE")
                        return "rstockdata.stockvalue+STOCKdata.stockvalue";
                    if (config.reportmode == "SALES")
                        return "basestock.stockqty*stock.ibuyingprice::numeric";
                    if (config.reportmode == "STOCK")
                        return "stock.ibuyingprice::numeric * (stock.qty - (stock.transfered+stock.returnqty+stock.damagedqty+stock.journalqty))";
                    return "stock.ibuyingprice::numeric * (salablegoods.iqty - (salablegoods.oqty +salablegoods.jqty + salablegoods.dqty))";
                case Options.StockSaleValue:
                    if (config.reportmode == "PURCHASE")
                        return "rstockdata.stockvalue+STOCKdata.stockvalue";
                    if (config.reportmode == "SALES")
                        return "basestock.stockqty*basestock.price::numeric";
                    if (config.reportmode == "STOCK")
                        return "(stock.price::numeric-stock.discount::numeric) * (stock.qty - (stock.transfered+stock.returnqty+stock.damagedqty+stock.journalqty))";
                    return "(salablegoods.price::numeric-salablegoods.discount::numeric) * (salablegoods.iqty - (salablegoods.oqty +salablegoods.jqty + salablegoods.dqty))";
                case Options.AvgStockDays:
                    return "(current_date - lrinvoice.entrydate::date)";
                case Options.AvgSaleDays:
                    return "(bill.billdate::date - lrinvoice.entrydate::date)";
                case Options.InwardQty:
                    if (config.reportmode == "PURCHASE")
                        return "rstockdata.tiqty";
                    if (config.reportmode == "STOCK")
                        return "stock.qty";
                    return "COALESCE(salablegoods.tiqty,0)";
                case Options.OutwardQty:
                    if (config.reportmode == "PURCHASE")
                        return "rstockdata.toqty";
                    if (config.reportmode == "STOCK")
                        return "stock.transfered";
                    return "COALESCE(salablegoods.toqty,0)";
                case Options.Custom_Fields:
                    return GetCustomField(coption);
                case Options.Custom_ProductGroups:
                    return "products.productgroup_" + coption.Split(':')[0];
                case Options.ReturnReason:
                    return "Bill.returnreasonid";
                case Options.TodayYesterDay:
                case Options.WeekLastWeek:
                case Options.MonthLastMonth:
                case Options.YearLastYear:
                case Options.FinYearLastFinYear:
                    if (config.reportmode == "PURCHASE")
                        return getDateRangeCase(config, option, "lrinvoice.entrydate");
                    else
                        return getDateRangeCase(config, option, "bill.billdate");
            }
            return option.ToString();
        }

        private string getDateRangeCase(ReportBuilder config, Options option, string field)
        {

            Tuple<DateTime, DateTime> tuple = config.GetDateFromRange("LAST2DAYS");
            string formula = @"case 
                             when {0}>='{1}' and {0}<'{2}' then '{3}'
                             when {0}>='{4}' and {0}<'{5}' then '{6}'
                             else '{7}' end";

            string casefield = string.Format(formula, field, tuple.Item2.ToString("yyyy-MM-dd"),
                             tuple.Item2.AddDays(1).ToString("yyyy-MM-dd"),
                             "Today", tuple.Item1.ToString("yyyy-MM-dd"),
                             tuple.Item1.AddDays(1).ToString("yyyy-MM-dd"),
                             "Yesterday", "Other");

            if (option == Options.WeekLastWeek)
            {
                tuple = config.GetDateFromRange("LAST2WEEK");
                casefield = string.Format(formula, field,
                             tuple.Item2.AddDays(-7).ToString("yyyy-MM-dd"),
                             tuple.Item2.AddDays(1).ToString("yyyy-MM-dd"),
                             "This Week", tuple.Item1.ToString("yyyy-MM-dd"),
                             tuple.Item1.AddDays(7).ToString("yyyy-MM-dd"),
                             "Last Week", "Other");

            }
            if (option == Options.MonthLastMonth)
            {
                tuple = config.GetDateFromRange("LAST2MONTH");
                casefield = string.Format(formula, field,
                 tuple.Item1.ToString("yyyy-MM-dd"),
                 tuple.Item1.AddMonths(1).ToString("yyyy-MM-dd"),
                 "Last Month", tuple.Item1.AddMonths(1).ToString("yyyy-MM-dd"),
                 tuple.Item2.AddDays(1).ToString("yyyy-MM-dd"),
                 "This Month", "Other");
            }
            if (option == Options.YearLastYear)
            {
                tuple = config.GetDateFromRange("LAST2YEAR");
                casefield = string.Format(formula, field,
                 tuple.Item1.ToString("yyyy-MM-dd"),
                 tuple.Item1.AddYears(1).ToString("yyyy-MM-dd"),
                 "Last Year", tuple.Item1.AddYears(1).ToString("yyyy-MM-dd"),
                 tuple.Item2.AddDays(1).ToString("yyyy-MM-dd"),
                 "This Year", "Other");
            }
            if (option == Options.FinYearLastFinYear)
            {
                tuple = config.GetDateFromRange("LAST2FINYEAR");
                casefield = string.Format(formula, field,
                 tuple.Item1.ToString("yyyy-MM-dd"),
                 tuple.Item1.AddYears(1).ToString("yyyy-MM-dd"),
                 "Last Year", tuple.Item1.AddYears(1).ToString("yyyy-MM-dd"),
                 tuple.Item2.AddDays(1).ToString("yyyy-MM-dd"),
                 "This Year", "Other");
            }
            return casefield;
        }


        private string GetCustomField(string customoption)
        {
            var customoptions = new CustomField(customoption);
            return "lrinvoiceitems." + customoptions.cfname;
        }

        private string getRangeCase(Condtion.Range rangecondition, string field, string typeCast)
        {
            var rangelist = rangecondition.getList();
            System.Text.StringBuilder conditionlist = new System.Text.StringBuilder();
            foreach (var rng in rangelist)
            {
                if (rng.start == 0)
                {
                    conditionlist.AppendLine("when " + field + "<=" + rng.end + typeCast + " then '" + rng.end + "'");
                }
                else if (rng.end == 0)
                {
                    conditionlist.AppendLine("when " + field + ">=" + rng.start + typeCast + " then '" + rng.start + "'");
                }
                else
                {
                    conditionlist.AppendLine("when " + field + " BETWEEN " + rng.start + typeCast + " AND " + rng.end + typeCast + " then '" + rng.start + " - " + rng.end + "'");
                }

            }
            return " case " + conditionlist.ToString() + " end ";
        }

        private string getRangeCase(Condtion.Range rangecondition, string field)
        {
            var rangelist = rangecondition.getList();
            System.Text.StringBuilder conditionlist = new System.Text.StringBuilder();
            foreach (var rng in rangelist)
            {
                if (rng.start == 0)
                {
                    conditionlist.AppendLine("when " + field + ">=" + rng.end + " then " + rng.end);
                }
                else if (rng.end == 0)
                {
                    conditionlist.AppendLine("when " + field + "<=" + rng.start + " then " + rng.start);
                }
                else
                {
                    conditionlist.AppendLine("when " + field + " BETWEEN " + rng.start + " AND" + rng.end + " then " + rng.start + " - " + rng.end);
                }

            }
            return " case " + conditionlist.ToString() + " end ";
        }

        private string getDayRangeCase(Condtion.Range rangecondition, string field, string typeCast)
        {
            rangecondition = new Condtion.Range();
            var rangelist = rangecondition.getDayList();
            System.Text.StringBuilder conditionlist = new System.Text.StringBuilder();
            foreach (var rng in rangelist)
            {
                if (rng.start == 0)
                {
                    conditionlist.AppendLine("when " + field + "<=" + rng.end + typeCast + " then '" + rng.end + "'");
                }
                else if (rng.end == 0)
                {
                    conditionlist.AppendLine("when " + field + ">=" + rng.start + typeCast + " then '" + rng.start + "'");
                }
                else
                {
                    conditionlist.AppendLine("when " + field + " BETWEEN " + rng.start + typeCast + " AND " + rng.end + typeCast + " then '" + rng.start + " - " + rng.end + "'");
                }

            }
            return " case " + conditionlist.ToString() + " end ";
        }
        private string getRangeCondtion(Condtion condition, string field)
        {
            var rangelist = condition.range.getList();
            List<string> conditionlist = new List<string>();
            foreach (var item in condition.longList)
            {
                if (rangelist.Exists(e => e.start == item))
                {
                    var rng = rangelist.Find(e => e.start == item);
                    if (rng.end == 0)
                    {
                        conditionlist.Add(field + ">=" + rng.start);
                    }
                    else if (rng.start == 0)
                    {
                        conditionlist.Add(field + "<=" + rng.end);
                    }
                    else
                    {
                        conditionlist.Add(field + " BETWEEN " + rng.start + " AND" + rng.end);
                    }
                }
            }
            return "(" + string.Join(" OR ", conditionlist) + ")";
        }



    }
}