
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
    [RoutePrefix("ReorderrulesService")]
    public class ReorderrulesServiceController : ApiController
    {

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<Reorderrules>();
            result.Item = new Reorderrules();
            return this.Ok(result);
        }


        [HttpPost]
        [Route("ReorderMatrix")]
        public async Task<IHttpActionResult> SearchMatrix(ActionRequest<ReorderrulesCreteria> request)
        {
            ReorderrulesDataService ReorderrulesData = new ReorderrulesDataService();
            ReorderMatrixSummary result = await Task<ReorderMatrixSummary>.Run(() => ReorderrulesData.SearchMatrix(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<ReorderrulesCreteria> request)
        {
            ReorderrulesDataService ReorderrulesData = new ReorderrulesDataService();
            List<Reorderrules> result = await Task<List<Reorderrules>>.Run(() => ReorderrulesData.Search(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Report")]
        public async Task<IHttpActionResult> Report(ActionRequest<ReorderrulesCreteria> request)
        {
            ReorderrulesDataService ReorderrulesData = new ReorderrulesDataService();
            List<ReorderrulesResult> result = await Task<List<ReorderrulesResult>>.Run(() => ReorderrulesData.Report(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("ReportItems")]
        public async Task<IHttpActionResult> ReportItems(ActionRequest<ReorderrulesCreteria> request)
        {
            ReorderrulesDataService ReorderrulesData = new ReorderrulesDataService();
            List<ReorderrulesResult> result = await Task<List<ReorderrulesResult>>.Run(() => ReorderrulesData.ReportItems(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("SaveAll")]
        public async Task<IHttpActionResult> SaveAll(ActionRequest<Reorder> request)
        {
            ReorderrulesDataService ReorderrulesData = new ReorderrulesDataService();
            Reorder result = await Task<Reorder>.Run(() => ReorderrulesData.SaveAll(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Save")]
        public async Task<IHttpActionResult> Save(ActionRequest<Reorderrules> request)
        {
            ReorderrulesDataService ReorderrulesData = new ReorderrulesDataService();
            if (request.Item.id <= 0)
            {
                Reorderrules result = await Task<Reorderrules>.Run(() => ReorderrulesData.Insert(request.Item));
                return this.Ok(result);
            }
            else
            {
                Reorderrules result = await Task<Reorderrules>.Run(() => ReorderrulesData.Update(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("Insert")]
        public async Task<IHttpActionResult> Insert(ActionRequest<Reorderrules> request)
        {
            ReorderrulesDataService ReorderrulesData = new ReorderrulesDataService();
            Reorderrules result = await Task<Reorderrules>.Run(() => ReorderrulesData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        public async Task<IHttpActionResult> Update(ActionRequest<Reorderrules> request)
        {
            ReorderrulesDataService ReorderrulesData = new ReorderrulesDataService();
            Reorderrules result = await Task<Reorderrules>.Run(() => ReorderrulesData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IHttpActionResult> Delete(ActionRequest<Reorderrules> request)
        {
            ReorderrulesDataService ReorderrulesData = new ReorderrulesDataService();
            bool result = await Task<bool>.Run(() => ReorderrulesData.Delete(request.Item));
            return this.Ok(result);
        }
    }
}