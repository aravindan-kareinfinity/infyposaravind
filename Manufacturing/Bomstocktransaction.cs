using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class Bomstocktransaction : BaseEntity
    {
        public long id { get; set; }
        public long companyid { get; set; }
        public long sourceid { get; set; }
        public bool isactive { get; set; }
        public long createdby { get; set; }
        public long modifiedby { get; set; }
        public int version { get; set; }
        public DateTime createdon { get; set; }
        public DateTime modifiedon { get; set; }
        public long bomid { get; set; }
        public long locationid { get; set; }
        public long colourid { get; set; }
        public string width { get; set; }
        public string refno { get; set; }
        public decimal qty { get; set; }
        public DateTime transactedon { get; set; }
        public long otherlocationid { get; set; }
        public long supplierid { get; set; }
        public int reftype { get; set; }
        public long refid { get; set; }
        public TransactionType RowType
        {
            get
            {
                return (TransactionType)reftype;
            }
            set
            {
                reftype = (int) value;
            }
        }
        public enum TransactionType
        {
            Opening = 0,
            Purchase =1,
            PurchaseReturn = 2,
            TransferIn = 3,
            TransferOut = 4,
            WorkOrderConsumption =5,
            JobOrderConsumption = 6,
            JobOrdeStageConsumption =7,
            JobOrderOutward =8,
            WorkOrderOutward = 9,
            Journal =10,
            Internal = 11,
            External = 12
        }
    }

    public class BomStcokTransactionSummary:BaseEntity
    {
        public long bomid { get; set; }
        public long companyid { get; set; }
        public long locationid { get; set; }
        public long colourid { get; set; }
        public string colourcode { get; set; }
        public long categoryid { get; set; }
        public decimal buyingrate { get; set; }
        public decimal landingcost { get; set; }
        public decimal romax { get; set; }
        public decimal romin { get; set; }
        //public string width { get; set; }
        public decimal opening { get; set; }
        public decimal inward { get; set; }
        public decimal outward { get; set; }
        public decimal closing
        {
            get
            {
                return opening + inward - outward;
            }
        }

        public decimal poqty { get;  set; }
        public decimal stockvalue { get; internal set; }
    }

    public class BomStcokTransactionDetail
    {
        public long locationid { get; set; }
        public long companyid { get; set; }
        public long bomid { get; set; }
        public long colourid { get; set; }
        public long otherlocationid { get; set; }
        public long supplierid { get; set; }
        //public string width { get; set; }
        public int reftype { get; set; }
        public string refno { get; set; }
        public DateTime transactedon { get; set; }
        public decimal opening { get; set; }
        public decimal inward { get; set; }
        public decimal outward { get; set; }
        public decimal closing { get; set; }
    }
    public class BomstocktransactionCreteria : Bomstocktransaction
    {
        public bool printpdf { get; set; }
        public DateTime fromdate { get; set; }
        public DateTime todate { get; set; }
        public long categoryid { get; set; }
        public int mode { get; set; }
        public int rolmode { get; set; }
        public bool reorderlevel { get; set; }
        public List<long> bomids { get;  set; }
        public bool stockvalue { get;  set; }
    }
}
