using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class Purchaseorderitems : BaseEntity
    {
        public long id { get; set; }
        public long productid { get; set; }
        public long itemid { get; set; }
        public decimal bqty { get; set; }
        public decimal oqty { get; set; }
        public decimal rqty { get; set; }
        public decimal cqty { get; set; }
        public decimal aqty { get; set; }
        public decimal designperqty { get; set; }
        public decimal designqty { get; set; }
        public int deliverymode { get; set; }
        public int uomid { get; set; }
        public string designid { get; set; }
        public string description { get; set; }
        public decimal price { get; set; }
        public decimal priceupto { get; set; }
        public int rowtype { get; set; }
        public long tradeagreementid { get; set; }
        public long sizeid { get; set; }
        public string sizegroup { get; set; }
        public long sizegroupid { get; set; }
        public long designuid { get; set; }
        public long colourid { get; set; }
        public decimal sellingprice { get; set; }
        public decimal mrp { get; set; }
        public decimal discount { get; set; }
        public decimal discountpercentage { get; set; }
        public string size { get; set; }
        public string gln { get; set; }
        public string hsncode { get; set; }
        public string pricerange { get; set; }
        [JsonIgnore]
        public string priceranges
        {
            get { return string.Concat(price, "-", priceupto); }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    decimal d;
                    if (value.IndexOf("-") > 0)
                    {
                        string[] item = value.Split('-');
                        if (item != null && item.Length > 0) price = decimal.Parse(item[0]);
                        if (item != null && item.Length > 1) priceupto = decimal.Parse(item[1]);
                    }
                    else
                    {
                        decimal.TryParse(value, out d);
                        price = d;
                    }
                }

            }
        }
        public bool isactive { get; set; }
        public long createdby { get; set; }
        public long modifiedby { get; set; }
        public int version { get; set; }
        public string itbuscode { get; set; }
        public string imageid { get; set; }
        public string groupcode { get; set; }
        public string groupname { get; set; }
        public long purchaseorderid { get; set; }
        public long taxid { get; set; }
        public decimal tax { get; set; }
        public decimal taxpercentage { get; set; }
        public decimal utolerance { get; set; }
        public decimal amount
        {
            get
            {
                return oqty * (price-discount);
            }
        }
        public long cessid { get; set; }
        public decimal cessvalue { get; set; }
        public decimal cesspercentage { get; set; }
        public DateTime createdon { get; set; }
        public DateTime modifiedon { get; set; }
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string productrequestformids_json { get { return Newtonsoft.Json.JsonConvert.SerializeObject(productrequestformids); } set { productrequestformids = Newtonsoft.Json.JsonConvert.DeserializeObject<List<PRFItem>>(value); } }
        public List<PRFItem> productrequestformids { get; set; }
        public class PRFItem
        {
            public long id { get; set; }
            public decimal qty { get; set; }
        }
    }
    public class PurchaseorderitemsDetail : Purchaseorderitems
    {
        public string pono { get; set; }
        public long supplierid { get; set; }
        public long materialid { get; set; }
        public long styleid { get; set; }
        public long brandid { get; set; }
        [JsonIgnore]
        public int sizeindex { get; set; }
        public long typeid { get; set; }
        public long sleeveid { get; set; }
        public long fitid { get; set; }
        public long patternid { get; set; }
        public string printingname { get; set; }
        public string productname { get; set; }
        public string brandname { get; set; }
        public string typename { get; set; }
        public string sleevename { get; set; }
        public string fitname { get; set; }
        public string patternname { get; set; }
        public string stylename { get; set; }
        public string materialname { get; set; }
        public string colourname { get; set; }
        public string designname { get; set; }
        public string sizename { get; set; }
        public decimal taxable
        {
            get
            {
                return sellingprice - tax;
            }
        }
        
        public decimal totaltax
        {
            get
            {
                return oqty * tax;
            }
        }
        public decimal totaltaxable
        {
            get
            {
                return oqty * taxable;
            }
        }
        public string taxname { get; set; }
        public string colourorstyle
        {
            get
            {
                if (!string.IsNullOrEmpty(colourname))
                    return colourname;
                return stylename;
            }
        }
        public string designorpattern
        {
            get
            {
                if (!string.IsNullOrEmpty(designid))
                    return designid;
                return patternname;
            }
        }
        public List<SizeQty> jumpqty { get; set; }
        [JsonIgnore]
        public String delivery
        {
            get
            {
                switch (deliverymode)
                {
                    case 0:
                        return "Any";
                    case 1:
                        return "Bag";
                    case 2:
                        return "Box";
                    case 3:
                        return "Pcs";
                    case 4:
                        return "Meter";
                    default:
                        return "Box";
                }
            }
        }
        [JsonIgnore]
        public String uom
        {
            get
            {
                switch (uomid)
                {
                    case 0:
                        return "Piece";
                    case 1:
                        return "Meter";
                    case 2:
                        return "Gram";
                    case 3:
                        return "Kg";
                    case 4:
                        return "Ml";
                    case 5:
                        return "Litter";
                    case 6:
                        return "Qty";
                    default:
                        return "Box";
                }
            }
        }

        [JsonIgnore]
        public string cut_delivery {
            get
            {
                return string.IsNullOrEmpty(sizename) ? delivery : (sizename + "/" + delivery);
            }
        }
        [JsonIgnore]
        public string design_oqty
        {
            get
            {
                return string.Format("{0}/{1}",designperqty,oqty);
            }
        }
        public ReportLibrary.DOSReporter.Table sizetable { get; set; }
        public string invoiceno { get; set; }
        public DateTime invoicedate { get; set; }
        public int sno { get; set; }
        public class SizeQty
        {
            public long id { get; set; }
            public decimal qty { get; set; }
            public string stringqty
            {
                get
                {
                    if (qty == 0) return "";
                    else
                        return qty.ToString();
                }
            }
            public string name { get; set; }
        }

        [JsonIgnore]
        public bool isinterstate { get; set; }
        [JsonIgnore]
        public decimal cost
        {
            get
            {
                return price-discount;
            }
        }
        [JsonIgnore]
        public decimal Gross
        {
            get
            {
                return oqty * cost;
            }
        }
        [JsonIgnore]
        public decimal Net
        {
            get
            {
                return Gross+ totaltax;
            }
        }

        [JsonIgnore]
        public decimal cgst
        {
            get
            {
                return isinterstate ? 0 : Math.Round(tax * oqty / 2, 2);
            }
        }
        [JsonIgnore]
        public decimal sgst
        {
            get
            {
                return isinterstate ? 0 : Math.Round(tax*oqty / 2, 2);
            }
        }
        [JsonIgnore]
        public decimal igst
        {
            get
            {
                return isinterstate ? tax * oqty : 0;
            }
        }

        [JsonIgnore]
        public decimal cgsttaxpercentage
        {
            get
            {
                return isinterstate || taxpercentage == 0 ? 0 : Math.Round(taxpercentage / 2, 2);
            }
        }
        [JsonIgnore]
        public decimal sgsttaxpercentage
        {
            get
            {
                return isinterstate || taxpercentage == 0 ? 0 : Math.Round(taxpercentage / 2, 2);
            }
        }
        [JsonIgnore]
        public decimal igsttaxpercentage
        {
            get
            {
                return isinterstate ? taxpercentage : 0;
            }
        }

        [JsonIgnore]
        public DataRow mfgfields { get; set; }
    }

    public class PurchaseorderitemsCreteria : Purchaseorderitems
    {
        public string pono { get; set; }
        public bool withinvoice { get; set; }
    }
}
