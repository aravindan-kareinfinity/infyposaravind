using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class Salesorder : BaseEntity
    {
        public long id { get; set; }
        public decimal tolerance { get; set; }
        public int sosourcetype { get; set; }
        public long salesmenid { get; set; }
        public string sobookno { get; set; }
        public string customerpo { get; set; }
        public decimal sobookqty { get; set; }
        public string remarks { get; set; }
        public string soracknumber { get; set; }
        public DateTime sobookdate { get; set; }
        public long agentid { get; set; }
        public int freight { get; set; }
        public long shiptocustomerid { get; set; }
        public long customerstateid { get; set; }
        public decimal discountpercentage { get; set; }
        public bool hasshipto { get; set; }
        public long companyid { get; set; }
        public long locationid { get; set; }
        public DateTime sodate { get; set; }
        public DateTime expectedate { get; set; }
        public string sono { get; set; }
        [JsonIgnore]
        public string box { get; set; }
        public string sizegroup { get; set; }
        public long customerid { get; set; }
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string termsandcondition_json { get { return Newtonsoft.Json.JsonConvert.SerializeObject(termsandcondition); } set { termsandcondition = Newtonsoft.Json.JsonConvert.DeserializeObject<Termsandcondition>(value); } }
        public Termsandcondition termsandcondition { get; set; }
        public bool isactive { get; set; }
        public long createdby { get; set; }
        public long modifiedby { get; set; }
        public long actionby { get; set; }
        public int version { get; set; }
        public int mode { get; set; }
        public DateTime createdon { get; set; }
        public DateTime modifiedon { get; set; }
        public DateTime actionon { get; set; }
        public List<SalesorderitemsDetail> Salesorderitems { get; set; }
        public List<SalesorderitemsDetail> Salesgroupitems { get; set; }
        public class Termsandcondition
        {
            public long pickedby { get; set; }
            public DateTime pickedon { get; set; }
            public long smapprovedby { get; set; }
            public DateTime smapprovedon { get; set; }
        }
        public long deliverylocationid { get; set; }
        public POStatus status { get; set; }
        public enum POStatus
        {
            PENDING = 0,
            APPROVED = 1,
            REJECTED = 2,
            CANCELLED = 3,
            COMPLETED = 4
        }
        public DateTime smapprovedon { get; set; }
        public long smapprovedby { get; set; }
        public decimal balance { get; set; }
        public decimal price { get; set; }
        public decimal ordered { get; set; }
        public decimal received { get; set; }
        public decimal cancelled { get; set; }
        public string designid { get; set; }
        public string customername { get; set; }
        public string cityname { get; set; }
        public string agentname { get; set; }
        public decimal totalqty { get; set; }
        public decimal totalsetqty { get; set; }
        public decimal plannedqty { get; set; }
        public string orderformimageid { get; set; }
        public decimal amounttotal
        {
            get
            {
                if (Salesorderitems != null && Salesorderitems.Count > 0)
                    return Salesorderitems.Sum(x => x.price * x.oqty);
                return 0;
            }
        }
        public decimal mrptotal
        {
            get
            {
                if (Salesorderitems != null && Salesorderitems.Count > 0)
                    return Salesorderitems.Sum(x => x.mrp * x.oqty);
                return 0;
            }
        }
        public long employeeid { get; set; }
        public bool isinterstate { get; set; }
        public mfglabelinfo mfglabel { get; set; }
        public class mfglabelinfo
        {
            public string entryno { get; set; }
            public DateTime entrydate { get; set; }
            public string designno { get; set; }
            public string samplename { get; set; }
            public string division { get; set; }
            public string merchant { get; internal set; }
            public string marketing { get; internal set; }
        }
    }

    public class SOTermsandcondition : BaseEntity
    {
        public List<long> ids { get; set; }
        public long pickedby { get; set; }
        public DateTime pickedon { get; set; }

        public Salesorder.Termsandcondition Copy
        {
            get
            {
                return new
                    Salesorder.Termsandcondition
                {
                    pickedby = this.pickedby,
                    pickedon = this.pickedon
                };
            }
        }
    }

    public class SalesorderSizeDetail : Salesorder
    {
        [JsonIgnore]
        public string sizeids { get; set; }
        public List<decimal> sz { get; set; }
        public List<SalesOrderSearchBySize.SizeQty> sizeqty { get; set; }
    }
    public class SalesOrderSearchBySize : BaseEntity
    {
        public SortedDictionary<long, string> sizenames { get; set; }
        public List<string> sizelist { get; set; }
        public List<long> sizeids { get; set; }
        public List<SalesorderSizeDetail> items { get; set; }

        public class SizeQty
        {
            public long id { get; set; }
            public decimal qty { get; set; }
        }
        public void ProcessSizeGroup()
        {
            foreach (var item in items)
            {
                List<SizeQty> sizeqty = new List<SizeQty>();
                foreach (var sitem in item.sizeids.Split(','))
                {
                    var parts = sitem.Split(':');
                    sizeqty.Add(new SizeQty() { id = long.Parse(parts[0]), qty = decimal.Parse(parts[1]) });
                }
                item.sizeqty = sizeqty.GroupBy(x => x.id).ToList().ConvertAll(y => new SizeQty()
                {
                    id = y.Key,
                    qty = y.Sum(z => z.qty)
                });
            }
            sizelist = new List<string>();
            foreach (var item in sizenames.Keys.ToList())
            {
                if (!items.Exists(x => x.sizeqty.Exists(y => y.id == item)))
                {
                    sizenames.Remove(item);
                }
                else
                {
                    sizelist.Add(sizenames[item]);
                }
            }
        }
        public void CompressSizeQty(List<string> groupsizelist)
        {
            sizeids = new List<long>();
            this.sizelist = groupsizelist;
            groupsizelist.RemoveAll(x => !sizenames.ToList().Exists(y => y.Value == x));
            foreach (var item in groupsizelist)
            {
                sizeids.Add(sizenames.First(x => x.Value == item).Key);
            }
            foreach (var item in items)
            {
                item.sz = new List<decimal>();
                foreach (var sz in sizeids)
                {
                    var index = item.sizeqty.FindIndex(x => x.id == sz);
                    item.sz.Add(index >= 0 ? item.sizeqty[index].qty : 0);
                }
                item.sizeqty = null;
                item.sizeids = null;
            }
        }
    }

    public class SalesOrderHistory : BaseEntity
    {
        public string customer { get; set; }
        public string remark { get; set; }
        public long id { get; set; }
        public DateTime sodate { get; set; }
        public string sono { get; set; }
        public decimal amount { get; set; }
        public decimal qty { get; set; }
        public string tradeagreementcode { get; set; }
        public long tradeagreementid { get; set; }
    }
    public class SOUANUploaded : BaseEntity
    {
        public int noofso { get; set; }
        public int validso { get; set; }
        public long companyid { get; set; }
        public long deliverylocationid { get; set; }
        public List<string> missingcustomers { get; set; }
        public List<string> missinguan { get; set; }
        public List<string> missingmargin { get; set; }
        public List<StockInfo> stocklist { get; set; }
        public List<Order> orders { get; set; }
        public bool ShowSONO
        {
            get
            {
                return orders != null &&
                    orders.Any(x => !string.IsNullOrEmpty(x.SONo));
            }
        }

        public class StockInfo
        {
            public string uan { get; set; }
            public decimal stock { get; set; }
            public decimal so { get; set; }
            public decimal sto { get; set; }
            public decimal required { get; set; }
            public List<string> orders { get; internal set; }
        }

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
            public decimal Price { get; set; }
            public long TaxID { get; set; }
        }
        public class Order
        {
            public string OrderType { get; set; }
            public string Remark { get; set; }
            public string OrderSplit { get; set; }
            public string BookingNo { get; set; }
            public string Company { get; set; }
            public long customerid { get; set; }
            public bool missingmargin
            {
                get
                {
                    return margin <= 0 && !manualprice;
                }
            }
            public string Location { get; set; }
            public string Customer { get; set; }
            public DateTime SODate { get; set; }
            public DateTime DueDate { get; set; }
            public DateTime ExpiryDate { get; set; }
            public string CustomerGST { get; set; }
            public decimal Tolerance { get; set; }
            public List<OrderItem> items { get; set; }
            public decimal Amount { get; set; }
            public decimal Tax { get; set; }
            public decimal Qty { get; set; }
            public int count
            {
                get
                {
                    return items.Count;
                }
            }
            public List<string> MissingUAN { get; set; }
            public List<string> MissingPrice { get; set; }
            public bool valid
            {
                get
                {
                    return customerid > 0 &&
                        locationid > 0 &&
                        companyid > 0 &&
                        !missingmargin && 
                        !nostock &&
                        string.IsNullOrEmpty(SONo) &&
                        MissingUAN.Count == 0;
                }
            }
            public string FailReason
            {
                get
                {
                    List<string> reasons = new List<string>();
                    if (customerid <= 0)
                        reasons.Add("Customer Missing");
                    if (locationid <= 0)
                        reasons.Add("Location Missing");
                    if (companyid <= 0)
                        reasons.Add("Company Missing");
                    if (missingmargin)
                        reasons.Add("Margin Missing");
                    if (nostock)
                        reasons.Add("Not enough stock");
                    if(!string.IsNullOrEmpty(SONo))
                        reasons.Add("Already Exist");
                    if (MissingUAN.Count > 0)
                        reasons.Add("Barcode Missing - "+string.Join(",",MissingUAN));
                    return string.Join("," , reasons);
                }
            }
            public string SONo { get; set; }
            public long companyid { get; set; }
            public long locationid { get; set; }
            public string category { get; set; }
            public decimal margin { get; set; }
            public bool manualprice { get; set; }
            public string rule { get; set; }
            public bool nostock { get; set; }
        }
    }
    public class SalesorderMobile : BaseEntity
    {
        public long id { get; set; }
        public string pono { get; set; }
        public string mobileno { get; set; }
        public DateTime podate { get; set; }
        public long customerid { get; set; }
        public string customer { get; set; }
        public string product { get; set; }
        public decimal qty { get; set; }
        public List<Item> items { get; set; }
        public SalesorderMobile()
        {

        }

        public SalesorderMobile(List<SalesorderItemSimple> sourceList,
            bool sizebased,
            List<string> sizelist,
            List<ReferenceItem> sizeInfos)
        {
            this.id = sourceList.First().soid;
            this.customerid = sourceList.First().customerid;
            this.mobileno = sourceList.First().mobileno;
            this.pono = sourceList.First().sono;
            this.podate = sourceList.First().sodate;
            this.customer = sourceList.First().customer;
            this.product = sourceList.First().product;
            this.qty = sourceList.Sum(e => e.qty);
            this.items = new List<Item>();

            if (sizebased)
            {
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
                    foreach (var item in sourceList.GroupBy(e => new { e.designid, e.designitemcode, e.sizegroup, e.productid, e.brandid }))
                    {
                        var listitem = new Item()
                        {
                            cloneid = item.First().id,
                            design = item.Key.designitemcode,
                            sizegroup = item.Key.sizegroup,
                            //designitemcode = item.Key.designitemcode,
                            items = list.ConvertAll<Item.order>(e => new Item.order()
                            {
                                name = e,
                                qty = 0,
                                id = sizeInfos.Exists(x => x.name == e) ? sizeInfos.Find(x => x.name == e).id : 0,

                                status = SalesorderItemSimple.Status.Undelivered
                            })
                        };
                        foreach (var oitem in item)
                        {
                            if (listitem.items.Exists(e => e.name == oitem.price.ToString("F0")))
                            {
                                listitem.items.Find(e => e.name == oitem.price.ToString("F0")).status = oitem.status;
                                listitem.items.Find(e => e.name == oitem.price.ToString("F0")).qty = oitem.qty;
                                listitem.items.Find(e => e.name == oitem.price.ToString("F0")).id = oitem.id;
                            }
                        }
                        this.items.Add(listitem);
                    }
                }
                else
                {
                    foreach (var item in sourceList.GroupBy(e => new { e.designid, e.designitemcode, e.productid, e.brandid, e.sizegroup }))
                    {
                        var listitem = new Item()
                        {
                            cloneid = item.First().id,
                            design = item.Key.designitemcode,
                            sizegroup = item.Key.sizegroup,
                            //designitemcode = item.Key.designitemcode,
                            items = list.ConvertAll<Item.order>(e => new Item.order()
                            {
                                name = e,
                                qty = 0,
                                id = sizeInfos.Exists(x => x.name == e) ? sizeInfos.Find(x => x.name == e).id : 0,
                                status = SalesorderItemSimple.Status.Undelivered
                            })
                        };
                        foreach (var oitem in item)
                        {
                            if (listitem.items.Exists(e => e.name == oitem.size))
                            {
                                listitem.items.Find(e => e.name == oitem.size).status = oitem.status;
                                listitem.items.Find(e => e.name == oitem.size).qty = oitem.qty;
                                listitem.items.Find(e => e.name == oitem.size).id = oitem.id;
                            }
                        }
                        this.items.Add(listitem);
                    }
                }
            }
            else
            {
                if (sizelist == null)
                    sizelist = sourceList.GroupBy(e => e.size).ToList().ConvertAll(e => e.Key);
                foreach (var item in sourceList.GroupBy(e => new
                {
                    e.designid,
                    e.productid,
                    e.brandid,
                    e.designitemcode
                }))
                {
                    var listitem = new Item()
                    {
                        cloneid = item.First().id,
                        design = item.Key.designitemcode,
                        //designitemcode = item.Key.designitemcode,
                        items = sizelist.ConvertAll<Item.order>(e => new Item.order()
                        {
                            name = e,
                            qty = 0,
                            id = sizeInfos.Exists(x => x.name == e) ? sizeInfos.Find(x => x.name == e).id : 0,
                            status = SalesorderItemSimple.Status.Undelivered
                        })
                    };
                    foreach (var oitem in item.GroupBy(sg => sg.size))
                    {
                        SalesorderItemSimple.Status status = SalesorderItemSimple.Status.Undelivered;
                        if (oitem.All(e => e.status == SalesorderItemSimple.Status.FullCancelled))
                            status = SalesorderItemSimple.Status.FullCancelled;
                        else if (oitem.All(e => e.status == SalesorderItemSimple.Status.FullDelivery))
                            status = SalesorderItemSimple.Status.FullDelivery;
                        else if (oitem.All(e => e.status == SalesorderItemSimple.Status.FullPlanned))
                            status = SalesorderItemSimple.Status.FullPlanned;
                        else if (oitem.All(e => e.status == SalesorderItemSimple.Status.PartialPlanned))
                            status = SalesorderItemSimple.Status.PartialPlanned;
                        else if (oitem.All(e => e.status == SalesorderItemSimple.Status.PartialDelivery))
                            status = SalesorderItemSimple.Status.PartialDelivery;
                        else if (oitem.All(e => e.status == SalesorderItemSimple.Status.PartialCancelled))
                            status = SalesorderItemSimple.Status.PartialCancelled;
                        if (!listitem.items.Exists(e => e.name == oitem.Key))
                        {
                            listitem.items.Add(new Item.order()
                            {
                                name = oitem.Key,
                                id = sizeInfos.Exists(x => x.name == oitem.Key) ? sizeInfos.Find(x => x.name == oitem.Key).id : 0,
                                qty = oitem.Sum(e => e.qty),
                                status = status
                            });
                        }
                        else
                        {
                            listitem.items.Find(e => e.name == oitem.Key).qty = oitem.Sum(e => e.qty);
                            listitem.items.Find(e => e.name == oitem.Key).status = status;
                        }
                    }
                    this.items.Add(listitem);
                }
            }
        }
        public SalesorderMobile(Salesorder source, List<string> list, List<ReferenceItem> sizeInfos)
        {
            this.id = source.id;
            this.pono = source.sono;
            this.podate = source.sodate;
            this.customerid = source.customerid;
            this.items = new List<Item>();

            if (list == null || list.Count == 0)
                list = source.Salesorderitems.GroupBy(e => e.size).ToList().ConvertAll(e => e.Key);

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
                list = source.Salesorderitems.GroupBy(e => e.price).ToList().ConvertAll(e => e.Key.ToString("F0"));
                foreach (var item in source.Salesorderitems.GroupBy(e => new { e.designid, e.designitemcode, e.productid, e.brandid, e.sizegroup }))
                {
                    var listitem = new Item()
                    {
                        cloneid = item.First().id,
                        design = item.Key.designitemcode,
                        sizegroup = item.Key.sizegroup,
                        items = list.ConvertAll<Item.order>(e => new Item.order()
                        {
                            name = e,
                            qty = 0,
                            id = sizeInfos.Exists(x => x.name == e) ? sizeInfos.Find(x => x.name == e).id : 0,
                            status = SalesorderItemSimple.Status.Undelivered
                        })
                    };
                    foreach (var oitem in item)
                    {
                        if (listitem.items.Exists(e => e.name == oitem.price.ToString("F0")))
                        {
                            listitem.items.Find(e => e.name == oitem.price.ToString("F0")).qty = oitem.oqty;
                            listitem.items.Find(e => e.name == oitem.price.ToString("F0")).id = oitem.id;
                            listitem.items.Find(e => e.name == oitem.price.ToString("F0")).status = oitem.status;
                        }
                    }
                    this.items.Add(listitem);

                }
            }
            else
            {
                foreach (var item in source.Salesorderitems.GroupBy(e => new { e.designid, e.designitemcode, e.price, e.productid, e.brandid, e.sizegroup, }))
                {
                    var listitem = new Item()
                    {
                        cloneid = item.First().id,
                        design = item.Key.designitemcode,
                        price = item.Key.price,
                        sizegroup = item.Key.sizegroup,
                        items = list.ConvertAll<Item.order>(e => new Item.order()
                        {
                            name = e,
                            qty = 0,
                            id = sizeInfos.Exists(x => x.name == e) ? sizeInfos.Find(x => x.name == e).id : 0,
                            status = SalesorderItemSimple.Status.Undelivered,

                        })
                    };
                    foreach (var oitem in item)
                    {
                        if (listitem.items.Exists(e => e.name == oitem.size))
                        {
                            listitem.items.Find(e => e.name == oitem.size).qty = oitem.oqty;
                            listitem.items.Find(e => e.name == oitem.size).id = oitem.id;
                            listitem.items.Find(e => e.name == oitem.size).status = oitem.status;
                        }
                    }
                    this.items.Add(listitem);
                }
            }
        }
        public class Item
        {
            //public string designitemcode { get; set; }
            public string design { get; set; }
            public decimal price { get; set; }
            public long cloneid { get; set; }
            public SalesorderItemSimple.Status status { get; set; }
            public List<order> items { get; set; }
            public string sizegroup { get; set; }

            public class order
            {
                public long id { get; set; }
                public string name { get; set; }
                public decimal qty { get; set; }
                public SalesorderItemSimple.Status status { get; set; }
            }
        }
    }

    public class SalesOrderCatalog : BaseEntity
    {
        public Salesorder currentorder { get; set; }
        public long customerid { get; set; }
        public long agentid { get; set; }
        public long salesmenid { get; set; }
        public long companyid { get; set; }
        public long soboxid { get; set; }
        public string remarks { get; set; }
        public string sizegroup { get; set; }
        public long deliverylocationid { get; set; }
        public List<Salesorderitems> orderitems { get; set; }
    }
    public class SalesorderCreteria : Salesorder
    {
        public bool unpacked { get; set; }
        public long cityid { get; set; }
        public bool ismobilepo { get; set; }
        public DateTime fromdate { get; set; }
        public DateTime todate { get; set; }
        public bool onlyheader { get; set; }
        public DateTime expectedatefrom { get; set; }
        public DateTime expectedateto { get; set; }

        public long productid { get; set; }
        public int statusid { get; set; }
        public int limit { get; set; }
        public bool showsize { get; set; }
        public List<long> ids { get; set; }
    }

    public class SalesorderItemDetail
    {
        public long Salesorderitemsid { get; set; }
        public DateTime sodate { get; set; }
        public long Salesorderid { get; set; }
        public string sono { get; set; }
        public string description { get; set; }
        public string imageid { get; set; }
        public decimal balance { get; set; }
        public string designid { get; set; }
        public decimal price { get; set; }
        public decimal order { get; set; }
        public decimal tolerance { get; set; }
        public decimal maxqty { get; set; }
    }
    public class SalesorderDetails
    {
        public long id { get; set; }
        public decimal tolerance { get; set; }
        public long agentid { get; set; }
        public int freight { get; set; }
        public long companyid { get; set; }
        public long locationid { get; set; }
        public DateTime sodate { get; set; }
        public DateTime expectedate { get; set; }
        public string sono { get; set; }
        public long customerid { get; set; }
        public int mode { get; set; }
        public DateTime actionon { get; set; }
        public long deliverylocationid { get; set; }
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
    public class SalesorderPrint : Salesorder
    {
        public Employee salesmen { get; set; }

        public double duedays
        {
            get
            {
                if (expectedate > DateTime.MinValue && expectedate > sodate)
                    return (expectedate.Date - sodate.Date).TotalDays;
                return 0;
            }
        }

        public Company company { get; set; }
        public Customer customer { get; set; }
        public Customer shiptocustomer { get; set; }
        public Transport transport { get; set; }
        public Agent agent { get; set; }
        [JsonIgnore]
        public bool hasremark
        {
            get
            {
                return !string.IsNullOrEmpty(remarks);
            }
        }
        [JsonIgnore]
        public bool hasbooking
        {
            get
            {
                return sobookdate != DateTime.MinValue && !string.IsNullOrEmpty(sobookno);
            }
        }
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

        public decimal Gross
        {
            get
            {
                if (Salesorderitems == null) return 0;
                return Salesorderitems.Sum(x => x.pricewithadditional * x.oqty);
            }
        }
        public decimal Tax
        {
            get
            {
                if (Salesorderitems == null) return 0;
                return Salesorderitems.Sum(x => x.tax);
            }
        }
        public decimal Net
        {
            get
            {
                if (Salesorderitems == null) return 0;
                return Salesorderitems.Sum(x => x.Net);
            }
        }
        public string amountinwords
        {
            get
            {
                if (Salesorderitems == null) return "";
                return WebAPI.Data.Numberconvertor.rupees(Salesorderitems.Sum(x => x.Net));
            }
        }

        public decimal errtotalqty { get; set; }
        [JsonIgnore]
        public CompanyLocation deliverylocation { get; set; }
        [JsonIgnore]
        public string preparedby { get; set; }

        public string singletaxpercentage { get; internal set; }
        public decimal stax { get; internal set; }
        public decimal itax { get; internal set; }
        public decimal ctax { get; internal set; }
        public decimal itaxpercentage { get; internal set; }
        public decimal ctaxpercentage { get; internal set; }
        public decimal staxpercentage { get; internal set; }

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

    public class SalesorderItemSimple : BaseEntity
    {
        public enum Status
        {
            Undelivered,
            PartialCancelled,
            FullCancelled,
            PartialPlanned,
            FullPlanned,
            PartialDelivery,
            FullDelivery
        }
        public Status status { get; set; }
        public int version { get; set; }
        public long cloneid { get; set; }
        public long id { get; set; }
        public long companyid { get; set; }
        public long addnlchargeid { get; set; }
        public long salesmenid { get; set; }
        public long agentid { get; set; }
        public long customerid { get; set; }
        public long productid { get; set; }
        public long brandid { get; set; }
        public long soid { get; set; }
        public long sizeid { get; set; }
        public long colourid { get; set; }
        public long sizegroupid { get; set; }
        public string uid { get; set; }
        public decimal qty { get; set; }
        public string designitemcode { get; set; }
        public decimal pqty { get; set; }
        public decimal cqty { get; set; }
        public decimal bqty { get; set; }
        public decimal rqty { get; set; }
        public decimal price { get; set; }
        public decimal mrp { get; set; }
        public string designid { get; set; }
        public long designuid { get; set; }
        public string imageid { get; set; }
        public string customer { get; set; }
        public string product { get; set; }
        public string brand { get; set; }
        public string sono { get; set; }
        public string mobileno { get; set; }
        public string size { get; set; }
        public DateTime sodate { get; set; }
        public string colour { get; set; }
        public string colourcode { get; set; }
        public string sizegroup { get; set; }
        public List<SizeItem> sizelist { get; set; }

        public MFGDesinInfo source { get; set; }
        public class SizeItem
        {
            public long id { get; set; }
            public string name { get; set; }
            public string uid { get; set; }
            public decimal qty { get; set; }
        }

    }
}
