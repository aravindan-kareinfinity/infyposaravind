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
using System.Runtime.Serialization;
using InfyPOS.Processors;

namespace InfyPOS.Controllers
{
    [RoutePrefix("SyncController")]
    public class SyncController : ApiController
    {

        //Windows service -> Offline Client 
        [HttpPost]
        [Route("OfflineClientConfig")]
        public async Task<IHttpActionResult> OfflineClientConfig(CreateRequest request)
        {
            string result = await Task<string>.Run(() => Services.SyncManager.Instance.OfflineClientConfig(request));
            return this.Ok(result);
        }

        //Windows service -> Offline Client 
        [HttpPost]
        [Route("EnableOfflineClient")]
        public async Task<IHttpActionResult> EnableOfflineClient(CreateRequest request)
        {
            string result = await Task<string>.Run(() => Services.SyncManager.Instance.EnableOfflineClient(request));
            return this.Ok(result);
        }


        //Primary Server
        [HttpPost]
        [AllowAnonymous]
        [Route("CreateOfflineClient")]
        public async Task<IHttpActionResult> CreateOfflineClient(CreateRequest request)
        {
            string result = await Task<string>.Run(() => Services.SyncManager.Instance.Create(request));
            return this.Ok(result);
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("ClearOfflineClient")]
        public async Task<IHttpActionResult> ClearOfflineClient(CreateRequest request)
        {
            string result = await Task<string>.Run(() => Services.SyncManager.Instance.ClearOfflineClient(request.organizationid, request));
            return this.Ok(result);
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("TriggerSyncOffline")]
        public async Task<IHttpActionResult> SyncOffline()
        {
            Services.SyncManager.DBDataAvailableSummary result = await Task<Services.SyncManager.DBDataAvailableSummary>.Run(() => Services.SyncManager.Instance.SyncOffline(new CreateRequest() { all = false }));
            return this.Ok(result);
        }
        //Primary Server
        [HttpPost]
        [AllowAnonymous]
        [Route("SyncOffline")]
        public async Task<IHttpActionResult> SyncOffline(CreateRequest request)
        {
            Services.SyncManager.DBDataAvailableSummary result = await Task<Services.SyncManager.DBDataAvailableSummary>.Run(() => Services.SyncManager.Instance.SyncOffline(request));
            return this.Ok(result);
        }


        [HttpPost]
        [AllowAnonymous]
        [Route("RestAutoNumber")]
        public async Task<IHttpActionResult> RestAutoNumber(CreateRequest request)
        {
            bool result = await Task<bool>.Run(() => Services.SyncManager.Instance.RestAutoNumber(request));
            return this.Ok(result);
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("DownloadMasters")]
        public async Task<IHttpActionResult> DownloadMasters(SyncRequest request)
        {
            UsersDataService UsersData = new UsersDataService() { Context = new Common.UserClaim() { OrganizationID = Data.OrganisationDataService.GetOrganizationID(request.organizationcode) } };
            List<Services.SyncManager.DBData> result = await Task<List<Services.SyncManager.DBData>>.Run(() => Services.SyncManager.Instance.DownloadMasters(request));
            return this.Ok(result);
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("DownloadOfflineTransaction")]
        public async Task<IHttpActionResult> DownloadOnetimeMasters(SyncRequest request)
        {
            List<Services.SyncManager.DBData> result = await Task<List<Services.SyncManager.DBData>>.Run(() => Services.SyncManager.Instance.DownloadOfflineTransaction(request));
            return this.Ok(result);
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("DownloadTransactions")]
        public async Task<IHttpActionResult> DownloadTransactions(SyncRequest request)
        {
            List<Services.SyncManager.DBData> result = await Task<List<Services.SyncManager.DBData>>.Run(() => Services.SyncManager.Instance.DownloadTransactions(request));
            return this.Ok(result);
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("UploadBills")]
        public async Task<IHttpActionResult> UploadBills(UploadBillRequest request)
        {
            List<Services.SyncManager.DBData> result = await Task<List<Services.SyncManager.DBData>>.Run(() => Services.SyncManager.Instance.UploadBills(request));
            return this.Ok(result);
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("UploadOfflineTransaction")]
        public async Task<IHttpActionResult> UploadOfflineTransaction(UploadTransactionRequest request)
        {
            List<Services.SyncManager.DBData> result = await Task<List<Services.SyncManager.DBData>>.Run(() => Services.SyncManager.Instance.UploadOfflineTransaction(request));
            return this.Ok(result);
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("CreateSyncPoint")]
        public async Task<IHttpActionResult> CreateSyncPoint(SyncRequest request)
        {
            InfyPOS.Services.Sync_Server sync_Server = new Services.Sync_Server(Data.OrganisationDataService.GetOrganizationID(request.organizationcode), 0, 0);
            Services.SyncManager.DBDataAvailableSummary byteArray = await Task<Services.SyncManager.DBDataAvailableSummary>.Run(() => sync_Server.CreateSyncPoint(request.organizationid, request));
            return this.Ok(byteArray);
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("DownloadServerStatus")]
        public async Task<IHttpActionResult> DownloadServerStatus(SyncRequest request)
        {
            InfyPOS.Services.Sync_Server sync_Server = new Services.Sync_Server(Data.OrganisationDataService.GetOrganizationID(request.organizationcode), 0, 0);
            Services.SyncManager.DBDataAvailableSummary byteArray = await Task<Services.SyncManager.DBDataAvailableSummary>.Run(() => sync_Server.DownloadDataStatus(request.organizationid, request,false));
            return this.Ok(byteArray);
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("DownloadBills")]
        public async Task<IHttpActionResult> DownloadBills(DownloadBillRequest request)
        {
            List<Models.Bill> result = await Task<List<Models.Bill>>.Run(() => Services.SyncManager.Instance.DownloadBills(request));
            return this.Ok(result);
        }

      

        [HttpPost]
        [AllowAnonymous]
        [Route("Initialize")]
        public async Task<IHttpActionResult> Initialize(SyncRequest request)
        {
            bool result = await Task<bool>.Run(() => Services.SyncManager.Instance.Initialize(request));
            return this.Ok(result);
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("ChangeMasterStatus")]
        public async Task<IHttpActionResult> ChangeMasterStatus(SyncStatus request)
        {
            bool result = await Task<bool>.Run(() => Services.SyncManager.Instance.ChangeMasterStatus(request));
            return this.Ok(result);
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("ChangeTransactionStatus")]
        public async Task<IHttpActionResult> ChangeTransactionStatus(SyncStatus request)
        {
            bool result = await Task<bool>.Run(() => Services.SyncManager.Instance.ChangeTransactionStatus(request));
            return this.Ok(result);
        }


       

        [System.Runtime.Serialization.DataContract]
        public class SyncRequest : BaseEntity
        {
            [DataMember]
            public string location { get; set; }
            [DataMember]
            public string clientid { get; set; }
            [DataMember]
            public string privatekey { get; set; }
            [DataMember]
            public bool all { get; set; }
            [DataMember]
            public long organizationid { get; set; }
            public string organizationcode { get;  set; }
        }

        [System.Runtime.Serialization.DataContract]
        public class DownloadBillRequest : SyncRequest
        {
            [DataMember]
            public bool offline { get; set; }
        }

        [System.Runtime.Serialization.DataContract]
        public class UploadBillRequest : SyncRequest
        {
            [DataMember]
            public bool offline { get; set; }
            [DataMember]
            public List<Bill> bills { get; set; }
        }

        [System.Runtime.Serialization.DataContract]
        public class UploadTransactionRequest : SyncRequest
        {
            [DataMember]
            public List<Services.SyncManager.DBData> Transactions { get; set; }
        }

        public class OfflineProgress
        {
            public string status { get; set; }
            public int percentage { get; set; }
            public bool hascompleted { get; set; }
            public string error { get; set; }
        }
        public class CreateRequest : SyncRequest
        {
            [DataMember]
            public string organizationcode { get; set; }

            [DataMember]
            public string mode { get; set; }
            [DataMember]
            public DateTime fromdate { get; set; }
            public int percentage { get; set; }
            public delegate void OnOfflineChangeHandler(OfflineProgress offlineProgress);
            public event OnOfflineChangeHandler OnOfflineChange;
            [Newtonsoft.Json.JsonIgnore]
            public OfflineProgress offlineProgress { get; set; }
            public void RaiseEvent(string status, int percentage)
            {
                RaiseEvent(status, percentage, "");
            }
            public void ShowError(string error)
            {
                RaiseEvent("Error", percentage, error);
            }
            public void RaiseEvent(string status,int percentage,string error)
            {
                this.percentage = percentage;
                if (offlineProgress == null)
                    offlineProgress = new OfflineProgress();

                if(!string.IsNullOrEmpty(error))
                    offlineProgress.error += "\r\n"+ error;
                offlineProgress.status = status;
                offlineProgress.percentage = percentage;
                if (OnOfflineChange != null)
                    OnOfflineChange(offlineProgress);
            }
        }

        public class SyncStatus : SyncRequest
        {
            [DataMember]
            public List<Services.SyncManager.DBData> data { get; set; }
        }

        //Windows offline client
        [HttpPost]
        [AllowAnonymous]
        [Route("DownloadWindowsOffline")]
        public async Task<IHttpActionResult> DownloadWindowsOffline(Processors.OfflineClient.WindowsOfflineRequest request)
        {
            OfflineClient.WindowsOfflineResponse result = await Task<OfflineClient.WindowsOfflineResponse>.Run(() => Services.SyncManager.Instance.DownloadWindowsOffline(request));
            return this.Ok(result);
        }

        //Windows offline client
        [HttpPost]
        [AllowAnonymous]
        [Route("UploadWindowsOffline")]
        public async Task<IHttpActionResult> UploadWindowsOffline(Processors.OfflineClient.WindowsOfflineRequest request)
        {
            OfflineClient.WindowsOfflineResponse result = await Task<OfflineClient.WindowsOfflineResponse>.Run(() => Services.SyncManager.Instance.WindowsOfflineSynch(request));
            return this.Ok(result);
        }

        //Windows offline client
        [HttpGet]
        [AllowAnonymous]
        [Route("UploadWindowsOfflineStatus/{id}")]
        public async Task<IHttpActionResult> UploadWindowsOfflineStatus(string id)
        {
            OfflineClient.WindowsOfflineResponse result = await Task<OfflineClient.WindowsOfflineResponse>.Run(() => Services.SyncManager.Instance.UploadWindowsOfflineStatus(id));
            return this.Ok(result);
        }
    }
}