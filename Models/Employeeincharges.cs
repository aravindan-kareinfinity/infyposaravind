using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class Employeeincharges : BaseEntity
    {
        public decimal incentive { get; set; }
        public long locationid { get; set; }
        public DateTime activefrom { get; set; }
        public DateTime activeto { get; set; }
        public bool isactive { get; set; }
        public int version { get; set; }
        public DateTime createdon { get; set; }
        public long createdby { get; set; }
        public long id { get; set; }
        public DateTime modifiedon { get; set; }
        public long employeeid { get; set; }
        public long modifiedby { get; set; }
        public long inchargeid { get; set; }
        public bool incasvalue { get; set; }
        public string inchargetype { get; set; }
    }
    public class EmployeeinchargesCreteria : Employeeincharges
    {
        public DateTime sfrom { get; set; }
        public DateTime sto { get; set; }

    }

    /// <summary>Rows for product incharge bill sales report (ReportPrdBillSales).</summary>
    public class EmployeeinchargePrdBillRow
    {
        public long _id { get; set; }
        public string employeename { get; set; }
        public string productname { get; set; }
        public string sectionname { get; set; }
        public decimal billedqty { get; set; }
        public decimal taxable { get; set; }
        public decimal netcost { get; set; }
        public decimal basecost { get; set; }
    }
}
