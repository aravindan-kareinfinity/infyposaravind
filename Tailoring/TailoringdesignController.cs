
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
    [RoutePrefix("TailoringdesignService")]
    public class TailoringdesignServiceController : ApiController
    {

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<Tailoringdesign>();
            result.Item = new Tailoringdesign();
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<TailoringdesignCreteria> request)
        {
            TailoringdesignDataService TailoringdesignData = new TailoringdesignDataService();
            List<Tailoringdesign> result = await Task<List<Tailoringdesign>>.Run(() => TailoringdesignData.Search(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Save")]
        public async Task<IHttpActionResult> Save(ActionRequest<Tailoringdesign> request)
        {
            TailoringdesignDataService TailoringdesignData = new TailoringdesignDataService();
            if (request.Item.id <= 0)
            {
                Tailoringdesign result = await Task<Tailoringdesign>.Run(() => TailoringdesignData.Insert(request.Item));
                return this.Ok(result);
            }
            else
            {
                Tailoringdesign result = await Task<Tailoringdesign>.Run(() => TailoringdesignData.Update(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("Insert")]
        public async Task<IHttpActionResult> Insert(ActionRequest<Tailoringdesign> request)
        {
            TailoringdesignDataService TailoringdesignData = new TailoringdesignDataService();
            Tailoringdesign result = await Task<Tailoringdesign>.Run(() => TailoringdesignData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        public async Task<IHttpActionResult> Update(ActionRequest<Tailoringdesign> request)
        {
            TailoringdesignDataService TailoringdesignData = new TailoringdesignDataService();
            Tailoringdesign result = await Task<Tailoringdesign>.Run(() => TailoringdesignData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IHttpActionResult> Delete(ActionRequest<Tailoringdesign> request)
        {
            TailoringdesignDataService TailoringdesignData = new TailoringdesignDataService();
            bool result = await Task<bool>.Run(() => TailoringdesignData.Delete(request.Item));
            return this.Ok(result);
        }
    }
}