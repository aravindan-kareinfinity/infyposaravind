using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class Ledgerstockopening : BaseEntity
    {
        public DateTime modifiedon { get; set; }
        public bool isactive { get; set; }
        public long companyid { get; set; }
        public long locationid { get; set; }
        public long ledgerid { get; set; }
        public long id { get; set; }
        public DateTime ondate { get; set; }
        public decimal value { get; set; }
        public int version { get; set; }
        public long createdby { get; set; }
        public long modifiedby { get; set; }
        public DateTime createdon { get; set; }
        public string reason { get; set; }
    }
    public class LedgerstockopeningCreteria : Ledgerstockopening
    {
        public DateTime fromdate { get; set; }
        public DateTime todate { get; set; }
        public Ledgerstockopening opening { get; set; }
        public Ledgerstockopening closing { get; set; }
        public List<Ledgerstockopening> items { get; set; }
    }
}
