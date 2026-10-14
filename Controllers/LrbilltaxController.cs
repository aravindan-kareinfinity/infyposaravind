
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
    [RoutePrefix("LrbilltaxService")]
    public class LrbilltaxServiceController : ApiController
    {

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<Lrbilltax>();
            result.Item = new Lrbilltax();
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<LrbilltaxCreteria> request)
        {
            LrbilltaxDataService LrbilltaxData = new LrbilltaxDataService();
            List<Lrbilltax> result = await Task<List<Lrbilltax>>.Run(() => LrbilltaxData.Search(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Save")]
        public async Task<IHttpActionResult> Save(ActionRequest<Lrbilltax> request)
        {
            LrbilltaxDataService LrbilltaxData = new LrbilltaxDataService();
            if (request.Item.id <= 0)
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Create);
                Lrbilltax result = await Task<Lrbilltax>.Run(() => LrbilltaxData.Insert(request.Item));
                return this.Ok(result);
            }
            else
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Update);
                Lrbilltax result = await Task<Lrbilltax>.Run(() => LrbilltaxData.Update(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("Insert")]
        public async Task<IHttpActionResult> Insert(ActionRequest<Lrbilltax> request)
        {
            LrbilltaxDataService LrbilltaxData = new LrbilltaxDataService();
            Lrbilltax result = await Task<Lrbilltax>.Run(() => LrbilltaxData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        public async Task<IHttpActionResult> Update(ActionRequest<Lrbilltax> request)
        {
            LrbilltaxDataService LrbilltaxData = new LrbilltaxDataService();
            Lrbilltax result = await Task<Lrbilltax>.Run(() => LrbilltaxData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IHttpActionResult> Delete(ActionRequest<Lrbilltax> request)
        {
            LrbilltaxDataService LrbilltaxData = new LrbilltaxDataService();
            bool result = await Task<bool>.Run(() => LrbilltaxData.Delete(request.Item));
            return this.Ok(result);
        }
    }
}