
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
    [RoutePrefix("Mfg_SwatchesService")]
    public class Mfg_SwatchesServiceController : ApiController
    {

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<Mfg_Swatches>();
            result.Item = new Mfg_Swatches();
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<Mfg_SwatchesCreteria> request)
        {
            Mfg_SwatchesDataService Mfg_SwatchesData = new Mfg_SwatchesDataService();
            List<Mfg_Swatches> result = await Task<List<Mfg_Swatches>>.Run(() => Mfg_SwatchesData.Search(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Save")]
        public async Task<IHttpActionResult> Save(ActionRequest<Mfg_Swatches> request)
        {
            Mfg_SwatchesDataService Mfg_SwatchesData = new Mfg_SwatchesDataService();
            if (request.Item.id <= 0)
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Create);
                Mfg_Swatches result = await Task<Mfg_Swatches>.Run(() => Mfg_SwatchesData.Insert(request.Item));
                return this.Ok(result);
            }
            else
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Update);
                Mfg_Swatches result = await Task<Mfg_Swatches>.Run(() => Mfg_SwatchesData.Update(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("Insert")]
        public async Task<IHttpActionResult> Insert(ActionRequest<Mfg_Swatches> request)
        {
            Mfg_SwatchesDataService Mfg_SwatchesData = new Mfg_SwatchesDataService();
            Mfg_Swatches result = await Task<Mfg_Swatches>.Run(() => Mfg_SwatchesData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        public async Task<IHttpActionResult> Update(ActionRequest<Mfg_Swatches> request)
        {
            Mfg_SwatchesDataService Mfg_SwatchesData = new Mfg_SwatchesDataService();
            Mfg_Swatches result = await Task<Mfg_Swatches>.Run(() => Mfg_SwatchesData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IHttpActionResult> Delete(ActionRequest<Mfg_Swatches> request)
        {
            Mfg_SwatchesDataService Mfg_SwatchesData = new Mfg_SwatchesDataService();
            request.Item.ChangeStatus(BaseEntity.DataStatus.Delete);
            bool result = await Task<bool>.Run(() => Mfg_SwatchesData.Delete(request.Item));
            return this.Ok(result);
        }
    }
}