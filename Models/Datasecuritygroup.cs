using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class Datasecuritygroup : BaseEntity
    {
        public bool isactive { get; set; }
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string config_json { get { return Newtonsoft.Json.JsonConvert.SerializeObject(config); } set { config = Newtonsoft.Json.JsonConvert.DeserializeObject<Config>(value); } }
        public Config config { get; set; }
        public long id { get; set; }
        public long userid { get; set; }
        public DateTime createdon { get; set; }
        public DateTime modifiedon { get; set; }
        public long createdby { get; set; }
        public long modifiedby { get; set; }
        public string name { get; set; }
        public string username { get; set; }
        public int version { get; set; }
        public class Config
        {
            public List<SecurityGroup> dsgrights { get; set; }
            public class SecurityGroup
            {
                public long id { get; set; }
                public bool view { get; set; }
                public bool full { get; set; }
            }
        }
    }
    public class DatasecuritygroupCreteria : Datasecuritygroup
    {
    }
}
