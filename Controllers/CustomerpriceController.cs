
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
    [RoutePrefix("CustomerpriceService")]
    public class CustomerpriceServiceController : ApiController
    {

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<Customerprice>();
            result.Item = new Customerprice();
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<CustomerpriceCreteria> request)
        {
            CustomerpriceDataService CustomerpriceData = new CustomerpriceDataService();
            List<Customerprice> result = await Task<List<Customerprice>>.Run(() => CustomerpriceData.Search(request.Item));
            return this.Ok(result);
        }


        [HttpGet]
        [Route("SearchAll/{customerid?}")]
        public async Task<IHttpActionResult> Barcode(string customerid = null)
        {
            var item = new CustomerpriceCreteria();
            if (string.IsNullOrEmpty(customerid))
                item.allcategory = true;
            else
            {
                item.customerids = new long[1];
                item.customerids[0] = long.Parse(customerid);
            }
            CustomerpriceDataService CustomerpriceData = new CustomerpriceDataService();
            List<DesignPriceItem> result = await Task<List<DesignPriceItem>>.Run(() =>
                CustomerpriceData.SearchDesignPrice(item));
            return this.Ok(result);
        }
        [HttpPost]
        [Route("SearchAll")]
        public async Task<IHttpActionResult> SearchAll(ActionRequest<CustomerpriceCreteria> request)
        {
            CustomerpriceDataService CustomerpriceData = new CustomerpriceDataService();
            List<CustomerPriceItem> result = await Task<List<CustomerPriceItem>>.Run(() => CustomerpriceData.SearchAll(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("SaveAll")]
        public async Task<IHttpActionResult> SaveAll(ActionMulitpleRequest<CustomerPriceItem> request)
        {
            CustomerpriceDataService CustomerpriceData = new CustomerpriceDataService();
            List<CustomerPriceItem> result = await Task<List<CustomerPriceItem>>.Run(() => CustomerpriceData.SaveAll(request.Items));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Save")]
        public async Task<IHttpActionResult> Save(ActionRequest<Customerprice> request)
        {
            CustomerpriceDataService CustomerpriceData = new CustomerpriceDataService();
            if (request.Item.id <= 0)
            {
                Customerprice result = await Task<Customerprice>.Run(() => CustomerpriceData.Insert(request.Item));
                return this.Ok(result);
            }
            else
            {
                Customerprice result = await Task<Customerprice>.Run(() => CustomerpriceData.Update(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("Insert")]
        public async Task<IHttpActionResult> Insert(ActionRequest<Customerprice> request)
        {
            CustomerpriceDataService CustomerpriceData = new CustomerpriceDataService();
            Customerprice result = await Task<Customerprice>.Run(() => CustomerpriceData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        public async Task<IHttpActionResult> Update(ActionRequest<Customerprice> request)
        {
            CustomerpriceDataService CustomerpriceData = new CustomerpriceDataService();
            Customerprice result = await Task<Customerprice>.Run(() => CustomerpriceData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IHttpActionResult> Delete(ActionRequest<Customerprice> request)
        {
            CustomerpriceDataService CustomerpriceData = new CustomerpriceDataService();
            bool result = await Task<bool>.Run(() => CustomerpriceData.Delete(request.Item));
            return this.Ok(result);
        }
    }
}