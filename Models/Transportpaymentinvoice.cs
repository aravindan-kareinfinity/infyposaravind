using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class Transportpaymentinvoice : BaseEntity
    {
        public enum SourceModes
        {
            Transport,
            Courier
        }
        public int sourcemode { get; set; }
        public long transportid { get; set; }
        public long createdby { get; set; }
        public bool isactive { get; set; }
        public DateTime createdon { get; set; }
        public decimal paidamount { get; set; }
        public string lrno { get; set; }
        public bool isloadingcharge { get; set; }
        public long transportpaymentid { get; set; }
        public DateTime modifiedon { get; set; }
        public long lrid { get; set; }
        public int version { get; set; }
        public long modifiedby { get; set; }
        public long id { get; set; }
        public decimal discountamount { get; set; }
        public decimal discountamountpercent { get; set; }
        public DateTime receivedon { get; set; }
        public DateTime lrdate { get; set; }
        [JsonIgnore]
        public decimal balance { get; set; }
        public string fromlocation { get; set; }

    }

    public class Transportpaymentinvoicedetail : Transportpaymentinvoice
    {
        public string supplier { get; set; }
        public string lrentryno { get; set; }
        public decimal actualweight { get; set; }
        public decimal chargedweight { get; set; }
        public decimal frightcharge { get; set; }
        public decimal loadingcharge { get; set; }
        public decimal othercharges { get; set; }
        public decimal noofbundelsreceived { get; set; }
        public decimal noofboxesreceived { get; set; }
        public decimal totalamount { get; set; }
        public decimal balanceamount { get; set; }
        public string invoiceno { get; set; }
        public decimal taxpercentage { get; set; }
        public decimal taxamount { get; set; }
        public decimal taxableamount { get; set; }
        public decimal cgsttaxpercentage { get { return taxpercentage > 0 ? taxpercentage / 2 : 0; } }
        public decimal cgsttaxamount { get { return taxamount > 0 ? taxamount / 2 : 0; } }
        public decimal sgsttaxpercentage { get { return taxpercentage > 0 ? taxpercentage / 2 : 0; } }
        public decimal sgsttaxamount { get { return taxamount > 0 ? taxamount / 2 : 0; } }


        public decimal actualpaid
        {
            get
            {
                return paidamount - discountamount;
            }
        }
        public int paymode { get; set; }
        public string Payment
        {
            get
            {
                if (paymode == 0) return "-";
                return paymode == 1 ? "SELF" : "TO PAY";
            }
        }
    }
    public class TransportpaymentinvoiceCreteria : Transportpaymentinvoice
    {
    }
}
