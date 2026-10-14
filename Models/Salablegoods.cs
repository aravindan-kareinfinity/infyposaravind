using InfyPOS.Common;
using InfyPOS.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
namespace InfyPOS.Models
{

    public class BatchBarcodes : BaseEntity
    {
        public long soitemid { get; set; }
        public decimal soqty { get; set; }
        public string id { set; get; }
        public string display { get; set; }
        public decimal qty { get; set; }
        public List<Barcodedgoods> items { get; set; }
    }
    public class BatchPO : BaseEntity
    {
        public long locationid { get; set; }
        public List<Item> items { get; set; }
        public class Item
        {
            public long soitemid { get; set; }
            public long soid { get; set; }
            public decimal qty { get; set; }
            public string uid { get; set; }
        }
    }

    public class SellableGoodsCustomerHistory : BaseEntity
    {
        public decimal qty { get; set; }
        public decimal billprice { get; set; }
        public decimal buyingprice { get; set; }
        public decimal receivable { get; set; }
        public decimal discountpercent { get; set; }
        public DateTime billedon { get; set; }
        public string billno { get; set; }
        public DateTime invoicedate { get; set; }
        public string invoiceno { get; set; }
        public string suppliername { get; set; }
    }
    public class SellableGoodsBrandPrice : BaseEntity
    {
        public string brand { get; set; }
        public decimal markup { get; set; }
        public decimal discountpercent { get; set; }
        public decimal markdown { get; set; }
        public DateTime billedon { get; set; }
        public string billno { get; set; }
        public decimal billmargin { get; set; }
    }
    public class BarcodedgoodList : Barcodedgoods
    {
        public List<Barcodedgoods> items { get; set; }
    }
    public class Barcodedbatchgoods : Barcodedgoods
    {
        public string batch { get; set; }
        public DateTime expiry { get; set; }
    }
    public class BarcodedgoodDetail : Barcodedgoods
    {
        public long styleid { get; set; }
        public long sleeveid { get; set; }
        public long fitid { get; set; }
        public long patternid { get; set; }
        public long materialid { get; set; }

    }
    public class Barcodedgoods : BaseEntity
    {
        public string hsncode { get; set; }
        public long id { get; set; }
        public decimal iqty { get; set; }
        public decimal oqty { get; set; }
        public decimal dqty { get; set; }
        public decimal jqty { get; set; }
        public decimal aqty { get; set; }
        public long stockid { get; set; }
        public string barcode { get; set; }
        public string typename { get; set; }
        public string materialname { get; set; }
        public string stylename { get; set; }
        public string patternname { get; set; }
        public string colourname { get; set; }
        public string sizename { get; set; }
        public string brandname { get; set; }
        public long itemid { get; set; }
        public long companyid { get; set; }
        public bool received { get; set; }
        public string sellingname { get; set; }
        public string imageid { get; set; }
        public decimal price { get; set; }
        public decimal discount { get; set; }
        public decimal additionaldiscount { get; set; }
        public long taxid { get; set; }
        public long productid { get; set; }
        public bool stockisbit { get; set; }
        public bool runtimeprice { get; set; }
        public decimal stockqty { get; set; }
        public decimal retailqty
        {
            get
            {
                return iqty - (oqty + jqty + dqty);
            }
        }
        public decimal pdpercent { get; set; }
        public Int32 sellingmode { get; set; }
        public string serialno { get; set; }
        public string obarcode { get; set; }
        public string errormessage { get; set; }
        public decimal buyingprice { get; set; }
        public decimal mrp { get; set; }
        public decimal actualdiscount { get; set; }
        public bool isexchange { get; set; }
        public decimal dealerprice { get; set; }
        public decimal brandmargin { get; set; }
        public bool ismultiprice { get; set; }
        public decimal wsrate { get; set; }
        public string groupkey { get; set; }
        public List<SalesorderDataService.SalesOrderBarcode.item> saleorderitems { get; set; }
        public decimal qty { get; set; }
        public long soitemid { get; set; }
        public string groupname { get; set; }
        public string designid { get; set; }
        public string sizegroup { get; set; }
        public decimal priceextax { get; set; }
        public decimal wsrateextax { get; set; }
        public decimal dealerpriceextax { get; set; }
        public long sizeid { get; set; }
        public long colourid { get; set; }
        public decimal marginpercentage { get; set; }
        public decimal rqty { get; set; }
        public List<Barcodedbatchgoods> batchlist { get; set; }
        public string colourcode { get;  set; }
        public decimal addnlcharge { get;  set; }
        public long addnlchargeid { get; set; }
        public decimal discountpercentage { get; set; }
        public long soboxid { get;  set; }
        public long fgboxid { get; set; }
    }

    public class Salablegoods : BaseEntity
    {
        public long id { get; set; }
        public bool isactive { get; set; }
        public long createdby { get; set; }
        public long modifiedby { get; set; }
        public int version { get; set; }
        public DateTime createdon { get; set; }
        public DateTime modifiedon { get; set; }
        public decimal iqty { get; set; }
        public decimal oqty { get; set; }
        public decimal dqty { get; set; }
        public decimal jqty { get; set; }
        public decimal aqty { get; set; }
        public long stockid { get; set; }
        public long saletaxid { get; set; }
        public string barcode { get; set; }
        public long locationid { get; set; }
        public long bundleid { get; set; }
        public long itemid { get; set; }
        public long companyid { get; set; }
        public bool received { get; set; }
        public string sellingname { get; set; }
        public long supplierid { get; set; }
        public string suppliername { get; set; }
        public string lrreference { get; set; }
        public DateTime inwardon { get; set; }
        public decimal price { get; set; }
        public decimal discount { get; set; }
        public decimal additionaldiscount { get; set; }
        public decimal tiqty { get; set; }
        public decimal toqty { get; set; }
        public decimal costprice { get; set; }
        public decimal icostprice { get; set; }
        public bool ispurchaselocation { get; set; }
        public decimal pqty { get; set; }
        public decimal rqty { get; set; }
        public decimal salereturnqty { get; set; }
        [Newtonsoft.Json.JsonIgnore]
        public long producttype { get; set; }
        [Newtonsoft.Json.JsonIgnore]
        public string serialno{ get; set; }

        //public long invoiceitemid { get; set; }
        //public long productid { get; set; }
        //public string designid { get; set; }
        //public decimal buyingprice { get; set; }
        //public bool isbit { get; set; }
        //public string printingname { get; set; }
        public Salablegoods Clone()
        {
            return (Salablegoods)this.MemberwiseClone();
        }

        [JsonIgnore]
        public decimal stockqty
        {
            get
            {
                return iqty - (oqty + jqty + dqty);
            }
        }
        public Salablegoodshistory GetSalablegoodsHistory()
        {
            return new Salablegoodshistory()
            {
                isactive = isactive,
                createdby = modifiedby,
                createdon = modifiedon,
                modifiedby = modifiedby,
                modifiedon = modifiedon,
                version = version,
                iqty = iqty,
                oqty = oqty,
                dqty = dqty,
                jqty = jqty,
                stockid = stockid,
                barcode = barcode,
                locationid = locationid,
                price = price,
                discount = discount,
                additionaldiscount = additionaldiscount,
                salablegoodsid = id
            };
        }
    }

    public class SalableResult : BaseEntity
    {
        public long id { get; set; }
        public decimal iqty { get; set; }
        public decimal oqty { get; set; }
        public decimal dqty { get; set; }
        public decimal jqty { get; set; }
        public long stockid { get; set; }
        public string barcode { get; set; }
        public string sellingname { get; set; }
        public long locationid { get; set; }
        public long bundleid { get; set; }
        public long itemid { get; set; }
        public bool received { get; set; }
    }
    public class SalablegoodsCreteria : Salablegoods
    {
        public List<long> stockids { get; set; }
        public List<long> ids { get; set; }
        public List<string> barcodes { get; set; }
        public List<string> uanbarcodes { get; set; }
        public string uan { get; set; }
    }

    public class RefilCreteria : BaseEntity
    {
        public long supplierid { get; set; }
        public long productid { get; set; }
        public long companyid { get; set; }
        public string barcode { get; set; }
        public long brandid { get; set; }
        public long materialid { get; set; }
        public long sizeid { get; set; }
        public string printingname { get; set; }

        public string SearchKeywords2TS()
        {
            string retstring = "";
            foreach (string str in printingname.Split(new char[] { ' ', ',' }))
            {
                retstring += (retstring.Length > 0 ? " & " : "") + Regex.Replace(str, @"[^\w\s]", "", RegexOptions.Compiled);
            }
            return retstring;
        }
    }

    public class RefillChange : BaseEntity
    {
        public bool Dispatch { get; set; }
        public bool Return { get; set; }
        public List<Item> Items { get; set; }
        public class Item
        {
            public long id { get; set; }
            public long refillboxid { get; set; }
        }
    }



    public class RefillResult
    {
        public long productid { get; set; }
        public string printingname { get; set; }
        public long id { get; set; }
        public long invoiceitemid { get; set; }
        public decimal stock { get; set; }
        public string barcode { get; set; }
        public long refillboxid { get; set; }
    }

    public class OnlineStock
    {
        public bool fullsync { get; set; }
        public long locationid { get; set; }
        public long stockmarkerid { get; set; }
        public string markername { get; set; }
        public List<OnlineBarcodes> items { get; set; }
        public class OnlineBarcodes
        {
            public long salablegoodsid { get; set; }
            public string barcode { get; set; }
            public string glno { get; set; }
            public decimal stockqty { get; set; }
            public decimal olqty { get; set; }

        }
    }
}
