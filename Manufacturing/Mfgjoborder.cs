using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class Mfgjoborder : BaseEntity
    {
        public Mfgjoborder()
        {
            corestageid = -1;
        }
        public long id { get; set; }
        public DateTime expectedate { get; set; }
        public long supplierid { get; set; }
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string termsandcondition_json { get { return Newtonsoft.Json.JsonConvert.SerializeObject(termsandcondition); } set { termsandcondition = Newtonsoft.Json.JsonConvert.DeserializeObject<Termsandcondition>(value); } }
        public Termsandcondition termsandcondition { get; set; }
        public bool isactive { get; set; }
        public bool inwardjob { get; set; }
        public bool inwardgoods { get; set; }
        public long createdby { get; set; }
        public long modifiedby { get; set; }
        public int version { get; set; }
        public DateTime createdon { get; set; }
        public DateTime modifiedon { get; set; }
        public long fromlocationid { get; set; }
        public int status { get; set; }
        public long tolocationid { get; set; }
        public decimal tolerance { get; set; }
        public int freight { get; set; }
        public long issuedby { get; set; }
        public long receivedby { get; set; }
        public DateTime receivedon { get; set; }
        public long companyid { get; set; }
        public long bominvoiceid { get; set; }
        public long bomrequestid { get; set; }
        public string bomentryno { get; set; }
        public decimal tax { get; set; }
        public decimal taxable { get; set; }
        public decimal total { get; set; }
        public string workorderno { get; set; }
        public long mfgworkorderid { get; set; }
        public string mode { get; set; }
        public long stageid { get; set; }
        public long mfgstageid { get; set; }
        public string jobcardno { get; set; }
        public DateTime joborderdate { get; set; }
        public string joborderno { get; set; }
        public int sourcematerial { get; set; }
        public int outcomematerial { get; set; }
        public List<Mfgjoborderitems> Mfgjoborderitems { get; set; }
        public string stagename { get; set; }
        public bool isbomprocess { get; set; }
        public long workflowid { get; set; }
        public long sourcejobid { get; set; }
        public string sourcejobno { get; set; }
        public long inwardedby { get; set; }
        public DateTime inwardeddate { get; set; }
        public class Termsandcondition
        {
        }
        public Mfgworkorder mfgworkorder { get; set; }
        public int corestageid { get; set; }

        public string invoiceno { get; set; }
        public DateTime invoicedate { get; set; }
        [JsonIgnore]
        public Mfgsampleorder mfgsampleorder { get; set; }
    }
    public class MfgjoborderCreteria : Mfgjoborder
    {
        public DateTime fromdate { get; set; }
        public DateTime todate { get; set; }
        public long productid { get; set; }
        public long brandid { get; set; }
        public long locationid { get; set; }
        public string design { get; set; }
        public bool showonlypending { get; set; }
        public string type { get; set; }

        public bool onlybom { get; set; }
    }

    public class Mfgjoborderstage : BaseEntity
    {
        public bool inwardjob { get; set; }
        public long workflowid { get; set; }
        public long workorderid { get; set; }
        public int stageid { get; set; }
        public int corestageid { get; set; }
        public long joborderid { get; set; }
    }


    public class MfgjoborderSummary
    {
        public long locationid { get; set; }
        public DateTime joborderdate { get; set; }
        public DateTime wodate { get; set; }
        public string workorderno { get; set; }
        public string jobworkname { get; set; }
        public string joborderno { get; set; }
        public string jobcardno { get; set; }
        public long mfgjoborderid { get; set; }
        public long mfgworkorderid { get; set; }
        public long supplierid { get; set; }
        public string designitemname { get; set; }
        public string colourname { get; set; }
        public string product { get; set; }
        public int corestageid { get; set; }
        public decimal issuedqty { get; set; }
        public decimal receivedqty { get; set; }
        public decimal wasteqty { get; set; }
        public string stage { get; set; }
    }
    public class MfgjoborderPrint : Mfgjoborder
    {
        public Company company { get; set; }
        public CompanyLocation location { get; set; }
        public Supplier supplier { get; set; }
        public Suppliercompany suppliercompany { get; set; }
        public Transport transport { get; set; }
        public Agent agent { get; set; }
        public string whocreated { get; set; }
        public string whoapproved { get; set; }
        public bool hassupplier
        {
            get
            {
                return supplier != null;
            }
        }
    }
}
