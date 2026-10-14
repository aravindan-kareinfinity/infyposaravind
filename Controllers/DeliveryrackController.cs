
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
    [RoutePrefix("DeliveryrackService")]
    public class DeliveryrackServiceController : ApiController
    {

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<Deliveryrack>();
            result.Item = new Deliveryrack();
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<DeliveryrackCreteria> request)
        {
            DeliveryrackDataService DeliveryrackData = new DeliveryrackDataService();
            List<Deliveryrack> result = await Task<List<Deliveryrack>>.Run(() => DeliveryrackData.Search(request.Item));
            return this.Ok(result);
        }

        [HttpGet]
        [Route("Find/{id}")]
        public async Task<IHttpActionResult> Find(string id)
        {
            DeliveryrackDataService DeliveryrackData = new DeliveryrackDataService();
            DeliveryrackDetail result = await Task<List<Deliveryrack>>.Run(() => DeliveryrackData.Find(long.Parse(id)));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("SearchBill")]
        public async Task<IHttpActionResult> SearchBill(ActionRequest<DeliveryrackCreteria> request)
        {
            DeliveryrackDataService DeliveryrackData = new DeliveryrackDataService();
            List<Bill> result = await Task<List<Bill>>.Run(() => DeliveryrackData.SearchBill(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Save")]
        public async Task<IHttpActionResult> Save(ActionRequest<DeliveryrackDetail> request)
        {
            DeliveryrackDataService DeliveryrackData = new DeliveryrackDataService();
            if (request.Item.id <= 0)
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Create, DeliveryrackData.Context);
                Deliveryrack result = await Task<Deliveryrack>.Run(() => DeliveryrackData.Insert(request.Item));
                return this.Ok(result);
            }
            else
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Update, DeliveryrackData.Context);
                Deliveryrack result = await Task<Deliveryrack>.Run(() => DeliveryrackData.Update(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("Insert")]
        public async Task<IHttpActionResult> Insert(ActionRequest<DeliveryrackDetail> request)
        {
            DeliveryrackDataService DeliveryrackData = new DeliveryrackDataService();
            Deliveryrack result = await Task<Deliveryrack>.Run(() => DeliveryrackData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        public async Task<IHttpActionResult> Update(ActionRequest<DeliveryrackDetail> request)
        {
            DeliveryrackDataService DeliveryrackData = new DeliveryrackDataService();
            DeliveryrackDetail result = await Task<DeliveryrackDetail>.Run(() => DeliveryrackData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Merge")]
        public async Task<IHttpActionResult> Merge(ActionRequest<DeliveryrackCreteria> request)
        {
            DeliveryrackDataService DeliveryrackData = new DeliveryrackDataService();
            List<Deliveryrack> result = await Task<List<Deliveryrack>>.Run(() => DeliveryrackData.Merge(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("RemoveEstimate")]
        public async Task<IHttpActionResult> RemoveEstimate(ActionRequest<DeliveryrackCreteria> request)
        {
            DeliveryrackDataService DeliveryrackData = new DeliveryrackDataService();
            Deliveryrack result = await Task<Deliveryrack>.Run(() => DeliveryrackData.RemoveEstimate(request.Item));
            return this.Ok(result);
        }


        [HttpPost]
        [Route("History")]
        public async Task<IHttpActionResult> History(ActionRequest<DeliveryRackHistory> request)
        {
            DeliveryrackDataService DeliveryrackData = new DeliveryrackDataService();
            List<DeliveryRackHistory> result = await Task<List<DeliveryRackHistory>>.Run(() => DeliveryrackData.DeliveryRackHistory(request.Item));
            return this.Ok(result);
        }
        [HttpPost]
        [Route("CheckWalkin")]
        public async Task<IHttpActionResult> CheckWalkin(ActionRequest<DeliveryrackCreteria> request)
        {
            DeliveryrackDataService DeliveryrackData = new DeliveryrackDataService();
            DeliveryRackWakin result = await Task<DeliveryRackWakin>.Run(() => DeliveryrackData.CheckWalkin(request.Item));
            return this.Ok(result);
        }

        
        [HttpPost]
        [Route("SetRackFree")]
        public async Task<IHttpActionResult> SetRackFree(ActionRequest<DeliveryrackCreteria> request)
        {
            DeliveryrackDataService DeliveryrackData = new DeliveryrackDataService();
            List<Deliveryrack> result = await Task<List<Deliveryrack>>.Run(() => DeliveryrackData.SetRackFree(request.Item));
            return this.Ok(result);
        }
        [HttpPost]
        [Route("UpdateStatus")]
        public async Task<IHttpActionResult> UpdateStatus(ActionRequest<DeliveryrackCreteria> request)
        {
            DeliveryrackDataService DeliveryrackData = new DeliveryrackDataService();
            List<Deliveryrack> result = await Task<List<Deliveryrack>>.Run(() => DeliveryrackData.UpdateStatus(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IHttpActionResult> Delete(ActionRequest<Deliveryrack> request)
        {
            DeliveryrackDataService DeliveryrackData = new DeliveryrackDataService();
            bool result = await Task<bool>.Run(() => DeliveryrackData.Delete(request.Item));
            return this.Ok(result);
        }
    }
}