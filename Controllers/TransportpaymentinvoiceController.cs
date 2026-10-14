
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
    [RoutePrefix("TransportpaymentinvoiceService")]
    public class TransportpaymentinvoiceServiceController : ApiController
    {

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<Transportpaymentinvoice>();
            result.Item = new Transportpaymentinvoice();
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<TransportpaymentinvoiceCreteria> request)
        {
            TransportpaymentinvoiceDataService TransportpaymentinvoiceData = new TransportpaymentinvoiceDataService();
            List<Transportpaymentinvoice> result = await Task<List<Transportpaymentinvoice>>.Run(() => TransportpaymentinvoiceData.Search(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Save")]
        public async Task<IHttpActionResult> Save(ActionRequest<Transportpaymentinvoice> request)
        {
            TransportpaymentinvoiceDataService TransportpaymentinvoiceData = new TransportpaymentinvoiceDataService();
            if (request.Item.id <= 0)
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Create);
                Transportpaymentinvoice result = await Task<Transportpaymentinvoice>.Run(() => TransportpaymentinvoiceData.Insert(request.Item));
                return this.Ok(result);
            }
            else
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Update);
                Transportpaymentinvoice result = await Task<Transportpaymentinvoice>.Run(() => TransportpaymentinvoiceData.Update(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("Insert")]
        public async Task<IHttpActionResult> Insert(ActionRequest<Transportpaymentinvoice> request)
        {
            TransportpaymentinvoiceDataService TransportpaymentinvoiceData = new TransportpaymentinvoiceDataService();
            Transportpaymentinvoice result = await Task<Transportpaymentinvoice>.Run(() => TransportpaymentinvoiceData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        public async Task<IHttpActionResult> Update(ActionRequest<Transportpaymentinvoice> request)
        {
            TransportpaymentinvoiceDataService TransportpaymentinvoiceData = new TransportpaymentinvoiceDataService();
            Transportpaymentinvoice result = await Task<Transportpaymentinvoice>.Run(() => TransportpaymentinvoiceData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IHttpActionResult> Delete(ActionRequest<Transportpaymentinvoice> request)
        {
            TransportpaymentinvoiceDataService TransportpaymentinvoiceData = new TransportpaymentinvoiceDataService();
            bool result = await Task<bool>.Run(() => TransportpaymentinvoiceData.Delete(request.Item));
            return this.Ok(result);
        }
    }
}