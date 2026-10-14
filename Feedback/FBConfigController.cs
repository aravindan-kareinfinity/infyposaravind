
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
    [RoutePrefix("FBConfigService")]
    public class FBConfigServiceController : ApiController
    {

        [HttpGet]
        [Route("Current")]
        public async Task<IHttpActionResult> Entity()
        {
            FBConfigDataService FBConfigData = new FBConfigDataService();
            FBConfig result = await Task<FBConfig>.Run(() => FBConfigData.Search());
            return this.Ok(result);
        }

      

        [HttpPost]
        [Route("Save")]
        public async Task<IHttpActionResult> Save(ActionRequest<FBConfig> request)
        {
            FBConfigDataService FBConfigData = new FBConfigDataService();
                FBConfig result = await Task<FBConfig>.Run(() => FBConfigData.Save(request.Item));
                return this.Ok(result);
        }

    }
}