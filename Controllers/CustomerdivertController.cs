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
    [RoutePrefix("CustomerdivertService")]
    public class CustomerdivertServiceController : ApiController
    {

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<Customerdivert>();
            result.Item = new Customerdivert();
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<CustomerdivertCreteria> request)
        {
            CustomerdivertDataService CustomerdivertData = new CustomerdivertDataService();
            List<Customerdivert> result = await Task<List<Customerdivert>>.Run(() => CustomerdivertData.Search(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Save")]
        public async Task<IHttpActionResult> Save(ActionRequest<Customerdivert> request)
        {
            CustomerdivertDataService CustomerdivertData = new CustomerdivertDataService();
            if (request.Item.id <= 0)
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Create);
                Customerdivert result = await Task<Customerdivert>.Run(() => CustomerdivertData.Insert(request.Item));
                return this.Ok(result);
            }
            else
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Update);
                Customerdivert result = await Task<Customerdivert>.Run(() => CustomerdivertData.Update(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("Insert")]
        public async Task<IHttpActionResult> Insert(ActionRequest<Customerdivert> request)
        {
            CustomerdivertDataService CustomerdivertData = new CustomerdivertDataService();
            Customerdivert result = await Task<Customerdivert>.Run(() => CustomerdivertData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        public async Task<IHttpActionResult> Update(ActionRequest<Customerdivert> request)
        {
            CustomerdivertDataService CustomerdivertData = new CustomerdivertDataService();
            Customerdivert result = await Task<Customerdivert>.Run(() => CustomerdivertData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IHttpActionResult> Delete(ActionRequest<Customerdivert> request)
        {
            CustomerdivertDataService CustomerdivertData = new CustomerdivertDataService();
            bool result = await Task<bool>.Run(() => CustomerdivertData.Delete(request.Item));
            return this.Ok(result);
        }

    }
}