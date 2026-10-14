using InfyPOS.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class Lrissuelist : BaseEntity
    {
        public long id { get; set; }
        public bool isactive { get; set; }
        public long createdby { get; set; }
        public long modifiedby { get; set; }
        public int version { get; set; }
        public DateTime createdon { get; set; }
        public DateTime modifiedon { get; set; }
        public decimal weight { get; set; }
        public int noofboxes { get; set; }
        public long lrid { get; set; }
        public string lrno { get; set; }
        public long supplierid { get; set; }
        public long fromlocationid { get; set; }
        public decimal totalamount { get; set; }
        public int noofbundels { get; set; }
        public int noofpieces { get; set; }
        public bool reversed { get; set; }
        public long reversedid { get; set; }
        public long lrissueid { get; set; }
        public string lrentryno { get; set; }
        public DateTime lrdate { get; set; }
        public decimal computedloadingcharge { get; set; }
        public decimal computedfrightcharge { get; set; }
    }
    public class LrissuelistCreteria : Lrissuelist
    {
    }
}
