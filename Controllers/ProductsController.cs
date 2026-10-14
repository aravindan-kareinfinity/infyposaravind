
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
    [RoutePrefix("ProductsService")]
    public class ProductsServiceController : ApiController
    {

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<Products>();
            result.Item = new Products();
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Attributelist")]
        public async Task<IHttpActionResult> Save(ActionRequest<ProductsBase> request)
        {
            ProductsDataService ProductsData = new ProductsDataService();
            bool result = await Task<bool>.Run(() => ProductsData.UpdateAttributelist(request.Item));
            return this.Ok(result);
        }

        [HttpGet]
        [Route("Attributelist/{id}")]
        public async Task<IHttpActionResult> Attributelist(string id)
        {
            ProductsDataService SupplierData = new ProductsDataService();
            ProductsBase result = await Task<ProductsBase>.Run(() => SupplierData.SearchList(new ProductsCreteria()
            {
                id = long.Parse(id)
            }).First());
            return this.Ok(result.attributelist);
        }

        [HttpPost]
        [Route("ChangeHSN")]
        [WebAPI.InfyAuthorize(PermissionSet.productmaster, InfyPOS.Common.Action.Modify)]
        public async Task<IHttpActionResult> ChangeHSN(ActionMulitpleRequest<ProductHSNCode> request)
        {
            ProductsDataService ProductsData = new ProductsDataService();
            bool result = await Task<bool>.Run(() => ProductsData.ChangeHSN(request.Items));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("ChangeProductGroup")]
        [WebAPI.InfyAuthorize(PermissionSet.productmaster, InfyPOS.Common.Action.Modify)]
        public async Task<IHttpActionResult> ChangeProductGroup(ActionRequest<ProductChangeGroup> request)
        {
            ProductsDataService ProductsData = new ProductsDataService();
            bool result = await Task<bool>.Run(() => ProductsData.ChangeProductGroup(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("ChangeTax")]
        [WebAPI.InfyAuthorize(PermissionSet.productmaster, InfyPOS.Common.Action.Modify)]
        public async Task<IHttpActionResult> ChangeTax(ActionRequest<ProductChangeTax> request)
        {
            ProductsDataService ProductsData = new ProductsDataService();
            bool result = await Task<bool>.Run(() => ProductsData.ChangeTax(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<ProductsCreteria> request)
        {
            ProductsDataService ProductsData = new ProductsDataService();
            List<Products> result = await Task<List<Products>>.Run(() => ProductsData.Search(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Save")]
        [WebAPI.InfyAuthorize(PermissionSet.productmaster, InfyPOS.Common.Action.Create)]
        public async Task<IHttpActionResult> Save(ActionRequest<Products> request)
        {
            ProductsDataService ProductsData = new ProductsDataService();
            if (request.Item.id <= 0)
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Create);
                Products result = await Task<Products>.Run(() => ProductsData.Insert(request.Item));
                return this.Ok(result);
            }
            else
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Update);
                Products result = await Task<Products>.Run(() => ProductsData.Update(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("Insert")]
        [WebAPI.InfyAuthorize(PermissionSet.productmaster, InfyPOS.Common.Action.Create)]
        public async Task<IHttpActionResult> Insert(ActionRequest<Products> request)
        {
            ProductsDataService ProductsData = new ProductsDataService();
            Products result = await Task<Products>.Run(() => ProductsData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        [WebAPI.InfyAuthorize(PermissionSet.productmaster, InfyPOS.Common.Action.Modify)]
        public async Task<IHttpActionResult> Update(ActionRequest<Products> request)
        {
            ProductsDataService ProductsData = new ProductsDataService();
            Products result = await Task<Products>.Run(() => ProductsData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        [WebAPI.InfyAuthorize(PermissionSet.productmaster, InfyPOS.Common.Action.Delete)]
        public async Task<IHttpActionResult> Delete(ActionRequest<Products> request)
        {
            ProductsDataService ProductsData = new ProductsDataService();
            bool result = await Task<bool>.Run(() => ProductsData.Delete(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Merge")]
        [WebAPI.InfyAuthorize(PermissionSet.productmaster, InfyPOS.Common.Action.Create)]
        public async Task<IHttpActionResult> Merge(ActionRequest<MergeProductsList> request)
        {
            ProductsDataService ReferencelistData = new ProductsDataService();
            bool result = await Task<bool>.Run(() => ReferencelistData.Marge(request.Item));
            return this.Ok(result);
        }
    }
}