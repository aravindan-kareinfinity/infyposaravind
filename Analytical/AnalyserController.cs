
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

namespace InfyPOS.Controllers
{
    [RoutePrefix("AnalyserService")]
    public class AnalyserServiceController : ApiController
    {

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<AnalyserDataService.Configuration>();
            result.Item = new AnalyserDataService.Configuration();
            return this.Ok(result);
        }

        //[AllowAnonymous]
        //[HttpGet]
        //[Route("Purchase/{type}/{id}")]
        //public async Task<IHttpActionResult> PurchaseOrder(string type, string id)
        //{
        //    InfyPOS.Analytical.PurchaseAnalyser analyserData = new InfyPOS.Analytical.PurchaseAnalyser();
        //    if (type == "lrinbox")
        //    {
        //        Analytical.PurchaseAnalyser.InsightInfo result = await Task<Analytical.PurchaseAnalyser.InsightInfo>.Run(() =>
        //            analyserData.LrEntry(long.Parse(id)));
        //        return this.Ok(result);
        //    }
        //    else
        //    {
        //        Analytical.PurchaseAnalyser.InsightInfo result = await Task<Analytical.PurchaseAnalyser.InsightInfo>.Run(() =>
        //            analyserData.PurchaseOrder(long.Parse(id)));
        //        return this.Ok(result);
        //    }
        //}

        [HttpPost]
        [Route("SupplierPendingInvoices")]
        public async Task<IHttpActionResult> SupplierPendingInvoices(ActionRequest<Analytical.MobileInsight.SummaryRequst> request)
        {
            SupplierpaymentDataService SupplierpaymentData = new SupplierpaymentDataService();
            List<PendingLrInvoice> result = await Task<List<PendingLrInvoice>>.Run(() => SupplierpaymentData.SearchPendingPayment(new SupplierpaymentCreteria()
            {
                supplierid = request.Item.supplierid,
                entryfromdate = SupplierpaymentData.DateTimeNow.AddMonths(-6)
            }));
            var response = result.ConvertAll(e => new Analytical.MobileInsight.InvoicePendingItem()
            {
                date = e.invoicedate,
                no = e.invoiceno,
                value = e.totalamount - e.paidamount,
                isinvoice = e.isinvoice,
                soldpercent = e.soldpercent,
                invoiceid = e.lrinvoiceid
            }).OrderByDescending(e => e.date).ToList();
            return this.Ok(response);
        }
        [HttpPost]
        [Route("FinanceSummary")]
        public async Task<IHttpActionResult> BuildFinanceSummary(ActionRequest<Analytical.MobileInsight.SummaryRequst> request)
        {
            InfyPOS.Analytical.MobileInsight analyserData = new InfyPOS.Analytical.MobileInsight();
            InfyPOS.Analytical.MobileInsight.FinanceSummary result = await Task<InfyPOS.Analytical.MobileInsight.FinanceSummary>.Run(() =>
                                analyserData.BuildFinanceSummary(request.Item));
            return this.Ok(result);
        }
        [HttpPost]
        [Route("DataSummary")]
        public async Task<IHttpActionResult> DataSummary(ActionRequest<Analytical.MobileInsight.SummaryRequst> request)
        {
            //if (request.Item.attribute == "supplier" && request.Item.supplierid == 0)
            //{
            //    request.Item.supplierid = request.Item.locationid;
            //    request.Item.locationid = 0;
            //}
            //if (request.Item.attribute == "product" && request.Item.productid == 0)
            //{
            //    request.Item.productid = request.Item.locationid;
            //    request.Item.locationid = 0;
            //}
            //if (request.Item.attribute == "brand" && request.Item.productid == 0)
            //{
            //    request.Item.productid = request.Item.locationid;
            //    request.Item.locationid = 0;
            //}


            InfyPOS.Analytical.MobileInsight analyserData = new InfyPOS.Analytical.MobileInsight();
            if (request.Item.requestType == Analytical.MobileInsight.SummaryRequst.RequestType.Monthly)
            {
                InfyPOS.Analytical.MobileInsight.MonthlySummary result = await Task<InfyPOS.Analytical.MobileInsight.MonthlySummary>.Run(() =>
                    analyserData.BuildMonthlySummary(request.Item));
                if (result.items == null || result.items.Count == 0)
                {
                    request.Item.month--;
                    if (request.Item.month == 0)
                        request.Item.year--;
                    result = await Task<InfyPOS.Analytical.MobileInsight.MonthlySummary>.Run(() =>
                    analyserData.BuildMonthlySummary(request.Item));
                }
                result.description = new DateTime(request.Item.year, request.Item.month, 1).ToString("MMM-yyyy");
                result.totalqty = result.items.Sum(e => e.qty);
                result.totalamount = result.items.Sum(e => e.amount);
                result.totalpqty = result.items.Sum(e => e.pqty);
                result.totalpamount = result.items.Sum(e => e.pamount);
                result.items = result.items.OrderByDescending(e => e.amount).ToList();
                return this.Ok(result);
            }
            else
            {

                if (request.Item.todate.HasValue)
                {
                    request.Item.date = request.Item.todate.Value;
                }
                else
                {
                    request.Item.date = new DateTime(request.Item.year, request.Item.month, 1);
                }
                DateTime fdate = request.Item.date;
                InfyPOS.Analytical.MobileInsight.DailySummary result = await Task<InfyPOS.Analytical.MobileInsight.DailySummary>.Run(() =>
                    analyserData.BuildDailySummary(request.Item));


                result.currentyear = request.Item.date.ToString("MMM-yyyy");
                request.Item.year -= 1;
                var lresult = new InfyPOS.Analytical.MobileInsight.DailySummary();
                var tdate = request.Item.date;
                if (request.Item.fromdate.HasValue)
                {
                    request.Item.date = request.Item.fromdate.Value;
                    lresult = await Task<InfyPOS.Analytical.MobileInsight.DailySummary>.Run(() => analyserData.BuildDailySummary(request.Item));
                }
                else
                {
                    request.Item.date = new DateTime(request.Item.year, request.Item.month, 1);
                    lresult = await Task<InfyPOS.Analytical.MobileInsight.DailySummary>.Run(() => analyserData.BuildDailySummary(request.Item));
                    if (lresult.items.Count == 0 &&
                        !request.Item.fromdate.HasValue)
                    {
                        request.Item.year += 1;
                        request.Item.month -= 1;
                        if (request.Item.month == 0)
                        {
                            request.Item.month = 12;
                            request.Item.year -= 1;
                        }
                        request.Item.date = new DateTime(request.Item.year, request.Item.month, 1);
                        lresult = await Task<InfyPOS.Analytical.MobileInsight.DailySummary>.Run(() => analyserData.BuildDailySummary(request.Item));
                        result.lastyear = new DateTime(request.Item.year, request.Item.month, 1).ToString("MMM-yyyy");
                    }
                }
                tdate = request.Item.date;
                result.lastyear = request.Item.date.ToString("MMM-yyyy");
                /*lastyear = new DateTime(request.Item.year, request.Item.month, 1);
                result.items.ForEach(e => e.currentyear = e.amount);
                lresult.items.ForEach(e =>
                {
                    if (result.items.Exists(x => x.day == e.day))
                        result.items.Find(x => x.day == e.day).lastyear = e.amount;
                    else
                        result.items.Add(new Analytical.MobileInsight.DailySaleData()
                        {
                            day = e.day,
                            lastyear = e.amount
                        });
                });*/

                if (result.items == null)
                    result.items = new List<Analytical.MobileInsight.DailySaleData>();
                result.items.ForEach(x =>
                {
                    x.cdate = x.date;
                    x.currentyear = x.amount;
                });
                if (lresult.items == null)
                    lresult.items = new List<Analytical.MobileInsight.DailySaleData>();
                lresult.items.ForEach(x => x.ldate = x.date);

                fdate = fdate.Date;
                tdate = tdate.Date;


                for (var i = 0; i < 31; i++)
                {
                    if (!result.items.Exists(x => x.cdate == fdate))
                    {
                        result.items.Add(new Analytical.MobileInsight.DailySaleData()
                        {
                            cdate = fdate
                        });
                    }
                    result.items.Find(x => x.cdate == fdate).ldate = tdate;
                    if (lresult.items.Exists(x => x.date == tdate))
                    {
                        result.items.Find(x => x.cdate == fdate).lastyear = lresult.items.Find(x => x.date == tdate).amount;
                    }
                    fdate = fdate.AddDays(1);
                    tdate = tdate.AddDays(1);
                }

                result.items.ForEach(e =>
                {
                    e.currentyearday = e.cdate.DayOfWeek;
                    e.lastyearday = e.ldate.DayOfWeek;
                    e.cday = e.cdate.Day;
                    e.lday = e.ldate.Day;
                    if (e.currentyear > 0 && e.lastyear > 0)
                    {
                        e.result = Math.Round((e.currentyear / e.lastyear * 100) - 100, 0);
                    }
                });

                //result.items.ForEach(e =>
                //{
                //    e.currentyearday = currentyear.AddDays(e.day - 1).DayOfWeek;
                //    e.lastyearday = lastyear.AddDays(e.day - 1).DayOfWeek;
                //});

                result.items.Sort(delegate (Analytical.MobileInsight.DailySaleData a, Analytical.MobileInsight.DailySaleData b)
                {
                    return a.cdate.CompareTo(b.cdate);
                });



                result.lastyeartotal = result.items.Sum(e => e.lastyear);
                result.currentyeartotal = result.items.Sum(e => e.currentyear);

                if (result.currentyeartotal > 0 && result.lastyeartotal > 0)
                    result.result = Math.Round((result.currentyeartotal / result.lastyeartotal * 100) - 100, 0);

                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("SearchIntray")]
        public async Task<IHttpActionResult> SearchIntray(ActionRequest<IntrayCreteria> request)
        {
            IntrayDataService IntrayData = new IntrayDataService();
            List<InfyPOS.Analytical.MobileInsight.IntrayItem> result = await Task<List<Intray>>.Run(() =>
            {
                var resultitems = IntrayData.Search(request.Item).ConvertAll(e => new InfyPOS.Analytical.MobileInsight.IntrayItem()
                {
                    can = !(e.referencetype == 4 || e.referencetype == 5 || e.referencetype == 7),
                    date = e.actionon,
                    description = e.detail != null ? e.detail.description : "",
                    typename = e.typename,
                    status = e.ActionType.ToString(),
                    durl = e.ReferenceType != Services.IntrayManager.EntryType.PurchaseOrder ? "" :
                     "PurchaseorderService/PrintPDF/" + IntrayData.Context.OrganizationID + "/" + e.referenceid,
                    id = e.id
                });
                return resultitems;
            });
            return this.Ok(result);
        }

        [HttpPost]
        [Route("SearchCreditHistory")]
        public async Task<IHttpActionResult> SearchCreditHistory(ActionRequest<RetailReportCreteria> request)
        {
            RetailReportDataService IntrayData = new RetailReportDataService();
            List<InfyPOS.Data.CreditHistory> result = await Task<List<InfyPOS.Data.CreditHistory>>.Run(() =>
            {
                return IntrayData.SearchCreditHistory(request.Item);
            });
            return this.Ok(result);
        }

        [HttpPost]
        [Route("DesignView")]
        public async Task<IHttpActionResult> DesignView(ActionRequest<Analytical.MobileInsight.DetailShowRequest> request)
        {
            InfyPOS.Analytical.MobileInsight analyserData = new InfyPOS.Analytical.MobileInsight();
            List<Analytical.MobileInsight.DesignShowResponse> result = await Task<List<Analytical.MobileInsight.DesignShowResponse>>.Run(() => analyserData.DesignView(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("DetailView")]
        public async Task<IHttpActionResult> DetailView(ActionRequest<Analytical.MobileInsight.DetailShowRequest> request)
        {
            InfyPOS.Analytical.MobileInsight analyserData = new InfyPOS.Analytical.MobileInsight();
            Analytical.MobileInsight.DetailShowResponse result = await Task<Analytical.MobileInsight.DetailShowResponse>.Run(() => analyserData.DetailView(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("SaleView")]
        public async Task<IHttpActionResult> SaleView(ActionRequest<Analytical.MobileInsight.SaleView_Request> request)
        {
            InfyPOS.Analytical.MobileInsight analyserData = new InfyPOS.Analytical.MobileInsight();
            List<Analytical.MobileInsight.SaleView_Response> result = await Task<List<Analytical.MobileInsight.SaleView_Response>>.Run(() =>
                request.Item.mode == "PRODUCT" ? analyserData.SaleView_Product(request.Item) : analyserData.SaleView_Supplier(request.Item));
            return this.Ok(result);
        }


        [HttpPost]
        [Route("StoreViewEOD")]
        public async Task<IHttpActionResult> StoreViewEOD(ActionRequest<Analytical.MobileInsight.StoreViewEOD_Request> request)
        {
            InfyPOS.Analytical.MobileInsight analyserData = new InfyPOS.Analytical.MobileInsight();
            Analytical.MobileInsight.StoreViewEOD result = await Task<Analytical.MobileInsight.StoreViewEOD>.Run(() =>
                analyserData.GetStoreViewEOD(request.Item));
            return this.Ok(result);
        }
        //[AllowAnonymous]
        [HttpGet]
        [Route("LocationSummary")]
        public async Task<IHttpActionResult> LocationSummary()
        {
            InfyPOS.Analytical.MobileInsight analyserData = new InfyPOS.Analytical.MobileInsight();
            InfyPOS.Analytical.MobileInsight.LocationItemSummary result = await Task<InfyPOS.Analytical.MobileInsight.LocationItemSummary>.Run(() =>
                analyserData.LocationSummary());
            return this.Ok(result);
        }

        [HttpGet]
        [Route("Locationstock")]
        public async Task<IHttpActionResult> Locationstock()
        {
            InfyPOS.Analytical.MobileInsight analyserData = new InfyPOS.Analytical.MobileInsight();
            List<InfyPOS.Analytical.MobileInsight.Insight.StockItem> result = await Task<List<InfyPOS.Analytical.MobileInsight.Insight.StockItem>>.Run(() =>
                analyserData.Location());
            return this.Ok(result);
        }
        [HttpGet]
        [Route("LocationPrices")]
        public async Task<IHttpActionResult> LocationPrices()
        {
            InfyPOS.Analytical.MobileInsight analyserData = new InfyPOS.Analytical.MobileInsight();
            List<InfyPOS.Analytical.MobileInsight.Insight.PriceRangeItem> result = await Task<List<InfyPOS.Analytical.MobileInsight.Insight.PriceRangeItem>>.Run(() =>
                analyserData.LocationPriceRanges(0, DateTime.Now.AddMonths(-1), DateTime.Now.Date));
            return this.Ok(result);
        }
        [HttpGet]
        [Route("StockSaleInsight")]
        public async Task<IHttpActionResult> StockSaleInsight()
        {
            InfyPOS.Analytical.MobileInsight analyserData = new InfyPOS.Analytical.MobileInsight();
            List<InfyPOS.Analytical.MobileInsight.Insight.SaleStockItem> result = await Task<List<InfyPOS.Analytical.MobileInsight.Insight.SaleStockItem>>.Run(() =>
                analyserData.StockSalesSummary());
            return this.Ok(result);
        }
        //[AllowAnonymous]
        [HttpGet]
        [Route("Location/{locationid}")]
        public async Task<IHttpActionResult> Location(string locationid)
        {
            InfyPOS.Analytical.MobileInsight analyserData = new InfyPOS.Analytical.MobileInsight();
            InfyPOS.Analytical.MobileInsight.Insight result = await Task<InfyPOS.Analytical.MobileInsight.Insight>.Run(() =>
                analyserData.Location(long.Parse(locationid)));
            return this.Ok(result);
        }

        //[AllowAnonymous]
        [HttpGet]
        [Route("Suppliers/{supplierid}/{locationid=null}")]
        public async Task<IHttpActionResult> Suppliers(string supplierid, string locationid)
        {
            InfyPOS.Analytical.MobileInsight analyserData = new InfyPOS.Analytical.MobileInsight();
            InfyPOS.Analytical.MobileInsight.Insight result = await Task<InfyPOS.Analytical.MobileInsight.Insight>.Run(() =>
                analyserData.Supplier(long.Parse(supplierid), locationid));
            return this.Ok(result);
        }

        //[AllowAnonymous]
        [HttpGet]
        [Route("Products/{productid}/{locationid=null}")]
        public async Task<IHttpActionResult> Products(string productid, string locationid)
        {
            InfyPOS.Analytical.MobileInsight analyserData = new InfyPOS.Analytical.MobileInsight();
            InfyPOS.Analytical.MobileInsight.Insight result = await Task<InfyPOS.Analytical.MobileInsight.Insight>.Run(() =>
                analyserData.Product(long.Parse(productid), locationid));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("StockMarker")]
        [WebAPI.InfyAuthorize(PermissionSet.stockanalyser, InfyPOS.Common.Action.View)]
        public async Task<IHttpActionResult> StockMarker(ActionRequest<AnalyserDataService.Configuration> request)
        {
            AnalyserDataService analyserData = new AnalyserDataService();
            if (!analyserData.Context.accessiblelocation.Exists(e => e == -1) && request.Item.locationid == 0)
                throw new Exception("Don't have permission to access all location data");

            request.Item.reporttype = AnalyserDataService.Configuration.ReportType.StockMarker;


            var queueManager = InfyPOS.Processors.WorkflowQueueManager.Instance;
            if (!queueManager.Enabled)
            {
                if (request.Item.isreport || (request.Item.Columns != null && request.Item.Columns.Count > 0) ||
                (!string.IsNullOrEmpty(request.Item.GroupColumn)))
                {
                    if (!string.IsNullOrEmpty(request.Item.GroupColumn))
                    {
                        AnalyserDataService.ResultGroup result = await Task<AnalyserDataService.ResultGroup>.Run(() => analyserData.StockAnalyserGroup(request.Item));
                        return this.Ok(result);
                    }
                    else
                    {

                        AnalyserDataService.ResultItemGroups result = await Task<AnalyserDataService.ResultItemGroups>.Run(() => analyserData.StockAnalyserReport(request.Item));
                        return this.Ok(result);
                    }
                }
                else
                {
                    List<AnalyserDataService.Items> result = await Task<List<AnalyserDataService.Items>>.Run(() => analyserData.StockAnalyser(request.Item));
                    return this.Ok(result);
                }
            }
            else
            {
                InboxDataService ActualDataService = new InboxDataService();
                var organizationId = ActualDataService.Context?.OrganizationID ?? 0;
                var userId = ActualDataService.Context?.UserID.ToString() ?? "";
                var response = await queueManager.ExecuteWithTimeout<object>(async () =>
                {
                    if (request.Item.isreport || (request.Item.Columns != null && request.Item.Columns.Count > 0) ||
                    (!string.IsNullOrEmpty(request.Item.GroupColumn)))
                    {
                        if (!string.IsNullOrEmpty(request.Item.GroupColumn))
                        {
                            return await Task<AnalyserDataService.ResultGroup>.Run(() => analyserData.StockAnalyserGroup(request.Item));
                        }
                        else
                        {
                            return await Task<AnalyserDataService.ResultItemGroups>.Run(() => analyserData.StockAnalyserReport(request.Item));
                        }
                    }
                    else
                    {
                        return await Task<List<AnalyserDataService.Items>>.Run(() => analyserData.StockAnalyser(request.Item));
                    }
                }, "AnalyserService.StockMarker", request.Item, 3, organizationId, userId);

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
        [Route("StockAnalyser")]
        [WebAPI.InfyAuthorize(PermissionSet.stockanalyser, InfyPOS.Common.Action.View)]
        public async Task<IHttpActionResult> StockAnalyser(ActionRequest<AnalyserDataService.Configuration> request)
        {
            AnalyserDataService analyserData = new AnalyserDataService();
            if (!analyserData.Context.accessiblelocation.Exists(e => e == -1) && request.Item.locationid == 0)
                throw new Exception("Don't have permission to access all location data");

            var queueManager = InfyPOS.Processors.WorkflowQueueManager.Instance;
            if (!queueManager.Enabled)
            {
                if (request.Item.isreport || (request.Item.Columns != null && request.Item.Columns.Count > 0) ||
                                (!string.IsNullOrEmpty(request.Item.GroupColumn)))
                {
                    if (!string.IsNullOrEmpty(request.Item.GroupColumn))
                    {
                        AnalyserDataService.ResultGroup result = await Task<AnalyserDataService.ResultGroup>.Run(() => analyserData.StockAnalyserGroup(request.Item));
                        return this.Ok(result);
                    }
                    else
                    {

                        AnalyserDataService.ResultItemGroups result = await Task<AnalyserDataService.ResultItemGroups>.Run(() => analyserData.StockAnalyserReport(request.Item));
                        return this.Ok(result);
                    }
                }
                else
                {
                    List<AnalyserDataService.Items> result = await Task<List<AnalyserDataService.Items>>.Run(() => analyserData.StockAnalyser(request.Item));
                    return this.Ok(result);
                }
            }
            else
            {
                InboxDataService ActualDataService = new InboxDataService();
                var organizationId = ActualDataService.Context?.OrganizationID ?? 0;
                var userId = ActualDataService.Context?.UserID.ToString() ?? "";
                var response = await queueManager.ExecuteWithTimeout<object>(async () =>
                {

                    if (request.Item.isreport || (request.Item.Columns != null && request.Item.Columns.Count > 0) ||
                               (!string.IsNullOrEmpty(request.Item.GroupColumn)))
                    {
                        if (!string.IsNullOrEmpty(request.Item.GroupColumn))
                        {
                            return await Task<AnalyserDataService.ResultGroup>.Run(() => analyserData.StockAnalyserGroup(request.Item));
                        }
                        else
                        {
                            return await Task<AnalyserDataService.ResultItemGroups>.Run(() => analyserData.StockAnalyserReport(request.Item));
                        }
                    }
                    else
                    {
                        return await Task<List<AnalyserDataService.Items>>.Run(() => analyserData.StockAnalyser(request.Item));
                    }

                }, "AnalyserService.StockAnalyser", request.Item, 3, organizationId, userId);

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
        [Route("ImagePDFGeneration")]
        public async Task<IHttpActionResult> ImagePDFGeneration(ActionRequest<AnalyserDataService.ImagePDFRequest> request)
        {
            PDFGenerator analyserData = new PDFGenerator();
            byte[] pdfbytes = await Task<byte[]>.Run(() => analyserData.ImagePDFGeneration(request.Item));
            string key = WebAPI.Data.Token.RandomString(16);
            System.Web.HttpContext.Current.Cache.Add("PDF-" + key, pdfbytes, null,
                DateTime.Now.AddMinutes(2), TimeSpan.Zero, System.Web.Caching.CacheItemPriority.Low, null);
            return this.Ok(key);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("ImagePDFGeneration/{key}")]
        public async Task<IHttpActionResult> ImagePDFGeneration(string key)
        {
            try
            {
                var httpcontext = System.Web.HttpContext.Current;
                if (httpcontext.Cache.Get("PDF-" + key) == null)
                {
                    return this.InternalServerError();
                }
                FileContentInfo byteArray = new FileContentInfo(new ByteArrayContent((byte[])httpcontext.Cache.Get("PDF-" + key)));
                byteArray.FileName = "images.pdf";
                byteArray.Extension = "application/pdf";
                httpcontext.Cache.Remove("PDF-" + key);
                return new FileActionResult(false, "", byteArray.Extension, byteArray.Content);
            }
            catch (Exception exp)
            {
                return this.Ok(exp);
            }
        }

        [HttpPost]
        [Route("ConversionAnalyser")]
        [WebAPI.InfyAuthorize(PermissionSet.revenuegeneration, InfyPOS.Common.Action.View)]
        public async Task<IHttpActionResult> ConversionAnalyser(ActionRequest<AnalyserDataService.Configuration> request)
        {
            AnalyserDataService analyserData = new AnalyserDataService();
            request.Item.reporttype = AnalyserDataService.Configuration.ReportType.Conversion;

            var queueManager = InfyPOS.Processors.WorkflowQueueManager.Instance;
            if (!queueManager.Enabled)
            {

                if (request.Item.isreport || (request.Item.Columns != null && request.Item.Columns.Count > 0) ||
                                (!string.IsNullOrEmpty(request.Item.GroupColumn)))
                {
                    if (!string.IsNullOrEmpty(request.Item.GroupColumn))
                    {
                        AnalyserDataService.ResultGroup result = await Task<AnalyserDataService.ResultGroup>.Run(() => analyserData.StockAnalyserGroup(request.Item));
                        return this.Ok(result);
                    }
                    else
                    {

                        AnalyserDataService.ResultItemGroups result = await Task<AnalyserDataService.ResultItemGroups>.Run(() => analyserData.StockAnalyserReport(request.Item));
                        return this.Ok(result);
                    }
                }
                else
                {
                    List<AnalyserDataService.Items> result = await Task<List<AnalyserDataService.Items>>.Run(() => analyserData.StockAnalyser(request.Item));
                    return this.Ok(result);
                }
            }
            else
            {
                InboxDataService ActualDataService = new InboxDataService();
                var organizationId = ActualDataService.Context?.OrganizationID ?? 0;
                var userId = ActualDataService.Context?.UserID.ToString() ?? "";
                var response = await queueManager.ExecuteWithTimeout<object>(async () =>
                {
                    if (request.Item.isreport || (request.Item.Columns != null && request.Item.Columns.Count > 0) ||
                               (!string.IsNullOrEmpty(request.Item.GroupColumn)))
                    {
                        if (!string.IsNullOrEmpty(request.Item.GroupColumn))
                        {
                            return await Task<AnalyserDataService.ResultGroup>.Run(() => analyserData.StockAnalyserGroup(request.Item));
                        }
                        else
                        {

                            return await Task<AnalyserDataService.ResultItemGroups>.Run(() => analyserData.StockAnalyserReport(request.Item));
                        }
                    }
                    else
                    {
                        return await Task<List<AnalyserDataService.Items>>.Run(() => analyserData.StockAnalyser(request.Item));
                    }


                }, "AnalyserService.ConversionAnalyser", request.Item, 3, organizationId, userId);

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
        [Route("PurchaseAnalyser")]
        [WebAPI.InfyAuthorize(PermissionSet.purchaseanalyser, InfyPOS.Common.Action.View)]
        public async Task<IHttpActionResult> PurchaseAnalyser(ActionRequest<AnalyserDataService.Configuration> request)
        {
            if (request.Item.fromdate == DateTime.MinValue || request.Item.todate == DateTime.MinValue)
                throw new Exception("Please Select Date Range for Analysis");

            AnalyserDataService analyserData = new AnalyserDataService();
            if (analyserData.HasResctrictedAccess && ((request.Item.fromdate.Date == DateTime.MinValue ||
              request.Item.todate.Date == DateTime.MinValue) || (request.Item.todate.Subtract(request.Item.fromdate).TotalDays > 45)))
            {
                throw new Exception("Please Select different date range for Analysis , you are restricted becuase of season sale is going");
            }
            request.Item.reporttype = AnalyserDataService.Configuration.ReportType.Purchase;

            var queueManager = InfyPOS.Processors.WorkflowQueueManager.Instance;
            if (!queueManager.Enabled)
            {

                if (request.Item.isreport || (request.Item.Columns != null && request.Item.Columns.Count > 0) ||
                                (!string.IsNullOrEmpty(request.Item.GroupColumn)))
                {
                    if (!string.IsNullOrEmpty(request.Item.GroupColumn))
                    {
                        AnalyserDataService.ResultGroup result = await Task<AnalyserDataService.ResultGroup>.Run(() => analyserData.StockAnalyserGroup(request.Item));
                        return this.Ok(result);
                    }
                    else
                    {
                        AnalyserDataService.ResultItemGroups result = await Task<AnalyserDataService.ResultItemGroups>.Run(() => analyserData.StockAnalyserReport(request.Item));
                        return this.Ok(result);
                    }
                }
                else
                {
                    List<AnalyserDataService.Items> result = await Task<List<AnalyserDataService.Items>>.Run(() => analyserData.StockAnalyser(request.Item));
                    return this.Ok(result);
                }
            }
            else
            {
                InboxDataService ActualDataService = new InboxDataService();
                var organizationId = ActualDataService.Context?.OrganizationID ?? 0;
                var userId = ActualDataService.Context?.UserID.ToString() ?? "";
                var response = await queueManager.ExecuteWithTimeout<object>(async () =>
                {

                    if (request.Item.isreport || (request.Item.Columns != null && request.Item.Columns.Count > 0) ||
                           (!string.IsNullOrEmpty(request.Item.GroupColumn)))
                    {
                        if (!string.IsNullOrEmpty(request.Item.GroupColumn))
                        {
                            return await Task<AnalyserDataService.ResultGroup>.Run(() => analyserData.StockAnalyserGroup(request.Item));
                        }
                        else
                        {
                            return await Task<AnalyserDataService.ResultItemGroups>.Run(() => analyserData.StockAnalyserReport(request.Item));
                        }
                    }
                    else
                    {
                        return await Task<List<AnalyserDataService.Items>>.Run(() => analyserData.StockAnalyser(request.Item));
                    }

                }, "AnalyserService.PurchaseAnalyser", request.Item, 3, organizationId, userId);

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
        [Route("PurchaseReturnAnalyser")]
        [WebAPI.InfyAuthorize(PermissionSet.purchaseanalyser, InfyPOS.Common.Action.View)]
        public async Task<IHttpActionResult> PurchaseReturnAnalyser(ActionRequest<AnalyserDataService.Configuration> request)
        {
            AnalyserDataService analyserData = new AnalyserDataService();
            request.Item.reporttype = AnalyserDataService.Configuration.ReportType.PurchaseReturn;

            if (analyserData.HasResctrictedAccess && ((request.Item.fromdate.Date == DateTime.MinValue ||
                request.Item.todate.Date == DateTime.MinValue) || (request.Item.todate.Subtract(request.Item.fromdate).TotalDays > 45)))
            {
                throw new Exception("Please Select different date range for Analysis , you are restricted becuase of season sale is going");
            }

            var queueManager = InfyPOS.Processors.WorkflowQueueManager.Instance;
            if (!queueManager.Enabled)
            {

                if (request.Item.isreport || (request.Item.Columns != null && request.Item.Columns.Count > 0) ||
                                (!string.IsNullOrEmpty(request.Item.GroupColumn)))
                {
                    if (!string.IsNullOrEmpty(request.Item.GroupColumn))
                    {
                        AnalyserDataService.ResultGroup result = await Task<AnalyserDataService.ResultGroup>.Run(() => analyserData.StockAnalyserGroup(request.Item));
                        return this.Ok(result);
                    }
                    else
                    {
                        AnalyserDataService.ResultItemGroups result = await Task<AnalyserDataService.ResultItemGroups>.Run(() => analyserData.StockAnalyserReport(request.Item));
                        return this.Ok(result);
                    }
                }
                else
                {
                    List<AnalyserDataService.Items> result = await Task<List<AnalyserDataService.Items>>.Run(() => analyserData.StockAnalyser(request.Item));
                    return this.Ok(result);
                }
            }
            else
            {
                InboxDataService ActualDataService = new InboxDataService();
                var organizationId = ActualDataService.Context?.OrganizationID ?? 0;
                var userId = ActualDataService.Context?.UserID.ToString() ?? "";
                var response = await queueManager.ExecuteWithTimeout<object>(async () =>
                {

                    if (request.Item.isreport || (request.Item.Columns != null && request.Item.Columns.Count > 0) ||
                                (!string.IsNullOrEmpty(request.Item.GroupColumn)))
                    {
                        if (!string.IsNullOrEmpty(request.Item.GroupColumn))
                        {
                            return await Task<AnalyserDataService.ResultGroup>.Run(() => analyserData.StockAnalyserGroup(request.Item));
                        }
                        else
                        {
                            return await Task<AnalyserDataService.ResultItemGroups>.Run(() => analyserData.StockAnalyserReport(request.Item));
                        }
                    }
                    else
                    {
                        return await Task<List<AnalyserDataService.Items>>.Run(() => analyserData.StockAnalyser(request.Item));
                    }

                }, "AnalyserService.PurchaseReturnAnalyser", request.Item, 3, organizationId, userId);

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
        [Route("PurchasePlanner")]
        [WebAPI.InfyAuthorize(PermissionSet.purchaseplanner, InfyPOS.Common.Action.View)]
        public async Task<IHttpActionResult> PurchasePlanner(ActionRequest<PlanningDataSource.PurchasePlanConfig> request)
        {
            PlanningDataSource analyserData = new PlanningDataSource();

            var queueManager = InfyPOS.Processors.WorkflowQueueManager.Instance;
            if (!queueManager.Enabled)
            {
                AnalyserDataService.ResultGroup result = await Task<AnalyserDataService.ResultGroup>.Run(() => analyserData.BuildPurchasePlan(request.Item));
                return this.Ok(result);
            }
            else
            {
                InboxDataService ActualDataService = new InboxDataService();
                var organizationId = ActualDataService.Context?.OrganizationID ?? 0;
                var userId = ActualDataService.Context?.UserID.ToString() ?? "";
                var response = await queueManager.ExecuteWithTimeout<object>(async () =>
                {
                    return await Task<AnalyserDataService.ResultGroup>.Run(() => analyserData.BuildPurchasePlan(request.Item));
                }, "AnalyserService.PurchasePlanner", request.Item, 3, organizationId, userId);

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
        [Route("SectionWiseSalesIncentive")]
        public async Task<IHttpActionResult> SectionWiseSalesIncentive(ActionRequest<Models.ReferenceSectionConfig> request)
        {
            AnalyserDataService analyserData = new AnalyserDataService();
            List<AnalyserDataService.SalesIncentiveValues> result = await Task<List<AnalyserDataService.SalesIncentiveValues>>.Run(() => analyserData.SectionWiseSalesIncentive(request.Item));
            return this.Ok(result);

        }

        [HttpPost]
        [Route("SalesPalanner")]
        [WebAPI.InfyAuthorize(PermissionSet.salesplanner, InfyPOS.Common.Action.View)]
        public async Task<IHttpActionResult> SalesPalanner(ActionRequest<AnalyserDataService.Configuration> request)
        {
            AnalyserDataService analyserData = new AnalyserDataService();
            request.Item.reporttype = AnalyserDataService.Configuration.ReportType.SalesPlan;

            var queueManager = InfyPOS.Processors.WorkflowQueueManager.Instance;
            if (!queueManager.Enabled)
            {

                if (request.Item.isreport || (request.Item.Columns != null && request.Item.Columns.Count > 0) ||
                                (!string.IsNullOrEmpty(request.Item.GroupColumn)))
                {
                    if (!string.IsNullOrEmpty(request.Item.GroupColumn))
                    {
                        AnalyserDataService.ResultGroup result = await Task<AnalyserDataService.ResultGroup>.Run(() => analyserData.StockAnalyserGroup(request.Item));
                        return this.Ok(result);
                    }
                    else
                    {
                        AnalyserDataService.ResultItemGroups result = await Task<AnalyserDataService.ResultItemGroups>.Run(() => analyserData.StockAnalyserReport(request.Item));
                        return this.Ok(result);
                    }
                }
                else
                {
                    List<AnalyserDataService.Items> result = await Task<List<AnalyserDataService.Items>>.Run(() => analyserData.StockAnalyser(request.Item));
                    return this.Ok(result);
                }
            }
            else
            {
                InboxDataService ActualDataService = new InboxDataService();
                var organizationId = ActualDataService.Context?.OrganizationID ?? 0;
                var userId = ActualDataService.Context?.UserID.ToString() ?? "";
                var response = await queueManager.ExecuteWithTimeout<object>(async () =>
                {
                    if (request.Item.isreport || (request.Item.Columns != null && request.Item.Columns.Count > 0) ||
                                (!string.IsNullOrEmpty(request.Item.GroupColumn)))
                    {
                        if (!string.IsNullOrEmpty(request.Item.GroupColumn))
                        {
                            return await Task<AnalyserDataService.ResultGroup>.Run(() => analyserData.StockAnalyserGroup(request.Item));
                        }
                        else
                        {
                            return await Task<AnalyserDataService.ResultItemGroups>.Run(() => analyserData.StockAnalyserReport(request.Item));
                        }
                    }
                    else
                    {
                        return await Task<List<AnalyserDataService.Items>>.Run(() => analyserData.StockAnalyser(request.Item));
                    }

                }, "AnalyserService.SalesPalanner", request.Item, 3, organizationId, userId);

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
        [Route("SalesAsOnDateAnalyser")]
        [WebAPI.InfyAuthorize(PermissionSet.salesvspurchase, InfyPOS.Common.Action.View)]
        public async Task<IHttpActionResult> SalesAsOnDateAnalyser(ActionRequest<AnalyserDataService.Configuration> request)
        {
            AnalyserDataService analyserData = new AnalyserDataService();
            request.Item.reporttype = AnalyserDataService.Configuration.ReportType.SalesAsOnDateAnalyser;

            var queueManager = InfyPOS.Processors.WorkflowQueueManager.Instance;
            if (!queueManager.Enabled)
            {

                if (request.Item.isreport || (request.Item.Columns != null && request.Item.Columns.Count > 0) ||
                                (!string.IsNullOrEmpty(request.Item.GroupColumn)))
                {
                    if (!string.IsNullOrEmpty(request.Item.GroupColumn))
                    {
                        AnalyserDataService.ResultGroup result = await Task<AnalyserDataService.ResultGroup>.Run(() => analyserData.StockAnalyserGroup(request.Item));
                        return this.Ok(result);
                    }
                    else
                    {
                        AnalyserDataService.ResultItemGroups result = await Task<AnalyserDataService.ResultItemGroups>.Run(() => analyserData.StockAnalyserReport(request.Item));
                        return this.Ok(result);
                    }
                }
                else
                {
                    List<AnalyserDataService.Items> result = await Task<List<AnalyserDataService.Items>>.Run(() => analyserData.StockAnalyser(request.Item));
                    return this.Ok(result);
                }
            }
            else
            {
                InboxDataService ActualDataService = new InboxDataService();
                var organizationId = ActualDataService.Context?.OrganizationID ?? 0;
                var userId = ActualDataService.Context?.UserID.ToString() ?? "";
                var response = await queueManager.ExecuteWithTimeout<object>(async () =>
                {

                    if (request.Item.isreport || (request.Item.Columns != null && request.Item.Columns.Count > 0) ||
                                (!string.IsNullOrEmpty(request.Item.GroupColumn)))
                    {
                        if (!string.IsNullOrEmpty(request.Item.GroupColumn))
                        {
                            return await Task<AnalyserDataService.ResultGroup>.Run(() => analyserData.StockAnalyserGroup(request.Item));
                        }
                        else
                        {
                            return await Task<AnalyserDataService.ResultItemGroups>.Run(() => analyserData.StockAnalyserReport(request.Item));
                        }
                    }
                    else
                    {
                        return await Task<List<AnalyserDataService.Items>>.Run(() => analyserData.StockAnalyser(request.Item));
                    }

                }, "AnalyserService.SalesAsOnDateAnalyser", request.Item, 3, organizationId, userId);

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
        [Route("SalesVsPurchaseAnalyser")]
        [WebAPI.InfyAuthorize(PermissionSet.salesvspurchase, InfyPOS.Common.Action.View)]
        public async Task<IHttpActionResult> SalesVsPurchaseAnalyser(ActionRequest<AnalyserDataService.Configuration> request)
        {

            AnalyserDataService analyserData = new AnalyserDataService();
            if (analyserData.HasResctrictedAccess && ((request.Item.fromdate.Date == DateTime.MinValue ||
              request.Item.todate.Date == DateTime.MinValue) || (request.Item.todate.Subtract(request.Item.fromdate).TotalDays > 45)))
            {
                throw new Exception("Please Select different date range for Analysis , you are restricted becuase of season sale is going");
            }
            if (analyserData.HasResctrictedAccess && ((request.Item.fromdatecompare.Date == DateTime.MinValue ||
             request.Item.todatecompare.Date == DateTime.MinValue) || (request.Item.todatecompare.Subtract(request.Item.fromdatecompare).TotalDays > 45)))
            {
                throw new Exception("Please Select different date range for Analysis , you are restricted becuase of season sale is going");
            }
            request.Item.reporttype = AnalyserDataService.Configuration.ReportType.SaleVsPurchase;
            if (!analyserData.Context.accessiblelocation.Exists(e => e == -1) && request.Item.locationid == 0)
                throw new Exception("Don't have permission to access all location data");


            var queueManager = InfyPOS.Processors.WorkflowQueueManager.Instance;
            if (!queueManager.Enabled)
            {
                if (request.Item.isreport || (request.Item.Columns != null && request.Item.Columns.Count > 0) ||
                                (!string.IsNullOrEmpty(request.Item.GroupColumn)))
                {
                    if (!string.IsNullOrEmpty(request.Item.GroupColumn))
                    {
                        AnalyserDataService.ResultGroup result = await Task<AnalyserDataService.ResultGroup>.Run(() => analyserData.StockAnalyserGroup(request.Item));
                        return this.Ok(result);
                    }
                    else
                    {
                        AnalyserDataService.ResultItemGroups result = await Task<AnalyserDataService.ResultItemGroups>.Run(() => analyserData.StockAnalyserReport(request.Item));
                        return this.Ok(result);
                    }
                }
                else
                {
                    List<AnalyserDataService.Items> result = await Task<List<AnalyserDataService.Items>>.Run(() => analyserData.StockAnalyser(request.Item));
                    return this.Ok(result);
                }
            }
            else
            {
                InboxDataService ActualDataService = new InboxDataService();
                var organizationId = ActualDataService.Context?.OrganizationID ?? 0;
                var userId = ActualDataService.Context?.UserID.ToString() ?? "";
                var response = await queueManager.ExecuteWithTimeout<object>(async () =>
                {
                    if (request.Item.isreport || (request.Item.Columns != null && request.Item.Columns.Count > 0) ||
                                (!string.IsNullOrEmpty(request.Item.GroupColumn)))
                    {
                        if (!string.IsNullOrEmpty(request.Item.GroupColumn))
                        {
                            return await Task<AnalyserDataService.ResultGroup>.Run(() => analyserData.StockAnalyserGroup(request.Item));
                        }
                        else
                        {
                            return await Task<AnalyserDataService.ResultItemGroups>.Run(() => analyserData.StockAnalyserReport(request.Item));
                        }
                    }
                    else
                    {
                        return await Task<List<AnalyserDataService.Items>>.Run(() => analyserData.StockAnalyser(request.Item));
                    }
                }, "AnalyserService.SalesVsPurchaseAnalyser", request.Item, 3, organizationId, userId);

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
        [Route("StockStressAnalyser")]
        [WebAPI.InfyAuthorize(PermissionSet.stressanalyser, InfyPOS.Common.Action.View)]
        public async Task<IHttpActionResult> StockStressAnalyser(ActionRequest<AnalyserDataService.Configuration> request)
        {
            AnalyserDataService analyserData = new AnalyserDataService();
            request.Item.reporttype = AnalyserDataService.Configuration.ReportType.StockStress;

            var queueManager = InfyPOS.Processors.WorkflowQueueManager.Instance;
            if (!queueManager.Enabled)
            {

                if (request.Item.isreport || (request.Item.Columns != null && request.Item.Columns.Count > 0) ||
                                (!string.IsNullOrEmpty(request.Item.GroupColumn)))
                {
                    if (!string.IsNullOrEmpty(request.Item.GroupColumn))
                    {
                        AnalyserDataService.ResultGroup result = await Task<AnalyserDataService.ResultGroup>.Run(() => analyserData.StockAnalyserGroup(request.Item));
                        return this.Ok(result);
                    }
                    else
                    {
                        AnalyserDataService.ResultItemGroups result = await Task<AnalyserDataService.ResultItemGroups>.Run(() => analyserData.StockAnalyserReport(request.Item));
                        return this.Ok(result);
                    }
                }
                else
                {
                    List<AnalyserDataService.Items> result = await Task<List<AnalyserDataService.Items>>.Run(() => analyserData.StockAnalyser(request.Item));
                    int tdays = (int)request.Item.todate.Subtract(request.Item.fromdate.Date).TotalDays;
                    result.ForEach(e =>
                    {
                        if (tdays > 0 && (e.qty / tdays) > 0)
                        {
                            var topup = (e.sqty * request.Item.markup) / 100;
                            e.cqty = Math.Round(((e.sqty + topup) / (e.qty / tdays)), 0);
                        }
                    });
                    return this.Ok(result);
                }
            }
            else
            {
                InboxDataService ActualDataService = new InboxDataService();
                var organizationId = ActualDataService.Context?.OrganizationID ?? 0;
                var userId = ActualDataService.Context?.UserID.ToString() ?? "";
                var response = await queueManager.ExecuteWithTimeout<object>(async () =>
                {


                    if (request.Item.isreport || (request.Item.Columns != null && request.Item.Columns.Count > 0) ||
                                    (!string.IsNullOrEmpty(request.Item.GroupColumn)))
                    {
                        if (!string.IsNullOrEmpty(request.Item.GroupColumn))
                        {
                            return await Task<AnalyserDataService.ResultGroup>.Run(() => analyserData.StockAnalyserGroup(request.Item));
                        }
                        else
                        {
                            return await Task<AnalyserDataService.ResultItemGroups>.Run(() => analyserData.StockAnalyserReport(request.Item));
                        }
                    }
                    else
                    {
                        List<AnalyserDataService.Items> result = await Task<List<AnalyserDataService.Items>>.Run(() => analyserData.StockAnalyser(request.Item));
                        int tdays = (int)request.Item.todate.Subtract(request.Item.fromdate.Date).TotalDays;
                        result.ForEach(e =>
                        {
                            if (tdays > 0 && (e.qty / tdays) > 0)
                            {
                                var topup = (e.sqty * request.Item.markup) / 100;
                                e.cqty = Math.Round(((e.sqty + topup) / (e.qty / tdays)), 0);
                            }
                        });
                        return this.Ok(result);
                    }


                }, "AnalyserService.StockStressAnalyser", request.Item, 3, organizationId, userId);

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
        [Route("SalesCompareAnalyser")]
        [WebAPI.InfyAuthorize(PermissionSet.salescomparer, InfyPOS.Common.Action.View)]
        public async Task<IHttpActionResult> SalesCompareAnalyser(ActionRequest<AnalyserDataService.Configuration> request)
        {
            AnalyserDataService analyserData = new AnalyserDataService();
            if (analyserData.HasResctrictedAccess && ((request.Item.fromdate.Date == DateTime.MinValue ||
              request.Item.todate.Date == DateTime.MinValue) || (request.Item.todate.Subtract(request.Item.fromdate).TotalDays > 45)))
            {
                throw new Exception("Please Select different date range for Analysis , you are restricted becuase of season sale is going");
            }
            if (analyserData.HasResctrictedAccess && ((request.Item.fromdatecompare.Date == DateTime.MinValue ||
             request.Item.todatecompare.Date == DateTime.MinValue) || (request.Item.todatecompare.Subtract(request.Item.fromdatecompare).TotalDays > 45)))
            {
                throw new Exception("Please Select different date range for Analysis , you are restricted becuase of season sale is going");
            }

            request.Item.reporttype = AnalyserDataService.Configuration.ReportType.SalesCompare;
            if (!analyserData.Context.accessiblelocation.Exists(e => e == -1) && request.Item.locationid == 0)
                throw new Exception("Don't have permission to access all location data");


            var queueManager = InfyPOS.Processors.WorkflowQueueManager.Instance;
            if (!queueManager.Enabled)
            {

                if (request.Item.isreport || (request.Item.Columns != null && request.Item.Columns.Count > 0) ||
                                (!string.IsNullOrEmpty(request.Item.GroupColumn)))
                {
                    if (!string.IsNullOrEmpty(request.Item.GroupColumn))
                    {
                        AnalyserDataService.ResultGroup result = await Task<AnalyserDataService.ResultGroup>.Run(() => analyserData.StockAnalyserGroup(request.Item));
                        return this.Ok(result);
                    }
                    else
                    {
                        AnalyserDataService.ResultItemGroups result = await Task<AnalyserDataService.ResultItemGroups>.Run(() => analyserData.StockAnalyserReport(request.Item));
                        return this.Ok(result);
                    }
                }
                else
                {
                    List<AnalyserDataService.Items> result = await Task<List<AnalyserDataService.Items>>.Run(() => analyserData.StockAnalyser(request.Item));
                    return this.Ok(result);
                }
            }
            else
            {
                InboxDataService ActualDataService = new InboxDataService();
                var organizationId = ActualDataService.Context?.OrganizationID ?? 0;
                var userId = ActualDataService.Context?.UserID.ToString() ?? "";
                var response = await queueManager.ExecuteWithTimeout<object>(async () =>
                {
                    if (request.Item.isreport || (request.Item.Columns != null && request.Item.Columns.Count > 0) ||
                                    (!string.IsNullOrEmpty(request.Item.GroupColumn)))
                    {
                        if (!string.IsNullOrEmpty(request.Item.GroupColumn))
                        {
                            return await Task<AnalyserDataService.ResultGroup>.Run(() => analyserData.StockAnalyserGroup(request.Item));
                        }
                        else
                        {
                            return await Task<AnalyserDataService.ResultItemGroups>.Run(() => analyserData.StockAnalyserReport(request.Item));
                        }
                    }
                    else
                    {
                        return await Task<List<AnalyserDataService.Items>>.Run(() => analyserData.StockAnalyser(request.Item));
                    }

                }, "AnalyserService.SalesCompareAnalyser", request.Item, 3, organizationId, userId);

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
        [Route("SalesAnalyser")]
        [WebAPI.InfyAuthorize(PermissionSet.salesanalyser, InfyPOS.Common.Action.View)]
        public async Task<IHttpActionResult> SalesAnalyser(ActionRequest<AnalyserDataService.Configuration> request)
        {
            AnalyserDataService analyserData = new AnalyserDataService();
            request.Item.reporttype = AnalyserDataService.Configuration.ReportType.Sales;
            if (analyserData.HasResctrictedAccess && ((request.Item.fromdate.Date == DateTime.MinValue ||
              request.Item.todate.Date == DateTime.MinValue) || (request.Item.todate.Subtract(request.Item.fromdate).TotalDays > 45)))
            {
                throw new Exception("Please Select different date range for Analysis , you are restricted becuase of season sale is going");
            }

            if (!analyserData.Context.accessiblelocation.Exists(e => e == -1) && request.Item.locationid == 0)
                throw new Exception("Don't have permission to access all location data");

            request.Item.currentreporttype = request.Item.reporttype;


            var queueManager = InfyPOS.Processors.WorkflowQueueManager.Instance;
            if (!queueManager.Enabled)
            {

                if (request.Item.isreport || (request.Item.Columns != null && request.Item.Columns.Count > 0) ||
                                (!string.IsNullOrEmpty(request.Item.GroupColumn)))
                {
                    if (!string.IsNullOrEmpty(request.Item.GroupColumn))
                    {
                        AnalyserDataService.ResultGroup result = await Task<AnalyserDataService.ResultGroup>.Run(() => analyserData.StockAnalyserGroup(request.Item));
                        return this.Ok(result);
                    }
                    else
                    {
                        AnalyserDataService.ResultItemGroups result = await Task<AnalyserDataService.ResultItemGroups>.Run(() => analyserData.StockAnalyserReport(request.Item));
                        return this.Ok(result);
                    }
                }
                else
                {
                    List<AnalyserDataService.Items> result = await Task<List<AnalyserDataService.Items>>.Run(() => analyserData.StockAnalyser(request.Item));
                    return this.Ok(result);
                }
            }
            else
            {
                InboxDataService ActualDataService = new InboxDataService();
                var organizationId = ActualDataService.Context?.OrganizationID ?? 0;
                var userId = ActualDataService.Context?.UserID.ToString() ?? "";
                var response = await queueManager.ExecuteWithTimeout<object>(async () =>
                {

                    if (request.Item.isreport || (request.Item.Columns != null && request.Item.Columns.Count > 0) ||
                                    (!string.IsNullOrEmpty(request.Item.GroupColumn)))
                    {
                        if (!string.IsNullOrEmpty(request.Item.GroupColumn))
                        {
                            return await Task<AnalyserDataService.ResultGroup>.Run(() => analyserData.StockAnalyserGroup(request.Item));
                        }
                        else
                        {
                            return await Task<AnalyserDataService.ResultItemGroups>.Run(() => analyserData.StockAnalyserReport(request.Item));
                        }
                    }
                    else
                    {
                        return await Task<List<AnalyserDataService.Items>>.Run(() => analyserData.StockAnalyser(request.Item));
                    }

                }, "AnalyserService.SalesAnalyser", request.Item, 3, organizationId, userId);

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
        [Route("SalesApprovalAnalyser")]
        [WebAPI.InfyAuthorize(PermissionSet.salesanalyser, InfyPOS.Common.Action.View)]
        public async Task<IHttpActionResult> SalesApprovalAnalyser(ActionRequest<AnalyserDataService.Configuration> request)
        {
            AnalyserDataService analyserData = new AnalyserDataService();
            request.Item.reporttype = AnalyserDataService.Configuration.ReportType.SalesOnApproval;
            if (!analyserData.Context.accessiblelocation.Exists(e => e == -1) && request.Item.locationid == 0)
                throw new Exception("Don't have permission to access all location data");

            request.Item.currentreporttype = request.Item.reporttype;

            var queueManager = InfyPOS.Processors.WorkflowQueueManager.Instance;
            if (!queueManager.Enabled)
            {

                if (request.Item.isreport || (request.Item.Columns != null && request.Item.Columns.Count > 0) ||
                                (!string.IsNullOrEmpty(request.Item.GroupColumn)))
                {
                    if (!string.IsNullOrEmpty(request.Item.GroupColumn))
                    {
                        AnalyserDataService.ResultGroup result = await Task<AnalyserDataService.ResultGroup>.Run(() => analyserData.StockAnalyserGroup(request.Item));
                        return this.Ok(result);
                    }
                    else
                    {
                        AnalyserDataService.ResultItemGroups result = await Task<AnalyserDataService.ResultItemGroups>.Run(() => analyserData.StockAnalyserReport(request.Item));
                        return this.Ok(result);
                    }
                }
                else
                {
                    List<AnalyserDataService.Items> result = await Task<List<AnalyserDataService.Items>>.Run(() => analyserData.StockAnalyser(request.Item));
                    return this.Ok(result);
                }
            }
            else
            {
                InboxDataService ActualDataService = new InboxDataService();
                var organizationId = ActualDataService.Context?.OrganizationID ?? 0;
                var userId = ActualDataService.Context?.UserID.ToString() ?? "";
                var response = await queueManager.ExecuteWithTimeout<object>(async () =>
                {

                    if (request.Item.isreport || (request.Item.Columns != null && request.Item.Columns.Count > 0) ||
                (!string.IsNullOrEmpty(request.Item.GroupColumn)))
                    {
                        if (!string.IsNullOrEmpty(request.Item.GroupColumn))
                        {
                            return await Task<AnalyserDataService.ResultGroup>.Run(() => analyserData.StockAnalyserGroup(request.Item));
                        }
                        else
                        {
                            return await Task<AnalyserDataService.ResultItemGroups>.Run(() => analyserData.StockAnalyserReport(request.Item));
                        }
                    }
                    else
                    {
                        return await Task<List<AnalyserDataService.Items>>.Run(() => analyserData.StockAnalyser(request.Item));
                    }


                }, "AnalyserService.SalesApprovalAnalyser", request.Item, 3, organizationId, userId);

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
        [Route("SalesVsStockAnalyser")]
        [WebAPI.InfyAuthorize(PermissionSet.salesanalyser, InfyPOS.Common.Action.View)]
        public async Task<IHttpActionResult> SalesVsStockAnalyser(ActionRequest<AnalyserDataService.Configuration> request)
        {
            AnalyserDataService analyserData = new AnalyserDataService();
            request.Item.reporttype = AnalyserDataService.Configuration.ReportType.SalesVsStockAnalyser;

            if (analyserData.HasResctrictedAccess && ((request.Item.fromdate.Date == DateTime.MinValue ||
            request.Item.todate.Date == DateTime.MinValue) || (request.Item.todate.Subtract(request.Item.fromdate).TotalDays > 45)))
            {
                throw new Exception("Please Select different date range for Analysis , you are restricted becuase of season sale is going");
            }

            if (!analyserData.Context.accessiblelocation.Exists(e => e == -1) && request.Item.locationid == 0)
                throw new Exception("Don't have permission to access all location data");

         


            var queueManager = InfyPOS.Processors.WorkflowQueueManager.Instance;
            if (!queueManager.Enabled)
            {
                if (request.Item.isreport || (request.Item.Columns != null && request.Item.Columns.Count > 0) ||
                               (!string.IsNullOrEmpty(request.Item.GroupColumn)))
                {
                    if (!string.IsNullOrEmpty(request.Item.GroupColumn))
                    {
                        AnalyserDataService.ResultGroup result = await Task<AnalyserDataService.ResultGroup>.Run(() => analyserData.StockAnalyserGroup(request.Item));
                        return this.Ok(result);
                    }
                    else
                    {
                        AnalyserDataService.ResultItemGroups result = await Task<AnalyserDataService.ResultItemGroups>.Run(() => analyserData.StockAnalyserReport(request.Item));
                        return this.Ok(result);
                    }
                }
                else
                {
                    List<AnalyserDataService.Items> result = await Task<List<AnalyserDataService.Items>>.Run(() => analyserData.StockAnalyser(request.Item));
                    return this.Ok(result);
                }
            }
            else
            {

                var organizationId = analyserData.Context?.OrganizationID ?? 0;
                var userId = analyserData.Context?.UserID.ToString() ?? "";
                var response = await queueManager.ExecuteWithTimeout<object>(async () =>
                {
                    if (request.Item.isreport || (request.Item.Columns != null && request.Item.Columns.Count > 0) ||
                         (!string.IsNullOrEmpty(request.Item.GroupColumn)))
                    {
                        if (!string.IsNullOrEmpty(request.Item.GroupColumn))
                        {
                            return await Task<AnalyserDataService.ResultGroup>.Run(() => analyserData.StockAnalyserGroup(request.Item));
                        }
                        else
                        {
                            return await Task<AnalyserDataService.ResultItemGroups>.Run(() => analyserData.StockAnalyserReport(request.Item));
                        }
                    }
                    else
                    {
                        return await Task<List<AnalyserDataService.Items>>.Run(() => analyserData.StockAnalyser(request.Item));
                    }
                }, "AnalyserService.SalesVsStockAnalyser", request.Item, 3, organizationId, userId);

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
        [Route("CustomerAnalyser")]
        [WebAPI.InfyAuthorize(PermissionSet.salesanalyser, InfyPOS.Common.Action.View)]
        public async Task<IHttpActionResult> CustomerAnalyser(ActionRequest<AnalyserDataService.Configuration> request)
        {
            AnalyserDataService analyserData = new AnalyserDataService();
            request.Item.reporttype = AnalyserDataService.Configuration.ReportType.Customers;
            request.Item.currentreporttype = AnalyserDataService.Configuration.ReportType.Customers;

            if (analyserData.HasResctrictedAccess && ((request.Item.fromdate.Date == DateTime.MinValue ||
                request.Item.todate.Date == DateTime.MinValue) || (request.Item.todate.Subtract(request.Item.fromdate).TotalDays > 45)))
            {
                throw new Exception("Please Select different date range for Analysis , you are restricted becuase of season sale is going");
            }

            if (!analyserData.Context.accessiblelocation.Exists(e => e == -1) && request.Item.locationid == 0)
                throw new Exception("Don't have permission to access all location data");

            var queueManager = InfyPOS.Processors.WorkflowQueueManager.Instance;
            if (!queueManager.Enabled)
            {
                if (request.Item.isreport || (request.Item.Columns != null && request.Item.Columns.Count > 0) ||
                                (!string.IsNullOrEmpty(request.Item.GroupColumn)))
                {
                    if (!string.IsNullOrEmpty(request.Item.GroupColumn))
                    {
                        AnalyserDataService.ResultGroup result = await Task<AnalyserDataService.ResultGroup>.Run(() => analyserData.StockAnalyserGroup(request.Item));
                        return this.Ok(result);
                    }
                    else
                    {
                        AnalyserDataService.ResultItemGroups result = await Task<AnalyserDataService.ResultItemGroups>.Run(() => analyserData.StockAnalyserReport(request.Item));
                        return this.Ok(result);
                    }
                }
                else
                {
                    List<AnalyserDataService.Items> result = await Task<List<AnalyserDataService.Items>>.Run(() => analyserData.StockAnalyser(request.Item));
                    return this.Ok(result);
                }
            }
            else
            {
                InboxDataService ActualDataService = new InboxDataService();
                var organizationId = ActualDataService.Context?.OrganizationID ?? 0;
                var userId = ActualDataService.Context?.UserID.ToString() ?? "";
                var response = await queueManager.ExecuteWithTimeout<object>(async () =>
                {

                    if (request.Item.isreport || (request.Item.Columns != null && request.Item.Columns.Count > 0) ||
                               (!string.IsNullOrEmpty(request.Item.GroupColumn)))
                    {
                        if (!string.IsNullOrEmpty(request.Item.GroupColumn))
                        {
                            return await Task<AnalyserDataService.ResultGroup>.Run(() => analyserData.StockAnalyserGroup(request.Item));
                        }
                        else
                        {
                            return await Task<AnalyserDataService.ResultItemGroups>.Run(() => analyserData.StockAnalyserReport(request.Item));
                        }
                    }
                    else
                    {
                        return await Task<List<AnalyserDataService.Items>>.Run(() => analyserData.StockAnalyser(request.Item));
                    }

                }, "AnalyserService.CustomerAnalyser", request.Item, 3, organizationId, userId);

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
        [Route("GenerateReportBuilder")]
        public async Task<IHttpActionResult> GenerateReportBuilder(ActionRequest<ReportBuilder> request)
        {
            ReportBuilderDataService reportdata = new ReportBuilderDataService();

            if (reportdata.HasResctrictedAccess && ((request.Item.fromdate.Date == DateTime.MinValue ||
                request.Item.todate.Date == DateTime.MinValue) || (request.Item.todate.Subtract(request.Item.fromdate).TotalDays > 45)))
            {
                throw new Exception("Please Select different date range for Analysis , you are restricted becuase of season sale is going");
            }

            var queueManager = InfyPOS.Processors.WorkflowQueueManager.Instance;
            if (!queueManager.Enabled)
            {
                ReportBuilder result = await Task<ReportBuilder>.Run(() => reportdata.BuildReport(request.Item));
                return this.Ok(result);
            }
            else
            {
                InboxDataService ActualDataService = new InboxDataService();
                var organizationId = ActualDataService.Context?.OrganizationID ?? 0;
                var userId = ActualDataService.Context?.UserID.ToString() ?? "";
                var response = await queueManager.ExecuteWithTimeout<object>(async () =>
                {
                    return await Task<ReportBuilder>.Run(() => reportdata.BuildReport(request.Item));
                }, "AnalyserService.GenerateReportBuilder", request.Item, 3, organizationId, userId);

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

        [Route("ProcessReportBuilder")]
        [HttpPost]
        public async Task<IHttpActionResult> ProcessReportBuilder(ActionRequest<ReportBuilder> request)
        {
            ReportBuilderDataService reportdata = new ReportBuilderDataService();
            if (reportdata.HasResctrictedAccess && ((request.Item.fromdate.Date == DateTime.MinValue ||
                request.Item.todate.Date == DateTime.MinValue) || (request.Item.todate.Subtract(request.Item.fromdate).TotalDays > 45)))
            {
                throw new Exception("Please Select different date range for Analysis , you are restricted becuase of season sale is going");
            }
            InfyPOS.Processors.ProcessManager.ProcessData data = new Processors.ProcessManager.ProcessData()
            {
                action = "REPORTBUILDER",
                ReportBuilderRequest = request.Item
            };
            data.Context = reportdata.Context;
            data.request = this.Request;
            InfyPOS.Processors.Migration.ProgressStatus status = await Task<InfyPOS.Processors.Migration.ProgressStatus>.Run(() => StartProcess(reportdata.Context.OrganizationID, data));
            return this.Ok(status);
        }

        private InfyPOS.Processors.Migration.ProgressStatus StartProcess(long organizationid,
        InfyPOS.Processors.ProcessManager.ProcessData data)
        {
            return InfyPOS.Processors.ProcessManager.Instance(organizationid).Start(data);
        }

        [HttpPost]
        [Route("SearchAsOnMfgStock")]
        [WebAPI.InfyAuthorize(PermissionSet.whstockanalysisreport, InfyPOS.Common.Action.View)]
        public async Task<IHttpActionResult> SearchAsOnMfgStock(ActionRequest<InfyPOS.AnalyserDataService.StockAsOnCriteria> request)
        {
            AnalyserDataService WarehouseReportData = new AnalyserDataService();
            var queueManager = InfyPOS.Processors.WorkflowQueueManager.Instance;
            if (!queueManager.Enabled)
            {
                if (request.Item.history)
                {
                    List<AnalyserDataService.StockTransactionAsOn.History> result = await Task<List<AnalyserDataService.StockTransactionAsOn.History>>.Run(() => WarehouseReportData.StockTransactionHistory(request.Item));
                    return this.Ok(result);
                }
                else
                {
                    AnalyserDataService.StockTransactionAsOn result = await Task<AnalyserDataService.StockTransactionAsOn>.Run(() => WarehouseReportData.SearchAsOnMfgStockTransaction(request.Item));
                    return this.Ok(result);
                }
            }
            else
            {
                InboxDataService ActualDataService = new InboxDataService();
                var organizationId = ActualDataService.Context?.OrganizationID ?? 0;
                var userId = ActualDataService.Context?.UserID.ToString() ?? "";
                var response = await queueManager.ExecuteWithTimeout<object>(async () =>
                {
                    if (request.Item.history)
                    {
                        return await Task<List<AnalyserDataService.StockTransactionAsOn.History>>.Run(() => WarehouseReportData.StockTransactionHistory(request.Item));
                    }
                    else
                    {
                        return await Task<AnalyserDataService.StockTransactionAsOn>.Run(() => WarehouseReportData.SearchAsOnMfgStockTransaction(request.Item));
                    }
                }, "AnalyserService.SearchAsOnMfgStock", request.Item, 3, organizationId, userId);

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
        [Route("SearchAsOnStock")]
        [WebAPI.InfyAuthorize(PermissionSet.whstockanalysisreport,
            InfyPOS.Common.Action.View)]
        public async Task<IHttpActionResult> SearchAsOnStock(ActionRequest<InfyPOS.AnalyserDataService.StockAsOnCriteria> request)
        {
            AnalyserDataService WarehouseReportData = new AnalyserDataService();

            var queueManager = InfyPOS.Processors.WorkflowQueueManager.Instance;
            if (!queueManager.Enabled)
            {
                if (request.Item.history)
                {
                    List<AnalyserDataService.StockTransactionAsOn.History> result = await Task<List<AnalyserDataService.StockTransactionAsOn.History>>.Run(() => WarehouseReportData.StockTransactionHistory(request.Item));
                    return this.Ok(result);
                }
                else
                {
                    AnalyserDataService.StockTransactionAsOn result = await Task<AnalyserDataService.StockTransactionAsOn>.Run(() => WarehouseReportData.SearchAsOnStockTransaction(request.Item));
                    return this.Ok(result);
                }
            }
            else
            {
                InboxDataService ActualDataService = new InboxDataService();
                var organizationId = ActualDataService.Context?.OrganizationID ?? 0;
                var userId = ActualDataService.Context?.UserID.ToString() ?? "";
                var response = await queueManager.ExecuteWithTimeout<object>(async () =>
                {

                    if (request.Item.history)
                    {
                        return await Task<List<AnalyserDataService.StockTransactionAsOn.History>>.Run(() => WarehouseReportData.StockTransactionHistory(request.Item));
                    }
                    else
                    {
                        return await Task<AnalyserDataService.StockTransactionAsOn>.Run(() => WarehouseReportData.SearchAsOnStockTransaction(request.Item));
                    }

                }, "AnalyserService.SearchAsOnStock", request.Item, 3, organizationId, userId);

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
        [Route("SaleVsPurchaseVsStock")]
        [WebAPI.InfyAuthorize(PermissionSet.whstockanalysisreport,
    InfyPOS.Common.Action.View)]
        public async Task<IHttpActionResult> SaleVsPurchaseVsStock(ActionRequest<InfyPOS.AnalyserDataService.StockAsOnCriteria> request)
        {
            AnalyserDataService WarehouseReportData = new AnalyserDataService();
            var queueManager = InfyPOS.Processors.WorkflowQueueManager.Instance;
            if (!queueManager.Enabled)
            {
                List<AnalyserDataService.StockTransactionAsOn> result = await Task<List<AnalyserDataService.StockTransactionAsOn>>.Run(() => WarehouseReportData.SaleVsPurchaseVsStock(request.Item));
                return this.Ok(result);

            }
            else
            {
                InboxDataService ActualDataService = new InboxDataService();
                var organizationId = ActualDataService.Context?.OrganizationID ?? 0;
                var userId = ActualDataService.Context?.UserID.ToString() ?? "";
                var response = await queueManager.ExecuteWithTimeout<object>(async () =>
                {
                    return await Task<List<AnalyserDataService.StockTransactionAsOn>>.Run(() => WarehouseReportData.SaleVsPurchaseVsStock(request.Item));
                }, "AnalyserService.SaleVsPurchaseVsStock", request.Item, 3, organizationId, userId);

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

    }
}