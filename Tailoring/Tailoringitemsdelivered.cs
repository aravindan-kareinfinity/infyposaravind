using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class Tailoringitemsdelivered : BaseEntity
    {
        public long id { get; set; }
        public long tailoringitemsid { get; set; }
        public long tailoringorderid { get; set; }
        public decimal deliveredqty { get; set; }
        public string product { get; set; }
        public long deliveredby { get; set; }
        public DateTime deliveredon { get; set; }
        public DateTime createdon { get; set; }
        public string receivedby { get; set; }
        public bool receivedbycustomer { get; set; }
        public int version { get; set; }
        public bool isactive { get; set; }
        public long modifiedby { get; set; }
        public long createdby { get; set; }
        public DateTime modifiedon { get; set; }
    }
    public class TailoringitemsdeliveredCreteria : Tailoringitemsdelivered
    {
    }
}
