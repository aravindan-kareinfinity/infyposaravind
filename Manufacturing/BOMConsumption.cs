using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class BOMConsumption : BaseEntity
    {
        public long id { get; set; }
        public decimal tolerance { get; set; }
        public long preparedby { get; set; }
        public long approvedby { get; set; }
        public long companyid { get; set; }
        public DateTime entrydate { get; set; }
        public DateTime expectedate { get; set; }
        public string entryno { get; set; }
        public string mode { get; set; }
        public string workorderno { get; set; }
        public string joborderno { get; set; }
        public string referenceno { get; set; }
        public long supplierid { get; set; }
        public bool isactive { get; set; }
        public long createdby { get; set; }
        public long modifiedby { get; set; }
        public int version { get; set; }
        public DateTime createdon { get; set; }
        public DateTime modifiedon { get; set; }
        public List<BOMConsumptionitemsDetail> BOMConsumptionitems { get; set; }
        public POStatus status { get; set; }
        public long mfgstageid { get; set; }
        public long bomrequestid { get; set; }
        public bool isrequest { get; set; }
        public enum POStatus
        {
            PENDING = 0,
            APPROVED = 1,
            REJECTED = 2
        }
        public long mfgworkorderid { get; set; }
        public long mfgjoborderid { get; set; }
        public string orderno { get; set; }
        public long locationid { get; set; }
        public DateTime orderdate { get; set; }
        public bool isoutward { get;  set; }
        public int consumptiontype { get; set; }
        public string bomentryno { get;  set; }
    }

    public class BOMConsumptionMobile : BaseEntity
    {
        public long id { get; set; }
        public string entryno { get; set; }
        public DateTime entrydate { get; set; }
        public long supplierid { get; set; }
        public string supplier { get; set; }
        public string product { get; set; }
        public decimal qty { get; set; }
        public List<Item> items { get; set; }
        public BOMConsumptionMobile()
        {

        }

        //public BOMConsumptionMobile(List<BOMConsumptionItemSimple> sourceList)
        //{
        //    this.id = sourceList.First().poid;
        //    this.supplierid = sourceList.First().supplierid;
        //    this.entryno = sourceList.First().entryno;
        //    this.entrydate = sourceList.First().entrydate;
        //    this.supplier = sourceList.First().supplier;
        //    this.product = sourceList.First().product;
        //    this.qty = sourceList.Sum(e => e.qty);
        //    this.items = new List<Item>();
        //    var list = sourceList.GroupBy(e => e.size).ToList().ConvertAll(e => e.Key);
        //    if (!string.IsNullOrEmpty(sourceList.First().sizegroup))
        //    {
        //        var key = sourceList.First().sizegroup;
        //        var lst = WebAPI.Data.Utility.GetSizeGroups().Find(e => e.Item1 == key);
        //        if (lst != null)
        //        {
        //            list = lst.Item2;
        //        }
        //    }
        //    if (list == null || list.Count <= 1)
        //    {
        //        list = sourceList.GroupBy(e => e.price).ToList().ConvertAll(e => e.Key.ToString("F0"));
        //        foreach (var item in sourceList.GroupBy(e => new { e.designid, e.productid, e.brandid }))
        //        {
        //            var listitem = new Item()
        //            {
        //                cloneid = item.First().id,
        //                design = item.Key.designid,
        //                items = list.ConvertAll<Item.order>(e => new Item.order()
        //                {
        //                    name = e,
        //                    qty = 0
        //                })
        //            };
        //            foreach (var oitem in item)
        //            {
        //                if (listitem.items.Exists(e => e.name == oitem.price.ToString("F0")))
        //                {
        //                    listitem.items.Find(e => e.name == oitem.price.ToString("F0")).qty = oitem.qty;
        //                    listitem.items.Find(e => e.name == oitem.price.ToString("F0")).id = oitem.id;
        //                }
        //            }
        //            this.items.Add(listitem);

        //        }
        //    }
        //    else
        //    {
        //        foreach (var item in sourceList.GroupBy(e => new { e.designid, e.productid, e.brandid }))
        //        {
        //            var listitem = new Item()
        //            {
        //                cloneid = item.First().id,
        //                design = item.Key.designid,
        //                items = list.ConvertAll<Item.order>(e => new Item.order()
        //                {
        //                    name = e,
        //                    qty = 0
        //                })
        //            };
        //            foreach (var oitem in item)
        //            {
        //                if (listitem.items.Exists(e => e.name == oitem.size))
        //                {
        //                    listitem.items.Find(e => e.name == oitem.size).qty = oitem.qty;
        //                    listitem.items.Find(e => e.name == oitem.size).id = oitem.id;
        //                }
        //            }
        //            this.items.Add(listitem);
        //        }
        //    }
        //}
        //public BOMConsumptionMobile(BOMConsumption source)
        //{
        //    this.id = source.id;
        //    this.entryno = source.entryno;
        //    this.entrydate = source.entrydate;
        //    this.supplierid = source.supplierid;
        //    this.items = new List<Item>();
        //    var list = source.BOMConsumptionitems.GroupBy(e => e.size).ToList().ConvertAll(e => e.Key);
        //    list.RemoveAll(e => string.IsNullOrEmpty(e));

        //    //if (list == null || list.Count <= 1)
        //    //{
        //    //    list = source.BOMConsumptionitems.GroupBy(e => e.price).ToList().ConvertAll(e => e.Key.ToString("F0"));
        //    //    foreach (var item in source.BOMConsumptionitems.GroupBy(e => new { e.designid, e.productid, e.brandid }))
        //    //    {
        //    //        var listitem = new Item()
        //    //        {
        //    //            cloneid = item.First().id,
        //    //            design = item.Key.designid,
        //    //            items = list.ConvertAll<Item.order>(e => new Item.order()
        //    //            {
        //    //                name = e,
        //    //                qty = 0
        //    //            })
        //    //        };
        //    //        foreach (var oitem in item)
        //    //        {
        //    //            if (listitem.items.Exists(e => e.name == oitem.price.ToString("F0")))
        //    //            {
        //    //                listitem.items.Find(e => e.name == oitem.price.ToString("F0")).qty = oitem.oqty;
        //    //                listitem.items.Find(e => e.name == oitem.price.ToString("F0")).id = oitem.id;
        //    //            }
        //    //        }
        //    //        this.items.Add(listitem);

        //    //    }
        //    //}
        //    //else
        //    //{
        //    //    foreach (var item in source.BOMConsumptionitems.GroupBy(e => new { e.designid, e.price, e.productid, e.brandid }))
        //    //    {
        //    //        var listitem = new Item()
        //    //        {
        //    //            cloneid = item.First().id,
        //    //            design = item.Key.designid,
        //    //            price = item.Key.price,
        //    //            items = list.ConvertAll<Item.order>(e => new Item.order()
        //    //            {
        //    //                name = e,
        //    //                qty = 0
        //    //            })
        //    //        };
        //    //        foreach (var oitem in item)
        //    //        {
        //    //            listitem.items.Find(e => e.name == oitem.size).qty = oitem.oqty;
        //    //            listitem.items.Find(e => e.name == oitem.size).id = oitem.id;
        //    //        }
        //    //        this.items.Add(listitem);
        //    //    }
        //    //}
        //}
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
    public class BOMConsumptionCreteria : BOMConsumption
    {
      
        public bool ismobilepo { get; set; }
        public DateTime fromdate { get; set; }
        public DateTime todate { get; set; }
        public long productid { get; set; }
        public int statusid { get; set; }
        public int limit { get; set; }
        public string design { get; set; }
        public string product { get; set; }
        public DateTime wodate { get; set; }
        public bool create { get; set; }
        
    }

    public class BOMConsumptionItemDetail
    {
        public long BOMConsumptionitemsid { get; set; }
        public DateTime entrydate { get; set; }
        public long bomconsumptionid { get; set; }
        public string entryno { get; set; }
        public string imageid { get; set; }
        public decimal rate { get; set; }
        public decimal qty { get; set; }
        public decimal tolerance { get; set; }
    }
    public class BOMConsumptionDetails
    {
        public long id { get; set; }
        public decimal tolerance { get; set; }
     
        public long companyid { get; set; }
        public DateTime entrydate { get; set; }
        public DateTime expectedate { get; set; }
        public string entryno { get; set; }
        public long supplierid { get; set; }
        public POStatus status { get; set; }
        public enum POStatus
        {
            PENDING = 0,
            APPROVED = 1,
            REJECTED = 2
        }
        public decimal qty { get; set; }
        public decimal rate { get; set; }
        public decimal amount { get; set; }

    }
    public class BOMConsumptionPrint : BOMConsumption
    {
        public double duedays
        {
            get
            {
                if (expectedate > DateTime.MinValue && expectedate > entrydate)
                    return (expectedate.Date - entrydate.Date).TotalDays;
                return 0;
            }
        }

        public Company company { get; set; }
        public Supplier supplier { get; set; }
        public Suppliercompany suppliercompany { get; set; }
        public Transport transport { get; set; }
        public Agent agent { get; set; }
        public DeliveryLocation deliverylocation { get; set; }
        public string whocreated { get; set; }
        public string whoapproved { get; set; }
      
        public class DeliveryLocation
        {
            public string address { get; set; }
        }
    }

    public class BOMConsumptionItemSimple : BaseEntity
    {
        public int version { get; set; }
        public long cloneid { get; set; }
        public long id { get; set; }
        public long companyid { get; set; }
        public long supplierid { get; set; }
        public long productid { get; set; }
        public long brandid { get; set; }
        public long poid { get; set; }
        public long sizeid { get; set; }
        public decimal qty { get; set; }
        public decimal price { get; set; }
        public string product { get; set; }
        public string brand { get; set; }
        public string entryno { get; set; }
        public string size { get; set; }
        public DateTime entrydate { get; set; }
       
    }

    public class BOMConsumePrint : BOMConsumption
    {
        public ReportLibrary.DOSReporter.Table sizetable { get; set; }
        public DateTime joborderdate { get; set; }
        public string jobcardno { get; set; }
        public Company company { get; set; }
        public Supplier supplier { get; set; }
        public Suppliercompany suppliercompany { get; set; }
        public Transport transport { get; set; }
        public string whocreated { get; set; }
        public string whoapproved { get; set; }
        public string brand { get; set; }
        public string fabric { get; set; }
        public string design { get; set; }
        public string product { get; set; }
        public DateTime wodate { get; set; }

        public Referencelist location { get; set; }
    }


    public class BomStockRequest:BaseEntity
    {
        public long locationid { get; set; }
        public long companyid { get; set; }
        public long bomid { get; set; }
        public long colourid { get; set; }
        public string bomcode { get; set; }
    }
}
