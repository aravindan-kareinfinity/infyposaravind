using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class Uniquesequance : BaseEntity
    {
        public long id { get; set; }
        public bool isactive { get; set; }
        public long createdby { get; set; }
        public long modifiedby { get; set; }
        public int version { get; set; }
        public long groupid { get; set; }
        public bool used { get; set; }
        public DateTime createdon { get; set; }
        public DateTime modifiedon { get; set; }
        public DateTime usedon { get; set; }
        public string sequenceno { get; set; }
        public long usedreferenceid { get; set; }
        public string usedreferencename { get; set; }
        public long sequenceindex { get; set; }
    }
    public class UniquesequanceCreteria : Uniquesequance
    {
        public long fromno { get; set; }
        public long tono { get; set; }
        public string prefix { get; set; }
        public string suffix { get; set; }
    }
}
