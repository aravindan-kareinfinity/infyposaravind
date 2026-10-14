using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class Mfgjoborderitems : BaseEntity
    {
        public long stageid { get; set; }
        public long sizeid { get; set; }
        public long id { get; set; }
        public long bomid { get; set; }
        public decimal oqty { get; set; }
        public decimal rqty { get; set; }
        public decimal woqty { get; set; }
        public decimal sqty { get; set; }
        public decimal cqty { get; set; }
        public decimal cost { get; set; }
        public decimal aqty { get; set; }
        public long mfgworkorderid { get; set; }
        public long mfgworkorderitemsid { get; set; }
        public long mfgworkorderitemsizesid { get; set; }
        public decimal marginpercentage { get; set; }
        public decimal sellingprice { get; set; }
        public decimal taxpercentage { get; set; }
        public decimal tax { get; set; }
        public decimal amount { get; set; }
        public long taxid { get; set; }
        public long sourcejoborderitemid { get; set; }
        public int corestageid { get; set; }
        public decimal brqty { get; set; }
        public long jobreasonid { get; set; }
        public long jobchargeid { get; set; }
        public bool isactive { get; set; }
        public bool inwardgoods { get; set; }
        public bool inwardjob { get; set; }
        public long createdby { get; set; }
        public long modifiedby { get; set; }
        public int version { get; set; }
        public long mfgjoborderid { get; set; }
        public DateTime createdon { get; set; }
        public DateTime modifiedon { get; set; }
        public long bomcategoryid { get; set; }
        public int materialtype { get; set; }
        public int sno { get; set; }
        public Mfgworkfloworchestration.IOMODE e_MaterialType
        {
            get
            {
                return (Mfgworkfloworchestration.IOMODE)materialtype;
            }
        }
        public bool isbom { get; set; }
        public long bomconsumptionitemid { get; set; }
        public string width { get; set; }
        public bool dynamicbom { get;  set; }
        public long productid { get;  set; }
        public long colourid { get;  set; }
        public string itemdisplay { get; set; }
        public string typedisplay { get; set; }
        public bool isfabric { get; set; }
        public string bominwardcode { get; set; }
        public long bominvoiceitemid { get; set; }
        public string bominvoiceitemimage { get; set; }
        public bool supportbominvoice { get; set; }

        [JsonIgnore]
        public string productname { get; set; }
        [JsonIgnore]
        public string sizename { get; set; }
        [JsonIgnore]
        public string colourname { get; set; }
        [JsonIgnore]
        public string jobreason { get; set; }
        [JsonIgnore]
        public string stagename { get; set; }
        
        [JsonIgnore]
        public string jobworkcharge{ get; set; }
        [JsonIgnore]
        public string jobworkchargeorreason
        {
            get
            {
                if (string.IsNullOrEmpty(jobreason)) return jobworkcharge;
                return jobreason;
            }
        }

        [JsonIgnore]
        public MfgjoborderPrint parent { get; set; }
    }
    public class MfgjoborderitemsCreteria : Mfgjoborderitems
    {
        public List<long> sourcejoborderitemids { get;  set; }
    }
}
