
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
    [RoutePrefix("BOMConsumptionitemsService")]
    public class BOMConsumptionitemsServiceController : ApiController
    {

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<BOMConsumptionitems>();
            result.Item = new BOMConsumptionitems();
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<BOMConsumptionitemsCreteria> request)
        {
            BOMConsumptionitemsDataService BOMConsumptionitemsData = new BOMConsumptionitemsDataService();
            List<BOMConsumptionitemsDetail> result = await Task<List<BOMConsumptionitemsDetail>>.Run(() => BOMConsumptionitemsData.Search(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Save")]
        public async Task<IHttpActionResult> Save(ActionRequest<BOMConsumptionitemsDetail> request)
        {
            BOMConsumptionitemsDataService BOMConsumptionitemsData = new BOMConsumptionitemsDataService();
            if (request.Item.id <= 0)
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Create);
                BOMConsumptionitems result = await Task<BOMConsumptionitems>.Run(() => BOMConsumptionitemsData.Insert(request.Item));
                return this.Ok(result);
            }
            else
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Update);
                BOMConsumptionitems result = await Task<BOMConsumptionitems>.Run(() => BOMConsumptionitemsData.Update(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("Insert")]
        public async Task<IHttpActionResult> Insert(ActionRequest<BOMConsumptionitemsDetail> request)
        {
            BOMConsumptionitemsDataService BOMConsumptionitemsData = new BOMConsumptionitemsDataService();
            BOMConsumptionitems result = await Task<BOMConsumptionitems>.Run(() => BOMConsumptionitemsData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        public async Task<IHttpActionResult> Update(ActionRequest<BOMConsumptionitemsDetail> request)
        {
            BOMConsumptionitemsDataService BOMConsumptionitemsData = new BOMConsumptionitemsDataService();
            BOMConsumptionitems result = await Task<BOMConsumptionitems>.Run(() => BOMConsumptionitemsData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IHttpActionResult> Delete(ActionRequest<BOMConsumptionitems> request)
        {
            BOMConsumptionitemsDataService BOMConsumptionitemsData = new BOMConsumptionitemsDataService();
            bool result = await Task<bool>.Run(() => BOMConsumptionitemsData.Delete(request.Item));
            return this.Ok(result);
        }
    }
}