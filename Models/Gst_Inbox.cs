using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class Gst_Inbox : BaseEntity
    {
        public long id { get; set; }
        public long companyid { get; set; }
        public DateTime fromdate { get; set; }
        public DateTime todate { get; set; }
        public string gstno { get; set; }
        public string gsttype { get; set; }
        public string apiresponse { get; set; }
        public string gstperiod { get; set; }

        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public bool showfiling { get; set; }
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string gstsession_json { get; set; }

        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string gstadata_json { get; set; }
        public long createdby { get; set; }
        public DateTime createdon { get; set; }
        public long modifiedby { get; set; }
        public DateTime modifiedon { get; set; }
        public bool isactive { get; set; }
        public bool hsn4digit { get; set; }
        public int version { get; set; }
    }

    public class GST_Invoice : BaseEntity
    {
        public long billid { get; set; }
        public long voucherid { get; set; }
        public long companyid { get; set; }
        public long billinboxid { get; set; }
        public long lrinvoicereturnid { get; set; }
        public long transportid { get; set; }
        public long distance { get; set; }
        public string vehcicleno { get;  set; }
        public long ewbNo { get; set; }
        public bool hsn4digit { get;  set; }
        public bool hsn8digit { get; set; }
        public bool hsn6digit { get; set; }
        public string irn { get; set; }
        public bool ewaypartb { get; set; }
        public int modeofdelivery { get; set; }
    }
    public class GST_Transfer : BaseEntity
    {
        public long bundleid { get; set; }
        public long companyid { get; set; }
        public long transportid { get; set; }
        public string vehcicleno { get; set; }
        public long ewbNo { get; set; }
        public bool hsn4digit { get; set; }
        public bool hsn8digit { get; set; }
        public bool hsn6digit { get; set; }
        public int modeofdelivery { get; set; }
        public int distance { get; set; }
        public string irn { get; set; }
        public bool ewaypartb { get; set; }
        public string subSupplyType { get; set; }
        public string subSupplyDesc { get; set; }
    }
    public class GST_Expense : BaseEntity
    {
        public long id { get; set; }
        public string gstno { get; set; }
        public string invoiceno { get; set; }
        public string tradename { get; set; }
        public DateTime invoicedate { get; set; }
        public decimal tax { get; set; }
        public decimal taxable { get; set; }
    }

    public class GST_Exclude : BaseEntity
    {
        public long id { get; set; }
        public long lrinvoiceid { get; set; }
        public long gstinboxid { get; set; }
        public DateTime invoicedate { get; set; }
        public string gstno { get; set; }
        public string invoiceno { get; set; }
        public string reason { get; set; }
        public bool isreserve { get; set; }
    }

    public class GST_Reconciliation : GST_Exclude
    {
        public DateTime fileddate { get; set; }
        public string suppliername { get; set; }
        public bool isinterstatesale { get; set; }
        public decimal taxable { get; set; }
        public decimal taxpercentage { get; set; }
        public decimal tax { get; set; }
        public decimal difftax { get; set; }
        public string type { get; set; }
        public int count { get; set; }

        public string ptinvoiceno { get; set; }
        public string qtinvoiceno { get; set; }
        public string qtentryno { get; set; }
        public string taxfrm { get; set; }
        public string rchrg { get; set; }
        public DateTime qtentrydate { get; set; }
        public string uniqueid { get; internal set; }
        public string filedmonth { get;  set; }
    }
    public class Gst_InboxCreteria : Gst_Inbox
    {
        public string gstusername { get; set; }
        public string gstaccesskey { get; set; }
        public string gstotp { get; set; }
    }

    public class Gst_InboxLocal : Gst_Inbox
    {
        public string type { get; set; }
        public string mode { get; set; }
    }

    public class SalesTransaction
    {
        public List<B2BInvoice> B2B { get; set; }
        public List<B2CInvoice> B2C { get; set; }
        public List<HSNCode> HSNCodes { get; set; }
        public List<HSNCode> WrongHSN { get; set; }
        public List<B2CSmall> B2CSmall { get; set; }
        public List<B2CLarge> B2CLarge { get; set; }
        public List<B2BInvoice> B2BCN { get; set; }
        public List<B2BInvoice> B2BDN { get; set; }
        public List<B2CExport> B2CExport { get; set; }
    }
    public class HSNCode
    {
        public bool isinterstatesale { get; set; }
        public decimal qty { get; set; }
        public string UQC { get; set; }
        public string hsncode { get; set; }
        public decimal taxpercentage { get; set; }
        public decimal tax { get; set; }
        public decimal receivable { get; set; }
    }
    public class B2CInvoice
    {
        public string pos { get; set; }
        public bool isinterstatesale { get; set; }
        public decimal taxpercentage { get; set; }
        public decimal tax { get; set; }
        public decimal receivable { get; set; }
    }
    public class B2CExport
    {
        public string customer { get; set; }
        public string billno { get; set; }
        public DateTime billdate { get; set; }
        public decimal taxpercentage { get; set; }
        public decimal tax { get; set; }
        public string type
        {
            get
            {
                return tax>0?"WPAY":"WOPAY";
            }
        }
        public decimal receivable { get; set; }
        public decimal fright { get; set; }
    }
    public class B2CSmall
    {
        public string pos { get; set; }
        public bool isinterstatesale { get; set; }
        public decimal taxpercentage { get; set; }
        public decimal tax { get; set; }
        public decimal receivable { get; set; }
    }

    
    public class B2BInvoice
    {
        public int billtype { get; set; }
        public long companyid { get; set; }
        public string pos { get; set; }
        public string gstno { get; set; }
        public string customer { get; set; }
        public string billno { get; set; }
        public DateTime billdate { get; set; }
        public decimal qty { get; set; }
        public bool isinterstatesale { get; set; }
        public decimal taxpercentage { get; set; }
        public decimal tax { get; set; }
        public decimal receivable { get; set; }
        public decimal filedtax { get; set; }
        public decimal filedreceivable { get; set; }
    }

    public class B2CLarge
    {
        public string pos { get; set; }
        public string customer { get; set; }
        public string billno { get; set; }
        public DateTime billdate { get; set; }

        public bool isinterstatesale { get; set; }
        public decimal taxpercentage { get; set; }
        public decimal tax { get; set; }
        public decimal receivable { get; set; }
    }
}
