using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class Customerpayment : BaseEntity
    {
        public long id { get; set; }
        public bool isactive { get; set; }
        public long createdby { get; set; }
        public long modifiedby { get; set; }
        public int version { get; set; }
        public int sourcetype { get; set; }
        public DateTime createdon { get; set; }
        public DateTime modifiedon { get; set; }
        public long customerid { get; set; }
        public string customername { get; set; }
        public string remarks { get; set; }
        public DateTime paidon { get; set; }
        public long receivedby { get; set; }
        public string receivedbyname { get; set; }
        public long companyid { get; set; }
        public long counterid { get; set; }
        public long locationid { get; set; }
        public long orderid { get; set; }
        public long ledgerid { get; set; }
        public string orderno { get; set; }
        public decimal amount { get; set; }
        public decimal discount { get; set; }
        public decimal rounding { get; set; }
        public string amountinwords
        {
            get
            {
                return WebAPI.Data.Numberconvertor.rupees(amount);
            }
        }
        public bool HasDisountOnly
        {
            get
            {
                return amount == 0 && tdsamount == 0 && discount > 0;
            }
        }
        public string receiptno { get; set; }
        public int collectedat { get; set; }
        public bool hastds { get; set; }
        public decimal tdspercentage { get; set; }
        public decimal tdsamount { get; set; }
        public string printdocument { get; set; }
        [JsonIgnore]
        public Company company { get; set; }
        [JsonIgnore]
        public Customer customer { get; set; }
        [JsonIgnore]
        public Referencelist location { get; set; }
        public string paymentmode { get; set; }
        public List<Customerinvoice> Customerinvoice { get; set; }
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string paymentinfo_json
        {
            get { return Newtonsoft.Json.JsonConvert.SerializeObject(paymentinfo); }
            set
            {
                if (!string.IsNullOrEmpty(value) && value != "null")
                    paymentinfo = Newtonsoft.Json.JsonConvert.DeserializeObject<Paymentinfo>(value);
            }
        }
        public Paymentinfo paymentinfo { get; set; }
        public class Paymentinfo
        {
            public int paymentmode { get; set; }
            public long bankid { get; set; }
            public DateTime chequedate { get; set; }
            public string referencedetail { get; set; }
            public decimal payableamount { get; set; }
            public decimal balanceamount
            {
                get
                {
                    return payableamount - (paidamount + discount_paid);
                }
            }
            public decimal paidamount { get; set; }
            public string remarks { get; set; }
            public string mode
            {
                get
                {
                    string smode = "";
                    //if (cash_paid > 0)
                    //    smode = "CASH";
                    //if (card_paid > 0)
                    //    smode += (smode.Length > 0 ? "," : "") + "CARD";
                    //if (voucher_paid > 0)
                    //    smode += (smode.Length > 0 ? "," : "") + "VOUCHER";
                    //if (credit_paid > 0)
                    //    smode += (smode.Length > 0 ? "," : "") + "CREDIT";
                    //if (discount_paid > 0)
                    //    smode += (smode.Length > 0 ? "," : "") + "DISCNT";
                    //if (cash_return < 0)
                    //    smode += (smode.Length > 0 ? "," : "") + "RETURN";
                    //if (smode.Length == 0 && adjustment_paid > 0)
                    //    smode += "ADJUSTMENT";
                    if (paymentmode == 0)
                        smode = "Card";
                    if (paymentmode == 1)
                        smode = "Voucher";
                    if (paymentmode == 2)
                        smode = "Cash";
                    if (paymentmode == 3)
                        smode = "UPI";
                    if (paymentmode == 4)
                        smode = "Cheque/DD";
                    if (paymentmode == 5)
                        smode = "NEFT";
                    return smode;
                }
            }
            public decimal upi_paid { get; set; }
            public decimal cash_paid { get; set; }
            public string cash_dinamination { get; set; }
            public string return_dinamination { get; set; }
            public decimal cash_return { get; set; }
            public decimal cash_refund { get; set; }
            public decimal card_paid { get; set; }
            public string card_no { get; set; }
            public long credit_approvedBy { get; set; }
            public long card_typeid { get; set; }
            public long upiproviderid { get; set; }
            public decimal credit_paid { get; set; }
            public bool credit_partly { get; set; }
            public long card_bankid { get; set; }
            public long card_machineid { get; set; }
            public string credit_info { get; set; }
            public string card_info { get; set; }
            public decimal voucher_paid { get; set; }
            public decimal discount_paid { get; set; }
            public long discount_approvedBy { get; set; }
            public string discount_info { get; set; }
            public decimal debt_paid { get; set; }
            public decimal loyalty_paid { get; set; }
            public decimal adjustment_paid { get; set; }
            public Decimal loan_paid { get; set; }
            public long loanproviderid { get; set; }
            public string loan_reference { get; set; }
            //public string voucher_reference { get; set; }
            public bool HasLoan
            {
                get
                {
                    return loan_paid > 0;
                }
            }

            public bool HasVoucher
            {
                get
                {
                    return voucher_paid > 0;
                }
            }
            public bool HasRefund
            {
                get
                {
                    return cash_refund > 0;
                }
            }
            public bool HasReturn
            {
                get
                {
                    return cash_return > 0;
                }
            }
            public bool HasAdjustment
            {
                get
                {
                    return adjustment_paid > 0;
                }
            }
            public bool HasCredit
            {
                get
                {
                    return credit_paid > 0;
                }
            }
            public bool HasDiscount
            {
                get
                {
                    return discount_paid > 0;
                }
            }
            public bool HasCash
            {
                get
                {
                    return cash_paid > 0;
                }
            }
            public bool HasCard
            {
                get
                {
                    return card_paid > 0;
                }
            }
            public decimal discount_adjusted { get; set; }

            public decimal couponvalue { get; set; }
            public bool HasReward
            {
                get
                {
                    return rewardpoint > 0 || avlrewardpoint > 0;
                }
            }
            public int rewardpoint { get; set; }
            public decimal avlrewardpoint { get; set; }

        }
        public string billnos { get; set; }
    }
    public class CustomerpaymentCreteria : Customerpayment
    {
        public DateTime fromdate { get; set; }
        public DateTime todate { get; set; }
        public bool showbills { get; set; }
    }
}
