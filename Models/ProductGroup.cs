using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class Productgroup : BaseEntity
    {
        public bool isactive { get; set; }
        public long createdby { get; set; }
        public long modifiedby { get; set; }
        public int version { get; set; }
        public long id { get; set; }
        public DateTime createdon { get; set; }
        public DateTime modifiedon { get; set; }
        public string code { get; set; }
        public string name { get; set; }
        public int searchmode { get; set; }
        public bool materialenable { get; set; }
        public string materialname { get; set; }
        public bool patternenable { get; set; }
        public string patternname { get; set; }
        public bool styleenable { get; set; }
        public string stylename { get; set; }
        public bool sizeenable { get; set; }
        public string sizename { get; set; }
        public bool typeenable { get; set; }
        public string typename { get; set; }
        public bool colourenable { get; set; }
        public string colourname { get; set; }
        public bool expiryenable { get; set; }
        public string expiryname { get; set; }
        public bool designenable { get; set; }
        public string designname { get; set; }
    }
    public class ProductgroupCreteria : Productgroup
    {
    }
}
