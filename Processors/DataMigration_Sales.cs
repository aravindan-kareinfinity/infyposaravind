using Npgsql;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;

namespace InfyPOS.Processors.Migration
{
    public class DataMigration_Sales
    {
        long organizationid;
        public DataMigration_Sales(long organizationid)
        {
            this.organizationid = organizationid;
        }

        public class Sales : IMigrationEntity
        {
            public long fitid { get; set; }
            public long sleeveid { get; set; }
            public long pricetagid { get; set; }

            private static CultureInfo enUS = new CultureInfo("en-US");
            public DateTime GetDate(List<string> source, SortedDictionary<string, MigrationManager.ColumnMap> columnMap, string name)
            {
                if (columnMap.ContainsKey(name) && columnMap[name].index >= 0)
                {
                    var value = source[columnMap[name].index];
                    if (!string.IsNullOrEmpty(value) && value.Length > 10)
                        value = value.Substring(0, 10);
                    DateTime dt;
                    if (DateTime.TryParseExact(value, "yyyy-MM-dd", enUS, DateTimeStyles.None, out dt))
                        return dt;
                    if (DateTime.TryParseExact(value, "dd-MM-yyyy", enUS, DateTimeStyles.None, out dt))
                        return dt;
                    double d;
                    if (double.TryParse(value, out d))
                        return DateTime.FromOADate(d);
                }
                return DateTime.MinValue;
            }

            public decimal GetDecimal(List<string> source, SortedDictionary<string, MigrationManager.ColumnMap> columnMap, string name)
            {
                if (columnMap.ContainsKey(name) && columnMap[name].index >= 0)
                {
                    var value = source[columnMap[name].index];
                    decimal dvalue;
                    if (decimal.TryParse(value, out dvalue))
                        return dvalue;
                }
                return 0;
            }

            private bool isEmpty(string value)
            {
                return string.IsNullOrEmpty(value) || value == "." || value == "NULL" || value == "[NULL]" || value == "None" || value == "[None]";
            }
            public string GetString(List<string> source, SortedDictionary<string, MigrationManager.ColumnMap> columnMap, string name)
            {
                if (columnMap.ContainsKey(name))
                {
                    if (columnMap[name].index >= 0 && source.Count > columnMap[name].index)
                        return isEmpty(source[columnMap[name].index]) ? "" : source[columnMap[name].index].Trim();

                    return isEmpty(columnMap[name].defaultvalue) ? "" : columnMap[name].defaultvalue;
                }
                return "";
            }

            public Sales(List<string> source, SortedDictionary<string, MigrationManager.ColumnMap> columnMap)
            {
                CompanyCode = GetString(source, columnMap, "company");
                LocationCode = GetString(source, columnMap, "location");

                cost = GetDecimal(source, columnMap, "cost");
                salqty = GetDecimal(source, columnMap, "salqty");
                salrate = GetDecimal(source, columnMap, "salrate");

                gln = GetString(source, columnMap, "gln");
                fit = GetString(source, columnMap, "fit");
                sleeve = GetString(source, columnMap, "sleeve");
                pricetag = GetString(source, columnMap, "pricetag");

                retqty = GetDecimal(source, columnMap, "retqty");
                retamount = GetDecimal(source, columnMap, "retamount");
                saldiscamount = GetDecimal(source, columnMap, "saldiscount");

                supplier = GetString(source, columnMap, "supplier");
                sourcesupplier = GetString(source, columnMap, "sourcesupplier");
                salesman = GetString(source, columnMap, "salesman");
                salesmancode = GetString(source, columnMap, "salesmancode");
                designno = GetString(source, columnMap, "designno");

                billid = GetString(source, columnMap, "billid");
                billno = GetString(source, columnMap, "billno");
                billdate = GetString(source, columnMap, "billdate");

                buyingdate = GetString(source, columnMap, "buyingdate");

                barcode = GetString(source, columnMap, "barcode");
                size = GetString(source, columnMap, "size");
                pattern = GetString(source, columnMap, "pattern");
                colour = GetString(source, columnMap, "colour");
                material = GetString(source, columnMap, "material");
                style = GetString(source, columnMap, "style");
                type = GetString(source, columnMap, "type");
                product = GetString(source, columnMap, "product");
                section = GetString(source, columnMap, "section");
                brand = GetString(source, columnMap, "brand");
                itemname = GetString(source, columnMap, "itemname");
                hsncode = GetString(source, columnMap, "hsncode");
                taxamt = GetDecimal(source, columnMap, "tax");

                invno = GetString(source, columnMap, "invno");
                invdate = GetDate(source, columnMap, "invdate");
                purentrydate = GetDate(source, columnMap, "purentrydate");
                pdiscount = GetDecimal(source, columnMap, "purdiscountamount");
                ptaxamount = GetDecimal(source, columnMap, "PurchaseTax1Amt");

                switch (GetString(source, columnMap, "barcodetype"))
                {
                    case "CUT":
                        sellingmode = 3;
                        break;
                    case "PACK":
                        sellingmode = 2;
                        break;
                    case "PIECE":
                        sellingmode = 2;
                        break;
                    default:
                        sellingmode = 2;
                        break;
                }

                salediscount = GetDecimal(source, columnMap, "salediscount");
                ptaxamount = GetDecimal(source, columnMap, "purchasetax");
                taxcode = GetString(source, columnMap, "tax");
                ptaxpercentage = GetDecimal(source, columnMap, "purchasetaxpercentage");
                saletaxpercentage = GetDecimal(source, columnMap, "saletaxpercentage");
                pdiscount = GetDecimal(source, columnMap, "purchasediscount");
                if (ptaxamount == 0 && ptaxpercentage > 0)
                    ptaxamount = Math.Round(cost / (100 + ptaxpercentage) * ptaxpercentage, 2);
            }

            public string hsncode { get; set; }
            public Sales(System.Data.DataRow dr)
            {
                LocationCode = dr.IsNull("LocationCode") ? String.Empty : dr["LocationCode"].ToString();
                CompanyCode = dr.IsNull("CompanyCode") ? String.Empty : dr["CompanyCode"].ToString();
                barcode = dr.IsNull("BARCODE") ? String.Empty : dr["BARCODE"].ToString();
                product = dr.IsNull("PRODUCTNAME") ? String.Empty : dr["PRODUCTNAME"].ToString();
                itemname = dr.IsNull("ITEMNAME") ? String.Empty : dr["ITEMNAME"].ToString();
                brand = dr.IsNull("Brand") ? String.Empty : dr["Brand"].ToString();
                type = dr.IsNull("Type") ? String.Empty : dr["Type"].ToString();
                style = dr.IsNull("Style") ? String.Empty : dr["Style"].ToString();
                pattern = dr.IsNull("Pattern") ? String.Empty : dr["Pattern"].ToString();
                colour = dr.IsNull("color") ? String.Empty : dr["color"].ToString();
                material = dr.IsNull("Material") ? String.Empty : dr["Material"].ToString();
                section = dr.IsNull("Section") ? String.Empty : dr["Section"].ToString();
                size = dr.IsNull("Size") ? String.Empty : dr["Size"].ToString();
                pricetag = dr.IsNull("PriceTag") ? String.Empty : dr["PriceTag"].ToString();
                designno = dr.IsNull("DESIGNNO") ? String.Empty : dr["DESIGNNO"].ToString();
                hsncode = dr.IsNull("HSNCODE") ? String.Empty : dr["HSNCODE"].ToString();

                supplier = dr.IsNull("SUPPLIER") ? String.Empty : dr["SUPPLIER"].ToString();
                invno = dr.IsNull("INVNO") ? String.Empty : dr["INVNO"].ToString();
                invdate = dr.IsNull("INVDATE") ? DateTime.MinValue : (DateTime)dr["INVDATE"];

                cost = dr.IsNull("cost") ? 0 : decimal.Parse(dr["cost"].ToString());
                salqty = dr.IsNull("salqty") ? 0 : decimal.Parse(dr["salqty"].ToString());
                salrate = dr.IsNull("salrate") ? 0 : decimal.Parse(dr["salrate"].ToString());

                retqty = dr.IsNull("retqty") ? 0 : decimal.Parse(dr["retqty"].ToString());
                retamount = dr.IsNull("retamount") ? 0 : decimal.Parse(dr["retamount"].ToString());
                saldiscamount = dr.IsNull("saldiscamount") ? 0 : decimal.Parse(dr["saldiscamount"].ToString());
                retamount = dr.IsNull("retamount") ? 0 : decimal.Parse(dr["retamount"].ToString());
                salesman = dr.IsNull("SALESMANNAME") ? string.Empty : dr["SALESMANNAME"].ToString();
                salesmancode = dr.IsNull("SALESMANCODE") ? string.Empty : dr["SALESMANCODE"].ToString();

                billid = dr.IsNull("billid") ? string.Empty : dr["billid"].ToString();
                billno = dr.IsNull("billno") ? string.Empty : dr["billno"].ToString();
                billdate = dr.IsNull("entrydate") ? string.Empty : dr["entrydate"].ToString();

                buyingdate = dr.IsNull("buyingdate") ? string.Empty : dr["buyingdate"].ToString();
                ptaxcode = dr.IsNull("taxcode") ? String.Empty : dr["taxcode"].ToString();
                taxcode = dr.IsNull("staxcode") ? String.Empty : dr["staxcode"].ToString();

                qty = dr.IsNull("qty") ? decimal.Parse("0") : decimal.Parse(dr["qty"].ToString());
                taxamt = dr.IsNull("SalesTax1Amt") ? 0 : decimal.Parse(dr["SalesTax1Amt"].ToString());


                if ((dr.IsNull("ITEMTYPECODE") || dr["ITEMTYPECODE"].ToString() == "" ? 0 : int.Parse(dr["ITEMTYPECODE"].ToString())) == 2)
                    sellingmode = 3;
                else if ((dr.IsNull("barcodetype") ? "" : dr["barcodetype"].ToString()) == "Batch" || (dr.IsNull("barcodetype") ? "" : dr["barcodetype"].ToString()) == "2")
                    sellingmode = 2;
                else
                    sellingmode = 1;

                salediscount = dr.IsNull("ActualSaleDiscamt") ? 0 : decimal.Parse(dr["ActualSaleDiscamt"].ToString());
                ptaxamount = dr.IsNull("PurchaseTax1Amt") ? 0 : decimal.Parse(dr["PurchaseTax1Amt"].ToString());
                taxcode = dr.IsNull("staxcode") ? String.Empty : dr["staxcode"].ToString();
                ptaxpercentage = dr.IsNull("taxpercentage") ? 0 : decimal.Parse(dr["taxpercentage"].ToString());
                saletaxpercentage = dr.IsNull("staxpercentage") ? 0 : decimal.Parse(dr["staxpercentage"].ToString());
                pdiscount = dr.IsNull("PurDiscountAmount") ? 0 : decimal.Parse(dr["PurDiscountAmount"].ToString());
                if (ptaxamount == 0 && ptaxpercentage > 0)
                    ptaxamount = Math.Round(cost / (100 + ptaxpercentage) * ptaxpercentage, 2);
            }

            public string suppliercode { get; set; }
            public decimal salediscount { get; set; }
            public decimal pdiscount { get; set; }
            public long invoiceitemid { get; set; }
            public long suppliercompanyid { get; set; }
            public int sellingmode { get; set; }
            public string CompanyCode { get; set; }
            public string LocationCode { get; set; }
            public string ptaxcode { get; set; }
            public long ptaxid { get; set; }
            public decimal taxamt { get; set; }
            public string taxcode { get; set; }

            public decimal qty { get; set; }
            public string billid { get; set; }
            public decimal ptaxpercentage { get; set; }
            public decimal saletaxpercentage { get; set; }
            public long saletaxid { get; set; }
            public long productid { get; set; }
            public long sectionid { get; set; }
            public long brandid { get; set; }
            public long itemid { get; set; }
            public string brand { get; set; }
            public string product { get; set; }
            public string itemname { get; set; }
            public string barcode { get; set; }
            public string invno { get; set; }
            public string section { get; set; }
            public DateTime invdate { get; set; }
            public DateTime purentrydate { get; set; }

            public string type { get; set; }
            public string style { get; set; }
            public string material { get; set; }
            public string colour { get; set; }
            public string pattern { get; set; }
            public decimal ptaxamount { get; set; }
            public long stockid { get; set; }
            public string billdate { get; set; }

            public string buyingdate { get; set; }
            public string billno { get; set; }
            public decimal cost { get; set; }

            public decimal salqty { get; set; }

            public decimal retqty { get; set; }

            public decimal retamount { get; set; }

            public long supplierid { get; set; }
            public long sourcesupplierid { get; set; }
            public decimal saldiscamount { get; set; }

            public string gln { get; set; }
            public string fit { get; set; }
            public string sleeve { get; set; }
            public string pricetag { get; set; }
            public decimal salrate { get; set; }
            public string salesman { get; set; }
            public string salesmancode { get; set; }
            public string designno { get; set; }
            public string size { get; set; }
            public string supplier { get; set; }
            public string sourcesupplier { get; set; }
            public long salableid { get; set; }
            public long colourid { get; set; }
            public long materialid { get; set; }
            public long patternid { get; set; }
            public long sizeid { get; set; }
            public long styleid { get; set; }
            public long typeid { get; set; }
            public long companyid { get; set; }
            public long locationid { get; set; }
        }


        internal NpgsqlConnection CreateConnection()
        {
            string connstring = System.Configuration.ConfigurationManager.ConnectionStrings["InfyPOS"].ConnectionString;
            return new NpgsqlConnection(connstring);
        }
        public static DateTime GetDataFromString(string source, string format)
        {
            CultureInfo enUS = new CultureInfo("en-US");
            DateTime dt;
            if (DateTime.TryParseExact(source, format, enUS, DateTimeStyles.None, out dt))
                return dt;
            return DateTime.MinValue;
        }


        public bool IsNumber(string value)
        {
            double number;
            return double.TryParse(value, out number);
        }
        public void MigrateSales(MigrationData data, SortedDictionary<string, MigrationManager.ColumnMap> columnmap, List<List<string>> rows)
        {

            List<Sales> sales = new List<Sales>();
            List<IMigrationEntity> salesinterface = new List<IMigrationEntity>();
            foreach (var dr in rows)
            {
                var sale = new Sales(dr, columnmap);
                sales.Add(sale);
                salesinterface.Add(sale);
            }

            InfyPOS.Data.EmployeeDataService employeeDataService = new InfyPOS.Data.EmployeeDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            var empList = employeeDataService.Search(new InfyPOS.Models.EmployeeCreteria()
            { });
            foreach (var salesman in sales.GroupBy(e => e.salesmancode))
            {
                if (!empList.Exists(e => e.employeecode == salesman.Key))
                {
                    var emp = employeeDataService.Insert(new InfyPOS.Models.Employee()
                    {
                        isactive = true,
                        version = 1,
                        employeecode = salesman.Key,
                        name = salesman.First().salesman
                    });
                    empList.Add(emp);
                }
            }

            SSProduct.UpdateLocationAndTax(organizationid, salesinterface, true);
            SSProduct.ProcessAll(organizationid, salesinterface);
            
            InfyPOS.Common.UserClaim context = new InfyPOS.Common.UserClaim();

            InfyPOS.Data.StockDataService stockdataservice = new InfyPOS.Data.StockDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            stockdataservice.Context = context;
            InfyPOS.Data.SalablegoodsDataService salabledataservice = new InfyPOS.Data.SalablegoodsDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            salabledataservice.Context = context;
            salabledataservice.isMigration = true;
            InfyPOS.Data.BillDataService billdataservice = new InfyPOS.Data.BillDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            billdataservice.Context = context;

            InfyPOS.Data.BillitemsDataService billitemdataservice = new InfyPOS.Data.BillitemsDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            billitemdataservice.Context = context;
            billitemdataservice.IsMigration = true;
            var barcodeList = SSProduct.GetBarcodeList(organizationid);
            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    SortedDictionary<string, InfyPOS.Models.Stock> stockList = new SortedDictionary<string, InfyPOS.Models.Stock>();
                    SortedDictionary<string, InfyPOS.Models.Salablegoods> goodsList = new SortedDictionary<string, InfyPOS.Models.Salablegoods>();

                    var stocklist = sales.Where(x => x.salqty != 0).GroupBy(e => new { e.barcode, e.locationid });
                    int count = stocklist.Count();
                    int ccount = 0;
                    foreach (var item in stocklist)
                    {
                        Sales firstitem = item.First();
                        InfyPOS.Models.Stock existingitem = null;
                        if (barcodeList.ContainsKey(firstitem.barcode))
                        {
                            if (stockList.ContainsKey(firstitem.barcode))
                                existingitem = stockList[firstitem.barcode];
                            else
                            {
                                existingitem = stockdataservice.Search(firstitem.companyid, firstitem.barcode, conn, transaction);
                                if (existingitem.id != 0)
                                    stockList.Add(firstitem.barcode, existingitem);
                                else
                                    existingitem = null;
                            }
                        }
                        InfyPOS.Models.Stock stock = new InfyPOS.Models.Stock();
                        stock.barcode = item.Key.barcode;
                        stock.hsncode = item.First().type;
                        stock.hsncode = IsNumber(stock.hsncode) ? stock.hsncode : "";
                        stock.buyingprice = firstitem.cost;
                        if (stock.buyingprice < 0)
                            stock.buyingprice = firstitem.salrate;
                        stock.designid = firstitem.designno;
                        stock.itemid = firstitem.itemid;
                        stock.discount = firstitem.salediscount;
                        stock.price = firstitem.salrate;
                        stock.printingname = firstitem.itemname;
                        stock.productid = firstitem.productid;
                        stock.qty = item.Sum(e => e.salqty);
                        stock.serialno = firstitem.gln;
                        //stock.returnqty = item.Sum(e => e.retqty) * -1;
                        stock.supplierid = firstitem.supplierid;
                        stock.transfered = stock.qty;
                        stock.lrreference = firstitem.invno;
                        //stock.invoicedate = GetDataFromString(firstitem.buyingdate, "dd/MM/yyyy");
                        stock.companyid = firstitem.companyid;
                        stock.stockdate = GetDataFromString(firstitem.buyingdate, "dd/MM/yyyy");
                        stock.isactive = true;
                        if (existingitem != null && existingitem.id > 0)
                        {
                            stock.id = existingitem.id;
                            existingitem.qty += item.Sum(e => e.salqty);
                            //existingitem.returnqty += item.Sum(e => e.retqty);
                            existingitem.transfered += stock.transfered;
                            stockdataservice.Update(transaction, conn, existingitem);
                        }
                        else
                        {
                            if (stock.buyingprice == 0)
                                stock.buyingprice = stock.price - stock.discount;
                            stockdataservice.Insert(transaction, conn, stock);
                            stockList.Add(stock.barcode, stock);
                        }
                        InfyPOS.Models.Salablegoods existingsgood = null;
                        if (goodsList.ContainsKey(firstitem.barcode + "~" + item.Key.locationid))
                        {
                            existingsgood = goodsList[firstitem.barcode + "~" + item.Key.locationid];
                        }
                        else if (barcodeList.ContainsKey(firstitem.barcode))
                        {
                            if (barcodeList[firstitem.barcode].Exists(e => e.locationid == item.Key.locationid))
                            {
                                var existingsgoodList = salabledataservice.Search(new InfyPOS.Models.SalablegoodsCreteria()
                                {
                                    id = barcodeList[firstitem.barcode].Find(e => e.locationid == item.Key.locationid).salableid
                                }, conn, transaction);

                                existingsgood = existingsgoodList[0];
                                goodsList.Add(firstitem.barcode + "~" + item.Key.locationid, existingsgood);
                            }
                        }

                        InfyPOS.Models.Salablegoods salable = new InfyPOS.Models.Salablegoods();
                        salable.stockid = stock.id;
                        salable.iqty = stock.qty;
                        salable.received = true;
                        salable.sellingname = firstitem.itemname;
                        salable.barcode = item.Key.barcode;
                        salable.itemid = stock.itemid;
                        salable.supplierid = firstitem.supplierid;
                        salable.inwardon = GetDataFromString(firstitem.buyingdate, "dd/MM/yyyy");
                        salable.companyid = firstitem.companyid;
                        salable.locationid = firstitem.locationid;
                        salable.price = stock.price;
                        salable.discount = stock.discount;
                        salable.isactive = true;
                        if (existingsgood != null)
                        {
                            existingsgood.iqty += stock.qty;
                            existingsgood.oqty += stock.qty;
                            salabledataservice.Update(transaction, conn, existingsgood, existingsgood);
                            salable.id = existingsgood.id;
                        }
                        else
                        {
                            while (true)
                            {
                                try
                                {
                                    salable.oqty = stock.qty;
                                    salable.costprice = stock.buyingprice;
                                    salable.icostprice = stock.ibuyingprice;
                                    salabledataservice.Insert(transaction, conn, salable);
                                    if (!barcodeList.ContainsKey(firstitem.barcode))
                                    {
                                        barcodeList.Add(firstitem.barcode, new List<InfyPOS.Data.StockDataService.BarcodeID>());
                                    }
                                    barcodeList[firstitem.barcode].Add(new InfyPOS.Data.StockDataService.BarcodeID()
                                    {
                                        barcode = firstitem.barcode,
                                        locationid = firstitem.locationid,
                                        salableid = salable.id,
                                        stockid = stock.id
                                    });
                                    break;
                                }
                                catch (Npgsql.NpgsqlException exp)
                                {

                                }
                            }
                        }
                        foreach (var stockitem in item)
                        {
                            stockitem.stockid = stock.id;
                            stockitem.salableid = salable.id;
                        }
                        ccount++;

                    }


                    ccount = 0;
                    var billlist = sales.Where(x => x.salqty != 0).GroupBy(e => new { e.billid, e.LocationCode, e.billdate });

                    count = billlist.Count();
                    List<string> billids = new List<string>();
                    bool billidbased = true;
                    foreach (var item in billlist)
                    {
                        try
                        {
                            if (!billidbased)
                            {
                                if (billids.Contains(item.First().billno))
                                {
                                    continue;
                                }
                                billids.Add(item.First().billno);
                            }
                            InfyPOS.Models.Bill bill = new InfyPOS.Models.Bill();
                            bill.billno = billidbased ? item.First().billid : item.First().billno;
                            bill.billdate = GetDataFromString(item.First().billdate, "dd/MM/yyyy");
                            bill.tax = item.Sum(e => e.taxamt);
                            bill.discount = item.Sum(e => e.saldiscamount);
                            bill.receivable = item.Sum(e => ((e.salrate - e.saldiscamount / e.salqty) * e.salqty));
                            bill.rounding = ((decimal)((int)bill.receivable)) - bill.receivable;
                            //bill.receivable = bill.total-bill.discount;
                            bill.qty = item.Sum(e => e.salqty);
                            bill.isactive = true;
                            bill.settled = true;
                            bill.companyid = item.First().companyid;
                            bill.Billitems = new List<InfyPOS.Models.Billitems>();
                            bill.locationid = item.First().locationid;
                            billdataservice.Insert(transaction, conn, bill, new Models.Billmaster()
                            {
                                id = bill.billmasterid,
                                total = bill.total
                            });
                            foreach (var bitem in item)
                            {
                                InfyPOS.Models.Billitems billitem = new InfyPOS.Models.Billitems();
                                billitem.barcode = bitem.barcode;
                                billitem.price = bitem.salrate - (bitem.saldiscamount / bitem.salqty);
                                billitem.printingname = bitem.itemname;
                                billitem.discount = bitem.saldiscamount;
                                billitem.qty = bitem.salqty;
                                billitem.tax = bitem.taxamt;
                                billitem.taxid = bitem.saletaxid;
                                billitem.receivable = bitem.qty * billitem.price;
                                billitem.salablegoodsid = bitem.salableid;
                                billitem.salesmanname = bitem.salesman;
                                billitem.salesmanid = empList.Exists(e => e.employeecode == bitem.salesmancode) ?
                                    empList.Find(e => e.employeecode == bitem.salesmancode).id : 0;
                                billitem.stockid = bitem.stockid;
                                billitem.billid = bill.id;
                                billitem.hsncode = item.First().type;
                                billitem.hsncode = IsNumber(billitem.hsncode) ? billitem.hsncode : "";
                                billitem.isactive = true;
                                billitem.rate = billitem.price - billitem.tax / bitem.qty;
                                //(billitem.qty == 0 || bitem.salrate == 0) ? 0 : Math.Round((bitem.salrate- (bitem.saldiscamount)) - (bitem.taxamt / billitem.qty), 2);
                                billitem.saleamount = billitem.rate * billitem.qty;
                                billitem.salerate = billitem.price + (bitem.saldiscamount / bitem.salqty);
                                billitem.itemtax = Math.Round(billitem.tax / billitem.qty, 4);
                                billitem.piece = item.First().sellingmode == 3 ? 1 : billitem.qty;
                                billitem.taxpercentage = bitem.saletaxpercentage; //billitem.rate>0 ? Math.Round(billitem.itemtax / billitem.rate * 100, 0) : 0;
                                if (billitem.discount + (billitem.price * billitem.qty) > 0)
                                    billitem.discountpercentage = Math.Round(billitem.discount / (billitem.discount + (billitem.price * billitem.qty)) * 100, 0);
                                billitemdataservice.Insert(transaction, conn, bill, billitem, 0);
                                data.progress.completedrows++;
                            }
                            ccount++;
                        }
                        catch (Exception exp)
                        {
                            data.progress.failedrows += item.Count();
                            data.progress.ExceptionList.Add(exp);
                        }
                    }

                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                data.progress.Exception = exp;
            }

        }
    }
}