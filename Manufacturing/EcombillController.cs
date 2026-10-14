
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
using InfyPOS.Processors;

namespace InfyPOS.Controllers
{
    [RoutePrefix("EcombillService")]
    public class EcombillServiceController : ApiController
    {

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<Ecombill>();
            result.Item = new Ecombill();
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<EcombillCreteria> request)
        {
            EcombillDataService EcombillData = new EcombillDataService();
            List<Ecombill> result = await Task<List<Ecombill>>.Run(() => EcombillData.Search(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("SearchStock")]
        public async Task<IHttpActionResult> SearchStock(ActionRequest<EcombillCreteria> request)
        {
            EcombillDataService EcombillData = new EcombillDataService();
            List<Unicomerce.QuantoSKUStcok> result = await Task<Unicomerce.QuantoSKUStcok>.Run(() => EcombillData.SearchStock(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("SearchOnline")]
        public async Task<IHttpActionResult> SearchOnline(ActionRequest<EcombillCreteria> request)
        {
            EcombillDataService EcombillData = new EcombillDataService();
            List<Ecombill> result = await Task<List<Ecombill>>.Run(() => EcombillData.SearchOnline(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("SearchBundle")]
        public async Task<IHttpActionResult> SearchBundle(ActionRequest<EcombillCreteria> request)
        {
            EcombillDataService EcombillData = new EcombillDataService();
            List<Unicomerce.QuantoBundleUAN> result = await Task<List<Unicomerce.QuantoBundleUAN>>.Run(() => EcombillData.SearchBundle(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Stock2Unicommerce")]
        public async Task<IHttpActionResult> Stock2Unicommerce(ActionMulitpleRequest<Unicomerce.QuantoSKUAdjust> request)
        {
            EcombillDataService EcombillData = new EcombillDataService();
            List<Unicomerce.QuantoSKUAdjust> result = await Task<Unicomerce.QuantoSKUAdjust>.Run(() => EcombillData.Stock2Unicommerce(request.Items));
            return this.Ok(result);
        }


        [HttpPost]
        [Route("ProcessOnline")]
        public async Task<IHttpActionResult> ProcessOnline(ActionMulitpleRequest<Ecombill> request)
        {
            EcombillDataService EcombillData = new EcombillDataService();
            InfyPOS.Processors.ExcelImportManager.DocumentStatus imageidentifier = await Task<InfyPOS.Processors.ExcelImportManager.DocumentStatus>.Run(() => EcombillData.ProcessOnline(request.Items));
            return this.Ok(imageidentifier);
        }

        [HttpPost]
        [Route("Save")]
        public async Task<IHttpActionResult> Save(ActionRequest<Ecombill> request)
        {
            EcombillDataService EcombillData = new EcombillDataService();
            if (request.Item.id <= 0)
            {
                Ecombill result = await Task<Ecombill>.Run(() => EcombillData.Insert(request.Item));
                return this.Ok(result);
            }
            else
            {
                Ecombill result = await Task<Ecombill>.Run(() => EcombillData.Update(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("Insert")]
        public async Task<IHttpActionResult> Insert(ActionRequest<Ecombill> request)
        {
            EcombillDataService EcombillData = new EcombillDataService();
            Ecombill result = await Task<Ecombill>.Run(() => EcombillData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        public async Task<IHttpActionResult> Update(ActionRequest<Ecombill> request)
        {
            EcombillDataService EcombillData = new EcombillDataService();
            Ecombill result = await Task<Ecombill>.Run(() => EcombillData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IHttpActionResult> Delete(ActionRequest<Ecombill> request)
        {
            EcombillDataService EcombillData = new EcombillDataService();
            bool result = await Task<bool>.Run(() => EcombillData.Delete(request.Item));
            return this.Ok(result);
        }
    }
}