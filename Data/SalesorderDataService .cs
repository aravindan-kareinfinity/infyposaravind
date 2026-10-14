using InfyPOS.Common;
using InfyPOS.Models;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;

using System.Linq;

namespace InfyPOS.Data
{
    public class SalesorderDataService : SAASData
    {

        //sales order billqty sync

        /*
         WITH DIFFSOLIST
AS
(
SELECT SALESORDERITEMS.ID,SALESORDERITEMS.OQTY,SALESORDERITEMS.CQTY,SALESORDERITEMS.BQTY FROM SALESORDERITEMS
INNER JOIN SALESORDER ON SALESORDER.ID = SALESORDERID
WHERE SALESORDER.SONO IN ('20976')
AND SALESORDERITEMS.OQTY-(SALESORDERITEMS.CQTY+SALESORDERITEMS.BQTY) != 0
)
--SELECT * FROM DIFFSOLIST
UPDATE SALESORDERITEMS SET CQTY=(SALESORDERITEMS.OQTY-SALESORDERITEMS.BQTY) FROM DIFFSOLIST
WHERE DIFFSOLIST.ID = SALESORDERITEMS.ID
AND SALESORDERITEMS.OQTY-(SALESORDERITEMS.BQTY+SALESORDERITEMS.CQTY)>0

            update  salesorderitems set bqty = oqty-cqty  where bqty>(oqty-cqty) 
            with allsalesorderitems
            as
            (
                select 
	            attributes->'so',
	            cast(schemelist->>'id' as bigint) salesorderitemid,cast(schemelist->>'qty' as numeric) qty 
	            from billitems
                inner join bill on bill.id = billid
                LEFT JOIN LATERAL
                json_array_elements (attributes->'so') schemelist ON true
                where bill.isactive=true and billitems.isactive=true and bill.billtype != 8 and
	            json_typeof(attributes->'so')='array'
            ),sumorderitems
            as
            (
	            select salesorderitemid,sum(qty) billedqty from allsalesorderitems 
	            group by salesorderitemid
            ),billqtysync
            as
            (
                select salesorderitems.id,salesorderitems.bqty,sumorderitems.billedqty from salesorder
                inner join salesorderitems on salesorderitems.salesorderid = salesorder.id
                inner join sumorderitems on sumorderitems.salesorderitemid = salesorderitems.id
                where salesorderitems.bqty != sumorderitems.billedqty
            )
            update salesorderitems set bqty = billedqty from billqtysync
            where salesorderitems.id = billqtysync.id 
*/


        //PRICE CHANGE
        /*
         * with alluid
as
(
select mfgdesign.code,schemelist->>'sizegroup' sizegroup,schemelist->>'uid' uid,cast(schemelist->>'sizeid' as bigint) sizeid,
cast(schemelist->>'wsrate' as numeric) wsrate,cast(schemelist->>'dealerprice' as numeric) dealerprice,
cast(schemelist->>'salerate' as numeric) salerate,cast(schemelist->>'mrp' as numeric) mrp,
schemelist->>'code' dname,cast(schemelist->>'colourid' as bigint) colourid,
	case when position('/' in schemelist->>'code') = 0 then '' else
	substring(schemelist->>'code',position('/' in schemelist->>'code')+1)
	end colourcode
	from mfgdesign 
LEFT JOIN LATERAL
json_array_elements (cast(mfgdesign.sizegroup->>'items' as json)) schemelist ON true
	where isactive=true 
)
	
	SELECT CUSTOMER.PRICESOURCE, SALESORDERITEMS.SALESORDERID,SALESORDERITEMS.ID,dname,
	ALLUID.WSRATE,ALLUID.dealerprice,ALLUID.MRP,SALESORDERITEMS.PRICE,SALESORDERITEMS.MRP FROM ALLUID
	INNER JOIN SALESORDERITEMS ON SALESORDERITEMS.UID = ALLUID.UID
	INNER JOIN SALESORDER ON SALESORDER.ID = SALESORDERID
	INNER JOIN CUSTOMER ON CUSTOMER.ID = CUSTOMERID
	WHERE  ALLUID.MRP != SALESORDERITEMS.MRP 
	AND SALESORDER.SODATE>='2022-12-01' AND ALLUID.WSRATE>0
	ORDER BY SALESORDERITEMS.SALESORDERID 
*/

        const string SQL_Search = @"
        select Salesorder.id, sodate, Salesorder.remarks, expectedate, sono, customerid, termsandcondition, 
        Salesorder.isactive, sizegroup,sosourcetype,salesmenid,sobookno,customerpo,sobookqty,sobookdate,
        Salesorder.createdby, Salesorder.modifiedby, Salesorder.version,tolerance, 
		Salesorder.createdon, Salesorder.modifiedon, Salesorder.locationid,Salesorder.status,
		deliverylocationid,mode,Salesorder.agentid,freight,actionon,actionby,Salesorder.companyid,
        plannedqty,shiptocustomerid,discountpercentage,hasshipto,employee.id employeeid,
		customer.name customername,referencelist.name cityname,customer.mobileno,salesorder.customerstateid,
        agent.name agentname,totalsetqty,totalqty,orderformimageid,soracknumber,smapprovedby,smapprovedon
		from Salesorder
		inner join customer on customer.id = customerid
		left outer join referencelist on referencelist.id = customer.cityid 
        left outer join agent on agent.id = Salesorder.agentid
        left outer join users on users.id = salesorder.createdby
        left outer join employee on employee.id = users.employeeid
        where Salesorder.id=@id";

        const string SQL_PipeLineStock = @"select productid,designname,colourid,size,sizecount,
products.name,colour.name,outqty-received as stockqty,mfgworkorderitems.designitemname from mfgworkorderitems
inner join mfgworkorder on mfgworkorder.id = mfgworkorderitems.mfgworkorderid
left outer join products on products.id = productid
left outer join referencelist colour on colour.id = colourid
where 
mfgworkorder.wodate>=@wofromdate and mfgworkorder.wodate<=@wotodate and 
mfgworkorder.isactive = true and mfgworkorderitems.isactive = true and outqty-received>0";

        const string SQL_SOSummary = @"select Salesorder.id,sodate::date sodate,sono,customer.name customername,
                                    shipto.name shiptoname,agent.name agentname,
                                    Salesorderitems.productid,products.name productname,city.name cityname,sono,
                                    sum(oqty::numeric-(cqty::numeric+bqty+rqty)) pendingqty, sum(bqty) billedqty,
                                    sum(pqty) plannedqty,salesorder.customerid,sum(cqty) cancelledqty,sum(rqty) removedqty,
                                    STRING_AGG(cast(Salesorderitems.id as varchar),',') Salesorderitemsids
                                    {0}
                                    from Salesorder 
									inner join Salesorderitems on Salesorderitems.salesorderid = Salesorder.id
									inner join customer on customer.id = customerid
                                    left outer join customer shipto on shipto.id = Salesorder.shiptocustomerid
									left outer join agent on agent.id = Salesorder.agentid
                                    left outer join items on items.id = Salesorderitems.itemid
									left outer join products on products.id = Salesorderitems.productid
									left outer join referencelist city on city.id = customer.cityid
                                    {1}
                                    where salesorder.isactive = true and Salesorderitems.isactive = true 
									group by Salesorder.id,customer.name,shipto.name,agent.name,products.name,sono,
                                    salesorder.customerid,city.name,sodate::date,Salesorderitems.productid
                                    {2}";

        const string SQL_SOUISummary = @"select Salesorder.id,employee.id createdby,sodate::date sodate,sono,shiptocustomerid,
                                    Salesorderitems.productid,customer.cityid,shipto.cityid scityid,sono,Salesorder.agentid,
                                    Salesorder.salesmenid,sum(oqty) orderqty,
                                    sum(oqty::numeric-(cqty::numeric+bqty+rqty)) pendingqty, sum(bqty) billedqty,Salesorder.remarks,
                                    Salesorder.sobookno,customerpo,sobookqty,
                                    sum(pqty) plannedqty,salesorder.customerid,sum(cqty) cancelledqty,sum(rqty) removedqty,
                                    Salesorderitems.designitemcode,STRING_AGG(cast(Salesorderitems.sizeid as varchar)||'~'||Salesorderitems.oqty::int||'~'||Salesorderitems.pqty::int||'~'||Salesorderitems.rqty::int||'~'||Salesorderitems.cqty::int||'~'||Salesorderitems.bqty::int,',') oqty,
                                    STRING_AGG(cast(Salesorderitems.id as varchar),',') Salesorderitemsids
                                    {0}
                                    from Salesorder 
        	                        inner join Salesorderitems on Salesorderitems.salesorderid = Salesorder.id
        	                        inner join customer on customer.id = customerid
                                    left outer join users on users.id = Salesorder.createdby
                                    left outer join employee on employee.id = users.employeeid
                                    left outer join customer shipto on shipto.id = Salesorder.shiptocustomerid
                                    left outer join items on items.id = Salesorderitems.itemid
        	                        left outer join products on products.id = Salesorderitems.productid
        	                        left outer join referencelist city on city.id = customer.cityid
                                    {1}
                                    where salesorder.isactive = true and Salesorderitems.isactive = true 
        	                        group by Salesorder.id,employee.id,sono,salesorder.customerid,Salesorder.shiptocustomerid,Salesorder.agentid,Salesorder.sobookno,customerpo,
                                    Salesorder.sobookqty,sodate::date,Salesorderitems.productid,shipto.cityid,customer.cityid,designitemcode,Salesorder.remarks,Salesorder.salesmenid
                                    {2}";

        const string SQL_SOSizeSummary = @"select Salesorder.id,sodate::date sodate,sono,customer.name customername,
                                    shipto.name shiptoname,agent.name agentname,brand.name brand,employee.name salesmen,
                                    Salesorderitems.productid,products.name productname,city.name cityname,sono,
                                    (oqty::numeric-(cqty::numeric+bqty+rqty)) pendingqty, (bqty) billedqty,
                                    (pqty) plannedqty,salesorder.customerid,(cqty) cancelledqty,(rqty) removedqty,
                                    Salesorderitems.sizeid,oqty orderqty,Salesorderitems.sizegroup,Salesorder.sobookno,
                                    Salesorderitems.id Salesorderitemsids,users.employeeid,Salesorder.remarks
                                    {0}
                                    from Salesorder 
									inner join Salesorderitems on Salesorderitems.salesorderid = Salesorder.id
									inner join customer on customer.id = customerid
                                    left outer join employee on employee.id = salesmenid
                                    left outer join customer shipto on shipto.id = Salesorder.shiptocustomerid
									left outer join agent on agent.id = Salesorder.agentid
                                    left outer join items on items.id = Salesorderitems.itemid
                                    left outer join brand on brand.id = items.brandid
									left outer join products on products.id = Salesorderitems.productid
									left outer join referencelist city on city.id = customer.cityid
                                    left outer join users on users.id = salesorder.createdby
                                    {1}
                                    where salesorder.isactive = true and Salesorderitems.isactive = true ";

        internal MFG_SizeGroup<MFG_SizeGroup_Stock> SearchSizeGroupSales(SalesorderitemsCreteria _reportcreteria)
        {
            var report = new MFG_SizeGroup<MFG_SizeGroup_Stock>();
            List<MFG_SizeGroup_Stock> lst = new List<MFG_SizeGroup_Stock>();
            report.items = lst;
            NpgsqlConnection conn = CreateConnection();
            try
            {
                if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_possizelist != null &&
                    InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_possizelist != "")
                {
                    report.sizenames = new List<string>(InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_possizelist.Split(','));
                }
                conn.Open();
                NpgsqlCommand command = null;

                QueryBuilder qb = new QueryBuilder();

                ReferencelistDataService referencelistDataService = new ReferencelistDataService() { Context = this.Context };
                var sizegroups = referencelistDataService.SearchSizeGroups();
                qb.CurrentQuery = UpdateQueryForHyper(@"select customer.name customername,citylist.name city,bill.isinterstatesale,
                                    colours.name colourname,products.name productname,
                                    mfg_uid_info.dname groupkey,brand.name brandname,
                                    sum(billitems.qty) salesqty,STRING_AGG(cast(billitems.attributes->'size' as varchar)||':'||cast(billitems.qty as numeric(8,2)),'~') qtylist
									from billitems
									inner join bill on bill.id = billitems.billid
									left outer join customer on customer.id= bill.customerid
									inner join salablegoods on salablegoods.id = billitems.salablegoodsid
                                    inner join stock on stock.id = salablegoods.stockid
                                    left outer join mfg_uid_info on mfg_uid_info.uid = stock.serialno
									left outer join products on products.id = mfg_uid_info.productid
                                    left outer join brand on brand.id = mfg_uid_info.brandid
									left outer join referencelist colours on colours.id = mfg_uid_info.colourid
									left outer join referencelist citylist on citylist.id = customer.cityid
                                
                                    where bill.isactive = true and bill.billdate>@fromdate and bill.billdate<@todate
                                    and bill.customerid = @customerid
                                    and bill.locationid = @locationid
                                    and bill.companyid = @companyid
                                    and items.brandid = @brandid
									group by customer.name,citylist.name ,bill.isinterstatesale,
                                    colours.name,products.name,mfg_uid_info.dname,brand.name");

                if (_reportcreteria.customerid <= 0)
                    qb.CurrentQuery = qb.CurrentQuery.Replace("and bill.customerid = @customerid", "");
                if (_reportcreteria.locationid <= 0)
                    qb.CurrentQuery = qb.CurrentQuery.Replace("and bill.locationid = @locationid", "");
                if (_reportcreteria.companyid <= 0)
                    qb.CurrentQuery = qb.CurrentQuery.Replace("and bill.companyid = @companyid", "");
                if (_reportcreteria.brandid <= 0)
                    qb.CurrentQuery = qb.CurrentQuery.Replace("and items.brandid = @brandid", "");

                if (report.sizenames == null || report.sizenames.Count <= 1)
                    qb.CurrentQuery = qb.CurrentQuery.Replace(",STRING_AGG(cast(billitems.attributes->'size' as varchar)||':'||cast(billitems.qty as numeric(8,2)),'~') qtylist", "");

                command = qb.GetCommand(conn);
                command.Parameters.AddWithValue("customerid", _reportcreteria.customerid);
                command.Parameters.AddWithValue("brandid", _reportcreteria.brandid);
                command.Parameters.AddWithValue("locationid", _reportcreteria.locationid);
                command.Parameters.AddWithValue("companyid", _reportcreteria.companyid);
                command.Parameters.AddWithValue("fromdate", _reportcreteria.fromdate);
                command.Parameters.AddWithValue("todate", _reportcreteria.todate.Date.AddDays(1));
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        MFG_SizeGroup_Stock oitem = new MFG_SizeGroup_Stock();
                        oitem.customer = reader["customername"] == DBNull.Value ? "" : reader["customername"].ToString();
                        oitem.city = reader["city"] == DBNull.Value ? "" : reader["city"].ToString();
                        oitem.brand = reader["brandname"] == DBNull.Value ? "" : reader["brandname"].ToString();
                        oitem.colourname = reader["colourname"] == DBNull.Value ? "" : reader["colourname"].ToString();
                        oitem.productname = reader["productname"] == DBNull.Value ? "" : reader["productname"].ToString();
                        oitem.groupkey = reader["groupkey"] == DBNull.Value ? "" : reader["groupkey"].ToString();
                        oitem.igst = reader["isinterstatesale"] == DBNull.Value ? false : Convert.ToBoolean(reader["isinterstatesale"]);

                        if (string.IsNullOrEmpty(oitem.sizegroup)) oitem.sizegroup = "-";
                        oitem.salesqty = reader["salesqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["salesqty"]);
                        if (!sizegroups.Exists(e => e.name == oitem.sizegroup))
                        {
                            var newsizegroup = new ReferenceSizeGroup()
                            {
                                id = 0,
                                name = oitem.sizegroup,
                                list = new List<ReferenceItem>()
                            };
                            newsizegroup.list.Add(new ReferenceItem());
                            sizegroups.Add(newsizegroup);
                        }

                        var sizegroup = sizegroups.Find(e => e.name == oitem.sizegroup);

                        var setqty = sizegroup.list.Count;
                        if (setqty > 0)
                        {
                            oitem.setsales = oitem.salesqty / setqty;
                        }
                        oitem.patchqty = setqty;

                        if (report.sizenames != null && report.sizenames.Count > 1 && reader["qtylist"] != DBNull.Value)
                        {
                            oitem.sz = new decimal[report.sizenames.Count];
                            var slist = reader["qtylist"].ToString().Replace(@"""", "").Split('~').ToList().ConvertAll(e => new KeyValuePair<string, decimal>(e.Split(':')[0], decimal.Parse(e.Split(':')[1])));
                            for (var i = 0; i < report.sizenames.Count; i++)
                            {
                                oitem.sz[i] = slist.Sum(x => x.Key == report.sizenames[i] ? x.Value : 0);
                            }


                            //var qtylist = reader["qtylist"].ToString().Replace(@"""", "").Split('-').ToList();
                            //var slist = new List<KeyValuePair<string, decimal>>();
                            //decimal d;
                            //foreach (var sitem in qtylist)
                            //{
                            //    var k = sitem.Split(':')[0];
                            //    var v = sitem.Split(':')[1];
                            //    if (decimal.TryParse(v, out d))
                            //        slist.Add(new KeyValuePair<string, decimal>(k, d));
                            //}
                            //for (var i = 0; i < report.sizenames.Count; i++)
                            //{
                            //    oitem.sz[i] = slist.Sum(x => x.Key == report.sizenames[i] ? x.Value : 0);
                            //}
                        }
                        lst.Add(oitem);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _reportcreteria);
            }
            return report;

        }



        internal MFG_SizeGroup<MFG_SizeGroup_Stock> SearchSizeGroupSalesDetails(SalesorderitemsCreteria _reportcreteria)
        {

            var report = new MFG_SizeGroup<MFG_SizeGroup_Stock>();
            List<MFG_SizeGroup_Stock> lst = new List<MFG_SizeGroup_Stock>();
            report.items = lst;
            NpgsqlConnection conn = CreateConnection();
            try
            {
                if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_possizelist != null &&
                    InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_possizelist != "")
                {
                    report.sizenames = new List<string>(InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_possizelist.Split(','));
                }
                conn.Open();
                NpgsqlCommand command = null;

                QueryBuilder qb = new QueryBuilder();

                ReferencelistDataService referencelistDataService = new ReferencelistDataService() { Context = this.Context };
                var sizegroups = referencelistDataService.SearchSizeGroups();

                /*

with soldsoitems
as
(
select billitems.attributes->>'serialno' uid,sum(billitems.qty) qty
                                  
									from billitems
									inner join bill on bill.id = billitems.billid
									left outer join customer on customer.id= bill.customerid
									inner join salablegoods on salablegoods.id = billitems.salablegoodsid
                                    inner join stock on stock.id = salablegoods.stockid
                                    left outer join mfg_uid_info on mfg_uid_info.uid = stock.serialno
									left outer join products on products.id = mfg_uid_info.productid
                                    left outer join brand on brand.id = mfg_uid_info.brandid
									left outer join referencelist colours on colours.id = mfg_uid_info.colourid
									left outer join referencelist citylist on citylist.id = customer.cityid
                                    where bill.isactive = true
                              
                                    and billattributes->>'orderrefid'= 'SO-12'
									group by billitems.attributes->>'serialno'
)

select salesorderitems.uid,id,qty from salesorderitems
inner join soldsoitems on soldsoitems.uid = salesorderitems.uid
where SALESORDERID in (select id from salesorder where sono='SO-12')
*/

                qb.CurrentQuery = UpdateQueryForHyper(@"
                                    select customer.name customername,citylist.name city,bill.isinterstatesale,stock.designid,
                                    colours.name colourname,products.name productname,bill.billdate::date billdate,
                                    billattributes->>'orderrefid' billso,sizes.name,
                                    mfg_uid_info.dname groupkey,brand.name brandname,bill.billno,
                                    sum(billitems.qty) salesqty,STRING_AGG(cast(billitems.attributes->'size' as varchar)||':'||cast(billitems.qty as numeric(8,2)),'~') qtylist
									from billitems
									inner join bill on bill.id = billitems.billid
									left outer join customer on customer.id= bill.customerid
									inner join salablegoods on salablegoods.id = billitems.salablegoodsid
                                    inner join stock on stock.id = salablegoods.stockid
                                    left outer join mfg_uid_info on mfg_uid_info.uid = stock.serialno
									left outer join products on products.id = mfg_uid_info.productid
                                    left outer join brand on brand.id = mfg_uid_info.brandid
									left outer join referencelist colours on colours.id = mfg_uid_info.colourid
                                    left outer join referencelist sizes on sizes.id = mfg_uid_info.sizeid
									left outer join referencelist citylist on citylist.id = customer.cityid
                                    where bill.isactive = true 
                                    and bill.billdate>@fromdate and bill.billdate<@todate
                                    and bill.customerid = @customerid
                                    and bill.locationid = @locationid
                                    and bill.companyid = @companyid
                                    and billitems.isactive=true
                                    and mfg_uid_info.brandid = @brandid
                                    and sizes.name = @sizename
                                    and billattributes->>'orderrefid'= @billso
									group by customer.name,citylist.name ,bill.isinterstatesale,stock.designid,
                                    colours.name,products.name,bill.billdate::date ,mfg_uid_info.dname,
                                    billattributes->>'orderrefid',brand.name,sizes.name ,bill.billno");

                if (_reportcreteria.customerid <= 0)
                    qb.CurrentQuery = qb.CurrentQuery.Replace("and bill.customerid = @customerid", "");
                if (_reportcreteria.locationid <= 0)
                    qb.CurrentQuery = qb.CurrentQuery.Replace("and bill.locationid = @locationid", "");
                if (_reportcreteria.companyid <= 0)
                    qb.CurrentQuery = qb.CurrentQuery.Replace("and bill.companyid = @companyid", "");
                if (_reportcreteria.brandid <= 0)
                    qb.CurrentQuery = qb.CurrentQuery.Replace("and items.brandid = @brandid", "");
                if (string.IsNullOrEmpty(_reportcreteria.sono))
                    qb.CurrentQuery = qb.CurrentQuery.Replace("and billattributes->>'orderrefid'= @billso", "");
                if (string.IsNullOrEmpty(_reportcreteria.size))
                    qb.CurrentQuery = qb.CurrentQuery.Replace("and sizes.name = @sizename", "");

                if (_reportcreteria.fromdate.Date == DateTime.MinValue)
                    qb.CurrentQuery = qb.CurrentQuery.Replace("and bill.billdate>@fromdate and bill.billdate<@todate", "");

                if (report.sizenames == null || report.sizenames.Count <= 1)
                    qb.CurrentQuery = qb.CurrentQuery.Replace(",STRING_AGG(cast(billitems.attributes->'size' as varchar)||':'||cast(billitems.qty as numeric(8,2)),'~') qtylist", "");

                command = qb.GetCommand(conn);
                command.Parameters.AddWithValue("customerid", _reportcreteria.customerid);
                command.Parameters.AddWithValue("brandid", _reportcreteria.brandid);
                command.Parameters.AddWithValue("locationid", _reportcreteria.locationid);
                command.Parameters.AddWithValue("companyid", _reportcreteria.companyid);
                command.Parameters.AddWithValue("fromdate", _reportcreteria.fromdate);
                command.Parameters.AddWithValue("sizename", string.IsNullOrEmpty(_reportcreteria.size) ? "" : _reportcreteria.size);
                command.Parameters.AddWithValue("billso", string.IsNullOrEmpty(_reportcreteria.sono) ? "" : _reportcreteria.sono);
                command.Parameters.AddWithValue("todate", _reportcreteria.todate.Date.AddDays(1));
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        MFG_SizeGroup_Stock oitem = new MFG_SizeGroup_Stock();
                        oitem.customer = reader["customername"] == DBNull.Value ? "" : reader["customername"].ToString();
                        oitem.city = reader["city"] == DBNull.Value ? "" : reader["city"].ToString();
                        oitem.brand = reader["brandname"] == DBNull.Value ? "" : reader["brandname"].ToString();
                        oitem.colourname = reader["colourname"] == DBNull.Value ? "" : reader["colourname"].ToString();
                        oitem.productname = reader["productname"] == DBNull.Value ? "" : reader["productname"].ToString();
                        oitem.billno = reader["billno"] == DBNull.Value ? "" : reader["billno"].ToString();
                        oitem.billdate = reader["billdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["billdate"]);
                        oitem.groupkey = reader["groupkey"] == DBNull.Value ? "" : reader["groupkey"].ToString();
                        oitem.billso = reader["billso"] == DBNull.Value ? "" : reader["billso"].ToString();
                        oitem.igst = reader["isinterstatesale"] == DBNull.Value ? false : Convert.ToBoolean(reader["isinterstatesale"]);

                        if (string.IsNullOrEmpty(oitem.sizegroup)) oitem.sizegroup = "-";
                        oitem.salesqty = reader["salesqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["salesqty"]);
                        if (!sizegroups.Exists(e => e.name == oitem.sizegroup))
                        {
                            var newsizegroup = new ReferenceSizeGroup()
                            {
                                id = 0,
                                name = oitem.sizegroup,
                                list = new List<ReferenceItem>()
                            };
                            newsizegroup.list.Add(new ReferenceItem());
                            sizegroups.Add(newsizegroup);
                        }

                        var sizegroup = sizegroups.Find(e => e.name == oitem.sizegroup);

                        var setqty = sizegroup.list.Count;
                        if (setqty > 0)
                        {
                            oitem.setsales = oitem.salesqty / setqty;
                        }
                        oitem.patchqty = setqty;

                        if (report.sizenames != null && report.sizenames.Count > 1 && reader["qtylist"] != DBNull.Value)
                        {
                            oitem.sz = new decimal[report.sizenames.Count];
                            var slist = reader["qtylist"].ToString().Replace(@"""", "").Split('~').ToList().ConvertAll(e => new KeyValuePair<string, decimal>(e.Split(':')[0], decimal.Parse(e.Split(':')[1])));
                            for (var i = 0; i < report.sizenames.Count; i++)
                            {
                                oitem.sz[i] = slist.Sum(x => x.Key == report.sizenames[i] ? x.Value : 0);
                            }
                        }
                        lst.Add(oitem);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _reportcreteria);
            }
            return report;

        }

        const string SQL_Insert = @"INSERT INTO public.Salesorder(sodate,remarks, expectedate, sono, customerid, termsandcondition, isactive, createdby, modifiedby, version, createdon, modifiedon, locationid, status, sobookno,customerpo,sobookdate,
        deliverylocationid,mode,tolerance,sosourcetype,sizegroup,salesmenid,agentid,freight,companyid,totalsetqty,totalqty,orderformimageid,shiptocustomerid,discountpercentage,hasshipto,soracknumber,sobookqty,customerstateid) 
        VALUES(@sodate,@remarks, @expectedate, @sono, @customerid, @termsandcondition, @isactive, @createdby, @modifiedby, @version, @createdon, @modifiedon, @locationid, @status, @sobookno,@customerpo,@sobookdate,
        @deliverylocationid,@mode,@tolerance,@sosourcetype,@sizegroup,@salesmenid,@agentid,@freight,@companyid,@totalsetqty,@totalqty,@orderformimageid,@shiptocustomerid,@discountpercentage,@hasshipto,@soracknumber,@sobookqty,@customerstateid) RETURNING id";

        const string SQL_SearchMobilePOSummary = @"select Salesorderitems.id,Salesorderitems.productid,Salesorderitems.brandid,
        products.name product,customer.mobileno mobileno,
        brandname brand,coalesce(size.name,sizename) size,designid,customer.name customer,
        customerid,colour.name colourname,Salesorderitems.designitemcode,
        Salesorderitems.designuid,Salesorderitems.colourid,Salesorderitems.sizegroupid,Salesorderitems.sizegroup,
        sodate,sono,designid,oqty,price,Salesorderitems.imageid,Salesorder.id soid,bqty,pqty,cqty,rqty from Salesorderitems
        inner join Salesorder on Salesorder.id = Salesorderitems.Salesorderid
        inner join customer on customer.id = customerid
        left outer join products on products.id = productid
        left outer join items on items.id = itemid
        left outer join referencelist size on size.id = Salesorderitems.sizeid
        left outer join referencelist colour on colour.id = Salesorderitems.colourid";

        const string SQL_SearchPOSummary = @"with itemsummary as
(
	select Salesorderid, sum(oqty) oqty, sum(rqty) rqty, sum(cqty) cqty from Salesorderitems 
	where isactive = true group by Salesorderid
)
                                select Salesorder.id, sodate, expectedate, sono, customerid, termsandcondition, Salesorder.isactive, 
                                Salesorder.createdby, Salesorder.modifiedby, Salesorder.version,tolerance, Salesorder.createdon, Salesorder.modifiedon,
                                Salesorder.locationid,status, itemsummary.oqty, itemsummary.rqty, itemsummary.cqty,
                                deliverylocationid,mode,agentid,freight,employee.name as employeename from Salesorder 
                                inner join itemsummary on itemsummary.Salesorderid = Salesorder.id
                                inner join users on users.id=Salesorder.createdby 
                                inner join employee on employee.id=users.employeeid";

        const string SQL_DesignWiseStock = @"select items.productid,stock.designid,items.colourid, colours.name colourname,products.name productname,
                                    lrinvoiceitems.itemdetail->> 'sizegroup' sizegroup,
                                    sum(salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty)) stockqty
									from salablegoods
                                    inner join stock on stock.id = salablegoods.stockid
                                    left outer join lrinvoiceitems on lrinvoiceitems.id = invoiceitemid
                                    inner join items on items.id = stock.itemid
									inner join products on products.id = items.productid
									inner join referencelist colours on colours.id = items.colourid
                                    where salablegoods.hasstock = true
									group by items.productid,stock.designid,items.colourid,lrinvoiceitems.itemdetail->> 'sizegroup',
									colours.name ,products.name";

        internal bool UpdateMobilePO(SalesorderItemSimple item)
        {
            var po = Search(new SalesorderCreteria()
            {
                id = item.soid
            }).First();

            var poitem = po.Salesorderitems.Find(e => e.id == item.id);
            poitem.oqty = item.qty;
            poitem.addnlchargeid = item.addnlchargeid;
            SalesorderitemsDataService salesorderitemsDataService = new SalesorderitemsDataService() { Context = this.Context };
            salesorderitemsDataService.Update(poitem);
            return true;
        }

        internal Salesorderitems ChangeMobilePO(SalesorderItemSimple item)
        {
            if (item.cloneid == 0 && item.id == 0)
            {
                throw new Exception("Given Sales order has mismatches, please refresh and try again.");
            }
            SalesorderitemsDataService salesorderitemsDataService = new SalesorderitemsDataService() { Context = this.Context };
            var orderitem = salesorderitemsDataService.Search(new SalesorderitemsCreteria()
            {
                id = item.id == 0 ? item.cloneid : item.id
            });
            if (orderitem.Count == 0)
            {
                throw new Exception("Given Sales order has mismatches, please refresh and try again.");
            }
            var salesorderid = orderitem.First().salesorderid;
            var uid = orderitem.First().uid;

            if (orderitem.First().sizeid == item.sizeid)
            {
                orderitem = salesorderitemsDataService.Search(new SalesorderitemsCreteria()
                {
                    salesorderid = salesorderid,
                    uid = uid
                });

                if (orderitem.Count > 1)
                {
                    for (var i = 1; i < orderitem.Count; i++)
                    {
                        salesorderitemsDataService.Delete(orderitem[i]);
                    }
                }
                orderitem[0].oqty = item.qty;
                return salesorderitemsDataService.Update(orderitem[0]);
            }
            else
            {
                orderitem = salesorderitemsDataService.Search(new SalesorderitemsCreteria()
                {
                    salesorderid = salesorderid,
                    sizeid = item.sizeid,
                    designitemcode = orderitem.First().designitemcode,
                    designid = orderitem.First().designid
                });

                if (orderitem.Count > 0)
                {
                    orderitem = salesorderitemsDataService.Search(new SalesorderitemsCreteria()
                    {
                        salesorderid = salesorderid,
                        uid = orderitem.First().uid
                    });

                    if (orderitem.Count > 1)
                    {
                        for (var i = 1; i < orderitem.Count; i++)
                        {
                            salesorderitemsDataService.Delete(orderitem[i]);
                        }
                    }
                    orderitem[0].oqty = item.qty;
                    return salesorderitemsDataService.Update(orderitem[0]);
                }
                else
                {
                    string query = @"with alluid
                as
                (
                    select mfgdesign.code,schemelist->>'sizegroup' sizegroup,schemelist->>'uid' uid,cast(schemelist->>'sizeid' as bigint) sizeid,
                    cast(schemelist->>'wsrate' as numeric) wsrate,cast(schemelist->>'dealerprice' as numeric) dealerprice,
                    cast(schemelist->>'salerate' as numeric) salerate,cast(schemelist->>'mrp' as numeric) mrp,
                    schemelist->>'code' dname,cast(schemelist->>'colourid' as bigint) colourid,
                    case when position('/' in schemelist->>'code') = 0 then '' else
                    substring(schemelist->>'code',position('/' in schemelist->>'code')+1)
                    end colourcode
                    from mfgdesign
                    LEFT JOIN LATERAL
                    json_array_elements (cast(mfgdesign.sizegroup->>'items' as json)) schemelist ON true
                    where (split_part(schemelist->>'code','/',2)~ '^\d+$')=false
                    and mfgdesign.isactive=true
                )
                SELECT UID FROM ALLUID where CODE = @CODE 
                AND DNAME = @DNAME AND SIZEID=@SIZEID";
                    uid = "";
                    var param = new SortedList<string, object>();
                    param.Add("CODE", orderitem.First().designid);
                    param.Add("DNAME", orderitem.First().designitemcode);
                    param.Add("SIZEID", item.sizeid);
                    using (var reader = ExecuteReader(query, param))
                    {
                        if (reader.Read())
                        {
                            uid = reader.GetString(0);
                        }
                    }
                    if (!string.IsNullOrEmpty(uid))
                    {
                        orderitem.First().sizeid = item.sizeid;
                        orderitem.First().id = 0;
                        orderitem.First().uid = uid;
                        return salesorderitemsDataService.Insert(orderitem.First());
                    }
                }
            }
            throw new Exception("Given Sales order has mismatches, please refresh and try again.");
            //if (item.id == 0)
            //{
            //    orderitem.First().id = 0;
            //    ReferencelistDataService referencelistDataService = new ReferencelistDataService() { Context = this.Context };
            //    var list = referencelistDataService.SearchList(new ReferencelistCreteria()
            //    {
            //        type = "SIZE",
            //        name = item.size
            //    });
            //    if (list.Count > 0)
            //    {
            //        orderitem.First().sizeid = list.First().id;
            //        orderitem.First().size = list.First().name;
            //        orderitem.First().sizename = list.First().name;
            //    }
            //    return salesorderitemsDataService.Insert(orderitem.First());
            //}

        }

        internal List<SalesOrderSummary> ClearPlan(List<SalesOrderSummary> items)
        {
            List<SalesorderitemsDetail> changeditems = new List<SalesorderitemsDetail>();
            foreach (var item in items)
            {
                var saleorder = Search(new SalesorderCreteria()
                {
                    id = item.soid
                }).First();

                var list = saleorder.Salesorderitems.FindAll(e => e.sizegroup == item.sizegroup &&
                e.productname == item.productname && e.designid == item.designid).GroupBy(ex => new { ex.sizegroup, ex.productname, ex.designid });
                foreach (var sitem in list)
                {
                    foreach (var saleorderitem in sitem)
                    {
                        saleorderitem.pqty = 0;
                        if (!changeditems.Exists(e => e.id == saleorderitem.id))
                            changeditems.Add(saleorderitem);
                    }
                }
            }




            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    string query = "update Salesorderitems set pqty=@pqty where id=@id";
                    var command = GetCommand(query, conn, transaction);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = 0;
                    command.Parameters.Add("pqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = 0;
                    foreach (var item in changeditems)
                    {
                        command.Parameters["id"].Value = item.id;
                        command.Parameters["pqty"].Value = item.pqty;
                        command.ExecuteNonQuery();
                    }
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, items);
            }
            return items;
        }

        internal void UpdateBarcodePrices(SearchBarcodeData item, List<Barcodedgoods> list)
        {
            if (item.ccmarginmrp > 0)
            {
                MarginRules marginRules = (MarginRules)item.cccomputemode;
                TaxDataService taxDataService = new TaxDataService() { Context = Context };
                var taxlist = taxDataService.TaxProducts(list.ConvertAll(x => x.productid).Distinct().ToList());
                foreach (var sitem in list)
                {
                    if (taxlist.ContainsKey(sitem.productid))
                    {
                        var taxid = taxlist[sitem.productid].id;
                        var taxpercentage = taxlist[sitem.productid].taxsplit.Find(sitem.mrp).taxpercent;
                        var tax = Math.Round((sitem.mrp * 1) * taxpercentage / 100, 2);
                        var mintaxrange = taxlist[sitem.productid].taxsplit.items.OrderBy(x => x.sgst + x.cgst).First();
                        var price = Math.Round(ComputeMargin(marginRules, taxpercentage,
                            mintaxrange.cgst + mintaxrange.sgst,
                            item.ccmarginmrp, sitem.mrp, taxlist[sitem.productid].taxsplit), 2);
                        tax = Math.Round((price * 1) * taxpercentage / 100, 2);
                        sitem.price = price + tax;
                        sitem.priceextax = price;
                        sitem.dealerprice = price + tax;
                        sitem.dealerpriceextax = price;
                        sitem.wsrate = price + tax;
                        sitem.wsrateextax = price;
                    }
                }
            }
        }



        internal List<SampleOrderSO> PendingSampleOrderSO(long customerid, long mfgsampleorderid)
        {
            List<SampleOrderSO> result = new List<SampleOrderSO>();
            string query = @"select Salesorderid,SODATE,SONO,uid,salesorderitems.designid,salesorderitems.sizegroup,
            itemid,description,designitemcode,mfgsampleorder.designno,Salesorderitems.sizeid,sum(oqty-(rqty+cqty)) qty from salesorder
	        inner join Salesorderitems on Salesorderitems.salesorderid = salesorder.id  and Salesorderitems.isactive=true
			left outer join mfgsampleorder on mfgsampleorder.id = mfgsampleorderid
	        where salesorder.customerid=@customerid and mfgsampleorderid=@mfgsampleorderid and salesorder.isactive=true
	        and oqty-(rqty+cqty)>0
	        group by Salesorderid,SODATE,SONO,uid,salesorderitems.designid,salesorderitems.sizegroup,
            itemid,description,designitemcode,mfgsampleorder.designno,salesorderitems.sizeid";
            var param = new SortedList<string, object>();
            param.Add("customerid", customerid);
            param.Add("mfgsampleorderid", mfgsampleorderid);
            using (var reader = ExecuteReader(query, param))
            {
                while (reader.Read())
                {
                    DateTime SODATE = reader["SODATE"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["SODATE"]);
                    long Salesorderid = reader["Salesorderid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["Salesorderid"]);
                    string uid = reader["uid"] == DBNull.Value ? "" : Convert.ToString(reader["uid"]);
                    string SONO = reader["SONO"] == DBNull.Value ? "" : Convert.ToString(reader["SONO"]);
                    decimal qty = reader["qty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["qty"]);

                    string designno = reader["designno"] == DBNull.Value ? "" : Convert.ToString(reader["designno"]);
                    string designid = reader["designid"] == DBNull.Value ? "" : Convert.ToString(reader["designid"]);
                    string sizegroup = reader["sizegroup"] == DBNull.Value ? "" : Convert.ToString(reader["sizegroup"]);
                    string description = reader["description"] == DBNull.Value ? "" : Convert.ToString(reader["description"]);
                    string designitemcode = reader["designitemcode"] == DBNull.Value ? "" : Convert.ToString(reader["designitemcode"]);
                    long itemid = reader["itemid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["itemid"]);
                    long sizeid = reader["sizeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sizeid"]);
                    if (!result.Exists(x => x.id == Salesorderid))
                    {
                        result.Add(new SampleOrderSO()
                        {
                            items = new List<SampleOrderSO.Item>(),
                            sono = SONO,
                            sodate = SODATE,
                            id = Salesorderid,
                            designid = designid,
                            designno = designno,
                            sizegroup = sizegroup
                        });
                    }
                    result.Find(x => x.id == Salesorderid).items.Add(new SampleOrderSO.Item()
                    {
                        uid = uid,
                        qty = qty,
                        sizeid = sizeid,
                        itemid = itemid,
                        description = description
                    });
                }
            }

            if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.mfg_wosodesignduplicate)
            {
                query = @"select mfgworkorder.mfgsampleorderid,
                mfgworkorderitems.salesorderid,customerid,
                mfgworkorderitemsizes.uid from mfgworkorder
                inner join mfgworkorderitems on mfgworkorderitems.mfgworkorderid = mfgworkorder.id
                and mfgworkorderitems.isactive=true
                inner join mfgworkorderitemsizes on mfgworkorderitemsizes.mfgworkorderitemsid = mfgworkorderitems.id
                and mfgworkorderitemsizes.isactive=true
                where mfgworkorder.isactive=true
                and mfgsampleorderid=@mfgsampleorderid 
                and customerid=@customerid";
                param = new SortedList<string, object>();
                param.Add("customerid", customerid);
                param.Add("mfgsampleorderid", mfgsampleorderid);
                using (var reader = ExecuteReader(query, param))
                {
                    while (reader.Read())
                    {
                        long salesorderid = reader["salesorderid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["salesorderid"]);
                        string uid = reader["uid"] == DBNull.Value ? "" : Convert.ToString(reader["uid"]);
                        if (result.Exists(x => x.id == salesorderid))
                        {
                            result.Find(x => x.id == salesorderid).items.RemoveAll(e => e.uid == uid);
                        }
                    }
                }
            }
            return result;
        }
        internal Barcodedgoods SearchBarcode(SearchBarcodeData item, bool includenegative)
        {
            SalablegoodsDataService StockData = new SalablegoodsDataService() { Context = this.Context };
            var rresult = StockData.BarcodeSearch(item.locationid.ToString(), item.code, includenegative);
            var list = new List<Barcodedgoods>();
            list.Add(rresult);
            if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.mfg_mode == 5)
            {
                if (item.ccmarginmrp > 0)
                {
                    MarginRules marginRules = (MarginRules)item.cccomputemode;
                    TaxDataService taxDataService = new TaxDataService() { Context = Context };
                    var taxlist = taxDataService.TaxProducts(list.ConvertAll(x => x.productid).Distinct().ToList());
                    foreach (var sitem in list)
                    {
                        var taxid = taxlist[sitem.productid].id;
                        var taxpercentage = taxlist[sitem.productid].taxsplit.Find(sitem.mrp).taxpercent;
                        var tax = Math.Round((sitem.mrp * 1) * taxpercentage / 100, 2);
                        var mintaxrange = taxlist[sitem.productid].taxsplit.items.OrderBy(x => x.sgst + x.cgst).First();

                        var price = Math.Round(ComputeMargin(marginRules, taxpercentage,
                            mintaxrange.cgst + mintaxrange.sgst,
                            item.ccmarginmrp, sitem.mrp, taxlist[sitem.productid].taxsplit), 2);
                        tax = Math.Round((price * 1) * taxpercentage / 100, 2);
                        sitem.price = price + tax;
                        sitem.priceextax = price;
                        sitem.dealerprice = price + tax;
                        sitem.dealerpriceextax = price;
                        sitem.wsrate = price + tax;
                        sitem.wsrateextax = price;
                    }
                }
                return rresult;
            }
            else if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_ismfgapplication)
            {
                return ChangePriceFromDesign(list,item, InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.mfg_salepricefromsku).First();
            }
            return rresult;
        }

        public decimal ComputeMargin(MarginRules rule,
            decimal taxpercentage,
            decimal mintaxpercentage,
            decimal margin,
            decimal mrp,
            Tax.TaxSplit taxsplit)
        {
            switch (rule)
            {
                case MarginRules.Markdown:
                    return mrp - (mrp * margin / 100);
                case MarginRules.Markdown_NetOff:
                    var marginvalue = (mrp * margin / 100);
                    var taxvalue = (mrp * (taxpercentage / 100)) / (1 + (taxpercentage / 100));
                    return mrp - marginvalue - taxvalue;
                case MarginRules.Markdown_ExcludeTax:
                    var rrp = mrp / (1 + (taxpercentage / 100));
                    var exmarginvalue = rrp * margin / 100;
                    return rrp - exmarginvalue;
                case MarginRules.Markdown_ExtraMargin:
                    if (taxpercentage > mintaxpercentage)
                    {
                        margin += (taxpercentage - mintaxpercentage);
                    }
                    return mrp - (mrp * margin / 100);
                //TONIQ
                case MarginRules.Markdown_IncludeTax:
                    var toniqrrp = mrp / (1 + (taxpercentage / 100));
                    var wholemargin = mrp * margin / 100;
                    var excludeprice = toniqrrp - wholemargin;
                    var includeprice = excludeprice + (excludeprice * taxpercentage / 100);
                    return includeprice;
                case MarginRules.Markdown_AdditionalTax:
                    var yestomrrp = mrp - (mrp * margin / 100);
                    taxpercentage = taxsplit.Find(yestomrrp).taxpercent;
                    var yestotax = yestomrrp * taxpercentage / 100;
                    return yestomrrp;
            }
            return 0;
        }

        public enum MarginRules
        {
            Markdown,
            Markdown_NetOff,
            Markdown_ExcludeTax,
            Markdown_ExtraMargin,
            Markdown_IncludeTax,
            Markdown_AdditionalTax
        }

        internal List<Barcodedgoods> SearchDesignSizeSetColourStock(
            SearchBarcodeData data, bool retry)
        {
            string[] codes = data.code.Split('/');
            string colourname = "";
            var referencelistDataService = new ReferencelistDataService() { Context = this.Context };
            if (codes.Length == 3)
            {
                long colourid = 0;
                if (long.TryParse(codes[2], out colourid))
                {
                    colourname = referencelistDataService.Search(new ReferencelistCreteria()
                    {
                        id = colourid
                    }).First().name;
                }
            }
            string query = string.Format(@"
                                    select items.productid,stock.designid,items.sizeid,items.colourid,lrinvoiceitems.itemdetail->> 'sizegroup' sizegroup, 
                                    (salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty)) stockqty,
                                    salablegoods.id as salablegoodsid, salablegoods.barcode from salablegoods
                                    inner join stock on stock.id = salablegoods.stockid
                                    inner join items on items.id = stock.itemid
                                    left outer join lrinvoiceitems on lrinvoiceitems.id = invoiceitemid
                                    where salablegoods.hasstock = true and salablegoods.locationid={2} 
                                    and designid = '{0}' and items.colourid={1}", codes[0], codes[2], data.locationid);



            List<SalesorderStock> salesorderStock = new List<SalesorderStock>();
            NpgsqlCommand command = null;
            QueryBuilder qb = new QueryBuilder();
            qb.CurrentQuery = query;


            var sizegroups = referencelistDataService.SearchSizeGroups();
            var sizegroup = sizegroups.Find(e => e.name == codes[1]);
            var sizecount = sizegroup.list.Count;
            if (data.usedstock == null) data.usedstock = new List<SearchBarcodeData.itemqty>();
            var usedstock = data.usedstock.GroupBy(e => e.id).ToList().ConvertAll(ex => new SearchBarcodeData.itemqty()
            {
                id = ex.Key,
                qty = ex.Sum(x => x.qty)
            });
            using (NpgsqlConnection conn = CreateConnection())
            {
                conn.Open();
                command = qb.GetCommand(conn);
                if (Services.ConfigManager.Instance(Context.OrganizationID).Config.analyser_stock_removeaqty)
                    command.CommandText = command.CommandText.Replace("salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty)", "salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty+salablegoods.aqty)");

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        SalesorderStock oSalesorder = new SalesorderStock();
                        oSalesorder.productid = reader["productid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productid"]);
                        oSalesorder.sizeid = reader["sizeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sizeid"]);
                        oSalesorder.salablegoodsid = reader["salablegoodsid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["salablegoodsid"]);
                        oSalesorder.colourid = reader["colourid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["colourid"]);
                        oSalesorder.designid = reader["designid"] == DBNull.Value ? "" : reader["designid"].ToString();
                        oSalesorder.barcode = reader["barcode"] == DBNull.Value ? "" : reader["barcode"].ToString();
                        oSalesorder.stockqty = reader["stockqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["stockqty"]);

                        if (sizegroup.list.Exists(e => e.id == oSalesorder.sizeid))
                        {
                            if (usedstock.Exists(e => e.id == oSalesorder.salablegoodsid))
                            {
                                oSalesorder.stockqty -= usedstock.Find(e => e.id == oSalesorder.salablegoodsid).qty;
                            }
                            if (oSalesorder.stockqty > 0)
                            {
                                var addedqty = salesorderStock.Sum(e => e.sizeid == oSalesorder.sizeid ? e.stockqty : 0);
                                if (data.qty > addedqty + oSalesorder.stockqty)
                                {
                                    salesorderStock.Add(oSalesorder);
                                }
                                else
                                {
                                    oSalesorder.stockqty = data.qty - addedqty;
                                    salesorderStock.Add(oSalesorder);
                                    sizegroup.list.RemoveAll(e => e.id == oSalesorder.sizeid);
                                }
                            }
                        }
                        if (sizegroup.list.Count == 0) break;
                    }
                }
                conn.Close();
            }

            if (!retry && sizegroup.list.Count > 0)
            {
                if (salesorderStock.GroupBy(e => e.sizeid).Count() == sizecount)
                {
                    var qty = decimal.MaxValue;
                    foreach (var item in salesorderStock.GroupBy(e => e.sizeid))
                    {
                        if (qty > item.Sum(e => e.stockqty))
                            qty = item.Sum(e => e.stockqty);
                    }
                    if (qty > 0)
                    {
                        data.qty = qty;
                    }
                }
            }

            if (sizegroup.list.Count == 0)
            {
                SalablegoodsDataService salablegoodsDataService = new SalablegoodsDataService() { Context = this.Context };
                var salableresult = salablegoodsDataService.BarcodeSearch(salesorderStock.ConvertAll(e => e.salablegoodsid));
                foreach (var item in salableresult)
                {
                    item.qty = salesorderStock.Find(e => e.salablegoodsid == item.id).stockqty;
                    item.groupkey = data.code;
                    item.groupname = data.code;
                    item.sizegroup = codes[1];
                    item.designid = codes[0];
                    item.colourname = colourname;
                }


                return ChangePriceFromDesign(salableresult, data, InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.mfg_salepricefromsku);
            }
            return new List<Barcodedgoods>();
        }

        public List<SalesOrderSummary> SearchCustomerSummary(SearchBarcodeData creteria)
        {
            List<SalesOrderSummary> lst = new List<SalesOrderSummary>();
            if (string.IsNullOrEmpty(creteria.sono)) return lst;
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                NpgsqlCommand command = null;
                QueryBuilder qb = new QueryBuilder();
                qb.CurrentQuery = @"
                                    select Salesorderitems.id,Salesorderitems.sizeid,
                                    Salesorderitems.designid,Salesorderitems.sizegroup,  
                                    Salesorderitems.colourid,Salesorderitems.productid,
                                    referencelist.name colourname,Salesorderitems.price,
                                    (oqty::numeric-(bqty::numeric+cqty::numeric+rqty::numeric)) pendingqty, 
                                    (pqty) plannedqty,discountpercentage,salesmenid,agentid,
                                    Salesorderitems.colourcode,addnlcharge,addnlchargeid,
                                    Salesorderitems.designitemcode,fgboxid,soboxid
                                    from Salesorder 
									inner join Salesorderitems on Salesorderitems.salesorderid = Salesorder.id
                                    left outer join items on items.id = Salesorderitems.itemid
									left outer join products on products.id = Salesorderitems.productid
                                    left outer join referencelist on referencelist.id = Salesorderitems.colourid
                                    where salesorder.isactive = true and Salesorderitems.isactive = true";
                if (!creteria.includesold && !creteria.returngoods)
                    qb.CurrentQuery += " and (oqty::numeric-(bqty::numeric+cqty::numeric+rqty::numeric)) >0";


                if (!string.IsNullOrEmpty(creteria.sono))
                {
                    qb.AddParameter("Salesorder.sono", "sono", creteria.sono, "=");
                }
                else
                {
                    qb.CurrentQuery += " and pqty> 0";
                    qb.AddParameter("customerid", "customerid", creteria.customerid, "=");
                    qb.AddParameter("shiptocustomerid", "shiptocustomerid", creteria.shipcustomerid, "=");
                }

                if (!string.IsNullOrEmpty(creteria.uid))
                {
                    qb.AddParameter("Salesorderitems.uid", "uid", creteria.uid, "=");
                }
                else if (creteria.barcodes != null && creteria.barcodes.Count > 0)
                {
                    qb.AddInParamer("Salesorderitems.uid", String.Join(",", creteria.barcodes.ConvertAll(x => "'" + x + "'")));
                }

                command = qb.GetCommand(conn);
                if (Services.ConfigManager.Instance(Context.OrganizationID).Config.analyser_stock_removeaqty)
                    command.CommandText = command.CommandText.Replace("salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty)", "salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty+salablegoods.aqty)");

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        SalesOrderSummary oSalesorder = new SalesOrderSummary();
                        oSalesorder.soid = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);

                        oSalesorder.fgboxid = reader["fgboxid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["fgboxid"]);
                        oSalesorder.soboxid = reader["soboxid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["soboxid"]);
                        oSalesorder.addnlchargeid = reader["addnlchargeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["addnlchargeid"]);
                        oSalesorder.addnlcharge = reader["addnlcharge"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["addnlcharge"]);
                        oSalesorder.dp = reader["discountpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discountpercentage"]);
                        oSalesorder.salesmenid = reader["salesmenid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["salesmenid"]);
                        oSalesorder.agentid = reader["agentid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["agentid"]);
                        oSalesorder.price = reader["price"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["price"]);
                        oSalesorder.sizeid = reader["sizeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sizeid"]);
                        oSalesorder.colourid = reader["colourid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["colourid"]);
                        oSalesorder.productid = reader["productid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productid"]);
                        oSalesorder.sizegroup = reader["sizegroup"] == DBNull.Value ? "" : reader["sizegroup"].ToString();
                        oSalesorder.designitemcode = reader["designitemcode"] == DBNull.Value ? "" : reader["designitemcode"].ToString();
                        oSalesorder.colourname = reader["colourname"] == DBNull.Value ? "" : reader["colourname"].ToString();
                        oSalesorder.colourcode = reader["colourcode"] == DBNull.Value ? "" : reader["colourcode"].ToString();
                        oSalesorder.designid = reader["designid"] == DBNull.Value ? "" : reader["designid"].ToString();
                        oSalesorder.pendingqty = reader["pendingqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["pendingqty"]);
                        oSalesorder.plannedqty = reader["plannedqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["plannedqty"]);
                        lst.Add(oSalesorder);
                    }
                }

                conn.Close();

            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, creteria);
            }
            return lst;
        }

        public class SalesOrderBarcode
        {
            public SalesOrderBarcode()
            {
                saleorderitems = new List<item>();
            }
            public string groupkey { get; set; }
            public string groupname { get; set; }
            public List<item> saleorderitems { get; set; }
            public decimal stockqty { get; set; }
            public string design { get; set; }
            public string sizegroup { get; set; }
            public string colour { get; set; }
            public string colourcode { get; set; }

            public class item
            {
                public long id { get; set; }
                public decimal qty { get; set; }
                public decimal price { get; set; }
                public decimal dp { get; set; }
                public long agentid { get; set; }
                public long salerefid { get; set; }
                public decimal addnlcharge { get; set; }
                public long addnlchargeid { get; set; }
                public long fgboxid { get; set; }
                public long soboxid { get; set; }
            }
        }

        public class SearchBarcodeData : BaseEntity
        {
            public long shipcustomerid { get; set; }
            public long customerid { get; set; }
            public long locationid { get; set; }
            public bool returngoods { get; set; }
            public string code { get; set; }
            public string source { get; set; }
            public decimal qty { get; set; }
            public string sono { get; set; }
            public string uid { get; set; }
            public List<string> barcodes { get; set; }
            public List<itemqty> usedstock { get; set; }
            public bool includesold { get; set; }
            public int cccomputemode { get; set; }
            public decimal ccmarginmrp { get; set; }
            public decimal ccmarginprice { get; set; }
            public class itemqty
            {
                public long id { get; set; }
                public decimal qty { get; set; }
            }
        }
        public List<Barcodedgoods> SearchPlannedOrder(SearchBarcodeData creteria)
        {
            var allcustomerorders = SearchCustomerSummary(creteria);
            List<SalesorderStock> salesorderStock = new List<SalesorderStock>();
            var designlist = string.Join(",", allcustomerorders.GroupBy(e => e.designid).ToList().ConvertAll(e => "'" + WebAPI.Data.Utility.SafeSQL(e.Key) + "'").Distinct().ToList());
            if (designlist.Length > 0)
            {

                string query = string.Format(@"
                                    select items.productid,stock.designid,items.sizeid,items.colourid, 
                                    lrinvoiceitems.itemdetail->> 'sizegroup' sizegroup,
                                    (salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty)) stockqty,
                                    salablegoods.id as salablegoodsid, salablegoods.barcode from salablegoods
                                    inner join stock on stock.id = salablegoods.stockid
                                    left outer join lrinvoiceitems on lrinvoiceitems.id = invoiceitemid
                                    inner join items on items.id = stock.itemid
                                    where salablegoods.hasstock = true and salablegoods.locationid={0} and stock.designid in ({1})",
                creteria.locationid, designlist);
                NpgsqlCommand command = null;
                QueryBuilder qb = new QueryBuilder();
                qb.CurrentQuery = query;
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    command = qb.GetCommand(conn);
                    if (Services.ConfigManager.Instance(Context.OrganizationID).Config.analyser_stock_removeaqty)
                        command.CommandText = command.CommandText.Replace("salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty)", "salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty+salablegoods.aqty)");

                    using (System.Data.IDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            SalesorderStock oSalesorder = new SalesorderStock();
                            oSalesorder.productid = reader["productid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productid"]);
                            oSalesorder.sizeid = reader["sizeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sizeid"]);
                            oSalesorder.salablegoodsid = reader["salablegoodsid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["salablegoodsid"]);
                            oSalesorder.colourid = reader["colourid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["colourid"]);
                            oSalesorder.sizegroup = reader["sizegroup"] == DBNull.Value ? "" : reader["sizegroup"].ToString();
                            oSalesorder.designid = reader["designid"] == DBNull.Value ? "" : reader["designid"].ToString();
                            oSalesorder.barcode = reader["barcode"] == DBNull.Value ? "" : reader["barcode"].ToString();
                            oSalesorder.stockqty = reader["stockqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["stockqty"]);
                            salesorderStock.Add(oSalesorder);
                        }
                    }
                    conn.Close();
                }
            }


            SortedDictionary<long, SalesOrderBarcode> salablegoods = new SortedDictionary<long, SalesOrderBarcode>();

            foreach (var gitem in allcustomerorders.GroupBy(e => new { e.designid, e.sizegroup, e.productid, e.colourid }))
            {

                foreach (var item in gitem)
                {
                    string groupkey = string.Format("{0}/{1}/{2}/{3}", gitem.Key.designid
                        , gitem.Key.sizegroup, gitem.Key.productid, gitem.Key.colourid);
                    string groupname = string.Format("{0}/{1}/{2}", gitem.Key.designid
                        , gitem.Key.sizegroup, gitem.First().colourname);

                    var stocklist = salesorderStock.FindAll(e => e.designid == item.designid &&
                    e.colourid == item.colourid && e.productid == item.productid &&
                    e.sizeid == item.sizeid);
                    // && e.sizegroup == item.sizegroup
                    var pqty = item.plannedqty;
                    SortedDictionary<long, SalesOrderBarcode> allocatedsalablegoods = new SortedDictionary<long, SalesOrderBarcode>();
                    foreach (var stock in stocklist)
                    {
                        if (stock.stockqty > pqty)
                        {
                            allocatedsalablegoods.Add(stock.salablegoodsid, new SalesOrderBarcode()
                            {
                                groupkey = groupkey,
                                design = gitem.Key.designid,
                                sizegroup = gitem.Key.sizegroup,
                                colour = gitem.First().colourname,
                                colourcode = gitem.First().colourcode,
                                groupname = groupname,
                                stockqty = stock.stockqty
                            });
                            allocatedsalablegoods[stock.salablegoodsid].saleorderitems.Add(new SalesOrderBarcode.item()
                            {
                                id = item.soid,
                                qty = pqty,
                                price = item.price,
                                dp = item.dp,
                                addnlcharge = item.addnlcharge,
                                addnlchargeid = item.addnlchargeid,
                                soboxid = item.soboxid,
                                fgboxid = item.fgboxid,
                                agentid = item.agentid,
                                salerefid = item.salesmenid
                            });
                            stock.stockqty -= pqty;
                            pqty = 0;
                        }
                        else if (stock.stockqty > 0)
                        {
                            allocatedsalablegoods.Add(stock.salablegoodsid, new SalesOrderBarcode()
                            {
                                groupkey = groupkey,
                                groupname = groupname,
                                design = gitem.Key.designid,
                                sizegroup = gitem.Key.sizegroup,
                                colour = gitem.First().colourname,
                                colourcode = gitem.First().colourcode,
                                stockqty = stock.stockqty
                            });
                            allocatedsalablegoods[stock.salablegoodsid].saleorderitems.Add(new SalesOrderBarcode.item()
                            {
                                id = item.soid,
                                price = item.price,
                                qty = stock.stockqty,
                                dp = item.dp,
                                addnlcharge = item.addnlcharge,
                                addnlchargeid = item.addnlchargeid,
                                fgboxid = item.fgboxid,
                                soboxid = item.soboxid,
                                agentid = item.agentid,
                                salerefid = item.salesmenid
                            });
                            pqty -= stock.stockqty;
                            stock.stockqty = 0;
                        }
                        if (pqty == 0) break;
                    }
                    if (pqty == 0)
                    {
                        foreach (var allocated in allocatedsalablegoods)
                        {
                            if (salablegoods.ContainsKey(allocated.Key))
                            {
                                salablegoods[allocated.Key].saleorderitems.AddRange(allocated.Value.saleorderitems);
                            }
                            else
                            {
                                salablegoods.Add(allocated.Key, allocated.Value);
                            }
                        }
                    }
                }
            }

            SalablegoodsDataService salablegoodsDataService = new SalablegoodsDataService() { Context = this.Context };
            var salableresult = salablegoodsDataService.BarcodeSearch(salablegoods.ToList().ConvertAll(e => e.Key).ToList());
            foreach (var item in salableresult)
            {
                if (salablegoods.ContainsKey(item.id))
                {
                    item.stockqty = salablegoods[item.id].stockqty;
                    item.qty = salablegoods[item.id].saleorderitems.Sum(e => e.qty);
                    if (salablegoods[item.id].saleorderitems.Exists(x => x.price > 0))
                    {
                        item.price = salablegoods[item.id].saleorderitems.Find(x => x.price > 0).price;
                        var dp = salablegoods[item.id].saleorderitems.Find(x => x.price > 0).dp;
                        if (dp > 0)
                            item.discount = Math.Round(item.price * dp / 100);
                        item.wsrate = item.price;
                        item.dealerprice = item.price;
                        item.priceextax = item.price;
                        item.wsrateextax = item.price;
                        item.buyingprice = item.buyingprice;
                        item.dealerpriceextax = item.price;
                        item.addnlcharge = salablegoods[item.id].saleorderitems.Find(x => x.price > 0).addnlcharge;
                        item.addnlchargeid = salablegoods[item.id].saleorderitems.Find(x => x.price > 0).addnlchargeid;
                    }
                    item.groupkey = salablegoods[item.id].groupkey;
                    item.groupname = salablegoods[item.id].groupname;
                    item.saleorderitems = salablegoods[item.id].saleorderitems;
                    item.designid = salablegoods[item.id].design;
                    item.sizegroup = salablegoods[item.id].sizegroup;
                    item.colourname = salablegoods[item.id].colour;
                    item.colourcode = salablegoods[item.id].colourcode;
                }
            }

            //var groupedstocks = salesorderStock.GroupBy(e => new { e.productid, e.colourid, e.designid }).ToList();
            //ReferencelistDataService referencelistDataService = new ReferencelistDataService() { Context = this.Context };
            //var sizegroups = referencelistDataService.SearchSizeGroups();
            //var plangroups = allcustomerorders.GroupBy(e => new { e.productid, e.designid, e.colourid, e.sizegroup }).ToList();

            //foreach (var item in lst)
            //{
            //    if (sizegroups.Exists(e => e.name == item.sizegroup) &&
            //        groupedstocks.Exists(e => e.Key.colourid == item.colourid &&
            //        e.Key.designid == item.designid &&
            //        e.Key.productid == item.productid))
            //    {
            //        var sizegroup = sizegroups.Find(e => e.name == item.sizegroup);
            //        var groupstocks = groupedstocks.Find(e => e.Key.colourid == item.colourid &&
            //        e.Key.designid == item.designid &&
            //        e.Key.productid == item.productid);
            //        var allsizestock = groupstocks.ToList().FindAll(e => sizegroup.list.Exists(x => x.id == e.sizeid));
            //        item.availableqty = Math.Floor(allsizestock.Average(e => e.stockqty));
            //        if (plangroups.Exists(e => e.Key.colourid == item.colourid &&
            //         e.Key.productid == item.productid && e.Key.designid == item.designid &&
            //         e.Key.sizegroup == item.sizegroup))
            //        {
            //            item.allocatedqty = plangroups.Find(e => e.Key.colourid == item.colourid &&
            //            e.Key.productid == item.productid && e.Key.designid == item.designid &&
            //            e.Key.sizegroup == item.sizegroup).Sum(e => e.plannedqty);
            //        }
            //    }
            //}


            return ChangePriceFromDesign(salableresult,creteria, InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.mfg_salepricefromsku);
        }
        public List<Barcodedgoods> SearchPlannedOrder(
            SalesorderDataService.SearchBarcodeData searchBarcodeData,
            string barcode,
            long locationid,
            string sono,
            bool retry)
        {
            var customerSOList = SearchCustomerSummary(new SearchBarcodeData()
            {
                sono = sono,
                uid = barcode,
                includesold = true
            });

            List<SalesorderStock> salesorderStock = new List<SalesorderStock>();

            string query = string.Format(@"
                                    select items.productid,stock.designid,items.sizeid,items.colourid,sizes.name sizename,
                                    lrinvoiceitems.itemdetail->> 'sizegroup' sizegroup,colours.name colourname,
                                    (salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty)) stockqty,
                                    salablegoods.id as salablegoodsid, salablegoods.barcode from salablegoods
                                    inner join stock on stock.id = salablegoods.stockid
                                    left outer join lrinvoiceitems on lrinvoiceitems.id = invoiceitemid
                                    inner join items on items.id = stock.itemid
                                    left outer join referencelist colours on colours.id = items.colourid
                                    left outer join referencelist sizes on sizes.id = items.sizeid
                                    where salablegoods.hasstock = true and salablegoods.isactive=true ");

            if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig != null &&
                InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig.ContainsKey(locationid) &&
                !InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[locationid].sale_negativestock)
            {
                query = query.Replace("salablegoods.hasstock = true and", "");
            }

            NpgsqlCommand command = null;
            QueryBuilder qb = new QueryBuilder();
            qb.CurrentQuery = query;
            if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_hyper_uan)
            {
                qb.AddParameter("stock.barcode", barcode, "=");
            }
            else
            {
                qb.AddParameter("stock.serialno", barcode, "=");
            }
            qb.AddParameter("salablegoods.locationid", locationid, "=");
            qb.Limit = 1;
            using (NpgsqlConnection conn = CreateConnection())
            {
                conn.Open();
                command = qb.GetCommand(conn);
                if (Services.ConfigManager.Instance(Context.OrganizationID).Config.analyser_stock_removeaqty)
                    command.CommandText = command.CommandText.Replace("salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty)", "salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty+salablegoods.aqty)");

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        SalesorderStock oSalesorder = new SalesorderStock();
                        oSalesorder.productid = reader["productid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productid"]);
                        oSalesorder.sizeid = reader["sizeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sizeid"]);
                        oSalesorder.salablegoodsid = reader["salablegoodsid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["salablegoodsid"]);
                        oSalesorder.colourid = reader["colourid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["colourid"]);
                        oSalesorder.sizegroup = reader["sizegroup"] == DBNull.Value ? "" : reader["sizegroup"].ToString();
                        oSalesorder.designid = reader["designid"] == DBNull.Value ? "" : reader["designid"].ToString();
                        oSalesorder.barcode = reader["barcode"] == DBNull.Value ? "" : reader["barcode"].ToString();
                        oSalesorder.colourname = reader["colourname"] == DBNull.Value ? "" : reader["colourname"].ToString();
                        oSalesorder.sizename = reader["sizename"] == DBNull.Value ? "" : reader["sizename"].ToString();
                        oSalesorder.stockqty = reader["stockqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["stockqty"]);
                        oSalesorder.sizegroup = oSalesorder.sizename + "(" + oSalesorder.sizegroup + ")";
                        salesorderStock.Add(oSalesorder);
                    }
                }
                conn.Close();
            }

            if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_hyper_uan &&
                salesorderStock.Count == 0 && retry == false)
            {
                SellablebarcodesDataService sellablebarcodesDataService = new SellablebarcodesDataService() { Context = this.Context };
                var alternatebarcodes = sellablebarcodesDataService.Search(new SellablebarcodesCreteria()
                {
                    barcode = barcode
                });

                if (alternatebarcodes.Count > 0)
                {
                    var responsealternate = SearchPlannedOrder(searchBarcodeData,alternatebarcodes[0].corebarcode, locationid, sono, true);
                    responsealternate.ForEach(x => x.obarcode = barcode);
                    return responsealternate;
                }
            }


            SalablegoodsDataService salablegoodsDataService = new SalablegoodsDataService() { Context = this.Context };
            var salableresult = salablegoodsDataService.BarcodeSearch(salesorderStock.ToList().ConvertAll(e => e.salablegoodsid).ToList());

            if (retry && salableresult.Count == 0)
            {
                if (salablegoodsDataService.IncreseUANStok(barcode, locationid))
                {
                    return SearchPlannedOrder(searchBarcodeData,barcode, locationid, sono, false);
                }
            }
            foreach (var item in salableresult)
            {
                string groupkey = string.Format("{0}/{1}/{2}", salesorderStock.First().designid,
                    salesorderStock.First().productid, salesorderStock.First().colourid);

                string groupname = string.Format("{0}/{1}", salesorderStock.First().designid
                    , salesorderStock.First().colourname);

                item.stockqty = salesorderStock.First().stockqty;
                item.qty = 1;
                item.groupkey = groupkey;
                if (customerSOList.Count > 0)
                {
                    item.price = customerSOList[0].price;
                    item.groupkey = customerSOList[0].designitemcode;
                    item.addnlcharge = customerSOList[0].addnlcharge;
                    item.addnlchargeid = customerSOList[0].addnlchargeid;
                    item.soboxid = customerSOList[0].soboxid;
                    item.fgboxid = customerSOList[0].fgboxid;
                    item.discountpercentage = customerSOList[0].dp;
                    item.colourcode = customerSOList[0].colourcode;
                    item.colourname = customerSOList[0].colourname;
                    item.colourid = customerSOList[0].colourid;
                    if (customerSOList[0].dp > 0)
                    {
                        item.discount = Math.Round(item.price * customerSOList[0].dp / 100);
                    }
                    item.priceextax = item.price;
                    item.wsrateextax = item.price;
                    item.dealerpriceextax = item.price;
                    item.wsrate = item.price;
                    item.dealerprice = item.mrp;
                }
                else if (!string.IsNullOrEmpty(salesorderStock.First().designid))
                {
                    MfgdesignDataService mfgdesignDataService = new MfgdesignDataService() { Context = this.Context };
                    var uanitem = mfgdesignDataService.SearchUANItem(barcode, salesorderStock.First().designid);
                    if (uanitem == null)
                    {
                        uanitem = mfgdesignDataService.SearchUANItem(barcode, "");
                    }
                    if (uanitem != null)
                    {
                        item.buyingprice = uanitem.cost;
                        item.price = uanitem.salerate;
                        item.wsrate = uanitem.wsrate;
                        item.dealerprice = uanitem.dealerprice;
                        item.groupkey = uanitem.name;
                        groupname = uanitem.name;
                        item.priceextax = uanitem.salerate;
                        item.wsrateextax = uanitem.wsrate;
                        item.dealerpriceextax = uanitem.dealerprice;
                        item.mrp = uanitem.mrp;
                        item.fgboxid = uanitem.boxid;
                        item.soboxid = uanitem.boxid;
                    }
                }
                item.groupname = groupname;
                item.designid = salesorderStock.First().designid;
                item.sizegroup = salesorderStock.First().sizegroup;
                item.colourname = salesorderStock.First().colourname;
                item.sizename = salesorderStock.First().sizename;
            }

            //var groupedstocks = salesorderStock.GroupBy(e => new { e.productid, e.colourid, e.designid }).ToList();
            //ReferencelistDataService referencelistDataService = new ReferencelistDataService() { Context = this.Context };
            //var sizegroups = referencelistDataService.SearchSizeGroups();
            //var plangroups = allcustomerorders.GroupBy(e => new { e.productid, e.designid, e.colourid, e.sizegroup }).ToList();

            //foreach (var item in lst)
            //{
            //    if (sizegroups.Exists(e => e.name == item.sizegroup) &&
            //        groupedstocks.Exists(e => e.Key.colourid == item.colourid &&
            //        e.Key.designid == item.designid &&
            //        e.Key.productid == item.productid))
            //    {
            //        var sizegroup = sizegroups.Find(e => e.name == item.sizegroup);
            //        var groupstocks = groupedstocks.Find(e => e.Key.colourid == item.colourid &&
            //        e.Key.designid == item.designid &&
            //        e.Key.productid == item.productid);
            //        var allsizestock = groupstocks.ToList().FindAll(e => sizegroup.list.Exists(x => x.id == e.sizeid));
            //        item.availableqty = Math.Floor(allsizestock.Average(e => e.stockqty));
            //        if (plangroups.Exists(e => e.Key.colourid == item.colourid &&
            //         e.Key.productid == item.productid && e.Key.designid == item.designid &&
            //         e.Key.sizegroup == item.sizegroup))
            //        {
            //            item.allocatedqty = plangroups.Find(e => e.Key.colourid == item.colourid &&
            //            e.Key.productid == item.productid && e.Key.designid == item.designid &&
            //            e.Key.sizegroup == item.sizegroup).Sum(e => e.plannedqty);
            //        }
            //    }
            //}


            salableresult.FindAll(x => x.wsrate == 0 && x.dealerprice == 0 && x.price > 0).ForEach(x =>
            {
                x.wsrate = x.price;
                x.dealerprice = x.price;
                x.wsrateextax = x.price;
                x.dealerpriceextax = x.price;
                x.priceextax = x.price;
            });
            return ChangePriceFromDesign(salableresult,
                searchBarcodeData,
                InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.mfg_salepricefromsku);
        }


        //public bool AutoStock(long locationid, string serialno)
        //{
        //    try
        //    {
        //        StockDataService stockdataservice = new StockDataService() { Context = this.Context };
        //        stockdataservice.Context = this.Context;

        //        long companyid = 0;
        //        if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_supportcompanywisebarcode)
        //        {
        //            if (stockdataservice.Context.CompanyID <= 0)
        //                companyid = long.MaxValue;
        //            else
        //                companyid = stockdataservice.Context.CompanyID;
        //        }


        //        var barcodestock = stockdataservice.Search(companyid, barcode);
        //        if (barcodestock.id <= 0) return false;

        //        var existingsalableitem = Search(new SalablegoodsCreteria()
        //        {
        //            barcode = barcode,
        //            locationid = locationid
        //        });

        //        if (existingsalableitem.Count == 0)
        //        {
        //            Salablegoods sitem = new Salablegoods();
        //            sitem.inwardon = DateTimeNow;
        //            sitem.barcode = barcode;
        //            sitem.bundleid = -2;
        //            if (barcodestock.isbit)
        //                sitem.iqty = barcodestock.qty;
        //            else
        //                sitem.iqty = 1;
        //            sitem.isactive = true;
        //            sitem.itemid = barcodestock.itemid;
        //            sitem.locationid = locationid;
        //            sitem.stockid = barcodestock.id;
        //            sitem.version = 1;
        //            sitem.companyid = barcodestock.companyid;
        //            sitem.supplierid = barcodestock.supplierid;
        //            sitem.received = true;
        //            sitem.price = barcodestock.price;
        //            sitem.discount = barcodestock.discount;
        //            sitem.additionaldiscount = barcodestock.additionaldiscount;
        //            Insert(sitem);
        //        }
        //        else
        //        {
        //            existingsalableitem[0].iqty += barcodestock.qty;
        //            Update(existingsalableitem[0], existingsalableitem[0]);
        //        }

        //        return true;
        //    }
        //    catch (Exception exp)
        //    {
        //        return false;
        //    }
        //}
        internal List<Barcodedgoods> ChangePriceFromDesign(List<Barcodedgoods> salableresult, 
            SearchBarcodeData searchBarcodeData, bool allbarcodes)
        {
            ProductsDataService productsDataService = new ProductsDataService() { Context = this.Context };
            TaxDataService taxDataService = new TaxDataService() { Context = this.Context };
            var tax = taxDataService.RangedTaxProducts(salableresult.ConvertAll(e => e.barcode).Distinct().ToList());
            MfgdesignDataService mfgdesignDataService = new MfgdesignDataService() { Context = this.Context };
            SortedDictionary<string, Mfgdesign> keyValues = new SortedDictionary<string, Mfgdesign>();
            foreach (var item in salableresult)
            {
                if (!keyValues.ContainsKey(item.designid))
                {
                    if (string.IsNullOrEmpty(item.designid))
                    {
                        List<Mfgdesign> designs = null;

                        if (!string.IsNullOrEmpty(item.serialno))
                        {
                            designs = mfgdesignDataService.Search(new MfgdesignCreteria()
                            {
                                uancode = item.serialno,
                                includedetail = true
                            });
                        }
                        else if (!string.IsNullOrEmpty(item.barcode))
                        {
                            designs = mfgdesignDataService.Search(new MfgdesignCreteria()
                            {
                                uancode = item.barcode,
                                includedetail = true
                            });
                        }
                        if (designs.Count == 0) continue;
                        keyValues.Add(item.designid, designs.First());
                    }
                    else
                    {
                        var designs = mfgdesignDataService.Search(new MfgdesignCreteria()
                        {
                            code = item.designid,
                            includedetail = true
                        });
                        if (designs.Count == 0) continue;
                        keyValues.Add(item.designid, designs.First());
                    }
                }
                var existserial = keyValues[item.designid].sizegroup.items.Exists(e => e.uid == item.serialno);
                var existsize = keyValues[item.designid].sizegroup.items.Exists(e => e.colourid == item.colourid
                    && e.sizeid == item.sizeid);

                if (!existserial && InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_hyper_uan)
                {
                    existserial = keyValues[item.designid].sizegroup.items.Exists(e => e.uid == item.barcode);
                }
                if (existserial || existsize)
                {
                    var designsizegroup = existserial ?
                        keyValues[item.designid].sizegroup.items.Find(e => e.uid == item.serialno) :
                        keyValues[item.designid].sizegroup.items.Find(e => e.colourid == item.colourid && e.sizeid == item.sizeid);

                    if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_hyper_uan &&
                         keyValues[item.designid].sizegroup.items.Exists(e => e.uid == item.barcode))
                    {
                        designsizegroup = keyValues[item.designid].sizegroup.items.Find(e => e.uid == item.barcode);
                    }

                    if (string.IsNullOrEmpty(item.colourcode) &&
                        designsizegroup != null && designsizegroup.code.IndexOf('/') > 0)
                        item.colourcode = designsizegroup.code.Substring(designsizegroup.code.IndexOf('/') + 1);

                    if (string.IsNullOrEmpty(item.groupkey))
                    {
                        item.groupkey = designsizegroup.code;
                        item.groupname = item.designid + "/" + item.colourname;
                        item.sizeid = designsizegroup.sizeid;
                        item.colourid = designsizegroup.colourid;
                    }
                    if (item.price == 0 || item.wsrate == 0 || item.dealerprice == 0 || allbarcodes)
                    {
                        item.buyingprice = designsizegroup.cost;
                        item.price = designsizegroup.salerate;
                        item.wsrate = designsizegroup.wsrate;
                        item.dealerprice = designsizegroup.dealerprice;
                        item.mrp = designsizegroup.mrp;
                        item.priceextax = designsizegroup.salerate;
                        item.wsrateextax = designsizegroup.wsrate;
                        item.dealerpriceextax = designsizegroup.dealerprice;

                        if (tax.ContainsKey(item.barcode))
                        {
                            var taxvalue = tax[item.barcode];
                            var taxpercentage = taxvalue.taxsplit.Find(item.price).taxpercent;
                            item.taxid = taxvalue.id;
                            item.priceextax = item.price;
                            item.price = Math.Round(item.price * (100 + taxpercentage) / 100, 4);


                            taxpercentage = taxvalue.taxsplit.Find(item.wsrate).taxpercent;
                            taxpercentage = taxvalue.taxsplit.Find(item.wsrate).taxpercent;
                            item.taxid = taxvalue.id;
                            item.wsrateextax = item.wsrate;
                            item.wsrate = Math.Round(item.wsrate * (100 + taxpercentage) / 100, 4);

                            taxpercentage = taxvalue.taxsplit.Find(item.dealerprice).taxpercent;
                            item.taxid = taxvalue.id;
                            item.dealerpriceextax = item.dealerprice;
                            item.dealerprice = Math.Round(item.dealerprice * (100 + taxpercentage) / 100, 4);
                        }
                    }
                }

                if (item.discountpercentage > 0)
                    item.discount = 0;
                //if (item.discountpercentage > 0)
                //{
                //    item.price = item.price - Math.Round(item.price * item.discountpercentage / 100, 2);
                //    item.wsrate = item.wsrate - Math.Round(item.wsrate * item.discountpercentage / 100, 2);
                //    item.dealerprice = item.dealerprice - Math.Round(item.dealerprice * item.discountpercentage / 100, 2);
                //    item.mrp = item.mrp;
                //    item.priceextax = item.priceextax - Math.Round(item.priceextax * item.discountpercentage / 100, 2);
                //    item.wsrateextax = item.wsrateextax - Math.Round(item.wsrateextax * item.discountpercentage / 100, 2);
                //    item.dealerpriceextax = item.dealerpriceextax - Math.Round(item.dealerpriceextax * item.discountpercentage / 100, 2);
                //}
            }

            if(searchBarcodeData.cccomputemode>0 && searchBarcodeData.ccmarginmrp>0)
            {
                UpdateBarcodePrices(searchBarcodeData, salableresult);
            }
            return salableresult;
        }
        internal List<SalesOrderSummary> CancelSO(List<SalesOrderSummary> items)
        {
            List<SalesorderitemsDetail> changeditems = new List<SalesorderitemsDetail>();
            List<long> cancelledsos = new List<long>();
            foreach (var item in items)
            {
                var saleorder = Search(new SalesorderCreteria()
                {
                    id = item.soid
                }).First();

                List<SalesorderitemsDetail> list = null;
                if (!string.IsNullOrEmpty(item.ids))
                {
                    var ids = item.ids.Split(',').ToList().ToList().ConvertAll(x => long.Parse(x));
                    list = saleorder.Salesorderitems.FindAll(e => ids.Contains(e.id));
                }
                else
                {
                    list = saleorder.Salesorderitems.FindAll(e => e.sizegroup == item.sizegroup &&
                    e.productname == item.productname &&
                    e.designid == item.designid &&
                    e.colourid == item.colourid);
                }

                foreach (var saleorderitem in list)
                {
                    saleorderitem.cqty += saleorderitem.pendingqty;
                    changeditems.Add(saleorderitem);
                }
                if (!saleorder.Salesorderitems.Any(x => x.pendingqty > 0))
                    cancelledsos.Add(saleorder.id);
            }


            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    string query = "update Salesorderitems set cqty=@cqty,modifiedon=@modifiedon,modifiedby=@modifiedby where id=@id";
                    var command = GetCommand(query, conn, transaction);
                    command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                    command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = 0;
                    command.Parameters.Add("cqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = 0;
                    foreach (var item in changeditems)
                    {
                        command.Parameters["id"].Value = item.id;
                        command.Parameters["cqty"].Value = item.cqty;
                        command.ExecuteNonQuery();
                    }
                    query = "update salesorder set status=3,modifiedon=@modifiedon,modifiedby=@modifiedby where id=@id";
                    command = GetCommand(query, conn, transaction);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = 0;
                    command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                    command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
                    foreach (var item in cancelledsos)
                    {
                        command.Parameters["id"].Value = item;
                        command.ExecuteNonQuery();
                    }
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, items);
            }
            return items;
        }


        internal List<SalesOrderSummary> CompleteSO(List<SalesOrderSummary> items)
        {
            List<SalesorderitemsDetail> changeditems = new List<SalesorderitemsDetail>();
            List<long> cancelledsos = new List<long>();
            foreach (var item in items)
            {
                var saleorder = Search(new SalesorderCreteria()
                {
                    id = item.soid
                }).First();

                List<SalesorderitemsDetail> list = null;
                if (!string.IsNullOrEmpty(item.ids))
                {
                    var ids = item.ids.Split(',').ToList().ToList().ConvertAll(x => long.Parse(x));
                    list = saleorder.Salesorderitems.FindAll(e => ids.Contains(e.id));
                }
                else
                {
                    list = saleorder.Salesorderitems.FindAll(e => e.sizegroup == item.sizegroup &&
                    e.productname == item.productname &&
                    e.designid == item.designid &&
                    e.colourid == item.colourid);
                }

                foreach (var saleorderitem in list)
                {
                    saleorderitem.rqty += saleorderitem.pendingqty;
                    changeditems.Add(saleorderitem);
                }
                if (!saleorder.Salesorderitems.Any(x => x.pendingqty > 0))
                    cancelledsos.Add(saleorder.id);
            }


            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    string query = "update Salesorderitems set rqty=@rqty where id=@id";
                    var command = GetCommand(query, conn, transaction);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = 0;
                    command.Parameters.Add("rqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = 0;
                    foreach (var item in changeditems)
                    {
                        command.Parameters["id"].Value = item.id;
                        command.Parameters["rqty"].Value = item.rqty;
                        command.ExecuteNonQuery();
                    }

                    query = "update salesorder set status=4 where id=@id";
                    command = GetCommand(query, conn, transaction);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = 0;
                    foreach (var item in cancelledsos)
                    {
                        command.Parameters["id"].Value = item;
                        command.ExecuteNonQuery();
                    }
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, items);
            }
            return items;
        }
        internal List<SalesOrderSummary> ChangePlan(List<SalesOrderSummary> items)
        {
            List<SalesorderitemsDetail> changeditems = new List<SalesorderitemsDetail>();
            foreach (var item in items)
            {
                var saleorder = Search(new SalesorderCreteria()
                {
                    id = item.soid
                }).First();

                var salesorderitemids = item.ids.Split(',').ToList().ConvertAll(x =>
                {
                    if (string.IsNullOrEmpty(x)) return 0;
                    return long.Parse(x);
                });
                var plannedqty = item.plannedqty;
                var list = saleorder.Salesorderitems.FindAll(e => salesorderitemids.Contains(e.id));
                foreach (var sitem in list)
                {
                    var pendingqty = sitem.oqty - (sitem.cqty + sitem.bqty + sitem.pqty + sitem.rqty);
                    if (pendingqty <= plannedqty)
                    {
                        sitem.pqty += pendingqty;
                        plannedqty -= pendingqty;
                    }
                    else
                    {
                        sitem.pqty += plannedqty;
                        plannedqty = 0;
                    }
                    changeditems.Add(sitem);
                }
            }




            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    string query = "update Salesorderitems set pqty=@pqty where id=@id";
                    var command = GetCommand(query, conn, transaction);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = 0;
                    command.Parameters.Add("pqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = 0;
                    foreach (var item in changeditems)
                    {
                        command.Parameters["id"].Value = item.id;
                        command.Parameters["pqty"].Value = item.pqty;
                        command.ExecuteNonQuery();
                    }
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, items);
            }
            return items;
        }

        internal string DeleteMobilePO(long salesorderid)
        {
            var so = Search(new SalesorderCreteria()
            {
                id = salesorderid
            });
            if (so.Count == 1)
            {
                return Delete(so.First()).ToString();
            }
            return "false";
            //SalesorderitemsDataService SalesorderitemsDataService = new SalesorderitemsDataService() { Context = this.Context };
            //var item = SalesorderitemsDataService.Search(new SalesorderitemsCreteria()
            //{
            //    id = Salesorderitemid
            //}).First();
            //return SalesorderitemsDataService.Delete(item).ToString();
        }

        internal Salesorder AddMobilePO(SalesorderItemSimple item)
        {
            if (item.customerid <= 0) throw new Exception("Please enter the customer name");
            long currentpoid = 0;
            var param = new SortedList<string, object>();
            param.Add("customerid", item.customerid);
            param.Add("fromdate", DateTimeNow.Date);
            param.Add("todate", DateTimeNow.Date.AddDays(1));
            param.Add("createdby", Context.UserID);
            var designitemcode = item.designitemcode;
            string query = "select id from salesorder where ismobilepo=true and sizegroup=@sizegroup and sodate>=@fromdate and sodate<@todate and customerid=@customerid and createdby=@createdby order by id desc limit 1";
            if (string.IsNullOrEmpty(item.sizegroup) || item.sizegroup == item.designid
                 || item.sizegroup == item.designitemcode)
                query = "select id from salesorder where ismobilepo=true and sodate>=@fromdate and sodate<@todate and customerid=@customerid and createdby=@createdby order by id desc limit 1";
            else
                param.Add("sizegroup", item.sizegroup);
            using (var reader = ExecuteReader(query, param))
            {
                if (reader.Read())
                {
                    currentpoid = reader.GetInt64(0);
                }
            }

            ProductsDataService productsDataService = new ProductsDataService() { Context = this.Context };
            item.product = productsDataService.Search(new ProductsCreteria()
            {
                id = item.productid
            }).First().name;
            if (item.brandid > 0)
            {
                BrandDataService brandData = new BrandDataService() { Context = this.Context };
                item.brand = brandData.Search(new BrandCreteria()
                {
                    id = item.brandid
                }).First().name;
            }


            if (item.sizelist != null && item.sizelist != null && item.sizelist.Count > 0)
            {
                item.sizelist.RemoveAll(e => e.qty == 0);
                ReferencelistDataService referencelistDataService = new ReferencelistDataService() { Context = this.Context };
                var list = referencelistDataService.SearchList(new ReferencelistCreteria()
                {
                    type = "SIZE"
                });
                foreach (var itm in item.sizelist.Where(x => x.id <= 0))
                {
                    if (list.Exists(e => e.name.ToLower() == itm.name.ToLower()))
                        itm.id = list.Find(e => e.name.ToLower() == itm.name.ToLower()).id;
                    else
                    {
                        itm.id = referencelistDataService.Insert(new Referencelist()
                        {
                            type = "SIZE",
                            name = itm.name,
                            isactive = true,
                            createdby = Context.UserID,
                            modifiedby = Context.UserID,
                            createdon = DateTimeNow,
                            modifiedon = DateTimeNow
                        }).id;
                    }
                }
            }
            if (currentpoid == 0)
            {
                var po = new Salesorder();
                po.sodate = DateTimeNow.Date;
                po.customerid = item.customerid;
                po.companyid = item.companyid;
                po.createdby = Context.UserID;
                po.modifiedby = Context.UserID;
                po.createdon = DateTimeNow;
                po.modifiedon = DateTimeNow;
                po.version = 1;
                po.sizegroup = item.sizegroup;
                po.isactive = true;
                po.salesmenid = item.salesmenid;
                po.agentid = item.agentid;
                po.mode = !string.IsNullOrEmpty(item.sizegroup) && item.sizegroup.IndexOf('-') > 0 ? 0 : 2;
                po.Salesorderitems = new List<SalesorderitemsDetail>();
                if (item.sizelist != null && item.sizelist.Count > 0)
                {
                    item.sizelist.ForEach(e =>
                    {
                        var uanitem = item.source.sizegroup.Find(x => x.list.Exists(y => y.uid == e.uid));
                        if (uanitem == null)
                            uanitem = item.source.sizegroup.Find(x => x.designcode == item.designitemcode);
                        if (uanitem != null)
                        {
                            if (item.designitemcode.IndexOf(",") < 0)
                                item.designitemcode = uanitem.designcode;
                            item.colourid = uanitem.colourid;
                            item.colourcode = uanitem.colourcode;
                            item.uid = uanitem.list.Find(x => x.id == e.id).uid;
                            item.sizegroupid = 0;
                            item.sizegroup = uanitem.name;
                            e.uid = item.uid;
                        }
                        po.Salesorderitems.Add(new SalesorderitemsDetail()
                        {
                            productid = item.productid,
                            brandid = item.brandid,
                            productname = item.product,
                            colourname = item.colour,
                            colourid = item.colourid,
                            brandname = item.brand,
                            salesorderid = currentpoid,
                            designid = item.source.design,
                            addnlchargeid = item.addnlchargeid,
                            colourcode = item.colourcode,
                            colourshade = item.colourcode,
                            sizegroup = item.sizegroup,
                            uid = e.uid,
                            sizegroupid = item.sizegroupid,
                            designitemcode = item.designitemcode,
                            oqty = e.qty,
                            price = item.price,
                            printingname = item.product,
                            description = item.brandid > 0 ? (item.product + "/" + item.brand) : (item.product),
                            size = e.name,
                            mrp = item.mrp,
                            sizename = e.name,
                            sizeid = e.id,
                            imageid = item.imageid,
                            isactive = true,
                            version = 1,
                            createdby = Context.UserID,
                            modifiedby = Context.UserID,
                            createdon = DateTimeNow,
                            modifiedon = DateTimeNow
                        });
                    });
                }
                else
                {
                    po.Salesorderitems.Add(new SalesorderitemsDetail()
                    {
                        productid = item.productid,
                        brandid = item.brandid,
                        addnlchargeid = item.addnlchargeid,
                        productname = item.product,
                        salesorderid = currentpoid,
                        colourname = item.colour,
                        colourid = item.colourid,
                        brandname = item.brand,
                        sizegroup = item.sizegroup,
                        sizegroupid = item.sizegroupid,
                        oqty = item.qty,
                        uid = item.uid,
                        price = item.price,
                        printingname = item.product,
                        designid = item.designitemcode.Split('/')[0],
                        colourcode = item.designitemcode.IndexOf('/') > 0 ? item.designitemcode.Substring(item.designitemcode.IndexOf('/') + 1) : "",
                        description = item.brandid > 0 ? (item.product + "/" + item.brand) : (item.product),
                        sizeid = item.sizeid,
                        size = item.size,
                        sizename = item.size,
                        imageid = item.imageid,
                        isactive = true,
                        version = 1,
                        createdby = Context.UserID,
                        modifiedby = Context.UserID,
                        createdon = DateTimeNow,
                        modifiedon = DateTimeNow
                    });
                }
                if (po.Salesorderitems.Exists(x => x.designitemcode != null && x.designitemcode.IndexOf(',') > 0))
                {
                    var multipleDesignItems = po.Salesorderitems.FindAll(x => x.designitemcode != null && x.designitemcode.IndexOf(',') > 0);
                    po.Salesorderitems.RemoveAll(x => x.designitemcode != null && x.designitemcode.IndexOf(',') > 0);
                    foreach (var designiditem in multipleDesignItems.GroupBy(x => x.designid))
                    {
                        List<SalesorderitemsDetail> sizes = new List<SalesorderitemsDetail>();
                        foreach (var designitem in designiditem)
                        {
                            foreach (var dcitem in designitem.designitemcode.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                            {
                                var findex = item.sizelist.Find(x => x.qty > 0);
                                var lindex = item.sizelist.FindLast(x => x.qty > 0);
                                foreach (var sitem in item.sizelist)
                                {
                                    if (sitem.qty > 0)
                                    {
                                        var duplicateitem = Newtonsoft.Json.JsonConvert.DeserializeObject<SalesorderitemsDetail>(Newtonsoft.Json.JsonConvert.SerializeObject(designitem));
                                        duplicateitem.designitemcode = dcitem;
                                        var designsizegroup = item.source.sizegroup.Find(x => x.designcode == dcitem);
                                        duplicateitem.uid = designsizegroup.list.Find(x => x.id == duplicateitem.sizeid).uid;
                                        duplicateitem.colourcode = designsizegroup.colourcode;
                                        duplicateitem.colourid = designsizegroup.colourid;
                                        duplicateitem.sizeid = sitem.id;
                                        duplicateitem.size = sitem.name;
                                        duplicateitem.oqty = sitem.qty;
                                        duplicateitem.sizegroup = findex.id != lindex.id ? (findex.name + "-" + lindex.name) : findex.name;
                                        duplicateitem.uid = designsizegroup.list.Find(x => x.id == sitem.id).uid;
                                        po.Salesorderitems.Add(duplicateitem);
                                    }
                                }
                            }
                        }
                    }
                }
                //else
                //{
                //    var designsizegroup = item.source.sizegroup.Find(x => x.designcode == item.designitemcode);
                //    if (designsizegroup.list.Exists(x => x.id == duplicateitem.sizeid))
                //    {
                //        duplicateitem.uid = designsizegroup.list.Find(x => x.id == duplicateitem.sizeid).uid;
                //        duplicateitem.colourcode = designsizegroup.colourcode;
                //        duplicateitem.colourid = designsizegroup.colourid;
                //        po.Salesorderitems.Add(duplicateitem);
                //        sizes.Add(duplicateitem);
                //    }
                //}

                Insert(po);
                ExecuteQuery("update salesorder set ismobilepo=true where id=" + po.id.ToString(), null);

                return po;
            }
            else
            {
                var po = Search(new SalesorderCreteria()
                {
                    id = currentpoid
                }).First();


                //if (item.sizeid > 0)
                //{
                //    po.Salesorderitems.RemoveAll(e => e.productid == item.productid &&
                //    e.brandid == item.brandid && e.sizeid == item.sizeid && e.designid == item.designid);
                //}
                //else
                //{
                //    var designitemcode = item.designitemcode.Split(',').ToList();
                //    po.Salesorderitems.RemoveAll(e => e.productid == item.productid &&
                //    e.brandid == item.brandid && e.price == item.price && e.designid == item.designid &&
                //    designitemcode.Contains(e.designitemcode));
                //}


                if (item.sizelist != null && item.sizelist.Count > 0)
                {
                    item.sizelist.ForEach(e =>
                    {
                        if (po.Salesorderitems.Exists(ex => ex.uid == e.uid ||
                        (ex.productid == item.productid && ex.designitemcode == item.designitemcode && ex.designid == item.designid &&
                        (item.colourid == 0 || ex.colourid == item.colourid) &&
                        ex.sizeid == e.id && ex.brandid == item.brandid)))
                        {
                            var mitem = po.Salesorderitems.Find(ex => ex.uid == e.uid ||
                        (ex.productid == item.productid && ex.designitemcode == item.designitemcode && ex.designid == item.designid &&
                        (item.colourid == 0 || ex.colourid == item.colourid) &&
                        ex.sizeid == e.id && ex.brandid == item.brandid));
                            mitem.oqty += e.qty;
                            //mitem.addnlchargeid = item.addnlchargeid;
                        }
                        else
                        {
                            var uanitem = item.source.sizegroup.Find(x => x.list.Exists(y => y.uid == e.uid));
                            if (uanitem == null)
                                uanitem = item.source.sizegroup.Find(x => x.designcode == item.designitemcode);
                            if (uanitem != null)
                            {
                                if (item.designitemcode.IndexOf(",") < 0)
                                    item.designitemcode = uanitem.designcode;
                                item.colourid = uanitem.colourid;
                                item.colourcode = uanitem.colourcode;
                                item.uid = uanitem.list.Find(x => x.id == e.id).uid;
                                item.sizegroupid = 0;
                                item.sizegroup = uanitem.name;
                                e.uid = item.uid;
                            }
                            po.Salesorderitems.Add(new SalesorderitemsDetail()
                            {
                                productid = item.productid,
                                brandid = item.brandid,
                                productname = item.product,
                                colourname = item.colour,
                                colourid = item.colourid,
                                addnlchargeid = item.addnlchargeid,
                                salesorderid = currentpoid,
                                brandname = item.brand,
                                designid = item.designid,
                                designitemcode = item.designitemcode,
                                colourcode = item.colourcode,
                                colourshade = item.colourcode,
                                oqty = e.qty,
                                uid = e.uid,
                                price = item.price,
                                pricerange = item.price.ToString(),
                                sizegroup = item.sizegroup,
                                sizegroupid = item.sizegroupid,
                                printingname = item.product,
                                description = item.brandid > 0 ? (item.product + "/" + item.brand) : (item.product),
                                size = e.name,
                                mrp = item.mrp,
                                sizename = e.name,
                                sizeid = e.id,
                                imageid = item.imageid,
                                isactive = true,
                                version = 1,
                                createdby = Context.UserID,
                                modifiedby = Context.UserID,
                                createdon = DateTimeNow,
                                modifiedon = DateTimeNow
                            });
                        }
                    });
                }
                else
                {
                    po.Salesorderitems.Add(new SalesorderitemsDetail()
                    {
                        productid = item.productid,
                        brandid = item.brandid,
                        salesorderid = currentpoid,
                        productname = item.product,
                        colourname = item.colour,
                        colourid = item.colourid,
                        brandname = item.brand,
                        designid = item.designid,
                        addnlchargeid = item.addnlchargeid,
                        oqty = item.qty,
                        price = item.price,
                        pricerange = item.price.ToString(),
                        sizegroup = item.sizegroup,
                        sizegroupid = item.sizegroupid,
                        printingname = item.product,
                        description = item.brandid > 0 ? (item.product + "/" + item.brand) : (item.product),
                        sizeid = item.sizeid,
                        uid = item.uid,
                        size = item.size,
                        sizename = item.size,
                        imageid = item.imageid,
                        isactive = true,
                        version = 1,
                        createdby = Context.UserID,
                        modifiedby = Context.UserID,
                        createdon = DateTimeNow,
                        modifiedon = DateTimeNow
                    });
                }

                if (po.Salesorderitems.Exists(x => x.designitemcode != null && x.designitemcode.IndexOf(',') > 0))
                {
                    var multipleDesignItems = po.Salesorderitems.FindAll(x => x.designitemcode != null && x.designitemcode.IndexOf(',') > 0);
                    po.Salesorderitems.RemoveAll(x => x.designitemcode != null && x.designitemcode.IndexOf(',') > 0);
                    foreach (var designiditem in multipleDesignItems.GroupBy(x => x.designid))
                    {
                        List<SalesorderitemsDetail> sizes = new List<SalesorderitemsDetail>();
                        foreach (var designitem in designiditem)
                        {
                            foreach (var dcitem in designitem.designitemcode.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                            {
                                var findex = item.sizelist.Find(x => x.qty > 0);
                                var lindex = item.sizelist.FindLast(x => x.qty > 0);
                                foreach (var sitem in item.sizelist)
                                {
                                    if (sitem.qty > 0)
                                    {
                                        var duplicateitem = Newtonsoft.Json.JsonConvert.DeserializeObject<SalesorderitemsDetail>(Newtonsoft.Json.JsonConvert.SerializeObject(designitem));
                                        duplicateitem.designitemcode = dcitem;
                                        var designsizegroup = item.source.sizegroup.Find(x => x.designcode == dcitem);
                                        duplicateitem.colourcode = designsizegroup.colourcode;
                                        duplicateitem.colourid = designsizegroup.colourid;
                                        duplicateitem.sizeid = sitem.id;
                                        duplicateitem.size = sitem.name;
                                        duplicateitem.oqty = sitem.qty;
                                        duplicateitem.sizegroup = findex.id != lindex.id ? (findex.name + "-" + lindex.name) : findex.name;
                                        duplicateitem.uid = designsizegroup.list.Find(x => x.id == sitem.id).uid;
                                        po.Salesorderitems.Add(duplicateitem);
                                    }
                                }
                            }
                        }
                    }
                }


                Update(po);
                return po;
            }
        }

        internal List<SalesorderItemSimple> SearchMobilePO(long customerid, long poid, long limit)
        {
            List<SalesorderItemSimple> lst = new List<SalesorderItemSimple>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                if (limit == 1 && poid == 0)
                {
                    var orders = Search(new SalesorderCreteria()
                    {
                        customerid = customerid,
                        limit = 1
                    });
                    if (orders.Count == 1)
                    {
                        poid = orders.First().id;
                    }
                    else
                    {
                        return new List<SalesorderItemSimple>();
                    }
                }
                conn.Open();
                NpgsqlCommand command = null;

                QueryBuilder qb = new QueryBuilder();
                qb.CurrentQuery = SQL_SearchMobilePOSummary;
                qb.AddParameter("Salesorderitems.isactive", true, "=");
                qb.AddParameter("salesorder.isactive", true, "=");
                if (poid > 0)
                {
                    qb.AddParameter("salesorder.id", "id", poid, "=");
                }
                else
                {
                    qb.AddParameter("salesorder.createdby", "createdby", Context.UserID, "=");
                    qb.AddParameter("salesorder.ismobilepo", "ismobilepo", true, "=");
                    if (customerid > 0)
                        qb.AddParameter("salesorder.customerid", "salesorder.customerid", customerid, "=");

                    qb.AddParameter("salesorder.isactive", "salesorder.isactive", true, "=");
                    qb.AddParameter("salesorderitems.isactive", "salesorderitems.isactive", true, "=");

                    qb.AddParameter("salesorder.sodate", "fromdate", DateTimeNow.AddDays(-7), ">=");
                    qb.AddParameter("salesorder.sodate", "todate", DateTimeNow.Date.AddDays(1), "<");
                    qb.SortField = "salesorder.modifiedon desc";
                }
                command = qb.GetCommand(conn);
                if (Services.ConfigManager.Instance(Context.OrganizationID).Config.analyser_stock_removeaqty)
                    command.CommandText = command.CommandText.Replace("salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty)", "salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty+salablegoods.aqty)");

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        SalesorderItemSimple osalesorder = new SalesorderItemSimple();
                        osalesorder.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        osalesorder.soid = reader["soid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["soid"]);
                        osalesorder.sodate = reader["sodate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["sodate"]);
                        osalesorder.sono = reader["sono"] == DBNull.Value ? "" : reader["sono"].ToString();
                        osalesorder.mobileno = reader["mobileno"] == DBNull.Value ? "" : reader["mobileno"].ToString();
                        osalesorder.designitemcode = reader["designitemcode"] == DBNull.Value ? "" : reader["designitemcode"].ToString();

                        long l = 0;
                        foreach (var mobileno in osalesorder.mobileno.Split(new char[] { ' ', ',', '.', '+' }))
                        {
                            if (long.TryParse(mobileno, out l))
                            {
                                osalesorder.mobileno = mobileno;
                                break;
                            }
                        }
                        osalesorder.customerid = reader["customerid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["customerid"]);
                        osalesorder.brandid = reader["brandid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["brandid"]);
                        osalesorder.price = reader["price"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["price"]);
                        osalesorder.qty = reader["oqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["oqty"]);

                        osalesorder.pqty = reader["pqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["pqty"]);
                        osalesorder.cqty = reader["cqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["cqty"]);
                        osalesorder.bqty = reader["bqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["bqty"]);
                        osalesorder.rqty = reader["rqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["rqty"]);
                        osalesorder.sizegroup = reader["sizegroup"] == DBNull.Value ? "" : reader["sizegroup"].ToString();
                        osalesorder.brand = reader["brand"] == DBNull.Value ? "" : reader["brand"].ToString();
                        osalesorder.designid = reader["designid"] == DBNull.Value ? "" : reader["designid"].ToString();
                        osalesorder.imageid = reader["imageid"] == DBNull.Value ? "" : reader["imageid"].ToString();
                        osalesorder.product = reader["product"] == DBNull.Value ? "" : reader["product"].ToString();
                        osalesorder.size = reader["size"] == DBNull.Value ? "" : reader["size"].ToString();
                        osalesorder.customer = reader["customer"] == DBNull.Value ? "" : reader["customer"].ToString();

                        if (osalesorder.cqty > 0)
                            osalesorder.status = osalesorder.bqty > 0 ? SalesorderItemSimple.Status.PartialCancelled : SalesorderItemSimple.Status.FullCancelled;

                        else if (osalesorder.pqty > 0)
                            osalesorder.status = osalesorder.qty - (osalesorder.bqty + osalesorder.pqty) > 0
                                ? SalesorderItemSimple.Status.PartialPlanned : SalesorderItemSimple.Status.FullPlanned;

                        else if (osalesorder.bqty > 0)
                            osalesorder.status = osalesorder.bqty >= osalesorder.qty ?
                                SalesorderItemSimple.Status.PartialDelivery : SalesorderItemSimple.Status.FullDelivery;
                        lst.Add(osalesorder);
                    }
                }




                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, "SearchMobilePO");
            }
            return lst;
        }

        const string SQL_OrderForm = @"select sono,cqty,oqty,rqty,bqty,pqty,oqty-(bqty+rqty+cqty) pendingqty,salesorderitems.description,
orderformimageid,customerid,sodate,size.name sizename,colour.name colourname,brand.name brandname,style.name stylename,Salesorderitems.sizeid,
shiptocustomerid,hasshipto,pattern.name patternname,fit.name fitname,sleeve.name sleevename,customer.name customername,description,salesorder.sono,
salesorderitems.brandid,salesorderitems.designid,salesorderitems.sizegroup,salesorderitems.colourid,styleid,patternid,fitid,salesorder.id soid,
salesorderitems.id soitemid,salesorderitems.productid,sleeveid,salesorderitems.price,salesorderitems.uid,salesorderitems.designitemcode,
salesorderitems.mrp from salesorder
inner join salesorderitems on salesorderitems.salesorderid = salesorder.id
left outer join customer on customer.id = customerid
left outer join items on items.id=salesorderitems.id
left outer join referencelist size on size.id = Salesorderitems.sizeid
left outer join referencelist colour on colour.id = Salesorderitems.colourid
left outer join referencelist style on style.id = items.styleid
left outer join referencelist pattern on pattern.id = items.patternid
left outer join referencelist fit on fit.id = items.fitid
left outer join referencelist sleeve on sleeve.id = items.sleeveid
left outer join brand on brand.id = items.brandid";

        const string SQL_SearchPendingPO = @"select Salesorder.id, sodate, expectedate, sono, customerid, termsandcondition, tolerance, 
deliverylocationid,mode,agentid,freight, Salesorder.locationid,status,
Salesorderitems.productid, itemid, oqty, rqty, cqty, Salesorderitems.price,
Salesorderitems.priceupto,Salesorderitems.designid,Salesorderitems.rowtype,Salesorderitems.size,
Salesorderitems.pricerange,description,Salesorderitems.imageid, Salesorderitems.sizeid, Salesorderitems.sizegroupid,Salesorderitems.sizegroup,
productrequestformids,productname,printingname,brandname,colourname,patternname,sizename,stylename,materialname,typename
from Salesorder
inner join Salesorderitems on Salesorderitems.Salesorderid = Salesorder.id
left outer join items on items.id = Salesorderitems.itemid
where Salesorder.isactive = true and Salesorderitems.isactive = true and oqty-(rqty+cqty)>0";

        const string SQL_Update = @"UPDATE public.Salesorder SET sodate=@sodate, expectedate=@expectedate, sono=@sono, customerid=@customerid, 
            termsandcondition=@termsandcondition, modifiedby=@modifiedby,tolerance=@tolerance, version=@version+1,mode=@mode,sizegroup=@sizegroup,
            modifiedon=@modifiedon, locationid=@locationid,remarks=@remarks, status=@status, deliverylocationid=@deliverylocationid,
            agentid=@agentid,shiptocustomerid=@shiptocustomerid,discountpercentage=@discountpercentage,hasshipto=@hasshipto,
            freight=@freight,companyid=@companyid,totalsetqty=@totalsetqty,totalqty=@totalqty,orderformimageid=@orderformimageid,
            salesmenid=@salesmenid,sobookno=@sobookno,customerpo=@customerpo,sobookdate=@sobookdate,soracknumber=@soracknumber,sobookqty=@sobookqty
            where id=@id and version=@version RETURNING version";

        const string SQL_Delete = @"update public.Salesorder set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";
        const string SQL_UpdateStatus = @"update public.Salesorder set status=@status,version=@version+1,actionby=@modifiedby,actionon=@modifiedon where id=@id and version=@version";

        public List<SalesOrderSummary> SearchSummary(SalesOrderSummaryCreteria _Salesorder)
        {
            List<SalesOrderSummary> lst = new List<SalesOrderSummary>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                NpgsqlCommand command = null;
                QueryBuilder qb = new QueryBuilder();
                qb.CurrentQuery = SQL_SOSummary.Substring(0, SQL_SOSummary.IndexOf("group "));

                if (_Salesorder.fromdate.Year > 2000 && _Salesorder.todate.Year > 2000)
                {
                    qb.AddParameter("sodate", "sodatefrom", _Salesorder.fromdate.Date, ">=");
                    qb.AddParameter("sodate", "sodatetodate", _Salesorder.todate.Date.AddDays(1), "<");
                }

                switch (_Salesorder.searchtype)
                {
                    case 1:
                        if (_Salesorder.customerid > 0)
                            qb.AddParameter("salesorder.customerid", "customerid", _Salesorder.customerid, "=");
                        break;
                    case 2:
                        if (_Salesorder.agentid > 0)
                            qb.AddParameter("salesorder.agentid", "agentid", _Salesorder.agentid, "=");
                        break;
                    case 3:
                        if (_Salesorder.cityid > 0)
                            qb.AddParameter("customer.cityid", "cityid", _Salesorder.cityid, "=");
                        break;
                    case 4:
                        if (_Salesorder.productid > 0)
                            qb.AddParameter("salesorderitems.productid", "productid", _Salesorder.productid, "=");
                        break;
                    case 5:
                        if (!string.IsNullOrEmpty(_Salesorder.design))
                            qb.AddInParamer("salesorderitems.designid", string.Join(",", _Salesorder.design.Split(',').ToList().ConvertAll(e => "'" + WebAPI.Data.Utility.SafeSQL(e) + "'")));
                        break;
                }

                switch (_Salesorder.statusid)
                {
                    case SalesOrderSummaryCreteria.Status.Billed:
                        qb.AddParameter("salesorderitems.bqty", "pending", 0, ">");
                        break;
                    case SalesOrderSummaryCreteria.Status.Planned:
                        qb.AddParameter("salesorderitems.pqty", "pending", 0, ">");
                        break;
                    case SalesOrderSummaryCreteria.Status.Pending:

                        qb.AddParameter("oqty::numeric-(cqty::numeric+bqty+rqty)", "pending", 0, ">");
                        if (Services.ConfigManager.Instance(Context.OrganizationID).Config.intray_salesorder)
                            qb.AddParameter("salesorder.status", "sostatus", 1, "=");
                        else
                            qb.AddParameter("salesorder.status", "sostatus", 2, "<");
                        break;
                    case SalesOrderSummaryCreteria.Status.Cancelled:
                        qb.AddParameter("salesorderitems.cqty", "pending", 0, ">");
                        break;
                    case SalesOrderSummaryCreteria.Status.Removed:
                        qb.AddParameter("salesorderitems.rqty", "pending", 0, ">");
                        break;
                }

                command = qb.GetCommand(conn);
                var sqlcomd = command.CommandText += " " + SQL_SOSummary.Substring(SQL_SOSummary.IndexOf("group ")) + " order by salesorder.id";
                if (Services.ConfigManager.Instance(Context.OrganizationID).Config.analyser_stock_removeaqty)
                    sqlcomd = sqlcomd.Replace("salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty)", "salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty+salablegoods.aqty)");

                List<string> fields = new List<string>();
                List<string> stockfields = new List<string>();
                List<string> tables = new List<string>();
                if (Services.ConfigManager.Instance(Context.OrganizationID).Config.saleorder != null)
                {

                    if (Services.ConfigManager.Instance(Context.OrganizationID).Config.saleorder.brand)
                    {
                        stockfields.Add("items.brandid");
                        fields.Add("items.brandid,brand.name brandname");
                        tables.Add("left outer join brand on brand.id = items.brandid");
                    }
                    if (Services.ConfigManager.Instance(Context.OrganizationID).Config.saleorder.sizegroup)
                    {
                        stockfields.Add("lrinvoiceitems.itemdetail->>'sizegroup' sizegroup");
                        fields.Add("Salesorderitems.sizegroup");
                    }
                    if (Services.ConfigManager.Instance(Context.OrganizationID).Config.saleorder.size)
                    {
                        stockfields.Add("items.sizeid");
                        fields.Add("items.sizeid,size.name sizename");
                        tables.Add("left outer join referencelist size on size.id = items.sizeid");
                    }
                    if (Services.ConfigManager.Instance(Context.OrganizationID).Config.saleorder.colour)
                    {
                        stockfields.Add("items.colourid");
                        fields.Add("items.colourid,colour.name colourname");
                        tables.Add("left outer join referencelist colour on colour.id = items.colourid");
                    }
                    if (Services.ConfigManager.Instance(Context.OrganizationID).Config.saleorder.style)
                    {
                        stockfields.Add("items.styleid");
                        fields.Add("items.styleid,style.name stylename");
                        tables.Add("left outer join referencelist style on style.id = items.styleid");
                    }
                    if (Services.ConfigManager.Instance(Context.OrganizationID).Config.saleorder.pattern)
                    {
                        stockfields.Add("items.patternid");
                        fields.Add("items.patternid,pattern.name patternname");
                        tables.Add("left outer join referencelist pattern on pattern.id = items.patternid");
                    }
                    if (Services.ConfigManager.Instance(Context.OrganizationID).Config.saleorder.sleeve)
                    {
                        stockfields.Add("items.sleeveid");
                        fields.Add("items.sleeveid,sleeve.name sleevename");
                        tables.Add("left outer join referencelist sleeve on sleeve.id = items.sleeveid");
                    }
                    if (Services.ConfigManager.Instance(Context.OrganizationID).Config.saleorder.fit)
                    {
                        stockfields.Add("items.fitid");
                        fields.Add("items.fitid,fit.name fitname");
                        tables.Add("left outer join referencelist fit on fit.id = items.fitid");
                    }
                    if (Services.ConfigManager.Instance(Context.OrganizationID).Config.saleorder.design)
                    {
                        stockfields.Add("stock.designid");
                        fields.Add("Salesorderitems.designid");
                    }
                    command.CommandText = string.Format(sqlcomd, (fields.Count > 0 ? "," : "") + string.Join(",", fields), string.Join(" ", tables),
                         (fields.Count > 0 ? "," : "") + string.Join(",", fields.ConvertAll(e => e.Split(' ')[0])));
                }
                else
                {
                    command.CommandText = string.Format(sqlcomd, "", "", "");
                }
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        SalesOrderSummary oSalesorder = new SalesOrderSummary();
                        oSalesorder.customerid = reader["customerid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["customerid"]);
                        oSalesorder.soid = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);

                        oSalesorder.productid = reader["productid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productid"]);
                        oSalesorder.customername = reader["customername"] == DBNull.Value ? "" : reader["customername"].ToString();
                        if (reader["shiptoname"] != DBNull.Value)
                        {
                            oSalesorder.customername += "(" + reader["shiptoname"].ToString() + ")";
                        }
                        oSalesorder.cityname = reader["cityname"] == DBNull.Value ? "" : reader["cityname"].ToString();
                        oSalesorder.agentname = reader["agentname"] == DBNull.Value ? "" : reader["agentname"].ToString();
                        oSalesorder.ids = (reader["Salesorderitemsids"] == DBNull.Value ? "" : reader["Salesorderitemsids"].ToString());
                        oSalesorder.productname = reader["productname"] == DBNull.Value ? "" : reader["productname"].ToString();
                        oSalesorder.pendingqty = reader["pendingqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["pendingqty"]);
                        oSalesorder.plannedqty = reader["plannedqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["plannedqty"]);
                        oSalesorder.billedqty = reader["billedqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["billedqty"]);
                        oSalesorder.cancelledqty = reader["cancelledqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["cancelledqty"]);
                        oSalesorder.removedqty = reader["removedqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["removedqty"]);
                        oSalesorder.sodate = reader["sodate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["sodate"]);
                        oSalesorder.sono = reader["sono"] == DBNull.Value ? "" : Convert.ToString(reader["sono"]);

                        if (Services.ConfigManager.Instance(Context.OrganizationID).Config.saleorder != null &&
                            Services.ConfigManager.Instance(Context.OrganizationID).Config.saleorder.colour)
                        {
                            oSalesorder.colourid = reader["colourid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["colourid"]);
                            oSalesorder.colourname = reader["colourname"] == DBNull.Value ? "" : reader["colourname"].ToString();
                        }
                        if (Services.ConfigManager.Instance(Context.OrganizationID).Config.saleorder != null &&
                        Services.ConfigManager.Instance(Context.OrganizationID).Config.saleorder.brand)
                        {
                            oSalesorder.brandname = reader["brandname"] == DBNull.Value ? "" : reader["brandname"].ToString();
                            oSalesorder.brandid = reader["brandid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["brandid"]);
                        }
                        if (Services.ConfigManager.Instance(Context.OrganizationID).Config.saleorder != null &&
                        Services.ConfigManager.Instance(Context.OrganizationID).Config.saleorder.style)
                        {
                            oSalesorder.styleid = reader["styleid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["styleid"]);
                            oSalesorder.stylename = reader["stylename"] == DBNull.Value ? "" : reader["stylename"].ToString();
                        }
                        if (Services.ConfigManager.Instance(Context.OrganizationID).Config.saleorder != null &&
                        Services.ConfigManager.Instance(Context.OrganizationID).Config.saleorder.pattern)
                        {
                            oSalesorder.patternid = reader["patternid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["patternid"]);
                            oSalesorder.patternname = reader["patternname"] == DBNull.Value ? "" : reader["patternname"].ToString();
                        }
                        if (Services.ConfigManager.Instance(Context.OrganizationID).Config.saleorder != null &&
                        Services.ConfigManager.Instance(Context.OrganizationID).Config.saleorder.sleeve)
                        {
                            oSalesorder.sleeveid = reader["sleeveid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sleeveid"]);
                            oSalesorder.sleevename = reader["sleevename"] == DBNull.Value ? "" : reader["sleevename"].ToString();
                        }
                        if (Services.ConfigManager.Instance(Context.OrganizationID).Config.saleorder != null &&
                        Services.ConfigManager.Instance(Context.OrganizationID).Config.saleorder.fit)
                        {
                            oSalesorder.fitid = reader["fitid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["fitid"]);
                            oSalesorder.fitname = reader["fitname"] == DBNull.Value ? "" : reader["fitname"].ToString();
                        }
                        if (Services.ConfigManager.Instance(Context.OrganizationID).Config.saleorder != null &&
                        Services.ConfigManager.Instance(Context.OrganizationID).Config.saleorder.colour)
                        {
                            oSalesorder.designid = reader["designid"] == DBNull.Value ? "" : reader["designid"].ToString();
                        }
                        if (Services.ConfigManager.Instance(Context.OrganizationID).Config.saleorder != null &&
                        Services.ConfigManager.Instance(Context.OrganizationID).Config.saleorder.sizegroup)
                        {
                            oSalesorder.sizegroup = reader["sizegroup"] == DBNull.Value ? "" : reader["sizegroup"].ToString();
                        }
                        lst.Add(oSalesorder);
                    }
                }



                var currentstockqty = @"
select items.productid,sum(salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+ salablegoods.jqty)) stockqty
{0} from salablegoods
inner join stock on stock.id = salablegoods.stockid and stock.isactive = true
inner join items on items.id = stock.itemid
left outer join lrinvoiceitems on lrinvoiceitems.id = invoiceitemid
where salablegoods.hasstock = true and salablegoods.isactive = true {1}
group by items.productid
{0}";
                string locationcondition = "";
                if (_Salesorder.locationid > 0)
                {
                    locationcondition = " and salablegoods.locationid=@locationid ";
                }

                qb.CurrentQuery = string.Format(currentstockqty,
                    (stockfields.Count > 0 ? "," : "") + string.Join(",", stockfields),
                    locationcondition);
                //stock.designid,items.sizeid,items.colourid,items.brandid,items.styleid, lrinvoiceitems.itemdetail->>'sizegroup' sizegroup,
                //,stock.designid,items.sizeid,items.colourid,items.brandid,items.styleid,lrinvoiceitems.itemdetail->>'sizegroup'
                command = qb.GetCommand(conn);
                command.Parameters.AddWithValue("locationid", _Salesorder.locationid);
                if (Services.ConfigManager.Instance(Context.OrganizationID).Config.analyser_stock_removeaqty)
                    command.CommandText = command.CommandText.Replace("salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty)", "salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty+salablegoods.aqty)");

                List<SalesorderStock> salesorderStock = new List<SalesorderStock>();
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        SalesorderStock oSalesorder = new SalesorderStock();
                        oSalesorder.productid = reader["productid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productid"]);

                        if (Services.ConfigManager.Instance(Context.OrganizationID).Config.saleorder != null &&
                            Services.ConfigManager.Instance(Context.OrganizationID).Config.saleorder.size)
                            oSalesorder.sizeid = reader["sizeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sizeid"]);
                        if (Services.ConfigManager.Instance(Context.OrganizationID).Config.saleorder != null &&
                            Services.ConfigManager.Instance(Context.OrganizationID).Config.saleorder.colour)
                            oSalesorder.colourid = reader["colourid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["colourid"]);
                        if (Services.ConfigManager.Instance(Context.OrganizationID).Config.saleorder != null &&
                            Services.ConfigManager.Instance(Context.OrganizationID).Config.saleorder.style)
                            oSalesorder.styleid = reader["styleid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["styleid"]);
                        if (Services.ConfigManager.Instance(Context.OrganizationID).Config.saleorder != null &&
                            Services.ConfigManager.Instance(Context.OrganizationID).Config.saleorder.brand)
                            oSalesorder.brandid = reader["brandid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["brandid"]);
                        if (Services.ConfigManager.Instance(Context.OrganizationID).Config.saleorder != null &&
                            Services.ConfigManager.Instance(Context.OrganizationID).Config.saleorder.design)
                            oSalesorder.designid = reader["designid"] == DBNull.Value ? "" : reader["designid"].ToString();
                        if (Services.ConfigManager.Instance(Context.OrganizationID).Config.saleorder != null &&
                            Services.ConfigManager.Instance(Context.OrganizationID).Config.saleorder.sizegroup)
                            oSalesorder.sizegroup = reader["sizegroup"] == DBNull.Value ? "" : reader["sizegroup"].ToString();
                        else
                            oSalesorder.sizegroup = "";

                        if (Services.ConfigManager.Instance(Context.OrganizationID).Config.saleorder != null &&
                            Services.ConfigManager.Instance(Context.OrganizationID).Config.saleorder.sleeve)
                            oSalesorder.sleeveid = reader["sleeveid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sleeveid"]);
                        if (Services.ConfigManager.Instance(Context.OrganizationID).Config.saleorder != null &&
                            Services.ConfigManager.Instance(Context.OrganizationID).Config.saleorder.pattern)
                            oSalesorder.patternid = reader["patternid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["patternid"]);
                        if (Services.ConfigManager.Instance(Context.OrganizationID).Config.saleorder != null &&
                            Services.ConfigManager.Instance(Context.OrganizationID).Config.saleorder.fit)
                            oSalesorder.fitid = reader["fitid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["fitid"]);

                        oSalesorder.stockqty = reader["stockqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["stockqty"]);
                        salesorderStock.Add(oSalesorder);
                    }
                }

                List<SalesorderStock> pipelineStock = new List<SalesorderStock>();
                if (Services.ConfigManager.Instance(Context.OrganizationID).Config.system_ismfgapplication)
                {
                    command.CommandText = SQL_PipeLineStock;
                    if (Services.ConfigManager.Instance(Context.OrganizationID).Config.analyser_stock_removeaqty)
                        command.CommandText = command.CommandText.Replace("salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty)", "salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty+salablegoods.aqty)");

                    command.Parameters.Clear();
                    command.Parameters.AddWithValue("@wofromdate", DateTimeNow.AddMonths(-1).Date);
                    command.Parameters.AddWithValue("@wotodate", DateTimeNow.AddDays(7));

                    using (System.Data.IDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int stockcount = reader["sizecount"] == DBNull.Value ? 0 : Convert.ToInt32(reader["sizecount"]);
                            SalesorderStock oSalesorder = new SalesorderStock();
                            oSalesorder.productid = reader["productid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productid"]);
                            oSalesorder.sizegroup = reader["size"] == DBNull.Value ? "" : reader["size"].ToString();
                            oSalesorder.colourid = reader["colourid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["colourid"]);
                            oSalesorder.designid = reader["designname"] == DBNull.Value ? "" : reader["designname"].ToString();
                            oSalesorder.stockqty = reader["stockqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["stockqty"]);
                            if (stockcount > 0)
                                oSalesorder.stockqty /= stockcount;
                            pipelineStock.Add(oSalesorder);
                        }
                    }
                }

                conn.Close();

                var groupedstocks = salesorderStock.GroupBy(e => new { e.productid, e.designid }).ToList();

                ReferencelistDataService referencelistDataService = new ReferencelistDataService() { Context = this.Context };
                var sizegroups = referencelistDataService.SearchSizeGroups();
                var plangroups = lst.GroupBy(e => new { e.productid, e.designid, e.colourid, e.sizegroup, e.patternid, e.fitid, e.sleeveid, e.brandid, e.styleid }).ToList();
                ReferenceSizeGroup sizegroup = null;
                foreach (var item in lst)
                {
                    if (Services.ConfigManager.Instance(Context.OrganizationID).Config.saleorder != null &&
                            Services.ConfigManager.Instance(Context.OrganizationID).Config.saleorder.sizegroup)
                    {
                        if (!sizegroups.Exists(e => e.name == item.sizegroup))
                        {
                            continue;
                        }
                        sizegroup = sizegroups.Find(e => e.name == item.sizegroup);
                        item.sizeset = sizegroup.list.Count;
                    }

                    if (!groupedstocks.Exists(e => e.Key.productid == item.productid &&
                    e.Key.designid == item.designid)) continue;

                    var productorders = groupedstocks.Find(e => e.Key.productid == item.productid &&
                    e.Key.designid == item.designid).ToList();

                    var allsizestock = productorders.FindAll(e =>
                    {
                        return (e.brandid == item.brandid || item.brandid == 0) &&
                        (e.sizegroup == item.sizegroup || string.IsNullOrEmpty(item.sizegroup)) &&
                        (e.sizeid == item.sizeid || item.sizeid == 0) &&
                        (e.sleeveid == item.sleeveid || item.sleeveid == 0) &&
                        (e.styleid == item.styleid || item.styleid == 0) &&
                        (e.fitid == item.fitid || item.fitid == 0) &&
                        (e.patternid == item.patternid || item.patternid == 0) &&
                        (e.colourid == item.colourid || item.colourid == 0);
                    });

                    if (allsizestock.Count > 0)
                    {
                        if (sizegroup != null)
                        {
                            if (allsizestock.All(e => e.stockqty == allsizestock[0].stockqty))
                            {
                                item.availableqty = Math.Floor(allsizestock.Sum(e => e.stockqty) / item.sizeset);
                            }
                            else
                            {
                                var minstocksqty = allsizestock.Min(e => e.stockqty);
                                item.availableqty = allsizestock.Count * minstocksqty / item.sizeset;
                                item.partialstock = true;
                            }
                        }
                        else
                        {
                            item.availableqty = allsizestock.Sum(e => e.stockqty);
                        }
                    }


                    item.allocatedqty = lst.FindAll(e =>
                    (e.designid == item.designid || string.IsNullOrEmpty(item.designid)) &&
                    (e.brandid == item.brandid || item.brandid == 0) &&
                        (e.sizegroup == item.sizegroup || string.IsNullOrEmpty(item.sizegroup)) &&
                        (e.sizeid == item.sizeid || item.sizeid == 0) &&
                        (e.sleeveid == item.sleeveid || item.sleeveid == 0) &&
                        (e.styleid == item.styleid || item.styleid == 0) &&
                        (e.fitid == item.fitid || item.fitid == 0) &&
                        (e.patternid == item.patternid || item.patternid == 0) &&
                        (e.colourid == item.colourid || item.colourid == 0)).Sum(e => e.plannedqty);

                    if (sizegroup != null && sizegroup.list.Count > 0)
                    {
                        if (item.pendingqty > 0)
                            item.pendingqty /= sizegroup.list.Count;
                        if (item.plannedqty > 0)
                            item.plannedqty /= sizegroup.list.Count;
                        if (item.allocatedqty > 0)
                            item.allocatedqty /= sizegroup.list.Count;
                        if (item.billedqty > 0)
                            item.billedqty /= sizegroup.list.Count;
                        if (item.cancelledqty > 0)
                            item.cancelledqty /= sizegroup.list.Count;
                        if (item.removedqty > 0)
                            item.removedqty /= sizegroup.list.Count;
                    }

                    if (Services.ConfigManager.Instance(Context.OrganizationID).Config.system_ismfgapplication)
                    {
                        if (pipelineStock.Exists(e => e.productid == item.productid &&
                     e.colourid == item.colourid &&
                     e.designid == item.designid))
                        {
                            item.pipelinestock = pipelineStock.Sum(e => e.productid == item.productid &&
                                                    e.colourid == item.colourid &&
                                                    e.designid == item.designid ? e.stockqty : 0);
                        }
                    }
                }

            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _Salesorder);
            }
            return lst;
        }

        public class BookingRequest : BaseEntity
        {
            public class item
            {
                public string uid { get; set; }
                public decimal qty { get; set; }
            }
            public List<item> items { get; set; }
            public long locationid { get; set; }
        }
        public class BookingResponse
        {
            public string uid { get; set; }
            public decimal qty { get; set; }
            public decimal sqty { get; set; }
            public decimal mrp { get; set; }
            public string design { get; set; }
            public long sizeid { get; set; }
            public long colourid { get; set; }
            public long itemid { get; set; }
            public string printingname { get; set; }
        }
        public List<BookingResponse> BookSO(BookingRequest request)
        {
            string query = @"with alluid
as
(
    select mfgdesign.code,schemelist->>'sizegroup' sizegroup,schemelist->>'uid' uid,
    schemelist->>'pname' pname,
    cast(schemelist->>'sizeid' as bigint) sizeid,
	cast(schemelist->>'itemid' as bigint) itemid,
    cast(schemelist->>'wsrate' as numeric) wsrate,cast(schemelist->>'dealerprice' as numeric) dealerprice,
    cast(schemelist->>'salerate' as numeric) salerate,cast(schemelist->>'mrp' as numeric) mrp,
    schemelist->>'code' dname,cast(schemelist->>'colourid' as bigint) colourid,
    case when position('/' in schemelist->>'code') = 0 then '' else
    substring(schemelist->>'code',position('/' in schemelist->>'code')+1)
    end colourcode
    from mfgdesign
    LEFT JOIN LATERAL
    json_array_elements (cast(mfgdesign.sizegroup->>'items' as json)) schemelist ON true
    where mfgdesign.isactive=true
)
select uid,mrp,itemid,colourid,code,pname,sizeid from alluid
where uid in ({0})";
            query = string.Format(query, string.Join(",", request.items.ConvertAll(y => "'" + y.uid + "'")));
            var response = new List<BookingResponse>();
            using (var reader = ExecuteReader(query, new SortedList<string, object>()))
            {
                while (reader.Read())
                {
                    BookingResponse bookingResponse = new BookingResponse()
                    {
                        uid = reader["uid"].ToString(),
                        printingname = reader["pname"].ToString(),
                        mrp = Convert.ToDecimal(reader["mrp"].ToString()),
                        itemid = Convert.ToInt64(reader["itemid"].ToString()),
                        sizeid = Convert.ToInt64(reader["sizeid"].ToString()),
                        colourid = Convert.ToInt64(reader["colourid"].ToString()),
                        design = reader["code"].ToString()
                    };
                    bookingResponse.qty = request.items.Find(x => x.uid == bookingResponse.uid).qty;
                    response.Add(bookingResponse);
                }
            }
            return response;
        }
        public List<SalesOrderUIDSummary> SearchUIDSummary(SalesOrderSummaryCreteria _Salesorder)
        {
            List<SalesOrderUIDSummary> lst = new List<SalesOrderUIDSummary>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                NpgsqlCommand command = null;
                QueryBuilder qb = new QueryBuilder();
                qb.CurrentQuery = SQL_SOUISummary.Substring(0, SQL_SOUISummary.IndexOf("group "));

                if (_Salesorder.fromdate.Year > 2000 && _Salesorder.todate.Year > 2000)
                {
                    qb.AddParameter("sodate", "sodatefrom", _Salesorder.fromdate.Date, ">=");
                    qb.AddParameter("sodate", "sodatetodate", _Salesorder.todate.Date.AddDays(1), "<");
                }

                switch (_Salesorder.searchtype)
                {
                    case 1:
                        if (_Salesorder.customerid > 0)
                            qb.AddParameter("salesorder.customerid", "customerid", _Salesorder.customerid, "=");
                        break;
                    case 2:
                        if (_Salesorder.agentid > 0)
                            qb.AddParameter("salesorder.agentid", "agentid", _Salesorder.agentid, "=");
                        break;
                    case 3:
                        if (_Salesorder.cityid > 0)
                            qb.AddParameter("customer.cityid", "cityid", _Salesorder.cityid, "=");
                        break;
                    case 4:
                        if (_Salesorder.productid > 0)
                            qb.AddParameter("salesorderitems.productid", "productid", _Salesorder.productid, "=");
                        break;
                    case 5:
                        if (!string.IsNullOrEmpty(_Salesorder.design))
                            qb.AddInParamer("salesorderitems.designid", string.Join(",", _Salesorder.design.Split(',').ToList().ConvertAll(e => "'" + WebAPI.Data.Utility.SafeSQL(e) + "'")));
                        break;
                }

                switch (_Salesorder.statusid)
                {
                    case SalesOrderSummaryCreteria.Status.Billed:
                        qb.AddParameter("salesorderitems.bqty", "pending", 0, ">");
                        break;
                    case SalesOrderSummaryCreteria.Status.Planned:
                        qb.AddParameter("salesorderitems.pqty", "pending", 0, ">");
                        break;
                    case SalesOrderSummaryCreteria.Status.Pending:
                        qb.AddParameter("oqty::numeric-(cqty::numeric+bqty+rqty)", "pending", 0, ">");
                        if (Services.ConfigManager.Instance(Context.OrganizationID).Config.intray_salesorder)
                            qb.AddParameter("salesorder.status", "sostatus", 1, "=");
                        else
                            qb.AddParameter("salesorder.status", "sostatus", 2, "<");
                        break;
                    case SalesOrderSummaryCreteria.Status.Cancelled:
                        qb.AddParameter("salesorderitems.cqty", "pending", 0, ">");
                        break;
                    case SalesOrderSummaryCreteria.Status.Removed:
                        qb.AddParameter("salesorderitems.rqty", "pending", 0, ">");
                        break;
                }

                command = qb.GetCommand(conn);
                command.CommandTimeout = 50000;
                var sqlcomd = command.CommandText += " " + SQL_SOUISummary.Substring(SQL_SOUISummary.IndexOf("group ")) + " order by salesorder.id";
                if (Services.ConfigManager.Instance(Context.OrganizationID).Config.analyser_stock_removeaqty)
                    sqlcomd = sqlcomd.Replace("salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty)", "salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty+salablegoods.aqty)");

                List<string> fields = new List<string>();
                List<string> stockfields = new List<string>();
                List<string> tables = new List<string>();
                if (Services.ConfigManager.Instance(Context.OrganizationID).Config.saleorder != null)
                {

                    if (Services.ConfigManager.Instance(Context.OrganizationID).Config.saleorder.brand)
                    {
                        stockfields.Add("items.brandid");
                        fields.Add("items.brandid,brand.name brandname");
                        tables.Add("left outer join brand on brand.id = items.brandid");
                    }
                    if (Services.ConfigManager.Instance(Context.OrganizationID).Config.saleorder.sizegroup)
                    {
                        stockfields.Add("lrinvoiceitems.itemdetail->>'sizegroup' sizegroup");
                        fields.Add("Salesorderitems.sizegroup");
                    }
                    //if (Services.ConfigManager.Instance(Context.OrganizationID).Config.saleorder.size)
                    //{
                    //    stockfields.Add("items.sizeid");
                    //    fields.Add("items.sizeid,size.name sizename");
                    //    tables.Add("left outer join referencelist size on size.id = items.sizeid");
                    //}
                    if (Services.ConfigManager.Instance(Context.OrganizationID).Config.saleorder.colour)
                    {
                        stockfields.Add("items.colourid");
                        fields.Add("items.colourid,colour.name colourname");
                        tables.Add("left outer join referencelist colour on colour.id = items.colourid");
                    }
                    if (Services.ConfigManager.Instance(Context.OrganizationID).Config.saleorder.style)
                    {
                        stockfields.Add("items.styleid");
                        fields.Add("items.styleid,style.name stylename");
                        tables.Add("left outer join referencelist style on style.id = items.styleid");
                    }
                    if (Services.ConfigManager.Instance(Context.OrganizationID).Config.saleorder.pattern)
                    {
                        stockfields.Add("items.patternid");
                        fields.Add("items.patternid,pattern.name patternname");
                        tables.Add("left outer join referencelist pattern on pattern.id = items.patternid");
                    }
                    if (Services.ConfigManager.Instance(Context.OrganizationID).Config.saleorder.sleeve)
                    {
                        stockfields.Add("items.sleeveid");
                        fields.Add("items.sleeveid,sleeve.name sleevename");
                        tables.Add("left outer join referencelist sleeve on sleeve.id = items.sleeveid");
                    }
                    if (Services.ConfigManager.Instance(Context.OrganizationID).Config.saleorder.fit)
                    {
                        stockfields.Add("items.fitid");
                        fields.Add("items.fitid,fit.name fitname");
                        tables.Add("left outer join referencelist fit on fit.id = items.fitid");
                    }
                    if (Services.ConfigManager.Instance(Context.OrganizationID).Config.saleorder.design)
                    {
                        stockfields.Add("stock.designid");
                        fields.Add("Salesorderitems.designid");
                    }
                    command.CommandText = string.Format(sqlcomd, (fields.Count > 0 ? "," : "") + string.Join(",", fields), string.Join(" ", tables),
                         (fields.Count > 0 ? "," : "") + string.Join(",", fields.ConvertAll(e => e.Split(' ')[0])));
                }
                else
                {
                    command.CommandText = string.Format(sqlcomd, "", "", "");
                }
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        SalesOrderUIDSummary oSalesorder = new SalesOrderUIDSummary();
                        oSalesorder.customerid = reader["customerid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["customerid"]);
                        oSalesorder.soid = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        oSalesorder.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        if (reader["scityid"] != DBNull.Value && Convert.ToInt64(reader["scityid"]) > 0)
                        {
                            oSalesorder.cityid = reader["scityid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["scityid"]);
                        }
                        else
                        {
                            oSalesorder.cityid = reader["cityid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["cityid"]);
                        }
                        oSalesorder.shipto = reader["shiptocustomerid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["shiptocustomerid"]);
                        oSalesorder.productid = reader["productid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productid"]);
                        oSalesorder.ids = (reader["Salesorderitemsids"] == DBNull.Value ? "" : reader["Salesorderitemsids"].ToString());
                        oSalesorder.szlist = (reader["oqty"] == DBNull.Value ? "" : reader["oqty"].ToString());
                        oSalesorder.pendingqty = reader["pendingqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["pendingqty"]);
                        oSalesorder.orderqty = reader["orderqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["orderqty"]);
                        oSalesorder.plannedqty = reader["plannedqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["plannedqty"]);
                        oSalesorder.billedqty = reader["billedqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["billedqty"]);
                        oSalesorder.cancelledqty = reader["cancelledqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["cancelledqty"]);
                        oSalesorder.removedqty = reader["removedqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["removedqty"]);
                        oSalesorder.sodate = reader["sodate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["sodate"]);
                        oSalesorder.sono = reader["sono"] == DBNull.Value ? "" : Convert.ToString(reader["sono"]);
                        oSalesorder.remarks = reader["remarks"] == DBNull.Value ? "" : Convert.ToString(reader["remarks"]);
                        oSalesorder.sobookno = reader["sobookno"] == DBNull.Value ? "" : Convert.ToString(reader["sobookno"]);
                        oSalesorder.customerpo = reader["customerpo"] == DBNull.Value ? "" : Convert.ToString(reader["customerpo"]);
                        oSalesorder.sobookqty = reader["sobookqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["sobookqty"]);
                        oSalesorder.designitemcode = reader["designitemcode"] == DBNull.Value ? "" : Convert.ToString(reader["designitemcode"]);
                        oSalesorder.agentid = reader["agentid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["agentid"]);
                        oSalesorder.salesmenid = reader["salesmenid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["salesmenid"]);
                        lst.Add(oSalesorder);
                    }
                }

                foreach (var sg in lst.GroupBy(x => x.soid))
                {
                    if (sg.Any(x => x.billedqty > 0))
                    {
                        if (sg.Any(x => x.pendingqty > 0))
                            sg.ToList().ForEach(x => x.sostatus = "P");
                        else
                            sg.ToList().ForEach(x => x.sostatus = "C");
                    }
                    else
                    {
                        sg.ToList().ForEach(x => x.sostatus = "N");
                    }
                }




                if (_Salesorder.statusid == SalesOrderSummaryCreteria.Status.Planning &&
                    lst.Count > 0)
                {
                    lst.RemoveAll(x => x.pendingqty <= 0);

                    var uniquedesignlist = lst.ConvertAll(x =>
                    {
                        return "'" + (x.designitemcode.IndexOf('/') > 0 ? x.designitemcode.Substring(0, x.designitemcode.IndexOf('/')) : x.designitemcode) + "'";
                    }).Distinct().ToList();

                    var currentstockqty = @"
with alluid
as
(
    select mfgdesign.code,schemelist->>'sizegroup' sizegroup,schemelist->>'uid' uid,
    cast(schemelist->>'sizeid' as bigint) sizeid,
    cast(schemelist->>'wsrate' as numeric) wsrate,cast(schemelist->>'dealerprice' as numeric) dealerprice,
    cast(schemelist->>'salerate' as numeric) salerate,cast(schemelist->>'mrp' as numeric) mrp,
    schemelist->>'code' dname,cast(schemelist->>'colourid' as bigint) colourid,
    case when position('/' in schemelist->>'code') = 0 then '' else
    substring(schemelist->>'code',position('/' in schemelist->>'code')+1)
    end colourcode
    from mfgdesign
    LEFT JOIN LATERAL
    json_array_elements (cast(mfgdesign.sizegroup->>'items' as json)) schemelist ON true
    where mfgdesign.code in ({0}) and mfgdesign.isactive=true
), plannedorders as
(
    select sizeid,designitemcode,sum(pqty) plannedqty from salesorderitems
    inner join salesorder on salesorder.id = salesorderitems.salesorderid
	where salesorderitems.isactive = true and salesorderitems.pqty>0 and 
    salesorder.isactive=true and salesorder.status <=1
    group by sizeid,designitemcode
)

select alluid.sizeid,dname,
sum(salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+ salablegoods.jqty)) stockqty,
sum(plannedqty) plannedqty
from salablegoods
inner join stock on stock.id = salablegoods.stockid and stock.isactive = true
inner join alluid on alluid.uid = stock.serialno
left outer join plannedorders on plannedorders.designitemcode = dname and plannedorders.sizeid = alluid.sizeid
where salablegoods.hasstock = true and salablegoods.isactive = true and designid in ({0}) {1}
group by serialno,alluid.sizeid,dname";
                    string locationcondition = "";
                    if (_Salesorder.locationid > 0)
                    {
                        locationcondition = " and salablegoods.locationid=@locationid ";
                    }
                    qb.CurrentQuery = UpdateQueryForHyper(string.Format(currentstockqty,
                        string.Join(",", uniquedesignlist),
                        locationcondition));
                    command = qb.GetCommand(conn);
                    command.Parameters.AddWithValue("locationid", _Salesorder.locationid);
                    if (Services.ConfigManager.Instance(Context.OrganizationID).Config.analyser_stock_removeaqty)
                        command.CommandText = command.CommandText.Replace("salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty)", "salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty+salablegoods.aqty)");

                    List<SalesorderStock> salesorderStock = new List<SalesorderStock>();
                    using (System.Data.IDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            SalesorderStock oSalesorder = new SalesorderStock();
                            oSalesorder.sizeid = reader["sizeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sizeid"]);
                            oSalesorder.designitemcode = reader["dname"] == DBNull.Value ? "" : Convert.ToString(reader["dname"]);
                            oSalesorder.stockqty = reader["stockqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["stockqty"]);
                            oSalesorder.plannedqty = reader["plannedqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["plannedqty"]);
                            salesorderStock.Add(oSalesorder);
                        }
                    }
                    foreach (var sitem in salesorderStock.GroupBy(x => x.designitemcode))
                    {
                        string st = string.Join(",", sitem.ToList().ConvertAll(x => x.sizeid + "-" + x.stockqty.ToString("N0") + "-" + x.plannedqty.ToString("N0")));
                        lst.FindAll(x => x.designitemcode == sitem.Key).ForEach(x => x.stlist = st);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _Salesorder);
            }
            return lst;
        }

        public List<SalesOrderUIDSummary> SearchSizeSummaryNew(SalesOrderSummaryCreteria _Salesorder)
        {
            List<SalesOrderUIDSummary> lst = new List<SalesOrderUIDSummary>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                NpgsqlCommand command = null;
                QueryBuilder qb = new QueryBuilder();
                qb.CurrentQuery = SQL_SOSizeSummary.Substring(0, SQL_SOSizeSummary.IndexOf("group "));

                if (_Salesorder.fromdate.Year > 2000 && _Salesorder.todate.Year > 2000)
                {
                    qb.AddParameter("sodate", "sodatefrom", _Salesorder.fromdate.Date, ">=");
                    qb.AddParameter("sodate", "sodatetodate", _Salesorder.todate.Date.AddDays(1), "<");
                }

                switch (_Salesorder.searchtype)
                {
                    case 1:
                        if (_Salesorder.customerid > 0)
                            qb.AddParameter("salesorder.customerid", "customerid", _Salesorder.customerid, "=");
                        break;
                    case 2:
                        if (_Salesorder.agentid > 0)
                            qb.AddParameter("salesorder.agentid", "agentid", _Salesorder.agentid, "=");
                        break;
                    case 3:
                        if (_Salesorder.cityid > 0)
                            qb.AddParameter("customer.cityid", "cityid", _Salesorder.cityid, "=");
                        break;
                    case 4:
                        if (_Salesorder.productid > 0)
                            qb.AddParameter("salesorderitems.productid", "productid", _Salesorder.productid, "=");
                        break;
                    case 5:
                        if (!string.IsNullOrEmpty(_Salesorder.design))
                            qb.AddInParamer("salesorderitems.designid", string.Join(",", _Salesorder.design.Split(',').ToList().ConvertAll(e => "'" + WebAPI.Data.Utility.SafeSQL(e) + "'")));
                        break;
                }

                switch (_Salesorder.statusid)
                {
                    case SalesOrderSummaryCreteria.Status.Billed:
                        qb.AddParameter("salesorderitems.bqty", "pending", 0, ">");
                        break;
                    case SalesOrderSummaryCreteria.Status.Planned:
                        qb.AddParameter("salesorderitems.pqty", "pending", 0, ">");
                        break;
                    case SalesOrderSummaryCreteria.Status.Pending:
                        qb.AddParameter("oqty::numeric-(cqty::numeric+bqty+rqty)", "pending", 0, ">");
                        if (Services.ConfigManager.Instance(Context.OrganizationID).Config.intray_salesorder)
                            qb.AddParameter("salesorder.status", "sostatus", 1, "=");
                        else
                            qb.AddParameter("salesorder.status", "sostatus", 2, "<");
                        break;
                    case SalesOrderSummaryCreteria.Status.Cancelled:
                        qb.AddParameter("salesorderitems.cqty", "pending", 0, ">");
                        break;
                    case SalesOrderSummaryCreteria.Status.Removed:
                        qb.AddParameter("salesorderitems.rqty", "pending", 0, ">");
                        break;
                }

                command = qb.GetCommand(conn);
                command.CommandTimeout = 50000;
                var sqlcomd = command.CommandText;
                if (Services.ConfigManager.Instance(Context.OrganizationID).Config.analyser_stock_removeaqty)
                    sqlcomd = sqlcomd.Replace("salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty)", "salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty+salablegoods.aqty)");

                List<string> fields = new List<string>();
                List<string> stockfields = new List<string>();
                List<string> tables = new List<string>();
                if (Services.ConfigManager.Instance(Context.OrganizationID).Config.saleorder != null)
                {

                    if (Services.ConfigManager.Instance(Context.OrganizationID).Config.saleorder.brand)
                    {
                        stockfields.Add("items.brandid");
                        fields.Add("items.brandid,brand.name brandname");
                        tables.Add("left outer join brand on brand.id = items.brandid");
                    }
                    //if (Services.ConfigManager.Instance(Context.OrganizationID).Config.saleorder.sizegroup)
                    //{
                    //    stockfields.Add("lrinvoiceitems.itemdetail->>'sizegroup' sizegroup");
                    //    fields.Add("Salesorderitems.sizegroup");
                    //}
                    //if (Services.ConfigManager.Instance(Context.OrganizationID).Config.saleorder.size)
                    //{
                    //    stockfields.Add("items.sizeid");
                    //    fields.Add("items.sizeid,size.name sizename");
                    //    tables.Add("left outer join referencelist size on size.id = items.sizeid");
                    //}
                    if (Services.ConfigManager.Instance(Context.OrganizationID).Config.saleorder.colour)
                    {
                        stockfields.Add("items.colourid");
                        fields.Add("items.colourid,colour.name colourname");
                        tables.Add("left outer join referencelist colour on colour.id = items.colourid");
                    }
                    if (Services.ConfigManager.Instance(Context.OrganizationID).Config.saleorder.style)
                    {
                        stockfields.Add("items.styleid");
                        fields.Add("items.styleid,style.name stylename");
                        tables.Add("left outer join referencelist style on style.id = items.styleid");
                    }
                    if (Services.ConfigManager.Instance(Context.OrganizationID).Config.saleorder.pattern)
                    {
                        stockfields.Add("items.patternid");
                        fields.Add("items.patternid,pattern.name patternname");
                        tables.Add("left outer join referencelist pattern on pattern.id = items.patternid");
                    }
                    if (Services.ConfigManager.Instance(Context.OrganizationID).Config.saleorder.sleeve)
                    {
                        stockfields.Add("items.sleeveid");
                        fields.Add("items.sleeveid,sleeve.name sleevename");
                        tables.Add("left outer join referencelist sleeve on sleeve.id = items.sleeveid");
                    }
                    if (Services.ConfigManager.Instance(Context.OrganizationID).Config.saleorder.fit)
                    {
                        stockfields.Add("items.fitid");
                        fields.Add("items.fitid,fit.name fitname");
                        tables.Add("left outer join referencelist fit on fit.id = items.fitid");
                    }
                    if (Services.ConfigManager.Instance(Context.OrganizationID).Config.saleorder.design)
                    {
                        stockfields.Add("stock.designid");
                        fields.Add("Salesorderitems.designid");
                    }
                    command.CommandText = string.Format(sqlcomd, (fields.Count > 0 ? "," : "") + string.Join(",", fields), string.Join(" ", tables),
                         (fields.Count > 0 ? "," : "") + string.Join(",", fields.ConvertAll(e => e.Split(' ')[0])));
                }
                else
                {
                    command.CommandText = string.Format(sqlcomd, "", "", "");
                }
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        SalesOrderUIDSummary oSalesorder = new SalesOrderUIDSummary();
                        oSalesorder.customerid = reader["customerid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["customerid"]);
                        oSalesorder.soid = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        oSalesorder.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        if (reader["scityid"] != DBNull.Value && Convert.ToInt64(reader["scityid"]) > 0)
                        {
                            oSalesorder.cityid = reader["scityid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["scityid"]);
                        }
                        else
                        {
                            oSalesorder.cityid = reader["cityid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["cityid"]);
                        }
                        oSalesorder.shipto = reader["shiptocustomerid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["shiptocustomerid"]);
                        oSalesorder.productid = reader["productid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productid"]);
                        oSalesorder.ids = (reader["Salesorderitemsids"] == DBNull.Value ? "" : reader["Salesorderitemsids"].ToString());
                        //oSalesorder.soitemid = (reader["Salesorderitemsids"] == DBNull.Value ? 0 :Convert.ToInt64(reader["Salesorderitemsids"]);
                        //oSalesorder.sizeid = (reader["sizeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sizeid"]);
                        oSalesorder.pendingqty = reader["pendingqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["pendingqty"]);
                        oSalesorder.orderqty = reader["orderqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["orderqty"]);
                        oSalesorder.plannedqty = reader["plannedqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["plannedqty"]);
                        oSalesorder.billedqty = reader["billedqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["billedqty"]);
                        oSalesorder.cancelledqty = reader["cancelledqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["cancelledqty"]);
                        oSalesorder.removedqty = reader["removedqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["removedqty"]);
                        oSalesorder.sodate = reader["sodate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["sodate"]);
                        oSalesorder.sono = reader["sono"] == DBNull.Value ? "" : Convert.ToString(reader["sono"]);
                        oSalesorder.remarks = reader["remarks"] == DBNull.Value ? "" : Convert.ToString(reader["remarks"]);
                        oSalesorder.sobookno = reader["sobookno"] == DBNull.Value ? "" : Convert.ToString(reader["sobookno"]);
                        oSalesorder.customerpo = reader["customerpo"] == DBNull.Value ? "" : Convert.ToString(reader["customerpo"]);
                        oSalesorder.sobookqty = reader["sobookqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["sobookqty"]);
                        oSalesorder.designitemcode = reader["designitemcode"] == DBNull.Value ? "" : Convert.ToString(reader["designitemcode"]);
                        oSalesorder.agentid = reader["agentid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["agentid"]);
                        oSalesorder.salesmenid = reader["salesmenid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["salesmenid"]);
                        lst.Add(oSalesorder);
                    }
                }

                foreach (var sg in lst.GroupBy(x => x.soid))
                {
                    if (sg.Any(x => x.billedqty > 0))
                    {
                        if (sg.Any(x => x.pendingqty > 0))
                            sg.ToList().ForEach(x => x.sostatus = "P");
                        else
                            sg.ToList().ForEach(x => x.sostatus = "C");
                    }
                    else
                    {
                        sg.ToList().ForEach(x => x.sostatus = "N");
                    }
                }




                if (_Salesorder.statusid == SalesOrderSummaryCreteria.Status.Planning &&
                    lst.Count > 0)
                {
                    lst.RemoveAll(x => x.pendingqty <= 0);

                    var uniquedesignlist = lst.ConvertAll(x =>
                    {
                        return "'" + (x.designitemcode.IndexOf('/') > 0 ? x.designitemcode.Substring(0, x.designitemcode.IndexOf('/')) : x.designitemcode) + "'";
                    }).Distinct().ToList();

                    var currentstockqty = @"
with alluid
as
(
    select mfgdesign.code,schemelist->>'sizegroup' sizegroup,schemelist->>'uid' uid,
    cast(schemelist->>'sizeid' as bigint) sizeid,
    cast(schemelist->>'wsrate' as numeric) wsrate,cast(schemelist->>'dealerprice' as numeric) dealerprice,
    cast(schemelist->>'salerate' as numeric) salerate,cast(schemelist->>'mrp' as numeric) mrp,
    schemelist->>'code' dname,cast(schemelist->>'colourid' as bigint) colourid,
    case when position('/' in schemelist->>'code') = 0 then '' else
    substring(schemelist->>'code',position('/' in schemelist->>'code')+1)
    end colourcode
    from mfgdesign
    LEFT JOIN LATERAL
    json_array_elements (cast(mfgdesign.sizegroup->>'items' as json)) schemelist ON true
    where mfgdesign.code in ({0}) and mfgdesign.isactive=true
), plannedorders as
(
    select sizeid,designitemcode,sum(pqty) plannedqty from salesorderitems
    inner join salesorder on salesorder.id = salesorderitems.salesorderid
	where salesorderitems.isactive = true and salesorderitems.pqty>0 and 
    salesorder.isactive=true and salesorder.status <=1
    group by sizeid,designitemcode
)

select alluid.sizeid,dname,
sum(salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+ salablegoods.jqty)) stockqty,
sum(plannedqty) plannedqty
from salablegoods
inner join stock on stock.id = salablegoods.stockid and stock.isactive = true
inner join alluid on alluid.uid = stock.serialno
left outer join plannedorders on plannedorders.designitemcode = dname and plannedorders.sizeid = alluid.sizeid
where salablegoods.hasstock = true and salablegoods.isactive = true and designid in ({0}) {1}
group by serialno,alluid.sizeid,dname";
                    string locationcondition = "";
                    if (_Salesorder.locationid > 0)
                    {
                        locationcondition = " and salablegoods.locationid=@locationid ";
                    }
                    qb.CurrentQuery = UpdateQueryForHyper(string.Format(currentstockqty,
                        string.Join(",", uniquedesignlist),
                        locationcondition));
                    command = qb.GetCommand(conn);
                    command.Parameters.AddWithValue("locationid", _Salesorder.locationid);
                    if (Services.ConfigManager.Instance(Context.OrganizationID).Config.analyser_stock_removeaqty)
                        command.CommandText = command.CommandText.Replace("salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty)", "salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty+salablegoods.aqty)");

                    List<SalesorderStock> salesorderStock = new List<SalesorderStock>();
                    using (System.Data.IDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            SalesorderStock oSalesorder = new SalesorderStock();
                            oSalesorder.sizeid = reader["sizeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sizeid"]);
                            oSalesorder.designitemcode = reader["dname"] == DBNull.Value ? "" : Convert.ToString(reader["dname"]);
                            oSalesorder.stockqty = reader["stockqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["stockqty"]);
                            oSalesorder.plannedqty = reader["plannedqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["plannedqty"]);
                            salesorderStock.Add(oSalesorder);
                        }
                    }
                    foreach (var sitem in salesorderStock.GroupBy(x => x.designitemcode))
                    {
                        string st = string.Join(",", sitem.ToList().ConvertAll(x => x.sizeid + "-" + x.stockqty.ToString("N0") + "-" + x.plannedqty.ToString("N0")));
                        lst.FindAll(x => x.designitemcode == sitem.Key).ForEach(x => x.stlist = st);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _Salesorder);
            }
            return lst;
        }

        public SalesOrderSizeSummary SearchSizeSummary(SalesOrderSummaryCreteria _Salesorder)
        {

            List<SalesOrderSizeSummary.SalesOrderSizeRow> lst = new List<SalesOrderSizeSummary.SalesOrderSizeRow>();
            var response = new SalesOrderSizeSummary() { items = lst };
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                NpgsqlCommand command = null;
                QueryBuilder qb = new QueryBuilder();
                qb.CurrentQuery = SQL_SOSizeSummary;

                if (_Salesorder.fromdate.Year > 2000 && _Salesorder.todate.Year > 2000)
                {
                    qb.AddParameter("sodate", "sodatefrom", _Salesorder.fromdate.Date, ">=");
                    qb.AddParameter("sodate", "sodatetodate", _Salesorder.todate.Date.AddDays(1), "<");
                }

                switch (_Salesorder.searchtype)
                {
                    case 1:
                        if (_Salesorder.customerid > 0)
                            qb.AddParameter("salesorder.customerid", "customerid", _Salesorder.customerid, "=");
                        break;
                    case 2:
                        if (_Salesorder.agentid > 0)
                            qb.AddParameter("salesorder.agentid", "agentid", _Salesorder.agentid, "=");
                        break;
                    case 3:
                        if (_Salesorder.cityid > 0)
                            qb.AddParameter("customer.cityid", "cityid", _Salesorder.cityid, "=");
                        break;
                    case 4:
                        if (_Salesorder.productid > 0)
                            qb.AddParameter("salesorderitems.productid", "productid", _Salesorder.productid, "=");
                        break;
                    case 5:
                        if (!string.IsNullOrEmpty(_Salesorder.design))
                            qb.AddInParamer("salesorderitems.designid", string.Join(",", _Salesorder.design.Split(',').ToList().ConvertAll(e => "'" + WebAPI.Data.Utility.SafeSQL(e) + "'")));
                        break;
                }

                switch (_Salesorder.statusid)
                {
                    case SalesOrderSummaryCreteria.Status.Billed:
                        qb.AddParameter("salesorderitems.bqty", "pending", 0, ">");
                        break;
                    case SalesOrderSummaryCreteria.Status.Planned:
                        qb.AddParameter("salesorderitems.pqty", "pending", 0, ">");
                        break;
                    case SalesOrderSummaryCreteria.Status.Pending:
                        qb.AddParameter("oqty::numeric-(cqty::numeric+bqty+rqty)", "pending", 0, ">");
                        if (Services.ConfigManager.Instance(Context.OrganizationID).Config.intray_salesorder)
                            qb.AddParameter("salesorder.status", "sostatus", 1, "=");
                        else
                            qb.AddParameter("salesorder.status", "sostatus", 2, "<");
                        break;
                    case SalesOrderSummaryCreteria.Status.Cancelled:
                        qb.AddParameter("salesorderitems.cqty", "pending", 0, ">");
                        break;
                    case SalesOrderSummaryCreteria.Status.Removed:
                        qb.AddParameter("salesorderitems.rqty", "pending", 0, ">");
                        break;
                }

                command = qb.GetCommand(conn);
                var sqlcomd = command.CommandText += " order by salesorder.id";
                if (Services.ConfigManager.Instance(Context.OrganizationID).Config.analyser_stock_removeaqty)
                    sqlcomd = sqlcomd.Replace("salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty)", "salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty+salablegoods.aqty)");

                List<string> fields = new List<string>();
                List<string> stockfields = new List<string>();
                List<string> tables = new List<string>();
                if (Services.ConfigManager.Instance(Context.OrganizationID).Config.saleorder != null)
                {

                    if (Services.ConfigManager.Instance(Context.OrganizationID).Config.saleorder.brand)
                    {
                        stockfields.Add("items.brandid");
                        fields.Add("items.brandid,brand.name brandname");
                        tables.Add("left outer join brand on brand.id = items.brandid");
                    }
                    //if (Services.ConfigManager.Instance(Context.OrganizationID).Config.saleorder.sizegroup)
                    //{
                    //    stockfields.Add("lrinvoiceitems.itemdetail->>'sizegroup' sizegroup");
                    //    fields.Add("Salesorderitems.sizegroup");
                    //}
                    if (Services.ConfigManager.Instance(Context.OrganizationID).Config.saleorder.size)
                    {
                        stockfields.Add("items.sizeid");
                        fields.Add("items.sizeid,size.name sizename");
                        tables.Add("left outer join referencelist size on size.id = items.sizeid");
                    }
                    if (Services.ConfigManager.Instance(Context.OrganizationID).Config.saleorder.colour)
                    {
                        stockfields.Add("items.colourid");
                        fields.Add("items.colourid,colour.name colourname");
                        tables.Add("left outer join referencelist colour on colour.id = items.colourid");
                    }
                    if (Services.ConfigManager.Instance(Context.OrganizationID).Config.saleorder.style)
                    {
                        stockfields.Add("items.styleid");
                        fields.Add("items.styleid,style.name stylename");
                        tables.Add("left outer join referencelist style on style.id = items.styleid");
                    }
                    if (Services.ConfigManager.Instance(Context.OrganizationID).Config.saleorder.pattern)
                    {
                        stockfields.Add("items.patternid");
                        fields.Add("items.patternid,pattern.name patternname");
                        tables.Add("left outer join referencelist pattern on pattern.id = items.patternid");
                    }
                    if (Services.ConfigManager.Instance(Context.OrganizationID).Config.saleorder.sleeve)
                    {
                        stockfields.Add("items.sleeveid");
                        fields.Add("items.sleeveid,sleeve.name sleevename");
                        tables.Add("left outer join referencelist sleeve on sleeve.id = items.sleeveid");
                    }
                    if (Services.ConfigManager.Instance(Context.OrganizationID).Config.saleorder.fit)
                    {
                        stockfields.Add("items.fitid");
                        fields.Add("items.fitid,fit.name fitname");
                        tables.Add("left outer join referencelist fit on fit.id = items.fitid");
                    }
                    if (Services.ConfigManager.Instance(Context.OrganizationID).Config.saleorder.design)
                    {
                        stockfields.Add("stock.designid");
                        fields.Add("Salesorderitems.designid");
                    }
                    command.CommandText = string.Format(sqlcomd, (fields.Count > 0 ? "," : "") + string.Join(",", fields), string.Join(" ", tables),
                         (fields.Count > 0 ? "," : "") + string.Join(",", fields.ConvertAll(e => e.Split(' ')[0])));
                }
                else
                {
                    command.CommandText = string.Format(sqlcomd, "", "", "");
                }
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        SalesOrderSizeSummary.SalesOrderSizeRow oSalesorder = new SalesOrderSizeSummary.SalesOrderSizeRow();
                        oSalesorder.soid = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        oSalesorder.createdby = reader["employeeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["employeeid"]);

                        oSalesorder.sobookno = reader["sobookno"] == DBNull.Value ? "" : reader["sobookno"].ToString();
                        oSalesorder.remarks = reader["remarks"] == DBNull.Value ? "" : reader["remarks"].ToString();

                        oSalesorder.customername = reader["customername"] == DBNull.Value ? "" : reader["customername"].ToString();
                        oSalesorder.sizeid = reader["sizeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sizeid"]);
                        oSalesorder.soitmid = reader["Salesorderitemsids"] == DBNull.Value ? 0 : Convert.ToInt64(reader["Salesorderitemsids"]);
                        oSalesorder.sizegroup = reader["sizegroup"] == DBNull.Value ? "" : reader["sizegroup"].ToString();
                        if (reader["shiptoname"] != DBNull.Value)
                        {
                            oSalesorder.customername += "(" + reader["shiptoname"].ToString() + ")";
                        }
                        oSalesorder.cityname = reader["cityname"] == DBNull.Value ? "" : reader["cityname"].ToString();
                        oSalesorder.salesmen = reader["salesmen"] == DBNull.Value ? "" : reader["salesmen"].ToString();
                        oSalesorder.brand = reader["brand"] == DBNull.Value ? "" : reader["brand"].ToString();
                        oSalesorder.agentname = reader["agentname"] == DBNull.Value ? "" : reader["agentname"].ToString();
                        oSalesorder.productname = reader["productname"] == DBNull.Value ? "" : reader["productname"].ToString();
                        oSalesorder.sodate = reader["sodate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["sodate"]);
                        oSalesorder.sono = reader["sono"] == DBNull.Value ? "" : Convert.ToString(reader["sono"]);
                        oSalesorder.pendingqty = reader["pendingqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["pendingqty"]);
                        oSalesorder.plannedqty = reader["plannedqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["plannedqty"]);
                        oSalesorder.billedqty = reader["billedqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["billedqty"]);
                        oSalesorder.cancelledqty = reader["cancelledqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["cancelledqty"]);
                        oSalesorder.removedqty = reader["removedqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["removedqty"]);
                        oSalesorder.orderqty = reader["orderqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["orderqty"]);

                        if (Services.ConfigManager.Instance(Context.OrganizationID).Config.saleorder != null &&
                            Services.ConfigManager.Instance(Context.OrganizationID).Config.saleorder.colour)
                        {
                            oSalesorder.colourname = reader["colourname"] == DBNull.Value ? "" : reader["colourname"].ToString();
                        }
                        if (Services.ConfigManager.Instance(Context.OrganizationID).Config.saleorder != null &&
                        Services.ConfigManager.Instance(Context.OrganizationID).Config.saleorder.brand)
                        {
                            oSalesorder.brandname = reader["brandname"] == DBNull.Value ? "" : reader["brandname"].ToString();
                        }
                        if (Services.ConfigManager.Instance(Context.OrganizationID).Config.saleorder != null &&
                        Services.ConfigManager.Instance(Context.OrganizationID).Config.saleorder.style)
                        {
                            oSalesorder.stylename = reader["stylename"] == DBNull.Value ? "" : reader["stylename"].ToString();
                        }
                        if (Services.ConfigManager.Instance(Context.OrganizationID).Config.saleorder != null &&
                        Services.ConfigManager.Instance(Context.OrganizationID).Config.saleorder.pattern)
                        {
                            oSalesorder.patternname = reader["patternname"] == DBNull.Value ? "" : reader["patternname"].ToString();
                        }
                        if (Services.ConfigManager.Instance(Context.OrganizationID).Config.saleorder != null &&
                        Services.ConfigManager.Instance(Context.OrganizationID).Config.saleorder.sleeve)
                        {
                            oSalesorder.sleevename = reader["sleevename"] == DBNull.Value ? "" : reader["sleevename"].ToString();
                        }
                        if (Services.ConfigManager.Instance(Context.OrganizationID).Config.saleorder != null &&
                        Services.ConfigManager.Instance(Context.OrganizationID).Config.saleorder.fit)
                        {
                            oSalesorder.fitname = reader["fitname"] == DBNull.Value ? "" : reader["fitname"].ToString();
                        }
                        if (Services.ConfigManager.Instance(Context.OrganizationID).Config.saleorder != null &&
                        Services.ConfigManager.Instance(Context.OrganizationID).Config.saleorder.colour)
                        {
                            oSalesorder.designid = reader["designid"] == DBNull.Value ? "" : reader["designid"].ToString();
                        }
                        if (Services.ConfigManager.Instance(Context.OrganizationID).Config.saleorder != null &&
                        Services.ConfigManager.Instance(Context.OrganizationID).Config.saleorder.sizegroup)
                        {
                            oSalesorder.sizegroup = reader["sizegroup"] == DBNull.Value ? "" : reader["sizegroup"].ToString();
                        }
                        lst.Add(oSalesorder);
                    }
                }
            }
            catch (Exception exp)
            {

            }
            //response.ProcessSizeGroup();
            //if (response.sizelist != null)
            //    response.CompressSizeQty(WebAPI.Data.Utility.GetSizeList(response.sizelist));
            return response;
        }
        public class SalesorderStock
        {
            public long productid { get; set; }
            public long sizeid { get; set; }
            public long colourid { get; set; }
            public long fitid { get; set; }
            public long sleeveid { get; set; }
            public long patternid { get; set; }
            public long styleid { get; set; }
            public long brandid { get; set; }
            public string designid { get; set; }
            public decimal stockqty { get; set; }
            public decimal plannedqty { get; set; }
            public long salablegoodsid { get; set; }
            public string barcode { get; set; }
            public string sizegroup { get; set; }
            public string colourname { get; set; }
            public string sizename { get; set; }
            public string designitemcode { get; set; }
        }

        public List<Salesorder> Search(SalesorderCreteria _Salesorder)
        {
            List<Salesorder> lst = new List<Salesorder>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_Salesorder.id != 0)
                {
                    command = GetCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _Salesorder.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("Salesorder.isactive", true, "=");

                    if (!string.IsNullOrEmpty(_Salesorder.sono))
                    {
                        qb.AddParameter("Salesorder.sono", _Salesorder.sono, "=");
                    }
                    else
                    {
                        if (_Salesorder.locationid > 0)
                            qb.AddParameter("Salesorder.deliverylocationid", _Salesorder.locationid, "=");
                        if (_Salesorder.companyid > 0)
                            qb.AddParameter("Salesorder.companyid", _Salesorder.companyid, "=");

                        if (_Salesorder.agentid > 0)
                            qb.AddParameter("Salesorder.agentid", _Salesorder.agentid, "=");

                        if (_Salesorder.customerid > 0)
                            qb.AddParameter("Salesorder.customerid", _Salesorder.customerid, "=");

                       
                        if (_Salesorder.statusid > 0)
                            qb.AddParameter("Salesorder.status", _Salesorder.statusid, "=");

                        if (_Salesorder.cityid > 0)
                            qb.AddParameter("customer.cityid", _Salesorder.cityid, "=");

                        if (_Salesorder.fromdate != DateTime.MinValue)
                        {
                            qb.AddParameter("sodate", "sodatefrom", _Salesorder.fromdate.Date, ">=");
                            qb.AddParameter("sodate", "sodateto", _Salesorder.todate.Date.AddDays(1), "<");
                        }
                        if (_Salesorder.expectedatefrom != DateTime.MinValue)
                        {
                            qb.AddParameter("expectedate", "expectedatefrom", _Salesorder.expectedatefrom.Date, ">=");
                            qb.AddParameter("expectedate", "expectedateto", _Salesorder.expectedateto.Date.AddDays(1), "<");
                        }
                    }

                    if (Context.HasCustomerRestricted)
                    {
                        qb.AddInParamer("customerid", @"(SELECT itemlist::text::bigint AS customer_id
                        FROM users
                        CROSS JOIN LATERAL json_array_elements(users.accessiblecustomer::json) AS itemlist
                        WHERE id = @userid)");
                    }
                    qb.SortField = "salesorder.id desc";
                    qb.Limit = _Salesorder.limit;
                    command = qb.GetCommand(conn);
                    command.Parameters.Add("userid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Salesorder oSalesorder = new Salesorder();
                        oSalesorder.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        oSalesorder.tolerance = reader["tolerance"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["tolerance"]);

                        oSalesorder.sosourcetype = reader["sosourcetype"] == DBNull.Value ? 0 : Convert.ToInt32(reader["sosourcetype"]);
                        oSalesorder.salesmenid = reader["salesmenid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["salesmenid"]);

                        oSalesorder.sobookdate = reader["sobookdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["sobookdate"]);
                        oSalesorder.sobookno = reader["sobookno"] == DBNull.Value ? "" : reader["sobookno"].ToString();
                        oSalesorder.customerpo = reader["customerpo"] == DBNull.Value ? "" : reader["customerpo"].ToString();
                        oSalesorder.sobookqty = reader["sobookqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["sobookqty"]);
                        oSalesorder.soracknumber = reader["soracknumber"] == DBNull.Value ? "" : reader["soracknumber"].ToString();

                        oSalesorder.smapprovedon = reader["smapprovedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["smapprovedon"]);
                        oSalesorder.smapprovedby = reader["smapprovedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["smapprovedby"]);

                        oSalesorder.sodate = reader["sodate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["sodate"]);
                        oSalesorder.expectedate = reader["expectedate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["expectedate"]);
                        oSalesorder.sono = reader["sono"] == DBNull.Value ? "" : reader["sono"].ToString();
                        oSalesorder.sizegroup = reader["sizegroup"] == DBNull.Value ? "" : reader["sizegroup"].ToString();

                        oSalesorder.remarks = reader["remarks"] == DBNull.Value ? "" : reader["remarks"].ToString();
                        oSalesorder.customername = reader["customername"] == DBNull.Value ? "" : reader["customername"].ToString();
                        oSalesorder.cityname = reader["cityname"] == DBNull.Value ? "" : reader["cityname"].ToString();
                        oSalesorder.agentname = reader["agentname"] == DBNull.Value ? "" : reader["agentname"].ToString();

                        oSalesorder.totalsetqty = reader["totalsetqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["totalsetqty"]);
                        oSalesorder.totalqty = reader["totalqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["totalqty"]);
                        oSalesorder.orderformimageid = reader["orderformimageid"] == DBNull.Value ? "" : reader["orderformimageid"].ToString();
                        oSalesorder.plannedqty = reader["plannedqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["plannedqty"]);
                        oSalesorder.customerid = reader["customerid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["customerid"]);
                        oSalesorder.locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                        oSalesorder.employeeid = reader["employeeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["employeeid"]);
                        oSalesorder.companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"]);
                        oSalesorder.deliverylocationid = reader["deliverylocationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["deliverylocationid"]);
                        oSalesorder.termsandcondition_json = reader["termsandcondition"] == DBNull.Value ? "null" : reader["termsandcondition"].ToString();
                        oSalesorder.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        oSalesorder.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        oSalesorder.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        oSalesorder.actionby = reader["actionby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["actionby"]);
                        oSalesorder.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        oSalesorder.mode = reader["mode"] == DBNull.Value ? 0 : Convert.ToInt32(reader["mode"]);
                        oSalesorder.status = reader["status"] == DBNull.Value ? Salesorder.POStatus.PENDING : (Salesorder.POStatus)Convert.ToInt32(reader["status"]);
                        oSalesorder.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        oSalesorder.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        oSalesorder.actionon = reader["actionon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["actionon"]);
                        oSalesorder.agentid = reader["agentid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["agentid"]);
                        oSalesorder.freight = reader["freight"] == DBNull.Value ? 0 : Convert.ToInt32(reader["freight"]);

                        oSalesorder.hasshipto = reader["hasshipto"] == DBNull.Value ? false : Convert.ToBoolean(reader["hasshipto"]);
                        oSalesorder.shiptocustomerid = reader["shiptocustomerid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["shiptocustomerid"]);

                        oSalesorder.customerstateid = reader["customerstateid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["customerstateid"]);


                        oSalesorder.discountpercentage = reader["discountpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discountpercentage"]);
                        lst.Add(oSalesorder);
                    }
                }
                if (!_Salesorder.onlyheader && (_Salesorder.id > 0 || !string.IsNullOrEmpty(_Salesorder.sono)) && lst.Count == 1)
                {

                    SalesorderitemsDataService subSalesorderitems = new SalesorderitemsDataService() { Context = this.Context };
                    subSalesorderitems.Context = this.Context;
                    lst[0].Salesorderitems = subSalesorderitems.Search(new SalesorderitemsCreteria()
                    {
                        salesorderid = lst[0].id
                    });
                }
                else if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.mfg_mode == 6)
                {
                    string query = string.Format(@"with sampleorders as
                    (
                        select distinct mfgsampleorderid,salesorderid from salesorderitems
                        where salesorderid in ({0})
                    )
                    select sampleorders.*,Mfgsampleorder.entryno,Mfgsampleorder.entrydate,Mfgsampleorder.designno,
                    Mfgsampleorder.samplename,referencelist.name division,employee.name merchant,agent.name marketing from sampleorders
                    inner join Mfgsampleorder on Mfgsampleorder.id = mfgsampleorderid
                    left outer join referencelist on referencelist.id = prddivisionid
                    left outer join employee on employee.id = mfgsampleorder.preparedby
                    left outer join agent on agent.id = mfgsampleorder.agentid",
                    string.Join(",", lst.ConvertAll(x => x.id)));
                    using (var reader = ExecuteReader(query, null))
                    {
                        while (reader.Read())
                        {
                            long salesorderid = Convert.ToInt64(reader["salesorderid"]);
                            var salesorder = lst.Find(x => x.id == salesorderid);
                            salesorder.mfglabel = new Salesorder.mfglabelinfo();
                            salesorder.mfglabel.samplename = reader["samplename"].ToString();
                            salesorder.mfglabel.division = reader["division"] == DBNull.Value ? "" : reader["division"].ToString();
                            salesorder.mfglabel.entrydate = Convert.ToDateTime(reader["entrydate"]);
                            salesorder.mfglabel.designno = reader["designno"].ToString();
                            salesorder.mfglabel.entryno = reader["entryno"].ToString();
                            salesorder.mfglabel.merchant = reader["merchant"].ToString();
                            salesorder.mfglabel.marketing = reader["marketing"].ToString();
                        }
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _Salesorder);
            }
            return lst;
        }

        public SalesOrderSearchBySize SearchBySize(List<Salesorder> list)
        {
            SalesOrderSearchBySize response = new SalesOrderSearchBySize();
            string query = @"
                    select ID,name from referencelist where type ='SIZE' and isactive=true;
                    select salesorderid id,
                    STRING_AGG(cast(Salesorderitems.sizeid as varchar)||':'||cast(oqty-(bqty+cqty) as varchar),',') sizeids
                    from Salesorderitems
                    where salesorderid in ({0})
                    group by salesorderid";


            response.items = list.ConvertAll(x => new SalesorderSizeDetail()
            {
                id = x.id,
                sono = x.sono,
                sodate = x.sodate,
                customerid = x.customerid,
                locationid = x.locationid,
                customername = x.customername,
                cancelled = x.cancelled,
                received = x.received,
                ordered = x.ordered,
                status = x.status,
                agentid = x.agentid,
                tolerance = x.tolerance,
                expectedate = x.expectedate,
                deliverylocationid = x.deliverylocationid,
            });
            response.sizenames = new SortedDictionary<long, string>();
            using (var reader = ExecuteReader(string.Format(query, string.Join(",", list.ConvertAll(x => x.id))),
                new SortedList<string, object>()))
            {
                while (reader.Read())
                {
                    response.sizenames.Add(reader.GetInt64(0), reader.GetString(1));
                }
                reader.NextResult();
                while (reader.Read())
                {
                    long id = reader.GetInt64(0);
                    response.items.Find(x => x.id == id).sizeids = reader.GetString(1);
                }
            }
            response.ProcessSizeGroup();
            response.CompressSizeQty(WebAPI.Data.Utility.GetSizeList(response.sizelist));
            return response;
        }

        public bool ValidateStock(Salesorder salesorder)
        {
            if (salesorder.id == 0 && salesorder.Salesorderitems != null &&
                salesorder.Salesorderitems.Exists(x => x.isactive == false))
                throw new Exception("New Sales Order can't have deleted order items, please clear the cache and try again or contact software vendor");

            if (salesorder.id > 0 && salesorder.Salesorderitems != null &&
                salesorder.Salesorderitems.Exists(x => x.isactive == false && x.id == 0))
                throw new Exception("Old Sales Order can't have deleted order items, please clear the cache and try again or contact software vendor");

            if (salesorder.id > 0 && salesorder.Salesorderitems != null &&
                salesorder.Salesorderitems.Count(x => x.isactive == false) > 5)
                throw new Exception("Can't delete more than 5 Sales Order designs at the time, please clear the cache and try again or contact software vendor");

            MfgdesignDataService mfgdesignDataService = new MfgdesignDataService() { Context = this.Context };
            List<MFGDesinInfo.SizeGroup> sizegroups = salesorder.Salesorderitems.GroupBy(x => x.designitemcode).ToList().ConvertAll(x =>
            {
                var sgitem = new MFGDesinInfo.SizeGroup()
                {
                    designcode = x.Key,
                    list = new List<MFGDesinInfo.SizeItem>()
                };

                foreach (var item in x)
                {
                    if (item.jumpqty != null && item.jumpqty.Count > 0)
                    {
                        sgitem.list.AddRange(item.jumpqty.ConvertAll(y =>
                        {
                            return new MFGDesinInfo.SizeItem()
                            {
                                uid = y.uid,
                                name = y.name,
                                oqty = y.oqty,
                            };
                        }));
                    }
                    else
                    {
                        sgitem.list.Add(new MFGDesinInfo.SizeItem()
                        {
                            uid = item.uid,
                            name = item.sizename
                        });
                    }
                }
                return sgitem;
            });

            List<MFGDesinInfo.SizeItem> existingqty = new List<MFGDesinInfo.SizeItem>();
            if (salesorder.id > 0)
            {
                var existing = Search(new SalesorderCreteria() { id = salesorder.id });
                existingqty = existing[0].Salesorderitems.ConvertAll(x =>
                {
                    return new MFGDesinInfo.SizeItem()
                    {
                        uid = x.uid,
                        order = x.oqty,
                        oqty = x.bqty + x.rqty + x.cqty
                    };
                });
            }

            mfgdesignDataService.UpdateStock(salesorder.id, sizegroups, new List<long>(new long[] { salesorder.deliverylocationid }),
                InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[salesorder.deliverylocationid].mfg_so_wip,
                InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[salesorder.deliverylocationid].mfg_so_fs);

            List<string> stockValidation = new List<string>();
            foreach (var item in sizegroups)
            {
                foreach (var sitem in item.list)
                {
                    var stock = sitem.stock;
                    //if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[salesorder.deliverylocationid].mfg_so_fg)
                    //    stock += sitem.fg;
                    //else 
                    if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[salesorder.deliverylocationid].mfg_so_wip)
                        stock += sitem.wip;
                    decimal deliveredqty = 0;
                    if (existingqty != null &&
                        existingqty.Exists(x => x.uid == sitem.uid))
                    {
                        deliveredqty = existingqty.Find(x => x.uid == sitem.uid).oqty;
                        if (existingqty.Find(x => x.uid == sitem.uid).order == sitem.oqty)
                            continue;
                    }
                    if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[salesorder.deliverylocationid].mfg_so_po)
                    {
                        if ((sitem.oqty - deliveredqty) + sitem.order > stock)
                        {
                            stockValidation.Add(sitem.uid+" - "+item.designcode + " (" + sitem.name + ") shortage of " + ((sitem.oqty + sitem.order) - stock));
                        }
                    }
                    else
                    {
                        if (sitem.oqty - deliveredqty > stock)
                        {
                            stockValidation.Add(sitem.uid + " - "+item.designcode + " (" + sitem.name + ") shortage of " + ((sitem.oqty + sitem.order) - stock));
                        }
                    }
                }
            }
            if (stockValidation.Count > 0)
            {
                throw new Exception(String.Join("<br>", stockValidation));
            }
            return true;
        }

        public bool ValidateSOBOOKNODuplicate(Salesorder salesorder)
        {
            string query = @"select sono,designid,uid from salesorder
                            inner join salesorderitems on salesorderitems.salesorderid = salesorder.id
                            and salesorderitems.isactive=true
                            where sobookno = @sobookno and salesorder.isactive=true and salesorder.id != @id";
            var param = new SortedList<string, object>();
            param.Add("sobookno", salesorder.sobookno);
            param.Add("id", salesorder.id);
            var table = ExecuteDataSet(query, param).Tables[0].AsEnumerable().Select(x =>
                new { sono = x["sono"].ToString(), designid = x["designid"].ToString(), uid = x["uid"].ToString() }).ToList();

            var duplicates = table.FindAll(x => salesorder.Salesorderitems.Exists(y => y.uid == x.uid));
            if (duplicates.Count > 0)
            {
                throw new Exception(salesorder.sobookno + " already having a salesorder - " + duplicates[0].sono);
            }
            return true;
        }

        public Salesorder Insert(Salesorder oSalesorder)
        {

            if (oSalesorder.deliverylocationid == 0)
            {
                throw new Exception("Please select the stock location");
            }

            SalesorderitemsDataService subSalesorderitems = new SalesorderitemsDataService() { Context = this.Context };
            subSalesorderitems.Context = this.Context;
            NpgsqlTransaction transaction = null;
            try
            {

                if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.mfg_mode == 6 &&
                        !string.IsNullOrEmpty(oSalesorder.sobookno))
                {
                    ValidateSOBOOKNODuplicate(oSalesorder);
                }


                if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig != null &&
                    InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig.ContainsKey(oSalesorder.deliverylocationid) &&
                    InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[oSalesorder.deliverylocationid].mfg_sovalidate)
                {
                    if (!ValidateStock(oSalesorder))
                        return oSalesorder;
                }
                var sequence = SequenceManager.Instance(Context.OrganizationID).Get("salesorder.sono",
                    InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_supportlocationwisesopackingno ? oSalesorder.locationid : 0,
                    InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_supportcompanywiseentryno ?
                    oSalesorder.companyid : 0);
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    if (string.IsNullOrEmpty(oSalesorder.sono))
                    {
                        oSalesorder.sono = sequence.GetNextNumber(this.Context, conn, transaction);
                    }

                    if (string.IsNullOrEmpty(oSalesorder.sobookno))
                        oSalesorder.sobookno = "-";

                    oSalesorder.totalsetqty = oSalesorder.Salesorderitems.GroupBy(x => new { x.designitemcode, x.sizeid })
                       .Select(y => new { y.Key.designitemcode, y.Key.sizeid, set = y.Average(z => z.oqty) })
                       .Sum(x => x.set);
                    oSalesorder.totalqty = oSalesorder.Salesorderitems.Sum(e => (e.jumpqty == null || e.jumpqty.Count == 0) ? e.oqty : e.jumpqty.Sum(x => x.oqty));

                    Insert(transaction, conn, oSalesorder);
                    foreach (var item in oSalesorder.Salesorderitems)
                    {
                        if (!string.IsNullOrEmpty(item.pricerange))
                            item.priceranges = item.pricerange;
                        item.isactive = true;
                        item.salesorderid = oSalesorder.id;
                        if (item.jumpqty != null && item.jumpqty.Count > 0)
                        {
                            foreach (var jitem in item.jumpqty)
                            {
                                //item.id = jitem.id;
                                item.itemid = jitem.itemid;
                                item.oqty = jitem.oqty;
                                item.sizeid = jitem.sizeid;
                                item.size = jitem.name;
                                item.sizename = jitem.name;
                                item.sizegroup = item.sizegroup;
                                item.sizegroupid = item.sizegroupid;
                                item.rowtype = 2;
                                item.uid = jitem.uid;
                                item.designid = item.designid;
                                item.designuid = item.designuid;
                                subSalesorderitems.Insert(transaction, conn, item);
                                jitem.id = item.id;
                            }
                            item.id = -1;
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(item.sizename) && string.IsNullOrEmpty(item.size))
                                item.size = item.sizename;
                            subSalesorderitems.Insert(transaction, conn, item);
                        }
                    }

                    string mfgvalidateion = @"            with alluid
            as
            (
            select mfgdesign.code,schemelist->>'sizegroup' sizegroup,schemelist->>'uid' uid,cast(schemelist->>'sizeid' as bigint) sizeid,
cast(schemelist->>'wsrate' as numeric) wsrate,cast(schemelist->>'dealerprice' as numeric) dealerprice,
cast(schemelist->>'salerate' as numeric) salerate,cast(schemelist->>'mrp' as numeric) mrp,
schemelist->>'code' dname,cast(schemelist->>'colourid' as bigint) colourid,
	case when position('/' in schemelist->>'code') = 0 then '' else
	substring(schemelist->>'code',position('/' in schemelist->>'code')+1)
	end colourcode
	from mfgdesign
            LEFT JOIN LATERAL
            json_array_elements (cast(mfgdesign.sizegroup->>'items' as json)) schemelist ON true
			where (split_part(schemelist->>'code','/',2)~ '^\d+$')=false
            and mfgdesign.isactive=true
            ),allnormaluid
            as
            (
            select mfgdesign.code,schemelist->>'sizegroup' sizegroup,schemelist->>'uid' uid,cast(schemelist->>'sizeid' as bigint) sizeid,
cast(schemelist->>'wsrate' as numeric) wsrate,cast(schemelist->>'dealerprice' as numeric) dealerprice,
cast(schemelist->>'salerate' as numeric) salerate,cast(schemelist->>'mrp' as numeric) mrp,
schemelist->>'code' dname,cast(schemelist->>'colourid' as bigint) colourid,
	case when position('/' in schemelist->>'code') = 0 then '' else
	substring(schemelist->>'code',position('/' in schemelist->>'code')+1)
	end colourcode
	from mfgdesign
            LEFT JOIN LATERAL
            json_array_elements (cast(mfgdesign.sizegroup->>'items' as json)) schemelist ON true
			where (split_part(schemelist->>'code','/',2)~ '^\d+$')=true
            and mfgdesign.isactive=true
            )
            select alluid.uid,allnormaluid.uid,split_part(alluid.colourcode,'(',1) from alluid
			left outer join allnormaluid on allnormaluid.code = alluid.code and 
			allnormaluid.colourcode = split_part(alluid.colourcode,'(',1) and
			allnormaluid.sizeid = alluid.sizeid
			where allnormaluid.uid is not null";


                    //                    string tracesovalitation = @"with alluid
                    //as
                    //(
                    //select mfgdesign.code,schemelist->> 'sizegroup' sizegroup,schemelist->> 'uid' uid,cast(schemelist->> 'sizeid' as bigint) sizeid,
                    //cast(schemelist->> 'wsrate' as numeric) wsrate,cast(schemelist->> 'dealerprice' as numeric) dealerprice,
                    //cast(schemelist->> 'salerate' as numeric) salerate,cast(schemelist->> 'mrp' as numeric) mrp,
                    //schemelist->> 'code' dname,cast(schemelist->> 'colourid' as bigint) colourid,
                    //	case when position('/' in schemelist->> 'code') = 0 then '' else
                    //	substring(schemelist->> 'code', position('/' in schemelist->> 'code') + 1)

                    //    end colourcode

                    //    from mfgdesign
                    //LEFT JOIN LATERAL
                    //json_array_elements(cast(mfgdesign.sizegroup->> 'items' as json)) schemelist ON true
                    //where isactive = true
                    //)

                    //select case when alluid.dname != salesorderitems.designitemcode then 'DEsignitemcode'
                    // when alluid.colourcode != salesorderitems.colourcode then 'colourcode'
                    // when alluid.code != salesorderitems.designid then 'designcode'
                    //  when alluid.colourid != salesorderitems.colourid then 'colourid'
                    //when alluid.sizeid != salesorderitems.sizeid  then 'sizeid'

                    //else 'No error' end reason,


                    //salesorderitems.id,alluid.dname,salesorderitems.designitemcode,alluid.colourcode,
                    //salesorderitems.colourcode,salesorderitems.uid,alluid.code , salesorderitems.designid,
                    //alluid.colourid , salesorderitems.colourid from salesorderitems
                    //    inner join salesorder on salesorder.id = salesorderitems.salesorderid

                    //    inner join alluid on alluid.uid = salesorderitems.uid

                    //    where(alluid.code != salesorderitems.designid or
                    //alluid.dname != salesorderitems.designitemcode or
                    //alluid.colourcode != salesorderitems.colourcode or
                    //alluid.colourid != salesorderitems.colourid or
                    //alluid.sizeid != salesorderitems.sizeid)
                    //and salesorderitems.isactive = true



                    //select info->>'sono', alluid.dname,salesorderitems->>'designitemcode',alluid.colourcode,
                    //salesorderitems->>'colourcode',salesorderitems->>'uid',alluid.code , salesorderitems->>'designid',
                    //alluid.colourid , salesorderitems->>'colourid',
                    //case when alluid.dname != salesorderitems->>'designitemcode' then 'DEsignitemcode'
                    // when alluid.colourcode != salesorderitems->>'colourcode' then 'colourcode'
                    // when alluid.code != salesorderitems->>'designid' then 'designcode'
                    //  when alluid.colourid != cast(salesorderitems->>'colourid' as bigint) then 'colourid'
                    //  when alluid.sizeid != cast(salesorderitems->>'sizeid' as bigint) then 'sizeid'

                    //  else 'No error' end reason


                    //from trace

                    //LEFT JOIN LATERAL json_array_elements (cast(info->>'Salesorderitems' as json)) salesorderitems ON true
                    //inner join alluid on alluid.uid = salesorderitems->>'uid'
                    //where "methodName"->>'Exception' ilike 'SO Validation failed %' and info::text != '"Unhandled Error"' and 
                    // (alluid.code != salesorderitems->> 'designid' or
                    //alluid.dname != salesorderitems->> 'designitemcode' or
                    //alluid.colourcode != salesorderitems->> 'colourcode' or
                    //alluid.colourid != cast(salesorderitems->> 'colourid' as bigint) or
                    //alluid.sizeid != cast(salesorderitems->> 'sizeid' as bigint))";


                    if ((InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.mfg_mode != 5 &&
                        InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.mfg_mode != 6))
                    {
                        string sovalidation = string.Format(@"with alluid
as
(
select mfgdesign.code,schemelist->>'sizegroup' sizegroup,schemelist->>'uid' uid,cast(schemelist->>'sizeid' as bigint) sizeid,
cast(schemelist->>'wsrate' as numeric) wsrate,cast(schemelist->>'dealerprice' as numeric) dealerprice,
cast(schemelist->>'salerate' as numeric) salerate,cast(schemelist->>'mrp' as numeric) mrp,
schemelist->>'code' dname,cast(schemelist->>'colourid' as bigint) colourid,
	case when position('/' in schemelist->>'code') = 0 then '' else
	substring(schemelist->>'code',position('/' in schemelist->>'code')+1)
	end colourcode
	from mfgdesign 
LEFT JOIN LATERAL
json_array_elements (cast(mfgdesign.sizegroup->>'items' as json)) schemelist ON true
where isactive=true and mfgdesign.code in ({0})
)

select salesorderitems.id,alluid.dname,salesorderitems.designitemcode,alluid.colourcode,
salesorderitems.colourcode,salesorderitems.uid,alluid.code , salesorderitems.designid,
alluid.colourid , salesorderitems.colourid,alluid.sizeid asizeid , salesorderitems.sizeid ssizeid from salesorder
	inner join salesorderitems on  salesorderitems.salesorderid=salesorder.id
	inner join alluid on alluid.uid = salesorderitems.uid
	where salesorder.id = @id and (alluid.code != salesorderitems.designid or 
alluid.dname != salesorderitems.designitemcode or 
alluid.colourcode != salesorderitems.colourcode or 
alluid.colourid != salesorderitems.colourid or 
alluid.sizeid != salesorderitems.sizeid)
and salesorderitems.isactive=true", string.Join(",", oSalesorder.Salesorderitems.GroupBy(x => x.designid).ToList().ConvertAll(y => "'" + y.Key + "'")));

                        var param = new SortedList<string, object>();
                        param.Add("id", oSalesorder.id);
                        using (var reader = ExecuteReader(sovalidation, param, conn, transaction))
                        {
                            if (reader.Read())
                            {
                                int failedcount = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
                                if (failedcount > 0)
                                {
                                    List<string> msg = new List<string>();
                                    string dname = reader.GetString(1);
                                    string designitemcode = reader.GetString(2);
                                    if (dname != designitemcode)
                                    {
                                        msg.Add("SKU Design " + dname + ", SO Design " + designitemcode);
                                    }
                                    string alluidcode = reader.GetString(6);
                                    string designid = reader.GetString(7);
                                    if (alluidcode != designid)
                                    {
                                        msg.Add("SKU Design " + alluidcode + ", SO Design " + designid);
                                    }
                                    string acolourcode = reader.GetString(3);
                                    string scolourcode = reader.GetString(4);
                                    if (acolourcode != scolourcode)
                                    {
                                        msg.Add("SKU Colour Code " + acolourcode + ", SO Colour Code " + scolourcode);
                                    }

                                    int acolourid = reader[8] == DBNull.Value ? 0 : reader.GetInt32(8);
                                    int scolourid = reader[9] == DBNull.Value ? 0 : reader.GetInt32(9);
                                    if (acolourid != scolourid)
                                    {
                                        msg.Add("SKU Colour ID " + acolourid + ", SO Colour ID " + scolourid);
                                    }

                                    int asizeid = reader[10] == DBNull.Value ? 0 : reader.GetInt32(10);
                                    int ssizeid = reader[11] == DBNull.Value ? 0 : reader.GetInt32(11);
                                    if (asizeid != ssizeid)
                                    {
                                        msg.Add("SKU Size ID " + asizeid + ", SO Size ID " + ssizeid);
                                    }

                                    string uid = reader.GetString(5);
                                    string info = string.Format("{0}  - {1}", uid, string.Join("<br>", msg));
                                    reader.Close();
                                    transaction.Rollback();
                                    WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(new Exception("SO Validation failed (" + info + ")," +
                                        "please contact support team"), WebAPI.Common.Logger.LogType.Error, oSalesorder);
                                }
                            }
                        }
                    }

                    if (Services.ConfigManager.Instance(Context.OrganizationID).Config.intray_salesorder)
                    {
                        CustomerDataService supplierdata = new CustomerDataService() { Context = this.Context };
                        var supplier = supplierdata.Search(new CustomerCreteria() { id = oSalesorder.customerid });
                        if (supplier != null && supplier.Count > 0)
                        {
                            Services.IntrayManager.Instance(Context.OrganizationID).Add(Context.OrganizationID, Services.IntrayManager.EntryType.SalesOrder,
                                oSalesorder.id, string.Format("Sales ORDER - FOR THE Customer {0} WITH {1} QTY.", supplier[0].name,
                                oSalesorder.Salesorderitems.Sum(x => x.oqty)), Context.UserID, oSalesorder.locationid, transaction, this.Context);
                        }
                    }
                    var paramso = new SortedList<string, object>();
                    paramso.Add("salesorderid", oSalesorder.id);
                    decimal orderqty = 0;
                    using (var reader = ExecuteReader("select sum(oqty) from salesorderitems where salesorderid=@salesorderid and isactive=true", paramso, conn, transaction))
                    {
                        if (reader.Read())
                        {
                            orderqty = reader[0] == DBNull.Value ? 0 : Convert.ToDecimal(reader[0]);
                        }
                    }
                    if (orderqty != oSalesorder.totalqty)
                    {
                        transaction.Rollback();
                        WebAPI.Common.Logger.Instance(Context.OrganizationID).PerformanceLog("Salesorder qty mismatches", WebAPI.Common.Logger.LogType.Info,
                            "salesorderdataservice/insert", oSalesorder, Context.UserID, oSalesorder.deliverylocationid);
                        throw new Exception("Saved qty " + oSalesorder.totalqty + ", avilable qty " + orderqty + " check the order detail before save");
                    }
                    transaction.Commit();
                    conn.Close();
                }
                oSalesorder.Salesorderitems = subSalesorderitems.Search(new SalesorderitemsCreteria()
                {
                    salesorderid = oSalesorder.id
                });
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oSalesorder);
            }

            return oSalesorder;
        }

        public List<SalesorderItemDetail> SearchSO(SalesorderCreteria SalesorderCreteria)
        {
            string SQL_SearchPO = @"select Salesorderitems.id,Salesorder.sodate,Salesorder.id,Salesorder.sono,oqty,
                            oqty-(rqty+cqty) balance,designid,price,Salesorderitems.imageid,Salesorderitems.description,referencelist.name sizename,
                            tolerance from Salesorder
                            inner join Salesorderitems on Salesorderitems.Salesorderid = Salesorder.id
                            left outer join items on items.id = itemid
							left outer join referencelist on referencelist.id = items.sizeid
                            where Salesorder.customerid = @customerid and Salesorderitems.productid = @productid 
                            and Salesorder.isactive = true and Salesorderitems.isactive = true and Salesorder.status = 1 
                            and Salesorder.sodate>=@fromdate and oqty-(rqty+cqty)>0";

            List<SalesorderItemDetail> SalesorderItemDetails = new List<SalesorderItemDetail>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                var command = GetCommand(SQL_SearchPO, conn);
                command.Parameters.Add("customerid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = SalesorderCreteria.customerid;
                command.Parameters.Add("productid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = SalesorderCreteria.productid;
                command.Parameters.Add("fromdate", NpgsqlTypes.NpgsqlDbType.Date).Value = DateTimeNow.AddMonths(-3);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        SalesorderItemDetail SalesorderItemDetail = new SalesorderItemDetail();
                        SalesorderItemDetail.Salesorderitemsid = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        SalesorderItemDetail.sodate = reader["sodate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["sodate"]);
                        SalesorderItemDetail.sono = reader["sono"] == DBNull.Value ? "" : reader["sono"].ToString();
                        SalesorderItemDetail.description = reader["description"] == DBNull.Value ? "" : reader["description"].ToString();
                        if (reader["sizename"] != DBNull.Value)
                            SalesorderItemDetail.description += "/" + reader["sizename"].ToString();
                        SalesorderItemDetail.imageid = reader["imageid"] == DBNull.Value ? "" : reader["imageid"].ToString();
                        SalesorderItemDetail.designid = reader["designid"] == DBNull.Value ? "" : reader["designid"].ToString();
                        SalesorderItemDetail.balance = reader["balance"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["balance"]);
                        SalesorderItemDetail.price = reader["price"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["price"]);
                        SalesorderItemDetail.order = reader["oqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["oqty"]);
                        SalesorderItemDetail.tolerance = reader["tolerance"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["tolerance"]);

                        if (SalesorderItemDetail.tolerance > 0)
                        {
                            SalesorderItemDetail.maxqty = (SalesorderItemDetail.order + (SalesorderItemDetail.order * SalesorderItemDetail.tolerance / 100));
                            SalesorderItemDetail.maxqty = Math.Round(SalesorderItemDetail.maxqty - (SalesorderItemDetail.order - SalesorderItemDetail.balance), 2);
                        }
                        else
                        {
                            SalesorderItemDetail.maxqty = (SalesorderItemDetail.order - SalesorderItemDetail.balance);
                        }
                        SalesorderItemDetails.Add(SalesorderItemDetail);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, SalesorderCreteria);
            }
            return SalesorderItemDetails.OrderBy(e => e.sodate).ToList();
        }

        public Salesorder.Termsandcondition GetTermsandcondition(long id)
        {
            Salesorder.Termsandcondition response = null;
            string query = "select termsandcondition from salesorder where id=@id";
            var param = new SortedList<string, object>();
            param.Add("id", id);
            using (var reader = ExecuteReader(query, param))
            {
                if (reader.Read())
                {
                    if (!reader.IsDBNull(0))
                    {
                        response = Newtonsoft.Json.JsonConvert.DeserializeObject<Salesorder.Termsandcondition>(reader.GetString(0));
                    }
                }
            }
            if (response == null)
                response = new Salesorder.Termsandcondition();

            return response;
        }
        public Salesorder.Termsandcondition GetSMApproveval(long id)
        {
            Salesorder.Termsandcondition response = null;
            string query = "select smapprovedby,smapprovedon from salesorder where id=@id";
            var param = new SortedList<string, object>();
            param.Add("id", id);
            using (var reader = ExecuteReader(query, param))
            {
                if (reader.Read())
                {
                    if (!reader.IsDBNull(0) &&
                        !reader.IsDBNull(1))
                    {
                        response = new Salesorder.Termsandcondition()
                        {
                            pickedby = reader.GetInt64(0),
                            pickedon = reader.GetDateTime(1)
                        };
                    }
                }
            }
            if (response == null)
                response = new Salesorder.Termsandcondition()
                {
                    pickedon = DateTime.Now,
                    pickedby = Context.EmployeeID
                };

            return response;
        }

        public bool SetTermsandcondition(SOTermsandcondition data)
        {
            int result = 0;
            string query = "update salesorder set termsandcondition = @termsandcondition where id in (" + String.Join(",", data.ids) + ")";
            using (NpgsqlConnection conn = CreateConnection())
            {
                conn.Open();
                try
                {
                    var command = GetCommand(query, conn);
                    command.Parameters.Add("termsandcondition", NpgsqlTypes.NpgsqlDbType.Json).Value =
                        data.Copy == null ? "null" : Newtonsoft.Json.JsonConvert.SerializeObject(data.Copy);
                    result = command.ExecuteNonQuery();
                    conn.Close();
                }
                catch (Exception exp)
                {
                    conn.Close();
                }
            }
            return result > 0;
        }
        public bool SetSMApproveval(SOTermsandcondition data)
        {
            int result = 0;
            string query = "update salesorder set smapprovedby = @smapprovedby,smapprovedon = @smapprovedon where id in (" + String.Join(",", data.ids) + ")";
            using (NpgsqlConnection conn = CreateConnection())
            {
                conn.Open();
                try
                {
                    var command = GetCommand(query, conn);
                    command.Parameters.Add("smapprovedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = data.pickedby;
                    if(data.pickedon.Date == DateTime.MinValue)
                        command.Parameters.Add("smapprovedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
                    else
                        command.Parameters.Add("smapprovedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = data.pickedon;
                    result = command.ExecuteNonQuery();
                    conn.Close();
                }
                catch (Exception exp)
                {
                    conn.Close();
                }
            }
            return result > 0;
        }
        public List<SalesOrderDetail> GetSaleOrder(string locationid, string sono, string sosourcetype)
        {
            List<SalesOrderDetail> lst = new List<SalesOrderDetail>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                QueryBuilder qb = new QueryBuilder();
                qb.CurrentQuery = SQL_OrderForm;
                qb.AddParameter("salesorderitems.isactive", true, "=");
                qb.AddParameter("salesorder.isactive", true, "=");

                if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.mfg_mode == 5)
                {
                    if (!string.IsNullOrEmpty(locationid))
                        qb.AddParameter("salesorder.locationid", long.Parse(locationid), "=");
                    if (!string.IsNullOrEmpty(sosourcetype) && sosourcetype == "2")
                        qb.AddParameter("salesorder.sosourcetype", 2, "=");
                    else
                        qb.AddParameter("salesorder.sosourcetype", 2, "!=");
                }

                if (Services.ConfigManager.Instance(Context.OrganizationID).Config.intray_salesorder)
                {
                    qb.AddParameter("salesorder.status", 1, "=");
                }
                if (sono.IndexOf(',') > 0)
                {
                    qb.AddInParamer("salesorder.sono", string.Join(",", sono.Split(',').ToList().ConvertAll(e => "'" + e + "'")));
                }
                else
                {
                    qb.AddParameter("salesorder.sono", sono, "=");
                }
                NpgsqlCommand command = qb.GetCommand(conn);
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        SalesOrderDetail oSalesorder = new SalesOrderDetail();
                        oSalesorder.customerid = reader["customerid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["customerid"]);
                        oSalesorder.soid = reader["soid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["soid"]);
                        oSalesorder.soitemid = reader["soitemid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["soitemid"]);
                        oSalesorder.productid = reader["productid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productid"]); ;
                        oSalesorder.pendingqty = reader["pendingqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["pendingqty"]);
                        oSalesorder.qty = reader["oqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["oqty"]);
                        oSalesorder.price = reader["price"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["price"]);
                        oSalesorder.mrp = reader["mrp"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["mrp"]);
                        oSalesorder.sodate = reader["sodate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["sodate"]);
                        oSalesorder.description = reader["description"] == DBNull.Value ? "" : Convert.ToString(reader["description"]);
                        oSalesorder.customername = reader["customername"] == DBNull.Value ? "" : Convert.ToString(reader["customername"]);
                        oSalesorder.designitemcode = reader["designitemcode"] == DBNull.Value ? "" : Convert.ToString(reader["designitemcode"]);
                        oSalesorder.uid = reader["uid"] == DBNull.Value ? "" : Convert.ToString(reader["uid"]);
                        oSalesorder.sono = reader["sono"] == DBNull.Value ? "" : Convert.ToString(reader["sono"]);
                        oSalesorder.billedqty = reader["bqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["bqty"]);
                        oSalesorder.sizeid = reader["sizeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sizeid"]);
                        oSalesorder.colourid = reader["colourid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["colourid"]);

                        if (Convert.ToBoolean(reader["hasshipto"]))
                            oSalesorder.shipto = reader["shiptocustomerid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["shiptocustomerid"]);

                        if (Services.ConfigManager.Instance(Context.OrganizationID).Config.saleorder != null &&
                            Services.ConfigManager.Instance(Context.OrganizationID).Config.saleorder.colour)
                        {
                            oSalesorder.colourname = reader["colourname"] == DBNull.Value ? "" : Convert.ToString(reader["colourname"]);
                            oSalesorder.colourid = reader["colourid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["colourid"]);
                        }
                        if (Services.ConfigManager.Instance(Context.OrganizationID).Config.saleorder != null &&
                        Services.ConfigManager.Instance(Context.OrganizationID).Config.saleorder.brand)
                        {
                            oSalesorder.brandname = reader["brandname"] == DBNull.Value ? "" : Convert.ToString(reader["brandname"]);
                            oSalesorder.brandid = reader["brandid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["brandid"]);
                        }
                        if (Services.ConfigManager.Instance(Context.OrganizationID).Config.saleorder != null &&
                        Services.ConfigManager.Instance(Context.OrganizationID).Config.saleorder.style)
                        {
                            oSalesorder.stylename = reader["stylename"] == DBNull.Value ? "" : Convert.ToString(reader["stylename"]);
                            oSalesorder.styleid = reader["styleid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["styleid"]);
                        }
                        if (Services.ConfigManager.Instance(Context.OrganizationID).Config.saleorder != null &&
                        Services.ConfigManager.Instance(Context.OrganizationID).Config.saleorder.pattern)
                        {
                            oSalesorder.patternname = reader["patternname"] == DBNull.Value ? "" : Convert.ToString(reader["patternname"]);
                            oSalesorder.patternid = reader["patternid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["patternid"]);
                        }
                        if (Services.ConfigManager.Instance(Context.OrganizationID).Config.saleorder != null &&
                        Services.ConfigManager.Instance(Context.OrganizationID).Config.saleorder.sleeve)
                        {
                            oSalesorder.sleevename = reader["sleevename"] == DBNull.Value ? "" : Convert.ToString(reader["sleevename"]);
                            oSalesorder.sleeveid = reader["sleeveid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sleeveid"]);
                        }
                        if (Services.ConfigManager.Instance(Context.OrganizationID).Config.saleorder != null &&
                        Services.ConfigManager.Instance(Context.OrganizationID).Config.saleorder.fit)
                        {
                            oSalesorder.fitname = reader["fitname"] == DBNull.Value ? "" : Convert.ToString(reader["fitname"]);
                            oSalesorder.fitid = reader["fitid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["fitid"]);
                        }
                        if (Services.ConfigManager.Instance(Context.OrganizationID).Config.saleorder != null &&
                        Services.ConfigManager.Instance(Context.OrganizationID).Config.saleorder.colour)
                        {
                            oSalesorder.designid = reader["designid"] == DBNull.Value ? "" : reader["designid"].ToString();
                        }
                        if (Services.ConfigManager.Instance(Context.OrganizationID).Config.saleorder != null &&
                        Services.ConfigManager.Instance(Context.OrganizationID).Config.saleorder.sizegroup)
                        {
                            oSalesorder.sizegroup = reader["sizegroup"] == DBNull.Value ? "" : reader["sizegroup"].ToString();
                        }
                        lst.Add(oSalesorder);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error,
                    "GetSaleOrder:" + sono);
            }

            long Lid = 0;
            if (long.TryParse(locationid, out Lid) &&
                Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig.ContainsKey(Lid) &&
               Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[Lid].sales_so_partial)
            {
                lst.RemoveAll(x => x.billedqty > 0);
            }

            if (lst.Count > 0)
            {
                if (!lst.All(e => e.customerid == lst[0].customerid))
                {
                    throw new Exception("Different customer order id, can't bill together.");
                }
            }

            if (sosourcetype != "0" && sosourcetype != "2")
            {
                if (!string.IsNullOrEmpty(sono))
                {
                    string pendingpacking = @"select schemelist->>'barcode' barcode,schemelist->>'qty' qty,Packingslip.sono from Packingslip 
                LEFT JOIN LATERAL 
                json_array_elements (barcodes) schemelist ON true
                where billid=0 and sono=@sno
                and isactive=true";

                    var paramso = new SortedList<string, object>();
                    paramso.Add("sno", sono);
                    using (var reader = ExecuteReader(pendingpacking, paramso))
                    {
                        while (reader.Read())
                        {
                            string barcode = reader["barcode"].ToString();
                            if (lst.Exists(x => x.uid == barcode))
                            {
                                decimal qty = Convert.ToDecimal(reader["qty"]);
                                lst.Find(x => x.uid == barcode).pendingqty -= qty;
                            }
                        }
                    }
                }
            }
            return lst;
        }
        public List<Salesorder> SearchSOSummary(SalesorderCreteria _reportcreteria)
        {
            List<Salesorder> lst = new List<Salesorder>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                NpgsqlCommand command = null;

                QueryBuilder qb = new QueryBuilder();
                qb.CurrentQuery = SQL_SearchPOSummary;

                if (_reportcreteria.ismobilepo)
                {
                    qb.AddParameter("Salesorder.createdby", "createdby", Context.UserID, "=");
                    qb.AddParameter("Salesorder.ismobilepo", "ismobilepo", true, "=");
                }
                if (_reportcreteria.fromdate != DateTime.MinValue && _reportcreteria.todate != DateTime.MinValue)
                {
                    qb.AddParameter("Salesorder.sodate", "fromdate", _reportcreteria.fromdate.Date, ">=");
                    qb.AddParameter("Salesorder.sodate", "todate", _reportcreteria.todate.Date.AddDays(1), "<");
                }

                if (!string.IsNullOrEmpty(_reportcreteria.sono))
                    qb.AddParameter("sono", _reportcreteria.sono, "=");

                qb.SortField = "Salesorder.id desc";
                command = qb.GetCommand(conn);

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Salesorder oSalesorder = new Salesorder();
                        oSalesorder.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        oSalesorder.tolerance = reader["tolerance"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["tolerance"]);
                        oSalesorder.sodate = reader["sodate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["sodate"]);
                        oSalesorder.expectedate = reader["expectedate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["expectedate"]);
                        oSalesorder.sono = reader["sono"] == DBNull.Value ? "" : reader["sono"].ToString();
                        oSalesorder.customerid = reader["customerid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["customerid"]);
                        oSalesorder.locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                        oSalesorder.deliverylocationid = reader["deliverylocationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["deliverylocationid"]);
                        oSalesorder.termsandcondition_json = reader["termsandcondition"] == DBNull.Value ? "null" : reader["termsandcondition"].ToString();
                        oSalesorder.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        oSalesorder.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        oSalesorder.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        oSalesorder.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        oSalesorder.mode = reader["mode"] == DBNull.Value ? 0 : Convert.ToInt32(reader["mode"]);
                        oSalesorder.status = reader["status"] == DBNull.Value ? Salesorder.POStatus.PENDING : (Salesorder.POStatus)Convert.ToInt32(reader["status"]);
                        oSalesorder.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        oSalesorder.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        oSalesorder.agentid = reader["agentid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["agentid"]);
                        oSalesorder.freight = reader["freight"] == DBNull.Value ? 0 : Convert.ToInt32(reader["freight"]);
                        oSalesorder.ordered = reader["oqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["oqty"]);
                        oSalesorder.received = reader["rqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["rqty"]);
                        oSalesorder.cancelled = reader["cqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["cqty"]);
                        lst.Add(oSalesorder);
                    }
                }
                //if (lst.Count == 1)
                //{
                //    SalesorderitemsDataService subSalesorderitems = new SalesorderitemsDataService() { Context = this.Context };
                //    subSalesorderitems.Context = this.Context;
                //    lst[0].Salesorderitems = subSalesorderitems.SearchAll(_reportcreteria.id);
                //}
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _reportcreteria);
            }
            return lst;
        }

        public List<SalesorderDetails> SearchPendingPO(SalesorderCreteria _reportcreteria)
        {
            List<SalesorderDetails> lst = new List<SalesorderDetails>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                var command = GetCommand(SQL_SearchPendingPO, conn);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        SalesorderDetails oSalesorder = new SalesorderDetails();
                        oSalesorder.tolerance = reader["tolerance"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["tolerance"]);
                        oSalesorder.sodate = reader["sodate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["sodate"]);
                        oSalesorder.expectedate = reader["expectedate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["expectedate"]);
                        oSalesorder.sono = reader["sono"] == DBNull.Value ? "" : reader["sono"].ToString();
                        oSalesorder.customerid = reader["customerid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["customerid"]);
                        oSalesorder.locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                        oSalesorder.deliverylocationid = reader["deliverylocationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["deliverylocationid"]);
                        oSalesorder.mode = reader["mode"] == DBNull.Value ? 0 : Convert.ToInt32(reader["mode"]);
                        oSalesorder.status = reader["status"] == DBNull.Value ? SalesorderDetails.POStatus.PENDING : (SalesorderDetails.POStatus)Convert.ToInt32(reader["status"]);
                        oSalesorder.agentid = reader["agentid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["agentid"]);
                        oSalesorder.freight = reader["freight"] == DBNull.Value ? 0 : Convert.ToInt32(reader["freight"]);
                        oSalesorder.ordered = reader["oqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["oqty"]);
                        oSalesorder.received = reader["rqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["rqty"]);
                        oSalesorder.cancelled = reader["cqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["cqty"]);
                        oSalesorder.sizegroup = reader["sizegroup"] == DBNull.Value ? "" : reader["sizegroup"].ToString();
                        oSalesorder.productname = reader["productname"] == DBNull.Value ? "" : reader["productname"].ToString();
                        oSalesorder.printingname = reader["printingname"] == DBNull.Value ? "" : reader["printingname"].ToString();
                        oSalesorder.brandname = reader["brandname"] == DBNull.Value ? "" : reader["brandname"].ToString();
                        oSalesorder.colourname = reader["colourname"] == DBNull.Value ? "" : reader["colourname"].ToString();
                        oSalesorder.patternname = reader["patternname"] == DBNull.Value ? "" : reader["patternname"].ToString();
                        oSalesorder.sizename = reader["sizename"] == DBNull.Value ? "" : reader["sizename"].ToString();
                        oSalesorder.stylename = reader["stylename"] == DBNull.Value ? "" : reader["stylename"].ToString();
                        oSalesorder.materialname = reader["materialname"] == DBNull.Value ? "" : reader["materialname"].ToString();
                        oSalesorder.typename = reader["typename"] == DBNull.Value ? "" : reader["typename"].ToString();
                        oSalesorder.description = reader["description"] == DBNull.Value ? "" : reader["description"].ToString();
                        oSalesorder.price = reader["price"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["price"]);
                        oSalesorder.priceupto = reader["priceupto"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["priceupto"]);
                        oSalesorder.designid = reader["designid"] == DBNull.Value ? "" : reader["designid"].ToString();
                        oSalesorder.pricerange = reader["pricerange"] == DBNull.Value ? "" : reader["pricerange"].ToString();

                        lst.Add(oSalesorder);

                    }
                }
                conn.Close();
            }

            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _reportcreteria);
            }
            return lst;
        }



        public Salesorder Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Salesorder oSalesorder)
        {

            NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("sodate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oSalesorder.sodate;
            command.Parameters.Add("expectedate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oSalesorder.expectedate;
            command.Parameters.Add("sono", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSalesorder.sono == null ? "" : oSalesorder.sono;
            command.Parameters.Add("sizegroup", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSalesorder.sizegroup == null ? "" : oSalesorder.sizegroup;
            command.Parameters.Add("customerid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSalesorder.customerid;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSalesorder.locationid;
            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSalesorder.companyid;
            command.Parameters.Add("deliverylocationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSalesorder.deliverylocationid;
            command.Parameters.Add("status", NpgsqlTypes.NpgsqlDbType.Integer).Value = (int)oSalesorder.status;
            command.Parameters.Add("tolerance", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oSalesorder.tolerance;
            command.Parameters.Add("salesmenid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSalesorder.salesmenid;
            command.Parameters.Add("customerstateid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSalesorder.customerstateid;
            command.Parameters.Add("sosourcetype", NpgsqlTypes.NpgsqlDbType.Integer).Value = oSalesorder.sosourcetype;
            command.Parameters.Add("sobookdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oSalesorder.sobookdate;
            command.Parameters.Add("sobookqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oSalesorder.sobookqty;
            command.Parameters.Add("sobookno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSalesorder.sobookno == null ? "" : oSalesorder.sobookno;
            command.Parameters.Add("customerpo", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSalesorder.customerpo == null ? "" : oSalesorder.customerpo;
            command.Parameters.Add("soracknumber", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSalesorder.soracknumber == null ? "" : oSalesorder.soracknumber;
            command.Parameters.Add("remarks", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSalesorder.remarks == null ? "" : oSalesorder.remarks;

            command.Parameters.Add("termsandcondition", NpgsqlTypes.NpgsqlDbType.Json).Value = oSalesorder.termsandcondition_json == null ? "null" : oSalesorder.termsandcondition_json;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oSalesorder.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oSalesorder.version;
            command.Parameters.Add("mode", NpgsqlTypes.NpgsqlDbType.Integer).Value = oSalesorder.mode;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("agentid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSalesorder.agentid;
            command.Parameters.Add("freight", NpgsqlTypes.NpgsqlDbType.Integer).Value = oSalesorder.freight;
            command.Parameters.Add("orderformimageid", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSalesorder.orderformimageid == null ? "null" : oSalesorder.orderformimageid;
            command.Parameters.Add("totalqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oSalesorder.totalqty;
            command.Parameters.Add("totalsetqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oSalesorder.totalsetqty;
            command.Parameters.Add("shiptocustomerid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSalesorder.shiptocustomerid;
            command.Parameters.Add("discountpercentage", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oSalesorder.discountpercentage;
            command.Parameters.Add("hasshipto", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oSalesorder.hasshipto;

            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oSalesorder.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            //InboxManager.Instance(true, oLrentry, transaction, conn);
            return oSalesorder;
        }
        public bool UpdatePlan(Salesorder oSalesorder)
        {

            SalesorderitemsDataService subSalesorderitems = new SalesorderitemsDataService() { Context = this.Context };
            subSalesorderitems.Context = this.Context;
            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    string query = "update Salesorderitems set pqty=@pqty where id=@id";
                    var command = GetCommand(query, conn, transaction);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = 0;
                    command.Parameters.Add("pqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = 0;
                    foreach (var item in oSalesorder.Salesorderitems)
                    {
                        command.Parameters["id"].Value = item.id;
                        command.Parameters["pqty"].Value = item.pqty;
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
        public Salesorder Update(Salesorder oSalesorder)
        {


            SalesorderitemsDataService subSalesorderitems = new SalesorderitemsDataService() { Context = this.Context };
            subSalesorderitems.Context = this.Context;
            NpgsqlTransaction transaction = null;
            try
            {
                if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.mfg_mode == 6 &&
                   !string.IsNullOrEmpty(oSalesorder.sobookno))
                {
                    ValidateSOBOOKNODuplicate(oSalesorder);
                }

                if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig != null &&
                    InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig.ContainsKey(oSalesorder.deliverylocationid) &&
                    InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[oSalesorder.deliverylocationid].mfg_sovalidate)
                {
                    if (!ValidateStock(oSalesorder))
                        return oSalesorder;
                }

                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    oSalesorder.totalsetqty = oSalesorder.Salesorderitems.GroupBy(x => new { x.designitemcode, x.sizeid })
                        .Select(y => new { y.Key.designitemcode, y.Key.sizeid, set = y.Average(z => z.oqty) })
                        .Sum(x => x.set);
                    oSalesorder.totalqty = oSalesorder.Salesorderitems.Sum(e => (e.jumpqty == null || e.jumpqty.Count == 0) ? e.oqty : e.jumpqty.Sum(x => x.oqty));

                    oSalesorder.status = Salesorder.POStatus.PENDING;
                    Update(transaction, conn, oSalesorder);
                    oSalesorder.Salesorderitems.ForEach(x => x.salesorderid = oSalesorder.id);
                    subSalesorderitems.Update(transaction, conn, oSalesorder.Salesorderitems, oSalesorder.id);

                    /*with alluid
                    as
                    (
                    select mfgdesign.code,schemelist->>'sizegroup' sizegroup,schemelist->>'uid' uid,cast(schemelist->>'sizeid' as bigint) sizeid,
                    cast(schemelist->>'wsrate' as numeric) wsrate,cast(schemelist->>'dealerprice' as numeric) dealerprice,
                    cast(schemelist->>'salerate' as numeric) salerate,cast(schemelist->>'mrp' as numeric) mrp,
                    schemelist->>'code' dname,cast(schemelist->>'colourid' as bigint) colourid,
                        case when position('/' in schemelist->>'code') = 0 then '' else
                        substring(schemelist->>'code',position('/' in schemelist->>'code')+1)
                        end colourcode
                        from mfgdesign 
                    LEFT JOIN LATERAL
                    json_array_elements (cast(mfgdesign.sizegroup->>'items' as json)) schemelist ON true
                    where isactive=true 
                    )

                    select salesorderitems.id,alluid.dname,salesorderitems.designitemcode,alluid.colourcode,salesorderitems.colourcode,
                    alluid.colourid , salesorderitems.colourid,salesorderitems.designid,alluid.code,
                    salesorderitems.uid from salesorderitems
                        inner join salesorder on salesorder.id = salesorderitems.salesorderid
                        inner join alluid on alluid.uid = salesorderitems.uid
                        where  (alluid.code != salesorderitems.designid or 
                    alluid.dname != salesorderitems.designitemcode or 
                    alluid.colourcode != salesorderitems.colourcode or 
                    alluid.colourid != salesorderitems.colourid or 
                    alluid.sizeid != salesorderitems.sizeid)


                    update salesorderitems set designitemcode = dname, colourcode = alluid.colourcode, designid = code from alluid
                            where alluid.uid = salesorderitems.uid and(alluid.code != salesorderitems.designid or
                            alluid.dname != salesorderitems.designitemcode or
                            alluid.colourcode != salesorderitems.colourcode or
                            alluid.colourid != salesorderitems.colourid or
                            alluid.sizeid != salesorderitems.sizeid)*/

                    if ((InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.mfg_mode != 5 &&
                        InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.mfg_mode != 6))
                    {
                        string sovalidation = string.Format(@"with alluid
as
(
select mfgdesign.code,schemelist->>'sizegroup' sizegroup,schemelist->>'uid' uid,cast(schemelist->>'sizeid' as bigint) sizeid,
cast(schemelist->>'wsrate' as numeric) wsrate,cast(schemelist->>'dealerprice' as numeric) dealerprice,
cast(schemelist->>'salerate' as numeric) salerate,cast(schemelist->>'mrp' as numeric) mrp,
schemelist->>'code' dname,cast(schemelist->>'colourid' as bigint) colourid,
	case when position('/' in schemelist->>'code') = 0 then '' else
	substring(schemelist->>'code',position('/' in schemelist->>'code')+1)
	end colourcode
	from mfgdesign 
LEFT JOIN LATERAL
json_array_elements (cast(mfgdesign.sizegroup->>'items' as json)) schemelist ON true
where isactive=true and mfgdesign.code in ({0})
)

select salesorderitems.id,alluid.dname,salesorderitems.designitemcode,alluid.colourcode,
salesorderitems.colourcode,salesorderitems.uid,alluid.code , salesorderitems.designid,
alluid.colourid , salesorderitems.colourid,alluid.sizeid asizeid, salesorderitems.sizeid ssizeid from salesorder
	inner join salesorderitems on  salesorderitems.salesorderid=salesorder.id
	inner join alluid on alluid.uid = salesorderitems.uid
	where salesorder.id = @id and (alluid.code != salesorderitems.designid or 
alluid.dname != salesorderitems.designitemcode or 
alluid.colourcode != salesorderitems.colourcode or 
alluid.colourid != salesorderitems.colourid or 
alluid.sizeid != salesorderitems.sizeid)
and salesorderitems.isactive=true", string.Join(",", oSalesorder.Salesorderitems.GroupBy(x => x.designid).ToList().ConvertAll(y => "'" + y.Key + "'")));

                        var param = new SortedList<string, object>();
                        param.Add("id", oSalesorder.id);
                        using (var reader = ExecuteReader(sovalidation, param, conn, transaction))
                        {
                            if (reader.Read())
                            {
                                int failedcount = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
                                if (failedcount > 0)
                                {
                                    List<string> msg = new List<string>();
                                    string dname = reader.GetString(1);
                                    string designitemcode = reader.GetString(2);
                                    if (dname != designitemcode)
                                    {
                                        msg.Add("SKU Design " + dname + ", SO Design " + designitemcode);
                                    }
                                    string alluidcode = reader.GetString(6);
                                    string designid = reader.GetString(7);
                                    if (alluidcode != designid)
                                    {
                                        msg.Add("SKU Design " + alluidcode + ", SO Design " + designid);
                                    }
                                    string acolourcode = reader.GetString(3);
                                    string scolourcode = reader.GetString(4);
                                    if (acolourcode != scolourcode)
                                    {
                                        msg.Add("SKU Colour Code " + acolourcode + ", SO Colour Code " + scolourcode);
                                    }

                                    int acolourid = reader[8] == DBNull.Value ? 0 : reader.GetInt32(8);
                                    int scolourid = reader[9] == DBNull.Value ? 0 : reader.GetInt32(9);
                                    if (acolourid != scolourid)
                                    {
                                        msg.Add("SKU Colour ID " + acolourid + ", SO Colour ID " + scolourid);
                                    }

                                    int asizeid = reader[10] == DBNull.Value ? 0 : reader.GetInt32(10);
                                    int ssizeid = reader[11] == DBNull.Value ? 0 : reader.GetInt32(11);
                                    if (asizeid != ssizeid)
                                    {
                                        msg.Add("SKU Size ID " + asizeid + ", SO Size ID " + ssizeid);
                                    }

                                    string uid = reader.GetString(5);
                                    string info = string.Format("{0}  - {1}", uid, string.Join("<br>", msg));
                                    reader.Close();
                                    transaction.Rollback();
                                    WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(new Exception("SO Validation failed (" + info + ")," +
                                        "please contact support team"), WebAPI.Common.Logger.LogType.Error, oSalesorder);
                                }
                            }
                        }
                    }

                    //if (Services.ConfigManager.Instance(Context.OrganizationID).Config.intray_notification_Salesorder)
                    //{
                    //    SupplierDataService supplierdata = new SupplierDataService() { Context = this.Context };
                    //    var supplier = supplierdata.Search(new SupplierCreteria() { id = oSalesorder.customerid });
                    //    if (supplier != null && supplier.Count > 0)
                    //    {
                    //        Services.IntrayManager.Instance.Add(Services.IntrayManager.EntryType.Salesorder,
                    //            oSalesorder.id, string.Format("PURCHASE ORDER MODIFIED - FOR THE SUPPLIER {0} WITH {1} QTY.", supplier[0].name,
                    //            oSalesorder.Salesorderitems.Sum(x => x.oqty)), Context.UserID, oSalesorder.locationid, transaction);
                    //    }
                    //}

                    if (Services.ConfigManager.Instance(Context.OrganizationID).Config.intray_salesorder)
                    {
                        CustomerDataService supplierdata = new CustomerDataService() { Context = this.Context };
                        var supplier = supplierdata.Search(new CustomerCreteria() { id = oSalesorder.customerid });
                        if (supplier != null && supplier.Count > 0)
                        {
                            Services.IntrayManager.Instance(Context.OrganizationID).Add(Context.OrganizationID, Services.IntrayManager.EntryType.SalesOrder,
                                oSalesorder.id, string.Format("Sales ORDER Modified - FOR THE Customer {0} WITH {1} QTY.", supplier[0].name,
                                oSalesorder.Salesorderitems.Sum(x => x.oqty)), Context.UserID, oSalesorder.locationid, transaction, this.Context);
                        }
                    }

                    var paramso = new SortedList<string, object>();
                    paramso.Add("salesorderid", oSalesorder.id);
                    decimal orderqty = 0;
                    using (var reader = ExecuteReader("select sum(oqty) from salesorderitems where salesorderid=@salesorderid and isactive=true", paramso, conn, transaction))
                    {
                        if (reader.Read())
                        {
                            orderqty = reader[0] == DBNull.Value ? 0 : Convert.ToDecimal(reader[0]);
                        }
                    }
                    if (orderqty != oSalesorder.totalqty)
                    {
                        transaction.Rollback();
                        WebAPI.Common.Logger.Instance(Context.OrganizationID).PerformanceLog("Salesorder qty mismatches", WebAPI.Common.Logger.LogType.Info,
                        "salesorderdataservice/update", oSalesorder, Context.UserID, oSalesorder.deliverylocationid);
                        throw new Exception("Saved qty " + oSalesorder.totalqty + ", avilable qty " + orderqty + " check the order detail before save");
                    }

                    transaction.Commit();
                    conn.Close();
                }

                oSalesorder.Salesorderitems = subSalesorderitems.Search(new SalesorderitemsCreteria()
                {
                    salesorderid = oSalesorder.id
                });
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oSalesorder);
            }

            return oSalesorder;
        }

        public Salesorder Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Salesorder oSalesorder)
        {

            NpgsqlCommand command = GetCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSalesorder.id;
            command.Parameters.Add("sodate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oSalesorder.sodate;
            command.Parameters.Add("expectedate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oSalesorder.expectedate;
            command.Parameters.Add("sizegroup", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSalesorder.sizegroup == null ? "" : oSalesorder.sizegroup;
            command.Parameters.Add("sono", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSalesorder.sono == null ? "" : oSalesorder.sono;
            command.Parameters.Add("customerid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSalesorder.customerid;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSalesorder.locationid;
            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSalesorder.companyid;
            command.Parameters.Add("deliverylocationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSalesorder.deliverylocationid;
            command.Parameters.Add("status", NpgsqlTypes.NpgsqlDbType.Integer).Value = (int)oSalesorder.status;
            command.Parameters.Add("tolerance", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oSalesorder.tolerance;
            command.Parameters.Add("salesmenid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSalesorder.salesmenid;
            command.Parameters.Add("sosourcetype", NpgsqlTypes.NpgsqlDbType.Integer).Value = oSalesorder.sosourcetype;
            command.Parameters.Add("sobookdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oSalesorder.sobookdate;
            command.Parameters.Add("sobookno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSalesorder.sobookno == null ? "" : oSalesorder.sobookno;
            command.Parameters.Add("customerpo", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSalesorder.customerpo == null ? "" : oSalesorder.customerpo;
            command.Parameters.Add("sobookqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oSalesorder.sobookqty;
            command.Parameters.Add("termsandcondition", NpgsqlTypes.NpgsqlDbType.Json).Value = oSalesorder.termsandcondition_json == null ? "null" : oSalesorder.termsandcondition_json;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oSalesorder.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSalesorder.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("mode", NpgsqlTypes.NpgsqlDbType.Integer).Value = oSalesorder.mode;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oSalesorder.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oSalesorder.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("agentid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSalesorder.agentid;
            command.Parameters.Add("freight", NpgsqlTypes.NpgsqlDbType.Integer).Value = oSalesorder.freight;
            command.Parameters.Add("totalqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oSalesorder.totalqty;
            command.Parameters.Add("totalsetqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oSalesorder.totalsetqty;
            command.Parameters.Add("orderformimageid", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSalesorder.orderformimageid == null ? "null" : oSalesorder.orderformimageid;
            command.Parameters.Add("remarks", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSalesorder.remarks == null ? "" : oSalesorder.remarks;
            command.Parameters.Add("shiptocustomerid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSalesorder.shiptocustomerid;
            command.Parameters.Add("discountpercentage", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oSalesorder.discountpercentage;
            command.Parameters.Add("hasshipto", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oSalesorder.hasshipto;
            command.Parameters.Add("soracknumber", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSalesorder.soracknumber == null ? "" : oSalesorder.soracknumber;

            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oSalesorder.version = Convert.ToInt32(reader[0].ToString());
                }
            }

            return oSalesorder;
        }

        internal bool UpdateDesingImage(long Salesorderitemid, string imageid)
        {
            var param = new SortedList<string, object>();
            param.Add("id", Salesorderitemid);
            param.Add("imageid", imageid);
            string query = "update Salesorderitems set imageid=@imageid where id=@id";
            return ExecuteQuery(query, param) == 1;
        }

        internal bool SaveRackNumber(Salesorder item)
        {
            var param = new SortedList<string, object>();
            param.Add("id", item.id);
            param.Add("soracknumber", item.soracknumber);
            string query = "update Salesorderitems set soracknumber=@soracknumber where id=@id";
            return ExecuteQuery(query, param) == 1;
        }
        public bool Delete(Salesorder oSalesorder)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oSalesorder);
                    SalesorderitemsDataService subSalesorderitems = new SalesorderitemsDataService() { Context = this.Context };
                    subSalesorderitems.Context = this.Context;
                    subSalesorderitems.DeleteAll(oSalesorder.id, oSalesorder.modifiedby, conn, transaction);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oSalesorder);
            }
            return hasSuccessCount;

        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Salesorder oSalesorder)
        {

            NpgsqlCommand command = GetCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oSalesorder.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oSalesorder.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSalesorder.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }
        public Salesorder UpdateStatus(NpgsqlTransaction transaction, NpgsqlConnection conn, Salesorder oSalesorder)
        {
            NpgsqlCommand command = GetCommand(SQL_UpdateStatus, conn, transaction);
            command.Parameters.Add("status", NpgsqlTypes.NpgsqlDbType.Integer).Value = (int)oSalesorder.status;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oSalesorder.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSalesorder.id;
            int successCount = command.ExecuteNonQuery();

            return oSalesorder;
        }
        public Salesorder UpdateStatus(Salesorder oSalesorder)
        {
            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    UpdateStatus(transaction, conn, oSalesorder);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oSalesorder);
            }

            return oSalesorder;
        }

        public bool AssignPO(SalesorderCreteria oSalesorder)
        {
            int count = 0;
            var param = new SortedList<string, object>();
            param.Add("customerpo", oSalesorder.customerpo);
            string query = string.Format("Update salesorder set customerpo=@customerpo,sobookno=@customerpo where id in ({0})", string.Join(",", oSalesorder.ids));
            return ExecuteQuery(query, param) > 0;
        }

        public SalesorderPrint Print(SalesorderCreteria _Salesorder)
        {
            List<Salesorder> Salesorder = null;
            if ((!string.IsNullOrEmpty(_Salesorder.sono) &&
                (_Salesorder.id == 0 || _Salesorder.sono.StartsWith("~"))))
            {
                Salesorder = Search(new SalesorderCreteria()
                {
                    sono = _Salesorder.sono.StartsWith("~") ? _Salesorder.sono.Substring(1) : _Salesorder.sono
                });
            }
            else if (_Salesorder.id > 0)
            {
                Salesorder = Search(new SalesorderCreteria()
                {
                    id = _Salesorder.id
                });
            }
            else if (_Salesorder.ids != null && _Salesorder.ids.Count> 0)
            {
                Salesorder = Search(new SalesorderCreteria()
                {
                    id = _Salesorder.ids[0]
                });

                SalesorderitemsDataService subSalesorderitems = new SalesorderitemsDataService() { Context = this.Context };
                subSalesorderitems.Context = this.Context;
                for(var i=1;i< _Salesorder.ids.Count;i++)
                {
                    Salesorder[0].Salesorderitems.AddRange(
                     subSalesorderitems.Search(new SalesorderitemsCreteria()
                     {
                         salesorderid = _Salesorder.ids[i]
                     }));
                }

                if(Salesorder[0].Salesorderitems.Any(x=>!string.IsNullOrEmpty(x.uid)))
                {
                    var newlist = Salesorder[0].Salesorderitems.GroupBy(x => x.uid).ToList();
                    Salesorder[0].Salesorderitems.Clear();
                    MfgdesignDataService mfgdesignDataService = new MfgdesignDataService();
                    var alldesigns = mfgdesignDataService.Search(new MfgdesignCreteria()
                    {
                        codes = Salesorder[0].Salesorderitems.GroupBy(x => x.designid).ToList().ConvertAll(x => x.Key).ToList()
                    });
                    newlist.ForEach(x =>
                    {
                        var soitem = x.First();
                        soitem.oqty = x.Sum(y => y.oqty);
                        if(alldesigns.Exists(y=>y.code == soitem.designid))
                        {
                            soitem.rackno = alldesigns.Find(y => y.code == soitem.designid).racknumber;
                        }
                        Salesorder[0].Salesorderitems.Add(soitem);
                    });
                    
                }
            }
            else
            {
                return new SalesorderPrint();
            }

            SalesorderPrint ret = new SalesorderPrint();
            ret.id = Salesorder[0].id;
            ret.salesmenid = Salesorder[0].salesmenid;
            ret.remarks = Salesorder[0].remarks;
            ret.agentid = Salesorder[0].agentid;
            ret.tolerance = Salesorder[0].tolerance;
            ret.companyid = Salesorder[0].companyid;
            ret.locationid = Salesorder[0].locationid;
            ret.deliverylocationid = Salesorder[0].deliverylocationid;
            ret.sodate = Salesorder[0].sodate;
            ret.sono = Salesorder[0].sono;
            var box = Salesorder[0].Salesorderitems.GroupBy(x => x.addnlchargename.Trim()).ToList().ConvertAll(y => y.Key);
            box.RemoveAll(xx => string.IsNullOrEmpty(xx));
            ret.box = string.Join(",", box);
            ret.sobookdate = Salesorder[0].sobookdate;
            ret.sobookno = Salesorder[0].sobookno;
            ret.customerpo = Salesorder[0].customerpo;
            ret.sobookqty = Salesorder[0].sobookqty;
            ret.expectedate = Salesorder[0].expectedate;
            ret.customerid = Salesorder[0].customerid;
            ret.termsandcondition = Salesorder[0].termsandcondition;
            ret.mode = Salesorder[0].mode;
            ret.status = Salesorder[0].status;
            ret.Salesorderitems = Salesorder[0].Salesorderitems;
            ret.freight = Salesorder[0].freight;
            ret.totalqty = Salesorder[0].totalqty;
            ret.totalsetqty = Salesorder[0].totalsetqty;
            ret.agentid = Salesorder[0].agentid;
            ret.createdby = Salesorder[0].createdby;
            ret.createdon = Salesorder[0].createdon;
            ret.modifiedby = Salesorder[0].modifiedby;
            ret.modifiedon = Salesorder[0].modifiedon;
            ret.actionby = Salesorder[0].actionby;
            ret.actionon = Salesorder[0].actionon;
            ret.totalqty = Salesorder[0].Salesorderitems.Sum(e => e.oqty);
            ret.singletaxpercentage = "";
            if (ret.Salesorderitems.ConvertAll(x => x.taxpercentage).Distinct().ToList().Count == 1)
            {
                ret.singletaxpercentage = ret.Salesorderitems[0].taxpercentage.ToString("F0") + "%";
            }
            Data.CompanyDataService companyservice = new CompanyDataService() { Context = this.Context };
            companyservice.Context = this.Context;
            var comnanylist = companyservice.Search(new CompanyCreteria()
            {
                id = ret.companyid
            });

            ret.company = comnanylist[0];
            ret.company.parseaddress();
            Data.CustomerDataService _supplierdata = new CustomerDataService() { Context = this.Context };
            var customer = _supplierdata.Search(new CustomerCreteria()
            {
                id = ret.customerid
            });


            Data.ReferencelistDataService _refdata = new ReferencelistDataService() { Context = this.Context };
            if (ret.deliverylocationid > 0)
            {
                var _deliverylocation = _refdata.Search(new ReferencelistCreteria()
                {
                    id = ret.deliverylocationid
                });

                if (_deliverylocation != null && _deliverylocation.Count > 0 && _deliverylocation[0].additionalinfo != null)
                {
                    ret.deliverylocation = new CompanyLocation(_deliverylocation[0]);
                    if (ret.deliverylocation.stateid > 0)
                    {
                        var states = _refdata.SearchList(new ReferencelistCreteria()
                        {
                            id = ret.deliverylocation.stateid
                        });
                        if (states != null && states.Count > 0)
                        {
                            ret.deliverylocation.statename = states[0].name;
                            ret.deliverylocation.statecode = states[0].code;
                        }
                    }
                    if (ret.deliverylocation.cityid > 0)
                    {
                        var states = _refdata.SearchList(new ReferencelistCreteria()
                        {
                            id = ret.deliverylocation.cityid
                        });
                        if (states != null && states.Count > 0)
                        {
                            ret.deliverylocation.cityname = states[0].name;
                        }
                    }
                }
            }

            if (customer != null && customer.Count > 0)
            {
                ret.customer = customer[0];
                if (Salesorder[0].hasshipto && Salesorder[0].shiptocustomerid > 0)
                {
                    customer = _supplierdata.Search(new CustomerCreteria()
                    {
                        id = Salesorder[0].shiptocustomerid
                    });
                    ret.shiptocustomer = customer[0];
                }
                else
                {
                    ret.shiptocustomer = customer[0];
                }

                if (ret.shiptocustomer != null)
                {
                    if (!string.IsNullOrEmpty(ret.shiptocustomer.gstno) &&
                        !string.IsNullOrEmpty(ret.company.gstno))
                    {
                        ret.isinterstate = ret.shiptocustomer.gstno.Substring(0, 2) != ret.company.gstno.Substring(0, 2);
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(ret.customer.gstno) &&
                        !string.IsNullOrEmpty(ret.company.gstno))
                    {
                        ret.isinterstate = ret.customer.gstno.Substring(0, 2) != ret.company.gstno.Substring(0, 2);
                    }
                }

                if (string.IsNullOrEmpty(ret.customer.deliveryaddress))
                    ret.customer.deliveryaddress = ret.customer.address;

                if (string.IsNullOrEmpty(ret.shiptocustomer.deliveryaddress))
                    ret.shiptocustomer.deliveryaddress = ret.shiptocustomer.address;

                if (ret.agentid > 0)
                {
                    AgentDataService _agentdata = new AgentDataService() { Context = this.Context };
                    var agent = _agentdata.Search(new AgentCreteria()
                    {
                        id = ret.agentid
                    });
                    ret.agent = agent[0];
                }
                else
                {
                    ret.agent = new Agent() { name = "DIRECT" };
                }

                if (ret.salesmenid > 0)
                {
                    EmployeeDataService _agentdata = new EmployeeDataService() { Context = this.Context };
                    var agent = _agentdata.Search(new EmployeeCreteria()
                    {
                        id = ret.salesmenid,
                        hidesalaryinfo = true
                    });
                    ret.salesmen = agent[0];
                }

                if (ret.shiptocustomer != null && ret.shiptocustomer.preferredtransportid > 0)
                {
                    TransportDataService transportDataService = new TransportDataService() { Context = this.Context };
                    var transport = transportDataService.Search(new TransportCreteria()
                    {
                        id = ret.shiptocustomer.preferredtransportid
                    });
                    ret.transport = transport[0];
                }

                else if (ret.customer.preferredtransportid > 0)
                {

                    TransportDataService transportDataService = new TransportDataService() { Context = this.Context };
                    var transport = transportDataService.Search(new TransportCreteria()
                    {
                        id = ret.customer.preferredtransportid
                    });
                    ret.transport = transport[0];


                }
                else
                {
                    ret.transport = new Transport() { name = "DIRECT" };
                }

            }

            UsersDataService usersDataService = new UsersDataService() { Context = this.Context };
            ret.preparedby = usersDataService.Search(new UsersCreteria()
            {
                id = ret.createdby
            }).First().title;

            List<long> userids = new List<long>();
            userids.Add(ret.createdby);
            userids.Add(ret.actionby);
            Data.EmployeeDataService _empdata = new EmployeeDataService() { Context = this.Context };
            var emplist = _empdata.GetUserName(userids);
            if (emplist != null && emplist.Count > 0)
            {
                if (emplist.Exists(e => e.id == ret.createdby)) ret.whocreated = emplist.Find(e => e.id == ret.createdby).name;
                if (emplist.Exists(e => e.id == ret.actionby)) ret.whoapproved = emplist.Find(e => e.id == ret.actionby).name;
            }

            if (_Salesorder.unpacked)
            {
                string query = @"with packedlist as
                                (
	                                select  schemelist->>'barcode' barcode,
	                                (cast(schemelist->>'qty' as numeric)) packedqty from packingslip
	                                LEFT JOIN LATERAL
	                                json_array_elements (cast(packingslip.barcodes as json)) schemelist ON true
	                                where sono=@sono
	                                and packingslip.isactive=true
	                                and packingslip.billid=0
                                    and packingslip.billno=''
                                ),soitems as
                                (
                                select sum(oqty-(bqty+rqty+cqty)) soqty,uid from salesorder
	                                inner join salesorderitems on salesorderitems.salesorderid = salesorder.id
	                                where salesorder.sono = @sono
	                                and salesorderitems.isactive=true
	                                group by uid
                                )
                                ,grpbarcodes as
                                (
                                select barcode,sum(packedqty) packqty from packedlist
                                group by barcode
                                )
                                select soitems.uid,packqty,soqty from soitems
                                left outer join grpbarcodes on grpbarcodes.barcode = soitems.uid";


                SortedDictionary<string, decimal> packedqty = new SortedDictionary<string, decimal>();
                var param = new SortedList<string, object>();
                param.Add("sono", ret.sono);
                var groupedorders = ret.Salesorderitems.GroupBy(x => x.uid).ToList();
                ret.Salesorderitems = new List<SalesorderitemsDetail>();
                using (var reader = ExecuteReader(query, param))
                {
                    while (reader.Read())
                    {
                        decimal packqty = reader["packqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["packqty"]);
                        decimal soqty = reader["soqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["soqty"]);
                        if (soqty > packqty)
                        {
                            string uid = reader["uid"].ToString();
                            if (groupedorders.Exists(x => x.Key == uid))
                            {
                                var soorditem = groupedorders.Find(x => x.Key == uid).First();
                                soorditem.oqty = soqty - packqty;
                                ret.Salesorderitems.Add(soorditem);
                            }
                        }
                    }
                }
            }

            ret.Salesorderitems.ForEach(x =>
            {
                if (x.taxpercentage > 0 && x.tax == 0)
                {
                    x.tax = (x.oqty * x.price) * x.taxpercentage / 100;
                }
            });
            if (ret.Tax > 0)
            {
                ret.stax = ret.Tax / 2;
                ret.itax = ret.Tax;
                ret.ctax = ret.Tax / 2;
                if (ret.Salesorderitems.ConvertAll(x => x.taxpercentage).Distinct().ToList().Count == 1 &&
                    ret.Salesorderitems[0].taxpercentage > 0)
                {
                    ret.itaxpercentage = ret.Salesorderitems[0].taxpercentage;
                    ret.ctaxpercentage = ret.Salesorderitems[0].taxpercentage / 2;
                    ret.staxpercentage = ret.Salesorderitems[0].taxpercentage / 2;
                }
            }
            return ret;
        }

        public MFG_SizeGroup<MFG_SizeGroup_Stock> SearchSizeGroupStock(SalesorderitemsCreteria _reportcreteria)
        {
            var report = new MFG_SizeGroup<MFG_SizeGroup_Stock>();
            List<MFG_SizeGroup_Stock> lst = new List<MFG_SizeGroup_Stock>();
            report.items = lst;
            NpgsqlConnection conn = CreateConnection();
            try
            {
                if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_possizelist != null &&
                    InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_possizelist != "")
                {
                    report.sizenames = new List<string>(InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_possizelist.Split(','));
                }
                conn.Open();
                NpgsqlCommand command = null;

                QueryBuilder qb = new QueryBuilder();

                ReferencelistDataService referencelistDataService = new ReferencelistDataService() { Context = this.Context };
                var sizegroups = referencelistDataService.SearchSizeGroups();
                qb.CurrentQuery = @"select items.productid,stock.designid,items.colourid, colours.name colourname,products.name productname,
                                    lrinvoiceitems.itemdetail->> 'sizegroup' sizegroup,
                                    sum(salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty)) stockqty,
                                    sum(salablegoods.iqty) inwardqty,
                                    sum(salablegoods.oqty) salesqty,
                                    sum(salablegoods.dqty) damageqty,
                                    sum(salablegoods.jqty) journalqty
									from salablegoods
                                    inner join stock on stock.id = salablegoods.stockid
                                    left outer join lrinvoiceitems on lrinvoiceitems.id = invoiceitemid
                                    inner join items on items.id = stock.itemid
									inner join products on products.id = items.productid
									inner join referencelist colours on colours.id = items.colourid
                                    where salablegoods.hasstock = true
									group by items.productid,stock.designid,items.colourid,lrinvoiceitems.itemdetail->> 'sizegroup',
									colours.name ,products.name";

                command = qb.GetCommand(conn);
                if (Services.ConfigManager.Instance(Context.OrganizationID).Config.analyser_stock_removeaqty)
                    command.CommandText = command.CommandText.Replace("salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty)", "salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty+salablegoods.aqty)");

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        MFG_SizeGroup_Stock oitem = new MFG_SizeGroup_Stock();
                        oitem.designid = reader["designid"] == DBNull.Value ? "" : reader["designid"].ToString();
                        oitem.colourname = reader["colourname"] == DBNull.Value ? "" : reader["colourname"].ToString();
                        oitem.productname = reader["productname"] == DBNull.Value ? "" : reader["productname"].ToString();
                        oitem.sizegroup = reader["sizegroup"] == DBNull.Value ? "" : reader["sizegroup"].ToString();
                        oitem.stockqty = reader["stockqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["stockqty"]);
                        oitem.inwardqty = reader["inwardqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["inwardqty"]);
                        oitem.salesqty = reader["salesqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["salesqty"]);
                        oitem.damageqty = reader["damageqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["damageqty"]);
                        if (!sizegroups.Exists(e => e.name == oitem.sizegroup))
                        {
                            var newsizegroup = new ReferenceSizeGroup()
                            {
                                id = 0,
                                name = oitem.sizegroup,
                                list = new List<ReferenceItem>()
                            };
                            newsizegroup.list.Add(new ReferenceItem());
                            sizegroups.Add(newsizegroup);
                        }
                        var sizegroup = sizegroups.Find(e => e.name == oitem.sizegroup);

                        var setqty = sizegroup.list.Count;
                        if (setqty > 0)
                        {
                            oitem.setstock = oitem.stockqty / setqty;
                            if (oitem.inwardqty > 0)
                                oitem.inwardqty = oitem.inwardqty / setqty;
                            if (oitem.salesqty > 0)
                                oitem.salesqty = oitem.salesqty / setqty;
                            if (oitem.damageqty > 0)
                                oitem.damageqty = oitem.damageqty / setqty;
                        }
                        oitem.patchqty = setqty;
                        //if (sizegroups.Exists(e => e.name == sizegroups))
                        //{
                        //    count = sizegroups.Find(e => e.name == sizegroup).list.Count;
                        //}
                        //oitem.setstock = stockqty / count;
                        lst.Add(oitem);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _reportcreteria);
            }
            return report;
        }

        internal bool ChangeStatus(ChangeAttributes item)
        {

            try
            {
                if (item.Status == ChangeAttributes.SOStatus.Cancelled ||
                    item.Status == ChangeAttributes.SOStatus.Completed)
                {
                    List<SalesorderitemsDetail> changeditems = new List<SalesorderitemsDetail>();
                    foreach (var id in item.ids)
                    {
                        var salesorder = Search(new SalesorderCreteria()
                        {
                            id = id
                        }).First();
                        changeditems.AddRange(salesorder.Salesorderitems.FindAll(x => x.pendingqty > 0));
                    }
                    if (item.Status == ChangeAttributes.SOStatus.Completed)
                        changeditems.ForEach(x => x.rqty = x.pendingqty);
                    else
                        changeditems.ForEach(x => x.cqty = x.pendingqty);

                    using (NpgsqlConnection conn = CreateConnection())
                    {
                        conn.Open();
                        var transaction = conn.BeginTransaction();
                        string query = "update Salesorderitems set cqty=@cqty,rqty=@rqty where id=@id";
                        var command = GetCommand(query, conn, transaction);
                        command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = 0;
                        command.Parameters.Add("cqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = 0;
                        command.Parameters.Add("rqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = 0;
                        foreach (var citem in changeditems)
                        {
                            command.Parameters["id"].Value = citem.id;
                            command.Parameters["cqty"].Value = citem.cqty;
                            command.Parameters["rqty"].Value = citem.rqty;
                            command.ExecuteNonQuery();
                        }

                        query = "update salesorder set status=@statusid where id=@id";
                        command = GetCommand(query, conn, transaction);
                        command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = 0;
                        command.Parameters.Add("statusid", NpgsqlTypes.NpgsqlDbType.Integer).Value = item.Status;
                        foreach (var id in item.ids)
                        {
                            command.Parameters["id"].Value = id;
                            command.ExecuteNonQuery();
                        }
                        transaction.Commit();
                        conn.Close();
                    }
                }
                else
                {
                    List<SalesorderitemsDetail> changeditems = new List<SalesorderitemsDetail>();
                    foreach (var id in item.ids)
                    {
                        var salesorder = Search(new SalesorderCreteria()
                        {
                            id = id
                        }).First();
                        if (item.Status == ChangeAttributes.SOStatus.Pending ||
                            item.Status == ChangeAttributes.SOStatus.Approved)
                        {
                            if (salesorder.status == Salesorder.POStatus.COMPLETED)
                            {
                                var completed = salesorder.Salesorderitems.FindAll(x => x.rqty > 0);
                                if (completed.Count > 0)
                                {
                                    completed.ForEach(x => x.rqty = 0);
                                    changeditems.AddRange(completed);
                                }
                            }
                            else if (salesorder.status == Salesorder.POStatus.CANCELLED)
                            {
                                var completed = salesorder.Salesorderitems.FindAll(x => x.cqty > 0);
                                if (completed.Count > 0)
                                {
                                    completed.ForEach(x => x.cqty = 0);
                                    changeditems.AddRange(completed);
                                }
                            }
                        }
                    }

                    using (NpgsqlConnection conn = CreateConnection())
                    {
                        conn.Open();
                        var transaction = conn.BeginTransaction();
                        string query = "update Salesorderitems set cqty=@cqty,rqty=@rqty where id=@id";
                        var command = GetCommand(query, conn, transaction);
                        command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = 0;
                        command.Parameters.Add("cqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = 0;
                        command.Parameters.Add("rqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = 0;
                        foreach (var citem in changeditems)
                        {
                            command.Parameters["id"].Value = citem.id;
                            command.Parameters["cqty"].Value = citem.cqty;
                            command.Parameters["rqty"].Value = citem.rqty;
                            command.ExecuteNonQuery();
                        }

                        if (item.statusid == 1)
                        {
                            query = @"update salesorder set status=@statusid,statusby=@statusby,
                                    statuson=@statuson,approvedby=@approvedby,approvedon=@approvedon where id=@id";
                            command = GetCommand(query, conn, transaction);
                            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = 0;
                            command.Parameters.Add("statusby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.approvedby;
                            command.Parameters.Add("approvedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.approvedby;
                            command.Parameters.Add("approvedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = item.statuson;
                            command.Parameters.Add("statuson", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = item.statuson;
                            command.Parameters.Add("statusid", NpgsqlTypes.NpgsqlDbType.Integer).Value = item.statusid;
                            foreach (var id in item.ids)
                            {
                                command.Parameters["id"].Value = id;
                                command.ExecuteNonQuery();
                            }
                        }
                        else
                        {
                            query = "update salesorder set status=@statusid,statusby=@statusby,statuson=@statuson where id=@id";
                            command = GetCommand(query, conn, transaction);
                            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = 0;
                            command.Parameters.Add("statusid", NpgsqlTypes.NpgsqlDbType.Integer).Value = item.statusid;
                            command.Parameters.Add("statusby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.approvedby;
                            command.Parameters.Add("statuson", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = item.statuson;
                            foreach (var id in item.ids)
                            {
                                command.Parameters["id"].Value = id;
                                command.ExecuteNonQuery();
                            }
                        }
                        transaction.Commit();
                        conn.Close();
                    }
                }
                //using (NpgsqlConnection conn = CreateConnection())
                //{
                //    conn.Open();
                //    NpgsqlCommand command = GetCommand("update salesorder set status=@status,modifiedby=@modifiedby,modifiedon=@modifiedon where id in (" + string.Join(",", item.ids) + ")", conn);
                //    command.Parameters.Add("status", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.statusid;
                //    command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                //    command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
                //    int successCount = command.ExecuteNonQuery();
                //    conn.Close();
                //    return successCount > 0;
                //}
                return true;
            }
            catch (Exception exp)
            {
                return false;
            }
        }

        public string UpdateQueryForHyper(string query)
        {
            if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_hyper_uan)
                return query.Replace("stock.serialno", "stock.barcode");
            return query;
        }

        internal List<SalesOrderHistory> SaveUANSO(SOUANUploaded pouploaditem)
        {
            List<SalesOrderHistory> history = new List<SalesOrderHistory>();
            NpgsqlTransaction transaction = null;
            try
            {
                List<string> alluid = new List<string>();
                foreach (var list in pouploaditem.orders.ConvertAll(x => x.items.ConvertAll(y => y.UAN)))
                    alluid.AddRange(list);
                alluid = alluid.Distinct().ToList();
                MfgdesignDataService mfgdesignDataService = new MfgdesignDataService() { Context = this.Context };
                var uidlist = mfgdesignDataService.SearchUANItem(alluid);
                ReferencelistDataService referencelistDataService = new ReferencelistDataService() { Context = this.Context };
                var colourcodes = referencelistDataService.Search(new ReferencelistCreteria()
                {
                    ids = uidlist.ConvertAll(x => x.colourid).Distinct().ToList()
                });
                foreach (var colourcode in colourcodes)
                {
                    uidlist.Where(x => x.colourid == colourcode.id).ToList().ForEach(y => y.colourcode = colourcode.code);
                }
                CustomerDataService supplierdata = new CustomerDataService() { Context = this.Context };
                SalesorderitemsDataService subPurchaseorderitems = new SalesorderitemsDataService() { Context = this.Context };
                CompanyDataService companyDataService = new CompanyDataService() { Context = this.Context };
                var allcompany = companyDataService.Search(new CompanyCreteria()
                {

                });


                subPurchaseorderitems.Context = this.Context;
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    foreach (var orderitem in pouploaditem.orders)
                    {
                        var company = allcompany[0];
                        if (allcompany.Exists(x => x.name.ToLower() == orderitem.Company.ToLower()))
                        {
                            company = allcompany.Find(x => x.name.ToLower() == orderitem.Company.ToLower());
                        }

                        var sequence = SequenceManager.Instance(Context.OrganizationID).Get("salesorder.sono",
                            InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_supportlocationwisesopackingno ? orderitem.locationid : 0,
                            InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_supportcompanywiseentryno ?
                        company.id : 0);

                        Salesorder oSalesOrder = new Salesorder();
                        oSalesOrder.companyid = company.id;
                        oSalesOrder.deliverylocationid = orderitem.locationid;
                        oSalesOrder.locationid = orderitem.locationid;
                        oSalesOrder.customerid = orderitem.customerid;
                        oSalesOrder.sobookno = orderitem.BookingNo;
                        oSalesOrder.sobookdate = DateTime.Now;
                        oSalesOrder.customerpo = orderitem.OrderSplit;
                        oSalesOrder.tolerance = orderitem.Tolerance;
                        oSalesOrder.expectedate = orderitem.DueDate;
                        oSalesOrder.sodate = orderitem.SODate;
                        oSalesOrder.remarks = orderitem.Remark;
                        oSalesOrder.mode = 5;
                        oSalesOrder.version = 1;
                        oSalesOrder.isactive = true;
                        switch (orderitem.OrderType.ToLower())
                        {
                            case "trade":
                                oSalesOrder.sosourcetype = 0;
                                break;
                            case "nontrade":
                                oSalesOrder.sosourcetype = 1;
                                break;
                            case "return":
                                oSalesOrder.sosourcetype = 2;
                                break;
                        }
                        oSalesOrder.Salesorderitems = new List<SalesorderitemsDetail>();
                        var supplier = supplierdata.Search(new CustomerCreteria() { id = orderitem.customerid });
                        oSalesOrder.agentid = supplier[0].agentid;
                        if (!string.IsNullOrEmpty(supplier.First().gstno) &&
                            !string.IsNullOrEmpty(company.gstno))
                        {
                            oSalesOrder.isinterstate = supplier.First().gstno.Substring(0, 2) != company.gstno.Substring(0, 2);
                        }

                        foreach (var oitem in orderitem.items)
                        {
                            var MfgItem = uidlist.Find(x => x.uid == oitem.UAN);
                            if (MfgItem == null) continue;
                            SalesorderitemsDetail oSalesorderitemsDetail = new SalesorderitemsDetail();
                            oSalesorderitemsDetail.uid = oitem.UAN;
                            oSalesorderitemsDetail.sno = oSalesOrder.Salesorderitems.Count() + 1;
                            oSalesorderitemsDetail.productname = oitem.Product;
                            oSalesorderitemsDetail.printingname = MfgItem.pname;
                            oSalesorderitemsDetail.size = oitem.Size;
                            oSalesorderitemsDetail.sizename = oitem.Size;
                            oSalesorderitemsDetail.sizeid = oitem.SizeID;
                            oSalesorderitemsDetail.colourid = oitem.ColourID;
                            oSalesorderitemsDetail.colourcode = MfgItem.colourcode;
                            oSalesorderitemsDetail.designuid = MfgItem.id;
                            oSalesorderitemsDetail.designid = MfgItem.code;
                            oSalesorderitemsDetail.colourshade = MfgItem.colourcode;
                            oSalesorderitemsDetail.designitemcode = MfgItem.designitemcode;
                            oSalesorderitemsDetail.typeid = MfgItem.typeid;
                            oSalesorderitemsDetail.brandid = MfgItem.brandid;
                            oSalesorderitemsDetail.mrp = MfgItem.mrp;
                            oSalesorderitemsDetail.price = oitem.Price;
                            oSalesorderitemsDetail.pricerange = oitem.Price.ToString("F2");
                            oSalesorderitemsDetail.priceranges = oitem.Price.ToString("F2");
                            oSalesorderitemsDetail.taxid = oitem.TaxID;
                            oSalesorderitemsDetail.tax = oitem.Tax;
                            oSalesorderitemsDetail.taxpercentage = oitem.Percentage;
                            oSalesorderitemsDetail.productid = oitem.ProductID;
                            oSalesorderitemsDetail.itemid = oitem.ItemID;
                            oSalesorderitemsDetail.designid = oitem.Design;
                            oSalesorderitemsDetail.hsncode = oitem.HSN;
                            oSalesorderitemsDetail.oqty = oitem.Qty;
                            oSalesorderitemsDetail.isactive = true;
                            oSalesorderitemsDetail.sno = oSalesOrder.Salesorderitems.Count + 1;
                            oSalesOrder.Salesorderitems.Add(oSalesorderitemsDetail);
                        }
                        oSalesOrder.sono = sequence.GetNextNumber(Context, conn, transaction);
                        Insert(transaction, conn, oSalesOrder);
                        foreach (var item in oSalesOrder.Salesorderitems)
                        {
                            if (!string.IsNullOrEmpty(item.pricerange))
                                item.priceranges = item.pricerange;
                            item.salesorderid = oSalesOrder.id;
                            if (!string.IsNullOrEmpty(item.sizename) && string.IsNullOrEmpty(item.size))
                                item.size = item.sizename;
                            subPurchaseorderitems.Insert(transaction, conn, item);
                        }
                        oSalesOrder.Salesorderitems = subPurchaseorderitems.SearchAll(oSalesOrder.id);
                        if (Services.ConfigManager.Instance(Context.OrganizationID).Config.intray_salesorder)
                        {
                            var customers = supplierdata.Search(new CustomerCreteria() { id = oSalesOrder.customerid });
                            if (customers != null && customers.Count > 0)
                            {
                                Services.IntrayManager.Instance(Context.OrganizationID).Add(Context.OrganizationID, Services.IntrayManager.EntryType.SalesOrder,
                                    oSalesOrder.id, string.Format("Sales ORDER - FOR THE Customer {0} WITH {1} QTY.", supplier[0].name,
                                    oSalesOrder.Salesorderitems.Sum(x => x.oqty)), Context.UserID, oSalesOrder.locationid, transaction, this.Context);
                            }
                        }

                        history.Add(new SalesOrderHistory()
                        {
                            id = oSalesOrder.id,
                            sono = oSalesOrder.sono,
                            sodate = oSalesOrder.sodate,
                            remark = oSalesOrder.remarks,
                            amount = oSalesOrder.Salesorderitems.Sum(x => x.amount),
                            customer = orderitem.Customer,
                            qty = oSalesOrder.Salesorderitems.Sum(x => x.oqty),
                        });

                        if (oSalesOrder.sosourcetype == 2)
                        {
                            CreateZeroStock(oSalesOrder, transaction, conn);
                        }
                    }
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, pouploaditem);
            }
            return history;
        }

        public void CreateZeroStock(Salesorder osalesorder, NpgsqlTransaction transation, NpgsqlConnection conn)
        {
            string query = @"
                            with alluids as
                            (
                                select salesorderitems.uid,salablegoods.id saleid,stock.id stockid from salesorder 
                                inner join salesorderitems on salesorderitems.salesorderid = salesorder.id
                                left outer join stock on stock.serialno = salesorderitems.uid
                                left outer join salablegoods on salablegoods.stockid = stock.id and salablegoods.locationid = salesorder.locationid
                                where salesorder.id = @salesorderid 
                                and (stock.id is  null or salablegoods.id is null)
                            ),availall as
                            (
                                select * from alluids where (alluids.stockid is not null and alluids.saleid is not null)	
                            ),availstock as
                            (
                                select * from alluids where (alluids.stockid is not null and alluids.saleid is  null)	
                            )
                            select alluids.uid,availstock.stockid,availall.saleid from alluids 
                            left outer join availall on availall.uid = alluids.uid
                            left outer join availstock on availstock.uid = alluids.uid
                            where (alluids.stockid is null or alluids.saleid is null)
                            and (availall.uid is null or availstock.uid is null)";

            List<Tuple<string, long, long>> lst = new List<Tuple<string, long, long>>();
            var param = new SortedList<string, object>();
            param.Add("salesorderid", osalesorder.id);
            using (var reader = ExecuteReader(query, param, conn, transation))
            {
                while (reader.Read())
                {
                    long saleid = reader["saleid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["saleid"]);
                    long stockid = reader["stockid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["stockid"]);
                    string uid = reader["uid"] == DBNull.Value ? "" : Convert.ToString(reader["uid"]);
                    if (lst.Exists(x => x.Item1 == uid)) continue;
                    lst.Add(new Tuple<string, long, long>(uid, stockid, saleid));
                }
            }

            MfgdesignDataService mfgdesignDataService = new MfgdesignDataService() { Context = Context };
            if (lst.Exists(x => x.Item2 == 0 && x.Item3 == 0))
            {
                var mlist = lst.FindAll(x => x.Item2 == 0 && x.Item3 == 0);
                var missingdesigns = mfgdesignDataService.SearchUANItem(mlist.ConvertAll(y => y.Item1));
                long lastid = 0;
                using (var reader = ExecuteReader("select barcode,id from stock order by id desc limit 1", null))
                {
                    if (reader.Read())
                    {
                        var lastbarcode = reader.GetString(0);
                        try
                        {
                            lastid = lastbarcode.LongFromBase62();
                        }
                        catch (Exception exp)
                        {
                            lastid = reader.GetInt64(1);
                        }
                    }
                }

                while (true)
                {
                    param = new SortedList<string, object>();
                    param.Add("barcode", lastid.ToBase62());
                    using (var reader = ExecuteReader("select id from stock where barcode=@barcode limit 1", param))
                    {
                        if (!reader.Read())
                        {
                            break;
                        }
                        else
                        {
                            lastid++;
                        }
                    }
                }

                InfyPOS.Data.StockDataService stockDataService = new InfyPOS.Data.StockDataService() { Context = Context };
                lst.RemoveAll(x => x.Item2 == 0 && x.Item3 == 0);
                foreach (var item in mlist)
                {
                    lastid++;
                    while (true)
                    {
                        try
                        {
                            var sitem = missingdesigns.Find(x => x.uid == item.Item1);
                            InfyPOS.Models.Stock stock = new InfyPOS.Models.Stock();
                            stock.barcode = lastid.ToBase62();
                            stock.dealerprice = sitem.dealerprice;
                            stock.designid = sitem.code;
                            stock.printingname = sitem.pname;
                            stock.productid = sitem.productid;
                            stock.itemid = sitem.itemid;
                            stock.hsncode = sitem.hsn;
                            stock.basebuyingprice = sitem.dealerprice;
                            stock.buyingprice = sitem.dealerprice;
                            stock.buyingqty = 0;
                            stock.displaymrp = sitem.mrp;
                            stock.eprice = sitem.dealerprice;
                            stock.invoiceitemid = -1;
                            stock.ibuyingprice = 0;
                            stock.isactive = true;
                            stock.price = sitem.mrp;
                            stock.serialno = sitem.uid;
                            stock.qty = 0;
                            stock.sizeid = sitem.sizeid;
                            stock.transfered = 0;
                            stock.companyid = osalesorder.companyid;
                            stockDataService.Insert(stock);
                            lst.Add(new Tuple<string, long, long>(item.Item1, stock.id, 0));
                            break;
                        }
                        catch (Exception exp)
                        {
                            if (exp.Message.Contains("duplicate key value violates unique constraint"))
                                lastid++;
                            else
                                break;
                        }
                    }
                }
            }


            if (lst.Exists(x => x.Item3 == 0))
            {
                var mlist = lst.FindAll(x => x.Item3 == 0);
                var missingdesigns = mfgdesignDataService.SearchUANItem(mlist.ConvertAll(y => y.Item1));
                SortedList<string, string> barcodes = new SortedList<string, string>();
                using (var reader = ExecuteReader("Select serialno,barcode from stock where id in (" +
                    string.Join(",", mlist.ConvertAll(y => y.Item2)) + ")", null, conn, transation))
                {
                    while (reader.Read())
                    {
                        var sno = reader["serialno"].ToString();
                        if (!barcodes.ContainsKey(sno))
                            barcodes.Add(sno, reader["barcode"].ToString());
                    }
                }
                foreach (var sitem in mlist)
                {
                    var dsnitem = missingdesigns.Find(x => x.uid == sitem.Item1);
                    InfyPOS.Data.SalablegoodsDataService salableDataService = new InfyPOS.Data.SalablegoodsDataService() { Context = Context };
                    Salablegoods salablegoods = new Salablegoods();
                    salablegoods.barcode = barcodes[sitem.Item1];
                    salablegoods.iqty = 0;
                    salablegoods.stockid = sitem.Item2;
                    salablegoods.companyid = osalesorder.companyid;
                    salablegoods.locationid = osalesorder.locationid;
                    salablegoods.itemid = dsnitem.itemid;
                    salablegoods.price = dsnitem.mrp;
                    salablegoods.sellingname = dsnitem.pname;
                    salablegoods.isactive = true;
                    salableDataService.Insert(salablegoods);
                }
            }
        }

        internal Salesorder SaveCatalog(SalesOrderCatalog item)
        {
            Salesorder oSalesorder = new Salesorder();
            oSalesorder.sodate = DateTimeNow.Date;
            oSalesorder.expectedate = DateTimeNow.Date.AddDays(15);
            oSalesorder.customerid = item.customerid;
            oSalesorder.locationid = item.deliverylocationid;
            oSalesorder.deliverylocationid = item.deliverylocationid;
            oSalesorder.agentid = item.agentid;
            oSalesorder.companyid = item.companyid;
            oSalesorder.salesmenid = item.salesmenid;
            oSalesorder.sizegroup = item.sizegroup;
            oSalesorder.remarks = item.remarks;
            oSalesorder.isactive = true;
            oSalesorder.sobookqty = item.orderitems.Sum(x => x.oqty);
            var alluid = item.orderitems.ConvertAll(x => x.uid);
            oSalesorder.Salesorderitems = new List<SalesorderitemsDetail>();
            MfgdesignDataService mfgdesignDataService = new MfgdesignDataService() { Context = this.Context };
            var alluandetails = mfgdesignDataService.SearchUANItem(alluid);
            ItemsDataService itemsDataService = new ItemsDataService() { Context = this.Context };
            var items = itemsDataService.Search(
                new ItemsCreteria()
                {
                    ids =
                alluandetails.ConvertAll(x => x.itemid)
                });

            ProductsDataService productsDataService = new ProductsDataService() { Context = this.Context };
            var products = productsDataService.Search(new ProductsCreteria()
            {
                ids = items.ConvertAll(x => x.productid)
            });

            TaxDataService taxDataService = new TaxDataService() { Context = this.Context };
            var taxlist = taxDataService.Search(new TaxCreteria()
            {
                ids = products.ConvertAll(x => x.salestaxid)
            });

            BrandDataService brandDataService = new BrandDataService() { Context = this.Context };
            var brands = brandDataService.Search(new BrandCreteria()
            {
                ids = items.ConvertAll(x => x.brandid)
            });

            ReferencelistDataService referencelistDataService = new ReferencelistDataService() { Context = this.Context };
            List<long> refids = new List<long>();
            refids.AddRange(alluandetails.ConvertAll(x => x.colourid));
            refids.AddRange(alluandetails.ConvertAll(x => x.styleid));
            refids.AddRange(alluandetails.ConvertAll(x => x.patternid));
            refids.AddRange(alluandetails.ConvertAll(x => x.sizeid));
            var referencelist = referencelistDataService.SearchList(new ReferencelistCreteria()
            {
                ids = refids
            });

            item.orderitems.RemoveAll(x => x.oqty == 0);

            foreach (var ordergrp in item.orderitems.GroupBy(x => x.designitemcode))
            {
                SalesorderitemsDetail salesorderitemsDetail = new SalesorderitemsDetail();
                salesorderitemsDetail.designitemcode = ordergrp.Key;
                var uandetail = alluandetails.FindAll(x => x.uid == ordergrp.First().uid);
                salesorderitemsDetail.colourcode = uandetail.First().colourcode;
                salesorderitemsDetail.colourid = uandetail.First().colourid;
                salesorderitemsDetail.brandid = uandetail.First().brandid;
                salesorderitemsDetail.styleid = uandetail.First().styleid;
                salesorderitemsDetail.designid = uandetail.First().code;
                salesorderitemsDetail.productid = uandetail.First().productid;
                salesorderitemsDetail.oqty = ordergrp.Sum(x => x.oqty);
                salesorderitemsDetail.price = ordergrp.First().price;
                salesorderitemsDetail.mrp = ordergrp.First().mrp;
                salesorderitemsDetail.isactive = true;

                if (referencelist.Exists(y => y.id == salesorderitemsDetail.colourid))
                    salesorderitemsDetail.stylename = referencelist.Find(y => y.id == salesorderitemsDetail.colourid).name;

                if (brands.Exists(y => y.id == salesorderitemsDetail.brandid))
                    salesorderitemsDetail.brandname = brands.Find(y => y.id == salesorderitemsDetail.brandid).name;

                if (products.Exists(y => y.id == salesorderitemsDetail.productid))
                {
                    var product = products.Find(y => y.id == salesorderitemsDetail.productid);
                    salesorderitemsDetail.productname = product.name;
                    var salestax = taxlist.FindAll(x => x.id == product.salestaxid);
                    if (salestax.Count > 0)
                    {
                        decimal taxpercentage = salestax[0].taxpercentage;
                        if (salestax[0].taxsplit != null)
                        {
                            var taxsplit = salestax[0].taxsplit.Find(salesorderitemsDetail.price);
                            taxpercentage = taxsplit.taxpercent;
                        }
                        salesorderitemsDetail.taxid = salestax[0].id;
                        salesorderitemsDetail.taxpercentage = taxpercentage;
                        salesorderitemsDetail.tax = Math.Round(salesorderitemsDetail.amount * taxpercentage / 100, 2);
                    }
                }

                if (referencelist.Exists(y => y.id == salesorderitemsDetail.colourid))
                    salesorderitemsDetail.colourname = referencelist.Find(y => y.id == salesorderitemsDetail.colourid).name;

                salesorderitemsDetail.description = string.Format("{0}/{1}/{2}", salesorderitemsDetail.productname,
                    salesorderitemsDetail.brandid, salesorderitemsDetail.colourname);

                salesorderitemsDetail.jumpqty = new List<SalesorderitemsDetail.SizeQty>();
                foreach (var size in ordergrp)
                {
                    uandetail = alluandetails.FindAll(x => x.uid == size.uid);
                    salesorderitemsDetail.jumpqty.Add(new SalesorderitemsDetail.SizeQty()
                    {
                        itemid = uandetail.First().itemid,
                        uid = uandetail.First().uid,
                        oqty = size.oqty,
                        sizeid = size.sizeid,
                        name = size.size
                    });
                }
                oSalesorder.Salesorderitems.Add(salesorderitemsDetail);
            }

            if (item.currentorder == null ||
                item.currentorder.id == 0)
                return Insert(oSalesorder);

            var oldorders = Search(new SalesorderCreteria()
            {
                id = item.currentorder.id
            });
            if (oldorders.Count == 0)
                throw new Exception("Given order is missing, search and edit again");

            oldorders[0].customerid = item.customerid;
            oldorders[0].locationid = item.deliverylocationid;
            oldorders[0].deliverylocationid = item.deliverylocationid;
            oldorders[0].agentid = item.agentid;
            oldorders[0].companyid = item.companyid;
            oldorders[0].sizegroup = item.sizegroup;
            oldorders[0].remarks = item.remarks;
            oldorders[0].salesmenid = item.salesmenid;
            List<SalesorderitemsDetail.SizeQty> neworderitems = new List<SalesorderitemsDetail.SizeQty>();
            oSalesorder.Salesorderitems.ForEach(x =>
            {
                neworderitems.AddRange(x.jumpqty);
            });

            oldorders[0].Salesorderitems.RemoveAll(x => !neworderitems.Exists(y => y.uid == x.uid));
            oSalesorder.Salesorderitems.ForEach(x =>
            {
                var existingchanged = x.jumpqty.FindAll(y => oldorders[0].Salesorderitems.Exists(z => z.uid == y.uid));
                x.jumpqty.RemoveAll(y => oldorders[0].Salesorderitems.Exists(z => z.uid == y.uid));
                foreach (var modifieditem in existingchanged)
                {
                    var eitem = oldorders[0].Salesorderitems.Find(z => z.uid == modifieditem.uid);
                    eitem.oqty = modifieditem.oqty;
                }
                if (x.jumpqty.Count > 0)
                {
                    oldorders[0].Salesorderitems.Add(x);
                }
            });

            oldorders[0].sobookqty = oldorders[0].Salesorderitems.Sum(x => x.oqty);
            return Update(oldorders[0]);
        }
    }
}
