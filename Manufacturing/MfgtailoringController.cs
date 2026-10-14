
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
using Newtonsoft.Json;
using WebAPI.Data;
namespace InfyPOS.Controllers
{
    [RoutePrefix("MfgtailoringService")]
    public class MfgtailoringServiceController : ApiController
    {

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<Mfgtailoring>();
            result.Item = new Mfgtailoring();
            return this.Ok(result);
        }

        [HttpPost]
        [Route("SearchSummary")]
        public async Task<IHttpActionResult> SearchSummary(ActionRequest<MfgtailoringCreteria> request)
        {
            MfgtailoringDataService MfgtailoringData = new MfgtailoringDataService();
            List<Mfgtailoring> result = await Task<List<Mfgtailoring>>.Run(() => MfgtailoringData.SearchSummary(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Payment")]
        public async Task<IHttpActionResult> Payment(ActionRequest<MfgPayment> request)
        {
            MfgtailoringDataService MfgtailoringData = new MfgtailoringDataService();
            MfgPayment result = await Task<MfgPayment>.Run(() => MfgtailoringData.Payment(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Received")]
        public async Task<IHttpActionResult> Received(ActionRequest<MfgReceived> request)
        {
            MfgtailoringDataService MfgtailoringData = new MfgtailoringDataService();
            MfgReceived result = await Task<MfgReceived>.Run(() => MfgtailoringData.Received(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("UpdateCharge")]
        public async Task<IHttpActionResult> UpdateCharge(ActionRequest<MfgtailoringCreteria> request)
        {
            MfgtailoringDataService MfgtailoringData = new MfgtailoringDataService();
            int result = await Task<int>.Run(() => MfgtailoringData.UpdateCharge());
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<MfgtailoringCreteria> request)
        {
            MfgtailoringDataService MfgtailoringData = new MfgtailoringDataService();
            List<Mfgtailoring> result = await Task<List<Mfgtailoring>>.Run(() => MfgtailoringData.Search(request.Item));
            return this.Ok(result);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("PrintPDF/{organizationid}/{data}")]
        public async Task<IHttpActionResult> PrintPDF(string organizationid, string data)
        {
            try
            {
                MfgTailoringPrint mfgTailoringPrint = JsonConvert.DeserializeObject<MfgTailoringPrint>(System.Text.ASCIIEncoding.ASCII.GetString(Convert.FromBase64String(data)));
                MfgtailoringDataService PurchaseorderData = new MfgtailoringDataService();
                PurchaseorderData.Context = new UserClaim()
                {
                    OrganizationID = long.Parse(organizationid)
                };
                FileContentInfo result = await Task<FileContentInfo>.Run(() =>
                {
                    var print = PurchaseorderData.Search(new MfgtailoringCreteria()
                    {
                         productid = mfgTailoringPrint.productid,
                        designid = mfgTailoringPrint.designid,
                        employeeid = mfgTailoringPrint.employeeid,
                        fromdate = mfgTailoringPrint.fromdate,
                        todate = mfgTailoringPrint.todate,
                    });
                    return PrintWorkOrder(mfgTailoringPrint,PurchaseorderData.Context.OrganizationID, print);
                });
                return new FileActionResult(false, "", result.Extension, result.Content);
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(long.Parse(organizationid)).Log(exp, WebAPI.Common.Logger.LogType.Error, "PrintPDF/" + data);
                return this.Ok(exp);
            }
        }


        public FileContentInfo PrintWorkOrder(MfgTailoringPrint mfgTailoringPrint,long organizationid, List<Mfgtailoring> result)
        {
            CompanyDataService companyDataService = new CompanyDataService();
            mfgTailoringPrint.company = companyDataService.Search(new CompanyCreteria()).OrderBy(x => x.id).First();
            InfyPOS.Data.ConfigurationDataService config = new ConfigurationDataService();
            config.Context = new UserClaim()
            {
                OrganizationID = organizationid
            };
            var lstconfig = config.Search(new ConfigurationCreteria
            {
                key = "TAILORING"
            });

            result = result.OrderBy(x => x.employeename).ThenBy(y => y.jobname).ToList();

            if (lstconfig == null || lstconfig.Count == 0)
                return null;
            ReportLibrary.DotMatrixReporter labreport = new ReportLibrary.DotMatrixReporter(organizationid);
            labreport.format = ReportLibrary.DosFormatParser.ParseXML(organizationid, lstconfig[0].reportformat.format);
            //labreport.format.PageBody.WrapRow = true;

            //if (hasSizeBased &&
            //    labreport.format.PageBody.Items[0].IsTable &&
            //labreport.format.PageBody.Items[0].Items.Exists(e => e.Type == ReportLibrary.DosFormat.CotnentType.Dynamicfield))
            //{
            //    List<ReportLibrary.DosFormat.Content> dynamiccolumns = new List<ReportLibrary.DosFormat.Content>();
            //    int index = 0;
            //    var rootcolumn = labreport.format.PageBody.Items[0].Items.First(e => e.Type == ReportLibrary.DosFormat.CotnentType.Dynamicfield);

            //    var totalwidth = labreport.format.PageBody.Items[0].Items.Sum(e => e.Length) - rootcolumn.Length;

            //    //labreport.format.PageBody.WrapRow = true;
            //    labreport.format.PageBody.Items[0].Items.InsertRange(labreport.format.PageBody.Items[0].Items.IndexOf(rootcolumn), dynamiccolumns);
            //    labreport.format.PageBody.Items[0].Items.Remove(rootcolumn);

            //    hasSizeBased = true;
            //}


            labreport.DefaultPageSettings = labreport.format.GetDefaultSetting();
            labreport.Additional = new SortedDictionary<string, object>();


            ReportLibrary.PDFPrinter printer = new ReportLibrary.PDFPrinter();
            printer.Report = labreport;
            printer.EnablePageNumber = lstconfig[0].reportformat.format.Contains("PageCount");
            labreport.DataSource = mfgTailoringPrint;
            labreport.DataItems = result;


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

            using (System.IO.MemoryStream stream = printer.Print(labreport.format.GetDefaultSetting()))
            {
                var fci = new FileContentInfo(new ByteArrayContent(stream.ToArray()));
                fci.FileName = "workorder.pdf";
                fci.Extension = "application/pdf";
                return fci;
            }

        }


        [HttpPost]
        [Route("Report")]
        public async Task<IHttpActionResult> Report(ActionRequest<MfgtailoringCreteria> request)
        {
            MfgtailoringDataService MfgtailoringData = new MfgtailoringDataService();
            List<MfgtailoringDetail> result = await Task<List<MfgtailoringDetail>>.Run(() => MfgtailoringData.Report(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("PrintSlip")]
        public async Task<IHttpActionResult> PrintSlip(ActionRequest<MfgtailoringCreteria> request)
        {
            MfgtailoringDataService MfgtailoringData = new MfgtailoringDataService();
            List<Mfgtailoring> result = await Task<List<Mfgtailoring>>.Run(() => MfgtailoringData.PrintSlip(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Save")]
        public async Task<IHttpActionResult> Save(ActionRequest<Mfgtailoring> request)
        {
            MfgtailoringDataService MfgtailoringData = new MfgtailoringDataService();
            if (request.Item.id <= 0)
            {
                Mfgtailoring result = await Task<Mfgtailoring>.Run(() => MfgtailoringData.Insert(request.Item));
                return this.Ok(result);
            }
            else
            {
                Mfgtailoring result = await Task<Mfgtailoring>.Run(() => MfgtailoringData.Update(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("Insert")]
        public async Task<IHttpActionResult> Insert(ActionRequest<Mfgtailoring> request)
        {
            MfgtailoringDataService MfgtailoringData = new MfgtailoringDataService();
            Mfgtailoring result = await Task<Mfgtailoring>.Run(() => MfgtailoringData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        public async Task<IHttpActionResult> Update(ActionRequest<Mfgtailoring> request)
        {
            MfgtailoringDataService MfgtailoringData = new MfgtailoringDataService();
            Mfgtailoring result = await Task<Mfgtailoring>.Run(() => MfgtailoringData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IHttpActionResult> Delete(ActionRequest<MfgtailoringCreteria> request)
        {
            MfgtailoringDataService MfgtailoringData = new MfgtailoringDataService();
            bool result = await Task<bool>.Run(() => MfgtailoringData.Delete(request.Item));
            return this.Ok(result);
        }
    }
}