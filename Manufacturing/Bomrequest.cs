using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using static InfyPOS.Models.Mfgworkfloworchestration;

namespace InfyPOS.Models
{
    public class BOMRequest : BaseEntity
    {

        public int activestatus { get; set; }
        [JsonIgnore]
        public ActiveStatus Activestatus
        {
            get
            {
                return (ActiveStatus)activestatus;
            }
            set
            {
                activestatus = (int)value;
            }
        }
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string items_json { get { return Newtonsoft.Json.JsonConvert.SerializeObject(items); } set { items = Newtonsoft.Json.JsonConvert.DeserializeObject<List<BOMConsumptionitemsDetail>>(value); } }
        public List<BOMConsumptionitemsDetail> items { get; set; }
        public int freight { get; set; }
        public long id { get; set; }
        public DateTime entrydate { get; set; }
        public long tolocationid { get; set; }
        public long bundleid { get; set; }
        public int status { get; set; }

        public enum Status
        {
            None,
            Approved,
            Rejected,
            Ready,
            Issued,
            Received,
            Consumed
        }
        public long modifiedby { get; set; }
        public long preparedby { get; set; }
        public long approvedby { get; set; }
        public long companyid { get; set; }
        public bool isactive { get; set; }
        public long createdby { get; set; }
        public int version { get; set; }
        public DateTime createdon { get; set; }
        public DateTime modifiedon { get; set; }
        public long mfgjoborderid { get; set; }
        public long mfgstageid { get; set; }
        public long bomconsumptionid { get; set; }
        public long mfgworkorderid { get; set; }
        public long fromlocationid { get; set; }
        public string entryno { get; set; }
        public string bundlecode { get; set; }
        public string workorderno { get; set; }
        public string joborderno { get; set; }
        public long supplierid { get; set; }
        public int mode { get; set; }
        public string remarks { get; set; }
    }
    public class BomrequestCreteria : BOMRequest
    {
        public DateTime addedfrom { get; set; }
        public DateTime addedto { get; set; }
        public bool create { get; set; }
        public bool consume { get; set; }
        public int statusid { get; set; }
    }

    public class BOMStatus : BaseEntity
    {
        public List<long> ids { get; set; }
        public string status { get; set; }
        public long employeeid { get; set; }
        public DateTime statuson { get; set; }
    }

    public class BomInboxItem : BaseEntity
    {
        public long id { get; set; }
        public DateTime entrydate { get; set; }
        public int status { get; set; }
        public int mode { get; set; }
        public long preparedby { get; set; }
        public long approvedby { get; set; }
        public long mfgjoborderid { get; set; }
        public long mfgstageid { get; set; }
        public long mfgworkorderid { get; set; }
        public string entryno { get; set; }
        public string bundlecode { get; set; }
        public string mfgwojono { get; set; }
        public string remarks { get; set; }
        public string stagename { get; set; }
        public string workorderno { get; set; }
        public DateTime wodate { get; set; }
        public string joborderno { get; set; }
        public DateTime joborderdate { get; set; }
        public string fromlocation { get; set; }
        public string tolocation { get; set; }
        public string approvedemp { get; set; }

    }
}
