using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class Mfgtailoringreceived : BaseEntity
    {
        public int qty { get; set; }
        public int cqty { get; set; }
        public long id { get; set; }
        public long mfgtailoringid { get; set; }
        public DateTime receivedon { get; set; }
        public long createdby { get; set; }
        public DateTime createdon { get; set; }
        public int version { get; set; }
        public long modifiedby { get; set; }
        public long employeeid { get; set; }
        public DateTime modifiedon { get; set; }
        public bool isactive { get; set; }
        public long sizeid { get; set; }
        public string sizename { get; set; }
        public string sizegroupname { get; set; }
    }
    public class MfgtailoringreceivedCreteria : Mfgtailoringreceived
    {
    }
}
