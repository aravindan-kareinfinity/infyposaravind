
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
    [RoutePrefix("CRMConfigService")]
    public class CRMConfigServiceController : ApiController
    {

        [HttpGet]
        [Route("Current")]
        public async Task<IHttpActionResult> Entity()
        {
            CRMConfigDataService CRMConfigData = new CRMConfigDataService();
            CRMConfig result = await Task<CRMConfig>.Run(() => CRMConfigData.Search());
            return this.Ok(result);
        }

      

        [HttpPost]
        [Route("Save")]
        public async Task<IHttpActionResult> Save(ActionRequest<CRMConfig> request)
        {
            CRMConfigDataService CRMConfigData = new CRMConfigDataService();
                CRMConfig result = await Task<CRMConfig>.Run(() => CRMConfigData.Save(request.Item));
                return this.Ok(result);
        }
    }
}