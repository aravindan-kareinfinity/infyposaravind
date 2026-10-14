using System;
using System.Threading.Tasks;
using Microsoft.Owin;
using Owin;
using System.Web.Http;
using Microsoft.Owin.Security.OAuth;
using System.ComponentModel.DataAnnotations;
using WebAPI.Providers;
using System.Web.Http.Controllers;
using Microsoft.Owin.FileSystems;
using System.Collections.Generic;
using System.Threading;
using System.Web.Http.ExceptionHandling;
using InfyPOS.Common;
using System.Net;

[assembly: OwinStartup(typeof(WebAPI.Startup))]

namespace WebAPI
{
    public class Startup
    {
        public static HttpConfiguration HttpConfiguration { get; private set; }
        public static OAuthBearerAuthenticationOptions OAuthBearerOptions { get; private set; }
        
        public static OwinOfflineEventsMiddleWare owinEventsMiddleWare { get; set; }
        public void Configuration(IAppBuilder app)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            HttpConfiguration = new HttpConfiguration();
            //HttpConfiguration.Services.Replace(typeof(IExceptionHandler), new CustomExceptionHandler());
            ConfigureOAuth(app);

            var tokenCorsPolicy = new System.Web.Cors.CorsPolicy
            {
                AllowAnyOrigin = true,
                AllowAnyHeader = true,
                AllowAnyMethod = true
            };

            var corsOptions = new Microsoft.Owin.Cors.CorsOptions
            {
                PolicyProvider = new Microsoft.Owin.Cors.CorsPolicyProvider
                {
                    PolicyResolver = request => Task.FromResult(request.Path.ToString().StartsWith("/token") ? tokenCorsPolicy : tokenCorsPolicy)
                }
            };

            //app.Use<GlobalExceptionMiddleware>();
            app.UseCors(corsOptions);

            WebApiConfig.Register(HttpConfiguration);

            try
            {
                if (!string.IsNullOrEmpty(System.Configuration.ConfigurationManager.AppSettings["Offline"])
                    && System.Configuration.ConfigurationManager.AppSettings["Offline"].ToLower() == "true")
                {
                    if (string.IsNullOrEmpty(System.Configuration.ConfigurationManager.AppSettings["OfflineAutoSync"])
                        ||
                        System.Configuration.ConfigurationManager.AppSettings["OfflineAutoSync"].ToLower() != "false")
                    {
                        app.Use<OwinOfflineEventsMiddleWare>();
                        WebAPI.Common.Logger.Instance(0).Log("Offline Scheduler Enabled", WebAPI.Common.Logger.LogType.Error, System.Configuration.ConfigurationManager.AppSettings["Offline"]);
                    }
                }

                if (!string.IsNullOrEmpty(System.Configuration.ConfigurationManager.AppSettings["SupportScheduler"])
                  && System.Configuration.ConfigurationManager.AppSettings["SupportScheduler"].ToLower() == "true")
                {
                    app.Use<OwinSchedulerEventsMiddleWare>();
                    WebAPI.Common.Logger.Instance(0).Log("Report Scheduler Enabled", WebAPI.Common.Logger.LogType.Error, System.Configuration.ConfigurationManager.AppSettings["SupportScheduler"]);
                }
                else
                {
                    WebAPI.Common.Logger.Instance(0).Log("Report Scheduler Disabled", WebAPI.Common.Logger.LogType.Error, System.Configuration.ConfigurationManager.AppSettings["SupportScheduler"]);
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(0).Log(exp.Message, WebAPI.Common.Logger.LogType.Error, System.Configuration.ConfigurationManager.AppSettings["SupportScheduler"]);
            }
            app.UseWebApi(HttpConfiguration);
            //if (System.Diagnostics.Debugger.IsAttached)
            //{
            //    var path = System.Web.Hosting.HostingEnvironment.MapPath("~/");
            //    var fileSystem = new VirtualFileSystem(path);
            //    var random = new Random(int.MaxValue);
            //    app.UseStaticFiles(new Microsoft.Owin.StaticFiles.StaticFileOptions()
            //    {
            //        FileSystem = fileSystem,
            //        OnPrepareResponse = ctx =>
            //        {
            //            ctx.OwinContext.Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
            //            ctx.OwinContext.Response.Headers["Pragma"] = "no-cache";
            //            ctx.OwinContext.Response.Headers["Expires"] = "0";
            //        }
            //    });
            //}

        }

        public void ConfigureOAuth(IAppBuilder app)
        {
            //use a cookie to temporarily store information about a user logging in with a third party login provider

            app.UseCors(Microsoft.Owin.Cors.CorsOptions.AllowAll);
            OAuthBearerOptions = new OAuthBearerAuthenticationOptions();

            OAuthAuthorizationServerOptions OAuthServerOptions = new OAuthAuthorizationServerOptions()
            {
                AllowInsecureHttp = true,
                TokenEndpointPath = new PathString("/token"),
                AccessTokenExpireTimeSpan = TimeSpan.FromMinutes(30),
                Provider = new SimpleAuthorizationServerProvider(),
                RefreshTokenProvider = new SimpleRefreshTokenProvider()
            };

            // Token Generation
            app.UseOAuthAuthorizationServer(OAuthServerOptions);
            app.UseOAuthBearerAuthentication(OAuthBearerOptions);

         
        }
    }

    //public class GlobalExceptionMiddleware : OwinMiddleware
    //{
    //    public GlobalExceptionMiddleware(OwinMiddleware next) : base(next)
    //    {

    //    }

    //    public override async Task Invoke(IOwinContext context)
    //    {
    //        try
    //        {
    //            await Next.Invoke(context);
    //        }
    //        catch (Exception ex)
    //        {
    //           try
    //            {

    //            }catch(Exception exp)
    //            {

    //            }
    //        }
    //    }
    //}

    public class CustomExceptionHandler : System.Web.Http.ExceptionHandling.IExceptionHandler
    {
        public Task HandleAsync(ExceptionHandlerContext context, CancellationToken cancellationToken)
        {
            try
            {
                WebAPI.Common.Logger.Instance(0).Log("CustomExceptionHandler",
                    WebAPI.Common.Logger.LogType.Error,
                    context.Exception
                    );
                context.Result = new System.Web.Http.Results.ResponseMessageResult(new System.Net.Http.HttpResponseMessage
                {
                    Content = new System.Net.Http.StringContent("CustomExceptionHandler - An unexpected error occurred - " + context.Exception.Message),
                    StatusCode = System.Net.HttpStatusCode.InternalServerError
                });
            }
            catch (Exception exp)
            {
                context.Result = new System.Web.Http.Results.ResponseMessageResult(new System.Net.Http.HttpResponseMessage
                {
                    Content = new System.Net.Http.StringContent("CustomExceptionHandler - An unexpected error occurred"),
                    StatusCode = System.Net.HttpStatusCode.InternalServerError
                });
            }

            return Task.FromResult(0);
        }
    }

    public class RequireAdminClaimAttribute : System.Web.Http.AuthorizeAttribute
    {
        protected override bool IsAuthorized(HttpActionContext context)
        {
            var principal =
              context.RequestContext.Principal as System.Security.Claims.ClaimsPrincipal;
            return principal.HasClaim(c => c.Type ==
              "http://KareInfinity.com/identity/claims/admin"
              && c.Value == "true");
        }
    }

    public class VirtualFileSystem : Microsoft.Owin.FileSystems.IFileSystem
    {
        Microsoft.Owin.FileSystems.PhysicalFileSystem pfs;
        log4net.ILog FileLog = null;
        public VirtualFileSystem(string path)
        {
            pfs = new Microsoft.Owin.FileSystems.PhysicalFileSystem(path);
            FileLog = log4net.LogManager.GetLogger(typeof(VirtualFileSystem));
        }

        public bool TryGetDirectoryContents(string subpath, out IEnumerable<IFileInfo> contents)
        {
            var result = pfs.TryGetDirectoryContents(subpath, out contents);
            if (result)
            {
                foreach (var fileInfo in contents)
                    FileLog.InfoFormat("TryGetDirectoryContents {0} - {1} - {2}", fileInfo.Name, fileInfo.LastModified, fileInfo.Length);
            }
            return result;
        }

        public bool TryGetFileInfo(string subpath, out IFileInfo fileInfo)
        {
            var result = pfs.TryGetFileInfo(subpath, out fileInfo);
            if (result)
                FileLog.InfoFormat("TryGetFileInfo {0} - {1} - {2}", subpath, fileInfo.LastModified, fileInfo.Length);
            return result;
        }
    }


    public enum ApplicationTypes
    {
        JavaScript = 0,
        NativeConfidential = 1
    };

    public class Client
    {
        [Key]
        public string Id { get; set; }
        [Required]
        public string Secret { get; set; }
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }
        public ApplicationTypes ApplicationType { get; set; }
        public bool Active { get; set; }
        public int RefreshTokenLifeTime { get; set; }
        [MaxLength(100)]
        public string AllowedOrigin { get; set; }
    }

    public class RefreshToken
    {
        [Key]
        public string RefereshTokenHash { get; set; }
        public string RefereshToken { get; set; }
        [Required]
        [MaxLength(50)]
        public string UserName { get; set; }
        [Required]
        [MaxLength(50)]
        public string ClientId { get; set; }
        public DateTime IssuedUtc { get; set; }
        public DateTime ExpiresUtc { get; set; }
        [Required]
        public string ProtectedTicket { get; set; }
        public long EmployeeID { get; set; }
        public long UserID { get; set; }
        public long SessionID { get; set; }
        public bool HasKilled { get; set; }
        public string KilledBy { get; set; }
    }

    public class ExternalLoginViewModel
    {
        public string Name { get; set; }

        public string Url { get; set; }

        public string State { get; set; }
    }

    public class RegisterExternalBindingModel
    {
        [Required]
        public string OrganizationCode { get; set; }
        [Required]
        public string UserName { get; set; }

        [Required]
        public string Provider { get; set; }

        [Required]
        public string ExternalAccessToken { get; set; }

    }

    public class ParsedExternalAccessToken
    {
        public string user_id { get; set; }
        public string app_id { get; set; }
    }
}
