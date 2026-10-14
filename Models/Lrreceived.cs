using InfyPOS.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class Lrreceived : BaseEntity
    {
        public long id { get; set; }
        public bool isactive { get; set; }
        public long createdby { get; set; }
        public long modifiedby { get; set; }
        public int version { get; set; }
        public DateTime createdon { get; set; }
        public DateTime modifiedon { get; set; }
        public string receiptno { get; set; }
        public long bookingofficeid { get; set; }
        public DateTime receivedon { get; set; }
        public long receivedby { get; set; }
        public long issueid { get; set; }
        public string issueno { get; set; }
        public string lrno { get; set; }
        public DateTime issuedate { get; set; }
        public decimal frieghtcharges { get; set; }
        public decimal noofbundels { get; set; }
        public decimal noofpieces { get; set; }

        public decimal noofboxes { get; set; }
        public decimal actualweight { get; set; }
        public decimal chargedweight { get; set; }
        public decimal loadingcharges { get; set; }

        public bool frieghtchargespaid { get; set; }
        public bool loadingchargespaid { get; set; }

        public List<receiptitem> receiptitem { get; set; }
        public string securityinwardno { get; set; }
        public DateTime securityinwarddate { get; set; }
        public long companyid { get;  set; }

        public decimal othercharges { get; set; }
        public decimal totalcharges { get; set; }
        public decimal receivedweight { get; set; }
    }
    public class LrreceivedCreteria : Lrreceived
    {
        public long supplierid { get; set; }
        public string lrno { get; set; }
    }
}
