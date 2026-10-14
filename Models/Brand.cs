using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class Brand : BaseEntity
    {
        public string ErrorMessage { get; set; }
        public long id { get; set; }
        public string code { get; set; }
        public string name { get; set; }
        public string printingname { get; set; }
        public string barcodeprefix { get; set; }
        public string logoid { get; set; }
        public string barcodesuffix { get; set; }
        public int discountmode { get; set; }
        public decimal discountvalue { get; set; }
        public long preferredsupplierid { get; set; }
        public bool isactive { get; set; }
        public long createdby { get; set; }
        public int groupid { get; set; }
        public long modifiedby { get; set; }
        public int version { get; set; }
        public DateTime createdon { get; set; }
        public DateTime modifiedon { get; set; }
        public decimal minmargin { get; set; }
        public decimal maxmargin { get; set; }

        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string marginlist_json
        {
            get { return Newtonsoft.Json.JsonConvert.SerializeObject(marginlist); }
            set { marginlist = Newtonsoft.Json.JsonConvert.DeserializeObject<List<MarginInfo>>(value); }
        }
        public List<MarginInfo> marginlist { get; set; }
        public class MarginInfo
        {
            public long productid { get; set; }
            public decimal margin { get; set; }
        }
    }
    public class BrandCreteria : Brand
    {
        public List<long> ids { get; set; }
        public bool inactive { get; set; }
    }

    public class MergeBrandList : BaseEntity
    {
        public long id { get; set; }
        public string type { get; set; }
        public List<Brand> deletelist { get; set; }
        public string ErrorMessage { get; set; }

    }
}
