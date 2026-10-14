using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class Employeetarget : BaseEntity
    {
        public long id { get; set; }
        public long employeeid { get; set; }
        public int version { get; set; }
        public long sectionid { get; set; }
        public long locationid { get; set; }
        public int year { get; set; }
        public int month { get; set; }
        public decimal ratio { get; set; }
        public decimal salary { get; set; }
        public decimal sales { get; set; }
        public decimal target { get; set; }
        public decimal dailysales { get; set; }
        public decimal dailytarget { get; set; }
        public bool isactive { get; set; }
        public long createdby { get; set; }
        public long modifiedby { get; set; }
        public DateTime createdon { get; set; }
        public DateTime modifiedon { get; set; }
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string attribs_json
        {
            get { return Newtonsoft.Json.JsonConvert.SerializeObject(attribs); }
            set
            {
                if (!string.IsNullOrEmpty(value) && value != "null")
                    attribs = Newtonsoft.Json.JsonConvert.DeserializeObject<Attribs>(value);
            }
        }
        public Attribs attribs { get; set; }
        public class Attribs
        {
            public decimal eligiblepercentage { get; set; }
            public int incentivemode { get; set; }
            public decimal inc_freshpercentage { get; set; }
            public decimal inc_oldpercentage { get; set; }
            public decimal inc_stdpercentage { get; set; }
            public decimal cnf_freshdaypercentage { get; set; }
            public int cnf_monthdays { get; set; }
            public int cnf_freshday { get; set; }
            public DateTime cnf_freshdate { get; set; }
        }
    }
    public class EmployeetargetDetail : Employeetarget
    {
        public string employeecode { get; set; }
        public string name { get; set; }
        public bool hasmultisection { get; set; }
        public string section { get; set; }
        public decimal otarget { get; set; }
        public decimal oldsales { get; set; }
        public decimal freshsales { get; set; }
        public decimal acheivedtarget { get; set; }
        public decimal acheivedftarget { get; set; }
        public decimal acheivedotarget { get; set; }
        public decimal frstarget { get; set; }
        public decimal oldtarget { get; set; }
        public decimal incentive { get; set; }
    }
    public class EmployeetargetCreteria : Employeetarget
    {
        public List<long> employeeids { get; set; }
        public DateTime sourcefrom { get; set; }
        public DateTime sourceto { get; set; }
        public DateTime targetfrom { get; set; }
        public DateTime targetto { get; set; }
        public int billsource { get; set; }
        public decimal targetpercentage { get; set; }
        public List<long> sectionids { get; set; }
        public List<long> locationlist { get; set; }
        public int monthdays { get; set; }
        public int freshday { get; set; }
        public DateTime freshdate { get; set; }
        public decimal eligiblepercentage { get; set; }
        public decimal freshdaypercentage { get; set; }
        public int incentivemode { get; set; }
        public decimal inc_freshpercentage { get; set; }
        public decimal inc_oldpercentage { get; set; }
        public decimal inc_stdpercentage { get; set; }
        public bool datebased { get; set; }
        public bool overrideincentive { get; set; }
    }
}
