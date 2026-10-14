
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
    [RoutePrefix("GatepassService")]
    public class GatepassServiceController : ApiController
    {

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<Gatepass>();
            result.Item = new Gatepass();
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<GatepassCreteria> request)
        {
            GatepassDataService GatepassData = new GatepassDataService();
            List<Gatepass> result = await Task<List<Gatepass>>.Run(() => GatepassData.Search(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Save")]
        public async Task<IHttpActionResult> Save(ActionRequest<Gatepass> request)
        {
            GatepassDataService GatepassData = new GatepassDataService();
            if (request.Item.id <= 0)
            {
                Gatepass result = await Task<Gatepass>.Run(() => GatepassData.Insert(request.Item));
                return this.Ok(result);
            }
            else
            {
                Gatepass result = await Task<Gatepass>.Run(() => GatepassData.Update(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("Defaults")]
        public async Task<IHttpActionResult> Defaults(ActionRequest<GatepassCreteria> request)
        {
            GatepassDataService GatepassData = new GatepassDataService();
            Gatepass result = await Task<Gatepass>.Run(() => GatepassData.Defaults(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Insert")]
        public async Task<IHttpActionResult> Insert(ActionRequest<Gatepass> request)
        {
            GatepassDataService GatepassData = new GatepassDataService();
            Gatepass result = await Task<Gatepass>.Run(() => GatepassData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        public async Task<IHttpActionResult> Update(ActionRequest<Gatepass> request)
        {
            GatepassDataService GatepassData = new GatepassDataService();
            Gatepass result = await Task<Gatepass>.Run(() => GatepassData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IHttpActionResult> Delete(ActionRequest<Gatepass> request)
        {
            GatepassDataService GatepassData = new GatepassDataService();
            bool result = await Task<bool>.Run(() => GatepassData.Delete(request.Item));
            return this.Ok(result);
        }
    }
}