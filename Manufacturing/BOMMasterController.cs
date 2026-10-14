
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
    [RoutePrefix("BOMMasterService")]
    public class BOMMasterServiceController : ApiController
    {

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<BOMMaster>();
            result.Item = new BOMMaster();
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<BOMMasterCreteria> request)
        {
            BOMMasterDataService BOMMasterData = new BOMMasterDataService();
            List<BOMMaster> result = await Task<List<BOMMaster>>.Run(() => BOMMasterData.Search(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Save")]
        public async Task<IHttpActionResult> Save(ActionRequest<BOMMaster> request)
        {
            BOMMasterDataService BOMMasterData = new BOMMasterDataService();
            if (request.Item.id <= 0)
            {
                BOMMaster result = await Task<BOMMaster>.Run(() => BOMMasterData.Insert(request.Item));
                return this.Ok(result);
            }
            else
            {
                BOMMaster result = await Task<BOMMaster>.Run(() => BOMMasterData.Update(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("Insert")]
        public async Task<IHttpActionResult> Insert(ActionRequest<BOMMaster> request)
        {
            BOMMasterDataService BOMMasterData = new BOMMasterDataService();
            BOMMaster result = await Task<BOMMaster>.Run(() => BOMMasterData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        public async Task<IHttpActionResult> Update(ActionRequest<BOMMaster> request)
        {
            BOMMasterDataService BOMMasterData = new BOMMasterDataService();
            BOMMaster result = await Task<BOMMaster>.Run(() => BOMMasterData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IHttpActionResult> Delete(ActionRequest<BOMMaster> request)
        {
            BOMMasterDataService BOMMasterData = new BOMMasterDataService();
            bool result = await Task<bool>.Run(() => BOMMasterData.Delete(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("ChangeStatus")]
        public async Task<IHttpActionResult> ChangeStatus(ActionRequest<ChangeAttributes> request)
        {
            BOMMasterDataService MfgdesignData = new BOMMasterDataService();
            bool result = await Task<bool>.Run(() => MfgdesignData.ChangeStatus(request.Item));
            return this.Ok(result);
        }
    }
}