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
    [RoutePrefix("BrandService")]
    public class BrandServiceController : ApiController
    {

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<Brand>();
            result.Item = new Brand();
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Search")]
        [WebAPI.InfyAuthorize(PermissionSet.brandmaster, InfyPOS.Common.Action.View)]
        public async Task<IHttpActionResult> Search(ActionRequest<BrandCreteria> request)
        {
            BrandDataService BrandData = new BrandDataService();
            List<Brand> result = await Task<List<Brand>>.Run(() => BrandData.Search(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Save")]
        [WebAPI.InfyAuthorize(PermissionSet.brandmaster, InfyPOS.Common.Action.Create)]
        public async Task<IHttpActionResult> Save(ActionRequest<Brand> request)
        {
            BrandDataService BrandData = new BrandDataService();
            if (request.Item.id <= 0)
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Create);
                Brand result = await Task<Brand>.Run(() => BrandData.Insert(request.Item));
                return this.Ok(result);
            }
            else
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Update);
                Brand result = await Task<Brand>.Run(() => BrandData.Update(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("Insert")]
        [WebAPI.InfyAuthorize(PermissionSet.brandmaster, InfyPOS.Common.Action.Create)]
        public async Task<IHttpActionResult> Insert(ActionRequest<Brand> request)
        {
            BrandDataService BrandData = new BrandDataService();
            Brand result = await Task<Brand>.Run(() => BrandData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        [WebAPI.InfyAuthorize(PermissionSet.brandmaster, InfyPOS.Common.Action.Modify)]
        public async Task<IHttpActionResult> Update(ActionRequest<Brand> request)
        {
            BrandDataService BrandData = new BrandDataService();
            Brand result = await Task<Brand>.Run(() => BrandData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        [WebAPI.InfyAuthorize(PermissionSet.brandmaster, InfyPOS.Common.Action.Delete)]
        public async Task<IHttpActionResult> Delete(ActionRequest<Brand> request)
        {
            BrandDataService BrandData = new BrandDataService();
            bool result = await Task<bool>.Run(() => BrandData.Delete(request.Item));
            return this.Ok(result);
        }


        [HttpPost]
        [Route("Merge")]
        [WebAPI.InfyAuthorize(PermissionSet.productmaster, InfyPOS.Common.Action.Create)]
        public async Task<IHttpActionResult> Merge(ActionRequest<MergeBrandList> request)
        {
            BrandDataService ReferencelistData = new BrandDataService();
            bool result = await Task<bool>.Run(() => ReferencelistData.Marge(request.Item));
            return this.Ok(result);
        }
    }
}