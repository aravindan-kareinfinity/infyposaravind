
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
using System.Net.Http.Headers;
using System.Collections.Specialized;

namespace InfyPOS.Controllers
{
    [RoutePrefix("FB")]
    public class FeedbackController : ApiController
    {
        public class FileContentInfo
        {
            public FileContentInfo(ByteArrayContent content)
            {
                this.Content = content;
            }
            public ByteArrayContent Content { get; set; }
            public string FileName { get; set; }
            public string Extension { get; set; }

        }

        public class FileActionResult : IHttpActionResult
        {
            HttpResponseMessage response = null;
            public FileActionResult(bool isAttachment,
                string fileName,
                string imageExtn,
                ByteArrayContent content)
            {
                response = new HttpResponseMessage();
                response.Content = content;
                if (!string.IsNullOrEmpty(imageExtn) && imageExtn.Split('/').Length == 2 && imageExtn.Split('/')[1].Length > 0)
                {
                    response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(imageExtn);
                    response.Content.Headers.ContentType.CharSet = System.Text.Encoding.Unicode.WebName;
                    response.Content.Headers.Expires = DateTime.Now.AddMinutes(-1);
                }
                if (isAttachment)
                {
                    response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
                    response.Content.Headers.ContentDisposition.FileName = fileName;
                }
            }


            public Task<HttpResponseMessage> ExecuteAsync(System.Threading.CancellationToken cancellationToken)
            {
                return Task.FromResult(response);
            }
        }

        const string answerlisttemplate = @"
        <div class='answer' style='margin-bottom:10px'>
            <div>
                {4} {0}
                <input id ='Q_{1}' name='Q_{1}' type='hidden' value='{2}'>
            </div>
            {3}
        </div>";

        const string questionlisttemplate = @"
            <div>
                <div class='radio'>
                    <input id ='Q_{0}_A_{1}' name='A_{0}' type='radio' value='{2}'>
                    <label for='Q_{0}_A_{1}' class='radio-label'>{2}</label>
                </div>
            </div>";

        public FileContentInfo BuildHTML(string source, IEnumerable<KeyValuePair<string, string>> enumerable)
        {
            var parameter = enumerable.ToList();
            string reference = parameter.Exists(e => e.Key.ToLower() == "r") ? parameter.Find(e => e.Key.ToLower() == "r").Value : "";
            string orgbranch = parameter.Exists(e => e.Key.ToLower() == "b") ? parameter.Find(e => e.Key.ToLower() == "b").Value : "";
            string customername = parameter.Exists(e => e.Key.ToLower() == "n") ? parameter.Find(e => e.Key.ToLower() == "n").Value : "";
            string customermobile = parameter.Exists(e => e.Key.ToLower() == "m") ? parameter.Find(e => e.Key.ToLower() == "m").Value : "";
            //0G000000000

            var html = prepareHTML(source == "0" ? 0 : source.LongFromBase62());
            html = html.Replace("{{customername}}", customername);
            html = html.Replace("{{customermobile}}", customermobile);
            html = html.Replace("{{orgbranch}}", orgbranch);
            html = html.Replace("{{reference}}", reference);
            html = html.Replace("{{orgcode}}", source);

            var fci = new FileContentInfo(new ByteArrayContent(System.Text.UnicodeEncoding.Unicode.GetBytes(html)));
            fci.FileName = "Feedback.html";
            fci.Extension = "text/html";
            return fci;
        }

        public string prepareHTML(long l)
        {
            string html = "";

            var file = System.Web.Hosting.HostingEnvironment.MapPath("~/Clientside/Feedback.html");
            html = System.IO.File.ReadAllText(file);

            FBConfigDataService fBConfigDataService = new FBConfigDataService()
            {
                Context = new UserClaim() { OrganizationID = l }
            };
            var fbconfig = fBConfigDataService.Search();
            QuestionsDataService questionsDataService = new QuestionsDataService() { Context = new UserClaim() { OrganizationID = l } };
            var questionlist = questionsDataService.Search(new QuestionsCreteria()
            {
            }).OrderBy(e => e.index).ToList().Where(e => e.enable).ToList();
            System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();

            foreach (var q in questionlist)
            {
                System.Text.StringBuilder sblist = new System.Text.StringBuilder();
                foreach (var ans in q.answerlist)
                {
                    sblist.AppendFormat(questionlisttemplate, q.id, ans.id, ans.answer);
                }
                stringBuilder.AppendFormat(answerlisttemplate,
                    q.question, q.id, q.code, sblist.ToString(), "");
                stringBuilder.AppendLine();
            }

            html = html.Replace("{{FeedBackQuestionList}}", stringBuilder.ToString());
            html = html.Replace("{{feedbackrating}}", fbconfig.feedbackrating);

            html = html.Replace("{{feedbackmobileno}}", fbconfig.feedbackmobileno);
            html = html.Replace("{{feedbackname}}", fbconfig.feedbackname);

            html = html.Replace("{{feedbackcomplaint}}", fbconfig.feedbackcomplaint);
            html = html.Replace("{{feedbackcomplaintshow}}", fbconfig.collectcomplaint ? "inherit" : "none");
            html = html.Replace("{{feedbacksuggesion}}", fbconfig.feedbacksuggestion);
            html = html.Replace("{{feedbacksuggesionshow}}", fbconfig.collectsuggestion ? "inherit" : "none");

            html = html.Replace("{{mobilebanner}}", fbconfig.mobilebanner);
            html = html.Replace("{{desktopbanner}}", fbconfig.desktopbanner);
            html = html.Replace("{{mobilebannerheight}}", fbconfig.mobilebannerheight.ToString("F0"));
            html = html.Replace("{{mobilebannerwidth}}", fbconfig.mobilebannerwidth.ToString("F0"));
            html = html.Replace("{{desktopbannerheight}}", fbconfig.desktopbannerheight.ToString("F0"));
            html = html.Replace("{{desktopbannerwidth}}", fbconfig.desktopbannerwidth.ToString("F0"));
            html = html.Replace("{{feedbackheader}}", fbconfig.html.header);

            return html;
        }

        public string prepareResponseHTML(long l)
        {

            FBConfigDataService fBConfigDataService = new FBConfigDataService()
            {
                Context = new UserClaim() { OrganizationID = l }
            };
            var fbconfig = fBConfigDataService.Search();
            var file = System.Web.Hosting.HostingEnvironment.MapPath("~/Clientside/Response.html");
            var html = System.IO.File.ReadAllText(file);
            html = html.Replace("{{mobilebanner}}", fbconfig.mobilebanner);
            html = html.Replace("{{desktopbanner}}", fbconfig.desktopbanner);
            html = html.Replace("{{mobilebannerheight}}", fbconfig.mobilebannerheight.ToString("F0"));
            html = html.Replace("{{mobilebannerwidth}}", fbconfig.mobilebannerwidth.ToString("F0"));
            html = html.Replace("{{desktopbannerheight}}", fbconfig.desktopbannerheight.ToString("F0"));
            html = html.Replace("{{desktopbannerwidth}}", fbconfig.desktopbannerwidth.ToString("F0"));
            html = html.Replace("{{feedbackresponse}}", fbconfig.html.response);
            return html;
        }
        public FileContentInfo BuildResponseHTML(NameValueCollection formdata)
        {
            
            long l = int.Parse(formdata["FeedbackID"]);

            QuestionsDataService questionsDataService = new QuestionsDataService();
            var questions = questionsDataService.Search(new QuestionsCreteria()
            {
            });


            FBAnswer feedback = new FBAnswer();
            feedback.name = formdata["name"];
            feedback.mobileno = formdata["mobileno"];
            feedback.area = formdata["FeedbackBranch"];
            feedback.complaintsuggesion = new FBAnswer.Complaintsuggesion();
            feedback.complaintsuggesion.complaint = formdata["complaint"];
            feedback.complaintsuggesion.suggesion = formdata["suggesion"];
            feedback.hascomplaint = !string.IsNullOrEmpty(feedback.complaintsuggesion.complaint) && feedback.complaintsuggesion.complaint.Length > 10;
            feedback.hassugession = !string.IsNullOrEmpty(feedback.complaintsuggesion.suggesion) && feedback.complaintsuggesion.suggesion.Length > 10;
            feedback.overrallrating = int.Parse(formdata["Ratingvalue"]);
            feedback.feedbackdate = DateTime.Now;
            feedback.feedbackstatus = 0;
            feedback.referenceid = formdata["FeedbackReference"];
            feedback.qafeedback = new List<FBAnswer.Qafeedback>();
            foreach (var item in formdata.AllKeys)
            {
                if (item.StartsWith("Q_"))
                {
                    string[] parts = item.Split('_');
                    long qid = 0;
                    long aid = 0;
                    long.TryParse(parts[1], out qid);
                    string question = formdata[item];
                    string answer = formdata["A_" + parts[1]];

                    if (questions.Exists(e => e.id == qid) &&
                        questions.Find(e => e.id == qid).answerlist.Exists(x => x.answer == answer))
                    {
                        aid = questions.Find(e => e.id == qid).answerlist.Find(x => x.answer == answer).id;
                    }
                    feedback.qafeedback.Add(new FBAnswer.Qafeedback()
                    {
                        question = question,
                        answer = answer,
                        questionid = qid,
                        answerid = aid,
                        uniqueid = string.Format("{0}_{1}", qid, aid)
                    });
                }
            }
            FeedbackDataService feedbackDataService = new FeedbackDataService();
            feedbackDataService.Insert(feedback);

            string html = prepareResponseHTML(l);
            var fci = new FileContentInfo(new ByteArrayContent(System.Text.UnicodeEncoding.Unicode.GetBytes(html)));
            fci.FileName = "Response.html";
            fci.Extension = "text/html";

            return fci;
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("Save")]
        public async Task<IHttpActionResult> Save()
        {
            FileContentInfo byteArray = await Task<FileContentInfo>.Run(() => SaveFeedback(Request.Content));
            if (byteArray == null)
                return this.NotFound();
            return new FileActionResult(false, "", byteArray.Extension, byteArray.Content);
        }

        private FileContentInfo SaveFeedback(HttpContent content)
        {
            var formdata = content.ReadAsFormDataAsync().Result;
            return BuildResponseHTML(formdata);
        }


        [AllowAnonymous]
        [HttpGet]
        [Route("Q/{key}")]
        public async Task<IHttpActionResult> Build(string key)
        {
            FileContentInfo byteArray = await Task<FileContentInfo>.Run(() => BuildHTML(key, Request.GetQueryNameValuePairs()));
            if (byteArray == null)
                return this.NotFound();
            return new FileActionResult(false, "", byteArray.Extension, byteArray.Content);
        }

    }
}