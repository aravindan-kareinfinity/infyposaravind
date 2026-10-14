
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
    [RoutePrefix("Mfg_Swatches_FolderService")]
    public class Mfg_Swatches_FolderServiceController : ApiController
    {

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<Mfg_Swatches_Folder>();
            result.Item = new Mfg_Swatches_Folder();
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<Mfg_Swatches_FolderCreteria> request)
        {
            Mfg_Swatches_FolderDataService Mfg_Swatches_FolderData = new Mfg_Swatches_FolderDataService();
            List<Mfg_Swatches_FolderDetail> result = await Task<List<Mfg_Swatches_FolderDetail>>.Run(() => Mfg_Swatches_FolderData.Search(request.Item));
            return this.Ok(result);
        }


        [HttpPost]
        [Route("Summary")]
        public async Task<IHttpActionResult> Summary(ActionRequest<Mfg_Swatches_creteria> request)
        {
            Mfg_Swatches_FolderDataService Mfg_Swatches_FolderData = new Mfg_Swatches_FolderDataService();
            List<Mfg_Swatches_FolderSummary> result = await Task<List<Mfg_Swatches_FolderSummary>>.Run(() => Mfg_Swatches_FolderData.Summary(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Journal")]
        public async Task<IHttpActionResult> Journal(ActionMulitpleRequest<Mfg_Folder_Journal> request)
        {
            Mfg_Swatches_FolderDataService Mfg_Swatches_FolderData = new Mfg_Swatches_FolderDataService();
            List<Mfg_Swatches_HistoryDetail> result = await Task<List<Mfg_Swatches_HistoryDetail>>.Run(() => Mfg_Swatches_FolderData.Journal(request.Items));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Issue")]
        public async Task<IHttpActionResult> Issue(ActionMulitpleRequest<Mfg_Folder_Issue> request)
        {
            Mfg_Swatches_FolderDataService Mfg_Swatches_FolderData = new Mfg_Swatches_FolderDataService();
            List<Mfg_Swatches_HistoryDetail> result = await Task<List<Mfg_Swatches_HistoryDetail>>.Run(() => Mfg_Swatches_FolderData.Issue(request.Items));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Receive")]
        public async Task<IHttpActionResult> Receive(ActionMulitpleRequest<Mfg_Folder_Receive> request)
        {
            Mfg_Swatches_FolderDataService Mfg_Swatches_FolderData = new Mfg_Swatches_FolderDataService();
            List<Mfg_Swatches_HistoryDetail> result = await Task<List<Mfg_Swatches_HistoryDetail>>.Run(() => Mfg_Swatches_FolderData.Receive(request.Items));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Stock")]
        public async Task<IHttpActionResult> Stock(ActionRequest<Mfg_Swatches_FolderCreteria> request)
        {
            Mfg_Swatches_FolderDataService Mfg_Swatches_FolderData = new Mfg_Swatches_FolderDataService();
            List<Mfg_Swatches_FolderDetail> result = await Task<List<Mfg_Swatches_FolderDetail>>.Run(() => Mfg_Swatches_FolderData.Stock(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Save")]
        public async Task<IHttpActionResult> Save(ActionRequest<Mfg_Swatches_Folder> request)
        {
            Mfg_Swatches_FolderDataService Mfg_Swatches_FolderData = new Mfg_Swatches_FolderDataService();
            if (request.Item.id <= 0)
            {
                Mfg_Swatches_Folder result = await Task<Mfg_Swatches_Folder>.Run(() => Mfg_Swatches_FolderData.Insert(request.Item));
                return this.Ok(result);
            }
            else
            {
                Mfg_Swatches_Folder result = await Task<Mfg_Swatches_Folder>.Run(() => Mfg_Swatches_FolderData.Update(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("Insert")]
        public async Task<IHttpActionResult> Insert(ActionRequest<Mfg_Swatches_Folder> request)
        {
            Mfg_Swatches_FolderDataService Mfg_Swatches_FolderData = new Mfg_Swatches_FolderDataService();
            Mfg_Swatches_Folder result = await Task<Mfg_Swatches_Folder>.Run(() => Mfg_Swatches_FolderData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        public async Task<IHttpActionResult> Update(ActionRequest<Mfg_Swatches_Folder> request)
        {
            Mfg_Swatches_FolderDataService Mfg_Swatches_FolderData = new Mfg_Swatches_FolderDataService();
            Mfg_Swatches_Folder result = await Task<Mfg_Swatches_Folder>.Run(() => Mfg_Swatches_FolderData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IHttpActionResult> Delete(ActionRequest<Mfg_Swatches_Folder> request)
        {
            Mfg_Swatches_FolderDataService Mfg_Swatches_FolderData = new Mfg_Swatches_FolderDataService();
            bool result = await Task<bool>.Run(() => Mfg_Swatches_FolderData.Delete(request.Item));
            return this.Ok(result);
        }
    }
}