
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
    [RoutePrefix("EmployeeleavemasterService")]
    public class EmployeeleavemasterServiceController : ApiController
    {

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<Employeeleavemaster>();
            result.Item = new Employeeleavemaster();
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Search")]
        //[WebAPI.InfyAuthorize(PermissionSet.employeeleave, InfyPOS.Common.Action.View)]
        public async Task<IHttpActionResult> Search(ActionRequest<EmployeeleavemasterCreteria> request)
        {
            EmployeeleavemasterDataService EmployeeleavemasterData = new EmployeeleavemasterDataService();
            List<Employeeleavemaster> result = await Task<List<Employeeleavemaster>>.Run(() => EmployeeleavemasterData.Search(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Save")]
        //[WebAPI.InfyAuthorize(PermissionSet.employeeleave, InfyPOS.Common.Action.Create)]
        public async Task<IHttpActionResult> Save(ActionRequest<Employeeleavemaster> request)
        {
            EmployeeleavemasterDataService EmployeeleavemasterData = new EmployeeleavemasterDataService();
            if (request.Item.id <= 0)
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Create);
                Employeeleavemaster result = await Task<Employeeleavemaster>.Run(() => EmployeeleavemasterData.Insert(request.Item));
                return this.Ok(result);
            }
            else
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Update);
                Employeeleavemaster result = await Task<Employeeleavemaster>.Run(() => EmployeeleavemasterData.Update(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("Insert")]
        //[WebAPI.InfyAuthorize(PermissionSet.employeeleave, InfyPOS.Common.Action.Create)]
        public async Task<IHttpActionResult> Insert(ActionRequest<Employeeleavemaster> request)
        {
            EmployeeleavemasterDataService EmployeeleavemasterData = new EmployeeleavemasterDataService();
            Employeeleavemaster result = await Task<Employeeleavemaster>.Run(() => EmployeeleavemasterData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        //[WebAPI.InfyAuthorize(PermissionSet.employeeleave, InfyPOS.Common.Action.Modify)]
        public async Task<IHttpActionResult> Update(ActionRequest<Employeeleavemaster> request)
        {
            EmployeeleavemasterDataService EmployeeleavemasterData = new EmployeeleavemasterDataService();
            Employeeleavemaster result = await Task<Employeeleavemaster>.Run(() => EmployeeleavemasterData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        //[WebAPI.InfyAuthorize(PermissionSet.employeeleave, InfyPOS.Common.Action.Delete)]
        public async Task<IHttpActionResult> Delete(ActionRequest<Employeeleavemaster> request)
        {
            EmployeeleavemasterDataService EmployeeleavemasterData = new EmployeeleavemasterDataService();
            bool result = await Task<bool>.Run(() => EmployeeleavemasterData.Delete(request.Item));
            return this.Ok(result);
        }
    }
}