
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
    [RoutePrefix("LedgerstockopeningService")]
    public class LedgerstockopeningServiceController : ApiController
    {

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<Ledgerstockopening>();
            result.Item = new Ledgerstockopening();
            return this.Ok(result);
        }

        [HttpPost]
        [Route("FindStockOpening")]
        public async Task<IHttpActionResult> FindStockOpening(ActionRequest<LedgerstockopeningCreteria> request)
        {
            LedgerstockopeningDataService LedgerstockopeningData = new LedgerstockopeningDataService();
            LedgerstockopeningCreteria result = await Task<LedgerstockopeningCreteria>.Run(() => LedgerstockopeningData.FindStockOpening(request.Item));
            return this.Ok(result);
        }


        [HttpGet]
        [Route("Find/{ledgerid}")]
        public async Task<IHttpActionResult> Find(string ledgerid)
        {
            LedgerstockopeningDataService LedgerstockopeningData = new LedgerstockopeningDataService();
            List<Ledgerstockopening> result = await Task<List<Ledgerstockopening>>.Run(() => LedgerstockopeningData.Search(new LedgerstockopeningCreteria() { ledgerid = long.Parse(ledgerid) }));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<LedgerstockopeningCreteria> request)
        {
            LedgerstockopeningDataService LedgerstockopeningData = new LedgerstockopeningDataService();
            List<Ledgerstockopening> result = await Task<List<Ledgerstockopening>>.Run(() => LedgerstockopeningData.Search(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("SaveAll")]
        public async Task<IHttpActionResult> SaveAll(ActionRequest<LedgerstockopeningCreteria> request)
        {
            LedgerstockopeningDataService LedgerstockopeningData = new LedgerstockopeningDataService();
            LedgerstockopeningCreteria result = await Task<LedgerstockopeningCreteria>.Run(() => LedgerstockopeningData.SaveAll(request.Item));
            return this.Ok(result);
        }


        [HttpPost]
        [Route("Save")]
        public async Task<IHttpActionResult> Save(ActionRequest<Ledgerstockopening> request)
        {
            LedgerstockopeningDataService LedgerstockopeningData = new LedgerstockopeningDataService();
            if (request.Item.id <= 0)
            {
                Ledgerstockopening result = await Task<Ledgerstockopening>.Run(() => LedgerstockopeningData.Insert(request.Item));
                return this.Ok(result);
            }
            else
            {
                Ledgerstockopening result = await Task<Ledgerstockopening>.Run(() => LedgerstockopeningData.Update(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("Insert")]
        public async Task<IHttpActionResult> Insert(ActionRequest<Ledgerstockopening> request)
        {
            LedgerstockopeningDataService LedgerstockopeningData = new LedgerstockopeningDataService();
            Ledgerstockopening result = await Task<Ledgerstockopening>.Run(() => LedgerstockopeningData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        public async Task<IHttpActionResult> Update(ActionRequest<Ledgerstockopening> request)
        {
            LedgerstockopeningDataService LedgerstockopeningData = new LedgerstockopeningDataService();
            Ledgerstockopening result = await Task<Ledgerstockopening>.Run(() => LedgerstockopeningData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IHttpActionResult> Delete(ActionRequest<Ledgerstockopening> request)
        {
            LedgerstockopeningDataService LedgerstockopeningData = new LedgerstockopeningDataService();
            bool result = await Task<bool>.Run(() => LedgerstockopeningData.Delete(request.Item));
            return this.Ok(result);
        }
    }
}