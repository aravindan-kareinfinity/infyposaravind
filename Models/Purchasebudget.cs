using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class Purchasebudget : BaseEntity
    {
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string budgets_json { get { return Newtonsoft.Json.JsonConvert.SerializeObject(budgets); } 
            set { budgets = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Budgets>>(value); } }
        public List<Budgets> budgets { get; set; }
        public int version { get; set; }
        public bool isactive { get; set; }
        public DateTime createdon { get; set; }
        public DateTime modifiedon { get; set; }
        public long createdby { get; set; }
        public long modifiedby { get; set; }
        public long budgetid { get; set; }
        public long employeeid { get; set; }
        public DateTime pfrom { get; set; }
        public DateTime pto { get; set; }
        public DateTime sfrom { get; set; }
        public DateTime sto { get; set; }
        public decimal budget { get; set; }
        public decimal alloted { get; set; }
        public long id { get; set; }
        public int status { get; set; }
        public string blockreason { get; set; }
        public class Budgets
        {
            public long productid { get; set; }
            public decimal value { get; set; }
            public decimal percentage { get; set; }
        }

        public class Report
        {
            public List<ReportItem> items { get; set; }
        }
        public class ReportItem
        {
            public string budgetname { get; set; }
            public string employee { get; set; }
            public decimal configbudgetproducts { get; set; }
            public decimal productbudgetconfig { get; set; }
            public string product { get; set; }
            public decimal orderqty { get; set; }
            public decimal receivedqty { get; set; }
            public decimal ordervalue { get; set; }
            public decimal purchaseqty { get; set; }
            public decimal purchasevalue { get; set; }
            public decimal configbudget { get; set; }
        }
    }
    public class PurchasebudgetCreteria : Purchasebudget
    {
        public List<long> budgetids { get; set; }
        public List<long> managerids { get; set; }
        public List<long> productids { get; set; }
        public Report reportid { get; set; }

        public enum Report
        {
            None,
            Budget,
            Manager,
            Product,
            Detail
        }
    }
}
