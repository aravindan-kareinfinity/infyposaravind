using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class Voucheritems : BaseEntity
    {
        public long id { get; set; }
        public long voucherid { get; set; }
        public bool iscredit { get; set; }
        public int typeid { get; set; }
        public long ledgerid { get; set; }
        public int bwise { get; set; }
        public decimal amount { get; set; }
        public decimal tds { get; set; }
        public long sledgerid { get; set; }
        public decimal rate { get; set; }
        public decimal qty { get; set; }
        public bool isinv { get; set; }
        public decimal taxable { get; set; }
        public decimal taxpercentage { get; set; }
        public string taxtype { get; set; }
        public bool isactive { get; set; }
        public long createdby { get; set; }
        public long modifiedby { get; set; }
        public int version { get; set; }
        public DateTime createdon { get; set; }
        public DateTime modifiedon { get; set; }
        public bool hastax { get; set; }
        public voucharattributes attributes { get; set; }

        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string attributes_json { get { return Newtonsoft.Json.JsonConvert.SerializeObject(attributes); } set { attributes = Newtonsoft.Json.JsonConvert.DeserializeObject<voucharattributes>(value); } }

        public long voucheritemstockid { get;  set; }

        public class voucharattributes
        {
            public bool hasgst { get; set; }
            public bool hastds { get; set; }
            public bool mantax { get; set; }
            public long ledgertdsid { get; set; }
            public int ledgertype { get; set; }
            public long customerid { get; set; }
            public long supplierid { get; set; }
            public long lrinvoicereturnid { get; set; }
            public int banktransfermode { get; set; }
            public string bankreference { get; set; }
            public string lguid { get; set; }
            public string narration { get; set; }
            public DateTime bankdate { get; set; }
        }
    }
    public class VoucheritemsCreteria : Voucheritems
    {
    }
}
