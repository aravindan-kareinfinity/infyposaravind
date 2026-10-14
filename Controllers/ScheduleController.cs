
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
    [RoutePrefix("ScheduleService")]
    public class ScheduleServiceController : ApiController
    {

        [HttpGet]
        [Route("TaskList")]
        public async Task<IHttpActionResult> TaskList()
        {
            return this.Ok(InfyPOS.Processors.ScheduleManager.Instance.TaskList());
        }

        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<ScheduleCreteria> request)
        {
            ScheduleDataService ScheduleData = new ScheduleDataService();
            List<Schedule> result = await Task<List<Schedule>>.Run(() => ScheduleData.Search(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Save")]
        public async Task<IHttpActionResult> Save(ActionRequest<Schedule> request)
        {
            ScheduleDataService ScheduleData = new ScheduleDataService();
            if (request.Item.id <= 0)
            {
                Schedule result = await Task<Schedule>.Run(() => ScheduleData.Insert(request.Item));
                return this.Ok(result);
            }
            else
            {
                Schedule result = await Task<Schedule>.Run(() => ScheduleData.Update(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("Insert")]
        public async Task<IHttpActionResult> Insert(ActionRequest<Schedule> request)
        {
            ScheduleDataService ScheduleData = new ScheduleDataService();
            Schedule result = await Task<Schedule>.Run(() => ScheduleData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        public async Task<IHttpActionResult> Update(ActionRequest<Schedule> request)
        {
            ScheduleDataService ScheduleData = new ScheduleDataService();
            Schedule result = await Task<Schedule>.Run(() => ScheduleData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IHttpActionResult> Delete(ActionRequest<Schedule> request)
        {
            ScheduleDataService ScheduleData = new ScheduleDataService();
            bool result = await Task<bool>.Run(() => ScheduleData.Delete(request.Item));
            return this.Ok(result);
        }
    }
}