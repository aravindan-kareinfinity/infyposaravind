using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class Tax : BaseEntity
    {
        public string code { get; set; }
        public string name { get; set; }
        public int type { get; set; }
        public bool issalestax { get; set; }
        public bool ispurchasetax { get; set; }
        public bool isactive { get; set; }
        public long createdby { get; set; }
        public long modifiedby { get; set; }
        public int version { get; set; }
        public long id { get; set; }
        public DateTime createdon { get; set; }
        public DateTime modifiedon { get; set; }
        public decimal taxpercentage { get; set; }
        public string taxlist { get; set; }
        public int taxorder { get; set; }
        public bool disable { get; set; }

        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string taxsplit_json { get { return Newtonsoft.Json.JsonConvert.SerializeObject(taxsplit); } set { taxsplit = Newtonsoft.Json.JsonConvert.DeserializeObject<TaxSplit>(value); } }
        public TaxSplit taxsplit { get; set; }
        public class TaxSplit
        {
            public TaxSplit()
            {
                items = new List<Item>();
            }
            public List<Item> items { get; set; }
            public decimal avoidrangefrom { get; set; }
            public decimal avoidrangeto { get; set; }
            public Item Find(decimal amount)
            {
                return Find(false, amount);
            }
            public Item Find(bool incost,decimal amount)
            {
                if (items.Count == 1) return items[0];
                int index = items.FindIndex(e => e.incost == incost && e.fromamount <= amount && e.toamount >= amount);
                if (index >= 0)
                    return items[index];
                index = items.FindIndex(e => e.incost == false && e.fromamount <= amount && e.toamount >= amount);
                if (index >= 0)
                    return items[index];
                if (items.Count > 0) return items[0];
                return new Item();
            }
            public class Item
            {
                public bool incost { get; set; }
                public long sourceid { get; set; }
                public decimal fromamount { get; set; }
                public decimal toamount { get; set; }
                public decimal cgst { get; set; }
                public decimal sgst { get; set; }
                public decimal igst { get; set; }
                public decimal taxpercent { get
                    {
                        return cgst + cgst;
                    }
                }
            }
        }

        public bool hascess { get; set; }
        public decimal cesspercentage { get; set; }
        public string employeename { get; set; }
    }
    public class TaxCreteria : Tax
    {
        public DateTime fromdate { get; set; }
        public DateTime todate { get; set; }
        public List<long> ids { get; set; }
    }

    public class MergeTaxList : BaseEntity
    {
        public long id { get; set; }
        public List<long> deletelist { get; set; }
        public string ErrorMessage { get; set; }

    }

}
