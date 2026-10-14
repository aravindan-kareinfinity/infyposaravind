using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class TailoringOutwardBundle : BaseEntity
    {
        public string outwardno { get; set; }
        public DateTime issuedon { get; set; }
        public decimal issuedqty { get; set; }
    }
    public class Tailoringitemsissued : BaseEntity
    {
        public long id { get; set; }
        public long tailoringitemsid { get; set; }
        public decimal issuedqty { get; set; }
        public long issuedby { get; set; }
        public long issuedto { get; set; }
        public string outwardno { get; set; }
        public DateTime issuedon { get; set; }
        public DateTime createdon { get; set; }
        public int version { get; set; }
        public bool isactive { get; set; }
        public long modifiedby { get; set; }
        public long createdby { get; set; }
        public DateTime modifiedon { get; set; }
    }
    
    public class TailoringitemsissuedDetail : Tailoringitemsissued
    {
        public string customername { get; set; }
        public string product { get; set; }
        public string orderno { get; set; }
        public string entryno { get; set; }
        public int sno { get; set; }
        public long locationid { get; set; }
        public decimal charge { get; set; }
        public decimal sourcemeter { get; set; }
        public decimal sourceqty { get; set; }
        public decimal expectedqty { get; set; }
        public decimal outwardqty { get; set; }
        public long tailoringorderid { get; set; }
    }

    public class TailoringitemsissuedCreteria : Tailoringitemsissued
    {
        public DateTime fromdate { get; set; }
        public DateTime todate { get; set; }
        public long locationid { get; set; }
        public long companyid { get; set; }
    }
}
