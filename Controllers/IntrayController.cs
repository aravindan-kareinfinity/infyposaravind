
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
    [RoutePrefix("IntrayService")]
    public class IntrayServiceController : ApiController
    {

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<Intray>();
            result.Item = new Intray();
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<IntrayCreteria> request)
        {
            IntrayDataService IntrayData = new IntrayDataService();
            List<Intray> result = await Task<List<Intray>>.Run(() => IntrayData.Search(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Save")]
        public async Task<IHttpActionResult> Save(ActionRequest<Intray> request)
        {
            IntrayDataService IntrayData = new IntrayDataService();
            if (request.Item.id <= 0)
            {
                Intray result = await Task<Intray>.Run(() => IntrayData.Insert(request.Item));
                return this.Ok(result);
            }
            else
            {
                Intray result = await Task<Intray>.Run(() => IntrayData.Update(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("Insert")]
        public async Task<IHttpActionResult> Insert(ActionRequest<Intray> request)
        {
            IntrayDataService IntrayData = new IntrayDataService();
            Intray result = await Task<Intray>.Run(() => IntrayData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        public async Task<IHttpActionResult> Update(ActionRequest<Intray> request)
        {
            IntrayDataService IntrayData = new IntrayDataService();
            Intray result = await Task<Intray>.Run(() => IntrayData.Update(request.Item));
            return this.Ok(result);
        }
        [HttpPost]
        [Route("UpdateStatus")]
        public async Task<IHttpActionResult> UpdateStatus(ActionRequest<Intray> request)
        {
            IntrayDataService IntrayData = new IntrayDataService();
            Intray result = await Task<Intray>.Run(() => IntrayData.UpdateStatus(request.Item));
            return this.Ok(result);
        }
        [HttpPost]
        [Route("Delete")]
        public async Task<IHttpActionResult> Delete(ActionRequest<Intray> request)
        {
            IntrayDataService IntrayData = new IntrayDataService();
            bool result = await Task<bool>.Run(() => IntrayData.Delete(request.Item));
            return this.Ok(result);
        }
    }
}