using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class Jobworkbill : BaseEntity
    {
        public long supplierid { get; set; }
        public decimal taxamount { get; set; }
        public long id { get; set; }
        public decimal totalamount { get; set; }
        public decimal grossamount { get; set; }
        public decimal otheramount { get; set; }
        public decimal materialcost { get; set; }
        public decimal materialsalevalue { get; set; }
        public DateTime modifiedon { get; set; }
        public DateTime createdon { get; set; }
        public long createdby { get; set; }
        public bool settled { get; set; }
        public long locationid { get; set; }
        public long modifiedby { get; set; }
        public int version { get; set; }
        public long suppliercompanyid { get; set; }
        public long companyid { get; set; }
        public bool isactive { get; set; }
        public DateTime entrydate { get; set; }
        public DateTime expecteddate { get; set; }
        public bool isinterstatesale { get; set; }
        public string invoiceno { get; set; }
        public DateTime invoicedate { get; set; }
        public int noofpieces { get; set; }
        public decimal qty { get; set; }
        public decimal oqty { get; set; }
        public int status { get; set; }
        public string billmode { get; set; }
        public decimal paidamount { get; set; }
        public long taxid { get; set; }
        public long jobchargeid { get; set; }
        public string remarks { get; set; }
        public decimal receivedqty { get; set; }
        public string statustext { get; set; }
        public List<JobworkbillitemsDetail> Jobworkbillitems { get; set; }
        [JsonIgnore]
        public List<JobworkbillitemSpecification> specification { get; set; }
        public decimal totalmaterialcost
        {
            get
            {
                if (Jobworkbillitems == null) return 0;
                return Jobworkbillitems.Sum(e => e.totalmaterialcost);
            }
        }
        public decimal totalmaterialsalevalue
        {
            get
            {
                if (Jobworkbillitems == null) return 0;
                return Jobworkbillitems.Sum(e => e.totalmaterialsalevalue);
            }
        }
    }
    public class StockInfo : BaseEntity
    {
        public long companyid { get; set; }
        public long locationid { get; set; }
        public long productid { get; set; }
        public long supplierid { get; set; }
        public long brandid { get; set; }
        public long itemid { get; set; }
        public DateTime inwardon { get; set; }
        public decimal buyingprice { get; set; }
        public decimal ibuyingprice { get; set; }
        public decimal mrp { get; set; }
        public decimal price { get; set; }
        public decimal discount { get; set; }
        public decimal addlndiscount { get; set; }
        public decimal schemediscount { get; set; }
        public decimal iqty { get; set; }
        public decimal dqty { get; set; }
        public decimal oqty { get; set; }
        public decimal qty { get; set; }
        public long typeid { get; set; }
        public long colourid { get; set; }
        public long materialid { get; set; }
        public long styleid { get; set; }
        public long patternid { get; set; }
        public long sectionid { get; set; }
        public long sizeid { get; set; }
        public string barcode { get; set; }
        public string glncode { get; set; }
        public string itemname { get; set; }
        public string serialno { get; set; }
        public long stockid { get; set; }
        public long invoiceitemid { get; set; }
        public long salablegoodsid { get; set; }
        public string brandname { get; set; }
        public string suppliername { get; set; }
        public string patternname { get; set; }
        public string stylename { get; set; }
        public string productname { get; set; }
        public string sizename { get; set; }
        public string materialname { get; set; }
        public string typename { get; set; }
        public string designid { get; set; }
        public string hsncode { get; set; }
        public string colourname { get; set; }
        public string suppliercode { get; set; }
        public int sellingmode { get; set; }
        public string employeename { get; set; }
        public string location { get; set; }
        public DateTime createdon { get; set; }
        public decimal taxablevalue { get; set; }
        public decimal stockvalue { get; set; }
        public decimal istockvalue { get; set; }
        public decimal grosssalevalue { get; set; }
        public decimal salevalue { get; set; }
        public decimal netsalevalue { get; set; }
        public decimal taxvalue { get; set; }
        public decimal taxable { get; set; }
        public decimal margin { get; set; }
        public long markerid { get; set; }
        public long producttype { get; set; }
        public int stockage { get; set; }
        public int stockholdingperiod { get; set; }
        public decimal marginpercentage { get; set; }
        public decimal markdown { get; set; }
        public string imageid { get; set; }
    }

    public class JobWorkImage
    {
        public string supplier { get; set; }
        public string invoiceno { get; set; }
        public DateTime entrydate { get; set; }
        public string barcode { get; set; }
        public string imageid { get; set; }
        public string toinvoiceno { get; set; }
        public DateTime toentrydate { get; set; }
        public string tobarcode { get; set; }
        public string toimageid { get; set; }
    }
    public class JobworkbillCreteria : Jobworkbill
    {
        public DateTime entryfrom { get; set; }
        public DateTime entryto { get; set; }
        public DateTime duefrom { get; set; }
        public DateTime dueto { get; set; }
        public string barcode { get; set; }
        public List<string> barcodes { get; set; }
        public long[] ids { get; set; }
        public List<Data.SalesorderDataService.SearchBarcodeData.itemqty> usedstock { get; set; }
    }

    public class JobworkbillPrint : Jobworkbill
    {
        public Company company { get; set; }
        public Suppliercompany supplier { get; set; }
        public Transport transport { get; set; }
        public CompanyLocation location { get; set; }
    }
}
