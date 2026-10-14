
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
    [RoutePrefix("ProductrequestformitemsService")]
    public class ProductrequestformitemsServiceController : ApiController
    {

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<Productrequestformitems>();
            result.Item = new Productrequestformitems();
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<ProductrequestformitemsCreteria> request)
        {
            ProductrequestformitemsDataService ProductrequestformitemsData = new ProductrequestformitemsDataService();
            List<Productrequestformitems> result = await Task<List<Productrequestformitems>>.Run(() => ProductrequestformitemsData.Search(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Save")]
        public async Task<IHttpActionResult> Save(ActionRequest<Productrequestformitems> request)
        {
            ProductrequestformitemsDataService ProductrequestformitemsData = new ProductrequestformitemsDataService();
            if (request.Item.id <= 0)
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Create);
             
                Productrequestformitems result = await Task<Productrequestformitems>.Run(() => ProductrequestformitemsData.Insert(request.Item));
                return this.Ok(result);
            }
            else
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Update);
                Productrequestformitems result = await Task<Productrequestformitems>.Run(() => ProductrequestformitemsData.Update(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("Insert")]
        public async Task<IHttpActionResult> Insert(ActionRequest<Productrequestformitems> request)
        {
            ProductrequestformitemsDataService ProductrequestformitemsData = new ProductrequestformitemsDataService();
            Productrequestformitems result = await Task<Productrequestformitems>.Run(() => ProductrequestformitemsData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        public async Task<IHttpActionResult> Update(ActionRequest<Productrequestformitems> request)
        {
            ProductrequestformitemsDataService ProductrequestformitemsData = new ProductrequestformitemsDataService();
            Productrequestformitems result = await Task<Productrequestformitems>.Run(() => ProductrequestformitemsData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IHttpActionResult> Delete(ActionRequest<Productrequestformitems> request)
        {
            ProductrequestformitemsDataService ProductrequestformitemsData = new ProductrequestformitemsDataService();
            bool result = await Task<bool>.Run(() => ProductrequestformitemsData.Delete(request.Item));
            return this.Ok(result);
        }
    }
}