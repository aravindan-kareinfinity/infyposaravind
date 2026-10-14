using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class Discountqueue : BaseEntity
    {
        public bool isactive { get; set; }
        public long createdby { get; set; }
        public long modifiedby { get; set; }
        public int version { get; set; }
        public long id { get; set; }
        public DateTime createdon { get; set; }
        public DateTime modifiedon { get; set; }
        public long billid { get; set; }
        public long requestedby { get; set; }
        public DiscountStatus status { get; set; }
        public long actionedby { get; set; }
        public string notes { get; set; }
        public string actionreason { get; set; }
        public decimal requestedamount { get; set; }
        public decimal requestedpercentage { get; set; }
        public decimal approvedamount { get; set; }
        public decimal approvedpercentage { get; set; }
        public DateTime requestedon { get; set; }
        public DateTime actionedon { get; set; }

        public enum DiscountStatus
        {
            NoDiscount = 0,
            Requested = 1,
            Rejected = 2,
            Approved = 3
        }
        public string billno { get; set; }
        public bool hasaccepted { get; set;}
        public decimal billamount { get; set; }
        public long companyid { get; set; }
    }
    public class DiscountqueueCreteria : Discountqueue
    {
        public enum RequestMode
        {
            None,
            Requester,
            Approver
        }
        public long fromid { get; set; }
        public RequestMode mode { get; set; }
    }
}
