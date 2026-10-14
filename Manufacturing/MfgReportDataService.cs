using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
using InfyPOS.Controllers;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Logical;
using static InfyPOS.Processors.OfflineClient;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Database;
using InfyPOS.QuantoAPI;
using System.Diagnostics;
using WebAPI.Data;


namespace InfyPOS.Data
{
    public class MfgReportDataService : SAASData
    {
        /*with allgrnitems
as
(
select mfgworkorderitemsizesid,sum(mfgworkordergrnitemsizes.qty) grnqty from mfgworkordergrn
					inner join mfgworkorder on mfgworkorder.id = workorderid
					inner join Mfgworkordergrnitems on Mfgworkordergrnitems.mfgworkordergrnid = mfgworkordergrn.id
					inner join mfgworkordergrnitemsizes on mfgworkordergrnitemsizes.mfgworkorderitemsgrnid = Mfgworkordergrnitems.id
					where mfgworkordergrn.isactive = true and Mfgworkordergrnitems.isactive = true and mfgworkordergrnitemsizes.isactive = true
	and mfgworkordergrnitemsizes.qty>0
	group by mfgworkorderitemsizesid
)

                    select qty,delivered,allgrnitems.grnqty,mfgworkorder.workorderno,mfgworkorder.wodate from mfgworkorder 
                    inner join Mfgworkorderitemsizes on Mfgworkorderitemsizes.mfgworkorderid = mfgworkorder.id
                    inner join Mfgworkorderitems on Mfgworkorderitems.id = Mfgworkorderitemsizes.mfgworkorderitemsid
					inner join allgrnitems on allgrnitems.mfgworkorderitemsizesid = Mfgworkorderitemsizes.id
                    where Mfgworkorderitems.isactive=true and Mfgworkorderitemsizes.isactive = true
                    and mfgworkorder.isactive=true and delivered=0 and Mfgworkorderitemsizes.qty>0 	
					
					*/

        public List<WIPTrace> WorkOrderGRNTrace(MfgReportServiceController.WorkOrdreTraceCriteria criteria)
        {
            string query = @"with alluid
as
(
select mfgdesign.code,schemelist->>'sizegroup' sizegroup,schemelist->>'uid' uid,cast(schemelist->>'sizeid' as bigint) sizeid,
cast(schemelist->>'wsrate' as numeric) wsrate,cast(schemelist->>'dealerprice' as numeric) dealerprice,
cast(schemelist->>'salerate' as numeric) salerate,cast(schemelist->>'mrp' as numeric) mrp,
schemelist->>'code' dname,cast(schemelist->>'colourid' as bigint) colourid from mfgdesign 
LEFT JOIN LATERAL
json_array_elements (cast(mfgdesign.sizegroup->>'items' as json)) schemelist ON true
where mfgdesign.isactive=true and schemelist->>'code' = @designitemname
)

					select  sum(qty) qty,mfgworkorder.workorderno,mfgworkorder.wodate,mfgworkordergrn.invoicedate,
					mfgworkordergrnitemsizes.uid,referencelist.name size,mfgworkordergrn.entryno from mfgworkordergrn
					inner join mfgworkorder on mfgworkorder.id = workorderid
					inner join Mfgworkordergrnitems on Mfgworkordergrnitems.mfgworkordergrnid = mfgworkordergrn.id
					inner join mfgworkordergrnitemsizes on mfgworkordergrnitemsizes.mfgworkorderitemsgrnid = Mfgworkordergrnitems.id
					inner join referencelist on referencelist.id = sizeid
					inner join alluid on alluid.uid = mfgworkordergrnitemsizes.uid
					where mfgworkordergrn.isactive=true and Mfgworkordergrnitems.isactive=true and mfgworkordergrnitemsizes.isactive = true and
					mfgworkordergrn.invoicedate>=@fromdate and mfgworkordergrn.invoicedate<@todate
					group by mfgworkorder.workorderno,mfgworkorder.wodate,mfgworkordergrn.invoicedate,
					mfgworkordergrnitemsizes.uid,referencelist.name,mfgworkordergrn.entryno";

            var param = new SortedList<string, object>();
            param.Add("designitemname", criteria.designitemname);
            param.Add("fromdate", criteria.fromdate.Date);
            param.Add("todate", criteria.todate.AddDays(1).Date);
            List<WIPTrace> results = new List<WIPTrace>();
            using (var reader = ExecuteReader(UpdateQueryForHyper(query), param))
            {
                while (reader.Read())
                {
                    WIPTrace stockItem = new WIPTrace();
                    stockItem.delivered = reader["delivered"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["delivered"]);
                    stockItem.ordered = reader["ordered"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["ordered"]);
                    stockItem.wodate = reader["wodate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["wodate"]);
                    stockItem.workorderno = reader["workorderno"] == DBNull.Value ? "" : Convert.ToString(reader["workorderno"]);
                    stockItem.uid = reader["uid"] == DBNull.Value ? "" : Convert.ToString(reader["uid"]);
                    stockItem.size = reader["size"] == DBNull.Value ? "" : Convert.ToString(reader["size"]);
                    results.Add(stockItem);
                }
            }

            return results;

        }


        internal bool ClosePendingPOitems(List<MfgReportServiceController.PlanedItem> items)
        {
            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    string query = "update ComponentPOitems set cqty=cqty+@cqty where id=@id and (qty-(rqty+cqty+@cqty))>=0";
                    var command = GetCommand(query, conn, transaction);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = 0;
                    command.Parameters.Add("cqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = 0;
                    foreach (var item in items)
                    {
                        command.Parameters["id"].Value = item.sid;
                        command.Parameters["cqty"].Value = item.pqty;
                        command.ExecuteNonQuery();
                    }
                    transaction.Commit();
                    conn.Close();
                }
                return true;
            }
            catch (Exception exp)
            {
                return false;
            }
        }


        internal bool SaveDesignwisePlan(List<MfgReportServiceController.PlanedItem> items)
        {
            try
            {
                Data.SalesorderDataService salesorderDataService = new SalesorderDataService() { Context = this.Context };
                foreach (var item in items.GroupBy(x => x.sono))
                {
                    var sos = salesorderDataService.Search(new SalesorderCreteria()
                    {
                        sono = item.Key
                    });
                    if (sos.Count == 1 && sos[0].customerid == item.First().customerid)
                    {
                        foreach (var sitem in item.GroupBy(x => x.designcode))
                        {
                            var designlist = sos[0].Salesorderitems.FindAll(x => x.designitemcode == sitem.Key);
                            foreach (var orderitem in designlist.GroupBy(x => x.sizeid))
                            {
                                var plannedqty = sitem.Sum(x => x.sid == orderitem.Key ? x.pqty : 0);
                                if (plannedqty > 0)
                                {
                                    foreach (var oitem in orderitem)
                                    {
                                        var pending = oitem.oqty - (oitem.rqty + oitem.bqty + oitem.cqty);
                                        if (pending >= plannedqty)
                                        {
                                            oitem.pqty = plannedqty;
                                        }
                                        else
                                        {
                                            oitem.pqty = pending;
                                        }
                                        plannedqty -= oitem.pqty;
                                        if (plannedqty <= 0)
                                            break;
                                    }
                                }
                            }
                        }
                        sos[0].modifiedby = Context.UserID;
                        sos[0].modifiedon = DateTimeNow;
                        salesorderDataService.UpdatePlan(sos[0]);
                    }
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, items);
                return false;
            }
            return true;
        }

        internal List<GRNTrace> SearchUIDGRN(MfgReportServiceController.StockReportCreteria item)
        {
            string query = @"with alluid
as
(
select mfgdesign.code,schemelist->>'sizegroup' sizegroup,schemelist->>'uid' uid,cast(schemelist->>'sizeid' as bigint) sizeid,
cast(schemelist->>'wsrate' as numeric) wsrate,cast(schemelist->>'dealerprice' as numeric) dealerprice,
cast(schemelist->>'salerate' as numeric) salerate,cast(schemelist->>'mrp' as numeric) mrp,
schemelist->>'code' dname,cast(schemelist->>'colourid' as bigint) colourid from mfgdesign 
LEFT JOIN LATERAL
json_array_elements (cast(mfgdesign.sizegroup->>'items' as json)) schemelist ON true
where mfgdesign.isactive=true and schemelist->>'uid' = @uid
)

					select  sum(qty) qty,mfgworkorder.workorderno,mfgworkorder.wodate,mfgworkordergrn.invoicedate,
					mfgworkordergrnitemsizes.uid,referencelist.name size,mfgworkordergrn.entryno from mfgworkordergrn
					inner join mfgworkorder on mfgworkorder.id = workorderid
					inner join Mfgworkordergrnitems on Mfgworkordergrnitems.mfgworkordergrnid = mfgworkordergrn.id
					inner join mfgworkordergrnitemsizes on mfgworkordergrnitemsizes.mfgworkorderitemsgrnid = Mfgworkordergrnitems.id
					inner join referencelist on referencelist.id = sizeid
					inner join alluid on alluid.uid = mfgworkordergrnitemsizes.uid
					where mfgworkordergrn.isactive=true and Mfgworkordergrnitems.isactive=true and mfgworkordergrnitemsizes.isactive = true and
					mfgworkordergrn.invoicedate>=@fromdate and mfgworkordergrn.invoicedate<@todate
					group by mfgworkorder.workorderno,mfgworkorder.wodate,mfgworkordergrn.invoicedate,
					mfgworkordergrnitemsizes.uid,referencelist.name,mfgworkordergrn.entryno";

            var param = new SortedList<string, object>();
            param.Add("uid", item.uid);
            param.Add("fromdate", item.fromdate.Date);
            param.Add("todate", item.todate.AddDays(1).Date);
            List<GRNTrace> results = new List<GRNTrace>();
            using (var reader = ExecuteReader(UpdateQueryForHyper(query), param))
            {
                while (reader.Read())
                {
                    GRNTrace stockItem = new GRNTrace();
                    stockItem.qty = reader["qty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["qty"]);
                    stockItem.wodate = reader["wodate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["wodate"]);
                    stockItem.invoicedate = reader["invoicedate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["invoicedate"]);
                    stockItem.workorderno = reader["workorderno"] == DBNull.Value ? "" : Convert.ToString(reader["workorderno"]);
                    stockItem.entryno = reader["entryno"] == DBNull.Value ? "" : Convert.ToString(reader["entryno"]);
                    stockItem.uid = reader["uid"] == DBNull.Value ? "" : Convert.ToString(reader["uid"]);
                    stockItem.size = reader["size"] == DBNull.Value ? "" : Convert.ToString(reader["size"]);
                    results.Add(stockItem);
                }
            }
            return results;
        }

        internal List<SalesTrace> SearchUIDSales(MfgReportServiceController.StockReportCreteria item)
        {
            string query = @"select sum(billitems.qty) qty,referencelist.name locationname,
					mfgworkordergrn.invoicedate,mfgworkordergrn.entryno,workorderno,workorderdate,
                    stock.serialno uid,bill.billno,bill.billdate,customer.name customername from bill
					left outer join customer on customer.id = bill.customerid
					inner join billitems on billitems.billid = bill.id
					inner join stock on stock.id = billitems.stockid
					left outer join lrinvoiceitems on lrinvoiceitems.id = stock.invoiceitemid
					left outer join lrinvoice on lrinvoice.id = lrinvoiceid
					left outer join lrentry on lrentry.id = lrid and lrentry.importreferencetype=1
					left outer join mfgworkordergrn on mfgworkordergrn.id = importreferenceid
					inner join salablegoods on salablegoods.stockid = stock.id
					inner join referencelist on referencelist.id = bill.locationid
                    where stock.serialno = @uid and bill.billdate>=@fromdate and bill.billdate<@todate 
                    and billitems.isactive=true and bill.isactive=true and bill.locationid=@locationid
					group by referencelist.name,mfgworkordergrn.invoicedate,mfgworkordergrn.entryno,workorderno,
					workorderdate,stock.serialno,bill.billno,bill.billdate,customer.name";
            if (item.locationid <= 0)
                query = query.Replace("and bill.locationid=@locationid", "");
            var param = new SortedList<string, object>();
            param.Add("uid", item.uid);
            param.Add("locationid", item.locationid);
            param.Add("fromdate", item.fromdate.Date);
            param.Add("todate", item.todate.AddDays(1).Date);
            List<SalesTrace> results = new List<SalesTrace>();
            using (var reader = ExecuteReader(UpdateQueryForHyper(query), param))
            {
                while (reader.Read())
                {
                    SalesTrace stockItem = new SalesTrace();
                    stockItem.qty = reader["qty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["qty"]);
                    stockItem.wodate = reader["workorderdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["workorderdate"]);
                    stockItem.billdate = reader["billdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["billdate"]);
                    stockItem.invoicedate = reader["invoicedate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["invoicedate"]);
                    stockItem.workorderno = reader["workorderno"] == DBNull.Value ? "" : Convert.ToString(reader["workorderno"]);
                    stockItem.uid = reader["uid"] == DBNull.Value ? "" : Convert.ToString(reader["uid"]);
                    stockItem.locationname = reader["locationname"] == DBNull.Value ? "" : Convert.ToString(reader["locationname"]);
                    stockItem.entryno = reader["entryno"] == DBNull.Value ? "" : Convert.ToString(reader["entryno"]);
                    stockItem.billno = reader["billno"] == DBNull.Value ? "" : Convert.ToString(reader["billno"]);
                    stockItem.customername = reader["customername"] == DBNull.Value ? "" : Convert.ToString(reader["customername"]);
                    results.Add(stockItem);
                }
            }

            return results;
        }
        internal List<StockTrace> SearchUIDStock(MfgReportServiceController.StockReportCreteria item)
        {
            string query = @"select sum(iqty-(oqty+dqty+jqty)) qty,referencelist.name locationname,
					mfgworkordergrn.invoicedate,mfgworkordergrn.entryno,workorderno,workorderdate,
                    stock.serialno uid
					from stock
					left outer join lrinvoiceitems on lrinvoiceitems.id = stock.invoiceitemid and lrinvoiceitems.isactive=true
					left outer join lrinvoice on lrinvoice.id = lrinvoiceid and lrinvoice.isactive=true
					left outer join lrentry on lrentry.id = lrid and lrentry.importreferencetype=1 and lrentry.isactive=true
					left outer join mfgworkordergrn on mfgworkordergrn.isactive=true and mfgworkordergrn.id = importreferenceid
					inner join salablegoods on salablegoods.stockid = stock.id and salablegoods.isactive=true
					inner join referencelist on referencelist.id = salablegoods.locationid
                    where stock.isactive=true and stock.serialno = @uid and locationid=@locationid and lrinvoice.invoicedate>=@fromdate and lrinvoice.invoicedate<@todate
					group by referencelist.name,mfgworkordergrn.invoicedate,mfgworkordergrn.entryno,workorderno,workorderdate,stock.serialno";

            if (Services.ConfigManager.Instance(Context.OrganizationID).Config.analyser_stock_removeaqty)
                query = query.Replace("sum(iqty-(oqty+dqty+jqty))", "sum(iqty-(oqty+dqty+jqty+aqty))");

            if (item.locationid < 0)
                query = query.Replace("and locationid=@locationid", "");

            if (item.fromdate.Year < 2000)
                query = query.Replace("and lrinvoice.invoicedate>=@fromdate and lrinvoice.invoicedate<@todate", "");

            var param = new SortedList<string, object>();
            param.Add("uid", item.uid);
            param.Add("fromdate", item.fromdate.Date);
            param.Add("todate", item.todate.AddDays(1).Date);
            param.Add("locationid", item.locationid);
            List<StockTrace> results = new List<StockTrace>();
            using (var reader = ExecuteReader(UpdateQueryForHyper(query), param))
            {
                while (reader.Read())
                {
                    StockTrace stockItem = new StockTrace();
                    stockItem.qty = reader["qty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["qty"]);
                    stockItem.wodate = reader["workorderdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["workorderdate"]);
                    stockItem.invoicedate = reader["invoicedate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["invoicedate"]);
                    stockItem.workorderno = reader["workorderno"] == DBNull.Value ? "" : Convert.ToString(reader["workorderno"]);
                    stockItem.uid = reader["uid"] == DBNull.Value ? "" : Convert.ToString(reader["uid"]);
                    stockItem.locationname = reader["locationname"] == DBNull.Value ? "" : Convert.ToString(reader["locationname"]);
                    stockItem.entryno = reader["entryno"] == DBNull.Value ? "" : Convert.ToString(reader["entryno"]);
                    results.Add(stockItem);
                }
            }

            return results;
        }

        internal List<WIPTrace> SearchUIDWorkorder(MfgReportServiceController.StockReportCreteria item)
        {
            string query = @"with alluid
                    as
                    (
                    select mfgdesign.code,schemelist->>'sizegroup' sizegroup,schemelist->>'uid' uid,cast(schemelist->>'sizeid' as bigint) sizeid,
                    cast(schemelist->>'wsrate' as numeric) wsrate,cast(schemelist->>'dealerprice' as numeric) dealerprice,
                    cast(schemelist->>'salerate' as numeric) salerate,cast(schemelist->>'mrp' as numeric) mrp,
                    schemelist->>'code' dname,cast(schemelist->>'colourid' as bigint) colourid from mfgdesign 
                    LEFT JOIN LATERAL
                    json_array_elements (cast(mfgdesign.sizegroup->>'items' as json)) schemelist ON true
                    where mfgdesign.isactive=true and schemelist->>'uid' = @uid
                    )

                    select sum(qty) ordered,sum(delivered) delivered,mfgworkorder.workorderno,mfgworkorder.wodate,
                    Mfgworkorderitemsizes.uid,referencelist.name size,Mfgworkorderitems.designitemname from mfgworkorder 
                    inner join Mfgworkorderitemsizes on Mfgworkorderitemsizes.mfgworkorderid = mfgworkorder.id
                    inner join Mfgworkorderitems on Mfgworkorderitems.id = Mfgworkorderitemsizes.mfgworkorderitemsid
					inner join referencelist on referencelist.id = sizeid
					inner join alluid on alluid.uid = Mfgworkorderitemsizes.uid
                    where Mfgworkorderitems.isactive=true and Mfgworkorderitemsizes.isactive = true
                    and mfgworkorder.isactive=true and mfgworkorder.wodate>=@fromdate and mfgworkorder.wodate<@todate
                    group by Mfgworkorderitems.designitemname,Mfgworkorderitemsizes.uid,referencelist.name,mfgworkorder.workorderno,mfgworkorder.wodate
					order by mfgworkorder.wodate,mfgworkorder.workorderno,Mfgworkorderitemsizes.uid";

            var param = new SortedList<string, object>();
            param.Add("uid", item.uid);
            param.Add("fromdate", item.fromdate.Date);
            param.Add("todate", item.todate.AddDays(1).Date);
            List<WIPTrace> results = new List<WIPTrace>();
            using (var reader = ExecuteReader(UpdateQueryForHyper(query), param))
            {
                while (reader.Read())
                {
                    WIPTrace stockItem = new WIPTrace();
                    stockItem.delivered = reader["delivered"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["delivered"]);
                    stockItem.ordered = reader["ordered"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["ordered"]);
                    stockItem.wodate = reader["wodate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["wodate"]);
                    stockItem.workorderno = reader["workorderno"] == DBNull.Value ? "" : Convert.ToString(reader["workorderno"]);
                    stockItem.uid = reader["uid"] == DBNull.Value ? "" : Convert.ToString(reader["uid"]);
                    stockItem.size = reader["size"] == DBNull.Value ? "" : Convert.ToString(reader["size"]);
                    results.Add(stockItem);
                }
            }

            return results;
        }

        public List<WIPTrace> WorkOrderTrace(MfgReportServiceController.WorkOrdreTraceCriteria criteria)
        {
            string query = @"with alluid
                    as
                    (
                    select mfgdesign.code,schemelist->>'sizegroup' sizegroup,schemelist->>'uid' uid,cast(schemelist->>'sizeid' as bigint) sizeid,
                    cast(schemelist->>'wsrate' as numeric) wsrate,cast(schemelist->>'dealerprice' as numeric) dealerprice,
                    cast(schemelist->>'salerate' as numeric) salerate,cast(schemelist->>'mrp' as numeric) mrp,
                    schemelist->>'code' dname,cast(schemelist->>'colourid' as bigint) colourid from mfgdesign 
                    LEFT JOIN LATERAL
                    json_array_elements (cast(mfgdesign.sizegroup->>'items' as json)) schemelist ON true
                    where mfgdesign.isactive=true and schemelist->>'code' = @designitemname
                    )

                    select sum(qty) ordered,sum(delivered) delivered,mfgworkorder.workorderno,mfgworkorder.wodate,
                    Mfgworkorderitemsizes.uid,referencelist.name size,Mfgworkorderitems.designitemname from mfgworkorder 
                    inner join Mfgworkorderitemsizes on Mfgworkorderitemsizes.mfgworkorderid = mfgworkorder.id
                    inner join Mfgworkorderitems on Mfgworkorderitems.id = Mfgworkorderitemsizes.mfgworkorderitemsid
					inner join referencelist on referencelist.id = sizeid
					inner join alluid on alluid.uid = Mfgworkorderitemsizes.uid
                    where Mfgworkorderitems.isactive=true and Mfgworkorderitemsizes.isactive = true
                    and mfgworkorder.isactive=true and mfgworkorder.wodate>=@fromdate and mfgworkorder.wodate<@todate
                    group by Mfgworkorderitems.designitemname,Mfgworkorderitemsizes.uid,referencelist.name,mfgworkorder.workorderno,mfgworkorder.wodate
					order by mfgworkorder.workorderno,Mfgworkorderitemsizes.uid";

            var param = new SortedList<string, object>();
            param.Add("designitemname", criteria.designitemname);
            param.Add("fromdate", criteria.fromdate.Date);
            param.Add("todate", criteria.todate.AddDays(1).Date);
            List<WIPTrace> results = new List<WIPTrace>();
            using (var reader = ExecuteReader(UpdateQueryForHyper(query), param))
            {
                while (reader.Read())
                {
                    WIPTrace stockItem = new WIPTrace();
                    stockItem.delivered = reader["delivered"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["delivered"]);
                    stockItem.ordered = reader["ordered"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["ordered"]);
                    stockItem.wodate = reader["wodate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["wodate"]);
                    stockItem.workorderno = reader["workorderno"] == DBNull.Value ? "" : Convert.ToString(reader["workorderno"]);
                    stockItem.uid = reader["uid"] == DBNull.Value ? "" : Convert.ToString(reader["uid"]);
                    stockItem.size = reader["designid"] == DBNull.Value ? "" : Convert.ToString(reader["designid"]);
                    results.Add(stockItem);
                }
            }

            return results;

        }



        public MfgReportServiceController.StockLocationReport
            SearchLocationwiseStock(MfgReportServiceController.StockReportCreteria item)
        {
            string query = @"select items.sizeid,colourid,stock.serialno serialno,stock.designid,
            sum(salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty)) stockqty,salablegoods.locationid from salablegoods
            inner join stock on stock.id = stockid
            inner join items on items.id = stock.itemid
            where salablegoods.hasstock=true and salablegoods.isactive=true and stock.isactive=true {0}
            group by items.sizeid,colourid,stock.designid,stock.serialno,salablegoods.locationid";

            if (Services.ConfigManager.Instance(Context.OrganizationID).Config.analyser_stock_removeaqty)
                query = query.Replace("sum(salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty))", "sum(salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty+salablegoods.aqty))");

            List<string> conditions = new List<string>();
            List<string> designlist = new List<string>();
            if (!string.IsNullOrEmpty(item.designid))
            {
                if (item.designid.IndexOf(',') > 0)
                {
                    designlist.AddRange(item.designid.Split(',').ToList());
                    conditions.Add(" stock.designid in (" + String.Join(",", item.designid.Split(',').ToList().ConvertAll(e => "'" + WebAPI.Data.Utility.SafeSQL(e) + "'")) + ") ");
                }
                else
                {
                    conditions.Add(" stock.designid = '" + WebAPI.Data.Utility.SafeSQL(item.designid) + "' ");
                }
            }
            if (item.companyid > 0)
                conditions.Add(" salablegoods.companyid =@companyid ");
            if (item.locations != null && item.locations.Count > 0)
                conditions.Add(" salablegoods.locationid in (" + string.Join(",", item.locations) + ") ");

            query = string.Format(query, (conditions.Count > 0 ? " and " : "") + string.Join(" and ", conditions));
            MfgReportServiceController.StockLocationReport report = new MfgReportServiceController.StockLocationReport();
            MfgdesignDataService mfgdesignDataService = new MfgdesignDataService() { Context = this.Context };

            string code = item.designid;
            if (code.IndexOf('/') > 0)
                code = code.Substring(code.IndexOf('/'));


            var designs = mfgdesignDataService.Search(new MfgdesignCreteria()
            {
                includedetail = true,
                name = code,
                codes = designlist.Count == 0 ? null : designlist.ConvertAll(x => "'" + x.Split('/')[0] + "'")
            });
            if (designs.Count == 0) return new MfgReportServiceController.StockLocationReport() { items = new List<MfgReportServiceController.StockLocationReport.LocationStock>() };
            ReferencelistDataService referencelistDataService = new ReferencelistDataService() { Context = this.Context };
            var sizelist = referencelistDataService.SearchList(new ReferencelistCreteria() { type = "SIZE" });
            var colourlist = referencelistDataService.SearchList(new ReferencelistCreteria() { type = "COLOUR" });


            if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_possizelist != null &&
                InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_possizelist != "")
            {
                if (item.sizegroupid > 0 && !string.IsNullOrEmpty(item.possizelist))
                {
                    report.sizenames = new List<string>(item.possizelist.Split(','));
                }
                else
                {
                    report.sizenames = new List<string>(InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_possizelist.Split(','));
                }
            }
            else
            {
                var fsize = sizelist.FindAll(x => designs[0].sizegroup.items.Exists(y => y.sizeid == x.id)).ConvertAll(x => x.name);
                report.sizenames = WebAPI.Data.Utility.GetSizeList(fsize.ConvertAll(e => e.ToUpper()));
            }
            report.sizenames.RemoveAll(x => !sizelist.Exists(y => y.name == x));
            report.sizenames = Utility.SortSizes(report.sizenames);
            report.sizeids = report.sizenames.ConvertAll(x => sizelist.Find(y => y.name == x).id);
            var param = new SortedList<string, object>();
            param.Add("companyid", item.companyid);
            param.Add("locationid", item.locationid);
            param.Add("designid", string.IsNullOrEmpty(item.designid) ? "" : item.designid);

            List<LocationStockItem> stocklist = new List<LocationStockItem>();
            using (var reader = ExecuteReader(UpdateQueryForHyper(query), param))
            {
                while (reader.Read())
                {

                    LocationStockItem stockItem = new LocationStockItem();
                    stockItem.locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                    stockItem.colourid = reader["colourid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["colourid"]);
                    stockItem.sizeid = reader["sizeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sizeid"]);
                    stockItem.stock = reader["stockqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["stockqty"]);
                    stockItem.serialno = reader["serialno"] == DBNull.Value ? "" : Convert.ToString(reader["serialno"]);
                    stockItem.designid = reader["designid"] == DBNull.Value ? "" : Convert.ToString(reader["designid"]);
                    stocklist.Add(stockItem);
                }
            }
            //var allsizeid = stocklist.GroupBy(x => x.sizeid).ToList().ConvertAll(e => e.Key);
            //var missing = allsizeid.FindAll(x=> !report.sizeids.Contains(x));
            stocklist.ForEach(x =>
            {
                if (designs.Exists(y => y.name == x.designid))
                {
                    var design = designs.Find(y => y.name == x.designid);
                    if (design.sizegroup.items.Exists(y => y.uid == x.serialno))
                    {
                        x.designcode = design.sizegroup.items.Find(y => y.uid == x.serialno).code;
                        x.colourid = design.sizegroup.items.Find(y => y.uid == x.serialno).colourid;
                    }
                }
            });

            stocklist = stocklist.GroupBy(x => new { x.locationid, x.designcode, x.colourid, x.designid, x.serialno, x.sizeid }).ToList().ConvertAll(x => new LocationStockItem()
            {
                colourid = x.Key.colourid,
                locationid = x.Key.locationid,
                designcode = x.Key.designcode,
                designid = x.Key.designid,
                serialno = x.Key.serialno,
                sizeid = x.Key.sizeid,
                stock = x.Sum(y => y.stock)
            });


            report.items = new List<MfgReportServiceController.StockLocationReport.LocationStock>();
            report.so = new List<MfgReportServiceController.StockLocationReport.DesignStock>();
            report.wip = new List<MfgReportServiceController.StockLocationReport.DesignStock>();
            foreach (var gitem in stocklist.GroupBy(x => new { x.locationid, x.designid, x.designcode, x.colourid }))
            {
                MfgReportServiceController.StockLocationReport.DesignStock designStock = new MfgReportServiceController.StockLocationReport.DesignStock();
                designStock.colour = colourlist.Exists(x => x.id == gitem.Key.colourid) ? colourlist.Find(x => x.id == gitem.Key.colourid).name : "";
                if (designs.Exists(x => x.name == gitem.Key.designid))
                {
                    var design = designs.Find(x => x.name == gitem.Key.designid);
                    designStock.brandid = design.brandid;
                    designStock.productid = design.productid;
                    designStock.fabricid = design.fabricid;
                    designStock.designid = gitem.Key.designid;
                    designStock.name = design.sizegroup.items.Exists(x => x.uid == gitem.First().serialno) ?
                        design.sizegroup.items.Find(x => x.uid == gitem.First().serialno).code : "";
                    designStock.barcode = gitem.First().serialno.Split('-')[0];
                    designStock.stock = new decimal[report.sizeids.Count];
                    foreach (var sitem in gitem)
                    {
                        if (report.sizeids.IndexOf(sitem.sizeid) >= 0)
                            designStock.stock[report.sizeids.IndexOf(sitem.sizeid)] += sitem.stock;
                        else
                        {
                            var uiditem = design.sizegroup.items.Find(x => x.uid == sitem.serialno);
                        }
                    }
                    var existindex = report.items.FindIndex(y => y.id == gitem.Key.locationid);
                    if (existindex < 0)
                    {
                        report.items.Add(new MfgReportServiceController.StockLocationReport.LocationStock() { id = gitem.Key.locationid, items = new List<MfgReportServiceController.StockLocationReport.DesignStock>() });
                        existindex = report.items.FindIndex(y => y.id == gitem.Key.locationid);
                    }
                    report.items[existindex].items.Add(designStock);
                }
            }


            if (item.ShowSO)
            {
                query = @"select sum(oqty-(bqty+cqty+rqty)) stockqty,colourid,uid,sizeid,designid 
                from salesorderitems 
				inner join salesorder on salesorder.id =salesorderid
				where salesorderitems.isactive=true and salesorder.isactive=true  and oqty-(bqty+cqty+rqty)>0 {0}
                group by uid,sizeid,colourid,designid";


                conditions = new List<string>();
                designlist = new List<string>();
                conditions.Add(" salesorder.status <2 ");

                if (!string.IsNullOrEmpty(item.designid))
                {
                    if (item.designid.IndexOf(',') > 0)
                    {
                        designlist.AddRange(item.designid.Split(',').ToList());
                        conditions.Add(" salesorderitems.designid in (" + String.Join(",", item.designid.Split(',').ToList().ConvertAll(e => "'" + WebAPI.Data.Utility.SafeSQL(e) + "'")) + ") ");
                    }
                    else
                    {
                        conditions.Add(" salesorderitems.designid = '" + WebAPI.Data.Utility.SafeSQL(item.designid) + "' ");
                    }
                }
                if (item.companyid > 0)
                    conditions.Add(" salesorder.companyid =@companyid ");
                //if (item.locationid > 0)
                //    conditions.Add(" salesorder.deliverylocationid =@locationid ");
                query = string.Format(query, conditions.Count > 0 ? (" and " + string.Join(" and ", conditions)) : "");

                report.so = new List<MfgReportServiceController.StockLocationReport.DesignStock>();
                var sostocklist = new List<StockItem>();
                using (var reader = ExecuteReader(UpdateQueryForHyper(query), param))
                {
                    while (reader.Read())
                    {
                        StockItem stockItem = new StockItem();
                        stockItem.colourid = reader["colourid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["colourid"]);
                        stockItem.sizeid = reader["sizeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sizeid"]);
                        stockItem.stock = reader["stockqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["stockqty"]);
                        stockItem.serialno = reader["uid"] == DBNull.Value ? "" : Convert.ToString(reader["uid"]);
                        stockItem.designid = reader["designid"] == DBNull.Value ? "" : Convert.ToString(reader["designid"]);
                        if (designs.Exists(x => x.name == stockItem.designid))
                        {
                            var design = designs.Find(x => x.name == stockItem.designid);
                            if (design.sizegroup.items.Exists(x => x.uid == stockItem.serialno))
                            {
                                stockItem.name = design.sizegroup.items.Find(x => x.uid == stockItem.serialno).code;
                            }
                        }
                        sostocklist.Add(stockItem);
                    }
                }

                foreach (var gitem in sostocklist.GroupBy(x => new { x.colourid, x.designid, x.name }))
                {
                    MfgReportServiceController.StockLocationReport.DesignStock designStock = new MfgReportServiceController.StockLocationReport.DesignStock();
                    designStock.colour = colourlist.Exists(x => x.id == gitem.Key.colourid) ? colourlist.Find(x => x.id == gitem.Key.colourid).name : "";

                    if (designs.Exists(x => x.name == gitem.Key.designid))
                    {

                        var design = designs.Find(x => x.name == gitem.Key.designid);
                        designStock.name = design.sizegroup.items.Exists(x => x.uid == gitem.First().serialno) ?
                            design.sizegroup.items.Find(x => x.uid == gitem.First().serialno).code : "";

                        if (report.so.Exists(x => x.name == designStock.name))
                        {
                            designStock = report.so.Find(x => x.name == designStock.name);
                            designStock.stock = new decimal[report.sizeids.Count];
                        }
                        else
                        {
                            designStock.brandid = design.brandid;
                            designStock.productid = design.productid;
                            designStock.fabricid = design.fabricid;
                            designStock.designid = gitem.Key.designid;
                            designStock.barcode = gitem.First().serialno.Split('-')[0];
                            designStock.stock = new decimal[report.sizeids.Count];
                            report.so.Add(designStock);
                        }
                        foreach (var sitem in gitem)
                        {
                            if (report.sizeids.IndexOf(sitem.sizeid) >= 0)
                                designStock.stock[report.sizeids.IndexOf(sitem.sizeid)] = sitem.stock;
                        }
                    }
                }


            }
            if (item.ShowWIP)
            {
                string desingcondition = "";
                if (designs.Count == 1)
                    desingcondition = " and Mfgworkorderitems.designname = @designid";
                else if (designs.Count > 1)
                    desingcondition = " and Mfgworkorderitems.designid in (" + string.Join(",", designs.ConvertAll(x => x.id)) + ")";
                query = string.Format(@"select sum(qty-delivered) stockqty,uid,sizeid,Mfgworkorderitems.designid from Mfgworkorderitemsizes
                    inner join mfgworkorderitems on mfgworkorderitems.id = Mfgworkorderitemsizes.mfgworkorderitemsid
                    inner join mfgworkorder on mfgworkorder.id = mfgworkorderitems.mfgworkorderid
                    where Mfgworkorderitemsizes.isactive = true and mfgworkorderitems.isactive = true
                    and mfgworkorder.isactive = true and mfgworkorder.statusid != 9
                    {0}
                    group by Mfgworkorderitems.designid,uid,sizeid", desingcondition);
                var wipstocklist = new List<StockItem>();
                using (var reader = ExecuteReader(UpdateQueryForHyper(query), param))
                {
                    while (reader.Read())
                    {
                        StockItem stockItem = new StockItem();
                        stockItem.stock = reader["stockqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["stockqty"]);
                        stockItem.sizeid = reader["sizeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sizeid"]);
                        stockItem.serialno = reader["uid"] == DBNull.Value ? "" : Convert.ToString(reader["uid"]);
                        long designid = reader["designid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["designid"]);
                        if (designs.Exists(x => x.id == designid))
                        {
                            var design = designs.Find(x => x.id == designid);
                            stockItem.designid = design.name;
                            if (design.sizegroup.items.Exists(x => x.uid == stockItem.serialno))
                            {
                                stockItem.name = design.sizegroup.items.Find(x => x.uid == stockItem.serialno).code;
                                stockItem.colourid = design.sizegroup.items.Find(x => x.uid == stockItem.serialno).colourid;
                                wipstocklist.Add(stockItem);
                            }

                        }
                    }
                }
                foreach (var gitem in wipstocklist.GroupBy(x => new { x.designid, x.name, x.colourid }))
                {

                    MfgReportServiceController.StockLocationReport.DesignStock designStock = new MfgReportServiceController.StockLocationReport.DesignStock();
                    designStock.colour = colourlist.Exists(x => x.id == gitem.Key.colourid) ? colourlist.Find(x => x.id == gitem.Key.colourid).name : "";
                    designStock.name = gitem.Key.name;
                    designStock.designid = gitem.Key.designid;
                    if (report.wip.Exists(x => x.name == designStock.name))
                    {
                        designStock = report.wip.Find(x => x.name == designStock.name);

                    }
                    else
                    {
                        var design = designs.Find(x => x.name == gitem.Key.designid);
                        designStock.brandid = design.brandid;
                        designStock.productid = design.productid;
                        designStock.fabricid = design.fabricid;
                        designStock.designid = gitem.Key.designid;
                        designStock.barcode = gitem.First().serialno.Split('-')[0];
                        designStock.stock = new decimal[report.sizeids.Count];
                        report.wip.Add(designStock);
                    }
                    foreach (var sitem in gitem)
                    {
                        if (report.sizeids.IndexOf(sitem.sizeid) >= 0)
                            designStock.stock[report.sizeids.IndexOf(sitem.sizeid)] = sitem.stock;
                    }
                }
            }
            report.creategroup();
            return report;
        }


        public MfgGallery SearchInGallery(MfgGalleryCreteria creteria)
        {
            MfgGallery gallery = new MfgGallery();
            ReferencelistDataService referencelistDataService = new ReferencelistDataService() { Context = this.Context };
            if (!creteria.onlyassorted && creteria.assortedzero)
                creteria.onlyassorted = true;

            if (creteria.sizegroupid > 0)
            {
                var sizegroups = referencelistDataService.Search(new ReferencelistCreteria()
                {
                    id = creteria.sizegroupid
                });
                if (sizegroups.Count > 0)
                {
                    var sizelist = sizegroups[0].additionalinfo.sizegroups;
                    var allsizes = referencelistDataService.Search(new ReferencelistCreteria()
                    {
                        ids = sizelist
                    });
                    gallery.sizelist = new List<ReferenceIdName>();
                    foreach (var si in sizelist)
                    {
                        var size = allsizes.Find(x => x.id == si);
                        if (size != null)
                        {
                            gallery.sizelist.Add(new ReferenceIdName()
                            {
                                id = size.id,
                                name = size.name
                            });
                        }
                    }
                }
            }
            if (creteria.assortedsizegroupid > 0)
            {
                var sizegroups = referencelistDataService.Search(new ReferencelistCreteria()
                {
                    id = creteria.assortedsizegroupid
                });
                if (sizegroups.Count > 0)
                {
                    var sizelist = sizegroups[0].additionalinfo.sizegroups;
                    var allsizes = referencelistDataService.Search(new ReferencelistCreteria()
                    {
                        ids = sizelist
                    });
                    gallery.assortedsizes = new List<ReferenceIdName>();
                    foreach (var si in sizelist)
                    {
                        var size = allsizes.Find(x => x.id == si);
                        if (size != null)
                        {
                            gallery.assortedsizes.Add(new ReferenceIdName()
                            {
                                id = size.id,
                                name = size.name
                            });
                        }
                    }
                }
            }
            if (creteria.sizeid > 0)
            {
                var sizegroups = referencelistDataService.Search(new ReferencelistCreteria()
                {
                    id = creteria.sizeid
                });
                if (sizegroups.Count > 0)
                {
                    gallery.sizelist = new List<ReferenceIdName>();

                    gallery.sizelist.Add(new ReferenceIdName()
                    {
                        id = sizegroups[0].id,
                        name = sizegroups[0].name
                    });
                }
            }
            string query = @"with alluid
as
(
select mfgdesign.code,schemelist->>'sizegroup' sizegroup,schemelist->>'uid' uid,cast(schemelist->>'sizeid' as bigint) sizeid,
cast(schemelist->>'wsrate' as numeric) wsrate,cast(schemelist->>'dealerprice' as numeric) dealerprice,
cast(schemelist->>'salerate' as numeric) salerate,cast(schemelist->>'mrp' as numeric) mrp,
schemelist->>'code' dname,cast(schemelist->>'colourid' as bigint) colourid,mfgdesign.imagelist,
colourlist->>'imageid' colourimage from mfgdesign 
LEFT JOIN LATERAL
json_array_elements (cast(mfgdesign.sizegroup->>'items' as json)) schemelist ON true
LEFT JOIN LATERAL
json_array_elements (cast(mfgdesign.sizegroup->>'colouritems' as json)) colourlist ON colourlist->>'colourid' = schemelist->>'colourid'

where mfgdesign.isactive=true {0}
)

select uid,code,dname,colourid,sizeid,colourimage,imagelist,wsrate,dealerprice,salerate,mrp from alluid";


            List<string> conditions = new List<string>();
            if (creteria.colourid > 0)
                conditions.Add(" mfgdesign.colourid =@colourid ");
            if (creteria.productid > 0)
                conditions.Add(" mfgdesign.productid =@productid ");
            if (creteria.brandid > 0)
                conditions.Add(" mfgdesign.brandid =@brandid ");
            if (creteria.materialid > 0)
                conditions.Add(" mfgdesign.materialid =@materialid ");
            if (creteria.seasonid > 0)
                conditions.Add(" mfgdesign.seasonid =@seasonid ");
            if (creteria.classid > 0)
                conditions.Add(" mfgdesign.classid =@classid ");
            if (creteria.styleid > 0)
                conditions.Add(" mfgdesign.styleid =@styleid ");
            if (creteria.typeid > 0)
                conditions.Add(" mfgdesign.typeid =@typeid ");
            if (creteria.fitid > 0)
                conditions.Add(" mfgdesign.fitid =@fitid ");
            if (creteria.sizeid > 0)
                conditions.Add(" cast(schemelist->>'sizeid' as bigint)=@sizeid ");
            if (!creteria.onlyassorted && creteria.sizegroupid > 0 &&
                gallery.sizelist.Count > 0)
            {
                conditions.Add(" cast(schemelist->>'sizeid' as bigint) in (" +
                    String.Join(",", gallery.sizelist.ConvertAll(x => x.id)) + ")");
            }

            if (creteria.salesmanid > 0)
            {
                conditions.Add("mfgdesign.code in (" +
                    @"select designcode from mfg_samples
                    where isactive=true and salesmenid = @salesmanid
                    group by designcode
                    having sum(qty) > 0
                    union all
                    select mfgdesign.code designcode from Mfg_Swatches_History 
                    inner join mfg_swatches_folder on mfg_swatches_folder.id=Mfg_Swatches_History.folderid 
                    left outer join mfgdesign on mfgdesign.id = Mfg_Swatches_Folder.designid
                    where Mfg_Swatches_History.isactive=true and Mfg_Swatches_History.referenceid=@salesmanid
                    group by mfgdesign.code,Mfg_Swatches_History.referenceid
                    having sum(Mfg_Swatches_History.qty)> 0)");
            }

            if (creteria.salesorderid > 0)
            {
                conditions.Add(@"mfgdesign.code in (select distinct designid from salesorderitems 
                    where salesorderitems.isactive=true 
                    and salesorderid=" + creteria.salesorderid + ")");
            }

            if (!string.IsNullOrEmpty(creteria.designid))
            {
                if (creteria.designid.IndexOf('%') >= 0)
                    conditions.Add(" mfgdesign.code ilike @designid ");
                else
                    conditions.Add(" mfgdesign.code = @designid ");
            }
            query = string.Format(query, (conditions.Count > 0) ? (" and " + string.Join(" AND ", conditions)) : "");

            var param = new SortedList<string, object>();
            param.Add("colourid", creteria.colourid);
            param.Add("salesmanid", creteria.salesmanid);
            param.Add("productid", creteria.productid);
            param.Add("brandid", creteria.brandid);
            param.Add("materialid", creteria.materialid);
            param.Add("classid", creteria.classid);
            param.Add("seasonid", creteria.seasonid);
            param.Add("styleid", creteria.styleid);
            param.Add("typeid", creteria.typeid);
            param.Add("sizeid", creteria.sizeid);
            param.Add("fitid", creteria.fitid);
            param.Add("locationid", creteria.locationid);
            param.Add("designid", string.IsNullOrEmpty(creteria.designid) ? "" : creteria.designid);
            List<StockItem> stocklist = new List<StockItem>();
            List<long> sizeids = new List<long>();


            SortedDictionary<string, MfgGalleryItem> uids = new SortedDictionary<string, MfgGalleryItem>();
            SortedDictionary<string, MfgGalleryItem> response = new SortedDictionary<string, MfgGalleryItem>();

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            using (var reader = ExecuteReader(UpdateQueryForHyper(query), param, 10000))
            {
                while (reader.Read())
                {
                    MfgGalleryItem mfgGalleryItem = null;
                    string dname = reader["dname"].ToString();
                    string code = reader["code"].ToString();
                    if (!response.ContainsKey(dname))
                    {
                        mfgGalleryItem = new MfgGalleryItem() { designitemcode = dname, code = code };
                        mfgGalleryItem.colourid = reader["colourid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["colourid"]);

                        if (reader["colourimage"] != DBNull.Value)
                        {
                            mfgGalleryItem.imgid = reader["colourimage"].ToString();
                        }

                        if (reader["imagelist"] != DBNull.Value &&
                            reader["imagelist"].ToString() != "" && reader["imagelist"].ToString() != "null")
                        {
                            var imagelist = Newtonsoft.Json.JsonConvert.DeserializeObject<List<string>>(reader["imagelist"].ToString());
                            if (imagelist != null && imagelist.Count > 0)
                            {
                                mfgGalleryItem.mimgid = imagelist[0];
                                if (string.IsNullOrEmpty(mfgGalleryItem.imgid))
                                    mfgGalleryItem.imgid = imagelist[0];
                            }
                        }
                        mfgGalleryItem.sizes = new List<MfgGalleryItem.SizeStock>();
                        mfgGalleryItem.stksizes = new List<MfgGalleryItem.SizeStockDetail>();
                        response.Add(dname, mfgGalleryItem);
                    }
                    else
                    {
                        mfgGalleryItem = response[dname];
                    }
                    long sizeid = reader["sizeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sizeid"]);


                    mfgGalleryItem.sizes.Add(new MfgGalleryItem.SizeStock()
                    {
                        id = sizeid,
                        uid = reader["uid"] == DBNull.Value ? "" : reader["uid"].ToString()
                    });

                    mfgGalleryItem.stksizes.Add(new MfgGalleryItem.SizeStockDetail()
                    {
                        id = sizeid,
                        uid = reader["uid"] == DBNull.Value ? "" : reader["uid"].ToString(),
                        rate = reader["dealerprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["dealerprice"]),
                        wsrate = reader["wsrate"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["wsrate"]),
                        salerate = reader["salerate"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["salerate"]),
                        mrp = reader["mrp"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["mrp"]),
                    });

                    if (!sizeids.Contains(sizeid))
                        sizeids.Add(sizeid);

                    if (!uids.ContainsKey(reader["uid"].ToString()))
                        uids.Add(reader["uid"].ToString(), mfgGalleryItem);
                }
            }
            stopwatch.Stop();
            if (uids.Count == 0) return new MfgGallery();

            if (gallery.sizelist == null || gallery.sizelist.Count == 0)
            {
                gallery.sizelist = new List<ReferenceIdName>();
                var allsize = referencelistDataService.SearchList(new ReferencelistCreteria() { type = "SIZE" });
                allsize.RemoveAll(x => !sizeids.Contains(x.id));
                allsize.ForEach(x =>
                {
                    if (sizeids.Contains(x.id))
                    {
                        gallery.sizelist.Add(new ReferenceIdName() { id = x.id, name = x.name });
                    }
                });
                List<string> sizelist = new List<string>();
                string system_possizelist = InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_possizelist;

                if (creteria.sizegroupid > 0 && !string.IsNullOrEmpty(creteria.possizelist))
                {
                    system_possizelist = creteria.possizelist;
                }

                if (!string.IsNullOrEmpty(system_possizelist))
                {
                    sizelist.AddRange(system_possizelist.Split(','));
                }
                else
                {
                    sizelist = WebAPI.Data.Utility.GetSizeList(gallery.sizelist.ConvertAll(e => e.name.ToUpper()));
                }
                if (sizelist != null && sizelist.Count > 0)
                {
                    gallery.sizelist.Sort(delegate (ReferenceIdName a, ReferenceIdName b)
                    {
                        var aindex = sizelist.IndexOf(a.name);
                        var bindex = sizelist.IndexOf(b.name);
                        return aindex.CompareTo(bindex);
                    });
                }
            }

            var responselist = response.Values.ToList();
            if (creteria.onlyimage)
            {
                responselist.ForEach(x =>
                {
                    if (string.IsNullOrEmpty(x.imgid) &&
                        x.sizes != null && x.sizes.Count > 0)
                    {
                        x.sizes.ForEach(y =>
                        {
                            if (uids.ContainsKey(y.uid))
                                uids.Remove(y.uid);
                        });
                    }
                });
                responselist.RemoveAll(X => string.IsNullOrEmpty(X.imgid));
            }

            List<long> locationids = new List<long>();
            if (creteria.locationid == 0)
            {
                foreach (var item in InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig)
                {
                    if (item.Value.mfg_so_retail)
                    {
                        locationids.Add(item.Key);
                    }
                }
                if (locationids.Count == 0)
                    locationids.Add(0);
                query = string.Format(@"select stock.serialno uid,
            sum(salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty)) stockqty from salablegoods
            inner join stock on stock.id = stockid
            where salablegoods.hasstock=true and salablegoods.isactive=true and stock.isactive=true and salablegoods.locationid in ({1})
            and stock.serialno in ({2})
            group by stock.serialno", (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_hyper_uan ? "barcode" : "serialno"),
                    string.Join(",", locationids), string.Join(",", uids.Keys.ToList().ConvertAll(x => "'" + x + "'")));
            }

            else
            {
                locationids.Add(creteria.locationid);
                creteria.onlywip = InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[creteria.locationid].mfg_so_wip;
                if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[creteria.locationid].mfg_so_fs)
                {
                    foreach (var lc in InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig.ToList())
                    {
                        if (lc.Value.mfg_so_retail)
                        {
                            locationids.Add(lc.Key);
                        }
                    }
                }
                query = string.Format(@"select stock.serialno uid,
            sum(salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty)) stockqty from salablegoods
            inner join stock on stock.id = stockid
            where salablegoods.hasstock=true and salablegoods.isactive=true and stock.isactive=true and salablegoods.locationid in ({1})
            and stock.serialno in ({2})
            group by stock.serialno", (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_hyper_uan ? "barcode" : "serialno"),
                string.Join(",", locationids), string.Join(",", uids.Keys.ToList().ConvertAll(x => "'" + x + "'")));
            }

            if (Services.ConfigManager.Instance(Context.OrganizationID).Config.analyser_stock_removeaqty)
                query = query.Replace("sum(salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty))", "sum(salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty+salablegoods.aqty))");

            if (creteria.valuesource != 2)
            {
                if (uids.Count > 0)
                {
                    using (var reader = ExecuteReader(UpdateQueryForHyper(query), param))
                    {
                        while (reader.Read())
                        {
                            var uid = reader["uid"].ToString();
                            if (uids.ContainsKey(uid))
                            {
                                var exist = uids[uid].sizes.Find(x => x.uid == uid);
                                if (exist != null)
                                {
                                    exist.stock += reader["stockqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["stockqty"]);
                                    uids[uid].stksizes.Find(x => x.uid == uid).stk += reader["stockqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["stockqty"]);
                                }
                            }
                        }
                    }
                }
            }

            if (creteria.valuesource != 1 && creteria.salesorderid == 0)
            {
                //        if (Services.ConfigManager.Instance(Context.OrganizationID).Config.intray_salesorder)
                //        {
                //            query = @"select sum(oqty-(bqty+cqty+rqty)) stockqty,uid from salesorderitems 
                //inner join salesorder on salesorder.id =salesorderid
                //where salesorderitems.isactive=true and salesorder.isactive=true 
                //            and oqty-(bqty+cqty+rqty)>0 and salesorder.status = 1
                //            group by uid";
                //        }
                //        else
                {
                    query = string.Format(@"select sum(oqty-(bqty+cqty+rqty)) stockqty,uid from salesorder
				    inner join salesorderitems on hascompleted=false and salesorderid = salesorder.id and salesorderitems.isactive=true
				    where salesorder.isactive=true and salesorder.status < 2
                    and salesorder.deliverylocationid in ({0})
                    and uid in (" + string.Join(",", uids.Keys.ToList().ConvertAll(x => "'" + x + "'")) + @") and oqty-(bqty+cqty+rqty)>0
                    group by uid", string.Join(",", locationids));
                }
                using (var reader = ExecuteReader(UpdateQueryForHyper(query), param, 1000))
                {
                    while (reader.Read())
                    {
                        var uid = reader["uid"].ToString();
                        if (uids.ContainsKey(uid))
                        {
                            var exist = uids[uid].sizes.Find(x => x.uid == uid);
                            if (exist != null)
                            {
                                uids[uid].stksizes.Find(x => x.uid == uid).so += reader["stockqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["stockqty"]);

                                exist.so += reader["stockqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["stockqty"]);
                                if (creteria.valuesource == 2)
                                    exist.stock += reader["stockqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["stockqty"]);
                                else
                                    exist.stock -= reader["stockqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["stockqty"]);
                            }
                        }
                    }
                }
            }

            if (creteria.valuesource < 2)
                responselist.ForEach(x => x.sizes.ForEach(y => y.stock = y.stock < 0 ? 0 : y.stock));


            if (creteria.onlywip && uids.Count > 0)
            {

                string desingcondition = " and Mfgworkorderitemsizes.uid in (" + string.Join(",", uids.Keys.ToList().ConvertAll(x => "'" + x + "'")) + ")";
                query = string.Format(@"select sum(qty-delivered) stockqty,uid from Mfgworkorderitemsizes
                    inner join mfgworkorderitems on mfgworkorderitems.id = Mfgworkorderitemsizes.mfgworkorderitemsid
                    inner join mfgworkorder on mfgworkorder.id = mfgworkorderitems.mfgworkorderid
                    where Mfgworkorderitemsizes.isactive = true and mfgworkorderitems.isactive = true
                    and mfgworkorder.isactive = true and mfgworkorder.statusid != 9 and qty-delivered>0
                    {0}
                    group by uid", desingcondition);
                stocklist = new List<StockItem>();
                using (var reader = ExecuteReader(UpdateQueryForHyper(query), param, 1000))
                {
                    while (reader.Read())
                    {
                        var uid = reader["uid"].ToString();
                        if (uids.ContainsKey(uid))
                        {
                            var exist = uids[uid].sizes.Find(x => x.uid == uid);
                            if (exist != null)
                            {
                                exist.wip += reader["stockqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["stockqty"]);
                                uids[uid].stksizes.Find(x => x.uid == uid).wip += reader["stockqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["stockqty"]);
                            }
                        }
                    }
                }
            }

            if (creteria.salesorderid == 0)
            {
                if (creteria.onlyso)
                {
                    responselist.RemoveAll(X => X.sizes == null || X.sizes.Count == 0 || (X.sizes.All(y => y.wip <= 0 && y.stock <= 0 && y.so <= 0)));
                }
                else
                {
                    if (creteria.onlystock && !creteria.onlywip)
                    {
                        responselist.RemoveAll(X => X.sizes == null || X.sizes.Count == 0 || X.sizes.All(y => y.stock <= 0));
                    }
                    if (!creteria.onlystock && creteria.onlywip)
                    {
                        responselist.RemoveAll(X => X.sizes == null || X.sizes.Count == 0 || X.sizes.All(y => y.wip <= 0));
                    }
                    if (creteria.onlystock && creteria.onlywip)
                    {
                        responselist.RemoveAll(X => X.sizes == null || X.sizes.Count == 0 || (X.sizes.All(y => y.wip <= 0 && y.stock <= 0)));
                    }
                    if (creteria.onlyassorted)
                    {
                        var assartedsizes = gallery.assortedsizes.ConvertAll(x => x.id);
                        var sizegroupsizes = gallery.sizelist.ConvertAll(x => x.id);

                        var newlist = new List<MfgGalleryItem>();
                        if (creteria.assortedzero)
                        {
                            responselist.ForEach(x =>
                            {
                                bool added = false;
                                if (x.sizes != null && x.sizes.Count > 0)
                                {
                                    x.sizes.RemoveAll(y => !sizegroupsizes.Contains(y.id));

                                    var assortedstocks = x.sizes.FindAll(y => y.stock > 0 && assartedsizes.Contains(y.id));
                                    var sizegroupstocks = x.sizes.FindAll(y => !assartedsizes.Contains(y.id) &&
                                    sizegroupsizes.Contains(y.id));
                                    if (assortedstocks.Count == 0 || sizegroupstocks.Count == 0) return;
                                    if (assortedstocks.Count != assartedsizes.Count ||
                                    assortedstocks.Any(y => y.stock <= 0)) return;
                                    if (sizegroupstocks.Any(y => y.stock > 0)) return;
                                    added = true;
                                    newlist.Add(x);
                                }

                                if (!added && x.stksizes != null && x.stksizes.Count > 0)
                                {
                                    x.stksizes.RemoveAll(y => !sizegroupsizes.Contains(y.id));

                                    var assortedstocks = x.stksizes.FindAll(y => y.stk > 0 && assartedsizes.Contains(y.id));
                                    var sizegroupstocks = x.stksizes.FindAll(y => !assartedsizes.Contains(y.id) &&
                                    sizegroupsizes.Contains(y.id));
                                    if (assortedstocks.Count == 0 || sizegroupstocks.Count == 0) return;
                                    if (assortedstocks.Count != assartedsizes.Count ||
                                    assortedstocks.Any(y => y.stk <= 0)) return;
                                    if (sizegroupstocks.Any(y => y.stk > 0)) return;
                                    newlist.Add(x);
                                }
                            });
                        }
                        else
                        {
                            responselist.ForEach(x =>
                            {
                                bool added = false;
                                if (x.sizes != null && x.sizes.Count > 0)
                                {
                                    x.sizes.RemoveAll(y => !sizegroupsizes.Contains(y.id));
                                    var assortedstocks = x.sizes.FindAll(y => y.stock > 0 && assartedsizes.Contains(y.id));
                                    var sizegroupstocks = x.sizes.FindAll(y => !assartedsizes.Contains(y.id) &&
                                    sizegroupsizes.Contains(y.id));
                                    if (assortedstocks.Count == 0 || sizegroupstocks.Count == 0) return;
                                    if (assortedstocks.Min(y => y.stock) >
                                    sizegroupstocks.Max(y => y.stock))
                                    {
                                        added = true;
                                        newlist.Add(x);
                                    }
                                }
                                if (!added && x.stksizes != null && x.stksizes.Count > 0)
                                {
                                    x.sizes.RemoveAll(y => !sizegroupsizes.Contains(y.id));
                                    var assortedstocks = x.stksizes.FindAll(y => y.stk > 0 && assartedsizes.Contains(y.id));
                                    var sizegroupstocks = x.stksizes.FindAll(y => !assartedsizes.Contains(y.id) &&
                                    sizegroupsizes.Contains(y.id));
                                    if (assortedstocks.Count == 0 || sizegroupstocks.Count == 0) return;
                                    if (assortedstocks.Min(y => y.stk) >
                                    sizegroupstocks.Max(y => y.stk))
                                    {
                                        newlist.Add(x);
                                    }
                                }
                            });
                        }
                        responselist = newlist;
                    }
                }
            }

            gallery.items = responselist;
            gallery.colourlist = referencelistDataService.SearchList(new ReferencelistCreteria() { type = "COLOUR" }).ConvertAll(X => new ReferenceIdName() { id = X.id, name = X.name });
            var availablecolours = responselist.ConvertAll(x => x.colourid).Distinct().ToList();
            gallery.colourlist.RemoveAll(x => !availablecolours.Contains(x.id));
            gallery.colourgroup = responselist.GroupBy(x => x.code).Select(x => new MfgGalleryColourGroup()
            {
                code = x.Key,
                designs = x.ToList().ConvertAll(y => y.designitemcode).Distinct().ToList(),
            }).ToList();

            if (creteria.salesorderid > 0)
            {
                var salesorder = new SalesorderDataService() { Context = this.Context };
                var so = salesorder.Search(new SalesorderCreteria() { id = creteria.salesorderid });
                if (so != null &&
                    so.Count > 0)
                {
                    gallery.salesorder = so[0];
                    responselist = responselist.FindAll(x => gallery.salesorder.Salesorderitems.Exists(y => y.designitemcode == x.designitemcode));
                    gallery.items = responselist;
                }
            }
            return gallery;
        }

        public string UpdateQueryForHyper(string query)
        {
            if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_hyper_uan)
                return query.Replace("stock.serialno", "stock.barcode");
            return query;
        }

        public MfgReportServiceController.PlanningReport
            SearchDesignwiseStock4Planner(MfgReportServiceController.StockReportCreteria item)
        {
            string query = @"select items.sizeid,colourid,stock.serialno serialno,stock.designid,
            sum(salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty)) stockqty from salablegoods
            inner join stock on stock.id = stockid
            inner join items on items.id = stock.itemid
            where salablegoods.hasstock=true and salablegoods.isactive=true and stock.isactive=true {0}
            group by items.sizeid,colourid,stock.designid,stock.serialno";
            if (Services.ConfigManager.Instance(Context.OrganizationID).Config.analyser_stock_removeaqty)
                query = query.Replace("sum(salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty))", "sum(salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty+salablegoods.aqty))");
            List<string> conditions = new List<string>();
            List<string> designlist = new List<string>();

            if(item.sectionid>0)
            {
                query.Replace("inner join items on items.id = stock.itemid",
                    @"inner join items on items.id = stock.itemid
                      inner join products on products.id = items.productid and products.salescounterid=" + item.sectionid);
            }

            if (item.brandid > 0)
            {
                query.Replace("inner join items on items.id = stock.itemid",
                    @"inner join items on items.id = stock.itemid and items.brandid = " + item.brandid);
            }

            if (!string.IsNullOrEmpty(item.designid))
            {
                if (item.designid.IndexOf(',') > 0)
                {
                    designlist.AddRange(item.designid.Split(',').ToList());
                    conditions.Add(" stock.designid in (" + String.Join(",", item.designid.Split(',').ToList().ConvertAll(e => "'" + WebAPI.Data.Utility.SafeSQL(e) + "'")) + ") ");
                }
                else
                {
                    conditions.Add(" stock.designid = '" + WebAPI.Data.Utility.SafeSQL(item.designid) + "' ");
                }
            }
            if (item.companyid > 0)
                conditions.Add(" salablegoods.companyid =@companyid ");
            if (item.locationid > 0)
            {
                List<long> stocklocation = new List<long>();
                if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[item.locationid].mfg_so_fs)
                {
                    foreach (var lc in InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig.ToList())
                    {
                        if (lc.Value.mfg_so_retail)
                        {
                            stocklocation.Add(lc.Key);
                        }
                    }
                }
                if (stocklocation.Count == 0)
                {
                    conditions.Add(" salablegoods.locationid =@locationid ");
                }
                else
                {
                    conditions.Add(" salablegoods.locationid in (" + String.Join(",", stocklocation) + ")");
                }
            }
            query = string.Format(query, (conditions.Count > 0 ? " and " : "") + string.Join(" and ", conditions));
            MfgReportServiceController.PlanningReport report = new MfgReportServiceController.PlanningReport()
            {
                stock = new List<MfgReportServiceController.PlanningReport.DesignStock>(),
                salesorder = new List<MfgReportServiceController.PlanningReport.SalesOrder>()
            };

            MfgdesignDataService mfgdesignDataService = new MfgdesignDataService() { Context = this.Context };

            string code = item.designid;
            if (code.IndexOf('/') > 0)
                code = code.Substring(code.IndexOf('/'));

            var codes = designlist.Count == 0 ? null : designlist.ConvertAll(x => "'" + x.Split('/')[0] + "'");

            var param = new SortedList<string, object>();
            param.Add("companyid", item.companyid);
            param.Add("locationid", item.locationid);
            param.Add("customerid", item.customerid);
            param.Add("fromdate", item.fromdate.Date);
            if (item.todate.Date != DateTime.MinValue)
                param.Add("todate", item.todate.Date.AddDays(-1));
            param.Add("designid", string.IsNullOrEmpty(item.designid) ? "" : item.designid);


            //        if (string.IsNullOrEmpty(code) &&
            //           (codes == null || codes.Count == 0))
            //        {
            //            var designlistquery = @"select distinct designid
            //            from salesorderitems 
            //inner join salesorder on salesorder.id = salesorderid
            //            inner join customer on customer.id = salesorder.customerid
            //where salesorderitems.isactive=true and salesorder.isactive=true and oqty-(bqty+cqty+rqty)>0 {0}";

            //            conditions = new List<string>();
            //            designlist = new List<string>();
            //            codes = new List<string>();
            //            if (Services.ConfigManager.Instance(Context.OrganizationID).Config.intray_salesorder)
            //            {
            //                conditions.Add(" salesorder.status = 1");
            //            }
            //            else
            //            {
            //                conditions.Add(" salesorder.status <2 ");
            //            }

            //            if (item.companyid > 0)
            //                conditions.Add(" salesorder.companyid =@companyid ");
            //            if (item.customerid > 0)
            //                conditions.Add(" salesorder.customerid =@customerid ");
            //            if (item.fromdate.Date != DateTime.MinValue)
            //                conditions.Add(" salesorder.sodate >=@fromdate");
            //            if (item.todate.Date != DateTime.MinValue)
            //                conditions.Add(" salesorder.sodate <@todate");

            //            designlistquery = string.Format(designlistquery, conditions.Count > 0 ? (" and " + string.Join(" and ", conditions)) : "");
            //            using (var reader = ExecuteReader(UpdateQueryForHyper(designlistquery), param))
            //            {
            //                while (reader.Read())
            //                {
            //                    if (!reader.IsDBNull(0))
            //                        codes.Add(reader.GetString(0));
            //                }
            //            }
            //        }

            var designs = mfgdesignDataService.Search(new MfgdesignCreteria()
            {
                includedetail = true,
                name = code,
                codes = codes,
                sectionid = item.sectionid,
                brandid =item.brandid
            });

            if (designs.Count == 0) return new MfgReportServiceController.PlanningReport() { stock = new List<MfgReportServiceController.PlanningReport.DesignStock>() };
            ReferencelistDataService referencelistDataService = new ReferencelistDataService() { Context = this.Context };
            var sizelist = referencelistDataService.SearchList(new ReferencelistCreteria() { type = "SIZE" });
            var colourlist = referencelistDataService.SearchList(new ReferencelistCreteria() { type = "COLOUR" });


            if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_possizelist != null &&
                InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_possizelist != "")
            {
                if (item.sizegroupid > 0 && !string.IsNullOrEmpty(item.possizelist))
                {
                    report.sizenames = new List<string>(item.possizelist.Split(','));
                }
                else
                {
                    report.sizenames = new List<string>(InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_possizelist.Split(','));
                }
            }
            else
            {
                var fsize = sizelist.FindAll(x => designs[0].sizegroup.items.Exists(y => y.sizeid == x.id)).ConvertAll(x => x.name);
                report.sizenames = WebAPI.Data.Utility.GetSizeList(fsize.ConvertAll(e => e.ToUpper()));
            }

            report.sizenames.RemoveAll(x => !sizelist.Exists(y => y.name == x));
            report.sizenames = Utility.SortSizes(report.sizenames);
            report.sizeids = report.sizenames.ConvertAll(x => sizelist.Find(y => y.name == x).id);
            List<StockItem> stocklist = new List<StockItem>();
            using (var reader = ExecuteReader(UpdateQueryForHyper(query), param))
            {
                while (reader.Read())
                {
                    StockItem stockItem = new StockItem();
                    stockItem.colourid = reader["colourid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["colourid"]);
                    stockItem.sizeid = reader["sizeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sizeid"]);
                    stockItem.stock = reader["stockqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["stockqty"]);
                    stockItem.serialno = reader["serialno"] == DBNull.Value ? "" : Convert.ToString(reader["serialno"]);
                    stockItem.designid = reader["designid"] == DBNull.Value ? "" : Convert.ToString(reader["designid"]);
                    stocklist.Add(stockItem);
                }
            }
            //var allsizeid = stocklist.GroupBy(x => x.sizeid).ToList().ConvertAll(e => e.Key);
            //var missing = allsizeid.FindAll(x=> !report.sizeids.Contains(x));
            stocklist.ForEach(x =>
            {
                if (designs.Exists(y => y.name == x.designid))
                {
                    var design = designs.Find(y => y.name == x.designid);
                    if (design.sizegroup.items.Exists(y => y.uid == x.serialno))
                    {
                        x.designcode = design.sizegroup.items.Find(y => y.uid == x.serialno).code;
                        x.colourid = design.sizegroup.items.Find(y => y.uid == x.serialno).colourid;
                    }
                    else
                    {
                        x.designcode = design.code;
                    }
                }
            });

            stocklist = stocklist.GroupBy(x => new { x.designcode, x.colourid, x.designid, x.serialno, x.sizeid }).ToList().ConvertAll(x => new StockItem()
            {
                colourid = x.Key.colourid,
                designcode = x.Key.designcode,
                designid = x.Key.designid,
                serialno = x.Key.serialno,
                sizeid = x.Key.sizeid,
                stock = x.Sum(y => y.stock)
            });

            foreach (var gitem in stocklist.GroupBy(x => new { x.designid, x.designcode, x.colourid }))
            {
                MfgReportServiceController.PlanningReport.DesignStock designStock = new MfgReportServiceController.PlanningReport.DesignStock();
                if (designs.Exists(x => x.name == gitem.Key.designid))
                {
                    var design = designs.Find(x => x.name == gitem.Key.designid);
                    designStock.designid = gitem.Key.designid;
                    designStock.name = design.sizegroup.items.Exists(x => x.uid == gitem.First().serialno) ?
                        design.sizegroup.items.Find(x => x.uid == gitem.First().serialno).code : "";
                    designStock.sk = new decimal[report.sizeids.Count];
                    foreach (var sitem in gitem)
                    {
                        if (report.sizeids.IndexOf(sitem.sizeid) >= 0)
                            designStock.sk[report.sizeids.IndexOf(sitem.sizeid)] += sitem.stock;
                        else
                        {
                            var uiditem = design.sizegroup.items.Find(x => x.uid == sitem.serialno);
                        }
                    }
                    report.stock.Add(designStock);
                }
            }

            query = @"select sum(oqty-(bqty+cqty+rqty)) stockqty,sum(pqty) planqty,colourid,uid,sizeid,designid,
                designitemcode designcode,customerid,customer.cityid,salesorder.sono
                from salesorderitems 
				inner join salesorder on salesorder.id = salesorderid
                inner join customer on customer.id = salesorder.customerid
				where salesorderitems.isactive=true and salesorder.isactive=true and oqty-(bqty+cqty+rqty)>0 {0}
                group by uid,sizeid,colourid,designid,designitemcode,customerid,cityid,salesorder.sono";

            conditions = new List<string>();
            designlist = new List<string>();

            if (Services.ConfigManager.Instance(Context.OrganizationID).Config.intray_salesorder)
            {
                conditions.Add(" salesorder.status = 1");
            }
            else
            {
                conditions.Add(" salesorder.status <2 ");
            }
            if (!string.IsNullOrEmpty(item.designid))
            {
                if (item.designid.IndexOf(',') > 0)
                {
                    designlist.AddRange(item.designid.Split(',').ToList());
                    conditions.Add(" salesorderitems.designid in (" + String.Join(",", item.designid.Split(',').ToList().ConvertAll(e => "'" + WebAPI.Data.Utility.SafeSQL(e) + "'")) + ") ");
                }
                else
                {
                    conditions.Add(" salesorderitems.designid = '" + WebAPI.Data.Utility.SafeSQL(item.designid) + "' ");
                }
            }
            if (item.companyid > 0)
                conditions.Add(" salesorder.companyid =@companyid ");
            //if (item.locationid > 0)
            //    conditions.Add(" salesorder.deliverylocationid =@locationid ");
            if (item.customerid > 0)
                conditions.Add(" salesorder.customerid =@customerid ");

            if (item.fromdate.Date != DateTime.MinValue)
                conditions.Add(" salesorder.sodate >=@fromdate");
            if (item.todate.Date != DateTime.MinValue)
                conditions.Add(" salesorder.sodate <@todate");

            query = string.Format(query, conditions.Count > 0 ? (" and " + string.Join(" and ", conditions)) : "");

            var solist = new List<StockItem>();
            using (var reader = ExecuteReader(UpdateQueryForHyper(query), param))
            {
                while (reader.Read())
                {
                    StockItem stockItem = new StockItem();
                    stockItem.colourid = reader["colourid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["colourid"]);
                    stockItem.customerid = reader["customerid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["customerid"]);
                    stockItem.cityid = reader["cityid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["cityid"]);
                    stockItem.sizeid = reader["sizeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sizeid"]);
                    stockItem.stock = reader["stockqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["stockqty"]);
                    stockItem.pqty = reader["planqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["planqty"]);
                    stockItem.serialno = reader["uid"] == DBNull.Value ? "" : Convert.ToString(reader["uid"]);
                    stockItem.designid = reader["designid"] == DBNull.Value ? "" : Convert.ToString(reader["designid"]);
                    stockItem.sono = reader["sono"] == DBNull.Value ? "" : Convert.ToString(reader["sono"]);
                    stockItem.name = reader["designcode"] == DBNull.Value ? "" : Convert.ToString(reader["designcode"]);



                    if (designs.Exists(x => x.name == stockItem.designid))
                    {
                        var design = designs.Find(x => x.name == stockItem.designid);
                        if (design.sizegroup != null && design.sizegroup.items.Exists(x => x.uid == stockItem.serialno))
                        {
                            stockItem.name = design.sizegroup.items.Find(x => x.uid == stockItem.serialno).code;
                        }
                    }
                    solist.Add(stockItem);
                }
            }

            foreach (var gitem in solist.GroupBy(x => new { x.customerid, x.sono, x.cityid, x.designid, x.name }))
            {
                MfgReportServiceController.PlanningReport.SalesOrder designStock = new MfgReportServiceController.PlanningReport.SalesOrder();
                designStock.customerid = gitem.Key.customerid;
                designStock.cityid = gitem.Key.cityid;
                designStock.designcode = gitem.Key.name;
                designStock.sono = gitem.Key.sono;

                var design = designs.Find(x => x.code == gitem.Key.designid);
                if (design != null && design.sizegroup != null)
                {
                    var uiditem = design.sizegroup.items.Find(x => x.code == designStock.designcode);
                    if (uiditem != null)
                    {
                        designStock.colourid = uiditem.colourid;
                        designStock.colour = colourlist.Exists(x => x.id == uiditem.colourid) ? colourlist.Find(x => x.id == uiditem.colourid).name : "";
                        designStock.brandid = design.brandid;
                        designStock.productid = design.productid;
                        designStock.fabricid = design.fabricid;
                        designStock.barcode = gitem.First().serialno.Split('-')[0];

                        designStock.so = new decimal[report.sizeids.Count];
                        if (gitem.Any(x => x.pqty > 0))
                            designStock.pn = new decimal[report.sizeids.Count];
                        foreach (var sitem in gitem)
                        {
                            if (report.sizeids.IndexOf(sitem.sizeid) >= 0)
                            {
                                designStock.so[report.sizeids.IndexOf(sitem.sizeid)] = sitem.stock;
                                if (sitem.pqty > 0)
                                    designStock.pn[report.sizeids.IndexOf(sitem.sizeid)] = sitem.stock < sitem.pqty ? sitem.stock : sitem.pqty;
                            }
                        }
                        report.salesorder.Add(designStock);
                    }
                }
            }


            if (item.ShowWIP)
            {
                string desingcondition = "";
                if (designs.Count == 1)
                    desingcondition = " and Mfgworkorderitems.designname = @designid";
                else if (designs.Count > 1)
                    desingcondition = " and Mfgworkorderitems.designid in (" + string.Join(",", designs.ConvertAll(x => x.id)) + ")";
                query = string.Format(@"select sum(qty-delivered) stockqty,uid,sizeid,Mfgworkorderitems.designid from Mfgworkorderitemsizes
                    inner join mfgworkorderitems on mfgworkorderitems.id = Mfgworkorderitemsizes.mfgworkorderitemsid
                    inner join mfgworkorder on mfgworkorder.id = mfgworkorderitems.mfgworkorderid
                    where Mfgworkorderitemsizes.isactive = true and mfgworkorderitems.isactive = true
                    and mfgworkorder.isactive = true and mfgworkorder.statusid != 9
                    {0}
                    group by Mfgworkorderitems.designid,uid,sizeid", desingcondition);
                stocklist = new List<StockItem>();
                using (var reader = ExecuteReader(UpdateQueryForHyper(query), param))
                {
                    while (reader.Read())
                    {
                        StockItem stockItem = new StockItem();
                        stockItem.stock = reader["stockqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["stockqty"]);
                        stockItem.sizeid = reader["sizeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sizeid"]);
                        stockItem.serialno = reader["uid"] == DBNull.Value ? "" : Convert.ToString(reader["uid"]);
                        long designid = reader["designid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["designid"]);
                        if (designs.Exists(x => x.id == designid))
                        {
                            var design = designs.Find(x => x.id == designid);
                            if (design != null && design.sizegroup != null)
                            {
                                stockItem.designid = design.name;
                                if (design.sizegroup.items.Exists(x => x.uid == stockItem.serialno))
                                {
                                    stockItem.name = design.sizegroup.items.Find(x => x.uid == stockItem.serialno).code;
                                    stockItem.colourid = design.sizegroup.items.Find(x => x.uid == stockItem.serialno).colourid;
                                    stocklist.Add(stockItem);
                                }
                            }
                        }
                    }
                }

                foreach (var gitem in stocklist.GroupBy(x => new { x.designid, x.name, x.colourid }))
                {

                    MfgReportServiceController.PlanningReport.DesignStock designStock = new MfgReportServiceController.PlanningReport.DesignStock();
                    designStock.name = gitem.Key.name;
                    designStock.designid = gitem.Key.designid;
                    if (report.stock.Exists(x => x.name == designStock.name))
                    {
                        designStock = report.stock.Find(x => x.name == designStock.name);
                        designStock.wip = new decimal[report.sizeids.Count];
                    }
                    else
                    {
                        var design = designs.Find(x => x.name == gitem.Key.designid);
                        designStock.designid = gitem.Key.designid;
                        designStock.wip = new decimal[report.sizeids.Count];
                        designStock.sk = new decimal[report.sizeids.Count];
                        report.stock.Add(designStock);
                    }
                    foreach (var sitem in gitem)
                    {
                        if (report.sizeids.IndexOf(sitem.sizeid) >= 0)
                            designStock.wip[report.sizeids.IndexOf(sitem.sizeid)] = sitem.stock;
                    }
                }
            }
            //report.items.RemoveAll(x => x.so == null || x.so.All(y => y == 0));
            return report;
        }

        public class MFG_Dashboard
        {
            public class Salesorder
            {
                public decimal ordered { get; set; }
                public decimal delivered { get; set; }
                public decimal cancelled { get; set; }
                public string month { get; set; }
            }

            public class Salesordermonth
            {
                public decimal bqty { get; set; }
                public string billmonth { get; set; }
                public string somonth { get; set; }

            }

            public class Salesordercls
            {
                public long classid { get; set; }
                public string monthname { get; set; }
                public decimal ordered { get; set; }
            }

            public class Salesorderclass
            {
                public string name { get; set; }
                public decimal ordered { get; set; }
                public decimal pending { get; set; }
                public string monthname { get; set; }
            }

            public class MfgExtraStock
            {
                public string code { get; set; }
                public decimal extrastock { get; set; }
            }

            public class Stockshortage
            {
                public string code { get; set; }
                public decimal pendingso { get; set; }
                public decimal pendingqty { get; set; }
                public decimal wipqty { get; set; }
            }


        }
        public void DashBoard()
        {
            /*
select sum(salesorderitems.oqty) ordered,
sum(salesorderitems.bqty) delivered,sum(salesorderitems.cqty) cancelled,
case when sodate>=@currentmonthstart then 'CURRENT' else 'LAST' end monthname
from salesorder 
inner join salesorderitems on salesorderitems.salesorderid = salesorder.id and salesorderitems.isactive=true
and salesorderitems.isactive=true and salesorder.isactive=true and salesorder.status<3
where sodate>=@lastmonthstart
group by case when sodate>=@currentmonthstart then 'CURRENT' else 'LAST' end

SELECT sum(billitems.qty) bqty,
case when billdate<@currentmonthstart then 'LAST' else 'CURRENT' end billmonth,
case when sodate<@currentmonthstart then 'LAST' else 'CURRENT' end somonth
FROM BILL
inner join salesorder on salesorder.sono = billattributes->>'orderrefid'
inner join billitems on billitems.billid = bill.id
where bill.isactive=true and billitems.isactive=true and bill.billtype != 8
and BIlLDATE>=@lastmonthstart 
group by
case when billdate<@currentmonthstart then 'LAST' else 'CURRENT' end,
case when sodate<@currentmonthstart then 'LAST' else 'CURRENT' end 

with mfgsocls as
(
select mfgdesign.classid,
case when sodate<@currentmonthstart then 'LAST' else 'CURRENT' end monthname,
sum(salesorderitems.oqty) ordered  from salesorder 
inner join salesorderitems on salesorderitems.salesorderid = salesorder.id and salesorderitems.isactive=true
inner join mfgdesign on mfgdesign.name = salesorderitems.designid
and salesorderitems.isactive=true
where sodate>=@lastmonthstart
group by mfgdesign.classid,
case when sodate<@currentmonthstart then 'LAST' else 'CURRENT' end
),mfgsopnd as 
(select mfgdesign.classid,
case when sodate<@currentmonthstart then 'LAST' else 'CURRENT' end monthname,
sum(salesorderitems.oqty-(salesorderitems.bqty+salesorderitems.rqty+salesorderitems.cqty)) pending  from salesorder 
inner join salesorderitems on salesorderitems.salesorderid = salesorder.id and salesorderitems.isactive=true
inner join mfgdesign on mfgdesign.name = salesorderitems.designid
and salesorderitems.isactive=true
where salesorder.isactive=true and 
salesorderitems.oqty-(salesorderitems.bqty+salesorderitems.rqty+salesorderitems.cqty)>0
group by  mfgdesign.classid,case when sodate<@currentmonthstart then 'LAST' else 'CURRENT' end 
)

select name,mfgsocls.ordered,mfgsocls.monthname
from referencelist 
left outer join mfgsocls on mfgsocls.classid = referencelist.id
where type='CLASSIFICATION'
and isactive=true

with mfgsocls as
(
select mfgdesign.classid,
case when sodate<@currentmonthstart then 'LAST' else 'CURRENT' end monthname,
sum(salesorderitems.oqty) ordered  from salesorder 
inner join salesorderitems on salesorderitems.salesorderid = salesorder.id and salesorderitems.isactive=true
inner join mfgdesign on mfgdesign.name = salesorderitems.designid
and salesorderitems.isactive=true
where sodate>=@lastmonthstart
group by mfgdesign.classid,
case when sodate<@currentmonthstart then 'LAST' else 'CURRENT' end
),mfgsopnd as 
(select mfgdesign.classid,
case when sodate<@currentmonthstart then 'LAST' else 'CURRENT' end monthname,
sum(salesorderitems.oqty-(salesorderitems.bqty+salesorderitems.rqty+salesorderitems.cqty)) pending  from salesorder 
inner join salesorderitems on salesorderitems.salesorderid = salesorder.id and salesorderitems.isactive=true
inner join mfgdesign on mfgdesign.name = salesorderitems.designid
and salesorderitems.isactive=true
where salesorder.isactive=true and 
salesorderitems.oqty-(salesorderitems.bqty+salesorderitems.rqty+salesorderitems.cqty)>0
group by  mfgdesign.classid,case when sodate<@currentmonthstart then 'LAST' else 'CURRENT' end 
)

select name,mfgsopnd.pending,mfgsopnd.monthname
from referen
            celist 
left outer join mfgsopnd on mfgsopnd.classid = referencelist.id
where type='CLASSIFICATION'
and isactive=true

with allpending as 
(
select salesorderitems.uid,sum(salesorderitems.oqty-(salesorderitems.bqty+salesorderitems.rqty+salesorderitems.cqty)) pendingsoqty  from salesorder 
inner join salesorderitems on salesorderitems.salesorderid = salesorder.id and salesorderitems.isactive=true
and salesorderitems.isactive=true
where salesorder.isactive=true and 
salesorderitems.oqty-(salesorderitems.bqty+salesorderitems.rqty+salesorderitems.cqty)>0
group by  salesorderitems.uid
),stock
as
(
select stock.serialno,sum(salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty)) pendingstockqty from salablegoods
inner join stock on stock.id = salablegoods.stockid
where salablegoods.isactive = true and salablegoods.hasstock =true and 
salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty)>0
group by stock.serialno
),
alluid
as
(
select mfgdesign.code,schemelist->>'uid' uid from mfgdesign 
LEFT JOIN LATERAL
json_array_elements (cast(mfgdesign.sizegroup->>'items' as json)) schemelist ON true
where mfgdesign.isactive=true
)
select alluid.code,sum(pendingstockqty-pendingsoqty) extrastock from stock
inner join alluid on alluid.uid = stock.serialno
left outer join allpending on allpending.uid = stock.serialno
where pendingstockqty>pendingsoqty
group by alluid.code
order by sum(pendingstockqty-pendingsoqty) desc 
limit 20


with allpending as 
(
select salesorderitems.uid,sum(salesorderitems.oqty-(salesorderitems.bqty+salesorderitems.rqty+salesorderitems.cqty)) pendingsoqty  from salesorder 
inner join salesorderitems on salesorderitems.salesorderid = salesorder.id and salesorderitems.isactive=true
and salesorderitems.isactive=true
where salesorder.isactive=true and 
salesorderitems.oqty-(salesorderitems.bqty+salesorderitems.rqty+salesorderitems.cqty)>0
group by  salesorderitems.uid
),stock
as
(
select stock.serialno,sum(salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty)) pendingstockqty from salablegoods
inner join stock on stock.id = salablegoods.stockid
where salablegoods.isactive = true and salablegoods.hasstock =true and 
salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty)>0
group by stock.serialno
),
alluid
as
(
select mfgdesign.code,schemelist->>'uid' uid from mfgdesign 
LEFT JOIN LATERAL
json_array_elements (cast(mfgdesign.sizegroup->>'items' as json)) schemelist ON true
where mfgdesign.isactive=true
),
wip as
(
select sum(qty-delivered) stockqty,Mfgworkorderitems.designname
from Mfgworkorderitemsizes
                    inner join mfgworkorderitems on mfgworkorderitems.id = Mfgworkorderitemsizes.mfgworkorderitemsid
                    inner join mfgworkorder on mfgworkorder.id = mfgworkorderitems.mfgworkorderid
                    where Mfgworkorderitemsizes.isactive = true and mfgworkorderitems.isactive = true
                    and mfgworkorder.isactive = true and mfgworkorder.statusid != 9
                    group by Mfgworkorderitems.designname
)
select alluid.code,sum(pendingsoqty) pendingso,
sum(pendingstockqty) pendingqty,
sum(wip.stockqty) wipqty from stock
inner join alluid on alluid.uid = stock.serialno
left outer join wip on wip.designname = alluid.code
left outer join allpending on allpending.uid = stock.serialno
where pendingsoqty>(pendingstockqty+wip.stockqty)
group by alluid.code
order by sum(pendingsoqty-pendingstockqty-wip.stockqty) desc 
            
					select sum(qty-delivered) wipqty,Mfgworkorderitems.designname,
					case when expectdate<@currentmonthstart then 'OLD' else 'THIS' end monthname
					from Mfgworkorderitemsizes
                    inner join mfgworkorderitems on mfgworkorderitems.id = Mfgworkorderitemsizes.mfgworkorderitemsid
                    inner join mfgworkorder on mfgworkorder.id = mfgworkorderitems.mfgworkorderid
                    where Mfgworkorderitemsizes.isactive = true and mfgworkorderitems.isactive = true
                    and mfgworkorder.isactive = true and mfgworkorder.statusid != 9
                    group by Mfgworkorderitems.designname,
					case when expectdate<@currentmonthstart then 'OLD' else 'THIS' end
					order by sum(qty-delivered) desc

                    select sum(receivable+rounding),billtype,case when 
                    billattributes->>'orderrefid' is null or 
                    billattributes->>'orderrefid' = '' then 'N' else 'Y' end,
                    case when billdate<@currentmonthstart then 'OLD' else 'THIS' end
                    from bill
                    where billdate>=@lastmonthstart
                    group by billtype,case when 
                    billattributes->>'orderrefid' is null or 
                    billattributes->>'orderrefid' = '' then 'N' else 'Y' end,
                    case when billdate<@currentmonthstart then 'OLD' else 'THIS' end
            
with currentstock
as
(
select Bomstocktransaction.bomid,
            sum(Bomstocktransaction.qty) bqty
			from Bomstocktransaction
            inner join  bommaster on bommaster.id = bomid
            where Bomstocktransaction.isactive = true
            group by Bomstocktransaction.bomid
			having sum(Bomstocktransaction.qty)>0
),purchasedetail
as
(
select sum(qty) pqty,bomid from Bomstocktransaction 
where transactedon>='2023-01-08' and reftype=1
group by bomid
)
select bommaster.name,bqty-pqty balance,pqty from bommaster
inner join  purchasedetail on purchasedetail.bomid = bommaster.id
inner join  currentstock on currentstock.bomid = bommaster.id
where pqty >0 and (bqty-pqty)>0             

             */
        }
        public MfgReportServiceController.StockReport
            SearchDesignwiseStock(MfgReportServiceController.StockReportCreteria item)
        {
            List<long> stocklocations = new List<long>();
            if (item.locationid > 0)
                stocklocations.Add(item.locationid);
            if (item.locationid > 0 && InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[item.locationid].mfg_so_fs)
            {
                foreach (var lc in InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig.ToList())
                {
                    if (lc.Value.mfg_so_retail)
                    {
                        stocklocations.Add(lc.Key);
                    }
                }
            }

            string query = @"select items.sizeid,colourid,stock.serialno serialno,stock.designid,
            sum(salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty)) stockqty from salablegoods
            inner join stock on stock.id = stockid
            inner join items on items.id = stock.itemid
            where salablegoods.hasstock=true and salablegoods.isactive=true and stock.isactive=true {0}
            group by items.sizeid,colourid,stock.designid,stock.serialno";
            if (Services.ConfigManager.Instance(Context.OrganizationID).Config.analyser_stock_removeaqty)
                query = query.Replace("sum(salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty))", "sum(salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty+salablegoods.aqty))");

            List<string> conditions = new List<string>();
            List<string> designlist = new List<string>();

            if (!string.IsNullOrEmpty(item.designid) && item.productid >= 0)
            {
                if (item.designid.IndexOf(',') > 0)
                {
                    designlist.AddRange(item.designid.Split(',').ToList());
                    conditions.Add(" stock.designid in (" + String.Join(",", item.designid.Split(',').ToList().ConvertAll(e => "'" + WebAPI.Data.Utility.SafeSQL(e) + "'")) + ") ");
                }
                else
                {
                    conditions.Add(" stock.designid = '" + WebAPI.Data.Utility.SafeSQL(item.designid) + "' ");
                }
            }

            if (item.productid > 0)
            {
                conditions.Add(" stock.productid = " + item.productid + " ");
            }

            if (item.companyid > 0)
                conditions.Add(" salablegoods.companyid =@companyid ");
            if (stocklocations.Count == 1)
                conditions.Add(" salablegoods.locationid = @locationid ");
            if (stocklocations.Count > 1)
                conditions.Add(" salablegoods.locationid in (" + string.Join(",", stocklocations) + ")");

            query = string.Format(query, (conditions.Count > 0 ? " and " : "") + string.Join(" and ", conditions));
            MfgReportServiceController.StockReport report = new MfgReportServiceController.StockReport();
            MfgdesignDataService mfgdesignDataService = new MfgdesignDataService() { Context = this.Context };

            string code = item.designid;
            if (code.IndexOf('/') > 0)
                code = code.Substring(code.IndexOf('/'));

            var designs = mfgdesignDataService.Search(new MfgdesignCreteria()
            {
                includedetail = true,
                productid = item.productid > 0 ? item.productid : 0,
                onlycombo = item.productid == -2,
                name = designlist.Count == 0 ? code : "",
                codes = designlist.Count == 0 ? null : designlist.ConvertAll(x => x.Split('/')[0])
            });
            if (designs.Count == 0) return new MfgReportServiceController.StockReport() { items = new List<MfgReportServiceController.StockReport.DesignStock>() };

            if (item.productid == -2)
            {
                designlist = designs.ConvertAll(x => x.name);
                if (designlist.Count > 1)
                {
                    conditions.Add(" stock.designid in (" + String.Join(",", designlist.ConvertAll(e => "'" + WebAPI.Data.Utility.SafeSQL(e) + "'")) + ") ");
                }
                else
                {
                    conditions.Add(" stock.designid = '" + WebAPI.Data.Utility.SafeSQL(designlist[0]) + "' ");
                }
            }

            ReferencelistDataService referencelistDataService = new ReferencelistDataService() { Context = this.Context };
            var sizelist = referencelistDataService.SearchList(new ReferencelistCreteria() { type = "SIZE" });
            var colourlist = referencelistDataService.SearchList(new ReferencelistCreteria() { type = "COLOUR" });
            if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_possizelist != null &&
                InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_possizelist != "")
            {
                if (item.sizegroupid > 0 && !string.IsNullOrEmpty(item.possizelist))
                {
                    report.sizenames = new List<string>(item.possizelist.Split(','));
                }
                else
                {
                    report.sizenames = new List<string>(InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_possizelist.Split(','));
                }
            }
            else
            {
                var fsize = sizelist.FindAll(x => designs[0].sizegroup.items.Exists(y => y.sizeid == x.id)).ConvertAll(x => x.name);
                report.sizenames = WebAPI.Data.Utility.GetSizeList(fsize.ConvertAll(e => e.ToUpper()));
                if (report.sizenames == null)
                    report.sizenames = fsize.ConvertAll(e => e.ToUpper());
                else
                    report.sizenames.AddRange(fsize.ConvertAll(e => e.ToUpper()).FindAll(x => !report.sizenames.Contains(x)));
            }
            report.sizenames.RemoveAll(x => !sizelist.Exists(y => y.name == x));
            report.sizenames = Utility.SortSizes(report.sizenames);
            report.sizeids = report.sizenames.ConvertAll(x => sizelist.Find(y => y.name == x).id);
            var param = new SortedList<string, object>();
            param.Add("companyid", item.companyid);
            param.Add("locationid", item.locationid);
            param.Add("designid", string.IsNullOrEmpty(item.designid) ? "" : item.designid);
            List<StockItem> stocklist = new List<StockItem>();
            using (var reader = ExecuteReader(UpdateQueryForHyper(query), param))
            {
                while (reader.Read())
                {
                    StockItem stockItem = new StockItem();
                    stockItem.colourid = reader["colourid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["colourid"]);
                    stockItem.sizeid = reader["sizeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sizeid"]);
                    stockItem.stock = reader["stockqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["stockqty"]);
                    stockItem.serialno = reader["serialno"] == DBNull.Value ? "" : Convert.ToString(reader["serialno"]);
                    stockItem.designid = reader["designid"] == DBNull.Value ? "" : Convert.ToString(reader["designid"]);
                    stocklist.Add(stockItem);
                }
            }
            //var allsizeid = stocklist.GroupBy(x => x.sizeid).ToList().ConvertAll(e => e.Key);
            //var missing = allsizeid.FindAll(x=> !report.sizeids.Contains(x));
            stocklist.ForEach(x =>
            {
                if (designs.Exists(y => y.name == x.designid))
                {
                    var design = designs.Find(y => y.name == x.designid);
                    if (design.sizegroup.items.Exists(y => y.uid == x.serialno))
                    {
                        x.designcode = design.sizegroup.items.Find(y => y.uid == x.serialno).code;
                        x.colourid = design.sizegroup.items.Find(y => y.uid == x.serialno).colourid;
                    }
                    else
                    {
                        x.designcode = design.code;
                    }
                }
            });

            stocklist = stocklist.GroupBy(x => new { x.designcode, x.colourid, x.designid, x.serialno, x.sizeid }).ToList().ConvertAll(x => new StockItem()
            {
                colourid = x.Key.colourid,
                designcode = x.Key.designcode,
                designid = x.Key.designid,
                serialno = x.Key.serialno,
                sizeid = x.Key.sizeid,
                stock = x.Sum(y => y.stock)
            });


            report.items = new List<MfgReportServiceController.StockReport.DesignStock>();
            report.erroritems = new List<MfgReportServiceController.StockReport.DesignStock>();
            foreach (var gitem in stocklist.GroupBy(x => new { x.designid, x.designcode, x.colourid }))
            {
                if (designs.Exists(x => x.name == gitem.Key.designid))
                {
                    MfgReportServiceController.StockReport.DesignStock designStock = new MfgReportServiceController.StockReport.DesignStock();
                    designStock.colourid = gitem.Key.colourid;
                    designStock.colour = colourlist.Exists(x => x.id == gitem.Key.colourid) ? colourlist.Find(x => x.id == gitem.Key.colourid).name : "";

                    var design = designs.Find(x => x.name == gitem.Key.designid);
                    designStock.brandid = design.brandid;
                    designStock.productid = design.productid;
                    designStock.racknumber = design.racknumber;
                    designStock.fabricid = design.fabricid;
                    designStock.designid = gitem.Key.designid;
                    designStock.name = design.sizegroup.items.Exists(x => x.uid == gitem.First().serialno) ?
                        design.sizegroup.items.Find(x => x.uid == gitem.First().serialno).code : "";
                    designStock.barcode = gitem.First().serialno.Split('-')[0];
                    designStock.stock = new decimal[report.sizeids.Count];
                    foreach (var sitem in gitem)
                    {
                        if (report.sizeids.IndexOf(sitem.sizeid) >= 0)
                            designStock.stock[report.sizeids.IndexOf(sitem.sizeid)] += sitem.stock;
                        else
                        {
                            var uiditem = design.sizegroup.items.Find(x => x.uid == sitem.serialno);
                        }
                    }
                    report.items.Add(designStock);
                }
                else
                {
                    MfgReportServiceController.StockReport.DesignStock designStock = new MfgReportServiceController.StockReport.DesignStock();
                    designStock.colourid = gitem.Key.colourid;
                    designStock.colour = colourlist.Exists(x => x.id == gitem.Key.colourid) ? colourlist.Find(x => x.id == gitem.Key.colourid).name : "";
                    designStock.barcode = gitem.First().serialno.Split('-')[0];
                    designStock.stock = new decimal[report.sizeids.Count];
                    foreach (var sitem in gitem)
                    {
                        if (report.sizeids.IndexOf(sitem.sizeid) >= 0)
                            designStock.stock[report.sizeids.IndexOf(sitem.sizeid)] += sitem.stock;
                    }
                    report.erroritems.Add(designStock);
                }
            }

            if (item.ShowSO)
            {
                query = @"select sum(oqty-(bqty+cqty+rqty)) stockqty,colourid,uid,sizeid,designid,designitemcode designcode
                from salesorderitems 
				inner join salesorder on salesorder.id =salesorderid
				where salesorderitems.isactive=true and salesorder.isactive=true  and oqty-(bqty+cqty+rqty)>0 {0}
                group by uid,sizeid,colourid,designid,designitemcode";

                conditions = new List<string>();
                designlist = new List<string>();

                if (item.ShowSOApproved)
                {
                    conditions.Add(" salesorder.status  = 1 ");
                }
                else
                {
                    conditions.Add(" salesorder.status <2 ");
                }
                //if (Services.ConfigManager.Instance(Context.OrganizationID).Config.intray_salesorder)
                //{
                //    conditions.Add(" salesorder.status = 1");
                //}
                //else
                //{
                //    conditions.Add(" salesorder.status <2 ");
                //}


                if (item.productid == -2)
                {
                    designlist = designs.ConvertAll(x => x.name);
                    if (designlist.Count > 1)
                    {
                        conditions.Add(" salesorderitems.designid in (" + String.Join(",", designlist.ConvertAll(e => "'" + WebAPI.Data.Utility.SafeSQL(e) + "'")) + ") ");
                    }
                    else
                    {
                        conditions.Add(" salesorderitems.designid = '" + WebAPI.Data.Utility.SafeSQL(designlist[0]) + "' ");
                    }
                }
                else
                {
                    if (item.productid > 0)
                        conditions.Add(" salesorderitems.productid =" + item.productid + " ");

                    if (!string.IsNullOrEmpty(item.designid))
                    {
                        if (item.designid.IndexOf(',') > 0)
                        {
                            designlist.AddRange(item.designid.Split(',').ToList());
                            conditions.Add(" salesorderitems.designid in (" + String.Join(",", item.designid.Split(',').ToList().ConvertAll(e => "'" + WebAPI.Data.Utility.SafeSQL(e) + "'")) + ") ");
                        }
                        else
                        {
                            conditions.Add(" salesorderitems.designid = '" + WebAPI.Data.Utility.SafeSQL(item.designid) + "' ");
                        }
                    }
                }



                if (item.companyid > 0)
                    conditions.Add(" salesorder.companyid =@companyid ");
                if (stocklocations.Count == 1)
                    conditions.Add(" salesorder.deliverylocationid = @locationid ");
                if (stocklocations.Count > 1)
                    conditions.Add(" salesorder.deliverylocationid in (" + string.Join(",", stocklocations) + ")");

                query = string.Format(query, conditions.Count > 0 ? (" and " + string.Join(" and ", conditions)) : "");

                stocklist = new List<StockItem>();
                using (var reader = ExecuteReader(UpdateQueryForHyper(query), param))
                {
                    while (reader.Read())
                    {
                        StockItem stockItem = new StockItem();
                        stockItem.colourid = reader["colourid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["colourid"]);
                        stockItem.sizeid = reader["sizeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sizeid"]);
                        stockItem.stock = reader["stockqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["stockqty"]);
                        stockItem.serialno = reader["uid"] == DBNull.Value ? "" : Convert.ToString(reader["uid"]);
                        stockItem.designid = reader["designid"] == DBNull.Value ? "" : Convert.ToString(reader["designid"]);
                        if (designs.Exists(x => x.name == stockItem.designid))
                        {
                            var design = designs.Find(x => x.name == stockItem.designid);
                            if (design.sizegroup.items.Exists(x => x.uid == stockItem.serialno))
                            {
                                stockItem.name = design.sizegroup.items.Find(x => x.uid == stockItem.serialno).code;
                            }
                        }
                        stocklist.Add(stockItem);
                    }
                }

                foreach (var gitem in stocklist.GroupBy(x => new { x.colourid, x.designid, x.name }))
                {
                    MfgReportServiceController.StockReport.DesignStock designStock = new MfgReportServiceController.StockReport.DesignStock();
                    designStock.colourid = gitem.Key.colourid;
                    designStock.colour = colourlist.Exists(x => x.id == gitem.Key.colourid) ? colourlist.Find(x => x.id == gitem.Key.colourid).name : "";

                    if (designs.Exists(x => x.name == gitem.Key.designid))
                    {

                        var design = designs.Find(x => x.name == gitem.Key.designid);
                        designStock.name = design.sizegroup.items.Exists(x => x.uid == gitem.First().serialno) ?
                            design.sizegroup.items.Find(x => x.uid == gitem.First().serialno).code : "";

                        if (report.items.Exists(x => x.name == designStock.name))
                        {
                            designStock = report.items.Find(x => x.name == designStock.name);
                            designStock.so = new decimal[report.sizeids.Count];
                        }
                        else
                        {
                            designStock.brandid = design.brandid;
                            designStock.productid = design.productid;
                            designStock.fabricid = design.fabricid;
                            designStock.designid = gitem.Key.designid;
                            designStock.barcode = gitem.First().serialno.Split('-')[0];
                            designStock.so = new decimal[report.sizeids.Count];
                            designStock.stock = new decimal[report.sizeids.Count];
                            report.items.Add(designStock);
                        }
                        foreach (var sitem in gitem)
                        {
                            if (report.sizeids.IndexOf(sitem.sizeid) >= 0)
                                designStock.so[report.sizeids.IndexOf(sitem.sizeid)] = sitem.stock;
                        }
                    }
                }


            }
            if (item.ShowWIP)
            {
                string desingcondition = "";
                if (designs.Count == 1)
                    desingcondition = " and Mfgworkorderitems.designname = @designid";
                else if (designs.Count > 1)
                    desingcondition = " and Mfgworkorderitems.designid in (" + string.Join(",", designs.ConvertAll(x => x.id)) + ")";

                if (item.productid > 0)
                {
                    desingcondition += " and Mfgworkorderitems.productid = " + item.productid + " ";
                }

                query = string.Format(@"select sum(qty-delivered) stockqty,uid,sizeid,Mfgworkorderitems.designid from Mfgworkorderitemsizes
                    inner join mfgworkorderitems on mfgworkorderitems.id = Mfgworkorderitemsizes.mfgworkorderitemsid
                    inner join mfgworkorder on mfgworkorder.id = mfgworkorderitems.mfgworkorderid
                    where Mfgworkorderitemsizes.isactive = true and mfgworkorderitems.isactive = true
                    and mfgworkorder.isactive = true and mfgworkorder.statusid != 9
                    {0}
                    group by Mfgworkorderitems.designid,uid,sizeid", desingcondition);
                stocklist = new List<StockItem>();
                using (var reader = ExecuteReader(UpdateQueryForHyper(query), param))
                {
                    while (reader.Read())
                    {
                        StockItem stockItem = new StockItem();
                        stockItem.stock = reader["stockqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["stockqty"]);
                        stockItem.sizeid = reader["sizeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sizeid"]);
                        stockItem.serialno = reader["uid"] == DBNull.Value ? "" : Convert.ToString(reader["uid"]);
                        long designid = reader["designid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["designid"]);
                        if (designs.Exists(x => x.id == designid))
                        {
                            var design = designs.Find(x => x.id == designid);
                            stockItem.designid = design.name;
                            if (design.sizegroup.items.Exists(x => x.uid == stockItem.serialno))
                            {
                                stockItem.name = design.sizegroup.items.Find(x => x.uid == stockItem.serialno).code;
                                stockItem.colourid = design.sizegroup.items.Find(x => x.uid == stockItem.serialno).colourid;
                                stocklist.Add(stockItem);
                            }

                        }
                    }
                }
                foreach (var gitem in stocklist.GroupBy(x => new { x.designid, x.name, x.colourid }))
                {

                    MfgReportServiceController.StockReport.DesignStock designStock = new MfgReportServiceController.StockReport.DesignStock();
                    designStock.colour = colourlist.Exists(x => x.id == gitem.Key.colourid) ? colourlist.Find(x => x.id == gitem.Key.colourid).name : "";
                    designStock.name = gitem.Key.name;
                    designStock.designid = gitem.Key.designid;
                    designStock.colourid = gitem.Key.colourid;
                    if (report.items.Exists(x => x.name == designStock.name))
                    {
                        designStock = report.items.Find(x => x.name == designStock.name);
                        designStock.wip = new decimal[report.sizeids.Count];
                    }
                    else
                    {
                        var design = designs.Find(x => x.name == gitem.Key.designid);
                        designStock.brandid = design.brandid;
                        designStock.productid = design.productid;
                        designStock.fabricid = design.fabricid;
                        designStock.designid = gitem.Key.designid;
                        designStock.barcode = gitem.First().serialno.Split('-')[0];
                        designStock.wip = new decimal[report.sizeids.Count];
                        designStock.so = new decimal[report.sizeids.Count];
                        designStock.stock = new decimal[report.sizeids.Count];
                        report.items.Add(designStock);
                    }
                    foreach (var sitem in gitem)
                    {
                        if (report.sizeids.IndexOf(sitem.sizeid) >= 0)
                            designStock.wip[report.sizeids.IndexOf(sitem.sizeid)] = sitem.stock;
                    }
                }
            }

            param.Add("fromdate", item.fromdate.Date);
            param.Add("todate", item.todate.Date.AddDays(1));
            if (item.ShowPurchase)
            {
                string desingcondition = "";
                if (designs.Count == 1)
                    desingcondition = " and lrinvoiceitems.designid = @designid";
                else if (designs.Count > 1)
                    desingcondition = " and lrinvoiceitems.designid in (" + string.Join(",", designs.ConvertAll(x => x.name)) + ")";

                if (item.productid > 0)
                {
                    desingcondition += " and lrinvoiceitems.productid = " + item.productid + " ";
                }

                query = string.Format(@"select sum(lrinvoiceitems.buyingqty) stockqty,lrinvoiceitems.itemdetail->>'sizeid',
lrinvoiceitems.designid,lrinvoiceitems.uan from lrentry
inner join mfgworkordergrn on mfgworkordergrn.id = importreferenceid and importreferencetype = 1
inner join lrinvoice on lrinvoice.lrid = lrentry.id
inner join lrinvoiceitems on lrinvoiceitems.lrinvoiceid = lrinvoice.id
where mfgworkordergrn.isactive=true and lrentry.isactive=true 
and lrinvoice.isactive=true and lrinvoiceitems.isactive=true
and mfgworkordergrn.invoicedate>=@fromdate and mfgworkordergrn.invoicedate<@todate
{0}
group by lrinvoiceitems.designid,lrinvoiceitems.itemdetail->>'sizeid',lrinvoiceitems.uan", desingcondition);
                stocklist = new List<StockItem>();
                using (var reader = ExecuteReader(query, param, 10000))
                {
                    while (reader.Read())
                    {
                        StockItem stockItem = new StockItem();
                        stockItem.stock = reader["stockqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["stockqty"]);
                        stockItem.sizeid = reader["sizeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sizeid"].ToString());
                        stockItem.serialno = reader["uan"] == DBNull.Value ? "" : Convert.ToString(reader["uan"]);
                        string designid = reader["designid"] == DBNull.Value ? "" : Convert.ToString(reader["designid"]);
                        if (designs.Exists(x => x.name == designid))
                        {
                            var design = designs.Find(x => x.name == designid);
                            stockItem.designid = design.name;
                            if (design.sizegroup.items.Exists(x => x.uid == stockItem.serialno))
                            {
                                stockItem.name = design.sizegroup.items.Find(x => x.uid == stockItem.serialno).code;
                                stockItem.colourid = design.sizegroup.items.Find(x => x.uid == stockItem.serialno).colourid;
                                stocklist.Add(stockItem);
                            }
                        }
                    }
                }
                foreach (var gitem in stocklist.GroupBy(x => new { x.designid, x.name, x.colourid }))
                {

                    MfgReportServiceController.StockReport.DesignStock designStock = new MfgReportServiceController.StockReport.DesignStock();
                    designStock.colour = colourlist.Exists(x => x.id == gitem.Key.colourid) ? colourlist.Find(x => x.id == gitem.Key.colourid).name : "";
                    designStock.name = gitem.Key.name;
                    designStock.designid = gitem.Key.designid;
                    designStock.colourid = gitem.Key.colourid;
                    if (report.items.Exists(x => x.name == designStock.name))
                    {
                        designStock = report.items.Find(x => x.name == designStock.name);
                        designStock.pur = new decimal[report.sizeids.Count];
                    }
                    else
                    {
                        var design = designs.Find(x => x.name == gitem.Key.designid);
                        designStock.brandid = design.brandid;
                        designStock.productid = design.productid;
                        designStock.fabricid = design.fabricid;
                        designStock.designid = gitem.Key.designid;
                        designStock.barcode = gitem.First().serialno.Split('-')[0];
                        designStock.wip = new decimal[report.sizeids.Count];
                        designStock.so = new decimal[report.sizeids.Count];
                        designStock.stock = new decimal[report.sizeids.Count];
                        designStock.sal = new decimal[report.sizeids.Count];
                        designStock.pur = new decimal[report.sizeids.Count];
                        report.items.Add(designStock);
                    }
                    foreach (var sitem in gitem)
                    {
                        if (report.sizeids.IndexOf(sitem.sizeid) >= 0)
                            designStock.pur[report.sizeids.IndexOf(sitem.sizeid)] = sitem.stock;
                    }
                }
            }


            if (item.ShowSales)
            {
                var allcombos = mfgdesignDataService.GetAllComboDesigns();
                List<string> designlistcritera = new List<string>();
                designlistcritera.AddRange(designs.ConvertAll(x => x.name));
                var combodesigns = allcombos.FindAll(x => designlistcritera.Contains(x.DesignList));
                designlistcritera.AddRange(combodesigns.ConvertAll(x => x.mfgdesignname));

                if (combodesigns.Count > 0)
                {
                    designs.AddRange(mfgdesignDataService.Search(new MfgdesignCreteria()
                    {
                        includedetail = true,
                        codes = combodesigns.ConvertAll(x => x.mfgdesignname)
                    }));
                }

                string desingcondition = "";
                if (designlistcritera.Count == 1)
                    desingcondition = " and stock.designid = @designid";
                else if (designlistcritera.Count > 1)
                    desingcondition = " and stock.designid in (" + string.Join(",", designlistcritera.ConvertAll(x => "'" + x + "'")) + ")";

                if (combodesigns.Count == 0 && item.productid > 0)
                {
                    desingcondition += " and stock.productid = " + item.productid + " ";
                }

                query = string.Format(@"select stock.barcode,stock.designid,items.sizeid,sum(billitems.qty) stockqty from bill
                inner join billitems on billitems.billid = bill.id
                inner join stock on stock.id = billitems.stockid
                inner join items on items.id = stock.itemid
                where bill.isactive=true and billitems.isactive=true 
                and stock.isactive=true and bill.billtype != 9
                and bill.billdate>=@fromdate and bill.billdate<@todate
                {0}
                group by stock.barcode,stock.designid,items.sizeid", desingcondition);

                stocklist = new List<StockItem>();
                using (var reader = ExecuteReader(query, param, 10000))
                {
                    while (reader.Read())
                    {
                        StockItem stockItem = new StockItem();
                        stockItem.stock = reader["stockqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["stockqty"]);
                        stockItem.sizeid = reader["sizeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sizeid"].ToString());
                        stockItem.serialno = reader["barcode"] == DBNull.Value ? "" : Convert.ToString(reader["barcode"]);
                        string designid = reader["designid"] == DBNull.Value ? "" : Convert.ToString(reader["designid"]);
                        if (designs.Exists(x => x.name == designid))
                        {
                            var design = designs.Find(x => x.name == designid);
                            stockItem.designid = design.name;
                            if (design.sizegroup.items.Exists(x => x.uid == stockItem.serialno))
                            {
                                stockItem.name = design.sizegroup.items.Find(x => x.uid == stockItem.serialno).code;
                                stockItem.colourid = design.sizegroup.items.Find(x => x.uid == stockItem.serialno).colourid;
                                stocklist.Add(stockItem);
                            }
                        }
                    }
                }

                var combostocks = stocklist.FindAll(x => combodesigns.Exists(y => y.mfgdesignname == x.designid));
                if (combostocks.Count > 0)
                {
                    var combosizelist = referencelistDataService.Search(new ReferencelistCreteria()
                    {
                        ids = combostocks.ConvertAll(x => x.sizeid).Distinct().ToList()
                    });

                    stocklist.RemoveAll(x => combodesigns.Exists(y => y.mfgdesignname == x.designid));
                    foreach (var cdesign in combostocks.GroupBy(x => x.designid))
                    {
                        var design = designs.Find(x => x.name == cdesign.Key);
                        var comboconfigitem = combodesigns.FindAll(y => y.mfgdesignname == cdesign.Key);
                        comboconfigitem.RemoveAll(x => !designs.Exists(y => y.name == x.DesignList));
                        foreach (var cstocks in cdesign)
                        {
                            if (design.sizegroup.items.Exists(x => x.uid == cstocks.serialno))
                            {
                                var uanitem = design.sizegroup.items.Find(x => x.uid == cstocks.serialno);
                                var combosetsize = combosizelist.Find(x => x.id == uanitem.sizeid);
                                if (combosetsize.additionalinfo == null)
                                    combosetsize.additionalinfo = new Referencelist.Attribute();
                                if (combosetsize.additionalinfo.sizegroups == null)
                                    combosetsize.additionalinfo.sizegroups = new List<long>();

                                foreach (var comboitemconfigitem in comboconfigitem)
                                {
                                    var comboitemdesign = designs.Find(x => x.name == comboitemconfigitem.DesignList);
                                    var comboitemuans = comboitemdesign.sizegroup.items.FindAll(x => x.colourid == uanitem.colourid &&
                                    combosetsize.additionalinfo.sizegroups.Contains(x.sizeid));
                                    if (comboitemuans.Count == 0)
                                    {
                                        comboitemuans = comboitemdesign.sizegroup.items.FindAll(x => x.colourid == uanitem.colourid);
                                    }
                                    if (comboitemuans.Count == 0)
                                    {
                                        comboitemuans = comboitemdesign.sizegroup.items.FindAll(x => combosetsize.additionalinfo.sizegroups.Contains(x.sizeid));
                                    }
                                    if (comboitemuans.Count == 0)
                                        comboitemuans.Add(comboitemdesign.sizegroup.items.First());

                                    StockItem stockItem = new StockItem();
                                    stockItem.stock = cstocks.stock;
                                    stockItem.sizeid = comboitemuans.First().sizeid;
                                    stockItem.serialno = comboitemuans.First().uid;
                                    stockItem.name = comboitemuans.First().name;
                                    stockItem.colourid = comboitemuans.First().colourid;
                                    stockItem.designid = comboitemdesign.name;
                                    stocklist.Add(stockItem);
                                }
                            }
                        }
                    }
                }

                foreach (var gitem in stocklist.GroupBy(x => new { x.designid, x.name, x.colourid }))
                {

                    MfgReportServiceController.StockReport.DesignStock designStock = new MfgReportServiceController.StockReport.DesignStock();
                    designStock.colour = colourlist.Exists(x => x.id == gitem.Key.colourid) ? colourlist.Find(x => x.id == gitem.Key.colourid).name : "";
                    designStock.name = gitem.Key.name;
                    designStock.designid = gitem.Key.designid;
                    designStock.colourid = gitem.Key.colourid;
                    if (report.items.Exists(x => x.name == designStock.name))
                    {
                        designStock = report.items.Find(x => x.name == designStock.name);
                        designStock.sal = new decimal[report.sizeids.Count];
                    }
                    else
                    {
                        var design = designs.Find(x => x.name == gitem.Key.designid);
                        designStock.brandid = design.brandid;
                        designStock.productid = design.productid;
                        designStock.fabricid = design.fabricid;
                        designStock.designid = gitem.Key.designid;
                        designStock.barcode = gitem.First().serialno.Split('-')[0];
                        designStock.wip = new decimal[report.sizeids.Count];
                        designStock.so = new decimal[report.sizeids.Count];
                        designStock.stock = new decimal[report.sizeids.Count];
                        designStock.pur = new decimal[report.sizeids.Count];
                        designStock.sal = new decimal[report.sizeids.Count];
                        report.items.Add(designStock);
                    }
                    foreach (var sitem in gitem)
                    {
                        if (report.sizeids.IndexOf(sitem.sizeid) >= 0)
                            designStock.sal[report.sizeids.IndexOf(sitem.sizeid)] = sitem.stock;
                    }
                }
            }

            //if (item.ShowFabric)
            //{
            //    BomstocktransactionDataService bomstocktransactionDataService = new BomstocktransactionDataService() { Context = this.Context };
            //    var stocksummary = bomstocktransactionDataService.SummaryReport(new BomstocktransactionCreteria()
            //    {
            //        fromdate = DateTime.MinValue,
            //        todate = DateTime.MaxValue.AddDays(-10),
            //        bomids = report.items.ConvertAll(x => x.fabricid).Distinct().ToList()
            //    });
            //    report.items.ForEach(x =>
            //    {
            //        x.fabricstock = stocksummary.Sum(y => y.colourid == x.colourid && y.bomid == x.fabricid ? y.inward - y.outward : 0);
            //        if (designs.Exists(y => y.name == x.designid))
            //        {
            //            var design = designs.Find(y => y.name == x.designid);
            //            if (design.cutratio > 0) x.forecast = Math.Round(x.fabricstock / design.cutratio, 0);
            //            if (design.sizegroup.items.Exists(y => y.name == x.name))
            //            {
            //                var pitem = design.sizegroup.items.Find(y => y.name == x.name);
            //            }
            //        }
            //    });
            //}

            if (item.ShowFabric)
            {
                BomstocktransactionDataService bomstocktransactionDataService = new BomstocktransactionDataService() { Context = this.Context };
                var stocksummary = bomstocktransactionDataService.SummaryReport(new BomstocktransactionCreteria()
                {
                    fromdate = DateTime.MinValue,
                    reorderlevel = true,
                    todate = DateTime.MaxValue.AddDays(-10),
                    bomids = report.items.ConvertAll(x => x.fabricid).Distinct().ToList()
                });
                report.items.ForEach(x =>
                {
                    x.fabricstock = stocksummary.Sum(y => y.colourid == x.colourid && y.bomid == x.fabricid ? y.inward - y.outward : 0);
                    x.fabricpo = stocksummary.Sum(y => y.colourid == x.colourid && y.bomid == x.fabricid ? y.poqty : 0);
                    if (designs.Exists(y => y.name == x.designid))
                    {
                        var design = designs.Find(y => y.name == x.designid);
                        if (design.cutratio > 0 && x.fabricstock > 0)
                            x.forecast = Math.Round(x.fabricstock / design.cutratio, 0);
                    }
                });
            }

            if (item.ShowROL)
            {
                BOMMasterDataService bomstocktransactionDataService = new BOMMasterDataService() { Context = this.Context };
                var stocksummary = bomstocktransactionDataService.Search(new BOMMasterCreteria()
                {
                    ids = report.items.ConvertAll(x => x.fabricid).Distinct().ToList()
                });
                report.items.ForEach(x =>
                {
                    var fabric = stocksummary.Find(y => y.id == x.fabricid);
                    if (fabric != null)
                    {
                        x.bommin = fabric.romin;
                        x.bommax = fabric.romax;
                        if (fabric.colourlist != null && fabric.colourlist.Count > 0)
                        {
                            var fcolour = fabric.colourlist.Find(c => c.id == x.colourid);
                            if (fcolour != null)
                            {
                                x.bommin = fcolour.romin;
                                x.bommax = fcolour.romax;
                            }
                        }

                        if (designs.Exists(y => y.name == x.designid))
                        {
                            var design = designs.Find(y => y.name == x.designid);
                            if (design.cutratio > 00)
                            {
                                x.fgromin = x.bommin > 0 ? (x.bommin * design.cutratio) : 0;
                                x.fgromax = x.bommax > 0 ? (x.bommax * design.cutratio) : 0;
                            }
                        }
                    }


                });
            }

            if (item.ShowPacking)
            {
                string packingbarcodes = @"select locationid,barcodelist->>'barcode' barcode,
                sum(cast(barcodelist->>'qty' as numeric)) stockqty
                from packingslip
                LEFT JOIN LATERAL json_array_elements (cast(packingslip.barcodes as json)) barcodelist ON true
                where billid=0 and isactive=true and locationid=@locationid
                group by locationid,barcodelist->>'barcode'";

                if (item.locationid == 0)
                {
                    packingbarcodes = packingbarcodes.Replace("and locationid=@locationid", "");
                }

                SortedDictionary<string, decimal> packstock = new SortedDictionary<string, decimal>();
                using (var reader = ExecuteReader(packingbarcodes, param, 10000))
                {
                    while (reader.Read())
                    {
                        StockItem stockItem = new StockItem();
                        var serialno = reader["barcode"] == DBNull.Value ? "" : Convert.ToString(reader["barcode"]);
                        var stock = reader["stockqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["stockqty"]);
                        var designindex = designs.FindIndex(x => x.sizegroup.items.Exists(y => y.uid == serialno));
                        if (designindex >= 0)
                        {
                            var pitem = designs[designindex].sizegroup.items.Find(y => y.uid == serialno);
                            if (report.items.Exists(x => x.designid == designs[designindex].code &&
                            x.colourid == designs[designindex].colourid))
                            {
                                var rptitem = report.items.Find(x => x.designid == designs[designindex].code &&
                                x.colourid == designs[designindex].colourid);
                                if (rptitem.pck == null)
                                    rptitem.pck = new decimal[report.sizeids.Count];

                                if (report.sizeids.IndexOf(pitem.sizeid) >= 0)
                                    rptitem.pck[report.sizeids.IndexOf(pitem.sizeid)] += stock;
                            }
                        }
                        else if (packstock.ContainsKey(serialno))
                        {
                            packstock[serialno] += stock;
                        }
                        else
                        {
                            packstock.Add(serialno, stock);
                        }
                    }
                }

                if (packstock.Count > 0)
                {
                    var designuanlist = mfgdesignDataService.SearchUANItem(new Mfgdesign.Sizegroup.SearchItem()
                    {
                        uids = packstock.Keys.ToList()
                    });
                    foreach (var grp in designuanlist.GroupBy(x => new {  x.code, x.name, x.colourid }))
                    {
                        var ds = new MfgReportServiceController.StockReport.DesignStock()
                        {
                            designid = grp.Key.code,
                            colourid = grp.Key.colourid,
                            barcode = grp.First().uid.Split(new char[] {'-','/' })[0],
                            name = grp.Key.name,
                            productid = grp.First().productid,
                            brandid = grp.First().brandid,
                            colour = colourlist.Exists(x => x.id == grp.Key.colourid) ? colourlist.Find(x => x.id == grp.Key.colourid).name : "",
                            pck = new decimal[report.sizeids.Count],
                            stock = new decimal[report.sizeids.Count]
                        };
                        foreach(var grpitem in grp)
                        {
                            if(packstock.ContainsKey(grpitem.uid))
                            {
                                if (report.sizeids.IndexOf(grpitem.sizeid) >= 0)
                                    ds.pck[report.sizeids.IndexOf(grpitem.sizeid)] += packstock[grpitem.uid];
                            }
                        }
                        report.items.Add(ds);
                    }
                }
            }

            if (item.ShowPrice)
            {
                report.items.ForEach(x =>
                {
                    if (designs.Exists(y => y.name == x.designid))
                    {
                        var design = designs.Find(y => y.name == x.designid);
                        if (design.sizegroup.items.Exists(y => y.code == x.name))
                        {
                            var pitem = design.sizegroup.items.Find(y => y.code == x.name);
                            x.price = new decimal[] { pitem.wsrate, pitem.dealerprice, pitem.salerate, pitem.mrp };
                        }
                    }
                });
            }
            return report;
        }

        public void BuildDashboard()
        {
            string query = @"select mfgdesign.classid,mfgdesign.productid,mfgdesign.brandid,items.sizeid,
            sum(salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty)) stockqty from salablegoods
            inner join stock on stock.id = stockid
			inner join mfgdesign on mfgdesign.code = stock.designid
            inner join items on items.id = stock.itemid
            where salablegoods.hasstock=true and salablegoods.isactive=true and stock.isactive=true 
            group by mfgdesign.classid,mfgdesign.productid,mfgdesign.brandid,items.sizeid;

            select mfgdesign.classid,mfgdesign.productid,mfgdesign.brandid,salesorderitems.sizeid,sum(oqty-(bqty+cqty+rqty)) stockqty
            from salesorderitems 
			inner join salesorder on salesorder.id =salesorderid
			inner join mfgdesign on mfgdesign.code = salesorderitems.designid
			where salesorderitems.isactive=true and salesorder.isactive=true  and oqty-(bqty+cqty+rqty)>0
            and sodate>=@datefrom and sodate<@dateto
            group by mfgdesign.classid,mfgdesign.productid,mfgdesign.brandid,salesorderitems.sizeid;

            select mfgdesign.classid,mfgdesign.productid,mfgdesign.brandid,salesorderitems.sizeid,sum(oqty-(bqty+cqty+rqty)) stockqty
            from salesorderitems 
			inner join salesorder on salesorder.id =salesorderid
			inner join mfgdesign on mfgdesign.code = salesorderitems.designid
			where salesorderitems.isactive=true and salesorder.isactive=true  and oqty-(bqty+cqty+rqty)>0
            group by mfgdesign.classid,mfgdesign.productid,mfgdesign.brandid,salesorderitems.sizeid;
					
            select sum(qty-delivered) stockqty,mfgdesign.productid,mfgdesign.classid,mfgdesign.brandid,Mfgworkorderitemsizes.sizeid from Mfgworkorderitemsizes
            inner join mfgworkorderitems on mfgworkorderitems.id = Mfgworkorderitemsizes.mfgworkorderitemsid
            inner join mfgworkorder on mfgworkorder.id = mfgworkorderitems.mfgworkorderid
			inner join mfgdesign on mfgdesign.code = Mfgworkorderitems.designname
            where Mfgworkorderitemsizes.isactive = true and mfgworkorderitems.isactive = true
            and mfgworkorder.isactive = true and mfgworkorder.statusid != 9
			group by mfgdesign.classid,mfgdesign.productid,mfgdesign.brandid,Mfgworkorderitemsizes.sizeid;

            select mfgdesign.classid,mfgdesign.productid,sum(billitems.qty) stockqty,items.brandid,items.sizeid from bill
            inner join billitems on billitems.billid = bill.id and billitems.isactive=true
            inner join stock on stock.id= billitems.stockid
            inner join items on items.id=stock.itemid
            inner join mfgdesign on mfgdesign.code = stock.designid
            where billdate>=@datefrom and billdate<@dateto
            group by mfgdesign.classid,mfgdesign.productid,items.brandid,items.sizeid;

            select sum(cast(schemelist->>'qty' as numeric)) stockqty,salesorder.agentid,salesorder.salesmenid,mfgdesign.classid,mfgdesign.productid from bill
			inner join billitems on billitems.billid = bill.id and billitems.isactive=true
			inner join stock on stock.id= billitems.stockid
            inner join items on items.id=stock.itemid
            inner join mfgdesign on mfgdesign.code = stock.designid
			LEFT JOIN LATERAL
			json_array_elements (cast(billitems.attributes->>'so' as json)) schemelist ON true
			inner join salesorderitems on salesorderitems.id = cast(schemelist->>'id' as bigint)
			inner join salesorder on salesorder.id= salesorderitems.salesorderid
            where billdate>=@datefrom and billdate<@dateto
			group by salesorder.agentid,salesorder.salesmenid,mfgdesign.classid,mfgdesign.productid";

            if (Services.ConfigManager.Instance(Context.OrganizationID).Config.analyser_stock_removeaqty)
                query = query.Replace("sum(salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty))", "sum(salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty+salablegoods.aqty))");
        }

        internal List<MfgReportServiceController.CustomStockCriteria.StockResult> SearchMfgUANStock(MfgReportServiceController.CustomStockCriteria items)
        {
            string query = @"with currentstock 
as
(
    select sum(iqty-(oqty+dqty+jqty)) curstk,sum(buyingprice*(iqty-(oqty+dqty+jqty))) cost,stock.serialno,
    salablegoods.locationid from salablegoods
    inner join stock on stock.id = stockid
    where salablegoods.isactive=true and stock.isactive=true and salablegoods.locationid=@locationid
    group by stock.serialno,salablegoods.locationid
),
currentpo as
(
    select sum(oqty::numeric-(bqty::numeric+cqty::numeric+rqty::numeric)) pendingqty,
    Salesorder.locationid,Salesorderitems.uid
    from Salesorder 
    inner join Salesorderitems on Salesorderitems.salesorderid = Salesorder.id
    inner join currentstock on currentstock.serialno = Salesorderitems.uid
    where oqty::numeric-(bqty::numeric+cqty::numeric+rqty::numeric)>0 and 
    salesorder.isactive=true and Salesorderitems.isactive=true and 
    Salesorder.locationid=currentstock.locationid and Salesorder.sosourcetype=0
    group by Salesorderitems.uid,Salesorder.locationid
),
currentsto as
(
    select 
    schemelist ->> 'serialno' uid,fromlocationid locationid,
    sum(cast(schemelist ->> 'iqty' as numeric)) qty from bundles 
    LEFT JOIN LATERAL 
    json_array_elements (case when bundles.items is null or bundles.items::text ='null' then '[]'::json else bundles.items :: json end) schemelist ON true
    where  bundles.isactive=true and bundles.status<=0 and bundles.fromlocationid = @locationid
    group by schemelist ->> 'serialno',fromlocationid
)
, alluid
as
(
    select curstk,pendingqty,currentsto.qty pendingsto,cost,currentstock.locationid,mfgdesign.code,
    schemelist->>'uid' uid,cast(schemelist->>'sizeid' as bigint) sizeid,
    cast(schemelist->>'mrp' as numeric) mrp,
    schemelist->>'code' dname,schemelist->>'prdsku' prdsku,
    cast(schemelist->>'colourid' as bigint) colourid,
    mfgdesign.productid,mfgdesign.prddivisionid,mfgdesign.classid {0}
    from mfgdesign 
    LEFT JOIN LATERAL
    json_array_elements (cast(mfgdesign.sizegroup->>'items' as json)) schemelist ON true
    inner join currentstock  on currentstock .serialno = schemelist->>'uid'
    left outer join currentpo on currentpo.uid =  currentstock.serialno and currentpo.locationid =  currentstock.locationid
    left outer join currentsto on currentsto.uid =  currentstock.serialno and currentsto.locationid =  currentstock.locationid
    where mfgdesign.isactive=true 
)

select * from alluid ";

            List<string> additionafields = new List<string>();
            if (items.customfields != null)
                additionafields = items.customfields.ConvertAll(x => "mfg_" + x.field.cfname);
            query = String.Format(query, additionafields.Count == 0 ? "" : "," + (string.Join(",", additionafields)));
            var reffields = items.customfields.FindAll(x => !string.IsNullOrEmpty(x.field.cfref));
            List<ReferenceItem> refdatalist = null;
            if (reffields.Count > 0)
            {
                ReferencelistDataService referencelistDataService = new ReferencelistDataService() { Context = this.Context };
                refdatalist = referencelistDataService.SearchList(new ReferencelistCreteria()
                {
                    types = reffields.ConvertAll(x => x.field.cfref)
                });
            }

            List<string> conditions = new List<string>();
            if (items.productid > 0)
                conditions.Add("alluid.productid=@productid");
            if (!string.IsNullOrEmpty(items.uid))
                conditions.Add("alluid.uid=@uid");
            if (!string.IsNullOrEmpty(items.designid))
                conditions.Add("alluid.code=@designid");
            if (items.locationid > 0)
                conditions.Add("alluid.locationid=@locationid");
            if (items.classificationid > 0)
                conditions.Add("alluid.classid=@classificationid");
            if (items.prddivisionid > 0)
                conditions.Add("alluid.prddivisionid=@prddivisionid");

            if (conditions.Count > 0)
                query += " where " + String.Join(" and ", conditions);

            bool addcustomfields = items.customfields != null && items.customfields.Count > 0;
            var response = new List<MfgReportServiceController.CustomStockCriteria.StockResult>();
            var param = new SortedList<string, object>();
            param.Add("locationid", items.locationid);
            param.Add("classificationid", items.classificationid);
            param.Add("prddivisionid", items.prddivisionid);
            param.Add("productid", items.productid);
            param.Add("uid", string.IsNullOrEmpty(items.uid) ? "" : items.uid);
            param.Add("designid", string.IsNullOrEmpty(items.designid) ? "" : items.designid);
            using (var reader = ExecuteReader(query, param, 10000))
            {
                while (reader.Read())
                {
                    MfgReportServiceController.CustomStockCriteria.StockResult result = new MfgReportServiceController.CustomStockCriteria.StockResult();
                    result.locationid = Convert.ToInt64(reader["locationid"]);
                    result.prddivisionid = Convert.ToInt64(reader["prddivisionid"]);
                    result.productid = Convert.ToInt64(reader["productid"]);
                    result.classid = Convert.ToInt64(reader["classid"]);
                    result.colourid = Convert.ToInt64(reader["colourid"]);
                    result.sizeid = Convert.ToInt64(reader["sizeid"]);

                    result.designname = Convert.ToString(reader["dname"]);
                    result.design = Convert.ToString(reader["code"]);
                    result.uid = Convert.ToString(reader["uid"]);

                    //schemelist->>'code' dname,schemelist->>'prdsku' prdsku,
                    result.mrp = Convert.ToDecimal(reader["mrp"]);
                    result.stock = reader["curstk"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["curstk"]);
                    result.so = reader["pendingqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["pendingqty"]);
                    result.sto = reader["pendingsto"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["pendingsto"]);
                    result.cost = reader["cost"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["cost"]);
                    if (result.cost > 0)
                        result.cost = result.stock / result.cost;

                    if (addcustomfields)
                    {
                        result.customfields = new List<string>();
                        foreach (var cfield in items.customfields)
                        {
                            string svalue = "";
                            if (cfield.field.cftype == "bigint")
                            {
                                long value = reader["mfg_" + cfield.field.cfname] == DBNull.Value ? 0 : Convert.ToInt64(reader["mfg_" + cfield.field.cfname]);
                                if (refdatalist != null &&
                                    refdatalist.Exists(x => x.type == cfield.field.cfref &&
                                    x.id == value))
                                {
                                    svalue = refdatalist.Find(x => x.type == cfield.field.cfref &&
                                    x.id == value).name;
                                }
                            }
                            else if (cfield.field.cftype.StartsWith("varchar"))
                            {
                                svalue = reader["mfg_" + cfield.field.cfname] == DBNull.Value ? "" : Convert.ToString(reader["mfg_" + cfield.field.cfname]);
                            }
                            result.customfields.Add(svalue);
                        }
                    }
                    response.Add(result);
                }
            }
            return response;
        }

        internal List<MfgReportServiceController.CustomStockCriteria.SalesReport> SearchUANSalesReport(MfgReportServiceController.CustomStockCriteria items)
        {
            string query = @"
            with grndetails as
            (
                select billattributes->'ecom'->>'channel' channel,
                billattributes->'ecom'->>'morderid' morderid,
                billattributes->'ecom'->>'orderid' orderid,
                billattributes->'ecom'->>'invoicecode' invoicecode,
                stock.serialno,billitems.qty,billitems.receivable,billitems.tax,bill.locationid,
                customer.name customer,referencelist.name category,billno,billdate::date,statelist.name pos,
                bill.billattributes->>'orderrefid' sono,bill.isinterstatesale,
                billitems.schemediscount,billitems.additionaldiscount,billitems.hsncode,
                billitems.discount,billitems.taxpercentage,items.brandid,
                items.productid,items.sizeid,items.colourid,billitems.price
                from bill
                left outer join customer on customer.id = customerid
                left outer join referencelist on referencelist.id = customer.categoryid
                inner join company on company.id = bill.companyid
                left outer join referencelist statelist on statelist.id = 
                case when customerstateid is not null and customerstateid > 0 then customerstateid else company.stateid end
                inner join billitems on billitems.billid = bill.id and billitems.isactive=true
                inner join salablegoods on salablegoods.id = salablegoodsid
                inner join stock on stock.id = salablegoods.stockid
                inner join items on items.id = stock.itemid
                where bill.billdate>=@fromdate and bill.billdate<@todate and bill.isactive=true 
                and billtype != 9  {1}
            ), alluid as
            (
                select mfgdesign.code,
                schemelist->>'uid' uid,cast(schemelist->>'sizeid' as bigint) sizeid,
                cast(schemelist->>'mrp' as numeric) mrp,
                schemelist->>'code' dname,schemelist->>'prdsku' prdsku,
                cast(schemelist->>'colourid' as bigint) colourid,prdhsncode,
                mfgdesign.productid,mfgdesign.prddivisionid,mfgdesign.classid {0}
                from mfgdesign 
                LEFT JOIN LATERAL
                json_array_elements (cast(mfgdesign.sizegroup->>'items' as json)) schemelist ON true
                where mfgdesign.isactive=true
            )
			select grndetails.*,alluid.* from grndetails
            inner join alluid  on  alluid.uid = grndetails.serialno";

            string additionalcondition = "";
            if (items.locationid > 0)
            {
                additionalcondition += " and bill.locationid=@locationid";
            }
            if (items.companyid > 0)
            {
                additionalcondition += " and bill.companyid=@companyid";
            }
            if (items.customerid > 0)
            {
                additionalcondition += " and bill.customerid=@customerid";
            }
            if (items.categoryid > 0)
            {
                additionalcondition += " and customer.categoryid=@categoryid";
            }
            List<string> additionafields = new List<string>();
            if (items.customfields != null)
                additionafields = items.customfields.ConvertAll(x => "mfg_" + x.field.cfname);
            query = String.Format(query, additionafields.Count == 0 ? "" : "," + (string.Join(",", additionafields)), additionalcondition);
            var reffields = items.customfields.FindAll(x => !string.IsNullOrEmpty(x.field.cfref));
            List<ReferenceItem> refdatalist = null;
            if (reffields.Count > 0)
            {
                ReferencelistDataService referencelistDataService = new ReferencelistDataService() { Context = this.Context };
                refdatalist = referencelistDataService.SearchList(new ReferencelistCreteria()
                {
                    types = reffields.ConvertAll(x => x.field.cfref)
                });
            }
            var response = new List<MfgReportServiceController.CustomStockCriteria.SalesReport>();
            List<string> conditions = new List<string>();
            if (items.productid > 0)
                conditions.Add("alluid.productid=@productid");
            if (!string.IsNullOrEmpty(items.uid))
                conditions.Add("alluid.uid=@uid");
            if (!string.IsNullOrEmpty(items.designid))
                conditions.Add("alluid.code=@designid");
            if (items.classificationid > 0)
                conditions.Add("alluid.classid=@classificationid");
            if (items.prddivisionid > 0)
                conditions.Add("alluid.prddivisionid=@prddivisionid");

            if (conditions.Count > 0)
                query += " where " + String.Join(" and ", conditions);

            var param = new SortedList<string, object>();
            param.Add("locationid", items.locationid);
            param.Add("companyid", items.companyid);
            param.Add("customerid", items.customerid);
            param.Add("categoryid", items.categoryid);
            param.Add("classificationid", items.classificationid);
            param.Add("prddivisionid", items.prddivisionid);
            param.Add("productid", items.productid);
            param.Add("fromdate", items.fromdate);
            param.Add("todate", items.todate.AddDays(1));
            param.Add("uid", string.IsNullOrEmpty(items.uid) ? "" : items.uid);
            param.Add("designid", string.IsNullOrEmpty(items.designid) ? "" : items.designid);
            using (var conn = CreateConnection())
            {
                conn.Open();
                string cursorName = ("SearchUANSalesReport_" + Guid.NewGuid().ToString().Replace("-", "")).ToLower();
                using (var tx = conn.BeginTransaction(System.Data.IsolationLevel.RepeatableRead))
                {
                    using (var setup = new Npgsql.NpgsqlCommand(@"
SET LOCAL statement_timeout = 0;
SET LOCAL idle_in_transaction_session_timeout = 0;
SET LOCAL work_mem = '256MB';              -- tune: 128–256MB is a good start
SET LOCAL jit = off;                       -- reduce JIT startup latency for one-off reports
SET LOCAL cursor_tuple_fraction = 1.0;     -- planner: we'll read most rows via cursor
", conn, tx))
                    {
                        setup.ExecuteNonQuery();
                    }
                    using (var declare = GetCommand($"DECLARE {cursorName} NO SCROLL CURSOR FOR {query}", param, conn, tx))
                    {
                        declare.ExecuteNonQuery();
                    }
                    try
                    {
                        while (true)
                        {
                            int batchSize = 5000;
                            using (var reader = ExecuteSequenceReader($"FETCH FORWARD {batchSize} FROM {cursorName}", param, 10000,
                                conn,tx))
                            {
                                SortedDictionary<string, int> ord = reader.Build();
                                int rows = 0;
                                if (!reader.Read())
                                    break; // no rows 
                                do
                                {
                                    Debug.WriteLine(rows++);
                                    MfgReportServiceController.CustomStockCriteria.SalesReport result = new MfgReportServiceController.CustomStockCriteria.SalesReport();

                                    result.channel = reader.GetStringOr(ord["channel"]);
                                    result.morderid = reader.GetStringOr(ord["morderid"]);

                                    var orderid = reader.GetStringOr(ord["orderid"]);
                                    if (!string.IsNullOrEmpty(orderid))
                                        result.morderid = orderid;

                                    var serialno = reader.GetStringOr(ord["serialno"]);
                                    result.qty = reader.GetDecimalOr(ord["qty"]);
                                    result.receivable = reader.GetDecimalOr(ord["receivable"]);
                                    result.tax = reader.GetDecimalOr(ord["tax"]);
                                    result.locationid = reader.GetInt64Or(ord["locationid"]);
                                    result.customer = reader.GetStringOr(ord["customer"]);
                                    result.category = reader.GetStringOr(ord["category"]);
                                    result.billno = reader.GetStringOr(ord["billno"]);
                                    result.billdate = reader.GetDateTimeOr(ord["billdate"]);
                                    result.pos = reader.GetStringOr(ord["pos"]);
                                    result.sono = reader.GetStringOr(ord["sono"]);
                                    result.isinterstatesale = reader.GetBooleanOr(ord["isinterstatesale"]);
                                    result.schemediscount = reader.GetDecimalOr(ord["schemediscount"]);
                                    result.additionaldiscount = reader.GetDecimalOr(ord["additionaldiscount"]);
                                    result.einvoicecode = reader.GetStringOr(ord["invoicecode"]);
                                    
                                    result.hsn = reader.GetStringOr(ord["hsncode"]);
                                    var discount = reader.GetDecimalOr(ord["discount"]);
                                    result.taxpercentage = reader.GetDecimalOr(ord["taxpercentage"]);
                                    var brandid = reader.GetInt64Or(ord["brandid"]);
                                    result.productid = reader.GetInt64Or(ord["productid"]);
                                    result.sizeid = reader.GetInt64Or(ord["sizeid"]);
                                    result.colourid = reader.GetInt64Or(ord["colourid"]);
                                    result.price = reader.GetDecimalOr(ord["price"]);
                                    result.design = reader.GetStringOr(ord["code"]);
                                    result.uid = reader.GetStringOr(ord["uid"]);
                                    result.mrp = reader.GetDecimalOr(ord["mrp"]);
                                    result.designname = reader.GetStringOr(ord["dname"]);
                                    var prdsku = reader.GetStringOr(ord["prdsku"]);

                                    // fallback hsn
                                    if (string.IsNullOrEmpty(result.hsn))
                                        result.hsn = reader.GetStringOr(ord["prdhsncode"]);

                                    result.prddivisionid = reader.GetInt64Or(ord["prddivisionid"]);
                                    result.classid = reader.GetInt64Or(ord["classid"]);

                                    if (items.customfields != null && items.customfields.Count > 0)
                                    {
                                        result.customfields = new List<string>();
                                        foreach (var cfield in items.customfields)
                                        {
                                            string svalue = "";
                                            if (cfield.field.cftype == "bigint")
                                            {
                                                long value = reader.GetInt64Or(ord["mfg_" + cfield.field.cfname]);
                                                if (refdatalist != null &&
                                                    refdatalist.Exists(x => x.type == cfield.field.cfref &&
                                                    x.id == value))
                                                {
                                                    svalue = refdatalist.Find(x => x.type == cfield.field.cfref &&
                                                    x.id == value).name;
                                                }
                                            }
                                            else if (cfield.field.cftype.StartsWith("varchar"))
                                            {
                                                reader.GetStringOr(ord["mfg_" + cfield.field.cfname]);
                                                svalue = reader["mfg_" + cfield.field.cfname] == DBNull.Value ? "" : Convert.ToString(reader["mfg_" + cfield.field.cfname]);
                                            }
                                            result.customfields.Add(svalue);
                                        }
                                    }
                                    response.Add(result);
                                } while (reader.Read());
                            }
                        }
                    }
                    finally
                    {
                        // 3) CLOSE cursor no matter what
                        using (var close = new NpgsqlCommand($"CLOSE {cursorName};", conn, tx))
                            close.ExecuteNonQuery();
                    }
                    tx.Commit();
                }
                conn.Close();
                return response;
            }
        }

        internal List<MfgReportServiceController.CustomStockCriteria.SOResult> SearchUANSOReport(MfgReportServiceController.CustomStockCriteria items)
        {
            string query = @"
            with grndetails as
            (
                select salesorder.sobookno,salesorder.status,salesorder.expectedate,salesorder.remarks,
                salesorder.locationid,salesorderitems.oqty,salesorderitems.price,salesorderitems.uid gln,
                salesorderitems.mrp, customer.name customer,salesorder.sono,salesorder.sodate,
	            salesorderitems.rqty,salesorderitems.bqty,salesorderitems.cqty {1}
                from salesorder
                inner join customer on customer.id = salesorder.customerid
                inner join salesorderitems on salesorderitems.salesorderid = salesorder.id
                and salesorderitems.isactive=true {1}
                where salesorder.sodate>=@fromdate and salesorder.sodate<@todate and salesorder.isactive=true
            ), alluid
            as
            (
                select grndetails.*,mfgdesign.code,
                schemelist->>'uid' uid,cast(schemelist->>'sizeid' as bigint) sizeid,
                cast(schemelist->>'mrp' as numeric) mrp,
                schemelist->>'code' dname,schemelist->>'prdsku' prdsku,
                cast(schemelist->>'colourid' as bigint) colourid,
                mfgdesign.productid,mfgdesign.prddivisionid,mfgdesign.classid {0}
                from mfgdesign 
                LEFT JOIN LATERAL
                json_array_elements (cast(mfgdesign.sizegroup->>'items' as json)) schemelist ON true
                inner join grndetails  on grndetails.gln = schemelist->>'uid'
                where mfgdesign.isactive=true
            )
            select * from alluid ";

            string additionalcondition = "";
            if (items.posearchmode > 0)
            {
                additionalcondition = items.posearchmode == 1 ? " and oqty-(cqty+rqty+bqty)>0" : " and oqty-(cqty+rqty+bqty)<=0";
            }
            List<string> additionafields = new List<string>();
            if (items.customfields != null)
                additionafields = items.customfields.ConvertAll(x => "mfg_" + x.field.cfname);
            query = String.Format(query, additionafields.Count == 0 ? "" : "," + (string.Join(",", additionafields)), additionalcondition);
            var reffields = items.customfields.FindAll(x => !string.IsNullOrEmpty(x.field.cfref));
            List<ReferenceItem> refdatalist = null;
            if (reffields.Count > 0)
            {
                ReferencelistDataService referencelistDataService = new ReferencelistDataService() { Context = this.Context };
                refdatalist = referencelistDataService.SearchList(new ReferencelistCreteria()
                {
                    types = reffields.ConvertAll(x => x.field.cfref)
                });
            }
            var response = new List<MfgReportServiceController.CustomStockCriteria.SOResult>();

            List<string> conditions = new List<string>();
            if (items.productid > 0)
                conditions.Add("alluid.productid=@productid");
            if (!string.IsNullOrEmpty(items.uid))
                conditions.Add("alluid.uid=@uid");
            if (!string.IsNullOrEmpty(items.designid))
                conditions.Add("alluid.code=@designid");
            if (items.locationid > 0)
                conditions.Add("alluid.locationid=@locationid");
            if (items.classificationid > 0)
                conditions.Add("alluid.classid=@classificationid");
            if (items.prddivisionid > 0)
                conditions.Add("alluid.prddivisionid=@prddivisionid");

            if (conditions.Count > 0)
                query += " where " + String.Join(" and ", conditions);

            var param = new SortedList<string, object>();
            param.Add("locationid", items.locationid);
            param.Add("classificationid", items.classificationid);
            param.Add("prddivisionid", items.prddivisionid);
            param.Add("productid", items.productid);
            param.Add("uid", string.IsNullOrEmpty(items.uid) ? "" : items.uid);
            param.Add("designid", string.IsNullOrEmpty(items.designid) ? "" : items.designid);
            param.Add("fromdate", items.fromdate.Date);
            param.Add("todate", items.todate.AddDays(1).Date);


            using (var reader = ExecuteReader(query, param, 10000))
            {
                while (reader.Read())
                {
                    MfgReportServiceController.CustomStockCriteria.SOResult result = new MfgReportServiceController.CustomStockCriteria.SOResult();
                    result.locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                    result.prddivisionid = reader["prddivisionid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["prddivisionid"]);
                    result.productid = reader["productid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productid"]);
                    result.classid = reader["classid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["classid"]);
                    result.colourid = reader["colourid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["colourid"]);
                    result.sizeid = reader["sizeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sizeid"]);

                    result.designname = Convert.ToString(reader["dname"]);
                    result.design = Convert.ToString(reader["code"]);
                    result.uid = Convert.ToString(reader["uid"]);

                    //schemelist->>'code' dname,schemelist->>'prdsku' prdsku,
                    result.mrp = Convert.ToDecimal(reader["mrp"]);

                    result.customer = reader["customer"] == DBNull.Value ? "" : Convert.ToString(reader["customer"]);
                    result.sono = reader["sono"] == DBNull.Value ? "" : Convert.ToString(reader["sono"]);
                    result.sodate = reader["sodate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["sodate"]);
                    result.orderqty = reader["oqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["oqty"]);
                    result.billedqty = reader["bqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["bqty"]);
                    result.rejecteddqty = reader["rqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["rqty"]);
                    result.cancelledqty = reader["cqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["cqty"]);
                    result.price = reader["price"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["price"]);
                    result.expecteddate = reader["expectedate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["expectedate"]);
                    result.remarks = reader["remarks"] == DBNull.Value ? "" : Convert.ToString(reader["remarks"]);
                    result.sobookno = reader["sobookno"] == DBNull.Value ? "" : Convert.ToString(reader["sobookno"]);

                    result.customfields = new List<string>();
                    foreach (var cfield in items.customfields)
                    {
                        string svalue = "";
                        if (cfield.field.cftype == "bigint")
                        {
                            long value = reader["mfg_" + cfield.field.cfname] == DBNull.Value ? 0 : Convert.ToInt64(reader["mfg_" + cfield.field.cfname]);
                            if (refdatalist != null &&
                                refdatalist.Exists(x => x.type == cfield.field.cfref &&
                                x.id == value))
                            {
                                svalue = refdatalist.Find(x => x.type == cfield.field.cfref &&
                                x.id == value).name;
                            }
                        }
                        else if (cfield.field.cftype.StartsWith("varchar"))
                        {
                            svalue = reader["mfg_" + cfield.field.cfname] == DBNull.Value ? "" : Convert.ToString(reader["mfg_" + cfield.field.cfname]);
                        }
                        result.customfields.Add(svalue);
                    }
                    response.Add(result);
                }
            }
            return response;
        }

        internal List<MfgReportServiceController.CustomStockCriteria.POResult> SearchUANPOReport(MfgReportServiceController.CustomStockCriteria items)
        {
            string query = @"
with grndetails as
(
    select purchaseorder.locationid,purchaseorderitems.oqty,purchaseorderitems.price,
    suppliercompany.name supplier,purchaseorder.pono,purchaseorder.podate,
	purchaseorderitems.rqty,purchaseorderitems.cqty,
	purchaseorderitems.gln,purchaseorder.expectedate,purchaseorder.expirydate,
	purchaseorder.amenddate,purchaseorder.amendreason
    from purchaseorder
    inner join suppliercompany on suppliercompany.supplierid = purchaseorder.supplierid
    inner join purchaseorderitems on purchaseorderitems.purchaseorderid = purchaseorder.id
    and purchaseorderitems.isactive=true {1}
    where purchaseorder.podate>=@fromdate and purchaseorder.podate<@todate and purchaseorder.isactive=true
), alluid
as
(
    select grndetails.*,mfgdesign.code,
    schemelist->>'uid' uid,cast(schemelist->>'sizeid' as bigint) sizeid,
    cast(schemelist->>'mrp' as numeric) mrp,
    schemelist->>'code' dname,schemelist->>'prdsku' prdsku,
    cast(schemelist->>'colourid' as bigint) colourid,
    mfgdesign.productid,mfgdesign.prddivisionid,mfgdesign.classid {0}
    from mfgdesign 
    LEFT JOIN LATERAL
    json_array_elements (cast(mfgdesign.sizegroup->>'items' as json)) schemelist ON true
    inner join grndetails  on grndetails.gln = schemelist->>'uid'
    where mfgdesign.isactive=true
)
select * from alluid ";

            string additionalcondition = "";
            if (items.posearchmode > 0)
            {
                additionalcondition = items.posearchmode == 1 ? " and oqty-(cqty+rqty)>0" : " and oqty-(cqty+rqty)<=0";
            }
            List<string> additionafields = new List<string>();
            if (items.customfields != null)
                additionafields = items.customfields.ConvertAll(x => "mfg_" + x.field.cfname);
            query = String.Format(query, additionafields.Count == 0 ? "" : "," + (string.Join(",", additionafields)), additionalcondition);
            var reffields = items.customfields.FindAll(x => !string.IsNullOrEmpty(x.field.cfref));
            List<ReferenceItem> refdatalist = null;
            if (reffields.Count > 0)
            {
                ReferencelistDataService referencelistDataService = new ReferencelistDataService() { Context = this.Context };
                refdatalist = referencelistDataService.SearchList(new ReferencelistCreteria()
                {
                    types = reffields.ConvertAll(x => x.field.cfref)
                });
            }
            var response = new List<MfgReportServiceController.CustomStockCriteria.POResult>();

            List<string> conditions = new List<string>();
            if (items.productid > 0)
                conditions.Add("alluid.productid=@productid");
            if (!string.IsNullOrEmpty(items.uid))
                conditions.Add("alluid.uid=@uid");
            if (!string.IsNullOrEmpty(items.designid))
                conditions.Add("alluid.code=@designid");
            if (items.locationid > 0)
                conditions.Add("alluid.locationid=@locationid");
            if (items.classificationid > 0)
                conditions.Add("alluid.classid=@classificationid");
            if (items.prddivisionid > 0)
                conditions.Add("alluid.prddivisionid=@prddivisionid");

            if (conditions.Count > 0)
                query += " where " + String.Join(" and ", conditions);

            var param = new SortedList<string, object>();
            param.Add("locationid", items.locationid);
            param.Add("classificationid", items.classificationid);
            param.Add("prddivisionid", items.prddivisionid);
            param.Add("productid", items.productid);
            param.Add("uid", string.IsNullOrEmpty(items.uid) ? "" : items.uid);
            param.Add("designid", string.IsNullOrEmpty(items.designid) ? "" : items.designid);
            param.Add("fromdate", items.fromdate.Date);
            param.Add("todate", items.todate.AddDays(1).Date);

            using (var reader = ExecuteReader(query, param, 10000))
            {
                while (reader.Read())
                {
                    MfgReportServiceController.CustomStockCriteria.POResult result = new MfgReportServiceController.CustomStockCriteria.POResult();
                    result.locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                    result.prddivisionid = reader["prddivisionid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["prddivisionid"]);
                    result.productid = reader["productid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productid"]);
                    result.classid = reader["classid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["classid"]);
                    result.colourid = reader["colourid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["colourid"]);
                    result.sizeid = reader["sizeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sizeid"]);

                    result.designname = Convert.ToString(reader["dname"]);
                    result.design = Convert.ToString(reader["code"]);
                    result.uid = Convert.ToString(reader["uid"]);

                    //schemelist->>'code' dname,schemelist->>'prdsku' prdsku,
                    result.mrp = Convert.ToDecimal(reader["mrp"]);
                    result.supplier = reader["supplier"] == DBNull.Value ? "" : Convert.ToString(reader["supplier"]);
                    result.pono = reader["pono"] == DBNull.Value ? "" : Convert.ToString(reader["pono"]);
                    result.podate = reader["podate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["podate"]);
                    result.orderqty = reader["oqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["oqty"]);
                    result.receivedqty = reader["rqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["rqty"]);
                    result.cancelledqty = reader["cqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["cqty"]);
                    result.price = reader["price"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["price"]);
                    result.expecteddate = reader["expectedate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["expectedate"]);
                    result.expirydate = reader["expirydate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["expirydate"]);
                    result.amenddate = reader["amenddate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["amenddate"]);
                    result.amendreason = reader["amendreason"] == DBNull.Value ? "" : Convert.ToString(reader["amendreason"]);


                    result.customfields = new List<string>();
                    foreach (var cfield in items.customfields)
                    {
                        string svalue = "";
                        if (cfield.field.cftype == "bigint")
                        {
                            long value = reader["mfg_" + cfield.field.cfname] == DBNull.Value ? 0 : Convert.ToInt64(reader["mfg_" + cfield.field.cfname]);
                            if (refdatalist != null &&
                                refdatalist.Exists(x => x.type == cfield.field.cfref &&
                                x.id == value))
                            {
                                svalue = refdatalist.Find(x => x.type == cfield.field.cfref &&
                                x.id == value).name;
                            }
                        }
                        else if (cfield.field.cftype.StartsWith("varchar"))
                        {
                            svalue = reader["mfg_" + cfield.field.cfname] == DBNull.Value ? "" : Convert.ToString(reader["mfg_" + cfield.field.cfname]);
                        }
                        result.customfields.Add(svalue);
                    }
                    response.Add(result);
                }
            }
            return response;
        }

        internal List<MfgReportServiceController.CustomStockCriteria.InvoiceResult> SearchMfgUANInvoice(MfgReportServiceController.CustomStockCriteria items)
        {
            string query = @"with grndetails as
(
	select lrentry.lrentrydate,lrentry.lrentryno,
case when lrinvoice.locationid=0 then lrentry.entrylocationid else lrinvoice.locationid end locationid,
lrinvoice.invoiceno,lrinvoice.invoiceentryno,lrinvoice.invoicedate,lrinvoiceitems.uan,lrinvoiceitems.buyingqty,
lrinvoiceitems.actualqty,lrinvoiceitems.amount,purchaseorderitems.oqty,lrinvoiceitems.buyingprice,lrinvoiceitems.mrp,
supplier.name supplier,purchaseorder.pono,purchaseorder.podate,
lrinvoicereturnitems.qty debitqty,lrinvoicereturnitems.amount debitamount from lrinvoice
inner join lrentry on lrentry.id = lrid
inner join suppliercompany on suppliercompany.id = lrinvoice.suppliercompanyid
inner join supplier on supplier.id = suppliercompany.supplierid
inner join lrinvoiceitems on lrinvoiceitems.lrinvoiceid = lrinvoice.id and lrinvoiceitems.isactive=true
left join purchaseorder on purchaseorder.id = lrinvoice.poid
left join purchaseorderitems on purchaseorderitems.purchaseorderid = lrinvoice.poid  and lrinvoiceitems.uan = purchaseorderitems.gln
			and purchaseorderitems.isactive=true
left outer join lrinvoicereturn on lrinvoicereturn.lrinvoiceid = lrinvoice.id and lrinvoicereturn.isactive=true and lrinvoicereturn.returntype<10
left outer join lrinvoicereturnitems on lrinvoicereturnitems.lrinvoicereturnid = lrinvoicereturn.id
	and lrinvoicereturnitems.uan = lrinvoiceitems.uan and lrinvoicereturnitems.isactive=true
where lrentry.lrentrydate>=@fromdate and lrentry.lrentrydate<@todate and lrinvoice.isactive=true
and lrinvoice.pono = @pono 
and (case when lrinvoice.locationid=0 then lrentry.entrylocationid else lrinvoice.locationid end) = @locationid
), alluid
as
(
    select grndetails.*,mfgdesign.code,
    schemelist->>'uid' uid,cast(schemelist->>'sizeid' as bigint) sizeid,
    cast(schemelist->>'mrp' as numeric) mrp,
    schemelist->>'code' dname,schemelist->>'prdsku' prdsku,
    cast(schemelist->>'colourid' as bigint) colourid,
    mfgdesign.productid,mfgdesign.prddivisionid,mfgdesign.classid {0}
    from mfgdesign 
    LEFT JOIN LATERAL
    json_array_elements (cast(mfgdesign.sizegroup->>'items' as json)) schemelist ON true
    inner join grndetails  on grndetails.uan = schemelist->>'uid'
    where mfgdesign.isactive=true
)
select * from alluid ";

            


            List<string> additionafields = new List<string>();
            if (items.customfields != null)
                additionafields = items.customfields.ConvertAll(x => "mfg_" + x.field.cfname);
            query = String.Format(query, additionafields.Count == 0 ? "" : "," + (string.Join(",", additionafields)));
            var reffields = items.customfields.FindAll(x => !string.IsNullOrEmpty(x.field.cfref));
            List<ReferenceItem> refdatalist = null;
            if (reffields.Count > 0)
            {
                ReferencelistDataService referencelistDataService = new ReferencelistDataService() { Context = this.Context };
                refdatalist = referencelistDataService.SearchList(new ReferencelistCreteria()
                {
                    types = reffields.ConvertAll(x => x.field.cfref)
                });
            }
            var response = new List<MfgReportServiceController.CustomStockCriteria.InvoiceResult>();
            var param = new SortedList<string, object>();
            param.Add("fromdate", items.fromdate);
            param.Add("pono", string.IsNullOrEmpty(items.pono) ? "":items.pono);
            param.Add("todate", items.todate.AddDays(1));
            param.Add("locationid", items.locationid);

            if (string.IsNullOrEmpty(items.pono))
            {
                query = query.Replace("and lrinvoice.pono = @pono", "");
            }

            if (items.locationid == 0)
                query = query.Replace(@"and (case when lrinvoice.locationid=0 then lrentry.entrylocationid else lrinvoice.locationid) = @locationid", "");


            using (var reader = ExecuteReader(query, param, 10000))
            {
                while (reader.Read())
                {
                    MfgReportServiceController.CustomStockCriteria.InvoiceResult result = new MfgReportServiceController.CustomStockCriteria.InvoiceResult();
                    result.locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                    result.prddivisionid = reader["prddivisionid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["prddivisionid"]);
                    result.productid = reader["productid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productid"]);
                    result.classid = reader["classid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["classid"]);
                    result.colourid = reader["colourid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["colourid"]);
                    result.sizeid = reader["sizeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sizeid"]);

                    result.designname = Convert.ToString(reader["dname"]);
                    result.design = Convert.ToString(reader["code"]);
                    result.uid = Convert.ToString(reader["uid"]);

                    //schemelist->>'code' dname,schemelist->>'prdsku' prdsku,
                    result.mrp = Convert.ToDecimal(reader["mrp"]);
                    result.invoicedate = reader["invoicedate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["invoicedate"]);
                    result.invoiceno = reader["invoiceno"] == DBNull.Value ? "" : Convert.ToString(reader["invoiceno"]);
                    result.entryno = reader["lrentryno"] == DBNull.Value ? "" : Convert.ToString(reader["lrentryno"]);
                    result.entrydate = reader["lrentrydate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["lrentrydate"]);

                    result.grnqty = reader["buyingqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["buyingqty"]);
                    result.grnvalue = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"]);
                    if(reader["actualqty"] != DBNull.Value)
                        result.actqty = reader["actualqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["actualqty"]);
                    else
                        result.actqty = reader["buyingqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["buyingqty"]);

                    result.orderqty = reader["oqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["oqty"]);
                    result.buyingprice = reader["buyingprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["buyingprice"]);
                    result.pono = reader["pono"] == DBNull.Value ? "" : Convert.ToString(reader["pono"]);
                    result.podate = reader["podate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["podate"]);
                    result.debitqty = reader["debitqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["debitqty"]);
                    result.debitvalue = reader["debitamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["debitamount"]);
                    result.supplier = reader["supplier"] == DBNull.Value ? "" : Convert.ToString(reader["supplier"]);

                    result.customfields = new List<string>();
                    foreach (var cfield in items.customfields)
                    {
                        string svalue = "";
                        if (cfield.field.cftype == "bigint")
                        {
                            long value = reader["mfg_" + cfield.field.cfname] == DBNull.Value ? 0 : Convert.ToInt64(reader["mfg_" + cfield.field.cfname]);
                            if (refdatalist != null &&
                                refdatalist.Exists(x => x.type == cfield.field.cfref &&
                                x.id == value))
                            {
                                svalue = refdatalist.Find(x => x.type == cfield.field.cfref &&
                                x.id == value).name;
                            }
                        }
                        else if (cfield.field.cftype.StartsWith("varchar"))
                        {
                            svalue = reader["mfg_" + cfield.field.cfname] == DBNull.Value ? "" : Convert.ToString(reader["mfg_" + cfield.field.cfname]);
                        }
                        result.customfields.Add(svalue);
                    }
                    response.Add(result);
                }
            }
            return response;
        }

        internal List<MfgReportServiceController.CustomStockCriteria.BundleResult> SearchMfgUANBundleReport(MfgReportServiceController.CustomStockCriteria items)
        {
            string query = @"
with grndetails as
(
    select 
    bundles.code,bundles.status,
    fromlocationid,tolocationid,packedby, packedon, receivedon,receivedby, 
    schemelist->>'serialno' gln,
    (schemelist ->> 'qty') qty,
    (schemelist ->> 'journalqty') jqty,
    (schemelist ->> 'damagedqty') dqty,
    (schemelist ->> 'rqty') rqty,
    (schemelist ->> 'aqty') aqty,
    (schemelist ->> 'iqty') iqty from bundles 
    LEFT JOIN LATERAL 
    json_array_elements (case when bundles.items is null or bundles.items::text ='null' then '[]'::json else bundles.items :: json end) schemelist ON true
    and bundles.isactive=true {1}
    where bundles.packedon>=@fromdate and bundles.packedon<@todate and bundles.isactive=true
), alluid
as
(
    select grndetails.*,mfgdesign.code designcode,
    schemelist->>'uid' uid,cast(schemelist->>'sizeid' as bigint) sizeid,
    cast(schemelist->>'mrp' as numeric) mrp,
    schemelist->>'code' dname,schemelist->>'prdsku' prdsku,
    cast(schemelist->>'colourid' as bigint) colourid,
    mfgdesign.productid,mfgdesign.prddivisionid,mfgdesign.classid {0}
    from mfgdesign 
    LEFT JOIN LATERAL
    json_array_elements (cast(mfgdesign.sizegroup->>'items' as json)) schemelist ON true
    inner join grndetails  on grndetails.gln = schemelist->>'uid'
    where mfgdesign.isactive=true
)
select * from alluid ";

            string additionalcondition = "";

            if (items.posearchmode == 0)
                additionalcondition = " and status<=0";
            else if (items.posearchmode != 10)
                additionalcondition = " and status=" + items.posearchmode;

            List<string> additionafields = new List<string>();
            if (items.customfields != null)
                additionafields = items.customfields.ConvertAll(x => "mfg_" + x.field.cfname);
            query = String.Format(query, additionafields.Count == 0 ? "" : "," + (string.Join(",", additionafields)), additionalcondition);
            var reffields = items.customfields.FindAll(x => !string.IsNullOrEmpty(x.field.cfref));
            List<ReferenceItem> refdatalist = null;
            if (reffields.Count > 0)
            {
                ReferencelistDataService referencelistDataService = new ReferencelistDataService() { Context = this.Context };
                refdatalist = referencelistDataService.SearchList(new ReferencelistCreteria()
                {
                    types = reffields.ConvertAll(x => x.field.cfref)
                });
            }
            var response = new List<MfgReportServiceController.CustomStockCriteria.BundleResult>();

            List<string> conditions = new List<string>();
            if (items.productid > 0)
                conditions.Add("alluid.productid=@productid");
            if (!string.IsNullOrEmpty(items.uid))
                conditions.Add("alluid.uid=@uid");
            if (!string.IsNullOrEmpty(items.designid))
                conditions.Add("alluid.code=@designid");
            if (items.locationid > 0)
                conditions.Add("fromlocationid=@locationid");
            if (items.classificationid > 0)
                conditions.Add("alluid.classid=@classificationid");
            if (items.prddivisionid > 0)
                conditions.Add("alluid.prddivisionid=@prddivisionid");

            if (conditions.Count > 0)
                query += " where " + String.Join(" and ", conditions);

            var param = new SortedList<string, object>();
            param.Add("locationid", items.locationid);
            param.Add("classificationid", items.classificationid);
            param.Add("prddivisionid", items.prddivisionid);
            param.Add("productid", items.productid);
            param.Add("uid", string.IsNullOrEmpty(items.uid) ? "" : items.uid);
            param.Add("designid", string.IsNullOrEmpty(items.designid) ? "" : items.designid);
            param.Add("fromdate", items.fromdate.Date);
            param.Add("todate", items.todate.AddDays(1).Date);


            using (var reader = ExecuteReader(query, param, 10000))
            {
                while (reader.Read())
                {
                    MfgReportServiceController.CustomStockCriteria.BundleResult result = new MfgReportServiceController.CustomStockCriteria.BundleResult();

                    result.prddivisionid = reader["prddivisionid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["prddivisionid"]);
                    result.productid = reader["productid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productid"]);
                    result.classid = reader["classid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["classid"]);
                    result.colourid = reader["colourid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["colourid"]);
                    result.sizeid = reader["sizeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sizeid"]);

                    result.designname = Convert.ToString(reader["dname"]);
                    result.design = Convert.ToString(reader["designcode"]);
                    result.uid = Convert.ToString(reader["uid"]);

                    //schemelist->>'code' dname,schemelist->>'prdsku' prdsku,
                    result.mrp = Convert.ToDecimal(reader["mrp"]);

                    result.code = reader["code"] == DBNull.Value ? "" : Convert.ToString(reader["code"]);
                    result.packedon = reader["packedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["packedon"]);
                    result.receivedon = reader["receivedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["receivedon"]);
                    result.status = reader["status"] == DBNull.Value ? 0 : Convert.ToInt32(reader["status"]);

                    result.fromlocationid = reader["fromlocationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["fromlocationid"]);
                    result.tolocationid = reader["tolocationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["tolocationid"]);

                    result.packedby = reader["packedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["packedby"]);
                    result.receivedby = reader["receivedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["receivedby"]);

                    result.requestqty = reader["qty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["qty"]);
                    result.requestqty += reader["jqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["jqty"]);
                    result.issuedqty = reader["iqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["iqty"]);
                    result.acceptedqty = reader["aqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["aqty"]);
                    result.rejectedqty = reader["rqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["rqty"]);
                    result.skippedqty = reader["jqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["jqty"]);

                    result.customfields = new List<string>();
                    foreach (var cfield in items.customfields)
                    {
                        string svalue = "";
                        if (cfield.field.cftype == "bigint")
                        {
                            long value = reader["mfg_" + cfield.field.cfname] == DBNull.Value ? 0 : Convert.ToInt64(reader["mfg_" + cfield.field.cfname]);
                            if (refdatalist != null &&
                                refdatalist.Exists(x => x.type == cfield.field.cfref &&
                                x.id == value))
                            {
                                svalue = refdatalist.Find(x => x.type == cfield.field.cfref &&
                                x.id == value).name;
                            }
                        }
                        else if (cfield.field.cftype.StartsWith("varchar"))
                        {
                            svalue = reader["mfg_" + cfield.field.cfname] == DBNull.Value ? "" : Convert.ToString(reader["mfg_" + cfield.field.cfname]);
                        }
                        result.customfields.Add(svalue);
                    }
                    response.Add(result);
                }
            }
            return response;
        }


        public List<WorkorderSampleStatus> SearchWorkorderSampleStatus(WorkorderSampleStatusRequest request)
        {
            string query = @"with activeworkorders 
                    as
                    (
                        select  mfgworkorderinbox.mfgworkorderid,wodate,mfgworkorder.customerid,mfgworkorderitems.designname,
                        mfgworkorder.supplierid,salesorderid,mfgsampleorderid,salesorder.sono,salesorder.sodate::date sodate,mfgworkorder.workorderno,
                        sum(mfgworkorderitems.outqty) outqty,sum(mfgworkorderitems.inqty) inqty,sobookno
                        from mfgworkorderinbox
                        inner join mfgworkorder on mfgworkorder.id = mfgworkorderid
                        left outer join mfgworkorderitems on mfgworkorderitems.mfgworkorderid = mfgworkorder.id
                        left outer join Mfgsampleorder on Mfgsampleorder.id = mfgsampleorderid
                        left outer join salesorder on salesorder.id = salesorderid
                        where mfgworkorderinbox.statusid != 9
                        and salesorderid>=0 and mfgsampleorderid>0
                        group by  mfgworkorderinbox.mfgworkorderid,salesorderid,mfgsampleorderid,salesorder.sono,salesorder.sodate::date,
                        wodate,mfgworkorder.customerid,mfgworkorder.supplierid,mfgworkorder.workorderno,mfgworkorderitems.designname,sobookno
                    ),jobworklast as
					(
						select max(id) id,activeworkorders.mfgworkorderid from activeworkorders
						inner join mfgjoborder on mfgjoborder.mfgworkorderid = activeworkorders.mfgworkorderid and mfgjoborder.isactive=true
						group by activeworkorders.mfgworkorderid
					),salesorderag as
                    (
                    select salesorderitems.salesorderid,sum(oqty-cqty) oqty,sum(bqty) rqty,sum(price*(oqty-cqty)) amount,avg(price) price from activeworkorders
                    inner join salesorderitems on salesorderitems.salesorderid = activeworkorders.salesorderid
                    where salesorderitems.isactive=true
                    group by salesorderitems.salesorderid
                    ),billdetails as
                    (
	                    select activeworkorders.salesorderid,
						string_agg(DISTINCT bill.billno, ', ') AS billno,
						sum(billitems.qty) bqty from activeworkorders 
	                    inner join bill on bill.billedsalesorderno = activeworkorders.sono
						inner join billitems on billitems.billid = bill.id and billitems.isactive=true
						group by activeworkorders.salesorderid
                    )
                    select activeworkorders.*,salesorderag.*,billdetails.*,division.name division,
					mfgjoborder.stagename,mfgjoborder.inwardjob,mfgjoborder.inwardgoods,
					mfgjoborder.joborderno,mfgjoborder.joborderdate,
                    mfgsampleorder.samplename,mfgsampleorder.designno,mfgsampleorder.entryno,prdform.name productform,
                    mfgsampleorder.preparedby,mfgsampleorder.agentid,customer.name customername
                    from activeworkorders
                    left outer join mfgsampleorder on mfgsampleorder.id = mfgsampleorderid
                    left outer join billdetails on billdetails.salesorderid = activeworkorders.salesorderid
                    left outer join salesorderag on salesorderag.salesorderid = activeworkorders.salesorderid
                    left outer join customer on customer.id = activeworkorders.customerid
					left outer join jobworklast on jobworklast.mfgworkorderid = activeworkorders.mfgworkorderid
					left outer join mfgjoborder on mfgjoborder.id = jobworklast.id
                    left outer join referencelist division on division.id = prddivisionid
                    left outer join referencelist prdform on prdform.id = productformid
                    where activeworkorders.mfgworkorderid>0";

            QueryBuilder builder = new QueryBuilder();
            builder.CurrentQuery = query;
            if (request.CustomerID > 0)
                builder.AddParameter("activeworkorders.customerid", "activeworkorders.customerid", request.CustomerID, "=");

            if (request.Divisionid > 0)
                builder.AddParameter("prddivisionid", "prddivisionid", request.Divisionid, "=");

            if(request.fromdate.Year>0)
            {
                builder.AddParameter("wodate", "fromwodate", request.fromdate, ">=");
                builder.AddParameter("wodate", "towodate", request.todate.AddDays(1), "<");
            }


            List<WorkorderSampleStatus> workorderSampleStatuses = new List<WorkorderSampleStatus>();
            using (var conn = CreateConnection())
            {
                var command = builder.GetCommand(conn);
                conn.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        WorkorderSampleStatus result = new WorkorderSampleStatus();

                        result.designname = reader["designname"] == DBNull.Value ? string.Empty : Convert.ToString(reader["designname"]);
                        result.sono = reader["sono"] == DBNull.Value ? string.Empty : Convert.ToString(reader["sono"]);
                        result.billno = reader["billno"] == DBNull.Value ? string.Empty : Convert.ToString(reader["billno"]);
                        result.workorderno = reader["workorderno"] == DBNull.Value ? string.Empty : Convert.ToString(reader["workorderno"]);
                        result.division = reader["division"] == DBNull.Value ? string.Empty : Convert.ToString(reader["division"]);
                        result.samplename = reader["samplename"] == DBNull.Value ? string.Empty : Convert.ToString(reader["samplename"]);
                        result.designno = reader["designno"] == DBNull.Value ? string.Empty : Convert.ToString(reader["designno"]);
                        result.entryno = reader["entryno"] == DBNull.Value ? string.Empty : Convert.ToString(reader["entryno"]);
                        result.productform = reader["productform"] == DBNull.Value ? string.Empty : Convert.ToString(reader["productform"]);

                        result.mfgworkorderid = reader["mfgworkorderid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["mfgworkorderid"]);
                        result.salesorderid = reader["salesorderid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["salesorderid"]);
                        result.mfgsampleorderid = reader["mfgsampleorderid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["mfgsampleorderid"]);

                        result.wodate = reader["wodate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["wodate"]);
                        result.sodate = reader["sodate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["sodate"]);

                        result.outqty = reader["outqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["outqty"]);
                        result.inqty = reader["inqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["inqty"]);
                        result.oqty = reader["oqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["oqty"]);
                        result.rqty = reader["rqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["rqty"]);
                        result.bqty = reader["bqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["bqty"]);

                        result.amount = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"]);

                        result.preparedby = reader["preparedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["preparedby"]);
                        result.agentid = reader["agentid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["agentid"]);
                        result.price = reader["price"] == DBNull.Value ? 0 : Convert.ToInt64(reader["price"]);
                        result.customername = reader["customername"] == DBNull.Value ? string.Empty : Convert.ToString(reader["customername"]);
                        result.sobookno = reader["sobookno"] == DBNull.Value ? string.Empty : Convert.ToString(reader["sobookno"]);


                        result.stagename = reader["stagename"] == DBNull.Value ? string.Empty : Convert.ToString(reader["stagename"]);
                        result.joborderno = reader["joborderno"] == DBNull.Value ? string.Empty : Convert.ToString(reader["joborderno"]);
                        result.inwardjob = reader["inwardjob"] == DBNull.Value ? false : Convert.ToBoolean(reader["inwardjob"]);
                        result.inwardgoods = reader["inwardgoods"] == DBNull.Value ? false : Convert.ToBoolean(reader["inwardgoods"]);
                        result.joborderdate = reader["joborderdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["joborderdate"]);

                        if (!string.IsNullOrEmpty(result.billno))
                            result.status = "COMPLETED";
                        else if (result.inqty > 0)
                            result.status = result.inqty < result.outqty ? "GRN" : "DISPATCH";
                        else if (!string.IsNullOrEmpty(result.stagename))
                            result.status = result.stagename.ToUpper();
                        else
                            result.status = "PLANNING";

                        workorderSampleStatuses.Add(result);
                    }
                }
                conn.Close();
            }
            return workorderSampleStatuses;
        }
        public class WorkorderSampleStatusRequest:BaseEntity
        {
            public long CustomerID { get; set; }
            public long Divisionid { get; set; }
            public DateTime fromdate { get; set; }
            public DateTime todate { get; set; }
        }
        public class WorkorderSampleStatus
        {
            public string designname { get; set; }
            public string sono { get; set; }
            public string billno { get; set; }
            public string workorderno { get; set; }
            public string division { get; set; }
            public string samplename { get; set; }
            public string designno { get; set; }
            public string entryno { get; set; }
            public string productform { get; set; }
            public long mfgworkorderid { get; set; }
            public long salesorderid { get; set; }
            public long mfgsampleorderid { get; set; }
            public DateTime wodate { get; set; }
            public decimal outqty { get; set; }
            public decimal inqty { get; set; }
            public decimal oqty { get; set; }
            public decimal rqty { get; set; }
            public decimal amount { get; set; }
            public string stagename { get; set; }
            public string joborderno { get; set; }
            public bool inwardjob { get; set; }
            public bool inwardgoods { get; set; }
            public DateTime joborderdate { get; set; }
            public string status { get; set; }
            public DateTime sodate { get; set; }
            public long preparedby { get; set; }
            public long agentid { get; set; }
            public decimal price { get; set; }
            public string customername { get; set; }
            public string sobookno { get; set; }
            public decimal balance
            {
                get
                {
                    return oqty - rqty;
                }
            }
            public int sodue
            {
                get
                {
                    if (sodate != DateTime.MinValue)
                        return Convert.ToInt32(DateTime.Today.Subtract(sodate).TotalDays);
                    return 0;
                }
            }
            public int wodue
            {
                get
                {
                    if (wodate != DateTime.MinValue)
                        return Convert.ToInt32(DateTime.Today.Subtract(wodate).TotalDays);
                    return 0;
                }
            }

            public decimal bqty { get; internal set; }
        }

        public class Dashboard
        {
            public List<Attribute> AgentData { get; set; }
            public List<Attribute> ProductData { get; set; }
            public List<Attribute> SizeData { get; set; }
            public List<Attribute> BrandData { get; set; }
            public List<Classification> ClassData { get; set; }
            public class Classification
            {
                public string name { get; set; }
                public long id { get; set; }
                public int so_pending { get; set; }
                public int so_today { get; set; }
                public int wip { get; set; }
                public int sale { get; set; }
            }

            public class AtributeData
            {
                long id { get; set; }
                public string name { get; set; }
                public List<BrandClassifcation> items { get; set; }
                public class BrandClassifcation
                {
                    public long id { get; set; }
                    public int so_pending { get; set; }
                    public int so_today { get; set; }
                    public int wip { get; set; }
                    public int sale { get; set; }
                }
            }
        }


        public class StockTrace
        {
            public decimal qty { get; set; }
            public DateTime invoicedate { get; set; }
            public DateTime wodate { get; set; }
            public string entryno { get; set; }
            public string workorderno { get; set; }
            public string uid { get; set; }
            public string locationname { get; set; }
        }


        public class SalesTrace
        {
            public decimal qty { get; set; }
            public DateTime invoicedate { get; set; }
            public DateTime wodate { get; set; }
            public DateTime billdate { get; set; }
            public string entryno { get; set; }
            public string billno { get; set; }
            public string customername { get; set; }
            public string workorderno { get; set; }
            public string uid { get; set; }
            public string locationname { get; set; }
        }
        public class GRNTrace
        {
            public decimal qty { get; set; }
            public DateTime invoicedate { get; set; }
            public DateTime wodate { get; set; }
            public string entryno { get; set; }
            public string workorderno { get; set; }
            public string uid { get; set; }
            public string size { get; set; }
        }
        public class WIPTrace
        {
            public decimal ordered { get; set; }
            public decimal delivered { get; set; }
            public string workorderno { get; set; }
            public string uid { get; set; }
            public string size { get; set; }
            public DateTime wodate { get; set; }
        }
        public class LocationStockItem : StockItem
        {
            public long locationid { get; set; }
        }
        public class StockItem
        {
            public string name { get; set; }
            public long cityid { get; set; }
            public long customerid { get; set; }
            public long colourid { get; set; }
            public long sizeid { get; set; }
            public decimal stock { get; set; }
            public decimal pqty { get; set; }
            public string serialno { get; set; }
            public string designid { get; set; }
            public string designcode { get; set; }
            public string sono { get; set; }
        }
    }
}
