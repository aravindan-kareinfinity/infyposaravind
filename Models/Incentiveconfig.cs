using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class Incentiveconfig : BaseEntity
    {
        public long id { get; set; }
        public bool isactive { get; set; }
        public long createdby { get; set; }
        public long modifiedby { get; set; }
        public int version { get; set; }
        public DateTime createdon { get; set; }
        public DateTime modifiedon { get; set; }
        public long salablegoodsid { get; set; }
        public decimal incentive { get; set; }
        public decimal percentage { get; set; }
        public long locationid { get; set; }
        public string barcode { get; set; }
    }

    public class IncentiveUpdate : BaseEntity
    {
        public long locationid { get; set; }
        public decimal incentive { get; set; }
        public List<IncentiveItem> items { get; set; }
        public bool success { get; set; }
        public string exception { get; set; }
    }
    public class IncentiveItem
    {
        public bool ispercentage { get; set; }
        public long id { get; set; }
        public string barcode { get; set; }
        public string sectionname { get; set; }
        public string productname { get; set; }
        public decimal incentive { get; set; }
        public decimal sectionincentive { get; set; }
        public decimal price { get; set; }
        public decimal from { get; set; }
        public decimal to { get; set; }
        public long salablegoodsid { get; set; }
        public int version { get;  set; }
    }
    public class IncentiveconfigCreteria : Incentiveconfig
    {
        public bool searchall { get; set; }
        public string type { get; set; }
        public List<string> barcodes { get; set; }

        public List<long> ids { get; set; }
    }
}
