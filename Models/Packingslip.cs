using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class Packingslip : BaseEntity
    {
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string barcodes_json { get { return Newtonsoft.Json.JsonConvert.SerializeObject(barcodes); } set { barcodes = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Barcodes>>(value); } }
        public List<Barcodes> barcodes { get; set; }

        [JsonIgnore]
        public List<Barcodes> barcodesordered { get; set; }
        public long locationid { get; set; }
        public long packedby { get; set; }
        public long id { get; set; }
        public long salesorderid { get; set; }
        public long customerid { get; set; }
        public long createdby { get; set; }
        public bool isactive { get; set; }
        public long modifiedby { get; set; }
        public DateTime packedon { get; set; }
        public DateTime modifiedon { get; set; }
        public DateTime createdon { get; set; }
        public int version { get; set; }
        public bool used { get; set; }
        public long billid { get; set; }
        public long companyid { get; set; }
        public string sono { get; set; }
        public string packingno { get; set; }
        public string billno { get; set; }
		[JsonIgnore]
        public string sobookno { get; set; }
        [JsonIgnore]
        public Company company { get; set; }
        [JsonIgnore]
        public CompanyLocation location { get; set; }
        [JsonIgnore]
        public Customer customer { get; set; }
        [JsonIgnore]
        public Transport transport { get; set; }

        [JsonIgnore]
        public List<Barcodes> SizeSummary { get; set; }
        public List<string> othersolist { get;  set; }
        [JsonIgnore]
        public Mfgsampleorder mfgsampleorder { get; set; }

        public class Barcodes
        {
            [JsonIgnore]
            public int sno { get; set; }
            public string itemdesigncode { get; set; }
            public long sizeid { get; set; }
            public long colourid { get; set; }
            public long productid { get; set; }
            public string barcode { get; set; }
            public decimal oqty { get; set; }
            public decimal qty { get; set; }
            public long soboxid { get; set; }
            public long fgboxid { get; set; }
            public decimal price { get; set; }
            public decimal fprice { get; set; }
            public decimal mrp { get; set; }
            [JsonIgnore]
            public string productname { get; set; }
            [JsonIgnore]
            public string sizename { get; set; }
            [JsonIgnore]
            public string colourname { get; set; }
            [JsonIgnore]
            public string hsncode { get; set; }
            [JsonIgnore]
            public string boxname { get; set; }
            [JsonIgnore]
            public string uid { get; set; }
            [JsonIgnore]
            public Packingslip parent { get; set; }
            [JsonIgnore]
            public Mfgdesign.Sizegroup.SearchItem uanitem { get; set; }
            [JsonIgnore]
            public decimal printprice
            {
                get
                {
                    if (fprice <= 0) return price;
                    return fprice;
                }
            }

            public List<SalesorderitemsDetail.SizeQty> jumpqty { get;  set; }
        }
    }
    public class PackingslipCreteria : Packingslip
    {
        public int searchmode { get; set; }
        public long productid { get; set; }
        public int auditactiontype { get; set; }
        public DateTime fromdate { get; set; }
        public DateTime todate { get; set; }
    }

    public class PackingslipBox
    {
        public decimal wsrate { get; set; }
        public string packingcode { get; set; }
        public string barcode { get; set; }
        public long soboxid { get; set; }
        public long fgboxid { get; set; }
        public long boxid { get; set; }
    }

    public class PSSummary
    {
        public long id { get; set; }
        public long packedby { get; set; }
        public string customer { get; set; }
        public string billno { get; set; }
        public string packingno { get; set; }
        public string sono { get; set; }
        public string createdby { get; set; }
        public DateTime packedon { get; set; }
        public string modifiedby { get; set; }
        public decimal packqty { get; set; }
        public decimal soqty { get; set; }
        public decimal pqty { get; set; }
        public decimal sopendingqty { get; set; }
    }

    public class PSOrphan
    {
        public string designid { get; set; }
        public string product { get; set; }
        public string colour { get; set; }
        public string size { get; set; }
        public DateTime packedon { get; set; }
        public string barcode { get; set; }
        public string packingno { get; set; }
        public DateTime packingdate { get; set; }
        public decimal qty { get; set; }
        public decimal packqty { get; set; }
        public int actiontype { get; set; }
    }

    public class PSSummaryItem : PSSummary
    {
        public string itemdesigncode { get; set; }
        public string barcode { get; set; }
        public string colour { get; set; }
        public string size { get; set; }
        public string product { get; set; }
        public decimal price { get; set; }
        public decimal qty { get; set; }
        public decimal mqty { get; set; }
    }
    public class PSComboSummaryItem : PSSummaryItem
    {
        public int actiontype { get; set; }
        public string mbarcode { get; set; }
    }
    public class ComboPack : BaseEntity
    {
        public long companyid { get; set; }
        public long locationid { get; set; }
        public long poid { get; set; }
        public string pono { get; set; }
        public List<Combo> comboitems { get; set; }
        public class Combo
        {
            public string uid { get; set; }
            public int qty { get; set; }
            public List<ComboItem> items { get; set; }
            public bool created { get; set; }
            public string message { get; set; }
            public long id { get; set; }

            public class ComboItem
            {
                public string barcode { get; set; }
                public int qty { get; set; }
                public long id { get; set; }
            }
        }
    }
}

