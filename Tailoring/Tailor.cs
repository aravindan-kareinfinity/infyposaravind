using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class Tailor : BaseEntity
    {
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string config_json { get { return Newtonsoft.Json.JsonConvert.SerializeObject(config); } set { config = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Config>>(value); } }
        public List<Config> config { get; set; }
        public long cityid { get; set; }
        public long stateid { get; set; }
        public long countryid { get; set; }
        public bool isactive { get; set; }
        public long createdby { get; set; }
        public long modifiedby { get; set; }
        public int version { get; set; }
        public DateTime createdon { get; set; }
        public DateTime modifiedon { get; set; }
        public long id { get; set; }
        public string name { get; set; }
        public string contactno { get; set; }
        public string address { get; set; }
        public string bankaccountname { get; set; }
        public string bankifci { get; set; }
        public string bankaccountno { get; set; }
        public string pincode { get; set; }
        public string emailid { get; set; }
        public string gst { get; set; }
        public string bankbranch { get; set; }
        public class Config
        {
            public long chargeid { get; set; }
            public decimal charge { get; set; }
            public decimal delayperday { get; set; }
            public decimal delaymaximum { get; set; }
            public decimal chargeonpriority { get; set; }
        }
    }
    public class TailorCreteria : Tailor
    {
    }
}
