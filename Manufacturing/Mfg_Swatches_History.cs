using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class Mfg_Swatches_History : BaseEntity
    {
        public long swatchesid { get; set; }
        public long folderid { get; set; }
        public int transactiontype { get; set; }
        public TransactionModes transactionmode
        {
            get
            {
                return (TransactionModes)transactiontype;
            }
            set
            {
                transactiontype = (int)value;
            }
        }
        public long actionedby { get; set; }
        public int qty { get; set; }
        public string remarks { get; set; }
        public DateTime transactionon { get; set; }
        public int referencetype { get; set; }
        public long referenceid { get; set; }
        public string product { get; set; }
        public string brand { get; set; }
        public string design { get; set; }
        public string salesmen { get; set; }
        public string action { get; set; }
        public bool isactive { get; set; }
        public int version { get; set; }
        public DateTime modifiedon { get; set; }
        public DateTime createdon { get; set; }
        public long createdby { get; set; }
        public long modifiedby { get; set; }
        public long id { get; set; }
        public List<long> ids { get; set; }
        public string receiptno { get; set; }
        public enum TransactionModes
        {
            Issue,
            Received,
            Journal
        }
        [JsonIgnore]
        public int orderid { get; set; }
        [JsonIgnore]
        public bool IsIssue
        {
            get
            {
                return transactionmode == TransactionModes.Issue;
            }
        }
        [JsonIgnore]
        public List<Mfg_Swatches_HistoryDetail> items { get; set; }
        [JsonIgnore]
        public Company company { get; internal set; }
    }


    public class Mfg_Swatches_HistoryDetail : Mfg_Swatches_History
    {
        public Employee employee { get; internal set; }
        public Employee employeesalesmen { get; internal set; }
    }
    public class Mfg_Folder_Issue : BaseEntity
    {
        public long id { get; set; }
        public string design { get; set; }
        public long folderid { get; set; }
        public long issuedby { get; set; }
        public long receivedby { get; set; }
        public long swatchesid { get; set; }
        public DateTime transactionon { get; set; }
        public int qty { get; set; }
        public string remark { get; set; }
    }
    public class Mfg_Folder_Receive : BaseEntity
    {
        public long id { get; set; }
        public string design { get; set; }
        public long folderid { get; set; }
        public long receivedby { get; set; }
        public long returnedby { get; set; }
        public long swatchesid { get; set; }
        public DateTime transactionon { get; set; }
        public int qty { get; set; }
        public string remark { get; set; }
    }

    public class Mfg_Folder_Journal : BaseEntity
    {
        public long id { get; set; }
        public string design { get; set; }
        public long folderid { get; set; }
        public long adjustedby { get; set; }
        public long salemenid { get; set; }
        public long swatchesid { get; set; }
        public DateTime transactionon { get; set; }
        public int qty { get; set; }
        public string remark { get; set; }
    }
    public class Mfg_Swatches_HistoryCreteria : Mfg_Swatches_History
    {
        public DateTime fromdate { get; set; }
        public DateTime todate { get; set; }
        public bool includeitems { get; set; }
    }
}
