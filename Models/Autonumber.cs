using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class Autonumber : BaseEntity
    {
        public long id { get; set; }
        public bool isactive { get; set; }
        public long createdby { get; set; }
        public long modifiedby { get; set; }
        public int version { get; set; }
        public DateTime createdon { get; set; }
        public DateTime modifiedon { get; set; }
        public long identityfrom { get; set; }
        public long identigyto { get; set; }
        public long timeout { get; set; }
        public string description { get; set; }
        public string prefix { get; set; }
        public string suffix { get; set; }
        public long currentidentity { get; set; }
        public string filler { get; set; }
        public int nolength { get; set; }
        public string type { get; set; }
        public bool preincrement { get; set; }
        public long companyid { get; set; }
        public long locationid { get; set; }
        public long floorid { get; set; }
        public bool dailyreset { get; set; }

        public bool prefixexpiry { get; set; }
        public bool suffixexpiry { get; set; }
        public DateTime prefixexpiryon { get; set; }
        public DateTime suffixexpiryon { get; set; }
        public DateTime prefixvalidfrom { get; set; }
        public DateTime suffixvalidfrom { get; set; }

    }
    public class AutonumberCreteria : Autonumber
    {
        public bool iswarehouse { get; set; }
    }
}
