
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
    [RoutePrefix("PaymentsService")]
    public class PaymentsServiceController : ApiController
    {

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<Payments>();
            result.Item = new Payments();
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<PaymentsCreteria> request)
        {
            PaymentsDataService PaymentsData = new PaymentsDataService();
            List<Payments> result = await Task<List<Payments>>.Run(() => PaymentsData.Search(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Save")]
        public async Task<IHttpActionResult> Save(ActionRequest<Payments> request)
        {
            PaymentsDataService PaymentsData = new PaymentsDataService();
            if (request.Item.id <= 0)
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Create);
                Payments result = await Task<Payments>.Run(() => PaymentsData.Insert(request.Item));
                return this.Ok(result);
            }
            else
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Update);
                Payments result = await Task<Payments>.Run(() => PaymentsData.Update(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("Insert")]
        public async Task<IHttpActionResult> Insert(ActionRequest<Payments> request)
        {
            PaymentsDataService PaymentsData = new PaymentsDataService();
            Payments result = await Task<Payments>.Run(() => PaymentsData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        public async Task<IHttpActionResult> Update(ActionRequest<Payments> request)
        {
            PaymentsDataService PaymentsData = new PaymentsDataService();
            Payments result = await Task<Payments>.Run(() => PaymentsData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IHttpActionResult> Delete(ActionRequest<Payments> request)
        {
            PaymentsDataService PaymentsData = new PaymentsDataService();
            bool result = await Task<bool>.Run(() => PaymentsData.Delete(request.Item));
            return this.Ok(result);
        }
    }
}