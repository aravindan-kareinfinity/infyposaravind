using InfyPOS.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class Lrentryfreightcharges : BaseEntity
    {
        public long lrid { get; set; }
        public long chargetypeid { get; set; }
        public decimal charge { get; set; }
        public long id { get; set; }
        public long createdby { get; set; }
        public long modifiedby { get; set; }
        public DateTime createdon { get; set; }
        public DateTime modifiedon { get; set; }
        public int version { get; set; }
        public bool isactive { get; set; }
    }
    public class LrentryfreightchargesCreteria : Lrentryfreightcharges
    {
    }
}
