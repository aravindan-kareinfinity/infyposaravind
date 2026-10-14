using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class Rechargescheme : BaseEntity
    {
        public string name { get; set; }
        public bool isactive { get; set; }
        public long createdby { get; set; }
        public long modifiedby { get; set; }
        public int version { get; set; }
        public decimal cashbackinpercentage { get; set; }
        public DateTime createdon { get; set; }
        public DateTime modifiedon { get; set; }
        public long id { get; set; }
        public long supplierid { get; set; }
        public long productid { get; set; }
        public long itemid { get; set; }
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string items_json { get { return Newtonsoft.Json.JsonConvert.SerializeObject(items); } set { items = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Items>>(value); } }
        public List<Items> items { get; set; }
        public class Items
        {
            public int id { get; set; }
            public string description { get; set; }
            public decimal rate { get; set; }
            public decimal cashback { get; set; }
            public bool isactive { get; set; }
            public bool isunlimitted { get; set; }
            public bool isratecutter { get; set; }
            public decimal discount { get; set; }
            public bool isnewconnection { get; set; }
            public DateTime from { get; set; }
            public DateTime to { get; set; }
        }
    }
    public class RechargeschemeCreteria : Rechargescheme
    {
        public bool hideitems { get;  set; }
    }
}
