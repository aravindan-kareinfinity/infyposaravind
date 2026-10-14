using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Instrumentation;
using System.Runtime.CompilerServices;
using System.Web;

namespace InfyPOS.Processors
{
    public class Config
    {
        public long organizationid { get; set; }
        public string type { get; set; }
        public string subtype { get; set; }
        public string value { get; set; }
        public DateTime actionon { get; set; }
        public long id { get; set; }
        public override string ToString()
        {
            return Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(this)));
        }

        public static Config Create(string value)
        {
            if (string.IsNullOrEmpty(value) || value.ToLower() == "null" || System.Text.ASCIIEncoding.ASCII.GetString(Convert.FromBase64String(value)) == "null")
                return new Config();

            return Newtonsoft.Json.JsonConvert.DeserializeObject<Config>(System.Text.ASCIIEncoding.ASCII.GetString(Convert.FromBase64String(value)));
        }
    }
    public class CoreDBListener
    {
        private long organizationid;
        public CoreDBListener(long organizationid)
        {
            this.organizationid = organizationid;
        }
        private static System.Collections.Concurrent.ConcurrentDictionary<long, CoreDBListener> instanceList = new System.Collections.Concurrent.ConcurrentDictionary<long, CoreDBListener>();
        public static CoreDBListener Instance(long organizationid)
        {
            CoreDBListener instance = null;
            if (instanceList.TryGetValue(organizationid, out instance))
                return instance;

            instance = new CoreDBListener(organizationid);
            if (instanceList.TryAdd(organizationid, instance))
            {
                instance.Initialize();
            }
            instanceList.TryGetValue(organizationid, out instance);
            return instance;

        }

        System.Threading.ManualResetEvent mre = new System.Threading.ManualResetEvent(true);
        public bool Initialized { get; set; }
        Npgsql.NpgsqlConnection notificationConnection;

        public static void SendNotification(long organizationid, InfyPOS.Processors.Config config, Npgsql.NpgsqlConnection conn)
        {
            config.organizationid = organizationid;
            var command = new Npgsql.NpgsqlCommand("NOTIFY dbchange, '" + config.ToString() + "';", conn);
            command.ExecuteNonQuery();
            Instance(organizationid).ProcessConfig(config.ToString());
        }

        public bool Initialize()
        {
            if (mre.WaitOne(100))
            {
                try
                {
                    mre.Reset();
                    if (!Initialized)
                    {

                        string connectionstring = Services.OrganizationManager.Instance.Get(organizationid).connectionstring;// + ";SyncNotification=true";
                        this.notificationConnection = new Npgsql.NpgsqlConnection(connectionstring);
                        this.notificationConnection.Open();
                        if (this.notificationConnection.State == System.Data.ConnectionState.Open)
                        {
                            using (var command = new Npgsql.NpgsqlCommand("listen dbchange", this.notificationConnection))
                            {
                                command.ExecuteNonQuery();
                            }
                            this.notificationConnection.Notification += PromoChanged;
                        }
                        Initialized = true;
                    }
                    mre.Set();
                }
                catch (Exception exp)
                {
                    WebAPI.Common.Logger.Instance(organizationid).Log(exp, WebAPI.Common.Logger.LogType.Error, "CoreDBListener Constructor");
                }

            }
            return Initialized;
        }

        public delegate void ConfigChange(Config config);
        public event ConfigChange OnPromotionChange;
        public event ConfigChange OnIncentiveChange;
        public event ConfigChange OnOrganizationChange;
        public event ConfigChange OnProductChange;
        public event ConfigChange OnConfigChange;
        public event ConfigChange OnCustomFieldsChange;
        public event ConfigChange OnCacheChange;
        private void PromoChanged(object sender, Npgsql.NpgsqlNotificationEventArgs e)
        {
            string changedscheme = e.AdditionalInformation;
            ProcessConfig(changedscheme);
        }

        public void ProcessConfig(string changedscheme)
        {
            var config = Config.Create(changedscheme);
            if (config.type == "PROMOTION")
            {
                if (OnPromotionChange != null)
                    OnPromotionChange(config);
                else
                {
                    InfyPOS.Services.PromotionManager.Instance(config.organizationid).LoadSchemes();
                    if (OnPromotionChange != null)
                    {
                        OnPromotionChange(config);
                    }
                }
            }
            else if (config.type == "INCENTIVE")
            {
                if (OnIncentiveChange != null)
                    OnIncentiveChange(config);
            }
            else if (config.type == "ORGANIZATION")
            {
                if (OnOrganizationChange != null)
                    OnOrganizationChange(config);
                else
                {
                    InfyPOS.Services.OrganizationManager.Instance.Initialize();
                    if (OnOrganizationChange != null)
                    {
                        OnOrganizationChange(config);
                    }
                }
            }
            else if (config.type == "CUSTOMFIELDS" || (config.subtype != null && config.subtype.EndsWith("-CUSTOMFIELDS")))
            {
                    
                if (OnCustomFieldsChange != null)
                {
                    OnCustomFieldsChange(config);
                }
                else
                {
                    InfyPOS.Services.CustomFieldsManager.Instance(organizationid).Initialize();
                    if (OnCustomFieldsChange != null)
                    {
                        OnCustomFieldsChange(config);
                    }
                }
            }
            else if (config.type == "PRODUCT")
            {
                if (OnProductChange != null)
                    OnProductChange(config);

                BarcodeManager.Instance(organizationid).ClearCache();

            }
            else if (config.type == "CONFIG")
            {
                if (OnConfigChange != null)
                {
                    OnConfigChange(config);
                }
                else
                {
                    InfyPOS.Services.ConfigManager.Instance(organizationid).Initialize();
                    if (OnConfigChange != null)
                    {
                        OnConfigChange(config);
                    }
                }
            }
            else if (config.type == "CACHE")
            {
                if (OnCacheChange != null)
                    OnCacheChange(config);
                else
                {
                    InfyPOS.Services.CacheManager.Instance(organizationid).Initialize();
                    if (OnCacheChange != null)
                    {
                        OnCacheChange(config);
                    }
                }

            }
        }

        private bool disposedValue = false; // To detect redundant calls
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (this.notificationConnection != null && this.notificationConnection.State != System.Data.ConnectionState.Closed)
                    {
                        this.notificationConnection.Notification -= this.PromoChanged;
                        using (var command = new Npgsql.NpgsqlCommand("unlisten dbchange", this.notificationConnection))
                        {
                            command.ExecuteNonQuery();
                        }
                        this.notificationConnection.Close();
                    }
                }
                disposedValue = true;
            }
        }

    }
}