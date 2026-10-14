
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
    [RoutePrefix("SalaryConfig")]
    public class SalaryconfigController : ApiController
    {

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<Agent>();
            result.Item = new Agent();
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Search")]
        [WebAPI.InfyAuthorize(PermissionSet.agentmaster, InfyPOS.Common.Action.View)]
        public async Task<IHttpActionResult> Search(ActionRequest<AgentCreteria> request)
        {
            AgentDataService AgentData = new AgentDataService();
            List<Agent> result = await Task<List<Agent>>.Run(() => AgentData.Search(request.Item));
            return this.Ok(result);
        }

       
        [HttpPost]
        [Route("Save")]
        public async Task<IHttpActionResult> Save(ActionRequest<Configuration> request)
        {
            ConfigSalaryDataService ConfigSalaryData = new ConfigSalaryDataService();
            if (request.Item.id <= 0)
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Create);
                Configuration result = await Task<Configuration>.Run(() => ConfigSalaryData.Insert(request.Item));
                return this.Ok(result);
            }
            else
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Update);
                Configuration result = await Task<Configuration>.Run(() => ConfigSalaryData.Update(request.Item));
                return this.Ok(result);
            }
        }


        [HttpPost]
        [Route("SaveConfig")]

        public async Task<IHttpActionResult> SaveConfig(ActionRequest<Configuration> request)
        {
            ConfigurationDataService ConfigurationData = new ConfigurationDataService();
            if (request.Item.id <= 0)
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Create);
                Configuration result = await Task<Configuration>.Run(() => ConfigurationData.Insert(request.Item));
                return this.Ok(result);
            }
            else
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Update);
                Configuration result = await Task<Configuration>.Run(() => ConfigurationData.Update(request.Item));
                return this.Ok(result);
            }
        }


        [HttpPost]
        [Route("Insert")]
        [WebAPI.InfyAuthorize(PermissionSet.agentmaster, InfyPOS.Common.Action.Create)]
        public async Task<IHttpActionResult> Insert(ActionRequest<Agent> request)
        {
            AgentDataService AgentData = new AgentDataService();
            Agent result = await Task<Agent>.Run(() => AgentData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        [WebAPI.InfyAuthorize(PermissionSet.agentmaster, InfyPOS.Common.Action.Modify)]
        public async Task<IHttpActionResult> Update(ActionRequest<Agent> request)
        {
            AgentDataService AgentData = new AgentDataService();
            Agent result = await Task<Agent>.Run(() => AgentData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Generate")]
        public async Task<IHttpActionResult> Generate(ActionRequest<EmployeesalaryCreteria> request)
        {
            try
            {
                ConfigSalaryDataService EmployeesalaryData = new ConfigSalaryDataService();
                List<EmployeeConfigsalary> result = await Task.Run(() => EmployeesalaryData.Generate(request.Item));
                return this.Ok(result);
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance.Log(exp, WebAPI.Common.Logger.LogType.Error, request);
                return this.BadRequest(exp.ToString());
            }
        }



        [HttpPost]
        [Route("Delete")]
        [WebAPI.InfyAuthorize(PermissionSet.agentmaster, InfyPOS.Common.Action.Delete)]
        public async Task<IHttpActionResult> Delete(ActionRequest<Agent> request)
        {
            AgentDataService AgentData = new AgentDataService();
            bool result = await Task<bool>.Run(() => AgentData.Delete(request.Item));
            return this.Ok(result);
        }
    }
}