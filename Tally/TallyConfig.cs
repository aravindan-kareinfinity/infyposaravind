
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using InfyPOS.Common;
using InfyPOS.Tally;


namespace InfyPOS.Models
{
    public class TallyConfig
    {
        public class Purchase
        {
            public TallyInterfaceConfig config { get; set; }
            public Purchase(TallyInterfaceConfig config)
            {
                this.config = config;
            }

            private string SyncName(string source, TallyDataservice.ReturnInfo invoice, Tally.TallyDataservice.TaxSummary taxsummary)
            {
                source = source.Replace("[STATE]", invoice.statename);
                source = source.Replace("[BRANCHCODE]", invoice.branchcode);
                source = source.Replace("[BRANCHNAME]", invoice.branchname);
                source = source.Replace("[CUSTOMER]", invoice.customername);
                source = source.Replace("[CITY]", invoice.cityname);
                source = source.Replace("[SUPPLIERCOMPANY]", invoice.suppliercompany);
                source = source.Replace("[SUPPLIERGROUP]", invoice.suppliergroup);
                source = source.Replace("[SUPPLIERNAME]", invoice.suppliername);
                source = source.Replace("[AGENT]", invoice.agentname);
                source = source.Replace("[REGION]", invoice.isinterstatesale ? config.configuration.Region_Interstate : config.configuration.Region_Local);
                source = source.Replace("[REGIONORCITY]", invoice.isinterstatesale ? invoice.cityname : config.configuration.Region_Local);

                if (taxsummary != null)
                {
                    if (Math.Floor(taxsummary.percentage) == Math.Ceiling(taxsummary.percentage))
                        source = source.Replace("[TAX-PERCENTAGE]", taxsummary.percentage.ToString("N0"));
                    else
                        source = source.Replace("[TAX-PERCENTAGE]", taxsummary.percentage.ToString("N1"));
                }
                return source;
            }

            public string ParentLedger(int type)
            {
                switch (type)
                {
                    case 1:
                        return "Capital Account";
                    case 2:
                        return "Fixed Assets";
                    case 3:
                        return "Current Liablities";
                    case 4:
                        return "Direct Incomes";
                    case 5:
                        return "Direct Expenses";
                    case 6:
                        return "Cash-in-Hand";
                    case 7:
                        return "Bank Accounts";
                    case 8:
                        return "Stock-in-Hand";
                    case 9:
                        return "Indirect Incomes";
                    case 10:
                        return "Indirect Expenses";
                    case 11:
                        return "Sundry Debtors";
                    case 12:
                        return "Sundry Creditors";
                    case 13:
                        return "Cash-in-Hand";
                    case 14:
                        return "Sales Accounts";
                    case 15:
                        return "Purchase Account";
                    case 16:
                        return "Duties & Taxes";
                    case 17:
                        return "Direct Expenses";
                    case 18:
                        return "Indirect Expenses";

                }
                return "Indirect Expenses";

            }
            public string SyncName(string source, Ledger invoice)
            {
                if (string.IsNullOrEmpty(source))
                {
                    if (invoice.parentinfo != null &&
                        invoice.parentinfo.levels != null)
                    {
                        List<string> levels = new List<string>(invoice.parentinfo.levels);
                        levels[0] = ParentLedger(invoice.typeid);
                        source = String.Join("~", levels) + "~" + invoice.displayname;
                    }
                    else
                    {
                        source = ParentLedger(invoice.typeid) + "~" + invoice.displayname;
                    }
                }
                return source.Replace("[LEDGER]", invoice.displayname);
            }

            public string SyncName(string source, Tally.TallyDataservice.CustomerPaymentInfo invoice)
            {
                source = source.Replace("[STATE]", invoice.statename);
                source = source.Replace("[CITY]", invoice.cityname);
                source = source.Replace("[BANK]", invoice.bankname);
                source = source.Replace("[CUSTOMER]", invoice.customername);
                source = source.Replace("[REGION]", invoice.isinterstatesale ? config.configuration.Region_Interstate : config.configuration.Region_Local);
                source = source.Replace("[REGIONORCITY]", invoice.isinterstatesale ? invoice.cityname : config.configuration.Region_Local);
                return source;
            }

            public string SyncName(string source, Tally.TallyDataservice.PaymentInfo invoice)
            {
                source = source.Replace("[STATE]", invoice.statename);
                source = source.Replace("[CITY]", invoice.cityname);
                source = source.Replace("[TDSPERCENTAGE]", invoice.tdspercentage.ToString("N2"));
                source = source.Replace("[BANK]", invoice.bankname);
                source = source.Replace("[SUPPLIERCOMPANY]", invoice.suppliercompany);
                source = source.Replace("[SUPPLIERGROUP]", invoice.suppliergroup);
                source = source.Replace("[SUPPLIERNAME]", invoice.suppliername);
                source = source.Replace("[REGION]", invoice.isinterstatesale ? config.configuration.Region_Interstate : config.configuration.Region_Local);
                source = source.Replace("[REGIONORCITY]", invoice.isinterstatesale ? invoice.cityname : config.configuration.Region_Local);
                return source;
            }

            public string SyncName(string source, Tally.TallyDataservice.InvoiceInfo invoice, Tally.TallyDataservice.TaxSummary taxsummary)
            {
                source = source.Replace("[STATE]", invoice.statename);
                source = source.Replace("[BRANCH]", invoice.locationcode);
                source = source.Replace("[BRANCHCODE]", invoice.locationcode);
                source = source.Replace("[BRANCHNAME]", invoice.locationname);
                source = source.Replace("[CUSTOMER]", invoice.customername);
                source = source.Replace("[CITY]", invoice.cityname);
                source = source.Replace("[INVOICENO]", EncodeXML(invoice.invoiceno));
                source = source.Replace("[ENTRYNO]", EncodeXML(invoice.entryno));
                source = source.Replace("[SUPPLIERCOMPANY]", invoice.suppliercompany);
                source = source.Replace("[SUPPLIERGROUP]", invoice.suppliergroup);
                source = source.Replace("[SUPPLIERNAME]", invoice.suppliername);
                source = source.Replace("[AGENT]", invoice.agentname);
                source = source.Replace("[REGION]", invoice.isinterstatesale ? config.configuration.Region_Interstate : config.configuration.Region_Local);
                source = source.Replace("[REGIONORCITY]", invoice.isinterstatesale ? invoice.cityname : config.configuration.Region_Local);
                if (taxsummary != null)
                {
                    if (Math.Floor(taxsummary.percentage) == Math.Ceiling(taxsummary.percentage))
                        source = source.Replace("[TAX-PERCENTAGE]", taxsummary.percentage.ToString("N0"));
                    else
                        source = source.Replace("[TAX-PERCENTAGE]", taxsummary.percentage.ToString("N1"));
                }
                return source;
            }

            public string SyncName(string source, Tally.TallyDataservice.BillInfo invoice, Tally.TallyDataservice.BillInfo.TaxInfo taxsummary)
            {
                if (string.IsNullOrEmpty(invoice.CustomerName)) invoice.CustomerName = "CASH";
                source = source.Replace("[BRANCHNAME]", invoice.LocationName);
                source = source.Replace("[BRANCHCODE]", invoice.Location);
                source = source.Replace("[CUSTOMER]", invoice.CustomerName);
                source = source.Replace("[CUSTOMERNAME]", invoice.CustomerName);
                source = source.Replace("[BILLINGNAME]", string.IsNullOrEmpty(invoice.billingname) ? invoice.CustomerName:invoice.billingname);
                source = source.Replace("[CUSTOMERGROUP]", invoice.customergroup);
                source = source.Replace("[AGENT]", invoice.agentname);
                source = source.Replace("[AREA]", invoice.CustomerArea);
                source = source.Replace("[OPTIONALAREA]", invoice.CustomerArea != "DEFAULT" ? invoice.CustomerArea : "");
                source = source.Replace("[CITY]", invoice.Cityname);
                source = source.Replace("[BRANCH]", invoice.Location);

                source = source.Replace("[SALEQTY]", invoice.qty.ToString("F0"));
                source = source.Replace("[CUSTOMERGST]", invoice.Gstno);
                source = source.Replace("[BILLDATE]", invoice.Date.ToString("dd-MM-yyyy"));
                source = source.Replace("[RETURNBILLNO]", invoice.ReturnBillno);
                source = source.Replace("[BILLNO]", invoice.BillNo);

                if (taxsummary != null)
                {
                    if (Math.Floor(taxsummary.Percentage) == Math.Ceiling(taxsummary.Percentage))
                        source = source.Replace("[TAX-PERCENTAGE]", taxsummary.Percentage.ToString("N0"));
                    else
                        source = source.Replace("[TAX-PERCENTAGE]", taxsummary.Percentage.ToString("N1"));
                }
                return source;
            }

            private string SyncName(string bank, TallyDataservice.CardSales card)
            {
                return bank.Replace("[BANK]", card.bank).Replace("[UPI]", card.upi);
            }

            public InfyPOS.Tally.TallyXML.Voucher CreateVoucher(Voucher invoice, List<LedgerDetail> ledgers)
            {
                InfyPOS.Tally.TallyXML.Voucher voucher = new InfyPOS.Tally.TallyXML.Voucher();
                voucher.ledgers = new List<Tally.TallyXML.LedgerVal>();
                voucher.type = config.configuration.vouchertype(invoice.TallyVoucherType(), config.configuration.vouchertype_voucher);
                voucher.narration = invoice.narration;
                voucher.voucherkey = "TP-" + invoice.id + "-" + invoice.adjustmentledgerid + "-" + invoice.voucherno;
                voucher.date = invoice.voucherdate;
                voucher.vouchernumber = voucher.voucherkey;
                var pettycash = ledgers.Find(x => x.id == invoice.adjustmentledgerid);

                voucher.addtionalAttribs = string.Format(@"<PARTYLEDGERNAME>{0}</PARTYLEDGERNAME>
                                                            <PERSISTEDVIEW>Accounting Voucher View</PERSISTEDVIEW>
                                                            <HASCASHFLOW>{2}</HASCASHFLOW>
                                                            <EFFECTIVEDATE>{1}</EFFECTIVEDATE>", EncodeXML(pettycash.displayname),
                                                           invoice.voucherdate.ToString("yyyyMMdd"),
                                                           (pettycash.typeid == 6 ||
                                                           pettycash.typeid == 13) ? "Yes" : "No");

                InfyPOS.Tally.TallyXML.LedgerVal customerledger = new InfyPOS.Tally.TallyXML.LedgerVal(SyncName(config.configuration.vouchertype_pettycash, pettycash),
                    invoice.amount, !invoice.adjustmentcredit);
                customerledger.addionalAttribs =
                    String.Format(@"<LEDGERFROMITEM>No</LEDGERFROMITEM>
                       <ISPARTYLEDGER>{0}</ISPARTYLEDGER>", invoice.adjustmentcredit ? "Yes" : "No");

                voucher.ledgers.Add(customerledger);

                foreach (var eledger in invoice.voucheritems)
                {
                    var expense = ledgers.Find(x => x.id == eledger.ledgerid);

                    InfyPOS.Tally.TallyXML.LedgerVal discountledger = new InfyPOS.Tally.TallyXML.LedgerVal(SyncName(
                        config.configuration.PettyExpenses, expense),
                        Math.Abs(eledger.amount), invoice.adjustmentcredit);

                    discountledger.IsNeedBankAllocation = false;
                    discountledger.ChildNodes =
                            String.Format(@"<LEDGERFROMITEM>No</LEDGERFROMITEM>
                       <ISPARTYLEDGER>{0}</ISPARTYLEDGER>
                       <ISLASTDEEMEDPOSITIVE>No</ISLASTDEEMEDPOSITIVE>",
                       invoice.adjustmentcredit ? "No" : "Yes");

                    voucher.ledgers.Add(discountledger);
                }
                return voucher;
            }
            public InfyPOS.Tally.TallyXML.Voucher CreatePettyCashVoucher(Voucher invoice, List<LedgerDetail> ledgers)
            {
                InfyPOS.Tally.TallyXML.Voucher voucher = new InfyPOS.Tally.TallyXML.Voucher();
                voucher.ledgers = new List<Tally.TallyXML.LedgerVal>();
                voucher.type = config.configuration.vouchertype("Payment", config.configuration.vouchertype_pettycash);
                voucher.narration = "Petty Cash " + invoice.voucherno + "@" + invoice.voucherdate.ToString("dd-MM-yyyy");
                voucher.voucherkey = "TP-" + invoice.id + "-" + invoice.adjustmentledgerid + "-" + invoice.voucherno;
                voucher.date = invoice.voucherdate;
                voucher.vouchernumber = voucher.voucherkey;
                var pettycash = ledgers.Find(x => x.id == invoice.adjustmentledgerid);

                voucher.addtionalAttribs = string.Format(@"<PARTYLEDGERNAME>{0}</PARTYLEDGERNAME>
                                                            <PERSISTEDVIEW>Accounting Voucher View</PERSISTEDVIEW>
                                                            <HASCASHFLOW>Yes</HASCASHFLOW>
                                                            <EFFECTIVEDATE>{1}</EFFECTIVEDATE>", EncodeXML(pettycash.displayname),
                                                           invoice.voucherdate.ToString("yyyyMMdd"));

                InfyPOS.Tally.TallyXML.LedgerVal customerledger = new InfyPOS.Tally.TallyXML.LedgerVal(SyncName(config.configuration.vouchertype_pettycash, pettycash),
                    invoice.amount, false);
                customerledger.addionalAttribs =
                    @"<LEDGERFROMITEM>No</LEDGERFROMITEM>
                       <ISPARTYLEDGER>Yes</ISPARTYLEDGER>";

                voucher.ledgers.Add(customerledger);

                foreach (var eledger in invoice.voucheritems)
                {
                    var expense = ledgers.Find(x => x.id == eledger.ledgerid);

                    InfyPOS.Tally.TallyXML.LedgerVal discountledger = new InfyPOS.Tally.TallyXML.LedgerVal(SyncName(
                        config.configuration.PettyExpenses, expense),
                        Math.Abs(eledger.amount), true);

                    discountledger.IsNeedBankAllocation = false;
                    discountledger.ChildNodes =
                            @"<LEDGERFROMITEM>No</LEDGERFROMITEM>
                       <ISPARTYLEDGER>No</ISPARTYLEDGER>
                       <ISLASTDEEMEDPOSITIVE>No</ISLASTDEEMEDPOSITIVE>";

                    voucher.ledgers.Add(discountledger);
                }
                return voucher;
            }
            public InfyPOS.Tally.TallyXML.Voucher CreateVoucher(Tally.TallyDataservice.CustomerPaymentInfo invoice)
            {
                InfyPOS.Tally.TallyXML.Voucher voucher = new InfyPOS.Tally.TallyXML.Voucher();
                voucher.ledgers = new List<Tally.TallyXML.LedgerVal>();
                voucher.type = config.configuration.vouchertype("Bank Payment", config.configuration.vouchertype_customerpayment);
                voucher.narration = "Payment " + invoice.receiptno + "@" + invoice.paidon.ToString("dd-MM-yyyy");
                voucher.voucherkey = "TP-" + invoice.id + "-" + invoice.customerid + "-" + invoice.receiptno;
                voucher.date = invoice.paidon;
                voucher.vouchernumber = voucher.voucherkey;

                voucher.addtionalAttribs = string.Format(@"<PARTYLEDGERNAME>{0}</PARTYLEDGERNAME>
                                                            <PERSISTEDVIEW>Accounting Voucher View</PERSISTEDVIEW>
                                                            <HASCASHFLOW>Yes</HASCASHFLOW>
                                                            <EFFECTIVEDATE>{1}</EFFECTIVEDATE>", EncodeXML(invoice.customername),
                                                           invoice.paidon.ToString("yyyyMMdd"));

                InfyPOS.Tally.TallyXML.LedgerVal customerledger = new InfyPOS.Tally.TallyXML.LedgerVal(SyncName(config.configuration.Customer, invoice),
                    invoice.allocations.Sum(e => e.discount + e.paid), false);
                customerledger.statename = !config.configuration.Data_Sales_Address ? invoice.statename : "";
                customerledger.address = !config.configuration.Data_Sales_Address ? invoice.address : "";
                customerledger.cityname = !config.configuration.Data_Sales_Address ? invoice.cityname : "";
                customerledger.gstno = invoice.gstno;
                customerledger.addionalAttribs =
                    @"<LEDGERFROMITEM>No</LEDGERFROMITEM>
                       <ISPARTYLEDGER>No</ISPARTYLEDGER>
                       <ISLASTDEEMEDPOSITIVE>Yes</ISLASTDEEMEDPOSITIVE>";

                foreach (var item in invoice.allocations)
                {
                    var allocation = customerledger.AddAllocation("CREDIT - " + item.billno + "@" + item.billdate.ToString("dd-MM-yyyy"), item.paid);
                    if (item.discount != 0)
                        allocation = customerledger.AddAllocation(item.billno, item.discount);
                }
                voucher.ledgers.Add(customerledger);

                InfyPOS.Tally.TallyXML.LedgerVal bankledger = new InfyPOS.Tally.TallyXML.LedgerVal(SyncName(
                    invoice.paymentmode != "Cash" ? config.configuration.Payment_Bank : config.configuration.Payment_Cash, invoice),
                    invoice.allocations.Sum(e => e.paid), true);

                bankledger.statename = invoice.statename;
                bankledger.cityname = invoice.cityname;

                switch (invoice.paymentmode)
                {
                    case "NEFT/RTGS":
                        if (!string.IsNullOrEmpty(config.configuration.paymode_neft))
                            bankledger.transactionmode = config.configuration.paymode_neft;
                        bankledger.IsNeedBankAllocation = true;
                        break;
                    case "Cheque/DD":
                        if (!string.IsNullOrEmpty(config.configuration.paymode_cheque))
                            bankledger.transactionmode = config.configuration.paymode_cheque;
                        bankledger.IsNeedBankAllocation = true;
                        break;
                    case "Cash":
                        if (!string.IsNullOrEmpty(config.configuration.paymode_cash))
                            bankledger.transactionmode = config.configuration.paymode_cash;
                        bankledger.IsNeedBankAllocation = false;
                        break;
                    case "Card":
                        if (!string.IsNullOrEmpty(config.configuration.paymode_card))
                            bankledger.transactionmode = config.configuration.paymode_card;
                        bankledger.IsNeedBankAllocation = true;
                        break;
                    case "UPI":
                        if (!string.IsNullOrEmpty(config.configuration.paymode_upi))
                            bankledger.transactionmode = config.configuration.paymode_upi;
                        bankledger.IsNeedBankAllocation = true;
                        break;
                }

                bankledger.ChildNodes =
                        @"<LEDGERFROMITEM>No</LEDGERFROMITEM>
                       <ISPARTYLEDGER>Yes</ISPARTYLEDGER>
                       <ISLASTDEEMEDPOSITIVE>Yes</ISLASTDEEMEDPOSITIVE>";

                voucher.ledgers.Add(bankledger);

                if (invoice.allocations.Sum(e => e.discount) != 0)
                {
                    InfyPOS.Tally.TallyXML.LedgerVal discountledger = new InfyPOS.Tally.TallyXML.LedgerVal(SyncName(
                        config.configuration.CustomerPaymentDiscount, invoice),
                        invoice.allocations.Sum(e => e.discount), true);

                    discountledger.statename = invoice.statename;
                    discountledger.cityname = invoice.cityname;
                    discountledger.IsNeedBankAllocation = false;
                    discountledger.ChildNodes =
                            @"<LEDGERFROMITEM>No</LEDGERFROMITEM>
                       <ISPARTYLEDGER>Yes</ISPARTYLEDGER>
                       <ISLASTDEEMEDPOSITIVE>Yes</ISLASTDEEMEDPOSITIVE>";

                    voucher.ledgers.Add(discountledger);
                }
                return voucher;
            }
            public InfyPOS.Tally.TallyXML.Voucher CreateVoucher(Tally.TallyDataservice.PaymentInfo invoice)
            {
                InfyPOS.Tally.TallyXML.Voucher voucher = new InfyPOS.Tally.TallyXML.Voucher();
                voucher.ledgers = new List<Tally.TallyXML.LedgerVal>();
                voucher.type = config.configuration.vouchertype("Bank Payment", config.configuration.vouchertype_supplierpayment);
                voucher.narration = "Payment " + invoice.receiptno + "@" + invoice.paymentdate.ToString("dd-MM-yyyy");
                voucher.voucherkey = "TP-" + invoice.id + "-" + invoice.suppliercompanyid;// + "-" + invoice.receiptno;
                voucher.date = invoice.paymentdate;
                voucher.vouchernumber = voucher.voucherkey;

                var suppliername = invoice.suppliername;
                if(config.configuration.Supplier.Contains("SUPPLIERCOMPANY"))
                {
                    suppliername = invoice.suppliercompany;
                }
                if (config.configuration.Supplier.Contains("SUPPLIERGROUP"))
                {
                    suppliername = invoice.suppliergroup;
                }
                voucher.addtionalAttribs = string.Format(@"<PARTYLEDGERNAME>{0}</PARTYLEDGERNAME>
                                                            <PERSISTEDVIEW>Accounting Voucher View</PERSISTEDVIEW>
                                                            <HASCASHFLOW>Yes</HASCASHFLOW>
                                                            <EFFECTIVEDATE>{1}</EFFECTIVEDATE>
                                                            <ISCOSTCENTRE>{2}</ISCOSTCENTRE>
                                                            <COSTCENTRENAME>{3}</COSTCENTRENAME>", 
                                                            EncodeXML(suppliername),
                                                            invoice.paymentdate.ToString("yyyyMMdd"),
                                                            config.configuration.Data_Purchase_Datacentre ? "Yes" : "No",
                                                            config.configuration.Data_Purchase_Datacentre ? invoice.locationname : "");



                InfyPOS.Tally.TallyXML.LedgerVal supplierLedger = new InfyPOS.Tally.TallyXML.LedgerVal(SyncName(config.configuration.Supplier, invoice),
                    invoice.allocations.Sum(e => e.paidamount) + invoice.rounding, true);
                supplierLedger.statename = !config.configuration.Data_Purchase_Address ? invoice.statename : "";
                supplierLedger.cityname = !config.configuration.Data_Purchase_Address ? invoice.cityname : "";
                supplierLedger.address = !config.configuration.Data_Purchase_Address ? invoice.address : "";
                supplierLedger.gstno = invoice.gstno;
                supplierLedger.addionalAttribs =
                    @"<LEDGERFROMITEM>No</LEDGERFROMITEM>
                       <ISPARTYLEDGER>No</ISPARTYLEDGER>
                       <ISLASTDEEMEDPOSITIVE>Yes</ISLASTDEEMEDPOSITIVE>";


                foreach (var item in invoice.allocations)
                {
                    if (item.isdebitnote)
                    {
                        supplierLedger.AddAllocation("DN/" + item.invoiceno, item.paidamount).IsReturn = item.isdebitnote;
                    }
                    else
                    {
                        //"DEBIT NOTE /" + invoice.returnno + "-" + invoice.invoiceno
                        supplierLedger.AddAllocation(item.invoiceno, item.paidamount).IsReturn = item.isdebitnote;
                    }
                }
                voucher.ledgers.Add(supplierLedger);

                InfyPOS.Tally.TallyXML.LedgerVal bankledger = new InfyPOS.Tally.TallyXML.LedgerVal(SyncName(
                    invoice.paymentmode != "Cash" ? config.configuration.Payment_Bank : config.configuration.Cash, invoice),
                    invoice.allocations.Sum(e => e.paidamount - (e.discountamount)) - invoice.tdsamount, false);

                switch (invoice.paymentmode)
                {
                    case "NEFT/RTGS":
                        if (!string.IsNullOrEmpty(config.configuration.paymode_neft))
                            bankledger.transactionmode = config.configuration.paymode_neft;
                        bankledger.IsNeedBankAllocation = true;
                        break;
                    case "Cheque/DD":
                        if (!string.IsNullOrEmpty(config.configuration.paymode_cheque))
                            bankledger.transactionmode = config.configuration.paymode_cheque;
                        bankledger.IsNeedBankAllocation = true;
                        break;
                    case "Cash":
                        if (!string.IsNullOrEmpty(config.configuration.paymode_cash))
                            bankledger.transactionmode = config.configuration.paymode_cash;
                        bankledger.IsNeedBankAllocation = false;
                        break;
                    case "Card":
                        if (!string.IsNullOrEmpty(config.configuration.paymode_card))
                            bankledger.transactionmode = config.configuration.paymode_card;
                        bankledger.IsNeedBankAllocation = true;
                        break;
                    case "UPI":
                        if (!string.IsNullOrEmpty(config.configuration.paymode_upi))
                            bankledger.transactionmode = config.configuration.paymode_upi;
                        bankledger.IsNeedBankAllocation = true;
                        break;
                }

                bankledger.statename = invoice.statename;
                bankledger.cityname = invoice.cityname;
                bankledger.chequeno = invoice.chequeno;
                bankledger.instrumentdate = invoice.instrumentdate;
                bankledger.ChildNodes =
                        @"<LEDGERFROMITEM>No</LEDGERFROMITEM>
                       <ISPARTYLEDGER>Yes</ISPARTYLEDGER>
                       <ISLASTDEEMEDPOSITIVE>Yes</ISLASTDEEMEDPOSITIVE>";
                voucher.ledgers.Add(bankledger);

                if (invoice.tdsamount != 0)
                {
                    var tdsledger = new InfyPOS.Tally.TallyXML.LedgerVal(SyncName(
                        config.configuration.TDS, invoice),
                         invoice.tdsamount, false);

                    tdsledger.statename = invoice.statename;
                    tdsledger.cityname = invoice.cityname;
                    tdsledger.transactionmode = invoice.paymentmode;
                    tdsledger.IsNeedBankAllocation = false;
                    tdsledger.ChildNodes =
                            @"<LEDGERFROMITEM>No</LEDGERFROMITEM>
                       <ISPARTYLEDGER>Yes</ISPARTYLEDGER>
                       <ISLASTDEEMEDPOSITIVE>Yes</ISLASTDEEMEDPOSITIVE>";
                    voucher.ledgers.Add(tdsledger);
                }

                if (invoice.allocations.Sum(e => e.discountamount) != 0)
                {
                    var discount = new InfyPOS.Tally.TallyXML.LedgerVal(SyncName(
                     config.configuration.Discount, invoice),
                      invoice.allocations.Sum(e => e.discountamount), false);

                    discount.statename = invoice.statename;
                    discount.cityname = invoice.cityname;
                    discount.transactionmode = invoice.paymentmode;
                    discount.IsNeedBankAllocation = false;
                    discount.ChildNodes =
                            @"<LEDGERFROMITEM>No</LEDGERFROMITEM>
                       <ISPARTYLEDGER>Yes</ISPARTYLEDGER>
                       <ISLASTDEEMEDPOSITIVE>Yes</ISLASTDEEMEDPOSITIVE>";
                    voucher.ledgers.Add(discount);
                }

                if (invoice.rounding != 0)
                {
                    var rounding = new InfyPOS.Tally.TallyXML.LedgerVal(SyncName(
                    config.configuration.Rounding, invoice),
                    invoice.rounding, false);

                    rounding.statename = invoice.statename;
                    rounding.cityname = invoice.cityname;
                    rounding.transactionmode = invoice.paymentmode;
                    rounding.IsNeedBankAllocation = false;
                    rounding.ChildNodes =
                            @"<LEDGERFROMITEM>No</LEDGERFROMITEM>
                       <ISPARTYLEDGER>Yes</ISPARTYLEDGER>
                       <ISLASTDEEMEDPOSITIVE>Yes</ISLASTDEEMEDPOSITIVE>";
                    voucher.ledgers.Add(rounding);
                }
                return voucher;
            }

            public InfyPOS.Tally.TallyXML.Voucher CreateVoucher(Tally.TallyDataservice.InvoiceInfo invoice,
                bool autoTaxValue, bool agentcommission)
            {
                InfyPOS.Tally.TallyXML.Voucher voucher = new InfyPOS.Tally.TallyXML.Voucher();
                voucher.ledgers = new List<Tally.TallyXML.LedgerVal>();
                voucher.type = config.configuration.vouchertype("PURCHASE", config.configuration.vouchertype_purchase);

                if (string.IsNullOrEmpty(config.configuration.Data_Purchase_Narration))
                    voucher.narration = "Purchase " + invoice.entryno + " - " + invoice.invoiceno + "@" + invoice.invoicedate.ToString("dd-MM-yyyy");
                else
                    voucher.narration = SyncVoucherNo(config.configuration.Data_Purchase_Narration, invoice);


                voucher.voucherkey = "TP-" + invoice.suppliercompanyid + "-" + invoice.id + "-" + invoice.invoiceno;
                voucher.date = invoice.entrydate.Year < 1000 ? invoice.invoicedate : invoice.entrydate;
                voucher.vouchernumber = "TP-" + invoice.suppliercompanyid + "-" + invoice.id + "-" + invoice.invoiceno;

                if (config.configuration.Data_Purchase_EntryNo)
                    voucher.vouchernumber = invoice.entryno;
                else if (!string.IsNullOrEmpty(config.configuration.Data_Purchase_VoucherNo))
                    voucher.vouchernumber = SyncVoucherNo(config.configuration.Data_Purchase_VoucherNo, invoice);

                voucher.addtionalAttribs = string.Format(@"<REFERENCE>{0}</REFERENCE>
                                                            <REFERENCEDATE>{1}</REFERENCEDATE>
                                                            <ISOPTIONAL>{2}</ISOPTIONAL>
                                                            <ISCOSTCENTRE>{3}</ISCOSTCENTRE>
                                                            <COSTCENTRENAME>{4}</COSTCENTRENAME>
                                                            <EFFECTIVEDATE>{5}</EFFECTIVEDATE>",
                                                            EncodeXML(invoice.invoiceno),
                                                            invoice.invoicedate.ToString("yyyyMMdd"),
                                                            config.configuration.Data_Purchase_Approval ? "Yes" : "No",
                                                            config.configuration.Data_Purchase_Datacentre ? "Yes" : "No",
                                                            config.configuration.Data_Purchase_Datacentre ? invoice.locationname : "",
                                                            invoice.effectivedate != DateTime.MinValue ? invoice.effectivedate.ToString("yyyyMMdd"): invoice.invoicedate.ToString("yyyyMMdd"));

                if (config.configuration.Data_Purchase_GSTInfo)
                {
                    voucher.addtionalAttribs += string.Format(@"<STATENAME>{0}</STATENAME>
                                                            <COUNTRYOFRESIDENCE>{1}</COUNTRYOFRESIDENCE>
                                                            <PARTYPINCODE>{2}</PARTYPINCODE>
                                                            <GSTREGISTRATIONTYPE>{3}</GSTREGISTRATIONTYPE>
                                                            <PLACEOFSUPPLY>{4}</PLACEOFSUPPLY>
                                                            <PARTYGSTIN>{5}</PARTYGSTIN>
                                                            <VATPARTYTRANSRETURNNUMBER>{6}</VATPARTYTRANSRETURNNUMBER>
                                                            <GSTNATUREOFRETURN>07-Others</GSTNATUREOFRETURN>
                                                            <VCHSTATUSDATE>{7}</VCHSTATUSDATE>",
                                                            EncodeXML(invoice.statename),
                                                            EncodeXML(invoice.countryname),
                                                            EncodeXML(invoice.supplierpincode),
                                                            EncodeXML("Regular"),
                                                            EncodeXML(invoice.companystate),
                                                            EncodeXML(invoice.gstno),
                                                            EncodeXML(voucher.vouchernumber),
                                                            invoice.entrydate.ToString("yyyyMMdd"));
                }
                //var taxableValue = invoice.taxsummary.Sum(e => (e.taxtype == Tally.TallyDataservice.TaxSummary.TaxType.Amount) ? e.taxable : 0);
                var taxableValue = invoice.taxsummary.Sum(e => e.taxable);
                var tax = invoice.taxsummary.Sum(e => e.tax);
                var rounding = invoice.roundamount;
                var inwardexpenseTaxable = invoice.taxsummary.Sum(e => ((e.taxtype == Tally.TallyDataservice.TaxSummary.TaxType.Courier ||
                                                                   e.taxtype == Tally.TallyDataservice.TaxSummary.TaxType.Fright ||
                                                                   e.taxtype == Tally.TallyDataservice.TaxSummary.TaxType.Insurance ||
                                                                   e.taxtype == Tally.TallyDataservice.TaxSummary.TaxType.Packing ||
                                                                   e.taxtype == Tally.TallyDataservice.TaxSummary.TaxType.Service) && e.tax != 0) ? e.taxable : 0);

                var inwardexpenseNontaxable = invoice.taxsummary.Sum(e => ((e.taxtype == Tally.TallyDataservice.TaxSummary.TaxType.Courier ||
                                                                   e.taxtype == Tally.TallyDataservice.TaxSummary.TaxType.Fright ||
                                                                   e.taxtype == Tally.TallyDataservice.TaxSummary.TaxType.Insurance ||
                                                                   e.taxtype == Tally.TallyDataservice.TaxSummary.TaxType.Packing ||
                                                                   e.taxtype == Tally.TallyDataservice.TaxSummary.TaxType.Service) && e.tax == 0) ? e.taxable : 0);

                if (invoice.isinterstatesale)
                {
                    if (!string.IsNullOrEmpty(config.configuration.InwardExpenses_IGST))
                    {
                        taxableValue -= inwardexpenseTaxable;
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(config.configuration.InwardExpenses_GST))
                    {
                        taxableValue -= inwardexpenseTaxable;
                    }
                }

                InfyPOS.Tally.TallyXML.LedgerVal supplierLedger = new InfyPOS.Tally.TallyXML.LedgerVal(SyncName(config.configuration.Supplier, invoice, null), invoice.totalamount, false);
                supplierLedger.statename = !config.configuration.Data_Purchase_Address ? invoice.statename : "";
                supplierLedger.address = !config.configuration.Data_Purchase_Address ? invoice.address : "";
                supplierLedger.agentname = invoice.agentname;
                supplierLedger.cityname = !config.configuration.Data_Purchase_Address ? invoice.cityname : "";
                supplierLedger.gstno = invoice.gstno;
                supplierLedger.addionalAttribs =
                    @"<LEDGERFROMITEM>No</LEDGERFROMITEM>
                       <ISPARTYLEDGER>No</ISPARTYLEDGER>
                       <ISLASTDEEMEDPOSITIVE>Yes</ISLASTDEEMEDPOSITIVE>";
                supplierLedger.AddAllocation(invoice.invoiceno, invoice.totalamount);
                voucher.ledgers.Add(supplierLedger);

                //if (invoice.taxsummary.Exists(e => e.taxtype == TallyDataservice.TaxSummary.TaxType.Amount && e.percentage == 0) &&
                //   invoice.taxsummary.FindAll(e => e.percentage > 0).Count == 1)
                //{
                //    invoice.taxsummary.FindAll(e => e.taxtype == TallyDataservice.TaxSummary.TaxType.Amount && e.percentage == 0).ForEach(x =>
                //    {
                //        x.percentage = invoice.taxsummary.Find(e => e.percentage > 0).percentage;
                //    });
                //}

                foreach (var taxitem in invoice.taxsummary)
                {
                    if (taxitem.taxtype == TallyDataservice.TaxSummary.TaxType.Amount)
                    {
                        if (taxitem.taxtype != TallyDataservice.TaxSummary.TaxType.Packing &&
                            taxitem.taxtype != TallyDataservice.TaxSummary.TaxType.Service &&
                            taxitem.taxtype != TallyDataservice.TaxSummary.TaxType.Courier &&
                            taxitem.taxtype != TallyDataservice.TaxSummary.TaxType.Fright &&
                            taxitem.taxtype != TallyDataservice.TaxSummary.TaxType.Insurance &&
                            taxitem.taxtype != TallyDataservice.TaxSummary.TaxType.Tax)
                        {

                            if(invoice.isexpense &&
                                !string.IsNullOrEmpty(config.configuration.Supplies_Expenses))
                            {
                                InfyPOS.Tally.TallyXML.LedgerVal suplyLedger = new InfyPOS.Tally.TallyXML.LedgerVal(SyncName(config.configuration.Supplies_Expenses, invoice, taxitem), taxitem.taxable, true);
                                Add(voucher.ledgers, suplyLedger);

                            }
                            else if (invoice.isinterstatesale)
                            {
                                InfyPOS.Tally.TallyXML.LedgerVal suplyLedger = null;
                                if (!string.IsNullOrEmpty(config.configuration.Supplies_StockTransfer) &&
                                    invoice.stocktransfer)
                                    suplyLedger = new InfyPOS.Tally.TallyXML.LedgerVal(SyncName(config.configuration.Supplies_StockTransfer, invoice, taxitem), taxitem.taxable, true);
                                else
                                    suplyLedger = new InfyPOS.Tally.TallyXML.LedgerVal(SyncName(config.configuration.Supplies_Interstate, invoice, taxitem), taxitem.taxable, true);
                                Add(voucher.ledgers, suplyLedger);
                            }
                            else
                            {
                                InfyPOS.Tally.TallyXML.LedgerVal suplyLedger = new InfyPOS.Tally.TallyXML.LedgerVal(SyncName(config.configuration.Supplies_Localstate, invoice, taxitem), taxitem.taxable, true);
                                Add(voucher.ledgers, suplyLedger);
                            }
                        }
                    }

                    if (taxitem.taxtype == TallyDataservice.TaxSummary.TaxType.Tax && taxitem.tax > 0)
                    {
                        if (taxitem.isinterstatesale)
                        {
                            InfyPOS.Tally.TallyXML.LedgerVal igstLedger = new InfyPOS.Tally.TallyXML.LedgerVal(SyncName(config.configuration.Input_IGST, invoice, taxitem), Math.Round(taxitem.tax, 2), true);
                            Add(voucher.ledgers, igstLedger);
                        }
                        else
                        {

                            if (autoTaxValue && taxitem.percentage > 0)
                            {
                                taxitem.percentage = taxitem.percentage / 2;
                            }

                            var taxValue = Math.Round(taxitem.tax / 2, 2);
                            InfyPOS.Tally.TallyXML.LedgerVal cgstLedger = new InfyPOS.Tally.TallyXML.LedgerVal(SyncName(config.configuration.Input_CGST, invoice, taxitem), taxValue, true);
                            Add(voucher.ledgers, cgstLedger);

                            InfyPOS.Tally.TallyXML.LedgerVal sgstLedger = new InfyPOS.Tally.TallyXML.LedgerVal(SyncName(config.configuration.Input_SGST, invoice, taxitem), taxValue, true);
                            Add(voucher.ledgers, sgstLedger);
                            rounding += (taxitem.tax - (taxValue + taxValue));
                        }
                    }


                    if (taxitem.taxtype != TallyDataservice.TaxSummary.TaxType.Tax && taxitem.tax > 0)
                    {
                        if (taxitem.isinterstatesale)
                        {
                            InfyPOS.Tally.TallyXML.LedgerVal igstLedger = new InfyPOS.Tally.TallyXML.LedgerVal(SyncName(config.configuration.Input_IGST, invoice, taxitem), 
                                Math.Round(taxitem.tax,2), true);
                            Add(voucher.ledgers, igstLedger);
                        }
                        else
                        {

                            if (autoTaxValue && taxitem.percentage > 0)
                            {
                                taxitem.percentage = taxitem.percentage / 2;
                            }

                            var taxValue = Math.Round(taxitem.tax / 2, 2);
                            InfyPOS.Tally.TallyXML.LedgerVal cgstLedger = new InfyPOS.Tally.TallyXML.LedgerVal(SyncName(config.configuration.Input_CGST, invoice, taxitem), taxValue, true);
                            Add(voucher.ledgers, cgstLedger);

                            InfyPOS.Tally.TallyXML.LedgerVal sgstLedger = new InfyPOS.Tally.TallyXML.LedgerVal(SyncName(config.configuration.Input_SGST, invoice, taxitem), taxValue, true);
                            Add(voucher.ledgers, sgstLedger);
                            rounding += (taxitem.tax - (taxValue + taxValue));
                        }
                    }
                }

                foreach (var taxitem in invoice.taxsummary.Where(e => e.taxtype == TallyDataservice.TaxSummary.TaxType.Discount ||
                                        e.taxtype == TallyDataservice.TaxSummary.TaxType.OnDiscount))
                {
                    var ledgername = SyncName(config.configuration.Discount, invoice, taxitem);
                    if (voucher.ledgers.Exists(e => e.Name == ledgername))
                    {
                        voucher.ledgers.Find(e => e.Name == ledgername).Value += taxitem.discount;
                    }
                    else
                    {
                        InfyPOS.Tally.TallyXML.LedgerVal inwardExp = new InfyPOS.Tally.TallyXML.LedgerVal(ledgername, taxitem.discount, false);
                        voucher.ledgers.Add(inwardExp);
                    }
                }

                if (inwardexpenseNontaxable != 0)
                {
                    InfyPOS.Tally.TallyXML.LedgerVal inwardExp = new InfyPOS.Tally.TallyXML.LedgerVal(SyncName(config.configuration.InwardExpenses, invoice, null), inwardexpenseNontaxable, true);
                    voucher.ledgers.Add(inwardExp);
                }

                if (inwardexpenseTaxable != 0)
                {
                    var taxsummary = invoice.taxsummary.Find(e => ((e.taxtype == Tally.TallyDataservice.TaxSummary.TaxType.Courier ||
                                                                  e.taxtype == Tally.TallyDataservice.TaxSummary.TaxType.Fright ||
                                                                  e.taxtype == Tally.TallyDataservice.TaxSummary.TaxType.Insurance ||
                                                                  e.taxtype == Tally.TallyDataservice.TaxSummary.TaxType.Packing ||
                                                                  e.taxtype == Tally.TallyDataservice.TaxSummary.TaxType.Service) && e.tax != 0));

                    if (invoice.isinterstatesale)
                    {
                        if (!string.IsNullOrEmpty(config.configuration.InwardExpenses_IGST))
                        {
                            InfyPOS.Tally.TallyXML.LedgerVal inwardExp = new InfyPOS.Tally.TallyXML.LedgerVal(SyncName(config.configuration.InwardExpenses_IGST, invoice, taxsummary), inwardexpenseTaxable, true);
                            voucher.ledgers.Add(inwardExp);
                        }
                        else if (!string.IsNullOrEmpty(config.configuration.InwardExpenses))
                        {
                            InfyPOS.Tally.TallyXML.LedgerVal inwardExp = new InfyPOS.Tally.TallyXML.LedgerVal(SyncName(config.configuration.InwardExpenses, invoice, taxsummary), inwardexpenseTaxable, true);
                            voucher.ledgers.Add(inwardExp);
                        }
                        else
                        {
                            InfyPOS.Tally.TallyXML.LedgerVal inwardExp = new InfyPOS.Tally.TallyXML.LedgerVal(SyncName("Purchase Expense Ledger Config Missing", invoice, taxsummary), inwardexpenseTaxable, true);
                            voucher.ledgers.Add(inwardExp);
                        }
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(config.configuration.InwardExpenses_GST))
                        {
                            InfyPOS.Tally.TallyXML.LedgerVal inwardExp = new InfyPOS.Tally.TallyXML.LedgerVal(SyncName(config.configuration.InwardExpenses_GST, invoice, taxsummary), inwardexpenseTaxable, true);
                            voucher.ledgers.Add(inwardExp);
                        }
                        else if (!string.IsNullOrEmpty(config.configuration.InwardExpenses))
                        {
                            InfyPOS.Tally.TallyXML.LedgerVal inwardExp = new InfyPOS.Tally.TallyXML.LedgerVal(SyncName(config.configuration.InwardExpenses, invoice, taxsummary), inwardexpenseTaxable, true);
                            voucher.ledgers.Add(inwardExp);
                        }
                        else
                        {
                            InfyPOS.Tally.TallyXML.LedgerVal inwardExp = new InfyPOS.Tally.TallyXML.LedgerVal(SyncName("Purchase Expense Ledger Config Missing", invoice, taxsummary), inwardexpenseTaxable, true);
                            voucher.ledgers.Add(inwardExp);
                        }
                    }

                }


                decimal total = voucher.ledgers.Sum(e => e.Value);
                InfyPOS.Tally.TallyXML.LedgerVal roundingLedger = null;

                if (total != 0)
                {
                    if (total > 0)
                        roundingLedger = new InfyPOS.Tally.TallyXML.LedgerVal(SyncName(config.configuration.Rounding, invoice, null), total, true);
                    else
                        roundingLedger = new InfyPOS.Tally.TallyXML.LedgerVal(SyncName(config.configuration.Rounding, invoice, null), total * -1, false);
                    voucher.ledgers.Add(roundingLedger);
                }
                return voucher;
            }

            private string SyncVoucherNo(string source, TallyDataservice.ReturnInfo invoice)
            {
                source = source.Replace("[STATE]", invoice.statename);
                source = source.Replace("[BRANCH]", invoice.branchcode);
                source = source.Replace("[BRANCHCODE]", invoice.branchname);
                source = source.Replace("[BRANCHNAME]", invoice.branchname);
                source = source.Replace("[CUSTOMER]", invoice.customername);
                source = source.Replace("[CITY]", invoice.cityname);
                source = source.Replace("[INVOICENO]", EncodeXML(invoice.invoiceno));
                if(!string.IsNullOrEmpty(invoice.returnno))
                    source = source.Replace("[ENTRYNO]", EncodeXML(invoice.returnno));
                if (!string.IsNullOrEmpty(invoice.entryno))
                    source = source.Replace("[ENTRYNO]", EncodeXML(invoice.entryno));
                source = source.Replace("[SUPPLIERID]", invoice.suppliercompanyid.ToString("F0"));
                source = source.Replace("[SUPPLIERCOMPANY]", invoice.suppliercompany);
                source = source.Replace("[SUPPLIERNAME]", invoice.suppliername);
                source = source.Replace("[AGENT]", invoice.agentname);
                source = source.Replace("[REGION]", invoice.isinterstatesale ? config.configuration.Region_Interstate : config.configuration.Region_Local);
                source = source.Replace("[REGIONORCITY]", invoice.isinterstatesale ? invoice.cityname : config.configuration.Region_Local);
                return source;
            }
            //GSTIN: [SUPPLIERGST] Being Goods Purchase from [SUPPLIERNAME] Inv.No. [INVOICENO] Inv.Date: [INVOICEDATE] GRN.No. [ENTRYNO] GRN DATE: [GRNDATE] QTY: [QTY]Pcs.
            private string SyncVoucherNo(string source, TallyDataservice.InvoiceInfo invoice)
            {
                source = source.Replace("[STATE]", invoice.statename);
                source = source.Replace("[BRANCH]", invoice.locationcode);
                source = source.Replace("[BRANCHCODE]", invoice.locationcode);
                source = source.Replace("[BRANCHNAME]", invoice.locationname);
                source = source.Replace("[CUSTOMER]", invoice.customername);
                source = source.Replace("[CITY]", invoice.cityname);
                source = source.Replace("[INVOICENO]", EncodeXML(invoice.invoiceno));
                source = source.Replace("[ENTRYNO]", EncodeXML(invoice.entryno));
                source = source.Replace("[SUPPLIERID]", invoice.suppliercompanyid.ToString("F0"));
                source = source.Replace("[SUPPLIERCOMPANY]", invoice.suppliercompany);
                source = source.Replace("[SUPPLIERNAME]", invoice.suppliername);
                source = source.Replace("[AGENT]", invoice.agentname);
                source = source.Replace("[REGION]", invoice.isinterstatesale ? config.configuration.Region_Interstate : config.configuration.Region_Local);
                source = source.Replace("[REGIONORCITY]", invoice.isinterstatesale ? invoice.cityname : config.configuration.Region_Local);
                source = source.Replace("[SUPPLIERGST]", invoice.gstno);
                source = source.Replace("[INVOICEDATE]", invoice.invoicedate.ToString("dd-MM-yyyy"));
                source = source.Replace("[GRNDATE]", invoice.entrydate.ToString("dd-MM-yyyy"));
                source = source.Replace("[QTY]", invoice.qty.ToString("F0"));
                return source;
            }
            void Add(List<Tally.TallyXML.LedgerVal> soruceList, Tally.TallyXML.LedgerVal igstLedger)
            {
                if (soruceList.Exists(e => e.Name == igstLedger.Name))
                    soruceList.Find(e => e.Name == igstLedger.Name).Value += igstLedger.Value;
                else
                    soruceList.Add(igstLedger);
            }
            public static string EncodeXML(string xml)
            {
                if (string.IsNullOrEmpty(xml)) return "";
                if (xml.Contains('~'))
                {
                    xml = xml.Split('~')[0];
                }
                return xml.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;").Replace("'", "&apos;");
            }

            internal TallyXML.Voucher CreateVoucher(TallyDataservice.BillInfo item, bool autoTaxValue)
            {
                InfyPOS.Tally.TallyXML.Voucher voucher = new InfyPOS.Tally.TallyXML.Voucher();
                voucher.ledgers = new List<Tally.TallyXML.LedgerVal>();
                if (item.Amount < 0)
                {
                    voucher.type = config.configuration.vouchertype("CREDIT NOTE", config.configuration.vouchertype_salesreturn);
                    voucher.narration = (item.IsCredit ? ("Credit Note - " + item.LocationName + " : " + item.BillNo) : "Credit Note - " + item.LocationName) + "@" + item.Date.ToString("dd-MM-yyyy") + item.qty.ToString("F0");
                    voucher.voucherkey = item.IsCredit ? ("TCN" + (item.IsInterstate ? "I" : "") + item.Location + item.BillNo) : ("TCN" + (item.IsInterstate ? "I" : "") + item.Location + item.Date.ToString("ddMMyyyy"));

                }
                else
                {
                    voucher.type = config.configuration.vouchertype("SALES", config.configuration.vouchertype_sales);
                    voucher.narration = (item.IsCredit ? ("Credit Sales - " + item.LocationName + " : " + item.BillNo + " - " + item.qty.ToString("F0")) : "Cash Sales - " + item.LocationName) + "@" + item.Date.ToString("dd-MM-yyyy");
                    voucher.voucherkey = item.IsCredit ? ("TCR" + (item.IsInterstate ? "I" : "") + item.Location + item.BillNo) : ("TCS" + (item.IsInterstate ? "I" : "") + item.Location + item.Date.ToString("ddMMyyyy"));
                }
                voucher.vouchernumber = item.BillNo;
                voucher.date = item.Date;

                InfyPOS.Tally.TallyXML.LedgerVal customerledger = null;

                if (item.Amount < 0)
                {
                    if (item.IsEmployeeCredit)
                        customerledger = new InfyPOS.Tally.TallyXML.LedgerVal(SyncName(config.configuration.Employee, item, null), item.Amount + item.Rounding, true);
                    else
                        customerledger = new InfyPOS.Tally.TallyXML.LedgerVal(SyncName(config.configuration.Customer, item, null), item.Amount + item.Rounding, true);
                    customerledger.AddAllocation("CREDIT NOTE - " + item.BillNo + "@" + item.Date.ToString("dd-MM-yyyy"), (item.Amount + item.Rounding));
                    voucher.addtionalAttribs = string.Format("<REFERENCE>{0}</REFERENCE>", EncodeXML("CREDIT NOTE - " + item.BillNo + "@" + item.Date.ToString("dd-MM-yyyy")));
                }
                else if (item.IsCredit)
                {

                    if (item.CashPaid > 0)
                    {
                        if (item.IsEmployeeCredit)
                            customerledger = new InfyPOS.Tally.TallyXML.LedgerVal(SyncName(config.configuration.Employee, item, null), item.Amount - item.CashPaid + item.Rounding, true);
                        else
                            customerledger = new InfyPOS.Tally.TallyXML.LedgerVal(SyncName(config.configuration.Customer, item, null), item.Amount - item.CashPaid + item.Rounding, true);

                        customerledger.AddAllocation("CREDIT - " + item.BillNo + "@" + item.Date.ToString("dd-MM-yyyy"), (item.Amount - item.CashPaid + item.Rounding));
                        var cashledger = new InfyPOS.Tally.TallyXML.LedgerVal(SyncName(config.configuration.Cash, item, null), item.CashPaid, true);
                        voucher.ledgers.Add(cashledger);
                    }
                    else
                    {
                        if (item.IsEmployeeCredit)
                            customerledger = new InfyPOS.Tally.TallyXML.LedgerVal(SyncName(config.configuration.Employee, item, null), item.Amount + item.Rounding, true);
                        else
                            customerledger = new InfyPOS.Tally.TallyXML.LedgerVal(SyncName(config.configuration.Customer, item, null), item.Amount + item.Rounding, true);

                        customerledger.AddAllocation("CREDIT - " + item.BillNo + "@" + item.Date.ToString("dd-MM-yyyy"), (item.Amount + item.Rounding));
                    }
                    voucher.addtionalAttribs = string.Format("<REFERENCE>{0}</REFERENCE>", EncodeXML("CREDIT - " + item.BillNo + "@" + item.Date.ToString("dd-MM-yyyy")));
                }
                else
                {
                    if (item.paid_voucher > 0)
                    {
                        InfyPOS.Tally.TallyXML.LedgerVal valueLedger = new InfyPOS.Tally.TallyXML.LedgerVal(SyncName(config.configuration.CustomerVouchers, item, null), item.paid_voucher, true);
                        voucher.ledgers.Add(valueLedger);
                    }
                    if (item.creditnote_refund- item.creditnote_reisssue > 0)
                    {
                        InfyPOS.Tally.TallyXML.LedgerVal valueLedger = new InfyPOS.Tally.TallyXML.LedgerVal(SyncName(config.configuration.CustomerVouchers, item, null), item.creditnote_refund - item.creditnote_reisssue, false);
                        voucher.ledgers.Add(valueLedger);
                    }
                    if (item.cash_refund > 0)
                    {
                        InfyPOS.Tally.TallyXML.LedgerVal valueLedger = new InfyPOS.Tally.TallyXML.LedgerVal(SyncName(config.configuration.Cash, item, null), item.cash_refund, false);
                        voucher.ledgers.Add(valueLedger);
                    }
                    item.Amount += (item.cash_refund + item.creditnote_refund - item.creditnote_reisssue) - item.paid_voucher;
                    customerledger = new InfyPOS.Tally.TallyXML.LedgerVal(SyncName(config.configuration.Cash, item, null), item.Amount + item.Rounding, true);
                    customerledger.AddAllocation("CASH @" + item.Date.ToString("dd-MM-yyyy"), item.Amount + item.Rounding);
                    voucher.addtionalAttribs = string.Format("<REFERENCE>{0}</REFERENCE>", "CASH @" + item.Date.ToString("dd-MM-yyyy"));
                }

                customerledger.statename = !config.configuration.Data_Sales_Address ? item.Statename : "";
                customerledger.cityname = !config.configuration.Data_Sales_Address ? item.Cityname : "";
                customerledger.gstno = item.Gstno;
                customerledger.address = !config.configuration.Data_Sales_Address ? item.Address : "";
                customerledger.creditdays = item.CreditDays;
                if (!string.IsNullOrEmpty(item.Gstno))
                {
                    string stateinfo = string.Format(@"<PLACEOFSUPPLY>{0}</PLACEOFSUPPLY>
                    <STATENAME>{0}</STATENAME>", EncodeXML(item.Statename));
                    if (config.configuration.Data_Sales_Address)
                    {
                        stateinfo = "";
                    }
                    var customername = config.configuration.Customer.Contains("BILLINGNAME") ? item.billingname : item.CustomerName;
                    if(config.configuration.Customer.Contains("CUSTOMERGROUP"))
                    {
                        customername =item.customergroup;
                    }
                    voucher.addtionalAttribs += string.Format(@"{0}
						<COUNTRYOFRESIDENCE>India</COUNTRYOFRESIDENCE>
						<PARTYGSTIN>{1}</PARTYGSTIN>
						<PARTYNAME>{2}</PARTYNAME>
						<PARTYLEDGERNAME>{2}</PARTYLEDGERNAME>
                        <ISVATDUTYPAID>Yes</ISVATDUTYPAID>",
                        stateinfo,
                        EncodeXML(item.Gstno),
                        EncodeXML(customername));
                }

                voucher.ledgers.Add(customerledger);

                foreach (var taxitem in item.TaxSummary)
                {
                    var baseValue = taxitem.Value - taxitem.Tax;

                    //if (autoTaxValue && taxitem.Percentage > 0)
                    //{
                    //    if (item.IsInterstate)
                    //    {
                    //        baseValue = Math.Round(taxitem.Tax / taxitem.Percentage * 100, 2);
                    //    }
                    //    else
                    //    {
                    //        var taxValue = Math.Round(taxitem.Tax / 2, 2);
                    //        baseValue = Math.Round(taxValue / taxitem.Percentage * 100 * 2, 2);
                    //        taxitem.Percentage = taxitem.Percentage / 2;
                    //    }
                    //}

                    if (!string.IsNullOrEmpty(item.EcomChennal) &&
                        !string.IsNullOrEmpty(config.configuration.B2BOutwardSupplies_Onlinelocal) &&
                        !string.IsNullOrEmpty(config.configuration.B2BOutwardSupplies_OnlineInterstate))
                    {
                        InfyPOS.Tally.TallyXML.LedgerVal valueLedger =
                           new InfyPOS.Tally.TallyXML.LedgerVal(SyncName(item.IsInterstate ?
                           config.configuration.B2BOutwardSupplies_OnlineInterstate : config.configuration.B2BOutwardSupplies_Onlinelocal, item, taxitem), baseValue, false);
                        voucher.ledgers.Add(valueLedger);
                    }
                    else if (item.IsB2B && !string.IsNullOrEmpty(config.configuration.OutwardSupplies_Interstate)
                        && !string.IsNullOrEmpty(config.configuration.OutwardSupplies_Localstate))
                    {
                        InfyPOS.Tally.TallyXML.LedgerVal valueLedger =
                            new InfyPOS.Tally.TallyXML.LedgerVal(SyncName(item.IsInterstate ?
                            config.configuration.B2BOutwardSupplies_Interstate : config.configuration.B2BOutwardSupplies_Localstate, item, taxitem), baseValue, false);

                        if (item.stocktransfer && !string.IsNullOrEmpty(config.configuration.B2BOutwardSupplies_StockTransfer))
                            valueLedger =
                            new InfyPOS.Tally.TallyXML.LedgerVal(SyncName(item.IsInterstate ?
                            config.configuration.B2BOutwardSupplies_StockTransfer : config.configuration.B2BOutwardSupplies_StockTransfer, item, taxitem), baseValue, false);

                        voucher.ledgers.Add(valueLedger);
                    }
                    else
                    {
                        InfyPOS.Tally.TallyXML.LedgerVal valueLedger =
                            new InfyPOS.Tally.TallyXML.LedgerVal(SyncName(item.IsInterstate ?
                            config.configuration.OutwardSupplies_Interstate : config.configuration.OutwardSupplies_Localstate, item, taxitem), baseValue, false);

                        if (item.stocktransfer && !string.IsNullOrEmpty(config.configuration.B2BOutwardSupplies_StockTransfer))
                        {
                            valueLedger = new InfyPOS.Tally.TallyXML.LedgerVal(SyncName(item.IsInterstate ?
                            config.configuration.B2BOutwardSupplies_StockTransfer : config.configuration.B2BOutwardSupplies_StockTransfer, item, taxitem), baseValue, false);
                        }
                        voucher.ledgers.Add(valueLedger);
                    }
                }

                foreach (var taxitem in item.TaxSummary)
                {
                    if (item.IsInterstate)
                    {
                        InfyPOS.Tally.TallyXML.LedgerVal igstLedger = new InfyPOS.Tally.TallyXML.LedgerVal(SyncName(config.configuration.Output_IGST, item, taxitem), Math.Round(taxitem.Tax,2), false);
                        Add(voucher.ledgers, igstLedger);
                    }
                    else
                    {

                        var taxValue = Math.Round(taxitem.Tax / 2, 2);
                        if (autoTaxValue && taxitem.Percentage > 0)
                        {
                            taxitem.Percentage = taxitem.Percentage / 2;
                        }

                        InfyPOS.Tally.TallyXML.LedgerVal cgstLedger = new InfyPOS.Tally.TallyXML.LedgerVal(SyncName(config.configuration.Output_CGST, item, taxitem), taxValue, false);
                        Add(voucher.ledgers, cgstLedger);

                        InfyPOS.Tally.TallyXML.LedgerVal sgstLedger = new InfyPOS.Tally.TallyXML.LedgerVal(SyncName(config.configuration.Output_SGST, item, taxitem), taxValue, false);
                        Add(voucher.ledgers, sgstLedger);
                    }
                }

                if (item.CardsSales != null && item.CardsSales.Count > 0)
                {
                    foreach (var card in item.CardsSales)
                    {
                        if (card.amount == 0) continue;
                        if (string.IsNullOrEmpty(card.bank))
                            card.bank = "";

                        customerledger = new InfyPOS.Tally.TallyXML.LedgerVal(SyncName(
                            card.isupi ? config.configuration.UPI : config.configuration.Bank, card), card.amount, true);
                        if (string.IsNullOrEmpty(card.bank))
                        {
                            card.bank = SyncName(card.isupi ? config.configuration.UPI : config.configuration.Bank, card).Split('~').Last();
                        }
                        if (card.isupi)
                            customerledger.AddAllocation("UPI_" + card.bank.Replace(" ", "") + "@" + item.Date.ToString("dd-MM-yyyy"), card.amount);
                        else
                            customerledger.AddAllocation("BANK_" + card.bank.Replace(" ", "") + "@" + item.Date.ToString("dd-MM-yyyy"), card.amount);
                        voucher.ledgers.Add(customerledger);
                    }
                }

                decimal total = voucher.ledgers.Sum(e => e.Value);
                InfyPOS.Tally.TallyXML.LedgerVal roundingLedger = null;

                if (total != 0)
                {
                    if (total > 0)
                        roundingLedger = new InfyPOS.Tally.TallyXML.LedgerVal(SyncName(config.configuration.Rounding, item, null), total, true);
                    else
                        roundingLedger = new InfyPOS.Tally.TallyXML.LedgerVal(SyncName(config.configuration.Rounding, item, null), total * -1, false);
                    voucher.ledgers.Add(roundingLedger);
                }


                if(!string.IsNullOrEmpty(config.configuration.Data_Sales_Narration))
                {
                    var defaultNarraion = config.configuration.Data_Sales_Narration;
                    if (config.configuration.Data_Sales_Narration.Split('"').Length > 2)
                    {
                        var narrationtemplate = ParseToSortedList(config.configuration.Data_Sales_Narration);
                        defaultNarraion = narrationtemplate.ContainsKey("DEFAULT") ?narrationtemplate["DEFAULT"]:"";
                        if (item.IsB2B)
                        {
                            if (item.Amount < 0)
                            {
                                if (narrationtemplate.ContainsKey("B2BCREDITNOTE"))
                                    defaultNarraion = narrationtemplate["B2BCREDITNOTE"];
                            }
                            else
                            {
                                if (narrationtemplate.ContainsKey("B2BSALES"))
                                    defaultNarraion = narrationtemplate["B2BSALES"];
                            }
                        }
                        else
                        {
                            if (item.Amount < 0 &&
                                narrationtemplate.ContainsKey("B2CCREDITNOTE"))
                            {
                                defaultNarraion = narrationtemplate["B2CCREDITNOTE"];
                            }
                            else
                            {
                                if (narrationtemplate.ContainsKey("B2CSALES"))
                                    defaultNarraion = narrationtemplate["B2CSALES"];
                            }
                        }
                    }
                    if (!string.IsNullOrEmpty(defaultNarraion))
                        voucher.narration = SyncName(defaultNarraion, item, null);
                }

                //if (total != 0)
                //{
                //    roundingLedger = new InfyPOS.Tally.TallyXML.LedgerVal(SyncName(config.configuration.Rounding, item, null), total, total > 0);
                //    voucher.ledgers.Add(roundingLedger);
                //}
                return voucher;

            }

            public String BuildNarration(TallyDataservice.BillInfo billinfo,string template)
            {
                template = template.Replace("CUSTOMERGST", billinfo.Gstno);
                template = template.Replace("CUSTOMERNAME", billinfo.CustomerName);
                template = template.Replace("BILLNO", billinfo.BillNo);
                template = template.Replace("BILLDATE", billinfo.Date.ToString("dd-MM-yyyy"));
                template = template.Replace("SALEQTY", billinfo.qty.ToString("F0"));
                template = template.Replace("LOCATION", billinfo.LocationName);
                return template;
            }

            public static SortedList<string, string> ParseToSortedList(string input)
            {
                var result = new SortedList<string, string>(StringComparer.OrdinalIgnoreCase);

                // Regex to capture "KEY"="VALUE"
                var pattern = new Regex("\"(?<key>[^\"]+)\"\\s*=\\s*\"(?<value>[^\"]*)\"",
                                         RegexOptions.Compiled);

                var matches = pattern.Matches(input);

                foreach (Match match in matches)
                {
                    var key = match.Groups["key"].Value.Trim();
                    var value = match.Groups["value"].Value.Trim();

                    if (!result.ContainsKey(key))
                        result.Add(key, value);
                }

                return result;
            }

            internal TallyXML.Voucher CreateVoucher(TallyDataservice.ReturnInfo invoice, bool autoTaxValue)
            {
                InfyPOS.Tally.TallyXML.Voucher voucher = new InfyPOS.Tally.TallyXML.Voucher();
                voucher.ledgers = new List<Tally.TallyXML.LedgerVal>();
                bool isdebitenote = invoice.returntype <10;
                if (isdebitenote)
                {
                    voucher.type = config.configuration.vouchertype("Debit Note", config.configuration.vouchertype_purchasereturn,invoice.returntype);
                    voucher.narration = "Purchase Return " + invoice.returnno + "@" + invoice.returndate.ToString("dd-MM-yyyy");
                    voucher.voucherkey = "TPR-" + (invoice.isinterstatesale ? "I-" : "") + invoice.suppliercompanyid + "-" + invoice.id + "-" + invoice.returnno;
                    voucher.date = invoice.returndate;
                    voucher.vouchernumber = "TPR-" + (invoice.isinterstatesale ? "I-" : "") + invoice.suppliercompanyid + "-" + invoice.id + "-" + invoice.returnno;
                }
                else
                {
                    voucher.type = config.configuration.vouchertype("Credit Note", config.configuration.vouchertype_purchasecreditnote, invoice.returntype);
                    voucher.narration = "Purchase - Credit Note " + invoice.returnno + "@" + invoice.returndate.ToString("dd-MM-yyyy");
                    voucher.voucherkey = "TPC-" + (invoice.isinterstatesale ? "I-" : "") + invoice.suppliercompanyid + "-" + invoice.id + "-" + invoice.returnno;
                    voucher.date = invoice.returndate;
                    voucher.vouchernumber = "TPC-" + (invoice.isinterstatesale ? "I-" : "") + invoice.suppliercompanyid + "-" + invoice.id + "-" + invoice.returnno;
                }

                if (config.configuration.Data_Purchase_EntryNo)
                    voucher.vouchernumber = invoice.returnno;
                else if (!string.IsNullOrEmpty(config.configuration.Data_Purchase_VoucherNo))
                    voucher.vouchernumber = SyncVoucherNo(config.configuration.Data_Purchase_VoucherNo, invoice);

                //var taxableValue = invoice.taxsummary.Sum(e => (e.taxtype == Tally.TallyDataservice.TaxSummary.TaxType.Amount) ? e.taxable : 0);
                var taxableValue = invoice.taxsummary.Sum(e => e.taxable);
                var tax = invoice.taxsummary.Sum(e => e.tax);
                var rounding = invoice.roundamount;

                voucher.addtionalAttribs = string.Format(@"<REFERENCE>{0}</REFERENCE>
                                                            <REFERENCEDATE>{1}</REFERENCEDATE>
                                                            <ISOPTIONAL>{2}</ISOPTIONAL>
                                                            <ISCOSTCENTRE>{3}</ISCOSTCENTRE>
                                                            <COSTCENTRENAME>{4}</COSTCENTRENAME>
                                                            <EFFECTIVEDATE>{5}</EFFECTIVEDATE>",
                                            EncodeXML(invoice.invoiceno),
                                            invoice.invoicedate.ToString("yyyyMMdd"),
                                            config.configuration.Data_Purchase_Approval ? "Yes" : "No",
                                            config.configuration.Data_Purchase_Datacentre ? "Yes" : "No",
                                            config.configuration.Data_Purchase_Datacentre ? invoice.branchname : "",
                                            invoice.effectivedate != DateTime.MinValue ? invoice.effectivedate.ToString("yyyyMMdd"): invoice.returndate.ToString("yyyyMMdd"));

                if (config.configuration.Data_Purchase_GSTInfo)
                {
                    voucher.addtionalAttribs += string.Format(@"<STATENAME>{0}</STATENAME>
                                                            <COUNTRYOFRESIDENCE>{1}</COUNTRYOFRESIDENCE>
                                                            <PARTYPINCODE>{2}</PARTYPINCODE>
                                                            <GSTREGISTRATIONTYPE>{3}</GSTREGISTRATIONTYPE>
                                                            <PLACEOFSUPPLY>{4}</PLACEOFSUPPLY>
                                                            <PARTYGSTIN>{5}</PARTYGSTIN>
                                                            <VATPARTYTRANSRETURNNUMBER>{6}</VATPARTYTRANSRETURNNUMBER>
                                                            <GSTNATUREOFRETURN>07-Others</GSTNATUREOFRETURN>
                                                            <VCHSTATUSDATE>{7}</VCHSTATUSDATE>",
                                                            EncodeXML(invoice.statename),
                                                            EncodeXML(invoice.countryname),
                                                            EncodeXML(invoice.supplierpincode),
                                                            EncodeXML("Regular"),
                                                            EncodeXML(invoice.companystate),
                                                            EncodeXML(invoice.gstno),
                                                            EncodeXML(voucher.vouchernumber),
                                                            invoice.returndate.ToString("yyyyMMdd"));
                }


                InfyPOS.Tally.TallyXML.LedgerVal supplierLedger = new InfyPOS.Tally.TallyXML.LedgerVal(SyncName(config.configuration.Supplier, invoice, null), invoice.totalamount, isdebitenote);
                supplierLedger.statename = !config.configuration.Data_Purchase_Address ? invoice.statename : "";
                supplierLedger.cityname = !config.configuration.Data_Purchase_Address ? invoice.cityname : "";
                supplierLedger.address = !config.configuration.Data_Purchase_Address ? invoice.address : "";

                if(isdebitenote)
                    supplierLedger.AddAllocation("DN/" + invoice.returnno + "/" + invoice.invoiceno, invoice.totalamount);
                else
                    supplierLedger.AddAllocation("PCN/" + invoice.returnno + "/" + invoice.invoiceno, invoice.totalamount);
                voucher.ledgers.Add(supplierLedger);

                foreach (var taxitem in invoice.taxsummary)
                {
                    if (taxitem.taxtype == TallyDataservice.TaxSummary.TaxType.Amount || taxitem.tax > 0)
                    {
                        if (invoice.isinterstatesale)
                        {
                            InfyPOS.Tally.TallyXML.LedgerVal suplyLedger = new InfyPOS.Tally.TallyXML.LedgerVal(SyncName(config.configuration.Supplies_Interstate, invoice, taxitem), taxitem.taxable, !isdebitenote);
                            Add(voucher.ledgers, suplyLedger);
                        }
                        else
                        {
                            InfyPOS.Tally.TallyXML.LedgerVal suplyLedger = new InfyPOS.Tally.TallyXML.LedgerVal(SyncName(config.configuration.Supplies_Localstate, invoice, taxitem), taxitem.taxable, !isdebitenote);
                            Add(voucher.ledgers, suplyLedger);
                        }
                    }

                    if (taxitem.tax > 0)
                    {
                        if (taxitem.isinterstatesale)
                        {
                            InfyPOS.Tally.TallyXML.LedgerVal igstLedger = new InfyPOS.Tally.TallyXML.LedgerVal(SyncName(config.configuration.Input_IGST, invoice, taxitem), taxitem.tax, !isdebitenote);
                            Add(voucher.ledgers, igstLedger);
                        }
                        else
                        {

                            if (autoTaxValue && taxitem.percentage > 0)
                            {
                                taxitem.percentage = taxitem.percentage / 2;
                            }

                            var taxValue = Math.Round(taxitem.tax / 2, 2);
                            InfyPOS.Tally.TallyXML.LedgerVal cgstLedger = new InfyPOS.Tally.TallyXML.LedgerVal(SyncName(config.configuration.Input_CGST, invoice, taxitem), taxValue, !isdebitenote);
                            Add(voucher.ledgers, cgstLedger);

                            InfyPOS.Tally.TallyXML.LedgerVal sgstLedger = new InfyPOS.Tally.TallyXML.LedgerVal(SyncName(config.configuration.Input_SGST, invoice, taxitem), taxValue, !isdebitenote);
                            Add(voucher.ledgers, sgstLedger);
                            rounding += (taxitem.tax - (taxValue + taxValue));
                        }
                    }
                }

                if (invoice.freightamt != 0)
                {
                    var inwardexpenseTaxable = invoice.freightamt;
                    var taxsummary = new TallyDataservice.TaxSummary()
                    {

                    };
                    if(invoice.freightamt>0 && invoice.freighttaxamt>0)
                    {
                        taxsummary.percentage = Math.Round(invoice.freightamt / invoice.freighttaxamt * 100, 2);
                    };
                    if (invoice.isinterstatesale)
                    {
                        if (!string.IsNullOrEmpty(config.configuration.InwardExpenses_IGST))
                        {
                            InfyPOS.Tally.TallyXML.LedgerVal inwardExp = new InfyPOS.Tally.TallyXML.LedgerVal(SyncName(config.configuration.InwardExpenses_IGST, invoice, taxsummary), inwardexpenseTaxable, true);
                            voucher.ledgers.Add(inwardExp);
                        }
                        else if (!string.IsNullOrEmpty(config.configuration.InwardExpenses))
                        {
                            InfyPOS.Tally.TallyXML.LedgerVal inwardExp = new InfyPOS.Tally.TallyXML.LedgerVal(SyncName(config.configuration.InwardExpenses, invoice, taxsummary), inwardexpenseTaxable, true);
                            voucher.ledgers.Add(inwardExp);
                        }
                        else
                        {
                            InfyPOS.Tally.TallyXML.LedgerVal inwardExp = new InfyPOS.Tally.TallyXML.LedgerVal(SyncName("Purchase Expense Ledger Config Missing", invoice, taxsummary), inwardexpenseTaxable, true);
                            voucher.ledgers.Add(inwardExp);
                        }
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(config.configuration.InwardExpenses_GST))
                        {
                            InfyPOS.Tally.TallyXML.LedgerVal inwardExp = new InfyPOS.Tally.TallyXML.LedgerVal(SyncName(config.configuration.InwardExpenses_GST, invoice, taxsummary), inwardexpenseTaxable, true);
                            voucher.ledgers.Add(inwardExp);
                        }
                        else if (!string.IsNullOrEmpty(config.configuration.InwardExpenses))
                        {
                            InfyPOS.Tally.TallyXML.LedgerVal inwardExp = new InfyPOS.Tally.TallyXML.LedgerVal(SyncName(config.configuration.InwardExpenses, invoice, taxsummary), inwardexpenseTaxable, true);
                            voucher.ledgers.Add(inwardExp);
                        }
                        else
                        {
                            InfyPOS.Tally.TallyXML.LedgerVal inwardExp = new InfyPOS.Tally.TallyXML.LedgerVal(SyncName("Purchase Expense Ledger Config Missing", invoice, taxsummary), inwardexpenseTaxable, true);
                            voucher.ledgers.Add(inwardExp);
                        }
                    }

                    if (invoice.freightamt> 0)
                    {
                        if (invoice.isinterstatesale)
                        {
                            InfyPOS.Tally.TallyXML.LedgerVal igstLedger = new InfyPOS.Tally.TallyXML.LedgerVal(SyncName(config.configuration.Input_IGST, invoice, taxsummary), invoice.freightamt, !isdebitenote);
                            Add(voucher.ledgers, igstLedger);
                        }
                        else
                        {

                            if (autoTaxValue && taxsummary.percentage > 0)
                            {
                                taxsummary.percentage = taxsummary.percentage / 2;
                            }

                            var taxValue = Math.Round(taxsummary.tax / 2, 2);
                            InfyPOS.Tally.TallyXML.LedgerVal cgstLedger = new InfyPOS.Tally.TallyXML.LedgerVal(SyncName(config.configuration.Input_CGST, invoice, taxsummary), taxValue, !isdebitenote);
                            Add(voucher.ledgers, cgstLedger);

                            InfyPOS.Tally.TallyXML.LedgerVal sgstLedger = new InfyPOS.Tally.TallyXML.LedgerVal(SyncName(config.configuration.Input_SGST, invoice, taxsummary), taxValue, !isdebitenote);
                            Add(voucher.ledgers, sgstLedger);
                            rounding += (taxsummary.tax - (taxValue + taxValue));
                        }
                    }
                }

                decimal total = voucher.ledgers.Sum(e => e.Value);
                InfyPOS.Tally.TallyXML.LedgerVal roundingLedger = null;

                if (total != 0)
                {
                    if (total > 0)
                        roundingLedger = new InfyPOS.Tally.TallyXML.LedgerVal(SyncName(config.configuration.Rounding, invoice, null), total, true);
                    else
                        roundingLedger = new InfyPOS.Tally.TallyXML.LedgerVal(SyncName(config.configuration.Rounding, invoice, null), total * -1, false);
                    voucher.ledgers.Add(roundingLedger);
                }

                //if (total != 0)
                //{
                //    roundingLedger = new InfyPOS.Tally.TallyXML.LedgerVal(SyncName(config.configuration.Rounding, invoice, null), total, total > 0);
                //    voucher.ledgers.Add(roundingLedger);
                //}
                return voucher;
            }


        }

        public class SalesCreteria : BaseEntity
        {
            public long companyid { get; set; }
            public long customerid { get; set; }
            public DateTime fromdate { get; set; }
            public int locationid { get; set; }
            public DateTime todate { get; set; }
            public bool multicompany { get; set; }
            public bool autosynctax { get; set; }
        }

        public class PurchaseReturnCreteria : BaseEntity
        {
            public long companyid { get; set; }
            public long supplierid { get; set; }
            public DateTime fromdate { get; set; }
            public int locationid { get; set; }
            public DateTime todate { get; set; }
            public bool autosynctax { get; set; }
            public bool agentcommission { get; set; }
        }

        public class PurchaseCreteria : BaseEntity
        {
            public long companyid { get; set; }
            public long supplierid { get; set; }
            public long entrylocationid { get; set; }
            public DateTime entrydatefrom { get; set; }
            public DateTime entrydateto { get; set; }
            public DateTime invoicedatefrom { get; set; }
            public DateTime invoicedateto { get; set; }
            public bool autosynctax { get; set; }
            public bool agentcommission { get; set; }
            public string entryno { get; set; }
        }

        public class SupplierPaymentCreteria : BaseEntity
        {
            public long companyid { get; set; }
            public long supplierid { get; set; }
            public long locationid { get; set; }
            public DateTime invoicedatefrom { get; set; }
            public DateTime invoicedateto { get; set; }
        }

    }
}