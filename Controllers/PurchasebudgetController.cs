
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
    [RoutePrefix("PurchasebudgetService")]
    public class PurchasebudgetServiceController : ApiController
    {

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<Purchasebudget>();
            result.Item = new Purchasebudget();
            return this.Ok(result);
        }


        [HttpPost]
        [Route("Report")]
        public async Task<IHttpActionResult> Report(ActionRequest<PurchasebudgetCreteria> request)
        {
            PurchasebudgetDataService PurchasebudgetData = new PurchasebudgetDataService();
            List<Purchasebudget.ReportItem> result = await Task<List<Purchasebudget.ReportItem>>.Run(() => PurchasebudgetData.Report(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<PurchasebudgetCreteria> request)
        {
            PurchasebudgetDataService PurchasebudgetData = new PurchasebudgetDataService();
            List<Purchasebudget> result = await Task<List<Purchasebudget>>.Run(() => PurchasebudgetData.Search(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("SaveAll")]
        public async Task<IHttpActionResult> Save(ActionMulitpleRequest<Purchasebudget> request)
        {
            PurchasebudgetDataService PurchasebudgetData = new PurchasebudgetDataService();
            int result = await Task<int>.Run(() => PurchasebudgetData.SaveAll(request.Items));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Save")]
        public async Task<IHttpActionResult> Save(ActionRequest<Purchasebudget> request)
        {
            PurchasebudgetDataService PurchasebudgetData = new PurchasebudgetDataService();
            if (request.Item.id <= 0)
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Create);
                Purchasebudget result = await Task<Purchasebudget>.Run(() => PurchasebudgetData.Insert(request.Item));
                return this.Ok(result);
            }
            else
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Update);
                Purchasebudget result = await Task<Purchasebudget>.Run(() => PurchasebudgetData.Update(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("Insert")]
        public async Task<IHttpActionResult> Insert(ActionRequest<Purchasebudget> request)
        {
            PurchasebudgetDataService PurchasebudgetData = new PurchasebudgetDataService();
            Purchasebudget result = await Task<Purchasebudget>.Run(() => PurchasebudgetData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        public async Task<IHttpActionResult> Update(ActionRequest<Purchasebudget> request)
        {
            PurchasebudgetDataService PurchasebudgetData = new PurchasebudgetDataService();
            Purchasebudget result = await Task<Purchasebudget>.Run(() => PurchasebudgetData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IHttpActionResult> Delete(ActionRequest<Purchasebudget> request)
        {
            PurchasebudgetDataService PurchasebudgetData = new PurchasebudgetDataService();
            bool result = await Task<bool>.Run(() => PurchasebudgetData.Delete(request.Item));
            return this.Ok(result);
        }
    }
}