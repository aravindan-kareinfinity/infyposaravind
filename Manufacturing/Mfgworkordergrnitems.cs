using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class Mfgworkordergrnitems : BaseEntity
    {
        public long id { get; set; }
        public long mfgworkordergrnid { get; set; }
        public bool isactive { get; set; }
        public long createdby { get; set; }
        public long modifiedby { get; set; }
        public int version { get; set; }
        public DateTime createdon { get; set; }
        public DateTime modifiedon { get; set; }
        public decimal received { get; set; }
        public long colourid { get; set; }
        public long mfgworkorderitemsid { get; set; }
        public long sleeveid { get; set; }
        public string size { get; set; }
        public long designid { get; set; }
        public decimal cost { get; set; }
        public decimal taxamount { get; set; }
        public decimal amount { get; set; }
        public decimal taxpercentage { get; set; }
        public string designitemcode { get; set; }
        public long taxid { get; set; }
        public decimal damaged { get; set; }
        public long productid { get; set; }
        public List<mfgworkordergrnitemsizes> sizelist { get; set; }
        public string designname { get;  set; }
        [JsonIgnore]
        public string productname { get; set; }

    }
    public class MfgworkordergrnitemsCreteria : Mfgworkordergrnitems
    {
    }
}
