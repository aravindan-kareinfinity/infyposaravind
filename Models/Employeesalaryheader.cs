using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class Employeesalaryheader : BaseEntity
    {
        public long id { get; set; }
        public long employeeid { get; set; }
        public string employeecode { get; set; }
        public DateTime fromdate { get; set; }
        public DateTime applyfrom { get; set; }
        public bool hasarriyar { get; set; }
        public bool iscurrent { get; set; }
        public decimal basicpay { get; set; }
        public bool eligibleesi { get; set; }
        public bool eligiblepf { get; set; }
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string configuration_json { get { return Newtonsoft.Json.JsonConvert.SerializeObject(configuration); } set { configuration = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Models.Employeesalarystructure.Configuration>>(string.IsNullOrEmpty(value) ? "null" : value); } }
        public List<Models.Employeesalarystructure.Configuration> configuration { get; set; }

        public bool isactive { get; set; }
        public long createdby { get; set; }
        public long modifiedby { get; set; }
        public int version { get; set; }
        public DateTime createdon { get; set; }
        public DateTime modifiedon { get; set; }
        public bool pfselfpaid { get; set; }
        public bool esiselfpaid { get; set; }
        public decimal grosspay { get; set; }
        public decimal lastgross { get; set; }
        public long inchargeid { get; set; }
        public decimal incentiveratio { get; set; }
        public long salarystructureid { get; set; }
        public decimal salestarget { get; set; }
        public int incentivesource { get; set; }

        public int incentivecalculationsource { get; set; }
        public int incentivevaluetype { get; set; }
        public int salestargetminimummode { get; set; }
        public decimal salesminimumtarget { get; set; }

        public int salestargetmethod { get; set; }

        public decimal ctc { get; set; }
        public decimal incentivepercentage { get; set; }

        [JsonIgnore]
        public IncentiveSources IncentiveFromSource
        {
            get
            {
                return (IncentiveSources)incentivesource;
            }
            set
            {
                incentivesource = (int)value;
            }
        }

        public enum IncentiveSources
        {
            TotalSales = 1,
            LocationSales = 2,
            SectionSales = 3,
            TeamSales = 4,
            FloorSales = 5,
            Salesmen = 6
        }
    }
    public class EmployeesalaryheaderCreteria : Employeesalaryheader
    {
        public long sectionid { get; set; }
        public long departmentid { get; set; }
        public long companyid { get; set; }
        public bool showallyears { get; set; }
        public bool showunassigned { get; set; }
        public int workingmode { get; set; }
        public int salarymode { get; set; }
        public bool showhasleft { get; set; }
        public DateTime dojfromdate { get; set; }
        public DateTime dojtodate { get; set; }
        public bool showesi { get; set; }
        public bool showpf { get; set; }
        public bool pfeligible { get; set; }
        public bool esieligible { get; set; }
        public bool showconfig { get; set; }
    }

    public class IncentiveConfig : Employeesalaryheader
    {
        public class EmployeeID
        {
            public long employeeid { get; set; }
            public long salaryheaderid { get; set; }
        }
        public List<EmployeeID> ids { get; set; }
        public bool changesupervisor { get; set; }
        public bool changeincentiveconfig { get; set; }
        public long supervisorid { get; set; }

        public bool changeesi { get; set; }
        public bool changepf { get; set; }
    }
    public class EmployeeSalaryInfo : Employeesalaryheader
    {
        public long salaryheaderid { get; set; }
        public long designationid { get; set; }
        public string name { get; set; }
        public DateTime dateofjoining { get; set; }
        public string structurename { get; set; }
        public string gender { get; set; }

        //public bool showesi { get; set; }
        //public bool showpf { get; set; }
        //public bool pfeligible { get; set; }
        //public bool esieligible { get; set; }
    }
}
