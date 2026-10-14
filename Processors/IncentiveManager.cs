using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using WebAPI.Data;
namespace InfyPOS.Services
{
    public class IncentiveManager :  IDisposable 
    {
        private static System.Collections.Concurrent.ConcurrentDictionary<long,IncentiveManager> instance;
        public static IncentiveManager Instance(long organizationid)
        {
            if(instance.ContainsKey(organizationid))
                return instance[instance];

            IncentiveManager incentiveManager = new IncentiveManager(organizationid);
            instance.TryAdd(organizationid, incentiveManager);
            return incentiveManager;
        }


        long organizationid = 0;
        public IncentiveManager(long organizationid)
        {
            this.organizationid = organizationid;
            dataservice = new Data.IncentiveconfigDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            productdataservice = new Data.ProductsDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
        }

        static IncentiveManager()
        {
            instance = new System.Collections.Concurrent.ConcurrentDictionary<long, IncentiveManager>();
        }

        public System.Collections.Concurrent.ConcurrentDictionary<long, long> ProductSectionMap = new System.Collections.Concurrent.ConcurrentDictionary<long, long>();
            
        public List<Incentiveconfig> Schems { get; set; }
        
        System.Threading.ManualResetEvent mre = new System.Threading.ManualResetEvent(true);
        public DateTime lastModified { get; set; }
        public bool Initialized { get; set; }

        Data.IncentiveconfigDataService dataservice = null;
        Data.ProductsDataService productdataservice = null;
        public void LoadSchemes()
        {
            if (mre.WaitOne(100))
            {
                mre.Reset();
                if (!Initialized)
                {
                    var list = dataservice.Search(new Models.IncentiveconfigCreteria()
                    {
                        onlycachable = true
                    });
                    Schems = new List<Incentiveconfig>();
                    Schems.AddRange(list);

                    var productList = productdataservice.Search(new ProductsCreteria()
                    {
                    });

                    foreach (var item in productList)
                        ProductSectionMap.TryAdd(item.id, item.salescounterid);


                    InfyPOS.Processors.CoreDBListener.Instance.Initialize();
                    InfyPOS.Processors.CoreDBListener.Instance.OnIncentiveChange += Instance_OnPromotionChange;
                    InfyPOS.Processors.CoreDBListener.Instance.OnProductChange += Instance_OnProductChange;
                    //string connectionstring = System.Configuration.ConfigurationManager.ConnectionStrings["InfyPOS"].ConnectionString + ";SyncNotification=true";
                    //this.notificationConnection = new Npgsql.NpgsqlConnection(connectionstring);
                    //this.notificationConnection.Open();
                    //if (this.notificationConnection.State == System.Data.ConnectionState.Open)
                    //{
                    //    using (var command = new Npgsql.NpgsqlCommand("listen incentivechange", this.notificationConnection))
                    //    {
                    //        command.ExecuteNonQuery();
                    //    }
                    //    this.notificationConnection.Notification += PromoChanged;
                    //}
                    Initialized = true;
                }
                mre.Set();
            }
        }

        private void Instance_OnProductChange(Processors.Config config)
        {
            var items = productdataservice.Search(new Models.ProductsCreteria()
            {
                id = config.id
            });
            mre.Reset();
            System.Threading.Thread.Sleep(500);
            long newid;
            ProductSectionMap.TryRemove(config.id, out newid);
            if(items.Count>0)
                ProductSectionMap.TryAdd(items[0].id, items[0].salescounterid);
            mre.Set();

        }

        private void Instance_OnPromotionChange(Processors.Config config)
        {
            var items = dataservice.Search(new Models.IncentiveconfigCreteria()
            {
                id = config.id
            });
            mre.Reset();
            System.Threading.Thread.Sleep(500);
            Schems.RemoveAll(entity => entity.id == items[0].id);
            if (items[0].enabled && items[0].isactive)
            {
                Schems.Add(items[0]);
            }
            mre.Set();
        }

        public class IncentiveConfig
        {
            public long salablegoodsid { get; set; }
            public long productid { get; set; }
            public long sectionid { get; set; }
        }
        public Models.Bill Process(Models.Bill bill)
        {
            if (!Initialized)
            {
                LoadSchemes();
            }

            var tList = bill.Billitems.ToList();
            string query = @"select incentive,salablegoodsid from incentiveconfig
                            where salablegoodsid in (" + string.Join<long>(",", bill.Billitems.ConvertAll<long>(e => e.salablegoodsid).Distinct()) + ")";

            using (var reader = Common.BaseData.Instance.ExecuteReader(query, null))
            {
                while(reader.Read())
                {
                    var incentive = reader.GetDecimal(0);
                    var salablegoodsid = reader.GetInt64(1);
                    tList.Find(e => e.salablegoodsid == salablegoodsid).incentive = incentive;
                    tList.RemoveAll(e => e.salablegoodsid == salablegoodsid);
                }
            }

            query = @"select id,invoiceitemid,productid from stock
                            where id in (" + string.Join<long>(",", bill.Billitems.ConvertAll<long>(e => e.stockid).Distinct()) + ")";
            using (var reader = Common.BaseData.Instance.ExecuteReader(query, null))
            {
                while (reader.Read())
                {
                    var id = reader.GetInt64(0);
                    var invoiceitemid = reader.GetInt64(1);
                    var productid = reader.GetInt64(2);
                    long sectionid = 0;
                    if (Schems.Exists(e => e.productid == id))
                    {
                        tList.FindAll(e => e.stockid == id).ForEach(e=>e.incentive = Schems.Find(ex => ex.productid == id).incentive);
                        tList.RemoveAll(e => e.stockid == id);
                    }
                    else if(ProductSectionMap.ContainsKey(productid))
                    {
                        if(ProductSectionMap.TryGetValue(productid,out sectionid))
                        {
                            if (Schems.Exists(e => e.sectionid == sectionid))
                            {
                                tList.FindAll(e => e.stockid == id).ForEach(e => e.incentive = Schems.Find(ex => ex.sectionid == sectionid).incentive);
                                tList.RemoveAll(e => e.stockid == id);
                            }
                        }
                    }
                }
            }
            return bill;
        }

        

        //private void PromoChanged(object sender, Npgsql.NpgsqlNotificationEventArgs e)
        //{
        //    string changedscheme = e.AdditionalInformation;
        //    long id = 0;
        //    if (long.TryParse(changedscheme, out id))
        //    {
        //        var items = dataservice.Search(new Models.IncentiveconfigCreteria()
        //        {
        //            id = id
        //        });
        //        mre.Reset();
        //        System.Threading.Thread.Sleep(500);
        //        Schems.RemoveAll(entity => entity.id == items[0].id);
        //        if (items[0].enabled && items[0].isactive)
        //        {
        //            Schems.Add(items[0]);
        //        }
        //        mre.Set();
        //    }
        //}
        

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
                Schems = null;
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