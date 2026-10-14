using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class Sellablebarcodes : BaseEntity
    {
        public long modifiedby { get; set; }
        public long createdby { get; set; }
        public bool isactive { get; set; }
        public long companyid { get; set; }
        public long itemid { get; set; }
        public long productid { get; set; }
        public string barcode { get; set; }
        public string corebarcode { get; set; }
        public string batchandexpiry { get; set; }
        public long stockid { get; set; }
        public long id { get; set; }
        public DateTime modifiedon { get; set; }
        public DateTime createdon { get; set; }
        public int version { get; set; }
    }
    public class SellablebarcodesCreteria : Sellablebarcodes
    {
        public List<string> ids { get; set; }
        public int limit { get; set; }
    }
}
