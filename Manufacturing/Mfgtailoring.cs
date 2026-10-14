using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class Mfgtailoring : BaseEntity
    {
        public long id { get; set; }
        public bool isactive { get; set; }
        public long createdby { get; set; }
        public long modifiedby { get; set; }
        public int version { get; set; }
        public DateTime createdon { get; set; }
        public DateTime modifiedon { get; set; }
        public long employeeid { get; set; }
        public long jobchargeid { get; set; }
        public decimal jobrate { get; set; }
        public decimal jobcharge { get; set; }
        public decimal paidcharge { get; set; }
        public DateTime paidon { get; set; }
        public decimal paidamount { get; set; }
        public string barcode { get; set; }
        public long workorderid { get; set; }
        public long jobworkid { get; set; }
        public long designid { get; set; }
        public long colourid { get; set; }
        public long sleeveid { get; set; }
        public long sizegroupid { get; set; }
        public int iqty { get; set; }
        public int rqty { get; set; }
        public int cqty { get; set; }
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string sizelist_json { get { return Newtonsoft.Json.JsonConvert.SerializeObject(sizelist); } set { sizelist = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Sizelist>>(value); } }
        public List<Sizelist> sizelist { get; set; }
        public int tqty { get; set; }
        public string sizegroupname { get; set; }
        public string sizename { get; set; }
        public string remarks { get; set; }
        public long sizeid { get; set; }
        public DateTime issuedon { get; set; }
        public DateTime receivedon { get; set; }
        public bool showpending { get; set; }
        public class Sizelist
        {
            public string name { get; set; }
            public long id { get; set; }
            public int rqty { get; set; }
            public int iqty { get; set; }
            public int cqty { get; set; }
            public decimal jobcharge { get; set; }

        }
        public long productid { get; set; }
        public long approvedby { get; set; }
        [JsonIgnore]
        public string employeename { get; set; }
        [JsonIgnore]
        public string department { get; set; }
        [JsonIgnore]
        public string jobname { get; set; }
        [JsonIgnore]
        public string employeenamedepartment
        {
            get
            {
                return employeename + " - " + department;
            }
        }
    }


    public class MfgTailoringPrint:BaseEntity
    {
        [JsonProperty("f", NullValueHandling = NullValueHandling.Ignore)]
        public DateTime fromdate { get; set; }
        [JsonProperty("t", NullValueHandling = NullValueHandling.Ignore)]
        public DateTime todate { get; set; }
        [JsonProperty("p", NullValueHandling = NullValueHandling.Ignore)]
        public long productid { get; set; }
        [JsonProperty("d", NullValueHandling = NullValueHandling.Ignore)]
        public long designid { get; set; }
        [JsonProperty("e", NullValueHandling = NullValueHandling.Ignore)]
        public long employeeid { get; set; }
        public Company company { get; set; }
        public string Title
        {
            get
            {
                if(fromdate.Date == todate.Date)
                {
                    return "For " + fromdate.Date.ToString("dd-MM-yyyy");
                }
                else
                {
                    return "From " + fromdate.Date.ToString("dd-MM-yyyy")+" to "+todate.ToString("dd-MM-yyyy");
                }
            }
        }
    }

    public class MfgtailoringCreteria : Mfgtailoring
    {
        public string status { get; set; }
        public DateTime fromdate { get; set; }
        public DateTime todate { get; set; }
        public List<long> ids { get; set; }
        public string employeecode { get; set; }
        public string colourname { get; set; }

    }

    public class MfgPayment : BaseEntity
    {
        public decimal paidamount { get; set; }
        public DateTime paidon { get; set; }
        public DateTime fromdate { get; set; }
        public DateTime todate { get; set; }
        public List<Mfgtailoring> items { get; set; }
    }

    public class MfgReceived : BaseEntity
    {
        public List<Mfgtailoringreceived> recieved { get; set; }
        public Mfgtailoring item { get; set; }
    }

    public class MfgtailoringDetail : Mfgtailoring
    {

    }

}
