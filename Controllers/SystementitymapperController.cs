
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
    [RoutePrefix("SystementitymapperService")]
    public class SystementitymapperServiceController : ApiController
    {

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<Systementitymapper>();
            result.Item = new Systementitymapper();
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<SystementitymapperCreteria> request)
        {
            SystementitymapperDataService SystementitymapperData = new SystementitymapperDataService();
            List<Systementitymapper> result = await Task<List<Systementitymapper>>.Run(() => SystementitymapperData.Search(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Save")]
        public async Task<IHttpActionResult> Save(ActionRequest<Systementitymapper> request)
        {
            SystementitymapperDataService SystementitymapperData = new SystementitymapperDataService();
            if (request.Item.id <= 0)
            {
                Systementitymapper result = await Task<Systementitymapper>.Run(() => SystementitymapperData.Insert(request.Item));
                return this.Ok(result);
            }
            else
            {
                Systementitymapper result = await Task<Systementitymapper>.Run(() => SystementitymapperData.Update(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("UpdateMapping")]
        public async Task<IHttpActionResult> UpdateMapping(ActionRequest<SystemMapperDetail> request)
        {
            SystementitymapperDataService SystementitymapperData = new SystementitymapperDataService();


            bool result = await Task<bool>.Run(() => SystementitymapperData.UpdateMapping(request.Item));
            return this.Ok(result);

        }

        [HttpPost]
        [Route("Insert")]
        public async Task<IHttpActionResult> Insert(ActionRequest<Systementitymapper> request)
        {
            SystementitymapperDataService SystementitymapperData = new SystementitymapperDataService();
            Systementitymapper result = await Task<Systementitymapper>.Run(() => SystementitymapperData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        public async Task<IHttpActionResult> Update(ActionRequest<Systementitymapper> request)
        {
            SystementitymapperDataService SystementitymapperData = new SystementitymapperDataService();
            Systementitymapper result = await Task<Systementitymapper>.Run(() => SystementitymapperData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IHttpActionResult> Delete(ActionRequest<Systementitymapper> request)
        {
            SystementitymapperDataService SystementitymapperData = new SystementitymapperDataService();
            bool result = await Task<bool>.Run(() => SystementitymapperData.Delete(request.Item));
            return this.Ok(result);
        }
    }
}