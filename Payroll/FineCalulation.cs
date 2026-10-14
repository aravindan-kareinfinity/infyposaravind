using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;

namespace InfyPOS.Payroll
{
    public class FineCalulation : Common.BaseData
    {
        public class FineCreteria: Common.BaseEntity
        {
            public long companyid { get; set; }
            public long locationid { get; set; }
            public DateTime fromdate { get; set; }
            public DateTime todate { get; set; }
            public Models.Referencelist.Attribute attributes { get; set; }
        }
        FineCreteria creteria;
        long organizationid;
        public FineCalulation()
        {
            
        }

        public TimeSpan ParseTime(string time)
        {
            return DateTime.ParseExact(time, "hh:mm tt", CultureInfo.InvariantCulture).TimeOfDay;
        }
        public List<FineCalulationData> Compute(long organizationid, FineCreteria creteria)
        {
            this.organizationid = organizationid;
            this.creteria = creteria;
            return this.Compute();
        }
        public List<FineCalulationData> Compute()
        {
            Data.ReferencelistDataService referencelistDataService = new Data.ReferencelistDataService() { Context = this.Context };
            var config = referencelistDataService.Search(new Models.ReferencelistCreteria()
            {
                type = "SPECIALPAYMENT"
            }).FindAll(e => e.additionalinfo != null &&
            e.additionalinfo.splpaytypelistid == "Fine");
            if (config.Count == 0) return new List<FineCalulationData>();
            creteria.attributes = config.First().additionalinfo;

            string query = @"with salarysummary as (Select employeeid,
sum(latecheckin) as latecheckin,
sum(earlycheckout) as earlycheckout,
sum(actualworking) as actualworking,
sum(case when nonworking>@nonworking then nonworking-@nonworking else 0 end) as nonworking,
sum(case when earlycheckout+latecheckin>@daily then earlycheckout+latecheckin-@daily else 0 end) as earlyandlate,
sum(case when nonworking>@nonworking then 1 else 0 end) as nonworkingcount,
sum(case when earlycheckout+latecheckin>@daily then 1 else 0 end) as earlyandlatecount,
sum(case when actualworking>0 then 1 else 0 end) as workingdays
from employeeinout
where attendanceon>=@fromdate and attendanceon<@todate
group by employeeid
),
salaryheader as (
SELECT id, employeeid, fromdate, row_number() OVER (PARTITION BY employeeid ORDER BY fromdate DESC,id DESC) as ranknumber  
FROM employeesalaryheader where fromdate<=@todate)

select employee.name,dept.name as department,design.name as designation,employeesalaryheader.grosspay,
employee.id employeeid,employeecode,latecheckin,earlycheckout,actualworking,nonworking,earlyandlate,
nonworkingcount,earlyandlatecount,employee.gender,workingdays from employee
left outer join salarysummary on salarysummary.employeeid= employee.id
left outer join salaryheader on salaryheader.ranknumber = 1 and salaryheader.employeeid = employee.id
left outer join employeesalaryheader on employeesalaryheader.id = salaryheader.id
left outer join referencelist dept on dept.id = employee.departmentid
left outer join referencelist design on design.id = employee.desingnationid
where employee.isactive = true and (employee.hasleft = false or employee.relievingdate>@todate)";
            SortedList<string, object> param = new SortedList<string, object>();
          
            param.Add("fromdate", creteria.fromdate);
            param.Add("todate", creteria.todate);
            if (creteria.companyid > 0)
            {
                param.Add("companyid", creteria.companyid);
                query = query + " and employee.companyid = @companyid";
            }
            if (creteria.locationid > 0)
            {
                param.Add("locationid", creteria.locationid);
                query = query + " and employee.locationid = @locationid";
            }

            query += " order by case when employeecode ~'^[0-9]+$' then cast(employeecode as numeric) else cast(0 as numeric) end";

            param.Add("daily", creteria.attributes.reportconfig.Dailygracetime);
            param.Add("nonworking", creteria.attributes.reportconfig.Lunchgracetime);

            InfyPOS.Common.BaseData baseData = new Common.BaseData() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            List<FineCalulationData> retValue = new List<FineCalulationData>();
            using (System.Data.IDataReader reader = baseData.ExecuteReader(query, param))
            {
                while (reader.Read())
                {
                    FineCalulationData entity = new FineCalulationData();
                    entity.Name = reader.GetString(0);
                    entity.Department = reader.IsDBNull(1) ? "" : reader.GetString(1);
                    entity.Designation = reader.IsDBNull(2) ? "" : reader.GetString(2);
                    entity.GrossPay = reader.IsDBNull(3) ? 0 : reader.GetDecimal(3);
                    entity.EmployeeID = reader.GetInt64(4);
                    entity.EmployeeCode = reader.IsDBNull(5) ? "" : reader.GetString(5);
                    entity.latecheckin = reader.IsDBNull(6) ? 0 : reader.GetInt32(6);
                    entity.earlycheckout = reader.IsDBNull(7) ? 0 : reader.GetInt32(7);
                    entity.actualworking = reader.IsDBNull(8) ? 0 : reader.GetInt32(8);
                    entity.nonworking = reader.IsDBNull(9) ?0 : reader.GetInt32(9);
                    entity.earlyandlate = reader.IsDBNull(10) ? 0 : reader.GetInt32(10);
                    entity.nonworkingcount = reader.IsDBNull(11) ? 0 : reader.GetInt32(11);
                    entity.earlyandlatecount = reader.IsDBNull(12) ? 0 : reader.GetInt32(12);
                    entity.gender = reader.IsDBNull(13) ? "M" : reader.GetString(13);
                    entity.workingdays = reader.IsDBNull(14) ? 0 : reader.GetInt32(14);

                    if (creteria.attributes.reportconfig.Monthlygracetime < entity.earlyandlate+entity.nonworking)
                    {
                        if(entity.gender == "M" && creteria.attributes.reportconfig.Fine_Male_Slab>0)
                        {
                            entity.Fine = Math.Floor(((decimal)entity.earlyandlate + entity.nonworking) / creteria.attributes.reportconfig.Fine_Male_Slab) * creteria.attributes.reportconfig.Fine_Male_Fine;
                            if (entity.Fine > creteria.attributes.reportconfig.Fine_Male_Max)
                                entity.Fine = creteria.attributes.reportconfig.Fine_Male_Max;
                            retValue.Add(entity);
                        }
                        if (entity.gender == "F" && creteria.attributes.reportconfig.Fine_Female_Slab > 0)
                        {
                            entity.Fine = Math.Floor(((decimal)entity.earlyandlate + entity.nonworking) / creteria.attributes.reportconfig.Fine_Female_Slab) * creteria.attributes.reportconfig.Fine_Female_Fine;
                            if (entity.Fine > creteria.attributes.reportconfig.Fine_Female_Max)
                                entity.Fine = creteria.attributes.reportconfig.Fine_Female_Max;
                            retValue.Add(entity);
                        }
                    }
                }
            }

            if ((creteria.fromdate - creteria.todate).TotalDays > 15)
            {
                var salarymonth = new DateTime(creteria.todate.Year, creteria.todate.Month, 1);
                Data.EmployeesalaryscheduleDataService employeesalaryscheduleDataService = new Data.EmployeesalaryscheduleDataService() { Context = this.Context };
                employeesalaryscheduleDataService.SaveFine(new Models.EmployeeAdvance()
                {
                    salarymonth = salarymonth,
                    searchmode = 3,
                    companyid = creteria.companyid,
                    locationid = creteria.locationid,
                    schedules = retValue.ConvertAll(e =>
                    {
                        return new Models.EmployeeAdvance.ScheduleItem()
                        {
                            employeeid = e.EmployeeID,
                            amount = e.Fine,
                            advancedate = salarymonth
                        };
                    })
                });
            }
            return retValue;
        }

        internal List<FineCalulationData> Compute(FineCreteria item)
        {
            this.creteria = item;
            return Compute();
        }

        public class FineCalulationData
        {
            public long EmployeeID { get; set; }
            public string Name { get; set; }
            public string Department { get; set; }
            public string Designation { get; set; }
            public decimal GrossPay { get; set; }
            public decimal Fine { get; set; }
            public string EmployeeCode { get; set; }
            public int latecheckin { get; set; }
            public int earlycheckout { get; set; }
            public int actualworking { get; set; }
            public int nonworking { get; set; }
            public int workingdays { get; set; }
            public int earlyandlate { get; set; }
            public int nonworkingcount { get; set; }
            public int earlyandlatecount { get; set; }
            public string gender { get; set; }
        }
    }
}