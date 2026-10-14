using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class Jobworkbillitems : BaseEntity
    {
        public long jobworkbillid { get; set; }
        public decimal qty { get; set; }
        // Job work expected receive qty
        public decimal oqty { get; set; }
        // Job work issue qty
        public decimal iqty { get; set; }
        public decimal dqty { get; set; }
        public decimal rqty { get; set; }
        public DateTime modifiedon { get; set; }
        public DateTime createdon { get; set; }
        public int version { get; set; }
        public long modifiedby { get; set; }
        public long id { get; set; }
        public int sno { get; set; }
        public string barcode { get; set; }
        public string imageid { get; set; }
        public long salablegoodsid { get; set; }
        public long stockid { get; set; }
        public long productid { get; set; }
        public long outproductid { get; set; }
        public long invoiceitemid { get; set; }
        public bool isactive { get; set; }
        public long createdby { get; set; }
        public string hsncode { get; set; }
        public decimal materialsalevalue { get; set; }
        public decimal materialcost { get; set; }
        public decimal chargerate { get; set; }
        public decimal chargetax { get; set; }
        public decimal chargetaxpercentage { get; set; }
        public long chargetaxid { get; set; }
        public long chargeid { get; set; }
        public decimal chargegross { get; set; }
        public decimal chargetotal { get; set; }
        public decimal totalmaterialsalevalue { get { return iqty * materialsalevalue; } }
        public decimal totalmaterialcost { get { return iqty * materialcost; } }

    }

    public class JobworkbillitemsDetail : Jobworkbillitems
    {
        public string productname { get; set; }
        public string brandname { get; set; }
        public string typename { get; set; }
        public string stylename { get; set; }
        public string outproductname { get; set; }
        public string perticulars { get
            {
                return productname + "/" + brandname + "/" + typename + "/" + stylename;
            } }
    }

    public class JobworkbillitemSpecification
    {
        public int sno { get; set; }
        public string outproductname { get; set; }
        public decimal oqty { get; set; }
        public decimal perqtyconsumption { get; set; }
        public decimal totalqtyconsumption { get; set; }
        public decimal chargerate { get; set; }
        public decimal chargegross { get; set; }
    }
    public class JobworkbillitemsCreteria : Jobworkbillitems
    {
        public string invoiceno { get; set; }
        public long locationid { get; set; }
        public long companyid { get; set; }
        public long supplierid { get; set; }
        public long suppliercompanyid { get; set; }
        public long jobworkbillgrnid { get; set; }
        public List<long> jobworkids { get; set; }
        public bool includeitemdetails { get; set; }
    }

    public class JobworkbillitemsPending : Jobworkbillitems
    {
        public DateTime entrydate { get; set; }
        public string invoiceno { get; set; }
        public long supplierid { get; set; }
        public long suppliercompanyid { get; set; }
        public long locationid { get; set; }
        public long companyid { get; set; }
        public long itemid { get; set; }
        public string designid { get; set; }
        public DisplayItem iteminfo { get; set; }
    }

    public class JobworkbillitemsSummary : BaseEntity
    {
        public long id { get; set; }
        public decimal oqty { get; set; }
        public DateTime entrydate { get; set; }
        public DateTime invoicedate { get; set; }
        public DateTime expecteddate { get; set; }
        public string invoiceno { get; set; }
        public decimal pqty { get; set; }
        public long outproductid { get; set; }
        public long suppliercompanyid { get; set; }
        public string jobworker { get; set; }
        public string sourcesupplier { get; set; }
        public string inproduct { get; set; }
        public string outproduct { get; set; }
        public long companyid { get;  set; }
    }
    public class Jobworkitems : JobworkbillitemsSummary
    {
        public decimal rqty { get; set; }
        public string barcode { get; set; }
        public decimal materialsalevalue { get; set; }
        public decimal materialcost { get; set; }
        public decimal chargerate { get; set; }
        public decimal chargegross { get; set; }
        public decimal chargetotal { get; set; }
        public string grndate { get; set; }
        public string grnno { get; set; }


    }
}
