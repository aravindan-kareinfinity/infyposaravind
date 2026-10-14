using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class Customerpricelist : BaseEntity
    {
        public DateTime createdon { get; set; }
        public bool isactive { get; set; }
        public long id { get; set; }
        public long modifiedby { get; set; }
        public int version { get; set; }
        public long createdby { get; set; }
        public DateTime modifiedon { get; set; }
        public long customercategoryid { get; set; }
        public long customerid { get; set; }
        public long customerpriceid { get; set; }
        public decimal price { get; set; }
    }
    public class CustomerpricelistCreteria : Customerpricelist
    {
    }
}
