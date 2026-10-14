using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class Stockmarker : BaseEntity
    {
        public bool isactive { get; set; }
        public long createdby { get; set; }
        public long modifiedby { get; set; }
        public int version { get; set; }
        public long id { get; set; }
        public bool isdamaged { get; set; }
        public DateTime createdon { get; set; }
        public DateTime modifiedon { get; set; }
        public long stockid { get; set; }
        public long markerid { get; set; }
        public decimal qty { get; set; }
        public decimal stockqty { get; set; }
        public decimal stockqtyatmark { get; set; }
        public DateTime addedon { get; set; }
        public long locationid { get; set; }
        public long salablegoodsid { get; set; }
        public string barcode { get; set; }
        public string glncode { get; set; }
        public long productid { get; set; }
        public long salestaxid { get; set; }
        public decimal taxpercentage { get; set; }
    }

    public class MarkerContainer : BaseEntity
    {
        public bool isdamaged { get; set; }
        public bool deleteall { get; set; }
        public bool delete { get; set; }
        public long markerid { get; set; }
        public long locationid { get; set; }
        public bool isonline { get; set; }
        public bool isinvoice { get; set; }
        public bool isexclusive { get; set; }
        public List<long> invoiceitemids { get; set; }
        public List<Stockmarker> items { get; set; }
    }
    public class Stockmarkerdetail : Stockmarker
    {
        public string brandname { get; set; }
        public string suppliername { get; set; }
        public string patternname { get; set; }
        public string stylename { get; set; }
        public string productname { get; set; }
        public string sizename { get; set; }
        public string materialname { get; set; }
        public string typename { get; set; }
        public string designid { get; set; }
        public string hsncode { get; set; }
        public string colourname { get; set; }
        public decimal price { get; set; }
        public decimal discount { get; set; }
        public decimal olqty { get; set; }

    }

    public class StockMarkerOLQty:BaseEntity
    {
        public long id { get; set; }
        public long salablegoodsid { get; set; }
        public long olqty { get; set; }
    }
    public class StockmarkerCreteria : Stockmarker
    {
        public int mode { get; set; }
        public List<long> markerids { get;  set; }
        public List<StockQty> StockList { get; set; }
        public class StockQty
        {
            public long markerid { get; set; }
            public decimal qty { get; set; }
        }
    }
}
