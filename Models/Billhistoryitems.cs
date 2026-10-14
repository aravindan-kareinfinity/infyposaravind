using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class Billhistoryitems : BaseEntity
    {
        public long id { get; set; }
        public long billhistoryid { get; set; }
        public bool isactive { get; set; }
        public long createdby { get; set; }
        public long modifiedby { get; set; }
        public int version { get; set; }
        public DateTime createdon { get; set; }
        public DateTime modifiedon { get; set; }
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string oldbilldata_json { get { return Newtonsoft.Json.JsonConvert.SerializeObject(oldbilldata); } set { oldbilldata = Newtonsoft.Json.JsonConvert.DeserializeObject<Bill>(value); } }
        public Bill oldbilldata { get; set; }

        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string newbilldata_json { get { return Newtonsoft.Json.JsonConvert.SerializeObject(newbilldata); } set { newbilldata = Newtonsoft.Json.JsonConvert.DeserializeObject<Bill>(value); } }
        public Bill newbilldata { get; set; }
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string oldsettlementdata_json { get { return Newtonsoft.Json.JsonConvert.SerializeObject(oldsettlementdata); } set { oldsettlementdata = Newtonsoft.Json.JsonConvert.DeserializeObject<Billsettlementmaster>(value); } }
        public Billsettlementmaster oldsettlementdata { get; set; }

        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string newsettlementdata_json { get { return Newtonsoft.Json.JsonConvert.SerializeObject(newsettlementdata); } set { newsettlementdata = Newtonsoft.Json.JsonConvert.DeserializeObject<Billsettlementmaster>(value); } }
        public Billsettlementmaster newsettlementdata { get; set; }
        public long billid { get; set; }
        public long billsettlementmasterid { get; set; }
        public bool multiplesettlement { get; set; }
        public bool deleted { get; set; }
    }
    public class BillhistoryitemsCreteria : Billhistoryitems
    {
    }
}
