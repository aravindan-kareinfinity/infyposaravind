
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
    [RoutePrefix("MfgjoborderService")]
    public class MfgjoborderServiceController : ApiController
    {

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<Mfgjoborder>();
            result.Item = new Mfgjoborder();
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<MfgjoborderCreteria> request)
        {
            MfgjoborderDataService MfgjoborderData = new MfgjoborderDataService();
            List<Mfgjoborder> result = await Task<List<Mfgjoborder>>.Run(() => MfgjoborderData.Search(request.Item));
            return this.Ok(result);
        }


        [HttpPost]
        [Route("Report")]
        public async Task<IHttpActionResult> Report(ActionRequest<MfgjoborderCreteria> request)
        {
            MfgjoborderDataService MfgworkorderData = new MfgjoborderDataService();
            List<MfgjoborderSummary> result = await Task<List<MfgjoborderSummary>>.Run(() => MfgworkorderData.Report(request.Item));
            return this.Ok(result);
        }


        [HttpPost]
        [Route("CreateStage")]
        public async Task<IHttpActionResult> CreateStage(ActionRequest<Mfgjoborderstage> request)
        {
            MfgjoborderDataService MfgjoborderData = new MfgjoborderDataService();
            Mfgjoborder result = await Task<Mfgjoborder>.Run(() => MfgjoborderData.CreateStage(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Save")]
        public async Task<IHttpActionResult> Save(ActionRequest<Mfgjoborder> request)
        {
            MfgjoborderDataService MfgjoborderData = new MfgjoborderDataService();
            if (request.Item.id <= 0)
            {
                Mfgjoborder result = await Task<Mfgjoborder>.Run(() => MfgjoborderData.Insert(request.Item));
                return this.Ok(result);
            }
            else
            {
                Mfgjoborder result = await Task<Mfgjoborder>.Run(() => MfgjoborderData.Update(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("Insert")]
        public async Task<IHttpActionResult> Insert(ActionRequest<Mfgjoborder> request)
        {
            MfgjoborderDataService MfgjoborderData = new MfgjoborderDataService();
            Mfgjoborder result = await Task<Mfgjoborder>.Run(() => MfgjoborderData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        public async Task<IHttpActionResult> Update(ActionRequest<Mfgjoborder> request)
        {
            MfgjoborderDataService MfgjoborderData = new MfgjoborderDataService();
            Mfgjoborder result = await Task<Mfgjoborder>.Run(() => MfgjoborderData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IHttpActionResult> Delete(ActionRequest<Mfgjoborder> request)
        {
            MfgjoborderDataService MfgjoborderData = new MfgjoborderDataService();
            bool result = await Task<bool>.Run(() => MfgjoborderData.Delete(request.Item));
            return this.Ok(result);
        }


        [HttpPost]
        [Route("PrintBarcode")]
        public async Task<IHttpActionResult> PrintBarcode(ActionRequest<BOMPrint> request)
        {
            var newbarcodelist = request.Item.items.FindAll(x => string.IsNullOrEmpty(x.bominwardcode) || x.bominwardcode == "0");

            if (newbarcodelist.Count > 0)
            {
                MfgjoborderDataService MfgjoborderData = new MfgjoborderDataService();
                newbarcodelist = await Task<Mfgjoborder>.Run(() => MfgjoborderData.GenerateBarcode(newbarcodelist));
            }
            InfyPOS.Data.ConfigurationDataService config = new ConfigurationDataService();
            var reportformats = config.Search(new ConfigurationCreteria
            {
                id = request.Item.reportid
            });

            List<string> base64Response = new List<string>();
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            System.Text.StringBuilder asb = new System.Text.StringBuilder();

            InfyPOS.Report.CitizenBarcode barcodeprinter = new Report.CitizenBarcode("",
                reportformats[0].reportformat.format, reportformats[0].reportformat.mode, reportformats[0].nooflines);

            List<Report.IBarcodeFormat> barcodeList = new List<Report.IBarcodeFormat>();
            foreach (var bcode in request.Item.items.OrderBy(x => x.sno))
            {
                for (var i = 0; i < bcode.pqty; i++)
                {
                    barcodeList.Add(bcode);
                }
            }
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

        [AllowAnonymous]
        [HttpGet]
        [Route("PrintPDF/{organizationid}/{id}/{id1?}/{id2?}/{id3?}")]
        public async Task<IHttpActionResult> PrintPDF(string organizationid, string id,
            string id1 = null, string id2 = null, string id3 = null)
        {
            try
            {
                long poid = 0;
                if (string.IsNullOrEmpty(id1) &&
                    string.IsNullOrEmpty(id2) &&
                    string.IsNullOrEmpty(id3))
                {
                    poid = long.Parse(id);
                    id = "";
                }
                else
                {
                    if (!string.IsNullOrEmpty(id1))
                        id += "/" + id1;
                    if (!string.IsNullOrEmpty(id2))
                        id += "/" + id2;
                    if (!string.IsNullOrEmpty(id3))
                        id += "/" + id3;
                }
                MfgjoborderDataService PurchaseorderData = new MfgjoborderDataService();
                PurchaseorderData.Context = new UserClaim()
                {
                    OrganizationID = long.Parse(organizationid)
                };
                FileContentInfo result = await Task<FileContentInfo>.Run(() =>
                {
                    MfgjoborderPrint print = PurchaseorderData.Print(new MfgjoborderCreteria()
                    {
                        id = poid,
                        joborderno = id
                    });
                    return PrintJobOrder(PurchaseorderData.Context.OrganizationID,print);
                });
                return new FileActionResult(false, "", result.Extension, result.Content);
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(long.Parse(organizationid)).Log(exp, WebAPI.Common.Logger.LogType.Error, "PrintPDF/" + id);
                return this.Ok(exp);
            }
        }


        public FileContentInfo PrintJobOrder(long organizationid, MfgjoborderPrint result)
        {
            bool hasSizeBased = true;
            InfyPOS.Data.ConfigurationDataService config = new ConfigurationDataService();
            var lstconfig = config.Search(new ConfigurationCreteria
            {
                key = result.inwardjob ? "JOBWORK-RETURN": "JOBWORK-ISSUE",
                organizationid = result.companyid
            });

            if (lstconfig == null || lstconfig.Count == 0)
                return null;
            ReportLibrary.DotMatrixReporter labreport = new ReportLibrary.DotMatrixReporter(organizationid);
            labreport.format = ReportLibrary.DosFormatParser.ParseXML(organizationid,lstconfig[0].reportformat.format);
            //labreport.format.PageBody.WrapRow = true;

            labreport.DefaultPageSettings = labreport.format.GetDefaultSetting();
            labreport.Additional = new SortedDictionary<string, object>();

            ReportLibrary.PDFPrinter printer = new ReportLibrary.PDFPrinter();
            printer.Report = labreport;
            printer.EnablePageNumber = lstconfig[0].reportformat.format.Contains("PageCount");
            labreport.DataSource = result;
            labreport.DataItems = result.Mfgjoborderitems;
            printer.Title = result.company.name + " Job Order @ " + result.joborderno + "-" + result.joborderdate.ToString("dd-MM-yyyy");
            //printer.Keywords = result.supplier.name + "," + result.suppliercompany.name;

            List<string> imageids = labreport.format.GetImages();
            var imageData = new Common.ImageData();
            foreach (var imgid in imageids.FindAll(x => x.Length > 6))
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