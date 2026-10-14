
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
    [RoutePrefix("FullstocksyncService")]
    public class FullstocksyncServiceController : ApiController
    {

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<Fullstocksync>();
            result.Item = new Fullstocksync();
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<FullstocksyncCreteria> request)
        {
            FullstocksyncDataService FullstocksyncData = new FullstocksyncDataService();
            List<Fullstocksync> result = await Task<List<Fullstocksync>>.Run(() => FullstocksyncData.Search(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("SearchFullStockConflict")]
        [WebAPI.InfyAuthorize(PermissionSet.stockadjustment, InfyPOS.Common.Action.View)]
        public async Task<IHttpActionResult> SearchFullStockConflict(ActionRequest<FullstocksyncDataService.SingleStockFile> request)
        {
            FullstocksyncDataService RetailReportData = new FullstocksyncDataService();
            List<FullstocksyncDataService.FullStockSummary.FullStockMismatchesItem> result = await Task<List<FullstocksyncDataService.FullStockSummary.FullStockMismatchesItem>>.Run(() => RetailReportData.SearchFullStockConflict(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("SearchFullStock")]
        [WebAPI.InfyAuthorize(PermissionSet.stockadjustment, InfyPOS.Common.Action.View)]
        public async Task<IHttpActionResult> SearchFullStock(ActionRequest<FullstocksyncDataService.SingleStockFile> request)
        {
            FullstocksyncDataService RetailReportData = new FullstocksyncDataService();
            FullstocksyncDataService.FullStockSummary result = await Task<FullstocksyncDataService.FullStockSummary>.Run(() => RetailReportData.SearchFullStock(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("SyncFullStock")]
        [WebAPI.InfyAuthorize(PermissionSet.stockadjustment, InfyPOS.Common.Action.Create)]
        public async Task<IHttpActionResult> SyncFullStock(ActionRequest<FullstocksyncDataService.SingleStockFile> request)
        {
            FullstocksyncDataService RetailReportData = new FullstocksyncDataService();
            bool result = await Task<bool>.Run(() => RetailReportData.SyncFullStock(request.Item));
            return this.Ok(result);
        }

    }
}