using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class Jobworksamples : BaseEntity
    {
        public bool isactive { get; set; }
        public long id { get; set; }
        public DateTime modifiedon { get; set; }
        public DateTime createdon { get; set; }
        public int version { get; set; }
        public long modifiedby { get; set; }
        public long createdby { get; set; }
        public long productid { get; set; }
        public string designname { get; set; }
        public long customerid { get; set; }
        public string customername { get; set; }
        public decimal sampleqty { get; set; }
        public decimal orderqty { get; set; }
        public decimal purchaseprice { get; set; }
        public decimal sellingprice { get; set; }
        public string sampleno { get; set; }
        public int samplestatus { get; set; }
        public long receivedby { get; set; }
        public DateTime receivedon { get; set; }
        public DateTime senton { get; set; }
        public long sentby { get; set; }
        public long receivedcouriercompanyid { get; set; }
        public long sentcouriercompanyid { get; set; }
        public string receivedcourierpod { get; set; }
        public string sentcourierpod { get; set; }
        public string sampleimageid { get; set; }
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string communication_json { get { return Newtonsoft.Json.JsonConvert.SerializeObject(communication); } set { communication = Newtonsoft.Json.JsonConvert.DeserializeObject<Communication>(value); } }
        public Communication communication { get; set; }
        public DateTime expecteddelivery { get; set; }
        public class Communication
        {
            public string remark { get; set; }
            public List<Item> list { get; set; }
            public class Item
            {
                public DateTime addedon { get; set; }
                public string remark { get; set; }
                public int status { get; set; }
            }
        }
    }
    public class JobworksamplesCreteria : Jobworksamples
    {
        public DateTime expectedtodate { get; set; }
        public DateTime expectedfromdate { get; set; }
        public DateTime receivedtodate { get; set; }
        public DateTime receivedfromdate { get; set; }
        public DateTime senttodate { get; set; }
        public DateTime sentfromdate { get; set; }
    }
}
