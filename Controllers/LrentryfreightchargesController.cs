
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
    [RoutePrefix("LrentryfreightchargesService")]
    public class LrentryfreightchargesServiceController : ApiController
    {

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<Lrentryfreightcharges>();
            result.Item = new Lrentryfreightcharges();
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<LrentryfreightchargesCreteria> request)
        {
            LrentryfreightchargesDataService LrentryfreightchargesData = new LrentryfreightchargesDataService();
            List<Lrentryfreightcharges> result = await Task<List<Lrentryfreightcharges>>.Run(() => LrentryfreightchargesData.Search(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Save")]
        public async Task<IHttpActionResult> Save(ActionRequest<Lrentryfreightcharges> request)
        {
            LrentryfreightchargesDataService LrentryfreightchargesData = new LrentryfreightchargesDataService();
            if (request.Item.id <= 0)
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Create);
                Lrentryfreightcharges result = await Task<Lrentryfreightcharges>.Run(() => LrentryfreightchargesData.Insert(request.Item));
                return this.Ok(result);
            }
            else
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Update);
                Lrentryfreightcharges result = await Task<Lrentryfreightcharges>.Run(() => LrentryfreightchargesData.Update(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("Insert")]
        public async Task<IHttpActionResult> Insert(ActionRequest<Lrentryfreightcharges> request)
        {
            LrentryfreightchargesDataService LrentryfreightchargesData = new LrentryfreightchargesDataService();
            Lrentryfreightcharges result = await Task<Lrentryfreightcharges>.Run(() => LrentryfreightchargesData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        public async Task<IHttpActionResult> Update(ActionRequest<Lrentryfreightcharges> request)
        {
            LrentryfreightchargesDataService LrentryfreightchargesData = new LrentryfreightchargesDataService();
            Lrentryfreightcharges result = await Task<Lrentryfreightcharges>.Run(() => LrentryfreightchargesData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IHttpActionResult> Delete(ActionRequest<Lrentryfreightcharges> request)
        {
            LrentryfreightchargesDataService LrentryfreightchargesData = new LrentryfreightchargesDataService();
            bool result = await Task<bool>.Run(() => LrentryfreightchargesData.Delete(request.Item));
            return this.Ok(result);
        }
    }
}