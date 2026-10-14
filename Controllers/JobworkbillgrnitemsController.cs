
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
    [RoutePrefix("JobworkbillgrnitemsService")]
    public class JobworkbillgrnitemsServiceController : ApiController
    {

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<Jobworkbillgrnitems>();
            result.Item = new Jobworkbillgrnitems();
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<JobworkbillgrnitemsCreteria> request)
        {
            JobworkbillgrnitemsDataService JobworkbillgrnitemsData = new JobworkbillgrnitemsDataService();
            List<Jobworkbillgrnitems> result = await Task<List<Jobworkbillgrnitems>>.Run(() => JobworkbillgrnitemsData.Search(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Save")]
        public async Task<IHttpActionResult> Save(ActionRequest<Jobworkbillgrnitems> request)
        {
            JobworkbillgrnitemsDataService JobworkbillgrnitemsData = new JobworkbillgrnitemsDataService();
            if (request.Item.id <= 0)
            {
                Jobworkbillgrnitems result = await Task<Jobworkbillgrnitems>.Run(() => JobworkbillgrnitemsData.Insert(request.Item));
                return this.Ok(result);
            }
            else
            {
                Jobworkbillgrnitems result = await Task<Jobworkbillgrnitems>.Run(() => JobworkbillgrnitemsData.Update(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("Insert")]
        public async Task<IHttpActionResult> Insert(ActionRequest<Jobworkbillgrnitems> request)
        {
            JobworkbillgrnitemsDataService JobworkbillgrnitemsData = new JobworkbillgrnitemsDataService();
            Jobworkbillgrnitems result = await Task<Jobworkbillgrnitems>.Run(() => JobworkbillgrnitemsData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        public async Task<IHttpActionResult> Update(ActionRequest<Jobworkbillgrnitems> request)
        {
            JobworkbillgrnitemsDataService JobworkbillgrnitemsData = new JobworkbillgrnitemsDataService();
            Jobworkbillgrnitems result = await Task<Jobworkbillgrnitems>.Run(() => JobworkbillgrnitemsData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("SearchJobWorkItems")]
        public async Task<IHttpActionResult> SearchJobWorkItems(ActionRequest<JobworkbillCreteria> request)
        {
            JobworkbillgrnitemsDataService JobworkbillitemsData = new JobworkbillgrnitemsDataService();
            List<Jobworkitems> result = await Task<List<Jobworkitems>>.Run(() => JobworkbillitemsData.SearchJobWorkItems(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IHttpActionResult> Delete(ActionRequest<Jobworkbillgrnitems> request)
        {
            JobworkbillgrnitemsDataService JobworkbillgrnitemsData = new JobworkbillgrnitemsDataService();
            bool result = await Task<bool>.Run(() => JobworkbillgrnitemsData.Delete(request.Item));
            return this.Ok(result);
        }
    }
}