using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class Refillbox : BaseEntity
    {
        public long id { get; set; }
        public bool isactive { get; set; }
        public long createdby { get; set; }
        public long modifiedby { get; set; }
        public int version { get; set; }
        public DateTime createdon { get; set; }
        public DateTime modifiedon { get; set; }
        public long companyid { get; set; }
        public string boxno { get; set; }
        public long floorid { get; set; }
        public long sectionid { get; set; }
        public int boxtype { get; set; }
        public int capacity { get; set; }
        public long productid { get; set; }
        public long brandid { get; set; }
        public long sizeid { get; set; }
        public long materialid { get; set; }
        public bool indisplay { get; set; }
        public long locationid { get; set; }
        public int length { get; set; }
        public int width { get; set; }
        public int height { get; set; }
    }
    public class RefillBoxInfo
    {
        public long id { get; set; }
        public string boxno { get; set; }
        public string floorname { get; set; }
        public string sectionname { get; set; }
        public string boxtypename { get; set; }
        public string locationname { get; set; }
    }
    public class RefillboxCreteria : Refillbox
    {
    }
}
