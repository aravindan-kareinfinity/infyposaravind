using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class Tailoringpayment : BaseEntity
    {
        public long id { get; set; }
        public long tailoringorderid { get; set; }
        public string paidby { get; set; }
        public string paymentcode { get; set; }
        public long customerid { get; set; }
        public long collectedby { get; set; }
        public long companyid { get; set; }
        public long locationid { get; set; }
        public int paymode { get; set; }
        public long bankid { get; set; }
        public decimal amount { get; set; }
        public decimal refund { get; set; }
        public DateTime collectedon { get; set; }
        public DateTime createdon { get; set; }
        public bool paidbycustomer { get; set; }
        public int version { get; set; }
        public bool isactive { get; set; }
        public long modifiedby { get; set; }
        public long createdby { get; set; }
        public long Tailoringitemid { get; set; }
        public long tailorid { get; set; }
        public DateTime modifiedon { get; set; }
    }
    public class TailoringpaymentCreteria : Tailoringpayment
    {
        public DateTime fromdate { get; set; }
        public DateTime todate { get; set; }
    }

    public class TailoringPaymentDetail : Tailoringpayment
    {
        public DateTime orderdate { get; set; }
        public string tailorname { get; set; }
        public string entryno { get; set; }
        public string orderno { get; set; }
        public string customername { get; set; }
        public string product { get; set; }
        public string measurementname { get; set; }
        public decimal receivedqty { get; set; }
        public decimal charge { get; set; }
        public decimal delaycharge { get; set; }
        public decimal prioritycharge { get; set; }
        public decimal priorityqty { get; set; }
    }

    public class TailoringPaymentPrint : TailoringPaymentDetail
    {
        public decimal totalamount { get; set; }
        public decimal totalqty { get; set; }
        public decimal totaldelaycharge { get; set; }
        public decimal totalprioritycharge { get; set; }
        public decimal totalpriorityqty { get; set; }
        public List<TailoringPaymentDetail> items { get; set; }
        public CompanyLocation location { get; internal set; }
        public Company company { get; internal set; }
        public string amountinwords
        {
            get
            {
                return WebAPI.Data.Numberconvertor.rupees(totalamount);
            }
        }
    }

}
