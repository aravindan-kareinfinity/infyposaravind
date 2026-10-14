using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class Bundles : BaseEntity
    {
        public long id { get; set; }
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string items_json
        {
            get { return Newtonsoft.Json.JsonConvert.SerializeObject(items); }
            set { items = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Bundleitem>>(value); }
        }
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public Company fromcompany { get; set; }

        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public Company tocompany { get; set; }

        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public Transport transport { get; set; }
        public bool iscompanytransfer { get; set; }
        public long tocompanyid { get; set; }
        public long fromcompanyid { get; set; }
        public long billid { get; set; }
        public long invoiceid { get; set; }
        public string lrentryno { get; set; }
        public bool autotransfer { get; set; }
        public List<Bundleitem> items { get; set; }
        public string code { get; set; }
        public string billno { get; set; }
        public bool scanmode { get; set; }
        public string remarks { get; set; }
        public long fromlocationid { get; set; }
        public long tolocationid { get; set; }
        public long packedby { get; set; }
        public string packedbyemployee { get; set; }
        public string createdbyemployee { get; set; }
        public long receivedby { get; set; }
        public DateTime requesteddate { get; set; }
        public DateTime packedon { get; set; }
        public DateTime receivedon { get; set; }
        public long pricetype { get; set; }
        public bool inward2warehouse { get; set; }
        public decimal qty { get; set; }
        public string notes { get; set; }
        public long approvedby { get; set; }
        public bool isactive { get; set; }
        public DateTime createdon { get; set; }
        public DateTime modifiedon { get; set; }
        public long createdby { get; set; }
        public long modifiedby { get; set; }
        public long companyid { get; set; }
        public int status { get; set; }
        public int version { get; set; }
        public List<Salablegoods> Salablegoods { get; set; }
        public bool returngoods { get; set; }
        public string errormessage { get; set; }
        public string vehicleno { get; set; }
        public string ewb { get; set; }
        public long transportid { get; set; }
        public int modeofdelivery { get; set; }
        public string requestcode { get; set; }
        //public void ToJSON()
        //{
        //    items_json = Newtonsoft.Json.JsonConvert.SerializeObject(items);
        //}
        //public void ToObject()
        //{
        //    items = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Bundleitem>>(items_json);
        //    items_json = "";
        //}
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string eway_json
        {
            get { return Newtonsoft.Json.JsonConvert.SerializeObject(eway); }
            set { eway = Newtonsoft.Json.JsonConvert.DeserializeObject<EwayInfo>(value); }
        }
        public EwayInfo eway { get; set; }
        public bool hasewb
        {
            get
            {
                return !string.IsNullOrEmpty(ewb);
            }
        }

        public long actionby { get; set; }
        public DateTime actionon { get; set; }
        public string actionemployee { get; set; }
        public bool bundleitemchanged { get; set; }
        public class EwayInfo
        {
            public long ewayBillNo { get; set; }
            public DateTime ewayBillDate { get; set; }
            public string validUpto { get; set; }
            public string alert { get; set; }
            public string lrno { get; set; }
            public DateTime lrdate { get; set; }
            public long tocityid { get; set; }
            public int noofpieces { get; set; }
            public int noofbundles { get; set; }
            public int scanned { get; set; }
            public int distance { get; set; }
            public string billimg { get; set; }
        }

        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string packslip_json
        {
            get { return Newtonsoft.Json.JsonConvert.SerializeObject(packslip); }
            set { packslip = Newtonsoft.Json.JsonConvert.DeserializeObject<List<PackSlip>>(value); }
        }
        public List<PackSlip> packslip { get; set; }
        public class PackSlip
        {
            public long id { get; set; }
            public long createdby { get; set; }
            public long modifiedby { get; set; }
            public string code { get; set; }
            public DateTime createdon { get; set; }
            public DateTime modifiedon { get; set; }
            public List<PackslipItem> items { get; set; }
            public long inwardby { get;  set; }
            public DateTime inwardon { get;  set; }
            public bool scanned { get; set; }
            public class PackslipItem
            {
                public string barcode { get; set; }
                public decimal qty { get; set; }
                public decimal aqty { get; set; }
            }
        }
    }

    public class BundleDetail : Bundles
    {
        public string fromgst { get; set; }
        public string togst { get; set; }
        public string fromlocation { get; set; }
        public string tolocation { get; set; }
        public bool allowed2createbill
        {
            get
            {
                if(!string.IsNullOrEmpty(fromgst) &&
                   !string.IsNullOrEmpty(togst))
                {
                    return fromgst.Trim() != togst.Trim() || iscompanytransfer;
                }
                return iscompanytransfer;
            }
        }
        public bool allowedigstbill
        {
            get
            {
                if (!string.IsNullOrEmpty(fromgst) &&
                   !string.IsNullOrEmpty(togst))
                {
                    return fromgst.Trim().Substring(0, 2) != togst.Trim().Substring(0, 2);
                }
                return false;
            }
        }

        public decimal itemsvalue { get;  set; }
        public string lrno { get;  set; }
        public DateTime lrdate { get;  set; }
        public string einv { get;  set; }
        public decimal scanned { get;  set; }
        public decimal noofpieces { get;  set; }
        public decimal noofbundles { get;  set; }
    }
    public class BundleTransport : BaseEntity
    {
        public int modeofdelivery { get; set; }
        public string vehicleno { get; set; }
        public long transportid { get; set; }
        public string ewb { get; set; }
        public long id { get; set; }
        public bool hsn8digit { get; set; }
        public bool hsn4digit { get; set; }
        public bool hsn6digit { get; set; }
        public string lrno { get; set; }
        public DateTime lrdate { get; set; }
        public long tocityid { get; set; }
        public int noofpieces { get; set; }
        public int noofbundles { get; set; }
        public long packedby { get; set; }
        public DateTime packedon { get; set; }
        public bool geneway { get; set; }
        public int distance { get; set; }
        public string billimg { get; set; }
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string eway_json
        {
            get { return Newtonsoft.Json.JsonConvert.SerializeObject(eway); }
            set { eway = Newtonsoft.Json.JsonConvert.DeserializeObject<Bundles.EwayInfo>(value); }
        }
        public Bundles.EwayInfo eway { get; set; }

        public string subSupplyDesc { get; set; }
        public string subSupplyType { get; set; }
    }

    public class StockOrderHistory : BaseEntity
    {
        public string stouniquenumber { get; set; }
        public string supplier { get; set; }
        public string remark { get; set; }
        public long id { get; set; }
        public DateTime podate { get; set; }
        public string stocode { get; set; }

        public decimal amount { get; set; }
        public decimal qty { get; set; }
        public string tradeagreementcode { get; set; }
        public long tradeagreementid { get; set; }
        public Bundles order { get; set; }
    }
    public class BundlesCreteria : Bundles
    {
        public bool bystatusid { get; set; }
        public int statusid { get; set; }
        public bool locationbasedsearch { get; set; }
        public bool fromwarehouseoutward { get; set; }
        public bool fromshowroom { get; set; }
        public bool validatetolocation { get; set; }
        public bool validatefromlocation { get; set; }
        public bool fromwarehousereturn { get; set; }
        public DateTime fromdate { get; set; }
        public DateTime todate { get; set; }
        public string ErrorMessage { get; set; }
        public List<long> ids { get; set; }
        public bool includeitems { get; set; }
        public string packingcode { get; set; }
    }

    public class STOUANUploaded : BaseEntity
    {
        public int noofsto { get; set; }
        public int validsto { get; set; }
        public long companyid { get; set; }
        public List<string> missingqty { get; set; }
        public List<string> missinguan { get; set; }
        public List<Order> orders { get; set; }
        public List<string> missingcompany { get; set; }
        public List<string> missinglocation { get; set; }
        public bool ShowBundleCode
        {
            get
            {
                return orders != null &&
                    orders.Any(x => !string.IsNullOrEmpty(x.BundleCode));
            }
        }

        public List<SOUANUploaded.StockInfo> stocklist { get; set; }

        public class OrderItem
        {
            public long ItemID { get; set; }
            public long ProductID { get; set; }
            public long ColourID { get; set; }
            public decimal MRP { get; set; }
            public long SizeID { get; set; }
            public string Design { get; set; }
            public string Product { get; set; }
            public String Size { get; set; }
            public decimal Qty { get; set; }
            public decimal Cost { get; set; }
            public string UAN { get; set; }
            public string HSN { get; set; }
            public decimal Percentage { get; set; }
            public decimal Tax { get; set; }
            public long TaxID { get; set; }
            public decimal Tolerance { get; set; }
        }
        public class Order
        {
            public decimal Tax { get; set; }

            public string Remark { get; set; }
            public string OrderSplit { get; set; }
            public DateTime STODate { get; set; }
            public string Company { get; set; }
            public string FromLocation { get; set; }
            public string ToLocation { get; set; }
            public List<OrderItem> items { get; set; }
            public long fromlocationid { get; set; }
            public long tolocationid { get; set; }
            public decimal Qty { get; set; }
            public bool stocknotavailable { get; set; }
            public List<string> MissingUAN { get; set; }
            public bool valid
            {
                get
                {
                    return fromlocationid > 0 &&
                        tolocationid > 0 &&
                        !stocknotavailable &&
                        STODate != DateTime.MinValue &&
                        MissingUAN.Count == 0 &&
                        string.IsNullOrEmpty(BundleCode);
                }
            }
            public int count
            {
                get
                {
                    return items.Count;
                }
            }

            public decimal Amount { get; set; }
            public long tocompanyid { get; set; }
            public long fromcompanyid { get; set; }
            public string BundleCode { get; set; }
            public string category { get; internal set; }
            public string rule { get; internal set; }
            public decimal margin { get; internal set; }
            public bool costprice { get; internal set; }
        }
    }

    public class Bundleitem : BaseEntity
    {
        public long productid { get; set; }
        public decimal qty { get; set; }
        public decimal rqty { get; set; }
        public decimal transfered { get; set; }
        public decimal damagedqty { get; set; }
        public decimal journalqty { get; set; }
        public decimal iqty { get; set; }
        public decimal aqty { get; set; }
        public long stockid { get; set; }
        public long supplierid { get; set; }
        public string barcode { get; set; }
        public string sizename { get; set; }
        public long itemid { get; set; }
        public string lrreference { get; set; }
        public string suppliername { get; set; }
        public string printingname { get; set; }
        public long id { get; set; }
        public bool ismeter { get; set; }
        public decimal pqty { get; set; }
        public string location { get; set; }
        public long toid { get; set; }
        public decimal buyingprice { get; set; }
        public decimal price { get; set; }
        public decimal uprice { get; set; }
        public string serialno { get; set; }
        public string designid { get; set; }
        public decimal mrp { get; set; }
        [JsonIgnore]
        public List<SalesorderitemsDetail.SizeQty> jumpqty { get; set; }
        [JsonIgnore]
        public string rackno { get; set; }
        [JsonIgnore]
        public string design { get; set; }
        [JsonIgnore]
        public string colourcode { get; set; }
        [JsonIgnore]
        public string colourname { get; set; }
        [JsonIgnore]
        public string hsn { get; set; }
        [JsonIgnore]
        public string product { get; set; }
        [JsonIgnore]
        public string brand { get; set; }
        [JsonIgnore]
        public int sno { get; set; }
        [JsonIgnore]
        public decimal totalbuyingcost
        {
            get
            {
                return qty * buyingprice;
            }
        }
        [JsonIgnore]
        public decimal totalprice
        {
            get
            {
                return qty * price;
            }
        }
        [JsonIgnore]
        public decimal totalmrp
        {
            get
            {
                return qty * mrp;
            }
        }
        [JsonIgnore]
        public MfgDesignItem mfgdesign { get; set; }
        [JsonIgnore]
        public decimal taxpercentage { get; set; }
        [JsonIgnore]
        public decimal tax { get; set; }
        [JsonIgnore]
        public decimal netprice
        {
            get
            {
                return totalprice + tax;
            }
        }
        [JsonIgnore]
        public decimal stqty { get; set; }
    }

    public class BundleItemPrife : BaseEntity
    {
        public long id { get; set; }
        public decimal price { get; set; }
        public bool exist { get; set; }
    }

    public class BundleStockCreteria : BaseEntity
    {
        public enum SearchType
        {
            None = 0,
            Brand = 1,
            Product = 2,
            BarcodedOn = 3,
            AllStock = 4,
            Barcode = 5,
            LREntry = 6,
            Supplier = 7,
            DesignNo = 9
        }
        public List<long> exclusionlist { get; set; }
        public long locationid { get; set; }
        public string fileid { get; set; }
        public DateTime date { get; set; }
        public long productid { get; set; }
        public long brandid { get; set; }
        public string barcode { get; set; }
        public long companyid { get; set; }
        public int searchtype { get; set; }
        public string lrentryno { get; set; }
        public string designcode { get; set; }
        public long retaillocationid { get; set; }
        public long supplierid { get; set; }
        public SearchType Type
        {
            get
            {
                return (SearchType)searchtype;
            }
        }
        public List<DesignSizeQty> requiredesignqty { get; set; }
        [JsonIgnore]
        public bool removelimit { get; set; }

        public class DesignSizeQty
        {
            public string name { get; set; }
            public long id { get; set; }
            public decimal qty { get; set; }
        }
    }

    public class BundleItemTransferHistory
    {
        public string code { get; set; }
        public long fromlocationid { get; set; }
        public long tolocationid { get; set; }
        public DateTime packedon { get; set; }
        public DateTime requesteddate { get; set; }
        public DateTime receivedon { get; set; }
        public decimal qty { get; set; }
        public decimal transferedqty { get; set; }
        public long id { get; set; }
        public bool returngoods { get; set; }
    }

    public class BundlesPrint : Bundles
    {
        public bool itemsum { get; set; }
        public decimal piece
        {
            get
            {
                if (items.Sum(e => e.aqty) > 0)
                    return items.Sum(e => e.ismeter ? 1 : e.aqty);
                else
                    return items.Sum(e => e.ismeter ? 1 : e.iqty);
            }
        }
        public decimal netamount
        {
            get
            {
                if (itemsum)
                    return lstoutwarditems != null ? lstoutwarditems.Sum(e => e.buyingprice) : 0;
                return groupitems != null ? groupitems.Sum(e => e.taxable) : 0;
            }
        }
        public decimal tax
        {
            get
            {
                if (itemsum)
                    return lstoutwarditems != null ? lstoutwarditems.Sum(e => e.taxamount) : 0;
                return groupitems != null ? groupitems.Sum(e => e.tax) : 0;
            }
        }
        public decimal grandtotal
        {
            get
            {
                if (itemsum)
                    return lstoutwarditems != null ? Math.Round(lstoutwarditems.Sum(e => e.ibuyingprice)) : 0;
                return groupitems != null ? Math.Round(groupitems.Sum(e => e.amount)) : 0;
            }
        }
        public decimal rounding
        {
            get
            {
                if (itemsum)
                    return lstoutwarditems != null ? (Math.Round(lstoutwarditems.Sum(e => e.ibuyingprice)) - lstoutwarditems.Sum(e => e.ibuyingprice)) : 0;
                return groupitems != null ? (Math.Round(groupitems.Sum(e => e.amount)) - groupitems.Sum(e => e.amount)) : 0;
            }
        }


        public decimal lstitemsnetamount
        {
            get
            {
                return lstoutwarditems != null ? lstoutwarditems.Sum(e => e.buyingprice * e.qty) : 0;
            }
        }
        public decimal lstitemstax
        {
            get
            {
                return lstoutwarditems != null ? lstoutwarditems.Sum(e => e.taxamount * e.qty) : 0;
            }
        }
        public decimal lstitemsgrandtotal
        {
            get
            {
                return lstoutwarditems != null ? Math.Round(lstitemsnetamount + lstitemstax) : 0;
            }
        }


        public string inwords { get; set; }
        public Company company { get; set; }
        public CompanyLocation fromlocation { get; set; }
        public CompanyLocation tolocation { get; set; }
        public List<Printableitem> groupitems { get; set; }
        public List<ShortPrintableitem> shortgroupitems { get; set; }
        public List<InfyPOS.Data.OutwardReport> lstoutwarditems { get; set; }
        public bool isinterstatesale { get; set; }
        public decimal mfgtaxable
        {
            get
            {
                return items.Sum(x => x.totalprice);
            }
        }
        public decimal mfgnet
        {
            get
            {
                return items.Sum(x => x.netprice);
            }
        }
        public decimal mfgtax
        {
            get
            {
                return items.Sum(x => x.tax);
            }
        }
        public decimal mfgigst
        {
            get
            {
                if (!isinterstatesale) return 0;
                return items.Sum(x => x.tax);
            }
        }
        public string mfgsingletax
        {
            get
            {
                if (items.All(x => x.taxpercentage == items[0].taxpercentage))
                    return items[0].taxpercentage.ToString("F2");
                return "";
            }
        }
        public decimal mfgcgst
        {
            get
            {
                if (isinterstatesale) return 0;
                return items.Sum(x => x.tax) / 2;
            }
        }
        public decimal mfgsgst
        {
            get
            {
                if (isinterstatesale) return 0;
                return items.Sum(x => x.tax) / 2;
            }
        }
        public class ShortPrintableitem
        {
            public string groupedproductname { get; set; }
            public decimal qty { get; set; }
            public int sno { get; set; }
        }

        public List<HSNItem> HSNList { get; set; }
        public bool ispackingslip { get; internal set; }
        public decimal packedqty { get; internal set; }
        public decimal totalqty { get; internal set; }
        [JsonIgnore]
        public BundleDetail source { get; internal set; }
        public class HSNItem
        {
            public bool isinterstate { get; set; }
            public string code { get; set; }
            public decimal qty { get; set; }
            public decimal peice { get; set; }
            public decimal tax { get; set; }
            public decimal taxpercentage
            {
                get
                {
                    if (igst_percentage > 0) return igst_percentage;
                    return cgst_percentage * 2;
                }
            }
            public decimal taxable { get; set; }
            public decimal total { get; set; }
            public decimal cgst_percentage { get; set; }
            public decimal sgst_percentage { get; set; }
            public decimal igst_percentage { get; set; }
            public decimal cgst_value { get; set; }
            public decimal sgst_value { get; set; }
            public decimal igst_value { get; set; }
        }
        public class Printableitem
        {
            public string description { get; set; }
            public int sno { get; set; }
            public string hsn { get; set; }
            public string sizename { get; set; }
            public string colourname { get; set; }
            public string designid { get; set; }
            public string gln { get; set; }
            public string serialno { get; set; }
            public decimal costprice { get; set; }
            public decimal price { get; set; }
            public decimal discount { get; set; }
            public decimal mrp { get; set; }
            public string unit { get; set; }
            public decimal qty { get; set; }

            [JsonIgnore]
            public decimal skippedqty { get; set; }
            [JsonIgnore]
            public decimal requestedqty { get; set; }
            [JsonIgnore]
            public decimal issueddqty { get; set; }
            [JsonIgnore]
            public decimal packedqty { get; set; }
            public decimal taxable { get; set; }
            public decimal taxpercentage { get; set; }
            public decimal igst { get; set; }
            public decimal cgst { get; set; }
            public decimal sgst { get; set; }
            public decimal amount { get; set; }
            public decimal saleprice
            {
                get { return price - discount; }
            }
            public decimal tax
            {
                get
                {
                    return igst + sgst + cgst;
                }
            }
        }
    }

}

