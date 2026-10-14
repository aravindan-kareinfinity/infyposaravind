using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class Rechargebalance : BaseEntity
    {
        public long schemeid { get; set; }
        public bool isactive { get; set; }
        public long createdby { get; set; }
        public long modifiedby { get; set; }
        public int version { get; set; }
        public decimal balance { get; set; }
        public DateTime createdon { get; set; }
        public DateTime modifiedon { get; set; }
        public long id { get; set; }
        public decimal topup { get; set; }
        public decimal discount { get; set; }
        public decimal transferin { get; set; }
        public decimal transferout { get; set; }
        public decimal recharged { get; set; }
        public decimal cashback { get; set; }
        public long locationid { get; set; }
        public string location { get; set; }
        public string scheme { get; set; }
    }
    public class RechargebalanceCreteria : Rechargebalance
    {
    }
}
