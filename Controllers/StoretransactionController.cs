
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
    [RoutePrefix("StoretransactionService")]
    public class StoretransactionServiceController : ApiController
    {

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<Storetransaction>();
            result.Item = new Storetransaction();
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<StoretransactionCreteria> request)
        {
            StoretransactionDataService StoretransactionData = new StoretransactionDataService();
            List<Storetransaction> result = await Task<List<Storetransaction>>.Run(() => StoretransactionData.Search(request.Item));
            return this.Ok(result);
        }


        [HttpPost]
        [Route("WalkinSummary")]
        public async Task<IHttpActionResult> WalkinSummary(ActionRequest<Storetransaction> request)
        {
            StoretransactionDataService StoretransactionData = new StoretransactionDataService();
            WalkinSummary result = await Task<WalkinSummary>.Run(() => StoretransactionData.WalkinSummary(request.Item));
            return this.Ok(result);
        }


        [HttpPost]
        [Route("SaveOutOfStock")]
        public async Task<IHttpActionResult> SaveOutOfStock(ActionRequest<Storetransaction> request)
        {
            StoretransactionDataService StoretransactionData = new StoretransactionDataService();
            Storetransaction result = await Task<Storetransaction>.Run(() => StoretransactionData.SaveOutOfStock(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("SearchOutOfStock")]
        public async Task<IHttpActionResult> SearchOutOfStock(ActionRequest<Storetransaction> request)
        {
            StoretransactionDataService StoretransactionData = new StoretransactionDataService();
            Storetransaction result = await Task<Storetransaction>.Run(() => StoretransactionData.SearchOutOfStock(request.Item));
            return this.Ok(result);
        }


        [HttpPost]
        [Route("StoreReturnBarcode")]
        public async Task<IHttpActionResult> StoreReturnBarcode(ActionRequest<Storetransaction> request)
        {
            StoretransactionDataService StoretransactionData = new StoretransactionDataService();
            Storetransaction.StoreReturnBarcodes result = await Task<Storetransaction.StoreReturnBarcodes>.Run(() => StoretransactionData.StoreReturnBarcode(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("CreateToken")]
        public async Task<IHttpActionResult> CreateToken(ActionRequest<StoretransactionCreteria> request)
        {
            StoretransactionDataService StoretransactionData = new StoretransactionDataService();

            StoretransactionCreteria result = await Task<StoretransactionCreteria>.Run(() => StoretransactionData.CreateToken(request.Item));
            return this.Ok(result);

        }

        [HttpPost]
        [Route("PrintToken")]
        public async Task<IHttpActionResult> PrintToken(ActionRequest<StoretransactionCreteria> request)
        {
            StoretransactionDataService StoretransactionData = new StoretransactionDataService();
            if (request.Item.type == "Referral")
            {
                StoretransactionCreteria result = await Task<StoretransactionCreteria>.Run(() => StoretransactionData.PrintReferralToken(request.Item));
                return this.Ok(result);
            }
            else
            {
                StoretransactionCreteria result = await Task<StoretransactionCreteria>.Run(() => StoretransactionData.PrintToken(request.Item));
                return this.Ok(result);
            }
        }



        [HttpPost]
        [Route("Save")]
        public async Task<IHttpActionResult> Save(ActionRequest<Storetransaction> request)
        {
            StoretransactionDataService StoretransactionData = new StoretransactionDataService();
            if (request.Item.id <= 0)
            {
                Storetransaction result = await Task<Storetransaction>.Run(() => StoretransactionData.Insert(request.Item));
                return this.Ok(result);
            }
            else
            {
                Storetransaction result = await Task<Storetransaction>.Run(() => StoretransactionData.Update(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("Insert")]
        public async Task<IHttpActionResult> Insert(ActionRequest<Storetransaction> request)
        {
            StoretransactionDataService StoretransactionData = new StoretransactionDataService();
            Storetransaction result = await Task<Storetransaction>.Run(() => StoretransactionData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        public async Task<IHttpActionResult> Update(ActionRequest<Storetransaction> request)
        {
            StoretransactionDataService StoretransactionData = new StoretransactionDataService();
            Storetransaction result = await Task<Storetransaction>.Run(() => StoretransactionData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IHttpActionResult> Delete(ActionRequest<Storetransaction> request)
        {
            StoretransactionDataService StoretransactionData = new StoretransactionDataService();
            bool result = await Task<bool>.Run(() => StoretransactionData.Delete(request.Item));
            return this.Ok(result);
        }

        [HttpGet]
        [Route("ReferralToken/{locationid}/{id}")]
        public async Task<IHttpActionResult> ReferralToken(string locationid, string id)
        {
            StoretransactionDataService StoretransactionData = new StoretransactionDataService();
            Storetransaction.ReferralToken.PrintCount result = await Task<Storetransaction.ReferralToken.PrintCount>.Run(() => StoretransactionData.SearchReferralToken(locationid, id));
            return this.Ok(result);
        }
    }
}