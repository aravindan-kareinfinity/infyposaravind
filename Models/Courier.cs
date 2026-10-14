using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{

    public class Courier : BaseEntity
    {
        public long id { get; set; }
        public bool isactive { get; set; }
        public long createdby { get; set; }
        public long modifiedby { get; set; }
        public int version { get; set; }
        public int paymode { get; set; }
        public string Payment
        {
            get
            {
                if(paymode == 0) return "-";
                return paymode == 1 ? "SELF" : "TO PAY";
            }
        }
        public bool ishold { get; set; }
        public DateTime createdon { get; set; }
        public DateTime modifiedon { get; set; }
        public long companyid { get; set; }
        public long receivingcompanyid { get; set; }
        public DateTime receivedon { get; set; }
        public long type { get; set; }
        public long agentid { get; set; }
        public string podnumber { get; set; }
        public decimal podtotalamount
        {
            get
            {
                if (podlist != null) return podlist.Sum(e => e.billamount);
                return 0;
            }
        }
        public string awbnumber { get; set; }
        public long supplierid { get; set; }
        public CourierStatus status { get; set; }
        public long receivedby { get; set; }
        public long deliveredto { get; set; }
        public enum CourierStatus
        {
            Received = 100,
            Dispatched = 101,
            Send = 102,
            Sent = 103,
            Working = 1,
            Completed = 2
        }
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string podlist_json { 
            get { return Newtonsoft.Json.JsonConvert.SerializeObject(podlist); }
            set {
                podlist = (value == null ? new List<BillInfo>() : Newtonsoft.Json.JsonConvert.DeserializeObject<List<BillInfo>>(value)); 
            } 
        }
        public List<BillInfo> podlist { get; set; }
        public string description { get; set; }
        public DateTime senton { get; set; }
        public long sentby { get; set; }
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string attachment_json { 
            get { return Newtonsoft.Json.JsonConvert.SerializeObject(attachment); } 
            set { 
                attachment = (value == null ? new List<slide>() : Newtonsoft.Json.JsonConvert.DeserializeObject<List<slide>>(value)); 
            } 
        }

        public List<slide> attachment { get; set; }
        public class slide
        {
            public string title { get; set; }
            public string image { get; set; }
        }
        public long employeeid { get; set; }
        public long packedby { get; set; }
        [JsonIgnore]
        public Company company { get; set; }
        [JsonIgnore]
        public Suppliercompany supplier { get; set; }
        [JsonIgnore]
        public Transport transport { get; set; }
        [JsonIgnore]
        public string packedbyname { get; set; }
        [JsonIgnore]
        public string bookedby { get; set; }
        [JsonIgnore]
        public string enteredby { get; set; }
        [JsonIgnore]
        public DateTime displaydate { get
            {
                if (mode == 0) return senton;
                else return receivedon;
            } 
        }
        
        public string entryno { get; set; }
        public int lrnos { get; set; }
        public int invnos { get; set; }
        public int copybills { get; set; }
        public bool isbillcopy { get; set; }
        public enum Modes
        {
            Inward = 0,
            Outward = 1
        }
        public int mode { get; set; }
        public string name { get; set; }
        public long customerid { get; set; }
        public DateTime poddate { get; set; }
        public decimal weight { get; set; }
        public decimal rate { get; set; }
        public decimal amount { get; set; }
        public decimal taxamount { get; set; }
        public long taxid { get; set; }
        public decimal taxpercentage { get; set; }
        public int bundles { get; set; }
        public string ewbno { get; set; }
        
        public class BillInfo
        {
            public string billno { get; set; }
            public decimal billamount { get; set; }
            public string billtype { get; set; }
            public long customerid { get; set; }
            public long supplierid { get; set; }
        }
        public decimal cgsttaxpercentage { get { return taxpercentage > 0 ? taxpercentage / 2 : 0; } }
        public decimal cgsttaxamount { get { return taxamount > 0 ? taxamount / 2 : 0; } }
        public decimal sgsttaxpercentage { get { return taxpercentage > 0 ? taxpercentage / 2 : 0; } }
        public decimal sgsttaxamount { get { return taxamount > 0 ? taxamount / 2 : 0; } }

    }
    public class CourierCreteria : Courier
    {
        public List<long> ids { get; set; }
        public DateTime fromdate { get; set; }
        public DateTime todate { get; set; }
    }
}