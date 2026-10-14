using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class CRMConfigFields : BaseEntity
    {
        public string fld_mobileno { get; set; }
        public string fld_firstname { get; set; }
        public string fld_lastname { get; set; }
        public string fld_mailid { get; set; }
        public string fld_addressline1 { get; set; }
        public string fld_addressline2 { get; set; }
        public string fld_area { get; set; }
        public string fld_city { get; set; }
        public string fld_state { get; set; }
        public string fld_postcode { get; set; }
        public string fld_dob { get; set; }
        public string fld_religion { get; set; }
        public string fld_marital { get; set; }
        public string fld_sp_firstname { get; set; }
        public string fld_sp_lastname { get; set; }
        public string fld_sp_dob { get; set; }
        public string fld_dom { get; set; }
        public string fld_source { get; set; }
        public string fld_often { get; set; }
        public string fld_gender { get; set; }

    }
    public class CRMConfig : CRMConfigFields
    {
        public string desktopbanner { get; set; }
        public int desktopbannerheight { get; set; }
        public int desktopbannerwidth { get; set; }
        public string mobilebanner { get; set; }
        public int mobilebannerheight { get; set; }
        public int mobilebannerwidth { get; set; }
        public string feedbackmobileno { get; set; }
        public string feedbackname { get; set; }
        public string feedbackrating { get; set; }
        public bool feedbackserialno { get; set; }

        public bool enb_deskbanner { get; set; }
        public bool enb_mobilebanner { get; set; }
        public bool enb_mobileno { get; set; }
        public bool enb_firstname { get; set; }
        public bool enb_lastname { get; set; }
        public bool enb_mailid { get; set; }
        public bool enb_addressline1 { get; set; }
        public bool enb_addressline2 { get; set; }
        public bool enb_area { get; set; }
        public bool enb_city{ get; set; }
        public bool enb_state { get; set; }
        public bool enb_postcode { get; set; }
        public bool enb_dob { get; set; }
        public bool enb_religion { get; set; }
        public bool enb_marital { get; set; }
        public bool enb_sp_firstname { get; set; }
        public bool enb_sp_lastname { get; set; }
        public bool enb_sp_dob { get; set; }
        public bool enb_dom { get; set; }
        public bool enb_source { get; set; }
        public bool enb_often { get; set; }
        public bool enb_gender { get; set; }
        public bool enb_termsandcondition { get; set; }
        public bool enb_title { get; set; }
        public string fld_response { get; set; }
        public string fld_termsandcondition { get; set; }
        public string fld_title { get; set; }
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string branches_json
        {
            get { return Newtonsoft.Json.JsonConvert.SerializeObject(branches); }
            set
            {
                if (string.IsNullOrEmpty(value))
                    branches = new List<Branches>();
                else
                    branches = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Branches>>(value);
            }
        }
        public List<Branches> branches { get; set; }
        public bool fld_responsefailed { get; set; }
        public string fld_failresponse { get; set; }

        public class Branches
        {
            public string code { get; set; }
            public string name { get; set; }
        }
    }
    public class CRMConfigCreteria : FBConfig
    {
        public bool current { get; internal set; }
    }
}

