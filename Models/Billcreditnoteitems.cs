using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class Billcreditnoteitems : BaseEntity
    {
        public long customerinvoiceid { get; set; }
        public long billcreditnoteid { get; set; }
        public string billno { get; set; }
        public long billid { get; set; }
        public decimal taxpercentage { get; set; }
        public decimal tax { get; set; }
        public long taxid { get; set; }
        public decimal amount { get; set; }
        public decimal totalamount { get; set; }
        public decimal adjustedtax { get; set; }
        public decimal adjustedvalue { get; set; }
        public decimal adjustedtotal { get; set; }
        public long id { get; set; }
        public bool isactive { get; set; }
        public long createdby { get; set; }
        public long modifiedby { get; set; }
        public int version { get; set; }
        public DateTime createdon { get; set; }
        public DateTime modifiedon { get; set; }
        public DateTime billdate { get; set; }
    }
    public class BillcreditnoteitemsCreteria : Billcreditnoteitems
    {
    }
}
