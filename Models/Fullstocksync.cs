using InfyPOS.Common;
using InfyPOS.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class Fullstocksync : BaseEntity
    {
        public long fileid { get; set; }
        public long companyid { get; set; }
        public long locationid { get; set; }
        public bool synced { get; set; }
        public bool rollbacked { get; set; }
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string stockdetail_json { get { return Newtonsoft.Json.JsonConvert.SerializeObject(stockdetail); } set { stockdetail = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Stockdetail>>(value); } }
        public List<Stockdetail> stockdetail { get; set; }

        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string missingb2b_json { get { return Newtonsoft.Json.JsonConvert.SerializeObject(missingb2b); }
            set { missingb2b = value == null ? null :  Newtonsoft.Json.JsonConvert.DeserializeObject<List<FullstocksyncDataService.b2bdesignbased>>(value); } }

        public List<FullstocksyncDataService.b2bdesignbased> missingb2b { get;  set; }

        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string errorb2b_json
        {
            get { return Newtonsoft.Json.JsonConvert.SerializeObject(errorb2b); }
            set { errorb2b = value == null ? null : Newtonsoft.Json.JsonConvert.DeserializeObject<List<string>>(value); }
        }

        public List<string> errorb2b { get;  set; }

        public class Stockdetail
        {
            public string barcode { get; set; }
            public decimal stock { get; set; }
            public decimal price { get; set; }
        }

        public class UANStockdetail : Stockdetail
        {
            public string uan { get; set; }
            public decimal qty { get; set; }
            public decimal transfered { get; set; }
        }
    }
    public class FullstocksyncCreteria : Fullstocksync
    {
    }
}
