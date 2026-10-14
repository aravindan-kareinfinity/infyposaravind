using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class Salesorderitems : BaseEntity
    {
        public long id { get; set; }
        public long productid { get; set; }
        public long itemid { get; set; }
        public decimal oqty { get; set; }
        public decimal bqty { get; set; }
        public decimal rqty { get; set; }
        public decimal pendingqty
        {
            get
            {
                return oqty - (bqty + cqty + rqty);
            }
        }
        public decimal cqty { get; set; }
        public decimal pqty { get; set; }
        public string designid { get; set; }
        public long designuid { get; set; }
        public long colourid { get; set; }
        public string description { get; set; }
        public decimal price { get; set; }
        public decimal tax { get; set; }
        public decimal taxpercentage { get; set; }
        public long taxid { get; set; }
        public decimal priceupto { get; set; }
        public decimal addnlcharge { get; set; }
        public decimal mrp { get; set; }
        public long addnlchargeid { get; set; }
        public long mfgsampleorderid { get; set; }
        public string mfgsampleordercode { get; set; }
        public int rowtype { get; set; }
        public string uid { get; set; }
        public long sizeid { get; set; }
        public string sizegroup { get; set; }
        [JsonIgnore]
        public string msizegroup
        {
            get
            {
                if (string.IsNullOrEmpty(sizegroup) || sizegroup.Length <= 2)
                    return "";
                return sizegroup;
            }
        }
        public long sizegroupid { get; set; }
        public string size { get; set; }
        public string pricerange { get; set; }
        public string priceranges
        {
            get { return string.Concat(price, "-", priceupto); }
            set
            {
                decimal d;
                if (value.IndexOf("-") > 0)
                {
                    decimal p = 0;
                    string[] item = value.Split('-');
                    if (item != null && item.Length > 0 && decimal.TryParse(item[0],out p) && p>0)
                        price = decimal.Parse(item[0]);
                    if (item != null && item.Length > 1 && decimal.TryParse(item[1], out p) && p > 0)
                        priceupto = decimal.Parse(item[1]);
                    else
                        priceupto = price;
                }
                else
                {
                    if (decimal.TryParse(value, out d) && d > 0)
                    {
                        price = d;
                        priceupto = d;
                    }
                    else if(priceupto ==0)
                    {
                        priceupto = price;
                    }
                }
            }
        }
        public bool isactive { get; set; }
        public long createdby { get; set; }
        public long modifiedby { get; set; }
        public int version { get; set; }
        public string imageid { get; set; }
        public long salesorderid { get; set; }
        public DateTime createdon { get; set; }
        public DateTime modifiedon { get; set; }
        public string colourcode { get; set; }
        public string colourshade { get; set; }
        public string designitemcode { get; set; }
        public SalesorderItemSimple.Status status
        {
            get
            {
                if (cqty > 0)
                    return bqty > 0 ? SalesorderItemSimple.Status.PartialCancelled : SalesorderItemSimple.Status.FullCancelled;

                else if (pqty > 0)
                    return oqty - (bqty + pqty) > 0
                        ? SalesorderItemSimple.Status.PartialPlanned : SalesorderItemSimple.Status.FullPlanned;

                else if (bqty > 0)
                    return bqty >= oqty ?
                        SalesorderItemSimple.Status.PartialDelivery : SalesorderItemSimple.Status.FullDelivery;
                return SalesorderItemSimple.Status.Undelivered;
            }
        }
        public long fgboxid { get; set; }
        public long soboxid { get; set; }
        public string hsncode { get; set; }
    }

    public class SalesOrderSizeSummary : BaseEntity
    {
        public SortedDictionary<long, string> sizenames { get; set; }
        public List<string> sizelist { get; set; }
        public List<long> sizeids { get; set; }
        public List<SalesOrderSizeRow> items { get; set; }
        public class SalesOrderSizeRow
        {
            public DateTime sodate { get; set; }
            public string sono { get; set; }
            public string customername { get; set; }
            public string sobookno { get; set; }
            public string remarks { get; set; }
            public string sizeids { get; set; }
            public long sizeid { get; set; }
            public long soitmid { get; set; }
            public string agentname { get; set; }
            public string cityname { get; set; }
            public string salesmen { get; set; }
            public string brand { get; set; }
            public string productname { get; set; }
            public string designid { get; set; }
            public string sizegroup { get; set; }
            public long soid { get; set; }
            public long createdby { get; set; }
            public string colourname { get; set; }
            public string stylename { get; set; }
            public string brandname { get; set; }
            public string fitname { get; set; }
            public string sleevename { get; set; }
            public string patternname { get; set; }

            public decimal pendingqty { get; set; }
            public decimal plannedqty { get; set; }
            public decimal billedqty { get; set; }
            public decimal cancelledqty { get; set; }
            public decimal removedqty { get; set; }
            public decimal orderqty { get; set; }
            public List<SizeQty> sizeqty { get; set; }
            public List<decimal> sz { get; set; }
        }
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
            if (sizenames == null) sizenames = new SortedDictionary<long, string>();
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

    public class SampleOrderSO : BaseEntity
    {
        public long id { get; set; }
        public string sono { get; set; }
        public string designid { get; set; }
        public string designno { get; set; }
        public string sizegroup { get; set; }
        public DateTime sodate { get; set; }
        public string name
        {
            get
            {
                return sono + " On " + sodate.ToString("dd-MM-yyyy");
            }
        }
        public List<Item> items { get; set; }
        public class Item
        {
            public long itemid { get; set; }
            public long sizeid { get; set; }
            public string description { get; set; }
            public string uid { get; set; }
            public decimal qty { get; set; }
        }
    }
    public class SalesOrderSummary : BaseEntity
    {
        public DateTime sodate { get; set; }
        public string sono { get; set; }
        public string customername { get; set; }
        public string agentname { get; set; }
        public string ids { get; set; }
        public string cityname { get; set; }
        public string productname { get; set; }
        public string designid { get; set; }
        public long productid { get; set; }
        public string sizegroup { get; set; }
        public string designitemcode { get; set; }
        public decimal pendingqty { get; set; }
        public decimal plannedqty { get; set; }
        public decimal availableqty { get; set; }
        public decimal allocatedqty { get; set; }
        public long soid { get; set; }
        [JsonIgnore]
        public long addnlchargeid { get; set; }
        [JsonIgnore]
        public long fgboxid { get; set; }
        [JsonIgnore]
        public long soboxid { get; set; }
        [JsonIgnore]
        public decimal addnlcharge { get; set; }
        [JsonIgnore]
        public long salesmenid { get; set; }
        [JsonIgnore]
        public long agentid { get; set; }
        [JsonIgnore]
        public long sizeid { get; set; }
        [JsonIgnore]
        public long colourid { get; set; }
        [JsonIgnore]
        public long brandid { get; set; }
        [JsonIgnore]
        public long styleid { get; set; }
        public string colourname { get; set; }
        public string colourcode { get; set; }
        public string stylename { get; set; }
        public string brandname { get; set; }
        [JsonIgnore]
        public long fitid { get; set; }
        public string fitname { get; set; }
        [JsonIgnore]
        public long sleeveid { get; set; }
        public string sleevename { get; set; }
        [JsonIgnore]
        public long patternid { get; set; }
        public string patternname { get; set; }

        public int sizeset { get; set; }
        public long customerid { get; set; }
        public decimal billedqty { get; set; }
        public decimal cancelledqty { get; set; }
        public decimal removedqty { get; set; }
        public decimal pipelinestock { get; set; }
        public bool partialstock { get; set; }
        public decimal dp { get; set; }
        public decimal price { get; set; }
    }

    public class SalesOrderUIDSummary : BaseEntity
    {
        [JsonProperty("sd")]
        public DateTime sodate { get; set; }
        public string sono { get; set; }
        [JsonProperty("rm")]
        public string remarks { get; set; }
        [JsonProperty("sb")]
        public string sobookno { get; set; }
        [JsonProperty("cbn")]
        public string customerpo { get; set; }
        [JsonProperty("sq")]
        public decimal sobookqty { get; set; }
        public string ids { get; set; }
        [JsonProperty("sz")]
        public string szlist { get; set; }
        [JsonProperty("zt")]
        public string stlist { get; set; }
        [JsonProperty("cyid")]
        public long cityid { get; set; }
        [JsonProperty("aid")]
        public long agentid { get; set; }
        [JsonProperty("slm")]
        public long salesmenid { get; set; }
        [JsonProperty("sid")]
        public long shipto { get; set; }
        [JsonIgnore]
        public string designid { get; set; }
        public long productid { get; set; }
        [JsonProperty("d")]
        public string designitemcode { get; set; }
        [JsonProperty("p")]
        public decimal pendingqty { get; set; }
        [JsonProperty("o")]
        public decimal orderqty { get; set; }
        [JsonProperty("n")]
        public decimal plannedqty { get; set; }
        [JsonProperty("v")]
        public decimal availableqty { get; set; }
        [JsonProperty("l")]
        public decimal allocatedqty { get; set; }
        public long soid { get; set; }
        [JsonIgnore]
        public int sizeset { get; set; }
        [JsonProperty("cid")]
        public long customerid { get; set; }
        [JsonProperty("b")]
        public decimal billedqty { get; set; }
        [JsonProperty("c")]
        public decimal cancelledqty { get; set; }
        [JsonProperty("r")]
        public decimal removedqty { get; set; }
        [JsonProperty("pi")]
        public decimal pipelinestock { get; set; }
        [JsonProperty("ps")]
        public bool partialstock { get; set; }
        [JsonProperty("cb")]
        public long createdby { get; set; }
        public decimal dp { get; set; }
        [JsonProperty("st")]
        public string sostatus { get; set; }
        [JsonIgnore]
        public decimal price { get; set; }
    }

    public class SalesOrderDetail : BaseEntity
    {
        public DateTime sodate { get; set; }
        public string customername { get; set; }
        public string designitemcode { get; set; }
        public string sono { get; set; }
        public string uid { get; set; }
        public string designid { get; set; }
        public long productid { get; set; }
        public string sizegroup { get; set; }
        public decimal pendingqty { get; set; }
        public decimal billedqty { get; set; }
        public decimal qty { get; set; }
        public decimal price { get; set; }
        public decimal mrp { get; set; }
        public long soid { get; set; }
        public long soitemid { get; set; }
        public long shipto { get; set; }
        public long sizeid { get; set; }
        public long colourid { get; set; }
        public long brandid { get; set; }
        public long styleid { get; set; }
        public long fitid { get; set; }
        public long sleeveid { get; set; }
        public long patternid { get; set; }
        public int sizeset { get; set; }
        public long customerid { get; set; }
        public string sizename { get; set; }
        public string brandname { get; set; }
        public string stylename { get; set; }
        public string patternname { get; set; }
        public string fitname { get; set; }
        public string sleevename { get; set; }
        public string colourname { get; set; }
        public string description { get; set; }
        public decimal pqty { get; set; }
    }

    public class MFG_SizeGroup<T>
    {
        public List<T> items { get; set; }
        public List<string> sizenames { get; set; }
    }
    public class MFG_SizeGroup_Stock
    {
        public decimal stockqty { get; set; }
        public decimal setstock { get; set; }
        public decimal patchqty { get; set; }
        public decimal inwardqty { get; set; }
        public decimal salesqty { get; set; }
        public decimal setsales { get; set; }
        public decimal damageqty { get; set; }
        public string designid { get; set; }
        public string customer { get; set; }
        public string city { get; set; }
        public string brand { get; set; }
        public string colourname { get; set; }
        public string productname { get; set; }
        public string sizegroup { get; set; }
        public bool igst { get; set; }
        public string groupkey { get; set; }
        public string billno { get; set; }
        public string billso { get; set; }
        public decimal[] sz { get; set; }
        public DateTime billdate { get; set; }
    }

    public class SalesOrderSummaryCreteria : SalesOrderSummary
    {
        public DateTime fromdate { get; set; }
        public DateTime todate { get; set; }
        public long locationid { get; set; }
        public long cityid { get; set; }

        public string design { get; set; }
        public int searchtype { get; set; }
        public enum Types
        {
            Default,
            Customer,
            Agent,
            City,
            Product
        }

        public Status statusid { get; set; }
        public bool sizesummary { get; set; }

        public enum Status
        {
            All,
            Pending,
            Planned,
            Cancelled,
            Billed,
            Planning,
            Removed
        }

    }
    public class SalesorderitemsDetail : Salesorderitems
    {
        public int sno { get; set; }
        public string sono { get; set; }
        public long customerid { get; set; }
        public long materialid { get; set; }
        public long styleid { get; set; }
        public long brandid { get; set; }
        public long typeid { get; set; }
        public long patternid { get; set; }
        public string printingname { get; set; }
        public string productname { get; set; }
        public string brandname { get; set; }
        public string typename { get; set; }
        public string patternname { get; set; }
        public string stylename { get; set; }
        public string materialname { get; set; }
        public string colourname { get; set; }
        public string sizename { get; set; }
        public List<SizeQty> jumpqty { get; set; }
        public decimal setqty { get; set; }
        public decimal tqty { get; set; }
        [JsonIgnore]
        public string addnlchargename { get; set; }
        [JsonIgnore]
        public string boxname { get; set; }
        [JsonIgnore]
        public decimal pricewithadditional
        {
            get
            {
                return price + addnlcharge;
            }
        }
        [JsonIgnore]
        public decimal amountwithadditional
        {
            get
            {
                return (price + addnlcharge) * oqty;
            }
        }
        [JsonIgnore]
        public decimal amount
        {
            get
            {
                return (price) * oqty;
            }
        }
        [JsonIgnore]
        public decimal mrpamount
        {
            get
            {
                return (mrp) * oqty;
            }
        }
        [JsonIgnore]
        public decimal slipmrpamount
        {
            get
            {
                return (mrp) * slipqty;
            }
        }
        [JsonIgnore]
        public decimal slipamountwithadditional
        {
            get
            {
                return (price + addnlcharge) * slipqty;
            }
        }
        [JsonIgnore]
        public decimal slipamount
        {
            get
            {
                return (price) * slipqty;
            }
        }
        [JsonIgnore]
        public decimal slipqty
        {
            get
            {
                var pqty = (oqty - (bqty + rqty + cqty));
                return pqty > 0 ? pqty : 0;
            }
        }

        [JsonIgnore]
        public decimal slipplanned
        {
            get
            {
                return pqty;
            }
        }
        [JsonIgnore]
        public decimal Net
        {
            get
            {
                return amount + tax;
            }
        }
        [JsonIgnore]
        public ReportLibrary.DOSReporter.Table sizetable { get; set; }
        [JsonIgnore]
        public string rackno { get; set; }
        [JsonIgnore]
        public string imgid { get; set; }
        public class SizeQty
        {
            public long id { get; set; }
            public long sizeid { get; set; }
            [JsonIgnore]
            public long itemid { get; set; }
            public decimal oqty { get; set; }
            public string name { get; set; }
            public string uid { get; set; }
            [JsonIgnore]
            public string sqty
            {
                get
                {
                    return oqty == 0 ? "" : oqty.ToString("N0");
                }
            }
            [JsonIgnore]
            public SalesorderitemsDetail parent { get; set; }
        }
    }

    public class SalesorderitemsCreteria : Salesorderitems
    {
        public bool groupbysizegroup { get; set; }
        public DateTime fromdate { get; set; }
        public DateTime todate { get; set; }
        public long customerid { get; set; }
        public int pricesource { get; set; }
        public long brandid { get; set; }
        public string sono { get; set; }
        public long companyid { get; set; }
        public long locationid { get; set; }
        public bool showdetail { get; set; }
    }
}
