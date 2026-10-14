using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class PricedropMaster : BaseEntity
    {
        public string changestatus { get; set; }
        public long suppliercompanyid { get; set; }
        public long supplierid { get; set; }
        public List<Pricedrop> Items { get; set; }
        public long receivedemployeeid { get; set; }
        public DateTime pricedropdate { get; set; }
        public string errormessage { get; set; }
        public Pricedrop.Status status { get; set; }
    }

    public class PriceDropValue: Pricedrop
    {
        public Pricedrop Replacement { get; set; }
    }
    public class Pricedrop : BaseEntity
    {
        public enum Status
        {
            None,
            CreditNote,
            DebitNote,
            Cancelled
        }
        public string registerno { get; set; }
        public DateTime modifiedon { get; set; }
        public bool isactive { get; set; }
        public long invoiceitemid { get; set; }
        public long itemid { get; set; }
        public long productid { get; set; }
        public long taxid { get; set; }
        public long suppliercompanyid { get; set; }
        public long supplierid { get; set; }
        public string invoiceno { get; set; }
        public decimal discountpercentage { get; set; }
        public decimal taxpercentage { get; set; }
        public DateTime invoicedate { get; set; }
        public long createdby { get; set; }
        public long locationid { get; set; }
        public long companyid { get; set; }
        public long receivedemployeeid { get; set; }
        public long approvedemployeeid { get; set; }
        public int status { get; set; }
        public DateTime pricedropdate { get; set; }
        public int version { get; set; }
        public long id { get; set; }
        public long modifiedby { get; set; }
        public long stockid { get; set; }
        public string barcode { get; set; }
        public long salablegoodsid { get; set; }
        public decimal qty { get; set; }
        public decimal rate { get; set; }
        public decimal discount { get; set; }
        public decimal sellingprice { get; set; }
        public decimal dealerprice { get; set; }
        public decimal mrp { get; set; }
        public decimal minrate { get; set; }
        public decimal tax { get; set; }
        public decimal pricedrop { get; set; }
        public DateTime createdon { get; set; }
        public string printingname { get; set; }
        public DateTime statusupdatedon { get; set; }
        public decimal newdealerprice { get; set; }
        public decimal newminrate { get; set; }
        public decimal newsellingprice { get; set; }
        public decimal newmrp { get; set; }
        public decimal receivable {
            get
            {
                return qty * pricedrop;    
            }
        }
    }
    public class PricedropCreteria : Pricedrop
    {
    }
}
