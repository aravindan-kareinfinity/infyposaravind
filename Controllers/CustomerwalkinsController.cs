
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
    [RoutePrefix("CustomerwalkinsService")]
    public class CustomerwalkinsServiceController : ApiController
    {
        [AllowAnonymous]
        [HttpGet]
        [Route("Entity/{id}")]
        public async Task<IHttpActionResult> Entity(string id)
        {
            if (id == "Save")
            {
                var result = new ActionRequest<Customerwalkins>();
                result.Item = new Customerwalkins();
                return this.Ok(result);
            }
            else
            {
                var result = new ActionMulitipleResponse<CustomerWalkinSummary>();
                result.Items = new List<CustomerWalkinSummary>();
                result.Items.Add(new CustomerWalkinSummary());
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<CustomerwalkinsCreteria> request)
        {
            CustomerwalkinsDataService CustomerwalkinsData = new CustomerwalkinsDataService();
            List<Customerwalkins> result = await Task<List<Customerwalkins>>.Run(() => CustomerwalkinsData.Search(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("SearchCustomerByMobileNo")]
        public async Task<IHttpActionResult> SearchCustomerByMobileNo(ActionRequest<CustomerwalkinsCreteria> request)
        {
            CustomerwalkinsDataService CustomerwalkinsData = new CustomerwalkinsDataService();
            Customerwalkins result = await Task<Customerwalkins>.Run(() => CustomerwalkinsData.SearchCustomerByMobileNo(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("SearchCustomerByBillNo")]
        public async Task<IHttpActionResult> SearchCustomerByBillNo(ActionRequest<CustomerwalkinsCreteria> request)
        {
            CustomerwalkinsDataService CustomerwalkinsData = new CustomerwalkinsDataService();
            Customerwalkins result = await Task<Customerwalkins>.Run(() => CustomerwalkinsData.SearchCustomerByBillNo(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("SearchSummary")]
        public async Task<IHttpActionResult> SearchSummary(ActionRequest<CustomerwalkinsCreteria> request)
        {
            CustomerwalkinsDataService CustomerwalkinsData = new CustomerwalkinsDataService();
            List<CustomerWalkinSummary> result = await Task<List<CustomerWalkinSummary>>.Run(() => CustomerwalkinsData.SearchSummary(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Save")]
        public async Task<IHttpActionResult> Save(ActionRequest<Customerwalkins> request)
        {
            CustomerwalkinsDataService CustomerwalkinsData = new CustomerwalkinsDataService();
            if (request.Item.id <= 0)
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Create);
                Customerwalkins result = await Task<Customerwalkins>.Run(() => CustomerwalkinsData.Insert(request.Item));
                return this.Ok(result);
            }
            else
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Update);
                Customerwalkins result = await Task<Customerwalkins>.Run(() => CustomerwalkinsData.Update(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("Insert")]
        public async Task<IHttpActionResult> Insert(ActionRequest<Customerwalkins> request)
        {
            CustomerwalkinsDataService CustomerwalkinsData = new CustomerwalkinsDataService();
            Customerwalkins result = await Task<Customerwalkins>.Run(() => CustomerwalkinsData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("DirectEntry")]
        public async Task<IHttpActionResult> DirectEntry(ActionRequest<Customerwalkins> request)
        {
            
            CustomerwalkinsDataService CustomerwalkinsData = new CustomerwalkinsDataService();
            request.Item.fromtime = CustomerwalkinsData.DateTimeNow;
            StoretransactionDataService storetransactionDataService = new StoretransactionDataService();
            storetransactionDataService.UpdateWalkinToken(request.Item);

            if (request.Item.walkincount > 0)
            {
                Customerwalkins result = await Task<Customerwalkins>.Run(() => CustomerwalkinsData.Insert(new Customerwalkins()
                {
                    walkincount = request.Item.walkincount,
                    isactive = true,
                    modifiedby = CustomerwalkinsData.Context.UserID,
                    createdby = CustomerwalkinsData.Context.UserID,
                    createdon = CustomerwalkinsData.DateTimeNow,
                    modifiedon = CustomerwalkinsData.DateTimeNow,
                    version = 1,
                    fromtime = CustomerwalkinsData.DateTimeNow,
                    totime = CustomerwalkinsData.DateTimeNow,
                    locationid = request.Item.locationid
                }));
                return this.Ok(result);
            }
            else
            {
                return this.Ok("Heigher token number already received");
            }
        }

        [HttpPost]
        [Route("Update")]
        public async Task<IHttpActionResult> Update(ActionRequest<Customerwalkins> request)
        {
            CustomerwalkinsDataService CustomerwalkinsData = new CustomerwalkinsDataService();
            Customerwalkins result = await Task<Customerwalkins>.Run(() => CustomerwalkinsData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IHttpActionResult> Delete(ActionRequest<Customerwalkins> request)
        {
            CustomerwalkinsDataService CustomerwalkinsData = new CustomerwalkinsDataService();
            bool result = await Task<bool>.Run(() => CustomerwalkinsData.Delete(request.Item));
            return this.Ok(result);
        }
    }
}