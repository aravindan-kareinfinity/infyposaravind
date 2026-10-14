
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
    [RoutePrefix("StockauditService")]
    public class StockauditServiceController : ApiController
    {

        [AllowAnonymous]
        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<Stockaudit>();
            result.Item = new Stockaudit();
            result.Item.additional = new List<StockMiniReport>();
            result.Item.additional.Add(new StockMiniReport()
            {
                barcode = "134",
                physicalqty = 1,
                systemqty = 2
            });
            result.Item.condition = new Stockaudit.Condition();
            result.Item.matched = new List<StockMiniReport>();
            result.Item.missing = new List<StockMiniReport>();
            
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<StockauditCreteria> request)
        {
            StockauditDataService StockauditData = new StockauditDataService();
            List<Stockaudit> result = await Task<List<Stockaudit>>.Run(() => StockauditData.Search(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Save")]
        public async Task<IHttpActionResult> Save(ActionRequest<Stockaudit> request)
        {
            StockauditDataService StockauditData = new StockauditDataService();
            if (request.Item.id <= 0)
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Create);
                Stockaudit result = await Task<Stockaudit>.Run(() => StockauditData.Insert(request.Item));
                return this.Ok(result);
            }
            else
            {
                Stockaudit result = await Task<Stockaudit>.Run(() => StockauditData.Update(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("Insert")]
        public async Task<IHttpActionResult> Insert(ActionRequest<Stockaudit> request)
        {
            StockauditDataService StockauditData = new StockauditDataService();
            Stockaudit result = await Task<Stockaudit>.Run(() => StockauditData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        public async Task<IHttpActionResult> Update(ActionRequest<Stockaudit> request)
        {
            StockauditDataService StockauditData = new StockauditDataService();
            Stockaudit result = await Task<Stockaudit>.Run(() => StockauditData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IHttpActionResult> Delete(ActionRequest<Stockaudit> request)
        {
            StockauditDataService StockauditData = new StockauditDataService();
            bool result = await Task<bool>.Run(() => StockauditData.Delete(request.Item));
            return this.Ok(result);
        }
    }
}