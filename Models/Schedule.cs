using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class Schedule : BaseEntity
    {
        public long id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string config_json { get { return Newtonsoft.Json.JsonConvert.SerializeObject(config); } set { config = Newtonsoft.Json.JsonConvert.DeserializeObject<Config>(value); } }
        public Config config { get; set; }
        public bool enabled { get; set; }
        public bool isactionbased { get; set; }
        public bool isdaily { get; set; }
        public bool ismonthly { get; set; }
        public bool isweekly { get; set; }
        public int version { get; set; }
        public DateTime createdon { get; set; }
        public DateTime modifiedon { get; set; }
        public long modifiedby { get; set; }
        public long createdby { get; set; }
        public bool isactive { get; set; }
        public List<Scheduleditems> Scheduleditems { get; set; }
        public string defaultSourceID { get; internal set; }

        public class Config
        {
            public class GrowAAsan
            {
                public string businessid { get; set; }
                public string authkey { get; set; }
                public string waTemplateName { get; set; }
                public string varCount { get; set; }
                public string httpmode { get; set; }
                public bool hasattachment { get; set; }
            }
            public class CustomerConfiguration
            {
                public int beforedays { get; set; }
                public decimal discountpercent { get; set; }
                public decimal disocuntvalue { get; set; }
                public int validity { get; set; }
                public decimal min { get; set; }
                public decimal max { get; set; }
            }
            public class StockSummaryConfig
            {
                public long productid { get; set; }
                public long supplierid { get; set; }
                public bool showdesign { get; set; }
                public bool showproduct { get; set; }
                public bool showsupplier { get; set; }
                public bool showlocation { get; set; }
                public string mode { get; set; }
            }
            public class Discount
            {
                public decimal inpercentage { get; set; }
                public decimal invalue { get; set; }
            }
            public CustomerConfiguration customerconfig { get; set; }
            public StockSummaryConfig stocksummaryconfig { get; set; }
            public Discount addlndiscountconfig { get; set; }
            public GrowAAsan growaasan { get; set; }
            public string source { get; set; }
            public string output { get; set; }
            public string monthly { get; set; }
            public string format { get; set; }
            public string parameter { get; set; }
            public bool wk_0 { get; set; }
            public bool wk_1 { get; set; }
            public bool wk_2 { get; set; }
            public bool wk_3 { get; set; }
            public bool wk_4 { get; set; }
            public bool wk_5 { get; set; }
            public bool wk_6 { get; set; }
            public int tzoffset { get; set; }
            public List<HourConfig> daily { get; set; }
            public string sendto { get; set; }
            public string url { get; set; }
            public string authentication { get; set; }
            public string sendmode { get; set; }
            public string emailfrom { get; set; }
            public string emailsubject { get; set; }
            public string emailusername { get; set; }
            public string emailpassword { get; set; }
            public string emailhost { get; set; }
            public string emailport { get; set; }
            public bool emailssl { get; set; }
            public long companyid { get; set; }
            public long locationid { get; set; }
            public long sectionid { get; set; }
            public long supplierid { get; set; }
            public string brand { get; set; }
            public long productid { get; set; }
            public int mode { get; set; }
            public List<Scheduleditems> BuildDaily(List<Scheduleditems> scheduleditems)
            {
                if (scheduleditems == null) scheduleditems = new List<Scheduleditems>();
                if (daily == null) daily = new List<HourConfig>();
                foreach (var item in scheduleditems)
                {
                    if(item.hour>12)
                    {
                        if(!daily.Exists(e=>e.hour == item.hour-12 && e.evening))
                        {
                            item.deletemarked = true;
                        }
                    }
                    else
                    {

                        if (!daily.Exists(e => e.hour == item.hour && e.morning))
                        {
                            item.deletemarked = true;
                        }
                    }
                }

                foreach(var item in daily)
                {
                    if(item.evening && !scheduleditems.Exists(e => e.hour == item.hour + 12))
                    {
                        scheduleditems.Add(new Models.Scheduleditems()
                        {
                            hour = item.hour + 12
                        });
                    }
                    if (item.morning && !scheduleditems.Exists(e => e.hour == item.hour))
                    {
                        scheduleditems.Add(new Models.Scheduleditems()
                        {
                            hour = item.hour
                        });
                    }
                }
                scheduleditems.RemoveAll(e => e.deletemarked);
                return scheduleditems;
            }
            public void PrepareDaily(List<Scheduleditems> scheduleditems)
            {
                daily = new List<HourConfig>();
                for (int hour=1;hour<=12;hour++)
                {
                    var morning = scheduleditems != null && scheduleditems.Exists(e => e.hour == hour);
                    var evening = scheduleditems != null && scheduleditems.Exists(e => e.hour == hour+12);
                    daily.Add(new HourConfig()
                    {
                        hour = hour,
                        morning = morning,
                        evening = evening
                    });
                }
            }
        }

        public class HourConfig
        {
            public string name { get; set; }
            public bool morning { get; set; }
            public bool evening { get; set; }
            public int hour { get; set; }
        }
    }
    public class ScheduleCreteria : Schedule
    {
        public bool onlyactionable { get; set; }
        public bool onlyreports { get; set; }
    }
}
