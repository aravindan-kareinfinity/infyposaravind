using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{

 
    public class Voucheritembillwise : BaseEntity
    {
        public long id { get; set; }
        public DateTime transactionon { get; set; }
        public decimal receivable { get; set; }
        public decimal payable { get; set; }
        public long sourceitemid { get; set; }
        public decimal paid { get; set; }
        public decimal adjusted { get; set; }
        public string referencetype { get; set; }
        public string referenceno { get; set; }
        public string referenceinvoiceno { get; set; }
        public DateTime referenceinvoicedate { get; set; }
        public long obcompanyid { get; set; }
        public long voucherid { get; set; }
        public int version { get; set; }
        public long voucheritemsid { get; set; }
        public long ledgerid { get; set; }
        public long createdby { get; set; }
        public long modifiedby { get; set; }
        public bool isactive { get; set; }
        public bool onadvance { get; set; }
        public DateTime createdon { get; set; }
        public DateTime modifiedon { get; set; }
        public long lrinvoicereturnid { get; set; }
        public long customerinvoiceid { get; set; }

    }
    public class VoucheritembillwiseCreteria : Voucheritembillwise
    {
        public List<long> ids { get; set; }
        public List<long> sourceitemids { get; set; }
        public List<long> voucherids { get; set; }
        public bool onlypaid { get; set; }
        public bool showopening { get; set; }
    }

    public class BillwiseItem : BaseEntity
    {
        public long id { get; set; }
        public DateTime transactionon { get; set; }
        public decimal amount { get; set; }
        public decimal adjusted { get; set; }
        public decimal paid { get; set; }
        public long ledgerid { get; set; }
        public bool onadvance { get; set; }
        public string referencetype { get; set; }
        public string referenceno { get; set; }
        public string referenceinvoiceno { get; set; }
        public DateTime referenceinvoicedate { get; set; }
    }

    public class LedgerOpening : BaseEntity
    {
        public long ledgerid { get; set; }
        public long companyid { get; set; }
        public long customerid { get; set; }
        public long supplierid { get; set; }
        public List<Voucheritembillwise> openinglist { get; set; }
    }
}
