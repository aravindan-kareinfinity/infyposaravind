using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class Customerrewards : BaseEntity
    {
        public long id { get; set; }
        public bool isactive { get; set; }
        public long createdby { get; set; }
        public long modifiedby { get; set; }
        public int version { get; set; }
        public DateTime createdon { get; set; }
        public DateTime modifiedon { get; set; }
        public DateTime transactiondate { get; set; }
        public DateTime expiry { get; set; }
        public long customerid { get; set; }
        public decimal rewards { get; set; }
        public decimal adjusted { get; set; }
        public decimal billvalue { get; set; }
        public long billid { get; set; }
        public string billno { get; set; }
        public long locationid { get; set; }
        public bool altered { get; set; }
        public string remark { get; set; }
        [JsonIgnore]
        public DateTime customerdob { get; set; }
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string audithistory_json
        {
            get { return Newtonsoft.Json.JsonConvert.SerializeObject(audithistory); }
            set
            {
                if (value != null)
                {
                    audithistory = Newtonsoft.Json.JsonConvert.DeserializeObject<List<AuditHistory>>(value);
                }
            }
        }
        public List<AuditHistory> audithistory { get; set; }
        public class AuditHistory
        {
            public DateTime transactiondate { get; set; }
            public decimal oldrewards { get; set; }
            public decimal oldadjusted { get; set; }
            public decimal newrewards { get; set; }
            public decimal newadjusted { get; set; }
            public long updatedby { get; set; }
        }
    }
    public class CustomerrewardsCreteria : Customerrewards
    {
        public string loyeltyno { get; set; }
        public string mobileno { get; set; }
        public Models.Bill bill { get; set; }
        public bool OnlySettled { get; set; }
        public bool OnlyAdjusted { get; set; }
        [JsonIgnore]
        public Customer customer { get;  set; }
    }

    public class CustomerRewardStatus : BaseEntity
    {
        //public long thisbillrewards { get; set; }
        public string error { get; set; }
        public decimal billrewards { get; set; }
        public decimal rewards { get; set; }
        public decimal available { get; set; }
        public decimal convertable { get; set; }
        public decimal billvalue { get; set; }
        public long customerid { get; set; }
        public string customername { get; set; }
        public string mobileno { get; set; }
        public string loyeltyno { get; set; }
        public decimal rewardpercent { get;  set; }
    }
}
