using InfyPOS.Common;
using InfyPOS.Data;
using InfyPOS.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Mail;
using System.Threading;

namespace InfyPOS.Processors
{
    public class Schedule_Actions
    {
        public bool logenabled { get; set; }
        public bool enabled { get; set; }
        System.Threading.Thread delaythread;
        public MessageQueue<QueueItem> messageQueue;
        public Schedule_Actions()
        {
            if (!string.IsNullOrEmpty(System.Configuration.ConfigurationManager.AppSettings["SupportScheduler"])
                && System.Configuration.ConfigurationManager.AppSettings["SupportScheduler"].ToLower() == "true")
            {
                messageQueue = new MessageQueue<QueueItem>(int.MaxValue);
                delaythread = new System.Threading.Thread(new System.Threading.ThreadStart(Processor));
                delaythread.Start();
                enabled = true;
                logenabled = !string.IsNullOrEmpty(System.Configuration.ConfigurationManager.AppSettings["LogScheduler"])
                && System.Configuration.ConfigurationManager.AppSettings["LogScheduler"].ToLower() == "true";
                WebAPI.Common.Logger.Instance(0).Log("Action Scheduler Log " + logenabled, WebAPI.Common.Logger.LogType.Error, System.Configuration.ConfigurationManager.AppSettings["LogScheduler"]);
                WebAPI.Common.Logger.Instance(0).Log("Action Scheduler Enabled", WebAPI.Common.Logger.LogType.Error, System.Configuration.ConfigurationManager.AppSettings["SupportScheduler"]);
            }
            else
            {
                WebAPI.Common.Logger.Instance(0).Log("Action Scheduler Disabled", WebAPI.Common.Logger.LogType.Error, System.Configuration.ConfigurationManager.AppSettings["SupportScheduler"]);
            }
        }
        public class MessageQueue<T>
        {
            readonly int _Size = 0;
            readonly Queue<T> _Queue = new Queue<T>();
            readonly object _Key = new object();
            bool _Quit = false;
            private static MessageQueue<T> tradeQueue = null;

            public MessageQueue(int size)
            {
                _Size = size;
            }

            public static MessageQueue<T> Current
            {
                get
                {
                    if (tradeQueue != null) return tradeQueue;
                    int maxqueuecount = maxqueuecount = 500000;
                    tradeQueue = new MessageQueue<T>(maxqueuecount);
                    return tradeQueue;
                }
            }

            public T[] GetAll()
            {
                return _Queue.ToArray();
            }
            public void Quit()
            {
                lock (_Key)
                {
                    _Quit = true;

                    Monitor.PulseAll(_Key);
                }
            }

            public bool Enqueue(T t)
            {
                lock (_Key)
                {
                    while (!_Quit && _Queue.Count >= _Size)
                    {
                        Monitor.Wait(_Key);
                    }

                    if (_Quit) return false;

                    _Queue.Enqueue(t);

                    Monitor.PulseAll(_Key);
                }

                return true;
            }

            public bool Dequeue(out T t)
            {
                t = default(T);

                lock (_Key)
                {
                    while (!_Quit && _Queue.Count == 0) Monitor.Wait(_Key);

                    if (_Queue.Count == 0) return false;

                    t = _Queue.Dequeue();

                    Monitor.PulseAll(_Key);
                }

                return true;
            }
        }

        public class QueueItem
        {
            public long organizationid { get; set; }
            public Actions action { get; set; }
            public Object source { get; set; }
            public SourceType type { get; set; }
            public Schedule schedule { get; set; }
        }

        public enum SourceType
        {
            Bill,
            Customer,
            PurchaseReturn,
            PurchaseOrder,
            Employee,
            DayClose,
            OTP,
            BOMPurchaseReturn,
            Stock,
            System,
            Alteration,
            SupplierPayment,
            Settlement
        }
        public enum Actions
        {
            New,
            Updated,
            Deleted,
            Cancelled,
            Return,
            Discounted,
            Exchange,
            CardPayment,
            Credit,
            DayClose,
            Publish,
            Otp_Loyelty,
            Otp_Loyelty_Validate,
            Otp_Discount,
            Otp_Credit,
            CRM,
            CRM_Loyelty,
            CRM_Voucher,
            CRM_Campaign,
            ECommerce,
            System_customerotp,
            System_approvalotp,
            System_approval,
            AlterationReceived,
            AlterationReady,
            AlterationDelivered,
            Settlement
        }

        private static Schedule_Actions instance;
        public static Schedule_Actions Instance
        {
            get
            {
                return instance;
            }
        }

        public void Processor()
        {
            System.Threading.Thread.Sleep(10000);
            QueueItem entity;
            while (messageQueue.Dequeue(out entity))
            {
                ExecuteQueueItem(entity.organizationid, entity);
            }
        }
        static Schedule_Actions()
        {
            instance = new Schedule_Actions();
        }

        //public bool TriggerSchedule(long organizationid,
        //    Dictionary<string, string> source,Actions actions, SourceType type)
        //{
        //    if (!enabled) return false;

        //    if (!Instance.HasConfig(organizationid,
        //           type, actions)) return false;
        //    messageQueue.Enqueue(new QueueItem()
        //    {
        //        source = source,
        //        action = actions,
        //        type = type,
        //        organizationid = organizationid
        //    });
        //    return true;
        //}
        public void OnChange(long organizationid, Models.BillOTP source, Actions actions)
        {
            if (!enabled) return;
            messageQueue.Enqueue(new QueueItem()
            {
                source = source,
                action = actions,
                type = SourceType.OTP,
                organizationid = organizationid
            });
        }

        public void OnChange(long organizationid, Models.Billsettlementmaster source, Actions actions)
        {
            if (!enabled) return;
            messageQueue.Enqueue(new QueueItem()
            {
                source = source,
                action = actions,
                type = SourceType.Settlement,
                organizationid = organizationid
            });
        }


        internal void EnqueScheduleItem(long organizationid,
            ScheduleManager.IReportExecutor reportEngine)
        {
            if (!enabled) return;

            var reportsource = reportEngine.Run(organizationid, reportEngine.Source.config.tzoffset);
            if (reportEngine.Source.config.output == "WebAPI")
            {
                if (reportsource.SMSList != null)
                {
                    var currenturl = reportEngine.Source.config.url;
                    try
                    {
                        foreach (var item in reportsource.SMSList)
                        {
                            var json = item.Convert2JSON(reportEngine.Source.config.format);
                            reportEngine.Source.config.url = item.Convert2JSON(currenturl);
                            var response = ScheduleManager.Instance.Send2WebAPIAsync(reportEngine.Source, json);
                            SmshistoryDataService smshistoryDataService = new SmshistoryDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
                            smshistoryDataService.Insert(response.Result);
                        }
                    }
                    catch (Exception exp)
                    {

                    }
                    reportEngine.Source.config.url = currenturl;
                }
            }
            else if (reportEngine.Source.config.output == "Growaasan")
            {
                if (reportsource.SMSList != null)
                    foreach (var smsitem in reportsource.SMSList)
                    {
                        var json = smsitem.Convert2JSON(reportEngine.Source.config.format);
                        List<KeyValuePair<string, string>> list = new List<KeyValuePair<string, string>>();
                        var jsonobject = Newtonsoft.Json.Linq.JObject.Parse(json);
                        SortedList<string, string> attachments = new SortedList<string, string>();
                        foreach (var item in jsonobject.Children())
                        {
                            var key = item.First().Path;
                            var value = ((Newtonsoft.Json.Linq.JValue)item.First()).Value.ToString();
                            if (value.StartsWith("ATTACHMENT"))
                            {
                                var filepart = value.Split(new char[] { '-' });
                                int idx = 0;
                                if (int.TryParse(filepart[1], out idx))
                                {
                                    attachments.Add(key, "file-" + idx);
                                }
                            }
                            else
                            {
                                list.Add(new KeyValuePair<string, string>(key, value));
                            }
                        }

                        using (var client = new HttpClient())
                        {
                            var httprequest = new HttpRequestMessage(reportEngine.Source.config.growaasan.httpmode.ToUpper() == "POST" ? HttpMethod.Post : HttpMethod.Get,
                                reportEngine.Source.config.url);
                            var content = new MultipartFormDataContent();

                            foreach (var stringitem in list)
                            {
                                if (string.IsNullOrEmpty(stringitem.Value) || stringitem.Value.Trim() == "0")
                                    content.Add(new StringContent("-"), stringitem.Key);
                                else
                                    content.Add(new StringContent(stringitem.Value), stringitem.Key);
                            }


                            httprequest.Content = content;

                            try
                            {
                                var response = System.Threading.Tasks.Task.Run(() => client.SendAsync(httprequest)).Result;
                                var responsestring = response.Content.ReadAsStringAsync().Result;
                                SmshistoryDataService smshistoryDataService = new SmshistoryDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
                                smshistoryDataService.Insert(new Smshistory()
                                {
                                    actionon = DateTime.Now,
                                    mobileno = smsitem.mobileno,
                                    message = JsonConvert.SerializeObject(attachments),
                                    response = responsestring,
                                    scheduleid = reportEngine.Source.id,
                                    config = json
                                });
                            }
                            catch (Exception exp)
                            {
                                SmshistoryDataService smshistoryDataService = new SmshistoryDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
                                smshistoryDataService.Insert(new Smshistory()
                                {
                                    actionon = DateTime.Now,
                                    mobileno = smsitem.mobileno,
                                    message = JsonConvert.SerializeObject(attachments),
                                    response = exp.Message,
                                    scheduleid = reportEngine.Source.id,
                                    config = json
                                });
                            }
                        }
                    }
            }
            else
            {
                if (reportsource.SMSList != null)
                    foreach (var item in reportsource.SMSList)
                    {
                        var json = item.Convert2JSON(reportEngine.Source.config.format);
                        var response = ScheduleManager.Instance.SendSMS(0, 0, reportEngine.Source,
                            "", item.mobileno, item.GetReplacements());
                        SmshistoryDataService smshistoryDataService = new SmshistoryDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
                        smshistoryDataService.Insert(response);
                    }
            }
        }

        public void OnChange(long organizationid, Models.Bill source, Actions actions)
        {
            if (!enabled) return;
            messageQueue.Enqueue(new QueueItem()
            {
                source = source,
                action = actions,
                type = SourceType.Bill,
                organizationid = organizationid
            });
        }

        internal void OnChange(long organizationid, OnlineStock source, Actions actions)
        {
            if (!enabled) return;
            messageQueue.Enqueue(new QueueItem()
            {
                source = source,
                action = actions,
                type = SourceType.Stock,
                organizationid = organizationid
            });
        }

        public void OnChange(long organizationid, Models.Customer source, Schedule schedule, Actions actions)
        {
            if (!enabled) return;
            messageQueue.Enqueue(new QueueItem()
            {
                source = source,
                action = actions,
                type = SourceType.Customer,
                schedule = schedule,
                organizationid = organizationid
            });
        }
        public void OnChange(long organizationid, Models.Customer source, Actions actions)
        {
            if (!enabled) return;
            messageQueue.Enqueue(new QueueItem()
            {
                source = source,
                action = actions,
                type = SourceType.Customer,
                organizationid = organizationid
            });
        }

        public void OnChange(long organizationid, Models.Delivery source, Actions actions)
        {
            if (!enabled) return;
            messageQueue.Enqueue(new QueueItem()
            {
                source = source,
                action = actions,
                type = SourceType.Alteration,
                organizationid = organizationid
            });
        }

        public void OnChange(long organizationid, Models.Employee source, Actions actions)
        {
            if (!enabled) return;
            messageQueue.Enqueue(new QueueItem()
            {
                source = source,
                action = actions,
                type = SourceType.Employee,
                organizationid = organizationid
            });
        }

        public void OnChange(long organizationid, Models.DateConfiguration source, Actions actions)
        {
            if (!enabled) return;
            messageQueue.Enqueue(new QueueItem()
            {
                source = source,
                action = actions,
                type = SourceType.DayClose,
                organizationid = organizationid
            });
            RunDateChangeReport(organizationid, source);
        }

        public void RundDateChange(long organizationid, Models.DateConfiguration source)
        {
            var queueItem = new QueueItem()
            {
                source = source,
                action = Actions.DayClose,
                type = SourceType.DayClose,
                organizationid = organizationid
            };

            var scheduleconfigList = GetSchedule(organizationid, queueItem);
            if (scheduleconfigList != null && scheduleconfigList.Count > 0)
            {
                foreach (var kv in scheduleconfigList)
                {
                    var scheduleconfig = kv.Value;
                    if (scheduleconfig != null && !string.IsNullOrEmpty(scheduleconfig.config.format))
                    {
                        if(scheduleconfig.config != null && 
                            (scheduleconfig.config.locationid == source.locationid || scheduleconfig.config.locationid == 0))
                            ExecuteItem(organizationid, scheduleconfig, queueItem);
                    }
                }
            }

            RunDateChangeReport(organizationid, source);
        }

        public void RunDateChangeReport(long organizationid, Models.DateConfiguration source)
        {
            var scheduleconfigs = new SortedDictionary<string, SortedDictionary<long, Models.Schedule>>();
            Data.ScheduleDataService scheduleDataService = new Data.ScheduleDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            var list = scheduleDataService.Search(new Models.ScheduleCreteria()
            {
                onlyreports = true
            });
            if (source.locationid > 0 && list.Exists(x => x.config != null && (x.config.locationid == source.locationid || x.config.locationid == 0)))
                list = list.FindAll(x => x.config != null && (x.config.locationid == source.locationid || x.config.locationid == 0));

            foreach (var item in list)
            {
                if (!scheduleconfigs.ContainsKey(item.name))
                {
                    scheduleconfigs.Add(item.name, new SortedDictionary<long, Models.Schedule>());
                    scheduleconfigs[item.name].Add(item.id, item);
                }
                else
                {
                    scheduleconfigs[item.name].Add(item.id, item);
                }
            }

            if (scheduleconfigs != null && scheduleconfigs.Count > 0)
            {
                List<Schedule> dayendschedules = new List<Schedule>();
                scheduleconfigs.Values.ToList().ForEach(x =>
                {
                    dayendschedules.AddRange(x.ToList().FindAll(y => y.Value.config != null && y.Value.config.monthly == "DayChange").ConvertAll(z => z.Value));
                });
                foreach (var schedule in dayendschedules)
                {
                    try
                    {
                        ScheduledReports.Instance(organizationid).RenderReport(schedule.name, schedule);
                    }
                    catch (Exception exp)
                    {

                    }
                }
            }
        }

        public void OnChange(long organizationid, Models.Lrinvoicereturn source, Actions actions)
        {
            if (!enabled) return;
            messageQueue.Enqueue(new QueueItem()
            {
                source = source,
                action = actions,
                type = SourceType.PurchaseReturn,
                organizationid = organizationid
            });
        }

        public void OnChange(long organizationid, Models.BOMInvoicereturn source, Actions actions)
        {
            if (!enabled) return;
            messageQueue.Enqueue(new QueueItem()
            {
                source = source,
                action = actions,
                type = SourceType.PurchaseReturn,
                organizationid = organizationid
            });
        }

        public bool HasConfig(long organizationID, SourceType sourcetype, Actions action)
        {
            var scheduleconfigList = GetSchedule(organizationID, new QueueItem()
            {
                action = action,
                type = sourcetype
            });

            if (scheduleconfigList == null || scheduleconfigList.Count == 0) return false;
            return true;
        }

        public void OnChange(long organizationid, Models.Purchaseorder source, Actions actions)
        {
            if (!enabled) return;
            messageQueue.Enqueue(new QueueItem()
            {
                source = source,
                action = actions,
                type = SourceType.PurchaseOrder,
                organizationid = organizationid
            });
        }
        public void OnChange(long organizationid, Models.PurchaseorderPrint source, Actions actions)
        {
            if (!enabled) return;
            messageQueue.Enqueue(new QueueItem()
            {
                source = source,
                action = actions,
                type = SourceType.PurchaseOrder,
                organizationid = organizationid
            });
        }

        public void OnChange(long organizationid, Models.SupplierPaymentPrint source, Actions actions)
        {
            if (!enabled) return;
            messageQueue.Enqueue(new QueueItem()
            {
                source = source,
                action = actions,
                type = SourceType.SupplierPayment,
                organizationid = organizationid
            });
        }

        internal void BillChanged(long id)
        {
            if (!enabled) return;
        }

        internal void ScheduleChanged(long organizationid, long id)
        {
            if (!enabled) return;
            if (scheduleconfigs != null)
            {
                Data.ScheduleDataService scheduleDataService = new Data.ScheduleDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
                var list = scheduleDataService.Search(new Models.ScheduleCreteria()
                {
                    id = id
                });

                if (list.Count == 1 && list[0].isactionbased)
                {
                    var item = list[0];
                    if (!scheduleconfigs.ContainsKey(item.name))
                    {
                        scheduleconfigs.Add(item.name, new SortedDictionary<long, Models.Schedule>());
                        scheduleconfigs[item.name].Add(item.id, item);
                    }
                    else
                    {
                        if (scheduleconfigs[item.name].ContainsKey(item.id))
                            scheduleconfigs[item.name][item.id] = item;
                        else
                            scheduleconfigs[item.name].Add(item.id, item);
                    }
                }
            }
        }

        private void ExecuteWebMailItem(long organizationid, Models.Schedule scheduleconfig, QueueItem queueItem)
        {
            long locationid = 0;
            Common.BaseData baseData = new Common.BaseData() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            ReportLibrary.DefaultPageSettings setting = new ReportLibrary.DefaultPageSettings();
            var format = ReportLibrary.DosFormatParser.ParseXML2HTML(organizationid, scheduleconfig.config.format);



            var result = "";
            string mailid = "";
            long companyid = 0;
            long supplierid = 0;
            long customerid = 0;
            long employeeid = 0;
            long agentid = 0;
            var attachments = new SortedDictionary<string, byte[]>();
            string attachmentname = "file.pdf";
            if (queueItem.type == SourceType.Bill)
            {
                Models.Bill bill = queueItem.source as Models.Bill;
                locationid = bill.locationid;
                if (scheduleconfig.config.companyid > 0 && bill.companyid != scheduleconfig.config.companyid) return;
                if (scheduleconfig.config.locationid > 0 && bill.locationid != scheduleconfig.config.locationid) return;

                if (scheduleconfig.config.parameter == "Discount Greater than")
                {
                    if (scheduleconfig.config.addlndiscountconfig.invalue > 0 &&
                        bill.totaldiscount < scheduleconfig.config.addlndiscountconfig.invalue) return;

                    if (scheduleconfig.config.addlndiscountconfig.inpercentage > 0 &&
                        bill.totaldiscountpercentage < scheduleconfig.config.addlndiscountconfig.inpercentage) return;
                }

                if (scheduleconfig.config.source == "Customer")
                {
                    if (bill.customer != null && !string.IsNullOrEmpty(bill.customer.emailid))
                    {
                        mailid = bill.customer.emailid;
                        CustomerrewardsDataService rewarddate = new CustomerrewardsDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
                        bill.customerreward = rewarddate.SearchReward(new Models.CustomerrewardsCreteria() { customerid = bill.customerid, locationid = bill.locationid }).available;

                        var BillDataService = new BillDataService() { Context = new UserClaim() { OrganizationID = organizationid } };
                        bill.fburl = BillDataService.GetFBURL(bill.billno);
                    }
                }

                if (bill.transport == null &&
                    bill.customer != null &&
                    bill.customer.preferredtransportid > 0)
                {
                    TransportDataService transportDataService = new TransportDataService()
                    {
                        Context = new UserClaim()
                        {
                            OrganizationID = organizationid,
                        }
                    };
                    var transports = transportDataService.Search(new TransportCreteria() { id = bill.customer.preferredtransportid });
                    if (transports != null &&
                        transports.Count > 0)
                    {
                        bill.transport = transports[0];
                        bill.transportname = transports[0].name;
                    }
                }

                if (bill.agent == null && bill.customer != null &&
                    bill.customer.agentid > 0)
                {
                    agentid = bill.customer.agentid;
                    AgentDataService agentDataService = new AgentDataService() { Context = new UserClaim() { OrganizationID = organizationid } };
                    var agents = agentDataService.Search(new AgentCreteria()
                    {
                        id = bill.customer.agentid
                    });
                    if (agents.Count == 1)
                    {
                        bill.agent = agents[0];
                        bill.agentname = agents[0].name;
                    }
                }

                var documentbytes = format.RenderBytes(new Common.ImageData(), bill, bill.Billitems, new SortedDictionary<string, object>(), false);
                result = System.Text.ASCIIEncoding.ASCII.GetString(documentbytes);

                companyid = bill.companyid;
                customerid = bill.customerid;

                if (scheduleconfig.config.source == "Company")
                {
                    if (bill.company != null && !string.IsNullOrEmpty(bill.company.emailid))
                        mailid = bill.company.emailid;
                }
                else if (scheduleconfig.config.source == "Customer")
                {
                    if (bill.customer != null && !string.IsNullOrEmpty(bill.customer.emailid))
                        mailid = bill.customer.emailid;
                }
                else if (scheduleconfig.config.source == "Agent" &&
                    bill.customer != null &&
                    bill.customer.agentid > 0)
                {
                    if (bill.agent == null)
                    {
                        AgentDataService agentDataService = new AgentDataService() { Context = new UserClaim() { OrganizationID = organizationid } };
                        var agents = agentDataService.Search(new AgentCreteria()
                        {
                            id = bill.customer.agentid
                        });
                        if (agents.Count == 1)
                            bill.agent = agents[0];
                    }

                    if (bill.agent != null && !string.IsNullOrEmpty(bill.agent.emailid))
                    {
                        mailid = bill.agent.emailid;
                    }
                }

                if (logenabled)
                {
                    WebAPI.Common.Logger.Instance(organizationid).Log("Action Scheduler - Bill " + bill.billno, WebAPI.Common.Logger.LogType.Error, result);
                }
            }
            else if (queueItem.type == SourceType.PurchaseOrder)
            {
                Models.Purchaseorder bill = queueItem.source as Models.Purchaseorder;
                var documentbytes = format.RenderBytes(new Common.ImageData(), bill, bill.Purchaseorderitems, new SortedDictionary<string, object>(), false);
                result = System.Text.ASCIIEncoding.ASCII.GetString(documentbytes);
                companyid = bill.companyid;
                supplierid = bill.supplierid;
                agentid = bill.agentid;
                if (bill.attachment != null)
                    attachments.Add("Purhase Order - " + bill.pono + ".pdf", bill.attachment);
                if (attachments.Count == 0)
                {
                    Controllers.PurchaseorderServiceController billDataService = new Controllers.PurchaseorderServiceController();
                    PurchaseorderDataService PurchaseorderData = new PurchaseorderDataService() { Context = new UserClaim() { OrganizationID = organizationid } };
                    PurchaseorderPrint print = PurchaseorderData.Print(new Purchaseorder()
                    {
                        id = bill.id
                    });
                    InfyPOS.Controllers.PurchaseorderServiceController purchaseorderServiceController = new InfyPOS.Controllers.PurchaseorderServiceController();
                    attachments.Add("Purhase Order - " + print.pono + ".pdf", purchaseorderServiceController.PrintPO(organizationid, print).Content.ReadAsByteArrayAsync().Result);
                }

                if (logenabled)
                {
                    WebAPI.Common.Logger.Instance(organizationid).Log("Action Scheduler - PurchaseOrder " + bill.pono, WebAPI.Common.Logger.LogType.Error, result);
                }
            }
            else if (queueItem.type == SourceType.SupplierPayment)
            {
                Models.SupplierPaymentPrint bill = queueItem.source as Models.SupplierPaymentPrint;
                attachments = bill.attachment;
                companyid = bill.company.id;
                supplierid = bill.supplier.id;
                agentid = bill.agentid;
                var documentbytes = format.RenderBytes(new Common.ImageData(), bill, bill.Bills, new SortedDictionary<string, object>(), false);
                result = System.Text.ASCIIEncoding.ASCII.GetString(documentbytes);

                if (attachments == null)
                {
                    attachmentname = bill.receiptno.Replace("/", "").Replace("\\", "") + ".pdf";
                    SupplierpaymentDataService SupplierpaymentData = new SupplierpaymentDataService() { Context = new UserClaim() { OrganizationID = organizationid } };
                    var fileattachment = SupplierpaymentData.PrintChequeAdvice(SupplierpaymentData.Context.OrganizationID, bill);

                    if (fileattachment != null)
                        attachments.Add(attachmentname, fileattachment.Content.ReadAsByteArrayAsync().Result);
                }

                if(!string.IsNullOrEmpty(result) 
                    && result.ToUpper().Contains("DEBIT NOTE"))
                {
                    if (bill.hasreturns)
                    {
                        foreach (var ret in bill.Returns)
                        {
                            if (ret.invoiceid > 0)
                            {
                                LrinvoicereturnDataService LrinvoicereturnData = new LrinvoicereturnDataService() { Context = new UserClaim() { OrganizationID = organizationid } };
                                LrinvoicereturnPrint lrresult = LrinvoicereturnData.Print(new Lrinvoicereturn() { id = ret.invoiceid });
                                if (lrresult.returnno == ret.invoiceno)
                                {
                                    Controllers.LrinvoicereturnServiceController lrinvoicereturnitemsServiceController = new Controllers.LrinvoicereturnServiceController();
                                    attachments.Add(lrresult.returnno + ".pdf", lrinvoicereturnitemsServiceController.PrintDebitNote(organizationid, lrresult).Content.ReadAsByteArrayAsync().Result);
                                }
                            }
                        }
                    }
                }

                if (logenabled)
                {
                    WebAPI.Common.Logger.Instance(organizationid).Log("Action Scheduler - Supplier Payment " + bill.receiptno, WebAPI.Common.Logger.LogType.Error, result);
                }
            }
            else if (queueItem.type == SourceType.PurchaseReturn)
            {
                Models.Lrinvoicereturn bill = queueItem.source as Models.Lrinvoicereturn;
                var documentbytes = format.RenderBytes(new Common.ImageData(), bill, bill.Lrinvoicereturnitems, new SortedDictionary<string, object>(), false);
                result = System.Text.ASCIIEncoding.ASCII.GetString(documentbytes);
                companyid = bill.companyid;
                supplierid = bill.supplierid;

                LrinvoicereturnDataService LrinvoicereturnData = new LrinvoicereturnDataService() { Context = new UserClaim() { OrganizationID = organizationid } };
                LrinvoicereturnPrint lrresult = LrinvoicereturnData.Print(new Lrinvoicereturn() { id = bill.id });
                Controllers.LrinvoicereturnServiceController lrinvoicereturnitemsServiceController = new Controllers.LrinvoicereturnServiceController();
                attachments.Add(lrresult.returnno + ".pdf", lrinvoicereturnitemsServiceController.PrintDebitNote(organizationid, lrresult).Content.ReadAsByteArrayAsync().Result);

                if (logenabled)
                {
                    WebAPI.Common.Logger.Instance(organizationid).Log("Action Scheduler - PurchaseReturn " + bill.returnno, WebAPI.Common.Logger.LogType.Error, result);
                }
            }
            else if (queueItem.type == SourceType.Alteration)
            {
                Models.Delivery alteration = queueItem.source as Models.Delivery;
                var documentbytes = format.RenderBytes(new Common.ImageData(), alteration, null, new SortedDictionary<string, object>(), false);
                result = System.Text.ASCIIEncoding.ASCII.GetString(documentbytes);
                customerid = alteration.customerid;
                if (logenabled)
                {
                    WebAPI.Common.Logger.Instance(organizationid).Log("Action Scheduler - Alteration " + alteration.altationno, WebAPI.Common.Logger.LogType.Error, result);
                }
            }
            else if (queueItem.type == SourceType.DayClose)
            {
                Models.DateConfiguration bill = queueItem.source as Models.DateConfiguration;
                locationid = bill.locationid;
                ScheduleManager.IReportExecutor.CommonRoot root = new ScheduleManager.IReportExecutor.CommonRoot()
                {
                    fromdate = bill.currentday,
                    todate = bill.currentday,
                    title = scheduleconfig.description
                };
                if (scheduleconfig.config.companyid > 0)
                {
                    using (var reader = baseData.ExecuteReader("Select name from company where id=" + scheduleconfig.config.companyid, null))
                    {
                        if (reader.Read())
                            root.company = reader.GetString(0);
                    }
                }
                if ((scheduleconfig.config.parameter == "All Location" ? bill.locationid : 0) > 0)
                {
                    using (var reader = baseData.ExecuteReader("Select name from referencelist where id=" + bill.locationid, null))
                    {
                        if (reader.Read())
                            root.location = reader.GetString(0);
                    }
                }

                RetailReportDataService RetailReportData = new RetailReportDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
                var dayclose = RetailReportData.SearchDaySummary(new SaleRetailCreteria()
                {
                    companyid = scheduleconfig.config.parameter != "All Location" ? scheduleconfig.config.companyid : 0,
                    locationid = scheduleconfig.config.parameter != "All Location" ? bill.locationid : 0,
                    fromdate = bill.currentday,
                    todate = bill.currentday
                }).Items;

                var documentbytes = format.RenderBytes(new Common.ImageData(), root, dayclose, new SortedDictionary<string, object>(), false);
                result = System.Text.ASCIIEncoding.ASCII.GetString(documentbytes);

                if (logenabled)
                {
                    WebAPI.Common.Logger.Instance(organizationid).Log("Action Scheduler - DayClose " + scheduleconfig.config.parameter, WebAPI.Common.Logger.LogType.Error, result);
                }
            }
            else if (queueItem.type == SourceType.Employee)
            {
                SortedDictionary<string, object> param = new SortedDictionary<string, object>();
                Models.Employee oemployee = queueItem.source as Models.Employee;
                employeeid = oemployee.id;
                if (oemployee.sectionid > 0)
                {
                    using (var reader = baseData.ExecuteReader("Select name from referencelist where id=" + oemployee.sectionid, null))
                    {
                        if (reader.Read())
                            oemployee.sectionname = reader.GetString(0);
                    }
                }
                if (oemployee.desingnationid > 0 && string.IsNullOrEmpty(oemployee.designation))
                {
                    using (var reader = baseData.ExecuteReader("Select name from referencelist where id=" + oemployee.desingnationid, null))
                    {
                        if (reader.Read())
                            oemployee.designation = reader.GetString(0);
                    }
                }

                var documentbytes = format.RenderBytes(new Common.ImageData(), queueItem.source, null, new SortedDictionary<string, object>(), false);
                result = System.Text.ASCIIEncoding.ASCII.GetString(documentbytes);
            }
            else
            {
                if (queueItem.source is Models.BillOTP)
                {
                    mailid = (queueItem.source as Models.BillOTP).EMAIL;
                }
                var documentbytes = format.RenderBytes(new Common.ImageData(), queueItem.source, null, new SortedDictionary<string, object>(), false);
                result = System.Text.ASCIIEncoding.ASCII.GetString(documentbytes);

            }

            if (!string.IsNullOrEmpty(result) || attachments != null)
            {
                if (!(queueItem.source is Models.BillOTP) &&
                    !string.IsNullOrEmpty(scheduleconfig.config.sendto) && scheduleconfig.config.source == "Custom")
                {
                    mailid = scheduleconfig.config.sendto;
                }
                else if (string.IsNullOrEmpty(mailid))
                {
                    if (scheduleconfig.config.source == "Company" && companyid > 0)
                    {
                        using (var reader = baseData.ExecuteReader("Select emailid from company where id=" + companyid, null))
                        {
                            if (reader.Read())
                                mailid = reader.IsDBNull(0) ? "" : reader.GetString(0);
                        }
                    }
                    else if (scheduleconfig.config.source == "Supplier" && supplierid > 0)
                    {
                        using (var reader = baseData.ExecuteReader("Select emailid from supplier where id=" + supplierid, null))
                        {
                            if (reader.Read())
                                mailid = reader.IsDBNull(0) ? "" : reader.GetString(0);
                        }
                    }
                    else if (scheduleconfig.config.source == "Customer" && customerid > 0)
                    {
                        using (var reader = baseData.ExecuteReader("Select emailid from customer where id=" + customerid, null))
                        {
                            if (reader.Read())
                                mailid = reader.IsDBNull(0) ? "" : reader.GetString(0);
                        }
                    }
                    else if (scheduleconfig.config.source == "Employee" && employeeid > 0)
                    {
                        using (var reader = baseData.ExecuteReader("Select emailid from employee where id=" + employeeid, null))
                        {
                            if (reader.Read())
                                mailid = reader.IsDBNull(0) ? "" : reader.GetString(0);
                        }
                    }
                    else if (scheduleconfig.config.source == "Agent" && customerid > 0)
                    {
                        using (var reader = baseData.ExecuteReader("Select agent.emailid from customer inner join agent on agent.id = agentid where customer.id=" + customerid, null))
                        {
                            if (reader.Read())
                                mailid = reader.IsDBNull(0) ? "" : reader.GetString(0);
                        }
                    }
                    else if (scheduleconfig.config.source == "Agent" && agentid > 0)
                    {
                        using (var reader = baseData.ExecuteReader("Select agent.emailid from agent where id=" + agentid, null))
                        {
                            if (reader.Read())
                                mailid = reader.IsDBNull(0) ? "" : reader.GetString(0);
                        }
                    }
                }

                if (!string.IsNullOrEmpty(mailid))
                {
                    if (logenabled)
                    {
                        WebAPI.Common.Logger.Instance(organizationid).Log("Action Scheduler " + scheduleconfig.name + " MAIL - " + mailid, WebAPI.Common.Logger.LogType.Error, result);
                    }
                    Processors.ScheduleManager.Instance.SendGMail(organizationid, locationid,
                        scheduleconfig, result, attachments, attachmentname, mailid);
                }
                else if (logenabled)
                {
                    WebAPI.Common.Logger.Instance(organizationid).Log("Action Scheduler - MailID Missing " + scheduleconfig.name, WebAPI.Common.Logger.LogType.Error, result);
                }
            }
        }

        private void ExecuteItem(long organizationid, Models.Schedule scheduleconfig, QueueItem queueItem)
        {
            long locationid = 0;
            Common.BaseData baseData = new Common.BaseData() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            ReportLibrary.DefaultPageSettings setting = new ReportLibrary.DefaultPageSettings();
            ReportLibrary.DotMatrixReporter labreport = new ReportLibrary.DotMatrixReporter(organizationid, new ReportLibrary.DotMatrixReporter.PrinterConfig());
            labreport.format = ReportLibrary.DosFormatParser.ParseXML(organizationid, scheduleconfig.config.format);
            var result = "";
            string mobileno = "";
            long companyid = 0;
            long supplierid = 0;
            long customerid = 0;
            long employeeid = 0;
            SortedDictionary<string, string> smsparameters = new SortedDictionary<string, string>();
            if (queueItem.type == SourceType.Bill)
            {
                Models.Bill bill = queueItem.source as Models.Bill;
                locationid = bill.locationid;
                if (scheduleconfig.config.companyid > 0 && bill.companyid != scheduleconfig.config.companyid) return;
                if (scheduleconfig.config.locationid > 0 && bill.locationid != scheduleconfig.config.locationid) return;

                smsparameters.Add("BILLKEY", "BILL_" + 0.ToBase62() + "_" + bill.id.ToBase62());
                smsparameters.Add("NAME", bill.customer != null && bill.customer.name != null ? bill.customer.name.Replace(@"""", "") : "");
                smsparameters.Add("LOYELTYAVAILABLE", bill.billattributes.remainingreward.ToString("F0"));
                smsparameters.Add("LOYELTYCLAIMED", bill.billattributes.spentreward.ToString("F0"));
                smsparameters.Add("LOYELTYISSUED", bill.billattributes.earnedreward.ToString("F0"));
                smsparameters.Add("BILLNO", string.IsNullOrEmpty(bill.billno) ? "" : bill.billno.Replace(@"""", ""));
                smsparameters.Add("BILLAMOUNT", bill.total.ToString("F0"));
                smsparameters.Add("BILLDATE", bill.billdate.ToString("yyyy-MM-dd hh:mm:ss"));
                smsparameters.Add("BILLEDON", bill.billdate.ToString("yyyy-MM-dd"));

                if (scheduleconfig.config.parameter == "Discount Greater than")
                {
                    if (scheduleconfig.config.addlndiscountconfig.invalue > 0 &&
                        bill.totaldiscount < scheduleconfig.config.addlndiscountconfig.invalue) return;

                    if (scheduleconfig.config.addlndiscountconfig.inpercentage > 0 &&
                        bill.totaldiscountpercentage < scheduleconfig.config.addlndiscountconfig.inpercentage) return;
                }

                if (scheduleconfig.config.source == "Customer")
                {
                    if (bill.customer != null && !string.IsNullOrEmpty(bill.customer.mobileno))
                    {
                        CustomerrewardsDataService rewarddate = new CustomerrewardsDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
                        bill.customerreward = rewarddate.SearchReward(new Models.CustomerrewardsCreteria() { customerid = bill.customerid, locationid = bill.locationid }).available;

                        var BillDataService = new BillDataService() { Context = new UserClaim() { OrganizationID = organizationid } };
                        bill.fburl = BillDataService.GetFBURL(bill.billno);
                        bill.billkey = "BILL_" + organizationid.ToBase62() + "_" + bill.id.ToBase62();
                    }
                }

                if (bill.transport == null &&
                    bill.customer != null &&
                    bill.customer.preferredtransportid > 0)
                {
                    TransportDataService transportDataService = new TransportDataService()
                    {
                        Context = new UserClaim()
                        {
                            OrganizationID = organizationid,
                        }
                    };
                    var transports = transportDataService.Search(new TransportCreteria() { id = bill.customer.preferredtransportid });
                    if (transports != null &&
                        transports.Count > 0)
                    {
                        bill.transport = transports[0];
                        bill.transportname = transports[0].name;
                    }
                }

                if (bill.agent == null && bill.customer != null &&
                    bill.customer.agentid > 0)
                {
                    AgentDataService agentDataService = new AgentDataService() { Context = new UserClaim() { OrganizationID = organizationid } };
                    var agents = agentDataService.Search(new AgentCreteria()
                    {
                        id = bill.customer.agentid
                    });
                    if (agents.Count == 1)
                    {
                        bill.agent = agents[0];
                        bill.agentname = agents[0].name;
                    }
                }


                result = labreport.format.Render(new Common.ImageData(), bill, bill.Billitems, new SortedDictionary<string, object>());
                companyid = bill.companyid;
                customerid = bill.customerid;

                if (scheduleconfig.config.source == "Company")
                {
                    if (bill.company != null && !string.IsNullOrEmpty(bill.company.contactno))
                        mobileno = bill.company.contactno;
                }
                else if (scheduleconfig.config.source == "Customer")
                {
                    if (bill.customer != null && !string.IsNullOrEmpty(bill.customer.mobileno))
                        mobileno = bill.customer.mobileno;
                }
                else if (scheduleconfig.config.source == "Agent" &&
                    bill.customer != null &&
                    bill.customer.agentid > 0)
                {
                    if (bill.agent == null)
                    {
                        AgentDataService agentDataService = new AgentDataService() { Context = new UserClaim() { OrganizationID = organizationid } };
                        var agents = agentDataService.Search(new AgentCreteria()
                        {
                            id = bill.customer.agentid
                        });
                        if (agents.Count == 1)
                            bill.agent = agents[0];
                    }

                    if (bill.agent != null && !string.IsNullOrEmpty(bill.agent.contactno))
                    {
                        mobileno = bill.agent.contactno;
                    }
                }

                if (logenabled)
                {
                    WebAPI.Common.Logger.Instance(organizationid).Log("Action Scheduler - Bill " + bill.billno, WebAPI.Common.Logger.LogType.Error, result);
                }
            }
            else if (queueItem.type == SourceType.PurchaseOrder)
            {
                Models.Purchaseorder bill = queueItem.source as Models.Purchaseorder;
                result = labreport.format.Render(new Common.ImageData(), bill, bill.Purchaseorderitems, new SortedDictionary<string, object>());
                companyid = bill.companyid;
                supplierid = bill.supplierid;
                if (logenabled)
                {
                    WebAPI.Common.Logger.Instance(organizationid).Log("Action Scheduler - PurchaseOrder " + bill.pono, WebAPI.Common.Logger.LogType.Error, result);
                }
            }
            else if (queueItem.type == SourceType.PurchaseReturn)
            {
                Models.Lrinvoicereturn bill = queueItem.source as Models.Lrinvoicereturn;
                result = labreport.format.Render(new Common.ImageData(), bill, bill.Lrinvoicereturnitems, new SortedDictionary<string, object>());
                companyid = bill.companyid;
                supplierid = bill.supplierid;
                if (logenabled)
                {
                    WebAPI.Common.Logger.Instance(organizationid).Log("Action Scheduler - PurchaseReturn " + bill.returnno, WebAPI.Common.Logger.LogType.Error, result);
                }
            }
            else if (queueItem.type == SourceType.SupplierPayment)
            {
                Models.SupplierPaymentPrint bill = queueItem.source as Models.SupplierPaymentPrint;
                result = labreport.format.Render(new Common.ImageData(), bill, bill.Bills, new SortedDictionary<string, object>());
                companyid = bill.company.id;
                supplierid = bill.supplier.id;
                if (logenabled)
                {
                    WebAPI.Common.Logger.Instance(organizationid).Log("Action Scheduler - Supplier Payment " + bill.receiptno, WebAPI.Common.Logger.LogType.Error, result);
                }
            }
            else if (queueItem.type == SourceType.Alteration)
            {
                Models.Delivery alteration = queueItem.source as Models.Delivery;
                result = labreport.format.Render(new Common.ImageData(), alteration, null, new SortedDictionary<string, object>());
                customerid = alteration.customerid;
                mobileno = alteration.contactno;
                smsparameters.Add("MOBILE", alteration.contactno);
                smsparameters.Add("NAME", alteration.customername);
                smsparameters.Add("QTY", alteration.qty.ToString());
                smsparameters.Add("DATE", alteration.billdate.ToString("dd-MM-yyyy"));
                smsparameters.Add("DUEDATE", alteration.expectedon.ToString("dd-MM-yyyy"));
                smsparameters.Add("ORDERNO", alteration.orderno);
                smsparameters.Add("ENTRYNO", alteration.entryno);
                if (logenabled)
                {
                    WebAPI.Common.Logger.Instance(organizationid).Log("Action Scheduler - Alteration " + alteration.altationno, WebAPI.Common.Logger.LogType.Error, result);
                }
            }
            else if (queueItem.type == SourceType.Settlement)
            {
                Models.Billsettlementmaster alteration = queueItem.source as Models.Billsettlementmaster;
                result = labreport.format.Render(new Common.ImageData(), alteration, null, new SortedDictionary<string, object>());
                customerid = alteration.customerid;
                mobileno = alteration.customermobileno;
                smsparameters.Add("MOBILE", alteration.customermobileno);
                smsparameters.Add("NAME", alteration.customername);
                smsparameters.Add("BILLKEY", "BILLSETTLEMENT_" + 0.ToBase62() + "_" + alteration.id.ToBase62());
                smsparameters.Add("NAME", alteration.customername != null ? alteration.customername.Replace(@"""", "") : "");

                smsparameters.Add("LOYELTYAVAILABLE", alteration.paymentinfo.avlrewardpoint.ToString("F0"));
                smsparameters.Add("LOYELTYCLAIMED", alteration.paymentinfo.reward_spent.ToString("F0"));
                smsparameters.Add("LOYELTYISSUED", alteration.paymentinfo.reward_earned.ToString("F0"));
                smsparameters.Add("LOYELTYCLAIMEDAMOUT", alteration.paymentinfo.reward_amount.ToString("F0"));

                smsparameters.Add("BILLNO", string.Join(",", alteration.Billsettlement.ConvertAll(y => y.billno)));
                smsparameters.Add("BILLAMOUNT", alteration.Billsettlement.Sum(x => x.receivable).ToString("F0"));
                smsparameters.Add("BILLDATE", alteration.settlementon.ToString("yyyy-MM-dd hh:mm:ss"));
                smsparameters.Add("BILLEDON", alteration.settlementon.ToString("yyyy-MM-dd"));
                smsparameters.Add("SETTLEMENTAMOUNT", alteration.receivable.ToString());
                if (alteration.coupons != null)
                    smsparameters.Add("COUPONS", string.Join(",", alteration.coupons.ConvertAll(X => X.code)));

                if (logenabled)
                {
                    WebAPI.Common.Logger.Instance(organizationid).Log("Action Scheduler - Settlement " + alteration.code, WebAPI.Common.Logger.LogType.Error, result);
                }
            }
            else if (queueItem.type == SourceType.DayClose)
            {
                Models.DateConfiguration bill = queueItem.source as Models.DateConfiguration;
                locationid = bill.locationid;
                ScheduleManager.IReportExecutor.CommonRoot root = new ScheduleManager.IReportExecutor.CommonRoot()
                {
                    fromdate = bill.currentday,
                    todate = bill.currentday,
                    title = scheduleconfig.description
                };
                if (scheduleconfig.config.companyid > 0)
                {
                    using (var reader = baseData.ExecuteReader("Select name from company where id=" + scheduleconfig.config.companyid, null))
                    {
                        if (reader.Read())
                            root.company = reader.GetString(0);
                    }
                }
                if ((scheduleconfig.config.parameter == "All Location" ? bill.locationid : 0) > 0)
                {
                    using (var reader = baseData.ExecuteReader("Select name from referencelist where id=" + bill.locationid, null))
                    {
                        if (reader.Read())
                            root.location = reader.GetString(0);
                    }
                }

                RetailReportDataService RetailReportData = new RetailReportDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
                var dayclose = RetailReportData.SearchDaySummary(new SaleRetailCreteria()
                {
                    companyid = scheduleconfig.config.parameter != "All Location" ? scheduleconfig.config.companyid : 0,
                    locationid = scheduleconfig.config.parameter != "All Location" ? bill.locationid : 0,
                    fromdate = bill.currentday,
                    todate = bill.currentday
                }).Items;

                result = labreport.format.Render(new Common.ImageData(), root, dayclose, new SortedDictionary<string, object>());
                if (logenabled)
                {
                    WebAPI.Common.Logger.Instance(organizationid).Log("Action Scheduler - DayClose " + scheduleconfig.config.parameter, WebAPI.Common.Logger.LogType.Error, result);
                }
            }
            else if (queueItem.type == SourceType.Employee)
            {
                SortedDictionary<string, object> param = new SortedDictionary<string, object>();
                Models.Employee oemployee = queueItem.source as Models.Employee;
                employeeid = oemployee.id;
                if (oemployee.sectionid > 0)
                {
                    using (var reader = baseData.ExecuteReader("Select name from referencelist where id=" + oemployee.sectionid, null))
                    {
                        if (reader.Read())
                            oemployee.sectionname = reader.GetString(0);
                    }
                }
                if (oemployee.desingnationid > 0 && string.IsNullOrEmpty(oemployee.designation))
                {
                    using (var reader = baseData.ExecuteReader("Select name from referencelist where id=" + oemployee.desingnationid, null))
                    {
                        if (reader.Read())
                            oemployee.designation = reader.GetString(0);
                    }
                }

                result = labreport.format.Render(new Common.ImageData(), queueItem.source, null, param);
            }
            else
            {
                SortedDictionary<string, object> param = new SortedDictionary<string, object>();
                if (queueItem.source is Models.BillOTP)
                {
                    var billotp = (queueItem.source as Models.BillOTP);
                    mobileno = billotp.MOBILE;
                    if (!smsparameters.ContainsKey("MOBILE"))
                        smsparameters.Add("MOBILE", billotp.MOBILE);
                    if (!smsparameters.ContainsKey("OTP"))
                        smsparameters.Add("OTP", billotp.OTP);
                    if (!smsparameters.ContainsKey("CUSTOMER"))
                        smsparameters.Add("CUSTOMER", billotp.CUSTOMER);
                    if (!smsparameters.ContainsKey("CLAIMAMOUNT"))
                        smsparameters.Add("CLAIMAMOUNT", billotp.CLAIMAMOUNT.ToString("N2"));
                    if (!smsparameters.ContainsKey("CLAIMPOINT"))
                        smsparameters.Add("CLAIMPOINT", billotp.CLAIMAMOUNT.ToString("N2"));

                    param.Add("MOBILE", billotp.MOBILE);
                    param.Add("OTP", billotp.OTP);
                    param.Add("CUSTOMER", billotp.CUSTOMER);
                    param.Add("CLAIMAMOUNT", billotp.CLAIMAMOUNT.ToString("N2"));
                    param.Add("CLAIMPOINT", billotp.CLAIMAMOUNT.ToString("N2"));
                    param.Add("MOBILENO", billotp.MOBILE);
                    param.Add("NAME", billotp.CUSTOMER);
                    param.Add("REASON", billotp.REASON);
                    param.Add("BILLAMOUNT", billotp.BILLAMOUNT.ToString("F0"));
                    param.Add("DISCOUNT", billotp.DISCOUNT.ToString("F0"));
                    param.Add("DISCOUNTPERCENT", billotp.DISCOUNTPERCENT.ToString("F0"));
                    param.Add("NETAMOUNT", billotp.NETAMOUNT.ToString("F0"));
                    if (!smsparameters.ContainsKey("MOBILENO"))
                        smsparameters.Add("MOBILENO", billotp.MOBILE);
                    if (!smsparameters.ContainsKey("NAME"))
                        smsparameters.Add("NAME", billotp.CUSTOMER);
                    if (!smsparameters.ContainsKey("REASON"))
                        smsparameters.Add("REASON", billotp.REASON);
                    if (!smsparameters.ContainsKey("BILLAMOUNT"))
                        smsparameters.Add("BILLAMOUNT", billotp.BILLAMOUNT.ToString("F0"));
                    if (!smsparameters.ContainsKey("DISCOUNT"))
                        smsparameters.Add("DISCOUNT", billotp.DISCOUNT.ToString("F0"));
                    if (!smsparameters.ContainsKey("DISCOUNTPERCENT"))
                        smsparameters.Add("DISCOUNTPERCENT", billotp.DISCOUNTPERCENT.ToString("F0"));
                    if (!smsparameters.ContainsKey("NETAMOUNT"))
                        smsparameters.Add("NETAMOUNT", billotp.NETAMOUNT.ToString("F0"));
                }
                result = labreport.format.Render(new Common.ImageData(), queueItem.source, null, param);
            }

            if (!string.IsNullOrEmpty(result))
            {
                List<InfyPOS.Models.Smshistory> smshistories = new List<Models.Smshistory>();
                if (scheduleconfig.config.output == "SMS")
                {
                    if (!(queueItem.source is Models.BillOTP) &&
                        !string.IsNullOrEmpty(scheduleconfig.config.sendto) && scheduleconfig.config.source == "Custom")
                    {
                        mobileno = scheduleconfig.config.sendto;
                    }
                    else if (string.IsNullOrEmpty(mobileno))
                    {
                        if (scheduleconfig.config.source == "Company" && companyid > 0)
                        {
                            using (var reader = baseData.ExecuteReader("Select contactno from company where id=" + companyid, null))
                            {
                                if (reader.Read())
                                    mobileno = reader.IsDBNull(0) ? "" : reader.GetString(0);
                            }
                        }
                        else if (scheduleconfig.config.source == "Supplier" && supplierid > 0)
                        {
                            using (var reader = baseData.ExecuteReader("Select contactnumber from supplier where id=" + supplierid, null))
                            {
                                if (reader.Read())
                                    mobileno = reader.IsDBNull(0) ? "" : reader.GetString(0);
                            }
                        }
                        else if (scheduleconfig.config.source == "Customer" && customerid > 0)
                        {
                            using (var reader = baseData.ExecuteReader("Select mobileno from customer where id=" + customerid, null))
                            {
                                if (reader.Read())
                                    mobileno = reader.IsDBNull(0) ? "" : reader.GetString(0);
                            }
                        }
                        else if (scheduleconfig.config.source == "Employee" && employeeid > 0)
                        {
                            using (var reader = baseData.ExecuteReader("Select contactno from employee where id=" + employeeid, null))
                            {
                                if (reader.Read())
                                    mobileno = reader.IsDBNull(0) ? "" : reader.GetString(0);
                            }
                        }
                        else if (scheduleconfig.config.source == "Agent" && customerid > 0)
                        {
                            using (var reader = baseData.ExecuteReader("Select agent.contactno from customer inner join agent on agent.id = agentid where customer.id=" + customerid, null))
                            {
                                if (reader.Read())
                                    mobileno = reader.IsDBNull(0) ? "" : reader.GetString(0);
                            }
                        }
                    }


                    if (!string.IsNullOrEmpty(mobileno))
                    {
                        if (!smsparameters.ContainsKey("MOBILENO"))
                            smsparameters.Add("MOBILENO", mobileno);

                        if (logenabled)
                        {
                            WebAPI.Common.Logger.Instance(organizationid).Log("Action Scheduler " + scheduleconfig.name + " SMS - " + mobileno, WebAPI.Common.Logger.LogType.Error, result);
                        }
                        if (result.EndsWith("\r\n"))
                        {
                            result = result.Substring(0, result.Length - "\r\n".Length);
                        }
                        smshistories.Add(Processors.ScheduleManager.Instance.SendSMS(organizationid, locationid,
                            scheduleconfig, result, mobileno, smsparameters));
                    }
                    else if (logenabled)
                    {
                        WebAPI.Common.Logger.Instance(organizationid).Log("Action Scheduler - Mobile No Missing " + scheduleconfig.name, WebAPI.Common.Logger.LogType.Error, result);
                    }
                }
                else if (logenabled)
                {
                    WebAPI.Common.Logger.Instance(organizationid).Log("Action Scheduler - Template Missing / Error " + scheduleconfig.name, WebAPI.Common.Logger.LogType.Error, scheduleconfig);
                }

                if (smshistories.Count > 0)
                {
                    InfyPOS.Data.SmshistoryDataService smshistoryDataService = new Data.SmshistoryDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
                    smshistoryDataService.InsertAll(smshistories);
                }
            }
        }
        public void ExecuteQueueItem(long organizationid, QueueItem queueItem)
        {
            try
            {
                if (queueItem == null) return;

                if (queueItem.action == Actions.CRM_Campaign && queueItem.schedule != null)
                {
                    if (queueItem.schedule != null &&
                        queueItem.schedule.config != null && 
                        queueItem.schedule.config.output == "WebAPI")
                    {
                        ExecuteWebAPIItem(organizationid, queueItem.schedule, queueItem);
                    }
                    else if (queueItem.schedule != null &&
                        queueItem.schedule.config != null && queueItem.schedule.config.output == "Email")
                    {
                        ExecuteWebMailItem(organizationid, queueItem.schedule, queueItem);
                    }
                    else if (queueItem.schedule != null &&
                        queueItem.schedule.config != null && queueItem.schedule.config.output == "Growaasan")
                    {
                        ExecuteGrowaasan(organizationid, queueItem.schedule, queueItem);
                    }
                    else if (queueItem.schedule != null &&
                        queueItem.schedule.config != null && 
                        !string.IsNullOrEmpty(queueItem.schedule.config.format))
                    {
                        ExecuteItem(organizationid, queueItem.schedule, queueItem);
                    }
                }
                else
                {
                    var scheduleconfigList = GetSchedule(organizationid, queueItem);
                    if (scheduleconfigList != null && scheduleconfigList.Count > 0)
                    {
                        foreach (var kv in scheduleconfigList)
                        {
                            var scheduleconfig = kv.Value;
                            if (scheduleconfig != null && scheduleconfig.config != null && scheduleconfig.config.output == "WebAPI")
                            {
                                ExecuteWebAPIItem(organizationid, scheduleconfig, queueItem);
                            }
                            else if (scheduleconfig != null && scheduleconfig.config != null && scheduleconfig.config.output == "Email")
                            {
                                ExecuteWebMailItem(organizationid, scheduleconfig, queueItem);
                            }
                            else if (scheduleconfig != null && scheduleconfig.config != null && scheduleconfig.config.output == "Growaasan")
                            {
                                ExecuteGrowaasan(organizationid, scheduleconfig, queueItem);
                            }
                            else if (scheduleconfig != null && scheduleconfig.config != null && !string.IsNullOrEmpty(scheduleconfig.config.format))
                            {
                                ExecuteItem(organizationid, scheduleconfig, queueItem);
                            }
                        }
                    }
                }
            }
            catch (Exception exp)
            {
               /* WebAPI.Common.Logger.Instance(0).PerformanceLog("ExecuteQueueItem-Schedule",
                    WebAPI.Common.Logger.LogType.Error,
                    (queueItem == null || queueItem.schedule == null) ? "ExecuteQueueItem":queueItem.schedule.name, 
                    queueItem, 0, 0);*/
            }
        }

        public void ExecuteGrowaasan(long organizationid, Schedule scheduleconfig, QueueItem queueItem)
        {
            Common.BaseData baseData = new Common.BaseData() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            SortedList<string, byte[]> files = new SortedList<string, byte[]>();
            string request = "";
            string mobileno = "";
            long companyid = 0;
            long supplierid = 0;
            long customerid = 0;
            long employeeid = 0;
            if (queueItem.type == SourceType.Bill && queueItem.action == Actions.ECommerce)
            {
                long locationid = 0;
                Models.Bill bill = queueItem.source as Models.Bill;
                locationid = bill.locationid;
                if (scheduleconfig.config.companyid > 0 && bill.companyid != scheduleconfig.config.companyid) return;
                if (scheduleconfig.config.locationid > 0 && bill.locationid != scheduleconfig.config.locationid) return;

                BillIntegration source = new BillIntegration(scheduleconfig.config.format);
                source.mode = "sales";
                source.type = bill.billattributes.online ? "ONLINE" : "OFFLINE";
                if (bill.billattributes == null)
                    bill.billattributes = new BillAttribute() { };

                source.bills = new BillIntegration.Bill()
                {
                    billamount = bill.total,
                    billdate = bill.billdate,
                    billno = bill.billno,
                    company = bill.company == null ? "" : bill.company.name,
                    branch = bill.location == null ? "" : bill.location.name,
                    loyeltyissued = bill.billattributes.earnedreward,
                    loyeltyclaimed = bill.billattributes.spentreward,
                    loyeltybalance = bill.billattributes.remainingreward
                };

                if (bill.billattributes.online)
                {
                    source.billitems = bill.Billitems.ConvertAll(x => new BillIntegration.BillItem()
                    {
                        amount = x.amount,
                        barcode = x.barcode,
                        gln = x.attributes == null ? "" : x.attributes.serialno,
                        onlinqty = x.attributes == null ? 0 : x.attributes.newolstock,
                        stockqty = x.attributes == null ? 0 : x.attributes.newstock,
                        soldqty = x.qty
                    });
                }
                else
                {
                    source.billitems = bill.Billitems.FindAll(x => x.attributes.isolstock).ConvertAll(x => new BillIntegration.BillItem()
                    {
                        amount = x.amount,
                        barcode = x.barcode,
                        gln = x.attributes == null ? "" : x.attributes.serialno,
                        onlinqty = x.attributes == null ? 0 : x.attributes.newolstock,
                        stockqty = x.attributes == null ? 0 : x.attributes.newstock,
                        soldqty = x.qty
                    });
                }

                if (InfyPOS.Services.ConfigManager.Instance(organizationid).Config.system_fixed_barcodes &&
                    !InfyPOS.Services.ConfigManager.Instance(organizationid).Config.system_hyper_uan &&
                    source.billitems.Exists(x => !string.IsNullOrEmpty(x.gln)))
                {
                    var glnbillitems = source.billitems.FindAll(x => !string.IsNullOrEmpty(x.gln));
                    SalablegoodsDataService salablegoodsDataService = new SalablegoodsDataService() { Context = new UserClaim() { OrganizationID = organizationid } };
                    glnbillitems = salablegoodsDataService.FindGLNStock(bill.locationid, glnbillitems);
                    source.billitems.RemoveAll(x => !string.IsNullOrEmpty(x.gln));
                    source.billitems.AddRange(glnbillitems);
                }

                source.vouchers = new List<BillIntegration.Vouchers>();
                if (bill.billattributes.coupons != null && bill.billattributes.coupons.Count > 0)
                {
                    foreach (var coupon in bill.billattributes.coupons)
                    {
                        source.vouchers.Add(new BillIntegration.Vouchers()
                        {
                            consumed = coupon.amount,
                            no = coupon.code
                        });
                    }
                }
                source.customer = new BillIntegration.Customer();
                if (bill.customer != null)
                {
                    source.customer.mobileno = bill.customer.mobileno;
                    source.customer.name = bill.customer.name;
                    source.customer.id = bill.customer.loyeltyno;
                }
                companyid = bill.companyid;
                customerid = bill.customerid;

                if (bill.billattributes.settlement != null &&
                    bill.billattributes.settlement.paymentinfo != null &&
                    bill.billattributes.settlement.paymentinfo.voucherinformation != null)
                {
                    foreach (var coupon in bill.billattributes.settlement.paymentinfo.voucherinformation)
                    {
                        source.vouchers.Add(new BillIntegration.Vouchers()
                        {
                            consumed = coupon.amount,
                            no = coupon.code
                        });
                    }
                }
                mobileno = source.customer.mobileno;
                request = source.destinationjson();

                if (scheduleconfig.config.growaasan.hasattachment)
                {
                    Controllers.BillServiceController billDataService = new Controllers.BillServiceController();
                    files.Add("file-1", billDataService.PrintBill(organizationid, bill, false, false, "", 0).Content.ReadAsByteArrayAsync().Result);
                }

            }
            else if (queueItem.type == SourceType.Bill && queueItem.action == Actions.New)
            {
                long locationid = 0;
                Models.Bill bill = queueItem.source as Models.Bill;
                if (bill.customer == null) return;
                locationid = bill.locationid;
                if (scheduleconfig.config.companyid > 0 && bill.companyid != scheduleconfig.config.companyid) return;
                if (scheduleconfig.config.locationid > 0 && bill.locationid != scheduleconfig.config.locationid) return;

                BillIntegration source = new BillIntegration(scheduleconfig.config.format);

                if (bill.billattributes == null)
                    bill.billattributes = new BillAttribute() { };

                source.bills = new BillIntegration.Bill()
                {
                    billamount = bill.total,
                    billdate = bill.billdate,
                    billno = bill.billno,
                    billid = bill.id,
                    organizationid = queueItem.organizationid,
                    loyeltyissued = bill.billattributes.earnedreward,
                    loyeltyclaimed = bill.billattributes.spentreward,
                    loyeltybalance = bill.billattributes.remainingreward
                };
                source.bills.company = bill.company == null ? "" : bill.company.name;
                source.bills.branch = bill.location == null ? "" : bill.location.name;
                source.vouchers = new List<BillIntegration.Vouchers>();
                if (bill.billattributes.coupons != null && bill.billattributes.coupons.Count > 0)
                {
                    foreach (var coupon in bill.billattributes.coupons)
                    {
                        source.vouchers.Add(new BillIntegration.Vouchers()
                        {
                            consumed = coupon.amount,
                            no = coupon.code
                        });
                    }
                }

                source.customer = new BillIntegration.Customer();
                source.customer.mobileno = bill.customer.mobileno;
                source.customer.name = bill.customer.name;
                source.customer.id = bill.customer.loyeltyno;

                if (bill.billattributes.settlement != null &&
                    bill.billattributes.settlement.paymentinfo != null &&
                    bill.billattributes.settlement.paymentinfo.voucherinformation != null)
                {
                    foreach (var coupon in bill.billattributes.settlement.paymentinfo.voucherinformation)
                    {
                        source.vouchers.Add(new BillIntegration.Vouchers()
                        {
                            consumed = coupon.amount,
                            no = coupon.code
                        });
                    }
                }
                mobileno = source.customer.mobileno;
                request = source.destinationjson();
                if (scheduleconfig.config.growaasan.hasattachment)
                {
                    Controllers.BillServiceController billDataService = new Controllers.BillServiceController();
                    files.Add("file-1", billDataService.PrintBill(organizationid, bill, false, false, "", 0).Content.ReadAsByteArrayAsync().Result);
                }
            }
            if (queueItem.type == SourceType.Customer && queueItem.action == Actions.CRM)
            {
                Models.Customer customer = queueItem.source as Models.Customer;
                if (scheduleconfig.config.locationid > 0 && customer.registredlocationid > 0 &&
                    customer.registredlocationid != scheduleconfig.config.locationid) return;
                customerid = customer.id;
                mobileno = customer.mobileno;
                request = scheduleconfig.config.format;
                request = request.Replace("[MOBILENO]", customer.mobileno);
                request = request.Replace("[NAME]", customer.name);
                request = request.Replace("[LOYELTY]", customer.loyelty.ToString("F0"));
            }
            if (queueItem.type == SourceType.Customer && queueItem.action == Actions.CRM_Campaign)
            {
                Models.Customer customer = queueItem.source as Models.Customer;
                if (scheduleconfig.config.locationid > 0 && customer.registredlocationid > 0 &&
                    customer.registredlocationid != scheduleconfig.config.locationid) return;

                customerid = customer.id;
                mobileno = customer.mobileno;
                request = scheduleconfig.config.format;
                request = request.Replace("[MOBILENO]", customer.mobileno);
                request = request.Replace("[NAME]", customer.name);
                request = request.Replace("[LOYELTYNO]", customer.loyeltyno);
                request = request.Replace("[LOYELTY]", customer.loyelty.ToString("F0"));
            }
            if (queueItem.type == SourceType.OTP && (queueItem.action == Actions.Otp_Loyelty || queueItem.action == Actions.Otp_Discount))
            {
                Models.BillOTP customer = queueItem.source as Models.BillOTP;
                if (scheduleconfig.config.locationid > 0 && customer.locationid > 0 &&
                    customer.locationid != scheduleconfig.config.locationid) return;
                mobileno = customer.MOBILE;
                request = scheduleconfig.config.format;
                request = request.Replace("[MOBILENO]", customer.MOBILE);
                request = request.Replace("[NAME]", customer.CUSTOMER);
                request = request.Replace("[OTP]", customer.OTP);
                request = request.Replace("[BILLAMOUNT]", customer.BILLAMOUNT.ToString("F0"));
                request = request.Replace("[DISCOUNT]", customer.DISCOUNT.ToString("F0"));
                request = request.Replace("[DISCOUNTPERCENT]", customer.DISCOUNTPERCENT.ToString("F0"));
                request = request.Replace("[NETAMOUNT]", customer.NETAMOUNT.ToString("F0"));
                request = request.Replace("[ALLOWEDCREDIT]", customer.ALLOWEDCREDIT.ToString("F0"));
                request = request.Replace("[REASON]", customer.REASON);
            }
            if (queueItem.type == SourceType.OTP && (queueItem.action == Actions.Otp_Credit))
            {
                Models.BillOTP customer = queueItem.source as Models.BillOTP;
                if (scheduleconfig.config.locationid > 0 && customer.locationid > 0 &&
                    customer.locationid != scheduleconfig.config.locationid) return;
                request = scheduleconfig.config.format;
                request = request.Replace("[MOBILENO]", scheduleconfig.config.sendto);
                request = request.Replace("[OTP]", customer.OTP);
                request = request.Replace("[CUSTOMER]", customer.CUSTOMER);
                request = request.Replace("[NAME]", customer.CUSTOMER);
                request = request.Replace("[BILLAMOUNT]", customer.BILLAMOUNT.ToString("F0"));
                request = request.Replace("[DISCOUNT]", customer.DISCOUNT.ToString("F0"));
                request = request.Replace("[NETAMOUNT]", customer.NETAMOUNT.ToString("F0"));
            }
            if (queueItem.type == SourceType.PurchaseOrder)
            {
                var format = ReportLibrary.DosFormatParser.ParseXML2HTML(organizationid, scheduleconfig.config.format);
                Models.Purchaseorder bill = queueItem.source as Models.Purchaseorder;
                companyid = bill.companyid;
                supplierid = bill.supplierid;
                var documentbytes = format.RenderBytes(new Common.ImageData(), bill, bill.Purchaseorderitems, new SortedDictionary<string, object>(), false);
                request = System.Text.ASCIIEncoding.ASCII.GetString(documentbytes);
                Controllers.PurchaseorderServiceController billDataService = new Controllers.PurchaseorderServiceController();
                PurchaseorderDataService PurchaseorderData = new PurchaseorderDataService() { Context = new UserClaim() { OrganizationID = organizationid } };
                PurchaseorderPrint print = PurchaseorderData.Print(new Purchaseorder()
                {
                    id = bill.id
                });

                files.Add("file-1", billDataService.PrintPO(PurchaseorderData.Context.OrganizationID, print).Content.ReadAsByteArrayAsync().Result);
                if (logenabled)
                {
                    WebAPI.Common.Logger.Instance(organizationid).Log("Action Scheduler - PurchaseOrder " + bill.pono, WebAPI.Common.Logger.LogType.Error, request);
                }
            }
            if (queueItem.type == SourceType.SupplierPayment)
            {
                ReportLibrary.DefaultPageSettings setting = new ReportLibrary.DefaultPageSettings();
                var format = ReportLibrary.DosFormatParser.ParseXML2HTML(organizationid, scheduleconfig.config.format);
                Models.SupplierPaymentPrint bill = queueItem.source as Models.SupplierPaymentPrint;
                if (bill.attachment == null)
                {
                    var documentbytes = format.RenderBytes(new Common.ImageData(), bill, bill.Bills, new SortedDictionary<string, object>(), false);
                    request = System.Text.ASCIIEncoding.ASCII.GetString(documentbytes);
                    SupplierpaymentDataService SupplierpaymentData = new SupplierpaymentDataService() { Context = new UserClaim() { OrganizationID = organizationid } };
                    var fileattachment = SupplierpaymentData.PrintChequeAdvice(SupplierpaymentData.Context.OrganizationID, bill);
                    files.Add("file-1", fileattachment.Content.ReadAsByteArrayAsync().Result);
                }
                if (logenabled)
                {
                    WebAPI.Common.Logger.Instance(organizationid).Log("Action Scheduler - Supplier Payment " + bill.receiptno, WebAPI.Common.Logger.LogType.Error, request);
                }
            }
            else if (queueItem.type == SourceType.Alteration)
            {
                Delivery delivery = queueItem.source as Delivery;

                if (scheduleconfig.config.locationid > 0 && delivery.locationid > 0 &&
                    delivery.locationid != scheduleconfig.config.locationid) return;

                request = scheduleconfig.config.format;

                if (delivery.customer != null)
                {
                    mobileno = delivery.customer.mobileno;
                    request = request.Replace("[MOBILENO]", delivery.customer.mobileno);
                    request = request.Replace("[NAME]", delivery.customer.name);
                }
                else
                {
                    mobileno = delivery.contactno;
                    request = request.Replace("[MOBILENO]", delivery.contactno);
                    request = request.Replace("[NAME]", delivery.customername);
                }
                request = request.Replace("[QTY]", delivery.qty.ToString());
                request = request.Replace("[DATE]", delivery.billdate.ToString("dd-MM-yyyy"));
                request = request.Replace("[DUEDATE]", delivery.expectedon.ToString("dd-MM-yyyy"));
                request = request.Replace("[ORDERNO]", delivery.orderno);
                request = request.Replace("[ENTRYNO]", delivery.entryno);
            }
            if (queueItem.type == SourceType.PurchaseReturn)
            {
                Models.Lrinvoicereturn bill = queueItem.source as Models.Lrinvoicereturn;
                ReportLibrary.DefaultPageSettings setting = new ReportLibrary.DefaultPageSettings();
                var format = ReportLibrary.DosFormatParser.ParseXML2HTML(organizationid, scheduleconfig.config.format);
                var documentbytes = format.RenderBytes(new Common.ImageData(), bill, bill.Lrinvoicereturnitems, new SortedDictionary<string, object>(), false);
                request = System.Text.ASCIIEncoding.ASCII.GetString(documentbytes);
                companyid = bill.companyid;
                supplierid = bill.supplierid;

                LrinvoicereturnDataService LrinvoicereturnData = new LrinvoicereturnDataService() { Context = new UserClaim() { OrganizationID = organizationid } };
                LrinvoicereturnPrint lrresult = LrinvoicereturnData.Print(new Lrinvoicereturn() { id = bill.id });
                Controllers.LrinvoicereturnServiceController lrinvoicereturnitemsServiceController = new Controllers.LrinvoicereturnServiceController();
                files.Add("file-1", lrinvoicereturnitemsServiceController.PrintDebitNote(organizationid, lrresult).Content.ReadAsByteArrayAsync().Result);

                if (logenabled)
                {
                    WebAPI.Common.Logger.Instance(organizationid).Log("Action Scheduler - PurchaseReturn " + bill.returnno, WebAPI.Common.Logger.LogType.Error, request);
                }
            }


            if (!(queueItem.source is Models.BillOTP) &&
                       !string.IsNullOrEmpty(scheduleconfig.config.sendto) && scheduleconfig.config.source == "Custom")
            {
                mobileno = scheduleconfig.config.sendto;
            }
            else if (string.IsNullOrEmpty(mobileno))
            {
                if (scheduleconfig.config.source == "Company" && companyid > 0)
                {
                    using (var reader = baseData.ExecuteReader("Select contactno from company where id=" + companyid, null))
                    {
                        if (reader.Read())
                            mobileno = reader.IsDBNull(0) ? "" : reader.GetString(0);
                    }
                }
                else if (scheduleconfig.config.source == "Supplier" && supplierid > 0)
                {
                    using (var reader = baseData.ExecuteReader("Select contactnumber from supplier where id=" + supplierid, null))
                    {
                        if (reader.Read())
                            mobileno = reader.IsDBNull(0) ? "" : reader.GetString(0);
                    }
                }
                else if (scheduleconfig.config.source == "Customer" && customerid > 0)
                {
                    using (var reader = baseData.ExecuteReader("Select mobileno from customer where id=" + customerid, null))
                    {
                        if (reader.Read())
                            mobileno = reader.IsDBNull(0) ? "" : reader.GetString(0);
                    }
                }
                else if (scheduleconfig.config.source == "Employee" && employeeid > 0)
                {
                    using (var reader = baseData.ExecuteReader("Select contactno from employee where id=" + employeeid, null))
                    {
                        if (reader.Read())
                            mobileno = reader.IsDBNull(0) ? "" : reader.GetString(0);
                    }
                }
                else if (scheduleconfig.config.source == "Agent" && customerid > 0)
                {
                    using (var reader = baseData.ExecuteReader("Select agent.contactno from customer inner join agent on agent.id = agentid where customer.id=" + customerid, null))
                    {
                        if (reader.Read())
                            mobileno = reader.IsDBNull(0) ? "" : reader.GetString(0);
                    }
                }
            }

            List<KeyValuePair<string, string>> list = new List<KeyValuePair<string, string>>();
            var jsonobject = Newtonsoft.Json.Linq.JObject.Parse(request);
            SortedList<string, string> attachments = new SortedList<string, string>();
            foreach (var item in jsonobject.Children())
            {
                var key = item.First().Path;
                var value = ((Newtonsoft.Json.Linq.JValue)item.First()).Value.ToString();
                if (value.StartsWith("ATTACHMENT"))
                {
                    var filepart = value.Split(new char[] { '-' });
                    int idx = 0;
                    if (int.TryParse(filepart[1], out idx))
                    {
                        attachments.Add(key, "file-" + idx);
                    }
                }
                else
                {
                    list.Add(new KeyValuePair<string, string>(key, value));
                }
            }

            using (var client = new HttpClient())
            {
                var httprequest = new HttpRequestMessage(scheduleconfig.config.growaasan.httpmode.ToUpper() == "POST" ? HttpMethod.Post : HttpMethod.Get,
                    scheduleconfig.config.url);
                var content = new MultipartFormDataContent();

                foreach (var stringitem in list)
                {
                    if (string.IsNullOrEmpty(stringitem.Value) || stringitem.Value.Trim() == "0")
                        content.Add(new StringContent("-"), stringitem.Key);
                    else
                        content.Add(new StringContent(stringitem.Value), stringitem.Key);
                }
                foreach (var fileitem in attachments)
                {
                    content.Add(new ByteArrayContent(files[fileitem.Value]), fileitem.Key, fileitem.Key + ".pdf");
                }

                //content = new MultipartFormDataContent();
                //String businessid = "101843";
                //String authkey = "eG8ybmt3YnpzU2lRb1c0UFhmRlhaZz09";
                //content.Add(new StringContent(businessid), "businessId");
                //content.Add(new StringContent(authkey), "authKey");
                //content.Add(new StringContent("91"), "country_code");
                //content.Add(new StringContent("9791040132"), "customerMobile");
                //content.Add(new StringContent("2"), "communicationType");
                //content.Add(new StringContent("bill_with_pdf"), "waTemplateName");
                //content.Add(new StringContent("en"), "waTemplateLang");
                //content.Add(new StringContent("document"), "mediaType");
                //content.Add(new StringContent("5"), "varCount");
                //content.Add(new StringContent("customername"), "var1");
                //content.Add(new StringContent("AMB24/974"), "var2");
                //content.Add(new StringContent("2024-07-01"), "var3");
                //content.Add(new StringContent("billamount"), "var4");
                //content.Add(new StringContent("remainingloyaltypoints"), "var5");
                //var filebytes = System.IO.File.ReadAllBytes(@"e:\a.pdf");
                //content.Add(new ByteArrayContent(filebytes), "file", "file.pdf");

                httprequest.Content = content;

                try
                {
                    var response = System.Threading.Tasks.Task.Run(() => client.SendAsync(httprequest)).Result;
                    var responsestring = response.Content.ReadAsStringAsync().Result;
                    SmshistoryDataService smshistoryDataService = new SmshistoryDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
                    smshistoryDataService.Insert(new Smshistory()
                    {
                        actionon = DateTime.Now,
                        mobileno = mobileno,
                        message = JsonConvert.SerializeObject(attachments),
                        response = responsestring,
                        scheduleid = scheduleconfig.id,
                        config = request
                    });
                }
                catch (Exception exp)
                {
                    SmshistoryDataService smshistoryDataService = new SmshistoryDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
                    smshistoryDataService.Insert(new Smshistory()
                    {
                        actionon = DateTime.Now,
                        mobileno = mobileno,
                        message = JsonConvert.SerializeObject(attachments),
                        response = exp.Message,
                        scheduleid = scheduleconfig.id,
                        config = request
                    });
                }
            }
            /*var response = ScheduleManager.Instance.Send2WebAPIAsync(scheduleconfig, request);
            SmshistoryDataService smshistoryDataService = new SmshistoryDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            smshistoryDataService.Insert(response.Result);*/
        }

        private void ExecuteWebAPIItem(long organizationid, Schedule scheduleconfig, QueueItem queueItem)
        {
            if (queueItem.type == SourceType.Bill && queueItem.action == Actions.ECommerce)
            {
                long locationid = 0;
                Common.BaseData baseData = new Common.BaseData() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
                Models.Bill bill = queueItem.source as Models.Bill;
                locationid = bill.locationid;
                if (scheduleconfig.config.companyid > 0 && bill.companyid != scheduleconfig.config.companyid) return;
                if (scheduleconfig.config.locationid > 0 && bill.locationid != scheduleconfig.config.locationid) return;

                BillIntegration source = new BillIntegration(scheduleconfig.config.format);
                source.mode = "sales";
                source.type = bill.billattributes.online ? "ONLINE" : "OFFLINE";
                if (bill.billattributes == null)
                    bill.billattributes = new BillAttribute() { };

                source.bills = new BillIntegration.Bill()
                {
                    billamount = bill.total,
                    billdate = bill.billdate,
                    billno = bill.billno,
                    company = bill.company == null ? "" : bill.company.name,
                    branch = bill.location == null ? "" : bill.location.name,
                    loyeltyissued = bill.billattributes.earnedreward,
                    loyeltyclaimed = bill.billattributes.spentreward,
                    loyeltybalance = bill.billattributes.remainingreward
                };

                if (bill.billattributes.online)
                {
                    source.billitems = bill.Billitems.ConvertAll(x => new BillIntegration.BillItem()
                    {
                        amount = x.amount,
                        barcode = x.barcode,
                        gln = x.attributes == null ? "" : x.attributes.serialno,
                        onlinqty = x.attributes == null ? 0 : x.attributes.newolstock,
                        stockqty = x.attributes == null ? 0 : x.attributes.newstock,
                        soldqty = x.qty
                    });
                }
                else
                {
                    source.billitems = bill.Billitems.FindAll(x => x.attributes.isolstock).ConvertAll(x => new BillIntegration.BillItem()
                    {
                        amount = x.amount,
                        barcode = x.barcode,
                        gln = x.attributes == null ? "" : x.attributes.serialno,
                        onlinqty = x.attributes == null ? 0 : x.attributes.newolstock,
                        stockqty = x.attributes == null ? 0 : x.attributes.newstock,
                        soldqty = x.qty
                    });
                }

                if (InfyPOS.Services.ConfigManager.Instance(organizationid).Config.system_fixed_barcodes &&
                    !InfyPOS.Services.ConfigManager.Instance(organizationid).Config.system_hyper_uan &&
                    source.billitems.Exists(x => !string.IsNullOrEmpty(x.gln)))
                {
                    var glnbillitems = source.billitems.FindAll(x => !string.IsNullOrEmpty(x.gln));
                    SalablegoodsDataService salablegoodsDataService = new SalablegoodsDataService() { Context = new UserClaim() { OrganizationID = organizationid } };
                    glnbillitems = salablegoodsDataService.FindGLNStock(bill.locationid, glnbillitems);
                    source.billitems.RemoveAll(x => !string.IsNullOrEmpty(x.gln));
                    source.billitems.AddRange(glnbillitems);
                }

                source.vouchers = new List<BillIntegration.Vouchers>();
                if (bill.billattributes.coupons != null && bill.billattributes.coupons.Count > 0)
                {
                    foreach (var coupon in bill.billattributes.coupons)
                    {
                        source.vouchers.Add(new BillIntegration.Vouchers()
                        {
                            consumed = coupon.amount,
                            no = coupon.code
                        });
                    }
                }
                source.customer = new BillIntegration.Customer();
                if (bill.customer != null)
                {
                    source.customer.mobileno = bill.customer.mobileno;
                    source.customer.name = bill.customer.name;
                    source.customer.id = bill.customer.loyeltyno;
                }

                if (bill.billattributes.settlement != null &&
                    bill.billattributes.settlement.paymentinfo != null &&
                    bill.billattributes.settlement.paymentinfo.voucherinformation != null)
                {
                    foreach (var coupon in bill.billattributes.settlement.paymentinfo.voucherinformation)
                    {
                        source.vouchers.Add(new BillIntegration.Vouchers()
                        {
                            consumed = coupon.amount,
                            no = coupon.code
                        });
                    }
                }

                var request = source.destinationjson();
                var response = ScheduleManager.Instance.Send2WebAPIAsync(scheduleconfig, request);
                SmshistoryDataService smshistoryDataService = new SmshistoryDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
                smshistoryDataService.Insert(response.Result);
            }
            else if (queueItem.type == SourceType.Bill)
            {
                long locationid = 0;
                Common.BaseData baseData = new Common.BaseData() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
                Models.Bill bill = queueItem.source as Models.Bill;
                if (bill.customer == null) return;
                locationid = bill.locationid;
                if (scheduleconfig.config.companyid > 0 && bill.companyid != scheduleconfig.config.companyid) return;
                if (scheduleconfig.config.locationid > 0 && bill.locationid != scheduleconfig.config.locationid) return;

                BillIntegration source = new BillIntegration(scheduleconfig.config.format);

                if (bill.billattributes == null)
                    bill.billattributes = new BillAttribute() { };

                source.bills = new BillIntegration.Bill()
                {
                    billamount = bill.total,
                    billdate = bill.billdate,
                    billno = bill.billno,
                    billid = bill.id,
                    organizationid = queueItem.organizationid,
                    loyeltyissued = bill.billattributes.earnedreward,
                    loyeltyclaimed = bill.billattributes.spentreward,
                    loyeltybalance = bill.billattributes.remainingreward
                };
                source.bills.company = bill.company == null ? "" : bill.company.name;
                source.bills.branch = bill.location == null ? "" : bill.location.name;
                source.vouchers = new List<BillIntegration.Vouchers>();
                if (bill.billattributes.coupons != null && bill.billattributes.coupons.Count > 0)
                {
                    foreach (var coupon in bill.billattributes.coupons)
                    {
                        source.vouchers.Add(new BillIntegration.Vouchers()
                        {
                            consumed = coupon.amount,
                            no = coupon.code
                        });
                    }
                }

                source.customer = new BillIntegration.Customer();
                source.customer.mobileno = bill.customer.mobileno;
                source.customer.name = bill.customer.name;
                source.customer.id = bill.customer.loyeltyno;

                if (bill.billattributes.settlement != null &&
                    bill.billattributes.settlement.paymentinfo != null &&
                    bill.billattributes.settlement.paymentinfo.voucherinformation != null)
                {
                    foreach (var coupon in bill.billattributes.settlement.paymentinfo.voucherinformation)
                    {
                        source.vouchers.Add(new BillIntegration.Vouchers()
                        {
                            consumed = coupon.amount,
                            no = coupon.code
                        });
                    }
                }

                var request = source.destinationjson();
                var response = ScheduleManager.Instance.Send2WebAPIAsync(scheduleconfig, request);
                SmshistoryDataService smshistoryDataService = new SmshistoryDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
                smshistoryDataService.Insert(response.Result);
            }

            else if (queueItem.type == SourceType.Settlement)
            {
                long locationid = 0;
                Common.BaseData baseData = new Common.BaseData() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
                Models.Billsettlementmaster bill = queueItem.source as Models.Billsettlementmaster;
                if (string.IsNullOrEmpty(bill.customermobileno)) return;
                bill.customer = new Customer()
                {
                    mobileno = bill.customermobileno,
                    name = bill.customername,
                    id = bill.customerid
                };
                
                locationid = bill.locationid;
                if (scheduleconfig.config.companyid > 0 && bill.companyid != scheduleconfig.config.companyid) return;
                if (scheduleconfig.config.locationid > 0 && bill.locationid != scheduleconfig.config.locationid) return;

                BillIntegration source = new BillIntegration(scheduleconfig.config.format);

                source.bills = new BillIntegration.Bill()
                {
                    billamount = bill.billamount,
                    billdate = bill.settlementon,
                    billno = string.Join(",", bill.Billsettlement.ConvertAll(x=>x.billno)),
                    billid = bill.id,
                    organizationid = queueItem.organizationid,
                    loyeltyissued = bill.paymentinfo.reward_earned,
                    loyeltyclaimed = bill.paymentinfo.reward_spent,
                    loyeltybalance = bill.paymentinfo.avlrewardpoint
                };
                source.branch = "";
                source.bills.company = bill.company == null ? "" : bill.company.name;
                source.vouchers = new List<BillIntegration.Vouchers>();
                source.customer = new BillIntegration.Customer();
                source.customer.mobileno = bill.customer.mobileno;
                source.customer.name = bill.customer.name;
                source.customer.id = bill.customer.loyeltyno;
                var request = source.destinationjson();
                var response = ScheduleManager.Instance.Send2WebAPIAsync(scheduleconfig, request);
                SmshistoryDataService smshistoryDataService = new SmshistoryDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
                smshistoryDataService.Insert(response.Result);
            }

            if (queueItem.type == SourceType.Stock && queueItem.action == Actions.ECommerce)
            {

                long locationid = 0;
                Common.BaseData baseData = new Common.BaseData() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
                OnlineStock onlinestock = queueItem.source as OnlineStock;
                locationid = onlinestock.locationid;
                ReferencelistDataService referencelistDataService = new ReferencelistDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
                var branch = referencelistDataService.Search(new ReferencelistCreteria() { id = locationid }).First().code;
                BillIntegration source = new BillIntegration(scheduleconfig.config.format);
                source.mode = onlinestock.fullsync ? "FULLSYNC" : "stock";
                source.type = "STOCK";
                source.bills = new BillIntegration.Bill()
                {
                    branch = branch
                };
                source.billitems = new List<BillIntegration.BillItem>();

                source.billitems = onlinestock.items.ConvertAll(x => new BillIntegration.BillItem()
                {
                    barcode = x.barcode,
                    gln = x.glno,
                    onlinqty = x.olqty,
                    stockqty = x.stockqty
                });

                if (InfyPOS.Services.ConfigManager.Instance(organizationid).Config.system_fixed_barcodes &&
                    !InfyPOS.Services.ConfigManager.Instance(organizationid).Config.system_hyper_uan &&
                    source.billitems.Exists(x => !string.IsNullOrEmpty(x.gln)))
                {
                    var glnbillitems = source.billitems.FindAll(x => !string.IsNullOrEmpty(x.gln));
                    SalablegoodsDataService salablegoodsDataService = new SalablegoodsDataService() { Context = new UserClaim() { OrganizationID = organizationid } };
                    glnbillitems = salablegoodsDataService.FindGLNStock(locationid, glnbillitems);
                    source.billitems.RemoveAll(x => !string.IsNullOrEmpty(x.gln));
                    source.billitems.AddRange(glnbillitems);
                }

                var request = source.destinationjson();
                var response = ScheduleManager.Instance.Send2WebAPIAsync(scheduleconfig, request);
                SmshistoryDataService smshistoryDataService = new SmshistoryDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
                smshistoryDataService.Insert(response.Result);
            }

            if (queueItem.type == SourceType.Customer && queueItem.action == Actions.CRM)
            {
                Common.BaseData baseData = new Common.BaseData() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
                Models.Customer customer = queueItem.source as Models.Customer;
                if (scheduleconfig.config.locationid > 0 && customer.registredlocationid > 0 &&
                    customer.registredlocationid != scheduleconfig.config.locationid) return;

                string request = scheduleconfig.config.format;
                request = request.Replace("[MOBILENO]", customer.mobileno);
                request = request.Replace("[NAME]", customer.name);
                request = request.Replace("[LOYELTY]", customer.loyelty.ToString("F0"));
                var response = ScheduleManager.Instance.Send2WebAPIAsync(scheduleconfig, request);
                SmshistoryDataService smshistoryDataService = new SmshistoryDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
                smshistoryDataService.Insert(response.Result);
            }

            if (queueItem.type == SourceType.Customer && queueItem.action == Actions.CRM_Campaign)
            {
                Common.BaseData baseData = new Common.BaseData() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
                Models.Customer customer = queueItem.source as Models.Customer;
                if (scheduleconfig.config.locationid > 0 && customer.registredlocationid > 0 &&
                    customer.registredlocationid != scheduleconfig.config.locationid) return;

                string request = scheduleconfig.config.format;
                request = request.Replace("[MOBILENO]", customer.mobileno);
                request = request.Replace("[NAME]", customer.name);
                request = request.Replace("[LOYELTYNO]", customer.loyeltyno);
                request = request.Replace("[LOYELTY]", customer.loyelty.ToString("F0"));
                var response = ScheduleManager.Instance.Send2WebAPIAsync(scheduleconfig, request);
                SmshistoryDataService smshistoryDataService = new SmshistoryDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
                smshistoryDataService.Insert(response.Result);
            }

            if (queueItem.type == SourceType.OTP &&
                (queueItem.action == Actions.Otp_Loyelty || queueItem.action == Actions.Otp_Discount))
            {
                Common.BaseData baseData = new Common.BaseData() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
                Models.BillOTP customer = queueItem.source as Models.BillOTP;
                if (scheduleconfig.config.locationid > 0 && customer.locationid > 0 &&
                    customer.locationid != scheduleconfig.config.locationid) return;

                string request = scheduleconfig.config.format;
                request = request.Replace("[MOBILENO]", customer.MOBILE);
                request = request.Replace("[NAME]", customer.CUSTOMER);
                request = request.Replace("[OTP]", customer.OTP);
                request = request.Replace("[BILLAMOUNT]", customer.BILLAMOUNT.ToString("F0"));
                request = request.Replace("[DISCOUNT]", customer.DISCOUNT.ToString("F0"));
                request = request.Replace("[DISCOUNTPERCENT]", customer.DISCOUNTPERCENT.ToString("F0"));
                request = request.Replace("[NETAMOUNT]", customer.NETAMOUNT.ToString("F0"));
                request = request.Replace("[ALLOWEDCREDIT]", customer.ALLOWEDCREDIT.ToString("F0"));
                request = request.Replace("[REASON]", customer.REASON);

                var response = ScheduleManager.Instance.Send2WebAPIAsync(scheduleconfig, request);
                SmshistoryDataService smshistoryDataService = new SmshistoryDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
                smshistoryDataService.Insert(response.Result);
            }

            if (queueItem.type == SourceType.OTP &&
               (queueItem.action == Actions.Otp_Credit))
            {
                Common.BaseData baseData = new Common.BaseData() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
                Models.BillOTP customer = queueItem.source as Models.BillOTP;
                if (scheduleconfig.config.locationid > 0 && customer.locationid > 0 &&
                    customer.locationid != scheduleconfig.config.locationid) return;

                string request = scheduleconfig.config.format;
                request = request.Replace("[MOBILENO]", scheduleconfig.config.sendto);
                request = request.Replace("[OTP]", customer.OTP);
                request = request.Replace("[CUSTOMER]", customer.CUSTOMER);
                request = request.Replace("[NAME]", customer.CUSTOMER);
                request = request.Replace("[BILLAMOUNT]", customer.BILLAMOUNT.ToString("F0"));
                request = request.Replace("[NETAMOUNT]", customer.NETAMOUNT.ToString("F0"));
                var response = ScheduleManager.Instance.Send2WebAPIAsync(scheduleconfig, request);
                SmshistoryDataService smshistoryDataService = new SmshistoryDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
                smshistoryDataService.Insert(response.Result);
            }

            else if (queueItem.type == SourceType.Alteration)
            {
                Common.BaseData baseData = new Common.BaseData() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
                Delivery delivery = queueItem.source as Delivery;

                if (scheduleconfig.config.locationid > 0 && delivery.locationid > 0 &&
                    delivery.locationid != scheduleconfig.config.locationid) return;

                string request = scheduleconfig.config.format;
                if (delivery.customer != null)
                {
                    request = request.Replace("[MOBILENO]", delivery.customer.mobileno);
                    request = request.Replace("[NAME]", delivery.customer.name);
                }
                else
                {
                    request = request.Replace("[MOBILENO]", delivery.contactno);
                    request = request.Replace("[NAME]", delivery.customername);
                }
                request = request.Replace("[QTY]", delivery.qty.ToString());
                request = request.Replace("[DATE]", delivery.billdate.ToString("dd-MM-yyyy"));
                request = request.Replace("[DUEDATE]", delivery.expectedon.ToString("dd-MM-yyyy"));
                request = request.Replace("[ORDERNO]", delivery.orderno);
                request = request.Replace("[ENTRYNO]", delivery.entryno);

                var response = ScheduleManager.Instance.Send2WebAPIAsync(scheduleconfig, request);
                SmshistoryDataService smshistoryDataService = new SmshistoryDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
                smshistoryDataService.Insert(response.Result);
            }
        }

        SortedDictionary<string, SortedDictionary<long, Models.Schedule>> scheduleconfigs = null;

        public void ClearSchedule(long organizationid)
        {
            scheduleconfigs = null;
        }
        public SortedDictionary<long, Models.Schedule> GetSchedule(long organizationid, QueueItem source)
        {
            if (scheduleconfigs == null)
            {
                scheduleconfigs = new SortedDictionary<string, SortedDictionary<long, Models.Schedule>>();
                Data.ScheduleDataService scheduleDataService = new Data.ScheduleDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
                var list = scheduleDataService.Search(new Models.ScheduleCreteria()
                {
                    onlyactionable = true
                });
                foreach (var item in list)
                {
                    if (!scheduleconfigs.ContainsKey(item.name))
                    {
                        scheduleconfigs.Add(item.name, new SortedDictionary<long, Models.Schedule>());
                        scheduleconfigs[item.name].Add(item.id, item);
                    }
                    else
                    {
                        scheduleconfigs[item.name].Add(item.id, item);
                    }
                }
            }

            string schedulename = "";
            switch (source.type)
            {
                case SourceType.OTP:
                    switch (source.action)
                    {
                        case Actions.Otp_Loyelty:
                            schedulename = "LoyeltyOTP";
                            break;
                        case Actions.Otp_Loyelty_Validate:
                            schedulename = "LoyeltyOTPValidate";
                            break;
                        case Actions.Otp_Discount:
                            schedulename = "DiscountOTP";
                            break;
                        case Actions.Otp_Credit:
                            schedulename = "CreditOTP";
                            break;
                    }
                    break;
                case SourceType.Stock:
                    switch (source.action)
                    {
                        case Actions.ECommerce:
                            schedulename = "Bill2ECOM";
                            break;
                    }
                    break;
                case SourceType.Bill:
                    switch (source.action)
                    {
                        case Actions.Cancelled:
                            schedulename = "BillCancel";
                            break;
                        case Actions.Discounted:
                            schedulename = "BillDiscount";
                            break;
                        case Actions.New:
                            schedulename = "BillSave";
                            break;
                        case Actions.Return:
                            schedulename = "BillReturn";
                            break;
                        case Actions.Exchange:
                            schedulename = "BillExchange";
                            break;
                        case Actions.CardPayment:
                            schedulename = "BillCardPayment";
                            break;
                        case Actions.Credit:
                            schedulename = "BillCredit";
                            break;
                        case Actions.CRM:
                            schedulename = "Bill2CRM";
                            break;
                        case Actions.ECommerce:
                            schedulename = "Bill2ECOM";
                            break;
                        case Actions.Publish:
                            schedulename = "BillPublish";
                            break;
                    }
                    break;
                case SourceType.Customer:
                    schedulename = "CustomerCreation";
                    switch (source.action)
                    {
                        case Actions.CRM:
                            schedulename = "CustomerSaveCRM";
                            break;
                        case Actions.CRM_Loyelty:
                            schedulename = "CustomerLoyaltyRequest";
                            break;
                        case Actions.CRM_Voucher:
                            schedulename = "CustomerVoucherRequest";
                            break;
                    }
                    break;
                case SourceType.Employee:
                    schedulename = "EmployeeCreation";
                    break;
                case SourceType.Settlement:
                    schedulename = "BillSettlement";
                    break;
                case SourceType.Alteration:
                    switch (source.action)
                    {
                        case Actions.AlterationReceived:
                            schedulename = "AlterationReceived";
                            break;
                        case Actions.AlterationReady:
                            schedulename = "AlterationReady";
                            break;
                        case Actions.AlterationDelivered:
                            schedulename = "AlterationDelivered";
                            break;
                    }
                    break;
                case SourceType.PurchaseReturn:
                    schedulename = "SupplierDebitNote";
                    break;
                case SourceType.SupplierPayment:
                    schedulename = "SupplierPayment";
                    break;
                case SourceType.PurchaseOrder:
                    switch (source.action)
                    {
                        case Actions.New:
                            schedulename = "SupplierOrder";
                            break;
                        case Actions.Publish:
                            schedulename = "SupplierOrderPublish";
                            break;
                    }
                    break;
            }

            if (string.IsNullOrEmpty(schedulename) || !scheduleconfigs.ContainsKey(schedulename))
            {
                return null;
            }
            return scheduleconfigs[schedulename];
        }

        bool billnotificationlog { get; set; }
        internal void BillNotificationDisabled()
        {
            if (billnotificationlog) return;
            billnotificationlog = true;
            WebAPI.Common.Logger.Instance(0).Log("Bill SMS Notification Disabled", WebAPI.Common.Logger.LogType.Error, "Change Global application setting - Bill Based SMS");
        }


        public class QuantoSMS
        {
            public string message { get; set; }
            public long id { get; set; }
            public long internalid { get; set; }
            public string senderid { get; set; }
            public string smsserverurl { get; set; }
            public string sendmethod { get; set; }
            public string smsconfirmation { get; set; }
            public List<Item> items { get; set; }
            public List<KeyValue> arguments { get; set; }
            public bool isdirect { get; set; }
            public class Item
            {
                public string mobile { get; set; }
                public List<KeyValue> arguments { get; set; }
                public bool hassent { get; set; }
                public string error { get; set; }
            }

            public class KeyValue
            {
                public string key { get; set; }
                public string value { get; set; }
            }

            public voice Voice { get; set; }
            public DateTime scheduleto { get; set; }

            public class voice
            {
                public byte[] content { get; set; }
                public string clipname { get; set; }
                public string lablename { get; set; }
                public string username { get; set; }
                public string password { get; set; }
            }
        }
    }
}