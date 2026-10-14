
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
    [RoutePrefix("JobworksamplesService")]
    public class JobworksamplesServiceController : ApiController
    {

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<Jobworksamples>();
            result.Item = new Jobworksamples();
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<JobworksamplesCreteria> request)
        {
            JobworksamplesDataService JobworksamplesData = new JobworksamplesDataService();
            List<Jobworksamples> result = await Task<List<Jobworksamples>>.Run(() => JobworksamplesData.Search(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Save")]
        public async Task<IHttpActionResult> Save(ActionRequest<Jobworksamples> request)
        {
            JobworksamplesDataService JobworksamplesData = new JobworksamplesDataService();
            if (request.Item.id <= 0)
            {
                Jobworksamples result = await Task<Jobworksamples>.Run(() => JobworksamplesData.Insert(request.Item));
                return this.Ok(result);
            }
            else
            {
                Jobworksamples result = await Task<Jobworksamples>.Run(() => JobworksamplesData.Update(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("Insert")]
        public async Task<IHttpActionResult> Insert(ActionRequest<Jobworksamples> request)
        {
            JobworksamplesDataService JobworksamplesData = new JobworksamplesDataService();
            Jobworksamples result = await Task<Jobworksamples>.Run(() => JobworksamplesData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        public async Task<IHttpActionResult> Update(ActionRequest<Jobworksamples> request)
        {
            JobworksamplesDataService JobworksamplesData = new JobworksamplesDataService();
            Jobworksamples result = await Task<Jobworksamples>.Run(() => JobworksamplesData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IHttpActionResult> Delete(ActionRequest<Jobworksamples> request)
        {
            JobworksamplesDataService JobworksamplesData = new JobworksamplesDataService();
            bool result = await Task<bool>.Run(() => JobworksamplesData.Delete(request.Item));
            return this.Ok(result);
        }
    }
}