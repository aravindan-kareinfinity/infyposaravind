
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
    [RoutePrefix("MfgbinstoragestockService")]
    public class MfgbinstoragestockServiceController : ApiController
    {

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<Mfgbinstoragestock>();
            result.Item = new Mfgbinstoragestock();
            return this.Ok(result);
        }

        [HttpGet]
        [Route("SearchBinStock/{parentid}/{id}")]
        public async Task<IHttpActionResult> Search(string parentid, string id)
        {
            MfgbinstorageDataService MfgbinstoragestockData = new MfgbinstorageDataService();
            List<Mfgbinstorage> result = await Task<List<Mfgbinstorage>>.Run(() => MfgbinstoragestockData.Search(new MfgbinstorageCreteria()
            {
                code = id,
                parentid = parentid
            }));
            if (result.Count == 1)
            {
                var stockconfig = result.First();
                stockconfig.stocklist = GetStorageStockSummary(stockconfig.id);
                return this.Ok(stockconfig);
            }
            return this.Ok(new Mfgbinstorage());
        }

        public List<Mfgbinstoragestockbase> GetStorageStockSummary(long mfgbinstorageid)
        {
            MfgbinstoragestockDataService mfgbinstoragestockDataService = new MfgbinstoragestockDataService();

            return mfgbinstoragestockDataService.Search(new MfgbinstoragestockCreteria()
            {
                mfgbinstorageid = mfgbinstorageid,
                onlystock = true
            }).GroupBy(e => new { e.colourname, e.sizename, e.uid, e.designname, e.sizegroup, e.product }).ToList().ConvertAll(ex =>
            {
                return new Mfgbinstoragestockbase()
                {
                    product = ex.Key.product,
                    colourname = ex.Key.colourname,
                    sizename = ex.Key.sizename,
                    sizegroup = ex.Key.sizegroup,
                    uid = ex.Key.uid,
                    designname = ex.Key.designname,
                    stock = ex.Sum(st => st.stock)
                };
            });
        }

        [HttpGet]
        [Route("SearchAvailableStock/{uan}")]
        public async Task<IHttpActionResult> SearchAvailableStock(string uan)
        {
            MfgbinstoragestockDataService MfgbinstoragestockData = new MfgbinstoragestockDataService();
            List<Mfgbinstoragestock.StockAvailable> result = await Task<List<Mfgbinstoragestock.StockAvailable>>.Run(() => MfgbinstoragestockData.SearchAvailableStock(uan));
            Mfgbinstoragestock.StockAvailableSummary stockAvailableSummary = new Mfgbinstoragestock.StockAvailableSummary();
            stockAvailableSummary.items = result;
            stockAvailableSummary.allocated = result.Sum(e => e.allocated);
            stockAvailableSummary.stockqty = result.Sum(e => e.stockqty);
            stockAvailableSummary.balance = stockAvailableSummary.stockqty - stockAvailableSummary.allocated;
            return this.Ok(stockAvailableSummary);
        }

        [HttpPost]
        [Route("Add2Bin")]
        public async Task<IHttpActionResult> Add2Bin(ActionRequest<MfgbinstorageChange> request)
        {
            MfgbinstoragestockDataService MfgbinstoragestockData = new MfgbinstoragestockDataService();
            List<Mfgbinstoragestockbase> result = await Task<List<Mfgbinstoragestockbase>>.Run(() =>
            {
                MfgbinstoragestockData.Add2Bin(request.Item);
                return GetStorageStockSummary(request.Item.storage.id);
            });
            if(result.Exists(e=>e.uid == request.Item.uan))
            {
                var item = result.Find(e => e.uid == request.Item.uan);
                result.Remove(item);
                result.Insert(0, item);
            }
            return this.Ok(result);
        }

        [HttpPost]
        [Route("RemovefromBin")]
        public async Task<IHttpActionResult> RemovefromBin(ActionRequest<MfgbinstorageChange> request)
        {
            MfgbinstoragestockDataService MfgbinstoragestockData = new MfgbinstoragestockDataService();
            List<Mfgbinstoragestockbase> result = await Task<List<Mfgbinstoragestockbase>>.Run(() =>
            {
                MfgbinstoragestockData.RemovefromBin(request.Item);
                return GetStorageStockSummary(request.Item.storage.id);
            });
            if (result.Exists(e => e.uid == request.Item.uan))
            {
                var item = result.Find(e => e.uid == request.Item.uan);
                result.Remove(item);
                result.Insert(0, item);
            }
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<MfgbinstoragestockCreteria> request)
        {
            MfgbinstoragestockDataService MfgbinstoragestockData = new MfgbinstoragestockDataService();
            List<Mfgbinstoragestock> result = await Task<List<Mfgbinstoragestock>>.Run(() => MfgbinstoragestockData.Search(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Save")]
        public async Task<IHttpActionResult> Save(ActionRequest<Mfgbinstoragestock> request)
        {
            MfgbinstoragestockDataService MfgbinstoragestockData = new MfgbinstoragestockDataService();
            if (request.Item.id <= 0)
            {
                Mfgbinstoragestock result = await Task<Mfgbinstoragestock>.Run(() => MfgbinstoragestockData.Insert(request.Item));
                return this.Ok(result);
            }
            else
            {
                Mfgbinstoragestock result = await Task<Mfgbinstoragestock>.Run(() => MfgbinstoragestockData.Update(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("Insert")]
        public async Task<IHttpActionResult> Insert(ActionRequest<Mfgbinstoragestock> request)
        {
            MfgbinstoragestockDataService MfgbinstoragestockData = new MfgbinstoragestockDataService();
            Mfgbinstoragestock result = await Task<Mfgbinstoragestock>.Run(() => MfgbinstoragestockData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        public async Task<IHttpActionResult> Update(ActionRequest<Mfgbinstoragestock> request)
        {
            MfgbinstoragestockDataService MfgbinstoragestockData = new MfgbinstoragestockDataService();
            Mfgbinstoragestock result = await Task<Mfgbinstoragestock>.Run(() => MfgbinstoragestockData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IHttpActionResult> Delete(ActionRequest<Mfgbinstoragestock> request)
        {
            MfgbinstoragestockDataService MfgbinstoragestockData = new MfgbinstoragestockDataService();
            bool result = await Task<bool>.Run(() => MfgbinstoragestockData.Delete(request.Item));
            return this.Ok(result);
        }
    }
}