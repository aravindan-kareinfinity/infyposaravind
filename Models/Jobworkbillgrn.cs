using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class Jobworkbillgrn : BaseEntity
    {
        public bool isinterstatesale { get; set; }
        public string invoiceno { get; set; }
        public string entryno { get; set; }
        public DateTime invoicedate { get; set; }
        public decimal paidamount { get; set; }
        public decimal otheramount { get; set; }
        public decimal grossamount { get; set; }
        public decimal totalamount { get; set; }
        public long id { get; set; }
        public decimal taxamount { get; set; }
        public long supplierid { get; set; }
        public decimal materialcost { get; set; }
        public decimal materialsalevalue { get; set; }
        public DateTime modifiedon { get; set; }
        public DateTime createdon { get; set; }
        public long createdby { get; set; }
        public bool settled { get; set; }
        public long locationid { get; set; }
        public long modifiedby { get; set; }
        public int version { get; set; }
        public long suppliercompanyid { get; set; }
        public long companyid { get; set; }
        public bool isactive { get; set; }
        public int status { get; set; }
        public DateTime entrydate { get; set; }
        public decimal qty { get; set; }
        public string lrinvoiceids { get; set; }
        public List<Jobworkbillgrnitems> Jobworkbillitems { get; set; }
        public string ErrorMessage { get; internal set; }
    }
    public class JobworkbillgrnCreteria : Jobworkbillgrn
    {
        public DateTime fromentrydate { get; set; }
        public DateTime toentrydate { get; set; }

        public DateTime frominvoicedate { get; set; }
        public DateTime toinvoicedate { get; set; }
    }
}
