using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class GiftVoucher : BaseEntity
    {
        public long id { get; set; }
        public long createdby { get; set; }
        public long modifiedby { get; set; }
        public DateTime createdon { get; set; }
        public DateTime modifiedon { get; set; }
        public bool isactive { get; set; }
        public int version { get; set; }
        public string name { get; set; }
        public string mobileno { get; set; }
        /*        $scope.VoucherIssueModes = [{ 'id': 0, 'name': 'Direct' }, { 'id': 1, 'name': 'Pre-Printed' },
            { 'id': 2, 'name': 'Issue Printed' }, { 'id': 3, 'name': 'Delete Issue' }, 
        { 'id': 4, 'name': 'Credit Note' },{ 'id': 5, 'name': 'Customer Advance' }];*/
        public enum IssueModes
        {
            Direct,
            PrePrinted,
            Issue,
            Delete,
            CreditNote,
            CustomerAdvance
        }
        public int issuemode { get; set; }
        public decimal discountpercentage { get; set; }
        public decimal discountvalue { get; set; }
        public DateTime validfrom { get; set; }
        public DateTime validto { get; set; }
        public int status { get; set; }
        public long customerid { get; set; }
        public long counterid { get; set; }
        public long billsettlementmasterid { get; set; }
        public decimal receivedamt { get; set; }
        public decimal vouchervalue { get; set; }
        public long issuedby { get; set; }
        public DateTime issuedon { get; set; }
        public DateTime printedon { get; set; }
        public string errormessage { get; set; }
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string denomination_json
        {
            get { return Newtonsoft.Json.JsonConvert.SerializeObject(denomination); }
            set
            {
                if (!string.IsNullOrEmpty(value))
                    denomination = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Denomination>>(value);
                else
                    denomination = new List<Denomination>();
            }
        }

        public List<Denomination> denomination { get; set; }


        public class Denomination
        {
            public int denomination { get; set; }
            public int qty { get; set; }
            public decimal amount { get; set; }
        }
        public string vouchercode { get; set; }
        public long receivedby { get; set; }
        public string givento { get; set; }
        public string seqcode { get; set; }
        public decimal discount { get;  set; }
        public long bankid { get; set; }
        public long ledgerid { get; set; }
        public long locationid { get; set; }
        public long companyid { get; set; }
        public int paymentmode { get; set; }
        public string paymentreference { get; set; }
        public List<CustomerCoupons> coupons { get; set; }
    }

    public class GiftVoucherIssue : BaseEntity
    {
        public long customerid { get; set; }
        public long counterid { get; set; }
        public decimal receivedamt { get; set; }
        public decimal vouchervalue { get; set; }
        public long issuedby { get; set; }
        public string errormessage { get; set; }
        public string vouchercode { get; set; }
        public long voucherid { get; set; }
        public long locationid { get; set; }
        public long ledgerid { get; set; }
        public long bankid { get; set; }
        public int paymentmode { get; set; }
        public string paymentreference { get; set; }
    }
    public class GiftVoucherCreteria : GiftVoucher
    {
        public string code { get; set; }
        public DateTime billfromdate { get; set; }
        public DateTime billtodate { get; set; }
        public DateTime issuedfrom { get; set; }
        public DateTime issuedto { get; set; }
        public int vouchertypeid { get; set; } 
        public bool iscoupononly { get; set; }
    }
    public class SchemeCouponList : BaseEntity
    {
        public long id { get; set; }
        public string name { get; set; }
    }

    public class SearchVoucherResult
    {
        public string name { get; set; }
        public string mobileno { get; set; }
        public string issuetono { get; set; }
        public string issuecountername { get; set; }
        public string issuelocationname { get; set; }
        public string settlementcode { get; set; }
        public DateTime settlementon { get; set; }
        public string settlementcountername { get; set; }
        public string vouchercode { get; set; }
        public decimal vouchervalue { get; set; }
        public string location { get; set; }
        public decimal unclaim { get;  set; }
        public bool isvoucher { get; set; }
        public decimal claim { get; set; }
        public DateTime validfrom { get; set; }
        public DateTime validto { get; set; }
        public DateTime issuedon { get; set; }
        public long issueto { get; set; }
        public string givento { get; set; }
        public string seqcode { get; set; }
        public string vouchertypename { get; set; }
        public decimal totalvouchervalue { get; set; }
        public string customername { get; set; }
        public string customeraddress { get; set; }
        public long voucherid { get; set; }
        public List<long> voucherids { get; set; }
        public long settlementmasterid { get; internal set; }
    }

    public class SearchCouponResult
    {
        public string name { get; set; }
        public string customername { get; set; }
        public string mobileno { get; set; }
        public string issuetocustomername { get; set; }
        public string issuetomobileno { get; set; }
        public string issuelocation { get; set; }
        public string issuecounter { get; set; }
        public DateTime issuedon { get; set; }
        public string billno { get; set; }
        public DateTime billdate { get; set; }
        public decimal billvalue { get; set; }
        public string vouchercode { get; set; }
        public decimal vouchervalue { get; set; }
        public bool ispercentage { get; set; }
        public decimal discount { get; set; }
        public DateTime validfrom { get; set; }
        public DateTime validto { get; set; }
        public string vouchertype { get; set; }
        public string countername { get; set; }
        public string issuedbyname { get; set; }
        public string schemename { get; set; }
    }

    public class VoucherPrintCreteria : BaseEntity
    {
        public long couponid { get; set; }
        public int copy { get; set; }
        public long reportid { get; set; }
        public List<long> items { get; set; }
        public string companytitle { get; set; }
        public string printdocument { get; set; }
        public long billsettlementmasterid { get; set; }
    }

    public class Voucher2Print : BaseEntity, Report.IBarcodeFormat
    {
        public bool SpecialField(string field, SortedDictionary<string, Object> data)
        {
            return false;
        }
        public string code { get; set; }
        public string id { get; set; }
        public string value { get; set; }
        public string name { get; set; }
        public DateTime  validfrom { get; set; }
        public DateTime validto { get; set; }
        public string ncode
        {
            get
            {
                int numcode = 0;
                int.TryParse(code, out numcode);
                return numcode.ToString("N0");
            }
        }

        public SortedDictionary<string, object> KeyValue
        {
            get
            {
                SortedDictionary<string, object> ret = new SortedDictionary<string, object>();
                ret.Add("CODE", code);
                ret.Add("NCODE", ncode);
                ret.Add("ID", id);
                ret.Add("NAME", name);
                ret.Add("VALUE", value);
                ret.Add("VALIDFROM", validfrom);
                ret.Add("VALIDTO", validto);
                return ret;
            }

        }

        public SortedDictionary<string, string> KeyFormat
        {
            get
            {
                SortedDictionary<string, string> ret = new SortedDictionary<string, string>();
                ret.Add("ID", "N0");
                ret.Add("CODE", "N0");
                ret.Add("VALIDFROM", "dd-MM-yyyy");
                ret.Add("VALIDTO", "dd-MM-yyyy");
                return ret;
            }
        }


    }


    public class SchemeCoupon : SchemeCouponList
    {
        public string givento { get; set; }
        public string seqcode { get; set; }
        public long createdby { get; set; }
        public long modifiedby { get; set; }
        public DateTime createdon { get; set; }
        public DateTime modifiedon { get; set; }
        public bool isactive { get; set; }
        public int version { get; set; }
        public int issuemode { get; set; }
        public decimal discountpercentage { get; set; }
        public decimal discountvalue { get; set; }
        public DateTime validfrom { get; set; }
        public DateTime validto { get; set; }
        public long issuedby { get; set; }
        public int paymentmode { get; set; }
        public long bankid { get; set; }
        public long ledgerid { get; set; }
        public long locationid { get; set; }
        public long companyid { get; set; }
        public long counterid { get; set; }
        public DateTime printedon { get; set; }
        public DateTime issuedon { get; set; }
        public string paymentreference { get; set; }

        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string rules_json
        {
            get { return Newtonsoft.Json.JsonConvert.SerializeObject(rules); }
            set
            {
                rules = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Rule>>(value);
            }
        }
        public List<Rule> rules { get; set; }
        public string errormessage { get;  set; }

        public class Rule
        {
            public long acceptablelocationid { get; set; }
            public decimal minbillvalue { get; set; }
            public decimal maxbilldiscountpercentage { get; set; }
            public bool amountaspercentage { get; set; }
            public decimal amount { get; set; }
            public int quantity { get; set; }
            public bool unlimitted { get; set; }
            public List<long> couponids { get; set; }
            public long id { get; set; }
            public bool ex_discount { get; set; }
            public bool ex_adiscount { get; set; }
            public bool ex_sdiscount { get; set; }
            public bool otprequired { get; set; }
            public bool isranged { get; set; }
            public string customermobileno { get; set; }
            public string customername { get; set; }
            public long otpcustomerid { get; set; }
            public List<CustomerCoupons.RangeRule> rangedrule { get; set; }
            [JsonIgnore]
            [JsonProperty(Required = Required.Default)]
            public string rangedrule_json
            {
                get { return Newtonsoft.Json.JsonConvert.SerializeObject(rangedrule); }
                set
                {
                    rangedrule = Newtonsoft.Json.JsonConvert.DeserializeObject<List<CustomerCoupons.RangeRule>>(value);
                }
            }
        }
    }
}
