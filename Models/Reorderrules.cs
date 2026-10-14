using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class Reorderrules : BaseEntity
    {
        public long id { get; set; }
        public bool isactive { get; set; }
        public long createdby { get; set; }
        public long modifiedby { get; set; }
        public int version { get; set; }
        public DateTime createdon { get; set; }
        public DateTime modifiedon { get; set; }
        public long productid { get; set; }
        public long locationid { get; set; }
        public long supplierid { get; set; }
        public long brandid { get; set; }
        public long patternid { get; set; }
        public long styleid { get; set; }
        public long colourid { get; set; }
        public long materialid { get; set; }
        public long typeid { get; set; }
        public long sizeid { get; set; }
        public decimal fromrate { get; set; }
        public decimal torate { get; set; }
        public decimal minro { get; set; }
        public decimal maxro { get; set; }
        public decimal minorder { get; set; }
        public decimal aprice { get; set; }
        public int priority { get; set; }
        public int coreid { get; set; }
        public string description { get; set; }
        public string designid { get; set; }
        public bool enable { get; set; }
        public int expirydays { get; set; }
        public bool onpurchaseprice { get; set; }
        [JsonIgnore]
        public string pricefield { get; set; }
        public long uid { get; set; }
    }

    public class ReorderGroupItem
    {
        public long productid = 0;
        public long brandid = 0;
        public long materialid = 0;
        public long colourid = 0;
        public long patternid = 0;
        public long sizeid = 0;
        public long styleid = 0;
        public long typeid = 0;
        public long supplierid = 0;
        public string designid = "";
        public decimal purchaseprice = 0;
        public decimal salesrate = 0;
        public long locationid;
        public decimal sold;
        public decimal stock;
        public decimal retailstock;
        public decimal otherretailstock;
    }

    public class ReorderrulesCreteria : Reorderrules
    {
        public long companyid { get; set; }
        public long sectionid { get; set; }
        public bool showall { get; set; }
        public bool salerange { get; set; }
        public DateTime salefromdate { get; set; }
        public DateTime saletodate { get; set; }
        public ReorderMatrix matrixconfig { get; set; }
    }

    public class ReorderMatrixSummary : BaseEntity
    {
        public class Range
        {
            public int from { get; set; }
            public int to { get; set; }
        }
        public List<Range> PriceRanges { get; set; }
        public List<Data> DataSource { get; set; }
        public List<Reorderrules> Rules { get; set; }
        public class Data
        {
            public string name { get; set; }
            public List<Row> rows { get; set; }
        }
        public class Row
        {
            public string name { get; set; }
            public long id { get; set; }
        }
    }
    public class ReorderMatrix : BaseEntity
    {
        public bool location { get; set; }
        public bool brand { get; set; }
        public bool size { get; set; }
        public bool style { get; set; }
        public bool fit { get; set; }
        public bool sleeve { get; set; }
        public bool colour { get; set; }
        public bool pattern { get; set; }
        public bool design { get; set; }
        public bool type { get; set; }
        public bool material { get; set; }
        public bool pricerange { get; set; }
        public string pricelist { get; set; }
        public string column { get; set; }
    }

    public class Reorder : BaseEntity
    {
        public long locationid { get; set; }
        public long productid { get; set; }
        public List<Reorderrules> rules { get; set; }
        public List<Reorderrules> deleted { get; set; }
    }

    public class ReorderrulesResult
    {
        public string title { get; set; }
        public string status { get; set; }
        public decimal stock { get; set; }
        public decimal sold { get; set; }
        public decimal tsold
        {
            get
            {
                return sold + othersold;
            }
        }
        public decimal othersold { get; set; }
        public string expect { get; set; }
        public string pricerange { get; set; }
        public string productname { get; set; }
        public long productid { get; set; }
        public long supplierid { get; set; }
        public decimal retailstock { get; set; }
        public decimal otherretailstock { get; set; }
        public long locationid { get; set; }
        public string stockstatus { get; set; }
        public long brandid { get; set; }
        public long patternid { get; set; }
        public long styleid { get; set; }
        public long colourid { get; set; }
        public long materialid { get; set; }
        public long typeid { get; set; }
        public long sizeid { get; set; }
        public string designid { get; set; }
        public decimal minorder { get; set; }
        public decimal aprice { get; set; }
        public decimal minro { get; set; }
        public decimal maxro { get; set; }
        public long itemid { get; set; }
        public decimal shortage { get; set; }
        public decimal excess { get; set; }
        public decimal orderqty { get; set; }
        public bool onpurchaseprice { get; set; }

        public enum Priority
        {
            LOW, MEDIUM, HIGH, CRITICAL
        }
    }
}
