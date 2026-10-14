
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
    [RoutePrefix("ProductrequestformService")]
    public class ProductrequestformServiceController : ApiController
    {

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<Productrequestform>();
            result.Item = new Productrequestform();
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<ProductrequestformCreteria> request)
        {
            ProductrequestformDataService ProductrequestformData = new ProductrequestformDataService();
            List<Productrequestform> result = await Task<List<Productrequestform>>.Run(() => ProductrequestformData.Search(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("SearchOrders")]
        public async Task<IHttpActionResult> Search(ActionRequest<PRFRequest> request)
        {
            ProductrequestformDataService ProductrequestformData = new ProductrequestformDataService();
            List<ProductrequestformitemsDetail> result = await Task<List<ProductrequestformitemsDetail>>.Run(() => ProductrequestformData.SearchOrders(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("CancelOrders")]
        public async Task<IHttpActionResult> CancelOrders(ActionMulitpleRequest<Productrequestformitems> request)
        {
            ProductrequestformitemsDataService ProductrequestformData = new ProductrequestformitemsDataService();
            List<Productrequestformitems> result = await Task<List<Productrequestformitems>>.Run(() => ProductrequestformData.CancelOrders(request.Items));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("SearchPO")]
        public async Task<IHttpActionResult> SearchPO(ActionRequest<POSearch> request)
        {
            ProductrequestformDataService ProductrequestformData = new ProductrequestformDataService();
            POResult result = await Task<POResult>.Run(() => ProductrequestformData.SearchPO(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Save")]
        public async Task<IHttpActionResult> Save(ActionRequest<Productrequestform> request)
        {

            ProductrequestformDataService ProductrequestformData = new ProductrequestformDataService();
            if (request.Item.id <= 0)
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Create);
                if (request.Item.Productrequestformitems != null)
                {
                    foreach(var item in request.Item.Productrequestformitems)
                    {
                        item.ChangeStatus(BaseEntity.DataStatus.Create);
                    }
                }
                Productrequestform result = await Task<Productrequestform>.Run(() => ProductrequestformData.Insert(request.Item));
                return this.Ok(result);
            }
            else
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Update);
                if (request.Item.Productrequestformitems != null)
                {
                    foreach (var item in request.Item.Productrequestformitems)
                    {
                        item.ChangeStatus(BaseEntity.DataStatus.Update);
                    }
                }
                Productrequestform result = await Task<Productrequestform>.Run(() => ProductrequestformData.Update(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("Insert")]
        public async Task<IHttpActionResult> Insert(ActionRequest<Productrequestform> request)
        {
            ProductrequestformDataService ProductrequestformData = new ProductrequestformDataService();
            Productrequestform result = await Task<Productrequestform>.Run(() => ProductrequestformData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        public async Task<IHttpActionResult> Update(ActionRequest<Productrequestform> request)
        {
            ProductrequestformDataService ProductrequestformData = new ProductrequestformDataService();
            Productrequestform result = await Task<Productrequestform>.Run(() => ProductrequestformData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IHttpActionResult> Delete(ActionRequest<Productrequestform> request)
        {
            ProductrequestformDataService ProductrequestformData = new ProductrequestformDataService();
            bool result = await Task<bool>.Run(() => ProductrequestformData.Delete(request.Item));
            return this.Ok(result);
        }
    }
}