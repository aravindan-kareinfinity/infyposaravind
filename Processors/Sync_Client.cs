using InfyPOS.Common;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using System.Net.Http;
using static InfyPOS.Services.SyncManager;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.IO;
using InfyPOS.Controllers;

namespace InfyPOS.Services
{
    public class Sync_Client : SAASData
    {
        public string CreateConfig(string location, string clientid, DateTime fromdate)
        {
            return CreateConfig(location, clientid, fromdate, "");
        }
        public string CreateConfig(string location, string clientid, DateTime fromdate, string server)
        {
            SyncConfiguration syncConfiguration = new SyncConfiguration(); ;
            syncConfiguration.PrivateKey = LicenseManager.License.PrivateKey;
            syncConfiguration.fromdate = fromdate;
            syncConfiguration.Location = location;
            syncConfiguration.Client = clientid;
            syncConfiguration.Server = server;
            return Newtonsoft.Json.JsonConvert.SerializeObject(syncConfiguration);
        }

        public async Task<string> Sync_Initialize()
        {
            var creteria = new Controllers.SyncController.CreateRequest();
            creteria.clientid = SyncManager.Instance.Configuration.Client;
            creteria.location = SyncManager.Instance.Configuration.Location;
            creteria.privatekey = LicenseManager.License.PrivateKey;
            creteria.fromdate = SyncManager.Instance.Configuration.fromdate;

            if (string.IsNullOrEmpty(SyncManager.Instance.Configuration.Client))
            {
                return "Client info not available in web.config";
            }

            if (string.IsNullOrEmpty(SyncManager.Instance.Configuration.Location))
            {
                return "Location info not available in web.config";
            }

            if (string.IsNullOrEmpty(SyncManager.Instance.Configuration.Server))
            {
                return "Server info not available in web.config";
            }

            string URI = SyncManager.Instance.Configuration.Server + "/SyncController";
            using (var client = new HttpClient()
            {
                Timeout = new TimeSpan(0, 10, 0)
            })
            {
                var serializedProduct = JsonConvert.SerializeObject(creteria);
                var content = new StringContent(serializedProduct, System.Text.Encoding.UTF8, "application/json");


                var result = Task.Run(() => client.PostAsync(URI + "/CreateOfflineClient", content)).Result;
                if (result.IsSuccessStatusCode)
                {
                    string json = await result.Content.ReadAsStringAsync();
                    var masterData = JsonConvert.DeserializeObject<string>(json);
                    return masterData;
                }
            }
            return "Offline successfully enabled.";
        }


        public async Task<bool> Sync_OfflineTransactions()
        {
            return await Sync_OfflineTransactions(null);
        }
        public async Task<bool> Sync_OfflineTransactions(SyncController.CreateRequest item)
        {
            
            Sync_Server sync_Server = new Sync_Server(Data.OrganisationDataService.GetOrganizationID(item.organizationcode),SyncManager.Instance.Configuration.Client, SyncManager.Instance.Configuration.Location,
                SyncManager.Instance.Configuration.PrivateKey);
            var offlinebills = sync_Server.DownloadOfflineTransaction(false);

            if (offlinebills.Count > 0)
            {
                var creteria = new Controllers.SyncController.UploadTransactionRequest();
                creteria.clientid = SyncManager.Instance.Configuration.Client;
                creteria.location = SyncManager.Instance.Configuration.Location;
                creteria.privatekey = LicenseManager.License.PrivateKey;
                creteria.Transactions = offlinebills;

                if (string.IsNullOrEmpty(SyncManager.Instance.Configuration.Server))
                    SyncManager.Instance.Configuration.Server = @"http://localhost/InfyPOS";
                string URI = SyncManager.Instance.Configuration.Server + "/SyncController";
                using (var client = new HttpClient()
                {
                    Timeout = new TimeSpan(0, 10, 0)
                })
                {
                    var serializedProduct = JsonConvert.SerializeObject(creteria);
                    var content = new StringContent(serializedProduct, System.Text.Encoding.UTF8, "application/json");


                    var result = Task.Run(() => client.PostAsync(URI + "/UploadOfflineTransaction", content)).Result;
                    if (result.IsSuccessStatusCode)
                    {
                        string json = await result.Content.ReadAsStringAsync();
                        var masterData = JsonConvert.DeserializeObject<List<Services.SyncManager.DBData>>(json);
                        return sync_Server.ChangeTransactionStatus(masterData);
                    }
                    else
                    {
                        if (item != null)
                        {
                            string json = await result.Content.ReadAsStringAsync();
                            item.ShowError(json);
                        }
                    }
                }
            }
            return true;
        }

        public async Task<bool> Sync_OfflineBills()
        {
            return await Sync_OfflineBills(null);
        }
        public async Task<bool> Sync_OfflineBills(SyncController.CreateRequest item)
        {
            WebAPI.Common.Logger.Instance(Context.OrganizationID).Log("Sync Offline Started - Offline Bill", WebAPI.Common.Logger.LogType.Sync, "");

            Sync_Server sync_Server = new Sync_Server(Data.OrganisationDataService.GetOrganizationID(item.organizationcode), SyncManager.Instance.Configuration.Client, SyncManager.Instance.Configuration.Location,
                SyncManager.Instance.Configuration.PrivateKey);
            var offlinebills = sync_Server.DownloadBills(true, false);

            WebAPI.Common.Logger.Instance(Context.OrganizationID).Log("Sync Offline Started - Offline Bill Counts", WebAPI.Common.Logger.LogType.Sync, offlinebills.Count);
            if (offlinebills.Count > 0)
            {
                var creteria = new Controllers.SyncController.UploadBillRequest();
                creteria.clientid = SyncManager.Instance.Configuration.Client;
                creteria.location = SyncManager.Instance.Configuration.Location;
                creteria.privatekey = LicenseManager.License.PrivateKey;
                creteria.bills = offlinebills;
                creteria.offline = true;
                if (string.IsNullOrEmpty(SyncManager.Instance.Configuration.Server))
                    SyncManager.Instance.Configuration.Server = @"http://localhost/InfyPOS";
                string URI = SyncManager.Instance.Configuration.Server + "/SyncController";
                using (var client = new HttpClient()
                {
                    Timeout = new TimeSpan(0, 10, 0)
                })
                {
                    var serializedProduct = JsonConvert.SerializeObject(creteria);
                    var content = new StringContent(serializedProduct, System.Text.Encoding.UTF8, "application/json");

                    var result = Task.Run(() => client.PostAsync(URI + "/UploadBills", content)).Result;
                    if (result.IsSuccessStatusCode)
                    {
                        string json = await result.Content.ReadAsStringAsync();
                        var masterData = JsonConvert.DeserializeObject<List<Services.SyncManager.DBData>>(json);

                        WebAPI.Common.Logger.Instance(Context.OrganizationID).Log("Sync Offline Started - Offline Bill Upload count", WebAPI.Common.Logger.LogType.Sync, masterData.Count);


                        return sync_Server.ChangeTransactionStatus(masterData);
                    }
                    else
                    {
                        if (item != null)
                        {
                            string json = await result.Content.ReadAsStringAsync();
                            WebAPI.Common.Logger.Instance(Context.OrganizationID).Log("Sync Offline Started - Offline Bill Upload Error", WebAPI.Common.Logger.LogType.Sync, json);
                            item.ShowError(json);
                        }
                    }
                }
            }
            return true;
        }

        public async Task<bool> Sync_OnlineBills(bool allbills)
        {
            return await Sync_OnlineBills(null, allbills);
        }
        public async Task<bool> Sync_OnlineBills(SyncController.CreateRequest item, bool allbills)
        {
            Sync_Server sync_Server = new Sync_Server(Data.OrganisationDataService.GetOrganizationID(item.organizationcode), SyncManager.Instance.Configuration.Client, SyncManager.Instance.Configuration.Location,
                SyncManager.Instance.Configuration.PrivateKey);

            var creteria = new Controllers.SyncController.DownloadBillRequest();
            creteria.clientid = SyncManager.Instance.Configuration.Client;
            creteria.location = SyncManager.Instance.Configuration.Location;
            creteria.privatekey = LicenseManager.License.PrivateKey;
            creteria.offline = false;
            creteria.all = allbills;

            if (string.IsNullOrEmpty(SyncManager.Instance.Configuration.Server))
                SyncManager.Instance.Configuration.Server = @"http://localhost/InfyPOS";
            string URI = SyncManager.Instance.Configuration.Server + "/SyncController";
            using (var client = new HttpClient()
            {
                Timeout = new TimeSpan(0, 10, 0)
            })
            {
                var serializedProduct = JsonConvert.SerializeObject(creteria);
                var content = new StringContent(serializedProduct, System.Text.Encoding.UTF8, "application/json");
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log("Sync Offline - OnlineBills Downloading", WebAPI.Common.Logger.LogType.Sync, content);
                var result = Task.Run(() => client.PostAsync(URI + "/DownloadBills", content)).Result;
                if (result.IsSuccessStatusCode)
                {
                    string json = await result.Content.ReadAsStringAsync();
                    var masterData = JsonConvert.DeserializeObject<List<Models.Bill>>(json);
                    if (masterData != null && masterData.Count > 0)
                    {
                        WebAPI.Common.Logger.Instance(Context.OrganizationID).Log("Sync Offline - OnlineBills Download billcount", WebAPI.Common.Logger.LogType.Sync, masterData.Count);
                        var statusData = sync_Server.UploadBills(new SyncController.UploadBillRequest()
                        {
                            bills = masterData,
                            offline = false,
                            location = SyncManager.Instance.Configuration.Location,
                            clientid = SyncManager.Instance.Configuration.Client
                        });
                        WebAPI.Common.Logger.Instance(Context.OrganizationID).Log("Sync Offline - OnlineBills updated count", WebAPI.Common.Logger.LogType.Sync, statusData.Count);
                        return await Sync_TransactionStatus(statusData);
                    }
                }
                else
                {
                    if (item != null)
                    {
                        string json = await result.Content.ReadAsStringAsync();
                        item.ShowError(json);
                        WebAPI.Common.Logger.Instance(Context.OrganizationID).Log("Sync Offline - OnlineBills ", WebAPI.Common.Logger.LogType.Error, json);
                    }
                }
            }
            return true;
        }

        public async Task<bool> DownloadOfflineTransaction()
        {
            return await DownloadOfflineTransaction(null);
        }
        public async Task<bool> DownloadOfflineTransaction(SyncController.CreateRequest item)
        {
            var creteria = new Controllers.SyncController.SyncRequest();
            creteria.clientid = SyncManager.Instance.Configuration.Client;
            creteria.location = SyncManager.Instance.Configuration.Location;
            creteria.privatekey = LicenseManager.License.PrivateKey;
            if (string.IsNullOrEmpty(SyncManager.Instance.Configuration.Server))
                SyncManager.Instance.Configuration.Server = @"http://localhost/InfyPOS";
            string URI = SyncManager.Instance.Configuration.Server + "/SyncController";
            using (var client = new HttpClient()
            {
                Timeout = new TimeSpan(0, 10, 0)
            })
            {
                var serializedProduct = JsonConvert.SerializeObject(creteria);
                var content = new StringContent(serializedProduct, System.Text.Encoding.UTF8, "application/json");


                var result = Task.Run(() => client.PostAsync(URI + "/DownloadOfflineTransaction", content)).Result;
                if (result.IsSuccessStatusCode)
                {
                    string json = await result.Content.ReadAsStringAsync();
                    var masterData = JsonConvert.DeserializeObject<List<Services.SyncManager.DBData>>(json);
                    foreach (var table in masterData)
                    {
                        var datatable = table.GetTable();
                        try
                        {
                            Sync_Masters(datatable, table.tablename);
                        }
                        catch (Exception exp)
                        {
                            WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);
                        }
                        table.status = new List<DBData.RowStatus>();
                        foreach (System.Data.DataRow dr in datatable.Rows)
                        {
                            table.status.Add(new DBData.RowStatus()
                            {
                                code = "",
                                id = dr.IsNull("id") ? -1 : Convert.ToInt64(dr["id"]),
                                version = dr.IsNull("version") ? -1 : Convert.ToInt32(dr["version"])
                            });
                        }
                        table.data = "";
                        table.status.RemoveAll(e => e.id == -1 || e.version == -1);
                    }
                    return await Sync_TransactionStatus(masterData);
                }
                else
                {
                    if (item != null)
                    {
                        string json = await result.Content.ReadAsStringAsync();
                        item.ShowError(json);
                    }
                }
            }
            return true;
        }

        public async Task<bool> Sync_Masters()
        {
            return await Sync_Masters(null);
        }
        public async Task<bool> Sync_Masters(SyncController.CreateRequest item)
        {
            var creteria = new Controllers.SyncController.SyncRequest();
            creteria.clientid = SyncManager.Instance.Configuration.Client;
            creteria.location = SyncManager.Instance.Configuration.Location;
            creteria.privatekey = LicenseManager.License.PrivateKey;
            if (string.IsNullOrEmpty(SyncManager.Instance.Configuration.Server))
                SyncManager.Instance.Configuration.Server = @"http://localhost/InfyPOS";
            string URI = SyncManager.Instance.Configuration.Server + "/SyncController";
            using (var client = new HttpClient()
            {
                Timeout = new TimeSpan(0, 10, 0)
            })
            {
                var serializedProduct = JsonConvert.SerializeObject(creteria);
                var content = new StringContent(serializedProduct, System.Text.Encoding.UTF8, "application/json");



                var result = Task.Run(() => client.PostAsync(URI + "/DownloadMasters", content)).Result;
                if (result.IsSuccessStatusCode)
                {
                    string json = await result.Content.ReadAsStringAsync();
                    var masterData = JsonConvert.DeserializeObject<List<Services.SyncManager.DBData>>(json);
                    foreach (var table in masterData)
                    {
                        var datatable = table.GetTable();
                        try
                        {
                            Sync_Masters(datatable, table.tablename);
                        }
                        catch (Exception exp)
                        {
                            WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);
                        }
                        table.status = new List<DBData.RowStatus>();
                        foreach (System.Data.DataRow dr in datatable.Rows)
                        {
                            table.status.Add(new DBData.RowStatus()
                            {
                                code = "",
                                id = dr.IsNull("id") ? -1 : Convert.ToInt64(dr["id"]),
                                version = dr.IsNull("version") ? -1 : Convert.ToInt32(dr["version"])
                            });
                        }
                        table.data = "";
                        table.status.RemoveAll(e => e.id == -1 || e.version == -1);
                    }
                    return await Sync_MasterStatus(masterData);
                }
                else
                {
                    if (item != null)
                    {
                        string json = await result.Content.ReadAsStringAsync();
                        item.ShowError(json);
                    }
                }
            }
            return true;
        }

        public async Task<Services.SyncManager.DBDataAvailableSummary> CreateSyncPoint(SyncController.SyncRequest creteria)
        {

            creteria.privatekey = LicenseManager.License.PrivateKey;

            string URI = SyncManager.Instance.Configuration.Server + "/SyncController";
            using (var client = new HttpClient()
            {
                Timeout = new TimeSpan(0, 10, 0)
            })
            {
                var serializedProduct = JsonConvert.SerializeObject(creteria);
                var content = new StringContent(serializedProduct, System.Text.Encoding.UTF8, "application/json");
                var result = Task.Run(() => client.PostAsync(URI + "/CreateSyncPoint", content)).Result;
                if (result.IsSuccessStatusCode)
                {
                    string json = await result.Content.ReadAsStringAsync();
                    var masterData = JsonConvert.DeserializeObject<Services.SyncManager.DBDataAvailableSummary>(json);
                    return masterData;
                }
            }
            return new DBDataAvailableSummary();
        }

        public async Task<string> ClearOfflineData()
        {
            var creteria = new Controllers.SyncController.CreateRequest();
            creteria.clientid = SyncManager.Instance.Configuration.Client;
            creteria.location = SyncManager.Instance.Configuration.Location;
            creteria.privatekey = LicenseManager.License.PrivateKey;
            creteria.fromdate = SyncManager.Instance.Configuration.fromdate;

            if (string.IsNullOrEmpty(SyncManager.Instance.Configuration.Client))
            {
                return "Client info not available in web.config";
            }

            if (string.IsNullOrEmpty(SyncManager.Instance.Configuration.Location))
            {
                return "Location info not available in web.config";
            }

            if (string.IsNullOrEmpty(SyncManager.Instance.Configuration.Server))
            {
                return "Server info not available in web.config";
            }

            string URI = SyncManager.Instance.Configuration.Server + "/SyncController";
            using (var client = new HttpClient()
            {
                Timeout = new TimeSpan(0, 10, 0)
            })
            {
                var serializedProduct = JsonConvert.SerializeObject(creteria);
                var content = new StringContent(serializedProduct, System.Text.Encoding.UTF8, "application/json");


                var result = Task.Run(() => client.PostAsync(URI + "/ClearOfflineClient", content)).Result;
                if (result.IsSuccessStatusCode)
                {
                    string json = await result.Content.ReadAsStringAsync();
                    var masterData = JsonConvert.DeserializeObject<string>(json);
                    return masterData;
                }
            }
            return "Offline successfully enabled.";
        }

        public async Task<bool> Sync_Transactions()
        {
            return await Sync_Transactions(null);
        }
        public async Task<bool> Sync_Transactions(SyncController.CreateRequest item)
        {
            var creteria = new Controllers.SyncController.SyncRequest();
            creteria.clientid = SyncManager.Instance.Configuration.Client;
            creteria.location = SyncManager.Instance.Configuration.Location;
            creteria.privatekey = LicenseManager.License.PrivateKey;
            if (string.IsNullOrEmpty(SyncManager.Instance.Configuration.Server))
                SyncManager.Instance.Configuration.Server = @"http://localhost/InfyPOS";
            string URI = SyncManager.Instance.Configuration.Server + "/SyncController";
            using (var client = new HttpClient()
            {
                Timeout = new TimeSpan(0, 10, 0)
            })
            {
                var serializedProduct = JsonConvert.SerializeObject(creteria);
                var content = new StringContent(serializedProduct, System.Text.Encoding.UTF8, "application/json");


                var result = Task.Run(() => client.PostAsync(URI + "/DownloadTransactions", content)).Result;
                if (result.IsSuccessStatusCode)
                {


                    var json = result.Content.ReadAsStreamAsync().GetAwaiter().GetResult();
                    List<Services.SyncManager.DBData> masterData;
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.DateFormatHandling = Newtonsoft.Json.DateFormatHandling.IsoDateFormat;
                    serializer.Converters.Add(new Newtonsoft.Json.Converters.JavaScriptDateTimeConverter()
                    {
                    });

                    serializer.NullValueHandling = NullValueHandling.Ignore;
                    TextReader textReader = new StreamReader(json);
                    using (JsonTextReader reader = new JsonTextReader(textReader))
                    {
                        masterData = serializer.Deserialize<List<Services.SyncManager.DBData>>(reader);
                        // {"ExpiryDate":new Date(1230375600000),"Price":0}
                    }


                    foreach (var table in masterData)
                    {
                        var datatable = table.GetTable();
                        try
                        {
                            Sync_Masters(datatable, table.tablename);
                        }
                        catch (Exception exp)
                        {
                            WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);
                        }
                        table.status = new List<DBData.RowStatus>();
                        foreach (System.Data.DataRow dr in datatable.Rows)
                        {
                            table.status.Add(new DBData.RowStatus()
                            {
                                code = dr.IsNull("barcode") ? "" : dr["barcode"].ToString(),
                                id = dr.IsNull("id") ? -1 : Convert.ToInt64(dr["id"]),
                                version = dr.IsNull("version") ? -1 : Convert.ToInt32(dr["version"])
                            });
                        }
                        table.data = "";
                        table.datas = null;
                        table.status.RemoveAll(e => e.id == -1 || e.version == -1);
                    }
                    return await Sync_TransactionStatus(masterData);
                }
                else
                {
                    if (item != null)
                    {
                        string json = await result.Content.ReadAsStringAsync();
                        item.ShowError(json);
                    }
                }
            }
            return true;
        }

        public async Task<Services.SyncManager.DBDataAvailableSummary> Sync_OfflineStatus(long organizationid,bool all)
        {
            var creteria = new Controllers.SyncController.SyncStatus();
            creteria.clientid = SyncManager.Instance.Configuration.Client;
            creteria.location = SyncManager.Instance.Configuration.Location;
            creteria.privatekey = LicenseManager.License.PrivateKey;
            creteria.all = all;

            string URI = SyncManager.Instance.Configuration.Server + "/SyncController";
            using (var client = new HttpClient()
            {
                Timeout = new TimeSpan(0, 10, 0)
            })
            {
                var serializedProduct = JsonConvert.SerializeObject(creteria);
                var content = new StringContent(serializedProduct, System.Text.Encoding.UTF8, "application/json");
                var result = Task.Run(() => client.PostAsync(URI + "/DownloadServerStatus", content)).Result;
                if (result.IsSuccessStatusCode)
                {
                    string json = await result.Content.ReadAsStringAsync();
                    var masterData = JsonConvert.DeserializeObject<Services.SyncManager.DBDataAvailableSummary>(json);

                    Sync_Server sync_Server = new Sync_Server(organizationid, SyncManager.Instance.Configuration.Client,
                        SyncManager.Instance.Configuration.Location, SyncManager.Instance.Configuration.PrivateKey);
                    creteria.all = false;
                    var billstatus = sync_Server.DownloadDataStatus(organizationid,creteria, true);
                    masterData.offlinebills = billstatus.onlinebills;
                    masterData.offlinetransactions = billstatus.offlinetransactions;
                    return masterData;
                }
            }
            return new DBDataAvailableSummary();
        }

        private async Task<bool> Sync_TransactionStatus(List<DBData> datas)
        {
            var creteria = new Controllers.SyncController.SyncStatus();
            creteria.clientid = SyncManager.Instance.Configuration.Client;
            creteria.location = SyncManager.Instance.Configuration.Location;
            creteria.privatekey = LicenseManager.License.PrivateKey;
            creteria.data = datas;

            foreach (var item in datas)
            {
                item.datas = null;
            }
            if (string.IsNullOrEmpty(SyncManager.Instance.Configuration.Server))
                SyncManager.Instance.Configuration.Server = @"http://localhost/InfyPOS";

            string URI = SyncManager.Instance.Configuration.Server + "/SyncController";
            using (var client = new HttpClient()
            {
                Timeout = new TimeSpan(0, 10, 0)
            })
            {



                foreach (var t in datas)
                {
                    creteria.data = new List<DBData>();
                    creteria.data.Add(t);
                    var serializedProduct = JsonConvert.SerializeObject(creteria);
                    var content = new StringContent(serializedProduct, System.Text.Encoding.UTF8, "application/json");
                    var result = Task.Run(() => client.PostAsync(URI + "/ChangeTransactionStatus", content)).Result;
                    if (result.IsSuccessStatusCode)
                    {
                        string json = await result.Content.ReadAsStringAsync();
                        var masterData = JsonConvert.DeserializeObject<bool>(json);
                    }
                }
            }
            return true;
        }
        private async Task<bool> Sync_MasterStatus(List<DBData> datas)
        {
            var creteria = new Controllers.SyncController.SyncStatus();
            creteria.clientid = SyncManager.Instance.Configuration.Client;
            creteria.location = SyncManager.Instance.Configuration.Location;
            creteria.privatekey = LicenseManager.License.PrivateKey;
            creteria.data = datas;
            datas.ForEach(e => e.datas = null);
            if (string.IsNullOrEmpty(SyncManager.Instance.Configuration.Server))
                SyncManager.Instance.Configuration.Server = @"http://localhost/InfyPOS";

            string URI = SyncManager.Instance.Configuration.Server + "/SyncController";
            using (var client = new HttpClient()
            {
                Timeout = new TimeSpan(0, 10, 0)
            })
            {

                foreach (var t in datas)
                {
                    if (t.status == null || t.status.Count == 0) continue;
                    creteria.data = new List<DBData>();
                    creteria.data.Add(t);
                    var serializedProduct = JsonConvert.SerializeObject(creteria);
                    var content = new StringContent(serializedProduct, System.Text.Encoding.UTF8, "application/json");
                    var result = Task.Run(() => client.PostAsync(URI + "/ChangeMasterStatus", content)).Result;
                    if (result.IsSuccessStatusCode)
                    {
                        string json = await result.Content.ReadAsStringAsync();
                        var masterData = JsonConvert.DeserializeObject<bool>(json);
                    }
                }
            }
            return true;
        }

        internal byte[] UploadData(byte[] source)
        {
            System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();
            try
            {
                List<InfyPOS.Services.SyncManager.DBData> dBRows = null;
                using (var output = new MemoryStream())
                {
                    using (var dstream = new System.IO.Compression.DeflateStream(output, System.IO.Compression.CompressionMode.Decompress))
                    {
                        dstream.Write(source, 0, source.Length);
                    }
                    string json = System.Text.ASCIIEncoding.ASCII.GetString(output.ToArray());
                    dBRows = Newtonsoft.Json.JsonConvert.DeserializeObject<List<InfyPOS.Services.SyncManager.DBData>>(json);
                }
                if (dBRows != null)
                {
                    stringBuilder.AppendLine("Available Tables :" + dBRows.Count);
                    foreach (var row in dBRows)
                    {
                        var table = row.GetTable();
                        stringBuilder.AppendFormat("{0} - {1} - Started - ", row.tablename, table.Rows.Count);
                        try
                        {
                            Sync_Masters(row.GetTable(), row.tablename);
                            stringBuilder.AppendLine("Completed");
                        }
                        catch (Exception exp)
                        {
                            stringBuilder.AppendLine("Exception - " + exp.ToString());
                        }
                    }
                }
                else
                {
                    stringBuilder.Append("No Tables available");
                }
            }
            catch (Exception exp)
            {
                stringBuilder.Append(exp.ToString());
            }
            return System.Text.ASCIIEncoding.ASCII.GetBytes(stringBuilder.ToString());
        }

        public bool ResetSequence(string tablename)
        {
            string sequencename = "";
            string query = @"
with sequencelist 
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
select substring(sequence ,8) as sequencename,substring(""table"" ,8) tablename from sequencelist where substring(""table"" ,8)=@tablename";
            var param = new SortedList<string, object>();
            param.Add("tablename", tablename);
            using (var reader = ExecuteReader(query, param))
            {
                if (reader.Read())
                    sequencename = reader.GetString(0);
            }

            long lastidentity = 0;
            if (!string.IsNullOrEmpty(sequencename))
            {
                query = "ALTER SEQUENCE {0} RESTART WITH {1}";
                using (var reader = ExecuteReader("select max(id) from " + tablename, param))
                {
                    if (reader.Read())
                        lastidentity = reader.IsDBNull(0) ? 0 : reader.GetInt64(0);
                }
                if (lastidentity > 0)
                {
                    return ExecuteQuery(string.Format(query, sequencename, lastidentity + 1), param) > 0;
                }
            }
            return false;
        }
        public List<DBData.RowStatus> Sync_Masters(DataTable table, string tablename)
        {
            if (table == null || table.Rows.Count == 0) return new List<DBData.RowStatus>();
            string columnlistquery = "";
            string parameterlistquery = "";
            string updatequery = "";
            List<DBData.RowStatus> returnList = new List<DBData.RowStatus>();
            foreach (DataColumn column in table.Columns)
            {
                if (tablename == "salablegoods" && column.ColumnName.ToLower() == "oqty")
                    continue;

                if (column.ColumnName.ToLower() != "id")
                    updatequery += (updatequery.Length > 0 ? "," : "update public." + tablename + " SET ") + column.ColumnName + "=@" + column.ColumnName;
                columnlistquery += (columnlistquery.Length > 0 ? "," : "") + column.ColumnName;
                parameterlistquery += (parameterlistquery.Length > 0 ? ",@" : "@") + column.ColumnName;
            }
            string insertQuery = string.Format("INSERT INTO public.{0}({1}) VALUES({2}) RETURNING id", tablename, columnlistquery, parameterlistquery);
            updatequery += " where id=@id";
            List<long> ids = new List<long>();
            List<long> duplicateids = new List<long>();
            foreach (DataRow row in table.Rows)
            {
                ids.Add((long)row["id"]);
            }

            SortedDictionary<string, string> columnTypes = new SortedDictionary<string, string>();
            NpgsqlConnection conn = CreateConnection();
            NpgsqlTransaction transaction = null;
            try
            {
                conn.Open();

                string searchQuery = "Select id from " + tablename + " where id in (" + string.Join<long>(",", ids) + ");" +
                                     "Select data_type,column_name from INFORMATION_SCHEMA.COLUMNS where table_name = '" + tablename + "';";
                NpgsqlCommand command = new NpgsqlCommand(searchQuery, conn);

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    ids.Clear();
                    while (reader.Read())
                    {
                        ids.Add(reader.GetInt64(0));
                    }
                    if (reader.NextResult())
                    {
                        while (reader.Read())
                        {
                            columnTypes.Add(reader.GetString(1), reader.GetString(0));
                        }
                    }
                }

                transaction = conn.BeginTransaction();
                foreach (DataRow row in table.Rows)
                {
                    long id = (long)row["id"];
                    if (duplicateids.Contains(id))
                        continue;
                    int version = row.IsNull("version") ? 0 : Convert.ToInt32(row["version"]);
                    returnList.Add(new DBData.RowStatus()
                    {
                        id = id,
                        version = version
                    });
                    command = new NpgsqlCommand(ids.Contains(id) ? updatequery : insertQuery, conn, transaction);
                    foreach (DataColumn column in table.Columns)
                    {
                        NpgsqlTypes.NpgsqlDbType type = NpgsqlTypes.NpgsqlDbType.Unknown;

                        switch (columnTypes[column.ColumnName])
                        {
                            case "tsvector":
                                type = NpgsqlTypes.NpgsqlDbType.TsVector;
                                command.CommandText = command.CommandText.Replace("@" + column.ColumnName, "to_tsvector(@" + column.ColumnName + ")");
                                break;
                            case "jsonb":
                                type = NpgsqlTypes.NpgsqlDbType.Jsonb;
                                break;
                            case "smallint":
                                type = NpgsqlTypes.NpgsqlDbType.Smallint;
                                break;
                            case "timestamp without time zone":
                                type = NpgsqlTypes.NpgsqlDbType.Timestamp;
                                break;
                            case "json":
                                type = NpgsqlTypes.NpgsqlDbType.Json;
                                break;
                            case "boolean":
                                type = NpgsqlTypes.NpgsqlDbType.Boolean;
                                break;
                            case "money":
                                type = NpgsqlTypes.NpgsqlDbType.Numeric;
                                break;
                            case "character varying":
                                type = NpgsqlTypes.NpgsqlDbType.Varchar;
                                break;
                            case "bytea":
                                type = NpgsqlTypes.NpgsqlDbType.Bytea;
                                break;
                            case "integer":
                                type = NpgsqlTypes.NpgsqlDbType.Integer;
                                break;
                            case "bit":
                                type = NpgsqlTypes.NpgsqlDbType.Bit;
                                break;
                            case "bigint":
                                type = NpgsqlTypes.NpgsqlDbType.Bigint;
                                break;
                        }
                        if (type == NpgsqlTypes.NpgsqlDbType.Bytea)
                            command.Parameters.Add(column.ColumnName, type).Value = DBNull.Value;
                        else if (type == NpgsqlTypes.NpgsqlDbType.TsVector)
                            command.Parameters.Add(column.ColumnName, NpgsqlTypes.NpgsqlDbType.Varchar).Value = CreatTSVector(row[column.ColumnName] as DataTable);
                        else
                            command.Parameters.Add(column.ColumnName, type).Value = row[column.ColumnName];
                    }
                    if (!ids.Contains(id))
                    {
                        command.ExecuteNonQuery();
                        duplicateids.Add(id);
                    }
                }
                transaction.Commit();
                ResetSequence(tablename);
                return returnList;
            }
            catch (Exception exp)
            {
                if (transaction != null)
                    transaction.Rollback();
                if (conn != null)
                    conn.Close();
                ResetSequence(tablename);
                return null;
            }
        }

        public string CreatTSVector(DataTable dt)
        {
            if (dt == null)
                return "";
            System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();
            foreach (DataRow dr in dt.Rows)
                stringBuilder.Append((stringBuilder.Length > 0 ? " " : "") + dr[0].ToString());

            return Regex.Replace(stringBuilder.ToString(), @"[^\w\s]", "", RegexOptions.Compiled);

        }
    }

}