
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
    [RoutePrefix("SuppliercompanyService")]
    public class SuppliercompanyServiceController : ApiController
    {

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<Suppliercompany>();
            result.Item = new Suppliercompany();
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<SuppliercompanyCreteria> request)
        {
            SuppliercompanyDataService SuppliercompanyData = new SuppliercompanyDataService();
            List<Suppliercompany> result = await Task<List<Suppliercompany>>.Run(() => SuppliercompanyData.Search(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Save")]
        public async Task<IHttpActionResult> Save(ActionRequest<Suppliercompany> request)
        {
            SuppliercompanyDataService SuppliercompanyData = new SuppliercompanyDataService();
            if (request.Item.id <= 0)
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Create);
                Suppliercompany result = await Task<Suppliercompany>.Run(() => SuppliercompanyData.Insert(request.Item));
                return this.Ok(result);
            }
            else
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Update);
                Suppliercompany result = await Task<Suppliercompany>.Run(() => SuppliercompanyData.Update(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("Insert")]
        public async Task<IHttpActionResult> Insert(ActionRequest<Suppliercompany> request)
        {
            SuppliercompanyDataService SuppliercompanyData = new SuppliercompanyDataService();
            Suppliercompany result = await Task<Suppliercompany>.Run(() => SuppliercompanyData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        public async Task<IHttpActionResult> Update(ActionRequest<Suppliercompany> request)
        {
            SuppliercompanyDataService SuppliercompanyData = new SuppliercompanyDataService();
            Suppliercompany result = await Task<Suppliercompany>.Run(() => SuppliercompanyData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IHttpActionResult> Delete(ActionRequest<Suppliercompany> request)
        {
            SuppliercompanyDataService SuppliercompanyData = new SuppliercompanyDataService();
            bool result = await Task<bool>.Run(() => SuppliercompanyData.Delete(request.Item));
            return this.Ok(result);
        }
    }
}