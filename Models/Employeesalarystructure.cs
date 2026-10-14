using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{

    public class EmployeesalarystructureReplace : BaseEntity
    {
        public long id { get; set; }
        public string source { get; set; }
        public string replace { get; set; }
        public long headerid { get; set; }
    }
    public class Employeesalarystructure : BaseEntity
    {
        public long id { get; set; }
        public string name { get; set; }
        public DateTime fromdate { get; set; }
        public DateTime todate { get; set; }
        public int mode { get; set; }
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string configuration_json { get { return Newtonsoft.Json.JsonConvert.SerializeObject(configuration); } set { configuration = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Configuration>>(value); } }
        public List<Configuration> configuration { get; set; }
        public List<Models.Employeesalarystructure.Configuration> configclone
        {
            get
            {
                return Newtonsoft.Json.JsonConvert.DeserializeObject<List<Models.Employeesalarystructure.Configuration>>(Newtonsoft.Json.JsonConvert.SerializeObject(configuration));
            }
        }
        public bool isactive { get; set; }
        public long createdby { get; set; }
        public long modifiedby { get; set; }
        public int version { get; set; }
        public DateTime createdon { get; set; }
        public DateTime modifiedon { get; set; }
        public decimal fixedbasic { get; set; }
        public decimal fixedda { get; set; }
        public long deducttextsalaryid { get; set; }
        public long splallowanceid { get; set; }

        
        public class Configuration
        {
            public decimal value { get; set; }
            public string code { get; set; }
            public long id { get; set; }
            public string formula { get; set; }
        }
    }
    public class EmployeesalarystructureCreteria : Employeesalarystructure
    {
    }
}
