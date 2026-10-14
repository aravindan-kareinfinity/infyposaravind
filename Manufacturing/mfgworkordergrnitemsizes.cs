using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class mfgworkordergrnitemsizes : BaseEntity
    {
        public long id { get; set; }
        public long mfgworkordergrnid { get; set; }
        public long mfgworkorderitemsgrnid { get; set; }
        public bool isactive { get; set; }
        public long createdby { get; set; }
        public long modifiedby { get; set; }
        public int version { get; set; }
        public DateTime createdon { get; set; }
        public DateTime modifiedon { get; set; }
        public long colourid { get; set; }
        public long itemid { get; set; }
        public string uid { get; set; }
        public long sleeveid { get; set; }
        public long sizeid { get; set; }
        public int sizeindex { get; set; }
        public int qty { get; set; }
        public int sqty { get; set; }
        public int cqty { get; set; }
        public int delivered { get; set; }
        public int excess { get; set; }
        public int shortage { get; set; }
        public long designid { get; set; }
        public decimal cost { get; set; }
        public decimal wsrate { get; set; }
        public decimal dealerprice { get; set; }
        public decimal salerate { get; set; }
        public long mfgworkorderitemsizesid { get; set; }
        public decimal eqty { get; set; }
        public long mfgworkorderid { get; set; }
        public long mfgworkorderitemsid { get; set; }
        public decimal aqty { get; set; }
        public decimal dqty { get; set; }
        [JsonIgnore]
        public Mfgworkordergrnitems parent { get; set; }
        [JsonIgnore]
        public int sno { get; set; }
        [JsonIgnore]
        public string sizename{ get; set; }
        [JsonIgnore]
        public string sleevename { get; set; }
        [JsonIgnore]
        public string colourname { get; set; }
    }
    public class MfgworkordergrnitemsizesCreteria : mfgworkordergrnitemsizes
    {
    }
}
