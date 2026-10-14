
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
    [RoutePrefix("RefillboxService")]
    public class RefillboxServiceController : ApiController
    {

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<Refillbox>();
            result.Item = new Refillbox();
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<RefillboxCreteria> request)
        {
            RefillboxDataService RefillboxData = new RefillboxDataService();
            List<Refillbox> result = await Task<List<Refillbox>>.Run(() => RefillboxData.Search(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Save")]
        public async Task<IHttpActionResult> Save(ActionRequest<Refillbox> request)
        {
            RefillboxDataService RefillboxData = new RefillboxDataService();
            if (request.Item.id <= 0)
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Create);
                Refillbox result = await Task<Refillbox>.Run(() => RefillboxData.Insert(request.Item));
                return this.Ok(result);
            }
            else
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Update);
                Refillbox result = await Task<Refillbox>.Run(() => RefillboxData.Update(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("Insert")]
        public async Task<IHttpActionResult> Insert(ActionRequest<Refillbox> request)
        {
            RefillboxDataService RefillboxData = new RefillboxDataService();
            Refillbox result = await Task<Refillbox>.Run(() => RefillboxData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        public async Task<IHttpActionResult> Update(ActionRequest<Refillbox> request)
        {
            RefillboxDataService RefillboxData = new RefillboxDataService();
            Refillbox result = await Task<Refillbox>.Run(() => RefillboxData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IHttpActionResult> Delete(ActionRequest<Refillbox> request)
        {
            RefillboxDataService RefillboxData = new RefillboxDataService();
            bool result = await Task<bool>.Run(() => RefillboxData.Delete(request.Item));
            return this.Ok(result);
        }
    }
}