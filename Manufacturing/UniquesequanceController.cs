
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
    [RoutePrefix("UniquesequanceService")]
    public class UniquesequanceServiceController : ApiController
    {

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<Uniquesequance>();
            result.Item = new Uniquesequance();
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<UniquesequanceCreteria> request)
        {
            UniquesequanceDataService UniquesequanceData = new UniquesequanceDataService();
            List<Uniquesequance> result = await Task<List<Uniquesequance>>.Run(() => UniquesequanceData.Search(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("DeleteAll")]
        public async Task<IHttpActionResult> DeleteAll(ActionMulitpleRequest<Uniquesequance> request)
        {
            UniquesequanceDataService UniquesequanceData = new UniquesequanceDataService();
            try
            {
                List<Uniquesequance> result = await Task<List<Uniquesequance>>.Run(() => UniquesequanceData.DeleteAll(request.Items));
                return this.Ok(result);
            }
            catch (Exception exp)
            {
                return this.InternalServerError(exp);
            }
        }

        [HttpPost]
        [Route("SaveAll")]
        public async Task<IHttpActionResult> SaveAll(ActionMulitpleRequest<Uniquesequance> request)
        {
            UniquesequanceDataService UniquesequanceData = new UniquesequanceDataService();
            try
            {
                List<Uniquesequance> result = await Task<List<Uniquesequance>>.Run(() => UniquesequanceData.InsertAll(request.Items));
                return this.Ok(result);
            }catch(Exception exp)
            {
                return this.InternalServerError(exp);
            }
        }


        [HttpPost]
        [Route("Save")]
        public async Task<IHttpActionResult> Save(ActionRequest<Uniquesequance> request)
        {
            UniquesequanceDataService UniquesequanceData = new UniquesequanceDataService();
            if (request.Item.id <= 0)
            {
                Uniquesequance result = await Task<Uniquesequance>.Run(() => UniquesequanceData.Insert(request.Item));
                return this.Ok(result);
            }
            else
            {
                Uniquesequance result = await Task<Uniquesequance>.Run(() => UniquesequanceData.Update(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("Insert")]
        public async Task<IHttpActionResult> Insert(ActionRequest<Uniquesequance> request)
        {
            UniquesequanceDataService UniquesequanceData = new UniquesequanceDataService();
            Uniquesequance result = await Task<Uniquesequance>.Run(() => UniquesequanceData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        public async Task<IHttpActionResult> Update(ActionRequest<Uniquesequance> request)
        {
            UniquesequanceDataService UniquesequanceData = new UniquesequanceDataService();
            Uniquesequance result = await Task<Uniquesequance>.Run(() => UniquesequanceData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IHttpActionResult> Delete(ActionRequest<Uniquesequance> request)
        {
            UniquesequanceDataService UniquesequanceData = new UniquesequanceDataService();
            bool result = await Task<bool>.Run(() => UniquesequanceData.Delete(request.Item));
            return this.Ok(result);
        }
    }
}