using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class Leadmanagement : BaseEntity
    {
        public string ErrorMessage { get; set; }
        public long id { get; set; }
        public string leadno { get; set; }
        public long customerid { get; set; }
        public long counterid { get; set; }
        public string customername { get; set; }
        public string contactno { get; set; }
        public long eventtypeid { get; set; }
        public long assignedbyid { get; set; }
        public string assignedby { get; set; }
        public string assignedto { get; set; }
        public long assignedid { get; set; }
        public long companyid { get; set; }
        public long locationid { get; set; }
        public long priorityid { get; set; }
        public long statusid { get; set; }
        public string remark { get; set; }
        public DateTime loggedon { get; set; }
        public DateTime eventon { get; set; }
        public DateTime followupon { get; set; }
        public string vouchercode { get; set; }
        public long voucherid { get; set; }
        public long billid { get; set; }
        public string billno { get; set; }
        public DateTime billdate { get; set; }
        public int vouchertypeid { get; set; }
        public bool isactive { get; set; }
        public int version { get; set; }
        public long modifiedby { get; set; }
        public long createdby { get; set; }
        public DateTime createdon { get; set; }
        public DateTime modifiedon { get; set; }
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string calllog_json { get { return Newtonsoft.Json.JsonConvert.SerializeObject(calllog); } set { calllog = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Calllog>>(value); } }

        public List<Calllog> calllog { get; set; }
        public class Calllog
        {
            public string contactperson { get; set; }
            public string contactnumber { get; set; }
            public DateTime calllogdate { get; set; }
            public string callresponse { get; set; }
        }
        public SchemeCoupon leadcoupon { get; set; }
        public GiftVoucher leadvoucher { get; set; }
        public bool isconsumed { get; set; }

        [JsonIgnore]
        public Company company { get; set; }
        [JsonIgnore]
        public Referencelist location { get; set; }
        [JsonIgnore]
        public Customer customer { get; set; }
        public string eventtypename { get; set; }
        public string priorityname { get; set; }
        public string statusname { get; set; }
    }
    public class LeadmanagementCreteria : Leadmanagement
    {
        public DateTime fromdate { get; set; }
        public DateTime todate { get; set; }
    }

}
