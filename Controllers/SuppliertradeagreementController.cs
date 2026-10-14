
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
    [RoutePrefix("SuppliertradeagreementService")]
    public class SuppliertradeagreementServiceController : ApiController
    {

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<Suppliertradeagreement>();
            result.Item = new Suppliertradeagreement();
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<SuppliertradeagreementCreteria> request)
        {
            SuppliertradeagreementDataService SuppliertradeagreementData = new SuppliertradeagreementDataService();

            if (request.Item.showdetail)
            {
                List<Suppliertradeagreementdetail> result = await Task<List<Suppliertradeagreementdetail>>.Run(() => SuppliertradeagreementData.SearchDetail(request.Item));
                return this.Ok(result);
            }
            else
            {
                List<Suppliertradeagreement> result = await Task<List<Suppliertradeagreement>>.Run(() => SuppliertradeagreementData.Search(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("Save")]
        public async Task<IHttpActionResult> Save(ActionRequest<Suppliertradeagreement> request)
        {
            SuppliertradeagreementDataService SuppliertradeagreementData = new SuppliertradeagreementDataService();
            if (request.Item.id <= 0)
            {
                Suppliertradeagreement result = await Task<Suppliertradeagreement>.Run(() => SuppliertradeagreementData.Insert(request.Item));
                return this.Ok(result);
            }
            else
            {
                Suppliertradeagreement result = await Task<Suppliertradeagreement>.Run(() => SuppliertradeagreementData.Update(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("UpdateMatrixItems")]
        public async Task<IHttpActionResult> UpdateMatrixItems(ActionRequest<SupplierTransactionWithMatrix> request)
        {
            SuppliertradeagreementDataService SuppliertradeagreementData = new SuppliertradeagreementDataService();
            Suppliertradeagreement result = await Task<Suppliertradeagreement>.Run(() => SuppliertradeagreementData.UpdateMatrixItems(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("CloneMatrixItems")]
        public async Task<IHttpActionResult> CloneMatrixItems(ActionRequest<Suppliertradeagreement.MatrixItem> request)
        {
            SuppliertradeagreementDataService SuppliertradeagreementData = new SuppliertradeagreementDataService();
            Suppliertradeagreement.MatrixItem result = await Task<Suppliertradeagreement.MatrixItem>.Run(() => SuppliertradeagreementData.CloneMatrixItems(request.Item));
            return this.Ok(result);
        }


        [HttpPost]
        [Route("UpdateDisable")]
        public async Task<IHttpActionResult> UpdateDisable(ActionRequest<Suppliertradeagreement> request)
        {
            SuppliertradeagreementDataService SuppliertradeagreementData = new SuppliertradeagreementDataService();
            bool result = await Task<bool>.Run(() => SuppliertradeagreementData.UpdateDisable(request.Item));
            return this.Ok(result);
        }

        [HttpGet]
        [Route("ActiveAgreement/{agreementid}")]
        public async Task<IHttpActionResult> ActiveProducts(string agreementid)
        {
            SuppliertradeagreementDataService SupplierproductgroupsData = new SuppliertradeagreementDataService();
            List<Suppliertradeagreement> result = await Task<List<Suppliertradeagreement>>.Run(() => SupplierproductgroupsData.Search(new SuppliertradeagreementCreteria() { Supplierproductgroupsid = long.Parse(agreementid)  }));
            result.RemoveAll(y => y.disable || y.disabledon >= DateTime.Now.Date);
            return this.Ok(result);
        }

        [HttpGet]
        [Route("AllAgreement/{Supplierproductgroupsid}")]
        public async Task<IHttpActionResult> AllAgreement(string Supplierproductgroupsid)
        {
            SuppliertradeagreementDataService SupplierproductgroupsData = new SuppliertradeagreementDataService();
            List<Suppliertradeagreement> result = await Task<List<Suppliertradeagreement>>.Run(() => SupplierproductgroupsData.Search(new SuppliertradeagreementCreteria() { Supplierproductgroupsid = long.Parse(Supplierproductgroupsid) }));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Insert")]
        public async Task<IHttpActionResult> Insert(ActionRequest<Suppliertradeagreement> request)
        {
            SuppliertradeagreementDataService SuppliertradeagreementData = new SuppliertradeagreementDataService();
            Suppliertradeagreement result = await Task<Suppliertradeagreement>.Run(() => SuppliertradeagreementData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        public async Task<IHttpActionResult> Update(ActionRequest<Suppliertradeagreement> request)
        {
            SuppliertradeagreementDataService SuppliertradeagreementData = new SuppliertradeagreementDataService();
            Suppliertradeagreement result = await Task<Suppliertradeagreement>.Run(() => SuppliertradeagreementData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IHttpActionResult> Delete(ActionRequest<Suppliertradeagreement> request)
        {
            SuppliertradeagreementDataService SuppliertradeagreementData = new SuppliertradeagreementDataService();
            bool result = await Task<bool>.Run(() => SuppliertradeagreementData.Delete(request.Item));
            return this.Ok(result);
        }
    }
}