
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
    [RoutePrefix("CRM")]
    public class CRMController : ApiController
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

        public FileContentInfo BuildHTML(IEnumerable<KeyValuePair<string, string>> enumerable)
        {
            var parameter = enumerable.ToList();
            string reference = parameter.Exists(e => e.Key.ToLower() == "r") ? parameter.Find(e => e.Key.ToLower() == "r").Value : "";
            string orgbranch = parameter.Exists(e => e.Key.ToLower() == "b") ? parameter.Find(e => e.Key.ToLower() == "b").Value : "";
            string customername = parameter.Exists(e => e.Key.ToLower() == "n") ? parameter.Find(e => e.Key.ToLower() == "n").Value : "";
            string customermobile = parameter.Exists(e => e.Key.ToLower() == "m") ? parameter.Find(e => e.Key.ToLower() == "m").Value : "";
            //0G000000000

            var html = prepareHTML();
            html = html.Replace("{{customername}}", customername);
            html = html.Replace("{{customermobile}}", customermobile);
            html = html.Replace("{{orgbranch}}", orgbranch);
            html = html.Replace("{{reference}}", reference);
            
            var fci = new FileContentInfo(new ByteArrayContent(System.Text.UnicodeEncoding.Unicode.GetBytes(html)));
            fci.FileName = "Feedback.html";
            fci.Extension = "text/html";
            return fci;
        }

        public string prepareHTML()
        {
            string html = "";

            var file = System.Web.Hosting.HostingEnvironment.MapPath("~/Clientside/register.html");
            html = System.IO.File.ReadAllText(file);

            CRMConfigDataService fBConfigDataService = new CRMConfigDataService()
            {
                Context = new UserClaim() { OrganizationID = 0 }
            };
            var fbconfig = fBConfigDataService.Search();

            if (fbconfig.desktopbannerheight == 0)
                fbconfig.desktopbannerheight = 160;
            if (fbconfig.mobilebannerheight == 0)
                fbconfig.mobilebannerheight = 100;

            html = html.Replace("{{mobilebanner}}", fbconfig.mobilebanner);
            html = html.Replace("{{desktopbanner}}", fbconfig.desktopbanner);
            html = html.Replace("{{mobilebannerheight}}", fbconfig.mobilebannerheight.ToString("F0"));
            html = html.Replace("{{mobilebannerwidth}}", fbconfig.mobilebannerwidth.ToString("F0"));
            html = html.Replace("{{desktopbannerheight}}", fbconfig.desktopbannerheight.ToString("F0"));
            html = html.Replace("{{desktopbannerwidth}}", fbconfig.desktopbannerwidth.ToString("F0"));

            html = html.Replace("{{enb_mobileno}}", fbconfig.enb_mobileno ? "block" : "none");
            html = html.Replace("{{enb_addressline1}}", fbconfig.enb_addressline1 ? "block" : "none");
            html = html.Replace("{{enb_addressline2}}", fbconfig.enb_addressline2 ? "block" : "none");
            html = html.Replace("{{enb_area}}", fbconfig.enb_area ? "block" : "none");
            html = html.Replace("{{enb_city}}", fbconfig.enb_city ? "block" : "none");
            html = html.Replace("{{enb_deskbanner}}", fbconfig.enb_deskbanner ? "block" : "none");
            html = html.Replace("{{enb_dob}}", fbconfig.enb_dob ? "block" : "none");
            html = html.Replace("{{enb_dom}}", fbconfig.enb_dom ? "block" : "none");
            html = html.Replace("{{enb_firstname}}", fbconfig.enb_firstname ? "block" : "none");
            html = html.Replace("{{enb_gender}}", fbconfig.enb_gender ? "block" : "none");
            html = html.Replace("{{enb_lastname}}", fbconfig.enb_lastname ? "block" : "none");
            html = html.Replace("{{enb_mailid}}", fbconfig.enb_mailid ? "block" : "none");
            html = html.Replace("{{enb_marital}}", fbconfig.enb_marital ? "block" : "none");
            html = html.Replace("{{enb_mobilebanner}}", fbconfig.enb_mobilebanner ? "block" : "none");
            html = html.Replace("{{enb_often}}", fbconfig.enb_often ? "block" : "none");
            html = html.Replace("{{enb_postcode}}", fbconfig.enb_postcode ? "block" : "none");
            html = html.Replace("{{enb_religion}}", fbconfig.enb_religion ? "block" : "none");
            html = html.Replace("{{enb_source}}", fbconfig.enb_source ? "block" : "none");
            html = html.Replace("{{enb_sp_dob}}", fbconfig.enb_sp_dob ? "block" : "none");
            html = html.Replace("{{enb_sp_firstname}}", fbconfig.enb_sp_firstname ? "block" : "none");
            html = html.Replace("{{enb_sp_lastname}}", fbconfig.enb_sp_lastname ? "block" : "none");
            html = html.Replace("{{enb_state}}", fbconfig.enb_state ? "block" : "none");
            html = html.Replace("{{enb_termsandcondition}}", fbconfig.enb_termsandcondition ? "block" : "none");
            html = html.Replace("{{enb_title}}", fbconfig.enb_title ? "block" : "none");

            html = html.Replace("{{fld_mobileno}}",System.Web.HttpUtility.HtmlEncode(fbconfig.fld_mobileno));
            html = html.Replace("{{fld_addressline1}}",System.Web.HttpUtility.HtmlEncode(fbconfig.fld_addressline1));
            html = html.Replace("{{fld_addressline2}}",System.Web.HttpUtility.HtmlEncode(fbconfig.fld_addressline2));
            html = html.Replace("{{fld_area}}", System.Web.HttpUtility.HtmlEncode(fbconfig.fld_area));
            html = html.Replace("{{fld_city}}",System.Web.HttpUtility.HtmlEncode(fbconfig.fld_city));
            html = html.Replace("{{fld_dob}}",System.Web.HttpUtility.HtmlEncode(fbconfig.fld_dob));
            html = html.Replace("{{fld_dom}}",System.Web.HttpUtility.HtmlEncode(fbconfig.fld_dom));
            html = html.Replace("{{fld_firstname}}",System.Web.HttpUtility.HtmlEncode(fbconfig.fld_firstname));
            html = html.Replace("{{fld_gender}}",System.Web.HttpUtility.HtmlEncode(fbconfig.fld_gender));
            html = html.Replace("{{fld_lastname}}",System.Web.HttpUtility.HtmlEncode(fbconfig.fld_lastname));
            html = html.Replace("{{fld_mailid}}",System.Web.HttpUtility.HtmlEncode(fbconfig.fld_mailid));
            html = html.Replace("{{fld_marital}}",System.Web.HttpUtility.HtmlEncode(fbconfig.fld_marital));
            html = html.Replace("{{fld_often}}",System.Web.HttpUtility.HtmlEncode(fbconfig.fld_often));
            html = html.Replace("{{fld_postcode}}",System.Web.HttpUtility.HtmlEncode(fbconfig.fld_postcode));
            html = html.Replace("{{fld_religion}}",System.Web.HttpUtility.HtmlEncode(fbconfig.fld_religion));
            html = html.Replace("{{fld_source}}",System.Web.HttpUtility.HtmlEncode(fbconfig.fld_source));
            html = html.Replace("{{fld_sp_dob}}",System.Web.HttpUtility.HtmlEncode(fbconfig.fld_sp_dob));
            html = html.Replace("{{fld_sp_firstname}}",System.Web.HttpUtility.HtmlEncode(fbconfig.fld_sp_firstname));
            html = html.Replace("{{fld_sp_lastname}}",System.Web.HttpUtility.HtmlEncode(fbconfig.fld_sp_lastname));
            html = html.Replace("{{fld_state}}",System.Web.HttpUtility.HtmlEncode(fbconfig.fld_state));
            html = html.Replace("{{fld_termsandcondition}}", System.Web.HttpUtility.HtmlEncode(fbconfig.fld_termsandcondition));


            //html = html.Replace("{{fld_termsandcondition}}",
            //String.Join("<br>",System.Web.HttpUtility.HtmlEncode(fbconfig.fld_termsandcondition).
            //Split(new string[] {"\r\n","\n"}, StringSplitOptions.RemoveEmptyEntries)));

            html = html.Replace("{{fld_title}}",System.Web.HttpUtility.HtmlEncode(fbconfig.fld_title));

            ReferencelistDataService referencelistDataService = new ReferencelistDataService();
            var list = referencelistDataService.SearchList(new ReferencelistCreteria()
            {
                types = new List<string>("AREA,CITY,STATE,RELIGION,REGSOURCE,REGVIST".Split(','))
            });

            html = html.Replace("{{fld_area_list}}", BuildSelectList("AREA", list));
            html = html.Replace("{{fld_city_list}}", BuildSelectList("CITY", list));
            html = html.Replace("{{fld_state_list}}", BuildSelectList("STATE", list));
            html = html.Replace("{{fld_religion_list}}", BuildSelectList("RELIGION", list));
            html = html.Replace("{{fld_source_list}}", BuildSelectList("REGSOURCE", list));
            html = html.Replace("{{fld_visit_list}}", BuildSelectList("REGVIST", list));
            return html;
        }

        public string BuildSelectList(string type,List<ReferenceItem> list)
        {
            var flist = list.FindAll(x => x.type == type);
            if (type == "AREA")
                return string.Join("\r\n", flist.ConvertAll(x => string.Format("<option value=\"{0}\">{1}</option>", System.Web.HttpUtility.HtmlEncode(x.name), System.Web.HttpUtility.HtmlEncode(x.name))));
            return string.Join("\r\n", flist.ConvertAll(x =>string.Format("<option value=\"{0}\">{1}</option>",x.id, System.Web.HttpUtility.HtmlEncode(x.name))));
        }

        public string prepareResponseHTML(CRMConfig fbconfig)
        {

            var file = System.Web.Hosting.HostingEnvironment.MapPath("~/Clientside/Response.html");
            var html = System.IO.File.ReadAllText(file);
            html = html.Replace("{{mobilebanner}}", fbconfig.mobilebanner);
            html = html.Replace("{{desktopbanner}}", fbconfig.desktopbanner);
            html = html.Replace("{{mobilebannerheight}}", fbconfig.mobilebannerheight.ToString("F0"));
            html = html.Replace("{{mobilebannerwidth}}", fbconfig.mobilebannerwidth.ToString("F0"));
            html = html.Replace("{{desktopbannerheight}}", fbconfig.desktopbannerheight.ToString("F0"));
            html = html.Replace("{{desktopbannerwidth}}", fbconfig.desktopbannerwidth.ToString("F0"));
            html = html.Replace("{{feedbackresponse}}", System.Web.HttpUtility.HtmlEncode(fbconfig.fld_response));
            html = html.Replace("{{failedresponse}}", System.Web.HttpUtility.HtmlEncode(fbconfig.fld_failresponse));
            html = html.Replace("{{hassuccess}}", fbconfig.fld_responsefailed ? "none" : "block");
            html = html.Replace("{{hasfailed}}", fbconfig.fld_responsefailed ? "block" : "none");

            
            return html;
        }
        public string GetString(string key, NameValueCollection formdata)
        {
            var value = formdata.Get(key);
            if (string.IsNullOrEmpty(value))
                return "";
            return value;
        }
        public long GetLong(string key, NameValueCollection formdata)
        {
            var value = formdata.Get(key);
            long valueLong = 0;
            if (long.TryParse(value, out valueLong))
            {
                return valueLong;
            }
            return 0;
        }
        public DateTime GetDate(string key, NameValueCollection formdata)
        {
            var value = formdata.Get(key);
            if (string.IsNullOrEmpty(value))
                return DateTime.MinValue;
            return BaseEntity.GetDataFromString(value, "yyyy-MM-dd");
        }
        public FileContentInfo BuildResponseHTML(NameValueCollection formdata)
        {
            CustomerDataService customerDataService = new CustomerDataService() { Context = new UserClaim() { OrganizationID = 0 } };
            Customer customer = new Customer();

            List<Customer> lst = customerDataService.Search(new CustomerCreteria() { mobileno = GetString("fld_mobileno", formdata) });
            if (lst != null && lst.Count > 0)
                customer = lst[0];

            customer.mobileno = GetString("fld_mobileno", formdata);
            customer.address = GetString("fld_addressline1", formdata) + "\r\n" + GetString("fld_addressline2", formdata);
            customer.cityid = GetLong("fld_city", formdata);
            customer.area = GetString("fld_area", formdata);
            customer.dateofbirth = GetDate("fld_dob", formdata);
            customer.marriagedate = GetDate("fld_dom", formdata);
            customer.name = GetString("fld_firstname", formdata) + " " + GetString("fld_lastname", formdata);
            customer.gender = GetString("fld_gender", formdata);
            customer.emailid = GetString("fld_mailid", formdata);
            customer.married = GetString("fld_marital", formdata) == "2";
            customer.howoftenid = GetLong("fld_often", formdata);
            customer.pincode = GetString("fld_postcode", formdata);
            customer.religionid = GetLong("fld_religion", formdata);
            customer.regsourceid = GetLong("fld_source", formdata);
            customer.spousedob = GetDate("fld_sp_dob", formdata);
            customer.spousename = GetString("fld_sp_firstname", formdata) + " " + GetString("fld_sp_lastname", formdata);
            customer.stateid = GetLong("fld_state", formdata);
            if (customer.married && !string.IsNullOrEmpty(customer.spousename))
            {
                customer.family = new List<Customer.Family>();
                customer.family.Add( new Customer.Family() {
                    relation = 1,
                    name = customer.spousename,
                    dateofbirth = customer.spousedob,
                    gender = (customer.gender == "Male" ? 2 : 1)
                });
            }

            customer.isactive = true;
            if (customer.id > 0)
            {
                customer.modifiedby = -1;
                customer.modifiedon = DateTime.Now;
                customerDataService.Update(customer);
            }
            else
            {
                customer.version = 1;
                customer.createdby = -1;
                customer.createdon = DateTime.Now;
                customer.modifiedby = -1;
                customer.modifiedon = DateTime.Now;
                customerDataService.Insert(customer);
            }

            CRMConfigDataService fBConfigDataService = new CRMConfigDataService()
            {
                Context = new UserClaim() { OrganizationID = 0 }
            };
            var fbconfig = fBConfigDataService.Search();
            if (customer.id == 0 && !string.IsNullOrEmpty(customer.ErrorMessage))
            {
                fbconfig.fld_failresponse = customer.ErrorMessage;
                fbconfig.fld_responsefailed = true;
            }
            else if (customer.id > 0 && string.IsNullOrEmpty(customer.ErrorMessage))
                fbconfig.fld_response = string.Format(fbconfig.fld_response, customer.name);

            string html = prepareResponseHTML(fbconfig);
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
        [Route("Register")]
        public async Task<IHttpActionResult> Build()
        {
            FileContentInfo byteArray = await Task<FileContentInfo>.Run(() => BuildHTML( Request.GetQueryNameValuePairs()));
            if (byteArray == null)
                return this.NotFound();
            return new FileActionResult(false, "", byteArray.Extension, byteArray.Content);
        }

    }
}