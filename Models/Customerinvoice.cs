using InfyPOS.Common;
using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class Customerinvoice : BaseEntity
    {
        public long id { get; set; }
        public bool isactive { get; set; }
        public long createdby { get; set; }
        public long modifiedby { get; set; }
        public int version { get; set; }
        public DateTime createdon { get; set; }
        public DateTime modifiedon { get; set; }
        public int type { get; set; }
        public long paidcustomerinvoiceid { get; set; }
        public enum Types
        {
            SettlementCredit = 1,
            B2BCredit = 2,
            CustomerOrder = 3,
            CreditCollection = 4,
            VoucherAdvance = 5,
            CreditNote = 6
        }
        public decimal receivable { get; set; }
        public decimal paid { get; set; }

        public decimal paidafterdiscount
        {
            get
            {
                return paid - discount;
            }
        }
        public decimal discount { get; set; }
        public DateTime transactiondate { get; set; }
        public long billid { get; set; }
        public long customerid { get; set; }
        public string customername { get; set; }
        public string mobileno { get; set; }
        public long paymentid { get; set; }
        public string receiptno { get; set; }
        public string orderno { get; set; }
        public decimal tax { get; set; }
        public long orderid { get; set; }
        public string billno { get; set; }
        public DateTime billdate { get; set; }
        public long approvedby { get; set; }
        public long companyid { get; set; }
        public long pendinginvoiceid { get; set; }
        public decimal advance { get; set; }
        public decimal qty { get; set; }
    }
    public class CustomerinvoiceCreteria : Customerinvoice
    {
        public DateTime fromdate { get; set; }
        public DateTime todate { get; set; }
        public bool showpending { get; set; }
        public List<long> ids { get; set; }
        public long locationid { get; set; }
    }
}
