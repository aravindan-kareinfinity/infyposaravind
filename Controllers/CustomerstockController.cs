
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
    [RoutePrefix("CustomerstockService")]
    public class CustomerstockServiceController : ApiController
    {

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<Customerstock>();
            result.Item = new Customerstock();
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<CustomerstockCreteria> request)
        {
            CustomerstockDataService CustomerstockData = new CustomerstockDataService();
            List<Customerstock> result = await Task<List<Customerstock>>.Run(() => CustomerstockData.Search(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Save")]
        public async Task<IHttpActionResult> Save(ActionRequest<Customerstock> request)
        {
            CustomerstockDataService CustomerstockData = new CustomerstockDataService();
            if (request.Item.id <= 0)
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Create);
                Customerstock result = await Task<Customerstock>.Run(() => CustomerstockData.Insert(request.Item));
                return this.Ok(result);
            }
            else
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Update);
                Customerstock result = await Task<Customerstock>.Run(() => CustomerstockData.Update(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("Insert")]
        public async Task<IHttpActionResult> Insert(ActionRequest<Customerstock> request)
        {
            CustomerstockDataService CustomerstockData = new CustomerstockDataService();
            Customerstock result = await Task<Customerstock>.Run(() => CustomerstockData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        public async Task<IHttpActionResult> Update(ActionRequest<Customerstock> request)
        {
            CustomerstockDataService CustomerstockData = new CustomerstockDataService();
            Customerstock result = await Task<Customerstock>.Run(() => CustomerstockData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IHttpActionResult> Delete(ActionRequest<Customerstock> request)
        {
            CustomerstockDataService CustomerstockData = new CustomerstockDataService();
            bool result = await Task<bool>.Run(() => CustomerstockData.Delete(request.Item));
            return this.Ok(result);
        }
    }
}