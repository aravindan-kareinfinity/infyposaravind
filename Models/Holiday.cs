using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class Holiday : BaseEntity
    {
        public long id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public DateTime holidayon { get; set; }
        public string holidaytype { get; set; }
        public bool isactive { get; set; }
        public long createdby { get; set; }
        public long modifiedby { get; set; }
        public int version { get; set; }
        public int leaveencashment { get; set; }
        public DateTime createdon { get; set; }
        public DateTime modifiedon { get; set; }
        public bool payable { get; set; }
    }
    public class HolidayCreteria : Holiday
    {
        public List<long> ids { get; set; }
        public int year { get; set; }
        public DateTime fromdate { get; set; }
        public DateTime todate { get; set; }
    }
}
