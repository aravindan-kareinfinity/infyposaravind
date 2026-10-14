
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
    [RoutePrefix("DiscountschemeService")]
    public class DiscountschemeServiceController : ApiController
    {

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<Discountscheme>();
            result.Item = new Discountscheme();
            return this.Ok(result);
        }

        [HttpPost]
        [Route("SearchItems")]
        public async Task<IHttpActionResult> SearchItems(ActionRequest<Discountscheme.FilterCreteria> request)
        {
            DiscountschemeDataService stockdataservice = new DiscountschemeDataService();
            List<Discountscheme.DiscountItem> result = await Task<List<Discountscheme.DiscountItem>>.Run(() => stockdataservice.SearchItems(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<DiscountschemeCreteria> request)
        {
            DiscountschemeDataService DiscountschemeData = new DiscountschemeDataService();
            List<Discountscheme> result = await Task<List<Discountscheme>>.Run(() => DiscountschemeData.Search(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("ChangeDiscount")]
        public async Task<IHttpActionResult> ChangeDiscount(ActionRequest<Discountscheme> request)
        {
            DiscountschemeDataService DiscountschemeData = new DiscountschemeDataService();
            Discountscheme result = await Task<Discountscheme>.Run(() => DiscountschemeData.UpdateStatus(request.Item));
            return this.Ok(result);
        }

        
        [HttpPost]
        [Route("Save")]
        public async Task<IHttpActionResult> Save(ActionRequest<DiscountschemeCover> request)
        {
            DiscountschemeDataService DiscountschemeData = new DiscountschemeDataService();
            if (request.Item.Scheme.id <= 0)
            {
                request.Item.Scheme.ChangeStatus(BaseEntity.DataStatus.Create);
                DiscountschemeCover result = await Task<DiscountschemeCover>.Run(() => DiscountschemeData.Insert(request.Item));
                return this.Ok(result);
            }
            else
            {
                request.Item.Scheme.ChangeStatus(BaseEntity.DataStatus.Update);
                DiscountschemeCover result = await Task<DiscountschemeCover>.Run(() => DiscountschemeData.Update(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("Insert")]
        public async Task<IHttpActionResult> Insert(ActionRequest<DiscountschemeCover> request)
        {
            DiscountschemeDataService DiscountschemeData = new DiscountschemeDataService();
            DiscountschemeCover result = await Task<DiscountschemeCover>.Run(() => DiscountschemeData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        public async Task<IHttpActionResult> Update(ActionRequest<DiscountschemeCover> request)
        {
            DiscountschemeDataService DiscountschemeData = new DiscountschemeDataService();
            DiscountschemeCover result = await Task<DiscountschemeCover>.Run(() => DiscountschemeData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IHttpActionResult> Delete(ActionRequest<Discountscheme> request)
        {
            DiscountschemeDataService DiscountschemeData = new DiscountschemeDataService();
            bool result = await Task<bool>.Run(() => DiscountschemeData.Delete(request.Item));
            return this.Ok(result);
        }
    }
}