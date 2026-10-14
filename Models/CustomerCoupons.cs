using InfyPOS.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
namespace InfyPOS.Models
{
    public class CustomerCoupons : BaseEntity
    {
        public string mobileno { get; set; }
        public long id { get; set; }
        public bool isactive { get; set; }
        public bool amountaspercentage { get; set; }
        public long createdby { get; set; }
        public long modifiedby { get; set; }
        public int version { get; set; }
        public DateTime createdon { get; set; }
        public DateTime modifiedon { get; set; }
        public long issuedby { get; set; }
        public DateTime issuedon { get; set; }
        public DateTime validfrom { get; set; }
        public DateTime validupto { get; set; }
        public decimal amount { get; set; }
        public int status { get; set; }
        public string code { get; set; }
        public string name { get; set; }
        public string uniquekey { get; set; }
        public long issueto { get; set; }
        public long adjustedrewards { get; set; }
        public long billid { get; set; }
        public bool unlimited { get; set; }
        public long coupenid { get; set; }
        public int type { get; set; }
        public decimal minbillvalue { get; set; }
        public bool ex_discount { get; set; }
        public bool ex_adiscount { get; set; }
        public bool ex_sdiscount { get; set; }
        public bool ex_coupon { get; set; }
        public long acceptablelocationid { get; set; }
        public decimal maxbilldiscountpercentage { get; set; }
        public CouponType vouchertype
        {
            get
            {
                return (CouponType)type;
            }
            set
            {
                type = (int)value;
            }
        }

        public long filterschemeid { get;  set; }

        public enum CouponType
        {
            Adjust = 0,
            GiftVoucher = 1,
            Coupon = 2,
            Return = 3,
            GiftClaim = 4,
            BirthdayCoupon = 5,
            Inbound = 6,
            CreditNote = 7,
            CustomerAdvance = 8
        }

        public long locationid { get; set; }
        public long counterid { get; set; }
        public decimal discountvalue { get; set; }
        public decimal discountpercentage { get; set; }
        public string customername { get; set; }
        public string employeename { get; set; }
        public bool isranged { get; set; }
        public List<RangeRule> rangedrule { get; set; }
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string rangedrule_json
        {
            get { return Newtonsoft.Json.JsonConvert.SerializeObject(rangedrule); }
            set
            {
                rangedrule = Newtonsoft.Json.JsonConvert.DeserializeObject<List<RangeRule>>(value);
            }
        }

        [JsonIgnore]
        public CompanyLocation location { get; set; }
        [JsonIgnore]
        public string issuedemployee { get; set; }
        [JsonIgnore]
        public Customer claimdetail { get; set; }
        [JsonIgnore]
        public Customer customer { get; set; }
        [JsonIgnore]		
        public long sourcebillsettlementmasterid { get;  set; }
        public bool otp { get;  set; }
        public long otpcustomerid { get;  set; }
        public List<string> eligiblebarcodes { get;  set; }
        public bool hasfilterscheme { get;  set; }

        public class RangeRule
        {
            public decimal fromamount { get; set; }
            public decimal toamount { get; set; }
            public bool amountaspercentage { get; set; }
            public decimal amount { get; set; }
        }

    }
    public class CustomerCouponsCreteria : CustomerCoupons
    {
        public Bill bill { get; set; }
        public decimal billvalue { get; set; }
        public bool validate { get; set; }
        public List<long> ids { get;  set; }
    }
    
    public class CustomerCouponsStatus : BaseEntity
    {
        public decimal available { get; set; }
        public decimal convertable { get; set; }
        public string reason { get; set; }
        public long couponid { get; set; }
        public List<string> eligiblebarcodes { get;  set; }
        public long otpcustomerid { get;  set; }
        public bool hasfilterscheme { get; internal set; }
    }
}
