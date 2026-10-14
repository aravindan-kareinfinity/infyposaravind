
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
    [RoutePrefix("EmployeesalaryService")]
    public class EmployeesalaryServiceController : ApiController
    {

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<Employeesalary>();
            result.Item = new Employeesalary();
            return this.Ok(result);
        }


        [HttpPost]
        [Route("LoadSchedule")]
        public async Task<IHttpActionResult> LoadSchedule(ActionRequest<EmployeeSalaryHeaderSchedule> request)
        {
            try
            {
                EmployeesalaryDataService EmployeesalaryData = new EmployeesalaryDataService();
                EmployeeSalaryHeaderSchedule result = await Task.Run(() => EmployeesalaryData.LoadSchedule(request.Item));
                return this.Ok(result);
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(0).Log(exp, WebAPI.Common.Logger.LogType.Error, request);
                return this.BadRequest(exp.ToString());
            }
        }


        [HttpPost]
        [Route("SaveSchedule")]
        public async Task<IHttpActionResult> SaveSchedule(ActionRequest<EmployeeSalaryHeaderSchedule> request)
        {
            try
            {
                EmployeesalaryDataService EmployeesalaryData = new EmployeesalaryDataService();
                EmployeeSalaryHeaderSchedule result = await Task.Run(() => EmployeesalaryData.SaveSchedule(request.Item));
                return this.Ok(result);
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(0).Log(exp, WebAPI.Common.Logger.LogType.Error, request);
                return this.BadRequest(exp.ToString());
            }
        }


        [HttpPost]
        [Route("GenerateCustom")]
        public async Task<IHttpActionResult> GenerateCustom(ActionRequest<EmployeeSalaryConfig> request)
        {
            try
            {
                EmployeesalaryDataService EmployeesalaryData = new EmployeesalaryDataService();
                request.Item.fromdate = request.Item.fromdate.Date;
                request.Item.todate = request.Item.todate.Date;
                request.Item.generatefrom = request.Item.generatefrom.Date;
                request.Item.generateto = request.Item.generateto.Date;
                List<Employeesalary> result = await Task.Run(() => EmployeesalaryData.GenerateCustom(request.Item));
                return this.Ok(result);
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(0).Log(exp, WebAPI.Common.Logger.LogType.Error, request);
                return this.BadRequest(exp.ToString());
            }
        }

        [HttpPost]
        [Route("Generate")]
        public async Task<IHttpActionResult> Generate(ActionRequest<EmployeesalaryCreteria> request)
        {
            try
            {
                EmployeesalaryDataService EmployeesalaryData = new EmployeesalaryDataService();
                List<Employeesalary> result = await Task.Run(() => EmployeesalaryData.Generate(request.Item));
                return this.Ok(result);
            }catch(Exception exp)
            {
                WebAPI.Common.Logger.Instance(new BaseData().Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, request);
                return this.BadRequest(exp.ToString());
            }
        }

        [HttpPost]
        [Route("ESIPFReport")]
        public async Task<IHttpActionResult> ESIPFReport(ActionRequest<Payroll.ESIRequest> request)
        {
            try
            {

                InfyPOS.Payroll.ESIPF EmployeesalaryData = new InfyPOS.Payroll.ESIPF();
                InfyPOS.Payroll.ESIReport result = await Task.Run(() => EmployeesalaryData.Compute(request.Item));
                return this.Ok(result);
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(new BaseData().Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, request);
                return this.BadRequest(exp.ToString());
            }
        }
            
        [HttpPost]
        [Route("GenerateDailywages")]
        public async Task<IHttpActionResult> GenerateDailywages(ActionRequest<InfyPOS.Models.EmployeesalaryCreteria> request)
        {
            try
            {
                InfyPOS.Payroll.SalarySummary EmployeesalaryData = new InfyPOS.Payroll.SalarySummary();
                List<InfyPOS.Payroll.SalarySummary.DailySalary> result = await Task.Run(() => EmployeesalaryData.GenerateDailywages(request.Item));
                return this.Ok(result);
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(new BaseData().Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, request);
                return this.BadRequest(exp.ToString());
            }
        }

        [HttpPost]
        [Route("SearchDailyWages")]
        public async Task<IHttpActionResult> SearchDailyWages(ActionRequest<InfyPOS.Models.EmployeesalaryCreteria> request)
        {
            try
            {
                InfyPOS.Payroll.SalarySummary EmployeesalaryData = new InfyPOS.Payroll.SalarySummary();
                List<InfyPOS.Payroll.SalarySummary.DailySalary> result = await Task.Run(() => EmployeesalaryData.SearchDailywages(request.Item));
                return this.Ok(result);
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(new BaseData().Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, request);
                return this.BadRequest(exp.ToString());
            }
        }


        [HttpPost]
        [Route("SalarySummary")]
        public async Task<IHttpActionResult> SalarySummary(ActionRequest<Payroll.SalarySummaryRequest> request)
        {
            try
            {
                InfyPOS.Payroll.SalarySummary EmployeesalaryData = new InfyPOS.Payroll.SalarySummary();
                List<InfyPOS.Payroll.SalarySummary.Item> result = await Task.Run(() => EmployeesalaryData.Search(request.Item));
                return this.Ok(result);
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(new BaseData().Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, request);
                return this.BadRequest(exp.ToString());
            }
        }

        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<EmployeesalaryCreteria> request)
        {
            EmployeesalaryDataService EmployeesalaryData = new EmployeesalaryDataService();
            List<Employeesalary> result = await Task<List<Employeesalary>>.Run(() => EmployeesalaryData.Search(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("UpdateDetail")]
        public async Task<IHttpActionResult> UpdateDetail(ActionMulitpleRequest<EmployeeSalaryUpdation> request)
        {
            EmployeesalaryDataService EmployeesalaryData = new EmployeesalaryDataService();
            var result = EmployeesalaryData.UpdateDetail(request.Items);
            return this.Ok(true);
        }

        [HttpPost]
        [Route("Save")]
        public async Task<IHttpActionResult> Save(ActionRequest<Employeesalary> request)
        {
            EmployeesalaryDataService EmployeesalaryData = new EmployeesalaryDataService();
            if (request.Item.id <= 0)
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Create);
                Employeesalary result = await Task<Employeesalary>.Run(() => EmployeesalaryData.Insert(request.Item));
                return this.Ok(result);
            }
            else
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Update);
                Employeesalary result = await Task<Employeesalary>.Run(() => EmployeesalaryData.Update(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("Insert")]
        public async Task<IHttpActionResult> Insert(ActionRequest<Employeesalary> request)
        {
            EmployeesalaryDataService EmployeesalaryData = new EmployeesalaryDataService();
            Employeesalary result = await Task<Employeesalary>.Run(() => EmployeesalaryData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        public async Task<IHttpActionResult> Update(ActionRequest<Employeesalary> request)
        {
            EmployeesalaryDataService EmployeesalaryData = new EmployeesalaryDataService();
            Employeesalary result = await Task<Employeesalary>.Run(() => EmployeesalaryData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IHttpActionResult> Delete(ActionRequest<Employeesalary> request)
        {
            EmployeesalaryDataService EmployeesalaryData = new EmployeesalaryDataService();
            bool result = await Task<bool>.Run(() => EmployeesalaryData.Delete(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Searchbank")]
        public async Task<IHttpActionResult> Searchbank(ActionRequest<EmployeeBankCriteria> request)
        {
            EmployeesalaryDataService EmployeeBankData = new EmployeesalaryDataService();
            List<EmployeeBankreport> result = await Task<List<EmployeeBankreport>>.Run(() => EmployeeBankData.SearchBankReport(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("SearchHRReports")]
        public async Task<IHttpActionResult> SearchHRReports(ActionRequest<EmployeesalaryDataService.HRReportCreteria> request)
        {
            EmployeesalaryDataService EmployeesalaryData = new EmployeesalaryDataService();
            List<EmployeesalaryDataService.HRReport> result = await Task<List<EmployeesalaryDataService.HRReport>>.Run(() => EmployeesalaryData.SearchHRReports(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("SearchHoldsalary")]
        public async Task<IHttpActionResult> SearchHoldsalary(ActionRequest<EmployeesalaryDataService.HRReportCreteria> request)
        {
            EmployeesalaryDataService EmployeesalaryData = new EmployeesalaryDataService();
            List<EmployeesalaryDataService.HRReport> result = await Task<List<EmployeesalaryDataService.HRReport>>.Run(() => EmployeesalaryData.SearchHoldsalary(request.Item));
            return this.Ok(result);
        }

        //[HttpPost]
        //[Route("SearchJoiningday")]
        //public async Task<IHttpActionResult> SearchJoiningday(ActionRequest<EmployeesalaryDataService.HRReportCreteria> request)
        //{
        //    EmployeesalaryDataService EmployeesalaryData = new EmployeesalaryDataService();
        //    List<EmployeesalaryDataService.HRReport> result = await Task<List<EmployeesalaryDataService.HRReport>>.Run(() => EmployeesalaryData.SearchJoiningday(request.Item));
        //    return this.Ok(result);
        //}
        //[HttpPost]
        //[Route("SearchServiceCompletion")]
        //public async Task<IHttpActionResult> SearchServiceCompletion(ActionRequest<EmployeesalaryDataService.HRReportCreteria> request)
        //{
        //    EmployeesalaryDataService EmployeesalaryData = new EmployeesalaryDataService();
        //    List<EmployeesalaryDataService.HRReport> result = await Task<List<EmployeesalaryDataService.HRReport>>.Run(() => EmployeesalaryData.SearchServiceCompletion(request.Item));
        //    return this.Ok(result);
        //}
        [HttpPost]
        [Route("HRDashboard")]
        [WebAPI.InfyAuthorize(PermissionSet.rpthrdashboard, InfyPOS.Common.Action.View)]
        public async Task<IHttpActionResult> HRDashboard(ActionRequest<EmployeesalaryDataService.HRReportCreteria> request)
        {
            try
            {
                EmployeesalaryDataService EmployeesalaryData = new EmployeesalaryDataService();
                EmployeesalaryDataService.HRDashboard result = await Task<EmployeesalaryDataService.HRDashboard>.Run(() => EmployeesalaryData.Dashboard(request.Item));
                return this.Ok(result);
            }catch(Exception exp)
            {
                WebAPI.Common.Logger.Instance(new BaseData().Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, request);
                return this.InternalServerError();
            }
        }
    }
}