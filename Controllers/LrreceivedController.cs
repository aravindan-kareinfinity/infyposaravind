
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
    [RoutePrefix("LrreceivedService")]
    public class LrreceivedServiceController : ApiController
    {

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<Lrreceived>();
            result.Item = new Lrreceived();
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<LrreceivedCreteria> request)
        {
            LrreceivedDataService LrreceivedData = new LrreceivedDataService();
            List<Lrreceived> result = await Task<List<Lrreceived>>.Run(() => LrreceivedData.Search(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Save")]
        public async Task<IHttpActionResult> Save(ActionRequest<Lrreceived> request)
        {
            LrreceivedDataService LrreceivedData = new LrreceivedDataService();
            if (request.Item.id <= 0)
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Create);
                if (request.Item.receiptitem != null)
                {
                    foreach (var item in request.Item.receiptitem)
                    {
                        item.ChangeStatus(BaseEntity.DataStatus.Create);
                    }
                }
                Lrreceived result = await Task<Lrreceived>.Run(() => LrreceivedData.Insert(request.Item));
                return this.Ok(result);
            }
            else
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Update);
                if (request.Item.receiptitem != null)
                {
                    foreach (var item in request.Item.receiptitem)
                    {
                        item.ChangeStatus(BaseEntity.DataStatus.Update);
                    }
                }
                Lrreceived result = await Task<Lrreceived>.Run(() => LrreceivedData.Update(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("Insert")]
        public async Task<IHttpActionResult> Insert(ActionRequest<Lrreceived> request)
        {
            LrreceivedDataService LrreceivedData = new LrreceivedDataService();
            Lrreceived result = await Task<Lrreceived>.Run(() => LrreceivedData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        public async Task<IHttpActionResult> Update(ActionRequest<Lrreceived> request)
        {
            LrreceivedDataService LrreceivedData = new LrreceivedDataService();
            Lrreceived result = await Task<Lrreceived>.Run(() => LrreceivedData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IHttpActionResult> Delete(ActionRequest<Lrreceived> request)
        {
            LrreceivedDataService LrreceivedData = new LrreceivedDataService();
            bool result = await Task<bool>.Run(() => LrreceivedData.Delete(request.Item));
            return this.Ok(result);
        }
    }
}