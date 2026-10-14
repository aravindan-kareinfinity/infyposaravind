using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class Productrequestformitems : BaseEntity
    {
        public long id { get; set; }
        public long productid { get; set; }
        public long supplierid { get; set; }
        public long itemid { get; set; }
        public decimal qty { get; set; }
        public decimal rqty { get; set; }
        public decimal cqty { get; set; }
        public bool isactive { get; set; }
        public long createdby { get; set; }
        public long modifiedby { get; set; }
        public int version { get; set; }
        public string designid { get; set; }
        public string pricerange { get; set; }
        public long productrequestformid { get; set; }
        public DateTime createdon { get; set; }
        public DateTime modifiedon { get; set; }
        public string description { get; set; }

        public Detail detail { get; set; }
        public class Detail
        {
            public long brandid { get; set; }
            public long colourid { get; set; }
            public long patternid { get; set; }
            public long typeid { get; set; }
            public long sizeid { get; set; }
            public long styleid { get; set; }
            public long materialid { get; set; }
            public long sleeveid { get; set; }
            public long fitid { get; set; }
            public List<Size> jumplist { get; set; }
            public class Size
            {
                public long id { get; set; }
                public decimal qty { get; set; }
            }
        }
    }
    public class ProductrequestformitemsCreteria : Productrequestformitems
    {
    }
}
