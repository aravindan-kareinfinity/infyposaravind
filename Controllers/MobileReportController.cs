
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
    [RoutePrefix("MobileReportService")]
    public class MobileReportController : ApiController
    {

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<Refillbox>();
            result.Item = new Refillbox();
            return this.Ok(result);
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

        [HttpPost]
        [Route("SearchSettlementSMachine")]
        [WebAPI.InfyAuthorize(PermissionSet.settlementreport, InfyPOS.Common.Action.View)]
        public async Task<IHttpActionResult> SearchSettlementSMachine(ActionRequest<RetailReportCreteria> request)
        {
            //if (!ValidateLocation(request.Item.locationid))
            //    return this.NotFound();

            MobileReportDataService MobileReportData = new MobileReportDataService();
            if (InfyPOS.Services.ConfigManager.Instance(MobileReportData.Context.OrganizationID).Config != null)
                request.Item.multicompany = InfyPOS.Services.ConfigManager.Instance(MobileReportData.Context.OrganizationID).Config.system_multicompany;

            

            List<SettlementReport> result = await Task<List<SettlementReport>>.Run(() => MobileReportData.SearchSettlementSMachine(request.Item));
            return this.Ok(result);
        }
        [HttpPost]
        [Route("SearchEMIStatement")]
        [WebAPI.InfyAuthorize(PermissionSet.settlementreport, InfyPOS.Common.Action.View)]
        public async Task<IHttpActionResult> SearchEMIStatement(ActionRequest<RetailReportCreteria> request)
        {
            //if (!ValidateLocation(request.Item.locationid))
            //    return this.NotFound();

            MobileReportDataService MobileReportData = new MobileReportDataService();
            if (InfyPOS.Services.ConfigManager.Instance(MobileReportData.Context.OrganizationID).Config != null)
                request.Item.multicompany = InfyPOS.Services.ConfigManager.Instance(MobileReportData.Context.OrganizationID).Config.system_multicompany;



            List<EMIStatementReport> result = await Task<List<EMIStatementReport>>.Run(() => MobileReportData.SearchEMIStatement(request.Item));
            return this.Ok(result);
        }
        [HttpPost]
        [Route("SearchECStatement")]
        [WebAPI.InfyAuthorize(PermissionSet.settlementreport, InfyPOS.Common.Action.View)]
        public async Task<IHttpActionResult> SearchECStatement(ActionRequest<RetailReportCreteria> request)
        {
            //if (!ValidateLocation(request.Item.locationid))
            //    return this.NotFound();

            MobileReportDataService MobileReportData = new MobileReportDataService();
            if (InfyPOS.Services.ConfigManager.Instance(MobileReportData.Context.OrganizationID).Config != null)
                request.Item.multicompany = InfyPOS.Services.ConfigManager.Instance(MobileReportData.Context.OrganizationID).Config.system_multicompany;



            List<ECReport> result = await Task<List<ECReport>>.Run(() => MobileReportData.SearchECStatement(request.Item));
            return this.Ok(result);
        }
        [HttpPost]
        [Route("SearchBillItemDetails")]
        [WebAPI.InfyAuthorize(PermissionSet.settlementreport, InfyPOS.Common.Action.View)]
        public async Task<IHttpActionResult> SearchBillItemDetails(ActionRequest<RetailReportCreteria> request)
        {
            //if (!ValidateLocation(request.Item.locationid))
            //    return this.NotFound();
            MobileReportDataService MobileReportData = new MobileReportDataService();
            if (InfyPOS.Services.ConfigManager.Instance(MobileReportData.Context.OrganizationID).Config != null)
                request.Item.multicompany = InfyPOS.Services.ConfigManager.Instance(MobileReportData.Context.OrganizationID).Config.system_multicompany;



            List<BillItemDetailsReport> result = await Task<List<BillItemDetailsReport>>.Run(() => MobileReportData.SearchBillItemDetails(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("SearchSchemeDetails")]
        [WebAPI.InfyAuthorize(PermissionSet.settlementreport, InfyPOS.Common.Action.View)]
        public async Task<IHttpActionResult> SearchSchemeDetails(ActionRequest<RetailReportCreteria> request)
        {
            //if (!ValidateLocation(request.Item.locationid))
            //    return this.NotFound();

            //if (InfyPOS.Services.ConfigManager.Instance != null && InfyPOS.Services.ConfigManager.Instance(new BaseData().Context.OrganizationID).Config != null)
            //    request.Item.multicompany = InfyPOS.Services.ConfigManager.Instance(new BaseData().Context.OrganizationID).Config.system_multicompany;

            MobileReportDataService MobileReportData = new MobileReportDataService();

            List<SchemeDetailsReport> result = await Task<List<SchemeDetailsReport>>.Run(() => MobileReportData.SearchSchemeDetails(request.Item));
            return this.Ok(result);
        }

    }
}