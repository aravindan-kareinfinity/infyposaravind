using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class Customerprice : BaseEntity
    {
        public DateTime createdon { get; set; }
        public bool isactive { get; set; }
        public long id { get; set; }
        public long modifiedby { get; set; }
        public int version { get; set; }
        public long createdby { get; set; }
        public DateTime modifiedon { get; set; }
        public string designid { get; set; }
        public long mfgdesignid { get; set; }
        public List<Customerpricelist> Customerpricelist { get; set; }
    }
    public class CustomerpriceCreteria : Customerprice
    {
        public bool includemfg { get; set; }
        public long[] mfgdesignids { get; set; }
        public long[] designids { get; set; }
        public long[] categoryids { get; set; }
        public long[] customerids { get; set; }
        public bool allcategory { get; set; }
    }
    public class DesignPriceItem : BaseEntity
    {
        public long customerid { get; set; }
        public long categoryid { get; set; }
        public string design { get; set; }
        public long mfgdesignid { get; set; }
        public decimal price { get; set; }
    }
    public class CustomerPriceItem : BaseEntity
    {
        public long id { get; set; }
        public long designid { get; set; }
        public long categoryid { get; set; }
        public long customerid { get; set; }
        public long mfgdesignid { get; set; }
        public decimal price { get; set; }
    }
}
