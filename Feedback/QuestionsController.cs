
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
    [RoutePrefix("QuestionService")]
    public class QuestionsServiceController : ApiController
    {

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<FBQuestions>();
            result.Item = new FBQuestions();
            return this.Ok(result);
        }

        [HttpGet]
        [Route("SearchAll")]
        public async Task<IHttpActionResult> SearchAll()
        {
            QuestionsDataService QuestionsData = new QuestionsDataService();
            List<FBQuestions> result = await Task<List<FBQuestions>>.Run(() => QuestionsData.SearchAll());
            result.RemoveAll(e => !e.enable);
            return this.Ok(result);
        }
        [HttpGet]
        [Route("SearchAllQuestions")]
        public async Task<IHttpActionResult> SearchAllQuestions()
        {
            QuestionsDataService QuestionsData = new QuestionsDataService();
            List<FBQuestions> result = await Task<List<FBQuestions>>.Run(() => QuestionsData.SearchAll());
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<QuestionsCreteria> request)
        {
            QuestionsDataService QuestionsData = new QuestionsDataService();
            List<FBQuestions> result = await Task<List<FBQuestions>>.Run(() => QuestionsData.Search(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Save")]
        public async Task<IHttpActionResult> Save(ActionRequest<FBQuestions> request)
        {
            QuestionsDataService QuestionsData = new QuestionsDataService();
            if (request.Item.id <= 0)
            {
                FBQuestions result = await Task<FBQuestions>.Run(() => QuestionsData.Insert(request.Item));
                return this.Ok(result);
            }
            else
            {
                FBQuestions result = await Task<FBQuestions>.Run(() => QuestionsData.Update(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("Insert")]
        public async Task<IHttpActionResult> Insert(ActionRequest<FBQuestions> request)
        {
            QuestionsDataService QuestionsData = new QuestionsDataService();
            FBQuestions result = await Task<FBQuestions>.Run(() => QuestionsData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        public async Task<IHttpActionResult> Update(ActionRequest<QuestionChanges> request)
        {
            QuestionsDataService QuestionsData = new QuestionsDataService();
            bool result = await Task<bool>.Run(() => QuestionsData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IHttpActionResult> Delete(ActionRequest<FBQuestions> request)
        {
            QuestionsDataService QuestionsData = new QuestionsDataService();
            bool result = await Task<bool>.Run(() => QuestionsData.Delete(request.Item));
            return this.Ok(result);
        }
    }
}