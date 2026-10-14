using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;

namespace InfyPOS.Payroll
{
    public class Beta
    {
        InfyPOS.Models.IncentivesCreteria creteria;
        long organizationid;
        public Beta(InfyPOS.Models.IncentivesCreteria creteria,long organizationid)
        {
            this.creteria = creteria;
            this.organizationid=  organizationid;
        }

        public TimeSpan ParseTime(string time)
        {
            return DateTime.ParseExact(time, "hh:mm tt", CultureInfo.InvariantCulture).TimeOfDay;
        }

        public List<BetaData> Compute()
        {

string query = @" with salarysummary as (Select employeeid,
sum(case when intime<= @Beta_Time and morningabsent = false and afternoonabsent = false then 1 else 0 end) as fullbeta,
sum(case when intime> @Beta_Time and intime<=@Beta_Mintime and morningabsent = false and afternoonabsent = false then 1 else 0 end) as latebeta,
sum(case when intime<= @Beta_Time and morningabsent = false and afternoonabsent = true then 1 else 0 end) as fullhalfbeta,
sum(case when intime > @Beta_Time and intime<=@Beta_Mintime and morningabsent = false and afternoonabsent = true then 1 else 0 end) as latehalfbeta,
sum(case when morningabsent = true and afternoonabsent = false then 1 else 0 end) as halfbeta,
count(*) as count from employeeinout
where attendanceon >= @fromdate and attendanceon< @todate
group by employeeid
)
select employee.name,dept.name as department,design.name as designation,employee.gender,employee.employeecode,salarysummary.* from salarysummary
inner join employee on employee.id = salarysummary.employeeid
inner
join referencelist dept on dept.id = employee.departmentid
inner
join referencelist design on design.id = employee.desingnationid
where employee.isactive = true and hasbeta=true
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

            TimeSpan ts = ParseTime(creteria.attributes.reportconfig.Beta_Time);
            param.Add("Beta_Time", ts.TotalMinutes);

            if(string.IsNullOrEmpty(creteria.attributes.reportconfig.Beta_MaxTime))
                param.Add("Beta_Mintime", 0);
            else
            {
                ts = ParseTime(creteria.attributes.reportconfig.Beta_MaxTime);
                param.Add("Beta_Mintime", ts.TotalMinutes);
            }
            InfyPOS.Common.BaseData baseData = new Common.BaseData() { Context = new Common.UserClaim() { OrganizationID = organizationid } };

            List<BetaData> retValue = new List<BetaData>();
            using (System.Data.IDataReader reader = baseData.ExecuteReader(query, param))
            {
                while (reader.Read())
                {
                    BetaData entity = new BetaData();
                    entity.Name = reader.GetString(0);
                    entity.Department = reader.IsDBNull(1) ? "" : reader.GetString(1);
                    entity.Designation = reader.IsDBNull(2) ? "" : reader.GetString(2);
                    entity.Gender = reader.GetString(3);
                    entity.Code = reader.GetString(4);
                    entity.EmployeeID = reader.GetInt64(5);
                    entity.FullBata = reader.GetInt32(6);
                    entity.LateBata = reader.GetInt32(7);
                    entity.FullHalfBata = reader.GetInt32(8);
                    entity.LateHalfBata = reader.GetInt32(9);
                    entity.HalfBata = reader.GetInt32(10);
                    entity.Total = reader.GetInt32(11);
                    entity.MinBata = entity.Total - (entity.FullBata+entity.LateBata+entity.FullHalfBata+entity.LateHalfBata+entity.HalfBata);
                    if (entity.Gender == "M")
                    {
                        entity.Beta = (entity.FullBata * creteria.attributes.reportconfig.Beta_Male_Before)
                                        + (entity.LateBata * creteria.attributes.reportconfig.Beta_Male_After)
                                        + (entity.FullHalfBata * (creteria.attributes.reportconfig.Beta_Male_Before/2))
                                        + (entity.LateHalfBata * (creteria.attributes.reportconfig.Beta_Male_After/2))
                                        + (entity.HalfBata * creteria.attributes.reportconfig.Beta_Male_Halfday)
                                        + (entity.MinBata * creteria.attributes.reportconfig.Beta_Male_Minimum);
                    }
                    else
                    {
                        entity.Beta = (entity.FullBata * creteria.attributes.reportconfig.Beta_Female_Before)
                                        + (entity.LateBata * creteria.attributes.reportconfig.Beta_Female_After)
                                        + (entity.FullHalfBata * (creteria.attributes.reportconfig.Beta_Female_Before / 2))
                                        + (entity.LateHalfBata * (creteria.attributes.reportconfig.Beta_Female_After / 2))
                                        + (entity.HalfBata * creteria.attributes.reportconfig.Beta_Female_Halfday)
                                        + (entity.MinBata * creteria.attributes.reportconfig.Beta_Female_Minimum);
                    }
                    retValue.Add(entity);
                }
            }
            return retValue;
        }
        public class BetaData
        {
            public long EmployeeID { get; set; }
            public string Name { get; set; }
            public string Code { get; set; }
            public string Department { get; set; }
            public string Designation { get; set; }
            public decimal Beta { get; set; }
            public int FullBata { get; set; }
            public int LateBata { get; set; }
            public int LateHalfBata { get; set; }
            public int FullHalfBata { get; set; }
            public int HalfBata { get; set; }
            public string Gender { get; set; }
            public int MinBata { get; set; }
            public int Total { get;  set; }
        }
    }
}