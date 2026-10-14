
using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class Supplierpayment : BaseEntity
    {
        public long id { get; set; }
        public long supplierid { get; set; }
        public long agentid { get; set; }
        public string agentname { get; set; }
        public long locationid { get; set; }
        public string receiptno { get; set; }
        public DateTime paymentdate { get; set; }
        public bool datatransmitted { get; set; }
        public decimal paidamount { get; set; }
        public bool isactive { get; set; }
        public long createdby { get; set; }
        public long modifiedby { get; set; }
        public int version { get; set; }
        public DateTime createdon { get; set; }
        public DateTime modifiedon { get; set; }
        public string createduser { get; set; }
        public string modifieduser { get; set; }
        public long suppliercompanyid { get; set; }
        public List<long> suppliercompanyids { get; set; }
        public long companyid { get; set; }
        public string remarks { get; set; }
        public string suppliermailid { get; set; }
        public string invoicenos { get; set; }
        public List<Supplierpaymentinvoice> Supplierpaymentinvoice { get; set; }
        public List<Bankchequetransaction> bankcheques { get; set; }
        public Paymentmode paymentmode { get; set; }
        public long bankid { get; set; }
        public long cityid { get; set; }
        public DateTime instrumentdate { get; set; }
        public string referencedetail { get; set; }
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

        public decimal agntcomm { get; set; }
        public decimal agntpaid { get; set; }
        public decimal rounding { get; set; }
        public decimal invoicetotal { get; set; }
        public decimal invoicediscount { get; set; }
        public decimal debitnotetotal { get; set; }
        public decimal debitnotediscount { get; set; }
        public decimal totaldiscount { get; set; }
        public bool hastds { get; set; }
        public bool tdsoninv { get; set; }
        public decimal tdspercentage { get; set; }
        public decimal tdsamount { get; set; }
        public long tdsledgerid { get; set; }
        public bool isinterstatesale { get; set; }
        public PaymentInfo paymentinfo { get; set; }
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string paymentinfo_json
        {
            get { return Newtonsoft.Json.JsonConvert.SerializeObject(paymentinfo); }
            set
            {
                if (!string.IsNullOrEmpty(value) && value != "null")
                    paymentinfo = Newtonsoft.Json.JsonConvert.DeserializeObject<PaymentInfo>(value);
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
        public string supplierbankname { get; set; }
        public string supplierbankbranch { get; set; }
        public string supplierbankacno { get; set; }
        public string supplierbankacname { get; set; }
        public string supplierbankifsccode { get; set; }
        public bool interbank { get; internal set; }
    }
    public class SupplierpaymentCreteria : Supplierpayment
    {
        public DateTime duefromdate { get; set; }
        public DateTime duetodate { get; set; }
        public bool duedebitnotes { get; set; }
        public DateTime fromdate { get; set; }
        public DateTime todate { get; set; }
        public string ageranges { get; set; }
        public string invno { get; set; }
        public DateTime invfromdate { get; set; }
        public DateTime invtodate { get; set; }
        public DateTime entryfromdate { get; set; }
        public DateTime entrytodate { get; set; }
        public bool showmsme { get; set; }
        public bool includebalance { get; set; }
        public long cityid { get; set; }
        public long sectionid { get; set; }
        public int rowtype { get; set; }
        public List<PendingLrInvoice> pendinginvoices { get; set; }
        public bool showplanned { get; set; }
        public int searchmode { get; set; }
    }


    public class SupplierPaymentPrint : BaseEntity
    {
        public long id { get; set; }
        public long agentid { get; set; }
        public int paymentmode { get; set; }
        public DateTime createdon { get; set; }
        public DateTime paymentdate { get; set; }
        public Supplier supplier { get; set; }
        public Suppliercompany suppliercompany { get; set; }
        public string agentname { get; set; }
        public string receiptno { get; set; }
        public Company company { get; set; }
        public CompanyLocation location { get; set; }
        public bool HasCheque
        {
            get
            {
                return cheques != null && cheques.Count > 0;
            }
        }
        public List<Bankchequetransaction> cheques { get; set; }
        [JsonIgnore]
        public List<Supplierpaymentinvoice> Bills { get; set; }
        [JsonIgnore]
        public List<Supplierpaymentinvoice> Returns { get; set; }
        [JsonIgnore]
        public decimal BillPayable { get; set; }
        [JsonIgnore]
        public decimal BillDiscount { get; set; }
        [JsonIgnore]
        public decimal ReturnAdjusted { get; set; }
        [JsonIgnore]
        public decimal BillPaid { get; set; }
        [JsonIgnore]
        public List<PendingLrInvoice> Balance { get; set; }
        [JsonIgnore]
        public bool hasbalance { get; set; }


        [JsonIgnore]
        public decimal BalanceBillValue { get; set; }
        [JsonIgnore]
        public decimal BalanceDebitnote { get; set; }
        [JsonIgnore]
        public decimal BalancePayable { get; set; }
        [JsonIgnore]
        public decimal PaidAmount { get; set; }
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
        public List<Supplierpaymentinvoice> invoicelist { get; set; }
        public decimal rounding { get; set; }
        public string remarks { get; set; }
        public string createdby { get; set; }
        public bool hasreturns
        {
            get
            {
                return (Returns == null || Returns.Count == 0) ? false : true;
            }
        }
        public bool hasdiscount
        {
            get
            {
                return TotalDiscount != 0;
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

        public decimal BillGross { get; set; }
        public decimal ReturnGross { get; set; }
        public decimal ReturnDiscount { get; set; }
        public decimal TotalDiscount { get; set; }
        public decimal DiscountPercentage { get; set; }
        public bool hastds { get; set; }
        public bool tdsoninv { get; set; }
        public long tdsledgerid { get; set; }
        public decimal tdspercentage { get; set; }
        public decimal tdsamount { get; set; }
        public string url { get; set; }
        [JsonIgnore]
        public SortedDictionary<string,byte[]> attachment { get; set; }
    }

    public class SupplierpaymentReport:BaseData
    {
        public DateTime paymentdate { get; set; }
        public string receiptno { get; set; }
        public string gstno { get; set; }
        public string agentname { get; set; }
        public string invoiceno { get; set; }
        public string suppliername { get; set; }
        public decimal paidamount { get; set; }
        public decimal discountamount { get; set; }
        public decimal agentcommissionvalue { get; set; }

    }
}


