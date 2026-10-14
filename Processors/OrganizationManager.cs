using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using WebAPI.Data;
namespace InfyPOS.Services
{
    public class OrganizationManager : IDisposable
    {
        static OrganizationManager()
        {
            instance = new OrganizationManager();
            instance.Initialize();
        }
        static OrganizationManager instance = null;
        public static OrganizationManager Instance
        {
            get
            {
                return instance;
            }
        }


        System.Threading.ManualResetEvent mre = new System.Threading.ManualResetEvent(true);
        public DateTime lastModified { get; set; }
        public bool Initialized { get; set; }

        Organisation defaultOrganization;
        Data.OrganisationDataService dataservice = new Data.OrganisationDataService();
        Npgsql.NpgsqlConnection notificationConnection;
        System.Collections.Concurrent.ConcurrentDictionary<long, Organisation> organizations = new System.Collections.Concurrent.ConcurrentDictionary<long, Organisation>();
        public bool Initialize()
        {
            if (mre.WaitOne(100))
            {
                try
                {
                    mre.Reset();
                    if (!Initialized)
                    {
                        //string connectionstring = System.Configuration.ConfigurationManager.ConnectionStrings["InfyPOS"].ConnectionString + ";SyncNotification=true";
                        //this.notificationConnection = new Npgsql.NpgsqlConnection(connectionstring);
                        //this.notificationConnection.Open();
                        //if (this.notificationConnection.State == System.Data.ConnectionState.Open)
                        //{
                        //    using (var command = new Npgsql.NpgsqlCommand("listen dbchange", this.notificationConnection))
                        //    {
                        //        command.ExecuteNonQuery();
                        //    }
                        //    this.notificationConnection.Notification += PromoChanged;
                        //}
                        defaultOrganization = new Organisation()
                        {
                            id = 0,
                            connectionstring = System.Configuration.ConfigurationManager.ConnectionStrings["InfyPOS"].ConnectionString
                        };
                        //var items = dataservice.Search(new OrganisationCreteria());
                        //foreach(var item in items)
                        //{
                        //    organizations.TryAdd(item.id, item);
                        //}
                        Initialized = true;
                    }
                    mre.Set();
                }
                catch (Exception exp)
                {
                    WebAPI.Common.Logger.Instance(0).Log(exp, WebAPI.Common.Logger.LogType.Error, "CoreDBListener Constructor");
                }

            }
            return Initialized;
        }

        private void PromoChanged(object sender, Npgsql.NpgsqlNotificationEventArgs e)
        {
            string changedscheme = e.AdditionalInformation;
            var config = InfyPOS.Processors.Config.Create(changedscheme);
            if (config.type == "ORGANIZATION")
            {
                Instance_OnConfigChange(config);
            }
        }

        public List<Organisation> GetAll()
        {
            return new List<Organisation>(organizations.Values);
        }
        public Organisation Get(string organizationcode)
        {
            return organizations.Values.First(e => e.code == organizationcode);
        }

        public Organisation Get(long organizationid)
        {
            if (organizationid <= 0) return defaultOrganization;
            Organisation org;
            if (organizations.TryGetValue(organizationid, out org))
                return org;
            return defaultOrganization;
        }

        public static void SendNotification(InfyPOS.Processors.Config config, Npgsql.NpgsqlConnection conn)
        {
            var command = new Npgsql.NpgsqlCommand("NOTIFY dbchange, '" + config.ToString() + "';", conn);
            command.ExecuteNonQuery();
        }
        private void Instance_OnConfigChange(Processors.Config config)
        {
            var items = dataservice.Search(new OrganisationCreteria()
            {
                id = config.id
            });
            if (items.Count != 1) return;

            try
            {
                mre.Reset();
                System.Threading.Thread.Sleep(500);
                Organisation org;
                organizations.TryRemove(items[0].id,out org);
                if (items[0].isactive)
                    organizations.TryAdd(items[0].id, items[0]);
            }
            catch (Exception exp)
            {

            }
            finally
            {
                mre.Set();
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
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