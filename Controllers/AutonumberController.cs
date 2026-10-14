
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
    [RoutePrefix("AutonumberService")]
    public class AutonumberServiceController : ApiController
    {

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<Autonumber>();
            result.Item = new Autonumber();
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<AutonumberCreteria> request)
        {
            AutonumberDataService AutonumberData = new AutonumberDataService();
            List<Autonumber> result = await Task<List<Autonumber>>.Run(() => AutonumberData.Search(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Save")]
        [WebAPI.InfyAuthorize(PermissionSet.numberconfig, InfyPOS.Common.Action.Create)]
        public async Task<IHttpActionResult> Save(ActionRequest<Autonumber> request)
        {
            AutonumberDataService AutonumberData = new AutonumberDataService();
            if (request.Item.id <= 0)
            {
                Autonumber result = await Task<Autonumber>.Run(() => AutonumberData.Insert(request.Item));
                return this.Ok(result);
            }
            else
            {
                Autonumber result = await Task<Autonumber>.Run(() => AutonumberData.Update(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("Insert")]
        [WebAPI.InfyAuthorize(PermissionSet.numberconfig, InfyPOS.Common.Action.Create)]
        public async Task<IHttpActionResult> Insert(ActionRequest<Autonumber> request)
        {
            AutonumberDataService AutonumberData = new AutonumberDataService();
            Autonumber result = await Task<Autonumber>.Run(() => AutonumberData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        [WebAPI.InfyAuthorize(PermissionSet.numberconfig, InfyPOS.Common.Action.Modify)]
        public async Task<IHttpActionResult> Update(ActionRequest<Autonumber> request)
        {
            AutonumberDataService AutonumberData = new AutonumberDataService();
            Autonumber result = await Task<Autonumber>.Run(() => AutonumberData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        [WebAPI.InfyAuthorize(PermissionSet.numberconfig, InfyPOS.Common.Action.Delete)]
        public async Task<IHttpActionResult> Delete(ActionRequest<Autonumber> request)
        {
            AutonumberDataService AutonumberData = new AutonumberDataService();
            bool result = await Task<bool>.Run(() => AutonumberData.Delete(request.Item));
            return this.Ok(result);
        }
    }
}