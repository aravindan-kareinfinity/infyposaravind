using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class UserBasic : BaseEntity
    {
        public string title { get; set; }
        public long id { get; set; }
        public string username { get; set; }
    }
    public class UserCustomer : UserBasic
    {
        public List<long> accessiblecustomer { get; set; }
    }
    public class Users : UserBasic
    {
        public string companyname { get; set; }
        public long companyid { get; set; }
        
        public bool isactive { get; set; }
        public long createdby { get; set; }
        public long modifiedby { get; set; }
        public int version { get; set; }
        public DateTime createdon { get; set; }
        public DateTime modifiedon { get; set; }
        
        [JsonIgnore]
        public string tr { get; set; }
        
        public string systemrole { get; set; }
        public List<Permissions> permissions { get; set; }

        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string permissions_json
        {
            get { return Newtonsoft.Json.JsonConvert.SerializeObject(permissions); }
            set
            {
                if (value != null)
                {
                    try
                    {
                        permissions = Newtonsoft.Json.JsonConvert.DeserializeObject< List<Permissions>>(value);
                    }
                    catch (Exception exp)
                    {

                    }
                }
            }
        }

        public UserSecurity security { get; set; }
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string security_json
        {
            get { return Newtonsoft.Json.JsonConvert.SerializeObject(security); }
            set
            {
                if (value != null)
                {
                    try
                    {
                        security = Newtonsoft.Json.JsonConvert.DeserializeObject<UserSecurity>(value);
                    }
                    catch (Exception exp)
                    {

                    }
                }
            }
        }

        public class LoggedInUser 
        {
            public Users user { get; set; }
            public string platform { get; set; }
            public string deviceid { get; set; }
            public string utcoffset { get; set; }
        }
        public class UserSecurity
        {
            public bool iprestriction { get; set; }
            public bool locationrestriction { get; set; }
            public bool biometric { get; set; }
            public bool timerestriction { get; set; }
            public string from { get; set; }
            public string to { get; set; }
            public bool globaluser { get; set; }
            public long locationid { get; set; }
            public long counterid { get; set; }
        }
        public List<ResourcePermission> accessablesections { get; set; }
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string accessablesections_json
        {
            get { return Newtonsoft.Json.JsonConvert.SerializeObject(accessablesections); }
            set
            {
                if (value != null)
                {
                    try
                    {
                        accessablesections = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ResourcePermission>>(value);
                    }
                    catch (Exception exp)
                    {

                    }
                }
            }
        }

        public string passcode { get; set; }
        public DateTime passcodeon { get; set; }




        public List<ResourcePermission> accessiblelocation { get; set; }
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string accessiblelocation_json
        {
            get { return Newtonsoft.Json.JsonConvert.SerializeObject(accessiblelocation); }
            set
            {
                if (value != null)
                {
                    try
                    {
                        accessiblelocation = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ResourcePermission>>(value);
                    }
                    catch (Exception exp)
                    {

                    }
                }
            }
        }

        public List<ResourcePermission> accessiblecompany { get; set; }
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string accessiblecompany_json
        {
            get { return Newtonsoft.Json.JsonConvert.SerializeObject(accessiblecompany); }
            set
            {
                if (value != null)
                {
                    try
                    {
                        accessiblecompany = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ResourcePermission>>(value);
                    }
                    catch (Exception exp)
                    {

                    }
                }
            }
        }

        public class ResourcePermission
        {
            public long id { get; set; }
            public bool view { get; set; }

        }
        public string password { get; set; }
        public string title { get; set; }
        public string designation { get; set; }
        public long actualuserid { get; set; }
        public string externalprovider { get; set; }
        public int role { get; set; }
        public string landingscreen { get; set; }
        public long employeeid { get; set; }
        public bool changepassword { get; set; }
        public DateTime lastloginon { get; set; }


        public Roles roleName {
            get
            {
                return (Roles)role;
            }
        }
        public enum Roles
        {
            Administrator,
            Salesman
        }
        public string accesskey { get; set; }
        public string organizationcode { get; set; }
        public long organizationid { get; set; }
        public long dsgid { get; set; }
        public decimal alloweddiscount { get; set; }
        public decimal allowedcredit { get; set; }
    }

    public class UsersToken : BaseEntity
    {
        public string refreshToken { get; set; }
        public string accessToken { get; set; }
    }
    public class UsersCreteria : Users
    {
        public List<long> ids { get; set; }
        public bool inactive { get; set; }
        public class LocationInfo
        {
            public string sourcekey { get; set; }
            public string licensekey { get; set; }
            public string computername { get; set; }
            public string ipaddress { get; set; }
            public string locationkey { get; set; }
            public string publicip { get; set; }
        }
        public LocationInfo locationinfo { get; set; }
        public string ipaddress { get; set; }
        public bool ValidateLogin { get; set; }
        public bool ValidateExternalLogin { get; set; }
        public bool ValidateSecurity { get; set; }
        public bool includesetting { get; set; }
        public string newpassword { get; set; }
        public bool globalpermission { get; set; }
        public Configuration.ApplicationSettings Config { get; set; }
    }

    public class UserPASSCODE : BaseEntity
    {
        public string code { get; set; }
        public string permission { get; set; }
        public long locationid { get; set; }
        public Bill source { get; set; }
    }
}
