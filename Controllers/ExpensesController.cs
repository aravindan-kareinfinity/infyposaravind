
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
    [RoutePrefix("ExpensesService")]
    public class ExpensesServiceController : ApiController
    {

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<Expenses>();
            result.Item = new Expenses();
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<ExpensesCreteria> request)
        {
            ExpensesDataService ExpensesData = new ExpensesDataService();
            List<Expenses> result = await Task<List<Expenses>>.Run(() => ExpensesData.Search(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Save")]
        public async Task<IHttpActionResult> Save(ActionRequest<Expenses> request)
        {
            ExpensesDataService ExpensesData = new ExpensesDataService();
            if (request.Item.id <= 0)
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Create);
                Expenses result = await Task<Expenses>.Run(() => ExpensesData.Insert(request.Item));
                return this.Ok(result);
            }
            else
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Update);
                Expenses result = await Task<Expenses>.Run(() => ExpensesData.Update(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("Insert")]
        public async Task<IHttpActionResult> Insert(ActionRequest<Expenses> request)
        {
            ExpensesDataService ExpensesData = new ExpensesDataService();
            Expenses result = await Task<Expenses>.Run(() => ExpensesData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        public async Task<IHttpActionResult> Update(ActionRequest<Expenses> request)
        {
            ExpensesDataService ExpensesData = new ExpensesDataService();
            Expenses result = await Task<Expenses>.Run(() => ExpensesData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IHttpActionResult> Delete(ActionRequest<Expenses> request)
        {
            ExpensesDataService ExpensesData = new ExpensesDataService();
            bool result = await Task<bool>.Run(() => ExpensesData.Delete(request.Item));
            return this.Ok(result);
        }
    }
}