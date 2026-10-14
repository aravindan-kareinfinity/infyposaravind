using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OAuth;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Web;
using InfyPOS.Models;
using Newtonsoft.Json;

namespace WebAPI.Providers
{

    public class AuthRepository : IDisposable
    {
        public void Dispose()
        {

        }

       
        internal async Task<bool> AddRefreshToken(long organizationid,RefreshToken token)
        {
            InfyPOS.Data.UsersessionDataService dataService = new InfyPOS.Data.UsersessionDataService() { Context = new InfyPOS.Common.UserClaim() { OrganizationID = organizationid } };
            var sessions =  dataService.Search(new UsersessionCreteria(){
                id = token.SessionID
            });
            if (sessions == null || sessions.Count != 1 || !sessions[0].isactive) return false;

            InfyPOS.Models.Usersession session = sessions[0];
            session.refreshtoken = token.RefereshToken;
            session.refereshtokenhash = token.RefereshTokenHash;
            session.protectedticket = token.ProtectedTicket ;
            session.refreshtokenexpired = token.ExpiresUtc;
            session.refreshtokenon = token.IssuedUtc;
            session.employeeid = token.EmployeeID;
            session.clientid = token.ClientId;
            session.userid = token.UserID;
            session.username = token.UserName;
            session.isactive = true;
            var retUser = dataService.Update(session);
            return true;
        }

        //public async Task<IdentityResult> RegisterUser(string organizationcode,UserModel userModel)
        //{
        //    InfyPOS.Data.UsersDataService dataService = new InfyPOS.Data.UsersDataService();
        //    InfyPOS.Models.Users user = new InfyPOS.Models.Users();
        //    user.changepassword = true;
        //    user.createdby = 0;
        //    user.createdon = DateTime.Now;
        //    user.employeeid = 0;
        //    user.isactive = true;
        //    user.modifiedby = 0;
        //    user.modifiedon = DateTime.Now;
        //    user.password = "ABC@123";
        //    user.permissions = null;
        //    user.role = 0;
        //    user.username = userModel.UserName;
        //    user.password = userModel.Password;
        //    user.version = 1;
        //    var retUser = dataService.Insert(user);
        //    IdentityResult ir = new IdentityResult();
        //    if (retUser.id == 0)
        //    {
        //        ir = new IdentityResult(new string[] { "Can't register this user" });
        //    }
        //    return ir;
        //}

        internal Client FindClient(string clientId)
        {
            int RefreshTokenLifeTime = 300;
            if(!string.IsNullOrEmpty(System.Configuration.ConfigurationManager.AppSettings["RefreshTokenLifeTime"]))
            {
                if(!int.TryParse(System.Configuration.ConfigurationManager.AppSettings["RefreshTokenLifeTime"], out RefreshTokenLifeTime))
                {
                    RefreshTokenLifeTime = 300;
                }
            }
            Client client = new Client();
            client.Active = true;
            client.AllowedOrigin = "true";
            client.ApplicationType = ApplicationTypes.NativeConfidential;
            client.Id = "1";
            client.Name = "Native";
            client.RefreshTokenLifeTime = RefreshTokenLifeTime;
            client.Secret = "QsDTq6GRy232J3HD+GRsf1PJQRIvmYw8RywCqfth484=";
            return client;
        }

        bool? issaas;
        public bool IsSAAS
        {
            get
            {
                if (issaas.HasValue) return issaas.Value;
                issaas = !string.IsNullOrEmpty(System.Configuration.ConfigurationManager.AppSettings["SAASMODE"]) &&
                    (System.Configuration.ConfigurationManager.AppSettings["SAASMODE"].ToLower() == "true" ||
                    System.Configuration.ConfigurationManager.AppSettings["SAASMODE"].ToLower() == "yes");

                return issaas.Value;
            }
        }

        public long organaizationid { get;  set; }

        internal async Task<InfyPOS.Models.Users> FindUser(string userName, string password,
            string organizationCode,string campusid)
        {
            InfyPOS.Data.UsersDataService dataService = new InfyPOS.Data.UsersDataService();
            if(IsSAAS && organizationCode != "QUANTO")
            {
                if (string.IsNullOrEmpty(organizationCode)) throw new Exception("Please enter the organization code");
                InfyPOS.Data.OrganisationDataService organisationDataService = new InfyPOS.Data.OrganisationDataService();
                var orglist = organisationDataService.Search(new OrganisationCreteria()
                {
                    code = organizationCode
                });
                if(orglist.Count == 0) throw new Exception("Please enter the organization code");
                dataService.Context = new InfyPOS.Common.UserClaim()
                {
                    OrganizationID = orglist[0].id
                };
                this.organaizationid = orglist[0].id;
            }
            var request = new InfyPOS.Models.UsersCreteria();
            request.username = userName;
            request.password = password;
            request.organizationcode = organizationCode;
            request.ValidateLogin = true;
            
            List<InfyPOS.Models.Users> users = dataService.Search(request);
            if (users.Count == 0)
                return null;

            if(users[0].companyid <=0 || string.IsNullOrEmpty(users[0].companyname))
            {
                InfyPOS.Data.CompanyDataService companyserive = new InfyPOS.Data.CompanyDataService();
                companyserive.Context = dataService.Context;

                var clist = companyserive.Search(new CompanyCreteria() { });
                if(clist.Count == 1)
                {
                    users[0].companyname = clist[0].name;
                    users[0].companyid = clist[0].id;
                }

            }

            var usr = users[0];
            usr.organizationid = this.organaizationid;
            usr.organizationcode = organizationCode;
            return usr;
        }

        internal async Task<InfyPOS.Models.Users> FindUser(string organizationCode,string userName, string password)
        {
            InfyPOS.Data.UsersDataService dataService = new InfyPOS.Data.UsersDataService();
            if (IsSAAS)
            {
                if (string.IsNullOrEmpty(organizationCode)) throw new Exception("Please enter the organization code");
                InfyPOS.Data.OrganisationDataService organisationDataService = new InfyPOS.Data.OrganisationDataService();
                var orglist = organisationDataService.Search(new OrganisationCreteria()
                {
                    code = organizationCode
                });
                if (orglist.Count == 0) throw new Exception("Please enter the organization code");
                dataService.Context = new InfyPOS.Common.UserClaim()
                {
                    OrganizationID = orglist[0].id
                };
            }
            var request = new InfyPOS.Models.UsersCreteria();
            request.username = userName;
            request.password = password;
            request.ValidateLogin = true;
            List<InfyPOS.Models.Users> users = dataService.Search(request);
            if (users.Count == 0)
                return null;
            if (users[0].companyid <= 0 || string.IsNullOrEmpty(users[0].companyname))
            {
                InfyPOS.Data.CompanyDataService companyserive = new InfyPOS.Data.CompanyDataService();
                var clist = companyserive.Search(new CompanyCreteria() { });
                if (clist.Count == 1)
                {
                    users[0].companyname = clist[0].name;
                    users[0].companyid = clist[0].id;
                }

            }
            return users[0];
        }

        internal async Task<InfyPOS.Models.Users> FindUser(long userid, long sessionid, string organizationCode, string campusid)
        {
            InfyPOS.Data.UsersDataService dataService = new InfyPOS.Data.UsersDataService();
            if (IsSAAS && organizationCode != "QUANTO")
            {
                if (string.IsNullOrEmpty(organizationCode)) throw new Exception("Please enter the organization code");
                InfyPOS.Data.OrganisationDataService organisationDataService = new InfyPOS.Data.OrganisationDataService();
                var orglist = organisationDataService.Search(new OrganisationCreteria()
                {
                    code = organizationCode
                });
                if (orglist.Count == 0) throw new Exception("Please enter the organization code");
                dataService.Context = new InfyPOS.Common.UserClaim()
                {
                    OrganizationID = orglist[0].id
                };
                this.organaizationid = orglist[0].id;
            }
            var request = new InfyPOS.Models.UsersCreteria();
            request.id = userid;
            request.organizationcode = organizationCode;
            request.ValidateLogin = true;

            List<InfyPOS.Models.Users> users = dataService.Search(request);
            if (users.Count == 0)
                return null;

            InfyPOS.Data.UsersessionDataService usersessionDataService = new InfyPOS.Data.UsersessionDataService()
            {
                Context = new InfyPOS.Common.UserClaim()
                {
                    OrganizationID = this.organaizationid
                }
            };
            var sessions = usersessionDataService.Search(new UsersessionCreteria()
            {
                id = sessionid
            });
            if (sessions.Count == 0)
                return null;

            if (sessions[0].userid != users[0].id)
                return null;

            if (users[0].companyid <= 0 || string.IsNullOrEmpty(users[0].companyname))
            {
                InfyPOS.Data.CompanyDataService companyserive = new InfyPOS.Data.CompanyDataService();
                companyserive.Context = dataService.Context;

                var clist = companyserive.Search(new CompanyCreteria() { });
                if (clist.Count == 1)
                {
                    users[0].companyname = clist[0].name;
                    users[0].companyid = clist[0].id;
                }
            }

            var usr = users[0];
            usr.organizationid = this.organaizationid;
            usr.organizationcode = organizationCode;
            return usr;
        }

        //internal async Task<InfyPOS.Models.Users> FindUser(string organizationcode,UserLoginInfo userinfo)
        //{
        //    InfyPOS.Data.UsersDataService dataService = new InfyPOS.Data.UsersDataService();
        //    var request = new InfyPOS.Models.UsersCreteria();
        //    request.username = userinfo.ProviderKey;
        //    request.externalprovider = userinfo.LoginProvider;
        //    request.ValidateExternalLogin = true;
        //    List<InfyPOS.Models.Users> users = dataService.Search(request);
        //    if (users == null || users.Count == 0)
        //        return null;
        //    if (users[0].companyid <= 0 || string.IsNullOrEmpty(users[0].companyname))
        //    {
        //        InfyPOS.Data.CompanyDataService companyserive = new InfyPOS.Data.CompanyDataService();
        //        var clist = companyserive.Search(new CompanyCreteria() { });
        //        if (clist.Count == 1)
        //        {
        //            users[0].companyname = clist[0].name;
        //            users[0].companyid = clist[0].id;
        //        }

        //    }
        //    return users[0];
        //}

        internal async Task<bool> RemoveRefreshToken(string organizationcode,string hashedTokenId)
        {
            InfyPOS.Data.UsersessionDataService dataService = new InfyPOS.Data.UsersessionDataService();
            var sessions = dataService.Search(new UsersessionCreteria()
            {
                refereshtokenhash = hashedTokenId
            });
            if (sessions == null || sessions.Count != 1 || !sessions[0].isactive) return false;
            sessions[0].refereshtokenhash = "";
            sessions[0].refreshtoken = "";
            sessions[0].refreshtokenexpired = DateTime.UtcNow;
            dataService.Update(sessions[0]);
            return true;
        }

        public async Task<bool> RemoveRefreshToken(string organizationcode,RefreshToken refreshToken)
        {
            InfyPOS.Data.UsersessionDataService dataService = new InfyPOS.Data.UsersessionDataService();
            var sessions = dataService.Search(new UsersessionCreteria()
            {
                refereshtokenhash = refreshToken.RefereshTokenHash
            });
            if (sessions == null || sessions.Count != 1 || !sessions[0].isactive) return false;
            sessions[0].refereshtokenhash = "";
            sessions[0].refreshtoken = "";
            sessions[0].refreshtokenexpired = DateTime.UtcNow;
            dataService.Update(sessions[0]);
            return true;
        }

        public async Task<RefreshToken> FindRefreshToken(long organizationid,string refreshTokenId)
        {
            InfyPOS.Data.UsersessionDataService dataService = new InfyPOS.Data.UsersessionDataService();
            var sessions = dataService.Search(new UsersessionCreteria()
            {
                refereshtokenhash = refreshTokenId
            });
            if (sessions == null) throw new Exception("Can't find the token from the server");
            if (sessions.Count != 1) throw new Exception("Can't find one token from the server");
            if (!sessions[0].isactive)
            {
                if (sessions[0].reason == "KI-CREATE SESSION") throw new Exception("Deactivated session because of logged in another device");
                if (sessions[0].reason == "LOGOUT-SESSION") throw new Exception("Deactivated session because of logged out in this device");
                throw new Exception("Deactivated session because of " + sessions[0].reason);
            }

            RefreshToken rf = new RefreshToken();
            rf.ExpiresUtc = DateTime.SpecifyKind(sessions[0].refreshtokenexpired, DateTimeKind.Utc);
            rf.RefereshToken = sessions[0].refreshtoken;
            rf.RefereshTokenHash = sessions[0].refereshtokenhash;
            rf.ProtectedTicket = sessions[0].protectedticket;
            rf.SessionID = sessions[0].id;
            rf.UserID = sessions[0].userid;
            rf.UserName = sessions[0].username;
            rf.ClientId = sessions[0].clientid;
            rf.EmployeeID = sessions[0].employeeid;
            //rf.HasKilled = sessions[0].killedBy != 0;
            //rf.KilledBy = sessions[0].killedBy == -1 ? sessions[0].username : sessions[0].sessionkilledby;
            rf.IssuedUtc = DateTime.SpecifyKind(sessions[0].refreshtokenon, DateTimeKind.Utc);
            return rf;
        }

        //internal async Task<IdentityResult> Create(string organizationcode,UserLoginInfo userinfo)
        //{
        //    InfyPOS.Data.UsersDataService dataService = new InfyPOS.Data.UsersDataService();
        //    var request = new InfyPOS.Models.UsersCreteria();
        //    request.username = userinfo.ProviderKey;
        //    request.externalprovider = userinfo.LoginProvider;
        //    request.ValidateExternalLogin = true;
        //    List<InfyPOS.Models.Users> users = dataService.Search(request);
        //    if (users == null || users.Count == 0)
        //    {
        //        IdentityResult ir = new IdentityResult(new string[] { "Can't register" });
        //        return ir;
        //    }
        //    else
        //    {
        //        IdentityResult ir = new IdentityResult();
        //        return ir;
        //    }
        //}

        public static string GetHash(string input)
        {
            HashAlgorithm hashAlgorithm = new SHA256CryptoServiceProvider();

            byte[] byteValue = System.Text.Encoding.UTF8.GetBytes(input);

            byte[] byteHash = hashAlgorithm.ComputeHash(byteValue);

            return Convert.ToBase64String(byteHash);
        }

        public class Token
        {
            public string Key { get; set; }
            public string Hash { get; set; }
        }

        public partial class AuthenticationResponse
        {
            [JsonProperty("access_token")]
            
            public string AccessToken { get; set; }

            [JsonProperty("token_type")]
            public string TokenType { get; set; }

            [JsonProperty("expires_in")]
            public long ExpiresIn { get; set; }

            [JsonProperty("refresh_token")]
            public string RefreshToken { get; set; }

            [JsonProperty("as:client_id")]
            public string AsClientId { get; set; }

            [JsonProperty("userName")]
            public string UserName { get; set; }

            [JsonProperty("employeeId")]
            public long EmployeeId { get; set; }

            [JsonProperty("userId")]
            public long UserId { get; set; }

            [JsonProperty("sessionId")]
            public long SessionId { get; set; }

            [JsonProperty("Title")]
            public string Title { get; set; }

            [JsonProperty("Designation")]
            public string Designation { get; set; }

            [JsonProperty("OrganizationID")]
            public long OrganizationId { get; set; }

            [JsonProperty("OrganizationCode")]
            public long OrganizationCode { get; set; }

            [JsonProperty("CompanyName")]
            public string CompanyName { get; set; }

            [JsonProperty("LandingScreen")]
            public string LandingScreen { get; set; }

            [JsonProperty("Passcode")]
            public string Passcode { get; set; }

            [JsonProperty("RootUser")]
            public string RootUser { get; set; }

            [JsonProperty("AD")]
            public long Ad { get; set; }

            [JsonProperty("KU")]
            public long Ku { get; set; }

            [JsonProperty(".issued")]
            public string Issued { get; set; }

            [JsonProperty(".expires")]
            public string Expires { get; set; }
        }
        internal Token GetNextToken()
        {
            Token token = new Token();
            token.Key  = Guid.NewGuid().ToString("n");
            token.Hash = GetHash(token.Key);
            return token;
        }

        internal async Task<Usersession> CreateSession(long organizatoinid, Users.LoggedInUser luser)
        {
            var user = luser.user;
            
            Usersession session = new Usersession();
            int utcoffset = 0;
            int.TryParse(luser.utcoffset, out utcoffset);
            session.utcoffset = utcoffset * -1;
            InfyPOS.Data.UsersessionDataService dataService = new InfyPOS.Data.UsersessionDataService()
            { Context = new InfyPOS.Common.UserClaim() { OrganizationID = organizatoinid, TZ = session.utcoffset, UserID = user.id } };
            session.createdby = 0;
            session.createdon = dataService.DateTimeNow;
            session.isactive = true;
            session.loginOn = dataService.DateTimeNow;
            session.logoutOn = DateTime.MaxValue;
            session.modifiedby = 0;
            session.modifiedon = dataService.DateTimeNow;
            if (!DisableRefreshToken)
            {
                dataService.KillUser(new Usersession()
                {
                    userid = user.id,
                    reason = "CREATE SESSION",
                    platform = luser.platform
                });
                session.removeusercount = dataService.ValidateUserCount(luser.platform);
            }
            session.refreshtokenexpired = DisableRefreshToken ? DateTime.UtcNow.AddDays(365) : DateTime.UtcNow.AddMinutes(Convert.ToDouble(5));
            session.refreshtokenon = DateTime.UtcNow;
            session.userid = user.id;
            session.platform = luser.platform;
            session.deviceid = luser.deviceid;
            session.employeeid = user.employeeid;
            var token = GetNextToken();
            session.token = token.Key;
            session.refreshtoken = token.Key;
            session.tokenexpired = session.refreshtokenexpired;
            session.version = 1;
            dataService.Insert(session);
            return session;
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
        //internal async Task<IdentityResult> AddLogin(string username, UserLoginInfo login)
        //{
        //    IdentityResult ir = new IdentityResult();
        //    return ir;
        //}
    }
}