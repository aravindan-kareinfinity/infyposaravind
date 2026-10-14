using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class Agentpaymentinvoice : BaseEntity
    {
        public decimal discountamountpercent { get; set; }
        public decimal discountamount { get; set; }
        public decimal paidamount { get; set; }
        public long id { get; set; }
        public DateTime createdon { get; set; }
        public bool isactive { get; set; }
        public long supplierpaymentinvoiceid { get; set; }
        public long supplierpaymentid { get; set; }
        public long createdby { get; set; }
        public long modifiedby { get; set; }
        public int version { get; set; }
        public bool isinterstatesale { get; set; }
        public DateTime modifiedon { get; set; }
        public long agentpaymentid { get; set; }
        public long taxledgerid { get; set; }
        public string invoiceno { get; set; }
        public string receiptno { get; set; }

        public DateTime invoicedate { get; set; }
        public DateTime supplierpaiddate { get; set; }
        public long supplierid { get; set; }
        public decimal agentcommissionvalue { get; set; }
        public decimal agentcommission { get; set; }
        public decimal taxamount { get; set; }
        public decimal invpaidamount { get; internal set; }
        public decimal invdisount { get; internal set; }
    }
    public class AgentpaymentinvoiceDetail : Agentpaymentinvoice
    {
        public string agentname { get; set; }
        public string gstno { get; set; }
        public long agentid { get; set; }
        public long locationid { get; set; }
        public decimal totalamount
        {
            get
            {
                return agentcommissionvalue + taxamount - paidamount;
            }
        }

        public decimal invpaidamount { get;  set; }
        public decimal invdisount { get;  set; }
    }
}
