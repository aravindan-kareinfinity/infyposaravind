using InfyPOS.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class Lrinvoicetax : BaseEntity
    {
        public enum EntryType
        {
            Taxable,
            Tax,
            Discount,
            DiscuntOnDiscount,
            Insurance,
            ServiceCharge,
            Courier,
            Packing,
            Fright,
            Adjustment
        }

        public EntryType type { get; set; }
        public long invoiceid { get; set; }
        public long taxid { get; set; }
        public decimal amounton { get; set; }
        public decimal percentage { get; set; }
        public decimal taxamount { get; set; }
        public decimal netamount { get; set; }
        public decimal discount { get; set; }
        public decimal discountpercentage { get; set; }
        public long createdby { get; set; }
        public long modifiedby { get; set; }
        public DateTime createdon { get; set; }
        public DateTime modifiedon { get; set; }
        public int version { get; set; }
        public bool isactive { get; set; }
        public bool isinterstatesale { get; set; }
        public long id { get; set; }
        public decimal taxableamount
        {
            get
            {
                return amounton - discount;
            }
        }
        public decimal cgsttaxamount
        {
            get
            {
                if (isinterstatesale) return 0;
                if (taxamount != 0) return Math.Round(taxamount / 2, 2);
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
                if (taxamount != 0) return Math.Round(taxamount / 2, 2);
                return 0;
            }
        }
        public decimal igsttaxamount
        {
            get
            {
                return isinterstatesale ? taxamount : 0;
            }
        }
    }
    public class LrinvoicetaxCreteria : Lrinvoicetax
    {
    }
}
