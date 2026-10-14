using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class Stockhistory : BaseEntity
    {
        public long id { get; set; }
        public bool isactive { get; set; }
        public long createdby { get; set; }
        public long modifiedby { get; set; }
        public int version { get; set; }
        public DateTime createdon { get; set; }
        public DateTime modifiedon { get; set; }
        public string printingname { get; set; }
        public decimal price { get; set; }
        public string barcode { get; set; }
        public long invoiceitemid { get; set; }
        public long itemid { get; set; }
        public long productid { get; set; }
        public decimal discount { get; set; }
        public decimal additionaldiscount { get; set; }
        public decimal qty { get; set; }
        public decimal transfered { get; set; }
        public decimal damagedqty { get; set; }
        public decimal journalqty { get; set; }
        public decimal returnqty { get; set; }
        public decimal auditqty { get; set; }
        public string designid { get; set; }
        public decimal buyingprice { get; set; }
        public decimal ibuyingprice { get; set; }
        public string hsncode { get; set; }
        public bool isbit { get; set; }
        public long locationid { get; set; }
        public long stockid { get; set; }
        public string referenceid { get; set; }
        public ActionType actiontype { get; set; }
        public string reason { get; set; }
        public enum ActionType
        {
            StockAudit = 1,
            AttributeChange = 2,
            ComboItem = 3,
            ComboSet = 4
        }
    }
    public class StockhistoryCreteria : Stockhistory
    {
    }

    public class StockuanCreteria :Stockuan
    {

    }
    public class Stockuan : BaseEntity
    {
        public long id { get; set; }
        public bool isactive { get; set; }
        public long createdby { get; set; }
        public long modifiedby { get; set; }
        public int version { get; set; }
        public DateTime createdon { get; set; }
        public DateTime modifiedon { get; set; }
        public decimal price { get; set; }
        public string barcode { get; set; }
        public string serialno { get; set; }
        public long invoiceitemid { get; set; }
        public decimal discount { get; set; }
        public decimal additionaldiscount { get; set; }
        public decimal qty { get; set; }
        public decimal transfered { get; set; }
        public decimal buyingprice { get; set; }
        public long stockid { get; set; }
        public long retaillocationid { get; set; }
    }
}
