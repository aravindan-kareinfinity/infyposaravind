using InfyPOS.Models;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using ClosedXML.Excel;
using InfyPOS.Analytical;
using InfyPOS.Data;
using System.Net.Configuration;
using InfyPOS.Services;

namespace InfyPOS.Tally
{
    public class FinanceService : InfyPOS.Common.ReportData
    {

        public class InternalValue : MobileInsight.FinanceSummary.InternalValue
        {
            public decimal costvalue { get; set; }
            public decimal salevalue { get; set; }
            public decimal taxvalue { get; set; }
            public long locationid { get; set; }
        }
        public class NameAndValue : MobileInsight.FinanceSummary.NameAndValue
        {
            public long locationid { get; set; }
            public long ledgerid { get; set; }
            public long tagid { get; set; }
            public decimal amount { get; set; }
        }
        public DailyPandL BuildDailyPandL(DateTime from, DateTime to, long locationid)
        {
            SortedDictionary<long, decimal> stockvalues = new SortedDictionary<long, decimal>();
            List<InternalValue> internalValues = new List<InternalValue>();
            List<NameAndValue> results = new List<NameAndValue>();
            List<Ledger> ledgers = new List<Ledger>();
            string query = @"with sales as
                            (
                                select sum(stock.ibuyingprice::numeric*billitems.qty) costvalue,
	                            sum(billitems.receivable) salevalue,
	                            sum(billitems.tax) taxvalue,billitems.productid,bill.locationid from bill
                                inner join billitems on billitems.billid = bill.id and billitems.isactive=true
                                inner join stock on stock.id = billitems.stockid
                                where bill.billdate>=@fromdate and bill.billdate<@todate
                                and billtype != 9 and bill.isactive=true
                                and bill.locationid=@locationid
                                group by billitems.productid,bill.locationid
                            )
                            select sum(sales.costvalue) costvalue,
                            sum(sales.salevalue) salevalue,
                            sum(sales.taxvalue) taxvalue,
                            locationid,
                            referencelist.name sectionname,barcodesection.name barcodesection from sales
                            inner join products on products.id = sales.productid
                            left outer join referencelist on referencelist.id = salescounterid
                            left outer join referencelist barcodesection on barcodesection.id = products.barcodeid
                            group by locationid,referencelist.name,barcodesection.name;

                            select 
                            salablegoods.locationid,
                            sum((salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty))*stock.ibuyingprice::numeric) as value
                            from salablegoods
                            inner join stock on stock.id = stockid
                            where salablegoods.hasstock = true and salablegoods.isactive = true
                            and salablegoods.locationid=@locationid
                            group by salablegoods.locationid;

                            select sum(voucheritems.amount) amount,ledger.id,ledger.tagid,voucher.locationid from voucher
                            inner join voucheritems on voucheritems.voucherid = voucher.id
                            inner join ledger on ledger.id = voucheritems.ledgerid and ledger.tagid>0
                            where voucher.voucherdate>=@fromdate and voucherdate<@todate
                            and voucher.locationid=@locationid
                            group by ledger.id,ledger.id,ledger.tagid,voucher.locationid;

                            select id,displayname,attributes,tagid from ledger where isactive=true and tagid>0";

            var param = new SortedList<string, object>();
            param.Add("fromdate", from.Date);
            param.Add("todate", to.Date.AddDays(1));
            param.Add("locationid", locationid);
            using (var reader = ExecuteReader(query, param, 1000))
            {
                while (reader.Read())
                {
                    InternalValue iv = new InternalValue();
                    iv.sectionname = reader["sectionname"] != DBNull.Value ? Convert.ToString(reader["sectionname"]) : "";
                    iv.bsectionname = reader["barcodesection"] != DBNull.Value ? Convert.ToString(reader["barcodesection"]) : "";
                    iv.costvalue = reader["costvalue"] != DBNull.Value ? Convert.ToDecimal(reader["costvalue"]) : 0;
                    iv.salevalue = reader["salevalue"] != DBNull.Value ? Convert.ToDecimal(reader["salevalue"]) : 0;
                    iv.taxvalue = reader["taxvalue"] != DBNull.Value ? Convert.ToDecimal(reader["taxvalue"]) : 0;
                    iv.locationid = reader["locationid"] != DBNull.Value ? Convert.ToInt64(reader["locationid"]) : 0;
                    internalValues.Add(iv);
                }
                if (reader.NextResult())
                {
                    while (reader.Read())
                    {
                        //sum(voucheritems.amount) amount,ledger.id,ledger.tagid,voucher.locationid
                        var value = reader["value"] != DBNull.Value ? Convert.ToDecimal(reader["value"]) : 0;
                        var locid = reader["locationid"] != DBNull.Value ? Convert.ToInt64(reader["locationid"]) : 0;
                        stockvalues.Add(locid, value);
                    }
                }
                if (reader.NextResult())
                {
                    while (reader.Read())
                    {
                        //sum(voucheritems.amount) amount,ledger.id,ledger.tagid,voucher.locationid
                        NameAndValue iv = new NameAndValue();
                        iv.amount = reader["amount"] != DBNull.Value ? Convert.ToDecimal(reader["amount"]) : 0;
                        iv.locationid = reader["locationid"] != DBNull.Value ? Convert.ToInt64(reader["locationid"]) : 0;
                        iv.tagid = reader["tagid"] != DBNull.Value ? Convert.ToInt64(reader["tagid"]) : 0;
                        iv.ledgerid = reader["id"] != DBNull.Value ? Convert.ToInt64(reader["id"]) : 0;
                        results.Add(iv);
                    }
                }
                if (reader.NextResult())
                {
                    while (reader.Read())
                    {
                        //id,displayname,attributes
                        Ledger oledger = new Ledger();
                        oledger.attributes_json = reader["attributes"] == DBNull.Value ? "null" : reader["attributes"].ToString();
                        oledger.displayname = reader["displayname"] != DBNull.Value ? Convert.ToString(reader["displayname"]) : "";
                        oledger.id = reader["id"] != DBNull.Value ? Convert.ToInt64(reader["id"]) : 0;
                        oledger.tagid = reader["tagid"] != DBNull.Value ? Convert.ToInt32(reader["tagid"]) : 0; ;
                        ledgers.Add(oledger);
                    }
                }
            }

            //{ 'id': 7, 'name': 'Inventory Interest' },
            //{ 'id': 20, 'name': 'P&L Expense' },
            //{ 'id': 21, 'name': 'Daily Expense' },
            //{ 'id': 22, 'name': 'Weekly Expense' },
            //{ 'id': 23, 'name': 'Monthly Expense' },
            //{ 'id': 24, 'name': 'Yearly Expense' },
            //{ 'id': 25, 'name': 'Asset Depreciation' },
            //{ 'id': 26, 'name': 'Salary Expense' },
            //{ 'id': 27, 'name': 'Sales Margin % Expense' },
            //{ 'id': 28, 'name': 'Purchase Margin % Expense' },

            DailyPandL dailyPandL = new DailyPandL();
            dailyPandL.costvalue = internalValues.Sum(x => x.costvalue);
            dailyPandL.salevalue = internalValues.Sum(x => x.salevalue);
            dailyPandL.taxvalue = internalValues.Sum(x => x.taxvalue);
            dailyPandL.grossmargin = dailyPandL.salevalue - (dailyPandL.taxvalue + dailyPandL.costvalue);
            dailyPandL.stockvalue = stockvalues.Values.Sum(x => x);
            if (ledgers.Exists(x => x.tagid == 7 &&
                x.attributes.tagvalues != null &&
                x.attributes.tagvalues.Count > 0))
            {
                var led = ledgers.Find(x => x.tagid == 7 &&
                x.attributes.tagvalues != null &&
                x.attributes.tagvalues.Count > 0);
                if (led.attributes.tagvalues.Exists(x => x.id == locationid && x.value > 0))
                {
                    dailyPandL.InventoryInterestPercentage = led.attributes.tagvalues.Find(x => x.id == locationid).value;
                }
                if (dailyPandL.stockvalue > 0 &&
                    dailyPandL.InventoryInterestPercentage > 0)
                {
                    dailyPandL.InventoryInterest = (dailyPandL.stockvalue * dailyPandL.InventoryInterestPercentage / 100) / 365;
                }
            }

            //{ 'id': 25, 'name': 'Asset Depreciation' },
            if (ledgers.Exists(x => x.tagid == 25 && x.attributes.tagvalues != null && x.attributes.tagvalues.Count > 0))
            {
                var assets = ledgers.FindAll(x => x.tagid == 25 && x.attributes.tagvalues != null && x.attributes.tagvalues.Count > 0);
                decimal assetvalue = 0;
                foreach (var asset in assets)
                {
                    if (asset.attributes.tagvalues.Exists(x => x.id == locationid && x.value > 0))
                    {
                        assetvalue += asset.attributes.tagvalues.Find(x => x.id == locationid).value;
                    }
                }
                dailyPandL.AssetValue = assetvalue;
                if (dailyPandL.AssetValue > 0)
                {
                    dailyPandL.AssetDepreciation = (dailyPandL.AssetValue) / 365;
                }
            }

            //{ 'id': 26, 'name': 'Salary Expense' },
            if (ledgers.Exists(x => x.tagid == 26 && x.attributes.tagvalues != null && x.attributes.tagvalues.Count > 0))
            {
                var assets = ledgers.FindAll(x => x.tagid == 26 && x.attributes.tagvalues != null && x.attributes.tagvalues.Count > 0);
                decimal salaryvalue = 0;
                foreach (var asset in assets)
                {
                    if (asset.attributes.tagvalues.Exists(x => x.id == locationid && x.value > 0))
                    {
                        salaryvalue += asset.attributes.tagvalues.Find(x => x.id == locationid).value;
                    }
                }
                dailyPandL.SalaryExpenseValue = salaryvalue;
                if (dailyPandL.SalaryExpenseValue > 0)
                {
                    dailyPandL.SalaryExpense = (dailyPandL.SalaryExpenseValue) / 365;
                }
            }


            ////{ 'id': 27, 'name': 'Sales Margin % Expense' },
            if (ledgers.Exists(x => x.tagid == 27 && x.attributes.tagvalues != null && x.attributes.tagvalues.Count > 0))
            {
                var led = ledgers.Find(x => x.tagid == 27 &&
                x.attributes.tagvalues != null &&
                x.attributes.tagvalues.Count > 0);
                if (led.attributes.tagvalues.Exists(x => x.id == locationid && x.value > 0))
                {
                    dailyPandL.SalesMarginExpensePercentage = led.attributes.tagvalues.Find(x => x.id == locationid).value;
                }
                if (dailyPandL.salevalue > 0 &&
                    dailyPandL.SalesMarginExpensePercentage > 0)
                {
                    dailyPandL.SalesMarginExpense = (dailyPandL.salevalue * dailyPandL.SalesMarginExpensePercentage / 100) / 365;
                }
            }

            //{ 'id': 28, 'name': 'Purchase Margin % Expense' },
            if (ledgers.Exists(x => x.tagid == 28 && x.attributes.tagvalues != null && x.attributes.tagvalues.Count > 0))
            {
                var led = ledgers.Find(x => x.tagid == 28 &&
                x.attributes.tagvalues != null &&
                x.attributes.tagvalues.Count > 0);
                if (led.attributes.tagvalues.Exists(x => x.id == locationid && x.value > 0))
                {
                    dailyPandL.PurchaseMarginxpensePercentage = led.attributes.tagvalues.Find(x => x.id == locationid).value;
                }
                if (dailyPandL.costvalue > 0 &&
                    dailyPandL.PurchaseMarginxpensePercentage > 0)
                {
                    dailyPandL.PurchaseMarginxpense = (dailyPandL.costvalue * dailyPandL.PurchaseMarginxpensePercentage / 100) / 365;
                }
            }

            //{ 'id': 20, 'name': 'P&L Expense' },
            dailyPandL.PanLExpense_List = new List<MobileInsight.FinanceSummary.NameAndValue>();
            results.ForEach(x =>
            {
                if (x.tagid == 20)
                {
                    dailyPandL.PanLExpense += x.amount;
                    if (ledgers.Exists(y => y.id == x.ledgerid))
                    {
                        dailyPandL.PanLExpense_List.Add(new MobileInsight.FinanceSummary.NameAndValue()
                        {
                            name = ledgers.Find(y => y.id == x.ledgerid).displayname,
                            value = x.amount
                        });
                    }
                }
            });

            //{ 'id': 21, 'name': 'Daily Expense' },
            if (ledgers.Exists(x => x.tagid == 21 && x.attributes.tagvalues != null && x.attributes.tagvalues.Count > 0))
            {
                dailyPandL.DailyExpense_List = new List<MobileInsight.FinanceSummary.NameAndValue>();
                foreach (var led in ledgers.FindAll(x => x.tagid == 21 && x.attributes.tagvalues != null && x.attributes.tagvalues.Count > 0))
                {
                    if (led.attributes.tagvalues.Exists(x => x.id == locationid))
                    {
                        dailyPandL.DailyExpense += led.attributes.tagvalues.Find(x => x.id == locationid && x.value > 0).value;
                        dailyPandL.DailyExpense_List.Add(new MobileInsight.FinanceSummary.NameAndValue()
                        {
                            name = led.displayname,
                            value = led.attributes.tagvalues.Find(x => x.id == locationid).value
                        });
                    }
                }
            }
            //{ 'id': 22, 'name': 'Weekly Expense' },
            if (ledgers.Exists(x => x.tagid == 22 && x.attributes.tagvalues != null && x.attributes.tagvalues.Count > 0))
            {
                dailyPandL.WeeklyExpense_List = new List<MobileInsight.FinanceSummary.NameAndValue>();
                foreach (var led in ledgers.FindAll(x => x.tagid == 22 && x.attributes.tagvalues != null && x.attributes.tagvalues.Count > 0))
                {
                    if (led.attributes.tagvalues.Exists(x => x.id == locationid && x.value > 0))
                    {
                        dailyPandL.WeeklyExpense += (led.attributes.tagvalues.Find(x => x.id == locationid).value / 7);
                        dailyPandL.WeeklyExpense_List.Add(new MobileInsight.FinanceSummary.NameAndValue()
                        {
                            name = led.displayname,
                            value = (led.attributes.tagvalues.Find(x => x.id == locationid).value / 7)
                        });
                    }
                }
            }

            //{ 'id': 23, 'name': 'Monthly Expense' },
            if (ledgers.Exists(x => x.tagid == 23 && x.attributes.tagvalues != null && x.attributes.tagvalues.Count > 0))
            {
                dailyPandL.MonthlyExpense_List = new List<MobileInsight.FinanceSummary.NameAndValue>();
                DateTime date = DateTimeNow.Date.AddMonths(1);
                int noofdays = date.AddDays(date.Day * -1).AddDays(-1).Day;

                foreach (var led in ledgers.FindAll(x => x.tagid == 23 && x.attributes.tagvalues != null && x.attributes.tagvalues.Count > 0))
                {
                    if (led.attributes.tagvalues.Exists(x => x.id == locationid && x.value > 0))
                    {
                        dailyPandL.MonthlyExpense += (led.attributes.tagvalues.Find(x => x.id == locationid).value / noofdays);
                        dailyPandL.MonthlyExpense_List.Add(new MobileInsight.FinanceSummary.NameAndValue()
                        {
                            name = led.displayname,
                            value = (led.attributes.tagvalues.Find(x => x.id == locationid).value / noofdays)
                        });
                    }
                }
            }

            //{ 'id': 24, 'name': 'Yearly Expense' },
            if (ledgers.Exists(x => x.tagid == 24 && x.attributes.tagvalues != null && x.attributes.tagvalues.Count > 0))
            {
                dailyPandL.YearlyExpense_List = new List<MobileInsight.FinanceSummary.NameAndValue>();
                foreach (var led in ledgers.FindAll(x => x.tagid == 24 && x.attributes.tagvalues != null && x.attributes.tagvalues.Count > 0))
                {
                    if (led.attributes.tagvalues.Exists(x => x.id == locationid && x.value > 0))
                    {
                        dailyPandL.YearlyExpense += (led.attributes.tagvalues.Find(x => x.id == locationid).value / 365);
                        dailyPandL.YearlyExpense_List.Add(new MobileInsight.FinanceSummary.NameAndValue()
                        {
                            name = led.displayname,
                            value = (led.attributes.tagvalues.Find(x => x.id == locationid).value / 365)
                        });
                    }
                }
            }

            return dailyPandL;
        }

        public class DailyPandL
        {
            public decimal stockvalue { get; set; }
            public decimal costvalue { get; set; }
            public decimal salevalue { get; set; }
            public decimal taxvalue { get; set; }
            public decimal grossmargin { get; set; }
            public long loctionid { get; set; }
            public decimal InventoryInterestPercentage { get; set; }
            public decimal InventoryInterest { get; set; }
            public decimal PanLExpense { get; set; }
            public List<MobileInsight.FinanceSummary.NameAndValue> PanLExpense_List { get; set; }
            public decimal DailyExpense { get; set; }
            public List<MobileInsight.FinanceSummary.NameAndValue> DailyExpense_List { get; set; }
            public decimal WeeklyExpense { get; set; }
            public List<MobileInsight.FinanceSummary.NameAndValue> WeeklyExpense_List { get; set; }
            public decimal MonthlyExpense { get; set; }
            public List<MobileInsight.FinanceSummary.NameAndValue> MonthlyExpense_List { get; set; }
            public decimal YearlyExpense { get; set; }
            public List<MobileInsight.FinanceSummary.NameAndValue> YearlyExpense_List { get; set; }
            public decimal AssetDepreciation { get; set; }
            public List<MobileInsight.FinanceSummary.NameAndValue> AssetDepreciation_List { get; set; }
            public decimal AssetValue { get; set; }
            public decimal SalaryExpenseValue { get; set; }
            public decimal SalaryExpense { get; set; }
            public decimal SalesMarginExpense { get; set; }
            public decimal SalesMarginExpensePercentage { get; set; }
            public decimal PurchaseMarginxpense { get; set; }
            public decimal PurchaseMarginxpensePercentage { get; set; }
        }

        public class QuantoLedgers
        {
            public List<TaxLedgerSummary> TaxLegers { get; set; }
            public List<PurchaseLedgerSummary> PurchaseLegers { get; set; }
            public List<TallyDataservice.CardSales> B2BSalesList { get; set; }
            public List<TallyDataservice.CardSales> CashSalesList { get; set; }
            public List<TallyDataservice.CardSales> CardSalesList { get; set; }
            public List<TallyDataservice.CardSales> VoucherSalesList { get; set; }
            public List<SupplierPaymentSummary> SupplierPayment { get; set; }
            public List<PurchaseReturnSummary> PurchaseReturn { get; set; }
            public List<CustomerPaymentSummary> CustomerPayment { get; set; }
            public List<CustomerPaymentSummary> CustomerAdjustment { get; set; }
            public List<TaxLedger> TaxList { get; set; }
            public List<SalesSummary> BillList { get; set; }
            public List<BalanceInfo> SupplierBalance { get; set; }
            public List<AdditionalCharges> AdditionalCharges { get; set; }
            public List<TallyDataservice.CardSales> Unsettled { get; internal set; }

            internal void Merge(QuantoLedgers fromdirect)
            {

                
                if (fromdirect.TaxList != null &&
                    fromdirect.TaxList.Count > 0)
                {
                    fromdirect.TaxList.ForEach(x =>
                    {
                        if (TaxList.Exists(y => y.IsCredit == x.IsCredit &&
                            y.IsCreditNote == x.IsCreditNote &&
                            y.CustomerID == x.CustomerID &&
                            y.LocationID == x.LocationID &&
                            y.IsInterstate == x.IsInterstate &&
                            y.taxpercentage == x.taxpercentage))
                        {

                            var ctax = TaxList.Find(y => y.IsCredit == x.IsCredit &&
                            y.IsCreditNote == x.IsCreditNote &&
                            y.CustomerID == x.CustomerID &&
                            y.IsInterstate == x.IsInterstate &&
                            y.taxpercentage == x.taxpercentage);
                            ctax.amount += x.amount;
                            ctax.tax += x.tax;
                        }
                        else
                        {
                            TaxList.Add(x);
                        }
                    });
                }

                if (fromdirect.BillList != null &&
                    fromdirect.BillList.Count > 0)
                {
                    fromdirect.BillList.ForEach(x =>
                    {
                        if (BillList.Exists(y => y.IsCredit == x.IsCredit &&
                            y.IsCreditNote == x.IsCreditNote &&
                            y.CustomerID == x.CustomerID &&
                            y.LocationID == x.LocationID &&
                            y.IsInterstate == x.IsInterstate))
                        {
                            var ctax = BillList.Find(y => y.IsCredit == x.IsCredit &&
                            y.IsCreditNote == x.IsCreditNote &&
                            y.LocationID == x.LocationID &&
                            y.CustomerID == x.CustomerID &&
                            y.IsInterstate == x.IsInterstate);
                            ctax.Amount += x.Amount;
                        }
                        else
                        {
                            BillList.Add(x);
                        }
                    });
                }

                if (fromdirect.VoucherSalesList != null &&
                    fromdirect.VoucherSalesList.Count > 0)
                {
                    fromdirect.VoucherSalesList.ForEach(x =>
                    {
                        if (VoucherSalesList.Exists(y => y.locationid == x.locationid && x.counterid == y.counterid))
                        {
                            var vs = VoucherSalesList.Find(y => y.locationid == x.locationid && x.counterid == y.counterid);
                            vs.amount += x.amount;
                        }
                        else
                        {
                            VoucherSalesList.Add(x);
                        }
                    });
                }

                if (fromdirect.AdditionalCharges != null &&
                    fromdirect.AdditionalCharges.Count > 0)
                {
                    fromdirect.AdditionalCharges.ForEach(x =>
                    {
                        if (AdditionalCharges.Exists(y => y.LocationID == x.LocationID && x.CounterID == y.CounterID
                         && x.IsInterstate == y.IsInterstate && x.taxpercentage == y.taxpercentage))
                        {
                            var vs = AdditionalCharges.Find(y => y.LocationID == x.LocationID && x.CounterID == y.CounterID
                                        && x.IsInterstate == y.IsInterstate && x.taxpercentage == y.taxpercentage);
                            vs.tax += x.tax;
                            vs.charge += x.charge;
                        }
                        else
                        {
                            AdditionalCharges.Add(x);
                        }
                    });
                }


                if (fromdirect.B2BSalesList != null &&
                    fromdirect.B2BSalesList.Count > 0)
                {
                    if (B2BSalesList == null || B2BSalesList.Count == 0)
                    {
                        B2BSalesList = new List<TallyDataservice.CardSales>();
                        B2BSalesList.AddRange(fromdirect.B2BSalesList);
                    }
                }

                if (fromdirect.Unsettled != null &&
                    fromdirect.Unsettled.Count > 0)
                {
                    if (Unsettled == null || Unsettled.Count == 0)
                    {
                        Unsettled = new List<TallyDataservice.CardSales>();
                    }
                    Unsettled.AddRange(fromdirect.Unsettled);
                }
            }
        }
        public class PurchaseReturnSummary : TallyDataservice.ReturnInfo
        {
            public decimal diff { get; internal set; }
        }
        public class CustomerPaymentSummary : TallyDataservice.CustomerPaymentInfo
        {
            public decimal amount { get; set; }
            public string vouchertype { get; set; }
            public string ledger { get; set; }
        }
        public class BalanceInfo
        {
            public long id { get; set; }
            public string name { get; set; }
            public decimal amount { get; set; }
            public int type { get; set; }
        }
        public class SupplierPaymentSummary : TallyDataservice.PaymentInfo
        {
            public long bankid { get; set; }
        }
        public class TaxLedgerSummary : TallyDataservice.TaxSummary
        {
            public bool adjusted { get; internal set; }
        }
        public class TaxLedger : TallyDataservice.BillInfo.TaxInfo
        {
            public string Customer { get; set; }
            public string Billno { get; set; }
            public string Location { get; set; }
            public long LocationID { get; set; }
            public long CustomerID { get; set; }
            public string LocationName { get; set; }
            public bool IsInterstate { get; set; }
            public bool IsCredit { get; set; }
            public bool IsCreditNote { get; set; }
            public decimal taxpercentage { get; set; }
            public decimal amount { get; set; }
            public decimal tax { get; set; }
            public DateTime date { get; set; }
            //public long CounterID { get; internal set; }
            //public string Counter { get; internal set; }
            //public string CounterName { get; internal set; }
        }
        public class CardSummary : TallyDataservice.CardSales
        {
            public long refid { get; set; }
        }
        public class SalesSummary : TallyDataservice.BillInfo
        {
            public string billno { get; set; }
            public long CounterID { get; internal set; }
            public string Counter { get; internal set; }
            public string CounterName { get; internal set; }
        }

        public class AdditionalCharges
        {
            public string customer { get; set; }
            public string chargename { get; set; }

            public string LocationCode { get; internal set; }
            public string CounterCode { get; internal set; }
            public long LocationID { get; internal set; }
            public string LocationName { get; internal set; }
            public long CounterID { get; internal set; }
            public string CounterName { get; internal set; }

            public decimal charge { get; set; }
            public decimal tax { get; set; }
            public decimal taxpercentage { get; set; }
            public bool IsInterstate { get; internal set; }
        }
        public class PurchaseLedgerSummary : TallyDataservice.InvoiceInfo
        {
            public decimal additionaldiscount { get; set; }
            public decimal additionalcharges { get; set; }
            public List<TaxLedgerSummary> taxledgersummary { get; internal set; }
            public decimal zerovalued { get; set; }
        }
        public QuantoLedgers SupplierReturnLedger(long companyid, int year)
        {
            var fromdate = new DateTime(year, 4, 1);
            var todate = new DateTime(year, 4, 1).AddYears(1);
            return SupplierReturnLedger(companyid, fromdate, todate, false);
        }
        internal QuantoLedgers SupplierReturnLedger(long companyid, DateTime from, DateTime to, bool isdetail)
        {

            string query = @"
                            select lrinvoicereturnitems.taxpercentage,lrinvoicereturn.returntype,
                            lrinvoicereturn.isinterstatesale,lrinvoicereturn.suppliercompanyid id,
                            sum(lrinvoicereturnitems.amount-lrinvoicereturnitems.discount) amount,
                            sum(lrinvoicereturnitems.tax) tax from lrinvoicereturnitems 
                            inner join lrinvoicereturn on lrinvoicereturn.id = lrinvoicereturnitems.lrinvoicereturnid 
                            inner join suppliercompany on suppliercompany.id = lrinvoicereturn.suppliercompanyid
                            left outer join referencelist state on state.id = suppliercompany.stateid
                            left outer join referencelist city on city.id = suppliercompany.cityid
                            where lrinvoicereturn.companyid=@companyid and lrinvoicereturn.isactive = true 
                            and lrinvoicereturnitems.isactive=true and returndate>=@fromdate and returndate<@todate {0}
			                group by lrinvoicereturn.suppliercompanyid,lrinvoicereturn.isinterstatesale,
                            lrinvoicereturnitems.taxpercentage,lrinvoicereturn.returntype;

                            select sum(tax) tax,sum(amount) amount,sum(freightamt) frightamt,
                            sum(total) total,sum(total) total,sum(freighttaxamt) freighttaxamt,
                            suppliercompany.name as suppliercompany,lrinvoicereturn.isinterstatesale,
                            lrinvoicereturn.suppliercompanyid id,lrinvoicereturn.returntype,
                            city.name cityname
                            from lrinvoicereturn
                            left outer join suppliercompany on suppliercompany.id = lrinvoicereturn.suppliercompanyid
                            left outer join referencelist city on city.id = suppliercompany.cityid
                            where lrinvoicereturn.companyid=@companyid and lrinvoicereturn.isactive = true 
                            and returndate>=@fromdate and returndate<@todate {0}
                            group by suppliercompany.name,lrinvoicereturn.isinterstatesale,
                            lrinvoicereturn.suppliercompanyid,lrinvoicereturn.returntype,city.name;";

            if (isdetail)
            {
                query = @"
                            select lrinvoicereturnitems.taxpercentage,lrinvoicereturn.returnno,
                            lrinvoicereturn.isinterstatesale,lrinvoicereturn.suppliercompanyid id,
                            lrinvoicereturn.returntype,
                            lrinvoicereturnitems.lrinvoicereturnid,STRING_AGG(coalesce(lrinvoice.invoiceno,''),',') invoiceno,
                            sum(lrinvoicereturnitems.amount-lrinvoicereturnitems.discount) amount,
                            sum(lrinvoicereturnitems.tax) tax,returndate from lrinvoicereturnitems 
                            inner join lrinvoicereturn on lrinvoicereturn.id = lrinvoicereturnitems.lrinvoicereturnid
                            inner join suppliercompany on suppliercompany.id = lrinvoicereturn.suppliercompanyid
                            left outer join lrinvoiceitems on lrinvoiceitems.id = lrinvoiceitemsid
                            left outer join lrinvoice on lrinvoice.id = lrinvoiceitems.lrinvoiceid
                            left outer join referencelist state on state.id = suppliercompany.stateid
                            left outer join referencelist city on city.id = suppliercompany.cityid
                            where lrinvoicereturn.companyid=@companyid and lrinvoicereturn.isactive = true  
                            and lrinvoicereturnitems.isactive=true and returndate>=@fromdate and returndate<@todate {0}
			                group by lrinvoicereturn.suppliercompanyid,lrinvoicereturn.isinterstatesale,
                            lrinvoicereturnitems.taxpercentage,returndate,lrinvoicereturn.returnno,
                            lrinvoicereturnitems.lrinvoicereturnid,lrinvoicereturn.returntype;

                            select sum(tax) tax,sum(amount) amount,sum(freightamt) frightamt,
                            sum(total) total,sum(total) total,sum(freighttaxamt) freighttaxamt,
                            suppliercompany.name as suppliercompany,lrinvoicereturn.isinterstatesale,
                            lrinvoicereturn.suppliercompanyid id,returndate,lrinvoicereturn.returnno,
                            lrinvoicereturn.id lrinvoicereturnid,lrinvoicereturn.createdby,lrinvoicereturn.returntype,
                            city.name cityname
                            from lrinvoicereturn
                            left outer join suppliercompany on suppliercompany.id = lrinvoicereturn.suppliercompanyid
                            left outer join referencelist city on city.id = suppliercompany.cityid
                            where lrinvoicereturn.companyid=@companyid and lrinvoicereturn.isactive = true 
                            and returndate>=@fromdate and returndate<@todate {0}
                            group by suppliercompany.name,lrinvoicereturn.isinterstatesale,
                            lrinvoicereturn.id,city.name,
                            returndate,lrinvoicereturn.suppliercompanyid,lrinvoicereturn.returnno,lrinvoicereturn.createdby,lrinvoicereturn.returntype;
";
            }
            SortedList<string, object> param = new SortedList<string, object>();
            param.Add("fromdate", from);
            param.Add("todate", to.AddDays(1));
            param.Add("companyid", companyid);
            query = string.Format(query, "");
            List<PurchaseReturnSummary> invoicelist = new List<PurchaseReturnSummary>();
            List<TallyDataservice.TaxSummary> taxlist = new List<TallyDataservice.TaxSummary>();
            using (System.Data.IDataReader reader = ExecuteReader(query, param, 1000))
            {
                while (reader.Read())
                {
                    TallyDataservice.TaxSummary invoice = new TallyDataservice.TaxSummary();
                    invoice.taxable = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"]);
                    invoice.tax = reader["tax"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["tax"]);
                    invoice.percentage = reader["taxpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["taxpercentage"]);
                    invoice.isinterstatesale = reader["isinterstatesale"] == DBNull.Value ? false : Convert.ToBoolean(reader["isinterstatesale"]);
                    invoice.suppliercompanyid = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                    invoice.returntype = reader["returntype"] == DBNull.Value ? 0 : Convert.ToInt32(reader["returntype"]);
                    if (isdetail)
                    {
                        invoice.invoiceid = reader["lrinvoicereturnid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["lrinvoicereturnid"]);
                        invoice.returnno = reader["returnno"] == DBNull.Value ? "" : Convert.ToString(reader["returnno"]);
                        invoice.invoiceno = reader["invoiceno"] == DBNull.Value ? "" : Convert.ToString(reader["invoiceno"]);
                        invoice.date = reader["returndate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["returndate"]);
                    }
                    taxlist.Add(invoice);
                }
                if (reader.NextResult())
                {
                    while (reader.Read())
                    {
                        //sum(amounton - lrinvoicetax.discount) taxable,sum(lrinvoicetax.taxamount) tax,type,
                        //  lrinvoicetax.percentage,lrinvoicetax.isinterstatesale,invoiceid
                        PurchaseReturnSummary invoice = new PurchaseReturnSummary();
                        invoice.suppliercompanyid = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        invoice.returntype = reader["returntype"] == DBNull.Value ? 0 : Convert.ToInt32(reader["returntype"]);
                        invoice.isinterstatesale = reader["isinterstatesale"] == DBNull.Value ? false : Convert.ToBoolean(reader["isinterstatesale"]);
                        invoice.totalamount = reader["total"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["total"]);
                        invoice.baseamount = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"]);
                        invoice.roundamount = invoice.totalamount - invoice.baseamount;
                        invoice.suppliercompany = reader["suppliercompany"] == DBNull.Value ? "" : reader["suppliercompany"].ToString();
                        invoice.cityname = reader["cityname"] == DBNull.Value ? "" : reader["cityname"].ToString();
                        if (isdetail)
                        {
                            invoice.invoiceid = reader["lrinvoicereturnid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["lrinvoicereturnid"]);
                            invoice.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                            invoice.returnno = reader["returnno"] == DBNull.Value ? "" : Convert.ToString(reader["returnno"]);
                            invoice.returndate = reader["returndate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["returndate"]);
                            invoice.taxsummary = taxlist.FindAll(x => x.invoiceid == invoice.invoiceid);
                            var invoicenos = new List<string>(string.Join(",", invoice.taxsummary.ConvertAll(x => x.invoiceno)).Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries));
                            invoice.invoiceno = string.Join(",", invoicenos.Distinct().ToList());
                        }
                        else
                        {
                            invoice.taxsummary = taxlist.FindAll(x => x.suppliercompanyid == invoice.suppliercompanyid 
                            && x.isinterstatesale == invoice.isinterstatesale && x.returntype == invoice.returntype);
                        }


                        invoice.taxsummary.ForEach(x => taxlist.Remove(x));
                        invoice.diff = Math.Abs(invoice.totalamount - (invoice.taxsummary.Sum(x => x.tax + x.taxable)));
                        invoicelist.Add(invoice);
                    }
                }
            }

            using (var reader = ExecuteReader(@"select suppliercompany.name from suppliercompany
                        group by suppliercompany.name
                        having count(*)>1", null))
            {
                List<string> names = new List<string>();
                while (reader.Read())
                {
                    names.Add(reader.IsDBNull(0) ? "" : reader.GetString(0));
                }
                invoicelist.FindAll(x => names.Contains(x.suppliercompany)).ForEach(y =>
                {
                    y.suppliercompany += "(" + y.cityname + ")";
                });
            }

            var difflist = invoicelist.FindAll(x => x.diff > 100);
            var diffvalue = difflist.Sum(x => x.diff);
            return new QuantoLedgers()
            {
                PurchaseReturn = invoicelist
            };
        }

        internal QuantoLedgers CustomerBalanceLedger(long companyid, int year)
        {

            string query = @"select customer.id,type,customer.name customername,
sum(round(customerinvoice.receivable::numeric,0)-(customerinvoice.paid+customerinvoice.discount+customerinvoice.advance)::numeric) amount 
from customerinvoice 
left outer join bill on bill.id = customerinvoice.billid and bill.billtype != 9 
left outer join customer on customer.id = customerinvoice.customerid
where customerinvoice.isactive=True And customerinvoice.companyid=@companyid
And customerinvoice.type in (1,2,5) 
and (round(customerinvoice.receivable::numeric,0)-(customerinvoice.paid+customerinvoice.discount)::numeric)!=0::numeric 
group by customer.id,type,customer.name ";

            SortedList<string, object> param = new SortedList<string, object>();
            param.Add("companyid", companyid);

            query = string.Format(query, "");

            List<BalanceInfo> invoicelist = new List<BalanceInfo>();

            using (System.Data.IDataReader reader = ExecuteReader(query, param, 1000))
            {
                while (reader.Read())
                {
                    BalanceInfo invoice = new BalanceInfo();
                    invoice.name = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
                    invoice.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                    invoice.type = reader["type"] == DBNull.Value ? 0 : Convert.ToInt32(reader["type"]);
                    invoice.amount = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"]);
                    invoicelist.Add(invoice);
                }
            }


            return new QuantoLedgers()
            {
                SupplierBalance = invoicelist
            };
        }
        internal QuantoLedgers SupplierBalanceLedger(long companyid, int year)
        {

            string query = @"select sum(lrinvoice.totalamount-lrinvoice.presettled-COALESCE(paidinvoice.paidamount,0)) amount,
suppliercompany.supplierid id,suppliercompany.name
from lrinvoice 
left outer join (select sum(supplierpaymentinvoice.paidamount) paidamount,invoiceid
from supplierpaymentinvoice
inner join supplierpayment on supplierpayment.id = supplierpaymentinvoice.supplierpaymentid
where islrinvoicereturn = false and supplierpayment.isactive = true  and supplierpayment.hascancel=false  
and supplierpayment.companyid = @companyid and supplierpaymentinvoice.isactive=true 
and supplierpaymentinvoice.hascancel=false and islrinvoicereturn = false
group by invoiceid) paidinvoice on paidinvoice.invoiceid = lrinvoice.id
left outer join suppliercompany on suppliercompany.id = lrinvoice.suppliercompanyid
left outer join supplier on supplier.id = suppliercompany.supplierid
left outer join lrentry on lrentry.id = lrinvoice.lrid
where lrinvoice.isactive = true and lrinvoice.settled = false and lrinvoice.companyid = @companyid
and lrinvoice.totalamount-COALESCE(paidinvoice.paidamount,0) != 0 
group by suppliercompany.supplierid,suppliercompany.name";

            SortedList<string, object> param = new SortedList<string, object>();
            param.Add("companyid", companyid);

            query = string.Format(query, "");

            List<BalanceInfo> invoicelist = new List<BalanceInfo>();

            using (System.Data.IDataReader reader = ExecuteReader(query, param, 1000))
            {
                while (reader.Read())
                {
                    BalanceInfo invoice = new BalanceInfo();
                    invoice.name = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
                    invoice.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                    invoice.amount = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"]);
                    invoicelist.Add(invoice);
                }
            }


            return new QuantoLedgers()
            {
                SupplierBalance = invoicelist
            };
        }
        public QuantoLedgers CreateCreditPaymentVouchers(long companyid, long locationid, int year)
        {
            var fromdate = new DateTime(year, 4, 1);
            var todate = new DateTime(year, 4, 1).AddYears(1);
            return CreateCreditPaymentVouchers(companyid, locationid, fromdate, todate, false);
        }
        internal QuantoLedgers CreateCreditPaymentVouchers(long companyid, long locationid, DateTime from, DateTime to, bool isdetail)
        {

            string query = @"select customer.id,customer.name,bank.id bankid,bank.name bankname,
                            cast(paymentinfo->>'paymentmode' as int) paymentmode, 
                            sum(customerpayment.AMOUNT) paid,sum(customerpayment.discount) discount							
                            from customerpayment
                            inner join customer on customer.id = customerpayment.customerid
                            left outer join referencelist bank on bank.id = cast(paymentinfo->>'bankid' as bigint)
                            where customerpayment.isactive=true 
                            and customerpayment.paidon>=@fromdate and customerpayment.paidon<@todate and 
                            customerpayment.companyid=@companyid and bill.locationid=@locationid {0}
                            group by customer.id,customer.name,bank.name,bank.id,cast(paymentinfo->>'paymentmode' as int)";

            if (isdetail)
            {
                query = @"select customer.id,customer.name,bank.id bankid,bank.name bankname,
                            cast(paymentinfo->>'paymentmode' as int) paymentmode, 
                            customerpayment.paidon date,customerpayment.receiptno,
                            sum(customerpayment.AMOUNT) paid,sum(customerpayment.discount) discount							
                            from customerpayment
                            inner join customer on customer.id = customerpayment.customerid
                            left outer join referencelist bank on bank.id = cast(paymentinfo->>'bankid' as bigint)
                            where customerpayment.isactive=true 
                            and customerpayment.paidon>=@fromdate and customerpayment.paidon<@todate and 
                            customerpayment.companyid=@companyid and bill.locationid=@locationid {0}
                            group by customer.id,customer.name,bank.name,bank.id,customerpayment.receiptno,
                            customerpayment.paidon,cast(paymentinfo->>'paymentmode' as int)";
            }

            SortedList<string, object> param = new SortedList<string, object>();
            param.Add("fromdate", from);
            param.Add("todate", to.AddDays(1));
            param.Add("companyid", companyid);
            param.Add("locationid", locationid);

            query = string.Format(query, "");
            if (locationid <= 0)
            {
                query = query.Replace("and bill.locationid=@locationid", "");
            }

            SortedDictionary<long, CustomerPaymentSummary> invoicelist = new SortedDictionary<long, CustomerPaymentSummary>();

            /*var allsalesdata = ExecuteDataSet(query, param);
            using (System.IO.MemoryStream memoryStream = new System.IO.MemoryStream())
            {
                ClosedXML.Excel.XLWorkbook xLWorkbook = new ClosedXML.Excel.XLWorkbook();
                foreach (System.Data.DataTable table in allsalesdata.Tables)
                {
                    var sheet = xLWorkbook.AddWorksheet(table);
                }
                xLWorkbook.SaveAs(memoryStream);
                //System.IO.File.WriteAllBytes(@"e:\t.xlsx", memoryStream.ToArray());
            }*/

            using (System.Data.IDataReader reader = ExecuteReader(query, param, 1000))
            {
                while (reader.Read())
                {
                    CustomerPaymentSummary invoice = new CustomerPaymentSummary();
                    invoice.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                    invoice.bankid = reader["bankid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["bankid"]);
                    invoice.customername = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
                    invoice.bankname = reader["bankname"] == DBNull.Value ? "" : reader["bankname"].ToString();
                    int paymentmode = reader["paymentmode"] == DBNull.Value ? 0 : (Int32)reader["paymentmode"];
                    if (isdetail)
                    {
                        invoice.receiptno = reader["receiptno"] == DBNull.Value ? "" : reader["receiptno"].ToString();
                        invoice.date = reader["date"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["date"]);
                    }
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
                    invoice.allocations = new List<TallyDataservice.CustomerPaymentInfo.Allocation>();
                    if (invoicelist.ContainsKey(invoice.id))
                        invoice = invoicelist[invoice.id];
                    else
                        invoicelist.Add(invoice.id, invoice);
                    invoice.allocations.Add(new TallyDataservice.CustomerPaymentInfo.Allocation()
                    {
                        paid = reader["paid"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["paid"]),
                        discount = reader["discount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discount"])
                    });
                }
            }

            List<CustomerPaymentSummary> allledgers = invoicelist.Values.ToList();
            var bankledgers = GetLedgerbanks(allledgers.ConvertAll(x => x.bankid).ToList());
            bankledgers.ForEach(x =>
            {
                allledgers.FindAll(y => y.bankid == x.Key).ForEach(z =>
                {
                    z.bankname = x.Value;
                });
            });

            List<long> customerids = new List<long>();
            if (allledgers.Exists(x => x.id > 0))
            {
                customerids.AddRange(allledgers.FindAll(x => x.id > 0).ConvertAll(y => y.id));
            }

            if (customerids.Count > 0)
            {
                using (var reader = ExecuteReader(string.Format(@"
                                    with duplicate as
                                    (
                                    select name from customer
                                    where isactive=true and customertype in (2, 7, 6,8)
                                    and customer.id in ({0})
                                    group by name
                                    having count(*)>1
                                    ) 
                                    select id,customer.name,customer.code from customer
                                    inner join duplicate on duplicate.name = customer.name
                                    where isactive=true and customertype in (2, 7, 6,8)
                                    order by customer.name", string.Join(",", customerids)), null,10000))
                {
                    while (reader.Read())
                    {
                        long id = reader.GetInt64(0);
                        string sname = reader.IsDBNull(1) ? "" : reader.GetString(1);
                        string cname = reader.IsDBNull(2) ? "" : reader.GetString(2);
                        if (allledgers.Exists(x => x.id == id))
                        {
                            allledgers.FindAll(x => x.id == id).ForEach(y =>
                            {
                                y.customername = sname + "(" + cname + ")";
                            });
                        }
                    }
                }
            }
            return new QuantoLedgers()
            {
                CustomerPayment = invoicelist.Values.ToList()
            };
        }

        public SortedDictionary<long, string> GetLedgerbanks(List<long> ids)
        {
            ids.RemoveAll(x => x == 0);
            SortedDictionary<long, string> ledgers = new SortedDictionary<long, string>();
            if (ids.Count == 0) return ledgers;

            string query = String.Format(@"select DISPLAYNAME,BANKID from ledger where BANKID IN ({0})", string.Join(",", ids));
            using (var reader = ExecuteReader(query, null))
            {
                while (reader.Read())
                {
                    ledgers.Add(Convert.ToInt64(reader["BANKID"]), reader["DISPLAYNAME"].ToString());
                }
            }
            return ledgers;
        }

        public QuantoLedgers SearchSupplierPaymentSummary(long companyid, long locationid, int year)
        {
            var fromdate = new DateTime(year, 4, 1);
            var todate = new DateTime(year, 4, 1).AddYears(1);
            return SearchSupplierPaymentSummary(companyid, locationid, fromdate, todate, false);
        }

        public class TDSSummaryItem
        {
            public string supplier { get; set; }
            public long tdsledgerid { get; set; }
            public decimal amount { get; set; }
        }
        public List<TDSSummaryItem> TDSSummary(long companyid, DateTime from, DateTime to)
        {
            List<TDSSummaryItem> response = new List<TDSSummaryItem>();
            NpgsqlConnection conn = CreateConnection();
            List<Tuple<VoucherDisplayItem, List<VoucherDisplayItem>>> lst = new List<Tuple<VoucherDisplayItem, List<VoucherDisplayItem>>>();
            try
            {
                conn.Open();


                string purchasetaxquery = @"select supplierpayment.tdsledgerid,sum(supplierpayment.tdsamount),suppliercompany.name 
                    from supplierpayment
                    inner join suppliercompany on suppliercompany.id = supplierpayment.suppliercompanyid
                    where tdsamount>0
					and tdsoninv=false
					and supplierpayment.isactive=true
                    and supplierpayment.companyid = @companyid
                    and supplierpayment.paymentdate>=@fromdate and supplierpayment.paymentdate<@todate
                    group by supplierpayment.tdsledgerid,suppliercompany.name";

                if (companyid <= 0)
                    purchasetaxquery = purchasetaxquery.Replace("and supplierpayment.companyid = @companyid", "");

                var command = GetCommand(purchasetaxquery, conn);
                command.Parameters.AddWithValue("fromdate", from);
                command.Parameters.AddWithValue("companyid", companyid);
                command.Parameters.AddWithValue("todate", to.AddDays(1));
                command.CommandTimeout = 1000;
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {

                        response.Add(new TDSSummaryItem()
                        {
                            supplier = reader["name"] == DBNull.Value ? "" : Convert.ToString(reader["name"]),
                            tdsledgerid = Convert.ToInt64(reader["tdsledgerid"]),
                            amount = (reader[1] == DBNull.Value ? 0 : Convert.ToDecimal(reader[1]))
                        });
                    }
                }

                purchasetaxquery = @"select tdsledgerid,sum(lrinvoice.tdsamount),suppliercompany.name
                    from lrinvoice
                    inner join suppliercompany on suppliercompany.id = lrinvoice.suppliercompanyid
                    where tdsamount>0 and lrinvoice.isactive=true
                    and lrinvoice.entrydate>=@fromdate and lrinvoice.entrydate<@todate
                    and lrinvoice.companyid=@companyid
                    group by tdsledgerid,suppliercompany.name";

                if (companyid <= 0)
                    purchasetaxquery = purchasetaxquery.Replace("and lrinvoice.companyid=@companyid", "");

                command = GetCommand(purchasetaxquery, conn);
                command.Parameters.AddWithValue("fromdate", from);
                command.Parameters.AddWithValue("todate", to.AddDays(1));
                command.Parameters.AddWithValue("companyid", companyid);
                command.CommandTimeout = 1000;
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        response.Add(new TDSSummaryItem()
                        {
                            supplier = reader["name"] == DBNull.Value ? "" : Convert.ToString(reader["name"]),
                            tdsledgerid = Convert.ToInt64(reader["tdsledgerid"]),
                            amount = (reader[1] == DBNull.Value ? 0 : Convert.ToDecimal(reader[1])) * -1
                        });
                    }
                }
            }
            catch (Exception exp)
            {
                if (conn != null && conn.State == System.Data.ConnectionState.Open)
                    conn.Close();
            }
            return response;
        }
        public QuantoLedgers SearchSupplierPaymentSummary(long companyid, long locationid, DateTime from, DateTime to, bool isdetail)
        {
            string query = @"select suppliercompany.id id,suppliercompany.name,islrinvoicereturn,bank.name as bankname,paymentmode,
                            sum(Supplierpaymentinvoice.paidamount) paidamount,bank.id bankid,
                            sum(Supplierpaymentinvoice.discountamount) discountamount,city.name cityname,
                            sum(Supplierpaymentinvoice.agentcommissionvalue) agentcommissionvalue
                            from supplierpayment
                            inner join Supplierpaymentinvoice on Supplierpaymentinvoice.supplierpaymentid = supplierpayment.id
                            inner join suppliercompany on suppliercompany.id = supplierpayment.suppliercompanyid
                            left outer join referencelist city on city.id = suppliercompany.cityid
							left outer join referencelist bank on bank.id = supplierpayment.bankid
                            where supplierpayment.isactive=true  and supplierpayment.hascancel=false
                            and Supplierpaymentinvoice.isactive = true and supplierpaymentinvoice.hascancel=false
                            and supplierpayment.paymentdate>=@fromdate and supplierpayment.paymentdate<@todate 
                            and supplierpayment.companyid=@companyid and supplierpayment.locationid=@locationid {0}
                            group by suppliercompany.id,suppliercompany.name,city.name,islrinvoicereturn,bank.name,bank.id ,paymentmode;

                            select suppliercompany.id id,supplierpayment.bankid,paymentmode,
							sum(tdsamount) tdsamount,sum(rounding) rounding
                            from supplierpayment
                            inner join suppliercompany on suppliercompany.id = supplierpayment.suppliercompanyid
                            where supplierpayment.isactive=true and supplierpayment.hascancel=false and supplierpayment.tdsoninv=false
                            and supplierpayment.paymentdate>=@fromdate and supplierpayment.paymentdate<@todate 
                            and supplierpayment.companyid=@companyid and supplierpayment.locationid=@locationid {0}
                            group by suppliercompany.id,supplierpayment.bankid,paymentmode";

            if (isdetail)
            {
                query = @"
                            select suppliercompany.id id,suppliercompany.name,islrinvoicereturn,bank.name as bankname,paymentmode,
							supplierpayment.paymentdate,sum(Supplierpaymentinvoice.paidamount) paidamount,supplierpayment.createdby,city.name cityname,
                            sum(discountamount) discountamount,sum(agentcommissionvalue) agentcommissionvalue,supplierpayment.receiptno,supplierpayment.id paymentid,
                            supplierpayment.referencedetail,STRING_AGG(Supplierpaymentinvoice.invoiceno,',') invoiceno,bank.id bankid
                            from supplierpayment
                            inner join Supplierpaymentinvoice on Supplierpaymentinvoice.supplierpaymentid = supplierpayment.id
                            inner join suppliercompany on suppliercompany.id = supplierpayment.suppliercompanyid
                            left outer join referencelist city on city.id = suppliercompany.cityid
							left outer join referencelist bank on bank.id = supplierpayment.bankid
                            where supplierpayment.isactive=true  and supplierpayment.hascancel=false 
                            and Supplierpaymentinvoice.isactive = true and supplierpaymentinvoice.hascancel=false
                            and supplierpayment.paymentdate>=@fromdate and supplierpayment.paymentdate<@todate 
                            and supplierpayment.companyid=@companyid and supplierpayment.locationid=@locationid
                            {0}
                            group by suppliercompany.id,suppliercompany.name,islrinvoicereturn,supplierpayment.id,
                            supplierpayment.paymentdate,bank.name,paymentmode,supplierpayment.receiptno,
                            supplierpayment.createdby,supplierpayment.referencedetail,bank.id,city.name;

                            select suppliercompany.id id,supplierpayment.receiptno,
							sum(tdsamount) tdsamount,sum(rounding) rounding,supplierpayment.id paymentid
                            from supplierpayment
                            inner join suppliercompany on suppliercompany.id = supplierpayment.suppliercompanyid
                            where supplierpayment.isactive=true  and supplierpayment.hascancel=false and supplierpayment.tdsoninv=false
                            and supplierpayment.paymentdate>=@fromdate and supplierpayment.paymentdate<@todate 
                            and supplierpayment.companyid=@companyid and supplierpayment.locationid=@locationid 
                            {0}
                            group by suppliercompany.id,supplierpayment.receiptno,supplierpayment.id";
            }
            SortedList<string, object> param = new SortedList<string, object>();
            param.Add("fromdate", from);
            param.Add("todate", to.AddDays(1));
            param.Add("companyid", companyid);
            param.Add("locationid", locationid);

            if (locationid <= 0)
                query = query.Replace("and supplierpayment.locationid=@locationid", "");
            query = string.Format(query, "");

            List<SupplierPaymentSummary> invoicelist = new List<SupplierPaymentSummary>();

            using (System.Data.IDataReader reader = ExecuteReader(query, param, 1000))
            {
                while (reader.Read())
                {
                    SupplierPaymentSummary invoice = new SupplierPaymentSummary();
                    invoice.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                    invoice.bankid = reader["bankid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["bankid"]);
                    if (isdetail)
                    {
                        invoice.refid = reader["paymentid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["paymentid"]);
                        invoice.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        invoice.receiptno = reader["receiptno"] == DBNull.Value ? "" : reader["receiptno"].ToString();
                        invoice.paymentdate = reader["paymentdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["paymentdate"]);
                        invoice.chequeno = reader["referencedetail"] == DBNull.Value ? "" : reader["referencedetail"].ToString();
                        invoice.invoiceno = reader["invoiceno"] == DBNull.Value ? "" : reader["invoiceno"].ToString();
                    }
                    invoice.suppliercompany = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
                    invoice.cityname = reader["cityname"] == DBNull.Value ? "" : reader["cityname"].ToString();
                    invoice.bankname = reader["bankname"] == DBNull.Value ? "" : reader["bankname"].ToString();

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
                    invoice.allocations = new List<SupplierPaymentSummary.Allocation>();
                    if (isdetail)
                    {
                        if (invoicelist.Exists(x => x.refid == invoice.refid))
                        {
                            invoice = invoicelist.Find(x => x.refid == invoice.refid);
                        }
                        else
                        {
                            invoicelist.Add(invoice);
                        }
                    }
                    else
                    {
                        if (invoicelist.Exists(x => x.id == invoice.id && x.bankid == invoice.bankid && x.paymentmode == invoice.paymentmode))
                        {
                            invoice = invoicelist.Find(x => x.id == invoice.id && x.bankid == invoice.bankid && x.paymentmode == invoice.paymentmode);
                        }
                        else
                        {
                            //invoice.tdsamount = reader["tdsamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["tdsamount"]);
                            //invoice.rounding = reader["rounding"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["rounding"]);
                            invoicelist.Add(invoice);
                        }
                    }

                    var isdebitnote = reader["islrinvoicereturn"] == DBNull.Value ? false : Convert.ToBoolean(reader["islrinvoicereturn"]);
                    if (invoice.allocations.Exists(x => x.isdebitnote == isdebitnote))
                    {
                        var allot = invoice.allocations.Find(x => x.isdebitnote == isdebitnote);
                        allot.paidamount += reader["paidamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["paidamount"]);
                        allot.agentcommission += reader["agentcommissionvalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["agentcommissionvalue"]);
                        allot.discountamount += reader["discountamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discountamount"]);
                    }
                    else
                    {
                        invoice.allocations.Add(new TallyDataservice.PaymentInfo.Allocation()
                        {
                            isdebitnote = reader["islrinvoicereturn"] == DBNull.Value ? false : Convert.ToBoolean(reader["islrinvoicereturn"]),
                            paidamount = reader["paidamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["paidamount"]),
                            agentcommission = reader["agentcommissionvalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["agentcommissionvalue"]),
                            discountamount = reader["discountamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discountamount"])
                        });
                    }
                }

                if (reader.NextResult())
                {
                    while (reader.Read())
                    {
                        if (isdetail)
                        {
                            var refid = reader["paymentid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["paymentid"]);
                            var id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                            if (invoicelist.Exists(x => x.refid == refid))
                            {
                                var invoice = invoicelist.Find(x => x.id == id);
                                invoice.tdsamount = reader["tdsamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["tdsamount"]);
                                invoice.rounding = reader["rounding"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["rounding"]);
                            }
                        }
                        else
                        {
                            var id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                            int paymentmode = reader["paymentmode"] == DBNull.Value ? 0 : (Int32)reader["paymentmode"];
                            long bankid = reader["bankid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["bankid"]);
                            string paymentname = "";
                            switch (paymentmode)
                            {
                                case 0:
                                    paymentname = "NEFT/RTGS";
                                    break;
                                case 1:
                                    paymentname = "Cheque/DD";
                                    break;
                                case 2:
                                    paymentname = "Cash";
                                    break;
                            }
                            if (invoicelist.Exists(x => x.id == id && x.bankid == bankid && x.paymentmode == paymentname))
                            {
                                var invoice = invoicelist.Find(x => x.id == id && x.bankid == bankid && x.paymentmode == paymentname);
                                invoice.tdsamount = reader["tdsamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["tdsamount"]);
                                invoice.rounding = reader["rounding"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["rounding"]);
                            }
                        }
                    }
                }
            }

            var allledgers = invoicelist.ToList();
            var bankledgers = GetLedgerbanks(allledgers.ConvertAll(x => x.bankid).ToList());
            bankledgers.ForEach(x =>
            {
                allledgers.FindAll(y => y.bankid == x.Key).ForEach(z =>
                {
                    z.bankname = x.Value;
                });
            });


            using (var reader = ExecuteReader(@"select suppliercompany.name from suppliercompany
                        group by suppliercompany.name
                        having count(*)>1", null))
            {
                List<string> names = new List<string>();
                while (reader.Read())
                {
                    names.Add(reader.IsDBNull(0) ? "" : reader.GetString(0));
                }
                invoicelist.FindAll(x => names.Contains(x.suppliercompany)).ForEach(y =>
                {
                    y.suppliercompany += "(" + y.cityname + ")";
                });
            }
            return new QuantoLedgers()
            {
                SupplierPayment = invoicelist
            };
        }
        public QuantoLedgers SearchSupplierPayment(long companyid, long locationid, int year)
        {
            var fromdate = new DateTime(year, 4, 1);
            var todate = new DateTime(year, 4, 1).AddYears(1);
            return SearchSupplierPayment(companyid, locationid, fromdate, todate);
        }
        public QuantoLedgers SearchSupplierPayment(long companyid, long locationid, DateTime from, DateTime to)
        {
            string query = @"
                            select supplierpayment.id,paymentdate,receiptno,suppliercompany.name,islrinvoicereturn,
                            supplierpayment.suppliercompanyid,tinanddate,cstanddate,supplierpayment.rounding,
                            suppliercompany.name as suppliercompany,bank.name as bankname,paymentmode,
                            state.name as statename,city.name as cityname,discountamount,hastds,tdsamount,tdspercentage,
                            supplierpayment.isinterstatesale isinterstatesale,bank.id bankid,
                            Supplierpaymentinvoice.invoiceno,Supplierpaymentinvoice.paidamount,paymentmode							
                            from supplierpayment
                            inner join Supplierpaymentinvoice on Supplierpaymentinvoice.supplierpaymentid = supplierpayment.id
                            inner join suppliercompany on suppliercompany.id = supplierpayment.suppliercompanyid
                            left outer join referencelist state on state.id = suppliercompany.stateid
                            left outer join referencelist city on city.id = suppliercompany.cityid
							left outer join referencelist bank on bank.id = supplierpayment.bankid
							where supplierpayment.isactive=true  and supplierpayment.hascancel=false 
                            and Supplierpaymentinvoice.isactive = true and supplierpaymentinvoice.hascancel=false
							and supplierpayment.paymentdate>=@fromdate and supplierpayment.paymentdate<@todate 
                            and supplierpayment.companyid=@companyid and supplierpayment.locationid=@locationid {0}";

            SortedList<string, object> param = new SortedList<string, object>();
            param.Add("fromdate", from);
            param.Add("todate", to.AddDays(1));
            param.Add("companyid", companyid);
            param.Add("locationid", locationid);

            if (locationid <= 0)
                query = query.Replace("and supplierpayment.locationid=@locationid", "");

            query = string.Format(query, "");

            SortedDictionary<long, SupplierPaymentSummary> invoicelist = new SortedDictionary<long, SupplierPaymentSummary>();

            /*var allsalesdata = ExecuteDataSet(query, param);
            using (System.IO.MemoryStream memoryStream = new System.IO.MemoryStream())
            {
                ClosedXML.Excel.XLWorkbook xLWorkbook = new ClosedXML.Excel.XLWorkbook();
                foreach (System.Data.DataTable table in allsalesdata.Tables)
                {
                    var sheet = xLWorkbook.AddWorksheet(table);
                }
                xLWorkbook.SaveAs(memoryStream);
                //System.IO.File.WriteAllBytes(@"e:\t.xlsx", memoryStream.ToArray());
            }*/

            using (System.Data.IDataReader reader = ExecuteReader(query, param, 1000))
            {
                while (reader.Read())
                {
                    SupplierPaymentSummary invoice = new SupplierPaymentSummary();
                    invoice.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                    invoice.bankid = reader["bankid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["bankid"]);
                    invoice.paymentdate = reader["paymentdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["paymentdate"]);
                    invoice.suppliercompany = reader["suppliercompany"] == DBNull.Value ? "" : reader["suppliercompany"].ToString();
                    invoice.statename = reader["statename"] == DBNull.Value ? "" : reader["statename"].ToString();
                    invoice.cityname = reader["cityname"] == DBNull.Value ? "" : reader["cityname"].ToString();
                    invoice.bankname = reader["bankname"] == DBNull.Value ? "" : reader["bankname"].ToString();
                    invoice.receiptno = reader["receiptno"] == DBNull.Value ? "" : reader["receiptno"].ToString();
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
                    invoice.allocations = new List<SupplierPaymentSummary.Allocation>();
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

                    invoice.allocations.Add(new TallyDataservice.PaymentInfo.Allocation()
                    {
                        isdebitnote = reader["islrinvoicereturn"] == DBNull.Value ? false : Convert.ToBoolean(reader["islrinvoicereturn"]),
                        invoiceno = reader["invoiceno"] == DBNull.Value ? "" : reader["invoiceno"].ToString(),
                        paidamount = reader["paidamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["paidamount"]),
                        discountamount = reader["discountamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discountamount"])
                    });
                }

            }

            var allledgers = invoicelist.Values.ToList();
            var bankledgers = GetLedgerbanks(allledgers.ConvertAll(x => x.bankid).ToList());
            bankledgers.ForEach(x =>
            {
                allledgers.FindAll(y => y.bankid == x.Key).ForEach(z =>
                {
                    z.bankname = x.Value;
                });
            });

            return new QuantoLedgers()
            {
                SupplierPayment = invoicelist.Values.ToList()
            };
        }
        public QuantoLedgers SearchSalesLedgerSummary(long companyid, long locationid, int year)
        {
            var fromdate = new DateTime(year, 4, 1);
            var todate = new DateTime(year, 4, 1).AddYears(1);
            return SearchSalesLedgerSummary(companyid, locationid, fromdate, todate);
        }
        public QuantoLedgers SearchSettlementLedgers(List<long> counterids, long locationid, DateTime from, DateTime to)
        {
            string query = @"select sum(paid_cash) cash,
                            sum(case when paid_cash=0 then cast(paymentinfo->>'cash_return' as numeric) else 0 end) cash_refund,
                            billsettlementmaster.counterid,billsettlementmaster.locationid,
                            billsettlementmaster.settlementon::date billdate,billsettlementmaster.companyid
                            from billsettlementmaster
                            where billsettlementmaster.isactive = true
                            and billsettlementmaster.settlementon >= @fromdate 
                            and billsettlementmaster.settlementon < @todate
                            and billsettlementmaster.counterid in ({0}) and billsettlementmaster.locationid=@locationid
                            group by billsettlementmaster.locationid,billsettlementmaster.counterid,
                            billsettlementmaster.settlementon::date,billsettlementmaster.companyid;

                            select sum(paid_card) card,cast(paymentinfo->>'card_bankid' as bigint) bankid,
                            billsettlementmaster.counterid,billsettlementmaster.locationid,
                            billsettlementmaster.settlementon::date billdate,billsettlementmaster.companyid
                            from billsettlementmaster
                            where billsettlementmaster.isactive = true
                            and billsettlementmaster.settlementon >= @fromdate 
                            and billsettlementmaster.settlementon < @todate
                            and paid_card!=0 and billsettlementmaster.counterid in ({0}) and billsettlementmaster.locationid=@locationid
                            group by billsettlementmaster.locationid,billsettlementmaster.counterid,
                            cast(paymentinfo->>'card_bankid' as bigint),billsettlementmaster.settlementon::date,billsettlementmaster.companyid;

                            select sum(paid_upi) upi_paid,cast(paymentinfo->>'upiproviderid' as bigint) upiproviderid,
                            billsettlementmaster.locationid,
                            billsettlementmaster.settlementon::date billdate,
                            billsettlementmaster.counterid,billsettlementmaster.companyid
                            from billsettlementmaster
                            where billsettlementmaster.isactive = true
                            and billsettlementmaster.settlementon >= @fromdate 
                            and billsettlementmaster.settlementon < @todate
                            and paid_upi != 0 and billsettlementmaster.counterid in ({0}) and billsettlementmaster.locationid=@locationid
                            group by billsettlementmaster.locationid,billsettlementmaster.counterid,
                            cast(paymentinfo->>'upiproviderid' as bigint),billsettlementmaster.settlementon::date,billsettlementmaster.companyid";

            SortedList<string, object> param = new SortedList<string, object>();
            param.Add("fromdate", from);
            param.Add("locationid", locationid);
            param.Add("todate", to.AddDays(1));

            if (locationid > 0)
            {
                query = query.Replace("and billsettlementmaster.counterid in ({0})", "");
            }
            query = string.Format(query, string.Join(",", counterids));

            Data.ReferencelistDataService locationdata = new Data.ReferencelistDataService();
            var locations = locationdata.Search(new ReferencelistCreteria()
            {
                type = "LOCATION"
            });

            var bankdata = locationdata.Search(new ReferencelistCreteria()
            {
                type = "BANK"
            });

            var counters = locationdata.Search(new ReferencelistCreteria()
            {
                type = "COUNTER"
            });

            var upiproviders = locationdata.Search(new ReferencelistCreteria()
            {
                type = "UPIPROVIDER"
            });

            var cardSalesList = new List<TallyDataservice.CardSales>();
            var cashSalesList = new List<TallyDataservice.CardSales>();

            using (System.Data.IDataReader reader = ExecuteReader(query, param, 1000))
            {

                while (reader.Read())
                {
                    CardSummary cashsales = new CardSummary();
                    cashsales.amount = reader["cash"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["cash"]);
                    cashsales.cash_refund = reader["cash_refund"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["cash_refund"]);
                    cashsales.locationid = reader["Locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["Locationid"]);
                    cashsales.companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"]);
                    cashsales.counterid = reader["CounterID"] == DBNull.Value ? 0 : Convert.ToInt64(reader["CounterID"]);
                    cashsales.date = reader["Billdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["Billdate"]);
                    cashsales.locationcode = locations.Find(e => e.id == cashsales.locationid).code;
                    cashsales.locationname = locations.Find(e => e.id == cashsales.locationid).name;

                    if (!counterids.Contains(cashsales.counterid) &&
                        counters.Exists(x => x.id == cashsales.counterid))
                    {
                        throw new Exception(counters.Find(x => x.id == cashsales.counterid).name + " Counter Opening Or Closing Missing");
                    }
                    cashSalesList.Add(cashsales);
                }

                if (reader.NextResult())
                {
                    while (reader.Read())
                    {
                        CardSummary cardSales = new CardSummary();
                        cardSales.bankid = reader["bankid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["bankid"]);
                        cardSales.amount = reader["card"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["card"]);
                        cardSales.companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"]);
                        cardSales.locationid = reader["Locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["Locationid"]);
                        cardSales.counterid = reader["CounterID"] == DBNull.Value ? 0 : Convert.ToInt64(reader["CounterID"]);

                        if (!counterids.Contains(cardSales.counterid) &&
                        counters.Exists(x => x.id == cardSales.counterid))
                        {
                            throw new Exception(counters.Find(x => x.id == cardSales.counterid).name + " Counter Opening Or Closing Missing");
                        }

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
                        cardSales.description = cardSales.bank;
                        cardSales.date = reader["Billdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["Billdate"]);
                        cardSales.locationcode = locations.Find(e => e.id == cardSales.locationid).code;
                        cardSales.locationname = locations.Find(e => e.id == cardSales.locationid).name;
                        cardSalesList.Add(cardSales);
                    }
                }

                if (reader.NextResult())
                {
                    while (reader.Read())
                    {
                        CardSummary cardSales = new CardSummary();
                        var providerid = reader["upiproviderid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["upiproviderid"]);
                        if (upiproviders.Exists(x => x.id == providerid))
                        {
                            var upi = upiproviders.Find(x => x.id == providerid);
                            cardSales.description = upi.name;
                            if (upi.additionalinfo != null)
                            {
                                cardSales.bankid = upi.additionalinfo.parentid;
                            }
                        }
                        cardSales.isupi = true;
                        cardSales.amount = reader["upi_paid"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["upi_paid"]);
                        cardSales.locationid = reader["Locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["Locationid"]);
                        cardSales.counterid = reader["CounterID"] == DBNull.Value ? 0 : Convert.ToInt64(reader["CounterID"]);
                        cardSales.companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"]);
                        if (!counterids.Contains(cardSales.counterid) &&
                            counters.Exists(x => x.id == cardSales.counterid))
                        {
                            throw new Exception(counters.Find(x => x.id == cardSales.counterid).name + " Counter Opening Or Closing Missing");
                        }
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
                        cardSales.date = reader["Billdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["Billdate"]);
                        cardSales.locationcode = locations.Find(e => e.id == cardSales.locationid).code;
                        cardSales.locationname = locations.Find(e => e.id == cardSales.locationid).name;
                        cardSalesList.Add(cardSales);
                    }
                }
            }

            var allledgers = cardSalesList.ToList();
            var bankledgers = GetLedgerbanks(allledgers.ConvertAll(x => x.bankid).ToList());
            bankledgers.ForEach(x =>
            {
                allledgers.FindAll(y => y.bankid == x.Key).ForEach(z =>
                {
                    z.bank = x.Value;
                });
            });

            var allcashledgers = cashSalesList.ToList();
            bankledgers = GetLedgerbanks(allledgers.ConvertAll(x => x.bankid).ToList());
            bankledgers.ForEach(x =>
            {
                allcashledgers.FindAll(y => y.bankid == x.Key).ForEach(z =>
                {
                    z.bank = x.Value;
                });
            });
            return new QuantoLedgers()
            {
                CardSalesList = cardSalesList,
                CashSalesList = cashSalesList
            };
        }
        public QuantoLedgers SearchSalesLedgers(long companyid, DateTime from, DateTime to)
        {
            /*             //voucher issue
                            select coupons.name, coupons.seqcode code,vouchervalue amount from coupons 
                            where coupons.isactive=true and entrytype=1 and coupons.createdon>=@fromdate and coupons.createdon<@todate 
                            and billsettlementmasterid=0 and coupons.counterid = @counterid and issuedby=@cashierid;

                            //consumption
                            select  vouchers.vouchertype,coupons.vouchervalue, coupons.name,
                            referencelist.name counter,vouchers.issuedon,billsettlement.billno,
                            vouchers.issueto billsettlementmasterid
                            from vouchers 
                            inner join coupons on coupons.id = vouchers.coupenid 
                            left outer join billsettlementmaster on billsettlementmaster.id = vouchers.issueto
                            left outer join billsettlement on billsettlement.billsettlementmasterid =billsettlementmaster.id
                            left outer join referencelist on referencelist.id = coupons.counterid
                            where coupons.isactive=True And vouchers.isactive=True And vouchers.vouchertype=4    
                            and vouchers.issuedon>=@fromdate And vouchers.issuedon<@todate 
                            and billsettlementmaster.companyid=@companyid;

                            //credit note
                            select vouchers.vouchertype,coupons.vouchervalue, coupons.name,
                            vouchers.issuedon,billsettlement.billno,coupons.billsettlementmasterid,
                            billitems.barcode,billitems.qty,billitems.receivable,billitems.tax,
                            lrinvoiceitems.itbuscode,referencelist.name counter
                            from vouchers 
                            inner join coupons on coupons.id = vouchers.coupenid 
                            inner join billsettlementmaster on billsettlementmaster.id = coupons.billsettlementmasterid
                            inner join billsettlement on billsettlement.billsettlementmasterid =billsettlementmaster.id
                            inner join billitems on billitems.billid = billsettlement.billid and billitems.isactive = true 
                            inner join stock on stock.id = billitems.stockid 
                            left outer join referencelist on referencelist.id = coupons.counterid
                            left outer join lrinvoiceitems on lrinvoiceitems.id =invoiceitemid
                            where coupons.isactive=True And vouchers.isactive=True And vouchers.vouchertype=7   
                            and vouchers.issuedon>=@fromdate And vouchers.issuedon<@todate 
                            and billsettlementmaster.companyid=@companyid

            */

            string query = @"Select bill.locationid,bill.counterid,bill.billdate::date billdate,sum(bill.receivable) amount,sum(rounding) rounding
                            from bill 
                            where bill.companyid=@companyid and bill.isactive = true and 
                            bill.billtype != 1 and bill.isb2b =false and bill.billtype != 9 and bill.billdate>=@fromdate and bill.billdate<@todate {0} {1}
                            group by bill.locationid,bill.counterid,bill.billdate::date;

                            Select customer.id customerid, customer.name as customername,bill.locationid,
                            bill.billdate::date billdate,sum(bill.receivable) amount,sum(rounding) rounding,
                            bill.billno,bill.createdby,bill.isinterstatesale
                            from bill 
                            left outer join customer on customer.id = bill.customerid
                            left outer join referencelist on referencelist.id = customer.cityid
                            where bill.companyid=@companyid and bill.isactive = true and bill.customerid >0 and 
                            bill.billtype = 1 and bill.isb2b =true and bill.billdate>=@fromdate and bill.billdate<@todate {0} {1}
                            group by bill.locationid,customer.name,customer.id,bill.billdate::date,bill.billno,bill.createdby,bill.isinterstatesale;

                            Select bill.locationid,bill.counterid,billitems.taxpercentage,sum(billitems.receivable) amount,
                            sum(billitems.tax) tax,bill.isinterstatesale,bill.billdate::date billdate
                            from bill 
                            inner join billitems on billitems.billid = bill.id and billitems.isactive=true
                            where bill.companyid=@companyid and bill.isactive = true and 
                            bill.billtype != 1 and bill.isb2b = false and bill.billtype != 9 and 
                            bill.billdate>=@fromdate and bill.billdate<@todate {0} {1}
                            group by bill.locationid,bill.counterid,billitems.taxpercentage,bill.isinterstatesale,bill.billdate::date;

                            Select customer.id customerid,customer.name as customername,bill.locationid,billitems.taxpercentage,sum(billitems.receivable) amount,
                            sum(billitems.tax) tax,bill.isinterstatesale,bill.billdate::date billdate,bill.billno,bill.createdby,bill.isinterstatesale
                            from bill 
                            inner join billitems on billitems.billid = bill.id and billitems.isactive=true
                            left outer join customer on customer.id = bill.customerid
                            where bill.companyid=@companyid and bill.isactive = true and 
                            bill.billtype = 1 and bill.isb2b =true and bill.billdate>=@fromdate and bill.billdate<@todate {0} {1}
                            group by customer.name,bill.locationid,billitems.taxpercentage,bill.billno,bill.isinterstatesale,customer.id,bill.billdate::date,bill.createdby,bill.isinterstatesale;
                            
                            Select customer.id customerid, customer.name as customername,bill.locationid,bill.billno,
                            sum(bill.receivable) amount,sum(rounding) rounding,bill.billdate::date billdate,bill.createdby,bill.isinterstatesale
                            from bill 
                            left outer join customer on customer.id = bill.customerid
                            left outer join referencelist on referencelist.id = customer.cityid
                            where bill.companyid=@companyid and bill.isactive = true and bill.customerid >0 and 
                            bill.billtype = 8 and bill.isb2b =true and bill.billdate>=@fromdate and
                            bill.billdate<@todate {0} {1}
                            group by bill.locationid,customer.name,customer.id,bill.billdate::date,bill.billno,bill.createdby,bill.isinterstatesale;

                            Select customer.id customerid,customer.name as customername,bill.locationid,
                            billitems.taxpercentage,sum(billitems.receivable) amount,bill.billno,
                            sum(billitems.tax) tax,bill.isinterstatesale,bill.billdate::date billdate,bill.createdby,bill.isinterstatesale
                            from bill 
                            inner join billitems on billitems.billid = bill.id and billitems.isactive=true
                            left outer join customer on customer.id = bill.customerid
                            where bill.companyid=@companyid and bill.isactive = true and 
                            bill.billtype = 8 and bill.isb2b =true and bill.billdate>=@fromdate and bill.billdate<@todate {0} {1}
                            group by customer.name,bill.locationid,billitems.taxpercentage,bill.isinterstatesale,customer.id,bill.billno,bill.billdate::date,bill.createdby,bill.isinterstatesale;

                            Select customer.id customerid, customer.name as customername,creditnoteno,
                            Billcreditnote.locationid,	sum(Billcreditnote.adjustedtotal) amount,
                            sum(Billcreditnote.rounding) rounding,Billcreditnote.creditnotedate::date billdate
                            from Billcreditnote 
                            left outer join customer on customer.id = Billcreditnote.customerid
                            left outer join referencelist on referencelist.id = customer.cityid
                            where Billcreditnote.companyid=@companyid and Billcreditnote.isactive = true and 
                            creditnotedate>=@fromdate and creditnotedate<@todate {0} {1}
                            group by Billcreditnote.locationid,customer.name,customer.id,
                            Billcreditnote.creditnotedate::date,creditnoteno;

                            Select customer.id customerid,customer.name as customername,
                            Billcreditnote.locationid,Billcreditnoteitems.taxpercentage,creditnoteno,
                            sum(Billcreditnoteitems.adjustedtotal) amount,Billcreditnote.creditnotedate::date billdate,
                            sum(Billcreditnoteitems.adjustedtax) tax,Billcreditnote.isinterstatesale
                            from Billcreditnote 
                            inner join Billcreditnoteitems on Billcreditnoteitems.billcreditnoteid = Billcreditnote.id and 
                            Billcreditnoteitems.isactive=true
                            inner join customer on customer.id = Billcreditnote.customerid
                            where Billcreditnote.companyid=@companyid and Billcreditnote.isactive = true and 
                            Billcreditnote.creditnotedate>=@fromdate and Billcreditnote.creditnotedate<@todate {0} {1}
                            group by customer.name,Billcreditnote.locationid,Billcreditnoteitems.taxpercentage,
                            Billcreditnote.isinterstatesale,customer.id,creditnoteno,Billcreditnote.creditnotedate::date;

                            select sum(paid_cash) cash,
                            billsettlementmaster.counterid,billsettlementmaster.locationid,
                            billsettlementmaster.settlementon::date billdate
                            from billsettlementmaster
                            where billsettlementmaster.isactive = true
                            and billsettlementmaster.settlementon >= @fromdate 
                            and billsettlementmaster.settlementon < @todate
                            and paid_cash!=0 
                            group by billsettlementmaster.locationid,billsettlementmaster.counterid,
                            billsettlementmaster.settlementon::date;

                            select sum(paid_card) card,cast(paymentinfo->>'card_bankid' as bigint) bankid,
                            billsettlementmaster.counterid,billsettlementmaster.locationid,
                            billsettlementmaster.settlementon::date billdate
                            from billsettlementmaster
                            where billsettlementmaster.isactive = true
                            and billsettlementmaster.settlementon >= @fromdate 
                            and billsettlementmaster.settlementon < @todate
                            and paid_card!=0 {2}
                            group by billsettlementmaster.locationid,billsettlementmaster.counterid,
                            cast(paymentinfo->>'card_bankid' as bigint),billsettlementmaster.settlementon::date;

                            select sum(paid_upi) upi_paid,upiproviderid,billsettlementmaster.locationid,
                            billsettlementmaster.settlementon::date billdate,
                            billsettlementmaster.counterid
                            from billsettlementmaster
                            where billsettlementmaster.isactive = true
                            and billsettlementmaster.settlementon >= @fromdate 
                            and billsettlementmaster.settlementon < @todate
                            and paid_upi != 0 {2}
                            group by billsettlementmaster.locationid,billsettlementmaster.counterid,
                            upiproviderid,billsettlementmaster.settlementon::date;

                            select abs(vouchers.amount) amount,settlementon date,locationid,CounterID
                            from billsettlementmaster 
                            inner join vouchers  on vouchers.issueto = billsettlementmaster.id and vouchers.vouchertype=4
                            where billsettlementmaster.isactive=true
							and billsettlementmaster.settlementon>= @fromdate
							and billsettlementmaster.settlementon< @todate;";

            SortedList<string, object> param = new SortedList<string, object>();
            param.Add("fromdate", from);
            param.Add("todate", to.AddDays(1));
            param.Add("companyid", companyid);
            string customercond = "";
            string locationcond = "";


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

            var counters = locationdata.Search(new ReferencelistCreteria()
            {
                type = "COUNTER"
            });

            var billList = new List<SalesSummary>();
            var taxlist = new List<TaxLedger>();
            var cardSalesList = new List<TallyDataservice.CardSales>();
            var cashSalesList = new List<TallyDataservice.CardSales>();
            var voucherSalesList = new List<TallyDataservice.CardSales>();

            using (System.Data.IDataReader reader = ExecuteReader(query, param, 1000))
            {
                while (reader.Read())
                {
                    SalesSummary invoice = new SalesSummary();
                    invoice.LocationID = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                    invoice.CounterID = reader["CounterID"] == DBNull.Value ? 0 : Convert.ToInt64(reader["CounterID"]);
                    invoice.Amount = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"]);
                    invoice.Rounding = reader["rounding"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["rounding"]);
                    invoice.Date = reader["Billdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["Billdate"]);
                    invoice.IsCredit = false;
                    invoice.Location = locations.Find(e => e.id == invoice.LocationID).code;
                    invoice.LocationName = locations.Find(e => e.id == invoice.LocationID).name;
                    invoice.Counter = counters.Find(e => e.id == invoice.CounterID).code;
                    invoice.CounterName = counters.Find(e => e.id == invoice.CounterID).name;
                    billList.Add(invoice);
                }

                if (reader.NextResult())
                {
                    while (reader.Read())
                    {
                        SalesSummary invoice = new SalesSummary();
                        invoice.LocationID = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                        invoice.Amount = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"]);
                        invoice.Rounding = reader["rounding"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["rounding"]);
                        invoice.CustomerName = reader["CustomerName"] == DBNull.Value ? null : reader["CustomerName"].ToString();
                        invoice.BillNo = reader["BillNo"] == DBNull.Value ? null : reader["BillNo"].ToString();
                        invoice.CreatedBy = reader["CreatedBy"] == DBNull.Value ? 0 : Convert.ToInt64(reader["CreatedBy"]);
                        invoice.IsInterstate = reader["isinterstatesale"] == DBNull.Value ? false : Convert.ToBoolean(reader["isinterstatesale"]);
                        invoice.CustomerID = reader["customerid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["customerid"]);
                        invoice.Location = locations.Find(e => e.id == invoice.LocationID).code;
                        invoice.LocationName = locations.Find(e => e.id == invoice.LocationID).name;
                        invoice.Date = reader["Billdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["Billdate"]);
                        invoice.IsCredit = true;
                        billList.Add(invoice);
                    }
                }

                if (reader.NextResult())
                {
                    while (reader.Read())
                    {
                        TaxLedger invoice = new TaxLedger();
                        invoice.LocationID = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                        //invoice.CounterID = reader["CounterID"] == DBNull.Value ? 0 : Convert.ToInt64(reader["CounterID"]);
                        //invoice.Counter = counters.Find(e => e.id == invoice.CounterID).code;
                        //invoice.CounterName = counters.Find(e => e.id == invoice.CounterID).name;
                        invoice.IsCredit = false;
                        invoice.Location = locations.Find(e => e.id == invoice.LocationID).code;
                        invoice.LocationName = locations.Find(e => e.id == invoice.LocationID).name;
                        invoice.taxpercentage = reader["taxpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["taxpercentage"]);
                        invoice.amount = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"]);
                        invoice.tax = reader["tax"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["tax"]);
                        invoice.date = reader["Billdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["Billdate"]);
                        invoice.IsInterstate = reader["isinterstatesale"] == DBNull.Value ? false : Convert.ToBoolean(reader["isinterstatesale"]);
                        taxlist.Add(invoice);
                    }
                }

                if (reader.NextResult())
                {
                    while (reader.Read())
                    {
                        TaxLedger invoice = new TaxLedger();
                        invoice.LocationID = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                        invoice.IsCredit = true;
                        invoice.Customer = reader["CustomerName"] == DBNull.Value ? null : reader["CustomerName"].ToString();
                        invoice.CustomerID = reader["customerid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["customerid"]);
                        invoice.Billno = reader["BillNo"] == DBNull.Value ? null : reader["BillNo"].ToString();
                        invoice.Location = locations.Find(e => e.id == invoice.LocationID).code;
                        invoice.LocationName = locations.Find(e => e.id == invoice.LocationID).name;
                        invoice.taxpercentage = reader["taxpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["taxpercentage"]);
                        invoice.amount = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"]);
                        invoice.tax = reader["tax"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["tax"]);
                        invoice.date = reader["Billdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["Billdate"]);
                        invoice.IsInterstate = reader["isinterstatesale"] == DBNull.Value ? false : Convert.ToBoolean(reader["isinterstatesale"]);
                        taxlist.Add(invoice);
                    }
                }

                if (reader.NextResult())
                {
                    while (reader.Read())
                    {
                        SalesSummary invoice = new SalesSummary();
                        invoice.LocationID = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                        invoice.Amount = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"]);
                        invoice.Rounding = reader["rounding"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["rounding"]);
                        invoice.CustomerName = reader["CustomerName"] == DBNull.Value ? null : reader["CustomerName"].ToString();
                        invoice.CustomerID = reader["customerid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["customerid"]);
                        invoice.CreatedBy = reader["CreatedBy"] == DBNull.Value ? 0 : Convert.ToInt64(reader["CreatedBy"]);
                        invoice.IsInterstate = reader["isinterstatesale"] == DBNull.Value ? false : Convert.ToBoolean(reader["isinterstatesale"]);
                        invoice.BillNo = reader["BillNo"] == DBNull.Value ? null : reader["BillNo"].ToString();
                        invoice.Location = locations.Find(e => e.id == invoice.LocationID).code;
                        invoice.LocationName = locations.Find(e => e.id == invoice.LocationID).name;
                        invoice.IsCreditNote = true;
                        invoice.Date = reader["Billdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["Billdate"]);
                        billList.Add(invoice);
                    }
                }

                if (reader.NextResult())
                {
                    while (reader.Read())
                    {
                        TaxLedger invoice = new TaxLedger();
                        invoice.LocationID = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                        invoice.IsCreditNote = true;
                        invoice.Customer = reader["CustomerName"] == DBNull.Value ? null : reader["CustomerName"].ToString();
                        invoice.Billno = reader["BillNo"] == DBNull.Value ? null : reader["BillNo"].ToString();
                        invoice.CustomerID = reader["customerid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["customerid"]);
                        invoice.Location = locations.Find(e => e.id == invoice.LocationID).code;
                        invoice.LocationName = locations.Find(e => e.id == invoice.LocationID).name;
                        invoice.taxpercentage = reader["taxpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["taxpercentage"]);
                        invoice.amount = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"]);
                        invoice.tax = reader["tax"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["tax"]);
                        invoice.date = reader["Billdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["Billdate"]);
                        invoice.IsInterstate = reader["isinterstatesale"] == DBNull.Value ? false : Convert.ToBoolean(reader["isinterstatesale"]);
                        taxlist.Add(invoice);
                    }
                }

                if (reader.NextResult())
                {
                    while (reader.Read())
                    {
                        SalesSummary invoice = new SalesSummary();
                        invoice.LocationID = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                        invoice.Amount = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"]);
                        invoice.Rounding = reader["rounding"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["rounding"]);
                        invoice.CustomerName = reader["CustomerName"] == DBNull.Value ? null : reader["CustomerName"].ToString();
                        invoice.BillNo = reader["BillNo"] == DBNull.Value ? null : reader["BillNo"].ToString();
                        invoice.CreatedBy = reader["CreatedBy"] == DBNull.Value ? 0 : Convert.ToInt64(reader["CreatedBy"]);
                        invoice.IsInterstate = reader["isinterstatesale"] == DBNull.Value ? false : Convert.ToBoolean(reader["isinterstatesale"]);
                        invoice.CustomerID = reader["customerid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["customerid"]);
                        invoice.CounterID = reader["CounterID"] == DBNull.Value ? 0 : Convert.ToInt64(reader["CounterID"]);
                        invoice.Location = locations.Find(e => e.id == invoice.LocationID).code;
                        invoice.LocationName = locations.Find(e => e.id == invoice.LocationID).name;
                        invoice.IsCreditNote = true;
                        invoice.Date = reader["Billdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["Billdate"]);
                        billList.Add(invoice);
                    }
                }

                if (reader.NextResult())
                {
                    while (reader.Read())
                    {
                        TaxLedger invoice = new TaxLedger();
                        invoice.LocationID = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                        invoice.IsCreditNote = true;
                        invoice.Customer = reader["CustomerName"] == DBNull.Value ? null : reader["CustomerName"].ToString();
                        invoice.Billno = reader["BillNo"] == DBNull.Value ? null : reader["BillNo"].ToString();
                        invoice.CustomerID = reader["customerid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["customerid"]);
                        invoice.Location = locations.Find(e => e.id == invoice.LocationID).code;
                        invoice.LocationName = locations.Find(e => e.id == invoice.LocationID).name;
                        invoice.taxpercentage = reader["taxpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["taxpercentage"]);
                        invoice.amount = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"]);
                        invoice.tax = reader["tax"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["tax"]);
                        invoice.IsInterstate = reader["isinterstatesale"] == DBNull.Value ? false : Convert.ToBoolean(reader["isinterstatesale"]);
                        invoice.date = reader["Billdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["Billdate"]);
                        taxlist.Add(invoice);
                    }
                }

                if (reader.NextResult())
                {
                    //sum(upi_paid) upi_paid,upiproviderid,billsettlementmaster.locationid
                    while (reader.Read())
                    {
                        CardSummary cashsales = new CardSummary();
                        cashsales.amount = reader["cash"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["cash"]);
                        cashsales.locationid = reader["Locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["Locationid"]);
                        cashsales.counterid = reader["CounterID"] == DBNull.Value ? 0 : Convert.ToInt64(reader["CounterID"]);
                        cashsales.date = reader["Billdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["Billdate"]);
                        cashsales.locationcode = locations.Find(e => e.id == cashsales.locationid).code;
                        cashsales.locationname = locations.Find(e => e.id == cashsales.locationid).name;
                        cashSalesList.Add(cashsales);
                    }
                }

                if (reader.NextResult())
                {
                    //sum(upi_paid) upi_paid,upiproviderid,billsettlementmaster.locationid
                    while (reader.Read())
                    {
                        CardSummary cardSales = new CardSummary();
                        cardSales.bankid = reader["bankid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["bankid"]);
                        cardSales.amount = reader["card"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["card"]);
                        cardSales.locationid = reader["Locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["Locationid"]);
                        cardSales.counterid = reader["CounterID"] == DBNull.Value ? 0 : Convert.ToInt64(reader["CounterID"]);
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
                        cardSales.date = reader["Billdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["Billdate"]);
                        cardSales.locationcode = locations.Find(e => e.id == cardSales.locationid).code;
                        cardSales.locationname = locations.Find(e => e.id == cardSales.locationid).name;
                        //cardSalesList.Add(cardSales);
                    }
                }

                if (reader.NextResult())
                {
                    while (reader.Read())
                    {
                        CardSummary cardSales = new CardSummary();
                        cardSales.bankid = reader["upiproviderid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["upiproviderid"]);
                        cardSales.amount = reader["upi_paid"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["upi_paid"]);
                        cardSales.locationid = reader["Locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["Locationid"]);
                        cardSales.counterid = reader["CounterID"] == DBNull.Value ? 0 : Convert.ToInt64(reader["CounterID"]);
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
                        cardSales.date = reader["Billdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["Billdate"]);
                        cardSales.locationcode = locations.Find(e => e.id == cardSales.locationid).code;
                        cardSales.locationname = locations.Find(e => e.id == cardSales.locationid).name;
                        //cardSalesList.Add(cardSales);
                    }
                }

                //if (reader.NextResult())
                //{
                //    while (reader.Read())
                //    {
                //        CardSummary cardSales = new CardSummary();
                //        cardSales.bankid = reader["upiproviderid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["upiproviderid"]);
                //        cardSales.amount = reader["upi_paid"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["upi_paid"]);
                //        cardSales.locationid = reader["Locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["Locationid"]);
                //        cardSales.CounterID = reader["CounterID"] == DBNull.Value ? 0 : Convert.ToInt64(reader["CounterID"]);
                //        if (bankdata.Exists(e => e.id == cardSales.bankid))
                //        {
                //            cardSales.bank = bankdata.Find(e => e.id == cardSales.bankid).name;
                //        }
                //        else if (bankdata.Count() == 1)
                //        {
                //            cardSales.bank = bankdata.First().name;
                //        }
                //        else
                //        {
                //            cardSales.bank = "DEFAULT BANK";
                //        }
                //        cardSales.date = reader["Billdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["Billdate"]);
                //        cardSales.locationcode = locations.Find(e => e.id == cardSales.locationid).code;
                //        cardSales.locationname = locations.Find(e => e.id == cardSales.locationid).name;
                //        //cardSalesList.Add(cardSales);
                //    }
                //}

                if (reader.NextResult())
                {
                    while (reader.Read())
                    {
                        CardSummary cardSales = new CardSummary();
                        cardSales.amount = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"]);
                        cardSales.locationid = reader["Locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["Locationid"]);
                        cardSales.counterid = reader["CounterID"] == DBNull.Value ? 0 : Convert.ToInt64(reader["CounterID"]);
                        cardSales.date = reader["date"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["date"]);
                        cardSales.locationcode = locations.Find(e => e.id == cardSales.locationid).code;
                        cardSales.locationname = locations.Find(e => e.id == cardSales.locationid).name;
                        voucherSalesList.Add(cardSales);
                    }
                }
            }
            return new QuantoLedgers()
            {
                CardSalesList = cardSalesList,
                CashSalesList = cashSalesList,
                VoucherSalesList = voucherSalesList,
                TaxList = taxlist,
                BillList = billList
            };
        }

        public QuantoLedgers SearchSalesLedgerSummary(long companyid, long locationid, DateTime from, DateTime to)
        {
            DateTime aviladate = DateTime.MinValue;
            QuantoLedgers quantoLedgers = null;
            using (var reader = ExecuteReader("select lastDateValue from Cache_View where  viewname='mv_fin_billitems_tax' limit 1", null))
            {
                if (reader.Read() && !reader.IsDBNull(0))
                    aviladate = reader.GetDateTime(0);
            }

            if (to <= aviladate)
            {
                quantoLedgers = SearchSalesLedgerSummaryCache(companyid, locationid, from, to);
            }
            else if (from > aviladate)
            {
                quantoLedgers = SearchSalesLedgerSummaryDirect(companyid, locationid, from, to);
            }
            else
            {
                quantoLedgers = SearchSalesLedgerSummaryCache(companyid, locationid, from, aviladate);
                var fromdirect = SearchSalesLedgerSummaryDirect(companyid, locationid, aviladate.AddDays(1), to);
                quantoLedgers.Merge(fromdirect);
            }


            List<long> customerids = new List<long>();
            if (quantoLedgers.TaxList != null &&
                quantoLedgers.TaxList.Exists(x => x.CustomerID > 0))
            {
                customerids.AddRange(quantoLedgers.TaxList.FindAll(x => x.CustomerID > 0).ConvertAll(y => y.CustomerID));
            }


            if (quantoLedgers.BillList != null &&
                quantoLedgers.BillList.Exists(x => x.CustomerID > 0))
            {
                customerids.AddRange(quantoLedgers.BillList.FindAll(x => x.CustomerID > 0).ConvertAll(y => y.CustomerID));
            }

            if (customerids.Count > 0)
            {
                using (var reader = ExecuteReader(string.Format(@"
                                    with duplicate as
                                    (
                                    select name from customer
                                    where isactive=true and customertype in (2, 7, 6,8)
                                    and customer.id in ({0})
                                    group by name
                                    having count(*)>1
                                    ) 
                                    select id,customer.name,customer.code from customer
                                    inner join duplicate on duplicate.name = customer.name
                                    where isactive=true and customertype in (2, 7, 6,8)
                                    order by customer.name", string.Join(",", customerids)), null,10000))
                {
                    while (reader.Read())
                    {
                        long id = reader.GetInt64(0);
                        string sname = reader.IsDBNull(1) ? "" : reader.GetString(1);
                        string cname = reader.IsDBNull(2) ? "" : reader.GetString(2);
                        if (quantoLedgers.TaxList.Exists(x => x.CustomerID == id))
                        {
                            quantoLedgers.TaxList.FindAll(x => x.CustomerID == id).ForEach(y =>
                            {
                                y.Customer = sname + "(" + cname + ")";
                            });
                        }
                        if (quantoLedgers.BillList.Exists(x => x.CustomerID == id))
                        {
                            quantoLedgers.BillList.FindAll(x => x.CustomerID == id).ForEach(y =>
                            {
                                y.CustomerName = sname + "(" + cname + ")";
                            });
                        }
                    }
                }
            }

            return quantoLedgers;
        }
        public QuantoLedgers SearchSalesLedgerSummaryCache(long companyid, long locationid, DateTime from, DateTime to)
        {
            string query = @"Select bill.locationid,bill.counterid,sum(bill.amount) amount,
                            sum(bill.netamount) netamount,sum(rounding) rounding
                            from mv_fin_bill bill 
                            where bill.companyid=@companyid and bill.locationid=@locationid 
                            and bill.isb2b =false and bill.iscreditbill = false and bill.billtype != 9 
                            and bill.billdate>=@fromdate and bill.billdate<@todate {0} {1}
                            group by bill.locationid,bill.counterid;

                            Select customer.id customerid, customer.name as customername,bill.locationid,
                            sum(bill.amount) amount,sum(rounding) rounding
                            from mv_fin_bill bill 
                            left outer join customer on customer.id = bill.customerid
                            left outer join referencelist on referencelist.id = customer.cityid
                            where bill.companyid=@companyid and bill.locationid=@locationid  and bill.customerid >0 and 
                            bill.isb2b =true and billtype!=8 and billtype!=9 and bill.billdate>=@fromdate and bill.billdate<@todate {0} {1}
                            group by bill.locationid,customer.name,customer.id;

                            Select bill.locationid,bill.taxpercentage,sum(bill.amount) amount,
                            sum(bill.tax) tax,bill.isinterstatesale
                            from mv_fin_billitems_tax bill 
                            where bill.companyid=@companyid and bill.locationid=@locationid  and 
                            bill.isb2b = false and bill.iscreditbill = false and bill.billtype != 9 and bill.billdate>=@fromdate and bill.billdate<@todate {0} {1}
                            group by bill.locationid,bill.taxpercentage,bill.isinterstatesale;

                            Select customer.id customerid,customer.name as customername,bill.locationid,bill.taxpercentage,
                            sum(bill.amount) amount,sum(bill.tax) tax,bill.isinterstatesale
                            from mv_fin_billitems_tax bill 
                            left outer join customer on customer.id = bill.customerid
                            where bill.companyid=@companyid and bill.locationid=@locationid and 
                            bill.isb2b =true and bill.billtype!=8 and bill.billtype!=9 and bill.billdate>=@fromdate and bill.billdate<@todate {0} {1}
                            group by customer.name,bill.locationid,bill.taxpercentage,bill.isinterstatesale,customer.id;
                            
                            Select customer.id customerid, customer.name as customername,bill.locationid,
                            sum(bill.amount) amount,sum(rounding) rounding
                            from mv_fin_bill bill 
                            left outer join customer on customer.id = bill.customerid
                            left outer join referencelist on referencelist.id = customer.cityid
                            where bill.companyid=@companyid and bill.locationid=@locationid and bill.customerid >0 and 
                            bill.billtype = 8 and bill.isb2b =true and bill.billdate>=@fromdate and bill.billdate<@todate {0} {1}
                            group by bill.locationid,customer.name,customer.id;

                            Select customer.id customerid,customer.name as customername,bill.locationid,bill.taxpercentage,sum(bill.amount) amount,
                            sum(bill.tax) tax,bill.isinterstatesale
                            from mv_fin_billitems_tax bill 
                            left outer join customer on customer.id = bill.customerid
                            where bill.companyid=@companyid and bill.locationid=@locationid  and bill.customerid >0 and  
                            bill.billtype = 8 and bill.isb2b =true and bill.billdate>=@fromdate and bill.billdate<@todate {0} {1}
                            group by customer.name,bill.locationid,bill.taxpercentage,bill.isinterstatesale,customer.id;

                            Select customer.id customerid, customer.name as customername,bill.locationid,
                            sum(bill.netamount) amount,sum(rounding) rounding
                            from mv_fin_bill bill 
                            left outer join customer on customer.id = bill.customerid
                            left outer join referencelist on referencelist.id = customer.cityid
                            where bill.companyid=@companyid and bill.locationid=@locationid and 
                            bill.billtype != 8 and bill.billtype != 9 and bill.isb2b =false and 
                            bill.iscreditbill=true and bill.billdate>=@fromdate and bill.billdate<@todate {0} {1}
                            group by bill.locationid,customer.name,customer.id;

                            Select customer.id customerid, customer.name as customername,bill.locationid,
                            sum(bill.netamount) amount,sum(rounding) rounding
                            from mv_fin_bill bill 
                            left outer join customer on customer.id = bill.customerid
                            left outer join referencelist on referencelist.id = customer.cityid
                            where bill.companyid=@companyid and bill.locationid=@locationid and 
                            bill.billtype = 8 and bill.isb2b =false and 
                            bill.iscreditbill=true and bill.billdate>=@fromdate and bill.billdate<@todate {0} {1}
                            group by bill.locationid,customer.name,customer.id;

                            Select customer.id customerid,customer.name as customername,bill.locationid,bill.taxpercentage,sum(bill.amount) amount,
                            sum(bill.tax) tax,bill.isinterstatesale
                            from mv_fin_billitems_tax bill 
                            left outer join customer on customer.id = bill.customerid
                            where bill.companyid=@companyid and bill.locationid=@locationid and 
                            bill.billtype != 9 and bill.isb2b =false and 
                            bill.iscreditbill=true and bill.billdate>=@fromdate and bill.billdate<@todate {0} {1}
                            group by customer.name,bill.locationid,bill.taxpercentage,bill.isinterstatesale,customer.id;

                            Select customer.id customerid, customer.name as customername,
                            Billcreditnote.locationid,	sum(Billcreditnote.adjustedtotal) amount,
                            sum(Billcreditnote.rounding) rounding
                            from Billcreditnote 
                            left outer join customer on customer.id = Billcreditnote.customerid
                            left outer join referencelist on referencelist.id = customer.cityid
                            where Billcreditnote.companyid=@companyid and Billcreditnote.locationid=@locationid and Billcreditnote.isactive = true and 
                            creditnotedate>=@fromdate and creditnotedate<@todate {0} {1}
                            group by Billcreditnote.locationid,customer.name,customer.id;

                            Select customer.id customerid,customer.name as customername,
                            Billcreditnote.locationid,Billcreditnoteitems.taxpercentage,
                            sum(Billcreditnoteitems.adjustedtotal) amount,
                            sum(Billcreditnoteitems.adjustedtax) tax,Billcreditnote.isinterstatesale
                            from Billcreditnote 
                            inner join Billcreditnoteitems on Billcreditnoteitems.billcreditnoteid = Billcreditnote.id and 
                            Billcreditnoteitems.isactive=true
                            inner join customer on customer.id = Billcreditnote.customerid
                            where Billcreditnote.companyid=@companyid and Billcreditnote.locationid=@locationid and Billcreditnote.isactive = true and 
                            Billcreditnote.creditnotedate>=@fromdate and Billcreditnote.creditnotedate<@todate {0} {1}
                            group by customer.name,Billcreditnote.locationid,Billcreditnoteitems.taxpercentage,
                            Billcreditnote.isinterstatesale,customer.id;

                            select sum(voucher) voucher,
                            billsettlementmaster.locationid,billsettlementmaster.counterid,billsettlementmaster.customerid
                            from mv_Fin_Billsettlement_Voucher billsettlementmaster
                            where billsettlementmaster.companyid=@companyid and billsettlementmaster.locationid=@locationid
                            and billsettlementmaster.settlementon >= @fromdate 
                            and billsettlementmaster.settlementon < @todate {2}
                            group by billsettlementmaster.customerid,billsettlementmaster.locationid,billsettlementmaster.counterid;

                            select customer.name customer,bill.locationid,bill.counterid,
                            referencelist.name chargename,billadditionalcharges.charge,
                            billadditionalcharges.tax,bill.isinterstatesale,
                            billadditionalcharges.taxpercentage from bill
                            inner join billadditionalcharges on billadditionalcharges.billid = bill.id and billadditionalcharges.isactive=true
                            inner join referencelist on referencelist.id = additionalchargestypeid
                            left outer join customer on customer.id = customerid
                            where 
                            bill.companyid=@companyid and bill.locationid=@locationid and bill.isactive = true and 
                            bill.billdate>=@fromdate and bill.billdate<@todate and bill.billtype != 9 AND bill.isb2b=true
                            and taxonly=false;

                            Select sum(bill.receivable) amount
                            from bill 
                            where bill.isactive = true 
                            and bill.isb2b =false and bill.billtype != 9 
							and iscreditbill=false
                            and bill.companyid=@companyid 
                            and bill.locationid=@locationid 
                            and bill.billdate>=@fromdate and bill.billdate<@todate
							and settled=false;                            

                            with b2bsettlement as
                            (
                                select distinct bill.customerid,billsettlementmaster.id
                                from billsettlementmaster
							    inner join billsettlement on billsettlement.billsettlementmasterid = billsettlementmaster.id
							    inner join bill on bill.id =billid
                                where billsettlementmaster.isactive = true
                                and bill.companyid=@companyid and bill.locationid=@locationid
                                and billsettlementmaster.settlementon >= @fromdate
                                and billsettlementmaster.settlementon < @todate
                                and bill.isb2b=true
                            )
                            select customer.name,
                            sum(paid_cash) cash,sum(paid_card) card,
                            sum(paid_upi) upi_paid,sum(cast(paymentinfo->>'voucher_paid' as numeric)) voucher_paid,
                            sum(case when paid_cash=0 then cast(paymentinfo->>'cash_return' as numeric) else 0 end) cash_refund from b2bsettlement
                            inner join billsettlementmaster on billsettlementmaster.id = b2bsettlement.id
                            inner join customer on customer.id = b2bsettlement.customerid
                            group by customer.name;

                            with tsettlement as (							
                            select distinct bill.isb2b,bill.iscreditbill,billsettlementmaster.id
                                                        from billsettlementmaster
							                            inner join billsettlement on billsettlement.billsettlementmasterid=billsettlementmaster.id
							                            inner join bill on bill.id = billsettlement.billid
                                                        where billsettlementmaster.isactive = true
							                            and bill.isb2b=false and bill.iscreditbill=false
							                            and paid_credit>0
                                                        and billsettlementmaster.companyid=@companyid and billsettlementmaster.locationid=@locationid
                                                        and billsettlementmaster.settlementon >= @fromdate
                                                        and billsettlementmaster.settlementon < @todate
                            )

                            select sum(paid_credit) amount
                            from billsettlementmaster
							inner join tsettlement on tsettlement.id = billsettlementmaster.id";


            SortedList<string, object> param = new SortedList<string, object>();
            param.Add("fromdate", from);
            param.Add("todate", to.AddDays(1));
            param.Add("companyid", companyid);
            param.Add("locationid", locationid);
            string customercond = "";
            string locationcond = "";

            if (locationid <= 0)
            {
                query = query.Replace("and Billcreditnote.locationid=@locationid", "");
                query = query.Replace("and billsettlementmaster.locationid=@locationid", "");
                query = query.Replace("and bill.locationid=@locationid", "");
            }
            query = string.Format(query, locationcond, customercond, locationcond.Replace("bill.", "billsettlementmaster."));



            Data.ReferencelistDataService locationdata = new Data.ReferencelistDataService();
            var locations = locationdata.Search(new ReferencelistCreteria()
            {
                type = "LOCATION",
                inactive = true
            });

            var bankdata = locationdata.Search(new ReferencelistCreteria()
            {
                type = "BANK",
                inactive = true
            });


            var counters = locationdata.Search(new ReferencelistCreteria()
            {
                type = "COUNTER",
                inactive = true
            });
            var billList = new List<SalesSummary>();
            var taxlist = new List<TaxLedger>();
            var additionallist = new List<AdditionalCharges>();
            var cardSalesList = new List<TallyDataservice.CardSales>();
            var voucherSalesList = new List<TallyDataservice.CardSales>();
            var b2bSalesList = new List<TallyDataservice.CardSales>();
            var unsettledbills = new List<TallyDataservice.CardSales>();
            /*var allsalesdata = ExecuteDataSet(query, param);
            using (System.IO.MemoryStream memoryStream = new System.IO.MemoryStream())
            {
                ClosedXML.Excel.XLWorkbook xLWorkbook = new ClosedXML.Excel.XLWorkbook();
                foreach (System.Data.DataTable table in allsalesdata.Tables)
                {
                    var sheet = xLWorkbook.AddWorksheet(table);
                }
                xLWorkbook.SaveAs(memoryStream);
                //System.IO.File.WriteAllBytes(@"e:\t.xlsx", memoryStream.ToArray());
            }*/

            using (System.Data.IDataReader reader = ExecuteReader(query, param, 1000))
            {
                //0
                while (reader.Read())
                {
                    SalesSummary invoice = new SalesSummary();
                    invoice.LocationID = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                    invoice.CounterID = reader["CounterID"] == DBNull.Value ? 0 : Convert.ToInt64(reader["CounterID"]);
                    invoice.Amount = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"]);
                    invoice.Rounding = reader["rounding"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["rounding"]);
                    invoice.IsCredit = false;
                    invoice.Location = locations.Find(e => e.id == invoice.LocationID).code;
                    invoice.LocationName = locations.Find(e => e.id == invoice.LocationID).name;
                    invoice.Counter = counters.Find(e => e.id == invoice.CounterID).code;
                    invoice.CounterName = counters.Find(e => e.id == invoice.CounterID).name;
                    billList.Add(invoice);
                }

                //1
                if (reader.NextResult())
                {
                    while (reader.Read())
                    {
                        SalesSummary invoice = new SalesSummary();
                        invoice.LocationID = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                        invoice.Amount = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"]);
                        invoice.Rounding = reader["rounding"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["rounding"]);
                        invoice.CustomerName = reader["CustomerName"] == DBNull.Value ? null : reader["CustomerName"].ToString();
                        invoice.CustomerID = reader["customerid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["customerid"]);
                        invoice.Location = locations.Find(e => e.id == invoice.LocationID).code;
                        invoice.LocationName = locations.Find(e => e.id == invoice.LocationID).name;
                        invoice.IsCredit = true;
                        billList.Add(invoice);
                    }
                }

                //2
                if (reader.NextResult())
                {
                    while (reader.Read())
                    {
                        TaxLedger invoice = new TaxLedger();
                        invoice.LocationID = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                        invoice.IsCredit = false;
                        invoice.Location = locations.Find(e => e.id == invoice.LocationID).code;
                        invoice.LocationName = locations.Find(e => e.id == invoice.LocationID).name;
                        //invoice.CounterID = reader["CounterID"] == DBNull.Value ? 0 : Convert.ToInt64(reader["CounterID"]);
                        //invoice.Counter = counters.Find(e => e.id == invoice.CounterID).code;
                        //invoice.CounterName = counters.Find(e => e.id == invoice.CounterID).name;
                        invoice.taxpercentage = reader["taxpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["taxpercentage"]);
                        invoice.amount = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"]);
                        invoice.tax = reader["tax"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["tax"]);
                        invoice.IsInterstate = reader["isinterstatesale"] == DBNull.Value ? false : Convert.ToBoolean(reader["isinterstatesale"]);
                        taxlist.Add(invoice);
                    }
                }

                //3
                if (reader.NextResult())
                {
                    while (reader.Read())
                    {
                        TaxLedger invoice = new TaxLedger();
                        invoice.LocationID = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                        invoice.IsCredit = true;
                        invoice.Customer = reader["CustomerName"] == DBNull.Value ? null : reader["CustomerName"].ToString();
                        invoice.CustomerID = reader["customerid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["customerid"]);
                        invoice.Location = locations.Find(e => e.id == invoice.LocationID).code;
                        invoice.LocationName = locations.Find(e => e.id == invoice.LocationID).name;
                        invoice.taxpercentage = reader["taxpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["taxpercentage"]);
                        invoice.amount = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"]);
                        invoice.tax = reader["tax"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["tax"]);
                        invoice.IsInterstate = reader["isinterstatesale"] == DBNull.Value ? false : Convert.ToBoolean(reader["isinterstatesale"]);
                        taxlist.Add(invoice);
                    }
                }

                //4
                if (reader.NextResult())
                {
                    while (reader.Read())
                    {
                        SalesSummary invoice = new SalesSummary();
                        invoice.LocationID = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                        invoice.Amount = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"]);
                        invoice.Rounding = reader["rounding"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["rounding"]);
                        invoice.CustomerName = reader["CustomerName"] == DBNull.Value ? null : reader["CustomerName"].ToString();
                        invoice.CustomerID = reader["customerid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["customerid"]);
                        invoice.Location = locations.Find(e => e.id == invoice.LocationID).code;
                        invoice.LocationName = locations.Find(e => e.id == invoice.LocationID).name;
                        invoice.IsCreditNote = true;
                        billList.Add(invoice);
                    }
                }

                //5
                if (reader.NextResult())
                {
                    while (reader.Read())
                    {
                        TaxLedger invoice = new TaxLedger();
                        invoice.LocationID = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                        invoice.IsCreditNote = true;
                        invoice.Customer = reader["CustomerName"] == DBNull.Value ? null : reader["CustomerName"].ToString();
                        invoice.CustomerID = reader["customerid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["customerid"]);
                        invoice.Location = locations.Find(e => e.id == invoice.LocationID).code;
                        invoice.LocationName = locations.Find(e => e.id == invoice.LocationID).name;
                        invoice.taxpercentage = reader["taxpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["taxpercentage"]);
                        invoice.amount = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"]);
                        invoice.tax = reader["tax"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["tax"]);
                        invoice.IsInterstate = reader["isinterstatesale"] == DBNull.Value ? false : Convert.ToBoolean(reader["isinterstatesale"]);
                        taxlist.Add(invoice);
                    }
                }


                //Select customer.id customerid, customer.name as customername,
                //            Billcreditnote.locationid,	sum(Billcreditnote.adjustedtotal) amount,
                //            sum(Billcreditnote.rounding) rounding

                //6

                if (reader.NextResult())
                {
                    while (reader.Read())
                    {
                        SalesSummary invoice = new SalesSummary();
                        invoice.LocationID = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                        invoice.Amount = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"]);
                        invoice.Rounding = reader["rounding"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["rounding"]);
                        invoice.CustomerName = reader["CustomerName"] == DBNull.Value ? null : reader["CustomerName"].ToString();
                        invoice.CustomerID = reader["customerid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["customerid"]);
                        invoice.Location = locations.Find(e => e.id == invoice.LocationID).code;
                        invoice.LocationName = locations.Find(e => e.id == invoice.LocationID).name;
                        invoice.IsCredit = true;
                        billList.Add(invoice);
                    }
                }


                //7
                if (reader.NextResult())
                {
                    while (reader.Read())
                    {
                        SalesSummary invoice = new SalesSummary();
                        invoice.LocationID = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                        invoice.Amount = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"]);
                        invoice.Rounding = reader["rounding"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["rounding"]);
                        invoice.CustomerName = reader["CustomerName"] == DBNull.Value ? null : reader["CustomerName"].ToString();
                        invoice.CustomerID = reader["customerid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["customerid"]);
                        invoice.Location = locations.Find(e => e.id == invoice.LocationID).code;
                        invoice.LocationName = locations.Find(e => e.id == invoice.LocationID).name;
                        invoice.IsCreditNote = true;
                        billList.Add(invoice);
                    }
                }

                //8
                if (reader.NextResult())
                {
                    while (reader.Read())
                    {
                        TaxLedger invoice = new TaxLedger();
                        invoice.LocationID = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                        invoice.IsCredit = true;
                        invoice.Customer = reader["CustomerName"] == DBNull.Value ? null : reader["CustomerName"].ToString();
                        invoice.CustomerID = reader["customerid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["customerid"]);
                        invoice.Location = locations.Find(e => e.id == invoice.LocationID).code;
                        invoice.LocationName = locations.Find(e => e.id == invoice.LocationID).name;
                        invoice.taxpercentage = reader["taxpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["taxpercentage"]);
                        invoice.amount = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"]);
                        invoice.tax = reader["tax"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["tax"]);
                        invoice.IsInterstate = reader["isinterstatesale"] == DBNull.Value ? false : Convert.ToBoolean(reader["isinterstatesale"]);
                        taxlist.Add(invoice);
                    }
                }



                if (reader.NextResult())
                {
                    while (reader.Read())
                    {
                        SalesSummary invoice = new SalesSummary();
                        invoice.LocationID = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                        invoice.Amount = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"]);
                        invoice.Rounding = reader["rounding"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["rounding"]);
                        invoice.CustomerName = reader["CustomerName"] == DBNull.Value ? null : reader["CustomerName"].ToString();
                        invoice.CustomerID = reader["customerid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["customerid"]);
                        invoice.Location = locations.Find(e => e.id == invoice.LocationID).code;
                        invoice.LocationName = locations.Find(e => e.id == invoice.LocationID).name;
                        invoice.IsCreditNote = true;
                        billList.Add(invoice);
                    }
                }

                if (reader.NextResult())
                {
                    while (reader.Read())
                    {
                        TaxLedger invoice = new TaxLedger();
                        invoice.LocationID = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                        invoice.IsCreditNote = true;
                        invoice.Customer = reader["CustomerName"] == DBNull.Value ? null : reader["CustomerName"].ToString();
                        invoice.CustomerID = reader["customerid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["customerid"]);
                        invoice.Location = locations.Find(e => e.id == invoice.LocationID).code;
                        invoice.LocationName = locations.Find(e => e.id == invoice.LocationID).name;
                        invoice.taxpercentage = reader["taxpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["taxpercentage"]);
                        invoice.amount = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"]);
                        invoice.tax = reader["tax"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["tax"]);
                        invoice.IsInterstate = reader["isinterstatesale"] == DBNull.Value ? false : Convert.ToBoolean(reader["isinterstatesale"]);
                        taxlist.Add(invoice);
                    }
                }

                //if (reader.NextResult())
                //{
                //    //sum(upi_paid) upi_paid,upiproviderid,billsettlementmaster.locationid
                //    while (reader.Read())
                //    {
                //        CardSummary cardSales = new CardSummary();
                //        cardSales.refid = reader["customerid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["customerid"]);
                //        cardSales.bankid = reader.IsDBNull(1) ? 0 : reader.GetInt64(1);
                //        cardSales.amount = reader.GetDecimal(0);
                //        cardSales.locationid = reader.GetInt64(2);
                //        cardSales.CounterID = reader["CounterID"] == DBNull.Value ? 0 : Convert.ToInt64(reader["CounterID"]);
                //        if (bankdata.Exists(e => e.id == cardSales.bankid))
                //        {
                //            cardSales.bank = bankdata.Find(e => e.id == cardSales.bankid).name;
                //        }
                //        else if (bankdata.Count() == 1)
                //        {
                //            cardSales.bank = bankdata.First().name;
                //        }
                //        else
                //        {
                //            cardSales.bank = "DEFAULT BANK";
                //        }
                //        //cardSalesList.Add(cardSales);
                //    }
                //}

                //if (reader.NextResult())
                //{
                //    while (reader.Read())
                //    {
                //        CardSummary cardSales = new CardSummary();
                //        cardSales.refid = reader["customerid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["customerid"]);
                //        cardSales.bankid = reader.IsDBNull(1) ? 0 : reader.GetInt64(1);
                //        cardSales.amount = reader.GetDecimal(0);
                //        cardSales.locationid = reader.GetInt64(2);
                //        cardSales.CounterID = reader["CounterID"] == DBNull.Value ? 0 : Convert.ToInt64(reader["CounterID"]);
                //        if (bankdata.Exists(e => e.id == cardSales.bankid))
                //        {
                //            cardSales.bank = bankdata.Find(e => e.id == cardSales.bankid).name;
                //        }
                //        else if (bankdata.Count() == 1)
                //        {
                //            cardSales.bank = bankdata.First().name;
                //        }
                //        else
                //        {
                //            cardSales.bank = "DEFAULT BANK";
                //        }
                //        //cardSalesList.Add(cardSales);
                //    }
                //}

                if (reader.NextResult())
                {
                    while (reader.Read())
                    {
                        CardSummary cardSales = new CardSummary();
                        cardSales.amount = reader["voucher"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["voucher"]);
                        cardSales.locationid = reader["Locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["Locationid"]);
                        cardSales.counterid = reader["CounterID"] == DBNull.Value ? 0 : Convert.ToInt64(reader["CounterID"]);
                        //cardSales.date = reader["Billdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["Billdate"]);
                        cardSales.locationcode = locations.Find(e => e.id == cardSales.locationid).code;
                        cardSales.locationname = locations.Find(e => e.id == cardSales.locationid).name;
                        voucherSalesList.Add(cardSales);
                    }
                }

                if (reader.NextResult())
                {
                    while (reader.Read())
                    {
                        AdditionalCharges cardSales = new AdditionalCharges();
                        cardSales.chargename = reader["chargename"] == DBNull.Value ? "" : Convert.ToString(reader["chargename"]);
                        cardSales.customer = reader["customer"] == DBNull.Value ? "" : Convert.ToString(reader["customer"]);
                        cardSales.charge = reader["charge"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["charge"]);
                        cardSales.tax = reader["tax"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["tax"]);
                        cardSales.taxpercentage = reader["taxpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["taxpercentage"]);
                        cardSales.LocationID = reader["Locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["Locationid"]);
                        cardSales.CounterID = reader["CounterID"] == DBNull.Value ? 0 : Convert.ToInt64(reader["CounterID"]);
                        cardSales.IsInterstate = reader["isinterstatesale"] == DBNull.Value ? false : Convert.ToBoolean(reader["isinterstatesale"]);
                        cardSales.LocationCode = locations.Find(e => e.id == cardSales.LocationID).code;
                        cardSales.LocationName = locations.Find(e => e.id == cardSales.LocationID).name;
                        cardSales.CounterCode = counters.Find(e => e.id == cardSales.CounterID).code;
                        cardSales.CounterName = counters.Find(e => e.id == cardSales.CounterID).name;
                        additionallist.Add(cardSales);
                    }
                }


                if (reader.NextResult())
                {
                    if (reader.Read())
                    {
                        CardSummary cardSales = new CardSummary();
                        cardSales.amount = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"]);
                        cardSales.isupi = false;
                        unsettledbills.Add(cardSales);
                    }
                }

                if (reader.NextResult())
                {
                    while (reader.Read())
                    {
                        CardSummary cardSales = new CardSummary();
                        cardSales.amount = reader["cash"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["cash"]);
                        cardSales.amount += reader["card"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["card"]);
                        cardSales.amount += reader["upi_paid"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["upi_paid"]);
                        cardSales.bank = reader["name"] == DBNull.Value ? "CUSTOMER" : reader["name"].ToString();
                        b2bSalesList.Add(cardSales);
                    }
                }

                if (reader.NextResult())
                {
                    if (reader.Read())
                    {
                        CardSummary cardSales = new CardSummary();
                        cardSales.amount = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"]);
                        cardSales.isupi = true;
                        unsettledbills.Add(cardSales);
                    }
                }

            }
            return new QuantoLedgers()
            {
                CardSalesList = cardSalesList,
                TaxList = taxlist,
                VoucherSalesList = voucherSalesList,
                BillList = billList,
                Unsettled = unsettledbills,
                B2BSalesList = b2bSalesList,
                AdditionalCharges = additionallist
            };
        }
        public QuantoLedgers SearchSalesLedgerSummaryDirect(long companyid, long locationid, DateTime from, DateTime to)
        {
            string query = @"Select bill.locationid,bill.counterid,sum(bill.receivable-bill.additionalcharges) amount,
                            sum(bill.receivable) netamount,sum(rounding) rounding
                            from bill 
                            where bill.companyid=@companyid and bill.locationid=@locationid and bill.isactive = true 
                            and bill.isb2b =false and bill.billtype != 9 and iscreditbill=false and bill.billdate>=@fromdate and bill.billdate<@todate {0} {1}
                            group by bill.locationid,bill.counterid;

                            Select customer.id customerid, customer.name as customername,bill.locationid,
                            sum(bill.receivable-bill.additionalcharges) amount,sum(rounding) rounding
                            from bill 
                            left outer join customer on customer.id = bill.customerid
                            left outer join referencelist on referencelist.id = customer.cityid
                            where bill.companyid=@companyid and bill.locationid=@locationid and bill.isactive = true and bill.customerid >0 and 
                            bill.isb2b =true and billtype!=8 and billtype!=9 and bill.billdate>=@fromdate and bill.billdate<@todate {0} {1}
                            group by bill.locationid,customer.name,customer.id;

                            Select bill.locationid,bill.counterid,billitems.taxpercentage,sum(billitems.receivable) amount,
                            sum(billitems.tax) tax,bill.isinterstatesale
                            from bill 
                            inner join billitems on billitems.billid = bill.id and billitems.isactive=true
                            where bill.companyid=@companyid and bill.locationid=@locationid  and bill.isactive = true and 
                            bill.isb2b = false and bill.billtype != 9 and bill.iscreditbill=false and bill.billdate>=@fromdate and bill.billdate<@todate {0} {1}
                            group by bill.locationid,bill.counterid,billitems.taxpercentage,bill.isinterstatesale;

                            Select customer.id customerid,customer.name as customername,bill.locationid,billitems.taxpercentage,
                            sum(billitems.receivable) amount,sum(billitems.tax) tax,bill.isinterstatesale
                            from bill 
                            inner join billitems on billitems.billid = bill.id and billitems.isactive=true
                            left outer join customer on customer.id = bill.customerid
                            where bill.companyid=@companyid and bill.locationid=@locationid  and bill.isactive = true and 
                            bill.isb2b =true and bill.billtype!=8 and bill.billtype!=9 and bill.billdate>=@fromdate and bill.billdate<@todate {0} {1}
                            group by customer.name,bill.locationid,billitems.taxpercentage,bill.isinterstatesale,customer.id;
                            
                            Select customer.id customerid, customer.name as customername,bill.locationid,
                            sum(bill.receivable) amount,sum(rounding) rounding
                            from bill 
                            left outer join customer on customer.id = bill.customerid
                            left outer join referencelist on referencelist.id = customer.cityid
                            where bill.companyid=@companyid and bill.locationid=@locationid  and bill.customerid >0 and 
                            bill.billtype = 8 and bill.isb2b =true and bill.billdate>=@fromdate and bill.billdate<@todate {0} {1}
                            group by bill.locationid,customer.name,customer.id;

                            Select customer.id customerid,customer.name as customername,bill.locationid,billitems.taxpercentage,sum(billitems.receivable) amount,
                            sum(billitems.tax) tax,bill.isinterstatesale
                            from bill 
                            inner join billitems on billitems.billid = bill.id and billitems.isactive=true
                            left outer join customer on customer.id = bill.customerid
                            where bill.companyid=@companyid and bill.locationid=@locationid and bill.isactive = true and 
                            bill.billtype = 8 and bill.isb2b =true and bill.billdate>=@fromdate and bill.billdate<@todate {0} {1}
                            group by customer.name,bill.locationid,billitems.taxpercentage,bill.isinterstatesale,customer.id;



                            Select customer.id customerid, customer.name as customername,bill.locationid,
                            sum(bill.receivable) amount,sum(rounding) rounding
                            from bill 
                            inner join customer on customer.id = bill.customerid
                            left outer join referencelist on referencelist.id = customer.cityid
                            where bill.companyid=@companyid and bill.locationid=@locationid  and bill.isactive = true and 
                            bill.isb2b =false and bill.iscreditbill=true and bill.billtype!=8 and bill.billtype!=9 
                            and bill.billdate>=@fromdate and bill.billdate<@todate {0} {1}                            
                            group by bill.locationid,customer.name,customer.id;

                            Select customer.id customerid, customer.name as customername,bill.locationid,
                            sum(bill.receivable) amount,sum(rounding) rounding
                            from bill 
                            left outer join customer on customer.id = bill.customerid
                            left outer join referencelist on referencelist.id = customer.cityid
                            where bill.companyid=@companyid and bill.locationid=@locationid  and bill.customerid >0 and 
                            bill.billtype = 8 and bill.isb2b =false and bill.iscreditbill=true and bill.billdate>=@fromdate and bill.billdate<@todate {0} {1}
                            group by bill.locationid,customer.name,customer.id;

                            Select customer.id customerid,customer.name as customername,bill.locationid,billitems.taxpercentage,
                            sum(billitems.receivable) amount,sum(billitems.tax) tax,bill.isinterstatesale
                            from bill 
                            inner join billitems on billitems.billid = bill.id and billitems.isactive=true
                            left outer join customer on customer.id = bill.customerid
                            where bill.companyid=@companyid and bill.locationid=@locationid  and bill.isactive = true and 
                            bill.isb2b =false and bill.iscreditbill=true and bill.billtype!=9 
                            and bill.billdate>=@fromdate and bill.billdate<@todate {0} {1}
                            group by customer.name,bill.locationid,billitems.taxpercentage,bill.isinterstatesale,customer.id;

                            Select customer.id customerid, customer.name as customername,
                            Billcreditnote.locationid,	sum(Billcreditnote.adjustedtotal) amount,
                            sum(Billcreditnote.rounding) rounding
                            from Billcreditnote 
                            left outer join customer on customer.id = Billcreditnote.customerid
                            left outer join referencelist on referencelist.id = customer.cityid
                            where Billcreditnote.companyid=@companyid and Billcreditnote.locationid=@locationid and Billcreditnote.isactive = true and 
                            creditnotedate>=@fromdate and creditnotedate<@todate {0} {1}
                            group by Billcreditnote.locationid,customer.name,customer.id;

                            Select customer.id customerid,customer.name as customername,
                            Billcreditnote.locationid,Billcreditnoteitems.taxpercentage,
                            sum(Billcreditnoteitems.adjustedtotal) amount,
                            sum(Billcreditnoteitems.adjustedtax) tax,Billcreditnote.isinterstatesale
                            from Billcreditnote 
                            inner join Billcreditnoteitems on Billcreditnoteitems.billcreditnoteid = Billcreditnote.id and 
                            Billcreditnoteitems.isactive=true
                            inner join customer on customer.id = Billcreditnote.customerid
                            where Billcreditnote.companyid=@companyid and Billcreditnote.locationid=@locationid and Billcreditnote.isactive = true and 
                            Billcreditnote.creditnotedate>=@fromdate and Billcreditnote.creditnotedate<@todate {0} {1}
                            group by customer.name,Billcreditnote.locationid,Billcreditnoteitems.taxpercentage,
                            Billcreditnote.isinterstatesale,customer.id;

                            select sum(paid_voucher) voucher,
                            billsettlementmaster.locationid,billsettlementmaster.counterid,billsettlementmaster.customerid
                            from billsettlementmaster
                            where billsettlementmaster.isactive = true
                            and billsettlementmaster.companyid=@companyid and billsettlementmaster.locationid=@locationid
                            and billsettlementmaster.settlementon >= @fromdate 
                            and billsettlementmaster.settlementon < @todate
                            and paid_voucher!=0 {2}
                            group by billsettlementmaster.customerid,billsettlementmaster.locationid,billsettlementmaster.counterid;

                            select customer.name customer,bill.locationid,bill.counterid,
                            referencelist.name chargename,billadditionalcharges.charge,
                            billadditionalcharges.tax,bill.isinterstatesale,
                            billadditionalcharges.taxpercentage from bill
                            inner join billadditionalcharges on billadditionalcharges.billid = bill.id and billadditionalcharges.isactive=true
                            inner join referencelist on referencelist.id = additionalchargestypeid
                            left outer join customer on customer.id = customerid
                            where 
                            bill.companyid=@companyid and bill.locationid=@locationid and bill.isactive = true and 
                            bill.billdate>=@fromdate and bill.billdate<@todate and bill.billtype != 9 AND bill.isb2b=true
                            and taxonly=false;

                            Select sum(bill.receivable) amount
                            from bill 
                            where bill.companyid=@companyid and bill.isactive = true 
                            and bill.isb2b =false and bill.billtype != 9 
							and iscreditbill=false
                            and bill.companyid=@companyid 
                            and bill.locationid=@locationid 
                            and bill.billdate>=@fromdate and bill.billdate<@todate
							and settled=false;                            

                            with b2bsettlement as
                            (
                                select distinct bill.customerid,billsettlementmaster.id
                                from billsettlementmaster
							    inner join billsettlement on billsettlement.billsettlementmasterid = billsettlementmaster.id
							    inner join bill on bill.id =billid
                                where billsettlementmaster.isactive = true
                                and bill.companyid=@companyid and bill.locationid=@locationid
                                and billsettlementmaster.settlementon >= @fromdate
                                and billsettlementmaster.settlementon < @todate
                                and bill.isb2b=true
                            )
                            select customer.name,
                            sum(paid_cash) cash,sum(paid_card) card,
                            sum(paid_upi) upi_paid,sum(cast(paymentinfo->>'voucher_paid' as numeric)) voucher_paid,
                            sum(case when paid_cash=0 then cast(paymentinfo->>'cash_return' as numeric) else 0 end) cash_refund from b2bsettlement
                            inner join billsettlementmaster on billsettlementmaster.id = b2bsettlement.id
                            inner join customer on customer.id = b2bsettlement.customerid
                            group by customer.name;

                            with tsettlement as (							
                            select distinct bill.isb2b,bill.iscreditbill,billsettlementmaster.id
                                                        from billsettlementmaster
							                            inner join billsettlement on billsettlement.billsettlementmasterid=billsettlementmaster.id
							                            inner join bill on bill.id = billsettlement.billid
                                                        where billsettlementmaster.isactive = true
							                            and bill.isb2b=false and bill.iscreditbill=false
							                            and paid_credit>0
                                                        and billsettlementmaster.companyid=@companyid and billsettlementmaster.locationid=@locationid
                                                        and billsettlementmaster.settlementon >= @fromdate
                                                        and billsettlementmaster.settlementon < @todate
                            )
                            select sum(paid_credit) amount
                            from billsettlementmaster
							inner join tsettlement on tsettlement.id = billsettlementmaster.id";


            SortedList<string, object> param = new SortedList<string, object>();
            param.Add("fromdate", from);
            param.Add("todate", to.AddDays(1));
            param.Add("companyid", companyid);
            param.Add("locationid", locationid);
            string customercond = "";
            string locationcond = "";

            if (locationid <= 0)
            {
                query = query.Replace("and Billcreditnote.locationid=@locationid", "");
                query = query.Replace("and billsettlementmaster.locationid=@locationid", "");
                query = query.Replace("and bill.locationid=@locationid", "");
            }
            query = string.Format(query, locationcond, customercond, locationcond.Replace("bill.", "billsettlementmaster."));



            Data.ReferencelistDataService locationdata = new Data.ReferencelistDataService();
            var locations = locationdata.Search(new ReferencelistCreteria()
            {
                type = "LOCATION",
                inactive = true
            });

            var bankdata = locationdata.Search(new ReferencelistCreteria()
            {
                type = "BANK",
                inactive = true
            });


            var counters = locationdata.Search(new ReferencelistCreteria()
            {
                type = "COUNTER",
                inactive = true
            });
            var billList = new List<SalesSummary>();
            var taxlist = new List<TaxLedger>();
            var additionallist = new List<AdditionalCharges>();
            var voucherSalesList = new List<TallyDataservice.CardSales>();
            var b2bSalesList = new List<TallyDataservice.CardSales>();
            var unsettledbills = new List<TallyDataservice.CardSales>();
            /*var allsalesdata = ExecuteDataSet(query, param);
            using (System.IO.MemoryStream memoryStream = new System.IO.MemoryStream())
            {
                ClosedXML.Excel.XLWorkbook xLWorkbook = new ClosedXML.Excel.XLWorkbook();
                foreach (System.Data.DataTable table in allsalesdata.Tables)
                {
                    var sheet = xLWorkbook.AddWorksheet(table);
                }
                xLWorkbook.SaveAs(memoryStream);
                //System.IO.File.WriteAllBytes(@"e:\t.xlsx", memoryStream.ToArray());
            }*/

            using (System.Data.IDataReader reader = ExecuteReader(query, param, 1000))
            {
                //all cash sales
                while (reader.Read())
                {
                    SalesSummary invoice = new SalesSummary();
                    invoice.LocationID = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                    invoice.CounterID = reader["CounterID"] == DBNull.Value ? 0 : Convert.ToInt64(reader["CounterID"]);
                    invoice.Amount = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"]);
                    invoice.Rounding = reader["rounding"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["rounding"]);
                    invoice.IsCredit = false;
                    invoice.Location = locations.Find(e => e.id == invoice.LocationID).code;
                    invoice.LocationName = locations.Find(e => e.id == invoice.LocationID).name;
                    invoice.Counter = counters.Find(e => e.id == invoice.CounterID).code;
                    invoice.CounterName = counters.Find(e => e.id == invoice.CounterID).name;
                    billList.Add(invoice);
                }

                if (reader.NextResult())
                {
                    //credit sales
                    while (reader.Read())
                    {
                        SalesSummary invoice = new SalesSummary();
                        invoice.LocationID = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                        invoice.Amount = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"]);
                        invoice.Rounding = reader["rounding"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["rounding"]);
                        invoice.CustomerName = reader["CustomerName"] == DBNull.Value ? null : reader["CustomerName"].ToString();
                        invoice.CustomerID = reader["customerid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["customerid"]);
                        invoice.Location = locations.Find(e => e.id == invoice.LocationID).code;
                        invoice.LocationName = locations.Find(e => e.id == invoice.LocationID).name;
                        invoice.IsCredit = true;
                        billList.Add(invoice);
                    }
                }

                if (reader.NextResult())
                {
                    //cash sales tax
                    while (reader.Read())
                    {
                        TaxLedger invoice = new TaxLedger();
                        invoice.LocationID = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                        invoice.IsCredit = false;
                        invoice.Location = locations.Find(e => e.id == invoice.LocationID).code;
                        invoice.LocationName = locations.Find(e => e.id == invoice.LocationID).name;
                        //invoice.CounterID = reader["CounterID"] == DBNull.Value ? 0 : Convert.ToInt64(reader["CounterID"]);
                        //invoice.Counter = counters.Find(e => e.id == invoice.CounterID).code;
                        //invoice.CounterName = counters.Find(e => e.id == invoice.CounterID).name;
                        invoice.taxpercentage = reader["taxpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["taxpercentage"]);
                        invoice.amount = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"]);
                        invoice.tax = reader["tax"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["tax"]);
                        invoice.IsInterstate = reader["isinterstatesale"] == DBNull.Value ? false : Convert.ToBoolean(reader["isinterstatesale"]);
                        taxlist.Add(invoice);
                    }
                }
                var t1 = taxlist.Sum(x => x.taxpercentage == 5 && x.IsInterstate ? x.tax : 0);

                if (reader.NextResult())
                {
                    //credit sales tax
                    while (reader.Read())
                    {
                        TaxLedger invoice = new TaxLedger();
                        invoice.LocationID = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                        invoice.IsCredit = true;
                        invoice.Customer = reader["CustomerName"] == DBNull.Value ? null : reader["CustomerName"].ToString();
                        invoice.CustomerID = reader["customerid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["customerid"]);
                        invoice.Location = locations.Find(e => e.id == invoice.LocationID).code;
                        invoice.LocationName = locations.Find(e => e.id == invoice.LocationID).name;
                        invoice.taxpercentage = reader["taxpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["taxpercentage"]);
                        invoice.amount = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"]);
                        invoice.tax = reader["tax"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["tax"]);
                        invoice.IsInterstate = reader["isinterstatesale"] == DBNull.Value ? false : Convert.ToBoolean(reader["isinterstatesale"]);
                        taxlist.Add(invoice);
                    }
                }

                if (reader.NextResult())
                {
                    //sales return - b2b
                    while (reader.Read())
                    {
                        SalesSummary invoice = new SalesSummary();
                        invoice.LocationID = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                        invoice.Amount = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"]);
                        invoice.Rounding = reader["rounding"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["rounding"]);
                        invoice.CustomerName = reader["CustomerName"] == DBNull.Value ? null : reader["CustomerName"].ToString();
                        invoice.CustomerID = reader["customerid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["customerid"]);
                        invoice.Location = locations.Find(e => e.id == invoice.LocationID).code;
                        invoice.LocationName = locations.Find(e => e.id == invoice.LocationID).name;
                        invoice.IsCreditNote = true;
                        billList.Add(invoice);
                    }
                }

                if (reader.NextResult())
                {
                    //salesreturn tax -b2b
                    while (reader.Read())
                    {
                        TaxLedger invoice = new TaxLedger();
                        invoice.LocationID = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                        invoice.IsCreditNote = true;
                        invoice.Customer = reader["CustomerName"] == DBNull.Value ? null : reader["CustomerName"].ToString();
                        invoice.CustomerID = reader["customerid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["customerid"]);
                        invoice.Location = locations.Find(e => e.id == invoice.LocationID).code;
                        invoice.LocationName = locations.Find(e => e.id == invoice.LocationID).name;
                        invoice.taxpercentage = reader["taxpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["taxpercentage"]);
                        invoice.amount = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"]);
                        invoice.tax = reader["tax"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["tax"]);
                        invoice.IsInterstate = reader["isinterstatesale"] == DBNull.Value ? false : Convert.ToBoolean(reader["isinterstatesale"]);
                        taxlist.Add(invoice);
                    }
                }



                //Select customer.id customerid, customer.name as customername,
                //            Billcreditnote.locationid,	sum(Billcreditnote.adjustedtotal) amount,
                //            sum(Billcreditnote.rounding) rounding


                if (reader.NextResult())
                {
                    //credit non b2b sales
                    while (reader.Read())
                    {
                        SalesSummary invoice = new SalesSummary();
                        invoice.LocationID = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                        invoice.Amount = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"]);
                        invoice.Rounding = reader["rounding"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["rounding"]);
                        invoice.CustomerName = reader["CustomerName"] == DBNull.Value ? null : reader["CustomerName"].ToString();
                        invoice.CustomerID = reader["customerid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["customerid"]);
                        invoice.Location = locations.Find(e => e.id == invoice.LocationID).code;
                        invoice.LocationName = locations.Find(e => e.id == invoice.LocationID).name;
                        invoice.IsCredit = true;
                        billList.Add(invoice);
                    }
                }



                if (reader.NextResult())
                {
                    while (reader.Read())
                    {
                        // sales return - non b2b sales - credit bill
                        SalesSummary invoice = new SalesSummary();
                        invoice.LocationID = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                        invoice.Amount = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"]);
                        invoice.Rounding = reader["rounding"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["rounding"]);
                        invoice.CustomerName = reader["CustomerName"] == DBNull.Value ? null : reader["CustomerName"].ToString();
                        invoice.CustomerID = reader["customerid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["customerid"]);
                        invoice.Location = locations.Find(e => e.id == invoice.LocationID).code;
                        invoice.LocationName = locations.Find(e => e.id == invoice.LocationID).name;
                        invoice.IsCreditNote = true;
                        billList.Add(invoice);
                    }
                }



                if (reader.NextResult())
                {
                    //credit non b2b  all tax 
                    while (reader.Read())
                    {
                        TaxLedger invoice = new TaxLedger();
                        invoice.LocationID = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                        invoice.IsCredit = true;
                        invoice.Customer = reader["CustomerName"] == DBNull.Value ? null : reader["CustomerName"].ToString();
                        invoice.CustomerID = reader["customerid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["customerid"]);
                        invoice.Location = locations.Find(e => e.id == invoice.LocationID).code;
                        invoice.LocationName = locations.Find(e => e.id == invoice.LocationID).name;
                        invoice.taxpercentage = reader["taxpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["taxpercentage"]);
                        invoice.amount = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"]);
                        invoice.tax = reader["tax"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["tax"]);
                        invoice.IsInterstate = reader["isinterstatesale"] == DBNull.Value ? false : Convert.ToBoolean(reader["isinterstatesale"]);
                        taxlist.Add(invoice);
                    }
                }



                if (reader.NextResult())
                {
                    while (reader.Read())
                    {
                        SalesSummary invoice = new SalesSummary();
                        invoice.LocationID = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                        invoice.Amount = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"]);
                        invoice.Rounding = reader["rounding"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["rounding"]);
                        invoice.CustomerName = reader["CustomerName"] == DBNull.Value ? null : reader["CustomerName"].ToString();
                        invoice.CustomerID = reader["customerid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["customerid"]);
                        invoice.Location = locations.Find(e => e.id == invoice.LocationID).code;
                        invoice.LocationName = locations.Find(e => e.id == invoice.LocationID).name;
                        invoice.IsCreditNote = true;
                        billList.Add(invoice);
                    }
                }


                if (reader.NextResult())
                {
                    while (reader.Read())
                    {
                        TaxLedger invoice = new TaxLedger();
                        invoice.LocationID = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                        invoice.IsCreditNote = true;
                        invoice.Customer = reader["CustomerName"] == DBNull.Value ? null : reader["CustomerName"].ToString();
                        invoice.CustomerID = reader["customerid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["customerid"]);
                        invoice.Location = locations.Find(e => e.id == invoice.LocationID).code;
                        invoice.LocationName = locations.Find(e => e.id == invoice.LocationID).name;
                        invoice.taxpercentage = reader["taxpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["taxpercentage"]);
                        invoice.amount = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"]);
                        invoice.tax = reader["tax"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["tax"]);
                        invoice.IsInterstate = reader["isinterstatesale"] == DBNull.Value ? false : Convert.ToBoolean(reader["isinterstatesale"]);
                        taxlist.Add(invoice);
                    }
                }
                var t4 = taxlist.Sum(x => x.taxpercentage == 5 && x.IsInterstate ? x.tax : 0);
                //if (reader.NextResult())
                //{
                //    //sum(upi_paid) upi_paid,upiproviderid,billsettlementmaster.locationid
                //    while (reader.Read())
                //    {
                //        CardSummary cardSales = new CardSummary();
                //        cardSales.refid = reader["customerid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["customerid"]);
                //        cardSales.bankid = reader.IsDBNull(1) ? 0 : reader.GetInt64(1);
                //        cardSales.amount = reader.GetDecimal(0);
                //        cardSales.locationid = reader.GetInt64(2);
                //        cardSales.CounterID = reader["CounterID"] == DBNull.Value ? 0 : Convert.ToInt64(reader["CounterID"]);
                //        if (bankdata.Exists(e => e.id == cardSales.bankid))
                //        {
                //            cardSales.bank = bankdata.Find(e => e.id == cardSales.bankid).name;
                //        }
                //        else if (bankdata.Count() == 1)
                //        {
                //            cardSales.bank = bankdata.First().name;
                //        }
                //        else
                //        {
                //            cardSales.bank = "DEFAULT BANK";
                //        }
                //        //cardSalesList.Add(cardSales);
                //    }
                //}

                //if (reader.NextResult())
                //{
                //    while (reader.Read())
                //    {
                //        CardSummary cardSales = new CardSummary();
                //        cardSales.refid = reader["customerid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["customerid"]);
                //        cardSales.bankid = reader.IsDBNull(1) ? 0 : reader.GetInt64(1);
                //        cardSales.amount = reader.GetDecimal(0);
                //        cardSales.locationid = reader.GetInt64(2);
                //        cardSales.CounterID = reader["CounterID"] == DBNull.Value ? 0 : Convert.ToInt64(reader["CounterID"]);
                //        if (bankdata.Exists(e => e.id == cardSales.bankid))
                //        {
                //            cardSales.bank = bankdata.Find(e => e.id == cardSales.bankid).name;
                //        }
                //        else if (bankdata.Count() == 1)
                //        {
                //            cardSales.bank = bankdata.First().name;
                //        }
                //        else
                //        {
                //            cardSales.bank = "DEFAULT BANK";
                //        }
                //        //cardSalesList.Add(cardSales);
                //    }
                //}

                if (reader.NextResult())
                {
                    while (reader.Read())
                    {
                        CardSummary cardSales = new CardSummary();
                        cardSales.amount = reader["voucher"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["voucher"]);
                        cardSales.locationid = reader["Locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["Locationid"]);
                        cardSales.counterid = reader["CounterID"] == DBNull.Value ? 0 : Convert.ToInt64(reader["CounterID"]);
                        //cardSales.date = reader["Billdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["Billdate"]);
                        cardSales.locationcode = locations.Find(e => e.id == cardSales.locationid).code;
                        cardSales.locationname = locations.Find(e => e.id == cardSales.locationid).name;
                        voucherSalesList.Add(cardSales);
                    }
                }

                if (reader.NextResult())
                {
                    while (reader.Read())
                    {
                        AdditionalCharges cardSales = new AdditionalCharges();
                        cardSales.chargename = reader["chargename"] == DBNull.Value ? "" : Convert.ToString(reader["chargename"]);
                        cardSales.customer = reader["customer"] == DBNull.Value ? "" : Convert.ToString(reader["customer"]);
                        cardSales.charge = reader["charge"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["charge"]);
                        cardSales.tax = reader["tax"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["tax"]);
                        cardSales.taxpercentage = reader["taxpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["taxpercentage"]);
                        cardSales.LocationID = reader["Locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["Locationid"]);
                        cardSales.CounterID = reader["CounterID"] == DBNull.Value ? 0 : Convert.ToInt64(reader["CounterID"]);
                        cardSales.IsInterstate = reader["isinterstatesale"] == DBNull.Value ? false : Convert.ToBoolean(reader["isinterstatesale"]);
                        cardSales.LocationCode = locations.Find(e => e.id == cardSales.LocationID).code;
                        cardSales.LocationName = locations.Find(e => e.id == cardSales.LocationID).name;
                        cardSales.CounterCode = counters.Find(e => e.id == cardSales.CounterID).code;
                        cardSales.CounterName = counters.Find(e => e.id == cardSales.CounterID).name;
                        additionallist.Add(cardSales);
                    }
                }


                if (reader.NextResult())
                {
                    if (reader.Read())
                    {
                        CardSummary cardSales = new CardSummary();
                        cardSales.amount = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"]);
                        cardSales.isupi = false;
                        unsettledbills.Add(cardSales);
                    }
                }

                if (reader.NextResult())
                {
                    while (reader.Read())
                    {
                        CardSummary cardSales = new CardSummary();
                        cardSales.amount = reader["cash"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["cash"]);
                        cardSales.amount += reader["card"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["card"]);
                        cardSales.amount += reader["upi_paid"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["upi_paid"]);
                        cardSales.bank = reader["name"] == DBNull.Value ? "CUSTOMER" : reader["name"].ToString();
                        b2bSalesList.Add(cardSales);
                    }
                }

                if (reader.NextResult())
                {
                    if (reader.Read())
                    {
                        CardSummary cardSales = new CardSummary();
                        cardSales.amount = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"]);
                        cardSales.isupi = true;
                        unsettledbills.Add(cardSales);
                    }
                }

            }


            return new QuantoLedgers()
            {
                TaxList = taxlist,
                VoucherSalesList = voucherSalesList,
                BillList = billList,
                B2BSalesList = b2bSalesList,
                Unsettled = unsettledbills,
                AdditionalCharges = additionallist
            };
        }

        public QuantoLedgers SearchPurchaseLedgers(long companyid, int year)
        {
            var fromdate = new DateTime(year, 4, 1);
            var todate = new DateTime(year, 4, 1).AddYears(1);
            return SearchPurchaseLedgers(companyid, fromdate, todate, false);
        }
        public QuantoLedgers SearchPurchaseLedgers(long companyid, DateTime from, DateTime to, bool isdetail)
        {
            /*with diff
            as
            (
            select lrinvoicetax.id,lrinvoicetax.invoiceid,amounton,netamount,lrinvoicetax.taxamount,lrinvoice.totalamount from lrinvoicetax
            inner join lrinvoice on lrinvoice.id = invoiceid
            where amounton=netamount and lrinvoicetax.taxamount!=0 and amounton>0 and netamount>0
            and totalamount=amounton
            )
            update lrinvoicetax set amounton=lrinvoicetax.netamount-lrinvoicetax.taxamount from diff
            where diff.id = lrinvoicetax.id*/

            QuantoLedgers quantoLedgers = new QuantoLedgers();
            string query = @"select sum(case when lrinvoicetax.type=1 then lrinvoicetax.amounton else lrinvoicetax.netamount-lrinvoicetax.taxamount end)  taxable,
                            sum(lrinvoicetax.taxamount) tax,type,suppliercompany.id as suppliercompanyid,
                            lrinvoicetax.percentage,
                            case 
                            when lrinvoicetax.type = 4 then 'Insurance' 
                            when lrinvoicetax.type = 5 then 'Service' 
                            when lrinvoicetax.type = 6 then 'Courier' 
                            when lrinvoicetax.type = 7 then 'Packing' 
                            when lrinvoicetax.type = 8 then 'Fright' 
                            when lrinvoicetax.type = 12 then 'TCS'
                            when lrinvoicetax.type = 11 then 'CESS' 
                            else 'GST' end taxname,
                            lrinvoice.isinterstatesale isinterstatesale from lrinvoicetax
                            inner join lrinvoice on lrinvoice.id = invoiceid  and lrinvoice.isactive = true
                            inner join suppliercompany on suppliercompany.id = lrinvoice.suppliercompanyid
                            where lrinvoice.isactive=true 
                            and lrinvoice.settled = false
							and lrinvoicetax.isactive = true 
                            and (lrinvoicetax.type=0 and lrinvoicetax.taxid=0 and lrinvoicetax.taxamount=0)=false
                            and lrinvoicetax.type not in (2,3,9,14)
						    and lrinvoice.companyid=@companyid 
							and lrinvoice.entrydate>=@fromdate and lrinvoice.entrydate<@todate
                            group by suppliercompany.id,type,lrinvoicetax.percentage,
                            case 
                            when lrinvoicetax.type = 4 then 'Insurance' 

                            when lrinvoicetax.type = 5 then 'Service' 
                            when lrinvoicetax.type = 6 then 'Courier' 
                            when lrinvoicetax.type = 7 then 'Packing' 
                            when lrinvoicetax.type = 8 then 'Fright' 
                            when lrinvoicetax.type = 12 then 'TCS'
                            when lrinvoicetax.type = 11 then 'CESS' 
                            else 'GST' end,
                            lrinvoice.isinterstatesale;

            select sum(lrinvoice.totalamount-coalesce(lrinvoice.presettled,0)) totalamount,
	        sum(lrinvoice.otheramount) otheramount,suppliercompany.id as suppliercompanyid,
                            suppliercompany.name as suppliercompany,city.name cityname,
							lrinvoice.isinterstatesale isinterstatesale,
                            sum(case when lrinvoice.taxamount=0 then lrinvoice.totalamount-coalesce(lrinvoice.presettled,0) else 0 end) zerovalued
                            from lrinvoice
                            left outer join lrentry on lrentry.id = lrid
                            inner join suppliercompany on suppliercompany.id = lrinvoice.suppliercompanyid
                            left outer join referencelist state on state.id = suppliercompany.stateid
                            left outer join referencelist city on city.id = suppliercompany.cityid
                            where lrinvoice.isactive=true
                            and lrinvoice.settled = false
                            and lrinvoice.companyid=@companyid 
							and lrinvoice.entrydate>=@fromdate 
							and lrinvoice.entrydate<@todate
							group by suppliercompany.name,suppliercompany.id,
							lrinvoice.isinterstatesale,city.name;";

            if (isdetail)
            {
                query = @"  select sum(case when lrinvoicetax.type=1 then lrinvoicetax.amounton else lrinvoicetax.netamount-lrinvoicetax.taxamount end)  taxable,
                            sum(lrinvoicetax.taxamount) tax,type,suppliercompany.id as suppliercompanyid,
                            lrinvoicetax.percentage,lrentry.lrentrydate date,lrinvoice.id invoiceid,
                            case 
                            when lrinvoicetax.type = 4 then 'Insurance' 
                            when lrinvoicetax.type = 5 then 'Service' 
                            when lrinvoicetax.type = 6 then 'Courier' 
                            when lrinvoicetax.type = 7 then 'Packing' 
                            when lrinvoicetax.type = 8 then 'Fright' 
                            when lrinvoicetax.type = 12 then 'TCS'
                            when lrinvoicetax.type = 11 then 'CESS' 
                            else 'GST' end taxname,
                            lrinvoice.isinterstatesale isinterstatesale 
                            from lrinvoicetax
                            inner join lrinvoice on lrinvoice.id = invoiceid  and lrinvoice.isactive = true
                            left outer join lrentry on lrentry.id = lrid
                            inner join suppliercompany on suppliercompany.id = lrinvoice.suppliercompanyid
                            where lrinvoice.isactive=true 
                            and lrinvoice.settled = false
							and lrinvoicetax.isactive = true
                            and (lrinvoicetax.type=0 and lrinvoicetax.taxid=0 and lrinvoicetax.taxamount=0)=false
                            and lrinvoicetax.type not in (2,3,9,14)
						    and lrinvoice.companyid=@companyid 
							and lrinvoice.entrydate>=@fromdate and lrinvoice.entrydate<@todate
                            group by suppliercompany.id,type,lrinvoicetax.percentage,lrentry.lrentrydate,
                            case 
                            when lrinvoicetax.type = 4 then 'Insurance' 
                            when lrinvoicetax.type = 5 then 'Service' 
                            when lrinvoicetax.type = 6 then 'Courier' 
                            when lrinvoicetax.type = 7 then 'Packing' 
                            when lrinvoicetax.type = 8 then 'Fright' 
                            when lrinvoicetax.type = 12 then 'TCS'
                            when lrinvoicetax.type = 11 then 'CESS' 
                            else 'GST' end,
                            lrinvoice.isinterstatesale,
                            lrinvoice.invoicedate,lrinvoice.id;

                            select sum(lrinvoice.totalamount-coalesce(lrinvoice.presettled,0)) totalamount,lrinvoice.invoiceno,lrinvoice.invoicedate,
	                        sum(lrinvoice.otheramount) otheramount,suppliercompany.id as suppliercompanyid,lrinvoice.createdby,
                            suppliercompany.name as suppliercompany,lrentry.lrentrydate date,lrinvoice.id invoiceid,
							lrinvoice.isinterstatesale isinterstatesale,city.name cityname,
                            sum(case when lrinvoice.taxamount=0 then lrinvoice.totalamount-coalesce(lrinvoice.presettled,0) else 0 end) zerovalued
                            from lrinvoice
                            left outer join lrentry on lrentry.id = lrid
                            inner join suppliercompany on suppliercompany.id = lrinvoice.suppliercompanyid
                            left outer join referencelist state on state.id = suppliercompany.stateid
                            left outer join referencelist city on city.id = suppliercompany.cityid
                            where lrinvoice.isactive=true
                            and lrinvoice.settled = false
                            and lrinvoice.companyid=@companyid 
							and lrinvoice.entrydate>=@fromdate 
							and lrinvoice.entrydate<@todate
							group by suppliercompany.name,suppliercompany.id,lrentry.lrentrydate,lrinvoice.invoiceno,lrinvoice.invoicedate,lrinvoice.id,
							lrinvoice.isinterstatesale,lrinvoice.createdby,city.name;";
            }
            SortedList<string, object> param = new SortedList<string, object>();
            param.Add("fromdate", from);
            param.Add("todate", to.AddDays(1));
            param.Add("companyid", companyid);

            List<PurchaseLedgerSummary> invoicelist = new List<PurchaseLedgerSummary>();
            List<TaxLedgerSummary> taxlist = new List<TaxLedgerSummary>();
            quantoLedgers.PurchaseLegers = invoicelist;
            quantoLedgers.TaxLegers = taxlist;


            /*var allsalesdata = ExecuteDataSet(query, param);
            using (System.IO.MemoryStream memoryStream = new System.IO.MemoryStream())
            {
                ClosedXML.Excel.XLWorkbook xLWorkbook = new ClosedXML.Excel.XLWorkbook();
                foreach (System.Data.DataTable table in allsalesdata.Tables)
                {
                    var sheet = xLWorkbook.AddWorksheet(table);
                }
                xLWorkbook.SaveAs(memoryStream);
                //System.IO.File.WriteAllBytes(@"e:\t.xlsx", memoryStream.ToArray());
            }*/

            using (System.Data.IDataReader reader = ExecuteReader(query, param, 1000))
            {
                while (reader.Read())
                {
                    TaxLedgerSummary invoice = new TaxLedgerSummary();
                    invoice.suppliercompanyid = reader["suppliercompanyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["suppliercompanyid"]);
                    invoice.taxable = reader["taxable"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["taxable"]);
                    invoice.tax = reader["tax"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["tax"]);
                    invoice.type = reader["type"] == DBNull.Value ? 0 : Convert.ToInt32(reader["type"]);
                    invoice.percentage = reader["percentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["percentage"]);
                    invoice.taxname = reader["taxname"] == DBNull.Value ? "" : reader["taxname"].ToString();
                    invoice.isinterstatesale = reader["isinterstatesale"] == DBNull.Value ? false : Convert.ToBoolean(reader["isinterstatesale"]);
                    if (isdetail)
                    {
                        invoice.invoiceid = reader["invoiceid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["invoiceid"]);
                        invoice.date = reader["date"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["date"]);
                    }
                    taxlist.Add(invoice);
                }
                if (reader.NextResult())
                {
                    while (reader.Read())
                    {
                        PurchaseLedgerSummary invoice = new PurchaseLedgerSummary();
                        invoice.suppliercompanyid = reader["suppliercompanyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["suppliercompanyid"]);
                        invoice.isinterstatesale = reader["isinterstatesale"] == DBNull.Value ? false : Convert.ToBoolean(reader["isinterstatesale"]);
                        invoice.totalamount = reader["totalamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["totalamount"]);
                        invoice.zerovalued = reader["zerovalued"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["zerovalued"]);
                        invoice.suppliercompany = reader["suppliercompany"] == DBNull.Value ? "" : reader["suppliercompany"].ToString();
                        invoice.cityname = reader["cityname"] == DBNull.Value ? "" : reader["cityname"].ToString();
                        if (isdetail)
                        {
                            invoice.invoiceid = reader["invoiceid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["invoiceid"]);
                            invoice.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                            invoice.entrydate = reader["date"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["date"]);
                            invoice.invoicedate = reader["invoicedate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["invoicedate"]);
                            invoice.invoiceno = reader["invoiceno"] == DBNull.Value ? "" : reader["invoiceno"].ToString();
                            invoice.taxledgersummary = taxlist.FindAll(x => x.invoiceid == invoice.invoiceid);
                            invoice.taxledgersummary.ForEach(x => x.adjusted = false);
                        }
                        else
                        {
                            invoice.taxledgersummary = taxlist.FindAll(x => x.suppliercompanyid == invoice.suppliercompanyid &&
                                x.isinterstatesale == invoice.isinterstatesale);
                            invoice.taxledgersummary.ForEach(x => x.adjusted = false);
                        }
                        if(invoice.zerovalued>0)
                        {
                            var zerotaxsummary = invoice.zerovalued - invoice.taxledgersummary.FindAll(x => x.percentage == 0).Sum(y => y.taxable);
                            invoice.taxledgersummary.Add(new TaxLedgerSummary()
                            {
                                percentage = 0,
                                taxname = "GST",
                                taxable = invoice.zerovalued
                            });
                        }
                        invoicelist.Add(invoice);
                        var parentamt = invoice.taxledgersummary.Sum(x => x.tax + x.taxable);
                        if (Math.Abs(parentamt - invoice.totalamount) > 5)
                        {
                            if (parentamt > invoice.totalamount)
                            {
                                invoice.additionaldiscount = (parentamt - invoice.totalamount);
                            }
                            else
                            {
                                invoice.additionalcharges = (invoice.totalamount - parentamt);
                            }
                        }
                    }
                }
            }

            using (var reader = ExecuteReader(@"select suppliercompany.name from suppliercompany
                        group by suppliercompany.name
                        having count(*)>1", null))
            {
                List<string> names = new List<string>();
                while (reader.Read())
                {
                    names.Add(reader.IsDBNull(0) ? "" : reader.GetString(0));
                }
                invoicelist.FindAll(x => names.Contains(x.suppliercompany)).ForEach(y =>
                {
                    y.suppliercompany += "(" + y.cityname + ")";
                });
            }
            return quantoLedgers;
        }

        public QuantoLedgers SearchJobWorkLedgers(long companyid, DateTime from, DateTime to, bool isdetail)
        {
            QuantoLedgers quantoLedgers = new QuantoLedgers();
            string query = @"select jobworkbillgrnid,chargetaxpercentage,Jobworkbillgrn.suppliercompanyid,
                            suppliercompany.name suppliercompany,Jobworkbillgrn.isinterstatesale,city.name cityname,
                            sum(chargetax) chargetax,sum(chargegross) taxable,sum(chargetotal) chargetotal
                            from Jobworkbillgrnitems
                            inner join Jobworkbillgrn on Jobworkbillgrn.id = jobworkbillgrnid
                            inner join suppliercompany on suppliercompany.id = Jobworkbillgrn.suppliercompanyid
                            left outer join referencelist city on city.id = suppliercompany.cityid
                            where Jobworkbillgrnitems.isactive=true 
                            and Jobworkbillgrn.companyid=@companyid 
							and Jobworkbillgrn.entrydate>=@fromdate and Jobworkbillgrn.entrydate<@todate
                            group by jobworkbillgrnid,chargetaxpercentage,Jobworkbillgrn.suppliercompanyid,
                            suppliercompany.name ,Jobworkbillgrn.isinterstatesale,city.name;";

            if (isdetail)
            {
                query = @"select jobworkbillgrnid,chargetaxpercentage,Jobworkbillgrn.suppliercompanyid,
                          suppliercompany.name suppliercompany,Jobworkbillgrn.isinterstatesale,
                          sum(chargetax) chargetax,sum(chargegross) taxable,sum(chargetotal) chargetotal,
                          Jobworkbillgrn.invoiceno,Jobworkbillgrn.id,Jobworkbillgrn.createdby,
                          Jobworkbillgrn.invoicedate,city.name cityname,
                          Jobworkbillgrn.entrydate,Jobworkbillgrn.entryno
                          from Jobworkbillgrnitems
                          inner join Jobworkbillgrn on Jobworkbillgrn.id = jobworkbillgrnid
                          inner join suppliercompany on suppliercompany.id = Jobworkbillgrn.suppliercompanyid
                          left outer join referencelist city on city.id = suppliercompany.cityid
                          where Jobworkbillgrnitems.isactive=true 
                          and Jobworkbillgrn.companyid=@companyid 
						  and Jobworkbillgrn.entrydate>=@fromdate and Jobworkbillgrn.entrydate<@todate
                          group by jobworkbillgrnid,chargetaxpercentage,Jobworkbillgrn.suppliercompanyid,
                          suppliercompany.name ,Jobworkbillgrn.isinterstatesale,Jobworkbillgrn.invoiceno,
                          Jobworkbillgrn.id,Jobworkbillgrn.createdby,Jobworkbillgrn.invoicedate,
                          Jobworkbillgrn.entrydate,Jobworkbillgrn.entryno,city.name;";
            }
            SortedList<string, object> param = new SortedList<string, object>();
            param.Add("fromdate", from);
            param.Add("todate", to.AddDays(1));
            param.Add("companyid", companyid);

            List<PurchaseLedgerSummary> invoicelist = new List<PurchaseLedgerSummary>();
            List<TaxLedgerSummary> taxlist = new List<TaxLedgerSummary>();
            quantoLedgers.PurchaseLegers = invoicelist;

            using (System.Data.IDataReader reader = ExecuteReader(query, param, 1000))
            {


                while (reader.Read())
                {
                    PurchaseLedgerSummary invoice = new PurchaseLedgerSummary();
                    invoice.suppliercompanyid = reader["suppliercompanyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["suppliercompanyid"]);
                    invoice.isinterstatesale = reader["isinterstatesale"] == DBNull.Value ? false : Convert.ToBoolean(reader["isinterstatesale"]);
                    invoice.totalamount = reader["chargetotal"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["chargetotal"]);
                    invoice.suppliercompany = reader["suppliercompany"] == DBNull.Value ? "" : reader["suppliercompany"].ToString();
                    invoice.cityname = reader["cityname"] == DBNull.Value ? "" : reader["cityname"].ToString();
                    var chargegross = reader["taxable"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["taxable"]);
                    var chargetax = reader["chargetax"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["chargetax"]);
                    var chargetaxpercentage = reader["chargetaxpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["chargetaxpercentage"]);
                    TaxLedgerSummary taxLedgerSummary = new TaxLedgerSummary()
                    {
                        percentage = chargetaxpercentage,
                        tax = chargetax,
                        taxable = chargegross,
                        type = 1
                    };
                    invoice.taxledgersummary = new List<TaxLedgerSummary>();
                    invoice.taxledgersummary.Add(taxLedgerSummary);
                    if (isdetail)
                    {
                        invoice.invoiceid = reader["invoiceid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["invoiceid"]);
                        invoice.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        invoice.entrydate = reader["entrydate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["entrydate"]);
                        invoice.invoicedate = reader["invoicedate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["invoicedate"]);
                        invoice.invoiceno = reader["invoiceno"] == DBNull.Value ? "" : reader["invoiceno"].ToString();
                    }
                    invoicelist.Add(invoice);
                }

            }

            using (var reader = ExecuteReader(@"select suppliercompany.name from suppliercompany
                        group by suppliercompany.name
                        having count(*)>1", null))
            {
                List<string> names = new List<string>();
                while (reader.Read())
                {
                    names.Add(reader.IsDBNull(0) ? "" : reader.GetString(0));
                }
                invoicelist.FindAll(x => names.Contains(x.suppliercompany)).ForEach(y =>
                {
                    y.suppliercompany += "(" + y.cityname + ")";
                });
            }
            return quantoLedgers;
        }

        public QuantoLedgers SearchTransportLedgers(long companyid, long locationid, DateTime from, DateTime to, bool isdetail)
        {
            QuantoLedgers quantoLedgers = new QuantoLedgers();
            string query = @"select sum(lrreceivedlist.totalamount) totalamount,
                            transport.name suppliercompany,lrreceived.bookingofficeid suppliercompanyid
                            from lrreceivedlist
                            inner join lrreceived on lrreceived.id = lrreceivedid
                            left outer join lrentry on lrentry.id = lrreceivedlist.lrid
                            inner join transport on transport.id = lrreceived.bookingofficeid
                            where lrreceivedlist.totalamount >0
                            and lrreceived.companyid=@companyid
                            and lrentry.receiveddate >=@fromdate and lrentry.receiveddate <@todate
                            group by transport.name,lrreceived.bookingofficeid;

                            select sum(courier.amount) totalamount,
                            transport.name suppliercompany,courier.companyid suppliercompanyid
                            from courier
                            inner join transport on transport.id = courier.companyid
                            where courier.receivingcompanyid=@companyid
                            and courier.receivedon >=@fromdate and courier.receivedon <@todate
                            group by transport.name,courier.companyid;


                            ";

            if (isdetail)
            {
                query = @"select sum(lrreceivedlist.totalamount) totalamount,lrreceived.bookingofficeid suppliercompanyid,
                            transport.name suppliercompany,lrentry.receiveddate date
                            from lrreceivedlist
                            inner join lrreceived on lrreceived.id = lrreceivedid
                            left outer join lrentry on lrentry.id = lrreceivedlist.lrid
                            inner join transport on transport.id = lrreceived.bookingofficeid
                            where lrreceivedlist.totalamount >0
                            and lrreceived.companyid=@companyid
                            and lrentry.receiveddate >=@fromdate and lrentry.receiveddate <@todate
                            group by lrreceived.bookingofficeid,lrentry.receiveddate,transport.name;

                            select sum(courier.amount) totalamount,courier.companyid suppliercompanyid,
                            transport.name suppliercompany,courier.receivedon date
                            from courier
                            inner join transport on transport.id = courier.companyid
                            where courier.receivingcompanyid=@companyid
                            and courier.receivedon >=@fromdate and courier.receivedon <@todate
                            group by transport.name,courier.receivedon,courier.companyid;";

            }
            SortedList<string, object> param = new SortedList<string, object>();
            param.Add("fromdate", from);
            param.Add("todate", to.AddDays(1));
            param.Add("companyid", companyid);

            List<PurchaseLedgerSummary> invoicelist = new List<PurchaseLedgerSummary>();
            List<TaxLedgerSummary> taxlist = new List<TaxLedgerSummary>();
            quantoLedgers.PurchaseLegers = invoicelist;
            quantoLedgers.TaxLegers = taxlist;
            using (System.Data.IDataReader reader = ExecuteReader(query, param, 1000))
            {
                while (reader.Read())
                {
                    PurchaseLedgerSummary invoice = new PurchaseLedgerSummary();
                    invoice.totalamount = reader["totalamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["totalamount"]);
                    invoice.suppliercompany = reader["suppliercompany"] == DBNull.Value ? "" : reader["suppliercompany"].ToString();
                    invoice.suppliercompanyid = reader["suppliercompanyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["suppliercompanyid"]);
                    if (isdetail)
                        invoice.entrydate = reader["date"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["date"]);
                    invoicelist.Add(invoice);
                }
                if (reader.NextResult())
                {
                    while (reader.Read())
                    {
                        PurchaseLedgerSummary invoice = new PurchaseLedgerSummary();
                        invoice.totalamount = reader["totalamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["totalamount"]);
                        invoice.suppliercompany = reader["suppliercompany"] == DBNull.Value ? "" : reader["suppliercompany"].ToString();
                        invoice.suppliercompanyid = reader["suppliercompanyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["suppliercompanyid"]);
                        if (isdetail)
                            invoice.entrydate = reader["date"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["date"]);
                        invoicelist.Add(invoice);
                    }
                }
            }
            return quantoLedgers;
        }

        public QuantoLedgers SearchCustomerOrderAndVouchers(long companyid, long locationid, DateTime from, DateTime to, bool isdetail)
        {
            /*with advancecollection as 
(select customer.name, customer.mobileno,
cast(paymentlist->>'mode' as int) paymentmode, 
cast(paymentlist->>'amount' as numeric) paid, 
 cast(paymentlist->>'bankid' as bigint) bankid, 
 cast(paymentlist->>'actionon' as timestamp without time zone) paidon, 
customerorder.locationid, customerorder.companyid  
from customerorder 
inner join customer on customer.id = customerid
INNER JOIN LATERAL json_array_elements (customerorder.paymentinfo :: json) paymentlist ON true
)
select * from advancecollection

select createdon,coupons.name, coupons.seqcode code,receivedamt,paymentmode from coupons 
where coupons.isactive=true and entrytype=1 
and billsettlementmasterid=0
and issuedon>='2024-06-04' and issuedon<'2024-06-05' 
and coupons.counterid = @counterid and issuedby=@cashierid
and coupons.createdon>=@fromdate and coupons.createdon<@todate */


            QuantoLedgers quantoLedgers = new QuantoLedgers();
            string query = @"select customer.id,customer.name,
                    SUM(cast(schemelist->>'amount' as numeric)) amount,ledger.displayname ledgername,
                    case when schemelist->>'mode' != '3' then referencelist.name else 'CASH' end bankname
                    from Customerorder
					LEFT JOIN LATERAL
					json_array_elements(Customerorder.paymentinfo :: json) schemelist ON true
                    LEFT OUTER join customer on customer.id = Customerorder.customerid
                    left outer join referencelist on referencelist.id = cast(schemelist->>'bankid' as bigint)
                    left outer join ledger on ledger.id =cast(schemelist->>'ledgerid' as bigint)
                    where Customerorder.isactive=true 
					and Customerorder.companyid=@companyid 
                    and Customerorder.locationid =@locationid
                    and cast(schemelist->>'actionon' as date)>=@fromdate
                    and cast(schemelist->>'actionon' as date)<@todate
                    group by customer.id,customer.name,
					case when schemelist->>'mode' != '3' then referencelist.name else 'CASH' end,
					ledger.displayname";

            if (isdetail)
            {
                query = @"select customer.id,customer.name,cast(schemelist->>'actionon' as date) actionon,
                    cast(schemelist->>'amount' as numeric) amount,customerorderno,ledger.displayname ledgername,
                    case when schemelist->>'mode' != '3' then referencelist.name else 'CASH' end bankname
                    from Customerorder
					LEFT JOIN LATERAL
					json_array_elements(Customerorder.paymentinfo :: json) schemelist ON true
                    inner join customer on customer.id = Customerorder.customerid
                    left outer join referencelist on referencelist.id = cast(schemelist->>'bankid' as bigint)
                    left outer join ledger on ledger.id =cast(schemelist->>'ledgerid' as bigint)
                    where Customerorder.isactive=true 
                    and customerorder.companyid=@companyid 
                    and customerorder.locationid =@locationid
                    and cast(schemelist->>'actionon' as date)>=@fromdate
                    and cast(schemelist->>'actionon' as date)<@todate;";

            }
            SortedList<string, object> param = new SortedList<string, object>();
            param.Add("fromdate", from);
            param.Add("todate", to.AddDays(1));
            param.Add("companyid", companyid);
            param.Add("locationid", locationid);
            if (locationid <= 0)
                query = query.Replace("and customerorder.locationid =@locationid", "");
            int id = 1;

            quantoLedgers.CustomerPayment = new List<CustomerPaymentSummary>();
            quantoLedgers.CustomerAdjustment = new List<CustomerPaymentSummary>();
            using (System.Data.IDataReader reader = ExecuteReader(query, param, 1000))
            {
                while (reader.Read())
                {
                    CustomerPaymentSummary invoice = new CustomerPaymentSummary();
                    invoice.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                    invoice.customername = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
                    invoice.bankname = reader["bankname"] == DBNull.Value ? "" : reader["bankname"].ToString();
                    invoice.ledger = reader["ledgername"] == DBNull.Value ? "" : reader["ledgername"].ToString();
                    invoice.amount = reader["amount"] == DBNull.Value ? 0 : (decimal)reader["amount"];
                    invoice.vouchertype = "Customer Order Advance";
                    if (isdetail)
                    {
                        invoice.orderno = reader["customerorderno"] == DBNull.Value ? "" : reader["customerorderno"].ToString();
                        invoice.date = reader["actionon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["actionon"]);
                    }
                    invoice.iscash = invoice.bankname == "CASH";
                    quantoLedgers.CustomerPayment.Add(invoice);
                }
            }

            query = @"select customer.id,sum(receivedamt) amount,customer.name,
                    case 
                    when coupons.paymentmode = 7 then 'EXPENSE'
                    when coupons.paymentmode != 3 AND referencelist.ID IS NOT NULL then referencelist.name else 'CASH' end bankname,
                    ledger.displayname ledgername
                    from coupons
                    inner join customer on customer.id = coupons.customerid
                    left outer join referencelist on referencelist.id = coupons.bankid
                    left outer join ledger on ledger.id = coupons.ledgerid
                    where coupons.isactive=true and coupons.issuedon>=@fromdate
                    and coupons.issuedon<@todate  
                    and coupons.companyid=@companyid  
                    and coupons.issuemode = 0
                    and coupons.id in (select distinct coupenid from vouchers where vouchertype=1 and isactive=true)
                    group by customer.id,customer.name,ledger.displayname,
                    case 
                    when coupons.paymentmode = 7 then 'EXPENSE'
                    when coupons.paymentmode != 3 AND referencelist.ID IS NOT NULL then referencelist.name else 'CASH' end
                    UNION ALL 
                    select customer.id,sum(receivedamt) amount,customer.name,
                    'CREDIT NOTE' bankname,
                    ledger.displayname ledgername
                    from vouchers
                    inner join coupons on coupons.id = coupenid
                    inner join customer on customer.id = coupons.customerid
                    left outer join referencelist on referencelist.id = coupons.bankid
                    left outer join ledger on ledger.id = coupons.ledgerid
                    where 
					vouchers.isactive=true and vouchers.vouchertype=7 and
					coupons.isactive=true 
					and vouchers.issuedon>=@fromdate
                    and vouchers.issuedon<@todate  
                    and coupons.companyid=@COMPANYID
                    group by customer.id,customer.name,ledger.displayname";

            /*
                    union all 

                    select sum(counteramount) amount,customer.name,
                    case when vouchers.ipaymentmode != 3 and vouchers.ipaymentmode != 0 then referencelist.name else 'CASH' end bankname,
                    ledger.displayname ledgername
                    from vouchers
                    inner join coupons on coupons.id = vouchers.coupenid
                    inner join customer on customer.id = vouchers.issueto
                    left outer join ledger on ledger.id = coupons.ledgerid
                    left outer join referencelist on referencelist.id = vouchers.ibankid
                    where vouchers.isactive=true and vouchers.issuedon>=@fromdate
                    and vouchers.issuedon<@todate 
                    and coupons.companyid=@companyid
                    and vouchers.status >= 0
                    and coupons.id in (select distinct coupenid from vouchers where vouchertype=1 and isactive=true)
                    group by customer.name,ledger.displayname,
                    case when vouchers.ipaymentmode != 3 and vouchers.ipaymentmode != 0 then referencelist.name else 'CASH' end*/

            /*SELECT VOUCHERS.issuedon,(VOUCHERS.amount),
            vouchers.vouchertype,
ledger.DISPLAYNAME,coupons.name,coupons.givento,coupons.entrytype,
case
when coupons.paymentmode = 7 then 'EXPENSE'
when coupons.paymentmode != 3 AND referencelist.ID IS NOT NULL then referencelist.name else 'CASH' end transactiontype
FROM VOUCHERS
left outer join coupons ON coupons.ID = VOUCHERS.coupenid and coupons.isactive = true
left outer join referencelist on referencelist.id = coupons.bankid
left outer join ledger on ledger.id = coupons.ledgerid
where VOUCHERS.isactive = true
and VOUCHERS.issuedon >= '2024-04-01'
and VOUCHERS.issuedon < '2025-04-01'
and VOUCHERtype != 4 */

            //voucher type 2:free,1:paid voucher,4-claim,7-creditnote,8-advance
            if (isdetail)
            {
                query = @"select customer.id,receivedamt amount,customer.name,coupons.issuedon,
                    case 
                    when coupons.paymentmode = 7 then 'EXPENSE'
                    when coupons.paymentmode != 3 AND referencelist.ID IS NOT NULL then referencelist.name else 'CASH' end bankname,
                    ledger.displayname ledgername
                    from coupons
                    inner join customer on customer.id = coupons.customerid
                    left outer join referencelist on referencelist.id = coupons.bankid
                    left outer join ledger on ledger.id = coupons.ledgerid
                    where coupons.isactive=true 
                    and coupons.issuedon>=@fromdate
                    and coupons.companyid=@companyid  
                    and coupons.issuedon<@todate  
                    and coupons.issuemode = 0
                    and coupons.id in (select distinct coupenid from vouchers where vouchertype=1 and isactive=true)
                    UNION ALL 
                    select customer.id,(receivedamt) amount,customer.name,
                    vouchers.ISSUEDON,'CREDIT NOTE' bankname,
                    ledger.displayname ledgername
                    from vouchers
                    inner join coupons on coupons.id = coupenid
                    inner join customer on customer.id = coupons.customerid
                    left outer join referencelist on referencelist.id = coupons.bankid
                    left outer join ledger on ledger.id = coupons.ledgerid
                    where 
					vouchers.isactive=true and vouchers.vouchertype=7 and
					coupons.isactive=true 
					and vouchers.issuedon>=@fromdate
                    and vouchers.issuedon<@todate  
                    and coupons.companyid=@COMPANYID";

                /*

                    union all 

                    select counteramount amount,customer.name,vouchers.issuedon,
                    case when vouchers.ipaymentmode != 3 and vouchers.ipaymentmode != 0 then referencelist.name else 'CASH' end bankname,
                    ledger.displayname ledgername
                    from vouchers
                    inner join coupons on coupons.id = vouchers.coupenid
                    inner join customer on customer.id = vouchers.issueto
                    left outer join ledger on ledger.id = coupons.ledgerid
                    left outer join referencelist on referencelist.id = vouchers.ibankid
                    where vouchers.isactive=true 
                    and vouchers.issuedon>=@fromdate
                    and coupons.companyid=@companyid  
                    and vouchers.issuedon<@todate 
                    and vouchers.status >= 0*/
            }

            /*select * from billsettlementmaster limit 10
with allvoucher as
(
select cast(schemelist->>'amount' as numeric) amount,
	billsettlementmaster.id,schemelist->>'code' vouchercode from billsettlementmaster
LEFT JOIN LATERAL json_array_elements (paymentinfo->'voucherinformation') schemelist ON true	
where cast(paymentinfo->>'voucher_paid' as numeric)>0
and billsettlementmaster.settlementon>=@fromdate
and billsettlementmaster.settlementon<@todate
and billsettlementmaster.companyid=@companyid
)

select customer.name,allvoucher.* from vouchers
inner join allvoucher on allvoucher.vouchercode = vouchers.code
inner join coupons on coupons.id = vouchers.coupenid
left outer join customer on customer.id = customerid
limit 10
*/
            //if (locationid <= 0)
            //    query = query.Replace("", "");

            using (System.Data.IDataReader reader = ExecuteReader(query, param, 1000))
            {
                while (reader.Read())
                {
                    CustomerPaymentSummary invoice = new CustomerPaymentSummary();
                    invoice.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                    invoice.customername = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
                    invoice.ledger = reader["ledgername"] == DBNull.Value ? "" : reader["ledgername"].ToString();
                    invoice.bankname = reader["bankname"] == DBNull.Value ? "" : reader["bankname"].ToString();
                    invoice.amount = reader["amount"] == DBNull.Value ? 0 : (decimal)reader["amount"];
                    if (invoice.bankname == "CREDIT NOTE")
                        invoice.vouchertype = "CREDIT NOTE";
                    else if (invoice.bankname == "EXPENSE")
                        invoice.vouchertype = "EXPENSE";
                    else
                        invoice.bankname = "Gift Voucher Sold";
                    if (isdetail)
                        invoice.date = reader["issuedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["issuedon"]);
                    invoice.iscash = invoice.bankname == "CASH";
                    quantoLedgers.CustomerPayment.Add(invoice);
                }
            }

            query = @"with allvoucher as
                    (
                        select cast(schemelist->>'amount' as numeric) amount,
	                    billsettlementmaster.id,schemelist->>'code' vouchercode from billsettlementmaster
                        LEFT JOIN LATERAL json_array_elements (paymentinfo->'voucherinformation') schemelist ON true	
                        where cast(paymentinfo->>'voucher_paid' as numeric)>0
                        and billsettlementmaster.settlementon>=@fromdate
                        and billsettlementmaster.settlementon<@todate
                        and billsettlementmaster.companyid=@companyid
                    )

                    select ledger.displayname ledgername,sum(allvoucher.amount) amount from vouchers
                    inner join allvoucher on allvoucher.vouchercode = vouchers.code
                    inner join coupons on coupons.id = vouchers.coupenid
                    left outer join ledger on ledger.id = coupons.ledgerid
                    left outer join customer on customer.id = coupons.customerid
                    where vouchers.vouchertype!=4
                    group by ledger.displayname";


            if (isdetail)
            {
                query = @"with allvoucher as
                    (
                        select cast(schemelist->>'amount' as numeric) amount,
	                    billsettlementmaster.id,schemelist->>'code' vouchercode,settlementon from billsettlementmaster
                        LEFT JOIN LATERAL json_array_elements (paymentinfo->'voucherinformation') schemelist ON true	
                        where cast(paymentinfo->>'voucher_paid' as numeric)>0
                        and billsettlementmaster.settlementon>=@fromdate
                        and billsettlementmaster.settlementon<@todate
                        and billsettlementmaster.companyid=@companyid
                    )

                    select ledger.displayname ledgername,customer.name,allvoucher.amount from vouchers
                    inner join allvoucher on allvoucher.vouchercode = vouchers.code
                    inner join coupons on coupons.id = vouchers.coupenid
                    inner join customer on customer.id = vouchers.issueto
                    left outer join ledger on ledger.id = coupons.ledgerid
                    left outer join customer couponuser on customer.id = coupons.customerid";
            }

            using (System.Data.IDataReader reader = ExecuteReader(query, param, 1000))
            {
                while (reader.Read())
                {
                    CustomerPaymentSummary invoice = new CustomerPaymentSummary();
                    invoice.id = id++;
                    invoice.ledger = reader["ledgername"] == DBNull.Value ? "" : reader["ledgername"].ToString();
                    invoice.amount = reader["amount"] == DBNull.Value ? 0 : (decimal)reader["amount"];
                    invoice.vouchertype = "Gift Voucher Adjusted";
                    if (isdetail)
                    {
                        invoice.date = reader["settlementon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["settlementon"]);
                        invoice.customername = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
                    }
                    quantoLedgers.CustomerAdjustment.Add(invoice);
                }
            }

            var customerids = quantoLedgers.CustomerPayment.FindAll(x => x.id > 0).ConvertAll(x => x.id);
            if (customerids.Count > 0)
            {
                using (var reader = ExecuteReader(string.Format(@"
                                    with duplicate as
                                    (
                                    select name from customer
                                    where isactive=true and customertype in (2, 7, 6,8)
                                    and customer.id in ({0})
                                    group by name
                                    having count(*)>1
                                    ) 
                                    select id,customer.name,customer.code from customer
                                    inner join duplicate on duplicate.name = customer.name
                                    where isactive=true and customertype in (2, 7, 6,8)
                                    order by customer.name", string.Join(",", customerids)), null,10000))
                {
                    while (reader.Read())
                    {
                        long customerid = reader.GetInt64(0);
                        string sname = reader.IsDBNull(1) ? "" : reader.GetString(1);
                        string cname = reader.IsDBNull(2) ? "" : reader.GetString(2);
                        if (quantoLedgers.CustomerPayment.Exists(x => x.id == customerid))
                        {
                            quantoLedgers.CustomerPayment.FindAll(x => x.id == customerid).ForEach(y =>
                            {
                                y.customername = sname + "(" + cname + ")";
                            });
                        }
                    }
                }
            }
            return quantoLedgers;
        }

        public QuantoLedgers SearchAgentPaymentSummary(long companyid, long locationid, DateTime from, DateTime to, bool isdetail)
        {
            string query = @"
                            select agent.id id,agent.name,bank.name as bankname,paymentmode,
							sum(tdsamount) tdsamount,sum(rounding) rounding,bank.id bankid,
                            sum(Agentpaymentinvoice.paidamount) paidamount,sum(discountamount) discountamount							
                            from Agentpayment
                            inner join Agentpaymentinvoice on transportpaymentinvoice.Agentpaymentid = Agentpayment.id
                            inner join Agent on agent.id = Agentpayment.agentid
							left outer join referencelist bank on bank.id = Agentpayment.bankid
                            where Agentpayment.isactive=true and Agentpaymentinvoice.isactive = true 
                            and Agentpayment.paymentdate>=@fromdate and Agentpayment.paymentdate<@todate 
                            and Agentpayment.companyid=@companyid and Agentpayment.locationid=@locationid {0}
                            group by agent.id,agent.name,bank.id,bank.name ,paymentmode";

            if (isdetail)
            {
                query = @"
                            select agent.id id,agent.name,bank.name as bankname,paymentmode,Agentpayment.paymentdate,
							sum(tdsamount) tdsamount,sum(rounding) rounding,bank.id bankid,
                            sum(Agentpaymentinvoice.paidamount) paidamount,sum(discountamount) discountamount							
                            from Agentpayment
                            inner join Agentpaymentinvoice on Agentpaymentinvoice.agentpaymentid = Agentpayment.id
                            inner join Agent on agent.id = Agentpayment.agentid
							left outer join referencelist bank on bank.id = Agentpayment.bankid
                            where Agentpayment.isactive=true and Agentpaymentinvoice.isactive = true 
                            and Agentpayment.paymentdate>=@fromdate and Agentpayment.paymentdate<@todate 
                            and Agentpayment.companyid=@companyid and Agentpayment.locationid=@locationid {0}
                            group by agent.id,bank.id,agent.name,bank.name ,paymentmode,Agentpayment.paymentdate";
            }
            SortedList<string, object> param = new SortedList<string, object>();
            param.Add("fromdate", from);
            param.Add("todate", to.AddDays(1));
            param.Add("companyid", companyid);
            param.Add("locationid", locationid);
            if (locationid <= 0)
                query = query.Replace("and Agentpayment.locationid=@locationid", "");

            query = string.Format(query, "");

            List<SupplierPaymentSummary> invoicelist = new List<SupplierPaymentSummary>();

            using (System.Data.IDataReader reader = ExecuteReader(query, param, 1000))
            {
                while (reader.Read())
                {
                    SupplierPaymentSummary invoice = new SupplierPaymentSummary();
                    invoice.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                    invoice.bankid = reader["bankid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["bankid"]);
                    if (isdetail)
                        invoice.paymentdate = reader["paymentdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["paymentdate"]);
                    invoice.suppliercompany = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
                    invoice.bankname = reader["bankname"] == DBNull.Value ? "" : reader["bankname"].ToString();
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
                    invoice.allocations = new List<SupplierPaymentSummary.Allocation>();
                    if (isdetail)
                    {
                        if (invoicelist.Exists(x => x.id == invoice.id && x.paymentdate == invoice.paymentdate))
                        {
                            invoice = invoicelist.Find(x => x.id == invoice.id && x.paymentdate == invoice.paymentdate);
                        }
                        else
                        {
                            invoice.tdsamount = reader["tdsamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["tdsamount"]);
                            invoice.rounding = reader["rounding"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["rounding"]);
                            invoicelist.Add(invoice);
                        }
                    }
                    else
                    {
                        if (invoicelist.Exists(x => x.id == invoice.id))
                        {
                            invoice = invoicelist.Find(x => x.id == invoice.id);
                        }
                        else
                        {
                            invoice.tdsamount = reader["tdsamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["tdsamount"]);
                            invoice.rounding = reader["rounding"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["rounding"]);
                            invoicelist.Add(invoice);
                        }
                    }

                    var isdebitnote = false;
                    if (invoice.allocations.Exists(x => x.isdebitnote == isdebitnote))
                    {
                        var allot = invoice.allocations.Find(x => x.isdebitnote == isdebitnote);
                        allot.paidamount += reader["paidamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["paidamount"]);
                        allot.discountamount += reader["discountamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discountamount"]);
                    }
                    else
                    {
                        invoice.allocations.Add(new TallyDataservice.PaymentInfo.Allocation()
                        {
                            isdebitnote = isdebitnote,
                            paidamount = reader["paidamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["paidamount"]),
                            discountamount = reader["discountamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discountamount"])
                        });
                    }
                }

            }



            return new QuantoLedgers()
            {
                SupplierPayment = invoicelist
            };
        }

        public QuantoLedgers SearchTransportPaymentSummary(long companyid, long locationid, DateTime from, DateTime to, bool isdetail)
        {
            string query = @"
                            select transport.id id,transport.name,bank.name as bankname,paymentmode,
							sum(tdsamount) tdsamount,sum(rounding) rounding,bank.id bankid,
                            sum(transportpaymentinvoice.paidamount) paidamount,sum(discountamount) discountamount							
                            from transportpayment
                            inner join transportpaymentinvoice on transportpaymentinvoice.transportpaymentid = transportpayment.id
                            inner join transport on transport.id = transportpayment.transportid
							left outer join referencelist bank on bank.id = transportpayment.bankid
                            where transportpayment.isactive=true and transportpaymentinvoice.isactive = true 
                            and transportpayment.paymentdate>=@fromdate and transportpayment.paymentdate<@todate 
                            and transportpayment.companyid=@companyid and transportpayment.locationid=@locationid {0}
                            group by transport.id,bank.id,transport.name,bank.name ,paymentmode";

            if (isdetail)
            {
                query = @"
                            select transport.id id,transport.name,bank.name as bankname,paymentmode,transportpayment.paymentdate,
							sum(tdsamount) tdsamount,sum(rounding) rounding,bank.id bank.id,
                            sum(transportpaymentinvoice.paidamount) paidamount,sum(discountamount) discountamount							
                            from transportpayment
                            inner join transportpaymentinvoice on transportpaymentinvoice.transportpaymentid = transportpayment.id
                            inner join transport on transport.id = transportpayment.transportid
							left outer join referencelist bank on bank.id = transportpayment.bankid
                            where transportpayment.isactive=true and transportpaymentinvoice.isactive = true 
                            and transportpayment.paymentdate>=@fromdate and transportpayment.paymentdate<@todate 
                            and transportpayment.companyid=@companyid and transportpayment.locationid=@locationid {0}
                            group by transport.id,bank.id,transport.name,bank.name ,paymentmode,transportpayment.paymentdate";
            }
            SortedList<string, object> param = new SortedList<string, object>();
            param.Add("fromdate", from);
            param.Add("todate", to.AddDays(1));
            param.Add("companyid", companyid);
            param.Add("locationid", locationid);

            query = string.Format(query, "");

            List<SupplierPaymentSummary> invoicelist = new List<SupplierPaymentSummary>();

            using (System.Data.IDataReader reader = ExecuteReader(query, param, 1000))
            {
                while (reader.Read())
                {
                    SupplierPaymentSummary invoice = new SupplierPaymentSummary();
                    invoice.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                    if (isdetail)
                        invoice.paymentdate = reader["paymentdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["paymentdate"]);
                    invoice.suppliercompany = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
                    invoice.bankname = reader["bankname"] == DBNull.Value ? "" : reader["bankname"].ToString();
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
                    invoice.allocations = new List<SupplierPaymentSummary.Allocation>();
                    if (isdetail)
                    {
                        if (invoicelist.Exists(x => x.id == invoice.id && x.paymentdate == invoice.paymentdate))
                        {
                            invoice = invoicelist.Find(x => x.id == invoice.id && x.paymentdate == invoice.paymentdate);
                        }
                        else
                        {
                            invoice.tdsamount = reader["tdsamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["tdsamount"]);
                            invoice.rounding = reader["rounding"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["rounding"]);
                            invoicelist.Add(invoice);
                        }
                    }
                    else
                    {
                        if (invoicelist.Exists(x => x.id == invoice.id))
                        {
                            invoice = invoicelist.Find(x => x.id == invoice.id);
                        }
                        else
                        {
                            invoice.tdsamount = reader["tdsamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["tdsamount"]);
                            invoice.rounding = reader["rounding"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["rounding"]);
                            invoicelist.Add(invoice);
                        }
                    }

                    var isdebitnote = false;
                    if (invoice.allocations.Exists(x => x.isdebitnote == isdebitnote))
                    {
                        var allot = invoice.allocations.Find(x => x.isdebitnote == isdebitnote);
                        allot.paidamount += reader["paidamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["paidamount"]);
                        allot.discountamount += reader["discountamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discountamount"]);
                    }
                    else
                    {
                        invoice.allocations.Add(new TallyDataservice.PaymentInfo.Allocation()
                        {
                            isdebitnote = isdebitnote,
                            paidamount = reader["paidamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["paidamount"]),
                            discountamount = reader["discountamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discountamount"])
                        });
                    }
                }

            }
            return new QuantoLedgers()
            {
                SupplierPayment = invoicelist
            };
        }

        public class TrailBalanceLegerOpening
        {
            public long ledgerid { get; set; }
            public decimal credit { get; set; }
            public decimal debit { get; set; }
            public string coretype { get; set; }
            public int coretypeid { get; set; }
            public string coremode { get; set; }
            public string parentgroup { get; set; }
            private string _displayname { get; set; }
            public string displayname
            {
                get
                {
                    return _displayname;
                }
                set
                {
                    _displayname = value;
                    if (string.IsNullOrEmpty(_displayname))
                        _displayname = "EMPTY";
                }
            }
        }
        public class TrailBalanceLedger
        {
            public string datasource { get; set; }
            public long ledgerid { get; set; }
            public string coretype { get; set; }
            public int coretypeid { get; set; }
            public string coremode { get; set; }
            private string _displayname { get; set; }
            public string displayname
            {
                get
                {
                    return _displayname;
                }
                set
                {
                    _displayname = value;
                    if (string.IsNullOrEmpty(_displayname))
                        _displayname = "EMPTY";
                }
            }
            public string parentgroup { get; set; }
            public string header
            {
                get
                {
                    if (parentgroup == null && coretype == null) return "";
                    if (parentgroup != null && coretype != null)
                        return (parentgroup == coretype || parentgroup.IndexOf(coretype) >= 0) ? parentgroup :
                            (string.IsNullOrEmpty(parentgroup) ? coretype : (coretype + "/" + parentgroup));
                    if (parentgroup != null) return parentgroup;
                    return coretype;
                }
            }
            public decimal debit { get; set; }
            public decimal credit { get; set; }
            public decimal balance { get; set; }
            public string balancemode { get; set; }
            public int itemlevel { get; set; }
            public decimal corebalance { get; set; }
            public decimal OpeningDR { get; set; }
            public decimal OpeningCR { get; set; }
            public decimal ClosingDR { get; set; }
            public decimal ClosingCR { get; set; }
            public bool only4tp { get; set; }
            public bool isposting { get; set; }
            [Newtonsoft.Json.JsonIgnore]
            public bool issupplier { get; set; }
            [Newtonsoft.Json.JsonIgnore]
            public bool iscustomer { get; set; }
            [Newtonsoft.Json.JsonIgnore]
            public long customerid { get; set; }
            [Newtonsoft.Json.JsonIgnore]
            public long supplierid { get; set; }
        }
        public List<TrailBalanceLedger> TrailBalanceLedgers(long companyid, long locationid, int financeyear)
        {
            DateTime fromdate = new DateTime(financeyear, 4, 1);
            DateTime todate = fromdate.AddYears(1);
            return TrailBalanceLedgers(companyid, locationid, fromdate, todate);
        }
        public List<TrailBalanceLedger> TrailBalanceLedgers(long companyid, long locationid, DateTime fromdate, DateTime todate)
        {
            //InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.finance_trace = true;
            //and coalesce(cast(voucher.attributes->>'lrinvoicereturnid' as bigint),0)=0
            string query = @"with allvouchers
as
(
select adjustmentledgerid ledgerid,
(case when vouchertype=4 and sourcetype >=3 and sourcetype<=5 then true else false end) isposting,
sum(case when adjustmentcredit then 0 else amount end) debit,
sum(case when adjustmentcredit then amount else 0 end) as credit from voucher
where voucherdate >=@fromdate AND  voucherdate<@todate and voucher.isactive = true and companyid =@companyid and voucher.locationid =@locationid
--and coalesce(cast(voucher.attributes->>'lrinvoicereturnid' as bigint),0)=0
--and coalesce(cast(voucher.attributes->>'customerinvoiceid' as bigint),0)=0
group by adjustmentledgerid,isposting

union all

select ledgerid,
(case when vouchertype=4 and sourcetype >=3 and sourcetype<=5 then true else false end) isposting,
sum(case when iscredit then 0 else voucheritems.amount end) debit,
sum(case when iscredit then voucheritems.amount else 0 end) as credit from voucher
inner join voucheritems on voucheritems.voucherid = voucher.id and voucheritems.isactive = true
where voucherdate >=@fromdate AND  voucherdate<@todate and voucher.isactive = true and companyid =@companyid and voucher.locationid =@locationid
--and coalesce(cast(voucher.attributes->>'lrinvoicereturnid' as bigint),0)=0
--and coalesce(cast(voucher.attributes->>'customerinvoiceid' as bigint),0)=0
group by ledgerid,isposting
), trailsums
as
(
select ledgerid,isposting,sum(debit) debit,sum(credit) credit from allvouchers
group by ledgerid,isposting
),
detailsums
as
(
select ledgerid,isposting,cast(typelist->>'name' as varchar) CoreType,cast(typelist->>'id' as int) CoreTypeid,cast(typelist->>'mode' as varchar) CoreMode,
        ledger.displayname,ledger.parentgroup,debit,credit,
        case when cast(typelist->>'mode' as varchar)='Dr' then credit-debit else credit-debit end as corebalance from trailsums
left outer join ledger on ledger.id = ledgerid
left outer join json_array_elements('[
            { ""id"": 0, ""name"": ""No Ledger Type"" ,""mode"":""Cr"" },
            { ""id"": 1, ""name"": ""Capital"" ,""mode"":""Cr"" },
            { ""id"": 2, ""name"": ""Fixed asset"" ,""mode"":""Cr"" },
            { ""id"": 3, ""name"": ""Libalities"" ,""mode"":""Cr"" },
            { ""id"": 4, ""name"": ""Direct Income""  ,""mode"":""Cr""},
            { ""id"": 5, ""name"": ""Direct Expense""  ,""mode"":""Cr""},
            { ""id"": 6, ""name"": ""Cash-In-Hand""  ,""mode"":""Cr""},
            { ""id"": 7, ""name"": ""Bank Account"" ,""mode"":""Cr"" },
            { ""id"": 8, ""name"": ""Stock-In-Hand""  ,""mode"":""Cr""},
            { ""id"": 9, ""name"": ""Indirect Incomes""  ,""mode"":""Cr""},
            { ""id"": 10, ""name"": ""Indirect Expenses""  ,""mode"":""Cr""},
            { ""id"": 11, ""name"": ""Sundry Debtors""  ,""mode"":""Cr""},
            { ""id"": 12, ""name"": ""Sundry Creditors""  ,""mode"":""Cr""},
            { ""id"": 13, ""name"": ""Petty Cash""  ,""mode"":""Cr""},
            { ""id"": 14, ""name"": ""Sales""  ,""mode"":""Cr""},
            { ""id"": 15, ""name"": ""Purchase""  ,""mode"":""Cr""},
            { ""id"": 16, ""name"": ""Tax & Duties"" ,""mode"":""Cr"" },
            { ""id"": 17, ""name"": ""Petty Cash Direct Expense"" ,""mode"":""Cr"" },
            { ""id"": 18, ""name"": ""Petty Cash In-Direct Expense"" ,""mode"":""Cr"" },
            { ""id"": 19, ""name"": ""Current asset"" ,""mode"":""Cr"" },
            { ""id"": 20, ""name"": ""Current Liablities"" ,""mode"":""Cr"" },
            { ""id"": 21, ""name"": ""Petty Cash Direct Income"" ,""mode"":""Cr"" },
            { ""id"": 22, ""name"": ""Petty Cash In-Direct Income"" ,""mode"":""Cr"" },
            { ""id"": 30, ""name"": ""Rounding"",""mode"":""Cr""  }]') typelist on cast(typelist->>'id' as bigint) = typeid)

select detailsums.*,abs(corebalance::numeric) balance,
case when corebalance> 0 then coremode else ( case when coremode = 'Cr' then 'Dr' else 'Cr' end) end balancemode,
ledger.supplierid,ledger.customerid
from detailsums
inner join ledger on ledger.id = detailsums.ledgerid";


            /*var param = new SortedList<string, object>();
            param.Add("fromdate", fromdate);
            param.Add("todate", todate.AddDays(1));
            param.Add("companyid", companyid);
            var allsalesdata = ExecuteDataSet(query, param);
            using (System.IO.MemoryStream memoryStream = new System.IO.MemoryStream())
            {
                ClosedXML.Excel.XLWorkbook xLWorkbook = new ClosedXML.Excel.XLWorkbook();
                foreach (System.Data.DataTable table in allsalesdata.Tables)
                {
                    var sheet = xLWorkbook.AddWorksheet(table);
                }
                xLWorkbook.SaveAs(memoryStream);
                //System.IO.File.WriteAllBytes(@"e:\t.xlsx", memoryStream.ToArray());
            }*/


            Data.LedgerDataService ledgerDataService = new Data.LedgerDataService() { Context = this.Context };
            var salesledger = ledgerDataService.Search(new LedgerCreteria()
            {
                typeid = 14
            });

            if (salesledger.Count == 0)
            {
                throw new Exception("Please create a sales ledger");
            }

            if (salesledger.Exists(x => x.eodcashledger))
                salesledger = salesledger.FindAll(x => x.eodcashledger);

            List<TrailBalanceLedger> finalresponse = new List<TrailBalanceLedger>();
            using (NpgsqlConnection conn = CreateConnection())
            {
                try
                {
                    conn.Open();
                    NpgsqlCommand command = GetCommand(locationid <= 0 ? query.Replace(" and voucher.locationid =@locationid", "") : query, conn);
                    command.Parameters.Add("fromdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = fromdate;
                    command.Parameters.Add("todate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = todate.AddDays(1);
                    command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = companyid;
                    command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = locationid;
                    command.CommandTimeout = 1000;
                    using (System.Data.IDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            TrailBalanceLedger obill = new TrailBalanceLedger();
                            obill.ledgerid = reader["ledgerid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["ledgerid"].ToString());
                            obill.balancemode = reader["balancemode"] == DBNull.Value ? "" : reader["balancemode"].ToString();
                            obill.coremode = reader["coremode"] == DBNull.Value ? "" : reader["coremode"].ToString();
                            obill.coretype = reader["coretype"] == DBNull.Value ? "" : reader["coretype"].ToString();
                            obill.coretypeid = reader["CoreTypeid"] == DBNull.Value ? 0 : Convert.ToInt32(reader["CoreTypeid"].ToString());
                            obill.corebalance = reader["corebalance"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["corebalance"].ToString());
                            obill.displayname = reader["displayname"] == DBNull.Value ? "" : reader["displayname"].ToString();

                            obill.parentgroup = reader["parentgroup"] == DBNull.Value ? "" : reader["parentgroup"].ToString();
                            obill.balance = reader["balance"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["balance"].ToString());
                            obill.debit = reader["debit"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["debit"].ToString());
                            obill.credit = reader["credit"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["credit"].ToString());
                            obill.isposting = reader["isposting"] == DBNull.Value ? false : Convert.ToBoolean(reader["isposting"].ToString());
                            obill.datasource = "VOUCHERS";

                            obill.customerid = reader["customerid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["customerid"].ToString());
                            obill.supplierid = reader["supplierid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["supplierid"].ToString());


                            if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.finance_trace)
                            {
                                if (obill.isposting)
                                {
                                    if(obill.displayname == salesledger[0].displayname)
                                        obill.displayname = "VOUCHER - SALES POSTING - " + obill.displayname;
                                    else
                                        obill.displayname = "VOUCHER - " + obill.displayname;
                                }
                                else
                                    obill.displayname = "VOUCHER - " + obill.displayname;
                            }

                            finalresponse.Add(obill);
                        }
                    }
                    conn.Close();
                }
                catch (Exception exp)
                {
                    if (conn != null)
                        conn.Close();
                }
            }


            if (!InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.int_finance)
                return finalresponse;

            string voucherledgername = "";
            var vouchercouponledger = ledgerDataService.Search(new LedgerCreteria()
            {
                tagid = 11
            });
            if (vouchercouponledger.Count == 0)
            {
                voucherledgername = "GIFT VOUCHER";
            }
            else
            {
                voucherledgername = vouchercouponledger[0].displayname;
            }

            var supplierids = finalresponse.FindAll(x => x.supplierid > 0).ConvertAll(x => x.supplierid);
            if (supplierids.Count > 0)
            {
                using (var reader = ExecuteReader(string.Format(@"
                        with suppliername as 
                        (select name from suppliercompany where supplierid in ({0}) and isactive=true),
                        duplicatename as (
                        select suppliercompany.name from suppliercompany
                        inner join suppliername on suppliername.name = suppliercompany.name
                        group by suppliercompany.name
                        having count(*)>1)
                        select supplierid,suppliercompany.name,city.name cityname from suppliercompany 
                        inner join duplicatename on duplicatename.name=suppliercompany.name
                        left outer join referencelist city on city.id=cityid
                        where suppliercompany.isactive=true", string.Join(",", supplierids)), null,10000))
                {
                    while (reader.Read())
                    {
                        long id = reader.GetInt64(0);
                        string sname = reader.IsDBNull(1) ? "" : reader.GetString(1);
                        string cname = reader.IsDBNull(2) ? "" : reader.GetString(2);
                        if (finalresponse.Exists(x => x.supplierid == id))
                        {
                            finalresponse.Find(x => x.supplierid == id).issupplier = true;
                            if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.finance_trace)
                                finalresponse.Find(x => x.supplierid == id).displayname = "VOUCHER - " + sname + "(" + cname + ")";
                            else
                                finalresponse.Find(x => x.supplierid == id).displayname = sname + "(" + cname + ")";
                        }
                    }
                }
            }


            var customerids = finalresponse.FindAll(x => x.customerid > 0).ConvertAll(x => x.customerid);
            if (customerids.Count > 0)
            {
                using (var reader = ExecuteReader(string.Format(@"
                                    with duplicate as
                                    (
                                    select name from customer
                                    where isactive=true and customertype in (2, 7, 6,8)
                                    and customer.id in ({0})
                                    group by name
                                    having count(*)>1
                                    ) 
                                    select id,customer.name,customer.code from customer
                                    inner join duplicate on duplicate.name = customer.name
                                    where isactive=true and customertype in (2, 7, 6,8)
                                    order by customer.name", string.Join(",", customerids)), null,10000))
                {
                    while (reader.Read())
                    {
                        long id = reader.GetInt64(0);
                        string sname = reader.IsDBNull(1) ? "" : reader.GetString(1);
                        string cname = reader.IsDBNull(2) ? "" : reader.GetString(2);
                        if (finalresponse.Exists(x => x.customerid == id))
                        {
                            if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.finance_trace)
                                finalresponse.Find(x => x.customerid == id).displayname = "VOUCHER - " + sname + "(" + cname + ")";
                            else
                                finalresponse.Find(x => x.customerid == id).displayname = sname + "(" + cname + ")";
                        }
                    }
                }
            }


            //filterledger.AddRange(finalresponse.FindAll(x => x.displayname.Contains("REGENT CREATION PVT LTD")));

            var response = new List<TrailBalanceLedger>();
            var quantoledgers = SearchSalesLedgerSummary(companyid, locationid, fromdate, todate);
            //quantoledgers.BillList.ForEach(x =>
            //{
            //    if (x.IsCreditNote || x.IsCredit)
            //    {
            //        response.Add(new TrailBalanceLedger()
            //        {
            //            coremode = "Cr",
            //            coretype = "Sundry Debtors",
            //            coretypeid = 1,
            //            debit = x.Amount,
            //            datasource = "Sales",
            //            displayname = string.IsNullOrEmpty(x.CustomerName) ? "Customer Missing" : x.CustomerName
            //        });
            //    }
            //});

            //quantoledgers.CardSalesList.ForEach(x =>
            //{
            //    response.Add(new TrailBalanceLedger()
            //    {
            //        coremode = "Cr",
            //        coretype = "Bank Account",
            //        coretypeid = 7,
            //        debit = x.amount,
            //        displayname = String.IsNullOrEmpty(x.bank) ? "Bank" : x.bank
            //    });
            //});



            //var cashsales = quantoledgers.BillList.Sum(x => (x.IsCredit || x.IsCreditNote) ? 0 : x.Amount) - quantoledgers.CardSalesList.Sum(x => x.amount);
            //response.Add(new TrailBalanceLedger()
            //{
            //    coremode = "Cr",
            //    coretype = "Cash-In-Hand",
            //    coretypeid = 6,
            //    debit = cashsales,
            //    displayname = "Cash"
            //});


            TaxDataService taxDataService = new TaxDataService() { Context = this.Context };
            var inventorytaxlist = taxDataService.Search(new TaxCreteria());

            var alltaxledgers = ledgerDataService.Search(new LedgerCreteria()
            {
                typeid = 16
            });

            var allsalestax = new List<Tuple<long, long>>();
            var allpurchasetax = new List<Tuple<long, long>>();

            alltaxledgers.FindAll(x => x.attributes != null &&
                x.attributes.invstaxids != null).ForEach(x =>
                {
                    allsalestax.AddRange(x.attributes.invstaxids.ConvertAll(y => new Tuple<long, long>(y, x.id)));
                });

            alltaxledgers.FindAll(x => x.attributes != null &&
                x.attributes.invptaxids != null).ForEach(x =>
                {
                    allpurchasetax.AddRange(x.attributes.invptaxids.ConvertAll(y => new Tuple<long, long>(y, x.id)));
                });


            /*var creditamount = quantoledgers.TaxList.Sum(x => x.IsCredit ? x.amount : 0);
            var creditnoteamount = quantoledgers.TaxList.Sum(x => x.IsCreditNote ? x.amount : 0);
            var creditbillamount = quantoledgers.BillList.Sum(x => x.IsCredit ? x.Amount : 0);
            var creditnotebillamount = quantoledgers.BillList.Sum(x => x.IsCreditNote ? x.Amount : 0);*/


            quantoledgers.VoucherSalesList.ForEach(x =>
            {
                response.Add(new TrailBalanceLedger()
                {
                    coremode = "Dr",
                    coretype = "Voucher Adjustment",
                    coretypeid = 7,
                    isposting = true,
                    debit = x.amount,
                    displayname = (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.finance_trace ? "GV - " : "") + voucherledgername,
                    datasource = "Sales",
                });
                response.Add(new TrailBalanceLedger()
                {
                    coremode = "Cr",
                    coretype = "Sales",
                    coretypeid = 14,
                    credit = x.amount,
                    isposting = true,
                    displayname = (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.finance_trace ? "GV - SALES POSTING - " : "") + salesledger[0].displayname,
                    datasource = "Sales"
                });
            });

            if (quantoledgers.B2BSalesList != null && quantoledgers.B2BSalesList.Count > 0)
            {

                foreach (var b2bitm in quantoledgers.B2BSalesList)
                {
                    response.Add(new TrailBalanceLedger()
                    {
                        coremode = "Cr",
                        coretype = "Sales",
                        coretypeid = 14,
                        credit = b2bitm.amount,
                        only4tp = true,
                        datasource = "Sales",
                        displayname = b2bitm.bank
                    });
                }

                //response.Add(new TrailBalanceLedger()
                //{
                //    coremode = "Dr",
                //    coretype = "Sales",
                //    coretypeid = 14,
                //    debit = quantoledgers.B2BSalesList.Sum(X => X.amount),
                //    datasource = "Sales",
                //    displayname = "B2B to Cash Sales"
                //});


                //response.Add(new TrailBalanceLedger()
                //{
                //    coremode = "Cr",
                //    coretype = "Sales",
                //    coretypeid = 14,
                //    credit = quantoledgers.B2BSalesList.Sum(X => X.amount),
                //    datasource = "Sales",
                //    displayname = "B2B to Cash Sales"
                //});

                response.Add(new TrailBalanceLedger()
                {
                    coremode = "Dr",
                    coretype = "Sales",
                    coretypeid = 14,
                    debit = quantoledgers.B2BSalesList.Sum(X => X.amount),
                    datasource = "Sales",
                    displayname = (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.finance_trace ? " SALES POSTING - B2B CASH ADJUSTMENT - " : "") + "Sales"
                });
            }

            if (quantoledgers.Unsettled != null && quantoledgers.Unsettled.Count > 0)
            {
                if (quantoledgers.Unsettled.Exists(x => !x.isupi))
                {
                    response.Add(new TrailBalanceLedger()
                    {
                        coremode = "Dr",
                        coretype = "Sales",
                        coretypeid = 14,
                        credit = quantoledgers.Unsettled.Sum(x => !x.isupi ? x.amount : 0),
                        only4tp = true,
                        datasource = "Sales",
                        displayname = (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.finance_trace ? " SALES POSTING - UNSETTLED CASH ADJUSTMENT - " : "") + salesledger[0].displayname
                    });

                    response.Add(new TrailBalanceLedger()
                    {
                        coremode = "Cr",
                        coretype = "Sales",
                        coretypeid = 14,
                        debit = quantoledgers.Unsettled.Sum(x => !x.isupi ? x.amount : 0),
                        datasource = "Sales",
                        displayname = "Unsettled Sales"
                    });
                }

                if (quantoledgers.Unsettled.Exists(x => x.isupi))
                {
                    response.Add(new TrailBalanceLedger()
                    {
                        coremode = "Dr",
                        coretype = "Sales",
                        coretypeid = 14,
                        credit = quantoledgers.Unsettled.Sum(x=>x.isupi ? x.amount:0),
                        only4tp = true,
                        datasource = "Sales",
                        displayname = (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.finance_trace ? " SALES POSTING - B2C SALES TO CREDIT - " : "") + salesledger[0].displayname
                    });

                    response.Add(new TrailBalanceLedger()
                    {
                        coremode = "Cr",
                        coretype = "Sales",
                        coretypeid = 14,
                        debit = quantoledgers.Unsettled.Sum(x => x.isupi ? x.amount : 0),
                        datasource = "Sales",
                        displayname = "B2C Sales to Credit"
                    });
                }


            }

            quantoledgers.TaxList.ForEach(x =>
            {

                //response.Clear();
                if (x.IsCreditNote || x.IsCredit)
                {
                    response.Add(new TrailBalanceLedger()
                    {
                        coremode = "Cr",
                        coretype = "Sundry Debtors",
                        coretypeid = 1,
                        debit = x.amount,
                        datasource = "Sales",
                        displayname = string.IsNullOrEmpty(x.Customer) ? "Customer Missing" : x.Customer
                    });
                }
                if (x.IsCredit)
                {
                    response.Add(new TrailBalanceLedger()
                    {
                        coremode = "Cr",
                        coretype = "Sales",
                        coretypeid = 14,
                        credit = x.amount - x.tax,
                        displayname = "B2B " + " Sales @" + x.taxpercentage.ToString("N2") + "%",
                        datasource = "Sales"
                    });
                }
                else if (x.IsCreditNote)
                {
                    response.Add(new TrailBalanceLedger()
                    {
                        coremode = "Cr",
                        coretype = "Sales",
                        coretypeid = 14,
                        credit = x.amount - x.tax,
                        datasource = "Sales",
                        displayname = "B2B " + " Sales @" + x.taxpercentage.ToString("N2") + "%"
                    });
                }
                else
                {
                    response.Add(new TrailBalanceLedger()
                    {
                        coremode = "Dr",
                        coretype = "Sales",
                        coretypeid = 14,
                        debit = x.amount,
                        only4tp = true,
                        datasource = "Sales",
                        displayname = (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.finance_trace ? " SALES POSTING - DIRECT SALES TAX SPLIT - " : "") + salesledger[0].displayname
                    });


                    response.Add(new TrailBalanceLedger()
                    {
                        coremode = "Cr",
                        coretype = "Sales",
                        coretypeid = 14,
                        credit = x.amount - x.tax,
                        datasource = "Sales",
                        displayname = "Direct Sales @" + x.taxpercentage.ToString("N2") + "%"
                    });
                }

                if (x.IsInterstate)
                {
                    var percentagestring = ((x.taxpercentage) % 1 != 0) ? (x.taxpercentage).ToString("N2") : (x.taxpercentage).ToString("N0");
                    response.Add(new TrailBalanceLedger()
                    {
                        coremode = "Cr",
                        coretype = "Tax & Duties",
                        coretypeid = 16,
                        credit = x.tax,
                        datasource = "Sales",
                        displayname = "OUTPUT IGST @" + percentagestring + "%"
                    });
                }
                else
                {
                    var percentagestring = ((x.taxpercentage / 2) % 1 != 0) ? (x.taxpercentage / 2).ToString("N2") : (x.taxpercentage / 2).ToString("N0");
                    response.Add(new TrailBalanceLedger()
                    {
                        coremode = "Cr",
                        coretype = "Tax & Duties",
                        coretypeid = 16,
                        credit = x.tax / 2,
                        datasource = "Sales",
                        displayname = "OUTPUT CGST @" + percentagestring + "%"
                    });
                    response.Add(new TrailBalanceLedger()
                    {
                        coremode = "Cr",
                        coretype = "Tax & Duties",
                        coretypeid = 16,
                        credit = x.tax / 2,
                        datasource = "Sales",
                        displayname = "OUTPUT SGST @" + percentagestring + "%"
                    });
                }

                /*var debit = response.Sum(z => z.debit);
                var credit = response.Sum(z => z.credit);
                var round = Math.Abs(debit - credit);
                if(round>100)
                {
                    response.Clear();
                }*/
            });


            quantoledgers.AdditionalCharges.ForEach(x =>
            {
                //response.Clear();
                if (!string.IsNullOrEmpty(x.customer))
                {
                    response.Add(new TrailBalanceLedger()
                    {
                        coremode = "Dr",
                        iscustomer = true,
                        coretype = "Sundry Debtors",
                        coretypeid = 14,
                        debit = x.charge + x.tax,
                        displayname = x.customer,
                        datasource = "Sales"
                    });
                    response.Add(new TrailBalanceLedger()
                    {
                        coremode = "Cr",
                        coretype = "Additional Charges",
                        coretypeid = 14,
                        credit = x.charge,
                        displayname = x.chargename,
                        datasource = "Sales"
                    });
                }
                else
                {
                    response.Add(new TrailBalanceLedger()
                    {
                        coremode = "Dr",
                        coretype = "Sundry Debtors",
                        coretypeid = 14,
                        debit = x.charge + x.tax,
                        displayname = "Bill Additional Charges",
                        datasource = "Sales"
                    });
                    response.Add(new TrailBalanceLedger()
                    {
                        coremode = "Dr",
                        coretype = "Additional Charges",
                        coretypeid = 14,
                        credit = x.charge,
                        displayname = x.chargename,
                        datasource = "Sales"
                    });
                }

                if (x.tax != 0)
                {
                    if (x.IsInterstate)
                    {
                        var percentagestring = ((x.taxpercentage) % 1 != 0) ? (x.taxpercentage).ToString("N2") : (x.taxpercentage).ToString("N0");
                        response.Add(new TrailBalanceLedger()
                        {
                            coremode = "Cr",
                            coretype = "Tax & Duties",
                            coretypeid = 16,
                            credit = x.tax,
                            datasource = "Sales",
                            displayname = "OUTPUT IGST @" + percentagestring + "%"
                            //displayname = "OUTPUT " + (string.IsNullOrEmpty(x.customer) ? " CHARGES " : x.customer) + " IGST @" + x.taxpercentage.ToString("N2")
                        });
                    }
                    else
                    {
                        var percentagestring = ((x.taxpercentage / 2) % 1 != 0) ? (x.taxpercentage / 2).ToString("N2") : (x.taxpercentage / 2).ToString("N0");
                        response.Add(new TrailBalanceLedger()
                        {
                            coremode = "Cr",
                            coretype = "Tax & Duties",
                            coretypeid = 16,
                            credit = x.tax / 2,
                            datasource = "Sales",
                            displayname = "OUTPUT CGST @" + percentagestring + "%"
                        });
                        response.Add(new TrailBalanceLedger()
                        {
                            coremode = "Cr",
                            coretype = "Tax & Duties",
                            coretypeid = 16,
                            credit = x.tax / 2,
                            datasource = "Sales",
                            displayname = "OUTPUT SGST @" + percentagestring + "%"
                        });
                    }
                }
                /*var debit = response.Sum(z => z.debit);
                var credit = response.Sum(z => z.credit);
                var round = Math.Abs(debit - credit);
                if (round > 100)
                {
                    response.Clear();
                }*/
            });


            //var log = response.ConvertAll(x => string.Format("{0}\t{1}\t{2}", x.displayname, x.credit, x.debit));
            //System.IO.File.WriteAllLines(@"e:\t.txt", log);
            //finalresponse.Clear();
            CheckCrDr(response, finalresponse, "Sales", 14, "Rounding");

            //return finalresponse

            quantoledgers = CreateCreditPaymentVouchers(companyid, locationid, fromdate, todate, false);

            quantoledgers.CustomerPayment.ForEach(x =>
            {
                //if (x.allocations.Sum(y => y.paid) > 0)
                {
                    response.Add(new TrailBalanceLedger()
                    {
                        coremode = "Dr",
                        coretype = "Sundry Debtors",
                        coretypeid = 1,
                        iscustomer = true,
                        credit = x.allocations.Sum(y => y.paid + y.discount),
                        datasource = "Customer Payment",
                        displayname = x.customername
                    });
                }

                if (x.iscash)
                {
                    response.Add(new TrailBalanceLedger()
                    {
                        coremode = "Cr",
                        coretype = "Cash-In-Hand",
                        coretypeid = 6,
                        datasource = "Customer Payment",
                        debit = x.allocations.Sum(y => y.paid),
                        displayname = "Cash"
                    });
                }
                else
                {
                    response.Add(new TrailBalanceLedger()
                    {
                        coremode = "Cr",
                        coretype = "Bank Account",
                        coretypeid = 7,
                        datasource = "Customer Payment",
                        debit = x.allocations.Sum(y => y.paid),
                        displayname = String.IsNullOrEmpty(x.bankname) ? "Cucstomer Payment Bank Missing" : x.bankname
                    });
                }

                if (x.allocations.Sum(y => y.discount) != 0)
                {
                    response.Add(new TrailBalanceLedger()
                    {
                        coremode = "Cr",
                        coretype = "Sales",
                        coretypeid = 1,
                        datasource = "Customer Payment",
                        debit = x.allocations.Sum(y => y.discount),
                        displayname = "Customer Paymemt Discount"
                    });
                }

            });


            CheckCrDr(response, finalresponse, "Credit Payment", 14, "Rounding");
            quantoledgers = SearchCustomerOrderAndVouchers(companyid, locationid, fromdate, todate, false);
            quantoledgers.CustomerPayment.GroupBy(x => new { x.ledger }).ForEach(y =>
            {
                response.Add(new TrailBalanceLedger()
                {
                    coremode = "Dr",
                    coretype = "Sundry Creditor",
                    coretypeid = 1,
                    datasource = "Customer Order",
                    credit = y.Sum(x => x.amount),
                    displayname = (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.finance_trace ? " GV -" : "") + voucherledgername
                });
                response.Add(new TrailBalanceLedger()
                {
                    coremode = "Cr",
                    coretype = "Sundry Debitor",
                    coretypeid = 6,
                    datasource = "Customer Order",
                    debit = y.Sum(x => x.amount),
                    displayname =
                    (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.finance_trace ? " GV -" : "") + (string.IsNullOrEmpty(y.Key.ledger) ? "Customer Order Payment" : y.Key.ledger)
                });
            });
            var creditnotecouponvalue = quantoledgers.CustomerPayment.FindAll(x => x.vouchertype == "CREDIT NOTE").Sum(X => X.amount);
            response.Add(new TrailBalanceLedger()
            {
                coremode = "Cr",
                coretype = "Sundry Creditor",
                coretypeid = 1,
                datasource = "Voucher Claim",
                credit = creditnotecouponvalue,
                displayname = (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.finance_trace ? " GV - CREDIT NOTE - " : "") + voucherledgername
            });
            response.Add(new TrailBalanceLedger()
            {
                coremode = "Dr",
                coretype = "Sundry Debitor",
                coretypeid = 6,
                datasource = "Voucher Claim",
                debit = creditnotecouponvalue,
                displayname = (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.finance_trace ? " GV - CREDIT NOTE- " : "") + "B2C SALES RETURN"
            });

            var expensecouponvalue = quantoledgers.CustomerPayment.FindAll(x => x.vouchertype == "EXPENSE").Sum(X => X.amount);
            response.Add(new TrailBalanceLedger()
            {
                coremode = "Cr",
                coretype = "Sundry Creditor",
                coretypeid = 1,
                datasource = "Voucher Claim",
                credit = creditnotecouponvalue,
                displayname = (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.finance_trace ? " GV - EXPENSE - " : "") + voucherledgername
            });
            response.Add(new TrailBalanceLedger()
            {
                coremode = "Dr",
                coretype = "Indirect Expense",
                coretypeid = 10,
                datasource = "Voucher Claim",
                debit = creditnotecouponvalue,
                displayname = (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.finance_trace ? " GV - EXPENSE -" : "") + "EXPENSE VOUCHER"
            });

            quantoledgers.CustomerAdjustment.FindAll(x => x.vouchertype != "CREDIT NOTE" && x.vouchertype != "EXPENSE").GroupBy(x => new { x.ledger, x.bankname }).ForEach(y =>
            {
                response.Add(new TrailBalanceLedger()
                {
                    coremode = "Cr",
                    coretype = "Sundry Creditor",
                    coretypeid = 1,
                    datasource = "Voucher Claim",
                    credit = y.Sum(x => x.amount),
                    displayname = (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.finance_trace ? " GV - SALES ADJUSTMENT - " : "") + voucherledgername
                });
                response.Add(new TrailBalanceLedger()
                {
                    coremode = "Dr",
                    coretype = "Sundry Debitor",
                    coretypeid = 6,
                    datasource = "Voucher Claim",
                    debit = y.Sum(x => x.amount),
                    displayname = (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.finance_trace ? " GV  - SALES ADJUSTMENT  - " : "") + 
                    (string.IsNullOrEmpty(y.Key.ledger) ? voucherledgername+" Claimed Customer": y.Key.ledger)
                });
            });

            response.Clear();
            //CheckCrDr(response, finalresponse, "Credit Payment", 14, "Rounding");


            quantoledgers = SearchPurchaseLedgers(companyid, fromdate, todate, false);


            //var conflictrows = quantoledgers.PurchaseLegers.FindAll(x => Math.Abs(x.totalamount - x.taxledgersummary.Sum(y => y.taxable + y.tax))>10);
            //var sconflictrows = quantoledgers.PurchaseLegers.FindAll(x => (x.totalamount-x.additionalcharges+x.additionaldiscount) != x.taxledgersummary.Sum(y => y.taxable + y.tax));
            //var nonadjustedtaxes = quantoledgers.TaxLegers.FindAll(x => !x.adjusted);

            //var suppliervalues = quantoledgers.PurchaseLegers.FindAll(x => x.additionalcharges != 0);
            //var sid = suppliervalues[0].suppliercompanyid;

            //var sid = quantoledgers.TaxLegers.FindAll(x => x.taxname != "GST")[0].suppliercompanyid;
            //quantoledgers.PurchaseLegers.RemoveAll(x => x.suppliercompanyid != sid);
            //quantoledgers.TaxLegers.RemoveAll(x => x.suppliercompanyid != sid);

            //suppliervalues = quantoledgers.PurchaseLegers.FindAll(x => x.additionalcharges != 0);
            //sid = suppliervalues[0].suppliercompanyid;
            //suppliertaxvalues = quantoledgers.TaxLegers.FindAll(x => x.suppliercompanyid == sid);

            //var nonadjustedtaxes = quantoledgers.TaxLegers.FindAll(x => x.taxname == "TCS");

            
            quantoledgers.PurchaseLegers.ForEach(x =>
            {
                //response.Clear();
                response.Add(new TrailBalanceLedger()
                {
                    coremode = "Cr",
                    coretype = "Sundry Creditors",
                    coretypeid = 1,
                    credit = x.totalamount,
                    datasource = "Purchase",
                    issupplier = true,
                    supplierid = x.suppliercompanyid,
                    displayname = string.IsNullOrEmpty(x.suppliercompany) ? "Missing Supplier" : x.suppliercompany
                });

                if (x.additionaldiscount > 0)
                {
                    response.Add(new TrailBalanceLedger()
                    {
                        coremode = "Cr",
                        coretype = "Purchase",
                        coretypeid = 15,
                        credit = x.additionaldiscount,
                        datasource = "Purchase",
                        displayname = "Purchase Discount"
                    });
                }
                if (x.additionalcharges > 0)
                {
                    response.Add(new TrailBalanceLedger()
                    {
                        coremode = "Dr",
                        coretype = "Purchase",
                        coretypeid = 15,
                        debit = x.additionalcharges,
                        datasource = "Purchase",
                        displayname = "Purchase Charges"
                    });
                }

                x.taxledgersummary.ForEach(y =>
                {

                    if (y.taxname != "GST")
                    {
                        var percentagestring = ((y.percentage) % 1 != 0) ? (y.percentage).ToString("N2") : (y.percentage).ToString("N0");
                        if (y.taxname != "TCS")
                        {
                            response.Add(new TrailBalanceLedger()
                            {
                                coremode = "Cr",
                                coretype = "Purchase",
                                coretypeid = 15,
                                debit = y.taxable,
                                datasource = "Purchase",
                                displayname = y.taxname + " @" + percentagestring + "%"
                            });

                            if (y.isinterstatesale)
                            {
                                percentagestring = ((y.percentage) % 1 != 0) ? (y.percentage).ToString("N2") : (y.percentage).ToString("N0");
                                response.Add(new TrailBalanceLedger()
                                {
                                    coremode = "Dr",
                                    coretype = "Tax & Duties",
                                    coretypeid = 16,
                                    debit = y.tax,
                                    datasource = "Purchase",
                                    displayname = "INPUT IGST @" + percentagestring + "%"
                                });
                            }
                            else
                            {
                                percentagestring = ((y.percentage / 2) % 1 != 0) ? (y.percentage / 2).ToString("N2") : (y.percentage / 2).ToString("N0");
                                response.Add(new TrailBalanceLedger()
                                {
                                    coremode = "Dr",
                                    coretype = "Tax & Duties",
                                    coretypeid = 16,
                                    debit = y.tax / 2,
                                    datasource = "Purchase",
                                    displayname = "INPUT CGST @" + percentagestring + "%"
                                });
                                response.Add(new TrailBalanceLedger()
                                {
                                    coremode = "Dr",
                                    coretype = "Tax & Duties",
                                    coretypeid = 16,
                                    debit = y.tax / 2,
                                    datasource = "Purchase",
                                    displayname = "INPUT SGST @" + percentagestring + "%"
                                });
                            }
                        }
                        else
                        {

                            response.Add(new TrailBalanceLedger()
                            {
                                coremode = "Dr",
                                coretype = "Tax & Duties",
                                coretypeid = 16,
                                debit = y.tax,
                                datasource = "Purchase",
                                displayname = y.taxname + " @" + percentagestring + "%"
                            });
                        }


                    }
                    else
                    {
                        var percentagestring = ((y.percentage) % 1 != 0) ? (y.percentage).ToString("N2") : (y.percentage).ToString("N0");
                        response.Add(new TrailBalanceLedger()
                        {
                            coremode = "Dr",
                            coretype = "Purchase",
                            coretypeid = 15,
                            debit = y.taxable,
                            datasource = "Purchase",
                            displayname = "Purchase @" + percentagestring + "%"
                        });

                        if (x.isinterstatesale)
                        {
                            percentagestring = ((y.percentage) % 1 != 0) ? (y.percentage).ToString("N2") : (y.percentage).ToString("N0");
                            response.Add(new TrailBalanceLedger()
                            {
                                coremode = "Dr",
                                coretype = "Tax & Duties",
                                coretypeid = 16,
                                debit = y.tax,
                                datasource = "Purchase",
                                displayname = "INPUT IGST @" + percentagestring + "%"
                            });
                        }
                        else
                        {
                            percentagestring = ((y.percentage / 2) % 1 != 0) ? (y.percentage / 2).ToString("N2") : (y.percentage / 2).ToString("N0");
                            response.Add(new TrailBalanceLedger()
                            {
                                coremode = "Dr",
                                coretype = "Tax & Duties",
                                coretypeid = 16,
                                debit = y.tax / 2,
                                datasource = "Purchase",
                                displayname = "INPUT CGST @" + percentagestring + "%"
                            });
                            response.Add(new TrailBalanceLedger()
                            {
                                coremode = "Dr",
                                coretype = "Tax & Duties",
                                coretypeid = 16,
                                debit = y.tax / 2,
                                datasource = "Purchase",
                                displayname = "INPUT SGST @" + percentagestring + "%"
                            });
                        }
                    }

                });


                var credit = response.Sum(z => z.credit);
                var debite = response.Sum(z => z.debit);
                var rounding = Math.Abs(credit - debite);
                //if (rounding > 100)
                //{
                //    response.Clear();
                //}
            });

            //quantoledgers.TaxLegers.ForEach(x =>
            //{

            //    if (x.taxname != "GST")
            //    {
            //        //response.Add(new TrailBalanceLedger()
            //        //{
            //        //    coremode = "Cr",
            //        //    coretype = "Purchase",
            //        //    coretypeid = 15,
            //        //    debit = x.taxable,
            //        //    datasource = "Purchase",
            //        //    displayname = "Purchase " + x.taxname + " @" + x.percentage.ToString("N2")
            //        //});
            //        var percentagestring = ((x.percentage) % 1 != 0) ? (x.percentage).ToString("N2") : (x.percentage).ToString("N0");
            //        response.Add(new TrailBalanceLedger()
            //        {
            //            coremode = "Dr",
            //            coretype = "Tax & Duties",
            //            coretypeid = 16,
            //            debit = x.tax,
            //            datasource = "Purchase",
            //            displayname = x.taxname + " @" + percentagestring + "%"
            //        });

            //    }
            //    else
            //    {
            //        response.Add(new TrailBalanceLedger()
            //        {
            //            coremode = "Dr",
            //            coretype = "Purchase",
            //            coretypeid = 15,
            //            debit = x.taxable,
            //            datasource = "Purchase",
            //            displayname = "Purchase @" + x.percentage.ToString("N2")
            //        });

            //        if (x.isinterstatesale)
            //        {
            //            var percentagestring = ((x.percentage) % 1 != 0) ? (x.percentage).ToString("N2") : (x.percentage).ToString("N0");
            //            response.Add(new TrailBalanceLedger()
            //            {
            //                coremode = "Dr",
            //                coretype = "Tax & Duties",
            //                coretypeid = 16,
            //                debit = x.tax,
            //                datasource = "Purchase",
            //                displayname = "INPUT IGST @" + percentagestring + "%"
            //            });
            //        }
            //        else
            //        {
            //            var percentagestring = ((x.percentage / 2) % 1 != 0) ? (x.percentage / 2).ToString("N2") : (x.percentage / 2).ToString("N0");


            //            response.Add(new TrailBalanceLedger()
            //            {
            //                coremode = "Dr",
            //                coretype = "Tax & Duties",
            //                coretypeid = 16,
            //                debit = x.tax / 2,
            //                datasource = "Purchase",
            //                displayname = "INPUT CGST @" + percentagestring + "%"
            //            });
            //            response.Add(new TrailBalanceLedger()
            //            {
            //                coremode = "Dr",
            //                coretype = "Tax & Duties",
            //                coretypeid = 16,
            //                debit = x.tax / 2,
            //                datasource = "Purchase",
            //                displayname = "INPUT SGST @" + percentagestring + "%"
            //            });
            //        }
            //    }


            //});


            CheckCrDr(response, finalresponse, "Purchase", 15, "Rounding");
            quantoledgers = SearchJobWorkLedgers(companyid, fromdate, todate, false);

            quantoledgers.PurchaseLegers.ForEach(x =>
            {
                //response.Clear();
                response.Add(new TrailBalanceLedger()
                {
                    coremode = "Cr",
                    coretype = "Sundry Creditors",
                    coretypeid = 1,
                    credit = x.totalamount,
                    datasource = "Job Work",
                    issupplier = true,
                    supplierid = x.suppliercompanyid,
                    displayname = string.IsNullOrEmpty(x.suppliercompany) ? "Missing Supplier" : x.suppliercompany
                });

                x.taxledgersummary.ForEach(y =>
                {

                    var percentagestring = ((y.percentage) % 1 != 0) ? (y.percentage).ToString("N2") : (y.percentage).ToString("N0");
                    response.Add(new TrailBalanceLedger()
                    {
                        coremode = "Dr",
                        coretype = "Job Work",
                        coretypeid = 15,
                        debit = y.taxable,
                        datasource = "Job Work",
                        displayname = "Job Work @" + percentagestring + "%"
                    });

                    if (x.isinterstatesale)
                    {
                        percentagestring = ((y.percentage) % 1 != 0) ? (y.percentage).ToString("N2") : (y.percentage).ToString("N0");
                        response.Add(new TrailBalanceLedger()
                        {
                            coremode = "Dr",
                            coretype = "Tax & Duties",
                            coretypeid = 16,
                            debit = y.tax,
                            datasource = "Job Work",
                            displayname = "INPUT IGST @" + percentagestring + "%"
                        });
                    }
                    else
                    {
                        percentagestring = ((y.percentage / 2) % 1 != 0) ? (y.percentage / 2).ToString("N2") : (y.percentage / 2).ToString("N0");
                        response.Add(new TrailBalanceLedger()
                        {
                            coremode = "Dr",
                            coretype = "Tax & Duties",
                            coretypeid = 16,
                            debit = y.tax / 2,
                            datasource = "Purchase",
                            displayname = "INPUT CGST @" + percentagestring + "%"
                        });
                        response.Add(new TrailBalanceLedger()
                        {
                            coremode = "Dr",
                            coretype = "Tax & Duties",
                            coretypeid = 16,
                            debit = y.tax / 2,
                            datasource = "Purchase",
                            displayname = "INPUT SGST @" + percentagestring + "%"
                        });
                    }

                });
            });


            CheckCrDr(response, finalresponse, "Job Work", 15, "Rounding");

            quantoledgers = SupplierReturnLedger(companyid, fromdate, todate, false);
            quantoledgers.PurchaseReturn.RemoveAll(x => x.returntype == 7);
            List<Tuple<long, decimal>> cnf = new List<Tuple<long, decimal>>();
            quantoledgers.PurchaseReturn.ForEach(x =>
            {

                response.Add(new TrailBalanceLedger()
                {
                    coremode = "Cr",
                    coretype = "Sundry Creditors",
                    coretypeid = 1,
                    issupplier = true,
                    supplierid = x.suppliercompanyid,
                    debit = x.returntype < 10 ? x.totalamount : 0,
                    credit = x.returntype >= 10 ? x.totalamount : 0,
                    datasource = x.returntype == 7 ? "Supplier Advance" : (x.returntype >= 10 ? "Excess Note" : "Debit Note"),
                    displayname = string.IsNullOrEmpty(x.suppliercompany) ? "Supplier Missing" : x.suppliercompany
                });

                if (Math.Abs(x.totalamount - x.taxsummary.Sum(y => y.taxable + y.tax)) > 10)
                {
                    if (x.totalamount > x.taxsummary.Sum(y => y.taxable + y.tax))
                    {
                        response.Add(new TrailBalanceLedger()
                        {
                            coremode = "Cr",
                            coretype = "Sundry Creditors",
                            coretypeid = 1,
                            credit = x.returntype >= 10 ? 0 : (x.totalamount - x.taxsummary.Sum(y => y.taxable + y.tax)),
                            debit = x.returntype >= 10 ? (x.totalamount - x.taxsummary.Sum(y => y.taxable + y.tax)) : 0,
                            datasource = x.returntype == 7 ? "Supplier Advance" : (x.returntype >= 10 ? "Excess Note" : "Debit Note"),
                            displayname = x.returntype == 7 ? "Supplier Advance" : (x.returntype >= 10 ? "Excess Note" : "Debit Note")
                        });
                    }
                    else
                    {
                        response.Add(new TrailBalanceLedger()
                        {
                            coremode = "Dr",
                            coretype = "Sundry Creditors",
                            coretypeid = 1,
                            debit = x.returntype >= 10 ? 0 : (x.taxsummary.Sum(y => y.taxable + y.tax) - x.totalamount),
                            credit = x.returntype >= 10 ? (x.taxsummary.Sum(y => y.taxable + y.tax) - x.totalamount) : 0,
                            datasource = x.returntype == 7 ? "Supplier Advance" : (x.returntype >= 10 ? "Excess Note" : "Debit Note"),
                            displayname = x.returntype == 7 ? "Supplier Advance" : (x.returntype >= 10 ? "Excess Note" : "Debit Note")
                        });
                    }
                }

                x.taxsummary.ForEach(y =>
                {
                    var percentagestring = ((y.percentage) % 1 != 0) ? (y.percentage).ToString("N2") : (y.percentage).ToString("N0");
                    response.Add(new TrailBalanceLedger()
                    {
                        coremode = "Cr",
                        coretype = "Purchase Return",
                        coretypeid = 15,
                        credit = x.returntype >= 10 ? 0 : y.taxable,
                        debit = x.returntype >= 10 ? y.taxable : 0,
                        datasource = x.returntype == 7 ? "Supplier Advance" : (x.returntype >= 10 ? "Excess Note" : "Debit Note"),
                        displayname =
                        (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.finance_trace ?
                        "Purchase Return @" : "Purchase @") + percentagestring + "%"
                    });
                    if (x.isinterstatesale)
                    {
                        response.Add(new TrailBalanceLedger()
                        {
                            coremode = "Cr",
                            coretype = "Tax & Duties",
                            coretypeid = 16,
                            credit = x.returntype >= 10 ? 0 : y.tax,
                            debit = x.returntype >= 10 ? y.tax : 0,
                            datasource = x.returntype == 7 ? "Supplier Advance" : (x.returntype >= 10 ? "Excess Note" : "Debit Note"),
                            displayname =
                            (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.finance_trace ?
                            "PR " : "") + ("INPUT IGST @" + percentagestring + "%")
                        });
                    }
                    else if (y.tax != 0)
                    {
                        percentagestring = ((y.percentage / 2) % 1 != 0) ? (y.percentage / 2).ToString("N2") : (y.percentage / 2).ToString("N0");
                        response.Add(new TrailBalanceLedger()
                        {
                            coremode = "Cr",
                            coretype = "Tax & Duties",
                            coretypeid = 16,
                            credit = x.returntype >= 10 ? 0 : y.tax / 2,
                            debit = x.returntype >= 10 ? y.tax / 2 : 0,
                            datasource = x.returntype == 7 ? "Supplier Advance" : (x.returntype == 10 ? "Excess Note" : "Debit Note"),
                            displayname = (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.finance_trace ?
                            "PR " : "") + "INPUT CGST @" + percentagestring + "%"
                        });
                        response.Add(new TrailBalanceLedger()
                        {
                            coremode = "Cr",
                            coretype = "Tax & Duties",
                            coretypeid = 16,
                            credit = x.returntype >= 10 ? 0 : y.tax / 2,
                            debit = x.returntype >= 10 ? y.tax / 2 : 0,
                            datasource = x.returntype == 7 ? "Supplier Advance" : (x.returntype == 10 ? "Excess Note" : "Debit Note"),
                            displayname = (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.finance_trace ?
                            "PR " : "") + "INPUT SGST @" + percentagestring + "%"
                        });
                    }
                });

                var dr = response.Sum(y => y.debit);
                var cr = response.Sum(y => y.credit);
                if (Math.Abs(dr - cr) > 10)
                {
                    cnf.Add(new Tuple<long, decimal>(x.id, dr - cr));
                }
            });

            CheckCrDr(response, finalresponse, (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.finance_trace ?
                "Purchase Return" : "Purchase"), 15, "Rounding");

            quantoledgers = SearchSupplierPaymentSummary(companyid, locationid, fromdate, todate, false);
            var suppliertdsname = @"Supplier TDS";

            if (Services.ConfigManager.Instance(Context.OrganizationID).Config.fin_paytdsledgerid > 0)
            {
                suppliertdsname = ledgerDataService.Search(new LedgerCreteria()
                {
                    id = Services.ConfigManager.Instance(Context.OrganizationID).Config.fin_paytdsledgerid
                }).First().displayname;
            }

            quantoledgers.SupplierPayment.ForEach(x =>
            {

                response.Add(new TrailBalanceLedger()
                {
                    coremode = "Cr",
                    issupplier = true,
                    supplierid = x.suppliercompanyid,
                    coretype = "Sundry Creditors",
                    coretypeid = 1,
                    datasource = "Supplier Payment",
                    debit = x.rounding + x.allocations.Sum(y => y.paidamount + y.agentcommission) - x.tdsamount,// + x.tdsamount, 
                    displayname = string.IsNullOrEmpty(x.suppliercompany) ? "Supplier Missing" : x.suppliercompany
                });

                if (x.tdsamount > 0)
                {
                    var tdsamount = x.tdsamount;
                    x.allocations.ForEach(y =>
                    {
                        if (tdsamount > 0)
                        {
                            if (y.paidamount >= tdsamount)
                            {
                                y.paidamount -= tdsamount;
                                tdsamount = 0;
                            }
                            else
                            {
                                tdsamount -= y.paidamount;
                                y.paidamount = 0;
                            }
                        }
                    });
                    if (tdsamount > 0)
                    {
                        x.allocations.ForEach(y =>
                        {
                            if (tdsamount > 0)
                            {
                                if (y.discountamount >= tdsamount)
                                {
                                    y.discountamount -= tdsamount;
                                    tdsamount = 0;
                                }
                                else
                                {
                                    tdsamount -= y.discountamount;
                                    y.discountamount = 0;
                                }
                            }
                        });
                    }
                }

                x.allocations.ForEach(y =>
                {
                    if (x.iscash)
                    {
                        response.Add(new TrailBalanceLedger()
                        {
                            coremode = "Cr",
                            coretype = "Cash-In-Hand",
                            coretypeid = 6,
                            credit = y.paidamount - y.discountamount,
                            datasource = "Supplier Payment",
                            displayname = "Cash"
                        });
                    }
                    else
                    {
                        response.Add(new TrailBalanceLedger()
                        {
                            coremode = "Cr",
                            coretype = "Bank Account",
                            coretypeid = 7,
                            credit = y.paidamount - y.discountamount,
                            datasource = "Supplier Payment",
                            displayname = String.IsNullOrEmpty(x.bankname) ? "Supplier Payment - Bank Missing" :
                            InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.finance_trace ? (x.bankname + " - " + x.suppliercompany) : x.bankname,
                        });
                    }

                    if (y.discountamount != 0)
                    {
                        response.Add(new TrailBalanceLedger()
                        {
                            coremode = "Dr",
                            coretype = "Purchase",
                            coretypeid = 15,
                            credit = y.discountamount,
                            datasource = "Supplier Payment",
                            displayname = InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.finance_trace ? ("Supplier Discount" + "-" + x.suppliercompany) : "Supplier Discount"
                        });
                    }

                    if (y.agentcommission != 0)
                    {
                        response.Add(new TrailBalanceLedger()
                        {
                            coremode = "Dr",
                            coretype = "Purchase",
                            coretypeid = 15,
                            credit = y.agentcommission,
                            datasource = "Supplier Payment",
                            displayname = InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.finance_trace ? ("Agent Commission" + "-" + x.suppliercompany) : "Agent Commission"
                        });
                    }
                });

                if (x.tdsamount != 0)
                {
                    response.Add(new TrailBalanceLedger()
                    {
                        coremode = "Cr",
                        coretype = "Purchase",
                        coretypeid = 15,
                        credit = x.tdsamount,
                        datasource = "Supplier Payment",
                        displayname = InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.finance_trace ? (suppliertdsname + "-" + x.suppliercompany) : suppliertdsname
                    });
                }
            });
            CheckCrDr(response, finalresponse, "Supplier Payment", 15, "Rounding");

            var tdsList = TDSSummary(companyid, fromdate, todate);
            if (tdsList.Count > 0)
            {
                var tdsledgers = ledgerDataService.Search(new LedgerCreteria()
                {
                    ids = tdsList.ConvertAll(x => x.tdsledgerid).Distinct().ToList()
                });
                tdsList.ForEach(x =>
                {
                    if (x.amount > 0)
                    {
                        response.Add(new TrailBalanceLedger()
                        {
                            coremode = "Cr",
                            coretype = "Tax & Duties",
                            coretypeid = 16,
                            credit = x.amount,
                            datasource = "TDS Purchase Summary",
                            displayname = tdsledgers.Exists(y => y.id == x.tdsledgerid) ? tdsledgers.Find(y => y.id == x.tdsledgerid).displayname : "Supplier TDS"
                        });
                        response.Add(new TrailBalanceLedger()
                        {
                            coremode = "Dr",
                            coretype = "Tax & Duties",
                            coretypeid = 16,
                            debit = x.amount,
                            issupplier = true,
                            datasource = "TDS Purchase Summary",
                            displayname = x.supplier
                        });
                    }
                    else
                    {
                        response.Add(new TrailBalanceLedger()
                        {
                            coremode = "Dr",
                            coretype = "Tax & Duties",
                            coretypeid = 16,
                            credit = x.amount * -1,
                            datasource = "TDS Payment Summary",
                            displayname = tdsledgers.Exists(y => y.id == x.tdsledgerid) ? tdsledgers.Find(y => y.id == x.tdsledgerid).displayname : "Supplier TDS"
                        });
                        response.Add(new TrailBalanceLedger()
                        {
                            coremode = "Cr",
                            coretype = "Tax & Duties",
                            coretypeid = 16,
                            debit = x.amount * -1,
                            issupplier = true,
                            datasource = "TDS Payment Summary",
                            displayname = x.supplier
                        });
                    }
                });
            }

            CheckCrDr(response, finalresponse, "TDS", 15, "Rounding");

            //string filtertext = string.Join("\r\n", filterledger.ConvertAll(x =>
            //{
            //    return x.datasource + "\t" + x.displayname + "\t" + x.debit + "\t" + x.credit;
            //}).ToList());


            if (!InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.finance_trace)
            {
                finalresponse = finalresponse.GroupBy(x => new { x.coretype, x.displayname, x.coretypeid, x.only4tp, x.isposting, x.supplierid }).ToList()
                    .ConvertAll(x => new TrailBalanceLedger()
                    {
                        coretype = x.Key.coretype,
                        coretypeid = x.Key.coretypeid,
                        displayname = x.Key.displayname,
                        only4tp = x.Key.only4tp,
                        isposting = x.Key.isposting,
                        supplierid = x.Key.supplierid,
                        parentgroup = x.Any(y => !string.IsNullOrEmpty(y.parentgroup)) ? x.First(y => string.IsNullOrEmpty(y.parentgroup)).parentgroup : "",
                        debit = x.Sum(y => y.debit),
                        credit = x.Sum(y => y.credit),
                    });
            }

            SortedList<int, string> AccountTypes = new SortedList<int, string>
                {
                    { 1, "Capital" },
                    { 2, "Fixed asset" },
                    { 19, "Current asset" },
                    { 3, "Liablities" },
                    { 20, "Current Liablities" },
                    { 4, "Direct Income" },
                    { 5, "Direct Expense" },
                    { 6, "Cash-In-Hand" },
                    { 7, "Bank Account" },
                    { 8, "Stock-In-Hand" },
                    { 9, "Indirect Incomes" },
                    { 10, "Indirect Expenses" },
                    { 11, "Sundry Debtors" },
                    { 12, "Sundry Creditors" },
                    { 13, "Petty Cash" },
                    { 14, "Sales" },
                    { 15, "Purchase" },
                    { 16, "Tax & Duties" },
                    { 17, "Petty Cash Direct Expense" },
                    { 18, "Petty Cash In-Direct Expense" },
                    { 21, "Petty Cash Direct Income" },
                    { 22, "Petty Cash In-Direct Income" },
                    { 30, "Rounding" }
                };

            var emptyparentgroups = finalresponse.FindAll(x => string.IsNullOrEmpty(x.parentgroup));
            string parentgrouplist = string.Join(",", emptyparentgroups.ConvertAll(x => "'" + WebAPI.Data.Utility.SafeSQL(x.displayname) + "'").ToList());
            if (!string.IsNullOrEmpty(parentgrouplist))
            {
                using (var reader = ExecuteReader("select displayname,typeid,cast(parentinfo->>'detail' as varchar) parent from ledger where displayname in (" + parentgrouplist + ")", null))
                {
                    while (reader.Read())
                    {
                        var parent = reader["parent"] == DBNull.Value ? "" : reader["parent"].ToString();
                        var name = reader["displayname"].ToString();
                        var typeid = Convert.ToInt32(reader["typeid"]);
                        var roottypename = AccountTypes.ContainsKey(typeid) ? AccountTypes[typeid] : "";
                        if (string.IsNullOrEmpty(parent))
                        {
                            parent = roottypename;
                        }
                        else if (!parent.StartsWith(roottypename))
                        {
                            parent = roottypename + "/" + parent;
                        }
                        finalresponse.FindAll(x => x.displayname == name).ForEach(x =>
                        {
                            x.parentgroup = parent;
                        });
                    }
                }
            }

            emptyparentgroups = finalresponse.FindAll(x => string.IsNullOrEmpty(x.parentgroup));
            parentgrouplist = string.Join(",", emptyparentgroups.ConvertAll(x => "'" + WebAPI.Data.Utility.SafeSQL(x.displayname) + "'").ToList());

            if (!string.IsNullOrEmpty(parentgrouplist))
            {

                using (var reader = ExecuteReader(@"select supplier.name displayname,additionalinfo->>'tallyledger' parent from supplier
                                inner join referencelist on referencelist.id = supplier.suppliergroupid
                                where supplier.name in (" + parentgrouplist + ")", null))
                {
                    while (reader.Read())
                    {
                        var parent = reader["parent"] == DBNull.Value ? "" : reader["parent"].ToString();
                        var name = reader["displayname"].ToString();
                        if (!string.IsNullOrEmpty(parent))
                        {
                            finalresponse.FindAll(x => x.displayname == name).ForEach(x =>
                            {
                                x.parentgroup = parent;
                            });
                        }
                    }
                }

            }

            if (!string.IsNullOrEmpty(parentgrouplist))
            {
                using (var reader = ExecuteReader(@"select customer.name displayname,additionalinfo->>'tallyledger' parent from customer
                                inner join referencelist on referencelist.id = customer.customergroupid
                                where customer.name in (" + parentgrouplist + ")", null))
                {
                    while (reader.Read())
                    {
                        var parent = reader["parent"] == DBNull.Value ? "" : reader["parent"].ToString();
                        var name = reader["displayname"].ToString();
                        if (!string.IsNullOrEmpty(parent))
                        {
                            finalresponse.FindAll(x => x.displayname == name).ForEach(x =>
                            {
                                x.parentgroup = parent;
                            });
                        }
                    }

                }
            }

            string customersuplierledger = @"select ledger.displayname,suppliercompany.name scname,supplier.name sname,ledger.parentgroup from ledger
            inner join supplier on supplier.id = ledger.supplierid
            inner join suppliercompany on suppliercompany.supplierid = supplier.id
            where ledger.isactive=true";
            using (var reader = ExecuteReader(customersuplierledger, null))
            {
                while (reader.Read())
                {
                    string ledgerdisplayname = reader["displayname"] == DBNull.Value ? "NONE" : Convert.ToString(reader["displayname"]);
                    string scname = reader["scname"] == DBNull.Value ? "NONE" : Convert.ToString(reader["scname"]);
                    string sname = reader["sname"] == DBNull.Value ? "NONE" : Convert.ToString(reader["sname"]);
                    string parentgroup = reader["parentgroup"] == DBNull.Value ? "NONE" : Convert.ToString(reader["parentgroup"]);
                    var records = finalresponse.FindAll(x => x.issupplier && (x.displayname == scname || x.displayname == sname));
                    records.ForEach(x =>
                    {
                        x.displayname = ledgerdisplayname;
                        x.parentgroup = parentgroup;
                    });
                }
            }

            customersuplierledger = @"select ledger.displayname,customer.name,ledger.parentgroup from ledger
            inner join customer on customer.id = ledger.customerid
            where ledger.isactive=true";
            using (var reader = ExecuteReader(customersuplierledger, null))
            {
                while (reader.Read())
                {
                    string ledgerdisplayname = reader["displayname"] == DBNull.Value ? "NONE" : Convert.ToString(reader["displayname"]);
                    string name = reader["name"] == DBNull.Value ? "NONE" : Convert.ToString(reader["name"]);
                    string parentgroup = reader["parentgroup"] == DBNull.Value ? "NONE" : Convert.ToString(reader["parentgroup"]);
                    var records = finalresponse.FindAll(x => x.iscustomer && x.displayname == name);
                    records.ForEach(x =>
                    {
                        x.displayname = ledgerdisplayname;
                        x.parentgroup = parentgroup;
                    });
                }
            }

            var drtotal = finalresponse.Sum(x => x.debit);
            var crtotal = finalresponse.Sum(x => x.credit);
            return finalresponse;
        }

        public List<TrailBalanceLedger> filterledger = new List<TrailBalanceLedger>();

        public bool LoadStockFromOpening { get; internal set; }

        public void CheckCrDr(List<TrailBalanceLedger> response,
            List<TrailBalanceLedger> finalresponse,
            string coretype, int coretypeid,
            string adjustmentledger)
        {


            if (response.Exists(x => x.displayname.ToUpper().Contains("M.M. HOTEL JAYANTHI")))
            {
                var filtered = response.FindAll(x => x.displayname.ToUpper().Contains("M.M. HOTEL JAYANTHI"));
                var credit = filtered.Sum(x => x.credit);
                var debit = filtered.Sum(x => x.debit);
            }
            if (response.Exists(x => x.displayname.ToUpper().Contains("M Z EMPORIUM")))
            {
                var filtered = response.FindAll(x => x.displayname.ToUpper().Contains("M Z EMPORIUM"));
                var credit = filtered.Sum(x => x.credit);
                var debit = filtered.Sum(x => x.debit);
            }
            //filterledger.AddRange(response.FindAll(x => x.displayname.Contains("SHOBHANA")));
            //if(response.Exists(x=>x.displayname.Contains("6.00")))
            //{
            //    var list = response.FindAll(x => x.displayname.Contains("6.00"));
            //}

            var cr = response.Sum(x => x.credit);
            var dr = response.Sum(x => x.debit);
            if (Math.Abs(cr) - Math.Abs(dr) > 100)
            {
                cr = 0;
                dr = 0;
            }

            response.ForEach(x =>
            {
                x.debit = Math.Round(x.debit, 0, MidpointRounding.ToEven);
                x.credit = Math.Round(x.credit, 0, MidpointRounding.ToEven);
                if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.finance_trace)
                    x.displayname = coretype + " - " + x.displayname;
            });

            var drtotal = response.Sum(x => x.debit);
            var crtotal = response.Sum(x => x.credit);
            decimal rounding = 0;
            TrailBalanceLedger roundingledger = null;
            if (drtotal > crtotal)
            {
                rounding = drtotal - crtotal;
                roundingledger = new TrailBalanceLedger()
                {
                    coremode = "Cr",
                    coretype = coretype,
                    coretypeid = coretypeid,
                    credit = rounding,
                    displayname = coretype + " " + adjustmentledger
                };
            }
            else if (drtotal < crtotal)
            {
                rounding = crtotal - drtotal;
                roundingledger = new TrailBalanceLedger()
                {
                    coremode = "Cr",
                    coretype = coretype,
                    coretypeid = coretypeid,
                    debit = rounding,
                    displayname = coretype + " " + adjustmentledger
                };
            }

            //var fulltext = string.Join("\r\n", response.ConvertAll(x => string.Format("{0}\t{1}\t{2}", x.displayname, x.credit, x.debit)));

            finalresponse.AddRange(response);
            if (roundingledger != null)
            {
                finalresponse.Add(roundingledger);
            }

            drtotal = finalresponse.Sum(x => x.debit);
            crtotal = finalresponse.Sum(x => x.credit);
            rounding = Math.Abs(crtotal - drtotal);
            if (rounding > 1000)
            {
                rounding = 0;
            }
            response.Clear();
        }

        public class FinanceResult
        {
            public List<TrailBalanceLedger> DebitList { get; set; }
            public List<TrailBalanceLedger> CreditList { get; set; }

            public decimal DebitTotal { get; set; }
            public decimal CreditTotal { get; set; }
            public decimal Profit { get; set; }
            public decimal Loss { get; set; }

            public void PrepareDisplayList()
            {
                if (DebitList.Any(x => x.credit > 0))
                {
                    var conflictdebit = DebitList.FindAll(x => x.credit > 0).ToList();
                    foreach (var credititem in conflictdebit)
                    {
                        //DebitList.Remove(credititem);
                        credititem.debit = credititem.debit - credititem.credit;
                    }
                }
                if (CreditList.Any(x => x.debit > 0))
                {
                    var conflictdebit = CreditList.FindAll(x => x.debit > 0).ToList();
                    foreach (var credititem in conflictdebit)
                    {
                        //DebitList.Remove(credititem);
                        credititem.credit = credititem.credit - credititem.debit;
                    }
                }
                DisplayList = new List<Row>();
                foreach (var item in DebitList)
                {
                    DisplayList.Add(new Row()
                    {
                        Debit = item.debit,
                        DebitDisplay = item.displayname,
                        DebitHeader = item.header,
                        CreditDisplay = "",
                        Credit = 0,
                        DebitOBCB = new Row.OBCB()
                        {
                            OpeningCR = item.OpeningCR,
                            OpeningDR = item.OpeningDR,
                            ClosingCR = item.ClosingCR,
                            ClosingDR = item.ClosingDR
                        }
                    });
                }
                for (int i = 0; i < CreditList.Count; i++)
                {
                    if (DisplayList.Count > i)
                    {
                        DisplayList[i].CreditDisplay = CreditList[i].displayname;
                        DisplayList[i].Credit = CreditList[i].credit;
                        DisplayList[i].CreditHeader = CreditList[i].header;
                        DisplayList[i].CreditOBCB = new Row.OBCB()
                        {
                            OpeningCR = CreditList[i].OpeningCR,
                            OpeningDR = CreditList[i].OpeningDR,
                            ClosingCR = CreditList[i].ClosingCR,
                            ClosingDR = CreditList[i].ClosingDR
                        };
                    }
                    else
                    {
                        DisplayList.Add(new Row()
                        {
                            Debit = 0,
                            DebitDisplay = "",
                            CreditDisplay = CreditList[i].displayname,
                            Credit = CreditList[i].credit,
                            CreditHeader = CreditList[i].header,
                            CreditOBCB = new Row.OBCB()
                            {
                                OpeningCR = CreditList[i].OpeningCR,
                                OpeningDR = CreditList[i].OpeningDR,
                                ClosingCR = CreditList[i].ClosingCR,
                                ClosingDR = CreditList[i].ClosingDR
                            }
                        });
                    }
                }
            }
            //public void PrepareDisplayList()
            //{
            //    DisplayList = new List<Row>();
            //    foreach (var item in DebitList)
            //    {
            //        DisplayList.Add(new Row()
            //        {
            //            Debit = item.debit,
            //            DebitDisplay = item.displayname,
            //            CreditDisplay = "",
            //            Credit = 0
            //        });
            //    }
            //    for (int i = 0; i < CreditList.Count; i++)
            //    {
            //        if (DisplayList.Count > i)
            //        {
            //            DisplayList[i].CreditDisplay = CreditList[i].displayname;
            //            DisplayList[i].Credit = CreditList[i].credit;
            //        }
            //        else
            //        {
            //            DisplayList.Add(new Row()
            //            {
            //                Debit = 0,
            //                DebitDisplay = "",
            //                CreditDisplay = CreditList[i].displayname,
            //                Credit = CreditList[i].credit
            //            });
            //        }
            //    }
            //}
            public List<Row> DisplayList { get; set; }
            public class Row
            {
                public string CreditDisplay { get; set; }
                public string DebitDisplay { get; set; }
                public decimal Credit { get; set; }
                public decimal Debit { get; set; }
                public OBCB CreditOBCB { get; set; }
                public OBCB DebitOBCB { get; set; }
                public string DebitHeader { get; internal set; }
                public string CreditHeader { get; internal set; }

                public class OBCB
                {
                    public decimal OpeningDR { get; set; }
                    public decimal OpeningCR { get; set; }
                    public decimal ClosingDR { get; set; }
                    public decimal ClosingCR { get; set; }
                }
            }
        }

        public InventoryStock FindStockAsOnDate(long companyid, long locationid, DateTime date)
        {
            if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.finance_stocktransaction)
                return FindStockAsOnDateByBackTrack(companyid, date);

            AnalyserDataService analyserDataService = new AnalyserDataService() { Context = this.Context };
            var stock = analyserDataService.SearchAsOnStockTransaction(new AnalyserDataService.StockAsOnCriteria()
            {
                locationid = locationid,
                companyid = companyid,
                fromdate = DateTime.MinValue,
                todate = date,
                fields = new List<int>() { 0 }
            });
            return new InventoryStock()
            {
                CLOSING = stock.items.Sum(x => x.closingcost)
            };
        }

        public InventoryStock FindStockAsOnDateByBackTrack(long companyid, DateTime date)
        {
            string query = @"    with currentretailclosing as
	(
		select
		sum((salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty+salablegoods.aqty)) * stock.buyingprice) as finstockcost,
		sum((salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty+salablegoods.aqty))) as finstockqty
		from salablegoods 
		inner join stock on stock.id = salablegoods.stockid and stock.isactive = true
		where salablegoods.isactive = true and salablegoods.companyid=@companyid 
		and salablegoods.hasstock = true 
		and salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty+salablegoods.aqty)>0 
	),currentwhclosing as
	(
    select
    sum((stock.qty - (stock.transfered+stock.returnqty+stock.damagedqty+stock.journalqty)) * stock.buyingprice::numeric) as finstockcost,
	sum((stock.qty - (stock.transfered+stock.returnqty+stock.damagedqty+stock.journalqty))) as finstockqty
    from stock
    where stock.isactive = true
    and stock.hasstock = true and stock.companyid=@companyid 
	and stock.qty-(stock.transfered+stock.returnqty+stock.damagedqty+stock.journalqty)>0 
	),purchase as
	(
    select 
    sum(lrinvoiceitems.buyingqty * lrinvoiceitems.buyingprice::numeric) as finstockcost,
    sum(lrinvoiceitems.buyingqty) as finstockqty
    from lrinvoice 
    inner join lrinvoiceitems on lrinvoiceitems.lrinvoiceid = lrinvoice.id 
	and lrinvoiceitems.isactive = true 
    Where lrinvoice.isactive=true  and lrinvoice.companyid=@companyid 
	and lrinvoice.entrydate>=@fromdate
    ),purchasereturn as
	(
select 
sum((lrinvoicereturnitems.qty * lrinvoicereturnitems.rate)) as finstockcost,
sum((lrinvoicereturnitems.qty)) as finstockqty
from lrinvoicereturn
inner join lrinvoicereturnitems on lrinvoicereturnid = lrinvoicereturn.id
where lrinvoicereturn.isactive = true and lrinvoicereturnitems.isactive = true 
and lrinvoicereturn.returntype = 0 
and lrinvoicereturnitems.stockid>0
and lrinvoicereturn.companyid=@companyid 
and lrinvoicereturn.returndate>=@fromdate
),sales as
(
select 
sum(billitems.qty * stock.buyingprice::numeric) as finstockcost,
sum(billitems.qty) as finstockqty	
from bill 
inner join billitems on billitems.billid = bill.id and billitems.isactive=true
left outer join salablegoods on salablegoods.id = salablegoodsid 
left outer join stock on stock.id = salablegoods.stockid
where bill.isactive = true and bill.billtype != 9 
and bill.billdate>=@fromdate
and bill.companyid=@companyid
),nonbarcodepurchase  as
(    
	select 
    sum((lrinvoiceitems.buyingqty-lrinvoiceitems.sellableqty-lrinvoiceitems.returnqty) * lrinvoiceitems.buyingprice::numeric) as finstockcost,
	sum((lrinvoiceitems.buyingqty-lrinvoiceitems.sellableqty-lrinvoiceitems.returnqty)) as finstockqty
    from lrinvoice 
    inner join lrinvoiceitems on lrinvoiceitems.lrinvoiceid = lrinvoice.id 
	and lrinvoiceitems.isactive = true 
    Where lrinvoice.isactive=true  and lrinvoice.companyid=@companyid 
	and lrinvoice.entrydate>=@fromdate
	and lrinvoiceitems.sellableqty<lrinvoiceitems.buyingqty
),nonbarcodereturn as
(
select 
sum((lrinvoicereturnitems.qty * lrinvoicereturnitems.rate)) as finstockcost,
sum((lrinvoicereturnitems.qty)) as finstockqty	
from lrinvoicereturn
inner join lrinvoicereturnitems on lrinvoicereturnid = lrinvoicereturn.id
where lrinvoicereturn.isactive = true and lrinvoicereturnitems.isactive = true 
and lrinvoicereturn.returntype = 0 
and lrinvoicereturnitems.stockid=0
and lrinvoicereturn.companyid=@companyid 
and lrinvoicereturn.returndate>=@fromdate
),auditretail as
(
	select 
    sum( salablegoodshistory.auditqty * stock.buyingprice::numeric) as finstockcost,
	sum( salablegoodshistory.auditqty) as finstockqty
    from salablegoodshistory 
	inner join salablegoods on salablegoods.id = salablegoodshistory.salablegoodsid
    inner join stock on stock.id = salablegoods.stockid
    where salablegoods.isactive=true and stock.isactive=true 
	and stock.companyid=@companyid 
	and salablegoodshistory.modifiedon>=@fromdate
),auditwh as
(
	select 
    sum( stockhistory.auditqty * stock.ibuyingprice::numeric) as finstockcost,
	sum( stockhistory.auditqty) as finstockqty
    from stockhistory 
	inner join stock on stock.id = stockhistory.stockid
    where stock.isactive=true 
	and stockhistory.modifiedon>=@fromdate
	and stock.companyid=@companyid 
)

select 'CLOSING' action,* from currentretailclosing
union all
SELECT 'CLOSING' action,* from currentwhclosing
union all
SELECT 'PURCHASE' action,* from purchase
union all
SELECT 'PURCHASERETURN' action,* from purchasereturn
union all
SELECT 'SALES' action,* from sales
union all
SELECT 'AUDIT' action,* from auditretail
union all
SELECT 'AUDIT' action,* from auditwh
union all
SELECT 'NPURCHASE' action,* from nonbarcodepurchase
union all
SELECT 'NPURCHASERETURN' action,* from nonbarcodereturn";

            if (companyid == 0)
            {
                query = query.Replace("and salablegoods.companyid=@companyid", "");
                query = query.Replace("and stock.companyid=@companyid", "");
                query = query.Replace("and lrinvoice.companyid=@companyid", "");
                query = query.Replace("and lrinvoicereturn.companyid=@companyid", "");
                query = query.Replace("and bill.companyid=@companyid", "");
                query = query.Replace("and lrinvoice.companyid=@companyid", "");
                query = query.Replace("and lrinvoicereturn.companyid=@companyid", "");
                query = query.Replace("and stock.companyid=@companyid", "");
            }
            InventoryStock inventoryStock = new InventoryStock();
            var param = new SortedList<string, object>();
            param.Add("companyid", companyid);
            param.Add("fromdate", date);
            using (var reader = ExecuteReader(query, param, 1000))
            {
                while (reader.Read())
                {
                    decimal value = reader["finstockcost"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["finstockcost"]);
                    string action = reader["action"] == DBNull.Value ? "" : Convert.ToString(reader["action"]);
                    switch (action)
                    {
                        case "CLOSING":
                            inventoryStock.CLOSING += value;
                            break;
                        case "PURCHASE":
                            inventoryStock.PURCHASE += value;
                            break;
                        case "PURCHASERETURN":
                            inventoryStock.PURCHASERETURN += value;
                            break;
                        case "SALES":
                            inventoryStock.SALES += value;
                            break;
                        case "AUDIT":
                            inventoryStock.AUDIT += value;
                            break;
                        case "NPURCHASE":
                            inventoryStock.NPURCHASE += value;
                            break;
                        case "NPURCHASERETURN":
                            inventoryStock.NPURCHASERETURN += value;
                            break;
                    }
                }
            }
            return inventoryStock;
        }

        public class InventoryStock
        {
            public decimal OPENING
            {
                get
                {
                    var openging = CLOSING + SALES + PURCHASERETURN + NPURCHASERETURN - (PURCHASE + NPURCHASE);
                    if (openging < 0) return 0;
                    return openging;
                }
            }
            public decimal CLOSING { get; set; }
            public decimal PURCHASE { get; set; }
            public decimal PURCHASERETURN { get; set; }
            public decimal SALES { get; set; }
            public decimal AUDIT { get; set; }
            public decimal NPURCHASE { get; set; }
            public decimal NPURCHASERETURN { get; set; }
        }
        public FinanceResult TradingAccount(long companyid, long locationid, DateTime from, DateTime to, bool addPandL)
        {
            var response = TrailBalanceLedgers(companyid, locationid, from, to);

            FinanceResult financeResult = new FinanceResult();

            financeResult.DebitList = response.FindAll(e => e.coretypeid == 15 || e.coretypeid == 5 || e.coretypeid == 17).FindAll(x => !x.only4tp && !x.isposting);
            financeResult.CreditList = response.FindAll(e => e.coretypeid == 14 || e.coretypeid == 4).FindAll(x => !x.only4tp && !x.isposting);

            //Data.WarehouseReportDataService warehouseReportDataService = new Data.WarehouseReportDataService() { Context = this.Context };
            //var stockreport = warehouseReportDataService.SearchAsOnStock(new StockReportCreteria()
            //{
            //    fromdate = from,
            //    todate = to
            //});

            //var openingstockvalue = stockreport.Sum(x => x.openingstockvalue);
            //var closingstockvalue = stockreport.Sum(x => x.closingstockvalue);

            decimal openingstockvalue = 0;
            decimal closingstockvalue = 0;
            if (LoadStockFromOpening)
            {
                LedgerstockopeningDataService ledgerstockopeningDataService = new LedgerstockopeningDataService() { Context = this.Context };
                var stockopening = ledgerstockopeningDataService.FindStockOpening(new LedgerstockopeningCreteria() { fromdate = from, todate = to, companyid = companyid });
                openingstockvalue = stockopening.opening.value;
                closingstockvalue = stockopening.closing == null ? FindStockAsOnDate(companyid, locationid, to).OPENING : stockopening.closing.value;
            }
            else
            {
                var stockason = FindStockAsOnDate(companyid, locationid, from);
                openingstockvalue = stockason.OPENING;
                closingstockvalue = to.Date >= DateTime.Now.Date ? stockason.CLOSING : FindStockAsOnDate(companyid, locationid, to).OPENING;
            }

            if (openingstockvalue > 0)
            {
                financeResult.DebitList.Insert(0, new TrailBalanceLedger()
                {
                    displayname = "Opening Stock",
                    debit = openingstockvalue
                });
            }


            if (closingstockvalue > 0)
            {
                financeResult.CreditList.Add(new TrailBalanceLedger()
                {
                    displayname = "Closing Stock",
                    credit = closingstockvalue
                });
            }

            if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.fin_nonbarcodedvalue)
            {

                string query = @"select sum((lrinvoiceitems.buyingqty - (lrinvoiceitems.sellableqty+lrinvoiceitems.returnqty))*buyingprice) from lrinvoice 
inner join lrinvoiceitems on lrinvoiceitems.lrinvoiceid = lrinvoice.id and lrinvoiceitems.isactive=true
where lrinvoice.entrydate>=@fromdate and lrinvoice.entrydate<@todate and lrinvoice.companyid=@companyid and lrinvoice.isactive=true
and lrinvoiceitems.buyingqty - (lrinvoiceitems.sellableqty+lrinvoiceitems.returnqty)>0;

select sum((stock.qty-stock.transfered)*lrinvoiceitems.buyingprice) from lrinvoice 
inner join lrinvoiceitems on lrinvoiceitems.lrinvoiceid = lrinvoice.id and lrinvoiceitems.isactive=true
inner join stock on stock.invoiceitemid = lrinvoiceitems.id
where lrinvoice.entrydate>=@fromdate and lrinvoice.entrydate<@todate and lrinvoice.companyid=@companyid and lrinvoice.isactive=true and stock.isactive=true
and stock.qty-stock.transfered>0;

select 
sum(case when lrinvoicetax.type=1 then lrinvoicetax.amounton else lrinvoicetax.netamount-lrinvoicetax.taxamount end)  taxable
from lrinvoice 
inner join lrinvoicetax on lrinvoicetax.invoiceid = lrinvoice.id
left outer join lrinvoiceitems on lrinvoiceitems.lrinvoiceid = lrinvoice.id
where lrinvoice.entrydate>=@fromdate and lrinvoice.entrydate<@todate and lrinvoice.companyid=@companyid and lrinvoice.isactive=true 
and lrinvoicetax.isactive = true 
and (lrinvoicetax.type=0 and lrinvoicetax.taxid=0 and lrinvoicetax.taxamount=0)=false
and lrinvoicetax.type not in (2,3,9,14)
and lrinvoiceitems.id is null";

                decimal invoicependingvalue = 0;
                var param = new SortedList<string, object>();
                param.Add("fromdate", from);
                param.Add("todate", to); ;
                param.Add("companyid", companyid);
                using (var reader = ExecuteReader(query, param, 1000))
                {
                    if (reader.Read())
                    {
                        invoicependingvalue += reader.IsDBNull(0) ? 0 : reader.GetDecimal(0);
                    }
                    if (reader.NextResult() && reader.Read())
                    {
                        invoicependingvalue += reader.IsDBNull(0) ? 0 : reader.GetDecimal(0);
                    }
                    if (reader.NextResult() && reader.Read())
                    {
                        invoicependingvalue += reader.IsDBNull(0) ? 0 : reader.GetDecimal(0);
                    }
                }
                if (invoicependingvalue > 0)
                {
                    financeResult.CreditList.Add(new TrailBalanceLedger()
                    {
                        displayname = "Nonbarcoded Stock",
                        credit = invoicependingvalue
                    });
                }
            }



            financeResult.CreditTotal = financeResult.CreditList.Sum(e => e.credit - e.debit);
            financeResult.DebitTotal = financeResult.DebitList.Sum(e => e.debit - e.credit);
            if (financeResult.DebitTotal > financeResult.CreditTotal)
                financeResult.Loss = financeResult.DebitTotal - financeResult.CreditTotal;
            else
                financeResult.Profit = financeResult.CreditTotal - financeResult.DebitTotal;

            if (addPandL)
            {
                if (financeResult.Profit > 0)
                {
                    financeResult.DebitList.Add(new TrailBalanceLedger()
                    {
                        coremode = "Dr",
                        corebalance = financeResult.Profit * -1,
                        debit = financeResult.Profit,
                        displayname = "Gross Profit",
                        balance = financeResult.Profit,
                        balancemode = "Dr"
                    }); ;
                }
                else
                {
                    financeResult.CreditList.Add(new TrailBalanceLedger()
                    {
                        corebalance = financeResult.Loss,
                        displayname = "Gross Loss",
                        credit = financeResult.Loss,
                        balance = Math.Abs(financeResult.Loss),
                        balancemode = "Cr",
                        coremode = "Cr",
                    });
                }
            }
            return financeResult;
        }

        public FinanceResult ProfitLossAccount(long companyid, long locationid, DateTime from, DateTime to, bool addPandL)
        {

            var response = TrailBalanceLedgers(companyid, locationid, from, to);
            FinanceResult tradingAccount = new FinanceResult();


            tradingAccount.DebitList = response.FindAll(e => e.coretypeid == 15 || e.coretypeid == 5 || e.coretypeid == 17).FindAll(x => !x.only4tp && !x.isposting);
            tradingAccount.CreditList = response.FindAll(e => e.coretypeid == 14 || e.coretypeid == 4).FindAll(x => !x.only4tp && !x.isposting);


            decimal openingstockvalue = 0;
            decimal closingstockvalue = 0;
            if (LoadStockFromOpening)
            {
                LedgerstockopeningDataService ledgerstockopeningDataService = new LedgerstockopeningDataService() { Context = this.Context };
                var stockopening = ledgerstockopeningDataService.FindStockOpening(new LedgerstockopeningCreteria() { fromdate = from, todate = to, companyid = companyid });
                openingstockvalue = stockopening.opening.value;
                closingstockvalue = stockopening.closing == null ? FindStockAsOnDate(companyid, locationid, to).OPENING : stockopening.closing.value;
            }
            else
            {
                var stockason = FindStockAsOnDate(companyid, locationid, from);
                openingstockvalue = stockason.OPENING;
                closingstockvalue = to.Date >= DateTime.Now.Date ? stockason.CLOSING : FindStockAsOnDate(companyid, locationid, to).OPENING;
            }

            if (openingstockvalue > 0)
            {
                tradingAccount.DebitList.Insert(0, new TrailBalanceLedger()
                {
                    displayname = "Opening Stock",
                    debit = openingstockvalue
                });
            }
            if (closingstockvalue > 0)
            {
                tradingAccount.CreditList.Add(new TrailBalanceLedger()
                {
                    displayname = "Closing Stock",
                    credit = closingstockvalue
                });
            }


            if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.fin_nonbarcodedvalue)
            {

                string query = @"select sum((lrinvoiceitems.buyingqty - (lrinvoiceitems.sellableqty+lrinvoiceitems.returnqty))*buyingprice) from lrinvoice 
inner join lrinvoiceitems on lrinvoiceitems.lrinvoiceid = lrinvoice.id and lrinvoiceitems.isactive=true
where lrinvoice.entrydate>=@fromdate and lrinvoice.entrydate<@todate and lrinvoice.companyid=@companyid and lrinvoice.isactive=true
and lrinvoiceitems.buyingqty - (lrinvoiceitems.sellableqty+lrinvoiceitems.returnqty)>0;

select sum((stock.qty-stock.transfered)*lrinvoiceitems.buyingprice) from lrinvoice 
inner join lrinvoiceitems on lrinvoiceitems.lrinvoiceid = lrinvoice.id and lrinvoiceitems.isactive=true
inner join stock on stock.invoiceitemid = lrinvoiceitems.id
where lrinvoice.entrydate>=@fromdate and lrinvoice.entrydate<@todate and lrinvoice.companyid=@companyid and lrinvoice.isactive=true and stock.isactive=true
and stock.qty-stock.transfered>0;

select 
sum(case when lrinvoicetax.type=1 then lrinvoicetax.amounton else lrinvoicetax.netamount-lrinvoicetax.taxamount end)  taxable
from lrinvoice 
inner join lrinvoicetax on lrinvoicetax.invoiceid = lrinvoice.id
left outer join lrinvoiceitems on lrinvoiceitems.lrinvoiceid = lrinvoice.id
where lrinvoice.entrydate>=@fromdate and lrinvoice.entrydate<@todate and lrinvoice.companyid=@companyid and lrinvoice.isactive=true 
and lrinvoicetax.isactive = true 
and (lrinvoicetax.type=0 and lrinvoicetax.taxid=0 and lrinvoicetax.taxamount=0)=false
and lrinvoicetax.type not in (2,3,9,14)
and lrinvoiceitems.id is null";

                decimal invoicependingvalue = 0;
                var param = new SortedList<string, object>();
                param.Add("fromdate", from);
                param.Add("todate", to); ;
                param.Add("companyid", companyid);
                using (var reader = ExecuteReader(query, param, 1000))
                {
                    if (reader.Read())
                    {
                        invoicependingvalue += reader.IsDBNull(0) ? 0 : reader.GetDecimal(0);
                    }
                    if (reader.NextResult() && reader.Read())
                    {
                        invoicependingvalue += reader.IsDBNull(0) ? 0 : reader.GetDecimal(0);
                    }
                    if (reader.NextResult() && reader.Read())
                    {
                        invoicependingvalue += reader.IsDBNull(0) ? 0 : reader.GetDecimal(0);
                    }
                }
                if (invoicependingvalue > 0)
                {
                    tradingAccount.CreditList.Add(new TrailBalanceLedger()
                    {
                        displayname = "Nonbarcoded Stock",
                        credit = invoicependingvalue
                    });
                }
            }

            tradingAccount.CreditTotal = tradingAccount.CreditList.Sum(e => e.credit - e.debit);
            tradingAccount.DebitTotal = tradingAccount.DebitList.Sum(e => e.debit - e.credit);
            if (tradingAccount.DebitTotal > tradingAccount.CreditTotal)
                tradingAccount.Loss = tradingAccount.DebitTotal - tradingAccount.CreditTotal;
            else
                tradingAccount.Profit = tradingAccount.CreditTotal - tradingAccount.DebitTotal;

            tradingAccount.DebitList = response.FindAll(e => e.coretypeid == 10 || e.coretypeid == 18).FindAll(x => !x.only4tp);
            tradingAccount.CreditList = response.FindAll(e => e.coretypeid == 9).FindAll(x => !x.only4tp);

            if (tradingAccount.Profit > 0)
            {
                tradingAccount.CreditList.Insert(0, new TrailBalanceLedger()
                {
                    corebalance = tradingAccount.Profit,
                    displayname = "Gross Profit",
                    balance = tradingAccount.Profit,
                    balancemode = "Cr",
                    coremode = "Cr",
                    credit = tradingAccount.Profit
                });
            }
            else
            {
                tradingAccount.DebitList.Insert(0, new TrailBalanceLedger()
                {
                    corebalance = tradingAccount.Loss,
                    displayname = "Gross Loss",
                    balance = Math.Abs(tradingAccount.Loss),
                    balancemode = "Dr",
                    coremode = "Dr",
                    debit = tradingAccount.Loss
                });
            }

            tradingAccount.Loss = 0;
            tradingAccount.Profit = 0;
            tradingAccount.CreditTotal = tradingAccount.CreditList.Sum(e => e.credit - e.debit);
            tradingAccount.DebitTotal = tradingAccount.DebitList.Sum(e => e.debit - e.credit);
            if (tradingAccount.DebitTotal > tradingAccount.CreditTotal)
                tradingAccount.Loss = tradingAccount.DebitTotal - tradingAccount.CreditTotal;
            else
                tradingAccount.Profit = tradingAccount.CreditTotal - tradingAccount.DebitTotal;

            if (addPandL)
            {
                if (tradingAccount.Profit > 0)
                {
                    tradingAccount.DebitList.Add(new TrailBalanceLedger()
                    {
                        corebalance = tradingAccount.Profit * -1,
                        displayname = "Net Profit",
                        debit = tradingAccount.Profit,
                        balancemode = "Dr"
                    });
                }
                else
                {
                    tradingAccount.CreditList.Add(new TrailBalanceLedger()
                    {
                        corebalance = tradingAccount.Loss,
                        displayname = "Net Loss",
                        credit = tradingAccount.Loss,
                        balancemode = "Cr"
                    });
                }
            }
            return tradingAccount;
        }
        public FinanceResult BalanceSheetDetail(long companyid, long locationid, DateTime from, DateTime to)
        {
            //FinanceResult tradingAccount = ProfitLossAccount(companyid, financeyear, false);
            var obresponse = TrailBalanceLedgers(companyid, locationid, DateTime.MinValue, from.AddDays(-1));
            var response = TrailBalanceLedgers(companyid, locationid, from, to);


            FinanceResult tradingAccount = new FinanceResult();

            tradingAccount.DebitList = response.FindAll(e => e.coretypeid == 15 || e.coretypeid == 5 || e.coretypeid == 17).FindAll(x => !x.only4tp);
            tradingAccount.CreditList = response.FindAll(e => e.coretypeid == 14 || e.coretypeid == 4).FindAll(x => !x.only4tp);

            Data.WarehouseReportDataService warehouseReportDataService = new Data.WarehouseReportDataService() { Context = this.Context };
            var stockreport = warehouseReportDataService.SearchAsOnStock(new StockReportCreteria()
            {
                fromdate = from,
                todate = to
            });

            var openingstockvalue = stockreport.Sum(x => x.openingstockvalue);
            var closingstockvalue = stockreport.Sum(x => x.closingstockvalue);


            if (openingstockvalue > 0)
            {
                tradingAccount.DebitList.Insert(0, new TrailBalanceLedger()
                {
                    displayname = "Opening Stock",
                    debit = Math.Round(openingstockvalue)
                });
            }
            if (closingstockvalue > 0)
            {
                tradingAccount.CreditList.Add(new TrailBalanceLedger()
                {
                    displayname = "Closing Stock",
                    credit = Math.Round(closingstockvalue)

                });
            }

            tradingAccount.CreditTotal = tradingAccount.CreditList.Sum(e => e.credit - e.debit);
            tradingAccount.DebitTotal = tradingAccount.DebitList.Sum(e => e.debit - e.credit);
            if (tradingAccount.DebitTotal > tradingAccount.CreditTotal)
                tradingAccount.Loss = tradingAccount.DebitTotal - tradingAccount.CreditTotal;
            else
                tradingAccount.Profit = tradingAccount.CreditTotal - tradingAccount.DebitTotal;

            tradingAccount.DebitList = response.FindAll(e => e.coretypeid == 10 || e.coretypeid == 18).FindAll(x => !x.only4tp);
            tradingAccount.CreditList = response.FindAll(e => e.coretypeid == 9).FindAll(x => !x.only4tp);

            if (tradingAccount.Profit > 0)
            {
                tradingAccount.CreditList.Insert(0, new TrailBalanceLedger()
                {
                    corebalance = tradingAccount.Profit,
                    displayname = "Gross Profit",
                    balance = tradingAccount.Profit,
                    balancemode = "Cr",
                    coremode = "Cr",
                    credit = tradingAccount.Profit
                });
            }
            else
            {
                tradingAccount.DebitList.Insert(0, new TrailBalanceLedger()
                {
                    corebalance = tradingAccount.Loss,
                    displayname = "Gross Loss",
                    balance = Math.Abs(tradingAccount.Loss),
                    balancemode = "Dr",
                    coremode = "Dr",
                    debit = tradingAccount.Loss
                });
            }

            tradingAccount.CreditTotal = tradingAccount.CreditList.Sum(e => e.credit - e.debit);
            tradingAccount.DebitTotal = tradingAccount.DebitList.Sum(e => e.debit - e.credit);
            FinanceResult financeResult = new FinanceResult();
            if (tradingAccount.DebitTotal > tradingAccount.CreditTotal)
                financeResult.Loss = tradingAccount.DebitTotal - tradingAccount.CreditTotal;
            else
                financeResult.Profit = tradingAccount.CreditTotal - tradingAccount.DebitTotal;

            foreach (var ledger in response)
            {
                if (obresponse.Exists(x => x.displayname == ledger.displayname &&
                    x.coretype == ledger.coretype))
                {
                    var ob = obresponse.Find(x => x.displayname == ledger.displayname &&
                    x.coretype == ledger.coretype);
                    obresponse.RemoveAll(x => x.displayname == ledger.displayname &&
                    x.coretype == ledger.coretype);
                    ledger.OpeningCR = ob.credit;
                    ledger.OpeningDR = ob.debit;
                }
            }

            foreach (var ledger in obresponse)
            {
                ledger.OpeningCR = ledger.credit;
                ledger.OpeningDR = ledger.debit;
                ledger.credit = 0;
                ledger.debit = 0;
                var duplicates = response.FindAll(X => X.displayname == ledger.displayname);
                if (duplicates.Count > 0 &&
                    duplicates[0].coretype == ledger.coretype)
                {
                    duplicates[0].OpeningCR += ledger.credit;
                    duplicates[0].OpeningDR += ledger.debit;
                }
                else
                {
                    response.Add(ledger);
                }
            }

            foreach (var ledger in response)
            {
                ledger.ClosingCR = ledger.OpeningCR + ledger.credit;
                ledger.ClosingDR = ledger.OpeningDR + ledger.debit;
            }

            financeResult.CreditList = response.FindAll(e => e.coretypeid == 1 || e.coretypeid == 12 ||
                                                        e.coretypeid == 3 || e.coretypeid == 16);

            financeResult.DebitList = response.FindAll(e => e.coretypeid == 2 || e.coretypeid == 11 ||
                                          e.coretypeid == 6 || e.coretypeid == 7 || e.coretypeid == 13);

            //var creditor = financeResult.CreditList.Sum(x =>
            //(x.coretypeid == 12 || x.coretypeid == 3 || x.coretypeid == 16) ? x.credit - x.debit : 0);

            //if (creditor < 0)
            //{
            //    var swap = financeResult.CreditList.FindAll(x => (x.coretypeid == 12 || x.coretypeid == 3 || x.coretypeid == 16));
            //    financeResult.CreditList.RemoveAll(x => (x.coretypeid == 12 || x.coretypeid == 3 || x.coretypeid == 16));
            //    foreach (var swapitem in swap)
            //    {
            //        swapitem.debit = swapitem.debit - swapitem.credit;
            //        swapitem.credit = 0;
            //        financeResult.DebitList.Insert(0, swapitem);
            //    }
            //}


            //if (financeResult.DebitList.Exists(x => x.credit > x.debit))
            //{
            //    var swap = financeResult.DebitList.FindAll(x => x.credit > x.debit);
            //    financeResult.DebitList.RemoveAll(x => x.credit > x.debit);
            //    foreach (var swapitem in swap)
            //    {
            //        swapitem.credit = swapitem.credit - swapitem.debit;
            //        swapitem.debit = 0;
            //        financeResult.CreditList.Add(swapitem);
            //    }
            //}

            if (financeResult.Profit > 0)
            {
                financeResult.CreditList.Add(new TrailBalanceLedger()
                {
                    corebalance = financeResult.Profit,
                    displayname = "Net Profit",
                    balance = financeResult.Profit,
                    credit = financeResult.Profit,
                    balancemode = "Cr",
                    coremode = "Cr"
                });
            }
            else
            {
                financeResult.DebitList.Add(new TrailBalanceLedger()
                {
                    corebalance = financeResult.Loss,
                    displayname = "Net Loss",
                    balance = Math.Abs(financeResult.Loss),
                    debit = financeResult.Loss,
                    balancemode = "Dr",
                    coremode = "Dr"
                });
            }
            financeResult.CreditTotal = financeResult.CreditList.Sum(e => e.corebalance);
            financeResult.DebitTotal = financeResult.DebitList.Sum(e => e.corebalance);
            return financeResult;
        }
        public FinanceResult BalanceSheet(long companyid, long locationid, DateTime from, DateTime to)
        {
            //FinanceResult tradingAccount = ProfitLossAccount(companyid, financeyear, false);
            /*var response = TrailBalanceLedgers(companyid, locationid, from, to);


            FinanceResult tradingAccount = new FinanceResult();

            tradingAccount.DebitList = response.FindAll(e => e.coretypeid == 15 || e.coretypeid == 5 || e.coretypeid == 17).FindAll(x => !x.only4tp && !x.isposting);
            tradingAccount.CreditList = response.FindAll(e => e.coretypeid == 14 || e.coretypeid == 4).FindAll(x => !x.only4tp && !x.isposting);


            tradingAccount.DebitList.ForEach(x => response.Remove(x));
            tradingAccount.CreditList.ForEach(x => response.Remove(x));

            Data.WarehouseReportDataService warehouseReportDataService = new Data.WarehouseReportDataService() { Context = this.Context };
            var stockreport = warehouseReportDataService.SearchAsOnStock(new StockReportCreteria()
            {
                fromdate = from,
                todate = to
            });

            var openingstockvalue = stockreport.Sum(x => x.openingstockvalue);
            var closingstockvalue = stockreport.Sum(x => x.closingstockvalue);


            if (openingstockvalue > 0)
            {
                tradingAccount.DebitList.Insert(0, new TrailBalanceLedger()
                {
                    displayname = "Opening Stock",
                    debit = Math.Round(openingstockvalue)
                });
            }
            if (closingstockvalue > 0)
            {
                tradingAccount.CreditList.Add(new TrailBalanceLedger()
                {
                    displayname = "Closing Stock",
                    credit = Math.Round(closingstockvalue)

                });
            }

            tradingAccount.CreditTotal = tradingAccount.CreditList.Sum(e => e.credit - e.debit);
            tradingAccount.DebitTotal = tradingAccount.DebitList.Sum(e => e.debit - e.credit);
            if (tradingAccount.DebitTotal > tradingAccount.CreditTotal)
                tradingAccount.Loss = tradingAccount.DebitTotal - tradingAccount.CreditTotal;
            else
                tradingAccount.Profit = tradingAccount.CreditTotal - tradingAccount.DebitTotal;

            FinanceResult profitandloss = new FinanceResult();
            profitandloss.DebitList = response.FindAll(e => e.coretypeid == 10 || e.coretypeid == 18).FindAll(x => !x.only4tp);
            profitandloss.CreditList = response.FindAll(e => e.coretypeid == 9).FindAll(x => !x.only4tp);

            profitandloss.DebitList.ForEach(x => response.Remove(x));
            profitandloss.CreditList.ForEach(x => response.Remove(x));

            if (tradingAccount.Profit > 0)
            {
                profitandloss.CreditList.Insert(0, new TrailBalanceLedger()
                {
                    corebalance = tradingAccount.Profit,
                    displayname = "Gross Profit",
                    balance = tradingAccount.Profit,
                    balancemode = "Cr",
                    coremode = "Cr",
                    credit = tradingAccount.Profit
                });
            }
            else
            {
                profitandloss.DebitList.Insert(0, new TrailBalanceLedger()
                {
                    corebalance = tradingAccount.Loss,
                    displayname = "Gross Loss",
                    balance = Math.Abs(tradingAccount.Loss),
                    balancemode = "Dr",
                    coremode = "Dr",
                    debit = tradingAccount.Loss
                });
            }

            profitandloss.CreditTotal = profitandloss.CreditList.Sum(e => e.credit - e.debit);
            profitandloss.DebitTotal = profitandloss.DebitList.Sum(e => e.debit - e.credit);
            FinanceResult financeResult = new FinanceResult();
            if (profitandloss.DebitTotal > profitandloss.CreditTotal)
                financeResult.Loss = profitandloss.DebitTotal - profitandloss.CreditTotal;
            else
                financeResult.Profit = profitandloss.CreditTotal - profitandloss.DebitTotal;
            */

            var obresponse = TrailBalanceLedgers(companyid, locationid, DateTime.MinValue, from.AddDays(-1));
            var response = TrailBalanceLedgers(companyid, locationid, from, to);

            FinanceResult tradingAccount = new FinanceResult();


            tradingAccount.DebitList = response.FindAll(e => e.coretypeid == 15 || e.coretypeid == 5 || e.coretypeid == 17).FindAll(x => !x.only4tp && !x.isposting);
            tradingAccount.CreditList = response.FindAll(e => e.coretypeid == 14 || e.coretypeid == 4).FindAll(x => !x.only4tp && !x.isposting);


            decimal openingstockvalue = 0;
            decimal closingstockvalue = 0;
            if (LoadStockFromOpening)
            {
                LedgerstockopeningDataService ledgerstockopeningDataService = new LedgerstockopeningDataService() { Context = this.Context };
                var stockopening = ledgerstockopeningDataService.FindStockOpening(new LedgerstockopeningCreteria() { fromdate = from, todate = to, companyid = companyid });
                openingstockvalue = stockopening.opening.value;
                closingstockvalue = stockopening.closing == null ? FindStockAsOnDate(companyid, locationid, to).OPENING : stockopening.closing.value;
            }
            else
            {
                var stockason = FindStockAsOnDate(companyid, locationid, from);
                openingstockvalue = stockason.OPENING;
                closingstockvalue = to.Date >= DateTime.Now.Date ? stockason.CLOSING : FindStockAsOnDate(companyid, locationid, to).OPENING;
            }




            decimal invoicependingvalue = 0;
            if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.fin_nonbarcodedvalue)
            {

                string query = @"select sum((lrinvoiceitems.buyingqty - (lrinvoiceitems.sellableqty+lrinvoiceitems.returnqty))*buyingprice) from lrinvoice 
inner join lrinvoiceitems on lrinvoiceitems.lrinvoiceid = lrinvoice.id and lrinvoiceitems.isactive=true
where lrinvoice.entrydate>=@fromdate and lrinvoice.entrydate<@todate and lrinvoice.companyid=@companyid and lrinvoice.isactive=true
and lrinvoiceitems.buyingqty - (lrinvoiceitems.sellableqty+lrinvoiceitems.returnqty)>0;

select sum((stock.qty-stock.transfered)*lrinvoiceitems.buyingprice) from lrinvoice 
inner join lrinvoiceitems on lrinvoiceitems.lrinvoiceid = lrinvoice.id and lrinvoiceitems.isactive=true
inner join stock on stock.invoiceitemid = lrinvoiceitems.id
where lrinvoice.entrydate>=@fromdate and lrinvoice.entrydate<@todate and lrinvoice.companyid=@companyid and lrinvoice.isactive=true and stock.isactive=true
and stock.qty-stock.transfered>0;

select 
sum(case when lrinvoicetax.type=1 then lrinvoicetax.amounton else lrinvoicetax.netamount-lrinvoicetax.taxamount end)  taxable
from lrinvoice 
inner join lrinvoicetax on lrinvoicetax.invoiceid = lrinvoice.id
left outer join lrinvoiceitems on lrinvoiceitems.lrinvoiceid = lrinvoice.id
where lrinvoice.entrydate>=@fromdate and lrinvoice.entrydate<@todate and lrinvoice.companyid=@companyid and lrinvoice.isactive=true 
and lrinvoicetax.isactive = true 
and (lrinvoicetax.type=0 and lrinvoicetax.taxid=0 and lrinvoicetax.taxamount=0)=false
and lrinvoicetax.type not in (2,3,9,14)
and lrinvoiceitems.id is null";


                var param = new SortedList<string, object>();
                param.Add("fromdate", from);
                param.Add("todate", to); ;
                param.Add("companyid", companyid);
                using (var reader = ExecuteReader(query, param, 1000))
                {
                    if (reader.Read())
                    {
                        invoicependingvalue += reader.IsDBNull(0) ? 0 : reader.GetDecimal(0);
                    }
                    if (reader.NextResult() && reader.Read())
                    {
                        if (!InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.finance_stocktransaction)
                            invoicependingvalue += reader.IsDBNull(0) ? 0 : reader.GetDecimal(0);
                    }
                    if (reader.NextResult() && reader.Read())
                    {
                        invoicependingvalue += reader.IsDBNull(0) ? 0 : reader.GetDecimal(0);
                    }
                }
                closingstockvalue += invoicependingvalue;
                //if (invoicependingvalue > 0)
                //{
                //    tradingAccount.CreditList.Add(new TrailBalanceLedger()
                //    {
                //        displayname = "Nonbarcoded Stock",
                //        credit = invoicependingvalue
                //    });
                //}
            }

            if (openingstockvalue > 0)
            {
                tradingAccount.DebitList.Insert(0, new TrailBalanceLedger()
                {
                    displayname = "Opening Stock",
                    debit = openingstockvalue,
                    coretype = "Current Asset",
                    parentgroup = "Current Asset"
                });
            }
            if (closingstockvalue > 0)
            {
                tradingAccount.CreditList.Add(new TrailBalanceLedger()
                {
                    displayname = "Closing Stock",
                    credit = closingstockvalue,
                    coretype = "Current Asset",
                    parentgroup = "Current Asset"
                });
            }

            tradingAccount.CreditTotal = tradingAccount.CreditList.Sum(e => e.credit - e.debit);
            tradingAccount.DebitTotal = tradingAccount.DebitList.Sum(e => e.debit - e.credit);
            if (tradingAccount.DebitTotal > tradingAccount.CreditTotal)
                tradingAccount.Loss = tradingAccount.DebitTotal - tradingAccount.CreditTotal;
            else
                tradingAccount.Profit = tradingAccount.CreditTotal - tradingAccount.DebitTotal;

            tradingAccount.DebitList = response.FindAll(e => e.coretypeid == 10 || e.coretypeid == 18).FindAll(x => !x.only4tp);
            tradingAccount.CreditList = response.FindAll(e => e.coretypeid == 9).FindAll(x => !x.only4tp);

            if (tradingAccount.Profit > 0)
            {
                tradingAccount.CreditList.Insert(0, new TrailBalanceLedger()
                {
                    corebalance = tradingAccount.Profit,
                    displayname = "Gross Profit",
                    balance = tradingAccount.Profit,
                    balancemode = "Cr",
                    coremode = "Cr",
                    credit = tradingAccount.Profit
                });
            }
            else
            {
                tradingAccount.DebitList.Insert(0, new TrailBalanceLedger()
                {
                    corebalance = tradingAccount.Loss,
                    displayname = "Gross Loss",
                    balance = Math.Abs(tradingAccount.Loss),
                    balancemode = "Dr",
                    coremode = "Dr",
                    debit = tradingAccount.Loss
                });
            }

            tradingAccount.Loss = 0;
            tradingAccount.Profit = 0;
            tradingAccount.CreditTotal = tradingAccount.CreditList.Sum(e => e.credit - e.debit);
            tradingAccount.DebitTotal = tradingAccount.DebitList.Sum(e => e.debit - e.credit);
            if (tradingAccount.DebitTotal > tradingAccount.CreditTotal)
                tradingAccount.Loss = tradingAccount.DebitTotal - tradingAccount.CreditTotal;
            else
                tradingAccount.Profit = tradingAccount.CreditTotal - tradingAccount.DebitTotal;

            FinanceResult financeResult = new FinanceResult();
            if (tradingAccount.Profit > 0)
            {
                financeResult.Profit = Math.Round(tradingAccount.Profit, 0);
                tradingAccount.DebitList.Add(new TrailBalanceLedger()
                {
                    corebalance = tradingAccount.Profit * -1,
                    displayname = "Net Profit",
                    debit = tradingAccount.Profit,
                    balancemode = "Dr",
                    ClosingDR = tradingAccount.Profit
                });
            }
            else
            {
                financeResult.Loss = Math.Round(tradingAccount.Loss, 0);
                tradingAccount.CreditList.Add(new TrailBalanceLedger()
                {
                    corebalance = tradingAccount.Loss,
                    displayname = "Net Loss",
                    credit = tradingAccount.Loss,
                    balancemode = "Cr",
                });
            }


            //foreach (var ledger in response)
            //{
            //    if (obresponse.Exists(x => x.displayname == ledger.displayname &&
            //        x.coretype == ledger.coretype))
            //    {
            //        var ob = obresponse.Find(x => x.displayname == ledger.displayname &&
            //        x.coretype == ledger.coretype);
            //        obresponse.Remove(ob);
            //        ledger.OpeningCR = Math.Round(ob.credit, 2);
            //        ledger.OpeningDR = Math.Round(ob.debit, 2);
            //    }
            //}

            //foreach (var ledger in obresponse)
            //{
            //    ledger.OpeningCR = ledger.credit;
            //    ledger.OpeningDR = ledger.debit;
            //    ledger.credit = 0;
            //    ledger.debit = 0;
            //    var duplicates = response.FindAll(X => X.displayname == ledger.displayname);
            //    if (duplicates.Count > 0 &&
            //        duplicates[0].coretype == ledger.coretype)
            //    {
            //        duplicates[0].OpeningCR += ledger.credit;
            //        duplicates[0].OpeningDR += ledger.debit;
            //    }
            //    else
            //    {
            //        response.Add(ledger);
            //    }
            //}

            //foreach (var grp in response.GroupBy(x => new { x.displayname, x.coretypeid, x.coretype }).ToList())
            //{
            //    if (grp.Count() > 1)
            //    {
            //        TrailBalanceLedger trailBalanceLedger = new TrailBalanceLedger()
            //        {
            //            displayname = grp.Key.displayname,
            //            coretypeid = grp.Key.coretypeid,
            //            coretype = grp.Key.coretype,
            //        };
            //        trailBalanceLedger.OpeningCR = grp.Sum(x => x.OpeningCR);
            //        trailBalanceLedger.OpeningDR = grp.Sum(x => x.OpeningDR);
            //        if (trailBalanceLedger.OpeningCR > trailBalanceLedger.OpeningDR)
            //        {
            //            trailBalanceLedger.OpeningCR -= trailBalanceLedger.OpeningDR;
            //            trailBalanceLedger.OpeningDR = 0;
            //        }
            //        else
            //        {
            //            trailBalanceLedger.OpeningDR -= trailBalanceLedger.OpeningCR;
            //            trailBalanceLedger.OpeningCR = 0;
            //        }
            //        trailBalanceLedger.credit = grp.Sum(x => x.credit);
            //        trailBalanceLedger.debit = grp.Sum(x => x.debit);
            //        trailBalanceLedger.ClosingCR = Math.Round(trailBalanceLedger.OpeningCR + trailBalanceLedger.credit, 2);
            //        trailBalanceLedger.ClosingDR = Math.Round(trailBalanceLedger.OpeningDR + trailBalanceLedger.debit, 2);

            //        if (trailBalanceLedger.OpeningCR > trailBalanceLedger.OpeningDR)
            //        {
            //            trailBalanceLedger.OpeningCR -= trailBalanceLedger.OpeningDR;
            //            trailBalanceLedger.OpeningDR = 0;
            //        }
            //        else
            //        {
            //            trailBalanceLedger.OpeningDR -= trailBalanceLedger.OpeningCR;
            //            trailBalanceLedger.OpeningCR = 0;
            //        }

            //        grp.ForEach(y =>
            //        {
            //            response.Remove(y);
            //        });
            //    }
            //}


            foreach (var ledger in response)
            {
                ledger.OpeningDR = Math.Round(ledger.OpeningDR, 2);
                ledger.OpeningCR = Math.Round(ledger.OpeningCR, 2);

                ledger.credit = Math.Round(ledger.credit, 2);
                ledger.debit = Math.Round(ledger.debit, 2);

                ledger.ClosingCR = Math.Round(ledger.OpeningCR + ledger.credit, 2);
                ledger.ClosingDR = Math.Round(ledger.OpeningDR + ledger.debit, 2);
            }


            financeResult.CreditList = response.FindAll(e => e.coretypeid == 1 || e.coretypeid == 3 ||
                                                        e.coretypeid == 12 || e.coretypeid == 16 || e.coretypeid == 20 || e.coretypeid == 30);

            financeResult.DebitList = response.FindAll(e => e.coretypeid == 2 || e.coretypeid == 19 || e.coretypeid == 8 ||
                                          e.coretypeid == 6 || e.coretypeid == 7 || e.coretypeid == 13 || e.coretypeid == 11);

            financeResult.DebitList.ForEach(x => response.Remove(x));
            financeResult.CreditList.ForEach(x => response.Remove(x));

            closingstockvalue = Math.Round(closingstockvalue, 0);
            if (closingstockvalue != 0)
            {
                financeResult.DebitList.Add(new TrailBalanceLedger()
                {
                    corebalance = closingstockvalue,
                    balance = closingstockvalue,
                    debit = closingstockvalue,
                    balancemode = "Dr",
                    coremode = "Dr",
                    ClosingDR = closingstockvalue,
                    displayname = "Closing Stock"
                });
            }

            //foreach (var switchitem in financeResult.CreditList.FindAll(x=>x.ClosingCR> x.ClosingDR))
            //{
            //    financeResult.DebitList.Add(switchitem);
            //    financeResult.CreditList.Remove(switchitem);
            //}


            //foreach (var switchitem in financeResult.DebitList.FindAll(x => x.ClosingCR < x.ClosingDR))
            //{
            //    financeResult.CreditList.Add(switchitem);
            //    financeResult.DebitList.Remove(switchitem);
            //}

            //var creditor = financeResult.CreditList.Sum(x =>
            //(x.coretypeid == 1 ||  x.coretypeid == 12 || x.coretypeid == 3 || x.coretypeid == 16) ? x.credit - x.debit : 0);

            //if (creditor < 0)
            //{
            //    var swap = financeResult.CreditList.FindAll(e => (e.coretypeid == 1 || e.coretypeid == 3 ||
            //                                            e.coretypeid == 12 || e.coretypeid == 16 || e.coretypeid == 20 || e.coretypeid == 30));
            //    financeResult.CreditList.RemoveAll(e => (e.coretypeid == 1 || e.coretypeid == 3 ||
            //                                            e.coretypeid == 12 || e.coretypeid == 16 || e.coretypeid == 20 || e.coretypeid == 30));
            //    foreach (var swapitem in swap)
            //    {
            //        swapitem.debit = swapitem.debit - swapitem.credit;
            //        swapitem.credit = 0;
            //        financeResult.DebitList.Insert(0, swapitem);
            //    }
            //}


            //if (financeResult.DebitList.Exists(x => x.credit > x.debit))
            //{
            //    var swap = financeResult.DebitList.FindAll(x => x.credit > x.debit);
            //    financeResult.DebitList.RemoveAll(x => x.credit > x.debit);
            //    foreach (var swapitem in swap)
            //    {
            //        swapitem.credit = swapitem.credit - swapitem.debit;
            //        swapitem.debit = 0;
            //        financeResult.CreditList.Add(swapitem);
            //    }
            //}

            if (financeResult.Profit > 0)
            {
                financeResult.CreditList.Add(new TrailBalanceLedger()
                {
                    corebalance = financeResult.Profit,
                    displayname = "Net Profit",
                    balance = financeResult.Profit,
                    credit = financeResult.Profit,
                    balancemode = "Cr",
                    coremode = "Cr",
                    ClosingCR = financeResult.Profit
                });

            }
            else
            {
                financeResult.DebitList.Add(new TrailBalanceLedger()
                {
                    corebalance = financeResult.Loss,
                    displayname = "Net Loss",
                    balance = Math.Abs(financeResult.Loss),
                    debit = financeResult.Loss,
                    balancemode = "Dr",
                    coremode = "Dr",
                    ClosingDR = financeResult.Loss
                });
            }
            financeResult.CreditTotal = financeResult.CreditList.Sum(e => e.corebalance);
            financeResult.DebitTotal = financeResult.DebitList.Sum(e => e.corebalance);
            return financeResult;
        }

        public List<TrailBalanceLedger> TrailBalance(long companyid, long locationid, int financeyear)
        {
            DateTime fromdate = new DateTime(financeyear, 4, 1);
            DateTime todate = fromdate.AddYears(1);
            return TrailBalance(companyid, locationid, fromdate, todate);
        }

        public List<TrailBalanceLegerOpening> GetOpening(long companyid)
        {
            string query = @"with ALLOPENGING as
            (
                select 'B' mode,ledgerid, sum(receivable) receivable,sum(payable) payable from Voucheritembillwise
                where  voucherid = 0 and isactive=true and obcompanyid=@companyid
                group by ledgerid
                union all
                select 'L' mode,id ledgerid,
                case when opening>0 then opening else 0 end,
			    case when opening<0 then opening*-1 else 0 end
                from ledger where ledger.isactive=true and ledger.opening != 0
            ), finalopening
            as
            (
                select ledgerid,mode,cast(typelist->>'name' as varchar) CoreType,cast(typelist->>'id' as int) CoreTypeid,cast(typelist->>'mode' as varchar) CoreMode,
                        ledger.displayname,ledger.parentgroup,payable credit,receivable debit,
                        case when cast(typelist->>'mode' as varchar)='Dr' then payable-receivable else payable-receivable end as corebalance from ALLOPENGING
                left outer join ledger on ledger.id = ledgerid
                left outer join json_array_elements('[
                            { ""id"": 0, ""name"": ""No Ledger Type"" ,""mode"":""Cr"" },
                            { ""id"": 1, ""name"": ""Capital"" ,""mode"":""Cr"" },
                            { ""id"": 2, ""name"": ""Fixed asset"" ,""mode"":""Cr"" },
                            { ""id"": 3, ""name"": ""Libalities"" ,""mode"":""Cr"" },
                            { ""id"": 4, ""name"": ""Direct Income""  ,""mode"":""Cr""},
                            { ""id"": 5, ""name"": ""Direct Expense""  ,""mode"":""Cr""},
                            { ""id"": 6, ""name"": ""Cash-In-Hand""  ,""mode"":""Cr""},
                            { ""id"": 7, ""name"": ""Bank Account"" ,""mode"":""Cr"" },
                            { ""id"": 8, ""name"": ""Stock-In-Hand""  ,""mode"":""Cr""},
                            { ""id"": 9, ""name"": ""Indirect Incomes""  ,""mode"":""Cr""},
                            { ""id"": 10, ""name"": ""Indirect Expenses""  ,""mode"":""Cr""},
                            { ""id"": 11, ""name"": ""Sundry Debtors""  ,""mode"":""Cr""},
                            { ""id"": 12, ""name"": ""Sundry Creditors""  ,""mode"":""Cr""},
                            { ""id"": 13, ""name"": ""Petty Cash""  ,""mode"":""Cr""},
                            { ""id"": 14, ""name"": ""Sales""  ,""mode"":""Cr""},
                            { ""id"": 15, ""name"": ""Purchase""  ,""mode"":""Cr""},
                            { ""id"": 16, ""name"": ""Tax & Duties"" ,""mode"":""Cr"" },
                            { ""id"": 17, ""name"": ""Petty Cash Direct Expense"" ,""mode"":""Cr"" },
                            { ""id"": 18, ""name"": ""Petty Cash In-Direct Expense"" ,""mode"":""Cr"" },
                            { ""id"": 19, ""name"": ""Current asset"" ,""mode"":""Cr"" },
                            { ""id"": 20, ""name"": ""Current Liablities"" ,""mode"":""Cr"" },
                            { ""id"": 21, ""name"": ""Petty Cash Direct Income"" ,""mode"":""Cr"" },
                            { ""id"": 22, ""name"": ""Petty Cash In-Direct Income"" ,""mode"":""Cr"" },
                            { ""id"": 30, ""name"": ""Rounding"",""mode"":""Cr""  }]') typelist on cast(typelist->>'id' as bigint) = typeid
            )

            select * from finalopening 
            order by mode";

            var param = new SortedList<string, object>();
            param.Add("companyid", companyid);
            List<TrailBalanceLegerOpening> ob = new List<TrailBalanceLegerOpening>();
            using (var reader = ExecuteReader(query,param))
            {
                while (reader.Read())
                {
                    TrailBalanceLegerOpening trailBalanceLegerOpening = new TrailBalanceLegerOpening();
                    trailBalanceLegerOpening.ledgerid = reader["ledgerid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["ledgerid"]);
                    trailBalanceLegerOpening.credit = reader["credit"] == DBNull.Value ? 0 : Convert.ToInt64(reader["credit"]);
                    trailBalanceLegerOpening.debit = reader["debit"] == DBNull.Value ? 0 : Convert.ToInt64(reader["debit"]);

                    trailBalanceLegerOpening.coremode = reader["coremode"] == DBNull.Value ? "" : reader["coremode"].ToString();
                    trailBalanceLegerOpening.coretype = reader["coretype"] == DBNull.Value ? "" : reader["coretype"].ToString();
                    trailBalanceLegerOpening.coretypeid = reader["CoreTypeid"] == DBNull.Value ? 0 : Convert.ToInt32(reader["CoreTypeid"].ToString());
                    trailBalanceLegerOpening.displayname = reader["displayname"] == DBNull.Value ? "" : reader["displayname"].ToString();
                    trailBalanceLegerOpening.parentgroup = reader["parentgroup"] == DBNull.Value ? "" : reader["parentgroup"].ToString();

                    if (!ob.Exists(x => x.ledgerid == trailBalanceLegerOpening.ledgerid))
                        ob.Add(trailBalanceLegerOpening);
                }
            }
            return ob;
        }

        public List<TrailBalanceLedger> TrailBalance(long companyid, long locationid, DateTime fromdate, DateTime todate)
        {
            //InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.finance_trace = true;
            var obresponse = TrailBalanceLedgers(companyid, locationid, DateTime.MinValue, fromdate.AddDays(-1));
            //List<TrailBalanceLedger> obresponse = new List<TrailBalanceLedger>();
            var response = TrailBalanceLedgers(companyid, locationid, fromdate, todate);
            var obledgers = GetOpening(companyid);


            foreach (var grp in obresponse.GroupBy(x => new { x.displayname, x.coretype, x.coretypeid }).ToList())
            {
                if (grp.Count() > 1)
                {
                    var items = grp.ToList();
                    obresponse.Add(new TrailBalanceLedger()
                    {
                        coretype = grp.Key.coretype,
                        ledgerid = grp.First().ledgerid,
                        coretypeid = grp.Key.coretypeid,
                        displayname = grp.Key.displayname,
                        credit = grp.Sum(x => x.credit),
                        debit = grp.Sum(x => x.debit),
                    });
                    obresponse.RemoveAll(x => items.IndexOf(x) >= 0);
                }
            }


            foreach (var grp in obresponse.ToList())
            {
                if (grp.ledgerid > 0 && obresponse.Exists(x => x.ledgerid == grp.ledgerid))
                {
                    obresponse.Find(x => x.ledgerid == grp.ledgerid).credit += grp.credit;
                    obresponse.Find(x => x.ledgerid == grp.ledgerid).debit += grp.debit;
                    obresponse.Remove(grp);
                }
            }


            if (obledgers.Count > 0)
            {
                obledgers.ForEach(e =>
                {
                    if (obresponse.Exists(x => x.ledgerid == e.ledgerid))
                    {
                        obresponse.Find(x => x.ledgerid == e.ledgerid).credit += e.credit;
                        obresponse.Find(x => x.ledgerid == e.ledgerid).debit += e.debit;
                    }
                    else if (obresponse.Exists(x => x.displayname == e.displayname))
                    {
                        obresponse.Find(x => x.displayname == e.displayname).credit += e.credit;
                        obresponse.Find(x => x.displayname == e.displayname).debit += e.debit;
                    }
                    else
                    {
                        obresponse.Add(new TrailBalanceLedger()
                        {
                            ledgerid = e.ledgerid,
                            credit = e.credit,
                            debit = e.debit,
                            coremode = e.coremode,
                            coretype = e.coretype,
                            coretypeid = e.coretypeid,
                            displayname = e.displayname,
                            parentgroup = e.parentgroup
                        });
                    }
                });
            }

            
            if (!InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.finance_trace)
            {
                foreach (var grp in response.GroupBy(x => new { x.displayname, x.coretype, x.coretypeid }).ToList())
                {
                    if (grp.Count() > 1)
                    {
                        var items = grp.ToList();
                        response.Add(new TrailBalanceLedger()
                        {
                            coretype = grp.Key.coretype,
                            coretypeid = grp.Key.coretypeid,
                            displayname = grp.Key.displayname,
                            credit = grp.Sum(x => x.credit),
                            debit = grp.Sum(x => x.debit),
                        });
                        response.RemoveAll(x => items.IndexOf(x) >= 0);
                    }
                }

            }

            foreach (var ledger in response)
            {
                if (obresponse.Exists(x => x.displayname == ledger.displayname &&
                    x.coretype == ledger.coretype))
                {
                    var ob = obresponse.FindAll(x => x.displayname == ledger.displayname &&
                    x.coretype == ledger.coretype);
                    ob.ForEach(x =>
                    { 
                        obresponse.Remove(x);
                    });
                    ledger.OpeningCR = ob.Sum(x=>x.credit);
                    ledger.OpeningDR = ob.Sum(x=>x.debit);
                }
            }

            foreach (var ledger in obresponse)
            {
                ledger.OpeningCR = ledger.credit;
                ledger.OpeningDR = ledger.debit;
                ledger.credit = 0;
                ledger.debit = 0;
                var duplicates = response.FindAll(X => X.displayname == ledger.displayname);
                if (duplicates.Count > 0 &&
                    duplicates[0].coretype == ledger.coretype)
                {
                    duplicates[0].OpeningCR += ledger.OpeningCR;
                    duplicates[0].OpeningDR += ledger.OpeningDR;
                }
                else
                {
                    response.Add(ledger);
                }
            }

            if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.finance_tbexclinexp)
            {
                List<int> inexptypes = new List<int>(new int[] { 4, 5, 9, 10, 14, 15, 17, 18, 21, 22 });
                response.FindAll(x => inexptypes.Contains(x.coretypeid)).ForEach(x =>
                {
                    x.OpeningCR = 0;
                    x.OpeningDR = 0;
                });
            }


            response.ForEach(x =>
            {
                x.OpeningCR = Math.Round(x.OpeningCR);
                x.OpeningDR = Math.Round(x.OpeningDR);
                x.debit = Math.Round(x.debit);
                x.credit = Math.Round(x.credit);

                if(x.credit<0)
                {
                    x.debit = x.debit + (x.credit * -1);
                    x.credit = 0;
                }
                else if (x.debit< 0)
                {
                    x.credit = x.credit + (x.debit * -1);
                    x.debit = 0;
                }

                //x.displayname = System.Text.RegularExpressions.Regex.Replace(x.displayname, @"[^a-zA-Z0-9\s%@_.-]", "");
                if (string.IsNullOrEmpty(x.displayname))
                    x.displayname = "UNKNOWN";
            });

            List<TrailBalanceLedger> finallist = new List<TrailBalanceLedger>();
            foreach (var grpitem in response.GroupBy(x => x.displayname))
            {
                if (grpitem.Count() == 1)
                {
                    finallist.Add(grpitem.First());
                }
                else
                {
                    var tbl = grpitem.First();
                    var dlist = grpitem.ToList();
                    for (int i = 1; i < dlist.Count; i++)
                    {
                        tbl.OpeningCR += dlist[i].OpeningCR;
                        tbl.OpeningDR += dlist[i].OpeningDR;
                        tbl.credit += dlist[i].credit;
                        tbl.debit += dlist[i].debit;
                    }
                    finallist.Add(tbl);
                }
            }
            response = finallist;
            //if (response == null) return new List<TrailBalanceLedger>();

            var result = new List<TrailBalanceLedger>();
            int id = 1;
            foreach (var grp in response.GroupBy(e => e.coretype))
            {


                grp.ToList().ForEach(e =>
                    {
                        e.itemlevel = 1;
                        var itembalance = e.credit - e.debit;
                        e.balancemode = itembalance > 0 ? "Cr" : "Dr";
                        e.balance = Math.Abs(itembalance);
                        e.ledgerid = id;
                        e.ClosingDR = e.OpeningDR + e.debit;
                        e.ClosingCR = e.OpeningCR + e.credit;
                    }
                );

                var rst = new TrailBalanceLedger()
                {
                    ledgerid = id++,
                    displayname = grp.Key,
                    coremode = grp.First().coremode,
                    debit = grp.Sum(e => e.debit), //grp.Sum(e => e.balancemode == "Dr" ? e.balance : 0),
                    credit = grp.Sum(e => e.credit), //grp.Sum(e => e.balancemode == "Cr" ? e.balance : 0),
                    OpeningCR = grp.Sum(x => x.OpeningCR),
                    OpeningDR = grp.Sum(x => x.OpeningDR),
                    ClosingCR = grp.Sum(x => x.ClosingCR),
                    ClosingDR = grp.Sum(x => x.ClosingDR),
                    itemlevel = 0
                };
                var corebalance = rst.credit - rst.debit;
                rst.balancemode = corebalance > 0 ? rst.coremode : (rst.coremode == "Cr" ? "Dr" : "Cr");
                rst.balance = Math.Abs(corebalance);
                result.Add(rst);
                result.AddRange(grp.OrderBy(x => x.displayname).ToList());
            }



            //var fulltext = string.Join("\r\n", result.ConvertAll(x => string.Format("{0},{1},{2}", x.displayname, x.credit, x.debit)));

            return result;
        }
    }
}