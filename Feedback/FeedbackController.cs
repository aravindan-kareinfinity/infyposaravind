
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
    [RoutePrefix("FeedbackService")]
    public class FBAnswerServiceController : ApiController
    {

        [HttpGet]
        [Route("Dashboard")]
        public async Task<IHttpActionResult> Dashboard()
        {
            FeedbackDataService FeedbackData = new FeedbackDataService();
            RatingSummary result = await Task<RatingSummary>.Run(() => FeedbackData.GetRatings());
            return this.Ok(result);
        }

        [HttpPost]
        [Route("SearchChartData")]
        public async Task<IHttpActionResult> SearchChartData(ActionRequest<FeedbackCreteria> request)
        {
            FeedbackDataService FeedbackData = new FeedbackDataService();
            FeedbackQASummary result = await Task<FeedbackQASummary>.Run(() => FeedbackData.GetChartData(request.Item.fromdate, request.Item.todate));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("SearchReport")]
        public async Task<IHttpActionResult> SearchReport(ActionRequest<ReportCreteria> request)
        {
            FeedbackDataService FeedbackData = new FeedbackDataService();
            RPTSummary result = await Task<RPTSummary>.Run(() => FeedbackData.SearchDetail(request.Item));
            return this.Ok(result);
        }


        [HttpPost]
        [Route("Register")]
        public async Task<IHttpActionResult> Register(ActionRequest<FBRegistration> request)
        {
            FeedbackDataService FeedbackData = new FeedbackDataService();
            FBRegistration result = await Task<FBRegistration>.Run(() => FeedbackData.Register(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<FeedbackCreteria> request)
        {
            FeedbackDataService FeedbackData = new FeedbackDataService();
            List<FBAnswer> result = await Task<List<FBAnswer>>.Run(() => FeedbackData.Search(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Save")]
        public async Task<IHttpActionResult> Save(ActionRequest<FBAnswer> request)
        {
            FeedbackDataService FeedbackData = new FeedbackDataService();
            if (request.Item.id <= 0)
            {
                FBAnswer result = await Task<FBAnswer>.Run(() => FeedbackData.Insert(request.Item));
                return this.Ok(result);
            }
            else
            {
                FBAnswer result = await Task<FBAnswer>.Run(() => FeedbackData.Update(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("Insert")]
        public async Task<IHttpActionResult> Insert(ActionRequest<FBAnswer> request)
        {
            FeedbackDataService FeedbackData = new FeedbackDataService();
            FBAnswer result = await Task<FBAnswer>.Run(() => FeedbackData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        public async Task<IHttpActionResult> Update(ActionRequest<FBAnswer> request)
        {
            FeedbackDataService FeedbackData = new FeedbackDataService();
            FBAnswer result = await Task<FBAnswer>.Run(() => FeedbackData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IHttpActionResult> Delete(ActionRequest<FBAnswer> request)
        {
            FeedbackDataService FeedbackData = new FeedbackDataService();
            bool result = await Task<bool>.Run(() => FeedbackData.Delete(request.Item));
            return this.Ok(result);
        }
    }
}