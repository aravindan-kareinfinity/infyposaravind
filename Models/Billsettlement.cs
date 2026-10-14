using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class Billsettlement : BaseEntity
    {
        public long locationid { get; set; }
        [JsonIgnore]
        public bool IsSynchronize { get; set; }
        public long id { get; set; }
        public bool isactive { get; set; }
        public long createdby { get; set; }
        public long modifiedby { get; set; }
        public int version { get; set; }
        public DateTime createdon { get; set; }
        public DateTime modifiedon { get; set; }
        public DateTime receiptdate { get; set; }
        public string receiptno { get; set; }
        public decimal paid { get; set; }
        public decimal receivable { get; set; }
        public long billid { get; set; }
        public string billno { get; set; }
        public long companyid { get; set; }
        public decimal billamount { get; set; }
        public decimal discount { get; set; }
        public decimal billadditionaldiscount { get; set; }
        public long discountapprovedby { get; set; }
        public int mode { get; set; }
        public long billsettlementmasterid { get; set; }
        public long customerid { get; set; }
        public decimal additionaldiscount { get; set; }
        public string discountreason { get; set; }
        public bool HasDiscount { get; set; }
        public decimal adjustablereward { get; set; }
        public DateTime billdate { get; set; }
        [JsonIgnore]
        public string deliveryno { get; set; }
        [JsonIgnore]
        public bool hasgift
        {
            get
            {
                return settlementgift != null && settlementgift.Count > 0;
            }
        }
        [JsonIgnore]
        public List<BillsettlementGift> settlementgift { get; set; }
        [JsonIgnore]
        public BillsettlementGift singlesettlementgift
        {
            get
            {
                if (hasgift) return settlementgift.First();
                return null;
            }
        }
        public int orderno { get; set; }
    }
    public class BillsettlementGift
    {
        public string code { get; set; }
        public string name { get; set; }
        public decimal amount { get; set; }
        public decimal qty { get; set; }
    }
    public class BillsettlementCreteria : Billsettlement
    {
        public List<long> billids { get; set; }
    }
}
