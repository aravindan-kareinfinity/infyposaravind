using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class Securitykeys : BaseEntity
    {
        public long id { get; set; }
        public string locationkey { get; set; }
        public string privatekey { get; set; }
        public string publickey { get; set; }
        public string ipaddress { get; set; }
        public string computername { get; set; }
        public string systemrole { get; set; }
        public bool isactive { get; set; }
        public bool disable { get; set; }
        public DateTime modifiedon { get; set; }
        public DateTime createdon { get; set; }
    }
    public class SecuritykeysCreteria : Securitykeys
    {
    }
}
