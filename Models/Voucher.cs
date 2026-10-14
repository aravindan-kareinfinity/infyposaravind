using InfyGST;
using InfyPOS.Common;
using InfyPOS.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Web;

namespace InfyPOS.Models
{
    public class Voucher : BaseEntity
    {
        public long id { get; set; }
        public long vouchertype { get; set; }
        public string uniqueuid { get; set; }
        public DateTime voucherdate { get; set; }
        public long vouchertypeid { get; set; }
        public long purchasetypeid { get; set; }
        public string voucherno { get; set; }
        public string narration { get; set; }
        public decimal amount { get; set; }
        public bool isactive { get; set; }
        public long createdby { get; set; }
        public long modifiedby { get; set; }
        public int version { get; set; }
        public decimal opening { get; set; }
        public DateTime createdon { get; set; }
        public DateTime modifiedon { get; set; }
        public List<Voucheritems> voucheritems { get; set; }
        public List<BillwiseItem> billadjustement { get; set; }
        public bool adjustmentcredit { get; set; }
        public long adjustmentledgerid { get; set; }
        public long locationid { get; set; }
        public long employeeid { get; set; }
        public long companyid { get; set; }
        public int sourcetype { get; set; }

        /*3 : cash sales,
         * 4 : bank sales
        */


        public bool isinterstate { get; set; }

        public voucharattributesExtension attributes { get; set; }

        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string attributes_json { get { return Newtonsoft.Json.JsonConvert.SerializeObject(attributes); } set { attributes = Newtonsoft.Json.JsonConvert.DeserializeObject<voucharattributesExtension>(value); } }

        public int bwise { get; set; }

        public string TallyVoucherType()
        {
            switch (vouchertype)
            {
                case 1:
                    return "Payment";
                case 2:
                    return "Receipt";
                case 3:
                    return "Purchase";
                case 4:
                    return "Sales";
                case 5:
                    return "Credit Note";
                case 6:
                    return "Debit Note";
                case 7:
                    return "Contra";
                case 8:
                    return "Journal";
                case 9:
                    return "Contra";
                case 10:
                    return "Contra";
                case 11:
                    return "Journal";
                case 12:
                    return "Journal";
                case 13:
                    return "Payment";
                case 14:
                    return "Payment";
            }
            return "Payment";

        }

        public class voucharattributes
        {
            public DateTime saledatefrom { get; set; }
            public DateTime saledateto { get; set; }
            public int banktransfermode { get; set; }
            public string bankreference { get; set; }
            public DateTime bankdate { get; set; }
            public long lrinvoicereturnid { get; set; }
            public long customerinvoiceid { get; set; }
            public bool hasgst { get; set; }
            public bool hasinv { get; set; }
            public long invledgerid { get; set; }
            public int ledgertype { get; set; }
            public DateTime invoicedate { get; set; }
            public string invoiceno { get; set; }
            public string eack { get; set; }
            public string einv { get; set; }
            public string einvdetail { get; set; }
            public bool einvcan { get; set; }
            public string einvcandetail { get; set; }
            public bool hastds { get; set; }
            public long ledgertdsid { get; set; }
            public string panno { get; set; }
            public bool hasrcm { get; set; }
            public bool dgst { get; set; }
            public bool dtds { get; set; }
            public string imgid { get; set; }
        }

        public class voucharattributesExtension : voucharattributes
        {
            public long counterid { get; set; }
            public decimal adjustedvouchervalue { get; set; }
            public string adjustedvouchers { get; set; }
            [JsonIgnore]
            public GSTPortal.EInvoiceResponse GSTEinvoice { get; set; }
        }
    }

    public class PettycashVoucher
    {
        public decimal cashincome { get; set; }
        public decimal cashexpense { get; set; }
        public decimal cashwithdraw { get; set; }
        public decimal cashdeposit { get; set; }
        public decimal opening { get; set; }
        public decimal closing { get; set; }
        public Voucher voucher { get; set; }
        public string title { get; set; }
        public string subtitle { get; set; }
        public List<ReportLine> items { get; set; }
        public VoucherDataService.CashResult CashOpening { get;  set; }
        public VoucherDataService.CashResult CashClosing { get;  set; }
        public string message { get; set; }
        public Company company { get; set; }
        public CompanyLocation location { get; set; }
        internal void SetCashTransaction(VoucherDataService.CashResult cashtransaction)
        {
            /*{ 'id': 12, 'name': 'CASH Sales' },
            { 'id': 13, 'name': 'CARD Sales' },
            { 'id': 14, 'name': 'UPI Sales' },
            { 'id': 15, 'name': 'CHQ Sales' },
            { 'id': 16, 'name': 'CREDIT Sales' },
            { 'id': 17, 'name': 'VOUCHER Sales' },
            { 'id': 18, 'name': 'CASH Ledger' }*/

            if (voucher.voucheritems == null) voucher.voucheritems = new List<Voucheritems>();
            decimal totalSales = 0;
            int voucheritemid = -100000;
            if(cashtransaction.cashsales>0)
            {
                //totalSales += cashtransaction.cashsales;
                voucheritemid++;
                var cashSaleLeger = new Voucheritems()
                {
                    id = voucheritemid,
                    ledgerid = cashtransaction.ledgers[12].id,
                    amount = cashtransaction.cashsales,
                    attributes = new Voucheritems.voucharattributes() { ledgertype = 14,narration = "CASH SALES" },
                    iscredit = true,
                    isactive=true
                };
                voucher.voucheritems.Add(cashSaleLeger);
            }
            if (cashtransaction.cardsales.Count > 0)
            {
                totalSales += cashtransaction.cardsales.Sum(x => x.amount);
                voucheritemid++;
                voucher.voucheritems.Add(new Voucheritems()
                {
                    id = voucheritemid,
                    ledgerid = cashtransaction.ledgers[13].id,
                    amount = cashtransaction.cardsales.Sum(x=>x.amount),
                    attributes = new Voucheritems.voucharattributes() { narration = "CARD SALES", ledgertype = 14 },
                    iscredit = true,
                    isactive =true
                });
            }
            if (cashtransaction.upisales.Count > 0)
            {
                totalSales += cashtransaction.upisales.Sum(x => x.amount);
                voucheritemid++;
                voucher.voucheritems.Add(new Voucheritems()
                {
                    id = voucheritemid,
                    ledgerid = cashtransaction.ledgers[14].id,
                    amount = cashtransaction.upisales.Sum(x => x.amount),
                    attributes = new Voucheritems.voucharattributes() { narration = "UPI SALES", ledgertype = 14 },
                    iscredit = true,
                    isactive = true
                });
            }
            if (cashtransaction.chequesales.Count > 0)
            {
                totalSales += cashtransaction.chequesales.Sum(x => x.amount);
                voucheritemid++;
                voucher.voucheritems.Add(new Voucheritems()
                {
                    id = voucheritemid,
                    ledgerid = cashtransaction.ledgers[15].id,
                    amount = cashtransaction.chequesales.Sum(x => x.amount),
                    attributes = new Voucheritems.voucharattributes() { narration = "CHEQUE SALES", ledgertype = 14 },
                    iscredit = true,
                    isactive = true
                });
            }
            if (cashtransaction.creditsales.Count > 0)
            {
                totalSales += cashtransaction.creditsales.Sum(x => x.amount);
                voucheritemid++;
                voucher.voucheritems.Add(new Voucheritems()
                {
                    id = voucheritemid,
                    ledgerid = cashtransaction.ledgers[16].id,
                    amount = cashtransaction.creditsales.Sum(x => x.amount),
                    attributes = new Voucheritems.voucharattributes() { narration = "CREDIT SALES", ledgertype = 14 },
                    iscredit = true,
                    isactive = true
                });
            }
            if (cashtransaction.vouchersales.Count > 0)
            {
                totalSales += cashtransaction.vouchersales.Sum(x => x.amount);
                voucheritemid++;
                voucher.voucheritems.Add(new Voucheritems()
                {
                    id = voucheritemid,
                    ledgerid = cashtransaction.ledgers[17].id,
                    amount = cashtransaction.vouchersales.Sum(x => x.amount),
                    attributes = new Voucheritems.voucharattributes() { narration = "VOUCHER SALES(ADJ)",ledgertype = 14 },
                    iscredit = true,
                    isactive = true
                });
            }

            if(cashtransaction.cashrefund>0)
            {
                voucheritemid++;
                voucher.voucheritems.Add(new Voucheritems()
                {
                    id = voucheritemid,
                    ledgerid = cashtransaction.ledgers[18].id,
                    amount = cashtransaction.cashrefund,
                    attributes = new Voucheritems.voucharattributes() { narration = "CASH REFUND",ledgertype = 11},
                    iscredit = false,
                    isactive = true
                });
            }

            if (totalSales != 0)
            {
                voucheritemid++;
                voucher.voucheritems.Add(new Voucheritems()
                {
                    id = voucheritemid,
                    ledgerid = cashtransaction.ledgers[1].id,
                    amount = totalSales,
                    attributes = new Voucheritems.voucharattributes() { narration = "TOTAL SALES", ledgertype = 14 },
                    iscredit = false,
                    isactive = true
                });
            }

            if (cashtransaction.customerpayments.Count > 0)
            {
                voucheritemid++;
                voucher.voucheritems.Add(new Voucheritems()
                {
                    id = voucheritemid,
                    ledgerid = cashtransaction.ledgers[18].id,
                    amount = cashtransaction.customerpayments.Sum(x => x.amount),
                    attributes = new Voucheritems.voucharattributes() { narration = "CUSTOMER PAYMENT", ledgertype = 11 },
                    iscredit = true,
                    isactive = true
                });
            }

            if (cashtransaction.vouchersold.Count > 0)
            {
                voucheritemid++;
                voucher.voucheritems.Add(new Voucheritems()
                {
                    id = voucheritemid,
                    ledgerid = cashtransaction.ledgers[18].id,
                    amount = cashtransaction.vouchersold.Sum(x => x.amount),
                    attributes = new Voucheritems.voucharattributes() { narration = "VOUCHER SOLD", ledgertype = 11 },
                    iscredit = true,
                    isactive = true
                });
            }
            voucher.voucheritems = voucher.voucheritems.OrderBy(x => x.id).ToList();

            var changes = new StringBuilder();

            if(voucher.voucheritems.FindAll(x => x.id > 0 && x.ledgerid == cashtransaction.ledgers[12].id &&
                (x.attributes != null && x.attributes.ledgertype == 14 && x.attributes.narration == "CASH SALES")).Sum(x=>x.amount) !=
            voucher.voucheritems.FindAll(x => x.id<0 && x.ledgerid == cashtransaction.ledgers[12].id &&
                (x.attributes != null && x.attributes.ledgertype == 14 && x.attributes.narration == "CASH SALES")).Sum(x => x.amount))
            {
                changes.AppendLine("Cash sales changed");
            }
            
            voucher.voucheritems.RemoveAll(x => x.id > 0 && x.ledgerid == cashtransaction.ledgers[12].id &&
            (x.attributes != null && x.attributes.ledgertype == 14 && x.attributes.narration == "CASH SALES"));

            voucher.voucheritems.FindAll(x => x.ledgerid == cashtransaction.ledgers[12].id &&
            (x.attributes != null && x.attributes.ledgertype == 14 && x.attributes.narration == "CASH SALES")).ForEach(x => x.id = 0);


            if(voucher.voucheritems.FindAll(x => x.id > 0 && x.ledgerid == cashtransaction.ledgers[18].id &&
            (x.attributes != null && x.attributes.ledgertype == 11 && x.attributes.narration == "VOUCHER SOLD")).Sum(x=>x.amount)!=
            voucher.voucheritems.FindAll(x => x.id < 0 && x.ledgerid == cashtransaction.ledgers[18].id &&
            (x.attributes != null && x.attributes.ledgertype == 11 && x.attributes.narration == "VOUCHER SOLD")).Sum(x => x.amount))
            {
                changes.AppendLine("Voucher sales changed");
            }

            voucher.voucheritems.RemoveAll(x => x.id > 0 && x.ledgerid == cashtransaction.ledgers[18].id &&
            (x.attributes != null && x.attributes.ledgertype == 11 && x.attributes.narration == "VOUCHER SOLD"));

            voucher.voucheritems.FindAll(x => x.ledgerid == cashtransaction.ledgers[18].id &&
            (x.attributes != null && x.attributes.ledgertype == 11 && x.attributes.narration == "VOUCHER SOLD")).ForEach(x => x.id = 0);



            if(voucher.voucheritems.FindAll(x => x.id > 0 && x.ledgerid == cashtransaction.ledgers[18].id &&
            (x.attributes != null && x.attributes.ledgertype == 11 && x.attributes.narration == "CUSTOMER PAYMENT")).Sum(x=>x.amount) !=
            voucher.voucheritems.FindAll(x => x.id < 0 && x.ledgerid == cashtransaction.ledgers[18].id &&
            (x.attributes != null && x.attributes.ledgertype == 11 && x.attributes.narration == "CUSTOMER PAYMENT")).Sum(x => x.amount)
            )
            {
                changes.AppendLine("Customer payment changed");
            }

            voucher.voucheritems.RemoveAll(x => x.id > 0 && x.ledgerid == cashtransaction.ledgers[18].id &&
            (x.attributes != null && x.attributes.ledgertype == 11 && x.attributes.narration == "CUSTOMER PAYMENT"));

            voucher.voucheritems.FindAll(x => x.ledgerid == cashtransaction.ledgers[18].id &&
            (x.attributes != null && x.attributes.ledgertype == 11 && x.attributes.narration == "CUSTOMER PAYMENT")).ForEach(x => x.id = 0);


            if(voucher.voucheritems.FindAll(x => x.id > 0 && x.ledgerid == cashtransaction.ledgers[18].id &&
            (x.attributes != null && x.attributes.ledgertype == 11 && x.attributes.narration == "CASH REFUND")).Sum(x=>x.amount) !=
            voucher.voucheritems.FindAll(x => x.id < 0 && x.ledgerid == cashtransaction.ledgers[18].id &&
            (x.attributes != null && x.attributes.ledgertype == 11 && x.attributes.narration == "CASH REFUND")).Sum(x => x.amount)
            )
            {
                changes.AppendLine("Cash refund changed");
            }
            voucher.voucheritems.RemoveAll(x => x.id > 0 && x.ledgerid == cashtransaction.ledgers[18].id &&
            (x.attributes != null && x.attributes.ledgertype == 11 && x.attributes.narration == "CASH REFUND"));

            voucher.voucheritems.FindAll(x => x.ledgerid == cashtransaction.ledgers[18].id &&
            (x.attributes != null && x.attributes.ledgertype == 11 && x.attributes.narration == "CASH REFUND")).ForEach(x => x.id = 0);

            if(changes.Length>0)
            {
                message = "Please save this voucher because following data changed " + changes.ToString();
            }
        }

        public class ReportLine
        {
            public DateTime date { get; set; }
            public string ledger { get; set; }
            public string narr { get; set; }
            public decimal credit
            {
                get;set;
            }
            public int typeid { get; set; }
            public int tagid { get; set; }
            public decimal debit
            {
                get; set;
            }
        }
    }

    public class LedgerBalance
    {
        public decimal debit { get; set; }
        public decimal credit { get; set; }
    }
    public class VoucherCreteria : Voucher
    {
        public bool onadvance { get; set; }
        public bool onvoucherledger { get; set; }
        public bool showdetail { get; set; }
        public long ledgerid { get; set; }
        public long secondaryid { get; set; }
        public DateTime fromdate { get; set; }
        public DateTime todate { get; set; }
        public List<long> ids { get; set; }
        public bool cashposting { get; set; }
        public int filtertype { get; set; }
        public decimal amountfrom { get; set; }
        public decimal amountto { get; set; }
        public long filterledgerid { get; set; }
        public string type { get; set; }
    }

    public class VoucherDetail : Voucher
    {
        [JsonIgnore]
        public long cempid { get; set; }
        [JsonIgnore]
        public long mempid { get; set; }
        public string cemp { get; set; }
        public string memp { get; set; }
    }
    public class OpeningCreteria : BaseEntity
    {
        public long locationid { get; set; }
        public long ledgerid { get; set; }
        public long companyid { get; set; }
        public DateTime voucherdate { get; set; }
        public Data.VoucherDataService.CashMode mode { get; set; }
    }

    public class FinanceAnalyseCreteria : BaseEntity
    {
        public List<long> ledgers { get; set; }
        public List<long> locations { get; set; }
        public List<long> types { get; set; }
        public List<long> vtypes { get; set; }
        public List<long> ptypes { get; set; }
        public long companyid { get; set; }
        public DateTime fromdate { get; set; }
        public DateTime todate { get; set; }
        public DateTime comparefromdate { get; set; }
        public DateTime comparetodate { get; set; }
        public long ledgerid { get; set; }
        public enum Modes
        {
            CompanySummary = 1,
            Compare = 2,
            Monthly = 3,
            Detail = 4,
            LocationSummary = 5,
            VoucherSummary = 6,
            VoucherDetail = 66,
            TaxSummary = 7,
            TaxDetails = 77,
        }
        public Modes modeid { get; set; }
    }

    public class FinanceAnalyse
    {
        public string title2 { get; set; }
        public string title1 { get; set; }
        public List<Detail> detail { get; set; }
        public List<Summary> summary { get; set; }
        public List<VoucherSummary> vsummary { get; set; }
        public List<MonthSummary> msummary { get; set; }
        public List<TDSSummary> tsummary { get; set; }
        public List<GSTSummary> gsummary { get; set; }
        public List<Compare> compare { get; set; }
        public List<string> mtitles { get; set; }
        public List<TaxSummaryGroup> taxsummary { get; set; }
        public List<TaxDetail> taxdetails { get; set; }
        public IEnumerable<int> taxvouchertypes { get; set; }
        public List<Summary.Value> chart { get; set; }

        public class Compare
        {
            public Compare()
            {
            }
            public Compare(int resultcount)
            {
                dr = new List<decimal>();
                cr = new List<decimal>();
                for (var i = 0; i < resultcount; i++)
                {
                    dr.Add(0);
                    cr.Add(0);
                }
            }
            public string name { get; set; }
            public long id { get; set; }
            public List<decimal> dr { get; set; }
            public List<decimal> cr { get; set; }
        }

        public class NameGroup
        {
            public long id { get; set; }
            public string name { get; set; }
        }
        public class Summary
        {
            public string name { get; set; }
            public long id { get; set; }
            public List<Value> values { get; set; }
            public class Value
            {
                public long lid { get; set; }
                public decimal dr { get; set; }
                public decimal cr { get; set; }
            }
        }

        public class TaxInventory
        {
            public string mode { get; set; }
            public decimal tax { get; set; }
            public long taxid { get; set; }
            public decimal taxpercentage { get; set; }
            public bool isinterstate { get; set; }
        }

        public class TaxSummaryGroup
        {
            public string name { get; set; }
            public decimal percentage { get; set; }
            public List<TaxSummary> items { get; set; }
        }
        public class TaxSummary
        {
            public long id { get; set; }
            public string name { get; set; }
            public string gstno { get; set; }
            public decimal percentage { get; set; }
            public int mode { get; set; }
            public int vouchertype { get; set; }
            public decimal cr { get; set; }
            public decimal dr { get; set; }
            public string source { get;  set; }
        }

        public class TaxDetail : TaxSummary
        {
            public string referenceno { get; set; }
            public string referencetype { get; set; }
            public DateTime referencedate { get; set; }
        }
        public class MonthSummary
        {
            public string name { get; set; }
            public long id { get; set; }
            public List<Value> values { get; set; }
            public class Value
            {
                public string myear { get; set; }
                public decimal dr { get; set; }
                public decimal cr { get; set; }
            }
        }

        public class GSTSummary : Detail
        {
            [JsonIgnore]
            public long createdby { get; set; }
            [JsonIgnore]
            public long modifiedby { get; set; }
            [JsonIgnore]
            public DateTime modifiedon { get; set; }

            public decimal igst { get; set; }
            public decimal cgst { get; set; }
            public decimal sgst { get; set; }
            public decimal input { get; set; }
            public decimal output { get; set; }
            public long voucherid
            {
                get
                {
                    return vid;
                }
            }
            [JsonIgnore]
            public DateTime createdon { get; set; }

        }

        public class TDSSummary : Detail
        {
		    [JsonIgnore]
            public long createdby { get; set; }
		    [JsonIgnore]			
            public long modifiedby { get; set; }
		    [JsonIgnore]			
            public DateTime modifiedon { get; set; }

            public decimal tds { get; set; }
            public decimal cess { get; set; }
            public decimal surcharge { get; set; }
            public long tdsledgerid { get; set; }
            public string tdsledgername { get; set; }
            public decimal tdspercentage { get; set; }
            public decimal surchargepercentage { get; set; }
            public decimal cesspercentage { get; set; }
            public string pan { get; set; }
            public string sname { get; set; }
            public long voucherid
            {
                get
                {
                    return vid;
                }
            }
		    [JsonIgnore]
            public DateTime createdon { get; set; }
        }

        public class VoucherSummary : VoucherDetail
        {
            public string crbname { get; set; }
            [JsonIgnore]
            public long crby { get; set; }
            public string aname { get; set; }
            public long vouchid { get; set; }
            public decimal txble { get; set; }
            public decimal tds { get; set; }
            public decimal cgst { get; set; }
            public decimal sgst { get; set; }
            public decimal igst { get; set; }
            public decimal tcs { get; set; }
            public decimal cess { get; set; }
            public decimal duty { get; set; }
            public decimal rnd { get; set; }
            public string tname { get; internal set; }
        }

        public class VoucherDetail : Detail
        {
            public long crby { get; set; }
            public string invno { get; set; }
            public DateTime invdate { get; set; }
            public string chequeno { get; set; }
            public string gstno { get; set; }
            public string narr { get; set; }
            public int ltype { get; internal set; }
        }
        public class Detail
        {
            [JsonIgnore]
            public long vvid { get; set; }
            [JsonIgnore]
            public long vid { get; set; }
            [JsonIgnore]
            public int taxmode { get; set; }
            [JsonIgnore]
            public int typeid { get; set; }
            public DateTime date { get; set; }
            public string no { get; set; }
            public int type { get; set; }
            public int ptype { get; set; }
            public string name { get; set; }
            public long id { get; set; }
            public decimal dr { get; set; }
            public decimal cr { get; set; }
            public decimal tp { get; set; }
            public long taxid { get;  set; }
            public long itaxid { get; set; }
            public long otaxid { get; set; }
        }
    }
    public class VoucherDisplayItem
    {
        public long id { get; set; }
        public long sledgerid { get; set; }
        public long ledgerid { get; set; }
        public long locationid { get; set; }
        public string no { get; set; }
        public string adjno { get; set; }
        public string uid { get; set; }
        public string ledger { get; set; }
        public string sourceledgername { get; set; }
        public long refid { get; set; }
        public bool ispossitive { get { return amount > 0; } }
        public int ledgertype { get; set; }
        public long purchasetypeid { get; set; }
        public int vouchertype { get; set; }
        public string vouchername { get; set; }
        public decimal amount { get; set; }
        public string crdr { get; set; }
        public string primcrdr { get; set; }
        public bool isheader { get; set; }
        public bool hasgst { get; set; }
        public bool hastds { get; set; }
        public int index { get; set; }
        public DateTime date { get; set; }
        [JsonIgnore]
        public List<VoucherDisplayItem> ownerlist { get; set; }
        public long vouchertypeid { get; set; }
        public string invoiceno { get; set; }
        public DateTime invoicedate { get; set; }
        public DateTime bankdate { get; set; }
        public string narration { get; set; }
        public string bankreference { get; set; }
        public string banktransfermode { get; set; }
        [JsonIgnore]
        public long createdby { get;  set; }
        [JsonIgnore]
        public long modifiedby { get;  set; }
        [JsonIgnore]
        public DateTime modifiedon { get;  set; }
        [JsonIgnore]
        public DateTime createdon { get;  set; }
        public string cuser { get;  set; }
        public string muser { get;  set; }
        public bool isvoucher { get;  set; }
        [JsonIgnore]
        public bool iscustomer { get; set; }
    }
    public class LedgerListCreteria : BaseEntity
    {
        public long ledgerid { get; set; }
        public long locationid { get; set; }
        public DateTime fromdate { get; set; }
        public DateTime todate { get; set; }
        public long companyid { get; set; }
        public int reporttypeid { get; set; }
        public int sourceid { get; set; }
        public string ageranges { get; set; }
        public string GetAgerangesquery()
        {

            var rangelist = GetAgeList();
            if (rangelist.Count == 0) return "";
            var field = "(@currentdate-transactionon::date)";
            var typeCast = "::integer";
            System.Text.StringBuilder conditionlist = new System.Text.StringBuilder();
            foreach (var rng in rangelist)
            {
                if (rng.from == 0)
                {
                    conditionlist.AppendLine("when " + field + "<" + rng.to + typeCast + " then '" + rng.to + "'");
                }
                else if (rng.to == 0)
                {
                    conditionlist.AppendLine("when " + field + ">=" + rng.from + typeCast + " then '" + rng.from + "'");
                }
                else
                {
                    conditionlist.AppendLine("when " + field + " >= " + rng.from + typeCast + " AND " + field + "<" + rng.to + typeCast + " then '" + rng.from + " - " + rng.to + "'");
                }
            }
            return ", case " + conditionlist.ToString() + " end ";
        }

        public List<InfyPOS.Data.SupplierpaymentDataService.RangeValue> GetAgeList()
        {
            List<InfyPOS.Data.SupplierpaymentDataService.RangeValue> rangeValues = new List<InfyPOS.Data.SupplierpaymentDataService.RangeValue>();
            if (!string.IsNullOrEmpty(ageranges) && reporttypeid >= 2)
            {
                var ranges = ageranges.Split('-');
                if (ranges.Length == 1)
                {
                    ranges = ageranges.Split(',');
                    int start = int.Parse(ranges[0]);
                    var current = start;
                    if (start > 0)
                    {
                        rangeValues.Add(new InfyPOS.Data.SupplierpaymentDataService.RangeValue()
                        {
                            from = 0,
                            to = start,
                            value = start
                        });
                    }
                    for (int i = 1; i < ranges.Length; i++)
                    {
                        start = int.Parse(ranges[i]);
                        rangeValues.Add(new InfyPOS.Data.SupplierpaymentDataService.RangeValue()
                        {
                            from = current,
                            to = start,
                            value = start
                        });
                        current = start;
                    }
                }
                else
                {
                    int start = int.Parse(ranges[0]);
                    int increment = int.Parse(ranges[1]);
                    int end = int.Parse(ranges[2]);
                    var current = start;
                    rangeValues.Add(new InfyPOS.Data.SupplierpaymentDataService.RangeValue()
                    {
                        from = 0,
                        to = start,
                        value = start
                    });

                    while (current < end)
                    {
                        rangeValues.Add(new InfyPOS.Data.SupplierpaymentDataService.RangeValue()
                        {
                            from = current,
                            to = current + increment,
                            value = current + increment
                        });
                        current += increment;
                    }
                }
            }
            return rangeValues;
        }
    }
    public class PayableReceivableLedger
    {
        public DateTime transactionon { get; set; }
        public string displayname { get; set; }
        public string createdby { get; set; }
        public string city { get; set; }
        public int bills { get; set; }
        public decimal amount { get; set; }
        public decimal adjusted { get; set; }
        public string referencetype { get; set; }
        public string referenceno { get; set; }
        public string referenceinvoiceno { get; set; }
        public DateTime referenceinvoicedate { get; set; }
        public string age { get; set; }
        public long locationid { get; set; }
        public long voucherid { get; set; }
    }

    public class VoucherPrint : Voucher
    {
        public List<VoucherItemPrint> transactions { get; set; }
        public List<VoucherItemPrint> voucheritem { get; set; }
        public Ledger ledger { get; set; }
        public Company company { get; set; }
        public CompanyLocation location { get; set; }
        public VoucherItemPrint parentledger { get; set; }
        public List<VoucherItemPrint> childledgers { get; set; }
        public bool hasinvoiceinfo
        {
            get
            {
                return attributes != null &&
                    !string.IsNullOrEmpty(attributes.invoiceno) &&
                    attributes.invoicedate.Year > 2000;
            }
        }
        public bool vt_purchase
        {
            get
            {
                return vouchertype == 3;
            }
        }
        public bool vt_payment
        {
            get
            {
                return vouchertype == 1;
            }
        }
        public bool vt_sales
        {
            get
            {
                return vouchertype == 4;
            }
        }
        public bool vt_receipt
        {
            get
            {
                return vouchertype == 2;
            }
        }
        public bool vt_advance
        {
            get
            {
                return vouchertype == 14;
            }
        }
        public bool vt_crnote
        {
            get
            {
                return vouchertype == 5;
            }
        }
        public bool vt_drnote
        {
            get
            {
                return vouchertype == 6;
            }
        }
        public string displaytitle { get; set; }
        public string purchasetype { get; set; }
        [JsonIgnore]
        public bool haspurchasetype
        {
            get
            {
                return !string.IsNullOrEmpty(purchasetype);
            }
        }
        [JsonIgnore]
        public bool ispo { get; set; }
        public Employee employee { get; set; }
        public string enteredby { get; set; }
        public string alteredby { get; set; }
        public List<VoucherItemPrint> taxledgers { get; set; }
        public List<VoucherItemPrint> taxableledgers { get; set; }
        public VoucherItemPrint tdsledger { get; set; }
        public VoucherItemPrint tcsledger { get; set; }
        public bool hastds
        {
            get
            {
                return tdsledger != null;
            }
        }
        public bool hastcs
        {
            get
            {
                return tcsledger != null;
            }
        }
        public decimal gross { get; set; }
        public decimal igst { get; set; }
        public decimal cgst { get; set; }
        public decimal sgst { get; set; }
        public decimal tds { get; set; }
        public decimal tcs { get; set; }
        public decimal payable { get; set; }
        public decimal netpayable { get; set; }
        public string numberinword { get; set; }
        public decimal rounding { get; set; }
        public int bankid { get; internal set; }
        public long adjustmentledgertypeid { get; internal set; }
        public Ledger partyledger { get; internal set; }
        public bool isbankpayment { get; internal set; }
        public bool iscashpayment { get; internal set; }

        public class VoucherItemPrint
        {
            public string name { get; set; }
            public string hsncode { get; set; }
            public decimal credit { get; set; }
            public decimal debit { get; set; }
            public decimal amount
            {
                get
                {
                    return credit + debit;
                }
            }
            public bool hasgst { get; set; }
            public bool hastds { get; set; }
            public bool hastcs { get; set; }
            public int taxmode { get; set; }
            public int typeid { get; set; }
            public decimal rate { get; set; }
            public decimal qty { get; set; }
            public decimal taxpercentage { get; set; }
            public decimal taxable { get; set; }
            public bool isparty { get; internal set; }
            public long supplierid { get; internal set; }
            public long bankid { get; internal set; }
            public long ledgerid { get; internal set; }
            public Ledger ledger { get; internal set; }
        }
    }

    public class vouchertype
    {
        public long id { get; set; }
        public string name { get; set; }
    }

}
