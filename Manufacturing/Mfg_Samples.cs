using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class Mfg_Samples : BaseEntity
    {
        public DateTime entrydate { get; set; }
        public long salesmenid { get; set; }
        public long employeeid { get; set; }
        public bool isactive { get; set; }
        public long locationid { get; set; }
        public string remark { get; set; }
        public int qty { get; set; }
        public string designcode { get; set; }
        public string size { get; set; }
        public long sizeid { get; set; }
        public string uid { get; set; }
        public int version { get; set; }
        public DateTime modifiedon { get; set; }
        public DateTime createdon { get; set; }
        public long createdby { get; set; }
        public long modifiedby { get; set; }
        public long id { get; set; }
        public int optionid { get; set; }
        public long salablegoodsid { get; set; }
        public long returnsampleid { get; set; }
        public List<UIDDesign> items { get; set; }
        public string receiptno { get; set; }
        public class UIDDesign
        {
            public string uid { get; set; }
            public string designcode { get; set; }
        }
        public List<Mfg_Samples> sampleitems { get; set; }
        [JsonIgnore]
        public List<SizeQty> jumpqty { get; set; }
        public class SizeQty
        {
            public long sizeid { get; set; }
            public decimal oqty { get; set; }
            public string name { get; set; }
        }
        [JsonIgnore]
        public int orderid { get; set; }
        [JsonIgnore]
        public Company company { get; internal set; }
        [JsonIgnore]
        public Models.Referencelist location { get; internal set; }
        [JsonIgnore]
        public Employee salesmen { get; internal set; }
        [JsonIgnore]
        public Employee employee { get; internal set; }
        [JsonIgnore]
        public bool IsIssue { get; set; }
    }
    public class Mfg_SamplesCreteria : Mfg_Samples
    {
        public bool includeitems { get; set; }
        public DateTime fromdate { get; set; }
        public DateTime todate { get; set; }
        public bool showdetail { get; set; }
        public bool includeso { get; set; }
        public bool includestock { get; set; }
        public int mode { get; set; }
        public List<long> ids { get; set; }
    }

    public class Mfg_SampleSummary
    {
        public int samplestock { get; set; }
        public int folderstock { get; set; }
        public int productstock { get; set; }
        public int salesorder { get; set; }
        public List<Item> productstocks { get; set; }
        public List<Item> salesorders { get; set; }
        public string design { get; set; }
        public string designcode { get; set; }
        public long brandid { get; set; }
        public long productid { get; set; }
        public class Item
        {
            public string uid { get; set; }
            public long sizeid { get; set; }
            public string designcode { get; set; }
            public int qty { get; set; }
        }
    }
    public class Mfg_SampleStock
    {
        public int selfstock { get; set; }
        public int locationstock { get; set; }
        public string design { get; set; }
        public string designcode { get; set; }
    }
}
