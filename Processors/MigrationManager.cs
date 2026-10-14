using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using InfyPOS.Common;
using OfficeOpenXml;
using System.Globalization;
using Newtonsoft.Json;
using InfyPOS.Models;
using InfyPOS.Data;
using WebAPI.Data;
using Npgsql;
using InfyPOS.Services;

namespace InfyPOS.Processors.Migration
{
    public class ProgressStatus
    {
        public string status { get; set; }
        public bool hascompleted { get; set; }
        public int percentage { get; set; }
        public string identifier { get; set; }
        public bool hasstarted { get; set; }
        public int totalrows { get; set; }
        public List<List<string>> faileddata { get; set; }
        public int completedrows { get; set; }
        public int failedrows { get; set; }
        public int updatedrows { get; set; }
        public int createdrows { get; set; }
        public Exception Exception { get; set; }
        public List<string> statuslist { get; set; }
        [JsonIgnore]
        public string AddStatus
        {
            set
            {
                this.status = value;
                statuslist.Add(status);
            }
        }

        public class ResultItem
        {
            public long id { get; set; }
            public string sid { get; set; }
            public string status { get; set; }
        }
        public object result { get; set; }
        public List<Exception> ExceptionList { get; set; }
        [JsonIgnore]
        public byte[] resultbytes { get; set; }
    }

    public class MigrationData
    {
        public class Column
        {
            public string table { get; set; }
            public string display { get; set; }
            public string name { get; set; }
            public string isdate { get; set; }
            public string isnumber { get; set; }
            public bool migrate { get; set; }
            public string value { get; set; }
            public string excelcolunmn { get; set; }
        }
        public string barcodemode { get; set; }
        public int ItemNameTemplateID { get; set; }
        public string ItemNameTemplate { get; set; }
        public long approvedby { get; set; }
        public long supplierid { get; set; }
        public List<Column> columns { get; set; }
        public string tablename { get; set; }
        public string uploadfileid { get; set; }
        public ProgressStatus progress { get; set; }
        public string sheetname { get; set; }
        public long organizationid { get; set; }
        public List<MigrationManager.MigratingItem> rows { get; set; }
    }

    public class MigrationManager
    {
        private long organizationid;
        public MigrationManager(long organizationid)
        {
            this.organizationid = organizationid;
        }
        private static System.Collections.Concurrent.ConcurrentDictionary<long, MigrationManager> instanceList = new System.Collections.Concurrent.ConcurrentDictionary<long, MigrationManager>();

        public static MigrationManager Instance(long organizationid)
        {
            MigrationManager instance = null;
            if (instanceList.TryGetValue(organizationid, out instance))
                return instance;

            instance = new MigrationManager(organizationid);
            instanceList.TryAdd(organizationid, instance);

            return instance;
        }

        public ProgressStatus GetStatus(string id)
        {
            try
            {
                if (progressStatuses.Exists(e => e.identifier == id))
                {
                    var statusobject = progressStatuses.Find(e => e.identifier == id);
                    var percnetage = statusobject.totalrows > 0 ? ((decimal)(statusobject.completedrows + statusobject.failedrows) / statusobject.totalrows * 100) : 0;
                    return new ProgressStatus()
                    {
                        completedrows = statusobject.completedrows,
                        failedrows = statusobject.failedrows,
                        hascompleted = statusobject.hascompleted,
                        percentage = (int)Math.Round(percnetage),
                        status = statusobject.status,
                        totalrows = statusobject.totalrows,
                        faileddata = statusobject.hascompleted ? statusobject.faileddata : null
                    };
                }

            }
            catch (Exception exp)
            {

            }
            return new ProgressStatus()
            {
                status = "Failed"
            };
        }
        public ProgressStatus StartMigration(MigrationData data)
        {
            ProgressStatus progressStatus = new ProgressStatus();
            progressStatus.identifier = Guid.NewGuid().ToString();
            progressStatus.percentage = 0;
            data.progress = progressStatus;
            System.Threading.Thread thread = new System.Threading.Thread(new System.Threading.ParameterizedThreadStart(Migrate));
            thread.Start(data);
            progressStatuses.Add(progressStatus);
            return progressStatus;
        }

        public List<ProgressStatus> progressStatuses = new List<ProgressStatus>();

        public void Migrate(object Source)
        {
            MigrationData data = Source as MigrationData;
            System.Threading.Thread.Sleep(1000);

            if (data.progress == null)
                data.progress = new ProgressStatus();

            var columnsheaders = new List<string>();
            var rows = new List<List<string>>();

            var imageData = new Common.ImageData() { Context = new UserClaim() { OrganizationID = organizationid } };

            data.progress.status = "Loading from db";
            var file = imageData.GetData(data.uploadfileid.LongFromBase62());

            using (MemoryStream ms = new MemoryStream(file.data))
            {
                data.progress.status = "Loading xlsx";
                ExcelPackage package = new ExcelPackage(ms);
                ExcelWorksheet worksheet = null;
                if (!string.IsNullOrEmpty(data.sheetname))
                {
                    foreach (var sh in package.Workbook.Worksheets)
                    {
                        if (sh.Name.ToLower() == data.sheetname.ToLower())
                        {
                            worksheet = sh;
                            break;
                        }
                    }
                }

                if (worksheet == null)
                {
                    data.progress.status = "failed with exception can't find the sheet " + data.sheetname;
                    data.progress.hascompleted = true;
                    return;
                }

                data.progress.status = "Parsing data from xlsx";
                bool firstRow = true;

                for (int row = 1; row <= worksheet.Dimension.Rows; row++)
                {
                    var excelrow = worksheet.Row(row);
                    var rowvalues = new List<string>();
                    for (int col = 1; col <= worksheet.Dimension.Columns; col++)
                    {
                        var cellvalue = worksheet.GetValue(row, col);
                        if (firstRow)
                        {
                            string scellvalue = (cellvalue != null ? cellvalue.ToString() : "").ToLower();
                            columnsheaders.Add(scellvalue.ToLower());
                        }
                        else
                        {
                            rowvalues.Add(cellvalue != null ? cellvalue.ToString() : "");
                        }
                    }
                    if (firstRow)
                    {
                        firstRow = false;
                    }
                    else
                    {
                        rows.Add(rowvalues);
                    }
                }
            }



            SortedDictionary<string, ColumnMap> columnmap = new SortedDictionary<string, ColumnMap>();
            foreach (var column in data.columns)
            {
                ColumnMap columnMap = new ColumnMap();
                columnMap.columnname = column.name.ToLower();
                columnMap.defaultvalue = column.value;
                if (!string.IsNullOrEmpty(column.excelcolunmn))
                {
                    columnMap.columnmap = column.excelcolunmn.ToLower();
                    columnMap.index = columnsheaders.Contains(columnMap.columnmap) ? (columnsheaders.IndexOf(columnMap.columnmap)) : -1;
                }
                else
                {
                    columnMap.columnmap = "";
                    columnMap.index = -1;
                }
                columnmap.Add(columnMap.columnname, columnMap);
            }

            try
            {
                data.progress.status = "Data ready for migration";
                data.progress.totalrows = rows.Count;
                data.progress.completedrows = 0;
                data.progress.failedrows = 0;
                data.progress.faileddata = new List<List<string>>();
                data.progress.ExceptionList = new List<Exception>();
                data.progress.completedrows = 0;

                switch (data.tablename.ToUpper())
                {
                    case "UANSTOCK":
                        MigrateUANStock(data, columnmap, rows);
                        break;
                    case "TAX":
                        MigrateTax(data, columnmap, rows);
                        break;
                    case "PRODUCT":
                        MigrateProduct(data, columnmap, rows);
                        break;
                    case "LOCATION":
                        MigrateLocation(data, columnmap, rows);
                        break;
                    case "DESIGN":
                        MigrateDesign(data, columnmap, rows);
                        break;
                    case "BRAND":
                        MigrateBrand(data, columnmap, rows);
                        break;
                    case "ITEM":
                        MigrateItems(data, columnmap, rows);
                        break;
                    case "SUPPLIER":
                        MigrateSupplier(data, columnmap, rows);
                        break;
                    case "EMPLOYEE":
                        MigrateEmployees(data, columnmap, rows);
                        break;
                    case "VENDOR AGREEMENT":
                        MigrateVendorAgreement(data, columnmap, rows);
                        break;
                    case "AGENT":
                        MigrateAgent(data, columnmap, rows);
                        break;
                    case "CUSTOMER":
                        MigrateCustomer(data, columnmap, rows);
                        break;
                    case "SUPPLIERBALANCE":
                        MigrateSupplierBalance(data, columnmap, rows);
                        break;
                    case "DEBITNOTE":
                        MigrateDebitNotes(data, columnmap, rows);
                        break;
                    case "CUSTOMERBALANCE":
                        MigrateCustomerBalance(data, columnmap, rows);
                        break;
                    case "ATTRIBUTE":
                        MigrateAttributes(data, columnmap, rows);
                        break;
                    case "LRENTRY":
                        MigrateLREntry(data, columnmap, rows);
                        break;
                    case "MULTIPLE PRICE":
                        MigrateMultiplePrice(data, columnmap, rows);
                        break;
                    case "STOCK":
                        DataMigration_Stock dataMigration_Stock = new DataMigration_Stock(organizationid);
                        dataMigration_Stock.MigrateStock(data, columnmap, rows);
                        break;
                    case "SALES":
                        DataMigration_Sales dataMigration_Sales = new DataMigration_Sales(organizationid);
                        dataMigration_Sales.MigrateSales(data, columnmap, rows);
                        break;
                    case "CUSTOMER CREDIT NOTE VOUCHER":
                        MigrateCustomerCreditNoteVoucher(data, columnmap, rows);
                        break;
                    case "TRANSPORT":
                        MigrateTransport(data, columnmap, rows);
                        break;
                }

                data.progress.completedrows = (data.progress.totalrows - data.progress.failedrows);
                data.progress.hascompleted = true;
            }
            catch (Exception exp)
            {
                data.progress.status = "failed with exception " + exp.Message;
                data.progress.completedrows = (data.progress.totalrows - data.progress.failedrows);
                data.progress.hascompleted = true;
            }
        }

        private void MigrateMultiplePrice(MigrationData data, SortedDictionary<string, ColumnMap> columnmap,
           List<List<string>> values)
        {
            SuppliercompanyDataService suppliercompanyDataService = new SuppliercompanyDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            ReferencelistDataService referencelistDataService = new ReferencelistDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };

            List<Salablegoodsprice> SSproducts = (from row in values.AsEnumerable()
                                                  select new Salablegoodsprice
                                                  {
                                                      barcode = GetString(row, columnmap, "barcode"),
                                                      batchid = GetString(row, columnmap, "location"),
                                                      price = GetDecimal(row, columnmap, "price"),
                                                      mrp = GetDecimal(row, columnmap, "mrp")
                                                  }).ToList();

            data.progress.totalrows = SSproducts.Count * 2;
            data.progress.completedrows = 0;
            var locations = referencelistDataService.Search(new ReferencelistCreteria()
            { type = "LOCATION" });
            CultureInfo enUS = new CultureInfo("en-US");
            SalablegoodspriceDataService salablegoodspriceDataService = new SalablegoodspriceDataService();
            List<Salablegoodsprice> items = new List<Salablegoodsprice>();
            foreach (var row in SSproducts)
            {
                Salablegoodsprice lrentry = new Salablegoodsprice();
                lrentry.barcode = row.barcode;
                lrentry.price = row.price;
                lrentry.mrp = row.mrp;
                lrentry.isactive = true;
                lrentry.createdon = DateTime.Now;
                lrentry.modifiedon = DateTime.Now;
                if (!string.IsNullOrEmpty(row.batchid))
                {
                    if (locations.Exists(x => x.name.ToLower() == row.batchid.ToLower()))
                    {
                        lrentry.locationid = locations.Find(x => x.name.ToLower() == row.batchid.ToLower()).id;
                    }
                }
                items.Add(lrentry);
                data.progress.completedrows++;
            }

            foreach (var it in items)
            {
                it.id = 0;
                it.sync = true;
                salablegoodspriceDataService.Insert(it);
                data.progress.completedrows++;
            }

            salablegoodspriceDataService.ExecuteQuery("update stock set ismultiprice=true from salablegoodsprice where salablegoodsprice.barcode = stock.serialno", null);
            salablegoodspriceDataService.ExecuteQuery("update stock set ismultiprice=true from salablegoodsprice where salablegoodsprice.barcode = stock.barcode", null);

        }
        private void MigrateLREntry(MigrationData data, SortedDictionary<string, ColumnMap> columnmap,
           List<List<string>> values)
        {
            SupplierDataService suppliercompanyDataService = new SupplierDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            ReferencelistDataService referencelistDataService = new ReferencelistDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };


            List<LREntry> SSproducts = (from row in values.AsEnumerable()
                                        select new LREntry
                                        {
                                            Section = GetString(row, columnmap, "Section"),
                                            Company = GetString(row, columnmap, "Company"),
                                            Remark = GetString(row, columnmap, "Remark"),
                                            Supplier = GetString(row, columnmap, "Supplier"),
                                            EntryNo = GetString(row, columnmap, "EntryNo"),
                                            EntryDate = GetDate(row, columnmap, "EntryDate"),
                                            InvoiceNo = GetString(row, columnmap, "InvoiceNo"),
                                            Location = GetString(row, columnmap, "Location"),
                                            PurchaseManager = GetString(row, columnmap, "PurchaseManager"),
                                            InvoiceDate = GetDate(row, columnmap, "InvoiceDate"),
                                            Transport = GetString(row, columnmap, "Transport"),
                                            FromCity = GetString(row, columnmap, "FromCity"),
                                            ToCity = GetString(row, columnmap, "ToCity"),
                                            LRNo = GetString(row, columnmap, "LRNo"),
                                            LRDate = GetDate(row, columnmap, "LRDate"),
                                            BillValue = GetDecimal(row, columnmap, "BillValue"),
                                            ActualWeight = GetDecimal(row, columnmap, "ActualWeight"),
                                            ChargedWeight = GetDecimal(row, columnmap, "ChargedWeight"),
                                            NoofBundles = GetDecimal(row, columnmap, "NoofBundles"),
                                            LRAmount = GetDecimal(row, columnmap, "LRAmount")
                                        }).ToList();

            data.progress.totalrows = SSproducts.Count * 2;
            data.progress.completedrows = 0;
            suppliercompanyDataService.isMigration = true;
            var suppliers = suppliercompanyDataService.Search(new SupplierCreteria());
            var cities = referencelistDataService.Search(new ReferencelistCreteria()
            { type = "CITY" });
            var sections = referencelistDataService.Search(new ReferencelistCreteria()
            { type = "DIVISION" });
            var locations = referencelistDataService.Search(new ReferencelistCreteria()
            { type = "LOCATION" });
            CompanyDataService companyDataService = new CompanyDataService();
            var companylist = companyDataService.Search(new CompanyCreteria());
            EmployeeDataService employeeDataService = new EmployeeDataService();
            var employeelist = employeeDataService.Search(new EmployeeCreteria());
            TransportDataService transportDataService = new TransportDataService();
            var transports = transportDataService.Search(new TransportCreteria());
            CultureInfo enUS = new CultureInfo("en-US");
            LrentryDataService lrentryDataService = new LrentryDataService();
            List<Lrentry> items = new List<Lrentry>();
            foreach (var row in SSproducts)
            {
                Lrentry lrentry = new Lrentry();
                lrentry.lrentryno = row.EntryNo;
                if (row.InvoiceDate.Date == DateTime.MinValue)
                    lrentry.invoicedate = row.LRDate;
                if (string.IsNullOrEmpty(row.InvoiceNo))
                    lrentry.invoiceno = "LRPND-" + row.LRNo;
                lrentry.lrno = row.LRNo;
                lrentry.lrdate = row.LRDate;
                lrentry.lrentrydate = row.EntryDate;
                lrentry.weight = row.ActualWeight;
                lrentry.invoicedate = row.InvoiceDate;
                lrentry.invoiceno = row.InvoiceNo;
                lrentry.chargeweight = row.ChargedWeight;
                lrentry.noofbundels = Convert.ToInt32(row.NoofBundles);
                lrentry.totalamount = row.LRAmount;
                Supplier suppliercompany = null;
                if (!string.IsNullOrEmpty(row.Supplier))
                {
                    if (suppliers.Exists(x => x.name.ToUpper() == row.Supplier.ToUpper()))
                    {
                        suppliercompany = suppliers.Find(x => x.name.ToUpper() == row.Supplier.ToUpper());
                    }
                    if (suppliercompany == null)
                    {
                        suppliercompany = new Supplier() { name = row.Supplier, isactive = true };
                        suppliers.Add(suppliercompanyDataService.Insert(suppliercompany));
                    }
                }
                Transport transport = null;
                if (!string.IsNullOrEmpty(row.Transport))
                {
                    if (transports.Exists(x => x.name.ToUpper() == row.Transport.ToUpper()))
                    {
                        transport = transports.Find(x => x.name.ToUpper() == row.Transport.ToUpper());
                    }
                    if (transport == null)
                    {
                        transport = new Transport() { name = row.Transport, isactive = true };
                        transports.Add(transportDataService.Insert(transport));
                    }
                }
                Referencelist fromcity = null;
                if (!string.IsNullOrEmpty(row.FromCity))
                {
                    if (cities.Exists(x => x.name.ToUpper() == row.FromCity.ToUpper()))
                    {
                        fromcity = cities.Find(x => x.name.ToUpper() == row.FromCity.ToUpper());
                    }
                    if (fromcity == null)
                    {
                        fromcity = new Referencelist() { name = row.FromCity, type = "CITY", isactive = true };
                        cities.Add(referencelistDataService.Insert(fromcity));
                    }
                }
                Referencelist tocity = null;
                if (!string.IsNullOrEmpty(row.ToCity))
                {
                    if (cities.Exists(x => x.name.ToUpper() == row.ToCity.ToUpper()))
                    {
                        tocity = cities.Find(x => x.name.ToUpper() == row.ToCity.ToUpper());
                    }
                    if (tocity == null)
                    {
                        tocity = new Referencelist() { name = row.ToCity, type = "CITY", isactive = true };
                        cities.Add(referencelistDataService.Insert(tocity));
                    }
                }
                Referencelist section = null;
                if (!string.IsNullOrEmpty(row.Section))
                {
                    if (sections.Exists(x => x.name.ToUpper() == row.Section.ToUpper()))
                    {
                        section = sections.Find(x => x.name.ToUpper() == row.Section.ToUpper());
                    }
                    if (section == null)
                    {
                        section = new Referencelist() { name = row.Section, type = "DIVISION", isactive = true };
                        sections.Add(referencelistDataService.Insert(tocity));
                    }
                }
                Referencelist location = null;

                if (!string.IsNullOrEmpty(row.Location))
                {
                    if (locations.Exists(x => x.name.ToUpper() == row.Location.ToUpper()))
                    {
                        location = locations.Find(x => x.name.ToUpper() == row.Location.ToUpper());
                    }
                    if (location == null)
                    {
                        location = new Referencelist() { name = row.Section, type = "LOCATION", isactive = true };
                        locations.Add(referencelistDataService.Insert(tocity));
                    }
                }
                Company company = null;

                if (!string.IsNullOrEmpty(row.Company))
                {
                    if (companylist.Exists(x => x.name.ToUpper() == row.Company.ToUpper()))
                    {
                        company = companylist.Find(x => x.name.ToUpper() == row.Company.ToUpper());
                    }
                    if (company == null)
                    {
                        company = new Company() { name = row.Company, isactive = true };
                        companylist.Add(companyDataService.Insert(company));
                    }
                }
                Employee employee = null;
                if (!string.IsNullOrEmpty(row.PurchaseManager))
                {
                    if (employeelist.Exists(x => x.name.ToUpper() == row.PurchaseManager.ToUpper()))
                    {
                        employee = employeelist.Find(x => x.name.ToUpper() == row.PurchaseManager.ToUpper());
                    }
                    if (employee == null)
                    {
                        employee = new Employee() { name = row.PurchaseManager, isactive = true };
                        employeelist.Add(employeeDataService.Insert(employee));
                    }
                }
                lrentry.isactive = true;
                lrentry.locationid = location == null ? 0 : location.id;
                lrentry.retaillocationid = location == null ? 0 : location.id;
                lrentry.lrcompanyid = company == null ? 0 : company.id;
                lrentry.lrsectionid = section == null ? 0 : section.id;
                lrentry.fromlocationid = fromcity == null ? 0 : fromcity.id;
                lrentry.receivinglocationid = tocity == null ? 0 : tocity.id;
                lrentry.supplierid = suppliercompany == null ? 0 : suppliercompany.id;
                lrentry.transportid = transport == null ? 0 : transport.id;
                lrentry.purchasemanagerid = employee == null ? 0 : employee.id;
                lrentry.remarks = row.Remark;
                lrentry.totalamount = row.BillValue;
                items.Add(lrentry);
                data.progress.completedrows++;
            }
            data.progress.statuslist = new List<string>();
            foreach (var it in items)
            {
                if (it.supplierid == 0)
                {
                    data.progress.completedrows++;
                    continue;
                }
                it.id = 0;
                lrentryDataService.Insert(it);
                if (it.id == 0)
                {
                    data.progress.failedrows++;
                    data.progress.statuslist.Add(suppliers.Find(x => x.id == it.supplierid).name + " - " +
                    it.invoiceno + "(" + it.invoicedate.ToString("dd-MMM-yyyy") + ") - " + it.ErrorMessage);
                }
                else
                {
                    it.ErrorMessage = "";
                }
                data.progress.completedrows++;
            }
        }

        private void MigrateAttributes(MigrationData data, SortedDictionary<string, ColumnMap> columnmap,
            List<List<string>> values)
        {
            ProductsDataService productsDataService = new ProductsDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            BrandDataService brandDataService = new BrandDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            ItemsDataService itemsDataService = new ItemsDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            ReferencelistDataService referencelistDataService = new ReferencelistDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            var allref = referencelistDataService.Search(new Models.CustomReferenceItem() { type = "COLOUR,FIT,MATERIAL,PATTERN,SIZE,SLEEVE,STYLE,TYPE" });

            var allproduts = productsDataService.Search(new Models.ProductsCreteria());
            var allbrands = brandDataService.Search(new Models.BrandCreteria());
            List<Attribute> SSproducts = (from row in values.AsEnumerable()
                                          select new Attribute
                                          {
                                              Barcode = GetString(row, columnmap, "Barcode"),
                                              Brand = GetString(row, columnmap, "Brand"),
                                              Colour = GetString(row, columnmap, "Colour"),
                                              Fit = GetString(row, columnmap, "Fit"),
                                              Material = GetString(row, columnmap, "Material"),
                                              Pattern = GetString(row, columnmap, "Pattern"),
                                              PriceTag = GetString(row, columnmap, "PriceTag"),
                                              Product = GetString(row, columnmap, "Product"),
                                              Size = GetString(row, columnmap, "Size"),
                                              Sleeve = GetString(row, columnmap, "Sleeve"),
                                              Style = GetString(row, columnmap, "Style"),
                                              Type = GetString(row, columnmap, "Type"),
                                          }).ToList();

            data.progress.totalrows = SSproducts.Count * 2;
            data.progress.completedrows = 0;
            foreach (var barcodes in SSproducts.ChunkBy(2000))
            {
                string query = @"select stock.barcode,items.id,products.name product,
brand.name brand,colour.name colour,fit.name fit,
size.name size,style.name style,pattern.name pattern,
material.name material,type.name typename,sleeve.name sleeve
from stock
inner join items on items.id = stock.itemid
left outer join products on products.id = items.productid
left outer join brand brand on brand.id = items.brandid
left outer join referencelist colour on colour.id = colourid
left outer join referencelist size on size.id = sizeid
left outer join referencelist style on style.id = styleid
left outer join referencelist pattern on pattern.id = patternid
left outer join referencelist material on material.id = materialid
left outer join referencelist type on type.id = typeid
left outer join referencelist sleeve on sleeve.id = sleeveid
left outer join referencelist fit on fit.id = fitid
where stock.barcode in (" + String.Join(",", barcodes.ConvertAll(x => "'" + x.Barcode + "'")) + ")";
                using (var reader = itemsDataService.ExecuteReader(query, null))
                {
                    while (reader.Read())
                    {
                        string barcode = reader["barcode"] == DBNull.Value ? "" : reader["barcode"].ToString();
                        var product = SSproducts.Find(x => x.Barcode == barcode);
                        if (product != null)
                        {
                            product.itemid = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                            if (columnmap["productname"].index < 0)
                                product.Product = reader["product"] == DBNull.Value ? "" : reader["product"].ToString();
                            if (columnmap["brand"].index < 0)
                                product.Brand = reader["brand"] == DBNull.Value ? "" : reader["brand"].ToString();
                            if (columnmap["colour"].index < 0)
                                product.Colour = reader["colour"] == DBNull.Value ? "" : reader["colour"].ToString();
                            if (columnmap["fit"].index < 0)
                                product.Fit = reader["fit"] == DBNull.Value ? "" : reader["fit"].ToString();
                            if (columnmap["sleeve"].index < 0)
                                product.Sleeve = reader["sleeve"] == DBNull.Value ? "" : reader["sleeve"].ToString();
                            if (columnmap["material"].index < 0)
                                product.Material = reader["material"] == DBNull.Value ? "" : reader["material"].ToString();
                            if (columnmap["pattern"].index < 0)
                                product.Pattern = reader["pattern"] == DBNull.Value ? "" : reader["pattern"].ToString();
                            if (columnmap["style"].index < 0)
                                product.Style = reader["style"] == DBNull.Value ? "" : reader["style"].ToString();
                            if (columnmap["type"].index < 0)
                                product.Type = reader["typename"] == DBNull.Value ? "" : reader["typename"].ToString();
                            if (columnmap["size"].index < 0)
                                product.Size = reader["size"] == DBNull.Value ? "" : reader["size"].ToString();
                        }
                        data.progress.completedrows++;
                    }
                }
            }


            foreach (var attr in SSproducts.GroupBy(e => new
            {
                e.Brand,
                e.Colour,
                e.Fit,
                e.Material,
                e.Pattern,
                e.Product,
                e.PriceTag,
                e.Size,
                e.Sleeve,
                e.Style,
                e.Type
            }))
            {
                try
                {
                    long productid = allproduts.Exists(x => x.name == attr.Key.Product) ? allproduts.Find(x => x.name == attr.Key.Product).id : 0;
                    long brandid = allbrands.Exists(x => x.name == attr.Key.Brand) ? allbrands.Find(x => x.name == attr.Key.Brand).id : 0;

                    if (productid == 0)
                    {
                        allproduts.Add(productsDataService.Insert(new Products()
                        {
                            isactive = true,
                            name = attr.Key.Product,
                            sellingmode = 2
                        }));
                        productid = allproduts.Exists(x => x.name == attr.Key.Product) ? allproduts.Find(x => x.name == attr.Key.Product).id : 0;
                    }

                    if (brandid == 0)
                    {
                        allbrands.Add(brandDataService.Insert(new Brand()
                        {
                            isactive = true,
                            name = attr.Key.Brand
                        }));
                        brandid = allbrands.Exists(x => x.name == attr.Key.Brand) ? allbrands.Find(x => x.name == attr.Key.Brand).id : 0;
                    }

                    long materialid = GetReferenceID(referencelistDataService, allref, "MATERIAL", attr.Key.Material);
                    long patternid = GetReferenceID(referencelistDataService, allref, "PATTERN", attr.Key.Pattern);
                    long fitid = GetReferenceID(referencelistDataService, allref, "FIT", attr.Key.Fit);
                    long sleeveid = GetReferenceID(referencelistDataService, allref, "SLEEVE", attr.Key.Sleeve);
                    long colourid = GetReferenceID(referencelistDataService, allref, "COLOUR", attr.Key.Colour);
                    long sizeid = GetReferenceID(referencelistDataService, allref, "SIZE", attr.Key.Size);
                    long styleid = GetReferenceID(referencelistDataService, allref, "STYLE", attr.Key.Style);
                    long typeid = GetReferenceID(referencelistDataService, allref, "TYPE", attr.Key.Type);

                    var existitem = itemsDataService.Search(new Models.ItemsCreteria()
                    {
                        productmap = true,
                        productid = productid,
                        brandid = brandid,
                        materialid = materialid,
                        patternid = patternid,
                        fitid = fitid,
                        sleeveid = sleeveid,
                        colourid = colourid,
                        sizeid = sizeid,
                        styleid = styleid,
                        typeid = typeid
                    });
                    if (existitem.Count == 0)
                    {
                        existitem.Add(itemsDataService.Insert(new Models.Items()
                        {
                            fitname = attr.Key.Fit,
                            sleevename = attr.Key.Sleeve,
                            materialname = attr.Key.Material,
                            patternname = attr.Key.Pattern,
                            productname = attr.Key.Product,
                            printingname = attr.Key.Product,
                            colourname = attr.Key.Colour,
                            brandname = attr.Key.Brand,
                            sizename = attr.Key.Size,
                            stylename = attr.Key.Style,
                            typename = attr.Key.Type,
                            productid = productid,
                            brandid = brandid,
                            materialid = materialid,
                            patternid = patternid,
                            fitid = fitid,
                            sleeveid = sleeveid,
                            colourid = colourid,
                            sizeid = sizeid,
                            styleid = styleid,
                            typeid = typeid
                        }));
                    }
                    foreach (var stocks in attr.ToList().ChunkBy(1000))
                    {
                        string query = "Update stock set itemid=@itemid where barcode in (" + string.Join(",", stocks.ConvertAll(x => "'" + x.Barcode + "'")) + ")";
                        var param = new SortedList<string, object>();
                        param.Add("itemid", existitem[0].id);
                        itemsDataService.ExecuteQuery(query, param);
                    }
                    data.progress.completedrows += attr.Count();
                }
                catch (Exception exp)
                {
                    data.progress.failedrows += attr.Count();
                }
            }
        }

        private long GetReferenceID(ReferencelistDataService referencelistDataService,
            List<CustomReferenceItem> allref, string type, string value)
        {
            var ritem = allref.Find(x => x.type == type && x.name == value);
            if (ritem == null) return 0;
            return ritem.id;
        }


        public List<MigrationData> migrationDatas = new List<MigrationData>();
        public class ColumnMap
        {
            public int index { get; set; }
            public string columnname { get; set; }
            public string columnmap { get; set; }
            public string defaultvalue { get; set; }
        }

        private static CultureInfo enUS = new CultureInfo("en-US");
        public DateTime GetDate(List<string> source, SortedDictionary<string, ColumnMap> columnMap, string name)
        {
            name = name.ToLower();
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
                if (DateTime.TryParseExact(value, "dd/MM/yyyy", enUS, DateTimeStyles.None, out dt))
                    return dt;
                double d;
                if (double.TryParse(value, out d))
                    return DateTime.FromOADate(d);
            }
            return DateTime.MinValue;
        }

        public decimal GetDecimal(List<string> source, SortedDictionary<string, ColumnMap> columnMap, string name)
        {
            name = name.ToLower();
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

        public bool GetBool(List<string> source, SortedDictionary<string, ColumnMap> columnMap, string name)
        {
            name = name.ToLower();
            var str = GetString(source, columnMap, name);
            if (str.Length >= 1)
                return str[0] == 'T' || str[0] == 'Y' || str[0] == '1';
            return false;
        }
        public string GetString(List<string> source, SortedDictionary<string, ColumnMap> columnMap, string name)
        {
            string _name = name.ToLower();
            if (columnMap.ContainsKey(_name))
            {
                if (columnMap[_name].index >= 0 && source.Count > columnMap[_name].index)
                    return isEmpty(source[columnMap[_name].index]) ? "" : source[columnMap[_name].index].Trim();

                return isEmpty(columnMap[_name].defaultvalue) ? "" : columnMap[_name].defaultvalue;
            }
            return "";
        }

        public class LREntry
        {
            public string Company { get; set; }
            public string Section { get; set; }
            public string Remark { get; set; }
            public string Location { get; set; }
            public string PurchaseManager { get; set; }
            public string Supplier { get; set; }
            public long itemid { get; set; }
            public string InvoiceNo { get; set; }
            public string EntryNo { get; set; }
            public DateTime EntryDate { get; set; }
            public DateTime InvoiceDate { get; set; }
            public string Transport { get; set; }
            public string FromCity { get; set; }
            public string ToCity { get; set; }
            public string LRNo { get; set; }
            public DateTime LRDate { get; set; }
            public decimal ActualWeight { get; set; }
            public decimal BillValue { get; set; }
            public decimal ChargedWeight { get; set; }
            public decimal NoofBundles { get; set; }
            public decimal LRAmount { get; set; }
        }
        public class Attribute
        {
            public string Barcode { get; set; }
            public string Product { get; set; }
            public long itemid { get; set; }
            public string Brand { get; set; }
            public string Style { get; set; }
            public string Pattern { get; set; }
            public string Colour { get; set; }
            public string Material { get; set; }
            public string Size { get; set; }
            public string Fit { get; set; }
            public string Sleeve { get; set; }
            public string Type { get; set; }
            public string PriceTag { get; set; }
        }
        public class CustomerBalance
        {
            public string location { get; set; }
            public string company { get; set; }
            public string name { get; set; }
            public string mobile { get; set; }
            public decimal balance { get; set; }
            public DateTime billdate { get; set; }
            public string billno { get; set; }
        }

        public class SupplierBalance
        {
            public string name { get; set; }
            public string city { get; set; }
            public string company { get; set; }
            public decimal balance { get; set; }
            public DateTime invdate { get; set; }
            public string invno { get; set; }
            public DateTime returndate { get; internal set; }
            public string returnno { get; internal set; }
            public DateTime entrydate { get; internal set; }
            public DateTime duedate { get; internal set; }
        }
        internal NpgsqlConnection CreateConnection()
        {
            string connstring = System.Configuration.ConfigurationManager.ConnectionStrings["InfyPOS"].ConnectionString;
            return new NpgsqlConnection(connstring);
        }

        public void MigrateSupplierBalance(MigrationData data, SortedDictionary<string, ColumnMap> columnmap, List<List<string>> values)
        {
            InfyPOS.Data.CompanyDataService companyDataService = new InfyPOS.Data.CompanyDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            var companylist = companyDataService.Search(new InfyPOS.Models.CompanyCreteria()
            {
            });

            List<SupplierBalance> _supplierbalance = (from row in values.AsEnumerable()
                                                      select new SupplierBalance
                                                      {
                                                          name = GetString(row, columnmap, "suppliername"),
                                                          city = GetString(row, columnmap, "suppliercity"),
                                                          invno = GetString(row, columnmap, "invno"),
                                                          company = GetString(row, columnmap, "company"),
                                                          invdate = GetDate(row, columnmap, "invdate"),
                                                          entrydate = GetDate(row, columnmap, "entrydate"),
                                                          duedate = GetDate(row, columnmap, "duedate"),
                                                          balance = GetDecimal(row, columnmap, "balance")
                                                      }).ToList();


            InfyPOS.Data.SupplierDataService supplierDataService = new InfyPOS.Data.SupplierDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            var supplierlist = supplierDataService.Search(new SupplierCreteria()
            {
            });

            InfyPOS.Data.SuppliercompanyDataService suppliercompanyDataService = new InfyPOS.Data.SuppliercompanyDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            suppliercompanyDataService.ismigration = true;
            var suppliercompanylist = suppliercompanyDataService.Search(new SuppliercompanyCreteria()
            {
            });

            InfyPOS.Data.LrentryDataService lrentryDataService = new InfyPOS.Data.LrentryDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            InfyPOS.Data.LrinvoiceDataService lrinvoiceDataService = new InfyPOS.Data.LrinvoiceDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            lrinvoiceDataService.IsMigration = true;
            //var invoiceall = lrinvoiceDataService.Search(new InfyPOS.Models.LrinvoiceCreteria());

            try
            {
                NpgsqlTransaction transaction = null;
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    foreach (var item in _supplierbalance)
                    {
                        long companyid = 0;
                        long supplierid = 0;
                        long suppliercompanyid = 0;
                        if (companylist.Exists(e => e.code.ToLower() == item.company.ToLower() || e.name.ToLower() == item.company.ToLower()))
                            companyid = companylist.Find(e => e.code.ToLower() == item.company.ToLower() || e.name.ToLower() == item.company.ToLower()).id;
                        else
                            throw new Exception("Company Not found : " + item.company);

                        if (supplierlist.Exists(e => e.code.ToLower() == item.name.ToLower() || e.name.ToLower() == item.name.ToLower()))
                        {
                            supplierid = supplierlist.Find(e => e.code.ToLower() == item.name.ToLower() || e.name.ToLower() == item.name.ToLower()).id;
                        }
                        else
                        {
                            var custom = new InfyPOS.Models.Supplier()
                            {
                                name = item.name,
                                isactive = true,
                                createdby = -1,
                                companylist = new List<InfyPOS.Models.Suppliercompany>()
                            };
                            custom.companylist.Add(new Suppliercompany()
                            {
                                name = custom.name,
                                isactive = true,
                                createdby = -1,
                            });
                            supplierDataService.Insert(custom);
                            supplierlist.Add(custom);
                            supplierid = custom.id;
                        }

                        if (suppliercompanylist.Exists(e => e.supplierid == supplierid))
                            suppliercompanyid = suppliercompanylist.Find(e => e.supplierid == supplierid).id;
                        else
                        {
                            data.progress.failedrows++;
                            data.progress.ExceptionList.Add(new Exception("Supplier Not found : " + item.name));
                            continue;
                        }

                        InfyPOS.Models.Lrinvoice lrinvoice = new InfyPOS.Models.Lrinvoice();
                        lrinvoice.companyid = companyid;
                        lrinvoice.invoicedate = item.invdate;
                        lrinvoice.supplierid = supplierid;
                        lrinvoice.suppliercompanyid = suppliercompanyid;
                        lrinvoice.entrydate = item.entrydate != DateTime.MinValue ? item.entrydate : item.invdate;
                        lrinvoice.duedate = item.duedate != DateTime.MinValue ? item.duedate : item.invdate.AddDays(60);
                        lrinvoice.invoiceno = string.IsNullOrEmpty(item.invno) ? "" : item.invno;
                        lrinvoice.totalamount = item.balance;

                        lrinvoice.isactive = true;
                        lrinvoice.lrid = -5;
                        lrinvoice.version = 0;
                        lrinvoice.createdby = -5;
                        lrinvoice.createdon = DateTime.Now;
                        lrinvoice.modifiedby = -5;
                        lrinvoice.modifiedon = DateTime.Now;
                        lrinvoiceDataService.Insert(transaction, conn, lrinvoice);
                        data.progress.completedrows++;

                    }
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                data.progress.failedrows++;
            }
        }


        public void MigrateCustomerBalance(MigrationData data, SortedDictionary<string, ColumnMap> columnmap, List<List<string>> values)
        {
            InfyPOS.Data.CompanyDataService companyDataService = new InfyPOS.Data.CompanyDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            var companylist = companyDataService.Search(new InfyPOS.Models.CompanyCreteria()
            {
            });

            InfyPOS.Data.ReferencelistDataService brandDataService = new InfyPOS.Data.ReferencelistDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            var locationlist = brandDataService.Search(new InfyPOS.Models.ReferencelistCreteria() { type = "LOCATION" });

            List<CustomerBalance> SSproducts = (from row in values.AsEnumerable()
                                                select new CustomerBalance
                                                {
                                                    name = GetString(row, columnmap, "name"),
                                                    company = GetString(row, columnmap, "company"),
                                                    location = GetString(row, columnmap, "location"),
                                                    mobile = GetString(row, columnmap, "mobile"),
                                                    billno = GetString(row, columnmap, "billno"),
                                                    billdate = GetDate(row, columnmap, "billdate"),
                                                    balance = GetDecimal(row, columnmap, "balance"),
                                                }).ToList();

            InfyPOS.Data.CustomerDataService customerDataService = new InfyPOS.Data.CustomerDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            InfyPOS.Data.CustomerinvoiceDataService customerinvoiceDataService = new InfyPOS.Data.CustomerinvoiceDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            int billid = 1;
            foreach (var customer in SSproducts.GroupBy(e => new { e.name, e.mobile, e.company }))
            {
                try
                {
                    var clist = customerDataService.Search(new InfyPOS.Models.CustomerCreteria()
                    {
                        mobileno = customer.First().mobile
                    });
                    if (clist.Count == 0)
                    {
                        var custom = new InfyPOS.Models.Customer()
                        {
                            name = customer.First().name,
                            mobileno = customer.First().mobile,
                            isactive = true,
                            createdby = -1
                        };
                        customerDataService.Insert(custom);
                        clist.Add(custom);
                    }

                    foreach (var bills in customer)
                    {
                        if (string.IsNullOrEmpty(bills.company))
                            bills.company = companylist.OrderBy(e => e.id).First().name;
                        if (string.IsNullOrEmpty(bills.location))
                            bills.location = locationlist.OrderBy(e => e.id).First().name;

                        customerinvoiceDataService.Insert(new InfyPOS.Models.Customerinvoice()
                        {
                            billno = bills.billno,
                            billid = billid++ * -1,
                            transactiondate = bills.billdate,
                            customerid = clist[0].id,
                            receivable = bills.balance,
                            isactive = true,
                            companyid = companylist.First(e => e.name == bills.company).id,
                            type = 1
                        });
                    }

                    data.progress.completedrows++;
                }
                catch (Exception exp)
                {
                    data.progress.failedrows++;
                }
            }
        }

        public void MigrateDebitNotes(MigrationData data, SortedDictionary<string, ColumnMap> columnmap, List<List<string>> values)
        {

            InfyPOS.Data.CompanyDataService companyDataService = new InfyPOS.Data.CompanyDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            var companylist = companyDataService.Search(new InfyPOS.Models.CompanyCreteria()
            {
            });
            InfyPOS.Data.ReferencelistDataService referencelistDataService = new InfyPOS.Data.ReferencelistDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            var citylist = referencelistDataService.Search(new InfyPOS.Models.ReferencelistCreteria()
            {
                type = "CITY"
            });

            List<InfyPOS.Models.Company> companies = new List<InfyPOS.Models.Company>();

            List<SupplierBalance> SSproducts = (from row in values.AsEnumerable()
                                                select new SupplierBalance
                                                {
                                                    name = GetString(row, columnmap, "name"),
                                                    city = GetString(row, columnmap, "city"),
                                                    invno = GetString(row, columnmap, "invno"),
                                                    returnno = GetString(row, columnmap, "returnno"),
                                                    company = GetString(row, columnmap, "company"),
                                                    invdate = GetDate(row, columnmap, "invdate"),
                                                    returndate = GetDate(row, columnmap, "returndate"),
                                                    balance = GetDecimal(row, columnmap, "balance")
                                                }).ToList();


            InfyPOS.Common.UserClaim context = new InfyPOS.Common.UserClaim()
            { UserID = -5 };

            InfyPOS.Data.SupplierDataService supplierDataService = new InfyPOS.Data.SupplierDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            supplierDataService.Context = context;
            supplierDataService.isMigration = true;
            InfyPOS.Data.LrinvoicereturnDataService lrinvoiceDataService = new InfyPOS.Data.LrinvoicereturnDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            lrinvoiceDataService.Context = context;
            lrinvoiceDataService.IsMigration = true;
            List<long> pendinginvoiceids = new List<long>();
            foreach (var customer in SSproducts)
            {
                try
                {
                    long cityid = 0;
                    if (citylist.Exists(e => e.name == customer.city))
                        cityid = citylist.Find(e => e.name == customer.city).id;

                    var clist = supplierDataService.Search(new InfyPOS.Models.SupplierCreteria()
                    {
                        name = customer.name,
                        cityid = cityid,
                        allsupplier = true
                    });

                    if (clist.Count == 0)
                    {
                        var custom = new InfyPOS.Models.Supplier()
                        {
                            name = customer.name,
                            isactive = true,
                            createdby = -1,
                            companylist = new List<InfyPOS.Models.Suppliercompany>()
                        };
                        custom.companylist.Add(new Suppliercompany()
                        {
                            name = custom.name,
                            cityid = cityid,
                            isactive = true,
                            createdby = -1,
                        });
                        supplierDataService.Insert(custom);
                        clist.Add(custom);
                    }

                    long companyid = 0;
                    if (companylist.Exists(e => e.code.ToLower() == customer.company.ToLower() || e.name.ToLower() == customer.company.ToLower()))
                        companyid = companylist.Find(e => e.code.ToLower() == customer.company.ToLower() || e.name.ToLower() == customer.company.ToLower()).id;

                    var lrinvoice = lrinvoiceDataService.Insert(new InfyPOS.Models.Lrinvoicereturn()
                    {
                        supplierid = clist[0].id,
                        suppliercompanyid = clist[0].companylist[0].id,
                        companyid = companyid,
                        isactive = true,
                        createdby = -5,
                        returndate = customer.returndate,
                        amount = customer.balance,
                        total = customer.balance,
                        returnno = customer.returnno,
                        Lrinvoicereturnitems = new List<Lrinvoicereturnitems>(),
                        remarks = "Return Invoice No : " + customer.invno + " On " + customer.invdate.ToString("dd-MM-yyyy")
                    });

                    data.progress.completedrows++;
                }
                catch (Exception exp)
                {
                    data.progress.failedrows++;
                }
            }
        }

        public void MigrateCustomer(MigrationData data, SortedDictionary<string, ColumnMap> columnmap, List<List<string>> values)
        {
            InfyPOS.Data.CustomerDataService customerDataService = new InfyPOS.Data.CustomerDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            customerDataService.isMigration = true;
            InfyPOS.Data.CustomerrewardsDataService customerrewardsDataService = new InfyPOS.Data.CustomerrewardsDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            var existing = customerDataService.Search(new InfyPOS.Models.CustomerCreteria() { limit = -9 });
            InfyPOS.Data.ReferencelistDataService refrdataservice = new InfyPOS.Data.ReferencelistDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            refrdataservice.isMigration = true;
            var citylist = refrdataservice.Search(new InfyPOS.Models.ReferencelistCreteria()
            {
                type = "CITY"
            });

            var statelist = refrdataservice.Search(new InfyPOS.Models.ReferencelistCreteria()
            {
                type = "STATE"
            });
            var religionlist = refrdataservice.Search(new InfyPOS.Models.ReferencelistCreteria()
            {
                type = "RELIGION"
            });

            var locationlist = refrdataservice.Search(new InfyPOS.Models.ReferencelistCreteria()
            {
                type = "LOCATION"
            });

            var categorylist = refrdataservice.Search(new InfyPOS.Models.ReferencelistCreteria()
            {
                type = "CUSTOMERCATEGORY"
            });

            foreach (var source in values)
            {
                try
                {
                    string code = GetString(source, columnmap, "code");
                    string customer = GetString(source, columnmap, "name");
                    string billingname = GetString(source, columnmap, "billingname");
                    string address = GetString(source, columnmap, "address");
                    string deliveryaddress = GetString(source, columnmap, "deliveryaddress");
                    string city = GetString(source, columnmap, "cityid");
                    string state = GetString(source, columnmap, "stateid");
                    string category = GetString(source, columnmap, "categoryid");
                    string area = GetString(source, columnmap, "area");
                    string gst = GetString(source, columnmap, "gstno");
                    string religion = GetString(source, columnmap, "religionid");
                    string loyeltyno = GetString(source, columnmap, "loyeltyno");
                    string emailid = GetString(source, columnmap, "emailid");
                    DateTime createdon = GetDate(source, columnmap, "createdon");
                    string gender = GetString(source, columnmap, "gender");
                    DateTime dateofbirth = GetDate(source, columnmap, "dateofbirth");
                    DateTime marriagedate = GetDate(source, columnmap, "marriagedate");
                    string pincode = GetString(source, columnmap, "pincode");
                    string mobile = GetString(source, columnmap, "mobileno");
                    decimal reward = GetDecimal(source, columnmap, "loyelty");
                    decimal margin = GetDecimal(source, columnmap, "margin");
                    string locationcode = GetString(source, columnmap, "locationcode");

                    long cityid = 0;
                    long statid = 0;
                    long religionid = 0;
                    long locationid = 0;
                    long categoryid = 0;

                    if (locationlist.Exists(e => e.code.ToLower() == locationcode.ToLower() || e.name.ToLower() == locationcode.ToLower()))
                        locationid = locationlist.First(e => e.code.ToLower() == locationcode.ToLower() || e.name.ToLower() == locationcode.ToLower()).id;

                    if (!string.IsNullOrEmpty(city))
                    {
                        if (!citylist.Exists(e => e.name.ToLower() == city.ToLower()))
                        {
                            InfyPOS.Models.Referencelist mref = new InfyPOS.Models.Referencelist();
                            mref.code = city;
                            mref.name = city;
                            mref.isactive = true;
                            mref.type = "CITY";
                            refrdataservice.Insert(mref);
                            cityid = mref.id;
                            citylist.Add(mref);
                        }
                        else
                        {
                            cityid = citylist.First(e => e.name.ToLower() == city.ToLower()).id;
                        }
                    }

                    if (!string.IsNullOrEmpty(state))
                    {
                        if (!statelist.Exists(e => e.name.ToLower() == state.ToLower()))
                        {
                            InfyPOS.Models.Referencelist mref = new InfyPOS.Models.Referencelist();
                            mref.code = state;
                            mref.name = state;
                            mref.isactive = true;
                            mref.type = "STATE";
                            refrdataservice.Insert(mref);
                            statid = mref.id;
                            statelist.Add(mref);
                        }
                        else
                        {
                            statid = statelist.First(e => e.name.ToLower() == state.ToLower()).id;
                        }
                    }

                    if (!string.IsNullOrEmpty(religion))
                    {
                        if (!religionlist.Exists(e => e.name.ToLower() == religion.ToLower()))
                        {
                            InfyPOS.Models.Referencelist mref = new InfyPOS.Models.Referencelist();
                            mref.code = religion;
                            mref.name = religion;
                            mref.isactive = true;
                            mref.type = "RELIGION";
                            refrdataservice.Insert(mref);
                            religionid = mref.id;
                            religionlist.Add(mref);
                        }
                        else
                        {
                            religionid = religionlist.First(e => e.name.ToLower() == religion.ToLower()).id;
                        }
                    }

                    if (!string.IsNullOrEmpty(category))
                    {
                        if (!categorylist.Exists(e => e.name.ToLower() == category.ToLower()))
                        {
                            InfyPOS.Models.Referencelist mref = new InfyPOS.Models.Referencelist();
                            mref.code = category;
                            mref.name = category;
                            mref.isactive = true;
                            mref.type = "CUSTOMERCATEGORY";
                            refrdataservice.Insert(mref);
                            categoryid = mref.id;
                            categorylist.Add(mref);
                        }
                        else
                        {
                            categoryid = categorylist.First(e => e.name.ToLower() == category.ToLower()).id;
                        }
                    }
                    if (
                         (!string.IsNullOrEmpty(mobile) && existing.Exists(ex => ex.mobileno == mobile)) ||
                         (!string.IsNullOrEmpty(gst) && existing.Exists(ex => ex.gstno == gst)) ||
                         (!string.IsNullOrEmpty(gst) && existing.Exists(ex => ex.code == code)))
                    {
                        InfyPOS.Models.Customer customermodel = null;

                        if (!string.IsNullOrEmpty(mobile))
                            customermodel = existing.Find(ex => ex.mobileno == mobile);

                        if (!string.IsNullOrEmpty(gst))
                            customermodel = existing.Find(ex => ex.gstno == gst);

                        if (!string.IsNullOrEmpty(code))
                        {
                            customermodel = null;
                            if (!string.IsNullOrEmpty(gst) && !string.IsNullOrEmpty(code))
                                customermodel = existing.Find(ex => ex.code == code &&
                                                ex.gstno == gst);
                            else
                                customermodel = existing.Find(ex => ex.code == code);
                        }

                        if (customermodel == null)
                        {
                            customermodel = new InfyPOS.Models.Customer();
                            customermodel.isactive = true;
                            customermodel.version = 1;
                            customermodel.customertype = 4;
                            customermodel.mobileno = mobile;
                            customermodel.address = FormatAddress(address);
                            customermodel.deliveryaddress = FormatAddress(deliveryaddress);
                            customermodel.name = customer;
                            customermodel.code = code;
                            customermodel.billingname = billingname;
                            customermodel.area = area;
                            customermodel.loyeltyno = loyeltyno;
                            customermodel.ccmargin = margin;
                            customermodel.loyelty = reward;
                            customermodel.stateid = statid;
                            customermodel.cityid = cityid;
                            customermodel.pincode = pincode;
                            customermodel.gstno = gst;
                            customermodel.gender = gender;
                            customermodel.emailid = emailid;
                            customermodel.dateofbirth = dateofbirth;
                            customermodel.marriagedate = marriagedate;
                            customermodel.createdon = createdon;
                            customermodel.religionid = religionid;
                            customermodel.registredlocationid = locationid;
                            customermodel.categoryid = categoryid;

                            customerDataService.Insert(customermodel);
                            if (customermodel.id > 0 && reward > 0)
                            {
                                customerrewardsDataService.Insert(new InfyPOS.Models.Customerrewards()
                                {
                                    customerid = customermodel.id,
                                    isactive = true,
                                    version = 1,
                                    rewards = (long)reward,
                                    adjusted = 0,
                                    transactiondate = DateTime.Now,
                                    billvalue = reward,
                                    expiry = DateTime.MaxValue
                                });
                            }
                        }
                        else
                        {
                            if (reward > 0)
                                customermodel.loyelty = reward;

                            customermodel.mobileno = mobile;

                            if (!string.IsNullOrEmpty(address))
                                customermodel.address = FormatAddress(address);
                            if (!string.IsNullOrEmpty(deliveryaddress))
                                customermodel.deliveryaddress = FormatAddress(deliveryaddress);
                            if (!string.IsNullOrEmpty(customer))
                                customermodel.name = customer;
                            if (!string.IsNullOrEmpty(code))
                                customermodel.code = code;
                            if (!string.IsNullOrEmpty(billingname))
                                customermodel.billingname = billingname;
                            if (!string.IsNullOrEmpty(area))
                                customermodel.area = area;
                            if (margin > 0)
                                customermodel.ccmargin = margin;
                            if (!string.IsNullOrEmpty(loyeltyno))
                                customermodel.loyeltyno = loyeltyno;
                            if (statid > 0)
                                customermodel.stateid = statid;
                            if (cityid > 0)
                                customermodel.cityid = cityid;
                            if (!string.IsNullOrEmpty(pincode))
                                customermodel.pincode = pincode;
                            if (!string.IsNullOrEmpty(gst))
                                customermodel.gstno = gst;
                            if (!string.IsNullOrEmpty(gender))
                                customermodel.gender = gender;
                            if (!string.IsNullOrEmpty(emailid))
                                customermodel.emailid = emailid;
                            if (dateofbirth != DateTime.MinValue)
                                customermodel.dateofbirth = dateofbirth;
                            if (marriagedate != DateTime.MinValue)
                                customermodel.marriagedate = marriagedate;
                            if (religionid > 0)
                                customermodel.religionid = religionid;
                            if (locationid > 0)
                                customermodel.registredlocationid = locationid;
                            if (categoryid > 0)
                                customermodel.categoryid = categoryid;

                            customerDataService.Update(customermodel);
                            if (customermodel.id > 0 && reward > 0)
                            {
                                var existingrewards = customerrewardsDataService.Search(new Models.CustomerrewardsCreteria()
                                {
                                    customerid = customermodel.id
                                });
                                if (existingrewards.Count > 0)
                                {
                                    customerrewardsDataService.Delete(existingrewards.OrderBy(e => e.id).First());
                                }

                                customerrewardsDataService.Insert(new InfyPOS.Models.Customerrewards()
                                {
                                    customerid = customermodel.id,
                                    isactive = true,
                                    version = 1,
                                    rewards = (long)reward,
                                    adjusted = 0,
                                    transactiondate = DateTime.Now,
                                    billvalue = reward,
                                    expiry = DateTime.MaxValue
                                });
                            }
                        }
                    }
                    else
                    {
                        InfyPOS.Models.Customer customermodel = new InfyPOS.Models.Customer();
                        customermodel.isactive = true;
                        customermodel.version = 1;
                        customermodel.customertype = 4;
                        customermodel.mobileno = mobile;
                        customermodel.address = FormatAddress(address);
                        customermodel.deliveryaddress = FormatAddress(deliveryaddress);
                        customermodel.name = customer;
                        customermodel.code = code;
                        customermodel.billingname = billingname;
                        customermodel.area = area;
                        customermodel.loyeltyno = loyeltyno;
                        customermodel.ccmargin = margin;
                        customermodel.loyelty = reward;
                        customermodel.stateid = statid;
                        customermodel.cityid = cityid;
                        customermodel.pincode = pincode;
                        customermodel.gstno = gst;
                        customermodel.gender = gender;
                        customermodel.emailid = emailid;
                        customermodel.dateofbirth = dateofbirth;
                        customermodel.marriagedate = marriagedate;
                        customermodel.createdon = createdon;
                        customermodel.religionid = religionid;
                        customermodel.registredlocationid = locationid;
                        customermodel.categoryid = categoryid;

                        customerDataService.Insert(customermodel);
                        if (customermodel.id > 0 && reward > 0)
                        {
                            customerrewardsDataService.Insert(new InfyPOS.Models.Customerrewards()
                            {
                                customerid = customermodel.id,
                                isactive = true,
                                version = 1,
                                rewards = (long)reward,
                                adjusted = 0,
                                transactiondate = DateTime.Now,
                                billvalue = reward,
                                expiry = DateTime.MaxValue
                            });
                        }
                    }

                    data.progress.completedrows++;
                }
                catch (Exception exp)
                {
                    data.progress.failedrows++;
                    source.Insert(0, exp.Message);
                    data.progress.faileddata.Add(source);
                }
            }
        }
        private string FormatAddress(string addr)
        {
            var formatedaddress = addr.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            if (formatedaddress.Length > 1) return addr;

            string line1 = "", line2 = "", line3 = "";
            if (!string.IsNullOrEmpty(addr))
            {
                var addresslist = addr.Split(new string[] { "," }, StringSplitOptions.None);
                if (addresslist.Length > 0)
                {
                    foreach (var str in addresslist)
                    {
                        if (line1.Length < 25) line1 += str + ",";
                        else if (line1.Length > 25 && line2.Length < 25) line2 += str + ",";
                        else if (line1.Length > 25 && line2.Length > 25 && line3.Length < 25) line3 += str + ",";
                    }
                    if (string.IsNullOrEmpty(line3) && string.IsNullOrEmpty(line2))
                        return line1.ToUpper();
                    else if (string.IsNullOrEmpty(line3) && !string.IsNullOrEmpty(line2))
                        return line1.ToUpper() + "\r\n" + line2.ToUpper();
                    else
                        return line1.ToUpper() + "\r\n" + line2.ToUpper() + "\r\n" + line3.ToUpper();
                }
                return string.Empty;
            }
            return string.Empty;

        }
        public void MigrateSupplier(MigrationData data, SortedDictionary<string, ColumnMap> columnmap, List<List<string>> values)
        {
            data.progress.hasstarted = true;


            InfyPOS.Data.SupplierDataService customerDataService = new InfyPOS.Data.SupplierDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            customerDataService.isMigration = true;
            var existing = customerDataService.Search(new InfyPOS.Models.SupplierCreteria() { allsupplier = true });

            InfyPOS.Data.ReferencelistDataService refrdataservice = new InfyPOS.Data.ReferencelistDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            refrdataservice.isMigration = true;

            var citylist = refrdataservice.Search(new InfyPOS.Models.ReferencelistCreteria()
            {
                type = "CITY"
            });

            var statelist = refrdataservice.Search(new InfyPOS.Models.ReferencelistCreteria()
            {
                type = "STATE"
            });

            var banklist = refrdataservice.Search(new InfyPOS.Models.ReferencelistCreteria()
            {
                type = "BANK"
            });

            InfyPOS.Data.AgentDataService agentdataservice = new InfyPOS.Data.AgentDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            var agentlist = agentdataservice.Search(new InfyPOS.Models.AgentCreteria()
            {

            });

            foreach (var source in values)
            {
                try
                {
                    string city = GetString(source, columnmap, "SupplierCompany.cityid");
                    string state = GetString(source, columnmap, "SupplierCompany.stateid");
                    string agent = GetString(source, columnmap, "SupplierCompany.agentid");
                    string bank = GetString(source, columnmap, "SupplierCompany.bankid");
                    long cityid = 0;
                    long statid = 0;
                    long agentid = 0;
                    long bankid = 0;

                    if (!string.IsNullOrEmpty(city))
                    {
                        if (!citylist.Exists(e => e.name.ToLower() == city.ToLower()))
                        {
                            InfyPOS.Models.Referencelist mref = new InfyPOS.Models.Referencelist();
                            mref.code = city;
                            mref.name = city;
                            mref.isactive = true;
                            mref.type = "CITY";
                            refrdataservice.Insert(mref);
                            cityid = mref.id;
                            citylist.Add(mref);
                        }
                        else
                        {
                            cityid = citylist.First(e => e.name.ToLower() == city.ToLower()).id;
                        }
                    }

                    if (!string.IsNullOrEmpty(state))
                    {
                        if (!statelist.Exists(e => e.name.ToLower() == state.ToLower()))
                        {
                            InfyPOS.Models.Referencelist mref = new InfyPOS.Models.Referencelist();
                            mref.code = state;
                            mref.name = state;
                            mref.isactive = true;
                            mref.type = "STATE";
                            refrdataservice.Insert(mref);
                            statid = mref.id;
                            statelist.Add(mref);
                        }
                        else
                        {
                            statid = statelist.First(e => e.name.ToLower() == state.ToLower()).id;
                        }
                    }

                    if (!string.IsNullOrEmpty(bank))
                    {
                        if (!banklist.Exists(e => e.name.ToLower() == bank.ToLower()))
                        {
                            InfyPOS.Models.Referencelist mref = new InfyPOS.Models.Referencelist();
                            mref.code = bank;
                            mref.name = bank;
                            mref.isactive = true;
                            mref.type = "BANK";
                            refrdataservice.Insert(mref);
                            bankid = mref.id;
                            banklist.Add(mref);
                        }
                        else
                        {
                            bankid = banklist.First(e => e.name.ToLower() == bank.ToLower()).id;
                        }
                    }

                    if (agentlist.Exists(e => e.name.ToLower() == agent.ToLower()))
                    {
                        agentid = agentlist.First(e => e.name.ToLower() == agent.ToLower()).id;
                    }

                    var suppliername = GetString(source, columnmap, "name");
                    if (existing.Exists(ex => ex.name.ToLower().Trim() == suppliername.ToLower().Trim()))
                    {
                        var suppliermodel = existing.Find(ex => ex.name.ToLower() == suppliername.ToLower());

                        if (!string.IsNullOrEmpty(GetString(source, columnmap, "code")))
                            suppliermodel.code = GetString(source, columnmap, "code");
                        suppliermodel.supportsourcesupplier = GetBool(source, columnmap, "supportsourcesupplier");

                        InfyPOS.Models.Suppliercompany suppliercompany = null;
                        if (suppliermodel.companylist != null && suppliermodel.companylist.Count > 0)
                            suppliercompany = suppliermodel.companylist.First();
                        else
                        {
                            suppliermodel.companylist = new List<InfyPOS.Models.Suppliercompany>();
                            suppliercompany = new InfyPOS.Models.Suppliercompany();
                            suppliermodel.companylist.Add(suppliercompany);
                            suppliercompany.version = 1;
                            suppliercompany.isactive = true;
                        }

                        if (!string.IsNullOrEmpty(suppliername))
                            suppliercompany.name = suppliername;
                        if (!string.IsNullOrEmpty(city))
                            suppliercompany.cityname = city;
                        if (agentid > 0)
                            suppliercompany.agentid = agentid;
                        if (!string.IsNullOrEmpty(GetString(source, columnmap, "contactnumber")))
                            suppliercompany.companycontactno = GetString(source, columnmap, "contactnumber");
                        if (!string.IsNullOrEmpty(GetString(source, columnmap, "contactperson")))
                            suppliercompany.contactperson = GetString(source, columnmap, "contactperson");
                        if (!string.IsNullOrEmpty(GetString(source, columnmap, "emailid")))
                            suppliercompany.companyemailid = GetString(source, columnmap, "emailid");
                        if (!string.IsNullOrEmpty(GetString(source, columnmap, "SupplierCompany.pincode")))
                            suppliercompany.pincode = GetString(source, columnmap, "SupplierCompany.pincode");
                        if (!string.IsNullOrEmpty(GetString(source, columnmap, "SupplierCompany.tinanddate")))
                            suppliercompany.tinanddate = GetString(source, columnmap, "SupplierCompany.tinanddate");
                        if (!string.IsNullOrEmpty(GetString(source, columnmap, "SupplierCompany.address")))
                            suppliercompany.address = GetString(source, columnmap, "SupplierCompany.address");
                        if (!string.IsNullOrEmpty(GetString(source, columnmap, "suppliercompany.address")))
                            suppliercompany.address = FormatAddress(suppliercompany.address);
                        suppliercompany.isinterstatesale = GetBool(source, columnmap, "SupplierCompany.isinterstatesale");

                        if (!string.IsNullOrEmpty(GetString(source, columnmap, "SupplierCompany.tan")))
                            suppliercompany.tan = GetString(source, columnmap, "SupplierCompany.tan");
                        if (!string.IsNullOrEmpty(GetString(source, columnmap, "SupplierCompany.pan")))
                            suppliercompany.pan = GetString(source, columnmap, "SupplierCompany.pan");
                        if (!string.IsNullOrEmpty(GetString(source, columnmap, "SupplierCompany.msmeno")))
                            suppliercompany.msmeno = GetString(source, columnmap, "SupplierCompany.msmeno");

                        if (!string.IsNullOrEmpty(GetString(source, columnmap, "SupplierCompany.bankbranch")))
                            suppliercompany.bankbranch = GetString(source, columnmap, "SupplierCompany.bankbranch");
                        if (!string.IsNullOrEmpty(GetString(source, columnmap, "SupplierCompany.bankaccountname")))
                            suppliercompany.bankaccountname = GetString(source, columnmap, "SupplierCompany.bankaccountname");
                        if (!string.IsNullOrEmpty(GetString(source, columnmap, "SupplierCompany.bankifci")))
                            suppliercompany.bankifci = GetString(source, columnmap, "SupplierCompany.bankifci");
                        if (!string.IsNullOrEmpty(GetString(source, columnmap, "SupplierCompany.bankaccountno")))
                            suppliercompany.bankaccountno = GetString(source, columnmap, "SupplierCompany.bankaccountno");

                        if (cityid > 0)
                            suppliercompany.cityid = cityid;
                        if (statid > 0)
                            suppliercompany.stateid = statid;
                        if (bankid > 0)
                            suppliercompany.bankid = bankid;

                        suppliermodel.contactperson = suppliercompany.contactperson;
                        suppliermodel.contactnumber = suppliercompany.companycontactno;
                        suppliermodel.emailid = suppliercompany.companyemailid;

                        customerDataService.Update(suppliermodel);
                        data.progress.updatedrows++;
                    }
                    else
                    {
                        InfyPOS.Models.Supplier _supplier = new InfyPOS.Models.Supplier();
                        _supplier.isactive = true;
                        _supplier.version = 1;

                        InfyPOS.Models.Suppliercompany suppliercompany = null;
                        _supplier.companylist = new List<InfyPOS.Models.Suppliercompany>();
                        suppliercompany = new InfyPOS.Models.Suppliercompany();
                        _supplier.companylist.Add(suppliercompany);

                        suppliercompany.version = 1;
                        suppliercompany.isactive = true;
                        suppliercompany.name = suppliername;
                        suppliercompany.cityname = city;
                        suppliercompany.agentid = agentid;
                        suppliercompany.companycontactno = GetString(source, columnmap, "contactnumber");
                        suppliercompany.contactperson = GetString(source, columnmap, "contactperson");
                        suppliercompany.companyemailid = GetString(source, columnmap, "emailid");
                        suppliercompany.pincode = GetString(source, columnmap, "SupplierCompany.pincode");
                        suppliercompany.tinanddate = GetString(source, columnmap, "SupplierCompany.tinanddate");
                        suppliercompany.address = GetString(source, columnmap, "SupplierCompany.address");
                        suppliercompany.address = FormatAddress(suppliercompany.address);
                        suppliercompany.isinterstatesale = GetBool(source, columnmap, "SupplierCompany.isinterstatesale");

                        suppliercompany.tan = GetString(source, columnmap, "SupplierCompany.tan");
                        suppliercompany.pan = GetString(source, columnmap, "SupplierCompany.pan");
                        suppliercompany.msmeno = GetString(source, columnmap, "SupplierCompany.msmeno");
                        suppliercompany.bankbranch = GetString(source, columnmap, "SupplierCompany.bankbranch");
                        suppliercompany.bankaccountname = GetString(source, columnmap, "SupplierCompany.bankaccountname");
                        suppliercompany.bankifci = GetString(source, columnmap, "SupplierCompany.bankifci");
                        suppliercompany.bankaccountno = GetString(source, columnmap, "SupplierCompany.bankaccountno");

                        suppliercompany.cityid = cityid;
                        suppliercompany.stateid = statid;
                        suppliercompany.bankid = bankid;

                        _supplier.contactperson = suppliercompany.contactperson;
                        _supplier.contactnumber = suppliercompany.companycontactno;
                        _supplier.emailid = suppliercompany.companyemailid;
                        _supplier.code = GetString(source, columnmap, "code");
                        _supplier.supportsourcesupplier = GetBool(source, columnmap, "supportsourcesupplier");
                        _supplier.name = suppliercompany.name;

                        customerDataService.Insert(_supplier);
                        data.progress.createdrows++;
                    }
                    data.progress.completedrows++;
                }
                catch (Exception exp)
                {
                    data.progress.failedrows++;
                    source.Insert(0, exp.Message);
                    data.progress.faileddata.Add(source);
                }
            }
            data.progress.hascompleted = true;
        }

        public void MigrateUANStock(MigrationData data, SortedDictionary<string, ColumnMap> columnmap, List<List<string>> values)
        {
            data.progress.hasstarted = true;
            InfyPOS.Data.StockDataService stockDataService = new InfyPOS.Data.StockDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            InfyPOS.Data.SalablegoodsDataService salableDataService = new InfyPOS.Data.SalablegoodsDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            MfgdesignDataService mfgdesignDataService = new MfgdesignDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            CompanyDataService companydatasource = new CompanyDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            var allcompany = companydatasource.Search(new CompanyCreteria());
            ReferencelistDataService referencelistDataService = new ReferencelistDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            var alllocations = referencelistDataService.SearchList(new ReferencelistCreteria() { type = "LOCATION" });
            //var alluids = mfgdesignDataService.SearchUANItems();
            long lastid = 0;
            TaxDataService taxDataService = new TaxDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            var allprodtaxes = taxDataService.TaxProducts(new List<long>());

            using (var reader = stockDataService.ExecuteReader("select barcode,id from stock order by id desc limit 1", null))
            {
                if (reader.Read())
                {
                    var lastbarcode = reader.GetString(0);
                    try
                    {
                        lastid = lastbarcode.LongFromBase62();
                    }
                    catch (Exception exp)
                    {
                        lastid = reader.GetInt64(1);
                    }
                }
            }


            while (true)
            {
                lastid++;
                var param = new SortedList<string, object>();
                param.Add("barcode", lastid.ToBase62());
                using (var reader = stockDataService.ExecuteReader("select id from stock where barcode=@barcode limit 1", param))
                {
                    if (!reader.Read())
                    {
                        break;
                    }
                }
            }
            data.progress.faileddata = new List<List<string>>();
            List<string> alluans = new List<string>();
            foreach (var source in values)
            {
                lastid++;
                try
                {
                    var UAN = GetString(source, columnmap, "UAN");
                    if (!alluans.Contains(UAN))
                        alluans.Add(UAN);
                }
                catch (Exception exp)
                {

                }
            }
            var alluids = mfgdesignDataService.SearchUANItem(alluans);

            foreach (var source in values)
            {
                lastid++;
                try
                {
                    var UAN = GetString(source, columnmap, "UAN");
                    var LocationCode = GetString(source, columnmap, "LocationCode");
                    var CompanyCode = GetString(source, columnmap, "CompanyCode");
                    var CLOSINGQTY = GetDecimal(source, columnmap, "CLOSINGQTY");
                    var cost = GetDecimal(source, columnmap, "cost");

                    long locationid = alllocations.Exists(x => x.code == LocationCode) ? alllocations.Find(x => x.code == LocationCode).id : 0;
                    long companyid = allcompany.Exists(x => x.code == CompanyCode) ? allcompany.Find(x => x.code == CompanyCode).id : 0;

                    if (locationid > 0 && companyid > 0 && alluids.Exists(x => x.uid == UAN))
                    {
                        var sitem = alluids.Find(x => x.uid == UAN);
                        InfyPOS.Models.Stock stock = new InfyPOS.Models.Stock();
                        stock.barcode = lastid.ToBase62();
                        stock.dealerprice = sitem.dealerprice;
                        stock.designid = sitem.code;
                        stock.printingname = sitem.pname;
                        stock.productid = sitem.productid;
                        stock.itemid = sitem.itemid;
                        stock.hsncode = sitem.hsn;
                        stock.basebuyingprice = cost;
                        stock.buyingprice = cost;
                        stock.buyingqty = CLOSINGQTY;
                        stock.displaymrp = sitem.mrp;
                        stock.eprice = sitem.dealerprice;
                        stock.invoiceitemid = -1;
                        stock.ibuyingprice = cost;
                        stock.isactive = true;
                        stock.price = sitem.mrp;
                        stock.serialno = sitem.uid;
                        stock.qty = CLOSINGQTY;
                        stock.sizeid = sitem.sizeid;
                        stock.transfered = CLOSINGQTY;
                        stock.companyid = companyid;
                        if (cost > 0 && allprodtaxes.ContainsKey(sitem.productid))
                        {
                            var tax = allprodtaxes[sitem.productid].taxsplit.Find(cost);
                            if (tax != null &&
                                tax.cgst + tax.sgst > 0)
                            {
                                stock.ibuyingprice = Math.Round(cost + (cost * (tax.cgst + tax.sgst) / 100), 0);
                            }
                            tax = allprodtaxes[sitem.productid].taxsplit.Find(sitem.mrp);
                            if (tax != null &&
                                tax.cgst + tax.sgst > 0)
                            {
                                stock.eprice = (sitem.mrp / (100 + tax.cgst + tax.sgst)) * 100;
                            }
                        }

                        stockDataService.Insert(stock);

                        Salablegoods salablegoods = new Salablegoods();
                        salablegoods.barcode = stock.barcode;
                        salablegoods.iqty = stock.qty;
                        salablegoods.stockid = stock.id;
                        salablegoods.companyid = companyid;
                        salablegoods.locationid = locationid;
                        salablegoods.itemid = stock.itemid;
                        salablegoods.price = stock.price;
                        salablegoods.sellingname = stock.printingname;
                        salablegoods.isactive = true;
                        salableDataService.Insert(salablegoods);
                    }
                    else
                    {
                        data.progress.failedrows++;
                        data.progress.faileddata.Add(source);
                    }
                    data.progress.completedrows++;
                }
                catch (Exception exp)
                {
                    data.progress.failedrows++;
                    source.Insert(0, exp.Message);
                    data.progress.faileddata.Add(source);
                }
            }
            data.progress.hascompleted = true;
        }

        public void MigrateTax(MigrationData data, SortedDictionary<string, ColumnMap> columnmap, List<List<string>> values)
        {
            data.progress.hasstarted = true;

            InfyPOS.Data.TaxDataService taxDataService = new InfyPOS.Data.TaxDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            var existing = taxDataService.Search(new InfyPOS.Models.TaxCreteria() { });

            foreach (var source in values)
            {
                try
                {
                    var name = GetString(source, columnmap, "name");

                    if (existing.Exists(ex => ex.name == name))
                    {
                        InfyPOS.Models.Tax taxmodel = existing.Find(ex => ex.name == name);
                        taxmodel.isactive = true;
                        taxmodel.version = 1;
                        taxmodel.code = GetString(source, columnmap, "code");
                        taxmodel.name = GetString(source, columnmap, "name");
                        taxmodel.taxpercentage = GetDecimal(source, columnmap, "percentage");
                        taxDataService.Update(taxmodel);
                        data.progress.updatedrows++;
                    }
                    else
                    {
                        InfyPOS.Models.Tax taxmodel = new InfyPOS.Models.Tax();
                        taxmodel.isactive = true;
                        taxmodel.version = 1;
                        taxmodel.code = GetString(source, columnmap, "code");
                        taxmodel.name = GetString(source, columnmap, "name");
                        taxmodel.taxpercentage = GetDecimal(source, columnmap, "percentage");
                        taxDataService.Insert(taxmodel);
                        data.progress.createdrows++;
                    }
                    data.progress.completedrows++;
                }
                catch (Exception exp)
                {
                    data.progress.failedrows++;
                    source.Insert(0, exp.Message);
                    data.progress.faileddata.Add(source);
                }
            }
            data.progress.hascompleted = true;
        }

        public void MigrateProduct(MigrationData data, SortedDictionary<string, ColumnMap> columnmap, List<List<string>> values)
        {
            data.progress.hasstarted = true;

            InfyPOS.Data.ProductsDataService productDataService = new InfyPOS.Data.ProductsDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            productDataService.isMigration = true;
            var existing = productDataService.Search(new InfyPOS.Models.ProductsCreteria() { });

            InfyPOS.Data.TaxDataService taxdataservice = new InfyPOS.Data.TaxDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            var taxlist = taxdataservice.Search(new InfyPOS.Models.TaxCreteria() { });

            InfyPOS.Data.ReferencelistDataService refrdataservice = new InfyPOS.Data.ReferencelistDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            refrdataservice.isMigration = true;
            var sectionlist = refrdataservice.Search(new InfyPOS.Models.ReferencelistCreteria()
            {
                type = "DIVISION"
            });

            var productgrouplist = refrdataservice.Search(new InfyPOS.Models.ReferencelistCreteria()
            {
                type = "PRODUCTGROUPS"
            });

            foreach (var source in values)
            {
                try
                {
                    var name = GetString(source, columnmap, "name");
                    var purchasetax = GetString(source, columnmap, "purchasetax");
                    var saletax = GetString(source, columnmap, "saletax");
                    var sectionname = GetString(source, columnmap, "section");
                    var productgroup = GetString(source, columnmap, "productgroup");
                    var hsncode = GetString(source, columnmap, "hsncode");
                    decimal ptax, stax;
                    long salescounterid = 0;
                    long productgroupid = 0;

                    if (!sectionlist.Exists(e => e.name.ToLower() == sectionname.ToLower()))
                    {
                        InfyPOS.Models.Referencelist mref = new InfyPOS.Models.Referencelist();
                        mref.code = sectionname;
                        mref.name = sectionname;
                        mref.isactive = true;
                        mref.type = "DIVISION";
                        refrdataservice.Insert(mref);
                        salescounterid = mref.id;
                        sectionlist.Add(mref);
                    }
                    else
                    {
                        salescounterid = sectionlist.First(e => e.name.ToLower() == sectionname.ToLower()).id;
                    }

                    if (!productgrouplist.Exists(e => e.name.ToLower() == productgroup.ToLower()))
                    {
                        InfyPOS.Models.Referencelist mref = new InfyPOS.Models.Referencelist();
                        mref.code = productgroup;
                        mref.name = productgroup;
                        mref.isactive = true;
                        mref.type = "PRODUCTGROUPS";
                        refrdataservice.Insert(mref);
                        productgroupid = mref.id;
                        productgrouplist.Add(mref);
                    }
                    else
                    {
                        productgroupid = productgrouplist.First(e => e.name.ToLower() == productgroup.ToLower()).id;
                    }

                    if (!taxlist.Exists(e => e.name == purchasetax))
                    {
                        InfyPOS.Models.Tax taxmodel = new InfyPOS.Models.Tax();
                        taxmodel.isactive = true;
                        taxmodel.version = 1;
                        taxmodel.code = purchasetax;
                        taxmodel.name = purchasetax;
                        if (decimal.TryParse(purchasetax, out ptax))
                            taxmodel.taxpercentage = ptax;
                        taxdataservice.Insert(taxmodel);
                        taxlist.Add(taxmodel);
                    }
                    if (!taxlist.Exists(e => e.name == saletax))
                    {
                        InfyPOS.Models.Tax taxmodel = new InfyPOS.Models.Tax();
                        taxmodel.isactive = true;
                        taxmodel.version = 1;
                        taxmodel.code = saletax;
                        taxmodel.name = saletax;
                        if (decimal.TryParse(saletax, out stax))
                            taxmodel.taxpercentage = stax;
                        taxdataservice.Insert(taxmodel);
                        taxlist.Add(taxmodel);
                    }

                    if (existing.Exists(ex => ex.name == name))
                    {
                        InfyPOS.Models.Products productmodel = existing.Find(ex => ex.name == name);
                        productmodel.isactive = true;
                        productmodel.version = 1;
                        productmodel.code = GetString(source, columnmap, "code");
                        productmodel.name = GetString(source, columnmap, "name");

                        if (!string.IsNullOrEmpty(GetString(source, columnmap, "hsncode")))
                            productmodel.hsncode = GetString(source, columnmap, "hsncode");
                        if (salescounterid > 0)
                            productmodel.salescounterid = salescounterid;
                        if (productgroupid > 0)
                            productmodel.productgroup = productgroupid;
                        productmodel.barcodemode = 2;

                        string _sellingmode = GetString(source, columnmap, "sellingmode");
                        if (string.IsNullOrEmpty(_sellingmode))
                            productmodel.sellingmode = 2;
                        else if (_sellingmode.ToLower() == "1" || _sellingmode.ToLower() == "piece" || _sellingmode.ToLower() == "unique" || _sellingmode.ToLower() == "single")
                            productmodel.sellingmode = 1;
                        else if (_sellingmode.ToLower() == "2" || _sellingmode.ToLower() == "pack" || _sellingmode.ToLower() == "number")
                            productmodel.sellingmode = 2;
                        else if (_sellingmode.ToLower() == "3" || _sellingmode.ToLower() == "cut" || _sellingmode.ToLower() == "meter")
                            productmodel.sellingmode = 3;

                        productmodel.purchasetaxid = taxlist.First(e => e.name == purchasetax).id;
                        productmodel.salestaxid = taxlist.First(e => e.name == saletax).id;
                        productDataService.Update(productmodel);
                        data.progress.updatedrows++;
                    }
                    else
                    {
                        InfyPOS.Models.Products productsmodel = new InfyPOS.Models.Products();
                        productsmodel.isactive = true;
                        productsmodel.version = 1;
                        productsmodel.code = GetString(source, columnmap, "code");
                        productsmodel.name = GetString(source, columnmap, "name");
                        if (!string.IsNullOrEmpty(GetString(source, columnmap, "hsncode")))
                            productsmodel.hsncode = GetString(source, columnmap, "hsncode");
                        if (salescounterid > 0)
                            productsmodel.salescounterid = salescounterid;
                        if (productgroupid > 0)
                            productsmodel.productgroup = productgroupid;
                        productsmodel.barcodemode = 2;

                        string _sellingmode = GetString(source, columnmap, "sellingmode");
                        if (string.IsNullOrEmpty(_sellingmode))
                            productsmodel.sellingmode = 2;
                        else if (_sellingmode.ToLower() == "1" || _sellingmode.ToLower() == "piece" || _sellingmode.ToLower() == "unique" || _sellingmode.ToLower() == "single")
                            productsmodel.sellingmode = 1;
                        else if (_sellingmode.ToLower() == "2" || _sellingmode.ToLower() == "pack" || _sellingmode.ToLower() == "number")
                            productsmodel.sellingmode = 2;
                        else if (_sellingmode.ToLower() == "3" || _sellingmode.ToLower() == "cut" || _sellingmode.ToLower() == "meter")
                            productsmodel.sellingmode = 3;

                        productsmodel.purchasetaxid = taxlist.First(e => e.name == purchasetax).id;
                        productsmodel.salestaxid = taxlist.First(e => e.name == saletax).id;
                        productDataService.Insert(productsmodel);
                        data.progress.createdrows++;
                    }
                    data.progress.completedrows++;
                }
                catch (Exception exp)
                {
                    data.progress.failedrows++;
                    source.Insert(0, exp.Message);
                    data.progress.faileddata.Add(source);
                }
            }
            data.progress.hascompleted = true;
        }

        public long getReferenceId(List<InfyPOS.Models.Referencelist> existlist,
            InfyPOS.Data.ReferencelistDataService refservie,
            string type, string value)
        {
            if (!string.IsNullOrEmpty(value)) value = value.ToLower();
            if (existlist.Exists(e => e.name.ToLower() == value))
                return existlist.Find(e => e.name.ToLower() == value).id;

            var result = refservie.Insert(new Models.Referencelist()
            {
                type = type,
                name = value,
                isactive = true
            });

            existlist.Add(result);
            return result.id;
        }

        public void MigrateLocation(MigrationData data, SortedDictionary<string, ColumnMap> columnmap, List<List<string>> values)
        {
            data.progress.hasstarted = true;

            var companylist = new Data.CompanyDataService().Search(new Models.CompanyCreteria());

            InfyPOS.Data.ReferencelistDataService brandDataService = new InfyPOS.Data.ReferencelistDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            var existing = brandDataService.Search(new InfyPOS.Models.ReferencelistCreteria() { type = "LOCATION" });
            var statelist = brandDataService.Search(new InfyPOS.Models.ReferencelistCreteria() { type = "STATE" });
            var citylist = brandDataService.Search(new InfyPOS.Models.ReferencelistCreteria() { type = "CITY" });

            foreach (var source in values)
            {
                try
                {
                    var name = GetString(source, columnmap, "name");

                    if (existing.Exists(ex => ex.name == name))
                    {
                        InfyPOS.Models.Referencelist brandmodel = existing.Find(ex => ex.name == name);
                        brandmodel.isactive = true;
                        brandmodel.version = 1;
                        brandmodel.code = GetString(source, columnmap, "code");
                        brandmodel.name = GetString(source, columnmap, "name");
                        string tvalue = string.IsNullOrEmpty(GetString(source, columnmap, "company")) ? "" : GetString(source, columnmap, "company").ToLower();
                        brandmodel.additionalinfo = new Models.Referencelist.Attribute();
                        brandmodel.additionalinfo.locationconfig = new Models.Referencelist.LocationConfig();
                        brandmodel.additionalinfo.parentid = companylist.Exists(e => e.name.ToLower() == tvalue) ?
                            companylist.Find(e => e.name.ToLower() == tvalue).id : 0;
                        brandmodel.additionalinfo.address = GetString(source, columnmap, "address");
                        brandmodel.additionalinfo.locationconfig.cityid = getReferenceId(citylist,
                            brandDataService, "CITY", GetString(source, columnmap, "city"));
                        brandmodel.additionalinfo.locationconfig.cityid = getReferenceId(statelist,
                            brandDataService, "STATE", GetString(source, columnmap, "state"));
                        brandmodel.additionalinfo.iswarehouse = GetBool(source, columnmap, "warehouse");
                        brandmodel.additionalinfo.autosettlement = GetBool(source, columnmap, "autosettlement");
                        brandmodel.additionalinfo.ccmargin = GetDecimal(source, columnmap, "margin");
                        brandmodel.additionalinfo.locationconfig.gst = GetString(source, columnmap, "gst");
                        brandmodel.additionalinfo.locationconfig.contactno = GetString(source, columnmap, "contactno");
                        brandDataService.Update(brandmodel);
                        data.progress.updatedrows++;
                    }
                    else
                    {
                        InfyPOS.Models.Referencelist brandmodel = new InfyPOS.Models.Referencelist();
                        brandmodel.isactive = true;
                        brandmodel.version = 1;
                        brandmodel.code = GetString(source, columnmap, "code");
                        brandmodel.name = GetString(source, columnmap, "name");
                        string tvalue = string.IsNullOrEmpty(GetString(source, columnmap, "company")) ? "" : GetString(source, columnmap, "company").ToLower();
                        brandmodel.additionalinfo = new Models.Referencelist.Attribute();
                        brandmodel.additionalinfo.locationconfig = new Models.Referencelist.LocationConfig();
                        brandmodel.additionalinfo.parentid = companylist.Exists(e => e.name.ToLower() == tvalue) ?
                            companylist.Find(e => e.name.ToLower() == tvalue).id : 0;
                        brandmodel.additionalinfo.address = GetString(source, columnmap, "address");
                        brandmodel.additionalinfo.locationconfig.cityid = getReferenceId(citylist,
                            brandDataService, "CITY", GetString(source, columnmap, "city"));
                        brandmodel.additionalinfo.locationconfig.cityid = getReferenceId(statelist,
                            brandDataService, "STATE", GetString(source, columnmap, "state"));
                        brandmodel.additionalinfo.iswarehouse = GetBool(source, columnmap, "warehouse");
                        brandmodel.additionalinfo.autosettlement = GetBool(source, columnmap, "autosettlement");
                        brandmodel.additionalinfo.ccmargin = GetDecimal(source, columnmap, "margin");
                        brandmodel.additionalinfo.locationconfig.gst = GetString(source, columnmap, "gst");
                        brandmodel.additionalinfo.locationconfig.contactno = GetString(source, columnmap, "contactno");
                        brandDataService.Insert(brandmodel);
                        data.progress.createdrows++;
                    }
                    data.progress.completedrows++;
                }
                catch (Exception exp)
                {
                    data.progress.failedrows++;
                    source.Insert(0, exp.Message);
                    data.progress.faileddata.Add(source);
                }
            }
            data.progress.hascompleted = true;
        }

        public void MigrateDesign(MigrationData data, SortedDictionary<string, ColumnMap> columnmap, List<List<string>> values)
        {
            data.progress.hasstarted = true;

            var companylist = new Data.CompanyDataService().Search(new Models.CompanyCreteria());

            InfyPOS.Data.ReferencelistDataService refdatalist = new InfyPOS.Data.ReferencelistDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            InfyPOS.Data.ProductsDataService proddata = new InfyPOS.Data.ProductsDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            var prodlist = proddata.Search(new Models.ProductsCreteria());
            foreach (var item in values.GroupBy(e => GetString(e, columnmap, "product")))
            {
                if (!prodlist.Exists(e => e.name.ToLower() == item.Key.ToLower()))
                {
                    prodlist.Add(proddata.Insert(new Models.Products()
                    {
                        name = item.Key,
                        isactive = true
                    }));
                }
            }
            var fabricdata = new Data.BOMMasterDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            var fabriclist = fabricdata.Search(new Models.BOMMasterCreteria());
            foreach (var item in values.GroupBy(e => GetString(e, columnmap, "fabric")))
            {
                if (!fabriclist.Exists(e => e.name.ToLower() == item.Key.ToLower()))
                {
                    fabriclist.Add(fabricdata.Insert(new Models.BOMMaster()
                    {
                        name = item.Key,
                        isactive = true,
                        coretypeid = 1
                    }));
                }
            }


            var colourlist = refdatalist.Search(new InfyPOS.Models.ReferencelistCreteria() { type = "COLOUR" });
            var sleevelist = refdatalist.Search(new InfyPOS.Models.ReferencelistCreteria() { type = "SLEEVE" });
            var designdata = new Data.MfgdesignDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            var existing = designdata.Search(new Models.MfgdesignCreteria()
            {
            });

            foreach (var source in values.GroupBy(e => GetString(e, columnmap, "code")))
            {
                try
                {
                    var code = GetString(source.First(), columnmap, "code");
                    if (!existing.Exists(ex => ex.code == code))
                    {
                        InfyPOS.Models.Mfgdesign mfgdesign = new InfyPOS.Models.Mfgdesign();
                        mfgdesign.isactive = true;
                        mfgdesign.version = 1;
                        mfgdesign.code = GetString(source.First(), columnmap, "code");
                        mfgdesign.name = GetString(source.First(), columnmap, "name");
                        mfgdesign.productid = prodlist.Find(e => e.name.ToLower() == GetString(source.First(), columnmap, "product").ToLower()).id;
                        mfgdesign.sleeveid = getReferenceId(sleevelist,
                            refdatalist, "SLEEVE", GetString(source.First(), columnmap, "sleeve"));
                        mfgdesign.fabricid = fabriclist.Find(e => e.name.ToLower() == GetString(source.First(), columnmap, "fabric").ToLower()).id;
                        mfgdesign.mfgyear = Convert.ToInt32(GetDecimal(source.First(), columnmap, "year"));
                        mfgdesign.mfgmonth = Convert.ToInt32(GetDecimal(source.First(), columnmap, "month"));
                        mfgdesign.sizegroup = new Models.Mfgdesign.Sizegroup()
                        {
                            items = new List<Models.Mfgdesign.Sizegroup.Item>()
                        };
                        foreach (var item in source)
                        {
                            Models.Mfgdesign.Sizegroup.Item sizeitem = new Models.Mfgdesign.Sizegroup.Item();
                            sizeitem.code = GetString(item, columnmap, "sizecode");
                            sizeitem.colourid = getReferenceId(colourlist,
                            refdatalist, "COLOUR", GetString(item, columnmap, "colour"));
                            sizeitem.cost = GetDecimal(item, columnmap, "cost");
                            sizeitem.dealerprice = GetDecimal(item, columnmap, "b2b");
                            sizeitem.salerate = GetDecimal(item, columnmap, "retail");
                            sizeitem.wsrate = GetDecimal(item, columnmap, "franchaise");
                            sizeitem.uid = GetString(item, columnmap, "uan");
                            mfgdesign.sizegroup.items.Add(sizeitem);
                        }
                        designdata.Insert(mfgdesign);
                        data.progress.createdrows += source.Count();
                    }
                    data.progress.completedrows += source.Count();
                }
                catch (Exception exp)
                {
                    data.progress.failedrows += source.Count();
                    source.First().Insert(0, exp.Message);
                    data.progress.faileddata.Add(source.First());
                }
            }
            data.progress.hascompleted = true;
        }

        public class MigratingItem
        {
            public string product { get; set; }
            public string brand { get; set; }
            public string stylename { get; set; }
            public string type { get; set; }
            public string supplier { get; set; }
            public string suppliercode { get; set; }
            public string colour { get; set; }
            public string size { get; set; }
            public string material { get; set; }
            public string pattern { get; set; }
            public string fit { get; set; }
            public string sleeve { get; set; }
            public string printing { get; set; }
            public string code { get; set; }
            public string gln { get; set; }
            public string producttype { get; set; }
            public bool supportpo { get; set; }
            public string productgroup { get; set; }
            public decimal purchaserate { get; set; }
            public decimal taxpercentage { get; set; }
            public string glndesignid { get; set; }
            public bool showlist { get; set; }
            public decimal salerate { get; set; }
            public decimal dealerprice { get; set; }
            [JsonIgnore]
            public List<string> source { get; set; }
            public decimal mrprate { get; set; }
            public bool exist { get; set; }
            public List<string> customfields { get; set; }
        }

        public void MigrateItems(MigrationData data, SortedDictionary<string, ColumnMap> columnmap, List<List<string>> values)
        {
            data.progress.hasstarted = true;

            InfyPOS.Data.ItemsDataService itemDataService = new InfyPOS.Data.ItemsDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            var existing = itemDataService.Search(new InfyPOS.Models.ItemsCreteria() { });

            InfyPOS.Data.BrandDataService brandDataService = new InfyPOS.Data.BrandDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            var brandlist = brandDataService.Search(new InfyPOS.Models.BrandCreteria() { });

            InfyPOS.Data.ProductsDataService productDataService = new InfyPOS.Data.ProductsDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            var productlist = productDataService.Search(new InfyPOS.Models.ProductsCreteria() { });

            InfyPOS.Data.ReferencelistDataService referenceDataService = new InfyPOS.Data.ReferencelistDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            var reflist = referenceDataService.SearchList(new Models.ReferencelistCreteria()
            {
                types = new List<string>("STYLE,PATTERN,COLOUR,SIZE,SLEEVE,FIT,TYPE,MATERIAL,PRODUCTGROUP".Split(','))
            });


            bool hascustomfields = false;
            List<string> customfields = new List<string>();
            if (ConfigManager.Instance(itemDataService.Context.OrganizationID).Config.system_customfields &&
               CustomFieldsManager.Instance(itemDataService.Context.OrganizationID).Config != null &&
               CustomFieldsManager.Instance(itemDataService.Context.OrganizationID).Config.Count > 0)
            {
                hascustomfields = true;
                foreach (var field in CustomFieldsManager.Instance(itemDataService.Context.OrganizationID).Config)
                {
                    customfields.Add(field.name);
                }
            }

            List<MigratingItem> migrationDatas = new List<MigratingItem>();
            foreach (var source in values)
            {
                try
                {
                    Decimal d;
                    MigratingItem migratingItem = new MigratingItem();
                    migratingItem.source = source;
                    migratingItem.brand = GetString(source, columnmap, "brand");
                    migratingItem.code = GetString(source, columnmap, "code");
                    migratingItem.colour = GetString(source, columnmap, "colour");
                    migratingItem.fit = GetString(source, columnmap, "fit");
                    migratingItem.gln = GetString(source, columnmap, "gln");
                    migratingItem.material = GetString(source, columnmap, "material");
                    migratingItem.pattern = GetString(source, columnmap, "pattern");
                    migratingItem.printing = GetString(source, columnmap, "printing");
                    migratingItem.product = GetString(source, columnmap, "product");
                    migratingItem.producttype = GetString(source, columnmap, "producttype");
                    migratingItem.productgroup = GetString(source, columnmap, "productgroup");
                    if (decimal.TryParse(GetString(source, columnmap, "salerate"), out d))
                    {
                        migratingItem.salerate = d;
                    }
                    if (decimal.TryParse(GetString(source, columnmap, "purchaserate"), out d))
                    {
                        migratingItem.purchaserate = d;
                    }
                    if (decimal.TryParse(GetString(source, columnmap, "mrprate"), out d))
                    {
                        migratingItem.mrprate = d;
                    }
                    if (decimal.TryParse(GetString(source, columnmap, "taxpercentage"), out d))
                    {
                        migratingItem.taxpercentage = d;
                    }
                    migratingItem.supportpo = GetString(source, columnmap, "supportpo").ToUpper() == "TRUE";
                    migratingItem.showlist = GetString(source, columnmap, "showlist").ToUpper() == "TRUE";
                    migratingItem.size = GetString(source, columnmap, "size");
                    migratingItem.sleeve = GetString(source, columnmap, "sleeve");
                    migratingItem.stylename = GetString(source, columnmap, "style");
                    migratingItem.type = GetString(source, columnmap, "type");
                    if (hascustomfields)
                    {
                        migratingItem.customfields = new List<string>();
                        foreach (var fld in customfields)
                        {
                            migratingItem.customfields.Add(GetString(source, columnmap, fld));
                        }
                    }
                    migrationDatas.Add(migratingItem);
                }
                catch (Exception exp)
                {
                    source.Insert(0, exp.Message);
                    data.progress.faileddata.Add(source);
                }
            }

            List<Supplier> supplierlist = null;
            InfyPOS.Data.SupplierDataService supplierDataService = new InfyPOS.Data.SupplierDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            if (migrationDatas.Exists(x => !string.IsNullOrEmpty(x.supplier)))
            {
                supplierlist = supplierDataService.Search(new InfyPOS.Models.SupplierCreteria() { });
            }
            List<Tax> taxlist = null;
            InfyPOS.Data.TaxDataService taxDataService = new InfyPOS.Data.TaxDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            if (migrationDatas.Exists(x => x.taxpercentage > 0))
            {
                taxlist = taxDataService.Search(new InfyPOS.Models.TaxCreteria() { });
            }


            foreach (var source in migrationDatas)
            {
                try
                {
                    Models.Items items = new Models.Items();
                    items.gln = source.gln;

                    items.salerate = source.salerate;
                    items.mrp = source.mrprate;
                    items.purchaserate = source.purchaserate;

                    items.allowsearch = source.showlist;
                    items.createdby = -1;
                    items.createdon = DateTime.Now;
                    items.isactive = true;
                    items.support_gln = !string.IsNullOrEmpty(source.gln);
                    items.productname = source.product;
                    if (!productlist.Exists(e => e.name == source.product))
                    {
                        long productgrpid = 0;
                        int productype = 0;
                        long taxid = 0;
                        if (source.taxpercentage > 0)
                        {
                            if (!taxlist.Exists(x => x.taxpercentage == source.taxpercentage))
                            {
                                taxlist.Add(taxDataService.Insert(new Tax()
                                {
                                    name = source.taxpercentage + "%",
                                    taxpercentage = source.taxpercentage
                                }));
                            }
                            taxid = taxlist.Find(x => x.taxpercentage == source.taxpercentage).id;
                        }
                        switch (source.producttype.ToUpper())
                        {
                            case "FMCG":
                                productype = 1;
                                break;
                            case "GROCERIES":
                                productype = 2;
                                break;
                            case "VEGITABLE":
                                productype = 3;
                                break;
                            case "FOOD":
                                productype = 4;
                                break;
                            case "MOBILE":
                                productype = 6;
                                break;
                            case "ACCESSORIES":
                                productype = 7;
                                break;
                            case "JEWELLERY":
                                productype = 8;
                                break;
                            case "CONSUMABLES":
                                productype = 9;
                                break;
                        }

                        if (!string.IsNullOrEmpty(source.productgroup))
                        {
                            if (!reflist.Exists(x => x.type == "PRODUCTGROUP" &&
                            x.name == source.productgroup))
                            {
                                var newitem = referenceDataService.Insert(new Models.Referencelist()
                                {
                                    name = source.productgroup,
                                    type = "PRODUCTGROUP",
                                    isactive = true
                                });
                                reflist.Add(new ReferenceItem()
                                {
                                    id = newitem.id,
                                    name = newitem.name,
                                    type = newitem.type
                                });
                            }
                            productgrpid = reflist.Find(x => x.type == "PRODUCTGROUP" &&
                            x.name == source.productgroup).id;
                        }

                        productlist.Add(productDataService.Insert(new Models.Products()
                        {
                            name = source.product,
                            producttype = productype,
                            productgroup = productgrpid,
                            autopo = source.supportpo,
                            purchasetaxid = taxid,
                            salestaxid = taxid,
                            SellingMode = Models.Products.sellingMode.Pack,
                            isactive = true
                        }));
                    }
                    items.productid = productlist.Find(e => e.name == source.product).id;

                    if (!string.IsNullOrEmpty(source.brand))
                    {
                        items.brandname = source.brand;
                        if (!brandlist.Exists(e => e.name == source.brand))
                        {
                            brandlist.Add(brandDataService.Insert(new Models.Brand()
                            {
                                name = source.brand,
                                isactive = true
                            }));
                        }
                        items.brandid = brandlist.Find(e => e.name == source.brand).id;
                    }

                    items.printingname = string.IsNullOrEmpty(source.printing) ? items.productname : source.printing;
                    items.sellingname = string.IsNullOrEmpty(source.printing) ? items.productname : source.printing;
                    items.code = source.code;

                    if (!string.IsNullOrEmpty(source.colour))
                    {
                        items.colourname = source.colour;
                        if (!reflist.Exists(e => e.name == source.colour))
                        {
                            var newitem = referenceDataService.Insert(new Models.Referencelist()
                            {
                                name = source.colour,
                                type = "COLOUR",
                                isactive = true
                            });
                            reflist.Add(new ReferenceItem()
                            {
                                id = newitem.id,
                                name = newitem.name,
                                type = newitem.type
                            });
                        }
                        items.colourid = reflist.Find(e => e.name == source.colour).id;
                    }

                    if (!string.IsNullOrEmpty(source.size))
                    {
                        items.sizename = source.size;
                        if (!reflist.Exists(e => e.name == source.size))
                        {
                            var newitem = referenceDataService.Insert(new Models.Referencelist()
                            {
                                name = source.size,
                                type = "SIZE",
                                isactive = true
                            });
                            reflist.Add(new ReferenceItem()
                            {
                                id = newitem.id,
                                name = newitem.name,
                                type = newitem.type
                            });

                        }
                        items.sizeid = reflist.Find(e => e.name == source.size).id;
                    }

                    if (!string.IsNullOrEmpty(source.pattern))
                    {
                        items.patternname = source.pattern;
                        if (!reflist.Exists(e => e.name == source.pattern))
                        {
                            var newitem = referenceDataService.Insert(new Models.Referencelist()
                            {
                                name = source.pattern,
                                type = "PATTERN",
                                isactive = true
                            });
                            reflist.Add(new ReferenceItem()
                            {
                                id = newitem.id,
                                name = newitem.name,
                                type = newitem.type
                            });

                        }
                        items.patternid = reflist.Find(e => e.name == source.pattern).id;
                    }

                    if (!string.IsNullOrEmpty(source.material))
                    {
                        items.materialname = source.material;
                        if (!reflist.Exists(e => e.name == source.material))
                        {
                            var newitem = referenceDataService.Insert(new Models.Referencelist()
                            {
                                name = source.material,
                                type = "MATERIAL",
                                isactive = true
                            });
                            reflist.Add(new ReferenceItem()
                            {
                                id = newitem.id,
                                name = newitem.name,
                                type = newitem.type
                            });
                        }
                        items.materialid = reflist.Find(e => e.name == source.material).id;
                    }

                    if (!string.IsNullOrEmpty(source.stylename))
                    {
                        items.stylename = source.stylename;
                        if (!reflist.Exists(e => e.name == source.stylename))
                        {
                            var newitem = referenceDataService.Insert(new Models.Referencelist()
                            {
                                name = source.stylename,
                                type = "STYLE",
                                isactive = true
                            });
                            reflist.Add(new ReferenceItem()
                            {
                                id = newitem.id,
                                name = newitem.name,
                                type = newitem.type
                            });
                        }
                        items.styleid = reflist.Find(e => e.name == source.stylename).id;
                    }

                    if (!string.IsNullOrEmpty(source.type))
                    {
                        items.typename = source.type;
                        if (!reflist.Exists(e => e.name == source.type))
                        {
                            var newitem = referenceDataService.Insert(new Models.Referencelist()
                            {
                                name = source.type,
                                type = "TYPE",
                                isactive = true
                            });
                            reflist.Add(new ReferenceItem()
                            {
                                id = newitem.id,
                                name = newitem.name,
                                type = newitem.type
                            });
                        }
                        items.typeid = reflist.Find(e => e.name == source.type).id;
                    }

                    if (!string.IsNullOrEmpty(source.sleeve))
                    {
                        if (!reflist.Exists(e => e.name == source.sleeve))
                        {
                            var newitem = referenceDataService.Insert(new Models.Referencelist()
                            {
                                name = source.sleeve,
                                type = "SLEEVE",
                                isactive = true
                            });
                            reflist.Add(new ReferenceItem()
                            {
                                id = newitem.id,
                                name = newitem.name,
                                type = newitem.type
                            });
                        }
                        items.sleeveid = reflist.Find(e => e.name == source.sleeve).id;
                    }

                    if (!string.IsNullOrEmpty(source.fit))
                    {
                        if (!reflist.Exists(e => e.name == source.fit))
                        {

                            var newitem = referenceDataService.Insert(new Models.Referencelist()
                            {
                                name = source.fit,
                                type = "FIT",
                                isactive = true
                            });
                            reflist.Add(new ReferenceItem()
                            {
                                id = newitem.id,
                                name = newitem.name,
                                type = newitem.type
                            });
                        }
                        items.fitid = reflist.Find(e => e.name == source.fit).id;
                    }

                    if (!existing.Exists(e => e.productid == items.productid &&
                                             e.styleid == items.styleid &&
                                             e.typeid == items.typeid &&
                                             e.patternid == items.patternid &&
                                             e.brandid == items.brandid &&
                                             e.materialid == items.materialid &&
                                             e.fitid == items.fitid &&
                                             e.sleeveid == items.sleeveid &&
                                             e.sizeid == items.sizeid))
                    {
                        itemDataService.Insert(items);
                    }
                    else
                    {
                        var eitem = existing.Find(e => e.productid == items.productid &&
                                             e.styleid == items.styleid &&
                                             e.typeid == items.typeid &&
                                             e.patternid == items.patternid &&
                                             e.brandid == items.brandid &&
                                             e.materialid == items.materialid &&
                                             e.fitid == items.fitid &&
                                             e.sleeveid == items.sleeveid &&
                                             e.sizeid == items.sizeid);
                        if (eitem.code != items.code || eitem.gln != items.gln)
                        {
                            eitem.code = items.code;
                            eitem.gln = items.gln;
                            eitem.allowsearch = items.allowsearch;
                            eitem.support_gln = eitem.support_gln;
                            if (source.purchaserate > 0)
                                eitem.purchaserate = source.purchaserate;
                            if (source.mrprate > 0)
                                eitem.mrp = source.mrprate;
                            if (source.salerate > 0)
                                eitem.salerate = source.salerate;
                            itemDataService.Update(eitem, false);
                        }
                    }
                    data.progress.completedrows++;
                }
                catch (Exception exp)
                {
                    data.progress.failedrows++;
                    data.progress.faileddata.Add(source.source);
                }
            }
            data.progress.hascompleted = true;
        }

        public void MigrateBrand(MigrationData data, SortedDictionary<string, ColumnMap> columnmap, List<List<string>> values)
        {
            data.progress.hasstarted = true;

            InfyPOS.Data.BrandDataService brandDataService = new InfyPOS.Data.BrandDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            var existing = brandDataService.Search(new InfyPOS.Models.BrandCreteria() { });

            foreach (var source in values)
            {
                try
                {
                    var name = GetString(source, columnmap, "name");

                    if (existing.Exists(ex => ex.name == name))
                    {
                        InfyPOS.Models.Brand brandmodel = existing.Find(ex => ex.name == name);
                        brandmodel.isactive = true;
                        brandmodel.version = 1;
                        brandmodel.code = GetString(source, columnmap, "code");
                        brandmodel.name = GetString(source, columnmap, "name");

                        brandDataService.Update(brandmodel);
                        data.progress.updatedrows++;
                    }
                    else
                    {
                        InfyPOS.Models.Brand brandmodel = new InfyPOS.Models.Brand();
                        brandmodel.isactive = true;
                        brandmodel.version = 1;
                        brandmodel.code = GetString(source, columnmap, "code");
                        brandmodel.name = GetString(source, columnmap, "name");
                        brandDataService.Insert(brandmodel);
                        data.progress.createdrows++;
                    }
                    data.progress.completedrows++;
                }
                catch (Exception exp)
                {
                    data.progress.failedrows++;
                    source.Insert(0, exp.Message);
                    data.progress.faileddata.Add(source);
                }
            }
            data.progress.hascompleted = true;
        }
        public void MigrateAgent(MigrationData data, SortedDictionary<string, ColumnMap> columnmap, List<List<string>> values)
        {
            data.progress.hasstarted = true;


            InfyPOS.Data.AgentDataService customerDataService = new InfyPOS.Data.AgentDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };

            var existing = customerDataService.Search(new InfyPOS.Models.AgentCreteria() { });

            InfyPOS.Data.ReferencelistDataService refrdataservice = new InfyPOS.Data.ReferencelistDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };

            var citylist = refrdataservice.Search(new InfyPOS.Models.ReferencelistCreteria()
            {
                type = "CITY"
            });
            var statelist = refrdataservice.Search(new InfyPOS.Models.ReferencelistCreteria()
            {
                type = "STATE"
            });
            var countrylist = refrdataservice.Search(new InfyPOS.Models.ReferencelistCreteria()
            {
                type = "COUNTRY"
            });


            foreach (var source in values)
            {
                try
                {
                    string city = GetString(source, columnmap, "cityid");
                    long cityid = 0;
                    if (!citylist.Exists(e => e.name.ToLower() == city.ToLower()))
                    {
                        InfyPOS.Models.Referencelist mref = new InfyPOS.Models.Referencelist();
                        mref.code = city;
                        mref.name = city;
                        mref.isactive = true;
                        mref.type = "CITY";
                        refrdataservice.Insert(mref);
                        cityid = mref.id;
                        citylist.Add(mref);
                    }
                    else
                    {
                        cityid = citylist.First(e => e.name == city).id;
                    }

                    string state = GetString(source, columnmap, "stateid");
                    long stateid = 0;
                    if (!statelist.Exists(e => e.name.ToLower() == state.ToLower()))
                    {
                        InfyPOS.Models.Referencelist mref = new InfyPOS.Models.Referencelist();
                        mref.code = state;
                        mref.name = state;
                        mref.isactive = true;
                        mref.type = "STATE";
                        refrdataservice.Insert(mref);
                        stateid = mref.id;
                        statelist.Add(mref);
                    }
                    else
                    {
                        stateid = statelist.First(e => e.name.ToLower() == state.ToLower()).id;
                    }


                    var suppliername = GetString(source, columnmap, "name");

                    if (existing.Exists(ex => ex.name.ToLower() == suppliername.ToLower()))
                    {
                        var customermodel = existing.Find(ex => ex.name == suppliername);
                        customermodel.isactive = true;
                        customermodel.version = 1;
                        customermodel.name = suppliername;
                        customermodel.contactno = GetString(source, columnmap, "contactno");
                        customermodel.contactperson = GetString(source, columnmap, "contactperson");
                        customermodel.emailid = GetString(source, columnmap, "emailid");
                        customermodel.pincode = GetString(source, columnmap, "pincode"); ;
                        customermodel.gst = GetString(source, columnmap, "gst"); ;
                        customermodel.address = GetString(source, columnmap, "address"); ;
                        customermodel.cityid = cityid;
                        customermodel.stateid = stateid;
                        customerDataService.Update(customermodel);
                        data.progress.updatedrows++;
                    }
                    else
                    {
                        InfyPOS.Models.Agent customermodel = new InfyPOS.Models.Agent();
                        customermodel.isactive = true;
                        customermodel.version = 1;
                        customermodel.name = suppliername;
                        customermodel.contactno = GetString(source, columnmap, "contactno");
                        customermodel.contactperson = GetString(source, columnmap, "contactperson");
                        customermodel.emailid = GetString(source, columnmap, "emailid");
                        customermodel.pincode = GetString(source, columnmap, "pincode"); ;
                        customermodel.gst = GetString(source, columnmap, "gst"); ;
                        customermodel.address = GetString(source, columnmap, "address"); ;
                        customermodel.cityid = cityid;
                        customermodel.stateid = stateid;
                        customerDataService.Insert(customermodel);
                        data.progress.createdrows++;
                    }
                    data.progress.completedrows++;
                }
                catch (Exception exp)
                {
                    data.progress.failedrows++;
                    source.Insert(0, exp.Message);
                    data.progress.faileddata.Add(source);
                }
            }
            data.progress.hascompleted = true;
        }

        public void MigrateVendorAgreement(MigrationData data, SortedDictionary<string, ColumnMap> columnmap, List<List<string>> values)
        {
            data.progress.hasstarted = true;

            BrandDataService brandDataService = new BrandDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            var brands = brandDataService.Search(new BrandCreteria());

            InfyPOS.Data.ProductsDataService productDataService = new InfyPOS.Data.ProductsDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            var products = productDataService.Search(new ProductsCreteria());

            InfyPOS.Data.SupplierDataService supplierDataService = new InfyPOS.Data.SupplierDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            var suppliers = supplierDataService.Search(new SupplierCreteria());

            InfyPOS.Data.SupplierproductgroupsDataService supplierproductgroupdata = new InfyPOS.Data.SupplierproductgroupsDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            var supplierproducts = supplierproductgroupdata.Search(new SupplierproductgroupsCreteria());

            SuppliertradeagreementDataService suppliertradeagreementDataService = new SuppliertradeagreementDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };

            InfyPOS.Data.ReferencelistDataService refrdataservice = new InfyPOS.Data.ReferencelistDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            var reflist = refrdataservice.SearchList(new Models.ReferencelistCreteria()
            {
                types = new List<string>("STYLE,COLOUR,SIZE".Split(','))
            }).ConvertAll(x => new Models.Referencelist()
            {
                id = x.id,
                code = x.code,
                name = x.name,
                type = x.type,
                locationid = x.locationid
            });


            List<Suppliertradeagreement> lstmSuppliertradeagreement = new List<Suppliertradeagreement>();

            foreach (var source in values)
            {
                try
                {
                    Supplier msupplier = null;
                    Products mproduct = null;
                    Supplierproductgroups msupplierproductgroups = null;
                    Suppliertradeagreement mSuppliertradeagreement = null;
                    Brand mbrand = null;
                    string vendorcode = GetString(source, columnmap, "vendorcode");
                    string vendorname = GetString(source, columnmap, "vendorname");
                    string product = GetString(source, columnmap, "product");
                    string productcode = GetString(source, columnmap, "productcode");
                    string itemcode = GetString(source, columnmap, "itemcode");
                    string itemname = GetString(source, columnmap, "itemname");
                    string barcode = GetString(source, columnmap, "barcode");
                    string brand = GetString(source, columnmap, "brand");

                    string style = GetString(source, columnmap, "style");
                    string colour = GetString(source, columnmap, "colour");
                    string size = GetString(source, columnmap, "size");

                    decimal cost = GetDecimal(source, columnmap, "cost");
                    decimal price = GetDecimal(source, columnmap, "price");
                    decimal mrp = GetDecimal(source, columnmap, "mrp");

                    string vendorproductcode = productcode + vendorcode;

                    if (products.Exists(x => x.name.ToUpper() == product.ToUpper()))
                    {
                        mproduct = products.Find(x => x.name.ToUpper() == product.ToUpper());
                    }

                    if (suppliers.Exists(x => x.code == vendorcode))
                    {
                        msupplier = suppliers.Find(x => x.code == vendorcode);
                    }
                    else if (suppliers.Exists(x => x.name.ToUpper() == vendorname.ToUpper()))
                    {
                        msupplier = suppliers.Find(x => x.name.ToUpper() == vendorname.ToUpper());
                    }

                    if (msupplier == null)
                    {
                        msupplier = new Supplier()
                        {
                            code = vendorcode,
                            name = vendorname,
                            isactive = true,
                            companylist = new List<Suppliercompany>()
                        };
                        msupplier.companylist.Add(new Suppliercompany()
                        {
                            code = vendorcode,
                            name = vendorname,
                            isactive = true,
                        });
                        suppliers.Add(supplierDataService.Insert(msupplier));
                    }

                    if (mproduct == null)
                    {
                        mproduct = new Products()
                        {
                            name = product,
                            code = productcode,
                            isactive = true
                        };
                        productDataService.isMigration = true;
                        products.Add(productDataService.Insert(mproduct));
                    }

                    if (supplierproducts.Exists(x => x.productgroupid == mproduct.id && x.supplierid == msupplier.id))
                    {
                        msupplierproductgroups = supplierproducts.Find(x => x.productgroupid == mproduct.id && x.supplierid == msupplier.id);
                    }
                    else
                    {
                        msupplierproductgroups = new Supplierproductgroups()
                        {
                            productgroupid = mproduct.id,
                            supplierid = msupplier.id,
                            isactive = true,
                            itemcode = vendorproductcode,
                            attributelist = new List<Supplierproductgroups.Attributelist>()
                        };
                        supplierproducts.Add(supplierproductgroupdata.Insert(msupplierproductgroups));
                    }

                    if (msupplierproductgroups.attributelist == null)
                        msupplierproductgroups.attributelist = new List<Supplierproductgroups.Attributelist>();

                    long sizeid = 0;
                    long styleid = 0;
                    long colourid = 0;
                    if (!string.IsNullOrEmpty(size))
                    {
                        if (!reflist.Exists(x => x.@type == "SIZE" && x.name.ToUpper() == size.ToUpper()))
                        {
                            reflist.Add(refrdataservice.Insert(new Referencelist()
                            {
                                @type = "SIZE",
                                name = size,
                                isactive = true
                            }));
                        }
                        sizeid = reflist.Find(x => x.@type == "SIZE" && x.name.ToUpper() == size.ToUpper()).id;
                    }

                    if (!string.IsNullOrEmpty(size))
                    {
                        if (!reflist.Exists(x => x.@type == "COLOUR" && x.name.ToUpper() == colour.ToUpper()))
                        {
                            reflist.Add(refrdataservice.Insert(new Referencelist()
                            {
                                @type = "COLOUR",
                                name = colour,
                                isactive = true
                            }));
                        }
                        colourid = reflist.Find(x => x.@type == "COLOUR" && x.name.ToUpper() == colour.ToUpper()).id;
                    }


                    if (!string.IsNullOrEmpty(style))
                    {
                        if (!reflist.Exists(x => x.@type == "STYLE" && x.name.ToUpper() == style.ToUpper()))
                        {
                            reflist.Add(refrdataservice.Insert(new Referencelist()
                            {
                                @type = "STYLE",
                                name = style,
                                isactive = true
                            }));
                        }
                        styleid = reflist.Find(x => x.@type == "STYLE" && x.name.ToUpper() == style.ToUpper()).id;
                    }

                    if (brands.Exists(x => x.name.ToUpper() == brand.ToUpper()))
                    {
                        mbrand = brands.Find(x => x.name.ToUpper() == brand.ToUpper());
                    }
                    else if (!string.IsNullOrEmpty(brand))
                    {
                        mbrand = new Brand()
                        {
                            name = brand,
                            isactive = true
                        };
                        brands.Add(brandDataService.Insert(mbrand));
                    }

                    bool updatesupplierproductgroups = false;
                    if (sizeid > 0 && (!msupplierproductgroups.attributelist.Exists(x => x.attributetype == "SIZE" && x.attributeid == sizeid)))
                    {
                        msupplierproductgroups.attributelist.Add(new Supplierproductgroups.Attributelist()
                        {
                            attributeid = sizeid,
                            attributetype = "SIZE",
                            attributename = size
                        });
                        updatesupplierproductgroups = true;
                    }
                    if (colourid > 0 && (!msupplierproductgroups.attributelist.Exists(x => x.attributetype == "COLOUR" && x.attributeid == colourid)))
                    {
                        msupplierproductgroups.attributelist.Add(new Supplierproductgroups.Attributelist()
                        {
                            attributeid = colourid,
                            attributetype = "COLOUR",
                            attributename = colour
                        });
                        updatesupplierproductgroups = true;
                    }
                    if (styleid > 0 && (!msupplierproductgroups.attributelist.Exists(x => x.attributetype == "STYLE" && x.attributeid == styleid)))
                    {
                        msupplierproductgroups.attributelist.Add(new Supplierproductgroups.Attributelist()
                        {
                            attributeid = styleid,
                            attributetype = "STYLE",
                            attributename = style
                        });
                        updatesupplierproductgroups = true;
                    }
                    if (mbrand != null && (!msupplierproductgroups.attributelist.Exists(x => x.attributetype == "BRAND" && x.attributeid == mbrand.id)))
                    {
                        msupplierproductgroups.attributelist.Add(new Supplierproductgroups.Attributelist()
                        {
                            attributeid = mbrand.id,
                            attributetype = "BRAND",
                            attributename = brand
                        });
                        updatesupplierproductgroups = true;
                    }
                    if (updatesupplierproductgroups)
                        supplierproductgroupdata.Update(msupplierproductgroups);

                    if (lstmSuppliertradeagreement.Exists(x => x.supplierid == msupplier.id &&
                    x.productgroupid == mproduct.id && x.itemcode == itemcode &&
                    x.Supplierproductgroupsid == msupplierproductgroups.id))
                    {
                        mSuppliertradeagreement = lstmSuppliertradeagreement.Find(x => x.supplierid == msupplier.id &&
                            x.productgroupid == mproduct.id && x.itemcode == itemcode &&
                            x.Supplierproductgroupsid == msupplierproductgroups.id);
                    }
                    else
                    {
                        var existingagreements = suppliertradeagreementDataService.Search(new SuppliertradeagreementCreteria()
                        {
                            supplierid = msupplier.id,
                            productgroupid = mproduct.id,
                            itemcode = itemcode,
                            Supplierproductgroupsid = msupplierproductgroups.id
                        });

                        if (existingagreements.Count == 0)
                        {
                            mSuppliertradeagreement = new Suppliertradeagreement()
                            {
                                supplierid = msupplier.id,
                                productgroupid = mproduct.id,
                                Supplierproductgroupsid = msupplierproductgroups.id,
                                isactive = true,
                                items = new List<Suppliertradeagreement.MatrixItem>(),
                                matrixlist = new List<Suppliertradeagreement.matrixconfig>(),
                                agreementname = itemname,
                                itemcode = itemcode
                            };
                            suppliertradeagreementDataService.IsMigration = true;
                            lstmSuppliertradeagreement.Add(suppliertradeagreementDataService.Insert(mSuppliertradeagreement));
                        }
                        else
                        {
                            mSuppliertradeagreement = existingagreements[0];
                        }
                    }

                    if (sizeid > 0)
                    {
                        if (!mSuppliertradeagreement.matrixlist.Exists(x => x.attributetype == "SIZE"))
                        {
                            mSuppliertradeagreement.matrixlist.Add(new Suppliertradeagreement.matrixconfig()
                            {
                                attributetype = "SIZE",
                                items = new List<Suppliertradeagreement.matrixconfig.attributeid>()
                            });
                        }
                        if (!mSuppliertradeagreement.matrixlist.Find(x => x.attributetype == "SIZE").items.Exists(y => y.id == sizeid))
                        {
                            mSuppliertradeagreement.matrixlist.Find(x => x.attributetype == "SIZE").items.Add(new Suppliertradeagreement.matrixconfig.attributeid()
                            {
                                id = sizeid
                            });
                        }
                    }

                    if (colourid > 0)
                    {
                        if (!mSuppliertradeagreement.matrixlist.Exists(x => x.attributetype == "COLOUR"))
                        {
                            mSuppliertradeagreement.matrixlist.Add(new Suppliertradeagreement.matrixconfig()
                            {
                                attributetype = "COLOUR",
                                items = new List<Suppliertradeagreement.matrixconfig.attributeid>()
                            });
                        }
                        if (!mSuppliertradeagreement.matrixlist.Find(x => x.attributetype == "COLOUR").items.Exists(y => y.id == colourid))
                        {
                            mSuppliertradeagreement.matrixlist.Find(x => x.attributetype == "COLOUR").items.Add(new Suppliertradeagreement.matrixconfig.attributeid()
                            {
                                id = colourid
                            });
                        }
                    }

                    if (styleid > 0)
                    {
                        if (!mSuppliertradeagreement.matrixlist.Exists(x => x.attributetype == "STYLE"))
                        {
                            mSuppliertradeagreement.matrixlist.Add(new Suppliertradeagreement.matrixconfig()
                            {
                                attributetype = "STYLE",
                                items = new List<Suppliertradeagreement.matrixconfig.attributeid>()
                            });
                        }
                        if (!mSuppliertradeagreement.matrixlist.Find(x => x.attributetype == "STYLE").items.Exists(y => y.id == styleid))
                        {
                            mSuppliertradeagreement.matrixlist.Find(x => x.attributetype == "STYLE").items.Add(new Suppliertradeagreement.matrixconfig.attributeid()
                            {
                                id = styleid
                            });
                        }
                    }

                    if (!mSuppliertradeagreement.items.Exists(x => x.gln == barcode))
                    {
                        mSuppliertradeagreement.items.Add(new Suppliertradeagreement.MatrixItem()
                        {
                            gln = barcode,
                            cost = cost,
                            price = price,
                            mrp = mrp,
                            printingname = itemname,
                            styleid = styleid,
                            sizeid = sizeid,
                            colourid = colourid
                        });
                        data.progress.createdrows++;
                        if (!lstmSuppliertradeagreement.Exists(x => x.id == mSuppliertradeagreement.id))
                            lstmSuppliertradeagreement.Add(mSuppliertradeagreement);
                    }
                    else
                    {
                        var agritem = mSuppliertradeagreement.items.Find(x => x.gln == barcode);
                        agritem.printingname = itemname;
                        agritem.cost = cost;
                        agritem.price = price;
                        agritem.mrp = mrp;
                    }
                    data.progress.completedrows++;
                }
                catch (Exception exp)
                {
                    data.progress.failedrows++;
                    source.Insert(0, exp.Message);
                    data.progress.faileddata.Add(source);
                }
            }
            foreach (var item in lstmSuppliertradeagreement)
            {
                suppliertradeagreementDataService.Update(item, true);
            }
            data.progress.hascompleted = true;
        }

        public void MigrateTransport(MigrationData data, SortedDictionary<string, ColumnMap> columnmap, List<List<string>> values)
        {
            data.progress.hasstarted = true;

            InfyPOS.Data.TransportDataService transportDataService = new InfyPOS.Data.TransportDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };

            var existing = transportDataService.Search(new InfyPOS.Models.TransportCreteria() { });

            InfyPOS.Data.ReferencelistDataService refrdataservice = new InfyPOS.Data.ReferencelistDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };

            var citylist = refrdataservice.Search(new InfyPOS.Models.ReferencelistCreteria()
            {
                type = "CITY"
            });
            var statelist = refrdataservice.Search(new InfyPOS.Models.ReferencelistCreteria()
            {
                type = "STATE"
            });
            var countrylist = refrdataservice.Search(new InfyPOS.Models.ReferencelistCreteria()
            {
                type = "COUNTRY"
            });


            foreach (var source in values)
            {
                try
                {
                    string city = GetString(source, columnmap, "cityid");
                    long cityid = 0;
                    if (!citylist.Exists(e => e.name.ToLower() == city.ToLower()))
                    {
                        InfyPOS.Models.Referencelist mref = new InfyPOS.Models.Referencelist();
                        mref.code = city;
                        mref.name = city;
                        mref.isactive = true;
                        mref.type = "CITY";
                        refrdataservice.Insert(mref);
                        cityid = mref.id;
                        citylist.Add(mref);
                    }
                    else
                    {
                        cityid = citylist.First(e => e.name == city).id;
                    }

                    string state = GetString(source, columnmap, "stateid");
                    long stateid = 0;
                    if (!statelist.Exists(e => e.name.ToLower() == state.ToLower()))
                    {
                        InfyPOS.Models.Referencelist mref = new InfyPOS.Models.Referencelist();
                        mref.code = state;
                        mref.name = state;
                        mref.isactive = true;
                        mref.type = "STATE";
                        refrdataservice.Insert(mref);
                        stateid = mref.id;
                        statelist.Add(mref);
                    }
                    else
                    {
                        stateid = statelist.First(e => e.name.ToLower() == state.ToLower()).id;
                    }


                    var transportname = GetString(source, columnmap, "name");

                    if (existing.Exists(ex => ex.name.ToLower() == transportname.ToLower()))
                    {
                        var transportmodel = existing.Find(ex => ex.name == transportname);
                        transportmodel.isactive = true;
                        transportmodel.version = 1;
                        transportmodel.name = transportname;
                        if (!string.IsNullOrEmpty(GetString(source, columnmap, "contactno")))
                            transportmodel.contactno = GetString(source, columnmap, "contactno");
                        if (!string.IsNullOrEmpty(GetString(source, columnmap, "contactperson")))
                            transportmodel.contactperson = GetString(source, columnmap, "contactperson");

                        if (!string.IsNullOrEmpty(GetString(source, columnmap, "emailid")))
                            transportmodel.emailid = GetString(source, columnmap, "emailid");
                        if (!string.IsNullOrEmpty(GetString(source, columnmap, "pincode")))
                            transportmodel.pincode = GetString(source, columnmap, "pincode");
                        if (!string.IsNullOrEmpty(GetString(source, columnmap, "gst")))
                            transportmodel.gst = GetString(source, columnmap, "gst");
                        if (!string.IsNullOrEmpty(GetString(source, columnmap, "address")))
                            transportmodel.address = FormatAddress(GetString(source, columnmap, "address"));

                        if (cityid > 0)
                            transportmodel.cityid = cityid;
                        if (stateid > 0)
                            transportmodel.stateid = stateid;
                        transportDataService.Update(transportmodel);
                        data.progress.updatedrows++;
                    }
                    else
                    {
                        InfyPOS.Models.Transport transportmodel = new InfyPOS.Models.Transport();
                        transportmodel.isactive = true;
                        transportmodel.version = 1;
                        transportmodel.name = transportname;
                        transportmodel.contactno = GetString(source, columnmap, "contactno");
                        transportmodel.contactperson = GetString(source, columnmap, "contactperson");
                        transportmodel.emailid = GetString(source, columnmap, "emailid");
                        transportmodel.pincode = GetString(source, columnmap, "pincode");
                        transportmodel.gst = GetString(source, columnmap, "gst");
                        transportmodel.address = FormatAddress(GetString(source, columnmap, "address"));
                        transportmodel.cityid = cityid;
                        transportmodel.stateid = stateid;
                        transportDataService.Insert(transportmodel);
                        data.progress.createdrows++;
                    }
                    data.progress.completedrows++;
                }
                catch (Exception exp)
                {
                    data.progress.failedrows++;
                    source.Insert(0, exp.Message);
                    data.progress.faileddata.Add(source);
                }
            }
            data.progress.hascompleted = true;
        }

        public void MigrateCustomerCreditNoteVoucher(MigrationData data, SortedDictionary<string, ColumnMap> columnmap, List<List<string>> values)
        {
            data.progress.hasstarted = true;
            InfyPOS.Data.GiftCouponDataService CouponData = new InfyPOS.Data.GiftCouponDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            InfyPOS.Data.CustomerDataService customerDataService = new InfyPOS.Data.CustomerDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            var existing = customerDataService.Search(new InfyPOS.Models.CustomerCreteria() { limit = -9 });
            foreach (var source in values)
            {
                try
                {
                    string code = GetString(source, columnmap, "code");
                    DateTime validfrom = GetDate(source, columnmap, "validfrom");
                    DateTime validto = GetDate(source, columnmap, "validto");
                    string customername = GetString(source, columnmap, "customername");
                    string mobile = GetString(source, columnmap, "customermobileno");
                    decimal voucheramount = GetDecimal(source, columnmap, "voucheramount");

                    Customer _customer = new Customer();
                    if (existing.Exists(ex => ex.mobileno.ToLower() == mobile.ToLower()))
                        _customer = existing.Find(ex => ex.mobileno.ToLower() == mobile.ToLower());

                    GiftVoucher gtvoucher = new GiftVoucher()
                    {
                        isactive = true,
                        version = 1,
                        validfrom = validfrom,
                        validto = validto,
                        customerid = _customer.id,
                        counterid = 0,
                        name = "CREDIT NOTE",
                        issuemode = 4,
                        mobileno = string.IsNullOrEmpty(_customer.mobileno) ? "0000000000" : _customer.mobileno,
                        receivedamt = voucheramount,
                        vouchervalue = voucheramount,
                        issuedby = 0,
                        vouchercode = code,
                        denomination = new List<GiftVoucher.Denomination>()
                    };

                    CouponData.InsertMigrationData(gtvoucher);

                    data.progress.completedrows++;
                }
                catch (Exception exp)
                {
                    data.progress.failedrows++;
                    source.Insert(0, exp.Message);
                    data.progress.faileddata.Add(source);
                }
            }
            data.progress.hascompleted = true;
        }

        public void MigrateEmployees(MigrationData data, SortedDictionary<string, ColumnMap> columnmap, List<List<string>> values)
        {
            data.progress.hasstarted = true;

            InfyPOS.Data.EmployeeDataService employeeDataService = new InfyPOS.Data.EmployeeDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };

            InfyPOS.Data.CompanyDataService companydata = new InfyPOS.Data.CompanyDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            companydata.Context = new InfyPOS.Common.UserClaim();
            var compList = companydata.Search(new InfyPOS.Models.CompanyCreteria());

            InfyPOS.Data.ReferencelistDataService referencelistDataService = new InfyPOS.Data.ReferencelistDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            referencelistDataService.isMigration = true;
            var locations = referencelistDataService.Search(new InfyPOS.Models.ReferencelistCreteria()
            {
                type = "LOCATION"
            });
            var floors = referencelistDataService.Search(new InfyPOS.Models.ReferencelistCreteria()
            {
                type = "FLOOR"
            });
            var departments = referencelistDataService.Search(new InfyPOS.Models.ReferencelistCreteria()
            {
                type = "DEPARTMENT"
            });
            var designation = referencelistDataService.Search(new InfyPOS.Models.ReferencelistCreteria()
            {
                type = "DESIGNATION"
            });
            var section = referencelistDataService.Search(new InfyPOS.Models.ReferencelistCreteria()
            {
                type = "DIVISION"
            });


            var existing = employeeDataService.Search(new InfyPOS.Models.EmployeeCreteria() { });
            foreach (var source in values)
            {
                try
                {
                    InfyPOS.Models.Employee employee = new InfyPOS.Models.Employee();
                    employee.employeecode = GetString(source, columnmap, "employeecode");
                    employee.name = GetString(source, columnmap, "name");

                    if (existing.Exists(e => e.name.ToLower() == employee.name.ToLower() && e.employeecode.ToLower() == employee.employeecode.ToLower()))
                    {
                        employee = existing.Find(e => e.name.ToLower() == employee.name.ToLower() && e.employeecode.ToLower() == employee.employeecode.ToLower());
                    }
                    string gender = GetString(source, columnmap, "gender").ToUpper();
                    if (!string.IsNullOrEmpty(gender))
                    {
                        if (gender == "M" || gender == "MALE" || gender == "1")
                            employee.gender = "M";
                        else
                            employee.gender = "F";
                    }
                    employee.aadharno = GetString(source, columnmap, "aadharno");
                    employee.pancardno = GetString(source, columnmap, "pancardno");
                    employee.dateofbirth = GetDate(source, columnmap, "dateofbirth");
                    employee.dateofjoining = GetDate(source, columnmap, "dateofjoining");
                    string locationcode = GetString(source, columnmap, "locationcode");
                    string companycode = GetString(source, columnmap, "companycode");

                    employee.presentaddress = new InfyPOS.Models.Employee.Presentaddress();
                    var addresslist = GetString(source, columnmap, "address").Split(',');

                    if (!string.IsNullOrEmpty(GetString(source, columnmap, "residentno")))
                        employee.presentaddress.residentno = GetString(source, columnmap, "residentno");
                    else
                        employee.presentaddress.residentno = addresslist.Length < 1 ? "" : addresslist[0];

                    if (!string.IsNullOrEmpty(GetString(source, columnmap, "street")))
                        employee.presentaddress.residentno = GetString(source, columnmap, "street");
                    else
                        employee.presentaddress.street = addresslist.Length < 2 ? "" : addresslist[1];
                    if (!string.IsNullOrEmpty(GetString(source, columnmap, "area")))
                        employee.presentaddress.residentno = GetString(source, columnmap, "area");
                    else
                        employee.presentaddress.area = addresslist.Length < 3 ? "" : addresslist[2];
                    if (!string.IsNullOrEmpty(GetString(source, columnmap, "city")))
                        employee.presentaddress.residentno = GetString(source, columnmap, "city");
                    else
                        employee.presentaddress.city = addresslist.Length < 4 ? "" : addresslist[3];

                    employee.presentaddress.pincode = GetString(source, columnmap, "pincode");
                    employee.contactno = GetString(source, columnmap, "contactno");
                    employee.locationid = locations.Exists(e => e.code == locationcode) ? locations.Find(e => e.code == locationcode).id : 0;
                    employee.companyid = compList.Exists(e => e.code == companycode) ? compList.Find(e => e.code == companycode).id : 0;

                    if (!floors.Exists(e => e.name == GetString(source, columnmap, "floor")))
                    {
                        InfyPOS.Models.Referencelist referenceItem = new InfyPOS.Models.Referencelist();
                        referenceItem.name = GetString(source, columnmap, "floor");
                        referenceItem.code = GetString(source, columnmap, "floor");
                        referenceItem.type = "FLOOR";
                        referenceItem.isactive = true;
                        referencelistDataService.Insert(referenceItem);
                        floors.Add(referenceItem);
                    }
                    employee.floorid = floors.Find(e => e.name == GetString(source, columnmap, "floor")).id;
                    if (!departments.Exists(e => e.name == GetString(source, columnmap, "department")))
                    {
                        InfyPOS.Models.Referencelist referenceItem = new InfyPOS.Models.Referencelist();
                        referenceItem.name = GetString(source, columnmap, "department");
                        referenceItem.code = GetString(source, columnmap, "department");
                        referenceItem.type = "DEPARTMENT";
                        referenceItem.isactive = true;
                        referencelistDataService.Insert(referenceItem);
                        departments.Add(referenceItem);
                    }
                    employee.departmentid = departments.Find(e => e.name == GetString(source, columnmap, "department")).id;


                    if (!designation.Exists(e => e.name == GetString(source, columnmap, "designation")))
                    {
                        InfyPOS.Models.Referencelist referenceItem = new InfyPOS.Models.Referencelist();
                        referenceItem.name = GetString(source, columnmap, "designation");
                        referenceItem.code = GetString(source, columnmap, "designation");
                        referenceItem.type = "DESIGNATION";
                        referenceItem.isactive = true;
                        referencelistDataService.Insert(referenceItem);
                        designation.Add(referenceItem);
                    }
                    employee.desingnationid = designation.Find(e => e.name == GetString(source, columnmap, "designation")).id;

                    if (!section.Exists(e => e.name == GetString(source, columnmap, "section")))
                    {
                        InfyPOS.Models.Referencelist referenceItem = new InfyPOS.Models.Referencelist();
                        referenceItem.name = GetString(source, columnmap, "section");
                        referenceItem.code = GetString(source, columnmap, "section");
                        referenceItem.type = "DIVISION";
                        referenceItem.isactive = true;
                        referencelistDataService.Insert(referenceItem);
                        section.Add(referenceItem);
                    }
                    employee.sectionid = section.Find(e => e.name == GetString(source, columnmap, "section")).id;

                    employee.pfandesi = new InfyPOS.Models.Employee.Pfandesi();
                    employee.pfandesi.pfno = GetString(source, columnmap, "pfno");
                    employee.pfandesi.pfuan = GetString(source, columnmap, "pfuan");
                    employee.pfandesi.esino = GetString(source, columnmap, "esino");
                    employee.pfandesi.pffrom = DateTime.Now;
                    employee.pfandesi.esienabledon = DateTime.Now;

                    if (employee.id > 0)
                    {
                        employee.isactive = true;
                        employee.version = employee.version;
                        employee.modifiedby = 1;
                        employee.modifiedon = DateTime.Now;
                        employeeDataService.Update(employee);
                        data.progress.updatedrows++;
                    }
                    else
                    {
                        employee.isactive = true;
                        employee.version = employee.version + 1;
                        employee.createdby = 1;
                        employee.createdon = DateTime.Now;
                        employee.modifiedby = 1;
                        employee.modifiedon = DateTime.Now;
                        employeeDataService.Insert(employee);
                        data.progress.createdrows++;
                    }
                    data.progress.completedrows++;
                }
                catch (Exception exp)
                {
                    data.progress.failedrows++;
                    source.Insert(0, exp.Message);
                    data.progress.faileddata.Add(source);
                }
            }
            data.progress.hascompleted = true;
        }
    }
}