using InfyPOS.Common;
using InfyPOS.Models;
using InfyPOS.Processors.Migration;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Web;

namespace InfyPOS.Processors
{
    public class BillImportManager
    {
        long organizationid = 0;
        public BillImportManager(long organizationid)
        {
            this.organizationid = organizationid;
        }
        public class BillStatus : Models.Lrinvoice
        {
            public int missing_design { get; set; }
            public int missing_uan { get; set; }
            public decimal salevalue { get; set; }
            public decimal salediscount { get; set; }
            public int missing_attribute
            {
                get
                {
                    return missing_brand +
                        missing_colour +
                        missing_design +
                        missing_material +
                        missing_pattern +
                        missing_product +
                        missing_productcategory +
                        missing_section +
                        missing_size +
                        missing_style +
                        missing_type;
                }
            }
            public int missing_product { get; set; }
            public int missing_section { get; set; }
            public int missing_productcategory { get; set; }
            public int missing_brand { get; set; }
            public int missing_size { get; set; }
            public int missing_pattern { get; set; }
            public int missing_material { get; set; }
            public int missing_style { get; set; }
            public int missing_colour { get; set; }
            public int missing_fit { get; set; }
            public int missing_sleeve { get; set; }
            public int missing_type { get; set; }
            public bool selected { get; set; }
            public string error { get; set; }
            public bool success { get; set; }
            public string missing_uanlist { get;  set; }
            public string brandlist { get;  set; }
            public object noofrows { get;  set; }
        }

        public class BillSummary : InfyPOS.Common.BaseEntity
        {
            public long lrentryid { get; set; }
            public MigrationData MigrationData { get; set; }
            public bool autogeneratebarcode { get; set; }
            public bool supportuanno { get; set; }
            public bool interstate { get; set; }
            public decimal marginpercentage { get; set; }
            public decimal discountpercentage { get; set; }
            public List<BillStatus> Items { get; set; }
            public string filename { get; set; }
            public long fileid { get; set; }
            public string[] columns { get; set; }
            public bool haserror { get; set; }
            public string errormessages { get; set; }
            public int importcount { get; set; }
            [Newtonsoft.Json.JsonIgnore]
            public List<LrinvoiceDetail> invoices { get; set; }
        }
        public BillSummary Validate(HttpRequestMessage Request, long type, Common.ImageData data)
        {
            BillSummary result = new BillSummary() { Items = new List<BillStatus>() };
            try
            {
                List<string> lst = new List<string>();
                if (!Request.Content.IsMimeMultipartContent())
                {
                    return null;
                }

                System.Threading.AutoResetEvent are = new System.Threading.AutoResetEvent(false);
                try
                {
                    var columnnames = "Doc.Num,Bill Date,Material code,LR Date,Reduction,Place of s,Supplier,Company,Location,Customer GSTIN,LR No,Qty,MRP Price,Cost,Tax Percentage,Tax Per Item,MRP Price,Sales Discount,Final,Inter Arti,HSN,Section,Product Category,Product,Brand Desc,Grid,Gender,Fashion,Season,Collection,Colour Des,Material description".Split(',');
                    if (type > 0)
                    {
                        InfyPOS.Data.ReferencelistDataService referencelistDataService = new Data.ReferencelistDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } }; ;
                        var columnConfig = referencelistDataService.Search(new Models.ReferencelistCreteria()
                        {
                            id = type
                        });
                        if (columnConfig.Count > 0 && columnConfig[0].additionalinfo != null &&
                            !string.IsNullOrEmpty(columnConfig[0].additionalinfo.formula) &&
                            columnConfig[0].additionalinfo.formula.Length > 50)
                        {
                            columnnames = columnConfig[0].additionalinfo.formula.Split(',');
                        }
                    }
                    else if (type == -1)
                    {
                        columnnames = @"Bill No,Bill Date,designid,Bill Date,Total disount,location,Supplier Company,Supplier Company,location,Supplier GST NO,,qty,MRP PRICE,RATE,Tax Percentage,Tax Per Item,price,,,Article ID,HSN,,,Product,Brand,Size,Style,Pattern,Item Type,Material,Colour,Detail".Split(',');
                    }

                    result.columns = columnnames;

                    Request.Content.ReadAsMultipartAsync<MultipartMemoryStreamProvider>(new MultipartMemoryStreamProvider()).ContinueWith((task) =>
                    {
                        MultipartMemoryStreamProvider provider = task.Result;

                        try
                        {
                            foreach (HttpContent content in provider.Contents)
                            {
                                Stream stream = content.ReadAsStreamAsync().Result;
                                Byte[] buffer = new Byte[stream.Length];
                                stream.Read(buffer, 0, buffer.Length);

                                if (content.Headers.ContentDisposition.FileName == null)
                                    continue;


                                var list = Process(stream, columnnames);

                                foreach (var item in list.GroupBy(e => new { e.BillNo, e.SupplierName }))
                                {

                                    var uanlist = item.ToList().FindAll(x => !string.IsNullOrEmpty(x.UAN)).ConvertAll(x => x.UAN);
                                    if (uanlist.Count > 0)
                                    {
                                        string query = @"select gln from items where gln in (" + string.Join(",", uanlist.ConvertAll(x => "'" + x + "'")) + ") and isactive=true";
                                        using (var reader = data.ExecuteReader(query, new SortedList<string, object>()))
                                        {
                                            while (reader.Read())
                                            {
                                                uanlist.Remove(reader.GetString(0));
                                            }
                                        }
                                    }
                                    result.Items.Add(new BillStatus()
                                    {
                                        suppliercompany = item.First().SupplierName,
                                        billvalue = item.Sum(e => e.BillAmount),
                                        taxamount = item.Sum(e => e.TaxAmount),
                                        baseamount = item.Sum(e => e.BillAmount - e.TaxAmount),
                                        invoicedate = item.First().BillDate,
                                        invoiceno = item.First().BillNo,
                                        noofpieces = item.Sum(e => e.Qty),
                                        discount = item.Sum(e => e.PurchaseDiscount),
                                        salevalue = item.Sum(e => e.FinalPrice * e.Qty),
                                        salediscount = item.Sum(e => (e.MRP - e.FinalPrice) * e.Qty),
                                        missing_product = item.Count(e => string.IsNullOrEmpty(e.Product)),
                                        missing_section = item.Count(e => string.IsNullOrEmpty(e.Section)),
                                        missing_productcategory = item.Count(e => string.IsNullOrEmpty(e.ProductCategory)),
                                        missing_brand = item.Count(e => string.IsNullOrEmpty(e.Brand)),
                                        missing_size = item.Count(e => string.IsNullOrEmpty(e.Size)),
                                        missing_pattern = item.Count(e => string.IsNullOrEmpty(e.Pattern)),
                                        missing_material = item.Count(e => string.IsNullOrEmpty(e.Material)),
                                        missing_style = item.Count(e => string.IsNullOrEmpty(e.Style)),
                                        missing_colour = item.Count(e => string.IsNullOrEmpty(e.Colour)),
                                        missing_fit = item.Count(e => string.IsNullOrEmpty(e.Fit)),
                                        missing_sleeve = item.Count(e => string.IsNullOrEmpty(e.Sleeve)),
                                        missing_type = item.Count(e => string.IsNullOrEmpty(e.Type)),
                                        missing_design = item.Count(e => string.IsNullOrEmpty(e.DesignID)),
                                        missing_uan = uanlist.Count,
                                        noofrows = item.Count(),
                                        brandlist = string.Join(",", item.ToList().ConvertAll(x => x.Brand).Distinct().ToList()),
                                        missing_uanlist = String.Join(",", uanlist)
                                    });

                                }


                                string filename = content.Headers.ContentDisposition.FileName.Replace("\"", "").ToLower();
                                FileInfo fi = new FileInfo(content.Headers.ContentDisposition.FileName.Replace("\"", ""));
                                Common.ImageData.File file = new Common.ImageData.File();
                                file.createdby = data.Context.UserID;
                                file.createdon = DateTime.Now;
                                file.data = buffer;
                                file.ext = fi.Extension;
                                file.filename = fi.Name;
                                file.isactive = true;
                                file.modifiedby = data.Context.UserID;
                                file.modifiedon = DateTime.Now;
                                file.version = 1;
                                file.type = "billimp";
                                file = data.Insert(file);

                                result.filename = file.filename;
                                result.fileid = file.id;
                                break;
                            }
                            are.Set();
                        }
                        catch (Exception exp)
                        {
                            WebAPI.Common.Logger.Instance(data.Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);
                            are.Set();
                        }
                    });
                    are.WaitOne();
                }
                catch (Exception exp)
                {
                    WebAPI.Common.Logger.Instance(data.Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);
                    are.Set();
                }
                return result;
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(data.Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);
            }
            return result;
        }

        internal BillSummary PrepareLRData(MigrationData data, ImageData imageData)
        {
            var file = imageData.GetData(data.uploadfileid.LongFromBase62());
            BillSummary result = null;
            using (System.IO.MemoryStream memoryStream = new MemoryStream(file.data))
            {
                var list = Process(memoryStream, data.columns.ConvertAll(e => e.excelcolunmn).ToArray());
                result = new BillSummary() { Items = new List<BillStatus>(), MigrationData = data };
                foreach (var item in list.GroupBy(e => e.BillNo))
                {
                    var uanlist = item.ToList().FindAll(x => !string.IsNullOrEmpty(x.UAN)).ConvertAll(x => x.UAN);
                    if (uanlist.Count > 0)
                    {
                        string query = @"select gln from items where gln in (" + string.Join(",", uanlist.ConvertAll(x => "'" + x + "'")) + ") and isactive=true";
                        using (var reader = imageData.ExecuteReader(query, new SortedList<string, object>()))
                        {
                            while (reader.Read())
                            {
                                uanlist.Remove(reader.GetString(0));
                            }
                        }
                    }
                    result.Items.Add(new BillStatus()
                    {
                        suppliercompany = item.First().SupplierName,
                        billvalue = item.Sum(e => e.BillAmount-e.PurchaseDiscount),
                        taxamount = item.Sum(e => e.TaxAmount),
                        baseamount = item.Sum(e => e.BillAmount - e.TaxAmount),
                        invoicedate = item.First().BillDate,
                        invoiceno = item.First().BillNo,
                        noofpieces = (int)item.Sum(e => e.Qty),
                        discount = item.Sum(e => e.PurchaseDiscount),
                        salevalue = item.Sum(e => e.FinalPrice * e.Qty),
                        salediscount = item.Sum(e => (e.MRP - e.FinalPrice) * e.Qty),
                        missing_product = item.Count(e => string.IsNullOrEmpty(e.Product)),
                        missing_section = item.Count(e => string.IsNullOrEmpty(e.Section)),
                        missing_productcategory = item.Count(e => string.IsNullOrEmpty(e.ProductCategory)),
                        missing_brand = item.Count(e => string.IsNullOrEmpty(e.Brand)),
                        missing_size = item.Count(e => string.IsNullOrEmpty(e.Size)),
                        missing_pattern = item.Count(e => string.IsNullOrEmpty(e.Pattern)),
                        missing_material = item.Count(e => string.IsNullOrEmpty(e.Material)),
                        missing_style = item.Count(e => string.IsNullOrEmpty(e.Style)),
                        missing_colour = item.Count(e => string.IsNullOrEmpty(e.Colour)),
                        missing_type = item.Count(e => string.IsNullOrEmpty(e.Type)),
                        missing_fit = item.Count(e => string.IsNullOrEmpty(e.Fit)),
                        missing_sleeve = item.Count(e => string.IsNullOrEmpty(e.Sleeve)),
                        missing_design = item.Count(e => string.IsNullOrEmpty(e.DesignID)),
                        missing_uan = uanlist.Count,
                        noofrows = item.Count(),
                        brandlist = string.Join(",", item.ToList().ConvertAll(x => x.Brand).Distinct().ToList()),
                        missing_uanlist = String.Join(",", uanlist)
                    });
                    //returnvalue = UploadBills(list, result);
                }
            }

            Common.ImageData.File filedata = new Common.ImageData.File();
            filedata.createdby = imageData.Context.UserID;
            filedata.createdon = DateTime.Now;
            filedata.data = file.data;
            filedata.ext = file.ext;
            filedata.filename = file.filename;
            filedata.isactive = true;
            filedata.modifiedby = imageData.Context.UserID;
            filedata.modifiedon = DateTime.Now;
            filedata.version = 1;
            filedata.type = "billimp";
            filedata = imageData.Insert(file);

            result.columns = data.columns.ConvertAll(e => e.excelcolunmn).ToArray();
            result.filename = file.filename;
            result.fileid = file.id;
            return result;
        }

        internal Models.LrinvoiceDetail UploadLRData(MigrationData data, ImageData imageData)
        {
            Models.LrinvoiceDetail returnvalue = null;
            var file = imageData.GetData(data.uploadfileid.LongFromBase62());
            using (System.IO.MemoryStream memoryStream = new MemoryStream(file.data))
            {
               var list = Process(memoryStream, data.columns.ConvertAll(e => e.excelcolunmn).ToArray());
                BillSummary result = new BillSummary() { Items = new List<BillStatus>(),MigrationData = data };
                foreach (var item in list.GroupBy(e => e.BillNo))
                {
                    var uanlist = item.ToList().FindAll(x => !string.IsNullOrEmpty(x.UAN)).ConvertAll(x => x.UAN);
                    if (uanlist.Count > 0)
                    {
                        string query = @"select gln from items where gln in (" + string.Join(",", uanlist.ConvertAll(x => "'" + x + "'")) + ") and isactive=true";
                        using (var reader = imageData.ExecuteReader(query, new SortedList<string, object>()))
                        {
                            while (reader.Read())
                            {
                                uanlist.Remove(reader.GetString(0));
                            }
                        }
                    }
                    result.Items.Add(new BillStatus()
                    {
                        suppliercompany = item.First().SupplierName,
                        billvalue = item.Sum(e => e.BillAmount),
                        taxamount = item.Sum(e => e.TaxAmount),
                        baseamount = item.Sum(e => e.BillAmount - e.TaxAmount),
                        invoicedate = item.First().BillDate,
                        invoiceno = item.First().BillNo,
                        noofpieces = (int)item.Sum(e => e.Qty),
                        discount = item.Sum(e => e.PurchaseDiscount),
                        salevalue = item.Sum(e => e.FinalPrice * e.Qty),
                        salediscount = item.Sum(e => (e.MRP - e.FinalPrice) * e.Qty),
                        missing_product = item.Count(e => string.IsNullOrEmpty(e.Product)),
                        missing_section = item.Count(e => string.IsNullOrEmpty(e.Section)),
                        missing_productcategory = item.Count(e => string.IsNullOrEmpty(e.ProductCategory)),
                        missing_brand = item.Count(e => string.IsNullOrEmpty(e.Brand)),
                        missing_size = item.Count(e => string.IsNullOrEmpty(e.Size)),
                        missing_pattern = item.Count(e => string.IsNullOrEmpty(e.Pattern)),
                        missing_material = item.Count(e => string.IsNullOrEmpty(e.Material)),
                        missing_style = item.Count(e => string.IsNullOrEmpty(e.Style)),
                        missing_colour = item.Count(e => string.IsNullOrEmpty(e.Colour)),
                        missing_type = item.Count(e => string.IsNullOrEmpty(e.Type)),
                        missing_sleeve = item.Count(e => string.IsNullOrEmpty(e.Sleeve)),
                        missing_design = item.Count(e => string.IsNullOrEmpty(e.DesignID)),
                        missing_uan = uanlist.Count,
                        noofrows = item.Count(),
                        brandlist = string.Join(",", item.ToList().ConvertAll(x => x.Brand).Distinct().ToList()),
                        missing_uanlist = String.Join(",", uanlist)
                    });
                   returnvalue = UploadBills(list, result);
                }
            }

            return returnvalue;
        }

        public List<LrInvoiceItem> Process(Stream source, string[] excelcolumnnames)
        {
            var excelheader = new List<string>();
            var columnsheaders = new List<string>();
            var rows = new List<List<string>>();
            var columnindex = new SortedDictionary<string, int>();
            var columnnames = "Bill No,Bill Date,DesignID,LR Date,Purchase Discount,From Location,Supplier Name,Source Supplier Name,Company,Location,Supplier GST,LRNO,Qty,MRP,Cost Price,Tax Percentage,Tax Peritem,Price (Before Discount),Sales Discount %,Final Price,UAN,HSN,Item Code,Section,Product Category,Product,Brand,Size,Style,Pattern,Type,Material,Colour,Fit,Sleeve,Full Name".Split(',');

            SortedDictionary<string, MigrationManager.ColumnMap> columnmap = new SortedDictionary<string, MigrationManager.ColumnMap>();

            if (columnnames.Count() == excelcolumnnames.Count())
            {
                for (int i = 0; i < columnnames.Length; i++)
                {
                    columnmap.Add(columnnames[i].ToString().ToLower(), new MigrationManager.ColumnMap()
                    {
                        columnmap = excelcolumnnames[i],
                        columnname = columnnames[i].ToString().ToLower()
                    });
                }
            }
            else
            {
                var ecolumns = new List<string>(excelcolumnnames).ConvertAll(e => e.ToLower());

                for (int i = 0; i < columnnames.Length; i++)
                {
                    if (ecolumns.Contains(columnnames[i].ToLower()))
                    {
                        columnmap.Add(columnnames[i].ToString().ToLower(), new MigrationManager.ColumnMap()
                        {
                            columnmap = columnnames[i],
                            columnname = columnnames[i].ToString().ToLower(),
                            index = ecolumns.IndexOf(columnnames[i].ToLower())
                        });
                    }
                }
            }

            using (source)
            {
                ExcelPackage package = new ExcelPackage(source);
                var worksheet = package.Workbook.Worksheets[1];
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
                            columnsheaders.Add(scellvalue);
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
                        if(!rowvalues.All(x=>string.IsNullOrEmpty(x)))
                            rows.Add(rowvalues);
                    }
                }
            }

            foreach (var item in columnmap)
            {
                item.Value.columnmap = item.Value.columnmap != null  ? item.Value.columnmap.ToLower() : "";
                item.Value.index = columnsheaders.Contains(item.Value.columnmap) ? (columnsheaders.IndexOf(item.Value.columnmap)) : -1;
            }

            List<LrInvoiceItem> lrInvoiceItems = new List<LrInvoiceItem>();
            foreach (var item in rows)
            {
                lrInvoiceItems.Add(new LrInvoiceItem(item, columnmap));
            }
            if(lrInvoiceItems.Exists(e=>string.IsNullOrEmpty(e.BillNo)))
            {
                lrInvoiceItems.RemoveAll(x => string.IsNullOrEmpty(x.BillNo) || string.IsNullOrEmpty(x.SupplierName));
                int i = 0;
                foreach (var item in lrInvoiceItems.GroupBy(e => new { e.BillNo, e.SupplierName }))
                {
                    i++;
                    if (string.IsNullOrEmpty(item.Key.BillNo))
                    {
                        var billno  = "AUTOBILL-" + i.ToString();
                        item.ToList().ForEach(e => e.BillNo = billno);
                    }
                }
            }


            return lrInvoiceItems;
        }


        public class LrInvoiceItem
        {

            private static CultureInfo enUS = new CultureInfo("en-US");
            public DateTime GetDate(List<string> source, SortedDictionary<string, MigrationManager.ColumnMap> columnMap, string name)
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

            public decimal GetDecimal(List<string> source, SortedDictionary<string, MigrationManager.ColumnMap> columnMap, string name)
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
            public string GetString(List<string> source, SortedDictionary<string, MigrationManager.ColumnMap> columnMap, string name)
            {
                name = name.ToLower();
                if (columnMap.ContainsKey(name))
                {
                    if (columnMap[name].index >= 0 && source.Count > columnMap[name].index)
                        return isEmpty(source[columnMap[name].index]) ? "" : source[columnMap[name].index].Trim();

                    return isEmpty(columnMap[name].defaultvalue) ? "" : columnMap[name].defaultvalue;
                }
                return "";
            }

            public LrInvoiceItem(List<string> source, SortedDictionary<string, MigrationManager.ColumnMap> columnMap)
            {
                DesignID = GetString(source, columnMap, "DesignID");
                BillNo = GetString(source, columnMap, "Bill No");
                FromLocation = GetString(source, columnMap, "From Location");
                BillDate = GetDate(source, columnMap, "Bill Date");
                LRDate = GetDate(source, columnMap, "LR Date");
                SupplierName = GetString(source, columnMap, "Supplier Name");
                SourceSupplierName = GetString(source, columnMap, "Source Supplier Name");
                Company = GetString(source, columnMap, "Company");
                Location = GetString(source, columnMap, "Location");
                SupplierGST = GetString(source, columnMap, "Supplier GST");
                LRNO = GetString(source, columnMap, "LRNO");
                Qty = GetDecimal(source, columnMap, "Qty");
                if (Qty == 0)
                    Qty = GetDecimal(source, columnMap, "AQty");
                MRP = GetDecimal(source, columnMap, "MRP");
                CostPrice = GetDecimal(source, columnMap, "Cost Price");

                IsRangedTax = GetString(source, columnMap, "Tax Percentage").IndexOf('-') > 0;

                TaxPercentage = Math.Round(GetDecimal(source, columnMap, "Tax Percentage"), 2);
                if (TaxPercentage <= 0 && TaxPercentage > 0)
                    TaxPercentage = TaxPercentage * 100;

                PurchaseDiscount = Math.Abs(GetDecimal(source, columnMap, "Purchase Discount"));
                TaxPeritem = GetDecimal(source, columnMap, "Tax Peritem");
                if (TaxPeritem == 0 && CostPrice > 0 && TaxPercentage > 0)
                {
                    TaxPeritem = Math.Round((CostPrice - (PurchaseDiscount/Qty)) * TaxPercentage / 100, 4);
                }
                Price = GetDecimal(source, columnMap, "Price (Before Discount)");
                SalesDiscount = GetDecimal(source, columnMap, "Sales Discount %");
                FinalPrice = GetDecimal(source, columnMap, "Final Price");
                UAN = GetString(source, columnMap, "UAN");
                HSN = GetString(source, columnMap, "HSN");
                ItemCode = GetString(source, columnMap, "Item Code");
                Section = GetString(source, columnMap, "Section");
                ProductCategory = GetString(source, columnMap, "Product Category");
                Product = GetString(source, columnMap, "Product");
                Brand = GetString(source, columnMap, "Brand");
                Size = GetString(source, columnMap, "Size");
                Style = GetString(source, columnMap, "Style");
                Pattern = GetString(source, columnMap, "Pattern");
                Type = GetString(source, columnMap, "Type");
                Material = GetString(source, columnMap, "Material");
                Colour = GetString(source, columnMap, "Colour");

                Fit = GetString(source, columnMap, "Fit");
                Sleeve = GetString(source, columnMap, "Sleeve");

                FullName = GetString(source, columnMap, "Full Name");

            }

            public decimal CostAmount
            {
                get
                {
                    return (CostPrice * Qty);
                }
            }

            public decimal BillAmount
            {
                get
                {
                    return CostAmount + TaxAmount;
                }
            }

            public decimal TaxAmount
            {
                get
                {
                    return TaxPeritem * Qty;
                }
            }


            public string DesignID { get; set; }
            public string BillNo { get; set; }
            public string FromLocation { get; private set; }
            public DateTime BillDate { get; set; }
            public DateTime LRDate { get; set; }
            public string SupplierName { get; set; }
            public string SourceSupplierName { get; set; }
            public string Company { get; set; }
            public string Location { get; set; }
            public string SupplierGST { get; set; }
            public string LRNO { get; set; }
            public decimal Qty { get; set; }
            public decimal MRP { get; set; }
            public decimal CostPrice { get; set; }
            public decimal TaxPercentage { get; set; }
            public bool IsRangedTax { get; set; }
            public decimal PurchaseDiscount { get; set; }
            public decimal TaxPeritem { get; set; }
            public decimal Price { get; set; }
            public decimal SalesDiscount { get; set; }
            public decimal FinalPrice { get; set; }
            public string UAN { get; set; }
            public string HSN { get; set; }
            public string ItemCode { get; set; }
            public string Section { get; set; }
            public string ProductCategory { get; set; }
            public string Product { get; set; }
            public string Brand { get; set; }
            public string Size { get; set; }
            public string Style { get; set; }
            public string Pattern { get; set; }
            public string Type { get; set; }
            public string Material { get; set; }
            public string Colour { get; set; }
            public string Fit { get; set; }
            public string Sleeve { get; set; }
            public string FullName { get; set; }
            public long supplierid { get; set; }
            public long sourcesupplierid { get; set; }
            public long itemid { get; set; }
            public long productid { get; set; }
            public long productcategoryid { get; set; }
            public long sectionid { get; set; }
            public long brandid { get; set; }
            public long colourid { get; set; }
            public long materialid { get; set; }
            public long patternid { get; set; }
            public long sizeid { get; set; }
            public long styleid { get; set; }
            public long typeid { get; set; }
            public long suppliercompanyid { get; set; }
            public long locationid { get; set; }
            public long companyid { get; set; }
            public long taxid { get; set; }
            public long fromlocationid { get; set; }
            public bool interstate { get; set; }
            public long sleeveid { get;  set; }
            public long fitid { get; set; }
        }

        public Models.LrinvoiceDetail UploadBills(List<LrInvoiceItem> lrInvoiceItems, BillSummary source)
        {
            Models.LrinvoiceDetail lrinvoice = new Models.LrinvoiceDetail();
            InfyPOS.Data.TaxDataService taxDataService = new InfyPOS.Data.TaxDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } }; 
            var taxlist = taxDataService.Search(new InfyPOS.Models.TaxCreteria());

            InfyPOS.Data.CompanyDataService companyDataService = new InfyPOS.Data.CompanyDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } }; 
            var companylist = companyDataService.Search(new InfyPOS.Models.CompanyCreteria());

            var missingcompany = lrInvoiceItems.FindAll(x => !companylist.Exists(y => y.name == x.Company));
            if(missingcompany.Count>0)
            {
                throw new Exception("Please create company - " + missingcompany.First().Company);
            }

            InfyPOS.Data.SupplierDataService supplierDataService = new InfyPOS.Data.SupplierDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } }; ;

            InfyPOS.Data.ProductsDataService productsDataService = new InfyPOS.Data.ProductsDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } }; ;
            var productlist = productsDataService.Search(new InfyPOS.Models.ProductsCreteria()
            {

            });

            InfyPOS.Data.ReferencelistDataService referencelistDataService = new InfyPOS.Data.ReferencelistDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } }; ;
            InfyPOS.Data.ItemsDataService itemsDataService = new InfyPOS.Data.ItemsDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } }; ;
            InfyPOS.Data.BrandDataService brandDataService = new InfyPOS.Data.BrandDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } }; ;
            var brandlist = brandDataService.Search(new InfyPOS.Models.BrandCreteria()
            {

            });

            foreach (var item in lrInvoiceItems.GroupBy(e => e.SupplierName))
            {
                var supplierlist = supplierDataService.Search(new InfyPOS.Models.SupplierCreteria()
                {
                    allsupplier=true,
                    name = item.Key,
                });
                var supplier = supplierlist.Exists(e => e.name.ToLower() == item.Key.ToLower()) ? supplierlist.Find(e => e.name.ToLower() == item.Key.ToLower()) : new InfyPOS.Models.Supplier() { isactive = true };

                if (supplier.id == 0) throw new Exception("Please Create Supplier  - " + item.Key);
                if (supplier.id == 0)
                {
                    supplier.name = item.Key;
                    supplier.isinterstatesale = source.interstate;
                    supplier.companylist = new List<InfyPOS.Models.Suppliercompany>();
                    supplier.companylist.Add(new InfyPOS.Models.Suppliercompany()
                    {
                        name = supplier.name,
                        isinterstatesale = source.interstate,
                        isactive = true
                    });
                    supplierDataService.Insert(supplier);
                }
                item.ToList().ForEach(e =>
                {
                    e.supplierid = supplier.id;
                    e.suppliercompanyid = supplier.companylist[0].id;
                    e.interstate = supplier.companylist[0].isinterstatesale;
                });
            }

            foreach (var item in lrInvoiceItems.GroupBy(e => e.SourceSupplierName))
            {
                if (string.IsNullOrEmpty(item.Key)) continue;
                var supplierlist = supplierDataService.Search(new InfyPOS.Models.SupplierCreteria()
                {
                    allsupplier = true,
                    name = item.Key,
                });
                var supplier = supplierlist.Exists(e => e.name.ToLower() == item.Key.ToLower()) ? supplierlist.Find(e => e.name.ToLower() == item.Key.ToLower()) : new InfyPOS.Models.Supplier() { isactive = true };

                if (supplier.id == 0) throw new Exception("Please Create Source Supplier  - " + item.Key);
                item.ToList().ForEach(e =>
                {
                    e.sourcesupplierid = supplier.id;
                });
            }

            var types = new List<string>();
            types.AddRange("PRODUCTGROUPS,SIZE,PATTERN,STYLE,TYPE,COLOUR,MATERIAL,DIVISION,LOCATION,CITY,FIT,SLEEVE".Split(','));

            var referencelist = referencelistDataService.Search(new InfyPOS.Models.ReferencelistCreteria()
            {
                types = types
            });

            var missinglocation = lrInvoiceItems.FindAll(x => !referencelist.Exists(y => y.type =="LOCATION" && y.name.ToLower() == x.Location.ToLower()));
            if (missinglocation.Count > 0)
            {
                throw new Exception("Please create location - " + missinglocation.First().Location);
            }
            
            List<LrInvoiceItem> newitems = new List<LrInvoiceItem>();
            foreach (var item in lrInvoiceItems.GroupBy(e => e.UAN))
            {
                if (!string.IsNullOrEmpty(item.Key))
                {
                    var itemlist = itemsDataService.Search(new InfyPOS.Models.ItemsCreteria()
                    {
                        support_gln = true,
                        gln = item.Key
                    });
                    var supplier = itemlist.Exists(e => e.gln == item.Key) ? itemlist.Find(e => e.gln == item.Key) : new InfyPOS.Models.Items();
                    if (supplier.id == 0)
                    {
                        newitems.AddRange(item);
                    }
                    else
                    {
                        var product = productlist.Find(e => e.id == supplier.productid);
                        if (product == null) product = new Products();
                        item.ToList().ForEach(e =>
                        {
                            e.itemid = supplier.id;
                            e.taxid = product.purchasetaxid;
                            e.productid = product.id;
                            e.brandid = supplier.brandid;
                            e.colourid = supplier.colourid;
                            e.fitid = supplier.fitid;
                            e.sleeveid = supplier.sleeveid;
                            e.materialid = supplier.materialid;
                            e.patternid = supplier.patternid;
                            e.productcategoryid = product.productgroup;
                            e.sectionid = product.salescounterid;
                            e.sizeid = supplier.sizeid;
                            e.styleid = supplier.styleid;
                            e.typeid = supplier.typeid;
                        });
                    }
                }
                else
                {
                    newitems.AddRange(item);
                }
            }

            if(!InfyPOS.Services.ConfigManager.Instance(organizationid).Config.system_autoglnimport)
            {
                var missinguan = lrInvoiceItems.FindAll(x => !string.IsNullOrEmpty(x.UAN) && x.itemid <= 0);
                if(missinguan.Count>0)
                {
                    throw new Exception("The below UAN not available in this system, please create them before to import \r\n" + string.Join("\r\n", missinguan.ConvertAll(x => x.UAN)));
                }
            }


            foreach (var item in lrInvoiceItems.GroupBy(e => e.FromLocation))
            {
                var supplier = referencelist.Exists(e => e.type == "CITY" && e.name.ToLower() == item.Key.ToLower()) ? referencelist.Find(e => e.type == "CITY" && e.name.ToLower() == item.Key.ToLower()) : new InfyPOS.Models.Referencelist() { isactive = true };
                if (supplier.id == 0)
                {
                    supplier.name = item.Key;
                    supplier.type = "CITY";
                    referencelistDataService.Insert(supplier);
                }
                item.ToList().ForEach(e => e.fromlocationid = supplier.id);
            }

            foreach (var item in lrInvoiceItems.GroupBy(e => e.Location))
            {
                var supplier = referencelist.Exists(e => e.type == "LOCATION" && e.name == item.Key) ? referencelist.Find(e => e.type == "LOCATION" && e.name == item.Key) : new InfyPOS.Models.Referencelist() { isactive = true };
                if (supplier.id == 0)
                {
                    supplier.name = item.Key;
                    supplier.type = "LOCATION";
                    referencelistDataService.Insert(supplier);
                }
                item.ToList().ForEach(e => e.locationid = supplier.id);
            }

            foreach (var item in lrInvoiceItems.GroupBy(e => e.Company))
            {
                var supplier = companylist.Exists(e => e.name == item.Key) ? companylist.Find(e => e.name == item.Key) : new InfyPOS.Models.Company() { isactive = true };
                if (supplier.id == 0)
                {
                    supplier.name = item.Key;
                    companyDataService.Insert(supplier);
                }
                item.ToList().ForEach(e => e.companyid = supplier.id);
            }

            foreach (var item in newitems.GroupBy(e => e.ProductCategory))
            {
                var supplier = referencelist.Exists(e => e.type == "PRODUCTGROUPS" && e.name == item.Key) ? referencelist.Find(e => e.type == "PRODUCTGROUPS" && e.name == item.Key) : new InfyPOS.Models.Referencelist() { isactive = true };
                if (supplier.id == 0)
                {
                    supplier.name = item.Key;
                    supplier.type = "PRODUCTGROUPS";
                    referencelistDataService.Insert(supplier);
                }
                item.ToList().ForEach(e => e.productcategoryid = supplier.id);
            }

            foreach (var item in newitems.GroupBy(e => e.Section))
            {
                var supplier = referencelist.Exists(e => e.type == "SECTION" && e.name == item.Key) ? referencelist.Find(e => e.type == "SECTION" && e.name == item.Key) : new InfyPOS.Models.Referencelist() { isactive = true };
                if (supplier.id == 0)
                {
                    supplier.name = item.Key;
                    supplier.type = "SECTION";
                    referencelistDataService.Insert(supplier);
                }
                item.ToList().ForEach(e => e.sectionid = supplier.id);
            }

            foreach (var item in newitems.GroupBy(e => e.Product))
            {
                var product = productlist.Exists(e => e.name == item.Key) ? productlist.Find(e => e.name == item.Key) : new InfyPOS.Models.Products() { isactive = true };
                if (product.id == 0)
                {
                    var istaxrange = item.Any(e => e.IsRangedTax);
                    if (istaxrange)
                    {
                        if (taxlist.Exists(e => e.type == 2 && e.taxsplit != null
                        && e.taxsplit.items != null
                        && e.taxsplit.items.Count == 2))
                        {
                            product.salestaxid = taxlist.Find(e => e.type == 2 && e.taxsplit != null
                                && e.taxsplit.items != null
                                && e.taxsplit.items.Count == 2).id;
                        }
                        else
                        {
                            InfyPOS.Models.Tax tax = new InfyPOS.Models.Tax()
                            {
                                taxpercentage = item.First().TaxPercentage,
                                ispurchasetax = true,
                                issalestax = true,
                                isactive = true,
                                type = 2,
                                taxsplit = new InfyPOS.Models.Tax.TaxSplit()
                            };
                            tax.taxsplit.items = new List<InfyPOS.Models.Tax.TaxSplit.Item>();
                            tax.taxsplit.items.Add(new InfyPOS.Models.Tax.TaxSplit.Item()
                            {
                                fromamount = 0,
                                toamount = 1045,
                                cgst = (decimal)2.5,
                                igst = (decimal)2.5,
                                sgst = (decimal)2.5
                            });
                            tax.taxsplit.items.Add(new InfyPOS.Models.Tax.TaxSplit.Item()
                            {
                                fromamount = 1045,
                                toamount = 1000000,
                                cgst = (decimal)6,
                                igst = (decimal)6,
                                sgst = (decimal)6
                            });
                            taxDataService.Insert(tax);
                            item.ToList().ForEach(e => e.taxid = tax.id);
                            product.salestaxid = tax.id;
                            product.purchasetaxid = tax.id;
                        }
                    }
                    else
                    {
                        if (taxlist.Exists(e => e.taxpercentage == item.First().TaxPercentage))
                        {
                            product.salestaxid = taxlist.Find(e => e.taxpercentage == item.First().TaxPercentage).id;
                            product.purchasetaxid = taxlist.Find(e => e.taxpercentage == item.First().TaxPercentage).id;
                            item.ToList().ForEach(e => e.taxid = product.salestaxid);
                        }
                        else
                        {
                            throw new Exception("Please create tax master - " + item.First().TaxPercentage);

                            //InfyPOS.Models.Tax tax = new InfyPOS.Models.Tax()
                            //{
                            //    taxpercentage = item.First().TaxPercentage,
                            //    ispurchasetax = true,
                            //    issalestax = true,
                            //    isactive = true,
                            //    type = 1,
                            //    taxsplit = new InfyPOS.Models.Tax.TaxSplit()
                            //};
                            //tax.taxsplit.items = new List<InfyPOS.Models.Tax.TaxSplit.Item>();
                            //tax.taxsplit.items.Add(new InfyPOS.Models.Tax.TaxSplit.Item()
                            //{
                            //    fromamount = 0,
                            //    toamount = 0,
                            //    cgst = (decimal)2.5,
                            //    igst = (decimal)2.5,
                            //    sgst = (decimal)2.5
                            //});
                            //taxDataService.Insert(tax);
                            //item.ToList().ForEach(e => e.taxid = tax.id);
                            //product.salestaxid = tax.id;
                            //product.purchasetaxid = tax.id;
                        }
                    }

                    product.name = item.First().Product;
                    product.productgroup = item.First().productcategoryid;
                    product.salescounterid = item.First().sectionid;
                    product.hsncode = item.First().HSN;
                    product.attributefilter = true;
                    product.barcodemode = 2;
                    product.BarcodeSource = InfyPOS.Models.Products.BarcodeSources.Item;
                    product.SellingMode = InfyPOS.Models.Products.sellingMode.Pack;
                    product.attributes = new InfyPOS.Models.Products.ProductAttributes()
                    {
                        serialno = true
                    };
                    productsDataService.Insert(product);
                }
                item.ToList().ForEach(e => e.productid = product.id);
            }

            foreach (var item in newitems.GroupBy(e => e.Brand))
            {
                var supplier = brandlist.Exists(e => e.name == item.Key) ? brandlist.Find(e => e.name == item.Key) : new InfyPOS.Models.Brand() { isactive = true };
                if (supplier.id == 0)
                {
                    supplier.name = item.Key;
                    brandDataService.Insert(supplier);
                }
                item.ToList().ForEach(e => e.brandid = supplier.id);
            }


            foreach (var item in newitems.GroupBy(e => e.Colour))
            {
                var supplier = referencelist.Exists(e => e.type == "COLOUR" && e.name == item.Key) ? referencelist.Find(e => e.type == "COLOUR" && e.name == item.Key) : new InfyPOS.Models.Referencelist() { isactive = true };
                if (supplier.id == 0)
                {
                    supplier.name = item.Key;
                    supplier.type = "COLOUR";
                    referencelistDataService.Insert(supplier);
                }
                item.ToList().ForEach(e => e.colourid = supplier.id);
            }

            foreach (var item in newitems.GroupBy(e => e.Sleeve))
            {
                var supplier = referencelist.Exists(e => e.type == "SLEEVE" && e.name == item.Key) ? referencelist.Find(e => e.type == "SLEEVE" && e.name == item.Key) : new InfyPOS.Models.Referencelist() { isactive = true };
                if (supplier.id == 0)
                {
                    supplier.name = item.Key;
                    supplier.type = "SLEEVE";
                    referencelistDataService.Insert(supplier);
                }
                item.ToList().ForEach(e => e.sleeveid = supplier.id);

            }

            foreach (var item in newitems.GroupBy(e => e.Fit))
            {
                var supplier = referencelist.Exists(e => e.type == "FIT" && e.name == item.Key) ? referencelist.Find(e => e.type == "FIT" && e.name == item.Key) : new InfyPOS.Models.Referencelist() { isactive = true };
                if (supplier.id == 0)
                {
                    supplier.name = item.Key;
                    supplier.type = "FIT";
                    referencelistDataService.Insert(supplier);
                }
                item.ToList().ForEach(e => e.fitid = supplier.id);
            }

            foreach (var item in newitems.GroupBy(e => e.Material))
            {
                var supplier = referencelist.Exists(e => e.type == "MATERIAL" && e.name == item.Key) ? referencelist.Find(e => e.type == "MATERIAL" && e.name == item.Key) : new InfyPOS.Models.Referencelist() { isactive = true };
                if (supplier.id == 0)
                {
                    supplier.name = item.Key;
                    supplier.type = "MATERIAL";
                    referencelistDataService.Insert(supplier);
                }
                item.ToList().ForEach(e => e.materialid = supplier.id);
            }

            foreach (var item in newitems.GroupBy(e => e.Pattern))
            {
                var supplier = referencelist.Exists(e => e.type == "PATTERN" && e.name == item.Key) ? referencelist.Find(e => e.type == "PATTERN" && e.name == item.Key) : new InfyPOS.Models.Referencelist() { isactive = true };
                if (supplier.id == 0)
                {
                    supplier.name = item.Key;
                    supplier.type = "PATTERN";
                    referencelistDataService.Insert(supplier);
                }
                item.ToList().ForEach(e => e.patternid = supplier.id);
            }

            foreach (var item in newitems.GroupBy(e => e.Size))
            {
                var supplier = referencelist.Exists(e => e.type == "SIZE" && e.name == item.Key) ? referencelist.Find(e => e.type == "SIZE" && e.name == item.Key) : new InfyPOS.Models.Referencelist() { isactive = true };
                if (supplier.id == 0)
                {
                    supplier.name = item.Key;
                    supplier.type = "SIZE";
                    referencelistDataService.Insert(supplier);
                }
                item.ToList().ForEach(e => e.sizeid = supplier.id);
            }

            foreach (var item in newitems.GroupBy(e => e.Style))
            {
                var supplier = referencelist.Exists(e => e.type == "STYLE" && e.name == item.Key) ? referencelist.Find(e => e.type == "STYLE" && e.name == item.Key) : new InfyPOS.Models.Referencelist() { isactive = true };
                if (supplier.id == 0)
                {
                    supplier.name = item.Key;
                    supplier.type = "STYLE";
                    referencelistDataService.Insert(supplier);
                }
                item.ToList().ForEach(e => e.styleid = supplier.id);
            }

            foreach (var item in newitems.GroupBy(e => e.Type))
            {
                var supplier = referencelist.Exists(e => e.type == "TYPE" && e.name == item.Key) ? referencelist.Find(e => e.type == "TYPE" && e.name == item.Key) : new InfyPOS.Models.Referencelist() { isactive = true };
                if (supplier.id == 0)
                {
                    supplier.name = item.Key;
                    supplier.type = "TYPE";
                    referencelistDataService.Insert(supplier);
                }
                item.ToList().ForEach(e => e.typeid = supplier.id);
            }


            foreach (var item in newitems.GroupBy(e => new
            {
                e.productid,
                e.brandid,
                e.materialid,
                e.colourid,
                e.sleeveid,
                e.fitid,
                e.sizeid,
                e.typeid,
                e.patternid,
                e.styleid,
                e.UAN
            }))
            {
                InfyPOS.Models.Items items = new InfyPOS.Models.Items()
                {
                    productid = item.Key.productid,
                    brandid = item.Key.brandid,
                    materialid = item.Key.materialid,
                    colourid = item.Key.colourid,
                    sizeid = item.Key.sizeid,
                    typeid = item.Key.typeid,
                    fitid = item.Key.fitid,
                    sleeveid = item.Key.sleeveid,
                    patternid = item.Key.patternid,
                    styleid = item.Key.styleid,
                    sellingname = item.First().FullName,
                    productname = item.First().Product,
                    materialname = item.First().Material,
                    colourname = item.First().Colour,
                    sizename = item.First().Size,
                    typename = item.First().Type,
                    patternname = item.First().Pattern,
                    stylename = item.First().Style,
                    fitname = item.First().Fit,
                    sleevename = item.First().Sleeve,
                    purchaserate = item.First().CostPrice,
                    allowsearch = true,
                    mrp = item.First().MRP,
                    salerate = item.First().Price,
                    isactive = true,
                    support_gln = !string.IsNullOrEmpty(item.Key.UAN),
                    gln = item.Key.UAN
                };
                itemsDataService.Insert(items);
                item.ToList().ForEach(e => e.itemid = items.id);
            }


            InfyPOS.Data.LrentryDataService lrentryDataService = new InfyPOS.Data.LrentryDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } }; ;
            InfyPOS.Data.LrinvoiceDataService lrinvoiceDataService = new InfyPOS.Data.LrinvoiceDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } }; ;

            foreach (var item in lrInvoiceItems.GroupBy(e => e.BillNo))
            {
                if (string.IsNullOrEmpty(item.First().LRNO))
                {
                    item.ToList().ForEach(e => e.LRNO = "LR-" + item.Key);
                    item.ToList().ForEach(e => e.LRDate = item.First().BillDate);
                }

                var locationinfo = referencelist.Find(x => x.id == item.First().locationid);

                if (locationinfo == null) throw new Exception("Location Information not found ");
                var iswarehouse = locationinfo.additionalinfo.iswarehouse;
                
                if (item.ToList().Exists(e => e.productid == 0))
                {
                    LrInvoiceItem missingproduct = item.ToList().Find(e => e.productid == 0);
                    throw new Exception("Product Mapping is Missing with GLN/UAN Code - (" + missingproduct.UAN + ")");
                }

                InfyPOS.Models.Lrentry lrentry = new InfyPOS.Models.Lrentry()
                {
                    supplierid = item.First().supplierid,
                    sourcesupplierid = item.First().sourcesupplierid,
                    isactive = true,
                    createdon = DateTime.Now,
                    modifiedon = DateTime.Now,
                    invoicedate = item.First().BillDate,
                    lrdate = item.First().LRDate,
                    lrentrydate = DateTime.Now.Date,
                    receiveddate = DateTime.Now.Date,
                    invoiceno = item.First().BillNo,
                    lrno = item.First().LRNO,
                    noofpieces = Convert.ToInt32(item.Sum(e => e.Qty)),
                    Status = InfyPOS.Models.Inbox.StatusList.New_LR,
                    lrcompanyid = item.First().companyid,
                    locationid = item.First().locationid,
                    fromlocationid = item.First().fromlocationid,
                    entrylocationid = item.First().locationid,
                    receivinglocationid = item.First().locationid,
                    retaillocationid = iswarehouse ? 0 : item.First().locationid,
                    totalamount = item.Sum(e => e.BillAmount-e.PurchaseDiscount),
                    attachment = new List<Lrentry.slide>()
                };

                if (source.MigrationData != null)
                {
                    lrentry.attachment.Add(new Lrentry.slide()
                    {
                        image = source.MigrationData.uploadfileid,
                        title = "Import Excel",
                        request = Newtonsoft.Json.JsonConvert.SerializeObject(source.MigrationData)
                    });
                }

                // Some times LR Entry will be created manually and PT file will be uploaded
                if (source.lrentryid == 0)
                    lrentryDataService.Insert(lrentry);
                else
                    lrentry.id = source.lrentryid;

                if (lrentry.id == 0) throw new Exception(string.Format("LR Entry Duplicate found. LR NO {0}, Invoice NO {1}, Invoice Date {2}, and Supplier {3}", lrentry.lrno, lrentry.invoiceno, lrentry.invoicedate.ToString("yyyy-MM-dd"), lrentry.supplierid));
                lrinvoice = new InfyPOS.Models.LrinvoiceDetail()
                {
                    lrid = lrentry.id,
                    companyid = item.First().companyid,
                    isinterstatesale = source.interstate || item.First().interstate,
                    entrydate = DateTime.Now.Date,
                    invoicedate = item.First().BillDate,
                    invoiceno = item.First().BillNo,
                    isactive = true,
                    noofpieces = (int)item.Sum(e => e.Qty),
                    Lrinvoiceitems = new List<InfyPOS.Models.LrinvoiceItemDetail>(),
                    Lrinvoicetaxitems = new List<InfyPOS.Models.Lrinvoicetax>(),
                    suppliercompanyid = item.First().suppliercompanyid,
                    supplierid = item.First().supplierid,
                    lrsupplierid = item.First().supplierid,
                    totalamount = item.Sum(e => e.BillAmount - e.PurchaseDiscount),
                    baseamount = item.Sum(e => e.CostAmount),
                    billvalue = item.Sum(e => e.BillAmount - e.PurchaseDiscount),
                    grossamount = item.Sum(e => e.BillAmount - e.PurchaseDiscount),
                    taxamount = item.Sum(e => e.TaxAmount),
                    discount = item.Sum(e => e.PurchaseDiscount)
                };

                foreach (var taxgroup in item.GroupBy(e => e.TaxPercentage))
                {
                    lrinvoice.Lrinvoicetaxitems.Add(new InfyPOS.Models.Lrinvoicetax()
                    {
                        isactive = true,
                        isinterstatesale = taxgroup.First().interstate,
                        percentage = taxgroup.First().TaxPercentage,
                        taxid = taxgroup.First().taxid,
                        type = 0,
                        discount = taxgroup.Sum(e => e.PurchaseDiscount),
                        discountpercentage = taxgroup.Average(e => e.PurchaseDiscount == 0 ? 0 : Math.Round(Math.Abs(e.PurchaseDiscount) / e.Qty / ((e.CostPrice > 0 ? e.CostPrice : e.MRP)) * 100, 2)),
                        amounton = taxgroup.Sum(e => e.CostAmount),
                        taxamount = taxgroup.Sum(e => e.TaxAmount),
                        netamount = taxgroup.Sum(e => e.BillAmount-e.PurchaseDiscount)
                    });
                }

                int serialno = 1;
                foreach (var lrinvoiceitem in item)
                {
                    var margin = source.marginpercentage;
                    if (margin == 0 && lrinvoiceitem.CostPrice > 0)
                        margin = Math.Round((lrinvoiceitem.MRP / lrinvoiceitem.CostPrice * 100) - 100, 2);
                    if (margin <= 2) margin = 5;

                    var newinvoiceitem = new InfyPOS.Models.LrinvoiceItemDetail()
                    {
                        sno = serialno++,
                        productid = lrinvoiceitem.productid,
                        basebuyingprice = lrinvoiceitem.CostPrice > 0 ? lrinvoiceitem.CostPrice : lrinvoiceitem.MRP,
                        buyingprice = lrinvoiceitem.CostPrice > 0 ? lrinvoiceitem.CostPrice : lrinvoiceitem.MRP,
                        buyingqty = lrinvoiceitem.Qty,
                        designid = lrinvoiceitem.DesignID,
                        amount = lrinvoiceitem.CostAmount,
                        hsncode = lrinvoiceitem.HSN,
                        itemid = lrinvoiceitem.itemid,
                        itbuscode = lrinvoiceitem.ItemCode,
                        lrid = lrentry.id,
                        mrp = lrinvoiceitem.MRP,
                        stocklocationid = iswarehouse ? 0 : lrinvoiceitem.locationid,
                        srcsupplierid = lrinvoiceitem.sourcesupplierid,
                        productname = lrinvoiceitem.Product,
                        ptaxid = lrinvoiceitem.taxid,
                        purchasetax = lrinvoiceitem.TaxAmount,
                        purchasetaxpercentage = lrinvoiceitem.TaxPercentage,
                        salediscountpercentage = lrinvoiceitem.SalesDiscount,
                        salediscount = lrinvoiceitem.FinalPrice > 0 ? (lrinvoiceitem.MRP - lrinvoiceitem.FinalPrice) : 0,
                        sellingprice = lrinvoiceitem.MRP,
                        supplierid = lrentry.supplierid,
                        purchasetotaldiscount = Math.Abs(lrinvoiceitem.PurchaseDiscount),
                        purchasediscount = lrinvoiceitem.PurchaseDiscount == 0 ? 0 : (Math.Abs(lrinvoiceitem.PurchaseDiscount) / lrinvoiceitem.Qty),
                        purchasediscountpercentage = lrinvoiceitem.PurchaseDiscount == 0 ? 0 : Math.Round(Math.Abs(lrinvoiceitem.PurchaseDiscount) / lrinvoiceitem.Qty / (lrinvoiceitem.CostPrice > 0 ? lrinvoiceitem.CostPrice : lrinvoiceitem.MRP) * 100, 2),
                        taxid = lrinvoiceitem.taxid,
                        marginpercentage = margin,
                        salestaxpercentage = lrinvoiceitem.TaxPercentage,
                        isactive = true,
                        Details = new InfyPOS.Models.Items()
                        {
                            id = lrinvoiceitem.itemid,
                            productid = lrinvoiceitem.productid,
                            brandid = lrinvoiceitem.brandid,
                            sleeveid = lrinvoiceitem.sleeveid,
                            fitid = lrinvoiceitem.fitid,
                            materialid = lrinvoiceitem.materialid,
                            colourid = lrinvoiceitem.colourid,
                            sizeid = lrinvoiceitem.sizeid,
                            typeid = lrinvoiceitem.typeid,
                            patternid = lrinvoiceitem.patternid,
                            styleid = lrinvoiceitem.styleid,
                            sellingname = lrinvoiceitem.Product,
                            productname = lrinvoiceitem.Product,
                            printingname = lrinvoiceitem.FullName,
                            brandname = lrinvoiceitem.Brand,
                            materialname = lrinvoiceitem.Material,
                            colourname = lrinvoiceitem.Colour,
                            sizename = lrinvoiceitem.Size,
                            typename = lrinvoiceitem.Type,
                            sleevename = lrinvoiceitem.Sleeve,
                            fitname = lrinvoiceitem.Fit,
                            patternname = lrinvoiceitem.Pattern,
                            stylename = lrinvoiceitem.Style,
                            serialno = lrinvoiceitem.UAN
                        }
                    };

                    if (newinvoiceitem.purchasediscount > 0)
                        newinvoiceitem.buyingprice -= newinvoiceitem.purchasediscount;

                    if (source.marginpercentage > 0)
                    {
                        newinvoiceitem.sellingprice = Math.Round((newinvoiceitem.amount / newinvoiceitem.buyingqty) * source.marginpercentage / 100, 0);
                    }
                    if (source.discountpercentage > 0)
                    {
                        newinvoiceitem.salediscount = Math.Round((newinvoiceitem.sellingprice) * source.discountpercentage / 100, 0);
                        newinvoiceitem.salediscountpercentage = source.discountpercentage;
                    }
                    lrinvoice.Lrinvoiceitems.Add(newinvoiceitem);
                }
                lrinvoiceDataService.Insert(lrinvoice);
            }
            return lrinvoice;
        }

        internal BillSummary UploadData(BillSummary bsitem, ImageData imageData)
        {
            var invoices = new List<Models.LrinvoiceDetail>();
            bsitem.invoices = new List<LrinvoiceDetail>();
            var data = imageData.GetData(bsitem.fileid);
            using (var stream = new MemoryStream(data.data))
            {
                var list = Process(stream, bsitem.columns);
                foreach (var item in bsitem.Items.Where(e => e.selected))
                {
                    try
                    {
                        var items = list.FindAll(e => e.BillNo == item.invoiceno && e.SupplierName == item.suppliercompany);
                        bsitem.invoices.Add(UploadBills(items, bsitem));
                        item.success = true;
                    }
                    catch (Exception exp)
                    {
                        item.error = exp.Message;
                    }
                }
                bsitem.haserror = bsitem.Items.Exists(e => e.selected && !e.success);
                bsitem.errormessages = string.Join("\r\n", bsitem.Items.ConvertAll(e => e.invoiceno + "\r\n" + e.error));
                bsitem.importcount = bsitem.Items.Count(e => e.selected && e.success);
            }

            if (bsitem.autogeneratebarcode)
            {
                GenerateBarcode(bsitem);
            }
            else
            {
                //UploadBarcode(bsitem);
            }
            return bsitem;
        }

        public void UploadBarcode(BillSummary bsitem)
        {
            Data.LrinvoiceitemsDataService LrinvoiceitemsData = new Data.LrinvoiceitemsDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } }; ;
            foreach (var invoice in bsitem.invoices)
            {
                var items = invoice.Lrinvoiceitems.ConvertAll(e =>

                {
                    var lb = new Lrinvoiceitemsbarcode()
                    {
                        id = e.id,
                        buyingqty = e.buyingqty,
                        freeqty = e.freeqty,
                        companyid = invoice.companyid,
                        Details = e.Details,
                        hsncode = e.hsncode,
                        itemid = e.itemid,
                        itemdetail = e.itemdetail,
                        lrid = e.lrid,
                        newbarcode = 1,
                        printingname = e.productname,
                        productid = e.productid,
                        salediscount = e.salediscount,
                        sellingprice = e.sellingprice,
                        stocklocationid = e.stocklocationid,
                        version = e.version
                    };
                    var l = new List<string>();
                    l.Add(e.barcode);
                    lb.barcodes = Newtonsoft.Json.JsonConvert.SerializeObject(l);
                    return lb;
                });

                foreach (var item in items)
                    LrinvoiceitemsData.UploadBarcode(item);
            }
        }
        public void GenerateBarcode(BillSummary bsitem)
        {
            foreach (var invoice in bsitem.invoices)
            {
                Data.StockDataService LrinvoiceitemsData = new Data.StockDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } }; ;
                var items = invoice.Lrinvoiceitems.ConvertAll(e => new Lrinvoiceitemsbarcode()
                {
                    id = e.id,
                    buyingqty = e.buyingqty,
                    freeqty = e.freeqty,
                    companyid = invoice.companyid,
                    Details = e.Details,
                    hsncode = e.hsncode,
                    itemid = e.itemid,
                    itemdetail = e.itemdetail,
                    lrid = e.lrid,
                    newbarcode = 1,
                    buyingprice = e.buyingprice,
                    printingname = e.productname,
                    productid = e.productid,
                    salediscount = e.salediscount,
                    sellingprice = e.sellingprice,
                    stocklocationid = e.stocklocationid,
                    version = e.version,
                    gln = e.Details != null ? e.Details.serialno : ""
                });
                LrinvoiceitemsData.CreateBarcodefromList(items);
            }
        }
    }
}