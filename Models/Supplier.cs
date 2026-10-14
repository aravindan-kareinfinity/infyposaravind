using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class SupplierHeader : BaseEntity
    {
        public long id { get; set; }
        public string name { get; set; }
        public string code { get; set; }
        public bool supportsourcesupplier { get; set; }
        public long creditdays { get; set; }
        public bool supportpo { get; set; }
        public long agentid { get; set; }
        public decimal minpurdisctpercentage { get; set; }
        public decimal intrateagrmnt { get; set; }
        public int intrateagrmntdays { get; set; }
        public decimal maxmargin { get; set; }
        public decimal minmargin { get; set; }
        public long companyid { get; set; }
        public int rowtype { get; set; }
        public bool limit_billvaluetaxable { get; set; }
        public decimal limit_billvalue { get; set; }
        public long suppliercompanyid { get; set; }
        public bool isinterstatesale { get; set; }
        public string gstno { get; set; }
        public long tdsgroupid { get; set; }
        public decimal soldpercentage { get; set; }

        [JsonIgnore]
        public string city { get; set; }
    }

    public class SupplierLedger : BaseEntity
    {
        public long id { get; set; }
        public string name { get; set; }
        public string gstno { get; set; }
        public string address { get; set; }
        public long stateid { get; set; }
        public long cityid { get; set; }
        [JsonIgnore]
        public string city { get; set; }
    }
    public class Supplier : SupplierHeader
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

        public long internalcustomerid { get; set; }
        public long suppliergroupid { get; set; }
        public string contactperson { get; set; }
        public string contactnumber { get; set; }
        public string emailid { get; set; }
        public long locationid { get; set; }
        public long cityid { get; set; }
        public string cityname { get; set; }
        public string msmeno { get; set; }
        public string pincode { get; set; }
        public string citywithpin
        {
            get
            {
                if (string.IsNullOrEmpty(pincode)) return cityname;
                else
                    return string.IsNullOrEmpty(cityname) ? pincode : cityname + " - " + pincode;
            }
        }

        public bool isactive { get; set; }
        public bool closed { get; set; }
        public long asupplierid { get; set; }
        public List<AddressLine> addresslines { get; set; }
        public class AddressLine
        {
            public string value { get; set; }
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
        public long createdby { get; set; }
        public long modifiedby { get; set; }
        public int version { get; set; }
        public DateTime createdon { get; set; }
        public DateTime modifiedon { get; set; }
        public List<Suppliercompany> companylist { get; set; }

        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string product_json { get { return Newtonsoft.Json.JsonConvert.SerializeObject(product); } set { product = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Product>>(value); } }
        public List<Product> product { get; set; }
        public int autopo_lead { get; set; }
        public decimal autopo_rate { get; set; }
        public class Product
        {
            public long id { get; set; }
        }

        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string brand_json { get { return Newtonsoft.Json.JsonConvert.SerializeObject(brand); } set { brand = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Brands>>(value); } }
        public List<Brands> brand { get; set; }

        public class Brands
        {
            public long id { get; set; }
        }
        public string  code { get; set; }
        public string discountpercentage { get; set; }
        public bool isunregistered { get; set; }
        public DateTime activatedon { get; set; }
        public DateTime deactivatedon { get; set; }

        public string employeename { get; set; }

        public bool isinterstatesale { get; set; }
        public string tinanddate { get; set; }
        public long preferredtransportid { get; set; }
        public string remarks { get; set; }

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
                        this.addresslines.Add(new AddressLine()
                        {
                            value = e
                        });
                    });
                }

            }
        }

        [JsonIgnore]
        public string statename
        {
            get
            {
                if (companylist != null && companylist.Count > 0)
                    return companylist[0].statename;
                return "";
            }
        }
        [JsonIgnore]
        public string legalname
        {
            get
            {
                if (companylist != null && companylist.Count > 0)
                    return companylist[0].name;
                return "";
            }
        }
        [JsonIgnore]
        public string gst
        {
            get
            {
                if (companylist != null && companylist.Count > 0)
                    return companylist[0].tinanddate;
                return "";
            }
        }
        public string ErrorMessage { get; set; }
        public long businessgroupid { get; set; }
        public long buyergroupid { get; set; }
        public long deliverylocationid { get; set; }
        public string companyname { get;  set; }
    }
    public class SupplierCreteria : Supplier
    {
  		public bool hasgst { get; set; }
        public DateTime fromdate { get; set; }
        public DateTime todate { get; set; }
        public bool inactive { get;  set; }
        public bool allsupplier { get;  set; }
        public List<long> ids { get; set; }
        public bool supplierwithcity { get; set; }
    }

    public class SupplierDetails : Supplier
    {
		public string msmeno { get; set; }
        public string address { get; set; }
        public string cityname { get; set; }
        public long stateid { get; set; }
        public string statename { get; set; }
        public long countryid { get; set; }
        public string pincode { get; set; }
        public string tallyledger { get; set; }
        public string companycontactno { get; set; }
        public string companyemailid { get; set; }
        public string cstanddate { get; set; }
        public string areacode { get; set; }
        public string tan { get; set; }
        public string pan { get; set; }
        public string bankname { get; set; }
        public string branchname { get; set; }
        public string bankaccountname { get; set; }
        public string bankaccountno { get; set; }
        public string bankifci { get; set; }
        public string createdusername { get; set; }
        public string modifiedusername { get; set; }

    }
    public class MergeSupplierList : BaseEntity
    {
        public long id { get; set; }
        public string type { get; set; }
        public List<Supplier> deletelist { get; set; }
        public string ErrorMessage { get; set; }

    }

    public class SupplierFilterConfig
    {
        public List<long> brands { get; set; }
        public List<long> products { get; set; }
    }

    public class Supplier2Print : SupplierDetails, Report.IBarcodeFormat
    {
        public bool SpecialField(string field, SortedDictionary<string, Object> data)
        {
            return true;
        }
        public Supplier2Print(long organizationid)
        {
            this.organizationid = organizationid;
        }
        public Supplier2Print()
        {
        }
        public SortedDictionary<string, object> KeyValue
        {
            get
            {
                SortedDictionary<string, object> ret = new SortedDictionary<string, object>();
                ret.Add("SUPPLIERCODE", code);
                ret.Add("SUPPLIERNAME", name);
                ret.Add("GST", tinanddate);
                ret.Add("ADDRESSLINE1", addressline1);
                ret.Add("ADDRESSLINE2", addressline2);
                ret.Add("ADDRESSLINE3", addressline3);
                ret.Add("ADDRESSLINE4", addressline4);
                ret.Add("ADDRESS", address);
                ret.Add("CITY", cityname);
                ret.Add("CITYPIN", citywithpin);
                ret.Add("CONTACTNO", contactnumber);

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

