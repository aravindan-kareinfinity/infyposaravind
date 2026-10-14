
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
    [RoutePrefix("LrentryService")]
    public class LrentryServiceController : ApiController
    {

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<Lrentry>();
            result.Item = new Lrentry();
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Search")]
        [WebAPI.InfyAuthorize(PermissionSet.lrentry, InfyPOS.Common.Action.View)]
        public async Task<IHttpActionResult> Search(ActionRequest<LrentryCreteria> request)
        {
            LrentryDataService LrentryData = new LrentryDataService();
            if (request.Item.modeofdelivery == 10)
            {
                List<LrRCMEntry> result = await Task<List<LrRCMEntry>>.Run(() => LrentryData.SearchRCMTransport(request.Item));
                return this.Ok(result);
            }
            else
            {
                List<Lrentry> result = await Task<List<Lrentry>>.Run(() => LrentryData.Search(request.Item));
                return this.Ok(result);
            }
        }


        [HttpPost]
        [Route("SearchLast")]
        [WebAPI.InfyAuthorize(PermissionSet.lrentry, InfyPOS.Common.Action.View)]
        public async Task<IHttpActionResult> SearchLast(ActionRequest<LrentryCreteria> request)
        {
            LrentryDataService LrentryData = new LrentryDataService();
            LrInformation result = await Task<LrInformation>.Run(() => LrentryData.SearchLast(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Save")]
        [WebAPI.InfyAuthorize(PermissionSet.lrentry, InfyPOS.Common.Action.Create)]
        public async Task<IHttpActionResult> Save(ActionRequest<Lrentry> request)
        {

            LrentryDataService LrentryData = new LrentryDataService();
            if (request.Item.id <= 0)
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Create);
                if (request.Item.Lrentryfreightcharges != null)
                {
                    foreach (var item in request.Item.Lrentryfreightcharges)
                    {
                        item.ChangeStatus(BaseEntity.DataStatus.Create);

                    }
                }
                Lrentry result = await Task<Lrentry>.Run(() => LrentryData.Insert(request.Item));
                return this.Ok(result);
            }
            else
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Update);
                if (request.Item.Lrentryfreightcharges != null)
                {
                    foreach (var item in request.Item.Lrentryfreightcharges)
                    {
                        item.ChangeStatus(BaseEntity.DataStatus.Update);
                    }
                }
                Lrentry result = await Task<Lrentry>.Run(() => LrentryData.Update(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("ImportQuantoBill")]
        [WebAPI.InfyAuthorize(PermissionSet.lrentry, InfyPOS.Common.Action.Create)]
        public async Task<IHttpActionResult> ImportQuantoBill(ActionRequest<LRBillImport> request)
        {

            LrentryDataService LrentryData = new LrentryDataService();
            request.Item.lrentry.ChangeStatus(BaseEntity.DataStatus.Create);
            if (request.Item.lrentry.Lrentryfreightcharges != null)
            {
                foreach (var item in request.Item.lrentry.Lrentryfreightcharges)
                {
                    item.ChangeStatus(BaseEntity.DataStatus.Create);

                }
            }
            Lrentry result = await Task<Lrentry>.Run(() => LrentryData.ImportQuantoBill(request.Item));
            return this.Ok(result);
        }


        [HttpGet]
        [Route("LastEntry/{companyid}/{locationid}")]
        public async Task<IHttpActionResult> LastEntry(string companyid,string locationid)
        {
            LrentryDataService LrentryData = new LrentryDataService();
            string result = await Task<string>.Run(() => LrentryData.LastEntry(companyid,locationid));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Insert")]
        [WebAPI.InfyAuthorize(PermissionSet.lrentry, InfyPOS.Common.Action.Create)]
        public async Task<IHttpActionResult> Insert(ActionRequest<Lrentry> request)
        {
            LrentryDataService LrentryData = new LrentryDataService();
            Lrentry result = await Task<Lrentry>.Run(() => LrentryData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        [WebAPI.InfyAuthorize(PermissionSet.lrentry, InfyPOS.Common.Action.Modify)]
        public async Task<IHttpActionResult> Update(ActionRequest<Lrentry> request)
        {
            LrentryDataService LrentryData = new LrentryDataService();
            Lrentry result = await Task<Lrentry>.Run(() => LrentryData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("UpdateLRExpense")]
        [WebAPI.InfyAuthorize(PermissionSet.lrentry, InfyPOS.Common.Action.Modify)]
        public async Task<IHttpActionResult> UpdateLRExpense(ActionRequest<Lrentry> request)
        {
            LrentryDataService LrentryData = new LrentryDataService();
            Lrentry result = await Task<Lrentry>.Run(() => LrentryData.UpdateLRExpense(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        [WebAPI.InfyAuthorize(PermissionSet.lrentry, InfyPOS.Common.Action.Delete)]
        public async Task<IHttpActionResult> Delete(ActionRequest<Lrentry> request)
        {
            LrentryDataService LrentryData = new LrentryDataService();
            bool result = await Task<bool>.Run(() => LrentryData.Delete(request.Item));
            return this.Ok(result);
        }

        [Route("MigrateData")]
        [HttpPost]
        public async Task<IHttpActionResult> MigrateData(InfyPOS.Processors.Migration.MigrationData data)
        {
            Common.ImageData imageData = new Common.ImageData();
            InfyPOS.Processors.BillImportManager billImportManager = new InfyPOS.Processors.BillImportManager(imageData.Context.OrganizationID);
            InfyPOS.Processors.BillImportManager.BillSummary imageidentifier =
                await Task<InfyPOS.Processors.BillImportManager.BillSummary>.Run(() =>
                billImportManager.PrepareLRData(data, imageData));
            return this.Ok(imageidentifier);
        }

        [HttpPost]
        [Route("PrintBarcode")]
        public async Task<IHttpActionResult> PrintBarcode(ActionRequest<LrEntryPrintCreteria> request)
        {
            if (request.Item.reportid == 0 || request.Item.id == 0)
                throw new Exception("Invalid input On Entry no or Report no");

            InfyPOS.Data.ConfigurationDataService config = new ConfigurationDataService();
            var reportformats = config.Search(new ConfigurationCreteria
            {
                id = request.Item.reportid
            });

            if (reportformats.Count > 0)
            {
                InfyPOS.Report.CitizenBarcode barcodeprinter = new Report.CitizenBarcode(request.Item.companytitle,
                             reportformats[0].reportformat.format, reportformats[0].reportformat.mode, reportformats[0].nooflines);

                List<LrEntry2Print> lrentry2print = new List<LrEntry2Print>();
                LrentryDataService LrentryData = new LrentryDataService();
                lrentry2print = LrentryData.SearchForLable(new LrentryCreteria() { id = request.Item.id });

                if (lrentry2print == null || lrentry2print.Count == 0)
                    throw new Exception("LR Entry not found. Try again");

                for (int i = 1; i < request.Item.copy; i++)
                {
                    var citem = lrentry2print[0].Clone();
                    lrentry2print.Add(citem);
                }

                var entryList = lrentry2print.OrderBy(e => e.id).ToList().ConvertAll(e => (Report.IBarcodeFormat)e);
                var response = barcodeprinter.Render(entryList);
                List<string> base64Response = new List<string>();
                foreach (var item in response)
                {
                    var bytes = System.Text.Encoding.ASCII.GetBytes(item);
                    base64Response.Add(Convert.ToBase64String(bytes));
                }
                return this.Ok(base64Response);
            }

            return this.NotFound();
        }

    }
}