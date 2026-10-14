using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class Mfgjobworkconfig : BaseEntity
    {
        public long id { get; set; }
        public string name { get; set; }
        public bool isactive { get; set; }
        public long createdby { get; set; }
        public long modifiedby { get; set; }
        public int version { get; set; }
        public DateTime createdon { get; set; }
        public DateTime modifiedon { get; set; }
        public long uomid { get; set; }
        public long sourcebomcategoryid { get; set; }
        public long sourcebomid { get; set; }
        public int sourcetypeid { get; set; }
        public long productid { get; set; }
        public long taxid { get; set; }
        public decimal rate { get; set; }
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string config_json { get { return Newtonsoft.Json.JsonConvert.SerializeObject(config); } set { config = Newtonsoft.Json.JsonConvert.DeserializeObject<Config>(value); } }
        public Config config { get; set; }
        public class Config
        {
        }
    }
    public class MfgjobworkconfigCreteria : Mfgjobworkconfig
    {
    }
}
