using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{

    public class LedgerHeader : BaseEntity
    {
        public long id { get; set; }
        public string displayname { get; set; }
        public int typeid { get; set; }

    }
    public class Ledger : LedgerHeader
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

        [JsonIgnore]
        public ReferenceItem State { get; set; }
        [JsonIgnore]
        public List<AddressLine> addresslines { get; set; }
        public class AddressLine
        {
            public string value { get; set; }
        }
        [JsonIgnore]
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
        [JsonIgnore]
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
        [JsonIgnore]
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
        [JsonIgnore]
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

        internal void parseaddress()
        {
            var addresslist = contactaddress.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            addresslines = new List<AddressLine>();
            addresslist.ToList().ForEach(e =>
            {
                addresslines.Add(new AddressLine()
                {
                    value = e
                });
            });
        }


        public string hsncode { get; set; }
        public string gstno { get; set; }
        public string billingname { get; set; }
        public string parentgroup { get; set; }
        public string pincode { get; set; }
        
        public bool isactive { get; set; }
        public bool isinterstate { get; set; }
        public long createdby { get; set; }
        public long modifiedby { get; set; }
        public int version { get; set; }
        public decimal opening { get; set; }
        public decimal closing { get; set; }
        public int tagid { get; set; }
        public DateTime createdon { get; set; }
        public DateTime modifiedon { get; set; }
        public string refid { get; set; }
        public string contactperson { get; set; }
        public string contactnumber { get; set; }
        public string contactaddress { get; set; }
        public string emailid { get; set; }
        public long parentid { get; set; }
        public long transportid { get; set; }
        public long customerid { get; set; }
        public long supplierid { get; set; }
        public long bankid { get; set; }
        public long cityid { get; set; }
        public long stateid { get; set; }
        public int taxmode { get; set; }
        public decimal taxpercentage { get; set; }
        public bool billwise { get; set; }

        public bool eodcashledger { get; set; }
        public LedgerExtension attributes { get; set; }

        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string attributes_json { get { return Newtonsoft.Json.JsonConvert.SerializeObject(attributes); } set { attributes = Newtonsoft.Json.JsonConvert.DeserializeObject<LedgerExtension>(value); } }

        public LedgerHierarchy parenthierarchy { get; set; }
        public ParentInfo parentinfo { get; set; }
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string parentinfo_json
        {
            get { return Newtonsoft.Json.JsonConvert.SerializeObject(parentinfo); }
            set
            {
                if (!string.IsNullOrEmpty(value) && value != "null")
                    parentinfo = Newtonsoft.Json.JsonConvert.DeserializeObject<ParentInfo>(value);
            }
        }
        public class ParentInfo
        {
            public int level { get; set; }
            public string detail { get; set; }
            public string[] levels { get; set; }

        }

        public class LedgerExtension
        {
            public bool hastds { get; set; }
            public bool hasgst { get; set; }
            public bool hasbank { get; set; }
            public bool issupplier { get; set; }
            public bool iscustomer { get; set; }
            public bool istransport { get; set; }
            public bool ismsme { get; set; }
            public long ledgertdsid { get; set; }
            public long tdsgroupid { get; set; }
            public long ledgersgstid { get; set; }
            public long ledgercgstid { get; set; }
            public long ledgerigstid { get; set; }
            public long ledgercessid { get; set; }
            public long assettypeid { get; set; }
            public string unit { get; set; }
            public string panno { get; set; }
            public string msmeno { get; set; }
            public List<long> invttaxids { get; set; }
            public List<long> invstaxids { get; set; }
            public List<long> invptaxids { get; set; }
            public List<long> branchids { get; set; }
            public AssetInfo asset { get; set; }
            public BankInfo bank { get; set; }
            public decimal tagvalue { get; set; }
            public List<TagValue> tagvalues { get; set; }
            public List<TDSInfo> tdsrules { get; set; }
        }

        public class TagValue
        {
            public long id { get; set; }
            public decimal value { get; set; }
        }
        public class LedgerTDSInfo : TDSInfo
        {
            public long id { get; set; }
        }
        public class TDSInfo
        {
            public int mode { get; set; }
            public int eligible { get; set; }
            public decimal value { get; set; }
        }

        public class BankInfo
        {
            public string accountname { get; set; }
            public string accountno { get; set; }
            public string ifsc { get; set; }
            public string branch { get; set; }
            public List<OBandInterest> ob { get; set; }
            public List<OBandInterest> ir { get; set; }
            public class OBandInterest
            {
                public DateTime date { get; set; }
                public decimal value { get; set; }
            }
        }

        public class AssetInfo
        {
            public long locationid { get; set; }
            public Referencelist.AssetConfig config { get; set; }
            public long supplierid { get; set; }
            public long amcid { get; set; }
            public long insuranceid { get; set; }
            public string registrationno { get; set; }
            public string insuranceno { get; set; }
            public string amcno { get; set; }
            public DateTime registrationon { get; set; }
            public DateTime registrationexpiry { get; set; }
            public DateTime insuranceon { get; set; }
            public DateTime insuranceexpiry { get; set; }
            public DateTime amcon { get; set; }
            public DateTime amcexpiry { get; set; }
            public DateTime expiryon { get; set; }
            public decimal total { get; set; }
            public decimal available { get; set; }
            public List<AssetTransaction> transactions { get; set; }
            public class AssetTransaction
            {
                public string type { get; set; }
                public string referenceno { get; set; }
                public DateTime issued { get; set; }
                public DateTime expiry { get; set; }
                public string status { get; set; }
                public decimal added { get; set; }
                public decimal removed { get; set; }
                public List<string> files { get; set; }
            }
        }
    }

    public class LedgerDetail : Ledger
    {
        [JsonIgnore]
        public long cempid { get; set; }
        [JsonIgnore]
        public long mempid { get; set; }
        public string cemp { get; set; }
        public string memp { get; set; }
    }
    public class LedgerCreteria : Ledger
    {
        public List<int> typeids { get; set; }
        public List<long> ids { get; set; }
        public bool suppliertds { get; set; }
        public Filter filterid { get; set; }
        public enum Filter
        {
            All = 0,
            InventorySupplier = 1,
            InventoryCustomer = 2,
            GSTAvailable = 3,
            PANAvailable = 4,
            ManagableAssets = 5,
            GSTNoMissing = 6,
            PANMissing = 7,
            BillwiseEnabled = 8,
            BillwiseDisabled = 9,
            TDSLedgers = 10,
            GSTEnabled = 11,
            GSTLedgers = 12,
            IGSTLedgers = 13,
            SGSTLedgers = 14,
            CGSTLedgers = 15,
            TCSLedgers = 16,
            CESSLedgers = 17,
            InventoryBank = 18,
            PandLExpense = 20,
            DailyExpense = 21,
            WeeklyExpense = 22,
            MonthlyExpense = 23,
            YearlyExpense = 24,
            AssetDepreciation = 25,
            SalaryExpense = 26,
            SalesMarginAtExpense = 27,
            PurchaseMarginExpense = 28,
            InventoryInterest = 37,
            InventoryStock = 38,
            CashRelated = 39
        }
    }
    public class PrintableLedger : Ledger
    {
        public void ProcessTotal()
        {
            ProcessTotal(false);
        }
        public void ProcessTotal(bool addobtototal)
        {
            var tcredit = transactions.Sum(x => string.IsNullOrEmpty(x.credit) ? 0 : double.Parse(x.credit));
            var tdebit = transactions.Sum(x => string.IsNullOrEmpty(x.debit) ? 0 : double.Parse(x.debit));
            var ocredit = string.IsNullOrEmpty(obcredit) ? 0 : double.Parse(obcredit);
            var odebit = string.IsNullOrEmpty(obdebit) ? 0 : double.Parse(obdebit);
            if (addobtototal)
            {
                tcredit += ocredit;
                tdebit += odebit;
                totalcredit = tcredit.ToString("F2");
                totaldebit = tdebit.ToString("F2");

                if (tcredit > tdebit)
                {
                    cbcredit = (tcredit - tdebit).ToString("F2");
                }
                else
                {
                    cbdebit = (tdebit - tcredit).ToString("F2");
                }
                totalcredit = tcredit > 0 ? tcredit.ToString("F2") : "";
                totaldebit = tdebit > 0 ? tdebit.ToString("F2") : "";
            }
            else
            {
                totalcredit = tcredit.ToString("F2");
                totaldebit = tdebit.ToString("F2");

                var ccredit = ocredit + tcredit;
                var cdebit = odebit + tdebit;
                if (ccredit > cdebit)
                {
                    cbcredit = (ccredit - cdebit).ToString("F2");
                }
                else
                {
                    cbdebit = (cdebit - ccredit).ToString("F2");
                }
                totalcredit = tcredit > 0 ? tcredit.ToString("F2") : "";
                totaldebit = tdebit > 0 ? tdebit.ToString("F2") : "";
            }
        }
		
        public Company company { get; set; }
        public CompanyLocation location { get; set; }
        public string title { get; set; }
        public string ledgername { get; set; }
        public bool issummary { get; set; }
        private string _address;
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
        public List<AddressLine> addresslines { get; set; }
        public class AddressLine
        {
            public string value { get; set; }
        }
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
        public class Transaction
        {
            public long id { get; set; }
            public string type { get; set; }
            public DateTime date { get; set; }
            public string description { get; set; }
            public string credit { get; set; }
            public string debit { get; set; }
            public decimal tds { get; set; }
            public decimal gst { get; set; }
            public decimal rnd { get; set; }
            public string reference { get; set; }
            public VoucherDisplayItem detail { get; set; }
            [JsonIgnore]
            public long createdby { get; internal set; }
            [JsonIgnore]
            public long modifiedby { get; internal set; }
            [JsonIgnore]
            public DateTime modifiedon { get; internal set; }
            [JsonIgnore]
            public DateTime createdon { get; internal set; }
            public string cuser { get; internal set; }
            public string muser { get; internal set; }
        }
        public List<Transaction> transactions { get; set; }
        public string obcredit { get; set; }
        public string obdebit { get; set; }
        public string totalcredit { get; set; }
        public string totaldebit { get; set; }
        public string cbcredit { get; set; }
        public string cbdebit { get; set; }
    }
}
