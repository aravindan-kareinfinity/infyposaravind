using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class Customerwalkins : BaseEntity
    {
        public long createdby { get; set; }
        public long id { get; set; }
        public DateTime createdon { get; set; }
        public int version { get; set; }
        public bool isactive { get; set; }
        public long modifiedby { get; set; }
        public DateTime modifiedon { get; set; }
        public int walkincount { get; set; }
        public string mobileno { get; set; }
        public string name { get; set; }
        public string gender { get; set; }
        public string ageset { get; set; }
        public string reason { get; set; }
        public string brand { get; set; }
        public DateTime fromtime { get; set; }
        public DateTime totime { get; set; }
        public string colourreason { get; set; }
        public string sizereason { get; set; }
        public string billno { get; set; }
        public int rating { get; set; }
        public long companyid { get; set; }
        public long sectionid { get; set; }
        public long locationid { get; set; }
        public string feedback { get; set; }
        public string walkintoken { get; set; }

    }
    public class CustomerwalkinsCreteria : Customerwalkins
    {
        public bool includebilledcustomer { get; set; }
        public DateTime fromdate { get; set; }
        public DateTime todate { get; set; }
    }

    public class CustomerWalkinSummary:BaseEntity
    {
        public DateTime walkinat { get; set; }
        public string walkinhour { get; set; }
        public string walkindate { get; set; }
        public int walkincount { get; set; }
        public int billcount { get; set; }
        public int billedcustomercount { get; set; }
        public string company { get;  set; }
        public string location { get;  set; }
        public string section { get;  set; }
    }
}
