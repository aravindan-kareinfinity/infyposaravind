using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace InfyPOS.Models
{
    public class Deliveryrack : BaseEntity
    {
        public string card { get; set; }
        public bool disabled { get; set; }
        public long modifiedby { get; set; }
        public long id { get; set; }
        public int pieces { get; set; }
        public long customerid { get; set; }
        public long locationid { get; set; }
        public int version { get; set; }
        public bool isactive { get; set; }
        public DateTime createdon { get; set; }
        public DateTime modifiedon { get; set; }
        public long createdby { get; set; }
        public string code { get; set; }
        public string mobileno { get; set; }
        public string billno { get; set; }
        public string customer { get; set; }
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string addinfo_json
        {
            get { return Newtonsoft.Json.JsonConvert.SerializeObject(addinfo); }
            set
            {
                if (!string.IsNullOrEmpty(value) && value != "null")
                    addinfo = Newtonsoft.Json.JsonConvert.DeserializeObject<AddInfo>(value);
            }
        }
        public AddInfo addinfo { get; set; }
        public class AddInfo
        {
            public AddInfo()
            {
                billnos = new List<string>();
                billids = new List<long>();
            }
            public DateTime starton { get; set; }
            public DateTime updateon { get; set; }
            public decimal noofbills { get; set; }
            public decimal gross { get; set; }
            public decimal amount { get; set; }
            public decimal noofpieces { get; set; }
            public List<string> billnos { get; set; }
            public List<long> billids { get; set; }
        }
    }
    public class DeliveryrackCreteria : Deliveryrack
    {
        public long primaryid { get; set; }
        public List<long> secondary { get; set; }
        public bool hasdelete { get; set; }
        public bool hasclear { get; set; }
        public bool ShowInactive { get; set; }
        public List<long> ids { get; set; }
        public long  billid { get; set; }
    }

    public class DeliveryRackHistory : BaseEntity
    {
        public long deliveryrackid { get; set; }
        public DateTime receivedon { get; set; }
        public DateTime deliveredon { get; set; }
        public string deliveredby { get; set; }
        public string billno { get; set; }
        public string customername { get; set; }
        public string contactno { get; set; }
        public decimal amount { get; set; }
        public decimal qty { get; set; }
    }
    public class DeliveryrackDetail : Deliveryrack
    {
        public string grp { set; get; }
        public string row { set; get; }
        public string col { set; get; }
    }
    public class DeliveryRackWakin : BaseEntity
    {
        public Deliveryrack Rack { get; set; }
        public bool nobills { get;  set; }
        public bool newcard { get; set; }
    }
}
