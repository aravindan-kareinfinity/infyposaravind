using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class Buyback : BaseEntity
    {
        public bool isactive { get; set; }
        public long createdby { get; set; }
        public long modifiedby { get; set; }
        public int version { get; set; }
        public DateTime createdon { get; set; }
        public DateTime modifiedon { get; set; }
        public long id { get; set; }
        public long supplierid { get; set; }
        public long productid { get; set; }
        public long brandid { get; set; }
        public long itemid { get; set; }
        public long schemeid { get; set; }
        public decimal purchaseprice { get; set; }
        public decimal schemediscount { get; set; }
        public long receivedemployeeid { get; set; }
        public long approvedemployeeid { get; set; }
        public int status { get; set; }
        public DateTime statuschangedon { get; set; }
        public DateTime buybackdate { get; set; }
        public bool hasconsumed { get; set; }
        public long consumedbillid { get; set; }
        public string consumedbillno { get; set; }
        public long consumeduserid { get; set; }
        public string buybackkey { get; set; }
        public string buybackno { get; set; }
        public long customerid { get; set; }
        public string customername { get; set; }
        public string customerphoneno { get; set; }
        public string schemename { get; set; }
        public long locationid { get; set; }
        public long companyid { get; set; }
        public string customeraddress { get; set; }
        public string imieno { get;  set; }
        public long salablegoodsid { get;  set; }
        public long stockid { get;  set; }
        public BuyBackStatus Status
        {
            get
            {
                return (BuyBackStatus)status;
            }
            set
            {
                status = (int)value;
            }
        }
        public enum BuyBackStatus
        {
            Received, Exchanged, CreditNote, DebitNote, Return, Sold
        }

    }
    public class BuybackCreteria : Buyback
    {
        public DateTime fromdate { get; set; }
        public DateTime todate { get; set; }
    }
}
