using InfyPOS.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class Denomination : BaseEntity
    {
        public int id { get; set; }
        public int employeeid { get; set; }
        public string date { get; set; }
        public DateTime time { get; set; }
        public float amount { get; set; }
       // public string denomination { get; set; }
        public int counterid { get; set; }
        public string isactive { get; set; }
        public int createdby { get; set; }
        public int modifiedby { get; set; }
        public int version { get; set; }
        public DateTime createdon { get; set; }
        public DateTime modifiedon { get; set; }
    }

    public class DenominationCreteria : Denomination
    {
}
}
