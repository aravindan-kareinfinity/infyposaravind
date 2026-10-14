using InfyPOS.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class Lrinvoicereturnitems : BaseEntity
    {
        public long productid { get; set; }
        public long id { get; set; }
        public long salablegoodsid { get; set; }
        public decimal sno { get; set; }
        public bool isactive { get; set; }
        public long createdby { get; set; }
        public long modifiedby { get; set; }
        public int version { get; set; }
        public DateTime createdon { get; set; }
        public DateTime modifiedon { get; set; }
        public long itemid { get; set; }
        public decimal qty { get; set; }
        public string barcode { get; set; }
        public long companyid { get; set; }
        public string designid { get; set; }
        public string groupeddesignid { get; set; }
        public string serialno { get; set; }
        public string imageid { get; set; }
        public long stockid { get; set; }
        public long lrinvoiceitemsid { get; set; }
        public long taxid { get; set; }
        public decimal rate { get; set; }
        public decimal tax { get; set; }
        public decimal amount { get; set; }
        public decimal taxableamount
        {
            get
            {
                return amount - discount;
            }
        }
        public decimal netamount
        {
            get { return taxableamount + tax; }
        }
        public decimal taxpercentage { get; set; }
        public decimal discount { get; set; }
        public long lrinvoicereturnid { get; set; }
        public string printingname { get; set; }
        public string invoiceno { get; set; }
        public string lrentryno { get; set; }
        public string uan { get; set; }
        public long supplierid { get; set; }
        public long suppliercompanyid { get; set; }
        public DateTime invoicedate { get; set; }
        public string hsncode { get; set; }
        public bool iscut { get; set; }
        public TaxSplit taxsplit { get; set; }
        public string particulars
        {
            get
            {
                if (string.IsNullOrEmpty(designid)) return printingname;
                return string.Concat(printingname, "/", designid);
            }
        }
        public string particularswithbrandsize
        {
            get
            {
                if (string.IsNullOrEmpty(designid)) return printingname;
                return string.Format("{0}/{1}/{2}/{3}", printingname, designid, brandname, sizename);
            }
        }
        public string srcinvno { get; set; }
        public bool isinterstatesale { get;  set; }
        public string sizename { get;  set; }
        public string stylename { get; set; }
        public string brandname { get; set; }
        public decimal discountpercentage { get; set; }
        [Newtonsoft.Json.JsonIgnore]
        public string invoiceserialno { get; set; }
        [Newtonsoft.Json.JsonIgnore]
        public int sellingmode { get;  set; }
        [Newtonsoft.Json.JsonIgnore]
        public string productname { get; set; }
    }

    public class Lrinvoicereturnitemdetail : Lrinvoicereturnitems
    {
        public string returnno { get; set; }
        public DateTime returndate { get; set; }
    }
    public class LrinvoicereturnitemsCreteria : Lrinvoicereturnitems
    {

    }


}
