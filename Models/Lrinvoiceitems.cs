using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class Lrinvoiceitems : BaseEntity
    {
        public long productid { get; set; }
        public string productname { get; set; }
        public int pricingid { get; set; }
        public long supplierid { get; set; }
        public long srcsupplierid { get; set; }
        public decimal buyingqty { get; set; }
        public decimal buyingprice { get; set; }
        public decimal mrp { get; set; }
        public decimal rowexpense { get; set; }
        public decimal rowexpensevalue { get; set; }
        public decimal amount { get; set; }
        public decimal sellingprice { get; set; }
        public decimal wsrate { get; set; }
        public decimal dealerprice { get; set; }
        public decimal weight { get; set; }
        public decimal workingcharge { get; set; }
        public decimal wastage { get; set; }
        public decimal weightrate { get; set; }

        public long cesstaxid { get; set; }
        public decimal cesstax { get; set; }
        public decimal cesspercentage { get; set; }

        public bool isseconds { get; set; }
        public long id { get; set; }
        public bool isactive { get; set; }
        public long createdby { get; set; }
        public long modifiedby { get; set; }
        public int version { get; set; }
        public int barcodecount { get; set; }
        public DateTime createdon { get; set; }
        public DateTime modifiedon { get; set; }
        public long itemid { get; set; }
        public decimal damagedqty { get; set; }
        public decimal sellableqty { get; set; }
        public decimal returnqty { get; set; }
        public long lrinvoiceid { get; set; }
        public decimal salediscount { get; set; }
        public decimal finalprice { get; set; }
        public decimal purchasediscount { get; set; }
        public decimal purchasetotaldiscount { get; set; }
        public decimal purchasediscountpercentage { get; set; }
        public decimal salediscountpercentage { get; set; }
        public decimal marginpercentage { get; set; }
        public decimal basebuyingprice { get; set; }
        public long taxid { get; set; }
        public long stocklocationid { get; set; }
        public long ptaxid { get; set; }
        public decimal purchasetax { get; set; }
        public long sourceid { get; set; }
        public decimal purchasetaxpercentage { get; set; }
        public decimal salestaxpercentage { get; set; }
        public decimal salestax { get; set; }
        public Products.sellingMode sellingmode { get; set; }
        public string barcodes { get; set; }
        public string uan { get; set; }
        public decimal freeqty { get; set; }
        public List<string> barcodeList
        {
            get
            {
                try
                {
                    if (!string.IsNullOrEmpty(barcodes) && barcodes != "null" && barcodes != "NOT GENERATED")
                    {
                        List<string> jsonbarcode = Newtonsoft.Json.JsonConvert.DeserializeObject<List<string>>(barcodes);
                        return jsonbarcode;
                    }
                }
                catch (Exception exp)
                {

                }
                return new List<string>();
            }
            set
            {
                barcodes = Newtonsoft.Json.JsonConvert.SerializeObject(value);
            }
        }
        public string itbuscode { get; set; }
        public string designid { get; set; }
        public long pricetagid { get; set; }
        public string hsncode { get; set; }
        public string imgid { get; set; }
        public decimal sno { get; set; }
        public List<POItemInfo> poinfo { get; set; }
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string POItemInfo { get { return Newtonsoft.Json.JsonConvert.SerializeObject(poinfo); } set { poinfo = Newtonsoft.Json.JsonConvert.DeserializeObject<List<POItemInfo>>(value); } }

        public PriceItemInfo priceinfo { get; set; }
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string PriceItemInfo { get { return Newtonsoft.Json.JsonConvert.SerializeObject(priceinfo); } set { priceinfo = Newtonsoft.Json.JsonConvert.DeserializeObject<PriceItemInfo>(value); } }
        public string ponos
        {
            get
            {
                if (poinfo != null && poinfo.Count > 0)
                {
                    List<string> po = new List<string>();
                    poinfo.ForEach(e => po.Add(e.pono));
                    return string.Join(",", po.ToList());
                }
                return "";
            }
        }
        [JsonIgnore]
        public decimal convertableqty { get; set; }
    }

    public class POItemInfo
    {
        public long purchaseorderitemsid { get; set; }
        public long sizeid { get; set; }
        public string designid { get; set; }
        public string pono { get; set; }
        public decimal price { get; set; }
        public decimal aqty { get; set; }
        public decimal oqty { get; set; }
        public decimal tqty { get; set; }
    }

    public class PriceItemInfo
    {
        public decimal margin { get; set; }
        public decimal discount { get; set; }
        public decimal lastdiscount { get; set; }
        public decimal lastprice { get; set; }
        public decimal lastmargin { get; set; }
    }

    public class BarcodeImageRequest : BaseEntity
    {
        public List<long> ids { get; set; }
    }
    public class BarcodeImageResponse : BaseEntity
    {
        public long id { get; set; }
        public string barcode { get; set; }
        public string imgid { get; set; }
    }
    public class LrinvoiceItems : LrinvoiceItemDetail
    {
        public List<long> itemids { get; set; }
    }

    public class LrinvoiceItemBarcode : BaseEntity
    {
        public string supplier { get; set; }
        public long id { get; set; }
        public DateTime invoicedate { get; set; }
        public decimal buyingqty { get; set; }
        public decimal stockqty { get; set; }
        public string display
        {
            get
            {
                return supplier + " (" + invoicedate.ToString("dd-MM-yyyy") + ")";
            }
        }
        public List<BarcodeStock> stockitems { get; set; }
        public class BarcodeStock
        {
            public string barcode { get; set; }
            public decimal buyingqty { get; set; }
            public decimal stockqty { get; set; }
        }
    }
    public class LrinvoiceItemDetail : Lrinvoiceitems
    {
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string itemdetail
        {
            get { return Newtonsoft.Json.JsonConvert.SerializeObject(Details); }
            set
            {
                if (!string.IsNullOrEmpty(value) && value != "null")
                    Details = Newtonsoft.Json.JsonConvert.DeserializeObject<Items>(value);
            }
        }
        [JsonIgnore]
        public decimal sizeinmeter { get; set; }
        public decimal actualqty { get; set; }
        public decimal actualprice { get; set; }
        public decimal damagedquantity { get; set; }
        public decimal newbuyingqty { get; set; }
        public Items Details { get; set; }
        public bool validateitem { get; set; }
        public long lrid { get; set; }
        public string barcode { get; set; }
        public string invoiceinformation { get; set; }
        public decimal newqty { get; set; }
        [JsonIgnore]
        public long jobworkgrnitemid { get; set; }
        [JsonIgnore]
        public long buyingitemid { get; set; }
        public string createduser { get; set; }
        public string modifieduser { get; set; }
        public List<CustomFieldValue> CustomFields { get; set; }
        public class CustomFieldValue
        {
            public string key { get; set; }
            public string value { get; set; }
        }
        public List<string> designlist { get; set; }
        [JsonIgnore]
        public string racknumber { get; set; }
    }

    public class LrinvoiceitemsCreteria : Lrinvoiceitems
    {
        public bool fetchprice { get; set; }
        public bool fetchdesign { get; set; }
        public bool fetchdesignlist { get; set; }
        public long suppliercompanyid { get; set; }
        public List<long> ids { get; set; }
    }


    public class LrinvoicePrintCreteria : BaseEntity
    {
        public bool IsPDF { get; set; }
        public long reportid { get; set; }
        public long companyid { get; set; }
        public string companytitle { get; set; }
        public long invoiceid { get; set; }
        public long lrid { get; set; }
        public long lrno { get; set; }
        public string lrentryno { get; set; }
        public List<LRInvoiceItemBarcode> filterbarcode { get; set; }
        public List<long> items { get; set; }
        public List<long> stockids { get; set; }
        public List<long> billids { get; set; }
        public int packagesize { get; set; }
        public bool GenerateGUID { get; set; }
        public List<Design2Print> barcodes { get; set; }
    }

    public class InvoiceItem : BaseEntity
    {
        public long id { get; set; }
        public decimal discount { get; set; }
        public decimal price { get; set; }
        public decimal discountpercentage { get; set; }
    }

    public class StockItem : BaseEntity
    {
        public string barcode { get; set; }
        public decimal qty { get; set; }
        public decimal damagedqty { get; set; }
        public decimal returnqty { get; set; }
        public decimal journalqty { get; set; }
        public decimal transfered { get; set; }
        public decimal wstock { get; set; }
        public decimal rstock { get; set; }
        public decimal iqty { get; set; }
        public decimal oqty { get; set; }
        public decimal dqty { get; set; }
        public decimal jqty { get; set; }
        public decimal aqty { get; set; }
    }

    public class MissingBarcode : BaseEntity
    {
        public decimal sellingprice { get; set; }
        public long barcodeid { get; set; }
        public string barcodeids { get; set; }
        public long sizeid { get; set; }
        public string barcode { get; set; }
        public string barcodes { get; set; }
        public long productid { get; set; }
        public long companyid { get; set; }
        public long locationid { get; set; }
        public decimal qty { get; set; }
    }

    public class MigrateStock : BaseEntity
    {
        public List<MissingStock> Stocks { get; set; }
    }
    public class MigrateStockItem : MissingStock
    {
        public string supplier { get; set; }
        public string product { get; set; }
        public string size { get; set; }
        public string invoiceno { get; set; }
        public string invoicedate { get; set; }
        public int taxpercentage { get; set; }
    }
    public class MissingStock : BaseEntity
    {
        public bool isbit { get; set; }
        public string barcode { get; set; }
        public string serialno { get; set; }
        public long sizeid { get; set; }
        public string productname { get; set; }
        public string sizename { get; set; }
        public long productid { get; set; }
        public string reason { get; set; }
        public decimal discount { get; set; }
        public decimal dealerprice { get; set; }
        public decimal price { get; set; }
        public decimal buyingprice { get; set; }
        public decimal discountpercentage { get; set; }
        public decimal qty { get; set; }
        public bool iswarehouse { get; set; }
        public bool directstockupload { get; set; }
        public string hsncode { get; set; }
        public long locationid { get; set; }
        public string sellingname { get; set; }
        public long stockid { get; set; }
        public long companyid { get; set; }
        public long salablegoodsid { get; set; }
        public bool UpdateAll { get; set; }
        public bool newbarcode { get; set; }
        public long itemid { get; set; }
        public long brandid { get; set; }
        public string brandname { get; set; }
        public long supplierid { get; set; }
        public string designid { get; set; }
        public List<sizeInfo> sizegroup { get; set; }
        public long barcodeid { get; set; }
        public long colourid { get; set; }
        public long styleid { get; set; }
        public string itemcode { get; set; }
        public string code { get; set; }
        public decimal displaymrp { get; set; }
        public string displayinvoiceno { get; set; }
        public DateTime displayinvoicedate { get; set; }
        [JsonIgnore]
        public int version { get; set; }
        public class sizeInfo
        {
            public long id { get; set; }
            public string name { get; set; }
            public decimal qty { get; set; }
        }
    }


    public class Lrinvoiceitemsbarcode : BaseEntity
    {
        public decimal mrp { get; set; }
        public decimal dealerprice { get; set; }
        public decimal buyingprice { get; set; }
        public long lrid { get; set; }
        public long productid { get; set; }
        public decimal buyingqty { get; set; }
        public decimal freeqty { get; set; }
        public decimal sellingprice { get; set; }
        public long id { get; set; }
        public long lrinvoiceid { get; set; }
        public long itemid { get; set; }
        public decimal damagedqty { get; set; }
        public decimal actualqty { get; set; }
        public decimal sellableqty { get; set; }
        public decimal salediscount { get; set; }
        public string barcodes { get; set; }
        public int newbarcode { get; set; }
        public string printingname { get; set; }
        public long companyid { get; set; }
        public int version { get; set; }
        public decimal returnqty { get; set; }
        public string hsncode { get; set; }
        public long stocklocationid { get; set; }
        public string uan { get; set; }
        public List<Stock> stock { get; set; }
        public List<Transtock> transaction { get; set; }
        public long srcsupplierid { get; set; }
        public class Transtock
        {
            public Transtock(decimal qty, Salablegoods salablegoods)
            {
                this.qty = qty;
                this.salablegoods = salablegoods;
            }
            public Transtock(decimal qty, Stock stock)
            {
                this.qty = qty;
                this.stock = stock;
            }
            public decimal qty { get; set; }
            public Stock stock { get; set; }
            public Salablegoods salablegoods { get; set; }
        }
        public Stock CreateStock(string barcode)
        {
            Stock stock = new Stock();
            stock.hsncode = hsncode;
            stock.barcode = barcode;
            stock.discount = salediscount;
            stock.invoiceitemid = id;
            stock.itemid = itemid;
            stock.price = sellingprice;
            stock.buyingprice = buyingprice;
            stock.dealerprice = dealerprice;
            stock.displaymrp = mrp;
            stock.productid = productid;
            stock.qty = 1;
            stock.version = 1;
            stock.isactive = true;
            stock.printingname = printingname;
            stock.companyid = companyid;
            stock.retaillocationid = stocklocationid;
            stock.serialno = string.IsNullOrWhiteSpace(this.uan) ? this.gln : this.uan;
            stock.quantoexistingbarcode = this.quantoexistingbarcode;
            return stock;
        }

        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string itemdetail { get { return Newtonsoft.Json.JsonConvert.SerializeObject(Details); } set { Details = Newtonsoft.Json.JsonConvert.DeserializeObject<Items>(value); } }
        public Items Details { get; set; }
        public bool validateitem { get; set; }
        public bool isQuantoImport { get; set; }
        public List<string> barcodeList
        {
            get
            {
                try
                {
                    if (!string.IsNullOrEmpty(barcodes) && barcodes != "null" && barcodes != "NOT GENERATED")
                    {
                        List<string> jsonbarcode = Newtonsoft.Json.JsonConvert.DeserializeObject<List<string>>(barcodes);
                        return jsonbarcode;
                    }
                }
                catch (Exception exp)
                {

                }
                return new List<string>();
            }
        }
        public string quantoexistingbarcode { get; set; }
        public string gln { get; set; }
        public string itbuscode { get; set; }
        public long startbarcodeno { get; set; }
        public long endbarcodeno { get; set; }

    }
}
