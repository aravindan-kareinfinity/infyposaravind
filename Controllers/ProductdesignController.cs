
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
    [RoutePrefix("ProductdesignService")]
    public class ProductdesignServiceController : ApiController
    {
        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<Productdesign>();
            result.Item = new Productdesign();
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Sync")]
        public async Task<IHttpActionResult> Sync(ActionRequest<ProductdesignCreteria> request)
        {
            ProductdesignDataService ProductdesignData = new ProductdesignDataService();
            List<Productdesign> result = await Task<List<Productdesign>>.Run(() => ProductdesignData.Sync(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Merge")]
        public async Task<IHttpActionResult> Import(ActionRequest<MergeProductDesign> request)
        {
            ProductdesignDataService ProductdesignData = new ProductdesignDataService();
            bool result = await Task<bool>.Run(() => ProductdesignData.MergeDesign(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Import")]
        public async Task<IHttpActionResult> Import(ActionRequest<ProductdesignCreteria> request)
        {
            ProductdesignDataService ProductdesignData = new ProductdesignDataService();
            List<Productdesign> result = await Task<List<Productdesign>>.Run(() => ProductdesignData.Import(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("SearchDesign")]
        public async Task<IHttpActionResult> SearchDesign(ActionRequest<ProductdesignCreteria> request)
        {
            ProductdesignDataService ProductdesignData = new ProductdesignDataService();
            List<ProductdesignBase> result = await Task<List<ProductdesignBase>>.Run(() => ProductdesignData.SearchDesignOnly(request.Item));
            return this.Ok(result);

        }
        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<ProductdesignCreteria> request)
        {
            ProductdesignDataService ProductdesignData = new ProductdesignDataService();
            List<Productdesign> result = await Task<List<Productdesign>>.Run(() => ProductdesignData.Search(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("SearchImage")]
        public async Task<IHttpActionResult> SearchImage(ActionRequest<ProductdesignCreteria> request)
        {
            ProductdesignDataService ProductdesignData = new ProductdesignDataService();
            Productdesign result = await Task<Productdesign>.Run(() => ProductdesignData.SearchImage(request.Item));
            return this.Ok(result);
        }


        [HttpPost]
        [Route("SearchImages")]
        [Route("SearchDesignImages")]
        public async Task<IHttpActionResult> SearchDesignImage(ActionRequest<ProductdesignCreteria> request)
        {
            ProductdesignDataService ProductdesignData = new ProductdesignDataService();
            List<ProductDesignImage> result = await Task<List<ProductDesignImage>>.Run(() => ProductdesignData.SearchDesignImage(request.Item));
            return this.Ok(result);
        }


        [HttpPost]
        [Route("SaveImage")]
        public async Task<IHttpActionResult> SaveImage(ActionRequest<Productdesign> request)
        {
            ProductdesignDataService ProductdesignData = new ProductdesignDataService();
            Productdesign result = await Task<Productdesign>.Run(() => ProductdesignData.SaveImage(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Save")]
        public async Task<IHttpActionResult> Save(ActionRequest<Productdesign> request)
        {
            ProductdesignDataService ProductdesignData = new ProductdesignDataService();
            if (request.Item.id <= 0)
            {
                Productdesign result = await Task<Productdesign>.Run(() => ProductdesignData.Insert(request.Item));
                return this.Ok(result);
            }
            else
            {
                Productdesign result = await Task<Productdesign>.Run(() => ProductdesignData.Update(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("Insert")]
        public async Task<IHttpActionResult> Insert(ActionRequest<Productdesign> request)
        {
            ProductdesignDataService ProductdesignData = new ProductdesignDataService();
            Productdesign result = await Task<Productdesign>.Run(() => ProductdesignData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        public async Task<IHttpActionResult> Update(ActionRequest<Productdesign> request)
        {
            ProductdesignDataService ProductdesignData = new ProductdesignDataService();
            Productdesign result = await Task<Productdesign>.Run(() => ProductdesignData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IHttpActionResult> Delete(ActionRequest<Productdesign> request)
        {
            ProductdesignDataService ProductdesignData = new ProductdesignDataService();
            bool result = await Task<bool>.Run(() => ProductdesignData.Delete(request.Item));
            return this.Ok(result);
        }
    }
}