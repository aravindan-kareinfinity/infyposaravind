using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class BOMinvoice : BaseEntity
    {
        public BOMinvoice() : base()
        {
        }
        public int invoicetypeid { get; set; }
        public string suppliercompany { get; set; }
        public string buyingcompany { get; set; }
        public long companyid { get; set; }
        public string invoiceno { get; set; }
        public DateTime invoicedate { get; set; }
        public int noofpieces { get; set; }
        public int noofbundle { get; set; }
        public decimal baseamount { get; set; }
        public decimal grossamount { get; set; }
        public decimal totalamount { get; set; }
        public long lrid { get; set; }
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
        public bool supportpo { get; set; }
        public bool isinclusiveall { get; set; }
        public bool isinclusivediscount { get; set; }
        public bool isinclusivetax { get; set; }
        public long locationid { get; set; }
        public bool isjobinward { get; set; }
        public long mfgjoborderid { get; set; }

        public decimal billvalue { get; set; }
        public bool isinterstatesale { get; set; }
        public List<Bominvoiceitems> bominvoiceitems { get; set; }
        public List<BOMinvoicetax> bominvoicetaxitems { get; set; }
        public List<Lrentry> lrentrydetails { get; set; }
        public long supplierid { get; set; }
        public string invoiceentryno { get; set; }
        public DateTime entrydate { get; set; }
        public bool isdirectpurchase { get; set; }
        public int statusid { get; set; }
        public string jobcardno { get; set; }
        public enum InvoiceStatus
        {
            Progress = 1,
            Completed = 2,
            Temporary = 3,
            Transfer = 4
        }
    }

    public class BOMinvoiceCreteria : BOMinvoiceCover
    {
        public bool isjobinward { get; set; }
        public long mfgjoborderid { get; set; }
        public int mode { get; set; }
        public string barcode { get; set; }
        public string ids { get; set; }
        public long id { get; set; }
        public string designid { get; set; }
        public DateTime fromdate { get; set; }
        public DateTime todate { get; set; }
        public DateTime entryfromdate { get; set; }
        public DateTime entrytodate { get; set; }
        public bool LimitOne { get; set; }
        public bool showitems { get; set; }
    }

    public class BOMPrint : BaseEntity
    {
        public long reportid { get; set; }
        public List<BOMPrint2Print> items { get; set; }
    }
    public class BOMPrint2Print : BaseEntity, Report.IBarcodeFormat
    {
        public bool SpecialField(string field, SortedDictionary<string, Object> data)
        {
            if (data.ContainsKey(field)) return true;

            var fields = field.Split(':');
            //SPL:SPLIT:NPRICE:100:1
            if (fields[1] == "SPLIT" && fields.Length == 5)
            {
                if (KeyValue.ContainsKey(fields[2]))
                {
                    int length = 0;
                    int position = 0;
                    if (int.TryParse(fields[3], out length) &&
                        int.TryParse(fields[4], out position))
                    {
                        var fullvalue = KeyValue[fields[2]].ToString();
                        int startindex = (position-1)*length;
                        if(fullvalue.Length<startindex)
                        {
                            data.Add(field, "");
                            return true;
                        }
                        if(startindex+length>fullvalue.Length)
                        {
                            length = fullvalue.Length - startindex;
                        }
                        data.Add(field, fullvalue.Substring(startindex,length));
                        return true;
                    }
                }
            }
            return false;
        }
        public long id { get; set; }
        public long invoiceid { get; set; }
        public string joborderno { get; set; }
        public string jobcardno { get; set; }
        public DateTime inwardeddate { get; set; }
        public DateTime joborderdate { get; set; }
        public long bomid { get; set; }
        public string sno { get; set; }
        public string bomname { get; set; }
        public string suppliername { get; set; }
        public string colourcode { get; set; }
        public string colour { get; set; }
        public string bomcode { get; set; }
        public string bomtitle { get; set; }
        public string suppliercode { get; set; }
        public string bominwardcode { get; set; }
        public DateTime grndate { get; set; }
        public string grnno { get; set; }
        public string invoiceno { get; set; }
        public string colourname { get; set; }
        public decimal qty { get; set; }
        public int pqty { get; set; }
        [JsonIgnore]
        public SortedDictionary<string, object> KeyValue
        {
            get
            {
                SortedDictionary<string, object> ret = new SortedDictionary<string, object>();
                ret.Add("COLOUR", colour);
                ret.Add("COLOURCODE", colourcode);
                ret.Add("SUPPLIERCODE", suppliercode);
                ret.Add("NAME", bomname);
                ret.Add("BOMNAME", bomname);
                ret.Add("BARCODE", bominwardcode);
                ret.Add("DATE", grndate.ToString("dd-MM-yyyy"));
                ret.Add("QTY", qty);
                ret.Add("SNO", sno);
                ret.Add("SUPPLIERNAME", suppliername);
                ret.Add("BOMCODE", bomcode);
                ret.Add("BOMTITLE", bomtitle);
                ret.Add("GRNNO", grnno);
                ret.Add("COLOURNAME", colourname);
                ret.Add("JOBDATE", joborderdate.ToString("dd-MM-yyyy"));
                ret.Add("INWARDDATE", inwardeddate.ToString("dd-MM-yyyy"));
                ret.Add("JOBORDERNO", joborderno);
                ret.Add("JOBCARDNO", jobcardno);
                return ret;
            }
        }
        [JsonIgnore]
        public SortedDictionary<string, string> KeyFormat
        {
            get
            {
                SortedDictionary<string, string> ret = new SortedDictionary<string, string>();
                ret.Add("QTY", "N2");
                return ret;
            }
        }

    }

    public class BOMinvoiceCover : BaseEntity
    {
        public long lrid { get; set; }
        public long suppliercompanyid { get; set; }
        public string invoiceentryno { get; set; }
        public long companyid { get; set; }
        public long supplierid { get; set; }
        public long sourcesupplierid { get; set; }
        public long agentid { get; set; }
        public string agentname { get; set; }
        public string lrno { get; set; }
        public string invoiceno { get; set; }
        public DateTime entrydate { get; set; }
        public string BOMLrentryno { get; set; }
        public string suppliername { get; set; }
        public string suppliercityname { get; set; }
        public string sourcesuppliername { get; set; }
        public string suppliercompanyname { get; set; }
        public string companyname { get; set; }
        public int modeofdelivery { get; set; }
        public Inbox.StatusList status { get; set; }
        public List<BOMinvoice> Items { get; set; }
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
    }

    public class BOMInvoiceReportDetail
    {
        [Newtonsoft.Json.JsonIgnore]
        public long uniqueid { get; set; }
        public string invoiceno { get; set; }
        public string bominwardcode { get; set; }
        public string cityname { get; set; }
        public string suppliername { get; set; }
        public string bomcode { get; set; }
        public DateTime entrydate { get; set; }
        public DateTime invoicedate { get; set; }
        public string name { get; set; }
        public string shade { get; set; }
        public string colour { get; set; }
        public string width { get; set; }
        public decimal buyingqty { get; set; }
        public decimal buyingprice { get; set; }
        public decimal amount { get; set; }
        public decimal OrderQty { get; set; }
        public string ponumber { get; set; }
    }
    public class BOMinvoiceDetail : BOMinvoice
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
        public bool buyer { get; set; }
        public decimal premargin { get; set; }
        public BOMLrentry lrentrydetail { get; internal set; }
        public Supplier supplierinfo { get; internal set; }
        public Suppliercompany suppliercompanyinfo { get; internal set; }
        public Company company { get; internal set; }

        public decimal TotalGross
        {
            get
            {
                return bominvoiceitems.Sum(x => x.buyingqty * x.basebuyingprice);
            }
        }

        public decimal TotalDiscount
        {
            get
            {
                return bominvoiceitems.Sum(x => x.buyingqty * x.purchasediscount);
            }
        }

        public decimal TotalTax
        {
            get
            {
                return bominvoiceitems.Sum(x => x.purchasetax);
            }
        }

        public decimal TotalTaxpercentage
        {
            get
            {
                return bominvoiceitems.Max(x => x.purchasetaxpercentage);
            }
        }

        public decimal Rounding
        {
            get
            {
                if(bominvoicetaxitems != null && bominvoicetaxitems.Exists(x=>x.taxid == -1 && x.netamount != 0))
                {
                    return bominvoicetaxitems.Find(x => x.taxid == -1 && x.netamount != 0).netamount;
                }
                return 0;
            }
        }
        public decimal Total
        {
            get
            {
                return TotalGross - TotalDiscount + otheramount + TotalTax+ Rounding;
            }
        }
        public string amountinwords
        {
            get
            {
                return WebAPI.Data.Numberconvertor.rupees(Total);
            }
        }
    }

    public class BOMinvoiceItemBitChange : BaseEntity
    {
        public long companyid { get; set; }
        public Bominvoiceitems Core { get; set; }
        public bool IsBit { get; set; }
        public bool IsPartial { get; set; }
        public string barcode { get; set; }
        public long locationid { get; set; }
        public List<Bominvoiceitems> items { get; set; }
    }

    public class BOMinvoiceItemSizeChange : BaseEntity
    {
        public Bominvoiceitems Core { get; set; }

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

    public class BOMinvoiceItemBarcode : BaseEntity
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
    }
    public class PendingBOMinvoice : BaseEntity
    {
        public string supplier { get; set; }
        public long agentid { get; set; }
        public long cityid { get; set; }
        public int invoicecount { get; set; }
        public DateTime duedate { get; set; }
        public long BOMinvoiceid { get; set; }
        public string invoiceno { get; set; }
        public DateTime invoicedate { get; set; }
        public decimal totalamount { get; set; }
        public decimal paidamount { get; set; }
        public decimal balanceamount { get; set; }
        public decimal soldamount { get; set; }
        public decimal pendingpercent { get; set; }
        public decimal settlementamount { get; set; }
        public bool isinvoice { get; set; }
        public int duedays { get; set; }
        public string days { get; set; }
    }
}
