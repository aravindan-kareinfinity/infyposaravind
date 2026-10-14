
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
    [RoutePrefix("UsersessionService")]
    public class UsersessionServiceController : ApiController
    {

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<Usersession>();
            result.Item = new Usersession();
            return this.Ok(result);
        }

        [HttpPost]
        [Route("SetMobileToken")]
        public async Task<IHttpActionResult> Search(ActionRequest<UsersToken> request)
        {
            UsersessionDataService UsersData = new UsersessionDataService();
            try
            {
                bool result = await Task<bool>.Run(() => UsersData.SetMobileToken(request.Item));
                return this.Ok(result);
            }
            catch (Exception exp)
            {
                return this.Ok(new GenericResponse<Exception>(exp)
                {
                    Success = false,
                    ErrorMessage = exp.Message
                });
            }
        }

        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<UsersessionCreteria> request)
        {
            UsersessionDataService UsersessionData = new UsersessionDataService();
            List<Usersession> result = await Task<List<Usersession>>.Run(() => UsersessionData.Search(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Save")]
        public async Task<IHttpActionResult> Save(ActionRequest<Usersession> request)
        {
            UsersessionDataService UsersessionData = new UsersessionDataService();
            if (request.Item.id <= 0)
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Create);
                Usersession result = await Task<Usersession>.Run(() => UsersessionData.Insert(request.Item));
                return this.Ok(result);
            }
            else
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Update);
                Usersession result = await Task<Usersession>.Run(() => UsersessionData.Update(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("Insert")]
        public async Task<IHttpActionResult> Insert(ActionRequest<Usersession> request)
        {
            UsersessionDataService UsersessionData = new UsersessionDataService();
            Usersession result = await Task<Usersession>.Run(() => UsersessionData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Logout")]
        public async Task<IHttpActionResult> Logout(ActionRequest<Usersession> request)
        {
            UsersessionDataService UsersessionData = new UsersessionDataService();
            bool result = await Task<bool>.Run(() => UsersessionData.Logout(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("KillUser")]
        public async Task<IHttpActionResult> KillUser(ActionRequest<Usersession> request)
        {
            UsersessionDataService UsersessionData = new UsersessionDataService();
            bool result = await Task<bool>.Run(() => UsersessionData.KillUser(request.Item));
            return this.Ok(result);
        }
       
        [HttpPost]
        [Route("Update")]
        public async Task<IHttpActionResult> Update(ActionRequest<Usersession> request)
        {
            UsersessionDataService UsersessionData = new UsersessionDataService();
            Usersession result = await Task<Usersession>.Run(() => UsersessionData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IHttpActionResult> Delete(ActionRequest<Usersession> request)
        {
            UsersessionDataService UsersessionData = new UsersessionDataService();
            bool result = await Task<bool>.Run(() => UsersessionData.Delete(request.Item));
            return this.Ok(result);
        }
    }
}