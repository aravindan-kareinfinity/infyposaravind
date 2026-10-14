using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace InfyPOS.GST
{
    public class GSTEWAY
    {
        public class ItemList
        {
            public int itemNo { get; set; }
            public string productName { get; set; }
            public string productDesc { get; set; }
            public int hsnCode { get; set; }
            public int quantity { get; set; }
            public string qtyUnit { get; set; }
            public int taxableAmount { get; set; }
            public int sgstRate { get; set; }
            public int cgstRate { get; set; }
            public int igstRate { get; set; }
            public int cessRate { get; set; }
        }

        public class BillList
        {
            public string userGstin { get; set; }
            public string supplyType { get; set; }
            public int subSupplyType { get; set; }
            public string docType { get; set; }
            public string docNo { get; set; }
            public string docDate { get; set; }
            public string fromGstin { get; set; }
            public string fromTrdName { get; set; }
            public string fromAddr1 { get; set; }
            public string fromAddr2 { get; set; }
            public string fromPlace { get; set; }
            public int fromPincode { get; set; }
            public int fromStateCode { get; set; }
            public int actualFromStateCode { get; set; }
            public string toGstin { get; set; }
            public string toTrdName { get; set; }
            public string toAddr1 { get; set; }
            public string toAddr2 { get; set; }
            public string toPlace { get; set; }
            public int toPincode { get; set; }
            public int toStateCode { get; set; }
            public int actualToStateCode { get; set; }
            public int totalValue { get; set; }
            public int cgstValue { get; set; }
            public int sgstValue { get; set; }
            public int igstValue { get; set; }
            public int cessValue { get; set; }
            public int transMode { get; set; }
            public int transDistance { get; set; }
            public string transporterName { get; set; }
            public string transporterId { get; set; }
            public string transDocNo { get; set; }
            public string transDocDate { get; set; }
            public string vehicleNo { get; set; }
            public string vehicleType { get; set; }
            public int totInvValue { get; set; }
            public int mainHsnCode { get; set; }
            public List<ItemList> itemList { get; set; }
        }

        public class RootObject
        {
            public string version { get; set; }
            public List<BillList> billLists { get; set; }
        }
    }
}