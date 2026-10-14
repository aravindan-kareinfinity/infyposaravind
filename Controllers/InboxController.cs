
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
    [RoutePrefix("InboxService")]
    public class InboxServiceController : ApiController
    {

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<Inbox>();
            result.Item = new Inbox();
            return this.Ok(result);
        }


        [HttpGet]
        [Route("LRReceived/{lrid}")]
        public async Task<IHttpActionResult> HasReceived(string lrid)
        {
            LrreceivedlistDataService lrreceivedlist = new LrreceivedlistDataService();
            List<receiptitem> result = await Task<List<receiptitem>>.Run(() => lrreceivedlist.Search(new LrreceivedlistCreteria() { lrid = long.Parse(lrid)}));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<InboxCreteria> request)
        {
            var queueManager = InfyPOS.Processors.WorkflowQueueManager.Instance;
            if (!queueManager.Enabled)
            {
                InboxDataService InboxData = new InboxDataService();
                List<Inbox> result = await Task<List<Inbox>>.Run(() => InboxData.Search(request.Item));
                return this.Ok(result);
            }
            else
                {
                InboxDataService ActualDataService = new InboxDataService();
                var organizationId = ActualDataService.Context?.OrganizationID ?? 0;
                var userId = ActualDataService.Context?.UserID.ToString() ?? "";
                var response = await queueManager.ExecuteWithTimeout<object>(async () =>
                {
                    return await Task<List<Inbox>>.Run(() => ActualDataService.Search(request.Item));
                }, "InboxService.Search", request.Item, 3, organizationId, userId);

                if (response.IsCompleted)
                {
                    return this.Ok(response.Result);
                }
                else
                {
                    return this.ResponseMessage(response.TimoutResponse);
                }
            }
        }

        [HttpPost]
        [Route("Save")]
        public async Task<IHttpActionResult> Save(ActionRequest<Inbox> request)
        {
            InboxDataService InboxData = new InboxDataService();
            if (request.Item.id <= 0)
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Create);
                Inbox result = await Task<Inbox>.Run(() => InboxData.Insert(request.Item));
                return this.Ok(result);
            }
            else
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Update);
                Inbox result = await Task<Inbox>.Run(() => InboxData.Update(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("Insert")]
        public async Task<IHttpActionResult> Insert(ActionRequest<Inbox> request)
        {
            InboxDataService InboxData = new InboxDataService();
            Inbox result = await Task<Inbox>.Run(() => InboxData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("UpdateStatus")]
        public async Task<IHttpActionResult> UpdateStatus(ActionMulitpleRequest<InBoxStatus> request)
        {
            InboxDataService InboxData = new InboxDataService();
            List<Inbox> result = await Task<List<Inbox>>.Run(() => InboxManager.Instance(InboxData.Context.OrganizationID).ChangeStatus(InboxData.Context,request.Items, InboxData.Context));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        public async Task<IHttpActionResult> Update(ActionRequest<Inbox> request)
        {
            InboxDataService InboxData = new InboxDataService();
            Inbox result = await Task<Inbox>.Run(() => InboxData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IHttpActionResult> Delete(ActionRequest<Inbox> request)
        {
            InboxDataService InboxData = new InboxDataService();
            bool result = await Task<bool>.Run(() => InboxData.Delete(request.Item));
            return this.Ok(result);
        }
    }
}