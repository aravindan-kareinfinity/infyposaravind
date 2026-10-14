using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class Trace : BaseEntity
    {
        public string type { get; set; }
        public DateTime loggedat { get; set; }
        public string methodName { get; set; }
        public string exception { get; set; }
        public string info { get; set; }
        public string requesturl { get; set; }
        
    }
    public class TraceCreteria : Trace
    {
        public DateTime fromdate { get; set; }
        public DateTime todate { get; set; }
        public long organizationid { get; set; }
    }
}
