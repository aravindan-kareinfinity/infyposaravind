
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
    [RoutePrefix("LoansettlementService")]
    public class LoansettlementServiceController : ApiController
    {

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<Loansettlement>();
            result.Item = new Loansettlement();
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<LoansettlementCreteria> request)
        {
            LoansettlementDataService LoansettlementData = new LoansettlementDataService();
            List<LoansettlementDetail> result = await Task<List<LoansettlementDetail>>.Run(() => LoansettlementData.Search(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Save")]
        public async Task<IHttpActionResult> Save(ActionRequest<LoandSettlementPayment> request)
        {
            LoansettlementDataService LoansettlementData = new LoansettlementDataService();

            LoandSettlementPayment result = await Task<LoandSettlementPayment>.Run(() => LoansettlementData.UpdatePayment(request.Item));
            return this.Ok(result);

        }

        [HttpPost]
        [Route("Insert")]
        public async Task<IHttpActionResult> Insert(ActionRequest<Loansettlement> request)
        {
            LoansettlementDataService LoansettlementData = new LoansettlementDataService();
            Loansettlement result = await Task<Loansettlement>.Run(() => LoansettlementData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        public async Task<IHttpActionResult> Update(ActionRequest<Loansettlement> request)
        {
            LoansettlementDataService LoansettlementData = new LoansettlementDataService();
            Loansettlement result = await Task<Loansettlement>.Run(() => LoansettlementData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IHttpActionResult> Delete(ActionRequest<Loansettlement> request)
        {
            LoansettlementDataService LoansettlementData = new LoansettlementDataService();
            bool result = await Task<bool>.Run(() => LoansettlementData.Delete(request.Item));
            return this.Ok(result);
        }
    }
}