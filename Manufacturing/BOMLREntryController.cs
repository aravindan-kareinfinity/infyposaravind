
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
    [RoutePrefix("BOMLrentryService")]
    public class BOMLrentryServiceController : ApiController
    {

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<BOMLrentry>();
            result.Item = new BOMLrentry();
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<BOMLrentryCreteria> request)
        {
            BOMLrentryDataService BOMLrentryData = new BOMLrentryDataService();
            List<BOMLrentry> result = await Task<List<BOMLrentry>>.Run(() => BOMLrentryData.Search(request.Item));
            return this.Ok(result);
        }


        [HttpPost]
        [Route("SearchLast")]
        public async Task<IHttpActionResult> SearchLast(ActionRequest<BOMLrentryCreteria> request)
        {
            BOMLrentryDataService BOMLrentryData = new BOMLrentryDataService();
            LrInformation result = await Task<LrInformation>.Run(() => BOMLrentryData.SearchLast(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Save")]
        public async Task<IHttpActionResult> Save(ActionRequest<BOMLrentry> request)
        {

            BOMLrentryDataService BOMLrentryData = new BOMLrentryDataService();
            if (request.Item.id <= 0)
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Create);
                //if (request.Item.BOMLrentryfreightcharges != null)
                //{
                //    foreach (var item in request.Item.BOMLrentryfreightcharges)
                //    {
                //        item.ChangeStatus(BaseEntity.DataStatus.Create);

                //    }
                //}
                BOMLrentry result = await Task<BOMLrentry>.Run(() => BOMLrentryData.Insert(request.Item));
                return this.Ok(result);
            }
            else
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Update);
                //if (request.Item.BOMLrentryfreightcharges != null)
                //{
                //    foreach (var item in request.Item.BOMLrentryfreightcharges)
                //    {
                //        item.ChangeStatus(BaseEntity.DataStatus.Update);
                //    }
                //}
                BOMLrentry result = await Task<BOMLrentry>.Run(() => BOMLrentryData.Update(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("Insert")]
        public async Task<IHttpActionResult> Insert(ActionRequest<BOMLrentry> request)
        {
            BOMLrentryDataService BOMLrentryData = new BOMLrentryDataService();
            BOMLrentry result = await Task<BOMLrentry>.Run(() => BOMLrentryData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        public async Task<IHttpActionResult> Update(ActionRequest<BOMLrentry> request)
        {
            BOMLrentryDataService BOMLrentryData = new BOMLrentryDataService();
            BOMLrentry result = await Task<BOMLrentry>.Run(() => BOMLrentryData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IHttpActionResult> Delete(ActionRequest<BOMLrentry> request)
        {
            BOMLrentryDataService BOMLrentryData = new BOMLrentryDataService();
            bool result = await Task<bool>.Run(() => BOMLrentryData.Delete(request.Item));
            return this.Ok(result);
        }
    }
}