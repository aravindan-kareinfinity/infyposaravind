
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
    [RoutePrefix("HolidayService")]
    public class HolidayServiceController : ApiController
    {

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<Holiday>();
            result.Item = new Holiday();
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Search")]

        public async Task<IHttpActionResult> Search(ActionRequest<HolidayCreteria> request)
        {
            HolidayDataService HolidayData = new HolidayDataService();
            List<Holiday> result = await Task<List<Holiday>>.Run(() => HolidayData.Search(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Save")]

        public async Task<IHttpActionResult> Save(ActionRequest<Holiday> request)
        {
            HolidayDataService HolidayData = new HolidayDataService();
            if (request.Item.id <= 0)
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Create);
                Holiday result = await Task<Holiday>.Run(() => HolidayData.Insert(request.Item));
                return this.Ok(result);
            }
            else
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Update);
                Holiday result = await Task<Holiday>.Run(() => HolidayData.Update(request.Item));
                return this.Ok(result);
            }
        }


        [HttpPost]
        [Route("Clone")]

        public async Task<IHttpActionResult> Clone(ActionRequest<HolidayCreteria> request)
        {
            HolidayDataService HolidayData = new HolidayDataService();
            int result = await Task<int>.Run(() => HolidayData.Clone(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Insert")]

        public async Task<IHttpActionResult> Insert(ActionRequest<Holiday> request)
        {
            HolidayDataService HolidayData = new HolidayDataService();
            Holiday result = await Task<Holiday>.Run(() => HolidayData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]

        public async Task<IHttpActionResult> Update(ActionRequest<Holiday> request)
        {
            HolidayDataService HolidayData = new HolidayDataService();
            Holiday result = await Task<Holiday>.Run(() => HolidayData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]

        public async Task<IHttpActionResult> Delete(ActionRequest<Holiday> request)
        {
            HolidayDataService HolidayData = new HolidayDataService();
            bool result = await Task<bool>.Run(() => HolidayData.Delete(request.Item));
            return this.Ok(result);
        }
    }
}