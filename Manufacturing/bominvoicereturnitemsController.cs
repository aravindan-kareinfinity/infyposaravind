
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
    [RoutePrefix("bominvoicereturnitemsService")]
    public class bominvoicereturnitemsServiceController : ApiController
    {

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<Lrinvoicereturnitems>();
            result.Item = new Lrinvoicereturnitems();
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<LrinvoicereturnitemsCreteria> request)
        {
            LrinvoicereturnitemsDataService LrinvoicereturnitemsData = new LrinvoicereturnitemsDataService();
            List<Lrinvoicereturnitems> result = await Task<List<Lrinvoicereturnitems>>.Run(() => LrinvoicereturnitemsData.Search(request.Item));
            return this.Ok(result);
        }

        //[HttpPost]
        //[Route("Save")]
        //public async Task<IHttpActionResult> Save(ActionRequest<Lrinvoicereturnitems> request)
        //{
        //    LrinvoicereturnitemsDataService LrinvoicereturnitemsData = new LrinvoicereturnitemsDataService();
        //    if (request.Item.id <= 0)
        //    {
        //        request.Item.ChangeStatus(BaseEntity.DataStatus.Create);
        //        Lrinvoicereturnitems result = await Task<Lrinvoicereturnitems>.Run(() => LrinvoicereturnitemsData.Insert(request.Item));
        //        return this.Ok(result);
        //    }
        //    else
        //    {
        //        request.Item.ChangeStatus(BaseEntity.DataStatus.Update);
        //        Lrinvoicereturnitems result = await Task<Lrinvoicereturnitems>.Run(() => LrinvoicereturnitemsData.Update(request.Item));
        //        return this.Ok(result);
        //    }
        //}

        //[HttpPost]
        //[Route("Insert")]
        //public async Task<IHttpActionResult> Insert(ActionRequest<Lrinvoicereturnitems> request)
        //{
        //    LrinvoicereturnitemsDataService LrinvoicereturnitemsData = new LrinvoicereturnitemsDataService();
        //    Lrinvoicereturnitems result = await Task<Lrinvoicereturnitems>.Run(() => LrinvoicereturnitemsData.Insert(request.Item));
        //    return this.Ok(result);
        //}

        //[HttpPost]
        //[Route("Update")]
        //public async Task<IHttpActionResult> Update(ActionRequest<Lrinvoicereturnitems> request)
        //{
        //    LrinvoicereturnitemsDataService LrinvoicereturnitemsData = new LrinvoicereturnitemsDataService();
        //    Lrinvoicereturnitems result = await Task<Lrinvoicereturnitems>.Run(() => LrinvoicereturnitemsData.Update(request.Item));
        //    return this.Ok(result);
        //}

        //[HttpPost]
        //[Route("Delete")]
        //public async Task<IHttpActionResult> Delete(ActionRequest<Lrinvoicereturnitems> request)
        //{
        //    LrinvoicereturnitemsDataService LrinvoicereturnitemsData = new LrinvoicereturnitemsDataService();
        //    bool result = await Task<bool>.Run(() => LrinvoicereturnitemsData.Delete(request.Item));
        //    return this.Ok(result);
        //}
    }
}