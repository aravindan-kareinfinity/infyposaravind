using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class Organisation : BaseEntity
    {
        public long id { get; set; }
        public string code { get; set; }
        public string name { get; set; }
        public string contactperson { get; set; }
        public string contactno { get; set; }
        public string emailid { get; set; }
        public string address { get; set; }
        public long cityid { get; set; }
        public long stateid { get; set; }
        public long countryid { get; set; }
        public string pincode { get; set; }
        public int nooflocation { get; set; }
        public int noofusers { get; set; }
        public DateTime lastpayment { get; set; }
        public decimal subscriptionfees { get; set; }
        public string remarks { get; set; }
        public bool haspaymentdue { get; set; }
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string modules_json { get { return Newtonsoft.Json.JsonConvert.SerializeObject(modules); } set { modules = Newtonsoft.Json.JsonConvert.DeserializeObject<Modules>(value); } }
        public Modules modules { get; set; }

        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string appconfig_json { get { return Newtonsoft.Json.JsonConvert.SerializeObject(appconfig); } set { appconfig = Newtonsoft.Json.JsonConvert.DeserializeObject<List<AppKeyValue>>(value); } }
        public List<AppKeyValue> appconfig { get; set; }
        public bool isactive { get; set; }
        public long createdby { get; set; }
        public long modifiedby { get; set; }
        public int version { get; set; }
        public int dbstatus { get; set; }
        public DateTime createdon { get; set; }
        public DateTime modifiedon { get; set; }
        public string ErrorMessage { get; set; }
        public string connectionstring { get; set; }
        public string dbname { get; set; }
        public string dbserver { get; set; }
        public DateTime accountlockedon { get; set; }
        public bool accountlock { get; set; }
        public int noofcompany { get; set; }

        public string quantousername { get; set; }
        public string quantopassword { get; set; }
        public string adminusername { get; set; }
        public string adminpassword { get; set; }
        public DateTime nextdue { get; set; }
        public class AppKeyValue
        {
            public string key { get; set; }
            public string value { get; set; }
        }
        public class Modules
        {
            public int mode { get; set; }
            public bool saas { get; set; }
            public DateTime amcupto { get; set; }
            public DateTime validupto { get; set; }
            public string company { get; set; }
            public int noofwarehouse { get; set; }
            public int noofshowroom { get; set; }
            public string key { get; set; }
            public string message { get; set; }
            public int noofusers { get; set; }
            public int noofemployees { get; set; }
            public int noofconcurrent { get; set; }
            public int noofcounters { get; set; }
            public bool validate_license { get; set; }
            public bool validate_upto { get; set; }
            public bool validate_amc { get; set; }
            public bool validate_company { get; set; }
            public bool validate_locations { get; set; }
            public bool validate_counters { get; set; }
            public bool validate_concurrent { get; set; }
            public bool validate_permission { get; set; }
            public bool validate_iosuser { get; set; }
            public bool validate_androiduser { get; set; }
            public int nowarehouse { get; set; }
            public bool onlyretail { get; set; }
            public int noofoffline { get; set; }
            public bool offline { get; set; }
            public bool payroll { get; set; }
            public bool manufacture { get; set; }
            public bool camera { get; set; }
            public bool gst { get; set; }
            public bool basicanalytical { get; set; }
            public bool allanalytical { get; set; }
            public bool advanceanalytical { get; set; }
            public bool manualpos { get; set; }
            public bool promotions { get; set; }
            public bool intray { get; set; }
            public bool inbox { get; set; }
            public bool stocksync { get; set; }
            public bool tallyintegration { get; set; }
            public bool finance { get; set; }
            public bool mobile { get; set; }
            public bool purchaseorder { get; set; }
            public bool itemimage { get; set; }
            public bool fmcgpos { get; set; }
            public bool crm { get; set; }
            public bool mobilebu { get; set; }
            public bool tailoring { get; set; }
            public bool jobwork { get; set; }
            public bool imagegallery { get; set; }
            public bool salesorder { get; set; }
            public bool b2bbu { get; set; }
            public bool allfeatures { get; set; }
            public bool developer { get; set; }
            public bool cancreatelocation { get; set; }
            public int iosusercount { get; set; }
            public int androidusercount { get; set; }
            public List<string> restricedcode { get;  set; }
            public List<string> permissioncode { get;  set; }
        }
    }
    public class OrganisationCreteria : Organisation
    {
    }

    public class ScriptRunner : BaseEntity
    {
        public long organizatoinid { get; set; }
        public string script { get; set; }
    }
}
