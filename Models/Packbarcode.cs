using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class Packbarcode : BaseEntity
    {
        public long id { get; set; }
        public bool isactive { get; set; }
        public long createdby { get; set; }
        public long modifiedby { get; set; }
        public int version { get; set; }
        public DateTime createdon { get; set; }
        public DateTime modifiedon { get; set; }
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string barcodes_json
        {
            get { return Newtonsoft.Json.JsonConvert.SerializeObject(barcodes); }
            set
            {
                if (!string.IsNullOrEmpty(value) && value != "null")
                    barcodes = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Barcodes>>(value);
            }
        }
        public List<Barcodes> barcodes { get; set; }
        public string barcode { get; set; }
        public long locationid { get; set; }
        public long stockid { get; set; }
        public long salablegoodsid { get; set; }
        public long companyid { get; set; }
        public long barcodeid { get; set; }

        public class Barcodes
        {
            public long salablegoodsid { get; set; }
            public long stockid { get; set; }
            public string barcode { get; set; }
            public decimal qty { get; set; }
            public int pieces { get; set; }
            public long itemid { get; set; }
        }
    }
    public class PackbarcodeCreteria : Packbarcode
    {
        public decimal discount { get; set; }
        public decimal price { get; set; }
        public decimal displaymrp { get; set; }
        public int noofpack { get; set; }
        public long productid { get; set; }
        public string hsncode { get; set; }
        public bool sourcesearch { get; set; }
        public bool isbit { get; set; }
        public List<source> sourcelist { get; set; }
        public List<packinfo> packlist { get; set; }
        public long companyid { get; set; }

        public class source
        {
            public string barcode { get; set; }
            public long stockid { get; set; }
            public long itemid { get; set; }
            public long salablegoodsid { get; set; }
            public long productid { get; set; }
            public decimal qty { get; set; }
            public decimal adjstock { get; set; }
            public decimal consumed { get; set; }
            public decimal buyingprice { get; set; }
        }
        public class packinfo
        {
            public long productid { get; set; }
            public decimal qty { get; set; }
            public decimal available { get; set; }
        }
    }
    public class PackbarcodeHistory : BaseEntity
    {
        public long id { get; set; }
        public string barcode { get; set; }
        public long locationid { get; set; }
        public string locationname { get; set; }
        public long stockid { get; set; }
        public long salablegoodsid { get; set; }
        public decimal qty { get; set; }
        public string productname { get; set; }
        public decimal price { get; set; }
        public string sourcebarcode { get; set; }
        public long sourcestockid { get; set; }
        public long sourcesalablegoodsid { get; set; }
        public decimal sourceqty { get; set; }
        public string sourceproductname { get; set; }
        public decimal sourcebuyingprice { get; set; }
        public decimal sourcesellingprice { get; set; }
        public DateTime createdon { get; set; }
        public string createdby { get; set; }
        public DateTime fromdate { get; set; }
        public DateTime todate { get; set; }
    }
}
