using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class Intray : BaseEntity
    {
        public long intrayid { get; set; }
        public long referenceid { get; set; }
        public long companyid { get; set; }
        public int referencetype { get; set; }
        public Services.IntrayManager.EntryType ReferenceType
        {
            get
            {
                return (Services.IntrayManager.EntryType)referencetype;
            }
            set
            {
                referencetype = (int)value;
            }
        }
        public string typename { get; set; }
        public DateTime createdon { get; set; }
        public DateTime modifiedon { get; set; }
        public long createdby { get; set; }
        public string employeename { get; set; }
        public string modifieduser { get; set; }
        public long modifiedby { get; set; }
        public bool isactive { get; set; }
        public int actiontype { get; set; }
        public IntrayActionTypes ActionType {
        get
            {
                return (IntrayActionTypes)actiontype;
            }
            set
            {
                actiontype = (int)actiontype;
            }
        }
        public enum IntrayActionTypes
        {
            None,
            Open,
            Closed,
            Rejected,
            Approved
        }
        
        public int version { get; set; }
        public long actionby { get; set; }
        public long locationid { get; set; }
        public string actionuser { get; set; }
        public DateTime actionon { get; set; }
        public long id { get; set; }
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string detail_json { get { return Newtonsoft.Json.JsonConvert.SerializeObject(detail); } set { detail = Newtonsoft.Json.JsonConvert.DeserializeObject<Detail>(value); } }
        public Detail detail { get; set; }
        public class Detail
        {
            public string description { get; set; }
            public string rejectedReason { get; set; }
            public List<Message> messages { get; set; }
        }
        public string displaydetail
        {
            get
            {
                if (detail != null) return detail.description;
                return "";
            }
        }
        public class Message
        {
            public string subject { get; set; }
            public DateTime messagedon { get; set; }
            public long userid { get; set; }
            public string message { get; set; }
            public List<string> attachments { get; set; }
        }
    }
    public class IntrayCreteria : Intray
    {
        public DateTime fromdate { get; set; }
        public DateTime todate { get; set; }
        public long lastvisitedid { get; set; }
        public int pagelength { get; set; }
    }
}
