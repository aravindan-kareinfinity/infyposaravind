using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class DOA :BaseEntity
    {
        public string changestatus { get; set; }
        public long suppliercompanyid { get;  set; }
        public long supplierid { get;  set; }
        public List<Deathonarrival> Items { get; set; }
        public List<DeathonarrivalReplacement> Replacements { get; set; }
        public long receivedemployeeid { get;  set; }
        public DateTime doadate { get;  set; }
        public string errormessage { get;  set; }
        public Deathonarrival.DOAStatus status { get; set; }
    }
    public class Deathonarrival : BaseEntity
    {
        public DateTime modifiedon { get; set; }
        public bool isactive { get; set; }
        public long createdby { get; set; }
        public long locationid { get; set; }
        public long companyid { get; set; }
        public string registerno { get; set; }
        public long receivedemployeeid { get; set; }
        public long approvedemployeeid { get; set; }
        
        public DateTime doadate { get; set; }
        public int version { get; set; }
        public long id { get; set; }
        public long modifiedby { get; set; }
        public long stockid { get; set; }
        public string barcode { get; set; }
        public long salablegoodsid { get; set; }
        public decimal qty { get; set; }
        public decimal rate { get; set; }
        public decimal discount { get; set; }
        public decimal tax { get; set; }
        public decimal receivable { get; set; }
        public DateTime createdon { get; set; }
        public long replacedoaid { get; set; }
        public decimal areceivable { get; set; }
        public string printingname { get; set; }
        public DateTime statusupdatedon { get; set; }
        public long lrinvoicereturnid { get; set; }
        public long lrinvoicereturnitemid { get; set; }
        public int status { get; set; }
        public DOAStatus doastatus { get
            {
                return (DOAStatus)status;
            }
            set
            {
                status = (int)value;
            }
        }

        public long invoiceitemid { get;  set; }
        public long itemid { get;  set; }
        public long productid { get;  set; }
        public long taxid { get;  set; }
        public decimal discountpercentage { get;  set; }
        public DateTime invoicedate { get;  set; }
        public string invoiceno { get;  set; }
        public string outcome { get; set; }
        public long suppliercompanyid { get;  set; }
        public long supplierid { get;  set; }
        public decimal taxpercentage { get;  set; }
        public decimal sellingprice { get; set; }
        public long barcodeid { get; set; }

        public enum DOAStatus
        {
            None,
            Replacement,
            CreditNote,
            DebitNote,
            Return,
            Alternate,
            ReplacedItem
        }
    }

    public class DeathonarrivalReplacement: Deathonarrival
    {
        public Deathonarrival replacement { get; set; }
    }
    public class DeathonarrivalCreteria : Deathonarrival
    {
        public DateTime todate { get;  set; }
        public DateTime fromdate { get;  set; }
    }
}
