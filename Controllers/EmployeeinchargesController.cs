
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
    [RoutePrefix("EmployeeinchargesService")]
    public class EmployeeinchargesServiceController : ApiController
    {

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<Employeeincharges>();
            result.Item = new Employeeincharges();
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<EmployeeinchargesCreteria> request)
        {
            EmployeeinchargesDataService EmployeeinchargesData = new EmployeeinchargesDataService();
            List<Employeeincharges> result = await Task<List<Employeeincharges>>.Run(() => EmployeeinchargesData.Search(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("ReportPrdBillSales")]
        public async Task<IHttpActionResult> ReportPrdBillSales(ActionRequest<EmployeeinchargesCreteria> request)
        {
            EmployeeinchargesDataService EmployeeinchargesData = new EmployeeinchargesDataService();
            List<EmployeeinchargePrdBillRow> result = await Task<List<EmployeeinchargePrdBillRow>>.Run(() => EmployeeinchargesData.SearchPrdBillSalesByBillDate(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Save")]
        public async Task<IHttpActionResult> Save(ActionRequest<Employeeincharges> request)
        {
            EmployeeinchargesDataService EmployeeinchargesData = new EmployeeinchargesDataService();
            if (request.Item.id <= 0)
            {
                Employeeincharges result = await Task<Employeeincharges>.Run(() => EmployeeinchargesData.Insert(request.Item));
                return this.Ok(result);
            }
            else
            {
                Employeeincharges result = await Task<Employeeincharges>.Run(() => EmployeeinchargesData.Update(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("SaveAll")]
        public async Task<IHttpActionResult> SaveAll(ActionMulitpleRequest<Employeeincharges> request)
        {
            EmployeeinchargesDataService EmployeeinchargesData = new EmployeeinchargesDataService();
            List<Employeeincharges> result = await Task<List<Employeeincharges>>.Run(() => EmployeeinchargesData.SaveAll(request.Items));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Insert")]
        public async Task<IHttpActionResult> Insert(ActionRequest<Employeeincharges> request)
        {
            EmployeeinchargesDataService EmployeeinchargesData = new EmployeeinchargesDataService();
            Employeeincharges result = await Task<Employeeincharges>.Run(() => EmployeeinchargesData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        public async Task<IHttpActionResult> Update(ActionRequest<Employeeincharges> request)
        {
            EmployeeinchargesDataService EmployeeinchargesData = new EmployeeinchargesDataService();
            Employeeincharges result = await Task<Employeeincharges>.Run(() => EmployeeinchargesData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IHttpActionResult> Delete(ActionRequest<Employeeincharges> request)
        {
            EmployeeinchargesDataService EmployeeinchargesData = new EmployeeinchargesDataService();
            bool result = await Task<bool>.Run(() => EmployeeinchargesData.Delete(request.Item));
            return this.Ok(result);
        }
    }
}