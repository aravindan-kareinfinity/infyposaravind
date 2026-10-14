
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
    [RoutePrefix("MfgsampleorderService")]
    public class MfgsampleorderServiceController : ApiController
    {

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<Mfgsampleorder>();
            result.Item = new Mfgsampleorder();
            return this.Ok(result);
        }


        [HttpPost]
        [Route("UpdateStatus")]
        public async Task<IHttpActionResult> UpdateStatus(ActionRequest<MfgStatusChange> request)
        {
            MfgsampleorderDataService MfgsampleorderData = new MfgsampleorderDataService();
            MfgStatusChange result = await Task<MfgStatusChange>.Run(() => MfgsampleorderData.ChangeStatus(request.Item));
            return this.Ok(result);
        }

        [HttpGet]
        [Route("MfgDesign/{id}")]
        public async Task<IHttpActionResult> MfgDesign(string id)
        {
            MfgsampleorderDataService MfgsampleorderData = new MfgsampleorderDataService();
            MfgSampleDesign result = await Task<MfgSampleDesign>.Run(() => MfgsampleorderData.DesignInfo(long.Parse(id)));
            return this.Ok(result);
        }


        [HttpPost]
        [Route("SearchSummary")]
        public async Task<IHttpActionResult> SearchSummary(ActionRequest<MfgsampleorderCreteria> request)
        {
            MfgsampleorderDataService MfgsampleorderData = new MfgsampleorderDataService();
            List<Mfgsampleorder> result = await Task<List<Mfgsampleorder>>.Run(() => MfgsampleorderData.SearchSummary(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<MfgsampleorderCreteria> request)
        {
            MfgsampleorderDataService MfgsampleorderData = new MfgsampleorderDataService();
            List<Mfgsampleorder> result = await Task<List<Mfgsampleorder>>.Run(() => MfgsampleorderData.Search(request.Item));
            return this.Ok(result);
        }

        

        [HttpPost]
        [Route("Save")]
        public async Task<IHttpActionResult> Save(ActionRequest<Mfgsampleorder> request)
        {
            MfgsampleorderDataService MfgsampleorderData = new MfgsampleorderDataService();
            if (request.Item.id <= 0)
            {
                Mfgsampleorder result = await Task<Mfgsampleorder>.Run(() => MfgsampleorderData.Insert(request.Item));
                return this.Ok(result);
            }
            else
            {
                Mfgsampleorder result = await Task<Mfgsampleorder>.Run(() => MfgsampleorderData.Update(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("Insert")]
        public async Task<IHttpActionResult> Insert(ActionRequest<Mfgsampleorder> request)
        {
            MfgsampleorderDataService MfgsampleorderData = new MfgsampleorderDataService();
            Mfgsampleorder result = await Task<Mfgsampleorder>.Run(() => MfgsampleorderData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        public async Task<IHttpActionResult> Update(ActionRequest<Mfgsampleorder> request)
        {
            MfgsampleorderDataService MfgsampleorderData = new MfgsampleorderDataService();
            Mfgsampleorder result = await Task<Mfgsampleorder>.Run(() => MfgsampleorderData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IHttpActionResult> Delete(ActionRequest<Mfgsampleorder> request)
        {
            MfgsampleorderDataService MfgsampleorderData = new MfgsampleorderDataService();
            bool result = await Task<bool>.Run(() => MfgsampleorderData.Delete(request.Item));
            return this.Ok(result);
        }

        
        [HttpGet]
        [Route("PrintPDF/{organizationid}/{id}")]
        public async Task<IHttpActionResult> PrintPDF(string organizationid, string id)
        {
            try
            {
                long poid = 0;
                if (!string.IsNullOrEmpty(id))
                {
                    poid = long.Parse(id);
                }
                MfgsampleorderDataService MfgsampleorderData = new MfgsampleorderDataService();

                FileContentInfo result = await Task<FileContentInfo>.Run(() =>
                {
                    MfgsampleorderPrint print = MfgsampleorderData.Print(new MfgsampleorderCreteria()
                    {
                        id = poid
                    });
                    return PrintJobOrder(MfgsampleorderData.Context.OrganizationID, print);
                });
                return new FileActionResult(false, "", result.Extension, result.Content);
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(long.Parse(organizationid)).Log(exp, WebAPI.Common.Logger.LogType.Error, "PrintPDF/" + id);
                return this.Ok(exp);
            }
        }


        public FileContentInfo PrintJobOrder(long organizationid, MfgsampleorderPrint result)
        {
            bool hasSizeBased = true;
            InfyPOS.Data.ConfigurationDataService config = new ConfigurationDataService();
            var lstconfig = config.Search(new ConfigurationCreteria
            {
                key = "SAMPLEORDER",
                organizationid = result.companyid
            });

            if (lstconfig == null || lstconfig.Count == 0)
                return null;
            ReportLibrary.DotMatrixReporter labreport = new ReportLibrary.DotMatrixReporter(organizationid);
            labreport.format = ReportLibrary.DosFormatParser.ParseXML(organizationid, lstconfig[0].reportformat.format);
            //labreport.format.PageBody.WrapRow = true;
            labreport.Mode = ReportLibrary.DotMatrixReporter.PrintingMode.Label;
            labreport.DefaultPageSettings = labreport.format.GetDefaultSetting();
            labreport.Additional = new SortedDictionary<string, object>();

            ReportLibrary.PDFPrinter printer = new ReportLibrary.PDFPrinter();
            printer.Report = labreport;
            printer.EnablePageNumber = lstconfig[0].reportformat.format.Contains("PageCount");
            labreport.DataSource = result;
            labreport.DataItems = result.Designs;
            labreport.DataSourceList = result.Designs;
            //printer.Title = result.company.name + " Job Order @ " + result.joborderno + "-" + result.joborderdate.ToString("dd-MM-yyyy");
            //printer.Keywords = result.supplier.name + "," + result.suppliercompany.name;

            List<string> imageids = labreport.format.GetImages();
            var imageData = new Common.ImageData();
            var imagelist = imageids.FindAll(x => x.Length > 6);
            if (!string.IsNullOrEmpty(result.imageid)) imagelist.Add(result.imageid);
            result.Designs.ForEach(x =>
            {
                x.parent = result;
                if (!string.IsNullOrEmpty(x.imageid))
                    imagelist.Add(x.imageid);
            });
            foreach (var imgid in imagelist)
            {
                var file = imageData.GetData(imgid.LongFromBase62());
                if (file != null)
                {
                    try
                    {
                        var img = System.Drawing.Image.FromStream(new System.IO.MemoryStream(file.data));
                        labreport.Additional.Add("IMG-" + imgid, img);
                    }
                    catch (Exception exp)
                    { }
                }
            }

            using (System.IO.MemoryStream stream = printer.Print(labreport.format.GetDefaultSetting()))
            {
                var fci = new FileContentInfo(new ByteArrayContent(stream.ToArray()));
                fci.FileName = "joborder.pdf";
                fci.Extension = "application/pdf";
                return fci;
            }

        }
    }
}