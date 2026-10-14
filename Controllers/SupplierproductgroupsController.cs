
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
    [RoutePrefix("SupplierproductgroupsService")]
    public class SupplierproductgroupsServiceController : ApiController
    {

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<Supplierproductgroups>();
            result.Item = new Supplierproductgroups();
            return this.Ok(result);
        }
        [HttpGet]
        [Route("ActiveProducts/{supplierid}")]
        public async Task<IHttpActionResult> ActiveProducts(string supplierid)
        {
            SupplierproductgroupsDataService SupplierproductgroupsData = new SupplierproductgroupsDataService();
            List<Supplierproductgroups> result = await Task<List<Supplierproductgroups>>.Run(() => SupplierproductgroupsData.Search(new SupplierproductgroupsCreteria() { includeitems = true, supplierid = long.Parse(supplierid) }));
            result.ForEach(x =>
            {
                if (x.priceagreements != null)
                    x.priceagreements.RemoveAll(y => y.disable || y.disabledon > DateTime.MinValue);

                if(x.tradeagreements != null)
                    x.tradeagreements.RemoveAll(y => y.disable || y.disabledon > DateTime.MinValue);
            });
            return this.Ok(result);
        }
        [HttpGet]
        [Route("ActiveDailyProducts/{supplierid}/{dailypricegroupid}")]
        public async Task<IHttpActionResult> ActiveDailyProducts(string supplierid, string dailypricegroupid)
        {
            SupplierproductgroupsDataService SupplierproductgroupsData = new SupplierproductgroupsDataService();
            List<Supplierproductgroups> result = await Task<List<Supplierproductgroups>>.Run(() =>
            SupplierproductgroupsData.Search(new SupplierproductgroupsCreteria()
            {
                supplierid = long.Parse(supplierid),
                dailyproducts = true,
                dailypricegroupid = long.Parse(dailypricegroupid)
            }));
            result.ForEach(x =>
            {
                if (x.priceagreements != null)
                    x.priceagreements.RemoveAll(y => y.disable || y.disabledon >= DateTime.Now.Date);
            });
            return this.Ok(result);
        }
        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<SupplierproductgroupsCreteria> request)
        {
            SupplierproductgroupsDataService SupplierproductgroupsData = new SupplierproductgroupsDataService();
            List<Supplierproductgroups> result = await Task<List<Supplierproductgroups>>.Run(() => SupplierproductgroupsData.Search(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Save")]
        public async Task<IHttpActionResult> Save(ActionRequest<Supplierproductgroups> request)
        {
            SupplierproductgroupsDataService SupplierproductgroupsData = new SupplierproductgroupsDataService();
            if (request.Item.id <= 0)
            {
                Supplierproductgroups result = await Task<Supplierproductgroups>.Run(() => SupplierproductgroupsData.Insert(request.Item));
                return this.Ok(result);
            }
            else
            {
                Supplierproductgroups result = await Task<Supplierproductgroups>.Run(() => SupplierproductgroupsData.Update(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("Insert")]
        public async Task<IHttpActionResult> Insert(ActionRequest<Supplierproductgroups> request)
        {
            SupplierproductgroupsDataService SupplierproductgroupsData = new SupplierproductgroupsDataService();
            Supplierproductgroups result = await Task<Supplierproductgroups>.Run(() => SupplierproductgroupsData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        public async Task<IHttpActionResult> Update(ActionRequest<Supplierproductgroups> request)
        {
            SupplierproductgroupsDataService SupplierproductgroupsData = new SupplierproductgroupsDataService();
            Supplierproductgroups result = await Task<Supplierproductgroups>.Run(() => SupplierproductgroupsData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IHttpActionResult> Delete(ActionRequest<Supplierproductgroups> request)
        {
            SupplierproductgroupsDataService SupplierproductgroupsData = new SupplierproductgroupsDataService();
            bool result = await Task<bool>.Run(() => SupplierproductgroupsData.Delete(request.Item));
            return this.Ok(result);
        }
    }
}