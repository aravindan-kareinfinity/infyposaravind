
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
    [RoutePrefix("OrganisationService")]
    public class OrganisationServiceController : ApiController
    {

        [HttpGet]
        [Route("CreateDB/{organizationid}")]
        public async Task<IHttpActionResult> CreateDB(string organizationid)
        {
            OrganisationDataService OrganisationData = new OrganisationDataService();
            string result = await Task<string>.Run(() => OrganisationData.CreateDB(long.Parse(organizationid)));
            return this.Ok(result);
        }

        [HttpGet]
        [Route("CreateUsers/{organizationid}")]
        public async Task<IHttpActionResult> CreateUsers(string organizationid)
        {
            OrganisationDataService OrganisationData = new OrganisationDataService();
            string result = await Task<string>.Run(() => OrganisationData.CreateAdminUsers(long.Parse(organizationid)));
            return this.Ok(result);
        }

        [HttpGet]
        [Route("Search/{organizationcode}")]
        public async Task<IHttpActionResult> Search(string organizationcode)
        {
            OrganisationDataService OrganisationData = new OrganisationDataService();
            Organisation result = await Task<Organisation>.Run(() => OrganisationData.Search(organizationcode));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<OrganisationCreteria> request)
        {
            OrganisationDataService OrganisationData = new OrganisationDataService();
            List<Organisation> result = await Task<List<Organisation>>.Run(() => OrganisationData.Search(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Save")]
        public async Task<IHttpActionResult> Save(ActionRequest<Organisation> request)
        {
            OrganisationDataService OrganisationData = new OrganisationDataService();
            if (request.Item.id <= 0)
            {
                Organisation result = await Task<Organisation>.Run(() => OrganisationData.Insert(request.Item));
                return this.Ok(result);
            }
            else
            {
                Organisation result = await Task<Organisation>.Run(() => OrganisationData.Update(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("Insert")]
        public async Task<IHttpActionResult> Insert(ActionRequest<Organisation> request)
        {
            OrganisationDataService OrganisationData = new OrganisationDataService();
            Organisation result = await Task<Organisation>.Run(() => OrganisationData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        public async Task<IHttpActionResult> Update(ActionRequest<Organisation> request)
        {
            OrganisationDataService OrganisationData = new OrganisationDataService();
            Organisation result = await Task<Organisation>.Run(() => OrganisationData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IHttpActionResult> Delete(ActionRequest<Organisation> request)
        {
            OrganisationDataService OrganisationData = new OrganisationDataService();
            bool result = await Task<bool>.Run(() => OrganisationData.Delete(request.Item));
            return this.Ok(result);
        }
    }
}