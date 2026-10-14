using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class Expenses : BaseEntity
    {

        public bool isactive { get; set; }
        public long createdby { get; set; }
        public long modifiedby { get; set; }
        public int version { get; set; }
        public long id { get; set; }
        public DateTime createdon { get; set; }
        public DateTime modifiedon { get; set; }

        public long expensestypeid { get; set; }
        public long vendortypeid { get; set; }

        public decimal amount { get; set; }

        public long modeofpaymentid { get; set; }

        public string note { get; set; }

        public string bankname { get; set; }

        public DateTime expensesdate { get; set; }

    }

    public class ExpensesCreteria : Expenses
    {
    }
}