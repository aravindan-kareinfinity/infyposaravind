using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class Gatepass : BaseEntity
    {
        public long modifiedby { get; set; }
        public long locationid { get; set; }
        public bool isactive { get; set; }
        public int version { get; set; }
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string attachments_json { get { return Newtonsoft.Json.JsonConvert.SerializeObject(attachments); } set { attachments = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Attachments>>(value); } }
        public List<Attachments> attachments { get; set; }
        public DateTime actionon { get; set; }
        public bool isoutward { get; set; }
        public long packagereferenceid { get; set; }
        public long prddivisionid { get; set; }
        public DateTime packagedate { get; set; }
        public decimal packagevalue { get; set; }
        public decimal packageqty { get; set; }
        public long packageuom { get; set; }
        public DateTime createdon { get; set; }
        public long createdby { get; set; }
        public long id { get; set; }
        public DateTime modifiedon { get; set; }
        public long actionby { get; set; }
        public string gatepassno { get; set; }
        public string vehicleno { get; set; }
        public string packagetype { get; set; }
        public string packagecode { get; set; }
        public string vendor { get; set; }
        public string transport { get; set; }
        public string remarks { get; set; }
        public string pono { get; set; }
        public string city { get; set; }
        public string driver { get; set; }
        public long inchargeid { get; set; }
        public string drivercontactno { get; set; }
        public long vendorid { get;  set; }
        public long cityid { get; set; }

        public class Attachments
        {
            public string image { get; set; }
            public string name { get; set; }
        }
    }
    public class GatepassCreteria : Gatepass
    {
        public string movement { get; set; }
        public bool showtoday { get; set; }
        public DateTime addedfrom { get; set; }
        public DateTime addedto { get; set; }
    }
}
