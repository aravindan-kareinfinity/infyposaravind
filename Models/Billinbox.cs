using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class BillDeliveryDetails : BaseEntity
    {
        public decimal loadingcharges { get; set; }
        public decimal frightcharges { get; set; }
        public long noofbundles { get; set; }
        public long noofpieces { get; set; }
        public long transportid { get; set; }
        public string transportname { get; set; }
        public string transportgst { get; set; }
        public string lrno { get; set; }
        public DateTime lrdate { get; set; }

        public bool haseinvoice { get; set; }
        public bool haseway { get; set; }
        [Newtonsoft.Json.JsonIgnore]

        public InfyGST.GSTPortal.EInvoiceResponse GSTEinvoice { get; set; }
        [Newtonsoft.Json.JsonIgnore]

        public InfyGST.GSTPortal.EwayFromGRNResponse GSTEWay { get; set; }

        
        [Newtonsoft.Json.JsonIgnore]
        public Billinbox billinbox { get; set; }

        [JsonIgnore]
        public string HSNList { get; set; }
    }
    public class BillinboxStatus :BaseEntity
    {
        public long id { get; set; }
        public int status { get; set; }
        public DateTime actionon { get; set; }
        public long actionby { get; set; }
        public long billid { get; set; }
    }
    public class Billinbox : BaseEntity
    {
        [JsonIgnore]
        public int sno{ get; set; }
        public long id { get; set; }
        public bool isactive { get; set; }
        public long createdby { get; set; }
        public long modifiedby { get; set; }
        public int version { get; set; }
        public DateTime createdon { get; set; }
        public DateTime modifiedon { get; set; }
        public decimal loadingcharges { get; set; }
        public decimal frightcharges { get; set; }
        public long noofbundles { get; set; }
        public long noofpieces { get; set; }
        public long transportid { get; set; }
        public string lrno { get; set; }
        public DateTime lrdate { get; set; }
        public long tolocationid { get; set; }
        public long companyid { get; set; }
        public string tocityname { get; set; }
        public long billid { get; set; }
        public long billlocationid { get; set; }
        public string billno { get; set; }
        public string einvoice { get; set; }
        public string ewb { get; set; }
        public string ackno { get; set; }
        public bool hascancel { get; set; }
        public string billimg { get; set; }
        public string orderby { get; set; }
        public string remark { get; set; }
        public decimal billvalue { get; set; }
        public decimal billqty { get; set; }
        public int modeofdelivery { get; set; }
        public decimal additionalcharges { get; set; }
        public long packedby { get; set; }
        public DateTime packedon { get; set; }
        public long handedtoid { get; set; }
        public DateTime handedoveron { get; set; }
        public long stitchedby { get; set; }
        public DateTime stitchededon { get; set; }

        public int status { get; set; }
        public DateTime billdate { get; set; }
        public long customerid { get; set; }
        public string customername { get; set; }
        public string transportname { get; set; }
        public string tolocation { get; set; }
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string address_json
        {
            get
            {
                return Newtonsoft.Json.JsonConvert.SerializeObject(address);
            }
            set
            {
                if (value != null)
                    address = Newtonsoft.Json.JsonConvert.DeserializeObject<Address>(value);
            }
        }
        public Address address { get; set; }
        public class Address
        {
            public string detail { get; set; }
            public string area { get; set; }
        }

        public string vehicleno { get; set; }
        public string einvoicejosn { get; set; }
        public string gstexception { get; set; }
        public string ewayjson { get; set; }
        public bool haseinvoice { get; set; }
        public bool haseway { get; set; }
        public string billremarks { get;  set; }
        public string billsono { get;  set; }
        public long billshipto { get;  set; }
    }
    public class BillNoChange : BaseEntity
    {
        public Billinbox bill { get; set; }
        public string newbillno { get; set; }
    }
    public class BillinboxCreteria : Billinbox
    {
        public long distance { get; set; }
        public string mode { get; set; }
        public DateTime addedfrom { get; set; }
        public DateTime addedto { get; set; }
        public int typeid { get; set; }
        public bool hsn4digit { get; set; }
        public bool hsn8digit { get; set; }
        public bool hsn6digit { get; set; }
        public long agentid { get; set; }
    }

    public class BillinboxHandovers : BaseEntity
    {
        public string transportname { get; set; }
        public string handoverperson { get; set; }
        public long handoverto { get; set; }
        public DateTime handoveron { get; set; }
        public List<Billinbox> items { get; set; }
        public long transportid { get;  set; }
        public Company company { get; internal set; }
        public CompanyLocation location { get; internal set; }
    }
}
