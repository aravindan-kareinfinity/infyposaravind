using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class Loansettlement : BaseEntity
    {
        public long id { get; set; }
        public DateTime settlementon { get; set; }
        public DateTime billdate { get; set; }
        public long billid { get; set; }
        public long billsettlementmasterid { get; set; }
        public long loanproviderid { get; set; }
        public decimal discount { get; set; }
        public long customerid { get; set; }
        public decimal loanamount { get; set; }
        public decimal paid { get; set; }
        public string receiptno { get; set; }
        public string referenceno { get; set; }
        public string billno { get; set; }
        public string remarks { get; set; }
        public DateTime createdon { get; set; }
        public DateTime modifiedon { get; set; }
        public long createdby { get; set; }
        public long modifiedby { get; set; }
        public bool isactive { get; set; }
        public int version { get; set; }
        public long companyid { get; set; }
        public long locationid { get; set; }
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string attributes_json { get { return Newtonsoft.Json.JsonConvert.SerializeObject(attributes); } set { attributes = Newtonsoft.Json.JsonConvert.DeserializeObject<Attributes>(value); } }
        public Attributes attributes { get; set; }
        public class Attributes
        {
        }
    }

    public class LoansettlementDetail : Loansettlement
    {
        public string customer { get; set; }
        public string area { get; set; }
        public string mobileno {get;set;}
    }

    public class LoandSettlementPayment : BaseEntity
    {
        public List<LoansettlementDetail> items { get; set; }
        public decimal paid{ get; set; }
        public decimal allocated { get; set; }
        public decimal discount { get; set; }
        public string receiptno { get; set; }
        public string remarks { get; set; }
        public DateTime settlementon { get; set; }
    }


    public class LoansettlementCreteria : Loansettlement
    {
        public int searchmode { get; set; }
        public DateTime fromdate { get; set; }
        public DateTime todate { get; set; }
        public enum Modes
        {
            All,
            FullPaid,
            PartialPaid,
            OnlyPaid,
            Pending
        }
    }
}
