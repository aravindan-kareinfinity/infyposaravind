using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class ComponentPOitems : BaseEntity
    {
        public long id { get; set; }
        public long bomcategoryid { get; set; }
        public string categoryname { get; set; }
        public long bomid { get; set; }
        public long colourid { get; set; }
        public string colourname { get; set; }
        public long taxid { get; set; }
        public long uomid { get; set; }
        public string uomname { get; set; }
        public int coretypeid { get; set; }
        public decimal qty { get; set; }
        public string width { get; set; }
        public decimal rate { get; set; }
        public decimal amount { get; set; }
        public string bomcode { get; set; }
        public bool isactive { get; set; }
        public long createdby { get; set; }
        public long modifiedby { get; set; }
        public int version { get; set; }
        public string imageid { get; set; }
        public long componentorderid { get; set; }
        public DateTime createdon { get; set; }
        public DateTime modifiedon { get; set; }
        public string brandname { get; set; }
        public string colourcode { get; set; }
        public string colourshade { get; set; }
        public string designid { get; set; }

    }
    public class ComponentPOitemsDetail : ComponentPOitems
    {
        public string pono { get; set; }
        public DateTime podate { get; set; }
        public string bomcategoryname { get; set; }
        public string bomname { get; set; }
        public long approvedby { get;set; }
        public long preparedby { get; set; }
        public long supplierid { get; set; }
        public long companyid { get; set; }
        public DateTime expectedate { get; set; }
        public string hsncode { get; set; }
        public string colourlist { get; set; }
        public int noofcolours { get; set; }
        public decimal avgqty { get; set; }
        public decimal taxpercentage { get; set; }
        public decimal tax { get; set; }
    }

    public class ComponentPOitemsCreteria : ComponentPOitems
    {
    }
}
