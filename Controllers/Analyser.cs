using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace InfyPOS
{
    public class Analyser
    {
        public class Configuration
        {
            public enum ReportType
            {
                BranchStock,
                Stock,
                Sales
            }

            public ReportType reporttype { get; set; }
            public long locationid { get; set; }
            public Options PrimaryCondtion { get; set; }
            public Condtion.Range PrimaryRange { get; set; }
            public List<Condtion> SecondaryCondtions { get; set; }
            public void AddCondition(Condtion condition)
            {
                if (SecondaryCondtions == null)
                    SecondaryCondtions = new List<Condtion>();
                SecondaryCondtions.Add(condition);
                //PrimaryCondtion = condition.option;
                //PrimaryRange = condition.range;
            }

            public string CreateQuery()
            {
                OptionQueryBuilder optionquery = new OptionQueryBuilder(this);
                string query = optionquery.CreateBranchStockQuery();
                return query;
            }
        }
        public class Condtion
        {
            public CondtionType type { get; set; }
            public Table table { get; set; }
            public Options option { get; set; }
            public List<long> longList { get; set; }
            public List<DateTime> dateList { get; set; }
            public List<string> stringList { get; set; }
            public Range range { get; set; }
            public class Range
            {
                public decimal start { get; set; }
                public decimal end { get; set; }
                public decimal incrementor { get; set; }

                internal List<Range> getList()
                {
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

                        list.Add(rng);

                    }

                    list.Add(new Range()
                    {
                        start = this.end,
                        end = 0
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
                Decmal
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
            Supplier
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
            retList.Add(Options.Design, new OptionConfig(Options.Design, Table.Stock, Condtion.CondtionType.String));
            retList.Add(Options.DiscountRate, new OptionConfig(Options.DiscountRate, Table.Stock, Condtion.CondtionType.Decmal));
            retList.Add(Options.DiscountRate_Range, new OptionConfig(Options.DiscountRate_Range, Table.Stock, Condtion.CondtionType.Range));
            retList.Add(Options.InvDate, new OptionConfig(Options.InvDate, Table.LrInvoice, Condtion.CondtionType.Date));
            retList.Add(Options.LREntryNo, new OptionConfig(Options.InvoiceNo, Table.LrEntry, Condtion.CondtionType.String));
            retList.Add(Options.InvoiceNo, new OptionConfig(Options.InvoiceNo, Table.LrInvoice, Condtion.CondtionType.String));
            retList.Add(Options.SaleLocation, new OptionConfig(Options.SaleLocation, Table.Bill, Condtion.CondtionType.Long));
            retList.Add(Options.StockLocation, new OptionConfig(Options.StockLocation, Table.Salablegoods, Condtion.CondtionType.Long));
            retList.Add(Options.Material, new OptionConfig(Options.Material, Table.Item, Condtion.CondtionType.Long));
            retList.Add(Options.Pattern, new OptionConfig(Options.Pattern, Table.Item, Condtion.CondtionType.Long));
            retList.Add(Options.Product, new OptionConfig(Options.Product, Table.Stock, Condtion.CondtionType.Long));
            retList.Add(Options.PurchaseMonth, new OptionConfig(Options.PurchaseMonth, Table.LrInvoice, Condtion.CondtionType.String));
            retList.Add(Options.PurchaseRate, new OptionConfig(Options.PurchaseRate, Table.LrInvoiceItems, Condtion.CondtionType.Decmal));
            retList.Add(Options.PurchaseRate_Range, new OptionConfig(Options.PurchaseRate_Range, Table.LrInvoiceItems, Condtion.CondtionType.Range));
            retList.Add(Options.PurchaseYear, new OptionConfig(Options.PurchaseYear, Table.LrInvoice, Condtion.CondtionType.String));
            retList.Add(Options.SaleMonth, new OptionConfig(Options.SaleMonth, Table.Bill, Condtion.CondtionType.String));
            retList.Add(Options.SoldRate, new OptionConfig(Options.SaleRate, Table.BillItem, Condtion.CondtionType.Decmal));
            retList.Add(Options.SoldRate_Range, new OptionConfig(Options.SaleRate, Table.BillItem, Condtion.CondtionType.Decmal));
            retList.Add(Options.SaleRate, new OptionConfig(Options.SaleRate, Table.Salablegoods, Condtion.CondtionType.Decmal));
            retList.Add(Options.SaleRate_Range, new OptionConfig(Options.SaleRate_Range, Table.BillItem, Condtion.CondtionType.Range));
            retList.Add(Options.Salesman, new OptionConfig(Options.Salesman, Table.BillItem, Condtion.CondtionType.Long));
            retList.Add(Options.SaleYear, new OptionConfig(Options.SaleYear, Table.Bill, Condtion.CondtionType.String));
            retList.Add(Options.Section, new OptionConfig(Options.Section, Table.Product, Condtion.CondtionType.Long));
            retList.Add(Options.Size, new OptionConfig(Options.Size, Table.Item, Condtion.CondtionType.Long));
            retList.Add(Options.Style, new OptionConfig(Options.Style, Table.Item, Condtion.CondtionType.Long));
            retList.Add(Options.Supplier_City, new OptionConfig(Options.Supplier_City, Table.Supplier, Condtion.CondtionType.String));
            retList.Add(Options.Supplier, new OptionConfig(Options.Supplier, Table.LrInvoice, Condtion.CondtionType.String));
            retList.Add(Options.Type, new OptionConfig(Options.Type, Table.Item, Condtion.CondtionType.Long));
            retList.Add(Options.HSNCode, new OptionConfig(Options.Supplier, Table.LrInvoice, Condtion.CondtionType.String));
            retList.Add(Options.Tax, new OptionConfig(Options.Type, Table.Item, Condtion.CondtionType.Long));
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
            Salesman,
            Supplier_City,
            Barcode,
            HSNCode,
            Tax
        }

        public class OptionQueryBuilder
        {
            string branchStockQuery = @"select {0},sum(salablegoods.iqty-(salablegoods.oqty+salablegoods.jqty+salablegoods.dqty)) stock from salablegoods 
inner join stock on stock.id = salablegoods.stockid
left outer join products on products.id = stock.productid
left outer join items on items.id = stock.itemid
inner join lrinvoiceitems on lrinvoiceitems.id = stock.invoiceitemid and lrinvoiceitems.isactive=true
inner join lrinvoice on lrinvoice.id = lrinvoiceitems.lrinvoiceid  and lrinvoice.isactive = true
inner join lrentry  on lrentry.id = lrid
inner join supplier on supplier.id = lrentry.supplierid
where salablegoods.isactive = true and salablegoods.hasstock = true and salablegoods.iqty-(salablegoods.oqty+salablegoods.jqty+salablegoods.dqty)>0 {1}
group by {0}";

            string whStockQuery = @"select {0},sum(stock.qty - (stock.transfered+stock.returnqty+stock.damagedqty)) stock from stock
left outer join products on products.id = stock.productid
left outer join items on items.id = stock.itemid
inner join lrinvoiceitems on lrinvoiceitems.id = stock.invoiceitemid  and lrinvoiceitems.isactive = true
inner join lrinvoice on lrinvoice.id = lrinvoiceitems.lrinvoiceid  and lrinvoice.isactive = true
inner join lrentry  on lrentry.id = lrid
inner join supplier on supplier.id = lrentry.supplierid
where stock.isactive =true and stock.hasstock = true and stock.qty - (stock.transfered+stock.returnqty+stock.damagedqty)>0 {1}
group by {0}";

            Configuration config;
            public OptionQueryBuilder(Configuration config)
            {
                this.config = config;
            }

            public string CreateBranchStockQuery()
            {
                System.Text.StringBuilder WHERE = new System.Text.StringBuilder();
                if (config.SecondaryCondtions != null)
                {
                    foreach (var item in config.SecondaryCondtions)
                    {
                        WHERE.Append(" AND (" + GetCondtion(item) + ") ");
                    }
                }
                var GROUPBY = GetField(config.PrimaryCondtion, config.PrimaryRange);
                return string.Format(branchStockQuery, GROUPBY, WHERE.ToString());
            }


            string GetCondtion(Condtion condition)
            {
                switch (condition.option)
                {
                    case Options.Barcode:
                        return "stock.barcode in (" + string.Join(",", condition.stringList.ConvertAll(e => "'" + WebAPI.Data.Utility.SafeSQL(e) + "'")) + ")";
                    case Options.HSNCode:
                        return "stock.hsncode in (" + string.Join(",", condition.stringList.ConvertAll(e => "'" + WebAPI.Data.Utility.SafeSQL(e) + "'")) + ")";
                    case Options.BillDate:
                        return "bill.billdate::Date in (" + string.Join(",", condition.dateList.ConvertAll(e => "'" + e.ToString("yyyy-MM-dd") + "'")) + ")";
                    case Options.BillNo:
                        return "bill.billno in (" + string.Join(",", condition.stringList.ConvertAll(e => "'" + WebAPI.Data.Utility.SafeSQL(e) + "'")) + ")";
                    case Options.Brand:
                        return "item.brandid in (" + string.Join(",", condition.longList) + ")";
                    case Options.Colour:
                        return "item.brandid in (" + string.Join(",", condition.longList) + ")";
                    case Options.Design:
                        return "stock.designid in (" + string.Join(",", condition.stringList.ConvertAll(e => "'" + WebAPI.Data.Utility.SafeSQL(e) + "'")) + ")";
                    case Options.DiscountRate:
                        return "(stock.price-stock.discount) in (" + string.Join(",", condition.longList) + ")";
                    case Options.DiscountRate_Range:
                        return getRangeCondtion(condition, "(stock.price-stock.discount)");
                    case Options.InvDate:
                        return "lrinvoice.invoicedate::Date in (" + string.Join(",", condition.dateList.ConvertAll(e => "'" + e.ToString("yyyy-MM-dd") + "'")) + ")";
                    case Options.InvoiceNo:
                        return "lrinvoice.invoiceno in (" + string.Join(",", condition.stringList.ConvertAll(e => "'" + WebAPI.Data.Utility.SafeSQL(e) + "'")) + ")";
                    case Options.LREntryNo:
                        return "lrentry.lrentryno in (" + string.Join(",", condition.stringList.ConvertAll(e => "'" + WebAPI.Data.Utility.SafeSQL(e) + "'")) + ")";

                    case Options.Material:
                        return "item.materialid in (" + string.Join(",", condition.longList) + ")";
                    case Options.Pattern:
                        return "item.patternid in (" + string.Join(",", condition.longList) + ")";
                    case Options.Product:
                        return "stock.productid in (" + string.Join(",", condition.longList) + ")";
                    case Options.Tax:
                        return "products.salestaxid in (" + string.Join(",", condition.longList) + ")";

                }
                return "";
            }

            string GetField(Options option,Condtion.Range range)
            {
                switch (option)
                {
                    case Options.Barcode:
                        return "stock.barcode";
                    case Options.BillDate:
                        return "bill.billdate::Date";
                    case Options.BillNo:
                        return "bill.billno";
                    case Options.Brand:
                        return "item.brandid";
                    case Options.Colour:
                        return "item.colourid";
                    case Options.Design:
                        return "lrinvoiceitems.designid";
                    case Options.DiscountRate:
                        return "(stock.price-stock.discount)";
                    case Options.DiscountRate_Range:
                        return getRangeCase(range, "(stock.price-stock.discount)");
                    case Options.InvDate:
                        return "lrinvoice.invoicedate::Date";
                    case Options.InvoiceNo:
                        return "lrinvoice.invoiceno";
                    case Options.LREntryNo:
                        return "lrentry.lrentryno";
                    case Options.Material:
                        return "item.materialid";
                    case Options.Pattern:
                        return "item.patternid";
                    case Options.Product:
                        return "stock.productid";
                    case Options.HSNCode:
                        return "stock.hsncode";
                    case Options.Tax:
                        return "products.salestaxid";
                }
                return "";
            }

            private string getRangeCase(Condtion.Range rangecondition,string field)
            {
                var rangelist = rangecondition.getList();
                System.Text.StringBuilder conditionlist = new System.Text.StringBuilder();
                foreach (var rng in rangelist)
                {
                    if (rng.start == 0)
                    {
                        conditionlist.AppendLine("when "+field + ">=" + rng.end + " then "+rng.end);
                    }
                    else if (rng.end == 0)
                    {
                        conditionlist.AppendLine("when "+field + "<=" + rng.start +" then "+rng.start);
                    }
                    else
                    {
                        conditionlist.AppendLine("when "+field + " BETWEEN " + rng.start + " AND" + rng.end+" then "+rng.start+" - "+rng.end);
                    }

                }
                return " case " + conditionlist .ToString() + " end ";
            }
            private string getRangeCondtion(Condtion condition, string field)
            {
                var rangelist = condition.range.getList();
                List<string> conditionlist = new List<string>();
                foreach (var item in condition.longList)
                {
                    if (rangelist.Exists(e => e.start == item))
                    {
                        var rng = rangelist.Find(e => e.start == item);
                        if (rng.end == 0)
                        {
                            conditionlist.Add(field+">=" + rng.start);
                        }
                        else if (rng.start == 0)
                        {
                            conditionlist.Add(field + "<=" + rng.end);
                        }
                        else
                        {
                            conditionlist.Add(field + " BETWEEN " + rng.start + " AND" + rng.end);
                        }
                    }
                }
                return "(" + string.Join(" OR ", conditionlist) + ")";
            }

        }
    }
}