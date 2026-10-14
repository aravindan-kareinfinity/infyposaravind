using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class Customer : BaseEntity
    {
        
        public int activestatus { get; set; }
        [JsonIgnore]
        public ActiveStatus Activestatus
        {
            get
            {
                return (ActiveStatus)activestatus;
            }
            set
            {
                activestatus = (int)value;
            }
        }

        private string FormatAddress(string addr)
        {
            var formatedaddress = addr.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            if (formatedaddress.Length > 1) return addr;

            string line1 = "", line2 = "", line3 = "";
            if (!string.IsNullOrEmpty(addr))
            {
                var addresslist = addr.Split(new string[] { "," }, StringSplitOptions.None);
                if (addresslist.Length > 0)
                {
                    foreach (var str in addresslist)
                    {
                        if (line1.Length < 25) line1 += str.Trim() + ",";
                        else if (line1.Length > 25 && line2.Length < 25) line2 += str.Trim() + ",";
                        else if (line1.Length > 25 && line2.Length > 25 && line3.Length < 25) line3 += str.Trim() + ",";
                    }
                    if (string.IsNullOrEmpty(line3) && string.IsNullOrEmpty(line2))
                        return line1.ToUpper();
                    else if (string.IsNullOrEmpty(line3) && !string.IsNullOrEmpty(line2))
                        return line1.ToUpper() + "\r\n" + line2.ToUpper();
                    else
                        return line1.ToUpper() + "\r\n" + line2.ToUpper() + "\r\n" + line3.ToUpper();
                }
                return string.Empty;
            }
            return string.Empty;

        }
        public long id { get; set; }
        public bool isactive { get; set; }
        public long createdby { get; set; }
        public long modifiedby { get; set; }
        public long preferredtransportid { get; set; }
        public long agentid { get; set; }
        public int version { get; set; }
        public DateTime createdon { get; set; }
        public DateTime modifiedon { get; set; }
        public bool supportcredit { get; set; }
        public decimal creditlimit { get; set; }
        public int srmaxdays { get; set; }
        public string referredby { get; set; }
        public decimal creditdays { get; set; }
        public long creditrightsgivenby { get; set; }
        public string emailid { get; set; }
        public string mobileno { get; set; }
        public string otherno { get; set; }
        public bool likemobileno { get; set; }
        public string shortmobileno { get
            {
                if (!string.IsNullOrEmpty(mobileno) && mobileno.Length >= 10)
                {
                    return mobileno.Substring(0, 6) + "****";
                }
                return "";
            } }
        public string otp { get; set; }
        public int customertype { get; set; }
        public bool newcustomer { get; set; }
        public bool withaddress { get; set; }
        public bool withoutaddress { get; set; }
        public string name { get; set; }
        public DateTime dateofbirth { get; set; }
        public DateTime marriagedate { get; set; }
        public decimal loyelty { get; set; }
        public string gender { get; set; }
        public string area { get; set; }
        public long areaid { get; set; }
        public string citywithpin { get
            {
                if (string.IsNullOrEmpty(pincode)) return cityname;
                else
                    return string.IsNullOrEmpty(cityname) ? pincode : cityname + " - " + pincode ;
            } }
        public string gstno { get; set; }
        public string supplytype { get; set; }
        public int mrpsource { get; set; }
        public bool married { get; set; }
        public bool hasgst { get; set; }
        public string malekids { get; set; }
        public string femalekids { get; set; }
        public string loyeltyno { get; set; }
        public string code { get; set; }
        public long categoryid {get; set; }
        public string categoryname { get; set; }
        public decimal rewardunit { get;  set; }
        public decimal discount { get;  set; }
        private string _address;
        public string address
        {
            get { return _address; }
            set
            {
                _address = value;
                if (!string.IsNullOrEmpty(_address))
                {
                    var str = FormatAddress(_address);
                    var addresslist = str.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                    this.addresslines = new List<AddressLine>();
                    addresslist.ToList().ForEach(e =>
                    {
                        this.addresslines.Add(new Customer.AddressLine()
                        {
                            value = e
                        });
                    });
                }

            }
        }


        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string attachment_json { get { return Newtonsoft.Json.JsonConvert.SerializeObject(attachment); } set { attachment = Newtonsoft.Json.JsonConvert.DeserializeObject<List<slide>>(value); } }

        public List<slide> attachment { get; set; }
        public class slide
        {
            public string typename { get; set; }
            public string title { get; set; }
            public string image { get; set; }
            public string request { get; set; }
        }
        private string _deliveryaddress;
        public List<CouponValue> couponlist { get; set; }
        public class CouponValue
        {
            public decimal claimamount { get; set; }
            public decimal claimpoint { get; set; }
        }
        public string deliveryaddress
        {
            get { return _deliveryaddress; }
            set
            {
                _deliveryaddress = value;
                if (!string.IsNullOrEmpty(_deliveryaddress))
                {
                    var str = FormatAddress(_deliveryaddress);
                    var deliveryaddress = str.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                    this.deliveryaddresslines = new List<AddressLine>();
                    deliveryaddress.ToList().ForEach(e =>
                    {
                        this.deliveryaddresslines.Add(new Customer.AddressLine()
                        {
                            value = e
                        });
                    });
                }

            }
        }
        public bool hasdeliveryaddress
        {
            get
            {
                return !string.IsNullOrEmpty(deliveryaddress) && deliveryaddress.Length > 5;
            }
        }
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string deliveryaddresslist_json
        {
            get { return Newtonsoft.Json.JsonConvert.SerializeObject(deliveryaddresslist); }
            set
            {
                if (value != null)
                    deliveryaddresslist = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Deliveryaddress>>(value);
            }

        }
        public List<Deliveryaddress> deliveryaddresslist { get; set; }
        public class Deliveryaddress
        {
           public string name { get; set; }
            public string address { get; set; }
            public long cityid { get; set; }
        }
        public bool disableloyalty { get; set; }
        public long cityid { get; set; }
        public long districtid { get; set; }
        public long stateid { get; set; }
        public long countryid { get; set; }
        public long portloadingid { get; set; }
        public long portdischargeid { get; set; }
        public string pincode { get; set; }

        public string tan { get; set; }
        public string pan { get; set; }

        public string contactperson { get; set; }
        public string bankaccountname { get; set; }
        public string bankaccountno { get; set; }
        public string bankifci { get; set; }
        public bool isinterstatesale { get; set; }
        public long locationid { get; set; }
        public long registredlocationid { get; set; }
        public List<CustomerBills> customerbill { get; set; }
        public string cityname { get;  set; }
        public string statename { get;  set; }
        public string districtname { get; set; }
        public bool billdiscountinclude { get;  set; }
        public bool discountotp { get;  set; }
        public List<AddressLine> addresslines { get; set; }
        public List<AddressLine> deliveryaddresslines { get; set; }
        public string deliveryaddressline1
        {
            get
            {
                if (deliveryaddresslines != null && deliveryaddresslines.Count > 0)
                    return deliveryaddresslines[0].value;
                else
                    return "";
            }
        }
        public string deliveryaddressline2
        {
            get
            {
                if (deliveryaddresslines != null && deliveryaddresslines.Count > 1)
                    return deliveryaddresslines[1].value;
                else
                    return "";
            }
        }
        public string deliveryaddressline3
        {
            get
            {
                if (deliveryaddresslines != null && deliveryaddresslines.Count > 2)
                    return deliveryaddresslines[2].value;
                else
                    return "";
            }
        }
        public string deliveryaddressline4
        {
            get
            {
                if (deliveryaddresslines != null && deliveryaddresslines.Count > 3)
                    return deliveryaddresslines[3].value;
                else
                    return "";
            }
        }
        public class AddressLine
        {
            public string value { get; set; }
        }
        public override string GetMobileNo()
        {
            return mobileno;
        }
        public override string GetEmailID()
        {
            return emailid;
        }
        public string addressline1
        {
            get
            {
                if (addresslines != null && addresslines.Count > 0)
                    return addresslines[0].value;
                else
                    return "";
            }
        }
        public string addressline2
        {
            get
            {
                if (addresslines != null && addresslines.Count > 1)
                    return addresslines[1].value;
                else
                    return "";
            }
        }
        public string addressline3
        {
            get
            {
                if (addresslines != null && addresslines.Count > 2)
                    return addresslines[2].value;
                else
                    return "";
            }
        }
        public string addressline4
        {
            get
            {
                if (addresslines != null && addresslines.Count > 3)
                    return addresslines[3].value;
                else
                    return "";
            }
        }
        public long customergroupid { get; set; }
        public int pricesource { get; set; }
        public decimal ccmargin { get;  set; }
        public decimal ccmarginmrp { get; set; }
        public decimal ccmarginprice { get; set; }
        public decimal claimamount { get; set; }
        public decimal claimpoint { get; set; }
        [JsonIgnore]
        public bool ccmarginadjusttax { get; set; }
        public int cccomputemode { get; set; }
        public bool ccmarginmode { get;  set; }
        public string billingname { get; set; }
        public string countryname { get; set; }
        public string loadingname { get; set; }
        public string dischargename { get; set; }
        public long religionid { get; set; }
        public string ErrorMessage { get; set; }
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string family_json
        {
            get { return Newtonsoft.Json.JsonConvert.SerializeObject(family); }
            set
            {
                if (value != null)
                    family = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Family>>(value);
            }

        }
        public List<Family> family { get; set; }
        public decimal creditlimit_pending { get;  set; }
        public decimal creditlimit_delay { get;  set; }
        public bool creditlimit_disable { get; set; }
        [JsonIgnore]
        public Referencelist state { get;  set; }
        public long howoftenid { get; set; }
        public long regsourceid { get; set; }
        public DateTime spousedob { get;set; }
        public string spousename { get; set; }

        public class Family
        {
            public string name { get; set; }
            public int gender { get; set; }
            public int relation { get; set; }
            public DateTime dateofbirth { get; set; }
        }
    }

    public class CustomerLedger : BaseEntity
    {
        public long id { get; set; }
        public int psource { get; set; }
        public int msource { get; set; }
        public string name { get; set; }
        public string gstno { get; set; }
        public string address { get; set; }
        public long stateid { get; set; }
        public long cityid { get; set; }
        public int type { get; set; }
        public decimal billdiscount { get; set; }
        public decimal marginprice { get; set; }
        public string marginmode { get; set; }
        public long agentid { get; set; }
        [JsonIgnore]
        public string city { get; set; }
    }
    public class CustomerCreteria : Customer
    {
        public bool includeunapproved { get; set; }
        public bool inactive { get; set; }
        public DateTime dobtodate { get; set; }
        public bool forkids { get; set; }
        public bool includebills { get; set; }
        public DateTime fromdate { get; set; }
        public DateTime todate { get; set; }
        public DateTime inactivefrom { get; set; }
        public DateTime inactiveto { get; set; }
        public bool ismarriageday { get; set; }
        public long areagroup2id { get; set; }
        public long areagroup1id { get; set; }
        public bool restricted { get; set; }
        public long productid { get; set; }
        public long brandid { get; set; }
        public long sizeid { get; set; }
        public long city { get; set; }
        public decimal fromprice { get; set; }
        public decimal toprice { get; set; }
        public decimal billvalue { get; set; }
        public decimal totalbills { get; set; }
        public decimal billvalue2 { get; set; }
        public decimal totalbills2 { get; set; }

        public List<long> ids { get; set; }
        public DateTime dobfromdate { get; set; }
        public int frompoint { get; set; }
        public int topoint { get; set; }
        public long floorid { get; set; }
        public long counterid { get; set; }
        public string rackno { get; set; }
        public string walkintoken { get; set; }
        public int limit { get; set; }
    }

    public class Customerbillrequest : BaseEntity
    {
        public string cardno { get; set; }
        public string mobileno { get; set; }
        public string customername { get; set; }
        public string emailid { get; set; }
        public long customerid { get; set; }
        public string billno { get; set; }
        public DateTime fromdate { get; set; }
        public DateTime todate { get; set; }
        public long locationid { get; set; }
        public long categoryid { get; set; }
        public decimal fromprice { get; set; }
        public decimal toprice { get; set; }
        public bool issummary { get; set; }
        public int limit { get; set; }
    }
    public class SMSMessageLog : BaseEntity
    {
        public string name { get; set; }
        public string description { get; set; }
        public string message { get; set; }
        public string mobileno { get; set; }
        public DateTime actionon { get; set; }

        public DateTime fromdate { get; set; }
        public DateTime todate { get; set; }
        public string taskname { get; set; }
    }

    public class CustomerBills : BaseEntity
    {
        public long customerid { get; set; }
        public long billtype { get; set; }
        public string customername { get; set; }
        public string remark { get; set; }
        public string gstno { get; set; }
        public string mobileno { get; set; }
        public string otherno { get; set; }
        public long id { get; set; }
        public string billno { get; set; }
        public long billid { get; set; }
        public DateTime billdate { get; set; }
        public decimal billvalue { get; set; }
        public long locationid { get; set; }
        public long counterid { get; set; }
        public long categoryid { get; set; }
        public decimal loyelty { get; set; }
        public decimal adjusted { get; set; }
        public decimal expired { get; set; }
        public DateTime expiry { get; set; }
        public bool altered { get; set; }
        public string message { get;  set; }
        public bool hasaddlndiscount { get; set; }
        public bool hasschemediscount { get; set; }
        public string loyeltyno { get; set; }
        public string cityname { get; set; }
        public string categoryname { get; set; }
        public long balancepoint { get; set; }
        public bool isinterstatesale { get; set; }
    }

    public class MergeCustomerList : BaseEntity
    {
        public long id { get; set; }
        public string type { get; set; }
        public List<Customer> deletelist { get; set; }
        public string ErrorMessage { get; set; }

    }
    public class CustomerIntegrationGroup
    {
        public int pagesize { get; set; }
        public int pageindex { get; set; }
        public List<CustomerIntegration.Customer> list { get; set; }
    }
    public class CustomerIntegration
    {
        public string accesskey { get; set; }
        public string possystem { get; set; }
        public string transactionon { get; set; }
        public string branch { get; set; }
        public Customer customer { get; set; }
        public List<Vouchers> vouchers { get; set; }
        public class Customer
        {
            [JsonIgnore]
            public long _id { get; set; }
            public string name { get; set; }
            public string id { get; set; }
            public string mobileno { get; set; }
            public decimal loyelty { get; set; }
            public List<Vouchers> vouchers { get; set; }
        }
        public class Vouchers
        {
            public DateTime issuedon { get; set; }
            public DateTime expiryon { get; set; }
            public string no { get; set; }
            public decimal issued { get; set; }
            public decimal consumed { get; set; }
        }
    }

    public class CustomerRack : Customer
    {
        public long rackid { get; set; }
        public long customerid { get; set; }
        public string rackno { get; set; }
        public string cardno { get; set; }
    }
    public class Customer2Print : Customer, Report.IBarcodeFormat
    {
        public bool SpecialField(string field, SortedDictionary<string, Object> data)
        {
            return true;
        }
        public Customer2Print(long organizationid)
        {
            this.organizationid = organizationid;
        }
        public Customer2Print()
        {
        }
        public SortedDictionary<string, object> KeyValue
        {
            get
            {
                SortedDictionary<string, object> ret = new SortedDictionary<string, object>();
                ret.Add("CUSTOMERCODE", code);
                ret.Add("CUSTOMERNAME", name);
                ret.Add("GST", gstno);
                ret.Add("ADDRESSLINE1", addressline1);
                ret.Add("ADDRESSLINE2", addressline2);
                ret.Add("ADDRESSLINE3", addressline3);
                ret.Add("ADDRESSLINE4", addressline3);
                ret.Add("ADDRESS", address);
                ret.Add("CITY", cityname);
                ret.Add("CITYPIN", citywithpin);
                ret.Add("CONTACTNO", mobileno);

                return ret;
            }
        }

        public long organizationid { get; set; }
        public SortedDictionary<string, string> KeyFormat
        {
            get
            {
                SortedDictionary<string, string> ret = new SortedDictionary<string, string>();
                return ret;
            }
        }
    }
}
