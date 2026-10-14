using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using static InfyPOS.Models.Supplierproductgroups;

namespace InfyPOS.Models
{
    public class Suppliertradeagreement : BaseEntity
    {
        public long id { get; set; }
        public bool isactive { get; set; }
        public long createdby { get; set; }
        public long modifiedby { get; set; }
        public int version { get; set; }
        public DateTime createdon { get; set; }
        public DateTime modifiedon { get; set; }
        public long productgroupid { get; set; }
        public long Supplierproductgroupsid { get; set; }
        public int cur_seq { get; set; }
        public decimal mrp { get; set; }
        public bool disable { get; set; }
        public DateTime disabledon { get; set; }
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string items_json { get { return Newtonsoft.Json.JsonConvert.SerializeObject(items); } set { items = Newtonsoft.Json.JsonConvert.DeserializeObject<List<MatrixItem>>(value); } }
        public List<MatrixItem> items { get; set; }
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string matrixlist_json { get { return Newtonsoft.Json.JsonConvert.SerializeObject(matrixlist); } set { matrixlist = Newtonsoft.Json.JsonConvert.DeserializeObject<List<matrixconfig>>(value); } }
        public List<matrixconfig> matrixlist { get; set; }
        public long supplierid { get; set; }
        public long approvedby { get; set; }
        public long disabledby { get; set; }
        public long brandid { get; set; }
        public DateTime agreementon { get; set; }
        public decimal cost { get; set; }
        public decimal price { get; set; }
        public string itemcode { get; set; }
        public string marginmode { get; set; }
        public string code { get; set; }
        public string agreementname { get; set; }
        public string barcodemode { get; set; }
        public long namingmode { get; set; }
        public class MatrixItem : BaseEntity
        {
            public long itemid { get; set; }
            public string gln { get; set; }
            public bool nonuan { get; set; }
            public string printingname { get; set; }
            public decimal cost { get; set; }
            public decimal price { get; set; }
            public decimal mrp { get; set; }
            public long brandid { get; set; }
            public long sizeid { get; set; }
            public long styleid { get; set; }
            public long colourid { get; set; }
            public long patternid { get; set; }
            public long materialid { get; set; }
            public long typeid { get; set; }
            public long sleeveid { get; set; }
            public long fitid { get; set; }
        }

        public class matrixconfig
        {
            public string attributetype { get; set; }
            public List<attributeid> items { get; set; }
            public class attributeid
            {
                public long id { get; set; }
            }
        }
    }

    public class SupplierTransactionWithMatrix : Suppliertradeagreement
    {
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string attributelist_json { get { return Newtonsoft.Json.JsonConvert.SerializeObject(attributelist); } set { attributelist = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Attributelist>>(value); } }
        public List<Attributelist> attributelist { get; set; }
        public bool updateitemonly { get; set; }
        public bool updateglnonly { get; set; }

    }

    public class Suppliertradeagreementdetail : Suppliertradeagreement
    {
        public string itemname { get; set; }
        public string itemgln { get; set; }
        public decimal itemcost { get; set; }
        public decimal itemmrp { get; set; }
        public decimal itemprice { get; set; }

    }
    public class SuppliertradeagreementCreteria : Suppliertradeagreement
    {
        public List<long> Supplierproductgroupsids { get; set; }
        public string uan { get; set; }
        public string itemname { get; set; }
        public DateTime fromcreated { get; set; }
        public DateTime fromdisabled { get; set; }
        public DateTime fromodified { get; set; }
        public DateTime tocreated { get; set; }
        public DateTime todisabled { get; set; }
        public DateTime tomodified { get; set; }
        public bool summaryview { get; set; }
        public bool showdetail { get; set; }
    }
}
