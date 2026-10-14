using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class Customerdivert : BaseEntity
    {
        public string ErrorMessage { get; set; }
        public long id { get; set; }
        public string customername { get; set; }
        public string mobileno { get; set; }
        public DateTime diverton { get; set; }
        public long ccinchargeid { get; set; }
        public long sectionid { get; set; }
        public long customertypeid { get; set; }
        public string reason { get; set; }
        public long sectioninchargeid { get; set; }
        public string sectionfeedback { get; set; }
        public long actiontakenbyid { get; set; }
        public long actionstatus { get; set; }
        public string actioncomments { get; set; }
        public int version { get; set; }
        public DateTime createdon { get; set; }
        public DateTime modifiedon { get; set; }
        public bool isactive { get; set; }
        public long createdby { get; set; }
        public long modifiedby { get; set; }
    }
    public class CustomerdivertCreteria : Customerdivert
    {
        public DateTime fromdate { get; set; }
        public DateTime todate { get; set; }
    }
}
