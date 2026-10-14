using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Data;
using InfyPOS.Models;
using WebAPI.Data;
namespace InfyPOS.Services
{

    public class CacheManager : IDisposable
    {
        private long organizationid;
        public CacheManager(long organizationid)
        {
            this.organizationid = organizationid;
        }
        private static System.Collections.Concurrent.ConcurrentDictionary<long, CacheManager> instanceList = new System.Collections.Concurrent.ConcurrentDictionary<long, CacheManager>();

        public static CacheManager Instance(long organizationid)
        {
            CacheManager instance = null;
            if (instanceList.TryGetValue(organizationid, out instance))
                return instance;

            instance = new CacheManager(organizationid);
            if (instanceList.TryAdd(organizationid, instance))
            {
                instance.Initialize();
            }
            instanceList.TryGetValue(organizationid, out instance);
            return instance;
        }

        System.Threading.ManualResetEvent mre = new System.Threading.ManualResetEvent(true);
        public List<string> StaticIPList = new List<string>();
        public List<long> ExcludedRewardProducts { get; set; }
        private List<LocationReferenceItem> locations;
        public List<LocationReferenceItem> Locations
        {
            get
            {
                try
                {
                    if (this.locations == null)
                        UpdateStaticIP();
                    if (this.locations == null)
                        this.locations = new List<LocationReferenceItem>();
                    return this.locations;
                }
                catch (Exception exp)
                {
                    WebAPI.Common.Logger.Instance(organizationid).Log(exp, WebAPI.Common.Logger.LogType.Error, "Cachemanager.locations");
                    this.locations = new List<LocationReferenceItem>();
                    return this.locations;
                }
            }
            set
            {
                this.locations = value;
            }
        }


        List<Storetransaction> outofstockproducts = null;

        public List<long> GetOutOfStockProducts(long locationid, long companyid)
        {
            if (outofstockproducts == null ||
               !outofstockproducts.Exists(x => x.locationid == locationid && x.companyid == companyid))
            {
                StoretransactionDataService storetransactionDataService = new StoretransactionDataService() { Context = new Common.UserClaim() { OrganizationID = this.organizationid } };
                var outofstocks = storetransactionDataService.SearchOutOfStock(new Storetransaction()
                {
                    locationid = locationid,
                    companyid = companyid
                });
                if (outofstockproducts == null)
                    outofstockproducts = new List<Storetransaction>();
                outofstockproducts.Add(outofstocks);
            }
            return outofstockproducts.Find(x => x.locationid == locationid && x.companyid == companyid).OutOfStocks;
        }

        public void SetOutOfStockProducts(Storetransaction storetransaction)
        {
            if (outofstockproducts == null)
                outofstockproducts = new List<Storetransaction>();
            outofstockproducts.RemoveAll(x => x.locationid == storetransaction.locationid && x.companyid == storetransaction.companyid);
            outofstockproducts.Add(storetransaction);
        }

        public List<ProductConfig> productCofigs { get; set; }
        public long SalesLedgerId { get; set; }
        public long CashLedgerId { get; set; }
        public void Initialize()
        {
            try
            {
                Common.BaseData baseData = new Common.BaseData() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
                using (var reader = baseData.ExecuteReader(@"Select id,cast(attributes->>'excludereward' as bool) excludereward,
            sellingmode,pricingmode,barcodesource,salestaxid,producttype from products", new SortedList<string, object>()))
                {
                    productCofigs = new List<ProductConfig>();
                    ExcludedRewardProducts = new List<long>();
                    while (reader.Read())
                    {
                        ProductConfig productConfig = new ProductConfig();
                        productConfig.id = reader.GetInt64(0);
                        productConfig.sellingmode = reader.IsDBNull(2) ? 0 : reader.GetInt32(2);
                        productConfig.pricingmode = reader.IsDBNull(3) ? 0 : reader.GetInt32(3);
                        productConfig.barcodesource = reader.IsDBNull(4) ? 0 : reader.GetInt32(4);
                        productConfig.taxid = reader.IsDBNull(5) ? 0 : reader.GetInt64(5);
                        productConfig.producttype = reader.IsDBNull(6) ? 0 : reader.GetInt64(6);
                        if (!reader.IsDBNull(1) && reader.GetBoolean(1))
                            ExcludedRewardProducts.Add(reader.GetInt64(0));

                        productCofigs.Add(productConfig);
                    }
                }

                UpdateStaticIP();

                SalesLedgerId = -1;
                using (var reader = baseData.ExecuteReader("select id from ledger where typeid=14 and isactive=true order by id limit 1", new SortedList<string, object>()))
                {
                    if (reader.Read())
                    {
                        SalesLedgerId = reader.GetInt64(0);
                    }
                }

                if (SalesLedgerId == -1)
                {
                    try
                    {
                        Data.LedgerDataService ledgerDataService = new Data.LedgerDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } }; ;
                        var ledger = ledgerDataService.Insert(new Ledger()
                        {
                            displayname = "Sales",
                            typeid = 14,
                            isactive = true,
                            version = 1,
                            createdon = DateTime.Now,
                            modifiedon = DateTime.Now,
                            createdby = -1,
                            modifiedby = -1
                        });
                        if (ledger != null)
                            SalesLedgerId = ledger.id;
                    }
                    catch (Exception exp)
                    {

                    }
                }


                CashLedgerId = -1;
                using (var reader = baseData.ExecuteReader("select id from ledger where typeid=6 and isactive=true order by id limit 1", new SortedList<string, object>()))
                {
                    if (reader.Read())
                    {
                        CashLedgerId = reader.GetInt64(0);
                    }
                }

                if (CashLedgerId == -1)
                {
                    try
                    {
                        Data.LedgerDataService ledgerDataService = new Data.LedgerDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } }; ;
                        var ledger = ledgerDataService.Insert(new Ledger()
                        {
                            displayname = "Cash",
                            typeid = 6,
                            isactive = true,
                            version = 1,
                            createdon = DateTime.Now,
                            modifiedon = DateTime.Now,
                            createdby = -1,
                            modifiedby = -1
                        });
                        if (ledger != null)
                            SalesLedgerId = ledger.id;
                    }
                    catch (Exception exp)
                    {

                    }
                }

            }
            catch (Exception exp)
            {
                exp = null;
            }
            InfyPOS.Processors.CoreDBListener.Instance(organizationid).Initialize();
            InfyPOS.Processors.CoreDBListener.Instance(organizationid).OnCacheChange += Instance_OnCacheChange;
        }

        private void UpdateStaticIP()
        {
            var newStaticIPList = new List<string>();
            var locations = new List<LocationReferenceItem>();
            Common.BaseData baseData = new Common.BaseData() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            using (var reader = baseData.ExecuteReader("Select additionalinfo,id,name from referencelist where isactive=true and type ='LOCATION'", new SortedList<string, object>()))
            {
                while (reader.Read())
                {
                    LocationReferenceItem locationReferenceItem = new LocationReferenceItem();
                    locationReferenceItem.id = reader["id"] != DBNull.Value ? Convert.ToInt64(reader["id"]) : 0;
                    locationReferenceItem.name = reader["name"] != DBNull.Value ? Convert.ToString(reader["name"]) : "";
                    if (!reader.IsDBNull(0) && !string.IsNullOrEmpty(reader.GetString(0)))
                    {
                        var additionalinfo = Newtonsoft.Json.JsonConvert.DeserializeObject<Referencelist.Attribute>(reader.GetString(0));
                        if (additionalinfo != null && additionalinfo.staticip != null)
                        {
                            locationReferenceItem.iswarehouse = additionalinfo.iswarehouse;
                            locationReferenceItem.companyid = additionalinfo.parentid;
                            additionalinfo.staticip.RemoveAll(e => e == null || string.IsNullOrEmpty(e.ip));
                            newStaticIPList.AddRange(additionalinfo.staticip.ConvertAll(e => e.ip));
                        }
                    }
                    locations.Add(locationReferenceItem);
                }
            }
            StaticIPList = newStaticIPList;
            this.Locations = locations;
        }
        private void Instance_OnCacheChange(Processors.Config config)
        {
            mre.Reset();
            System.Threading.Thread.Sleep(500);

            if (config.subtype == "PRODUCT")
            {
                if (ExcludedRewardProducts == null) ExcludedRewardProducts = new List<long>();

                if (config.value == "ExcludeReward")
                {
                    if (ExcludedRewardProducts.Contains(config.id))
                        ExcludedRewardProducts.Remove(config.id);
                }
                else if (config.value == "IncludeReward")
                {
                    if (!ExcludedRewardProducts.Contains(config.id))
                        ExcludedRewardProducts.Add(config.id);
                }

                Common.BaseData baseData = new Common.BaseData() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
                using (var reader = baseData.ExecuteReader(@"Select id,cast(attributes->>'excludereward' as bool) excludereward,
                sellingmode,pricingmode,barcodesource,salestaxid,producttype from products where id=" + config.id, new SortedList<string, object>()))
                {
                    while (reader.Read())
                    {
                        ProductConfig productConfig = new ProductConfig();
                        productConfig.id = reader.GetInt64(0);
                        productConfig.sellingmode = reader.IsDBNull(2) ? 0 : reader.GetInt32(2);
                        productConfig.pricingmode = reader.IsDBNull(3) ? 0 : reader.GetInt32(3);
                        productConfig.barcodesource = reader.IsDBNull(4) ? 0 : reader.GetInt32(4);
                        productConfig.taxid = reader.IsDBNull(5) ? 0 : reader.GetInt64(5);
                        productConfig.producttype = reader.IsDBNull(6) ? 0 : reader.GetInt64(6);
                        productCofigs.RemoveAll(e => e.id == config.id);
                        productCofigs.Add(productConfig);
                    }
                }

            }
            else if (config.subtype == "REFERENCELIST-LOCATION")
            {
                UpdateStaticIP();
            }
            else if (config.subtype == "SCHEDULE")
            {
                Processors.Schedule_Actions.Instance.ScheduleChanged(organizationid, config.id);
            }
            else if (config.subtype == "SCHEDULE-ACTION")
            {
                Processors.Schedule_Actions.Instance.ScheduleChanged(organizationid, config.id);
            }
            else if (config.subtype == "SCHEDULE-REPORT")
            {
                Processors.ScheduledReports.Instance(organizationid).ScheduleChanged(config.id);
            }
            else if (config.subtype == "BILL")
            {
                Processors.Schedule_Actions.Instance.BillChanged(config.id);
            }
            mre.Set();
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