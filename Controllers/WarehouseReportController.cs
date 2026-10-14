
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
    [RoutePrefix("WarehouseReportService")]
    public class WarehouseReportController : ApiController
    {

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<Refillbox>();
            result.Item = new Refillbox();
            return this.Ok(result);
        }

        [HttpPost]
        [Route("SearchLR")]
        [WebAPI.InfyAuthorize(PermissionSet.lrreport, InfyPOS.Common.Action.View)]
        public async Task<IHttpActionResult> SearchLR(ActionRequest<WarehouseReportCreteria> request)
        {
            WarehouseReportDataService WarehouseReportData = new WarehouseReportDataService();
            if (InfyPOS.Services.ConfigManager.Instance(WarehouseReportData.Context.OrganizationID).Config != null)
                request.Item.multicompany = InfyPOS.Services.ConfigManager.Instance(WarehouseReportData.Context.OrganizationID).Config.system_multicompany;

            List<LRReport> result = await Task<List<LRReport>>.Run(() => WarehouseReportData.SearchLR(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("SearchLRInvoice")]
        [WebAPI.InfyAuthorize(PermissionSet.lrinvoicereport, InfyPOS.Common.Action.View)]
        public async Task<IHttpActionResult> SearchLRInvoice(ActionRequest<WarehouseReportCreteria> request)
        {
            WarehouseReportDataService WarehouseReportData = new WarehouseReportDataService();
            if (InfyPOS.Services.ConfigManager.Instance(WarehouseReportData.Context.OrganizationID).Config != null)
                request.Item.multicompany = InfyPOS.Services.ConfigManager.Instance(WarehouseReportData.Context.OrganizationID).Config.system_multicompany;

            List<POEntry> result = await Task<List<POEntry>>.Run(() => WarehouseReportData.SearchInvoice(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("SearchPurchaseReturn")]
        [WebAPI.InfyAuthorize(PermissionSet.purchasereturnreport, InfyPOS.Common.Action.View)]
        public async Task<IHttpActionResult> SearchPurchaseReturn(ActionRequest<WarehouseReportCreteria> request)
        {
            WarehouseReportDataService WarehouseReportData = new WarehouseReportDataService();
            if (InfyPOS.Services.ConfigManager.Instance(WarehouseReportData.Context.OrganizationID).Config != null)
                request.Item.multicompany = InfyPOS.Services.ConfigManager.Instance(WarehouseReportData.Context.OrganizationID).Config.system_multicompany;

            PurchaseReturnReport result = await Task<PurchaseReturnReport>.Run(() => WarehouseReportData.SearchPurchaseReturn(request.Item));
            return this.Ok(result);
        }
        [HttpPost]
        [Route("SearchStock")]
        //[WebAPI.InfyAuthorize(PermissionSet.whstockreport, InfyPOS.Common.Action.View)]
        public async Task<IHttpActionResult> SearchStock(ActionRequest<StockReportCreteria> request)
        {
            WarehouseReportDataService WarehouseReportData = new WarehouseReportDataService();
            if (InfyPOS.Services.ConfigManager.Instance(WarehouseReportData.Context.OrganizationID).Config != null)
                request.Item.multicompany = InfyPOS.Services.ConfigManager.Instance(WarehouseReportData.Context.OrganizationID).Config.system_multicompany;

            List<StockReport> result = await Task<List<StockReport>>.Run(() => WarehouseReportData.SearchStock(request.Item));
            return this.Ok(result);
        }
        [HttpPost]
        [Route("SearchPurchaseItems")]
        //[WebAPI.InfyAuthorize(PermissionSet.whstockreport, InfyPOS.Common.Action.View)]
        public async Task<IHttpActionResult> SearchPurchaseItems(ActionRequest<StockReportCreteria> request)
        {
            WarehouseReportDataService WarehouseReportData = new WarehouseReportDataService();
            if (InfyPOS.Services.ConfigManager.Instance(WarehouseReportData.Context.OrganizationID).Config != null)
                request.Item.multicompany = InfyPOS.Services.ConfigManager.Instance(WarehouseReportData.Context.OrganizationID).Config.system_multicompany;

            List<StockReport> result = await Task<List<StockReport>>.Run(() => WarehouseReportData.SearchPurchaseItems(request.Item));
            return this.Ok(result);
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("PurchaseHistory/{invoiceid}")]
        public async Task<IHttpActionResult> PurchaseHistory(string invoiceid)
        {
            WarehouseReportDataService WarehouseReportData = new WarehouseReportDataService();
            List<PurchaseHistoryItem> result = await Task<List<PurchaseHistoryItem>>.Run(() => WarehouseReportData.PurchaseHistory(new StockReportCreteria()
            {
                invoiceid = long.Parse(invoiceid)
            }));
            return this.Ok(result);
        }

        
        [HttpPost]
        [Route("SearchInoicePOSummary")]
        //[WebAPI.InfyAuthorize(PermissionSet.whstockreport, InfyPOS.Common.Action.View)]
        public async Task<IHttpActionResult> SearchInoicePOSummary(ActionRequest<WarehouseReportCreteria> request)
        {
            WarehouseReportDataService WarehouseReportData = new WarehouseReportDataService();
            if (InfyPOS.Services.ConfigManager.Instance(WarehouseReportData.Context.OrganizationID).Config != null)
                request.Item.multicompany = InfyPOS.Services.ConfigManager.Instance(WarehouseReportData.Context.OrganizationID).Config.system_multicompany;

            List<POInvoiceSummaryItem> result = await Task<List<POInvoiceSummaryItem>>.Run(() => WarehouseReportData.SearchInoicePOSummary(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("PurchaseHistory")]
        //[WebAPI.InfyAuthorize(PermissionSet.whstockreport, InfyPOS.Common.Action.View)]
        public async Task<IHttpActionResult> PurchaseHistory(ActionRequest<StockReportCreteria> request)
        {
            WarehouseReportDataService WarehouseReportData = new WarehouseReportDataService();
            if (InfyPOS.Services.ConfigManager.Instance(WarehouseReportData.Context.OrganizationID).Config != null)
                request.Item.multicompany = InfyPOS.Services.ConfigManager.Instance(WarehouseReportData.Context.OrganizationID).Config.system_multicompany;

            List<PurchaseHistoryItem> result = await Task<List<PurchaseHistoryItem>>.Run(() => WarehouseReportData.PurchaseHistory(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("SearchBarcodeDetails")]
        //[WebAPI.InfyAuthorize(PermissionSet.whstockreport, InfyPOS.Common.Action.View)]
        public async Task<IHttpActionResult> SearchBarcodeDetails(ActionRequest<StockReportCreteria> request)
        {
            WarehouseReportDataService WarehouseReportData = new WarehouseReportDataService();
            if (InfyPOS.Services.ConfigManager.Instance(WarehouseReportData.Context.OrganizationID).Config != null)
                request.Item.multicompany = InfyPOS.Services.ConfigManager.Instance(WarehouseReportData.Context.OrganizationID).Config.system_multicompany;

            List<StockReport> result = await Task<List<StockReport>>.Run(() => WarehouseReportData.SearchBarcodeDetails(request.Item));
            return this.Ok(result);
        }
        [HttpPost]
        [Route("ConvertBarcode2PRF")]
        //[WebAPI.InfyAuthorize(PermissionSet.whstockreport, InfyPOS.Common.Action.View)]
        public async Task<IHttpActionResult> ConvertBarcode2PRF(ActionRequest<PRFFromPurchaseBarcode> request)
        {
            WarehouseReportDataService WarehouseReportData = new WarehouseReportDataService();
            bool result = await Task<bool>.Run(() => WarehouseReportData.ConvertBarcode2PRF(request.Item));
            return this.Ok(result);
        }
        
        [HttpPost]
        [Route("SearchStockMovement")]
        [WebAPI.InfyAuthorize(PermissionSet.whstockreport, InfyPOS.Common.Action.View)]
        public async Task<IHttpActionResult> SearchStockMovement(ActionRequest<StockMovementCreteria> request)
        {
            WarehouseReportDataService WarehouseReportData = new WarehouseReportDataService();
            if (InfyPOS.Services.ConfigManager.Instance(WarehouseReportData.Context.OrganizationID).Config != null)
                request.Item.multicompany = InfyPOS.Services.ConfigManager.Instance(WarehouseReportData.Context.OrganizationID).Config.system_multicompany;

            List<StockMovement> result = await Task<List<StockMovement>>.Run(() => WarehouseReportData.SearchStockMovement(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("StockDepreciation")]
        [WebAPI.InfyAuthorize(PermissionSet.whstockreport, InfyPOS.Common.Action.View)]
        public async Task<IHttpActionResult> StockDepreciation(ActionRequest<StockDepreciationCreteria> request)
        {
            WarehouseReportDataService WarehouseReportData = new WarehouseReportDataService();
            List<StockDepreciation> result = await Task<List<StockDepreciation>>.Run(() => WarehouseReportData.StockDepreciation(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("SearchNewProducts")]
        [WebAPI.InfyAuthorize(PermissionSet.whstockreport, InfyPOS.Common.Action.View)]
        public async Task<IHttpActionResult> SearchNewProducts(ActionRequest<ProductsCreteria> request)
        {
            WarehouseReportDataService WarehouseReportData = new WarehouseReportDataService();
            List<Products> result = await Task<List<Products>>.Run(() => WarehouseReportData.SearchNewProducts(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("SearchNewSuppliers")]
        [WebAPI.InfyAuthorize(PermissionSet.whstockreport, InfyPOS.Common.Action.View)]
        public async Task<IHttpActionResult> SearchNewSuppliers(ActionRequest<SupplierCreteria> request)
        {
            WarehouseReportDataService WarehouseReportData = new WarehouseReportDataService();
            List<Supplier> result = await Task<List<Supplier>>.Run(() => WarehouseReportData.SearchNewSuppliers(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("SearchOutwardCost")]
        [WebAPI.InfyAuthorize(PermissionSet.outwardreport, InfyPOS.Common.Action.View)]
        public async Task<IHttpActionResult> SearchOutwardCost(ActionRequest<WarehouseReportCreteria> request)
        {
            WarehouseReportDataService WarehouseReportData = new WarehouseReportDataService();
            if (InfyPOS.Services.ConfigManager.Instance(WarehouseReportData.Context.OrganizationID).Config != null)
                request.Item.multicompany = InfyPOS.Services.ConfigManager.Instance(WarehouseReportData.Context.OrganizationID).Config.system_multicompany;

            List<OutwardReport> result = await Task<List<OutwardReport>>.Run(() => WarehouseReportData.SearchOutwardCost(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("SearchOutward")]
        [WebAPI.InfyAuthorize(PermissionSet.outwardreport, InfyPOS.Common.Action.View)]
        public async Task<IHttpActionResult> SearchOutward(ActionRequest<WarehouseReportCreteria> request)
        {
            WarehouseReportDataService WarehouseReportData = new WarehouseReportDataService();
            if (InfyPOS.Services.ConfigManager.Instance(WarehouseReportData.Context.OrganizationID).Config != null)
                request.Item.multicompany = InfyPOS.Services.ConfigManager.Instance(WarehouseReportData.Context.OrganizationID).Config.system_multicompany;

            List<OutwardReport> result = await Task<List<OutwardReport>>.Run(() => WarehouseReportData.SearchOutward(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("SearchPendingTransfer")]
        [WebAPI.InfyAuthorize(PermissionSet.outwardreport, InfyPOS.Common.Action.View)]
        public async Task<IHttpActionResult> SearchPendingTransfer(ActionRequest<StockReportCreteria> request)
        {
            WarehouseReportDataService WarehouseReportData = new WarehouseReportDataService();
            if (InfyPOS.Services.ConfigManager.Instance(WarehouseReportData.Context.OrganizationID).Config != null)
                request.Item.multicompany = InfyPOS.Services.ConfigManager.Instance(WarehouseReportData.Context.OrganizationID).Config.system_multicompany;

            List<StockReport> result = await Task<List<StockReport>>.Run(() => WarehouseReportData.SearchPendingTransfer(request.Item));
            return this.Ok(result);
        }
        [HttpPost]
        [Route("SearchBarcode")]

        public async Task<IHttpActionResult> SearchBarcode(ActionRequest<StockReportCreteria> request)
        {
            WarehouseReportDataService WarehouseReportData = new WarehouseReportDataService();
            if (InfyPOS.Services.ConfigManager.Instance(WarehouseReportData.Context.OrganizationID).Config != null)
                request.Item.multicompany = InfyPOS.Services.ConfigManager.Instance(WarehouseReportData.Context.OrganizationID).Config.system_multicompany;

            BarcodeReport result = await Task<BarcodeReport>.Run(() => WarehouseReportData.SearchBarcode(request.Item));
            return this.Ok(result);
        }
        [HttpPost]
        [Route("SearchBarcodes")]

        public async Task<IHttpActionResult> SearchBarcodes(ActionRequest<StockReportCreteria> request)
        {
            WarehouseReportDataService WarehouseReportData = new WarehouseReportDataService();
            if (InfyPOS.Services.ConfigManager.Instance(WarehouseReportData.Context.OrganizationID).Config != null)
                request.Item.multicompany = InfyPOS.Services.ConfigManager.Instance(WarehouseReportData.Context.OrganizationID).Config.system_multicompany;

            List<BarcodeReport> result = await Task<List<BarcodeReport>>.Run(() => WarehouseReportData.SearchBarcodes(request.Item));
            return this.Ok(result);
        }
        [HttpPost]
        [Route("SearchStockAnalysis")]
        [WebAPI.InfyAuthorize(PermissionSet.whstockanalysisreport, InfyPOS.Common.Action.View)]
        public async Task<IHttpActionResult> SearchStockAnalysis(ActionRequest<StockReportCreteria> request)
        {
            WarehouseReportDataService WarehouseReportData = new WarehouseReportDataService();
            if (InfyPOS.Services.ConfigManager.Instance(WarehouseReportData.Context.OrganizationID).Config != null)
                request.Item.multicompany = InfyPOS.Services.ConfigManager.Instance(WarehouseReportData.Context.OrganizationID).Config.system_multicompany;

            List<StockReport> result = await Task<List<StockReport>>.Run(() => WarehouseReportData.SearchStockAnalysis(request.Item));
            return this.Ok(result);
        }
        //[HttpPost]
        //[Route("SearchAsOnStock")]
        //[WebAPI.InfyAuthorize(PermissionSet.whstockanalysisreport, InfyPOS.Common.Action.View)]
        //public async Task<IHttpActionResult> SearchAsOnStock(ActionRequest<StockReportCreteria> request)
        //{
        //    WarehouseReportDataService WarehouseReportData = new WarehouseReportDataService();
        //    if (InfyPOS.Services.ConfigManager.Instance(WarehouseReportData.Context.OrganizationID).Config != null)
        //        request.Item.multicompany = InfyPOS.Services.ConfigManager.Instance(WarehouseReportData.Context.OrganizationID).Config.system_multicompany;

        //    List<AsOnStock> result = await Task<List<AsOnStock>>.Run(() => WarehouseReportData.SearchAsOnStockV1(request.Item));
        //    return this.Ok(result);
        //}
        [HttpPost]
        [Route("SearchAsOnStock")]
        [WebAPI.InfyAuthorize(PermissionSet.whstockanalysisreport, InfyPOS.Common.Action.View)]
        public async Task<IHttpActionResult> SearchAsOnStock(ActionRequest<StockReportCreteria> request)
        {
            WarehouseReportDataService WarehouseReportData = new WarehouseReportDataService();
            if (InfyPOS.Services.ConfigManager.Instance(WarehouseReportData.Context.OrganizationID).Config != null)
                request.Item.multicompany = InfyPOS.Services.ConfigManager.Instance(WarehouseReportData.Context.OrganizationID).Config.system_multicompany;

            List<StockTransaction> result = await Task<List<StockTransaction>>.Run(() => WarehouseReportData.SearchAsOnStock(request.Item));
            return this.Ok(result);
        }
        [HttpPost]
        [Route("SearchPOEntry")]
        [WebAPI.InfyAuthorize(PermissionSet.purchasereport, InfyPOS.Common.Action.View)]
        public async Task<IHttpActionResult> SearchPOEntry(ActionRequest<WarehouseReportCreteria> request)
        {
            WarehouseReportDataService WarehouseReportData = new WarehouseReportDataService();
            if (InfyPOS.Services.ConfigManager.Instance(WarehouseReportData.Context.OrganizationID).Config != null)
                request.Item.multicompany = InfyPOS.Services.ConfigManager.Instance(WarehouseReportData.Context.OrganizationID).Config.system_multicompany;

            POReport result = await Task<POReport>.Run(() => WarehouseReportData.SearchPO(request.Item));
            return this.Ok(result);
        }
        [HttpPost]
        [Route("SearchPOEntryList")]
        [WebAPI.InfyAuthorize(PermissionSet.purchasereport, InfyPOS.Common.Action.View)]
        public async Task<IHttpActionResult> SearchPOEntryList(ActionRequest<WarehouseReportCreteria> request)
        {
            WarehouseReportDataService WarehouseReportData = new WarehouseReportDataService();
            if (InfyPOS.Services.ConfigManager.Instance(WarehouseReportData.Context.OrganizationID).Config != null)
                request.Item.multicompany = InfyPOS.Services.ConfigManager.Instance(WarehouseReportData.Context.OrganizationID).Config.system_multicompany;

            List<POEntry> result = await Task<List<POEntry>>.Run(() => WarehouseReportData.SearchPOList(request.Item));
            return this.Ok(result);
        }
        [HttpPost]
        [Route("SearchWHEntry")]
        [WebAPI.InfyAuthorize(PermissionSet.purchasereport, InfyPOS.Common.Action.View)]
        public async Task<IHttpActionResult> SearchWHEntry(ActionRequest<WarehouseReportCreteria> request)
        {
            WarehouseReportDataService WarehouseReportData = new WarehouseReportDataService();
            if (InfyPOS.Services.ConfigManager.Instance(WarehouseReportData.Context.OrganizationID).Config != null)
                request.Item.multicompany = InfyPOS.Services.ConfigManager.Instance(WarehouseReportData.Context.OrganizationID).Config.system_multicompany;

            List<POEntry> result = await Task<List<POEntry>>.Run(() => WarehouseReportData.SearchWHEntry(request.Item));
            return this.Ok(result);
        }
        [HttpPost]
        [Route("SearchPurchaseHSN")]
        [WebAPI.InfyAuthorize(PermissionSet.purchasereport, InfyPOS.Common.Action.View)]
        public async Task<IHttpActionResult> SearchPurchaseHSN(ActionRequest<WarehouseReportCreteria> request)
        {
            WarehouseReportDataService WarehouseReportData = new WarehouseReportDataService();
            if (InfyPOS.Services.ConfigManager.Instance(WarehouseReportData.Context.OrganizationID).Config != null)
                request.Item.multicompany = InfyPOS.Services.ConfigManager.Instance(WarehouseReportData.Context.OrganizationID).Config.system_multicompany;

            POReport result = await Task<POReport>.Run(() => WarehouseReportData.SearchPurchaseHSN(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("SearchPurchaseTax")]
        [WebAPI.InfyAuthorize(PermissionSet.purchasereport, InfyPOS.Common.Action.View)]
        public async Task<IHttpActionResult> SearchPurchaseTax(ActionRequest<WarehouseReportCreteria> request)
        {
            WarehouseReportDataService WarehouseReportData = new WarehouseReportDataService();
            if (InfyPOS.Services.ConfigManager.Instance(WarehouseReportData.Context.OrganizationID).Config != null)
                request.Item.multicompany = InfyPOS.Services.ConfigManager.Instance(WarehouseReportData.Context.OrganizationID).Config.system_multicompany;

            POReport result = await Task<POReport>.Run(() => WarehouseReportData.SearchPurchaseTax(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("SearchPurchaseTaxSummary")]
        [WebAPI.InfyAuthorize(PermissionSet.purchasereport, InfyPOS.Common.Action.View)]
        public async Task<IHttpActionResult> SearchPurchaseTaxSummary(ActionRequest<WarehouseReportCreteria> request)
        {
            WarehouseReportDataService WarehouseReportData = new WarehouseReportDataService();
            if (InfyPOS.Services.ConfigManager.Instance(WarehouseReportData.Context.OrganizationID).Config != null)
                request.Item.multicompany = InfyPOS.Services.ConfigManager.Instance(WarehouseReportData.Context.OrganizationID).Config.system_multicompany;

            List<SalesTaxSummary> result = await Task<List<SalesTaxSummary>>.Run(() => WarehouseReportData.SearchPurchaseTaxSummary(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("SearchPurchaseItemDetails")]
        [WebAPI.InfyAuthorize(PermissionSet.purchasereport, InfyPOS.Common.Action.View)]
        public async Task<IHttpActionResult> SearchPurchaseItemDetails(ActionRequest<WarehouseReportCreteria> request)
        {
            WarehouseReportDataService WarehouseReportData = new WarehouseReportDataService();
            if (InfyPOS.Services.ConfigManager.Instance(WarehouseReportData.Context.OrganizationID).Config != null)
                request.Item.multicompany = InfyPOS.Services.ConfigManager.Instance(WarehouseReportData.Context.OrganizationID).Config.system_multicompany;

            List<PurhcaseItemDetails> result = await Task<List<PurhcaseItemDetails>>.Run(() => WarehouseReportData.SearchPurchaseItemDetails(request.Item));
            return this.Ok(result);
        }
        [HttpPost]
        [Route("SearchPurchaseSectionwise")]
        [WebAPI.InfyAuthorize(PermissionSet.purchasereport, InfyPOS.Common.Action.View)]
        public async Task<IHttpActionResult> SearchPurchaseSectionwise(ActionRequest<WarehouseReportCreteria> request)
        {
            WarehouseReportDataService WarehouseReportData = new WarehouseReportDataService();
            if (InfyPOS.Services.ConfigManager.Instance(WarehouseReportData.Context.OrganizationID).Config != null)
                request.Item.multicompany = InfyPOS.Services.ConfigManager.Instance(WarehouseReportData.Context.OrganizationID).Config.system_multicompany;

            List<PurhcaseItemDetails> result = await Task<List<PurhcaseItemDetails>>.Run(() => WarehouseReportData.SearchPurchaseSectionwise(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("ROLStoreView")]
        public async Task<IHttpActionResult> ROLStoreView(ActionRequest<ROLStoreView> request)
        {
            WarehouseReportDataService WarehouseReportData = new WarehouseReportDataService();
            List<ROLStoreView> result = await Task<List<ROLStoreView>>.Run(() => WarehouseReportData.ROLStoreView(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("ROLStoreViewDetails")]
        public async Task<IHttpActionResult> ROLStoreViewDetails(ActionRequest<ROLStoreView> request)
        {
            WarehouseReportDataService WarehouseReportData = new WarehouseReportDataService();
            List<ROLStoreView> result = await Task<List<ROLStoreView>>.Run(() => WarehouseReportData.ROLStoreViewDetails(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("SearchPurchaseReturnSectionwise")]
        [WebAPI.InfyAuthorize(PermissionSet.purchasereturnreport, InfyPOS.Common.Action.View)]
        public async Task<IHttpActionResult> SearchPurchaseReturnSectionwise(ActionRequest<WarehouseReportCreteria> request)
        {
            WarehouseReportDataService WarehouseReportData = new WarehouseReportDataService();
            if (InfyPOS.Services.ConfigManager.Instance(WarehouseReportData.Context.OrganizationID).Config != null)
                request.Item.multicompany = InfyPOS.Services.ConfigManager.Instance(WarehouseReportData.Context.OrganizationID).Config.system_multicompany;

            List<PurhcaseItemDetails> result = await Task<List<PurhcaseItemDetails>>.Run(() => WarehouseReportData.SearchPurchaseReturnSectionwise(request.Item));
            return this.Ok(result);
        }
    }
}