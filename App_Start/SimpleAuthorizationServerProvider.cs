using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OAuth;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace WebAPI.Providers
{
    public class SimpleAuthorizationServerProvider : OAuthAuthorizationServerProvider
    {
        private static bool? multipletimezone;
        public bool HasMultipleTimeZone
        {
            get
            {
                if (multipletimezone.HasValue) return multipletimezone.Value;
                if (string.IsNullOrEmpty(System.Configuration.ConfigurationManager.AppSettings["MultipleTimeZone"]))
                    multipletimezone = false;
                else
                    multipletimezone = System.Configuration.ConfigurationManager.AppSettings["MultipleTimeZone"].ToLower() == "true";

                return multipletimezone.Value;
            }
        }
     
        public DateTime DateTimeNow(int UTCOffset)
        {
                if (!HasMultipleTimeZone)
                    return DateTime.Now;

                var utcnow = DateTime.UtcNow.AddMinutes(UTCOffset);
                DateTime.SpecifyKind(utcnow, DateTimeKind.Unspecified);
                return utcnow;
        }
        public static string GetHash(string input)
        {
            HashAlgorithm hashAlgorithm = new SHA256CryptoServiceProvider();

            byte[] byteValue = System.Text.Encoding.UTF8.GetBytes(input);

            byte[] byteHash = hashAlgorithm.ComputeHash(byteValue);

            return Convert.ToBase64String(byteHash);
        }

        public override Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            try
            {
                string clientId = string.Empty;
                string clientSecret = string.Empty;
                Client client = null;
                string[] organizationCodes;
                string organizationCode = string.Empty;
                string campusid = string.Empty;
                string platform = "";
                string deviceid = "";
                string userid = "";
                string sessionid = "";
                string tz = "";
                if (context.Parameters.Count() == 0)
                    return Task.FromResult<object>(null);
                if (context.Parameters["org"] != null)
                {
                    organizationCodes = context.Parameters["org"].Split(':');
                    organizationCode = organizationCodes[0];
                    campusid = organizationCodes.Length == 2 ? organizationCodes[1] : "0";
                }
                if (context.Parameters["platform"] != null)
                {
                    platform = context.Parameters["platform"];
                }
                if (context.Parameters["tz"] != null)
                {
                    tz = context.Parameters["tz"];
                }
                if (context.Parameters["deviceid"] != null)
                {
                    deviceid = context.Parameters["deviceid"];
                }
                if (context.Parameters["userid"] != null)
                {
                    userid = context.Parameters["userid"];
                }
                if (context.Parameters["sessionid"] != null)
                {
                    sessionid = context.Parameters["sessionid"];
                }
                if (!context.TryGetBasicCredentials(out clientId, out clientSecret))
                {
                    context.TryGetFormCredentials(out clientId, out clientSecret);
                }

                if (context.ClientId == null)
                {
                    //Remove the comments from the below line context.SetError, and invalidate context 
                    //if you want to force sending clientId/secrects once obtain access tokens. 
                    context.Validated();
                    //context.SetError("invalid_clientId", "ClientId should be sent.");
                    return Task.FromResult<object>(null);
                }

                using (AuthRepository _repo = new AuthRepository())
                {
                    client = _repo.FindClient(context.ClientId);
                }

                if (client == null)
                {
                    context.SetError("invalid_clientId", string.Format("Client '{0}' is not registered in the system.", context.ClientId));
                    return Task.FromResult<object>(null);
                }

                if (client.ApplicationType == ApplicationTypes.NativeConfidential)
                {
                    if (string.IsNullOrWhiteSpace(clientSecret))
                    {
                        context.SetError("invalid_clientId", "Client secret should be sent.");
                        return Task.FromResult<object>(null);
                    }
                    else
                    {
                        if (client.Secret != GetHash(clientSecret))
                        {
                            context.SetError("invalid_clientId", "Client secret is invalid.");
                            return Task.FromResult<object>(null);
                        }
                    }
                }

                if (!client.Active)
                {
                    context.SetError("invalid_clientId", "Client is inactive.");
                    return Task.FromResult<object>(null);
                }

                context.OwinContext.Set<string>("as:clientOrganizationCode", organizationCode);
                context.OwinContext.Set<string>("as:clientCampusID", campusid);
                context.OwinContext.Set<string>("as:deviceID", deviceid);
                context.OwinContext.Set<string>("as:platformID", platform);
                context.OwinContext.Set<string>("as:userid", userid);
                context.OwinContext.Set<string>("as:sessionid", sessionid);
                context.OwinContext.Set<string>("as:tz", tz);
                context.OwinContext.Set<string>("as:clientAllowedOrigin", client.AllowedOrigin);
                context.OwinContext.Set<string>("as:clientRefreshTokenLifeTime", client.RefreshTokenLifeTime.ToString());

                context.Validated();
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(0).Log(exp, WebAPI.Common.Logger.LogType.Error, context);
            }
            return Task.FromResult(0);
        }

        public override async Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {
            try
            {
                var allowedOrigin = context.OwinContext.Get<string>("as:clientAllowedOrigin");
                if (allowedOrigin == null) allowedOrigin = "*";
                if (context.OwinContext.Response.Headers.ContainsKey("Access-Control-Allow-Origin"))
                    context.OwinContext.Response.Headers.Remove("Access-Control-Allow-Origin");

                context.OwinContext.Response.Headers.Add("Access-Control-Allow-Origin", new[] { "*", "http://localhost" });
                //context.OwinContext.Response.Headers.Add("Access-Control-Allow-Methods", "GET,POST,PUT,DELETE,OPTIONS".Split(','));

                InfyPOS.Models.Users user = null;
                InfyPOS.Models.Usersession session = null;
                using (AuthRepository _repo = new AuthRepository())
                {
                    try
                    {
                        if (string.IsNullOrEmpty(context.UserName) && string.IsNullOrEmpty(context.Password))
                        {
                            long userid = 0;
                            long sessionid = 0;
                            if(long.TryParse(context.OwinContext.Get<string>("as:userid"), out userid) &&
                            long.TryParse(context.OwinContext.Get<string>("as:sessionid"), out sessionid))
                            {
                                user = await _repo.FindUser(userid, sessionid, context.OwinContext.Get<string>("as:clientOrganizationCode"),
                                        context.OwinContext.Get<string>("as:clientCampusID"));
                            }
                        }
                        else
                        {
                            user = await _repo.FindUser(context.UserName,
                                context.Password,
                                context.OwinContext.Get<string>("as:clientOrganizationCode"),
                                context.OwinContext.Get<string>("as:clientCampusID"));
                        }
                        if (user == null)
                        {
                            context.SetError("invalid_grant", "The user name or password is incorrect.");

                            /*Common.Logger.Instance(Context.OrganizationID).Log(new Exception("The user name or password is incorrect."),
                                Common.Logger.LogType.Error, string.Format("Username :{0} password: {1} orgcode = {2} campusid = {3}",
                                context.UserName, context.Password, context.OwinContext.Get<string>("as:clientOrganizationCode"),
                                context.OwinContext.Get<string>("as:clientCampusID")));*/

                            return;
                        }

                        if (!string.IsNullOrEmpty(user.passcode))
                            user.passcode = WebAPI.Data.Cryptor.DecryptString(user.passcode, WebAPI.Data.Cryptor.DefaultToken);

                        int tz = 0;
                        int.TryParse(context.OwinContext.Get<string>("as:tz"), out tz);
                        if (user.passcodeon.Date != DateTimeNow(tz).Date)
                        {
                            InfyPOS.Data.UsersDataService usersDataService = new InfyPOS.Data.UsersDataService() {
                                Context = new InfyPOS.Common.UserClaim() {
                                    OrganizationID = _repo.organaizationid,
                                    OrganizationCode = context.OwinContext.Get<string>("as:clientOrganizationCode")
                                } };
                            user.passcode = usersDataService.CreatePasscode(user.id, user.passcode);
                        }
                        session = await _repo.CreateSession(_repo.organaizationid,new InfyPOS.Models.Users.LoggedInUser()
                        {
                            user = user,
                            platform = context.OwinContext.Get<string>("as:platformID"),
                            deviceid = context.OwinContext.Get<string>("as:deviceID"),
                            utcoffset = context.OwinContext.Get<string>("as:tz"),
                        });
                    }
                    catch (Exception exp)
                    {
                        context.SetError("invalid_grant", exp.Message);
                        return;
                    }
                }


                var identity = new ClaimsIdentity(context.Options.AuthenticationType);
                identity.AddClaim(new Claim(ClaimTypes.Role, user.roleName.ToString()));
                identity.AddClaim(new Claim(ClaimTypes.Name, context.UserName));
                identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.id.ToString()));
                identity.AddClaim(new Claim(ClaimTypes.Sid, session.id.ToString()));
                identity.AddClaim(new Claim(ClaimTypes.PrimaryGroupSid, user.organizationcode));
                identity.AddClaim(new Claim(ClaimTypes.PrimarySid, user.organizationid.ToString()));
                identity.AddClaim(new Claim("Infy.EmployeeName", user.title));
                identity.AddClaim(new Claim("Infy.Employeeid", user.employeeid.ToString()));
                identity.AddClaim(new Claim("Infy.Companyid", user.companyid.ToString()));
                identity.AddClaim(new Claim("Infy.TR", string.IsNullOrEmpty(user.tr) ?"" :  user.tr));
                identity.AddClaim(new Claim("Infy.AD", user.alloweddiscount.ToString()));
                if(user.permissions != null &&
                    user.permissions.Exists(x=>x.code == "RESTRICTEDCUSTOMER") &&
                    user.permissions.Find(x => x.code == "RESTRICTEDCUSTOMER").view)
                {
                    identity.AddClaim(new Claim("Infy.RC", true.ToString()));
                }
                else
                {
                    identity.AddClaim(new Claim("Infy.RC", false.ToString()));
                }
                identity.AddClaim(new Claim("Infy.TZ", session.utcoffset.ToString()));
                identity.AddClaim(new Claim("Infy.DSG", user.dsgid.ToString()));
                identity.AddClaim(new Claim("Infy.PFM", session.platform));
                identity.AddClaim(new Claim("Infy.DVC", session.deviceid));
                identity.AddClaim(new Claim("Infy.Permission", InfyPOS.Common.PermissionUtil.GetPermissionString(user.organizationid,user.permissions)));
                if (user.accessablesections == null || user.accessablesections.Count == 0)
                {
                    user.accessablesections = new List<InfyPOS.Models.Users.ResourcePermission>();
                    user.accessablesections.Add(new InfyPOS.Models.Users.ResourcePermission()
                    {
                        id = -1
                    });
                }
                identity.AddClaim(new Claim("Infy.Section", Newtonsoft.Json.JsonConvert.SerializeObject(user.accessablesections.ConvertAll(e => e.id))));
                if(user.accessiblelocation == null || user.accessiblelocation.Count == 0)
                {
                    user.accessiblelocation = new List<InfyPOS.Models.Users.ResourcePermission>();
                    user.accessiblelocation.Add(new InfyPOS.Models.Users.ResourcePermission()
                    {
                        id = -1
                    });
                }
                identity.AddClaim(new Claim("Infy.Location", Newtonsoft.Json.JsonConvert.SerializeObject(user.accessiblelocation.ConvertAll(e => e.id))));
                if (user.accessiblecompany == null || user.accessiblecompany.Count == 0)
                {
                    user.accessiblecompany = new List<InfyPOS.Models.Users.ResourcePermission>();
                    user.accessiblecompany.Add(new InfyPOS.Models.Users.ResourcePermission()
                    {
                        id = -1
                    });
                }
                identity.AddClaim(new Claim("Infy.Company", Newtonsoft.Json.JsonConvert.SerializeObject(user.accessiblecompany.ConvertAll(e => e.id))));
                bool isrootuser = false;
                if (!string.IsNullOrEmpty(context.OwinContext.Get<string>("as:clientOrganizationCode")) &&
                    context.OwinContext.Get<string>("as:clientOrganizationCode") == "QUANTO")
                    isrootuser = true;

                var props = new AuthenticationProperties(new Dictionary<string, string>
                {
                    {
                        "as:client_id", (context.ClientId == null) ? string.Empty : context.ClientId
                    },
                    {
                        "userName", context.UserName
                    },
                    {
                        "employeeId" , user.employeeid.ToString()
                    },
                    {
                        "userId" , user.id.ToString()
                    },
                    {
                        "sessionId" , session.id.ToString()
                    },
                    {
                        "Title", user.title
                    },
                    {
                        "Designation",  user.designation
                    },
                    {
                        "OrganizationID", user.organizationid.ToString()
                    },
                    {
                        "OrganizationCode",context.OwinContext.Get<string>("as:clientOrganizationCode")
                    },
                    {
                        "CompanyName", user.companyname
                    },
                    {
                        "LandingScreen", user.landingscreen
                    },
                    {
                        "Passcode", user.passcode
                    }
                    ,
                    {
                        "RootUser", isrootuser.ToString()
                    },
                    {
                        "AD", user.alloweddiscount.ToString()
                    },
                    {
                        "KU",session.removeusercount.ToString()
                    }
                });

                //,
                //        {
                //    "refresh_token" , session.refreshtoken
                //        },
                //        {
                //    "refresh_token_hash" , GetHash(session.refreshtoken)
                //        }

                var ticket = new AuthenticationTicket(identity, props);
                context.Validated(ticket);
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(0).Log(exp, WebAPI.Common.Logger.LogType.Error, context);
            }
        }
        public override Task GrantRefreshToken(OAuthGrantRefreshTokenContext context)
        {
            try
            {
                var originalClient = context.Ticket.Properties.Dictionary["as:client_id"];
                var currentClient = context.ClientId;

                if (originalClient != currentClient)
                {
                    context.SetError("invalid_clientId", "Refresh token is issued to a different clientId.");
                    return Task.FromResult<object>(null);
                }

                // Change auth ticket for refresh token requests
                var newIdentity = new ClaimsIdentity(context.Ticket.Identity);

                var newClaim = newIdentity.Claims.Where(c => c.Type == "newClaim").FirstOrDefault();
                if (newClaim != null)
                {
                    newIdentity.RemoveClaim(newClaim);
                }
                newIdentity.AddClaim(new Claim("newClaim", Guid.NewGuid().ToString()));

                var newTicket = new AuthenticationTicket(newIdentity, context.Ticket.Properties);
                context.Validated(newTicket);
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(0).Log(exp, WebAPI.Common.Logger.LogType.Error, context);
            }
            return Task.FromResult<object>(null);
        }

        public override Task TokenEndpoint(OAuthTokenEndpointContext context)
        {
            foreach (KeyValuePair<string, string> property in context.Properties.Dictionary)
            {
                context.AdditionalResponseParameters.Add(property.Key, property.Value);
            }

            return Task.FromResult<object>(null);
        }
    }

    public class UserModel
    {
        [Required]
        [Display(Name = "User name")]
        public string UserName { get; set; }
        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        [Display(Name = "Organization Code")]
        public string OrganizationCode { get; set; }
    }


}