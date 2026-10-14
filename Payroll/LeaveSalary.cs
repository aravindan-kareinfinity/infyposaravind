using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using static InfyPOS.Data.SalaryManager;

namespace InfyPOS.Payroll
{
    public class LeaveSalary
    {
       
        int month;
        int year;
        int days;
        int workingdays = 0;
        long organizationid;
        public LeaveSalary(int month, int year,long organizationid)
        {
            this.month = month;
            this.year = year;
            this.organizationid = organizationid;
            //days = new DateTime(year, month, 1).AddMonths(1).AddDays(-1).Day;
            workingdays = ActualWorkingDays();
        }


        int totalHoliday = 0;
        int totalSunday = 0;

        public int TotalHoliday
        {
            get
            {
                return totalHoliday;
            }

            set
            {
                totalHoliday = value;
            }
        }

        public int TotalSunday
        {
            get
            {
                return totalSunday;
            }

            set
            {
                totalSunday = value;
            }
        }
        List<DateTime> holidayList = new List<DateTime>();
        private int ActualWorkingDays()
        {
            InfyPOS.Common.BaseData baseData = new InfyPOS.Common.BaseData() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            string query = @"select holidayon from holiday where date_part('month', holidayon)= @month AND date_part('year', holidayon) = @year and isactive=true";
            SortedList<string, object> param = new SortedList<string, object>();
            var fromDate = new DateTime(year, month, 1);
            param.Add("month", month);
            param.Add("year", year);
            totalHoliday = 0;
            using (System.Data.IDataReader reader = baseData.ExecuteReader(query, param))
            {
                while (reader.Read())
                {
                    if (reader.GetDateTime(0).DayOfWeek == DayOfWeek.Sunday)
                        continue;
                    totalHoliday++;
                    holidayList.Add(reader.GetDateTime(0));
                }
            }
            DateTime monthstart = new DateTime(year, month, 1);
            DateTime monthend = monthstart.AddMonths(1);
            TotalSunday = SundayTotal();
            return (int)monthend.Subtract(monthstart).TotalDays - (TotalHoliday+ TotalSunday);
        }

        private int SundayTotal()
        {
            int sundays = 0;
            DateTime monthstart = new DateTime(year, month, 1);
            DateTime monthend = monthstart.AddMonths(1);
            while (monthstart < monthend)
            {
                if (monthstart.DayOfWeek == DayOfWeek.Sunday)
                    sundays++;
                monthstart = monthstart.AddDays(1);
            }
            return sundays;
        }

        private int SundayTotal(DateTime todate, bool isfrom)
        {
            int sundays = 0;
            DateTime monthstart = new DateTime(year, month, 1);
            DateTime monthend = monthstart.AddMonths(1);
            if (isfrom)
                monthstart = todate;
            else
                monthend = todate;
            while (monthstart < monthend)
            {
                if (monthstart.DayOfWeek == DayOfWeek.Sunday)
                    sundays++;
                monthstart = monthstart.AddDays(1);
            }
            return sundays;
        }


        private int HolidayTotay(DateTime dateOfJoin)
        {
            return holidayList.Count(e => e >= dateOfJoin);
        }

        public List<Models.Incentives.LeaveData> Process(long employeeid)
        {
            var list = BuildMasters(employeeid);
            List<Models.Incentives.LeaveData> lst = new List<Models.Incentives.LeaveData>();
            foreach (var item in list)
            {
                if (item.grosspay == 0) continue;
                Models.Incentives.LeaveData ss = new Models.Incentives.LeaveData();
                ss.employeeid = item.EmployeeID;
                decimal sundaysalarydays = TotalSunday - item.SundayLeaves;
                ss.sundaysalarydays = sundaysalarydays;
                ss.holidaysalarydays = (TotalHoliday - (item.HolidayAbsent + item.HolidayLeaves));

                if (item.DateOfJoin.Month == month && item.DateOfJoin.Year == year)
                {
                    ss.sundaysalarydays = SundayTotal(item.DateOfBirth, true)- item.SundayLeaves;
                    ss.holidaysalarydays = HolidayTotay(item.DateOfBirth)-(item.HolidayAbsent + item.HolidayLeaves);
                }
                else if (item.Hasleft && item.Relievingdate.Year == year && item.Relievingdate.Month == month)
                {
                    ss.sundaysalarydays = SundayTotal(item.DateOfBirth, false) - item.SundayLeaves;
                    ss.holidaysalarydays = holidayList.Count(e => e < item.Relievingdate) - (item.HolidayAbsent + item.HolidayLeaves);
                }
                ss.employeename = item.Name;
                ss.designation = item.Designation;
                ss.employeecode = item.EmployeeCode;
                ss.grosspay = item.grosspay;
                ss.workingdays = workingdays;
                ss.sundaysalary = Math.Round(sundaysalarydays * (item.grosspay / workingdays), 0, MidpointRounding.AwayFromZero);
                ss.holidaysalary = Math.Round((TotalHoliday - (item.HolidayAbsent + item.HolidayLeaves)) * (item.grosspay / (workingdays)), 0, MidpointRounding.AwayFromZero);
                lst.Add(ss);
            }
            return lst;
        }

        private List<EmployeeSalaryConfig> BuildMasters(long employeeID)
        {
            string query = @"

                        update employeeleave set isactive=false 
                        from employee 
                        where employee.id = employeeleave.employeeid and employeeleave.fromdate<employee.dateofjoining;


                        update employeeinout set isactive=false 
                        from employee 
                        where employee.id = employeeinout.employeeid and employeeinout.attendanceon<employee.dateofjoining;



                        update employeeleave set isactive=false 
                        from employee 
                        where employee.hasleft and employee.relievingdate>'2000-01-01' and employee.id = employeeleave.employeeid and employeeleave.fromdate>employee.relievingdate;


                        update employeeinout set isactive=false 
                        from employee 
                        where employee.hasleft and employee.relievingdate>'2000-01-01' and employee.id = employeeinout.employeeid and employeeinout.attendanceon>employee.relievingdate;



            with salaryheader as (
			SELECT id, employeeid, fromdate, row_number() OVER (PARTITION BY employeeid ORDER BY fromdate DESC,id DESC) as ranknumber  
			FROM employeesalaryheader where fromdate<=@todate)
			
			    select employee.name,referencelist.name as Designation, employee.id as EmployeeID,dateofjoining,
			    employeesalaryheader.basicpay,employeesalaryheader.grosspay,employeesalarystructure.fixedbasic,
                employeesalarystructure.fixedda,employeesalarystructure.deducttextsalaryid, 
                0.00 as CurrentOverTimePerHour, employeesalaryheader.configuration as CurrentSalaryBreakup,
                Leave.LeaveDays,SundayLeaves, holidayLeaves.LeaveDays HolidayLeaves,
                holidaypayable.LeaveDays HolidayAbsent, employeesalarystructure.splallowanceid,employee.employeecode
                from employee 
                left outer join (
                            select employeeid,SUM(days) as LeaveDays,SUM(sundaydays) as SundayLeaves from employeeleave 
                            where EmployeeLeave.isactive and deductsalary=true and 
                            ((fromdate>=@fromdate and todate<@todate))
                                group by EmployeeID
                            ) Leave
                            on Leave.employeeid = employee.id

                left outer join (select employeeid,sum(days) as LeaveDays from holiday
	            inner join EmployeeLeave on EmployeeLeave.fromdate = holiday.holidayon and EmployeeLeave.isactive=true
	            where fromdate>=@fromdate and todate<@todate and holiday.isactive = true and employeeleave.isactive = true
	            group by employeeid) holidayLeaves on holidayLeaves.employeeid = employee.id

                left outer join (select employeeid,sum(days) as LeaveDays from holiday
	            inner join EmployeeLeave on EmployeeLeave.fromdate = holiday.holidayon and EmployeeLeave.isactive=true
	            where payable=true and fromdate>=@fromdate and todate<@todate and holiday.isactive = true and employeeleave.isactive = true
	            group by employeeid) holidaypayable on holidaypayable.employeeid = employee.id

                left outer join salaryheader on salaryheader.ranknumber = 1 and salaryheader.employeeid = employee.id
			    left outer join employeesalaryheader on employeesalaryheader.id = salaryheader.id
                left outer join employeesalarystructure on employeesalarystructure.id = employeesalaryheader.salarystructureid
                left outer join referencelist on referencelist.id = employee.desingnationid
                where employee.isactive and employee.SalaryMode=0 and employee.DateOfJoining<@todate {0}
                order by case when employeecode ~ '^[0-9]+$' then cast(employeecode as numeric) else cast(0 as numeric) end";

            // and (dateofreleaving <DateOfJoining or dateofreleaving is null) 

            SortedList<string, object> param = new SortedList<string, object>();
            DateTime fromDate = new DateTime(year, month, 1);
            DateTime todate = fromDate.AddMonths(1);
            List<EmployeeSalaryConfig> retValue = new List<EmployeeSalaryConfig>();
            param.Add("fromdate", fromDate);
            param.Add("todate", todate);
            if (employeeID > 0)
            {
                param.Add("employeeID", employeeID);
                query = string.Format(query, " and employee.id = @employeeID ");
            }
            else
            {
                query = string.Format(query, " ");
            }

            return BuildMasters(query, param);
        }



        private List<EmployeeSalaryConfig> BuildMasters(string query, SortedList<string, object> param)
        {
            List<EmployeeSalaryConfig> retValue = new List<EmployeeSalaryConfig>();
            InfyPOS.Common.BaseData baseData = new Common.BaseData() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            using (System.Data.IDataReader reader = baseData.ExecuteReader(query, param,10000))
            {
                while (reader.Read())
                {
                    EmployeeSalaryConfig entity = new EmployeeSalaryConfig();
                    entity.Name = reader.GetString(0);
                    entity.Designation = reader.IsDBNull(1) ? "" : reader.GetString(1);
                    entity.EmployeeID = reader.GetInt64(2);
                    entity.DateOfJoin = reader.IsDBNull(3) ? DateTime.Now.Date : reader.GetDateTime(3);
                    entity.BasicPay = reader.IsDBNull(4) ? 0 : reader.GetDecimal(4);
                    entity.grosspay = reader.IsDBNull(5) ? 0 : reader.GetDecimal(5);
                    entity.fixedbasic = reader.IsDBNull(6) ? 0 : reader.GetDecimal(6);
                    entity.fixedda = reader.IsDBNull(7) ? 0 : reader.GetDecimal(7);
                    entity.spldeductid = reader.IsDBNull(8) ? 0 : reader.GetInt64(8);
                    entity.LeaveDays = reader.IsDBNull(11) ? 0 : reader.GetDecimal(11);
                    entity.SundayLeaves = reader.IsDBNull(12) ? 0 : reader.GetDecimal(12);
                    entity.HolidayLeaves = reader.IsDBNull(13) ? 0 : reader.GetDecimal(13);
                    entity.HolidayAbsent = 0;
                    entity.splallowanceid = reader.IsDBNull(15) ? 0 : reader.GetInt64(15);
                    entity.EmployeeCode = reader.IsDBNull(16) ? "" : reader.GetString(16);
                    retValue.Add(entity);
                }
            }
            //entity.DateOfJoin.Month == month o&& entity.DateOfJoin.Year == year
            return retValue;
        }


    }
}