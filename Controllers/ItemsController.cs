
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
using InfyPOS.Processors.Migration;
using System.IO;
using OfficeOpenXml;

namespace InfyPOS.Controllers
{
    [RoutePrefix("ItemsService")]
    public class ItemsServiceController : ApiController
    {

        [AllowAnonymous]
        [HttpGet]
        [Route("DuplicateAttributes/{organizationid}")]
        public async Task<IHttpActionResult> DuplicateAttributes(string organizationid)
        {
            ItemsDataService ItemsData = new ItemsDataService() { Context = new UserClaim() { OrganizationID = long.Parse(organizationid) } };
            string result = await Task<string>.Run(() => ItemsData.RemoveDuplicate());
            return new FileActionResult(true, "itemupdatescript.txt", "text/plain", new ByteArrayContent(System.Text.ASCIIEncoding.ASCII.GetBytes(result)));
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("DuplicateItems/{organizationid}")]
        public async Task<IHttpActionResult> DuplicateItems(string organizationid)
        {
            ItemsDataService ItemsData = new ItemsDataService() { Context = new UserClaim() { OrganizationID = long.Parse(organizationid) } };
            string result = await Task<string>.Run(() => ItemsData.RemoveDuplicateRecords());
            return new FileActionResult(true, "itemupdatescript.txt", "text/plain", new ByteArrayContent(System.Text.ASCIIEncoding.ASCII.GetBytes(result)));
        }

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<Items>();
            result.Item = new Items();
            return this.Ok(result);
        }


        [HttpPost]
        [Route("SearchByDesignName")]
        public async Task<IHttpActionResult> Search(ActionRequest<SearchByDesignName> request)
        {
            ItemsDataService ItemsData = new ItemsDataService();
            LrinvoiceitemsDataService designservice = new LrinvoiceitemsDataService();

            List<LrinvoiceItemDetail> result = await Task<List<LrinvoiceItemDetail>>.Run(() => designservice.Search(new LrinvoiceitemsCreteria()
            {
                designid = request.Item.designid,
                supplierid = request.Item.supplierid,
                productid = request.Item.productid,
                fetchdesign = true
            }));

            if (result.Count == 1)
            {
                LrinvoiceItemDetail invoiceresult = result[0];
                if (invoiceresult.itemid > 0)
                {
                    var itemresult = await Task<List<Items>>.Run(() => ItemsData.Search(new ItemsCreteria() { id = invoiceresult.itemid }));
                    if (itemresult.Count == 1)
                        invoiceresult.Details = itemresult[0];
                }
                return this.Ok(invoiceresult);
            }
            return this.NotFound();
        }

        [HttpPost]
        [Route("SearchByName")]
        public async Task<IHttpActionResult> Search(ActionRequest<SearchByName> request)
        {
            ItemsDataService ItemsData = new ItemsDataService();
            List<Items> result = await Task<List<Items>>.Run(() => ItemsData.Search(new ItemsCreteria() { code = request.Item.name, productid = request.Item.productid }));
            if (result.Count == 0)
                result = await Task<List<Items>>.Run(() => ItemsData.Search(new ItemsCreteria() { SearchKeywords = request.Item.name, productid = request.Item.productid, intelligent = true }));
            if (result.Count > 0)
                return this.Ok(result);
            return this.NotFound();
        }

        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<ItemsCreteria> request)
        {
            ItemsDataService ItemsData = new ItemsDataService();
            if (request.Item.skudesign)
            {
                MfgdesignDataService mfgdesignDataService = new MfgdesignDataService();
                var uanitem = mfgdesignDataService.SearchUANItem(request.Item.gln, null);
                if (uanitem != null)
                {
                    List<Items> results = await Task<List<Items>>.Run(() => ItemsData.Search(new ItemsCreteria()
                    {
                        id = uanitem.itemid
                    }));
                    if(results.Count == 1)
                    {
                        results[0].gln = request.Item.gln;
                        results[0].mrp = uanitem.mrp;
                        results[0].salerate = uanitem.wsrate;
                        return this.Ok(results);
                    }
                }
            }

            
            List<Items> result = await Task<List<Items>>.Run(() => ItemsData.Search(request.Item));
            if (result.Count == 0 && request.Item.autocreate)
            {
                Items resultitem = await Task<Items>.Run(() => ItemsData.Insert(request.Item));
                result.Add(resultitem);
            }
            return this.Ok(result);

        }

        [HttpPost]
        [Route("Save")]
        public async Task<IHttpActionResult> Save(ActionRequest<Items> request)
        {
            ItemsDataService ItemsData = new ItemsDataService();
            if (request.Item.id <= 0)
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Create);
                Items result = await Task<Items>.Run(() => ItemsData.Insert(request.Item));
                return this.Ok(result);
            }
            else
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Update);
                Items result = await Task<Items>.Run(() => ItemsData.Update(request.Item, true));
                return this.Ok(result);
            }
        }


        [HttpGet]
        [AllowAnonymous]
        [Route("SyncDenormalizedNames/{organizationid}")]
        public async Task<IHttpActionResult> SyncDenormalizedNames(string organizationid)
        {
            long orgid = 0;
            if (!long.TryParse(organizationid, out orgid))
                return this.NotFound();

            ItemsDataService ItemsData = new ItemsDataService()
            {
                Context = new UserClaim()
                {
                    OrganizationID = orgid
                }
            };
            bool result = await Task<bool>.Run(() => ItemsData.SyncDenormalizedNames());
            return this.Ok(result);
        }


        [HttpPost]
        [Route("Insert")]
        public async Task<IHttpActionResult> Insert(ActionRequest<Items> request)
        {
            ItemsDataService ItemsData = new ItemsDataService();
            Items result = await Task<Items>.Run(() => ItemsData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        public async Task<IHttpActionResult> Update(ActionRequest<Items> request)
        {
            ItemsDataService ItemsData = new ItemsDataService();
            Items result = await Task<Items>.Run(() => ItemsData.Update(request.Item, true));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IHttpActionResult> Delete(ActionRequest<Items> request)
        {
            ItemsDataService ItemsData = new ItemsDataService();
            bool result = await Task<bool>.Run(() => ItemsData.Delete(request.Item));
            return this.Ok(result);
        }


        

        [Route("MigrateItemImport")]
        [HttpPost]
        public async Task<IHttpActionResult> MigrateItemImport(InfyPOS.Processors.Migration.MigrationData idata)
        {
            BaseData baseData = new BaseData();
            var data = new InfyPOS.Processors.ProcessManager.ProcessData();
            data.action = "ITEMMIGRATION";
            data.Context = baseData.Context;
            data.request = this.Request;
            data.migrationdata = idata;
            InfyPOS.Processors.Migration.ProgressStatus status = await Task<InfyPOS.Processors.Migration.ProgressStatus>.Run(() =>
                StartProcess(baseData.Context.OrganizationID, data));
            return this.Ok(status);

        }

        private InfyPOS.Processors.Migration.ProgressStatus StartProcess(long organizationid,
            InfyPOS.Processors.ProcessManager.ProcessData data)
        {
            return InfyPOS.Processors.ProcessManager.Instance(organizationid).Start(data);
        }

        //[Route("MigrateItemData")]
        //[HttpPost]
        //public async Task<IHttpActionResult> MigrateItemData(InfyPOS.Processors.Migration.MigrationData data)
        //{
        //    ItemsDataService baseData = new ItemsDataService();
        //    InfyPOS.Processors.Migration.ProgressStatus status = await Task<InfyPOS.Processors.Migration.ProgressStatus>.Run(() =>
        //        baseData.MigrateItemData(data));
        //    return this.Ok(status);

        //}

        [Route("MigrateItemProcess")]
        [HttpPost]
        public async Task<IHttpActionResult> MigrateItemProcess(InfyPOS.Processors.Migration.MigrationData data)
        {
            ItemsDataService baseData = new ItemsDataService();
            List<MigrationManager.MigratingItem> rows = await Task<List<MigrationManager.MigratingItem>>.Run(() =>
                baseData.MigrateItemProcess(data));
            data.rows = rows;
            return this.Ok(data);
        }

    }
}