using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class CustomerorderInfo : BaseEntity
    {
        public string ordertakenby { get; set; }
        public long id { get; set; }
        public long createdby { get; set; }
        public long modifiedby { get; set; }
        public long companyid { get; set; }
        public long locationid { get; set; }
        public long customerid { get; set; }
        public long categoryid { get; set; }
        public long prfid { get; set; }
        public int version { get; set; }
        public int status { get; set; }
        public int modeofdelivery { get; set; }
        public bool isactive { get; set; }
        public DateTime createdon { get; set; }
        public DateTime modifiedon { get; set; }
        public DateTime orderdate { get; set; }
        public DateTime planneddeliverydate { get; set; }
        public DateTime deliverydate { get; set; }
        public decimal orderamount { get; set; }
        public decimal paidamount { get; set; }
        public decimal totalqty { get; set; }
        public string orderamountinwords
        {
            get
            {
                return WebAPI.Data.Numberconvertor.rupees(orderamount);
            }
        }
        public string paidamountinwords
        {
            get
            {
                return WebAPI.Data.Numberconvertor.rupees(paidamount);
            }
        }
        public decimal balanceamount
        {
            get
            {
                return orderamount - paidamount;
            }
        }
        public bool HasCommunication
        {
            get
            {
                return communication != null && communication.Count > 0;
            }
        }
        public bool HasOrderItems
        {
            get
            {
                return orderitems != null && orderitems.Count > 0;
            }
        }
        public bool HasStatusHistory
        {
            get
            {
                return statushistory != null && statushistory.Count > 0;
            }
        }
        public long ledgerid { get; set; }
        public string customerorderno { get; set; }
        public string notes { get; set; }

        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string delieryinfo_json { get { return Newtonsoft.Json.JsonConvert.SerializeObject(delieryinfo); } set { delieryinfo = Newtonsoft.Json.JsonConvert.DeserializeObject<Delieryinfo>(value); } }
        public Delieryinfo delieryinfo { get; set; }
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string orderitems_json
        {
            get { return Newtonsoft.Json.JsonConvert.SerializeObject(orderitems); }
            set
            {
                if (value != null)
                    orderitems = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Orderitems>>(value);

                if (orderitems == null)
                    orderitems = new List<Orderitems>();
            }
        }
        public List<Orderitems> orderitems { get; set; }
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string statushistory_json { get { return Newtonsoft.Json.JsonConvert.SerializeObject(statushistory); } set { statushistory = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Statushistory>>(value); } }
        public List<Statushistory> statushistory { get; set; }
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string communication_json { get { return Newtonsoft.Json.JsonConvert.SerializeObject(communication); } set { communication = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Communication>>(value); } }
        public List<Communication> communication { get; set; }
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string paymentinfo_json { get { return Newtonsoft.Json.JsonConvert.SerializeObject(paymentinfo); } set { paymentinfo = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Paymentinfo>>(value); } }
        public List<Paymentinfo> paymentinfo { get; set; }
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string venderinfo_json { get { return Newtonsoft.Json.JsonConvert.SerializeObject(venderinfo); } set { venderinfo = Newtonsoft.Json.JsonConvert.DeserializeObject<Venderinfo>(value); } }
        public Venderinfo venderinfo { get; set; }
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string voucherinfo_json
        {
            get { return Newtonsoft.Json.JsonConvert.SerializeObject(voucherinfo); }
            set
            {
                if (value == null)
                    voucherinfo = null;
                else
                    voucherinfo = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Voucherinfo>>(value);
            }
        }
        public List<Voucherinfo> voucherinfo { get; set; }

        public class Voucherinfo
        {
            public long couponid { get; set; }
            public string code { get; set; }
            public decimal amount { get; set; }
            public DateTime validupto { get; set; }
        }
        public class Delieryinfo
        {
            public string receivedby { get; set; }
            public string courier { get; set; }
            public string transport { get; set; }
            public string awbno { get; set; }
            public string lrno { get; set; }
            public string couriercopy { get; set; }
        }
        public class Orderitems
        {
            public long itemid { get; set; }
            public long supplierid { get; set; }
            public long productid { get; set; }
            public long styleid { get; set; }
            public long brandid { get; set; }
            public long sizeid { get; set; }
            public long colorid { get; set; }
            public string designno { get; set; }
            public long patternid { get; set; }
            public string product { get; set; }
            public string description { get; set; }
            public string notes { get; set; }
            public int status { get; set; }
            public decimal price { get; set; }
            public decimal qty { get; set; }
            public decimal amount
            {
                get
                {
                    return price * qty;
                }
            }
        }
        public class Statushistory
        {
            public int status { get; set; }
            public DateTime actionon { get; set; }
            public string changedby { get; set; }
        }
        public class Communication
        {
            public bool iscustomer { get; set; }
            public DateTime actionon { get; set; }
            public string contactno { get; set; }
            public string spokenwith { get; set; }
            public string notes { get; set; }
        }
        public class Paymentinfo
        {
            public DateTime actionon { get; set; }
            public decimal amount { get; set; }
            public string notes { get; set; }
            public int mode { get; set; }
            public int customerpaymentmode
            {
                get
                {
                    switch (mode)
                    {
                        case 1:
                            return 4;
                            break;
                        case 2:
                            return 4;
							break;
                        case 3:
                            return 2;
                            break;
                        case 4:
                            return 0;
                            break;
                        case 5:
                            return 3;
                            break;

                    }
                    return 4;
                }
            }



            public long locationid { get; set; }
            public long counterid { get; set; }
            public string countername { get; set; }
            public long ledgerid { get; set; }
            public long createdby { get; set; }
            public string employeename { get; set; }
            public long receivedby { get; set; }
            public bool receiptgenerated { get; set; }
            public long bankid { get; set; }
            public DateTime chequedate { get; set; }
            public string referencedetail { get; set; }
            public string remarks { get; set; }
            [JsonIgnore]
            public string paymentmode
            {
                get
                {
                    switch (mode)
                    {
                        case 0:
                            return "CASH";
                        case 1:
                            return "VOUCHER";
                        case 2:
                            return "CASH";
                        case 3:
                            return "UPI";
                    }
                    return "CHEQUE";
                }
            }
            public string uniqueid { get; set; }
            public long paymentid { get; set; }
            public long id { get; set; }
        }
        public class Venderinfo
        {
            public DateTime actionon { get; set; }
            public string person { get; set; }
            public long supplierid { get; set; }
            public string orderno { get; set; }
            public DateTime commitedon { get; set; }
        }
        public string name { get; set; }
        public long cityid { get; set; }
        public string address { get; set; }
        public string contactno { get; set; }

    }
    public class Customerorder : CustomerorderInfo
    {
        [JsonIgnore]
        public Company company { get; set; }
        [JsonIgnore]
        public CompanyLocation location { get; set; }
        [JsonIgnore]
        public Customer customer { get; set; }
        public string customermobileno { get; set; }
        public string customername { get; set; }
        public string printdocument { get; set; }
    }
    public class CustomerorderCreteria : CustomerorderInfo
    {
        public DateTime fromdate { get; set; }
        public DateTime todate { get; set; }
    }

    public enum CustomerOrderStatus
    {

    }
}
