
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
using Microsoft.Owin.Security;
using System.Security.Claims;

namespace InfyPOS.Controllers
{
    [RoutePrefix("TaxService")]
    public class TaxServiceController : ApiController
    {

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<Tax>();
            result.Item = new Tax();
            return this.Ok(result);
        }

        private IAuthenticationManager Authentication
        {
            get
            {
                ClaimsPrincipal principal = Request.GetRequestContext().Principal as ClaimsPrincipal;
                return Request.GetOwinContext().Authentication;
            }
        }

        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<TaxCreteria> request)
        {
            TaxDataService TaxData = new TaxDataService();
            List<Tax> result = await Task<List<Tax>>.Run(() => TaxData.Search(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Save")]
        [WebAPI.InfyAuthorize(PermissionSet.taxmaster, InfyPOS.Common.Action.Create)]
        public async Task<IHttpActionResult> Save(ActionRequest<Tax> request)
        {
            TaxDataService TaxData = new TaxDataService();
            if (request.Item.id <= 0)
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Create);
                Tax result = await Task<Tax>.Run(() => TaxData.Insert(request.Item));
                return this.Ok(result);
            }
            else
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Update);
                Tax result = await Task<Tax>.Run(() => TaxData.Update(request.Item));
                return this.Ok(result);
            }
        }


        [HttpPost]
        [Route("Merge")]
        [WebAPI.InfyAuthorize(PermissionSet.suppliermaster, InfyPOS.Common.Action.Modify)]
        public async Task<IHttpActionResult> Merge(ActionRequest<MergeTaxList> request)
        {
            TaxDataService TaxData = new TaxDataService();
            request.Item.ChangeStatus(BaseEntity.DataStatus.Update);
            MergeTaxList result = await Task<MergeTaxList>.Run(() => TaxData.Merge(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Insert")]
        [WebAPI.InfyAuthorize(PermissionSet.taxmaster, InfyPOS.Common.Action.Create)]
        public async Task<IHttpActionResult> Insert(ActionRequest<Tax> request)
        {
            TaxDataService TaxData = new TaxDataService();
            Tax result = await Task<Tax>.Run(() => TaxData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        [WebAPI.InfyAuthorize(PermissionSet.taxmaster, InfyPOS.Common.Action.Modify)]
        public async Task<IHttpActionResult> Update(ActionRequest<Tax> request)
        {
            TaxDataService TaxData = new TaxDataService();
            Tax result = await Task<Tax>.Run(() => TaxData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        [WebAPI.InfyAuthorize(PermissionSet.taxmaster, InfyPOS.Common.Action.Delete)]
        public async Task<IHttpActionResult> Delete(ActionRequest<Tax> request)
        {
            TaxDataService TaxData = new TaxDataService();
            bool result = await Task<bool>.Run(() => TaxData.Delete(request.Item));
            return this.Ok(result);
        }
    }
}