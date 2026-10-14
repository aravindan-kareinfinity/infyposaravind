using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class Schemebarcodes : BaseEntity
    {
        public long id { get; set; }
        public long schemeid { get; set; }
        public long locationid { get; set; }
        public string barcode { get; set; }
    }
    public class SchemebarcodesCreteria : Schemebarcodes
    {
    }
}
