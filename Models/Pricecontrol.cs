using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class Pricecontrol : BaseEntity
    {
        public long id { get; set; }
        public string name { get; set; }
        public long productid { get; set; }
        public long brandid { get; set; }
        public long patternid { get; set; }
        public long materialid { get; set; }
        public long sizeid { get; set; }
        public long styleid { get; set; }
        public long colourid { get; set; }
        public long typeid { get; set; }
        public bool disabled { get; set; }
        public bool isactive { get; set; }
        public long createdby { get; set; }
        public long modifiedby { get; set; }
        public int version { get; set; }

        public int costmode { get; set; }
        public decimal costvalue { get; set; }

        public int marginmode { get; set; }
        public decimal marginvalue { get; set; }
        public string itemcode { get; set; }

        public int pricingmode { get; set; }
        public decimal pricingvalue { get; set; }
        public decimal dealervalue { get; set; }
        public int discountmode { get; set; }
        public decimal discountvalue { get; set; }
        public DateTime createdon { get; set; }
        public DateTime modifiedon { get; set; }
        public int Weightage { get; set; }
        public void setWeightage()
        {
            Weightage = productid > 0 ? 25 : 0;
            Weightage += sectionid > 0 ? 20 : 0;
            Weightage += supplierid > 0 ? 35 : 0;
            Weightage += brandid > 0 ? 15 : 0;
            Weightage += (sizeid > 0 ? 14 : 0);
            Weightage += (styleid > 0 ? 13 : 0);
            Weightage += (materialid > 0 ? 12 : 0);
            Weightage += (patternid > 0 ? 11 : 0);
            Weightage += (colourid > 0 ? 10 : 0);
        }
        public decimal pricefrom { get; set; }
        public decimal priceto { get; set; }
        public bool isupdated { get; set; }
        public long supplierid { get; set; }
        public long locationid { get; set; }
        public long sectionid { get; set; }
        public long companyid { get; set; }
    }
    public class PricecontrolCreteria : Pricecontrol
    {
        public bool fetchprices { get; set; }
        public long itemid { get; set; }
        public string designid { get; set; }
        public decimal price { get; set; }
        public long suppliercompanyid { get; set; }
    }

    public class Pricecontrolinfo :BaseEntity
    {
        public List<Pricecontrol> items { get; set; }
        public LrinvoiceItemDetail lastprice  { get; set; }
        public List<LrinvoiceItemDetail> lastpricelist { get; set; }
    }

}
