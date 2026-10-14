using InfyPOS.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace InfyPOS.Models
{
    public class SKUMaster : BaseEntity
    {
        public long id { get; set; }
        public string code { get; set; }
        public string name { get; set; }
        public long productid { get; set; }
        public long brandid { get; set; }
        public long sleeveid { get; set; }
        public long fitid { get; set; }
        public long seasonid { get; set; }
        public long typeid { get; set; }
        public long styleid { get; set; }
        public long colorid { get; set; }
        public long uomid { get; set; }
        public string pack { get; set; }
        public long patternid { get; set; }
        public long materialid { get; set; }
        public string printingname { get; set; }

        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string sizegroup_json { get { return Newtonsoft.Json.JsonConvert.SerializeObject(sizegroup); } set { sizegroup = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Sizes>>(value); } }
        public List<Sizes> sizegroup { get; set; }
        public class Sizes
        {
            public string sizename { get; set; }
            public decimal qty { get; set; }
        }

       
        public bool isactive { get; set; }
        public long createdby { get; set; }
        public long modifiedby { get; set; }
        public int version { get; set; }
        public DateTime createdon { get; set; }
        public DateTime modifiedon { get; set; }
    }
    public class SKUMasterCreteria : Agent
    {
        public bool inactive { get; set; }
        public DateTime fromdate { get; set; }
        public DateTime todate { get; set; }
    }
}
