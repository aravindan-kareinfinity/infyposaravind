
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
    [RoutePrefix("OpeningbalancesService")]
    public class OpeningbalancesServiceController : ApiController
    {

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<Openingbalances>();
            result.Item = new Openingbalances();
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<OpeningbalancesCreteria> request)
        {
            OpeningbalancesDataService OpeningbalancesData = new OpeningbalancesDataService();
            List<Openingbalances> result = await Task<List<Openingbalances>>.Run(() => OpeningbalancesData.Search(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Save")]
        public async Task<IHttpActionResult> Save(ActionRequest<Openingbalances> request)
        {
            OpeningbalancesDataService OpeningbalancesData = new OpeningbalancesDataService();
            if (request.Item.id <= 0)
            {
                Openingbalances result = await Task<Openingbalances>.Run(() => OpeningbalancesData.Insert(request.Item));
                return this.Ok(result);
            }
            else
            {
                Openingbalances result = await Task<Openingbalances>.Run(() => OpeningbalancesData.Update(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("Insert")]
        public async Task<IHttpActionResult> Insert(ActionRequest<Openingbalances> request)
        {
            OpeningbalancesDataService OpeningbalancesData = new OpeningbalancesDataService();
            Openingbalances result = await Task<Openingbalances>.Run(() => OpeningbalancesData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        public async Task<IHttpActionResult> Update(ActionRequest<Openingbalances> request)
        {
            OpeningbalancesDataService OpeningbalancesData = new OpeningbalancesDataService();
            Openingbalances result = await Task<Openingbalances>.Run(() => OpeningbalancesData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IHttpActionResult> Delete(ActionRequest<Openingbalances> request)
        {
            OpeningbalancesDataService OpeningbalancesData = new OpeningbalancesDataService();
            bool result = await Task<bool>.Run(() => OpeningbalancesData.Delete(request.Item));
            return this.Ok(result);
        }
    }
}