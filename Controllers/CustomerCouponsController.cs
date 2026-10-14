
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
    [RoutePrefix("VouchersService")]
    public class CustomerCouponsServiceController : ApiController
    {

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<CustomerCoupons>();
            result.Item = new CustomerCoupons();
            return this.Ok(result);
        }

        [HttpPost]
        [Route("SearchCoupen")]
        public async Task<IHttpActionResult> SearchCoupen(ActionRequest<CustomerCouponsCreteria> request)
        {
            CustomerCouponsDataService VouchersData = new CustomerCouponsDataService();
            CustomerCouponsStatus customerCouponsStatus = new CustomerCouponsStatus();

            if (!string.IsNullOrEmpty(InfyPOS.Services.ConfigManager.Instance(VouchersData.Context.OrganizationID).Config.integration_coupon))
            {
                CouponManager couponManager = new CouponManager(InfyPOS.Services.ConfigManager.Instance(VouchersData.Context.OrganizationID).Config.integration_coupon);
                bool hascoupon = couponManager.HasCoupon(request.Item.code);
                if(!hascoupon)
                {
                    customerCouponsStatus.reason = "Not found";
                    return this.Ok(customerCouponsStatus);
                }
            }

            List<CustomerCoupons> result = await Task<List<CustomerCoupons>>.Run(() => VouchersData.Search(request.Item));
            if (result.Count == 1)
            {
                result[0].vouchertype = CustomerCoupons.CouponType.Coupon;
                customerCouponsStatus = VouchersData.FindStatus(result[0], true, request.Item.bill, request.Item.billvalue);
            }
            else
            {
                customerCouponsStatus.reason = "Not found";
            }

            return this.Ok(customerCouponsStatus);
        }

        [HttpPost]
        [Route("SearchVouchers")]
        public async Task<IHttpActionResult> SearchVouchers(ActionRequest<CustomerCouponsCreteria> request)
        {
            CustomerCouponsDataService VouchersData = new CustomerCouponsDataService();
            List<CustomerCoupons> result = await Task<List<CustomerCoupons>>.Run(() => VouchersData.SearchVouchers(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<CustomerCouponsCreteria> request)
        {
            CustomerCouponsDataService VouchersData = new CustomerCouponsDataService();
            List<CustomerCoupons> result = await Task<List<CustomerCoupons>>.Run(() => VouchersData.Search(request.Item));
            return this.Ok(result);
        }

        [HttpGet]
        [Route("FreeCoupon/{code}")]
        public async Task<IHttpActionResult> FreeCoupon(string code)
        {
            CustomerCouponsDataService VouchersData = new CustomerCouponsDataService();
            List<CustomerCoupons> result = await Task<List<CustomerCoupons>>.Run(() => VouchersData.Search(new CustomerCouponsCreteria()
            {
                code = code,
                issueto = -1
            }));
            return this.Ok(result);
        }

        [HttpGet]
        [Route("IssuedCoupon/{code}")]
        public async Task<IHttpActionResult> IssuedCoupon(string code)
        {
            CustomerCouponsDataService VouchersData = new CustomerCouponsDataService();
            List<CustomerCoupons> result = await Task<List<CustomerCoupons>>.Run(() => VouchersData.Search(new CustomerCouponsCreteria()
            {
                code = code
            }));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Save")]
        public async Task<IHttpActionResult> Save(ActionRequest<CustomerCoupons> request)
        {
            CustomerCouponsDataService VouchersData = new CustomerCouponsDataService();
            if (request.Item.id <= 0)
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Create);
                CustomerCoupons result = await Task<CustomerCoupons>.Run(() => VouchersData.Insert(request.Item));
                return this.Ok(result);
            }
            else
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Update);
                CustomerCoupons result = await Task<CustomerCoupons>.Run(() => VouchersData.Update(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("Insert")]
        public async Task<IHttpActionResult> Insert(ActionRequest<CustomerCoupons> request)
        {
            CustomerCouponsDataService VouchersData = new CustomerCouponsDataService();
            CustomerCoupons result = await Task<CustomerCoupons>.Run(() => VouchersData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        public async Task<IHttpActionResult> Update(ActionRequest<CustomerCoupons> request)
        {
            CustomerCouponsDataService VouchersData = new CustomerCouponsDataService();
            CustomerCoupons result = await Task<CustomerCoupons>.Run(() => VouchersData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IHttpActionResult> Delete(ActionRequest<CustomerCoupons> request)
        {
            CustomerCouponsDataService VouchersData = new CustomerCouponsDataService();
            bool result = await Task<bool>.Run(() => VouchersData.Delete(request.Item));
            return this.Ok(result);
        }
    }
}