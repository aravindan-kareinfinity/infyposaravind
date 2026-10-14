using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class Cash : BaseEntity
    {
        public string ErrorMessage { get; set; }
        public long id { get; set; }
        public bool isactive { get; set; }
        public int status { get; set; }
        public long createdby { get; set; }
        public long modifiedby { get; set; }
        public int version { get; set; }
        public DateTime createdon { get; set; }
        public DateTime modifiedon { get; set; }
        public long cashierid { get; set; }
        public long paidbyemployeeid { get; set; }
        public long receivedbyemployeid { get; set; }
        public DateTime paidon { get; set; }
        public DateTime returnedon { get; set; }
        public string billno { get; set; }
        public string notes { get; set; }
        public long counterid { get; set; }
        public long locationid { get; set; }
        public decimal expenses { get; set; }
        public decimal openingamount { get; set; }
        public decimal closingamount { get; set; }
        public decimal takenamount { get; set; }
        public decimal cardamount { get; set; }
        public decimal upiamount { get; set; }
        public decimal balanceamount { get; set; }
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string openingdinomination_json
        {
            get { return Newtonsoft.Json.JsonConvert.SerializeObject(openingdinomination); }
            set
            {
                if (value != null)
                {
                    try
                    {
                        openingdinomination = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Dinomination>>(value);
                    }
                    catch (Exception exp)
                    {

                    }
                }
            }
        }
        public List<Dinomination> openingdinomination { get; set; }
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string closingdinomination_json
        {
            get { return Newtonsoft.Json.JsonConvert.SerializeObject(closingdinomination); }
            set
            {
                if (value != null)
                {
                    try
                    {
                        closingdinomination = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Dinomination>>(value);
                    }
                    catch (Exception exp)
                    {

                    }
                }
            }
        }
        public List<Dinomination> closingdinomination { get; set; }


        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string takendinomination_json
        {
            get { return Newtonsoft.Json.JsonConvert.SerializeObject(takendinomination); }
            set
            {
                if (value != null)
                {
                    try
                    {
                        takendinomination = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Dinomination>>(value);
                    }
                    catch (Exception exp)
                    {

                    }
                }
            }
        }
        public List<Dinomination> takendinomination { get; set; }

        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string balancedinomination_json
        {
            get { return Newtonsoft.Json.JsonConvert.SerializeObject(balancedinomination); }
            set
            {
                if (value != null)
                {
                    try
                    {
                        balancedinomination = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Dinomination>>(value);
                    }
                    catch (Exception exp)
                    {

                    }
                }
            }
        }
        public List<Dinomination> balancedinomination { get; set; }
        public List<BankSettlement> banktransactions { get; set; }

    }

    public class CashPosting : BaseEntity
    {
        public long locationid { get; set; }
        public DateTime paidon { get; set; }
        public List<long> closings { get; set; }

        public int upicount { get; set; }
        public int bankcount { get; set; }
        public decimal upi { get; set; }
        public decimal cash { get; set; }
        public decimal bank { get; set; }
        public decimal total
        {
            get
            {
                return cash + upi + bank;
            }
        }
    }

    public class CashPostingResponse : BaseEntity
    {
        public long locationid { get; set; }
        public DateTime paidon { get; set; }
        public List<Cash> closings { get; set; }
    }
    public class CashCreteria : Cash
    {
        public List<long> ids { get; set; }
    }
    public class Dinomination
    {
        public int dinomination { get; set; }
        public int qty { get; set; }
        public decimal amount { get; set; }
    }
        
    public class BankSettlement
    {
        public string type { get; set; }
        public long bankid { get; set; }
        public decimal amount { get; set; }
    }   
}
