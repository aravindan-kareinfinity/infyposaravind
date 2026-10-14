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
using System.Runtime.Serialization;
using InfyPOS.Processors;
using InfyPOS.Services;

namespace InfyPOS.Controllers
{
    [RoutePrefix("SyncBI_Controller")]
    public class SyncBI_Controller : ApiController
    {


        [HttpGet]
        [AllowAnonymous]
        [Route("DownloadSchema/{clientid}")]
        public async Task<IHttpActionResult> DownloadSchema(string clientid)
        {
            BI_Server bI_Server = new BI_Server();
            if (!bI_Server.Connect(clientid))
            {
                throw new Exception(clientid + " Not found");
            }
            List<BI_Server.DBSchemaTable> result = await Task<List<BI_Server.DBSchemaTable>>.Run(() => bI_Server.DBSchema());
            return this.Ok(result);
        }



        [HttpGet]
        [AllowAnonymous]
        [Route("ClearStatus/{clientid}/{sales}/{purchase}/{stock}")]
        public async Task<IHttpActionResult> ClearStatus(string clientid, string sales, string purchase, string stock)
        {
            BI_Server bI_Server = new BI_Server();
            if (!bI_Server.Connect(clientid))
            {
                throw new Exception(clientid + " Not found");
            }
            var parameter = new BI_Server.DownloadList()
            {
                sales = !string.IsNullOrEmpty(sales) && sales.ToLower() == "true",
                purchase = !string.IsNullOrEmpty(purchase) && purchase.ToLower() == "true",
                stock = !string.IsNullOrEmpty(stock) && stock.ToLower() == "true",
            };
            bool result = await Task<bool>.Run(() => bI_Server.ClearStatus(new SyncManager.ProcessStatus()
            {
              parameter = parameter
            }));

            return this.Ok(result);
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("StartDownloadMasters/{clientid}/{args}")]
        public async Task<IHttpActionResult> StartDownloadMasters(string clientid, string args)
        {
            BI_Server bI_Server = new BI_Server();
            if (!bI_Server.Connect(clientid))
            {
                throw new Exception(clientid + " Not found");
            }
            SyncManager.ProcessStatus result = await Task<SyncManager.ProcessStatus>.Run(() => bI_Server.StartDownloadMaster(args));
            return this.Ok(result);
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("GetDownloadedMaster/{clientid}/{key}")]
        public async Task<IHttpActionResult> GetDownloadedMaster(string clientid, string key)
        {
            BI_Server bI_Server = new BI_Server();
            if (!bI_Server.Connect(clientid))
            {
                throw new Exception(clientid + " Not found");
            }
            Services.SyncManager.ProcessStatus result = await Task<Services.SyncManager.ProcessStatus>.Run(() => bI_Server.GetDownloadedMaster(key));
            return this.Ok(result);
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("ChangeMasterStatus")]
        public async Task<IHttpActionResult> ChangeMasterStatus(SyncStatus request)
        {
            BI_Server bI_Server = new BI_Server();
            if (!bI_Server.Connect(request.clientid))
            {
                throw new Exception(request.clientid + " Not found");
            }
            bool result = await Task<bool>.Run(() => bI_Server.ChangeMasterStatus(request.data));
            return this.Ok(result);
        }


        [System.Runtime.Serialization.DataContract]
        public class SyncRequest : BaseEntity
        {
            [DataMember]
            public string location { get; set; }
            [DataMember]
            public string clientid { get; set; }
            [DataMember]
            public string privatekey { get; set; }
            [DataMember]
            public bool all { get; set; }
            [DataMember]
            public long organizationid { get; set; }
            public string organizationcode { get; set; }
        }


        public class CreateRequest : SyncRequest
        {

            [DataMember]
            public string mode { get; set; }
            [DataMember]
            public DateTime fromdate { get; set; }
            public int percentage { get; set; }

        }

        public class SyncStatus : SyncRequest
        {
            [DataMember]
            public List<Services.SyncManager.DBData> data { get; set; }
        }
    }
}