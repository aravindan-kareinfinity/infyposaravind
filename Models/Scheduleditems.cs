using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class Scheduleditems : BaseEntity
    {
        public long scheduleid { get; set; }
        public int hour { get; set; }
        public DateTime lastrun { get; set; }
        public long id { get; set; }

        [Newtonsoft.Json.JsonIgnore]
        public bool deletemarked { get; set; }
        public DateTime validtill { get; set; }
    }
    public class ScheduleditemsCreteria : Scheduleditems
    {
    }
}
