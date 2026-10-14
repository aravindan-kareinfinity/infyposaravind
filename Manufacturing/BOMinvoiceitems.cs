using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class Bominvoiceitems : BaseEntity
    {
        public long id { get; set; }
        public decimal sno { get; set; }
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string poinfo_json { get { return Newtonsoft.Json.JsonConvert.SerializeObject(poinfo); } set { poinfo = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Poinfo>>(value); } }
        public List<Poinfo> poinfo { get; set; }
        public long bomid { get; set; }
        public decimal buyingqty { get; set; }
        public decimal buyingprice { get; set; }
        public decimal purchasetotaldiscount { get; set; }
        public decimal amount { get; set; }
        public bool isactive { get; set; }
        public long createdby { get; set; }
        public long modifiedby { get; set; }
        public int version { get; set; }
        public DateTime createdon { get; set; }
        public DateTime modifiedon { get; set; }
        public decimal damagedqty { get; set; }
        public decimal sellableqty { get; set; }
        public long taxid { get; set; }
        public long dcid { get; set; }
        public long dcitemid { get; set; }
        public decimal returnqty { get; set; }
        public decimal basebuyingprice { get; set; }
        public string hsncode { get; set; }
        public decimal purchasediscount { get; set; }
        public decimal purchasediscountpercentage { get; set; }
        public decimal purchasetaxpercentage { get; set; }
        public decimal purchasetax { get; set; }
        public long bomcategoryid { get; set; }
        public long bominvoiceid { get; set; }
        public decimal sellingprice { get; set; }
        public decimal marginpercentage { get; set; }
        public decimal salestax { get; set; }
        public string imageid { get; set; }
        public string width { get; set; }
        public long colourid { get; set; }
        public string bominwardcode { get; set; }
        public bool bominwardcodechanged { get; set; }
        public class Poinfo
        {
            public string pono { get; set; }
            public long componentpoitemsid { get; set; }
            public decimal oqty { get; set; }
            public decimal aqty { get; set; }
            public decimal rate { get; set; }
        }
        
        public string categoryname { get; set; }
        public string bomname { get; set; }
        public string bomprintingname { get; set; }
        public string colourname { get; set; }
        public string fgskuid { get; set; }
        public string fguid { get; set; }
        public long fgproductid { get; set; }
        [JsonIgnore]
        public Mfgdesign.Sizegroup.Item skudesignitem { get;  set; }
    }

    public class BominvoiceitemsCreteria : Bominvoiceitems
    {
    }

    public class BOMItemWithStock:BaseEntity
    {
        public Bominvoiceitems bominvoiceitem { get; set; }
        public List<BomStcokTransactionSummary> stocksummary { get; set; }
    }
}
