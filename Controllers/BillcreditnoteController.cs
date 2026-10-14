
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
    [RoutePrefix("BillcreditnoteService")]
    public class BillcreditnoteServiceController : ApiController
    {

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<Billcreditnote>();
            result.Item = new Billcreditnote();
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<BillcreditnoteCreteria> request)
        {
            BillcreditnoteDataService BillcreditnoteData = new BillcreditnoteDataService();
            List<Billcreditnote> result = await Task<List<Billcreditnote>>.Run(() => BillcreditnoteData.Search(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Save")]
        public async Task<IHttpActionResult> Save(ActionRequest<Billcreditnote> request)
        {
            BillcreditnoteDataService BillcreditnoteData = new BillcreditnoteDataService();
            if (request.Item.id <= 0)
            {
                Billcreditnote result = await Task<Billcreditnote>.Run(() => BillcreditnoteData.Insert(request.Item));
                return this.Ok(result);
            }
            else
            {
                Billcreditnote result = await Task<Billcreditnote>.Run(() => BillcreditnoteData.Update(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("Insert")]
        public async Task<IHttpActionResult> Insert(ActionRequest<Billcreditnote> request)
        {
            BillcreditnoteDataService BillcreditnoteData = new BillcreditnoteDataService();
            Billcreditnote result = await Task<Billcreditnote>.Run(() => BillcreditnoteData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        public async Task<IHttpActionResult> Update(ActionRequest<Billcreditnote> request)
        {
            BillcreditnoteDataService BillcreditnoteData = new BillcreditnoteDataService();
            Billcreditnote result = await Task<Billcreditnote>.Run(() => BillcreditnoteData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IHttpActionResult> Delete(ActionRequest<Billcreditnote> request)
        {
            BillcreditnoteDataService BillcreditnoteData = new BillcreditnoteDataService();
            bool result = await Task<bool>.Run(() => BillcreditnoteData.Delete(request.Item));
            return this.Ok(result);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("PrintPDF/{organizationid}/{id}")]
        public async Task<IHttpActionResult> PrintPDF(string organizationid, string id)
        {
            try
            {
                if (long.Parse(id) == 0) return null;
                BillcreditnoteDataService billcreditnotedata = new BillcreditnoteDataService();
                billcreditnotedata.Context = new UserClaim() { OrganizationID = long.Parse(organizationid) };
                var result = billcreditnotedata.Print(long.Parse(id));
                FileContentInfo byteArray = PrintCreditNote(long.Parse(organizationid), result);
                return new FileActionResult(false, "", byteArray.Extension, byteArray.Content);
            }
            catch (Exception exp)
            {
                return this.Ok(exp);
            }
        }
        private FileContentInfo PrintCreditNote(long organizationid, BillcreditnotePrint result)
        {
            InfyPOS.Data.ConfigurationDataService config = new ConfigurationDataService();
            config.Context = new UserClaim()
            {
                OrganizationID = organizationid
            };
            var lstconfig = config.Search(new ConfigurationCreteria
            {
                key = "CUSTOMERCREDITNOTE",
                organizationid = result.companyid
            });

            if (lstconfig == null || lstconfig.Count == 0)
            {
                lstconfig = config.Search(new ConfigurationCreteria
                {
                    key = "CUSTOMERCREDITNOTE"
                });
            }
            if (lstconfig == null || lstconfig.Count == 0)
                return null;

            ReportLibrary.DotMatrixReporter labreport = new ReportLibrary.DotMatrixReporter(organizationid);
            labreport.format = ReportLibrary.DosFormatParser.ParseXML(organizationid, lstconfig[0].reportformat.format);
            labreport.DefaultPageSettings = labreport.format.GetDefaultSetting();
            labreport.Additional = new SortedDictionary<string, object>();

            ReportLibrary.PDFPrinter printer = new ReportLibrary.PDFPrinter();
            printer.Report = labreport;
            printer.EnablePageNumber = lstconfig[0].reportformat.format.Contains("PageCount");
            labreport.DataSource = result;

            labreport.DataItems = result.Billcreditnoteitems;
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
                fci.FileName = "creditnote.pdf";
                fci.Extension = "application/pdf";
                return fci;
            }

        }
    }
}