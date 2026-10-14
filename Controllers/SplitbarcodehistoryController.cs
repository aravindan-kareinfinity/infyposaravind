
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
    [RoutePrefix("SplitbarcodehistoryService")]
    public class SplitbarcodehistoryServiceController : ApiController
    {

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<Splitbarcodehistory>();
            result.Item = new Splitbarcodehistory();
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<SplitbarcodehistoryCreteria> request)
        {
            SplitbarcodehistoryDataService SplitbarcodehistoryData = new SplitbarcodehistoryDataService();
            List<Splitbarcodehistory> result = await Task<List<Splitbarcodehistory>>.Run(() => SplitbarcodehistoryData.Search(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Save")]
        public async Task<IHttpActionResult> Save(ActionRequest<Splitbarcodehistory> request)
        {
            SplitbarcodehistoryDataService SplitbarcodehistoryData = new SplitbarcodehistoryDataService();
            if (request.Item.id <= 0)
            {
                Splitbarcodehistory result = await Task<Splitbarcodehistory>.Run(() => SplitbarcodehistoryData.Insert(request.Item));
                return this.Ok(result);
            }
            else
            {
                Splitbarcodehistory result = await Task<Splitbarcodehistory>.Run(() => SplitbarcodehistoryData.Update(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("Insert")]
        public async Task<IHttpActionResult> Insert(ActionRequest<Splitbarcodehistory> request)
        {
            SplitbarcodehistoryDataService SplitbarcodehistoryData = new SplitbarcodehistoryDataService();
            Splitbarcodehistory result = await Task<Splitbarcodehistory>.Run(() => SplitbarcodehistoryData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        public async Task<IHttpActionResult> Update(ActionRequest<Splitbarcodehistory> request)
        {
            SplitbarcodehistoryDataService SplitbarcodehistoryData = new SplitbarcodehistoryDataService();
            Splitbarcodehistory result = await Task<Splitbarcodehistory>.Run(() => SplitbarcodehistoryData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IHttpActionResult> Delete(ActionRequest<Splitbarcodehistory> request)
        {
            SplitbarcodehistoryDataService SplitbarcodehistoryData = new SplitbarcodehistoryDataService();
            bool result = await Task<bool>.Run(() => SplitbarcodehistoryData.Delete(request.Item));
            return this.Ok(result);
        }
    }
}