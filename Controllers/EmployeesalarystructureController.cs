
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
    [RoutePrefix("EmployeesalarystructureService")]
    public class EmployeesalarystructureServiceController : ApiController
    {

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<Employeesalarystructure>();
            result.Item = new Employeesalarystructure();
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<EmployeesalarystructureCreteria> request)
        {
            EmployeesalarystructureDataService EmployeesalarystructureData = new EmployeesalarystructureDataService();
            List<Employeesalarystructure> result = await Task<List<Employeesalarystructure>>.Run(() => EmployeesalarystructureData.Search(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Save")]
        public async Task<IHttpActionResult> Save(ActionRequest<Employeesalarystructure> request)
        {
            EmployeesalarystructureDataService EmployeesalarystructureData = new EmployeesalarystructureDataService();
            if (request.Item.id <= 0)
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Create);
                Employeesalarystructure result = await Task<Employeesalarystructure>.Run(() => EmployeesalarystructureData.Insert(request.Item));
                return this.Ok(result);
            }
            else
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Update);
                Employeesalarystructure result = await Task<Employeesalarystructure>.Run(() => EmployeesalarystructureData.Update(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("Replace")]
        public async Task<IHttpActionResult> Replace(ActionRequest<EmployeesalarystructureReplace> request)
        {
            EmployeesalarystructureDataService EmployeesalarystructureData = new EmployeesalarystructureDataService();
            
               
                EmployeesalarystructureReplace result = await Task<EmployeesalarystructureReplace>.Run(() => EmployeesalarystructureData.Replace(request.Item));
                return this.Ok(result);
           
        }

        [HttpPost]
        [Route("Insert")]
        public async Task<IHttpActionResult> Insert(ActionRequest<Employeesalarystructure> request)
        {
            EmployeesalarystructureDataService EmployeesalarystructureData = new EmployeesalarystructureDataService();
            Employeesalarystructure result = await Task<Employeesalarystructure>.Run(() => EmployeesalarystructureData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        public async Task<IHttpActionResult> Update(ActionRequest<Employeesalarystructure> request)
        {
            EmployeesalarystructureDataService EmployeesalarystructureData = new EmployeesalarystructureDataService();
            Employeesalarystructure result = await Task<Employeesalarystructure>.Run(() => EmployeesalarystructureData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IHttpActionResult> Delete(ActionRequest<Employeesalarystructure> request)
        {
            EmployeesalarystructureDataService EmployeesalarystructureData = new EmployeesalarystructureDataService();
            bool result = await Task<bool>.Run(() => EmployeesalarystructureData.Delete(request.Item));
            return this.Ok(result);
        }
    }
}