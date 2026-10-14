using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace InfyPOS.Processors
{
    public class ScheduledReports
    {
        private long organizationid;
        public ScheduledReports(long organizationid)
        {
            this.organizationid = organizationid;
            if (scheduleconfigs == null)
            {
                LoadConfiguration();
            }
        }
        private static System.Collections.Concurrent.ConcurrentDictionary<long, ScheduledReports> instanceList = new System.Collections.Concurrent.ConcurrentDictionary<long, ScheduledReports>();
        public static ScheduledReports Instance(long organizationid)
        {
            ScheduledReports instance = null;
            if (instanceList.TryGetValue(organizationid, out instance))
                return instance;

            instance = new ScheduledReports(organizationid);
            if (!instanceList.TryAdd(organizationid, instance))
                instanceList.TryGetValue(organizationid, out instance);
            return instance;
        }

        SortedDictionary<long, Models.Schedule> scheduleconfigs = null;
        List<Models.Scheduleditems> scheduleditems = null;

        private void LoadConfiguration()
        {
            scheduleconfigs = new SortedDictionary<long, Models.Schedule>();
            Data.ScheduleDataService scheduleDataService = new Data.ScheduleDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            var list = scheduleDataService.Search(new Models.ScheduleCreteria()
            {
                onlyreports = true
            });
            foreach (var item in list)
            {
                if (!scheduleconfigs.ContainsKey(item.id))
                {
                    scheduleconfigs.Add(item.id, item);
                }
                else
                {
                    scheduleconfigs[item.id] = item;
                }
            }

            Data.ScheduleditemsDataService scheduleditemsDataService = new Data.ScheduleditemsDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            scheduleditems = scheduleditemsDataService.SearchAll(-1);
        }

        List<ScheduleManager.ScheduleableTask> alltasklist = null;
        public Tuple<bool, string> Run()
        {
            return Run(false,0);
        }

        public Tuple<bool, string> Run(long scheduleid, string id)
        {
            Data.ScheduleDataService scheduleDataService = new Data.ScheduleDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            var list = scheduleDataService.Search(new Models.ScheduleCreteria()
            {
                id = scheduleid
            });
            if (list.Count == 1)
            {
                list[0].defaultSourceID = id;
                return RenderReport(list[0].name, list[0]);
            }
            return new Tuple<bool, string>(false, "Not found");
        }

        public Tuple<bool, string> RenderReport(string uniquename, Models.Schedule source)
        {

            if (alltasklist == null)
                alltasklist = ScheduleManager.Instance.TaskList();
            List<InfyPOS.Models.Smshistory> smshistories = new List<Models.Smshistory>();
            Exception lastexp = null;
            if (alltasklist.Exists(e => e.name == uniquename))
            {
                var reportEngine = alltasklist.Find(e => e.name == uniquename).GetReportEngine();
                reportEngine.Source = source;
                if (alltasklist.Find(e => e.name == uniquename).multipleresult)
                {
                    var reportResult = reportEngine.RenderMultiple(organizationid);
                    if (reportEngine.Source.config.output == "SMS")
                    {
                        try
                        {
                            List<ScheduleManager.IReportExecutor.SMSDetails> listofsmsitems = Processors.ScheduleManager.Instance.ValidateSMS(organizationid, reportResult, reportEngine);
                            foreach (var smsitem in listofsmsitems)
                            {
                                smshistories.Add(Processors.ScheduleManager.Instance.SendSMS(organizationid,
                                    0, reportEngine.Source, smsitem.content,
                                    smsitem.mobileno, null));
                            }
                        }
                        catch (Exception exp)
                        {
                            lastexp = exp;
                        }

                    }
                    else
                    {
                        foreach (var smsitem in reportResult)
                        {
                            Processors.ScheduleManager.Instance.SendEmail(reportEngine.Source, false, smsitem.content, reportEngine.Source.description,
                                smsitem.mailid);
                        }
                    }
                }
                else
                {
                    if (reportEngine.Source.config.output == "SMS")
                    {

                        if (reportEngine.Source.config.parameter == "Customer DOB"
                            || reportEngine.Source.config.parameter == "Customer Payment Reminder"
                            || reportEngine.Source.config.parameter == "Customer Kids DOB"
                            || reportEngine.Source.config.parameter == "Customer Anniversary"
                            || reportEngine.Source.config.parameter == "Approved Bill Reminder")
                        {
                            var reportResult = reportEngine.RenderMultiple(organizationid);

                            try
                            {
                                List<ScheduleManager.IReportExecutor.SMSDetails> listofsmsitems = Processors.ScheduleManager.Instance.ValidateSMS(organizationid, reportResult, reportEngine);
                                if (listofsmsitems != null)
                                {
                                    foreach (var smsitem in listofsmsitems)
                                    {
                                        smshistories.Add(Processors.ScheduleManager.Instance.SendSMS(organizationid, 0, reportEngine.Source, smsitem.content,
                                            smsitem.mobileno, smsitem.GetReplacements()));
                                    }
                                }
                            }
                            catch (Exception exp)
                            {
                                lastexp = exp;
                            }
                        }
                        else
                        {
                            if (reportEngine.Source.config.parameter == "Current Day" && reportEngine.Source.Scheduleditems != null && reportEngine.Source.Scheduleditems.Count > 0)
                            {
                                Data.ScheduleditemsDataService scheduleDataService = new Data.ScheduleditemsDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
                                var list = scheduleDataService.Search(new Models.ScheduleditemsCreteria()
                                {
                                    id = reportEngine.Source.Scheduleditems[0].id
                                });
                                if (list != null && list.Count > 0)
                                {
                                    if (list[0].validtill == DateTime.MinValue || list[0].validtill < DateTime.Now)
                                    {
                                        string reportResult = reportEngine.RenderReport(organizationid);
                                        smshistories.Add(Processors.ScheduleManager.Instance.SendSMS(organizationid, 0, reportEngine.Source,
                                            reportResult, reportEngine.Source.config.sendto, null));
                                    }
                                }
                                else
                                {
                                    string reportResult = reportEngine.RenderReport(organizationid);
                                    smshistories.Add(Processors.ScheduleManager.Instance.SendSMS(organizationid, 0, reportEngine.Source, reportResult, reportEngine.Source.config.sendto, null));
                                }
                            }
                            else
                            {
                                string reportResult = reportEngine.RenderReport(organizationid);
                                smshistories.Add(Processors.ScheduleManager.Instance.SendSMS(organizationid, 0, reportEngine.Source, reportResult, reportEngine.Source.config.sendto, null));
                            }
                        }
                    }

                    //string html = reportEngine.RenderHTML(organizationid);
                    if (reportEngine.Source.config.output == "PDFLink")
                    {
                        string urllink = reportEngine.RenderPDFReport(organizationid);
                        string smscontent = reportEngine.Source.description + " - " + DateTime.Now.ToString("dd-MM-yyyy") + "\r\n";
                        smscontent += urllink;
                        smshistories.Add(Processors.ScheduleManager.Instance.SendSMS(organizationid, 0, reportEngine.Source, smscontent, reportEngine.Source.config.sendto, null));
                    }
                    if (reportEngine.Source.config.output == "Email")
                    {
                        var pdffile = reportEngine.RenderPDFFile(organizationid);
                        pdffile.ext = "pdf";
                        string smscontent = reportEngine.Source.description + " - " + DateTime.Now.ToString("dd-MM-yyyy");
                        Processors.ScheduleManager.Instance.SendSMTPMail(reportEngine.Source, smscontent, pdffile, reportEngine.Source.config.sendto);
                    }
                    if (reportEngine.Source.config.output == "WebAPI" || reportEngine.Source.config.output == "Growaasan")
                    {
                        //try
                        //{
                        //    string json = reportEngine.RenderHTML(organizationid);
                        //    Processors.ScheduleManager.Instance.Send2WebAPIAsync(reportEngine.Source, json);
                        //}
                        //catch (Exception exp)
                        //{
                        Processors.Schedule_Actions.Instance.EnqueScheduleItem(organizationid, reportEngine);
                        //}
                    }
                    if (reportEngine.Source.config.output == "EmailContent")
                    {
                        string html = reportEngine.RenderHTML(organizationid);
                        smshistories.Add(Processors.ScheduleManager.Instance.Create(organizationid, reportEngine.Source,
                            html, reportEngine.Source.config.sendto, reportEngine.DateTimeNow));
                        Processors.ScheduleManager.Instance.SendEmail(reportEngine.Source, true, html, reportEngine.Source.description + " - " + DateTime.Now.ToString("dd-MM-yyyy"), reportEngine.Source.config.sendto);
                    }
                    if (reportEngine.Source.config.output == "PDFMail")
                    {
                        var pdffile = reportEngine.RenderPDFFile(organizationid);
                        pdffile.ext = "pdf";
                        string smscontent = reportEngine.Source.description + " - " + DateTime.Now.ToString("dd-MM-yyyy") + "\r\n";
                        smshistories.Add(Processors.ScheduleManager.Instance.Create(organizationid, reportEngine.Source,
                            smscontent, reportEngine.Source.config.sendto, reportEngine.DateTimeNow));
                        Processors.ScheduleManager.Instance.SendSMTPMail(reportEngine.Source, smscontent, pdffile, reportEngine.Source.config.sendto);
                    }
                    if (reportEngine.Source.config.output == "ExcelMail")
                    {
                        var pdffile = reportEngine.RenderExcelFile(organizationid);
                        pdffile.ext = "xlsx";
                        string smscontent = reportEngine.Source.description + " - " + DateTime.Now.ToString("dd-MM-yyyy") + "\r\n";
                        smshistories.Add(Processors.ScheduleManager.Instance.Create(organizationid, reportEngine.Source,
                            smscontent, reportEngine.Source.config.sendto, reportEngine.DateTimeNow));
                        Processors.ScheduleManager.Instance.SendSMTPMail(reportEngine.Source, smscontent, pdffile, reportEngine.Source.config.sendto);
                    }
                }

                if (smshistories.Count > 0)
                {
                    InfyPOS.Data.SmshistoryDataService smshistoryDataService = new Data.SmshistoryDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
                    smshistoryDataService.InsertAll(smshistories);
                }
                if (lastexp != null)
                    throw lastexp;
                return new Tuple<bool, string>(true, createresponsehistory(smshistories));
            }

            return new Tuple<bool, string>(false, createresponsehistory(smshistories));
        }

        private string createresponsehistory(List<Models.Smshistory> smshistories)
        {
            System.Text.StringBuilder response = new System.Text.StringBuilder();
            foreach (var item in smshistories)
                response.AppendLine(item.message);
            return response.ToString();
        }


        public Tuple<bool, string> Run(bool reloadconfig, long scheduleid)
        {
            List<string> response = new List<string>();
            if (reloadconfig || scheduleditems == null)
            {
                LoadConfiguration();
            }
            if (scheduleditems != null)
            {
                if (reloadconfig && scheduleid > 0 && scheduleconfigs.ContainsKey(scheduleid))
                {
                    var report = RenderReport(scheduleconfigs[scheduleid].name, scheduleconfigs[scheduleid]);
                    response.Add(report.Item2);
                }
                else
                {
                    var filterreports = scheduleditems.FindAll(e => e.lastrun < DateTime.Now.Date && e.hour <= DateTime.Now.Hour);
                    foreach (var report in filterreports)
                    {
                        if (scheduleconfigs.ContainsKey(report.scheduleid))
                        {
                            var uniquename = scheduleconfigs[report.scheduleid].name;
                            var result = RenderReport(scheduleconfigs[report.scheduleid].name, scheduleconfigs[report.scheduleid]);
                            if (result.Item1)
                            {
                                UpdateScheduleItem(report);
                                response.Add(result.Item2);
                            }
                        }
                    }
                }
            }
            return new Tuple<bool, string>(true, string.Join("\r\n", response));
        }

        public void UpdateScheduleItem(Models.Scheduleditems source)
        {
            DateTime lastrun = DateTime.Now;
            var param = new SortedList<string, object>();
            param.Add("lastrun", lastrun);
            param.Add("id", source.id);
            param.Add("validtill", lastrun.AddMinutes(45));
            Common.BaseData baseData = new Common.BaseData() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            if (baseData.ExecuteQuery("Update Scheduleditems set lastrun=@lastrun, validtill=@validtill where id=@id", param) > 0)
            {
                source.lastrun = lastrun;
                source.validtill = lastrun.AddMinutes(45);
            }
        }

        internal void ScheduleChanged(long id)
        {
            if (scheduleconfigs != null)
            {
                if (scheduleditems != null)
                    scheduleditems.RemoveAll(e => e.scheduleid == id);

                Data.ScheduleDataService scheduleDataService = new Data.ScheduleDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
                var list = scheduleDataService.Search(new Models.ScheduleCreteria()
                {
                    id = id
                });

                if (list.Count == 1 && !list[0].isactionbased)
                {
                    var item = list[0];
                    if (!scheduleconfigs.ContainsKey(item.id))
                    {
                        scheduleconfigs.Add(item.id, item);
                    }
                    else
                    {
                        scheduleconfigs[item.id] = item;
                    }

                    if (scheduleditems != null)
                        scheduleditems.AddRange(list[0].Scheduleditems);
                }
            }
        }
    }
}