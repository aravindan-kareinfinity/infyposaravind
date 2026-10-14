using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace InfyPOS.Payroll
{
    public class Bonus
    {
        InfyPOS.Models.IncentivesCreteria creteria;
        long organizationid;
        public Bonus(InfyPOS.Models.IncentivesCreteria creteria,long organizationid)
        {
            this.organizationid = organizationid;
            this.creteria = creteria;
        }

        public class BounsData
        {
            public long EmployeeID { get; set; }
            public string Name { get; set; }
            public string Department { get; set; }
            public string Designation { get; set; }
            public decimal Workingdays { get; set; }
            public decimal LeaveDays { get; set; }
            public decimal Earnings { get; set; }
            public decimal BasicPay { get; set; }
            public decimal GrossPay { get; set; }
            public decimal Bonus { get; set; }
        }
        public List<BounsData> Compute()
        {
            string query = @"with salaryheader as (
			SELECT id, employeeid, fromdate, row_number() OVER (PARTITION BY employeeid ORDER BY fromdate DESC,id DESC) as ranknumber  
			FROM employeesalaryheader where fromdate<=@todate),
salarysummary as 
(select employeesalary.employeeid, sum(workingdays) workingdays,sum(leavedays) as leavedays,sum(earnings) as earnings
from employeesalary
where salarymonth>=@fromdate and salarymonth<@todate
group by employeesalary.employeeid )

select salarysummary.*,
employee.name,dept.name as department,design.name as designation,employeesalaryheader.basicpay,employeesalaryheader.grosspay from salarysummary

inner join employee on employee.id = salarysummary.employeeid
inner join referencelist dept on dept.id = employee.departmentid
inner join referencelist design on design.id = employee.desingnationid
left outer join salaryheader on salaryheader.ranknumber = 1 and salaryheader.employeeid = employee.id
inner join employeesalaryheader on employeesalaryheader.id = salaryheader.id
where employee.isactive=true and employee.DateOfJoining<@todate
order by case when employeecode ~ '^[0-9]+$' then cast(employeecode as numeric) else cast(0 as numeric) end";

            // and (dateofreleaving <DateOfJoining or dateofreleaving is null) 

            SortedList<string, object> param = new SortedList<string, object>();
            DateTime fromDate = new DateTime(creteria.fromyear, creteria.frommonth, 1);
            DateTime todate = new DateTime(creteria.toyear, creteria.tomonth, 1).AddMonths(1);
            param.Add("fromdate", fromDate);
            param.Add("todate", todate);
            if (creteria.companyid > 0)
            {
                param.Add("companyid", creteria.companyid);
                query = query + " and employee.companyid = @companyid";
            }
            if (creteria.companyid > 0)
            {
                param.Add("departmentid", creteria.departmentid);
                query = query + " and employee.departmentid = @departmentid";
            }

            decimal bonuspercentage = creteria.attributes.reportconfig.Bonus_Percentage;
            
            InfyPOS.Common.BaseData baseData = new Common.BaseData() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            List<BounsData> retValue = new List<BounsData>();
            using (System.Data.IDataReader reader = baseData.ExecuteReader(query, param))
            {
                while (reader.Read())
                {
                    BounsData entity = new BounsData();
                    entity.Name = reader.GetString(4);
                    entity.Department = reader.IsDBNull(5) ? "" : reader.GetString(5);
                    entity.Designation = reader.IsDBNull(6) ? "" : reader.GetString(6);
                    entity.EmployeeID = reader.GetInt64(2);
                    entity.BasicPay = reader.GetDecimal(7);
                    entity.GrossPay = reader.GetDecimal(8);
                    entity.LeaveDays = reader.IsDBNull(2) ? 0 : reader.GetDecimal(2);
                    entity.Earnings = reader.IsDBNull(3) ? 0 : reader.GetDecimal(3);
                    entity.Workingdays = reader.IsDBNull(1) ? 0 : reader.GetDecimal(1);
                    entity.Bonus = Math.Round(bonuspercentage * entity.Earnings / 100,0);
                    retValue.Add(entity);
                }
            }

            return retValue;
        }
    }
}