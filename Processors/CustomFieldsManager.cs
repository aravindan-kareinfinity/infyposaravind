using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using WebAPI.Data;
namespace InfyPOS.Services
{
    public class CustomFieldsManager : IDisposable
    {
        private long organizationid;
        public CustomFieldsManager(long organizationid)
        {
            this.organizationid = organizationid;
        }
        private static System.Collections.Concurrent.ConcurrentDictionary<long, CustomFieldsManager> instanceList = new System.Collections.Concurrent.ConcurrentDictionary<long, CustomFieldsManager>();
        public static CustomFieldsManager Instance(long organizationid)
        {
            CustomFieldsManager instance = null;
            if (instanceList.TryGetValue(organizationid, out instance))
                return instance;

            instance = new CustomFieldsManager(organizationid);
            if (instanceList.TryAdd(organizationid, instance))
            {
                instance.Initialize();
            }
            instanceList.TryGetValue(organizationid, out instance);
            return instance;

        }

        public class CustomField
        {
            public long id { get; set; }
            public string name { get; set; }
            public string cfref { get; set; }
            public string cfname { get; set; }
            public string cftype { get; set; }
            public long datatype { get; set; }
        }


        public List<CustomField> Config { get; set; }

        System.Threading.ManualResetEvent mre = new System.Threading.ManualResetEvent(true);
        public DateTime lastModified { get; set; }
        public bool Initialized { get; set; }

        Data.ReferencelistDataService dataservice = null;

        public void Initialize()
        {
            try
            {
                dataservice = new Data.ReferencelistDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
                var list = dataservice.Search(new ReferencelistCreteria()
                {
                    type = "CUSTOMFIELDS"
                });
                Config = new List<CustomField>();
                if (list.Count != 0)
                {
                    foreach (var item in list)
                    {
                        if(item.additionalinfo.customfield != null &&
                            item.additionalinfo.customfield.cfbuilt)
                        {
                            Config.Add(new CustomField()
                            {
                                id = item.id,
                                name = item.name,
                                cfname = item.additionalinfo.customfield.cfname,
                                cfref = item.additionalinfo.customfield.cfref,
                                cftype = item.additionalinfo.customfield.cftype,
                                datatype = item.referenceid
                            });
                        }
                    }
                }
                InfyPOS.Processors.CoreDBListener.Instance(organizationid).Initialize();
                InfyPOS.Processors.CoreDBListener.Instance(organizationid).OnCustomFieldsChange += Instance_OnConfigChange;
                Initialized = true;
            }
            catch (Exception exp)
            {
                Initialized = false;
                WebAPI.Common.Logger.Instance(organizationid).Log(exp, WebAPI.Common.Logger.LogType.Error, "CustomFieldsManager constructor");
            }
        }
        private void Instance_OnConfigChange(Processors.Config config)
        {
            var items = dataservice.Search(new ReferencelistCreteria
            {
                id = config.id
            });
            mre.Reset();
            System.Threading.Thread.Sleep(500);

            if (items.Count > 0)
            {
                var item = items.First();
                if (item.additionalinfo.customfield != null &&
                            item.additionalinfo.customfield.cfbuilt)
                {
                    Config.RemoveAll(x => x.id == item.id);
                    Config.Add(new CustomField()
                    {
                        id = item.id,
                        name = item.name,
                        cfname = item.additionalinfo.customfield.cfname,
                        cfref = item.additionalinfo.customfield.cfref,
                        cftype = item.additionalinfo.customfield.cftype,
                        datatype = item.referenceid
                    });
                }
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