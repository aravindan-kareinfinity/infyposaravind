
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
    [RoutePrefix("RechargeService")]
    public class RechargeServiceController : ApiController
    {

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<Recharge>();
            result.Item = new Recharge();
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<RechargeCreteria> request)
        {
            RechargeDataService RechargeData = new RechargeDataService();
            List<Recharge> result = await Task<List<Recharge>>.Run(() => RechargeData.Search(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Save")]
        public async Task<IHttpActionResult> Save(ActionRequest<RechargeCreteria> request)
        {
            RechargeDataService RechargeData = new RechargeDataService();
            if (request.Item.id <= 0)
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Create);
                Recharge result = await Task<Recharge>.Run(() => RechargeData.Insert(request.Item));
                return this.Ok(result);
            }
            else
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Update);
                Recharge result = await Task<Recharge>.Run(() => RechargeData.Update(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("Insert")]
        public async Task<IHttpActionResult> Insert(ActionRequest<RechargeCreteria> request)
        {
            RechargeDataService RechargeData = new RechargeDataService();
            Recharge result = await Task<Recharge>.Run(() => RechargeData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        public async Task<IHttpActionResult> Update(ActionRequest<Recharge> request)
        {
            RechargeDataService RechargeData = new RechargeDataService();
            Recharge result = await Task<Recharge>.Run(() => RechargeData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IHttpActionResult> Delete(ActionRequest<RechargeCreteria> request)
        {
            RechargeDataService RechargeData = new RechargeDataService();
            bool result = await Task<bool>.Run(() => RechargeData.Delete(request.Item));
            return this.Ok(result);
        }
    }
}