using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class ProductdesignBase : BaseEntity
    {
        public string designid { get; set; }
        public long productid { get; set; }
        public long supplierid { get; set; }
        public long id { get; set; }

    }
    public class Productdesign : ProductdesignBase
    {

        
        public int activestatus { get; set; }
        [JsonIgnore]
        public ActiveStatus Activestatus
        {
            get
            {
                return (ActiveStatus)activestatus;
            }
            set
            {
                activestatus = (int)value;
            }
        }
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string detail_json { get { return Newtonsoft.Json.JsonConvert.SerializeObject(detail); } set { detail = Newtonsoft.Json.JsonConvert.DeserializeObject<Detail>(value); } }
        public Detail detail { get; set; }
        public long createdby { get; set; }
        public long modifiedby { get; set; }
        public bool isactive { get; set; }
        public int version { get; set; }
        public DateTime modifiedon { get; set; }
        public DateTime createdon { get; set; }
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string colours_json { get { return Newtonsoft.Json.JsonConvert.SerializeObject(colours); } set { colours = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Colours>>(value); } }
        public List<Colours> colours { get; set; }
        public bool autonumber { get; set; }
        public class Detail
        {
            public List<long> sizes { get; set; }
            public List<string> images { get; set; }
        }
        public class Colours
        {
            public long id { get; set; }
            public string name { get; set; }
            public string image { get; set; }
        }
    }
    public class ProductdesignCreteria : Productdesign
    {
        public List<long> productids { get; set; }
        public List<long> supplierids { get; set; }
        public List<string> designids { get; set; }
    }

    public class ProductDesignImage
    {
        public string image { get; set; }
        public string design { get; set; }
        public long supplierid { get; set; }
        public long productid { get; set; }
        [JsonIgnore]
        public string colours_json { get { return Newtonsoft.Json.JsonConvert.SerializeObject(colours); } set { colours = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Colours>>(value); } }
        [JsonIgnore]
        public List<Colours> colours { get; set; }
        public class Colours
        {
            public long id { get; set; }
            public string name { get; set; }
            public string image { get; set; }
        }
    }

    public class MergeProductDesign:BaseEntity
    {
        public long mergeid { get; set; }
        public string mergename { get; set; }
        public List<long> ids { get; set; }
    }
}
