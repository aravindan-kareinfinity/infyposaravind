
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
    [RoutePrefix("PackbarcodeService")]
    public class PackbarcodeServiceController : ApiController
    {

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<Packbarcode>();
            result.Item = new Packbarcode();
            return this.Ok(result);
        }


        [HttpGet]
        [Route("SearchPack/{locationid}/{id}/{id1?}/{id2?}/{id3?}")]
        public async Task<IHttpActionResult> SearchPack(string locationid, string id,
           string id1 = null, string id2 = null, string id3 = null)
        {
            PackingslipDataService StockData = new PackingslipDataService();
            if (!string.IsNullOrEmpty(id1))
                id += "/" + id1;
            if (!string.IsNullOrEmpty(id2))
                id += "/" + id2;
            if (!string.IsNullOrEmpty(id3))
                id += "/" + id3;

            if (id.IndexOf("~dot~") >= 0) id = id.Replace("~dot~", ".");
            if (id.IndexOf("~sls~") >= 0) id = id.Replace("~sls~", "/");
            PackbarcodeDataService PackbarcodeData = new PackbarcodeDataService();
            List<Packbarcode> result = await Task<List<Packbarcode>>.Run(() => PackbarcodeData.Search(new PackbarcodeCreteria()
            {
                barcode = id
            }));
            return this.Ok(result.Count == 1 ? result.First() : null);
        }
        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<PackbarcodeCreteria> request)
        {
            PackbarcodeDataService PackbarcodeData = new PackbarcodeDataService();
            List<Packbarcode> result = await Task<List<Packbarcode>>.Run(() => PackbarcodeData.Search(request.Item));
            if(result.Count == 0 && request.Item.sourcesearch)
            {
                request.Item.sourcesearch = false;
                result = await Task<List<Packbarcode>>.Run(() => PackbarcodeData.Search(request.Item));
            }
            return this.Ok(result);
        }
        [HttpPost]
        [Route("SearchHistory")]
        public async Task<IHttpActionResult> SearchHistory(ActionRequest<PackbarcodeHistory> request)
        {
            PackbarcodeDataService PackbarcodeData = new PackbarcodeDataService();
            List<PackbarcodeHistory> result = await Task<List<PackbarcodeHistory>>.Run(() => PackbarcodeData.SearchHistory(request.Item));
            return this.Ok(result);
        }
        [HttpPost]
        [Route("CreateLastBits")]
        public async Task<IHttpActionResult> CreateLastBits(ActionRequest<PackbarcodeCreteria> request)
        {
            PackbarcodeDataService PackbarcodeData = new PackbarcodeDataService();
            Packbarcode result = await Task<Packbarcode>.Run(() => PackbarcodeData.CreateLastBits(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("CreateBarcode")]
        public async Task<IHttpActionResult> CreateBarcode(ActionRequest<PackbarcodeCreteria> request)
        {
            PackbarcodeDataService PackbarcodeData = new PackbarcodeDataService();
            Packbarcode result = await Task<Packbarcode>.Run(() => PackbarcodeData.CreateBarcode(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("CreatePackcode")]
        public async Task<IHttpActionResult> CreatePackcode(ActionRequest<PackbarcodeCreteria> request)
        {
            PackbarcodeDataService PackbarcodeData = new PackbarcodeDataService();

            Packbarcode result = await Task<Packbarcode>.Run(() => PackbarcodeData.CreatePackcode(request.Item));
            return this.Ok(result);

        }

        [HttpPost]
        [Route("Update")]
        public async Task<IHttpActionResult> Update(ActionRequest<Packbarcode> request)
        {
            PackbarcodeDataService PackbarcodeData = new PackbarcodeDataService();
            Packbarcode result = await Task<Packbarcode>.Run(() => PackbarcodeData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IHttpActionResult> Delete(ActionRequest<Packbarcode> request)
        {
            PackbarcodeDataService PackbarcodeData = new PackbarcodeDataService();
            bool result = await Task<bool>.Run(() => PackbarcodeData.Delete(request.Item));
            return this.Ok(result);
        }
    }
}