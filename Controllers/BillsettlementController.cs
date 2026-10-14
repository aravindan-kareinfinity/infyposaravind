
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
    [RoutePrefix("BillsettlementService")]
    public class BillsettlementServiceController : ApiController
    {

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<Billsettlement>();
            result.Item = new Billsettlement();
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Search")]
        [WebAPI.InfyAuthorize(PermissionSet.settlement, InfyPOS.Common.Action.View)]
        public async Task<IHttpActionResult> Search(ActionRequest<BillsettlementCreteria> request)
        {
            BillsettlementDataService BillsettlementData = new BillsettlementDataService();
            List<Billsettlement> result = await Task<List<Billsettlement>>.Run(() => BillsettlementData.Search(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Save")]
        [WebAPI.InfyAuthorize(PermissionSet.settlement, InfyPOS.Common.Action.Create)]
        public async Task<IHttpActionResult> Save(ActionRequest<Billsettlement> request)
        {
            BillsettlementDataService BillsettlementData = new BillsettlementDataService();
            if (request.Item.id <= 0)
            {
                if (request.Item.locationid <= 0)
                {
                    throw new Exception("Please select the location.");
                }
                request.Item.ChangeStatus(BaseEntity.DataStatus.Create);
                Billsettlement result = await Task<Billsettlement>.Run(() => BillsettlementData.Insert(request.Item));
                return this.Ok(result);
            }
            else
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Update);
                Billsettlement result = await Task<Billsettlement>.Run(() => BillsettlementData.Update(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("Insert")]
        [WebAPI.InfyAuthorize(PermissionSet.settlement, InfyPOS.Common.Action.Create)]
        public async Task<IHttpActionResult> Insert(ActionRequest<Billsettlement> request)
        {
            BillsettlementDataService BillsettlementData = new BillsettlementDataService();
            Billsettlement result = await Task<Billsettlement>.Run(() => BillsettlementData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        [WebAPI.InfyAuthorize(PermissionSet.settlement, InfyPOS.Common.Action.Modify)]
        public async Task<IHttpActionResult> Update(ActionRequest<Billsettlement> request)
        {
            BillsettlementDataService BillsettlementData = new BillsettlementDataService();
            Billsettlement result = await Task<Billsettlement>.Run(() => BillsettlementData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        [WebAPI.InfyAuthorize(PermissionSet.settlement, InfyPOS.Common.Action.Delete)]
        public async Task<IHttpActionResult> Delete(ActionRequest<Billsettlement> request)
        {
            BillsettlementDataService BillsettlementData = new BillsettlementDataService();
            bool result = await Task<bool>.Run(() => BillsettlementData.Delete(request.Item));
            return this.Ok(result);
        }
    }
}