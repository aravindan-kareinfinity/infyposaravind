using InfyPOS.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class BOMInvoicereturnitems : BaseEntity
    {
        public long bomcategoryid { get; set; }
        public long id { get; set; }
        public decimal sno { get; set; }
        public bool isactive { get; set; }
        public long createdby { get; set; }
        public long modifiedby { get; set; }
        public int version { get; set; }
        public DateTime createdon { get; set; }
        public DateTime modifiedon { get; set; }
        public decimal qty { get; set; }
        public string bominwardcode { get; set; }
        public long companyid { get; set; }
        public long bomid { get; set; }
        public string imageid { get; set; }
        public long bominvoiceitemsid { get; set; }
        public long taxid { get; set; }
        public decimal rate { get; set; }
        public decimal tax { get; set; }
        public decimal amount { get; set; }
        public long colourid { get; set; }
        public string width { get; set; }
        public decimal taxableamount
        {
            get
            {
                return amount - discount;
            }
        }
        public decimal taxpercentage { get; set; }
        public decimal discount { get; set; }
        public long bominvoicereturnid { get; set; }
        public string printingname { get; set; }
        public string invoiceno { get; set; }
        public long supplierid { get; set; }
        public long suppliercompanyid { get; set; }
        public DateTime invoicedate { get; set; }
        public string hsncode { get; set; }
        public TaxSplit taxsplit { get; set; }
        public string particulars
        {
            get
            {
                return printingname;
            }
        }

        public bool isinterstatesale { get;  set; }
        public string sizename { get;  set; }
        public decimal discountpercentage { get; set; }
        [Newtonsoft.Json.JsonIgnore]
        public string invoiceserialno { get; set; }

        [Newtonsoft.Json.JsonIgnore]
        public string bomname { get; set; }
        [Newtonsoft.Json.JsonIgnore]
        public string colourname { get; set; }

    }

    public class BOMInvoicereturnitemdetail : BOMInvoicereturnitems
    {
        public string returnno { get; set; }
        public DateTime returndate { get; set; }
    }
    public class BOMInvoicereturnitemsCreteria : BOMInvoicereturnitems
    {

    }


}
