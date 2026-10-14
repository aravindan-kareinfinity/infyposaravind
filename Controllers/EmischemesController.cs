
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
    [RoutePrefix("EmischemesService")]
    public class EmischemesServiceController : ApiController
    {

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<Emischemes>();
            result.Item = new Emischemes();
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<EmischemesCreteria> request)
        {
            EmischemesDataService EmischemesData = new EmischemesDataService();
            List<Emischemes> result = await Task<List<Emischemes>>.Run(() => EmischemesData.Search(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Save")]
        public async Task<IHttpActionResult> Save(ActionRequest<Emischemes> request)
        {
            EmischemesDataService EmischemesData = new EmischemesDataService();
            if (request.Item.id <= 0)
            {
                Emischemes result = await Task<Emischemes>.Run(() => EmischemesData.Insert(request.Item));
                return this.Ok(result);
            }
            else
            {
                Emischemes result = await Task<Emischemes>.Run(() => EmischemesData.Update(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("Insert")]
        public async Task<IHttpActionResult> Insert(ActionRequest<Emischemes> request)
        {
            EmischemesDataService EmischemesData = new EmischemesDataService();
            Emischemes result = await Task<Emischemes>.Run(() => EmischemesData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        public async Task<IHttpActionResult> Update(ActionRequest<Emischemes> request)
        {
            EmischemesDataService EmischemesData = new EmischemesDataService();
            Emischemes result = await Task<Emischemes>.Run(() => EmischemesData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IHttpActionResult> Delete(ActionRequest<Emischemes> request)
        {
            EmischemesDataService EmischemesData = new EmischemesDataService();
            bool result = await Task<bool>.Run(() => EmischemesData.Delete(request.Item));
            return this.Ok(result);
        }
    }
}