using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class Discountscheme : BaseEntity
    {
        public long id { get; set; }
        public bool isactive { get; set; }
        public long createdby { get; set; }
        public long modifiedby { get; set; }
        public int version { get; set; }
        public DateTime createdon { get; set; }
        public DateTime modifiedon { get; set; }
        public DateTime schemedate { get; set; }
        public DateTime schemeappliedon { get; set; }
        public DateTime schemeremovedon { get; set; }
        public long schemeappliedby { get; set; }
        public string reason { get; set; }
        public long companyid { get; set; }
        public long locationid { get; set; }
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string filter_json { get { return Newtonsoft.Json.JsonConvert.SerializeObject(filter); } set { filter = Newtonsoft.Json.JsonConvert.DeserializeObject<Filter>(value); } }
        public Filter filter { get; set; }
        public int discountmode { get; set; }
        public decimal discountvalue { get; set; }
        public decimal rounding { get; set; }
        public int status { get; set; }
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string itemlist_json { get; set; }


        public class Filter:BaseEntity
        {
            public long supplierid { get; set; }
            public long productid { get; set; }
            public long materialid { get; set; }
            public long brandid { get; set; }
            public long patternid { get; set; }
            public long sizeid { get; set; }
            public long typeid { get; set; }
            public long styleid { get; set; }
            public long colourid { get; set; }
            public long pricetagid { get; set; }
            public string designid { get; set; }
        }

        public class FilterCreteria : Filter
        {
            public bool dosort { get; set; }
            public bool iswarehouse { get; set; }
            public long locationid { get; set; }
            public string barcode { get; set; }
            public long id { get; set; }
            public long discountschemeid { get; set; }
            public DateTime fromdate { get; set; }
            public DateTime todate { get; set; }

            public DateTime changedfromdate { get; set; }
            public DateTime changedtodate { get; set; }

            public long taxid { get; set; }
            public decimal taxpercentage { get; set; }
            public List<string> barcodes { get; set; }
            public List<BarcodewithIndex> indexedbarcodes { get; set; }

            public class BarcodewithIndex
            {
                public int itemindex { get; set; }
                public string barcode { get; set; }
            }
        }

        public class DiscountValue
        {
            public long id { get; set; }
            public long salablegoodsid { get; set; }
            public decimal amt { get; set; }
        }
        public class DiscountItem
        {
            public long stockid { get; set; }
            public string printingname { get; set; }
            public string barcode { get; set; }
            public byte status { get; set; }
            public decimal price { get; set; }
            public decimal additionaldiscount { get; set; }
            public long salalblegoodsid { get; set; }
            public decimal salablestock { get; set; }
            public decimal stockqty { get; set; }
            public decimal discount { get; set; }
            public decimal applieddiscount { get; set; }
        }
    }
    public class DiscountschemeCreteria : Discountscheme
    {
        public DateTime fromdate { get; set; }
        public DateTime todate { get; set; }
    }

    public class DiscountschemeCover:BaseEntity
    {
        public Discountscheme Scheme { get; set; }
        public List<Discountscheme.DiscountItem> Items { get; set; }
    }

}
