using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class TailoringInwardBundle : BaseEntity
    {
        public string inwardno { get; set; }
        public DateTime receivedon { get; set; }
        public decimal receivedqty { get; set; }
    }
    public class Tailoringitemsreceived : BaseEntity
    {
        public long id { get; set; }
        public string inwardno { get; set; }
        public long tailoringitemsid { get; set; }
        public decimal receivedqty { get; set; }
        public long receivedby { get; set; }
        public long receivedfrom { get; set; }
        public DateTime receivedon { get; set; }
        public DateTime createdon { get; set; }
        public int version { get; set; }
        public bool isactive { get; set; }
        public long modifiedby { get; set; }
        public long createdby { get; set; }
        public DateTime modifiedon { get; set; }
    }

    public class TailoringitemsreceivedDetail : Tailoringitemsreceived
    {
        public string customername { get; set; }
        public string product { get; set; }
        public string orderno { get; set; }
        public string entryno { get; set; }
        public int sno { get; set; }
        public long locationid { get; set; }
        public decimal charge { get; set; }
        public decimal pendingqty { get; set; }
        public decimal expectedqty { get; set; }
        public long tailoringorderid { get; set; }
    }
    public class TailoringitemsreceivedCreteria : Tailoringitemsreceived
    {
        public DateTime fromdate { get; set; }
        public DateTime todate { get; set; }
        public long locationid { get; set; }
        public long companyid { get; set; }

    }
}
