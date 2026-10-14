
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
    [RoutePrefix("TallyService")]
    public class TallyServiceController : ApiController
    {


        [HttpPost]
        [Route("PurchaseReturn")]
        public async Task<IHttpActionResult> PurchaseReturn(ActionRequest<TallyConfig.PurchaseReturnCreteria> request)
        {
            Tally.TallyDataservice tallydata = new Tally.TallyDataservice();
            Tally.TallyDataservice.TallyVoucher result = await Task<Tally.TallyDataservice.TallyVoucher>.Run(() => tallydata.CreatePurchaseReturnVouchers(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Purchase")]
        public async Task<IHttpActionResult> Search(ActionRequest<TallyConfig.PurchaseCreteria> request)
        {
            Tally.TallyDataservice tallydata = new Tally.TallyDataservice();
            Tally.TallyDataservice.TallyVoucher result = await Task<Tally.TallyDataservice.TallyVoucher>.Run(() => tallydata.CreatePurchaseVouchers(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Save")]
        public async Task<IHttpActionResult> Save(ActionRequest<Autonumber> request)
        {
            AutonumberDataService AutonumberData = new AutonumberDataService();
            if (request.Item.id <= 0)
            {
                Autonumber result = await Task<Autonumber>.Run(() => AutonumberData.Insert(request.Item));
                return this.Ok(result);
            }
            else
            {
                Autonumber result = await Task<Autonumber>.Run(() => AutonumberData.Update(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("Insert")]
        public async Task<IHttpActionResult> Insert(ActionRequest<Autonumber> request)
        {
            AutonumberDataService AutonumberData = new AutonumberDataService();
            Autonumber result = await Task<Autonumber>.Run(() => AutonumberData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        public async Task<IHttpActionResult> Update(ActionRequest<Autonumber> request)
        {
            AutonumberDataService AutonumberData = new AutonumberDataService();
            Autonumber result = await Task<Autonumber>.Run(() => AutonumberData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IHttpActionResult> Delete(ActionRequest<Autonumber> request)
        {
            AutonumberDataService AutonumberData = new AutonumberDataService();
            bool result = await Task<bool>.Run(() => AutonumberData.Delete(request.Item));
            return this.Ok(result);
        }
    }
}