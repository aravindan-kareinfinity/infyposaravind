
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
    [RoutePrefix("EmployeesalaryheaderService")]
    public class EmployeesalaryheaderServiceController : ApiController
    {

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<Employeesalaryheader>();
            result.Item = new Employeesalaryheader();
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<EmployeesalaryheaderCreteria> request)
        {
            EmployeesalaryheaderDataService EmployeesalaryheaderData = new EmployeesalaryheaderDataService();
            List<Employeesalaryheader> result = await Task<List<Employeesalaryheader>>.Run(() => EmployeesalaryheaderData.Search(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("SearchAll")]
        public async Task<IHttpActionResult> SearchAll(ActionRequest<EmployeesalaryheaderCreteria> request)
        {
            EmployeesalaryheaderDataService EmployeesalaryheaderData = new EmployeesalaryheaderDataService();
            List<EmployeeSalaryInfo> result = await Task<List<EmployeeSalaryInfo>>.Run(() => EmployeesalaryheaderData.SearchALL(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Save")]
        public async Task<IHttpActionResult> Save(ActionRequest<Employeesalaryheader> request)
        {
            if (request.Item == null)
                return this.NotFound();

            EmployeesalaryheaderDataService EmployeesalaryheaderData = new EmployeesalaryheaderDataService();
            if (request.Item.id <= 0)
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Create);
                Employeesalaryheader result = await Task<Employeesalaryheader>.Run(() => EmployeesalaryheaderData.Insert(request.Item));
                return this.Ok(result);
            }
            else
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Update);
                Employeesalaryheader result = await Task<Employeesalaryheader>.Run(() => EmployeesalaryheaderData.Update(request.Item));
                return this.Ok(result);
            }
        }


        [HttpPost]
        [Route("SaveIncentiveConfig")]
        public async Task<IHttpActionResult> SaveIncentiveConfig(ActionRequest<IncentiveConfig> request)
        {
            EmployeesalaryheaderDataService EmployeesalaryheaderData = new EmployeesalaryheaderDataService();
            var result = await Task<bool>.Run(() => EmployeesalaryheaderData.SaveIncentiveConfig(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("SaveESIPFConfig")]
        public async Task<IHttpActionResult> SaveESIPFConfig(ActionRequest<IncentiveConfig> request)
        {
            EmployeesalaryheaderDataService EmployeesalaryheaderData = new EmployeesalaryheaderDataService();
            var result = await Task<bool>.Run(() => EmployeesalaryheaderData.SaveESIPFConfig(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("SaveAll")]
        public async Task<IHttpActionResult> SaveAll(ActionMulitpleRequest<EmployeeSalaryInfo> request)
        {
      
            EmployeesalaryheaderDataService EmployeesalaryheaderData = new EmployeesalaryheaderDataService();

            var result = await Task<List<EmployeeSalaryInfo>>.Run(() => EmployeesalaryheaderData.SaveAll(request.Items));
            return this.Ok(result);

        }

        [HttpPost]
        [Route("Insert")]
        public async Task<IHttpActionResult> Insert(ActionRequest<Employeesalaryheader> request)
        {
            EmployeesalaryheaderDataService EmployeesalaryheaderData = new EmployeesalaryheaderDataService();
            Employeesalaryheader result = await Task<Employeesalaryheader>.Run(() => EmployeesalaryheaderData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        public async Task<IHttpActionResult> Update(ActionRequest<Employeesalaryheader> request)
        {
            EmployeesalaryheaderDataService EmployeesalaryheaderData = new EmployeesalaryheaderDataService();
            Employeesalaryheader result = await Task<Employeesalaryheader>.Run(() => EmployeesalaryheaderData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IHttpActionResult> Delete(ActionRequest<Employeesalaryheader> request)
        {
            EmployeesalaryheaderDataService EmployeesalaryheaderData = new EmployeesalaryheaderDataService();
            bool result = await Task<bool>.Run(() => EmployeesalaryheaderData.Delete(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("MultipleDelete")]
        public async Task<IHttpActionResult> MultipleDelete(ActionRequest<Employeesalaryheader> request)
        {
            EmployeesalaryheaderDataService EmployeesalaryheaderData = new EmployeesalaryheaderDataService();
            int result = await Task<int>.Run(() => EmployeesalaryheaderData.MultipleDelete(request.Item));
            return this.Ok(result);
        }
    }
}