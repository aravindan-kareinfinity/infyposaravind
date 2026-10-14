
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
    [RoutePrefix("BOMinvoicetaxService")]
    public class BOMinvoicetaxServiceController : ApiController
    {

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<BOMinvoicetax>();
            result.Item = new BOMinvoicetax();
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<BOMinvoicetaxCreteria> request)
        {
            BOMinvoicetaxDataService BOMinvoicetaxData = new BOMinvoicetaxDataService();
            List<BOMinvoicetax> result = await Task<List<BOMinvoicetax>>.Run(() => BOMinvoicetaxData.Search(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Save")]
        public async Task<IHttpActionResult> Save(ActionRequest<BOMinvoicetax> request)
        {
            BOMinvoicetaxDataService BOMinvoicetaxData = new BOMinvoicetaxDataService();
            if (request.Item.id <= 0)
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Create);
                BOMinvoicetax result = await Task<BOMinvoicetax>.Run(() => BOMinvoicetaxData.Insert(request.Item));
                return this.Ok(result);
            }
            else
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Update);
                BOMinvoicetax result = await Task<BOMinvoicetax>.Run(() => BOMinvoicetaxData.Update(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("Insert")]
        public async Task<IHttpActionResult> Insert(ActionRequest<BOMinvoicetax> request)
        {
            BOMinvoicetaxDataService BOMinvoicetaxData = new BOMinvoicetaxDataService();
            BOMinvoicetax result = await Task<BOMinvoicetax>.Run(() => BOMinvoicetaxData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        public async Task<IHttpActionResult> Update(ActionRequest<BOMinvoicetax> request)
        {
            BOMinvoicetaxDataService BOMinvoicetaxData = new BOMinvoicetaxDataService();
            BOMinvoicetax result = await Task<BOMinvoicetax>.Run(() => BOMinvoicetaxData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IHttpActionResult> Delete(ActionRequest<BOMinvoicetax> request)
        {
            BOMinvoicetaxDataService BOMinvoicetaxData = new BOMinvoicetaxDataService();
            bool result = await Task<bool>.Run(() => BOMinvoicetaxData.Delete(request.Item));
            return this.Ok(result);
        }
    }
}