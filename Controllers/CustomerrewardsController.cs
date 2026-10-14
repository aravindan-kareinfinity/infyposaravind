
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
    [RoutePrefix("CustomerrewardsService")]
    public class CustomerrewardsServiceController : ApiController
    {

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<Customerrewards>();
            result.Item = new Customerrewards();
            return this.Ok(result);
        }


        [HttpPost]
        [Route("SearchCustomerReward")]
        public async Task<IHttpActionResult> SearchCustomerReward(ActionRequest<CustomerrewardsCreteria> request)
        {
            CustomerrewardsDataService CustomerrewardsData = new CustomerrewardsDataService();
            CustomerRewardStatus result = await Task<CustomerRewardStatus>.Run(() => CustomerrewardsData.SearchCustomerReward(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("GenerateCoupon")]
        public async Task<IHttpActionResult> GenerateCoupon(ActionRequest<Customer> request)
        {
            CustomerrewardsDataService CustomerrewardsData = new CustomerrewardsDataService();
            string result = await Task<CustomerRewardStatus>.Run(() => CustomerrewardsData.GenerateCoupon(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("SearchReward")]
        public async Task<IHttpActionResult> SearchReward(ActionRequest<CustomerrewardsCreteria> request)
        {
            CustomerrewardsDataService CustomerrewardsData = new CustomerrewardsDataService();
            CustomerRewardStatus result = await Task<CustomerRewardStatus>.Run(() => CustomerrewardsData.SearchReward(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<CustomerrewardsCreteria> request)
        {
            CustomerrewardsDataService CustomerrewardsData = new CustomerrewardsDataService();
            List<Customerrewards> result = await Task<List<Customerrewards>>.Run(() => CustomerrewardsData.Search(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Save")]
        public async Task<IHttpActionResult> Save(ActionRequest<Customerrewards> request)
        {
            CustomerrewardsDataService CustomerrewardsData = new CustomerrewardsDataService();
            if (request.Item.id <= 0)
            {
                Customerrewards result = await Task<Customerrewards>.Run(() => CustomerrewardsData.Insert(request.Item));
                return this.Ok(result);
            }
            else
            {
                Customerrewards result = await Task<Customerrewards>.Run(() => CustomerrewardsData.Update(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("UpdateRewards")]
        public async Task<IHttpActionResult> UpdateRewards(ActionRequest<CustomerBills> request)
        {
            CustomerrewardsDataService CustomerrewardsData = new CustomerrewardsDataService();
            CustomerBills result = await Task<CustomerBills>.Run(() => CustomerrewardsData.UpdateRewards(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Insert")]
        public async Task<IHttpActionResult> Insert(ActionRequest<Customerrewards> request)
        {
            CustomerrewardsDataService CustomerrewardsData = new CustomerrewardsDataService();
            Customerrewards result = await Task<Customerrewards>.Run(() => CustomerrewardsData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        public async Task<IHttpActionResult> Update(ActionRequest<Customerrewards> request)
        {
            CustomerrewardsDataService CustomerrewardsData = new CustomerrewardsDataService();
            Customerrewards result = await Task<Customerrewards>.Run(() => CustomerrewardsData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IHttpActionResult> Delete(ActionRequest<Customerrewards> request)
        {
            CustomerrewardsDataService CustomerrewardsData = new CustomerrewardsDataService();
            bool result = await Task<bool>.Run(() => CustomerrewardsData.Delete(request.Item));
            return this.Ok(result);
        }
    }
}