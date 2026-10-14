using InfyPOS.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    
    public class Inbox : BaseEntity
    {
        public enum StatusList
        {
            Open=0,
            Working = 1,
            Completed = 2,
            Cancel = 3,
            Approved = 4,
            Rejected = 5,
            Pending = 6,
            //Courier
            Received = 100,
            Dispatched = 101,
            Send = 102,
            Sent = 103,
            ReceivedHold = 104,
            //LR
            New_LR =200,
            LRIssue = 201,
            LRReceived = 202,
            Invoice_Preparation =203,
            Items_Entering=204,
            Barcode_Generating=205,
            Item_Pricing = 206
        }

        public enum Types
        {
            ALL,
            LREntry,
            Courier,
            LRIssue,
            LRReceived,
            Package,
            PurchaseOrder,
            PurchaseReturn
        }
       
        public Types Type {
            get
            {
                return (Types)typeid;
            }
            set
            {
                typeid = (int)value;
                typedisplay = value.ToString();
            }
        }
        public StatusList Status
        {
            get
            {
                return (StatusList)statusid;
            }
            set
            {
                statusid = (int)value;
                statusname = value.ToString();
            }
        }
        public long lrcompanyid { get; set; }
        public long entrylocationid { get; set; }
        public int typeid { get; set; }
        public string age { get; set; }
        public int modeofdelivery { get; set; }
        public string typedisplay { get; set; }
        public string slipno { get; set; }
        public string createdusername { get; set; }
        public long referenceid { get; set; }
        public string referenceno { get; set; }
        public DateTime addedon { get; set; }
        public long supplierid { get; set; }
        public long sourcesupplierid { get; set; }
        public string suppliername { get; set; }
        public int invoices { get; set; }
        public int statusid { get; set; }
        public string statusname { get; set; }
        public DateTime createdon { get; set; }
        public DateTime modifiedon { get; set; }
        public long createdby { get; set; }
        public long modifiedby { get; set; }
        public bool isactive { get; set; }
        public bool hasclosed { get; set; }
        public long closedby { get; set; }
        public string closeduser { get; set; }
        public DateTime closedon { get; set; }
        public decimal numbers { get; set; }
        public string notes { get; set; }
        public string section { get; set; }
        public int version { get; set; }
        public long id { get; set; }
        public long parentid { get; set; }
        public DateTime referencedate { get; set; }
        public string invoiceno { get;  set; }
        public string lrentryno { get;  set; }
        public DateTime invoicedate { get; set; }
        public long transportid { get; set; }
        public long locationid { get; set; }
        public int noofbundels { get; set; }
        public long cityid { get; set; }
        public long agentid { get; set; }
        public string lrentrydetail { get; set; }
        public string invoicenodetail { get; set; }
        public DateTime receiveddate { get; set; }
        public DateTime lrdate { get; set; }
        public DateTime lrentrydate { get; set; }
        public int noofpieces { get; set; }
        public decimal totalamount { get; set; }
        public string msmeno { get; set; }
    }

    public class InBoxStatus : BaseEntity
    {
        public long id { get; set; }
        public bool hasclosed { get; set; }
        public long closedby { get; set; }
        public string closeduser { get; set; }
        public DateTime closedon { get; set; }
        public string notes { get; set; }
        public int statusid { get; set; }
        public string statusname { get; set; }
    }

    public class InboxCreteria : Inbox
    {
        public DateTime addedfrom { get; set; }
        public DateTime addedto { get; set; }
        public bool showallopen { get; set; }
        public long currentuserid { get; set; }
    }
}
