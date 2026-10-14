
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
    [RoutePrefix("MfgworkorderinboxService")]
    public class MfgworkorderinboxServiceController : ApiController
    {

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<Mfgworkorderinbox>();
            result.Item = new Mfgworkorderinbox();
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<MfgworkorderinboxCreteria> request)
        {
            MfgworkorderinboxDataService MfgworkorderinboxData = new MfgworkorderinboxDataService();
            List<Mfgworkorderinbox> result = await Task<List<Mfgworkorderinbox>>.Run(() => MfgworkorderinboxData.Search(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Save")]
        public async Task<IHttpActionResult> Save(ActionRequest<Mfgworkorderinbox> request)
        {
            MfgworkorderinboxDataService MfgworkorderinboxData = new MfgworkorderinboxDataService();
            if (request.Item.id <= 0)
            {
                Mfgworkorderinbox result = await Task<Mfgworkorderinbox>.Run(() => MfgworkorderinboxData.Insert(request.Item));
                return this.Ok(result);
            }
            else
            {
                Mfgworkorderinbox result = await Task<Mfgworkorderinbox>.Run(() => MfgworkorderinboxData.Update(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("UpdateStatus")]
        public async Task<IHttpActionResult> UpdateStatus(ActionMulitpleRequest<MfgworkorderinboxStatus> request)
        {
            MfgworkorderinboxDataService MfgworkorderinboxData = new MfgworkorderinboxDataService();
            List<MfgworkorderinboxStatus> result = await Task<List<MfgworkorderinboxStatus>>.Run(() => MfgworkorderinboxData.UpdateStatus(request.Items));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Insert")]
        public async Task<IHttpActionResult> Insert(ActionRequest<Mfgworkorderinbox> request)
        {
            MfgworkorderinboxDataService MfgworkorderinboxData = new MfgworkorderinboxDataService();
            Mfgworkorderinbox result = await Task<Mfgworkorderinbox>.Run(() => MfgworkorderinboxData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        public async Task<IHttpActionResult> Update(ActionRequest<Mfgworkorderinbox> request)
        {
            MfgworkorderinboxDataService MfgworkorderinboxData = new MfgworkorderinboxDataService();
            Mfgworkorderinbox result = await Task<Mfgworkorderinbox>.Run(() => MfgworkorderinboxData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IHttpActionResult> Delete(ActionRequest<Mfgworkorderinbox> request)
        {
            MfgworkorderinboxDataService MfgworkorderinboxData = new MfgworkorderinboxDataService();
            bool result = await Task<bool>.Run(() => MfgworkorderinboxData.Delete(request.Item));
            return this.Ok(result);
        }
    }
}