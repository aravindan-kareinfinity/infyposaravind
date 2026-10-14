
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
    [RoutePrefix("SellablebarcodesService")]
    public class SellablebarcodesServiceController : ApiController
    {

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<Sellablebarcodes>();
            result.Item = new Sellablebarcodes();
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<SellablebarcodesCreteria> request)
        {
            SellablebarcodesDataService SellablebarcodesData = new SellablebarcodesDataService();
            List<Sellablebarcodes> result = await Task<List<Sellablebarcodes>>.Run(() => SellablebarcodesData.Search(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Save")]
        public async Task<IHttpActionResult> Save(ActionRequest<Sellablebarcodes> request)
        {
            SellablebarcodesDataService SellablebarcodesData = new SellablebarcodesDataService();
            if (request.Item.id <= 0)
            {
                Sellablebarcodes result = await Task<Sellablebarcodes>.Run(() => SellablebarcodesData.Insert(request.Item));
                return this.Ok(result);
            }
            else
            {
                Sellablebarcodes result = await Task<Sellablebarcodes>.Run(() => SellablebarcodesData.Update(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("Insert")]
        public async Task<IHttpActionResult> Insert(ActionRequest<Sellablebarcodes> request)
        {
            SellablebarcodesDataService SellablebarcodesData = new SellablebarcodesDataService();
            Sellablebarcodes result = await Task<Sellablebarcodes>.Run(() => SellablebarcodesData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        public async Task<IHttpActionResult> Update(ActionRequest<Sellablebarcodes> request)
        {
            SellablebarcodesDataService SellablebarcodesData = new SellablebarcodesDataService();
            Sellablebarcodes result = await Task<Sellablebarcodes>.Run(() => SellablebarcodesData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IHttpActionResult> Delete(ActionRequest<Sellablebarcodes> request)
        {
            SellablebarcodesDataService SellablebarcodesData = new SellablebarcodesDataService();
            bool result = await Task<bool>.Run(() => SellablebarcodesData.Delete(request.Item));
            return this.Ok(result);
        }
    }
}