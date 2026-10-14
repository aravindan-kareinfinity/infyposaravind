using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class Mfg_Swatches_Folder : BaseEntity
    {
        public long swatchesid { get; set; }
        
        public string code { get; set; }
        public int status { get; set; }
        public bool isactive { get; set; }
        public int version { get; set; }
        public int noofcolours { get; set; }
        public DateTime modifiedon { get; set; }
        public DateTime createdon { get; set; }
        public DateTime creationon { get; set; }
        public long preparedby { get; set; }
        public long createdby { get; set; }
        public long modifiedby { get; set; }
        public long designid { get; set; }
        public long id { get; set; }
        public int qty { get; set; }
        public string design { get; set; }
        public string brand { get; set; }
        public string product { get; set; }
        public int issue { get; set; }
        public int received { get; set; }
        public int journal { get; set; }

    }
    public class Mfg_Swatches_creteria:BaseEntity
    {
        public long salesmenid { get; set; }
        public long folderid { get; set; }
        public long designid { get; set; }
        public DateTime fromdate { get; set; }
        public DateTime todate { get; set; }
    }
    public class Mfg_Swatches_FolderSummary
    {
        public string salesmen { get; set; }
        public string design { get; set; }
        public string folder { get; set; }
        public string product { get; set; }
        public string brand { get; set; }
        public long fgstock { get; set; }
        public long folderstock { get; set; }
        public long sopending { get; set; }
        public long soselfpending { get; set; }
        public long soselforder { get; set; }
    }
    public class Mgg_Stockitem
    {
        public string design { get; set; }
        public long colourid { get; set; }
        public decimal stock { get; set; }
        public decimal order { get; set; }
    }
    public class Mfg_Swatches_FolderDetail : Mfg_Swatches_Folder
    {
        public long designid { get; set; }
        public int stock { get; set; }
    }
    public class Mfg_Swatches_FolderCreteria : Mfg_Swatches_Folder
    {
        public long salesmenid { get; set; }
        public bool showdetail { get; set; }
    }
}
