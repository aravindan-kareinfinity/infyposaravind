using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class Mfgtailoringwages : BaseEntity
    {
        public int qty { get; set; }
        public long id { get; set; }
        public long createdby { get; set; }
        public int version { get; set; }
        public long modifiedby { get; set; }
        public long employeeid { get; set; }
        public DateTime modifiedon { get; set; }
        public DateTime createdon { get; set; }
        public decimal jobcharge { get; set; }
        public decimal paidcharge { get; set; }
        public DateTime fromdate { get; set; }
        public DateTime todate { get; set; }
        public bool isactive { get; set; }
        public string receiptno { get; set; }
        public DateTime paidon { get; set; }
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string mfgtailoringids_json { get { return Newtonsoft.Json.JsonConvert.SerializeObject(mfgtailoringids); } set { mfgtailoringids = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Mfgtailoringids>>(value); } }
        public List<Mfgtailoringids> mfgtailoringids { get; set; }
        public class Mfgtailoringids
        {
            public long id { get; set; }
            public decimal amount { get; set; }
        }
    }
    public class MfgtailoringwagesCreteria : Mfgtailoringwages
    {
        public string status { get; set; }
    }
}
