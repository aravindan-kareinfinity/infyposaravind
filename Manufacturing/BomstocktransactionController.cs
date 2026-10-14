
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
    [RoutePrefix("BomstocktransactionService")]
    public class BomstocktransactionServiceController : ApiController
    {

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<Bomstocktransaction>();
            result.Item = new Bomstocktransaction();
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<BomstocktransactionCreteria> request)
        {
            BomstocktransactionDataService BomstocktransactionData = new BomstocktransactionDataService();
            List<Bomstocktransaction> result = await Task<List<Bomstocktransaction>>.Run(() => BomstocktransactionData.Search(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Save")]
        public async Task<IHttpActionResult> Save(ActionRequest<Bomstocktransaction> request)
        {
            BomstocktransactionDataService BomstocktransactionData = new BomstocktransactionDataService();
            if (request.Item.id <= 0)
            {
                Bomstocktransaction result = await Task<Bomstocktransaction>.Run(() => BomstocktransactionData.Insert(request.Item));
                return this.Ok(result);
            }
            else
            {
                Bomstocktransaction result = await Task<Bomstocktransaction>.Run(() => BomstocktransactionData.Update(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("Insert")]
        public async Task<IHttpActionResult> Insert(ActionRequest<Bomstocktransaction> request)
        {
            BomstocktransactionDataService BomstocktransactionData = new BomstocktransactionDataService();
            Bomstocktransaction result = await Task<Bomstocktransaction>.Run(() => BomstocktransactionData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("StockRemoveTransaction")]
        public async Task<IHttpActionResult> StockRemoveTransaction(ActionMulitpleRequest<BomStcokTransactionSummary> request)
        {
            BomstocktransactionDataService BomstocktransactionData = new BomstocktransactionDataService();
            int result = await Task<int>.Run(() => BomstocktransactionData.RemoveBomStcokTransaction(request.Items));
            return this.Ok(result);
        }

        

        [HttpPost]
        [Route("StockReport")]
        public async Task<IHttpActionResult> StockReport(ActionRequest<BomstocktransactionCreteria> request)
        {
            BomstocktransactionDataService BomstocktransactionData = new BomstocktransactionDataService();
            if (request.Item.mode == 1)
            {
                List<BomStcokTransactionDetail> result = await Task<BomStcokTransactionDetail>.Run(() => BomstocktransactionData.DetailReport(request.Item));
                return this.Ok(result);
            }
            else
            {
                if(request.Item.mode == 3)
                {
                    request.Item.fromdate = DateTime.Now.Date.AddDays(1);
                    request.Item.todate = DateTime.MaxValue.Date.AddDays(-1);
                }
                List<BomStcokTransactionSummary> result = await Task<BomStcokTransactionSummary>.Run(() => BomstocktransactionData.SummaryReport(request.Item));
                if (request.Item.printpdf)
                {
                    byte[] pdfbytes = await Task<byte[]>.Run(() => PrintStockPDF(BomstocktransactionData.Context.OrganizationID,request.Item.companyid,
                       result));
                    string key = WebAPI.Data.Token.RandomString(16);
                    System.Web.HttpContext.Current.Cache.Add("PDF-" + key, pdfbytes, null,
                        DateTime.Now.AddMinutes(2), TimeSpan.Zero, System.Web.Caching.CacheItemPriority.Low, null);
                    return this.Ok(key);
                }
                else
                {
                    return this.Ok(result);
                }
            }
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("StockReportPDF/{key}")]
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
        public byte[] PrintStockPDF(long organizationid, long companyid, List<BomStcokTransactionSummary> result)
        {
            bool hasSizeBased = true;
            InfyPOS.Data.ConfigurationDataService config = new ConfigurationDataService();
            config.Context = new UserClaim()
            {
                OrganizationID = organizationid
            };
            var lstconfig = config.Search(new ConfigurationCreteria
            {
                key = "BOMSTOCKSUMMARY",
                organizationid = companyid
            });

            if (lstconfig == null || lstconfig.Count == 0)
                return null;
            ReportLibrary.DotMatrixReporter labreport = new ReportLibrary.DotMatrixReporter(organizationid);
            labreport.format = ReportLibrary.DosFormatParser.ParseXML(organizationid, lstconfig[0].reportformat.format);
            //labreport.format.PageBody.WrapRow = true;

            if (hasSizeBased &&
                labreport.format.PageBody.Items[0].IsTable &&
            labreport.format.PageBody.Items[0].Items.Exists(e => e.Type == ReportLibrary.DosFormat.CotnentType.Dynamicfield))
            {
                List<ReportLibrary.DosFormat.Content> dynamiccolumns = new List<ReportLibrary.DosFormat.Content>();
                int index = 0;
                var rootcolumn = labreport.format.PageBody.Items[0].Items.First(e => e.Type == ReportLibrary.DosFormat.CotnentType.Dynamicfield);

                var totalwidth = labreport.format.PageBody.Items[0].Items.Sum(e => e.Length) - rootcolumn.Length;

                //labreport.format.PageBody.WrapRow = true;
                labreport.format.PageBody.Items[0].Items.InsertRange(labreport.format.PageBody.Items[0].Items.IndexOf(rootcolumn), dynamiccolumns);
                labreport.format.PageBody.Items[0].Items.Remove(rootcolumn);

                hasSizeBased = true;
            }


            labreport.DefaultPageSettings = labreport.format.GetDefaultSetting();
            labreport.Additional = new SortedDictionary<string, object>();


            ReportLibrary.PDFPrinter printer = new ReportLibrary.PDFPrinter();
            printer.EnablePageNumber = lstconfig[0].reportformat.format.Contains("PageCount");
            printer.Report = labreport;
            printer.EnablePageNumber = lstconfig[0].reportformat.format.Contains("PageCount");
            labreport.DataSource = result;
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
                return stream.ToArray();
            }

        }

        [HttpPost]
        [Route("Update")]
        public async Task<IHttpActionResult> Update(ActionRequest<Bomstocktransaction> request)
        {
            BomstocktransactionDataService BomstocktransactionData = new BomstocktransactionDataService();
            Bomstocktransaction result = await Task<Bomstocktransaction>.Run(() => BomstocktransactionData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IHttpActionResult> Delete(ActionRequest<Bomstocktransaction> request)
        {
            BomstocktransactionDataService BomstocktransactionData = new BomstocktransactionDataService();
            bool result = await Task<bool>.Run(() => BomstocktransactionData.Delete(request.Item));
            return this.Ok(result);
        }
    }
}