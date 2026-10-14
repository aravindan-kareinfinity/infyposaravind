using Npgsql;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;

namespace InfyPOS.Processors.Migration
{
    public class DataMigration_Stock
    {
        long organizationid;
        public DataMigration_Stock(long organizationid)
        {
            this.organizationid = organizationid;
        }
        public class Stock : IMigrationEntity
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

            public Stock(List<string> source, SortedDictionary<string, MigrationManager.ColumnMap> columnMap)
            {
                LocationCode = GetString(source, columnMap, "locationcode");
                CompanyCode = GetString(source, columnMap, "companycode");

                barcode = GetString(source, columnMap, "barcode");
                productname = GetString(source, columnMap, "productname").Trim();
                product = productname;
                brand = GetString(source, columnMap, "brand");
                gln = GetString(source, columnMap, "gln");
                fit = GetString(source, columnMap, "fit");
                sleeve = GetString(source, columnMap, "sleeve");
                pricetag = GetString(source, columnMap, "pricetag");

                type = GetString(source, columnMap, "type");
                style = GetString(source, columnMap, "style");
                pattern = GetString(source, columnMap, "pattern");
                colour = GetString(source, columnMap, "colour");
                material = GetString(source, columnMap, "material");
                section = GetString(source, columnMap, "section");
                size = GetString(source, columnMap, "size");
                designno = GetString(source, columnMap, "designno");
                itbuscode = GetString(source, columnMap, "itbuscode");
                hsncode = GetString(source, columnMap, "hsncode");
                long hsnno = 0;
                if (!long.TryParse(hsncode, out hsnno))
                    hsncode = "";

                supplier = GetString(source, columnMap, "supplier");
                sourcesupplier = GetString(source, columnMap, "sourcesupplier");
                supcity = GetString(source, columnMap, "supcity");
                supstate = GetString(source, columnMap, "supstate");
                agent = GetString(source, columnMap, "agent");
                invno = GetString(source, columnMap, "invno");
                invdate = GetDate(source, columnMap, "invdate");
                purentrydate = GetDate(source, columnMap, "purentrydate");

                cost = GetDecimal(source, columnMap, "cost");
                pdiscount = GetDecimal(source, columnMap, "purchasediscount");
                ptaxamount = GetDecimal(source, columnMap, "purchasetax1amt");
                wsrate = GetDecimal(source, columnMap, "wsrate");
                mrp = GetDecimal(source, columnMap, "mrp");
                ptaxpercentage = GetDecimal(source, columnMap, "purchasetaxpercentage");

                salerate = GetDecimal(source, columnMap, "salerate");
                salediscount = GetDecimal(source, columnMap, "salediscount");

                saletaxpercentage = GetDecimal(source, columnMap, "taxpercentage");

                purqty = GetDecimal(source, columnMap, "purqty");
                salqty = GetDecimal(source, columnMap, "salqty");
                retqty = GetDecimal(source, columnMap, "retqty");
                closingqty = GetDecimal(source, columnMap, "closingqty");
                if (closingqty > 0 && purqty == 0)
                    purqty = closingqty;

                if (GetString(source, columnMap, "barcodetype").ToLower() == "true"
                    || GetString(source, columnMap, "barcodetype").ToLower() == "meters"
                    || GetString(source, columnMap, "barcodetype").ToLower() == "meter"
                    || GetString(source, columnMap, "barcodetype").ToLower() == "metres"
                    || GetString(source, columnMap, "barcodetype").ToLower() == "metre")
                {
                    sellingmode = 3;
                }
                else
                {
                    sellingmode = 2;
                }
                taxcode = saletaxpercentage.ToString();
            }
            public Stock(System.Data.DataRow dr)
            {
                LocationCode = dr.IsNull("LocationCode") ? String.Empty : dr["LocationCode"].ToString();
                CompanyCode = dr.IsNull("CompanyCode") ? String.Empty : dr["CompanyCode"].ToString();

                barcode = dr.IsNull("BARCODE") ? String.Empty : dr["BARCODE"].ToString();
                productname = dr.IsNull("PRODUCTNAME") ? String.Empty : dr["PRODUCTNAME"].ToString();
                product = productname;
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
                itbuscode = dr.IsNull("ITEMCODE") ? String.Empty : dr["ITEMCODE"].ToString();
                hsncode = dr.IsNull("HSNCODE") ? String.Empty : dr["HSNCODE"].ToString();

                supplier = dr.IsNull("SUPPLIER") ? String.Empty : dr["SUPPLIER"].ToString();
                sourcesupplier = dr.IsNull("SOURCESUPPLIER") ? String.Empty : dr["SOURCESUPPLIER"].ToString();
                supcity = dr.IsNull("SUPCITY") ? String.Empty : dr["SUPCITY"].ToString();
                supgroup = dr.IsNull("SUPGROUP") ? String.Empty : dr["SUPGROUP"].ToString();
                agent = dr.IsNull("AGENT") ? String.Empty : dr["AGENT"].ToString();
                invno = dr.IsNull("INVNO") ? String.Empty : dr["INVNO"].ToString();
                invdate = dr.IsNull("INVDATE") ? DateTime.MinValue : (DateTime)dr["INVDATE"];

                salerate = dr.IsNull("SALERATE") ? 0 : decimal.Parse(dr["SALERATE"].ToString());
                purentrydate = dr.IsNull("PURENTRYDATE") ? DateTime.MinValue : (DateTime)dr["PURENTRYDATE"];
                openingqty = dr.IsNull("OPENINGQTY") ? 0 : decimal.Parse(dr["OPENINGQTY"].ToString());

                if ((dr.IsNull("ITEMTYPECODE") || dr["ITEMTYPECODE"].ToString() == "" ? 0 : int.Parse(dr["ITEMTYPECODE"].ToString())) == 2)
                    sellingmode = 3;
                else if ((dr.IsNull("barcodetype") ? "" : dr["barcodetype"].ToString()) == "Batch" || (dr.IsNull("barcodetype") ? "" : dr["barcodetype"].ToString()) == "2")
                    sellingmode = 2;
                else
                    sellingmode = 1;

                purqty = dr.IsNull("PURQTY") ? 0 : decimal.Parse(dr["PURQTY"].ToString());
                if (purqty == 0 && openingqty > 0)
                    purqty = openingqty;

                salretqty = dr.IsNull("SALRETQTY") ? 0 : decimal.Parse(dr["SALRETQTY"].ToString());
                salinvretqty = salretqty;
                prretqty = dr.IsNull("PRRETQTY") ? 0 : decimal.Parse(dr["PRRETQTY"].ToString());
                salinvqty = dr.IsNull("SALINVQTY") ? 0 : decimal.Parse(dr["SALINVQTY"].ToString());
                salqty = salinvqty;
                inwqty = dr.IsNull("INWQTY") ? 0 : decimal.Parse(dr["INWQTY"].ToString());
                ouwqty = dr.IsNull("OUWQTY") ? 0 : decimal.Parse(dr["OUWQTY"].ToString());
                closingqty = dr.IsNull("CLOSINGQTY") ? 0 : decimal.Parse(dr["CLOSINGQTY"].ToString());
                cost = dr.IsNull("cost") ? 0 : decimal.Parse(dr["cost"].ToString());
                ptaxcode = dr.IsNull("taxcode") ? String.Empty : dr["taxcode"].ToString();

                ptaxpercentage = saletaxpercentage = dr.IsNull("taxpercentage") ? 0 : decimal.Parse(dr["taxpercentage"].ToString());
                salediscount = dr.IsNull("SALEDISCOUNT") ? 0 : decimal.Parse(dr["SALEDISCOUNT"].ToString());
                //if (salediscount > 0)
                //    salediscount = salerate - salediscount;

                pdiscount = dr.IsNull("PurDiscountAmount") ? 0 : decimal.Parse(dr["PurDiscountAmount"].ToString());

                ptaxamount = dr.IsNull("PurchaseTax1Amt") ? 0 : decimal.Parse(dr["PurchaseTax1Amt"].ToString());
                taxcode = dr.IsNull("staxcode") ? String.Empty : dr["staxcode"].ToString();
            }
            public int sellingmode { get; set; }
            public long saletaxid { get; set; }
            public decimal pdiscount { get; set; }
            public decimal ptaxamount { get; set; }
            public string ptaxcode { get; set; }
            public long ptaxid { get; set; }
            public string taxcode { get; set; }
            public decimal saletaxpercentage { get; set; }
            public decimal ptaxpercentage { get; set; }
            public decimal cost { get; set; }
            public string material { get; set; }
            public string section { get; set; }
            public string size { get; set; }
            public string supplier { get; set; }
            public string sourcesupplier { get; set; }
            public string supcity { get; set; }
            public string supstate { get; set; }
            public string supgroup { get; set; }
            public string agent { get; set; }
            public string invno { get; set; }
            public DateTime invdate { get; set; }
            public string designno { get; set; }
            public string itbuscode { get; set; }
            public long invoiceitemid { get; set; }
            public decimal salerate { get; set; }
            public decimal wsrate { get; set; }
            public decimal mrp { get; set; }
            public DateTime purentrydate { get; set; }
            public long suppliercompanyid { get; set; }
            public decimal purdays { get; set; }
            public decimal openingqty { get; set; }
            public decimal purqty { get; set; }
            public decimal salretqty { get; set; }
            public decimal salinvretqty { get; set; }
            public string hsncode { get; set; }
            public decimal prretqty { get; set; }
            public decimal salqty { get; set; }
            public decimal salinvqty { get; set; }
            public decimal inwqty { get; set; }
            public decimal ouwqty { get; set; }
            public decimal closingqty { get; set; }
            public decimal retqty { get; set; }
            public string barcode { get; set; }
            public string productname { get; set; }
            public string itemname { get; set; }
            public string brand { get; set; }
            public string gln { get; set; }
            public string fit { get; set; }
            public string sleeve { get; set; }
            public string pricetag { get; set; }
            public string type { get; set; }
            public string style { get; set; }
            public string pattern { get; set; }
            public string colour { get; set; }

            public long brandid { get; set; }

            public long itemid { get; set; }
            public string product { get; set; }
            public long productid { get; set; }
            public long sectionid { get; set; }
            public long supplierid { get; set; }
            public long sourcesupplierid { get; set; }
            public long colourid { get; set; }
            public long materialid { get; set; }

            public long patternid { get; set; }

            public long sizeid { get; set; }
            public long typeid { get; set; }
            public long styleid { get; set; }

            public long locationid { get; set; }
            public string CompanyCode { get; set; }
            public long companyid { get; set; }
            public string LocationCode { get; set; }
            public decimal salediscount { get; set; }
            public long lrinvoiceid { get; set; }

            public string suppliercode { get; set; }
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
        public void MigrateStock(MigrationData data, SortedDictionary<string, MigrationManager.ColumnMap> columnmap, List<List<string>> rows)
        {
            List<Stock> sales = new List<Stock>();
            List<IMigrationEntity> salesinterface = new List<IMigrationEntity>();
            int productmissing = 0;
            int pricemissing = 0;
            foreach (var dr in rows)
            {
                var sale = new Stock(dr, columnmap);
                if (string.IsNullOrEmpty(sale.product) || (string.IsNullOrEmpty(sale.barcode) && string.IsNullOrEmpty(sale.gln)))
                {
                    productmissing++;
                    continue;
                }

                if (sale.cost <= 0)
                {
                    if (sale.salerate > 0)
                    {
                        sale.cost = sale.salerate;
                    }
                    else
                    {
                        sale.cost = 1;
                        sale.salerate = 0;
                        pricemissing++;
                        //continue;
                    }
                }

                if (sale.salerate <= 0)
                    sale.salerate = 1;

                sales.Add(sale);
                salesinterface.Add(sale);
            }

            data.progress.status = "Updating Location/Tax";
            SSProduct.UpdateLocationAndTax(organizationid, salesinterface, false);
            data.progress.status = "Processing item attributes";
            SSProduct.ProcessAll(organizationid, salesinterface);
            data.progress.status = "Processing item attributes done";
            InfyPOS.Common.UserClaim context = new InfyPOS.Common.UserClaim();
            InfyPOS.Data.TransportDataService transdataservice = new InfyPOS.Data.TransportDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            transdataservice.Context = context;
            InfyPOS.Data.ReferencelistDataService refrdataservice = new InfyPOS.Data.ReferencelistDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            refrdataservice.Context = context;
            refrdataservice.isMigration = true;
            var citylist = refrdataservice.Search(new InfyPOS.Models.ReferencelistCreteria()
            {
                type = "CITY"
            });

            var loclist = refrdataservice.Search(new InfyPOS.Models.ReferencelistCreteria()
            {
                type = "LOCATION"
            });

            foreach (var city in sales.GroupBy(e => e.supcity))
            {
                if (!citylist.Exists(e => e.name == city.Key))
                {
                    InfyPOS.Models.Referencelist mref = new InfyPOS.Models.Referencelist();
                    mref.code = city.Key;
                    mref.name = city.Key;
                    mref.isactive = true;
                    mref.type = "CITY";
                    refrdataservice.Insert(mref);
                    citylist.Add(mref);
                }
            }
            var statelist = refrdataservice.Search(new InfyPOS.Models.ReferencelistCreteria()
            {
                type = "STATE"
            });
            foreach (var city in sales.GroupBy(e => e.supstate))
            {
                if (!statelist.Exists(e => e.name == city.Key))
                {
                    InfyPOS.Models.Referencelist mref = new InfyPOS.Models.Referencelist();
                    mref.code = city.Key;
                    mref.name = city.Key;
                    mref.isactive = true;
                    mref.type = "STATE";
                    refrdataservice.Insert(mref);
                    statelist.Add(mref);
                }
            }


            var translist = transdataservice.Search(new InfyPOS.Models.TransportCreteria()
            {
            });

            InfyPOS.Data.StockDataService stockdataservice = new InfyPOS.Data.StockDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            stockdataservice.Context = context;
            InfyPOS.Data.SalablegoodsDataService salabledataservice = new InfyPOS.Data.SalablegoodsDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            salabledataservice.isMigration = true;
            salabledataservice.Context = context;
            InfyPOS.Data.BillDataService billdataservice = new InfyPOS.Data.BillDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            billdataservice.Context = context;
            InfyPOS.Data.LrinvoiceDataService invoicedata = new InfyPOS.Data.LrinvoiceDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            invoicedata.Context = context;
            InfyPOS.Data.LrinvoiceitemsDataService invoiceitemsdata = new InfyPOS.Data.LrinvoiceitemsDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            invoiceitemsdata.Context = context;
            invoiceitemsdata.IsMigration = true;
            InfyPOS.Data.LrentryDataService lrentrydata = new InfyPOS.Data.LrentryDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            lrentrydata.Context = context;
            InfyPOS.Data.LrinvoicetaxDataService lrinvoicetax = new InfyPOS.Data.LrinvoicetaxDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            lrinvoicetax.Context = context;
            InfyPOS.Data.SellablebarcodesDataService sellablebarcodesDataService = new InfyPOS.Data.SellablebarcodesDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            sellablebarcodesDataService.Context = context;
            InfyPOS.Data.SalablegoodspriceDataService salablegoodspriceDataService = new Data.SalablegoodspriceDataService();
            salablegoodspriceDataService.Context = context;

            var barcodeList = SSProduct.GetBarcodeList(organizationid);
            NpgsqlTransaction transaction = null;
            try
            {
                data.progress.status = "Stock Migration started";
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    var invoicelist = sales.GroupBy(e => new { e.supplierid, e.invdate, e.invno });
                    foreach (var item in invoicelist)
                    {
                        Stock firstitem = item.First();

                        InfyPOS.Models.Lrentry lrentry = new InfyPOS.Models.Lrentry();
                        lrentry.supplierid = firstitem.supplierid;
                        lrentry.sourcesupplierid = firstitem.sourcesupplierid;
                        lrentry.lrdate = firstitem.invdate;
                        lrentry.lrno = "OLR-" + firstitem.invno;
                        lrentry.modeofdelivery = 2;
                        lrentry.receiveddate = firstitem.invdate;
                        lrentry.totalamount = item.Sum(e => e.purqty * (e.cost + e.ptaxamount)) - item.Sum(e => e.pdiscount * e.purqty);
                        lrentry.isactive = true;
                        lrentry.lrentryno = firstitem.invno;
                        lrentrydata.Insert(transaction, conn, lrentry);

                        InfyPOS.Models.Lrinvoice lrinvoice = new InfyPOS.Models.Lrinvoice();
                        lrinvoice.companyid = firstitem.companyid;
                        lrinvoice.invoicedate = item.Key.invdate;
                        lrinvoice.entrydate = (firstitem.purentrydate == DateTime.MinValue ? item.Key.invdate : firstitem.purentrydate);
                        lrinvoice.invoiceno = item.Key.invno;
                        lrinvoice.isactive = true;
                        lrinvoice.lrid = lrentry.id;
                        lrinvoice.suppliercompanyid = firstitem.suppliercompanyid;
                        lrinvoice.supplierid = firstitem.supplierid;
                        lrinvoice.discount = item.Sum(e => e.pdiscount * e.purqty);
                        lrinvoice.totalamount = item.Sum(e => e.purqty * (e.cost)) - lrinvoice.discount;
                        lrinvoice.grossamount = item.Sum(e => e.purqty * (e.cost - e.ptaxamount));
                        lrinvoice.taxamount = item.Sum(e => e.purqty * (e.ptaxamount));
                        lrinvoice.Lrinvoiceitems = new List<InfyPOS.Models.LrinvoiceItemDetail>();
                        lrinvoice.Lrinvoicetaxitems = new List<InfyPOS.Models.Lrinvoicetax>();
                        List<Tuple<InfyPOS.Models.LrinvoiceItemDetail, Stock>> mapitems = new List<Tuple<InfyPOS.Models.LrinvoiceItemDetail, Stock>>();
                        foreach (var invoiceitem in item)
                        {
                            var baseprice = invoiceitem.cost - invoiceitem.ptaxamount;
                            if (lrinvoice.Lrinvoicetaxitems.Exists(e => e.taxid == invoiceitem.ptaxid))
                            {
                                lrinvoice.Lrinvoicetaxitems.Find(e => e.taxid == invoiceitem.ptaxid).amounton += (invoiceitem.purqty * baseprice);
                                lrinvoice.Lrinvoicetaxitems.Find(e => e.taxid == invoiceitem.ptaxid).taxamount += invoiceitem.ptaxpercentage == 0 ? 0 : Math.Round(invoiceitem.purqty * baseprice * invoiceitem.ptaxpercentage / 100, 2);
                                lrinvoice.Lrinvoicetaxitems.Find(e => e.taxid == invoiceitem.ptaxid).discount += (invoiceitem.pdiscount * invoiceitem.purqty);
                                lrinvoice.Lrinvoicetaxitems.Find(e => e.taxid == invoiceitem.ptaxid).netamount += (invoiceitem.purqty * invoiceitem.cost);
                            }
                            else
                            {
                                lrinvoice.Lrinvoicetaxitems.Add(new InfyPOS.Models.Lrinvoicetax()
                                {
                                    taxid = invoiceitem.ptaxid,
                                    amounton = invoiceitem.purqty * baseprice,
                                    taxamount = invoiceitem.ptaxpercentage == 0 ? 0 : Math.Round(invoiceitem.purqty * baseprice * invoiceitem.ptaxpercentage / 100, 2),
                                    discount = invoiceitem.pdiscount * invoiceitem.purqty,
                                    isactive = true,
                                    percentage = invoiceitem.ptaxpercentage,
                                    netamount = (invoiceitem.purqty * invoiceitem.cost)
                                });
                            }

                            InfyPOS.Models.LrinvoiceItemDetail invoiceitemdetail = new InfyPOS.Models.LrinvoiceItemDetail();
                            invoiceitemdetail.basebuyingprice = invoiceitem.cost + invoiceitem.pdiscount - invoiceitem.ptaxamount;
                            invoiceitemdetail.buyingprice = invoiceitem.cost - invoiceitem.ptaxamount;
                            invoiceitemdetail.buyingqty = invoiceitem.purqty == 0 ? invoiceitem.closingqty : invoiceitem.purqty;
                            invoiceitemdetail.sellableqty = invoiceitemdetail.buyingqty;
                            invoiceitemdetail.designid = invoiceitem.designno;
                            invoiceitemdetail.itbuscode = invoiceitem.itbuscode;
                            invoiceitemdetail.pricetagid = invoiceitem.pricetagid;
                            invoiceitemdetail.itemid = invoiceitem.itemid;
                            invoiceitemdetail.mrp = invoiceitem.salerate;
                            invoiceitemdetail.hsncode = invoiceitem.hsncode;
                            invoiceitemdetail.productid = invoiceitem.productid;
                            invoiceitemdetail.ptaxid = invoiceitem.ptaxid;
                            invoiceitemdetail.purchasediscount = invoiceitem.pdiscount;
                            invoiceitemdetail.purchasetax = invoiceitem.ptaxamount;
                            invoiceitemdetail.purchasetaxpercentage = invoiceitem.ptaxpercentage;
                            invoiceitemdetail.salediscount = invoiceitem.salediscount;
                            invoiceitemdetail.sellingprice = invoiceitem.salerate - invoiceitem.salediscount;
                            invoiceitemdetail.dealerprice = invoiceitem.wsrate;
                            invoiceitemdetail.mrp = invoiceitem.mrp;
                            if (invoiceitemdetail.Details == null)
                                invoiceitemdetail.Details = new Models.Items();

                            invoiceitemdetail.Details.serialno = invoiceitem.gln;
                            invoiceitemdetail.Details.gln = invoiceitem.gln;
                            invoiceitemdetail.Details.productid = invoiceitem.productid;
                            invoiceitemdetail.Details.brandid = invoiceitem.brandid;
                            invoiceitemdetail.Details.fitid = invoiceitem.fitid;
                            invoiceitemdetail.Details.sleeveid = invoiceitem.sleeveid;
                            invoiceitemdetail.Details.dealerprice = invoiceitem.wsrate;
                            invoiceitemdetail.Details.mrp = invoiceitem.mrp;

                            if (invoiceitem.saletaxpercentage > 0)
                                invoiceitemdetail.salestax = Math.Round(invoiceitemdetail.sellingprice / (100 + invoiceitem.saletaxpercentage) * 100, 2);
                            invoiceitemdetail.salestaxpercentage = invoiceitem.saletaxpercentage;
                            invoiceitemdetail.supplierid = invoiceitem.supplierid;
                            invoiceitemdetail.taxid = invoiceitem.saletaxid;

                            invoiceitemdetail.hsncode = IsNumber(invoiceitem.type) ? invoiceitem.type : "";
                            invoiceitemdetail.lrinvoiceid = lrinvoice.id;
                            invoiceitemdetail.lrid = lrentry.id;
                            invoiceitemdetail.sno = lrinvoice.Lrinvoiceitems.Count + 1;
                            invoiceitemdetail.barcodes = Newtonsoft.Json.JsonConvert.SerializeObject(new List<string>(new string[] { invoiceitem.barcode }));

                            if (invoiceitemdetail.purchasediscount > 0 && invoiceitemdetail.purchasediscountpercentage == 0)
                                invoiceitemdetail.purchasediscountpercentage = Math.Round((invoiceitemdetail.purchasediscount * 100) / invoiceitemdetail.basebuyingprice, 2);

                            lrinvoice.Lrinvoiceitems.Add(invoiceitemdetail);
                            mapitems.Add(new Tuple<InfyPOS.Models.LrinvoiceItemDetail, Stock>(invoiceitemdetail, invoiceitem));
                        }
                        invoicedata.Insert(transaction, conn, lrinvoice);

                        foreach (var taxitem in lrinvoice.Lrinvoicetaxitems)
                        {
                            taxitem.invoiceid = lrinvoice.id;
                            lrinvoicetax.Insert(transaction, conn, taxitem);
                        }
                        foreach (var titem in mapitems)
                        {
                            titem.Item1.lrinvoiceid = lrinvoice.id;
                            invoiceitemsdata.Insert(transaction, conn, titem.Item1);
                            if (!string.IsNullOrEmpty(titem.Item1.barcodes))
                                invoiceitemsdata.UpdateBarcode(transaction, conn, titem.Item1);
                            titem.Item2.invoiceitemid = titem.Item1.id;
                            titem.Item2.invno = lrentry.lrentryno;// + "." + titem.Item1.sno;
                        }
                    }


                    var stocklist = sales.GroupBy(e => new { e.barcode });
                    List<string> sellablebarcodeList = new List<string>();
                    if (InfyPOS.Services.ConfigManager.Instance(stockdataservice.Context.OrganizationID).Config.system_hyper_uan &&
                    InfyPOS.Services.ConfigManager.Instance(stockdataservice.Context.OrganizationID).Config.system_ismfgapplication)
                    {
                        sellablebarcodeList = sellablebarcodesDataService.Search(new Models.SellablebarcodesCreteria()
                        {
                            ids = stocklist.ToList().ConvertAll(x => x.Key.barcode).ToList()
                        }).ConvertAll(x => x.barcode);
                    }

                    var whids = loclist.FindAll(x => x.additionalinfo != null && x.additionalinfo.iswarehouse).ConvertAll(x => x.id).ToList();

                    int count = stocklist.Count();
                    int ccount = 0;
                    int dcount = 0;
                    foreach (var item in stocklist)
                    {

                        try
                        {
                            Stock firstitem = item.First();

                            InfyPOS.Models.StockWithTax existingitem = null;
                            if (!string.IsNullOrEmpty(firstitem.gln) &&
                                barcodeList.ContainsKey(firstitem.gln))
                            {
                                existingitem = stockdataservice.Search(0, firstitem.gln, conn, transaction);
                                if (existingitem != null &&
                                    existingitem.id > 0)
                                    dcount++;
                            }

                            if ((existingitem == null ||
                               existingitem.id <= 0) && !string.IsNullOrEmpty(firstitem.barcode) &&
                                barcodeList.ContainsKey(firstitem.barcode))
                            {
                                existingitem = stockdataservice.Search(0, firstitem.barcode, conn, transaction);
                                if (existingitem != null &&
                                    existingitem.id > 0)
                                    dcount++;
                            }

                            InfyPOS.Models.Stock stock = new InfyPOS.Models.Stock();
                            stock.itemid = firstitem.itemid;
                            stock.barcode = item.Key.barcode;

                            stock.buyingprice = firstitem.cost - firstitem.ptaxamount;
                            if (stock.buyingprice < 0)
                                stock.buyingprice = firstitem.salerate;

                            stock.ibuyingprice = firstitem.cost;
                            if (stock.ibuyingprice < 0)
                                stock.ibuyingprice = firstitem.salerate;

                            stock.designid = firstitem.designno;
                            stock.pricetagid = firstitem.pricetagid;
                            stock.itemid = firstitem.itemid;
                            stock.discount = firstitem.salediscount;
                            stock.price = firstitem.salerate;
                            stock.serialno = firstitem.gln;
                            stock.printingname = firstitem.product;
                            stock.productid = firstitem.productid;
                            stock.qty = item.Sum(e => e.purqty > 0 ? e.purqty : e.closingqty);
                            stock.supplierid = firstitem.supplierid;
                            stock.srcsupplierid = firstitem.sourcesupplierid;
                            stock.transfered = stock.qty;

                            if(item.ToList().Exists(x=>whids.Contains(x.locationid)))
                            {
                                var whstock = item.ToList().FindAll(x => whids.Contains(x.locationid)).Sum(x => x.closingqty);
                                stock.transfered = stock.qty - whstock;
                            }
                            stock.dealerprice = firstitem.wsrate;
                            stock.displaymrp = firstitem.mrp;
                            stock.invoiceitemid = firstitem.invoiceitemid;
                            stock.lrreference = firstitem.invno;
                            stock.hsncode = firstitem.hsncode;
                            stock.companyid = firstitem.companyid;
                            stock.stockdate = firstitem.purentrydate;
                            stock.isactive = true;

                            if ((stock.price - stock.discount) > 0 && firstitem.ptaxpercentage > 0)
                                stock.eprice = (stock.price - stock.discount) / (100 + firstitem.ptaxpercentage) * 100;

                            if (existingitem != null && existingitem.id > 0)
                            {
                                existingitem.qty += stock.qty;
                                existingitem.transfered += stock.transfered;
                                //existingitem.price = stock.price;
                                //existingitem.buyingprice = stock.buyingprice;
                                //existingitem.discount = stock.discount;
                                //existingitem.invoiceitemid = stock.invoiceitemid;
                                //existingitem.lrreference = stock.lrreference;
                                stock.id = existingitem.id;
                                stockdataservice.Update(transaction, conn, existingitem);
                            }
                            else
                            {
                                if (string.IsNullOrEmpty(stock.barcode))
                                {
                                    var barcodes = stockdataservice.CreateBarcode(BarcodeManager.BarcodeType.Base62, 1, stock.companyid, conn, transaction);
                                    stock.barcode = barcodes[0].code;
                                }
                                stockdataservice.Insert(transaction, conn, stock);
                            }

                            if (InfyPOS.Services.ConfigManager.Instance(stockdataservice.Context.OrganizationID).Config.system_hyper_uan &&
                                InfyPOS.Services.ConfigManager.Instance(stockdataservice.Context.OrganizationID).Config.system_ismfgapplication)
                            {
                                if (!string.IsNullOrEmpty(firstitem.gln) &&
                                    !string.IsNullOrEmpty(firstitem.barcode) &&
                                    firstitem.gln != firstitem.barcode)
                                {
                                    stock.barcode = firstitem.gln;
                                    stock.serialno = "";
                                    if (!sellablebarcodeList.Contains(firstitem.barcode))
                                    {
                                        sellablebarcodesDataService.Insert(new Models.Sellablebarcodes()
                                        {
                                            corebarcode = firstitem.gln,
                                            barcode = firstitem.barcode,
                                            isactive = true,
                                            version = 1
                                        });
                                        sellablebarcodeList.Add(firstitem.barcode);
                                    }
                                }
                            }

                            if (!string.IsNullOrEmpty(firstitem.gln) && item.Count() > 1 && item.ToList().ConvertAll(x => x.mrp).Distinct().Count() > 1)
                            {
                                var param = new SortedList<string, object>();
                                param.Add("id", stock.id);
                                salablegoodspriceDataService.ExecuteQuery("update stock set ismultiprice=true where id=@id", param, conn, transaction);
                                foreach (var mpriceitem in item)
                                {
                                    salablegoodspriceDataService.Insert(new Models.Salablegoodsprice()
                                    {
                                        isactive = true,
                                        locationid = firstitem.locationid,
                                        barcode = string.IsNullOrEmpty(stock.serialno) ? stock.barcode : stock.serialno,
                                        invoiceitemid = stock.invoiceitemid,
                                        price = mpriceitem.salerate,
                                        mrp = mpriceitem.mrp
                                    });
                                }
                            }

                            foreach (var salablestocks in item)
                            {
                                
                                if (!whids.Contains(salablestocks.locationid))
                                {
                                    InfyPOS.Models.Salablegoods existingsgood = null;
                                    if (barcodeList.ContainsKey(stock.barcode))
                                    {
                                        if (barcodeList[stock.barcode].Exists(e => e.locationid == salablestocks.locationid))
                                        {
                                            var existingsgoodList = salabledataservice.Search(new InfyPOS.Models.SalablegoodsCreteria()
                                            {
                                                id = barcodeList[stock.barcode].Find(e => e.locationid == salablestocks.locationid).salableid
                                            }, conn, transaction);

                                            existingsgood = existingsgoodList[0];
                                        }
                                    }

                                    if (stock.qty <= 0) stock.qty = 1;
                                    InfyPOS.Models.Salablegoods salable = new InfyPOS.Models.Salablegoods();
                                    salable.stockid = stock.id;

                                    if (item.Sum(e => e.purqty) > 0)
                                    {
                                        salable.iqty = item.Sum(e => e.purqty);
                                        salable.oqty = item.Sum(e => e.purqty - e.closingqty);
                                    }
                                    else
                                    {
                                        salable.iqty = item.Sum(e => e.closingqty);
                                        salable.oqty = 0;
                                    }
                                    salable.itemid = stock.itemid;
                                    salable.companyid = salablestocks.companyid;
                                    salable.locationid = salablestocks.locationid;
                                    //salable.oqty = stock.qty;
                                    salable.received = true;
                                    salable.isactive = true;
                                    salable.price = salablestocks.salerate;
                                    salable.discount = salablestocks.salediscount;
                                    salable.sellingname = salablestocks.itemname;
                                    salable.barcode = stock.barcode;
                                    salable.supplierid = salablestocks.supplierid;
                                    salable.inwardon = salablestocks.purentrydate;
                                    salable.costprice = salablestocks.cost;
                                    salable.icostprice = salablestocks.cost;
                                    if (existingsgood != null)
                                    {
                                        existingsgood.iqty += stock.qty;
                                        salabledataservice.Update(transaction, conn, existingsgood, existingsgood);
                                        salable.id = existingsgood.id;
                                    }
                                    else
                                    {
                                        salabledataservice.Insert(transaction, conn, salable);
                                        if (!barcodeList.ContainsKey(stock.barcode))
                                        {
                                            barcodeList.Add(stock.barcode, new List<InfyPOS.Data.StockDataService.BarcodeID>());
                                        }
                                        barcodeList[stock.barcode].Add(new InfyPOS.Data.StockDataService.BarcodeID()
                                        {
                                            barcode = stock.barcode,
                                            locationid = salablestocks.locationid,
                                            salableid = salable.id,
                                            stockid = stock.id
                                        });
                                    }
                                }
                            }
                            
                            ccount++;
                            data.progress.completedrows += item.Count();
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