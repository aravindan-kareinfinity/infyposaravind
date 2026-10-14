
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
    [RoutePrefix("CourierService")]
    public class CourierServiceController : ApiController
    {

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<Courier>();
            result.Item = new Courier();
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<CourierCreteria> request)
        {
            CourierDataService CourierData = new CourierDataService();
            List<Courier> result = await Task<List<Courier>>.Run(() => CourierData.Search(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("SearchBill")]
        public async Task<IHttpActionResult> SearchBill(ActionRequest<CourierCreteria> request)
        {
            CourierDataService CourierData = new CourierDataService();
            List<Courier.BillInfo> result = await Task<List<Courier.BillInfo>>.Run(() => CourierData.SearchBill(request.Item));
            return this.Ok(result);
        }
        

        [HttpPost]
        [Route("Save")]
        public async Task<IHttpActionResult> Save(ActionRequest<Courier> request)
        {
            CourierDataService CourierData = new CourierDataService();
            if (request.Item.id <= 0)
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Create);
                Courier result = await Task<Courier>.Run(() => CourierData.Insert(request.Item));
                return this.Ok(result);
            }
            else
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Update);
                Courier result = await Task<Courier>.Run(() => CourierData.Update(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("Insert")]
        public async Task<IHttpActionResult> Insert(ActionRequest<Courier> request)
        {
            CourierDataService CourierData = new CourierDataService();
            Courier result = await Task<Courier>.Run(() => CourierData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        public async Task<IHttpActionResult> Update(ActionRequest<Courier> request)
        {
            CourierDataService CourierData = new CourierDataService();
            Courier result = await Task<Courier>.Run(() => CourierData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IHttpActionResult> Delete(ActionRequest<Courier> request)
        {
            CourierDataService CourierData = new CourierDataService();
            bool result = await Task<bool>.Run(() => CourierData.Delete(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("ImagePDFGeneration")]
        public async Task<IHttpActionResult> ImagePDFGeneration(ActionRequest<CourierCreteria> request)
        {
            CourierDataService CourierData = new CourierDataService();
            byte[] pdfbytes = await Task<byte[]>.Run(() => CourierData.ImagePDFGeneration(request.Item));
            string key = WebAPI.Data.Token.RandomString(16);
            System.Web.HttpContext.Current.Cache.Add("PDF-" + key, pdfbytes, null,
                DateTime.UtcNow.AddMinutes(2), TimeSpan.Zero, System.Web.Caching.CacheItemPriority.Low, null);
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

        [AllowAnonymous]
        [HttpGet]
        [Route("PrintPDF/{organizationid}/{id}")]
        public async Task<IHttpActionResult> PrintPDF(string organizationid, string id)
        {
            try
            {
                if (long.Parse(id) == 0) return null;
                CourierDataService CourierData = new CourierDataService();
                CourierData.Context = new UserClaim() { OrganizationID = long.Parse(organizationid) };
                Courier result = CourierData.Print(new CourierCreteria() { id = long.Parse(id) });
                FileContentInfo byteArray = PrintCourier(long.Parse(organizationid), result);
                return new FileActionResult(false, "", byteArray.Extension, byteArray.Content);
            }
            catch (Exception exp)
            {
                return this.Ok(exp);
            }
        }
        private FileContentInfo PrintCourier(long organizationid, Courier result)
        {
            InfyPOS.Data.ConfigurationDataService config = new ConfigurationDataService();
            config.Context = new UserClaim()
            {
                OrganizationID = organizationid
            };
            var lstconfig = config.Search(new ConfigurationCreteria
            {
                key = "COURIER"
            });

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

            labreport.DataItems = result.podlist;
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
                fci.FileName = "courier.pdf";
                fci.Extension = "application/pdf";
                return fci;
            }

        }

    }
}