using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class BOMBundles : BaseEntity
    {
        public long id { get; set; }
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string items_json
        {
            get { return Newtonsoft.Json.JsonConvert.SerializeObject(items); }
            set { items = Newtonsoft.Json.JsonConvert.DeserializeObject<List<BOMBundlesitem>>(value); }
        }
        public List<BOMBundlesitem> items { get; set; }
        public string code { get; set; }
        public long fromlocationid { get; set; }
        public long fromdepartmentid { get; set; }
        public long todepartmentid { get; set; }
        public long fromcompanyid { get; set; }
        public long bomrequestid { get; set; }
        public long tocompanyid { get; set; }
        public long tolocationid { get; set; }
        public long packedby { get; set; }
        public long receivedby { get; set; }
        public DateTime packedon { get; set; }
        public DateTime receivedon { get; set; }
        public decimal qty { get; set; }
        public string notes { get; set; }
        public long approvedby { get; set; }
        public bool isactive { get; set; }
        public bool iscompanytransfer { get; set; }

        public DateTime createdon { get; set; }
        public DateTime modifiedon { get; set; }
        public long createdby { get; set; }
        public long modifiedby { get; set; }
        public long companyid { get; set; }
        public int status { get; set; }
        public int version { get; set; }
        public string errormessage { get; set; }
        public statuslist Status
        {
            get
            {
                return (statuslist)status;
            }
            set
            {
                status = (int)value;
            }
        }
        public enum statuslist
        {
            NewBundle= 0,
            Inwarded = 1,
            InwardedWithChanges = 2,
            Rejected=9
        }
    }
    public class BOMBundlesCreteria : BOMBundles
    {
        public DateTime fromdate { get; set; }
        public DateTime todate { get; set; }
        public string ErrorMessage { get; set; }
    }

    public class BOMBundlesitem : BaseEntity
    {
        public long id { get; set; }
        public long bomcategoryid { get; set; }
        public long bomid { get; set; }
        public decimal qty { get; set; }
        public decimal aqty { get; set; }
        public decimal cost { get; set; }
        public long colourid { get; set; }
        public string imageid { get; set; }
        public string bomcode { get; set; }
        public string width { get; set; }
    }

    public class BOMBundlesStockCreteria : BaseEntity
    {
        public enum SearchType
        {
            None = 0,
            Brand = 1,
            Product = 2,
            BarcodedOn = 3,
            AllStock = 4,
            Barcode = 5,
            LREntry = 6,
            Supplier = 7
        }

        public long locationid { get; set; }
        public string fileid { get; set; }
        public DateTime date { get; set; }
        public long productid { get; set; }
        public long brandid { get; set; }
        public string barcode { get; set; }
        public long companyid { get; set; }
        public int searchtype { get; set; }
        public string lrentryno { get; set; }
        public long retaillocationid { get; set; }
        public long supplierid { get; set; }
        public SearchType Type
        {
            get
            {
                return (SearchType)searchtype;
            }
        }
    }

    public class BOMBundlesItemTransferHistory
    {
        public string code { get; set; }
        public long fromlocationid { get; set; }
        public long tolocationid { get; set; }
        public DateTime packedon { get; set; }
        public DateTime receivedon { get; set; }
        public decimal qty { get; set; }
        public long id { get; set; }
        public bool returngoods { get; set; }
    }

    public class BOMBundlesPrint : BOMBundles
    {
        
        public decimal netamount
        {
            get
            {
                return groupitems != null ? groupitems.Sum(e => e.taxable) : 0;
            }
        }
        public decimal tax
        {
            get
            {
                return groupitems != null ? groupitems.Sum(e => e.tax) : 0;
            }
        }
        public decimal grandtotal
        {
            get
            {
                return groupitems != null ? Math.Round(groupitems.Sum(e => e.amount)) : 0;
            }
        }
        public decimal rounding
        {
            get
            {
                return groupitems != null ? (Math.Round(groupitems.Sum(e => e.amount))- groupitems.Sum(e => e.amount)) : 0;
            }
        }
        public string inwords { get; set; }
        public Company company { get; set; }
        public Company tocompany { get; set; }
        public CompanyLocation fromlocation { get; set; }
        public CompanyLocation tolocation { get; set; }
        public List<Printableitem> groupitems { get; set; }

        public class Printableitem
        {
            public long bomid { get; set; }
            public long colourid { get; set; }
            public string bom { get; set; }
            public string bomprintingname { get; set; }
            public string bomcode { get; set; }
            public string category { get; set; }
            public string colour { get; set; }
            public string width { get; set; }
            public int sno { get; set; }
            public string hsn { get; set; }
            public decimal costprice { get; set; }
            public string unit { get; set; }
            public decimal qty { get; set; }
            public decimal rqty { get; set; }
            public decimal aqty { get; set; }
            public decimal taxable { get; set; }
            public decimal taxpercentage { get; set; }
            public decimal igst { get; set; }
            public decimal cgst { get; set; }
            public decimal sgst { get; set; }
            public decimal amount { get; set; }

            public decimal tax
            {
                get
                {
                    return igst + sgst + cgst;
                }
            }
        }
    }

}

