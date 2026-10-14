
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
    [RoutePrefix("CashService")]
    public class CashServiceController : ApiController
    {

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<Cash>();
            result.Item = new Cash();
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<CashCreteria> request)
        {
            CashDataService CashData = new CashDataService();
            if(request.Item.status == 10)
            {
                request.Item.paidon = CashData.DateTimeNow.Date;
                request.Item.cashierid = CashData.Context.EmployeeID;
                request.Item.status = 9;
            }
            List<Cash> result = await Task<List<Cash>>.Run(() => CashData.Search(request.Item));

            if (CashData.Context.HasPermission(PermissionSet.hidecashcollection, InfyPOS.Common.Action.View))
            {
                result.RemoveAll(e => e.cashierid != CashData.Context.EmployeeID);
            }
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Save")]
        public async Task<IHttpActionResult> Save(ActionRequest<Cash> request)
        {
            CashDataService CashData = new CashDataService();

            if (!CashData.Context.HasPermission(PermissionSet.cashclosing, InfyPOS.Common.Action.Create) &&
                    !CashData.Context.HasPermission(PermissionSet.cashopening, InfyPOS.Common.Action.Create))
            {
                return StatusCode(HttpStatusCode.Forbidden);
            }

            if (request.Item.id <= 0)
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Create);
                Cash result = await Task<Cash>.Run(() => CashData.Insert(request.Item));
                return this.Ok(result);
            }
            else
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Update);
                Cash result = await Task<Cash>.Run(() => CashData.Update(request.Item));
                if (result == null)
                    return StatusCode(HttpStatusCode.Forbidden);
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("Insert")]
        public async Task<IHttpActionResult> Insert(ActionRequest<Cash> request)
        {
            CashDataService CashData = new CashDataService();

            if (!CashData.Context.HasPermission(PermissionSet.cashclosing, InfyPOS.Common.Action.Create) &&
                !CashData.Context.HasPermission(PermissionSet.cashopening, InfyPOS.Common.Action.Create))
            {
                return StatusCode(HttpStatusCode.Forbidden);
            }

            Cash result = await Task<Cash>.Run(() => CashData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Post2Finance")]
        public async Task<IHttpActionResult> Post2Finance(ActionRequest<CashPosting> request)
        {
            CashDataService CashData = new CashDataService();
            CashPosting result = await Task<CashPosting>.Run(() => CashData.Post2Finance(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        public async Task<IHttpActionResult> Update(ActionRequest<Cash> request)
        {
            CashDataService CashData = new CashDataService();

            if (!CashData.Context.HasPermission(PermissionSet.cashclosing, InfyPOS.Common.Action.Modify) &&
                !CashData.Context.HasPermission(PermissionSet.cashopening, InfyPOS.Common.Action.Modify))
            {
                return StatusCode(HttpStatusCode.Forbidden);
            }

            Cash result = await Task<Cash>.Run(() => CashData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IHttpActionResult> Delete(ActionRequest<Cash> request)
        {
            CashDataService CashData = new CashDataService();

            if(!CashData.Context.HasPermission(PermissionSet.cashclosing, InfyPOS.Common.Action.Delete) &&
                !CashData.Context.HasPermission(PermissionSet.cashopening, InfyPOS.Common.Action.Delete) )
            {
                return StatusCode(HttpStatusCode.Forbidden);
            }

            bool result = await Task<bool>.Run(() => CashData.Delete(request.Item));
            return this.Ok(result);
        }
    }
}