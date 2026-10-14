using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class BillhistoryFull : BaseEntity
    {

    }
    public class Billhistory : BaseEntity
    {
        public long companyid { get; set; }
        public long locationid { get; set; }
        public long id { get; set; }
        public DateTime fromdate { get; set; }
        public DateTime todate { get; set; }
        public bool isactive { get; set; }
        public long createdby { get; set; }
        public long modifiedby { get; set; }
        public int version { get; set; }
        public DateTime createdon { get; set; }
        public DateTime modifiedon { get; set; }
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string creteria_json { get { return Newtonsoft.Json.JsonConvert.SerializeObject(creteria); } set { creteria = Newtonsoft.Json.JsonConvert.DeserializeObject<SaleEdit>(value); } }
        public SaleEdit creteria { get; set; }
        public decimal tax { get; set; }
        public decimal receivable { get; set; }
        public string editedby { get; set; }
        public List<Billhistoryitems> Billhistoryitems { get; set; }
    }
    public class BillhistoryCreteria : Billhistory
    {

    }


    public class BillEdit
    {
        public long billid { get; set; }
        public Bill OLDBill { get; set; }
        public Bill NewBill { get; set; }
        public List<SaleSaveItem> items { get; set; }
        public bool delete { get; set; }
        public Billsettlementmaster OLDSettlement { get;  set; }
        public bool multiplesettlement { get;  set; }
        public long settlementmasterid { get;  set; }
        public bool rejected { get;  set; }
        public Billsettlementmaster NewSettlement { get;  set; }
    }
    public class SaleEdit : BaseEntity
    {
        public DateTime billdatefrom { get; set; }
        public DateTime billdateto { get; set; }
        public string barcode { get; set; }
        public string billno { get; set; }
        public bool includeaditionaldiscount { get; set; }
        public bool includediscount { get; set; }
        public bool includescheme { get; set; }
        public bool includegst { get; set; }
        public bool includecash { get; set; }
        public bool includecard { get; set; }
        public bool includecredit { get; set; }
        public bool includeb2b { get; set; }
        public long brandid { get; set; }
        public long companyid { get; set; }
        public long locationid { get; set; }
        public long productid { get; set; }
        public long sectionid { get; set; }
        public int minimumqty { get; set; }
        public int minimumbarcode { get; set; }
        public decimal billvaluefrom { get; set; }
        public decimal billvalueto { get; set; }
        public decimal marginfrom { get; set; }
        public decimal marginto { get; set; }
        public decimal pricefrom { get; set; }
        public decimal priceto { get; set; }
        public decimal purchasetaxpercentage { get; set; }
        public decimal salestaxpercentage { get; set; }
        public int mode { get; set; }
        public Modes SearchMode
        {
            get
            {
                return (Modes)mode;
            }
        }

        public enum Modes
        {
            AllBill = 1,
            CreditBills = 2,
            CashBills = 3,
            B2BBills = 4,
            CardBIlls = 5
        }
        public List<SaleSaveItem> items { get; set; }
    }

    public class BillHistoryInfo:BaseEntity
    {
        public int deletedbillcount { get; set; }
        public int deletedbillitemcount { get; set; }
        public int rejectedbillcount { get; set; }
        public int rejectedbillitemcount { get; set; }
        public decimal reducedamount { get; set; }
        public decimal reducedtax { get; set; }
        public decimal rejectedbillamount { get; set; }
        public class ProductWise
        {
            public string name { get; set; }
            public decimal tax { get; set; }
            public decimal receivable { get; set; }
        }
    }

    public class SaleSaveItem : SaleEditItem
    {
        public bool deleted { get; set; }
    }
    public class SaleEditItem : BaseEntity
    {
        public string product { get; set; }
        public long pid { get; set; }
        public string barcode { get; set; }
        public string bno { get; set; }
        public DateTime bdate { get; set; }
        public long bid { get; set; }
        public decimal tax { get; set; }
        public decimal rate { get; set; }
        public decimal salerate { get; set; }
        public decimal saleamount { get; set; }
        public decimal qty { get; set; }
        public decimal price { get; set; }
        public decimal cost { get; set; }
        public decimal discount { get; set; }
        public decimal additionaldiscount { get; set; }
        public decimal schemediscount { get; set; }
        public decimal receivable { get; set; }
        public decimal billvalue { get; set; }
        public decimal taxpercentage { get; set; }
        public decimal ptaxpercentage { get; set; }
        public long schemeid { get; set; }
        public long id { get; set; }
        public long tid { get; set; }
    }
}
