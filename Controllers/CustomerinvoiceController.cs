
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
    [RoutePrefix("CustomerinvoiceService")]
    public class CustomerinvoiceServiceController : ApiController
    {

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<Customerinvoice>();
            result.Item = new Customerinvoice();
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<CustomerinvoiceCreteria> request)
        {
            CustomerinvoiceDataService CustomerinvoiceData = new CustomerinvoiceDataService();
            List<Customerinvoice> result = await Task<List<Customerinvoice>>.Run(() => CustomerinvoiceData.Search(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Save")]
        public async Task<IHttpActionResult> Save(ActionRequest<Customerinvoice> request)
        {
            CustomerinvoiceDataService CustomerinvoiceData = new CustomerinvoiceDataService();
            if (request.Item.id <= 0)
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Create);
                Customerinvoice result = await Task<Customerinvoice>.Run(() => CustomerinvoiceData.Insert(request.Item));
                return this.Ok(result);
            }
            else
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Update);
                Customerinvoice result = await Task<Customerinvoice>.Run(() => CustomerinvoiceData.Update(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("Insert")]
        public async Task<IHttpActionResult> Insert(ActionRequest<Customerinvoice> request)
        {
            CustomerinvoiceDataService CustomerinvoiceData = new CustomerinvoiceDataService();
            Customerinvoice result = await Task<Customerinvoice>.Run(() => CustomerinvoiceData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        public async Task<IHttpActionResult> Update(ActionRequest<Customerinvoice> request)
        {
            CustomerinvoiceDataService CustomerinvoiceData = new CustomerinvoiceDataService();
            Customerinvoice result = await Task<Customerinvoice>.Run(() => CustomerinvoiceData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IHttpActionResult> Delete(ActionRequest<CustomerinvoiceCreteria> request)
        {
            CustomerinvoiceDataService CustomerinvoiceData = new CustomerinvoiceDataService();
            bool result = await Task<bool>.Run(() => CustomerinvoiceData.Delete(request.Item));
            return this.Ok(result);
        }
    }
}