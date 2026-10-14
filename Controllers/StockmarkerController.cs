
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
    [RoutePrefix("StockmarkerService")]
    public class StockmarkerServiceController : ApiController
    {

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<Stockmarker>();
            result.Item = new Stockmarker();
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<StockmarkerCreteria> request)
        {
            StockmarkerDataService StockmarkerData = new StockmarkerDataService();
            List<Stockmarkerdetail> result = await Task<List<Stockmarkerdetail>>.Run(() => StockmarkerData.Search(request.Item));
            return this.Ok(result);
        }



        [HttpPost]
        [Route("SearchStock")]
        public async Task<IHttpActionResult> SearchStock(ActionRequest<StockmarkerCreteria> request)
        {
            StockmarkerDataService StockmarkerData = new StockmarkerDataService();
            List<Stockmarkerdetail> result = await Task<List<Stockmarkerdetail>>.Run(() => StockmarkerData.SearchStock(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("SaveOLQTY")]
        public async Task<IHttpActionResult> SaveOLQTY(ActionMulitpleRequest<StockMarkerOLQty> request)
        {
            StockmarkerDataService StockmarkerData = new StockmarkerDataService();
            bool result = await Task<bool>.Run(() => StockmarkerData.SaveOLQTY(request.Items));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Save")]
        public async Task<IHttpActionResult> Save(ActionRequest<MarkerContainer> request)
        {
            List<Stockmarker> result = null;
            StockmarkerDataService StockmarkerData = new StockmarkerDataService();

            if (request == null || request.Item == null)
                throw new Exception("Request object is null");

            if (!request.Item.isinvoice)
            {
                if (request.Item.items == null || request.Item.items.Count == 0)
                    throw new Exception("Request object is null");
            }
            else
            {
                if (request.Item.isinvoice)
                    request.Item.items = await Task<List<Stockmarker>>.Run(() => StockmarkerData.UpdateBarcodes(request.Item));
            }
            if(request.Item.deleteall == true)
                result = await Task<List<Stockmarker>>.Run(() => StockmarkerData.Delete(request.Item));
            else if (request.Item.delete == true)
                result = await Task<List<Stockmarker>>.Run(() => StockmarkerData.Delete(request.Item));
            else
                result = await Task<List<Stockmarker>>.Run(() => StockmarkerData.Save(request.Item));
            return this.Ok(result);
        }
      

        //[HttpPost]
        //[Route("Insert")]
        //public async Task<IHttpActionResult> Insert(ActionRequest<Stockmarker> request)
        //{
        //    StockmarkerDataService StockmarkerData = new StockmarkerDataService();
        //    Stockmarker result = await Task<Stockmarker>.Run(() => StockmarkerData.Insert(request.Item));
        //    return this.Ok(result);
        //}

        //[HttpPost]
        //[Route("Update")]
        //public async Task<IHttpActionResult> Update(ActionRequest<Stockmarker> request)
        //{
        //    StockmarkerDataService StockmarkerData = new StockmarkerDataService();
        //    Stockmarker result = await Task<Stockmarker>.Run(() => StockmarkerData.Update(request.Item));
        //    return this.Ok(result);
        //}


    }
}