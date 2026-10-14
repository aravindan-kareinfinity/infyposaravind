using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class Mfgworkorder : BaseEntity
    {
        public long id { get; set; }
        public long companyid { get; set; }
        public long customerid { get; set; }
        public long mfgsampleorderid { get; set; }
        public long proddivid { get; set; }
        public string mode { get; set; }
        public string workorderno { get; set; }
        public long locationid { get; set; }
        public long designid { get; set; }
        public DateTime wodate { get; set; }
        public DateTime expectdate { get; set; }
        public DateTime deliverydate { get; set; }
        public long issuedby { get; set; }
        public bool isactive { get; set; }
        public long createdby { get; set; }
        public long modifiedby { get; set; }
        public int version { get; set; }
        public DateTime createdon { get; set; }
        public DateTime modifiedon { get; set; }
        public decimal tax { get; set; }
        public decimal taxable { get; set; }
        public decimal total { get; set; }
        public int statusid { get; set; }
        public long supplierid { get; set; }
        public string jobcardno { get; set; }
        public string lotno { get; set; }
        public long suppliercompanyid { get; set; }
        public decimal variance { get; set; }
        public decimal totalqty { get; set; }
        public decimal fabricqty { get; set; }
        public long receivedby { get; set; }
        public long woflowid { get; set; }
        public bool onlybom { get; set; }
        public string product { get; set; }
        public string design { get; set; }
        [JsonIgnore]
        public string suppliername { get; set; }
        public List<Mfgworkorderitems> Mfgworkorderitems { get; set; }

        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string sizelist_json
        {
            get { return Newtonsoft.Json.JsonConvert.SerializeObject(sizelist); }
            set
            {
                if (string.IsNullOrEmpty(value))
                    sizelist = new List<SizeInfo>();
                else
                    sizelist = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SizeInfo>>(value);
            }
        }
        public List<SizeInfo> sizelist { get; set; }
        public class SizeInfo
        {
            public long sizeid { get; set; }
            public int sizeindex { get; set; }
            public string name { get; set; }
        }
        public string brandname { get; set; }
        public string remarks { get; set; }
        public string sleevename { get; set; }
        public string colourname { get; set; }



    }

    public class MfgworkorderSummary
    {
        public decimal totalqty { get; set; }
        public long locationid { get; set; }
        public decimal fabricqty { get; set; }
        public decimal scutratio { get; set; }
        public decimal acutratio { get; set; }
        public decimal grnqty { get; set; }
        public DateTime expectdate { get; set; }
        public DateTime wodate { get; set; }
        public string workorderno { get; set; }
        public string jobcardno { get; set; }
        public string lotno { get; set; }
        public long mfgworkorderid { get; set; }
        public long supplierid { get; set; }
        public string designitemname { get; set; }
        public string colourname { get; set; }
        public string product { get; set; }
        public string eproduct { get; set; }
        public decimal cfabricqty { get;  set; }

        public List<Stage> stages = new List<Stage>();
        public List<Size> sizes = new List<Size>();
        public class Stage
        {
            public int corestageid { get; set; }
            public decimal issuedqty { get; set; }
            public decimal receivedqty { get; set; }
            public string stage { get; set; }
        }

        public class Size
        {
            public string name { get; set; }
            public decimal qty { get; set; }
            public decimal gqty { get; set; }
        }
    }
    public class MfgworkorderCreteria : Mfgworkorder
    {
        public DateTime fromdate { get; set; }
        public DateTime todate { get; set; }
        public bool showonlypending { get; set; }
        
        public DateTime duefromdate { get; set; }
        public DateTime duetodate { get; set; }
        public int corestageid { get; set; }
        public long brandid { get; set; }
        public long productid { get; set; }
        public long productgroupid { get; set; }
    }

    public class MfgworkorderPrint : Mfgworkorder
    {
        public double duedays
        {
            get
            {
                if (expectdate > DateTime.MinValue && expectdate > wodate)
                    return (expectdate.Date - wodate.Date).TotalDays;
                return 0;
            }
        }
        public ReportLibrary.DOSReporter.Table sizetable { get; set; }

        public Company company { get; set; }
        public Supplier supplier { get; set; }
        public Suppliercompany suppliercompany { get; set; }
        public Transport transport { get; set; }
        public string whocreated { get; set; }
        public string whoapproved { get; set; }
        public string brand { get; set; }
        public string fabric { get; set; }
        public CompanyLocation location { get; set; }
        public List<Mfgworkorderitemsizes> Mfgworkorderitemsizes { get;  set; }
        [JsonIgnore]
        public Mfgsampleorder mfgsampleorder { get; set; }
        [JsonIgnore]
        public List<Mfgsampleorder.SampleSize> mfgsamplesizes { get;  set; }
        [JsonIgnore]
        public Mfgsampleorder.SampleDesign mfgsampledesign { get;  set; }
        public string division { get; set; }
        public string productform { get; set; }
        public string agent { get; set; }
        public string quality { get; set; }
        public Customer customer { get;  set; }
        public Referencelist machine { get; set; }
        public decimal workinghours { get;  set; }
        public string workinghour { get; set; }
        public bool hasworkinghour
        {
            get
            {
                return !string.IsNullOrEmpty(workinghour);
            }
        }
    }
}


