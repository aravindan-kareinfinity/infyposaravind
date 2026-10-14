using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebAPI.Data;
namespace InfyPOS.Models
{
    public class Purchaseorder : BaseEntity
    {
        public long id { get; set; }
        public decimal tolerance { get; set; }
        public long agentid { get; set; }
        public long transportid { get; set; }
        public long shopcontactemployeeid { get; set; }
        public string employeeincharge { get; set; }
        public decimal discountallowed { get; set; }
        public string shopcontactnumber { get; set; }
        public string suppliername { get; set; }
        public string remarks { get; set; }
        public string pouniquenumber { get; set; }
        public string actionemployee { get; set; }
        public string createdemployee { get; set; }
        public int freight { get; set; }
        public long companyid { get; set; }
        public long locationid { get; set; }
        public long requestlocationid { get; set; }
        public DateTime podate { get; set; }
        public DateTime expectedate { get; set; }
        public DateTime expirydate { get; set; }
        public string pono { get; set; }
        public long potype { get; set; }
        public string invoiceno { get; set; }
        public long agreementpoid { get; set; }
        public string agreementpono { get; set; }
        public string potypename { get; set; }
        public string sizegroup { get; set; }
        public long supplierid { get; set; }
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string termsandcondition_json { get { return Newtonsoft.Json.JsonConvert.SerializeObject(termsandcondition); } set { termsandcondition = Newtonsoft.Json.JsonConvert.DeserializeObject<Termsandcondition>(value); } }
        public Termsandcondition termsandcondition { get; set; }

        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string additionalcharges_json { get { return Newtonsoft.Json.JsonConvert.SerializeObject(additionalcharges); } set { additionalcharges = Newtonsoft.Json.JsonConvert.DeserializeObject<List<AdditionalCharge>>(value); } }
        public List<AdditionalCharge> additionalcharges { get; set; }
        public bool isactive { get; set; }
        public bool inclusivetax { get; set; }
        public bool isinterstate { get; set; }
        public long createdby { get; set; }
        public long modifiedby { get; set; }
        public long actionby { get; set; }
        public int version { get; set; }
        public int mode { get; set; }
        public DateTime invoicedate { get; set; }
        public DateTime createdon { get; set; }
        public DateTime modifiedon { get; set; }
        public DateTime actionon { get; set; }
        [JsonIgnore]
        public byte[] attachment { get; set; }
        public List<PurchaseorderitemsDetail> Purchaseorderitems { get; set; }
        public class Termsandcondition
        {
        }

        public class AdditionalCharge
        {
            public long additionalchargeid { get; set; }
            public string name { get; set; }
            public bool taxcomponent { get; set; }
            public string hsn { get; set; }
            public long taxid { get; set; }
            public decimal value { get; set; }

        }
        public long deliverylocationid { get; set; }
        public POStatus status { get; set; }
        public enum POStatus
        {
            PENDING = 0,
            APPROVED = 1,
            REJECTED = 2,
            PARTIALLYCOMPLETED = 3,
            COMPLETED = 4,
            UNAPPROVED = 11
        }
        public InternalStatus internalstatus { get; set; }
        public enum InternalStatus
        {
            ALL = 0,
            APPROVED = 1,
            REJECTED = 2,
            FULLCOMPLETE = 3,
            GRN = 6,
            BILLED = 7,
            SCANNED = 8,
            WAREHOUSE = 9,
            PENDING = 10,
            UNAPPROVED = 11,
            COMPLETED = 12
        }
        public decimal balance { get; set; }
        public decimal price { get; set; }
        public decimal ordered { get; set; }
        public decimal received { get; set; }
        public decimal cancelled { get; set; }
        public decimal agreed { get; set; }
        public string designid { get; set; }
        public int overduedays { get; set; }
        public string suppliercity { get; set; }
        public long suppliercompanyid { get; set; }

        [JsonIgnore]
        public decimal TotalQty
        {
            get
            {
                return Purchaseorderitems == null ? 0 : Purchaseorderitems.Sum(x => x.oqty);
            }
        }
        [JsonIgnore]
        public int DesignCount
        {
            get
            {
                return Purchaseorderitems == null ? 0 : Purchaseorderitems.DistinctBy(x => x.designid).Count();
            }
        }
    }

    public class PurchaseOrderSummary : Purchaseorder
    {

    }

    public class PurchaseOrderHistory : BaseEntity
    {
        public string pouniquenumber { get; set; }
        public string supplier { get; set; }
        public string remark { get; set; }
        public long id { get; set; }
        public DateTime podate { get; set; }
        public string pono { get; set; }
        public decimal amount { get; set; }
        public decimal qty { get; set; }
        public string tradeagreementcode { get; set; }
        public long tradeagreementid { get; set; }
        public Purchaseorder order { get; set; }
    }

    public class PurchaseorderMobile : BaseEntity
    {
        public long locationid { get; set; }
        public string location { get; set; }
        public long transportid { get; set; }
        public string transportname { get; set; }
        public long id { get; set; }
        public string pono { get; set; }
        public DateTime podate { get; set; }
        public long supplierid { get; set; }
        public string supplier { get; set; }
        public string product { get; set; }
        public decimal qty { get; set; }
        public List<Item> items { get; set; }
        public PurchaseorderMobile()
        {

        }

        public PurchaseorderMobile(List<PurchaseOrderItemSimple> sourceList)
        {
            this.id = sourceList.First().poid;
            this.supplierid = sourceList.First().supplierid;
            this.pono = sourceList.First().pono;
            this.podate = sourceList.First().podate;
            this.supplier = sourceList.First().supplier;
            this.product = sourceList.First().product;
            this.transportname = sourceList.First().transportname;
            this.transportid = sourceList.First().transportid;
            this.location = sourceList.First().location;
            this.locationid = sourceList.First().locationid;
            this.qty = sourceList.Sum(e => e.qty);
            this.items = new List<Item>();
            var list = sourceList.GroupBy(e => e.size).ToList().ConvertAll(e => e.Key);
            if (!string.IsNullOrEmpty(sourceList.First().sizegroup))
            {
                var key = sourceList.First().sizegroup;
                var lst = WebAPI.Data.Utility.GetSizeGroups().Find(e => e.Item1 == key);
                if (lst != null)
                {
                    list = lst.Item2;
                }
            }
            if (list == null || list.Count <= 1)
            {
                list = sourceList.GroupBy(e => e.price).ToList().ConvertAll(e => e.Key.ToString("F0"));
                foreach (var item in sourceList.GroupBy(e => new { e.designid, e.productid, e.brandid }))
                {
                    var listitem = new Item()
                    {
                        cloneid = item.First().id,
                        price = item.First().price,
                        design = item.Key.designid,
                        items = list.ConvertAll<Item.order>(e => new Item.order()
                        {
                            name = e,
                            qty = 0
                        })
                    };
                    foreach (var oitem in item)
                    {
                        if (listitem.items.Exists(e => e.name == oitem.price.ToString("F0")))
                        {
                            listitem.items.Find(e => e.name == oitem.price.ToString("F0")).qty = oitem.qty;
                            listitem.items.Find(e => e.name == oitem.price.ToString("F0")).id = oitem.id;
                        }
                    }
                    this.items.Add(listitem);

                }
            }
            else
            {
                foreach (var item in sourceList.GroupBy(e => new { e.designid, e.productid, e.brandid }))
                {
                    var listitem = new Item()
                    {
                        cloneid = item.First().id,
                        design = item.Key.designid,
                        price = item.First().price,
                        items = list.ConvertAll<Item.order>(e => new Item.order()
                        {
                            name = e,
                            qty = 0
                        })
                    };
                    foreach (var oitem in item)
                    {
                        if (listitem.items.Exists(e => e.name == oitem.size))
                        {
                            listitem.items.Find(e => e.name == oitem.size).qty = oitem.qty;
                            listitem.items.Find(e => e.name == oitem.size).id = oitem.id;
                        }
                    }
                    this.items.Add(listitem);
                }
            }
        }
        public PurchaseorderMobile(Purchaseorder source)
        {
            this.id = source.id;
            this.pono = source.pono;
            this.podate = source.podate;
            this.supplierid = source.supplierid;
            this.items = new List<Item>();
            var list = source.Purchaseorderitems.GroupBy(e => e.size).ToList().ConvertAll(e => e.Key);
            list.RemoveAll(e => string.IsNullOrEmpty(e));
            if (!string.IsNullOrEmpty(source.sizegroup))
            {
                var key = source.sizegroup;
                var lst = WebAPI.Data.Utility.GetSizeGroups().Find(e => e.Item1 == key);
                if (lst != null)
                {
                    list = lst.Item2;
                }
            }

            if (list == null || list.Count <= 1)
            {
                list = source.Purchaseorderitems.GroupBy(e => e.price).ToList().ConvertAll(e => e.Key.ToString("F0"));
                foreach (var item in source.Purchaseorderitems.GroupBy(e => new { e.designid, e.productid, e.brandid }))
                {
                    var listitem = new Item()
                    {
                        cloneid = item.First().id,
                        design = item.Key.designid,
                        price = item.First().price,
                        items = list.ConvertAll<Item.order>(e => new Item.order()
                        {
                            name = e,
                            qty = 0
                        })
                    };
                    foreach (var oitem in item)
                    {
                        if (listitem.items.Exists(e => e.name == oitem.price.ToString("F0")))
                        {
                            listitem.items.Find(e => e.name == oitem.price.ToString("F0")).qty = oitem.oqty;
                            listitem.items.Find(e => e.name == oitem.price.ToString("F0")).id = oitem.id;
                        }
                    }
                    this.items.Add(listitem);

                }
            }
            else
            {
                foreach (var item in source.Purchaseorderitems.GroupBy(e => new { e.designid, e.price, e.productid, e.brandid }))
                {
                    var listitem = new Item()
                    {
                        cloneid = item.First().id,
                        design = item.Key.designid,
                        price = item.Key.price,
                        items = list.ConvertAll<Item.order>(e => new Item.order()
                        {
                            name = e,
                            qty = 0
                        })
                    };
                    foreach (var oitem in item)
                    {
                        listitem.items.Find(e => e.name == oitem.size).qty = oitem.oqty;
                        listitem.items.Find(e => e.name == oitem.size).id = oitem.id;
                    }
                    this.items.Add(listitem);
                }
            }
        }
        public class Item
        {
            public string design { get; set; }
            public decimal price { get; set; }
            public long cloneid { get; set; }
            public List<order> items { get; set; }
            public class order
            {
                public long id { get; set; }
                public string name { get; set; }
                public decimal qty { get; set; }
            }
        }
    }
    public class PurchaseorderCreteria : Purchaseorder
    {
        public long lrinvoiceid { get; set; }
        public bool ismobilepo { get; set; }
        public DateTime fromdate { get; set; }
        public DateTime todate { get; set; }
        public long productid { get; set; }
        public long brandid { get; set; }
        public int statusid { get; set; }
        public int limit { get; set; }
        public bool showdetail { get; set; }
        public long suppliercityid { get; set; }
        public bool isagreement { get; set; }
        public bool istradeagreement { get; set; }
        public long tradeagreementid { get; set; }
        public bool includeitems { get; set; }
        public List<long> ids { get;  set; }
    }

    public class PurchaseOrderItemDetail
    {
        public long productid { get; set; }
        public long purchaseorderitemsid { get; set; }
        public DateTime podate { get; set; }
        public long purchaseorderid { get; set; }
        public string pono { get; set; }
        public string description { get; set; }
        public string potypename { get; set; }
        public string imageid { get; set; }
        public decimal balance { get; set; }
        public string designid { get; set; }
        public decimal price { get; set; }
        public decimal sprice { get; set; }
        public decimal mrp { get; set; }
        public decimal order { get; set; }
        public decimal tolerance { get; set; }
        public decimal maxqty { get; set; }
        public long sizeid { get; set; }
        public long tradeagreementid { get; set; }
        public long itemid { get; set; }
        public long brandid { get; set; }
        public long styleid { get; set; }
        public long patternid { get; set; }
        public string gln { get; set; }
        public long materialid { get; set; }
        public long fitid { get; set; }
        public long sleeveid { get; set; }
        public long typeid { get; set; }

    }

    public class POUANUploaded : BaseEntity
    {
        public int noofpo { get; set; }
        public int validpo { get; set; }
        public long companyid { get; set; }
        public long deliverylocationid { get; set; }
        public List<string> missingsuppliers { get; set; }
        public List<string> missingqty { get; set; }
        public List<string> missinguan { get; set; }
        public bool ShowPONO
        {
            get
            {
                return orders != null &&
                    orders.Any(x => !string.IsNullOrEmpty(x.PONo));
            }
        }
        public List<Order> orders { get; set; }
        public List<string> missingcompany { get; internal set; }
        public List<string> missinglocation { get; internal set; }

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
            public string OrderType { get; set; }
            public string Remark { get; set; }
            public string OrderSplit { get; set; }
            public long supplierid { get; set; }
            public string Supplier { get; set; }
            public DateTime PODate { get; set; }
            public DateTime DueDate { get; set; }
            public DateTime ExpiryDate { get; set; }
            public string SupplierGST { get; set; }
            public string Company { get; set; }
            public string Location { get; set; }
            public string EntryLocation { get; set; }
            public string PONo { get; set; }
            public List<OrderItem> items { get; set; }
            public decimal Amount { get; set; }
            public decimal Tax { get; set; }
            public long enntrylocationid { get; set; }
            public long locationid { get; set; }
            public long companyid { get; set; }
            public decimal Qty { get; set; }
            public List<string> MissingUAN { get; set; }
            public bool valid
            {
                get
                {
                    return supplierid > 0 &&
                        locationid>0 &&
                        companyid>0 &&
                        PODate != DateTime.MinValue && 
                        string.IsNullOrEmpty(PONo) &&
                        MissingUAN.Count == 0;
                }
            }
            public int count
            {
                get
                {
                    return items.Count;
                }
            }

            [JsonIgnore]
            public string category { get; set; }
            [JsonIgnore]
            public string rule { get; set; }
            [JsonIgnore]
            public decimal margin { get; set; }
        }
    }
    public class PurchaseorderDetails
    {
        public long id { get; set; }
        public decimal tolerance { get; set; }
        public long agentid { get; set; }
        public int freight { get; set; }
        public long companyid { get; set; }
        public long locationid { get; set; }
        public long agreementpoid { get; set; }
        public DateTime podate { get; set; }
        public DateTime expectedate { get; set; }
        public DateTime expirydate { get; set; }
        public string pono { get; set; }
        public int potype { get; set; }
        public long supplierid { get; set; }
        public int mode { get; set; }
        public DateTime actionon { get; set; }
        public long deliverylocationid { get; set; }
        public long requestlocationid { get; set; }
        public POStatus status { get; set; }
        public enum POStatus
        {
            PENDING = 0,
            APPROVED = 1,
            REJECTED = 2
        }
        public decimal balance { get; set; }
        public decimal price { get; set; }
        public decimal ordered { get; set; }
        public decimal received { get; set; }
        public decimal cancelled { get; set; }
        public string designid { get; set; }
        public string printingname { get; set; }
        public string productname { get; set; }
        public string sizegroup { get; set; }
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
        public string description { get; set; }
        public decimal priceupto { get; set; }
        public string pricerange { get; set; }
        public string priceranges
        {
            get { return string.Concat(price, "-", priceupto); }
            set
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
    public class PurchaseorderPrint : Purchaseorder
    {
        public double duedays
        {
            get
            {
                if (expectedate > DateTime.MinValue && expectedate > podate)
                    return (expectedate.Date - podate.Date).TotalDays;
                return 0;
            }
        }
        public bool isnontextilepo { get; set; }
        public Company company { get; set; }
        public Supplier supplier { get; set; }
        public Suppliercompany suppliercompany { get; set; }
        public Transport transport { get; set; }
        public Agent agent { get; set; }
        public CompanyLocation deliverylocation { get; set; }
        public CompanyLocation requestlocation { get; set; }
        public string whocreated { get; set; }
        public string whoapproved { get; set; }
        public bool hasagent
        {
            get
            {
                if (agentid > 0)
                    return true;
                else
                    return false;
            }
        }
        public string freightdisplaytext
        {
            get
            {
                if (freight == 1)
                    return "TO PAY";
                else
                    return "PAID";
            }
        }
        public string potypename { get; set; }

        public string brandname
        {
            get
            {
                if (Purchaseorderitems.Count == 0) return "";
                return Purchaseorderitems.First().brandname;
            }
        }
        public bool hasbrandname
        {
            get
            {
                if (Purchaseorderitems.Count == 0) return false;
                var bid = Purchaseorderitems.First().brandid;
                return Purchaseorderitems.All(e => e.brandid == bid);
            }
        }

        public string shopcontactemployee { get; set; }

        public string amountinwords
        {
            get
            {
                if (Purchaseorderitems == null) return "";
                return WebAPI.Data.Numberconvertor.rupees(Purchaseorderitems.Sum(x=>x.Net));
            }
        }
        public decimal totaltaxable { get; set; }
        public decimal tax { get; set; }
        public decimal Net { get; set; }
        public List<HSNItem> HSNList { get; set; }
        public decimal Gross { get; internal set; }
        public string singletaxpercentage { get; internal set; }

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
    }

    public class MobilePurchaseOrder : PurchaseorderMobile
    {
        public bool clone { get; set; }
    }

    public class PurchaseOrderItemSimple : BaseEntity
    {
        public int version { get; set; }
        public long cloneid { get; set; }
        public long id { get; set; }
        public long locationid { get; set; }
        public string location { get; set; }
        public long companyid { get; set; }
        public long supplierid { get; set; }
        public long transportid { get; set; }
        public long productid { get; set; }
        public long brandid { get; set; }
        public long poid { get; set; }
        public long sizeid { get; set; }
        public long colourid { get; set; }
        public long sizegroupid { get; set; }
        public decimal qty { get; set; }
        public decimal price { get; set; }
        public string designid { get; set; }
        public long designuid { get; set; }
        public string imageid { get; set; }
        public string supplier { get; set; }
        public string product { get; set; }
        public string brand { get; set; }
        public string pono { get; set; }
        public string transportname { get; set; }
        public string size { get; set; }
        public string colour { get; set; }
        public DateTime podate { get; set; }
        public string sizegroup { get; set; }
        public List<SizeItem> sizelist { get; set; }
        public class SizeItem
        {
            public long id { get; set; }
            public string name { get; set; }
            public decimal qty { get; set; }
        }
    }
}
