using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Common;
using InfyPOS.Data;
using InfyPOS.GST;
using InfyPOS.Models;
using WebAPI.Models;

namespace InfyPOS.Tally
{
    public class TallyDataservice : Common.BaseData
    {
        public class TaxSummary
        {
            public decimal taxable;
            public decimal tax;
            public int type;
            public decimal percentage;
            public bool isinterstatesale;
            public long invoiceid;
            public long createdby;
            public long suppliercompanyid;
            public string returnno;
            public DateTime date;
            public TaxType taxtype
            {
                get
                {
                    return (TaxType)type;
                }
            }

            public string taxname { get; set; }
            public decimal discount { get; set; }
            public string invoiceno { get; set; }
            public int returntype { get; internal set; }

            public enum TaxType
            {
                Amount,
                Tax,
                Discount,
                OnDiscount,
                Insurance,
                Service,
                Courier,
                Packing,
                Fright,
                Rounding
            }
        }

        internal TallyVoucher CreateBOMPurchaseReturnVouchers(TallyConfig.PurchaseReturnCreteria criteria)
        {
            Data.CompanyDataService companydata = new Data.CompanyDataService();
            var companies = companydata.Search(new CompanyCreteria()
            {
                id = criteria.companyid
            });
            if (companies.Count != 1) return null;

            TallyConfig.Purchase purchaseConfig = new TallyConfig.Purchase(companies[0].tallyconfig);
            return CreateBOMPurchaseReturnVouchers(criteria, purchaseConfig);
        }

        internal TallyVoucher CreatePurchaseReturnVouchers(TallyConfig.PurchaseReturnCreteria criteria)
        {
            Data.CompanyDataService companydata = new Data.CompanyDataService();
            var companies = companydata.Search(new CompanyCreteria()
            {
                id = criteria.companyid
            });
            if (companies.Count != 1) return null;

            TallyConfig.Purchase purchaseConfig = new TallyConfig.Purchase(companies[0].tallyconfig);
            return CreatePurchaseReturnVouchers(criteria, purchaseConfig);
        }

        public class ReturnInfo
        {
            public ReturnInfo()
            {
                taxsummary = new List<TaxSummary>();
            }
            public List<TaxSummary> taxsummary;
            public long id;
            public int returntype;
            public long invoiceid;
            public long createdby;
            public string returnno;
            public DateTime returndate;
            public decimal totalamount;
            public decimal baseamount;
            public decimal packingcharge;
            public decimal freightamt;
            public decimal freighttaxamt;

            public decimal roundamount;
            public string suppliercompany;
            public string suppliername;
            public string address;
            public string agentname;
            public long suppliercompanyid;
            public string statename;
            public string cityname;
            public string entryno;
            public bool isinterstatesale;
            public string branchname;
            public string branchcode;
            public string customername;
            public string invoiceno;
            public DateTime invoicedate;
            public string countryname;
            public string supplierpincode;
            public string companystate;
            public string gstno;
            public DateTime effectivedate;
            public string suppliergroup;
        }

        public class PaymentInfo
        {
            public long id { get; set; }
            public long locationid { get; set; }
            public string receiptno { get; set; }
            public long refid { get; set; }
            public long createdby { get; set; }
            public string invoiceno { get; set; }
            public DateTime paymentdate { get; set; }
            public string suppliercompany { get; set; }
            public string suppliername { get; set; }
            public string address { get; set; }
            public string statename { get; set; }
            public string cityname { get; set; }
            public string location { get; set; }
            public List<Allocation> allocations { get; set; }
            public string bankname { get; set; }
            public DateTime instrumentdate { get; set; }
            public string chequeno { get; set; }
            public long suppliercompanyid { get; set; }
            public bool isinterstatesale { get; set; }
            public string paymentmode { get; set; }
            public bool iscash { get; set; }
            public string gstno { get; set; }
            public decimal tdspercentage { get; set; }
            public decimal tdsamount { get; set; }
            public decimal rounding { get; set; }
            public string locationcode { get; set; }
            public string locationname { get; set; }
            public string suppliergroup { get; set; }

            public class Allocation
            {
                public bool isdebitnote { get; set; }
                public string returnno { get; set; }
                public string invoiceno { get; set; }
                public decimal paidamount { get; set; }
                public decimal agentcommission { get; set; }
                public decimal discountamount { get; set; }
            }

        }

        public class CustomerPaymentInfo
        {
            public long id { get; set; }
            public long bankid { get; set; }
            public string receiptno { get; set; }
            public string billno { get; set; }
            public DateTime paidon { get; set; }
            public string customername { get; set; }
            public string statename { get; set; }
            public string cityname { get; set; }
            public List<Allocation> allocations { get; set; }
            public string bankname { get; set; }
            public DateTime date { get; set; }
            public string orderno { get; set; }
            public string customerid { get; set; }
            public bool isinterstatesale { get; set; }
            public string paymentmode { get; set; }
            public bool iscash { get; set; }
            public string gstno { get; set; }
            public string address { get; set; }
            public class Allocation
            {
                public string billno { get; set; }
                public DateTime billdate { get; set; }
                public decimal paid { get; set; }
                public decimal discount { get; set; }
            }

        }


        public class InvoiceInfo
        {
            public InvoiceInfo()
            {
                taxsummary = new List<TaxSummary>();
            }
            public List<TaxSummary> taxsummary;
            public long id { get; set; }
            public long createdby { get; set; }
            public long invoiceid { get; set; }
            public string invoiceno { get; set; }
            public bool stocktransfer { get; set; }
            public DateTime invoicedate { get; set; }
            public DateTime effectivedate { get; set; }
            public DateTime entrydate { get; set; }
            public decimal totalamount { get; set; }
            public decimal diffvalue { get; set; }
            public decimal roundamount { get; set; }
            public string address { get; set; }
            public string suppliercompany { get; set; }
            public string suppliername { get; set; }
            public string groupname { get; set; }
            public long suppliercompanyid { get; set; }
            public string statename { get; set; }
            public string countryname { get; set; }
            public string supplierpincode { get; set; }
            public string companystate { get; set; }
            public string cityname { get; set; }
            public string entryno { get; set; }
            public bool isexpense { get; set; }
            public bool isinterstatesale { get; set; }
            public string branchname { get; set; }
            public string customername { get; set; }
            public string agentname { get; set; }
            public decimal agentcommission { get; set; }
            public string gstno { get; set; }
            public long entrylocationid { get; set; }
            public string locationcode { get; set; }
            public string locationname { get; set; }
            public string suppliergroup { get;  set; }
            public decimal qty { get; set; }
        }

        public class CashCreditSales
        {
            public long id { get; set; }
            public long customerid { get; set; }
            public long billid { get; set; }
            public string billno { get; set; }
            public decimal paid_cash { get; set; }
            public decimal paid_credit { get; set; }
            public decimal billamount { get; set; }
        }
        public class CardSales
        {
            public string description { get; set; }
            public string locationcode { get; set; }
            public string locationname { get; set; }
            public long counterid { get; set; }
            public long locationid { get; set; }
            public long companyid { get; set; }
            public string bank { get; set; }
            public DateTime date { get; set; }
            public long bankid { get; set; }
            public decimal amount { get; set; }
            public bool isupi { get; set; }
            public string upi { get; set; }
            public long upiid { get; set; }
            public decimal paid_voucher { get; set; }
            public decimal adjustment_paid { get; set; }
            public decimal creditnote_refund { get; set; }
            public decimal creditnote_reisssue { get; set; }
            public decimal cash_refund { get; internal set; }
        }

        public class CardSalesMultiCompany : CardSales
        {
            public long id { get; set; }
            public long billedcompany { get; set; }
            public long settledcompany { get; set; }
            public decimal settled { get; set; }
            public decimal billed { get; set; }
            public decimal cash { get; set; }
            public string billno { get; set; }
        }
        public class BillInfo
        {
            public int CreditDays { get; set; }
            public string CustomerName { get; set; }
            public long CustomerID { get; set; }
            public string agentname { get; set; }
            public string billingname { get; set; }
            public string customergroup { get; set; }
            public string BillNo { get; set; }
            public string ReturnBillno { get; set; }
            public string EcomChennal { get; set; }
            public bool IsCredit { get; set; }
            public decimal CashPaid { get; set; }
            public bool IsCreditNote { get; set; }
            public DateTime Date { get; set; }
            public bool IsInterstate { get; set; }
            public bool IsB2B { get; set; }
            public long LocationID { get; set; }
            public long CreatedBy { get; set; }

            public BillInfo()
            {
                TaxSummary = new List<TaxInfo>();
            }
            public List<TaxInfo> TaxSummary { get; set; }
            public class TaxInfo
            {
                public decimal Percentage { get; set; }
                public decimal Tax { get; set; }
                public decimal Value { get; set; }
            }

            public String Key
            {
                get
                {
                    return string.Format("{0}:{1}:{2}:{3}:{4}", Date, IsInterstate, BillNo, IsCredit, LocationID);
                }
            }

            public decimal Amount { get; set; }
            public decimal Rounding { get; set; }
            public string branchname { get; set; }
            public string Location { get; set; }
            public string LocationName { get; set; }
            public bool IsEmployeeCredit { get; set; }
            public List<CardSales> CardsSales { get; set; }
            public void AddCardSales(CardSales card)
            {
                if (CardsSales == null)
                    CardsSales = new List<CardSales>();
                CardsSales.Add(card);
                CardAmount += card.amount;
                Amount -= card.amount;
            }
            public decimal paid_voucher { get { return CardsSales == null ?0:CardsSales.Sum(x => x.paid_voucher); } }
            public decimal adjustment_paid { get { return CardsSales == null ? 0 : CardsSales.Sum(x => x.adjustment_paid); } }
            public decimal creditnote_refund { get { return CardsSales == null ? 0 : CardsSales.Sum(x => x.creditnote_refund); } }
            public decimal creditnote_reisssue { get { return CardsSales == null ? 0 : CardsSales.Sum(x => x.creditnote_reisssue); } }
            public decimal cash_refund { get { return CardsSales == null ? 0 : CardsSales.Sum(x => x.cash_refund); } }

            public decimal CardAmount { get; set; }
            public string CustomerArea { get; set; }
            public string Cityname { get; set; }
            public string Statename { get; set; }
            public string Gstno { get; set; }
            public string Address { get; set; }
            public decimal qty { get; set; }
            public long id { get; set; }
            public bool stocktransfer { get; set; }
            public string einv { get;  set; }
            public string ewb { get;  set; }

            internal void AddTax(decimal taxpercentage, decimal tax, decimal amount)
            {
                if (TaxSummary.Exists(x => x.Percentage == taxpercentage))
                {
                    var ts = TaxSummary.Find(x => x.Percentage == taxpercentage);
                    ts.Tax += tax;
                    ts.Value += amount;
                }
                else
                {
                    TaxSummary.Add(new TaxInfo()
                    {
                        Percentage = taxpercentage,
                        Tax = tax,
                        Value = amount
                    });
                }
            }
        }

        public class TallyVoucher
        {
            public string company { get; set; }
            public string machine { get; set; }
            public string port { get; set; }
            public List<InfyPOS.Tally.TallyXML.Voucher> vouchers { get; set; }
            public List<InfyPOS.Tally.TallyXML.Ledger> ledgers { get; set; }
            public List<InfyPOS.Tally.TallyXML.LedgerGroup> groups { get; set; }
            public List<BillInfo> BillInfos { get; internal set; }
            public List<CardSales> CardSales { get; internal set; }
        }

        public TallyVoucher CreateSupplierPaymentVouchers(TallyConfig.SupplierPaymentCreteria criteria)
        {
            Data.CompanyDataService companydata = new Data.CompanyDataService();
            var companies = companydata.Search(new CompanyCreteria()
            {
                id = criteria.companyid
            });
            if (companies.Count != 1) return null;

            TallyConfig.Purchase purchaseConfig = new TallyConfig.Purchase(companies[0].tallyconfig);
            return CreateSupplierPayment(criteria, purchaseConfig);
        }

        public TallyVoucher CreateBOMPurchaseVouchers(TallyConfig.PurchaseCreteria criteria)
        {
            Data.CompanyDataService companydata = new Data.CompanyDataService();
            var companies = companydata.Search(new CompanyCreteria()
            {
                id = criteria.companyid
            });
            if (companies.Count != 1) return null;

            TallyConfig.Purchase purchaseConfig = new TallyConfig.Purchase(companies[0].tallyconfig);
            return CreateBOMPurchaseVouchers(criteria, purchaseConfig);
        }
        public TallyVoucher CreatePurchaseVouchers(TallyConfig.PurchaseCreteria criteria)
        {
            Data.CompanyDataService companydata = new Data.CompanyDataService();
            var companies = companydata.Search(new CompanyCreteria()
            {
                id = criteria.companyid
            });
            if (companies.Count != 1) return null;

            TallyConfig.Purchase purchaseConfig = new TallyConfig.Purchase(companies[0].tallyconfig);
            return CreatePurchaseVouchers(criteria, purchaseConfig);
        }

        internal TallyVoucher CreatePettyCashVouchers(TallyConfig.SalesCreteria criteria)
        {
            Data.CompanyDataService companydata = new Data.CompanyDataService();
            var companies = companydata.Search(new CompanyCreteria()
            {
                id = criteria.companyid
            });
            if (companies.Count != 1) return null;

            TallyConfig.Purchase purchaseConfig = new TallyConfig.Purchase(companies[0].tallyconfig);
            return CreatePettyCashVouchers(criteria, purchaseConfig);
        }

        internal TallyVoucher CreateCreditPaymentVouchers(TallyConfig.SalesCreteria criteria)
        {
            Data.CompanyDataService companydata = new Data.CompanyDataService();
            var companies = companydata.Search(new CompanyCreteria()
            {
                id = criteria.companyid
            });
            if (companies.Count != 1) return null;

            TallyConfig.Purchase purchaseConfig = new TallyConfig.Purchase(companies[0].tallyconfig);
            return CreateCreditPaymentVouchers(criteria, purchaseConfig);
        }

        internal TallyVoucher CreateVouchers(TallyConfig.SalesCreteria criteria)
        {
            Data.CompanyDataService companydata = new Data.CompanyDataService();
            var companies = companydata.Search(new CompanyCreteria()
            {
                id = criteria.companyid
            });
            if (companies.Count != 1) return null;

            TallyConfig.Purchase purchaseConfig = new TallyConfig.Purchase(companies[0].tallyconfig);
            return CreateVouchers(criteria, purchaseConfig);
        }
        internal TallyVoucher CreateVouchers(TallyConfig.SalesCreteria criteria, TallyConfig.Purchase config)
        {
            VoucherDataService voucherDataService = new VoucherDataService() { Context = this.Context };
            var pettycash = voucherDataService.Search(new VoucherCreteria()
            {
                sourcetype = 0,
                fromdate = criteria.fromdate,
                todate = criteria.todate,
                companyid = criteria.companyid,
                showdetail = true,
                locationid = criteria.locationid
            });
            pettycash.RemoveAll(x => x.sourcetype != 0);
            LedgerDataService ledgerDataService = new LedgerDataService()
            {
                Context = this.Context,
            };
            List<long> ids = new List<long>();
            pettycash.ForEach(x =>
            {
                ids.Add(x.adjustmentledgerid);
                ids.AddRange(x.voucheritems.ConvertAll(y => y.ledgerid));
            });
            var ledgers = ledgerDataService.Search(new LedgerCreteria()
            {
                ids = ids
            });

            List<InfyPOS.Tally.TallyXML.Voucher> vouchers = new List<InfyPOS.Tally.TallyXML.Voucher>();
            foreach (var item in pettycash)
            {
                vouchers.Add(config.CreateVoucher(item, ledgers));
            }
            var voucher = CreateVoucher(vouchers);
            voucher.company = string.IsNullOrEmpty(config.config.companyname) ? "TESTING" : config.config.companyname;
            voucher.port = config.config.port;
            voucher.machine = config.config.machine;

            InfyPOS.Tally.TallyXML tallyInterface = new InfyPOS.Tally.TallyXML("http://" + config.config.machine + ":" + config.config.port);
            tallyInterface.Context = this.Context;
            //tallyInterface.Transer(tallyInterface.CreateXML(voucher));
            return tallyInterface.CreateXML(voucher, "");
        }
        internal TallyVoucher CreatePettyCashVouchers(TallyConfig.SalesCreteria criteria, TallyConfig.Purchase config)
        {
            VoucherDataService voucherDataService = new VoucherDataService() { Context = this.Context };
            var pettycash = voucherDataService.Search(new VoucherCreteria()
            {
                sourcetype = 1,
                fromdate = criteria.fromdate,
                todate = criteria.todate,
                companyid = criteria.companyid,
                showdetail = true,
                locationid = criteria.locationid
            });
            LedgerDataService ledgerDataService = new LedgerDataService()
            {
                Context = this.Context,
            };
            List<long> ids = new List<long>();
            pettycash.ForEach(x =>
            {
                ids.Add(x.adjustmentledgerid);
                ids.AddRange(x.voucheritems.ConvertAll(y => y.ledgerid));
            });
            var ledgers = ledgerDataService.Search(new LedgerCreteria()
            {
                ids = ids
            });

            List<InfyPOS.Tally.TallyXML.Voucher> vouchers = new List<InfyPOS.Tally.TallyXML.Voucher>();
            foreach (var item in pettycash)
            {
                vouchers.Add(config.CreatePettyCashVoucher(item, ledgers));
            }
            var voucher = CreateVoucher(vouchers);
            voucher.company = string.IsNullOrEmpty(config.config.companyname) ? "TESTING" : config.config.companyname;
            voucher.port = config.config.port;
            voucher.machine = config.config.machine;

            InfyPOS.Tally.TallyXML tallyInterface = new InfyPOS.Tally.TallyXML("http://" + config.config.machine + ":" + config.config.port);
            tallyInterface.Context = this.Context;
            //tallyInterface.Transer(tallyInterface.CreateXML(voucher));
            return tallyInterface.CreateXML(voucher, "");
        }

        internal TallyVoucher CreateCreditPaymentVouchers(TallyConfig.SalesCreteria criteria, TallyConfig.Purchase config)
        {

            string query = @"select customerpayment.id,paidon,customerpayment.receiptno,customer.name,
                            customerpayment.companyid,customer.gstno,customerpayment.customerid,customer.address,
                            customer.name as suppliercompany,bank.name as bankname,cast(paymentinfo->>'paymentmode' as int) paymentmode, 
                            state.name as statename,city.name as cityname,bill.billdate,
                            case when customer.isinterstatesale then true else customer.isinterstatesale end isinterstatesale,
                            customerinvoice.billno,customerinvoice.paid,customerinvoice.discount							
                            from customerpayment
                            inner join customerinvoice on customerinvoice.paymentid = customerpayment.id
                            inner join customer on customer.id = customerpayment.customerid
                            left outer join bill on bill.id = customerinvoice.billid
                            left outer join referencelist state on state.id = customer.stateid
                            left outer join referencelist city on city.id = customer.cityid
							left outer join referencelist bank on bank.id = cast(paymentinfo->>'bankid' as bigint)
                            where customerpayment.isactive=true and customerinvoice.isactive = true 
                            and customerpayment.paidon>=@fromdate and customerpayment.paidon<@todate and customerpayment.companyid=@companyid {0}";

            SortedList<string, object> param = new SortedList<string, object>();
            param.Add("fromdate", criteria.fromdate);
            param.Add("todate", criteria.todate.Date.AddDays(1));
            param.Add("companyid", criteria.companyid);
            if (criteria.customerid > 0)
            {
                param.Add("customerid", criteria.customerid);
                query = string.Format(query, " and customerpayment.customerid=@customerid");
            }
            else
            {
                query = string.Format(query, "");
            }


            SortedDictionary<long, CustomerPaymentInfo> invoicelist = new SortedDictionary<long, CustomerPaymentInfo>();

            using (System.Data.IDataReader reader = ExecuteReader(query, param))
            {
                while (reader.Read())
                {
                    CustomerPaymentInfo invoice = new CustomerPaymentInfo();
                    invoice.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                    invoice.paidon = reader["paidon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["paidon"]);
                    invoice.customername = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
                    invoice.statename = reader["statename"] == DBNull.Value ? "" : reader["statename"].ToString();
                    invoice.cityname = reader["cityname"] == DBNull.Value ? "" : reader["cityname"].ToString();
                    invoice.bankname = reader["bankname"] == DBNull.Value ? "" : reader["bankname"].ToString();
                    if (reader["gstno"] != DBNull.Value)
                        invoice.gstno = reader["gstno"].ToString();
                    if (reader["address"] != DBNull.Value)
                        invoice.address = reader["address"].ToString();
                    invoice.customerid = reader["customerid"] == DBNull.Value ? "" : reader["customerid"].ToString();
                    invoice.isinterstatesale = reader["isinterstatesale"] == DBNull.Value ? false : (bool)reader["isinterstatesale"];

                    int paymentmode = reader["paymentmode"] == DBNull.Value ? 0 : (Int32)reader["paymentmode"];
                    switch (paymentmode)
                    {
                        case 0:
                            invoice.paymentmode = "NEFT/RTGS";
                            break;
                        case 1:
                            invoice.paymentmode = "Cheque/DD";
                            break;
                        case 2:
                            invoice.paymentmode = "Cash";
                            invoice.iscash = true;
                            break;
                    }
                    invoice.allocations = new List<CustomerPaymentInfo.Allocation>();
                    if (invoicelist.ContainsKey(invoice.id))
                        invoice = invoicelist[invoice.id];
                    else
                        invoicelist.Add(invoice.id, invoice);

                    invoice.allocations.Add(new CustomerPaymentInfo.Allocation()
                    {
                        billno = reader["billno"] == DBNull.Value ? "" : reader["billno"].ToString(),
                        paid = reader["paid"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["paid"]),
                        billdate = reader["billdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["billdate"]),
                        discount = reader["discount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discount"])
                    });
                }

            }

            return Transform(config, invoicelist.Values.ToList());
        }

        public TallyVoucher Transform(TallyConfig.Purchase config, List<CustomerPaymentInfo> invoiceList)
        {
            List<InfyPOS.Tally.TallyXML.Voucher> vouchers = new List<InfyPOS.Tally.TallyXML.Voucher>();
            foreach (var item in invoiceList)
            {
                vouchers.Add(config.CreateVoucher(item));
            }
            var voucher = CreateVoucher(vouchers);
            voucher.company = string.IsNullOrEmpty(config.config.companyname) ? "TESTING" : config.config.companyname;
            voucher.port = config.config.port;
            voucher.machine = config.config.machine;
            if (config.config.configuration.Data_Sales_Master)
            {
                voucher.ledgers = new List<TallyXML.Ledger>();
                voucher.groups = new List<TallyXML.LedgerGroup>();
            }
            InfyPOS.Tally.TallyXML tallyInterface = new InfyPOS.Tally.TallyXML("http://" + config.config.machine + ":" + config.config.port);
            tallyInterface.Context = this.Context;
            //tallyInterface.Transer(tallyInterface.CreateXML(voucher));
            return tallyInterface.CreateXML(voucher, "Agst Ref");
        }


        internal TallyVoucher CreateSalesVouchers(TallyConfig.SalesCreteria criteria)
        {
            Data.CompanyDataService companydata = new Data.CompanyDataService();
            var companies = companydata.Search(new CompanyCreteria()
            {
                id = criteria.companyid
            });
            if (companies.Count != 1) return null;

            TallyConfig.Purchase purchaseConfig = new TallyConfig.Purchase(companies[0].tallyconfig);
            return CreateSalesVouchers(criteria, purchaseConfig);
        }

        internal TallyVoucher CreateSalesVouchers(TallyConfig.SalesCreteria criteria, TallyConfig.Purchase config)
        {
            /*
            with billtaxadjustment
            as
            (
            Select billdate,billitems.barcode,billid,billitems.id,billitems.tax,salablegoods.price,billitems.receivable,
	            billitems.taxpercentage,billitems.qty
	            ,billitems.receivable/(100+billitems.taxpercentage)*billitems.taxpercentage newtax
                                        from bill 
                                        inner join billitems on billitems.billid = bill.id
	            inner join salablegoods on salablegoods.id = billitems.salablegoodsid
                                        where  bill.isactive = true 
							            and bill.billtype != 1 and bill.billtype != 9
							            and bill.billdate>='2018-10-01'
							            and abs(abs(billitems.tax::numeric) - abs(billitems.receivable/(100+billitems.taxpercentage)*billitems.taxpercentage::numeric))>0.5::numeric
            )

            select * from billtaxadjustment
            update billitems set tax = billtaxadjustment.newtax
            from billtaxadjustment where
            billtaxadjustment.id = billitems.id

            select * from billitems where billid=539132
            update bill set tax =23.7142,receivable=498,additionaldiscount=0 where id=539132
            update billitems set receivable=price,tax=billitems.price/(100+billitems.taxpercentage)*billitems.taxpercentage,
            additionaldiscount=0 where billid=539132
            update billitems set rate=price-tax where billid=539132
            */

            /*Select bill.billdate::date,BILLTYPE,
            case when bill.receivable<0 then 'returnbill' 
            else 'normlabill' end isreturn,
            bill.isinterstatesale,bill.locationid,sum(bill.receivable) amount,sum(rounding) rounding
                            from bill 
                            where bill.isactive = true and 
                            bill.billtype != 1 and 
                            (bill.iscreditbill = false) and bill.billtype != 9 
                            and locationid =58
                            and bill.billdate>='2025-05-07' and bill.billdate<'2025-05-08'  
                            group by bill.billdate::date,BILLTYPE,bill.isinterstatesale,
                            case when bill.receivable<0 then 'returnbill' else 'normlabill' end,bill.locationid;

            select billsettlementmaster.settlementon::date,sum(paid_card) card,
                            cast(paymentinfo->>'card_bankid' as bigint) bankid,
                            cast(paymentinfo->>'upiproviderid' as bigint) upiproviderid, 
                            billsettlementmaster.locationid,
                            sum(paid_upi) upi,
                            sum(paid_cash) paid_cash,
                            sum(paid_voucher) paid_voucher,
                            sum(cast(paymentinfo->> 'adjustment_paid' as numeric)) adjustment_paid,
                            sum(cast(paymentinfo->>'creditnote_refund' as numeric)) creditnote_refund,
                            sum(case when paid_voucher>0 and cast(paymentinfo->>'creditnote_refund' as numeric)>0 then 
                            cast(paymentinfo->>'creditnote_refund' as numeric) else 0 end) creditnote_reisssue,
                            sum(case when paid_credit>0 and paid_cash>0 then paid_cash else 0 end) partialcash,
                            sum(case when paid_credit>0 and paid_cash>0 then paid_credit else 0 end) partialcredit,
                            sum(case when billsettlementmaster.paidamount<0 then cast(paymentinfo->>'cash_return' as numeric) else 0 end) refund
                            from billsettlementmaster
                            where billsettlementmaster.isactive = true
                            and  billsettlementmaster.locationid = 58
                            and billsettlementmaster.settlementon >= '2025-05-07' 
                            and billsettlementmaster.settlementon < '2025-05-08' 
                            group by billsettlementmaster.locationid,billsettlementmaster.settlementon::date,cast(paymentinfo->>'upiproviderid' as bigint),
                            cast(paymentinfo->>'card_bankid' as bigint)*/


            string query = @"Select bill.billdate::date, bill.isinterstatesale,bill.locationid,sum(bill.receivable) amount,sum(rounding) rounding
                from bill 
                where bill.companyid=@companyid and bill.isactive = true and bill.billtype != 1 and 
                (bill.iscreditbill = false) and bill.billtype != 9 and bill.billdate>=@fromdate and bill.billdate<@todate {0} {1}
                group by bill.billdate::date,bill.isinterstatesale,bill.locationid;

                Select bill.id,bill.billdate::date,bill.billno,customer.name as customername, customer.billingname,billattributes->'ecom'->>'channel' ecom,
                customer.creditdays,bill.isinterstatesale,bill.locationid,referencelist.name statename,customer.gstno,agent.name agentname, bill.customerid,
                customer.area as customerarea,customer.address,city.name cityname,customer.customertype,sum(bill.receivable) amount,sum(rounding) rounding,
                einvoice,ewb,custgroup.name customergroup,isb2b,returnbillno
                from bill 
                inner join customer on customer.id = bill.customerid
                left outer join billinbox on billinbox.billid = bill.id and billinbox.isactive=true
                left outer join agent on agent.id = customer.agentid
                left outer join referencelist on referencelist.id = customer.stateid
                left outer join referencelist city on city.id = customer.cityid
                left outer join referencelist custgroup on custgroup.id = customergroupid
                where bill.companyid=@companyid and bill.isactive = true and (bill.billtype = 1  or (bill.billtype=8 and bill.iscreditbill=true)) and bill.billdate>=@fromdate and bill.billdate<@todate {0} {1}
                group by bill.id,bill.billdate::date,bill.isinterstatesale,bill.locationid,bill.billno,customer.name,customer.address,billattributes->'ecom'->>'channel',custgroup.name,
                customer.customertype,customer.area,referencelist.name,customer.gstno,customer.billingname,agent.name,city.name,customer.creditdays,bill.customerid,einvoice,ewb,isb2b,returnbillno;

                Select bill.billdate::date, bill.isinterstatesale,bill.locationid,billitems.taxpercentage,sum(billitems.receivable) amount,sum(billitems.tax) tax,sum(billitems.qty) qty
                from bill 
                inner join billitems on billitems.billid = bill.id and billitems.isactive=true
                where bill.companyid=@companyid and bill.isactive = true and bill.billtype != 1 and bill.billtype != 9 and
                (bill.iscreditbill = false) and bill.billdate>=@fromdate and bill.billdate<@todate {0} {1}
                group by bill.billdate::date,bill.isinterstatesale,bill.locationid,billitems.taxpercentage
                order by bill.locationid,billitems.taxpercentage;

                Select bill.billdate::date, bill.isinterstatesale,bill.locationid,billitems.taxpercentage,bill.billno,sum(billitems.receivable) amount,sum(billitems.tax) tax,sum(billitems.qty) qty
                from bill 
                inner join customer on customer.id = bill.customerid
                inner join billitems on billitems.billid = bill.id and billitems.isactive=true
                where bill.companyid=@companyid and bill.isactive = true and (bill.billtype = 1  or (bill.billtype=8 and bill.iscreditbill=true)) and bill.billdate>=@fromdate and bill.billdate<@todate {0} {1}
                group by bill.billdate::date,bill.isinterstatesale,bill.locationid,billitems.taxpercentage,bill.billno
                order by bill.locationid,billitems.taxpercentage; 

                select billid,taxpercentage,sum(tax) tax,sum(amount) amount,sum(charge) charge
                from billadditionalcharges where billid in (Select bill.id
                from bill 
                inner join customer on customer.id = bill.customerid
                where bill.companyid=@companyid and bill.isactive = true and
                (bill.billtype = 1  or (bill.billtype=8 and bill.iscreditbill=true)) and 
                bill.billdate>=@fromdate and bill.billdate<@todate {0} {1}) and isactive=true
                group by taxpercentage,billid;";

            if (criteria.multicompany)
            {
                query += @"select 
                billsettlementmaster.settlementon::date,(paid_card) card,
                cast(paymentinfo->>'card_bankid' as bigint) bankid,
                cast(paymentinfo->>'upiproviderid' as bigint) upiproviderid, 
                billsettlementmaster.locationid,
                (paid_upi) upi,
                (case when paid_credit>0 and paid_cash>0 then paid_cash else 0 end) partialcash,
                (case when paid_credit>0 and paid_cash>0 then paid_credit else 0 end) partialcredit,
                billsettlementmaster.id,bill.companyid billedcompany,
                billsettlementmaster.companyid settledcompany,
                billsettlement.billno,billsettlementmaster.receivable settled,
                bill.receivable billed,(paid_cash) cash,
                (paid_voucher) paid_voucher,
                (cast(paymentinfo->> 'adjustment_paid' as numeric)) adjustment_paid,
				(cast(paymentinfo->>'creditnote_refund' as numeric)) creditnote_refund,
				(case when paid_voucher>0 and cast(paymentinfo->>'creditnote_refund' as numeric)>0 then cast(paymentinfo->>'creditnote_refund' as numeric) else 0 end) creditnote_reisssue,
                (case when billsettlementmaster.paidamount<0 then cast(paymentinfo->>'cash_return' as numeric) else 0 end) refund
                from billsettlementmaster
                inner join billsettlement on billsettlement.billsettlementmasterid = billsettlementmaster.id and billsettlement.isactive=true
                inner join bill on bill.id = billsettlement.billid
                where billsettlementmaster.isactive = true
                and billsettlementmaster.settlementon >= @fromdate
                and billsettlementmaster.settlementon < @todate
                {2};";
            }
            else
            {
                query += @" select billsettlementmaster.settlementon::date,sum(paid_card) card,
                cast(paymentinfo->>'card_bankid' as bigint) bankid,
                cast(paymentinfo->>'upiproviderid' as bigint) upiproviderid, 
                billsettlementmaster.locationid,
                sum(paid_upi) upi,
                sum(case when paid_credit>0 and paid_cash>0 then paid_cash else 0 end) partialcash,
                sum(case when paid_credit>0 and paid_cash>0 then paid_credit else 0 end) partialcredit,
                sum(paid_voucher) paid_voucher,
                sum(cast(paymentinfo->> 'adjustment_paid' as numeric)) adjustment_paid,
				sum(cast(paymentinfo->>'creditnote_refund' as numeric)) creditnote_refund,
				sum(case when paid_voucher>0 and cast(paymentinfo->>'creditnote_refund' as numeric)>0 then cast(paymentinfo->>'creditnote_refund' as numeric) else 0 end) creditnote_reisssue,
                sum(case when billsettlementmaster.paidamount<0 then cast(paymentinfo->>'cash_return' as numeric) else 0 end) refund
                from billsettlementmaster
                where billsettlementmaster.isactive = true and billsettlementmaster.companyid=@companyid
                and billsettlementmaster.settlementon >= @fromdate and billsettlementmaster.settlementon < @todate {2}
                group by billsettlementmaster.locationid,billsettlementmaster.settlementon::date,cast(paymentinfo->>'upiproviderid' as bigint),
                cast(paymentinfo->>'card_bankid' as bigint)";
            }

            SortedList<string, object> param = new SortedList<string, object>();
            param.Add("fromdate", criteria.fromdate.Date);
            param.Add("todate", criteria.todate.Date.AddDays(1));
            param.Add("companyid", criteria.companyid);
            string customercond = "";
            string locationcond = "";
            if (criteria.customerid > 0)
            {
                param.Add("customerid", criteria.customerid);
                customercond = " and bill.customerid=@customerid";
            }

            if (criteria.locationid > 0)
            {
                param.Add("locationid", criteria.locationid);
                locationcond = " and bill.locationid=@locationid";
            }

            query = string.Format(query, locationcond, customercond, locationcond.Replace("bill.", "billsettlementmaster."));


            Data.ReferencelistDataService locationdata = new Data.ReferencelistDataService();
            var locations = locationdata.Search(new ReferencelistCreteria()
            {
                type = "LOCATION"
            });

            var bankdata = locationdata.Search(new ReferencelistCreteria()
            {
                type = "BANK"
            });

            var upidata = locationdata.Search(new ReferencelistCreteria()
            {
                type = "UPIPROVIDER"
            });

            var billList = new SortedDictionary<string, BillInfo>();
            List<CardSales> cardSalesList = new List<CardSales>();
            List<CardSalesMultiCompany> cardSalesMultiList = new List<CardSalesMultiCompany>();
            decimal partialcash = 0;
            decimal partialcredit = 0;
            using (System.Data.IDataReader reader = ExecuteReader(query, param))
            {
                while (reader.Read())
                {
                    BillInfo invoice = new BillInfo();
                    invoice.Date = reader["billdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["billdate"]);
                    invoice.IsInterstate = reader["isinterstatesale"] == DBNull.Value ? false : Convert.ToBoolean(reader["isinterstatesale"]);
                    invoice.LocationID = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                    invoice.Amount = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"]);
                    invoice.Rounding = reader["rounding"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["rounding"]);
                    invoice.IsCredit = false;
                    invoice.Location = locations.Find(e => e.id == invoice.LocationID).code;
                    invoice.LocationName = locations.Find(e => e.id == invoice.LocationID).name;
                    billList.Add(invoice.Key, invoice);
                }
                if (reader.NextResult())
                {
                    while (reader.Read())
                    {
                        BillInfo invoice = new BillInfo();
                        invoice.Date = reader["billdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["billdate"]);
                        invoice.IsInterstate = reader["isinterstatesale"] == DBNull.Value ? false : Convert.ToBoolean(reader["isinterstatesale"]);
                        invoice.LocationID = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                        invoice.CustomerID = reader["CustomerID"] == DBNull.Value ? 0 : Convert.ToInt64(reader["CustomerID"]);
                        invoice.Amount = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"]);
                        invoice.Rounding = reader["rounding"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["rounding"]);
                        invoice.BillNo = reader["BillNo"] == DBNull.Value ? "" : reader["BillNo"].ToString();
                        invoice.ReturnBillno= reader["ReturnBillno"] == DBNull.Value ? "" : reader["ReturnBillno"].ToString();
                        invoice.IsB2B = reader["isb2b"] == DBNull.Value ? false : Convert.ToBoolean(reader["isb2b"]);
                        invoice.EcomChennal = reader["ecom"] == DBNull.Value ? "" : reader["ecom"].ToString();
                        invoice.CreditDays = reader["creditdays"] == DBNull.Value ? 0 : Convert.ToInt32(reader["creditdays"]);
                        invoice.CustomerName = reader["CustomerName"] == DBNull.Value ? "No Customer" : reader["CustomerName"].ToString();
                        invoice.billingname = reader["billingname"] == DBNull.Value ? "" : reader["billingname"].ToString();
                        invoice.customergroup = reader["customergroup"] == DBNull.Value ? "" : reader["customergroup"].ToString();
                        invoice.agentname = reader["agentname"] == DBNull.Value ? "No Agents" : reader["agentname"].ToString();

                        if (string.IsNullOrEmpty(invoice.billingname))
                            invoice.billingname = invoice.CustomerName;

                        if (string.IsNullOrEmpty(invoice.customergroup))
						{
							if(!string.IsNullOrEmpty(invoice.billingname))
	                            invoice.customergroup = invoice.billingname;
							else
								invoice.customergroup = invoice.CustomerName;
						}
                        invoice.CustomerArea = reader["customerarea"] == DBNull.Value ? "DEFAULT" : reader["customerarea"].ToString();
                        invoice.Cityname = reader["cityname"] == DBNull.Value ? "DEFAULT" : reader["cityname"].ToString();

                        invoice.Statename = reader["statename"] == DBNull.Value ? "NO State" : reader["statename"].ToString();
                        invoice.Gstno = reader["gstno"] == DBNull.Value ? "" : reader["gstno"].ToString();
                        invoice.Address = reader["Address"] == DBNull.Value ? "" : reader["Address"].ToString();

                        invoice.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);

                        invoice.IsEmployeeCredit = (reader["CustomerType"] != DBNull.Value && (int)reader["customertype"] == 1);
                        invoice.Location = locations.Find(e => e.id == invoice.LocationID).code;
                        invoice.LocationName = locations.Find(e => e.id == invoice.LocationID).name;
                        invoice.IsCredit = true;
                        invoice.CashPaid = 0;
                        
                        if (string.IsNullOrEmpty(invoice.Gstno) && invoice.IsB2B)
                            invoice.IsB2B = false;

                        invoice.einv = reader["einvoice"] == DBNull.Value ? "" : reader["einvoice"].ToString();
                        invoice.ewb = reader["ewb"] == DBNull.Value ? "" : reader["ewb"].ToString();

                        billList.Add(invoice.Key, invoice);
                    }
                }

                if (reader.NextResult())
                {
                    while (reader.Read())
                    {
                        BillInfo invoice = new BillInfo();
                        invoice.Date = reader["billdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["billdate"]);
                        invoice.IsInterstate = reader["isinterstatesale"] == DBNull.Value ? false : Convert.ToBoolean(reader["isinterstatesale"]);
                        invoice.LocationID = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                        invoice.IsCredit = false;
                        invoice = billList[invoice.Key];

                        decimal taxpercentage = reader["taxpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["taxpercentage"]);
                        decimal amount = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"]);
                        decimal tax = reader["tax"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["tax"]);
                        invoice.qty += reader["qty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["qty"]);
                        if (taxpercentage > 0)
                        {
                            amount = Math.Round(amount, 2);
                            tax = Math.Round(amount / (taxpercentage + 100) * taxpercentage, 2);
                        }
                        if (!invoice.IsInterstate)
                        {
                            tax = (Math.Round((amount - tax) * (taxpercentage / 2) / 100, 2, MidpointRounding.AwayFromZero)) * 2;
                        }
                        invoice.AddTax(taxpercentage, tax, amount);
                    }
                }

                if (reader.NextResult())
                {
                    while (reader.Read())
                    {
                        BillInfo invoice = new BillInfo();
                        invoice.Date = reader["billdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["billdate"]);
                        invoice.IsInterstate = reader["isinterstatesale"] == DBNull.Value ? false : Convert.ToBoolean(reader["isinterstatesale"]);
                        invoice.LocationID = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                        invoice.BillNo = reader["BillNo"] == DBNull.Value ? "" : reader["BillNo"].ToString();
                        invoice.IsCredit = true;
                        invoice = billList[invoice.Key];

                        decimal taxpercentage = reader["taxpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["taxpercentage"]);
                        decimal amount = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"]);
                        decimal tax = reader["tax"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["tax"]);
                        invoice.qty += reader["qty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["qty"]);
                        if (taxpercentage > 0)
                        {
                            amount = Math.Round(amount, 2);
                            tax = Math.Round(amount / (taxpercentage + 100) * taxpercentage, 2);
                        }
                        if (!invoice.IsInterstate)
                        {
                            var newtax = (Math.Round((amount - tax) * (taxpercentage / 2) / 100, 2, MidpointRounding.AwayFromZero)) * 2;
                            if (newtax > tax)
                            {
                                amount += tax - newtax;
                            }
                            else if (newtax < tax)
                            {
                                amount += newtax - tax;
                            }
                            tax = newtax;
                        }

                        invoice.AddTax(taxpercentage, tax, amount);
                    }
                }

                if (reader.NextResult())
                {
                    while (reader.Read())
                    {

                        var billid = reader["billid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["billid"]);
                        var billinfo = billList.Values.First(x => x.id == billid);

                        decimal taxpercentage = reader["taxpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["taxpercentage"]);
                        decimal amount = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"]);
                        decimal charge = reader["charge"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["charge"]);
                        decimal tax = reader["tax"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["tax"]);

                        if (!billinfo.IsInterstate)
                        {
                            tax = (Math.Round((amount - tax) * (taxpercentage / 2) / 100, 2, MidpointRounding.AwayFromZero)) * 2;
                        }

                        billinfo.AddTax(taxpercentage, tax, amount);
                    }
                }

                if (reader.NextResult())
                {
                    if (criteria.multicompany)
                    {
                        while (reader.Read())
                        {
                            CardSalesMultiCompany cardSales = new CardSalesMultiCompany();
                            cardSales.date = reader.GetDateTime(0);
                            cardSales.amount = reader.GetDecimal(1);
                            cardSales.paid_voucher = reader["paid_voucher"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["paid_voucher"]);
                            cardSales.adjustment_paid = reader["adjustment_paid"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["adjustment_paid"]);
                            cardSales.creditnote_refund = reader["creditnote_refund"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["creditnote_refund"]);
                            cardSales.creditnote_reisssue = reader["creditnote_reisssue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["creditnote_reisssue"]);
                            cardSales.cash_refund = reader["refund"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["refund"]);
                            cardSales.bankid = reader.GetInt64(2);
                            cardSales.locationid = reader.GetInt64(4);
                            if (bankdata.Exists(e => e.id == cardSales.bankid))
                            {
                                cardSales.bank = bankdata.Find(e => e.id == cardSales.bankid).name;
                            }
                            else if (bankdata.Count() == 1)
                            {
                                cardSales.bank = bankdata.First().name;
                            }
                            else
                            {
                                cardSales.bank = "DEFAULT BANK";
                            }

                            cardSales.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                            cardSales.billedcompany = reader["billedcompany"] == DBNull.Value ? 0 : Convert.ToInt64(reader["billedcompany"]);
                            cardSales.settledcompany = reader["settledcompany"] == DBNull.Value ? 0 : Convert.ToInt64(reader["settledcompany"]);
                            cardSales.settled = reader["id"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["settled"]);
                            cardSales.billed = reader["billed"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["billed"]);
                            cardSales.cash = reader["cash"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["cash"]);
                            cardSales.billno = reader["billno"] == DBNull.Value ? "" : Convert.ToString(reader["billno"]);

                            partialcash += reader.GetDecimal(6);
                            partialcredit += reader.GetDecimal(7);



                            cardSalesMultiList.Add(cardSales);
                            if (!reader.IsDBNull(5) && reader.GetDecimal(5) > 0)
                            {
                                cardSales = new CardSalesMultiCompany();
                                cardSales.date = reader.GetDateTime(0);
                                cardSales.upiid = reader.GetInt64(3);
                                cardSales.amount = reader.GetDecimal(5);
                                cardSales.locationid = reader.GetInt64(4);
                                cardSales.isupi = true;

                                cardSales.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                                cardSales.billedcompany = reader["billedcompany"] == DBNull.Value ? 0 : Convert.ToInt64(reader["billedcompany"]);
                                cardSales.settledcompany = reader["settledcompany"] == DBNull.Value ? 0 : Convert.ToInt64(reader["settledcompany"]);
                                cardSales.settled = reader["id"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["settled"]);
                                cardSales.billed = reader["billed"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["billed"]);
                                cardSales.cash = reader["cash"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["cash"]);
                                cardSales.billno = reader["billno"] == DBNull.Value ? "" : Convert.ToString(reader["billno"]);


                                if (upidata.Exists(e => e.id == cardSales.upiid))
                                {
                                    cardSales.upi = upidata.Find(e => e.id == cardSales.upiid).name;
                                }
                                else if (upidata.Count() == 1)
                                {
                                    cardSales.upi = upidata.First().name;
                                }
                                else
                                {
                                    cardSales.upi = "DEFAULT UPI";
                                }
                                cardSalesMultiList.Add(cardSales);
                            }
                        }
                        List<CardSalesMultiCompany> fcardSalesMultiList = new List<CardSalesMultiCompany>();
                        var propersettled = cardSalesMultiList.GroupBy(x => x.id).ToList().FindAll(y => y.ToList().All(z => z.settledcompany == z.billedcompany));
                        propersettled.ForEach(x =>
                        {
                            if (criteria.companyid == x.First().billedcompany)
                            {
                                fcardSalesMultiList.AddRange(x.ToList());
                            }
                        });

                        if (fcardSalesMultiList.Count > 0)
                        {
                            cardSalesList.AddRange(fcardSalesMultiList.GroupBy(y => new { y.date, y.upiid, y.isupi, y.bankid, y.bank, y.upi, y.locationid }).ToList().ConvertAll(z => new CardSales()
                            {
                                date = z.Key.date,
                                isupi = z.Key.isupi,
                                bank = z.Key.bank,
                                upi = z.Key.upi,
                                bankid = z.Key.bankid,
                                upiid = z.Key.upiid,
                                locationid = z.Key.locationid,
                                amount = z.Sum(a => a.amount),
                                paid_voucher = z.Sum(a => a.isupi ? 0 : a.paid_voucher),
                                adjustment_paid = z.Sum(a => a.isupi ? 0 : a.adjustment_paid),
                                creditnote_refund = z.Sum(a => a.isupi ? 0 : a.creditnote_refund),
                                creditnote_reisssue = z.Sum(a => a.isupi ? 0 : a.creditnote_reisssue),
                            }));
                        }

                        var impropersettled = cardSalesMultiList.GroupBy(x => x.id).ToList().FindAll(y => !y.ToList().All(z => z.settledcompany == z.billedcompany));
                        List<CardSales> impropercardSales = new List<CardSales>();
                        impropersettled.ForEach(x =>
                        {
                            var items = x.ToList().OrderBy(y => y.settledcompany).ToList();
                            var card = x.Sum(y => y.isupi ? 0 : y.amount);
                            var upi = x.Sum(y => !y.isupi ? 0 : y.amount);
                            var cash = x.Sum(y => y.billed) - (card + upi);
                            if ((card > 0 && upi == 0) || (upi > 0 && card == 0))
                            {
                                items.ForEach(y =>
                                {
                                    impropercardSales.Add(new CardSales()
                                    {
                                        amount = y.billed,
                                        bankid = y.bankid,
                                        bank = y.bank,
                                        date = y.date,
                                        isupi = y.isupi,
                                        locationid = y.locationid,
                                        upiid = y.upiid,
                                        companyid = y.billedcompany,
                                        paid_voucher = y.isupi ? 0 : y.paid_voucher,
                                        adjustment_paid = y.isupi ? 0 : y.adjustment_paid,
                                        creditnote_refund = y.isupi ? 0 : y.creditnote_refund,
                                        creditnote_reisssue = y.isupi ? 0 : y.creditnote_reisssue
                                    });
                                });

                            }
                            else
                            {
                                items.ForEach(y => y.amount = 0);
                                int index = 0;
                                while (index < items.Count && (card > 0 || upi > 0))
                                {

                                    impropercardSales.Add(new CardSales()
                                    {
                                        bankid = items[index].bankid,
                                        bank = items[index].bank,
                                        date = items[index].date,
                                        isupi = false,
                                        locationid = items[index].locationid,
                                        companyid = items[index].billedcompany,
                                        paid_voucher = items[index].isupi ? 0 : items[index].paid_voucher,
                                        adjustment_paid = items[index].isupi ? 0 : items[index].adjustment_paid,
                                        creditnote_refund = items[index].isupi ? 0 : items[index].creditnote_refund,
                                        creditnote_reisssue = items[index].isupi ? 0 : items[index].creditnote_reisssue
                                    });

                                    if (card > 0 && items[index].billed > 0)
                                    {
                                        var adjamount = items[index].billed >= card ? items[index].billed : card;
                                        items[index].amount = adjamount;
                                        impropercardSales.Add(new CardSales()
                                        {
                                            amount = adjamount,
                                            bankid = items[index].bankid,
                                            bank = items[index].bank,
                                            date = items[index].date,
                                            isupi = false,
                                            locationid = items[index].locationid,
                                            companyid = items[index].billedcompany
                                        });
                                        card -= adjamount;
                                    }
                                    if (upi > 0 && items[index].amount < items[index].billed)
                                    {
                                        var adjamount = (items[index].billed - items[index].amount) >= upi ? (items[index].billed - items[index].amount) : upi;
                                        items[index].amount += adjamount;
                                        impropercardSales.Add(new CardSales()
                                        {
                                            amount = adjamount,
                                            upiid = items[index].upiid,
                                            upi = items[index].upi,
                                            bank = items[index].bank,
                                            date = items[index].date,
                                            isupi = true,
                                            locationid = items[index].locationid,
                                            companyid = items[index].billedcompany
                                        });
                                        upi -= adjamount;
                                    }
                                    index++;
                                }
                            }
                        });

                        cardSalesList.AddRange(impropercardSales.FindAll(x => x.companyid == criteria.companyid).GroupBy(y => 
                        new { y.date, y.upiid, y.isupi, y.bankid, y.bank, y.upi,y.locationid }).ToList().ConvertAll(z => new CardSales()
                        {
                            date = z.Key.date,
                            isupi = z.Key.isupi,
                            bank = z.Key.bank,
                            upi = z.Key.upi,
                            bankid = z.Key.bankid,
                            upiid = z.Key.upiid,
                            locationid=z.Key.locationid,
                            amount = z.Sum(a => a.amount),
                        }));
                    }
                    else
                    {
                        while (reader.Read())
                        {
                            CardSales cardSales = new CardSales();
                            cardSales.date = reader.GetDateTime(0);
                            cardSales.amount = reader.GetDecimal(1);
                            cardSales.bankid = reader.GetInt64(2);
                            cardSales.locationid = reader.GetInt64(4);

                            cardSales.amount = reader.GetDecimal(1);

                            cardSales.paid_voucher = reader["paid_voucher"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["paid_voucher"]);
                            cardSales.adjustment_paid = reader["adjustment_paid"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["adjustment_paid"]);
                            cardSales.creditnote_refund = reader["creditnote_refund"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["creditnote_refund"]);
                            cardSales.creditnote_reisssue = reader["creditnote_reisssue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["creditnote_reisssue"]);
                            cardSales.cash_refund = reader["refund"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["refund"]);

                            if (bankdata.Exists(e => e.id == cardSales.bankid))
                            {
                                cardSales.bank = bankdata.Find(e => e.id == cardSales.bankid).name;
                            }
                            else if (bankdata.Count() == 1)
                            {
                                cardSales.bank = bankdata.First().name;
                            }
                            else
                            {
                                cardSales.bank = "DEFAULT BANK";
                            }
                            partialcash += reader.GetDecimal(6);
                            partialcredit += reader.GetDecimal(7);

                            cardSalesList.Add(cardSales);
                            if (!reader.IsDBNull(5) && reader.GetDecimal(5) > 0)
                            {
                                cardSales = new CardSales();
                                cardSales.date = reader.GetDateTime(0);
                                cardSales.upiid = reader.GetInt64(3);
                                cardSales.amount = reader.GetDecimal(5);
                                cardSales.locationid = reader.GetInt64(4);
                                cardSales.isupi = true;



                                if (upidata.Exists(e => e.id == cardSales.upiid))
                                {
                                    cardSales.upi = upidata.Find(e => e.id == cardSales.upiid).name;
                                }
                                else if (upidata.Count() == 1)
                                {
                                    cardSales.upi = upidata.First().name;
                                }
                                else
                                {
                                    cardSales.upi = "DEFAULT UPI";
                                }
                                cardSalesList.Add(cardSales);
                            }
                        }
                    }
                }
            }



            if (partialcash > 0 && partialcredit > 0)
            {
                query = @"select billsettlementmaster.id,billid,billsettlementmaster.customerid,billno,paid_cash,
                paid_credit,billsettlement.billamount
                from billsettlementmaster
                inner join billsettlement 
                on billsettlement.billsettlementmasterid = billsettlementmaster.id
                where billsettlementmaster.isactive = true
                and billsettlementmaster.settlementon >= @fromdate and billsettlementmaster.settlementon < @todate 
                {0} {1}
                and paid_credit>0 and paid_cash>0";
                query = string.Format(query, locationcond.Replace("bill.", "billsettlementmaster."), customercond.Replace("bill.", "billsettlementmaster."));
                List<CashCreditSales> cashCreditSales = new List<CashCreditSales>();
                using (var reader = ExecuteReader(query, param))
                {
                    while (reader.Read())
                    {
                        CashCreditSales cashCredit = new CashCreditSales();
                        cashCredit.id = reader.GetInt64(0);
                        cashCredit.customerid = reader.GetInt64(1);
                        cashCredit.billid = reader.GetInt64(2);
                        cashCredit.billno = reader.GetString(3);
                        cashCredit.paid_cash = reader.GetDecimal(4);
                        cashCredit.paid_credit = reader.GetDecimal(5);
                        cashCredit.billamount = reader.GetDecimal(6);
                        cashCreditSales.Add(cashCredit);
                    }
                }

                foreach (var items in cashCreditSales.GroupBy(x => x.id))
                {
                    if (items.Count() > 1)
                    {
                        var fullcash = items.First().paid_cash;
                        foreach (var item in items)
                        {
                            var bill = billList.ToList().FindAll(x => x.Value.BillNo == item.billno);
                            if (bill[0].Value.Amount >= fullcash)
                            {
                                bill[0].Value.CashPaid = fullcash;
                            }
                            else
                            {
                                bill[0].Value.CashPaid = bill[0].Value.Amount;
                            }
                            fullcash -= bill[0].Value.CashPaid;
                            if (fullcash <= 0) break;
                        }
                    }
                    else
                    {
                        var bill = billList.ToList().FindAll(x => x.Value.BillNo == items.First().billno);
                        if (bill.Count == 1)
                        {
                            bill[0].Value.CashPaid = items.First().paid_cash;
                        }
                    }
                }
            }
            return Transform(config, criteria.autosynctax, billList.Values.ToList(), cardSalesList);
        }

        private TallyVoucher Transform(TallyConfig.Purchase config, bool autosynctax, List<BillInfo> list, List<CardSales> cardSales)
        {
            if (!string.IsNullOrEmpty(config.config.configuration.Bank))
            {
                foreach (var item in cardSales)
                {
                    if (list.Exists(e => e.Date == item.date && e.LocationID == item.locationid && !e.IsCredit))
                    {
                        var billinfo = list.Find(e => e.Date == item.date && !e.IsCredit && e.LocationID == item.locationid);
                        billinfo.AddCardSales(item);
                    }
                }
            }

            CompanyDataService companyDataService = new CompanyDataService() { Context = this.Context };
            var configuration = companyDataService.InternalVendorConfig();
            var transfercompanyid = configuration.ConvertAll(x => x.customerid);
            var transfers = list.FindAll(x => x.CustomerID > 0 && transfercompanyid.Contains(x.CustomerID));
            transfers.ForEach(x => x.stocktransfer = true);

            List<InfyPOS.Tally.TallyXML.Voucher> vouchers = new List<InfyPOS.Tally.TallyXML.Voucher>();
            foreach (var item in list)
            {
                //item.Amount = Math.Round(item.Amount, 0);
                item.TaxSummary.ForEach(e =>
                {
                    e.Tax = Math.Round(e.Tax, 2);
                    e.Value = Math.Round(e.Value, 2);
                });
                var totalbillvalue = item.TaxSummary.Sum(e => e.Value);
                var totalcollection = item.Amount + item.CardAmount;
                //item.Rounding = totalcollection-totalbillvalue;
                vouchers.Add(config.CreateVoucher(item, autosynctax));
            }
            var voucher = CreateVoucher(vouchers);
            voucher.company = string.IsNullOrEmpty(config.config.companyname) ? "Enter Tally Company Name" : config.config.companyname;
            voucher.port = config.config.port;
            voucher.machine = config.config.machine;
            if (config.config.configuration.Data_Sales_Master)
            {
                voucher.ledgers = new List<TallyXML.Ledger>();
                voucher.groups = new List<TallyXML.LedgerGroup>();
            }
            InfyPOS.Tally.TallyXML tallyInterface = new InfyPOS.Tally.TallyXML("http://" + config.config.machine + ":" + config.config.port);
            tallyInterface.Context = this.Context;
            var tvoucher = tallyInterface.CreateXML(voucher, "New Ref");
            tvoucher.BillInfos = list;
            tvoucher.CardSales = cardSales;
            return tvoucher;
        }




        private TallyVoucher CreateBOMPurchaseReturnVouchers(TallyConfig.PurchaseReturnCreteria criteria, TallyConfig.Purchase config)
        {
            Data.ReferencelistDataService locationdata = new Data.ReferencelistDataService();
            var locations = locationdata.Search(new ReferencelistCreteria()
            {
                type = "LOCATION"
            });

            string query = @"select BOMInvoicereturn.id,BOMInvoicereturn.returnno,BOMInvoicereturn.returndate,discount,tax,amount,freightamt,total,
                suppliercompany.name as suppliercompany,supplier.name suppliername,suppliercompanyid,location.code locationcode,agent.name as agentname,
                state.name as statename,city.name as cityname,location.name locationname,suppliercompany.address,
                BOMInvoicereturn.isinterstatesale isinterstatesale,suppliergroup.name suppliergroup from BOMInvoicereturn
                left outer join suppliercompany on suppliercompany.id = BOMInvoicereturn.suppliercompanyid
                inner join supplier on supplier.id = suppliercompany.supplierid
                left outer join agent on agent.id = BOMInvoicereturn.agentid
                left outer join referencelist state on state.id = suppliercompany.stateid
                left outer join referencelist city on city.id = suppliercompany.cityid
                left outer join referencelist location on location.id = BOMInvoicereturn.locationid
                left outer join referencelist suppliergroup on suppliergroup.id = supplier.suppliergroupid
                where BOMInvoicereturn.companyid=@companyid and BOMInvoicereturn.isactive = true and returndate>=@fromdate and returndate<@todate {0};

                select BOMInvoicereturnid,BOMInvoicereturnitems.taxpercentage,
                BOMInvoicereturn.isinterstatesale isinterstatesale,
                sum(BOMInvoicereturnitems.amount-BOMInvoicereturnitems.discount) amount,sum(BOMInvoicereturnitems.tax) tax from BOMInvoicereturnitems 
                inner join BOMInvoicereturn on BOMInvoicereturn.id = BOMInvoicereturnitems.BOMInvoicereturnid
                inner join suppliercompany on suppliercompany.id = BOMInvoicereturn.suppliercompanyid
                left outer join referencelist state on state.id = suppliercompany.stateid
                left outer join referencelist city on city.id = suppliercompany.cityid
                where BOMInvoicereturn.companyid=@companyid and BOMInvoicereturn.isactive = true and BOMInvoicereturnitems.isactive=true and returndate>=@fromdate and returndate<@todate {0}
                group by bomInvoicereturnid,BOMInvoicereturn.isinterstatesale,BOMInvoicereturnitems.taxpercentage";

            SortedList<string, object> param = new SortedList<string, object>();
            param.Add("fromdate", criteria.fromdate);
            param.Add("todate", criteria.todate.Date.AddDays(1));
            param.Add("companyid", criteria.companyid);

            string replacement = "";
            if (criteria.locationid > 0)
            {
                replacement += " and BOMInvoicereturn.locationid = @locationid";
                param.Add("locationid", criteria.locationid);
            }
            if (criteria.supplierid > 0)
            {
                param.Add("supplierid", criteria.supplierid);
                replacement += " and suppliercompany.supplierid=@supplierid";
            }

            query = string.Format(query, replacement);

            SortedDictionary<long, ReturnInfo> invoicelist = new SortedDictionary<long, ReturnInfo>();

            using (System.Data.IDataReader reader = ExecuteReader(query, param))
            {
                while (reader.Read())
                {
                    ReturnInfo invoice = new ReturnInfo();
                    invoice.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                    invoice.suppliercompanyid = reader["suppliercompanyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["suppliercompanyid"]);
                    invoice.isinterstatesale = reader["isinterstatesale"] == DBNull.Value ? false : Convert.ToBoolean(reader["isinterstatesale"]);
                    invoice.returnno = reader["returnno"] == DBNull.Value ? "" : reader["returnno"].ToString();
                    invoice.returndate = reader["returndate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["returndate"]);
                    invoice.totalamount = reader["total"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["total"]);
                    invoice.baseamount = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"]);
                    invoice.roundamount = invoice.totalamount - invoice.baseamount;
                    invoice.address = reader["address"] == DBNull.Value ? "" : reader["address"].ToString();
                    invoice.suppliercompany = reader["suppliercompany"] == DBNull.Value ? "" : reader["suppliercompany"].ToString();
                    invoice.suppliername = reader["suppliername"] == DBNull.Value ? "" : reader["suppliername"].ToString();
                    invoice.suppliergroup = reader["suppliergroup"] == DBNull.Value ? "" : reader["suppliergroup"].ToString();
                    invoice.statename = reader["statename"] == DBNull.Value ? "" : reader["statename"].ToString();
                    invoice.cityname = reader["cityname"] == DBNull.Value ? "" : reader["cityname"].ToString();
                    invoice.agentname = reader["agentname"] == DBNull.Value ? "" : reader["agentname"].ToString();
                    invoice.branchcode = reader["locationcode"] == DBNull.Value ? "" : reader["locationcode"].ToString();
                    invoice.branchname = reader["locationname"] == DBNull.Value ? "" : reader["locationname"].ToString();
                    invoicelist.Add(invoice.id, invoice);
                }
                if (reader.NextResult())
                {
                    while (reader.Read())
                    {
                        //sum(amounton - lrinvoicetax.discount) taxable,sum(lrinvoicetax.taxamount) tax,type,
                        //  lrinvoicetax.percentage,lrinvoicetax.isinterstatesale,invoiceid

                        TaxSummary invoice = new TaxSummary();
                        invoice.taxable = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"]);
                        invoice.tax = reader["tax"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["tax"]);
                        invoice.percentage = reader["taxpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["taxpercentage"]);
                        invoice.isinterstatesale = reader["isinterstatesale"] == DBNull.Value ? false : Convert.ToBoolean(reader["isinterstatesale"]);
                        invoice.invoiceid = reader["BOMInvoicereturnid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["BOMInvoicereturnid"]);
                        if (invoicelist.ContainsKey(invoice.invoiceid))
                        {
                            invoicelist[invoice.invoiceid].taxsummary.Add(invoice);
                        }
                    }
                }
            }

            return Transform(config, criteria.autosynctax, invoicelist.Values.ToList());
        }
        private TallyVoucher CreatePurchaseReturnVouchers(TallyConfig.PurchaseReturnCreteria criteria, TallyConfig.Purchase config)
        {
            Data.ReferencelistDataService locationdata = new Data.ReferencelistDataService();
            var locations = locationdata.Search(new ReferencelistCreteria()
            {
                type = "LOCATION"
            });

            string query = @"select lrinvoicereturn.id,lrinvoicereturn.returnno,lrinvoicereturn.returndate,discount,tax,amount,packingcharge,freightamt,freighttaxamt,total,
                suppliercompany.name as suppliercompany,supplier.name suppliername,suppliercompanyid,location.code locationcode,agent.name as agentname,
                state.name as statename,city.name as cityname,location.name locationname,suppliercompany.address,
                lrinvoicereturn.isinterstatesale isinterstatesale,suppliergroup.name suppliergroup,
                lrinvoicereturn.returntype,country.name countryname,suppliercompany.pincode,lrinvoicereturn.approvedon,
                companystate.name companystatename,suppliercompany.tinanddate,suppliercompany.cstanddate from lrinvoicereturn
                inner join suppliercompany on suppliercompany.id = lrinvoicereturn.suppliercompanyid
                inner join supplier on supplier.id = suppliercompany.supplierid
                inner join company on company.id = lrinvoicereturn.companyid
                left outer join agent on agent.id = lrinvoicereturn.agentid
                left outer join referencelist state on state.id = suppliercompany.stateid
                left outer join referencelist city on city.id = suppliercompany.cityid
                left outer join referencelist location on location.id = lrinvoicereturn.locationid
                left outer join referencelist country on country.id = suppliercompany.countryid
                left outer join referencelist companystate on companystate.id = company.stateid
                left outer join referencelist suppliergroup on suppliergroup.id = supplier.suppliergroupid
                where lrinvoicereturn.companyid=@companyid and lrinvoicereturn.isactive = true and returndate>=@fromdate and returndate<@todate {0};

                select lrinvoicereturnid,lrinvoicereturnitems.taxpercentage,
                lrinvoicereturn.isinterstatesale isinterstatesale,
                sum(lrinvoicereturnitems.amount-lrinvoicereturnitems.discount) amount,sum(lrinvoicereturnitems.tax) tax from lrinvoicereturnitems 
                inner join lrinvoicereturn on lrinvoicereturn.id = lrinvoicereturnitems.lrinvoicereturnid
                inner join suppliercompany on suppliercompany.id = lrinvoicereturn.suppliercompanyid
                left outer join referencelist state on state.id = suppliercompany.stateid
                left outer join referencelist city on city.id = suppliercompany.cityid
                where lrinvoicereturn.companyid=@companyid and lrinvoicereturn.isactive = true and lrinvoicereturnitems.isactive=true and returndate>=@fromdate and returndate<@todate {0}
                group by lrinvoicereturnid,lrinvoicereturn.isinterstatesale,lrinvoicereturnitems.taxpercentage;";

            SortedList<string, object> param = new SortedList<string, object>();
            param.Add("fromdate", criteria.fromdate);
            param.Add("todate", criteria.todate.Date.AddDays(1));
            param.Add("companyid", criteria.companyid);

            string replacement = "";
            if (Services.ConfigManager.Instance(Context.OrganizationID).Config.intray_pur_prt)
                replacement += " and lrinvoicereturn.hasapprove=true";
            if (criteria.locationid > 0)
            {
                replacement += " and lrinvoicereturn.locationid = @locationid";
                param.Add("locationid", criteria.locationid);
            }
            if (criteria.supplierid > 0)
            {
                param.Add("supplierid", criteria.supplierid);
                replacement += " and suppliercompany.supplierid=@supplierid";
            }

            query = string.Format(query, replacement);

            SortedDictionary<long, ReturnInfo> invoicelist = new SortedDictionary<long, ReturnInfo>();

            using (System.Data.IDataReader reader = ExecuteReader(query, param))
            {
                while (reader.Read())
                {
                    ReturnInfo invoice = new ReturnInfo();
                    invoice.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                    invoice.returntype = reader["returntype"] == DBNull.Value ? 0 : Convert.ToInt32(reader["returntype"]);
                    invoice.suppliercompanyid = reader["suppliercompanyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["suppliercompanyid"]);
                    invoice.isinterstatesale = reader["isinterstatesale"] == DBNull.Value ? false : Convert.ToBoolean(reader["isinterstatesale"]);
                    invoice.returnno = reader["returnno"] == DBNull.Value ? "" : reader["returnno"].ToString();
                    invoice.returndate = reader["returndate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["returndate"]);
                    invoice.totalamount = reader["total"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["total"]);
                    invoice.baseamount = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"]);

                    invoice.packingcharge = reader["packingcharge"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["packingcharge"]);
                    invoice.freightamt = reader["freightamt"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["freightamt"]);
                    invoice.freighttaxamt = reader["freighttaxamt"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["freighttaxamt"]);

                    invoice.roundamount = invoice.totalamount - invoice.baseamount;
                    invoice.suppliercompany = reader["suppliercompany"] == DBNull.Value ? "" : reader["suppliercompany"].ToString();
                    invoice.suppliername = reader["suppliername"] == DBNull.Value ? "" : reader["suppliername"].ToString();
                    invoice.suppliergroup = reader["suppliergroup"] == DBNull.Value ? "" : reader["suppliergroup"].ToString();
                    invoice.address = reader["address"] == DBNull.Value ? "" : reader["address"].ToString();
                    invoice.statename = reader["statename"] == DBNull.Value ? "" : reader["statename"].ToString();
                    invoice.cityname = reader["cityname"] == DBNull.Value ? "" : reader["cityname"].ToString();
                    invoice.agentname = reader["agentname"] == DBNull.Value ? "" : reader["agentname"].ToString();
                    invoice.branchcode = reader["locationcode"] == DBNull.Value ? "" : reader["locationcode"].ToString();
                    invoice.branchname = reader["locationname"] == DBNull.Value ? "" : reader["locationname"].ToString();
                    invoice.countryname = reader["countryname"] == DBNull.Value ? "" : reader["countryname"].ToString();
                    invoice.supplierpincode = reader["pincode"] == DBNull.Value ? "" : reader["pincode"].ToString();
                    invoice.companystate = reader["companystatename"] == DBNull.Value ? "" : reader["companystatename"].ToString();
                    invoice.effectivedate = reader["approvedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["approvedon"]);
                    if (Services.ConfigManager.Instance(Context.OrganizationID).Config.intray_pur_prt)
                    {
                        invoice.effectivedate = invoice.returndate;
                    }
                    if (reader["tinanddate"] != DBNull.Value)
                        invoice.gstno = reader["tinanddate"].ToString();
                    else if (reader["cstanddate"] != DBNull.Value)
                        invoice.gstno = reader["cstanddate"].ToString();
                    invoicelist.Add(invoice.id, invoice);
                }
                if (reader.NextResult())
                {
                    while (reader.Read())
                    {
                        //sum(amounton - lrinvoicetax.discount) taxable,sum(lrinvoicetax.taxamount) tax,type,
                        //  lrinvoicetax.percentage,lrinvoicetax.isinterstatesale,invoiceid

                        TaxSummary invoice = new TaxSummary();
                        invoice.taxable = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"]);
                        invoice.tax = reader["tax"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["tax"]);
                        invoice.percentage = reader["taxpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["taxpercentage"]);
                        invoice.isinterstatesale = reader["isinterstatesale"] == DBNull.Value ? false : Convert.ToBoolean(reader["isinterstatesale"]);
                        invoice.invoiceid = reader["lrinvoicereturnid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["lrinvoicereturnid"]);
                        if (invoicelist.ContainsKey(invoice.invoiceid))
                        {
                            invoicelist[invoice.invoiceid].taxsummary.Add(invoice);
                        }
                    }
                }
            }

            if (invoicelist.Count > 0)
            {
                query = string.Format(@"with test as (	
        select lrinvoicereturnitems.lrinvoicereturnid, lrinvoice.invoiceno,lrinvoice.invoicedate,row_number() OVER (PARTITION BY lrinvoicereturnitems.lrinvoicereturnid ORDER BY lrinvoicereturnitems.id DESC) as ranknumber from lrinvoicereturnitems
        inner join lrinvoiceitems on lrinvoiceitems.id = lrinvoiceitemsid and lrinvoiceitems.isactive=true
        inner join lrinvoice on lrinvoice.id = lrinvoiceitems.lrinvoiceid  and lrinvoice.isactive = true
        where lrinvoicereturnitems.lrinvoicereturnid in ({0}))

        select lrinvoicereturnid,invoicedate,invoiceno from test where ranknumber=1", string.Join<long>(",", invoicelist.Values.ToList().ConvertAll(e => e.id)));
                using (System.Data.IDataReader reader = ExecuteReader(query, param))
                {
                    while (reader.Read())
                    {
                        long id = reader["lrinvoicereturnid"] == DBNull.Value ? 0 : (long)reader["lrinvoicereturnid"];
                        if (invoicelist.ContainsKey(id))
                        {
                            invoicelist[id].invoicedate = reader["invoicedate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["invoicedate"]);
                            invoicelist[id].invoiceno = reader["invoiceno"] == DBNull.Value ? "" : reader["invoiceno"].ToString();
                        }
                    }
                }
            }
            return Transform(config, criteria.autosynctax, invoicelist.Values.ToList());
        }

        private TallyVoucher Transform(TallyConfig.Purchase config, bool autosynctax, List<ReturnInfo> list)
        {
            List<InfyPOS.Tally.TallyXML.Voucher> vouchers = new List<InfyPOS.Tally.TallyXML.Voucher>();
            foreach (var item in list)
            {
                vouchers.Add(config.CreateVoucher(item, autosynctax));
            }
            var voucher = CreateVoucher(vouchers);
            voucher.company = string.IsNullOrEmpty(config.config.companyname) ? "TESTING" : config.config.companyname;
            voucher.port = config.config.port;
            voucher.machine = config.config.machine;
            if (config.config.configuration.Data_Purchase_Master)
            {
                voucher.ledgers = new List<TallyXML.Ledger>();
                voucher.groups = new List<TallyXML.LedgerGroup>();
            }
            InfyPOS.Tally.TallyXML tallyInterface = new InfyPOS.Tally.TallyXML("http://" + config.config.machine + ":" + config.config.port);
            tallyInterface.Context = this.Context;
            return tallyInterface.CreateXML(voucher, "New Ref");
        }

        public TallyVoucher CreateSupplierPayment(TallyConfig.SupplierPaymentCreteria criteria, TallyConfig.Purchase config)
        {
            Data.ReferencelistDataService locationdata = new Data.ReferencelistDataService();
            var locations = locationdata.Search(new ReferencelistCreteria()
            {
                type = "LOCATION"
            });

            string query = @"
                select supplierpayment.id,paymentdate,receiptno,suppliercompany.name,islrinvoicereturn,
                supplierpayment.suppliercompanyid,tinanddate,cstanddate,supplierpayment.rounding,
                suppliercompany.name as suppliercompany,supplier.name suppliername,bank.name as bankname,paymentmode,suppliercompany.address,
                state.name as statename,city.name as cityname,discountamount,hastds,tdsamount,tdspercentage,supplierpayment.locationid,
                supplierpayment.isinterstatesale isinterstatesale,suppliergroup.name suppliergroup,
                Supplierpaymentinvoice.invoiceno,Supplierpaymentinvoice.paidamount,paymentmode,referencedetail,instrumentdate							
                from supplierpayment
                inner join Supplierpaymentinvoice on Supplierpaymentinvoice.supplierpaymentid = supplierpayment.id
                inner join suppliercompany on suppliercompany.id = supplierpayment.suppliercompanyid
                inner join supplier on supplier.id = suppliercompany.supplierid
                left outer join referencelist state on state.id = suppliercompany.stateid
                left outer join referencelist city on city.id = suppliercompany.cityid
                left outer join referencelist bank on bank.id = supplierpayment.bankid
                left outer join referencelist suppliergroup on suppliergroup.id = supplier.suppliergroupid
                where supplierpayment.isactive=true  and supplierpayment.hascancel=false 
                and Supplierpaymentinvoice.isactive = true and supplierpaymentinvoice.hascancel=false
                and supplierpayment.paymentdate>=@fromdate and supplierpayment.paymentdate<@todate and supplierpayment.companyid=@companyid {0}";

            SortedList<string, object> param = new SortedList<string, object>();
            param.Add("fromdate", criteria.invoicedatefrom);
            param.Add("todate", criteria.invoicedateto.Date.AddDays(1));
            param.Add("companyid", criteria.companyid);

            string replacement = "";
            if (criteria.locationid > 0)
            {
                replacement += " and supplierpayment.locationid = @locationid";
                param.Add("locationid", criteria.locationid);
            }
            if (criteria.supplierid > 0)
            {
                param.Add("supplierid", criteria.supplierid);
                replacement += " and suppliercompany.supplierid=@supplierid";
            }

            query = string.Format(query, replacement);


            SortedDictionary<long, PaymentInfo> invoicelist = new SortedDictionary<long, PaymentInfo>();

            using (System.Data.IDataReader reader = ExecuteReader(query, param))
            {
                while (reader.Read())
                {
                    PaymentInfo invoice = new PaymentInfo();
                    invoice.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                    invoice.paymentdate = reader["paymentdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["paymentdate"]);
                    invoice.suppliercompany = reader["suppliercompany"] == DBNull.Value ? "" : reader["suppliercompany"].ToString();
                    invoice.suppliername = reader["suppliername"] == DBNull.Value ? "" : reader["suppliername"].ToString();
                    invoice.suppliergroup = reader["suppliergroup"] == DBNull.Value ? "" : reader["suppliergroup"].ToString();
                    invoice.address = reader["address"] == DBNull.Value ? "" : reader["address"].ToString();
                    invoice.statename = reader["statename"] == DBNull.Value ? "" : reader["statename"].ToString();
                    invoice.cityname = reader["cityname"] == DBNull.Value ? "" : reader["cityname"].ToString();
                    invoice.bankname = reader["bankname"] == DBNull.Value ? "" : reader["bankname"].ToString();
                    invoice.chequeno = reader["referencedetail"] == DBNull.Value ? "" : reader["referencedetail"].ToString();
                    invoice.instrumentdate = reader["instrumentdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["instrumentdate"]);

                    invoice.locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                    if (invoice.locationid > 0 && locations.Exists(e => e.id == invoice.locationid))
                    {
                        invoice.locationcode = locations.Find(e => e.id == invoice.locationid).code;
                        invoice.locationname = locations.Find(e => e.id == invoice.locationid).name;
                    }

                    if (invoice.instrumentdate == DateTime.MinValue && !string.IsNullOrEmpty(invoice.chequeno))
                        invoice.instrumentdate = invoice.paymentdate;

                    if (reader["tinanddate"] != DBNull.Value)
                        invoice.gstno = reader["tinanddate"].ToString();
                    else if (reader["cstanddate"] != DBNull.Value)
                        invoice.gstno = reader["cstanddate"].ToString();
                    invoice.suppliercompanyid = reader["suppliercompanyid"] == DBNull.Value ? 0 : (long)reader["suppliercompanyid"];
                    invoice.isinterstatesale = reader["isinterstatesale"] == DBNull.Value ? false : (bool)reader["isinterstatesale"];

                    int paymentmode = reader["paymentmode"] == DBNull.Value ? 0 : (Int32)reader["paymentmode"];
                    switch (paymentmode)
                    {
                        case 0:
                            invoice.paymentmode = "NEFT/RTGS";
                            break;
                        case 1:
                            invoice.paymentmode = "Cheque/DD";
                            break;
                        case 2:
                            invoice.paymentmode = "Cash";
                            invoice.iscash = true;
                            break;
                    }
                    invoice.allocations = new List<PaymentInfo.Allocation>();
                    if (invoicelist.ContainsKey(invoice.id))
                        invoice = invoicelist[invoice.id];
                    else
                        invoicelist.Add(invoice.id, invoice);
                    if (reader["hastds"] != DBNull.Value && Convert.ToBoolean(reader["hastds"]))
                    {
                        invoice.tdspercentage = reader["tdspercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["tdspercentage"]);
                        invoice.tdsamount = reader["tdsamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["tdsamount"]);
                    }
                    invoice.rounding = reader["rounding"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["rounding"]);

                    invoice.allocations.Add(new PaymentInfo.Allocation()
                    {
                        isdebitnote = reader["islrinvoicereturn"] == DBNull.Value ? false : Convert.ToBoolean(reader["islrinvoicereturn"]),
                        invoiceno = reader["invoiceno"] == DBNull.Value ? "" : reader["invoiceno"].ToString(),
                        paidamount = reader["paidamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["paidamount"]),
                        discountamount = reader["discountamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discountamount"])
                    }); ;
                }
            }
            return Transform(config, invoicelist.Values.ToList());
        }


        public TallyVoucher Transform(TallyConfig.Purchase config, List<PaymentInfo> invoiceList)
        {
            List<InfyPOS.Tally.TallyXML.Voucher> vouchers = new List<InfyPOS.Tally.TallyXML.Voucher>();
            foreach (var item in invoiceList)
            {
                vouchers.Add(config.CreateVoucher(item));
            }
            var voucher = CreateVoucher(vouchers);
            voucher.company = string.IsNullOrEmpty(config.config.companyname) ? "TESTING" : config.config.companyname;
            voucher.port = config.config.port;
            voucher.machine = config.config.machine;
            if (config.config.configuration.Data_Purchase_Master)
            {
                voucher.ledgers = new List<TallyXML.Ledger>();
                voucher.groups = new List<TallyXML.LedgerGroup>();
            }
            InfyPOS.Tally.TallyXML tallyInterface = new InfyPOS.Tally.TallyXML("http://" + config.config.machine + ":" + config.config.port);
            tallyInterface.Context = this.Context;
            //tallyInterface.Transer(tallyInterface.CreateXML(voucher));
            return tallyInterface.CreateXML(voucher, "Agst Ref");
        }

        public TallyVoucher CreatePurchaseVouchers(TallyConfig.PurchaseCreteria criteria, TallyConfig.Purchase config)
        {
            Data.ReferencelistDataService locationdata = new Data.ReferencelistDataService();
            var locations = locationdata.Search(new ReferencelistCreteria()
            {
                type = "LOCATION"
            });

            string query = @"select lrinvoice.id,lrinvoice.invoiceno,lrinvoice.invoicedate,lrinvoice.totalamount,lrinvoice.otheramount roundamount,agent.name as agentname,
                suppliercompany.name as suppliercompany,supplier.name suppliername,suppliercompany.tinanddate,suppliercompany.cstanddate,lrentry.entrylocationid,lrentry.agentid,lrentry.agentcommission,
                suppliercompany.address,state.name as statename,city.name as cityname,lrinvoice.isinterstatesale isinterstatesale,
                lrentry.lrentryno,suppliercompanyid,lrinvoice.entrydate entrydate,country.name countryname,suppliercompany.pincode,
                companystate.name companystatename,lrinvoice.approvedon,lrentry.modeofdelivery,suppliergroup.name suppliergroup,
                lrentry.NOOFPIECES from lrinvoice
                inner join lrentry on lrentry.id = lrid
                inner join company on company.id = lrinvoice.companyid
                inner join suppliercompany on suppliercompany.id = lrinvoice.suppliercompanyid
                inner join supplier on supplier.id = suppliercompany.supplierid
                left outer join agent on agent.id = lrentry.agentid
                left outer join referencelist state on state.id = suppliercompany.stateid
                left outer join referencelist companystate on companystate.id = company.stateid
                left outer join referencelist country on country.id = suppliercompany.countryid
                left outer join referencelist city on city.id = suppliercompany.cityid
                left outer join referencelist suppliergroup on suppliergroup.id = supplier.suppliergroupid
                where lrinvoice.companyid=@companyid and lrinvoice.isactive=true and lrinvoice.entrydate>=@fromdate and lrinvoice.entrydate<@todate {0};

                select sum(amounton-lrinvoicetax.discount) taxable,sum(lrinvoicetax.taxamount) tax,type,
                lrinvoicetax.percentage,lrinvoice.isinterstatesale isinterstatesale,
                invoiceid,sum(lrinvoicetax.discount) discount from lrinvoicetax
                inner join lrinvoice on lrinvoice.id = invoiceid  and lrinvoice.isactive = true
                inner join lrentry on lrentry.id = lrinvoice.lrid and lrentry.isactive = true
                inner join suppliercompany on suppliercompany.id = lrinvoice.suppliercompanyid
                where lrinvoice.companyid=@companyid and lrinvoice.isactive=true and lrinvoicetax.isactive = true and lrinvoice.entrydate>=@fromdate and lrinvoice.entrydate<@todate {0}
                group by type,lrinvoicetax.percentage,lrinvoice.isinterstatesale,invoiceid;";


            SortedList<string, object> param = new SortedList<string, object>();
            param.Add("fromdate", criteria.entrydatefrom);
            param.Add("todate", criteria.entrydateto.Date.AddDays(1));
            param.Add("companyid", criteria.companyid);
            param.Add("supplierid", criteria.supplierid);
            param.Add("entrylocationid", criteria.entrylocationid);
            param.Add("entryno", string.IsNullOrEmpty(criteria.entryno) ? "" : criteria.entryno);
            string additionalcondition = "";
            if (criteria.supplierid > 0 && criteria.entrylocationid > 0)
            {
                additionalcondition = " and suppliercompany.supplierid=@supplierid and lrentry.entrylocationid=@entrylocationid";
            }
            else if (criteria.supplierid > 0)
            {
                additionalcondition = " and suppliercompany.supplierid=@supplierid";
            }
            else if (criteria.entrylocationid > 0)
            {
                additionalcondition = " and lrentry.entrylocationid=@entrylocationid";
            }
            if (!string.IsNullOrEmpty(criteria.entryno))
                additionalcondition += " and lrentry.lrentryno=@entryno";

            if (Services.ConfigManager.Instance(Context.OrganizationID).Config.intray_pur_prt)
                additionalcondition += " and lrinvoice.hasapprove=true";

            query = string.Format(query, additionalcondition);

            SortedDictionary<long, InvoiceInfo> invoicelist = new SortedDictionary<long, InvoiceInfo>();

            using (System.Data.IDataReader reader = ExecuteReader(query, param))
            {
                while (reader.Read())
                {
                    InvoiceInfo invoice = new InvoiceInfo();
                    invoice.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                    invoice.entrylocationid = reader["entrylocationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["entrylocationid"]);
                    if (invoice.entrylocationid > 0 && locations.Exists(e => e.id == invoice.entrylocationid))
                    {
                        invoice.locationcode = locations.Find(e => e.id == invoice.entrylocationid).code;
                        invoice.locationname = locations.Find(e => e.id == invoice.entrylocationid).name;
                    }
                    invoice.suppliercompanyid = reader["suppliercompanyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["suppliercompanyid"]);
                    invoice.isinterstatesale = reader["isinterstatesale"] == DBNull.Value ? false : Convert.ToBoolean(reader["isinterstatesale"]);
                    invoice.isexpense = reader["modeofdelivery"] == DBNull.Value ? false : Convert.ToInt32(reader["modeofdelivery"])==9;
                    invoice.suppliergroup = reader["suppliergroup"] == DBNull.Value ? "" : reader["suppliergroup"].ToString();

                    invoice.invoiceno = reader["invoiceno"] == DBNull.Value ? "" : reader["invoiceno"].ToString();
                    invoice.invoicedate = reader["invoicedate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["invoicedate"]);
                    invoice.entrydate = reader["entrydate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["entrydate"]);
                    invoice.totalamount = reader["totalamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["totalamount"]);
                    invoice.roundamount = reader["roundamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["roundamount"]);
                    invoice.qty = reader["NOOFPIECES"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["NOOFPIECES"]);

                    invoice.suppliercompany = reader["suppliercompany"] == DBNull.Value ? "" : reader["suppliercompany"].ToString();
                    invoice.suppliername = reader["suppliername"] == DBNull.Value ? "" : reader["suppliername"].ToString();

                    invoice.address = reader["address"] == DBNull.Value ? "" : reader["address"].ToString();
                    invoice.statename = reader["statename"] == DBNull.Value ? "" : reader["statename"].ToString();
                    invoice.cityname = reader["cityname"] == DBNull.Value ? "" : reader["cityname"].ToString();

                    invoice.countryname = reader["countryname"] == DBNull.Value ? "" : reader["countryname"].ToString();
                    invoice.supplierpincode = reader["pincode"] == DBNull.Value ? "" : reader["pincode"].ToString();
                    invoice.companystate = reader["companystatename"] == DBNull.Value ? "" : reader["companystatename"].ToString();

                    if (reader["tinanddate"] != DBNull.Value)
                        invoice.gstno = reader["tinanddate"].ToString();
                    else if (reader["cstanddate"] != DBNull.Value)
                        invoice.gstno = reader["cstanddate"].ToString();
                    invoice.entryno = reader["lrentryno"] == DBNull.Value ? "" : reader["lrentryno"].ToString();
                    invoice.effectivedate = reader["approvedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["approvedon"]);
                    if (Services.ConfigManager.Instance(Context.OrganizationID).Config.intray_pur_prt)
                    {
                        invoice.effectivedate = invoice.invoicedate;
                    }
                    invoice.agentname = reader["agentname"] == DBNull.Value ? "" : reader["agentname"].ToString();
                    invoice.agentcommission = reader["agentcommission"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["agentcommission"]);
                    invoicelist.Add(invoice.id, invoice);
                }
                if (reader.NextResult())
                {
                    while (reader.Read())
                    {
                        //sum(amounton - lrinvoicetax.discount) taxable,sum(lrinvoicetax.taxamount) tax,type,
                        //  lrinvoicetax.percentage,lrinvoicetax.isinterstatesale,invoiceid
                        TaxSummary invoice = new TaxSummary();
                        invoice.taxable = reader["taxable"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["taxable"]);
                        invoice.tax = reader["tax"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["tax"]);
                        invoice.discount = reader["discount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discount"]);
                        invoice.type = reader["type"] == DBNull.Value ? 0 : Convert.ToInt32(reader["type"]);
                        invoice.percentage = reader["percentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["percentage"]);
                        invoice.isinterstatesale = reader["isinterstatesale"] == DBNull.Value ? false : Convert.ToBoolean(reader["isinterstatesale"]);
                        invoice.invoiceid = reader["invoiceid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["invoiceid"]);
                        if (invoicelist.ContainsKey(invoice.invoiceid))
                        {
                            invoicelist[invoice.invoiceid].taxsummary.Add(invoice);
                        }
                    }
                }
            }

            if (invoicelist.ToList().Exists(x => x.Value.totalamount == 0))
            {
                var amtmissing = invoicelist.ToList().FindAll(x => x.Value.totalamount == 0);
                amtmissing.ForEach(x =>
                {
                    x.Value.totalamount = x.Value.taxsummary.Sum(y => y.taxable + y.tax);
                });
            }

            return Transform(config, criteria.autosynctax, criteria.agentcommission, invoicelist.Values.ToList());
        }


        public TallyVoucher CreateBOMPurchaseVouchers(TallyConfig.PurchaseCreteria criteria, TallyConfig.Purchase config)
        {
            Data.ReferencelistDataService locationdata = new Data.ReferencelistDataService();
            var locations = locationdata.Search(new ReferencelistCreteria()
            {
                type = "LOCATION"
            });

            string query = @"select BOMinvoice.id,BOMinvoice.invoiceno,BOMinvoice.invoicedate,BOMinvoice.totalamount,BOMinvoice.otheramount roundamount,
                suppliercompany.name as suppliercompany,supplier.name suppliername,tinanddate,cstanddate,BOMinvoice.invoiceentryno,BOMinvoice.locationid,
                state.name as statename,city.name as cityname,BOMinvoice.isinterstatesale isinterstatesale,suppliergroup.name suppliergroup,
                suppliercompanyid,BOMinvoice.entrydate entrydate,suppliercompany.address from BOMinvoice
                inner join suppliercompany on suppliercompany.id = BOMinvoice.suppliercompanyid
                inner join supplier on supplier.id = suppliercompany.supplierid
                left outer join referencelist state on state.id = suppliercompany.stateid
                left outer join referencelist city on city.id = suppliercompany.cityid
                left outer join referencelist suppliergroup on suppliergroup.id = supplier.suppliergroupid
                where BOMinvoice.companyid=@companyid and BOMinvoice.isactive=true and BOMinvoice.entrydate>=@fromdate and BOMinvoice.entrydate<@todate {0};

                select sum(amounton-BOMinvoicetax.discount) taxable,sum(BOMinvoicetax.taxamount) tax,type,
                BOMinvoicetax.percentage,bominvoice.isinterstatesale isinterstatesale,
                invoiceid,sum(BOMinvoicetax.discount) discount from BOMinvoicetax
                inner join bominvoice on bominvoice.id = invoiceid  and bominvoice.isactive = true
                inner join suppliercompany on suppliercompany.id = bominvoice.suppliercompanyid
                where bominvoice.companyid=@companyid and bominvoice.isactive=true and BOMinvoicetax.isactive = true and bominvoice.entrydate>=@fromdate and bominvoice.entrydate<@todate {0}
                group by type,BOMinvoicetax.percentage,bominvoice.isinterstatesale,invoiceid;";


            SortedList<string, object> param = new SortedList<string, object>();
            param.Add("fromdate", criteria.entrydatefrom);
            param.Add("todate", criteria.entrydateto.Date.AddDays(1));
            param.Add("companyid", criteria.companyid);
            param.Add("supplierid", criteria.supplierid);
            param.Add("locationid", criteria.entrylocationid);
            param.Add("entryno", string.IsNullOrEmpty(criteria.entryno) ? "" : criteria.entryno);
            string additionalcondition = "";
            if (criteria.supplierid > 0 && criteria.entrylocationid > 0)
            {
                additionalcondition = " and suppliercompany.supplierid=@supplierid and bominvoice.locationid=@locationid";
            }
            else if (criteria.supplierid > 0)
            {
                additionalcondition = " and suppliercompany.supplierid=@supplierid";
            }
            else if (criteria.entrylocationid > 0)
            {
                additionalcondition = " and bominvoice.locationid=@locationid";
            }
            if (!string.IsNullOrEmpty(criteria.entryno))
                additionalcondition += " and BOMinvoice.invoiceentryno=@entryno";

            query = string.Format(query, additionalcondition);

            SortedDictionary<long, InvoiceInfo> invoicelist = new SortedDictionary<long, InvoiceInfo>();

            using (System.Data.IDataReader reader = ExecuteReader(query, param))
            {
                while (reader.Read())
                {
                    InvoiceInfo invoice = new InvoiceInfo();
                    invoice.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                    invoice.entrylocationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                    if (invoice.entrylocationid > 0 && locations.Exists(e => e.id == invoice.entrylocationid))
                    {
                        invoice.locationcode = locations.Find(e => e.id == invoice.entrylocationid).code;
                        invoice.locationname = locations.Find(e => e.id == invoice.entrylocationid).name;
                    }
                    invoice.suppliercompanyid = reader["suppliercompanyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["suppliercompanyid"]);
                    invoice.isinterstatesale = reader["isinterstatesale"] == DBNull.Value ? false : Convert.ToBoolean(reader["isinterstatesale"]);
                    invoice.invoiceno = reader["invoiceno"] == DBNull.Value ? "" : reader["invoiceno"].ToString();
                    invoice.invoicedate = reader["invoicedate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["invoicedate"]);
                    invoice.entrydate = reader["entrydate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["entrydate"]);
                    invoice.totalamount = reader["totalamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["totalamount"]);
                    invoice.roundamount = reader["roundamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["roundamount"]);
                    invoice.suppliercompany = reader["suppliercompany"] == DBNull.Value ? "" : reader["suppliercompany"].ToString();
                    invoice.suppliername = reader["suppliername"] == DBNull.Value ? "" : reader["suppliername"].ToString();
                    invoice.suppliergroup = reader["suppliergroup"] == DBNull.Value ? "" : reader["suppliergroup"].ToString();
                    invoice.address = reader["address"] == DBNull.Value ? "" : reader["address"].ToString();
                    invoice.statename = reader["statename"] == DBNull.Value ? "" : reader["statename"].ToString();
                    invoice.cityname = reader["cityname"] == DBNull.Value ? "" : reader["cityname"].ToString();
                    if (reader["tinanddate"] != DBNull.Value)
                        invoice.gstno = reader["tinanddate"].ToString();
                    else if (reader["cstanddate"] != DBNull.Value)
                        invoice.gstno = reader["cstanddate"].ToString();
                    invoice.entryno = reader["invoiceentryno"] == DBNull.Value ? "" : reader["invoiceentryno"].ToString();
                    invoicelist.Add(invoice.id, invoice);
                }
                if (reader.NextResult())
                {
                    while (reader.Read())
                    {
                        //sum(amounton - lrinvoicetax.discount) taxable,sum(lrinvoicetax.taxamount) tax,type,
                        //  lrinvoicetax.percentage,lrinvoicetax.isinterstatesale,invoiceid

                        TaxSummary invoice = new TaxSummary();
                        invoice.taxable = reader["taxable"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["taxable"]);
                        invoice.tax = reader["tax"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["tax"]);
                        invoice.discount = reader["discount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discount"]);
                        invoice.type = reader["type"] == DBNull.Value ? 0 : Convert.ToInt32(reader["type"]);
                        invoice.percentage = reader["percentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["percentage"]);
                        invoice.isinterstatesale = reader["isinterstatesale"] == DBNull.Value ? false : Convert.ToBoolean(reader["isinterstatesale"]);
                        invoice.invoiceid = reader["invoiceid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["invoiceid"]);
                        if (invoicelist.ContainsKey(invoice.invoiceid))
                        {
                            invoicelist[invoice.invoiceid].taxsummary.Add(invoice);
                        }
                    }
                }
            }

            return Transform(config, criteria.autosynctax, criteria.agentcommission, invoicelist.Values.ToList());
        }



        public TallyVoucher Transform(TallyConfig.Purchase config, bool autosynctax, bool agentcommission, List<InvoiceInfo> invoiceList)
        {
            CompanyDataService companyDataService = new CompanyDataService() { Context = this.Context };
            var configuration = companyDataService.InternalVendorConfig();
            var transfercompanyid = configuration.ConvertAll(x => x.suppliercompanyid);
            var transfers = invoiceList.FindAll(x => x.suppliercompanyid > 0 && transfercompanyid.Contains(x.suppliercompanyid));
            transfers.ForEach(x => x.stocktransfer = true);

            List<InfyPOS.Tally.TallyXML.Voucher> vouchers = new List<InfyPOS.Tally.TallyXML.Voucher>();
            foreach (var item in invoiceList)
            {
                vouchers.Add(config.CreateVoucher(item, autosynctax, agentcommission));
            }
            var voucher = CreateVoucher(vouchers);
            voucher.company = string.IsNullOrEmpty(config.config.companyname) ? "TESTING" : config.config.companyname;
            voucher.port = config.config.port;
            voucher.machine = config.config.machine;

            if (config.config.configuration.Data_Purchase_Master)
            {
                voucher.ledgers = new List<TallyXML.Ledger>();
                voucher.groups = new List<TallyXML.LedgerGroup>();
            }

            InfyPOS.Tally.TallyXML tallyInterface = new InfyPOS.Tally.TallyXML("http://" + config.config.machine + ":" + config.config.port);
            tallyInterface.Context = this.Context;
            //tallyInterface.Transer(tallyInterface.CreateXML(voucher));
            return tallyInterface.CreateXML(voucher, "New Ref");
        }


        private TallyVoucher CreateVoucher(List<InfyPOS.Tally.TallyXML.Voucher> vouchers)
        {
            List<InfyPOS.Tally.TallyXML.LedgerGroup> parentGroups = new List<InfyPOS.Tally.TallyXML.LedgerGroup>();
            List<InfyPOS.Tally.TallyXML.Ledger> ledgers = new List<InfyPOS.Tally.TallyXML.Ledger>();
            foreach (var voucher in vouchers)
            {
                foreach (var item in voucher.ledgers)
                {
                    List<InfyPOS.Tally.TallyXML.LedgerGroup> groups = new List<InfyPOS.Tally.TallyXML.LedgerGroup>();
                    List<InfyPOS.Tally.TallyXML.Ledger> ledger = new List<InfyPOS.Tally.TallyXML.Ledger>();
                    string[] ledgerParts = item.Name.Split(new char[] { '~' }, StringSplitOptions.RemoveEmptyEntries);
                    if(ledgerParts !=null && ledgerParts.Length>0)
                        item.Name = ledgerParts[0];
                    if (ledgerParts.Length > 2)
                    {
                        //Current Liabilities~Duties & Taxes~GST~Input CGST~CGST @[TAX-PERCENTAGE]

                        for (int j = ledgerParts.Length - 2; j > 0; j--)
                        {
                            groups.Add(new InfyPOS.Tally.TallyXML.LedgerGroup()
                            {
                                name = ledgerParts[j],
                                parent = ledgerParts[j - 1]
                            });
                        }

                        item.Name = ledgerParts[ledgerParts.Length - 1];
                        ledger.Add(new InfyPOS.Tally.TallyXML.Ledger()
                        {
                            name = ledgerParts[ledgerParts.Length - 1],
                            parent = groups.First().name,
                            isbillwise = item.Allocations.Count > 0,
                            statename = item.statename,
                            cityname = item.cityname,
                            additionalattribs = item.addionalAttribs,
                            creditdays = item.creditdays
                        });
                    }
                    else if (ledgerParts.Length == 2)
                    {
                        item.Name = ledgerParts[1];
                        ledger.Add(new InfyPOS.Tally.TallyXML.Ledger()
                        {
                            name = ledgerParts[1],
                            parent = ledgerParts[0],
                            isbillwise = item.Allocations.Count > 0,
                            statename = item.statename,
                            cityname = item.cityname,
                            additionalattribs = item.addionalAttribs,
                            creditdays = item.creditdays
                        });
                    }

                    groups.ForEach(e => voucher.require_groups.Add(e.name));
                    ledger.ForEach(e => voucher.require_ledgers.Add(e.name));

                    groups.RemoveAll(e => parentGroups.Exists(p => p.name == e.name));
                    groups.Reverse();
                    foreach (var grp in groups)
                        parentGroups.Add(grp);

                    ledger.RemoveAll(e => ledgers.Exists(l => l.name == e.name));
                    foreach (var led in ledger)
                        ledgers.Add(led);

                }
            }

            TallyVoucher tvoucher = new TallyVoucher();
            tvoucher.vouchers = vouchers;
            tvoucher.ledgers = ledgers;
            tvoucher.groups = parentGroups;
            return tvoucher;
        }
    }
}