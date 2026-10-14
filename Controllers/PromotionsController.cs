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
    [RoutePrefix("PromotionsService")]
    public class PromotionsServiceController : ApiController
    {

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<Promotions>();
            result.Item = new Promotions();
            return this.Ok(result);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("DownloadBarcodes/{schemeid}/{mode}")]
        public async Task<IHttpActionResult> DownloadSchema(string schemeid, string mode)
        {
            FileContentInfo byteArray = await Task<FileContentInfo>.Run(() => DownloadBarcodes(schemeid, mode));
            if (byteArray == null)
                return this.NotFound();
            return new FileActionResult(false, "", byteArray.Extension, byteArray.Content);
        }

        private FileContentInfo DownloadBarcodes(string schemeid,string mode)
        {
            List<string> barcodeList = null;
            if(mode == "include")
            {
                SchemebarcodesDataService schemebarcodesDataService = new SchemebarcodesDataService();
                barcodeList = schemebarcodesDataService.Search(new SchemebarcodesCreteria()
                {
                    schemeid = long.Parse(schemeid)
                }).ConvertAll(e=>e.barcode);
            }
            else
            {
                SchemeExcludedbarcodesDataService schemebarcodesDataService = new SchemeExcludedbarcodesDataService();
                barcodeList = schemebarcodesDataService.Search(new  SchemebarcodesCreteria()
                {
                    schemeid = long.Parse(schemeid)
                }).ConvertAll(e => e.barcode);
            }

            System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();
            barcodeList.ForEach(e =>
            {
                stringBuilder.AppendLine(e);
            });

            var fci = new FileContentInfo(new ByteArrayContent(System.Text.ASCIIEncoding.ASCII.GetBytes(stringBuilder.ToString())));
            fci.FileName = "barcodes.txt";
            fci.Extension = "text/plain";
            return fci;
        }

        [HttpPost]
        [Route("RemoveBarcode")]
        [WebAPI.InfyAuthorize(PermissionSet.salepromotion, InfyPOS.Common.Action.Delete)]
        public async Task<IHttpActionResult> RemoveBarcode(ActionRequest<PromotionBarcode> request)
        {
            PromotionsDataService PromotionsData = new PromotionsDataService();
            bool result = await Task<bool>.Run(() => PromotionsData.RemoveBarcode(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<PromotionsCreteria> request)
        {
            PromotionsDataService PromotionsData = new PromotionsDataService();
            List<Promotions> result = await Task<List<Promotions>>.Run(() => PromotionsData.Search(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("SearchItems")]
        public async Task<IHttpActionResult> SearchItems(ActionRequest<PromotionSearch> request)
        {
            PromotionsDataService PromotionsData = new PromotionsDataService();
            List<PromotionItem> result = await Task<List<PromotionItem>>.Run(() => PromotionsData.SearchItems(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("UpdateRule")]
        [WebAPI.InfyAuthorize(PermissionSet.salepromotion, InfyPOS.Common.Action.Modify)]
        public async Task<IHttpActionResult> UpdateRule(ActionRequest<PromotionUpdate> request)
        {
            PromotionsDataService PromotionsData = new PromotionsDataService();
            bool result = await Task<bool>.Run(() => PromotionsData.UpdateRule(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("SearchPromotion")]
        public async Task<IHttpActionResult> SearchPromotion(ActionRequest<PromotionSearch> request)
        {
            PromotionsDataService PromotionsData = new PromotionsDataService();
            long companyid = 0;
            if (InfyPOS.Services.ConfigManager.Instance(new BaseData().Context.OrganizationID).Config.system_supportcompanywisebarcode)
            {
                if (PromotionsData.Context.CompanyID <= 0)
                    companyid = long.MaxValue;
                else
                    companyid = PromotionsData.Context.CompanyID;
            }
            request.Item.compayid = companyid;
            List<Services.PromotionManager.ElibleReason> result = await Task<List<Services.PromotionManager.ElibleReason>>.Run(() => PromotionsData.SearchPromotion(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("UploadHyperPromotion")]
        [WebAPI.InfyAuthorize(PermissionSet.salepromotion, InfyPOS.Common.Action.Create)]
        public async Task<IHttpActionResult> UploadHyperPromotion(ActionRequest<PromotionUpload> request)
        {
            PromotionsDataService PromotionsData = new PromotionsDataService();
            List<Promotions> result = await Task<Promotions>.Run(() => PromotionsData.InsertAll(request.Item));
            return this.Ok(result);
        }


        [HttpPost]
        [Route("Save")]
        [WebAPI.InfyAuthorize(PermissionSet.salepromotion, InfyPOS.Common.Action.Create)]
        public async Task<IHttpActionResult> Save(ActionRequest<Promotions> request)
        {
            PromotionsDataService PromotionsData = new PromotionsDataService();
            if (request.Item.id <= 0)
            {
                Promotions result = await Task<Promotions>.Run(() => PromotionsData.Insert(request.Item));
                return this.Ok(result);
            }
            else
            {
                Promotions result = await Task<Promotions>.Run(() => PromotionsData.Update(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("Insert")]
        [WebAPI.InfyAuthorize(PermissionSet.salepromotion, InfyPOS.Common.Action.Create)]
        public async Task<IHttpActionResult> Insert(ActionRequest<Promotions> request)
        {
            PromotionsDataService PromotionsData = new PromotionsDataService();
            Promotions result = await Task<Promotions>.Run(() => PromotionsData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("UpdateSchemeLocation")]
        [WebAPI.InfyAuthorize(PermissionSet.salepromotion, InfyPOS.Common.Action.Create)]
        public async Task<IHttpActionResult> UpdateSchemeLocation(ActionRequest<PromotionsCreteria> request)
        {
            PromotionsDataService PromotionsData = new PromotionsDataService();
            bool result = await Task<bool>.Run(() => PromotionsData.UpdateSchemeLocation(request.Item));
            return this.Ok(result);
        }
        [HttpPost]
        [Route("Clone")]
        [WebAPI.InfyAuthorize(PermissionSet.salepromotion, InfyPOS.Common.Action.Delete)]
        public async Task<IHttpActionResult> Clone(ActionRequest<Promotions> request)
        {
            PromotionsDataService PromotionsData = new PromotionsDataService();
            Promotions result = await Task<Promotions>.Run(() => PromotionsData.Clone(request.Item));
            return this.Ok(result);
        }
        [HttpPost]
        [Route("Update")]
        [WebAPI.InfyAuthorize(PermissionSet.salepromotion, InfyPOS.Common.Action.Modify)]
        public async Task<IHttpActionResult> Update(ActionRequest<Promotions> request)
        {
            PromotionsDataService PromotionsData = new PromotionsDataService();
            Promotions result = await Task<Promotions>.Run(() => PromotionsData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        [WebAPI.InfyAuthorize(PermissionSet.salepromotion, InfyPOS.Common.Action.Delete)]
        public async Task<IHttpActionResult> Delete(ActionRequest<Promotions> request)
        {
            PromotionsDataService PromotionsData = new PromotionsDataService();
            bool result = await Task<bool>.Run(() => PromotionsData.Delete(request.Item));
            return this.Ok(result);
        }
        [HttpPost]
        [Route("Deactivate")]
        [WebAPI.InfyAuthorize(PermissionSet.salepromotion, InfyPOS.Common.Action.Delete)]
        public async Task<IHttpActionResult> Deactivate(ActionRequest<Promotions> request)
        {
            PromotionsDataService PromotionsData = new PromotionsDataService();
            bool result = await Task<bool>.Run(() => PromotionsData.Deactivate(request.Item));
            return this.Ok(result);
        }
    }
}