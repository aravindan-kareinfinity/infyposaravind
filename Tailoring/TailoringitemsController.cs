
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
    [RoutePrefix("TailoringitemsService")]
    public class TailoringitemsServiceController : ApiController
    {

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<Tailoringitems>();
            result.Item = new Tailoringitems();
            return this.Ok(result);
        }


        [HttpPost]
        [Route("Save")]
        public async Task<IHttpActionResult> Save(ActionRequest<Tailoringitems> request)
        {
            TailoringitemsDataService TailoringitemsData = new TailoringitemsDataService();
            if (request.Item.id <= 0)
            {
                Tailoringitems result = await Task<Tailoringitems>.Run(() => TailoringitemsData.Insert(request.Item));
                return this.Ok(result);
            }
            else
            {
                Tailoringitems result = await Task<Tailoringitems>.Run(() => TailoringitemsData.Update(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("Insert")]
        public async Task<IHttpActionResult> Insert(ActionRequest<Tailoringitems> request)
        {
            TailoringitemsDataService TailoringitemsData = new TailoringitemsDataService();
            Tailoringitems result = await Task<Tailoringitems>.Run(() => TailoringitemsData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        public async Task<IHttpActionResult> Update(ActionRequest<Tailoringitems> request)
        {
            TailoringitemsDataService TailoringitemsData = new TailoringitemsDataService();
            Tailoringitems result = await Task<Tailoringitems>.Run(() => TailoringitemsData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("UpdateTailor")]
        public async Task<IHttpActionResult> UpdateTailor(ActionMulitpleRequest<Tailoringitems> request)
        {
            TailoringitemsDataService TailoringitemsData = new TailoringitemsDataService();
            bool result = await Task<bool>.Run(() => TailoringitemsData.UpdateTailor(request.Items));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("TailoringPayment")]
        public async Task<IHttpActionResult> ReceivedQty(ActionRequest<TailoringPayment> request)
        {
            TailoringitemsDataService TailoringitemsData = new TailoringitemsDataService();
            bool result = await Task<bool>.Run(() => TailoringitemsData.TailoringPayment(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("ReceivedQty")]
        public async Task<IHttpActionResult> ReceivedQty(ActionRequest<TailoringitemsTransaction> request)
        {
            TailoringitemsDataService TailoringitemsData = new TailoringitemsDataService();
            bool result = await Task<bool>.Run(() => TailoringitemsData.ReceivedQty(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("SplitQty")]
        public async Task<IHttpActionResult> SplitQty(ActionRequest<TailoringitemsTransaction> request)
        {
            TailoringitemsDataService TailoringitemsData = new TailoringitemsDataService();
            bool result = await Task<bool>.Run(() => TailoringitemsData.SplitQty(request.Item));
            return this.Ok(result);
        }
        [HttpPost]
        [Route("DeliverQty")]
        public async Task<IHttpActionResult> DeliverQty(ActionRequest<TailoringitemsTransaction> request)
        {
            TailoringitemsDataService TailoringitemsData = new TailoringitemsDataService();
            bool result = await Task<bool>.Run(() => TailoringitemsData.DeliverQty(request.Item));
            return this.Ok(result);
        }
        [HttpPost]
        [Route("Delete")]
        public async Task<IHttpActionResult> Delete(ActionRequest<Tailoringitems> request)
        {
            TailoringitemsDataService TailoringitemsData = new TailoringitemsDataService();
            bool result = await Task<bool>.Run(() => TailoringitemsData.Delete(request.Item));
            return this.Ok(result);
        }
    }
}