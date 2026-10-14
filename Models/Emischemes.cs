using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class Emischemes : BaseEntity
    {
        public long id { get; set; }
        public long loanproviderid { get; set; }
        public string name { get; set; }
        public bool isactive { get; set; }
        public long createdby { get; set; }
        public long modifiedby { get; set; }
        public int version { get; set; }
        public DateTime createdon { get; set; }
        public DateTime modifiedon { get; set; }
        public decimal rateofinterest { get; set; }
        public int minimumadvancemonth { get; set; }
        public decimal dbdpercentage { get; set; }
        public decimal dbdvalue { get; set; }
        public decimal emicardcharge { get; set; }
        public decimal documentchargepercentage { get; set; }
        public decimal documentcharge { get; set; }
        public DateTime validfrom { get; set; }
        public DateTime validto { get; set; }
        public decimal minimumvalue { get; set; }
        public decimal maximumvalue { get; set; }
        public int tenure { get;  set; }
    }
    public class EmischemesCreteria : Emischemes
    {
    }
}
