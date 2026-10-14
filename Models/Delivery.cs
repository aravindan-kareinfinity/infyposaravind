using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class Delivery : BaseEntity
    {
        public long id { get; set; }
        public bool isactive { get; set; }
        public long createdby { get; set; }
        public long modifiedby { get; set; }
        public int version { get; set; }
        public DateTime createdon { get; set; }
        public DateTime modifiedon { get; set; }
        public long billid { get; set; }
        public bool isgift { get; set; }
        public bool isgiftcoupon { get; set; }
        public long locationid { get; set; }
        public long counterid { get; set; }
        public decimal receivable { get; set; }
        public decimal amount { get; set; }
        public decimal excluded { get; set; }
        public string billno { get; set; }
        public DateTime billdate { get; set; }
        public DateTime settlementon { get; set; }
        public string settlementcode { get; set; }
        public string remarks { get; set; }
        public long receivedby { get; set; }
        public long deliveredby { get; set; }
        public long validatedby { get; set; }
        public string validatedbyname { get; set; }
        public long returnby { get; set; }
        public long returnto { get; set; }
        public long cancelledby { get; set; }
        public int qty { get; set; }
        public DateTime receivedon { get; set; }
        public DateTime deliveredon { get; set; }
        public int status { get; set; }
        [JsonIgnore]
        public DeliveryStatus Deliverystatus
        {
            get
            {
                return (DeliveryStatus)status;
            }
            set
            {
                status = (int)value;
            }
        }

        public string deliverystatus
        {
            get
            {
                return ((DeliveryStatus)status).ToString();
            }
        }
        public string altationno { get; set; }
        public string customername { get; set; }
        public string contactno { get; set; }
        public long customerid { get; set; }
        public long companyid { get; set; }
        public bool isalteration { get; set; }
        [JsonIgnore]
        public string orderno { get; set; }
        [JsonIgnore]
        public string entryno { get; set; }
        [JsonIgnore]
        public DateTime expectedon { get; set; }

        public class Voucherinfo
        {
            public DateTime validfrom { get; set; }
            public DateTime expiry { get; set; }
            public int expirydays { get; set; }
            public int fromdays { get; set; }
            public decimal amount { get; set; }
            public string couponcode { get; set; }
        }
        [JsonIgnore]
        public bool HasVoucher
        {
            get
            {
                return voucherinfo != null && voucherinfo.amount > 0;
            }
        }

        public List<GiftItem> giftlist { get; set; }
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string giftlist_json
        {
            get { return Newtonsoft.Json.JsonConvert.SerializeObject(giftlist); }
            set
            {
                if (!string.IsNullOrEmpty(value) && value != "null")
                    giftlist = Newtonsoft.Json.JsonConvert.DeserializeObject<List<GiftItem>>(value);
            }
        }

        public Voucherinfo voucherinfo { get; set; }
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string voucherinfo_json
        {
            get { return Newtonsoft.Json.JsonConvert.SerializeObject(voucherinfo); }
            set
            {
                if (!string.IsNullOrEmpty(value) && value != "null")
                    voucherinfo = Newtonsoft.Json.JsonConvert.DeserializeObject<Voucherinfo>(value);
            }
        }
        public ReceivedItem billinfo { get; set; }
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string billinfo_json
        {
            get { return Newtonsoft.Json.JsonConvert.SerializeObject(billinfo); }
            set
            {
                if (!string.IsNullOrEmpty(value) && value != "null")
                    billinfo = Newtonsoft.Json.JsonConvert.DeserializeObject<ReceivedItem>(value);
            }
        }
        [JsonIgnore]
        public string countername { get; set; }
        [JsonIgnore]
        public string cashiername { get; set; }
        public bool settled { get; set; }
        public bool candeliver { get; set; }
        public bool received { get; set; }
        public int receivedcount { get; set; }
        public bool notreceived { get; set; }

        [Newtonsoft.Json.JsonIgnore]
        public Company company { get; set; }
        [Newtonsoft.Json.JsonIgnore]
        public CompanyLocation location { get; set; }
        [Newtonsoft.Json.JsonIgnore]
        public Customer customer { get; set; }
        [JsonIgnore]
        public decimal alterationamount
        {
            get
            {
                if (billinfo != null) return billinfo.amount;
                return 0;
            }
        }
        [JsonIgnore]
        public decimal alterationqty
        {
            get
            {
                if (billinfo != null) return billinfo.qty;
                return 0;
            }
        }
        public string printdocument { get; set; }
        public List<DeliveryGift> giftitems { get; set; }
        public List<ReceivedItem> bills { get; set; }
        [JsonIgnore]
        public List<SettlementItem> setlist { get; set; }
        public string printbarcode { get; set; }

        public class SettlementItem
        {
            public string code { get; set; }
            public DateTime date { get; set; }
            public decimal amount { get; set; }
            public int sno { get; set; }
        }
        public enum DeliveryStatus
        {
            Received,
            Validated,
            Delivered,
            Returned,
            Cancelled,
            NotReceived,
            StockMarker
        }
        public class GiftItem
        {
            public long stockmarkerid { get; set; }
            public long salablegoodsid { get; set; }
            public string barcode { get; set; }
            public int qty { get; set; }
            public decimal value { get; set; }
            public bool issued { get; set; }
            public string giftmarkername { get; set; }
        }
        public class ReceivedItem
        {
            public ReceivedItem()
            {

            }
            public ReceivedItem(Bill bill, bool includedesign)
            {
                this.billno = bill.billno;
                this.qty = bill.qty;
                this.billid = bill.id;
                this.amount = bill.receivable+bill.additionalcharges+bill.rounding;
                this.items = new List<Item>();
                foreach (var item in bill.Billitems)
                {
                    var display = item.printingname;
                    if (includedesign)
                    {
                        display = item.attributes.designid + "/" + item.attributes.colourname + "/" + item.attributes.size;
                    }
                    this.items.Add(new Item()
                    {
                        printingname = display,
                        price = item.price,
                        piece = item.piece,
                        qty = item.qty,
                        receivable = item.receivable,
                        designid = item.attributes == null ? "" : item.attributes.designid,
                        barcode = item.attributes != null &&
                        !string.IsNullOrEmpty(item.attributes.serialno)
                        && includedesign ? item.attributes.serialno : item.barcode
                    }); 

                    if (item.schemeid > 0 && item.attributes != null && item.attributes.freeitems != null)
                    {
                        foreach (var gitem in item.attributes.freeitems)
                        {
                            this.items.Add(new Item()
                            {
                                amount = gitem.amount,
                                qty = gitem.qty,
                                giftmarkerid = gitem.markerid,
                                freegift = true
                            });
                        }
                    }

                }
            }
            public DateTime settlementon { get; set; }
            public string settlementcode { get; set; }
            public string billno { get; set; }
            public ReceivedItem billinfo { get; set; }
            public decimal qty { get; set; }
            public long billid { get; set; }
            public decimal excluded { get; set; }
            public decimal amount { get; set; }
            public decimal chargeamount { get; set; }
            [JsonIgnore]
            public List<Item> GiftItems
            {
                get
                {
                    if (items == null) return new List<Item>();
                    return items.FindAll(x => !string.IsNullOrEmpty(x.barcode));
                }
            }
            [JsonIgnore]
            public List<Item> Bills
            {
                get
                {
                    if (items == null) return new List<Item>();
                    return items.FindAll(x => string.IsNullOrEmpty(x.barcode));
                }
            }
            [JsonIgnore]
            public decimal GiftValue
            {
                get
                {
                    return (items != null ? items.Sum(x => x.amount) : 0);
                }
            }
            [JsonIgnore]
            public decimal BillValue
            {
                get
                {
                    return Bills.Sum(x => x.receivable);
                }
            }
            public List<Item> items { get; set; }
            public class Item
            {
                [JsonIgnore]
                public int sno { get; set; }
                public DateTime settlementon { get; set; }
                public string settlementcode { get; set; }
                public string billno { get; set; }
                public string barcode { get; set; }
                public string chargename { get; set; }
                public string printingname { get; set; }
                public string designid { get; set; }
                public decimal price { get; set; }
                public decimal piece { get; set; }
                public decimal receivable { get; set; }
                public decimal amount { get; set; }
                public decimal excluded { get; set; }
                public decimal qty { get; set; }
                public decimal rqty { get; set; }
                public decimal chargerate { get; set; }
                public decimal chargeamount { get; set; }
                public DeliveryStatus status { get; set; }
                public long tailoringid { get; set; }
                public long giftmarkerid { get; set; }
                public bool freegift { get; set; }
                public string notes { get; set; }
                public long billid { get; set; }
                public long salablegoodsid { get; set; }
            }
        }
    }

    public class DeliveryGift : BaseEntity
    {
        public long stockmarkerid { get; set; }
        public long stockid { get; set; }
        public long productid { get; set; }
        public long salestaxid { get; set; }
        public decimal taxpercentage { get; set; }
        public long locationid { get; set; }
        public long salablegoodsid { get; set; }
        public decimal value { get; set; }
        public int qty { get; set; }
        public string barcode { get; set; }
        public string name { get; set; }
        public int status { get; set; }
        [JsonIgnore]
        public Delivery.DeliveryStatus Deliverystatus
        {
            get
            {
                return (Delivery.DeliveryStatus)status;
            }
            set
            {
                status = (int)value;
            }
        }
        public bool Success { get; set; }
        public string deliverystatus
        {
            get
            {
                return ((Delivery.DeliveryStatus)status).ToString();
            }
        }
    }
    public class DeliveryCreteria : Delivery
    {
        public string settlementcode { get; set; }
        public bool isgiftcoupon { get; set; }
        public bool searchbill { get; set; }
        public List<long> billids { get; set; }
        public List<long> stockmarkersids { get; set; }
        public bool isgiftissue { get; set; }
        public bool isdelivery { get; set; }
    }

    public class DelivaryGiftBarcode : InfyPOS.Report.IBarcodeFormat
    {
        public bool SpecialField(string field, SortedDictionary<string, Object> data)
        {
            return false;
        }
        public DelivaryGiftBarcode(Delivery odelivery)
        {
            this.customername = odelivery.customer.name;
            this.mobileno = odelivery.customer.mobileno;
            this.amount = odelivery.voucherinfo.amount;
            this.settlementcode = odelivery.billinfo != null 
                && !string.IsNullOrEmpty(odelivery.billinfo.settlementcode) ? odelivery.billinfo.settlementcode :
                odelivery.billinfo != null && odelivery.bills != null && odelivery.bills.Count > 0 ? odelivery.bills[0].settlementcode : "";
            this.issuedon= odelivery.receivedon;
            this.validity = odelivery.voucherinfo.expiry;
            this.validfrom = odelivery.voucherinfo.validfrom;
            this.code = odelivery.voucherinfo.couponcode;
            this.words = WebAPI.Data.Numberconvertor.rupees(amount);
            this.word1 = "";
            this.word2 = "";
            foreach (var word in this.words.Split(' '))
            {
                if (word1.Length > 40)
                {
                    word2 += (word2.Length > 0 ? " " : "") + word;
                }
                else
                {
                    word1 += (word1.Length > 0 ? " " : "") + word;
                }
            }
        }
        public string words { get; set; }
        public string code { get; set; }
        public string customername { get; set; }
        public string mobileno { get; set; }
        public decimal amount { get; set; }
        public string settlementcode { get; set; }
        public DateTime validfrom { get; set; }
        public DateTime validity { get; set; }
        public DateTime issuedon { get; set; }
        public string word1 { get; set; }
        public string word2 { get; set; }
        public SortedDictionary<string, object> KeyValue
        {
            get
            {
                SortedDictionary<string, object> ret = new SortedDictionary<string, object>();
                ret.Add("CODE", code);
                ret.Add("BARCODE", code);
                ret.Add("CUSTOMERNAME", customername);
                ret.Add("SETTLEMENTCODE", settlementcode);
                ret.Add("MOBILENO", mobileno);
                ret.Add("AMOUNT", amount);
                ret.Add("ISSUEDON", issuedon);
                ret.Add("VALIDFROM", validfrom);
                ret.Add("VALIDTO", validity);
                ret.Add("VALIDITY", validity);
                ret.Add("WORDS-1", word1);
                ret.Add("WORDS-2", word2);
                ret.Add("WORDS", words);
                return ret;
            }
        }
        public SortedDictionary<string, string> KeyFormat
        {
            get
            {
                SortedDictionary<string, string> ret = new SortedDictionary<string, string>();
                ret.Add("ISSUEDON", "dd-MM-yyyy");
                ret.Add("VALIDFROM", "dd-MM-yyyy");
                ret.Add("VALIDITY", "dd-MM-yyyy");
                ret.Add("VALIDTO", "dd-MM-yyyy");
                ret.Add("AMOUNT", "F0");
                return ret;
            }
        }
    }
    public class DeliverySearch : BaseEntity
    {
        public int mode { get; set; }
        public bool isb2b { get; set; }
        public bool multicompany { get; set; }
        public DateTime fromdate { get; set; }
        public DateTime todate { get; set; }
        public long locationid { get; set; }
        public long counterid { get; set; }
        public long companyid { get; set; }
        public string billno { get; set; }
        public string voucherno { get; set; }
    }

    public class DeliveryGiftSummary
    {
        public DateTime receivedon { get; set; }
        public string categoryname { get; set; }
        public string barcode { get; set; }
        public decimal voucherqty { get; set; }
        public decimal issueqty { get; set; }
        public decimal giftqty { get; set; }
        public string giftproduct { get; set; }
    }
    public class DelieryGiftItem
    {
        public string voucherno { get; set; }
        public decimal billamount { get; set; }
        public string billno { get; set; }
        public string barcode { get; set; }
        public string settlementcode { get; set; }
        public string categoryname { get; set; }
        public string giftproduct { get; set; }
        public string counter { get; set; }
        public decimal giftvalue { get; set; }
        public decimal giftqty { get; set; }
        public bool issued { get; set; }
        public DateTime deliveredon { get; set; }
        public DateTime receivedon { get; set; }
        public DateTime settlementon { get; set; }
        public string voucheredby { get; set; }
        public string issuedby { get; set; }
    }

}
