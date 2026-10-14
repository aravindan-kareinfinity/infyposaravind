using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class Salablegoodsprice : BaseEntity
    {
        public long locationid { get; set; }
        public int version { get; set; }
        public DateTime createdon { get; set; }
        public DateTime modifiedon { get; set; }
        public long createdby { get; set; }
        public long modifiedby { get; set; }
        public bool isactive { get; set; }
        public long approvedby { get; set; }
        public long removedby { get; set; }
        public DateTime addon { get; set; }
        public DateTime removedon { get; set; }
        public long invoiceitemid { get; set; }
        public decimal price { get; set; }
        public long id { get; set; }
        public decimal mrp { get; set; }
        public string batchid { get; set; }
        public string barcode { get; set; }

        [JsonIgnore]
        public bool sync { get; set; }
    }
    public class SalablegoodspriceCreteria : Salablegoodsprice
    {
        public bool includedeleted { get; set; }
        public List<long> locationids { get; set; }
    }
}
