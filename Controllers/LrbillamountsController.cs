
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
    [RoutePrefix("LrbillamountsService")]
    public class LrbillamountsServiceController : ApiController
    {

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<Lrbillamounts>();
            result.Item = new Lrbillamounts();
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<LrbillamountsCreteria> request)
        {
            LrbillamountsDataService LrbillamountsData = new LrbillamountsDataService();
            List<Lrbillamounts> result = await Task<List<Lrbillamounts>>.Run(() => LrbillamountsData.Search(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Save")]
        public async Task<IHttpActionResult> Save(ActionRequest<Lrbillamounts> request)
        {
            LrbillamountsDataService LrbillamountsData = new LrbillamountsDataService();
            if (request.Item.id <= 0)
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Create);
                Lrbillamounts result = await Task<Lrbillamounts>.Run(() => LrbillamountsData.Insert(request.Item));
                return this.Ok(result);
            }
            else
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Update);
                Lrbillamounts result = await Task<Lrbillamounts>.Run(() => LrbillamountsData.Update(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("Insert")]
        public async Task<IHttpActionResult> Insert(ActionRequest<Lrbillamounts> request)
        {
            LrbillamountsDataService LrbillamountsData = new LrbillamountsDataService();
            Lrbillamounts result = await Task<Lrbillamounts>.Run(() => LrbillamountsData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        public async Task<IHttpActionResult> Update(ActionRequest<Lrbillamounts> request)
        {
            LrbillamountsDataService LrbillamountsData = new LrbillamountsDataService();
            Lrbillamounts result = await Task<Lrbillamounts>.Run(() => LrbillamountsData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IHttpActionResult> Delete(ActionRequest<Lrbillamounts> request)
        {
            LrbillamountsDataService LrbillamountsData = new LrbillamountsDataService();
            bool result = await Task<bool>.Run(() => LrbillamountsData.Delete(request.Item));
            return this.Ok(result);
        }
    }
}