using InfyPOS.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class Lrissue : BaseEntity
    {
        public long id { get; set; }
        public bool isactive { get; set; }
        public long createdby { get; set; }
        public long modifiedby { get; set; }
        public int version { get; set; }
        public DateTime createdon { get; set; }
        public DateTime modifiedon { get; set; }
        public long bookingofficeid { get; set; }
        public long companyid { get; set; }
        public long collectingby { get; set; }
        public string issueno { get; set; }
        public DateTime issuedate { get; set; }
        public long receivinglocationid { get; set; }
        public long fromlocationid { get; set; }
        public decimal frieghtcharges { get; set; }
        public decimal noofbundels { get; set; }
        public decimal noofpieces { get; set; }
        public string lrids { get; set; }
        public List<Lrissuelist> lrissuelist { get; set; }
        public long supplierid { get; set; }
    }

    public class LrissueCreteria : Lrissue
    {
        public bool computecharges { get; set; }
    }

    public class LRIssuePrint : Lrissue
    {
        public Company company { get; set; }
        public string lrinfo { get; set; }
        public Suppliercompany supplier { get; set; }
        public Transport transport { get; set; }
        //public Lrinvoice invoice { get; set; }
        //public Lrentry lrentry { get; set; }
    }
}
