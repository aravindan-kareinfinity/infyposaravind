using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class Mfgbinstoragestockbase : BaseEntity
    {
        public string sizegroup { get; set; }
        public string designname { get; set; }
        public string uid { get; set; }
        public string sizename { get; set; }
        public string colourname { get; set; }
        public decimal stock { get; set; }
        public string product { get; set; }

    }
    public class Mfgbinstoragestock : Mfgbinstoragestockbase
    {
        public long id { get; set; }
        public bool isactive { get; set; }
        public DateTime createdon { get; set; }
        public DateTime modifiedon { get; set; }
        public long createdby { get; set; }
        public long modifiedby { get; set; }
        public int status { get; set; }
        public int version { get; set; }
        public long stockid { get; set; }
        public decimal iqty { get; set; }
        public decimal oqty { get; set; }
        public decimal dqty { get; set; }
        
        public long designid { get; set; }
        public long sizegroupid { get; set; }

        public long sizeid { get; set; }
        public long colourid { get; set; }

        public bool hasstock { get; set; }
        public long mfgbinstorageid { get; set; }

        public class StockAvailable
        {
            public long stockid { get; set; }
            public decimal stockqty { get; set; }
            public decimal allocated { get; set; }
        }
        public class StockAvailableSummary
        {
            public decimal stockqty { get; set; }
            public decimal allocated { get; set; }
            public decimal balance { get; set; }
            public List<StockAvailable> items { get; set; }
        }
    }
   
        public class MfgbinstoragestockCreteria : Mfgbinstoragestock
    {
        public bool onlystock { get; set; }
    }

    public class MfgbinstorageChange : BaseEntity
    {
        public List<Mfgbinstoragestock.StockAvailable> items { get; set; }
        public Mfgbinstorage storage { get; set; }
        public decimal qty { get; set; }
        public string uan { get; set; }
        public string mfgbinstoragecode { get; set; }
    }
}
