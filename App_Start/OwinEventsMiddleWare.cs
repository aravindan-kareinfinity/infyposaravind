
using InfyPOS.Services;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Web;

namespace WebAPI.Providers
{
    public class OwinOfflineEventsMiddleWare : OwinMiddleware
    {
        InfyPOS.Controllers.SyncController.CreateRequest request = null;
        System.Timers.Timer timer = null;
        public bool HasInitialized = false;
        public OwinOfflineEventsMiddleWare(OwinMiddleware next)
            : base(next)
        {
        }

        public async override System.Threading.Tasks.Task Invoke(IOwinContext context)
        {
            if (!HasInitialized)
                await Intialize(context, context.Authentication);
            await Next.Invoke(context);
        }

        private async Task Intialize(IOwinContext context, IAuthenticationManager authentication)
        {
            int interval = 1;
            if (!string.IsNullOrEmpty(System.Configuration.ConfigurationManager.AppSettings["OfflineIntervalMinutes"]))
                int.TryParse(System.Configuration.ConfigurationManager.AppSettings["OfflineIntervalMinutes"], out interval);
            HasInitialized = true;
            timer = new System.Timers.Timer(TimeSpan.FromMinutes(interval).TotalMilliseconds);
            timer.Elapsed += Timer_Elapsed;
            timer.Start();
        }



        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                request = new InfyPOS.Controllers.SyncController.CreateRequest();
                request.clientid = SyncManager.Instance.Configuration.Client;
                request.location = SyncManager.Instance.Configuration.Location;
                request.privatekey = LicenseManager.License.PrivateKey;
                request.fromdate = SyncManager.Instance.Configuration.fromdate;
                request.mode = SyncManager.Instance.Configuration.mode;
                WebAPI.Common.Logger.Instance(0).Log("Offline Sync Started", WebAPI.Common.Logger.LogType.Error, request);
                InfyPOS.Services.SyncManager.Instance.SyncOffline(request);
                WebAPI.Common.Logger.Instance(0).Log("Offline Sync Completed", WebAPI.Common.Logger.LogType.Error, request);
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(0).Log("Offline Sync Error", WebAPI.Common.Logger.LogType.Error, exp);
            }

        }
    }
    public class OwinTimeZoneEventsMiddleWare : OwinMiddleware
    {
        static int counter = 0;
        public OwinTimeZoneEventsMiddleWare(OwinMiddleware next)
            : base(next)
        {

        }

        public async override System.Threading.Tasks.Task Invoke(IOwinContext context)
        {
            var retvalue = System.Threading.Interlocked.Add(ref counter, 1);
            if (retvalue % 3 == 0)
            {
                Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("de-DE"); // de-DE
                Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("de-DE");

            }
            else
            {
                Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-IN"); // de-DE
                Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("en-IN");
            }
            await Next.Invoke(context);
        }
    }
    public class TimeZoneHandler : Common.MessageHandler
    {

        protected override async Task IncommingMessageAsync(string correlationId, string requestInfo, byte[] message)
        {


        }

        protected override async Task OutgoingMessageAsync(string correlationId, string requestInfo, byte[] message)
        {
        }

    }

    public class OwinSchedulerEventsMiddleWare : OwinMiddleware
    {
        // Static = shared across requests
        private static bool _initialized = false;
        private static readonly object _initLock = new object();

        private static System.Timers.Timer _timer;

        public OwinSchedulerEventsMiddleWare(OwinMiddleware next)
            : base(next)
        {
        }

        public override async Task Invoke(IOwinContext context)
        {
            try
            {
                EnsureInitialized();  
                await Next.Invoke(context);
            }
            catch (TaskCanceledException)
            {
                // Do NOT crash AppPool
                return;
            }
            catch (Exception ex)
            {
                WebAPI.Common.Logger.Instance(0)
                    .Log(ex, WebAPI.Common.Logger.LogType.Error, "OwinSchedulerEventsMiddleWare.Invoke");
                throw;
            }
        }

        // -------------------- Initialization --------------------

        private void EnsureInitialized()
        {
            if (_initialized) return;

            lock (_initLock)
            {
                if (_initialized) return;

                int interval = 1;
                if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["SchedulerIntervalMinutes"]))
                {
                    int.TryParse(
                        ConfigurationManager.AppSettings["SchedulerIntervalMinutes"],
                        out interval
                    );
                }

                _timer = new System.Timers.Timer(TimeSpan.FromMinutes(interval).TotalMilliseconds);
                _timer.AutoReset = true;
                _timer.Elapsed += Timer_Elapsed;
                _timer.Start();

                _initialized = true;

                WebAPI.Common.Logger.Instance(0)
                    .PerformanceLog("Scheduler Initialized", WebAPI.Common.Logger.LogType.Info, "", "", 0, 0);
            }
        }

        // -------------------- Scheduler Logic --------------------

        private static void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                if (IsSAAS)
                {
                    foreach (var org in OrganizationManager.Instance.GetAll())
                    {
                        try
                        {
                            InfyPOS.Processors.ScheduledReports
                                .Instance(org.id)
                                .Run();
                        }
                        catch (Exception ex)
                        {
                            WebAPI.Common.Logger.Instance(0)
                                .PerformanceLog(ex.ToString(), WebAPI.Common.Logger.LogType.Error,
                                $"Scheduler Org {org.id}", "", 0, 0);
                        }
                    }
                }
                else
                {
                    InfyPOS.Processors.ScheduledReports
                        .Instance(0)
                        .Run();
                }
            }
            catch (Exception ex)
            {
                WebAPI.Common.Logger.Instance(0)
                    .Log(ex, WebAPI.Common.Logger.LogType.Error, "Scheduler Timer");
            }
        }

        // -------------------- SAAS Mode --------------------

        private static bool? _isSaas;

        private static bool IsSAAS
        {
            get
            {
                if (_isSaas.HasValue)
                    return _isSaas.Value;

                _isSaas =
                    !string.IsNullOrEmpty(ConfigurationManager.AppSettings["SAASMODE"]) &&
                    (
                        ConfigurationManager.AppSettings["SAASMODE"].Equals("true", StringComparison.OrdinalIgnoreCase) ||
                        ConfigurationManager.AppSettings["SAASMODE"].Equals("yes", StringComparison.OrdinalIgnoreCase)
                    );

                return _isSaas.Value;
            }
        }
    }

}