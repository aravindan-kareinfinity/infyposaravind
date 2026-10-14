using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class Employeesalaryschedule : BaseEntity
    {
        public long id { get; set; }
        public bool isactive { get; set; }
        public bool payateom { get; set; }
        public DateTime modifiedon { get; set; }
        public DateTime createdon { get; set; }
        public long createdby { get; set; }
        public long modifiedby { get; set; }
        public int version { get; set; }
        public long employeeid { get; set; }
        public bool isreceivable { get; set; }
        public decimal amount { get; set; }
        public string detail { get; set; }
        public DateTime schedulemonth { get; set; }
        public long salaryheaderid { get; set; }
        public long customerinvoiceid { get;set; }
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string deductionconfig_json
        {
            get { return Newtonsoft.Json.JsonConvert.SerializeObject(deductionconfig); }
            set
            {
                if (value != null)
                    deductionconfig = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ScheduleItem>>(value);
            }
        }
        public List<ScheduleItem> deductionconfig { get; set; }
        public long companyid { get; set; }
        public long locationid { get; set; }
        public string name { get; set; }
        public string employeecode { get; set; }
        public string designation { get; set; }
        public long parentid { get;  set; }
        public decimal currentamount { get; set; }
        public string remarks { get; set; }
        public string inwords { get;  set; }
        public string companyname { get;  set; }
        public decimal paid { get;  set; }
        public decimal receivable { get;  set; }


        public class ScheduleItem
        {
            public decimal amount { get; set; }
            public DateTime schedulemonth { get; set; }
        }
    }

    public class EmployeesalaryscheduleDetails : Employeesalaryschedule
    {
        public decimal pendingamt { get; set; }
        public decimal openingamt { get; set; }
        public string description { get; set; }
    }
    
    public class EmployeeAdvance : BaseEntity
    {
        public long salaryheaderid { get; set; }
        public DateTime salarymonth { get; set; }
        public List<ScheduleItem> schedules { get; set; }
        public bool payateom { get; set; }
        public string ErrorMessage { get;  set; }
        public int searchmode { get; set; }
        public class ScheduleItem
        {
            public long employeeid { get; set; }
            public decimal amount { get; set; }
            public DateTime advancedate { get; set; }
        }
        public string remarks { get; set; }
        public long companyid { get;  set; }
        public long locationid { get;  set; }
    }

    public class EmployeesalaryscheduleCreteria : Employeesalaryschedule
    {
        public DateTime from { get; set; }
        public DateTime to { get; set; }
        public FilterMode mode { get; set; }
        public long floorid { get; set; }
        public long departmentid { get; set; }
        public long sectionid { get; set; }
        public bool showhasleft { get; set; }
        public int workingmode { get; set; }
        public int salarymode { get; set; }
        public bool showallemployee { get; set; }
        public int searchmode { get; set; }
        public enum SearchMode
        {
            None,
            Search,
            Multiple,
            Indivitual,
            Summary
        }
        public int searchmonth { get; set; }
        public int searchyear { get; set; }
        public enum FilterMode
        {
            All,
            ScheduledPaid,
            ScheduledReceivable,
            AdvancePaid,
            AdvanceReceivble,
            PendingBills,
            OtherDeduction
        }
        public DateTime fromdate { get; set; }
        public DateTime todate { get; set; }
        public bool summary { get; set; }
        public string particulars { get; set; }
        public decimal deductionamt { get; set; }
        public decimal loanamt { get; set; }
        public DateTime date { get; set; }
        
    }

    public class EmployeeScheduleDetail
    {
        public decimal receivable { get; set; }
        public decimal paid { get; set; }
        public decimal pendingamt { get; set; }
        public long employeeid { get; set; }
        public string employeename { get; set; }
        public string employeecode { get; set; }

        public List<AdvanceDetails> advancedetails { get; set; }
        public class AdvanceDetails
        {
            public string particulars { get; set; }
            public decimal deductionamt { get; set; }
            public decimal loanamt { get; set; }
            public DateTime date { get; set; }
        }
    }
}
