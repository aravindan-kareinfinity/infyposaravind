
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
    public class TallyController : ApiController
    {

        [HttpPost]
        [Route("CreatePurchaseVouchers")]
        public async Task<IHttpActionResult> CreatePurchaseVouchers(ActionRequest<InfyPOS.Models.TallyConfig.PurchaseCreteria> request)
        {
            InfyPOS.Tally.TallyDataservice dataservice = new InfyPOS.Tally.TallyDataservice();
            AnalyserDataService analyserData = new AnalyserDataService();
            InfyPOS.Tally.TallyDataservice.TallyVoucher result = await Task<InfyPOS.Tally.TallyDataservice.TallyVoucher>.Run(() => dataservice.CreatePurchaseVouchers(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("CreateBOMPurchaseVouchers")]
        public async Task<IHttpActionResult> CreateBOMPurchaseVouchers(ActionRequest<InfyPOS.Models.TallyConfig.PurchaseCreteria> request)
        {
            InfyPOS.Tally.TallyDataservice dataservice = new InfyPOS.Tally.TallyDataservice();
            AnalyserDataService analyserData = new AnalyserDataService();
            InfyPOS.Tally.TallyDataservice.TallyVoucher result = await Task<InfyPOS.Tally.TallyDataservice.TallyVoucher>.Run(() => dataservice.CreateBOMPurchaseVouchers(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("CreatePaymentVouchers")]
        public async Task<IHttpActionResult> CreatePaymentVouchers(ActionRequest<InfyPOS.Models.TallyConfig.SupplierPaymentCreteria> request)
        {
            InfyPOS.Tally.TallyDataservice dataservice = new InfyPOS.Tally.TallyDataservice();
            AnalyserDataService analyserData = new AnalyserDataService();
            InfyPOS.Tally.TallyDataservice.TallyVoucher result = await Task<InfyPOS.Tally.TallyDataservice.TallyVoucher>.Run(() => dataservice.CreateSupplierPaymentVouchers(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("CreatePurchaseReturnVouchers")]
        public async Task<IHttpActionResult> CreatePurchaseReturnVouchers(ActionRequest<InfyPOS.Models.TallyConfig.PurchaseReturnCreteria> request)
        {
            InfyPOS.Tally.TallyDataservice dataservice = new InfyPOS.Tally.TallyDataservice();
            AnalyserDataService analyserData = new AnalyserDataService();
            InfyPOS.Tally.TallyDataservice.TallyVoucher result = await Task<InfyPOS.Tally.TallyDataservice.TallyVoucher>.Run(() => dataservice.CreatePurchaseReturnVouchers(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("CreateBOMPurchaseReturnVouchers")]
        public async Task<IHttpActionResult> CreateBOMPurchaseReturnVouchers(ActionRequest<InfyPOS.Models.TallyConfig.PurchaseReturnCreteria> request)
        {
            InfyPOS.Tally.TallyDataservice dataservice = new InfyPOS.Tally.TallyDataservice();
            AnalyserDataService analyserData = new AnalyserDataService();
            InfyPOS.Tally.TallyDataservice.TallyVoucher result = await Task<InfyPOS.Tally.TallyDataservice.TallyVoucher>.Run(() => dataservice.CreateBOMPurchaseReturnVouchers(request.Item));
            return this.Ok(result);
        }


        [HttpPost]
        [Route("CreateCreditPaymentVouchers")]
        public async Task<IHttpActionResult> CreateCreditPaymentVouchers(ActionRequest<InfyPOS.Models.TallyConfig.SalesCreteria> request)
        {
            InfyPOS.Tally.TallyDataservice dataservice = new InfyPOS.Tally.TallyDataservice();
            AnalyserDataService analyserData = new AnalyserDataService();
            InfyPOS.Tally.TallyDataservice.TallyVoucher result = await Task<InfyPOS.Tally.TallyDataservice.TallyVoucher>.Run(() => dataservice.CreateCreditPaymentVouchers(request.Item));
            return this.Ok(result);
        }


        [HttpPost]
        [Route("CreatePettycashVouchers")]
        public async Task<IHttpActionResult> CreatePettycashVouchers(ActionRequest<InfyPOS.Models.TallyConfig.SalesCreteria> request)
        {
            InfyPOS.Tally.TallyDataservice dataservice = new InfyPOS.Tally.TallyDataservice();
            AnalyserDataService analyserData = new AnalyserDataService();
            InfyPOS.Tally.TallyDataservice.TallyVoucher result = await Task<InfyPOS.Tally.TallyDataservice.TallyVoucher>.Run(() => dataservice.CreatePettyCashVouchers(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("CreateVouchers")]
        public async Task<IHttpActionResult> CreateVouchers(ActionRequest<InfyPOS.Models.TallyConfig.SalesCreteria> request)
        {
            InfyPOS.Tally.TallyDataservice dataservice = new InfyPOS.Tally.TallyDataservice();
            AnalyserDataService analyserData = new AnalyserDataService();
            InfyPOS.Tally.TallyDataservice.TallyVoucher result = await Task<InfyPOS.Tally.TallyDataservice.TallyVoucher>.Run(() => dataservice.CreateVouchers(request.Item));
            return this.Ok(result);
        }


        [HttpPost]
        [Route("CreateSalesVouchers")]
        public async Task<IHttpActionResult> CreateSalesVouchers(ActionRequest<InfyPOS.Models.TallyConfig.SalesCreteria> request)
        {
            InfyPOS.Tally.TallyDataservice dataservice = new InfyPOS.Tally.TallyDataservice();
            AnalyserDataService analyserData = new AnalyserDataService();
            InfyPOS.Tally.TallyDataservice.TallyVoucher result = await Task<InfyPOS.Tally.TallyDataservice.TallyVoucher>.Run(() => dataservice.CreateSalesVouchers(request.Item));
            return this.Ok(result);
        }

    }
}