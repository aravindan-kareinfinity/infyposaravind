using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class Jobworkbillgrnitems : BaseEntity
    {
        public long chargetaxid { get; set; }
        public decimal chargetaxpercentage { get; set; }
        public decimal chargetax { get; set; }
        public decimal materialcost { get; set; }
        public decimal cost { get; set; }
        public decimal mcost { get; set; }
        public decimal chargerate { get; set; }
        public decimal materialsalevalue { get; set; }
        public long createdby { get; set; }
        public bool isactive { get; set; }
        public long invoiceitemid { get; set; }
        public long productid { get; set; }
        public long stockid { get; set; }
        public long salablegoodsid { get; set; }
        public long newinvoiceitemid { get; set; }
        public string designid { get; set; }
        public string barcode { get; set; }
        public long styleid { get; set; }
        public long patternid { get; set; }
        public long brandid { get; set; }
        public long sizeid { get; set; }
        public long sleeveid { get; set; }
        public long typeid { get; set; }
        public long colourid { get; set; }
        public long materialid { get; set; }
        public int sno { get; set; }
        public long id { get; set; }
        public long jobworkbillgrnid { get; set; }
        public decimal qty { get; set; }
        public decimal generatedqty { get; set; }
        public DateTime modifiedon { get; set; }
        public DateTime createdon { get; set; }
        public int version { get; set; }
        public long modifiedby { get; set; }
        public long jobworkbillitemsid { get; set; }
        public decimal chargetotal { get; set; }
        public decimal chargegross { get; set; }
        public long chargeid { get; set; }
        public string attributedisplay { get; set; }
        public decimal finalprice { get; set; }
        public decimal dealerprice { get; set; }
        public long lrinvoiceid { get; set; }
        public long oldproductid { get; set; }
        public string imageid { get; set; }
        public List<string> barcodes { get; internal set; }
    }

    public class JobworkbillgrnitemsCreteria : Jobworkbillgrnitems
    {
    }

    public class JobWorkDetails
    {
        public string jobworkername { get; set; }
        public string jobworkercity { get; set; }
        public decimal jobworkcharge { get; set; }
        public decimal jobworkchargetotal { get; set; }
        public DateTime entrydate { get; set; }
        public Models.Items detail { get; set; }
        public string entryno { get; set; }

    }
}
