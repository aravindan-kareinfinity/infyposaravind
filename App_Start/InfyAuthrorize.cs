using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;

namespace WebAPI
{
    public class InfyAuthorize : System.Web.Http.AuthorizeAttribute
    {
        InfyPOS.Common.PermissionSet permission;
        InfyPOS.Common.Action action;
        InfyPOS.Common.LicenseModes license= InfyPOS.Common.LicenseModes.All;
        bool hasfailedAuthorization;
        public InfyAuthorize(InfyPOS.Common.PermissionSet permission, InfyPOS.Common.Action action)
        {
            this.permission = permission;
            this.action = action;
        }
        public InfyAuthorize(InfyPOS.Common.PermissionSet permission, InfyPOS.Common.Action action, InfyPOS.Common.LicenseModes license)
        {
            this.license = license;
            this.permission = permission;
            this.action = action;
        }

        protected override void HandleUnauthorizedRequest(HttpActionContext actionContext)
        {
            if (hasfailedAuthorization)
            {
                actionContext.Response = new System.Net.Http.HttpResponseMessage(System.Net.HttpStatusCode.Forbidden);
            }
            else
            {
                base.HandleUnauthorizedRequest(actionContext);
            }
        }

        protected override bool IsAuthorized(HttpActionContext actionContext)
        {
            var identity = Thread.CurrentPrincipal.Identity;

            if (identity == null && HttpContext.Current != null)
            {
                identity = HttpContext.Current.User.Identity;
            }

            if (identity == null && !identity.IsAuthenticated)
            {
                return false;
            }

            InfyPOS.Common.UserClaim Context = new InfyPOS.Common.UserClaim(System.Web.HttpContext.Current.Request.GetOwinContext().Authentication);
            if(!Context.HasPermission(permission, action))
            {
                hasfailedAuthorization = true;
                return false;
            }
            if(Context.AllowedTimeFrom >0 && Context.AllowedTimeTo>0)
            {
                if (DateTime.UtcNow.AddMinutes(Context.UTCOffset).TimeOfDay.TotalMilliseconds < Context.AllowedTimeFrom ||
                    DateTime.UtcNow.AddMinutes(Context.UTCOffset).TimeOfDay.TotalMilliseconds > Context.AllowedTimeTo)
                {
                    hasfailedAuthorization = true;
                    return false;
                }
            }
            return true;
        }

    }
}