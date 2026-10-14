using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class Servicecenters : BaseEntity
    {
        public DateTime modifiedon { get; set; }
        public long createdby { get; set; }
        public long modifiedby { get; set; }
        public bool isactive { get; set; }
        public long id { get; set; }
        public string name { get; set; }
        public string address { get; set; }
        public string city { get; set; }
        public string contactno { get; set; }
        public string contactperson { get; set; }
        public string emailid { get; set; }
        public string workinghours { get; set; }
        public DateTime createdon { get; set; }
        public int version { get; set; }
        public long brandid { get; set; }
        public long productid { get; set; }
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string locations_json { get { return Newtonsoft.Json.JsonConvert.SerializeObject(locations); } set { locations = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Locations>>(value); } }
        public List<Locations> locations { get; set; }
        public class Locations
        {
            public long id { get; set; }
            public string name { get; set; }
            public bool selected { get; set; }
        }
    }
    public class ServicecentersCreteria : Servicecenters
    {
    }
}
