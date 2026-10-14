
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
    [RoutePrefix("DiscountqueueService")]
    public class DiscountqueueServiceController : ApiController
    {

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<Discountqueue>();
            result.Item = new Discountqueue();
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<DiscountqueueCreteria> request)
        {
            DiscountqueueDataService DiscountqueueData = new DiscountqueueDataService();
            List<Discountqueue> result = await Task<List<Discountqueue>>.Run(() => DiscountqueueData.Search(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Save")]
        public async Task<IHttpActionResult> Save(ActionRequest<Discountqueue> request)
        {
            DiscountqueueDataService DiscountqueueData = new DiscountqueueDataService();
            if (request.Item.id <= 0)
            {
                Discountqueue result = await Task<Discountqueue>.Run(() => DiscountqueueData.Insert(request.Item));
                return this.Ok(result);
            }
            else
            {
                Discountqueue result = await Task<Discountqueue>.Run(() => DiscountqueueData.Update(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("Insert")]
        public async Task<IHttpActionResult> Insert(ActionRequest<Discountqueue> request)
        {
            DiscountqueueDataService DiscountqueueData = new DiscountqueueDataService();
            Discountqueue result = await Task<Discountqueue>.Run(() => DiscountqueueData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        public async Task<IHttpActionResult> Update(ActionRequest<Discountqueue> request)
        {
            DiscountqueueDataService DiscountqueueData = new DiscountqueueDataService();
            Discountqueue result = await Task<Discountqueue>.Run(() => DiscountqueueData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IHttpActionResult> Delete(ActionRequest<Discountqueue> request)
        {
            DiscountqueueDataService DiscountqueueData = new DiscountqueueDataService();
            bool result = await Task<bool>.Run(() => DiscountqueueData.Delete(request.Item));
            return this.Ok(result);
        }
    }
}