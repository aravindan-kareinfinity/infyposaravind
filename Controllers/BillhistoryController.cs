
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
    [RoutePrefix("BillhistoryService")]
    public class BillhistoryServiceController : ApiController
    {

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<Billhistory>();
            result.Item = new Billhistory();
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<BillhistoryCreteria> request)
        {
            BillhistoryDataService BillhistoryData = new BillhistoryDataService();
            List<Billhistory> result = await Task<List<Billhistory>>.Run(() => BillhistoryData.Search(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Bill4Update")]
        public async Task<IHttpActionResult> SearchBill4Update(ActionRequest<SaleEdit> request)
        {
            BillhistoryDataService BillData = new BillhistoryDataService();
            try
            {
                BillHistoryInfo result = await Task<BillHistoryInfo>.Run(() => BillData.Bill4Update(request.Item));
                return this.Ok(result);
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(BillData.Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);
                return this.Ok(exp);
            }
        }

        [HttpPost]
        [Route("SearchBill4Edit")]
        public async Task<IHttpActionResult> SearchBill4Edit(ActionRequest<SaleEdit> request)
        {
            BillhistoryDataService BillData = new BillhistoryDataService();
            try
            {
                List<SaleEditItem> result = await Task<List<SaleEditItem>>.Run(() => BillData.SearchBill4Edit(request.Item));
                return this.Ok(result);
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(BillData.Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);
                return this.Ok(exp);
            }
        }

        [HttpPost]
        [Route("Save")]
        public async Task<IHttpActionResult> Save(ActionRequest<Billhistory> request)
        {
            BillhistoryDataService BillhistoryData = new BillhistoryDataService();
            if (request.Item.id <= 0)
            {
                Billhistory result = await Task<Billhistory>.Run(() => BillhistoryData.Insert(request.Item));
                return this.Ok(result);
            }
            else
            {
                Billhistory result = await Task<Billhistory>.Run(() => BillhistoryData.Update(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("Insert")]
        public async Task<IHttpActionResult> Insert(ActionRequest<Billhistory> request)
        {
            BillhistoryDataService BillhistoryData = new BillhistoryDataService();
            Billhistory result = await Task<Billhistory>.Run(() => BillhistoryData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        public async Task<IHttpActionResult> Update(ActionRequest<Billhistory> request)
        {
            BillhistoryDataService BillhistoryData = new BillhistoryDataService();
            Billhistory result = await Task<Billhistory>.Run(() => BillhistoryData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IHttpActionResult> Delete(ActionRequest<Billhistory> request)
        {
            BillhistoryDataService BillhistoryData = new BillhistoryDataService();
            bool result = await Task<bool>.Run(() => BillhistoryData.Delete(request.Item));
            return this.Ok(result);
        }


        [HttpPost]
        [Route("RollBack")]
        public async Task<IHttpActionResult> RollBack(ActionRequest<Billhistory> request)
        {
            BillhistoryDataService BillhistoryData = new BillhistoryDataService();
            bool result = await Task<bool>.Run(() => BillhistoryData.RollBack(request.Item));
            return this.Ok(result);
        }
    }
}