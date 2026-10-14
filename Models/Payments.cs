using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class Payments : BaseEntity
    {
        public long id { get; set; }
        public long curstomerid { get; set; }
        public long employeeid { get; set; }
        public DateTime paymenton { get; set; }
        public decimal paidamount { get; set; }
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string paymentinfo_json { get { return Newtonsoft.Json.JsonConvert.SerializeObject(paymentinfo); } set { paymentinfo = Newtonsoft.Json.JsonConvert.DeserializeObject<Paymentinfo>(value); } }
        public Paymentinfo paymentinfo { get; set; }
        public int paymentmode { get; set; }
        public bool isactive { get; set; }
        public long createdby { get; set; }
        public long modifiedby { get; set; }
        public int version { get; set; }
        public DateTime createdon { get; set; }
        public DateTime modifiedon { get; set; }
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string billinformation_json { get { return Newtonsoft.Json.JsonConvert.SerializeObject(billinformation); } set { billinformation = Newtonsoft.Json.JsonConvert.DeserializeObject<Billinformation>(value); } }
        public Billinformation billinformation { get; set; }
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string emiinformation_json { get { return Newtonsoft.Json.JsonConvert.SerializeObject(emiinformation); } set { emiinformation = Newtonsoft.Json.JsonConvert.DeserializeObject<Emiinformation>(value); } }
        public Emiinformation emiinformation { get; set; }
        public long emiapprovedby { get; set; }
        public string notes { get; set; }
        public string code { get; set; }
        public class Paymentinfo
        {
            public decimal cash_paid { get; set; }
            public string cash_dinamination { get; set; }
            public decimal cash_return { get; set; }
            public decimal card_paid { get; set; }
            public string card_no { get; set; }
            public long credit_approvedBy { get; set; }
            public decimal credit_paid { get; set; }
            public string credit_info { get; set; }
            public decimal voucher_paid { get; set; }
            public decimal loyalty_paid { get; set; }
            public voucherinfo voucherinformation { get; set; }
        }
        public class voucherinfo
        {
            public long id { get; set; }
            public string code { get; set; }
            public int type { get; set; }
            public decimal amount { get; set; }
        }
        public class Billinformation
        {
            public long billid { get; set; }
            public decimal billamount { get; set; }
            public string billno { get; set; }
            public DateTime billdate { get; set; }
            public decimal paidamount { get; set; }
        }
        public class Emiinformation
        {
            public int noofmonths { get; set; }
            public int frommonth { get; set; }
            public int fromyear { get; set; }
            public decimal emiamount { get; set; }
            public List<EMI> paymentschedule { get; set; }
            public class EMI
            {
                public int month { get; set; }
                public int year { get; set; }
                public decimal amount { get; set; }
            }
        }
    }
    public class PaymentsCreteria : Payments
    {
    }
}
