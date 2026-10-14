
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
    [RoutePrefix("JobworkbillitemsService")]
    public class JobworkbillitemsServiceController : ApiController
    {

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<Jobworkbillitems>();
            result.Item = new Jobworkbillitems();
            return this.Ok(result);
        }

        [HttpGet]
        [Route("SearchSummary/{status?}")]
        public async Task<IHttpActionResult> SearchPending(string status = null)
        {
            int nvalue = 0;
            int.TryParse(status, out nvalue);
            JobworkbillitemsDataService JobworkbillitemsData = new JobworkbillitemsDataService();
            List<JobworkbillitemsSummary> result = await Task<List<JobworkbillitemsSummary>>.Run(() => JobworkbillitemsData.SearchSummary(new JobworkbillCreteria() { status = nvalue } ));
            return this.Ok(result);
        }
        [HttpPost]
        [Route("SearchPendingSummary")]
        public async Task<IHttpActionResult> SearchPendingSummary(ActionRequest<JobworkbillCreteria> request)
        {
            JobworkbillitemsDataService JobworkbillitemsData = new JobworkbillitemsDataService();
            List<JobworkbillitemsSummary> result = await Task<List<JobworkbillitemsSummary>>.Run(() => JobworkbillitemsData.SearchSummary(request.Item));
            return this.Ok(result);
        }
        [HttpPost]
        [Route("SearchJobWorkItems")]
        public async Task<IHttpActionResult> SearchJobWorkItems(ActionRequest<JobworkbillCreteria> request)
        {
            JobworkbillitemsDataService JobworkbillitemsData = new JobworkbillitemsDataService();
            List<Jobworkitems> result = await Task<List<Jobworkitems>>.Run(() => JobworkbillitemsData.SearchJobWorkItems(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("SearchPending")]
        public async Task<IHttpActionResult> SearchPending(ActionRequest<JobworkbillitemsCreteria> request)
        {
            JobworkbillitemsDataService JobworkbillitemsData = new JobworkbillitemsDataService();
            List<JobworkbillitemsPending> result = await Task<List<JobworkbillitemsPending>>.Run(() => JobworkbillitemsData.SearchPending(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Save")]
        public async Task<IHttpActionResult> Save(ActionRequest<Jobworkbillitems> request)
        {
            JobworkbillitemsDataService JobworkbillitemsData = new JobworkbillitemsDataService();
            if (request.Item.id <= 0)
            {
                Jobworkbillitems result = await Task<Jobworkbillitems>.Run(() => JobworkbillitemsData.Insert(request.Item));
                return this.Ok(result);
            }
            else
            {
                Jobworkbillitems result = await Task<Jobworkbillitems>.Run(() => JobworkbillitemsData.Update(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("Insert")]
        public async Task<IHttpActionResult> Insert(ActionRequest<Jobworkbillitems> request)
        {
            JobworkbillitemsDataService JobworkbillitemsData = new JobworkbillitemsDataService();
            Jobworkbillitems result = await Task<Jobworkbillitems>.Run(() => JobworkbillitemsData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        public async Task<IHttpActionResult> Update(ActionRequest<Jobworkbillitems> request)
        {
            JobworkbillitemsDataService JobworkbillitemsData = new JobworkbillitemsDataService();
            Jobworkbillitems result = await Task<Jobworkbillitems>.Run(() => JobworkbillitemsData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IHttpActionResult> Delete(ActionRequest<Jobworkbillitems> request)
        {
            JobworkbillitemsDataService JobworkbillitemsData = new JobworkbillitemsDataService();
            bool result = await Task<bool>.Run(() => JobworkbillitemsData.Delete(request.Item));
            return this.Ok(result);
        }
    }
}