
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
    [RoutePrefix("BuybackService")]
    public class BuybackServiceController : ApiController
    {

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<Buyback>();
            result.Item = new Buyback();
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<BuybackCreteria> request)
        {
            BuybackDataService BuybackData = new BuybackDataService();
            List<Buyback> result = await Task<List<Buyback>>.Run(() => BuybackData.Search(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Save")]
        public async Task<IHttpActionResult> Save(ActionRequest<Buyback> request)
        {
            BuybackDataService BuybackData = new BuybackDataService();
            try
            {
                if (request.Item.id <= 0)
                {
                    Buyback result = await Task<Buyback>.Run(() => BuybackData.Insert(request.Item));
                    return this.Ok(result);
                }
                else
                {
                    Buyback result = await Task<Buyback>.Run(() => BuybackData.Update(request.Item));
                    return this.Ok(result);
                }
            }
            catch (Exception exp)
            {
                return this.Ok(new ActionSingleResponse<Buyback>()
                {
                    Item = request.Item,
                    Success = false,
                    ErrorMessage = exp.Message
                });
            }
        }

        [HttpPost]
        [Route("Insert")]
        public async Task<IHttpActionResult> Insert(ActionRequest<Buyback> request)
        {
            BuybackDataService BuybackData = new BuybackDataService();
            Buyback result = await Task<Buyback>.Run(() => BuybackData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpGet]
        [Route("Barcode/{locationid}/{id}/{id1?}/{id2?}/{id3?}")]
        public async Task<IHttpActionResult> Barcode(string locationid, string id,
            string id1 = null, string id2 = null, string id3 = null)
        {
            BuybackDataService StockData = new BuybackDataService();
            if (!string.IsNullOrEmpty(id1))
                id += "/" + id1;
            if (!string.IsNullOrEmpty(id2))
                id += "/" + id2;
            if (!string.IsNullOrEmpty(id3))
                id += "/" + id3;
            if (id.IndexOf("~dot~") >= 0) id = id.Replace("~dot~", ".");
            if (id.IndexOf("~sls~") >= 0) id = id.Replace("~sls~", "/");
            var result = await Task<Barcodedgoods>.Run(() => StockData.BarcodeSearch(locationid, id));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        public async Task<IHttpActionResult> Update(ActionRequest<Buyback> request)
        {
            BuybackDataService BuybackData = new BuybackDataService();
            Buyback result = await Task<Buyback>.Run(() => BuybackData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IHttpActionResult> Delete(ActionRequest<Buyback> request)
        {
            BuybackDataService BuybackData = new BuybackDataService();
            bool result = await Task<bool>.Run(() => BuybackData.Delete(request.Item));
            return this.Ok(result);
        }
    }
}