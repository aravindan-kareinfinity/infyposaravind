using InfyGST;
using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class Lrinvoicereturn : BaseEntity
    {
        [JsonIgnore]
        public string title
        {
            get
            {
                return returntype < 10 ? "Debit Note" : "Credit Note";
            }
        }
        public bool IsDebitNote
        {
            get
            {
                return returntype < 10;
            }
        }
        [JsonIgnore]
        public string reporttype
        {
            get
            {
                string[] types =
                    {"Debit Note",
                    "DOA Debit Note",
                    "DOA Credit Note",
                    "Price Drop Credit Note",
                    "Recycle Credit Note",
                    "Discount Note",
                    "Supplier Credit Note",
                    "Supplier Advance",
                    "Supplier Advance Refund","",
                    "Excess Credit Note",
                    "Price Credit Note"};
                return types[returntype];
            }
        }

        public long companyid { get; set; }
        public long locationid { get; set; }
        public DateTime lrdate { get; set; }
        public long id { get; set; }
        public bool isactive { get; set; }
        public bool hasapprove { get; set; }
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
        public List<Lrinvoicereturnitems> Lrinvoicereturnitems { get; set; }
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

        [JsonIgnore]
        public string einvoicejson { get; set; }
        public string billimg { get; set; }
        public string eway { get; set; }
        public string einvoice { get; set; }
        public string vehicleno { get; set; }
        public bool hsn4digit { get; set; }
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
        [JsonIgnore]
        public Company company { get; set; }
        public Transport transport { get; set; }

        public enum ReturnTypes
        {
            DebitNote=0,
            DOADebitNote=1,
            DOACreditNote=2,
            PriceDropCreditNote=3,
            RecycleCreditNote=4,
            DiscountDebityNote=5,
            SupplierCreditNote=6,
            SupplierAdvanceNote=7,
            ExcessCreditNote =11,
            PriceCreditNote=12,
            SupplierAdvanceRefund = 13
        }
    }
    public class LrinvoicereturnCreteria : Lrinvoicereturn
    {
        public string ids { get; set; }
        public DateTime fromreturndate { get; set; }
        public DateTime toreturndate { get; set; }
        public string barcode { get; set; }
        public bool isaudit { get; set; }
        public bool isprint { get; set; }
        public string lrentryno { get; set; }
        public string invoiceno { get; set; }
        public int modeofdelivery { get; set; }
    }

    public class LrinvoicereturnAuto : BaseEntity
    {
        public long id { get; set; }
        public decimal qtyadded { get; set; }
        public decimal qtyremoved { get; set; }
        public decimal valinc { get; set; }
        public decimal valdec { get; set; }
        public decimal taxinc { get; set; }
        public decimal taxdec { get; set; }
        public decimal drnotevalue { get; set; }
        public decimal crnotevalue { get; set; }
        public string overridemode { get; set; }
        public List<LrinvoiceItemsAuto> lrinvoicereturnitems { get; set; }
        public class LrinvoiceItemsAuto : LrinvoiceItemDetail
        {
            public decimal nactualqty { get; set; }
            public decimal nbuyingprice { get; set; }
        }
    }
    public class LrinvoicereturnProductsCreteria : BaseEntity
    {
        public Lrinvoicereturn.ReturnTypes returntype { get; set; }
        public bool includebarcoded { get; set; }
        public string mode { get; set; }
        public long supplierid { get; set; }
        public string invoiceno { get; set; }
        public string lrentryno { get; set; }
        public string barcode { get; set; }
        public bool allowall { get; set; }
        public DateTime returndate { get; set; }
        public long locationid { get; set; }
        public long companyid { get; set; }
        public long patternid { get; set; }
        public long styleid { get; set; }
        public long brandid { get; set; }
        public List<string> barcodes { get; set; }
        public List<long> invoiceitemids { get; set; }
    }

    public class LrinvoicereturnPrint : Lrinvoicereturn
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

        public decimal cgsttaxamount
        {
            get
            {
                if (isinterstatesale) return 0;
                decimal value = 0;
                foreach (var item in Lrinvoicereturnitems)
                    value += item.taxsplit.cgsttaxamount;

                // freight tax given separately freighttaxamt
                //if (freighttaxamt > 0)
                //    return value + freighttaxamt;
                return value;
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
                if (isinterstatesale) return tax;
                decimal value = 0;
                foreach (var item in Lrinvoicereturnitems)
                    value += item.taxsplit.sgsttaxamount;

                // freight tax given separately freighttaxamt
                //if (freighttaxamt > 0)
                //    return value + freighttaxamt;
                return value;
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
        public CompanyLocation location { get; set; }
        public List<TaxSplit> taxlist = new List<TaxSplit>();
        public List<DebitNoteHSN> HSNList { get; set; }

        public class DebitNoteHSN
        {
            public string code { get; set; }
            public decimal qty { get; set; }
            public decimal peice { get; set; }
            public decimal tax { get; set; }
            public decimal taxpercentage
            {
                get
                {
                    if (igst_percentage > 0) return igst_percentage;
                    return cgst_percentage * 2;
                }
            }
            public decimal taxable { get; set; }
            public decimal total { get; set; }
            public decimal cgst_percentage { get; set; }
            public decimal sgst_percentage { get; set; }
            public decimal igst_percentage { get; set; }
            public decimal cgst_value { get; set; }
            public decimal sgst_value { get; set; }
            public decimal igst_value { get; set; }
        }
        public List<Lrinvoicereturnitems> particularwise { get; set; }
        [JsonIgnore]
        public GSTPortal.EInvoiceResponse GSTEinvoice { get; set; }
        [JsonIgnore]
        public bool HasEinvoice
        {
            get
            {
                return GSTEinvoice != null;
            }
        }
        //public Lrinvoice invoice { get; set; }
        //public Lrentry lrentry { get; set; }
    }
}
