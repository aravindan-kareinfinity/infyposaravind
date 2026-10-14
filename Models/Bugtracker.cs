using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class Bugtracker : BaseEntity
    {
        public long id { get; set; }
        public long companyid { get; set; }
        public long locationid { get; set; }
        public long typeid { get; set; }
        public long priorityid { get; set; }
        public long statusid { get; set; }
        public string requestedby { get; set; }
        public DateTime requestedon { get; set; }
        public string contactno { get; set; }
        public string bugdetails { get; set; }
        public long assignedby { get; set; }
        public DateTime assignedon { get; set; }
        public long resolvedby { get; set; }
        public DateTime resolvedon { get; set; }
        public bool isactive { get; set; }
        public int version { get; set; }
        public long modifiedby { get; set; }
        public long createdby { get; set; }
        public DateTime createdon { get; set; }
        public DateTime modifiedon { get; set; }
        public string bugno { get; set; }
        public string comments { get; set; }
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string attachment_json { get { return Newtonsoft.Json.JsonConvert.SerializeObject(attachment); } set { attachment = Newtonsoft.Json.JsonConvert.DeserializeObject<List<slide>>(value); } }

        public List<slide> attachment { get; set; }
        public class slide
        {
            public string title { get; set; }
            public string image { get; set; }
        }

    }
    public class BugtrackerCreteria : Bugtracker
    {
        public DateTime fromdate { get; set; }
        public DateTime todate { get; set; }
    }
}
