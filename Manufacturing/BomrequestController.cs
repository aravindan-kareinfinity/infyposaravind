
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
    [RoutePrefix("BomrequestService")]
    public class BomrequestServiceController : ApiController
    {

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<BOMRequest>();
            result.Item = new BOMRequest();
            return this.Ok(result);
        }


        [HttpPost]
        [Route("UpdateStatus")]
        public async Task<IHttpActionResult> UpdateStatus(ActionRequest<BOMStatus> request)
        {
            BomrequestDataService BomrequestData = new BomrequestDataService();
            bool result = await Task<bool>.Run(() => BomrequestData.UpdateStatus(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("SearchInbox")]
        public async Task<IHttpActionResult> SearchInbox(ActionRequest<BomrequestCreteria> request)
        {
            BomrequestDataService BomrequestData = new BomrequestDataService();
            List<BomInboxItem> result = await Task<List<BomInboxItem>>.Run(() => BomrequestData.SearchInbox(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Import4Job")]
        public async Task<IHttpActionResult> Import4Job(ActionRequest<BomrequestCreteria> request)
        {
            BomrequestDataService BomrequestData = new BomrequestDataService();
            BOMConsumption result = await Task<BOMConsumption>.Run(() => BomrequestData.Import4Job(request.Item));
            return this.Ok(result);
        }


        [HttpPost]
        [Route("ImportBOM")]
        public async Task<IHttpActionResult> ImportBOM(ActionRequest<BomrequestCreteria> request)
        {
            BomrequestDataService BOMConsumptionData = new BomrequestDataService();
            List<BOMConsumptionitemsDetail> result = await Task<List<BOMConsumptionitemsDetail>>.Run(() => BOMConsumptionData.ImportBOM(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Create")]
        public async Task<IHttpActionResult> Create(ActionRequest<BomrequestCreteria> request)
        {
            BomrequestDataService BomrequestData = new BomrequestDataService();
            List<BOMRequest> result = await Task<List<BOMRequest>>.Run(() => BomrequestData.CreateRequest(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<BomrequestCreteria> request)
        {
            BomrequestDataService BomrequestData = new BomrequestDataService();
            List<BOMRequest> result = await Task<List<BOMRequest>>.Run(() => BomrequestData.Search(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Save")]
        public async Task<IHttpActionResult> Save(ActionRequest<BOMRequest> request)
        {
            BomrequestDataService BomrequestData = new BomrequestDataService();
            if (request.Item.id <= 0)
            {
                BOMRequest result = await Task<BOMRequest>.Run(() => BomrequestData.Insert(request.Item));
                return this.Ok(result);
            }
            else
            {
                BOMRequest result = await Task<BOMRequest>.Run(() => BomrequestData.Update(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("Insert")]
        public async Task<IHttpActionResult> Insert(ActionRequest<BOMRequest> request)
        {
            BomrequestDataService BomrequestData = new BomrequestDataService();
            BOMRequest result = await Task<BOMRequest>.Run(() => BomrequestData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        public async Task<IHttpActionResult> Update(ActionRequest<BOMRequest> request)
        {
            BomrequestDataService BomrequestData = new BomrequestDataService();
            BOMRequest result = await Task<BOMRequest>.Run(() => BomrequestData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IHttpActionResult> Delete(ActionRequest<BOMRequest> request)
        {
            BomrequestDataService BomrequestData = new BomrequestDataService();
            bool result = await Task<bool>.Run(() => BomrequestData.Delete(request.Item));
            return this.Ok(result);
        }
    }
}