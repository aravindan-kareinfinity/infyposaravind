using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class Mfgsampleorder : BaseEntity
    {
        public long id { get; set; }
        public DateTime entrydate { get; set; }
        public long companyid { get; set; }
        public long locationid { get; set; }
        public long prddivisionid { get; set; }
        public string designno { get; set; }
        public long productid { get; set; }
        public long customerid { get; set; }
        public long preparedby { get; set; }
        public long approvedby { get; set; }
        public long statusid { get; set; }
        public bool allowdirectorder { get; set; }
        public long productformid { get; set; }
        public long agentid { get; set; }
        public decimal price { get; set; }
        public bool hasapproved { get; set; }
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string config_json { get { return Newtonsoft.Json.JsonConvert.SerializeObject(config); } set { config = Newtonsoft.Json.JsonConvert.DeserializeObject<SampleConfig>(value); } }
        public SampleConfig config { get; set; }
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string designs_json { get { return Newtonsoft.Json.JsonConvert.SerializeObject(Designs); } set { Designs = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SampleDesign>>(value); } }
        public List<SampleDesign> Designs { get; set; }
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string sizes_json { get { return Newtonsoft.Json.JsonConvert.SerializeObject(Sizes); } set { Sizes = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SampleSize>>(value); } }
        public List<SampleSize> Sizes { get; set; }
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string requestimageids_json { get { return Newtonsoft.Json.JsonConvert.SerializeObject(requestimageids); } set { requestimageids = Newtonsoft.Json.JsonConvert.DeserializeObject<List<string>>(value); } }
        public List<string> requestimageids { get; set; }

        public List<History> history { get; set; }
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string history_json { get { return Newtonsoft.Json.JsonConvert.SerializeObject(history); } set { history = Newtonsoft.Json.JsonConvert.DeserializeObject<List<History>>(value); } }

        public long modifiedby { get; set; }
        public long mfgdesignid { get; set; }
        public long createdby { get; set; }
        public bool isactive { get; set; }
        public int duedays
        {
            get
            {
                return (int)DateTime.Now.Subtract(entrydate.Date).TotalDays;
            }
        }
        public DateTime approvedon { get; set; }
        public DateTime modifiedon { get; set; }
        public DateTime createdon { get; set; }
        public bool hasdeactivated { get; set; }
        public long deactivatedby { get; set; }
        public DateTime deactivateon { get; set; }
        public int version { get; set; }
        public string imageid { get; set; }
        public string deactivatereason { get; set; }
        public string entryno { get; set; }
        public string bookorderno { get; set; }
        public DateTime bookingdate { get; set; }
        public string samplename { get; set; }

        public class History
        {
            public DateTime actionon { get; set; }
            public string reason { get; set; }
            public long approvedby { get; set; }
            public long actionby { get; set; }
            public string user { get; set; }
            public string employee { get; set; }
        }

        public class SampleConfig
        {

            [JsonIgnore]
            public string machinename { get; set; }
            [JsonIgnore]
            public string designername { get; set; }
            [JsonIgnore]
            public string qualityname { get; set; }
            public long machine { get; set; }
            public long designer { get; set; }
            public long quality { get; set; }
            public decimal width { get; set; }
            public decimal length { get; set; }
            public int picks { get; set; }
            public int repeat { get; set; }
            public string flength { get; set; }
            public int noofcolours { get; set; }
            public int lastdesignid { get; set; }
        }
        public class SampleDesign
        {
            public decimal requiredhours { get; set; }
            public decimal requiredbom { get; set; }
            public decimal requiredyarn { get; set; }
            public decimal requiredcost { get; set; }
            public decimal requiredprice { get; set; }
            public string design_code { get; set; }
            public string design_name { get; set; }
            public string imageid { get; set; }
            public int tpicks { get; set; }
            public List<DesignItem> items { get; set; }
            public class DesignItem
            {
                public string mfgitemcode { get; set; }
                public string designid { get; set; }
                [JsonIgnore]
                public string colourname { get; set; }
                public long colourid
                {
                    get
                    {
                        if (design_colour > 0)
                            return design_colour;
                        if (design_shade > 0)
                            return design_shade;
                        if (design_frontcolour > 0)
                            return design_frontcolour;
                        if (design_backcolour > 0)
                            return design_backcolour;
                        return 0;
                    }
                }
                [JsonIgnore]
                public string design_colourname { get; set; }
                [JsonIgnore]
                public string design_frontcolourname { get; set; }
                [JsonIgnore]
                public string design_backcolourname { get; set; }
                [JsonIgnore]
                public string design_bomname { get; set; }
                [JsonIgnore]
                public string design_shadename { get; set; }
                public long design_colour { get; set; }
                public int design_thickness { get; set; }
                public long design_shade { get; set; }
                public int design_picks { get; set; }
                public long design_frontcolour { get; set; }
                public long design_backcolour { get; set; }
                public long design_bom { get; set; }
                public decimal design_yarnqty { get; set; }
                public int sno { get; set; }
            }
            public List<Size> sizes { get; set; }
            public long colourid { get; set; }
            [JsonIgnore]
            public string colourname { get; set; }
            [JsonIgnore]
            public MfgsampleorderPrint parent { get; set; }

            public class Size
            {
                [JsonIgnore]
                public string sizename { get; set; }
                [JsonIgnore]
                public Decimal qty { get; set; }
                public long itemid { get; set; }
                public string uid { get; set; }
                public long id { get; set; }
            }
        }
        public class SampleSize
        {
            [JsonIgnore]
            public string sizename { get; set; }
            [JsonIgnore]
            public Decimal qty { get; set; }
            public string uid { get; set; }
            public long id { get; set; }
        }
    }

    public class MfgsampleorderPrint : Mfgsampleorder
    {
        public Company company { get; set; }
        public CompanyLocation location { get; set; }
        public Customer customer { get; set; }
        public Agent agent { get; set; }
        public string whocreated { get; set; }
        public string whoapproved { get; set; }
        public Products product { get; set; }
        public Referencelist productform { get; set; }
        public Referencelist productdivision { get; set; }
        public Employee employ { get; set; }
        public Employee designer { get; set; }
        public string productformname { get; set; }
        public Referencelist.MfgLabelConfig labelconfig { get; set; }
    }

    public class MfgStatusChange : BaseEntity
    {
        public string status { get; set; }
        public long statusid { get; set; }
        public long employeeid { get; set; }
        public DateTime actionon { get; set; }
        public string reason { get; set; }
        public List<long> ids { get; set; }
    }
    public class MfgSampleDesign : Mfgsampleorder
    {
        public long brandid { get; set; }
        public string hsncode { get; set; }
        public Mfgdesign mfgdesign { get; set; }
        public string message { get; set; }
    }
    public class MfgSampleList : BaseEntity
    {
        public long id { get; set; }
        public string name { get; set; }
        public string samplename { get; set; }
        public string designno { get; set; }
        public string imageid { get; set; }
        public long mfgid { get; set; }
        public long prdid { get; set; }
        public long prddivid { get; set; }
        public List<design> designs { get; set; }
        public MfgSampleList()
        {
            designs = new List<design>();
        }
        public class design
        {
            public design()
            {
                sizes = new List<size>();
            }

            public decimal requiredhours { get; set; }
            public decimal requiredcost { get; set; }
            public decimal requiredprice { get; set; }
            public string code { get; set; }
            public string name { get; set; }
            public string imageid { get; set; }
            public long colourid { get; set; }
            public int id { get; set; }
            public List<size> sizes { get; set; }
        }
        public class size
        {
            public string uid { get; set; }
            public string name { get; set; }
            public long id { get; set; }
            public long itemid { get; set; }
        }
    }
    public class MfgsampleorderCreteria : Mfgsampleorder
    {
        public DateTime addedfrom { get; set; }
        public DateTime addedto { get; set; }
        public DateTime fromdate { get; set; }
        public DateTime todate { get; set; }
        public string sono { get; set; }
    }
}
