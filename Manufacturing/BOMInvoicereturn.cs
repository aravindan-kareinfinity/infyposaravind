using InfyPOS.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class BOMInvoicereturn : BaseEntity
    {
        public long companyid { get; set; }
        public long locationid { get; set; }
        public DateTime lrdate { get; set; }
        public long id { get; set; }
        public bool isactive { get; set; }
        public long createdby { get; set; }
        public string createdusername { get; set; }
        public string modifiedusername { get; set; }
        public long modifiedby { get; set; }
        public int version { get; set; }
        public DateTime createdon { get; set; }
        public DateTime modifiedon { get; set; }
        public string returnno { get; set; }
        public DateTime returndate { get; set; }
        public long lrinvoiceid { get; set; }
        public long lrid { get; set; }
        public decimal discount { get; set; }
        public decimal tax { get; set; }
        public decimal amount { get; set; }
        public decimal total { get; set; }
        public decimal qty { get; set; }
        public List<BOMInvoicereturnitems> BOMinvoicereturnitems { get; set; }
        public long agentid { get; set; }
        public long supplierid { get; set; }
        public long shiptosupplierid { get; set; }
        public long transportid { get; set; }
        public decimal bundles { get; set; }
        public decimal freightamt { get; set; }
        public decimal freighttaxamt { get; set; }
        public string freighttaxname { get; set; }
        public long freighttaxid { get; set; }
        public decimal packingcharge { get; set; }
        public decimal weight { get; set; }
        public string remarks { get; set; }
        public decimal rounding { get; set; }
        public string inwords { get; set; }
        public long suppliercompanyid { get; set; }
        public long cityid { get; set; }
        public bool isinterstatesale { get; set; }
        public int freight { get; set; }
        public TaxSplit taxsplit { get; set; }
        public string lrno { get; set; }
        public string errormessage { get; set; }
        public int returntype { get; set; }

        public string billimg { get; set; }
        public string eway { get; set; }
        public string einvoice { get; set; }
        public long tolocationid { get; set; }
        public long packedby { get; set; }
        public ReturnTypes ReturnType
        {
            get
            {
                return (ReturnTypes)returntype;
            }
            set
            {
                returntype = (int)value;
            }
        }
        public enum ReturnTypes
        {
            DebitNote,
            DOADebitNote,
            DOACreditNote,
            PriceDropCreditNote,
            RecycleCreditNote,
            DiscountDebityNote,
            SupplierCreditNote,
            SupplierAdvanceNote
        }
    }
    public class BOMInvoicereturnCreteria : BOMInvoicereturn
    {
        public DateTime fromreturndate { get; set; }
        public DateTime toreturndate { get; set; }
        public string barcode { get; set; }
        public bool isaudit { get; set; }
        public bool isprint { get; set; }
        public string lrentryno { get; set; }
        public string invoiceno { get; set; }
    }

    public class BOMInvoicereturnProductsCreteria : BaseEntity
    {
        public Lrinvoicereturn.ReturnTypes returntype { get; set; }
        public bool includebarcoded { get; set; }
        public string mode { get; set; }
        public long supplierid { get; set; }
        public string invoiceno { get; set; }
        public string barcode { get; set; }
        public bool allowall { get; set; }
        public DateTime returndate { get; set; }
        public long locationid { get; set; }
        public long companyid { get; set; }
        public long patternid { get; set; }
        public long styleid { get; set; }
        public long brandid { get; set; }
        public string lrentryno { get; set; }
        public List<string> barcodes { get; set; }
        public List<long> invoiceitemids { get; set; }
    }

    public class BOMInvoicereturnPrint : BOMInvoicereturn
    {
        public decimal totaltax { get; set; }
        public string freightdisplaytext
        {
            get
            {
                if (freight == 1)
                    return "TO PAY";
                else if (freight == 2)
                    return "PAID";
                else
                    return "";
            }
        }
        public decimal netamount
        {
            get
            {
                return amount - discount;
            }
        }
        public decimal grandtotal
        {
            get
            {
                return total;
            }
        }

        public decimal igsttaxpercentage
        {
            get
            {
                if (BOMinvoicereturnitems.Count > 0)
                    return BOMinvoicereturnitems.Max(x => x.taxpercentage);
                return 0;
            }
        }

        public decimal sgsttaxpercentage
        {
            get
            {
                if (BOMinvoicereturnitems.Count > 0)
                    return BOMinvoicereturnitems.Max(x => x.taxpercentage / 2);
                return 0;
            }
        }

        public decimal cgsttaxpercentage
        {
            get
            {
                if (BOMinvoicereturnitems.Count > 0)
                    return BOMinvoicereturnitems.Max(x => x.taxpercentage / 2);
                return 0;
            }
        }

        public decimal cgsttaxamount
        {
            get
            {
                if (isinterstatesale) return 0;
                return BOMinvoicereturnitems.Sum(x => x.tax / 2);
            }
        }

        public decimal igsttaxamount
        {
            get
            {
                if (isinterstatesale) 
                    return BOMinvoicereturnitems.Sum(x => x.tax);
                return 0;
            }
        }
        public string secondtax
        {
            get
            {
                return isinterstatesale ? "IGST" : "SGST";
            }
        }
        public decimal sgsttaxamount
        {
            get
            {
                if (isinterstatesale) return 0;
                return BOMinvoicereturnitems.Sum(x => x.tax / 2);
            }
        }

        public decimal totaletdiscountpercentage
        {
            get
            {
                if (discount == 0) return 0;
                return Math.Round((discount / amount) * 100, 2);
            }
        }


        public Company company { get; set; }
        public string invoiceinfo { get; set; }
        public string lrentrynos { get; set; }
        public string invoicenos { get; set; }
        public string invoicedates { get; set; }
        public string agentname { get; set; }
        public string lrinfo { get; set; }
        public decimal piece { get; set; }
        public Suppliercompany supplier { get; set; }
        public Transport transport { get; set; }
        public Suppliercompany shiptosupplier { get; set; }
        public bool hasship
        {
            get
            {
                return shiptosupplierid > 0;
            }
        }

        public bool hasFreight
        {
            get
            {
                return freightamt > 0;
            }
        }
        public decimal taxpercentage { get; set; }
        public string reporttype
        {
            get
            {
                string[] types = {"Debit Note","DOA Debit Note","DOA Credit Note",
                    "Price Drop Credit Note","Recycle Credit Note","Discount Note","Supplier Credit Note" };
                return types[returntype];
            }
        }
        public CompanyLocation location { get; set; }
        public List<TaxSplit> taxlist = new List<TaxSplit>();
        public List<Lrinvoicereturnitems> particularwise { get; set; }
        //public Lrinvoice invoice { get; set; }
        //public Lrentry lrentry { get; set; }
    }
}
