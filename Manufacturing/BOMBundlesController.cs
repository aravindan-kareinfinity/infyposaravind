
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
    [RoutePrefix("BOMBundlesService")]
    public class BOMBundlesServiceController : ApiController
    {

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<BOMBundles>();
            result.Item = new BOMBundles();
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<BOMBundlesCreteria> request)
        {
            BOMBundlesDataService BOMBundlesData = new BOMBundlesDataService();
            List<BOMBundles> result = await Task<List<BOMBundles>>.Run(() => BOMBundlesData.Search(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("RejectGoods")]
        public async Task<IHttpActionResult> RejectGoods(ActionRequest<BOMBundles> request)
        {
            BOMBundlesDataService BOMBundlesData = new BOMBundlesDataService();
            if (request.Item.id > 0)
            {
                bool result = await Task<bool>.Run(() => BOMBundlesData.Reject(request.Item));
                return this.Ok(result);
            }
            return this.NotFound();
        }


        [HttpPost]
        [Route("Save")]
        public async Task<IHttpActionResult> Save(ActionRequest<BOMBundles> request)
        {
            BOMBundlesDataService BOMBundlesData = new BOMBundlesDataService();
            if (request.Item.id <= 0)
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Create);
                BOMBundles result = await Task<BOMBundles>.Run(() => BOMBundlesData.Insert(request.Item));
                return this.Ok(result);
            }
            else
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Update);
                BOMBundles result = await Task<BOMBundles>.Run(() => BOMBundlesData.Update(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("Outward")]
        public async Task<IHttpActionResult> Outward(ActionRequest<BOMBundles> request)
        {
            BOMBundlesDataService BOMBundlesData = new BOMBundlesDataService();
            if (request.Item.id > 0)
            {
                BOMBundles result = await Task<BOMBundles>.Run(() => BOMBundlesData.Insert(request.Item));
                return this.Ok(result);
            }
            return this.NotFound();
        }


        [HttpPost]
        [Route("Inward")]
        public async Task<IHttpActionResult> Inward(ActionRequest<BOMBundles> request)
        {
            BOMBundlesDataService BOMBundlesData = new BOMBundlesDataService();
            if (request.Item.id > 0)
            {
                BOMBundles result = await Task<bool>.Run(() => BOMBundlesData.Inward(request.Item));
                return this.Ok(result);
            }
            return this.NotFound();
        }

        [HttpPost]
        [Route("Reject")]
        public async Task<IHttpActionResult> Reject(ActionRequest<BOMBundles> request)
        {
            BOMBundlesDataService BOMBundlesData = new BOMBundlesDataService();
            if (request.Item.id > 0)
            {
                bool result = await Task<bool>.Run(() => BOMBundlesData.Reject(request.Item));
                return this.Ok(result);
            }
            return this.NotFound();
        }

        [HttpPost]
        [Route("Update")]
        public async Task<IHttpActionResult> Update(ActionRequest<BOMBundles> request)
        {
            BOMBundlesDataService BOMBundlesData = new BOMBundlesDataService();
            BOMBundles result = await Task<BOMBundles>.Run(() => BOMBundlesData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IHttpActionResult> Delete(ActionRequest<BOMBundles> request)
        {
            BOMBundlesDataService BOMBundlesData = new BOMBundlesDataService();
            bool result = await Task<bool>.Run(() => BOMBundlesData.Delete(request.Item));
            return this.Ok(result);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("PrintPDF/{organizationid}/{id}")]
        public async Task<IHttpActionResult> PrintPDF(string organizationid,string id)
        {
            try
            {
                BOMBundlesDataService BOMBundlesData = new BOMBundlesDataService();
                BOMBundlesData.Context = new UserClaim()
                {
                    OrganizationID = long.Parse(organizationid)
                };
                BOMBundlesPrint result = BOMBundlesData.Print(long.Parse(id) );
                if (result != null)
                {
                    FileContentInfo byteArray = PrintReceipt(BOMBundlesData.Context.OrganizationID, "BOMSTOCKTRANSFEROUT", result);
                    return new FileActionResult(false, "", byteArray.Extension, byteArray.Content);
                }
                return null;
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(long.Parse(organizationid)).Log(exp, WebAPI.Common.Logger.LogType.Error, "PrintPDF/" + id);
                return this.Ok(exp);
            }
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("PrintInPDF/{organizationid}/{id}")]
        public async Task<IHttpActionResult> PrintInPDF(string organizationid, string id)
        {
            try
            {
                BOMBundlesDataService BOMBundlesData = new BOMBundlesDataService();
                BOMBundlesData.Context = new UserClaim()
                {
                    OrganizationID = long.Parse(organizationid)
                };
                BOMBundlesPrint result = BOMBundlesData.Print(long.Parse(id));
                if (result != null)
                {
                    FileContentInfo byteArray = PrintReceipt(BOMBundlesData.Context.OrganizationID, "BOMSTOCKTRANSFERIN", result);
                    return new FileActionResult(false, "", byteArray.Extension, byteArray.Content);
                }
                return null;
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(long.Parse(organizationid)).Log(exp, WebAPI.Common.Logger.LogType.Error, "PrintPDF/" + id);
                return this.Ok(exp);
            }
        }

        private FileContentInfo PrintReceipt(long organizationid,string key, BOMBundlesPrint result)
        {
            InfyPOS.Data.ConfigurationDataService config = new ConfigurationDataService();
            config.Context.OrganizationID = organizationid;
            var lstconfig = config.Search(new ConfigurationCreteria
            {
                key = key
            });

            if (lstconfig == null || lstconfig.Count == 0)
                return null;

            ReportLibrary.DotMatrixReporter labreport = new ReportLibrary.DotMatrixReporter(organizationid);
            labreport.format = ReportLibrary.DosFormatParser.ParseXML(config.Context.OrganizationID, lstconfig[0].reportformat.format);
            labreport.DefaultPageSettings = labreport.format.GetDefaultSetting();
            labreport.Additional = new SortedDictionary<string, object>();

            ReportLibrary.PDFPrinter printer = new ReportLibrary.PDFPrinter();
            printer.Report = labreport;
            printer.EnablePageNumber = lstconfig[0].reportformat.format.Contains("PageCount");
            labreport.DataSource = result;
            labreport.DataItems = result.groupitems;

            List<string> imageids = labreport.format.GetImages();
            var imageData = new Common.ImageData();
            imageData.Context.OrganizationID = organizationid;
            foreach (var imgid in imageids.FindAll(x => x.Length > 6))
            {
                var file = imageData.GetData(imgid.LongFromBase62());
                if (file != null && file.data != null)
                {
                    var img = imageData.FromBytes(file.data);
                    labreport.Additional.Add("IMG-" + imgid, img);
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
                fci.FileName = "stocktransfer.pdf";
                fci.Extension = "application/pdf";
                return fci;
            }

        }

    }
}