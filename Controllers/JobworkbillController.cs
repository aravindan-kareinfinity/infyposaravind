
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
    [RoutePrefix("JobworkbillService")]
    public class JobworkbillServiceController : ApiController
    {

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<Jobworkbill>();
            result.Item = new Jobworkbill();
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<JobworkbillCreteria> request)
        {
            JobworkbillDataService JobworkbillData = new JobworkbillDataService();
            List<Jobworkbill> result = await Task<List<Jobworkbill>>.Run(() => JobworkbillData.Search(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("SearchStock")]
        public async Task<IHttpActionResult> SearchStock(ActionRequest<JobworkbillCreteria> request)
        {
            JobworkbillDataService JobworkbillData = new JobworkbillDataService();
            List<StockInfo> result = await Task<List<StockInfo>>.Run(() => JobworkbillData.SearchStock(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("SearchBillValue")]
        public async Task<IHttpActionResult> SearchBillValue(ActionRequest<JobworkbillCreteria> request)
        {
            JobworkbillDataService JobworkbillData = new JobworkbillDataService();
            decimal result = await Task<decimal>.Run(() => JobworkbillData.SearchBillValue(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Save")]
        public async Task<IHttpActionResult> Save(ActionRequest<Jobworkbill> request)
        {
            JobworkbillDataService JobworkbillData = new JobworkbillDataService();
            if (request.Item.id <= 0)
            {
                Jobworkbill result = await Task<Jobworkbill>.Run(() => JobworkbillData.Insert(request.Item));
                return this.Ok(result);
            }
            else
            {
                Jobworkbill result = await Task<Jobworkbill>.Run(() => JobworkbillData.Update(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("Insert")]
        public async Task<IHttpActionResult> Insert(ActionRequest<Jobworkbill> request)
        {
            JobworkbillDataService JobworkbillData = new JobworkbillDataService();
            Jobworkbill result = await Task<Jobworkbill>.Run(() => JobworkbillData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        public async Task<IHttpActionResult> Update(ActionRequest<Jobworkbill> request)
        {
            JobworkbillDataService JobworkbillData = new JobworkbillDataService();
            Jobworkbill result = await Task<Jobworkbill>.Run(() => JobworkbillData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IHttpActionResult> Delete(ActionRequest<Jobworkbill> request)
        {
            JobworkbillDataService JobworkbillData = new JobworkbillDataService();
            bool result = await Task<bool>.Run(() => JobworkbillData.Delete(request.Item));
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
                JobworkbillDataService _jobworkdata = new JobworkbillDataService();
                _jobworkdata.Context = new UserClaim()
                {
                    OrganizationID = long.Parse(organizationid)
                };
                var result = _jobworkdata.Print(new JobworkbillCreteria() { id = long.Parse(id) });
                FileContentInfo byteArray = PrintJobWork(long.Parse(organizationid), result);
                return new FileActionResult(false, "", byteArray.Extension, byteArray.Content);
            }
            catch (Exception exp)
            {
                return this.Ok(exp);
            }
        }

        [HttpPost]
        [Route("ImagePDFGeneration")]
        public async Task<IHttpActionResult> ImagePDFGeneration(ActionRequest<JobworkbillCreteria> request)
        {
            JobworkbillDataService _jobworkdata = new JobworkbillDataService();
            byte[] pdfbytes = await Task<byte[]>.Run(() => _jobworkdata.ImagePDFGeneration(request.Item));
            FileContentInfo byteArray = new FileContentInfo(new ByteArrayContent(pdfbytes));
            byteArray.FileName = "JobworkImage.pdf";
            byteArray.Extension = "application/pdf";
            return new FileActionResult(false, "", byteArray.Extension, byteArray.Content);
        }
        private FileContentInfo PrintJobWork(long organizationid, JobworkbillPrint result)
        {
            InfyPOS.Data.ConfigurationDataService config = new ConfigurationDataService();
            config.Context = new UserClaim()
            {
                OrganizationID = organizationid
            };
            var lstconfig = config.Search(new ConfigurationCreteria
            {
                key = "JOBWORKBILL",
                organizationid = result.companyid
            });

            if (lstconfig == null || lstconfig.Count == 0)
            {
                lstconfig = config.Search(new ConfigurationCreteria
                {
                    key = "JOBWORKBILL"
                });
            }
            if (lstconfig == null || lstconfig.Count == 0)
                return null;

            result.specification = new List<JobworkbillitemSpecification>();
            result.specification = result.Jobworkbillitems.GroupBy(e => new { e.outproductname, e.chargerate }).ToList().ConvertAll<JobworkbillitemSpecification>(x =>
             {
                 return new JobworkbillitemSpecification()
                 {
                     sno = result.specification.Count + 1,
                     outproductname = x.Key.outproductname,
                     chargerate = x.Key.chargerate,
                     chargegross = x.Sum(y => y.chargegross),
                     oqty = x.Sum(y => y.oqty),
                     totalqtyconsumption = x.Sum(y => y.iqty),
                     //perqtyconsumption = x.Average(y => y.oqty / y.iqty)
                     perqtyconsumption = x.Sum(y => y.iqty) / x.Sum(y => y.oqty)
                 };
             });

            ReportLibrary.DotMatrixReporter labreport = new ReportLibrary.DotMatrixReporter(organizationid);
            labreport.format = ReportLibrary.DosFormatParser.ParseXML(organizationid, lstconfig[0].reportformat.format);
            labreport.DefaultPageSettings = labreport.format.GetDefaultSetting();
            labreport.Additional = new SortedDictionary<string, object>();

            ReportLibrary.PDFPrinter printer = new ReportLibrary.PDFPrinter();
            printer.Report = labreport;
            printer.EnablePageNumber = lstconfig[0].reportformat.format.Contains("PageCount");
            labreport.DataSource = result;
            labreport.DataItems = result.Jobworkbillitems;

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
                fci.FileName = "jobworkinvoice.pdf";
                fci.Extension = "application/pdf";
                return fci;
            }

        }
    }
}