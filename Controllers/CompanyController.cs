
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
    [RoutePrefix("CompanyService")]
    public class CompanyServiceController : ApiController
    {
        [HttpGet]
        [AllowAnonymous]
        [Route("Status")]
        public async Task<IHttpActionResult> Status()
        {
            return this.Ok("Working...");
        }
        [HttpGet]
        [Route("License/{companycode=null}")]
        [AllowAnonymous]
        public async Task<IHttpActionResult> License(string companycode)
        {
            try
            {
                if (companycode == "null") companycode = null;
                LicenseManager.License result = await Task<LicenseManager.License>.Run(() => PrivateLicense(companycode));
                return this.Ok(result);
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(new BaseData().Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, "CompanyService/License");
                return this.InternalServerError(exp);
            }
        }

        public LicenseManager.License PrivateLicense(string companycode)
        {
            if (LicenseManager.IsSAAS)
            {
                LicenseManager.License saaslicense = null;
                if (LicenseManager.Instance(companycode) != null)
                {
                    saaslicense = LicenseManager.Instance(companycode).GetLicense();
                }

                saaslicense = LicenseManager.License.Create("Quanto Retails");
                saaslicense.IsSAAS = true;
                saaslicense.saas = true;
                return saaslicense;
            }
            else
            {
                return LicenseManager.DirectLicense();
            }
        }

        [HttpGet]
        [Route("CheckLicense")]
        [AllowAnonymous]
        public async Task<IHttpActionResult> CheckLicense()
        {
            try
            {
                LicenseManager.License result = await Task<LicenseManager.License>.Run(() => LicenseManager.DirectLicense());
                return this.Ok(result);
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(new BaseData().Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, "CompanyService/License");
                return this.InternalServerError(exp);
            }
        }

        [HttpGet]
        [Route("ListPrinter")]
        public async Task<IHttpActionResult> ListPrinter()
        {
            CompanyDataService CompanyData = new CompanyDataService();
            List<PrintServer> result = await Task<List<PrintServer>>.Run(() => CompanyData.SearchPrinters());
            return this.Ok(result);
        }


        //[HttpGet]
        //[AllowAnonymous]
        //[Route("GetMyIP")]
        //public async Task<IHttpActionResult> GetMyIP()
        //{


        //    try
        //    {
        //        var request = System.Web.HttpContext.Current.Request;
        //        string ipaddress = System.Web.HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
        //        if (string.IsNullOrEmpty(ipaddress) || ipaddress.Length < 5)
        //            ipaddress = System.Web.HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
        //        if (string.IsNullOrEmpty(ipaddress) || ipaddress.Length < 5)
        //            ipaddress = request.UserHostAddress;
        //        if (string.IsNullOrEmpty(ipaddress) || ipaddress.Length < 5)
        //            ipaddress = System.Web.HttpContext.Current.Request.GetOwinContext().Request.LocalIpAddress;
        //        if (string.IsNullOrEmpty(ipaddress) || ipaddress.Length < 5)
        //            ipaddress = (string)System.Web.HttpContext.Current.Request.GetOwinContext().Request.Environment["server.RemoteIpAddress"];
        //        return this.Ok(ipaddress);
        //    }
        //    catch (Exception exp)
        //    {
        //        return this.Ok(exp);
        //    }
        //}

        [HttpPost]
        [Route("CreateClientLicense")]
        [AllowAnonymous]
        public async Task<IHttpActionResult> CreateClientLicense(PrinterConfig request)
        {
            CompanyDataService CompanyData = new CompanyDataService();
            LicenseInfo result = await Task<LicenseInfo>.Run(() => CompanyData.CreateClientLicense(request));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("SendPasscode")]
        [AllowAnonymous]
        public async Task<IHttpActionResult> SendPasscode(PrinterConfig request)
        {
            UsersDataService usersDataService = new UsersDataService();
            usersDataService.Context = new UserClaim() { OrganizationID = request.Organizationid };
            bool result = await Task<bool>.Run(() => usersDataService.SendCurrentPasscode(request.employeecode));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("AddPrinter")]
        [AllowAnonymous]
        public async Task<IHttpActionResult> AddPrinter(PrinterConfig request)
        {
            CompanyDataService CompanyData = new CompanyDataService();
            CompanyData.Context = new UserClaim() { OrganizationID = request.Organizationid };
            bool result = await Task<bool>.Run(() => CompanyData.AddPrinter(request));

            if (!result)
                return this.Ok(result);
            else
                return this.NotFound();
        }

        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<CompanyCreteria> request)
        {
            CompanyDataService CompanyData = new CompanyDataService();
            List<Company> result = await Task<List<Company>>.Run(() => CompanyData.Search(request.Item));
            return this.Ok(result);
        }


        [HttpPost]
        [Route("Save")]
        public async Task<IHttpActionResult> Save(ActionRequest<Company> request)
        {
            CompanyDataService CompanyData = new CompanyDataService();
            if (request.Item.id <= 0)
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Create);
                Company result = await Task<Company>.Run(() => CompanyData.Insert(request.Item));
                return this.Ok(result);
            }
            else
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Update);
                Company result = await Task<Company>.Run(() => CompanyData.Update(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("Insert")]
        public async Task<IHttpActionResult> Insert(ActionRequest<Company> request)
        {
            CompanyDataService CompanyData = new CompanyDataService();
            Company result = await Task<Company>.Run(() => CompanyData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        public async Task<IHttpActionResult> Update(ActionRequest<Company> request)
        {
            CompanyDataService CompanyData = new CompanyDataService();
            Company result = await Task<Company>.Run(() => CompanyData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IHttpActionResult> Delete(ActionRequest<Company> request)
        {
            CompanyDataService CompanyData = new CompanyDataService();
            bool result = await Task<bool>.Run(() => CompanyData.Delete(request.Item));
            return this.Ok(result);
        }
    }
}