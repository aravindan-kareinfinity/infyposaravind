using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Infrastructure;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Web;

namespace WebAPI.Providers
{
    public class SimpleRefreshTokenProvider : IAuthenticationTokenProvider
    {
        public static string GetHash(string input)
        {
            HashAlgorithm hashAlgorithm = new SHA256CryptoServiceProvider();

            byte[] byteValue = System.Text.Encoding.UTF8.GetBytes(input);

            byte[] byteHash = hashAlgorithm.ComputeHash(byteValue);

            return Convert.ToBase64String(byteHash);
        }

        public async Task CreateAsync(AuthenticationTokenCreateContext context)
        {
            try
            {
                var clientid = context.Ticket.Properties.Dictionary.ContainsKey("as:client_id") ?
                            context.Ticket.Properties.Dictionary["as:client_id"] : "InfyPOS";

                if (string.IsNullOrEmpty(clientid))
                {
                    return;
                }

                using (AuthRepository _repo = new AuthRepository())
                {
                    var refreshTokenLifeTime = context.OwinContext.Get<string>("as:clientRefreshTokenLifeTime");
                    var tkn = _repo.GetNextToken();

                    if (context.Ticket.Properties.Dictionary.ContainsKey("refresh_token") &&
                        context.Ticket.Properties.Dictionary.ContainsKey("refresh_token_hash"))
                    {
                        tkn.Key = context.Ticket.Properties.Dictionary["refresh_token"];
                        tkn.Hash = context.Ticket.Properties.Dictionary["refresh_token_hash"];
                    }

                    var token = new RefreshToken()
                    {
                        RefereshToken = tkn.Key,
                        RefereshTokenHash = tkn.Hash,
                        ClientId = clientid,
                        UserName = context.Ticket.Identity.Name,
                        IssuedUtc = DateTime.UtcNow,
                        ExpiresUtc = DisableRefreshToken?DateTime.UtcNow.AddDays(365): DateTime.UtcNow.AddMinutes(Convert.ToDouble(refreshTokenLifeTime)),
                        UserID = Convert.ToInt64(context.Ticket.Properties.Dictionary["userId"]),
                        EmployeeID = Convert.ToInt64(context.Ticket.Properties.Dictionary["employeeId"]),
                        SessionID = Convert.ToInt64(context.Ticket.Properties.Dictionary["sessionId"])
                    };

                    context.Ticket.Properties.IssuedUtc = token.IssuedUtc;
                    context.Ticket.Properties.ExpiresUtc = token.ExpiresUtc;
                    context.Ticket.Properties.AllowRefresh = true;
                    context.Ticket.Properties.IsPersistent = true;

                    if (context.Ticket.Properties.Dictionary.ContainsKey("refresh_token"))
                        context.Ticket.Properties.Dictionary.Remove("refresh_token");

                    if (context.Ticket.Properties.Dictionary.ContainsKey("refresh_token_hash"))
                        context.Ticket.Properties.Dictionary.Remove("refresh_token_hash");

                    if (context.Ticket.Properties.Dictionary.ContainsKey("access_token"))
                        context.Ticket.Properties.Dictionary.Remove("access_token");

                    context.Ticket.Properties.Dictionary.Add("refresh_token", token.RefereshToken);
                    context.Ticket.Properties.Dictionary.Add("refresh_token_hash", token.RefereshTokenHash);

                    context.SetToken(token.RefereshToken);
                    token.ProtectedTicket = context.SerializeTicket();

                    var result = await _repo.AddRefreshToken(Convert.ToInt64(context.Ticket.Properties.Dictionary["OrganizationID"]), token);
                }
            }catch(Exception exp)
            {
                WebAPI.Common.Logger.Instance(0).Log(exp, WebAPI.Common.Logger.LogType.Error, "CreateAsync Simple referesh token");
            }
        }


        bool? disableRefreshToken;
        public bool DisableRefreshToken
        {
            get
            {
                if (disableRefreshToken.HasValue)
                    return disableRefreshToken.Value;

                disableRefreshToken = !string.IsNullOrEmpty(System.Configuration.ConfigurationManager.AppSettings["DisableRefreshToken"]) &&
(System.Configuration.ConfigurationManager.AppSettings["DisableRefreshToken"].ToLower() == "true" ||
System.Configuration.ConfigurationManager.AppSettings["DisableRefreshToken"].ToLower() == "yes");

                return disableRefreshToken.Value;
            }
        }
        public async Task ReceiveAsync(AuthenticationTokenReceiveContext context)
        {
            try
            {
                var allowedOrigin = context.OwinContext.Get<string>("as:clientAllowedOrigin");

                if (context.OwinContext.Response.Headers.ContainsKey("Access-Control-Allow-Origin"))
                    context.OwinContext.Response.Headers.Remove("Access-Control-Allow-Origin");

                context.OwinContext.Response.Headers.Add("Access-Control-Allow-Origin", new[] { "*" });

                string hashedTokenId = GetHash(context.Token);

                InfyPOS.Common.UserClaim Context = new InfyPOS.Common.UserClaim()
                {
                    OrganizationID = 0
                };
                try
                {
                    Context = new InfyPOS.Common.UserClaim(System.Web.HttpContext.Current.Request.GetOwinContext().Authentication);
                }
                catch(Exception exp)
                {

                }
                using (AuthRepository _repo = new AuthRepository())
                {
                    var refreshToken = await _repo.FindRefreshToken(Context.OrganizationID, hashedTokenId);

                    if (refreshToken != null )
                    {
                        if (refreshToken.HasKilled)
                        {
                            WebAPI.Common.Logger.Instance(0).Log(new Exception("Token killed By "+ refreshToken.KilledBy), WebAPI.Common.Logger.LogType.Error, "CreateAsync Simple referesh token");
                        }
                        else
                        {
                            //Get protectedTicket from refreshToken class
                            context.DeserializeTicket(refreshToken.ProtectedTicket);

                            if (context.Ticket != null && context.Ticket.Properties != null &&
                                context.Ticket.Properties.Dictionary != null &&
                                context.Ticket.Properties.Dictionary.ContainsKey("userId") &&
                                context.Ticket.Properties.Dictionary.ContainsKey("employeeId") &&
                                context.Ticket.Properties.Dictionary.ContainsKey("sessionId"))
                            {

                                var refreshTokenLifeTime = context.OwinContext.Get<string>("as:clientRefreshTokenLifeTime");
                                var clientid = context.Ticket.Properties.Dictionary.ContainsKey("as:client_id") ?
                                    context.Ticket.Properties.Dictionary["as:client_id"] : "InfyPOS";
                                var token = new RefreshToken()
                                {
                                    RefereshToken = refreshToken.RefereshToken,
                                    RefereshTokenHash = refreshToken.RefereshTokenHash,
                                    ClientId = clientid,
                                    UserName = context.Ticket.Identity.Name,
                                    IssuedUtc = DateTime.UtcNow,
                                    ExpiresUtc = DisableRefreshToken ? DateTime.UtcNow.AddDays(365) : DateTime.UtcNow.AddMinutes(Convert.ToDouble(refreshTokenLifeTime)),
                                    UserID = Convert.ToInt64(context.Ticket.Properties.Dictionary["userId"]),
                                    EmployeeID = Convert.ToInt64(context.Ticket.Properties.Dictionary["employeeId"]),
                                    SessionID = Convert.ToInt64(context.Ticket.Properties.Dictionary["sessionId"]),
                                    ProtectedTicket = refreshToken.ProtectedTicket
                                };

                                if (context.Ticket.Properties.Dictionary.ContainsKey("refresh_token"))
                                    context.Ticket.Properties.Dictionary.Remove("refresh_token");

                                if (context.Ticket.Properties.Dictionary.ContainsKey("refresh_token_hash"))
                                    context.Ticket.Properties.Dictionary.Remove("refresh_token_hash");

                                if (context.Ticket.Properties.Dictionary.ContainsKey("access_token"))
                                    context.Ticket.Properties.Dictionary.Remove("access_token");

                                context.Ticket.Properties.Dictionary.Add("refresh_token", token.RefereshToken);
                                context.Ticket.Properties.Dictionary.Add("refresh_token_hash", token.RefereshTokenHash);

                                var result = await _repo.AddRefreshToken(Convert.ToInt64(context.Ticket.Properties.Dictionary["OrganizationID"]),token);

                                var refreshTokenProperties = new AuthenticationProperties(context.Ticket.Properties.Dictionary)
                                {
                                    IssuedUtc = context.Ticket.Properties.IssuedUtc,
                                    ExpiresUtc = token.ExpiresUtc
                                };
                                var refreshTokenTicket = new AuthenticationTicket(context.Ticket.Identity, new AuthenticationProperties(context.Ticket.Properties.Dictionary));
                                context.SetTicket(refreshTokenTicket);
                            }
                        }
                    }
                }
            }catch(Exception exp)
            {
                WebAPI.Common.Logger.Instance(0).Log(exp, WebAPI.Common.Logger.LogType.Error, "ReceiveAsync Simple referesh token");
                var response = context.Response;
                response.StatusCode = 500;
                response.ContentType = "application/json";
                var errorBody = new
                {
                    error = "server_error",
                    error_description = exp.Message
                };
                var json = JsonConvert.SerializeObject(errorBody);
                await response.WriteAsync(json);
            }
        }

        public void Create(AuthenticationTokenCreateContext context)
        {
            throw new NotImplementedException();
        }

        public void Receive(AuthenticationTokenReceiveContext context)
        {
            throw new NotImplementedException();
        }
    }
}