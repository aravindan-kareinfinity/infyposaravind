using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace InfyPOS.Payroll
{
    public class SalarySummary : Common.BaseData
    {
        public List<Item> Search(SalarySummaryRequest request)
        {
            Data.EmployeesalaryDataService salarydata = new Data.EmployeesalaryDataService() { Context = this.Context };
            return salarydata.SearchSummary(request);
        }

        public List<DailySalary> SearchDailywages(InfyPOS.Models.EmployeesalaryCreteria request)
        {
            Data.EmployeeDataService employdata = new Data.EmployeeDataService() { Context = this.Context };
            var seasonalStaffs = employdata.Search(new Models.EmployeeCreteria()
            {
                salarymode = 1
            });


            Data.EmployeesalaryDataService salarydata = new Data.EmployeesalaryDataService() { Context = this.Context };
            salarydata.Context = this.Context;
            request.salarymode = 1;
            var result = salarydata.Search(request);
            DateTime fromdate = new DateTime(request.year, request.month, 1);
            Data.EmployeesalaryscheduleDataService scheduledAdvance = new Data.EmployeesalaryscheduleDataService() { Context = this.Context };
            var slist = scheduledAdvance.SearchSchedule(fromdate, fromdate.AddMonths(1));
            List<DailySalary> ret = new List<DailySalary>();
            foreach (var item in result)
            {
                DailySalary ds = new DailySalary();
                ds.employeecode = item.employeeno; ;
                ds.name = item.name;
                ds.salarymonth = item.salarymonth;
                ds.earnings = item.earnings;
                ds.designation = item.designation;
                ds.paiddays = (item.workingdays - item.leavedays);
                if ((ds.paiddays) > 0)
                    ds.perday = Math.Round(item.earnings / ds.paiddays, 0);
                
                if (seasonalStaffs.Exists(e => e.id == item.employeeid))
                {
                    var emp = seasonalStaffs.Find(e => e.id == item.employeeid);
                    ds.dateofjoin = emp.dateofjoining;
                    ds.gender = emp.gender == "M" ? "Male" : "Female";
                }
                ds.deductions = item.deductions;
                ds.advance = slist.Where(e => e.employeeid == item.employeeid && !e.payateom).Sum(e => e.amount);
                ds.advanceeom = slist.Where(e => e.employeeid == item.employeeid && e.payateom).Sum(e => e.amount);
                ds.salary = item.earnings - ds.deductions;
                ds.advancelist = new List<DailySalary.advanceitem>();
                ds.advancelist.AddRange(slist.Where(e => e.employeeid == item.employeeid).ToList().ConvertAll(e => new DailySalary.advanceitem()
                {
                    date = e.schedulemonth,
                    amount = e.amount
                }));
                ret.Add(ds);
            }
            return ret;
        }

        public List<DailySalary> GenerateDailywages(InfyPOS.Models.EmployeesalaryCreteria request)
        {
            request.fromdate = request.fromdate.Date;
            request.todate = request.todate.Date.AddDays(1);
            Data.EmployeeDataService employdata = new Data.EmployeeDataService() { Context = this.Context };
            var seasonalStaffs = employdata.Search(new Models.EmployeeCreteria()
            {
                salarymode = 1
            });

            InfyPOS.Data.SalaryManager salary = new InfyPOS.Data.SalaryManager() { Context = this.Context };
            salary.Initialize(this.Context.OrganizationID, this.Context);
            var leaveinformation = salary.BuildMasters(request.fromdate.Date, request.todate, request.employeeid, 1);
           
            Data.EmployeesalaryscheduleDataService scheduledAdvance = new Data.EmployeesalaryscheduleDataService() { Context = this.Context };
            var slist = scheduledAdvance.SearchSchedule(request.fromdate, request.todate);
            DateTime todate = request.todate.AddDays(-1);
            List<DailySalary> ret = new List<DailySalary>();
            decimal totalworkingdays =(decimal) request.todate.Date.Subtract(request.fromdate.Date).TotalDays;
            foreach (var item in leaveinformation)
            {
                if (item.Hasleft && item.Relievingdate < request.fromdate.Date) continue;
                DailySalary ds = new DailySalary();
                ds.employeecode = item.EmployeeCode; ;
                ds.name = item.Name;
                decimal totalwordays = item.fulldays + (item.halfday * (decimal)0.5);
                double befordays = (request.fromdate.Date < item.DateOfJoin.Date) ? item.DateOfJoin.Date.Subtract(request.fromdate.Date).TotalDays : 0;
                double afterdays = (item.Hasleft && request.fromdate.Date < item.Relievingdate.Date && request.todate.Date>item.Relievingdate.Date) ? todate.Date.Subtract(item.Relievingdate.Date).TotalDays : 0;

                totalwordays = totalworkingdays - (item.LeaveDays + (decimal)befordays + (decimal)afterdays);

                decimal basicPay = Math.Round(item.BasicPay * totalwordays, 0);

                ds.earnings = basicPay;
                ds.designation = item.Designation;
                ds.paiddays = totalwordays;
                if ((ds.paiddays) > 0)
                    ds.perday = item.BasicPay;

                if (seasonalStaffs.Exists(e => e.id == item.EmployeeID))
                {
                    var emp = seasonalStaffs.Find(e => e.id == item.EmployeeID);
                    ds.dateofjoin = emp.dateofjoining;
                    ds.gender = emp.gender == "M" ? "Male" : "Female";
                }
                ds.advance = slist.Where(e => e.employeeid == item.EmployeeID && !e.payateom).Sum(e => e.amount);
                ds.advanceeom = slist.Where(e => e.employeeid == item.EmployeeID && e.payateom).Sum(e => e.amount);
                ds.salary = ds.earnings - ds.advance;
                ds.advancelist = new List<DailySalary.advanceitem>();
                ds.advancelist.AddRange(slist.Where(e => e.employeeid == item.EmployeeID).ToList().ConvertAll(e => new DailySalary.advanceitem()
                {
                    date = e.schedulemonth,
                    amount = e.amount
                }));
                ds.salaryinwords = WebAPI.Data.Numberconvertor.rupees(ds.salary);
                ret.Add(ds);
            }
            return ret;
        }

        public class DailySalary
        {
            public DateTime dateofjoin { get; set; }
            public string gender { get; set; }
            public string name { get; set; }
            public string employeecode { get; set; }
            public DateTime salarymonth { get; set; }
            public decimal earnings { get; set; }
            public decimal salary { get; set; }
            public decimal deductions { get; set; }
            public decimal perday { get; set; }
            public decimal paiddays { get; set; }
            public decimal advance { get; set; }
            public decimal advanceeom { get; set; }
            public string designation { get; set; }
            public List<advanceitem> advancelist { get; set; }
            public string salaryinwords { get; set; }

            public class advanceitem
            {
                public DateTime date { get; set; }
                public decimal amount { get; set; }
            }
        }
        public class Item
        {
            public int sno { get; set; }
            public string name { get; set; }
            public string employeecode { get; set; }
            public DateTime salarymonth { get; set; }
            public decimal netsalary { get; set; }
            public decimal pf { get; set; }
            public decimal esi { get; set; }
            public decimal earnings { get; set; }
            public decimal deductions { get; set; }

        }
    }

    public class SalarySummaryRequest : InfyPOS.Common.BaseEntity
    {
        public DateTime from { get; set; }
        public DateTime to { get; set; }
        public long companyid { get; set; }
        public long employeeid { get; set; }
        public Mode mode { get; set; }
        public enum Mode
        {
            CompanySummary,
            EmployeeSummary,
        }
    }
}