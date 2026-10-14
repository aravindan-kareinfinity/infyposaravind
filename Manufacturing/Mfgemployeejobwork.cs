using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class Mfgemployeejobwork : BaseEntity
    {
        public long id { get; set; }
        public long employeeid { get; set; }
        public DateTime issedon { get; set; }
        public DateTime receivedon { get; set; }
        public long issedby { get; set; }
        public long recievedby { get; set; }
        public decimal issuedqty { get; set; }
        public decimal receivedqty { get; set; }
        public decimal returnqty { get; set; }
        public long designid { get; set; }
        public string designname { get; set; }
        public long colourid { get; set; }
        public decimal rate { get; set; }
        public decimal paidamount { get; set; }
        public DateTime paidon { get; set; }
        public int statusid { get; set; }
        public long mfgjobworkconfigid { get; set; }
        public bool isactive { get; set; }
        public long createdby { get; set; }
        public long modifiedby { get; set; }
        public int version { get; set; }
        public DateTime createdon { get; set; }
        public DateTime modifiedon { get; set; }
    }
    public class MfgemployeejobworkCreteria : Mfgemployeejobwork
    {
    }
}
