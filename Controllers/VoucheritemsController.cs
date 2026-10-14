
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
    [RoutePrefix("VoucheritemsService")]
    public class VoucheritemsServiceController : ApiController
    {

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<Voucheritems>();
            result.Item = new Voucheritems();
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Search")]
        //[WebAPI.InfyAuthorize(PermissionSet.voucher, InfyPOS.Common.Action.View)]
        public async Task<IHttpActionResult> Search(ActionRequest<VoucheritemsCreteria> request)
        {
            VoucheritemsDataService VoucheritemsData = new VoucheritemsDataService();
            List<Voucheritems> result = await Task<List<Voucheritems>>.Run(() => VoucheritemsData.Search(request.Item));
            return this.Ok(result);
        }

        //[HttpPost]
        //[Route("Save")]
        ////[WebAPI.InfyAuthorize(PermissionSet.voucher, InfyPOS.Common.Action.Create)]
        //public async Task<IHttpActionResult> Save(ActionRequest<Voucheritems> request)
        //{
        //    VoucheritemsDataService VoucheritemsData = new VoucheritemsDataService();
        //    if (request.Item.id <= 0)
        //    {
        //        request.Item.ChangeStatus(BaseEntity.DataStatus.Create);
             
        //        Voucheritems result = await Task<Voucheritems>.Run(() => VoucheritemsData.Insert(request.Item));
        //        return this.Ok(result);
        //    }
        //    else
        //    {
        //        request.Item.ChangeStatus(BaseEntity.DataStatus.Update);
        //        Voucheritems result = await Task<Voucheritems>.Run(() => VoucheritemsData.Update(request.Item));
        //        return this.Ok(result);
        //    }
        //}

        //[HttpPost]
        //[Route("Insert")]
        ////[WebAPI.InfyAuthorize(PermissionSet.voucher, InfyPOS.Common.Action.Create)]
        //public async Task<IHttpActionResult> Insert(ActionRequest<Voucheritems> request)
        //{
        //    VoucheritemsDataService VoucheritemsData = new VoucheritemsDataService();
        //    Voucheritems result = await Task<Voucheritems>.Run(() => VoucheritemsData.Insert(request.Item));
        //    return this.Ok(result);
        //}

        [HttpPost]
        [Route("Update")]
        //[WebAPI.InfyAuthorize(PermissionSet.voucher, InfyPOS.Common.Action.Modify)]
        public async Task<IHttpActionResult> Update(ActionRequest<Voucheritems> request)
        {
            VoucheritemsDataService VoucheritemsData = new VoucheritemsDataService();
            Voucheritems result = await Task<Voucheritems>.Run(() => VoucheritemsData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        //[WebAPI.InfyAuthorize(PermissionSet.voucher, InfyPOS.Common.Action.Delete)]
        public async Task<IHttpActionResult> Delete(ActionRequest<Voucheritems> request)
        {
            VoucheritemsDataService VoucheritemsData = new VoucheritemsDataService();
            bool result = await Task<bool>.Run(() => VoucheritemsData.Delete(request.Item));
            return this.Ok(result);
        }
    }
}