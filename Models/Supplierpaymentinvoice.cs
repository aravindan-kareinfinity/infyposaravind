using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class Supplierpaymentinvoice : BaseEntity
    {
        public long id { get; set; }
        public long supplierid { get; set; }
        public long invoiceid { get; set; }
        public string invoiceno { get; set; }
        public string coreinvoiceno { get; set; }
        public DateTime coreinvoicedate { get; set; }
        public long lrinvoicereturnid { get; set; }
        public long companyid { get; set; }
        public string lrinvoicereturnno { get; set; }
        public long returntype { get; set; }
        public decimal paidamount { get; set; }
        public bool isactive { get; set; }
        public long createdby { get; set; }
        public long modifiedby { get; set; }
        public int version { get; set; }
        public DateTime createdon { get; set; }
        public DateTime modifiedon { get; set; }
        public long supplierpaymentid { get; set; }
        public bool isjobwork { get; set; }
        public bool iscreditnote { get; set; }
        public bool islrinvoicereturn { get; set; }
        [JsonIgnore]
        public decimal balance { get; set; }
        [JsonIgnore]
        public decimal invoiceamount { get; set; }
        public DateTime invoicedate { get; set; }
        public decimal discountamountpercent { get; set; }
        public decimal discountamount { get; set; }
        [JsonIgnore]
        public decimal invoiceamountdisplay { get {
                return islrinvoicereturn ? 0 : invoiceamount;
            } }
        [JsonIgnore]
        public decimal actualpaidamount
        {
            get
            {
                return paidamount - discountamount;
            }
        }
        public long agentid { get; set; }
        public decimal agentcommission { get; set; }
        public decimal agentcommissionvalue { get; set; }
        public decimal intreceive { get; set; }
        public decimal intpay { get; set; }

        [JsonIgnore]
        public string lrno { get; set; }
        [JsonIgnore]
        public string transport { get; set; }
        [JsonIgnore]
        public DateTime lrdate { get; set; }
        [JsonIgnore]
        public string remarks { get; set; }
        public string invoicetype { get; set; }
    }
    public class SupplierpaymentinvoiceCreteria : Supplierpaymentinvoice
    {
    }
}
