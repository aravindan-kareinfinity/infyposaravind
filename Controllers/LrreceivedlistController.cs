
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
    [RoutePrefix("LrreceivedlistService")]
    public class LrreceivedlistServiceController : ApiController
    {

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<receiptitem>();
            result.Item = new receiptitem();
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<LrreceivedlistCreteria> request)
        {
            LrreceivedlistDataService LrreceivedlistData = new LrreceivedlistDataService();
            List<receiptitem> result = await Task<List<receiptitem>>.Run(() => LrreceivedlistData.Search(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Save")]
        public async Task<IHttpActionResult> Save(ActionRequest<receiptitem> request)
        {
            LrreceivedlistDataService LrreceivedlistData = new LrreceivedlistDataService();
            if (request.Item.id <= 0)
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Create);
                receiptitem result = await Task<receiptitem>.Run(() => LrreceivedlistData.Insert(request.Item));
                return this.Ok(result);
            }
            else
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Update);
                receiptitem result = await Task<receiptitem>.Run(() => LrreceivedlistData.Update(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("Insert")]
        public async Task<IHttpActionResult> Insert(ActionRequest<receiptitem> request)
        {
            LrreceivedlistDataService LrreceivedlistData = new LrreceivedlistDataService();
            receiptitem result = await Task<receiptitem>.Run(() => LrreceivedlistData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        public async Task<IHttpActionResult> Update(ActionRequest<receiptitem> request)
        {
            LrreceivedlistDataService LrreceivedlistData = new LrreceivedlistDataService();
            receiptitem result = await Task<receiptitem>.Run(() => LrreceivedlistData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IHttpActionResult> Delete(ActionRequest<receiptitem> request)
        {
            LrreceivedlistDataService LrreceivedlistData = new LrreceivedlistDataService();
            bool result = await Task<bool>.Run(() => LrreceivedlistData.Delete(request.Item));
            return this.Ok(result);
        }
    }
}