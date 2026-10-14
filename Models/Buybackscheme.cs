using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class Buybackscheme : BaseEntity
    {
        public string name { get; set; }
        public bool isactive { get; set; }
        public long createdby { get; set; }
        public long modifiedby { get; set; }
        public int version { get; set; }
        public DateTime createdon { get; set; }
        public DateTime modifiedon { get; set; }
        public long id { get; set; }
        public long supplierid { get; set; }
        public long productid { get; set; }
        public long brandid { get; set; }
        public DateTime fromdate { get; set; }
        public DateTime todate { get; set; }
        public DateTime schemedate { get; set; }
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string items_json { get { return Newtonsoft.Json.JsonConvert.SerializeObject(items); } set { items = Newtonsoft.Json.JsonConvert.DeserializeObject<List<BuyBackItem>>(value); } }
        public List<BuyBackItem> items { get; set; }
        public decimal schemediscount { get; set; }
        public class BuyBackItem
        {
            public long itemid { get; set; }
            public string itemname { get; set; }
            public decimal min { get; set; }
            public decimal max { get; set; }
        }
    }
    public class BuybackschemeCreteria : Buybackscheme
    {
        public bool hideitems { get; set; }
    }
}
