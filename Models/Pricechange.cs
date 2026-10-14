using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class Pricechange : BaseEntity
    {
        public long id { get; set; }
        public long approvedby { get; set; }
        public string reason { get; set; }
        public DateTime changedon { get; set; }
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string itemlist_json { get { return Newtonsoft.Json.JsonConvert.SerializeObject(itemlist); } set { itemlist = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ItemDetail>>(value); } }
        public List<ItemDetail> itemlist { get; set; }
        public bool isactive { get; set; }
        public long createdby { get; set; }
        public long modifiedby { get; set; }
        public long companyid { get; set; }
        public long locationid { get; set; }
        public int version { get; set; }
        public DateTime createdon { get; set; }
        public DateTime modifiedon { get; set; }
        public bool iswarehouse { get; set; }
        public class ItemDetail
        {
            public int _itemindex { get; set; }
            public long stockid { get; set; }
            public long salablegoodsid { get; set; }
            public decimal oprice { get; set; }
            public decimal nprice { get; set; }
            public decimal odiscount { get; set; }
            public decimal ndiscount { get; set; }
            public decimal odealerprice { get; set; }
            public decimal ndealerprice { get; set; }
            //[JsonIgnore]
            public string printingname { get; set; }
            //[JsonIgnore]
            public string barcode { get; set; }
            public string pricetagname { get; set; }
            //[JsonIgnore]
            public decimal stock { get; set; }
            //[JsonIgnore]
            public decimal discount { get; set; }
            //[JsonIgnore]
            public decimal price { get; set; }
            public decimal dealerprice { get; set; }
            public decimal buyingprice { get; set; }
            public decimal ibuyingprice { get; set; }
            public decimal marginpercentage { get; set; }
            public decimal mrp { get; set; }
            public string serialno { get; set; }
            public decimal taxpercentage { get; set; }
            public int printcopy { get; set; }
        }
        public string empname { get; set; }

    }
    public class PricechangeCreteria : Pricechange
    {
        public int mode { get; set; }
        public DateTime fromdate { get; set; }
        public DateTime todate { get; set; }
        public bool changedealerprice { get; set; }
    }
    public class Pricechangehistory : BaseEntity
    {
        public string locationname { get; set; }
        public int sellingmode { get; set; }
        public long stockid { get; set; }
        public long locationid { get; set; }
        public string reason { get; set; }
        public DateTime createdon { get; set; }
        public string empname { get; set; }
        public string printingname { get; set; }
        public string barcode { get; set; }
        public decimal oprice { get; set; }
        public decimal nprice { get; set; }
        public decimal odiscount { get; set; }
        public decimal ndiscount { get; set; }
        public decimal odealerprice { get; set; }
        public decimal ndealerprice { get; set; }
        public decimal stockqty { get; set; }
        public decimal changevalue { get; set; }
        public decimal buyingprice { get; set; }
        public string sectionname { get; set; }
        public decimal totalcost { get; set; }
        public decimal totalchangevalue { get; set; }
    }
}
