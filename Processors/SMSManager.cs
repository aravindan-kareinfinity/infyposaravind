using Newtonsoft.Json;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace InfyPOS.Services
{
    public class SMSManager
    {
        private static SMSManager instance;
        public static SMSManager Instance
        {
            get
            {
                if (instance == null)
                    instance = new SMSManager();

                return instance;
            }
        }
        public SMSManager()
        {
            
        }
        public bool HasSent(long organizationid,InfyPOS.Models.Smshistory smshistory)
        {
            var dataService = new Data.SmshistoryDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            return dataService.HasSent(smshistory);
        }
        public bool SendSMS(long organizationid, string[] mobile, string msg)
        {
            foreach (string no in mobile)
            {
                if (!string.IsNullOrEmpty(no) && no.Length >= 10)
                {
                    if (!this.SendSMS(organizationid, no, msg)) return false;
                }
            }
            return true;
        }

        public bool SendInlineSMS(long organizationid, string mobile, string msg)
        {
            if (!string.IsNullOrEmpty(mobile) && mobile.Length >= 10)
            {

                bool success = false;
                foreach (var mobileitem in GetMobileNoList(organizationid, mobile))
                {
                    string url = msg.Replace("{Mobile}", "{0}");
                    url = url.Replace("{mobile}", "{0}");
                    url = url.Replace("{MOBILE}", "{0}");

                    string result = HttpGetAsync(organizationid, url).Result;

                    success = result != null && result.Contains("Sent");
                }
                //if (!success)
                //{
                //    WebAPI.Common.Logger.Instance(Context.OrganizationID).Log("", WebAPI.Common.Logger.LogType.Error,
                //        string.Format("SendSMS : Failed msg \r\n{0}", body));
                //}
                return success;
            }
            return true;
        }
        public bool SendSMS(long organizationid, string mobile, string msg)
        {
            if (msg.StartsWith("http")) return SendInlineSMS(organizationid, mobile, msg);
            if (string.IsNullOrEmpty(weburl(organizationid))) return false;
            if (!string.IsNullOrEmpty(mobile) && mobile.Length >= 10)
            {

                msg = SMSEscape(msg);
                bool success = false;
                foreach (var mobileitem in GetMobileNoList(organizationid, mobile))
                {
                    string url = weburl(organizationid).Replace("{Mobile}", "{0}").Replace("{Message}", "{1}").Replace("{SenderID}", "{2}");
                    url = url.Replace("{mobile}", "{0}").Replace("{message}", "{1}").Replace("{senderid}", "{2}");
                    url = url.Replace("{MOBILE}", "{0}").Replace("{MESSAGE}", "{1}").Replace("{SENDERID}", "{2}");

                    string body = string.Format(url, System.Web.HttpUtility.UrlPathEncode(mobileitem),
                        System.Web.HttpUtility.UrlPathEncode(msg.Replace("\n", "").Replace("\r", "")));
                    string result = HttpGetAsync(organizationid, body).Result;

                    success = result != null && result.Contains("Sent");
                }
                //if (!success)
                //{
                //    WebAPI.Common.Logger.Instance(Context.OrganizationID).Log("", WebAPI.Common.Logger.LogType.Error,
                //        string.Format("SendSMS : Failed msg \r\n{0}", body));
                //}
                return success;
            }
            return true;
        }

        public string SendSMSURLSchedule(long organizationid,long locationid, string url, string mobile, string msg)
        {
            if (string.IsNullOrEmpty(url) && string.IsNullOrEmpty(weburl(organizationid, locationid))) return "";
            if (!string.IsNullOrEmpty(mobile) && mobile.Length >= 10)
            {
                string senderid = "";
                if (locationid > 0 && string.IsNullOrEmpty(url) &&
                    InfyPOS.Services.ConfigManager.Instance(organizationid).LocationConfig.ContainsKey(locationid))
                {
                    if (!string.IsNullOrEmpty(InfyPOS.Services.ConfigManager.Instance(organizationid).LocationConfig[locationid].sms_tran_senderid))
                        senderid = InfyPOS.Services.ConfigManager.Instance(organizationid).LocationConfig[locationid].sms_tran_senderid;
                }
                msg = SMSEscape(msg);
                string result = "";
                foreach (var mobileitem in GetMobileNoList(organizationid, mobile))
                {
                    url = url.Replace("{Mobile}", "{0}").Replace("{Message}", "{1}").Replace("{SenderID}", "{2}");
                    url = url.Replace("{mobile}", "{0}").Replace("{message}", "{1}").Replace("{senderid}", "{2}");
                    url = url.Replace("{MOBILE}", "{0}").Replace("{MESSAGE}", "{1}").Replace("{SENDERID}", "{2}");
                    url = url.Replace("[MOBILE]", "{0}").Replace("[MESSAGE]", "{1}").Replace("[SENDERID]", "{2}");

                    //string body = string.Format(url, System.Web.HttpUtility.UrlPathEncode(mobileitem),
                    //    System.Web.HttpUtility.UrlPathEncode(msg.Replace("\n", "").Replace("\r", "")), senderid);

                    string body = string.Format(url, System.Web.HttpUtility.UrlPathEncode(mobileitem),
                    System.Web.HttpUtility.UrlPathEncode(msg), senderid);

                    result += HttpGetAsync(organizationid, body).Result;
                }
                return result;
            }
            return "";
        }
        public string SendSMSSchedule(long organizationid, long locationid, string senderid, string mobile, string msg)
        {
            if (string.IsNullOrEmpty(weburl(organizationid, locationid))) return "";
            if (!string.IsNullOrEmpty(mobile) && mobile.Length >= 10)
            {

                if (locationid > 0 &&
                    InfyPOS.Services.ConfigManager.Instance(organizationid).LocationConfig.ContainsKey(locationid))
                {
                    if (!string.IsNullOrEmpty(InfyPOS.Services.ConfigManager.Instance(organizationid).LocationConfig[locationid].sms_tran_senderid))
                        senderid = InfyPOS.Services.ConfigManager.Instance(organizationid).LocationConfig[locationid].sms_tran_senderid;
                }
                msg = SMSEscape(msg);
                string result = "";
                foreach (var mobileitem in GetMobileNoList(organizationid, mobile))
                {
                    string url = weburl(organizationid, locationid).Replace("{Mobile}", "{0}").Replace("{Message}", "{1}").Replace("{SenderID}", "{2}");
                    url = url.Replace("{mobile}", "{0}").Replace("{message}", "{1}").Replace("{senderid}", "{2}");
                    url = url.Replace("{MOBILE}", "{0}").Replace("{MESSAGE}", "{1}").Replace("{SENDERID}", "{2}");
                    url = url.Replace("[MOBILE]", "{0}").Replace("[MESSAGE]", "{1}").Replace("[SENDERID]", "{2}");

                    string body = string.Format(url, System.Web.HttpUtility.UrlPathEncode(mobileitem),
                        System.Web.HttpUtility.UrlPathEncode(msg.Replace("\n", "").Replace("\r","")), senderid);
                    result += HttpGetAsync(organizationid, body).Result;
                }
                return result;
            }
            return "";
        }
        public bool SendSMS(long organizationid, string senderid, string mobile, string msg)
        {
            if (string.IsNullOrEmpty(weburl(organizationid))) return false;
            if (!string.IsNullOrEmpty(mobile) && mobile.Length >= 10)
            {
                msg = SMSEscape(msg);
                bool success = false;
                foreach (var mobileitem in GetMobileNoList(organizationid, mobile))
                {
                    string url = weburl(organizationid).Replace("{Mobile}", "{0}").Replace("{Message}", "{1}").Replace("{SenderID}", "{2}");
                    url = url.Replace("{mobile}", "{0}").Replace("{message}", "{1}").Replace("{senderid}", "{2}");
                    url = url.Replace("{MOBILE}", "{0}").Replace("{MESSAGE}", "{1}").Replace("{SENDERID}", "{2}");

                    string body = string.Format(url, System.Web.HttpUtility.UrlPathEncode(mobileitem),
                        System.Web.HttpUtility.UrlPathEncode(msg.Replace("\n", "").Replace("\r", "")), senderid);
                    string result = HttpGetAsync(organizationid, body).Result;

                    success = result != null && result.Contains("Sent");
                }
                //if (!success)
                //{
                //    WebAPI.Common.Logger.Instance(Context.OrganizationID).Log("", WebAPI.Common.Logger.LogType.Error,
                //        string.Format("SendSMS : Failed msg \r\n{0}", body));
                //}
                return success;
            }
            return true;
        }

        string _weburl = "";
        //@"http://sms.finddeal.in/sendsms/?token=005f908b6519b0e746590ebe5271a298&credit=2&sender=QUANTO&message={Message}&number={Mobile}";

        public string weburl(long organizationid, long locationid)
        {

            if (locationid > 0 &&
                InfyPOS.Services.ConfigManager.Instance(organizationid).LocationConfig.ContainsKey(locationid))
            {
                if (!string.IsNullOrEmpty(InfyPOS.Services.ConfigManager.Instance(organizationid).LocationConfig[locationid].sms_url))
                    return InfyPOS.Services.ConfigManager.Instance(organizationid).LocationConfig[locationid].sms_url;
            }

            if (string.IsNullOrEmpty(InfyPOS.Services.ConfigManager.Instance(organizationid).Config.sms_url))
                return _weburl;
            return InfyPOS.Services.ConfigManager.Instance(organizationid).Config.sms_url;
        }
        public string weburl(long organizationid)
        {
            if (string.IsNullOrEmpty(InfyPOS.Services.ConfigManager.Instance(organizationid).Config.sms_url))
                return _weburl;
            return InfyPOS.Services.ConfigManager.Instance(organizationid).Config.sms_url;
        }

        public string[] GetMobileNoList(long orgaizationid, string source)
        {
            string[] _MobileNo = null;
            if (sms_multiple_no(orgaizationid))
                _MobileNo = source.Split(',');
            else
                _MobileNo = new string[] { source };
            return _MobileNo;
        }
        public bool sms_multiple_no(long organizationid)
        {
            return InfyPOS.Services.ConfigManager.Instance(organizationid).Config.sms_multiple_no;

        }
        public bool QueueSMSMessage(InfyPOS.Models.Smsmessage message)
        {
            InfyPOS.Models.SMSQueue smsqueue = new InfyPOS.Models.SMSQueue();
            smsqueue.id = message.id;
            smsqueue.isdirect = message.isdirect;
            smsqueue.message = message.message;
            smsqueue.arguments = message.parameter;
            smsqueue.senderid = message.smsconfig.senderid;
            smsqueue.sendmethod = message.smsconfig.action;
            smsqueue.smsserverurl = message.smsconfig.url;
            smsqueue.smsconfirmation = message.smsconfig.confirmation;
            smsqueue.items = new List<InfyPOS.Models.SMSQueue.Item>();
            foreach (var item in message.publishto)
            {
                InfyPOS.Models.SMSQueue.Item smsitem = new InfyPOS.Models.SMSQueue.Item();
                smsitem.arguments = new List<InfyPOS.Models.KeyVal>();
                smsitem.arguments.Add(new InfyPOS.Models.KeyVal()
                {
                    key = "[CUSTOMERNAME]",
                    value = item.name
                });
                smsitem.arguments.Add(new InfyPOS.Models.KeyVal()
                {
                    key = "[DATE]",
                    value = DateTime.Now.ToString("dd/MMM/yyyy")
                });
                smsitem.mobile = item.mobileno;
                smsqueue.items.Add(smsitem);
            }

            var returnvalue = Task<bool>.Run(() => QueueSMSMessage(smsqueue));
            returnvalue.Wait();
            return returnvalue.Result;
        }

        public async Task<bool> QueueSMSMessage(InfyPOS.Models.SMSQueue queue)
        {
            string SMSURI = System.Configuration.ConfigurationManager.AppSettings["SMSURI"];
            using (var client = new HttpClient())
            {
                var serializedProduct = JsonConvert.SerializeObject(queue);
                var content = new StringContent(serializedProduct, Encoding.UTF8, "application/json");

                var result = Task.Run(() => client.PostAsync(SMSURI, content)).Result;
                if (result.IsSuccessStatusCode)
                {
                    string json = await result.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<bool>(json);
                }
            }
            return false;
        }

        public bool SendByWebDirect(long organizationid, InfyPOS.Models.Smsmessage smsmessage)
        {
            smsmessage.message = SMSEscape(smsmessage.message);
            string MobileNo = "";
            string body = "";
            try
            {

                foreach (InfyPOS.Models.Publishto item in smsmessage.publishto)
                {
                    if (!string.IsNullOrEmpty(item.mobileno) && item.mobileno.Length >= 10)
                    {
                        MobileNo = item.mobileno;
                        string message = smsmessage.message;

                        message = (message.IndexOf("[CUSTOMERNAME]") >= 0) ? message.Replace("[CUSTOMERNAME]", item.name) : message;
                        message = (message.IndexOf("[DATE]") >= 0) ? message.Replace("[DATE]", DateTime.Now.ToString("dd/MMM/yyyy")) : message;


                        foreach (var mobileitem in GetMobileNoList(organizationid, MobileNo))
                        {
                            string url = weburl(organizationid).Replace("{Mobile}", "{0}").Replace("{Message}", "{1}");
                            body = string.Format(url, System.Web.HttpUtility.UrlPathEncode(mobileitem),
                                System.Web.HttpUtility.UrlPathEncode(message));
                            string result = HttpGetAsync(organizationid, body).Result;

                            bool success = result != null && result.Contains("Sent");
                            if (!success)
                            {
                                WebAPI.Common.Logger.Instance(organizationid).Log("", WebAPI.Common.Logger.LogType.Error,
                                    string.Format("SendByWeb : Failed msg \r\n{0}", body));
                            }
                        }
                    }
                }
                return true;
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(organizationid).Log(exp, WebAPI.Common.Logger.LogType.Error,
                    string.Format("SendByWeb : Error {0} to {1} - {2}", smsmessage.message, MobileNo, exp.Message));
                return false;
            }
        }
        private string SMSEscape(string message)
        {
            message = (message.IndexOf("%") >= 0) ? message.Replace("%", "%25") : message;
            message = (message.IndexOf("&") >= 0) ? message.Replace("&", "%26") : message;
            message = (message.IndexOf("?") >= 0) ? message.Replace("?", "%3F") : message;
            message = (message.IndexOf("\"") >= 0) ? message.Replace("\"", "%22") : message;
            message = (message.IndexOf("<") >= 0) ? message.Replace("<", "%3C") : message;
            message = (message.IndexOf(">") >= 0) ? message.Replace(">", "%3E") : message;
            message = (message.IndexOf("+") >= 0) ? message.Replace("+", "%2B") : message;
            message = (message.IndexOf("#") >= 0) ? message.Replace("#", "%23") : message;
            message = (message.IndexOf("*") >= 0) ? message.Replace("*", "%2A") : message;
            message = (message.IndexOf("!") >= 0) ? message.Replace("!", "%21") : message;
            //message = (message.IndexOf("'") >= 0) ? message.Replace("'", "%2C") : message;
            message = (message.IndexOf("=") >= 0) ? message.Replace("=", "%3D") : message;

            return message;
        }
        private async Task<string> HttpGetAsync(long organizationid, string URI)
        {
            try
            {

                using (var client = new HttpClient())
                {
                    var result = Task.Run(() => client.GetAsync(URI, HttpCompletionOption.ResponseContentRead));
                    return await result.Result.Content.ReadAsStringAsync();
                }
                //System.Net.WebRequest req = System.Net.WebRequest.Create(URI);
                //System.Net.WebResponse resp = req.GetResponse();
                //System.IO.StreamReader sr = new System.IO.StreamReader(resp.GetResponseStream());
                //return sr.ReadToEnd().Trim();
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(organizationid).Log(exp, WebAPI.Common.Logger.LogType.Error, URI);
            }
            return "FALSE";
        }
    }
}