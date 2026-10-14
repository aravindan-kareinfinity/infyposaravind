using InfyPOS.Common;
using InfyPOS.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebAPI.Data;

namespace InfyPOS.Payroll
{
    public class IncentiveEngine : BaseData
    {
        long departmentid;
        long companyid;
        long sectionid;
        long locationid;
        bool includesales;
        long employeeid = 0;
        long organizationid;
        decimal totalSales = 0;
        List<int> searchmodes = new List<int>();
        DateTime fromdate;
        DateTime todate;
        List<long> accessiblelocation;
        public IncentiveEngine(long organizationid, DateTime fromdate, DateTime todate, long departmentid, long sectionid,
            long companyid, List<long> accessiblelocation, long locationid, bool includesales, long employeeid, List<int> searchmodes) : base()
        {
            this.organizationid = organizationid;
            this.accessiblelocation = accessiblelocation;
            this.companyid = companyid;
            this.departmentid = departmentid;
            this.locationid = locationid;
            this.includesales = includesales;
            this.fromdate = fromdate;
            this.todate = todate;
            this.employeeid = employeeid;
            this.searchmodes = searchmodes;
            this.sectionid = sectionid;
        }

        public enum IncentiveSource
        {
            Prefix,
            Star,
            SectionPriceRange,
            ProductPriceRange,
            EmployeePriceRange,
            SectionAgeRange,
            ProductAgeRange,
            EmployeeAgeRange,
            ProductLevel,
            ProductPriceAgeRange,
            StockMarker,
            EmployeeSection,
            Tailoring,
            EmployeeRole,
        }
        public enum ApplyType
        {
            All,
            Employee,
            Incharge,
            Management
        }
        public class Salespersondata
        {
            public long salesmanid { get; set; }
            public decimal salevalue { get; set; }
            public decimal saleqty { get; set; }
            public IncentiveSource Source { get; set; }
            public ApplyType applyType { get; set; }
            public decimal incentive { get; set; }
            public decimal incentivecount { get; set; }
            public decimal incentivepercentage { get; set; }
            public long salescounterid { get; set; }
            public Range range { get; set; }
            public long productid { get; set; }
            public long referencecolumn { get; set; }
            public string name { get; set; }
        }

        public class Range
        {
            public string prefix { get; set; }
            public int start { get; set; }
            public int end { get; set; }
            public decimal fromprice { get; set; }
            public decimal toprice { get; set; }
            public string id { get; set; }
            public decimal incentive { get; set; }
            public bool value { get; set; }
            public long referenceid { get; set; }
            public decimal sincentive { get; set; }
        }

        private string getNamePrefixCase(List<Range> rangelist, string field, string typeCast)
        {
            System.Text.StringBuilder conditionlist = new System.Text.StringBuilder();
            foreach (var rng in rangelist)
            {
                conditionlist.AppendLine("when billitems.barcode ilike '" + rng.prefix + "%' and " + field + " >= " + rng.start + typeCast + " AND " + field + "<" + rng.end + typeCast + " then '" + rng.prefix + "'");
            }
            return " case " + conditionlist.ToString() + " else 'UNKNOWN' end ";
        }
        private string getIncentivePrefixCase(List<Range> rangelist, string field, string typeCast)
        {
            System.Text.StringBuilder conditionlist = new System.Text.StringBuilder();
            foreach (var rng in rangelist)
            {
                conditionlist.AppendLine("when billitems.barcode ilike '" + rng.prefix + "%' and " + field + " >= " + rng.start + typeCast + " AND " + field + "<" + rng.end + typeCast +
                   (!rng.value ? " then (billitems.receivable*" + rng.incentive.ToString("F4") + "/100" : " then (billitems.qty*" + rng.incentive.ToString("F4")) + ")::numeric");
            }
            return " case " + conditionlist.ToString() + " else 0::numeric end ";
        }

        private string getSaleValuePrefixCase(List<Range> rangelist, string field, string typeCast)
        {
            System.Text.StringBuilder conditionlist = new System.Text.StringBuilder();
            foreach (var rng in rangelist)
            {
                conditionlist.AppendLine("when billitems.barcode ilike '" + rng.prefix + "%' and " + field + " >= " + rng.start + typeCast + " AND " + field + "<" + rng.end + typeCast + " then (billitems.receivable)::numeric");
            }
            return " case " + conditionlist.ToString() + " else 0::numeric end ";
        }

        private string getCountPrefixCase(List<Range> rangelist, string field, string typeCast)
        {
            System.Text.StringBuilder conditionlist = new System.Text.StringBuilder();
            foreach (var rng in rangelist)
            {
                conditionlist.AppendLine("when billitems.barcode ilike '" + rng.prefix + "%' and " + field + " >= " + rng.start + typeCast + " AND " + field + "<" + rng.end + typeCast + " then 1::numeric");
            }
            return " case " + conditionlist.ToString() + " else 0::numeric end ";
        }

        private string getRangeCase(List<Range> rangelist, string field, string typeCast)
        {
            System.Text.StringBuilder conditionlist = new System.Text.StringBuilder();
            foreach (var rng in rangelist)
            {
                conditionlist.AppendLine("when " + field + " >= " + rng.start + typeCast + " AND " + field + "<" + rng.end + typeCast + " then '" + rng.id + "'");
            }
            return " case " + conditionlist.ToString() + " else 'All' end ";
        }

        private string getRangeCaseWithPriceAge(List<Range> rangelist, string field, string typeCast,
            string fieldname)
        {
            string pfield = "(billitems.receivable/billitems.qty)";
            string ptypecast = "::numeric";
            System.Text.StringBuilder conditionlist = new System.Text.StringBuilder();
            foreach (var rng in rangelist)
            {
                if (rng.referenceid > 0)
                    conditionlist.AppendLine("when " + fieldname + "=" + rng.referenceid
                        + " and " + field + " >= " + rng.start + typeCast + " AND " + field + "<" + rng.end + typeCast
                        + " and " + pfield + " >= " + rng.fromprice + ptypecast + " AND " + pfield + "<" + rng.toprice + ptypecast
                        + " then '" + rng.id + "'");
                else
                    conditionlist.AppendLine("when " + field + " >= " + rng.start + typeCast + " AND " + field + "<" + rng.end + typeCast
                        + " and " + pfield + " >= " + rng.fromprice + ptypecast + " AND " + pfield + "<" + rng.toprice + ptypecast
                        + " then '" + rng.id + "'");
            }
            return " case " + conditionlist.ToString() + " else 'All' end ";
        }

        private string getRangeCaseWithReference(List<Range> rangelist, string field, string typeCast, string fieldname)
        {
            System.Text.StringBuilder conditionlist = new System.Text.StringBuilder();
            foreach (var rng in rangelist)
            {
                if (rng.referenceid > 0)
                    conditionlist.AppendLine("when " + fieldname + "=" + rng.referenceid + " and " + field + " >= " + rng.start + typeCast + " AND " + field + "<" + rng.end + typeCast + " then '" + rng.id + "'");
                else
                    conditionlist.AppendLine("when " + field + " >= " + rng.start + typeCast + " AND " + field + "<" + rng.end + typeCast + " then '" + rng.id + "'");
            }
            return " case " + conditionlist.ToString() + " else 'All' end ";
        }


        private List<Salespersondata> SearchStarIncentives()
        {
            var result = new List<Salespersondata>();
            string query = @"
                                with salaryheader as (
			                    SELECT id, employeeid, fromdate, inchargeid, row_number() OVER (PARTITION BY employeeid ORDER BY fromdate DESC,id DESC) as ranknumber  
			                    FROM employeesalaryheader where fromdate<=@todate)
            select sum(billitems.receivable) as salevalue,
            sum(billitems.qty) as incentivecount,
            sum(billitems.qty*incentiveconfig.incentive::numeric) as incentive,
            sum(case when coalesce(incentiveconfig.percentage::numeric,0::numeric)>0::numeric then billitems.receivable::numeric*coalesce(incentiveconfig.percentage::numeric,0::numeric)/100::numeric else 0::numeric end) as incentivebypercentage,
            salesmanid from billitems
            inner join bill on bill.id = billitems.billid
            left outer join stock on stock.id = billitems.stockid
            left outer join products on products.id = stock.productid
            inner join incentiveconfig on incentiveconfig.salablegoodsid = billitems.salablegoodsid and incentiveconfig.isactive = true
            where bill.isactive=true and billitems.isactive=true  and billitems.qty!=0 and billdate>=@fromdate and billdate<@todate 
            and bill.locationid=@locationid and bill.billtype != 9 and bill.isb2b=false and salescounterid=@sectionid
            group by salesmanid";

            SortedList<string, object> param = new SortedList<string, object>();

            param.Add("salarymonth", fromdate);
            param.Add("fromdate", fromdate.Date);
            param.Add("locationid", locationid);
            param.Add("sectionid", sectionid);
            param.Add("todate", todate.Date.AddDays(1));

            if (employeeid > 0)
            {
                param.Add("employeeid", employeeid);
                query = query.Replace("and bill.locationid=@locationid", @"and (salesmanid=@employeeid or salesmanid in (select employeeid from salaryheader where ranknumber = 1 and inchargeid=@employeeid)) 
                and bill.locationid=@locationid");
                query = query.Replace("hasleft=false and isactive=true", " employee.id=@employeeid");
            }

            if (locationid == 0)
            {
                if (accessiblelocation == null || accessiblelocation.Contains(-1))
                {
                    query = query.Replace("and bill.locationid=@locationid", "");
                }
                else
                {
                    query = query.Replace("and bill.locationid=@locationid", "and bill.locationid in (" + string.Join(",", accessiblelocation) + ")");
                }
            }
            InfyPOS.Common.BaseData baseData = new Common.BaseData() { Context = new Common.UserClaim() { OrganizationID = organizationid } };

            if (InfyPOS.Services.ConfigManager.Instance(organizationid).Config.system_isb2bapplication)
                query = query.Replace("and bill.isb2b=false", "");
            if (sectionid <= 0)
                query = query.Replace("and salescounterid=@sectionid", "");


            using (System.Data.IDataReader reader = baseData.ExecuteReader(query, param, 10000))
            {
                while (reader.Read())
                {
                    result.Add(new Salespersondata()
                    {
                        salesmanid = reader["salesmanid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["salesmanid"]),
                        incentive = (reader["incentive"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["incentive"])) +
                        (reader["incentivebypercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["incentivebypercentage"])),
                        incentivecount = reader["incentivecount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["incentivecount"]),
                        salevalue = reader["salevalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["salevalue"]),
                        Source = IncentiveSource.Star
                    });
                }
            }

            return result;
        }

        private string getPrefixCondtion()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            foreach (var item in prefixrangelist)
            {
                if (sb.Length > 0)
                    sb.Append(" and ");
                sb.AppendFormat(" billitems.barcode not ilike '{0}%' ", item.prefix);
            }
            return sb.ToString();
        }

        private List<Salespersondata> ExecuteEmployeeSalesmenGroup()
        {

            List<Salespersondata> result = new List<Salespersondata>();
            var query = string.Format(@"
with markedlist as
(
				select distinct stockmarker.markerid,stockmarker.stockid from stockmarker
    left outer join incentiveconfig on incentiveconfig.salablegoodsid = stockmarker.salablegoodsid and incentiveconfig.isactive=true
    where markerid in ({0})
				and stockmarker.isactive = true and incentiveconfig.salablegoodsid is null
),
barcodelist as
(
				select distinct salablegoodsid from incentiveconfig where 
				incentiveconfig.isactive = true
),salegroup as
(
	SELECT billitems.id,t.smcode::int salesmanid,
    jsonb_array_length((billitems.attributes->'smcodes')::jsonb) AS scount
	FROM bill
	inner join billitems on billitems.billid = bill.id
	CROSS JOIN LATERAL jsonb_array_elements_text((billitems.attributes->'smcodes')::jsonb) AS t(smcode)
	where bill.isactive=true and billitems.isactive=true  and billitems.qty!=0 
	and billdate>=@fromdate and billdate<@todate 
	and bill.locationid=@locationid 
	and bill.billtype != 9 and bill.isb2b=false 
)
select sum(billitems.qty) saleqty,
sum(billitems.receivable/scount) as salevalue,salegroup.salesmanid,
sum(case when markedlist.stockid is null then 0 else billitems.qty end) mqty,
sum(case when markedlist.stockid is null then 0 else billitems.receivable/scount end) mamt, 
sum(case when barcodelist.salablegoodsid is null then 0 else billitems.qty end) bqty,
sum(case when barcodelist.salablegoodsid is null then 0 else billitems.receivable/scount end) bamt from bill
inner join billitems on bill.id = billitems.billid
left outer join stock on stock.id = billitems.stockid
left outer join products on products.id = stock.productid
left outer join lrinvoiceitems on lrinvoiceitems.id = stock.invoiceitemid
left outer join lrinvoice on lrinvoice.id = lrinvoiceid
left outer join markedlist on markedlist.stockid = billitems.stockid
left outer join barcodelist on barcodelist.salablegoodsid = billitems.salablegoodsid
left outer join salegroup on salegroup.id = billitems.id
where bill.isactive=true and billitems.isactive=true  and billitems.qty!=0 
and billdate>=@fromdate and billdate<@todate 
and bill.billtype != 9 and bill.isb2b=false 
group by salegroup.salesmanid
", string.Join(",", GetStockMarkerList()));


            if (locationid == 0)
            {
                if (accessiblelocation == null || accessiblelocation.Contains(-1))
                {
                    query = query.Replace("and bill.locationid=@locationid", "");
                }
                else
                {
                    query = query.Replace("and bill.locationid=@locationid", "and bill.locationid in (" + string.Join(",", accessiblelocation) + ")");
                }
            }
            InfyPOS.Common.BaseData baseData = new Common.BaseData() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            SortedList<string, object> param = new SortedList<string, object>();
            param.Add("salarymonth", fromdate);
            param.Add("fromdate", fromdate.Date);
            param.Add("locationid", locationid);
            param.Add("sectionid", sectionid);
            param.Add("employeeid", employeeid);
            param.Add("todate", todate.Date.AddDays(1));

            if (InfyPOS.Services.ConfigManager.Instance(organizationid).Config.payroll_incentive_addb2b)
                query = query.Replace("bill.isb2b=false", "bill.isexport=false");

            if (InfyPOS.Services.ConfigManager.Instance(organizationid).Config.payroll_incentive_addexport)
                query = query.Replace("and bill.isexport=false", "");

            if (InfyPOS.Services.ConfigManager.Instance(organizationid).Config.system_isb2bapplication)
                query = query.Replace("and bill.isb2b=false", "");


            using (System.Data.IDataReader reader = baseData.ExecuteReader(query, param, 10000))
            {
                while (reader.Read())
                {
                    var spd = new Salespersondata()
                    {
                        salesmanid = reader["salesmanid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["salesmanid"]),
                        salevalue = reader["salevalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["salevalue"]),
                        saleqty = reader["saleqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["saleqty"]),
                        Source = IncentiveSource.EmployeeRole,
                        applyType = ApplyType.Employee
                    };

                    if (InfyPOS.Services.ConfigManager.Instance(this.organizationid).Config.payroll_incentive_removebarcode)
                    {
                        spd.salevalue -= (reader["mamt"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["mamt"]));
                        spd.saleqty -= (reader["mqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["mqty"]));
                        spd.salevalue -= (reader["bamt"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["bamt"]));
                        spd.saleqty -= (reader["bqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["bqty"]));
                    }
                    result.Add(spd);
                }
            }
            return result;
        }

        private List<Salespersondata> ExecuteEmployeeAgeResult(List<Range> rangelist,
    string referencecolumn, IncentiveSource incentiveSource, ApplyType applyType, string condition)
        {
            if (prefixrangelist.Count > 0)
            {
                var prefixcondtion = getPrefixCondtion();
                condition = string.IsNullOrEmpty(condition) ? prefixcondtion : condition + " and " + prefixcondtion;
            }

            List<Salespersondata> result = new List<Salespersondata>();
            var field = getRangeCaseWithReference(rangelist, "((EXTRACT(epoch FROM(SELECT(bill.billdate - lrinvoice.entrydate))) / 86400))", "::integer", "billitems.salesmanid");
            var query = string.Format(@"

                                with salaryheader as (
			                    SELECT id, employeeid, fromdate, inchargeid, row_number() OVER (PARTITION BY employeeid ORDER BY fromdate DESC,id DESC) as ranknumber  
			                    FROM employeesalaryheader where fromdate<=@todate),
                                markedlist as
								(
									select distinct stockmarker.markerid,stockmarker.stockid from stockmarker
                                    left outer join incentiveconfig on incentiveconfig.salablegoodsid = stockmarker.salablegoodsid and incentiveconfig.isactive=true
                                    where markerid in ({3})
									and stockmarker.isactive = true and incentiveconfig.salablegoodsid is null
								),
                                barcodelist as
								(
									select distinct salablegoodsid from incentiveconfig where 
									incentiveconfig.isactive = true
								)
                                select {0} as rangeid,{1} referencecolumn,sum(billitems.qty) saleqty,
                                sum(billitems.receivable) as salevalue,salesmanid,
                                sum(case when markedlist.stockid is null then 0 else billitems.qty end) mqty,
                                sum(case when markedlist.stockid is null then 0 else billitems.receivable end) mamt, 
                                sum(case when barcodelist.salablegoodsid is null then 0 else billitems.qty end) bqty,
                                sum(case when barcodelist.salablegoodsid is null then 0 else billitems.receivable end) bamt from billitems
                                inner join bill on bill.id = billitems.billid
                                left outer join stock on stock.id = billitems.stockid
                                left outer join products on products.id = stock.productid
                                left outer join lrinvoiceitems on lrinvoiceitems.id = stock.invoiceitemid
                                left outer join lrinvoice on lrinvoice.id = lrinvoiceid
                                left outer join markedlist on markedlist.stockid = billitems.stockid
                                left outer join barcodelist on barcodelist.salablegoodsid = billitems.salablegoodsid
                                where bill.isactive=true and billitems.isactive=true  and billitems.qty!=0 and billdate>=@fromdate and billdate<@todate 
                                and bill.locationid=@locationid and bill.billtype != 9 and bill.isb2b=false and salescounterid=@sectionid
                                and (salesmanid=@employeeid or salesmanid in (select employeeid from salaryheader where ranknumber = 1 and inchargeid=@employeeid))
                                group by salesmanid,{1},{0}", field, referencecolumn,
                                string.IsNullOrEmpty(condition) ? "" : (" and " + condition),
                                string.Join(",", GetStockMarkerList()));

            if (employeeid == 0)
            {
                query = query.Replace("and (salesmanid=@employeeid or salesmanid in (select employeeid from salaryheader where ranknumber = 1 and inchargeid=@employeeid))", "");
            }


            if (locationid == 0)
            {
                if (accessiblelocation == null || accessiblelocation.Contains(-1))
                {
                    query = query.Replace("and bill.locationid=@locationid", "");
                }
                else
                {
                    query = query.Replace("and bill.locationid=@locationid", "and bill.locationid in (" + string.Join(",", accessiblelocation) + ")");
                }
            }
            InfyPOS.Common.BaseData baseData = new Common.BaseData() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            SortedList<string, object> param = new SortedList<string, object>();
            param.Add("salarymonth", fromdate);
            param.Add("fromdate", fromdate.Date);
            param.Add("locationid", locationid);
            param.Add("sectionid", sectionid);
            param.Add("employeeid", employeeid);
            param.Add("todate", todate.Date.AddDays(1));

            if (InfyPOS.Services.ConfigManager.Instance(organizationid).Config.payroll_incentive_addb2b)
                query = query.Replace("bill.isb2b=false", "bill.isexport=false");

            if (InfyPOS.Services.ConfigManager.Instance(organizationid).Config.payroll_incentive_addexport)
                query = query.Replace("and bill.isexport=false", "");

            if (InfyPOS.Services.ConfigManager.Instance(organizationid).Config.system_isb2bapplication)
                query = query.Replace("and bill.isb2b=false", "");
            if (sectionid <= 0)
                query = query.Replace("and salescounterid=@sectionid", "");


            using (System.Data.IDataReader reader = baseData.ExecuteReader(query, param, 10000))
            {
                while (reader.Read())
                {
                    string rangeid = reader["rangeid"].ToString();
                    if (rangeid == "All") continue;
                    var spd = new Salespersondata()
                    {
                        salesmanid = reader["salesmanid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["salesmanid"]),
                        referencecolumn = reader["referencecolumn"] == DBNull.Value ? 0 : Convert.ToInt64(reader["referencecolumn"]),
                        range = rangelist.Exists(e => e.id == rangeid) ? rangelist.Find(e => e.id == rangeid) : new Range(),
                        salevalue = reader["salevalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["salevalue"]),
                        saleqty = reader["saleqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["saleqty"]),
                        Source = IncentiveSource.ProductAgeRange,
                        applyType = ApplyType.Employee
                    };

                    if (InfyPOS.Services.ConfigManager.Instance(this.organizationid).Config.payroll_incentive_removebarcode)
                    {
                        spd.salevalue -= (reader["mamt"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["mamt"]));
                        spd.saleqty -= (reader["mqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["mqty"]));
                        spd.salevalue -= (reader["bamt"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["bamt"]));
                        spd.saleqty -= (reader["bqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["bqty"]));
                    }
                    result.Add(spd);
                }
            }
            return result;
        }

        private List<Salespersondata> ExecuteTailoringIncentive()
        {
            Data.ReferencelistDataService referencelistDataService = new Data.ReferencelistDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            var config = referencelistDataService.Search(new Models.ReferencelistCreteria()
            {
                locationid = locationid,
                type = "INCENTIVE-EMPLOYEE-TAILORING"
            });

            if (config.Count == 0)
            {
                config = referencelistDataService.Search(new Models.ReferencelistCreteria()
                {
                    type = "INCENTIVE-EMPLOYEE-TAILORING"
                });

                if (config.Exists(e => e.locationid == -1))
                {
                    config.RemoveAll(e => e.locationid != -1);
                }
                else if (!config.All(e => e.locationid == -1))
                {
                    config.RemoveAll(e => e.locationid != config[0].locationid);
                }
            }

            List<Salespersondata> result = new List<Salespersondata>();
            if (config.Count == 0) return result;

            var salesincentive = config[0].additionalinfo.salesincentive;

            var query = string.Format(@"
                                with salaryheader as (
			                    SELECT id, employeeid, fromdate, inchargeid, row_number() OVER (PARTITION BY employeeid ORDER BY fromdate DESC,id DESC) as ranknumber  
			                    FROM employeesalaryheader where fromdate<=@todate)
                                select sum(gross) bgamt,
		                        sum(total) salevalue,
		                        sum(expectedqty) saleqty,
		                        receivedby salesmanid from TailoringOrder
		                        where isactive=true 
                                and receivedon>=@fromdate and receivedon<@todate
		                        and TailoringOrder.companyid=@companyid and TailoringOrder.locationid=@locationid
                                and (salesmanid=@employeeid or salesmanid in (select employeeid from salaryheader where ranknumber = 1 and inchargeid=@employeeid))
                                group by receivedby", "");

            if (employeeid == 0)
            {
                query = query.Replace("and (salesmanid=@employeeid or salesmanid in (select employeeid from salaryheader where ranknumber = 1 and inchargeid=@employeeid))", "");
            }


            if (locationid == 0)
            {
                if (accessiblelocation == null || accessiblelocation.Contains(-1))
                {
                    query = query.Replace("and bill.locationid=@locationid", "");
                }
                else
                {
                    query = query.Replace("and TailoringOrder.locationid=@locationid", "and TailoringOrder.locationid in (" + string.Join(",", accessiblelocation) + ")");
                }
            }
            InfyPOS.Common.BaseData baseData = new Common.BaseData() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            SortedList<string, object> param = new SortedList<string, object>();
            param.Add("salarymonth", fromdate);
            param.Add("fromdate", fromdate.Date);
            param.Add("locationid", locationid);
            param.Add("companyid", companyid);
            param.Add("sectionid", sectionid);
            param.Add("employeeid", employeeid);
            param.Add("todate", todate.Date.AddDays(1));

            decimal defaultincentive = salesincentive.Exists(x => x.referenceid == 0) ? salesincentive.Find(x => x.referenceid == 0).incentive : 0;



            using (System.Data.IDataReader reader = baseData.ExecuteReader(query, param, 10000))
            {
                while (reader.Read())
                {

                    var spd = new Salespersondata()
                    {
                        salesmanid = reader["salesmanid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["salesmanid"]),
                        salevalue = reader["salevalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["salevalue"]),
                        saleqty = reader["saleqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["saleqty"]),
                        Source = IncentiveSource.Tailoring,
                        applyType = ApplyType.Employee,
                    };
                    if (salesincentive.Exists(x => x.referenceid == spd.salesmanid))
                    {
                        spd.incentivepercentage = salesincentive.Find(x => x.referenceid == spd.salesmanid).incentive;
                    }
                    else
                    {
                        spd.incentivepercentage = defaultincentive;
                    }
                    spd.incentive = Math.Round(spd.incentivepercentage * spd.salevalue / 100, 0);
                    result.Add(spd);
                }
            }
            return result;
        }

        private List<Salespersondata> ExecuteProductAgeResult(List<Range> rangelist,
            string referencecolumn, IncentiveSource incentiveSource, ApplyType applyType, string condition)
        {
            if (prefixrangelist.Count > 0)
            {
                var prefixcondtion = getPrefixCondtion();
                condition = string.IsNullOrEmpty(condition) ? prefixcondtion : condition + " and " + prefixcondtion;
            }

            List<Salespersondata> result = new List<Salespersondata>();
            var field = getRangeCaseWithReference(rangelist, "((EXTRACT(epoch FROM(SELECT(bill.billdate - lrinvoice.entrydate))) / 86400))", "::integer", referencecolumn);
            var query = string.Format(@"
                                with salaryheader as (
			                    SELECT id, employeeid, fromdate, inchargeid, row_number() OVER (PARTITION BY employeeid ORDER BY fromdate DESC,id DESC) as ranknumber  
			                    FROM employeesalaryheader where fromdate<=@todate),
                                markedlist as
								(
									select distinct stockmarker.markerid,stockmarker.stockid from stockmarker
                                    left outer join incentiveconfig on incentiveconfig.salablegoodsid = stockmarker.salablegoodsid and incentiveconfig.isactive=true
                                    where markerid in ({3})
									and stockmarker.isactive = true and incentiveconfig.salablegoodsid is null
								),
                                barcodelist as
								(
									select distinct salablegoodsid from incentiveconfig where 
									incentiveconfig.isactive = true
								)

                                select {0} as rangeid,{1} referencecolumn,sum(billitems.qty) saleqty,
                                sum(billitems.receivable) as salevalue,salesmanid,
                                sum(case when markedlist.stockid is null then 0 else billitems.qty end) mqty,
                                sum(case when markedlist.stockid is null then 0 else billitems.receivable end) mamt, 
                                sum(case when barcodelist.salablegoodsid is null then 0 else billitems.qty end) bqty,
                                sum(case when barcodelist.salablegoodsid is null then 0 else billitems.receivable end) bamt from billitems
                                inner join bill on bill.id = billitems.billid
                                left outer join stock on stock.id = billitems.stockid
                                left outer join products on products.id = stock.productid
                                left outer join lrinvoiceitems on lrinvoiceitems.id = stock.invoiceitemid
                                left outer join lrinvoice on lrinvoice.id = lrinvoiceid
                                left outer join markedlist on markedlist.stockid = billitems.stockid
                                left outer join barcodelist on barcodelist.salablegoodsid = billitems.salablegoodsid
                                where bill.isactive=true and billitems.isactive=true  and billitems.qty!=0 and billdate>=@fromdate and billdate<@todate 
                                and bill.locationid=@locationid and bill.billtype != 9 and bill.isb2b=false and salescounterid=@sectionid
                                and (salesmanid=@employeeid or salesmanid in (select employeeid from salaryheader where ranknumber = 1 and inchargeid=@employeeid)) {2}
                                group by salesmanid,{1},{0}", field, referencecolumn,
                                string.IsNullOrEmpty(condition) ? "" : (" and " + condition),
                                string.Join(",", GetStockMarkerList()));

            if (employeeid == 0)
            {
                query = query.Replace("and (salesmanid=@employeeid or salesmanid in (select employeeid from salaryheader where ranknumber = 1 and inchargeid=@employeeid))", "");
            }


            if (locationid == 0)
            {
                if (accessiblelocation == null || accessiblelocation.Contains(-1))
                {
                    query = query.Replace("and bill.locationid=@locationid", "");
                }
                else
                {
                    query = query.Replace("and bill.locationid=@locationid", "and bill.locationid in (" + string.Join(",", accessiblelocation) + ")");
                }
            }
            InfyPOS.Common.BaseData baseData = new Common.BaseData() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            SortedList<string, object> param = new SortedList<string, object>();
            param.Add("salarymonth", fromdate);
            param.Add("fromdate", fromdate.Date);
            param.Add("locationid", locationid);
            param.Add("sectionid", sectionid);
            param.Add("employeeid", employeeid);
            param.Add("todate", todate.Date.AddDays(1));

            if (InfyPOS.Services.ConfigManager.Instance(organizationid).Config.payroll_incentive_addb2b)
                query = query.Replace("bill.isb2b=false", "bill.isexport=false");

            if (InfyPOS.Services.ConfigManager.Instance(organizationid).Config.payroll_incentive_addexport)
                query = query.Replace("and bill.isexport=false", "");

            if (InfyPOS.Services.ConfigManager.Instance(organizationid).Config.system_isb2bapplication)
                query = query.Replace("and bill.isb2b=false", "");
            if (sectionid <= 0)
                query = query.Replace("and salescounterid=@sectionid", "");


            using (System.Data.IDataReader reader = baseData.ExecuteReader(query, param, 10000))
            {
                while (reader.Read())
                {
                    string rangeid = reader["rangeid"].ToString();
                    if (rangeid == "All") continue;
                    var spd = new Salespersondata()
                    {
                        salesmanid = reader["salesmanid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["salesmanid"]),
                        referencecolumn = reader["referencecolumn"] == DBNull.Value ? 0 : Convert.ToInt64(reader["referencecolumn"]),
                        range = rangelist.Exists(e => e.id == rangeid) ? rangelist.Find(e => e.id == rangeid) : new Range(),
                        salevalue = reader["salevalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["salevalue"]),
                        saleqty = reader["saleqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["saleqty"]),
                        Source = IncentiveSource.ProductAgeRange,
                        applyType = ApplyType.Employee
                    };

                    if (InfyPOS.Services.ConfigManager.Instance(this.organizationid).Config.payroll_incentive_removebarcode)
                    {
                        spd.salevalue -= (reader["mamt"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["mamt"]));
                        spd.saleqty -= (reader["mqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["mqty"]));
                        spd.salevalue -= (reader["bamt"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["bamt"]));
                        spd.saleqty -= (reader["bqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["bqty"]));
                    }
                    result.Add(spd);
                }
            }
            return result;
        }


        private List<Salespersondata> ExecuteAgePriceResult(List<Range> rangelist,
            string referencecolumn, IncentiveSource incentiveSource, ApplyType applyType, string condition)
        {
            if (prefixrangelist.Count > 0)
            {
                var prefixcondtion = getPrefixCondtion();
                condition = string.IsNullOrEmpty(condition) ? prefixcondtion : condition + " and " + prefixcondtion;
            }

            List<Salespersondata> result = new List<Salespersondata>();
            var field = getRangeCaseWithPriceAge(rangelist, "((EXTRACT(epoch FROM(SELECT(bill.billdate - lrinvoice.entrydate))) / 86400))", "::integer", "stock.productid");
            var query = string.Format(@"
                                with salaryheader as (
			                    SELECT id, employeeid, fromdate, inchargeid, row_number() OVER (PARTITION BY employeeid ORDER BY fromdate DESC,id DESC) as ranknumber  
			                    FROM employeesalaryheader where fromdate<=@todate),
                                markedlist as
								(
									select distinct stockmarker.markerid,stockmarker.stockid from stockmarker
                                    left outer join incentiveconfig on incentiveconfig.salablegoodsid = stockmarker.salablegoodsid and incentiveconfig.isactive=true
                                    where markerid in ({3})
									and stockmarker.isactive = true and incentiveconfig.salablegoodsid is null
								),
                                barcodelist as
								(
									select distinct salablegoodsid from incentiveconfig where 
									incentiveconfig.isactive = true
								)

                                select {0} as rangeid,{1} referencecolumn,sum(billitems.qty) saleqty,sum(billitems.receivable) as salevalue,salesmanid,
                                sum(case when markedlist.stockid is null then 0 else billitems.qty end) mqty,
                                sum(case when markedlist.stockid is null then 0 else billitems.receivable end) mamt, 
                                sum(case when barcodelist.salablegoodsid is null then 0 else billitems.qty end) bqty,
                                sum(case when barcodelist.salablegoodsid is null then 0 else billitems.receivable end) bamt from billitems
                                inner join bill on bill.id = billitems.billid
                                left outer join stock on stock.id = billitems.stockid
                                left outer join products on products.id = stock.productid
                                left outer join lrinvoiceitems on lrinvoiceitems.id = stock.invoiceitemid
                                left outer join lrinvoice on lrinvoice.id = lrinvoiceid
                                left outer join markedlist on markedlist.stockid = billitems.stockid
                                left outer join barcodelist on barcodelist.salablegoodsid = billitems.salablegoodsid
                                where bill.isactive=true and billitems.isactive=true  and billitems.qty!=0 and billdate>=@fromdate and billdate<@todate 
                                and bill.locationid=@locationid and bill.billtype != 9 and bill.isb2b=false and salescounterid=@sectionid
                                and (salesmanid=@employeeid or salesmanid in (select employeeid from salaryheader where ranknumber = 1 and inchargeid=@employeeid)) {2}
                                group by salesmanid,{1},{0}", field, referencecolumn,
                                string.IsNullOrEmpty(condition) ? "" : (" and " + condition),
                                string.Join(",", GetStockMarkerList()));

            if (employeeid == 0)
            {
                query = query.Replace("and (salesmanid=@employeeid or salesmanid in (select employeeid from salaryheader where ranknumber = 1 and inchargeid=@employeeid))", "");

            }
            if (locationid == 0)
            {
                if (accessiblelocation == null || accessiblelocation.Contains(-1))
                {
                    query = query.Replace("and bill.locationid=@locationid", "");
                }
                else
                {
                    query = query.Replace("and bill.locationid=@locationid", "and bill.locationid in (" + string.Join(",", accessiblelocation) + ")");
                }
            }
            InfyPOS.Common.BaseData baseData = new Common.BaseData() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            SortedList<string, object> param = new SortedList<string, object>();
            param.Add("salarymonth", fromdate);
            param.Add("fromdate", fromdate.Date);
            param.Add("locationid", locationid);
            param.Add("sectionid", sectionid);
            param.Add("employeeid", employeeid);
            param.Add("todate", todate.Date.AddDays(1));

            if (InfyPOS.Services.ConfigManager.Instance(organizationid).Config.payroll_incentive_addb2b)
                query = query.Replace("bill.isb2b=false", "bill.isexport=false");

            if (InfyPOS.Services.ConfigManager.Instance(organizationid).Config.payroll_incentive_addexport)
                query = query.Replace("and bill.isexport=false", "");

            if (InfyPOS.Services.ConfigManager.Instance(organizationid).Config.system_isb2bapplication)
                query = query.Replace("and bill.isb2b=false", "");
            if (sectionid <= 0)
                query = query.Replace("and salescounterid=@sectionid", "");


            using (System.Data.IDataReader reader = baseData.ExecuteReader(query, param, 10000))
            {
                while (reader.Read())
                {
                    string rangeid = reader["rangeid"].ToString();
                    if (rangeid == "All") continue;
                    var spd = new Salespersondata()
                    {
                        salesmanid = reader["salesmanid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["salesmanid"]),
                        referencecolumn = reader["referencecolumn"] == DBNull.Value ? 0 : Convert.ToInt64(reader["referencecolumn"]),
                        range = rangelist.Exists(e => e.id == rangeid) ? rangelist.Find(e => e.id == rangeid) : new Range(),
                        salevalue = reader["salevalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["salevalue"]),
                        saleqty = reader["saleqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["saleqty"]),
                        Source = IncentiveSource.ProductAgeRange,
                        applyType = ApplyType.Employee
                    };

                    if (InfyPOS.Services.ConfigManager.Instance(this.organizationid).Config.payroll_incentive_removebarcode)
                    {
                        spd.salevalue -= (reader["mamt"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["mamt"]));
                        spd.saleqty -= (reader["mqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["mqty"]));
                        spd.salevalue -= (reader["bamt"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["bamt"]));
                        spd.saleqty -= (reader["bqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["bqty"]));
                    }
                    result.Add(spd);
                }
            }
            return result;
        }

        private List<Salespersondata> ExecutePriceResult(List<Range> rangelist,
         string referencecolumn, IncentiveSource incentiveSource, ApplyType applyType, string condition)
        {
            if (prefixrangelist.Count > 0)
            {
                var prefixcondtion = getPrefixCondtion();
                condition = string.IsNullOrEmpty(condition) ? prefixcondtion : condition + " and " + prefixcondtion;
            }

            List<Salespersondata> result = new List<Salespersondata>();

            string field = getRangeCaseWithReference(rangelist, "(billitems.receivable/billitems.qty)", "::numeric", "salesmanid");

            var query = string.Format(@"
                                with salaryheader as (
			                    SELECT id, employeeid, fromdate, inchargeid, row_number() OVER (PARTITION BY employeeid ORDER BY fromdate DESC,id DESC) as ranknumber  
			                    FROM employeesalaryheader where fromdate<=@todate),
                                markedlist as
								(
									select distinct stockmarker.markerid,stockmarker.stockid from stockmarker
                                    left outer join incentiveconfig on incentiveconfig.salablegoodsid = stockmarker.salablegoodsid and incentiveconfig.isactive=true
                                    where markerid in ({3})
									and stockmarker.isactive = true and incentiveconfig.salablegoodsid is null
								),
                                barcodelist as
								(
									select distinct salablegoodsid from incentiveconfig where 
									incentiveconfig.isactive = true
								)

                                select {0} as rangeid,{1} referencecolumn,sum(billitems.qty) saleqty,sum(billitems.receivable) as salevalue,salesmanid,
                                sum(case when markedlist.stockid is null then 0 else billitems.qty end) mqty,
                                sum(case when markedlist.stockid is null then 0 else billitems.receivable end) mamt, 
                                sum(case when barcodelist.salablegoodsid is null then 0 else billitems.qty end) bqty,
                                sum(case when barcodelist.salablegoodsid is null then 0 else billitems.receivable end) bamt  from billitems
                                inner join bill on bill.id = billitems.billid
                                left outer join stock on stock.id = billitems.stockid
                                left outer join products on products.id = stock.productid
                                left outer join markedlist on markedlist.stockid = billitems.stockid
                                left outer join barcodelist on barcodelist.salablegoodsid = billitems.salablegoodsid
                                where bill.isactive=true and billitems.isactive=true  and billitems.qty!=0 and billdate>=@fromdate and billdate<@todate 
                                and bill.locationid=@locationid and bill.billtype != 9 and bill.isb2b=false and salescounterid=@sectionid
                                and (salesmanid=@employeeid or salesmanid in (select employeeid from salaryheader where ranknumber = 1 and inchargeid=@employeeid)) {2}
                                group by salesmanid,{1},{0}", field, referencecolumn,
                                string.IsNullOrEmpty(condition) ? "" : (" and " + condition),
                                string.Join(",", GetStockMarkerList()));

            if (employeeid == 0)
            {
                query = query.Replace("and (salesmanid=@employeeid or salesmanid in (select employeeid from salaryheader where ranknumber = 1 and inchargeid=@employeeid))", "");
            }
            if (locationid == 0)
            {
                if (accessiblelocation == null || accessiblelocation.Contains(-1))
                {
                    query = query.Replace("and bill.locationid=@locationid", "");
                }
                else
                {
                    query = query.Replace("and bill.locationid=@locationid", "and bill.locationid in (" + string.Join(",", accessiblelocation) + ")");
                }
            }
            InfyPOS.Common.BaseData baseData = new Common.BaseData() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            SortedList<string, object> param = new SortedList<string, object>();
            param.Add("salarymonth", fromdate);
            param.Add("fromdate", fromdate.Date);
            param.Add("employeeid", employeeid);
            param.Add("locationid", locationid);
            param.Add("sectionid", sectionid);
            param.Add("todate", todate.Date.AddDays(1));

            if (InfyPOS.Services.ConfigManager.Instance(organizationid).Config.payroll_incentive_addb2b)
                query = query.Replace("bill.isb2b=false", "bill.isexport=false");

            if (InfyPOS.Services.ConfigManager.Instance(organizationid).Config.payroll_incentive_addexport)
                query = query.Replace("and bill.isexport=false", "");

            if (InfyPOS.Services.ConfigManager.Instance(organizationid).Config.system_isb2bapplication)
                query = query.Replace("and bill.isb2b=false", "");
            if (sectionid <= 0)
                query = query.Replace("and salescounterid=@sectionid", "");


            using (System.Data.IDataReader reader = baseData.ExecuteReader(query, param, 10000))
            {
                while (reader.Read())
                {
                    string rangeid = reader["rangeid"].ToString();
                    if (rangeid == "All") continue;
                    var spd = new Salespersondata()
                    {
                        salesmanid = reader["salesmanid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["salesmanid"]),
                        referencecolumn = reader["referencecolumn"] == DBNull.Value ? 0 : Convert.ToInt64(reader["referencecolumn"]),
                        range = rangelist.Exists(e => e.id == rangeid) ? rangelist.Find(e => e.id == rangeid) : new Range(),
                        salevalue = reader["salevalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["salevalue"]),
                        saleqty = reader["saleqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["saleqty"]),
                        Source = IncentiveSource.ProductAgeRange,
                        applyType = ApplyType.Employee
                    };

                    if (InfyPOS.Services.ConfigManager.Instance(this.organizationid).Config.payroll_incentive_removebarcode)
                    {
                        spd.salevalue -= (reader["mamt"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["mamt"]));
                        spd.saleqty -= (reader["mqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["mqty"]));
                        spd.salevalue -= (reader["bamt"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["bamt"]));
                        spd.saleqty -= (reader["bqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["bqty"]));
                    }
                    result.Add(spd);
                }
            }
            return result;
        }

        private List<Salespersondata> SearchProductPriceAgeRangeIncentives(List<IncentiveSalary.IncentiveData.Item> allEmployees)
        {
            var result = new List<Salespersondata>();
            Data.ReferencelistDataService referencelistDataService = new Data.ReferencelistDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            var config = referencelistDataService.Search(new Models.ReferencelistCreteria()
            {
                locationid = locationid,
                type = "INCENTIVE-PRODUCT-AGEPRICE"
            });

            if (config.Count == 0)
            {
                config = referencelistDataService.Search(new Models.ReferencelistCreteria()
                {
                    type = "INCENTIVE-PRODUCT-AGEPRICE"
                });

                if (config.Exists(e => e.locationid == -1))
                {
                    config.RemoveAll(e => e.locationid != -1);
                }
                else if (!config.All(e => e.locationid == -1))
                {
                    config.RemoveAll(e => e.locationid != config[0].locationid);
                }
            }

            if (config.Count > 0 && config.First().additionalinfo != null && config.First().additionalinfo.salesincentive != null && config.First().additionalinfo.salesincentive.Count > 0)
            {
                List<Range> rangelist = new List<Range>();
                foreach (var item in config.First().additionalinfo.salesincentive.OrderBy(e => e.from))
                {
                    if (item.from > 0 || item.to > 0)
                    {
                        var rng = new Range()
                        {
                            start = (int)item.from,
                            end = (int)item.to,
                            fromprice = (int)item.fromprice,
                            toprice = (int)item.toprice,
                            incentive = item.incentive,
                            prefix = item.prefix,
                            referenceid = item.referenceid,
                            value = item.value,
                            id = string.Format("{0}_{1}", item.referenceid, rangelist.Count.ToString())
                        };
                        rangelist.Add(rng);
                    }
                }

                if (rangelist.Exists(e => e.referenceid > 0))
                {
                    var ageresut = ExecuteAgePriceResult(rangelist.FindAll(e => e.referenceid > 0), "stock.productid",
                         IncentiveSource.ProductPriceAgeRange, ApplyType.All, "");
                    allEmployees.ForEach(e =>
                    {
                        e.AddSalebyages(IncentiveSource.ProductPriceAgeRange, ageresut.FindAll(ex => ex.salesmanid == e.employeeid));
                        //if (e.ValueType == IncentiveSalary.IncentiveData.Item.ValueTypes.Value)
                        //{
                        //    var incentive = ageresut.FindAll(ex => ex.salesmanid == e.employeeid).Sum(ex =>
                        //    {
                        //        if (ex.range.value)
                        //            return ex.range.incentive * ex.salevalue;
                        //        return ex.range.incentive * ex.salevalue / 100;
                        //    });
                        //    e.productagerangeincentive += incentive;
                        //    e.incentive += incentive;
                        //    e.amount += incentive;
                        //}
                        //else if (e.ValueType == IncentiveSalary.IncentiveData.Item.ValueTypes.Quantity)
                        //{
                        //    var incentive = ageresut.FindAll(ex => ex.salesmanid == e.employeeid).Sum(ex => ex.range.incentive * ex.saleqty);
                        //    e.incentive += incentive;
                        //    e.amount += incentive;
                        //    e.productagerangeincentive += incentive;
                        //}
                    });
                }

                if (rangelist.Exists(e => e.referenceid == 0))
                {
                    string condition = rangelist.Exists(e => e.referenceid > 0) ? "stock.productid not in (" + string.Join(", ", rangelist.FindAll(e => e.referenceid > 0).ConvertAll(e => e.referenceid)) + ")" : "";
                    var ageresut = ExecuteAgePriceResult(rangelist.FindAll(e => e.referenceid == 0), "stock.productid",
                         IncentiveSource.ProductPriceAgeRange, ApplyType.Employee, condition);

                    allEmployees.ForEach(e =>
                    {
                        e.AddSalebyages(IncentiveSource.ProductPriceAgeRange, ageresut.FindAll(ex => ex.salesmanid == e.employeeid));
                        //if (e.ValueType == IncentiveSalary.IncentiveData.Item.ValueTypes.Value)
                        //{
                        //    var incentive = ageresut.FindAll(ex => ex.salesmanid == e.employeeid).Sum(ex =>
                        //    {
                        //        if (ex.range.value)
                        //            return ex.range.incentive * ex.salevalue;
                        //        return ex.range.incentive * ex.salevalue / 100;
                        //    });
                        //    e.productagepricerangeincentive += incentive;
                        //    e.incentive += incentive;
                        //    e.amount += incentive;
                        //}
                        //else if (e.ValueType == IncentiveSalary.IncentiveData.Item.ValueTypes.Quantity)
                        //{
                        //    var incentive = ageresut.FindAll(ex => ex.salesmanid == e.employeeid).Sum(ex => ex.range.incentive * ex.saleqty);
                        //    e.incentive += incentive;
                        //    e.amount += incentive;
                        //    e.productagepricerangeincentive += incentive;
                        //}
                    });
                }
            }
            return result;
        }

        private List<Salespersondata> SearchEmployeeAgeRangeIncentives(List<IncentiveSalary.IncentiveData.Item> allEmployees)
        {
            var result = new List<Salespersondata>();
            Data.ReferencelistDataService referencelistDataService = new Data.ReferencelistDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            var config = referencelistDataService.Search(new Models.ReferencelistCreteria()
            {
                locationid = locationid,
                type = "INCENTIVE-EMPLOYEE-AGE"
            });

            if (config.Count == 0)
            {
                config = referencelistDataService.Search(new Models.ReferencelistCreteria()
                {
                    type = "INCENTIVE-EMPLOYEE-AGE"
                });

                if (config.Exists(e => e.locationid == -1))
                {
                    config.RemoveAll(e => e.locationid != -1);
                }
                else if (!config.All(e => e.locationid == -1))
                {
                    config.RemoveAll(e => e.locationid != config[0].locationid);
                }
            }
            if (config.Count > 0 && config.First().additionalinfo != null && config.First().additionalinfo.salesincentive != null && config.First().additionalinfo.salesincentive.Count > 0)
            {
                List<Range> rangelist = new List<Range>();
                foreach (var item in config.First().additionalinfo.salesincentive.OrderBy(e => e.from))
                {
                    if (item.from > 0 || item.to > 0)
                    {
                        var rng = new Range()
                        {
                            start = (int)item.from,
                            end = (int)item.to,
                            incentive = item.incentive,
                            prefix = item.prefix,
                            referenceid = item.referenceid,
                            value = item.value,
                            id = string.Format("{0}_{1}", item.referenceid, rangelist.Count.ToString())
                        };
                        rangelist.Add(rng);
                    }
                }

                var allsalesmenconfig = rangelist.FindAll(ex => ex.referenceid == 0).OrderBy(e => e.start).ToList();
                var allinchargeconfig = rangelist.FindAll(ex => ex.referenceid == -1).OrderBy(e => e.start).ToList();
                var allconfiguration = rangelist.FindAll(ex => ex.referenceid > 0).OrderBy(e => e.start).ToList();

                if (rangelist.Exists(e => e.referenceid > 0))
                {
                    var ageresut = ExecuteEmployeeAgeResult(rangelist.FindAll(e => e.referenceid > 0), "billitems.salesmanid",
                         IncentiveSource.EmployeeAgeRange, ApplyType.All, "");
                    allEmployees.ForEach(e =>
                    {
                        e.AddSalebyages(IncentiveSource.EmployeeAgeRange, ageresut.FindAll(ex => ex.salesmanid == e.employeeid));
                    });
                }

                if (rangelist.Exists(e => e.referenceid == 0))
                {
                    string condition = rangelist.Exists(e => e.referenceid > 0) ? "billitems.salesmanid not in (" + string.Join(", ", rangelist.FindAll(e => e.referenceid > 0).ConvertAll(e => e.referenceid)) + ")" : "";
                    var ageresut = ExecuteEmployeeAgeResult(rangelist.FindAll(e => e.referenceid == 0), "billitems.salesmanid",
                         IncentiveSource.EmployeeAgeRange, ApplyType.Employee, condition);

                    allEmployees.ForEach(e =>
                    {
                        e.AddSalebyages(IncentiveSource.EmployeeAgeRange, ageresut.FindAll(ex => ex.salesmanid == e.employeeid));
                    });
                }

                var allincharges = allEmployees.FindAll(e => e.IsIncharge || e.Incentivesource == IncentiveSalary.IncentiveData.Item.IncentiveSources.TeamSales);
                foreach (var incharge in allincharges)
                {
                    List<Range> employeeconfig = null;
                    if (allconfiguration.Exists(e => e.referenceid == incharge.employeeid))
                    {
                        employeeconfig = allconfiguration.FindAll(e => e.referenceid == incharge.employeeid);
                    }
                    else
                    {
                        employeeconfig = allinchargeconfig;
                    }
                    var teammembers = allEmployees.FindAll(e => e.inchargeid == incharge.employeeid);
                    if (incharge.Salebyages == null)
                        incharge.Salebyages = new List<IncentiveSalary.IncentiveData.salebyage>();

                    var teammembersalesvalue = new List<IncentiveSalary.IncentiveData.salebyage>();
                    teammembers.ForEach(x =>
                    {
                        if (x.Salebyages != null && x.Salebyages.Count > 0)
                            teammembersalesvalue.AddRange(x.Salebyages);
                    });

                    foreach (var econfig in employeeconfig)
                    {
                        string key = String.Format("{0} - {1}", econfig.start, econfig.end);
                        //var salevalue = teammembers.Sum(e => e.Salebyages.Sum(x => x.from == econfig.start && x.to == econfig.end ? x.salevalue : 0));
                        //var saleqty = teammembers.Sum(e => e.Salebyages.Sum(x => x.from == econfig.start && x.to == econfig.end ? x.incentivecount : 0));

                        var salevalue = teammembersalesvalue.Sum(x => x.from == econfig.start && x.to == econfig.end ? x.salevalue : 0);
                        var saleqty = teammembersalesvalue.Sum(x => x.from == econfig.start && x.to == econfig.end ? x.incentivecount : 0);

                        teammembersalesvalue.RemoveAll(x => x.from == econfig.start && x.to == econfig.end);

                        if (!incharge.Salebyages.Exists(e => e.from == econfig.start && e.to == econfig.end))
                        {
                            incharge.Salebyages.Add(new IncentiveSalary.IncentiveData.salebyage()
                            {
                                from = econfig.start,
                                to = econfig.end,
                                name = String.Format("{0} - {1}", econfig.start, econfig.end)
                            });
                        }
                        var salebyage = incharge.Salebyages.Find(e => e.from == econfig.start && e.to == econfig.end);
                        salebyage.salevalue += salevalue;
                        salebyage.incentivecount += saleqty;
                        salebyage.incentive += Math.Round(salevalue * econfig.incentive / 100, 0);
                    }

                    foreach (var econfig in teammembersalesvalue)
                    {
                        incharge.Salebyages.Add(econfig);
                    }
                    incharge.amount += incharge.Salebyages.Sum(e => e.incentive);
                    incharge.incentive += incharge.Salebyages.Sum(e => e.incentive);
                    incharge.employeeagerangeincentive += incharge.Salebyages.Sum(e => e.incentive);
                    incharge.salesvalue += incharge.Salebyages.Sum(e => e.salevalue);
                }


            }
            return result;
        }
        private List<Salespersondata> SearchEmployeeRoleIncentives(List<IncentiveSalary.IncentiveData.Item> allEmployees)
        {
            var result = new List<Salespersondata>();
            Data.ReferencelistDataService referencelistDataService = new Data.ReferencelistDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            var config = referencelistDataService.Search(new Models.ReferencelistCreteria()
            {
                locationid = locationid,
                type = "INCENTIVE-EMP-ROLE"
            });

            if (config.Count == 0)
            {
                config = referencelistDataService.Search(new Models.ReferencelistCreteria()
                {
                    type = "INCENTIVE-EMP-ROLE"
                });

                if (config.Exists(e => e.locationid == -1))
                {
                    config.RemoveAll(e => e.locationid != -1);
                }
                else if (!config.All(e => e.locationid == -1))
                {
                    config.RemoveAll(e => e.locationid != config[0].locationid);
                }
            }
            if (config.Count > 0 && config.First().additionalinfo != null && config.First().additionalinfo.salesincentive != null && config.First().additionalinfo.salesincentive.Count > 0)
            {
                List<Range> rangelist = new List<Range>();
                foreach (var item in config.First().additionalinfo.salesincentive.OrderBy(e => e.from))
                {
                    if (item.incentive > 0)
                    {
                        var rng = new Range()
                        {
                            incentive = item.incentive,
                            referenceid = item.referenceid,
                            id = string.Format("{0}_{1}", item.referenceid, rangelist.Count.ToString())
                        };
                        rangelist.Add(rng);
                    }
                }

                var salesmen = rangelist.FindAll(ex => ex.referenceid == 1).OrderBy(e => e.start).ToList();
                var salesmengroup = rangelist.FindAll(ex => ex.referenceid == 2).OrderBy(e => e.start).ToList();

                if (salesmen.Count > 0)
                {
                    foreach (var supervisor in allEmployees.FindAll(x => x.salesvalue > 0))
                    {
                        supervisor.rolesupervisorincentive += salesmen[0].incentive * supervisor.salesvalue / 100;
                        supervisor.amount += salesmen[0].incentive * supervisor.salesvalue / 100;
                    }
                }

                if (salesmengroup.Count > 0)
                {
                    var ageresut = ExecuteEmployeeSalesmenGroup();
                    foreach (var salesmenrole in ageresut)
                    {
                        if (allEmployees.Exists(x => x.employeeid == salesmenrole.salesmanid))
                        {
                            var ae = allEmployees.Find(x => x.employeeid == salesmenrole.salesmanid);
                            ae.rolesalesmenincentive += (salesmengroup[0].incentive * salesmenrole.salevalue / 100);
                            ae.amount += (salesmengroup[0].incentive * salesmenrole.salevalue / 100);
                        }
                    }
                }
                //foreach (var employeeconfig in allEmployees)
                //{

                //    var teammembers = allEmployees.FindAll(e => e.inchargeid == incharge.employeeid);
                //    if (incharge.Salebyages == null)
                //        incharge.Salebyages = new List<IncentiveSalary.IncentiveData.salebyage>();

                //    var teammembersalesvalue = new List<IncentiveSalary.IncentiveData.salebyage>();
                //    teammembers.ForEach(x =>
                //    {
                //        if (x.Salebyages != null && x.Salebyages.Count > 0)
                //            teammembersalesvalue.AddRange(x.Salebyages);
                //    });

                //    foreach (var econfig in employeeconfig)
                //    {
                //        string key = String.Format("{0} - {1}", econfig.start, econfig.end);
                //        //var salevalue = teammembers.Sum(e => e.Salebyages.Sum(x => x.from == econfig.start && x.to == econfig.end ? x.salevalue : 0));
                //        //var saleqty = teammembers.Sum(e => e.Salebyages.Sum(x => x.from == econfig.start && x.to == econfig.end ? x.incentivecount : 0));

                //        var salevalue = teammembersalesvalue.Sum(x => x.from == econfig.start && x.to == econfig.end ? x.salevalue : 0);
                //        var saleqty = teammembersalesvalue.Sum(x => x.from == econfig.start && x.to == econfig.end ? x.incentivecount : 0);

                //        teammembersalesvalue.RemoveAll(x => x.from == econfig.start && x.to == econfig.end);

                //        if (!incharge.Salebyages.Exists(e => e.from == econfig.start && e.to == econfig.end))
                //        {
                //            incharge.Salebyages.Add(new IncentiveSalary.IncentiveData.salebyage()
                //            {
                //                from = econfig.start,
                //                to = econfig.end,
                //                name = String.Format("{0} - {1}", econfig.start, econfig.end)
                //            });
                //        }
                //        var salebyage = incharge.Salebyages.Find(e => e.from == econfig.start && e.to == econfig.end);
                //        salebyage.salevalue += salevalue;
                //        salebyage.incentivecount += saleqty;
                //        salebyage.incentive += Math.Round(salevalue * econfig.incentive / 100, 0);
                //    }

                //    foreach (var econfig in teammembersalesvalue)
                //    {
                //        incharge.Salebyages.Add(econfig);
                //    }
                //    incharge.amount += incharge.Salebyages.Sum(e => e.incentive);
                //    incharge.incentive += incharge.Salebyages.Sum(e => e.incentive);
                //    incharge.employeeagerangeincentive += incharge.Salebyages.Sum(e => e.incentive);
                //    incharge.salesvalue += incharge.Salebyages.Sum(e => e.salevalue);
                //}


            }
            return result;
        }
        private List<Salespersondata> SearchProductAgeRangeIncentives(List<IncentiveSalary.IncentiveData.Item> allEmployees)
        {
            var result = new List<Salespersondata>();
            Data.ReferencelistDataService referencelistDataService = new Data.ReferencelistDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            var config = referencelistDataService.Search(new Models.ReferencelistCreteria()
            {
                locationid = locationid,
                type = "INCENTIVE-PRODUCT-AGE"
            });

            if (config.Count == 0)
            {
                config = referencelistDataService.Search(new Models.ReferencelistCreteria()
                {
                    type = "INCENTIVE-PRODUCT-AGE"
                });

                if (config.Exists(e => e.locationid == -1))
                {
                    config.RemoveAll(e => e.locationid != -1);
                }
                else if (!config.All(e => e.locationid == -1))
                {
                    config.RemoveAll(e => e.locationid != config[0].locationid);
                }
            }

            if (config.Count > 0 && config.First().additionalinfo != null && config.First().additionalinfo.salesincentive != null && config.First().additionalinfo.salesincentive.Count > 0)
            {
                List<Range> rangelist = new List<Range>();
                foreach (var item in config.First().additionalinfo.salesincentive.OrderBy(e => e.from))
                {
                    if (item.from > 0 || item.to > 0)
                    {
                        var rng = new Range()
                        {
                            start = (int)item.from,
                            end = (int)item.to,
                            incentive = item.incentive,
                            prefix = item.prefix,
                            referenceid = item.referenceid,
                            value = item.value,
                            id = string.Format("{0}_{1}", item.referenceid, rangelist.Count.ToString())
                        };
                        rangelist.Add(rng);
                    }
                }

                if (rangelist.Exists(e => e.referenceid > 0))
                {
                    var ageresut = ExecuteProductAgeResult(rangelist.FindAll(e => e.referenceid > 0), "stock.productid",
                         IncentiveSource.ProductAgeRange, ApplyType.All, "");
                    allEmployees.ForEach(e =>
                    {
                        e.AddSalebyages(IncentiveSource.ProductAgeRange, ageresut.FindAll(ex => ex.salesmanid == e.employeeid));
                        //if (e.ValueType == IncentiveSalary.IncentiveData.Item.ValueTypes.Value)
                        //{
                        //    var incentive = ageresut.FindAll(ex => ex.salesmanid == e.employeeid).Sum(ex =>
                        //    {
                        //        if (ex.range.value)
                        //            return ex.range.incentive * ex.salevalue;
                        //        return ex.range.incentive * ex.salevalue / 100;
                        //    });
                        //    e.productagerangeincentive += incentive;
                        //    e.incentive += incentive;
                        //    e.amount += incentive;
                        //}
                        //else if (e.ValueType == IncentiveSalary.IncentiveData.Item.ValueTypes.Quantity)
                        //{
                        //    var incentive = ageresut.FindAll(ex => ex.salesmanid == e.employeeid).Sum(ex => ex.range.incentive * ex.saleqty);
                        //    e.incentive += incentive;
                        //    e.amount += incentive;
                        //    e.productagerangeincentive += incentive;
                        //}
                    });
                }

                if (rangelist.Exists(e => e.referenceid == 0))
                {
                    string condition = rangelist.Exists(e => e.referenceid > 0) ? "stock.productid not in (" + string.Join(", ", rangelist.FindAll(e => e.referenceid > 0).ConvertAll(e => e.referenceid)) + ")" : "";
                    var ageresut = ExecuteProductAgeResult(rangelist.FindAll(e => e.referenceid == 0), "stock.productid",
                         IncentiveSource.ProductAgeRange, ApplyType.Employee, condition);

                    allEmployees.ForEach(e =>
                    {
                        e.AddSalebyages(IncentiveSource.ProductAgeRange, ageresut.FindAll(ex => ex.salesmanid == e.employeeid));
                        //if (e.ValueType == IncentiveSalary.IncentiveData.Item.ValueTypes.Value)
                        //{
                        //    var incentive = ageresut.FindAll(ex => ex.salesmanid == e.employeeid).Sum(ex =>
                        //    {
                        //        var salevalue = ex.salevalue;
                        //        if (ex.range.value)
                        //            return ex.range.incentive * salevalue;
                        //        return ex.range.incentive * salevalue / 100;
                        //    });
                        //    e.incentive += incentive;
                        //    e.amount += incentive;
                        //    e.productagerangeincentive += incentive;
                        //}
                        //else if (e.ValueType == IncentiveSalary.IncentiveData.Item.ValueTypes.Quantity)
                        //{
                        //    var incentive = ageresut.FindAll(ex => ex.salesmanid == e.employeeid).Sum(ex =>
                        //    {
                        //        return ex.range.incentive * ex.saleqty;
                        //    });
                        //    e.incentive += incentive;
                        //    e.amount += incentive;
                        //    e.productagerangeincentive += incentive;
                        //}
                    });
                }


            }

            return result;
        }

        private List<Salespersondata> SearchProductPriceRangeIncentives(List<IncentiveSalary.IncentiveData.Item> allEmployees)
        {
            var result = new List<Salespersondata>();
            Data.ReferencelistDataService referencelistDataService = new Data.ReferencelistDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            var config = referencelistDataService.Search(new Models.ReferencelistCreteria()
            {
                locationid = locationid,
                type = "INCENTIVE-PRODUCT"
            });

            if (config.Count == 0)
            {
                config = referencelistDataService.Search(new Models.ReferencelistCreteria()
                {
                    type = "INCENTIVE-PRODUCT"
                });

                if (config.Exists(e => e.locationid == -1))
                {
                    config.RemoveAll(e => e.locationid != -1);
                }
                else if (!config.All(e => e.locationid == -1))
                {
                    config.RemoveAll(e => e.locationid != config[0].locationid);
                }
            }

            if (config.Count > 0 && config.First().additionalinfo != null && config.First().additionalinfo.salesincentive != null && config.First().additionalinfo.salesincentive.Count > 0)
            {
                List<Range> rangelist = new List<Range>();
                foreach (var item in config.First().additionalinfo.salesincentive.OrderBy(e => e.from))
                {
                    if (item.from > 0 || item.to > 0)
                    {
                        var rng = new Range()
                        {
                            start = (int)item.from,
                            end = (int)item.to,
                            incentive = item.incentive,
                            prefix = item.prefix,
                            referenceid = item.referenceid,
                            value = item.value,
                            id = string.Format("{0}_{1}", item.referenceid, rangelist.Count.ToString())
                        };
                        rangelist.Add(rng);
                    }
                }

                if (rangelist.Exists(e => e.referenceid > 0))
                {
                    var ageresut = ExecutePriceResult(rangelist.FindAll(e => e.referenceid > 0), "stock.productid",
                         IncentiveSource.ProductPriceRange, ApplyType.All, "");
                    allEmployees.ForEach(e =>
                    {
                        e.AddSalebyages(IncentiveSource.ProductPriceRange, ageresut.FindAll(ex => ex.salesmanid == e.employeeid));
                        //if (e.ValueType == IncentiveSalary.IncentiveData.Item.ValueTypes.Value)
                        //{
                        //    var incentive = ageresut.FindAll(ex => ex.salesmanid == e.employeeid).Sum(ex =>
                        //    {
                        //        if (ex.range.value)
                        //            return ex.range.incentive * ex.salevalue;
                        //        return ex.range.incentive * ex.salevalue / 100;
                        //    });
                        //    e.incentive += incentive;
                        //    e.amount += incentive;
                        //    e.productpricerangeincentive += incentive;
                        //}
                        //else if (e.ValueType == IncentiveSalary.IncentiveData.Item.ValueTypes.Quantity)
                        //{
                        //    var incentive = ageresut.FindAll(ex => ex.salesmanid == e.employeeid).Sum(ex => ex.range.incentive * ex.saleqty);
                        //    e.incentive += incentive;
                        //    e.amount += incentive;
                        //    e.productpricerangeincentive += incentive;
                        //}
                    });
                }

                if (rangelist.Exists(e => e.referenceid == 0))
                {
                    var condition = rangelist.Exists(e => e.referenceid > 0) ? " stock.productid not in (" + string.Join(",", rangelist.FindAll(e => e.referenceid > 0).ConvertAll(e => e.referenceid)) + ")" : "";

                    var ageresut = ExecutePriceResult(rangelist.FindAll(e => e.referenceid == 0), "stock.productid",
                         IncentiveSource.ProductPriceAgeRange, ApplyType.Employee, condition
                         );

                    allEmployees.ForEach(e =>
                    {
                        e.AddSalebyages(IncentiveSource.ProductPriceAgeRange, ageresut.FindAll(ex => ex.salesmanid == e.employeeid));
                        //if (e.ValueType == IncentiveSalary.IncentiveData.Item.ValueTypes.Value)
                        //{
                        //    var incentive = ageresut.FindAll(ex => ex.salesmanid == e.employeeid).Sum(ex =>
                        //    {
                        //        if (ex.range.value)
                        //            return ex.range.incentive * ex.salevalue;
                        //        return ex.range.incentive * ex.salevalue / 100;
                        //    });
                        //    e.incentive += incentive;
                        //    e.amount += incentive;
                        //    e.productpricerangeincentive += incentive;
                        //}
                        //else if (e.ValueType == IncentiveSalary.IncentiveData.Item.ValueTypes.Quantity)
                        //{
                        //    var incentive = ageresut.FindAll(ex => ex.salesmanid == e.employeeid).Sum(ex => ex.range.incentive * ex.saleqty);
                        //    e.incentive += incentive;
                        //    e.amount += incentive;
                        //    e.productpricerangeincentive += incentive;
                        //}
                    });
                }
            }

            return result;
        }

        private List<Salespersondata> SearchEmployeeSectionIncentives(List<IncentiveSalary.IncentiveData.Item> allEmployees)
        {
            var result = new List<Salespersondata>();
            Data.ReferencelistDataService referencelistDataService = new Data.ReferencelistDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            var config = referencelistDataService.Search(new Models.ReferencelistCreteria()
            {
                locationid = locationid,
                type = "INCENTIVE-EMP-SECTION"
            });

            if (config.Count == 0)
            {
                config = referencelistDataService.Search(new Models.ReferencelistCreteria()
                {
                    type = "INCENTIVE-EMP-SECTION"
                });

                if (config.Exists(e => e.locationid == -1))
                {
                    config.RemoveAll(e => e.locationid != -1);
                }
                else if (!config.All(e => e.locationid == -1))
                {
                    config.RemoveAll(e => e.locationid != config[0].locationid);
                }
            }

            if (config.Count > 0)
            {
                var configlist = config[0].additionalinfo.salesincentive;

                string query = @"select sum(billitems.receivable) as salevalue,
            sum(billitems.qty) as saleqty,
            salescounterid from billitems
            inner join bill on bill.id = billitems.billid
            left outer join stock on stock.id = billitems.stockid
            left outer join products on products.id = stock.productid
            where bill.isactive=true and billitems.isactive=true  and billitems.qty!=0 and billdate>=@fromdate and billdate<@todate 
            and bill.locationid=@locationid and bill.billtype != 9 and bill.isb2b=false 
            group by salescounterid";

                SortedList<string, object> param = new SortedList<string, object>();
                param.Add("salarymonth", fromdate);
                param.Add("fromdate", fromdate.Date);
                param.Add("employeeid", employeeid);
                param.Add("locationid", locationid);
                param.Add("sectionid", sectionid);
                param.Add("todate", todate.Date.AddDays(1));


                if (InfyPOS.Services.ConfigManager.Instance(organizationid).Config.payroll_incentive_addb2b)
                    query = query.Replace("bill.isb2b=false", "bill.isexport=false");

                if (InfyPOS.Services.ConfigManager.Instance(organizationid).Config.payroll_incentive_addexport)
                    query = query.Replace("and bill.isexport=false", "");

                InfyPOS.Common.BaseData baseData = new Common.BaseData() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
                if (InfyPOS.Services.ConfigManager.Instance(organizationid).Config.system_isb2bapplication)
                    query = query.Replace("and bill.isb2b=false", "");
                if (locationid <= 0)
                    query = query.Replace("and bill.locationid=@locationid", "");


                var resultlist = new List<Salespersondata>();
                using (System.Data.IDataReader reader = baseData.ExecuteReader(query, param, 10000))
                {
                    while (reader.Read())
                    {
                        var salescounterid = reader["salescounterid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["salescounterid"]);
                        var salevalue = reader["salevalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["salevalue"]);
                        var saleqty = reader["saleqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["saleqty"]);
                        resultlist.Add(new Salespersondata()
                        {
                            salescounterid = salescounterid,
                            saleqty = saleqty,
                            salevalue = salevalue
                        });
                    }
                }

                foreach (var citems in configlist.GroupBy(x => x.referenceid))
                {
                    foreach (var citem in citems)
                    {
                        if (resultlist.Exists(x => x.salescounterid == citem.targetsource))
                        {
                            var sdata = resultlist.Find(x => x.salescounterid == citem.targetsource);

                            result.Add(new Salespersondata()
                            {
                                salesmanid = citems.Key,
                                salevalue = sdata.salevalue,
                                saleqty = sdata.saleqty,
                                incentive = (citem.incentive * sdata.salevalue / 100),
                                Source = IncentiveSource.EmployeeSection
                            });

                        }
                    }
                }
            }

            allEmployees.ForEach(x =>
            {
                if (result.Exists(y => y.salesmanid == x.employeeid))
                {
                    x.salesqty += result.Sum(y => y.salesmanid == x.employeeid ? y.saleqty : 0);
                    x.salesvalue += result.Sum(y => y.salesmanid == x.employeeid ? y.salevalue : 0);
                    var sectionincentive = result.Sum(y => y.salesmanid == x.employeeid ? y.incentive : 0);
                    x.sectionincentive = Math.Round(sectionincentive, 0);
                    x.amount += x.sectionincentive;
                }
            });

            return result;
        }
        private List<Salespersondata> SearchSectionAgeRangeIncentives(List<IncentiveSalary.IncentiveData.Item> allEmployees)
        {
            var result = new List<Salespersondata>();
            Data.ReferencelistDataService referencelistDataService = new Data.ReferencelistDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            var config = referencelistDataService.Search(new Models.ReferencelistCreteria()
            {
                locationid = locationid,
                type = "INCENTIVE-SECTION-AGE"
            });

            if (config.Count == 0)
            {
                config = referencelistDataService.Search(new Models.ReferencelistCreteria()
                {
                    type = "INCENTIVE-SECTION-AGE"
                });

                if (config.Exists(e => e.locationid == -1))
                {
                    config.RemoveAll(e => e.locationid != -1);
                }
                else if (!config.All(e => e.locationid == -1))
                {
                    config.RemoveAll(e => e.locationid != config[0].locationid);
                }
            }

            if (config.Count > 0 && config.First().additionalinfo != null && config.First().additionalinfo.salesincentive != null && config.First().additionalinfo.salesincentive.Count > 0)
            {
                List<Range> rangelist = new List<Range>();
                foreach (var item in config.First().additionalinfo.salesincentive.OrderBy(e => e.from))
                {
                    if (item.from > 0 || item.to > 0)
                    {
                        var rng = new Range()
                        {
                            start = (int)item.from,
                            end = (int)item.to,
                            incentive = item.incentive,
                            prefix = item.prefix,
                            referenceid = item.referenceid,
                            value = item.value,
                            id = string.Format("{0}_{1}", item.referenceid, rangelist.Count.ToString())
                        };
                        rangelist.Add(rng);
                    }
                }

                if (rangelist.Exists(e => e.referenceid > 0))
                {
                    var ageresut = ExecuteProductAgeResult(rangelist.FindAll(e => e.referenceid > 0), "products.salescounterid",
                         IncentiveSource.SectionAgeRange, ApplyType.All, "");
                    allEmployees.ForEach(e =>
                    {

                        e.AddSalebyages(IncentiveSource.SectionAgeRange, ageresut.FindAll(ex => ex.salesmanid == e.employeeid));

                        //if (e.ValueType == IncentiveSalary.IncentiveData.Item.ValueTypes.Value)
                        //{
                        //    var incentive = ageresut.FindAll(ex => ex.salesmanid == e.employeeid).Sum(ex =>
                        //    {
                        //        if (ex.range.value)
                        //            return ex.range.incentive * ex.salevalue;
                        //        return ex.range.incentive * ex.salevalue / 100;
                        //    });
                        //    e.incentive += incentive;
                        //    e.amount += incentive;
                        //    e.sectionagerangeincentive += incentive;
                        //}
                        //else if (e.ValueType == IncentiveSalary.IncentiveData.Item.ValueTypes.Quantity)
                        //{
                        //    var incentive = ageresut.FindAll(ex => ex.salesmanid == e.employeeid).Sum(ex => ex.range.incentive * ex.saleqty);
                        //    e.incentive += incentive;
                        //    e.amount += incentive;
                        //    e.sectionagerangeincentive += incentive;
                        //}
                    });
                }

                if (rangelist.Exists(e => e.referenceid == 0))
                {
                    var ageresut = ExecuteProductAgeResult(rangelist.FindAll(e => e.referenceid == 0), "products.salescounterid",
                         IncentiveSource.SectionAgeRange, ApplyType.Employee,
                         " salescounterid not in (" + string.Join(",", rangelist.FindAll(e => e.referenceid > 0).ConvertAll(e => e.referenceid)) + ")");

                    allEmployees.ForEach(e =>
                    {
                        e.AddSalebyages(IncentiveSource.SectionAgeRange, ageresut.FindAll(ex => ex.salesmanid == e.employeeid));
                        //if (e.ValueType == IncentiveSalary.IncentiveData.Item.ValueTypes.Value)
                        //{
                        //    var incentive = ageresut.FindAll(ex => ex.salesmanid == e.employeeid).Sum(ex =>
                        //    {
                        //        if (ex.range.value)
                        //            return ex.range.incentive * ex.salevalue;
                        //        return ex.range.incentive * ex.salevalue / 100;
                        //    });
                        //    e.incentive += incentive;
                        //    e.amount += incentive;
                        //    e.sectionagerangeincentive += incentive;
                        //}
                        //else if (e.ValueType == IncentiveSalary.IncentiveData.Item.ValueTypes.Quantity)
                        //{
                        //    var incentive = ageresut.FindAll(ex => ex.salesmanid == e.employeeid).Sum(ex => ex.range.incentive * ex.saleqty);
                        //    e.incentive += incentive;
                        //    e.amount += incentive;
                        //    e.sectionagerangeincentive += incentive;
                        //}
                    });
                }
            }
            return result;
        }
        private List<Salespersondata> SearchSectionPriceRangeIncentives(List<IncentiveSalary.IncentiveData.Item> allEmployees)
        {
            var result = new List<Salespersondata>();
            Data.ReferencelistDataService referencelistDataService = new Data.ReferencelistDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            var config = referencelistDataService.Search(new Models.ReferencelistCreteria()
            {
                locationid = locationid,
                type = "INCENTIVE-SECTION"
            });

            if (config.Count == 0)
            {
                config = referencelistDataService.Search(new Models.ReferencelistCreteria()
                {
                    type = "INCENTIVE-SECTION"
                });

                if (config.Exists(e => e.locationid == -1))
                {
                    config.RemoveAll(e => e.locationid != -1);
                }
                else if (!config.All(e => e.locationid == -1))
                {
                    config.RemoveAll(e => e.locationid != config[0].locationid);
                }
            }

            if (config.Count > 0 && config.First().additionalinfo != null && config.First().additionalinfo.salesincentive != null && config.First().additionalinfo.salesincentive.Count > 0)
            {
                List<Range> rangelist = new List<Range>();
                foreach (var item in config.First().additionalinfo.salesincentive.OrderBy(e => e.from))
                {
                    if (item.from > 0 || item.to > 0)
                    {
                        var rng = new Range()
                        {
                            start = (int)item.from,
                            end = (int)item.to,
                            incentive = item.incentive,
                            prefix = item.prefix,
                            referenceid = item.referenceid,
                            value = item.value,
                            id = string.Format("{0}_{1}", item.referenceid, rangelist.Count.ToString())
                        };
                        rangelist.Add(rng);
                    }
                }

                if (rangelist.Exists(e => e.referenceid > 0))
                {
                    var ageresut = ExecutePriceResult(rangelist.FindAll(e => e.referenceid > 0), "salescounterid",
                         IncentiveSource.SectionPriceRange, ApplyType.All, "");
                    allEmployees.ForEach(e =>
                    {
                        e.AddSalebyages(IncentiveSource.SectionPriceRange, ageresut.FindAll(ex => ex.salesmanid == e.employeeid));
                        //if (e.ValueType == IncentiveSalary.IncentiveData.Item.ValueTypes.Value)
                        //{
                        //    var incentive = ageresut.FindAll(ex => ex.salesmanid == e.employeeid).Sum(ex =>
                        //    {
                        //        if (ex.range.value)
                        //            return ex.range.incentive * ex.salevalue;
                        //        return ex.range.incentive * ex.salevalue / 100;
                        //    });
                        //    e.incentive += incentive;
                        //    e.amount += incentive;
                        //    e.sectionpricerange += incentive;
                        //}
                        //else if (e.ValueType == IncentiveSalary.IncentiveData.Item.ValueTypes.Quantity)
                        //{
                        //    var incentive = ageresut.FindAll(ex => ex.salesmanid == e.employeeid).Sum(ex => ex.range.incentive * ex.saleqty);
                        //    e.incentive += incentive;
                        //    e.amount += incentive;
                        //    e.sectionpricerange += incentive;
                        //}
                    });
                }

                if (rangelist.Exists(e => e.referenceid == 0))
                {
                    var ageresut = ExecutePriceResult(rangelist.FindAll(e => e.referenceid == 0), "salescounterid",
                         IncentiveSource.SectionPriceRange, ApplyType.Employee,
                         " salescounterid in (" + string.Join(",", rangelist.FindAll(e => e.referenceid > 0).ConvertAll(e => e.referenceid)) + ")");

                    allEmployees.ForEach(e =>
                    {
                        e.AddSalebyages(IncentiveSource.SectionPriceRange, ageresut.FindAll(ex => ex.salesmanid == e.employeeid));
                        //if (e.ValueType == IncentiveSalary.IncentiveData.Item.ValueTypes.Value)
                        //{
                        //    var incentive = ageresut.FindAll(ex => ex.salesmanid == e.employeeid).Sum(ex =>
                        //    {
                        //        if (ex.range.value)
                        //            return ex.range.incentive * ex.salevalue;
                        //        return ex.range.incentive * ex.salevalue / 100;
                        //    });
                        //    e.incentive += incentive;
                        //    e.amount += incentive;
                        //    e.sectionpricerange += incentive;
                        //}
                        //else if (e.ValueType == IncentiveSalary.IncentiveData.Item.ValueTypes.Quantity)
                        //{
                        //    var incentive = ageresut.FindAll(ex => ex.salesmanid == e.employeeid).Sum(ex => ex.range.incentive * ex.saleqty);
                        //    e.incentive += incentive;
                        //    e.amount += incentive;
                        //    e.sectionpricerange += incentive;
                        //}
                    });
                }
            }
            return result;
        }
        List<Range> prefixrangelist = new List<Range>();
        List<Range> productrangelist = new List<Range>();
        private List<Salespersondata> SearchPrefixIncentives()
        {
            var result = new List<Salespersondata>();
            Data.ReferencelistDataService referencelistDataService = new Data.ReferencelistDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            var config = referencelistDataService.Search(new Models.ReferencelistCreteria()
            {
                locationid = locationid,
                type = "INCENTIVE-BARCODE"
            });

            if (config.Count == 0)
            {
                config = referencelistDataService.Search(new Models.ReferencelistCreteria()
                {
                    type = "INCENTIVE-BARCODE"
                });

                if (config.Exists(e => e.locationid == -1))
                {
                    config.RemoveAll(e => e.locationid != -1);
                }
                else if (!config.All(e => e.locationid == -1))
                {
                    config.RemoveAll(e => e.locationid != config[0].locationid);
                }
            }


            if (config.Count > 0 && config.First().additionalinfo != null && config.First().additionalinfo.salesincentive != null && config.First().additionalinfo.salesincentive.Count > 0)
            {
                prefixrangelist = new List<Range>();
                foreach (var item in config.First().additionalinfo.salesincentive.OrderBy(e => e.from))
                {
                    if (!string.IsNullOrEmpty(item.prefix) &&
                            (item.from > 0 || item.to > 0))
                    {
                        var rng = new Range()
                        {
                            start = (int)item.from,
                            end = (int)item.to,
                            incentive = item.incentive,
                            prefix = item.prefix,
                            value = item.value,
                            id = prefixrangelist.Count.ToString()
                        };
                        prefixrangelist.Add(rng);
                    }
                }

                if (prefixrangelist.Count == 0) return result;
                string productlevelcondition = getIncentiveProductLevelCase(productrangelist, "(billitems.receivable/billitems.qty)", "::numeric");
                string field = getIncentivePrefixCase(prefixrangelist, "(billitems.receivable/billitems.qty)", "::numeric");
                string field1 = getSaleValuePrefixCase(prefixrangelist, "(billitems.receivable/billitems.qty)", "::numeric");
                string field2 = getCountPrefixCase(prefixrangelist, "(billitems.receivable/billitems.qty)", "::numeric");
                string field3 = getNamePrefixCase(prefixrangelist, "(billitems.receivable/billitems.qty)", "::numeric");
                string query = string.Format(@"

                                with salaryheader as (
			                    SELECT id, employeeid, fromdate, inchargeid, row_number() OVER (PARTITION BY employeeid ORDER BY fromdate DESC,id DESC) as ranknumber  
			                    FROM employeesalaryheader where fromdate<=@todate),
                                markedlist as
								(
									select distinct stockmarker.markerid,stockmarker.stockid from stockmarker
                                    left outer join incentiveconfig on incentiveconfig.salablegoodsid = stockmarker.salablegoodsid and incentiveconfig.isactive=true
                                    where markerid in ({5})
									and stockmarker.isactive = true and incentiveconfig.salablegoodsid is null
								),
								barcodelist as
								(
									select distinct salablegoodsid from incentiveconfig where 
									incentiveconfig.isactive = true
								)

                                select sum({0}) as incentive,sum({1}) as salevalue,sum({2}) as salecount,
                                {3} as prefix,salesmanid from billitems
                                inner join bill on bill.id = billitems.billid
                                left outer join stock on stock.id = billitems.stockid
                                left outer join products on products.id = stock.productid
                                left outer join markedlist on markedlist.stockid = billitems.stockid
                                left outer join barcodelist on barcodelist.salablegoodsid = billitems.salablegoodsid
                                where bill.isactive=true and billitems.isactive=true  and  billdate>=@fromdate and billdate<@todate 
                                and billitems.receivable!=0 and bill.locationid=@locationid and bill.billtype != 9 and bill.isb2b=false and salescounterid=@sectionid
                                and (salesmanid=@employeeid or salesmanid in (select employeeid from salaryheader where ranknumber = 1 and inchargeid=@employeeid)) 
                                and markedlist.stockid is null and barcodelist.salablegoodsid is null and ({3}) != 'UNKNOWN'
                                {4}
                                group by salesmanid,{3}", field, field1, field2, field3,
                                productrangelist.Count > 0 ? (" and " + productlevelcondition + "=0::numeric") : "",
                                string.Join(",", GetStockMarkerList()));

                SortedList<string, object> param = new SortedList<string, object>();


                param.Add("salarymonth", fromdate);
                param.Add("fromdate", fromdate.Date);
                param.Add("employeeid", employeeid);
                param.Add("locationid", locationid);
                param.Add("sectionid", sectionid);
                param.Add("todate", todate.Date.AddDays(1));

                if (employeeid == 0)
                {
                    query = query.Replace("and (salesmanid=@employeeid or salesmanid in (select employeeid from salaryheader where ranknumber = 1 and inchargeid=@employeeid))", "");
                }

                if (locationid == 0)
                {
                    if (accessiblelocation == null || accessiblelocation.Contains(-1))
                    {
                        query = query.Replace("and bill.locationid=@locationid", "");
                    }
                    else
                    {
                        query = query.Replace("and bill.locationid=@locationid", "and bill.locationid in (" + string.Join(",", accessiblelocation) + ")");
                    }
                }

                if (InfyPOS.Services.ConfigManager.Instance(organizationid).Config.payroll_incentive_addb2b)
                    query = query.Replace("bill.isb2b=false", "bill.isexport=false");

                if (InfyPOS.Services.ConfigManager.Instance(organizationid).Config.payroll_incentive_addexport)
                    query = query.Replace("and bill.isexport=false", "");

                InfyPOS.Common.BaseData baseData = new Common.BaseData() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
                if (InfyPOS.Services.ConfigManager.Instance(organizationid).Config.system_isb2bapplication)
                    query = query.Replace("and bill.isb2b=false", "");
                if (sectionid <= 0)
                    query = query.Replace("and salescounterid=@sectionid", "");


                using (System.Data.IDataReader reader = baseData.ExecuteReader(query, param, 10000))
                {
                    while (reader.Read())
                    {
                        result.Add(new Salespersondata()
                        {
                            salesmanid = reader["salesmanid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["salesmanid"]),
                            incentive = reader["incentive"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["incentive"]),
                            salevalue = reader["salevalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["salevalue"]),
                            incentivecount = reader["salecount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["salecount"]),
                            name = reader["prefix"] == DBNull.Value ? "UNKNOWN" : reader["prefix"].ToString(),
                            Source = IncentiveSource.Prefix
                        });
                    }
                }
            }
            return result;
        }

        public List<long> GetStockMarkerList()
        {
            Data.ReferencelistDataService referencelistDataService = new Data.ReferencelistDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            var config = referencelistDataService.Search(new Models.ReferencelistCreteria()
            {
                type = "MARKER"
            }).FindAll(e => e.additionalinfo != null && e.additionalinfo.rewardpoint > 0).ConvertAll(e => new { name = e.name, id = e.id, rewardpoint = e.additionalinfo.rewardpoint });
            var list = config.ConvertAll(x => x.id);
            if (list.Count == 0)
                list.Add(0);
            return list;
        }
        private List<Salespersondata> SearchStockMarkerIncentives()
        {
            Data.ReferencelistDataService referencelistDataService = new Data.ReferencelistDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            var config = referencelistDataService.Search(new Models.ReferencelistCreteria()
            {
                type = "MARKER"
            }).FindAll(e => e.additionalinfo != null && e.additionalinfo.rewardpoint > 0).ConvertAll(e => new { name = e.name, id = e.id, rewardpoint = e.additionalinfo.rewardpoint });
            if (config.Count == 0) return new List<Salespersondata>();

            var result = new List<Salespersondata>();
            string query = string.Format(@"
                                with salaryheader as (
			                    SELECT id, employeeid, fromdate, inchargeid, row_number() OVER (PARTITION BY employeeid ORDER BY fromdate DESC,id DESC) as ranknumber  
			                    FROM employeesalaryheader where fromdate<=@todate),
                                markedlist as
								(
									select distinct stockmarker.markerid,stockmarker.stockid from stockmarker
                                    left outer join incentiveconfig on incentiveconfig.salablegoodsid = stockmarker.salablegoodsid and incentiveconfig.isactive=true
                                    where markerid in ({0})
									and stockmarker.isactive = true 
                                    --and incentiveconfig.salablegoodsid is null
								),
                                barcodelist as
								(
									select distinct salablegoodsid from incentiveconfig where 
									incentiveconfig.isactive = true
								)

                                select sum(billitems.qty) as qty,sum(billitems.receivable) as salevalue,
                                markedlist.markerid,salesmanid,
                                sum(case when barcodelist.salablegoodsid is null then 0 else billitems.qty end) bqty,
                                sum(case when barcodelist.salablegoodsid is null then 0 else billitems.receivable end) bamt  from billitems
                                inner join bill on bill.id = billitems.billid
                                inner join stock on stock.id = billitems.stockid
                                inner join products on products.id = stock.productid
                                inner join markedlist on  markedlist.stockid = billitems.stockid 
                                inner join referencelist on referencelist.id = markedlist.markerid and referencelist.isactive = true
                                left outer join barcodelist on barcodelist.salablegoodsid = billitems.salablegoodsid
                                where bill.isactive=true and billitems.isactive=true and billdate>=@fromdate and billdate<@todate 
                                and billitems.receivable!=0 and bill.locationid=@locationid and bill.billtype != 9 and bill.isb2b=false and salescounterid=@sectionid
                                and (salesmanid=@employeeid or salesmanid in (select employeeid from salaryheader where ranknumber = 1 and inchargeid=@employeeid)) 
                                group by salesmanid,markedlist.markerid", string.Join(",", config.ConvertAll(e => e.id)));

            SortedList<string, object> param = new SortedList<string, object>();

            param.Add("salarymonth", fromdate);
            param.Add("fromdate", fromdate.Date);
            param.Add("employeeid", employeeid);
            param.Add("locationid", locationid);
            param.Add("sectionid", sectionid);
            param.Add("todate", todate.Date.AddDays(1));

            if (employeeid > 0)
            {
                string employeidlistquery = @"            
                with salaryheader as (
			        SELECT id, employeeid, fromdate, inchargeid, row_number() OVER (PARTITION BY employeeid ORDER BY fromdate DESC,id DESC) as ranknumber  
			        FROM employeesalaryheader where fromdate<=@todate
                )
            select employeeid from salaryheader where ranknumber = 1 and inchargeid=@employeeid;";
                List<long> employeidlist = new List<long>();
                employeidlist.Add(employeeid);
                using (var reader = ExecuteReader(employeidlistquery, param))
                {
                    while (reader.Read())
                    {
                        if (!reader.IsDBNull(0))
                            employeidlist.Add(reader.GetInt64(0));
                    }
                }
                if (employeidlist.Count == 1)
                {
                    query = query.Replace("and (employee.id = @employeeid or employee.id in (select employeeid from salaryheader where inchargeid=@employeeid))", "and (employee.id = @employeeid)");
                    query = query.Replace("and (salesmanid=@employeeid or salesmanid in (select employeeid from salaryheader where ranknumber = 1 and inchargeid=@employeeid))", "and (salesmanid=@employeeid)");
                    query = query.Replace("and (employee.id = @employeeid or employee.id in (select employeeid from salaryheader where ranknumber = 1 and inchargeid=@employeeid))", "and (employee.id = @employeeid)");
                }
                else
                {
                    query = query.Replace("and (employee.id = @employeeid or employee.id in (select employeeid from salaryheader where inchargeid=@employeeid))", string.Format("and (employee.id in ({0})", string.Join(",", employeidlist)));
                    query = query.Replace("and (salesmanid=@employeeid or salesmanid in (select employeeid from salaryheader where ranknumber = 1 and inchargeid=@employeeid))", string.Format("and (salesmanid in {0})", string.Join(",", employeidlist)));
                    query = query.Replace("and (employee.id = @employeeid or employee.id in (select employeeid from salaryheader where ranknumber = 1 and inchargeid=@employeeid))", string.Format("and (employee.id {0})", string.Join(",", employeidlist)));
                }
            }
            if (employeeid == 0)
            {
                query = query.Replace("and (salesmanid=@employeeid or salesmanid in (select employeeid from salaryheader where ranknumber = 1 and inchargeid=@employeeid))", "");
            }

            if (locationid == 0)
            {
                if (accessiblelocation == null || accessiblelocation.Contains(-1))
                {
                    query = query.Replace("and bill.locationid=@locationid", "");
                }
                else
                {
                    query = query.Replace("and bill.locationid=@locationid", "and bill.locationid in (" + string.Join(",", accessiblelocation) + ")");
                }
            }

            if (InfyPOS.Services.ConfigManager.Instance(organizationid).Config.payroll_incentive_addb2b)
                query = query.Replace("bill.isb2b=false", "bill.isexport=false");

            if (InfyPOS.Services.ConfigManager.Instance(organizationid).Config.payroll_incentive_addexport)
                query = query.Replace("and bill.isexport=false", "");

            InfyPOS.Common.BaseData baseData = new Common.BaseData() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            if (InfyPOS.Services.ConfigManager.Instance(organizationid).Config.system_isb2bapplication)
                query = query.Replace("and bill.isb2b=false", "");
            if (sectionid <= 0)
                query = query.Replace("and salescounterid=@sectionid", "");


            using (System.Data.IDataReader reader = baseData.ExecuteReader(query, param, 10000))
            {
                while (reader.Read())
                {
                    var salesmanid = reader["salesmanid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["salesmanid"]);
                    var qty = reader["qty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["qty"]);
                    var smconfig = config.Find(e => e.id == (reader["markerid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["markerid"])));
                    var reward = smconfig.rewardpoint;
                    var salevalue = reader["salevalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["salevalue"]);
                    if (InfyPOS.Services.ConfigManager.Instance(this.organizationid).Config.payroll_incentive_removebarcode)
                    {
                        qty -= reader["bqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["bqty"]);
                        salevalue -= reader["bamt"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["bamt"]);
                    }

                    if (result.Exists(e => e.salesmanid == salesmanid))
                    {
                        var spd = result.Find(e => e.salesmanid == salesmanid);
                        if (smconfig.name.IndexOf('%') > 0)
                            spd.incentive += Math.Round(salevalue * reward / 100, 0);
                        else
                            spd.incentive += (qty * reward);
                        spd.salevalue += salevalue;
                        spd.incentivecount += qty;
                    }
                    else
                    {
                        decimal incentive = 0;
                        if (smconfig.name.IndexOf('%') > 0)
                            incentive = Math.Round(salevalue * reward / 100, 0);
                        else
                            incentive = (qty * reward);
                        result.Add(new Salespersondata()
                        {
                            salesmanid = reader["salesmanid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["salesmanid"]),
                            incentive = incentive,
                            salevalue = salevalue,
                            incentivecount = qty,
                            Source = IncentiveSource.StockMarker
                        });
                    }
                }
            }
            return result;
        }

        private string getNameProductLevelCase(List<Range> rangelist, string field, string typeCast)
        {
            System.Text.StringBuilder conditionlist = new System.Text.StringBuilder();
            foreach (var rng in rangelist)
            {
                conditionlist.AppendLine("when stock.productid = " + rng.referenceid + " and " + field + " >= " + rng.start + typeCast + " AND " + field + "<" + rng.end + typeCast + " then '" + rng.prefix + "'");
            }
            return " case " + conditionlist.ToString() + " else 'UNKNOWN' end ";
        }
        private string getIncentiveProductLevelCase(List<Range> rangelist, string field, string typeCast)
        {
            System.Text.StringBuilder conditionlist = new System.Text.StringBuilder();
            foreach (var rng in rangelist.OrderByDescending(e => e.referenceid))
            {
                if (rng.referenceid > 0)
                    conditionlist.AppendLine("when stock.productid = " + rng.referenceid + " and " + field + " >= " + rng.fromprice + typeCast + " AND " + field + "<" + rng.toprice + typeCast +
                           (!rng.value ? " then (billitems.receivable*" + rng.incentive.ToString("F4") + "/100" : " then (billitems.qty*" + rng.incentive.ToString("F4")) + ")::numeric");
                else
                    conditionlist.AppendLine("when " + field + " >= " + rng.fromprice + typeCast + " AND " + field + "<" + rng.toprice + typeCast +
                           (!rng.value ? " then (billitems.receivable*" + rng.incentive.ToString("F4") + "/100" : " then (billitems.qty*" + rng.incentive.ToString("F4")) + ")::numeric");
            }
            return " case " + conditionlist.ToString() + " else 0::numeric end ";
        }

        private string getSaleValueProductLevelCase(List<Range> rangelist, string field, string typeCast)
        {
            System.Text.StringBuilder conditionlist = new System.Text.StringBuilder();
            foreach (var rng in rangelist.OrderByDescending(e => e.referenceid))
            {
                if (rng.referenceid > 0)
                    conditionlist.AppendLine("when stock.productid = " + rng.referenceid + " and " + field + " >= " + rng.fromprice + typeCast + " AND " + field + "<" + rng.toprice + typeCast + " then (billitems.receivable)::numeric");
                else
                    conditionlist.AppendLine("when " + field + " >= " + rng.fromprice + typeCast + " AND " + field + "<" + rng.toprice + typeCast + " then (billitems.receivable)::numeric");
            }
            return " case " + conditionlist.ToString() + " else 0::numeric end ";
        }

        private string getCountProductLevelCase(List<Range> rangelist, string field, string typeCast)
        {
            System.Text.StringBuilder conditionlist = new System.Text.StringBuilder();
            foreach (var rng in rangelist.OrderByDescending(e => e.referenceid))
            {
                if (rng.referenceid > 0)
                    conditionlist.AppendLine("when stock.productid = " + rng.referenceid + " and " + field + " >= " + rng.fromprice + typeCast + " AND " + field + "<" + rng.toprice + typeCast + " then 1::numeric");
                else
                    conditionlist.AppendLine("when " + field + " >= " + rng.fromprice + typeCast + " AND " + field + "<" + rng.toprice + typeCast + " then 1::numeric");
            }
            return " case " + conditionlist.ToString() + " else 0::numeric end ";
        }

        private string getRangeIDProductLevelCase(List<Range> rangelist, string field, string typeCast)
        {
            System.Text.StringBuilder conditionlist = new System.Text.StringBuilder();
            foreach (var rng in rangelist.OrderByDescending(e => e.referenceid))
            {
                if (rng.referenceid > 0)
                    conditionlist.AppendLine("when stock.productid = " + rng.referenceid + " and " + field + " >= " + rng.fromprice + typeCast + " AND " + field + "<" + rng.toprice + typeCast + " then '" + rng.id + "'");
                else
                    conditionlist.AppendLine("when " + field + " >= " + rng.fromprice + typeCast + " AND " + field + "<" + rng.toprice + typeCast + " then '" + rng.id + "'");
            }
            return " case " + conditionlist.ToString() + " else '' end ";
        }
        private List<Salespersondata> SearchProductLevelIncentives()
        {
            var result = new List<Salespersondata>();
            Data.ReferencelistDataService referencelistDataService = new Data.ReferencelistDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            var config = referencelistDataService.Search(new Models.ReferencelistCreteria()
            {
                type = "INCENTIVE-LEVEL-PRODUCT"
            });

            //WebAPI.Common.Logger.Instance(this.organizationid).Log(new Exception("INCENTIVE-LEVEL-PRODUCT "+config.Count)
            //, WebAPI.Common.Logger.LogType.Error, config.Count>0 ? config.First().additionalinfo.ToJSON():"");

            if (config.Count > 0)
            {
                List<Models.Referencelist.SalesIncentive> configList = null;
                if (locationid > 0 && config.Exists(e => e.locationid == locationid))
                    configList = config.Find(e => e.locationid == locationid).additionalinfo.salesincentive;
                else if (locationid == 0 && config.Exists(e => e.locationid == 0))
                    configList = config.Find(e => e.locationid == 0).additionalinfo.salesincentive;
                else
                    configList = config.First().additionalinfo.salesincentive;

                productrangelist = new List<Range>();
                foreach (var item in configList.OrderBy(e => e.from))
                {
                    if (item.from > 0 || item.to > 0)
                    {
                        var rng = new Range()
                        {
                            start = (int)item.from,
                            end = (int)item.to,
                            fromprice = item.fromprice,
                            toprice = item.toprice,
                            incentive = item.incentive,
                            referenceid = item.referenceid,
                            value = item.value,
                            id = productrangelist.Count.ToString()
                        };
                        productrangelist.Add(rng);
                    }
                }

                if (productrangelist.Count == 0) return result;

                string field = getIncentiveProductLevelCase(productrangelist, "(billitems.receivable/billitems.qty)", "::numeric");
                string field1 = getSaleValueProductLevelCase(productrangelist, "(billitems.receivable/billitems.qty)", "::numeric");
                string field2 = getCountProductLevelCase(productrangelist, "(billitems.receivable/billitems.qty)", "::numeric");
                string field3 = getRangeIDProductLevelCase(productrangelist, "(billitems.receivable/billitems.qty)", "::numeric");

                string query = string.Format(@"
                                with salaryheader as (
			                    SELECT id, employeeid, fromdate, inchargeid, row_number() OVER (PARTITION BY employeeid ORDER BY fromdate DESC,id DESC) as ranknumber  
			                    FROM employeesalaryheader where fromdate<=@todate),
                                markedlist as
								(
									select distinct stockmarker.markerid,stockmarker.stockid from stockmarker
                                    left outer join incentiveconfig on incentiveconfig.salablegoodsid = stockmarker.salablegoodsid and incentiveconfig.isactive=true
                                    where markerid in ({4})
									and stockmarker.isactive = true and incentiveconfig.salablegoodsid is null
								),
                                barcodelist as
								(
									select distinct salablegoodsid from incentiveconfig where 
									incentiveconfig.isactive = true
								)

                                select sum({0}) as incentive,sum({1}) as salevalue,sum({2}) as salecount,
                                salesmanid,{3} as id
                                from billitems
                                inner join bill on bill.id = billitems.billid
                                left outer join stock on stock.id = billitems.stockid
                                left outer join products on products.id = stock.productid
                                left outer join markedlist on markedlist.stockid = billitems.stockid
                                left outer join barcodelist on barcodelist.salablegoodsid = billitems.salablegoodsid
                                where bill.isactive=true and billitems.isactive=true  and billitems.qty!=0 and billdate>=@fromdate and billdate<@todate 
                                and bill.locationid=@locationid and bill.billtype != 9 and bill.isb2b=false and salescounterid=@sectionid
                                and (salesmanid=@employeeid or salesmanid in (select employeeid from salaryheader where ranknumber = 1 and inchargeid=@employeeid)) 
                                and markedlist.stockid is null and barcodelist.salablegoodsid is null
                                group by salesmanid,{3}", field, field1, field2, field3, string.Join(",", GetStockMarkerList()));

                if (InfyPOS.Services.ConfigManager.Instance(this.organizationid).Config.payroll_incentive_removebarcode)
                {
                    query = string.Format(@"
                                with salaryheader as (
			                    SELECT id, employeeid, fromdate, inchargeid, row_number() OVER (PARTITION BY employeeid ORDER BY fromdate DESC,id DESC) as ranknumber  
			                    FROM employeesalaryheader where fromdate<=@todate)

                                select sum({0}) as incentive,sum({1}) as salevalue,sum({2}) as salecount,
                                salesmanid,{3} as id
                                from billitems
                                inner join bill on bill.id = billitems.billid
                                left outer join stock on stock.id = billitems.stockid
                                left outer join products on products.id = stock.productid
                                where bill.isactive=true and billitems.isactive=true  and billitems.qty!=0 and billdate>=@fromdate and billdate<@todate 
                                and bill.locationid=@locationid and bill.billtype != 9 and bill.isb2b=false and salescounterid=@sectionid
                                and (salesmanid=@employeeid or salesmanid in (select employeeid from salaryheader where ranknumber = 1 and inchargeid=@employeeid)) 
                                group by salesmanid,{3}", field, field1, field2, field3);

                }
                SortedList<string, object> param = new SortedList<string, object>();


                param.Add("salarymonth", fromdate);
                param.Add("fromdate", fromdate.Date);
                param.Add("employeeid", employeeid);
                param.Add("locationid", locationid);
                param.Add("sectionid", sectionid);
                param.Add("todate", todate.Date.AddDays(1));

                if (employeeid > 0)
                {
                    string employeidlistquery = @"            
                with salaryheader as (
			        SELECT id, employeeid, fromdate, inchargeid, row_number() OVER (PARTITION BY employeeid ORDER BY fromdate DESC,id DESC) as ranknumber  
			        FROM employeesalaryheader where fromdate<=@todate
                )
            select employeeid from salaryheader where ranknumber = 1 and inchargeid=@employeeid;";
                    List<long> employeidlist = new List<long>();
                    employeidlist.Add(employeeid);
                    using (var reader = ExecuteReader(employeidlistquery, param))
                    {
                        while (reader.Read())
                        {
                            if (!reader.IsDBNull(0))
                                employeidlist.Add(reader.GetInt64(0));
                        }
                    }
                    if (employeidlist.Count == 1)
                    {
                        query = query.Replace("and (employee.id = @employeeid or employee.id in (select employeeid from salaryheader where inchargeid=@employeeid))", "and (employee.id = @employeeid)");
                        query = query.Replace("and (salesmanid=@employeeid or salesmanid in (select employeeid from salaryheader where ranknumber = 1 and inchargeid=@employeeid))", "and (salesmanid=@employeeid)");
                        query = query.Replace("and (employee.id = @employeeid or employee.id in (select employeeid from salaryheader where ranknumber = 1 and inchargeid=@employeeid))", "and (employee.id = @employeeid)");
                    }
                    else
                    {
                        query = query.Replace("and (employee.id = @employeeid or employee.id in (select employeeid from salaryheader where inchargeid=@employeeid))", string.Format("and (employee.id in ({0})", string.Join(",", employeidlist)));
                        query = query.Replace("and (salesmanid=@employeeid or salesmanid in (select employeeid from salaryheader where ranknumber = 1 and inchargeid=@employeeid))", string.Format("and (salesmanid in {0})", string.Join(",", employeidlist)));
                        query = query.Replace("and (employee.id = @employeeid or employee.id in (select employeeid from salaryheader where ranknumber = 1 and inchargeid=@employeeid))", string.Format("and (employee.id {0})", string.Join(",", employeidlist)));
                    }
                }
                if (employeeid == 0)
                {
                    query = query.Replace("and (salesmanid=@employeeid or salesmanid in (select employeeid from salaryheader where ranknumber = 1 and inchargeid=@employeeid))", "");
                }

                if (locationid == 0)
                {
                    if (accessiblelocation == null || accessiblelocation.Contains(-1))
                    {
                        query = query.Replace("and bill.locationid=@locationid", "");
                    }
                    else
                    {
                        query = query.Replace("and bill.locationid=@locationid", "and bill.locationid in (" + string.Join(",", accessiblelocation) + ")");
                    }
                }

                if (InfyPOS.Services.ConfigManager.Instance(organizationid).Config.payroll_incentive_addb2b)
                    query = query.Replace("bill.isb2b=false", "bill.isexport=false");

                if (InfyPOS.Services.ConfigManager.Instance(organizationid).Config.payroll_incentive_addexport)
                    query = query.Replace("and bill.isexport=false", "");


                InfyPOS.Common.BaseData baseData = new Common.BaseData() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
                //string cmdquery = baseData.GetSQL(baseData.GetCommand(query, param));
                //WebAPI.Common.Logger.Instance(this.organizationid).Log(new Exception("INCENTIVE-LEVEL-PRODUCT QUERY"), WebAPI.Common.Logger.LogType.Error, cmdquery);
                if (InfyPOS.Services.ConfigManager.Instance(organizationid).Config.system_isb2bapplication)
                    query = query.Replace("and bill.isb2b=false", "");
                if (sectionid <= 0)
                    query = query.Replace("and salescounterid=@sectionid", "");


                using (System.Data.IDataReader reader = baseData.ExecuteReader(query, param, 10000))
                {

                    while (reader.Read())
                    {
                        string id = reader["id"].ToString();

                        var sp = new Salespersondata()
                        {
                            salesmanid = reader["salesmanid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["salesmanid"]),
                            incentive = reader["incentive"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["incentive"]),
                            salevalue = reader["salevalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["salevalue"]),
                            incentivecount = reader["salecount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["salecount"]),
                            Source = IncentiveSource.ProductLevel
                        };

                        if (productrangelist.Exists(e => e.id == id))
                        {
                            var rng = productrangelist.Find(e => e.id == id);
                            var lst = productrangelist.FindAll(e => e.referenceid == rng.referenceid).OrderByDescending(e => e.start).ToList();
                            if (lst.Exists(e => e.start <= sp.incentivecount && e.end >= sp.incentivecount))
                            {
                                rng = lst.Find(e => e.start <= sp.incentivecount && e.end >= sp.incentivecount);
                                if (rng.value)
                                    rng.incentive = sp.incentivecount * rng.incentive;
                                else
                                    rng.incentive = Math.Round(sp.salevalue * rng.incentive / 100);
                            }
                            else if (lst.First().end < sp.incentivecount)
                            {
                                rng = lst.First();
                                if (rng.value)
                                    rng.incentive = sp.incentivecount * rng.incentive;
                                else
                                    rng.incentive = Math.Round(sp.salevalue * rng.incentive / 100);
                            }
                        }

                        result.Add(sp);
                    }
                }
            }
            return result;
        }

        private int SundayTotal()
        {
            int sundays = 0;
            DateTime monthstart = fromdate;
            DateTime monthend = todate.AddDays(1);
            while (monthstart < monthend)
            {
                if (monthstart.DayOfWeek == DayOfWeek.Sunday)
                    sundays++;
                monthstart = monthstart.AddDays(1);
            }
            return sundays;
        }

        private decimal WorkingDays(DateTime fromdate, DateTime todate)
        {
            var includeSunday = InfyPOS.Services.ConfigManager.Instance(organizationid).Config.payroll_workdaysmode == 1;
            InfyPOS.Common.BaseData baseData = new InfyPOS.Common.BaseData();
            baseData.Context = new Common.UserClaim() { OrganizationID = organizationid };
            var query = @"select holidayon,leaveencashment from holiday where holidayon >= @fromdate AND holidayon < @todate and isactive=true";

            SortedList<string, object> param = new SortedList<string, object>();
            param.Add("fromdate", fromdate);
            param.Add("todate", todate.AddDays(1));
            decimal totalHoliday = 0;
            decimal totalHolidayEncashment = 0;
            decimal totalPayableHoliday = 0;
            using (System.Data.IDataReader reader = baseData.ExecuteReader(query, param))
            {
                while (reader.Read())
                {
                    totalHolidayEncashment += (reader.IsDBNull(1) ? 0 : reader.GetInt32(1));
                    if ((reader.IsDBNull(1) ? 0 : reader.GetInt32(1)) > 0)
                        totalPayableHoliday++;
                    if (reader.GetDateTime(0).DayOfWeek == DayOfWeek.Sunday && !includeSunday)
                        continue;
                    totalHoliday++;
                    //holidayList.Add(reader.GetDateTime(0));
                }
            }
            decimal TotalHoliday = totalHoliday;
            DateTime monthstart = fromdate;
            DateTime monthend = todate;
            decimal TotalSunday = !includeSunday ? SundayTotal() : 0;
            //nonpayableholiday = NonPayableHoliday();
            //payableholiday = PayableHoliday();
            totalHoliday += TotalSunday;
            var TotalDays = (decimal)monthend.Subtract(monthstart).TotalDays;
            var TotalWorkingDays = (decimal)monthend.Subtract(monthstart).TotalDays - totalHoliday;
            if (InfyPOS.Services.ConfigManager.Instance(organizationid).Config.payroll_workdaysmode == 2)
                TotalWorkingDays = 30;
            if (InfyPOS.Services.ConfigManager.Instance(organizationid).Config.payroll_workdaysmode == 3)
                TotalWorkingDays = TotalDays;


            decimal eligibleleave = 0;

            if (InfyPOS.Services.ConfigManager.Instance(organizationid).Config.payroll_leaveeligiblemode <= 0)
            {
                eligibleleave = InfyPOS.Services.ConfigManager.Instance(organizationid).Config.payroll_leaveeligibledays;
            }
            else if (InfyPOS.Services.ConfigManager.Instance(organizationid).Config.payroll_leaveeligiblemode == 1)
            {
                eligibleleave = TotalSunday;
            }
            else if (InfyPOS.Services.ConfigManager.Instance(organizationid).Config.payroll_leaveeligiblemode == 2)
            {
                eligibleleave = TotalSunday + TotalHoliday;
            }
            else
            {
                eligibleleave = 0;
            }

            return TotalWorkingDays - eligibleleave;
            //var days = (decimal)todate.Subtract(fromdate).TotalDays;
            //InfyPOS.Common.BaseData baseData = new InfyPOS.Common.BaseData() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            //string query = @"select count(*) from holiday where holidayon>=@fromdate AND holidayon<@todate and isactive=true";
            //SortedList<string, object> param = new SortedList<string, object>();
            //param.Add("fromdate", fromdate);
            //param.Add("todate", todate);

            //using (System.Data.IDataReader reader = baseData.ExecuteReader(query, param))
            //{
            //    while (reader.Read())
            //    {
            //        days -= reader.GetInt32(0);
            //    }
            //}

            //while (fromdate < todate)
            //{
            //    if (fromdate.DayOfWeek == DayOfWeek.Sunday)
            //        days--;
            //    fromdate = fromdate.AddDays(1);
            //}

            //return days;
        }

        private List<IncentiveSalary.IncentiveData> BuildMasters()
        {


            string query = @"
            select id,additionalinfo,name from referencelist where type='DEPARTMENT';

            with salaryheader as (
			SELECT id, employeeid, fromdate, inchargeid, row_number() OVER (PARTITION BY employeeid ORDER BY fromdate DESC,id DESC) as ranknumber  
			FROM employeesalaryheader where fromdate<=@todate)

            select name as employeename,employeecode,workingdays,leavedays,employeesalaryheader.incentiveratio,employeesalaryheader.salestarget,employeesalary.earnings,
            employee.departmentid,employee.companyid,employee.sectionid,employee.id as employeeid,grosspay,employee.locationid,salaryheader.inchargeid,
            employeesalaryheader.incentivesource,employeesalaryheader.incentivepercentage,employeesalaryheader.salestargetmethod,
            employeesalaryheader.incentivevaluetype,employeesalaryheader.incentivecalculationsource,employeesalaryheader.salestargetminimummode,
            employeesalaryheader.salesminimumtarget,salestarget,employee.floorid,inout.IOLeaveDays,inout.IOSundayLeaves
            from employee
            left outer join salaryheader on salaryheader.ranknumber = 1 and salaryheader.employeeid = employee.id    
            left outer join employeesalary on employeesalary.employeeid = employee.id and salarymonth=@salarymonth
            left outer join employeesalaryheader on employeesalaryheader.id = salaryheader.id
            left outer join (select employeeid,SUM(days) as IOLeaveDays,SUM(sundaydays) as IOSundayLeaves from employeeleave 
                                inner join employee on employee.id = employeeleave.employeeid
                                where EmployeeLeave.isactive and deductsalary=true 
                                and fromdate>=dateofjoining and
                                (employee.hasleft = false or relievingdate>=fromdate) and
                                fromdate >=@fromdate and todate<@todate
                                group by EmployeeID) inout
                            on inout.employeeid = employee.id


            where (employee.hasleft = false or relievingdate>=@fromdate) and employee.isactive = true 
            and (employee.id = @employeeid or employee.id in (select employeeid from salaryheader where inchargeid=@employeeid))
            order by case when employeecode ~ '^[0-9]+$' then cast(employeecode as numeric) else cast(0 as numeric) end;

            with salaryheader as (
			SELECT id, employeeid, fromdate, inchargeid, row_number() OVER (PARTITION BY employeeid ORDER BY fromdate DESC,id DESC) as ranknumber  
			FROM employeesalaryheader where fromdate<=@todate)

            select sum(billitems.receivable) as amount,salescounterid as sectionid,bill.companyid,bill.locationid,
            bill.floorid,sum(case when billitems.additionaldiscount>0 then billitems.receivable else 0 end) additionaldiscount,
            sum(case when billitems.schemediscount>0 then billitems.receivable else 0 end) schemediscount
            from billitems
            inner join bill on bill.id = billitems.billid
            left outer join stock on stock.id = billitems.stockid
            left outer join products on products.id = stock.productid
            where bill.isactive=true and billitems.isactive=true  and billitems.qty!=0 and billdate>=@fromdate and billdate<@todate 
            and bill.locationid=@locationid and bill.companyid=@companyid and bill.billtype != 9 and bill.isb2b=false and salescounterid=@sectionid
            and (salesmanid=@employeeid or salesmanid in (select employeeid from salaryheader where ranknumber = 1 and inchargeid=@employeeid))
            group by salescounterid,bill.companyid,bill.locationid,bill.floorid,billitems.salesmanid;

            with salaryheader as (
			SELECT id, employeeid, fromdate, inchargeid, row_number() OVER (PARTITION BY employeeid ORDER BY fromdate DESC,id DESC) as ranknumber  
			FROM employeesalaryheader where fromdate<=@todate)

            select sum(billitems.receivable) as amount,salesmanid,sum(billitems.qty) qty,
            sum(case when billitems.schemediscount>0 then billitems.receivable else 0 end) schemediscount,
            sum(case when billitems.additionaldiscount>0 then billitems.receivable else 0 end) additionaldiscount
            from billitems
            inner join bill on bill.id = billitems.billid
            left outer join stock on stock.id = billitems.stockid
            left outer join products on products.id = stock.productid
            where bill.isactive=true and billitems.isactive=true  and billitems.qty!=0 and billdate>=@fromdate and billdate<@todate 
            and bill.locationid=@locationid and bill.billtype != 9 and bill.isb2b=false and bill.companyid=@companyid and salescounterid=@sectionid
            and (salesmanid=@employeeid or salesmanid in (select employeeid from salaryheader where ranknumber = 1 and inchargeid=@employeeid))
            and case when @companyid=0 then bill.companyid >= 0 else bill.companyid = @companyid end 
            group by salesmanid;


            with salaryheader as (
			SELECT id, employeeid, fromdate, inchargeid, row_number() OVER (PARTITION BY employeeid ORDER BY fromdate DESC,id DESC) as ranknumber  
			FROM employeesalaryheader where fromdate<=@todate)

            select sum(billitems.receivable) as amount,
            sum(billitems.qty) as incentivecount,
            sum(billitems.qty*incentiveconfig.incentive::numeric) as incentive,
            sum(case when coalesce(incentiveconfig.percentage::numeric,0::numeric)>0::numeric then billitems.receivable::numeric*coalesce(incentiveconfig.percentage::numeric,0::numeric)/100::numeric else 0::numeric end) as incentivebypercentage,
            salesmanid from billitems
            inner join bill on bill.id = billitems.billid
            left outer join stock on stock.id = billitems.stockid
            left outer join products on products.id = stock.productid
            inner join incentiveconfig on incentiveconfig.salablegoodsid = billitems.salablegoodsid and incentiveconfig.isactive = true
            where bill.isactive=true and billitems.isactive=true  and billitems.qty!=0 and billdate>=@fromdate and billdate<@todate 
            and bill.locationid=@locationid and bill.billtype != 9 and bill.isb2b=false and salescounterid=@sectionid
            and (salesmanid=@employeeid or salesmanid in (select employeeid from salaryheader where ranknumber = 1 and inchargeid=@employeeid))
            and case when @companyid=0 then bill.companyid >= 0 else bill.companyid = @companyid end 
            group by salesmanid";

            SortedList<string, object> param = new SortedList<string, object>();


            param.Add("salarymonth", fromdate);
            param.Add("fromdate", fromdate.Date);
            param.Add("locationid", locationid);
            param.Add("employeeid", employeeid);
            param.Add("companyid", companyid);
            param.Add("sectionid", sectionid);
            param.Add("todate", todate.Date.AddDays(1));


            if (employeeid > 0)
            {
                string employeidlistquery = @"            
                with salaryheader as (
			        SELECT id, employeeid, fromdate, inchargeid, row_number() OVER (PARTITION BY employeeid ORDER BY fromdate DESC,id DESC) as ranknumber  
			        FROM employeesalaryheader where fromdate<=@todate
                )
            select employeeid from salaryheader where ranknumber = 1 and inchargeid=@employeeid;";
                List<long> employeidlist = new List<long>();
                employeidlist.Add(employeeid);
                using (var reader = ExecuteReader(employeidlistquery, param))
                {
                    while (reader.Read())
                    {
                        if (!reader.IsDBNull(0))
                            employeidlist.Add(reader.GetInt64(0));
                    }
                }
                if (employeidlist.Count == 1)
                {
                    query = query.Replace("and (employee.id = @employeeid or employee.id in (select employeeid from salaryheader where inchargeid=@employeeid))", "and (employee.id = @employeeid)");
                    query = query.Replace("and (salesmanid=@employeeid or salesmanid in (select employeeid from salaryheader where ranknumber = 1 and inchargeid=@employeeid))", "and (salesmanid=@employeeid)");
                    query = query.Replace("and (employee.id = @employeeid or employee.id in (select employeeid from salaryheader where ranknumber = 1 and inchargeid=@employeeid))", "and (employee.id = @employeeid)");
                }
                else
                {
                    query = query.Replace("and (employee.id = @employeeid or employee.id in (select employeeid from salaryheader where inchargeid=@employeeid))", string.Format("and (employee.id in ({0})", string.Join(",", employeidlist)));
                    query = query.Replace("and (salesmanid=@employeeid or salesmanid in (select employeeid from salaryheader where ranknumber = 1 and inchargeid=@employeeid))", string.Format("and (salesmanid in {0})", string.Join(",", employeidlist)));
                    query = query.Replace("and (employee.id = @employeeid or employee.id in (select employeeid from salaryheader where ranknumber = 1 and inchargeid=@employeeid))", string.Format("and (employee.id {0})", string.Join(",", employeidlist)));
                }
            }

            if (employeeid == 0)
            {
                query = query.Replace("and (employee.id = @employeeid or employee.id in (select employeeid from salaryheader where inchargeid=@employeeid))", "");
                query = query.Replace("and (salesmanid=@employeeid or salesmanid in (select employeeid from salaryheader where ranknumber = 1 and inchargeid=@employeeid))", "");
                query = query.Replace("and (employee.id = @employeeid or employee.id in (select employeeid from salaryheader where ranknumber = 1 and inchargeid=@employeeid))", "");
            }

            if (companyid == 0)
            {
                query = query.Replace("and bill.companyid=@companyid", "");
            }

            if (sectionid == 0)
            {
                query = query.Replace("and salescounterid=@sectionid", "");
            }

            if (locationid == 0)
            {
                if (accessiblelocation == null || accessiblelocation.Contains(-1))
                {
                    query = query.Replace("and bill.locationid=@locationid", "");
                }
                else
                {
                    query = query.Replace("and bill.locationid=@locationid", "and bill.locationid in (" + string.Join(",", accessiblelocation) + ")");
                }
            }

            return BuildMasters(query, param);
        }

        private List<IncentiveSalary.IncentiveData> BuildMasters(string query, SortedList<string, object> param)
        {
            List<IncentiveSalary.IncentiveData> retValue = new List<IncentiveSalary.IncentiveData>();
            InfyPOS.Common.BaseData baseData = new Common.BaseData() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            var fromdate = (DateTime)param["fromdate"];
            var todate = (DateTime)param["todate"];
            var days = (decimal)todate.Subtract(fromdate).TotalDays;
            var totalworkingdays = WorkingDays(fromdate, todate);

            if (InfyPOS.Services.ConfigManager.Instance(organizationid).Config.payroll_incentive_addb2b)
                query = query.Replace("bill.isb2b=false", "bill.isexport=false");

            if (InfyPOS.Services.ConfigManager.Instance(organizationid).Config.payroll_incentive_addexport)
                query = query.Replace("and bill.isexport=false", "");

            if (InfyPOS.Services.ConfigManager.Instance(organizationid).Config.system_isb2bapplication)
                query = query.Replace("and bill.isb2b=false", "");
            if (sectionid <= 0)
                query = query.Replace("and salescounterid=@sectionid", "");


            using (System.Data.IDataReader reader = baseData.ExecuteReader(query, param, 10000))
            {
                while (reader.Read())
                {
                    //name as employeename,referencelist.name departmentname,additionalinfo,workingdays,leavedays,employeesalaryheader.incentiveratio
                    Models.Referencelist reflist = new Models.Referencelist();
                    reflist.additionalinfo_json = reader["additionalinfo"] == DBNull.Value ? "null" : reader["additionalinfo"].ToString();
                    reflist.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                    reflist.name = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();

                    if (this.departmentid != 0 && reflist.id != this.departmentid) continue;

                    IncentiveSalary.IncentiveData config = new IncentiveSalary.IncentiveData();
                    config.department = reflist.name;
                    config.departmentid = reflist.id;
                    if (reflist.additionalinfo != null)
                    {
                        decimal percentage = reflist.additionalinfo.fixedincentive + reflist.additionalinfo.dynamicincentive;
                        //decimal.TryParse(reflist.additionalinfo.fixedincentive, out percentage);
                        config.fixedpercentage = percentage;
                        config.dynamicpercentage = reflist.additionalinfo.dynamicincentive;
                        config.IsSales = reflist.additionalinfo.issales;
                        config.salesIncentives = reflist.additionalinfo.salesincentive;
                    }
                    retValue.Add(config);
                }

                if (Services.ConfigManager.Instance(this.organizationid).Config.incentivemode == 4 &&
                    retValue.Count == 0)
                {

                    IncentiveSalary.IncentiveData config = new IncentiveSalary.IncentiveData();
                    config.department = "Sales";
                    config.departmentid = 0;
                    var incentiveratio = locationid != 0 &&
                        Services.ConfigManager.Instance(this.organizationid).LocationConfig.ContainsKey(locationid) &&
                       Services.ConfigManager.Instance(this.organizationid).LocationConfig[locationid].incentiveskipmonths != 0 ?
                       Services.ConfigManager.Instance(this.organizationid).LocationConfig[locationid].incentiveskipmonths :
                       Services.ConfigManager.Instance(this.organizationid).Config.incentiveskipmonths;

                    config.fixedpercentage = incentiveratio;
                    config.dynamicpercentage = 0;
                    config.IsSales = true;
                    config.salesIncentives = new List<Models.Referencelist.SalesIncentive>();
                    retValue.Add(config);
                }
                if (reader.NextResult())
                {
                    while (reader.Read())
                    {
                        //name as employeename,referencelist.name departmentname,additionalinfo,workingdays,leavedays,employeesalaryheader.incentiveratio
                        string employeename = reader["employeename"] == DBNull.Value ? "" : reader["employeename"].ToString();
                        string employeecode = reader["employeecode"] == DBNull.Value ? "" : reader["employeecode"].ToString();
                        decimal workingdays = reader["workingdays"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["workingdays"]);
                        decimal leavedays = reader["leavedays"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["leavedays"]);

                        decimal inoutdays = reader["IOLeaveDays"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["IOLeaveDays"]);
                        decimal halfdays = reader["IOSundayLeaves"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["IOSundayLeaves"]);
                        inoutdays = days - inoutdays;

                        decimal grosspay = reader["grosspay"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["grosspay"]);
                        decimal earnings = reader["earnings"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["earnings"]);
                        long departmentid = reader["departmentid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["departmentid"]);
                        long sectionid = reader["sectionid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sectionid"]);
                        long employeeid = reader["employeeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["employeeid"]);
                        long companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"]);
                        long locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                        long floorid = reader["floorid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["floorid"]);

                        long inchargeid = reader["inchargeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["inchargeid"]);
                        decimal salestarget = reader["salestarget"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["salestarget"]);
                        decimal incentivepercentage = reader["incentivepercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["incentivepercentage"]);
                        decimal incentiveratio = reader["incentiveratio"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["incentiveratio"]);
                        decimal salesminimumtarget = reader["salesminimumtarget"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["salesminimumtarget"]);

                        int incentivesource = reader["incentivesource"] == DBNull.Value ? 0 : Convert.ToInt32(reader["incentivesource"]);
                        int salestargetmethod = reader["salestargetmethod"] == DBNull.Value ? 0 : Convert.ToInt32(reader["salestargetmethod"]);
                        int incentivevaluetype = reader["incentivevaluetype"] == DBNull.Value ? 0 : Convert.ToInt32(reader["incentivevaluetype"]);
                        int incentivecalculationsource = reader["incentivecalculationsource"] == DBNull.Value ? 0 : Convert.ToInt32(reader["incentivecalculationsource"]);
                        int salestargetminimummode = reader["salestargetminimummode"] == DBNull.Value ? 0 : Convert.ToInt32(reader["salestargetminimummode"]);

                        if (incentiveratio <= 0) incentiveratio = 1;
                        if (incentivesource != 7 && companyid != 0 && this.companyid != 0 && companyid != this.companyid) continue;
                        //if (departmentid != 0 && this.departmentid != 0 && departmentid != this.departmentid) continue;
                        if (locationid != 0 && this.locationid != 0 && locationid != this.locationid) continue;

                        if (!retValue.Exists(e => e.departmentid == departmentid))
                        {
                            retValue.Add(new IncentiveSalary.IncentiveData()
                            {
                                departmentid = departmentid,
                                department = "Unknown"
                            });
                        }

                        retValue.Find(e => e.departmentid == departmentid).Employees.Add(new IncentiveSalary.IncentiveData.Item()
                        {
                            incentiveratio = incentiveratio,
                            salesminimumtarget = salesminimumtarget,
                            salestargetmethod = salestargetmethod,
                            incentivecalculationsource = incentivecalculationsource,
                            incentivevaluetype = incentivevaluetype,
                            salestargetminimummode = salestargetminimummode,
                            employeename = employeename,
                            employeeid = employeeid,
                            days = days - leavedays,
                            workingdays = inoutdays,
                            totalworkingdays = totalworkingdays,
                            incentivepercentage = incentivepercentage,
                            incentivesource = incentivesource,
                            payratio = workingdays <= 0 ? 1 : (((days - leavedays) / workingdays)),
                            ratio = incentiveratio,
                            sectionid = sectionid,
                            floorid = floorid,
                            departmentid = departmentid,
                            companyid = companyid,
                            employeecode = employeecode,
                            grosspay = grosspay,
                            earnings = earnings,
                            salestarget = salestarget,
                            inchargeid = inchargeid
                        });

                    }
                }

                if (reader.NextResult())
                {
                    while (reader.Read())
                    {
                        long sectionid = reader["sectionid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sectionid"]);
                        long companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"]);
                        long floorid = reader["floorid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["floorid"]);
                        long locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                        decimal amount = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"]);
                        decimal additionaldiscount = reader["additionaldiscount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["additionaldiscount"]);
                        decimal schemediscount = reader["schemediscount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["schemediscount"]);

                        totalSales += amount;
                        if (retValue.Exists(e => e.IsSales))
                        {
                            retValue.First(e => e.IsSales).SalesValue.Add(new IncentiveSalary.IncentiveData.salevalue()
                            {
                                amount = amount,
                                additionaldiscount = additionaldiscount,
                                schemediscount = schemediscount,
                                companyid = companyid,
                                sectionid = sectionid,
                                locationid = locationid,
                                floorid = floorid
                            });
                        }
                    }
                }

                if (reader.NextResult())
                {
                    while (reader.Read())
                    {
                        long salesmanid = reader["salesmanid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["salesmanid"]);
                        decimal amount = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"]);
                        decimal qty = reader["qty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["qty"]);
                        decimal additionaldiscount = reader["additionaldiscount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["additionaldiscount"]);
                        decimal schemediscount = reader["schemediscount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["schemediscount"]);

                        foreach (var dpt in retValue)
                        {
                            if (dpt.Employees.Exists(e => e.employeeid == salesmanid))
                            {
                                dpt.Employees.FindAll(e => e.employeeid == salesmanid).ForEach(e =>
                                {
                                    e.salesvalue = amount;
                                    e.salesqty = qty;
                                    e.additionaldiscount = additionaldiscount;
                                    e.schemediscount = schemediscount;
                                });
                            }
                        }
                    }
                }

                if (reader.NextResult())
                {
                    while (reader.Read())
                    {
                        long salesmanid = reader["salesmanid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["salesmanid"]);
                        decimal amount = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"]);
                        decimal incentive = reader["incentive"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["incentive"]);
                        decimal incentivebypercentage = reader["incentivebypercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["incentivebypercentage"]);
                        decimal incentivecount = reader["incentivecount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["incentivecount"]);
                        foreach (var dpt in retValue)
                        {
                            var employee = dpt.Employees.FindAll(e => e.employeeid == salesmanid);
                            if (employee.Count > 0)
                            {
                                employee.ForEach(e =>
                                {
                                    //e.salesvalue += amount;
                                    e.itemincentive += incentive + incentivebypercentage;
                                    e.itemsalevalue += amount;
                                    e.incentivecount += incentivecount;
                                });
                                break;
                            }
                        }
                    }
                }
            }
            return retValue;
        }
        public List<IncentiveSalary.IncentiveData> Compute()
        {
            var result = BuildMasters();
            var all = true; // !LicenseManager.Instance(organizationid).GetLicense().payroll;
            // * Barcode 
            List<IncentiveSalary.IncentiveData.Item> allEmployees = new List<IncentiveSalary.IncentiveData.Item>();
            foreach (var dpt in result)
            {
                if (dpt.IsSales)
                {
                    dpt.Employees.FindAll(e => dpt.Employees.Exists(x => x.inchargeid == e.employeeid)).ForEach(e => e.IsIncharge = true);
                    dpt.Employees.FindAll(e => !dpt.Employees.Exists(x => x.inchargeid == e.employeeid)).ForEach(e => e.IsSalesmen = true);
                }
                allEmployees.AddRange(dpt.Employees);
            }

            if (!searchmodes.Exists(e => e == 0))
            {
                if (!searchmodes.Exists(e => e == 12))
                {
                    foreach (var employee in allEmployees)
                    {
                        employee.itemincentive = 0;
                        employee.itemsalevalue = 0;
                        employee.incentivecount = 0;
                    }
                }
            }

            List<IncentiveSalary.IncentiveData.salevalue> SalesValue = new List<IncentiveSalary.IncentiveData.salevalue>();
            foreach (var sv in result.FindAll(e => e.SalesValue != null && e.SalesValue.Count > 0))
            {
                SalesValue.AddRange(sv.SalesValue);
            }


            allEmployees.ForEach(e =>
            {
                e.amount += e.itemincentive;

                if (InfyPOS.Services.ConfigManager.Instance(this.organizationid).Config.payroll_incentive_removebarcode)
                    e.salesvalue -= e.itemsalevalue;
                if (InfyPOS.Services.ConfigManager.Instance(this.organizationid).Config.payroll_incentive_removeprefix)
                    e.salesvalue -= e.prefixvalue;
                if (InfyPOS.Services.ConfigManager.Instance(this.organizationid).Config.payroll_incentive_removeproduct)
                    e.salesvalue -= e.productlevelvalue;

                if (InfyPOS.Services.ConfigManager.Instance(this.organizationid).Config.payroll_incentive_removebarcode)
                    e.salesqty -= e.incentivecount;
                if (InfyPOS.Services.ConfigManager.Instance(this.organizationid).Config.payroll_incentive_removeprefix)
                    e.salesqty -= e.prefixcount;
                if (InfyPOS.Services.ConfigManager.Instance(this.organizationid).Config.payroll_incentive_removeproduct)
                    e.salesqty -= e.productlevelcount;
            });

            // Product Level
            if (searchmodes.Exists(e => e == 0) || searchmodes.Exists(e => e == 11))
            {
                var searchProductLevelIncentives = SearchProductLevelIncentives();
                searchProductLevelIncentives.ForEach(e =>
                {
                    var employee = allEmployees.Find(x => x.employeeid == e.salesmanid);
                    if (employee != null)
                    {
                        employee.productlevelincentive += e.incentive;
                        employee.productlevelcount += e.incentivecount;
                        employee.productlevelvalue += e.salevalue;
                        employee.amount += e.incentive;
                    }
                });
            }

            // Stock Marker Prefix
            if (searchmodes.Exists(e => e == 0) || searchmodes.Exists(e => e == 13))
            {
                var searchIncentives = SearchStockMarkerIncentives();
                searchIncentives.ForEach(e =>
                {
                    var employee = allEmployees.Find(x => x.employeeid == e.salesmanid);
                    if (employee != null)
                    {
                        employee.smincentive += e.incentive;
                        employee.smcount += e.incentivecount;
                        employee.smvalue += e.salevalue;
                        employee.amount += e.incentive;
                    }
                });
            }

            if (searchmodes.Exists(e => e == 0) || searchmodes.Exists(e => e == 22))
            {
                var searchIncentives = ExecuteTailoringIncentive();
                searchIncentives.ForEach(e =>
                {
                    var employee = allEmployees.Find(x => x.employeeid == e.salesmanid);
                    if (employee != null)
                    {
                        employee.employeetailoringincentive += e.incentive;
                        employee.employeetailoringvalue += e.salevalue;
                        employee.employeetailoringqty += e.saleqty;
                        employee.amount += e.incentive;
                    }
                });
            }


            // Barcode Prefix
            if (searchmodes.Exists(e => e == 0) || searchmodes.Exists(e => e == 6))
            {
                var searchIncentives = SearchPrefixIncentives();
                searchIncentives.ForEach(e =>
                {
                    var employee = allEmployees.Find(x => x.employeeid == e.salesmanid);
                    if (employee != null)
                    {
                        employee.prefixincentive += e.incentive;
                        employee.prefixcount += e.incentivecount;
                        employee.prefixvalue += e.salevalue;
                        employee.amount += e.incentive;
                    }
                });
            }


            //Employee By Product Age
            if (searchmodes.Exists(e => e == 0) || searchmodes.Exists(e => e == 88))
            {
                SearchEmployeeRoleIncentives(allEmployees);
            }

            //Employee By Product Age
            if (searchmodes.Exists(e => e == 0) || searchmodes.Exists(e => e == 5))
            {
                if ((all || allEmployees.Count == 1) || allEmployees.Exists(e => e.CalculationSource == IncentiveSalary.IncentiveData.Item.CalculationSources.EmployeeProductAge))
                {
                    SearchEmployeeAgeRangeIncentives((all || allEmployees.Count == 1) ? allEmployees : allEmployees.FindAll(e => e.CalculationSource == IncentiveSalary.IncentiveData.Item.CalculationSources.EmployeeProductAge));
                }
            }


            //  Product By Age
            if (searchmodes.Exists(e => e == 0) || searchmodes.Exists(e => e == 4))
            {
                if ((all || allEmployees.Count == 1) || allEmployees.Exists(e => e.CalculationSource == IncentiveSalary.IncentiveData.Item.CalculationSources.ProductAgeSlab))
                    SearchProductAgeRangeIncentives((all || allEmployees.Count == 1) ? allEmployees : allEmployees.FindAll(e => e.CalculationSource == IncentiveSalary.IncentiveData.Item.CalculationSources.ProductAgeSlab));
            }

            //  Product By Age
            if (searchmodes.Exists(e => e == 0) || searchmodes.Exists(e => e == 15))
            {
                if ((all || allEmployees.Count == 1) || allEmployees.Exists(e => e.CalculationSource == IncentiveSalary.IncentiveData.Item.CalculationSources.ProductAgeAndPrice))
                    SearchProductPriceAgeRangeIncentives((all || allEmployees.Count == 1) ? allEmployees : allEmployees.FindAll(e => e.CalculationSource == IncentiveSalary.IncentiveData.Item.CalculationSources.ProductAgeAndPrice));
            }

            if (searchmodes.Exists(e => e == 0) || searchmodes.Exists(e => e == 3))
            {
                if ((all || allEmployees.Count == 1) || allEmployees.Exists(e => e.CalculationSource == IncentiveSalary.IncentiveData.Item.CalculationSources.SectionAgeSlab))
                    SearchSectionAgeRangeIncentives((all || allEmployees.Count == 1) ? allEmployees : allEmployees.FindAll(e => e.CalculationSource == IncentiveSalary.IncentiveData.Item.CalculationSources.SectionAgeSlab));
            }

            if (searchmodes.Exists(e => e == 0) || searchmodes.Exists(e => e == 17))
            {
                if ((all || allEmployees.Count == 1) || allEmployees.Exists(e => e.CalculationSource == IncentiveSalary.IncentiveData.Item.CalculationSources.EmployeeSection))
                    SearchEmployeeSectionIncentives((all || allEmployees.Count == 1) ? allEmployees : allEmployees.FindAll(e => e.CalculationSource == IncentiveSalary.IncentiveData.Item.CalculationSources.EmployeeSection));
            }

            if (searchmodes.Exists(e => e == 0) || searchmodes.Exists(e => e == 1))
            {
                if ((all || allEmployees.Count == 1) || allEmployees.Exists(e => e.CalculationSource == IncentiveSalary.IncentiveData.Item.CalculationSources.ProductPriceSlab))
                    SearchProductPriceRangeIncentives((all || allEmployees.Count == 1) ? allEmployees : allEmployees.FindAll(e => e.CalculationSource == IncentiveSalary.IncentiveData.Item.CalculationSources.ProductPriceSlab));
            }

            if (searchmodes.Exists(e => e == 0) || searchmodes.Exists(e => e == 10))
            {
                if ((all || allEmployees.Count == 1) || allEmployees.Exists(e => e.CalculationSource == IncentiveSalary.IncentiveData.Item.CalculationSources.SectionPriceSlab))
                    SearchSectionPriceRangeIncentives((all || allEmployees.Count == 1) ? allEmployees : allEmployees.FindAll(e => e.CalculationSource == IncentiveSalary.IncentiveData.Item.CalculationSources.SectionPriceSlab));
            }

            // Sales Value By Slab
            if (searchmodes.Exists(e => e == 0) || searchmodes.Exists(e => e == 7))
            {
                if ((all || allEmployees.Count == 1) || allEmployees.Exists(e => e.CalculationSource == IncentiveSalary.IncentiveData.Item.CalculationSources.ValueSlab))
                    SearchSalesSlabIncentives((all || allEmployees.Count == 1) ? allEmployees : allEmployees.FindAll(e => e.CalculationSource == IncentiveSalary.IncentiveData.Item.CalculationSources.ValueSlab), SalesValue);
            }

            // Target Value By Slab
            if (searchmodes.Exists(e => e == 0) || searchmodes.Exists(e => e == 77))
            {
                if ((all || allEmployees.Count == 1))
                    SearchSalesSlabTargeIncentives((all || allEmployees.Count == 1) ? allEmployees : allEmployees.FindAll(e => e.CalculationSource == IncentiveSalary.IncentiveData.Item.CalculationSources.ValueSlab), SalesValue);
            }

            // Target / Gross / Days
            if (searchmodes.Exists(e => e == 0) || searchmodes.Exists(e => e == 8))
            {
                if ((all || allEmployees.Count == 1) || allEmployees.Exists(e => e.CalculationSource == IncentiveSalary.IncentiveData.Item.CalculationSources.TargetSlab))
                    SearchTargetSlabIncentives((all || allEmployees.Count == 1) ? allEmployees : allEmployees.FindAll(e => e.CalculationSource == IncentiveSalary.IncentiveData.Item.CalculationSources.TargetSlab), SalesValue);
            }

            if (searchmodes.Exists(e => e == 0) || searchmodes.Exists(e => e == 9))
            {
                if ((all || allEmployees.Count == 1) || allEmployees.Exists(e => e.CalculationSource == IncentiveSalary.IncentiveData.Item.CalculationSources.Company))
                    SearchCompanyIncentives((all || allEmployees.Count == 1) ? allEmployees : allEmployees.FindAll(e => e.CalculationSource == IncentiveSalary.IncentiveData.Item.CalculationSources.Company), SalesValue);
            }

            // Fixed %
            if (searchmodes.Exists(e => e == 0) || searchmodes.Exists(e => e == 44))
            {
                allEmployees.Where(e => e.CalculationSource == IncentiveSalary.IncentiveData.Item.CalculationSources.Fixed)
                    .ToList().ForEach(e =>
                {
                    switch (e.Incentivesource)
                    {
                        case IncentiveSalary.IncentiveData.Item.IncentiveSources.TotalSales:
                        case IncentiveSalary.IncentiveData.Item.IncentiveSources.Company:
                            e.salesvalue = totalSales;
                            break;
                        case IncentiveSalary.IncentiveData.Item.IncentiveSources.LocationSales:
                            e.salesvalue = SalesValue != null ? SalesValue.Sum(s => s.locationid == e.locationid ? e.amount : 0) : 0;
                            break;
                        case IncentiveSalary.IncentiveData.Item.IncentiveSources.SectionSales:
                            e.salesvalue = SalesValue != null ? SalesValue.Sum(s => s.sectionid == e.sectionid ? e.amount : 0) : 0;
                            break;
                        case IncentiveSalary.IncentiveData.Item.IncentiveSources.TeamSales:
                            e.salesvalue = result.Sum(e1 => e1.Employees.Sum(e2 => e2.inchargeid == e.employeeid ? e2.salesvalue : 0));
                            break;
                        case IncentiveSalary.IncentiveData.Item.IncentiveSources.FloorSales:
                            e.salesvalue = SalesValue != null ? SalesValue.Sum(s => s.floorid == e.floorid ? e.amount : 0) : 0;
                            break;
                    }

                    if (e.ValueType == IncentiveSalary.IncentiveData.Item.ValueTypes.Value ||
                    e.ValueType == IncentiveSalary.IncentiveData.Item.ValueTypes.None)
                    {
                        e.incentive = Math.Round((e.incentivepercentage * e.salesvalue / 100));
                        e.amount += e.incentive;
                    }
                    else if (e.ValueType == IncentiveSalary.IncentiveData.Item.ValueTypes.Quantity)
                    {
                        e.incentive = Math.Round((e.incentivepercentage * e.salesqty));
                        e.amount += e.incentive;
                    }
                });
            }

            if (InfyPOS.Services.ConfigManager.Instance(this.organizationid).Config.incentivebyworkingdays)
            {
                allEmployees.ForEach(e =>
                {
                    if (e.payratio <= 0)
                        e.payratio = 1;
                    e.amount = Math.Round(e.amount * e.payratio, 0);
                });
            }

            allEmployees.ForEach(e =>
            {
                if (e.ratio <= 0)
                    e.ratio = 1;
                e.amount = Math.Round(e.amount * e.ratio, 0);
                e.fixedincentive = e.amount;
            });

            return result;
        }

        private void SearchCompanyIncentives(List<IncentiveSalary.IncentiveData.Item> list,
            List<IncentiveSalary.IncentiveData.salevalue> SalesValue)
        {

            Data.ReferencelistDataService referencelistDataService = new Data.ReferencelistDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            var config = referencelistDataService.Search(new Models.ReferencelistCreteria()
            {
                locationid = locationid,
                type = "INCENTIVE-COMPANY"
            });


            if (config.Count == 0)
            {
                config = referencelistDataService.Search(new Models.ReferencelistCreteria()
                {
                    type = "INCENTIVE-COMPANY"
                });

                if (config.Exists(e => e.locationid == -1))
                {
                    config.RemoveAll(e => e.locationid != -1);
                }
                else if (!config.All(e => e.locationid == -1))
                {
                    config.RemoveAll(e => e.locationid != config[0].locationid);
                }
            }

            if (config.Count > 0 && config.First().additionalinfo != null && config.First().additionalinfo.salesincentive != null
                && config.First().additionalinfo.salesincentive.Count > 0)
            {

                string query = @"
            with salaryheader as (
			SELECT id, employeeid, fromdate, inchargeid, row_number() OVER (PARTITION BY employeeid ORDER BY fromdate DESC,id DESC) as ranknumber  
			FROM employeesalaryheader where fromdate<=@todate)

            select sum(billitems.receivable) as amount,salescounterid as sectionid,bill.companyid,bill.locationid,
            bill.floorid,sum(case when billitems.additionaldiscount>0 then billitems.receivable else 0 end) additionaldiscount,
            sum(case when billitems.schemediscount>0 then billitems.receivable else 0 end) schemediscount,
            sum(case when billitems.additionaldiscount>0 then billitems.receivable else 0 end) additionaldiscount,salesmanid as employeeid
            from billitems
            inner join bill on bill.id = billitems.billid
            left outer join stock on stock.id = billitems.stockid
            left outer join products on products.id = stock.productid
            where bill.isactive=true and billitems.isactive=true  and billitems.qty!=0 and billdate>=@fromdate and billdate<@todate 
            and bill.locationid=@locationid and bill.companyid=@companyid and bill.billtype != 9 and bill.isb2b=false and salescounterid=@sectionid
            and (salesmanid=@employeeid or salesmanid in (select employeeid from salaryheader where ranknumber = 1 and inchargeid=@employeeid)) 
            group by salescounterid,bill.companyid,bill.locationid,bill.floorid,salesmanid;";

                SortedList<string, object> param = new SortedList<string, object>();
                param.Add("salarymonth", fromdate);
                param.Add("fromdate", fromdate.Date);
                param.Add("locationid", locationid);
                param.Add("employeeid", employeeid);
                param.Add("companyid", companyid);
                param.Add("sectionid", sectionid);
                param.Add("todate", todate.Date.AddDays(1));
                if (companyid == 0)
                {
                    query = query.Replace("and bill.companyid=@companyid", "");
                }
                if (sectionid == 0)
                {
                    query = query.Replace("and salescounterid=@sectionid", "");
                }

                if (employeeid == 0)
                {
                    query = query.Replace("and (salesmanid=@employeeid or salesmanid in (select employeeid from salaryheader where ranknumber = 1 and inchargeid=@employeeid))", "");
                }
                if (locationid == 0)
                {
                    if (accessiblelocation == null || accessiblelocation.Contains(-1))
                    {
                        query = query.Replace("and bill.locationid=@locationid", "");
                    }
                    else
                    {
                        query = query.Replace("and bill.locationid=@locationid", "and bill.locationid in (" + string.Join(",", accessiblelocation) + ")");
                    }
                }
                Common.BaseData baseData = new Common.BaseData() { Context = new Common.UserClaim() { OrganizationID = organizationid } };

                if (InfyPOS.Services.ConfigManager.Instance(organizationid).Config.payroll_incentive_addb2b)
                    query = query.Replace("bill.isb2b=false", "bill.isexport=false");

                if (InfyPOS.Services.ConfigManager.Instance(organizationid).Config.payroll_incentive_addexport)
                    query = query.Replace("and bill.isexport=false", "");

                if (InfyPOS.Services.ConfigManager.Instance(organizationid).Config.system_isb2bapplication)
                    query = query.Replace("and bill.isb2b=false", "");
                if (sectionid <= 0)
                    query = query.Replace("and salescounterid=@sectionid", "");


                using (var reader = baseData.ExecuteReader(query, param))
                {
                    SalesValue = new List<IncentiveSalary.IncentiveData.salevalue>();
                    while (reader.Read())
                    {
                        long sectionid = reader["sectionid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sectionid"]);
                        long companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"]);
                        long floorid = reader["floorid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["floorid"]);
                        long locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                        long employeeid = reader["employeeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["employeeid"]);
                        decimal amount = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"]);
                        decimal additionaldiscount = reader["additionaldiscount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["additionaldiscount"]);
                        decimal schemediscount = reader["schemediscount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["schemediscount"]);

                        SalesValue.Add(new IncentiveSalary.IncentiveData.salevalue()
                        {
                            amount = amount,
                            additionaldiscount = additionaldiscount,
                            schemediscount = schemediscount,
                            companyid = companyid,
                            sectionid = sectionid,
                            locationid = locationid,
                            floorid = floorid,
                            employeeid = employeeid
                        });

                    }
                }

                List<Range> rangelist = new List<Range>();
                foreach (var item in config.First().additionalinfo.salesincentive.OrderBy(e => e.from))
                {
                    var rng = new Range()
                    {
                        start = (int)item.from,
                        incentive = item.incentive,
                        prefix = item.prefix,
                        sincentive = item.sincentive,
                        referenceid = item.referenceid,
                        value = item.value,
                        id = string.Format("{0}_{1}", item.referenceid, rangelist.Count.ToString())
                    };
                    rangelist.Add(rng);
                }

                var allsalesmenconfig = rangelist.FindAll(ex => ex.start == 0).ToList();
                var allinchargeconfig = rangelist.FindAll(ex => ex.start == -1).ToList();
                var allmanagementconfig = rangelist.FindAll(ex => ex.start == -2).ToList();

                list.ForEach(e =>
                {
                    try
                    {
                        foreach (var item in SalesValue.GroupBy(ec => ec.companyid))
                        {
                            var listofvalues = list.FindAll(ex => ex.employeeid == e.employeeid);
                            if (allsalesmenconfig.Exists(es => es.referenceid == item.Key))
                            {
                                var saleamount = item.Sum(ex => ex.employeeid == e.employeeid && ex.companyid == item.Key ? ex.amount : 0);
                                if (InfyPOS.Services.ConfigManager.Instance(this.organizationid).Config.payroll_incentive_removeprefix)
                                    saleamount -= listofvalues.Sum(ex => ex.employeeid == e.employeeid && ex.companyid == item.Key ? ex.prefixvalue : 0);
                                if (InfyPOS.Services.ConfigManager.Instance(this.organizationid).Config.payroll_incentive_removebarcode)
                                    saleamount -= listofvalues.Sum(ex => ex.employeeid == e.employeeid && ex.companyid == item.Key ? ex.itemsalevalue : 0);
                                if (InfyPOS.Services.ConfigManager.Instance(this.organizationid).Config.payroll_incentive_removeproduct)
                                    saleamount -= listofvalues.Sum(ex => ex.employeeid == e.employeeid && ex.companyid == item.Key ? ex.productlevelvalue : 0);

                                var incentivepercentage = allsalesmenconfig.Find(es => es.referenceid == item.Key).incentive;
                                e.amount += Math.Round(saleamount * incentivepercentage / 100, 0);
                                e.companyincentive += Math.Round(saleamount * incentivepercentage / 100, 0);
                            }
                        }
                    }
                    catch (Exception exp)
                    {

                    }
                });


                //list.ForEach(e =>
                //{
                //    try
                //    {
                //        foreach (var item in SalesValue.GroupBy(ec => ec.companyid))
                //        {
                //            if (e.IsSalesmen && allsalesmenconfig.Exists(es => es.referenceid == item.Key))
                //            {
                //                var saleamount = item.Sum(ex => ex.employeeid == e.employeeid ? ex.amount : 0);
                //                if (InfyPOS.Services.ConfigManager.Instance(this.organizationid).Config.payroll_incentive_removeprefix)
                //                    saleamount -= e.prefixvalue;
                //                if (InfyPOS.Services.ConfigManager.Instance(this.organizationid).Config.payroll_incentive_removebarcode)
                //                    saleamount -= e.itemsalevalue;
                //                if (InfyPOS.Services.ConfigManager.Instance(this.organizationid).Config.payroll_incentive_removeproduct)
                //                    e.salesvalue -= e.productlevelvalue;

                //                var incentivepercentage = e.incentivepercentage > 0 ? e.incentivepercentage : allsalesmenconfig.Find(es => es.referenceid == item.Key).incentive;
                //                e.amount += saleamount * incentivepercentage / 100;
                //                e.companyincentive += saleamount * incentivepercentage / 100;

                //            }
                //            else if (e.IsIncharge && allinchargeconfig.Exists(es => es.referenceid == item.Key))
                //            {
                //                var listofvalues = list.FindAll(ex => ex.inchargeid == e.employeeid);
                //                var saleamount = item.Sum(ex => listofvalues.Exists(yx => yx.employeeid == ex.employeeid) ? ex.amount : 0);

                //                if (InfyPOS.Services.ConfigManager.Instance(this.organizationid).Config.payroll_incentive_removeprefix)
                //                    saleamount -= listofvalues.Sum(ex => ex.prefixvalue);
                //                if (InfyPOS.Services.ConfigManager.Instance(this.organizationid).Config.payroll_incentive_removebarcode)
                //                    saleamount -= listofvalues.Sum(ex => ex.itemsalevalue);
                //                if (InfyPOS.Services.ConfigManager.Instance(this.organizationid).Config.payroll_incentive_removeproduct)
                //                    saleamount -= listofvalues.Sum(ex => ex.productlevelvalue);

                //                var incentivepercentage = e.incentivepercentage > 0 ? e.incentivepercentage : allinchargeconfig.Find(es => es.referenceid == item.Key).incentive;
                //                e.amount += saleamount * incentivepercentage / 100;
                //                e.companyincentive += saleamount * incentivepercentage / 100;
                //            }
                //            else if (allmanagementconfig.Count > 0)
                //            {
                //                var saleamount = item.Sum(ex => ex.amount);

                //                if (InfyPOS.Services.ConfigManager.Instance(this.organizationid).Config.payroll_incentive_removeprefix)
                //                    saleamount -= list.Sum(ex => ex.prefixvalue);
                //                if (InfyPOS.Services.ConfigManager.Instance(this.organizationid).Config.payroll_incentive_removebarcode)
                //                    saleamount -= list.Sum(ex => ex.itemsalevalue);
                //                if (InfyPOS.Services.ConfigManager.Instance(this.organizationid).Config.payroll_incentive_removeproduct)
                //                    saleamount -= list.Sum(ex => ex.productlevelvalue);

                //                var incentivepercentage = e.incentivepercentage > 0 ? e.incentivepercentage : allmanagementconfig.Find(es => es.referenceid == item.Key).incentive;
                //                e.amount += saleamount * incentivepercentage / 100;
                //                e.companyincentive += saleamount * incentivepercentage / 100;
                //            }
                //            else if (rangelist.Exists(es => es.referenceid == item.Key))
                //            {
                //                var saleamount = item.Sum(ex => ex.amount);

                //                if (InfyPOS.Services.ConfigManager.Instance(this.organizationid).Config.payroll_incentive_removeprefix)
                //                    saleamount -= list.Sum(ex => ex.prefixvalue);
                //                if (InfyPOS.Services.ConfigManager.Instance(this.organizationid).Config.payroll_incentive_removebarcode)
                //                    saleamount -= list.Sum(ex => ex.itemsalevalue);
                //                if (InfyPOS.Services.ConfigManager.Instance(this.organizationid).Config.payroll_incentive_removeproduct)
                //                    saleamount -= list.Sum(ex => ex.productlevelvalue);


                //                var incentivepercentage = e.incentivepercentage > 0 ? e.incentivepercentage : rangelist.Find(es => es.referenceid == item.Key).incentive;
                //                e.amount += saleamount * incentivepercentage / 100;
                //                e.companyincentive += saleamount * incentivepercentage / 100;
                //            }
                //        }
                //    }
                //    catch (Exception exp)
                //    {

                //    }
                //});
            }
        }

        private void SearchTargetSlabIncentives(List<IncentiveSalary.IncentiveData.Item> list,
    List<IncentiveSalary.IncentiveData.salevalue> SalesValue)
        {
            Data.ReferencelistDataService referencelistDataService = new Data.ReferencelistDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            var config = referencelistDataService.Search(new Models.ReferencelistCreteria()
            {
                locationid = locationid,
                type = "INCENTIVE-TARGET-GROSS-DAYS"
            });

            if (config.Count == 0)
            {
                config = referencelistDataService.Search(new Models.ReferencelistCreteria()
                {
                    type = "INCENTIVE-TARGET-GROSS-DAYS"
                });

                if (config.Exists(e => e.locationid == -1))
                {
                    config.RemoveAll(e => e.locationid != -1);
                }
                else if (!config.All(e => e.locationid == -1))
                {
                    config.RemoveAll(e => e.locationid != config[0].locationid);
                }
            }

            if (config.Count > 0 && config.First().additionalinfo != null && config.First().additionalinfo.salesincentive != null
                && config.First().additionalinfo.salesincentive.Count > 0)
            {
                List<Range> rangelist = new List<Range>();
                foreach (var item in config.First().additionalinfo.salesincentive.OrderBy(e => e.from))
                {

                    var rng = new Range()
                    {
                        start = (int)item.from,
                        end = (int)item.to,
                        incentive = item.incentive,
                        prefix = item.prefix,
                        sincentive = item.sincentive,
                        referenceid = item.referenceid,
                        value = item.value,
                        id = string.Format("{0}_{1}", item.referenceid, rangelist.Count.ToString())
                    };
                    rangelist.Add(rng);

                }

                var allsalesmenconfig = rangelist.FindAll(ex => ex.referenceid == 0).OrderBy(e => e.start).ToList();
                var allinchargeconfig = rangelist.FindAll(ex => ex.referenceid == -1).OrderBy(e => e.start).ToList();
                var allmanagementconfig = rangelist.FindAll(ex => ex.referenceid == -2).OrderBy(e => e.start).ToList();

                list.ForEach(e =>
                {
                    if (list.Exists(i => i.inchargeid == e.employeeid && i.salesvalue > 0))
                    {
                        e.IsIncharge = true;
                        e.IsSalesmen = false;
                        e.salesvalue += list.Sum(e2 => e2.inchargeid == e.employeeid ? e2.salesvalue : 0);
                    }
                });
                list.Where(e => !e.IsIncharge).ToList().ForEach(e =>
                  {
                      try
                      {

                          switch (e.Incentivesource)
                          {
                              case IncentiveSalary.IncentiveData.Item.IncentiveSources.TotalSales:
                              case IncentiveSalary.IncentiveData.Item.IncentiveSources.Company:
                                  e.salesvalue = totalSales;
                                  break;
                              case IncentiveSalary.IncentiveData.Item.IncentiveSources.LocationSales:
                                  e.salesvalue = SalesValue != null ? SalesValue.Sum(s => s.locationid == e.locationid ? e.amount : 0) : 0;
                                  break;
                              case IncentiveSalary.IncentiveData.Item.IncentiveSources.SectionSales:
                                  e.salesvalue = SalesValue != null ? SalesValue.Sum(s => s.sectionid == e.sectionid ? e.amount : 0) : 0;
                                  break;
                              case IncentiveSalary.IncentiveData.Item.IncentiveSources.TeamSales:
                                  e.salesvalue = list.Sum(e2 => e2.inchargeid == e.employeeid ? e2.salesvalue : 0);
                                  break;
                              case IncentiveSalary.IncentiveData.Item.IncentiveSources.FloorSales:
                                  e.salesvalue = SalesValue != null ? SalesValue.Sum(s => s.floorid == e.floorid ? e.amount : 0) : 0;
                                  break;
                          }

                          if (e.salesvalue > 0)
                          {
                              var configlist = rangelist.FindAll(ex => ex.referenceid == e.employeeid);
                              if (configlist.Count == 0)
                              {
                                  if (e.IsSalesmen)
                                      configlist = allsalesmenconfig;
                                  else if (e.IsIncharge)
                                      configlist = allinchargeconfig;
                                  else
                                      configlist = allmanagementconfig;
                              }

                              IncentiveSalary.IncentiveData.Item incharge = null;

                              if (list.Exists(e2 => e2.employeeid == e.inchargeid && e.Incentivesource == IncentiveSalary.IncentiveData.Item.IncentiveSources.Salesmen))
                              {
                                  incharge = list.Find(e2 => e2.employeeid == e.inchargeid && e.Incentivesource == IncentiveSalary.IncentiveData.Item.IncentiveSources.Salesmen);
                              }

                              decimal availabledays = 0;
                              decimal targetamount = e.salestarget;
                              if (e.TargetMethod == IncentiveSalary.IncentiveData.Item.TargetMethods.NoOfWorkingsXGross)
                              {
                                  targetamount = e.workingdays * e.grosspay;
                              }
                              if (e.TargetMethod == IncentiveSalary.IncentiveData.Item.TargetMethods.FixedDaysXGross)
                              {
                                  targetamount = e.grosspay * e.salestarget / e.totalworkingdays * e.workingdays;
                              }

                              //e.additionaldiscount = SalesValue.Sum(ad => ad.employeeid == e.employeeid ? ad.additionaldiscount : 0);
                              //e.schemediscount = SalesValue.Sum(ad => ad.employeeid == e.employeeid ? ad.schemediscount : 0);
                              e.targetamount = targetamount;
                              if (configlist.Count > 0 && targetamount < e.salesvalue)
                              {
                                  var incentivesalevalue = e.salesvalue;
                                  if (configlist.Exists(ex => ex.start == 0 || ex.end == 0))
                                  {
                                      if (InfyPOS.Services.ConfigManager.Instance(this.organizationid).Config.payroll_incentive_removebarcode)
                                          incentivesalevalue -= e.itemsalevalue;
                                      if (InfyPOS.Services.ConfigManager.Instance(this.organizationid).Config.payroll_incentive_removeprefix)
                                          incentivesalevalue -= e.prefixvalue;
                                      if (InfyPOS.Services.ConfigManager.Instance(this.organizationid).Config.payroll_incentive_removeproduct)
                                          incentivesalevalue -= e.productlevelvalue;
                                      if (InfyPOS.Services.ConfigManager.Instance(this.organizationid).Config.payroll_incentive_addiscount)
                                          incentivesalevalue -= e.additionaldiscount;

                                      e.salesvalue = incentivesalevalue;
                                      //- (e.itemsalevalue + e.prefixvalue + e.productlevelvalue);
                                      if (incentivesalevalue > 0)
                                      {
                                          var computeincentive = targetamount < incentivesalevalue ? targetamount : incentivesalevalue;
                                          incentivesalevalue -= computeincentive;

                                          var cl = configlist.Find(ex => ex.start == 0 || ex.end == 0);
                                          e.incentive += (cl.incentive * computeincentive / 100);
                                          e.amount += (cl.incentive * computeincentive / 100);
                                          e.targetslapincentive += (cl.incentive * computeincentive / 100);

                                          if (incharge != null)
                                          {
                                              incharge.salesvalue += computeincentive;
                                              incharge.incentive += (cl.sincentive * computeincentive / 100);
                                              incharge.amount += (cl.sincentive * computeincentive / 100);
                                              incharge.targetslapincentive += (cl.sincentive * computeincentive / 100);
                                              if (incharge.notes == null)
                                              {
                                                  incharge.notes = new List<KeyValuePair<string, string>>();
                                              }
                                              incharge.notes.Add(new KeyValuePair<string, string>("Sales man", e.employeename));
                                              incharge.notes.Add(new KeyValuePair<string, string>("Target", computeincentive.ToString("N0")));
                                              incharge.notes.Add(new KeyValuePair<string, string>("Base Incentive X " + cl.sincentive.ToString("N2"), (cl.sincentive * computeincentive / 100).ToString("N0")));
                                          }
                                      }

                                      if (incentivesalevalue > 0)
                                          availabledays = Math.Abs(incentivesalevalue / e.grosspay);


                                  }

                                  if (e.notes == null)
                                      e.notes = new List<KeyValuePair<string, string>>();
                                  e.notes.Add(new KeyValuePair<string, string>("Accepted Sale Value", incentivesalevalue.ToString("N0")));
                                  e.notes.Add(new KeyValuePair<string, string>("Gross Pay", e.grosspay.ToString("N0")));
                                  e.notes.Add(new KeyValuePair<string, string>("Availabledays", availabledays.ToString("N2")));


                                  if (availabledays > 0)
                                  {
                                      foreach (var cl in configlist.Where(w => w.start > 0 && w.end > 0).OrderByDescending(y => y.end))
                                      {
                                          if (availabledays > 0 &&
                                              cl.start < availabledays)
                                          {
                                              var incentivesinglerow = (cl.incentive * (availabledays - (cl.start - 1)) * e.grosspay / 100);

                                              e.notes.Add(new KeyValuePair<string, string>("Incentive X Days X Gross % ",
                                                  string.Format("{0:N2} X {1:N0} X {2:N0} = {3:N0}", cl.incentive,
                                                  (availabledays - (cl.start - 1)), e.grosspay, incentivesinglerow)));

                                              e.amount += incentivesinglerow;
                                              e.incentive += (cl.incentive * (availabledays - (cl.start - 1)) * e.grosspay / 100);
                                              e.targetslapincentive += (cl.incentive * (availabledays - (cl.start - 1)) * e.grosspay / 100);
                                              if (incharge != null)
                                              {
                                                  if (incharge.notes == null)
                                                  {
                                                      incharge.notes = new List<KeyValuePair<string, string>>();
                                                  }

                                                  incentivesinglerow = (cl.sincentive * (availabledays - (cl.start - 1)) * e.grosspay / 100);
                                                  incharge.notes.Add(new KeyValuePair<string, string>("Incentive X Days X Gross % ",
                                                   string.Format("{0:N2} X {1:N0} X {2:N0} = {3:N0}", cl.sincentive,
                                                   (availabledays - (cl.start - 1)), e.grosspay, incentivesinglerow)));

                                                  incharge.salesvalue += (availabledays - (cl.start - 1)) * e.grosspay;
                                                  incharge.amount += incentivesinglerow;
                                                  incharge.incentive += (cl.sincentive * (availabledays - (cl.start - 1)) * e.grosspay / 100);
                                                  incharge.targetslapincentive += (cl.incentive * (availabledays - (cl.start - 1)) * e.grosspay / 100);
                                              }
                                              availabledays -= (availabledays - (cl.start - 1));
                                          }
                                      }
                                  }

                                  if (availabledays > 0)
                                  {
                                      var lastconfig = configlist.Where(w => w.start > 0 && w.end > 0).OrderByDescending(y => y.end).Last();

                                      var incentivesinglerow = (lastconfig.incentive * availabledays * e.grosspay / 100);

                                      e.notes.Add(new KeyValuePair<string, string>("Incentive X Days X Gross % ",
                                          string.Format("{0:N2} X {1:N0} X {2:N0} = {3:N0}", lastconfig.incentive,
                                          (availabledays - (lastconfig.start - 1)), e.grosspay, incentivesinglerow)));

                                      e.amount += incentivesinglerow;
                                      e.incentive += incentivesinglerow;
                                      e.targetslapincentive += incentivesinglerow;
                                      if (incharge != null)
                                      {
                                          incharge.salesvalue += (availabledays * e.grosspay);
                                          if (incharge.notes == null)
                                          {
                                              incharge.notes = new List<KeyValuePair<string, string>>();
                                          }

                                          incentivesinglerow = (lastconfig.sincentive * availabledays * e.grosspay / 100);
                                          incharge.notes.Add(new KeyValuePair<string, string>("Incentive X Days X Gross % ",
                                           string.Format("{0:N2} X {1:N0} X {2:N0} = {3:N0}", lastconfig.sincentive,
                                           (availabledays - (lastconfig.start - 1)), e.grosspay, incentivesinglerow)));

                                          incharge.amount += incentivesinglerow;
                                          incharge.incentive += incentivesinglerow;
                                          incharge.targetslapincentive += incentivesinglerow;
                                      }
                                  }
                              }
                          }
                      }
                      catch (Exception exp)
                      {

                      }
                  });
            }
        }


        private void SearchSalesSlabTargeIncentives(List<IncentiveSalary.IncentiveData.Item> list,
        List<IncentiveSalary.IncentiveData.salevalue> SalesValue)
        {
            Data.ReferencelistDataService referencelistDataService = new Data.ReferencelistDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            var config = referencelistDataService.Search(new Models.ReferencelistCreteria()
            {
                locationid = locationid,
                type = "INCENTIVE-TARGET-SLAB"
            });

            if (config.Count == 0)
            {
                config = referencelistDataService.Search(new Models.ReferencelistCreteria()
                {
                    type = "INCENTIVE-TARGET-SLAB"
                });

                if (config.Exists(e => e.locationid == -1))
                {
                    config.RemoveAll(e => e.locationid != -1);
                }
                else if (!config.All(e => e.locationid == -1))
                {
                    config.RemoveAll(e => e.locationid != config[0].locationid);
                }
            }

            if (config.Count > 0 && config.Count > 0 && config.First().additionalinfo != null && config.First().additionalinfo.salesincentive != null
                && config.First().additionalinfo.salesincentive.Count > 0)
            {
                List<Range> rangelist = new List<Range>();
                foreach (var item in config.First().additionalinfo.salesincentive.OrderBy(e => e.from))
                {
                    if (item.from > 0 || item.to > 0)
                    {
                        var rng = new Range()
                        {
                            start = (int)item.from,
                            end = (int)item.to,
                            incentive = item.incentive,
                            prefix = item.prefix,
                            referenceid = item.referenceid,
                            value = item.value,
                            id = string.Format("{0}_{1}", item.referenceid, rangelist.Count.ToString())
                        };
                        rangelist.Add(rng);
                    }
                }

                var allsalesmenconfig = rangelist.FindAll(ex => ex.referenceid == 0).OrderBy(e => e.start).ToList();
                var allinchargeconfig = rangelist.FindAll(ex => ex.referenceid == -1).OrderBy(e => e.start).ToList();
                var allmanagementconfig = rangelist.FindAll(ex => ex.referenceid == -2).OrderBy(e => e.start).ToList();

                list.ForEach(e =>
                {
                    switch (e.Incentivesource)
                    {
                        case IncentiveSalary.IncentiveData.Item.IncentiveSources.TotalSales:
                        case IncentiveSalary.IncentiveData.Item.IncentiveSources.Company:
                            e.salesvalue = totalSales;
                            break;
                        case IncentiveSalary.IncentiveData.Item.IncentiveSources.LocationSales:
                            e.salesvalue = SalesValue != null ? SalesValue.Sum(s => s.locationid == e.locationid ? e.amount : 0) : 0;
                            break;
                        case IncentiveSalary.IncentiveData.Item.IncentiveSources.SectionSales:
                            e.salesvalue = SalesValue != null ? SalesValue.Sum(s => s.sectionid == e.sectionid ? e.amount : 0) : 0;
                            break;
                        case IncentiveSalary.IncentiveData.Item.IncentiveSources.TeamSales:
                            e.salesvalue = list.Sum(e2 => e2.inchargeid == e.employeeid ? e2.salesvalue : 0);
                            break;
                        case IncentiveSalary.IncentiveData.Item.IncentiveSources.FloorSales:
                            e.salesvalue = SalesValue != null ? SalesValue.Sum(s => s.floorid == e.floorid ? e.amount : 0) : 0;
                            break;
                        default:
                            if (!e.IsSalesmen && e.salesvalue > 0)
                                e.IsSalesmen = true;
                            break;
                    }

                    if (e.salesvalue > 0)
                    {
                        var configlist = rangelist.FindAll(ex => ex.referenceid == e.employeeid);
                        if (configlist.Count == 0)
                        {
                            if (e.IsSalesmen)
                                configlist = allsalesmenconfig;
                            else if (e.IsIncharge)
                                configlist = allinchargeconfig;
                            else
                                configlist = allmanagementconfig;
                        }

                        if (configlist.Count > 0)
                        {
                            var avilable = e.salesvalue;
                            if (configlist.Exists(x => x.start <= avilable && x.end >= avilable))
                            {
                                var rc = configlist.Find(x => x.start <= avilable && x.end >= avilable);
                                if (rc.value)
                                {
                                    e.amount += (avilable * rc.incentive);
                                    e.targetincentive += (avilable * rc.incentive);
                                }
                                else
                                {
                                    e.amount += Math.Round(avilable * rc.incentive / 100, 0);
                                    e.targetincentive += Math.Round(avilable * rc.incentive / 100, 0);

                                }
                            }
                        }
                    }
                });
            }
        }
        private void SearchSalesSlabIncentives(List<IncentiveSalary.IncentiveData.Item> list,
            List<IncentiveSalary.IncentiveData.salevalue> SalesValue)
        {
            Data.ReferencelistDataService referencelistDataService = new Data.ReferencelistDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            var config = referencelistDataService.Search(new Models.ReferencelistCreteria()
            {
                locationid = locationid,
                type = "INCENTIVE-SALES-VALUE"
            });

            if (config.Count == 0)
            {
                config = referencelistDataService.Search(new Models.ReferencelistCreteria()
                {
                    type = "INCENTIVE-SALES-VALUE"
                });

                if (config.Exists(e => e.locationid == -1))
                {
                    config.RemoveAll(e => e.locationid != -1);
                }
                else if (!config.All(e => e.locationid == -1))
                {
                    config.RemoveAll(e => e.locationid != config[0].locationid);
                }
            }

            if (config.Count > 0 && config.Count > 0 && config.First().additionalinfo != null && config.First().additionalinfo.salesincentive != null
                && config.First().additionalinfo.salesincentive.Count > 0)
            {
                List<Range> rangelist = new List<Range>();
                foreach (var item in config.First().additionalinfo.salesincentive.OrderBy(e => e.from))
                {
                    if (item.from > 0 || item.to > 0)
                    {
                        var rng = new Range()
                        {
                            start = (int)item.from,
                            end = (int)item.to,
                            incentive = item.incentive,
                            prefix = item.prefix,
                            referenceid = item.referenceid,
                            value = item.value,
                            id = string.Format("{0}_{1}", item.referenceid, rangelist.Count.ToString())
                        };
                        rangelist.Add(rng);
                    }
                }

                var allsalesmenconfig = rangelist.FindAll(ex => ex.referenceid == 0).OrderBy(e => e.start).ToList();
                var allinchargeconfig = rangelist.FindAll(ex => ex.referenceid == -1).OrderBy(e => e.start).ToList();
                var allmanagementconfig = rangelist.FindAll(ex => ex.referenceid == -2).OrderBy(e => e.start).ToList();

                list.ForEach(e =>
                {
                    switch (e.Incentivesource)
                    {
                        case IncentiveSalary.IncentiveData.Item.IncentiveSources.TotalSales:
                        case IncentiveSalary.IncentiveData.Item.IncentiveSources.Company:
                            e.salesvalue = totalSales;
                            break;
                        case IncentiveSalary.IncentiveData.Item.IncentiveSources.LocationSales:
                            e.salesvalue = SalesValue != null ? SalesValue.Sum(s => s.locationid == e.locationid ? e.amount : 0) : 0;
                            break;
                        case IncentiveSalary.IncentiveData.Item.IncentiveSources.SectionSales:
                            e.salesvalue = SalesValue != null ? SalesValue.Sum(s => s.sectionid == e.sectionid ? e.amount : 0) : 0;
                            break;
                        case IncentiveSalary.IncentiveData.Item.IncentiveSources.TeamSales:
                            e.salesvalue = list.Sum(e2 => e2.inchargeid == e.employeeid ? e2.salesvalue : 0);
                            break;
                        case IncentiveSalary.IncentiveData.Item.IncentiveSources.FloorSales:
                            e.salesvalue = SalesValue != null ? SalesValue.Sum(s => s.floorid == e.floorid ? e.amount : 0) : 0;
                            break;
                        default:
                            if (!e.IsSalesmen && e.salesvalue > 0)
                                e.IsSalesmen = true;
                            break;
                    }

                    if (e.salesvalue > 0)
                    {
                        var configlist = rangelist.FindAll(ex => ex.referenceid == e.employeeid);
                        if (configlist.Count == 0)
                        {
                            if (e.IsSalesmen)
                                configlist = allsalesmenconfig;
                            else if (e.IsIncharge)
                                configlist = allinchargeconfig;
                            else
                                configlist = allmanagementconfig;
                        }

                        if (configlist.Count > 0)
                        {
                            var avilable = e.salesvalue;
                            if (configlist.Exists(ex => ex.value))
                            {
                                foreach (var cl in configlist)
                                {
                                    if (cl.start <= avilable)
                                    {
                                        e.amount += cl.incentive;
                                        e.salesslabincentive += cl.incentive;
                                    }
                                }
                            }
                            else
                            {
                                foreach (var cl in configlist)
                                {
                                    if (cl.end <= avilable)
                                    {
                                        e.amount += (cl.incentive * (cl.end - cl.start) / 100);
                                        e.salesslabincentive += (cl.incentive * (cl.end - cl.start) / 100);
                                    }
                                    else if ((avilable - cl.start) > 0)
                                    {
                                        e.amount += (cl.incentive * (avilable - cl.start) / 100);
                                        e.salesslabincentive += (cl.incentive * (avilable - cl.start) / 100);
                                    }
                                }
                                if (avilable > configlist.Last().end)
                                {
                                    e.amount += (configlist.Last().incentive * (avilable - configlist.Last().end) / 100);
                                    e.salesslabincentive += (configlist.Last().incentive * (avilable - configlist.Last().end) / 100);
                                }
                            }
                        }
                    }
                });
            }
        }
    }
}
