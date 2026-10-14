
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
    [RoutePrefix("DailypriceService")]
    public class DailypriceServiceController : ApiController
    {

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<Dailyprice>();
            result.Item = new Dailyprice();
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<DailypriceCreteria> request)
        {
            DailypriceDataService DailypriceData = new DailypriceDataService();
            List<DailyPriceDetail> result = await Task<List<DailyPriceDetail>>.Run(() => DailypriceData.Search(request.Item));
            return this.Ok(result);
        }


        [HttpPost]
        [Route("PriceList")]
        public async Task<IHttpActionResult> NewPriceList(ActionRequest<DailypriceCreteria> request)
        {
            DailypriceDataService DailypriceData = new DailypriceDataService();
            Dailyprice result = await Task<Dailyprice>.Run(() => DailypriceData.NewPriceList(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Copy")]
        public async Task<IHttpActionResult> Copy(ActionRequest<DailypriceCreteria> request)
        {
            DailypriceDataService DailypriceData = new DailypriceDataService();
            bool result = await Task<bool>.Run(() => DailypriceData.Copy(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Apply")]
        public async Task<IHttpActionResult> Apply(ActionRequest<DailypriceCreteria> request)
        {
            DailypriceDataService DailypriceData = new DailypriceDataService();
            bool result = await Task<bool>.Run(() => DailypriceData.Apply(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Save")]
        public async Task<IHttpActionResult> Save(ActionRequest<Dailyprice> request)
        {
            DailypriceDataService DailypriceData = new DailypriceDataService();
            if (request.Item.id <= 0)
            {
                Dailyprice result = await Task<Dailyprice>.Run(() => DailypriceData.Insert(request.Item));
                return this.Ok(result);
            }
            else
            {
                Dailyprice result = await Task<Dailyprice>.Run(() => DailypriceData.Update(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("Insert")]
        public async Task<IHttpActionResult> Insert(ActionRequest<Dailyprice> request)
        {
            DailypriceDataService DailypriceData = new DailypriceDataService();
            Dailyprice result = await Task<Dailyprice>.Run(() => DailypriceData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        public async Task<IHttpActionResult> Update(ActionRequest<Dailyprice> request)
        {
            DailypriceDataService DailypriceData = new DailypriceDataService();
            Dailyprice result = await Task<Dailyprice>.Run(() => DailypriceData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IHttpActionResult> Delete(ActionRequest<Dailyprice> request)
        {
            DailypriceDataService DailypriceData = new DailypriceDataService();
            bool result = await Task<bool>.Run(() => DailypriceData.Delete(request.Item));
            return this.Ok(result);
        }
    }
}