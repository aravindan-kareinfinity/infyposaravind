
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
    [RoutePrefix("HomepageService")]
    public class HomepageServiceController : ApiController
    {

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<Homepage>();
            result.Item = new Homepage();
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Search")]
        [AllowAnonymous]
        public async Task<IHttpActionResult> Search(ActionRequest<HomepageCreteria> request)
        {
            HomepageDataService HomepageData = new HomepageDataService();
            List<Homepage> result = await Task<List<Homepage>>.Run(() => HomepageData.Search(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Save")]
        public async Task<IHttpActionResult> Save(ActionRequest<Homepage> request)
        {
            HomepageDataService HomepageData = new HomepageDataService();
            if (request.Item.id <= 0)
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Create);
                Homepage result = await Task<Homepage>.Run(() => HomepageData.Insert(request.Item));
                return this.Ok(result);
            }
            else
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Update);
                Homepage result = await Task<Homepage>.Run(() => HomepageData.Update(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("Insert")]
        public async Task<IHttpActionResult> Insert(ActionRequest<Homepage> request)
        {
            HomepageDataService HomepageData = new HomepageDataService();
            Homepage result = await Task<Homepage>.Run(() => HomepageData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        public async Task<IHttpActionResult> Update(ActionRequest<Homepage> request)
        {
            HomepageDataService HomepageData = new HomepageDataService();
            Homepage result = await Task<Homepage>.Run(() => HomepageData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]

        public async Task<IHttpActionResult> Delete(ActionRequest<Homepage> request)
        {
            HomepageDataService HomepageData = new HomepageDataService();
            bool result = await Task<bool>.Run(() => HomepageData.Delete(request.Item));
            return this.Ok(result);
        }
    }
}