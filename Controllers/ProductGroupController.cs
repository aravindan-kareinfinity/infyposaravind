
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
    [RoutePrefix("ProductgroupService")]
    public class ProductgroupServiceController : ApiController
    {

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<Productgroup>();
            result.Item = new Productgroup();
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<ProductgroupCreteria> request)
        {
            ProductgroupDataService ProductgroupData = new ProductgroupDataService();
            List<Productgroup> result = await Task<List<Productgroup>>.Run(() => ProductgroupData.Search(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Save")]
        public async Task<IHttpActionResult> Save(ActionRequest<Productgroup> request)
        {
            ProductgroupDataService ProductgroupData = new ProductgroupDataService();
            if (request.Item.id <= 0)
            {
                Productgroup result = await Task<Productgroup>.Run(() => ProductgroupData.Insert(request.Item));
                return this.Ok(result);
            }
            else
            {
                Productgroup result = await Task<Productgroup>.Run(() => ProductgroupData.Update(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("Insert")]
        public async Task<IHttpActionResult> Insert(ActionRequest<Productgroup> request)
        {
            ProductgroupDataService ProductgroupData = new ProductgroupDataService();
            Productgroup result = await Task<Productgroup>.Run(() => ProductgroupData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        public async Task<IHttpActionResult> Update(ActionRequest<Productgroup> request)
        {
            ProductgroupDataService ProductgroupData = new ProductgroupDataService();
            Productgroup result = await Task<Productgroup>.Run(() => ProductgroupData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IHttpActionResult> Delete(ActionRequest<Productgroup> request)
        {
            ProductgroupDataService ProductgroupData = new ProductgroupDataService();
            bool result = await Task<bool>.Run(() => ProductgroupData.Delete(request.Item));
            return this.Ok(result);
        }
    }
}