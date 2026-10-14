
using InfyPOS.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class Addondata : BaseEntity
    {
        public bool isactive { get; set; }
        public long modifiedby { get; set; }
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string customdata_json
        {
            get
            {
                if (customdata != null)
                    return customdata.ToString();
                if (customArraydata != null)
                    return customArraydata.ToString();
                return "";
            }
            set
            {

                if (string.IsNullOrEmpty(value))
                {
                    customdata = null;
                    customArraydata = null;
                }
                else if (value.StartsWith("["))
                {
                    customArraydata = JArray.Parse(value);
                }
                else
                {
                    customdata = JObject.Parse(value);
                }
            }
        }
        public JObject customdata { get; set; }
        public JArray customArraydata { get; set; }
        public long companyid { get; set; }
        public long referenceid { get; set; }
        public DateTime modifiedon { get; set; }
        public long id { get; set; }
        public long createdby { get; set; }
        public DateTime createdon { get; set; }
        public int version { get; set; }
        public string customtype { get; set; }
    }
    public class AddondataCreteria : Addondata
    {
    }

    public class Addon_ReportTargetMargin : Addondata
    {
        public DateTime salesfromdate { get; set; }
        public DateTime salestodate { get; set; }
        public DateTime targetfrom { get; set; }
        public DateTime targettodate { get; set; }
        public long locationid { get; set; }
        public long sectionid { get; set; }
        public List<long> productids { get; set; }
        public class ReportItem
        {
            public long productid { get; set; }
            public string productname { get; set; }
            //F.Stock
            public decimal configuredqty { get; set; }
            //C.Stock
            public decimal currentstock { get; set; }
            //target range sales
            public decimal targetqty { get; set; }
            //target range sales
            public decimal targetvalue { get; set; }
            public decimal targetivalue { get; set; }
            //target
            public decimal salesqty { get; set; }
            public decimal salevalue { get; set; }
            public decimal saleivalue { get; set; }
            public decimal stockqty { get; set; }
            public decimal stockbuyingprice { get; set; }
            public decimal stockibuyingprice { get; set; }

            public decimal salescost { get; set; }
            public decimal salesicost { get; set; }
            public decimal targetcost { get; set; }
            public decimal targeticost { get; set; }
            public List<ReportMarkerItem> markers { get; set; }
            public class ReportMarkerItem : ReportItem
            {
                public string name { get; set; }
                public long id { get; set; }
            }
        }
    }
}
