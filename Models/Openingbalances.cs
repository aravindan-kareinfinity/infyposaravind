using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class Openingbalances : BaseEntity
    {
        public bool isactive { get; set; }
        public DateTime createdon { get; set; }
        public DateTime modifiedon { get; set; }
        public long createdby { get; set; }
        public int version { get; set; }
        public long modifiedby { get; set; }
        public long id { get; set; }
        public long locationid { get; set; }
        public long companyid { get; set; }
        public int ledgertype { get; set; }
        public long ledgerid { get; set; }
        public int financeyear { get; set; }
        public decimal opening { get; set; }
        public decimal closing { get; set; }
        public int obsource { get; set; }
        public string ledgername { get; set; }
    }
    public class OpeningbalancesCreteria : Openingbalances
    {
    }
}
