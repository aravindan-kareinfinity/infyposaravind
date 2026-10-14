
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
    [RoutePrefix("TailoringitemsreceivedService")]
    public class TailoringitemsreceivedServiceController : ApiController
    {

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<Tailoringitemsreceived>();
            result.Item = new Tailoringitemsreceived();
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<TailoringitemsreceivedCreteria> request)
        {
            TailoringitemsreceivedDataService TailoringitemsreceivedData = new TailoringitemsreceivedDataService();
            List<TailoringitemsreceivedDetail> result = await Task<List<TailoringitemsreceivedDetail>>.Run(() => TailoringitemsreceivedData.Search(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("SearchBundle")]
        public async Task<IHttpActionResult> SearchBundle(ActionRequest<TailoringitemsreceivedCreteria> request)
        {
            TailoringitemsreceivedDataService TailoringitemsissuedData = new TailoringitemsreceivedDataService();
            List<TailoringInwardBundle> result = await Task<List<TailoringInwardBundle>>.Run(() => TailoringitemsissuedData.SearchBundle(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("SaveBundle")]
        public async Task<IHttpActionResult> Save(ActionMulitpleRequest<TailoringitemsreceivedDetail> request)
        {
            TailoringitemsreceivedDataService TailoringitemsissuedData = new TailoringitemsreceivedDataService();
            List<TailoringitemsreceivedDetail> result = await Task<List<TailoringitemsreceivedDetail>>.Run(() => TailoringitemsissuedData.Insert(request.Items));
            return this.Ok(result);
        }


        [HttpPost]
        [Route("Save")]
        public async Task<IHttpActionResult> Save(ActionRequest<Tailoringitemsreceived> request)
        {
            TailoringitemsreceivedDataService TailoringitemsreceivedData = new TailoringitemsreceivedDataService();
            if (request.Item.id <= 0)
            {
                Tailoringitemsreceived result = await Task<Tailoringitemsreceived>.Run(() => TailoringitemsreceivedData.Insert(request.Item));
                return this.Ok(result);
            }
            else
            {
                Tailoringitemsreceived result = await Task<Tailoringitemsreceived>.Run(() => TailoringitemsreceivedData.Update(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("Insert")]
        public async Task<IHttpActionResult> Insert(ActionRequest<Tailoringitemsreceived> request)
        {
            TailoringitemsreceivedDataService TailoringitemsreceivedData = new TailoringitemsreceivedDataService();
            Tailoringitemsreceived result = await Task<Tailoringitemsreceived>.Run(() => TailoringitemsreceivedData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        public async Task<IHttpActionResult> Update(ActionRequest<Tailoringitemsreceived> request)
        {
            TailoringitemsreceivedDataService TailoringitemsreceivedData = new TailoringitemsreceivedDataService();
            Tailoringitemsreceived result = await Task<Tailoringitemsreceived>.Run(() => TailoringitemsreceivedData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IHttpActionResult> Delete(ActionRequest<Tailoringitemsreceived> request)
        {
            TailoringitemsreceivedDataService TailoringitemsreceivedData = new TailoringitemsreceivedDataService();
            bool result = await Task<bool>.Run(() => TailoringitemsreceivedData.Delete(request.Item));
            return this.Ok(result);
        }
    }
}