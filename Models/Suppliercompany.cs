using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class SupplierCompanyHeader : BaseEntity
    {
        public long id { get; set; }
        public long supplierid { get; set; }
        public string name { get; set; }
        public string code { get; set; }
        public bool isinterstatesale { get; set; }
        [JsonIgnore]
        public string city { get; set; }
        public string gst { get; set; }
    }

    public class SupplierProduct : BaseEntity
    {
        public long id { get; set; }
        public List<long> child { get; set; }
    }
    public class Suppliercompany : SupplierCompanyHeader
    {
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
        private string _address;
        public string address {
            get { return _address; }
            set {
                _address = value;
                if (!string.IsNullOrEmpty(_address))
                {
                    var str = FormatAddress(_address);
                    var addresslist = str.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                    this.addresslines = new List<AddressLine>();
                    addresslist.ToList().ForEach(e =>
                    {
                        this.addresslines.Add(new Suppliercompany.AddressLine()
                        {
                            value = e
                        });
                    });
                }
            }
        }
        public long cityid { get; set; }
        public string cityname { get; set; }
        public long stateid { get; set; }
        public string statename { get; set; }
        public string statecode { get; set; }
        public long countryid { get; set; }
        public string pincode { get; set; }
        public string contactperson { get; set; }
        public string contactnumber { get; set; }
        public string tallyledger { get; set; }
        public long agentid { get; set; }
        public string companycontactno { get; set; }
        public string companyemailid { get; set; }
        public string tinanddate { get; set; }
        public string gstno {
            get
            {
                return tinanddate;
            }
        }
        public string cstanddate { get; set; }
        public string areacode { get; set; }
        public string tan { get; set; }
        public string pan { get; set; }
        public string bankaccountname { get; set; }
        public string bankaccountno { get; set; }
        public string bankifci { get; set; }
        public string bankname { get; set; }
        public string bankbranch { get; set; }
        public long bankid { get; set; }
        public bool isactive { get; set; }
        public long createdby { get; set; }
        public long modifiedby { get; set; }
        public int version { get; set; }
        public DateTime createdon { get; set; }
        public DateTime modifiedon { get; set; }
        public long preferredtransportid { get; set; }
        public string displaycontactno { get
            {
                if (string.IsNullOrEmpty(contactnumber)) return companycontactno;
                else
                    return string.IsNullOrEmpty(companycontactno) ? contactnumber : contactnumber + ", " + companycontactno;
            }
        }
        public string citywithpin
        {
            get
            {
                if (string.IsNullOrEmpty(pincode)) return cityname;
                else
                    return string.IsNullOrEmpty(cityname) ? pincode : cityname + " - " + pincode;
            }
        }
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

        public string msmeno { get; set; }
        public long tdsgroupid { get; set; }
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
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string addresslist_json { get { return Newtonsoft.Json.JsonConvert.SerializeObject(addresslist); } set { addresslist = Newtonsoft.Json.JsonConvert.DeserializeObject<List<AddressList>>(value); } }

        public List<AddressList> addresslist { get; set; }
        public class AddressList
        {
            public long typeid { get; set; }
            public string typename { get; set; }
            public long cityid { get; set; }
            public string cityname { get; set; }
            public long stateid { get; set; }
            public string statename { get; set; }
            public long countryid { get; set; }
            public string pincode { get; set; }
            public string address { get; set; }
            public string contactnumber { get; set; }
        }

    }
    public class SuppliercompanyCreteria : Suppliercompany
    {
        public List<long> ids { get; set; }
        public bool isprint { get; set; }
        public long rowtype { get; set; }
    }
}
