using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class Splitbarcodehistory : BaseEntity
    {
        public long id { get; set; }
        public long locationid { get; set; }
        public long companyid { get; set; }
        public DateTime modifiedon { get; set; }
        public DateTime createdon { get; set; }
        public int version { get; set; }
        public long modifiedby { get; set; }
        public long createdby { get; set; }
        public bool isactive { get; set; }
        public long sourceproductid { get; set; }
        public long convertedproductid { get; set; }
        public string barcode { get; set; }
        public string newbarcode { get; set; }
        public string code { get; set; }
        public long stockid { get; set; }
        public long salablegoodsid { get; set; }
        public decimal stockqty { get; set; }
        public decimal splitqty { get; set; }
        public decimal noofqtyeachbarcode { get; set; }
        public int noofnewbarcode { get; set; }
        public bool isbit { get; set; }
        public decimal newprice { get; set; }
        public decimal newdiscount { get; set; }
        public decimal price { get; set; }
        public decimal discount { get; set; }
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string barcodelist_json { get { return Newtonsoft.Json.JsonConvert.SerializeObject(barcodelist); } set { barcodelist = Newtonsoft.Json.JsonConvert.DeserializeObject<List<string>>(value); } }
        public List<string> barcodelist { get; set; }
        public string createdemployee { get; set; }
        public int newbarcodecount { get; set; } // To display how many nos of unique barcode generated.
    }
    public class SplitbarcodehistoryCreteria : Splitbarcodehistory
    {
        public DateTime fromdate { get; set; }
        public DateTime todate { get; set; }
    }
}
