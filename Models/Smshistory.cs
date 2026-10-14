using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class Smshistory : BaseEntity
    {
        public bool HasDuplicate { get; set; }
        public long scheduleid { get; set; }
        public string mobileno { get; set; }
        public string message { get; set; }
        public string response { get; set; }
        public DateTime actionon { get; set; }
        public string config { get; set; }
        public string hash { get; set; }
    }
    public class SmshistoryCreteria : Smshistory
    {
    }
}
