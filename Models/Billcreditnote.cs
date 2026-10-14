using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class Billcreditnote : BaseEntity
    {
        public long customerid { get; set; }
        public decimal tax { get; set; }
        public decimal amount { get; set; }
        public decimal totalamount { get; set; }
        public decimal adjustedtax { get; set; }
        public decimal adjustedvalue { get; set; }
        public decimal adjustedtotal { get; set; }
        public long companyid { get; set; }
        public DateTime creditnotedate { get; set; }
        public string creditnoteno { get; set; }
        public DateTime modifiedon { get; set; }
        public DateTime createdon { get; set; }
        public int version { get; set; }
        public long modifiedby { get; set; }
        public long createdby { get; set; }
        public bool isactive { get; set; }
        public long id { get; set; }
        public long returnreasonid { get; set; }
        public decimal rounding { get; set; }
        public long locationid { get; set; }
        public bool isinterstatesale { get; set; }
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string voucherinfo_json { get { return Newtonsoft.Json.JsonConvert.SerializeObject(voucherinfo); } set { voucherinfo = Newtonsoft.Json.JsonConvert.DeserializeObject<Voucherinfo>(value); } }
        public Voucherinfo voucherinfo { get; set; }
        public long mastersettlementid { get; set; }
        public string customername { get; set; }
        public string remarks { get; set; }
        public string enteredby { get; set; }
        public List<Billcreditnoteitems> Billcreditnoteitems { get; set; }
        public class Voucherinfo
        {
        }
    }
    public class BillcreditnoteCreteria : Billcreditnote
    {
        public DateTime fromdate { get; set; }
        public DateTime todate { get; set; }
        public string billno { get; set; }

      
    }

    public class BillcreditnotePrint : Billcreditnote
    {
        public decimal totaltax { get; set; }
        public string inwords { get; set; }
        public decimal netamount
        {
            get
            {
                return adjustedvalue;
            }
        }
        public decimal grandtotal
        {
            get
            {
                return adjustedtotal;
            }
        }

        public decimal cgsttaxamount
        {
            get
            {
                if (isinterstatesale) return 0;
                return adjustedtax;
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
                if (isinterstatesale) return adjustedtax;
                return adjustedtax > 0 ? adjustedtax / 2 : 0;
            }
        }
        public Company company { get; set; }
        public string invoiceinfo { get; set; }
        public string invoicenos { get; set; }
        public string invoicedates { get; set; }
        public string agentname { get; set; }
        public string lrinfo { get; set; }
        public decimal piece { get; set; }
        public Suppliercompany supplier { get; set; }
        public Transport transport { get; set; }
        public Suppliercompany shiptosupplier { get; set; }
        public CompanyLocation location { get; set; }
        public Customer customer { get; internal set; }

        public List<TaxSplit> taxlist = new List<TaxSplit>();
        //public Lrinvoice invoice { get; set; }
        //public Lrentry lrentry { get; set; }
    }
}
