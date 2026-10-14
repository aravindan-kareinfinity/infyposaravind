using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class Salablegoodshistory : BaseEntity
    {
        public long id { get; set; }
        public bool isactive { get; set; }
        public long createdby { get; set; }
        public long modifiedby { get; set; }
        public int version { get; set; }
        public DateTime createdon { get; set; }
        public DateTime modifiedon { get; set; }
        public string productname { get; set; }
        public decimal iqty { get; set; }
        public decimal oqty { get; set; }
        public decimal dqty { get; set; }
        public decimal jqty { get; set; }
        public decimal auditqty { get; set; }
        public long stockid { get; set; }
        public string barcode { get; set; }
        public string mbarcode { get; set; }
        public long locationid { get; set; }
        public decimal price { get; set; }
        public decimal discount { get; set; }
        public decimal additionaldiscount { get; set; }
        public long salablegoodsid { get; set; }
        public string referenceid { get; set; }
        public ActionType actiontype { get; set; }
        public enum ActionType
        {
            StockAudit = 1,
            AttributeChange = 2,
            ComboItem = 3,
            ComboSet = 4
        }
        public decimal currentqty { get; set; }
        public decimal whenqty { get; set; }
        public string empname { get; set; }
        public decimal auditvalue { get; set; }
        public string reason { get; set; }
    }
    public class SalablegoodshistoryCreteria : Salablegoodshistory
    {
    }
}
