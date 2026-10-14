
using InfyPOS.Common;
using InfyPOS.Data;
using InfyPOS.Models;
using Newtonsoft.Json;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Web;
using WebAPI.Data;
namespace InfyPOS.Processors
{
    public class OfflineManager : Common.BaseData
    {
        long locationid;
        public OfflineManager(long locationid) : base()
        {
            this.locationid = locationid;
        }
        public Controllers.FileContentInfo CreateMasterFile()
        {
            OfflineClient.MasterData data = CreateMasterData(DateTime.MinValue, false, DateTime.Now);
            string json = JsonConvert.SerializeObject(data);
            var fi = new Controllers.FileContentInfo(new System.Net.Http.ByteArrayContent(System.Text.Encoding.UTF8.GetBytes(json)));
            fi.FileName = data.LocationName.Replace(" ", "") + ".offline";
            fi.Extension = "offline";
            return fi;
        }
        public OfflineClient.MasterData CreateMasterData(DateTime afterchanges, bool zerostock, DateTime zerostockfrom)
        {
            OfflineClient.MasterData masterData = new OfflineClient.MasterData();
            masterData.lastSyncOn = DateTime.Now.ToUniversalTime();

            InfyPOS.Data.CompanyDataService company = new InfyPOS.Data.CompanyDataService() { Context = this.Context };
            var companyforlogo = company.Search(new InfyPOS.Models.CompanyCreteria());

            InfyPOS.Data.ConfigurationDataService config = new InfyPOS.Data.ConfigurationDataService() { Context = this.Context };
            var reportformats = config.Search(new InfyPOS.Models.ConfigurationCreteria
            {
                key = "SALERECEIPT"
            });

            if (reportformats.Count == 0)
                reportformats = config.Search(new InfyPOS.Models.ConfigurationCreteria
                {
                    key = "SALEINVOICE"
                });

            reportformats.AddRange(config.Search(new InfyPOS.Models.ConfigurationCreteria
            {
                key = "SETTLEMENT"
            }));

            var imageData = new Common.ImageData() { Context = this.Context };

            List<OfflineClient.ImageData> imageList = new List<OfflineClient.ImageData>();

            ReportLibrary.DotMatrixReporter labreport = new ReportLibrary.DotMatrixReporter(Context.OrganizationID, new ReportLibrary.DotMatrixReporter.PrinterConfig());
            List<OfflineClient.PrintCofig> printconfig = new List<OfflineClient.PrintCofig>();
            foreach (var item in reportformats)
            {
                printconfig.Add(new OfflineClient.PrintCofig()
                {
                    companyid = item.organizationid,
                    locationid = item.locationid,
                    format = item.reportformat.format,
                    key = item.key
                });

                labreport.format = ReportLibrary.DosFormatParser.ParseXML(imageData.Context.OrganizationID, item.reportformat.format);

                foreach (var imgid in labreport.format.GetImages())
                {
                    var file = imageData.GetData(imgid.LongFromBase62());
                    if (file != null && file.data != null)
                    {
                        imageList.Add(new OfflineClient.ImageData() { key = imgid, data = file.data });
                    }
                }
            }

            foreach (var cmp in companyforlogo)
            {
                if (string.IsNullOrEmpty(cmp.logo)) continue;
                var file = imageData.GetData(cmp.logo.LongFromBase62());
                if (file != null && file.data != null)
                {
                    imageList.Add(new OfflineClient.ImageData() { key = "CMP-" + cmp.id, data = file.data });
                }
            }

            string query = @"select name,tax.id,tax.type,taxpercentage,taxsplit from tax 
where isactive = true  and (createdon>=@fromdate or modifiedon>=@fromdate);

select products.id,products.name,products.code,salestaxid,case when sellingmode=3 then '1' else '0' end as mode,prodcompanyid cmpid 
from products where isactive = true  and (createdon>=@fromdate or modifiedon>=@fromdate);

select id,employeecode,name from employee where isactive = true and (createdon>=@fromdate or modifiedon>=@fromdate);

select id,code,name,logo,address from company where isactive = true and (createdon>=@fromdate or modifiedon>=@fromdate);

select code,id,name,additionalinfo->>'address' as address,additionalinfo->>'autosettlement' as autosettlement,
additionalinfo->>'iswarehouse' as iswarehouse
from referencelist  where id = @locationid ;

select salablegoods.companyid,salablegoods.barcode,productid,salablegoods.price,salablegoods.discount,stock.serialno,stock.hsncode from salablegoods
inner join stock on stock.id = salablegoods.stockid 
where salablegoods.locationid = @locationid and iqty-(oqty+dqty+jqty)>0
and salablegoods.isactive = true and (salablegoods.createdon>=@fromdate or stock.modifiedon>=@fromdate);

select id,code,name,additionalinfo->>'floorid' as floorid,additionalinfo->>'locationid' as locationid from referencelist 
where type = 'COUNTER' and isactive = true  and (createdon>=@fromdate or modifiedon>=@fromdate);

select id,username,employeeid,password,permissions,alloweddiscount from users 
where isactive = true  and (createdon>=@fromdate or modifiedon>=@fromdate);

select floorid,referencelist.name floor,companyid,company.name company,locationid,prefix from autonumber
left outer join referencelist on referencelist.id = floorid
left outer join company on company.id = companyid
where autonumber.type ='bill.billno' and autonumber.isactive = true and autonumber.locationid = @locationid";

            //and (createdon>=@fromdate or modifiedon>=@fromdate);
            if (zerostock && afterchanges == DateTime.MinValue)
            {
                query = query.Replace("salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty)>0", "((salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty)>0) or stock.stockdate>=@stockdate)");
            }
            DataSet dset = new DataSet();
            using (var conn = CreateConnection())
            {
                conn.Open();
                var command = GetCommand(query, conn);
                command.CommandTimeout = 100000;
                command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = locationid;
                command.Parameters.Add("fromdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = afterchanges.ToLocalTime();
                command.Parameters.Add("stockdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = zerostockfrom.ToLocalTime();
                NpgsqlDataAdapter npgsqlDataAdapter = new NpgsqlDataAdapter(command);
                npgsqlDataAdapter.Fill(dset);
                conn.Close();
            }
            masterData.autobarcode = InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_supportautomissingstock;
            masterData.locationid = locationid;
            masterData.organizationid = this.Context.OrganizationID;
            masterData.imageDatas = imageList;
            masterData.Tax = Processors.DataTableCustomFormatter.Serialize(dset.Tables[0], true);
            masterData.Products = Processors.DataTableCustomFormatter.Serialize(dset.Tables[1], true);
            masterData.Employee = Processors.DataTableCustomFormatter.Serialize(dset.Tables[2], true);
            masterData.Company = Processors.DataTableCustomFormatter.Serialize(dset.Tables[3], true);
            masterData.Location = Processors.DataTableCustomFormatter.Serialize(dset.Tables[4], true);
            masterData.Stock = Processors.DataTableCustomFormatter.Serialize(dset.Tables[5], true);
            masterData.Counter = Processors.DataTableCustomFormatter.Serialize(dset.Tables[6], true);
            masterData.Users = Processors.DataTableCustomFormatter.Serialize(dset.Tables[7], true);
            masterData.AutoNumber = Processors.DataTableCustomFormatter.Serialize(dset.Tables[8], true);
            masterData.LocationName = dset.Tables[4].Rows[0]["code"].ToString();
            masterData.PrintConfig = printconfig;
            masterData.Promotions = PromotionTable(Context.OrganizationID, locationid);
            return masterData;
        }

        private List<InfyPOS.Processors.OfflineClient.Promotion> PromotionTable(long organizationid, long locationid)
        {
            List<InfyPOS.Processors.OfflineClient.Promotion> results = new List<OfflineClient.Promotion>();
            try
            {
                Data.PromotionsDataService dataservice = new PromotionsDataService() { Context = new UserClaim() { OrganizationID = organizationid } };
                var promotions = dataservice.Search(new Models.PromotionsCreteria()
                {
                    locationid = locationid,
                    includedetail = true
                });
                foreach (var item in promotions)
                {
                    if (!item.enabled) continue;

                    if (item.mode == 1)
                    {
                        results.Add(new OfflineClient.Promotion()
                        {
                            scheme = item
                        });
                        continue;
                    }
                    if (item.rules == null || item.rules.Count == 0) continue;
                    if (!item.enabled) continue;

                    List<string> includebarcodes = new List<string>();
                    List<string> excludebarcodes = new List<string>();
                    bool includebarcode = false;
                    bool excludebarcode = false;
                    System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();
                    foreach (var rule in item.rules)
                    {
                        string query = "";
                        switch (rule.mode)
                        {
                            case Models.Promotions.Rules.RuleMode.barcode:
                                if (!includebarcode)
                                {
                                    includebarcode = true;
                                    query = @"Select 'I',barcode from schemebarcodes where schemeid =" + item.id;
                                }
                                break;
                            case Models.Promotions.Rules.RuleMode.excludebarcode:
                                if (!excludebarcode)
                                {
                                    excludebarcode = true;
                                    query = @"Select 'E',barcode from schemeexcludedbarcodes where schemeid =" + item.id;
                                }
                                break;
                            case Models.Promotions.Rules.RuleMode.attributes:
                                query = @"select 'I',salablegoods.barcode from salablegoods
                                    inner join stock on stock.id = stockid
                                    inner join items on items.id = stock.itemid
                                    where salablegoods.locationid = @locationid ";
                                if (rule.item.productid > 0)
                                    query += " and items.productid = " + rule.item.productid;
                                if (rule.item.brandid > 0)
                                    query += " and items.brandid = " + rule.item.brandid;
                                if (rule.item.styleid > 0)
                                    query += " and items.styleid = " + rule.item.styleid;
                                if (rule.item.typeid > 0)
                                    query += " and items.typeid = " + rule.item.typeid;
                                if (rule.item.patternid > 0)
                                    query += " and items.patternid = " + rule.item.patternid;
                                if (rule.item.materialid > 0)
                                    query += " and items.materialid = " + rule.item.materialid;
                                if (rule.item.sizeid > 0)
                                    query += " and items.sizeid = " + rule.item.sizeid;
                                if (rule.item.colourid > 0)
                                    query += " and items.colourid = " + rule.item.colourid;
                                if (rule.item.fitid > 0)
                                    query += " and items.fitid = " + rule.item.fitid;
                                if (rule.item.sleeveid > 0)
                                    query += " and items.sleeveid = " + rule.item.sleeveid;

                                break;
                            case Models.Promotions.Rules.RuleMode.excludeattribute:
                                query = @"select 'E',salablegoods.barcode from salablegoods
                                    inner join stock on stock.id = stockid
                                    inner join items on items.id = stock.itemid
                                    where salablegoods.locationid = @locationid ";
                                if (rule.item.productid > 0)
                                    query += " and items.productid = " + rule.item.productid;
                                if (rule.item.brandid > 0)
                                    query += " and items.brandid = " + rule.item.brandid;
                                if (rule.item.styleid > 0)
                                    query += " and items.styleid = " + rule.item.styleid;
                                if (rule.item.typeid > 0)
                                    query += " and items.typeid = " + rule.item.typeid;
                                if (rule.item.patternid > 0)
                                    query += " and items.patternid = " + rule.item.patternid;
                                if (rule.item.materialid > 0)
                                    query += " and items.materialid = " + rule.item.materialid;
                                if (rule.item.sizeid > 0)
                                    query += " and items.sizeid = " + rule.item.sizeid;
                                if (rule.item.colourid > 0)
                                    query += " and items.colourid = " + rule.item.colourid;
                                if (rule.item.fitid > 0)
                                    query += " and items.fitid = " + rule.item.fitid;
                                if (rule.item.sleeveid > 0)
                                    query += " and items.sleeveid = " + rule.item.sleeveid;
                                break;
                            case Models.Promotions.Rules.RuleMode.invoiceitem:
                                query = @"select 'I',salablegoods.barcode from salablegoods
                                    inner join stock on stock.id = stockid
                                    inner join items on items.id = stock.itemid
                                    where salablegoods.locationid = @locationid and stock.lrinvoiceitemid in (" + string.Join(",", rule.invoiceitemid) + ")";
                                break;
                            case Models.Promotions.Rules.RuleMode.marker:
                                query = @"select 'I',stockmarker.barcode from stockmarker
                                    where markerid=" + rule.item.markerid;
                                break;
                            case Models.Promotions.Rules.RuleMode.excludemarker:
                                query = @"select 'E',stockmarker.barcode from stockmarker
                                    where markerid=" + rule.item.markerid;
                                break;
                        }
                        if (!string.IsNullOrEmpty(query))
                        {
                            if (stringBuilder.Length > 0)
                                stringBuilder.AppendLine(" union all ");
                            stringBuilder.AppendLine(query);
                        }
                    }
                    if (string.IsNullOrEmpty(stringBuilder.ToString())) continue;
                    var param = new SortedList<string, object>();
                    param.Add("locationid", locationid);
                    using (var reader = ExecuteReader(stringBuilder.ToString(), param))
                    {
                        while (reader.Read())
                        {
                            if (reader.GetString(0) == "I")
                                includebarcodes.Add(reader.GetString(1));
                            else
                                excludebarcodes.Add(reader.GetString(1));
                        }
                    }

                    includebarcodes.RemoveAll(e => excludebarcodes.Contains(e));
                    if (includebarcodes.Count > 0)
                    {
                        results.Add(new OfflineClient.Promotion()
                        {
                            scheme = item,
                            barcode = includebarcodes
                        });
                    }
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(organizationid).Log(exp, WebAPI.Common.Logger.LogType.Error, "Promotion table - " + locationid);
            }
            return results;
        }

        internal void SynchronizeBill(OfflineClient.WindowsOfflineRequest request,
            OfflineClient.WindowsOfflineResponse response)
        {
            var billdata = BufferedRealtimeCompressionEngine.Decompress(request.data);
            var Bills = Newtonsoft.Json.JsonConvert.DeserializeObject<List<OfflineClient.Bill>>(System.Text.ASCIIEncoding.ASCII.GetString(billdata));
            BillDataService BillData = new BillDataService() { Context = new UserClaim() { OrganizationID = request.organizationid } };
            CustomerDataService customerDataService = new CustomerDataService() { Context = new UserClaim { OrganizationID = request.organizationid } };

            var barcodeList = new List<string>();
            Bills.ConvertAll(e => e.Billitems.ConvertAll(ex => ex.barcode.Trim())).ForEach(e =>
            {
                barcodeList.AddRange(e);
            });

            var barcodelist = barcodeList.Distinct().ToList();
            var conditions = barcodelist.ConvertAll(e => "'" + e + "'");
            string query = string.Format(@"select salablegoods.id,stockid,barcode from salablegoods
            where locationid = {0} and barcode in ({1}) ", request.locationid, string.Join(",", conditions));
            SortedDictionary<string, Tuple<long, long>> keyValues = new SortedDictionary<string, Tuple<long, long>>();
            using (var reader = ExecuteReader(query, new SortedList<string, object>()))
            {
                while (reader.Read())
                {
                    keyValues.Add(reader.GetString(2), new Tuple<long, long>(reader.GetInt64(0), reader.GetInt64(1)));
                }
            }

            var missingbarcodes = barcodelist.FindAll(e => !keyValues.ContainsKey(e));


            if (missingbarcodes.Count > 0)
            {
                List<OfflineClient.BillItems> missingbillItems = new List<OfflineClient.BillItems>();
                StringBuilder sb = new StringBuilder();
                Bills.ForEach(e =>
                {
                    var billitems = (e.Billitems.FindAll(y => missingbarcodes.Contains(y.barcode)));
                    if (billitems.Count > 0)
                    {
                        missingbillItems.AddRange(billitems);
                        billitems.ForEach(y =>
                        {
                            y.autobarcode = request.autobarcode;
                            sb.AppendLine("Bill No :" + e.billno + " Product:" + y.printingname + " Barcode:" +
                                y.barcode + " Rate:" + y.rate + " Qty:" + y.qty);
                        });
                    }
                });

                if (!request.autobarcode)
                {
                    lock (response.lockobject)
                    {
                        response.errormessage = "Missing Barcodes " + sb.ToString();
                        response.error = true;
                        response.completed = true;
                        response.completedon = DateTime.Now;
                    }
                    return;
                }
                else
                {
                    var missingproducts = missingbillItems.FindAll(x => x.productid == 0);
                    if (missingproducts.Count > 0)
                    {
                        ProductsDataService productsDataService = new ProductsDataService() { Context = new UserClaim() { OrganizationID = request.organizationid } };
                        var allproducts = productsDataService.SearchList(new ProductsCreteria());
                        missingproducts.ToList().ForEach(x =>
                        {
                            var product = allproducts.Find(y => y.name == x.printingname);
                            if (product != null)
                            {
                                x.productid = product.id;
                            }
                        });
                        missingproducts = missingbillItems.FindAll(x => x.productid == 0);
                        if (missingproducts.Count > 0)
                        {
                            lock (response.lockobject)
                            {
                                response.errormessage = "Missing Products " + String.Join(",",
                                    missingproducts.ConvertAll(x => x.printingname));
                                response.error = true;
                                response.completed = true;
                                response.completedon = DateTime.Now;
                            }
                            return;
                        }
                    }
                    var missingstock = missingbillItems.ConvertAll(x => new MissingStock()
                    {
                        barcode = x.barcode,
                        price = x.price,
                        productid = x.productid,
                        locationid = request.locationid,
                        companyid = x.companyid,
                        buyingprice = x.price,
                        qty = x.qty,
                        sellingname = x.printingname,
                        productname = x.printingname
                    });

                    StockDataService LrinvoiceitemsData = new StockDataService()
                    {
                        Context = new UserClaim()
                        {
                            OrganizationID = request.organizationid,
                            UserID = request.userid
                        }
                    };
                    missingstock = LrinvoiceitemsData.SaveMissingStock(missingstock);
                    missingstock.ForEach(x =>
                    {
                        if (!keyValues.ContainsKey(x.barcode))
                        {
                            keyValues.Add(x.barcode,
                                new Tuple<long, long>(x.salablegoodsid, x.stockid));
                        }
                    });
                }
            }

            var customerid = new SortedList<string, long>();
            var mobilenos = Bills.ConvertAll(e => e.customermobileno).ToList();
            mobilenos.RemoveAll(e => string.IsNullOrEmpty(e));
            if (mobilenos.Count > 0)
            {
                query = string.Format(@"select id,mobileno from customers
            where isactive = true and barcode in ({0}) ", string.Join(",", mobilenos.ConvertAll(e => "'" + e + "'")));

                using (var reader = ExecuteReader(query, new SortedList<string, object>()))
                {
                    while (reader.Read())
                    {
                        customerid.Add(reader.GetString(0), reader.GetInt64(1));
                    }
                }
            }

            SortedList<string, long> duplicatebillnos = new SortedList<string, long>();
            query = "select id,billno from bill where billno in (" + string.Join(",", Bills.ConvertAll(x => "'" + x.billno + "'")) + ")";
            using (var reader = ExecuteReader(query, new SortedList<string, object>()))
            {
                while(reader.Read())
                {
                    duplicatebillnos.Add(reader.GetString(1), reader.GetInt64(0));
                }
            }

            if(duplicatebillnos.Count>0)
            {
                response.status = "Duplicate bill nos - " +
                    String.Join(",", duplicatebillnos.Keys.ToList());
                Bills.RemoveAll(x => duplicatebillnos.ContainsKey(x.billno));
            }

            List<InfyPOS.Models.Bill> billlist = new List<Models.Bill>();
            foreach (var item in Bills)
            {
                InfyPOS.Models.Bill bill = new Models.Bill();
                if (!string.IsNullOrEmpty(item.customermobileno))
                {
                    if (customerid.ContainsKey(item.customermobileno))
                    {
                        bill.customerid = customerid[item.customermobileno];
                        bill.customername = item.customername;
                    }
                    else
                    {
                        bill.customerid = customerDataService.Insert(new Models.Customer()
                        {
                            name = item.customername,
                            mobileno = item.customermobileno,
                            isactive = true,
                            version = 1,
                            registredlocationid = item.locationid,
                            customertype = 4,
                            createdby = item.createdby,
                            createdon = item.createdon,
                            modifiedby = item.createdby,
                            modifiedon = item.createdon
                        }).id;

                    }
                }
                bill.additionaldiscount = item.additionaldiscount;
                bill.billdate = item.billdate;
                bill.Billitems = new List<Models.Billitems>();
                bill.billno = item.billno;
                bill.billtype = Models.Bill.BillTypes.Cash;
                bill.companyid = item.companyid;
                bill.counterid = item.counterid;
                bill.createdby = item.createdby;
                bill.createdon = item.createdon;
                bill.modifiedby = item.createdby;
                bill.modifiedon = item.createdon;
                bill.discount = item.discount;
                bill.issynchronize = true;
                bill.locationid = request.locationid;
                bill.isactive = true;
                bill.offline = true;
                bill.qty = item.Billitems.Sum(e => e.qty);
                bill.receivable = item.gross;
                bill.rounding = item.total - item.gross;
                bill.updatestock = true;
                bill.version = 1;
                bill.tax = item.Billitems.Sum(e => e.totaltax);
                bill.additionaldiscount = item.Billitems.Sum(e => e.additionaldiscount * e.qty);
                bill.discount = item.Billitems.Sum(e => e.totaldiscount) - bill.additionaldiscount;
                int index = 1;
                foreach (var bi in item.Billitems)
                {
                    Models.Billitems billitems = new Models.Billitems();
                    billitems.additionaldiscountpercentage = bi.additionaldiscountpercentage;
                    billitems.additionaldiscount = bi.additionaldiscount;
                    billitems.barcode = bi.barcode.Trim();
                    billitems.companyid = item.companyid;
                    billitems.createdby = item.createdby;
                    billitems.createdon = item.createdon;
                    billitems.modifiedby = item.createdby;
                    billitems.modifiedon = item.createdon;
                    billitems.printingname = bi.printingname;
                    billitems.discount = bi.discount;
                    billitems.isactive = true;
                    billitems.version = 1;
                    billitems.itemtax = bi.taxperitem;
                    billitems.locationid = request.locationid;
                    billitems.orderid = index++;
                    billitems.price = bi.price;
                    billitems.productid = bi.productid;
                    billitems.taxid = bi.taxid;
                    billitems.qty = bi.qty;
                    billitems.piece = bi.sellingmode == 3 ? 1 : bi.qty;
                    billitems.rate = (bi.price - bi.additionaldiscount - bi.schemediscount) - bi.taxperitem;
                    billitems.receivable = bi.receivable;
                    if (bi.discount > 0)
                    {
                        billitems.discountpercentage = Math.Round((bi.discount) / ((bi.discount) + bi.price) * 100, 2);
                    }
                    billitems.salablegoodsid = keyValues[bi.barcode.Trim()].Item1;
                    billitems.saleamount = billitems.rate * billitems.qty;
                    billitems.salerate = bi.price + (bi.discount);
                    billitems.salesmanid = bi.salesmanid;
                    billitems.stockid = keyValues[bi.barcode].Item2;
                    billitems.tax = bi.totaltax;
                    billitems.taxpercentage = bi.taxpercentage;
                    billitems.schemediscount = bi.schemediscount;
                    billitems.schemeid = bi.schemeid;
                    billitems.schemeeligble = bi.schemeeligble;
                    billitems.discount += bi.schemediscount;
                    bill.Billitems.Add(billitems);
                }
                bill.RecalculateTax();

                billlist.Add(bill);
            }

            BillData.Insert(response, billlist);
        }

        internal void SynchronizeSettlement(OfflineClient.WindowsOfflineRequest request,
            OfflineClient.WindowsOfflineResponse response)
        {
            var billdata = BufferedRealtimeCompressionEngine.Decompress(request.data);
            var settlement = Newtonsoft.Json.JsonConvert.DeserializeObject<List<OfflineClient.Settlement>>(System.Text.ASCIIEncoding.ASCII.GetString(billdata));
            Data.BillsettlementmasterDataService settlementdata = new BillsettlementmasterDataService() { Context = new UserClaim() { OrganizationID = request.organizationid } };

            var billnos = new List<string>();
            settlement.ConvertAll(e => e.Billsettlement.ConvertAll(x => x.billno)).ForEach(e =>
            {
                billnos.AddRange(e);
            });


            var conditions = billnos.ConvertAll(e => "'" + e + "'");
            string query = string.Format(@"select billno,id,companyid from bill
            where locationid = {0} and billno in ({1}) ", request.locationid, string.Join(",", conditions));
            SortedDictionary<string, Tuple<long, long>> keyValues = new SortedDictionary<string, Tuple<long, long>>();
            using (var reader = ExecuteReader(query, new SortedList<string, object>()))
            {
                while (reader.Read())
                {
                    keyValues.Add(reader.GetString(0), new Tuple<long, long>(reader.GetInt64(1), reader.GetInt64(2)));
                }
            }

            var missingbarcodes = billnos.FindAll(e => !keyValues.ContainsKey(e));
            if (missingbarcodes.Count > 0)
            {
                lock (response.lockobject)
                {
                    response.errormessage = "Missing Bill Numbers " + string.Join(",", missingbarcodes);
                    response.error = true;
                    response.completed = true;
                    response.completedon = DateTime.Now;
                }
                return;
            }

            List<Models.Billsettlementmaster> billsettlementmasters = new List<Models.Billsettlementmaster>();
            foreach (var item in settlement)
            {
                Models.Billsettlementmaster billsettlementmaster = new Models.Billsettlementmaster();
                billsettlementmaster.code = item.code;
                if (string.IsNullOrEmpty(billsettlementmaster.code)) continue;
                billsettlementmaster.paymentinfo = new Models.Billsettlementmaster.Paymentinfo();

                billsettlementmaster.paymentinfo.card_paid = item.paymentinfo.card_paid;
                billsettlementmaster.paymentinfo.adjustment_paid = item.paymentinfo.adjustment_paid;
                billsettlementmaster.paymentinfo.cash_paid = item.paymentinfo.cash_paid;
                billsettlementmaster.paymentinfo.cash_return = item.paymentinfo.cash_return;
                billsettlementmaster.paymentinfo.credit_approvedBy = item.paymentinfo.credit_approvedby;
                billsettlementmaster.paymentinfo.credit_paid = item.paymentinfo.credit_paid;
                billsettlementmaster.paymentinfo.discount_approvedBy = item.paymentinfo.discount_approvedby;
                billsettlementmaster.paymentinfo.discount_paid = item.paymentinfo.discount_paid;
                billsettlementmaster.paymentinfo.voucher_paid = item.paymentinfo.voucher_paid;

                billsettlementmaster.Billsettlement = new List<Models.Billsettlement>();
                billsettlementmaster.locationid = request.locationid;
                billsettlementmaster.counterid = item.counterid;

                billsettlementmaster.createdby = item.createdby;
                billsettlementmaster.createdon = item.createdon;
                billsettlementmaster.modifiedby = item.createdby;
                billsettlementmaster.modifiedon = item.createdon;
                billsettlementmaster.discount = item.paymentinfo.discount_paid;
                billsettlementmaster.reason = item.reason;
                billsettlementmaster.receivable = item.receivable;
                billsettlementmaster.settlementon = item.settlementon;
                billsettlementmaster.isactive = true;
                billsettlementmaster.version = 1;
                billsettlementmaster.billamount = item.receivable;
                billsettlementmaster.paidamount = item.paymentinfo.cash_paid + item.paymentinfo.card_paid;
                foreach (var sitem in item.Billsettlement)
                {
                    Models.Billsettlement billsettlement = new Models.Billsettlement();
                    billsettlement.billno = sitem.billno;
                    billsettlement.billid = keyValues[sitem.billno].Item1;
                    billsettlement.billamount = sitem.receivable;
                    billsettlement.billdate = item.settlementon;
                    billsettlement.companyid = keyValues[sitem.billno].Item2;
                    billsettlementmaster.companyid = keyValues[sitem.billno].Item2;
                    billsettlement.createdby = item.createdby;
                    billsettlement.createdon = item.createdon;
                    billsettlement.isactive = true;
                    billsettlement.IsSynchronize = true;
                    billsettlement.locationid = request.locationid;
                    billsettlement.modifiedby = item.createdby;
                    billsettlement.modifiedon = item.createdon;
                    billsettlement.paid = sitem.receivable;
                    billsettlement.version = 1;
                    billsettlementmaster.Billsettlement.Add(billsettlement);
                }
                billsettlementmasters.Add(billsettlementmaster);
            }
            settlementdata.Insert(response, billsettlementmasters);
        }




        internal void SynchronizeStock(OfflineClient.WindowsOfflineRequest request,
            OfflineClient.WindowsOfflineResponse response)
        {
            var billdata = BufferedRealtimeCompressionEngine.Decompress(request.data);
            var settlement = Newtonsoft.Json.JsonConvert.DeserializeObject<DataTable>(System.Text.ASCIIEncoding.ASCII.GetString(billdata));
            var barcodeList = new List<string>();
            foreach (DataRow dr in settlement.Rows)
            {
                barcodeList.Add(dr["barcode"].ToString());
            }

            var barcodelist = barcodeList.Distinct().ToList();
            var conditions = barcodelist.ConvertAll(e => "'" + e + "'");
            string query = string.Format(@"select salablegoods.id,stockid,barcode from salablegoods
            where locationid = {0} and barcode in ({1}) ", request.locationid, string.Join(",", conditions));
            SortedDictionary<string, Tuple<long, long>> keyValues = new SortedDictionary<string, Tuple<long, long>>();
            using (var reader = ExecuteReader(query, new SortedList<string, object>()))
            {
                while (reader.Read())
                {
                    keyValues.Add(reader.GetString(2), new Tuple<long, long>(reader.GetInt64(0), reader.GetInt64(1)));
                }
            }

            var missingbarcodes = barcodelist.FindAll(e => !keyValues.ContainsKey(e));
            if (missingbarcodes.Count > 0)
            {
                foreach (var misinglist in missingbarcodes.ChunkBy(50))
                {
                    List<InfyPOS.Models.MissingStock> syncbarcodes = new List<InfyPOS.Models.MissingStock>();
                    foreach (var bcode in misinglist)
                    {
                        MissingStock missingBarcode = new MissingStock();
                        missingBarcode.barcode = bcode;
                        var row = settlement.Select("barcode = '" + bcode + "'");
                        missingBarcode.locationid = request.locationid;
                        missingBarcode.sellingname = Convert.ToString(row[0]["productname"]);
                        missingBarcode.productname = Convert.ToString(row[0]["productname"]);
                        missingBarcode.productid = Convert.ToInt64(row[0]["productid"]);
                        missingBarcode.companyid = Convert.ToInt64(row[0]["companyid"]);
                        missingBarcode.serialno = Convert.ToString(row[0]["serialno"]);
                        missingBarcode.hsncode = Convert.ToString(row[0]["hsncode"]);
                        missingBarcode.price = Convert.ToDecimal(row[0]["price"]);
                        missingBarcode.discount = Convert.ToDecimal(row[0]["discount"]);

                        missingBarcode.version = 99;
                        syncbarcodes.Add(missingBarcode);
                    }
                    StockDataService LrinvoiceitemsData = new StockDataService()
                    {
                        Context = new UserClaim()
                        {
                            OrganizationID = request.organizationid,
                            UserID = request.userid
                        }
                    };
                    LrinvoiceitemsData.SaveMissingStock(syncbarcodes);
                }
            }

            lock (response.lockobject)
            {
                response.error = false;
                response.completed = true;
                response.completedon = DateTime.Now;
            }
            return;
        }
    }
}