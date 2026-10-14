
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
    [RoutePrefix("MfgtailoringwagesService")]
    public class MfgtailoringwagesServiceController : ApiController
    {

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<Mfgtailoringwages>();
            result.Item = new Mfgtailoringwages();
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<MfgtailoringwagesCreteria> request)
        {
            MfgtailoringwagesDataService MfgtailoringwagesData = new MfgtailoringwagesDataService();
            List<Mfgtailoringwages> result = await Task<List<Mfgtailoringwages>>.Run(() => MfgtailoringwagesData.Search(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Save")]
        public async Task<IHttpActionResult> Save(ActionRequest<Mfgtailoringwages> request)
        {
            MfgtailoringwagesDataService MfgtailoringwagesData = new MfgtailoringwagesDataService();
            if (request.Item.id <= 0)
            {
                Mfgtailoringwages result = await Task<Mfgtailoringwages>.Run(() => MfgtailoringwagesData.Insert(request.Item));
                return this.Ok(result);
            }
            else
            {
                Mfgtailoringwages result = await Task<Mfgtailoringwages>.Run(() => MfgtailoringwagesData.Update(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("Insert")]
        public async Task<IHttpActionResult> Insert(ActionRequest<Mfgtailoringwages> request)
        {
            MfgtailoringwagesDataService MfgtailoringwagesData = new MfgtailoringwagesDataService();
            Mfgtailoringwages result = await Task<Mfgtailoringwages>.Run(() => MfgtailoringwagesData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        public async Task<IHttpActionResult> Update(ActionRequest<Mfgtailoringwages> request)
        {
            MfgtailoringwagesDataService MfgtailoringwagesData = new MfgtailoringwagesDataService();
            Mfgtailoringwages result = await Task<Mfgtailoringwages>.Run(() => MfgtailoringwagesData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IHttpActionResult> Delete(ActionRequest<Mfgtailoringwages> request)
        {
            MfgtailoringwagesDataService MfgtailoringwagesData = new MfgtailoringwagesDataService();
            bool result = await Task<bool>.Run(() => MfgtailoringwagesData.Delete(request.Item));
            return this.Ok(result);
        }
    }
}