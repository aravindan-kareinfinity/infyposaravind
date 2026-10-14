
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
    [RoutePrefix("IncentiveconfigService")]
    public class IncentiveconfigServiceController : ApiController
    {

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<Incentiveconfig>();
            result.Item = new Incentiveconfig();
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<IncentiveconfigCreteria> request)
        {
            IncentiveconfigDataService IncentiveconfigData = new IncentiveconfigDataService();
            List<Incentiveconfig> result = await Task<List<Incentiveconfig>>.Run(() => IncentiveconfigData.Search(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("SaveAll")]
        public async Task<IHttpActionResult> SaveAll(ActionRequest<IncentiveUpdate> request)
        {
            IncentiveconfigDataService IncentiveconfigData = new IncentiveconfigDataService();
            IncentiveUpdate result = await Task<IncentiveUpdate>.Run(() => IncentiveconfigData.SaveAll(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("SearchBarcode")]
        public async Task<IHttpActionResult> SearchBarcode(ActionRequest<IncentiveconfigCreteria> request)
        {
            IncentiveconfigDataService IncentiveconfigData = new IncentiveconfigDataService();
            List<IncentiveItem> result = await Task<List<IncentiveItem>>.Run(() => IncentiveconfigData.SearchBarcode(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Save")]
        public async Task<IHttpActionResult> Save(ActionRequest<Incentiveconfig> request)
        {
            IncentiveconfigDataService IncentiveconfigData = new IncentiveconfigDataService();
            if (request.Item.id <= 0)
            {
                Incentiveconfig result = await Task<Incentiveconfig>.Run(() => IncentiveconfigData.Insert(request.Item));
                return this.Ok(result);
            }
            else
            {
                Incentiveconfig result = await Task<Incentiveconfig>.Run(() => IncentiveconfigData.Update(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("Insert")]
        public async Task<IHttpActionResult> Insert(ActionRequest<Incentiveconfig> request)
        {
            IncentiveconfigDataService IncentiveconfigData = new IncentiveconfigDataService();
            Incentiveconfig result = await Task<Incentiveconfig>.Run(() => IncentiveconfigData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        public async Task<IHttpActionResult> Update(ActionRequest<Incentiveconfig> request)
        {
            IncentiveconfigDataService IncentiveconfigData = new IncentiveconfigDataService();
            Incentiveconfig result = await Task<Incentiveconfig>.Run(() => IncentiveconfigData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IHttpActionResult> Delete(ActionRequest<IncentiveconfigCreteria> request)
        {
            IncentiveconfigDataService IncentiveconfigData = new IncentiveconfigDataService();
            bool result = await Task<bool>.Run(() => IncentiveconfigData.Delete(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("DeleteAll")]
        public async Task<IHttpActionResult> DeleteAll(ActionRequest<IncentiveconfigCreteria> request)
        {
            IncentiveconfigDataService IncentiveconfigData = new IncentiveconfigDataService();
            bool result = await Task<bool>.Run(() => IncentiveconfigData.DeleteAll(request.Item));
            return this.Ok(result);
        }
    }
}