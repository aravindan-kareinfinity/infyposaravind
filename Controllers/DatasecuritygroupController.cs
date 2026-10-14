
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
    [RoutePrefix("DatasecuritygroupService")]
    public class DatasecuritygroupServiceController : ApiController
    {

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<Datasecuritygroup>();
            result.Item = new Datasecuritygroup();
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<DatasecuritygroupCreteria> request)
        {
            DatasecuritygroupDataService DatasecuritygroupData = new DatasecuritygroupDataService();
            List<Datasecuritygroup> result = await Task<List<Datasecuritygroup>>.Run(() => DatasecuritygroupData.Search(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Save")]
        public async Task<IHttpActionResult> Save(ActionRequest<Datasecuritygroup> request)
        {
            DatasecuritygroupDataService DatasecuritygroupData = new DatasecuritygroupDataService();
            if (request.Item.id <= 0)
            {
                Datasecuritygroup result = await Task<Datasecuritygroup>.Run(() => DatasecuritygroupData.Insert(request.Item));
                return this.Ok(result);
            }
            else
            {
                Datasecuritygroup result = await Task<Datasecuritygroup>.Run(() => DatasecuritygroupData.Update(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("Insert")]
        public async Task<IHttpActionResult> Insert(ActionRequest<Datasecuritygroup> request)
        {
            DatasecuritygroupDataService DatasecuritygroupData = new DatasecuritygroupDataService();
            Datasecuritygroup result = await Task<Datasecuritygroup>.Run(() => DatasecuritygroupData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        public async Task<IHttpActionResult> Update(ActionRequest<Datasecuritygroup> request)
        {
            DatasecuritygroupDataService DatasecuritygroupData = new DatasecuritygroupDataService();
            Datasecuritygroup result = await Task<Datasecuritygroup>.Run(() => DatasecuritygroupData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IHttpActionResult> Delete(ActionRequest<Datasecuritygroup> request)
        {
            DatasecuritygroupDataService DatasecuritygroupData = new DatasecuritygroupDataService();
            bool result = await Task<bool>.Run(() => DatasecuritygroupData.Delete(request.Item));
            return this.Ok(result);
        }
    }
}