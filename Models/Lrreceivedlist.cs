using InfyPOS.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class receiptitem : BaseEntity
    {
        public long id { get; set; }
        public bool isactive { get; set; }
        public long createdby { get; set; }
        public long modifiedby { get; set; }
        public int version { get; set; }
        public DateTime createdon { get; set; }
        public DateTime modifiedon { get; set; }
        public long lrid { get; set; }
        public string lrno { get; set; }
        public long supplierid { get; set; }
        public decimal noofbundels { get; set; }
        public decimal noofpieces { get; set; }
        public decimal noofboxes { get; set; }
        public decimal actualweight { get; set; }
        public decimal chargedweight { get; set; }
        public decimal loadingcharge { get; set; }
        public long fromlocationid { get; set; }
        public decimal totalamount { get; set; }
        public bool isreversed { get; set; }
        public long lrreceivedid { get; set; }
        public decimal noofbundelsreceived { get; set; }
        public decimal noofboxesreceived { get; set; }
        public decimal frightcharge { get; set; }
        public decimal receivedweight { get; set; }
        public decimal forward { get; set; }
        public decimal packing { get; set; }
        public decimal others { get; set; }
        public decimal insurance { get; set; }
    }
    public class LrreceivedlistCreteria : receiptitem
    {
        public bool onlyreceived { get; set; }
    }
}
