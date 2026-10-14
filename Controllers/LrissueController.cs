
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
    [RoutePrefix("LrissueService")]
    public class LrissueServiceController : ApiController
    {

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<Lrissue>();
            result.Item = new Lrissue();
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<LrissueCreteria> request)
        {
            LrissueDataService LrissueData = new LrissueDataService();
            List<Lrissue> result = await Task<List<Lrissue>>.Run(() => LrissueData.Search(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Save")]
        public async Task<IHttpActionResult> Save(ActionRequest<Lrissue> request)
        {
            LrissueDataService LrissueData = new LrissueDataService();
            if (request.Item.id <= 0)
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Create);
                if (request.Item.lrissuelist != null)
                {
                  foreach (var item in request.Item.lrissuelist)
                    {
                        item.ChangeStatus(BaseEntity.DataStatus.Create);
                    }
                }
                Lrissue result = await Task<Lrissue>.Run(() => LrissueData.Insert(request.Item));
                return this.Ok(result);
            }
            else
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Update);
                if (request.Item.lrissuelist != null)
                {
                    foreach (var item in request.Item.lrissuelist)
                    {
                        item.ChangeStatus(BaseEntity.DataStatus.Update);
                    }
                }
                Lrissue result = await Task<Lrissue>.Run(() => LrissueData.Update(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("Insert")]
        public async Task<IHttpActionResult> Insert(ActionRequest<Lrissue> request)
        {
            LrissueDataService LrissueData = new LrissueDataService();
            Lrissue result = await Task<Lrissue>.Run(() => LrissueData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        public async Task<IHttpActionResult> Update(ActionRequest<Lrissue> request)
        {
            LrissueDataService LrissueData = new LrissueDataService();
            Lrissue result = await Task<Lrissue>.Run(() => LrissueData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IHttpActionResult> Delete(ActionRequest<Lrissue> request)
        {
            LrissueDataService LrissueData = new LrissueDataService();
            bool result = await Task<bool>.Run(() => LrissueData.Delete(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Print")]
        public async Task<IHttpActionResult> Print(ActionRequest<Lrissue> request)
        {
            LrissueDataService LrissueData = new LrissueDataService();
            LRIssuePrint result = await Task<LRIssuePrint>.Run(() => LrissueData.Print(request.Item));
            return this.Ok(result);
        }
    }
}