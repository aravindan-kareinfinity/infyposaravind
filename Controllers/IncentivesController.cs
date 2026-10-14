
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
using System.Web;

namespace InfyPOS.Controllers
{
    [RoutePrefix("IncentivesService")]
    public class IncentivesServiceController : ApiController
    {

        [HttpPost]
        [Route("GenerateLeave")]
        public async Task<IHttpActionResult> GenerateLeave(ActionRequest<IncentivesCreteria> request)
        {
            try
            {
                IncentivesDataService IncentivesData = new IncentivesDataService();
                List<Incentives.LeaveData> result = await Task.Run(() => IncentivesData.GenerateLeave(request.Item));
                return this.Ok(result);
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(new BaseData().Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, request);
                return this.BadRequest(exp.ToString());
            }
        }

        [HttpPost]
        [Route("MfgIncentive")]
        public async Task<IHttpActionResult> MfgIncentive(ActionRequest<IncentivesCreteria> request)
        {
            try
            {
                //if (!ValidateLocation(request.Item.locationid))
                //    return this.NotFound();
                IncentivesDataService IncentivesData = new IncentivesDataService();
                List<Payroll.IncentiveSalary.IncentiveData.MfgIncentive> result = await Task.Run(() => IncentivesData.MfgIncentive(request.Item));
                return this.Ok(result);
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(new BaseData().Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, request);
                return this.BadRequest(exp.ToString());
            }
        }

        [HttpPost]
        [Route("GenerateIncentive")]
        public async Task<IHttpActionResult> GenerateIncentive(ActionRequest<IncentivesCreteria> request)
        {
            try
            {
                //if (!ValidateLocation(request.Item.locationid))
                //    return this.NotFound();
                IncentivesDataService IncentivesData = new IncentivesDataService();
                List<Payroll.IncentiveSalary.IncentiveData.Item> result = await Task.Run(() => IncentivesData.GenerateIncentive(request.Item));
                return this.Ok(result);
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(new BaseData().Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, request);
                return this.BadRequest(exp.ToString());
            }
        }
        bool ValidateLocation(long locationid)
        {
            if (locationid > 0) return true;
            if (System.Web.HttpContext.Current != null)
            {
                var Context = new UserClaim(System.Web.HttpContext.Current.Request.GetOwinContext().Authentication);
                if (Context.accessiblelocation == null || !Context.accessiblelocation.Contains(-1))
                    return false;
            }
            return true;
        }

        [HttpPost]
        [Route("GenerateBonus")]
        public async Task<IHttpActionResult> GenerateBonus(ActionRequest<IncentivesCreteria> request)
        {
            try
            {
                IncentivesDataService IncentivesData = new IncentivesDataService();
                List<InfyPOS.Payroll.Bonus.BounsData> result = await Task.Run(() => IncentivesData.GenerateBonus(request.Item));
                return this.Ok(result);

            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(new BaseData().Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, request);
                return this.BadRequest(exp.ToString());
            }
        }

        [HttpPost]
        [Route("GenerateFine")]
        public async Task<IHttpActionResult> GenerateFine(ActionRequest<Payroll.FineCalulation.FineCreteria> request)
        {
            try
            {
                InfyPOS.Payroll.FineCalulation IncentivesData = new Payroll.FineCalulation();
                List <InfyPOS.Payroll.FineCalulation.FineCalulationData> result = await Task.Run(() => IncentivesData.Compute(request.Item));
                return this.Ok(result);
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(new BaseData().Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, request);
                return this.BadRequest(exp.ToString());
            }
        }

        [HttpPost]
        [Route("GenerateBeta")]
        public async Task<IHttpActionResult> GenerateBeta(ActionRequest<IncentivesCreteria> request)
        {
            try
            {
                IncentivesDataService IncentivesData = new IncentivesDataService();
                List<InfyPOS.Payroll.Beta.BetaData> result = await Task.Run(() => IncentivesData.GenerateBeta(request.Item));
                return this.Ok(result);

            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(new BaseData().Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, request);
                return this.BadRequest(exp.ToString());
            }
        }

        [HttpPost]
        [Route("GenerateOT")]
        public async Task<IHttpActionResult> GenerateOT(ActionRequest<IncentivesCreteria> request)
        {
            try
            {
                IncentivesDataService IncentivesData = new IncentivesDataService();
                List<InfyPOS.Payroll.Overtime.OvertimeData> result = await Task.Run(() => IncentivesData.GenerateOT(request.Item));
                return this.Ok(result);
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(new BaseData().Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, request);
                return this.BadRequest(exp.ToString());
            }
        }

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<Incentives>();
            result.Item = new Incentives();
            return this.Ok(result);
        }

        public List<InfyPOS.Payroll.IncentiveSalary.IncentiveData.Item> ConverToIncentiveList(long departmentid, List<InfyPOS.Payroll.IncentiveSalary.IncentiveData> source)
        {
            var result = new List<InfyPOS.Payroll.IncentiveSalary.IncentiveData.Item>();
            foreach (var item in source)
            {
                if (item.departmentid != departmentid && departmentid != 0) continue;
                result.AddRange(item.Employees);
                item.Employees.ForEach(e => e.departmentid = item.departmentid);
            }
            return result;
        }

        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<IncentivesCreteria> request)
        {
            IncentivesDataService IncentivesData = new IncentivesDataService();
            if(request.Item.datatype == 0)
            {
                List<Incentives.SummaryData> resultsummary = await Task<List<Incentives.SummaryData>>.Run(() => IncentivesData.SearchSummary(request.Item));
                return this.Ok(resultsummary);

            }
            List<Incentives> result = await Task<List<Incentives>>.Run(() => IncentivesData.Search(request.Item));

            if (result.Count == 1)
            {
                switch (result[0].CalculationType)
                {
                    case Incentives.Calculationtype.beta:
                        return this.Ok(Newtonsoft.Json.JsonConvert.DeserializeObject<List<Payroll.Beta.BetaData>>(result[0].data_json));
                    case Incentives.Calculationtype.bonus:
                        return this.Ok(Newtonsoft.Json.JsonConvert.DeserializeObject<List<Payroll.Bonus.BounsData>>(result[0].data_json));
                    case Incentives.Calculationtype.incentive:
                        var results = Newtonsoft.Json.JsonConvert.DeserializeObject<List<InfyPOS.Payroll.IncentiveSalary.IncentiveData>>(result[0].data_json);
                        return this.Ok(ConverToIncentiveList(request.Item.departmentid,results));
                    case Incentives.Calculationtype.leavesalary:
                        return this.Ok(Newtonsoft.Json.JsonConvert.DeserializeObject<List<InfyPOS.Payroll.LeaveSalary>>(result[0].data_json));
                    case Incentives.Calculationtype.overtime:
                        return this.Ok(Newtonsoft.Json.JsonConvert.DeserializeObject<List<Payroll.Overtime.OvertimeData>>(result[0].data_json));
                }
            }
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Save")]
        public async Task<IHttpActionResult> Save(ActionRequest<Incentives> request)
        {
            IncentivesDataService IncentivesData = new IncentivesDataService();
            if (request.Item.id <= 0)
            {
                Incentives result = await Task<Incentives>.Run(() => IncentivesData.Insert(request.Item));
                return this.Ok(result);
            }
            else
            {
                Incentives result = await Task<Incentives>.Run(() => IncentivesData.Update(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("Insert")]
        public async Task<IHttpActionResult> Insert(ActionRequest<Incentives> request)
        {
            IncentivesDataService IncentivesData = new IncentivesDataService();
            Incentives result = await Task<Incentives>.Run(() => IncentivesData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        public async Task<IHttpActionResult> Update(ActionRequest<Incentives> request)
        {
            IncentivesDataService IncentivesData = new IncentivesDataService();
            Incentives result = await Task<Incentives>.Run(() => IncentivesData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IHttpActionResult> Delete(ActionRequest<Incentives> request)
        {
            IncentivesDataService IncentivesData = new IncentivesDataService();
            bool result = await Task<bool>.Run(() => IncentivesData.Delete(request.Item));
            return this.Ok(result);
        }
    }
}