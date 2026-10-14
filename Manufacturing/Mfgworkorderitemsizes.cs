using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class Mfgworkorderitemsizes : BaseEntity
    {
        public long id { get; set; }
        public long mfgworkorderid { get; set; }
        public long mfgworkorderitemsid { get; set; }
        public bool isactive { get; set; }
        public long createdby { get; set; }
        public long modifiedby { get; set; }
        public int version { get; set; }
        public DateTime createdon { get; set; }
        public DateTime modifiedon { get; set; }
        public long colourid { get; set; }
        public long itemid { get; set; }
        public long sleeveid { get; set; }
        public long fabricid { get; set; }
        public long sizeid { get; set; }
        public int sizeindex { get; set; }
        public int qty { get; set; }
        public int delivered { get; set; }
        public int excess { get; set; }
        public int shortage { get; set; }
        public decimal cutratio { get; set; }
        public long designid { get; set; }
        public string uid { get; set; }
        public decimal cost { get; set; }
        public decimal wsrate { get; set; }
        public decimal dealerprice { get; set; }
        public decimal salerate { get; set; }
        public string name { get;  set; }
        [JsonIgnore]
        public int sno { get; set; }
    }
    public class MfgworkorderitemsizesCreteria : Mfgworkorderitemsizes
    {
    }
}
