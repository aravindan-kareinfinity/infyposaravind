using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class BaseCompany : BaseEntity
    {
        public long id { get; set; }
        public string name { get; set; }
        public bool internalbuyer { get; set; }
        public decimal internalmargin { get; set; }
        public long stateid { get; set; }
        public long company_as_supplierid { get; set; }
        public string gstno { get; set; }
    }


    public class GST_Info
    {
        public string gstno { get; set; }
        public string name { get; set; }
        [JsonIgnore]
        public long supplierid { get; set; }
        [JsonIgnore]
        public DateTime filedon { get; set; }
        [JsonIgnore]
        public string filedmonth { get; set; }
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string data_json
        {
            get { return Newtonsoft.Json.JsonConvert.SerializeObject(data); }
            set
            {
                data = Newtonsoft.Json.JsonConvert.DeserializeObject<InfyGST.GSTPortal.Taxpayer>(value);
                if (data == null)
                    data = new InfyGST.GSTPortal.Taxpayer();
            }
        }
        public InfyGST.GSTPortal.Taxpayer data { get; set; }
        [JsonIgnore]		
        public bool update { get; internal set; }
    }
    
    public class Company : BaseCompany
    {

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
        public string title { get; set; }
        public string contactperson { get; set; }
        public string contactno { get; set; }
        public string billingname { get; set; }
        public string einvoicepwd { get; set; }
        public string einvoiceusername { get; set; }
        public string address { get; set; }
        public long cityid { get; set; }
        public string cityname { get; set; }
        public string statecode { get; set; }
        public string statename { get; set; }
        public string citywithpin
        {
            get
            {
                if (string.IsNullOrEmpty(pincode)) return cityname;
                else
                    return string.IsNullOrEmpty(cityname) ? pincode : cityname + " - " + pincode;
            }
        }
        public long countryid { get; set; }
        public string pincode { get; set; }
        public string tinanddate { get; set; }
        public string gstusername { get; set; }
        public string gstaccesskey { get; set; }
        public string cstanddate { get; set; }
        public string areacode { get; set; }
        public string tan { get; set; }
        public string pan { get; set; }
        public string website { get; set; }
        public string bankaccountname { get; set; }
        public string bankaccountno { get; set; }
        public string bankifci { get; set; }
        public string pfno { get; set; }
        public string esino { get; set; }
        public string gstno
        {
            get
            {
                return tinanddate;
            }
        }
        public bool isactive { get; set; }
        public long createdby { get; set; }
        public long modifiedby { get; set; }
        public int version { get; set; }
        public DateTime createdon { get; set; }
        public DateTime modifiedon { get; set; }
        public string emailid { get; set; }
        public string code { get; set; }
        public string logo { get; set; }
        public bool isprimary { get; set; }
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string tallyconfig_json
        {
            get { return Newtonsoft.Json.JsonConvert.SerializeObject(tallyconfig); }
            set
            {
                tallyconfig = Newtonsoft.Json.JsonConvert.DeserializeObject<TallyInterfaceConfig>(value);
                if (tallyconfig == null)
                    tallyconfig = new TallyInterfaceConfig();
            }
        }

        public long company_as_customerid { get; set; }
        public string company_as_suppliercode { get; set; }
        public string company_as_customercode { get; set; }
        public TallyInterfaceConfig tallyconfig { get; set; }

        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string servers_json { get { return Newtonsoft.Json.JsonConvert.SerializeObject(servers); } set { servers = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Servers>>(value); } }
        public List<Servers> servers { get; set; }
        [JsonIgnore]
        public Referencelist state { get;  set; }

        public class Servers
        {
            public string location { get; set; }
            public string name { get; set; }
            public string port { get; set; }
            public string type { get; set; }
            public string ip { get; set; }
            public int id { get; set; }
        }

        internal void parseaddress()
        {
            var addresslist = address.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            addresslines = new List<Company.AddressLine>();
            addresslist.ToList().ForEach(e =>
            {
                addresslines.Add(new Company.AddressLine()
                {
                    value = e
                });
            });
        }
    }

    public class PrintServer
    {
        public long id { get; set; }
        public string name { get; set; }
        public string location { get; set; }
        public List<Company.Servers> servers { get; set; }
        public string servers_json
        {
            get { return Newtonsoft.Json.JsonConvert.SerializeObject(servers); }
            set
            {
                servers = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Company.Servers>>(value);
                if (servers != null && servers.Count > 0)
                {
                    int id = 1;
                    servers.ForEach(e => e.id = id++);
                }
            }
        }
    }

    public class TallyInterfaceConfig
    {
        public TallyInterfaceConfig()
        {
            configuration = new Configuration();
        }
        public string companyname { get; set; }
        public string machine { get; set; }
        public string port { get; set; }
        public Configuration configuration { get; set; }
        public class Configuration
        {
            public string vouchertype(string defaultvalue, string configvalue, int key)
            {
                var newconfigvalue = string.IsNullOrEmpty(configvalue) ? defaultvalue : configvalue;
                if(newconfigvalue.IndexOf(',')>0 &&
                    newconfigvalue.IndexOf(':')>0)
                {
                    foreach (var item in newconfigvalue.Split(','))
                    {
                        if (item.Split(':').Length == 2 && 
                            item.Split(':')[0] == key.ToString("F0"))
                            return item.Split(':')[1];
                    }
                }
                return newconfigvalue;
            }
            public string vouchertype(string defaultvalue,string configvalue)
            {
                return string.IsNullOrEmpty(configvalue) ? defaultvalue : configvalue;
            }
            public string paymode_cash { get; set; }
            public string paymode_neft { get; set; }
            public string paymode_upi { get; set; }
            public string paymode_cheque { get; set; }
            public string paymode_card { get; set; }
            public string vouchertype_purchase { get; set; }
            public string vouchertype_sales { get; set; }
            public string vouchertype_purchasereturn { get; set; }
            public string vouchertype_purchasecreditnote { get; set; }
            public string vouchertype_salesreturn { get; set; }
            public string vouchertype_supplierpayment { get; set; }
            public string vouchertype_customerpayment { get; set; }
            public string vouchertype_pettycash { get; set; }
            public string vouchertype_voucher{ get; set; }
            public string B2BOutwardSupplies_StockTransfer { get; set; }
            public string B2BOutwardSupplies_Onlinelocal { get; set; }
            public string B2BOutwardSupplies_OnlineInterstate { get; set; }
            public string Supplies_StockTransfer { get; set; }
            public string Supplies_Interstate { get; set; }
            public string Supplies_Localstate { get; set; }
            public string Supplies_Expenses { get; set; }
            public string Input_CGST { get; set; }
            public string Input_SGST { get; set; }
            public string Input_IGST { get; set; }
            public string Supplier { get; set; }
            public string InwardExpenses { get; set; }
            public string Rounding { get; set; }
            public string PettyExpenses { get; set; }
            public string Bank { get; set; }
            public string UPI { get; set; }
            public string Cash { get; set; }
            public string CustomerPaymentDiscount { get; set; }
            public string CustomerVouchers { get; set; }
            public bool Data_Sales_Address { get; set; }
            public bool Data_Purchase_Address { get; set; }
            public string Data_Purchase_VoucherNo { get; set; }
            public string Data_Purchase_Narration { get; set; }
            public bool Data_Sales_Master { get; set; }
            public bool Data_Purchase_Master { get; set; }
            public string Data_Sales_Reference { get; set; }
            public string Data_Sales_Narration { get; set; }
            public string Customer { get; set; }
            public string Employee { get; set; }
            public string Agent { get; set; }
            public string OutwardSupplies_Interstate { get; set; }
            public string OutwardSupplies_Localstate { get; set; }
            public string B2BOutwardSupplies_Interstate { get; set; }
            public string B2BOutwardSupplies_Localstate { get; set; }
            public string Output_CGST { get; set; }
            public string Output_SGST { get; set; }
            public string Output_IGST { get; set; }
            public string InwardExpenses_GST { get; set; }
            public string InwardExpenses_IGST { get; set; }

            public string TDS { get; set; }
            public string Discount { get; set; }

            public string Region_Local { get; set; }
            public string Region_Interstate { get; set; }
            public string Payment_Cash { get; set; }
            public string Payment_Bank { get; set; }
            public bool Data_Purchase_ApprovalDate { get; set; }
            public bool Data_Purchase_EntryNo { get; set; }
            public bool Data_Purchase_Approval { get; set; }
            public bool Data_Purchase_Datacentre { get; set; }
            public bool Data_Purchase_GSTInfo { get; set; }

            public Configuration()
            {
                vouchertype_purchase = "Purchase";
                vouchertype_sales = "Sales";
                vouchertype_purchasereturn = "";
                vouchertype_salesreturn = "";
                vouchertype_supplierpayment = "";
                vouchertype_customerpayment = "";

                Region_Local = "Local";
                Region_Interstate = "Interstate";
                Supplies_Interstate = "Purchase Accounts~Interstate Inward Supplies";
                Supplies_Localstate = "Purchase Accounts~Local Inward Supplies";
                Supplies_Expenses = "Purchase Accounts~Expense Supplies";

                Input_CGST = "Current Liabilities~Duties & Taxes~GST~Input CGST~CGST @[TAX-PERCENTAGE]";
                Input_SGST = "Current Liabilities~Duties & Taxes~GST~Input SGST~SGST @[TAX-PERCENTAGE]";
                Input_IGST = "Current Liabilities~Duties & Taxes~GST~Input IGST~IGST @[TAX-PERCENTAGE]";
                InwardExpenses = "Direct Expenses~Carriage Inward Expenses";

                TDS = "Current Liabilities~Duties & Taxes~TDS";
                Discount = "Indirect Income~Supplier Discount";

                Rounding = "Direct Expenses~ROUND OFF";
                Supplier = "Sundry Creditors~Supplier~[STATE]~[CITY]~[SUPPLIERCOMPANY]";
                Supplier = "Sundry Creditors~[REGION]~[CITY]~[SUPPLIERCOMPANY]";
                Supplier = "Sundry Creditors~[REGIONORCITY]~[SUPPLIERCOMPANY]";
                Agent = "Sundry Creditors~[REGIONORCITY]~[AGENT]";

                Bank = "Current Assets~[BANK]";
                UPI = "Current Assets~[UPI]";
                Cash = "Cash-in-Hand~Cash";

                Payment_Bank = "Current Assets~[BANK]";
                Payment_Cash = "Cash-in-Hand~Cash";

                Employee = "Sundry Debtors~Trade Debtors~DEBTORS-STAFFS~Customer";
                Customer = "Sundry Debtors~Trade Debtors~DEBTORS~Customer";
                CustomerVouchers = "Sundry Creditors~Credit Vouchers";
                CustomerPaymentDiscount = "Sales Accounts~Sales Discount";
                OutwardSupplies_Interstate = "Sales Accounts~Outward Supplies @ [TAX-PERCENTAGE]% - [BRANCH]";
                OutwardSupplies_Localstate = "Sales Accounts~Outward Supplies @ [TAX-PERCENTAGE]% - [BRANCH]";
                B2BOutwardSupplies_Interstate = "Sales Accounts~B2B Outward Supplies @ [TAX-PERCENTAGE]% - [BRANCH]";
                B2BOutwardSupplies_Localstate = "Sales Accounts~B2B Outward Supplies @ [TAX-PERCENTAGE]% - [BRANCH]";
                Output_CGST = "Current Liabilities~Duties & Taxes~GST~Output CGST";
                Output_SGST = "Current Liabilities~Duties & Taxes~GST~Output SGST";
                Output_IGST = "Current Liabilities~Duties & Taxes~GST~Output IGST";
            }
        }

    }
    public class PrinterConfig
    {
        public string companycode { get; set; }
        public string locationcode { get; set; }
        public string server { get; set; }
        public string port { get; set; }
        public string ipaddress { get; set; }
        public string publicip { get; set; }
        public string licensekey { get; set; }
        public string computername { get; set; }
        public string systemrole { get; set; }
        public string employeecode { get; set; }
        public string passcode { get; set; }
        public string staticip { get; set; }
        public long Organizationid { get; set; }
    }

    public class LicenseInfo
    {
        public bool disabled { get; set; }
        public string publickey { get; set; }
        public bool created { get; set; }
        public bool exist { get; set; }
        public bool passcode { get; set; }
    }

    public class CompanyLocation
    {
        public CompanyLocation()
        {

        }
        public CompanyLocation(Referencelist source)
        {
            this.code = source.code;
            this.name = source.name;
            if (source.additionalinfo != null)
            {
                this.phonenumber = source.additionalinfo == null ? "" : source.additionalinfo.formula;
                this.address = source.additionalinfo == null ? "" : source.additionalinfo.address;
                this.pincode = (source.additionalinfo == null || source.additionalinfo.locationconfig == null) ? "" : source.additionalinfo.locationconfig.pincode;
                if (source.additionalinfo.locationconfig != null)
                {
                    this.contactno = source.additionalinfo.locationconfig.contactno;
                    this.gst = source.additionalinfo.locationconfig.gst;
                }
            }

            this.addresslines = new List<CompanyLocation.Address>();
            if (!string.IsNullOrEmpty(this.address))
            {
                var addresslist = this.address.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                addresslist.ToList().ForEach(e =>
                {
                    this.addresslines.Add(new CompanyLocation.Address()
                    {
                        value = e
                    });
                });
            }
            if (source.additionalinfo != null && source.additionalinfo.locationconfig != null)
            {
                cityid = source.additionalinfo.locationconfig.cityid;
                stateid = source.additionalinfo.locationconfig.stateid;
            }
            companyid = source.additionalinfo != null ? source.additionalinfo.parentid : 0;
        }
        public long companyid { get; set; }
        public long cityid { get; set; }
        public long stateid { get; set; }
        public string cityname { get; set; }
        public string pincode { get; set; }
        public string statename { get; set; }
        public string statecode { get; set; }
        public string name { get; set; }
        public string phonenumber { get; set; }
        public string contactno { get; set; }
        public string code { get; set; }
        public string gst { get; set; }
        public List<Address> addresslines { get; set; }
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
                    this.addresslines = new List<Address>();
                    addresslist.ToList().ForEach(e =>
                    {
                        this.addresslines.Add(new Address()
                        {
                            value = e
                        });
                    });
                }

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

        public string citywithpin
        {
            get
            {
                if(!string.IsNullOrEmpty(cityname) &&
                    !string.IsNullOrEmpty(pincode))
                return cityname + " - " + pincode;

                if (!string.IsNullOrEmpty(cityname))
                    return cityname;
                return pincode;
            }
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

        public class Address
        {
            public string value { get; set; }
        }
    }
    public class CompanyCreteria : Company
    {
        [JsonIgnore]
        public bool onlyseller { get; set; }
        public bool uniquename { get; set; }
    }
}
