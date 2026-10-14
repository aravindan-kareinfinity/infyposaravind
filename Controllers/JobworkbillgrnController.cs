
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
    [RoutePrefix("JobworkbillgrnService")]
    public class JobworkbillgrnServiceController : ApiController
    {

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<Jobworkbillgrn>();
            result.Item = new Jobworkbillgrn();
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<JobworkbillgrnCreteria> request)
        {
            JobworkbillgrnDataService JobworkbillgrnData = new JobworkbillgrnDataService();
            List<Jobworkbillgrn> result = await Task<List<Jobworkbillgrn>>.Run(() => JobworkbillgrnData.Search(request.Item));
            return this.Ok(result);
        }



        [HttpPost]
        [Route("Save")]
        public async Task<IHttpActionResult> Save(ActionRequest<Jobworkbillgrn> request)
        {
            JobworkbillgrnDataService JobworkbillgrnData = new JobworkbillgrnDataService();
            if (request.Item.id <= 0)
            {
                Jobworkbillgrn result = await Task<Jobworkbillgrn>.Run(() => JobworkbillgrnData.Insert(request.Item));
                return this.Ok(result);
            }
            else
            {
                Jobworkbillgrn result = await Task<Jobworkbillgrn>.Run(() => JobworkbillgrnData.Update(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("Insert")]
        public async Task<IHttpActionResult> Insert(ActionRequest<Jobworkbillgrn> request)
        {
            JobworkbillgrnDataService JobworkbillgrnData = new JobworkbillgrnDataService();
            Jobworkbillgrn result = await Task<Jobworkbillgrn>.Run(() => JobworkbillgrnData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        public async Task<IHttpActionResult> Update(ActionRequest<Jobworkbillgrn> request)
        {
            JobworkbillgrnDataService JobworkbillgrnData = new JobworkbillgrnDataService();
            Jobworkbillgrn result = await Task<Jobworkbillgrn>.Run(() => JobworkbillgrnData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IHttpActionResult> Delete(ActionRequest<Jobworkbillgrn> request)
        {
            JobworkbillgrnDataService JobworkbillgrnData = new JobworkbillgrnDataService();
            bool result = await Task<bool>.Run(() => JobworkbillgrnData.Delete(request.Item));
            return this.Ok(result);
        }
    }
}