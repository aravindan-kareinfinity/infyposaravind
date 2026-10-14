
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
    [RoutePrefix("VoucheritembillwiseService")]
    public class VoucheritembillwiseServiceController : ApiController
    {

        [HttpGet]
        [Route("LedgerOpening/{ledgerid}")]
        public async Task<IHttpActionResult> LedgerOpening(string ledgerid)
        {
            VoucheritembillwiseDataService VoucheritembillwiseData = new VoucheritembillwiseDataService();
            List<Voucheritembillwise> result = await Task<List<Voucheritembillwise>>.Run(() => VoucheritembillwiseData.Search(new VoucheritembillwiseCreteria()
            {
                showopening = true,
                ledgerid = long.Parse(ledgerid)
            }));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<VoucheritembillwiseCreteria> request)
        {
            VoucheritembillwiseDataService VoucheritembillwiseData = new VoucheritembillwiseDataService();
            List<Voucheritembillwise> result = await Task<List<Voucheritembillwise>>.Run(() => VoucheritembillwiseData.Search(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("SaveOpening")]
        public async Task<IHttpActionResult> SaveOpening(ActionRequest<LedgerOpening> request)
        {
            VoucheritembillwiseDataService VoucheritembillwiseData = new VoucheritembillwiseDataService();
            bool result = await Task<bool>.Run(() => VoucheritembillwiseData.SaveOpening(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Save")]
        public async Task<IHttpActionResult> Save(ActionRequest<Voucheritembillwise> request)
        {
            VoucheritembillwiseDataService VoucheritembillwiseData = new VoucheritembillwiseDataService();
            if (request.Item.id <= 0)
            {
                Voucheritembillwise result = await Task<Voucheritembillwise>.Run(() => VoucheritembillwiseData.Insert(request.Item));
                return this.Ok(result);
            }
            else
            {
                Voucheritembillwise result = await Task<Voucheritembillwise>.Run(() => VoucheritembillwiseData.Update(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("Insert")]
        public async Task<IHttpActionResult> Insert(ActionRequest<Voucheritembillwise> request)
        {
            VoucheritembillwiseDataService VoucheritembillwiseData = new VoucheritembillwiseDataService();
            Voucheritembillwise result = await Task<Voucheritembillwise>.Run(() => VoucheritembillwiseData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        public async Task<IHttpActionResult> Update(ActionRequest<Voucheritembillwise> request)
        {
            VoucheritembillwiseDataService VoucheritembillwiseData = new VoucheritembillwiseDataService();
            Voucheritembillwise result = await Task<Voucheritembillwise>.Run(() => VoucheritembillwiseData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IHttpActionResult> Delete(ActionRequest<Voucheritembillwise> request)
        {
            VoucheritembillwiseDataService VoucheritembillwiseData = new VoucheritembillwiseDataService();
            bool result = await Task<bool>.Run(() => VoucheritembillwiseData.Delete(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("DeleteAll")]
        public async Task<IHttpActionResult> DeleteAll(ActionRequest<VoucheritembillwiseCreteria> request)
        {
            VoucheritembillwiseDataService VoucheritembillwiseData = new VoucheritembillwiseDataService();
            bool result = await Task<bool>.Run(() => VoucheritembillwiseData.DeleteAll(request.Item));
            return this.Ok(result);
        }
    }
}