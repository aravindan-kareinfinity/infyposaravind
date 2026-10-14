using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class Incentives : BaseEntity
    {
        public int totalsunday { get; set; }
        public int totalholiday { get; set; }
        public long locationid { get; set; }
        public long id { get; set; }
        public int month { get; set; }
        public int year { get; set; }
        public int datatype { get; set; }

        public Calculationtype CalculationType
        {
            get
            {
                return (Calculationtype)datatype;
            }
            set
            {
                datatype = (int)value;
            }
        }
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string data_json { get; set; }
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string parameter_json { get { return Newtonsoft.Json.JsonConvert.SerializeObject(parameter); } set { parameter = Newtonsoft.Json.JsonConvert.DeserializeObject<Parameter>(value); } }
        public Parameter parameter { get; set; }
        public bool isactive { get; set; }
        public long createdby { get; set; }
        public long modifiedby { get; set; }
        public int version { get; set; }
        public DateTime createdon { get; set; }
        public DateTime modifiedon { get; set; }
   //     public class Data
   //     {
   //         public long employeeid { get; set; }
   //         public decimal sundaysalary { get; set; }
   //         public decimal holidaysalary { get; set; }
   //         public decimal sundaysalarydays { get; set; }
   //         public decimal holidaysalarydays { get; set; }
   //         public decimal ratio { get; set; }
   //         public decimal incentive { get; set; }
   //         [JsonIgnore]
   //         [JsonProperty(Required = Required.Default)]
   //         public string employeename { get; set; }
   //         public string employeecode { get; set; }
   //         public decimal amount { get; set; }
			//public decimal days { get; set; }
   //     }
        public class Parameter
        {
            public long companyid { get; set; }
            public long departmentid { get; set; }
            public DateTime from { get; set; }
            public DateTime to { get; set; }

            public int noofmonths { get; set; }

            public decimal percentage { get; set; }

            public long referenceid { get; set; }
        }

        public enum Calculationtype
        {
            none,
            incentive,
            leavesalary,
            bonus,
            overtime,
            beta
        }

        public class LeaveData
        {
            public long employeeid { get; set; }
            public decimal sundaysalary { get; set; }
            public decimal holidaysalary { get; set; }
            public decimal sundaysalarydays { get; set; }
            public decimal holidaysalarydays { get; set; }
            public string employeename { get; set; }

            public string designation { get; set; }
            public string employeecode { get; set; }
            public decimal grosspay { get; set; }
            public int workingdays { get; set; }
        }

        public class SummaryData
        {
            public long employeeid { get; set; }
            public string employeename { get; set;}
            public string employeecode { get; set; }
            public decimal leavesalary { get; set; }
            public decimal beta { get; set; }
            public decimal ot { get; set; }
            public decimal total {
                get
                {
                    return beta + ot + leavesalary;
                }
            }
        }
    }

    public class IncentiveActual<T> : Incentives
    {
        public List<T> data
        {
            get
            {
                return  Newtonsoft.Json.JsonConvert.DeserializeObject<List<T>>(data_json);
            }
            set
            {
                data_json = Newtonsoft.Json.JsonConvert.SerializeObject(value);
            }
        }
    }

    public class IncentivesCreteria : Incentives
    {
        public long employeefloorid { get; set; }
        public long companyid { get; set; }
        public long departmentid { get; set; }
        public long sectionid { get; set; }
        public long employeeid { get; set; }
        public bool regenerate { get; set; }
        public List<int> searchmodes { get; set; }
        public long employeesectionid { get; set; }
        //public int searchmode { get; set; }
        public bool includesales { get; set; }
        public Referencelist.Attribute attributes { get; set; }
        public DateTime fromdate { get; set; }
        public DateTime todate { get; set; }
        public int frommonth { get; set; }
        public int tomonth { get; set; }
        public int fromyear { get; set; }
        public int toyear { get; set; }
        public bool bydate { get; set; }
    }

    
}
