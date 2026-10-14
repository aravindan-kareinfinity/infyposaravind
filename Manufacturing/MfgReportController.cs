
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
    [RoutePrefix("MfgReportService")]
    public class MfgReportServiceController : ApiController
    {

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<Mfgdesign>();
            result.Item = new Mfgdesign();
            return this.Ok(result);
        }


        [HttpPost]
        [Route("SearchUIDStock")]
        public async Task<IHttpActionResult> SearchUIDStock(ActionRequest<StockReportCreteria> request)
        {
            MfgReportDataService MfgdesignData = new MfgReportDataService();
            if (request.Item.reporttypeid == 0)
            {
                List<MfgReportDataService.WIPTrace> result = await Task<List<MfgReportDataService.WIPTrace>>.Run(() => MfgdesignData.SearchUIDWorkorder(request.Item));
                return this.Ok(result);
            }
            if (request.Item.reporttypeid == 1)
            {
                List<MfgReportDataService.GRNTrace> result = await Task<List<MfgReportDataService.GRNTrace>>.Run(() => MfgdesignData.SearchUIDGRN(request.Item));
                return this.Ok(result);
            }
            if (request.Item.reporttypeid == 2)
            {
                List<MfgReportDataService.StockTrace> result = await Task<List<MfgReportDataService.StockTrace>>.Run(() => MfgdesignData.SearchUIDStock(request.Item));
                return this.Ok(result);
            }
            if (request.Item.reporttypeid == 3)
            {
                List<MfgReportDataService.SalesTrace> result = await Task<List<MfgReportDataService.SalesTrace>>.Run(() => MfgdesignData.SearchUIDSales(request.Item));
                return this.Ok(result);
            }
            return this.NotFound();
        }


        [HttpPost]
        [Route("SaveDesignwisePlan")]
        public async Task<IHttpActionResult> SaveDesignwisePlan(ActionMulitpleRequest<PlanedItem> request)
        {
            MfgReportDataService MfgdesignData = new MfgReportDataService();
            var queueManager = InfyPOS.Processors.WorkflowQueueManager.Instance;
            if (!queueManager.Enabled)
            {
                bool result = await Task<bool>.Run(() => MfgdesignData.SaveDesignwisePlan(request.Items));
                return this.Ok(result);
            }
            else
            {
                InboxDataService ActualDataService = new InboxDataService();
                var organizationId = ActualDataService.Context?.OrganizationID ?? 0;
                var userId = ActualDataService.Context?.UserID.ToString() ?? "";
                var response = await queueManager.ExecuteWithTimeout<object>(async () =>
                {
                    return await Task<bool>.Run(() => MfgdesignData.SaveDesignwisePlan(request.Items));
                }, "MfgReportService.SaveDesignwisePlan", request.Items, 3, organizationId, userId);

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
        [Route("SearchMfgUANStock")]
        public async Task<IHttpActionResult> SearchMfgUANStock(ActionRequest<CustomStockCriteria> request)
        {
            MfgReportDataService MfgdesignData = new MfgReportDataService();

            var queueManager = InfyPOS.Processors.WorkflowQueueManager.Instance;
            if (!queueManager.Enabled)
            {
                List<MfgReportServiceController.CustomStockCriteria.StockResult> result = await Task<List<MfgReportServiceController.CustomStockCriteria.StockResult>>.Run(() => MfgdesignData.SearchMfgUANStock(request.Item));
                return this.Ok(result);            }
            else
            {
                InboxDataService ActualDataService = new InboxDataService();
                var organizationId = ActualDataService.Context?.OrganizationID ?? 0;
                var userId = ActualDataService.Context?.UserID.ToString() ?? "";
                var response = await queueManager.ExecuteWithTimeout<object>(async () =>
                {
                    return await Task<List<MfgReportServiceController.CustomStockCriteria.StockResult>>.Run(() => MfgdesignData.SearchMfgUANStock(request.Item));
                }, "MfgReportService.SearchMfgUANStock", request.Item, 3, organizationId, userId);

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
        [Route("SearchUANSalesReport")]
        public async Task<IHttpActionResult> SearchUANSalesReport(ActionRequest<CustomStockCriteria> request)
        {
            MfgReportDataService MfgdesignData = new MfgReportDataService();
            

            var queueManager = InfyPOS.Processors.WorkflowQueueManager.Instance;
            if (!queueManager.Enabled)
            {
                List<MfgReportServiceController.CustomStockCriteria.SalesReport> result = await Task<List<MfgReportServiceController.CustomStockCriteria.StockResult>>.Run(() => MfgdesignData.SearchUANSalesReport(request.Item));
                return this.Ok(result);
            }
            else
            {
                InboxDataService ActualDataService = new InboxDataService();
                var organizationId = ActualDataService.Context?.OrganizationID ?? 0;
                var userId = ActualDataService.Context?.UserID.ToString() ?? "";
                var response = await queueManager.ExecuteWithTimeout<object>(async () =>
                {
                    return await Task<List<MfgReportServiceController.CustomStockCriteria.StockResult>>.Run(() => MfgdesignData.SearchUANSalesReport(request.Item));
                }, "MfgReportService.SearchUANSalesReport", request.Item, 3, organizationId, userId);

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
        [Route("SearchUANPOReport")]
        public async Task<IHttpActionResult> SearchUANPOReport(ActionRequest<CustomStockCriteria> request)
        {
            MfgReportDataService MfgdesignData = new MfgReportDataService();

            var queueManager = InfyPOS.Processors.WorkflowQueueManager.Instance;
            if (!queueManager.Enabled)
            {
                List<MfgReportServiceController.CustomStockCriteria.POResult> result = await Task<List<MfgReportServiceController.CustomStockCriteria.POResult>>.Run(() => MfgdesignData.SearchUANPOReport(request.Item));
                return this.Ok(result);
            }
            else
            {
                InboxDataService ActualDataService = new InboxDataService();
                var organizationId = ActualDataService.Context?.OrganizationID ?? 0;
                var userId = ActualDataService.Context?.UserID.ToString() ?? "";
                var response = await queueManager.ExecuteWithTimeout<object>(async () =>
                {
                    return await Task<List<MfgReportServiceController.CustomStockCriteria.POResult>>.Run(() => MfgdesignData.SearchUANPOReport(request.Item));
                }, "MfgReportService.SearchUANPOReport", request.Item, 3, organizationId, userId);

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
        [Route("SearchUANSOReport")]
        public async Task<IHttpActionResult> SearchUANSOReport(ActionRequest<CustomStockCriteria> request)
        {
            MfgReportDataService MfgdesignData = new MfgReportDataService();

            var queueManager = InfyPOS.Processors.WorkflowQueueManager.Instance;
            if (!queueManager.Enabled)
            {
                List<MfgReportServiceController.CustomStockCriteria.SOResult> result = await Task<List<MfgReportServiceController.CustomStockCriteria.SOResult>>.Run(() => MfgdesignData.SearchUANSOReport(request.Item));
                return this.Ok(result);
            }
            else
            {
                InboxDataService ActualDataService = new InboxDataService();
                var organizationId = ActualDataService.Context?.OrganizationID ?? 0;
                var userId = ActualDataService.Context?.UserID.ToString() ?? "";
                var response = await queueManager.ExecuteWithTimeout<object>(async () =>
                {
                    return await Task<List<MfgReportServiceController.CustomStockCriteria.SOResult>>.Run(() => MfgdesignData.SearchUANSOReport(request.Item));
                }, "MfgReportService.SearchUANSOReport", request.Item, 3, organizationId, userId);

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
        [Route("SearchMfgUANBundleReport")]
        public async Task<IHttpActionResult> SearchMfgUANBundleReport(ActionRequest<CustomStockCriteria> request)
        {
            MfgReportDataService MfgdesignData = new MfgReportDataService();
            List<MfgReportServiceController.CustomStockCriteria.BundleResult> result = await Task<List<MfgReportServiceController.CustomStockCriteria.BundleResult>>.Run(() => MfgdesignData.SearchMfgUANBundleReport(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("SearchWorkorderSampleStatus")]
        public async Task<IHttpActionResult> SearchWorkorderSampleStatus(ActionRequest<MfgReportDataService.WorkorderSampleStatusRequest> request)
        {
            MfgReportDataService MfgdesignData = new MfgReportDataService();
            List<MfgReportDataService.WorkorderSampleStatus> result = await Task<List<MfgReportDataService.WorkorderSampleStatus>>.Run(() => MfgdesignData.SearchWorkorderSampleStatus(request.Item));
            return this.Ok(result);
        }


        [HttpPost]
        [Route("SearchMfgUANInvoice")]
        public async Task<IHttpActionResult> SearchMfgUANInvoice(ActionRequest<CustomStockCriteria> request)
        {
            MfgReportDataService MfgdesignData = new MfgReportDataService();
            List<MfgReportServiceController.CustomStockCriteria.InvoiceResult> result = await Task<List<MfgReportServiceController.CustomStockCriteria.InvoiceResult>>.Run(() => MfgdesignData.SearchMfgUANInvoice(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("ClosePendingPOitems")]
        public async Task<IHttpActionResult> ClosePendingPOitems(ActionMulitpleRequest<PlanedItem> request)
        {
            MfgReportDataService MfgdesignData = new MfgReportDataService();
            bool result = await Task<bool>.Run(() => MfgdesignData.ClosePendingPOitems(request.Items));
            return this.Ok(result);
        }


        [HttpPost]
        [Route("SearchDesignwiseStock")]
        public async Task<IHttpActionResult> SearchDesignwiseStock(ActionRequest<StockReportCreteria> request)
        {
            MfgReportDataService MfgdesignData = new MfgReportDataService();

            var queueManager = InfyPOS.Processors.WorkflowQueueManager.Instance;
            if (!queueManager.Enabled)
            {
                StockReport result = await Task<StockReport>.Run(() => MfgdesignData.SearchDesignwiseStock(request.Item));
                return this.Ok(result);
            }
            else
            {
                InboxDataService ActualDataService = new InboxDataService();
                var organizationId = ActualDataService.Context?.OrganizationID ?? 0;
                var userId = ActualDataService.Context?.UserID.ToString() ?? "";
                var response = await queueManager.ExecuteWithTimeout<object>(async () =>
                {
                    return await Task<StockReport>.Run(() => MfgdesignData.SearchDesignwiseStock(request.Item));
                }, "MftReportService.SearchDesignwiseStock", request.Item, 3, organizationId, userId);

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
        [Route("SearchDesignwiseStock4Planner")]
        public async Task<IHttpActionResult> SearchDesignwiseStock4Planner(ActionRequest<StockReportCreteria> request)
        {
            MfgReportDataService MfgdesignData = new MfgReportDataService();

            var queueManager = InfyPOS.Processors.WorkflowQueueManager.Instance;
            if (!queueManager.Enabled)
            {
                PlanningReport result = await Task<PlanningReport>.Run(() => MfgdesignData.SearchDesignwiseStock4Planner(request.Item));
                return this.Ok(result);
            }
            else
            {
                InboxDataService ActualDataService = new InboxDataService();
                var organizationId = ActualDataService.Context?.OrganizationID ?? 0;
                var userId = ActualDataService.Context?.UserID.ToString() ?? "";
                var response = await queueManager.ExecuteWithTimeout<object>(async () =>
                {
                    return await Task<PlanningReport>.Run(() => MfgdesignData.SearchDesignwiseStock4Planner(request.Item));
                }, "MfgReportService.SearchDesignwiseStock4Planner", request.Item, 3, organizationId, userId);

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
        [Route("SearchInGallery")]
        public async Task<IHttpActionResult> SearchInGallery(ActionRequest<MfgGalleryCreteria> request)
        {
            MfgReportDataService MfgdesignData = new MfgReportDataService();
            MfgGallery result = await Task<MfgGallery>.Run(() => MfgdesignData.SearchInGallery(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("SearchLocationwiseStock")]
        public async Task<IHttpActionResult> SearchLocationwiseStock(ActionRequest<StockReportCreteria> request)
        {
            MfgReportDataService MfgdesignData = new MfgReportDataService();

            var queueManager = InfyPOS.Processors.WorkflowQueueManager.Instance;
            if (!queueManager.Enabled)
            {
                StockLocationReport result = await Task<StockLocationReport>.Run(() => MfgdesignData.SearchLocationwiseStock(request.Item));
                return this.Ok(result);
            }
            else
            {
                InboxDataService ActualDataService = new InboxDataService();
                var organizationId = ActualDataService.Context?.OrganizationID ?? 0;
                var userId = ActualDataService.Context?.UserID.ToString() ?? "";
                var response = await queueManager.ExecuteWithTimeout<object>(async () =>
                {
                    return await Task<StockLocationReport>.Run(() => MfgdesignData.SearchLocationwiseStock(request.Item));
                }, "MfgReportService.SearchLocationwiseStock", request.Item, 3, organizationId, userId);

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
        [Route("ImagePDFGenerationStart")]
        public async Task<IHttpActionResult> ImagePDFGenerationStart(ActionRequest<MfgGalleryCreteria.ImagePDFRequest> request)
        {
            PDFGenerator pdfgenerator = new PDFGenerator();
            var context = System.Web.HttpContext.Current;
            string key = await Task<byte[]>.Run(() => pdfgenerator.Initiate(context, request.Item));
            return this.Ok(key);
        }


        [HttpGet]
        [Route("ImagePDFGenerationStatus/{key}")]
        public async Task<IHttpActionResult> ImagePDFGenerationStatus(string key)
        {
            try
            {
                PDFGenerator pdfgenerator = new PDFGenerator();
                PDFGenerator.Status status = await Task<PDFGenerator.Status>.Run(() => pdfgenerator.QueueStatus(key));
                return this.Ok(status);
            }
            catch (Exception exp)
            {
                return this.Ok(exp);
            }
        }


        [HttpPost]
        [Route("ImagePDFGeneration")]
        public async Task<IHttpActionResult> ImagePDFGeneration(ActionRequest<MfgGalleryCreteria.ImagePDFRequest> request)
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

        public class StockLocationReport
        {
            [Newtonsoft.Json.JsonIgnore]
            public List<LocationStock> items { get; set; }
            public List<string> sizenames { get; set; }
            public List<long> sizeids { get; set; }

            public class LocationStock
            {
                public long id { get; set; }
                public List<DesignStock> items { get; set; }
            }
            public List<DesignItemSOWIP> designs { get; set; }
            [Newtonsoft.Json.JsonIgnore]
            public List<DesignStock> so { get; set; }
            [Newtonsoft.Json.JsonIgnore]
            public List<DesignStock> wip { get; set; }
            public class DesignItem
            {
                public int id { get; set; }
                public string name { get; set; }
                public string barcode { get; set; }
                public string colour { get; set; }
                public long brandid { get; set; }
                public long fabricid { get; set; }
                public long productid { get; set; }
                public string designid { get; set; }
            }
            public class DesignStock : DesignItem
            {
                public decimal[] stock { get; set; }
            }

            public class DesignItemSOWIP : DesignItem
            {
                public decimal[] wip { get; set; }
                public decimal[] so { get; set; }
                public List<LocationItem> stock { get; set; }
            }


            public class LocationItem
            {
                public long locationid { get; set; }
                public decimal[] stock { get; set; }
            }

            internal void creategroup()
            {
                List<DesignStock> alldesigns = new List<DesignStock>();
                foreach (var item in items)
                {
                    alldesigns.AddRange(item.items);
                }
                alldesigns.AddRange(so);
                alldesigns.AddRange(wip);
                designs = alldesigns.GroupBy(x => new { x.designid, x.name, x.barcode, x.colour, x.brandid, x.fabricid, x.productid }).ToList().ConvertAll(e =>
                {
                    return new DesignItemSOWIP()
                    {
                        barcode = e.Key.barcode,
                        name = e.Key.name,
                        designid = e.Key.designid,
                        colour = e.Key.colour,
                        brandid = e.Key.brandid,
                        fabricid = e.Key.fabricid,
                        productid = e.Key.productid
                    };
                });
                wip.ForEach(x =>
                {
                    var dindex = designs.FindIndex(y => y.barcode == x.barcode &&
                     y.name == x.name &&
                     y.designid == x.designid &&
                     y.colour == x.colour &&
                     y.brandid == x.brandid &&
                     y.fabricid == x.fabricid &&
                     y.productid == x.productid);
                    if (dindex >= 0)
                        designs[dindex].wip = x.stock;
                });
                so.ForEach(x =>
                {
                    var dindex = designs.FindIndex(y => y.barcode == x.barcode &&
                     y.name == x.name &&
                     y.designid == x.designid &&
                     y.colour == x.colour &&
                     y.brandid == x.brandid &&
                     y.fabricid == x.fabricid &&
                     y.productid == x.productid);
                    if (dindex >= 0)
                        designs[dindex].so = x.stock;
                });
                int index = 0;
                designs.ForEach(x =>
                {
                    x.id = index++;
                    x.stock = new List<LocationItem>();
                    foreach (var litem in items)
                    {
                        var dindex = litem.items.FindIndex(y => y.barcode == x.barcode &&
                     y.name == x.name &&
                     y.designid == x.designid &&
                     y.colour == x.colour &&
                     y.brandid == x.brandid &&
                     y.fabricid == x.fabricid &&
                     y.productid == x.productid);
                        if (dindex >= 0)
                        {
                            x.stock.Add(new LocationItem() { locationid = litem.id, stock = litem.items[dindex].stock });
                        }
                    }
                });
            }
        }

        public class CustomStockCriteria : BaseEntity
        {
            public DateTime fromdate { get; set; }
            public DateTime todate { get; set; }
            public int posearchmode { get; set; }
            public long prddivisionid { get; set; }
            public long productid { get; set; }
            public string pono { get; set; }
            public string uid { get; set; }
            public string designid { get; set; }
            public long classificationid { get; set; }
            public long companyid { get; set; }
            public long customerid { get; set; }
            public long categoryid { get; set; }
            public long locationid { get; set; }
            public List<CustomField> customfields { get; set; }
            public class CustomField
            {
                public string display { get; set; }
                public Referencelist.CustomField field { get; set; }
            }

            public class Result
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
            public class StockResult : Result
            {
                public decimal cost { get; set; }
                public decimal stock { get; set; }
                public decimal so { get; set; }
                public decimal sto { get; set; }
            }
            public class BundleResult : Result
            {
                public string code { get; set; }
                public DateTime packedon { get; set; }
                public DateTime receivedon { get; set; }
                public int status { get; set; }
                public long fromlocationid { get; set; }
                public long tolocationid { get; set; }
                public long packedby { get; set; }
                public long receivedby { get; set; }
                public decimal acceptedqty { get; set; }
                public decimal rejectedqty { get; set; }
                public decimal skippedqty { get; set; }
                public decimal requestqty { get; set; }
                public decimal issuedqty { get; set; }
            }
            public class SalesReport : Result
            {
                public string hsn { get; set; }
                public string customer { get; set; }
                public string sono { get; set; }
                public string morderid { get; set; }
                public string channel { get; set; }
                public string billno { get; set; }
                public DateTime billdate { get; set; }
                public string pos { get; set; }
                public string category { get; set; }
                public bool isinterstatesale { get; set; }
                public decimal qty { get; set; }
                public decimal receivable { get; set; }
                public decimal tax { get; set; }
                public decimal schemediscount { get; set; }
                public decimal additionaldiscount { get; set; }
                public decimal taxpercentage { get; set; }
                public string einvoicecode { get;  set; }
            }
            public class SOResult : Result
            {
                public string customer { get; set; }
                public decimal orderqty { get; set; }
                public decimal billedqty { get; set; }
                public decimal rejecteddqty { get; set; }
                public decimal cancelledqty { get; set; }
                public string sono { get; set; }
                public decimal price { get; set; }
                public DateTime sodate { get; set; }
                public DateTime expecteddate { get; set; }
                public string remarks { get; set; }
                public string sobookno { get; set; }
            }

            public class POResult : Result
            {
                public string supplier { get; set; }
                public decimal orderqty { get; set; }
                public decimal receivedqty { get; set; }
                public decimal cancelledqty { get; set; }
                public string pono { get; set; }
                public decimal price { get; set; }
                public DateTime podate { get; set; }
                public DateTime expecteddate { get; set; }
                public DateTime expirydate { get; set; }
                public DateTime amenddate { get; set; }
                public string amendreason { get; set; }
            }

            public class InvoiceResult : Result
            {
                public string supplier { get; set; }
                public string pono { get; set; }
                public DateTime podate { get; set; }
                public decimal poqty { get; set; }
                public string invoiceno { get; set; }
                public string entryno { get; set; }
                public DateTime entrydate { get; set; }
                public DateTime invoicedate { get; set; }
                public decimal buyingprice { get; set; }
                public decimal orderqty { get; set; }
                public decimal actqty { get; set; }
                public decimal grnqty { get; set; }
                public decimal grnvalue { get; set; }
                public decimal grntvalue { get; set; }
                public decimal debitqty { get; set; }
                public decimal debitvalue { get; set; }
            }
        }
        public class PlanedItem : BaseEntity
        {
            public string sono { get; set; }
            public string designcode { get; set; }
            public long sid { get; set; }
            public long customerid { get; set; }
            public decimal pqty { get; set; }
        }
        public class PlanningReport
        {
            public List<DesignStock> stock { get; set; }
            public List<SalesOrder> salesorder { get; set; }
            public List<string> sizenames { get; set; }
            public List<long> sizeids { get; set; }

            public class SalesOrder
            {
                public string barcode { get; set; }
                public decimal fabricstock { get; set; }
                public decimal forecast { get; set; }
                public string colour { get; set; }
                public long colourid { get; set; }
                public long brandid { get; set; }
                public long fabricid { get; set; }
                public long productid { get; set; }
                public long customerid { get; set; }
                public long cityid { get; set; }
                public string designcode { get; set; }
                public string sono { get; set; }
                public decimal[] so { get; set; }
                public decimal[] pn { get; set; }
            }
            public class DesignStock
            {
                public string name { get; set; }
                public decimal[] wip { get; set; }
                public decimal[] sk { get; set; }
                public string designid { get; set; }
            }
        }
        public class StockReport
        {
            public List<DesignStock> erroritems { get; set; }
            public List<DesignStock> items { get; set; }
            public List<string> sizenames { get; set; }
            public List<long> sizeids { get; set; }

            public class DesignStock
            {
                public string name { get; set; }
                public string barcode { get; set; }
                public decimal fabricstock { get; set; }
                public decimal fabricpo { get; set; }
                public decimal forecast { get; set; }
                public decimal[] price { get; set; }
                public string colour { get; set; }
                public long colourid { get; set; }
                public long brandid { get; set; }
                public long fabricid { get; set; }
                public long productid { get; set; }
                public string racknumber { get; set; }
                public decimal[] so { get; set; }
                public decimal[] wip { get; set; }
                public decimal[] stock { get; set; }
                public decimal[] pur { get; set; }
                public decimal[] sal { get; set; }
                public decimal[] pck { get; set; }
                public string designid { get; set; }
                public decimal bommin { get; set; }
                public decimal bommax { get; set; }
                public decimal fgromin { get; set; }
                public decimal fgromax { get; set; }

            }
        }
        public class StockReportCreteria : BaseEntity
        {
            public int rolmode { get; set; }
            public long reporttypeid { get; set; }
            public long locationid { get; set; }
            public long customerid { get; set; }
            public long companyid { get; set; }
            public long sizegroupid { get; set; }
            public string possizelist { get; set; }
            public long sectionid { get; set; }
            public long brandid { get; set; }
            public string designid { get; set; }
            public long productid { get; set; }
            public string colourcode { get; set; }
            public string uid { get; set; }
            public bool ShowSO { get; set; }
            public bool ShowWIP { get; set; }
            public bool ShowSOApproved { get; set; }
            public bool ShowFabric { get; set; }
            public bool ShowROL { get; set; }
            public bool ShowPrice { get; set; }
            public bool ShowRemaining { get; set; }
            public bool ShowPacking { get; set; }
            public bool ShowSales { get; set; }
            public bool ShowPurchase { get; set; }
            public DateTime fromdate { get; set; }
            public DateTime todate { get; set; }
            public List<long> locations { get; set; }

        }

        public class WorkOrdreTraceCriteria
        {
            public string designitemname { get; set; }
            public DateTime fromdate { get; set; }
            public DateTime todate { get; set; }
        }
    }
}