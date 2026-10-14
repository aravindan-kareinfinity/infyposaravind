
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
    [RoutePrefix("EmployeetargetService")]
    public class EmployeetargetServiceController : ApiController
    {

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<Employeetarget>();
            result.Item = new Employeetarget();
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Incentive")]
        public async Task<IHttpActionResult> Incentive(ActionRequest<EmployeetargetCreteria> request)
        {
            EmployeetargetDataService EmployeetargetData = new EmployeetargetDataService();
            List<EmployeetargetDetail> result = await Task<List<EmployeetargetDetail>>.Run(() => EmployeetargetData.Incentive(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("SearchAll")]
        public async Task<IHttpActionResult> Search(ActionRequest<EmployeetargetCreteria> request)
        {
            EmployeetargetDataService EmployeetargetData = new EmployeetargetDataService();
            List<Employeetarget> result = await Task<List<Employeetarget>>.Run(() => EmployeetargetData.Search(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("SaveAll")]
        public async Task<IHttpActionResult> SaveAll(ActionMulitpleRequest<Employeetarget> request)
        {
            EmployeetargetDataService EmployeetargetData = new EmployeetargetDataService();
            List<Employeetarget> result = await Task<List<Employeetarget>>.Run(() => EmployeetargetData.SaveAll(request.Items));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        public async Task<IHttpActionResult> Update(ActionRequest<Employeetarget> request)
        {
            EmployeetargetDataService EmployeetargetData = new EmployeetargetDataService();
            Employeetarget result = await Task<Employeetarget>.Run(() => EmployeetargetData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Build")]
        public async Task<IHttpActionResult> Build(ActionRequest<EmployeetargetCreteria> request)
        {
            EmployeetargetDataService EmployeetargetData = new EmployeetargetDataService();
            if (request.Item.locationid == -9)
            {
                List<EmployeetargetDetail> result = await Task<List<EmployeetargetDetail>>.Run(() => EmployeetargetData.Build4Locations(request.Item));
                return this.Ok(result);
            }
            else if (request.Item.locationid == -10)
            {
                List<EmployeetargetDetail> result = await Task<List<EmployeetargetDetail>>.Run(() => EmployeetargetData.Build4Section(request.Item));
                return this.Ok(result);
            }
            else
            {
                List<EmployeetargetDetail> result = await Task<List<EmployeetargetDetail>>.Run(() => EmployeetargetData.Build(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IHttpActionResult> Delete(ActionMulitpleRequest<Employeetarget> request)
        {
            EmployeetargetDataService EmployeetargetData = new EmployeetargetDataService();
            bool result = await Task<bool>.Run(() => EmployeetargetData.Delete(request.Items));
            return this.Ok(result);
        }
    }
}