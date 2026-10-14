using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class Productrequestform : BaseEntity
    {
        public long id { get; set; }
        public long requestedby { get; set; }
        public DateTime requestedon { get; set; }
        public string prfno { get; set; }
        public long reasonid { get; set; }
        public long priorityid { get; set; }
        public long companyid { get; set; }
        public long locationid { get; set; }
        public long customerid { get; set; }
        public bool isactive { get; set; }
        public long createdby { get; set; }
        public long modifiedby { get; set; }
        public int version { get; set; }
        public DateTime createdon { get; set; }
        public DateTime modifiedon { get; set; }
        public List<Productrequestformitems> Productrequestformitems { get; set; }
        
    }

    public class ProductrequestformitemsDetail : Productrequestformitems
    {
        public string productname;
        public DateTime requestedon;
        public long requestedby { get; set; }
        public string printingname { get; set; }
        public string brandname { get; set; }
        public string typename { get; set; }
        public string patternname { get; set; }
        public string stylename { get; set; }
        public string materialname { get; set; }
        public string colourname { get; set; }
        public string sizename { get; set; }
        public long sizeid { get; set; }
    }

    public class ProductrequestformCreteria : Productrequestform
    {
    }

    public class PRFRequest : BaseEntity
    {
        public long id { get; set; }
        public long locationid { get; set; }
        public long brandid { get; set; }
        public long productid { get; set; }
        public long colourid { get; set; }
        public long patternid { get; set; }
        public long sizeid { get; set; }
        public long styleid { get; set; }
        public long materialid { get; set; }
        public long supplierid { get; set; }
        public long requestedby { get; set; }
        public DateTime fromdate { get; set; }
        public DateTime todate { get; set; }
        public long reasonid { get; set; }
        public long priorityid { get; set; }
        public bool pending { get; set; }
        public bool cancelled { get; set; }
        public string designid { get; set; }
        public List<Size> jumplist { get; set; }
        public class Size
        {
            public long id { get; set; }
            public decimal qty { get; set; }
        }
    }

    public class PRFFromPurchaseBarcode:BaseEntity
    {
        public List<StockReport> items { get; set; }
        public long companyid { get; set; }
        public long supplierid { get; set; }
        public long locationid { get; set; }
        public long employeeid { get; set; }
    }
    public class POSearch : Items
    {
        //public long id { get; set; }
        //public long brandid { get; set; }
        //public long productid { get; set; }
        //public long typeid { get; set; }
        //public long colourid { get; set; }
        //public long patternid { get; set; }
        //public long sizeid { get; set; }
        //public long styleid { get; set; }
        //public long companyid { get; set; }
        //public long materialid { get; set; }
        //public string printingname { get; set; }
    }

    public class POResult
    {
        public long itemid { get; set; }
        public decimal warehouse { get; set; }
        public decimal showroom { get; set; }
        public decimal ordered { get; set; }
        public decimal invoice { get; set; }
        public decimal today { get; set; }
    }
}
