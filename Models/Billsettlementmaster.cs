using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class Billsettlementmaster : BaseEntity
    {
        public long intvendor { get; set; }
        public string intmode { get; set; }
        public long companyid { get; set; }
        public string printdocument { get; set; }
        public List<BillInfo> discountedbills { get; set; }
        public long locationid { get; set; }
        public long id { get; set; }
        public bool isactive { get; set; }
        public long createdby { get; set; }
        public long modifiedby { get; set; }
        public int version { get; set; }
        [JsonIgnore]
        public bool IsSynchronize { get; set; }
        [JsonIgnore]
        public Company company { get; set; }
        public DateTime createdon { get; set; }
        public DateTime modifiedon { get; set; }
        public int mode { get; set; }
        public int papersize { get; set; }
        public long printtemplateid { get; set; }
        public decimal billadditionaldiscount { get; set; }
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
        public bool hasgift
        {
            get
            {
                return settlementgift != null && settlementgift.Count > 0;
            }
        }
        [JsonIgnore]
        public List<BillsettlementGift> settlementgift { get; set; }
        public List<BillDeliveryMasterInfo> deliverylist { get; set; }
        [JsonIgnore]
        public BillsettlementGift singlesettlementgift
        {
            get
            {
                if (hasgift) return settlementgift.First();
                return null;
            }
        }
        public Paymentinfo paymentinfo { get; set; }
        public long loanproviderid { get; set; }
        public string loanprovider { get; set; }
        public decimal loan_paid { get; set; }
        public string loan_reference { get; set; }
        public long swipbankid { get; set; }
        public long swipmachineid { get; set; }
        public long customerid { get; set; }
        public decimal paidamount { get; set; }
        public decimal billamount { get; set; }
        public decimal discount { get; set; }
        public decimal tax { get; set; }
        public decimal returnbillvalue { get; set; }
        public decimal paid_refund { get; set; }
        public decimal paid_discount { get; set; }
        public decimal creditamount { get; set; }
        public decimal receivable { get; set; }
        public decimal receivableafterdiscount
        {
            get
            {
                return receivable - (paymentinfo != null ? paymentinfo.discount_paid : 0);
            }
        }
        public string code { get; set; }
        public string billmastercode { get; set; }
        public string customername { get; set; }
        public string customerno { get; set; }
        public string customermobileno { get; set; }
        public DateTime settlementon { get; set; }
        public long swipingmachineid { get; set; }
        public decimal additionaldiscount { get; set; }
        public List<Billsettlement> Billsettlement { get; set; }
        public string cashiername { get; set; }
        public string countername { get; set; }
        public long casherid { get; set; }
        public long counterid { get; set; }
        public string billnos { get; set; }
        public string reason { get; set; }
        public decimal rewardunit { get; set; }
        public string floorname { get; set; }
        public long floorid { get; set; }
        [JsonIgnore]
        public bool HasFreeGift
        {
            get
            {
                return FreeItems != null && FreeItems.Count > 0;
            }
        }
        [JsonIgnore]
        public List<BillItemAttributes.FreeItems> FreeItems { get; set; }

        [JsonIgnore]
        public DateTime customerdob { get; set; }
        [JsonIgnore]
        public bool backwardserialno { get; set; }
        public bool directpdf { get; set; }
        public bool directdos { get; set; }
        public List<voucherinfo> coupons
        {
            get
            {
                if (paymentinfo != null && paymentinfo.HasCoupen && paymentinfo.coupons.Count>0)
                {
                    if(paymentinfo.coupons.All(x=>x.id>0) && paymentinfo.coupons.GroupBy(x => x.id).ToList().Exists(y=>y.Count()>1))
                    {
                        var oldlist = paymentinfo.coupons;
                        var newcoupons = new List<voucherinfo>();
                        foreach (var item in oldlist.GroupBy(x => x.id))
                        {
                            newcoupons.Add(item.First());
                        }
                        paymentinfo.coupons = newcoupons;
                    }
                    return paymentinfo.coupons;
                }
                return null;
            }
        }
        public bool hasCoupen
        {
            get
            {
                if (paymentinfo != null)
                    return paymentinfo.HasCoupen;
                return false;
            }
        }
        public List<voucherinfo> creditnote
        {
            get
            {
                if (paymentinfo != null && paymentinfo.HasCreditNote)
                    return paymentinfo.voucherinformation.FindAll(e => e.type == 7);
                return null;
            }
        }
        public bool hasCreditNote
        {
            get
            {
                if (paymentinfo != null)
                    return paymentinfo.HasCreditNote;
                return false;
            }
        }
        public bool hasDiscountAdjusted
        {
            get
            {
                if (paymentinfo != null)
                    return (paymentinfo.discount_adjusted > 0);
                return false;
            }
        }
        public class Paymentinfo
        {
            public string mode
            {
                get
                {
                    string smode = "";
                    if (cash_paid > 0)
                        smode = "CASH";
                    if (card_paid > 0)
                        smode += (smode.Length > 0 ? "," : "") + "CARD";
                    if (voucher_paid > 0)
                        smode += (smode.Length > 0 ? "," : "") + "VOUCHER";
                    if (credit_paid > 0)
                        smode += (smode.Length > 0 ? "," : "") + "CREDIT";
                    if (upi_paid > 0)
                        smode += (smode.Length > 0 ? "," : "") + "UPI";
                    if (discount_paid > 0)
                        smode += (smode.Length > 0 ? "," : "") + "DISCNT";
                    if (cash_return < 0)
                        smode += (smode.Length > 0 ? "," : "") + "RETURN";
                    if (smode.Length == 0 && adjustment_paid > 0)
                        smode += "ADJUSTMENT";

                    return smode;
                }

                set
                {
                }
            }

            public bool autosettlement { get; set; }
            public decimal cash_paid { get; set; }
            public string cash_dinamination { get; set; }
            public string return_dinamination { get; set; }
            public decimal cash_return { get; set; }
            public decimal cash_refund { get; set; }
            public decimal card_paid { get; set; }
            public string card_no { get; set; }
            public long credit_approvedBy { get; set; }
            public long card_typeid { get; set; }
            public decimal credit_paid { get; set; }
            public bool credit_partly { get; set; }
            public long card_bankid { get; set; }
            public long card_machineid { get; set; }
            public string credit_info { get; set; }
            public string card_info { get; set; }
            public decimal voucher_paid { get; set; }
            public decimal discount_paid { get; set; }
            public long discount_approvedBy { get; set; }
            public string discount_approvedbyname { get; set; }
            public string discount_info { get; set; }
            public decimal debt_paid { get; set; }
            public decimal loyalty_paid { get; set; }
            public decimal adjustment_paid { get; set; }
            public List<voucherinfo> voucherinformation { get; set; }
            public Decimal loan_paid { get; set; }
            public decimal upi_paid { get; set; }
            public long upiproviderid { get; set; }
            public decimal creditnote_refund { get; set; }
            public bool HasUPI
            {
                get
                {
                    return upi_paid > 0 && !HasCheque;
                }
            }
            public bool HasCheque
            {
                get
                {
                    return !string.IsNullOrEmpty(upiprovidername) && upiprovidername.ToUpper() == "CHEQUE";
                }
            }
            public long loanproviderid { get; set; }
            public string loanprovidername { get; set; }
            [JsonIgnore]
            public string card_bankname { get; set; }
            [JsonIgnore]
            public string upiprovidername { get; set; }
            public string loan_reference { get; set; }
            public string remarks { get; set; }
            public string voucher_reference { get; set; }
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
            public bool HasCreditNote
            {
                get
                {
                    return creditnote_refund > 0;
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
                    return !HasRefund && cash_return > 0;
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
            public List<voucherinfo> coupons { get; set; }
            public decimal couponvalue { get; set; }

            public bool HasCoupen
            {
                get
                {
                    return (coupons != null && coupons.Count > 0) || couponvalue > 0;
                }
            }
            public string couponcode
            {
                get
                {
                    if (!HasCoupen) return "";
                    return coupons.OrderByDescending(e => e.amount).First().code;
                }
            }
            public DateTime couponvalidupto
            {
                get
                {
                    if (!HasCoupen) return DateTime.MinValue;
                    return coupons.OrderByDescending(e => e.amount).First().validupto;
                }
            }
            public decimal couponminbillvalue
            {
                get
                {
                    if (!HasCoupen) return 0;
                    return coupons.OrderByDescending(e => e.amount).First().minbillvalue;
                }
            }
            public decimal couponmaxbilldiscountpercentage
            {
                get
                {
                    if (!HasCoupen) return 0;
                    return coupons.OrderByDescending(e => e.amount).First().maxbilldiscountpercentage;
                }
            }
            public bool couponamountaspercentage
            {
                get
                {
                    if (!HasCoupen) return false;
                    return coupons.OrderByDescending(e => e.amount).First().amountaspercentage;

                }
            }

            public string referral { get; set; }
            public long referralemployeeid { get; set; }
            public long referralagentid { get; set; }
            public bool HasReward
            {
                get
                {
                    return rewardpoint > 0 || avlrewardpoint > 0;
                }
            }
            public decimal rewardpoint { get; set; }
            public decimal avlrewardpoint { get; set; }
            public string paymentuniqueid { get; set; }
            public PaymentDeviceResult deviceresult { get; set; }
            public FreeProducts freeproducts { get; set; }
            public bool hasfreeproducts
            {
                get
                {
                    return freeproducts != null;
                }
            }

            [JsonIgnore]
            public decimal reward_amount { get; set; }
            [JsonIgnore]
            public decimal reward_spent { get; set; }
            [JsonIgnore]
            public decimal reward_earned { get; set; }
        }
        public class FreeProducts
        {
            public Item Selected { get; set; }
            public List<Item> Available { get; set; }
            public class Item
            {
                public decimal from { get; set; }
                public decimal to { get; set; }
                public string product { get; set; }
                public string uancode { get; set; }
                public long productid { get; set; }
            }
        }

        public class PaymentDeviceResult
        {
            public bool Manual { get; set; }
            public string FullResponse { get; set; }
            public string Request_Input { get; set; }
            public string ResponseCode { get; set; }
            public string ResponseMessage { get; set; }
            public string ApprovalCode { get; set; }
            public string RRN_No { get; set; }
            public string Amount { get; set; }
            public string Card_Num { get; set; }
            public string Card_Type { get; set; }
            public string CardHolder_Name { get; set; }
            public string Acquirer_Bank { get; set; }
            public DateTime Txn_Date { get; set; }
            public string Txn_Type { get; set; }
            public string Txn_ID { get; set; }
            public string BankInvoice_Num { get; set; }
            public string Batch_Number { get; set; }
            public string Terminal_Id { get; set; }
            public string Merchant_Id { get; set; }
            public object errorMsg { get; set; }
            public object errorCode { get; set; }
        }
        public class BillInfo
        {
            public long billid { get; set; }
            public string billno { get; set; }
            public string printdocument { get; set; }
            [JsonIgnore]
            public Bill source { get; set; }
            public bool directpdf { get; set; }
            public bool directdos { get; set; }
        }
        [JsonIgnore]
        public Customer customer { get; set; }
        [JsonIgnore]
        public bool Lottery_Avilable { get; set; }
        [JsonIgnore]
        public int Lottery_Count { get; set; }
        [JsonIgnore]
        public Billsettlementmaster Lottery_Bill
        {
            get
            {
                return this;
            }
        }
        [JsonIgnore]
        public List<Billsettlementmaster> Lottery_Bills
        {
            get
            {
                List<Billsettlementmaster> bills = new List<Billsettlementmaster>();
                for (var i = 0; i < Lottery_Count; i++)
                    bills.Add(this);
                return bills;
            }
        }
        public class voucherinfo
        {
            public long id { get; set; }
            public string code { get; set; }
            public int type { get; set; }
            public decimal amount { get; set; }
            public long locationid { get; set; }
            public DateTime validupto { get; set; }
            public decimal minbillvalue { get; set; }
            public decimal maxbilldiscountpercentage { get; set; }
            public bool amountaspercentage { get; set; }
            public decimal available { get; set; }
            public string schemename { get;  set; }
            [JsonIgnore]
            public Customer customer { get; set; }
        }
    }
    public class BillsettlementmasterCreteria : Billsettlementmaster
    {
        public List<long> ids { get; set; }
        public List<long> billids { get; set; }

        public List<long> reconids { get; set; }
        public List<long> reconmids { get; set; }

    }

    public class SettlementPrint : BaseEntity
    {
        public long id { get; set; }
        public string billno { get; set; }
        public string billmasterno { get; set; }
        public bool directdos { get; set; }
    }
}
