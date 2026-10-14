
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
    [RoutePrefix("ExcelExportService")]
    public class ExcelExportServiceController : ApiController
    {
        [AllowAnonymous]
        [HttpGet]
        [Route("DownLoadFormP/{organizationid}/{companyid}/{month}/{year}")]
        public async Task<IHttpActionResult> DownLoadFormP(string organizationid,string companyid, string month, string year)
        {
            try
            {

                FileContentInfo byteArray = await Task<FileContentInfo>.Run(() => CreateForm(organizationid,companyid, "FORMP", month, year));
                return new FileActionResult(false, "", byteArray.Extension, byteArray.Content);
            }
            catch (Exception exp)
            {
                return this.Ok(exp);
            }
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("DownLoadFormQ/{organizationid}/{companyid}/{month}/{year}")]
        public async Task<IHttpActionResult> DownLoadFormQ(string organizationid, string companyid, string month, string year)
        {
            try
            {

                FileContentInfo byteArray = await Task<FileContentInfo>.Run(() => CreateForm(organizationid, companyid, "FORMQ", month, year));
                return new FileActionResult(false, "", byteArray.Extension, byteArray.Content);
            }
            catch (Exception exp)
            {
                return this.Ok(exp);
            }
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("DownLoadFormR/{organizationid}/{companyid}/{month}/{year}")]
        public async Task<IHttpActionResult> DownLoadFormR(string organizationid, string companyid, string month, string year)
        {
            try
            {

                FileContentInfo byteArray = await Task<FileContentInfo>.Run(() => CreateForm(organizationid, companyid, "FORMR", month, year));
                return new FileActionResult(false, "", byteArray.Extension, byteArray.Content);
            }
            catch (Exception exp)
            {
                return this.Ok(exp);
            }
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("DownLoadFormS/{organizationid}/{companyid}/{month}/{year}")]
        public async Task<IHttpActionResult> DownLoadFormS(string organizationid, string companyid, string month,string year)
        {
            try
            {

                FileContentInfo byteArray = await Task<FileContentInfo>.Run(() => CreateForm(organizationid, companyid,"FORMS",month,year));
                return new FileActionResult(false, "", byteArray.Extension, byteArray.Content);
            }
            catch (Exception exp)
            {
                return this.Ok(exp);
            }
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("DownLoadESI/{organizationid}/{companyid}/{month}/{year}")]
        public async Task<IHttpActionResult> DownLoadFORMESI(string organizationid,string companyid, string month, string year)
        {
            try
            {

                FileContentInfo byteArray = await Task<FileContentInfo>.Run(() => CreateForm(organizationid, companyid, "FORMESI", month, year));
                return new FileActionResult(false, "", byteArray.Extension, byteArray.Content);
            }
            catch (Exception exp)
            {
                return this.Ok(exp);
            }
        }

        private FileContentInfo CreateForm(string organizationid,string companyid, string form,string month,string year)
        {
            try
            {
                ExcelExportDataService exceldata = new ExcelExportDataService();
                exceldata.Context = new UserClaim() { OrganizationID = long.Parse(organizationid) };
                var fci = exceldata.FORM(new ExcelExportDataService.ExcelExportCreteria { companyid = long.Parse(companyid),
                    form = form,
                    month = int.Parse(month),year = int.Parse(year)});
                return fci;
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(new BaseData().Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, null);
            }
            return null;
        }

    }

}