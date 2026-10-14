
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
    [RoutePrefix("AgentpaymentService")]
    public class AgentpaymentServiceController : ApiController
    {

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<Agentpayment>();
            result.Item = new Agentpayment();
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<AgentpaymentCreteria> request)
        {
            AgentpaymentDataService AgentpaymentData = new AgentpaymentDataService();
            List<Agentpayment> result = await Task<List<Agentpayment>>.Run(() => AgentpaymentData.Search(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Save")]
        public async Task<IHttpActionResult> Save(ActionRequest<Agentpayment> request)
        {
            AgentpaymentDataService AgentpaymentData = new AgentpaymentDataService();
            if (request.Item.id <= 0)
            {
                Agentpayment result = await Task<Agentpayment>.Run(() => AgentpaymentData.Insert(request.Item));
                return this.Ok(result);
            }
            else
            {
                Agentpayment result = await Task<Agentpayment>.Run(() => AgentpaymentData.Update(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("Insert")]
        public async Task<IHttpActionResult> Insert(ActionRequest<Agentpayment> request)
        {
            AgentpaymentDataService AgentpaymentData = new AgentpaymentDataService();
            Agentpayment result = await Task<Agentpayment>.Run(() => AgentpaymentData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        public async Task<IHttpActionResult> Update(ActionRequest<Agentpayment> request)
        {
            AgentpaymentDataService AgentpaymentData = new AgentpaymentDataService();
            Agentpayment result = await Task<Agentpayment>.Run(() => AgentpaymentData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IHttpActionResult> Delete(ActionRequest<Agentpayment> request)
        {
            AgentpaymentDataService AgentpaymentData = new AgentpaymentDataService();
            bool result = await Task<bool>.Run(() => AgentpaymentData.Delete(request.Item));
            return this.Ok(result);
        }



        [HttpPost]
        [Route("SearchPendingPayment")]
        public async Task<IHttpActionResult> SearchPendingPayment(ActionRequest<AgentpaymentCreteria> request)
        {
            AgentpaymentDataService agentpaymentdata = new AgentpaymentDataService();
            List<AgentpaymentinvoiceDetail> result = await Task<List<AgentpaymentinvoiceDetail>>.Run(() => agentpaymentdata.SearchPendingPayment(request.Item));
            return this.Ok(result);
        }
    }
}