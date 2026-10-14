
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
    [RoutePrefix("ComponentPOitemsService")]
    public class ComponentPOitemsServiceController : ApiController
    {

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<ComponentPOitems>();
            result.Item = new ComponentPOitems();
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<ComponentPOitemsCreteria> request)
        {
            ComponentPOitemsDataService ComponentPOitemsData = new ComponentPOitemsDataService();
            List<ComponentPOitems> result = await Task<List<ComponentPOitems>>.Run(() => ComponentPOitemsData.Search(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Save")]
        public async Task<IHttpActionResult> Save(ActionRequest<ComponentPOitemsDetail> request)
        {
            ComponentPOitemsDataService ComponentPOitemsData = new ComponentPOitemsDataService();
            if (request.Item.id <= 0)
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Create);
                ComponentPOitems result = await Task<ComponentPOitems>.Run(() => ComponentPOitemsData.Insert(request.Item));
                return this.Ok(result);
            }
            else
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Update);
                ComponentPOitems result = await Task<ComponentPOitems>.Run(() => ComponentPOitemsData.Update(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("Insert")]
        public async Task<IHttpActionResult> Insert(ActionRequest<ComponentPOitemsDetail> request)
        {
            ComponentPOitemsDataService ComponentPOitemsData = new ComponentPOitemsDataService();
            ComponentPOitems result = await Task<ComponentPOitems>.Run(() => ComponentPOitemsData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        public async Task<IHttpActionResult> Update(ActionRequest<ComponentPOitemsDetail> request)
        {
            ComponentPOitemsDataService ComponentPOitemsData = new ComponentPOitemsDataService();
            ComponentPOitems result = await Task<ComponentPOitems>.Run(() => ComponentPOitemsData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IHttpActionResult> Delete(ActionRequest<ComponentPOitems> request)
        {
            ComponentPOitemsDataService ComponentPOitemsData = new ComponentPOitemsDataService();
            bool result = await Task<bool>.Run(() => ComponentPOitemsData.Delete(request.Item));
            return this.Ok(result);
        }
    }
}