using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class Supplierproductgroups : BaseEntity
    {
        public long id { get; set; }
        public bool isactive { get; set; }
        public long createdby { get; set; }
        public long modifiedby { get; set; }
        public int version { get; set; }
        public DateTime createdon { get; set; }
        public DateTime modifiedon { get; set; }
        public long productgroupid { get; set; }
        public int cur_seq { get; set; }
        public string itemcode { get; set; }
        public long supplierid { get; set; }
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string attributelist_json { get { return Newtonsoft.Json.JsonConvert.SerializeObject(attributelist); } set { attributelist = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Attributelist>>(value); } }
        public List<Attributelist> attributelist { get; set; }
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string priceagreements_json { get { return Newtonsoft.Json.JsonConvert.SerializeObject(priceagreements); } set { priceagreements = Newtonsoft.Json.JsonConvert.DeserializeObject<List<PriceAgreement>>(value); } }
        public List<PriceAgreement> priceagreements { get; set; }

        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string items_json { get { return Newtonsoft.Json.JsonConvert.SerializeObject(priceagreements); } set { priceagreements = Newtonsoft.Json.JsonConvert.DeserializeObject<List<PriceAgreement>>(value); } }
        
        public string code { get; set; }
        public List<Suppliertradeagreement> tradeagreements { get;  set; }

        public class Attributelist
        {
            public string attributetype { get; set; }
            public string attributename { get; set; }
            public long attributeid { get; set; }
            public int attributetypeid { get; set; }
            public bool disable { get; set; }
        }
        public class PriceAgreement
        {
            public string itemcode { get; set; }
            public string agreementname { get; set; }
            public long approvedby { get; set; }
            public long brandid { get; set; }
            public DateTime agreementon { get; set; }
            public string marginmode { get; set; }
            public decimal cost { get; set; }
            public decimal price { get; set; }
            public decimal mrp { get; set; }
            public bool disable { get; set; }
            public int cur_seq { get; set; }
            public string barcodemode { get; set; }
            public DateTime disabledon { get; set; }
            public List<MatrixItem> items { get; set; }
            public List<matrixconfig> matrixlist { get; set; }
            public class matrixconfig
            {
                public string attributetype { get; set; }
                public List<attributeid> items { get; set; }
                public class attributeid
                {
                    public long id { get; set; }
                }
            }
            public class MatrixItem
            {
                public long itemid { get; set; }
                public string gln { get; set; }
                public string printingname { get; set; }
                public decimal cost { get; set; }
                public decimal price { get; set; }
                public decimal mrp { get; set; }
            }

        }

    }
    public class SupplierproductgroupsCreteria : Supplierproductgroups
    {
        public bool dailyproducts { get; set; }
        public long dailypricegroupid { get; set; }
        public bool includeitems { get; set; }
        
    }
}
