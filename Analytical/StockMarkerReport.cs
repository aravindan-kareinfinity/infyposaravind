using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
using Newtonsoft.Json;
using System.Text;
using InfyPOS.Data;

namespace InfyPOS
{
    public class StockMarkerReport : SAASData
    {
        public class PurchasePlanConfig : BaseEntity
        {
            public long productid { get; set; }
            public long productgroupid { get; set; }
            public long companyid { get; set; }
            public long suppliercityid { get; set; }
            public long supplierid { get; set; }
            public long brandid { get; set; }
            public long sectionid { get; set; }
            public decimal markup { get; set; }
            public decimal plandays { get; set; }
            public DateTime fromdate { get; set; }
            public DateTime todate { get; set; }
            public AnalyserDataService.Options primarycolumn { get; set; }
            public AnalyserDataService.Options secondarycolumn { get; set; }
            public List<string> columns { get; set; }
            public string pricerange { get; set; }
        }
        //public AnalyserDataService.ResultGroup BuildPurchasePlan(PurchasePlanConfig pconfig)
        //{
        //    AnalyserDataService source = new AnalyserDataService() { Context = this.Context };
        //    source.Context = this.Context;
        //    return source.PurchasePlan(pconfig);
        //}
    }
}