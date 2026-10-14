
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
    [RoutePrefix("TransportService")]
    public class TransportServiceController : ApiController
    {

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<Transport>();
            result.Item = new Transport();
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<TransportCreteria> request)
        {
            TransportDataService TransportData = new TransportDataService();
            List<Transport> result = await Task<List<Transport>>.Run(() => TransportData.Search(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Save")]
        [WebAPI.InfyAuthorize(PermissionSet.transportmaster, InfyPOS.Common.Action.Create)]
        public async Task<IHttpActionResult> Save(ActionRequest<Transport> request)
        {
            TransportDataService TransportData = new TransportDataService();
            if (request.Item.id <= 0)
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Create);
                Transport result = await Task<Transport>.Run(() => TransportData.Insert(request.Item));
                return this.Ok(result);
            }
            else
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Update);
                Transport result = await Task<Transport>.Run(() => TransportData.Update(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("Insert")]
        [WebAPI.InfyAuthorize(PermissionSet.transportmaster, InfyPOS.Common.Action.Create)]
        public async Task<IHttpActionResult> Insert(ActionRequest<Transport> request)
        {
            TransportDataService TransportData = new TransportDataService();
            Transport result = await Task<Transport>.Run(() => TransportData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        [WebAPI.InfyAuthorize(PermissionSet.transportmaster, InfyPOS.Common.Action.Modify)]
        public async Task<IHttpActionResult> Update(ActionRequest<Transport> request)
        {
            TransportDataService TransportData = new TransportDataService();
            Transport result = await Task<Transport>.Run(() => TransportData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        [WebAPI.InfyAuthorize(PermissionSet.transportmaster, InfyPOS.Common.Action.Delete)]
        public async Task<IHttpActionResult> Delete(ActionRequest<Transport> request)
        {
            TransportDataService TransportData = new TransportDataService();
            bool result = await Task<bool>.Run(() => TransportData.Delete(request.Item));
            return this.Ok(result);
        }
    }
}