using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class Mfgworkorderitems : BaseEntity
    {
        public long id { get; set; }
        public long mfgworkorderid { get; set; }
        public bool isactive { get; set; }
        public long createdby { get; set; }
        public long modifiedby { get; set; }
        public int version { get; set; }
        public DateTime createdon { get; set; }
        public DateTime modifiedon { get; set; }
        public decimal received { get; set; }
        public decimal damaged { get; set; }
        public long colourid { get; set; }
        public string designitemname { get; set; }
        public long sleeveid { get; set; }
        public long fabricid { get; set; }
        public long efabricid { get; set; }
        public long efabriccolourid { get; set; }
        public long bomcategoryid { get; set; }
        public string bominvoiceitemimage { get; set; }
        public string bominwardcode { get; set; }
        public string size { get; set; }
        public long designid { get; set; }
        public decimal inqty { get; set; }
        public decimal outqty { get; set; }
        public decimal cutratio { get; set; }
        public decimal length { get; set; }
        public decimal cost { get; set; }
        public decimal taxamount { get; set; }
        public decimal amount { get; set; }
        public decimal taxpercentage { get; set; }
        public long taxid { get; set; }
        public long productid { get; set; }
        public string designname { get; set; }
        public string productname { get; set; }
        public int sizecount { get; set; }
        public List<Mfgworkorderitemsizes> sizelist { get; set; }
        public List<PurchaseorderitemsDetail.SizeQty> jumpqty { get; set; }
        public ReportLibrary.DOSReporter.Table sizetable { get; set; }
        public string colourname { get; set; }
        public string sleevename { get; set; }
        public string colourcode
        {
            get
            {
                if (string.IsNullOrEmpty(designitemname))
                    return "";
                if(designitemname.IndexOf('/')>0)
                    return designitemname.Substring(designitemname.IndexOf('/') + 1);
                return designitemname;
            }
        }

        [JsonIgnore]
        public int sno { get;  set; }
        public long salesorderid { get; set; }

    }
    public class MfgworkorderitemsCreteria : Mfgworkorderitems
    {
    }
}
