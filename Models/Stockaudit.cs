using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class Stockaudit : BaseEntity
    {
        public long modifiedby { get; set; }
        public DateTime modifiedon { get; set; }
        public long createdby { get; set; }
        public bool isactive { get; set; }
        public long id { get; set; }
        public DateTime createdon { get; set; }
        public int version { get; set; }
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string condition_json { get { return Newtonsoft.Json.JsonConvert.SerializeObject(condition); } set { condition = Newtonsoft.Json.JsonConvert.DeserializeObject<Condition>(value); } }
        public Condition condition { get; set; }
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string matched_json { get { return Newtonsoft.Json.JsonConvert.SerializeObject(matched); } set { matched = Newtonsoft.Json.JsonConvert.DeserializeObject<List<StockMiniReport>>(value); } }
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string missing_json { get { return Newtonsoft.Json.JsonConvert.SerializeObject(missing); } set { missing = Newtonsoft.Json.JsonConvert.DeserializeObject<List<StockMiniReport>>(value); } }
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string additional_json { get { return Newtonsoft.Json.JsonConvert.SerializeObject(additional); } set { additional = Newtonsoft.Json.JsonConvert.DeserializeObject<List<StockMiniReport>>(value); } }

        public List<StockMiniReport> matched { get; set; }
        public List<StockMiniReport> missing { get; set; }
        public List<StockMiniReport> additional { get; set; }
        public DateTime recordedon { get; set; }
        public long recordedemployeeid { get; set; }
        public long locationid { get; set; }
        public long producttype { get; set; }
        public int missingcount { get; set; }
        public int additionalcount { get; set; }
        public string recorededby { get; set; }
        public class Condition
        {
            public long productgroupid { get; set; }
            public long productid { get; set; }
            public long supplierid { get; set; }
            public long brandid { get; set; }
            public long typeid { get; set; }
            public long colourid { get; set; }
            public long materialid { get; set; }
            public long styleid { get; set; }
            public long patternid { get; set; }
            public long sectionid { get; set; }
            public long sizeid { get; set; }
            public string designid { get; set; }
            public long markerid { get; set; }
        }
    }
    public class StockauditCreteria : Stockaudit
    {
    }
}
