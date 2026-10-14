using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class Mfgdesigncost : BaseEntity
    {
        public long id { get; set; }
        public long mfgdesignid { get; set; }
        public long createdby { get; set; }
        public long modifiedby { get; set; }
        public DateTime createdon { get; set; }
        public DateTime modifiedon { get; set; }
        public bool isactive { get; set; }
        public int version { get; set; }
        public long sizeid { get; set; }
        public long colourid { get; set; }
        public long mfgcolourid { get; set; }
        public long workflowid { get; set; }
        public long stageid { get; set; }
        public int type { get; set; }
        public int mode { get; set; }
        public long bomid { get; set; }
        public long categoryid { get; set; }
        public long uomid { get; set; }
        public decimal qty { get; set; }
        public decimal amount { get; set; }
        public long chargeid { get; set; }
        public string sizegroupid { get; set; }
        public string uniqueid { get; set; }
    }
    public class MfgdesigncostCreteria : Mfgdesigncost
    {
        public List<long> mfgdesignids { get; set; }
    }
}
