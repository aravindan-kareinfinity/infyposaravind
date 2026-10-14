using InfyPOS.Common;
using InfyPOS.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class Storetransaction : BaseEntity
    {
        public long companyid { get; set; }
        public long locationid { get; set; }
        public DateTime actiondate { get; set; }
        public string type { get; set; }
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string attributes_json {
            get
            {
                if(type == "StoreTime")
                    return Newtonsoft.Json.JsonConvert.SerializeObject(StoreOpeningAttribute);
                if (type == "EBReading")
                    return Newtonsoft.Json.JsonConvert.SerializeObject(EBReadingAttribute);
                if (type == "WalkIn")
                    return Newtonsoft.Json.JsonConvert.SerializeObject(WalkinTokenAttribute);
                if (type == "Referral")
                    return Newtonsoft.Json.JsonConvert.SerializeObject(ReferralTokenAttribute);
                if (type == "SaleReturn")
                    return Newtonsoft.Json.JsonConvert.SerializeObject(SalesReturnAtribute);
                if (type == "StkDBoard")
                    return Newtonsoft.Json.JsonConvert.SerializeObject(StockDBoardAtribute);
                if (type == "OutOfStock")
                    return Newtonsoft.Json.JsonConvert.SerializeObject(OutOfStocks);
                return "null";
            }
            set
            {
                if (type == "StoreTime")
                    StoreOpeningAttribute = Newtonsoft.Json.JsonConvert.DeserializeObject<StoreOpening>(value);
                if (type == "EBReading")
                    EBReadingAttribute = Newtonsoft.Json.JsonConvert.DeserializeObject<EBReading>(value);
                if (type == "WalkIn")
                    WalkinTokenAttribute = Newtonsoft.Json.JsonConvert.DeserializeObject<WalkinToken>(value);
                if (type == "Referral")
                    ReferralTokenAttribute = Newtonsoft.Json.JsonConvert.DeserializeObject<ReferralToken>(value);
                if (type == "SaleReturn")
                    SalesReturnAtribute = Newtonsoft.Json.JsonConvert.DeserializeObject<StoreReturnBarcodes>(value);
                if (type == "StkDBoard")
                    StockDBoardAtribute = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Dashboard_StockVsSales>>(value);
                if (type == "OutOfStock")
                    OutOfStocks = Newtonsoft.Json.JsonConvert.DeserializeObject<List<long>>(value);
            }
        }

        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string walkincustomerinfo_json
        {
            get { return Newtonsoft.Json.JsonConvert.SerializeObject(walkincustomerinfo); }
            set
            {
                if (value != null)
                {
                    try
                    {
                        walkincustomerinfo = Newtonsoft.Json.JsonConvert.DeserializeObject<List<WalkinCustomerDetails>>(value);
                    }
                    catch (Exception exp)
                    {

                    }
                }
            }
        }
        public List<WalkinCustomerDetails> walkincustomerinfo { get; set; }
        public List<Dashboard_StockVsSales> StockDBoardAtribute { get; set; }
        public List<long> OutOfStocks { get; set; }
        public EBReading EBReadingAttribute { get; set; }
        public StoreOpening StoreOpeningAttribute { get; set; }
        public WalkinToken WalkinTokenAttribute { get; set; }
        public ReferralToken ReferralTokenAttribute { get; set; }
        public StoreReturnBarcodes SalesReturnAtribute { get; set; }
        public long createdby { get; set; }
        public long id { get; set; }
        public DateTime createdon { get; set; }
        public int version { get; set; }
        public bool isactive { get; set; }
        public long modifiedby { get; set; }
        public DateTime modifiedon { get; set; }
        public class WalkinCustomerDetails
        {
            public string token { get; set; }
            public long customerid { get; set; }
            public string mobileno { get; set; }
            public DateTime at { get; set; }
        }

        public class StoreOpening
        {
            public string opening { get; set; }
            public string closing { get; set; }
        }

        public class EBReading
        {
            public string no { get; set; }
            public string reading { get; set; }
        }
        public class WalkinToken
        {
            public int startno { get; set; }
            public int endno { get; set; }
            public int lastissuedno { get; set; }
            public List<WalkinCount> issuehistory { get; set; }
            public List<PrintCount> printhistory { get; set; }
            public class PrintCount
            {
                public DateTime at { get; set; }
                public int from { get; set; }
                public int to { get; set; }
            }
            public class WalkinCount
            {
                public long id { get; set; }
                public DateTime at { get; set; }
                public int count { get; set; }
                public int from { get; set; }
                public int to { get; set; }
                public string key { get; set; }
            }
        }

        public class StoreReturnBarcodes
        {
            public DateTime date { get; set; }
            public List<Item> items { get; set; }
            public class Item
            {
                public string barcode { get; set; }
                public decimal qty { get; set; }
                public decimal vqty { get; set; }
            }
        }
        public class ReferralToken
        {
            public int startno { get; set; }
            public int endno { get; set; }
            public int lastissuedno { get; set; }
            public List<PrintCount> printhistory { get; set; }
            public class PrintCount
            {
                public string token { get; set; }
                public DateTime at { get; set; }
                public int no { get; set; }
                public int mode { get; set; }
                public Modes Mode
                {
                    get
                    {
                        return (Modes)mode;
                    }
                }
                public enum Modes
                {
                    Customer,
                    Employee,
                    Agent
                }
                public long employeeid { get; set; }
                public long agentid { get; set; }
                public long printedby { get; set; }
                public long settlementid { get; set; }
                public decimal discount { get; set; }
                public decimal discountpercentage { get; set; }
                public string printtoken
                {
                    get
                    {
                        if(Mode == Modes.Customer)
                        {
                            return "CUS:" + token;
                        }
                        else if(Mode == Modes.Employee)
                        {
                            return "AGT:" + token;
                        }
                        return "EMP:" + token;
                    }
                }
                public string printtitle
                {
                    get
                    {
                        if (Mode == Modes.Customer)
                        {
                            return "Customer Discount Coupen";
                        }
                        else if (Mode == Modes.Employee)
                        {
                            return "Agent Referral";
                        }
                        return "Emplyee Referral";
                    }
                }
            }
        }
    }

    public class WalkinSummary : BaseEntity
    {
        public List<WalkinSummaryItem> List { get; set; }
        public List<WalkinSummaryHour> Hourly { get; set; }
        public class WalkinSummaryHour
        {
            public int hour24 { get; set; }
            public string hour { get; set; }
            public int printed { get; set; }
            public int issued { get; set; }
            public int billed { get; set; }
            public int actualbilled { get; set; }
        }
        public class WalkinSummaryItem
        {
            [JsonIgnore]
            public int token { get; set; }
            public string tokenno { get; set; }
            public string tokenprinted { get; set; }
            public string tokenissued { get; set; }
            public string billno { get; set; }
            public string billprinted { get; set; }
            [JsonIgnore]
            public DateTime printed { get; set; }
            [JsonIgnore]
            public DateTime issued { get; set; }
            [JsonIgnore]
            public DateTime billed { get; set; }
            public string customername { get; set; }
            public string mobileno { get; set; }
        }
    }
    
    public class StoretransactionCreteria : Storetransaction
    {
        public string tokennumbers { get; set; }
        public string scanin { get; set; }
        public string scanout { get; set; }
        public string printdocument { get; set; }
        public long agentid { get; set; }
        public long employeeid { get; set; }
        public decimal discount { get; set; }
        public decimal discountpercentage { get; set; }
        public string customermobileno { get; set; }
        public string customername { get; set; }
        public long customerid { get; set; }
        public DateTime fromdate { get; set; }
        public DateTime todate { get; set; }
    }
}
