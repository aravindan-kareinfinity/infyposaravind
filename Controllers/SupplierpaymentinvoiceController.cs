
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
    [RoutePrefix("SupplierpaymentinvoiceService")]
    public class SupplierpaymentinvoiceServiceController : ApiController
    {

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<Supplierpaymentinvoice>();
            result.Item = new Supplierpaymentinvoice();
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<SupplierpaymentinvoiceCreteria> request)
        {
            SupplierpaymentinvoiceDataService SupplierpaymentinvoiceData = new SupplierpaymentinvoiceDataService();
            List<Supplierpaymentinvoice> result = await Task<List<Supplierpaymentinvoice>>.Run(() => SupplierpaymentinvoiceData.Search(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Save")]
        public async Task<IHttpActionResult> Save(ActionRequest<Supplierpaymentinvoice> request)
        {
            SupplierpaymentinvoiceDataService SupplierpaymentinvoiceData = new SupplierpaymentinvoiceDataService();
            if (request.Item.id <= 0)
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Create);
                Supplierpaymentinvoice result = await Task<Supplierpaymentinvoice>.Run(() => SupplierpaymentinvoiceData.Insert(request.Item));
                return this.Ok(result);
            }
            else
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Update);
                Supplierpaymentinvoice result = await Task<Supplierpaymentinvoice>.Run(() => SupplierpaymentinvoiceData.Update(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("Insert")]
        public async Task<IHttpActionResult> Insert(ActionRequest<Supplierpaymentinvoice> request)
        {
            SupplierpaymentinvoiceDataService SupplierpaymentinvoiceData = new SupplierpaymentinvoiceDataService();
            Supplierpaymentinvoice result = await Task<Supplierpaymentinvoice>.Run(() => SupplierpaymentinvoiceData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        public async Task<IHttpActionResult> Update(ActionRequest<Supplierpaymentinvoice> request)
        {
            SupplierpaymentinvoiceDataService SupplierpaymentinvoiceData = new SupplierpaymentinvoiceDataService();
            Supplierpaymentinvoice result = await Task<Supplierpaymentinvoice>.Run(() => SupplierpaymentinvoiceData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IHttpActionResult> Delete(ActionRequest<Supplierpaymentinvoice> request)
        {
            SupplierpaymentinvoiceDataService SupplierpaymentinvoiceData = new SupplierpaymentinvoiceDataService();
            bool result = await Task<bool>.Run(() => SupplierpaymentinvoiceData.Delete(request.Item));
            return this.Ok(result);
        }
    }
}