
using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace InfyPOS.Models
{
    public class ReportBuilder : BaseEntity
    {
        public bool datemode { get; set; }
        public string daterange { get; set; }
        public string reportmode { get; set; }
        public long locationid { get; set; }
        public DateTime fromdate { get; set; }
        public DateTime todate { get; set; }
        public long widgetid { get; set; }
        public int topcount { get; set; }
        public string topfield { get; set; }

        public List<ReportAttributes> creterialist { get; set; }
        public class ReportAttributes
        {
            public Options option { get; set; }
            public long selectedvalue { get; set; }
            public string entervalue { get; set; }
            public decimal fromvalue { get; set; }
            public decimal tovalue { get; set; }
            public string daterange { get; set; }
            public DateTime fromdate { get; set; }
            public DateTime todate { get; set; }
            public string display { get; set; }
            public bool showcolumn { get; set; }
            public InfyPOS.AnalyserDataService.Condtion.Range range { get; set; }
        }

        public class ReportResultItems : BaseEntity
        {
            [JsonIgnore]
            public string id { get; set; }
            [JsonIgnore]
            public string name { get; set; }
            [JsonIgnore]
            public long[] Values { get; set; }
            public string[] String { get; set; }
            public decimal[] Amount { get; set; }
            public string[] RangeResult { get; set; }
        }

        public List<ReportResultItems> result { get; set; }
        public List<Options> columnlist { get; set; }
        public List<Options> rangelist { get; set; }
        public List<Options> aggregatelist { get; set; }
        public List<Options> conditionlist { get; set; }
        public void UpdateDateRange(List<ReportAttributes> list)
        {
            if (!datemode)
            {
                var dr = GetDateFromRange(daterange);
                fromdate = dr.Item1;
                todate = dr.Item2;
            }
            foreach (var item in list.FindAll(x => x.option.type == 3))
            {
                if (!string.IsNullOrEmpty(item.daterange))
                {
                    var dr = GetDateFromRange(item.daterange);
                    item.fromdate = dr.Item1;
                    item.todate = dr.Item2;
                }
            }
        }
        public Tuple<DateTime, DateTime> GetDateFromRange(string source)
        {
            int reducedays = 0;
            DateTime fromdate;
            switch (source)
            {
                case "YESTERDAY":
                    return new Tuple<DateTime, DateTime>(DateTime.Now.Date.AddDays(-1), DateTime.Now.Date.AddDays(-1));
                case "LAST2DAYS":
                    return new Tuple<DateTime, DateTime>(DateTime.Now.Date.AddDays(-1), DateTime.Now.Date);
                case "LAST7DAYS":
                    return new Tuple<DateTime, DateTime>(DateTime.Now.Date.AddDays(-6), DateTime.Now.Date);
                case "WEEK":
                    reducedays = ((int)DateTime.Now.Date.DayOfWeek) * -1;
                    fromdate = DateTime.Now.Date.AddDays(reducedays);
                    return new Tuple<DateTime, DateTime>(fromdate, fromdate.AddDays(7));
                case "LAST2WEEK":
                    reducedays = ((int)DateTime.Now.Date.DayOfWeek) * -1;
                    fromdate = DateTime.Now.Date.AddDays(reducedays);
                    return new Tuple<DateTime, DateTime>(fromdate.AddDays(-7), fromdate.AddDays(7));
                case "MONTH":
                    reducedays = (DateTime.Now.Date.Day-1) * -1;
                    fromdate = DateTime.Now.Date.AddDays(reducedays);
                    return new Tuple<DateTime, DateTime>(fromdate, fromdate.AddMonths(1).AddDays(-1));
                case "LASTMONTH":
                    reducedays = (DateTime.Now.Date.Day-1) * -1;
                    fromdate = DateTime.Now.Date.AddDays(reducedays).AddMonths(-1);
                    return new Tuple<DateTime, DateTime>(fromdate, fromdate.AddMonths(1).AddDays(-1));
                case "LAST2MONTH":
                    reducedays = (DateTime.Now.Date.Day-1) * -1;
                    fromdate = DateTime.Now.Date.AddDays(reducedays).AddMonths(-1);
                    return new Tuple<DateTime, DateTime>(fromdate, fromdate.AddMonths(2).AddDays(-1));
                case "YEAR":
                    fromdate = new DateTime(DateTime.Now.Date.Year, 1, 1);
                    return new Tuple<DateTime, DateTime>(fromdate, fromdate.AddYears(1).AddDays(-1));
                case "LASTYEAR":
                    fromdate = new DateTime(DateTime.Now.Date.Year - 1, 1, 1);
                    return new Tuple<DateTime, DateTime>(fromdate, fromdate.AddYears(1).AddDays(-1));
                case "LASTYEARTODAY":
                    fromdate = DateTime.Now.Date.AddYears(-1);
                    return new Tuple<DateTime, DateTime>(fromdate, fromdate);
                case "LASTYEARYESTERDAY":
                    fromdate = DateTime.Now.Date.AddYears(-1).AddDays(-1);
                    return new Tuple<DateTime, DateTime>(fromdate, fromdate);
                case "LASTYEARTOMORROW":
                    fromdate = DateTime.Now.Date.AddYears(-1).AddDays(1);
                    return new Tuple<DateTime, DateTime>(fromdate, fromdate);
                case "LASTYEARTHISMONTH":
                    reducedays = (DateTime.Now.Date.Day - 1) * -1;
                    fromdate = DateTime.Now.Date.AddDays(reducedays);
                    return new Tuple<DateTime, DateTime>(fromdate.AddYears(-1), fromdate.AddYears(-1).AddMonths(1));
                case "LASTYEARPREVIOUSMONTH":
                    reducedays = (DateTime.Now.Date.Day - 1) * -1;
                    fromdate = DateTime.Now.Date.AddDays(reducedays).AddMonths(-1);
                    return new Tuple<DateTime, DateTime>(fromdate.AddYears(-1), fromdate.AddYears(-1).AddMonths(1));
                case "LASTYEARNEXTMONTH":
                    reducedays = (DateTime.Now.Date.Day - 1) * -1;
                    fromdate = DateTime.Now.Date.AddDays(reducedays);
                    return new Tuple<DateTime, DateTime>(fromdate.AddYears(-1).AddMonths(1), fromdate.AddYears(-1).AddMonths(2));
                case "LAST2YEAR":
                    fromdate = new DateTime(DateTime.Now.Date.Year - 1, 1, 1);
                    return new Tuple<DateTime, DateTime>(fromdate, fromdate.AddYears(2).AddDays(-1));
                case "FINYEAR":
                    if (DateTime.Now.Date.Month > 3)
                        fromdate = new DateTime(DateTime.Now.Date.Year, 4, 1);
                    else
                        fromdate = new DateTime(DateTime.Now.Date.Year - 1, 4, 1);
                    return new Tuple<DateTime, DateTime>(fromdate, fromdate.AddYears(1).AddDays(-1));
                case "LASTFINYEAR":
                    if (DateTime.Now.Date.Month > 3)
                        fromdate = new DateTime(DateTime.Now.Date.Year - 1, 4, 1);
                    else
                        fromdate = new DateTime(DateTime.Now.Date.Year - 2, 4, 1);
                    return new Tuple<DateTime, DateTime>(fromdate, fromdate.AddYears(1).AddDays(-1));
                case "LAST2FINYEAR":
                    if (DateTime.Now.Date.Month > 3)
                        fromdate = new DateTime(DateTime.Now.Date.Year - 1, 4, 1);
                    else
                        fromdate = new DateTime(DateTime.Now.Date.Year - 2, 4, 1);
                    return new Tuple<DateTime, DateTime>(fromdate, fromdate.AddYears(2).AddDays(-1));
            }
            return new Tuple<DateTime, DateTime>(DateTime.Now.Date, DateTime.Now.Date);
        }
        
        public class Options
        {
            public string name { get; set; }
            public int id { get; set; }
            public int type { get; set; }
            public int index { get; set; }
            public string columnname { get; set; }
            public bool hasreference { get; set; }
            public string customoption { get; set; }
        }

    }

}
