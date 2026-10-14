using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class Bankchequetransaction : BaseEntity
    {
        public long id { get; set; }
        //Direct , Supplier Payment, Voucher Payment, Voucher Receipt
        public string sourcetype { get; set; }
        public long companyid { get; set; }
        public long locationid { get; set; }
        public long employeeid { get; set; }
        public long sourceid { get; set; }
        public long bankid { get; set; }
        public string bankname { get; set; }
        public DateTime chequedate { get; set; }
        public string chequeno { get; set; }
        public decimal amount { get; set; }
        public string accountname { get; set; }
        public DateTime createdon { get; set; }
        public DateTime modifiedon { get; set; }
        //Open,Bounce, Cancelled,Rejected
        public int status { get; set; }
        public bool isactive { get; set; }
        public long createdby { get; set; }
        public long modifiedby { get; set; }
        public int version { get; set; }
        public string bankaccountno { get; set; }
        public string bankifsc { get; set; }
        public string bankbranch { get; set; }
        public DateTime entrydate { get; set; }

        public string ledgername { get; set; }
        public string referenceno { get; set; }
        public long sourcesubid { get; set; }
        

        public CheckSource checksource { get; set; }

        public ChequeStatus brsstatus { get; set; }
        public enum CheckSource
        {
            None,
            Supplier,
            Transport,
            Customer,
            Voucher,
            Agent,
            VoucherItem
        }
        public enum ChequeStatus
        {
            None,
            Bounced,
            Completed,
            Cancelled,
            Rejected
        }
    }
    public class BankchequeSummary
    {
        public Company company { get; set; }
        public CompanyLocation location { get; set; }
        public Referencelist bankledger { get; set; }
        public string ChequeNo { get; set; }
        public DateTime ChequeDate { get; set; }
        public List<Bankchequetransaction> items { get; set; }
    }
    public class BankchequetransactionCreteria : Bankchequetransaction
    {
        public DateTime entryfromdate { get; set; }
        public DateTime entrytodate { get; set; }
        public DateTime fromdate { get; set; }
        public DateTime todate { get; set; }
        public long reportid { get; set; }
        public List<long> ids { get; set; }
    }
    public class BRSCreteria : BaseEntity
    {
        public List<BRSLedger> items { get; set; }
        public DateTime fromdate { get; set; }
        public DateTime todate { get; set; }
        public int statusid { get; set; }
        public long companyid { get; set; }
        public long bankid { get; set; }
        public bool supplierpayment { get; set; }
        public bool transportpayment { get; set; }
        public bool customerpayment { get; set; }
        public bool agentpayment { get; set; }
        public bool vouchers { get; set; }
    }

    public class BRSSummary
    {
        public decimal opening { get; set; }
        public List<BRSLedger> transactions { get; set; }
        public decimal closing { get; set; }
    }
    public class BRSLedger
    {
        public long id { get; set; }
        public long refid { get; set; }
        public long bankid { get; set; }
        public long companyid { get; set; }
        public int transactiontype { get; set; }
        public int brsstatus { get; set; }
        public string description { get; set; }
        public string remark { get; set; }
        public string age
        {
            get
            {
                if (instrumentdate != DateTime.MinValue &&
                    instrumentdate.Year > 2000)
                    return DateTime.Now.Date.Subtract(instrumentdate).Days.ToString("F0") + " Days";

                return DateTime.Now.Date.Subtract(transactionon).Days.ToString("F0") + " Days";
            }
        }
        public int iage
        {
            get
            {
                if (instrumentdate != DateTime.MinValue &&
                    instrumentdate.Year > 2000)
                    return DateTime.Now.Date.Subtract(instrumentdate).Days;

                return DateTime.Now.Date.Subtract(transactionon).Days;
            }
        }
        public DateTime instrumentdate { get; set; }
        public DateTime transactionon { get; set; }
        public string transactioncode { get; set; }
        public DateTime realisationon { get; set; }
        public decimal issued { get; set; }
        public decimal received { get; set; }
    }
}
