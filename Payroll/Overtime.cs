using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;

namespace InfyPOS.Payroll
{
    public class Overtime
    {
        InfyPOS.Models.IncentivesCreteria creteria;
        public Overtime()
        {
        }

        public TimeSpan ParseTime(string time)
        {
            return DateTime.ParseExact(time, "hh:mm tt", CultureInfo.InvariantCulture).TimeOfDay;
        }

        long organizationid;
        public List<OvertimeData> Compute(long organizationid, InfyPOS.Models.IncentivesCreteria creteria)
        {
            this.creteria = creteria;
            this.organizationid = organizationid;
            return Compute();
        }
        
       
		
		
		 public List<OvertimeData> Compute()
        {
           String query = @"with salarysummary as (Select employeeid,  sum(case when intime<@Working_Hours_From then @Working_Hours_From-intime else 0 end) as morningot, sum(case when outtime>@Working_Hours_To then outtime-@Working_Hours_To else 0 end) as nightot from employeeinout 
where attendanceon>=@fromDate and attendanceon<@todate  group by employeeid) select employee.name,dept.name as department,design.name as designation,salarysummary.*, employee.employeecode,employee.hasot,employee.hasmorningot from salarysummary 
inner join employee on employee.id = salarysummary.employeeid 
inner join referencelist dept on dept.id = employee.departmentid
inner join referencelist design on design.id = employee.desingnationid
where employee.isactive = true ";

      
            SortedList<string, object> param = new SortedList<string, object>();
           
            DateTime fromDate = creteria.fromdate;
            DateTime todate = creteria.todate.Date;
            param.Add("fromdate", fromDate);
            param.Add("todate", todate);


            if (creteria.employeeid > 0)
            {
                param.Add("employeeid", creteria.employeeid);
                query = query + " and employee.id = @employeeid";
            }
            
            if (creteria.companyid > 0)
            {
                param.Add("companyid", creteria.companyid);
                query = query + " and employee.companyid = @companyid";
            }
            if (creteria.departmentid > 0)
            {
                param.Add("departmentid", creteria.departmentid);
                query = query + " and employee.departmentid = @departmentid";
            }

            if(creteria.employeeid == 0)
            {
                query += @"Order By case when employeecode ~ '^[0-9]+$' then cast(employeecode as numeric) else cast(0 as numeric) end";
                
            }


            TimeSpan fsts = ParseTime(creteria.attributes.reportconfig.Working_Hours_From);
            param.Add("Working_Hours_From", fsts.TotalMinutes);

            TimeSpan tsts = ParseTime(creteria.attributes.reportconfig.Working_Hours_To);
            param.Add("Working_Hours_To", tsts.TotalMinutes);

            InfyPOS.Common.BaseData baseData = new Common.BaseData() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            List<OvertimeData> retValue = new List<OvertimeData>();
            using (System.Data.IDataReader reader = baseData.ExecuteReader(query, param))
            {
                while (reader.Read())
                {
                    OvertimeData entity = new OvertimeData();

                    bool hasmorningot = reader["hasmorningot"] == DBNull.Value ? false : Convert.ToBoolean(reader["hasmorningot"]);
                    bool hasot = reader["hasot"] == DBNull.Value ? false : Convert.ToBoolean(reader["hasot"]);
                    entity.Name = reader.GetString(0);
                    entity.Department = reader.IsDBNull(1) ? "" : reader.GetString(1);
                    entity.Designation = reader.IsDBNull(2) ? "" : reader.GetString(2);
                   
                    entity.EmployeeID = reader.GetInt64(3);
                    if (!hasmorningot)
                    {
                        entity.MorningOT = 0;
                    }
                    else
                    {
                        entity.MorningOT = (int)reader.GetDouble(4);
                    }

                    if (!hasot)
                    {
                        entity.NightOT = 0;
                    }
                    else
                    {
                        entity.NightOT = (int)reader.GetDouble(5);
                    }
                   
                    entity.EmployeeCode = reader.IsDBNull(6) ? "" : reader.GetString(6);
                   
                    entity.Overtime = Math.Round((entity.MorningOT * creteria.attributes.reportconfig.OT_Before_WH) + (entity.NightOT * creteria.attributes.reportconfig.OT_After_WH),0);
                   
                   
                    //if (!hasot) entity.NightOT = 0;
                    //if (!hasmorningot) entity.MorningOT = 0;
                    if(entity.NightOT>=0 || entity.MorningOT>=0)
                        retValue.Add(entity);
                }
            }

            return retValue;
        }
		
		
        public class OvertimeData
        {
            public long EmployeeID { get; set; }
            public string Name { get; set; }
            public string Department { get; set; }
            public string Designation { get; set; }
            public decimal Overtime { get; set; }
            public int MorningOT { get; set; }
            public int NightOT { get; set; }

            public string EmployeeCode { get; set; }
            public decimal GrossPay { get; set; }
        }
		
		 public List<OvertimeData> ComputeOLD()
        {

            string query = @"with salarysummary as (Select employeeid,
sum(case when intime<@Working_Hours_From then @Working_Hours_From-intime else 0 end) as morningot,
sum(case when outtime>@Working_Hours_To then outtime-@Working_Hours_To else 0 end) as nightot
from employeeinout
where attendanceon>=@fromdate and attendanceon<@todate
group by employeeid
),
salaryheader as (
SELECT id, employeeid, fromdate, row_number() OVER (PARTITION BY employeeid ORDER BY fromdate DESC,id DESC) as ranknumber  
FROM employeesalaryheader where fromdate<=@todate)

select employee.name,dept.name as department,design.name as designation,employeesalaryheader.grosspay,salarysummary.*,employeecode,employee.hasot,employee.hasmorningot from salarysummary

inner join employee on employee.id = salarysummary.employeeid
left outer join salaryheader on salaryheader.ranknumber = 1 and salaryheader.employeeid = employee.id
inner join employeesalaryheader on employeesalaryheader.id = salaryheader.id
inner join referencelist dept on dept.id = employee.departmentid
inner join referencelist design on design.id = employee.desingnationid
where employee.isactive = true
order by case when employeecode ~ '^[0-9]+$' then cast(employeecode as numeric) else cast(0 as numeric) end";
            SortedList<string, object> param = new SortedList<string, object>();
            DateTime fromDate = new DateTime(creteria.year, creteria.month, 1);
            DateTime todate = new DateTime(creteria.year, creteria.month, 1).AddMonths(1);
            param.Add("fromdate", fromDate);
            param.Add("todate", todate);
            if (creteria.companyid > 0)
            {
                param.Add("companyid", creteria.companyid);
                query = query + " and employee.companyid = @companyid";
            }
            if (creteria.departmentid > 0)
            {
                param.Add("departmentid", creteria.departmentid);
                query = query + " and employee.departmentid = @departmentid";
            }

            TimeSpan fsts = ParseTime(creteria.attributes.reportconfig.Working_Hours_From);
            param.Add("Working_Hours_From", fsts.TotalMinutes);

            TimeSpan tsts = ParseTime(creteria.attributes.reportconfig.Working_Hours_To);
            param.Add("Working_Hours_To", tsts.TotalMinutes);

            InfyPOS.Common.BaseData baseData = new Common.BaseData() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            List<OvertimeData> retValue = new List<OvertimeData>();
            using (System.Data.IDataReader reader = baseData.ExecuteReader(query, param))
            {
                while (reader.Read())
                {
                    OvertimeData entity = new OvertimeData();
                    entity.Name = reader.GetString(0);
                    entity.Department = reader.IsDBNull(1) ? "" : reader.GetString(1);
                    entity.Designation = reader.IsDBNull(2) ? "" : reader.GetString(2);
                    entity.GrossPay = reader.GetDecimal(3);
                    entity.EmployeeID = reader.GetInt64(4);
                    entity.MorningOT = (int) reader.GetDouble(5);
                    entity.NightOT = (int)reader.GetDouble(6);
                    entity.EmployeeCode = reader.IsDBNull(7) ? "" : reader.GetString(7);
                    decimal perminute = entity.GrossPay / 30 / 12 / 60;
                    entity.Overtime = Math.Round((perminute * entity.MorningOT * creteria.attributes.reportconfig.OT_Before_WH) + (perminute * entity.NightOT * creteria.attributes.reportconfig.OT_After_WH),0);
                    bool hasot = reader["hasot"] == DBNull.Value ? false : Convert.ToBoolean(reader["hasot"]);
                    bool hasmorningot = reader["hasmorningot"] == DBNull.Value ? false : Convert.ToBoolean(reader["hasmorningot"]);
                    if (!hasot) entity.NightOT = 0;
                    if (!hasmorningot) entity.MorningOT = 0;
                    if(entity.NightOT>=0 || entity.MorningOT>=0)
                        retValue.Add(entity);
                }
            }

            return retValue;
        }
    }
}