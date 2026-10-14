
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
using WebAPI.Data;
namespace InfyPOS.Controllers
{
    [RoutePrefix("TransportpaymentService")]
    public class TransportpaymentServiceController : ApiController
    {

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<TransportPayment>();
            result.Item = new TransportPayment();
            return this.Ok(result);
        }


        [HttpPost]
        [Route("SearchAll")]
        public async Task<IHttpActionResult> SearchAll(ActionRequest<TransportPaymentCreteria> request)
        {
            TransportPaymentDataService TransportPaymentData = new TransportPaymentDataService();
            List<TransportPayment> result = await Task<List<TransportPayment>>.Run(() => TransportPaymentData.SearchAll(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<TransportPaymentCreteria> request)
        {
            TransportPaymentDataService TransportPaymentData = new TransportPaymentDataService();
            List<TransportPayment> result = await Task<List<TransportPayment>>.Run(() => TransportPaymentData.Search(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("SearchPendingPayment")]
        public async Task<IHttpActionResult> SearchPendingPayment(ActionRequest<TransportPaymentCreteria> request)
        {
            TransportPaymentDataService TransportPaymentData = new TransportPaymentDataService();
            List<PendingTransportInvoice> result = await Task<List<PendingTransportInvoice>>.Run(() => TransportPaymentData.SearchPendingPayment(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("SearchSupplierPaymentSummary")]
        public async Task<IHttpActionResult> SearchSupplierPaymentSummary(ActionRequest<TransportPaymentCreteria> request)
        {
            TransportPaymentDataService SupplierpaymentData = new TransportPaymentDataService();
            List<PendingLrInvoice> result = await Task<List<PendingLrInvoice>>.Run(() => SupplierpaymentData.SearchSupplierPaymentSummary(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("SearchSupplierPayment")]
        public async Task<IHttpActionResult> SearchSupplierPayment(ActionRequest<TransportPaymentCreteria> request)
        {
            TransportPaymentDataService TransportPaymentData = new TransportPaymentDataService();
            List<PendingLrInvoice> result = await Task<List<PendingLrInvoice>>.Run(() => TransportPaymentData.SearchSupplierPayment(request.Item));
            return this.Ok(result);
        }
        [HttpPost]
        [Route("Save")]
        public async Task<IHttpActionResult> Save(ActionRequest<TransportPayment> request)
        {
            TransportPaymentDataService TransportPaymentData = new TransportPaymentDataService();
            if (request.Item.id <= 0)
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Create);
                //if (request.Item.TransportPaymentinvoice != null)
                //{
                //    foreach (var item in request.Item.TransportPaymentinvoice)
                //    {
                //        item.ChangeStatus(BaseEntity.DataStatus.Create);
                //    }
                //}
                TransportPayment result = await Task<TransportPayment>.Run(() => TransportPaymentData.Insert(request.Item));
                return this.Ok(result);
            }
            else
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Update);
                //if (request.Item.TransportPaymentinvoice != null)
                //{
                //    foreach (var item in request.Item.TransportPaymentinvoice)
                //    {
                //        item.ChangeStatus(BaseEntity.DataStatus.Update);
                //    }
                //}
                TransportPayment result = await Task<TransportPayment>.Run(() => TransportPaymentData.Update(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("Insert")]
        public async Task<IHttpActionResult> Insert(ActionRequest<TransportPayment> request)
        {
            TransportPaymentDataService TransportPaymentData = new TransportPaymentDataService();
            TransportPayment result = await Task<TransportPayment>.Run(() => TransportPaymentData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        public async Task<IHttpActionResult> Update(ActionRequest<TransportPayment> request)
        {
            TransportPaymentDataService TransportPaymentData = new TransportPaymentDataService();
            TransportPayment result = await Task<TransportPayment>.Run(() => TransportPaymentData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IHttpActionResult> Delete(ActionRequest<TransportPayment> request)
        {
            TransportPaymentDataService TransportPaymentData = new TransportPaymentDataService();
            bool result = await Task<bool>.Run(() => TransportPaymentData.Delete(request.Item));
            return this.Ok(result);
        }

        [HttpGet]
        [Route("TransportLedger/{companyid}/{id}/{from}/{to}")]
        public async Task<IHttpActionResult> TransportLedger(string companyid,
    string id, string from, string to)
        {
            TransportPaymentDataService SupplierpaymentData = new TransportPaymentDataService();
            PrintableLedger printable = await Task<FileContentInfo>.Run(() =>
            {
                return SupplierpaymentData.PrintSummary(long.Parse(id), long.Parse(companyid),
                BaseEntity.GetDataFromString(from), BaseEntity.GetDataFromString(to));
            });
            return this.Ok(printable);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("PrintSummaryPDF/{organizationid}/{companyid}/{id}/{from}/{to}")]
        public async Task<IHttpActionResult> PrintSummaryPDF(string organizationid, string companyid,
            string id, string from, string to)
        {
            TransportPaymentDataService SupplierpaymentData = new TransportPaymentDataService();
            SupplierpaymentData.Context = new UserClaim()
            {
                OrganizationID = long.Parse(organizationid)
            };
            try
            {
                FileContentInfo result = await Task<FileContentInfo>.Run(() =>
                {
                    PrintableLedger print = SupplierpaymentData.PrintSummary(long.Parse(id), long.Parse(companyid),
                      BaseEntity.GetDataFromString(from), BaseEntity.GetDataFromString(to));
                    if (print.transactions != null &&
                    print.transactions.Count > 0)
                    {
                        print.transactions.ForEach(titem =>
                        {
                            if (!string.IsNullOrEmpty(titem.description) &&
                                    titem.description.Length > 50)
                                titem.description = titem.description.Substring(0, 50) + "..";
                        });
                    }
                    LedgerServiceController ledgerServiceController = new LedgerServiceController();
                    return ledgerServiceController.PrintLedgerPDF(SupplierpaymentData.Context.OrganizationID, print);
                });
                return new FileActionResult(false, "", result.Extension, result.Content);
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(SupplierpaymentData.Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, "PrintCheuqeAdvicePDF/" + id);
                return this.Ok(exp);
            }
        }


        [AllowAnonymous]
        [HttpGet]
        [Route("PrintCheuqeAdvicePDF/{organizationid}/{id}")]
        public async Task<IHttpActionResult> PrintCheuqeAdvicePDF(string organizationid, string id)
        {
            TransportPaymentDataService TransportpaymentData = new TransportPaymentDataService();
            TransportpaymentData.Context = new UserClaim()
            {
                OrganizationID = long.Parse(organizationid)
            };
            try
            {
                FileContentInfo result = await Task<FileContentInfo>.Run(() =>
                {
                    TransportPaymentPrint print = TransportpaymentData.Print(new TransportPayment() { id = long.Parse(id) });
                    return PrintChequeAdvice(TransportpaymentData.Context.OrganizationID, print);
                });
                return new FileActionResult(false, "", result.Extension, result.Content);
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(TransportpaymentData.Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, "PrintCheuqeAdvicePDF/" + id);
                return this.Ok(exp);
            }
        }
        private FileContentInfo PrintChequeAdvice(long organizationid, TransportPaymentPrint transportPaymentPrint)
        {
            InfyPOS.Data.ConfigurationDataService config = new ConfigurationDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            var lstconfig = config.Search(new ConfigurationCreteria
            {
                key = transportPaymentPrint.sourcemode == 0 ?  "TRANSPORTPAYMENT" : "COURIERPAYMENT"
            });

            if (lstconfig == null || lstconfig.Count == 0)
                return null;

            ReportLibrary.LaserReporter labreport = new ReportLibrary.LaserReporter(organizationid);
            labreport.format = ReportLibrary.DosFormatParser.ParseXML(organizationid, lstconfig[0].reportformat.format);
            labreport.DefaultPageSettings = labreport.format.GetDefaultSetting();
            labreport.Additional = new SortedDictionary<string, object>();
            ReportLibrary.PDFPrinter printer = new ReportLibrary.PDFPrinter();
            printer.Report = labreport;
            printer.EnablePageNumber = lstconfig[0].reportformat.format.Contains("PageCount");
            labreport.DataSource = transportPaymentPrint;

            List<string> imageids = labreport.format.GetImages();
            var imageData = new Common.ImageData() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            foreach (var imgid in imageids.FindAll(x => x.Length > 6))
            {
                var file = imageData.GetData(imgid.LongFromBase62());
                if (file != null)
                {
                    var img = imageData.FromBytes(file.data);
                    labreport.Additional.Add("IMG-" + imgid, img);
                }
            }
            if (transportPaymentPrint.company != null && !string.IsNullOrEmpty(transportPaymentPrint.company.logo))
            {
                var file = imageData.GetData(transportPaymentPrint.company.logo.LongFromBase62());
                if (file != null && file.data != null)
                {
                    var img = imageData.FromBytes(file.data);
                    labreport.Additional.Add("LOGO", img);
                }
            }

            using (System.IO.MemoryStream stream = printer.Print(labreport.format.GetDefaultSetting()))
            {
                var fci = new FileContentInfo(new ByteArrayContent(stream.ToArray()));
                fci.FileName = "paymentadvice.pdf";
                fci.Extension = "application/pdf";
                return fci;
            }

        }
    }
}