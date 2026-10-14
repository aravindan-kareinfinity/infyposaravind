using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class DailyPriceDetail : Dailyprice
    {
        public DateTime appliedon { get; set; }
        public long appliedby { get; set; }
        public int appliedrows { get; set; }
        public decimal lastprice { get; set; }
        public decimal newprice { get; set; }
        public string barcode { get; set; }
        public string itemcode { get; set; }
        public string description { get; set; }
    }

    public class Dailyprice : BaseEntity
    {
        public long modifiedby { get; set; }
        public bool isactive { get; set; }
        public int version { get; set; }
        public long id { get; set; }
        public DateTime modifiedon { get; set; }
        public DateTime createdon { get; set; }
        public long createdby { get; set; }
        public DateTime priceon { get; set; }
        public long employeeid { get; set; }
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string items_json { get { return Newtonsoft.Json.JsonConvert.SerializeObject(items); } set { items = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Items>>(value); } }
        public List<Items> items { get; set; }
        public long companyid { get; set; }
        public long locationid { get; set; }
        public long pricegroupid { get; set; }
        public class Items
        {
            public long productid { get; set; }
            public decimal lastprice { get; set; }
            public decimal newprice { get; set; }
            public string itemcode { get; set; }
            public string barcode { get; set; }
            public string description { get; set; }
            public long agreementid { get; set; }
            public string agreementname { get; set; }
            public bool stockexist { get; set; }
            public List<StockInfo> stock { get; set; }
            public class StockInfo
            {
                public long id { get; set; }
                public decimal qty { get; set; }
            }
        }

        public List<AppliedInfo> priceavailablelocations { get; set; }

        public class AppliedInfo
        {
            public long locationid { get; set; }
            public bool applied { get; set; }
            public long rows { get; set; }
            public long existpriceid { get; set; }
        }
    }
    public class DailypriceCreteria : Dailyprice
    {
        public List<Dailyprice.AppliedInfo> applylocations { get; set; }
        public List<Dailyprice.AppliedInfo> copylocations { get; set; }
        public DateTime fromdate { get; set; }
        public DateTime todate { get; set; }
        public long productid { get; set; }
        public bool searchonly { get; set; }
    }
}
