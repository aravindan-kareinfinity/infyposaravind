using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Data;

namespace InfyPOS.Models
{
    public class Employeesalary : BaseEntity
    {
        public decimal earnings { get; set; }
        public decimal deductions { get; set; }
        public long id { get; set; }
        public bool isactive { get; set; }
        public DateTime modifiedon { get; set; }
        public DateTime createdon { get; set; }
        public long createdby { get; set; }
        public long modifiedby { get; set; }
        public int version { get; set; }
        public long employeeid { get; set; }
        public decimal netsalary { get; set; }
        public decimal leavedays { get; set; }
        public decimal workingdays { get; set; }
        public decimal pf { get; set; }
        public decimal esi { get; set; }
        public string name { get; set; }
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string detail_json { get { return Newtonsoft.Json.JsonConvert.SerializeObject(detail); } set { detail = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Detail>>(value); } }
        public List<Detail> detail { get; set; }

        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string additionalinfo_json { get { return Newtonsoft.Json.JsonConvert.SerializeObject(additionalinfo); } set { additionalinfo = Newtonsoft.Json.JsonConvert.DeserializeObject<AdditionalInfo>(value); } }
        public AdditionalInfo additionalinfo { get; set; }

        public DateTime salarymonth { get; set; }
        public string employeeno { get; set; }
        public string designation { get; set; }
        public string jobtitle { get; set; }
        public string pfno { get; set; }
        public int workingmode { get; set; }
        public int salarymode { get; set; }
        public long divisionid { get; set; }
        public long locationid { get; set; }
        public long companyid { get; set; }
        public long departmentid { get; set; }
        public long floorid { get; set; }
        public DateTime relievingdate { get; set; }
        public DateTime dateofjoining { get; set; }
        public string sectionid { get; set; }
        public string esino { get; set; }
        public string pfuan { get; set; }
        public DateTime holddate { get; set; }
        public DateTime settlementdate { get; set; }
        public string inwords { get; set; }

        public DateTime paydate { get; set; }
        public bool haswithheld { get; set; }
        public DateTime withheldfrom { get; set; }
        public string companyname { get; set; }
        public decimal presentdays { get;  set; }

        public class Detail
        {
            public long id { get; set; }
            public decimal amount { get; set; }
            public bool isdeduction { get; set; }
            public decimal value { get; set; }
        }

        public class AdditionalInfo
        {
            public decimal workingdays { get; set; }
            public decimal leavetotal { get; set; }
            public decimal leaveencashmentdays { get; set; }
            public decimal acceptedworkdays { get; set; }
            public decimal paydays { get; set; }
            public decimal leavetaken { get; set; }
            public decimal sundayleaves { get; set; }
            public decimal holidayleaves { get; set; }
            public decimal holidaysundayleaves { get; set; }
            public string encashmentmode { get; set; }
            public decimal holidayabsent { get; set; }
            public decimal holidaysundayabsent { get; set; }
            public decimal holidays { get; set; }
            public int sundays { get; set; }
            public decimal totaldays { get; set; }
            public decimal gross { get; set; }
            public decimal basicpay { get; set; }
            public string doj { get; set; }
            public string dor { get; set; }
            public decimal absentdays { get;  set; }
            public decimal additionalabsentdays { get; set; }
        }
    }

    public class EmployeeSalaryUpdation : BaseEntity
    {
        public long id { get; set; }
        public long headerid { get; set; }
        public decimal amount { get; set; }
        public bool isdeduction { get; set; }
    }
    public class EmployeesalaryCreteria : Employeesalary
    {
        public enum FilterModes
        {
            All,
            Payable,
            Left,
            WithHeld
        }
        public int filtermode { get; set; }
        public int month { get; set; }
        public int year { get; set; }
        public long floorid { get; set; }
        public List<long> ids { get; set; }
        public DateTime fromdate { get; set; }
        public DateTime todate { get; set; }
    }

    public class EmployeesalaryGenerate : Employeesalary
    {

        public int month { get; set; }
        public int year { get; set; }
        public long floorid { get; set; }

    }
    public class EmployeeSalaryHeaderSchedule:BaseEntity
    {
        public int month { get; set; }
        public int year { get; set; }
        public long headerid { get; set; }
        public long locationid { get; set; }
        public long companyid { get; set; }
        public List<Schedule> schedules { get; set; }
        public class Schedule
        {
            [JsonIgnore]
            public long id { get; set; }
            public long salaryid { get; set; }
            public long employeeid { get; set; }
            public decimal amount { get; set; }
        }
    }
    public class EmployeeSalaryConfig : BaseEntity
    {
        public DateTime fromdate { get; set; }
        public DateTime todate { get; set; }
        public int month { get; set; }
        public int year { get; set; }
        public long employeeid { get; set; }
        public int salarymode { get; set; }
        public DateTime paydate { get; set; }

        public long locationid { get; set; }
        public long companyid { get; set; }
        public long departmentid { get; set; }
        public long floorid { get; set; }
        public long filtermode { get; set; }

        public long workingmode { get; set; }
        public int workdays_mode { get; set; }

        public enum WorkDays_Mode
        {
            FixedDays = 1,
            AllDays = 2
        }
        public enum LeaveDaysType
        {
            FixedDays = 1,
            Config = 2,
            Prorate=3
        }

        public enum LeaveEncashType
        {
            FixedDays = 1,
            Config = 2
        }

        public enum HolidayDayType
        {
            FixedDays = 1,
            AllDays = 2
        }

        public bool updatedafromstructure { get; set; }
        public bool updatebpfromstructure { get; set; }
        public bool proratedaysbased { get; set; }
        public int workdays_fixed { get; set; }
        public bool workdays_exceptsunday { get; set; }
        public bool workdays_exceptholiday { get; set; }
        public bool workdays_exceptleave { get; set; }

        public int leave_mode { get; set; }
        public int leave_fixeddays { get; set; }
        public bool leave_includesunday { get; set; }
        public bool leave_includeholiday { get; set; }
        public bool leave_proportionate { get; set; }
        public bool leave_absentadjustment { get; set; }
        public int encashment_fixeddays { get; set; }
        public int encashment_workdays_mode { get; set; }

        public int encashment_workdays_fixed { get; set; }
        public bool encashment_workdays_exceptsunday { get; set; }
        public bool encashment_workdays_exceptholiday { get; set; }

        public bool encashment_exceptsunday { get; set; }
        public bool encashment_exceptholiday { get; set; }

        public int encashment_mode { get; set; }
        public int encashment_perday { get; set; }
        public int encashment_swapmonth { get; set; }
        public int encashment_eligiblemonth { get; set; }
        public bool encashment_allemployees { get; set; }
        public bool encashment_adjustabsent { get; set; }
        public int encashment_permanent_extra { get; set; }
        public int encashment_permanent_minimum { get; set; }
        public int holiday_mode { get; set; }
        public int holiday_fixeddays { get; set; }
        public bool holiday_exceptsunday { get; set; }
        public bool holiday_exceptholiday { get; set; }
        public bool holiday_adjustabsent { get; set; }
        public bool holiday_doublepay { get; set; }
        public bool holiday_permanentonly { get; set; }
        public bool fine_leave { get; set; }
        public decimal fine_leave_perday { get; set; }
        public bool fine_late { get; set; }
        public decimal fine_late_perminute { get; set; }
        public decimal fine_late_fee { get; set; }

        public bool overtime_morning { get; set; }
        public decimal overtime_morning_perminute { get; set; }
        public decimal overrime_morning_fee { get; set; }

        public bool overtime_formula { get; set; }
        public decimal overtime_multiplier { get; set; }
        public int overtime_source { get; set; }
        public decimal overtime_perdayhours { get; set; }
        public decimal overtime_totaldays { get; set; }

        public bool overtime_evening { get; set; }
        public decimal overtime_evening_perminute { get; set; }
        public decimal overrime_evening_fee { get; set; }
        public bool incentive { get;  set; }
        public bool bydate { get;  set; }
        public DateTime generatefrom { get;  set; }
        public DateTime generateto { get;  set; }

        public bool fine_formula { get; set; }
        public decimal fine_multiplier { get; set; }
        public int fine_source { get; set; }
        public decimal fine_perdayhours { get; set; }
        public decimal fine_totaldays { get; set; }
    }

    //public class EmployeeIncentiveCreteria : BaseEntity
    //{
    //    public int locationid { get; set; }
    //    public int frommonth { get; set; }
    //    public int fromyear { get; set; }
    //    public int tomonth { get; set; }
    //    public int toyear { get; set; }
    //    public int departmentid { get; set; }


    //}

    //public class EmployeeLeaveCreteria : BaseEntity
    //{
    //    public long employeeid { get; set; }
    //    public int month { get; set; }
    //    public int year { get; set; }
    //    public int departmentid { get; set; }
    //    public int locationid { get; set; }


    //}

    //public class EmployeeBonusCreteria : BaseEntity
    //{
    //    public long employeeid { get; set; }
    //    public int month { get; set; }
    //    public int year { get; set; }
    //    public int departmentid { get; set; }
    //    public int locationid { get; set; }

    //}
    //public class EmployeeBetaCreteria : BaseEntity
    //{
    //    public long employeeid { get; set; }
    //    public int month { get; set; }
    //    public int year { get; set; }
    //    public int departmentid { get; set; }
    //    public int locationid { get; set; }

    //}

    //public class EmployeeOTCreteria : BaseEntity
    //{
    //    public long employeeid { get; set; }
    //    public int month { get; set; }
    //    public int year { get; set; }
    //    public int departmentid { get; set; }
    //    public int locationid { get; set; }
    //}

}
