
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
    [RoutePrefix("BillitemsService")]
    public class BillitemsServiceController : ApiController
    {

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<Billitems>();
            result.Item = new Billitems();
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<BillitemsCreteria> request)
        {
            BillitemsDataService BillitemsData = new BillitemsDataService();
            List<Billitems> result = await Task<List<Billitems>>.Run(() => BillitemsData.Search(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Save")]
        public async Task<IHttpActionResult> Save(ActionRequest<Billitems> request)
        {
            BillitemsDataService BillitemsData = new BillitemsDataService();
            if (request.Item.id <= 0)
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Create);
                Billitems result = await Task<Billitems>.Run(() => BillitemsData.Insert(request.Item,null,0));
                return this.Ok(result);
            }
            else
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Update);
                Billitems result = await Task<Billitems>.Run(() => BillitemsData.Update(new Bill(), request.Item, request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("Insert")]
        public async Task<IHttpActionResult> Insert(ActionRequest<Billitems> request)
        {
            BillitemsDataService BillitemsData = new BillitemsDataService();
            Billitems result = await Task<Billitems>.Run(() => BillitemsData.Insert(request.Item,null,0));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        public async Task<IHttpActionResult> Update(ActionRequest<Billitems> request)
        {
            BillitemsDataService BillitemsData = new BillitemsDataService();
            Billitems result = await Task<Billitems>.Run(() => BillitemsData.Update(new Bill(), request.Item, request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IHttpActionResult> Delete(ActionRequest<Billitems> request)
        {
            BillitemsDataService BillitemsData = new BillitemsDataService();
            bool result = await Task<bool>.Run(() => BillitemsData.Delete(request.Item));
            return this.Ok(result);
        }
    }
}