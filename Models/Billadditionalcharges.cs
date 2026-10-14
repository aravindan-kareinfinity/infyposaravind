using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class Billadditionalcharges : BaseEntity
    {
        public long id { get; set; }
        public bool isactive { get; set; }
        public long createdby { get; set; }
        public long modifiedby { get; set; }
        public int version { get; set; }
        public DateTime createdon { get; set; }
        public DateTime modifiedon { get; set; }
        public decimal Charge { get; set; }
        public decimal Tax { get; set; }
        public long taxid { get; set; }
        public long additionalchargestypeid { get; set; }
        public long billid { get; set; }
        public decimal Amount { get; set; }
        public decimal taxpercentage { get; set; }
        public bool taxonly { get; set; }
        public string chargetype { get; set; }
        public string hsncode { get; set; }
        public string Name { get; set; }
        public decimal acharge
        {
            get
            {
                return Math.Abs(Charge);
            }
        }
    }
    public class BilladditionalchargesCreteria : Billadditionalcharges
    {
    }
}
