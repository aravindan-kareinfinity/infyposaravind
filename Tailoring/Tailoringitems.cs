using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace InfyPOS.Models
{
    public class Tailoringitems : BaseEntity
    {
        public long id { get; set; }
        public long tailoringorderid { get; set; }
        public long sourceproductid { get; set; }
        public long tailorid { get; set; }
        public long outcomeproductid { get; set; }
        public decimal sourceqty { get; set; }
        public decimal sourcemeter { get; set; }
        public decimal expectedqty { get; set; }
        public decimal receivedqty { get; set; }
        public decimal issuedqty { get; set; }
        public decimal returnqty { get; set; }
        public decimal deliveredqty { get; set; }
        public long chargeid { get; set; }
        public decimal charge { get; set; }
        public decimal gross { get; set; }
        public decimal tax { get; set; }
        public decimal total { get; set; }

        public long additionalchargeid { get; set; }
        public decimal additionalchargeamount { get; set; }

        public decimal delayperday { get; set; }
        public decimal delaymaximum { get; set; }
        public decimal chargeonpriority { get; set; }
        public decimal delaycharge { get; set; }
        public decimal prioritycharge { get; set; }
        public int priorityqty { get; set; }
        public string designname { get; set; }
        public long designid { get; set; }
        public string product { get; set; }
        public string measurementname { get; set; }
        public long measurementid { get; set; }
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string measurement_json { get { return Newtonsoft.Json.JsonConvert.SerializeObject(measurement); } set { measurement = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Measurement>>(value); } }
        public List<Measurement> measurement { get; set; }

        public string measurementtext
        {
            get
            {
                if (measurement == null) return "";
                return string.Join(",", measurement.ConvertAll(x => string.Format("{0}-{1}", x.key, x.value)));
            }
        }
        public DateTime createdon { get; set; }
        public int version { get; set; }
        public bool isactive { get; set; }
        public long modifiedby { get; set; }
        public long createdby { get; set; }
        public DateTime modifiedon { get; set; }
        public class Measurement
        {
            public string key { get; set; }
            public string value { get; set; }
        }

        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string barcodes_json { get { return Newtonsoft.Json.JsonConvert.SerializeObject(barcodes); } set { barcodes = Newtonsoft.Json.JsonConvert.DeserializeObject<List<BarCode>>(value); } }
        public List<BarCode> barcodes { get; set; }
        public class BarCode
        {
            public string barcode { get; set; }
            public decimal meter { get; set; }
            public int pcs { get; set; }
        }
    }
    public class TailoringitemsTransaction : Tailoringitems
    {
        public decimal splitqty { get; set; }
        public decimal rqty { get; set; }
        public decimal dqty { get; set; }
        public List<long> allids { get; set; }
        public long deliveredby { get; set; }
        public string receivedby { get; set; }
        public bool receivedbycustomer { get; set; }
    }

    public class TailoringitemInbox : Tailoringitems
    {
        public decimal amount { get; set; }
        public decimal paid { get; set; }
        public decimal balance { get; set; }
        public string tailorname { get; set; }
        public DateTime orderdate { get; set; }

        public DateTime expectedon { get; set; }

        public DateTime deliveredon { get; set; }

        public string billno { get; set; }
        public string entryno { get; set; }
        public DateTime paidon { get; set; }
        public string customername { get; set; }
        public string customermobile { get; set; }
        public string notes { get; set; }
        public decimal additionalcharge { get;  set; }
        public string orderno { get; set; }
    }

    public class TailoringPayment : BaseEntity
    {
        public long paymode { get; set; }
        public decimal amount { get; set; }
        public DateTime date { get; set; }
        public long locationid { get; set; }
        public long companyid { get; set; }
        public Tailor[] items { get; set; }
        public List<TailoringItem> GetAllItems()
        {
            List<TailoringItem> tailoringItems = new List<TailoringItem>();
            foreach (var itm in items)
            {
                tailoringItems.AddRange(itm.TailoringItems.ToList().ConvertAll(x => new TailoringItem()
                {
                    tailorid = itm.tailorid,
                    id = x.id,
                    amount = x.amount,
                    newpaid = x.newpaid
                }));
            }
            return tailoringItems;
        }
        public partial class Tailor
        {
            public decimal balance { get; set; }
            public long tailorid { get; set; }
            public decimal newpaid { get; set; }
            public TailoringItem[] TailoringItems { get; set; }            
        }

        public partial class TailoringItem
        {
            public long tailorid { get; set; }  
            public long id { get; set; }
            public decimal amount { get; set; }
            public decimal newpaid { get; set; }
        }

    }

    public class TailoringitemPayments : BaseEntity
    {
        public string tailorid { get; set; }
        public long Tailoringitemid { get; set; }
        public decimal amount { get; set; }
        public decimal paid { get; set; }
        public decimal balance { get; set; }
        public DateTime paidon { get; set; }
        public string design { get; set; }
        public string product { get; set; }
        public string tailorname { get; set; }
        public DateTime orderdate { get; set; }
        public string entryno { get; set; }
        public decimal charge { get; set; }
        public decimal deliveredqty { get; set; }
        public string measurementname { get; set; }
        public string designname { get; set; }



    }
}
