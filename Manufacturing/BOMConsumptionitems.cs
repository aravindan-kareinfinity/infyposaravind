using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class BOMConsumptionitems : BaseEntity
    {
        public long id { get; set; }
        public long bomcategoryid { get; set; }
        public long bomid { get; set; }
        public long colourid { get; set; }
        public string colourcode { get; set; }
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
        public long bomconsumptionid { get; set; }
        public DateTime createdon { get; set; }
        public DateTime modifiedon { get; set; }
        public bool isoutward { get; set; }
        public bool isconsumption { get; set; }
        public bool isrequest { get; set; }
    }
    public class BOMConsumptionitemsDetail : BOMConsumptionitems
    {
        public string entryno { get; set; }
        public DateTime entrydate { get; set; }
        public string bomcategoryname { get; set; }
        public string bomname { get; set; }
        public string bomtitle { get; set; }
        public long approvedby { get;set; }
        public long preparedby { get; set; }
        public long supplierid { get; set; }
        public long companyid { get; set; }
        public DateTime expectedate { get; set; }
        public string colourname { get; set; }

        public decimal opening { get; set; }
        public decimal closing { get; set; }
    }

    public class BOMConsumptionitemsCreteria : BOMConsumptionitems
    {
    }
}
