using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class BOMLrentry : BaseEntity
    {
       
        public Inbox.StatusList Status
        {
            get
            {
                return (Inbox.StatusList)status;
            }
            set
            {
                status = (int)value;
            }
        }
        public long id { get; set; }
        public long lrcompanyid { get; set; }
        public string lrno { get; set; }
        public DateTime BOMLrentrydate { get; set; }
        public string invoiceno { get; set; }
        public DateTime invoicedate { get; set; }
        public string slipno { get; set; }
        public DateTime slipdate { get; set; }

        public string BOMLrentryno { get; set; }
        public DateTime receiveddate { get; set; }
        public DateTime lrdate { get; set; }
        public long receivinglocationid { get; set; }
        public long fromlocationid { get; set; }
        public long locationid { get; set; }
        public long retaillocationid { get; set; }
        public long entrylocationid { get; set; }
        public long purchasemanagerid { get; set; }
        public int stockholdingperiod { get; set; }
        public long supplierid { get; set; }
        public long sourcesupplierid { get; set; }
        public bool hasfreightcharge { get; set; }
        public int noofbundels { get; set; }
        public int noofpieces { get; set; }
        public decimal weight { get; set; }
        public string issuedlrno { get; set; }
        public long issuedlrid { get; set; }
        public decimal totalamount { get; set; }
        public decimal premargin { get; set; }
        public DateTime modifiedon { get; set; }
        public DateTime createdon { get; set; }
        public long agentid { get; set; }
        public DateTime duedate { get; set; }
        public long createdby { get; set; }
        public long modifiedby { get; set; }
        public int version { get; set; }
        public bool isactive { get; set; }
        public int status { get; set; }
        public long transportid { get; set; }
        public int modeofdelivery { get; set; }
        public int paymentmode { get; set; }
        public int noofboxes { get; set; }
        public string lrids { get; set; }

        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string attachment_json { get { return Newtonsoft.Json.JsonConvert.SerializeObject(attachment); } set { attachment = Newtonsoft.Json.JsonConvert.DeserializeObject<List<slide>>(value); } }

        public List<slide> attachment { get; set; }
        public class slide
        {
            public string title { get; set; }
            public string image { get; set; }
        }
        public string ErrorMessage { get; set; }
    }
    public class BOMLrentryCreteria : BOMLrentry
    {
        public DateTime fromdate { get; set; }
        public DateTime todate { get; set; }

    }

    //public class LrInformation
    //{
    //    public string BOMLrentryno { get; set; }
    //    public DateTime lrdate { get; set; }
    //    public string name { get; set; }
    //    public string invoiceno { get; set; }
    //    public DateTime invoicedate { get; set; }
    //    public decimal grossamount { get; set; }
    //}
}
