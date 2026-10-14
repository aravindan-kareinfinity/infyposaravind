
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using InfyPOS.Models;
using InfyPOS.Common;
using InfyPOS.Data;
using System.Text;
using WebAPI.Data;


namespace InfyPOS.Controllers
{
    [RoutePrefix("StockService")]
    public class StockServiceController : ApiController
    {

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<Stock>();
            result.Item = new Stock();
            return this.Ok(result);
        }

        [HttpGet]
        [Route("Barcode/{id}/{id1?}/{id2?}/{id3?}")]
        public async Task<IHttpActionResult> Search(string id,
            string id1 = null, string id2 = null, string id3 = null)
        {
            StockDataService StockData = new StockDataService();
            long companyid = 0;
            if (InfyPOS.Services.ConfigManager.Instance(new BaseData().Context.OrganizationID).Config.system_supportcompanywisebarcode)
            {
                if (StockData.Context.CompanyID <= 0)
                    companyid = long.MaxValue;
                else
                    companyid = StockData.Context.CompanyID;
            }

            if (!string.IsNullOrEmpty(id1))
                id += "/" + id1;
            if (!string.IsNullOrEmpty(id2))
                id += "/" + id2;
            if (!string.IsNullOrEmpty(id3))
                id += "/" + id3;
            if (id.IndexOf("~dot~") >= 0) id = id.Replace("~dot~", ".");
            if (id.IndexOf("~sls~") >= 0) id = id.Replace("~sls~", "/");

            StockWithTax result = await Task<StockWithTax>.Run(() => StockData.Search(companyid, id));
            return this.Ok(result);
        }



        [AllowAnonymous]
        [HttpGet]
        [Route("SearchInfo/{organizationid}/{id}")]
        public async Task<IHttpActionResult> SearchInfo(string organizationid, string id)
        {
            StockDataService StockData = new StockDataService() { Context = new UserClaim() { OrganizationID = long.Parse(organizationid) } };
            long companyid = 0;
            if (InfyPOS.Services.ConfigManager.Instance(long.Parse(organizationid)).Config.system_supportcompanywisebarcode)
            {
                if (StockData.Context.CompanyID <= 0)
                    companyid = long.MaxValue;
                else
                    companyid = StockData.Context.CompanyID;
            }
            StockDataService.StockInfo result = await Task<StockDataService.StockInfo>.Run(() => StockData.SearchInfo(companyid, id));
            result.barcode = id;
            return this.Ok(result);
        }

        [HttpGet]
        [Route("Barcode/{id}")]
        public async Task<IHttpActionResult> SearchBarcode(string id)
        {
            StockDataService StockData = new StockDataService();
            long companyid = 0;
            if (InfyPOS.Services.ConfigManager.Instance(new BaseData().Context.OrganizationID).Config.system_supportcompanywisebarcode)
            {
                if (StockData.Context.CompanyID <= 0)
                    companyid = long.MaxValue;
                else
                    companyid = StockData.Context.CompanyID;
            }
            StockWithTax result = await Task<StockWithTax>.Run(() => StockData.Search(companyid, id));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Image")]
        public async Task<IHttpActionResult> UpdateImage(ActionRequest<StockImage> request)
        {
            StockDataService StockData = new StockDataService();
            StockImage result = await Task<StockImage>.Run(() => StockData.UpdateImage(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("ChangeMultiplePrice")]
        public async Task<IHttpActionResult> ChangeMultiplePrice(ActionRequest<Stock> request)
        {
            StockDataService StockData = new StockDataService();
            bool result = await Task<bool>.Run(() => StockData.ChangeMultiplePrice(request.Item));
            return this.Ok(result);
        }




        [HttpPost]
        [Route("StoreImage")]
        public async Task<IHttpActionResult> StoreImage(ActionRequest<StockImage> request)
        {
            StockDataService StockData = new StockDataService();
            string result = await Task<string>.Run(() => StockData.StoreImage(request.Item.image,
                request.Item.lrinvoiceitemid, "Inv_" + DateTime.UtcNow.Ticks));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<StockCreteria> request)
        {
            StockDataService StockData = new StockDataService();
            List<Stock> result = await Task<List<Stock>>.Run(() => StockData.Search(request.Item));
            return this.Ok(result);
        }


        [HttpPost]
        [Route("SearchStock4Sync")]
        public async Task<IHttpActionResult> SearchStock4Sync(ActionRequest<StockReportCreteria> request)
        {

            var queueManager = InfyPOS.Processors.WorkflowQueueManager.Instance;
            if (!queueManager.Enabled)
            {

                List<StockMiniReport> result = null;

                if (request.Item.brandid <= 0 && request.Item.colourid <= 0 && request.Item.materialid <= 0 &&
                    request.Item.patternid <= 0 && request.Item.productid <= 0 && request.Item.producttype <= 0 &&
                    request.Item.sizeid <= 0 && request.Item.sectionid <= 0 && request.Item.styleid <= 0 &&
                    request.Item.supplierid <= 0 && request.Item.typeid <= 0)
                {
                    return this.Ok(new { haserror = true, message = "Minimum condition missing." });
                }

                var ibarcode = request.Item.barcode;
                var ibarcodes = request.Item.barcodes;
                request.Item.barcode = null;
                request.Item.barcodes = null;
                if (request.Item.locationid <= 0)
                {
                    WarehouseReportDataService StockData = new WarehouseReportDataService();
                    result = await Task<List<StockMiniReport>>.Run(() => StockData.SearchStock(request.Item)
                    .ConvertAll(e => new StockMiniReport()
                    {
                        systemqty = e.qty,
                        barcode = e.barcode,
                        stockid = e.stockid,
                        salablegoodsid = e.salablegoodsid
                    }));
                }
                else
                {
                    RetailReportDataService retailReportDataService = new RetailReportDataService();
                    result = await Task<List<StockMiniReport>>.Run(() => retailReportDataService.SearchStock(request.Item)
                    .ConvertAll(e => new StockMiniReport()
                    {
                        systemqty = e.qty,
                        barcode = e.barcode,
                        stockid = e.stockid,
                        salablegoodsid = e.salablegoodsid
                    }));
                }


                var multcount = ibarcodes.FindAll(x => x.IndexOf(',') > 0);
                if (multcount.Count > 0)
                {
                    ibarcodes.RemoveAll(x => x.IndexOf(',') > 0);
                    foreach (var mbarcode in multcount)
                    {
                        var barcodeinfo = mbarcode.Split(',');
                        decimal count;
                        if (!decimal.TryParse(barcodeinfo[1], out count))
                            count = 1;
                        for (var i = 0; i < count; i++)
                            ibarcodes.Add(barcodeinfo[0]);
                    }
                }

                if (request.Item.removezerostock &&
                   ibarcodes != null &&
                   ibarcodes.Count > 0)
                {
                    var missingbarcodedata = ibarcodes.FindAll(x => !result.Exists(y => y.barcode == x)).Distinct().ToList();
                    if (missingbarcodedata.Count > 0)
                    {
                        request.Item.removezerostock = false;
                        request.Item.barcodes = missingbarcodedata;
                        if (request.Item.locationid <= 0)
                        {
                            WarehouseReportDataService StockData = new WarehouseReportDataService();
                            var mresult = await Task<List<StockMiniReport>>.Run(() => StockData.SearchStock(request.Item)
                            .ConvertAll(e => new StockMiniReport()
                            {
                                systemqty = e.qty,
                                barcode = e.barcode,
                                stockid = e.stockid,
                                salablegoodsid = e.salablegoodsid
                            }));
                            result.AddRange(mresult);
                        }
                        else
                        {
                            RetailReportDataService retailReportDataService = new RetailReportDataService();
                            var mresult = await Task<List<StockMiniReport>>.Run(() => retailReportDataService.SearchStock(request.Item)
                            .ConvertAll(e => new StockMiniReport()
                            {
                                systemqty = e.qty,
                                barcode = e.barcode,
                                stockid = e.stockid,
                                salablegoodsid = e.salablegoodsid
                            }));
                            result.AddRange(mresult);
                        }
                    }
                }

                var list = ibarcodes.GroupBy(barcode => barcode)
                            .Select(group => new
                            {
                                barcode = group.Key,
                                Count = group.Count()
                            }).ToList();

                var missingbarcodes = list.FindAll(e => !result.Exists(y => y.barcode == e.barcode));


                Stock4Sync stock4Sync = new Stock4Sync();
                if (missingbarcodes.Count > 0)
                {
                    stock4Sync.Missing.AddRange(missingbarcodes.ConvertAll(x => new StockMiniReport()
                    {
                        systemqty = 0,
                        physicalqty = x.Count,
                        barcode = x.barcode
                    }));
                }

                foreach (var stockitem in result)
                {
                    if (!list.Exists(e => e.barcode == stockitem.barcode))
                    {
                        stock4Sync.Missing.Add(stockitem);
                    }
                    else
                    {
                        var item = list.Find(e => e.barcode == stockitem.barcode);
                        stockitem.physicalqty = item.Count;
                        if (stockitem.systemqty == item.Count)
                        {
                            stock4Sync.Matched.Add(stockitem);
                        }
                        else if (stockitem.systemqty != item.Count)
                        {
                            stock4Sync.Additional.Add(new StockMiniReport()
                            {
                                systemqty = stockitem.systemqty,
                                physicalqty = item.Count,
                                barcode = stockitem.barcode,
                                stockid = stockitem.stockid,
                                salablegoodsid = stockitem.salablegoodsid
                            });
                        }
                    }
                }
                List<string> additionals = new List<string>();
                foreach (var istockitem in list)
                {
                    if (!result.Exists(e => e.barcode == istockitem.barcode))
                    {
                        additionals.Add(istockitem.barcode);
                    }
                }
                if (additionals.Count > 0)
                {
                    List<StockMiniReport> aresult = null;
                    if (request.Item.locationid <= 0)
                    {
                        WarehouseReportDataService StockData = new WarehouseReportDataService();
                        aresult = await Task<List<StockMiniReport>>.Run(() => StockData.SearchStock(new StockReportCreteria()
                        {
                            barcodes = additionals
                        })
                        .ConvertAll(e => new StockMiniReport()
                        {
                            systemqty = e.qty,
                            barcode = e.barcode,
                            stockid = e.stockid,
                            salablegoodsid = e.salablegoodsid
                        }));
                    }
                    else
                    {
                        RetailReportDataService retailReportDataService = new RetailReportDataService();
                        aresult = await Task<List<StockMiniReport>>.Run(() => retailReportDataService.SearchStock(new StockReportCreteria()
                        {
                            barcodes = additionals,
                            locationid = request.Item.locationid
                        })
                        .ConvertAll(e => new StockMiniReport()
                        {
                            systemqty = e.qty,
                            barcode = e.barcode,
                            stockid = e.stockid,
                            salablegoodsid = e.salablegoodsid
                        }));
                    }

                    foreach (var stockitem in aresult)
                    {
                        var item = list.Find(e => e.barcode == stockitem.barcode);
                        stockitem.physicalqty = item.Count;
                        stock4Sync.Additional.Add(stockitem);
                    }
                }
                return this.Ok(stock4Sync);
            }
            else
            {
                InboxDataService ActualDataService = new InboxDataService();
                var organizationId = ActualDataService.Context?.OrganizationID ?? 0;
                var userId = ActualDataService.Context?.UserID.ToString() ?? "";
                var response = await queueManager.ExecuteWithTimeout<object>(async () =>
                {
                    List<StockMiniReport> result = null;

                    if (request.Item.brandid <= 0 && request.Item.colourid <= 0 && request.Item.materialid <= 0 &&
                        request.Item.patternid <= 0 && request.Item.productid <= 0 && request.Item.producttype <= 0 &&
                        request.Item.sizeid <= 0 && request.Item.sectionid <= 0 && request.Item.styleid <= 0 &&
                        request.Item.supplierid <= 0 && request.Item.typeid <= 0)
                    {
                        return this.Ok(new { haserror = true, message = "Minimum condition missing." });
                    }

                    var ibarcode = request.Item.barcode;
                    var ibarcodes = request.Item.barcodes;
                    request.Item.barcode = null;
                    request.Item.barcodes = null;
                    if (request.Item.locationid <= 0)
                    {
                        WarehouseReportDataService StockData = new WarehouseReportDataService();
                        result = await Task<List<StockMiniReport>>.Run(() => StockData.SearchStock(request.Item)
                        .ConvertAll(e => new StockMiniReport()
                        {
                            systemqty = e.qty,
                            barcode = e.barcode,
                            stockid = e.stockid,
                            salablegoodsid = e.salablegoodsid
                        }));
                    }
                    else
                    {
                        RetailReportDataService retailReportDataService = new RetailReportDataService();
                        result = await Task<List<StockMiniReport>>.Run(() => retailReportDataService.SearchStock(request.Item)
                        .ConvertAll(e => new StockMiniReport()
                        {
                            systemqty = e.qty,
                            barcode = e.barcode,
                            stockid = e.stockid,
                            salablegoodsid = e.salablegoodsid
                        }));
                    }


                    var multcount = ibarcodes.FindAll(x => x.IndexOf(',') > 0);
                    if (multcount.Count > 0)
                    {
                        ibarcodes.RemoveAll(x => x.IndexOf(',') > 0);
                        foreach (var mbarcode in multcount)
                        {
                            var barcodeinfo = mbarcode.Split(',');
                            decimal count;
                            if (!decimal.TryParse(barcodeinfo[1], out count))
                                count = 1;
                            for (var i = 0; i < count; i++)
                                ibarcodes.Add(barcodeinfo[0]);
                        }
                    }

                    if (request.Item.removezerostock &&
                       ibarcodes != null &&
                       ibarcodes.Count > 0)
                    {
                        var missingbarcodedata = ibarcodes.FindAll(x => !result.Exists(y => y.barcode == x)).Distinct().ToList();
                        if (missingbarcodedata.Count > 0)
                        {
                            request.Item.removezerostock = false;
                            request.Item.barcodes = missingbarcodedata;
                            if (request.Item.locationid <= 0)
                            {
                                WarehouseReportDataService StockData = new WarehouseReportDataService();
                                var mresult = await Task<List<StockMiniReport>>.Run(() => StockData.SearchStock(request.Item)
                                .ConvertAll(e => new StockMiniReport()
                                {
                                    systemqty = e.qty,
                                    barcode = e.barcode,
                                    stockid = e.stockid,
                                    salablegoodsid = e.salablegoodsid
                                }));
                                result.AddRange(mresult);
                            }
                            else
                            {
                                RetailReportDataService retailReportDataService = new RetailReportDataService();
                                var mresult = await Task<List<StockMiniReport>>.Run(() => retailReportDataService.SearchStock(request.Item)
                                .ConvertAll(e => new StockMiniReport()
                                {
                                    systemqty = e.qty,
                                    barcode = e.barcode,
                                    stockid = e.stockid,
                                    salablegoodsid = e.salablegoodsid
                                }));
                                result.AddRange(mresult);
                            }
                        }
                    }

                    var list = ibarcodes.GroupBy(barcode => barcode)
                                .Select(group => new
                                {
                                    barcode = group.Key,
                                    Count = group.Count()
                                }).ToList();

                    var missingbarcodes = list.FindAll(e => !result.Exists(y => y.barcode == e.barcode));


                    Stock4Sync stock4Sync = new Stock4Sync();
                    if (missingbarcodes.Count > 0)
                    {
                        stock4Sync.Missing.AddRange(missingbarcodes.ConvertAll(x => new StockMiniReport()
                        {
                            systemqty = 0,
                            physicalqty = x.Count,
                            barcode = x.barcode
                        }));
                    }

                    foreach (var stockitem in result)
                    {
                        if (!list.Exists(e => e.barcode == stockitem.barcode))
                        {
                            stock4Sync.Missing.Add(stockitem);
                        }
                        else
                        {
                            var item = list.Find(e => e.barcode == stockitem.barcode);
                            stockitem.physicalqty = item.Count;
                            if (stockitem.systemqty == item.Count)
                            {
                                stock4Sync.Matched.Add(stockitem);
                            }
                            else if (stockitem.systemqty != item.Count)
                            {
                                stock4Sync.Additional.Add(new StockMiniReport()
                                {
                                    systemqty = stockitem.systemqty,
                                    physicalqty = item.Count,
                                    barcode = stockitem.barcode,
                                    stockid = stockitem.stockid,
                                    salablegoodsid = stockitem.salablegoodsid
                                });
                            }
                        }
                    }
                    List<string> additionals = new List<string>();
                    foreach (var istockitem in list)
                    {
                        if (!result.Exists(e => e.barcode == istockitem.barcode))
                        {
                            additionals.Add(istockitem.barcode);
                        }
                    }
                    if (additionals.Count > 0)
                    {
                        List<StockMiniReport> aresult = null;
                        if (request.Item.locationid <= 0)
                        {
                            WarehouseReportDataService StockData = new WarehouseReportDataService();
                            aresult = await Task<List<StockMiniReport>>.Run(() => StockData.SearchStock(new StockReportCreteria()
                            {
                                barcodes = additionals
                            })
                            .ConvertAll(e => new StockMiniReport()
                            {
                                systemqty = e.qty,
                                barcode = e.barcode,
                                stockid = e.stockid,
                                salablegoodsid = e.salablegoodsid
                            }));
                        }
                        else
                        {
                            RetailReportDataService retailReportDataService = new RetailReportDataService();
                            aresult = await Task<List<StockMiniReport>>.Run(() => retailReportDataService.SearchStock(new StockReportCreteria()
                            {
                                barcodes = additionals,
                                locationid = request.Item.locationid
                            })
                            .ConvertAll(e => new StockMiniReport()
                            {
                                systemqty = e.qty,
                                barcode = e.barcode,
                                stockid = e.stockid,
                                salablegoodsid = e.salablegoodsid
                            }));
                        }

                        foreach (var stockitem in aresult)
                        {
                            var item = list.Find(e => e.barcode == stockitem.barcode);
                            stockitem.physicalqty = item.Count;
                            stock4Sync.Additional.Add(stockitem);
                        }
                    }
                    return stock4Sync;

                }, "InboxService.Search", request.Item, 3, organizationId, userId);

                if (response.IsCompleted)
                {
                    return this.Ok(response.Result);
                }
                else
                {
                    return this.ResponseMessage(response.TimoutResponse);
                }
            }

        }

        [HttpPost]
        [Route("SearchStock")]
        public async Task<IHttpActionResult> SearchStock(ActionRequest<StockReportCreteria> request)
        {
            StockDataService StockData = new StockDataService();
            var queueManager = InfyPOS.Processors.WorkflowQueueManager.Instance;
            if (!queueManager.Enabled)
            {
                List<StockReport> result = await Task<List<StockReport>>.Run(() => StockData.SearchStock(request.Item));
                return this.Ok(result);
            }
            else
            {
                InboxDataService ActualDataService = new InboxDataService();
                var organizationId = ActualDataService.Context?.OrganizationID ?? 0;
                var userId = ActualDataService.Context?.UserID.ToString() ?? "";
                var response = await queueManager.ExecuteWithTimeout<object>(async () =>
                {
                    return await Task<List<StockReport>>.Run(() => StockData.SearchStock(request.Item));
                }, "InboxService.SearchStock", request.Item, 3, organizationId, userId);

                if (response.IsCompleted)
                {
                    return this.Ok(response.Result);
                }
                else
                {
                    return this.ResponseMessage(response.TimoutResponse);
                }
            }
        }

        [HttpPost]
        [Route("UpdateBitStock")]
        [WebAPI.InfyAuthorize(PermissionSet.stockadjustment, InfyPOS.Common.Action.Create)]
        public async Task<IHttpActionResult> UpdateBitStock(ActionRequest<StockReport> request)
        {
            StockDataService StockData = new StockDataService();
            StockReport result = await Task<StockReport>.Run(() => StockData.UpdateBitStock(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("ChangeStock")]
        [WebAPI.InfyAuthorize(PermissionSet.stockadjustment, InfyPOS.Common.Action.Create)]
        public async Task<IHttpActionResult> ChangeStock(ActionRequest<StockSync> request)
        {
            StockDataService StockData = new StockDataService();
            StockSync result = await Task<StockSync>.Run(() => StockData.ChangeStock(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("ChangeCompany")]
        [WebAPI.InfyAuthorize(PermissionSet.stockadjustment, InfyPOS.Common.Action.Create)]
        public async Task<IHttpActionResult> ChangeCompany(ActionRequest<StockSync> request)
        {
            StockDataService StockData = new StockDataService();
            StockSync result = await Task<StockSync>.Run(() => StockData.ChangeCompany(request.Item));
            return this.Ok(result);
        }


        [HttpPost]
        [Route("ChangeHSN")]
        [WebAPI.InfyAuthorize(PermissionSet.stockadjustment, InfyPOS.Common.Action.Create)]
        public async Task<IHttpActionResult> ChangeHSN(ActionMulitpleRequest<BarcodeHSN> request)
        {
            StockDataService StockData = new StockDataService();
            int result = await Task<int>.Run(() => StockData.ChangeHSN(request.Items));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("SyncStock")]
        [WebAPI.InfyAuthorize(PermissionSet.stockadjustment, InfyPOS.Common.Action.Create)]
        public async Task<IHttpActionResult> SyncStock(ActionRequest<StockSync> request)
        {
            StockDataService StockData = new StockDataService();
            var queueManager = InfyPOS.Processors.WorkflowQueueManager.Instance;
            if (!queueManager.Enabled)
            {
                StockSync result = await Task<StockSync>.Run(() => StockData.SyncStock(request.Item));
                return this.Ok(result);
            }
            else
            {
                InboxDataService ActualDataService = new InboxDataService();
                var organizationId = ActualDataService.Context?.OrganizationID ?? 0;
                var userId = ActualDataService.Context?.UserID.ToString() ?? "";
                var response = await queueManager.ExecuteWithTimeout<object>(async () =>
                {
                    return await Task<StockSync>.Run(() => StockData.SyncStock(request.Item));
                }, "InboxService.SearchStock", request.Item, 3, organizationId, userId);

                if (response.IsCompleted)
                {
                    return this.Ok(response.Result);
                }
                else
                {
                    return this.ResponseMessage(response.TimoutResponse);
                }
            }
        }


        [HttpPost]
        [Route("SearchRetailStock")]
        public async Task<IHttpActionResult> SearchRetailStock(ActionRequest<StockReportCreteria> request)
        {
            StockDataService StockData = new StockDataService();
            var queueManager = InfyPOS.Processors.WorkflowQueueManager.Instance;
            if (!queueManager.Enabled)
            {
                List<StockReport> result = await Task<List<StockReport>>.Run(() => StockData.SearchRetailStock(request.Item));
                return this.Ok(result);
            }
            else
            {
                InboxDataService ActualDataService = new InboxDataService();
                var organizationId = ActualDataService.Context?.OrganizationID ?? 0;
                var userId = ActualDataService.Context?.UserID.ToString() ?? "";
                var response = await queueManager.ExecuteWithTimeout<object>(async () =>
                {
                    return await Task<List<StockReport>>.Run(() => StockData.SearchRetailStock(request.Item));
                }, "InboxService.SearchStock", request.Item, 3, organizationId, userId);

                if (response.IsCompleted)
                {
                    return this.Ok(response.Result);
                }
                else
                {
                    return this.ResponseMessage(response.TimoutResponse);
                }
            }
        }

        [HttpPost]
        [Route("OutwardSearch")]
        public async Task<IHttpActionResult> OutwardSearch(ActionRequest<OutwardSearchCreteria> request)
        {
            StockDataService StockData = new StockDataService();
            List<OutwardStock> result = await Task<List<OutwardStock>>.Run(() => StockData.OutwardSearch(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("ItemHistory")]
        public async Task<IHttpActionResult> ItemHistory(ActionRequest<ItemSearchCreteria> request)
        {
            StockDataService StockData = new StockDataService();
            var queueManager = InfyPOS.Processors.WorkflowQueueManager.Instance;
            if (!queueManager.Enabled)
            {
                ItemSearchResult result = await Task<ItemSearchResult>.Run(() => StockData.ItemHistory(request.Item));
                return this.Ok(result);
            }
            else
            {
                InboxDataService ActualDataService = new InboxDataService();
                var organizationId = ActualDataService.Context?.OrganizationID ?? 0;
                var userId = ActualDataService.Context?.UserID.ToString() ?? "";
                var response = await queueManager.ExecuteWithTimeout<object>(async () =>
                {
                    return await Task<ItemSearchResult>.Run(() => StockData.ItemHistory(request.Item));
                }, "StockController.ItemHistory", request.Item, 3, organizationId, userId);

                if (response.IsCompleted)
                {
                    return this.Ok(response.Result);
                }
                else
                {
                    return this.ResponseMessage(response.TimoutResponse);
                }
            }
        }

        [HttpGet]
        [Route("StockHistory/{id}")]
        public async Task<IHttpActionResult> StockHistory(ActionRequest<ItemSearchCreteria> request)
        {
            StocktransactionDataService StockData = new StocktransactionDataService();
            ItemSearchResult result = await Task<ItemSearchResult>.Run(() => StockData.StockHistory(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("TransferHistory")]
        public async Task<IHttpActionResult> TransferHistory(ActionRequest<ItemSearchCreteria> request)
        {
            StockDataService StockData = new StockDataService();
            List<BundleItemTransferHistory> result = await Task<List<BundleItemTransferHistory>>.Run(() => StockData.TransferHistory(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("GenarateBarcode")]
        public async Task<IHttpActionResult> GenarateBarcode(ActionRequest<Lrinvoiceitemsbarcode> request)
        {
            try
            {
                StockDataService LrinvoiceitemsData = new StockDataService();
                Lrinvoiceitemsbarcode result = await Task<Lrinvoiceitemsbarcode>.Run(() => LrinvoiceitemsData.CreateSingleBarcode(request.Item));
                ActionSingleResponse<Lrinvoiceitemsbarcode> response = new ActionSingleResponse<Lrinvoiceitemsbarcode>(result);
                response.Success = result != null && result.stock != null && result.stock.Count > 0;
                return this.Ok(response);
            }
            catch (Exception exp)
            {
                return this.Ok(new ActionSingleResponse<Lrinvoiceitemsbarcode>()
                {
                    Success = false,
                    ErrorMessage = exp.Message
                });
            }
        }

        [HttpPost]
        [Route("CreateBarcode")]
        public async Task<IHttpActionResult> CreateBarcode(ActionRequest<MissingBarcode> request)
        {
            StockDataService LrinvoiceitemsData = new StockDataService();
            MissingBarcode result = await Task<MissingBarcode>.Run(() => LrinvoiceitemsData.CreateBarcode(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("MissingStock")]
        public async Task<IHttpActionResult> MissingStock(ActionMulitpleRequest<MissingStock> request)
        {
            StockDataService LrinvoiceitemsData = new StockDataService();
            request.Items = await Task<Lrinvoiceitemsbarcode>.Run(() => LrinvoiceitemsData.SaveMissingStock(request.Items));
            ActionMulitipleResponse<MissingStock> response = new ActionMulitipleResponse<MissingStock>(request.Items);
            response.Success = true;
            return this.Ok(response);
        }

        [HttpPost]
        [Route("MigrateStock")]
        public async Task<IHttpActionResult> MigrateStock(ActionSingleValueRequest<MigrateStock> request)
        {
            StockDataService LrinvoiceitemsData = new StockDataService();
            request.Value = await Task<MigrateStock>.Run(() => LrinvoiceitemsData.MigrateStock(request.Value));
            ActionSingleResponse<MigrateStock> response = new ActionSingleResponse<MigrateStock>(request.Value);
            response.Success = true;
            return this.Ok(response);
        }

        [HttpPost]
        [Route("GenarateBarcodeMultiple")]
        public async Task<IHttpActionResult> GenarateBarcodeMultiple(ActionMulitpleRequest<Lrinvoiceitemsbarcode> request)
        {
            StockDataService LrinvoiceitemsData = new StockDataService();
            try
            {
                List<Lrinvoiceitemsbarcode> result = await Task<List<Lrinvoiceitemsbarcode>>.Run(() => LrinvoiceitemsData.CreateBarcodefromList(request.Items));
                ActionMulitipleResponse<Lrinvoiceitemsbarcode> response = new ActionMulitipleResponse<Lrinvoiceitemsbarcode>(result);

                response.Success = true;
                return this.Ok(response);
            }
            catch (Exception exp)
            {
                return this.Ok(new ActionMulitipleResponse<Lrinvoiceitemsbarcode>()
                {
                    Success = false,
                    ErrorMessage = exp.Message
                });
            }
        }


        [HttpPost]
        [AllowAnonymous]
        [Route("BarcodeInfo")]
        public async Task<IHttpActionResult> BarcodeInfo(StockDataService.CuttingBarcode item)
        {
            StockDataService LrinvoiceitemsData = new StockDataService() { Context = new UserClaim() { OrganizationID = item.organizatinoid } };
            StockDataService.CuttingBarcode result = await Task<StockDataService.CuttingBarcode>.Run(() => LrinvoiceitemsData.BarcodeInfo(item));
            return this.Ok(result);
        }


        [HttpPost]
        [Route("PrintCuttingBarcode")]
        public async Task<IHttpActionResult> PrintCuttingBarcode(ActionRequest<StockDataService.CuttingBarcode> request)
        {
            StockDataService LrinvoiceitemsData = new StockDataService();
            StockDataService.CuttingBarcode result = await Task<StockDataService.CuttingBarcode>.Run(() => LrinvoiceitemsData.PrintCuttingBarcode(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("PrintPDFBarcode")]
        public async Task<IHttpActionResult> PrintPDFBarcode(ActionRequest<LrinvoicePrintCreteria> request)
        {
            StockDataService LrinvoiceitemsData = new StockDataService();
            byte[] result = await Task<byte[]>.Run(() => LrinvoiceitemsData.PrintPDFBarcode(request.Item));
            if (request.Item.GenerateGUID)
            {
                ImageData imageData = new ImageData() { Context = LrinvoiceitemsData.Context };
                var id = imageData.Insert(new ImageData.File()
                {
                    data = result,
                    isactive = true
                }).id.ToBase62();
                return this.Ok(id);
            }
            else
            {
                return this.Ok(Convert.ToBase64String(result));
            }
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("PrintPDFBarcode/{organizationid}/{id}")]
        public async Task<IHttpActionResult> PrintPDFBarcode(string organizationid, string id)
        {
            ImageData imageData = new ImageData() { Context = new UserClaim() { OrganizationID = long.Parse(organizationid) } };

            byte[] bytes = await Task<byte[]>.Run(() => imageData.GetData(id.LongFromBase62()).data);

            var fci = new FileContentInfo(new ByteArrayContent(bytes));
            fci.FileName = "barcode.PDF";
            fci.Extension = "application/pdf";
            return new FileActionResult(false, "barcode.pdf", fci.Extension, fci.Content);
        }


        [HttpPost]
        [Route("PrintBarcode")]
        public async Task<IHttpActionResult> PrintBarcode(ActionRequest<LrinvoicePrintCreteria> request)
        {

            try
            {

                LrinvoiceitemsDataService LrinvoiceitemsData = new LrinvoiceitemsDataService();
                if (Services.ConfigManager.Instance(LrinvoiceitemsData.Context.OrganizationID).Config.system_ismfgapplication)
                {
                    var invoiceitems = LrinvoiceitemsData.SearchAll(request.Item.invoiceid);
                    var allitems = invoiceitems.ConvertAll(x =>
                    {
                        return new Mfgdesign.Sizegroup.PrintItem()
                        {
                            uid = x.uan,
                            pqty = (request.Item.filterbarcode != null &&
                            request.Item.filterbarcode.Exists(b => b.uan == x.uan)) ? (int)request.Item.filterbarcode.Find(b => b.uan == x.uan).copy : 1
                        };
                    });

                    if (allitems.Count > 0)
                    {
                        MfgdesignServiceController mfgdesignServiceController = new MfgdesignServiceController();
                        var response = mfgdesignServiceController.PrintBarcodeDirect(new ActionRequest<MfgdesignServiceController.PrintDesignBarcode>()
                        {
                            Item = new MfgdesignServiceController.PrintDesignBarcode()
                            {
                                list = allitems,
                                reportid = request.Item.reportid,
                                disable64 = false
                            },
                        });
                        return this.Ok(response);
                    }
                }
                if (request.Item.stockids == null) request.Item.stockids = new List<long>();
                if (request.Item.items == null) request.Item.items = new List<long>();
                StockDataService stockdataservice = new StockDataService();
                if (request.Item.IsPDF)
                {
                    string resultJSON = Convert.ToBase64String(stockdataservice.PrintPDFBarcode(request.Item));
                    return this.Ok(resultJSON);
                }
                else
                {

                    InfyPOS.Data.ConfigurationDataService config = new ConfigurationDataService();
                    var reportformats = config.Search(new ConfigurationCreteria
                    {
                        id = request.Item.reportid
                    });

                    if (reportformats.Count > 0)
                    {
                        List<Stock2Print> result = await Task<List<Stock2Print>>.Run(() => stockdataservice.SearchForIds(string.Join(",", request.Item.items),
                            string.Join(",", request.Item.stockids)));

                        // Hypermarket UAN configuration doesn't have reference with Lrinvoiceitems and stock table hence.
                        if (result != null && result.Count == 0 && request.Item.items.Count == 1 
                            && InfyPOS.Services.ConfigManager.Instance(stockdataservice.Context.OrganizationID).Config.system_hyper_uan)
                        {
                            long stockid = 0;
                            long stockqty = 0;
                            var param = new SortedList<string, object>();
                            param.Add("invoiceitemid", request.Item.items[0]);
                            string query = @"select stockid from stockuan where invoiceitemid=@invoiceitemid and isactive=true order by id desc limit 1;
                                select id,sellableqty-returnqty qty from lrinvoiceitems where id=@invoiceitemid";
                            using (var reader = stockdataservice.ExecuteReader(query, param))
                            {
                                while (reader.Read())
                                {
                                    stockid = reader["stockid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["stockid"].ToString());
                                }
                                if (reader.NextResult())
                                {
                                    while (reader.Read())
                                    {
                                        stockqty = reader["qty"] == DBNull.Value ? 0 : Convert.ToInt64(reader["qty"].ToString());
                                    }
                                }
                            }
                            if (stockid > 0)
                            {
                                result = await Task<List<Stock2Print>>.Run(() => stockdataservice.SearchForIds("", stockid.ToString(), 0));
                                if (result != null && result.Count == 1)
                                    result[0].qty = stockqty;
                                else if (result != null && result.Count > 1)
                                    result.ForEach(e => e.qty = 1);
                            }
                        }

                        foreach (Stock2Print item in result)
                        {
                            SupplierDataService supplierdate = new SupplierDataService();
                            if (item.supplierid == item.sourcesupplierid || item.sourcesupplierid == 0)
                                item.sourcesuppliercode = item.suppliercode;
                            else
                                item.sourcesuppliercode = supplierdate.Search(new SupplierCreteria { id = item.sourcesupplierid }).First().code;
                        }
                        if (request.Item.filterbarcode != null && request.Item.filterbarcode.Count > 0)
                        {
                            result.RemoveAll(e => !request.Item.filterbarcode.Exists(ex => ex.code == e.barcode));
                            result.ForEach(e => e.noofcopy = request.Item.filterbarcode.Find(ex => ex.code == e.barcode).copy);
                        }

                        int index = 1;
                        foreach (var item in result)
                        {
                            item.OIndex = index++;
                        }


                        result.ForEach(e => e.Index = 1);
                        List<Stock2Print> duplicate = new List<Stock2Print>();
                        foreach (var item in result.Where(e => e.qty > 0))
                        {
                            if (item.sellingmode == 2 && item.noofcopy == 0)
                                item.noofcopy = Convert.ToInt32(item.qty - item.returnqty);

                            for (int i = 1; i < item.noofcopy; i++)
                            {
                                var citem = item.Clone();
                                citem.Index = i + 1;
                                duplicate.Add(citem);
                            }
                        }

                        if (duplicate.Count > 0)
                        {
                            result.AddRange(duplicate);
                            result = result.OrderBy(e => e.OIndex).ToList();
                        }

                        var reportformat = reportformats.First();

                        InfyPOS.Report.CitizenBarcode barcodeprinter = new Report.CitizenBarcode(request.Item.companytitle,
                            reportformat.reportformat.format, reportformat.reportformat.mode, reportformat.nooflines);


                        if (reportformat.reportformat.format.Contains("MARKER"))
                        {
                            stockdataservice.UpdateStockMarker(result);
                        }

                        if (reportformat.reportformat.format.Contains("PRDEXPIRY"))
                        {
                            stockdataservice.UpdateStockProductExpiry(result);
                        }
                        
                        if (reportformat.reportformat.format.Contains("SIZEGROUP"))
                        {
                            stockdataservice.UpdateSizeGroup(result);
                        }

                        List<string> base64Response = new List<string>();
                        StringBuilder sb = new StringBuilder();
                        StringBuilder sbplain = new StringBuilder();
                        if (request.Item.packagesize > 0)
                        {
                            PackbarcodeDataService packcodeservice = new PackbarcodeDataService();
                            packcodeservice.Context = stockdataservice.Context;
                            foreach (var barcodechunk in result.ChunkBy(request.Item.packagesize))
                            {
                                var barcodeList = barcodechunk.ConvertAll(e => (Report.IBarcodeFormat)e);
                                var response = barcodeprinter.Render(barcodeList);
                                foreach (var item in response)
                                {
                                    var bytes = Encoding.ASCII.GetBytes(item);
                                    sb.Append(Convert.ToBase64String(bytes));
                                    sb.Append(";");
                                    //base64Response.Add(Convert.ToBase64String(bytes));
                                }
                                Stock2Print stock = ((Stock2Print)barcodechunk.Last()).Clone();
                                stock.printingname = "P-" + stock.printingname;
                                Packbarcode pcode = new Packbarcode()
                                {
                                    barcode = stock.barcode,
                                    isactive = true,
                                    version = 1,
                                    barcodes = barcodechunk.ConvertAll(e => new Packbarcode.Barcodes()
                                    {
                                        barcode = e.barcode,
                                        qty = 1,
                                        pieces = 1,
                                        stockid = e.id
                                    })
                                };
                                packcodeservice.Insert(pcode, stockdataservice);
                                stock.barcode = pcode.barcode;
                                response = barcodeprinter.Render(new List<Report.IBarcodeFormat>(new Report.IBarcodeFormat[] { stock }));
                                foreach (var item in response)
                                {
                                    var bytes = Encoding.ASCII.GetBytes(item);
                                    sb.Append(Convert.ToBase64String(bytes));
                                    sb.Append(";");
                                    sbplain.Append(item + "\r\n");
                                    //base64Response.Add(Convert.ToBase64String(bytes));
                                }
                            }
                        }
                        else
                        {
                            var barcodeList = result.ConvertAll(e => (Report.IBarcodeFormat)e);
                            var response = barcodeprinter.Render(barcodeList);
                            foreach (var item in response)
                            {
                                var bytes = Encoding.ASCII.GetBytes(item);
                                sb.Append(Convert.ToBase64String(bytes));
                                sb.Append(";");
                                sbplain.Append(item + "\r\n");
                                //base64Response.Add(Convert.ToBase64String(bytes));
                            }
                        }
                        base64Response.Add(sb.ToString());


                        if (InfyPOS.Services.ConfigManager.Instance(new BaseData().Context.OrganizationID).Config.system_barcodeonceprint)
                        {
                            stockdataservice.UpdateBarcodePrintCount(result);
                        }
                        return this.Ok(base64Response);
                    }
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(new BaseData().Context.OrganizationID).Log("Print Barcode", WebAPI.Common.Logger.LogType.Error, exp.Message);
                WebAPI.Common.Logger.Instance(new BaseData().Context.OrganizationID).Log("Print Barcode", WebAPI.Common.Logger.LogType.Error, exp.StackTrace);
                return this.Ok(new ActionSingleResponse<LrinvoicePrintCreteria>()
                {
                    Success = false,
                    ErrorMessage = exp.Message
                });
            }

            return this.NotFound();
        }


        [HttpPost]
        [Route("ResetBarcodePrintCount")]
        public async Task<IHttpActionResult> ResetBarcodePrintCount(ActionMulitpleRequest<LRInvoiceItemBarcode> request)
        {
            StockDataService LrinvoiceitemsData = new StockDataService();
            bool result = await Task<bool>.Run(() => LrinvoiceitemsData.ResetBarcodePrintCount(request.Items));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("BillBarcodeList")]
        public async Task<IHttpActionResult> BillBarcodeList(ActionRequest<LrinvoicePrintCreteria> request)
        {


            if (request.Item.stockids == null) request.Item.stockids = new List<long>();
            if (request.Item.items == null) request.Item.items = new List<long>();


            BillitemsDataService billDataService = new BillitemsDataService();
            List<Billitems> billitems = billDataService.Search(new BillitemsCreteria()
            {
                billids = request.Item.billids
            }).OrderBy(x => x.billid).ThenBy(y => y.id).ToList();

            MfgdesignDataService mfgdesignDataService = new MfgdesignDataService();
            var alldesigns = mfgdesignDataService.Search(new MfgdesignCreteria()
            {
                codes = billitems.GroupBy(x => x.attributes.designid).ToList().ConvertAll(x => x.Key).Distinct().ToList(),
                includedetail = true
            });


            ReferencelistDataService referencelistDataService = new ReferencelistDataService();
            ProductsDataService productsDataService = new ProductsDataService();
            BrandDataService brandDataService = new BrandDataService();

            var colournames = referencelistDataService.SearchList(new ReferencelistCreteria()
            {
                type = "COLOUR"
            });

            var sizenames = referencelistDataService.SearchList(new ReferencelistCreteria()
            {
                type = "SIZE"
            });


            var patternnames = referencelistDataService.SearchList(new ReferencelistCreteria()
            {
                type = "PATTERN"
            });


            var materialnames = referencelistDataService.SearchList(new ReferencelistCreteria()
            {
                type = "MATERIAL"
            });

            var typenames = referencelistDataService.SearchList(new ReferencelistCreteria()
            {
                type = "TYPE"
            });

            var stylenames = referencelistDataService.SearchList(new ReferencelistCreteria()
            {
                type = "STYLE"
            });

            var sleevelist = referencelistDataService.SearchList(new ReferencelistCreteria()
            {
                type = "SLEEVE"
            });

            var fitlist = referencelistDataService.SearchList(new ReferencelistCreteria()
            {
                type = "FIT"
            });


            List<Design2Print> barcodeList = new List<Design2Print>();

            foreach (var design in alldesigns)
            {
                var productnames = productsDataService.Search(new ProductsCreteria()
                {
                    id = design.productid
                });
                var brandnames = brandDataService.Search(new BrandCreteria()
                {
                    id = design.brandid
                });
                foreach (var pitem in billitems.Where(x => x.attributes.designid == design.code).GroupBy(x => new
                {
                    x.attributes.serialno,
                    x.barcode
                }))
                {
                    string colourcode = "";
                    var idx = design.sizegroup.items.FindIndex(x => x.uid == pitem.Key.barcode);
                    if (idx < 0)
                    {
                        idx = design.sizegroup.items.FindIndex(x => x.uid == pitem.Key.serialno);
                        if (idx < 0)
                            continue;
                    }
                    var item = design.sizegroup.items[idx];

                    if (item.name.IndexOf('/') > 0)
                        colourcode = item.code.Substring(item.code.IndexOf('/') + 1);

                    foreach (var bitem in pitem)
                    {
                        Design2Print design2Print = new Design2Print();
                        design2Print.billid = bitem.billid;
                        design2Print.groupkey = bitem.attributes.groupkey;
                        design2Print.brand = brandnames.Count > 0 ? brandnames[0].name : "";
                        design2Print.product = productnames.Count > 0 ? productnames[0].name : "";
                        design2Print.colour = colournames.Exists(e => e.id == item.colourid) ? colournames.Find(e => e.id == item.colourid).name : "";
                        design2Print.colourcode = colournames.Exists(e => e.id == item.colourid) ? colournames.Find(e => e.id == item.colourid).code : "";
                        design2Print.size = sizenames.Exists(e => e.id == item.sizeid) ? sizenames.Find(e => e.id == item.sizeid).name : "";
                        design2Print.sizecode = sizenames.Exists(e => e.id == item.sizeid) ? sizenames.Find(e => e.id == item.sizeid).code : "";

                        design2Print.fitname = fitlist.Exists(e => e.id == design.fitid) ? fitlist.Find(e => e.id == design.fitid).name : "";
                        design2Print.fitcode = fitlist.Exists(e => e.id == design.fitid) ? fitlist.Find(e => e.id == design.fitid).code : "";

                        design2Print.sleevename = sleevelist.Exists(e => e.id == item.sleeveid) ? sleevelist.Find(e => e.id == item.sleeveid).name : "";
                        design2Print.sleevecode = sleevelist.Exists(e => e.id == item.sleeveid) ? sleevelist.Find(e => e.id == item.sleeveid).code : "";

                        design2Print.materialname = materialnames.Exists(e => e.id == design.materialid) ? materialnames.Find(e => e.id == design.materialid).name : "";
                        design2Print.materialcode = materialnames.Exists(e => e.id == design.materialid) ? materialnames.Find(e => e.id == design.materialid).code : "";

                        design2Print.typename = typenames.Exists(e => e.id == design.typeid) ? typenames.Find(e => e.id == design.typeid).name : "";
                        design2Print.typecode = typenames.Exists(e => e.id == design.typeid) ? typenames.Find(e => e.id == design.typeid).code : "";

                        design2Print.patternname = patternnames.Exists(e => e.id == design.patternid) ? patternnames.Find(e => e.id == design.patternid).name : "";
                        design2Print.patterncode = patternnames.Exists(e => e.id == design.patternid) ? patternnames.Find(e => e.id == design.patternid).code : "";

                        design2Print.stylename = stylenames.Exists(e => e.id == design.styleid) ? stylenames.Find(e => e.id == design.styleid).name : "";
                        design2Print.stylecode = stylenames.Exists(e => e.id == design.styleid) ? stylenames.Find(e => e.id == design.styleid).code : "";

                        design2Print.colourshade = colourcode;
                        design2Print.design = design.name;
                        design2Print.designcode = item.code;
                        design2Print.id = design.id;
                        design2Print.uid = item.uid;
                        design2Print.name = item.code;
                        design2Print.colourid = item.colourid;
                        design2Print.sizeid = item.sizeid;
                        design2Print.sizegroup = item.sizegroup;
                        design2Print.salerate = item.salerate;
                        design2Print.mrp = item.mrp;
                        design2Print.wsrate = item.wsrate;
                        design2Print.dealerprice = item.dealerprice;
                        design2Print.qty = Convert.ToInt32(bitem.qty);
                        barcodeList.Add(design2Print);
                    }
                }
            }
            barcodeList = barcodeList.OrderBy(x => (x as Design2Print).billid).ThenBy(y => (y as Design2Print).groupkey).ToList();
            return this.Ok(barcodeList);

        }


        [HttpPost]
        [Route("PrintBillBarcode")]
        public async Task<IHttpActionResult> PrintBillBarcode(ActionRequest<LrinvoicePrintCreteria> request)
        {


            if (request.Item.stockids == null) request.Item.stockids = new List<long>();
            if (request.Item.items == null) request.Item.items = new List<long>();

            InfyPOS.Data.ConfigurationDataService config = new ConfigurationDataService();
            var reportformats = config.Search(new ConfigurationCreteria
            {
                id = request.Item.reportid
            });

            if (reportformats.Count > 0 && request.Item.barcodes != null && request.Item.barcodes.Count > 0)
            {
                List<string> base64Response = new List<string>();
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                System.Text.StringBuilder asb = new System.Text.StringBuilder();

                InfyPOS.Report.CitizenBarcode barcodeprinter = new Report.CitizenBarcode(request.Item.companytitle,
                    reportformats[0].reportformat.format, reportformats[0].reportformat.mode, reportformats[0].nooflines);

                List<Report.IBarcodeFormat> barcodeList = new List<Report.IBarcodeFormat>();
                foreach (var bcode in request.Item.barcodes)
                {
                    for (var i = 0; i < bcode.qty; i++)
                    {
                        barcodeList.Add(bcode);
                    }
                }
                barcodeList = barcodeList.OrderBy(x => (x as Design2Print).billid).ThenBy(y => (y as Design2Print).groupkey).ToList();
                var response = barcodeprinter.Render(barcodeList.ConvertAll(x => x as Report.IBarcodeFormat));
                foreach (var item in response)
                {
                    asb.AppendLine(item);
                    asb.Append(";");
                    var bytes = System.Text.Encoding.ASCII.GetBytes(item);
                    sb.Append(Convert.ToBase64String(bytes));
                    sb.Append(";");
                }
                base64Response.Add(sb.ToString());
                return this.Ok(base64Response);
            }
            else if (reportformats.Count > 0)
            {
                BillitemsDataService billDataService = new BillitemsDataService();
                List<Billitems> billitems = billDataService.Search(new BillitemsCreteria()
                {
                    billids = request.Item.billids
                }).OrderBy(x => x.billid).ThenBy(y => y.id).ToList();

                MfgdesignDataService mfgdesignDataService = new MfgdesignDataService();
                var alldesigns = mfgdesignDataService.Search(new MfgdesignCreteria()
                {
                    codes = billitems.GroupBy(x => x.attributes.designid).ToList().ConvertAll(x => x.Key).Distinct().ToList(),
                    includedetail = true
                });



                InfyPOS.Report.CitizenBarcode barcodeprinter = new Report.CitizenBarcode(request.Item.companytitle,
                    reportformats[0].reportformat.format, reportformats[0].reportformat.mode, reportformats[0].nooflines);
                ReferencelistDataService referencelistDataService = new ReferencelistDataService();
                ProductsDataService productsDataService = new ProductsDataService();
                BrandDataService brandDataService = new BrandDataService();

                var colournames = referencelistDataService.SearchList(new ReferencelistCreteria()
                {
                    type = "COLOUR"
                });

                var sizenames = referencelistDataService.SearchList(new ReferencelistCreteria()
                {
                    type = "SIZE"
                });

                var patternnames = referencelistDataService.SearchList(new ReferencelistCreteria()
                {
                    type = "PATTERN"
                });


                var materialnames = referencelistDataService.SearchList(new ReferencelistCreteria()
                {
                    type = "MATERIAL"
                });

                var typenames = referencelistDataService.SearchList(new ReferencelistCreteria()
                {
                    type = "TYPE"
                });

                var stylenames = referencelistDataService.SearchList(new ReferencelistCreteria()
                {
                    type = "STYLE"
                });

                var sleevelist = referencelistDataService.SearchList(new ReferencelistCreteria()
                {
                    type = "SLEEVE"
                });

                var fitlist = referencelistDataService.SearchList(new ReferencelistCreteria()
                {
                    type = "FIT"
                });

                List<string> base64Response = new List<string>();
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                System.Text.StringBuilder asb = new System.Text.StringBuilder();
                var barcodeList = new List<Report.IBarcodeFormat>();

                foreach (var design in alldesigns)
                {
                    var productnames = productsDataService.Search(new ProductsCreteria()
                    {
                        id = design.productid
                    });
                    var brandnames = brandDataService.Search(new BrandCreteria()
                    {
                        id = design.brandid
                    });
                    foreach (var pitem in billitems.Where(x => x.attributes.designid == design.code).GroupBy(x => new { x.attributes.serialno, x.barcode }))
                    {
                        string colourcode = "";
                        var idx = design.sizegroup.items.FindIndex(x => x.uid == pitem.Key.barcode);
                        if (idx < 0)
                        {
                            idx = design.sizegroup.items.FindIndex(x => x.uid == pitem.Key.serialno);
                            if (idx < 0)
                                continue;
                        }
                        var item = design.sizegroup.items[idx];

                        if (item.name.IndexOf('/') > 0)
                            colourcode = item.code.Substring(item.code.IndexOf('/') + 1);

                        foreach (var bitem in pitem)
                        {
                            for (int i = 0; i < bitem.qty; i++)
                            {
                                Design2Print design2Print = new Design2Print();
                                design2Print.billid = bitem.billid;
                                design2Print.groupkey = bitem.attributes.groupkey;
                                design2Print.brand = brandnames.Count > 0 ? brandnames[0].name : "";
                                design2Print.product = productnames.Count > 0 ? productnames[0].name : "";
                                design2Print.colour = colournames.Exists(e => e.id == item.colourid) ? colournames.Find(e => e.id == item.colourid).name : "";
                                design2Print.colourcode = colournames.Exists(e => e.id == item.colourid) ? colournames.Find(e => e.id == item.colourid).code : "";
                                design2Print.size = sizenames.Exists(e => e.id == item.sizeid) ? sizenames.Find(e => e.id == item.sizeid).name : "";
                                design2Print.sizecode = sizenames.Exists(e => e.id == item.sizeid) ? sizenames.Find(e => e.id == item.sizeid).code : "";
                                design2Print.fitname = fitlist.Exists(e => e.id == design.fitid) ? fitlist.Find(e => e.id == design.fitid).name : "";
                                design2Print.fitcode = fitlist.Exists(e => e.id == design.fitid) ? fitlist.Find(e => e.id == design.fitid).code : "";

                                design2Print.sleevename = sleevelist.Exists(e => e.id == item.sleeveid) ? sleevelist.Find(e => e.id == item.sleeveid).name : "";
                                design2Print.sleevecode = sleevelist.Exists(e => e.id == item.sleeveid) ? sleevelist.Find(e => e.id == item.sleeveid).code : "";

                                design2Print.materialname = materialnames.Exists(e => e.id == design.materialid) ? materialnames.Find(e => e.id == design.materialid).name : "";
                                design2Print.materialcode = materialnames.Exists(e => e.id == design.materialid) ? materialnames.Find(e => e.id == design.materialid).code : "";

                                design2Print.typename = typenames.Exists(e => e.id == design.typeid) ? typenames.Find(e => e.id == design.typeid).name : "";
                                design2Print.typecode = typenames.Exists(e => e.id == design.typeid) ? typenames.Find(e => e.id == design.typeid).code : "";

                                design2Print.patternname = patternnames.Exists(e => e.id == design.patternid) ? patternnames.Find(e => e.id == design.patternid).name : "";
                                design2Print.patterncode = patternnames.Exists(e => e.id == design.patternid) ? patternnames.Find(e => e.id == design.patternid).code : "";

                                design2Print.stylename = stylenames.Exists(e => e.id == design.styleid) ? stylenames.Find(e => e.id == design.styleid).name : "";
                                design2Print.stylecode = stylenames.Exists(e => e.id == design.styleid) ? stylenames.Find(e => e.id == design.styleid).code : "";


                                design2Print.colourshade = colourcode;
                                design2Print.design = design.name;
                                design2Print.designcode = item.code;
                                design2Print.id = design.id;
                                design2Print.uid = item.uid;
                                design2Print.name = item.code;
                                design2Print.colourid = item.colourid;
                                design2Print.sizeid = item.sizeid;
                                design2Print.sizegroup = item.sizegroup;
                                design2Print.salerate = item.salerate;
                                design2Print.mrp = item.mrp;
                                design2Print.wsrate = item.wsrate;
                                design2Print.dealerprice = item.dealerprice;
                                barcodeList.Add(design2Print);
                            }
                        }
                    }
                }
                barcodeList = barcodeList.OrderBy(x => (x as Design2Print).billid).ThenBy(y => (y as Design2Print).groupkey).ToList();
                var response = barcodeprinter.Render(barcodeList);
                foreach (var item in response)
                {
                    asb.AppendLine(item);
                    asb.Append(";");
                    var bytes = System.Text.Encoding.ASCII.GetBytes(item);
                    sb.Append(Convert.ToBase64String(bytes));
                    sb.Append(";");
                    //base64Response.Add(Convert.ToBase64String(bytes));
                }

                base64Response.Add(sb.ToString());
                return this.Ok(base64Response);


            }

            return this.NotFound();
        }

        [HttpPost]
        [Route("Save")]
        public async Task<IHttpActionResult> Save(ActionRequest<Stock> request)
        {
            StockDataService StockData = new StockDataService();
            if (request.Item.id <= 0)
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Create);
                Stock result = await Task<Stock>.Run(() => StockData.Insert(request.Item));
                return this.Ok(result);
            }
            else
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Update);
                Stock result = await Task<Stock>.Run(() => StockData.Update(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("Insert")]
        public async Task<IHttpActionResult> Insert(ActionRequest<Stock> request)
        {
            StockDataService StockData = new StockDataService();
            Stock result = await Task<Stock>.Run(() => StockData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        public async Task<IHttpActionResult> Update(ActionRequest<Stock> request)
        {
            StockDataService StockData = new StockDataService();
            Stock result = await Task<Stock>.Run(() => StockData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IHttpActionResult> Delete(ActionRequest<Stock> request)
        {
            StockDataService StockData = new StockDataService();
            bool result = await Task<bool>.Run(() => StockData.Delete(request.Item));
            return this.Ok(result);
        }

        public void ListMissmatchStock()
        {
            string query = @"
with allsales
as
(
select sum(billitems.qty) as sold,billitems.barcode from billitems
inner join bill on bill.id = billid
	
	group by billitems.barcode 
),
retailstock
as
(
	select salablegoods.barcode,sum(salablegoods.iqty-(salablegoods.oqty+salablegoods.jqty+salablegoods.dqty)) retstock,
    sum(salablegoods.oqty) sold,sum(jqty) journal,sum(dqty) damaged from salablegoods
	where salablegoods.hasstock = true 
	group by salablegoods.barcode
),
allwsstock
as
(
select 
sum(stock.qty - (stock.returnqty+stock.damagedqty+stock.journalqty)) as purchased,
sum(stock.damagedqty) damaged,sum(stock.journalqty) journal,
sum(stock.qty - (stock.transfered+stock.returnqty+stock.damagedqty+stock.journalqty)) as wsstock,barcode from stock
group by barcode
),
stockchecker
as
(
select allwsstock.barcode,purchased,coalesce(allsales.sold,0) as sold,retstock+wsstock closingstock,
purchased-coalesce(allsales.sold,0) as computedstock,retailstock.damaged rdamaged,allwsstock.damaged as sdamaged
from allwsstock
inner join retailstock on retailstock.barcode = allwsstock.barcode
left outer join allsales on allsales.barcode = allwsstock.barcode
)

select stockchecker.barcode,purchased,sold,computedstock,closingstock,sdamaged,rdamaged ,salablegoods.id,
(salablegoods.iqty-(salablegoods.oqty+salablegoods.jqty+salablegoods.dqty)) as retstock
from stockchecker 
inner join salablegoods on salablegoods.barcode = stockchecker.barcode
where computedstock != closingstock";
        }
    }
}