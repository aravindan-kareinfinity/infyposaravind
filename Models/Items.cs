using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
namespace InfyPOS.Models
{

    public class BaseItem : BaseEntity
    {
        public long id { get; set; }
        public string sellingname { get; set; }
        public long brandid { get; set; }
        public long productid { get; set; }
        public long fixedsupplierid { get; set; }
        public bool disablepo { get; set; }

        public long colourid { get; set; }
        public long patternid { get; set; }
        public long sizeid { get; set; }
        public long styleid { get; set; }
        public long typeid { get; set; }
        public long materialid { get; set; }
        public long tradeagreeentid { get; set; }
        public string agmtcode { get; set; }    
        public string printingname { get; set; }
        public string serialno { get; set; }
        public decimal minrate { get; set; }
        public decimal ratetobebilled { get; set; }
        public decimal mrp { get; set; }
        public decimal salerate { get; set; }
        public decimal purchaserate { get; set; }
        public decimal dealerprice { get; set; }
        public int dsmode { get; set; }
        public bool dssupport { get; set; }
        public decimal dsbuyingqty { get; set; }
        public decimal dsfreeqty { get; set; }
        public decimal dscomboprice { get; set; }
        public decimal dsdiscountpercent { get; set; }
        public decimal dsfromqty { get; set; }
        public long dsdiscountproductid { get; set; }
        public bool allowsearch { get; set; }
        public bool support_itemlevelprice { get; set; }
        public bool support_pos { get; set; }
        public bool support_gln { get; set; }
        public bool support_expiry { get; set; }
        
        
        [Newtonsoft.Json.JsonIgnore]
        public bool changed { get; set; }
        public string imageid { get; set; }
        public string gln { get; set; }
        public long classid { get; set; }
        public long prddivisionid { get; set; }
        public long sleeveid { get; set; }
        public long fitid { get; set; }
        public long seasonid { get; set; }
        public string quantoexistingbarcode { get; set; }
    }
    public class Items : BaseItem
    {
        public string searchkeys
        {
            get
            {
                var str = string.Format("{0} {1} {2} {3} {4} {5} {6} {7}",
                    brandname, productname, colourname, patternname, sizename, stylename, materialname, typename).Replace("  ", " ").ToLower();

                return Regex.Replace(str, @"[^\w\s]", "", RegexOptions.Compiled);
            }
        }

        public class MultiSupplier
        {
            public long id { get; set; }
        }
        public bool singlesupplier { get; set; }
        public bool multisupplier { get; set; }
        public List<MultiSupplier> supplierlist { get; set; }
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string supplierlist_json
        {
            get { return Newtonsoft.Json.JsonConvert.SerializeObject(supplierlist); }
            set
            {
                if (value != null)
                {
                    try
                    {
                        supplierlist = Newtonsoft.Json.JsonConvert.DeserializeObject<List<MultiSupplier>>(value);
                    }
                    catch (Exception exp)
                    {

                    }
                }
            }
        }

        public string code { get; set; }
        public string brandname { get; set; }
        public string productname { get; set; }
        public string colourname { get; set; }
        public string patternname { get; set; }
        public string sizename { get; set; }
        public string typename { get; set; }
        public string stylename { get; set; }
        public string sleevename { get; set; }
        public string fitname { get; set; }
        public string materialname { get; set; }
        public string glndesignid { get; set; }
        public int salemode { get; set; }
        public int pricingmode { get; set; }
        public long createdby { get; set; }
        public long modifiedby { get; set; }
        public bool isactive { get; set; }
        public int version { get; set; }
        public DateTime expiry { get; set; }
        public string batch { get; set; }
        public decimal podays { get; set; }
        public decimal stockagedays { get; set; }
        public decimal reordermin {get;set;}
        public decimal reordermax {get;set;}
        public decimal autopomin { get; set; }
        public decimal autopomax { get; set; }

        public int expiryvalue { get; set; }
        public int expirymode { get; set; }
        public DateTime createdon { get; set; }
        public DateTime modifiedon { get; set; }
        public long sourceitemid { get; set; }
        public long grnitemid { get;  set; }
        public long grnsizeid { get;  set; }
        public string sizegroup { get;  set; }
        public long pricetagid { get; set; }
        public bool isbit { get; set; }
        public List<PriceListItem> pricelist { get; set; }
        public class PriceListItem
        {
            public long id { get; set; }
            public decimal price { get; set; }
        }

        public static List<PriceListItem> CreatePriceList(string source)
        {
            if (string.IsNullOrEmpty(source)) return new List<PriceListItem>();
            return Newtonsoft.Json.JsonConvert.DeserializeObject<List<PriceListItem>>(source);
        }

        public List<CustomFieldValue> CustomFields { get; set; }
        public class CustomFieldValue
        {
            public string key { get; set; }
            public string value { get; set; }
        }

    }

    public class TouchItem
    {
        public long productgroupid { get; set; }
        public string productgroup { get; set; }
        public long foodcounterid { get; set; }
        public string foodcounter { get; set; }
        public List<Product> products { get; set; }
        public class Product
        {
            public string productname { get; set; }
            public string sellingname { get; set; }
            public string sizename { get; set; }
            public string barcode { get; set; }
            public bool dssupport { get; set; }
            public long productid { get; set; }
            public long taxid { get; set; }
            public string imageid { get; set; }
            public decimal rate { get; set; }
            public decimal mrp { get; set; }
            public long itemid { get; set; }
            public DiscountConfig discount { get; set; }
        }

        public class DiscountConfig
        {
            public long dsdiscountproductid { get; set; }
            public int dsmode { get; set; }
            public decimal dsbuyingqty { get; set; }
            public decimal dsfreeqty { get; set; }
            public decimal dscomboprice { get; set; }
            public decimal dsdiscountpercent { get; set; }
            public decimal dsfromqty { get; set; }
        }
    }
    public class SearchByDesignName : BaseEntity
    {
        public long supplierid { get; set; }
        public long productid { get; set; }
        public string designid { get; set; }
    }

    public class SearchByName : BaseEntity
    {
        public string name { get; set; }
        public long productid { get; set; }
    }

    public class ItemsCreteria : Items
    {
        public bool skudesign { get; set; }
        public bool onlysearchable { get; set; }
        public string SearchKeywords { get; set; }
        public bool intelligent { get; set; }
        public bool productmap { get; set; }
        public string barcode { get; set; }
        public bool autocreate { get; set; }
        public bool onlyglnitems { get; set; }
        public bool onlytouchitems { get; set; }
        public bool onlynonuan { get; set; }
        public List<long> ids { get; set; }
        public List<string> glns { get; set; }
        public string SearchKeywords2TS()
        {
            string retstring = "";
            foreach (string str in SearchKeywords.ToLower().Split(new char[] {' ',','}, StringSplitOptions.RemoveEmptyEntries))
            {
                retstring += (retstring.Length>0 ? " & ":"") + Regex.Replace(str, @"[^\w\s]", "", RegexOptions.Compiled);
            }
            return retstring;
        }
    }

    public class NonUANItem : BaseEntity
    {
        public long id { get; set; }
        public string gln { get; set; }
        public string printingname { get; set; }
        public string productgroup { get; set; }
        public string imageid { get; set; }
        public decimal mrp { get; set; }
        public decimal price { get; set; }
    }
    public class DisplayItem : BaseItem
    {

        public string code { get; set; }
        public string brandname { get; set; }
        public string productname { get; set; }
        public string colourname { get; set; }
        public string patternname { get; set; }
        public string sizename { get; set; }
        public string typename { get; set; }
        public string stylename { get; set; }
        public string materialname { get; set; }
        public long id { get; set; }
    }
}
