using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class Lrinvoice : BaseEntity
    {
        public Lrinvoice() : base()
        {
            allowgenerte = true;
        }

        public List<decimal> multipacksno { get; set; }
        public string suppliercompany { get; set; }
        public string suppliercityname { get; set; }
        public string buyingcompany { get; set; }
        public long entrylocationid { get; set; }
        public long retaillocationid { get; set; }
        public long companyid { get; set; }
        public string invoiceno { get; set; }
        public string lrentryno { get; set; }
        public DateTime duedate { get; set; }
        public DateTime invoicedate { get; set; }
        public DateTime barcodedon { get; set; }
        public Decimal noofpieces { get; set; }
        public int noofbundle { get; set; }
        public decimal baseamount { get; set; }
        public decimal grossamount { get; set; }
        public decimal totalamount { get; set; }

        public decimal taxableamount { get; set; }
        public long lrid { get; set; }
        public bool allowgenerte { get; set; }
        public long id { get; set; }
        public decimal otheramount { get; set; }
        public decimal discount { get; set; }
        public decimal taxamount { get; set; }
        public DateTime modifiedon { get; set; }
        public DateTime createdon { get; set; }
        public long createdby { get; set; }
        public long modifiedby { get; set; }
        public int version { get; set; }
        public long suppliercompanyid { get; set; }
        public bool isactive { get; set; }
        public bool hasapprove { get; set; }
        public long qcby { get; set; }
        public bool supportpo { get; set; }
        public bool isinclusiveall { get; set; }
        public bool isinclusivediscount { get; set; }
        public bool hascommission { get; set; }
        public bool isinclusivetax { get; set; }
        public decimal billvalue { get; set; }
        public bool isinterstatesale { get; set; }
        public bool iscreditnote { get; set; }
        public bool isintercompanytransfer { get; set; }
        public List<LrinvoiceItemDetail> Lrinvoiceitems { get; set; }
        public List<Lrinvoicetax> Lrinvoicetaxitems { get; set; }
        public List<Lrentry> lrentrydetails { get; set; }
        public long supplierid { get; set; }
        public string invoiceentryno { get; set; }
        public DateTime entrydate { get; set; }
        public long lrsupplierid { get; set; }
        public bool isdirectpurchase { get; set; }
        public bool isdeliverychallan { get; set; }
        public bool isvendorbill { get; set; }

        public bool isrcm { get; set; }
        public bool hastds { get; set; }
        public long tdsledgerid { get; set; }
        public decimal tdsamount { get; set; }
        public decimal tdspercentage { get; set; }
        public bool hastcs { get; set; }
        public decimal tcsamount { get; set; }
        public bool isgenerated { get; set; }
        public bool barcodegenerationapproval { get; set; }
        public bool hasitems { get; set; }
        public long buyerinvoiceid { get; set; }
        public int statusid { get; set; }
        public int internalstatus { get; set; }
        public enum InvoiceStatus
        {
            Progress = 1,
            Completed = 2,
            Temporary = 3,
            Transfer = 4
        }
        public string errormessage { get; set; }
        public decimal expensepercentage { get; set; }
        public decimal expenseperpiece { get; set; }
        public decimal plessdiscount { get; set; }
        public decimal plessvalue { get; set; }
        public string createduser { get; set; }
        public string modifieduser { get; set; }
        //public long potypeid { get; set; }
        //public long poid { get; set; }
        public long purchaseorderid { get; set; }
        public int ptype { get; set; }
        public string pono { get; set; }
        public DateTime podate { get; set; }
        public bool settled { get; set; }
    }

    public class LrinvoiceCreteria : LrinvoiceCover
    {
        public bool hasapprove { get; set; }
        public bool includedebitnote { get; set; }
        public int paymentmode { get; set; }
        public string barcode { get; set; }
        public string ids { get; set; }
        public long id { get; set; }
        public string designid { get; set; }
        public DateTime fromdate { get; set; }
        public DateTime todate { get; set; }
        public DateTime entryfromdate { get; set; }
        public DateTime entrytodate { get; set; }
        public DateTime barcodedfromdate { get; set; }
        public DateTime barcodedtodate { get; set; }

        public bool LimitOne { get; set; }
        public bool showitems { get; set; }
        public long locationid { get; set; }
        public long purchaseorderid { get; set; }
        public long importreferenceid { get; set; }
        public int importreferencetype { get; set; }
        public enum ImportReferenceTypes
        {
            Direct,
            WorkOrderGRN,
            BOMGRN,
            UANPO,
            BOMTRANSFER
        }
        public bool includetdsrule { get; set; }
        public string searchtype { get; set; }
        public bool IsGRN { get; set; }
    }

    public class LrinvoiceCover : BaseEntity
    {
        public class TDSInformation
        {
            public long tdsledgerid { get; set; }
            public Ledger tdsledger { get; set; }
            public decimal invoiceamount { get; set; }
            public decimal returnamount { get; set; }
            public decimal taxpercentage
            {
                get
                {
                    if (tdsledger == null) return 0;
                    return tdsledger.taxpercentage > 0 ? tdsledger.taxpercentage : 0;
                }
            }
            public decimal balanceamount
            {
                get
                {
                    return (invoiceamount - returnamount);
                }
            }
            public decimal singletransaction { get; set; }
            public decimal totaltransaction { get; set; }
            public bool tdseligible
            {
                get
                {
                    if (tdsledgerid > 0 && totaltransaction > 0)
                    {
                        return balanceamount >= totaltransaction;
                    }
                    return false;
                }
            }
        }
        public TDSInformation tdsinfo { get; set; }
        public string city { get; set; }
        public long lrid { get; set; }
        public long suppliercompanyid { get; set; }
        public long companyid { get; set; }
        public long supplierid { get; set; }
        public long sourcesupplierid { get; set; }
        public long agentid { get; set; }
        public string agentname { get; set; }
        public string lrno { get; set; }
        public string invoiceno { get; set; }
        public DateTime entrydate { get; set; }
        public string lrentryno { get; set; }
        public string invoiceentryno { get; set; }
        public string suppliername { get; set; }
        public string suppliercityname { get; set; }
        public string sourcesuppliername { get; set; }
        public string suppliercompanyname { get; set; }
        public string suppliergstno { get; set; }
        public string companyname { get; set; }
        public int modeofdelivery { get; set; }
        public Inbox.StatusList status { get; set; }
        public List<Lrinvoice> Items { get; set; }
        public string companytitle { get; set; }
        public DateTime invoicedate { get; set; }
        public long invoiceid { get; set; }
        public bool supportpo { get; set; }
        public decimal additionaltax { get; set; }
        public decimal additionalamount { get; set; }
        public decimal totaldiscount { get; set; }
        public decimal totalamount { get; set; }
        public decimal totaltax { get; set; }
        public decimal addtionalcostpercentage { get; set; }
        public long retaillocationid { get; set; }
        public int noofbundels { get; set; }
        public int noofpieces { get; set; }
        public long transportid { get; set; }
        public DateTime lrdate { get; set; }
        public bool internalbuyer { get; set; }
        public decimal premargin { get; set; }
        public DateTime lrentrydate { get; set; }
        public int stockdays { get; set; }
        public int stockbatch { get; set; }
        public string errormessage { get; set; }
        public decimal lrcomputedexpense { get; set; }
        public decimal lrexpense { get; set; }
        public decimal agentpercentage { get; set; }
        public decimal agentcommission { get; set; }
        public string transport { get; set; }
        public DateTime receiveddate { get; set; }
        public decimal actualweight { get; set; }
        public decimal receivedweight { get; set; }
        public decimal chargedweight { get; set; }
        public int noofboxes { get; set; }
        public decimal frightcharge { get; set; }
        public decimal loadingcharge { get; set; }
        public decimal insurancecharge { get; set; }
        public decimal packingcharge { get; set; }
        public decimal forwardcharge { get; set; }
        public decimal otherscharge { get; set; }
        public long receivinglocationid { get; set; }

    }

    public class LrinvoiceDetail : Lrinvoice
    {
        public DateTime lrdate { get; set; }
        public int bundles { get; set; }
        public long transportid { get; set; }
        public string lrno { get; set; }
        public string lrentryno { get; set; }
        public string suppliercompanyname { get; set; }
        public string companyname { get; set; }
        public bool deliverychallan { get; set; }
        public long retaillocationid { get; set; }
        public Inbox.StatusList status { get; set; }
        public long transfercompanyid { get; set; }
        public bool internalbuyer { get; set; }
        public decimal premargin { get; set; }
        public bool ispricing { get; set; }
    }

    public class Stock2BulkSplit : BaseEntity
    {
        public long companyid { get; set; }
        public long locationid { get; set; }
        public class Item
        {
            public string nbarcode { get; set; }
            public string barcode { get; set; }
            public decimal qty { get; set; }
            public string product { get; set; }
            public string brand { get; set; }
            public string colour { get; set; }
            public string size { get; set; }
            public string style { get; set; }
            public string pattern { get; set; }
            public string material { get; set; }
            public string sleeve { get; set; }
            public long stockid { get; set; }
            public long nstockid { get; set; }
            public long itemid { get; set; }
            public long invoiceitemid { get; set; }
            public long salid { get; set; }
            public long productid { get; set; }
            public long brandid { get; set; }
            public long colourid { get; set; }
            public long sizeid { get; set; }
            public long styleid { get; set; }
            public long patternid { get; set; }
            public long materialid { get; set; }
            public long sleeveid { get; set; }
        }
        public List<Item> items { get; set; }
    }

    public class LRInvoiceItemBitChange : BaseEntity
    {
        public long companyid { get; set; }
        public LrinvoiceItemDetail Core { get; set; }
        public bool IsBit { get; set; }
        public bool IsPartial { get; set; }
        public string barcode { get; set; }
        public long locationid { get; set; }
        public List<LrinvoiceItemDetail> items { get; set; }
    }

    public class LRInvoiceItemSizeChange : BaseEntity
    {
        public LrinvoiceItemDetail Core { get; set; }

        public long companyid { get; set; }
        public List<Item> items { get; set; }
        public class Item
        {
            public long productid { get; set; }
            public long id { get; set; }
            public string code { get; set; }
            public string sizename { get; set; }
            public decimal qty { get; set; }
            public long sizeid { get; set; }
        }
    }

    public class LRInvoiceItemBarcode : BaseEntity
    {
        public long productid { get; set; }
        public long id { get; set; }
        public string code { get; set; }
        public int no { get; set; }
        public string name { get; set; }
        public decimal qty { get; set; }
        public int copy { get; set; }
        public int sellingmode { get; set; }
        public string sno { get; set; }
        public decimal price { get; set; }
        public string imageid { get; set; }
        public long companyid { get; set; }
        public string uan { get; set; }
    }

    public class PlannedInvoice : BaseEntity
    {
        public long supplierid { get; set; }
        public long companyid { get; set; }
        public long locationid { get; set; }
        public decimal dn { get; set; }
        public decimal amount { get; set; }
        public decimal balance { get; set; }
        public List<invoice> invoicelist { get; set; }
        public long suppliercompanyid { get; set; }

        public class invoice
        {
            public long lrinvoiceid { get; set; }
            public DateTime invoicedate { get; set; }
            public string invoiceno { get; set; }
            public decimal amount { get; set; }
            public decimal balance { get; set; }
            public bool isinvoice { get; set; }
        }
    }
    public class PendingLrInvoiceDetail : BaseEntity
    {
        public PendingLrInvoice source { get; set; }
        public Lrinvoice invoice { get; set; }
        public Purchaseorder purchaseorder { get; set; }
        public List<StockInfo> stocklist { get; set; }
        public List<TaxMatch> taxlist { get; set; }
        public List<AlertItem> alertlist { get; set; }
        public class AlertItem
        {
            public string reason { get; set; }
            public string description { get; set; }
            public decimal qty { get; set; }
            public decimal value { get; set; }
        }
        public class StockInfo
        {
            public string name { get; set; }
            public decimal soldqty { get; set; }
            public decimal stockqty { get; set; }
            public decimal preturnqty { get; set; }

            public decimal sold { get; set; }
            public decimal stock { get; set; }
            public decimal preturn { get; set; }
        }

        public class TaxMatch
        {
            public decimal percentage { get; set; }
            public decimal orderqty { get; set; }
            public decimal ordervalue { get; set; }
            public decimal ordertax { get; set; }
            public decimal invqty { get; set; }
            public decimal invvalue { get; set; }
            public decimal invtax { get; set; }

        }
    }
    public class PendingLrInvoice : BaseEntity
    {
        public long supplierid { get; set; }
        public long suppliercompanyid { get; set; }
        public long companyid { get; set; }
        public string supplier { get; set; }
        public string gstno { get; set; }
        public string msmeno { get; set; }
        public string city { get; set; }
        public long agentid { get; set; }
        public long tdsledgerid { get; set; }
        public decimal tdsamount { get; set; }
        public long cityid { get; set; }
        public long locationid { get; set; }
        public long sectionid { get; set; }
        public int invoicecount { get; set; }
        public DateTime duedate { get; set; }

        public decimal intreceive { get; set; }
        public decimal intpay { get; set; }
        public string pono { get; set; }
        public DateTime podate { get; set; }

        public long lrinvoiceid { get; set; }
        public string invoiceno { get; set; }
        public DateTime invoicedate { get; set; }
        public DateTime entrydate { get; set; }
        public string lrentryno { get; set; }
        public string slipno { get; set; }
        public DateTime invoiceentrydate { get; set; }
        public decimal invoicediscount { get; set; }
        public decimal totalamount { get; set; }
        public decimal taxamount { get; set; }
        public decimal agentcommission { get; set; }
        public decimal paidamount { get; set; }
        public decimal plannedpayment { get; set; }
        public decimal debetnoteamount { get; set; }
        public decimal balanceamount { get; set; }
        public decimal soldamount { get; set; }
        public decimal pendingpercent
        {
            get
            {
                return 100 - soldpercent;
            }
        }
        public decimal agentcomvalue
        {
            get
            {
                if(agentcommission>0 && grossamount>0)
                {
                    return Math.Round(grossamount * agentcommission / 100, 2);
                }
                return 0;
            }
        }
        public decimal soldpercent { get; set; }
        public bool intransit { get; set; }
        public decimal settlementamount { get; set; }
        public bool isinvoice { get; set; }
        public bool isjobwork { get; set; }
        public bool isdebitnote { get; set; }
        public bool iscreditnote { get; set; }
        public int duedays { get; set; }
        public string days { get; set; }
        public int ndays { get; set; }
        public string invoicetype { get; set; }
        public DateTime lrdate { get; set; }
        public string lrno { get; set; }
        public string transport { get; set; }
        public string remarks { get; set; }
        public string coreinvoiceno { get; internal set; }
        public DateTime coreinvoicedate { get; internal set; }

        public string agentname { get; set; }
        public string bankname { get; set; }
        public string bankbranch { get; set; }
        public string bankifsccode { get; set; }
        public string bankacno { get; set; }
        public string bankacname { get; set; }
        public string warn { get; set; }
        public decimal grossamount { get; set; }
        [JsonIgnore]
        public List<long> coreinvoiceids { get; set; }
        public long createdby { get; set; }
    }

    public class InvoiceSetted : BaseEntity
    {
        public List<long> ids { get; set; }
        public bool changetosettled { get; set; }
        public bool removesettled { get; set; }
    }
    public class InvoiceReport
    {
        public InvoiceReport()
        {

        }
        public InvoiceReport(Lrinvoice lrinvoice)
        {
            supplier = lrinvoice.suppliercompany;
            invoicedate = lrinvoice.invoicedate;
            invoiceno = lrinvoice.invoiceno;
            billvalue = lrinvoice.billvalue;
            minmargin = lrinvoice.Lrinvoiceitems.Min(e => e.marginpercentage);
            maxmargin = lrinvoice.Lrinvoiceitems.Max(e => e.marginpercentage);
            sellingprice = lrinvoice.Lrinvoiceitems.Sum(e => e.sellingprice * e.buyingqty);
            buyingprice = lrinvoice.Lrinvoiceitems.Sum(e => (e.basebuyingprice - e.purchasediscount) * e.buyingqty);
            avgmargin = Math.Round(buyingprice / sellingprice * 100, 2);
            totalqty = lrinvoice.Lrinvoiceitems.Sum(e => e.buyingqty);
            items = lrinvoice.Lrinvoiceitems.ConvertAll<Item>(e =>
            new Item()
            {
                cost = e.buyingprice + e.purchasetax / e.buyingqty,
                design = e.designid,
                price = e.sellingprice,
                product = e.productname,
                qty = e.buyingqty,
                margin = e.marginpercentage
            });
        }
        public string supplier { get; set; }
        public DateTime invoicedate { get; set; }
        public string invoiceno { get; set; }
        public decimal billvalue { get; set; }
        public decimal minmargin { get; set; }
        public decimal maxmargin { get; set; }
        public decimal sellingprice { get; private set; }
        public decimal buyingprice { get; private set; }
        public decimal avgmargin { get; set; }
        public decimal totalqty { get; private set; }
        public List<Item> items { get; set; }
        public class Item
        {
            public string product { get; set; }
            public string design { get; set; }
            public decimal qty { get; set; }
            public decimal cost { get; set; }
            public decimal price { get; set; }
            public decimal margin { get; set; }
        }
    }
    public class LrInvoiceQC : BaseEntity
    {
        public long id { get; set; }
        public long qcby { get; set; }
        public DateTime qcon { get; set; }
    }
    public class LrinvoicePrint : Lrinvoice
    {
        public string whocreated { get; set; }
        public decimal additionalamount { get; set; }
        public decimal additionaltax { get; set; }
        public List<DescripencyItem> descripencyItems { get; set; }
        public List<HSNItem> HSNList { get; set; }
        [JsonIgnore]
        public decimal rounding
        {
            get
            {
                return Math.Round(totalamount, MidpointRounding.AwayFromZero) - totalamount;
            }
        }
        [JsonIgnore]
        public decimal netamount
        {
            get
            {
                return Math.Round(totalamount, MidpointRounding.AwayFromZero);
            }
        }
        public class HSNItem
        {
            public bool isinterstate { get; set; }
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
        public class DescripencyItem
        {
            public int orderid { get; set; }
            public string uid { get; set; }
            public string product { get; set; }
            public decimal poqty { get; set; }
            public decimal tax { get; set; }
            public decimal totaltax
            {
                get
                {
                    return tax * descripencyqty;
                }
            }
            public decimal billqty { get; set; }
            public decimal actualqty { get; set; }
            public decimal cost { get; set; }
            public string design { get; set; }
            public decimal descripencyqty
            {
                get
                {
                    if (actualqty <= 0) return 0;
                    return actualqty != billqty ? actualqty - billqty : 0;
                }
            }
            public decimal descripencyvalue
            {
                get
                {
                    return cost * descripencyqty;
                }
            }

            public string designid { get; set; }
            public string colourname { get; set; }
            public string sizename { get; set; }
            public Items detail { get; set; }
        }
        public decimal cgsttaxamount
        {
            get
            {
                if (isinterstatesale) return 0;
                decimal value = 0;
                foreach (var item in Lrinvoicetaxitems)
                    value += item.taxamount;
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
                decimal value = 0;
                foreach (var item in Lrinvoicetaxitems)
                    value += (isinterstatesale ? item.taxamount : item.taxamount / 2);
                return value;
            }
        }
        public string amountinwords
        {
            get
            {
                return WebAPI.Data.Numberconvertor.rupees(totalamount);
            }
        }
        public string descripencyinwords
        {
            get
            {
                return WebAPI.Data.Numberconvertor.rupees(descripencyvalue);
            }
        }
        public decimal descripencyqty { get; set; }
        public decimal descripencyvalue { get; set; }
        public decimal actualvalue { get; set; }
        public Company company { get; set; }
        public Agent agent { get; set; }
        public Suppliercompany supplier { get; set; }
        public CompanyLocation location { get; set; }
        public long sourcesupplierid { get; set; }
        public string sourcesuppliername { get; set; }
        public string inwords { get; set; }
        public string lrentryinfo { get; set; }
        public string invoiceinfo { get; set; }
        public string lrinfo { get; set; }
        public decimal debitnoteqty { get; set; }
        public decimal debitnotevalue { get; set; }
        public string debitnoteno { get; set; }
        public bool hasdebitnote
        {
            get
            {
                return !string.IsNullOrEmpty(debitnoteno);
            }
        }

        public Transport transport { get; set; }
    }
}
