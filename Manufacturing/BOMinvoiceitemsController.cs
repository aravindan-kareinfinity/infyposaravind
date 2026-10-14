
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
    [RoutePrefix("BominvoiceitemsService")]
    public class BominvoiceitemsServiceController : ApiController
    {

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<Bominvoiceitems>();
            result.Item = new Bominvoiceitems();
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<BominvoiceitemsCreteria> request)
        {
            BominvoiceitemsDataService BominvoiceitemsData = new BominvoiceitemsDataService();
            List<Bominvoiceitems> result = await Task<List<Bominvoiceitems>>.Run(() => BominvoiceitemsData.Search(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Save")]
        public async Task<IHttpActionResult> Save(ActionRequest<Bominvoiceitems> request)
        {
            BominvoiceitemsDataService BominvoiceitemsData = new BominvoiceitemsDataService();
            if (request.Item.id <= 0)
            {
                Bominvoiceitems result = await Task<Bominvoiceitems>.Run(() => BominvoiceitemsData.Insert(request.Item));
                return this.Ok(result);
            }
            else
            {
                Bominvoiceitems result = await Task<Bominvoiceitems>.Run(() => BominvoiceitemsData.Update(request.Item));
                return this.Ok(result);
            }
        }

        [Route("SearchCode/{code}")]
        [HttpGet]
        public async Task<IHttpActionResult> SearchCode(string code)
        {
            BominvoiceitemsDataService BominvoiceitemsData = new BominvoiceitemsDataService();
            Bominvoiceitems result = await Task<Bominvoiceitems>.Run(() => BominvoiceitemsData.SearchCode(code));
            return this.Ok(result);
        }

        [Route("SearchBOMStock/{code}")]
        [HttpGet]
        public async Task<IHttpActionResult> SearchBOMStock(string code)
        {
            BomstocktransactionDataService bomstocktransactionDataService = new BomstocktransactionDataService();
            BominvoiceitemsDataService BominvoiceitemsData = new BominvoiceitemsDataService();
            BOMItemWithStock result = await Task<BOMItemWithStock>.Run(() =>
            {
                BOMItemWithStock bOMItemWithStock = new BOMItemWithStock();
                bOMItemWithStock.bominvoiceitem = BominvoiceitemsData.SearchCode(code);
                bOMItemWithStock.stocksummary = bomstocktransactionDataService.StockReport(new BomstocktransactionCreteria()
                {
                    bomid = bOMItemWithStock.bominvoiceitem.bomid
                });
                return bOMItemWithStock;
            });
            return this.Ok(result);

        }

        [HttpPost]
        [Route("Insert")]
        public async Task<IHttpActionResult> Insert(ActionRequest<Bominvoiceitems> request)
        {
            BominvoiceitemsDataService BominvoiceitemsData = new BominvoiceitemsDataService();
            Bominvoiceitems result = await Task<Bominvoiceitems>.Run(() => BominvoiceitemsData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        public async Task<IHttpActionResult> Update(ActionRequest<Bominvoiceitems> request)
        {
            BominvoiceitemsDataService BominvoiceitemsData = new BominvoiceitemsDataService();
            Bominvoiceitems result = await Task<Bominvoiceitems>.Run(() => BominvoiceitemsData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IHttpActionResult> Delete(ActionRequest<Bominvoiceitems> request)
        {
            BominvoiceitemsDataService BominvoiceitemsData = new BominvoiceitemsDataService();
            bool result = await Task<bool>.Run(() => BominvoiceitemsData.Delete(request.Item));
            return this.Ok(result);
        }
    }
}