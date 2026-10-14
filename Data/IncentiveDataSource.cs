using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
using Newtonsoft.Json;
using System.Text;

namespace InfyPOS.Data
{
    public class IncentiveDataSource : SAASData
    {
        const string SQL_incentivies_Section = @"with sectionconfig 
            as 
            (
	            select referencelist.id,code,cast(schemelist ->> 'to' as numeric) as to,cast(schemelist ->> 'from' as numeric) as from
	            ,cast(schemelist ->> 'incentive' as numeric) as incentive from referencelist
	            LEFT JOIN LATERAL
	            json_array_elements (cast(additionalinfo::json->>'salesincentive' as json)) schemelist ON true
	            where type = 'DIVISION' and cast(schemelist ->> 'to' as numeric) is not null 
            )
            select referencelist.name as sectionname,sum(billitems.receivable) as receivable,sum(billitems.qty) qty,sum(billitems.qty*coalesce(incentiveconfig.incentive::numeric,sectionconfig.incentive::numeric)) amount from billitems 
            inner join bill on bill.id =  billitems.billid
            inner join salablegoods on salablegoods.id = salablegoodsid and salablegoods.isactive = true
            inner join stock on stock.id = salablegoods.stockid and stock.isactive = true
            inner join employee on employee.id = salesmanid
            inner join products on products.id = stock.productid
            inner join referencelist on referencelist.id = products.salescounterid
            inner join sectionconfig on sectionconfig.id = salescounterid and billitems.price::numeric between sectionconfig.from and sectionconfig.to
            left outer join incentiveconfig on incentiveconfig.salablegoodsid = salablegoods.id
            where 
            bill.billdate>=@fromdate and bill.billdate<@todate
            AND ( bill.locationid = @locationid and employee.locationid=@locationid )  and (employee.hasleft = false or employee.relievingdate>@fromdate)
            group by referencelist.name";

        const string SQL_incentivies_Products = @"with sectionconfig 
            as 
            (
	            select referencelist.referenceid as id,code,cast(schemelist ->> 'to' as numeric) as to,cast(schemelist ->> 'from' as numeric) as from
	            ,cast(schemelist ->> 'incentive' as numeric) as incentive from referencelist
	            LEFT JOIN LATERAL
	            json_array_elements (cast(additionalinfo::json->>'salesincentive' as json)) schemelist ON true
	            where referencelocationid = @locationid and type = 'INCENTIVE-PRODUCT' and cast(schemelist ->> 'to' as numeric) is not null 
            )
            select referencelist.name as sectionname,sum(billitems.receivable) as receivable,sum(billitems.qty) qty,sum(billitems.qty*coalesce(incentiveconfig.incentive::numeric,sectionconfig.incentive::numeric)) amount from billitems 
            inner join bill on bill.id =  billitems.billid
            inner join salablegoods on salablegoods.id = salablegoodsid and salablegoods.isactive = true
            inner join stock on stock.id = salablegoods.stockid and stock.isactive = true
            inner join employee on employee.id = salesmanid
            inner join products on products.id = stock.productid
            left outer join referencelist on referencelist.id = employee.sectionid
            inner join sectionconfig on sectionconfig.id = stock.productid and billitems.price::numeric between sectionconfig.from and sectionconfig.to
            left outer join incentiveconfig on incentiveconfig.salablegoodsid = salablegoods.id
            where 
            bill.billdate>=@fromdate and bill.billdate<@todate
            AND ( bill.locationid = @locationid and employee.locationid=@locationid )  
            and (employee.hasleft = false or employee.relievingdate>@fromdate)
            group by referencelist.name";

        internal List<IncentiveValue> SearchIncentive(RetailReportCreteria item)
        {
            List<IncentiveValue> saleValues = new List<IncentiveValue>();

            var appconfig = Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig.ContainsKey(item.locationid) ?
                    Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[item.locationid] : null;
            string SQL_incentivies_Section_Employees = @"with sectionconfig 
            as 
            (
	            select referencelist.id,code,cast(schemelist ->> 'to' as numeric) as to,cast(schemelist ->> 'from' as numeric) as from
	            ,cast(schemelist ->> 'incentive' as numeric) as incentive from referencelist
	            LEFT JOIN LATERAL
	            json_array_elements (cast(additionalinfo::json->>'salesincentive' as json)) schemelist ON true
	            where type = 'DIVISION' and cast(schemelist ->> 'to' as numeric) is not null 
            )

            select employee.name,employee.id as employeeid,referencelist.name sectionname,sum(billitems.receivable) as receivable,sum(billitems.qty) qty,
            sum(billitems.qty*coalesce(incentiveconfig.incentive::numeric,sectionconfig.incentive::numeric)) amount,employeecode,
            employee.sectionid from billitems 
            inner join bill on bill.id =  billitems.billid
            inner join salablegoods on salablegoods.id = salablegoodsid and salablegoods.isactive = true
            inner join stock on stock.id = salablegoods.stockid and stock.isactive = true
            inner join employee on employee.id = salesmanid
            inner join products on products.id = stock.productid
            inner join sectionconfig on sectionconfig.id = salescounterid and billitems.price::numeric between sectionconfig.from and sectionconfig.to
            left outer join incentiveconfig on incentiveconfig.salablegoodsid = salablegoods.id
            left outer join referencelist on referencelist.id = employee.sectionid
            where 
            bill.billdate>=@fromdate and bill.billdate<@todate and (employee.hasleft = false or employee.relievingdate>@fromdate)
            AND (bill.locationid = @locationid and employee.locationid=@locationid) 
            group by employee.name,referencelist.name,employee.id,employee.sectionid";

            string SQL_incentivies_Product_Employees = @"with sectionconfig 
            as 
            (
	            select referencelist.referenceid as id,code,cast(schemelist ->> 'to' as numeric) as to,cast(schemelist ->> 'from' as numeric) as from
	            ,cast(schemelist ->> 'incentive' as numeric) as incentive from referencelist
	            LEFT JOIN LATERAL
	            json_array_elements (cast(additionalinfo::json->>'salesincentive' as json)) schemelist ON true
	            where referencelocationid = @locationid and type = 'INCENTIVE-PRODUCT' and cast(schemelist ->> 'to' as numeric) is not null 
            )

            select employee.name,employee.id as employeeid,referencelist.name sectionname,sum(billitems.receivable) as receivable,sum(billitems.qty) qty,
            sum(billitems.qty*coalesce(incentiveconfig.incentive::numeric,sectionconfig.incentive::numeric)) amount,employeecode,
            employee.sectionid from billitems 
            inner join bill on bill.id =  billitems.billid
            inner join salablegoods on salablegoods.id = salablegoodsid and salablegoods.isactive = true
            inner join stock on stock.id = salablegoods.stockid and stock.isactive = true
            inner join employee on employee.id = salesmanid
            inner join products on products.id = stock.productid
            left outer join referencelist on referencelist.id = employee.sectionid
            inner join sectionconfig on sectionconfig.id = stock.productid and billitems.price::numeric between sectionconfig.from and sectionconfig.to
            left outer join incentiveconfig on incentiveconfig.salablegoodsid = salablegoods.id
            where 
            bill.billdate>=@fromdate and bill.billdate<@todate and (employee.hasleft = false or employee.relievingdate>@fromdate)
            AND (bill.locationid = @locationid and employee.locationid=@locationid) 
            group by employee.name,referencelist.name,employee.id,employee.sectionid";

            string SQL_incentives_product_employees_detail = @"with sectionconfig 
            as 
            (
	            select referencelist.referenceid as id,code,cast(schemelist ->> 'to' as numeric) as to,cast(schemelist ->> 'from' as numeric) as from
	            ,cast(schemelist ->> 'incentive' as numeric) as incentive from referencelist
	            LEFT JOIN LATERAL
	            json_array_elements (cast(additionalinfo::json->>'salesincentive' as json)) schemelist ON true
	            where referencelocationid = @locationid and type = 'INCENTIVE-PRODUCT' and cast(schemelist ->> 'to' as numeric) is not null 
            )

            select employee.name,employee.id as employeeid,products.name sectionname,billitems.receivable,billitems.qty,bill.billno,bill.billdate,
			billitems.price,sectionconfig.from,sectionconfig.to,coalesce(incentiveconfig.incentive::numeric,sectionconfig.incentive::numeric) as incentive, 
            employee.employeecode,salescounterid as sectionid,billitems.qty*sectionconfig.incentive amount from billitems 
            inner join bill on bill.id =  billitems.billid
            inner join salablegoods on salablegoods.id = salablegoodsid and salablegoods.isactive = true
            inner join stock on stock.id = salablegoods.stockid and stock.isactive = true
            inner join employee on employee.id = salesmanid
            inner join products on products.id = stock.productid
            inner join sectionconfig on sectionconfig.id = stock.productid and billitems.price::numeric between sectionconfig.from and sectionconfig.to
            left outer join incentiveconfig on incentiveconfig.salablegoodsid = salablegoods.id
            where 
            bill.billdate>=@fromdate and bill.billdate<@todate  AND ( bill.locationid = @locationid and employee.locationid=@locationid) and
			employee.id=@employeeid";

            string SQL_incentives_section_employees_detail = @"with sectionconfig 
            as 
            (
	            select referencelist.id,code,cast(schemelist ->> 'to' as numeric) as to,cast(schemelist ->> 'from' as numeric) as from
	            ,cast(schemelist ->> 'incentive' as numeric) as incentive from referencelist
	            LEFT JOIN LATERAL
	            json_array_elements (cast(additionalinfo::json->>'salesincentive' as json)) schemelist ON true
	            where type = 'DIVISION' and cast(schemelist ->> 'to' as numeric) is not null 
            )

            select employee.name,employee.id as employeeid,referencelist.name sectionname,billitems.receivable,billitems.qty,bill.billno,bill.billdate,
			billitems.price,sectionconfig.from,sectionconfig.to,coalesce(incentiveconfig.incentive::numeric,sectionconfig.incentive::numeric) as incentive, 
            employee.employeecode,salescounterid as sectionid,billitems.qty*sectionconfig.incentive amount from billitems 
            inner join bill on bill.id =  billitems.billid
            inner join salablegoods on salablegoods.id = salablegoodsid and salablegoods.isactive = true
            inner join stock on stock.id = salablegoods.stockid and stock.isactive = true
            inner join employee on employee.id = salesmanid
            inner join products on products.id = stock.productid
            inner join referencelist on referencelist.id = salescounterid
            inner join sectionconfig on sectionconfig.id = salescounterid and billitems.price::numeric between sectionconfig.from and sectionconfig.to
            left outer join incentiveconfig on incentiveconfig.salablegoodsid = salablegoods.id
            where 
            bill.billdate>=@fromdate and bill.billdate<@todate  AND ( bill.locationid = @locationid and employee.locationid=@locationid) and
			employee.id=@employeeid";


            List<InfyPOS.Payroll.IncentiveSalary.IncentiveData.Item> inchargelist = null;
            DateTime today = item.fromdate.Date;
            DateTime nextdate = item.todate.Date.AddDays(1);
            NpgsqlConnection conn = CreateConnection();
            try
            {
                
                string query = "";
                if(item.mode == "section")
                {
                    if(appconfig != null && appconfig.incentivemode == 3)
                    {
                        query = SQL_incentivies_Products;
                    }
                    else
                    {
                        query = SQL_incentivies_Section;
                    }
                }
                else if(item.showdetail)
                {
                    if (appconfig != null && appconfig.incentivemode == 3)
                    {
                        query = SQL_incentives_product_employees_detail;
                    }
                    else
                    {
                        query = SQL_incentives_section_employees_detail;
                    }
                }
                else
                {
                    if (appconfig != null && appconfig.incentivemode == 3)
                    {
                        query = SQL_incentivies_Product_Employees;
                    }
                    else
                    {
                        query = SQL_incentivies_Section_Employees;
                    }
                }
               
                if (item.mode != "section")
                {
                    inchargelist = CalculateIncentive4Incharges(item);
                    if (item.employeeid > 0)
                    {
                        inchargelist.RemoveAll(e => e.inchargeid != item.employeeid && e.employeeid != item.employeeid);
                        if (inchargelist.Exists(e => e.inchargeid == item.employeeid))
                        {
                            if (!item.showdetail)
                            {
                                List<long> employeelist = inchargelist.ConvertAll<long>(x => x.employeeid);
                                query = query.Replace("AND (bill.locationid = @locationid)", "AND (bill.locationid = @locationid) and employee.id in (" + string.Join<long>(",", employeelist) + ")");
                            }
                        }
                        else
                        {
                            if (!item.showdetail)
                            {
                                List<long> employeelist = inchargelist.ConvertAll<long>(x => x.employeeid);
                                query = query.Replace("AND (bill.locationid = @locationid)", "AND (bill.locationid = @locationid) and employee.id=@employeeid");
                            }
                        }
                    }
                }

                conn.Open();

                NpgsqlCommand command = null;
                command = GetCommand(query,conn);
                command.Parameters.Add("fromdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = today;
                command.Parameters.Add("todate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = nextdate;
                command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.locationid;
                command.Parameters.Add("employeeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.employeeid;
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        IncentiveValue oitem = new IncentiveValue();
                        oitem.sectionname = reader["sectionname"] == DBNull.Value ? "" : reader["sectionname"].ToString();
                        oitem.receivable = reader["receivable"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["receivable"]);
                        if (item.mode != "section")
                        {
                            oitem.employeename = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
                            oitem.employeeid = reader["employeeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["employeeid"]);
                            oitem.employeecode = reader["employeecode"] == DBNull.Value ? "" : reader["employeecode"].ToString();
                            oitem.sectionid = reader["sectionid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sectionid"]);
                            oitem.locationid = item.locationid;
                            if (item.showdetail)
                            {
                                oitem.price = reader["price"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["price"]);
                                oitem.from = reader["from"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["from"]);
                                oitem.to = reader["to"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["to"]);
                                oitem.incentive = reader["incentive"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["incentive"]);
                                oitem.billno = reader["billno"] == DBNull.Value ? "" : reader["billno"].ToString();
                                oitem.billdate = reader["billdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["billdate"]);
                            }
                        }
                        oitem.qty = reader["qty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["qty"]);
                        oitem.amount = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"]);
                        saleValues.Add(oitem);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, item);
            }
            
            //saleValues.RemoveAll(e => !inchargelist.Exists(y => y.employeeid == e.employeeid)); 
            //if (item.sectionid > 0) saleValues.RemoveAll(e => e.sectionid != item.sectionid);

            if (inchargelist != null)
            {
                if (inchargelist.Count > 0)
                {
                    var departmentid = inchargelist.First().departmentid;
                    saleValues.ForEach(e => e.departmentid = departmentid);
                }

                foreach (var incharge in inchargelist)
                {
                    if (incharge.inchargeid > 0)
                    {
                        if (saleValues.Exists(e => e.employeeid == incharge.employeeid))
                        {
                            var list = saleValues.FindAll(e => e.employeeid == incharge.employeeid).ToList();
                            list.ForEach(e=>e.inchargeid = incharge.inchargeid);
                            list.ForEach(e => e.days = incharge.days);
                            list.ForEach(e => e.payratio = incharge.payratio);
                        }
                    }
                }

                foreach (var incharges in saleValues.GroupBy(e => e.inchargeid))
                {
                    var totalIncentive = incharges.Sum(e => e.amount);

                    if (inchargelist.Exists(e => e.employeeid == incharges.Key))
                    {
                        decimal skipmonths = appconfig != null ? appconfig.incentiveskipmonths : 3;
                        var incharge = inchargelist.Find(e => e.employeeid == incharges.Key);
                        var teammembers = incharges.Count();
                        foreach (var teammember in incharges)
                        {
                            teammember.ratio = "";
                            if (inchargelist.Exists(e => e.employeeid == teammember.employeeid))
                            {
                                var empitem = inchargelist.Find(e => e.employeeid == teammember.employeeid);
                                if ((empitem.dateofjoining > item.fromdate.Date.AddMonths((int)(skipmonths*-1))) ||
                                   (empitem.relievingdate > item.fromdate.Date && empitem.relievingdate < item.todate.Date))
                                    teammembers--;
                            }
                        }

                        incharge.teammembers = teammembers;
                        
                        var avg = teammembers>0 ? totalIncentive / teammembers * incharge.payratio : 0;

                        if (saleValues.Exists(e => e.employeeid == incharge.employeeid))
                            saleValues.RemoveAll(e => e.employeeid == incharge.employeeid);

                        saleValues.Add(new IncentiveValue()
                        {
                            employeename = incharge.employeename,
                            employeeid = incharge.employeeid,
                            amount = avg,
                            sectionid = incharge.sectionid,
                            departmentid = incharge.departmentid,
                            employeecode = incharge.employeecode,
                            sectionname = incharges.First().sectionname,
                            days = incharge.days,
                            ratio = incharge.payratio.ToString("N2") + " (" + incharge.ratio.ToString("N2") + " times)" + "(" + incharge.days.ToString("N2") + " days)"
                        });
                    }
                }
            }
            if (item.fromdate.AddMonths(1).AddDays(-1).Date == item.todate.Date && item.fromdate.Day == 1)
            {
                IncentivesDataService incentivesDataService = new IncentivesDataService() { Context = this.Context };
                incentivesDataService.Update(item, saleValues);
            }
            return (item.sectionid > 0 ? saleValues.FindAll(e => e.sectionid == item.sectionid) : saleValues).OrderBy(e=>e.sectionname+e.inchargeid.ToString()).ToList();
        }


        public List<InfyPOS.Payroll.IncentiveSalary.IncentiveData.Item> CalculateIncentive4Incharges(RetailReportCreteria item)
        {
            string baseQuery = @"with salaryheader as (
			SELECT id, employeeid, fromdate, inchargeid, row_number() OVER (PARTITION BY employeeid ORDER BY fromdate DESC,id DESC) as ranknumber  
			FROM employeesalaryheader where fromdate<=@fromdate)
            select employee.name as employeename,employeecode,workingdays,leavedays,employeesalaryheader.incentiveratio,
            employee.departmentid,employee.companyid,employee.sectionid,employee.id as employeeid,grosspay,dateofjoining,
            employee.locationid,salaryheader.inchargeid,salestarget,relievingdate
            from employee
            inner join salaryheader on salaryheader.ranknumber = 1 and salaryheader.employeeid = employee.id  
			inner join referencelist on referencelist.id = employee.departmentid
            left outer join employeesalary on employeesalary.employeeid = employee.id and salarymonth=@salarymonth
            left outer join employeesalaryheader on employeesalaryheader.id = salaryheader.id
            where cast(referencelist.additionalinfo->>'issales' as boolean) = true  and (employee.hasleft = false or employee.relievingdate>@fromdate)";

            List<InfyPOS.Payroll.IncentiveSalary.IncentiveData.Item> items = new List<Payroll.IncentiveSalary.IncentiveData.Item>();
            DateTime fromDate = new DateTime(item.fromdate.Date.Year, item.fromdate.Date.Month, 1);
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                NpgsqlCommand command = null;
                command = GetCommand(baseQuery, conn);
                command.Parameters.Add("fromdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = fromDate;
                command.Parameters.Add("salarymonth", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = fromDate;
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string employeename = reader["employeename"] == DBNull.Value ? "" : reader["employeename"].ToString();
                        string employeecode = reader["employeecode"] == DBNull.Value ? "" : reader["employeecode"].ToString();
                        decimal workingdays = reader["workingdays"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["workingdays"]);
                        decimal leavedays = reader["leavedays"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["leavedays"]);
                        decimal incentiveratio = reader["incentiveratio"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["incentiveratio"]);
                        decimal grosspay = reader["grosspay"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["grosspay"]);
                        long departmentid = reader["departmentid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["departmentid"]);
                        long sectionid = reader["sectionid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sectionid"]);
                        long employeeid = reader["employeeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["employeeid"]);
                        long companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"]);
                        long locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                        long inchargeid = reader["inchargeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["inchargeid"]);
                        decimal salestarget = reader["salestarget"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["salestarget"]);

                        if (inchargeid == employeeid)
                            inchargeid = 0;
                        DateTime dateofjoining = reader["dateofjoining"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["dateofjoining"]);
                        DateTime relievingdate = reader["relievingdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["relievingdate"]);
                        if (incentiveratio <= 0) incentiveratio = 1;
                        items.Add(new InfyPOS.Payroll.IncentiveSalary.IncentiveData.Item()
                        {
                            employeename = employeename,
                            employeeid = employeeid,
                            days = workingdays - leavedays,
                            payratio = workingdays ==0 ? incentiveratio : (incentiveratio * (100 - (leavedays / workingdays) * 100) / 100),
                            ratio = incentiveratio,
                            sectionid = sectionid,
                            departmentid = departmentid,
                            companyid = companyid,
                            employeecode = employeecode,
                            grosspay = grosspay,
                            inchargeid = inchargeid,
                            dateofjoining = dateofjoining,
                            salestarget = salestarget,
                            relievingdate = relievingdate
                        });
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, item);
            }

            return items;
        }


    }
}