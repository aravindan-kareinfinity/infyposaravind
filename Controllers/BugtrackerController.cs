
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
    [RoutePrefix("BugtrackerService")]
    public class BugtrackerServiceController : ApiController
    {
        [AllowAnonymous]
        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<Bugtracker>();
            result.Item = new Bugtracker();
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<BugtrackerCreteria> request)
        {
            BugtrackerDataService BugtrackerData = new BugtrackerDataService();
            List<Bugtracker> result = await Task<List<Bugtracker>>.Run(() => BugtrackerData.Search(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Save")]
        public async Task<IHttpActionResult> Save(ActionRequest<Bugtracker> request)
        {
            BugtrackerDataService BugtrackerData = new BugtrackerDataService();
            if (request.Item.id <= 0)
            {
                Bugtracker result = await Task<Bugtracker>.Run(() => BugtrackerData.Insert(request.Item));
                return this.Ok(result);
            }
            else
            {
                Bugtracker result = await Task<Bugtracker>.Run(() => BugtrackerData.Update(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("Insert")]
        public async Task<IHttpActionResult> Insert(ActionRequest<Bugtracker> request)
        {
            BugtrackerDataService BugtrackerData = new BugtrackerDataService();
            Bugtracker result = await Task<Bugtracker>.Run(() => BugtrackerData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        public async Task<IHttpActionResult> Update(ActionRequest<Bugtracker> request)
        {
            BugtrackerDataService BugtrackerData = new BugtrackerDataService();
            Bugtracker result = await Task<Bugtracker>.Run(() => BugtrackerData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IHttpActionResult> Delete(ActionRequest<Bugtracker> request)
        {
            BugtrackerDataService BugtrackerData = new BugtrackerDataService();
            bool result = await Task<bool>.Run(() => BugtrackerData.Delete(request.Item));
            return this.Ok(result);
        }
    }
}