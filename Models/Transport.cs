using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class Transport : BaseEntity
    {
        public long id { get; set; }
        public string name { get; set; }
        public string contactperson { get; set; }
        public string contactno { get; set; }
        public string address { get; set; }
        public long cityid { get; set; }
        public long stateid { get; set; }
        public long basetransportid { get; set; }
        public string vehicles { get; set; }
        public long countryid { get; set; }
        public long taxid { get; set; }
        public bool rcm { get; set; }
        public string pincode { get; set; }
        public bool isactive { get; set; }
        public long createdby { get; set; }
        public long modifiedby { get; set; }
        public int version { get; set; }
        public DateTime createdon { get; set; }
        public DateTime modifiedon { get; set; }
        public string emailid { get; set; }
        public string pan { get; set; }
        public string gst { get; set; }
        public bool dailycollection { get; set; }
        public string bankaccountname { get; set; }
        public string bankaccountno { get; set; }
        public string bankifci { get; set; }
        public string bankname { get; set; }
        public string bankbranch { get; set; }
        public long bankid { get; set; }

        public decimal loadingchargeperbox { get; set; }
        public decimal loadingchargeperbundle { get; set; }
        public bool loadingchargedaycollection { get; set; }
        public List<loadingcharge> loadingcharges { get; set; }
        public int transportertype { get; set; }
        public enum TransportType
        {
            Both = 0,
            Lorry = 1,
            Courier = 2
        }
        public class loadingcharge
        {
            public long cityid { get; set; }
            public decimal charge { get; set; }
            public decimal boxcharge { get; set; }
            public decimal bundlecharge { get; set; }
        }

        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string loadingcharges_json
        {
            get { return loadingcharges == null ? "null" : Newtonsoft.Json.JsonConvert.SerializeObject(loadingcharges); }
            set { if(value != null) loadingcharges = Newtonsoft.Json.JsonConvert.DeserializeObject<List<loadingcharge>>(value); }
        }

        public int paymentmode { get;  set; }
    }
    public class TransportCreteria : Transport
    {
    }
}
