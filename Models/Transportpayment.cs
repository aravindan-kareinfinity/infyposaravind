using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class TransportPayment : BaseEntity
    {
        public enum SourceModes
        {
            Transport,
            Courier
        }
        public int sourcemode { get; set; }
        public string referencedetail { get; set; }
        public DateTime instrumentdate { get; set; }
        public long bankid { get; set; }
        public DateTime createdon { get; set; }
        public int version { get; set; }
        public long modifiedby { get; set; }
        public long createdby { get; set; }
        public bool isactive { get; set; }
        public decimal paidamount { get; set; }
        public bool datatransmitted { get; set; }
        public DateTime paymentdate { get; set; }
        public string receiptno { get; set; }
        public string lrentrynos { get; set; }
        public long transportid { get; set; }
        public DateTime modifiedon { get; set; }
        public long id { get; set; }
        public long locationid { get; set; }
        public long companyid { get; set; }
        public decimal rounding { get; set; }
        public bool hastds { get; set; }
        public decimal tdspercentage { get; set; }
        public decimal tdsamount { get; set; }
        public string remarks { get; set; }
        public string invoicenos { get; set; }
        public List<Transportpaymentinvoicedetail> Transportpaymentinvoice { get; set; }

        //public List<TransportPaymentinvoice> TransportPaymentinvoice { get; set; }
        public List<Bankchequetransaction> bankcheques { get; set; }
        public Paymentmode paymentmode { get; set; }
        public enum Paymentmode
        {
            InterBankTransfer,
            Cheque,
            Cash,
            MultipleCheque,
            PDC,
            Pending,
            Adjustment
        }

        public PaymentInfo paymentinfo { get; set; }
        public decimal invoicetotal { get;  set; }
        public decimal invoicediscount { get;  set; }
        public decimal taxableamount
        {
            get
            {
                return paidamount - taxamount;
            }
        }
        public decimal taxamount
        {
            get
            {
                if (Transportpaymentinvoice != null && Transportpaymentinvoice.Count > 0)
                    return Transportpaymentinvoice.Sum(e => e.taxamount);
                else
                    return 0;
            }
        }
        public decimal cgsttaxamount
        {
            get
            {
                return taxamount > 0 ? taxamount / 2 : 0;
            }
        }
        public decimal sgsttaxamount
        {
            get
            {
                return taxamount > 0 ? taxamount / 2 : 0;
            }
        }
        public class PaymentInfo
        {
            public string bankname { get; set; }
            public string bankbranch { get; set; }
            public string bankacno { get; set; }
            public string bankacname { get; set; }
            public string bankifsccode { get; set; }
        }
    }

    public class PendingTransportInvoice : BaseEntity
    {
        public string lrreceiptno { get; set; }
        public string supplier { get; set; }
        public string fromlocation { get; set; }
        public string transportname { get; set; }
        public long lrid { get; set; }
        public string lrno { get; set; }
        public string invoiceno { get; set; }
        public string lrentryno { get; set; }
        public DateTime lrdate { get; set; }
        public DateTime receivedon { get; set; }
        public DateTime entrydate { get; set; }
        public DateTime invoicedate { get; set; }
        public decimal actualweight { get; set; }
        public decimal chargedweight { get; set; }
        public decimal frightcharge { get; set; }
        public decimal loadingcharge { get; set; }
        public decimal othercharges { get; set; }
        public decimal noofbundelsreceived { get; set; }
        public decimal noofboxesreceived { get; set; }
        public decimal totalamount { get; set; }
        public decimal paidamount { get; set; }
        public decimal balanceamount { get; set; }
        public decimal taxamount { get; set; }
        public string days { get; set; }
        public string city { get; set; }
        public string source { get; set; }
    }

    public class TransportPaymentCreteria : TransportPayment
    {
        public DateTime fromdate { get; set; }
        public DateTime todate { get; set; }
        public string ageranges { get; set; }
        public DateTime invfromdate { get; set; }
        public DateTime invtodate { get; set; }
        public DateTime entryfromdate { get; set; }
        public DateTime entrytodate { get; set; }
        public long agentid { get; set; }
        public bool includebalance { get; set; }
        public bool groupbytransport { get; set; }
    }




    public class TransportPaymentPrint : BaseEntity
    {
        public decimal BillDiscount { get; set; }
        public int paymentmode { get; set; }
        public DateTime paymentdate { get; set; }
        public Transport transport { get; set; }
        public string receiptno { get; set; }
        public Company company { get; set; }
        public List<Bankchequetransaction> cheques { get; set; }
        [JsonIgnore]
        public List<Transportpaymentinvoicedetail> Bills { get; set; }
        [JsonIgnore]
        public List<Transportpaymentinvoicedetail> Returns { get; set; }
        [JsonIgnore]
        public decimal BillPaid { get; set; }
        [JsonIgnore]
        public decimal ReturnAdjusted { get; set; }
        public decimal ChequeTotal
        {
            get
            {
                return cheques == null ? 0 : cheques.Sum(e => e.amount);
            }
        }
        public string NoInWord
        {
            get
            {
                return WebAPI.Data.Numberconvertor.rupees(ChequeTotal);
            }
        }
        public string paymentmodedisplay
        {
            get
            {
                if (paymentmode == 0)
                    return "NEFT/RTGS";
                else if (paymentmode == 1)
                    return "Cheque/DD";
                else if (paymentmode == 2)
                    return "Cash";
                else if (paymentmode == 3)
                    return "Multiple Cheque/DD";
                else if (paymentmode == 4)
                    return "PDC";
                else
                    return "";
            }
        }
        public List<Transportpaymentinvoicedetail> invoicelist { get; set; }
        public decimal rounding { get; set; }
        public string createdby { get; set; }
        public bool hastds { get; set; }
        public decimal tdspercentage { get; set; }
        public decimal tdsamount { get; set; }
        public int sourcemode { get; set; }
        public string remarks { get; set; }
        public decimal taxableamount { get; set; }
        public decimal taxamount { get; set; }
        public decimal cgsttaxamount { get; set; }
        public decimal sgsttaxamount { get; set; }
    }
}


