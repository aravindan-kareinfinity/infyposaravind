
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
    [RoutePrefix("PurchaseorderitemsService")]
    public class PurchaseorderitemsServiceController : ApiController
    {

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<Purchaseorderitems>();
            result.Item = new Purchaseorderitems();
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<PurchaseorderitemsCreteria> request)
        {
            PurchaseorderitemsDataService PurchaseorderitemsData = new PurchaseorderitemsDataService();
            List<PurchaseorderitemsDetail> result = await Task<List<PurchaseorderitemsDetail>>.Run(() => PurchaseorderitemsData.Search(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Save")]
        public async Task<IHttpActionResult> Save(ActionRequest<PurchaseorderitemsDetail> request)
        {
            PurchaseorderitemsDataService PurchaseorderitemsData = new PurchaseorderitemsDataService();
            if (request.Item.id <= 0)
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Create);
                Purchaseorderitems result = await Task<Purchaseorderitems>.Run(() => PurchaseorderitemsData.Insert(request.Item));
                return this.Ok(result);
            }
            else
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Update);
                Purchaseorderitems result = await Task<Purchaseorderitems>.Run(() => PurchaseorderitemsData.Update(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("Insert")]
        public async Task<IHttpActionResult> Insert(ActionRequest<PurchaseorderitemsDetail> request)
        {
            PurchaseorderitemsDataService PurchaseorderitemsData = new PurchaseorderitemsDataService();
            Purchaseorderitems result = await Task<Purchaseorderitems>.Run(() => PurchaseorderitemsData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        public async Task<IHttpActionResult> Update(ActionRequest<PurchaseorderitemsDetail> request)
        {
            PurchaseorderitemsDataService PurchaseorderitemsData = new PurchaseorderitemsDataService();
            Purchaseorderitems result = await Task<Purchaseorderitems>.Run(() => PurchaseorderitemsData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IHttpActionResult> Delete(ActionRequest<Purchaseorderitems> request)
        {
            PurchaseorderitemsDataService PurchaseorderitemsData = new PurchaseorderitemsDataService();
            bool result = await Task<bool>.Run(() => PurchaseorderitemsData.Delete(request.Item));
            return this.Ok(result);
        }
    }
}