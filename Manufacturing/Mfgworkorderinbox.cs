using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class Mfgworkorderinbox : BaseEntity
    {
        public long id { get; set; }
        public long mfgworkorderid { get; set; }
        public string workorderno { get; set; }
        public long statusid { get; set; }
        public bool isactive { get; set; }
        public long createdby { get; set; }
        public long modifiedby { get; set; }
        public int version { get; set; }
        public long workflowid { get; set; }
        public long stageid { get; set; }
        public DateTime createdon { get; set; }
        public DateTime modifiedon { get; set; }
        public DateTime workorderdate { get; set; }
        public long supplierid { get; set; }
        public string description { get; set; }
        public int inboxtype { get; set; }
        public long mfgjoborderid { get; set; }
        public string jobcardno { get; set; }
        public string joborderno { get; set; }
        public string jobname { get; set; }
        public string lotno { get; set; }
        public int jobstatusid { get; set; }
        public int workorderstatusid { get; set; }
        public DateTime woexpdate { get; set; }
        public DateTime joborderdate { get; set; }
        public DateTime joexpdate { get; set; }
        public string suppliername { get; set; }
        public decimal inqty { get; set; }
        public decimal outqty { get; set; }
        public decimal expqty { get; set; }
        public bool isbomprocess { get; set; }
        public decimal fabricqty { get; set; }
        public decimal wasteqty { get; set; }
        public decimal secondqty { get; set; }
        public decimal transferedqty { get; set; }
        public long locationid { get; set; }
        public long companyid { get; set; }
        public string design { get;  set; }
        public string product { get;  set; }
        public string customername { get;  set; }

        public mfglabelinfo mfglabel { get; set; }
        public class mfglabelinfo
        {
            public string entryno { get; set; }
            public DateTime entrydate { get; set; }
            public string designno { get; set; }
            public string samplename { get; set; }
            public string division { get; set; }
            public string sono { get; set; }
            public DateTime sodate { get; set; }
            public string merchant { get; internal set; }
            public string marketing { get; internal set; }
            public double duedays { get; internal set; }
            public decimal soqty { get; internal set; }
        }
        public enum RowType
        {
            None = 0,
            WorkOrder = 1,
            ExternalWorkOrder = 2,
            JobOrder = 3,
            ExternalJobOrder = 4
        }
    }

    public class MfgworkorderinboxStatus :BaseEntity
    {
        public string statusname { get; set; }
        public int statusid { get; set; }
        public long id { get; set; }
    }
    public class MfgworkorderinboxCreteria : Mfgworkorderinbox
    {
        public DateTime addedfrom { get; set; }
        public DateTime addedto { get; set; }
        public bool showallopen { get; set; }
        public long currentuserid { get; set; }
        public long customerid { get; set; }

    }
}
