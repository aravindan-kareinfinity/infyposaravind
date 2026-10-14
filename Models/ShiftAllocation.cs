using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class ShiftAllocation : BaseEntity
    {
        public List<DateTime> days { get; set; }

        public int maxemployeeperday { get; set; }
        public long id { get; set; }
        public long departmentid {get; set;}
        public long companyid { get; set; }
        public long locationid { get; set; }
        public int monthid { get; set; }
        public int floorid { get; set; }
        public int sectionid { get; set; }
        public int year { get; set; }
        public DateTime fromdate { get; set; }
        public DateTime todate { get; set; }
        public bool isactive { get; set; }
        public long createdby { get; set; }
        public long modifiedby { get; set; }
        public int version { get; set; }
        public DateTime createdon { get; set; }
        public DateTime modifiedon { get; set; }
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string allocation_json { get { return Newtonsoft.Json.JsonConvert.SerializeObject(allocation); } set { allocation  = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Allocation>>(value); } }
        public List<Allocation> allocation { get; set; }
        public class Allocation : Employee
        {
            public Allocation()
            {
                shifts = new List<AllocationShift>();
            }
            public DateTime dateofjoin { get; set; }
            public DateTime dateofrelieaving { get; set; }
            public bool hasleft { get;  set; }
            public bool existing { get; set; }
            public DayOfWeek day { get; set; }

            public long sectionid { get; set; }
            public List<AllocationShift> shifts { get; set; }
        }
        public class AllocationShift
        {
            public int shift { get; set; }
            public DateTime date { get; set; }
        }
        public class Employee
        {
            public long employeeid { get; set; }
            public string employeecode { get; set; }
            public string name { get; set; }
            [JsonIgnore]
            public DateTime Ndateofjoin { get; set; }
            [JsonIgnore]
            public DateTime Ndateofrelieaving { get; set; }
            [JsonIgnore]
            public bool Nhasleft { get; set; }

            public long fixedleaveday { get; set; }
        }
        public class Summary
        {
            public string day { get; set; }
            public string days { get; set; }

            public List<Employee> employees { get; set; }

        }

        public List<Summary> summary { get; set; }
    }

    public class ShiftAllocationCreteria : ShiftAllocation
    {
        public bool regenerate { get; set; }
        public int salarymode { get;  set; }
        public int workingmode { get; set; }
        public bool fetchallemployees { get; set; }
        public bool weekbased { get; set; }
        public bool week4model { get; set; }
    }
}
