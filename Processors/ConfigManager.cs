using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using WebAPI.Data;
namespace InfyPOS.Services
{
    public class ConfigManager : IDisposable
    {
        private long organizationid;
        public ConfigManager(long organizationid)
        {
            this.organizationid = organizationid;
        }
        private static System.Collections.Concurrent.ConcurrentDictionary<long, ConfigManager> instanceList = new System.Collections.Concurrent.ConcurrentDictionary<long, ConfigManager>();
        public static ConfigManager Instance(long organizationid)
        {
            ConfigManager instance = null;
            if (instanceList.TryGetValue(organizationid, out instance))
                return instance;

            instance = new ConfigManager(organizationid);
            instance.Config = new Configuration.ApplicationSettings();
            if (instanceList.TryAdd(organizationid, instance))
            {
                instance.Initialize();
            }
            instanceList.TryGetValue(organizationid, out instance);
            return instance;

        }


        private SortedDictionary<long, Configuration.ApplicationSettings> locationConfig = null;
        public Configuration.ApplicationSettings Config { get; set; }
        public SortedDictionary<long, Configuration.ApplicationSettings> LocationConfig
        {
            get
            {
                if(locationConfig == null)
                {
                    lock (this.Config)
                    {
                        Initialize();
                    }
                }
                return locationConfig;
            }
        }

        System.Threading.ManualResetEvent mre = new System.Threading.ManualResetEvent(true);
        public DateTime lastModified { get; set; }
        public bool Initialized { get; set; }

        Data.ConfigurationDataService dataservice = null;

        public void Initialize()
        {
            try
            {
                dataservice = new Data.ConfigurationDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
                var list = dataservice.Search(new Models.ConfigurationCreteria()
                {
                    key = "APPLICATIONSETTINGS"
                });
                locationConfig = new SortedDictionary<long, Configuration.ApplicationSettings>();
                Config = new Configuration.ApplicationSettings();
                if (list.Count != 0)
                {
                    foreach (var item in list)
                    {
                        item.appsettings.nextdaycount = item.nextdaycount;
                        item.appsettings.currentday = item.currentday;
                        item.appsettings.freezedate = item.freezedate;
                        if (item.locationid > 0)
                        {
                            if (LocationConfig.ContainsKey(item.locationid))
                                LocationConfig.Remove(item.locationid);

                            LocationConfig.Add(item.locationid, item.appsettings);
                        }
                        else
                        {
                            Config = item.appsettings;
                        }
                    }
                }
                Config.instancename = string.IsNullOrEmpty(System.Configuration.ConfigurationManager.AppSettings["INSTANCE"])
                    ? "PRODUCTION" : System.Configuration.ConfigurationManager.AppSettings["INSTANCE"];
                InfyPOS.Processors.CoreDBListener.Instance(organizationid).Initialize();
                InfyPOS.Processors.CoreDBListener.Instance(organizationid).OnConfigChange += Instance_OnConfigChange;
                Initialized = true;
            }
            catch (Exception exp)
            {
                Initialized = false;
                WebAPI.Common.Logger.Instance(organizationid).Log(exp, WebAPI.Common.Logger.LogType.Error, "Configmanager constructor");
            }
        }
        private void Instance_OnConfigChange(Processors.Config config)
        {
            var items = dataservice.Search(new Models.ConfigurationCreteria()
            {
                id = config.id
            });
            mre.Reset();
            System.Threading.Thread.Sleep(500);

            if (items.Count > 0)
            {
                items[0].appsettings.nextdaycount = items[0].nextdaycount;
                items[0].appsettings.currentday = items[0].currentday;
                items[0].appsettings.freezedate = items[0].freezedate;
                if (items[0].locationid > 0)
                {
                    if (LocationConfig.ContainsKey(items[0].locationid))
                        LocationConfig.Remove(items[0].locationid);
                    LocationConfig.Add(items[0].locationid, items[0].appsettings);
                }
                else
                {
                    Config = items[0].appsettings;
                }
            }
            else
            {
                Config = items[0].appsettings;
            }
            mre.Set();
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            instanceList = null;
            if (!disposedValue)
            {
                //if (disposing)
                //{
                //    if (this.notificationConnection != null && this.notificationConnection.State != System.Data.ConnectionState.Closed)
                //    {
                //        this.notificationConnection.Notification -= this.PromoChanged;
                //        using (var command = new Npgsql.NpgsqlCommand("unlisten incentivechange", this.notificationConnection))
                //        {
                //            command.ExecuteNonQuery();
                //        }
                //        this.notificationConnection.Close();
                //    }
                //}
                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~Manager() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}