using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class Brsstatus : BaseEntity
    {
        public long id { get; set; }
        public int transactiontype { get; set; }
        public long bankid { get; set; }
        public long companyid { get; set; }
        public long refid { get; set; }
        public int status { get; set; }
        public DateTime realisationon { get; set; }
        public bool isactive { get; set; }
        public long createdby { get; set; }
        public long modifiedby { get; set; }
        public int version { get; set; }
        public DateTime createdon { get; set; }
        public DateTime modifiedon { get; set; }
    }
	
    public class BrsstatusCreteria : Brsstatus
    {
    }
}
