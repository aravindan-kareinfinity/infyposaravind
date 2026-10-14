
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
    [RoutePrefix("LrinvoicetaxService")]
    public class LrinvoicetaxServiceController : ApiController
    {

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<Lrinvoicetax>();
            result.Item = new Lrinvoicetax();
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<LrinvoicetaxCreteria> request)
        {
            LrinvoicetaxDataService LrinvoicetaxData = new LrinvoicetaxDataService();
            List<Lrinvoicetax> result = await Task<List<Lrinvoicetax>>.Run(() => LrinvoicetaxData.Search(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Save")]
        public async Task<IHttpActionResult> Save(ActionRequest<Lrinvoicetax> request)
        {
            LrinvoicetaxDataService LrinvoicetaxData = new LrinvoicetaxDataService();
            if (request.Item.id <= 0)
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Create);
                Lrinvoicetax result = await Task<Lrinvoicetax>.Run(() => LrinvoicetaxData.Insert(request.Item));
                return this.Ok(result);
            }
            else
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Update);
                Lrinvoicetax result = await Task<Lrinvoicetax>.Run(() => LrinvoicetaxData.Update(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("Insert")]
        public async Task<IHttpActionResult> Insert(ActionRequest<Lrinvoicetax> request)
        {
            LrinvoicetaxDataService LrinvoicetaxData = new LrinvoicetaxDataService();
            Lrinvoicetax result = await Task<Lrinvoicetax>.Run(() => LrinvoicetaxData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        public async Task<IHttpActionResult> Update(ActionRequest<Lrinvoicetax> request)
        {
            LrinvoicetaxDataService LrinvoicetaxData = new LrinvoicetaxDataService();
            Lrinvoicetax result = await Task<Lrinvoicetax>.Run(() => LrinvoicetaxData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IHttpActionResult> Delete(ActionRequest<Lrinvoicetax> request)
        {
            LrinvoicetaxDataService LrinvoicetaxData = new LrinvoicetaxDataService();
            bool result = await Task<bool>.Run(() => LrinvoicetaxData.Delete(request.Item));
            return this.Ok(result);
        }
    }
}