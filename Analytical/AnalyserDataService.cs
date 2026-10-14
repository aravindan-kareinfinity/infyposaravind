using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
using Newtonsoft.Json;
using System.Text;
using InfyPOS.Data;
using System.Diagnostics;

namespace InfyPOS
{
    public class AnalyserDataService : ReportData
    {
        public class AdditionalColumn
        {
            public Options option { get; set; }
            public int index { get; set; }
            public string customoption { get; set; }
            public string columnname { get; set; }
            public string headername { get; set; }
            public string key { get; set; }
            public bool hasreference { get; set; }
            public bool isdate
            {
                get
                {
                    return option == Options.BillDate || option == Options.DebitNoteDate ||
                        option == Options.InvDate || option == Options.LRDate ||
                        option == Options.PurchaseDate || option == Options.LREntryDate ||
                        option == Options.BarcodedOn || 
                        option == Options.Customer_DOB || option == Options.Customer_Anniversary;
                }
            }
            public bool isnumber
            {
                get
                {
                    return option == Options.AddDiscountRate ||
                        option == Options.AvgSaleDays ||
                        option == Options.AvgStockDays ||
                        option == Options.Customer_Age ||
                        option == Options.DealerPrice ||
                        option == Options.DealerPrice_Range ||
                        option == Options.DebitNoteQty ||
                        option == Options.DebitNoteValue ||
                        option == Options.DiscountRate ||
                        option == Options.DiscountRate_Range ||
                        option == Options.InwardQty ||
                        option == Options.IStockValue ||
                        option == Options.JournalQty ||
                        option == Options.JournalValue ||
                        option == Options.Margin ||
                        option == Options.OutwardQty ||
                        option == Options.PurchaseMonth ||
                        option == Options.PurchaseQty ||
                        option == Options.PurchaseRate ||
                        option == Options.PurchaseRate_Range ||
                        option == Options.PurchaseValue ||
                        option == Options.PurchaseYear ||
                        option == Options.RetailMargin_Range ||
                        option == Options.ReturnQty ||
                        option == Options.ReturnValue ||
                        option == Options.SaleMargin_Range ||
                        option == Options.SaleMonth ||
                        option == Options.SaleMonthDay ||
                        option == Options.SaleQty ||
                        option == Options.SaleRate ||
                        option == Options.SaleRate_Range ||
                        option == Options.SaleValue ||
                        option == Options.SaleWeekDay ||
                        option == Options.SaleYear ||
                        option == Options.SoldRate ||
                        option == Options.SoldRate_Range ||
                        option == Options.StockAge_Range ||
                        option == Options.ShelfAge_Range ||
                        option == Options.StockQty ||
                        option == Options.StockSaleValue ||
                        option == Options.StockValue ||
                        option == Options.Tax ||
                        option == Options.TaxPercentage;
                }
            }
        }
        public class Configuration : BaseEntity
        {
            public enum ReportType
            {
                WarehouseStock = 0,
                RetailStock = 1,
                Sales = 2,
                AllStock = 3,
                SalesCompare = 4,
                Purchase = 5,
                SaleVsPurchase = 6,
                StockStress = 7,
                PurchasePlan = 8,
                SalesPlan = 9,
                Conversion = 10,
                SalesAsOnDateAnalyser = 11,
                Customers = 12,
                SalesVsStockAnalyser = 13,
                StockMarker = 14,
                PurchaseReturn = 15,
                SalesOnApproval = 16
            }
            [JsonIgnore]
            public bool issingleyearcreteria { get; set; }
            public bool isreport { get; set; }
            public int ImageRows { get; set; }
            public ReportType reporttype { get; set; }
            [JsonIgnore]
            public ReportType currentreporttype { get; set; }
            public long locationid { get; set; }
            public DateTime fromdate { get; set; }
            public DateTime todate { get; set; }
            public DateTime fromdatecompare { get; set; }
            public DateTime todatecompare { get; set; }
            public Options PrimaryCondtion { get; set; }
            public string CustomOption { get; set; }
            public Condtion.Range PrimaryRange { get; set; }
            public List<Condtion> SecondaryCondtions { get; set; }
            public List<string> Columns { get; set; }
            public string GroupColumn { get; set; }
            public int GroupColumnFieldIndex { get; set; }
            public List<AdditionalColumn> AdditionalColumns { get; set; }
            public decimal markup { get; set; }
            public decimal days { get; set; }
            public decimal margin { get; set; }
            public decimal interestrate { get; set; }
            public long warehouselocationid { get; set; }
        }


        public class Condtion
        {
            public bool HasCondition
            {
                get
                {
                    if (selectedall) return false;

                    switch (type)
                    {
                        case CondtionType.Date:
                            return dateList != null && dateList.Count > 0;
                        case CondtionType.Decimal:
                            return decimalList != null && decimalList.Count > 0;
                        case CondtionType.Long:
                            return longList != null && longList.Count > 0;
                        case CondtionType.Range:
                            return range != null && ((stringList != null && stringList.Count > 0)
                                || (decimalList != null && decimalList.Count > 0)
                                || (dateList != null && dateList.Count > 0)
                                || (longList != null && longList.Count > 0));
                        case CondtionType.String:
                            return stringList != null && stringList.Count > 0;
                        case CondtionType.DayRange:
                            return stringList != null && stringList.Count > 0;
                    }
                    return false;
                }
            }
            public CondtionType type { get; set; }
            public Table table { get; set; }
            public Options option { get; set; }
            public string customoption { get; set; }
            public List<long> longList { get; set; }
            public List<DateTime> dateList { get; set; }
            public List<Decimal> decimalList { get; set; }
            public List<string> stringList { get; set; }
            public List<string> nameList { get; set; }
            public bool selectedall { get; set; }
            public bool ShowColumn { get; set; }
            public string typeName { get; set; }
            public Range range { get; set; }
            public class Range
            {
                public decimal start { get; set; }
                public decimal end { get; set; }
                public decimal incrementor { get; set; }
                public string direct { get; set; }
                internal List<Range> getList()
                {
                    if (!string.IsNullOrEmpty(direct))
                        return getDirectList();

                    var list = new List<Range>();
                    var current = start;
                    list.Add(new Range()
                    {
                        end = this.start,
                        start = 0
                    });

                    while (current < end)
                    {
                        Range rng = new Range()
                        {
                            start = current
                        };

                        current += incrementor;
                        if (current > end)
                        {
                            current = end;
                        }
                        rng.end = current;
                        list.Add(rng);
                    }

                    list.Add(new Range()
                    {
                        start = this.end,
                        end = 0
                    });

                    return list;
                }

                internal List<Range> getDirectList()
                {
                    var list = new List<Range>();
                    string[] decparts = direct.Split(',');
                    decimal current = 0;
                    decimal.TryParse(decparts[0], out current);
                    if (current > 0)
                    {
                        list.Add(new Range()
                        {
                            end = current,
                            start = 0
                        });
                    }
                    this.start = current;
                    for (var index = 1; index < decparts.Length; index++)
                    {
                        decimal tdec = 0;
                        if (decimal.TryParse(decparts[index], out tdec))
                        {
                            list.Add(new Range()
                            {
                                end = tdec,
                                start = current
                            });
                            current = tdec;
                        }
                    }
                    if (current > 0)
                    {
                        this.end = current;
                        list.Add(new Range()
                        {
                            start = current,
                            end = 0
                        });
                    }
                    return list;
                }

                internal List<Range> getDayList()
                {
                    var list = new List<Range>();
                    list.Add(new Range()
                    {
                        start = 1,
                        end = 7
                    });
                    list.Add(new Range()
                    {
                        start = 8,
                        end = 14
                    });
                    list.Add(new Range()
                    {
                        start = 15,
                        end = 21
                    });
                    list.Add(new Range()
                    {
                        start = 22,
                        end = 31
                    });
                    return list;
                }
            }

            public enum CondtionType
            {
                Range,
                String,
                Long,
                Date,
                Decimal,
                DayRange
            }
        }

        public enum Table
        {
            Item,
            LrEntry,
            LrInvoice,
            LrInvoiceItems,
            Stock,
            Bill,
            BillItem,
            Salablegoods,
            Product,
            Supplier,
            Agent,
            Customer,
            Stockmarker,
            LrInvoicereturn
        }

        public class OptionConfig
        {
            public OptionConfig(Options option, Table table, Condtion.CondtionType type)
            {
                this.option = option;
                this.table = table;
                this.conditiontype = type;
            }
            public Options option { get; set; }
            public Table table { get; set; }
            public Condtion.CondtionType conditiontype { get; set; }

        }

        public SortedDictionary<Options, OptionConfig> GetOptionConfiguration()
        {
            SortedDictionary<Options, OptionConfig> retList = new SortedDictionary<Options, OptionConfig>();
            retList.Add(Options.Barcode, new OptionConfig(Options.Barcode, Table.Stock, Condtion.CondtionType.String));
            retList.Add(Options.BillDate, new OptionConfig(Options.BillDate, Table.Bill, Condtion.CondtionType.Date));
            retList.Add(Options.BillNo, new OptionConfig(Options.BillNo, Table.Bill, Condtion.CondtionType.String));
            retList.Add(Options.Brand, new OptionConfig(Options.Brand, Table.Item, Condtion.CondtionType.Long));
            retList.Add(Options.Colour, new OptionConfig(Options.Colour, Table.Item, Condtion.CondtionType.Long));
            retList.Add(Options.Company, new OptionConfig(Options.Company, Table.Item, Condtion.CondtionType.Long));
            retList.Add(Options.ProductGroup, new OptionConfig(Options.ProductGroup, Table.Item, Condtion.CondtionType.Long));
            retList.Add(Options.BarcodeGroup, new OptionConfig(Options.BarcodeGroup, Table.Item, Condtion.CondtionType.Long));
            retList.Add(Options.GLNNO, new OptionConfig(Options.GLNNO, Table.Item, Condtion.CondtionType.String));

            retList.Add(Options.Fit, new OptionConfig(Options.Fit, Table.Item, Condtion.CondtionType.Long));
            retList.Add(Options.Sleeve, new OptionConfig(Options.Sleeve, Table.Item, Condtion.CondtionType.Long));
            retList.Add(Options.PriceTag, new OptionConfig(Options.PriceTag, Table.Stock, Condtion.CondtionType.Long));

            retList.Add(Options.Design, new OptionConfig(Options.Design, Table.Stock, Condtion.CondtionType.String));
            retList.Add(Options.DiscountRate, new OptionConfig(Options.DiscountRate, Table.Stock, Condtion.CondtionType.Decimal));
            retList.Add(Options.DiscountRate_Range, new OptionConfig(Options.DiscountRate_Range, Table.Stock, Condtion.CondtionType.Range));
            retList.Add(Options.InvDate, new OptionConfig(Options.InvDate, Table.LrInvoice, Condtion.CondtionType.Date));
            retList.Add(Options.PurchaseDate, new OptionConfig(Options.PurchaseDate, Table.LrInvoice, Condtion.CondtionType.Date));
            retList.Add(Options.BarcodedOn, new OptionConfig(Options.BarcodedOn, Table.LrInvoiceItems, Condtion.CondtionType.Date));
            retList.Add(Options.LREntryNo, new OptionConfig(Options.InvoiceNo, Table.LrEntry, Condtion.CondtionType.String));
            retList.Add(Options.DebitNoteNo, new OptionConfig(Options.DebitNoteNo, Table.LrInvoicereturn, Condtion.CondtionType.String));
            retList.Add(Options.DebitNoteDate, new OptionConfig(Options.DebitNoteDate, Table.LrInvoicereturn, Condtion.CondtionType.String));
            retList.Add(Options.EntryLocation, new OptionConfig(Options.EntryLocation, Table.LrEntry, Condtion.CondtionType.Long));
            retList.Add(Options.InvoiceNo, new OptionConfig(Options.InvoiceNo, Table.LrInvoice, Condtion.CondtionType.String));
            retList.Add(Options.SaleLocation, new OptionConfig(Options.SaleLocation, Table.Bill, Condtion.CondtionType.Long));
            retList.Add(Options.StockLocation, new OptionConfig(Options.StockLocation, Table.Salablegoods, Condtion.CondtionType.Long));
            retList.Add(Options.Material, new OptionConfig(Options.Material, Table.Item, Condtion.CondtionType.Long));
            retList.Add(Options.Pattern, new OptionConfig(Options.Pattern, Table.Item, Condtion.CondtionType.Long));
            //retList.Add(Options.Fit, new OptionConfig(Options.Fit, Table.Item, Condtion.CondtionType.Long));
            retList.Add(Options.ProductCode, new OptionConfig(Options.ProductCode, Table.Product, Condtion.CondtionType.String));
            retList.Add(Options.Product, new OptionConfig(Options.Product, Table.Stock, Condtion.CondtionType.Long));
            retList.Add(Options.PurchaseMonth, new OptionConfig(Options.PurchaseMonth, Table.LrInvoice, Condtion.CondtionType.String));
            retList.Add(Options.PurchaseRate, new OptionConfig(Options.PurchaseRate, Table.LrInvoiceItems, Condtion.CondtionType.Decimal));
            retList.Add(Options.DealerPrice, new OptionConfig(Options.DealerPrice, Table.Stock, Condtion.CondtionType.Decimal));
            retList.Add(Options.DealerPrice_Range, new OptionConfig(Options.DealerPrice_Range, Table.Stock, Condtion.CondtionType.Range));
            retList.Add(Options.PurchaseTax, new OptionConfig(Options.PurchaseTax, Table.LrInvoiceItems, Condtion.CondtionType.Decimal));
            retList.Add(Options.PurchaseRate_Range, new OptionConfig(Options.PurchaseRate_Range, Table.LrInvoiceItems, Condtion.CondtionType.Range));
            retList.Add(Options.PurchaseYear, new OptionConfig(Options.PurchaseYear, Table.LrInvoice, Condtion.CondtionType.String));
            retList.Add(Options.SaleMonth, new OptionConfig(Options.SaleMonth, Table.Bill, Condtion.CondtionType.String));
            retList.Add(Options.SoldRate, new OptionConfig(Options.SaleRate, Table.BillItem, Condtion.CondtionType.Decimal));
            retList.Add(Options.SoldRate_Range, new OptionConfig(Options.SaleRate, Table.BillItem, Condtion.CondtionType.Range));
            retList.Add(Options.SaleRate, new OptionConfig(Options.SaleRate, Table.Salablegoods, Condtion.CondtionType.Decimal));
            retList.Add(Options.SaleRate_Range, new OptionConfig(Options.SaleRate_Range, Table.BillItem, Condtion.CondtionType.Range));
            retList.Add(Options.ShelfAge_Range, new OptionConfig(Options.ShelfAge_Range, Table.BillItem, Condtion.CondtionType.Range));
            retList.Add(Options.StockAge_Range, new OptionConfig(Options.PurchaseRate, Table.LrInvoiceItems, Condtion.CondtionType.Range));
            retList.Add(Options.RetailMargin_Range, new OptionConfig(Options.RetailMargin_Range, Table.BillItem, Condtion.CondtionType.Range));
            retList.Add(Options.SaleMargin_Range, new OptionConfig(Options.SaleMargin_Range, Table.BillItem, Condtion.CondtionType.Range));
            retList.Add(Options.SalesmanName, new OptionConfig(Options.SalesmanName, Table.BillItem, Condtion.CondtionType.Long));
            retList.Add(Options.Incharge, new OptionConfig(Options.Incharge, Table.Product, Condtion.CondtionType.Long));
            retList.Add(Options.SalesmanCode, new OptionConfig(Options.SalesmanCode, Table.BillItem, Condtion.CondtionType.String));
            retList.Add(Options.SaleYear, new OptionConfig(Options.SaleYear, Table.Bill, Condtion.CondtionType.String));
            retList.Add(Options.Section, new OptionConfig(Options.Section, Table.Product, Condtion.CondtionType.Long));
            retList.Add(Options.Item, new OptionConfig(Options.Item, Table.Product, Condtion.CondtionType.Long));
            retList.Add(Options.Size, new OptionConfig(Options.Size, Table.Item, Condtion.CondtionType.Long));
            retList.Add(Options.Style, new OptionConfig(Options.Style, Table.Item, Condtion.CondtionType.Long));
            retList.Add(Options.Supplier_City, new OptionConfig(Options.Supplier_City, Table.Supplier, Condtion.CondtionType.Long));
            retList.Add(Options.Supplier, new OptionConfig(Options.Supplier, Table.LrInvoice, Condtion.CondtionType.Long));
            //retList.Add(Options.Source_Supplier, new OptionConfig(Options.Source_Supplier, Table.LrInvoice, Condtion.CondtionType.String));
            retList.Add(Options.Type, new OptionConfig(Options.Type, Table.Item, Condtion.CondtionType.Long));
            retList.Add(Options.HSNCode, new OptionConfig(Options.HSNCode, Table.LrInvoice, Condtion.CondtionType.String));
            retList.Add(Options.Tax, new OptionConfig(Options.Tax, Table.Item, Condtion.CondtionType.Long));
            retList.Add(Options.SaleMonthDay, new OptionConfig(Options.SaleMonthDay, Table.Item, Condtion.CondtionType.Range));
            retList.Add(Options.SaleWeekDay, new OptionConfig(Options.SaleWeekDay, Table.Item, Condtion.CondtionType.Long));
            retList.Add(Options.Agent, new OptionConfig(Options.Agent, Table.Agent, Condtion.CondtionType.Long));
            retList.Add(Options.PurchaseQty, new OptionConfig(Options.PurchaseQty, Table.LrInvoice, Condtion.CondtionType.String));

            retList.Add(Options.Customer_Name, new OptionConfig(Options.Customer_Name, Table.Customer, Condtion.CondtionType.Long));
            retList.Add(Options.Customer_AgeSet, new OptionConfig(Options.Customer_AgeSet, Table.Customer, Condtion.CondtionType.Long));
            retList.Add(Options.Customer_Gender, new OptionConfig(Options.Customer_Gender, Table.Customer, Condtion.CondtionType.String));
            retList.Add(Options.Customer_DOB, new OptionConfig(Options.Customer_DOB, Table.Customer, Condtion.CondtionType.Date));
            retList.Add(Options.Customer_Anniversary, new OptionConfig(Options.Customer_Anniversary, Table.Customer, Condtion.CondtionType.Date));
            retList.Add(Options.Customer_LoyeltyNo, new OptionConfig(Options.Customer_LoyeltyNo, Table.Customer, Condtion.CondtionType.String));
            retList.Add(Options.Customer_Mobile, new OptionConfig(Options.Customer_Mobile, Table.Customer, Condtion.CondtionType.String));
            retList.Add(Options.Customer_Religion, new OptionConfig(Options.Customer_Religion, Table.Customer, Condtion.CondtionType.String));
            retList.Add(Options.Customer_Category, new OptionConfig(Options.Customer_Category, Table.Customer, Condtion.CondtionType.String));
            retList.Add(Options.Customer_Agent, new OptionConfig(Options.Customer_Agent, Table.Customer, Condtion.CondtionType.String));
            retList.Add(Options.Customer_City, new OptionConfig(Options.Customer_City, Table.Customer, Condtion.CondtionType.String));
            retList.Add(Options.Source_Supplier, new OptionConfig(Options.Source_Supplier, Table.Stock, Condtion.CondtionType.Long));
            retList.Add(Options.Customer_Age, new OptionConfig(Options.Customer_Age, Table.Customer, Condtion.CondtionType.Long));
            retList.Add(Options.StockMarker, new OptionConfig(Options.StockMarker, Table.Stockmarker, Condtion.CondtionType.Long));
            return retList;
        }
        public enum Options
        {
            Product,
            Brand,
            Size,
            Pattern,
            Material,
            Style,
            Colour,
            Type,
            Section,
            SaleLocation,
            StockLocation,
            Design,
            SaleRate,
            PurchaseRate,
            DiscountRate,
            SoldRate,
            SaleRate_Range,
            StockAge_Range,
            PurchaseRate_Range,
            DiscountRate_Range,
            SoldRate_Range,
            LREntryNo,
            InvoiceNo,
            InvDate,
            SaleYear,
            PurchaseYear,
            PurchaseMonth,
            SaleMonth,
            BillNo,
            BillDate,
            Supplier,
            SalesmanName,
            Supplier_City,
            Barcode,
            HSNCode,
            Tax,
            SaleWeekDay,
            SaleMonthDay = 37,
            SaleMargin_Range = 38,
            RetailMargin_Range = 39,
            Agent = 40,
            PurchaseQty = 41,
            SaleQty = 42,
            PurchaseValue = 43,
            SaleValue = 44,
            AddDiscountRate = 45,
            ZeroQty = 46,
            NegativeQty = 47,
            Counter = 48,
            Company = 49,
            ProductGroup = 50,
            ReturnReason = 51,
            SaleReason = 52,
            Area = 53,
            Season = 54,
            Customer_Name = 55,
            Customer_AgeSet = 56,
            Customer_Gender = 57,
            Customer_DOB = 58,
            Customer_Anniversary = 59,
            Customer_LoyeltyNo = 60,
            Customer_Mobile = 61,
            Product_Owner = 62,
            StockQty = 63,
            StockValue = 64,
            AvgStockDays = 65,
            AvgSaleDays = 66,
            InwardQty = 67,
            OutwardQty = 68,
            ReturnQty = 69,
            ReturnValue = 70,
            Margin = 71,
            EntryLocation = 72,
            LRNo = 73,
            LRDate = 74,
            LREntryDate = 75,
            DebitNoteNo = 76,
            DebitNoteDate = 77,
            SalesmanCode = 78,
            SchemeDiscount = 79,
            SaleTax = 80,
            PurchaseDiscount = 81,
            PurchaseTax = 82,
            Promotion = 83,
            PurchaseDate = 84,
            DebitNoteQty = 85,
            DebitNoteValue = 86,
            JournalQty = 87,
            JournalValue = 88,
            B2BSales = 89,
            CREDITSALES = 90,
            SALESRETURN = 91,
            APPROVEDSALES = 92,
            Customer_Religion = 93,
            Source_Supplier = 94,
            Customer_City = 95,
            IStockValue = 96,
            StockSaleValue = 97,
            ProductCode = 98,
            Fit = 99,
            Sleeve = 100,
            Customer_Age = 101,
            TaxPercentage = 102,
            GLNNO = 103,
            Floor = 104,
            Item = 105,
            StockMarker = 106,
            DealerPrice = 107,
            DealerPrice_Range = 108,
            WithZeroandNagativeStock = 109,
            JobworkSupplier = 110,
            SellingMode = 111,
            Customer_Area = 112,
            BillType = 113,
            PriceTag = 114,
            Customer_Category = 115,
            Customer_Agent = 116,
            BarcodeGroup = 117,
            SaleAreaGroup1 = 118,
            SaleAreaGroup2 = 119,
            TodayYesterDay = 120,
            WeekLastWeek = 121,
            MonthLastMonth = 122,
            YearLastYear = 123,
            FinYearLastFinYear = 124,
            ShelfAge_Range = 125,
            Incharge = 126,
            Custom_ProductGroups = 200,
            Custom_Fields = 201,
            BarcodedOn = 202
        }

        public class OptionQueryBuilder
        {
            AnalyserDataService Owner;
            public OptionQueryBuilder(AnalyserDataService parent)
            {
                this.Owner = parent;
            }

            string branchStockMarkerQuery = @"select {0},sum(salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty)) stockqty,
                                        sum((salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty)) * (salablegoods.price-salablegoods.discount)::numeric) as stocksalevalue,
                                        sum((salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty)) * stock.ibuyingprice::numeric) as stockcost
from stockmarker
inner join salablegoods on salablegoods.id = stockmarker.salablegoodsid
inner join stock on stock.id = salablegoods.stockid and stock.isactive = true
left outer join products on products.id = stock.productid
left outer join items on items.id = stock.itemid 
left outer join lrinvoiceitems on lrinvoiceitems.id = stock.invoiceitemid and lrinvoiceitems.isactive = true
left outer join lrinvoice on lrinvoice.id = lrinvoiceitems.lrinvoiceid and lrinvoice.isactive = true
left outer join lrentry  on lrentry.id = lrid and lrentry.isactive = true
left outer join supplier on supplier.id = stock.supplierid 
left outer join suppliercompany on suppliercompany.supplierid = supplier.id
{3}
where stockmarker.isactive=true and salablegoods.isactive = true {1}
group by {2}";


            string StockMarkerStockQuery = @"select {0},sum(stockmarker.qty) as smqty,
                                        sum((stockmarker.qty) * (salablegoods.price-salablegoods.discount)::numeric) as smsalevalue,
                                        sum((stockmarker.qty) * stock.ibuyingprice::numeric) as smcost
                                        from stockmarker
inner join salablegoods on salablegoods.id = stockmarker.salablegoodsid
inner join stock on stock.id = salablegoods.stockid and stock.isactive = true
left outer join products on products.id = stock.productid
left outer join items on items.id = stock.itemid 
left outer join lrinvoiceitems on lrinvoiceitems.id = stock.invoiceitemid and lrinvoiceitems.isactive = true
left outer join lrinvoice on lrinvoice.id = lrinvoiceitems.lrinvoiceid and lrinvoice.isactive = true
left outer join lrentry  on lrentry.id = lrid and lrentry.isactive = true
left outer join supplier on supplier.id = stock.supplierid 
left outer join suppliercompany on suppliercompany.supplierid = supplier.id
{3}
where stockmarker.isactive=true and salablegoods.isactive = true {1}
group by {2}";

            string branchStockQuery = @"select {0},sum(salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty)) stock,
                                        sum(salablegoods.dqty+salablegoods.jqty) adjqty,
										sum((salablegoods.dqty+salablegoods.jqty) * stock.ibuyingprice::numeric) adjval,
                                        sum((salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty)) * stock.buyingprice::numeric) as cost,
                                        sum((salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty)) * (stock.eprice)::numeric) as scost,
                                        sum((salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty)) * stock.ibuyingprice::numeric) as buyingcost,
                                        sum((salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty)) * (salablegoods.price-salablegoods.discount)::numeric) as sellingprice,
                                        sum(salablegoods.iqty) as istock,
                                        sum(salablegoods.iqty * stock.buyingprice::numeric) as icost,
                                        sum(salablegoods.iqty * stock.eprice::numeric) as avalue
                                        from salablegoods 
inner join stock on stock.id = salablegoods.stockid and stock.isactive = true
left outer join products on products.id = stock.productid
left outer join items on items.id = stock.itemid 
left outer join lrinvoiceitems on lrinvoiceitems.id = stock.invoiceitemid and lrinvoiceitems.isactive = true
left outer join lrinvoice on lrinvoice.id = lrinvoiceitems.lrinvoiceid and lrinvoice.isactive = true
left outer join lrentry  on lrentry.id = lrid and lrentry.isactive = true
left outer join supplier on supplier.id = stock.supplierid 
left outer join suppliercompany on suppliercompany.supplierid = supplier.id
{3}
where salablegoods.isactive = true and lrinvoice.entrydate>=@fromdate and lrinvoice.entrydate<@todate and salablegoods.hasstock = true and salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty)>0 {1}
group by {2}";


            string branchStockAsOnQuery = @"select {0},sum(salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty)) stock,
                                        sum( (salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty)) * stock.buyingprice::numeric) as cost,
                                        sum( (salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty)) * (salablegoods.price-salablegoods.discount)::numeric) as scost from salablegoods 
inner join stock on stock.id = salablegoods.stockid and stock.isactive = true
left outer join products on products.id = stock.productid
left outer join items on items.id = stock.itemid 
left outer join lrinvoiceitems on lrinvoiceitems.id = stock.invoiceitemid and lrinvoiceitems.isactive = true
left outer join lrinvoice on lrinvoice.id = lrinvoiceitems.lrinvoiceid and lrinvoice.isactive = true
left outer join lrentry  on lrentry.id = lrid and lrentry.isactive = true
left outer join supplier on supplier.id = stock.supplierid 
left outer join suppliercompany on suppliercompany.supplierid = supplier.id
{3}
where salablegoods.isactive = true and salablegoods.hasstock = true and salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty)>0 and stock.stockdate<@todate {1}
group by {2}";

            string priorityQuery = @"with purchaseSQL as 
(
select {0} item,sum(lrinvoiceitems.buyingqty) bqty from lrinvoice
inner join lrinvoiceitems on lrinvoiceitems.lrinvoiceid = lrinvoice.id and lrinvoiceitems.isactive=true and lrinvoiceitems.lrinvoiceid>0
inner join stock on stock.invoiceitemid = lrinvoiceitems.id
left outer join lrentry  on lrentry.id = lrid and lrentry.isactive = true
left outer join supplier on supplier.id = stock.supplierid 
left outer join items on items.id = stock.itemid
where lrinvoiceitems.productid=@productid and lrinvoice.entrydate>=@fromdate and lrinvoice.entrydate<@todate
group by {0}
), salesSQL as 
(
select {0} item,sum(billitems.qty) sqty from bill
inner join billitems on billitems.billid = bill.id and billitems.isactive=true
inner join stock on stock.id = billitems.stockid
inner join lrinvoiceitems on lrinvoiceitems.id = stock.invoiceitemid and lrinvoiceitems.isactive=true and lrinvoiceitems.lrinvoiceid>0
left outer join lrinvoice on lrinvoice.id = lrinvoiceitems.lrinvoiceid and lrinvoice.isactive = true
left outer join lrentry  on lrentry.id = lrid and lrentry.isactive = true
left outer join supplier on supplier.id = stock.supplierid 
left outer join items on items.id = stock.itemid
where stock.productid=@productid and bill.billdate>=@fromdate and bill.billdate<@todate
group by {0}
), unionSQL as 
(
select salesSQL.item,sqty, 0 as bqty from salesSQL
union all
select purchaseSQL.item,0 as sqty, bqty from purchaseSQL
)
select item,sum(sqty) sqty,sum(bqty) bqty from unionSQL
group by item";

            string whStockQuery = @"select {0},sum(stock.qty - (stock.transfered+stock.returnqty+stock.damagedqty+stock.journalqty)) stock,
                                               sum(stock.damagedqty+stock.journalqty) adjqty,
										       sum((stock.damagedqty+stock.journalqty) * stock.ibuyingprice::numeric) adjval,
                                               sum((stock.qty - (stock.transfered+stock.returnqty+stock.damagedqty+stock.journalqty)) * stock.buyingprice::numeric) as cost,
                                               sum((stock.qty - (stock.transfered+stock.returnqty+stock.damagedqty+stock.journalqty)) * (stock.eprice)::numeric) as scost,
                                               sum((stock.qty - (stock.transfered+stock.returnqty+stock.damagedqty+stock.journalqty)) * stock.ibuyingprice::numeric) as buyingcost,
                                               sum((stock.qty - (stock.transfered+stock.returnqty+stock.damagedqty+stock.journalqty)) * (stock.price-stock.discount)::numeric) as sellingprice,
                                               sum(stock.qty - (stock.transfered+stock.returnqty+stock.damagedqty+stock.journalqty)) istock,
                                               sum((stock.qty - (stock.transfered+stock.returnqty+stock.damagedqty+stock.journalqty)) * stock.buyingprice::numeric) as icost,
                                               sum((stock.qty - (stock.transfered+stock.returnqty+stock.damagedqty+stock.journalqty)) * stock.eprice::numeric) as avalue
                                    from stock
                                    left outer join products on products.id = stock.productid
                                    left outer join items on items.id = stock.itemid 
                                    left outer join lrinvoiceitems on lrinvoiceitems.id = stock.invoiceitemid and lrinvoiceitems.isactive = true
                                    left outer join lrinvoice on lrinvoice.id = lrinvoiceitems.lrinvoiceid and lrinvoice.isactive = true
                                    left outer join lrentry  on lrentry.id = lrid and lrentry.isactive = true
                                    left outer join supplier on supplier.id = stock.supplierid
                                    left outer join suppliercompany on suppliercompany.supplierid = supplier.id
                                    {3}
                                    where stock.isactive = true and lrinvoice.entrydate>=@fromdate and lrinvoice.entrydate<@todate and stock.hasstock = true and stock.qty - (stock.transfered+stock.returnqty+stock.damagedqty+stock.journalqty)>0 {1}
                                    group by {2}";

            string inwardQuery = @"select {0},sum( (COALESCE(salablegoods.iqty,00)-COALESCE(salablegoods.oqty,00))) cstock,
                                    sum( ((COALESCE(salablegoods.iqty,00)-COALESCE(salablegoods.oqty,00))) * stock.buyingprice::numeric ) ccost from salablegoods
inner join stock on stock.id = salablegoods.stockid and stock.isactive = true
left outer join products on products.id = stock.productid
left outer join items on items.id = stock.itemid 
left outer join lrinvoiceitems on lrinvoiceitems.id = stock.invoiceitemid and lrinvoiceitems.isactive = true
left outer join lrinvoice on lrinvoice.id = lrinvoiceitems.lrinvoiceid and lrinvoice.isactive = true
left outer join lrentry  on lrentry.id = lrid and lrentry.isactive = true
left outer join supplier on supplier.id = stock.supplierid
left outer join suppliercompany on suppliercompany.supplierid = supplier.id
{3}
where salablegoods.isactive = true and salablegoods.locationid=@locationid and stock.stockdate>=@fromdate and stock.stockdate<@todate {1}
group by {2}";

            //            string stockRemoveQuery = @"select {0},sum( (COALESCE(salablegoods.iqty,00)-COALESCE(salablegoods.oqty,00))) cstock,
            //                                    sum( ((COALESCE(salablegoods.iqty,00)-COALESCE(salablegoods.oqty,00))) * stock.buyingprice::numeric ) ccost from salablegoods
            //inner join stock on stock.id = salablegoods.stockid and stock.isactive = true
            //left outer join products on products.id = stock.productid
            //left outer join items on items.id = stock.itemid 
            //left outer join lrinvoiceitems on lrinvoiceitems.id = stock.invoiceitemid and lrinvoiceitems.isactive = true
            //left outer join lrinvoice on lrinvoice.id = lrinvoiceitems.lrinvoiceid and lrinvoice.isactive = true
            //left outer join lrentry  on lrentry.id = lrid and lrentry.isactive = true
            //left outer join supplier on supplier.id = lrentry.supplierid
            //left outer join suppliercompany on suppliercompany.supplierid = supplier.id
            //where salablegoods.isactive = true and salablegoods.locationid=@locationid and salablegoods.inwardon>=@todate {1}
            //group by {2}";

            string salesRemoveQuery = @"select {0},sum(salablegoods.iqty) stock,sum(billitems.qty) cstock,sum(billitems.qty * stock.buyingprice::numeric) as ccost from bill 
inner join billitems on billitems.billid = bill.id and billitems.isactive=true
left outer join salablegoods on salablegoods.id = salablegoodsid and salablegoods.isactive = true
left outer join stock on stock.id = salablegoods.stockid and stock.isactive = true
left outer join products on products.id = stock.productid
left outer join items on items.id = stock.itemid 
left outer join lrinvoiceitems on lrinvoiceitems.id = stock.invoiceitemid and lrinvoiceitems.isactive = true
left outer join lrinvoice on lrinvoice.id = lrinvoiceitems.lrinvoiceid and lrinvoice.isactive = true
left outer join lrentry  on lrentry.id = lrid and lrentry.isactive = true
left outer join supplier on supplier.id = stock.supplierid 
left outer join suppliercompany on suppliercompany.supplierid = supplier.id
{3}
where bill.billdate>=@todate and stock.stockdate<@todate {1}
group by {2}";

            /*                                    sum( (COALESCE(salablegoods.iqty,00)-COALESCE(salablegoods.oqty,00))+stock.qty - (stock.transfered+stock.returnqty+stock.damagedqty) ) cstock,
                                    sum( ((COALESCE(salablegoods.iqty,00)-COALESCE(salablegoods.oqty,00))+(stock.qty - (stock.transfered+stock.returnqty+stock.damagedqty))) * stock.buyingprice::numeric ) ccost from stock
                                    left outer join salablegoods on salablegoods.stockid = stock.id and salablegoods.isactive = true
 */

            string purchaseQuery = @"select {0},sum(stock.qty) stock,
sum((stock.qty) * stock.buyingprice) as cost,
sum((stock.qty) * stock.ibuyingprice) as cvalue,
sum((stock.qty) * stock.eprice) as sellingprice,
sum(stock.returnqty) rqty,	
sum(stock.returnqty * stock.buyingprice) rcost,		
sum(stock.returnqty * stock.ibuyingprice) rvalue,		
sum(salablestock.available+(stock.qty-stock.returnqty-stock.transfered)) cqty,
sum((salablestock.available+(stock.qty-stock.returnqty-stock.transfered)) * stock.buyingprice) as iqty,
sum((salablestock.available+(stock.qty-stock.returnqty-stock.transfered)) * stock.ibuyingprice) as ivalue,
sum((salablestock.available+(stock.qty-stock.returnqty-stock.transfered)) * (stock.price-stock.discount)) as svalue
from stock
left outer join (select stockid,sum(salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty)) available from salablegoods
where hasstock=true and isactive=true
group by stockid) as salablestock on salablestock.stockid = stock.id
left outer join products on products.id = stock.productid
left outer join items on items.id = stock.itemid 
left outer join lrinvoiceitems on lrinvoiceitems.id = stock.invoiceitemid and lrinvoiceitems.isactive = true
left outer join lrinvoice on lrinvoice.id = lrinvoiceitems.lrinvoiceid and lrinvoice.isactive = true
left outer join lrentry  on lrentry.id = lrid and lrentry.isactive = true
left outer join supplier on supplier.id = stock.supplierid
left outer join suppliercompany on suppliercompany.supplierid = supplier.id
{3}
where stock.isactive = true and lrinvoice.entrydate>=@fromdate and lrinvoice.entrydate<@todate {1}
group by {2}";

            string purchaseReturnQuery = @"select {0},
sum(lrinvoicereturnitems.qty) stock,
sum(lrinvoicereturnitems.qty * lrinvoicereturnitems.rate) as cost,
sum((lrinvoicereturnitems.qty * lrinvoicereturnitems.rate) + lrinvoicereturnitems.tax) as cvalue,
sum(salablestock.available+(stock.qty-stock.returnqty-stock.transfered)) cqty,
sum((salablestock.available+(stock.qty-stock.returnqty-stock.transfered)) * stock.buyingprice) as iqty,
sum((salablestock.available+(stock.qty-stock.returnqty-stock.transfered)) * stock.ibuyingprice) as ivalue,
sum((salablestock.available+(stock.qty-stock.returnqty-stock.transfered)) * (stock.price-stock.discount)) as svalue
from lrinvoicereturn
inner join lrinvoicereturnitems on lrinvoicereturnid = lrinvoicereturn.id
left outer join stock on stock.id = lrinvoicereturnitems.stockid
left outer join (select stockid,sum(salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty)) available from salablegoods
where hasstock=true and isactive=true
group by stockid) as salablestock on salablestock.stockid = stock.id
left outer join products on products.id = lrinvoicereturnitems.productid
left outer join items on items.id = lrinvoicereturnitems.itemid 
left outer join lrinvoiceitems on lrinvoiceitems.id = lrinvoicereturnitems.lrinvoiceitemsid and lrinvoiceitems.isactive = true
left outer join lrinvoice on lrinvoice.id = lrinvoiceitems.lrinvoiceid and lrinvoice.isactive = true
left outer join lrentry  on lrentry.id = lrid and lrentry.isactive = true
left outer join supplier on supplier.id = stock.supplierid
left outer join suppliercompany on suppliercompany.supplierid = supplier.id
{3}
where lrinvoicereturn.isactive = true and lrinvoicereturnitems.isactive = true and lrinvoicereturn.returntype = 0 and lrinvoicereturn.returndate>=@fromdate and lrinvoicereturn.returndate<@todate {1}
group by {2}";

            string customerQuery = @"select {0},sum(billitems.qty) stock,
sum(billitems.qty * stock.ibuyingprice::numeric) as cstock, 
sum((billitems.qty * billitems.additionaldiscount::numeric) + billitems.schemediscount) scost,
sum(billitems.receivable) as cost,
sum(billitems.qty * stock.buyingprice::numeric) as ivalue, 
sum(billitems.receivable-billitems.tax) as iqty from bill 
inner join billitems on billitems.billid = bill.id and billitems.isactive=true
left outer join salablegoods on salablegoods.id = salablegoodsid 
left outer join stock on stock.id = salablegoods.stockid 
left outer join customer on customer.id = bill.customerid
left outer join (select id,cast(additionalinfo->>'parentid' as bigint) saleareagroup1,
cast(additionalinfo->>'floorid' as bigint) saleareagroup2 from referencelist where type ='AREA') customerarea on customerarea.id = customer.areaid
left outer join products on products.id = stock.productid
left outer join items on items.id = stock.itemid 
left outer join referencelist sizelist on sizelist.id = items.sizeid
left outer join referencelist agelist on agelist.id  = products.ageset and agelist.type = 'AGESET' 
left outer join lrinvoiceitems on lrinvoiceitems.id = stock.invoiceitemid and lrinvoiceitems.isactive = true
left outer join lrinvoice on lrinvoice.id = lrinvoiceitems.lrinvoiceid and lrinvoice.isactive = true
left outer join lrentry  on lrentry.id = lrinvoice.lrid and lrentry.isactive = true
left outer join supplier on supplier.id = stock.supplierid 
left outer join suppliercompany on suppliercompany.supplierid = supplier.id
{3}
where bill.isactive=true And bill.billtype!=9 
and bill.billdate>=@fromdate and bill.billdate<@todate {1}
group by {2}";

            string salesMarkerQuery = @"select {0},sum((case when bill.billdate::date>=stockmarker.addedon::date then billitems.qty else 0 end)) saleqty,
sum((case when bill.billdate::date>=stockmarker.addedon::date then billitems.qty else 0 end) * stock.ibuyingprice::numeric) as salecost, 
sum((case when bill.billdate::date>=stockmarker.addedon::date then billitems.receivable else 0 end)) as salevalue from bill 
inner join billitems on billitems.billid = bill.id and billitems.isactive=true
inner join stockmarker on stockmarker.salablegoodsid = billitems.salablegoodsid
left outer join salablegoods on salablegoods.id = billitems.salablegoodsid 
left outer join stock on stock.id = salablegoods.stockid
left outer join products on products.id = stock.productid
left outer join items on items.id = stock.itemid 
left outer join lrinvoiceitems on lrinvoiceitems.id = stock.invoiceitemid and lrinvoiceitems.isactive = true
left outer join lrinvoice on lrinvoice.id = lrinvoiceitems.lrinvoiceid and lrinvoice.isactive = true
left outer join lrentry  on lrentry.id = lrid and lrentry.isactive = true
left outer join supplier on supplier.id = stock.supplierid 
left outer join suppliercompany on suppliercompany.supplierid = supplier.id
{3}
where stockmarker.isactive=true and bill.isactive = true and bill.billtype != 9 and bill.billdate>=@fromdate and bill.billdate<@todate {1}
group by {2}";

            string salesQuery = @"select {0},sum(billitems.qty) stock,
sum(billitems.qty * stock.ibuyingprice::numeric) as cstock, 
sum((billitems.qty * billitems.additionaldiscount::numeric) + billitems.schemediscount) scost,
sum((billitems.receivable - billitems.tax) - (billitems.qty * stock.buyingprice::numeric)) margin,
sum(billitems.receivable) as cost,
sum(billitems.qty * stock.buyingprice::numeric) as ivalue, 
sum(billitems.receivable-billitems.tax) as iqty from bill 
inner join billitems on billitems.billid = bill.id and billitems.isactive=true
left outer join salablegoods on salablegoods.id = salablegoodsid 
left outer join stock on stock.id = salablegoods.stockid
left outer join products on products.id = stock.productid
left outer join items on items.id = stock.itemid 
left outer join lrinvoiceitems on lrinvoiceitems.id = stock.invoiceitemid and lrinvoiceitems.isactive = true
left outer join lrinvoice on lrinvoice.id = lrinvoiceitems.lrinvoiceid and lrinvoice.isactive = true
left outer join lrentry  on lrentry.id = lrid and lrentry.isactive = true
left outer join supplier on supplier.id = stock.supplierid 
left outer join suppliercompany on suppliercompany.supplierid = supplier.id
{3}
where bill.isactive = true and bill.billtype != 9 and bill.billdate>=@fromdate and bill.billdate<@todate {1}
group by {2}";

            string salesApprovalQuery = @"select {0},sum(billitems.qty) stock,
sum(billitems.qty * stock.ibuyingprice::numeric) as cstock, 
sum((billitems.qty * billitems.additionaldiscount::numeric)) scost,
sum((billitems.receivable - billitems.tax) - (billitems.qty * stock.buyingprice::numeric)) margin,
sum(billitems.receivable) as cost,
sum(billitems.qty * stock.buyingprice::numeric) as ivalue, 
sum(billitems.receivable-billitems.tax) as iqty from bulkbill bill 
inner join bulkbillitems billitems on billitems.billid = bill.id and billitems.isactive=true
left outer join salablegoods on salablegoods.id = salablegoodsid 
left outer join stock on stock.id = salablegoods.stockid
left outer join products on products.id = stock.productid
left outer join items on items.id = stock.itemid 
left outer join lrinvoiceitems on lrinvoiceitems.id = stock.invoiceitemid and lrinvoiceitems.isactive = true
left outer join lrinvoice on lrinvoice.id = lrinvoiceitems.lrinvoiceid and lrinvoice.isactive = true
left outer join lrentry  on lrentry.id = lrid and lrentry.isactive = true
left outer join supplier on supplier.id = stock.supplierid 
left outer join suppliercompany on suppliercompany.supplierid = supplier.id
where bill.isactive = true and bill.billdate>=@fromdate and bill.billdate<@todate {1}
group by {2}";

            string salescompareQuery = @"select {0},sum(billitems.qty) stock,
sum(billitems.qty * stock.ibuyingprice::numeric) as cstock,
sum(billitems.receivable) as cost,
sum(case when lrinvoice.entrydate>=@fromdate and lrinvoice.entrydate<@todate then billitems.qty else 0 end) istock,	
sum(case when lrinvoice.entrydate>=@fromdate and lrinvoice.entrydate<@todate then billitems.qty * stock.ibuyingprice::numeric else 0 end) icost		
from bill 
inner join billitems on billitems.billid = bill.id and billitems.isactive=true
left outer join salablegoods on salablegoods.id = salablegoodsid
left outer join stock on stock.id = salablegoods.stockid
left outer join products on products.id = stock.productid
left outer join items on items.id = stock.itemid 
left outer join lrinvoiceitems on lrinvoiceitems.id = stock.invoiceitemid and lrinvoiceitems.isactive = true
left outer join lrinvoice on lrinvoice.id = lrinvoiceitems.lrinvoiceid and lrinvoice.isactive = true
left outer join lrentry  on lrentry.id = lrid and lrentry.isactive = true
left outer join supplier on supplier.id = stock.supplierid 
left outer join suppliercompany on suppliercompany.supplierid = supplier.id
{3}
where bill.isactive = true and billitems.isactive = true and bill.billtype != 9 
and bill.billdate>=@fromdatecompare and bill.billdate<@todatecompare {1}
group by {2}";

 
            public bool HasSupport(Configuration config, Options option)
            {
                switch (option)
                {
                    case Options.SaleWeekDay:
                    case Options.SellingMode:
                    case Options.BillType:
                    case Options.SaleMonthDay:
                    case Options.SalesmanCode:
                    case Options.SalesmanName:
                    case Options.SaleYear:
                    case Options.SoldRate:
                    case Options.SoldRate_Range:
                    case Options.BillNo:
                    case Options.BillDate:
                    case Options.SaleLocation:
                    case Options.SaleReason:
                    case Options.ReturnReason:
                    case Options.Area:
                    case Options.Season:
                    case Options.Customer_Religion:
                    case Options.Customer_City:
                    case Options.SaleMonth:
                    case Options.Counter:
                    case Options.Floor:
                        return config.currentreporttype == Configuration.ReportType.Sales ||
                            config.reporttype == Configuration.ReportType.Sales ||
                            config.reporttype == Configuration.ReportType.SalesOnApproval ||
                            config.reporttype == Configuration.ReportType.SalesCompare ||
                            config.currentreporttype == Configuration.ReportType.Customers;
                    case Options.Incharge:
                        return config.currentreporttype == Configuration.ReportType.Sales ||
                            config.currentreporttype == Configuration.ReportType.SalesCompare ||
                            config.currentreporttype == Configuration.ReportType.AllStock ||
                            config.currentreporttype == Configuration.ReportType.RetailStock ||
                            config.currentreporttype == Configuration.ReportType.WarehouseStock ||
                            config.currentreporttype == Configuration.ReportType.Customers;
                    case Options.Customer_AgeSet:
                    case Options.Customer_Anniversary:
                    case Options.Customer_DOB:
                    case Options.Customer_Age:
                    case Options.Customer_Category:
                    case Options.Customer_Agent:
                    case Options.Customer_Gender:
                    case Options.Customer_LoyeltyNo:
                    case Options.Customer_Area:
                    case Options.SaleAreaGroup1:
                    case Options.SaleAreaGroup2:
                    case Options.Customer_Mobile:
                    case Options.Customer_Name:
                        return config.currentreporttype == Configuration.ReportType.Customers;
                }
                return true;
            }

            public string CreatePriorityQuery(PlanningDataSource.PurchasePlanConfig config)
            {
                string fieldName = GetField(new Configuration()
                {
                    reporttype = Configuration.ReportType.Purchase,
                    currentreporttype = Configuration.ReportType.Purchase
                }, config.primarycolumn, null);
                return string.Format(priorityQuery, fieldName);
            }


            public string CreateStockQuery(Configuration config, string query, bool includesalablegoods)
            {
                //if (!HasSupport(config, config.PrimaryCondtion))
                //    return "";


                List<string> joinlist = new List<string>();
                if(config.PrimaryCondtion == Options.Incharge ||
                   (config.SecondaryCondtions != null && config.SecondaryCondtions.Exists(y=>y.option == Options.Incharge)))
                {
                    joinlist.Add(@"left outer join (select EMPLOYEEID,INCHARGEID,row_number() OVER (PARTITION BY INCHARGEID ORDER BY id DESC) as ranknumber
                                   from Employeeincharges where inchargetype ='PRD'
                                   AND ISACTIVE=TRUE) incharge on incharge.ranknumber = 1 and incharge.INCHARGEID = products.id ");
                }

                if (InfyPOS.Services.ConfigManager.Instance(Owner.Context.OrganizationID).Config.analyser_stock_withzerostock)
                {
                    query = query.Replace("and salablegoods.hasstock = true and salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty)>0", "");
                    query = query.Replace("and stock.hasstock = true and stock.qty - (stock.transfered+stock.returnqty+stock.damagedqty+stock.journalqty)>0", "");
                }

                System.Text.StringBuilder WHERE = new System.Text.StringBuilder();
                if(config.ImageRows == 1)
                {
                    WHERE.Append(" AND ( stock.imageid != '') ");
                }
                if (includesalablegoods && config.locationid > 0 && query.IndexOf("salablegoods") > 0)
                {
                    WHERE.Append(" AND ( salablegoods.locationid = @locationid ) ");

                    if (Services.ConfigManager.Instance(Owner.Context.OrganizationID).Config.analyser_access_location && this.Owner.Context.accessiblelocation != null)
                    {
                        if (!this.Owner.Context.accessiblelocation.Exists(e => e == -1) && this.Owner.Context.accessiblelocation.Count > 0)
                        {
                            WHERE.AppendFormat(" AND ( salablegoods.locationid in ({0}) ) ", string.Join<long>(",", this.Owner.Context.accessiblelocation));
                        }
                    }

                }
                else if (config.locationid > 0 && query.IndexOf("billitems") > 0)
                {
                    WHERE.Append(" AND ( bill.locationid = @locationid ) ");

                    if (Services.ConfigManager.Instance(Owner.Context.OrganizationID).Config.analyser_access_location && this.Owner.Context.accessiblelocation != null)
                    {
                        if (!this.Owner.Context.accessiblelocation.Exists(e => e == -1) && this.Owner.Context.accessiblelocation.Count > 0)
                        {
                            WHERE.AppendFormat(" AND ( bill.locationid in ({0}) ) ", string.Join<long>(",", this.Owner.Context.accessiblelocation));
                        }
                    }
                }

                if (Services.ConfigManager.Instance(Owner.Context.OrganizationID).Config.analyser_access_section &&
                    this.Owner.Context.accessablesections != null)
                {
                    if (!this.Owner.Context.accessablesections.Exists(e => e == -1) && this.Owner.Context.accessablesections.Count > 0)
                    {
                        WHERE.AppendFormat(" AND ( products.salescounterid in ({0}) ) ", string.Join<long>(",", this.Owner.Context.accessablesections));
                    }
                }

                List<AdditionalColumn> conditionalColumns = new List<AdditionalColumn>();
                config.AdditionalColumns = conditionalColumns;
                if (config.SecondaryCondtions != null)
                {
                    foreach (var item in config.SecondaryCondtions)
                    {
                        if (!HasSupport(config, item.option)) continue;

                        if (item.HasCondition)
                            WHERE.Append(" AND (" + GetCondtion(config, item) + ") ");

                        if (item.ShowColumn || (!string.IsNullOrEmpty(config.GroupColumn) && config.GroupColumn.Substring(1) == item.typeName))
                        {
                            var hasreference = Owner.GetReferenceQuery(item.option, item.customoption) != "";
                            if (!hasreference)
                            {
                                hasreference = item.option == Options.SaleWeekDay ||
                                    item.option == Options.SellingMode ||
                                    item.option == Options.BillType ||
                                    item.option == Options.PurchaseMonth ||
                                    item.option == Options.SaleMonth;
                            }
                            conditionalColumns.Add(new AdditionalColumn()
                            {
                                key = GetField(config, item),
                                option = item.option,
                                headername = item.typeName,
                                hasreference = hasreference
                            });
                        }
                    }
                }

                if (!string.IsNullOrEmpty(config.GroupColumn) && !config.GroupColumn.StartsWith("$"))
                {
                    if (config.Columns == null)
                        config.Columns = new List<string>();
                    if (!config.Columns.Contains(config.GroupColumn))
                        config.Columns.Add(config.GroupColumn);
                }

                if ((config.Columns != null && config.Columns.Count > 0) || (conditionalColumns != null && conditionalColumns.Count > 0))
                {
                    string primField = GetField(config, config.PrimaryCondtion, config.PrimaryRange);
                    List<string> columns = new List<string>();
                    int colindex = -1;
                    foreach (var colmn in config.Columns)
                    {
                        colindex++;
                        if (config.GroupColumn == colmn) config.GroupColumnFieldIndex = colindex;
                        switch (colmn)
                        {
                            case "Product":
                                if (primField == "stock.productid") continue;
                                columns.Add("stock.productid");
                                break;
                            case "ImageID":
                                columns.Add("stock.imageid");
                                break;
                            case "Barcode":
                                columns.Add("stock.barcode");
                                break;
                            case "Brand":

                                if (primField == "items.brandid") continue;
                                columns.Add("items.brandid");
                                break;
                            case "Style":
                                if (primField == "items.styleid") continue;
                                columns.Add("items.styleid");

                                break;
                            case "Pattern":
                                if (primField == "items.patternid") continue;
                                columns.Add("items.patternid");

                                break;
                            case "Size":
                                if (primField == "items.sizeid") continue;
                                columns.Add("items.sizeid");

                                break;
                            case "Colour":
                                if (primField == "items.colourid") continue;
                                columns.Add("items.colourid");
                                break;

                            case "Fit":
                                if (primField == "items.fitid") continue;
                                columns.Add("items.fitid");
                                break;

                            case "Sleeve":
                                if (primField == "items.sleeveid") continue;
                                columns.Add("items.sleeveid");
                                break;

                            case "Type":
                                if (primField == "items.typeid") continue;
                                columns.Add("items.typeid");
                                break;

                            case "Item":
                                if (primField == "stock.itemid") continue;
                                columns.Add("stock.itemid");

                                break;
                            case "Design":
                                if (primField == "stock.designid") continue;
                                columns.Add("stock.designid");

                                break;
                            case "Price Tag":
                                if (primField == "stock.pricetagid") continue;
                                columns.Add("stock.pricetagid");

                                break;
                            case "Supplier City":
                                if (primField == "suppliercompany.cityid") continue;
                                columns.Add("suppliercompany.cityid");
                                break;
                            default:
                                break;
                        }
                    }
                    List<string> groupBy = new List<string>();
                    conditionalColumns.RemoveAll(e => string.IsNullOrEmpty(e.key));
                    columns.RemoveAll(e => string.IsNullOrEmpty(e));
                    groupBy.AddRange(columns);

                    var primaryCondition = GetField(config, config.PrimaryCondtion, config.PrimaryRange);

                    if (config.PrimaryCondtion == Options.Product || config.PrimaryCondtion == Options.Brand || config.PrimaryCondtion == Options.Style
                         || config.PrimaryCondtion == Options.Pattern || config.PrimaryCondtion == Options.Size || config.PrimaryCondtion == Options.Colour
                         || config.PrimaryCondtion == Options.Type || config.PrimaryCondtion == Options.Design
                         || config.PrimaryCondtion == Options.Fit || config.PrimaryCondtion == Options.Sleeve || config.PrimaryCondtion == Options.Item)
                    {
                        if (!groupBy.Exists(e => e.StartsWith(primaryCondition + " as ") || e == primaryCondition))
                        {
                            groupBy.Add(primaryCondition);
                            columns.Add(primaryCondition);
                        }
                    }

                    conditionalColumns.ForEach(e =>
                    {
                        if (!columns.Contains(e.key))
                        {
                            groupBy.Add(e.key);
                            e.index = columns.Count;
                            e.columnname = "f" + columns.Count;
                            columns.Add(e.key + " as f" + columns.Count);
                            if (config.GroupColumn != null && config.GroupColumn.Substring(1) == e.headername) config.GroupColumnFieldIndex = columns.Count - 1;
                        }
                        else
                        {
                            e.index = columns.IndexOf(e.key);
                            if (e.key.IndexOf('.') > 0)
                            {
                                e.columnname = e.key.Split('.')[1];
                            }
                            else
                            {
                                e.columnname = e.key;
                            }
                        }
                    });

                    //foreach (var aclmn in config.AdditionalColumns)
                    //    aclmn.index = aclmn.index + config.Columns.Count;

                    return string.Format(query, string.Join(",", columns), WHERE.ToString(), string.Join(",", groupBy),string.Join("\r\n", joinlist));
                }
                else
                {
                    var GROUPBY = GetField(config, config.PrimaryCondtion, config.PrimaryRange);
                    if (config.reporttype == Configuration.ReportType.SalesCompare ||
                        config.reporttype == Configuration.ReportType.StockStress ||
                        config.reporttype == Configuration.ReportType.AllStock ||
                        config.reporttype == Configuration.ReportType.RetailStock ||
                        config.reporttype == Configuration.ReportType.SalesVsStockAnalyser ||
                        config.reporttype == Configuration.ReportType.SaleVsPurchase ||
                        config.reporttype == Configuration.ReportType.StockMarker)
                        return string.Format(query, GROUPBY + " as grp", WHERE.ToString(), GROUPBY, string.Join("\r\n", joinlist));
                    return string.Format(query, GROUPBY, WHERE.ToString(), GROUPBY, string.Join("\r\n", joinlist));
                }
            }

            public string CreateStockQuery(Configuration config)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(@"with totalstock as (");
                config.currentreporttype = Configuration.ReportType.RetailStock;
                sb.AppendLine(CreateBranchStockQuery(config));
                sb.AppendLine(@" UNION ALL ");
                config.currentreporttype = Configuration.ReportType.WarehouseStock;
                sb.AppendLine(CreateWHStockQuery(config));
                sb.AppendLine(@")");
                if (config.Columns != null && (config.Columns.Count > 0 || config.AdditionalColumns.Count > 0))
                {
                    var collist = config.AdditionalColumns.ConvertAll(e => e.columnname);
                    foreach (var colmn in config.Columns)
                    {
                        switch (colmn)
                        {
                            case "Product":
                                collist.Add("productid");
                                break;
                            case "Brand":
                                collist.Add("brandid");
                                break;
                            case "Style":
                                collist.Add("styleid");
                                break;
                            case "Pattern":
                                collist.Add("patternid");
                                break;
                            case "Size":
                                collist.Add("sizeid");
                                break;
                            case "Colour":
                                collist.Add("colourid");
                                break;
                            case "Fit":
                                collist.Add("fitid");
                                break;
                            case "Sleeve":
                                collist.Add("sleeveid");
                                break;
                            case "Type":
                                collist.Add("typeid");
                                break;
                            case "Design":
                                collist.Add("designid");
                                break;
                            case "Price Tag":
                                collist.Add("pricetagid");
                                break;
                            case "Item":
                                collist.Add("itemid");
                                break;
                            case "ImageID":
                                collist.Add("imageid");
                                break;
                            case "Barcode":
                                collist.Add("barcode");
                                break;
                        }
                    }
                    sb.AppendLine(string.Format(@"select {0},sum(stock) as stock,sum(cost) as cost,sum(adjqty) adjqty,sum(adjval) adjval,sum(scost) as scost,sum(istock) as istock,sum(icost) as icost,sum(buyingcost) as buyingcost,sum(sellingprice) as sellingprice, sum(avalue) as avalue from totalstock group by {0}", string.Join(",", collist)));
                }
                else
                {
                    var GROUPBY = GetField(config, config.PrimaryCondtion, config.PrimaryRange).Split('.');
                    sb.AppendLine(string.Format(@"select grp,sum(stock) as stock,sum(cost) as cost,sum(adjqty) adjqty,sum(adjval) adjval,sum(scost) as scost,sum(istock) as istock,sum(icost) as icost,sum(buyingcost) as buyingcost,sum(sellingprice) as sellingprice, sum(avalue) as avalue from totalstock group by grp", GROUPBY.Length == 1 ? GROUPBY[0] : GROUPBY[1]));
                }
                config.currentreporttype = Configuration.ReportType.AllStock;
                return sb.ToString();
            }
            public string CreatePurchaseQuery(Configuration config)
            {
                return CreateStockQuery(config, purchaseQuery, false);
            }
            public string CreatePurchaseReturnQuery(Configuration config)
            {
                return CreateStockQuery(config, purchaseReturnQuery, false);
            }

            public string CreateBranchStockQuery(Configuration config)
            {
                return CreateBranchStockQuery(config, false);
            }
            public string CreateBranchStockQuery(Configuration config, bool asondate)
            {
                config.currentreporttype = Configuration.ReportType.RetailStock;
                return CreateStockQuery(config, asondate ? branchStockAsOnQuery : branchStockQuery, true);
            }
            public string CreateBranchStockQueryForConversion(Configuration config)
            {
                return CreateStockQuery(config, branchStockQuery, true);
            }

            public string CreateWHStockQuery(Configuration config)
            {
                var query = CreateStockQuery(config, whStockQuery, true);
                if (query.Contains("salablegoods.locationid"))
                    query = query//.Replace("group by salablegoods.locationid", "group by grp")
                    .Replace("salablegoods.locationid in", config.warehouselocationid + " in ")
                    .Replace("salablegoods.locationid", "stock.locationid")
                    .Replace("salablegoods.", "stock.");
                //if (query.Contains("0 as locationid"))
                //{
                //    query = query.Replace("0 as locationid", config.warehouselocationid + " as locationid");
                //}
                return query;
            }

            //public string CreateStockRemoveQuery(Configuration config)
            //{
            //    var query = CreateStockQuery(config, stockRemoveQuery);
            //    if (config.locationid <= 0)
            //        query = query.Replace("and salablegoods.locationid=@locationid", "");
            //    return query;
            //}

            public string CreateSalesRemoveQuery(Configuration config, string todateparameter)
            {
                var query = CreateStockQuery(config, salesRemoveQuery.Replace("@todate", todateparameter), true);
                if (config.locationid <= 0)
                    query = query.Replace("and salablegoods.locationid=@locationid", "");
                return query;
            }

            public string CreateInwardQuery(Configuration config)
            {
                var query = CreateStockQuery(config, inwardQuery, true);
                if (config.locationid <= 0)
                    query = query.Replace("and salablegoods.locationid=@locationid", "");
                return query;
            }

            public string CreateCustomerQuery(Configuration config)
            {
                return CreateStockQuery(config, customerQuery, true);
            }

            public string GetField(string colmn)
            {

                switch (colmn)
                {
                    case "ImageID":
                        return "stock.imageid";
                    case "Barcode":
                        return "stock.barcode";
                    case "Product":
                        return "stock.productid";
                    case "Brand":
                        return "items.brandid";
                    case "Style":
                        return "items.styleid";
                    case "Pattern":
                        return "items.patternid";
                    case "Size":
                        return "items.sizeid";
                    case "Colour":
                        return "items.colourid";
                    case "Fit":
                        return "items.fitid";
                    case "Sleeve":
                        return "items.sleeveid";
                    case "Type":
                        return "items.typeid";
                    case "Item":
                        return "stock.itemid";
                    case "Design":
                        return "stock.designid";
                    case "Price Tag":
                        return "stock.pricetagid";
                    case "Supplier City":
                        return "suppliercompany.cityid";
                }
                return "";
            }
            public string StockMarkerQuery(Configuration config)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(@"with baseSales as (");
                sb.AppendLine(CreateStockQuery(config, salesMarkerQuery, true));
                sb.AppendLine(@"), compareStock as (");
                sb.AppendLine(CreateStockQuery(config, branchStockMarkerQuery, true));
                sb.AppendLine(@"), compareStockMarker as (");
                sb.AppendLine(CreateStockQuery(config, StockMarkerStockQuery, true));
                sb.AppendLine(@")");

                if ((config.Columns != null && config.Columns.Count > 0) || config.AdditionalColumns.Count > 0)
                {
                    StringBuilder sbfields = new StringBuilder();
                    StringBuilder sbconidtions = new StringBuilder();
                    StringBuilder sbsmconidtions = new StringBuilder();
                    for (int c = 0; c < config.Columns.Count; c++)
                    {
                        //if (config.Columns[c] == "ImageID")
                        //{
                        //    sbfields.AppendLine(string.Format("coalesce(basesales.imageid,compareStock.imageid) as {0}", config.Columns[c]));
                        //    continue;
                        //}
                        if (sbconidtions.Length > 0)
                        {
                            sbconidtions.Append(" and ");
                            sbsmconidtions.Append(" and ");
                            sbfields.Append(",");
                        }
                        if (GetField(config.Columns[c]) != "")
                        {
                            var GROUPBY = GetField(config.Columns[c]).Split('.');
                            sbconidtions.AppendLine(string.Format("compareStockMarker.{0} = compareStock.{0}", GROUPBY.Length == 1 ? GROUPBY[0] : GROUPBY[1]));
                            sbsmconidtions.AppendLine(string.Format("baseSales.{0} = compareStock.{0}", GROUPBY.Length == 1 ? GROUPBY[0] : GROUPBY[1]));
                            sbfields.AppendLine(string.Format("coalesce(basesales.{0},compareStock.{0}) as {0}", GROUPBY.Length == 1 ? GROUPBY[0] : GROUPBY[1]));
                        }
                    }

                    for (int c = 0; c < config.AdditionalColumns.Count; c++)
                    {
                        if (sbconidtions.Length > 0)
                        {
                            sbconidtions.Append(" and ");
                            sbsmconidtions.Append(" and ");
                            sbfields.Append(",");
                        }
                        sbconidtions.AppendLine(string.Format("compareStockMarker.{0} = compareStock.{0}", config.AdditionalColumns[c].columnname));
                        sbsmconidtions.AppendLine(string.Format("baseSales.{0} = compareStock.{0}", config.AdditionalColumns[c].columnname));
                        sbfields.AppendLine(string.Format("coalesce(basesales.{0},compareStock.{0}) as {0}", config.AdditionalColumns[c].columnname));
                    }

                    sb.AppendLine(string.Format(@"select {0}, saleqty,salecost,salevalue,stockqty,stocksalevalue,stockcost,smqty,smsalevalue,smcost from compareStock
                        left outer join compareStockMarker on {1}
                        left outer join basesales on {2}", sbfields.ToString(),
                        sbconidtions.ToString(),
                        sbsmconidtions.ToString()));
                }
                else
                {
                    var GROUPBY = GetField(config, config.PrimaryCondtion, config.PrimaryRange).Split('.');
                    //sb.AppendLine(string.Format(@"select coalesce(basesales.grp,compareStock.grp) as grp, saleqty,salecost,salevalue,stockqty,stocksalevalue,stockcost,smqty,smsalevalue,smcost from basesales
                    //FULL OUTER JOIN compareStock on compareStock.grp = basesales.grp
                    //FULL OUTER JOIN compareStockMarker on compareStockMarker.grp = basesales.grp", GROUPBY.Length == 1 ? GROUPBY[0] : GROUPBY[1]));
                    sb.AppendLine(string.Format(@"select coalesce(basesales.grp,compareStock.grp) as grp, saleqty,salecost,salevalue,stockqty,stocksalevalue,stockcost,smqty,smsalevalue,smcost from compareStock
                    left outer join compareStockMarker on compareStockMarker.grp = compareStock.grp
                    left outer join basesales on basesales.grp = compareStock.grp", GROUPBY.Length == 1 ? GROUPBY[0] : GROUPBY[1]));

                }
                return sb.ToString();
            }


            public string CreateSalesVsStockQuery(Configuration config)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(@"with baseSales as (");
                sb.AppendLine(CreateSalesQuery(config));
                sb.AppendLine(@"), compareSales as (");
                sb.AppendLine(CreateBranchStockQuery(config));
                sb.AppendLine(@")");

                if ((config.Columns != null && config.Columns.Count > 0) || config.AdditionalColumns.Count > 0)
                {
                    StringBuilder sbfields = new StringBuilder();
                    StringBuilder sbconidtions = new StringBuilder();

                    for (int c = 0; c < config.Columns.Count; c++)
                    {
                        if (sbconidtions.Length > 0)
                        {
                            sbconidtions.Append(" and ");
                            sbfields.Append(",");
                        }
                        var GROUPBY = GetField(config.Columns[c]).Split('.');
                        sbconidtions.AppendLine(string.Format("comparesales.{0} = baseSales.{0}", GROUPBY.Length == 1 ? GROUPBY[0] : GROUPBY[1]));
                        sbfields.AppendLine(string.Format("coalesce(basesales.{0},comparesales.{0}) as {0}", GROUPBY.Length == 1 ? GROUPBY[0] : GROUPBY[1]));
                    }

                    for (int c = 0; c < config.AdditionalColumns.Count; c++)
                    {
                        if (sbconidtions.Length > 0)
                        {
                            sbconidtions.Append(" and ");
                            sbfields.Append(",");
                        }
                        sbconidtions.AppendLine(string.Format("comparesales.{0} = baseSales.{0}", config.AdditionalColumns[c].columnname));
                        sbfields.AppendLine(string.Format("coalesce(basesales.{0},comparesales.{0}) as {0}", config.AdditionalColumns[c].columnname));
                    }

                    sb.AppendLine(string.Format(@"select {0}, basesales.stock,basesales.cstock,basesales.scost,basesales.cost ,
                        comparesales.stock as comparestock,comparesales.buyingcost as comparecost,comparesales.sellingprice as comparesalesprice from basesales
                        FULL OUTER JOIN comparesales on {1}", sbfields.ToString(), sbconidtions.ToString()));
                }
                else
                {
                    var GROUPBY = GetField(config, config.PrimaryCondtion, config.PrimaryRange).Split('.');
                    sb.AppendLine(string.Format(@"select coalesce(basesales.grp,comparesales.grp) as grp, basesales.stock,basesales.cstock,basesales.scost,basesales.cost ,
                comparesales.stock as comparestock,comparesales.buyingcost as comparecost,comparesales.sellingprice as comparesalesprice from basesales
                        FULL OUTER JOIN comparesales on comparesales.grp = basesales.grp", GROUPBY.Length == 1 ? GROUPBY[0] : GROUPBY[1]));
                }
                return sb.ToString();
            }

            public string CreateSalesQuery(Configuration config)
            {
                if (config.reporttype == Configuration.ReportType.SaleVsPurchase)
                {
                    if (config.locationid <= 0)
                        salescompareQuery = salescompareQuery.Replace("and bill.locationid=@locationid", "");

                    if (InfyPOS.Services.ConfigManager.Instance(Owner.Context.OrganizationID).Config.system_isb2bapplication)
                        salescompareQuery = salescompareQuery.Replace("and bill.isb2b=false", "");

                    return CreateStockQuery(config, salescompareQuery, true);
                }

                if (config.locationid <= 0)
                    salesQuery = salesQuery.Replace("and bill.locationid=@locationid", "");

                if (InfyPOS.Services.ConfigManager.Instance(Owner.Context.OrganizationID).Config.system_isb2bapplication)
                    salesQuery = salesQuery.Replace("and bill.isb2b=false", "");

                return CreateStockQuery(config, salesQuery, true);
            }
            string GetNullCondition(Condtion condition, string existing)
            {
                if (condition.longList != null && !condition.longList.Contains(0)) return existing;
                var field = existing.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries).First();
                return string.Format("({0} is null or ({1}))", field, existing);
                //(condition.longList.Contains(0) ? "(supplier.id is null or " : "(") +
            }
            string GetCondtion(Configuration config, Condtion condition)
            {
                switch (condition.option)
                {
                    case Options.Barcode:
                        if (condition.stringList.Count > 5000)
                            throw new Exception("Maximum supported barcode filter is 5000");
                        return "stock.barcode in (" + string.Join(",", condition.stringList.ConvertAll(e => "'" + WebAPI.Data.Utility.SafeSQL(e) + "'")) + ")";
                    case Options.GLNNO:
                        if (condition.stringList.Count > 5000)
                            throw new Exception("Maximum supported barcode filter is 5000");
                        return "stock.serialno in (" + string.Join(",", condition.stringList.ConvertAll(e => "'" + WebAPI.Data.Utility.SafeSQL(e) + "'")) + ")";
                    case Options.BillDate:
                        if (config.reporttype == Configuration.ReportType.SalesCompare)
                        {
                            if (config.issingleyearcreteria)
                                return "cast(Extract(day from bill.billdate) as varchar) in (" + string.Join(",", condition.stringList.ConvertAll(e => "'" + WebAPI.Data.Utility.SafeSQL(e) + "'")) + ")";

                            return "cast(Extract(day from bill.billdate) as varchar)||' - '||cast(Extract(month from bill.billdate) as varchar) in (" + string.Join(",", condition.stringList.ConvertAll(e => "'" + WebAPI.Data.Utility.SafeSQL(e) + "'")) + ")";
                        }
                        return "bill.billdate::date in (" + string.Join(",", condition.dateList.ConvertAll(e => "'" + e.ToString("yyyy-MM-dd") + "'")) + ")";
                    case Options.BillNo:
                        return "bill.billno in (" + string.Join(",", condition.stringList.ConvertAll(e => "'" + WebAPI.Data.Utility.SafeSQL(e) + "'")) + ")";
                    case Options.Brand:
                        return GetNullCondition(condition, "items.brandid in (" + string.Join(",", condition.longList) + ")");
                    case Options.Colour:
                        return GetNullCondition(condition, "items.colourid in (" + string.Join(",", condition.longList) + ")");
                    case Options.Design:
                        return GetNullCondition(condition, "stock.designid in (" + string.Join(",", condition.stringList.ConvertAll(e => "'" + WebAPI.Data.Utility.SafeSQL(e) + "'")) + ")");
                    case Options.DiscountRate:
                        if (config.currentreporttype == Configuration.ReportType.WarehouseStock)
                            return "stock.discount in (" + string.Join(",", condition.decimalList) + ")";
                        return "salablegoods.discount in (" + string.Join(",", condition.decimalList) + ")";
                    case Options.DiscountRate_Range:
                        if (config.currentreporttype == Configuration.ReportType.WarehouseStock)
                            return getRangeCondtion(condition, "stock.discount", "");
                        return getRangeCondtion(condition, "salablegoods.discount", "");
                    case Options.InvDate:
                        return "lrinvoice.invoicedate::date in (" + string.Join(",", condition.dateList.ConvertAll(e => "'" + e.ToString("yyyy-MM-dd") + "'")) + ")";
                    case Options.PurchaseDate:
                        return "lrinvoice.entrydate::date in (" + string.Join(",", condition.dateList.ConvertAll(e => "'" + e.ToString("yyyy-MM-dd") + "'")) + ")";
                    case Options.BarcodedOn:
                        return "lrinvoiceitems.barcodedon::date in (" + string.Join(",", condition.dateList.ConvertAll(e => "'" + e.ToString("yyyy-MM-dd") + "'")) + ")";
                    case Options.DebitNoteDate:
                        return "lrinvoicereturn.returndate::date in (" + string.Join(",", condition.dateList.ConvertAll(e => "'" + e.ToString("yyyy-MM-dd") + "'")) + ")";
                    case Options.InvoiceNo:
                        return "lrinvoice.invoiceno in (" + string.Join(",", condition.stringList.ConvertAll(e => "'" + WebAPI.Data.Utility.SafeSQL(e) + "'")) + ")";
                    case Options.LREntryNo:
                        return "lrentry.lrentryno in (" + string.Join(",", condition.stringList.ConvertAll(e => "'" + WebAPI.Data.Utility.SafeSQL(e) + "'")) + ")";
                    case Options.DebitNoteNo:
                        return "lrinvoicereturn.returnno in (" + string.Join(",", condition.stringList.ConvertAll(e => "'" + WebAPI.Data.Utility.SafeSQL(e) + "'")) + ")";
                    case Options.EntryLocation:
                        return GetNullCondition(condition, "lrentry.entrylocationid in (" + string.Join(",", condition.longList) + ")");
                    case Options.Material:
                        return GetNullCondition(condition, "items.materialid in (" + string.Join(",", condition.longList) + ")");
                    case Options.Fit:
                        return GetNullCondition(condition, "items.fitid in (" + string.Join(",", condition.longList) + ")");
                    case Options.Sleeve:
                        return GetNullCondition(condition, "items.sleeveid in (" + string.Join(",", condition.longList) + ")");
                    case Options.PriceTag:
                        return GetNullCondition(condition, "stock.pricetagid in (" + string.Join(",", condition.longList) + ")");
                    case Options.Pattern:
                        return GetNullCondition(condition, "items.patternid in (" + string.Join(",", condition.longList) + ")");
                    case Options.Product:
                        return GetNullCondition(condition, "stock.productid in (" + string.Join(",", condition.longList) + ")");
                    case Options.ProductCode:
                        return "products.code in (" + string.Join(",", condition.stringList.ConvertAll(e => "'" + WebAPI.Data.Utility.SafeSQL(e) + "'")) + ")";
                    case Options.PurchaseMonth:
                        return "date_part('month',lrinvoice.entrydate) in (" + string.Join(",", condition.longList) + ")";
                    case Options.PurchaseRate:
                        return "lrinvoiceitems.buyingprice in (" + string.Join(",", condition.decimalList.ConvertAll<string>(e => e.ToString() + "")) + ")";
                    case Options.DealerPrice:
                        return "stock.dealerprice in (" + string.Join(",", condition.decimalList.ConvertAll<string>(e => e.ToString() + "")) + ")";
                    case Options.PurchaseTax:
                        return "lrinvoiceitems.purchasetaxpercentage in (" + string.Join(",", condition.decimalList.ConvertAll<string>(e => e.ToString() + "")) + ")";
                    case Options.PurchaseRate_Range:
                        return getRangeCondtion(condition, "(lrinvoiceitems.buyingprice)", "");
                    case Options.DealerPrice_Range:
                        return getRangeCondtion(condition, "(stock.dealerprice)", "");
                    case Options.PurchaseYear:
                        return "date_part('year',lrinvoice.entrydate) in (" + string.Join(",", condition.longList) + ")";
                    case Options.SaleLocation:
                        return "salablegoods.locationid in (" + string.Join(",", condition.longList) + ")";
                    case Options.SaleMonth:
                        return "date_part('month',bill.billdate) in (" + string.Join(",", condition.longList) + ")";
                    case Options.TaxPercentage:
                        if (config.reporttype == Configuration.ReportType.Sales ||
                            config.reporttype == Configuration.ReportType.SalesCompare ||
                            config.reporttype == Configuration.ReportType.SalesOnApproval)
                            return "(billitems.taxpercentage) in (" + string.Join(",", condition.decimalList.ConvertAll<string>(e => e.ToString() + "")) + ")";

                        return @"round(case when stock.ibuyingprice>stock.buyingprice and 
                                ibuyingprice>0 and stock.buyingprice>0 then ((ibuyingprice-stock.buyingprice)/stock.buyingprice*100)::numeric
                                 else
                                0::numeric
                                end,0) in (" + string.Join(",", condition.decimalList.ConvertAll<string>(e => e.ToString() + "::numeric")) + ")";

                    case Options.SaleRate:
                        if (config.currentreporttype == Configuration.ReportType.WarehouseStock)
                            return "(stock.price-stock.discount) in (" + string.Join(",", condition.decimalList.ConvertAll<string>(e => e.ToString() + "")) + ")";
                        return "(salablegoods.price-salablegoods.discount) in (" + string.Join(",", condition.decimalList.ConvertAll<string>(e => e.ToString() + "")) + ")";
                    case Options.SaleRate_Range:
                        if (config.currentreporttype == Configuration.ReportType.WarehouseStock)
                            return getRangeCondtion(condition, "(stock.price-stock.discount)", "");
                        return getRangeCondtion(condition, "(salablegoods.price-salablegoods.discount)", "");
                    case Options.SaleMargin_Range:
                        return getRangeCondtion(condition, "(round(((stock.price-stock.discount)-stock.buyingprice)/stock.buyingprice*100))", "::numeric");
                    case Options.RetailMargin_Range:
                        if (config.currentreporttype == Configuration.ReportType.WarehouseStock)
                            return getRangeCondtion(condition, "(round(((stock.price-stock.discount)-stock.buyingprice)/stock.buyingprice*100))", "::numeric");
                        return getRangeCondtion(condition, "(round(((salablegoods.price-salablegoods.discount)-stock.buyingprice)/stock.buyingprice*100))", "::numeric");
                    case Options.ShelfAge_Range:
                        if (config.reporttype == Configuration.ReportType.Sales ||
                            config.reporttype == Configuration.ReportType.SalesCompare ||
                            config.reporttype == Configuration.ReportType.SalesOnApproval)
                            return getRangeCondtion(condition, "(bill.billdate::date - salablegoods.inwardon::date)", "::integer");
                        if (config.currentreporttype == Configuration.ReportType.WarehouseStock)
                            return getRangeCondtion(condition, "(@current_date::date - stock.stockdate::date)", "::integer");
                        return getRangeCondtion(condition, "(@current_date::date - salablegoods.inwardon::date)", "::integer");
                    case Options.StockAge_Range:
                        if (config.reporttype == Configuration.ReportType.Sales ||
                            config.reporttype == Configuration.ReportType.SalesCompare ||
                            config.reporttype == Configuration.ReportType.SalesOnApproval)
                            return getRangeCondtion(condition, "(bill.billdate::date - lrinvoice.entrydate::date)", "::integer");

                        return getRangeCondtion(condition, "(@current_date::date - lrinvoice.entrydate::date)", "::integer");
                    case Options.SalesmanName:
                        return GetNullCondition(condition, "billitems.salesmanid in (" + string.Join(",", condition.longList) + ")");
                    case Options.Incharge:
                        return GetNullCondition(condition, "incharge.employeeid in (" + string.Join(",", condition.longList) + ")");
                    case Options.SaleYear:
                        return GetNullCondition(condition, "date_part('year',bill.billdate) in (" + string.Join(",", condition.longList) + ")");
                    case Options.Size:
                        return GetNullCondition(condition, "items.sizeid in (" + string.Join(",", condition.longList) + ")");
                    case Options.Agent:
                        return GetNullCondition(condition, "lrentry.agentid in (" + string.Join(",", condition.longList) + ")");
                    case Options.Section:
                        return GetNullCondition(condition, "products.salescounterid in (" + string.Join(",", condition.longList) + ")");
                    case Options.SoldRate:
                        return "billitems.salerate in (" + string.Join(",", condition.decimalList.ConvertAll<string>(e => e.ToString() + "")) + ")";
                    case Options.SoldRate_Range:
                        return getRangeCondtion(condition, "(billitems.salerate)", "");
                    case Options.StockLocation:
                        if (config.reporttype == Configuration.ReportType.PurchaseReturn)
                            return GetNullCondition(condition, "lrinvoicereturn.locationid in (" + string.Join(",", condition.longList) + ")");
                        return GetNullCondition(condition, "salablegoods.locationid in (" + string.Join(",", condition.longList) + ")");
                    case Options.Style:
                        return GetNullCondition(condition, "items.styleid in (" + string.Join(",", condition.longList) + ")");
                    case Options.Company:
                        if (config.reporttype == Configuration.ReportType.Sales ||
                            config.reporttype == Configuration.ReportType.SalesCompare ||
                            config.reporttype == Configuration.ReportType.SalesOnApproval)
                            return GetNullCondition(condition, "bill.companyid in (" + string.Join(",", condition.longList) + ")");

                        if (config.currentreporttype == Configuration.ReportType.RetailStock || config.currentreporttype == Configuration.ReportType.Customers)
                            return GetNullCondition(condition, "salablegoods.companyid in (" + string.Join(",", condition.longList) + ")");

                        if (config.currentreporttype == Configuration.ReportType.WarehouseStock)
                            return GetNullCondition(condition, "stock.companyid in (" + string.Join(",", condition.longList) + ")");

                        return GetNullCondition(condition, "lrinvoice.companyid in (" + string.Join(",", condition.longList) + ")");
                    case Options.ProductGroup:
                        return GetNullCondition(condition, "products.productgroup in (" + string.Join(",", condition.longList) + ")");
                    case Options.BarcodeGroup:
                        return GetNullCondition(condition, "products.barcodeid in (" + string.Join(",", condition.longList) + ")");
                    case Options.ReturnReason:
                        return GetNullCondition(condition, "Bill.returnreasonid in (" + string.Join(",", condition.longList) + ")");
                    case Options.SaleReason:
                        return GetNullCondition(condition, "Bill.salereasonid in (" + string.Join(",", condition.longList) + ")");

                    case Options.Counter:
                        return GetNullCondition(condition, "Bill.counterid in (" + string.Join(",", condition.longList) + ")");

                    case Options.Floor:
                        return GetNullCondition(condition, "Bill.floorid in (" + string.Join(",", condition.longList) + ")");


                    case Options.Area:
                        return GetNullCondition(condition, "Bill.areaid in (" + string.Join(",", condition.longList) + ")");
                    case Options.Season:
                        return GetNullCondition(condition, "Bill.seasonid in (" + string.Join(",", condition.longList) + ")");
                    case Options.Supplier:
                        return GetNullCondition(condition, "supplier.id in (" + string.Join(",", condition.longList) + ")");
                    case Options.Source_Supplier:
                        return GetNullCondition(condition, "stock.srcsupplierid in (" + string.Join(",", condition.longList) + ")");
                    case Options.JobworkSupplier:
                        return GetNullCondition(condition, "lrinvoiceitems.jobworkerid in (" + string.Join(",", condition.longList) + ")");
                    case Options.Supplier_City:
                        return GetNullCondition(condition, "suppliercompany.cityid in (" + string.Join(",", condition.longList) + ")");
                    case Options.Type:
                        return GetNullCondition(condition, "items.typeid in (" + string.Join(",", condition.longList) + ")");
                    case Options.Item:
                        return GetNullCondition(condition, "stock.id in (" + string.Join(",", condition.longList) + ")");
                    case Options.Tax:
                        return GetNullCondition(condition, "products.salestaxid in (" + string.Join(",", condition.longList) + ")");
                    case Options.SaleWeekDay:
                        return "date_part('dow',bill.billdate) in (" + string.Join(",", condition.longList) + ")";
                    case Options.SellingMode:
                        return condition.longList.Exists(e => e == 1) ? "bill.isb2b=true" : "bill.isb2b=false";
                    case Options.BillType:
                        if (condition.longList.Exists(e => e == 0)
                            && condition.longList.Exists(e => e == 1)
                            && condition.longList.Exists(e => e == 2)
                            && condition.longList.Exists(e => e == 3))
                            return "bill.billtype!=9";
                        List<string> cndbilltype = new List<string>();
                        if (condition.longList.Exists(e => e == 1))
                            cndbilltype.Add("bill.billtype=8");
                        if (condition.longList.Exists(e => e == 2))
                            cndbilltype.Add("bill.istransfer=true");
                        if (condition.longList.Exists(e => e == 3))
                            cndbilltype.Add("bill.isexport=true");
                        if(cndbilltype.Count == 0)
                            cndbilltype.Add("bill.billtype!=9");
                        return string.Join(" or ", cndbilltype);
                    case Options.SaleMonthDay:
                        return getDayRangeCondtion(condition, "date_part('d',bill.billdate)", "::integer");
                    case Options.HSNCode:
                        return "stock.hsncode in (" + string.Join(",", condition.stringList.ConvertAll(e => "'" + WebAPI.Data.Utility.SafeSQL(e) + "'")) + ")";

                    case Options.Customer_AgeSet:
                        return GetNullCondition(condition, "agelist.id in (" + string.Join(",", condition.longList) + ")");
                    case Options.Customer_Religion:
                        return GetNullCondition(condition, "customer.religionid in (" + string.Join(",", condition.longList) + ")");
                    case Options.Customer_City:
                        return GetNullCondition(condition, "customer.cityid in (" + string.Join(",", condition.longList) + ")");

                    case Options.Customer_Anniversary:
                        return "customer.marriagedate::date in (" + string.Join(",", condition.dateList.ConvertAll(e => "'" + e.ToString("yyyy-MM-dd") + "'")) + ")";
                    case Options.Customer_DOB:
                        return "customer.dateofbirth::date in (" + string.Join(",", condition.dateList.ConvertAll(e => "'" + e.ToString("yyyy-MM-dd") + "'")) + ")";
                    case Options.Customer_Gender:
                        return "customer.gender in (" + string.Join(",", condition.stringList.ConvertAll(e => "'" + WebAPI.Data.Utility.SafeSQL(e) + "'")) + ")";
                    case Options.Customer_Category:
                        return GetNullCondition(condition, "customer.categoryid in (" + string.Join(",", condition.longList) + ")");
                    case Options.Customer_Agent:
                        return GetNullCondition(condition, "customer.agentid in (" + string.Join(",", condition.longList) + ")");
                    case Options.Customer_LoyeltyNo:
                        return "customer.loyeltyno in (" + string.Join(",", condition.stringList.ConvertAll(e => "'" + WebAPI.Data.Utility.SafeSQL(e) + "'")) + ")";
                    case Options.Customer_Area:
                        return "customer.area in (" + string.Join(",", condition.stringList.ConvertAll(e => "'" + WebAPI.Data.Utility.SafeSQL(e) + "'")) + ")";
                    case Options.SaleAreaGroup1:
                        return GetNullCondition(condition, "customerarea.saleareagroup1 in (" + string.Join(",", condition.longList) + ")");
                    case Options.SaleAreaGroup2:
                        return GetNullCondition(condition, "customerarea.saleareagroup2 in (" + string.Join(",", condition.longList) + ")");
                    case Options.Customer_Mobile:
                        return "customer.mobileno in (" + string.Join(",", condition.stringList.ConvertAll(e => "'" + WebAPI.Data.Utility.SafeSQL(e) + "'")) + ")";
                    //case Options.Customer_Name:
                    //    return "customer.name in (" + string.Join(",", condition.stringList.ConvertAll(e => "'" + WebAPI.Data.Utility.SafeSQL(e) + "'")) + ")";
                    case Options.Customer_Name:
                        return GetNullCondition(condition, "customer.id in (" + string.Join(",", condition.longList) + ")");
                    case Options.Customer_Age:
                        return "extract (year from (age(customer.dateofbirth))) in (" + string.Join(",", condition.longList) + ")";
                    case Options.Custom_ProductGroups:
                        var customfied = "products.productgroup_" + condition.customoption.Split(':')[0];
                        return GetNullCondition(condition, customfied + " in (" + string.Join(",", condition.longList) + ")");
                    case Options.Custom_Fields:
                        return GetCustomFieldCondition(condition, condition.customoption);
                    case Options.StockMarker:
                        return GetNullCondition(condition, "stockmarker.markerid in (" + string.Join(",", condition.longList) + ")");
                }
                return "";
            }
            private string GetCustomFieldCondition(Condtion condition, string customoption)
            {
                var customoptions = new CustomField(customoption);
                if (customoptions.referenceid == 1)
                    return "lrinvoiceitems." + customoptions.cfname + " in (" + string.Join(",", condition.stringList.ConvertAll(e => "'" + WebAPI.Data.Utility.SafeSQL(e) + "'")) + ")";
                else if (customoptions.referenceid == 2)
                    return "lrinvoiceitems." + customoptions.cfname + " in (" + string.Join(", ", condition.decimalList) + ")";
                else if (customoptions.referenceid == 3)
                    return "lrinvoiceitems." + customoptions.cfname + " in (" + string.Join(",", condition.dateList.ConvertAll(e => "'" + e.ToString("yyyy-MM-dd") + "'")) + ")";
                else
                    return "lrinvoiceitems." + customoptions.cfname + " in (" + string.Join(", ", condition.longList) + ")";
            }


            string GetField(Configuration config, Condtion condition)
            {
                Options option = condition.option;
                Condtion.Range range = condition.range;
                switch (option)
                {
                    case Options.Barcode:
                        return "stock.barcode";
                    case Options.GLNNO:
                        return "stock.serialno";
                    case Options.BillDate:
                        if (config.reporttype == Configuration.ReportType.SalesCompare)
                        {
                            if (config.issingleyearcreteria)
                                return "cast(Extract(day from bill.billdate) as varchar)";
                            else
                                return "cast(Extract(day from bill.billdate) as varchar)||' - '||cast(Extract(month from bill.billdate) as varchar)";
                        }
                        return "bill.billdate::date";
                    case Options.BillNo:
                        return "bill.billno";
                    case Options.Brand:
                        return "items.brandid";
                    case Options.Colour:
                        return "items.colourid";

                    case Options.Fit:
                        return "items.fitid";
                    case Options.Sleeve:
                        return "items.sleeveid";
                    case Options.PriceTag:
                        return "stock.pricetagid";
                    case Options.Design:
                        return "stock.designid";
                    case Options.DiscountRate:
                        if (config.currentreporttype == Configuration.ReportType.WarehouseStock)
                            return "stock.discount";
                        return "salablegoods.discount";
                    case Options.DiscountRate_Range:
                        if (config.currentreporttype == Configuration.ReportType.WarehouseStock)
                            return getRangeCase(range, "stock.discount", "");
                        return getRangeCase(range, "salablegoods.discount", "");
                    case Options.InvDate:
                        return "lrinvoice.invoicedate";
                    case Options.PurchaseDate:
                        return "lrinvoice.entrydate::date";
                    case Options.BarcodedOn:
                        return "lrinvoiceitems.barcodedon::date";
                    case Options.DebitNoteDate:
                        return "lrinvoicereturn.returndate::date";
                    case Options.InvoiceNo:
                        return "lrinvoice.invoiceno";
                    case Options.LREntryNo:
                        return "lrentry.lrentryno";
                    case Options.DebitNoteNo:
                        return "lrinvoicereturn.returnno";
                    case Options.EntryLocation:
                        return "lrentry.entrylocationid";
                    case Options.Material:
                        return "items.materialid";
                    case Options.Pattern:
                        return "items.patternid";
                    case Options.Product:
                        return "stock.productid";
                    case Options.ProductCode:
                        return "products.code";
                    case Options.PurchaseMonth:
                        return "date_part('month',lrinvoice.entrydate)";
                    case Options.PurchaseRate:
                        return "lrinvoiceitems.buyingprice";
                    case Options.DealerPrice:
                        return "stock.dealerprice";
                    case Options.PurchaseTax:
                        return "lrinvoiceitems.purchasetaxpercentage";
                    case Options.PurchaseYear:
                        return "date_part('year',lrinvoice.entrydate)";
                    case Options.SaleLocation:
                        return "bill.locationid";
                    case Options.SaleMonth:
                        return "date_part('month',bill.billdate)";
                    case Options.SaleRate:
                        if (config.currentreporttype == Configuration.ReportType.WarehouseStock)
                            return "(stock.price-stock.discount)";
                        return "(salablegoods.price-salablegoods.discount)";
                    case Options.SaleRate_Range:
                        if (config.currentreporttype == Configuration.ReportType.WarehouseStock)
                            return getRangeCase(range, "(stock.price-stock.discount)", "");
                        return getRangeCase(range, "(salablegoods.price-salablegoods.discount)", "");
                    case Options.SaleMargin_Range:
                        return getRangeCase(range, "(round(((stock.price-stock.discount)-stock.buyingprice)/stock.buyingprice*100))", "::numeric");
                    case Options.RetailMargin_Range:
                        if (config.currentreporttype == Configuration.ReportType.WarehouseStock)
                            return getRangeCase(range, "(round(((stock.price-stock.discount)-stock.buyingprice)/stock.buyingprice*100))", "::numeric");
                        return getRangeCase(range, "(round(((salablegoods.price-salablegoods.discount)-stock.buyingprice)/stock.buyingprice*100))", "::numeric");
                    case Options.StockAge_Range:

                        if (config.reporttype == Configuration.ReportType.Sales ||
                            config.reporttype == Configuration.ReportType.SalesCompare ||
                            config.reporttype == Configuration.ReportType.SalesOnApproval)
                            return getRangeCase(range, "(bill.billdate::date - lrinvoice.entrydate::date)", "::integer");

                        return getRangeCase(range, "(@current_date::date - lrinvoice.entrydate::date)", "::integer");
                    case Options.ShelfAge_Range:
                        if (config.reporttype == Configuration.ReportType.Sales ||
                            config.reporttype == Configuration.ReportType.SalesCompare ||
                            config.reporttype == Configuration.ReportType.SalesOnApproval)
                            return getRangeCase(range, "(bill.billdate::date - salablegoods.inwardon::date)", "::integer");
                        if (config.currentreporttype == Configuration.ReportType.WarehouseStock)
                            return getRangeCase(range, "(@current_date::date - stock.stockdate::date)", "::integer");
                        return getRangeCase(range, "(@current_date::date - salablegoods.inwardon::date)", "::integer");
                    case Options.TaxPercentage:
                        if (config.reporttype == Configuration.ReportType.Sales ||
                            config.reporttype == Configuration.ReportType.SalesCompare ||
                            config.reporttype == Configuration.ReportType.SalesOnApproval)
                            return "(billitems.taxpercentage)";
                        return @"round(case when stock.ibuyingprice>stock.buyingprice and 
                                ibuyingprice>0 and stock.buyingprice>0 then ((ibuyingprice-stock.buyingprice)/stock.buyingprice*100)::numeric
                                 else
                                0::numeric
                                end,0)";

                    case Options.Company:
                        if (config.reporttype == Configuration.ReportType.Sales ||
                            config.reporttype == Configuration.ReportType.SalesCompare ||
                            config.reporttype == Configuration.ReportType.SalesOnApproval)
                            return "bill.companyid";
                        if (config.reporttype == Configuration.ReportType.SalesVsStockAnalyser && config.currentreporttype == Configuration.ReportType.WarehouseStock)
                            return "bill.companyid";
                        if (config.currentreporttype == Configuration.ReportType.RetailStock)
                            return "salablegoods.companyid";
                        if (config.currentreporttype == Configuration.ReportType.Customers)
                            return "bill.companyid";
                        if (config.currentreporttype == Configuration.ReportType.WarehouseStock)
                            return "stock.companyid";

                        return "lrinvoice.companyid";
                    case Options.ProductGroup:
                        return "products.productgroup";
                    case Options.BarcodeGroup:
                        return "products.barcodeid";
                    case Options.SaleAreaGroup1:
                        return "customerarea.saleareagroup1";
                    case Options.SaleAreaGroup2:
                        return "customerarea.saleareagroup2";
                    case Options.ReturnReason:
                        return "Bill.returnreasonid";
                    case Options.SaleReason:
                        return "Bill.salereasonid";
                    case Options.Counter:
                        return "Bill.counterid";
                    case Options.Floor:
                        return "Bill.floorid";
                    case Options.Area:
                        return "Bill.areaid";
                    case Options.Season:
                        return "Bill.seasonid";
                    case Options.DealerPrice_Range:
                        return getRangeCase(range, "stock.dealerprice", "");
                    case Options.PurchaseRate_Range:
                        return getRangeCase(range, "lrinvoiceitems.buyingprice", "");
                    case Options.SaleWeekDay:
                        return "date_part('dow',bill.billdate)";
                    case Options.SellingMode:
                        return "bill.isb2b::integer";
                    case Options.BillType:
                        return @"case when bill.billtype=8 then 1::integer 
                                      when bill.istransfer then 2::integer
                                      when bill.isexport then 3::integer
                                      else 0::integer end";
                    case Options.SaleMonthDay:
                        return getDayRangeCase(range, "date_part('d',bill.billdate)", "::integer");
                    case Options.SalesmanName:
                    case Options.SalesmanCode:
                        return "billitems.salesmanid";
                    case Options.Incharge:
                        return "Incharge.employeeid";
                    case Options.SaleYear:
                        return "date_part('year',bill.billdate)";
                    case Options.Section:
                        return "products.salescounterid";
                    case Options.Size:
                        return "items.sizeid";

                    case Options.Agent:
                        return "lrentry.agentid";
                    case Options.SoldRate:
                        return "billitems.salerate";
                    case Options.SoldRate_Range:
                        return getRangeCase(range, "billitems.salerate", "");
                    case Options.StockLocation:
                        if (config.reporttype == Configuration.ReportType.Purchase)
                            return "COALESCE(lrinvoice.locationid,lrentry.locationid,0::bigint)";
                        if (config.reporttype == Configuration.ReportType.PurchaseReturn)
                            return "COALESCE(lrinvoicereturn.locationid,lrentry.locationid,0::bigint)";
                        return "salablegoods.locationid";
                    case Options.Style:
                        return "items.styleid";
                    case Options.Supplier:
                        return "supplier.id";
                    case Options.Source_Supplier:
                        return "stock.srcsupplierid";
                    case Options.JobworkSupplier:
                        return "lrinvoiceitems.jobworkerid";
                    case Options.Supplier_City:
                        return "suppliercompany.cityid";
                    case Options.Type:
                        return "items.typeid";
                    case Options.Item:
                        return "stock.itemid";
                    case Options.HSNCode:
                        return "stock.hsncode";
                    case Options.Tax:
                        return "products.salestaxid";

                    case Options.Customer_City:
                        return "customer.cityid";
                    case Options.Customer_Religion:
                        return "customer.religionid";
                    case Options.Customer_Category:
                        return "customer.categoryid";
                    case Options.Customer_Agent:
                        return "customer.agentid";
                    case Options.Customer_AgeSet:
                        return "agelist.id";
                    case Options.Customer_Anniversary:
                        return "customer.marriagedate::date";
                    case Options.Customer_DOB:
                        return "customer.dateofbirth::date";
                    case Options.Customer_Gender:
                        return "customer.gender";
                    case Options.Customer_Area:
                        return "customer.area";
                    case Options.Customer_LoyeltyNo:
                        return "customer.loyeltyno";
                    case Options.Customer_Mobile:
                        return "customer.mobileno";
                    case Options.Customer_Name:
                        return "customer.id";
                    case Options.Customer_Age:
                        return "extract (year from (age(customer.dateofbirth)))";
                    case Options.StockMarker:
                        return "stockmarker.markerid";
                    case Options.Custom_Fields:
                        return GetCustomField(config.CustomOption);
                    case Options.Custom_ProductGroups:
                        return "products.productgroup_" + condition.customoption.Split(':')[0];
                }
                return "";
            }
            string GetField(Configuration config, Options option, Condtion.Range range)
            {
                switch (option)
                {
                    case Options.Barcode:
                        return "stock.barcode";
                    case Options.GLNNO:
                        return "stock.serialno";
                    case Options.BillDate:
                        if (config.reporttype == Configuration.ReportType.SalesCompare)
                        {
                            if (config.issingleyearcreteria)
                                return "cast(Extract(day from bill.billdate) as varchar)";
                            else
                                return "cast(Extract(day from bill.billdate) as varchar)||' - '||cast(Extract(month from bill.billdate) as varchar)";
                        }
                        return "bill.billdate::date";
                    case Options.BillNo:
                        return "bill.billno";
                    case Options.Brand:
                        return "items.brandid";
                    case Options.Colour:
                        return "items.colourid";

                    case Options.Fit:
                        return "items.fitid";
                    case Options.Sleeve:
                        return "items.sleeveid";
                    case Options.PriceTag:
                        return "stock.pricetagid";
                    case Options.Design:
                        return "stock.designid";
                    case Options.DiscountRate:
                        if (config.currentreporttype == Configuration.ReportType.WarehouseStock)
                            return "stock.discount";
                        return "salablegoods.discount";
                    case Options.DiscountRate_Range:
                        if (config.currentreporttype == Configuration.ReportType.WarehouseStock)
                            return getRangeCase(range, "stock.discount", "");
                        return getRangeCase(range, "salablegoods.discount", "");
                    case Options.InvDate:
                        return "lrinvoice.invoicedate";
                    case Options.PurchaseDate:
                        return "lrinvoice.entrydate::date";
                    case Options.BarcodedOn:
                        return "lrinvoiceitems.barcodedon::date";
                    case Options.DebitNoteDate:
                        return "lrinvoicereturn.returndate::date";
                    case Options.InvoiceNo:
                        return "lrinvoice.invoiceno";
                    case Options.LREntryNo:
                        return "lrentry.lrentryno";
                    case Options.DebitNoteNo:
                        return "lrinvoicereturn.returnno";
                    case Options.EntryLocation:
                        return "lrentry.entrylocationid";
                    case Options.Material:
                        return "items.materialid";
                    case Options.Pattern:
                        return "items.patternid";
                    case Options.Product:
                        return "stock.productid";
                    case Options.ProductCode:
                        return "products.code";
                    case Options.PurchaseMonth:
                        return "date_part('month',lrinvoice.entrydate)";
                    case Options.PurchaseRate:
                        return "lrinvoiceitems.buyingprice";
                    case Options.DealerPrice:
                        return "stock.dealerprice";
                    case Options.PurchaseTax:
                        return "lrinvoiceitems.purchasetaxpercentage";
                    case Options.PurchaseYear:
                        return "date_part('year',lrinvoice.entrydate)";
                    case Options.SaleLocation:
                        return "bill.locationid";
                    case Options.SaleMonth:
                        return "date_part('month',bill.billdate)";
                    case Options.SaleRate:
                        if (config.currentreporttype == Configuration.ReportType.WarehouseStock)
                            return "(stock.price-stock.discount)";
                        return "(salablegoods.price-salablegoods.discount)";
                    case Options.SaleRate_Range:
                        if (config.currentreporttype == Configuration.ReportType.WarehouseStock)
                            return getRangeCase(range, "(stock.price-stock.discount)", "");
                        return getRangeCase(range, "(salablegoods.price-salablegoods.discount)", "");
                    case Options.SaleMargin_Range:
                        return getRangeCase(range, "(round(((stock.price-stock.discount)-stock.buyingprice)/stock.buyingprice*100))", "::numeric");
                    case Options.RetailMargin_Range:
                        if (config.currentreporttype == Configuration.ReportType.WarehouseStock)
                            return getRangeCase(range, "(round(((stock.price-stock.discount)-stock.buyingprice)/stock.buyingprice*100))", "::numeric");
                        return getRangeCase(range, "(round(((salablegoods.price-salablegoods.discount)-stock.buyingprice)/stock.buyingprice*100))", "::numeric");
                    case Options.StockAge_Range:

                        if (config.reporttype == Configuration.ReportType.Sales ||
                            config.reporttype == Configuration.ReportType.SalesCompare ||
                            config.reporttype == Configuration.ReportType.SalesOnApproval)
                            return getRangeCase(range, "(bill.billdate::date - lrinvoice.entrydate::date)", "::integer");

                        return getRangeCase(range, "(@current_date::date - lrinvoice.entrydate::date)", "::integer");
                    case Options.ShelfAge_Range:
                        if (config.reporttype == Configuration.ReportType.Sales ||
                            config.reporttype == Configuration.ReportType.SalesCompare ||
                            config.reporttype == Configuration.ReportType.SalesOnApproval)
                            return getRangeCase(range, "(bill.billdate::date - salablegoods.inwardon::date)", "::integer");
                        if (config.currentreporttype == Configuration.ReportType.WarehouseStock)
                            return getRangeCase(range, "(@current_date::date - stock.stockdate::date)", "::integer");
                        return getRangeCase(range, "(@current_date::date - salablegoods.inwardon::date)", "::integer");
                    case Options.TaxPercentage:
                        if (config.reporttype == Configuration.ReportType.Sales ||
                            config.reporttype == Configuration.ReportType.SalesCompare ||
                            config.reporttype == Configuration.ReportType.SalesOnApproval)
                            return "(billitems.taxpercentage)";
                        return @"round(case when stock.ibuyingprice>stock.buyingprice and 
                                ibuyingprice>0 and stock.buyingprice>0 then ((ibuyingprice-stock.buyingprice)/stock.buyingprice*100)::numeric
                                 else
                                0::numeric
                                end,0)";

                    case Options.Company:
                        if (config.reporttype == Configuration.ReportType.Sales ||
                            config.reporttype == Configuration.ReportType.SalesCompare ||
                            config.reporttype == Configuration.ReportType.SalesOnApproval)
                            return "bill.companyid";
                        if (config.reporttype == Configuration.ReportType.SalesVsStockAnalyser && config.currentreporttype == Configuration.ReportType.WarehouseStock)
                            return "bill.companyid";
                        if (config.currentreporttype == Configuration.ReportType.RetailStock)
                            return "salablegoods.companyid";
                        if (config.currentreporttype == Configuration.ReportType.Customers)
                            return "bill.companyid";
                        if (config.currentreporttype == Configuration.ReportType.WarehouseStock)
                            return "stock.companyid";

                        return "lrinvoice.companyid";
                    case Options.ProductGroup:
                        return "products.productgroup";
                    case Options.BarcodeGroup:
                        return "products.barcodeid";
                    case Options.ReturnReason:
                        return "Bill.returnreasonid";
                    case Options.SaleReason:
                        return "Bill.salereasonid";
                    case Options.Counter:
                        return "Bill.counterid";
                    case Options.Floor:
                        return "Bill.floorid";
                    case Options.Area:
                        return "Bill.areaid";
                    case Options.Season:
                        return "Bill.seasonid";
                    case Options.DealerPrice_Range:
                        return getRangeCase(range, "stock.dealerprice", "");
                    case Options.PurchaseRate_Range:
                        return getRangeCase(range, "lrinvoiceitems.buyingprice", "");
                    case Options.SaleWeekDay:
                        return "date_part('dow',bill.billdate)";
                    case Options.SellingMode:
                        return "bill.isb2b::integer";
                    case Options.BillType:
                        return @"case when bill.billtype=8 then 1::integer 
                                      when bill.istransfer then 2::integer
                                      when bill.isexport then 3::integer
                                      else 0::integer end";
                    case Options.SaleMonthDay:
                        return getDayRangeCase(range, "date_part('d',bill.billdate)", "::integer");
                    case Options.SalesmanName:
                    case Options.SalesmanCode:
                        return "billitems.salesmanid";
                    case Options.Incharge:
                        return "Incharge.employeeid";
                    case Options.SaleYear:
                        return "date_part('year',bill.billdate)";
                    case Options.Section:
                        return "products.salescounterid";
                    case Options.Size:
                        return "items.sizeid";

                    case Options.Agent:
                        return "lrentry.agentid";
                    case Options.SoldRate:
                        return "billitems.salerate";
                    case Options.SoldRate_Range:
                        return getRangeCase(range, "billitems.salerate", "");
                    case Options.StockLocation:
                        if (config.reporttype == Configuration.ReportType.Purchase)
                            return "COALESCE(lrinvoice.locationid,lrentry.locationid,0::bigint)";
                        if (config.reporttype == Configuration.ReportType.PurchaseReturn)
                            return "COALESCE(lrinvoicereturn.locationid,lrentry.locationid,0::bigint)";
                        return "salablegoods.locationid";
                    case Options.Style:
                        return "items.styleid";
                    case Options.Supplier:
                        return "supplier.id";
                    case Options.Source_Supplier:
                        return "stock.srcsupplierid";
                    case Options.JobworkSupplier:
                        return "lrinvoiceitems.jobworkerid";
                    case Options.Supplier_City:
                        return "suppliercompany.cityid";
                    case Options.Type:
                        return "items.typeid";
                    case Options.Item:
                        return "stock.itemid";
                    case Options.HSNCode:
                        return "stock.hsncode";
                    case Options.Tax:
                        return "products.salestaxid";

                    case Options.Customer_City:
                        return "customer.cityid";
                    case Options.Customer_Religion:
                        return "customer.religionid";
                    case Options.Customer_Category:
                        return "customer.categoryid";
                    case Options.Customer_Agent:
                        return "customer.agentid";
                    case Options.Customer_AgeSet:
                        return "agelist.id";
                    case Options.Customer_Anniversary:
                        return "customer.marriagedate::date";
                    case Options.Customer_DOB:
                        return "customer.dateofbirth::date";
                    case Options.Customer_Gender:
                        return "customer.gender";
                    case Options.Customer_Area:
                        return "customer.area";
                    case Options.Customer_LoyeltyNo:
                        return "customer.loyeltyno";
                    case Options.Customer_Mobile:
                        return "customer.mobileno";
                    case Options.Customer_Name:
                        return "customer.id";
                    case Options.Customer_Age:
                        return "extract (year from (age(customer.dateofbirth)))";
                    case Options.StockMarker:
                        return "stockmarker.markerid";
                    case Options.Custom_Fields:
                        return GetCustomField(config.CustomOption);
                    case Options.Custom_ProductGroups:
                        if (config.CustomOption == null)
                            return "products.productgroup";
                        return "products.productgroup_" + config.CustomOption.Split(':')[0];
                }
                return "";
            }
            private string GetCustomField(string customoption)
            {
                var customoptions = new CustomField(customoption);
                return "lrinvoiceitems." + customoptions.cfname;
            }

            private string getDayRangeCase(Condtion.Range rangecondition, string field, string typeCast)
            {
                rangecondition = new Condtion.Range();
                var rangelist = rangecondition.getDayList();
                System.Text.StringBuilder conditionlist = new System.Text.StringBuilder();
                foreach (var rng in rangelist)
                {
                    if (rng.start == 0)
                    {
                        conditionlist.AppendLine("when " + field + "<" + rng.end + typeCast + " then '" + rng.end + "'");
                    }
                    else if (rng.end == 0)
                    {
                        conditionlist.AppendLine("when " + field + ">" + rng.start + typeCast + " then '" + rng.start + "'");
                    }
                    else
                    {
                        conditionlist.AppendLine("when " + field + " >= " + rng.start + typeCast + " AND " + field + "<=" + rng.end + typeCast + " then '" + rng.start + " - " + rng.end + "'");
                    }

                }
                if (rangelist.Count > 20)
                    throw new Exception("Maximum supported ranges 20");
                return " case " + conditionlist.ToString() + " end ";
            }

            private string getRangeCase(Condtion.Range rangecondition, string field, string typeCast)
            {
                var rangelist = rangecondition.getList();
                System.Text.StringBuilder conditionlist = new System.Text.StringBuilder();
                foreach (var rng in rangelist)
                {
                    if (rng.start == 0)
                    {
                        conditionlist.AppendLine("when " + field + "<" + rng.end + typeCast + " then '" + rng.end + "'");
                    }
                    else if (rng.end == 0)
                    {
                        conditionlist.AppendLine("when " + field + ">=" + rng.start + typeCast + " then '" + rng.start + "'");
                    }
                    else
                    {
                        conditionlist.AppendLine("when " + field + " >= " + rng.start + typeCast + " AND " + field + "<" + rng.end + typeCast + " then '" + rng.start + " - " + rng.end + "'");
                    }

                }
                if (rangelist.Count > 20)
                    throw new Exception("Maximum supported ranges 20");
                return " case " + conditionlist.ToString() + " end ";
            }

            private string getRangeCondtion(Condtion condition, string field, string typeCast)
            {
                var rangelist = condition.range.getList();
                List<string> conditionlist = new List<string>();
                foreach (var item in condition.stringList)
                {
                    string[] parts = item.Replace(" ", "").Split('-');
                    decimal start = 0; decimal end = 0;
                    if (parts.Length == 1)
                    {
                        if (decimal.TryParse(parts[0], out start))
                        {
                            if (condition.range.end == start)
                            {
                                conditionlist.Add(field + ">=" + start + typeCast);
                            }
                            else
                            {
                                conditionlist.Add(field + "<" + start + typeCast);
                            }
                        }
                        else
                        {
                            conditionlist.Add(field + " is null ");
                        }
                    }
                    else if (decimal.TryParse(parts[0], out start) && decimal.TryParse(parts[1], out end))
                    {
                        if (rangelist.Exists(e => e.start == start && e.end == end))
                        {
                            var rng = rangelist.Find(e => e.start == start && e.end == end);
                            conditionlist.Add("(" + field + " >= " + rng.start + typeCast + " AND " + field + "<" + rng.end + typeCast + ")");
                        }
                    }
                }
                if (conditionlist.Count > 20)
                    throw new Exception("Maximum supported ranges 20");
                return "(" + string.Join(" OR ", conditionlist) + ")";
            }

            private string getDayRangeCondtion(Condtion condition, string field, string typeCast)
            {
                if (condition.range == null)
                {
                    condition.range = new Condtion.Range();
                }
                var rangelist = condition.option == Options.SaleMonthDay ? condition.range.getDayList() : condition.range.getList();
                List<string> conditionlist = new List<string>();
                foreach (var item in condition.stringList)
                {
                    string[] parts = item.Replace(" ", "").Split('-');
                    decimal start = 0; decimal end = 0;
                    if (parts.Length == 1)
                    {
                        if (decimal.TryParse(parts[0], out start))
                        {
                            if (rangelist[0].end == start)
                            {
                                conditionlist.Add(field + "<" + rangelist[0].end + typeCast);
                            }
                            else
                            {
                                conditionlist.Add(field + ">" + rangelist[0].start + typeCast);
                            }
                        }
                    }
                    else if (decimal.TryParse(parts[0], out start) && decimal.TryParse(parts[1], out end))
                    {
                        if (rangelist.Exists(e => e.start == start && e.end == end))
                        {
                            var rng = rangelist.Find(e => e.start == start && e.end == end);
                            //conditionlist.Add(field + " BETWEEN " + rng.start + " AND " + rng.end + typeCast);
                            conditionlist.Add("(" + field + " >= " + rng.start + " AND " + field + "<=" + rng.end + typeCast + ")");
                        }
                    }
                }
                if (conditionlist.Count > 20)
                    throw new Exception("Maximum supported ranges 20");
                return "(" + string.Join(" OR ", conditionlist) + ")";
            }

            internal string CreateComapreQuery(Configuration config)
            {
                int year = config.fromdate.Year;
                config.issingleyearcreteria = (year == config.fromdatecompare.Year &&
                                                year == config.todate.Year &&
                                                year == config.todatecompare.Year);
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(@"with baseSales as (");
                sb.AppendLine(CreateSalesQuery(config));
                sb.AppendLine(@"), compareSales as (");
                sb.AppendLine(CreateSalesQuery(config).Replace("@fromdate", "@fromdatecompare").Replace("@todate", "@todatecompare"));
                sb.AppendLine(@")");
                if ((config.Columns != null && config.Columns.Count > 0) || config.AdditionalColumns.Count > 0)
                {
                    StringBuilder sbfields = new StringBuilder();
                    StringBuilder sbconidtions = new StringBuilder();

                    for (int c = 0; c < config.Columns.Count; c++)
                    {
                        if (sbconidtions.Length > 0)
                        {
                            sbconidtions.Append(" and ");
                            sbfields.Append(",");
                        }
                        var GROUPBY = GetField(config.Columns[c]).Split('.');
                        sbconidtions.AppendLine(string.Format("comparesales.{0} = baseSales.{0}", GROUPBY.Length == 1 ? GROUPBY[0] : GROUPBY[1]));
                        sbfields.AppendLine(string.Format("coalesce(basesales.{0},comparesales.{0}) as {0}", GROUPBY.Length == 1 ? GROUPBY[0] : GROUPBY[1]));
                    }

                    for (int c = 0; c < config.AdditionalColumns.Count; c++)
                    {
                        if (sbconidtions.Length > 0)
                        {
                            sbconidtions.Append(" and ");
                            sbfields.Append(",");
                        }
                        sbconidtions.AppendLine(string.Format("comparesales.{0} = baseSales.{0}", config.AdditionalColumns[c].columnname));
                        sbfields.AppendLine(string.Format("coalesce(basesales.{0},comparesales.{0}) as {0}", config.AdditionalColumns[c].columnname));
                    }

                    sb.AppendLine(string.Format(@"select {0}, basesales.stock,basesales.cstock,basesales.scost,basesales.cost,
                    basesales.cstock ,comparesales.stock as comparestock,comparesales.cost as comparecost,comparesales.cstock as comparecstock from basesales
                        FULL OUTER JOIN comparesales on {1}", sbfields.ToString(), sbconidtions.ToString()));
                }
                else
                {
                    var GROUPBY = GetField(config, config.PrimaryCondtion, config.PrimaryRange).Split('.');
                    sb.AppendLine(string.Format(@"select coalesce(basesales.grp,comparesales.grp) as grp,basesales.stock,basesales.cstock,basesales.scost,basesales.cost,
                    basesales.cstock ,comparesales.stock as comparestock,comparesales.cost as comparecost,comparesales.cstock as comparecstock from basesales
                        FULL OUTER JOIN comparesales on comparesales.grp = basesales.grp", GROUPBY.Length == 1 ? GROUPBY[0] : GROUPBY[1]));
                }

                return sb.ToString();
            }

            internal string SalesAsOnDateAnalyserQuery(Configuration config)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(@"with baseStock as (");
                sb.AppendLine(CreateBranchStockQuery(config, true));
                sb.AppendLine(@"), removeSales as (");
                sb.AppendLine(CreateSalesRemoveQuery(config, "@todate"));
                sb.AppendLine(@")");
                var GROUPBY = GetField(config, config.PrimaryCondtion, config.PrimaryRange).Split('.');
                sb.AppendLine(string.Format(@"select baseStock.*,removeSales.stock istock,removeSales.cstock rstock,removeSales.ccost rcost from basePurchase
                        left outer join removeSales on removeSales.{0} = baseStock.{0}", GROUPBY.Length == 1 ? GROUPBY[0] : GROUPBY[1]));
                return sb.ToString();
            }

            internal string CreateSaleVsPurchaseQuery(Configuration config)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(@"with basePurchase as (");
                sb.AppendLine(CreatePurchaseQuery(config));
                sb.AppendLine(@"), compareSales as (");
                sb.AppendLine(CreateSalesQuery(config));
                sb.AppendLine(@")");

                if ((config.Columns != null && config.Columns.Count > 0) || config.AdditionalColumns.Count > 0)
                {
                    StringBuilder sbfields = new StringBuilder();
                    StringBuilder sbconidtions = new StringBuilder();

                    for (int c = 0; c < config.Columns.Count; c++)
                    {
                        if (sbconidtions.Length > 0)
                        {
                            sbconidtions.Append(" and ");
                            sbfields.Append(",");
                        }
                        var GROUPBY = GetField(config.Columns[c]).Split('.');
                        sbconidtions.AppendLine(string.Format("compareSales.{0} = basePurchase.{0}", GROUPBY.Length == 1 ? GROUPBY[0] : GROUPBY[1]));
                        sbfields.AppendLine(string.Format("coalesce(compareSales.{0},basePurchase.{0}) as {0}", GROUPBY.Length == 1 ? GROUPBY[0] : GROUPBY[1]));
                    }

                    for (int c = 0; c < config.AdditionalColumns.Count; c++)
                    {
                        if (sbconidtions.Length > 0)
                        {
                            sbconidtions.Append(" and ");
                            sbfields.Append(",");
                        }
                        sbconidtions.AppendLine(string.Format("compareSales.{0} = basePurchase.{0}", config.AdditionalColumns[c].columnname));
                        sbfields.AppendLine(string.Format("coalesce(compareSales.{0},basePurchase.{0}) as {0}", config.AdditionalColumns[c].columnname));
                    }

                    sb.AppendLine(string.Format(@"select {0}, basePurchase.*, compareSales.stock sstock,compareSales.cost salcost,compareSales.istock,compareSales.icost from basePurchase
                        FULL OUTER JOIN compareSales on {1}", sbfields.ToString(), sbconidtions.ToString()));
                }
                else
                {
                    var GROUPBY = GetField(config, config.PrimaryCondtion, config.PrimaryRange).Split('.');
                    sb.AppendLine(string.Format(@"select coalesce(compareSales.grp,basePurchase.grp) as grp,basePurchase.*, compareSales.stock sstock,
                        compareSales.cost salcost,compareSales.istock,compareSales.icost from basePurchase
                        FULL OUTER JOIN compareSales on compareSales.grp = basePurchase.grp ", GROUPBY.Length == 1 ? GROUPBY[0] : GROUPBY[1]));
                }

                //var GROUPBY = GetField(config, config.PrimaryCondtion, config.PrimaryRange).Split('.');
                //sb.AppendLine(string.Format(@"select basePurchase.*,compareSales.stock sstock,compareSales.cost salcost,compareSales.istock,compareSales.icost from basePurchase
                //        left outer join compareSales on compareSales.{0} = basePurchase.{0}", GROUPBY.Length == 1 ? GROUPBY[0] : GROUPBY[1]));
                return sb.ToString();
            }

            internal string CreateStockStressDaysQuery(Configuration config)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(@"with baseSales as (");
                sb.AppendLine(CreateSalesQuery(config));
                sb.AppendLine(@"), compareStock as (");

                if (config.locationid > 0)
                {
                    config.reporttype = Configuration.ReportType.RetailStock;
                    sb.AppendLine(CreateBranchStockQuery(config));
                }
                else
                {
                    config.reporttype = Configuration.ReportType.AllStock;
                    sb.AppendLine(CreateStockQuery(config));
                }

                sb.AppendLine(@")");
                config.reporttype = Configuration.ReportType.StockStress;
                if (config.Columns != null && config.AdditionalColumns.Count > 0)
                {
                    sb.AppendLine(string.Format(@"select baseSales.*,compareStock.stock sstock,compareStock.cost scost from baseSales
                                                 left outer join compareStock on "));

                    for (int c = 0; c < config.AdditionalColumns.Count; c++)
                    {
                        if (c > 0) sb.Append(" and ");
                        sb.AppendLine(string.Format("compareStock.{0} = baseSales.{0}", config.AdditionalColumns[c].columnname));
                    }

                    for (int c = 0; c < config.Columns.Count; c++)
                    {
                        string columnname = "";
                        switch (config.Columns[c])
                        {
                            case "Product":
                                columnname = "productid";
                                break;
                            case "Brand":
                                columnname = "brandid";
                                break;
                            case "Style":
                                columnname = "styleid";
                                break;
                            case "Pattern":
                                columnname = "patternid";
                                break;
                            case "Size":
                                columnname = "sizeid";
                                break;
                            case "Colour":
                                columnname = "colourid";
                                break;
                            case "Fit":
                                columnname = "fitid";
                                break;
                            case "Sleeve":
                                columnname = "sleeveid";
                                break;
                            case "Price Tag":
                                columnname = "pricetagid";
                                break;
                            case "Type":
                                columnname = "typeid";
                                break;
                            case "Item":
                                columnname = "itemid";
                                break;
                            case "Design":
                                columnname = "designid";
                                break;
                            case "Company":
                                columnname = "companyid";
                                break;
                            case "Productgroup":
                                columnname = "productgroupid";
                                break;
                        }
                        if (!string.IsNullOrEmpty(columnname))
                            sb.AppendLine(string.Format(" and compareStock.{0} = baseSales.{0}", columnname));
                    }
                }
                else
                {
                    var GROUPBY = GetField(config, config.PrimaryCondtion, config.PrimaryRange).Split('.');
                    sb.AppendLine(string.Format(@"select baseSales.*,compareStock.stock sstock,compareStock.cost scost from baseSales
                        left outer join compareStock on compareStock.grp = baseSales.grp", GROUPBY.Length == 1 ? GROUPBY[0] : GROUPBY[1]));
                }
                return sb.ToString();
            }

            internal string CreateConversionQuery(Configuration config)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(@"with baseSales as (");
                sb.AppendLine(CreateSalesQuery(config));
                sb.AppendLine(@"), compareStock as (");

                //config.reporttype = Configuration.ReportType.RetailStock;
                //sb.AppendLine(CreateBranchStockQuery(config).Replace("and lrinvoice.entrydate>=@fromdate and lrinvoice.entrydate<@todate", ""));
                sb.AppendLine(CreateBranchStockQueryForConversion(config).Replace("and lrinvoice.entrydate>=@fromdate and lrinvoice.entrydate<@todate", ""));

                sb.AppendLine(@"), inwardstock as (");
                sb.AppendLine(CreateInwardQuery(config));

                sb.AppendLine(@"), removestock as (");
                sb.AppendLine(CreateSalesRemoveQuery(config, "@todate"));


                sb.AppendLine(@"), removestartstock as (");
                sb.AppendLine(CreateSalesRemoveQuery(config, "@fromdate"));

                sb.AppendLine(@")");

                //config.reporttype = Configuration.ReportType.Conversion;
                if (config.Columns != null && config.Columns.Count == 0 && config.AdditionalColumns.Count > 0)
                {
                    var pfield = GetField(config, config.PrimaryCondtion, config.PrimaryRange);
                    var GROUPBY = pfield.Split('.');
                    if (string.IsNullOrEmpty(pfield))
                    {
                        GROUPBY = config.AdditionalColumns[0].columnname.Split('.');
                    }
                    sb.AppendLine(string.Format(@"select baseSales.*,compareStock.stock sstock,compareStock.buyingcost scost,
                                                 compareStock.{0} comparestockgroup,inwardstock.cstock istock,
                                                 inwardstock.ccost icost,removestock.cstock rstock,removestock.ccost rcost,
                                                 removestartstock.cstock rfstock,removestartstock.ccost rfcost from baseSales
                                                 full outer join compareStock on ", GROUPBY.Length == 1 ? GROUPBY[0] : GROUPBY[1]));

                    for (int c = 0; c < config.AdditionalColumns.Count; c++)
                    {
                        if (c > 0) sb.Append(" and ");
                        sb.AppendLine(string.Format("compareStock.{0} = baseSales.{0}", config.AdditionalColumns[c].columnname));
                    }

                    sb.AppendLine("left outer join compareStock on ");
                    for (int c = 0; c < config.AdditionalColumns.Count; c++)
                    {
                        if (c > 0) sb.Append(" and ");
                        sb.AppendLine(string.Format("inwardstock.{0} = baseSales.{0}", config.AdditionalColumns[c].columnname));
                    }

                    sb.AppendLine("left outer join removestock on ");
                    for (int c = 0; c < config.AdditionalColumns.Count; c++)
                    {
                        if (c > 0) sb.Append(" and ");
                        sb.AppendLine(string.Format("removestock.{0} = baseSales.{0}", config.AdditionalColumns[c].columnname));
                    }

                    sb.AppendLine("left outer join removestartstock on ");
                    for (int c = 0; c < config.AdditionalColumns.Count; c++)
                    {
                        if (c > 0) sb.Append(" and ");
                        sb.AppendLine(string.Format("removestartstock.{0} = baseSales.{0}", config.AdditionalColumns[c].columnname));
                    }
                }
                else
                {
                    var GROUPBY = GetField(config, config.PrimaryCondtion, config.PrimaryRange).Split('.');
                    sb.AppendLine(string.Format(@"select baseSales.*,compareStock.stock sstock,
                        compareStock.cost ccost,compareStock.{0} comparestockgroup,inwardstock.cstock istock,inwardstock.ccost icost,
                        removestock.cstock rstock,removestock.ccost rcost,removestartstock.cstock rfstock,
                        removestartstock.ccost rfcost from baseSales
                        full outer join compareStock on compareStock.{0} = baseSales.{0}
                        left outer join inwardstock on inwardstock.{0} = baseSales.{0}
                        left outer join removestock on removestock.{0} = baseSales.{0}
                        left outer join removestartstock on removestartstock.{0} = baseSales.{0}", GROUPBY.Length == 1 ? GROUPBY[0] : GROUPBY[1]));
                }
                return sb.ToString();
            }

            internal string CreateSalesPlan(UserClaim Context, Configuration config)
            {
                string applySQL = branchStockQuery.Replace("{0}", @"sum((stock.buyingprice * @interestrate /100 / 365 ) * (@current_date - stockdate::date)::int) as Interest,{0}");
                applySQL = applySQL.Replace("{1}", "and stockdate<@fromdate and salablegoods.hasstock and stock.buyingprice>0 and round((((salablegoods.price-salablegoods.discount-((stock.buyingprice * @interestrate /100 / 365 ) * (@current_date - stockdate::date)::int))/stock.buyingprice)*100-100)::numeric,2::int)<@margin {1}");
                config.fromdate = Context.DateTimeNow.AddDays((double)config.days * -1);
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(@"with totalstock as (");
                sb.AppendLine(CreateStockQuery(config, applySQL, true));

                if (config.locationid <= 0)
                {
                    sb.AppendLine(@" UNION ALL ");
                    applySQL = whStockQuery.Replace("{0}", @"sum((stock.buyingprice * @interestrate /100 / 365 ) * (@current_date - stockdate::date)::int) as Interest,{0}");
                    applySQL = applySQL.Replace("{1}", "and stockdate<@fromdate and stock.hasstock and stock.buyingprice>0 and round((((stock.price-stock.discount-((stock.buyingprice * @interestrate /100 / 365 ) * (@current_date - stockdate::date)::int))/stock.buyingprice)*100-100)::numeric,2::int)<@margin {1}");
                    var query = CreateStockQuery(config, applySQL, false);
                    if (query.Contains("salablegoods.locationid"))
                        query = query.Replace("group by salablegoods.locationid", "group by locationid").Replace("salablegoods.locationid in", config.locationid + " in ").
                            Replace("salablegoods.locationid", config.locationid + " as locationid");
                    sb.AppendLine(query);
                }

                sb.AppendLine(@")");

                var GROUPBY = GetField(config, config.PrimaryCondtion, config.PrimaryRange).Split('.');
                sb.AppendLine(string.Format(@"select {0},sum(Interest) as Interest,sum(stock) as stock,sum(cost) as cost,sum(scost) as scost from totalstock group by {0}", GROUPBY.Length == 1 ? GROUPBY[0] : GROUPBY[1]));

                return sb.ToString();
            }
        }

        public string GetQuery(Configuration _config)
        {
            OptionQueryBuilder querybuilder = new OptionQueryBuilder(this);
            if (_config.reporttype == Configuration.ReportType.WarehouseStock)
                return querybuilder.CreateWHStockQuery(_config);
            if (_config.reporttype == Configuration.ReportType.RetailStock)
                return querybuilder.CreateBranchStockQuery(_config);
            if (_config.reporttype == Configuration.ReportType.AllStock)
                return querybuilder.CreateStockQuery(_config);
            if (_config.reporttype == Configuration.ReportType.SalesCompare)
                return querybuilder.CreateComapreQuery(_config);
            if (_config.reporttype == Configuration.ReportType.Purchase)
                return querybuilder.CreatePurchaseQuery(_config);
            if (_config.reporttype == Configuration.ReportType.PurchaseReturn)
                return querybuilder.CreatePurchaseReturnQuery(_config);
            if (_config.reporttype == Configuration.ReportType.SaleVsPurchase)
                return querybuilder.CreateSaleVsPurchaseQuery(_config);
            if (_config.reporttype == Configuration.ReportType.SalesVsStockAnalyser)
                return querybuilder.CreateSalesVsStockQuery(_config);
            if (_config.reporttype == Configuration.ReportType.StockMarker)
                return querybuilder.StockMarkerQuery(_config);
            if (_config.reporttype == Configuration.ReportType.SalesAsOnDateAnalyser)
                return querybuilder.SalesAsOnDateAnalyserQuery(_config);
            if (_config.reporttype == Configuration.ReportType.StockStress)
                return querybuilder.CreateStockStressDaysQuery(_config);
            if (_config.reporttype == Configuration.ReportType.SalesPlan)
                return querybuilder.CreateSalesPlan(this.Context, _config);
            if (_config.reporttype == Configuration.ReportType.Conversion)
                return querybuilder.CreateConversionQuery(_config);
            if (_config.reporttype == Configuration.ReportType.Customers)
                return querybuilder.CreateCustomerQuery(_config);
            return querybuilder.CreateSalesQuery(_config);
        }

        //internal ResultGroup PurchasePlanGroup(PurchasePlannerDataService.PurchasePlanConfig item)
        //{
        //    var config = new AnalyserDataService.Configuration();
        //    config.reporttype = AnalyserDataService.Configuration.ReportType.Sales;
        //    config.fromdate = item.fromdate;
        //    config.todate = item.todate;
        //    config.Columns = new List<string>();
        //    config.Columns.Add("Product");
        //    config.Columns.Add("Size");
        //    config.Columns.Add("Style");
        //    config.AdditionalColumns = new List<AdditionalColumn>();
        //    config.AdditionalColumns.Add(new AdditionalColumn()
        //    {
        //        columnname ="supplierid",
        //        headername = "Supplier",
        //        key = "supplierid",
        //        PrimaryCondtion = Options.Supplier
        //    });

        //    List<AnalyserDataService.ResultItems> list = StockAnalyserReport(config).items;
        //    var result = new ResultGroup();
        //    result.Columns = new List<ResultGroup.ColumnInfo>();
        //    for (var index = 0; index < config.Columns.Count; index++)
        //    {
        //        if (!item.KeyGroup.Exists(e=>e.name != config.Columns[index]))
        //        {
        //            result.Columns.Add(new ResultGroup.ColumnInfo()
        //            {
        //                Index = index,
        //                Header = config.Columns[index]
        //            });
        //        }
        //    }

        //    result.Groups = new List<ResultGroup.ColumnInfo>();
        //    result.ResultGroups = new List<ResultGroup.ColumnInfo>();
        //    //List<decimal> defaultValues = new List<decimal>();
        //    var groupList = list.GroupBy(e =>
        //    {
        //    string[] values = new string[item.GroupColumnFieldIndex.Count];
        //    for (int i = 0; i < item.GroupColumnFieldIndex.Count; i++)
        //        values[item.GroupColumnFieldIndex[i]] = e.String[i];
        //        return string.Join(",", values);
        //    }).ToList();

        //    //if(groupList.Count>8)
        //    //{
        //    //    AnalyserDataService.ResultItems otheritem = new ResultItems();
        //    //    groupList = groupList.OrderBy(e => e.Sum(x => x.qty)).ToList();
        //    //    groupList = groupList.Skip(8).ToList();
        //    //    otheritem.qty = groupList.Sum(e => e.Sum(x => x.qty));
        //    //    otheritem.value= groupList.Sum(e => e.Sum(x => x.value));
        //    //    foreach(var gitem in groupList)
        //    //    {
        //    //        list.RemoveAll(e => e.String[item.GroupColumnFieldIndex] == gitem.Key);
        //    //    }
        //    //    groupList = list.GroupBy(e => e.String[item.GroupColumnFieldIndex]).ToList();
        //    //}

        //    foreach (var column in groupList)
        //    {
        //        result.Groups.Add(new ResultGroup.ColumnInfo()
        //        {
        //            Index = result.Groups.Count,
        //            Header = column.Key
        //        });
        //        //defaultValues.Add(0);
        //        //defaultValues.Add(0);
        //    }

        //    list.ForEach(e =>
        //    {
        //        var tList = e.Values.ToList();
        //        for (int i = item.GroupColumnFieldIndex.Count -1; i >=0; i--)
        //            tList.RemoveAt(item.GroupColumnFieldIndex[i]);
        //        e.GroupBy = String.Join<long>("", tList);
        //    });

        //    result.Items = new List<ResultGroupItems>();
        //    foreach (var gitem in list.GroupBy(e => e.GroupBy))
        //    {
        //        ResultGroupItems rgi = new ResultGroupItems();
        //        List<string> dispValues = new List<string>(gitem.First().String);
        //        for (int i = item.GroupColumnFieldIndex.Count - 1; i >= 0; i--)
        //            dispValues.RemoveAt(item.GroupColumnFieldIndex[i]);
        //        rgi.String = dispValues;
        //        rgi.GroupValues = new ResultItems[groupList.Count];
        //        foreach (var kitem in gitem)
        //        {
        //            //var stockIndex = result.Groups.Find(e => e.Header == kitem.String[item.GroupColumnFieldIndex]).Index;
        //            //rgi.GroupValues[stockIndex] = kitem;
        //        }
        //        result.Items.Add(rgi);
        //    }

        //    //foreach (var gitem in result.Groups)
        //    //{
        //    //    result.ResultGroups.Add(new ResultGroup.ColumnInfo()
        //    //    {
        //    //        Index = result.ResultGroups.Count,
        //    //        Header = gitem.Header
        //    //    });
        //    //    result.ResultGroups.Add(new ResultGroup.ColumnInfo()
        //    //    {
        //    //        Index = result.ResultGroups.Count,
        //    //        Header = gitem.Header
        //    //    });
        //    //}
        //    return result;
        //}

        private Condtion.Range GetRange(string config)
        {
            var range = new Condtion.Range();
            string[] prices = config.Split('-');
            decimal d;
            if (decimal.TryParse(prices[0], out d))
                range.start = d;
            if (decimal.TryParse(prices[1], out d))
                range.incrementor = d;
            if (decimal.TryParse(prices[2], out d))
                range.end = d;
            return range;
        }

        internal ResultGroup PurchasePlan(PlanningDataSource.PurchasePlanConfig config)
        {
            var item = new AnalyserDataService.Configuration();
            item.reporttype = AnalyserDataService.Configuration.ReportType.StockStress;
            item.fromdate = config.fromdate;
            item.todate = config.todate;
            item.Columns = config.columns;
            item.SecondaryCondtions = new List<Condtion>();

            item.PrimaryCondtion = config.primarycolumn;
            if (config.primarycolumn == Options.PurchaseRate_Range)
                item.PrimaryRange = GetRange(config.pricerange);


            item.SecondaryCondtions.Add(new Condtion()
            {
                option = config.primarycolumn,
                typeName = config.primarycolumn.ToString(),
                ShowColumn = true,
                range = config.primarycolumn == Options.PurchaseRate_Range ? GetRange(config.pricerange) : null
            });

            item.SecondaryCondtions.Add(new Condtion()
            {
                option = config.secondarycolumn,
                typeName = config.secondarycolumn.ToString(),
                ShowColumn = true,
                range = config.secondarycolumn == Options.PurchaseRate_Range ? GetRange(config.pricerange) : null
            });

            if (config.productid > 0)
            {
                item.SecondaryCondtions.Add(new Condtion()
                {
                    option = Options.Product,
                    longList = new List<long>(),
                    type = Condtion.CondtionType.Long
                });
                item.SecondaryCondtions[item.SecondaryCondtions.Count - 1].longList.Add(config.productid);
            }

            if (config.supplierid > 0)
            {
                item.SecondaryCondtions.Add(new Condtion()
                {
                    option = Options.Supplier,
                    longList = new List<long>(),
                    type = Condtion.CondtionType.Long
                });
                item.SecondaryCondtions[item.SecondaryCondtions.Count - 1].longList.Add(config.supplierid);
            }

            if (config.productgroupid > 0)
            {
                item.SecondaryCondtions.Add(new Condtion()
                {
                    option = Options.ProductGroup,
                    longList = new List<long>(),
                    type = Condtion.CondtionType.Long
                });
                item.SecondaryCondtions[item.SecondaryCondtions.Count - 1].longList.Add(config.productgroupid);
            }

            if (config.companyid > 0)
            {
                item.SecondaryCondtions.Add(new Condtion()
                {
                    option = Options.Company,
                    longList = new List<long>(),
                    type = Condtion.CondtionType.Long
                });
                item.SecondaryCondtions[item.SecondaryCondtions.Count - 1].longList.Add(config.companyid);
            }

            if (config.suppliercityid > 0)
            {
                item.SecondaryCondtions.Add(new Condtion()
                {
                    option = Options.Supplier_City,
                    longList = new List<long>(),
                    type = Condtion.CondtionType.Long
                });
                item.SecondaryCondtions[item.SecondaryCondtions.Count - 1].longList.Add(config.suppliercityid);
            }

            if (config.brandid > 0)
            {
                item.SecondaryCondtions.Add(new Condtion()
                {
                    option = Options.Brand,
                    longList = new List<long>(),
                    type = Condtion.CondtionType.Long
                });
                item.SecondaryCondtions[item.SecondaryCondtions.Count - 1].longList.Add(config.brandid);
            }

            if (config.sectionid > 0)
            {
                item.SecondaryCondtions.Add(new Condtion()
                {
                    option = Options.Section,
                    longList = new List<long>(),
                    type = Condtion.CondtionType.Long
                });
                item.SecondaryCondtions[item.SecondaryCondtions.Count - 1].longList.Add(config.sectionid);
            }

            var result = new ResultGroup();
            item.reporttype = AnalyserDataService.Configuration.ReportType.StockStress;
            var saledata = StockAnalyserReport(item);
            List<AnalyserDataService.ResultItems> salelist = saledata.items;

            List<Priority> plist = null;
            if (config.primarycolumn != Options.PurchaseRate_Range)
            {
                plist = GetPriority(config);
                plist.Sort(delegate (Priority a, Priority b)
                {
                    return b.pqty.CompareTo(a.pqty);
                });

                //if (plist.Count > 10)
                //{
                //    int avg = plist.Count / 4;
                //    List<List<AnalyserDataService.Priority>> buckets = new List<List<Priority>>();
                //    Queue<AnalyserDataService.Priority> queue = new Queue<Priority>(plist);
                //    for (int b = 0; b < 4; b++)
                //    {
                //        List<AnalyserDataService.Priority> l = new List<Priority>();
                //        buckets.Add(l);
                //        for (int q = 0; q < avg; q++)
                //        {
                //            l.Add(queue.Dequeue());
                //        }
                //        while (b == 3 && queue.Count > 0)
                //            l.Add(queue.Dequeue());

                //        UpdateRation(l);
                //    }
                //}
                UpdateRation(plist);
                result.EnablePriority = config.primarycolumn == Options.Supplier;
            }

            result.Groups = new List<ResultGroup.GroupInfo>();
            result.ResultGroups = new List<ResultGroup.ColumnInfo>();

            item.GroupColumnFieldIndex = saledata.additionalcolumns.Find(e => e.option == config.secondarycolumn).index;
            item.GroupColumn = saledata.additionalcolumns.Find(e => e.option == config.secondarycolumn).headername;


            int tdays = (int)config.todate.Subtract(config.fromdate.Date).TotalDays;


            salelist.ForEach(e =>
            {
                if (tdays > 0 && (e.qty / tdays) > 0)
                {
                    var topup = (e.qty * config.markup) / 100;
                    var tsaleqty = e.qty + topup;

                    e.cqty = Math.Round(tsaleqty / tdays * config.plandays, 0);
                }

                var tList = e.Values.ToList();
                tList.RemoveAt(item.GroupColumnFieldIndex);
                e.GroupBy = String.Join<long>("", tList);
            });

            salelist.RemoveAll(e => e.GroupBy == "" || e.String[item.GroupColumnFieldIndex] == "");
            var groupList = salelist.GroupBy(e => e.String[item.GroupColumnFieldIndex]).ToList();

            foreach (var column in groupList)
            {
                result.Groups.Add(new ResultGroup.GroupInfo()
                {
                    Index = result.Groups.Count,
                    Header = column.Key,
                    Value = column.First().Values[item.GroupColumnFieldIndex]
                });
            }

            //purchaselist.ForEach(e =>
            //{
            //    var tList = e.Values.ToList();
            //    tList.RemoveAt(item.GroupColumnFieldIndex);
            //    e.GroupBy = String.Join<long>("", tList);
            //});
            result.Request = config;
            result.Configuration = Newtonsoft.Json.JsonConvert.DeserializeObject<Configuration>(Newtonsoft.Json.JsonConvert.SerializeObject(item));
            result.Items = new List<ResultGroupItems>();
            int days = config.todate.Subtract(config.fromdate).Days;
            foreach (var gitem in salelist.GroupBy(e => e.GroupBy))
            {
                ResultGroupItems rgi = new ResultGroupItems();
                List<string> dispValues = new List<string>(gitem.First().String);
                dispValues.RemoveAt(item.GroupColumnFieldIndex);
                rgi.String = dispValues;
                var identityValues = new List<long>(gitem.First().Values);
                rgi.Values = identityValues;
                identityValues.RemoveAt(item.GroupColumnFieldIndex);
                rgi.GroupValues = new ResultItems[groupList.Count];

                if (result.EnablePriority)
                {
                    var supplierIndex = saledata.additionalcolumns.Find(e => e.option == config.primarycolumn).index;
                    var id = gitem.First().Values[supplierIndex];
                    if (plist.Exists(e => e.id == id))
                    {
                        rgi.priority = plist.Find(e => e.id == id);
                    }
                    else
                    {
                        rgi.priority = new Priority();
                    }
                }

                //var purchaseItem = purchaselist.FindAll(e => e.GroupBy == gitem.Key);

                var tpqty = gitem.Sum(e => e.qty);
                foreach (var kitem in gitem)
                {
                    var stockIndex = result.Groups.Find(e => e.Header == kitem.String[item.GroupColumnFieldIndex]).Index;
                    rgi.GroupValues[stockIndex] = kitem;

                }
                result.Items.Add(rgi);
            }

            result.Columns = new List<ResultGroup.ColumnInfo>();
            result.Columns.Add(new ResultGroup.ColumnInfo()
            {
                Header = item.AdditionalColumns.Find(e => e.option == config.primarycolumn).headername,
                Index = item.AdditionalColumns.Find(e => e.option == config.primarycolumn).index
            });

            for (var index = 0; index < item.Columns.Count; index++)
            {
                if (item.GroupColumn != item.Columns[index])
                {
                    result.Columns.Add(new ResultGroup.ColumnInfo()
                    {
                        Index = index,
                        Header = item.Columns[index]
                    });
                }
            }
            item.AdditionalColumns.Clear();

            if (item.SecondaryCondtions[1].range != null)
            {
                result.Groups = result.Groups.OrderBy(e =>
                {
                    decimal d;
                    string[] parts = e.Header.Split('-');
                    if (parts.Length > 1)
                        decimal.TryParse(parts[1], out d);
                    else
                    {
                        decimal.TryParse(parts[0], out d);
                        d++;
                    }
                    return d;
                }).ToList();
            }
            else
            {
                result.Groups = result.Groups.OrderBy(e => e.Header).ToList();
            }

            if (item.SecondaryCondtions[0].range != null)
            {
                result.Items = result.Items.OrderBy(e =>
                {
                    decimal d;
                    string[] parts = e.String[0].Split('-');
                    if (parts.Length > 1)
                        decimal.TryParse(parts[1], out d);
                    else
                    {
                        decimal.TryParse(parts[0], out d);
                        d++;
                    }
                    return d;
                }).ToList();
            }
            else
            {
                //result.Items = result.Items.OrderBy(e => e.String[0]).ToList();

                result.Items = result.Items.OrderByDescending(e => e.priority != null ? e.priority.salesvspsratio : 0).ToList();
            }

            return result;
        }

        private void UpdateRation(List<Priority> l)
        {
            var tpqty = l.Sum(e => e.pqty);
            var tsqty = l.Sum(e => e.sqty);
            l.ForEach(e =>
            {
                if (tpqty > 0)
                    e.pratio = (e.pqty / tpqty * 100);
                if (tsqty > 0)
                    e.sratio = e.sqty / tsqty * 100;
                if (e.pqty > 0)
                    e.psratio = e.sqty / e.pqty * 100;
                e.salesvspsratio = e.sqty * e.psratio / 100;
            });

            var tsalesvspsratio = l.Sum(e => e.salesvspsratio);
            l.ForEach(e => e.recommend = tsalesvspsratio == 0 ? 0 : (e.salesvspsratio / tsalesvspsratio * 100));
            //var tpqtyratio = l.Sum(e => e.pqtyratio);
            //if (tpqtyratio > 0)
            //{
            //    l.ForEach(e =>
            //    {
            //        e.pratio = e.pqtyratio / tpqtyratio * 100;
            //        e.tratio = e.pratio + e.pqtyratio;
            //    });
            //}

            //var tratio = l.Sum(e => e.tratio);
            //if (tratio>0)
            //{
            //    l.ForEach(e =>
            //    {
            //        e.percentage = e.tratio / tratio * 100;
            //    });
            //}
        }

        internal ResultGroup StockAnalyserGroup(Configuration item)
        {
            List<AnalyserDataService.ResultItems> list = StockAnalyserReport(item).items;
            var result = new ResultGroup();
            result.Columns = new List<ResultGroup.ColumnInfo>();
            for (var index = 0; index < item.Columns.Count; index++)
            {
                if (item.GroupColumn != item.Columns[index])
                {
                    result.Columns.Add(new ResultGroup.ColumnInfo()
                    {
                        Index = index,
                        Header = item.Columns[index]
                    });
                }
            }

            for (var index = 0; index < item.AdditionalColumns.Count; index++)
            {
                if (item.GroupColumn.ToUpper() != item.AdditionalColumns[index].columnname &&
                    !item.GroupColumn.ToUpper().EndsWith(item.AdditionalColumns[index].headername))
                {
                    result.Columns.Add(new ResultGroup.ColumnInfo()
                    {
                        Index = item.GroupColumnFieldIndex <= item.AdditionalColumns[index].index ? item.AdditionalColumns[index].index - 1 : item.AdditionalColumns[index].index,
                        Header = item.AdditionalColumns[index].headername
                    });
                }
            }

            result.Groups = new List<ResultGroup.GroupInfo>();
            result.ResultGroups = new List<ResultGroup.ColumnInfo>();
            //List<decimal> defaultValues = new List<decimal>();
            list = list.OrderBy(e => e.String[item.GroupColumnFieldIndex]).ToList();
            var groupList = list.GroupBy(e => e.String[item.GroupColumnFieldIndex]).ToList();

            //if(groupList.Count>8)
            //{
            //    AnalyserDataService.ResultItems otheritem = new ResultItems();
            //    groupList = groupList.OrderBy(e => e.Sum(x => x.qty)).ToList();
            //    groupList = groupList.Skip(8).ToList();
            //    otheritem.qty = groupList.Sum(e => e.Sum(x => x.qty));
            //    otheritem.value= groupList.Sum(e => e.Sum(x => x.value));
            //    foreach(var gitem in groupList)
            //    {
            //        list.RemoveAll(e => e.String[item.GroupColumnFieldIndex] == gitem.Key);
            //    }
            //    groupList = list.GroupBy(e => e.String[item.GroupColumnFieldIndex]).ToList();
            //}

            foreach (var column in groupList)
            {
                result.Groups.Add(new ResultGroup.GroupInfo()
                {
                    Index = result.Groups.Count,
                    Header = column.Key,
                    Value = column.First().Values[item.GroupColumnFieldIndex]
                });
                //defaultValues.Add(0);
                //defaultValues.Add(0);
            }

            list.ForEach(e =>
            {
                var tList = e.String.ToList();
                tList.RemoveAt(item.GroupColumnFieldIndex);
                e.GroupBy = String.Join<string>("", tList);
            });
            result.Items = new List<ResultGroupItems>();
            foreach (var gitem in list.GroupBy(e => e.GroupBy))
            {
                ResultGroupItems rgi = new ResultGroupItems();
                List<string> dispValues = new List<string>(gitem.First().String);
                dispValues.RemoveAt(item.GroupColumnFieldIndex);
                rgi.String = dispValues;
                //rgi.GroupValues = new List<decimal>(defaultValues);
                rgi.GroupValues = new ResultItems[groupList.Count];
                foreach (var kitem in gitem)
                {
                    var stockIndex = result.Groups.Find(e => e.Header == kitem.String[item.GroupColumnFieldIndex]).Index;
                    rgi.GroupValues[stockIndex] = kitem;
                    //rgi.GroupValues[(stockIndex * 2)] = kitem.qty;
                    //rgi.GroupValues[(stockIndex * 2) + 1] = kitem.value;
                }
                result.Items.Add(rgi);
            }

            if (item.GroupColumn.ToUpper().Contains("RANGE"))
            {
                result.Groups.Sort(delegate (ResultGroup.GroupInfo a, ResultGroup.GroupInfo b)
                {
                    decimal avalue = 0;
                    decimal bvalue = 0;
                    decimal.TryParse(a.Header.IndexOf('-') > 0 ? a.Header.Split('-')[0] : a.Header, out avalue);
                    decimal.TryParse(b.Header.IndexOf('-') > 0 ? b.Header.Split('-')[0] : b.Header, out bvalue);
                    return avalue.CompareTo(bvalue);
                });
            }
            if (item.GroupColumn.ToUpper().Contains("SALE MONTH"))
            {
                result.Groups.Sort(delegate (ResultGroup.GroupInfo a, ResultGroup.GroupInfo b)
                {
                    long avalue = 0;
                    long bvalue = 0;
                    long.TryParse(a.Value.ToString(), out avalue);
                    long.TryParse(b.Value.ToString(), out bvalue);
                    return avalue.CompareTo(bvalue);
                });
            }

            foreach (var gitem in result.Groups)
            {
                result.ResultGroups.Add(new ResultGroup.ColumnInfo()
                {
                    Index = result.ResultGroups.Count,
                    Header = gitem.Header
                });
                result.ResultGroups.Add(new ResultGroup.ColumnInfo()
                {
                    Index = result.ResultGroups.Count,
                    Header = gitem.Header
                });
            }
            return result;
        }

        public List<Priority> GetPriority(PlanningDataSource.PurchasePlanConfig config)
        {
            OptionQueryBuilder ob = new OptionQueryBuilder(this);
            string SQL = ob.CreatePriorityQuery(config);

            List<Priority> lstitems = new List<Priority>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                NpgsqlCommand command = null;
                QueryBuilder qb = new QueryBuilder();
                qb.CurrentQuery = SQL;
                command = qb.GetCommand(conn);
                command.CommandTimeout = 10000;
                command.Parameters.Add(new NpgsqlParameter("fromdate", config.todate.AddYears(-1).Date));
                command.Parameters.Add(new NpgsqlParameter("todate", config.todate.AddDays(1).Date));
                command.Parameters.Add(new NpgsqlParameter("productid", config.productid));
                command.Parameters.Add(new NpgsqlParameter("current_date", DateTimeNow.Date));
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Priority priority = new Priority();
                        priority.id = reader["item"] == DBNull.Value ? 0 : Convert.ToInt64(reader["item"]);
                        priority.pqty = reader["bqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["bqty"]);
                        priority.sqty = reader["sqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["sqty"]);
                        lstitems.Add(priority);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null)
                    conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, config);
            }
            return lstitems;
        }
        public ResultItemGroups StockAnalyserReport(Configuration _config)
        {
            _config.isreport = true;
            string SQL = GetQuery(_config);
            if (_config.fromdate.Year < 2000 || _config.todate.Year < 2000 ||
                _config.reporttype == Configuration.ReportType.StockStress ||
                _config.reporttype == Configuration.ReportType.SalesVsStockAnalyser)
            {
                SQL = SQL.Replace("and lrinvoice.entrydate>=@fromdate and lrinvoice.entrydate<@todate", "");
                SQL = SQL.Replace("lrinvoicereturn.returndate>=@fromdate and lrinvoicereturn.returndate<@todate", "");
            }

            List<AnalyserDataService.ResultItems> lstitems = new List<ResultItems>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                NpgsqlCommand command = null;
                int totalColumn = _config.AdditionalColumns.Count() + (_config.Columns == null ? 0 : _config.Columns.Count);
                if (!_config.AdditionalColumns.Exists(e => e.columnname == _config.GroupColumn) &&
                    !_config.Columns.Exists(e => e == _config.GroupColumn))
                    totalColumn++;

                if (!_config.AdditionalColumns.Exists(e => e.columnname == _config.PrimaryCondtion.ToString()) &&
                    !_config.Columns.Exists(e => e == _config.PrimaryCondtion.ToString()))
                    totalColumn++;

                QueryBuilder qb = new QueryBuilder();
                qb.CurrentQuery = SQL;
                if (Services.ConfigManager.Instance(Context.OrganizationID).Config.analyser_stock_removeaqty)
                    qb.CurrentQuery = SQL.Replace("salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty)", "salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty+salablegoods.aqty)");

                if (_config.PrimaryCondtion != Options.BillType)
                    qb.CurrentQuery = SQL.Replace("and bill.istranfser = false", "");

                if (_config.SecondaryCondtions != null &&
                    _config.SecondaryCondtions.Count > 0 &&
                    _config.SecondaryCondtions.Any(x => x.option == Options.BillType))
                {
                    qb.CurrentQuery = SQL.Replace("and bill.istranfser = false", "");
                }

                var noofdays = _config.todate.Date.Subtract(_config.fromdate.Date).Days;
                command = qb.GetCommand(conn);
                command.CommandTimeout = 10000;
                //command.CommandText = "/*Running By "+Context.UserID + "/"+Context.EmployeeName+ "*/\r\n" + command.CommandText;

                //if (_config.reporttype == Configuration.ReportType.Sales || _config.reporttype == Configuration.ReportType.SalesCompare
                //    || _config.reporttype == Configuration.ReportType.Purchase || _config.reporttype == Configuration.ReportType.SaleVsPurchase || _config.reporttype == Configuration.ReportType.CoverDays)
                {
                    command.Parameters.Add(new NpgsqlParameter("fromdate", _config.fromdate.Date));
                    command.Parameters.Add(new NpgsqlParameter("todate", _config.todate.AddDays(1).Date));
                    command.Parameters.Add(new NpgsqlParameter("fromdatecompare", _config.fromdatecompare));
                    command.Parameters.Add(new NpgsqlParameter("todatecompare", _config.todatecompare.AddDays(1).Date));
                    command.Parameters.Add(new NpgsqlParameter("locationid", _config.locationid));
                    command.Parameters.Add(new NpgsqlParameter("margin", _config.margin));
                    command.Parameters.Add(new NpgsqlParameter("interestrate", _config.interestrate));
                    command.Parameters.Add(new NpgsqlParameter("current_date", DateTimeNow.Date));
                }

                if (_config.todate.Subtract(_config.fromdate).TotalDays > 31)
                {
                    WebAPI.Common.Logger.Instance(Context.OrganizationID).PerformanceLog("DBPerformance : " +
                        (String.IsNullOrEmpty(HttpRuntime.AppDomainAppVirtualPath) ? "" : HttpRuntime.AppDomainAppVirtualPath),
                    WebAPI.Common.Logger.LogType.Info, Environment.StackTrace, GetSQL(command), Context.UserID, _config.locationid);
                }

                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    if (stopwatch.Elapsed.TotalSeconds > 30)
                    {
                        WebAPI.Common.Logger.Instance(Context.OrganizationID).PerformanceLog("DBPerformance :" +
                            stopwatch.Elapsed.TotalSeconds,
                            WebAPI.Common.Logger.LogType.Info, Environment.StackTrace, GetSQL(command), Context.UserID,
                            _config.locationid);
                    }
                    while (reader.Read())
                    {
                        AnalyserDataService.ResultItems oitem = new ResultItems();
                        oitem.String = new string[totalColumn];
                        oitem.Values = new long[totalColumn];
                        int index = 0;
                        int usedIndex = 0;
                        foreach (var colmn in _config.Columns)
                        {
                            switch (colmn)
                            {
                                case "Product":
                                    oitem.Values[index] = reader["Productid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["Productid"]);
                                    break;
                                case "Brand":
                                    oitem.Values[index] = reader["brandid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["brandid"]);
                                    break;
                                case "Style":
                                    oitem.Values[index] = reader["styleid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["styleid"]);
                                    break;
                                case "Pattern":
                                    oitem.Values[index] = reader["patternid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["patternid"]);
                                    break;
                                case "Size":
                                    oitem.Values[index] = reader["sizeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sizeid"]);
                                    break;
                                case "Colour":
                                    oitem.Values[index] = reader["colourid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["colourid"]);
                                    break;
                                case "Fit":
                                    oitem.Values[index] = reader["fitid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["fitid"]);
                                    break;
                                case "Sleeve":
                                    oitem.Values[index] = reader["sleeveid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sleeveid"]);
                                    break;
                                case "Price Tag":
                                    oitem.Values[index] = reader["pricetagid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["pricetagid"]);
                                    break;
                                case "Type":
                                    oitem.Values[index] = reader["typeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["typeid"]);
                                    break;
                                case "Item":
                                    oitem.Values[index] = reader["itemid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["itemid"]);
                                    break;
                                case "Supplier City":
                                    oitem.Values[index] = reader["cityid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["cityid"]);
                                    break;
                                case "Design":
                                    oitem.Values[index] = 0;
                                    oitem.String[index] = reader["designid"] == DBNull.Value ? "" : reader["designid"].ToString();
                                    break;
                                case "ImageID":
                                    oitem.Values[index] = 0;
                                    oitem.String[index] = reader["imageid"] == DBNull.Value ? "" : reader["imageid"].ToString();
                                    break;
                                case "Barcode":
                                    oitem.Values[index] = 0;
                                    oitem.String[index] = reader["barcode"] == DBNull.Value ? "" : reader["barcode"].ToString();
                                    break;
                                default:
                                    usedIndex--;
                                    break;
                            }
                            index++;
                            usedIndex++;
                        }

                        usedIndex = 0;
                        foreach (var column in _config.AdditionalColumns)
                        {
                            if (column.hasreference)
                            {
                                oitem.Values[column.index + usedIndex] = reader[column.columnname] == DBNull.Value ? 0 : Convert.ToInt64(reader[column.columnname]);
                            }
                            else
                            {
                                oitem.Values[column.index + usedIndex] = 0;
                                if (column.isdate && _config.issingleyearcreteria)
                                    oitem.String[column.index + usedIndex] = reader[column.columnname] == DBNull.Value ? "" : reader[column.columnname].ToString();
                                else if (column.isdate)
                                    oitem.String[column.index + usedIndex] = reader[column.columnname] == DBNull.Value ? "" : ((DateTime)reader[column.columnname]).ToString("dd-MM-yyyy");
                                else
                                    oitem.String[column.index + usedIndex] = reader[column.columnname] == DBNull.Value ? "" : reader[column.columnname].ToString();
                            }
                        }

                        if (_config.reporttype == Configuration.ReportType.StockMarker)
                        {
                            oitem.qty = reader["stockqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["stockqty"]);
                            oitem.value = reader["stockcost"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["stockcost"]);
                            oitem.sqty = reader["stocksalevalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["stocksalevalue"]);

                            oitem.cqty = reader["saleqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["saleqty"]);
                            oitem.cvalue = reader["salecost"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["salecost"]);
                            oitem.svalue = reader["salevalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["salevalue"]);

                            oitem.aqty = reader["smqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["smqty"]);
                            oitem.iqty = reader["smsalevalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["smsalevalue"]);
                            oitem.ivalue = reader["smcost"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["smcost"]);

                        }
                        else
                        {
                            oitem.qty = reader["stock"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["stock"]);
                            oitem.value = reader["cost"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["cost"]);
                        }

                        if (_config.reporttype == Configuration.ReportType.SalesCompare)
                        {
                            oitem.cqty = reader["comparestock"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["comparestock"]);
                            oitem.cvalue = reader["comparecost"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["comparecost"]);

                            oitem.iqty = reader["cstock"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["cstock"]);
                            oitem.ivalue = reader["comparecstock"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["comparecstock"]);
                        }
                        if (_config.reporttype == Configuration.ReportType.SalesVsStockAnalyser)
                        {
                            oitem.iqty = reader["comparestock"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["comparestock"]);
                            oitem.ivalue = reader["comparecost"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["comparecost"]);
                            oitem.avalue = reader["comparesalesprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["comparesalesprice"]);
                        }
                        if (_config.reporttype == Configuration.ReportType.Conversion)
                        {
                            oitem.cvalue = reader["cstock"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["cstock"]);

                            //current stock
                            oitem.aqty = reader["sstock"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["sstock"]);
                            oitem.avalue = reader["ccost"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["ccost"]);

                            var rqty = reader["rfstock"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["rfstock"]);
                            var rvalue = reader["rfcost"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["rfcost"]);

                            oitem.sqty = reader["sstock"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["sstock"]);
                            oitem.svalue = reader["ccost"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["ccost"]);

                            oitem.iqty = reader["istock"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["istock"]);
                            oitem.ivalue = reader["icost"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["icost"]);

                            //end of day stock
                            oitem.sqty += (reader["rstock"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["rstock"]));
                            oitem.svalue += (reader["rcost"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["rcost"]));

                            //start of day stock
                            rqty += reader["rfstock"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["rfstock"]);
                            rvalue += reader["rfcost"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["rfcost"]);

                            oitem.oqty = oitem.aqty - oitem.iqty + (reader["rfstock"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["rfstock"])) +
                                (reader["rstock"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["rstock"]));

                            oitem.ovalue = oitem.avalue - oitem.ivalue + (reader["rfcost"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["rfcost"])) +
                                (reader["rcost"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["rcost"]));

                            var avaeragestockqty = rqty + oitem.sqty + oitem.aqty;
                            var avaeragestockvalue = rvalue + oitem.svalue + oitem.avalue;
                            if (avaeragestockvalue > 0)
                            {
                                oitem.cqty = (avaeragestockvalue / 3);
                            }
                            //if (oitem.cvalue > 0 &&
                            //    noofdays > 0 &&
                            //    oitem.svalue > 0)
                            //    oitem.cqty = Math.Round(oitem.cvalue / noofdays * 365 / oitem.svalue, 2);
                        }


                        if (_config.reporttype == Configuration.ReportType.AllStock || _config.reporttype == Configuration.ReportType.WarehouseStock || _config.reporttype == Configuration.ReportType.RetailStock)
                        {
                            oitem.svalue = reader["scost"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["scost"]);
                            oitem.iqty = reader["istock"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["istock"]);
                            oitem.ivalue = reader["icost"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["icost"]);
                            oitem.avalue = reader["avalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["avalue"]);
                            oitem.buyingcost = reader["buyingcost"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["buyingcost"]);
                            oitem.sellingprice = reader["sellingprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["sellingprice"]);
                            oitem.cqty = reader["adjqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["adjqty"]);
                            oitem.cvalue = reader["adjval"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["adjval"]);
                        }

                        if (_config.reporttype == Configuration.ReportType.Purchase)
                        {
                            //available sock
                            //(stock.qty-stock.returnqty) * stock.ibuyingprice::numeric
                            oitem.cvalue = reader["cvalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["cvalue"]);
                            //salablestock.available+(stock.qty-stock.returnqty-stock.transfered)
                            oitem.cqty = reader["cqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["cqty"]);
                            //(salablestock.available+(stock.qty-stock.returnqty-stock.transfered)) * stock.buyingprice::numeric
                            oitem.iqty = reader["iqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["iqty"]);
                            //(salablestock.available+(stock.qty-stock.returnqty-stock.transfered)) * stock.ibuyingprice::numeric)
                            oitem.ivalue = reader["ivalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["ivalue"]);
                            //(salablestock.available+(stock.qty-stock.returnqty-stock.transfered)) * (stock.price-stock.discount)::numeric)
                            oitem.svalue = reader["svalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["svalue"]);
                        }

                        if (_config.reporttype == Configuration.ReportType.PurchaseReturn)
                        {
                            //sum(lrinvoicereturnitems.qty * (lrinvoicereturnitems.rate + tax))
                            oitem.cvalue = reader["cvalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["cvalue"]);
                            //salablestock.available+(stock.qty-stock.returnqty-stock.transfered)
                            oitem.cqty = reader["cqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["cqty"]);
                            //(salablestock.available+(stock.qty-stock.returnqty-stock.transfered)) * stock.buyingprice::numeric
                            oitem.iqty = reader["iqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["iqty"]);
                            //(salablestock.available+(stock.qty-stock.returnqty-stock.transfered)) * stock.ibuyingprice::numeric)
                            oitem.ivalue = reader["ivalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["ivalue"]);
                            //(salablestock.available+(stock.qty-stock.returnqty-stock.transfered)) * (stock.price-stock.discount)::numeric)
                            oitem.svalue = reader["svalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["svalue"]);
                        }

                        if (_config.reporttype == Configuration.ReportType.SaleVsPurchase)
                        {
                            oitem.sqty = reader["sstock"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["sstock"]);
                            oitem.svalue = reader["salcost"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["salcost"]);
                        }

                        if (_config.reporttype == Configuration.ReportType.StockStress)
                        {
                            oitem.sqty = reader["sstock"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["sstock"]);
                            oitem.svalue = reader["scost"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["scost"]);
                        }

                        if (_config.reporttype == Configuration.ReportType.SaleVsPurchase)
                        {
                            oitem.iqty = reader["istock"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["istock"]);
                            oitem.ivalue = reader["icost"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["icost"]);

                            // Replaced buying price to ibuying price only on Sales Vs Purchase 
                            oitem.value = reader["cvalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["cvalue"]);
                            //
                            //oitem.cqty = reader["cqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["cqty"]);
                            //oitem.cvalue = reader["ivalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["ivalue"]);

                            oitem.rqty = reader["rqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["rqty"]);
                            oitem.rvalue = reader["rvalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["rvalue"]);

                            //if (oitem.rqty > 0)
                            //{
                            //    oitem.qty = oitem.qty - oitem.rqty;
                            //    oitem.value = oitem.value - oitem.rvalue;
                            //}
                            //oitem.cqty = oitem.qty - oitem.iqty;
                            //oitem.cvalue = oitem.value - oitem.ivalue;

                            oitem.cqty = oitem.qty - oitem.iqty - oitem.rqty;
                            oitem.cvalue = oitem.value - oitem.ivalue - oitem.rvalue;
                        }

                        if (_config.reporttype == Configuration.ReportType.SalesPlan)
                        {
                            oitem.svalue = reader["scost"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["scost"]);
                            oitem.cvalue = reader["interest"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["interest"]);
                        }
                        if (_config.reporttype == Configuration.ReportType.Sales ||
                            _config.reporttype == Configuration.ReportType.SalesVsStockAnalyser ||
                            _config.reporttype == Configuration.ReportType.SalesOnApproval)
                        {
                            oitem.cvalue = reader["cstock"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["cstock"]);
                            oitem.svalue = reader["scost"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["scost"]);
                        }

                        if (_config.reporttype == Configuration.ReportType.Customers)
                        {
                            //sum(billitems.qty * stock.ibuyingprice::numeric) as cstock, 
                            oitem.cvalue = reader["cstock"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["cstock"]);
                            //sum((billitems.qty * billitems.additionaldiscount::numeric) + billitems.schemediscount) scost,
                            oitem.svalue = reader["scost"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["scost"]);
                            //sum(billitems.qty * stock.buyingprice::numeric) as ivalue, 
                            oitem.ivalue = reader["ivalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["ivalue"]);
                            //sum((billitems.receivable - billitems.tax) / billitems.qty) as iqty from bill
                            oitem.iqty = reader["iqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["iqty"]);
                        }

                        if (_config.reporttype == Configuration.ReportType.Sales || _config.reporttype == Configuration.ReportType.SalesOnApproval)
                        {
                            oitem.sellingprice = reader["margin"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["margin"]);
                            //sum(billitems.qty * stock.buyingprice::numeric) as ivalue, 
                            oitem.ivalue = reader["ivalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["ivalue"]);
                            //sum((billitems.receivable - billitems.tax) / billitems.qty) as iqty from bill
                            oitem.iqty = reader["iqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["iqty"]);
                        }
                        lstitems.Add(oitem);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _config);
            }

            UpdateDisplay(_config, lstitems);

            ResultItemGroups ritem = new ResultItemGroups();
            ritem.columns = _config.Columns;
            ritem.additionalcolumns = _config.AdditionalColumns;
            ritem.items = lstitems;

            SortResultGroup(ritem, _config);
            return ritem;
        }

        public class SortingField
        {
            public string name { get; set; }
            public int index { get; set; }
            public bool isDate { get; set; }
            public bool isNumber { get; set; }
        }
        private void SortResultGroup(ResultItemGroups ritem, Configuration config)
        {
            List<SortingField> sortingFields = new List<SortingField>();
            for (var i = 0; i < ritem.columns.Count; i++)
            {
                if (ritem.columns[i] == "Barcode" || ritem.columns[i] == "ImageID") continue;
                sortingFields.Add(new SortingField()
                {
                    name = ritem.columns[i],
                    index = i
                });
            }
            for (var i = 0; i < ritem.additionalcolumns.Count; i++)
            {
                sortingFields.Add(new SortingField()
                {
                    name = ritem.additionalcolumns[i].columnname,
                    isDate = ritem.additionalcolumns[i].isdate,
                    index = ritem.additionalcolumns[i].index,
                    isNumber = ritem.additionalcolumns[i].isnumber
                });
            }
            ritem.items.Sort((x, y) =>
            {
                foreach (var sitem in sortingFields)
                {
                    if (sitem.isDate)
                    {
                        var xvalue = string.IsNullOrEmpty(x.String[sitem.index]) ? DateTime.MinValue : BaseEntity.GetDataFromString(x.String[sitem.index]);
                        var yvalue = string.IsNullOrEmpty(y.String[sitem.index]) ? DateTime.MinValue : BaseEntity.GetDataFromString(y.String[sitem.index]);
                        var ret = xvalue.CompareTo(yvalue);
                        if (ret != 0) return ret;
                    }
                    else if (sitem.isNumber)
                    {
                        var xvalue = string.IsNullOrEmpty(x.String[sitem.index]) ? 0 : BaseEntity.GetNumberFromString(x.String[sitem.index]);
                        var yvalue = string.IsNullOrEmpty(y.String[sitem.index]) ? 0 : BaseEntity.GetNumberFromString(y.String[sitem.index]);
                        var ret = xvalue.CompareTo(yvalue);
                        if (ret != 0) return ret;
                    }
                    else
                    {
                        var xvalue = string.IsNullOrEmpty(x.String[sitem.index]) ? "" : x.String[sitem.index];
                        var yvalue = string.IsNullOrEmpty(y.String[sitem.index]) ? "" : y.String[sitem.index];
                        var ret = xvalue.CompareTo(yvalue);
                        if (ret != 0) return ret;
                    }
                }
                return 0;
            });
        }

        public void UpdateDisplay(Configuration _config, List<ResultItems> lstitems)
        {
            if (lstitems.Count == 0) return;
            foreach (var item in _config.AdditionalColumns)
            {
                UpdateDisplayList(lstitems, item.index, item.option, item.customoption, false);
            }

            int productIndex = _config.Columns.IndexOf("Product");
            if (productIndex >= 0)
            {
                List<long> ids = lstitems.ConvertAll<long>(e => e.Values[productIndex]);
                Data.ProductsDataService productdata = new ProductsDataService() { Context = this.Context };
                var valuelist = productdata.Search(new ProductsCreteria()
                {
                    ids = ids
                });
                lstitems.ForEach(e =>
                {
                    var index = valuelist.FindIndex(x => x.id == e.Values[productIndex]);
                    if (index >= 0)
                    {
                        e.String[productIndex] = valuelist[index].name;
                    }
                });
            }

            productIndex = _config.Columns.IndexOf("Brand");
            if (productIndex >= 0)
            {
                List<long> ids = lstitems.ConvertAll<long>(e => e.Values[productIndex]);
                Data.BrandDataService productdata = new BrandDataService() { Context = this.Context };
                var valuelist = productdata.Search(new BrandCreteria()
                {
                    ids = ids
                });
                lstitems.ForEach(e =>
                {
                    var index = valuelist.FindIndex(x => x.id == e.Values[productIndex]);
                    if (index >= 0)
                    {
                        e.String[productIndex] = valuelist[index].name;
                    }
                });
            }

            productIndex = _config.Columns.IndexOf("Item");
            if (productIndex >= 0)
            {
                List<long> ids = lstitems.ConvertAll<long>(e => e.Values[productIndex]);
                Data.ItemsDataService productdata = new ItemsDataService() { Context = this.Context };
                var valuelist = productdata.SearchList(ids);
                lstitems.ForEach(e =>
                {
                    var index = valuelist.FindIndex(x => x.ID == e.Values[productIndex]);
                    if (index >= 0)
                    {
                        e.String[productIndex] = valuelist[index].Name;
                    }
                });
            }

            List<string> referecneTypes = new List<string>();
            if (_config.Columns.IndexOf("Style") >= 0)
                referecneTypes.Add("STYLE");
            if (_config.Columns.IndexOf("Pattern") >= 0)
                referecneTypes.Add("PATTERN");
            if (_config.Columns.IndexOf("Size") >= 0)
                referecneTypes.Add("SIZE");
            if (_config.Columns.IndexOf("Colour") >= 0)
                referecneTypes.Add("COLOUR");
            if (_config.Columns.IndexOf("Fit") >= 0)
                referecneTypes.Add("FIT");
            if (_config.Columns.IndexOf("Sleeve") >= 0)
                referecneTypes.Add("SLEEVE");
            if (_config.Columns.IndexOf("Price Tag") >= 0)
                referecneTypes.Add("PRICETAGS");
            if (_config.Columns.IndexOf("Type") >= 0)
                referecneTypes.Add("TYPE");
            if (_config.Columns.IndexOf("Supplier City") >= 0)
                referecneTypes.Add("CITY");

            if (referecneTypes.Count > 0)
            {
                Data.ReferencelistDataService referencedata = new ReferencelistDataService() { Context = this.Context };
                var valuelist = referencedata.SearchList(new ReferencelistCreteria()
                {
                    types = referecneTypes
                });
                productIndex = _config.Columns.IndexOf("Style");
                if (productIndex >= 0)
                {
                    lstitems.ForEach(e =>
                    {
                        var index = valuelist.FindIndex(x => x.id == e.Values[productIndex]);
                        if (index >= 0)
                        {
                            e.String[productIndex] = valuelist[index].name;
                        }
                    });
                }
                productIndex = _config.Columns.IndexOf("Supplier City");
                if (productIndex >= 0)
                {
                    lstitems.ForEach(e =>
                    {
                        var index = valuelist.FindIndex(x => x.id == e.Values[productIndex]);
                        if (index >= 0)
                        {
                            e.String[productIndex] = valuelist[index].name;
                        }
                    });
                }
                productIndex = _config.Columns.IndexOf("Pattern");
                if (productIndex >= 0)
                {
                    lstitems.ForEach(e =>
                    {
                        var index = valuelist.FindIndex(x => x.id == e.Values[productIndex]);
                        if (index >= 0)
                        {
                            e.String[productIndex] = valuelist[index].name;
                        }
                    });
                }
                productIndex = _config.Columns.IndexOf("Size");
                if (productIndex >= 0)
                {
                    lstitems.ForEach(e =>
                    {
                        var index = valuelist.FindIndex(x => x.id == e.Values[productIndex]);
                        if (index >= 0)
                        {
                            e.String[productIndex] = valuelist[index].name;
                        }
                    });
                }
                productIndex = _config.Columns.IndexOf("Colour");
                if (productIndex >= 0)
                {
                    lstitems.ForEach(e =>
                    {
                        var index = valuelist.FindIndex(x => x.id == e.Values[productIndex]);
                        if (index >= 0)
                        {
                            e.String[productIndex] = valuelist[index].name;
                        }
                    });
                }
                productIndex = _config.Columns.IndexOf("Fit");
                if (productIndex >= 0)
                {
                    lstitems.ForEach(e =>
                    {
                        var index = valuelist.FindIndex(x => x.id == e.Values[productIndex]);
                        if (index >= 0)
                        {
                            e.String[productIndex] = valuelist[index].name;
                        }
                    });
                }
                productIndex = _config.Columns.IndexOf("Sleeve");
                if (productIndex >= 0)
                {
                    lstitems.ForEach(e =>
                    {
                        var index = valuelist.FindIndex(x => x.id == e.Values[productIndex]);
                        if (index >= 0)
                        {
                            e.String[productIndex] = valuelist[index].name;
                        }
                    });
                }
                productIndex = _config.Columns.IndexOf("Type");
                if (productIndex >= 0)
                {
                    lstitems.ForEach(e =>
                    {
                        var index = valuelist.FindIndex(x => x.id == e.Values[productIndex]);
                        if (index >= 0)
                        {
                            e.String[productIndex] = valuelist[index].name;
                        }
                    });
                }
            }
        }

        public List<AnalyserDataService.Items> StockAnalyser(Configuration _config)
        {
            string SQL = GetQuery(_config);
            if (_config.fromdate.Year < 2000 || _config.todate.Year < 2000 ||
                _config.reporttype == Configuration.ReportType.StockStress ||
                _config.reporttype == Configuration.ReportType.SalesVsStockAnalyser)
            {
                SQL = SQL.Replace("and lrinvoice.entrydate>=@fromdate and lrinvoice.entrydate<@todate", "");
                SQL = SQL.Replace("lrinvoicereturn.returndate>=@fromdate and lrinvoicereturn.returndate<@todate", "");
            }
            List<AnalyserDataService.Items> lstitems = new List<Items>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                NpgsqlCommand command = null;

                QueryBuilder qb = new QueryBuilder();
                qb.CurrentQuery = SQL;
                if (Services.ConfigManager.Instance(Context.OrganizationID).Config.analyser_stock_removeaqty)
                    SQL = SQL.Replace("salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty)", "salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty+salablegoods.aqty)");

                if (_config.PrimaryCondtion != Options.BillType)
                    SQL = SQL.Replace("and bill.istranfser = false", "");

                if (_config.SecondaryCondtions != null &&
                    _config.SecondaryCondtions.Count > 0 &&
                    _config.SecondaryCondtions.Any(x => x.option == Options.BillType))
                {
                    SQL = SQL.Replace("and bill.istranfser = false", "");
                }
                qb.CurrentQuery = SQL;

                command = qb.GetCommand(conn);
                //command.CommandText = "/*Running By " + Context.UserID + "/" + Context.EmployeeName + "*/\r\n" + command.CommandText;
                command.CommandTimeout = 10000;
                //if (_config.reporttype == Configuration.ReportType.Sales || _config.reporttype == Configuration.ReportType.SalesCompare || _config.reporttype == Configuration.ReportType.CoverDays
                //    || _config.reporttype == Configuration.ReportType.Purchase || _config.reporttype == Configuration.ReportType.SaleVsPurchase)
                {
                    command.Parameters.Add(new NpgsqlParameter("fromdate", _config.fromdate.Date));
                    command.Parameters.Add(new NpgsqlParameter("todate", _config.todate.AddDays(1).Date));
                    command.Parameters.Add(new NpgsqlParameter("fromdatecompare", _config.fromdatecompare.Date));
                    command.Parameters.Add(new NpgsqlParameter("todatecompare", _config.todatecompare.AddDays(1).Date));
                    command.Parameters.Add(new NpgsqlParameter("locationid", _config.locationid));
                    command.Parameters.Add(new NpgsqlParameter("margin", _config.margin));
                    command.Parameters.Add(new NpgsqlParameter("interestrate", _config.interestrate));
                    command.Parameters.Add(new NpgsqlParameter("current_date", DateTimeNow.Date));
                }
                var noofdays = _config.todate.Date.Subtract(_config.fromdate.Date).Days;
                if (_config.todate.Subtract(_config.fromdate).TotalDays > 31)
                {

                    WebAPI.Common.Logger.Instance(Context.OrganizationID).PerformanceLog("DBPerformance : " +
                        (String.IsNullOrEmpty(HttpRuntime.AppDomainAppVirtualPath) ? "" : HttpRuntime.AppDomainAppVirtualPath),
                    WebAPI.Common.Logger.LogType.Info, Environment.StackTrace, GetSQL(command), Context.UserID, _config.locationid);
                }
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    if(stopwatch.Elapsed.TotalSeconds>30)
                    {
                        WebAPI.Common.Logger.Instance(Context.OrganizationID).PerformanceLog("DBPerformance : " + stopwatch.Elapsed.TotalSeconds,
                            WebAPI.Common.Logger.LogType.Info, Environment.StackTrace,GetSQL(command),Context.UserID,_config.locationid);
                    }
                    while (reader.Read())
                    {
                        AnalyserDataService.Items oitem = new Items();
                        oitem.id = reader[0] == DBNull.Value ? "null" : reader[0].ToString();
                        if (reader[0] == DBNull.Value && _config.reporttype == Configuration.ReportType.Conversion)
                        {
                            oitem.id = reader["comparestockgroup"] == DBNull.Value ? "null" : reader["comparestockgroup"].ToString();
                        }

                        if (_config.reporttype == Configuration.ReportType.StockMarker)
                        {

                            oitem.qty = reader["stockqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["stockqty"]);
                            oitem.value = reader["stockcost"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["stockcost"]);
                            oitem.sqty = reader["stocksalevalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["stocksalevalue"]);

                            oitem.cqty = reader["saleqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["saleqty"]);
                            oitem.cvalue = reader["salecost"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["salecost"]);
                            oitem.svalue = reader["salevalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["salevalue"]);

                            oitem.aqty = reader["smqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["smqty"]);
                            oitem.iqty = reader["smsalevalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["smsalevalue"]);
                            oitem.ivalue = reader["smcost"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["smcost"]);

                        }
                        else
                        {
                            oitem.qty = reader["stock"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["stock"]);
                            oitem.value = reader["cost"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["cost"]);
                        }

                        if (_config.reporttype == Configuration.ReportType.SalesCompare)
                        {
                            oitem.cqty = reader["comparestock"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["comparestock"]);
                            oitem.cvalue = reader["comparecost"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["comparecost"]);

                            oitem.iqty = reader["cstock"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["cstock"]);
                            oitem.ivalue = reader["comparecstock"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["comparecstock"]);

                        }
                        if (_config.reporttype == Configuration.ReportType.SalesVsStockAnalyser)
                        {
                            oitem.iqty = reader["comparestock"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["comparestock"]);
                            oitem.ivalue = reader["comparecost"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["comparecost"]);
                            oitem.avalue = reader["comparesalesprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["comparesalesprice"]);
                        }
                        if (_config.reporttype == Configuration.ReportType.Conversion)
                        {
                            oitem.cvalue = reader["cstock"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["cstock"]);

                            //current stock
                            oitem.aqty = reader["sstock"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["sstock"]);
                            oitem.avalue = reader["ccost"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["ccost"]);

                            var rqty = reader["rfstock"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["rfstock"]);
                            var rvalue = reader["rfcost"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["rfcost"]);

                            oitem.sqty = reader["sstock"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["sstock"]);
                            oitem.svalue = reader["ccost"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["ccost"]);

                            oitem.iqty = reader["istock"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["istock"]);
                            oitem.ivalue = reader["icost"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["icost"]);

                            //end of day stock
                            oitem.sqty += (reader["rstock"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["rstock"]));
                            oitem.svalue += (reader["rcost"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["rcost"]));

                            //start of day stock
                            rqty += reader["rfstock"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["rfstock"]);
                            rvalue += reader["rfcost"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["rfcost"]);

                            oitem.oqty = oitem.aqty - oitem.iqty + (reader["rfstock"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["rfstock"])) +
                                (reader["rstock"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["rstock"]));

                            oitem.ovalue = oitem.avalue - oitem.ivalue + (reader["rfcost"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["rfcost"])) +
                                (reader["rcost"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["rcost"]));

                            var avaeragestockqty = rqty + oitem.sqty + oitem.aqty;
                            var avaeragestockvalue = rvalue + oitem.svalue + oitem.avalue;
                            if (avaeragestockvalue > 0)
                            {
                                oitem.cqty = (avaeragestockvalue / 3);
                            }
                            //if (oitem.cvalue > 0 &&
                            //    noofdays > 0 &&
                            //    oitem.svalue > 0)
                            //    oitem.cqty = Math.Round(oitem.cvalue / noofdays * 365 / oitem.svalue, 2);
                        }


                        if (_config.reporttype == Configuration.ReportType.AllStock || _config.reporttype == Configuration.ReportType.WarehouseStock || _config.reporttype == Configuration.ReportType.RetailStock)
                        {
                            //sum((salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty+salablegoods.aqty)) * (stock.eprice)::numeric) as scost
                            oitem.svalue = reader["scost"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["scost"]);

                            oitem.iqty = reader["istock"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["istock"]);
                            oitem.ivalue = reader["icost"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["icost"]);
                            oitem.avalue = reader["avalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["avalue"]);
                            oitem.buyingcost = reader["buyingcost"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["buyingcost"]);
                            oitem.sellingprice = reader["sellingprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["sellingprice"]);

                            oitem.cqty = reader["adjqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["adjqty"]);
                            oitem.cvalue = reader["adjval"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["adjval"]);
                        }


                        if (_config.reporttype == Configuration.ReportType.Purchase)
                        {
                            //(stock.qty-stock.returnqty) * stock.ibuyingprice::numeric
                            oitem.cvalue = reader["cvalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["cvalue"]);
                            //salablestock.available+(stock.qty-stock.returnqty-stock.transfered)
                            oitem.cqty = reader["cqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["cqty"]);
                            //(salablestock.available+(stock.qty-stock.returnqty-stock.transfered)) * stock.buyingprice::numeric
                            oitem.iqty = reader["iqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["iqty"]);
                            //(salablestock.available+(stock.qty-stock.returnqty-stock.transfered)) * stock.ibuyingprice::numeric)
                            oitem.ivalue = reader["ivalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["ivalue"]);
                            //(salablestock.available+(stock.qty-stock.returnqty-stock.transfered)) * (stock.price-stock.discount)::numeric)
                            oitem.svalue = reader["svalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["svalue"]);
                            oitem.buyingcost = reader["cost"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["cost"]);
                            oitem.sellingprice = reader["sellingprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["sellingprice"]);
                        }

                        if (_config.reporttype == Configuration.ReportType.PurchaseReturn)
                        {
                            //sum(lrinvoicereturnitems.qty * (lrinvoicereturnitems.rate + tax))
                            oitem.cvalue = reader["cvalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["cvalue"]);
                            //salablestock.available+(stock.qty-stock.returnqty-stock.transfered)
                            oitem.cqty = reader["cqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["cqty"]);
                            //(salablestock.available+(stock.qty-stock.returnqty-stock.transfered)) * stock.buyingprice::numeric
                            oitem.iqty = reader["iqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["iqty"]);
                            //(salablestock.available+(stock.qty-stock.returnqty-stock.transfered)) * stock.ibuyingprice::numeric)
                            oitem.ivalue = reader["ivalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["ivalue"]);
                            //(salablestock.available+(stock.qty-stock.returnqty-stock.transfered)) * (stock.price-stock.discount)::numeric)
                            oitem.svalue = reader["svalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["svalue"]);
                        }

                        if (_config.reporttype == Configuration.ReportType.SaleVsPurchase)
                        {
                            oitem.sqty = reader["sstock"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["sstock"]);
                            oitem.svalue = reader["salcost"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["salcost"]);
                        }


                        if (_config.reporttype == Configuration.ReportType.StockStress)
                        {
                            oitem.sqty = reader["sstock"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["sstock"]);
                            oitem.svalue = reader["scost"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["scost"]);
                        }

                        if (_config.reporttype == Configuration.ReportType.SaleVsPurchase)
                        {
                            oitem.iqty = reader["istock"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["istock"]);
                            oitem.ivalue = reader["icost"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["icost"]);

                            // Replaced buying price to ibuying price only on Sales Vs Purchase 
                            oitem.value = reader["cvalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["cvalue"]);
                            //
                            //oitem.cqty = reader["cqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["cqty"]);
                            //oitem.cvalue = reader["ivalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["ivalue"]);

                            oitem.rqty = reader["rqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["rqty"]);
                            oitem.rvalue = reader["rvalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["rvalue"]);

                            oitem.cqty = oitem.qty - oitem.iqty - oitem.rqty;
                            oitem.cvalue = oitem.value - oitem.ivalue - oitem.rvalue;
                        }

                        if (_config.reporttype == Configuration.ReportType.SalesPlan)
                        {
                            oitem.svalue = reader["scost"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["scost"]);
                            oitem.cvalue = reader["interest"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["interest"]);
                        }
                        if (_config.reporttype == Configuration.ReportType.Sales ||
                            _config.reporttype == Configuration.ReportType.SalesVsStockAnalyser ||
                            _config.reporttype == Configuration.ReportType.SalesOnApproval)
                        {
                            oitem.cvalue = reader["cstock"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["cstock"]);
                            oitem.svalue = reader["scost"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["scost"]);
                        }
                        if (_config.reporttype == Configuration.ReportType.Customers)
                        {
                            //sum(billitems.qty * stock.ibuyingprice::numeric) as cstock, 
                            oitem.cvalue = reader["cstock"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["cstock"]);
                            //sum((billitems.qty * billitems.additionaldiscount::numeric) + billitems.schemediscount) scost,
                            oitem.svalue = reader["scost"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["scost"]);
                            //sum(billitems.qty * stock.buyingprice::numeric) as ivalue, 
                            oitem.ivalue = reader["ivalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["ivalue"]);
                            //sum((billitems.receivable - billitems.tax) / billitems.qty) as iqty from bill
                            oitem.iqty = reader["iqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["iqty"]);
                        }
                        if (_config.reporttype == Configuration.ReportType.Sales || _config.reporttype == Configuration.ReportType.SalesOnApproval)
                        {
                            oitem.sellingprice = reader["margin"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["margin"]);
                            //sum(billitems.qty * stock.buyingprice::numeric) as ivalue, 
                            oitem.ivalue = reader["ivalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["ivalue"]);
                            //sum((billitems.receivable - billitems.tax) / billitems.qty) as iqty from bill
                            oitem.iqty = reader["iqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["iqty"]);
                        }
                        lstitems.Add(oitem);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _config);
            }


            UpdateDisplayList(_config, lstitems, _config.PrimaryCondtion, _config.CustomOption);
            if (_config.PrimaryRange != null)
            {
                lstitems.Sort(delegate (AnalyserDataService.Items a, AnalyserDataService.Items b)
                {
                    decimal avalue = 0;
                    decimal bvalue = 0;
                    decimal.TryParse(a.name.IndexOf('-') > 0 ? a.name.Split('-')[0] : a.name, out avalue);
                    decimal.TryParse(b.name.IndexOf('-') > 0 ? b.name.Split('-')[0] : b.name, out bvalue);
                    return avalue.CompareTo(bvalue);
                });
            }
            else
            {
                if (_config.PrimaryCondtion == Options.Size)
                {

                }
                if (_config.PrimaryCondtion != Options.SaleMonth)
                {
                    lstitems.Sort(delegate (AnalyserDataService.Items a, AnalyserDataService.Items b)
                    {
                        if (string.IsNullOrEmpty(a.name) && string.IsNullOrEmpty(b.name)) return 0;
                        if (string.IsNullOrEmpty(a.name) && !string.IsNullOrEmpty(b.name)) return 1;
                        if (!string.IsNullOrEmpty(a.name) && string.IsNullOrEmpty(b.name)) return -1;
                        return a.name.CompareTo(b.name);
                    });
                }
            }
            return lstitems;
        }


        public string GetReferenceQuery(Options option, string customoption)
        {
            string query = "";
            switch (option)
            {
                case Options.Company:
                    query = "Select id,name from company";
                    break;
                case Options.ProductGroup:
                    query = "Select id,name from referencelist where type = 'PRODUCTGROUPS'";
                    break;
                case Options.BarcodeGroup:
                    query = "Select id,name from referencelist where type = 'BARCODEID'";
                    break;
                case Options.ReturnReason:
                    query = "Select id,name from referencelist where type = 'RETURNREASON'";
                    break;
                case Options.SaleReason:
                    query = "Select id,name from referencelist where type = 'BULKSALEREASON'";
                    break;
                case Options.Floor:
                    query = "Select id,name from referencelist where type = 'FLOOR'";
                    break;
                case Options.Counter:
                    query = "Select id,name from referencelist where type = 'COUNTER'";
                    break;
                case Options.Season:
                    query = "Select id,name from referencelist where type = 'SEASONS'";
                    break;
                case Options.Area:
                    query = "Select id,name from referencelist where type = 'AREA'";
                    break;
                case Options.Brand:
                    query = "Select id,name from brand";
                    break;
                case Options.Colour:
                    query = "select id,name from referencelist where type = 'COLOUR'";
                    break;
                case Options.Material:
                    query = "select id,name from referencelist where type = 'MATERIAL'";
                    break;

                case Options.Fit:
                    query = "select id,name from referencelist where type = 'FIT'";
                    break;
                case Options.Sleeve:
                    query = "select id,name from referencelist where type = 'SLEEVE'";
                    break;
                case Options.PriceTag:
                    query = "select id,name from referencelist where type = 'PRICETAGS'";
                    break;

                case Options.Pattern:
                    query = "select id,name from referencelist where type = 'PATTERN'";
                    break;
                case Options.Product:
                    query = "Select id,name from products";
                    break;
                case Options.Tax:
                    query = "Select id,name from tax";
                    break;
                case Options.SalesmanCode:
                    query = "Select id,employeecode as name from employee";
                    break;
                case Options.Incharge:
                case Options.SalesmanName:
                    query = "Select id, concat (employeecode, ' - ', name) as name from employee";
                    break;
                case Options.Section:
                    query = "select id,name from referencelist where type = 'DIVISION'";
                    break;
                case Options.Item:
                    query = "select id,printingname as name from items ";
                    break;
                case Options.Size:
                    query = "select id,name from referencelist where type = 'SIZE'";
                    break;
                case Options.EntryLocation:
                case Options.SaleLocation:
                case Options.StockLocation:
                    query = "select id,name from referencelist where type = 'LOCATION'";
                    break;
                case Options.Style:
                    query = "select id,name from referencelist where type = 'STYLE'";
                    break;
                case Options.Agent:
                case Options.Customer_Agent:
                    query = "select id,name from Agent";
                    if (Context.HasPermission(PermissionSet.hidesupplier, InfyPOS.Common.Action.View))
                    {
                        query = "Select id,(case when code is null then cast(id as varchar(20)) else code end) as name from Agent";
                    }
                    break;
                case Options.Type:
                    query = "select id,name from referencelist where type = 'TYPE'";
                    break;
                case Options.Customer_Religion:
                    query = "select id,name from referencelist where type = 'RELIGION'";
                    break;
                case Options.Customer_Category:
                    query = "select id,name from referencelist where type = 'CUSTOMERCATEGORY'";
                    break;
                case Options.Customer_Name:
                    query = "select id,name from customer";
                    break;
                case Options.Promotion:
                    query = "Select id,name from promotions";
                    break;
                case Options.Supplier:
                case Options.Source_Supplier:
                case Options.JobworkSupplier:
                    query = "Select id,name from supplier";
                    if (Context.HasPermission(PermissionSet.hidesupplier, InfyPOS.Common.Action.View))
                    {
                        query = "Select id,(case when code is null then cast(id as varchar(20)) else code end) as name from supplier";
                    }
                    break;
                case Options.SaleAreaGroup1:
                    query = "select id,name from referencelist where type = 'AREAGROUP1'";
                    break;
                case Options.SaleAreaGroup2:
                    query = "select id,name from referencelist where type = 'AREAGROUP2'";
                    break;
                case Options.Customer_City:
                case Options.Supplier_City:
                    query = "select id,name from referencelist where type = 'CITY'";
                    break;
                case Options.StockMarker:
                    query = "select id,name from referencelist where type = 'MARKER'";
                    break;
                case Options.Customer_AgeSet:
                    query = "select id,name from referencelist where type = 'AGESET'";
                    break;
                case Options.Custom_ProductGroups:
                    query = "select id,name from referencelist where type = 'PRODUCTHIERARCHY'";
                    break;
                case Options.Custom_Fields:
                    query = GetCustomFieldReferenceQuery(customoption);
                    break;
            }
            return query;
        }

        private string GetCustomFieldReferenceQuery(string coption)
        {
            var customoptions = new CustomField(coption);
            switch (customoptions.referenceid)
            {
                case 4:
                    return "select id,name from referencelist where type = '" + customoptions.referencetype + "'";
                case 5:
                    return "select id,name from referencelist where type = 'COLOUR'";
                case 6:
                    return "select id,name from referencelist where type = 'SIZE'";
                case 7:
                    return "select id,name from referencelist where type = 'FIT'";
                case 8:
                    return "select id,name from referencelist where type = 'TYPE'";
                case 9:
                    return "select id,name from referencelist where type = 'MATERIAL'";
                case 10:
                    return "select id,name from referencelist where type = 'STYLE'";
                case 11:
                    return "select id,name from referencelist where type = 'PATTERN'";
            }
            return "";
        }
        public class SalesIncentiveValues
        {
            public decimal from { get; set; }
            public decimal to { get; set; }
            public decimal incentive { get; set; }
            public decimal newincentive { get; set; }
            public decimal qty { get; set; }
            public decimal value { get; set; }
        }

        public List<SalesIncentiveValues> SectionWiseSalesIncentive(Models.ReferenceSectionConfig source)
        {
            List<SalesIncentiveValues> salesIncentiveValues = new List<SalesIncentiveValues>();

            string query = @"select {0} as config,sum(billitems.qty) as qty, sum(billitems.receivable) as value from bill 
            inner join billitems on billitems.billid = bill.id and billitems.isactive=true
            left outer join salablegoods on salablegoods.id = salablegoodsid
            left outer join stock on stock.id = salablegoods.stockid
            left outer join products on products.id = stock.productid
            where bill.billdate>=@fromdate and bill.billdate<@todate  AND salablegoods.locationid = @locationid 
            and products.salescounterid = @sectionid
            group by {0}";

            if (source.productwise)
                query = query.Replace("products.salescounterid = @sectionid", "stock.productid = @sectionid");


            string range = @"case 
                            {0}
                            else 'Others'
                            end";

            StringBuilder rangeconditions = new StringBuilder();
            foreach (var item in source.additionalinfo.salesincentive)
            {
                rangeconditions.AppendFormat("when (stock.price-stock.discount) BETWEEN {0} AND {1} then '{0} - {1}'\r\n", item.from, item.to);
            }
            NpgsqlConnection conn = CreateConnection();
            try
            {
                decimal totalSales = 0;
                conn.Open();
                NpgsqlCommand command = null;
                command = new NpgsqlCommand(string.Format(query, string.Format(range, rangeconditions.ToString())), conn);
                command.Parameters.Add("sectionid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = source.referenceid > 0 ? source.referenceid : source.id;
                command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = source.locationid;
                command.Parameters.Add("fromdate", NpgsqlTypes.NpgsqlDbType.Date).Value = source.fromdate.Date;
                command.Parameters.Add("todate", NpgsqlTypes.NpgsqlDbType.Date).Value = source.todate.AddDays(1).Date;
                command.Parameters.Add("current_date", NpgsqlTypes.NpgsqlDbType.Date).Value = DateTimeNow.Date;
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        SalesIncentiveValues salesIncentive = new SalesIncentiveValues();
                        string config = reader["config"] == DBNull.Value ? "" : reader["config"].ToString();
                        if (config == "Others") continue;
                        string[] ranges = config.Split('-');
                        salesIncentive.from = decimal.Parse(ranges[0].Trim());
                        salesIncentive.to = decimal.Parse(ranges[1].Trim());
                        salesIncentive.qty = reader["qty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["qty"]);
                        salesIncentive.value = reader["value"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["value"]);
                        salesIncentiveValues.Add(salesIncentive);
                        totalSales += reader["value"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["value"]);
                    }
                }
                conn.Close();

                var availableValue = totalSales * source.incentivepercentage / 100;
                foreach (var item in salesIncentiveValues)
                {
                    if (item.value != 0)
                        item.newincentive = Math.Round((availableValue * (item.value / totalSales * 100)) / 100 / item.qty, 2);
                    item.incentive = source.additionalinfo.salesincentive.Find(e => e.from == item.from && e.to == item.to).incentive;
                }
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);
                throw exp;
            }
            return salesIncentiveValues.OrderBy(e => e.from).ToList();
        }

        private void UpdateDisplayList(List<AnalyserDataService.ResultItems> source, int index, Options option, string customoption, bool fromid)
        {
            string query = GetReferenceQuery(option, customoption);
            if (query != "")
            {
                if (fromid)
                    query += (query.Contains(" where ") ? " and " : " where ") + " id in (" + string.Join<long>(",", source.ConvertAll(e => (string.IsNullOrEmpty(e.id) || e.id == "null") ? 0 : long.Parse(e.id)).Distinct().ToList()) + ")";
                else
                    query += (query.Contains(" where ") ? " and " : " where ") + " id in (" + string.Join<long>(",", source.ConvertAll(e => e.Values[index]).Distinct().ToList()) + ")";

                if (query.IndexOf(" referencelist ") > 0)
                    query += " order by sortingindex";

                NpgsqlConnection conn = CreateConnection();
                try
                {
                    conn.Open();
                    NpgsqlCommand command = null;

                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = query;
                    command = qb.GetCommand(conn);
                    SortedDictionary<long, string> displaylist = new SortedDictionary<long, string>();
                    using (System.Data.IDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {

                            long id = reader.IsDBNull(0) ? 0 : Convert.ToInt64(reader[0]);
                            string name = reader.IsDBNull(1) ? "" : reader.GetString(1);
                            displaylist.Add(id, name);
                        }
                    }
                    conn.Close();

                    source.ForEach(e =>
                    {
                        e.String[index] = "";
                        if (displaylist.ContainsKey(e.Values[index]))
                        {
                            e.String[index] = displaylist[e.Values[index]];
                        }
                    });
                }
                catch (Exception exp)
                {
                    if (conn != null) conn.Close();
                    WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, option);
                }
            }
            else if (option == Options.SaleWeekDay)
            {
                var dayList = System.Globalization.DateTimeFormatInfo.CurrentInfo.DayNames;
                source.ForEach(e =>
                {
                    e.String[index] = dayList[e.Values[index]];
                });
            }
            else if (option == Options.SellingMode)
            {
                source.ForEach(e =>
                {
                    e.String[index] = e.Values[index] == 1 ? "B2B" : "RETAIL";
                });
            }
            else if (option == Options.BillType)
            {
                source.ForEach(e =>
                {
                    switch(e.Values[index])
                    {
                        case 1:
                            e.String[index] = "Sales Return";
                            break;
                        case 2:
                            e.String[index] = "Company Transfer";
                            break;
                        case 3:
                            e.String[index] = "Export";
                            break;
                        default:
                            e.String[index] = "Sales Invoice";
                            break;
                    }
                });
            }
            else if (option == Options.PurchaseMonth || option == Options.SaleMonth)
            {
                var monthList = System.Globalization.DateTimeFormatInfo.CurrentInfo.MonthNames;
                source.ForEach(e =>
                {
                    e.String[index] = (e.Values[index] == 0 ? "" : monthList[e.Values[index] - 1]);
                });
            }
            else
            {
                source.ForEach(e => e.name = e.id);
            }
        }

        private void UpdateDisplayList(Configuration config, List<AnalyserDataService.Items> source, Options option, string customoption)
        {
            if (source.Count == 0) return;
            string query = GetReferenceQuery(option, customoption);
            if (query != "")
            {
                query += (query.Contains(" where ") ? " and " : " where ") + " id in (" + string.Join<long>(",", source.ConvertAll(e => (string.IsNullOrEmpty(e.id) || e.id == "null") ? 0 : long.Parse(e.id))) + ")";
                NpgsqlConnection conn = CreateConnection();
                try
                {
                    conn.Open();
                    NpgsqlCommand command = null;

                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = query;
                    command = qb.GetCommand(conn);
                    SortedDictionary<long, string> displaylist = new SortedDictionary<long, string>();
                    using (System.Data.IDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {

                            long id = reader.IsDBNull(0) ? 0 : Convert.ToInt64(reader[0]);
                            string name = reader.IsDBNull(1) ? "" : reader.GetString(1);
                            displaylist.Add(id, name);
                        }
                    }
                    conn.Close();

                    source.ForEach(e =>
                    {
                        long id;
                        e.name = "";
                        if (long.TryParse(e.id, out id))
                        {
                            if (displaylist.ContainsKey(id))
                            {
                                e.name = displaylist[id];
                            }
                        }
                    });
                }
                catch (Exception exp)
                {
                    if (conn != null) conn.Close();
                    WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, option);
                }
            }
            else if (option == Options.SellingMode)
            {
                source.ForEach(e =>
                {
                    long id;
                    e.name = "";
                    if (long.TryParse(e.id, out id))
                        e.name = id == 1 ? "B2B" : "RETAIL";
                });
            }
            else if (option == Options.BillType)
            {
                source.ForEach(e =>
                {
                    e.name = e.id == "1" ? "Sales Return" : (e.id == "2" ? "Company Transfer" : "Sales Invoice");
                });
                source.ForEach(e =>
                {
                    switch (e.id)
                    {
                        case "1":
                            e.name = "Sales Return";
                            break;
                        case "2":
                            e.name = "Company Transfer";
                            break;
                        case "3":
                            e.name = "Export";
                            break;
                        default:
                            e.name = "Sales Invoice";
                            break;
                    }
                });
            }
            else if (option == Options.SaleWeekDay)
            {
                var dayList = System.Globalization.DateTimeFormatInfo.CurrentInfo.DayNames;
                source.ForEach(e =>
                {
                    long id;
                    e.name = "";
                    if (long.TryParse(e.id, out id))
                        e.name = dayList[id];
                });
            }
            else if (option == Options.Customer_Gender)
            {
                var dayList = new string[] { "Unknown", "Male", "Female", "Transgender" };
                source.ForEach(e =>
                {
                    long id;
                    e.name = "";
                    if (long.TryParse(e.id, out id))
                        e.name = dayList[id];
                });
            }
            else if (option == Options.PurchaseMonth || option == Options.SaleMonth)
            {
                var monthList = System.Globalization.DateTimeFormatInfo.CurrentInfo.MonthNames;
                source.ForEach(e =>
                {
                    long id;
                    e.name = "";
                    if (long.TryParse(e.id, out id))
                        e.name = (id == 0 ? "" : monthList[id - 1]);
                });
            }
            else if ((option == Options.BillDate && config.reporttype != Configuration.ReportType.SalesCompare) || option == Options.InvDate || option == Options.Customer_Anniversary
                || option == Options.Customer_DOB || option == Options.DebitNoteDate || option == Options.LRDate
                || option == Options.LREntryDate || option == Options.PurchaseDate || option == Options.BarcodedOn)
            {
                source.ForEach(e =>
                {
                    DateTime dt;
                    if (DateTime.TryParse(e.id, out dt))
                    {
                        e.name = string.IsNullOrEmpty(e.id) ? "" : DateTime.Parse(e.id).ToString("dd-MM-yyyy");
                        e.id = DateTime.Parse(e.id).ToString("yyyy-MM-dd");
                    }
                    else
                    {
                        e.name = DateTime.MinValue.ToString("dd-MM-yyyy");
                        e.id = DateTime.MinValue.ToString("yyyy-MM-dd");
                    }
                });
            }
            else
            {
                source.ForEach(e => e.name = e.id);
            }

            foreach (var item in source.GroupBy(e => e.id).ToList())
            {
                if (item.Count() > 1)
                {
                    item.ToList().ForEach(e => e.id = Guid.NewGuid().ToString());
                }
            }
        }



        private string GetCustomFieldCondition(Condtion condition, string customoption)
        {
            var customoptions = new CustomField(customoption);
            if (customoptions.referenceid == 1)
                return "lrinvoiceitems." + customoptions.cfname + " in (" + string.Join(",", condition.stringList.ConvertAll(e => "'" + WebAPI.Data.Utility.SafeSQL(e) + "'")) + ")";
            else if (customoptions.referenceid == 2)
                return "lrinvoiceitems." + customoptions.cfname + " in (" + string.Join(", ", condition.decimalList) + ")";
            else if (customoptions.referenceid == 3)
                return "lrinvoiceitems." + customoptions.cfname + " in (" + string.Join(",", condition.dateList.ConvertAll(e => "'" + e.ToString("yyyy-MM-dd") + "'")) + ")";
            else
                return "lrinvoiceitems." + customoptions.cfname + " in (" + string.Join(", ", condition.longList) + ")";
        }

        private string GetCustomField(string customoption)
        {
            var customoptions = new CustomField(customoption);
            return "lrinvoiceitems." + customoptions.cfname;
        }

        public List<StockTransactionAsOn.History> StockTransactionHistory(StockAsOnCriteria _reportcreteria)
        {
            var response = new List<StockTransactionAsOn.History>();
            var param = new SortedList<string, object>();
            var barcodecondition = _reportcreteria.conditions.Find(x => x.option == (int)InfyPOS.AnalyserDataService.Options.Barcode);
            param.Add("barcode", barcodecondition.value);
            param.Add("locationid", _reportcreteria.locationid);
            
            long salablegoodsid = 0;
            decimal opening = 0;
            using (var reader = ExecuteReader("select id from salablegoods where barcode=@barcode and locationid=@locationid", param))
            {
                if(reader.Read())
                {
                    salablegoodsid = reader.GetInt64(0);
                }
            }

            if (salablegoodsid == 0)
                throw new Exception("Can't find the barcode on the selected location");

            param.Add("salablegoodsid", salablegoodsid);
            param.Add("obfromdate", _reportcreteria.fromdate);
            using (var reader = ExecuteReader(@"select sum(qty) from stocktransaction where salablegoodsid=@salablegoodsid
                                                and transactedon<@obfromdate", param))
            {
                if (reader.Read())
                {
                    opening = reader.IsDBNull(0) ? 0: reader.GetDecimal(0);
                }
            }

            response.Add(new StockTransactionAsOn.History()
            {
                mode = "OP",
                ob = opening,
                cb = opening
            });
            param.Add("todate", _reportcreteria.todate.AddDays(1));
            using (var reader = ExecuteReader(@"select transactedon,createdby,reftype,qty from stocktransaction where salablegoodsid=@salablegoodsid
                                                and transactedon>=@obfromdate and transactedon<@todate
                                                order by transactedon", param))
            {
                while (reader.Read())
                {
                    StockTransactionAsOn.History history = new StockTransactionAsOn.History();
                    decimal qty = Convert.ToDecimal(reader["qty"]);
                    if (qty > 0)
                        history.ti = qty;
                    else
                        history.to = qty * -1;
                    history.createdby = Convert.ToInt64(reader["createdby"]);
                    history.ason = Convert.ToDateTime(reader["transactedon"]);
                    history.mode = Convert.ToString(reader["reftype"]);
                    response.Add(history);
                }
            }

            UsersDataService usersDataService = new UsersDataService() { Context = this.Context };
            var users = usersDataService.SearchAll(new UsersCreteria()
            {
                ids = response.ConvertAll(x => x.createdby).Distinct().ToList()
            });
            
            response.ForEach(x =>
            {
                x.ob = opening;
                x.cb = opening + x.ti - x.to;
                opening = x.cb;
                var idx = users.FindIndex(y => y.id == x.createdby);
                if (idx >= 0)
                {
                    if (!string.IsNullOrEmpty(users[idx].title))
                        x.username = users[idx].title + " (" + users[idx].username + " )";
                    else
                        x.username = users[idx].username;
                }
            });
            return response;
        }

        public StockTransactionAsOn SearchAsOnStockTransaction(StockAsOnCriteria _reportcreteria)
        {

            List<StockTransactionAsOn.StockTransactionRow> response = new List<StockTransactionAsOn.StockTransactionRow>();
            string fromdate = _reportcreteria.fromdate.Date.ToString("yyyy-MM-dd");
            string rangefrom = _reportcreteria.fromdate.ToString("yyyy-MM-dd");
            string rangeto = _reportcreteria.todate.AddDays(1).ToString("yyyy-MM-dd");
            string query = @"with openingstock
            as
            (
            select sum(qty) opening,salablegoodsid from stocktransaction
            where salablegoodsid>0 and transactedon<'{0}'
            and stocktransaction.locationid = @locationid
            group by salablegoodsid
            ),sttransaction as
            (
            select * from crosstab(
            'select salablegoodsid,reftype,sum(qty) qty from stocktransaction
            where salablegoodsid>0 and transactedon>''{1}'' and transactedon<''{2}''
            and stocktransaction.locationid = @locationid
            group by salablegoodsid,reftype
            ORDER BY 1,2',
            'select * from unnest(ARRAY[''TI'', ''TO'',''PU'',''PR'',''SL'',''OP'',''AU'',''MS''])')
            as ct(salablegoodsid bigint,""TI"" numeric,""TO"" numeric,""PU"" numeric,""PR"" numeric,
	              ""SL"" numeric,""OP"" numeric,""AU"" numeric,""MS"" numeric)
            ),closingstock
            as
            (select salablegoods.id salablegoodsid,
            COALESCE(openingstock.opening,0) opening,""TI"",""TO"",""PU"",""PR"",""SL"",""OP"",""AU"",""MS"",
            COALESCE(openingstock.opening,0)+
            COALESCE(""TI"",0)+
            COALESCE(""TO"",0)+
            COALESCE(""PU"",0)+
            COALESCE(""PR"",0)+
            COALESCE(""SL"",0)+
            COALESCE(""OP"",0)+
            COALESCE(""AU"",0)+
            COALESCE(""MS"",0) closing
            from salablegoods
            left outer join sttransaction on salablegoodsid=salablegoods.id
            left outer join openingstock on openingstock.salablegoodsid = salablegoods.id
            where salablegoods.locationid = @locationid
            ),asstocklist as
            (
            select stock.serialno,sum(closing*stock.buyingprice) buyingprice,sum(closing*stock.ibuyingprice) ibuyingprice,
            sum(opening) opening,sum(opening*stock.buyingprice) obuyingprice,sum(opening*stock.ibuyingprice) oibuyingprice,
            sum(""TI"") xti,
            sum(""TO"") xto,sum(""PU"") xpu,sum(""PR"") xpr,sum(""SL"") xsl,sum(""OP"") xop,
            sum(""AU"") xau,sum(""MS"") xms,sum(closing) closing {3}
            from salablegoods
            inner join closingstock on salablegoodsid = salablegoods.id
            inner join stock on stock.id = salablegoods.stockid
            inner join items on items.id = stock.itemid
            inner join products on products.id = stock.productid
            left outer join lrinvoiceitems on lrinvoiceitems.id = stock.invoiceitemid and lrinvoiceitems.isactive = true
            left outer join lrinvoice on lrinvoice.id = lrinvoiceitems.lrinvoiceid and lrinvoice.isactive = true
            left outer join lrentry  on lrentry.id = lrid and lrentry.isactive = true
            left outer join supplier on supplier.id = stock.supplierid 
            left outer join suppliercompany on suppliercompany.supplierid = supplier.id
            where salablegoods.isactive=true {4}
            and salablegoods.locationid = @locationid
            group by stock.serialno,{5}
            )
            select * from asstocklist ";
            List<String> fields = new List<string>();
            List<String> gfields = new List<string>();
            List<string> numberfields = new List<string>();
            List<string> datefields = new List<string>();
            List<string> longfields = new List<string>();
            List<string> strfields = new List<string>();
            List<string> conditions = new List<string>();
            List<int> displayfields = new List<int>();



            if (_reportcreteria.companyid > 0)
                conditions.Add("salablegoods.companyid=@companyid");
            if (_reportcreteria.locationid > 0)
            {
                conditions.Add("salablegoods.locationid=@locationid");
            }
            else
            {
                query = query.Replace("and salablegoods.locationid = @locationid", "");
                query = query.Replace("where stocktransaction.locationid = @locationid", "");
                query = query.Replace("and stocktransaction.locationid = @locationid", "");
            }
            List<string> additionafields = new List<string>();
            if (_reportcreteria.customfields != null)
                additionafields = _reportcreteria.customfields.ConvertAll(x => "mfg_" + x.field.cfname);

            if (_reportcreteria.conditions != null && _reportcreteria.conditions.Count > 0)
            {
                foreach (var cnd in _reportcreteria.conditions)
                {
                    var option = (InfyPOS.AnalyserDataService.Options)cnd.option;
                    var field = GetField(option);
                    if (option == Options.Design ||
                        option == Options.LREntryNo ||
                        option == Options.InvoiceNo ||
                        option == Options.BillNo ||
                        option == Options.Barcode ||
                        option == Options.HSNCode ||
                        option == Options.LRNo ||
                        option == Options.PurchaseDate ||
                        option == Options.BarcodedOn ||
                        option == Options.InvDate ||
                        option == Options.GLNNO)
                    {
                        conditions.Add(String.Format("{0}='{1}'", field, cnd.value));
                    }
                    else
                    {
                        conditions.Add(String.Format("{0}={1}", field, cnd.value));
                    }
                }
            }


            string condition = string.Join(" and ", conditions);
            SortedDictionary<string, int> posfields = new SortedDictionary<string, int>();
            SortedDictionary<string, string> refquerylist = new SortedDictionary<string, string>();
            var optionconfiguration = GetOptionConfiguration();
            foreach (var f in _reportcreteria.fields)
            {
                var field = GetField((InfyPOS.AnalyserDataService.Options)f);
                if (!string.IsNullOrEmpty(field))
                {
                    if (!optionconfiguration.ContainsKey((InfyPOS.AnalyserDataService.Options)f))
                    {
                        continue;
                    }
                    displayfields.Add(f);
                    string fieldname = "f" + (gfields.Count + 1);
                    posfields.Add(fieldname, gfields.Count + 1);
                    fields.Add(field + " as f" + (gfields.Count + 1));

                    gfields.Add(field);



                    if (optionconfiguration[(InfyPOS.AnalyserDataService.Options)f].conditiontype == Condtion.CondtionType.Date)
                    {
                        datefields.Add(fieldname);
                    }
                    else if (optionconfiguration[(InfyPOS.AnalyserDataService.Options)f].conditiontype == Condtion.CondtionType.Decimal)
                    {
                        numberfields.Add(fieldname);
                    }
                    else if (optionconfiguration[(InfyPOS.AnalyserDataService.Options)f].conditiontype == Condtion.CondtionType.Long)
                    {
                        longfields.Add(fieldname);
                    }
                    else
                    {
                        strfields.Add(fieldname);
                    }
                    var refquery = GetReferenceQuery((InfyPOS.AnalyserDataService.Options)f, "");
                    if (!string.IsNullOrEmpty(refquery))
                        refquerylist.Add(fieldname, refquery);
                }
            }

            query = string.Format(query, fromdate, rangefrom, rangeto,
                fields.Count == 0 ? "" : ("," + string.Join(",", fields)),
                string.IsNullOrEmpty(condition) ? "" : (" and " + condition),
                string.Join(",", gfields),
                additionafields.Count == 0 ? "" : ("," + (string.Join(",", additionafields))));

            if (gfields.Count == 0)
                query = query.Replace("group by stock.serialno,", "").Replace("select stock.serialno,", "select ");

            var param = new SortedList<string, object>();
            param.Add("companyid", _reportcreteria.companyid);
            param.Add("locationid", _reportcreteria.locationid);
            using (var reader = ExecuteReader(query, param,100000))
            {
                while (reader.Read())
                {
                    StockTransactionAsOn.StockTransactionRow stockTransationRow = new StockTransactionAsOn.StockTransactionRow()
                    {
                        _value = new SortedList<string, long>(),
                        values = new List<string>()
                    };
                    stockTransationRow.values.AddRange(Enumerable.Repeat("", fields.Count));
                    stockTransationRow.opening = reader["opening"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["opening"]);
                    stockTransationRow.closing = reader["closing"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["closing"]);
                    stockTransationRow.AU = reader["xau"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["xau"]);
                    stockTransationRow.MS = reader["xms"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["xms"]);
                    stockTransationRow.OP = reader["xop"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["xop"]);
                    stockTransationRow.PR = reader["xpr"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["xpr"]);
                    stockTransationRow.PU = reader["xpu"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["xpu"]);
                    stockTransationRow.SL = reader["xsl"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["xsl"]);
                    stockTransationRow.TI = reader["xti"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["xti"]);
                    stockTransationRow.TO = reader["xto"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["xto"]);

                    stockTransationRow.closingcost = reader["buyingprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["buyingprice"]);
                    stockTransationRow.closingicost = reader["ibuyingprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["ibuyingprice"]);

                    stockTransationRow.openingcost = reader["obuyingprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["obuyingprice"]);
                    stockTransationRow.openingicost = reader["oibuyingprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["oibuyingprice"]);

                    stockTransationRow.SL = stockTransationRow.SL * -1;
                    stockTransationRow.PR = stockTransationRow.PR * -1;
                    stockTransationRow.TO = stockTransationRow.TO * -1;
                    foreach (var lng in longfields)
                    {
                        stockTransationRow._value.Add(lng, reader[lng] == DBNull.Value ? 0 : Convert.ToInt64(reader[lng]));
                    }
                    foreach (var lng in datefields)
                    {
                        string value = reader[lng] == DBNull.Value ? "" : Convert.ToDateTime(reader[lng]).ToString("dd-MM-yyyy");
                        stockTransationRow.values[posfields[lng] - 1] = value;
                    }
                    foreach (var lng in numberfields)
                    {
                        string value = reader[lng] == DBNull.Value ? "" : Convert.ToDecimal(reader[lng]).ToString("dd-MM-yyyy");
                        stockTransationRow.values[posfields[lng] - 1] = value;
                    }
                    foreach (var lng in strfields)
                    {
                        string value = reader[lng] == DBNull.Value ? "" : Convert.ToString(reader[lng]);
                        stockTransationRow.values[posfields[lng] - 1] = value;
                    }
                    response.Add(stockTransationRow);
                }
            }

            foreach (var refquery in refquerylist)
            {
                var position = posfields[refquery.Key];
                var allids = response.ConvertAll(x => x._value[refquery.Key]).Distinct();
                if (allids.Count() > 0)
                {
                    SortedList<long, string> refvalue = new SortedList<long, string>();
                    query = refquery.Value + (refquery.Value.Contains(" where ") ? " and " : " where ") + " id in (" + String.Join(",", allids) + ")";

                    using (var reader = ExecuteReader(query, null))
                    {
                        while (reader.Read())
                        {
                            refvalue.Add(Convert.ToInt64(reader["id"]),
                                reader["name"] == DBNull.Value ? "" : reader["name"].ToString());
                        }
                    }
                    response.ForEach(x =>
                    {
                        var id = x._value[refquery.Key];
                        if (refvalue.ContainsKey(id))
                        {
                            x.values[position - 1] = refvalue[id];
                        }
                    });
                }
            }
            return new StockTransactionAsOn()
            {
                items = response,
                display = displayfields
            };
        }
        public StockTransactionAsOn SearchAsOnMfgStockTransaction(StockAsOnCriteria _reportcreteria)
        {

            List<StockTransactionAsOn.StockMfgTransactionRow> response = new List<StockTransactionAsOn.StockMfgTransactionRow>();
            string fromdate = _reportcreteria.fromdate.AddDays(1).ToString("yyyy-MM-dd");
            string rangefrom = _reportcreteria.fromdate.ToString("yyyy-MM-dd");
            string rangeto = _reportcreteria.todate.AddDays(1).ToString("yyyy-MM-dd");
            string query = @"with openingstock
            as
            (
            select sum(qty) opening,salablegoodsid from stocktransaction
            where salablegoodsid>0 and transactedon<'{0}'
            and stocktransaction.locationid = @locationid
            group by salablegoodsid
            ),sttransaction as
            (
            select * from crosstab(
            'select salablegoodsid,reftype,sum(qty) qty from stocktransaction
            where salablegoodsid>0 and transactedon>''{1}'' and transactedon<''{2}''
            and stocktransaction.locationid = @locationid
            group by salablegoodsid,reftype
            ORDER BY 1,2',
            'select * from unnest(ARRAY[''TI'', ''TO'',''PU'',''PR'',''SL'',''OP'',''AU'',''MS''])')
            as ct(salablegoodsid bigint,""TI"" numeric,""TO"" numeric,""PU"" numeric,""PR"" numeric,
	              ""SL"" numeric,""OP"" numeric,""AU"" numeric,""MS"" numeric)
            ),closingstock
            as
            (
                select salablegoods.id salablegoodsid,
                COALESCE(openingstock.opening,0) opening,""TI"",""TO"",""PU"",""PR"",""SL"",""OP"",""AU"",""MS"",
                COALESCE(openingstock.opening,0)+
                COALESCE(""TI"",0)+
                COALESCE(""TO"",0)+
                COALESCE(""PU"",0)+
                COALESCE(""PR"",0)+
                COALESCE(""SL"",0)+
                COALESCE(""OP"",0)+
                COALESCE(""AU"",0)+
                COALESCE(""MS"",0) closing
                from salablegoods
                left outer join sttransaction on salablegoodsid=salablegoods.id
                left outer join openingstock on openingstock.salablegoodsid = salablegoods.id
                where salablegoods.locationid = @locationid
            ),AsonStock as 
            (
                select stock.serialno,sum(opening) opening,sum(""TI"") xti,
                sum(""TO"") xto,sum(""PU"") xpu,sum(""PR"") xpr,sum(""SL"") xsl,sum(""OP"") xop,
                sum(""AU"") xau,sum(""MS"") xms,sum(closing) closing {3}
                from salablegoods
                inner join closingstock on salablegoodsid = salablegoods.id
                inner join stock on stock.id = salablegoods.stockid
                inner join items on items.id = stock.itemid
                inner join products on products.id = stock.productid
                left outer join lrinvoiceitems on lrinvoiceitems.id = stock.invoiceitemid and lrinvoiceitems.isactive = true
                left outer join lrinvoice on lrinvoice.id = lrinvoiceitems.lrinvoiceid and lrinvoice.isactive = true
                left outer join lrentry  on lrentry.id = lrid and lrentry.isactive = true
                left outer join supplier on supplier.id = stock.supplierid 
                left outer join suppliercompany on suppliercompany.supplierid = supplier.id
                where salablegoods.isactive=true {4}
                group by {5}
            ), alluid
            as
            (
                select AsonStock.*,mfgdesign.code,
                schemelist->>'uid' uid,cast(schemelist->>'sizeid' as bigint) sizeid,
                cast(schemelist->>'mrp' as numeric) mrp,
                schemelist->>'code' dname,schemelist->>'prdsku' prdsku,
                cast(schemelist->>'colourid' as bigint) colourid,
                mfgdesign.productid,mfgdesign.prddivisionid,mfgdesign.classid {6}
                from mfgdesign 
                LEFT JOIN LATERAL
                json_array_elements (cast(mfgdesign.sizegroup->>'items' as json)) schemelist ON true
                inner join AsonStock on AsonStock.serialno = schemelist->>'uid'
                where mfgdesign.isactive=true
            )
            select * from alluid ";
            List<String> fields = new List<string>();
            List<String> gfields = new List<string>();
            List<string> numberfields = new List<string>();
            List<string> datefields = new List<string>();
            List<string> longfields = new List<string>();
            List<string> strfields = new List<string>();
            List<string> conditions = new List<string>();
            List<int> displayfields = new List<int>();
            if (_reportcreteria.companyid > 0)
            {
                conditions.Add("salablegoods.companyid=@companyid");
            }
            if (_reportcreteria.locationid > 0)
            {
                conditions.Add("salablegoods.locationid=@locationid");
            }
            else
            {
                query = query.Replace("and salablegoods.locationid = @locationid", "");
                query = query.Replace("where stocktransaction.locationid = @locationid", "");
                query = query.Replace("and stocktransaction.locationid = @locationid", "");
            }

            gfields.Add("stock.serialno");
            if (_reportcreteria.conditions != null && _reportcreteria.conditions.Count > 0)
            {
                foreach(var cnd in _reportcreteria.conditions)
                {
                    var option = (InfyPOS.AnalyserDataService.Options)cnd.option;
                    var field = GetField(option);
                    if(option == Options.Design ||
                        option == Options.LREntryNo ||
                        option == Options.InvoiceNo ||
                        option == Options.BillNo ||
                        option == Options.Barcode ||
                        option == Options.HSNCode ||
                        option == Options.LRNo ||
                        option == Options.PurchaseDate ||
                        option == Options.BarcodedOn ||
                        option == Options.InvDate ||
                        option == Options.GLNNO)
                    {
                        conditions.Add(String.Format("{0}='{1}'", field,cnd.value));
                    }
                    else
                    {
                        conditions.Add(String.Format("{0}={1}", field, cnd.value));
                    }
                }
            }

            string condition = string.Join(" and ", conditions);
            SortedDictionary<string, int> posfields = new SortedDictionary<string, int>();
            SortedDictionary<string, string> refquerylist = new SortedDictionary<string, string>();
            var optionconfiguration = GetOptionConfiguration();
            foreach (var f in _reportcreteria.fields)
            {
                var field = GetField((InfyPOS.AnalyserDataService.Options)f);
                if (!string.IsNullOrEmpty(field))
                {
                    if (!optionconfiguration.ContainsKey((InfyPOS.AnalyserDataService.Options)f))
                    {
                        continue;
                    }
                    displayfields.Add(f);
                    string fieldname = "f" + (gfields.Count + 1);
                    posfields.Add(fieldname, gfields.Count + 1);
                    fields.Add(field + " as f" + (gfields.Count + 1));
                    gfields.Add(field);
                    if (optionconfiguration[(InfyPOS.AnalyserDataService.Options)f].conditiontype == Condtion.CondtionType.Date)
                    {
                        datefields.Add(fieldname);
                    }
                    else if (optionconfiguration[(InfyPOS.AnalyserDataService.Options)f].conditiontype == Condtion.CondtionType.Decimal)
                    {
                        numberfields.Add(fieldname);
                    }
                    else if (optionconfiguration[(InfyPOS.AnalyserDataService.Options)f].conditiontype == Condtion.CondtionType.Long)
                    {
                        longfields.Add(fieldname);
                    }
                    else
                    {
                        strfields.Add(fieldname);
                    }
                    var refquery = GetReferenceQuery((InfyPOS.AnalyserDataService.Options)f, "");
                    if (!string.IsNullOrEmpty(refquery))
                        refquerylist.Add(fieldname, refquery);
                }
            }

            List<string> additionafields = new List<string>();
            if (_reportcreteria.customfields != null)
                additionafields = _reportcreteria.customfields.ConvertAll(x => "mfg_" + x.field.cfname);

            query = string.Format(query, fromdate, rangefrom, rangeto,
                fields.Count == 0 ? "" : ("," + string.Join(",", fields)),
                string.IsNullOrEmpty(condition) ? "" : (" and " + condition),
                string.Join(",", gfields),
                additionafields.Count == 0 ? "" : "," + (string.Join(",", additionafields)));

            //if (gfields.Count == 0)
            //    query = query.Replace("group by", "");

            List<ReferenceItem> refdatalist = null;
            if (_reportcreteria.customfields != null)
            {
                var reffields = _reportcreteria.customfields.FindAll(x => !string.IsNullOrEmpty(x.field.cfref));
                if (reffields.Count > 0)
                {
                    ReferencelistDataService referencelistDataService = new ReferencelistDataService() { Context = this.Context };
                    refdatalist = referencelistDataService.SearchList(new ReferencelistCreteria()
                    {
                        types = reffields.ConvertAll(x => x.field.cfref)
                    });
                }
            }

            var param = new SortedList<string, object>();
            param.Add("companyid", _reportcreteria.companyid);
            param.Add("locationid", _reportcreteria.locationid);
            using (var reader = ExecuteReader(query, param,10000))
            {
                while (reader.Read())
                {
                    StockTransactionAsOn.StockMfgTransactionRow stockTransationRow = new StockTransactionAsOn.StockMfgTransactionRow()
                    {
                        _value = new SortedList<string, long>(),
                        values = new List<string>()
                    };
                    stockTransationRow.productid = reader["productid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productid"]);
                    stockTransationRow.classid = reader["classid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["classid"]);
                    stockTransationRow.colourid = reader["colourid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["colourid"]);
                    stockTransationRow.sizeid = reader["sizeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sizeid"]);
                    stockTransationRow.designname = Convert.ToString(reader["dname"]);
                    stockTransationRow.design = Convert.ToString(reader["code"]);
                    stockTransationRow.uid = Convert.ToString(reader["uid"]);
                    stockTransationRow.mrp = reader["mrp"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["mrp"]);
                    stockTransationRow.values.AddRange(Enumerable.Repeat("", fields.Count));
                    stockTransationRow.opening = reader["opening"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["opening"]);
                    stockTransationRow.closing = reader["closing"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["closing"]);
                    stockTransationRow.AU = reader["xau"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["xau"]);
                    stockTransationRow.MS = reader["xms"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["xms"]);
                    stockTransationRow.OP = reader["xop"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["xop"]);
                    stockTransationRow.PR = reader["xpr"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["xpr"]);
                    stockTransationRow.PU = reader["xpu"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["xpu"]);
                    stockTransationRow.SL = reader["xsl"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["xsl"]);
                    stockTransationRow.TI = reader["xti"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["xti"]);
                    stockTransationRow.TO = reader["xto"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["xto"]);
                    stockTransationRow.SL = stockTransationRow.SL * -1;
                    stockTransationRow.PR = stockTransationRow.PR * -1;
                    stockTransationRow.TO = stockTransationRow.TO * -1;
                    foreach (var lng in longfields)
                    {
                        stockTransationRow._value.Add(lng, reader[lng] == DBNull.Value ? 0 : Convert.ToInt64(reader[lng]));
                    }
                    foreach (var lng in datefields)
                    {
                        string value = reader[lng] == DBNull.Value ? "" : Convert.ToDateTime(reader[lng]).ToString("dd-MM-yyyy");
                        stockTransationRow.values[posfields[lng] - 1] = value;
                    }
                    foreach (var lng in numberfields)
                    {
                        string value = reader[lng] == DBNull.Value ? "" : Convert.ToDecimal(reader[lng]).ToString("dd-MM-yyyy");
                        stockTransationRow.values[posfields[lng] - 1] = value;
                    }
                    foreach (var lng in strfields)
                    {
                        string value = reader[lng] == DBNull.Value ? "" : Convert.ToString(reader[lng]);
                        stockTransationRow.values[posfields[lng] - 1] = value;
                    }

                    stockTransationRow.customfields = new List<string>();
                    if (_reportcreteria.customfields != null)
                    {
                        foreach (var cfield in _reportcreteria.customfields)
                        {
                            string svalue = "";
                            if (cfield.field.cftype == "bigint")
                            {
                                long value = reader["mfg_" + cfield.field.cfname] == DBNull.Value ? 0 : Convert.ToInt64(reader["mfg_" + cfield.field.cfname]);
                                if (refdatalist != null &&
                                    refdatalist.Exists(x => x.type == cfield.field.cfref &&
                                    x.id == value))
                                {
                                    svalue = refdatalist.Find(x => x.type == cfield.field.cfref &&
                                    x.id == value).name;
                                }
                            }
                            else if (cfield.field.cftype.StartsWith("varchar"))
                            {
                                svalue = reader["mfg_" + cfield.field.cfname] == DBNull.Value ? "" : Convert.ToString(reader["mfg_" + cfield.field.cfname]);
                            }
                            stockTransationRow.customfields.Add(svalue);
                        }
                    }
                    response.Add(stockTransationRow);
                }
            }

            foreach (var refquery in refquerylist)
            {
                var position = posfields[refquery.Key];
                var allids = response.ConvertAll(x => x._value[refquery.Key]).Distinct();
                if (allids.Count() > 0)
                {
                    SortedList<long, string> refvalue = new SortedList<long, string>();
                    query = refquery.Value + (refquery.Value.Contains(" where ") ? " and " : " where ") + " id in (" + String.Join(",", allids) + ")";

                    using (var reader = ExecuteReader(query, null))
                    {
                        while (reader.Read())
                        {
                            refvalue.Add(Convert.ToInt64(reader["id"]),
                                reader["name"] == DBNull.Value ? "" : reader["name"].ToString());
                        }
                    }
                    response.ForEach(x =>
                    {
                        var id = x._value[refquery.Key];
                        if (refvalue.ContainsKey(id))
                        {
                            x.values[position - 1] = refvalue[id];
                        }
                    });
                }
            }
            return new StockTransactionAsOn()
            {
                mfgitems = response,
                display = displayfields
            };
        }

        string GetField(InfyPOS.AnalyserDataService.Options option)

        {

            switch (option)
            {
                case InfyPOS.AnalyserDataService.Options.Barcode:
                    return "stock.barcode";
                case InfyPOS.AnalyserDataService.Options.GLNNO:
                    return "stock.serialno";
                case InfyPOS.AnalyserDataService.Options.Brand:
                    return "items.brandid";
                case InfyPOS.AnalyserDataService.Options.Colour:
                    return "items.colourid";
                case InfyPOS.AnalyserDataService.Options.Fit:
                    return "items.fitid";
                case InfyPOS.AnalyserDataService.Options.Sleeve:
                    return "items.sleeveid";
                case InfyPOS.AnalyserDataService.Options.PriceTag:
                    return "stock.pricetagid";
                case InfyPOS.AnalyserDataService.Options.Design:
                    return "stock.designid";
                case InfyPOS.AnalyserDataService.Options.InvDate:
                    return "lrinvoice.invoicedate";
                case InfyPOS.AnalyserDataService.Options.PurchaseDate:
                    return "lrinvoice.entrydate::date";
                case InfyPOS.AnalyserDataService.Options.BarcodedOn:
                    return "lrinvoiceitems.barcodedon::date";
                case InfyPOS.AnalyserDataService.Options.InvoiceNo:
                    return "lrinvoice.invoiceno";
                case InfyPOS.AnalyserDataService.Options.LREntryNo:
                    return "lrentry.lrentryno";
                case InfyPOS.AnalyserDataService.Options.EntryLocation:
                    return "lrentry.entrylocationid";
                case InfyPOS.AnalyserDataService.Options.Material:
                    return "items.materialid";
                case InfyPOS.AnalyserDataService.Options.Pattern:
                    return "items.patternid";
                case InfyPOS.AnalyserDataService.Options.Product:
                    return "stock.productid";
                case InfyPOS.AnalyserDataService.Options.ProductCode:
                    return "products.code";
                case InfyPOS.AnalyserDataService.Options.PurchaseRate:
                    return "lrinvoiceitems.buyingprice";
                case InfyPOS.AnalyserDataService.Options.DealerPrice:
                    return "stock.dealerprice";
                case InfyPOS.AnalyserDataService.Options.PurchaseTax:
                    return "lrinvoiceitems.purchasetaxpercentage";
                case InfyPOS.AnalyserDataService.Options.PurchaseYear:
                    return "date_part('year',lrinvoice.entrydate)";
                case InfyPOS.AnalyserDataService.Options.Company:
                    return "stock.companyid";
                case InfyPOS.AnalyserDataService.Options.ProductGroup:
                    return "products.productgroup";
                case InfyPOS.AnalyserDataService.Options.BarcodeGroup:
                    return "products.barcodeid";
                case InfyPOS.AnalyserDataService.Options.Section:
                    return "products.salescounterid";
                case InfyPOS.AnalyserDataService.Options.Size:
                    return "items.sizeid";
                case InfyPOS.AnalyserDataService.Options.Agent:
                    return "lrentry.agentid";
                case InfyPOS.AnalyserDataService.Options.StockLocation:
                    return "salablegoods.locationid";
                case InfyPOS.AnalyserDataService.Options.Style:
                    return "items.styleid";
                case InfyPOS.AnalyserDataService.Options.Supplier:
                    return "supplier.id";
                case InfyPOS.AnalyserDataService.Options.Source_Supplier:
                    return "stock.srcsupplierid";
                case InfyPOS.AnalyserDataService.Options.JobworkSupplier:
                    return "lrinvoiceitems.jobworkerid";
                case InfyPOS.AnalyserDataService.Options.Supplier_City:
                    return "suppliercompany.cityid";
                case InfyPOS.AnalyserDataService.Options.Type:
                    return "items.typeid";
                case InfyPOS.AnalyserDataService.Options.HSNCode:
                    return "stock.hsncode";
                case InfyPOS.AnalyserDataService.Options.Tax:
                    return "products.salestaxid";
            }
            return "";
        }

        internal List<StockTransactionAsOn> SaleVsPurchaseVsStock(StockAsOnCriteria item)
        {
            throw new NotImplementedException();
        }

        public class CustomField
        {
            public CustomField(string source)
            {
                var parts = source.Split(',');
                if (parts.Length == 4)
                {
                    cfname = parts[0];
                    referenceid = int.Parse(parts[1]);
                    cftype = parts[2];
                    referencetype = parts[3];
                }
                else
                {
                    isempty = true;
                }
            }
            public bool isempty { get; set; }
            public string cfname { get; set; }
            public int referenceid { get; set; }
            public string cftype { get; set; }
            public string referencetype { get; set; }
        }
        public class StockMarkerItems : BaseEntity
        {
            public string id { get; set; }
            public string name { get; set; }
            public decimal stockqty { get; set; }
            public decimal stockcost { get; set; }
            public decimal buyingprice { get; set; }
            public decimal sellingprice { get; set; }
            public decimal saleqty { get; set; }
            public decimal salevalue { get; set; }
        }
        public class Items : BaseEntity
        {
            public string id { get; set; }
            public string name { get; set; }
            public decimal qty { get; set; }
            public decimal value { get; set; }
            public decimal cqty { get; set; }
            public decimal cvalue { get; set; }
            public decimal sqty { get; set; }
            public decimal svalue { get; set; }
            public decimal iqty { get; set; }
            public decimal ivalue { get; set; }
            public decimal avalue { get; set; }
            public decimal aqty { get; set; }
            public decimal buyingcost { get; set; }
            public decimal sellingprice { get; set; }
            public decimal oqty { get; set; }
            public decimal ovalue { get; set; }
            //Return qty
            public decimal rqty { get; set; }
            // Return value
            public decimal rvalue { get; set; }

        }

        public class ResultItemGroups
        {
            [JsonIgnore]
            public List<int> excludeColumns { get; set; }
            public List<string> columns { get; set; }
            public List<AdditionalColumn> additionalcolumns { get; set; }
            public List<ResultItems> items { get; set; }
        }

        public class ImagePDFRequest : BaseEntity
        {
            public int RowCount { get; set; }
            public int ColumnCount { get; set; }
            public int DetailPosition { get; set; }
            public int ImageMode { get; set; }
            public bool HasRowLabel
            {
                get
                {
                    return Detail == EnumDetail.Top || Detail == EnumDetail.Bottom;
                }
            }
            public bool HasColumnLabel
            {
                get
                {
                    return Detail == EnumDetail.Left || Detail == EnumDetail.Right;
                }
            }
            public EnumDetail Detail
            {
                get
                {
                    return (EnumDetail)DetailPosition;
                }
                set
                {
                    DetailPosition = (int)value;
                }
            }
            public enum EnumDetail
            {
                None,
                Top,
                Bottom,
                Left,
                Right
            }
            public AnalyserDataService.ResultItemGroups datasource { get; set; }
        }
        public class ResultItems : Items
        {
            [JsonIgnore]
            public long[] Values { get; set; }
            public string[] String { get; set; }
            public string GroupBy { get; set; }
            public Priority priority { get; set; }

        }

        public class PPResultItems : ResultItems
        {
            public long[] DValues { get; set; }
        }

        public class Priority
        {
            public long id { get; set; }
            public decimal sqty { get; set; }
            public decimal pqty { get; set; }
            public decimal pratio { get; set; }
            public decimal sratio { get; set; }
            public decimal psratio { get; set; }
            public decimal salesvspsratio { get; set; }
            public decimal recommend { get; set; }
        }

        public class ResultGroup
        {
            public List<ResultGroupItems> Items { get; set; }
            public List<ColumnInfo> Columns { get; set; }
            public List<GroupInfo> Groups { get; set; }
            public List<ColumnInfo> ResultGroups { get; set; }
            public bool EnablePriority { get; set; }
            public Configuration Configuration { get; set; }
            [JsonIgnore]
            public PlanningDataSource.PurchasePlanConfig Request { get; set; }

            public class ColumnInfo
            {
                public int Index { get; set; }
                public string Header { get; set; }
            }

            public class GroupInfo : ColumnInfo
            {
                public long Value { get; set; }
            }
        }
        public class ResultGroupItems
        {
            [JsonIgnore]
            public string Key { get; set; }
            //public List<decimal> GroupValues { get; set; }
            public ResultItems[] GroupValues { get; set; }
            public List<string> String { get; set; }
            public List<long> Values { get; set; }
            public Priority priority { get; set; }
        }

        public class StockAsOnCriteria : BaseEntity
        {
            public DateTime fromdate { get; set; }
            public DateTime todate { get; set; }
            public long companyid { get; set; }
            public long locationid { get; set; }
            public bool history { get; set; }
            public List<int> fields { get; set; }
            public List<condition> conditions { get;set; }
            public List<CustomField> customfields { get; set; }
            public class CustomField
            {
                public string display { get; set; }
                public Referencelist.CustomField field { get; set; }
            }
            public class condition
            {
                public int option { get; set; }
                public string key { get; set; }
                public string value { get; set; }
                public string display { get; set; }
            }
        }
        public class StockTransactionAsOn : BaseEntity
        {
            public class History
            {
                public string mode { get; set; }
                public DateTime ason { get; set; }
                public long createdby { get; set; }
                public string username { get; set; }
                public decimal ob { get; set; }
                public decimal ti { get; set; }
                public decimal to { get; set; }
                public decimal cb { get; set; }
            }
            public List<StockTransactionRow> items { get; set; }
            public List<StockMfgTransactionRow> mfgitems { get; set; }
            public List<int> display { get; set; }

            public class StockMfgTransactionRow: StockTransactionRow
            {
                public long prddivisionid { get; set; }
                public long productid { get; set; }
                public long classid { get; set; }
                public long companyid { get; set; }
                public long locationid { get; set; }
                public string uid { get; set; }
                public string designname { get; set; }
                public string design { get; set; }
                public long colourid { get; set; }
                public long sizeid { get; set; }
                public decimal mrp { get; set; }
                public decimal price { get; set; }
                public List<string> customfields { get; set; }
            }
            public class StockTransactionRow
            {
                public decimal opening { get; set; }
                public decimal closing { get; set; }
                public decimal TI { get; set; }
                public decimal TO { get; set; }
                public decimal PU { get; set; }
                public decimal PR { get; set; }
                public decimal SL { get; set; }
                public decimal OP { get; set; }
                public decimal AU { get; set; }
                public decimal MS { get; set; }
                [JsonIgnore]
                public SortedList<string, long> _value { get; set; }
                public List<string> values { get; set; }
                public decimal closingcost { get;  set; }
                public decimal closingicost { get; set; }
                public decimal openingcost { get; internal set; }
                public decimal openingicost { get; internal set; }
            }
        }
    }

}
