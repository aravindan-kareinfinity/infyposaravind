using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class Stockdumping : BaseEntity
    {
        public string dumpcode { get; set; }
        public long id { get; set; }
        public DateTime createdon { get; set; }
        public DateTime modifiedon { get; set; }
        public long createdby { get; set; }
        public long modifiedby { get; set; }
        public int version { get; set; }
        public long companyid { get; set; }
        public long locationid { get; set; }
        public long employeeid { get; set; }
        public string employeename { get; set; }
        public long approvedby { get; set; }
        public string createdbyemp { get; set; }
        public string approvedbyemp { get; set; }
        public bool isactive { get; set; }
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string itemlist_json { get { return Newtonsoft.Json.JsonConvert.SerializeObject(itemlist); } set { itemlist = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ItemDetail>>(value); } }
        public List<ItemDetail> itemlist { get; set; }
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string updatelist_json { get { return Newtonsoft.Json.JsonConvert.SerializeObject(updatelist); } set { updatelist = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ItemDetail>>(value); } }
        public List<ItemDetail> updatelist { get; set; }
        public long dumpgroupid { get; set; }
        public DateTime dumpon { get; set; }
        [JsonIgnore]
        public Company company { get; set; }

        internal void UpdateInternalList()
        {
            this.updatelist = this.itemlist.FindAll(x => x.stocklist.Count == 1).ConvertAll(y => new Stockdumping.ItemDetail()
            {
                salablegoodsid = y.salablegoodsid,
                dqty = y.dqty
            });
            this.updatelist.ForEach(x => x.stocklist = null);

            foreach (var stkitm in this.itemlist.FindAll(x => x.stocklist.Count > 1))
            {
                var avilable = stkitm.dqty;
                var positivelist = stkitm.stocklist.FindAll(x => x.qty > 0).OrderBy(x => x.salablegoodsid).ToList();
                for (var i = 0; i < positivelist.Count && avilable > 0; i++)
                {
                    if (avilable >= positivelist[i].qty)
                    {
                        avilable -= positivelist[i].qty;
                        this.updatelist.Add(new Stockdumping.ItemDetail()
                        {
                            salablegoodsid = positivelist[i].salablegoodsid,
                            dqty = positivelist[i].qty
                        });
                    }
                    else
                    {
                        this.updatelist.Add(new Stockdumping.ItemDetail()
                        {
                            salablegoodsid = positivelist[i].salablegoodsid,
                            dqty = avilable
                        });
                        avilable = 0;
                    }
                }

                if (avilable > 0 && stkitm.stocklist.Exists(x => x.qty <= 0))
                {
                    var negativelist = stkitm.stocklist.FindAll(x => x.qty <= 0).OrderByDescending(x => x.salablegoodsid).ToList();
                    this.updatelist.Add(new Stockdumping.ItemDetail()
                    {
                        salablegoodsid = negativelist[0].salablegoodsid,
                        dqty = avilable
                    });
                    avilable = 0;
                }

                if (avilable > 0)
                {
                    this.updatelist.Add(new Stockdumping.ItemDetail()
                    {
                        salablegoodsid = stkitm.salablegoodsid,
                        dqty = avilable
                    });
                    avilable = 0;
                }
            }
        }

        public class ItemDetail
        {
            public List<ItemDetail> stocklist { get; set; }
            public long salablegoodsid { get; set; }
            public string barcode { get; set; }
            public decimal qty { get; set; }
            public decimal dqty { get; set; }
            public decimal price { get; set; }  
            public long productid { get; set; }
            public long itemid { get; set; }
            public string productname { get; set; }
            public decimal costprice { get; set; }
            public decimal amount
            {
                get
                {
                    return dqty * costprice;
                }
            }
            public string itemcode { get; set; }
        }
    }
    public class StockdumpingCreteria : Stockdumping
    {
        public bool onlynegative { get; set; }
        public string barcode { get; set; }
        public long productid { get; set; }
        public long itemid { get; set; }
        public long supplierid { get; set; }
        public long brandid { get; set; }
        public DateTime fromdate { get; set; }
        public DateTime todate { get; set; }
    }
}
