using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Web;
using InfyPOS.Controllers;
using InfyPOS.Models;
using InfyPOS.Processors;
using Newtonsoft.Json;

namespace InfyPOS.Services
{
    public class SyncManager : IDisposable
    {
        private static SyncManager instance;
        public static SyncManager Instance
        {
            get
            {
                return instance;
            }
        }

        System.Timers.Timer timer = null;
        System.Threading.AutoResetEvent are;
        public void Start()
        {
            timer = new System.Timers.Timer(syncConfiguration.intervel);
            timer.Elapsed += Timer_Elapsed;
            are = new System.Threading.AutoResetEvent(true);
        }

        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (are.WaitOne(10))
            {
                //check masters, bundles, sales
                are.Set();
            }
        }

        public List<DBData> DownloadMasters(SyncController.SyncRequest request)
        {
            Sync_Server sync_Server = new Sync_Server(Data.OrganisationDataService.GetOrganizationID(request.organizationcode), request.clientid, request.location, request.privatekey);
            return sync_Server.DownloadMaster();
        }

        public string Create(SyncController.CreateRequest request)
        {
            Sync_Server sync_Server = new Sync_Server(Data.OrganisationDataService.GetOrganizationID(request.organizationcode), request.clientid, request.location, request.privatekey);
            return sync_Server.Create(request.clientid, request.location, request.privatekey, request.fromdate);
        }

        public List<DBData> DownloadTransactions(SyncController.SyncRequest request)
        {
            Sync_Server sync_Server = new Sync_Server(Data.OrganisationDataService.GetOrganizationID(request.organizationcode), request.clientid, request.location, request.privatekey);
            return sync_Server.DownloadTransaction();
        }

        public List<Models.Bill> DownloadBills(SyncController.DownloadBillRequest request)
        {
            Sync_Server sync_Server = new Sync_Server(Data.OrganisationDataService.GetOrganizationID(request.organizationcode), request.clientid, request.location, request.privatekey);
            return sync_Server.DownloadBills(request.offline, request.all);
        }

        internal string ClearOfflineClient(long organizationid, SyncController.CreateRequest request)
        {
            Sync_Server sync_Server = new Sync_Server(Data.OrganisationDataService.GetOrganizationID(request.organizationcode), request.clientid, request.location, request.privatekey);
            return sync_Server.ClearOfflineClient(organizationid);
        }


        System.Threading.AutoResetEvent resetEvent = new System.Threading.AutoResetEvent(true);
        public Services.SyncManager.DBDataAvailableSummary SyncOffline(SyncController.CreateRequest request)
        {
            if (resetEvent.WaitOne(10))
            {
                try
                {

                    InfyPOS.Services.Sync_Client sync_Client = new InfyPOS.Services.Sync_Client();
                    var result = sync_Client.Sync_OfflineStatus(request.organizationid, request.all).Result;
                    if (result.haschanged())
                    {
                        System.Threading.Thread thread = new System.Threading.Thread(new System.Threading.ParameterizedThreadStart(SyncOffline));
                        thread.Start(new Tuple<SyncController.CreateRequest, DBDataAvailableSummary>(request, result));
                    }
                    return result;
                }
                catch (Exception exp)
                {

                }
                finally
                {
                    resetEvent.Set();
                }
            }
            return null;
        }

        internal bool RestAutoNumber(SyncController.CreateRequest request)
        {
            Common.BaseData baseData = new Common.BaseData();
            baseData.Context = new Common.UserClaim() { OrganizationID = Data.OrganisationDataService.GetOrganizationID( request.organizationcode) };

            SortedDictionary<string, string> tablelist = new SortedDictionary<string, string>();
            using (var reader = baseData.ExecuteReader(@"with sequencelist 
as
(
WITH fq_objects AS (SELECT c.oid,n.nspname || '.' ||c.relname AS fqname , 
                           c.relkind, c.relname AS relation 
                    FROM pg_class c JOIN pg_namespace n ON n.oid = c.relnamespace ),

     sequences AS (SELECT oid,fqname FROM fq_objects WHERE relkind = 'S'),  
     tables    AS (SELECT oid, fqname FROM fq_objects WHERE relkind = 'r' )  
SELECT
       s.fqname AS sequence, 
       '->' as depends, 
       t.fqname AS table 
FROM 
     pg_depend d JOIN sequences s ON s.oid = d.objid  
                 JOIN tables t ON t.oid = d.refobjid  
WHERE 
     d.deptype = 'a'
)
select substring(sequence ,8) as sequencename,""table"" tablename from sequencelist ", null))
            {

                while (reader.Read())
                {
                    if (!tablelist.ContainsKey(reader.GetString(1)))
                    {
                        tablelist.Add(reader.GetString(1), reader.GetString(0));
                    }
                }
            }

            var query = "ALTER SEQUENCE {0} RESTART WITH {1}";
            long lastidentity = 0;
            foreach (var item in tablelist)
            {

                using (var reader = baseData.ExecuteReader("select max(id) from " + item.Key, null))
                {
                    if (reader.Read())
                        lastidentity = reader.IsDBNull(0) ? 0 : reader.GetInt64(0);
                }
                if (lastidentity > 0)
                {
                    baseData.ExecuteQuery(string.Format(query, item.Value, lastidentity + 1), null);
                }
            }

            return true;
        }

        public void CreateSyncPoint(SyncController.CreateRequest request)
        {
            if (resetEvent.WaitOne(10))
            {
                try
                {
                    InfyPOS.Services.Sync_Client sync_Client = new InfyPOS.Services.Sync_Client();

                    System.Threading.Thread thread = new System.Threading.Thread(new System.Threading.ParameterizedThreadStart(CreateSyncPoint));
                    thread.Start(new Tuple<SyncController.CreateRequest, DBDataAvailableSummary>(request, new DBDataAvailableSummary()));

                }
                catch (Exception exp)
                {

                }
                finally
                {
                    resetEvent.Set();
                }
            }
        }

        public void CreateSyncPoint(object request)
        {
            var createRequest = request as Tuple<SyncController.CreateRequest, DBDataAvailableSummary>;
            System.Threading.Thread.Sleep(1000);

            createRequest.Item1.RaiseEvent("Requested", 10);

            InfyPOS.Services.Sync_Client sync_Client = new InfyPOS.Services.Sync_Client();
            var result = sync_Client.CreateSyncPoint(createRequest.Item1);
            if (result != null)
            {
                createRequest.Item1.ShowError(JsonConvert.SerializeObject(result));
            }
            else
            {
                createRequest.Item1.RaiseEvent("Exception", 100);
            }
        }


        private void SyncOffline(object request)
        {
            try
            {
                var createRequest = request as Tuple<SyncController.CreateRequest, DBDataAvailableSummary>;
                System.Threading.Thread.Sleep(1000);
                InfyPOS.Services.Sync_Client sync_Client = new InfyPOS.Services.Sync_Client();

                WebAPI.Common.Logger.Instance(0).Log("Sync Offline Started - All", WebAPI.Common.Logger.LogType.Sync, createRequest.Item2);

                createRequest.Item1.RaiseEvent("Starting..", 0, Newtonsoft.Json.JsonConvert.SerializeObject(createRequest.Item1) + "\r\n" + Newtonsoft.Json.JsonConvert.SerializeObject(createRequest.Item2));

                if (createRequest.Item2.masters > 0)
                {
                    try
                    {
                        createRequest.Item1.RaiseEvent("Master Sync Started", 0);
                        WebAPI.Common.Logger.Instance(0).Log("Sync Offline Started - Master", WebAPI.Common.Logger.LogType.Info, "");
                        var masterstatus = sync_Client.Sync_Masters(createRequest.Item1).Result;
                        WebAPI.Common.Logger.Instance(0).Log("Sync Offline Completed - Master", WebAPI.Common.Logger.LogType.Info, "");
                        createRequest.Item1.RaiseEvent("Master Sync Completed", 25);
                    }
                    catch (Exception exp)
                    {
                        createRequest.Item1.RaiseEvent("Master Sync Failed", 25, exp.Message);
                        WebAPI.Common.Logger.Instance(0).Log("Sync Offline Error - Master " + exp.ToString(), WebAPI.Common.Logger.LogType.Info, "");
                    }
                }
                if (createRequest.Item1.all)
                {
                    try
                    {
                        createRequest.Item1.RaiseEvent("Offline Transaction Started", 30);
                        WebAPI.Common.Logger.Instance(0).Log("Sync Offline Started - OfflineTransaction", WebAPI.Common.Logger.LogType.Info, "");
                        var masterstatus = sync_Client.DownloadOfflineTransaction(createRequest.Item1).Result;
                        WebAPI.Common.Logger.Instance(0).Log("Sync Offline Completed - OfflineTransaction", WebAPI.Common.Logger.LogType.Info, "");
                        createRequest.Item1.RaiseEvent("Offline Transaction Completed", 40);
                    }
                    catch (Exception exp)
                    {
                        createRequest.Item1.RaiseEvent("Offline Transaction Failed", 40, exp.Message);
                        WebAPI.Common.Logger.Instance(0).Log("Sync Offline Error - OfflineTransaction " + exp.ToString(), WebAPI.Common.Logger.LogType.Info, "");
                    }
                }
                if (createRequest.Item2.stocks > 0)
                {
                    try
                    {
                        createRequest.Item1.RaiseEvent("Online Transaction Started", 42);
                        WebAPI.Common.Logger.Instance(0).Log("Sync Offline Started - Transaction", WebAPI.Common.Logger.LogType.Info, "");
                        var transactionstatus = sync_Client.Sync_Transactions(createRequest.Item1).Result;
                        WebAPI.Common.Logger.Instance(0).Log("Sync Offline Completed - Transaction", WebAPI.Common.Logger.LogType.Info, "");
                        createRequest.Item1.RaiseEvent("Online Transaction Completed", 60);
                    }
                    catch (Exception exp)
                    {
                        createRequest.Item1.RaiseEvent("Online Transaction Failed", 60, exp.Message);
                        WebAPI.Common.Logger.Instance(0).Log("Sync Offline Error - Transaction " + exp.ToString(), WebAPI.Common.Logger.LogType.Info, "");
                    }
                }
                if (createRequest.Item2.onlinebills > 0 && createRequest.Item1.mode != "OFFLINE")
                {
                    try
                    {
                        createRequest.Item1.RaiseEvent("Online Bills Started", 62);
                        WebAPI.Common.Logger.Instance(0).Log("Sync Offline Started - Online Bill", WebAPI.Common.Logger.LogType.Info, "");
                        var billstatus = sync_Client.Sync_OnlineBills(createRequest.Item1, createRequest.Item1.all).Result;
                        WebAPI.Common.Logger.Instance(0).Log("Sync Offline Completed - Online Bill", WebAPI.Common.Logger.LogType.Info, "");
                        createRequest.Item1.RaiseEvent("Online Bills Completed", 70);
                    }
                    catch (Exception exp)
                    {
                        createRequest.Item1.RaiseEvent("Online Bills Failed", 70, exp.Message);
                        WebAPI.Common.Logger.Instance(0).Log("Sync Offline Error - Online Bill " + exp.ToString(), WebAPI.Common.Logger.LogType.Info, "");
                    }
                }
                if (createRequest.Item2.offlinebills > 0)
                {
                    try
                    {
                        createRequest.Item1.RaiseEvent("Offline Bills Started", 72);
                        WebAPI.Common.Logger.Instance(0).Log("Sync Offline Started - Offline Bill", WebAPI.Common.Logger.LogType.Info, "");
                        var billstatus = sync_Client.Sync_OfflineBills(createRequest.Item1).Result;
                        WebAPI.Common.Logger.Instance(0).Log("Sync Offline Completed - Offline Bill", WebAPI.Common.Logger.LogType.Info, "");
                        createRequest.Item1.RaiseEvent("Offline Bills Completed", 85);
                    }
                    catch (Exception exp)
                    {
                        createRequest.Item1.RaiseEvent("Offline Bills Failed", 70, exp.Message);
                        WebAPI.Common.Logger.Instance(0).Log("Sync Offline Error - Offline Bill " + exp.ToString(), WebAPI.Common.Logger.LogType.Info, "");
                    }
                }
                //if (createRequest.Item2.offlinetransactions > 0)
                //{
                //    try
                //    {
                //        createRequest.Item1.RaiseEvent("Offline Transaction Started", 88);
                //        WebAPI.Common.Logger.Instance(Context.OrganizationID).Log("Sync Offline Started - Offline Transactions", WebAPI.Common.Logger.LogType.Info, "");
                //        var billstatus = sync_Client.Sync_OfflineTransactions(createRequest.Item1).Result;
                //        WebAPI.Common.Logger.Instance(Context.OrganizationID).Log("Sync Offline Completed - Offline Transactions", WebAPI.Common.Logger.LogType.Info, "");
                //        createRequest.Item1.RaiseEvent("Online Transaction Completed", 98);
                //    }
                //    catch (Exception exp)
                //    {
                //        createRequest.Item1.RaiseEvent("Offline Transaction Failed", 98, exp.Message);
                //        WebAPI.Common.Logger.Instance(Context.OrganizationID).Log("Sync Offline Error - Offline Transactions " + exp.ToString(), WebAPI.Common.Logger.LogType.Info, "");
                //    }
                //}
                createRequest.Item1.RaiseEvent("Online Sync Completed", 100);
                resetEvent.Set();
            }
            catch (Exception exp)
            {
                resetEvent.Set();
            }
        }

        void ProcessMasterDataDownload(object data)
        {
            System.Threading.Thread.Sleep(100);
            var reqres = data as Tuple<OfflineClient.WindowsOfflineRequest, OfflineClient.WindowsOfflineResponse>;
            if(reqres.Item1.organizationid ==0 && !string.IsNullOrEmpty(reqres.Item1.orgainzationcode))
            {
                var org = OrganizationManager.Instance.Get(reqres.Item1.orgainzationcode);
                if(org == null)
                {
                    reqres.Item2.error = true;
                    reqres.Item2.errormessage = "given organization not found";
                    reqres.Item2.completedon = DateTime.Now;
                    reqres.Item2.completed = true;
                    return;
                }
                else
                {
                    reqres.Item1.organizationid = org.id;
                }
            }

            
            Data.UsersDataService usersDataService = new Data.UsersDataService() { Context = new Common.UserClaim() { OrganizationID = reqres.Item1.organizationid } };
            if(usersDataService.IsSAAS && reqres.Item1.organizationid == 0)
            {
                reqres.Item2.error = true;
                reqres.Item2.errormessage = "Please enter the organization details";
                reqres.Item2.completedon = DateTime.Now;
                reqres.Item2.completed = true;
                return;
            }

            bool hasauthenticated = false;
            if (!string.IsNullOrEmpty(reqres.Item1.username) || !string.IsNullOrEmpty(reqres.Item1.password))
            {
                var users = usersDataService.Search(new UsersCreteria()
                {
                    username = reqres.Item1.username,
                    password = reqres.Item1.password,
                    ValidateLogin = true
                });
                if (users.Count == 0)
                {
                    reqres.Item2.error = true;
                    reqres.Item2.errormessage = "Username or password missmatch";
                    reqres.Item2.completedon = DateTime.Now;
                    reqres.Item2.completed = true;
                    return;
                }
                else
                {
                    hasauthenticated = true;
                }
            }

            Data.ReferencelistDataService referencelistDataService = new Data.ReferencelistDataService() { Context = new Common.UserClaim() { OrganizationID = reqres.Item1.organizationid } };
            var locations = referencelistDataService.SearchList(new ReferencelistCreteria()
            { type = "LOCATION" });
            if (!locations.Exists(e => e.code == reqres.Item1.locationcode))
            {
                reqres.Item2.error = true;
                reqres.Item2.errormessage = "Location Code not available";
                reqres.Item2.completedon = DateTime.Now;
                reqres.Item2.completed = true;
                return;
            }
            else
            {
                reqres.Item1.locationid = locations.Find(e => e.code == reqres.Item1.locationcode).id;
            }

            var offlineclients = referencelistDataService.Search(new ReferencelistCreteria() { type = "OFFLINECLIENT",
                code = reqres.Item1.systemkey });
            
            if (!hasauthenticated && offlineclients.Count == 0)
            {
                reqres.Item2.error = true;
                reqres.Item2.errormessage = "This is not the correct from quanto offline pc";
                reqres.Item2.completedon = DateTime.Now;
                reqres.Item2.completed = true;
                return;
            }
            else if(hasauthenticated && offlineclients.Count == 0)
            {
                var lastCreated = referencelistDataService.Insert(new Referencelist()
                {
                    type = "OFFLINECLIENT",
                    code = reqres.Item1.systemkey,
                    name = "Offline client (Auto)",
                    isactive = true
                });
                if(LicenseManager.Instance(reqres.Item1.organizationid).GetLicense().noofoffline != 0)
                {
                    offlineclients = referencelistDataService.Search(new ReferencelistCreteria()
                    {
                        type = "OFFLINECLIENT"
                    });
                    if(offlineclients.Count> LicenseManager.Instance(reqres.Item1.organizationid).GetLicense().noofoffline)
                    {
                        reqres.Item2.error = true;
                        reqres.Item2.errormessage = "Maximum licensed clients reached";
                        reqres.Item2.completedon = DateTime.Now;
                        reqres.Item2.completed = true;
                        return;
                    }
                }
            }

            InfyPOS.Processors.OfflineManager offlineManager = new OfflineManager(reqres.Item1.locationid);
            offlineManager.Context = new Common.UserClaim() { OrganizationID = reqres.Item1.organizationid };
            OfflineClient.MasterData offlinedata = offlineManager.CreateMasterData(reqres.Item1.datafrom, reqres.Item1.zerostock, reqres.Item1.zerostockfrom);
            string json = JsonConvert.SerializeObject(offlinedata);
            reqres.Item2.data = System.Text.Encoding.UTF8.GetBytes(json);
            reqres.Item2.completedon = DateTime.Now;
            reqres.Item2.completed = true;
        }
        internal OfflineClient.WindowsOfflineResponse DownloadWindowsOffline(OfflineClient.WindowsOfflineRequest request)
        {
            if (WindowsOfflineList == null) WindowsOfflineList = new System.Collections.Concurrent.ConcurrentDictionary<string, OfflineClient.WindowsOfflineResponse>();
            OfflineClient.WindowsOfflineResponse offlineResponse = new OfflineClient.WindowsOfflineResponse()
            {
                key = Guid.NewGuid().ToString()
            };
            WindowsOfflineList.TryAdd(offlineResponse.key, offlineResponse);
            if (WindowsOfflineList.Count > 50)
            {
                foreach(var key in  WindowsOfflineList.Keys.ToList())
                {
                    if(WindowsOfflineList.ContainsKey(key))
                    {
                        OfflineClient.WindowsOfflineResponse outvalue = null;
                        if(WindowsOfflineList.TryGetValue(key, out outvalue))
                        {
                            if (outvalue.hasexpired())
                                WindowsOfflineList.TryRemove(key, out outvalue);
                        }
                    }
                }
            }
            System.Threading.Thread thread = new System.Threading.Thread(new System.Threading.ParameterizedThreadStart(ProcessMasterDataDownload));
            thread.Start(new Tuple<OfflineClient.WindowsOfflineRequest, OfflineClient.WindowsOfflineResponse>(request, offlineResponse));
            return offlineResponse;
        }

        internal OfflineClient.WindowsOfflineResponse UploadWindowsOfflineStatus(string id)
        {
            if (!WindowsOfflineList.ContainsKey(id)) return new OfflineClient.WindowsOfflineResponse() { error = true, errormessage = "Request not dound" };
            OfflineClient.WindowsOfflineResponse windowsOfflineResponse = null;
            if(!WindowsOfflineList.TryGetValue(id,out windowsOfflineResponse)) return new OfflineClient.WindowsOfflineResponse() { error = true, errormessage = "Request not dound" };
            lock (windowsOfflineResponse.lockobject)
            {
                return windowsOfflineResponse;
            }
        }

        public void ProcessOfflineData(object data)
        {
            System.Threading.Thread.Sleep(100);
            var reqres = data as Tuple<OfflineClient.WindowsOfflineRequest, OfflineClient.WindowsOfflineResponse>;
            InfyPOS.Processors.OfflineManager offlineManager = new OfflineManager(reqres.Item1.locationid) { Context = new Common.UserClaim { OrganizationID = reqres.Item1.organizationid } };
            if (reqres.Item1.datatype == "BILL")
                offlineManager.SynchronizeBill(reqres.Item1, reqres.Item2);
            else if (reqres.Item1.datatype == "SETTLEMENT")
                offlineManager.SynchronizeSettlement(reqres.Item1, reqres.Item2);
            else if (reqres.Item1.datatype == "STOCK")
                offlineManager.SynchronizeStock(reqres.Item1, reqres.Item2);
        }

        System.Collections.Concurrent.ConcurrentDictionary<string, OfflineClient.WindowsOfflineResponse> WindowsOfflineList = null;
        internal OfflineClient.WindowsOfflineResponse WindowsOfflineSynch(OfflineClient.WindowsOfflineRequest request)
        {
            if (WindowsOfflineList == null) WindowsOfflineList = new System.Collections.Concurrent.ConcurrentDictionary<string, OfflineClient.WindowsOfflineResponse>();
            OfflineClient.WindowsOfflineResponse offlineResponse = new OfflineClient.WindowsOfflineResponse()
            {
                key = Guid.NewGuid().ToString(),
                lockobject = new object()
            };
            if(!WindowsOfflineList.TryAdd(offlineResponse.key, offlineResponse))
            {
                offlineResponse.error = true;
                offlineResponse.completed = true;
                offlineResponse.errormessage = "Already scheduled";
                return offlineResponse;
            }
            if(WindowsOfflineList.Count>50)
            {
                OfflineClient.WindowsOfflineResponse outvalue;
                var list = WindowsOfflineList.Where(e => e.Value.completed && DateTime.Now.Subtract(e.Value.completedon).TotalMinutes > 15).ToList();
                foreach (var item in list)
                    WindowsOfflineList.TryRemove(item.Key, out outvalue);
            }
            System.Threading.Thread thread = new System.Threading.Thread(new System.Threading.ParameterizedThreadStart(ProcessOfflineData));
            thread.Start(new Tuple<OfflineClient.WindowsOfflineRequest, OfflineClient.WindowsOfflineResponse>(request, offlineResponse));
            return offlineResponse;
        }

        internal List<DBData> DownloadOfflineTransaction(SyncController.SyncRequest request)
        {
            Sync_Server sync_Server = new Sync_Server(Data.OrganisationDataService.GetOrganizationID(request.organizationcode), request.clientid, request.location, request.privatekey);
            return sync_Server.DownloadOfflineTransaction(true);
        }

        internal List<DBData> UploadOfflineTransaction(SyncController.UploadTransactionRequest request)
        {

            Sync_Server sync_Server = new Sync_Server(Data.OrganisationDataService.GetOrganizationID(request.organizationcode),request.clientid, request.location, request.privatekey);
            return sync_Server.UploadOfflineTransaction(request.Transactions);
        }

        public bool Initialize(SyncController.SyncRequest request)
        {
            Sync_Server sync_Server = new Sync_Server(Data.OrganisationDataService.GetOrganizationID(request.organizationcode), request.clientid, request.location, request.privatekey);
            return sync_Server.Initialize();
        }

        public List<Services.SyncManager.DBData> UploadBills(SyncController.UploadBillRequest request)
        {
            Sync_Server sync_Server = new Sync_Server(Data.OrganisationDataService.GetOrganizationID(request.organizationcode), request.clientid, request.location, request.privatekey);
            return sync_Server.UploadBills(request);
        }

        public bool ChangeMasterStatus(SyncController.SyncStatus request)
        {
            Sync_Server sync_Server = new Sync_Server(Data.OrganisationDataService.GetOrganizationID(request.organizationcode), request.clientid, request.location, request.privatekey);
            return sync_Server.ChangeMasterStatus(request.data);
        }

        CacheManager cacheManager;
        SyncConfiguration syncConfiguration;

        public string OfflineClientConfig(SyncController.CreateRequest request)
        {
            InfyPOS.Services.Sync_Client sync_Client = new InfyPOS.Services.Sync_Client();
            return sync_Client.CreateConfig(request.location, request.clientid, request.fromdate);
        }


        public void RefreshConfig()
        {
            syncConfiguration = SyncConfiguration.Create();
        }
        public SyncConfiguration Configuration
        {
            get
            {
                return syncConfiguration;
            }
        }


        public string ClearOfflineData(SyncController.CreateRequest creteria)
        {
            InfyPOS.Services.Sync_Client sync_Client = new InfyPOS.Services.Sync_Client();
            return sync_Client.ClearOfflineData().Result;
        }

        public string EnableOfflineClient(SyncController.CreateRequest request)
        {
            InfyPOS.Services.Sync_Client sync_Client = new InfyPOS.Services.Sync_Client();
            return sync_Client.Sync_Initialize().Result;
        }

        private SyncManager()
        {
            cacheManager = CacheManager.Instance(0);
            syncConfiguration = SyncConfiguration.Create();
        }

        public class SyncConfiguration
        {
            [Newtonsoft.Json.JsonIgnoreAttribute]
            public string PrivateKey { get; set; }
            public DateTime fromdate { get; set; }
            public string Server { get; set; }
            public string Location { get; set; }
            public string Client { get; set; }
            public string Message { get; set; }
            public double intervel { get; set; }
            public bool Enabled { get; set; }

            public string mode { get; set; }

            public static SyncConfiguration Create()
            {
                string licefile = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Synchronization.Config");
                try
                {
                    SyncConfiguration syncConfiguration = new SyncConfiguration();
                    if (!System.IO.File.Exists(licefile))
                    {
                        syncConfiguration.Message = ("Can't find the license file @ " + licefile);
                        syncConfiguration.Client = System.Configuration.ConfigurationManager.AppSettings["Client"];
                        syncConfiguration.Location = System.Configuration.ConfigurationManager.AppSettings["Location"];
                        DateTime fromdate;
                        if (!DateTime.TryParse(System.Configuration.ConfigurationManager.AppSettings["InstalledOn"], out fromdate))
                            fromdate = DateTime.Now.Date.AddMonths(-3);
                        syncConfiguration.fromdate = fromdate;
                        syncConfiguration.Enabled = true;
                        syncConfiguration.Server = System.Configuration.ConfigurationManager.AppSettings["Server"];
                        syncConfiguration.PrivateKey = LicenseManager.License.PrivateKey;
                        return syncConfiguration;
                    }
                    string json = System.IO.File.ReadAllText(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Synchronization.Config"));
                    if (!string.IsNullOrEmpty(json) && json != "null")
                    {
                        syncConfiguration = Newtonsoft.Json.JsonConvert.DeserializeObject<SyncConfiguration>(json);
                        syncConfiguration.Enabled = true;
                    }
                    syncConfiguration.PrivateKey = LicenseManager.License.PrivateKey;
                    syncConfiguration.mode = System.Configuration.ConfigurationManager.AppSettings["SyncMode"];
                    return syncConfiguration;
                }
                catch (Exception exp)
                {
                    SyncConfiguration syncConfiguration = new SyncConfiguration();
                    syncConfiguration.Message = (exp.Message + " - " + licefile);
                    syncConfiguration.PrivateKey = LicenseManager.License.PrivateKey;
                    return syncConfiguration;
                }

            }
        }

        public bool ChangeTransactionStatus(SyncController.SyncStatus request)
        {
            Sync_Server sync_Server = new Sync_Server(Data.OrganisationDataService.GetOrganizationID(request.organizationcode), request.clientid, request.location, request.privatekey);
            return sync_Server.ChangeTransactionStatus(request.data);
        }

        static SyncManager()
        {
            instance = new SyncManager();
        }

        public void Dispose()
        {

        }

        public enum ServerMasters
        {
            configuration = 0,
            file = 1,
            referencelist = 2,
            tax = 3,
            products = 4,
            items = 5,
            employee = 6,
            users = 7,
            company = 8,
            promotions = 9,
            schemebarcodes = 10,
            schemeexcludedbarcodes = 11,
            stockmarker = 12,
            supplier = 13,
            suppliercompany = 14,
            brand = 15,
            transport = 16
        }

        public enum AllTransaction
        {
            stock = 100,
            salablegoods = 101,
            bill = 200,
            billsettlementmaster = 201,
            customerinvoice = 202,
            customerrewards = 500,
            vouchers = 501,
            customer = 502,
            billitems = 700,
            billsettlement = 701
        }
        public enum ServerTransaction
        {
            stock = 100,
            salablegoods = 101
        }



        public enum BillTransaction
        {
            bill = 200
        }

        public enum ClientTransaction
        {
            customerinvoice = 202,
            customerrewards = 500,
            vouchers = 501,
            customer = 502,
            autonumber = 503
        }

        public class DBDataAvailable
        {
            public int type { get; set; }
            public string tablename { get; set; }
            public int available { get; set; }
        }

        public class DBDataAvailableSummary
        {
            public int masters { get; set; }
            public int onlinebills { get; set; }
            public int stocks { get; set; }
            public int offlinebills { get; set; }

            public int offlinetransactions { get; set; }
            public bool haschanged()
            {
                return masters > 0 || onlinebills > 0 || stocks > 0 || offlinebills > 0 || offlinetransactions > 0;
            }
        }


        public class ProcessStatus
        {
            public string key { get; set; }
            public int percentage { get; set; }
            public bool hascompleted { get; set; }
            public bool hasfailed { get; set; }
            public string exception { get; set; }

            public List<DBData> result { get; set; }
            [JsonIgnore]
            public BI_Server.DownloadList parameter { get;  set; }
        }

        public class DBData
        {
            public int type { get; set; }
            public string tablename { get; set; }
            public string data { get; set; }
            public byte[] datas { get; set; }
            public List<RowStatus> status { get; set; }

            public List<List<RowStatus>> GetPartionedStatus(int length)
            {
                List<List<RowStatus>> rowStatuses = new List<List<RowStatus>>();
                int startindex = 0;
                while (true)
                {
                    if (startindex + length >= status.Count)
                    {
                        rowStatuses.Add(status.GetRange(startindex, status.Count - startindex));
                        break;
                    }
                    else
                    {
                        rowStatuses.Add(status.GetRange(startindex, length));
                        startindex += length;
                    }
                }
                return rowStatuses;
            }
            public System.Data.DataTable GetTable()
            {
                if (datas != null)
                {
                    return Processors.DataTableCustomFormatter.Deserialize(datas, true);
                    //JsonSerializer serializer = new JsonSerializer();
                    //serializer.DateFormatHandling = Newtonsoft.Json.DateFormatHandling.IsoDateFormat;
                    //serializer.Converters.Add(new Newtonsoft.Json.Converters.JavaScriptDateTimeConverter()
                    //{
                    //});
                    //serializer.NullValueHandling = NullValueHandling.Ignore;
                    //using (var ms = new MemoryStream(datas))
                    //{
                    //    GZipStream gZipStream = new GZipStream(ms, CompressionMode.Decompress);
                    //    using (MemoryStream msout = new MemoryStream())
                    //    {
                    //        gZipStream.CopyTo(msout);
                    //        msout.Seek(0, SeekOrigin.Begin);
                    //        return JsonHelpers.CreateFromJsonStream<System.Data.DataTable>(msout);
                    //    }

                    //}
                }
                return Newtonsoft.Json.JsonConvert.DeserializeObject<System.Data.DataTable>(data, new Newtonsoft.Json.JsonSerializerSettings()
                {
                    DateFormatHandling = Newtonsoft.Json.DateFormatHandling.IsoDateFormat
                });
            }
            public string GetStatusJSON(List<RowStatus> status)
            {
                return Newtonsoft.Json.JsonConvert.SerializeObject(status, new Newtonsoft.Json.JsonSerializerSettings()
                {
                    DateFormatHandling = Newtonsoft.Json.DateFormatHandling.IsoDateFormat
                });
            }
            public string GetStatusJSON()
            {
                return Newtonsoft.Json.JsonConvert.SerializeObject(status, new Newtonsoft.Json.JsonSerializerSettings()
                {
                    DateFormatHandling = Newtonsoft.Json.DateFormatHandling.IsoDateFormat
                });
            }
            public class RowStatus
            {
                public long id { get; set; }
                public int version { get; set; }
                public string code { get; set; }
            }

            public void Sync_Masters()
            {

            }

        }

    }








    public static class JsonHelpers
    {
        public static T CreateFromJsonStream<T>(this Stream stream)
        {
            JsonSerializer serializer = new JsonSerializer();
            serializer.DateFormatHandling = Newtonsoft.Json.DateFormatHandling.IsoDateFormat;
            serializer.Converters.Add(new Newtonsoft.Json.Converters.JavaScriptDateTimeConverter()
            {
            });
            serializer.NullValueHandling = NullValueHandling.Ignore;
            T data;
            using (StreamReader streamReader = new StreamReader(stream))
            {
                data = (T)serializer.Deserialize(streamReader, typeof(T));
            }
            return data;
        }

        public static T CreateFromJsonString<T>(this String json)
        {
            T data;
            using (MemoryStream stream = new MemoryStream(System.Text.Encoding.Default.GetBytes(json)))
            {
                data = CreateFromJsonStream<T>(stream);
            }
            return data;
        }

        public static T CreateFromJsonFile<T>(this String fileName)
        {
            T data;
            using (FileStream fileStream = new FileStream(fileName, FileMode.Open))
            {
                data = CreateFromJsonStream<T>(fileStream);
            }
            return data;
        }
    }
}