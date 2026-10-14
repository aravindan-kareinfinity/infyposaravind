using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class Mfg_Swatches : BaseEntity
    {
        public string code { get; set; }
        public DateTime receivedon { get; set; }
        public DateTime entrydate { get; set; }
        public long receivedby { get; set; }
        public decimal receivedmtr { get; set; }
        public decimal requiredmtr { get; set; }
        public string pono { get; set; }
        public int receivedpcs { get; set; }
        public string invoiceno { get; set; }
        public DateTime invoicedate { get; set; }
        public long supplierid { get; set; }
        public bool isactive { get; set; }
        public long companyid { get; set; }
        public long designid { get; set; }
        public string design { get; set; }
        public string brand { get; set; }
        public string product { get; set; }
        public int version { get; set; }
        public DateTime modifiedon { get; set; }
        public DateTime createdon { get; set; }
        public long createdby { get; set; }
        public long modifiedby { get; set; }
        public long id { get; set; }
    }
    public class Mfg_SwatchesCreteria : Mfg_Swatches
    {
        public bool showpending { get; set; }
        public long productid { get; set; }
        public DateTime invoicefromdate { get; set; }
        public DateTime invoicetodate { get; set; }
        public DateTime receivedfromdate { get; set; }
        public DateTime receivedtodate { get; set; }
        public DateTime entryfromdate { get; set; }
        public DateTime entrytodate { get; set; }
    }
}
