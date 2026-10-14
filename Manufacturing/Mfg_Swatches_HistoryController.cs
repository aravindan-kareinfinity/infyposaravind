
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
    [RoutePrefix("Mfg_Swatches_HistoryService")]
    public class Mfg_Swatches_HistoryServiceController : ApiController
    {

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<Mfg_Swatches_History>();
            result.Item = new Mfg_Swatches_History();
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<Mfg_Swatches_HistoryCreteria> request)
        {
            Mfg_Swatches_HistoryDataService Mfg_Swatches_HistoryData = new Mfg_Swatches_HistoryDataService();
            List<Mfg_Swatches_HistoryDetail> result = await Task<List<Mfg_Swatches_HistoryDetail>>.Run(() => Mfg_Swatches_HistoryData.Search(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Save")]
        public async Task<IHttpActionResult> Save(ActionMulitpleRequest<Mfg_Swatches_HistoryDetail> request)
        {
            Mfg_Swatches_HistoryDataService Mfg_Swatches_HistoryData = new Mfg_Swatches_HistoryDataService();

            List<Mfg_Swatches_HistoryDetail> result = await Task<List<Mfg_Swatches_HistoryDetail>>.Run(() => Mfg_Swatches_HistoryData.Insert(request.Items));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Insert")]
        public async Task<IHttpActionResult> Insert(ActionMulitpleRequest<Mfg_Swatches_HistoryDetail> request)
        {
            Mfg_Swatches_HistoryDataService Mfg_Swatches_HistoryData = new Mfg_Swatches_HistoryDataService();
            List<Mfg_Swatches_HistoryDetail> result = await Task<List<Mfg_Swatches_HistoryDetail>>.Run(() => Mfg_Swatches_HistoryData.Insert(request.Items));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        public async Task<IHttpActionResult> Update(ActionRequest<Mfg_Swatches_HistoryDetail> request)
        {
            Mfg_Swatches_HistoryDataService Mfg_Swatches_HistoryData = new Mfg_Swatches_HistoryDataService();
            Mfg_Swatches_HistoryDetail result = await Task<Mfg_Swatches_HistoryDetail>.Run(() => Mfg_Swatches_HistoryData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IHttpActionResult> Delete(ActionRequest<Mfg_Swatches_History> request)
        {
            Mfg_Swatches_HistoryDataService Mfg_Swatches_HistoryData = new Mfg_Swatches_HistoryDataService();
            bool result = await Task<bool>.Run(() => Mfg_Swatches_HistoryData.Delete(request.Item));
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
                Mfg_Swatches_HistoryDataService ComponentPOData = new Mfg_Swatches_HistoryDataService();
                ComponentPOData.Context = new UserClaim() { OrganizationID = long.Parse(organizationid) };
                var print = ComponentPOData.Search(new Mfg_Swatches_HistoryCreteria() { id = long.Parse(id), includeitems = true });
                FileContentInfo byteArray = PrintPO(long.Parse(organizationid), print[0]);
                return new FileActionResult(false, "", byteArray.Extension, byteArray.Content);
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(long.Parse(organizationid)).Log(exp, WebAPI.Common.Logger.LogType.Error, "PrintPDF/" + id);
                return this.Ok(exp);
            }
        }
        public FileContentInfo PrintPO(long organizationid, Mfg_Swatches_History result)
        {
            bool hasSizeBased = true;
            InfyPOS.Data.ConfigurationDataService config = new ConfigurationDataService();
            config.Context = new UserClaim()
            {
                OrganizationID = organizationid
            };
            var lstconfig = config.Search(new ConfigurationCreteria
            {
                key = "SWATCHFOLDER"
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
            labreport.DataItems = result.items;

            printer.Title = " Swatches & Folders @ " + result.receiptno + "-" + result.transactionon.ToString("dd-MM-yyyy");
            if (result.qty < 0)
            {
                result.items.ForEach(x => x.qty = Math.Abs(x.qty));
            }
            List<string> imageids = labreport.format.GetImages();
            var imageData = new Common.ImageData();
            foreach (var imgid in imageids.FindAll(x => x.Length > 6))
            {
                var file = imageData.GetData(imgid.LongFromBase62());
                if (file != null)
                {
                    try
                    {
                        var img = imageData.FromBytes(file.data);
                        labreport.Additional.Add("IMG-" + imgid, img);
                    }
                    catch (Exception exp)
                    { }
                }
            }

            if (!string.IsNullOrEmpty(result.company.logo))
            {
                var file = imageData.GetData(result.company.logo.LongFromBase62());
                if (file != null)
                {
                    var img = imageData.FromBytes(file.data);
                    labreport.Additional.Add("LOGO", img);
                }
            }

            using (System.IO.MemoryStream stream = printer.Print(labreport.format.GetDefaultSetting()))
            {
                var fci = new FileContentInfo(new ByteArrayContent(stream.ToArray()));
                fci.FileName = "swatchandfolder.pdf";
                fci.Extension = "application/pdf";
                return fci;
            }

        }
    }
}