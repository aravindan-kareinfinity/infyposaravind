using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class Smsmessage : BaseEntity
    {
        public long id { get; set; }
        public string message { get; set; }
        public long companyid { get; set; }
        public long locationid { get; set; }
        public bool isactive { get; set; }
        public DateTime modifiedon { get; set; }
        public DateTime createdon { get; set; }
        public long createdby { get; set; }
        public long modifiedby { get; set; }
        public int version { get; set; }
        public int smscount { get; set; }
        public int failedcount { get; set; }
        public bool isdirect { get; set; }

        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string parameter_json
        {
            get { return Newtonsoft.Json.JsonConvert.SerializeObject(parameter); }
            set
            {
                if (value != null)
                    parameter = Newtonsoft.Json.JsonConvert.DeserializeObject<List<KeyVal>>(value);
            }
        }
        public List<KeyVal> parameter { get; set; }

        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string result_json
        {
            get { return Newtonsoft.Json.JsonConvert.SerializeObject(result); }
            set
            {
                if (value != null)
                    result = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Publishto>>(value);
            }
        }
        public List<Publishto> result { get; set; }

        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string publishto_json
        {
            get { return Newtonsoft.Json.JsonConvert.SerializeObject(publishto); }
            set
            {
                if (value != null)
                    publishto = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Publishto>>(value);
            }
        }
        public List<Publishto> publishto { get; set; }
        public Smsconfig smsconfig { get; set; }

    }
    public class KeyVal
    {
        public string key { get; set; }
        public string value { get; set; }
    }
    public class Publishto
    {
        public long id { get; set; }
        public string name { get; set; }
        public string mobileno { get; set; }
        public string error { get; set; }
    }

    public class SmsmessageCreteria : Smsmessage
    {
        public DateTime fromdate { get; set; }
        public DateTime todate { get; set; }
    }

    public class Smsconfig
    {
        public string senderid { get; set; }
        public string url { get; set; }
        public string action { get; set; }

        public string confirmation { get; set; }
        public Smsconfig()
        {

        }

        public bool IsValid
        {
            get
            {
                return !string.IsNullOrEmpty(url) && url.Length > 5;
            }
        }
        public static Smsconfig Default
        {
            get
            {
                return new Smsconfig()
                {
                    url = @"http://sms.finddeal.in/sendsms/?token=005f908b6519b0e746590ebe5271a298&credit=2&sender=SCHOOL&message={Message}&number={Mobile}",
                    senderid = "SCHOOL",
                    action = "get",
                    confirmation = "sent"
                };
            }
        }
    }

    public class SMSQueue
    {
        public string message { get; set; }
        public long id { get; set; }
        public bool isdirect { get; set; }
        public long internalid { get; set; }
        public string senderid { get; set; }
        public string smsserverurl { get; set; }
        public string sendmethod { get; set; }
        public string smsconfirmation { get; set; }
        public List<Item> items { get; set; }

        public List<KeyVal> arguments { get; set; }
        public class Item
        {
            public string mobile { get; set; }
            public List<KeyVal> arguments { get; set; }
            public bool hassent { get; set; }
            public string error { get; set; }
        }

    }
}
