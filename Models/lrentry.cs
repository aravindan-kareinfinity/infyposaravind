using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Report;

namespace InfyPOS.Models
{
    public class Lrentry : BaseEntity
    {
        public string remarks { get; set; }
        public string statusname { get; set; }
        public Inbox.StatusList Status
        {
            get
            {
                return (Inbox.StatusList)status;
            }
            set
            {
                status = (int)value;
            }
        }
        public long id { get; set; }
        public long lrcompanyid { get; set; }
        public string lrno { get; set; }
        public long bundlerackid { get; set; }
        public DateTime lrentrydate { get; set; }
        public string invoiceno { get; set; }
        public DateTime invoicedate { get; set; }
        public string slipno { get; set; }
        public DateTime slipdate { get; set; }

        public string lrentryno { get; set; }
        public DateTime receiveddate { get; set; }
        public DateTime lrdate { get; set; }
        public long receivinglocationid { get; set; }
        public long fromlocationid { get; set; }
        public long locationid { get; set; }
        public long retaillocationid { get; set; }
        public long entrylocationid { get; set; }
        public long purchasemanagerid { get; set; }
        public int stockholdingperiod { get; set; }
        public long lrsectionid { get; set; }
        public long supplierid { get; set; }
        public long sourcesupplierid { get; set; }
        public bool hasfreightcharge { get; set; }
        public bool hasloadingcharge { get; set; }
        public decimal loadingcharge { get; set; }
        public decimal freightcharge { get; set; }
        public int noofbundels { get; set; }
        public int noofpieces { get; set; }
        public decimal weight { get; set; }
        public string issuedlrno { get; set; }
        public long issuedlrid { get; set; }
        public decimal totalamount { get; set; }
        public decimal premargin { get; set; }
        public DateTime modifiedon { get; set; }
        public DateTime createdon { get; set; }
        public long agentid { get; set; }
        public DateTime duedate { get; set; }
        public long createdby { get; set; }
        public long modifiedby { get; set; }
        public int version { get; set; }
        public bool isactive { get; set; }
        public int status { get; set; }
        public long transportid { get; set; }
        public int modeofdelivery { get; set; }
        public int paymentmode { get; set; }
        public int noofboxes { get; set; }
        public string lrids { get; set; }
        public decimal chargeweight { get; set; }
        public decimal agentcommission { get; set; }
        public List<Lrentryfreightcharges> Lrentryfreightcharges { get; set; }

        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string attachment_json { get { return Newtonsoft.Json.JsonConvert.SerializeObject(attachment); } set { attachment = Newtonsoft.Json.JsonConvert.DeserializeObject<List<slide>>(value); } }

        public List<slide> attachment { get; set; }
        public class slide
        {
            public string title { get; set; }
            public string image { get; set; }
            public string request { get; set; }
        }
        public string ErrorMessage { get; set; }
        public bool Changed_lrcompanyid { get; set; }
        public bool Changed_supplierid { get; set; }
        public bool Changed_retaillocationid { get; set; }
        public decimal lrexpense { get; set; }
        public string createduser { get; set; }
        public string modifieduser { get; set; }
        public long importreferenceid { get; set; }
        public enum ImportReferenceTypes
        {
            Direct,
            WorkOrderGRN,
            BOMGRN,
            UANPO,
            BOMTRANSFER,
            UANTRANSFER
        }
        public ImportReferenceTypes importreferencetype { get; set; }
    }
    public class LrentryCreteria : Lrentry
    {
        public DateTime fromdate { get; set; }
        public DateTime todate { get; set; }
        public DateTime entryfromdate { get; set; }
        public DateTime entrytodate { get; set; }

    
    }
    public class LrEntryPrintCreteria : BaseEntity
    {
        public int copy { get; set; }
        public long reportid { get; set; }
        public long id { get; set; }
        public string companytitle { get; set; }

    }

    public class LrInformation
    {
        public string lrentryno { get; set; }
        public DateTime lrdate { get; set; }
        public string name { get; set; }
        public string invoiceno { get; set; }
        public string lrno { get; set; }
        public DateTime invoicedate { get; set; }
        public decimal grossamount { get; set; }
        public long id { get; set; }
    }

    public class LRBillImport : BaseEntity
    {
        public Lrentry lrentry { get; set; }
        public Bill bill { get; set; }
        public bool autogeneratebarcode { get; set; }
        public bool supportmrp { get; set; }
        public bool supportuanno { get; set; }
        public bool supportmarkdown { get; set; }
        public bool interstate { get;  set; }
        public long suppliercompanyid { get;  set; }
        public decimal marginpercentage { get; set; }
        public decimal discountpercentage { get; set; }
    }

    public class LrRCMEntry : BaseEntity
    {
        public long supplierid { get; set; }
        public long transportid { get; set; }
        public DateTime lrentrydate { get; set; }
        public DateTime lrdate { get; set; }
        public string lrno { get; set; }
        public string lrentryno { get; set; }
        public decimal freightcharge { get; set; }
        public decimal taxpercentage { get; set; }
        public decimal tax { get; set; }
        public decimal total { get; set; }
        public string tgst { get; set; }
        public decimal loadingcharge { get; set; }
        public bool isinterstate { get; set; }
    }

    public class LrEntry2Print : BaseEntity, IBarcodeFormat
    {
        public LrEntry2Print Clone()
        {
            return this.MemberwiseClone() as LrEntry2Print;
        }

        public bool SpecialField(string field, SortedDictionary<string, Object> data)
        {
            return false;
        }
        public long id { get; set; }
        public string suppliername { get; set; }
        public string transportname { get; set; }
        public string sectionname { get; set; }
        public string createdusername { get; set; }
        public string modifiedusername { get; set; }
        public string purchasemanager { get; set; }
        public string remarks { get; set; }
        public string fromcityname { get; set; }
        public string lrno { get; set; }
        public string lrentryno { get; set; }
        public DateTime lrentrydate { get; set; }
        public DateTime lrdate { get; set; }
        public string invoiceno { get; set; }
        public DateTime invoicedate { get; set; }
        public decimal totalamount { get; set; }
        public decimal noofbundels { get; set; }
        public decimal noofpieces { get; set; }
        public decimal noofboxes { get; set; }
        public SortedDictionary<string, object> KeyValue
        {
            get
            {
                SortedDictionary<string, object> ret = new SortedDictionary<string, object>();
                ret.Add("ID", id);
                ret.Add("SUPPLIER", suppliername);
                ret.Add("TRANSPORT", transportname);
                ret.Add("SECTION", sectionname);
                ret.Add("CREATEDUSER", createdusername);
                ret.Add("MODIFIEDUSER", modifiedusername);
                ret.Add("SUPPLIERCITY", fromcityname);
                ret.Add("LRNO", lrno);
                ret.Add("LRDATE", lrdate.ToString("dd-MM-yyyy"));
                ret.Add("LRENTRYNO", lrentryno);
                ret.Add("LRENTRYDATE", lrentrydate.ToString("dd-MM-yyyy"));
                ret.Add("INVOICENO", invoiceno);
                ret.Add("INVOICEDATE", invoicedate.ToString("dd-MM-yyyy"));
                ret.Add("AMOUNT", totalamount);
                ret.Add("NOBUNDLE", noofbundels);
                ret.Add("NOBOX", noofboxes);
                ret.Add("NOPIECE", noofpieces);
                ret.Add("PURMGR", purchasemanager);
                ret.Add("REMARK", remarks);
                return ret;
            }

        }

        public SortedDictionary<string, string> KeyFormat
        {
            get
            {
                SortedDictionary<string, string> ret = new SortedDictionary<string, string>();
                ret.Add("ID", "N0");
                ret.Add("SUPPLIER", "F0");
                ret.Add("TRANSPORT", "F0");
                ret.Add("SECTION", "F0");
                ret.Add("CREATEDUSER", "F0");
                ret.Add("MODIFIEDUSER", "F0");
                ret.Add("SUPPLIERCITY", "F0");
                ret.Add("LRNO", "F0");
                ret.Add("LRDATE", "dd-MM-yyyy");
                ret.Add("LRENTRYNO", "F0");
                ret.Add("LRENTRYDATE", "dd-MM-yyyy");
                ret.Add("INVOICENO", "F0");
                ret.Add("INVOICEDATE", "dd-MM-yyyy");
                ret.Add("AMOUNT", "N0");
                ret.Add("NOBUNDLE", "N0");
                ret.Add("NOBOX", "N0");
                ret.Add("NOPIECE", "N0");
                ret.Add("PURMGR", "F0");
                ret.Add("REMARK", "F0");
                return ret;
            }
        }

    }
}
