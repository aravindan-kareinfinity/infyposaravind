
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
using InfyPOS.Processors.Migration;
using PdfSharp.Pdf.IO;
using PdfSharp.Pdf;
using System.IO;
using Newtonsoft.Json;
using WebAPI.Data;

namespace InfyPOS.Controllers
{
    [RoutePrefix("BillinboxService")]
    public class BillinboxServiceController : ApiController
    {

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<Billinbox>();
            result.Item = new Billinbox();
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<BillinboxCreteria> request)
        {
            BillinboxDataService BillinboxData = new BillinboxDataService();
            if (request.Item.status == 10)
            {
                List<BillinboxHandovers> result = await Task<List<BillinboxHandovers>>.Run(() => BillinboxData.SearchHandovers(request.Item));
                return this.Ok(result);
            }
            else
            {
                List<Billinbox> result = await Task<List<Billinbox>>.Run(() => BillinboxData.Search(request.Item));
                return this.Ok(result);
            }
        }
        [HttpPost]
        [Route("UpdateStatus")]
        public async Task<IHttpActionResult> UpdateStatus(ActionMulitpleRequest<BillinboxStatus> request)
        {
            BillinboxDataService InboxData = new BillinboxDataService();
            List<BillinboxStatus> result = await Task<List<BillinboxStatus>>.Run(() => InboxData.ChangeStatus(request.Items));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Save")]
        public async Task<IHttpActionResult> Save(ActionRequest<Billinbox> request)
        {
            BillinboxDataService BillinboxData = new BillinboxDataService();
            if (request.Item.id <= 0)
            {
                Billinbox result = await Task<Billinbox>.Run(() => BillinboxData.Insert(request.Item));
                return this.Ok(result);
            }
            else
            {
                Billinbox result = await Task<Billinbox>.Run(() => BillinboxData.Update(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("SaveHandover")]
        public async Task<IHttpActionResult> SaveHandover(ActionMulitpleRequest<Billinbox> request)
        {
            BillinboxDataService BillinboxData = new BillinboxDataService();
            bool result = await Task<bool>.Run(() => BillinboxData.SaveHandover(request.Items));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Publish")]
        public async Task<IHttpActionResult> Publish(ActionRequest<Billinbox> request)
        {
            BillinboxDataService BillinboxData = new BillinboxDataService();
            bool result = await Task<bool>.Run(() => BillinboxData.Publish(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("GenerateEInvoice")]
        public async Task<IHttpActionResult> GenerateEInvoice(ActionRequest<BillinboxCreteria> request)
        {
            BillinboxDataService BillinboxData = new BillinboxDataService();
            InfyGST.GSTPortal.EInvoiceResponse result = await Task<InfyGST.GSTPortal.EInvoiceResponse>.Run(() => BillinboxData.GenerateEInvoice(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("GenerateEWay")]
        public async Task<IHttpActionResult> GenerateEWay(ActionRequest<BillinboxCreteria> request)
        {
            BillinboxDataService BillinboxData = new BillinboxDataService();
            InfyGST.GSTPortal.EWAYbillResponse result = await Task<InfyGST.GSTPortal.EWAYbillResponse>.Run(() => BillinboxData.GenerateEWay(request.Item));
            return this.Ok(result);
        }
        [HttpPost]
        [Route("CancelEway")]
        public async Task<IHttpActionResult> CancelEway(ActionRequest<BillinboxCreteria> request)
        {
            BillinboxDataService BillinboxData = new BillinboxDataService();
            InfyGST.GSTPortal.EInvoiceResponse result = await Task<InfyGST.GSTPortal.EInvoiceResponse>.Run(() => BillinboxData.CancelEway(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("CancelEInvoice")]
        public async Task<IHttpActionResult> CancelEInvoice(ActionRequest<BillinboxCreteria> request)
        {
            BillinboxDataService BillinboxData = new BillinboxDataService();
            InfyGST.GSTPortal.EInvoiceResponse result = await Task<InfyGST.GSTPortal.EInvoiceResponse>.Run(() => BillinboxData.CancelEInvoice(request.Item));
            return this.Ok(result);
        }


        [HttpPost]
        [Route("Insert")]
        public async Task<IHttpActionResult> Insert(ActionRequest<Billinbox> request)
        {
            BillinboxDataService BillinboxData = new BillinboxDataService();
            Billinbox result = await Task<Billinbox>.Run(() => BillinboxData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        public async Task<IHttpActionResult> Update(ActionRequest<Billinbox> request)
        {
            BillinboxDataService BillinboxData = new BillinboxDataService();
            Billinbox result = await Task<Billinbox>.Run(() => BillinboxData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IHttpActionResult> Delete(ActionRequest<Billinbox> request)
        {
            BillinboxDataService BillinboxData = new BillinboxDataService();
            bool result = await Task<bool>.Run(() => BillinboxData.Delete(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("SaveUANECOM")]
        public async Task<IHttpActionResult> SaveUANECOM(ActionRequest<ECOMUANUploaded> request)
        {
            BillinboxDataService BillinboxData = new BillinboxDataService();
            ProgressStatus result = await Task<ProgressStatus>.Run(() => BillinboxData.SaveUANECOM(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("ChangeBillNo")]
        public async Task<IHttpActionResult> ChangeBillNo(ActionRequest<BillNoChange> request)
        {
            BillinboxDataService BillinboxData = new BillinboxDataService();
            string result = await Task<string>.Run(() => BillinboxData.ChangeBillNo(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("PrintPDF")]
        public async Task<IHttpActionResult> PrintPDF(ActionMulitpleRequest<Billinbox> request)
        {
            var handovers = new BillinboxHandovers()
            {
                items = request.Items,
                transportname = request.Items[0].transportname,
                handoveron = request.Items[0].handedoveron
            };
            BillinboxDataService BillinboxData = new BillinboxDataService();
            int sno = 1;
            handovers.items.ForEach(x => x.sno = sno++);
            ReferencelistDataService referencelistDataService = new ReferencelistDataService() { Context = BillinboxData.Context };
            var handoverpersons = referencelistDataService.SearchList(new ReferencelistCreteria() { id = request.Items[0].handedtoid });
            handovers.handoverperson = handoverpersons.Count == 1 ? handoverpersons[0].name : "";
            CompanyDataService companyDataService = new CompanyDataService() { Context= BillinboxData.Context };
            var companies = companyDataService.Search(new CompanyCreteria() { id = request.Items[0].companyid });
            handovers.company = companies[0];
            var locations = referencelistDataService.Search(new ReferencelistCreteria() { id = request.Items[0].billlocationid });
            handovers.location = new CompanyLocation(locations[0]);
            FileContentInfo byteArray = PrintDebitNote(BillinboxData.Context.OrganizationID, handovers);
            return new FileActionResult(false, "", byteArray.Extension, byteArray.Content);
        }

        public FileContentInfo PrintDebitNote(long organizationid, BillinboxHandovers result)
        {
            if (result == null)
            {
                var fci = new FileContentInfo(new ByteArrayContent(new byte[0]));
                fci.FileName = "deliverypersonslip.pdf";
                fci.Extension = "application/pdf";
                return fci;
            }
            InfyPOS.Data.ConfigurationDataService config = new ConfigurationDataService();
            config.Context = new UserClaim()
            {
                OrganizationID = organizationid
            };
            var lstconfig = config.Search(new ConfigurationCreteria
            {
                key = "DELIVERYPERSONSLIP"
            });

            if (lstconfig == null || lstconfig.Count == 0)
            {
                var fci = new FileContentInfo(new ByteArrayContent(new byte[0]));
                fci.FileName = "deliverypersonslip.pdf";
                fci.Extension = "application/pdf";
                return fci;
            }

            ReportLibrary.DotMatrixReporter labreport = new ReportLibrary.DotMatrixReporter(organizationid);
            labreport.format = ReportLibrary.DosFormatParser.ParseXML(organizationid, lstconfig[0].reportformat.format);
            labreport.DefaultPageSettings = labreport.format.GetDefaultSetting();
            labreport.Additional = new SortedDictionary<string, object>();

            ReportLibrary.PDFPrinter printer = new ReportLibrary.PDFPrinter();
            printer.Report = labreport;
            printer.EnablePageNumber = lstconfig[0].reportformat.format.Contains("PageCount");
            labreport.DataSource = result;

            labreport.DataItems = result.items;
            List<string> imageids = labreport.format.GetImages();
            var imageData = new Common.ImageData();
            foreach (var imgid in imageids.FindAll(x => x.Length > 6))
            {
                var file = imageData.GetData(imgid.LongFromBase62());
                if (file != null && file.data != null)
                {
                    var img = imageData.FromBytes(file.data);
                    labreport.Additional.Add("IMG-" + imgid, img);
                }
            }

            using (System.IO.MemoryStream stream = printer.Print(labreport.format.GetDefaultSetting()))
            {
                var fci = new FileContentInfo(new ByteArrayContent(stream.ToArray()));
                fci.FileName = "deliverypersonslip.pdf";
                fci.Extension = "application/pdf";
                return fci;
            }

        }
    }
}