
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
    [RoutePrefix("AddondataService")]
    public class AddondataServiceController : ApiController
    {

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<Addondata>();
            result.Item = new Addondata();
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<AddondataCreteria> request)
        {
            AddondataDataService AddondataData = new AddondataDataService();
            List<Addondata> result = await Task<List<Addondata>>.Run(() => AddondataData.Search(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Save")]
        public async Task<IHttpActionResult> Save(ActionRequest<Addondata> request)
        {
            AddondataDataService AddondataData = new AddondataDataService();
            if (request.Item.id <= 0)
            {
                Addondata result = await Task<Addondata>.Run(() => AddondataData.Insert(request.Item));
                return this.Ok(result);
            }
            else
            {
                Addondata result = await Task<Addondata>.Run(() => AddondataData.Update(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("ReportTargetMargin")]
        public async Task<IHttpActionResult> ReportTargetMargin(ActionRequest<Addon_ReportTargetMargin> request)
        {
            AddondataDataService AddondataData = new AddondataDataService();
            List<Addon_ReportTargetMargin.ReportItem> result = await Task<List<Addon_ReportTargetMargin.ReportItem>>.Run(() => AddondataData.ReportTargetMargin(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Insert")]
        public async Task<IHttpActionResult> Insert(ActionRequest<Addondata> request)
        {
            AddondataDataService AddondataData = new AddondataDataService();
            Addondata result = await Task<Addondata>.Run(() => AddondataData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        public async Task<IHttpActionResult> Update(ActionRequest<Addondata> request)
        {
            AddondataDataService AddondataData = new AddondataDataService();
            Addondata result = await Task<Addondata>.Run(() => AddondataData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IHttpActionResult> Delete(ActionRequest<Addondata> request)
        {
            AddondataDataService AddondataData = new AddondataDataService();
            bool result = await Task<bool>.Run(() => AddondataData.Delete(request.Item));
            return this.Ok(result);
        }
    }
}