
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
    [RoutePrefix("LrissuelistService")]
    public class LrissuelistServiceController : ApiController
    {

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<Lrissuelist>();
            result.Item = new Lrissuelist();
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<LrissuelistCreteria> request)
        {
            LrissuelistDataService LrissuelistData = new LrissuelistDataService();
            List<Lrissuelist> result = await Task<List<Lrissuelist>>.Run(() => LrissuelistData.Search(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Save")]
        public async Task<IHttpActionResult> Save(ActionRequest<Lrissuelist> request)
        {
            LrissuelistDataService LrissuelistData = new LrissuelistDataService();
            if (request.Item.id <= 0)
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Create);
                Lrissuelist result = await Task<Lrissuelist>.Run(() => LrissuelistData.Insert(request.Item));
                return this.Ok(result);
            }
            else
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Update);
                Lrissuelist result = await Task<Lrissuelist>.Run(() => LrissuelistData.Update(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("Insert")]
        public async Task<IHttpActionResult> Insert(ActionRequest<Lrissuelist> request)
        {
            LrissuelistDataService LrissuelistData = new LrissuelistDataService();
            Lrissuelist result = await Task<Lrissuelist>.Run(() => LrissuelistData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        public async Task<IHttpActionResult> Update(ActionRequest<Lrissuelist> request)
        {
            LrissuelistDataService LrissuelistData = new LrissuelistDataService();
            Lrissuelist result = await Task<Lrissuelist>.Run(() => LrissuelistData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IHttpActionResult> Delete(ActionRequest<Lrissuelist> request)
        {
            LrissuelistDataService LrissuelistData = new LrissuelistDataService();
            bool result = await Task<bool>.Run(() => LrissuelistData.Delete(request.Item));
            return this.Ok(result);
        }
    }
}