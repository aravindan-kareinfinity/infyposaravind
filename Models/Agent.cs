using InfyPOS.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class AgentBase : BaseEntity
    {
        public long id { get; set; }
        public string name { get; set; }
        public decimal commissionamount { get; set; }
        public decimal commissionpercentage { get; set; }

        public int agenttypeid { get; set; }

    }
    public class Agent : AgentBase
    {
        public string contactperson { get; set; }
        public string contactno { get; set; }
        public string address { get; set; }
        public long cityid { get; set; }
        public long stateid { get; set; }
        public long countryid { get; set; }
        public string pincode { get; set; }
        public bool isactive { get; set; }
        public long createdby { get; set; }
        public long modifiedby { get; set; }
        public int version { get; set; }
        public DateTime createdon { get; set; }
        public DateTime modifiedon { get; set; }
        public string emailid { get; set; }
        public string pan { get; set; }
        public string gst { get; set; }

        public string bankbranch { get; set; }
        public string bankaccountname { get; set; }
        public string bankifci { get; set; }
        public string bankaccountno { get; set; }
        public long bankid { get; set; }
        public long taxledgerid { get; set; }
        public string employeename { get; set; }
        public string bankname { get; internal set; }
    }
    public class AgentCreteria : Agent
    {
        public bool inactive { get; set; }
        public DateTime fromdate { get; set; }
        public DateTime todate { get; set; }
    }
}
