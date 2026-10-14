
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
    [RoutePrefix("TailoringpaymentService")]
    public class TailoringpaymentServiceController : ApiController
    {

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<Tailoringpayment>();
            result.Item = new Tailoringpayment();
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<TailoringpaymentCreteria> request)
        {
            TailoringpaymentDataService TailoringpaymentData = new TailoringpaymentDataService();
            List<TailoringPaymentDetail> result = await Task<List<TailoringPaymentDetail>>.Run(() => TailoringpaymentData.Search(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Save")]
        public async Task<IHttpActionResult> Save(ActionRequest<Tailoringpayment> request)
        {
            TailoringpaymentDataService TailoringpaymentData = new TailoringpaymentDataService();
            if (request.Item.id <= 0)
            {
                Tailoringpayment result = await Task<Tailoringpayment>.Run(() => TailoringpaymentData.Insert(request.Item));
                return this.Ok(result);
            }
            else
            {
                Tailoringpayment result = await Task<Tailoringpayment>.Run(() => TailoringpaymentData.Update(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("Insert")]
        public async Task<IHttpActionResult> Insert(ActionRequest<Tailoringpayment> request)
        {
            TailoringpaymentDataService TailoringpaymentData = new TailoringpaymentDataService();
            Tailoringpayment result = await Task<Tailoringpayment>.Run(() => TailoringpaymentData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        public async Task<IHttpActionResult> Update(ActionRequest<Tailoringpayment> request)
        {
            TailoringpaymentDataService TailoringpaymentData = new TailoringpaymentDataService();
            Tailoringpayment result = await Task<Tailoringpayment>.Run(() => TailoringpaymentData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IHttpActionResult> Delete(ActionRequest<Tailoringpayment> request)
        {
            TailoringpaymentDataService TailoringpaymentData = new TailoringpaymentDataService();
            bool result = await Task<bool>.Run(() => TailoringpaymentData.Delete(request.Item));
            return this.Ok(result);
        }
    }
}