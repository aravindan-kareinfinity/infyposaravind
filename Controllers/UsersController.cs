
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
using System.Data;
using System.Diagnostics;
using System.Web;

namespace InfyPOS.Controllers
{
    [RoutePrefix("UsersService")]
    public class UsersServiceController : ApiController
    {
        [HttpGet]
        [Route("Passcode/{ecode}/{passcode}/{permission}/{locationid}")]
        public async Task<IHttpActionResult> Passcode(string ecode, string passcode, string permission, string locationid)
        {
            UsersDataService UsersData = new UsersDataService();
            UsersDataService.PasscodeInfo result = await Task<UsersDataService.PasscodeInfo>.Run(() => UsersData.Passcode(ecode, passcode, permission, long.Parse(locationid)));
            return this.Ok(result);
        }

        [HttpGet]
        [Route("GetMyPermission")]
        public async Task<IHttpActionResult> GetMyPermission()
        {
            UsersDataService UsersData = new UsersDataService();
            Users result = await Task<Users>.Run(() => UsersData.GetMyPermission());
            return this.Ok(result);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("Connect/{organizationid}/{token}")]
        public async Task<IHttpActionResult> Connect(string organizationid,string token)
        {

            UsersDataService UsersData = new UsersDataService();
            UsersData.Context = new UserClaim() { OrganizationID = long.Parse(organizationid) };
            string serviceurl = string.IsNullOrEmpty(InfyPOS.Services.ConfigManager.Instance(UsersData.Context.OrganizationID).Config.hostedserver) ?
            this.Request.RequestUri.AbsoluteUri.Substring(0, this.Request.RequestUri.AbsoluteUri.IndexOf("/UsersService/")) : InfyPOS.Services.ConfigManager.Instance(UsersData.Context.OrganizationID).Config.hostedserver;
            UsersDataService.ConnectionInfo result = await Task<UsersDataService.ConnectionInfo>.Run(() => UsersData.ConnectAsync(token, serviceurl));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("RequestPASSCODE")]
        public async Task<IHttpActionResult> RequestPASSCODE(ActionRequest<UserPASSCODE> request)
        {
            UsersDataService UsersData = new UsersDataService();
            UsersDataService.PasscodeInfo result = await Task<UsersDataService.PasscodeInfo>.Run(() => UsersData.Passcode(request.Item));
            return this.Ok(result);

        }

        [HttpGet]
        [Route("MyPasscode")]
        public async Task<IHttpActionResult> MyPasscode()
        {
            UsersDataService UsersData = new UsersDataService();
            string result = await Task<UsersDataService.PasscodeInfo>.Run(() => UsersData.MyPasscode());
            return this.Ok(result);
        }

        [HttpGet]
        [Route("MyCustomer/{userid}")]
        public async Task<IHttpActionResult> GetMyCustomer(string userid)
        {
            UsersDataService UsersData = new UsersDataService();
            UserCustomer result = await Task<UserCustomer>.Run(() => UsersData.GetMyCustomer(userid));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("MyCustomer")]
        public async Task<IHttpActionResult> SetMyCustomer(ActionRequest<UserCustomer> request)
        {
            UsersDataService UsersData = new UsersDataService();
            bool result = await Task<bool>.Run(() => UsersData.SetMyCustomer(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("SetGlobalPermission")]
        public async Task<IHttpActionResult> SetGlobalPermission(ActionRequest<UsersCreteria> request)
        {
            UsersDataService UsersData = new UsersDataService();
            bool result = await Task<bool>.Run(() => UsersData.SetGlobalPermission(request.Item));
            return this.Ok(result);
        }

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<Users>();
            result.Item = new Users();
            return this.Ok(result);
        }


       
        private string getStaticIP()
        {
            try
            {
                var request = System.Web.HttpContext.Current.Request;
                string ipaddress = System.Web.HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                if (string.IsNullOrEmpty(ipaddress) || ipaddress.Length<5)
                    ipaddress = System.Web.HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
                if (string.IsNullOrEmpty(ipaddress) || ipaddress.Length < 5)
                    ipaddress = request.UserHostAddress;
                if (string.IsNullOrEmpty(ipaddress) || ipaddress.Length < 5)
                    ipaddress = System.Web.HttpContext.Current.Request.GetOwinContext().Request.LocalIpAddress;
                if (string.IsNullOrEmpty(ipaddress) || ipaddress.Length < 5)
                    ipaddress = (string)System.Web.HttpContext.Current.Request.GetOwinContext().Request.Environment["server.RemoteIpAddress"];
                return ipaddress;
            }catch(Exception exp)
            {
                return "";
            }
        }

        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<UsersCreteria> request)
        {
            UsersDataService UsersData = new UsersDataService();
            try
            {
                if ((request.Item.ValidateSecurity ||
                    request.Item.ValidateLogin) &&
                    string.IsNullOrEmpty(request.Item.ipaddress))
                    request.Item.ipaddress = getStaticIP();
                List<Users> result = await Task<List<Users>>.Run(() => UsersData.Search(request.Item));
                return this.Ok(result);
            }catch(Exception exp)
            {
                
                return this.Ok(new GenericResponse<Exception>(exp)
                {
                    Success = false,
                    ErrorMessage = exp.Message
                });
            }
        }

        [HttpPost]
        [Route("Save")]
        public async Task<IHttpActionResult> Save(ActionRequest<Users> request)
        {
            UsersDataService UsersData = new UsersDataService();
            if (request.Item.id <= 0)
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Create);
                Users result = await Task<Users>.Run(() => UsersData.Insert(request.Item));
                return this.Ok(result);
            }
            else
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Update);
                Users result = await Task<Users>.Run(() => UsersData.Update(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("Insert")]
        public async Task<IHttpActionResult> Insert(ActionRequest<Users> request)
        {
            UsersDataService UsersData = new UsersDataService();
            Users result = await Task<Users>.Run(() => UsersData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("GenerateAccessKey")]
        public async Task<IHttpActionResult> GenerateAccessKey(ActionRequest<Users> request)
        {
            UsersDataService UsersData = new UsersDataService();
            Users result = await Task<Users>.Run(() => UsersData.GenerateAccessKey(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        public async Task<IHttpActionResult> Update(ActionRequest<Users> request)
        {
            UsersDataService UsersData = new UsersDataService();
            Users result = await Task<Users>.Run(() => UsersData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("ChangePassword")]
        [AllowAnonymous]
        public async Task<IHttpActionResult> ChangePassword(ActionRequest<UsersCreteria> request)
        {
            UsersDataService UsersData = new UsersDataService();
            UsersData.Context.OrganizationID = OrganisationDataService.GetOrganizationID(request.Item.organizationcode);
            Users result = await Task<Users>.Run(() => UsersData.ChangePassword(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IHttpActionResult> Delete(ActionRequest<Users> request)
        {
            UsersDataService UsersData = new UsersDataService();
            bool result = await Task<bool>.Run(() => UsersData.Delete(request.Item));
            return this.Ok(result);
        }
    }
}