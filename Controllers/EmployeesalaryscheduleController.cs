
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
    [RoutePrefix("EmployeesalaryscheduleService")]
    public class EmployeesalaryscheduleServiceController : ApiController
    {

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<Employeesalaryschedule>();
            result.Item = new Employeesalaryschedule();
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<EmployeesalaryscheduleCreteria> request)
        {
            EmployeesalaryscheduleDataService EmployeesalaryscheduleData = new EmployeesalaryscheduleDataService();
            List<Employeesalaryschedule> result = await Task<List<Employeesalaryschedule>>.Run(() => EmployeesalaryscheduleData.Search(request.Item));

            return this.Ok(result);
        }

        [HttpPost]
        [Route("SaveAdvance")]
        public async Task<IHttpActionResult> SaveAdvance(ActionRequest<EmployeeAdvance> request)
        {
            EmployeesalaryscheduleDataService EmployeesalaryscheduleData = new EmployeesalaryscheduleDataService();
            if (request.Item.searchmode == 3)
            {
                List<Employeesalaryschedule> result = await Task<List<Employeesalaryschedule>>.Run(() => EmployeesalaryscheduleData.SaveAdvance(request.Item));
                return this.Ok(result);
            }
            else
            {
                EmployeeAdvance result = await Task<EmployeeAdvance>.Run(() => EmployeesalaryscheduleData.UpdateAdvance(request.Item));
                return this.Ok(result);
            }

        }

        [HttpPost]
        [Route("Save")]
        public async Task<IHttpActionResult> Save(ActionRequest<Employeesalaryschedule> request)
        {
            EmployeesalaryscheduleDataService EmployeesalaryscheduleData = new EmployeesalaryscheduleDataService();
            if (request.Item.id <= 0)
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Create);
                Employeesalaryschedule result = await Task<Employeesalaryschedule>.Run(() => EmployeesalaryscheduleData.Insert(request.Item));
                return this.Ok(result);
            }
            else
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Update);
                Employeesalaryschedule result = await Task<Employeesalaryschedule>.Run(() => EmployeesalaryscheduleData.Update(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("Insert")]
        public async Task<IHttpActionResult> Insert(ActionRequest<Employeesalaryschedule> request)
        {
            EmployeesalaryscheduleDataService EmployeesalaryscheduleData = new EmployeesalaryscheduleDataService();
            Employeesalaryschedule result = await Task<Employeesalaryschedule>.Run(() => EmployeesalaryscheduleData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        public async Task<IHttpActionResult> Update(ActionRequest<Employeesalaryschedule> request)
        {
            EmployeesalaryscheduleDataService EmployeesalaryscheduleData = new EmployeesalaryscheduleDataService();
            Employeesalaryschedule result = await Task<Employeesalaryschedule>.Run(() => EmployeesalaryscheduleData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("DeleteAll")]
        public async Task<IHttpActionResult> DeleteAll(ActionMulitpleRequest<Employeesalaryschedule> request)
        {
            EmployeesalaryscheduleDataService EmployeesalaryscheduleData = new EmployeesalaryscheduleDataService();
            bool result = await Task<bool>.Run(() => EmployeesalaryscheduleData.DeleteAll(request.Items));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IHttpActionResult> Delete(ActionRequest<Employeesalaryschedule> request)
        {
            EmployeesalaryscheduleDataService EmployeesalaryscheduleData = new EmployeesalaryscheduleDataService();
            bool result = await Task<bool>.Run(() => EmployeesalaryscheduleData.Delete(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("SearchAdvanceSummary")]
        public async Task<IHttpActionResult> SearchAdvanceSummary(ActionRequest<EmployeesalaryscheduleCreteria> request)
        {
            EmployeesalaryscheduleDataService EmployeesalaryscheduleData = new EmployeesalaryscheduleDataService();
            List<EmployeesalaryscheduleDetails> result = await Task<List<EmployeesalaryscheduleDetails>>.Run(() => EmployeesalaryscheduleData.SearchAdvanceSummary(request.Item));

            return this.Ok(result);
        }

        [HttpPost]
        [Route("SearchAdvanceDetails")]
        public async Task<IHttpActionResult> SearchAdvanceDetail(ActionRequest<EmployeesalaryscheduleCreteria> request)
        {
            EmployeesalaryscheduleDataService EmployeesalaryscheduleData = new EmployeesalaryscheduleDataService();
            EmployeeScheduleDetail result = await Task<EmployeeScheduleDetail>.Run(() => EmployeesalaryscheduleData.SearchAdvanceDetail(request.Item));

            return this.Ok(result);
        }

    }
}