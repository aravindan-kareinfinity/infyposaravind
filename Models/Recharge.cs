using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class Recharge : BaseEntity
    {
        public bool isactive { get; set; }
        public bool isinterstatesale { get; set; }
        public long createdby { get; set; }
        public long modifiedby { get; set; }
        public int version { get; set; }
        public DateTime createdon { get; set; }
        public DateTime modifiedon { get; set; }
        public long id { get; set; }
        public long supplierid { get; set; }
        public long productid { get; set; }
        public long itemid { get; set; }
        public long schemeid { get; set; }
        public decimal rate { get; set; }
        public decimal cashback { get; set; }
        public decimal discount { get; set; }
        public long receivedemployeeid { get; set; }
        public long approvedemployeeid { get; set; }
        public DateTime rechargedate { get; set; }
        public long consumedbillid { get; set; }
        public string consumedbillno { get; set; }
        public long consumeduserid { get; set; }
        public string rechargekey { get; set; }
        public string rechargeno { get; set; }
        public long customerid { get; set; }
        public string customername { get; set; }
        public string customerphoneno { get; set; }
        public string operatorname { get; set; }
        public string schemename { get; set; }
        public long locationid { get; set; }
        public long companyid { get; set; }
        public long swipmachineid { get; set; }
        public long upiproviderid { get; set; }
        public decimal paid_upi { get; set; }
        public long swipbankid { get; set; }
        public decimal paid_cash { get; set; }
        public decimal paid_card { get; set; }
        public string customeraddress { get; set; }
        public decimal amount { get; set; }
        public long fromlocationid { get; set; }
        public long tolocationid { get; set; }
        public int tranmode { get; set; }
        public string rechargecode { get; set; }
        public string description { get; set; }
        public TranModes mode
        {
            get
            {
                return (TranModes)tranmode;
            }
        }

        public string invoiceno { get; set; }
        public decimal billvalue { get; set; }
        public long taxid { get; set; }
        public long lrinvoiceid { get; set; }
        public enum TranModes
        {
            Topup = 1,
            Transfer = 2,
            Recharge = 3
        }
    }

  
    public class RechargeCreteria : Recharge
    {
        public Rechargescheme scheme { get; set; }
        public string errormessage { get; set; }
        public long fromlocation { get; set; }
        public long tolocation { get; set; }
        public DateTime fromdate { get; set; }
        public DateTime todate { get; set; }
    }
}
