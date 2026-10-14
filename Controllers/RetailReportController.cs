
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
using System.Web;

namespace InfyPOS.Controllers
{
    [RoutePrefix("RetailReportService")]
    public class RetailReportController : ApiController
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
        [Route("SearchTextAllTransaction")]
        [WebAPI.InfyAuthorize(PermissionSet.textdaysummary, InfyPOS.Common.Action.View)]
        public async Task<IHttpActionResult> SearchTextAllTransaction(ActionRequest<SaleRetailCreteria> request)
        {
            if (!ValidateLocation(request.Item.locationid))
                throw new Exception("Don't have permission to access all location data");

            RetailReportDataService RetailReportData = new RetailReportDataService();
            if (InfyPOS.Services.ConfigManager.Instance(RetailReportData.Context.OrganizationID).Config != null)
                request.Item.multicompany = InfyPOS.Services.ConfigManager.Instance(RetailReportData.Context.OrganizationID).Config.system_multicompany;

            string result = await Task<string>.Run(() => RetailReportData.SearchTextAllTransaction(request.Item.locationid, request.Item.fromdate, request.Item.todate, request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("TextDaySummary")]
        [WebAPI.InfyAuthorize(PermissionSet.textdaysummary, InfyPOS.Common.Action.View)]
        public async Task<IHttpActionResult> TextDaySummary(ActionRequest<SaleRetailCreteria> request)
        {
            if (!ValidateLocation(request.Item.locationid))
                throw new Exception("Don't have permission to access all location data");

            RetailReportDataService RetailReportData = new RetailReportDataService();
            if (InfyPOS.Services.ConfigManager.Instance(RetailReportData.Context.OrganizationID).Config != null)
                request.Item.multicompany = InfyPOS.Services.ConfigManager.Instance(RetailReportData.Context.OrganizationID).Config.system_multicompany;

            string result = await Task<string>.Run(() => RetailReportData.TextDaySummary(request.Item.locationid,request.Item.fromdate,request.Item.todate, request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("SaveStockImages")]
        public async Task<IHttpActionResult> SaveStockImages(ActionRequest<InfyPOS.Data.RetailReportDataService.DesignStockImageRequest> request)
        {
            RetailReportDataService RetailReportData = new RetailReportDataService();
            string result = await Task<string>.Run(() => RetailReportData.SaveStockImages(request.Item));
            return this.Ok(result);
        }


        [HttpPost]
        [Route("SearchDesignList")]
        public async Task<IHttpActionResult> SearchDesignList(ActionRequest<InfyPOS.Data.RetailReportDataService.SearchDesignCriteria> request)
        {
            RetailReportDataService RetailReportData = new RetailReportDataService();
            List<string> result = await Task<List<string>>.Run(() => RetailReportData.SearchDesignList(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("SearchDesignwiseStock")]
        public async Task<IHttpActionResult> SearchDesignwiseStock(ActionRequest<InfyPOS.Data.RetailReportDataService.SearchDesignCriteria> request)
        {
            RetailReportDataService RetailReportData = new RetailReportDataService();
            InfyPOS.Data.RetailReportDataService.DesignStockResponse result = await Task<InfyPOS.Data.RetailReportDataService.DesignStockResponse>.Run(() => RetailReportData.SearchDesignwiseStock(request.Item));
            return this.Ok(result);
        }


        [HttpPost]
        [Route("SyncOnline")]
        public async Task<IHttpActionResult> SyncOnline(ActionRequest<InfyPOS.Data.RetailReportDataService.SyncOnlineRequest> request)
        {
            RetailReportDataService RetailReportData = new RetailReportDataService();
            bool result = await Task<bool>.Run(() => RetailReportData.SyncOnline(request.Item));
            return this.Ok(result);
        }

        [HttpGet]
        [Route("Cashierstatus/{locationid}/{counterid}")]
        public async Task<IHttpActionResult> Cashierstatus(string locationid,string counterid)
        {
            RetailReportDataService RetailReportData = new RetailReportDataService();
            CashierStatus status = await Task<CashierStatus>.Run(() =>RetailReportData.GetCashierStatus(long.Parse(locationid), long.Parse(counterid), RetailReportData.Context.UserID));
            return this.Ok(status);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("DayTransaction/{orgid}/{billdate}/{companyid}/{locationid}/{isb2b}")]
        public async Task<IHttpActionResult> DayTransaction(string orgid,string billdate,string companyid,string locationid,string isb2b)
        {
            RetailReportDataService RetailReportData = new RetailReportDataService();
            byte[] content = await Task< InfyPOS.Controllers.ImageController.ExcelDocument >.Run(() => 
            RetailReportData.DayTransactionXLSX(orgid, billdate, companyid, locationid, isb2b));
            return new FileActionResult(true, "Products.xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", new ByteArrayContent(content));
        }
        [AllowAnonymous]
        [HttpGet]
        [Route("DownloadSalesBarcode/{orgid}/{fromdate}/{todate}/{companyid}/{locationid}/{isb2b}")]
        public async Task<IHttpActionResult> DownloadSalesBarcode(string orgid, string fromdate, string todate, string companyid, string locationid, string isb2b)
        {
            RetailReportDataService RetailReportData = new RetailReportDataService();
            byte[] content = await Task<InfyPOS.Controllers.ImageController.ExcelDocument>.Run(() =>
            RetailReportData.DownloadSalesBarcode(orgid, fromdate, todate, companyid, locationid, isb2b));
            return new FileActionResult(true, "Products.xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", new ByteArrayContent(content));
        }


        [HttpPost]
        [Route("SearchUnsettledSales")]
        [WebAPI.InfyAuthorize(PermissionSet.unsettledsalesreport, InfyPOS.Common.Action.View)]
        public async Task<IHttpActionResult> SearchUnsettledSales(ActionRequest<SaleRetailCreteria> request)
        {
            if (!ValidateLocation(request.Item.locationid))
                throw new Exception("Don't have permission to access all location data");

            RetailReportDataService RetailReportData = new RetailReportDataService();
            if (InfyPOS.Services.ConfigManager.Instance(RetailReportData.Context.OrganizationID).Config != null)
                request.Item.multicompany = InfyPOS.Services.ConfigManager.Instance(RetailReportData.Context.OrganizationID).Config.system_multicompany;

            List<RetailReport> result = await Task<List<RetailReport>>.Run(() => RetailReportData.SearchSales(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("SearchSales")]
        //[WebAPI.InfyAuthorize(PermissionSet.salesreport, InfyPOS.Common.Action.View)]
        public async Task<IHttpActionResult> SearchSales(ActionRequest<SaleRetailCreteria> request)
        {
            if (!ValidateLocation(request.Item.locationid))
                throw new Exception("Don't have permission to access all location data");

            RetailReportDataService RetailReportData = new RetailReportDataService();
            if (InfyPOS.Services.ConfigManager.Instance(RetailReportData.Context.OrganizationID).Config != null)
                request.Item.multicompany = InfyPOS.Services.ConfigManager.Instance(RetailReportData.Context.OrganizationID).Config.system_multicompany;

            if (request.Item.mode == "7")
            {
                return this.Ok(RetailReportData.SearchBillNoSummary(request.Item));
            }
            if (request.Item.mode == "8")
            {
                return this.Ok(RetailReportData.SearchBillTaxValidation(request.Item));
            }
            if (request.Item.mode == "9")
            {
                return this.Ok(RetailReportData.SearchMobileDaySummary(request.Item));
            }
            List<RetailReport> result = await Task<List<RetailReport>>.Run(() => RetailReportData.SearchSales(request.Item));
            return this.Ok(result);
        }
        [HttpPost]
        [Route("SearchRewardConsumption")]
        public async Task<IHttpActionResult> SearchRewardConsumption(ActionRequest<SaleRetailCreteria> request)
        {
            if (!ValidateLocation(request.Item.locationid))
                throw new Exception("Don't have permission to access all location data");

            RetailReportDataService RetailReportData = new RetailReportDataService();
            List<RetailReport> result = await Task<List<RetailReport>>.Run(() => RetailReportData.SearchRewardConsumption(request.Item));
            return this.Ok(result);
        }

        bool ValidateLocationCompany(long companyid,long locationid)
        {
            if (locationid <= 0)
            {
                if (System.Web.HttpContext.Current != null)
                {
                    var Context = new UserClaim(System.Web.HttpContext.Current.Request.GetOwinContext().Authentication);
                    if (Context.accessiblelocation == null || !Context.accessiblelocation.Contains(-1))
                        return false;
                }
            }
            if (companyid <= 0)
            {
                if (System.Web.HttpContext.Current != null)
                {
                    var Context = new UserClaim(System.Web.HttpContext.Current.Request.GetOwinContext().Authentication);
                    if (Context.accessiblecompany == null || !Context.accessiblecompany.Contains(-1))
                        return false;
                }
            }
            return true;
        }

        bool ValidateLocation(long locationid)
        {
            if (locationid > 0) return true;
            if (System.Web.HttpContext.Current != null)
            {
                var Context = new UserClaim(System.Web.HttpContext.Current.Request.GetOwinContext().Authentication);
                if (Context.accessiblelocation == null || !Context.accessiblelocation.Contains(-1))
                    return false;
            }
            return true;
        }

        bool ValidateCompany(long companyid)
        {
            if (companyid > 0) return true;
            if (System.Web.HttpContext.Current != null)
            {
                var Context = new UserClaim(System.Web.HttpContext.Current.Request.GetOwinContext().Authentication);
                if (Context.accessiblecompany == null || !Context.accessiblecompany.Contains(-1))
                    return false;
            }
            return true;
        }

        [HttpPost]
        [Route("SearchDaySummary")]
        [WebAPI.InfyAuthorize(PermissionSet.salesreport, InfyPOS.Common.Action.View)]
        public async Task<IHttpActionResult> SearchDaySummary(ActionRequest<SaleRetailCreteria> request)
        {
            if (!ValidateLocation(request.Item.locationid))
                return this.NotFound();
            RetailReportDataService RetailReportData = new RetailReportDataService();
            if (InfyPOS.Services.ConfigManager.Instance(RetailReportData.Context.OrganizationID).Config != null)
                request.Item.multicompany = InfyPOS.Services.ConfigManager.Instance(RetailReportData.Context.OrganizationID).Config.system_multicompany;


            DaySummary result = await Task<DaySummary>.Run(() => RetailReportData.SearchDaySummary(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("SearchDaySalesSummary")]
        [WebAPI.InfyAuthorize(PermissionSet.salesreport, InfyPOS.Common.Action.View)]
        public async Task<IHttpActionResult> SearchDaySalesSummary(ActionRequest<SaleRetailCreteria> request)
        {
            if (!ValidateLocation(request.Item.locationid))
                return this.NotFound();

            RetailReportDataService RetailReportData = new RetailReportDataService();
            if (InfyPOS.Services.ConfigManager.Instance(RetailReportData.Context.OrganizationID).Config != null)
                request.Item.multicompany = InfyPOS.Services.ConfigManager.Instance(RetailReportData.Context.OrganizationID).Config.system_multicompany;

            List<DaySalesSummary> result = await Task<List<DaySalesSummary>>.Run(() => RetailReportData.SearchDaySalesSummary(request.Item));
            return this.Ok(result);
        }


        [HttpPost]
        [Route("SearchSoldBarcodes")]
        [WebAPI.InfyAuthorize(PermissionSet.salesreport, InfyPOS.Common.Action.View)]
        public async Task<IHttpActionResult> SearchSoldBarcodes(ActionRequest<StockReportCreteria> request)
        {

            RetailReportDataService RetailReportData = new RetailReportDataService();
            if (InfyPOS.Services.ConfigManager.Instance(RetailReportData.Context.OrganizationID).Config != null)
                request.Item.multicompany = InfyPOS.Services.ConfigManager.Instance(RetailReportData.Context.OrganizationID).Config.system_multicompany;

            List<SoldBarcodes> result = await Task<List<SoldBarcodes>>.Run(() => RetailReportData.SearchSoldBarcodes(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("SearchDaySummaryAutoSettlement")]
        [WebAPI.InfyAuthorize(PermissionSet.daysalesreport, InfyPOS.Common.Action.View)]
        public async Task<IHttpActionResult> SearchDaySummaryAutoSettlement(ActionRequest<SaleRetailCreteria> request)
        {
            if (!ValidateLocation(request.Item.locationid))
                return this.NotFound();

            RetailReportDataService RetailReportData = new RetailReportDataService();
            if (InfyPOS.Services.ConfigManager.Instance(RetailReportData.Context.OrganizationID).Config != null)
                request.Item.multicompany = InfyPOS.Services.ConfigManager.Instance(RetailReportData.Context.OrganizationID).Config.system_multicompany;


            DaySummary result = await Task<DaySummary>.Run(() => RetailReportData.SearchDaySummaryAutoSettlement(request.Item));
            return this.Ok(result);
        }

        [HttpGet]
        [Route("LocationView")]
        public async Task<IHttpActionResult> LocationView()
        {
            RetailReportDataService RetailReportData = new RetailReportDataService();
            List<LocationViewItem> result = await Task<List<LocationViewItem>>.Run(() => RetailReportData.GetLocationView());
            return this.Ok(result);
        }

        [HttpPost]
        [Route("GetNotifications")]
        public async Task<IHttpActionResult> GetNotifications()
        {
            RetailReportDataService RetailReportData = new RetailReportDataService();
            NotificationSummary result = await Task<NotificationSummary>.Run(() => RetailReportData.GetNotifications());
            return this.Ok(result);
        }

        [HttpPost]
        [Route("SearchSalesSnapshot")]
        [WebAPI.InfyAuthorize(PermissionSet.salesreport, InfyPOS.Common.Action.View)]
        public async Task<IHttpActionResult> SearchSalesSnapshot(ActionRequest<SaleRetailCreteria> request)
        {
            if (!ValidateLocation(request.Item.locationid))
                return this.NotFound();

            RetailReportDataService RetailReportData = new RetailReportDataService();
            if (InfyPOS.Services.ConfigManager.Instance(RetailReportData.Context.OrganizationID).Config != null)
                request.Item.multicompany = InfyPOS.Services.ConfigManager.Instance(RetailReportData.Context.OrganizationID).Config.system_multicompany;


            List<SalesSnapshot> result = await Task<List<SalesSnapshot>>.Run(() => RetailReportData.SearchSalesSnapshot(request.Item));
            return this.Ok(result);
        }
        [HttpPost]
        [Route("SearchSalesMargin")]
        [WebAPI.InfyAuthorize(PermissionSet.salesreport, InfyPOS.Common.Action.View)]
        public async Task<IHttpActionResult> SearchSalesMargin(ActionRequest<SaleRetailCreteria> request)
        {
            RetailReportDataService RetailReportData = new RetailReportDataService();
            if (InfyPOS.Services.ConfigManager.Instance(RetailReportData.Context.OrganizationID).Config != null)
                request.Item.multicompany = InfyPOS.Services.ConfigManager.Instance(RetailReportData.Context.OrganizationID).Config.system_multicompany;


            List<RetailReport> result = await Task<List<RetailReport>>.Run(() => RetailReportData.SearchSalesMargin(request.Item));
            return this.Ok(result);
        }
        [HttpPost]
        [Route("SearchSalesReturn")]
        [WebAPI.InfyAuthorize(PermissionSet.salesreturnreport, InfyPOS.Common.Action.View)]
        public async Task<IHttpActionResult> SearchSalesReturn(ActionRequest<SaleRetailCreteria> request)
        {
            if (!ValidateLocation(request.Item.locationid))
                return this.NotFound();

            RetailReportDataService RetailReportData = new RetailReportDataService();
            if (InfyPOS.Services.ConfigManager.Instance(RetailReportData.Context.OrganizationID).Config != null)
                request.Item.multicompany = InfyPOS.Services.ConfigManager.Instance(RetailReportData.Context.OrganizationID).Config.system_multicompany;


            List<RetailReport> result = await Task<List<RetailReport>>.Run(() => RetailReportData.SearchSalesReturn(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("SearchSettlementByTax")]
        [WebAPI.InfyAuthorize(PermissionSet.settlementreport, InfyPOS.Common.Action.View)]
        public async Task<IHttpActionResult> SearchSettlementByTax(ActionRequest<RetailReportCreteria> request)
        {
            if (!ValidateLocation(request.Item.locationid))
                return this.NotFound();

            RetailReportDataService RetailReportData = new RetailReportDataService();
            if (InfyPOS.Services.ConfigManager.Instance(RetailReportData.Context.OrganizationID).Config != null)
                request.Item.multicompany = InfyPOS.Services.ConfigManager.Instance(RetailReportData.Context.OrganizationID).Config.system_multicompany;


            List<SettlementReport> result = await Task<List<SettlementReport>>.Run(() => RetailReportData.SearchSettlementByTax(request.Item));
            return this.Ok(result);
        }
        [HttpPost]
        [Route("SearchSettlement")]
        [WebAPI.InfyAuthorize(PermissionSet.settlementreport, InfyPOS.Common.Action.View)]
        public async Task<IHttpActionResult> SearchSettlement(ActionRequest<RetailReportCreteria> request)
        {
            if (!ValidateLocation(request.Item.locationid))
                return this.NotFound();

            RetailReportDataService RetailReportData = new RetailReportDataService();
            if (InfyPOS.Services.ConfigManager.Instance(RetailReportData.Context.OrganizationID).Config != null)
                request.Item.multicompany = InfyPOS.Services.ConfigManager.Instance(RetailReportData.Context.OrganizationID).Config.system_multicompany;


            List<SettlementReport> result = await Task<List<SettlementReport>>.Run(() => RetailReportData.SearchSettlement(request.Item));
            return this.Ok(result);
        }
        [HttpPost]
        [Route("SearchSettlementForReconcile")]
        [WebAPI.InfyAuthorize(PermissionSet.settlementreport, InfyPOS.Common.Action.View)]
        public async Task<IHttpActionResult> SearchSettlementForReconcile(ActionRequest<RetailReportCreteria> request)
        {
            if (!ValidateLocation(request.Item.locationid))
                return this.NotFound();

            RetailReportDataService RetailReportData = new RetailReportDataService();
            if (InfyPOS.Services.ConfigManager.Instance(RetailReportData.Context.OrganizationID).Config != null)
                request.Item.multicompany = InfyPOS.Services.ConfigManager.Instance(RetailReportData.Context.OrganizationID).Config.system_multicompany;


            List<SettlementReport> result = await Task<List<SettlementReport>>.Run(() => RetailReportData.SearchSettlementForReconcile(request.Item));
            return this.Ok(result);
        }
        [HttpPost]
        [Route("SearchSaleVsSettlement")]
        [WebAPI.InfyAuthorize(PermissionSet.settlementreport, InfyPOS.Common.Action.View)]
        public async Task<IHttpActionResult> SearchSaleVsSettlement(ActionRequest<RetailReportCreteria> request)
        {
            if (!ValidateLocation(request.Item.locationid))
                return this.NotFound();

            RetailReportDataService RetailReportData = new RetailReportDataService();
            if (InfyPOS.Services.ConfigManager.Instance(RetailReportData.Context.OrganizationID).Config != null)
                request.Item.multicompany = InfyPOS.Services.ConfigManager.Instance(RetailReportData.Context.OrganizationID).Config.system_multicompany;


            List<SaleVsSettlement> result = await Task<List<SaleVsSettlement>>.Run(() => RetailReportData.SearchSaleVsSettlement(request.Item, false));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("SearchSaleVsStock")]
        [WebAPI.InfyAuthorize(PermissionSet.settlementreport, InfyPOS.Common.Action.View)]
        public async Task<IHttpActionResult> SearchSalesStockSummary(ActionRequest<SalesStockCreteria> request)
        {
            if (!ValidateLocation(request.Item.locationid))
                return this.NotFound();

            WarehouseReportDataService RetailReportData = new WarehouseReportDataService();
           

            SalesStockReport result = await Task<SalesStockReport>.Run(() => RetailReportData.SearchSalesStockSummary(request.Item));
            return this.Ok(result);
        }
        [HttpPost]
        [Route("SearchSaleVsPurchaseVsStock")]
        [WebAPI.InfyAuthorize(PermissionSet.settlementreport, InfyPOS.Common.Action.View)]
        public async Task<IHttpActionResult> SearchSaleVsPurchaseVsStock(ActionRequest<SalesStockCreteria> request)
        {
            if (!ValidateLocation(request.Item.locationid))
                return this.NotFound();

            WarehouseReportDataService RetailReportData = new WarehouseReportDataService();


            SalesStockReport result = await Task<SalesStockReport>.Run(() => RetailReportData.SearchSaleVsPurchaseVsStock(request.Item));
            return this.Ok(result);
        }
        [HttpPost]
        [Route("SearchInward")]
        [WebAPI.InfyAuthorize(PermissionSet.inwardreport, InfyPOS.Common.Action.View)]
        public async Task<IHttpActionResult> SearchInward(ActionRequest<RetailReportCreteria> request)
        {
            RetailReportDataService RetailReportData = new RetailReportDataService();
            if (InfyPOS.Services.ConfigManager.Instance(RetailReportData.Context.OrganizationID).Config != null)
                request.Item.multicompany = InfyPOS.Services.ConfigManager.Instance(RetailReportData.Context.OrganizationID).Config.system_multicompany;


            List<InwardReport> result = await Task<List<InwardReport>>.Run(() => RetailReportData.SearchInward(request.Item));
            return this.Ok(result);
        }
        [HttpPost]
        [Route("SearchBarcode")]
        [WebAPI.InfyAuthorize(PermissionSet.itemsearch, InfyPOS.Common.Action.View)]
        public async Task<IHttpActionResult> SearchBarcode(ActionRequest<RetailReportCreteria> request)
        {
            RetailReportDataService RetailReportData = new RetailReportDataService();
            if (InfyPOS.Services.ConfigManager.Instance(RetailReportData.Context.OrganizationID).Config != null)
                request.Item.multicompany = InfyPOS.Services.ConfigManager.Instance(RetailReportData.Context.OrganizationID).Config.system_multicompany;

            List<RetailReport> result = await Task<List<RetailReport>>.Run(() => RetailReportData.SearchBarcode(request.Item));
            return this.Ok(result);
        }
        [HttpPost]
        [Route("SearchStock")]
        //[WebAPI.InfyAuthorize(PermissionSet.retailstockreport, InfyPOS.Common.Action.View)]
        public async Task<IHttpActionResult> SearchStock(ActionRequest<StockReportCreteria> request)
        {
            if (!ValidateLocation(request.Item.locationid))
                throw new Exception("Don't have permission to access all location data");

            RetailReportDataService RetailReportData = new RetailReportDataService();
            if (InfyPOS.Services.ConfigManager.Instance(RetailReportData.Context.OrganizationID).Config != null)
                request.Item.multicompany = InfyPOS.Services.ConfigManager.Instance(RetailReportData.Context.OrganizationID).Config.system_multicompany;

            if (((request.Item.barcodes != null && request.Item.barcodes.Count>0) || (request.Item.barcode != null && request.Item.barcode.Length > 0)) &&
                (InfyPOS.Services.ConfigManager.Instance(RetailReportData.Context.OrganizationID).Config.system_fixed_barcodes ||
                InfyPOS.Services.ConfigManager.Instance(RetailReportData.Context.OrganizationID).Config.system_hyper_uan ||
                InfyPOS.Services.ConfigManager.Instance(RetailReportData.Context.OrganizationID).Config.system_ismfgapplication))
            {
                request.Item.searchbarcodeonuan = false;
                List<StockReport> result = await Task<List<StockReport>>.Run(() => RetailReportData.SearchStock(request.Item));
                if ((request.Item.barcode != null && request.Item.barcode.Length > 0) && result.Exists(e => e.barcode == request.Item.barcode))
                    return this.Ok(result);

                if ((request.Item.barcodes != null && request.Item.barcodes.Count > 0) &&
                    request.Item.barcodes.All(e=> result.Exists(x=>x.barcode == e)))
                    return this.Ok(result);

                request.Item.searchbarcodeonuan = true;
                List<StockReport> normalbarcodesresult = await Task<List<StockReport>>.Run(() => RetailReportData.SearchStock(request.Item));
                result.AddRange(normalbarcodesresult);
                return this.Ok(result);
            }
            else
            {
                request.Item.searchbarcodeonuan = false;
                List<StockReport> result = await Task<List<StockReport>>.Run(() => RetailReportData.SearchStock(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("SearchStock4File")]
        //[WebAPI.InfyAuthorize(PermissionSet.retailstockreport, InfyPOS.Common.Action.View)]
        public async Task<IHttpActionResult> SearchStock4File(ActionRequest<StockReportCreteria> request)
        {
            RetailReportDataService RetailReportData = new RetailReportDataService();
            if (((request.Item.barcodes != null && request.Item.barcodes.Count > 0) || (request.Item.barcode != null && request.Item.barcode.Length > 0)) &&
                (InfyPOS.Services.ConfigManager.Instance(RetailReportData.Context.OrganizationID).Config.system_fixed_barcodes ||
                InfyPOS.Services.ConfigManager.Instance(RetailReportData.Context.OrganizationID).Config.system_hyper_uan ||
                InfyPOS.Services.ConfigManager.Instance(RetailReportData.Context.OrganizationID).Config.system_ismfgapplication))
            {
                request.Item.searchbarcodeonuan = false;
                List<StockFileItem> result = await Task<List<StockFileItem>>.Run(() => RetailReportData.SearchStock4File(request.Item));
                if ((request.Item.barcode != null && request.Item.barcode.Length > 0) && result.Exists(e => e.barcode == request.Item.barcode))
                    return this.Ok(result);

                if ((request.Item.barcodes != null && request.Item.barcodes.Count > 0) &&
                    request.Item.barcodes.All(e => result.Exists(x => x.barcode == e)))
                    return this.Ok(result);

                request.Item.searchbarcodeonuan = true;
                List<StockFileItem> normalbarcodesresult = await Task<List<StockFileItem>>.Run(() => RetailReportData.SearchStock4File(request.Item));
                result.AddRange(normalbarcodesresult);
                return this.Ok(result);
            }
            else
            {
                request.Item.searchbarcodeonuan = false;
                List<StockFileItem> result = await Task<List<StockFileItem>>.Run(() => RetailReportData.SearchStock4File(request.Item));
                return this.Ok(result);
            }
        }


        [HttpPost]
        [Route("SearchShelfStock")]
        //[WebAPI.InfyAuthorize(PermissionSet.whstockreport, InfyPOS.Common.Action.View)]
        public async Task<IHttpActionResult> SearchShelfStock(ActionRequest<StockReportCreteria> request)
        {
            if (!ValidateLocation(request.Item.locationid))
                throw new Exception("Don't have permission to access all location data");

            RetailReportDataService RetailReportData = new RetailReportDataService();
            if (InfyPOS.Services.ConfigManager.Instance(RetailReportData.Context.OrganizationID).Config != null)
                request.Item.multicompany = InfyPOS.Services.ConfigManager.Instance(RetailReportData.Context.OrganizationID).Config.system_multicompany;

            List<StockReport> result = await Task<List<StockReport>>.Run(() => RetailReportData.SearchShelfStock(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("SearchAgeStock")]
        //[WebAPI.InfyAuthorize(PermissionSet.whstockreport, InfyPOS.Common.Action.View)]
        public async Task<IHttpActionResult> SearchAgeStock(ActionRequest<StockReportCreteria> request)
        {
            if (!ValidateLocation(request.Item.locationid))
                throw new Exception("Don't have permission to access all location data");

            RetailReportDataService RetailReportData = new RetailReportDataService();
            if (InfyPOS.Services.ConfigManager.Instance(RetailReportData.Context.OrganizationID).Config != null)
                request.Item.multicompany = InfyPOS.Services.ConfigManager.Instance(RetailReportData.Context.OrganizationID).Config.system_multicompany;

            List<StockAgeReport> result = await Task<List<StockAgeReport>>.Run(() => RetailReportData.SearchAgeStock(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("SearchCashCounter")]
        public async Task<IHttpActionResult> SearchCashCounter(ActionRequest<RetailReportCreteria> request)
        {
            RetailReportDataService RetailReportData = new RetailReportDataService();
            if (InfyPOS.Services.ConfigManager.Instance(RetailReportData.Context.OrganizationID).Config != null)
                request.Item.multicompany = InfyPOS.Services.ConfigManager.Instance(RetailReportData.Context.OrganizationID).Config.system_multicompany;


            List<RetailReport> result = await Task<List<RetailReport>>.Run(() => RetailReportData.SearchCashCounter(request.Item));
            return this.Ok(result);
        }
        [HttpPost]
        [Route("SearchSalesTax")]
        [WebAPI.InfyAuthorize(PermissionSet.salestaxreport, InfyPOS.Common.Action.View)]
        public async Task<IHttpActionResult> SearchSalesTax(ActionRequest<RetailReportCreteria> request)
        {
            if (!ValidateLocation(request.Item.locationid))
                throw new Exception("Don't have permission to access all location data");

            RetailReportDataService RetailReportData = new RetailReportDataService();
            if (InfyPOS.Services.ConfigManager.Instance(RetailReportData.Context.OrganizationID).Config != null)
                request.Item.multicompany = InfyPOS.Services.ConfigManager.Instance(RetailReportData.Context.OrganizationID).Config.system_multicompany;


            SalesTaxReport result = await Task<SalesTaxReport>.Run(() => RetailReportData.SearchSalesTax(request.Item));
            return this.Ok(result);
        }
        [HttpPost]
        [Route("SearchSalesTaxSummary")]
        [WebAPI.InfyAuthorize(PermissionSet.salestaxreport, InfyPOS.Common.Action.View)]
        public async Task<IHttpActionResult> SearchSalesTaxSummary(ActionRequest<RetailReportCreteria> request)
        {
            if (!ValidateLocation(request.Item.locationid))
                throw new Exception("Don't have permission to access all location data");

            RetailReportDataService RetailReportData = new RetailReportDataService();
            if (InfyPOS.Services.ConfigManager.Instance(RetailReportData.Context.OrganizationID).Config != null)
                request.Item.multicompany = InfyPOS.Services.ConfigManager.Instance(RetailReportData.Context.OrganizationID).Config.system_multicompany;

            var queueManager = InfyPOS.Processors.WorkflowQueueManager.Instance;
            if (!queueManager.Enabled)
            {
                List<SalesTaxSummary> result = await Task<List<SalesTaxSummary>>.Run(() => RetailReportData.SearchSalesTaxSummary(request.Item));
                return this.Ok(result);
            }
            else
            {
                InboxDataService ActualDataService = new InboxDataService();
                var organizationId = ActualDataService.Context?.OrganizationID ?? 0;
                var userId = ActualDataService.Context?.UserID.ToString() ?? "";
                var response = await queueManager.ExecuteWithTimeout<object>(async () =>
                {
                    return await Task<List<SalesTaxSummary>>.Run(() => RetailReportData.SearchSalesTaxSummary(request.Item));
                }, "RetailReport.SearchSalesTaxSummary", request.Item, 3, organizationId, userId);

                if (response.IsCompleted)
                {
                    return this.Ok(response.Result);
                }
                else
                {
                    return this.ResponseMessage(response.TimoutResponse);
                }
            }

        }

        [HttpPost]
        [Route("SearchSalesTaxHSNSummary")]
        [WebAPI.InfyAuthorize(PermissionSet.salestaxreport, InfyPOS.Common.Action.View)]
        public async Task<IHttpActionResult> SearchSalesTaxHSNSummary(ActionRequest<RetailReportCreteria> request)
        {
            if (!ValidateLocation(request.Item.locationid))
                throw new Exception("Don't have permission to access all location data");

            RetailReportDataService RetailReportData = new RetailReportDataService();
            if (InfyPOS.Services.ConfigManager.Instance(RetailReportData.Context.OrganizationID).Config != null)
                request.Item.multicompany = InfyPOS.Services.ConfigManager.Instance(RetailReportData.Context.OrganizationID).Config.system_multicompany;

            List<SalesTaxHSNSummary> result = await Task<List<SalesTaxHSNSummary>>.Run(() => RetailReportData.SearchSalesTaxHSNSummary(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("SearchSalesTaxFullSummary")]
        [WebAPI.InfyAuthorize(PermissionSet.salestaxreport, InfyPOS.Common.Action.View)]
        public async Task<IHttpActionResult> SearchSalesTaxFullSummary(ActionRequest<RetailReportCreteria> request)
        {
            if (!ValidateLocation(request.Item.locationid))
                throw new Exception("Don't have permission to access all location data");

            RetailReportDataService RetailReportData = new RetailReportDataService();
            if (InfyPOS.Services.ConfigManager.Instance(RetailReportData.Context.OrganizationID).Config != null)
                request.Item.multicompany = InfyPOS.Services.ConfigManager.Instance(RetailReportData.Context.OrganizationID).Config.system_multicompany;

            List<SalesTaxSummary> result = await Task<List<SalesTaxSummary>>.Run(() => RetailReportData.SearchSalesTaxFullSummary(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("SearchInvoicewiseSales")]
        [WebAPI.InfyAuthorize(PermissionSet.salestaxreport, InfyPOS.Common.Action.View)]
        public async Task<IHttpActionResult> SearchInvoicewiseSales(ActionRequest<RetailReportCreteria> request)
        {
            if (!ValidateLocation(request.Item.locationid))
                throw new Exception("Don't have permission to access all location data");

            RetailReportDataService RetailReportData = new RetailReportDataService();
            if (InfyPOS.Services.ConfigManager.Instance(RetailReportData.Context.OrganizationID).Config != null)
                request.Item.multicompany = InfyPOS.Services.ConfigManager.Instance(RetailReportData.Context.OrganizationID).Config.system_multicompany;


            List<SalesTaxSummary> result = await Task<List<SalesTaxSummary>>.Run(() => RetailReportData.SearchInvoicewiseSales(request.Item));
            return this.Ok(result);
        }
        [HttpPost]
        [Route("SearchCreditSalesTaxSummary")]
        [WebAPI.InfyAuthorize(PermissionSet.salestaxreport, InfyPOS.Common.Action.View)]
        public async Task<IHttpActionResult> SearchCreditSalesTaxSummary(ActionRequest<RetailReportCreteria> request)
        {
            if (!ValidateLocation(request.Item.locationid))
                throw new Exception("Don't have permission to access all location data");

            RetailReportDataService RetailReportData = new RetailReportDataService();
            if (InfyPOS.Services.ConfigManager.Instance(RetailReportData.Context.OrganizationID).Config != null)
                request.Item.multicompany = InfyPOS.Services.ConfigManager.Instance(RetailReportData.Context.OrganizationID).Config.system_multicompany;


            List<SalesTaxSummary> result = await Task<List<SalesTaxSummary>>.Run(() => RetailReportData.SearchCreditSalesTaxSummary(request.Item));
            return this.Ok(result);
        }
        [HttpPost]
        [Route("SearchCancelledBill")]
        [WebAPI.InfyAuthorize(PermissionSet.salescancelledreport, InfyPOS.Common.Action.View)]
        public async Task<IHttpActionResult> SearchCancelledBill(ActionRequest<SaleRetailCreteria> request)
        {
            if (!ValidateLocation(request.Item.locationid))
                throw new Exception("Don't have permission to access all location data");

            RetailReportDataService RetailReportData = new RetailReportDataService();
            if (InfyPOS.Services.ConfigManager.Instance(RetailReportData.Context.OrganizationID).Config != null)
                request.Item.multicompany = InfyPOS.Services.ConfigManager.Instance(RetailReportData.Context.OrganizationID).Config.system_multicompany;


            List<RetailReport> result = await Task<List<RetailReport>>.Run(() => RetailReportData.SearchCancelledBill(request.Item));
            return this.Ok(result);
        }
        [HttpPost]
        [Route("SearchCustomerPayment")]
        [WebAPI.InfyAuthorize(PermissionSet.customerpaymentreport, InfyPOS.Common.Action.View)]
        public async Task<IHttpActionResult> SearchCustomerPayment(ActionRequest<RetailReportCreteria> request)
        {
            RetailReportDataService RetailReportData = new RetailReportDataService();
            if (InfyPOS.Services.ConfigManager.Instance(RetailReportData.Context.OrganizationID).Config != null)
                request.Item.multicompany = InfyPOS.Services.ConfigManager.Instance(RetailReportData.Context.OrganizationID).Config.system_multicompany;

            List<CustomerPayReport> result = await Task<List<CustomerPayReport>>.Run(() => RetailReportData.SearchCustomerPayment(request.Item));
            return this.Ok(result);
        }
        [HttpPost]
        [Route("SearchSalesmanSales")]
        [WebAPI.InfyAuthorize(PermissionSet.salesmanreport, InfyPOS.Common.Action.View)]
        public async Task<IHttpActionResult> SearchSalesmanSales(ActionRequest<RetailReportCreteria> request)
        {
            if (!ValidateLocation(request.Item.locationid))
                throw new Exception("Don't have permission to access all location data");

            RetailReportDataService RetailReportData = new RetailReportDataService();
            if (InfyPOS.Services.ConfigManager.Instance(RetailReportData.Context.OrganizationID).Config != null)
                request.Item.multicompany = InfyPOS.Services.ConfigManager.Instance(RetailReportData.Context.OrganizationID).Config.system_multicompany;


            var queueManager = InfyPOS.Processors.WorkflowQueueManager.Instance;
            if (!queueManager.Enabled)
            {
                List<SalesmanReport> result = await Task<List<SalesmanReport>>.Run(() => RetailReportData.SearchSalesman(request.Item));
                return this.Ok(result);
            }
            else
            {
                InboxDataService ActualDataService = new InboxDataService();
                var organizationId = ActualDataService.Context?.OrganizationID ?? 0;
                var userId = ActualDataService.Context?.UserID.ToString() ?? "";
                var response = await queueManager.ExecuteWithTimeout<object>(async () =>
                {
                    return await Task<List<SalesmanReport>>.Run(() => RetailReportData.SearchSalesman(request.Item));
                }, "InboxService.Search", request.Item, 3, organizationId, userId);
                if (response.IsCompleted)
                {
                    return this.Ok(response.Result);
                }
                else
                {
                    return this.ResponseMessage(response.TimoutResponse);
                }
            }

        }
        [HttpPost]
        [Route("SearchSalesmanReceipt")]
        [WebAPI.InfyAuthorize(PermissionSet.salesmanreport, InfyPOS.Common.Action.View)]
        public async Task<IHttpActionResult> SearchSalesmanReceipt(ActionRequest<RetailReportCreteria> request)
        {
            if (!ValidateLocation(request.Item.locationid))
                throw new Exception("Don't have permission to access all location data");

            RetailReportDataService RetailReportData = new RetailReportDataService();
            if (InfyPOS.Services.ConfigManager.Instance(RetailReportData.Context.OrganizationID).Config != null)
                request.Item.multicompany = InfyPOS.Services.ConfigManager.Instance(RetailReportData.Context.OrganizationID).Config.system_multicompany;


            var queueManager = InfyPOS.Processors.WorkflowQueueManager.Instance;
            if (!queueManager.Enabled)
            {

                SalesmanReceipt result = await Task<SalesmanReceipt>.Run(() => RetailReportData.SearchSalesmanReceipt(request.Item));
                return this.Ok(result);
            }
            else
            {
                InboxDataService ActualDataService = new InboxDataService();
                var organizationId = ActualDataService.Context?.OrganizationID ?? 0;
                var userId = ActualDataService.Context?.UserID.ToString() ?? "";
                var response = await queueManager.ExecuteWithTimeout<object>(async () =>
                {
                    return await Task<SalesmanReceipt>.Run(() => RetailReportData.SearchSalesmanReceipt(request.Item));
                }, "RetailReport.SearchSalesTaxSummary", request.Item, 3, organizationId, userId);

                if (response.IsCompleted)
                {
                    return this.Ok(response.Result);
                }
                else
                {
                    return this.ResponseMessage(response.TimoutResponse);
                }
            }

            
        }
        [HttpPost]
        [Route("SearchSalesmanDetailSales")]
        [WebAPI.InfyAuthorize(PermissionSet.salesmanreport, InfyPOS.Common.Action.View)]
        public async Task<IHttpActionResult> SearchSalesmanDetailSales(ActionRequest<RetailReportCreteria> request)
        {
            if (!ValidateLocation(request.Item.locationid))
                throw new Exception("Don't have permission to access all location data");

            RetailReportDataService RetailReportData = new RetailReportDataService();
            if (InfyPOS.Services.ConfigManager.Instance(RetailReportData.Context.OrganizationID).Config != null)
                request.Item.multicompany = InfyPOS.Services.ConfigManager.Instance(RetailReportData.Context.OrganizationID).Config.system_multicompany;



            var queueManager = InfyPOS.Processors.WorkflowQueueManager.Instance;
            if (!queueManager.Enabled)
            {

                List<SalesmanReport> result = await Task<List<SalesmanReport>>.Run(() => RetailReportData.SearchSalesmanDetailSales(request.Item));
                return this.Ok(result);
            }
            else
            {
                InboxDataService ActualDataService = new InboxDataService();
                var organizationId = ActualDataService.Context?.OrganizationID ?? 0;
                var userId = ActualDataService.Context?.UserID.ToString() ?? "";
                var response = await queueManager.ExecuteWithTimeout<object>(async () =>
                {
                    return await Task<List<SalesmanReport>>.Run(() => RetailReportData.SearchSalesmanDetailSales(request.Item));
                }, "RetailReport.SearchSalesTaxSummary", request.Item, 3, organizationId, userId);

                if (response.IsCompleted)
                {
                    return this.Ok(response.Result);
                }
                else
                {
                    return this.ResponseMessage(response.TimoutResponse);
                }
            }

        }
        [HttpPost]
        [Route("SearchSalesBarcode")]
        //[WebAPI.InfyAuthorize(PermissionSet.saleshsnreport, InfyPOS.Common.Action.View)]
        public async Task<IHttpActionResult> SearchSalesBarcode(ActionRequest<RetailReportCreteria> request)
        {
            if (!ValidateLocation(request.Item.locationid))
                throw new Exception("Don't have permission to access all location data");

            RetailReportDataService RetailReportData = new RetailReportDataService();
            if (InfyPOS.Services.ConfigManager.Instance(RetailReportData.Context.OrganizationID).Config != null)
                request.Item.multicompany = InfyPOS.Services.ConfigManager.Instance(RetailReportData.Context.OrganizationID).Config.system_multicompany;


            List<SalesBarcodeWise> result = await Task<List<SalesBarcodeWise>>.Run(() => RetailReportData.SearchSalesBarcode(request.Item));
            return this.Ok(result);
        }
        [HttpPost]
        [Route("SearchSalesVoid")]
        [WebAPI.InfyAuthorize(PermissionSet.saleshsnreport, InfyPOS.Common.Action.View)]
        public async Task<IHttpActionResult> SearchSalesVoid(ActionRequest<RetailReportCreteria> request)
        {
            if (!ValidateLocation(request.Item.locationid))
                throw new Exception("Don't have permission to access all location data");

            RetailReportDataService RetailReportData = new RetailReportDataService();
            if (InfyPOS.Services.ConfigManager.Instance(RetailReportData.Context.OrganizationID).Config != null)
                request.Item.multicompany = InfyPOS.Services.ConfigManager.Instance(RetailReportData.Context.OrganizationID).Config.system_multicompany;


            List<SalesBarcodeVoid> result = await Task<List<SalesBarcodeVoid>>.Run(() => RetailReportData.SearchSalesVoid(request.Item));
            return this.Ok(result);
        }
        [HttpPost]
        [Route("SearchSalesHSN")]
        [WebAPI.InfyAuthorize(PermissionSet.saleshsnreport, InfyPOS.Common.Action.View)]
        public async Task<IHttpActionResult> SearchSalesHSN(ActionRequest<RetailReportCreteria> request)
        {
            if (!ValidateLocation(request.Item.locationid))
                throw new Exception("Don't have permission to access all location data");

            RetailReportDataService RetailReportData = new RetailReportDataService();
            if (InfyPOS.Services.ConfigManager.Instance(RetailReportData.Context.OrganizationID).Config != null)
                request.Item.multicompany = InfyPOS.Services.ConfigManager.Instance(RetailReportData.Context.OrganizationID).Config.system_multicompany;

            if (request.Item.showsalesummary)
            {
                List<HSNSummary> result = await Task<List<HSNSummary>>.Run(() => RetailReportData.SearchSalesHSNSummary(request.Item));
                return this.Ok(result);
            }
            else
            {
                POReport result = await Task<POReport>.Run(() => RetailReportData.SearchSalesHSN(request.Item));
                return this.Ok(result);
            }            
        }
        [HttpPost]
        [Route("SearchSalesAnalysis")]
        [WebAPI.InfyAuthorize(PermissionSet.salesanalysisreport, InfyPOS.Common.Action.View)]
        public async Task<IHttpActionResult> SearchSalesAnalysis(ActionRequest<StockReportCreteria> request)
        {
            if (!ValidateLocation(request.Item.locationid))
                throw new Exception("Don't have permission to access all location data");

            RetailReportDataService RetailReportData = new RetailReportDataService();
            if (InfyPOS.Services.ConfigManager.Instance(RetailReportData.Context.OrganizationID).Config != null)
                request.Item.multicompany = InfyPOS.Services.ConfigManager.Instance(RetailReportData.Context.OrganizationID).Config.system_multicompany;
            var queueManager = InfyPOS.Processors.WorkflowQueueManager.Instance;
            if (!queueManager.Enabled)
            {
                List<StockReport> result = await Task<List<StockReport>>.Run(() => RetailReportData.SearchSalesAnalysis(request.Item));
                return this.Ok(result);
            }
            else
            {
                InboxDataService ActualDataService = new InboxDataService();
                var organizationId = ActualDataService.Context?.OrganizationID ?? 0;
                var userId = ActualDataService.Context?.UserID.ToString() ?? "";
                var response = await queueManager.ExecuteWithTimeout<object>(async () =>
                {
                    return await Task<List<StockReport>>.Run(() => RetailReportData.SearchSalesAnalysis(request.Item));
                }, "InboxService.Search", request.Item, 3, organizationId, userId);

                if (response.IsCompleted)
                {
                    return this.Ok(response.Result);
                }
                else
                {
                    return this.ResponseMessage(response.TimoutResponse);
                }
            }
        }

        [HttpPost]
        [Route("BusinessTrend")]
        [WebAPI.InfyAuthorize(PermissionSet.dashboardsales, InfyPOS.Common.Action.View)]
        public async Task<IHttpActionResult> StockVsSales(ActionRequest<RetailReportCreteria> request)
        {
            RetailReportDataService RetailReportData = new RetailReportDataService();
            var queueManager = InfyPOS.Processors.WorkflowQueueManager.Instance;
            if (!queueManager.Enabled)
            {
                List<Dashboard_StockVsSales> result = await Task<List<Dashboard_StockVsSales>>.Run(() => RetailReportData.GetDashboard_StockVsSalesCached(request.Item.currentstock,
                    RetailReportData.DateTimeNow.Date.AddDays(-15)));
                return this.Ok(result);
            }
            else
            {
                InboxDataService ActualDataService = new InboxDataService();
                var organizationId = ActualDataService.Context?.OrganizationID ?? 0;
                var userId = ActualDataService.Context?.UserID.ToString() ?? "";
                var response = await queueManager.ExecuteWithTimeout<object>(async () =>
                {
                    return await Task<List<Dashboard_StockVsSales>>.Run(() => RetailReportData.GetDashboard_StockVsSalesCached(request.Item.currentstock,
                    RetailReportData.DateTimeNow.Date.AddDays(-15)));
                }, "InboxService.Search", request.Item, 3, organizationId, userId);

                if (response.IsCompleted)
                {
                    return this.Ok(response.Result);
                }
                else
                {
                    return this.ResponseMessage(response.TimoutResponse);
                }
            }
        }

        [HttpPost]
        [Route("SearchDashboardData")]
        [WebAPI.InfyAuthorize(PermissionSet.dashboardsales, InfyPOS.Common.Action.View)]
        public async Task<IHttpActionResult> SearchDashboardData(ActionRequest<RetailReportCreteria> request)
        {
            RetailReportDataService RetailReportData = new RetailReportDataService();
            if (InfyPOS.Services.ConfigManager.Instance(RetailReportData.Context.OrganizationID).Config != null)
                request.Item.multicompany = InfyPOS.Services.ConfigManager.Instance(RetailReportData.Context.OrganizationID).Config.system_multicompany;

            var queueManager = InfyPOS.Processors.WorkflowQueueManager.Instance;
            if (!queueManager.Enabled || (RetailReportData.Context.Platform != "WEB" &&
                RetailReportData.Context.Platform != ""))
            {
                DashboardData result = await Task<DashboardData>.Run(() => RetailReportData.SearchDashboardData(request.Item));
                return this.Ok(result);
            }
            else
            {
                InboxDataService ActualDataService = new InboxDataService();
                var organizationId = ActualDataService.Context?.OrganizationID ?? 0;
                var userId = ActualDataService.Context?.UserID.ToString() ?? "";
                var response = await queueManager.ExecuteWithTimeout<object>(async () =>
                {
                    return await Task<DashboardData>.Run(() => RetailReportData.SearchDashboardData(request.Item));
                }, "InboxService.Search", request.Item, 3, organizationId, userId);

                if (response.IsCompleted)
                {
                    return this.Ok(response.Result);
                }
                else
                {
                    return this.ResponseMessage(response.TimoutResponse);
                }
            }

        }

        [HttpPost]
        [Route("SearchB2BDashboard")]
        [WebAPI.InfyAuthorize(PermissionSet.dashboardsales, InfyPOS.Common.Action.View)]
        public async Task<IHttpActionResult> SearchB2BDashboard(ActionRequest<RetailReportCreteria> request)
        {
            
            RetailReportDataService RetailReportData = new RetailReportDataService();
            if (InfyPOS.Services.ConfigManager.Instance(RetailReportData.Context.OrganizationID).Config != null)
                request.Item.multicompany = InfyPOS.Services.ConfigManager.Instance(RetailReportData.Context.OrganizationID).Config.system_multicompany;

            var queueManager = InfyPOS.Processors.WorkflowQueueManager.Instance;
            if (!queueManager.Enabled)
            {
                B2BDashboard result = await Task<B2BDashboard>.Run(() => RetailReportData.SearchB2BDashboard(request.Item));
                return this.Ok(result);
            }
            else
            {
                InboxDataService ActualDataService = new InboxDataService();
                var organizationId = ActualDataService.Context?.OrganizationID ?? 0;
                var userId = ActualDataService.Context?.UserID.ToString() ?? "";
                var response = await queueManager.ExecuteWithTimeout<object>(async () =>
                {
                    return await Task<B2BDashboard>.Run(() => RetailReportData.SearchB2BDashboard(request.Item));
                }, "InboxService.Search", request.Item, 3, organizationId, userId);

                if (response.IsCompleted)
                {
                    return this.Ok(response.Result);
                }
                else
                {
                    return this.ResponseMessage(response.TimoutResponse);
                }
            }

        }
        

        [HttpPost]
        [Route("SearchAdditionalDashboard")]
        [WebAPI.InfyAuthorize(PermissionSet.dashboardsales, InfyPOS.Common.Action.View)]
        public async Task<IHttpActionResult> SearchAdditionalDashboard(ActionRequest<RetailReportCreteria> request)
        {

            RetailReportDataService RetailReportData = new RetailReportDataService();
            if (InfyPOS.Services.ConfigManager.Instance(RetailReportData.Context.OrganizationID).Config != null)
                request.Item.multicompany = InfyPOS.Services.ConfigManager.Instance(RetailReportData.Context.OrganizationID).Config.system_multicompany;

            var queueManager = InfyPOS.Processors.WorkflowQueueManager.Instance;
            if (!queueManager.Enabled)
            {
                AdditionalDashboardData result = await Task<AdditionalDashboardData>.Run(() => RetailReportData.SearchAdditionalDashboard(request.Item));
                return this.Ok(result);
            }
            else
            {
                InboxDataService ActualDataService = new InboxDataService();
                var organizationId = ActualDataService.Context?.OrganizationID ?? 0;
                var userId = ActualDataService.Context?.UserID.ToString() ?? "";
                var response = await queueManager.ExecuteWithTimeout<object>(async () =>
                {
                    return await Task<AdditionalDashboardData>.Run(() => RetailReportData.SearchAdditionalDashboard(request.Item));
                }, "InboxService.Search", request.Item, 3, organizationId, userId);

                if (response.IsCompleted)
                {
                    return this.Ok(response.Result);
                }
                else
                {
                    return this.ResponseMessage(response.TimoutResponse);
                }
            }
        }


        [HttpPost]
        [Route("SearchAdditionalDiscount")]
        public async Task<IHttpActionResult> SearchAdditionalDiscount(ActionRequest<RetailReportCreteria> request)
        {

            RetailReportDataService RetailReportData = new RetailReportDataService();
            if (InfyPOS.Services.ConfigManager.Instance(RetailReportData.Context.OrganizationID).Config != null)
                request.Item.multicompany = InfyPOS.Services.ConfigManager.Instance(RetailReportData.Context.OrganizationID).Config.system_multicompany;

            AdditionalDashboardData result = await Task<AdditionalDashboardData>.Run(() => RetailReportData.SearchAdditionalDiscount(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("SearchIncentive")]
        public async Task<IHttpActionResult> SearchIncentive(ActionRequest<RetailReportCreteria> request)
        {
            if (!ValidateLocation(request.Item.locationid))
                throw new Exception("Don't have permission to access all location data");

            IncentiveDataSource RetailReportData = new IncentiveDataSource();
            if (InfyPOS.Services.ConfigManager.Instance(RetailReportData.Context.OrganizationID).Config != null)
                request.Item.multicompany = InfyPOS.Services.ConfigManager.Instance(RetailReportData.Context.OrganizationID).Config.system_multicompany;

            List<IncentiveValue> result = await Task<List<IncentiveValue>>.Run(() => RetailReportData.SearchIncentive(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("SearchBulkBill")]
        [WebAPI.InfyAuthorize(PermissionSet.bulksales, InfyPOS.Common.Action.View)]
        public async Task<IHttpActionResult> SearchBulkBill(ActionRequest<SaleRetailCreteria> request)
        {
            if (!ValidateLocation(request.Item.locationid))
                throw new Exception("Don't have permission to access all location data");

            RetailReportDataService RetailReportData = new RetailReportDataService();
            if (InfyPOS.Services.ConfigManager.Instance(RetailReportData.Context.OrganizationID).Config != null)
                request.Item.multicompany = InfyPOS.Services.ConfigManager.Instance(RetailReportData.Context.OrganizationID).Config.system_multicompany;


            List<RetailReport> result = await Task<List<RetailReport>>.Run(() =>
                RetailReportData.SearchBulkBill(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("SearchEstimateBill")]
        public async Task<IHttpActionResult> SearchEstimateBill(ActionRequest<SaleRetailCreteria> request)
        {
            if (!ValidateLocation(request.Item.locationid))
                throw new Exception("Don't have permission to access all location data");

            RetailReportDataService RetailReportData = new RetailReportDataService();
            if (InfyPOS.Services.ConfigManager.Instance(RetailReportData.Context.OrganizationID).Config != null)
                request.Item.multicompany = InfyPOS.Services.ConfigManager.Instance(RetailReportData.Context.OrganizationID).Config.system_multicompany;


            List<RetailReport> result = await Task<List<RetailReport>>.Run(() =>
                RetailReportData.SearchEstimateBill(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("BulkBillPDF")]
        [WebAPI.InfyAuthorize(PermissionSet.bulksales, InfyPOS.Common.Action.View)]
        public async Task<IHttpActionResult> BulkBillPDF(ActionRequest<SaleRetailCreteria> request)
        {
            if (!ValidateLocation(request.Item.locationid))
                throw new Exception("Don't have permission to access all location data");

            RetailReportDataService RetailReportData = new RetailReportDataService();
            if (InfyPOS.Services.ConfigManager.Instance(RetailReportData.Context.OrganizationID).Config != null)
                request.Item.multicompany = InfyPOS.Services.ConfigManager.Instance(RetailReportData.Context.OrganizationID).Config.system_multicompany;

            request.Item.showdetail = true;
            byte[] pdfbytes = await Task<byte[]>.Run(() => RetailReportData.BulkBillImagePDFGeneration(request.Item));
            string key = WebAPI.Data.Token.RandomString(16);
            System.Web.HttpContext.Current.Cache.Add("PDF-" + key, pdfbytes, null,
                DateTime.UtcNow.AddMinutes(2), TimeSpan.Zero, System.Web.Caching.CacheItemPriority.Low, null);
            return this.Ok(key);
        }
        [AllowAnonymous]
        [HttpGet]
        [Route("ImagePDFGeneration/{key}")]
        public async Task<IHttpActionResult> ImagePDFGeneration(string key)
        {
            try
            {
                var httpcontext = System.Web.HttpContext.Current;
                if (httpcontext.Cache.Get("PDF-" + key) == null)
                {
                    return this.InternalServerError();
                }
                FileContentInfo byteArray = new FileContentInfo(new ByteArrayContent((byte[])httpcontext.Cache.Get("PDF-" + key)));
                byteArray.FileName = "images.pdf";
                byteArray.Extension = "application/pdf";
                httpcontext.Cache.Remove("PDF-" + key);
                return new FileActionResult(false, "", byteArray.Extension, byteArray.Content);
            }
            catch (Exception exp)
            {
                return this.Ok(exp);
            }
        }

        [HttpPost]
        [Route("SearchDeliveryStatus")]
        [WebAPI.InfyAuthorize(PermissionSet.deliveryreport, InfyPOS.Common.Action.View)]
        public async Task<IHttpActionResult> SearchDeliveryStatus(ActionRequest<SaleRetailCreteria> request)
        {

            RetailReportDataService RetailReportData = new RetailReportDataService();
            if (InfyPOS.Services.ConfigManager.Instance(RetailReportData.Context.OrganizationID).Config != null)
                request.Item.multicompany = InfyPOS.Services.ConfigManager.Instance(RetailReportData.Context.OrganizationID).Config.system_multicompany;


            List<DeliveryReport> result = await Task<List<DeliveryReport>>.Run(() => RetailReportData.SearchDeliveryStatus(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("SearchDelivery")]
        [WebAPI.InfyAuthorize(PermissionSet.deliveryreport, InfyPOS.Common.Action.View)]
        public async Task<IHttpActionResult> SearchDelivery(ActionRequest<SaleRetailCreteria> request)
        {
            RetailReportDataService RetailReportData = new RetailReportDataService();
            if (InfyPOS.Services.ConfigManager.Instance(RetailReportData.Context.OrganizationID).Config != null)
                request.Item.multicompany = InfyPOS.Services.ConfigManager.Instance(RetailReportData.Context.OrganizationID).Config.system_multicompany;

            List<DeliveryReport> result = await Task<List<DeliveryReport>>.Run(() => RetailReportData.SearchDelivery(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("SearchAlteration")]
        public async Task<IHttpActionResult> SearchAlteration(ActionRequest<SaleRetailCreteria> request)
        {
            RetailReportDataService RetailReportData = new RetailReportDataService();
            if (InfyPOS.Services.ConfigManager.Instance(RetailReportData.Context.OrganizationID).Config != null)
                request.Item.multicompany = InfyPOS.Services.ConfigManager.Instance(RetailReportData.Context.OrganizationID).Config.system_multicompany;

            List<DeliveryReport> result = await Task<List<DeliveryReport>>.Run(() => RetailReportData.SearchAlteration(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("SearchFastmovingItem")]
        [WebAPI.InfyAuthorize(PermissionSet.dashboardsales, InfyPOS.Common.Action.View)]
        public async Task<IHttpActionResult> SearchFastmovingItem(ActionRequest<RetailReportCreteria> request)
        {

            RetailReportDataService RetailReportData = new RetailReportDataService();
            if (InfyPOS.Services.ConfigManager.Instance(RetailReportData.Context.OrganizationID).Config != null)
                request.Item.multicompany = InfyPOS.Services.ConfigManager.Instance(RetailReportData.Context.OrganizationID).Config.system_multicompany;


            DashboardData result = await Task<DashboardData>.Run(() => RetailReportData.SearchFastmovingItem(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("SearchStockValue")]
        [WebAPI.InfyAuthorize(PermissionSet.dashboardsales, InfyPOS.Common.Action.View)]
        public async Task<IHttpActionResult> SearchStockValue(ActionRequest<RetailReportCreteria> request)
        {

            RetailReportDataService RetailReportData = new RetailReportDataService();
            if (InfyPOS.Services.ConfigManager.Instance(RetailReportData.Context.OrganizationID).Config != null)
                request.Item.multicompany = InfyPOS.Services.ConfigManager.Instance(RetailReportData.Context.OrganizationID).Config.system_multicompany;


            List<StockValue> result = await Task<List<StockValue>>.Run(() => RetailReportData.SearchStockValue(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("StockVsSales")]
        [WebAPI.InfyAuthorize(PermissionSet.dashboardsales, InfyPOS.Common.Action.View)]
        public async Task<IHttpActionResult> StockVsSales()
        {
            RetailReportDataService RetailReportData = new RetailReportDataService();
            List<Dashboard_StockVsSales> result = await Task<List<Dashboard_StockVsSales>>.Run(() => RetailReportData.GetDashboard_StockVsSalesCached(0,
                RetailReportData.DateTimeNow.Date.AddDays(-15)));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("SearchMissingStock")]
        public async Task<IHttpActionResult> SearchMissingStock(ActionRequest<StockReportCreteria> request)
        {

            RetailReportDataService RetailReportData = new RetailReportDataService();
            if (InfyPOS.Services.ConfigManager.Instance(RetailReportData.Context.OrganizationID).Config != null)
                request.Item.multicompany = InfyPOS.Services.ConfigManager.Instance(RetailReportData.Context.OrganizationID).Config.system_multicompany;


            List<StockReport> result = await Task<List<StockReport>>.Run(() => RetailReportData.SearchMissingStock(request.Item));
            return this.Ok(result);
        }
        [HttpPost]
        [Route("SearchSettlementDetail")]
        [WebAPI.InfyAuthorize(PermissionSet.settlementreport, InfyPOS.Common.Action.View)]
        public async Task<IHttpActionResult> SearchSettlementDetail(ActionRequest<RetailReportCreteria> request)
        {
            if (!ValidateLocation(request.Item.locationid))
                throw new Exception("Don't have permission to access all location data");

            RetailReportDataService RetailReportData = new RetailReportDataService();
            if (InfyPOS.Services.ConfigManager.Instance(RetailReportData.Context.OrganizationID).Config != null)
                request.Item.multicompany = InfyPOS.Services.ConfigManager.Instance(RetailReportData.Context.OrganizationID).Config.system_multicompany;


            List<SettlementReport> result = await Task<List<SettlementReport>>.Run(() => RetailReportData.SearchSettlementDetail(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("SettlementSummaryByDay")]
        [WebAPI.InfyAuthorize(PermissionSet.settlementreport, InfyPOS.Common.Action.View)]
        public async Task<IHttpActionResult> SettlementSummaryByDay(ActionRequest<RetailReportCreteria> request)
        {
           
            RetailReportDataService RetailReportData = new RetailReportDataService();
            if (InfyPOS.Services.ConfigManager.Instance(RetailReportData.Context.OrganizationID).Config != null)
                request.Item.multicompany = InfyPOS.Services.ConfigManager.Instance(RetailReportData.Context.OrganizationID).Config.system_multicompany;


            List<RetailReportDataService.DaySettlement> result = await Task<List<RetailReportDataService.DaySettlement>>.Run(() => RetailReportData.SettlementSummaryByDay(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("SearchBillSummary")]
        public async Task<IHttpActionResult> SearchBillSummary(ActionRequest<SaleRetailCreteria> request)
        {

            RetailReportDataService RetailReportData = new RetailReportDataService();
            if (InfyPOS.Services.ConfigManager.Instance(RetailReportData.Context.OrganizationID).Config != null)
                request.Item.multicompany = InfyPOS.Services.ConfigManager.Instance(RetailReportData.Context.OrganizationID).Config.system_multicompany;

            BillSummary result = await Task<BillSummary>.Run(() => RetailReportData.SearchBillSummary(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("SettlementSummaryByCompany")]
        [WebAPI.InfyAuthorize(PermissionSet.settlementreport, InfyPOS.Common.Action.View)]
        public async Task<IHttpActionResult> SettlementSummaryByCompany(ActionRequest<RetailReportCreteria> request)
        {
            if (!ValidateLocation(request.Item.locationid))
                throw new Exception("Don't have permission to access all location data");

            RetailReportDataService RetailReportData = new RetailReportDataService();
            if (InfyPOS.Services.ConfigManager.Instance(RetailReportData.Context.OrganizationID).Config != null)
                request.Item.multicompany = InfyPOS.Services.ConfigManager.Instance(RetailReportData.Context.OrganizationID).Config.system_multicompany;


            List<RetailReportDataService.CompanySettlement> result = await Task<List<RetailReportDataService.CompanySettlement>>.Run(() => RetailReportData.SettlementSummaryByCompany(request.Item));
            return this.Ok(result);
        }
        [HttpPost]
        [Route("SearchSettlementReconcilation")]
        [WebAPI.InfyAuthorize(PermissionSet.settlementreport, InfyPOS.Common.Action.View)]
        public async Task<IHttpActionResult> SearchSettlementReconcilation(ActionRequest<RetailReportCreteria> request)
        {
            if (!ValidateLocation(request.Item.locationid))
                throw new Exception("Don't have permission to access all location data");

            RetailReportDataService RetailReportData = new RetailReportDataService();
            if (InfyPOS.Services.ConfigManager.Instance(RetailReportData.Context.OrganizationID).Config != null)
                request.Item.multicompany = InfyPOS.Services.ConfigManager.Instance(RetailReportData.Context.OrganizationID).Config.system_multicompany;


            List<RetailReportDataService.ReconilationSettlement> result = await Task<List<RetailReportDataService.ReconilationSettlement>>.Run(() => RetailReportData.SearchSettlementReconcilation(request.Item));
            return this.Ok(result);
        }

        

        [HttpPost]
        [Route("StockMarker")]
        public async Task<IHttpActionResult> StockMarker(ActionRequest<StockReportCreteria> request)
        {

            RetailReportDataService RetailReportData = new RetailReportDataService();
            if (InfyPOS.Services.ConfigManager.Instance(RetailReportData.Context.OrganizationID).Config != null)
                request.Item.multicompany = InfyPOS.Services.ConfigManager.Instance(RetailReportData.Context.OrganizationID).Config.system_multicompany;


            List<Stockmarkerdetail> result = await Task<List<Stockmarkerdetail>>.Run(() => RetailReportData.StockMarker(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("SearchPriceChange")]
        public async Task<IHttpActionResult> SearchPriceChange(ActionRequest<RetailReportCreteria> request)
        {

            RetailReportDataService RetailReportData = new RetailReportDataService();
            if (InfyPOS.Services.ConfigManager.Instance(RetailReportData.Context.OrganizationID).Config != null)
                request.Item.multicompany = InfyPOS.Services.ConfigManager.Instance(RetailReportData.Context.OrganizationID).Config.system_multicompany;


            List<Pricechangehistory> result = await Task<List<Pricechangehistory>>.Run(() => RetailReportData.PriceChangerHistory(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("SearchStockAudit")]
        public async Task<IHttpActionResult> SearchStockAudit(ActionRequest<StockReportCreteria> request)
        {
            RetailReportDataService RetailReportData = new RetailReportDataService();
            if (InfyPOS.Services.ConfigManager.Instance(RetailReportData.Context.OrganizationID).Config != null)
                request.Item.multicompany = InfyPOS.Services.ConfigManager.Instance(RetailReportData.Context.OrganizationID).Config.system_multicompany;

            List<Salablegoodshistory> result = await Task<List<Salablegoodshistory>>.Run(() => RetailReportData.SearchsalablegoodsAudit(request.Item));
            return this.Ok(result);
        }


        [HttpPost]
        [Route("SearchSplitDetails")]
        public async Task<IHttpActionResult> SearchSplitDetails(ActionRequest<SplitbarcodehistoryCreteria> request)
        {
            RetailReportDataService RetailReportData = new RetailReportDataService();
            List<Splitbarcodehistory> result = await Task<List<Splitbarcodehistory>>.Run(() => RetailReportData.SearchSplitDetails(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("SearchOpeningClosing")]
        public async Task<IHttpActionResult> SearchOpeningClosing(ActionRequest<OpeningClosingSummary> request)
        {
            RetailReportDataService RetailReportData = new RetailReportDataService();
            List<OpeningClosingSummary> result = await Task<List<OpeningClosingSummary>>.Run(() => RetailReportData.SearchOpeningClosing(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("SearchSaleSummarySlip")]
        [WebAPI.InfyAuthorize(PermissionSet.salesreport, InfyPOS.Common.Action.View)]
        public async Task<IHttpActionResult> SearchSaleSummarySlip(ActionRequest<RetailReportCreteria> request)
        {
            if (!ValidateLocation(request.Item.locationid))
                throw new Exception("Don't have permission to access all location data");

            RetailReportDataService RetailReportData = new RetailReportDataService();
            if (InfyPOS.Services.ConfigManager.Instance(RetailReportData.Context.OrganizationID).Config != null)
                request.Item.multicompany = InfyPOS.Services.ConfigManager.Instance(RetailReportData.Context.OrganizationID).Config.system_multicompany;


            List<SaleSummary> result = await Task<List<SaleSummary>>.Run(() => RetailReportData.SearchSaleSummarySlip(request.Item));
            return this.Ok(result);
        }
    }
}