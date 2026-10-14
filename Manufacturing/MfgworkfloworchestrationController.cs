
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
    [RoutePrefix("MfgworkfloworchestrationService")]
    public class MfgworkfloworchestrationServiceController : ApiController
    {

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<Mfgworkfloworchestration>();
            result.Item = new Mfgworkfloworchestration();
            return this.Ok(result);
        }


        [HttpGet]
        [Route("FindStageList/{workorderid}")]
        public async Task<IHttpActionResult> FindStageList(string workorderid)
        {
            MfgworkfloworchestrationDataService MfgworkfloworchestrationData = new MfgworkfloworchestrationDataService();
            Mfgworkfloworchestration result = await Task<Mfgworkfloworchestration>.Run(() =>
            {
                MfgworkorderDataService mfgworkorderDataService = new MfgworkorderDataService();
                long woflowid = mfgworkorderDataService.Search(new MfgworkorderCreteria()
                {
                    id = long.Parse(workorderid)
                }).First().woflowid;

                return MfgworkfloworchestrationData.Search(
                     new MfgworkfloworchestrationCreteria()
                     {
                         id = woflowid
                     }).First();
            }
            );
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<MfgworkfloworchestrationCreteria> request)
        {
            MfgworkfloworchestrationDataService MfgworkfloworchestrationData = new MfgworkfloworchestrationDataService();
            List<Mfgworkfloworchestration> result = await Task<List<Mfgworkfloworchestration>>.Run(() => MfgworkfloworchestrationData.Search(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Save")]
        public async Task<IHttpActionResult> Save(ActionRequest<Mfgworkfloworchestration> request)
        {
            MfgworkfloworchestrationDataService MfgworkfloworchestrationData = new MfgworkfloworchestrationDataService();
            if (request.Item.id <= 0)
            {
                Mfgworkfloworchestration result = await Task<Mfgworkfloworchestration>.Run(() => MfgworkfloworchestrationData.Insert(request.Item));
                return this.Ok(result);
            }
            else
            {
                Mfgworkfloworchestration result = await Task<Mfgworkfloworchestration>.Run(() => MfgworkfloworchestrationData.Update(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("Insert")]
        public async Task<IHttpActionResult> Insert(ActionRequest<Mfgworkfloworchestration> request)
        {
            MfgworkfloworchestrationDataService MfgworkfloworchestrationData = new MfgworkfloworchestrationDataService();
            Mfgworkfloworchestration result = await Task<Mfgworkfloworchestration>.Run(() => MfgworkfloworchestrationData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        public async Task<IHttpActionResult> Update(ActionRequest<Mfgworkfloworchestration> request)
        {
            MfgworkfloworchestrationDataService MfgworkfloworchestrationData = new MfgworkfloworchestrationDataService();
            Mfgworkfloworchestration result = await Task<Mfgworkfloworchestration>.Run(() => MfgworkfloworchestrationData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IHttpActionResult> Delete(ActionRequest<Mfgworkfloworchestration> request)
        {
            MfgworkfloworchestrationDataService MfgworkfloworchestrationData = new MfgworkfloworchestrationDataService();
            bool result = await Task<bool>.Run(() => MfgworkfloworchestrationData.Delete(request.Item));
            return this.Ok(result);
        }
    }
}