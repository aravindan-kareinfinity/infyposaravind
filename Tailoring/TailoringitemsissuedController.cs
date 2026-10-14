
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
    [RoutePrefix("TailoringitemsissuedService")]
    public class TailoringitemsissuedServiceController : ApiController
    {

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<Tailoringitemsissued>();
            result.Item = new Tailoringitemsissued();
            return this.Ok(result);
        }

        [HttpPost]
        [Route("SearchBundle")]
        public async Task<IHttpActionResult> SearchBundle(ActionRequest<TailoringitemsissuedCreteria> request)
        {
            TailoringitemsissuedDataService TailoringitemsissuedData = new TailoringitemsissuedDataService();
            List<TailoringOutwardBundle> result = await Task<List<TailoringOutwardBundle>>.Run(() => TailoringitemsissuedData.SearchBundle(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("SaveBundle")]
        public async Task<IHttpActionResult> Save(ActionMulitpleRequest<TailoringitemsissuedDetail> request)
        {
            TailoringitemsissuedDataService TailoringitemsissuedData = new TailoringitemsissuedDataService();

            List<TailoringitemsissuedDetail> result = await Task<List<TailoringitemsissuedDetail>>.Run(() => TailoringitemsissuedData.Insert(request.Items));
            return this.Ok(result);
        }


        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<TailoringitemsissuedCreteria> request)
        {
            TailoringitemsissuedDataService TailoringitemsissuedData = new TailoringitemsissuedDataService();
            List<TailoringitemsissuedDetail> result = await Task<List<TailoringitemsissuedDetail>>.Run(() => TailoringitemsissuedData.Search(request.Item));
            return this.Ok(result);
        }


        [HttpPost]
        [Route("Save")]
        public async Task<IHttpActionResult> Save(ActionRequest<Tailoringitemsissued> request)
        {
            TailoringitemsissuedDataService TailoringitemsissuedData = new TailoringitemsissuedDataService();
            if (request.Item.id <= 0)
            {
                Tailoringitemsissued result = await Task<Tailoringitemsissued>.Run(() => TailoringitemsissuedData.Insert(request.Item));
                return this.Ok(result);
            }
            else
            {
                Tailoringitemsissued result = await Task<Tailoringitemsissued>.Run(() => TailoringitemsissuedData.Update(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("Insert")]
        public async Task<IHttpActionResult> Insert(ActionRequest<Tailoringitemsissued> request)
        {
            TailoringitemsissuedDataService TailoringitemsissuedData = new TailoringitemsissuedDataService();
            Tailoringitemsissued result = await Task<Tailoringitemsissued>.Run(() => TailoringitemsissuedData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        public async Task<IHttpActionResult> Update(ActionRequest<Tailoringitemsissued> request)
        {
            TailoringitemsissuedDataService TailoringitemsissuedData = new TailoringitemsissuedDataService();
            Tailoringitemsissued result = await Task<Tailoringitemsissued>.Run(() => TailoringitemsissuedData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IHttpActionResult> Delete(ActionRequest<Tailoringitemsissued> request)
        {
            TailoringitemsissuedDataService TailoringitemsissuedData = new TailoringitemsissuedDataService();
            bool result = await Task<bool>.Run(() => TailoringitemsissuedData.Delete(request.Item));
            return this.Ok(result);
        }
    }
}