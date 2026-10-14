
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
    [RoutePrefix("RechargebalanceService")]
    public class RechargebalanceServiceController : ApiController
    {

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<Rechargebalance>();
            result.Item = new Rechargebalance();
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<RechargebalanceCreteria> request)
        {
            RechargebalanceDataService RechargebalanceData = new RechargebalanceDataService();
            List<Rechargebalance> result = await Task<List<Rechargebalance>>.Run(() => RechargebalanceData.Search(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Save")]
        public async Task<IHttpActionResult> Save(ActionRequest<Rechargebalance> request)
        {
            RechargebalanceDataService RechargebalanceData = new RechargebalanceDataService();
            if (request.Item.id <= 0)
            {
                Rechargebalance result = await Task<Rechargebalance>.Run(() => RechargebalanceData.Insert(request.Item));
                return this.Ok(result);
            }
            else
            {
                Rechargebalance result = await Task<Rechargebalance>.Run(() => RechargebalanceData.Update(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("Insert")]
        public async Task<IHttpActionResult> Insert(ActionRequest<Rechargebalance> request)
        {
            RechargebalanceDataService RechargebalanceData = new RechargebalanceDataService();
            Rechargebalance result = await Task<Rechargebalance>.Run(() => RechargebalanceData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        public async Task<IHttpActionResult> Update(ActionRequest<Rechargebalance> request)
        {
            RechargebalanceDataService RechargebalanceData = new RechargebalanceDataService();
            Rechargebalance result = await Task<Rechargebalance>.Run(() => RechargebalanceData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IHttpActionResult> Delete(ActionRequest<Rechargebalance> request)
        {
            RechargebalanceDataService RechargebalanceData = new RechargebalanceDataService();
            bool result = await Task<bool>.Run(() => RechargebalanceData.Delete(request.Item));
            return this.Ok(result);
        }
    }
}