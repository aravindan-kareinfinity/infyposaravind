using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class Employeeleavemaster : BaseEntity
    {
        public long id { get; set; }
        public long employeeid { get; set; }
        public string leavetype { get; set; }
        public int days { get; set; }
        public int availdays { get; set; }
        public DateTime expirydate { get; set; }
        public bool carryforward { get; set; }
        public bool isactive { get; set; }
        public long createdby { get; set; }
        public long modifiedby { get; set; }
        public int version { get; set; }
        public DateTime createdon { get; set; }
        public DateTime modifiedon { get; set; }
        public long institutionid { get; set; }
    }
    public class EmployeeleavemasterCreteria : Employeeleavemaster
    {
    }
}
