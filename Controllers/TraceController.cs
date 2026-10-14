
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
    [RoutePrefix("TraceService")]
    public class TraceServiceController : ApiController
    {

        [HttpGet]
        [Route("Log/{enabled=null}/{organizationid=null}")]
        public async Task<IHttpActionResult> Log(string enabled,string organizationid)
        {
            var basedata = new BaseData();
            var contextorganizationid = basedata.Context.OrganizationID;
            if(basedata.IsSAAS)
            {
                if (!long.TryParse(organizationid, out contextorganizationid))
                    return this.Ok(false);
            }

            if (string.IsNullOrEmpty(enabled) || enabled == "null")
                return this.Ok(WebAPI.Common.Logger.Instance(contextorganizationid).LogEnabled());
            WebAPI.Common.Logger.Instance(contextorganizationid).SetLogEnable(enabled.ToLower() == "true");
            return this.Ok(WebAPI.Common.Logger.Instance(contextorganizationid).LogEnabled());
        }

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<Trace>();
            result.Item = new Trace();
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<TraceCreteria> request)
        {
            TraceDataService TraceData = new TraceDataService();
            if(TraceData.IsSAAS)
            {
                TraceData.Context.OrganizationID = request.Item.organizationid;
            }
            List<Trace> result = await Task<List<Trace>>.Run(() => TraceData.Search(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Save")]
        public async Task<IHttpActionResult> Save(ActionRequest<Trace> request)
        {
            TraceDataService TraceData = new TraceDataService();
            Trace result = await Task<Trace>.Run(() => TraceData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Insert")]
        public async Task<IHttpActionResult> Insert(ActionRequest<Trace> request)
        {
            TraceDataService TraceData = new TraceDataService();
            Trace result = await Task<Trace>.Run(() => TraceData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        public async Task<IHttpActionResult> Update(ActionRequest<Trace> request)
        {
            TraceDataService TraceData = new TraceDataService();
            Trace result = await Task<Trace>.Run(() => TraceData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IHttpActionResult> Delete(ActionRequest<Trace> request)
        {
            TraceDataService TraceData = new TraceDataService();
            bool result = await Task<bool>.Run(() => TraceData.Delete(request.Item));
            return this.Ok(result);
        }
    }
}