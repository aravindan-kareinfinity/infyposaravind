using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class Customerstock : BaseEntity
    {
        public long id { get; set; }
        public long curstomerid { get; set; }
        public long iemployeeid { get; set; }
        public long aemployeeid { get; set; }
        public long remployeeid { get; set; }
        public DateTime issuedon { get; set; }
        public int noofitemsissued { get; set; }
        public int noofitemsreceived { get; set; }
        public DateTime receivedon { get; set; }
        public bool isactive { get; set; }
        public long createdby { get; set; }
        public long modifiedby { get; set; }
        public int version { get; set; }
        public DateTime createdon { get; set; }
        public DateTime modifiedon { get; set; }
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string stockitems_json { get { return Newtonsoft.Json.JsonConvert.SerializeObject(stockitems); } set { stockitems = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Stockitems>>(value); } }
        public List<Stockitems> stockitems { get; set; }
        public int status { get; set; }
        public string code { get; set; }
        public class Stockitems
        {
            public long salablegoodsid { get; set; }
            public string barcode { get; set; }
            public string name { get; set; }
            public decimal qty { get; set; }
            public decimal rqty { get; set; }
        }
    }
    public class CustomerstockCreteria : Customerstock
    {
    }
}
