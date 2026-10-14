
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
    [RoutePrefix("StockdumpingService")]
    public class StockdumpingServiceController : ApiController
    {

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<Stockdumping>();
            result.Item = new Stockdumping();
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<StockdumpingCreteria> request)
        {
            StockdumpingDataService StockdumpingData = new StockdumpingDataService();
            List<Stockdumping> result = await Task<List<Stockdumping>>.Run(() => StockdumpingData.Search(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Save")]
        public async Task<IHttpActionResult> Save(ActionRequest<Stockdumping> request)
        {
            StockdumpingDataService StockdumpingData = new StockdumpingDataService();
            if (request.Item.id <= 0)
            {
                Stockdumping result = await Task<Stockdumping>.Run(() => StockdumpingData.Insert(request.Item));
                return this.Ok(result);
            }
            else
            {
                Stockdumping result = await Task<Stockdumping>.Run(() => StockdumpingData.Update(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("SearchItems")]
        public async Task<IHttpActionResult> SearchItems(ActionRequest<StockdumpingCreteria> request)
        {
            
                StockdumpingDataService stockdataservice = new StockdumpingDataService();
                List<Stockdumping.ItemDetail> result = await Task<List<Stockdumping.ItemDetail>>.Run(() => stockdataservice.SearchItems(request.Item));
                return this.Ok(result);
            
        }


        [HttpPost]
        [Route("Insert")]
        public async Task<IHttpActionResult> Insert(ActionRequest<Stockdumping> request)
        {
            StockdumpingDataService StockdumpingData = new StockdumpingDataService();
            Stockdumping result = await Task<Stockdumping>.Run(() => StockdumpingData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        public async Task<IHttpActionResult> Update(ActionRequest<Stockdumping> request)
        {
            StockdumpingDataService StockdumpingData = new StockdumpingDataService();
            Stockdumping result = await Task<Stockdumping>.Run(() => StockdumpingData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IHttpActionResult> Delete(ActionRequest<Stockdumping> request)
        {
            StockdumpingDataService StockdumpingData = new StockdumpingDataService();
            bool result = await Task<bool>.Run(() => StockdumpingData.Delete(request.Item));
            return this.Ok(result);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("PrintPDF/{organizationid}/{id}")]
        public async Task<IHttpActionResult> PrintPDF(string organizationid, string id)
        {
            StockdumpingDataService stockdumpdata = new StockdumpingDataService();
            stockdumpdata.Context = new UserClaim()
            {
                OrganizationID = long.Parse(organizationid)
            };
            try
            {
                FileContentInfo result = await Task<FileContentInfo>.Run(() =>
                {
                    var stockdumps = stockdumpdata.Search(new StockdumpingCreteria() { id = long.Parse(id) });
                    return stockdumpdata.PrintAdvice(stockdumpdata.Context.OrganizationID, stockdumps[0]);
                });
                return new FileActionResult(false, "", result.Extension, result.Content);
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(stockdumpdata.Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, "PrintPDF/" + id);
                return this.Ok(exp);
            }
        }
    }
}