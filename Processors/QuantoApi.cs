using InfyPOS.Common;
using InfyPOS.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
namespace InfyPOS.QuantoAPI
{
    public class QuantoAPIDataService : ReportData
    {
        internal byte[] Excel(List<BillsData> resultFULL, string title)
        {
            DataTable dt = new DataTable();
            dt.Columns.AddRange(new DataColumn[1] { new DataColumn("Bill Date", typeof(DateTime)) });
            dt.Columns.AddRange(new DataColumn[1] { new DataColumn("Bill No", typeof(string)) });
            dt.Columns.AddRange(new DataColumn[1] { new DataColumn("Payment Mode", typeof(string)) });
            dt.Columns.AddRange(new DataColumn[1] { new DataColumn("NetAmount", typeof(decimal)) });
            dt.Columns.AddRange(new DataColumn[1] { new DataColumn("Tax", typeof(decimal)) });
            dt.Columns.AddRange(new DataColumn[1] { new DataColumn("Amount", typeof(decimal)) });
            dt.Columns.AddRange(new DataColumn[1] { new DataColumn("Refund", typeof(decimal)) });

            foreach (var p in resultFULL)
            {
                dt.Rows.Add(new object[] { p.billdate,p.billno,p.paymode,p.netamount,p.tax,
                    p.amount,p.returnamount});
            }

            using (System.IO.MemoryStream memoryStream = new System.IO.MemoryStream())
            {
                ClosedXML.Excel.XLWorkbook xLWorkbook = new ClosedXML.Excel.XLWorkbook();
                var sheet = xLWorkbook.AddWorksheet(dt, title);
                xLWorkbook.SaveAs(memoryStream);
                return memoryStream.ToArray();
            }
        }
        internal byte[] Excel(List<SalesData> resultFULL, string title)
        {
            DataTable dt = new DataTable();
            dt.Columns.AddRange(new DataColumn[1] { new DataColumn("Bill Date", typeof(DateTime)) });
            dt.Columns.AddRange(new DataColumn[1] { new DataColumn("Bill No", typeof(string)) });
            dt.Columns.AddRange(new DataColumn[1] { new DataColumn("Company", typeof(string)) });
            dt.Columns.AddRange(new DataColumn[1] { new DataColumn("Branch", typeof(string)) });
            dt.Columns.AddRange(new DataColumn[1] { new DataColumn("City", typeof(string)) });

            dt.Columns.AddRange(new DataColumn[1] { new DataColumn("Product", typeof(string)) });
            dt.Columns.AddRange(new DataColumn[1] { new DataColumn("Brand", typeof(string)) });
            dt.Columns.AddRange(new DataColumn[1] { new DataColumn("Style", typeof(string)) });
            dt.Columns.AddRange(new DataColumn[1] { new DataColumn("Pattern", typeof(string)) });
            dt.Columns.AddRange(new DataColumn[1] { new DataColumn("Material", typeof(string)) });
            dt.Columns.AddRange(new DataColumn[1] { new DataColumn("Design", typeof(string)) });
            dt.Columns.AddRange(new DataColumn[1] { new DataColumn("Colour", typeof(string)) });
            dt.Columns.AddRange(new DataColumn[1] { new DataColumn("Size", typeof(string)) });
            dt.Columns.AddRange(new DataColumn[1] { new DataColumn("Sleeve", typeof(string)) });
            dt.Columns.AddRange(new DataColumn[1] { new DataColumn("Fit", typeof(string)) });
            dt.Columns.AddRange(new DataColumn[1] { new DataColumn("Type", typeof(string)) });
            dt.Columns.AddRange(new DataColumn[1] { new DataColumn("Barcode", typeof(string)) });
            dt.Columns.AddRange(new DataColumn[1] { new DataColumn("Qty", typeof(decimal)) });
            dt.Columns.AddRange(new DataColumn[1] { new DataColumn("MRP", typeof(decimal)) });
            dt.Columns.AddRange(new DataColumn[1] { new DataColumn("Price", typeof(decimal)) });
            dt.Columns.AddRange(new DataColumn[1] { new DataColumn("Discount", typeof(decimal)) });
            dt.Columns.AddRange(new DataColumn[1] { new DataColumn("Promotion", typeof(decimal)) });
            dt.Columns.AddRange(new DataColumn[1] { new DataColumn("Amount", typeof(decimal)) });

            foreach (var p in resultFULL)
            {
                dt.Rows.Add(new object[] { p.billdate.Date,p.billno,p.company,p.branch,p.city,
                    p.product,p.brand,p.style,p.pattern,p.material,p.design,p.colour,p.size,
                    p.sleeve,p.fit,p.type,p.barcode,p.qty,p.mrp,p.price,p.discount,p.promotion,p.amount});
            }

            using (System.IO.MemoryStream memoryStream = new System.IO.MemoryStream())
            {
                ClosedXML.Excel.XLWorkbook xLWorkbook = new ClosedXML.Excel.XLWorkbook();
                var sheet = xLWorkbook.AddWorksheet(dt, title);
                xLWorkbook.SaveAs(memoryStream);
                return memoryStream.ToArray();
            }
        }
        internal List<SalesData> SalesAPI(Criteria criteria)
        {
            if (criteria.supplierid == 0)
            {
                AccesskeysDataService accesskeysDataService = new AccesskeysDataService() { Context = this.Context };
                var supplier = accesskeysDataService.Search(new AccesskeysDataService.AccesskeysCreteria()
                {
                    accesskey = criteria.token
                });
                if (supplier.Count == 0)
                    throw new Exception("Given Token key is not valid");

                if (supplier.Exists(x => x.status == 1))
                {
                    criteria.supplierid = supplier.Find(x => x.status == 1).sourceid;
                }
                else
                {
                    throw new Exception("Access revoked - " + supplier.OrderBy(x => x.id).Last().statusmessage);
                }
            }
            string query = @"select billno,billdate,company.name company,
location.name branch,
city.name city,billitems.barcode,billitems.printingname,
billitems.price,billitems.discount,
billitems.schemediscount promotion,
billitems.receivable amount,
billitems.attributes->>'serialno' serialno,
billitems.qty,stock.designid design,brand.name Brand,
pattern.name  Pattern,
style.name Style,  
products.name Product, 
size.name Size,
material.name Material,
colour.name Colour,
types.name ""type"",
sleeve.name  Sleeve,
fit.name Fit,
stock.displaymrp mrp
from bill 
inner join billitems on billitems.billid = bill.id
inner join stock on stock.id = billitems.stockid
inner join items on items.id = stock.itemid
left outer join products on products.id = stock.productid
left outer join brand brand on brand.id = items.brandid
left outer join referencelist colour on colour.id = colourid
left outer join referencelist size on size.id = sizeid
left outer join referencelist style on style.id = styleid
left outer join referencelist pattern on pattern.id = patternid
left outer join referencelist material on material.id = materialid
left outer join referencelist types on types.id = typeid
left outer join referencelist sleeve on sleeve.id = sleeveid
left outer join referencelist fit on fit.id = fitid
inner join company on company.id = bill.companyid
inner join referencelist location on location.id =bill.locationid
left outer join referencelist city on city.id =cast(location.additionalinfo->'locationconfig'->>'cityid' as bigint)
where billdate>=@fromdate and billdate<@todate and bill.isactive=true
and bill.billtype != 9 and billitems.qty!=0 and billitems.isactive=true 
and (stock.supplierid = @supplierid or stock.srcsupplierid= @supplierid)";

            var param = new SortedList<string, object>();
            var lst = new List<SalesData>();
            param.Add("supplierid", criteria.supplierid);
            param.Add("fromdate", BaseEntity.GetDataFromString(criteria.from, "yyyyMMdd"));
            param.Add("todate", BaseEntity.GetDataFromString(criteria.to, "yyyyMMdd").AddDays(1));
            using (System.Data.IDataReader reader = ExecuteReader(query, param))
            {
                while (reader.Read())
                {
                    SalesData obill = new SalesData();
                    obill.amount = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"]);
                    obill.barcode = reader["barcode"] == DBNull.Value ? "" : reader["barcode"].ToString();
                    obill.billdate = reader["billdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["billdate"]);
                    obill.billno = reader["billno"] == DBNull.Value ? "" : reader["billno"].ToString();
                    obill.branch = reader["branch"] == DBNull.Value ? "" : reader["branch"].ToString();
                    obill.brand = reader["brand"] == DBNull.Value ? "" : reader["brand"].ToString();
                    obill.city = reader["city"] == DBNull.Value ? "" : reader["city"].ToString();
                    obill.colour = reader["colour"] == DBNull.Value ? "" : reader["colour"].ToString();
                    obill.company = reader["company"] == DBNull.Value ? "" : reader["company"].ToString();
                    obill.design = reader["design"] == DBNull.Value ? "" : reader["design"].ToString();
                    obill.fit = reader["fit"] == DBNull.Value ? "" : reader["fit"].ToString();
                    obill.material = reader["material"] == DBNull.Value ? "" : reader["material"].ToString();
                    obill.mrp = reader["mrp"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["mrp"]);
                    obill.pattern = reader["pattern"] == DBNull.Value ? "" : reader["pattern"].ToString();
                    obill.price = reader["price"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["price"]);
                    obill.product = reader["product"] == DBNull.Value ? "" : reader["product"].ToString();

                    obill.qty = reader["qty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["qty"]);

                    obill.promotion = reader["promotion"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["promotion"]);
                    obill.discount = reader["discount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discount"]);
                    if (obill.promotion > 0)
                        obill.promotion = obill.promotion / obill.qty;
                    if (obill.discount > 0)
                        obill.discount = obill.discount / obill.qty;
                    var serialno = reader["serialno"] == DBNull.Value ? "" : reader["serialno"].ToString();
                    if (!string.IsNullOrEmpty(serialno))
                        obill.barcode = serialno;
                    obill.size = reader["size"] == DBNull.Value ? "" : reader["size"].ToString();
                    obill.sleeve = reader["sleeve"] == DBNull.Value ? "" : reader["sleeve"].ToString();
                    obill.type = reader["type"] == DBNull.Value ? "" : reader["type"].ToString();
                    if (obill.mrp == 0)
                        obill.mrp = obill.price;
                    lst.Add(obill);
                }
            }
            if (!string.IsNullOrEmpty(criteria.brand))
            {
                var brandlist = criteria.brand.Split(',').ToList().ConvertAll(x => x.ToUpper());
                return lst.FindAll(x => brandlist.Contains(x.brand.ToUpper()));
            }
            return lst;
        }


        internal List<BillsData> BillsAPI(Criteria criteria)
        {
            if (string.IsNullOrEmpty(criteria.token))
                throw new Exception("Token is missing");

            UsersDataService usersDataService = new UsersDataService();
            var users = usersDataService.Search(new Models.UsersCreteria()
            {
                accesskey = criteria.token
            });
            if (users.Count == 0)
                throw new Exception("Token is not valid");

            if (string.IsNullOrEmpty(criteria.location)) throw new Exception("Location missing");
            ReferencelistDataService referencelistDataService = new ReferencelistDataService() { Context = this.Context };
            var locations = referencelistDataService.Search(new Models.ReferencelistCreteria()
            {
                code = criteria.location,
                type = "LOCATION"
            });
            if (locations.Count == 0)
                throw new Exception("Location missing - " + criteria.location);


            string query = @"select bill.billno,billdate,
bill.tax,bill.receivable amount,bill.rounding,billtype,bill.discount,
billsettlementmaster.paid_refund,
billsettlementmaster.paid_upi,
billsettlementmaster.paid_voucher,
billsettlementmaster.paid_cash
from bill 
inner join billsettlement on billsettlement.billid = bill.id 
inner join billsettlementmaster on billsettlementmaster.id =billsettlementmasterid and billsettlementmaster.isactive=true
where bill.billdate>=@fromdate and bill.billdate<@todate and bill.isactive=true
and bill.locationid=@locationid";

            var param = new SortedList<string, object>();
            var lst = new List<BillsData>();
            param.Add("locationid", locations[0].id);
            param.Add("fromdate", BaseEntity.GetDataFromString(criteria.from, "yyyyMMdd"));
            param.Add("todate", BaseEntity.GetDataFromString(criteria.to, "yyyyMMdd").AddDays(1));
            using (System.Data.IDataReader reader = ExecuteReader(query, param))
            {
                while (reader.Read())
                {
                    BillsData obill = new BillsData();
                    obill.amount = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"]);
                    obill.amount += reader["rounding"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["rounding"]);
                    obill.billdate = reader["billdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["billdate"]);
                    obill.billno = reader["billno"] == DBNull.Value ? "" : reader["billno"].ToString();
                    obill.tax = reader["tax"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["tax"]);
                    obill.discount = reader["discount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discount"]);
                    int billtype = Convert.ToInt32(reader["billtype"]);
                    if (reader["paid_refund"] != DBNull.Value &&
                        Convert.ToDecimal(reader["paid_refund"]) > 0)
                    {
                        obill.returnamount = Convert.ToDecimal(reader["paid_refund"]);
                    }
                    else if (reader["paid_upi"] != DBNull.Value &&
                        Convert.ToDecimal(reader["paid_upi"]) > 0)
                    {
                        obill.paymode = "UPI";
                    }
                    else if (reader["paid_voucher"] != DBNull.Value &&
                        Convert.ToDecimal(reader["paid_voucher"]) > 0)
                    {
                        obill.paymode = "VOUCHER";
                    }
                    else if (reader["paid_cash"] != DBNull.Value &&
                        Convert.ToDecimal(reader["paid_cash"]) > 0)
                    {
                        obill.paymode = "CASH";
                    }
                    lst.Add(obill);
                }
            }
            return lst;
        }

        internal byte[] Excel(List<StockData> resultFULL, string title)
        {
            DataTable dt = new DataTable();
            dt.Columns.AddRange(new DataColumn[1] { new DataColumn("Company", typeof(string)) });
            dt.Columns.AddRange(new DataColumn[1] { new DataColumn("Branch", typeof(string)) });
            dt.Columns.AddRange(new DataColumn[1] { new DataColumn("City", typeof(string)) });

            dt.Columns.AddRange(new DataColumn[1] { new DataColumn("Invoice No", typeof(string)) });
            dt.Columns.AddRange(new DataColumn[1] { new DataColumn("Invoice Date", typeof(DateTime)) });
            dt.Columns.AddRange(new DataColumn[1] { new DataColumn("Section", typeof(string)) });

            dt.Columns.AddRange(new DataColumn[1] { new DataColumn("Product", typeof(string)) });
            dt.Columns.AddRange(new DataColumn[1] { new DataColumn("Brand", typeof(string)) });
            dt.Columns.AddRange(new DataColumn[1] { new DataColumn("Style", typeof(string)) });
            dt.Columns.AddRange(new DataColumn[1] { new DataColumn("Pattern", typeof(string)) });
            dt.Columns.AddRange(new DataColumn[1] { new DataColumn("Material", typeof(string)) });
            dt.Columns.AddRange(new DataColumn[1] { new DataColumn("Design", typeof(string)) });
            dt.Columns.AddRange(new DataColumn[1] { new DataColumn("Colour", typeof(string)) });
            dt.Columns.AddRange(new DataColumn[1] { new DataColumn("Size", typeof(string)) });
            dt.Columns.AddRange(new DataColumn[1] { new DataColumn("Sleeve", typeof(string)) });
            dt.Columns.AddRange(new DataColumn[1] { new DataColumn("Fit", typeof(string)) });
            dt.Columns.AddRange(new DataColumn[1] { new DataColumn("Type", typeof(string)) });
            dt.Columns.AddRange(new DataColumn[1] { new DataColumn("Barcode", typeof(string)) });
            dt.Columns.AddRange(new DataColumn[1] { new DataColumn("Qty", typeof(decimal)) });
            dt.Columns.AddRange(new DataColumn[1] { new DataColumn("MRP", typeof(decimal)) });
            dt.Columns.AddRange(new DataColumn[1] { new DataColumn("Price", typeof(decimal)) });
            dt.Columns.AddRange(new DataColumn[1] { new DataColumn("Discount", typeof(decimal)) });

            foreach (var p in resultFULL)
            {
                dt.Rows.Add(new object[] {p.company,p.branch,p.city,p.invoiceno,p.invoicedate,p.section,
                    p.product,p.brand,p.style,p.pattern,p.material,p.design,p.colour,p.size,
                    p.sleeve,p.fit,p.type,p.barcode,p.qty,p.mrp,p.price,p.discount});
            }

            using (System.IO.MemoryStream memoryStream = new System.IO.MemoryStream())
            {
                ClosedXML.Excel.XLWorkbook xLWorkbook = new ClosedXML.Excel.XLWorkbook();
                var sheet = xLWorkbook.AddWorksheet(dt, title);
                xLWorkbook.SaveAs(memoryStream);
                return memoryStream.ToArray();
            }
        }

        internal List<StockData> StockAPI(Criteria criteria)
        {
            if (criteria.supplierid == 0)
            {
                AccesskeysDataService accesskeysDataService = new AccesskeysDataService() { Context = this.Context };
                var supplier = accesskeysDataService.Search(new AccesskeysDataService.AccesskeysCreteria()
                {
                    accesskey = criteria.token
                });
                if (supplier.Count == 0)
                    throw new Exception("Given Token key is not valid");

                if (supplier.Exists(x => x.status == 1))
                {
                    criteria.supplierid = supplier.Find(x => x.status == 1).sourceid;
                }
                else
                {
                    throw new Exception("Access revoked - " + supplier.OrderBy(x => x.id).Last().statusmessage);
                }
            }

            string query = @"select company.name company,
location.name branch,
city.name city,salablegoods.barcode,items.printingname,
salablegoods.price,salablegoods.discount,stock.serialno,
iqty-(oqty+jqty+aqty) qty,
stock.designid design,brand.name Brand,
pattern.name  Pattern,
style.name Style,  
products.name Product, 
size.name Size,
material.name Material,
colour.name Colour,
types.name ""type"",
sleeve.name  Sleeve,
fit.name Fit,stock.displaymrp mrp,
section.name sectionname,lrinvoice.invoicedate,lrinvoice.invoiceno
from salablegoods 
inner join stock on stock.id = salablegoods.stockid
left outer join lrinvoiceitems on lrinvoiceitems.id = invoiceitemid
left outer join lrinvoice on lrinvoice.id = lrinvoiceid
inner join items on items.id = stock.itemid
left outer join products on products.id = stock.productid
left outer join brand brand on brand.id = items.brandid
left outer join referencelist colour on colour.id = colourid
left outer join referencelist size on size.id = sizeid
left outer join referencelist style on style.id = styleid
left outer join referencelist pattern on pattern.id = patternid
left outer join referencelist material on material.id = materialid
left outer join referencelist types on types.id = typeid
left outer join referencelist sleeve on sleeve.id = sleeveid
left outer join referencelist fit on fit.id = fitid
inner join company on company.id = salablegoods.companyid
inner join referencelist location on location.id =salablegoods.locationid
left outer join referencelist section on section.id = products.salescounterid
left outer join referencelist city on city.id =cast(location.additionalinfo->'locationconfig'->>'cityid' as bigint)
where iqty-(oqty+jqty+aqty)>0
and (stock.supplierid = @supplierid or stock.srcsupplierid= @supplierid)";

            var param = new SortedList<string, object>();
            var lst = new List<StockData>();
            param.Add("supplierid", criteria.supplierid);
            using (System.Data.IDataReader reader = ExecuteReader(query, param))
            {
                while (reader.Read())
                {
                    StockData obill = new StockData();
                    obill.barcode = reader["barcode"] == DBNull.Value ? "" : reader["barcode"].ToString();
                    obill.branch = reader["branch"] == DBNull.Value ? "" : reader["branch"].ToString();
                    obill.brand = reader["brand"] == DBNull.Value ? "" : reader["brand"].ToString();
                    obill.city = reader["city"] == DBNull.Value ? "" : reader["city"].ToString();
                    obill.colour = reader["colour"] == DBNull.Value ? "" : reader["colour"].ToString();
                    obill.company = reader["company"] == DBNull.Value ? "" : reader["company"].ToString();
                    obill.design = reader["design"] == DBNull.Value ? "" : reader["design"].ToString();
                    obill.fit = reader["fit"] == DBNull.Value ? "" : reader["fit"].ToString();
                    obill.material = reader["material"] == DBNull.Value ? "" : reader["material"].ToString();
                    obill.mrp = reader["mrp"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["mrp"]);
                    obill.pattern = reader["pattern"] == DBNull.Value ? "" : reader["pattern"].ToString();
                    obill.price = reader["price"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["price"]);
                    obill.product = reader["product"] == DBNull.Value ? "" : reader["product"].ToString();
                    obill.qty = reader["qty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["qty"]);
                    obill.discount = reader["discount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discount"]);
                    var serialno = reader["serialno"] == DBNull.Value ? "" : reader["serialno"].ToString();
                    if (!string.IsNullOrEmpty(serialno))
                        obill.barcode = serialno;
                    obill.size = reader["size"] == DBNull.Value ? "" : reader["size"].ToString();
                    obill.sleeve = reader["sleeve"] == DBNull.Value ? "" : reader["sleeve"].ToString();
                    obill.type = reader["type"] == DBNull.Value ? "" : reader["type"].ToString();
                    if (obill.mrp == 0)
                        obill.mrp = obill.price;

                    obill.section = reader["sectionname"] == DBNull.Value ? "" : reader["sectionname"].ToString();
                    obill.invoiceno = reader["invoiceno"] == DBNull.Value ? "" : reader["invoiceno"].ToString();
                    obill.invoicedate = reader["invoicedate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["invoicedate"]);

                    lst.Add(obill);
                }
            }
            if (!string.IsNullOrEmpty(criteria.brand))
            {
                var brandlist = criteria.brand.Split(',').ToList().ConvertAll(x => x.ToUpper());
                return lst.FindAll(x => brandlist.Contains(x.brand.ToUpper()));
            }
            return lst;
        }

        internal byte[] Excel(List<PurchaseData> resultFULL, string title)
        {
            DataTable dt = new DataTable();
            dt.Columns.AddRange(new DataColumn[1] { new DataColumn("Company", typeof(string)) });

            dt.Columns.AddRange(new DataColumn[1] { new DataColumn("Invoice No", typeof(string)) });
            dt.Columns.AddRange(new DataColumn[1] { new DataColumn("Invoice Date", typeof(DateTime)) });
            dt.Columns.AddRange(new DataColumn[1] { new DataColumn("Received Date", typeof(DateTime)) });
            dt.Columns.AddRange(new DataColumn[1] { new DataColumn("Section", typeof(string)) });

            dt.Columns.AddRange(new DataColumn[1] { new DataColumn("Product", typeof(string)) });
            dt.Columns.AddRange(new DataColumn[1] { new DataColumn("Brand", typeof(string)) });
            dt.Columns.AddRange(new DataColumn[1] { new DataColumn("Style", typeof(string)) });
            dt.Columns.AddRange(new DataColumn[1] { new DataColumn("Pattern", typeof(string)) });
            dt.Columns.AddRange(new DataColumn[1] { new DataColumn("Material", typeof(string)) });
            dt.Columns.AddRange(new DataColumn[1] { new DataColumn("Design", typeof(string)) });
            dt.Columns.AddRange(new DataColumn[1] { new DataColumn("Colour", typeof(string)) });
            dt.Columns.AddRange(new DataColumn[1] { new DataColumn("Size", typeof(string)) });
            dt.Columns.AddRange(new DataColumn[1] { new DataColumn("Sleeve", typeof(string)) });
            dt.Columns.AddRange(new DataColumn[1] { new DataColumn("Fit", typeof(string)) });
            dt.Columns.AddRange(new DataColumn[1] { new DataColumn("Type", typeof(string)) });
            dt.Columns.AddRange(new DataColumn[1] { new DataColumn("Barcode", typeof(string)) });
            dt.Columns.AddRange(new DataColumn[1] { new DataColumn("Qty", typeof(decimal)) });
            dt.Columns.AddRange(new DataColumn[1] { new DataColumn("Cost", typeof(decimal)) });
            dt.Columns.AddRange(new DataColumn[1] { new DataColumn("MRP", typeof(decimal)) });
            dt.Columns.AddRange(new DataColumn[1] { new DataColumn("Price", typeof(decimal)) });
            dt.Columns.AddRange(new DataColumn[1] { new DataColumn("Discount", typeof(decimal)) });

            foreach (var p in resultFULL)
            {
                dt.Rows.Add(new object[] {p.company,p.invoiceno,p.invoicedate,p.receivedate,p.section,
                    p.product,p.brand,p.style,p.pattern,p.material,p.design,p.colour,p.size,
                    p.sleeve,p.fit,p.type,p.barcode,p.qty,p.buyingprice,p.mrp,p.price,p.discount});
            }

            using (System.IO.MemoryStream memoryStream = new System.IO.MemoryStream())
            {
                ClosedXML.Excel.XLWorkbook xLWorkbook = new ClosedXML.Excel.XLWorkbook();
                var sheet = xLWorkbook.AddWorksheet(dt, title);
                xLWorkbook.SaveAs(memoryStream);
                return memoryStream.ToArray();
            }
        }

        internal List<PurchaseData> PurchaseAPI(Criteria criteria)
        {
            if (criteria.supplierid == 0)
            {
                AccesskeysDataService accesskeysDataService = new AccesskeysDataService() { Context = this.Context };
                var supplier = accesskeysDataService.Search(new AccesskeysDataService.AccesskeysCreteria()
                {
                    accesskey = criteria.token
                });
                if (supplier.Count == 0)
                    throw new Exception("Given Token key is not valid");

                if (supplier.Exists(x => x.status == 1))
                {
                    criteria.supplierid = supplier.Find(x => x.status == 1).sourceid;
                }
                else
                {
                    throw new Exception("Access revoked - " + supplier.OrderBy(x => x.id).Last().statusmessage);
                }
            }
            string query = @"select company.name company,
stock.barcode,items.printingname,
stock.price,stock.discount,stock.serialno,
stock.qty,stock.buyingprice,
stock.designid design,brand.name Brand,
pattern.name  Pattern,
style.name Style,  
products.name Product, 
size.name Size,
material.name Material,
colour.name Colour,
types.name ""type"",
sleeve.name  Sleeve,
fit.name Fit,stock.displaymrp mrp,lrinvoice.entrydate,
section.name sectionname,lrinvoice.invoicedate,lrinvoice.invoiceno
from stock 
left outer join lrinvoiceitems on lrinvoiceitems.id = invoiceitemid
left outer join lrinvoice on lrinvoice.id = lrinvoiceid
inner join items on items.id = stock.itemid
left outer join products on products.id = stock.productid
left outer join brand brand on brand.id = items.brandid
left outer join referencelist colour on colour.id = colourid
left outer join referencelist size on size.id = sizeid
left outer join referencelist style on style.id = styleid
left outer join referencelist pattern on pattern.id = patternid
left outer join referencelist material on material.id = materialid
left outer join referencelist types on types.id = typeid
left outer join referencelist sleeve on sleeve.id = sleeveid
left outer join referencelist fit on fit.id = fitid
inner join company on company.id = lrinvoice.companyid
left outer join referencelist section on section.id = products.salescounterid
where  lrinvoice.entrydate>=@fromdate and lrinvoice.entrydate<@todate
and (stock.supplierid = @supplierid or stock.srcsupplierid = @supplierid)";

            var param = new SortedList<string, object>();
            var lst = new List<PurchaseData>();
            param.Add("supplierid", criteria.supplierid);
            param.Add("fromdate", BaseEntity.GetDataFromString(criteria.from, "yyyyMMdd"));
            param.Add("todate", BaseEntity.GetDataFromString(criteria.to, "yyyyMMdd").AddDays(1));

            using (System.Data.IDataReader reader = ExecuteReader(query, param))
            {
                while (reader.Read())
                {
                    PurchaseData obill = new PurchaseData();
                    obill.barcode = reader["barcode"] == DBNull.Value ? "" : reader["barcode"].ToString();
                    obill.brand = reader["brand"] == DBNull.Value ? "" : reader["brand"].ToString();
                    obill.colour = reader["colour"] == DBNull.Value ? "" : reader["colour"].ToString();
                    obill.company = reader["company"] == DBNull.Value ? "" : reader["company"].ToString();
                    obill.design = reader["design"] == DBNull.Value ? "" : reader["design"].ToString();
                    obill.fit = reader["fit"] == DBNull.Value ? "" : reader["fit"].ToString();
                    obill.material = reader["material"] == DBNull.Value ? "" : reader["material"].ToString();
                    obill.mrp = reader["mrp"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["mrp"]);
                    obill.pattern = reader["pattern"] == DBNull.Value ? "" : reader["pattern"].ToString();
                    obill.price = reader["price"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["price"]);
                    obill.product = reader["product"] == DBNull.Value ? "" : reader["product"].ToString();
                    obill.buyingprice = reader["buyingprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["buyingprice"]);
                    obill.qty = reader["qty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["qty"]);
                    obill.discount = reader["discount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discount"]);
                    var serialno = reader["serialno"] == DBNull.Value ? "" : reader["serialno"].ToString();
                    if (!string.IsNullOrEmpty(serialno))
                        obill.barcode = serialno;
                    obill.size = reader["size"] == DBNull.Value ? "" : reader["size"].ToString();
                    obill.sleeve = reader["sleeve"] == DBNull.Value ? "" : reader["sleeve"].ToString();
                    obill.type = reader["type"] == DBNull.Value ? "" : reader["type"].ToString();
                    if (obill.mrp == 0)
                        obill.mrp = obill.price;

                    obill.section = reader["sectionname"] == DBNull.Value ? "" : reader["sectionname"].ToString();
                    obill.invoiceno = reader["invoiceno"] == DBNull.Value ? "" : reader["invoiceno"].ToString();
                    obill.invoicedate = reader["invoicedate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["invoicedate"]);
                    obill.receivedate = reader["entrydate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["entrydate"]);
                    lst.Add(obill);
                }
            }
            if (!string.IsNullOrEmpty(criteria.brand))
            {
                var brandlist = criteria.brand.Split(',').ToList().ConvertAll(x => x.ToUpper());
                return lst.FindAll(x => brandlist.Contains(x.brand.ToUpper()));
            }
            return lst;
        }
    }
    public class Criteria
    {
        public string token { get; set; }
        public string brand { get; set; }
        public string from { get; set; }
        public string to { get; set; }
        public string location { get; set; }
        public long supplierid { get; set; }
    }
    public class PurchaseData
    {
        public string company { get; set; }
        public string barcode { get; set; }
        public string brand { get; set; }
        public string design { get; set; }
        public string size { get; set; }
        public string colour { get; set; }
        public string sleeve { get; set; }
        public string fit { get; set; }
        public string product { get; set; }
        public string style { get; set; }
        public string pattern { get; set; }
        public string material { get; set; }
        public string type { get; set; }
        public decimal mrp { get; set; }
        public decimal price { get; set; }
        public decimal discount { get; set; }
        public decimal qty { get; set; }
        public string section { get; set; }
        public string invoiceno { get; set; }
        public DateTime invoicedate { get; set; }
        public DateTime receivedate { get; set; }
        public decimal buyingprice { get; set; }

    }
    public class StockData
    {
        public string company { get; set; }
        public string branch { get; set; }
        public string city { get; set; }
        public string barcode { get; set; }
        public string brand { get; set; }
        public string design { get; set; }
        public string size { get; set; }
        public string colour { get; set; }
        public string sleeve { get; set; }
        public string fit { get; set; }
        public string product { get; set; }
        public string style { get; set; }
        public string pattern { get; set; }
        public string material { get; set; }
        public string type { get; set; }
        public decimal mrp { get; set; }
        public decimal price { get; set; }
        public decimal discount { get; set; }
        public decimal qty { get; set; }
        public string section { get; set; }
        public string invoiceno { get; set; }
        public DateTime invoicedate { get; set; }
    }
    public class SalesData
    {
        public string billno { get; set; }
        public DateTime billdate { get; set; }
        public string company { get; set; }
        public string branch { get; set; }
        public string city { get; set; }
        public string barcode { get; set; }
        public string brand { get; set; }
        public string design { get; set; }
        public string size { get; set; }
        public string colour { get; set; }
        public string sleeve { get; set; }
        public string fit { get; set; }
        public string product { get; set; }
        public string style { get; set; }
        public string pattern { get; set; }
        public string material { get; set; }
        public string type { get; set; }
        public decimal mrp { get; set; }
        public decimal price { get; set; }
        public decimal discount { get; set; }
        public decimal promotion { get; set; }
        public decimal qty { get; set; }
        public decimal amount { get; set; }
    }

    public class BillsData
    {
        public string billno { get; set; }
        public DateTime billdate { get; set; }
        public string billtype { get; set; }
        public decimal discount { get; set; }
        public decimal amount { get; set; }
        public decimal tax { get; set; }
        public decimal netamount
        {
            get
            {
                return amount - tax;
            }
        }
        public string paymode { get; set; }
        public decimal returnamount { get; internal set; }
    }
}