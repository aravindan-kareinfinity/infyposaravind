using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace InfyPOS.Models
{

    public class BOMBase : BaseEntity
    {
        public long id { get; set; }
        public string name { get; set; }
        public string printingname { get; set; }
        public string title { get; set; }
        public long categoryid { get; set; }
        public int coretypeid { get; set; }
        public string bommastercode { get; set; }
        public List<BOMMaster.colouritem> colourlist { get; set; }
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string colourlist_json { get { return Newtonsoft.Json.JsonConvert.SerializeObject(colourlist); } set { colourlist = Newtonsoft.Json.JsonConvert.DeserializeObject<List<BOMMaster.colouritem>>(value); } }


        public BOMMaster.ColourMode colourmode { get; set; }
        public long taxid { get; set; }
        public Decimal buyingrate { get; set; }
        public string tskuid { get; set; }
        public long tproductid { get; set; }
    }
    public class BOMMaster : BOMBase
    {
        public decimal landingcost { get; set; }
        public decimal romin { get; set; }
        public decimal romax { get; set; }
        public long uomid { get; set; }
        public long colorid { get; set; }
        public long supplierid { get; set; }
        public string width { get; set; }
        public string hsncode { get; set; }
        public string imageid { get; set; }
        public string supplierbrand { get; set; }
        public string designno { get; set; }
        public bool isactive { get; set; }
        public long createdby { get; set; }
        public long modifiedby { get; set; }
        public int version { get; set; }

        public DateTime createdon { get; set; }
        public DateTime modifiedon { get; set; }


        
        public int activestatus { get; set; }
        [JsonIgnore]
        public ActiveStatus Activestatus
        {
            get
            {
                return (ActiveStatus)activestatus;
            }
            set
            {
                activestatus = (int)value;
            }
        }
        public List<supplieritem> supplierlist { get; set; }
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string supplierlist_json { get { return Newtonsoft.Json.JsonConvert.SerializeObject(colourlist); } set { supplierlist = Newtonsoft.Json.JsonConvert.DeserializeObject<List<BOMMaster.supplieritem>>(value); } }
        public bool multisupplier { get; set; }

        public class colouritem
        {
            public long id { get; set; }
            public decimal romin { get; set; }
            public decimal romax { get; set; }
            public string name { get; set; }
            public string imageid { get; set; }
        }
        public class supplieritem
        {
            public long id { get; set; }
            public decimal rate { get; set; }
            public decimal min { get; set; }
            public decimal max { get; set; }
            public string design { get; set; }
            public string brand { get; set; }
        }
        public enum ColourMode
        {
            None,
            Single,
            Multiple
        }
    }
    public class BOMMasterCreteria : BOMMaster
    {
        public bool inactive { get; set; }
        public DateTime fromdate { get; set; }
        public DateTime todate { get; set; }
        public List<long> ids { get; set; }
    }
}
