using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
using WebAPI.Data;
using System.Security.Cryptography;
using System.Net;
using System.Net.Http;

namespace InfyPOS.Data
{
    public class UsersDataService : BaseData
    {
        const string SQL_Search = @"select users.id, users.isactive, users.createdby, users.modifiedby, users.version, users.createdon, users.alloweddiscount,users.allowedcredit,
                                           users.modifiedon, users.username, permissions, accessiblelocation, accessiblecompany, password, role, employee.companyid,users.accesskey,
                                           landingscreen, employeeid, changepassword, lastloginon,externalprovider,actualuserid,organizationid,security,
                                           organizationcode,employee.name title,referencelist.name designation,company.name companyname,accessablesections,passcode,passcodeon from users
                                           left outer join employee on employee.id = users.employeeid
                                           left outer join referencelist on referencelist.id = employee.desingnationid
					                       left outer join company on company.id = employee.companyid
                                           where users.id=@id";

        internal Users GetMyPermission()
        {
            return Search(new UsersCreteria()
            {
                id = Context.UserID
            }).First();
        }

        public class ConnectionInfo
        {
            public string BearerToken { get; set; }
            public string RefreshToken { get; set; }
            public Models.Users UserDetail { get; set; }
            public Models.Employee EmployeeDetail { get; set; }
            public List<PrintServer> Printers { get; set; }
            public string ServiceURL { get; set; }

            public long userid { get; set; }
            public long employeeid { get; set; }
            public string UniqueID { get; set; }
            public string OS { get; set; }
            [Newtonsoft.Json.JsonIgnore]
            public long SessionID { get; set; }
        }

        //public BearerOAuthTokenDeserializer.AuthenticationTicket DecodeBearerToken(string btoken)
        //{
        //    BearerOAuthTokenDeserializer.AuthenticationTicket token = null;
        //    try
        //    {
        //        token = BearerOAuthTokenDeserializer.DataProtector.Create().Unprotect(btoken);
        //    }
        //    catch (Exception ex)
        //    {
        //        return null;
        //    }
        //    return token;
        //}
        public async System.Threading.Tasks.Task<ConnectionInfo> ConnectAsync(string token, string serverinfo)
        {
            if (!CanRunONSAAS())
            {
                throw new Exception("SAAS Application can't run without organization information");
            }

            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;

            var tokenparts = token.Split('~');

            ConnectionInfo connectionInfo = new ConnectionInfo()
            {
                RefreshToken = token,
                ServiceURL = serverinfo
            };

            if (tokenparts.Length == 3)
            {
                token = tokenparts[0];
                connectionInfo = new ConnectionInfo()
                {
                    RefreshToken = tokenparts[0],
                    UniqueID = tokenparts[1],
                    OS = tokenparts[2],
                    ServiceURL = serverinfo
                };
            }
            var param = new SortedList<string, object>();
            param.Add("refreshtoken", token);
            string query = "Select userid,employeeid,accesstoken,id from usersession where refreshtoken=@refreshtoken and isactive = true limit 1";
            using (var reader = ExecuteReader(query, param))
            {
                if (reader.Read())
                {
                    connectionInfo.userid = reader.GetInt64(0);
                    connectionInfo.employeeid = reader.GetInt64(1);
                    connectionInfo.BearerToken = Cryptor.DecryptString(reader.GetString(2), Cryptor.DefaultToken);
                    connectionInfo.SessionID = reader.GetInt64(3);
                }
            }
            if (connectionInfo.SessionID == 0)
            {
                throw new Exception("Please relogin to quanto application.");
            }

            if (string.IsNullOrEmpty(connectionInfo.OS))
            {
                throw new Exception("Platform information missing, upgrade your app.");
            }

            if (connectionInfo.userid > 0)
            {
                connectionInfo.UserDetail = Search(new UsersCreteria()
                {
                    id = connectionInfo.userid
                }).First();

                if (Services.ConfigManager.Instance(Context.OrganizationID).Config.system_locationrestriction)
                {
                    if (!(connectionInfo.UserDetail.security != null && (connectionInfo.UserDetail.security.globaluser || (
                        connectionInfo.UserDetail.permissions.Exists(e => e.code == "SECURITY_GLOBALUSER") &&
                        connectionInfo.UserDetail.permissions.Find(e => e.code == "SECURITY_GLOBALUSER").view))))
                    {
                        //throw new Exception("Location information not found, can't login from out of office.");
                    }
                }
            }

            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(connectionInfo.ServiceURL + "/token");
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, client.BaseAddress);

            List<KeyValuePair<string, string>> postData = new List<KeyValuePair<string, string>>();

            postData.Add(new KeyValuePair<string, string>("grant_type", "password"));
            postData.Add(new KeyValuePair<string, string>("username", ""));
            postData.Add(new KeyValuePair<string, string>("password", ""));
            postData.Add(new KeyValuePair<string, string>("userid", connectionInfo.userid.ToString()));
            postData.Add(new KeyValuePair<string, string>("sessionid", connectionInfo.SessionID.ToString()));
            postData.Add(new KeyValuePair<string, string>("org", Context.OrganizationID.ToString()));
            postData.Add(new KeyValuePair<string, string>("client_id", "InfyPOS"));
            postData.Add(new KeyValuePair<string, string>("client_secret", "KareInfinity"));
            postData.Add(new KeyValuePair<string, string>("deviceid", connectionInfo.UniqueID));
            postData.Add(new KeyValuePair<string, string>("platform", connectionInfo.OS));

            request.Content = new FormUrlEncodedContent(postData);
            HttpResponseMessage tokenResponse = client.PostAsync(connectionInfo.ServiceURL + "/token",
                new FormUrlEncodedContent(postData)).Result;

            var response = await tokenResponse.Content.ReadAsAsync<WebAPI.Providers.AuthRepository.AuthenticationResponse>();
            connectionInfo.BearerToken = response.AccessToken;
            connectionInfo.RefreshToken = response.RefreshToken;
            if (connectionInfo.userid > 0)
            {


                CompanyDataService companyDataService = new CompanyDataService() { Context = this.Context };
                connectionInfo.Printers = companyDataService.SearchPrinters(false);

                EmployeeDataService employeeDataService = new EmployeeDataService() { Context = this.Context };
                var employee = employeeDataService.Search(new EmployeeCreteria()
                {
                    id = connectionInfo.employeeid,
                    hidesalaryinfo = true
                });

                if (employee.Count > 0)
                    connectionInfo.EmployeeDetail = employee.First();
            }
            return connectionInfo;
        }

        internal WebAPI.Providers.AuthRepository.Token GetNextToken()
        {
            WebAPI.Providers.AuthRepository.Token token = new WebAPI.Providers.AuthRepository.Token();
            token.Key = Guid.NewGuid().ToString("n");
            token.Hash = GetHash(token.Key);
            return token;
        }
        public static string GetHash(string input)
        {
            HashAlgorithm hashAlgorithm = new SHA256CryptoServiceProvider();

            byte[] byteValue = System.Text.Encoding.UTF8.GetBytes(input);

            byte[] byteHash = hashAlgorithm.ComputeHash(byteValue);

            return Convert.ToBase64String(byteHash);
        }

        public class PasscodeInfo
        {
            public string employee { get; set; }
            public bool success { get; set; }
            public Permissions permission { get; set; }
            public decimal alloweddiscount { get; set; }
            public decimal allowedcredit { get; set; }
        }

        internal string MyPasscode()
        {
            if (!CanRunONSAAS())
            {
                throw new Exception("SAAS Application can't run without organization information");
            }
            var user = Search(new UsersCreteria()
            {
                id = Context.UserID
            });
            if (user.Count == 1)
                return WebAPI.Data.Cryptor.DecryptString(user[0].passcode, WebAPI.Data.Cryptor.DefaultToken);
            return "Login..";
        }

        public bool MatchCurrentPasscode(string employeecode, string comparepasscode)
        {
            if (!CanRunONSAAS())
            {
                throw new Exception("SAAS Application can't run without organization information");
            }
            SortedList<string, object> param = new SortedList<string, object>();
            param.Add("employeecode", employeecode);

            string query = @"select passcode from users
                inner join employee on employee.id = employeeid 
                where users.isactive = true and employee.employeecode = @employeecode and employee.isactive=true";
            PasscodeInfo p = new PasscodeInfo();
            string passcode = "";
            using (var reader = ExecuteReader(query, param))
            {
                if (reader.Read())
                {
                    passcode = reader.IsDBNull(0) ? "" : reader.GetString(0);
                    if (!string.IsNullOrEmpty(passcode))
                        passcode = Cryptor.DecryptString(passcode, Cryptor.DefaultToken);
                }
                if(reader.Read())
                {
                    throw new Exception("Multiple employee connected with the given employee code " + employeecode);
                }
            }
            return comparepasscode == passcode;
        }


        public bool SendCurrentPasscode(string employeecode)
        {
            if (!CanRunONSAAS())
            {
                throw new Exception("SAAS Application can't run without organization information");
            }
            SortedList<string, object> param = new SortedList<string, object>();
            param.Add("employeecode", employeecode);

            string query = @"select employee.name,users.id,employee.contactno,passcode from users
                inner join employee on employee.id = employeeid 
                where users.isactive = true and employee.employeecode = @employeecode and employee.isactive=true";
            PasscodeInfo p = new PasscodeInfo();
            long userid = 0;
            string mobileno = "";
            string passcode = "";
            using (var reader = ExecuteReader(query, param))
            {
                if (reader.Read())
                {
                    p.employee = reader.GetString(0);
                    userid = reader.GetInt64(1);
                    mobileno = reader.IsDBNull(2) ? "" : reader.GetString(2);
                    passcode = reader.IsDBNull(3) ? "" : reader.GetString(3);
                    if (!string.IsNullOrEmpty(passcode))
                        passcode = Cryptor.DecryptString(passcode, Cryptor.DefaultToken);
                }
            }

            if (string.IsNullOrEmpty(passcode)) passcode = CreatePasscode(userid, passcode);
            if (!string.IsNullOrEmpty(mobileno) && mobileno.Length >= 10)
            {
                var passcodemsg = string.Format("Your New PASSCODE is {0}", passcode);
                if (!string.IsNullOrEmpty(InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.approvalotpfmt))
                    passcodemsg = InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.approvalotpfmt.Replace("[OTP]", passcode);

                if (Processors.Schedule_Actions.Instance.HasConfig(this.Context.OrganizationID,
                   Processors.Schedule_Actions.SourceType.OTP, Processors.Schedule_Actions.Actions.Otp_Discount))
                {
                    Processors.Schedule_Actions.Instance.OnChange(this.Context.OrganizationID, new BillOTP()
                    {
                        OTP = passcode,
                        MOBILE = mobileno
                    }, Processors.Schedule_Actions.Actions.Otp_Discount);
                    return true;
                }
                else
                {
                    if (!string.IsNullOrEmpty(InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.approvaltemplateurl))
                        InfyPOS.Services.SMSManager.Instance.SendSMSURLSchedule(Context.OrganizationID, 0,
                            InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.approvaltemplateurl, mobileno, passcodemsg);
                    else
                        InfyPOS.Services.SMSManager.Instance.SendSMS(Context.OrganizationID, mobileno, passcodemsg);
                    return true;
                }
            }
            else
            {
                return false;
            }
        }

        internal Users GenerateAccessKey(Users item)
        {
            var user = Search(new UsersCreteria()
            {
                id = item.id
            }).First();
            if (user.version == item.version)
            {
                var newcode = Token.RandomString(16);
                user.accesskey = newcode;
                return Update(user);
            }
            else
            {
                throw new Exception("Please reload the users to generate access key");
            }
        }

        internal PasscodeInfo Passcode(UserPASSCODE item)
        {
            if (!CanRunONSAAS())
            {
                throw new Exception("SAAS Application can't run without organization information");
            }
            SortedList<string, object> param = new SortedList<string, object>();
            param.Add("employeecode", item.code);

            string query = @"select employee.name,permissions,users.id,employee.contactno,alloweddiscount,passcode,allowedcredit from users
                inner join employee on employee.id = employeeid 
                where users.isactive = true and employee.employeecode = @employeecode and employee.isactive=true";
            PasscodeInfo p = new PasscodeInfo();
            long userid = 0;
            string mobileno = "";
            string passcode = "";
            
            using (var reader = ExecuteReader(query, param))
            {
                if (reader.Read())
                {
                    p.employee = reader.GetString(0);
                    userid = reader.GetInt64(2);
                    mobileno = reader.IsDBNull(3) ? "" : reader.GetString(3);
                    passcode = reader.IsDBNull(5) ? "" : reader.GetString(5);
                    if (!string.IsNullOrEmpty(passcode))
                        passcode = Cryptor.DecryptString(passcode, Cryptor.DefaultToken);

                    var permissions = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Permissions>>(reader.GetString(1));
                    if (permissions != null && permissions.Exists(e => e.code == item.permission))
                    {
                        p.success = true;
                        p.alloweddiscount = reader.IsDBNull(4) ? 0 : reader.GetDecimal(4);
                        //if (p.alloweddiscount == 0)
                        //    p.alloweddiscount = 100;
                        p.allowedcredit = reader.IsDBNull(6) ? 0 : reader.GetDecimal(6);
                        p.permission = permissions.First(e => e.code == item.permission);
                    }
                }
                if(reader.Read())
                {
                    throw new Exception("Multiple employee connected with the given employee code "+ item.code);
                }
            }

            if (p.permission == null || !p.permission.view)
            {
                throw new Exception("The given user doesn't have permission to approve discount.");
            }

            var locationname = "";
            if (item.locationid > 0)
            {
                param.Add("locationid", item.locationid);
                using (var reader = ExecuteReader("select name from referencelist where id=@locationid", param))
                {
                    if (reader.Read())
                    {
                        locationname = reader.IsDBNull(0) ? "" : reader.GetString(0);
                    }
                }
            }
            var countername = "";
            if (item.source != null && item.source.counterid > 0)
            {
                param.Add("counterid", item.source.counterid);
                using (var reader = ExecuteReader("select name from referencelist where id=@counterid", param))
                {
                    if (reader.Read())
                    {
                        countername = reader.IsDBNull(0) ? "" : reader.GetString(0);
                    }
                }
            }

            if (userid > 0)
            {
                if (!string.IsNullOrEmpty(mobileno) && mobileno.Length >= 10 &&
                            InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig != null &&
                            InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig.ContainsKey(item.locationid) &&
                            InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[item.locationid].sale_requestpasscodetosms)
                {
                    if (string.IsNullOrEmpty(passcode)) passcode = CreatePasscode(userid, passcode);

                    if (item.source != null &&
                        item.source.customerid > 0)
                    {
                        CustomerDataService customerDataService = new CustomerDataService() { Context = this.Context };
                        var customer = customerDataService.Search(new CustomerCreteria() { id = item.source.customerid });
                        if(customer.Count>0)
                        {
                            item.source.customer = customer[0];
                        }
                    }

                    var percentage = Math.Round(Math.Abs(item.source.discount) / (Math.Abs(item.source.receivable) + Math.Abs(item.source.discount)) * 100, 2);
                    string msg = string.Format("Bill Value:{0}\r\nDiscount:{1}({2}%)\r\n{3}",
                        Math.Round(item.source.receivable + item.source.discount, 2),
                        Math.Round(item.source.discount, 2), percentage, locationname);
                    var passcodemsg = string.Format("Your New PASSCODE is {0}\r\n{1}", passcode, msg);

                    if (p.alloweddiscount < percentage)
                    {
                        throw new Exception(string.Format("The given user have permission to approve upto {0}% discount.", p.alloweddiscount));
                    }

                    if (Processors.Schedule_Actions.Instance.HasConfig(this.Context.OrganizationID,
                       Processors.Schedule_Actions.SourceType.OTP, Processors.Schedule_Actions.Actions.Otp_Discount))
                    {
                        Processors.Schedule_Actions.Instance.OnChange(this.Context.OrganizationID, new BillOTP()
                        {
                            OTP = passcode,
                            MOBILE = mobileno,
                            CUSTOMER = item.source == null ? "" : item.source.customername,
                            CUSTOMERMOBILENO = item.source == null ? "" : item.source.phoneno,
                            BILLAMOUNT = Math.Round(item.source.receivable + item.source.discount, 2),
                            DISCOUNT = Math.Round(item.source.discount, 2),
                            DISCOUNTPERCENT = percentage,
                            NETAMOUNT = Math.Round(item.source.receivable, 2),
                            LOCATION = locationname,
                            COUNTER = countername,
                            locationid = item.source.locationid,
                            ALLOWEDCREDIT = p.allowedcredit,
                            BILLNO = item.source.billno,
                            BILLDATE = item.source.billdate,
                            REASON = item.source.discountrequestreason,
                        }, Processors.Schedule_Actions.Actions.Otp_Discount);
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.approvalotpfmt))
                        {
                            passcodemsg = InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.approvalotpfmt.Replace("[OTP]", passcode);
                            passcodemsg = passcodemsg.Replace("[BILLAMOUNT]", Math.Round(item.source.receivable + item.source.discount, 2).ToString());
                            passcodemsg = passcodemsg.Replace("[DISCOUNT]", Math.Round(item.source.discount, 2).ToString());
                            passcodemsg = passcodemsg.Replace("[DISCOUNTPERCENT]", percentage.ToString());
                            passcodemsg = passcodemsg.Replace("[NETAMOUNT]", Math.Round(item.source.receivable, 2).ToString());
                            passcodemsg = passcodemsg.Replace("[ALLOWEDCREDIT]", Math.Round(p.allowedcredit, 2).ToString());
                            passcodemsg = passcodemsg.Replace("[LOCATION]", locationname);
                            passcodemsg = passcodemsg.Replace("[COUNTER]", countername);
                            passcodemsg = passcodemsg.Replace("[BILLNO]", item.source.billno);
                            passcodemsg = passcodemsg.Replace("[BILLDATE]", item.source.billdate.ToString("dd/MM/yyyy"));
                            passcodemsg = passcodemsg.Replace("[REASON]", item.source.discountrequestreason);
                            passcodemsg = passcodemsg.Replace("[MOBILE]", mobileno);
                            passcodemsg = passcodemsg.Replace("[CUSTOMER]", item.source == null ? "" : item.source.customername);
                            passcodemsg = passcodemsg.Replace("[CUSTOMERMOBILENO]", item.source == null ? "" : item.source.phoneno);
                        }

                        if (!string.IsNullOrEmpty(InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.approvaltemplateurl))
                            InfyPOS.Services.SMSManager.Instance.SendSMSURLSchedule(Context.OrganizationID, 0,
                                InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.approvaltemplateurl, mobileno, passcodemsg);
                        else
                            InfyPOS.Services.SMSManager.Instance.SendSMS((Context.OrganizationID), mobileno, passcodemsg);
                    }


                    using (NpgsqlConnection conn = CreateConnection())
                    {
                        conn.Open();
                        var transaction = conn.BeginTransaction();
                        InfyPOS.Services.IntrayManager.Instance(Context.OrganizationID).Add(Context.OrganizationID,
                            Services.IntrayManager.EntryType.Discount, userid, passcodemsg, Context.UserID,
                            item.locationid, transaction, Context);
                        transaction.Commit();
                        conn.Close();
                    }
                }
            }
            return p;
        }

        internal PasscodeInfo Passcode(string ecode, string passcode, string permissioncode, long locationid)
        {
            if (!CanRunONSAAS())
            {
                throw new Exception("SAAS Application can't run without organization information");
            }
            long userid = 0;
            string mobileno = "0";
            passcode = Cryptor.EncryptString(passcode, Cryptor.DefaultToken);
            SortedList<string, object> param = new SortedList<string, object>();
            param.Add("passcode", passcode);
            param.Add("ecode", ecode);

            string query = @"
                select employee.name,permissions,users.id,employee.contactno,alloweddiscount,allowedcredit from users
                inner join employee on employee.id = employeeid 
                where users.isactive = true and users.passcode = @passcode and employee.isactive=true and employee.employeecode = @ecode ";
            PasscodeInfo p = new PasscodeInfo();
            using (var reader = ExecuteReader(query, param))
            {
                if (reader.Read())
                {
                    p.employee = reader.GetString(0);
                    userid = reader.GetInt64(2);
                    mobileno = reader.IsDBNull(3) ? "" : reader.GetString(3);
                    var permissions = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Permissions>>(reader.GetString(1));
                    if (permissions != null && permissions.Exists(e => e.code == permissioncode))
                    {
                        p.success = true;
                        p.alloweddiscount = reader.IsDBNull(4) ? 0 : reader.GetDecimal(4);
                        //if (p.alloweddiscount == 0)
                        //    p.alloweddiscount = 100;
                        p.allowedcredit = reader.IsDBNull(5) ? 0 : reader.GetDecimal(5);
                        p.permission = permissions.First(e => e.code == permissioncode);
                    }
                }
            }
            if (p.permission == null || !p.permission.view)
            {
                throw new Exception("The given user doesn't have permission to approve discount.");
            }

            if (p.success)
            {
                if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig != null &&
                        InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig.ContainsKey(locationid) &&
                        (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[locationid].sale_regeneratepasscode ||
                        InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[locationid].sale_requestpasscodetosms))
                {
                    var newcode = CreatePasscode(userid, passcode);

                    if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[locationid].sale_regeneratepasscode &&
                        !string.IsNullOrEmpty(mobileno) && mobileno.Length >= 10 &&
                        InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig != null &&
                        InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig.ContainsKey(locationid) &&
                        InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[locationid].sale_sendpasscodetosms)
                    {

                        if (Processors.Schedule_Actions.Instance.HasConfig(this.Context.OrganizationID,
                                Processors.Schedule_Actions.SourceType.OTP, Processors.Schedule_Actions.Actions.Otp_Discount))
                        {
                            Processors.Schedule_Actions.Instance.OnChange(this.Context.OrganizationID, new BillOTP()
                            {
                                OTP = passcode,
                                MOBILE = mobileno
                            }, Processors.Schedule_Actions.Actions.Otp_Discount);
                        }
                        else
                        {
                            var passcodemsg = string.Format("Your New PASSCODE is {0}", newcode);
                            if (!string.IsNullOrEmpty(InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.approvalotpfmt))
                                passcodemsg = InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.approvalotpfmt.Replace("[OTP]", newcode);

                            if (!string.IsNullOrEmpty(InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.approvaltemplateurl))
                                InfyPOS.Services.SMSManager.Instance.SendSMSURLSchedule(Context.OrganizationID, 0,
                                    InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.approvaltemplateurl, mobileno, passcodemsg);
                            else
                                InfyPOS.Services.SMSManager.Instance.SendSMS((Context.OrganizationID), mobileno, passcodemsg);
                        }

                    }
                }
            }
            return p;
        }

        const string SQL_Insert = @"INSERT INTO public.users(isactive, createdby, modifiedby, version, createdon, modifiedon, username, permissions, password, role, landingscreen, employeeid, changepassword, lastloginon,externalprovider,actualuserid,organizationid,organizationcode,accessiblelocation,accessablesections,alloweddiscount,security,accesskey,allowedcredit,accessiblecompany) 
                                    VALUES(@isactive, @createdby, @modifiedby, @version, @createdon, @modifiedon, @username, @permissions, @password, @role, @landingscreen, @employeeid, @changepassword, @lastloginon,@externalprovider,@actualuserid,@organizationid,@organizationcode,@accessiblelocation,@accessablesections,@alloweddiscount,@security,@accesskey,@allowedcredit,@accessiblecompany) RETURNING id";

        const string SQL_Update = @"UPDATE public.users SET isactive=@isactive, modifiedby=@modifiedby, version=@version+1, modifiedon=@modifiedon, username=@username, permissions=@permissions, 
                                    role=@role, landingscreen=@landingscreen, employeeid=@employeeid, alloweddiscount=@alloweddiscount,security=@security, allowedcredit=@allowedcredit, accesskey=@accesskey,
                                    accessiblelocation=@accessiblelocation,accessablesections=@accessablesections,accessiblecompany=@accessiblecompany where id=@id and version=@version  RETURNING version";


        const string SQL_Delete = @"update public.users set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version  RETURNING version";

        public List<Users> Search(UsersCreteria _users)
        {
            if (LicenseManager.Instance(Context.OrganizationID).GetLicense().valiate_upto)
            {
                if (LicenseManager.Instance(Context.OrganizationID).GetLicense().validupto < DateTimeNow)
                {
                    throw new Exception("License expired, please contact finance team, Store Soft Solution Pvt Ltd.");
                }
            }

            List<Users> lst = new List<Users>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                NpgsqlCommand command = null;
                if (_users.id != 0)
                {
                    command = GetCommand(SQL_Search, conn);
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));

                    if (_users.inactive)
                        qb.AddParameter("users.isactive", false, "=");
                    else
                        qb.AddParameter("users.isactive", true, "=");

                    if(_users.ids != null &&
                        _users.ids.Count>0)
                    {
                        qb.AddInParamer("users.id", string.Join(",", _users.ids));
                    }

                    if (_users.ValidateLogin)
                    {
                        qb.AddParameter("users.username", _users.username, "=");
                        //qb.AddParameter("users.password",  _users.password, "=");
                        qb.AddORParameter("users.password", _users.password, "=", "users.hashpassword", "users.password", Cryptor.EncryptString(_users.password, Cryptor.DefaultToken), "=");
                    }
                    else
                    {
                        if (_users.role > 0)
                        {
                            qb.AddParameter("users.role", _users.role, " = ");
                        }
                        else if (_users.ValidateExternalLogin)
                        {
                            qb.AddParameter("users.username", _users.username, "=");
                            qb.AddParameter("users.externalprovider", _users.externalprovider, "=");
                        }
                        if (_users.employeeid > 0)
                            qb.AddParameter("employee.id", _users.employeeid, "=");
                        else if (!string.IsNullOrEmpty(_users.username))
                            qb.AddParameter("users.username", "%" + _users.username + "%", " ilike ");
                        if (_users.role <= 0)
                            qb.AddParameter("users.role", 999, "!=");
                        if (!string.IsNullOrEmpty(_users.accesskey))
                            qb.AddParameter("users.accesskey", _users.accesskey, "=");
                    }
                    //if (!string.IsNullOrEmpty(_users.Code))
                    //    qb.AddParameter("code", _users.Code, "=");

                    command = qb.GetCommand(conn);

                    //WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(new Exception(GetSQL(command)), WebAPI.Common.Logger.LogType.Error, _users);
                }
                command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _users.id;
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Users ousers = new Users();
                        ousers.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);

                        ousers.companyname = reader["companyname"] == DBNull.Value ? "" : reader["companyname"].ToString();
                        ousers.companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"]);
                        ousers.organizationid = reader["organizationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["organizationid"]);
                        ousers.role = reader["role"] == DBNull.Value ? 0 : Convert.ToInt32(reader["role"]);
                        ousers.organizationcode = reader["organizationcode"] == DBNull.Value ? "" : reader["organizationcode"].ToString();
                        ousers.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        ousers.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        ousers.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        ousers.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        ousers.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        ousers.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"].ToString());
                        ousers.username = reader["username"] == DBNull.Value ? "" : reader["username"].ToString();
                        ousers.alloweddiscount = reader["alloweddiscount"] == DBNull.Value ? 100 : Convert.ToDecimal(reader["alloweddiscount"].ToString());
                        ousers.allowedcredit = reader["allowedcredit"] == DBNull.Value ? 100 : Convert.ToDecimal(reader["allowedcredit"].ToString());
                        //if (ousers.alloweddiscount == 0)
                        //if (ousers.alloweddiscount == 0)
                        //    ousers.alloweddiscount = 100;
                        //if (_users.ValidateLogin)
                        //{
                        //    ousers.password = reader["password"] == DBNull.Value ? "" : reader["password"].ToString();
                        //}
                        ousers.title = reader["title"] == DBNull.Value ? "" : reader["title"].ToString();
                        ousers.designation = reader["designation"] == DBNull.Value ? "" : reader["designation"].ToString();
                        ousers.permissions_json = reader["permissions"] == DBNull.Value ? "null" : reader["permissions"].ToString();

                        ousers.landingscreen = reader["landingscreen"] == DBNull.Value ? "" : reader["landingscreen"].ToString();
                        ousers.externalprovider = reader["externalprovider"] == DBNull.Value ? "" : reader["externalprovider"].ToString();
                        ousers.actualuserid = reader["actualuserid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["actualuserid"].ToString());
                        ousers.employeeid = reader["employeeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["employeeid"].ToString());
                        ousers.changepassword = reader["changepassword"] == DBNull.Value ? false : Convert.ToBoolean(reader["changepassword"].ToString());
                        ousers.lastloginon = reader["lastloginon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["lastloginon"].ToString());

                        ousers.passcodeon = reader["passcodeon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["passcodeon"].ToString());
                        ousers.passcode = reader["passcode"] == DBNull.Value ? "" : reader["passcode"].ToString();
                        ousers.security_json = reader["security"] == DBNull.Value ? "null" : reader["security"].ToString();
                        ousers.accesskey = reader["accesskey"] == DBNull.Value ? "" : reader["accesskey"].ToString();

                        ousers.accessablesections_json = reader["accessablesections"] == DBNull.Value ? "null" : reader["accessablesections"].ToString();
                        ousers.accessiblelocation_json = reader["accessiblelocation"] == DBNull.Value ? "null" : reader["accessiblelocation"].ToString();
                        ousers.accessiblecompany_json = reader["accessiblecompany"] == DBNull.Value ? "null" : reader["accessiblecompany"].ToString();
                        if (ousers.accessablesections == null) ousers.accessablesections = new List<Users.ResourcePermission>();
                        if (ousers.accessiblelocation == null) ousers.accessiblelocation = new List<Users.ResourcePermission>();
                        if (ousers.accessiblecompany == null) ousers.accessiblecompany = new List<Users.ResourcePermission>();
                        if (ousers.permissions == null) ousers.permissions = new List<Permissions>();

                        if (!ousers.permissions.Exists(e => e.code == "BILLITEM"))
                        {
                            Permissions p = ousers.permissions.Find(e => e.code == "SALES");
                            ousers.permissions.Add(new Permissions()
                            {
                                code = "BILLITEM",
                                view = p.view,
                                create = p.create,
                                modify = p.delete,
                                delete = p.delete,
                                print = p.print
                            });
                        }
                        //if (System.Diagnostics.Debugger.IsAttached || ousers.username == "elayaraja@gmail.com")
                        //{
                        //    ousers.permissions.ForEach(e =>
                        //    {
                        //        e.view = true;
                        //        e.delete = true;
                        //        e.modify = true;
                        //        e.print = true;
                        //        e.create = true;
                        //    });
                        //    ousers.permissions.FindAll(x =>
                        //    LicenseManager.Instance(Context.OrganizationID).Permissions.DataPermissions.Exists(e => e.code.ToLower() == x.code.ToLower())).ForEach(e =>
                        //      {
                        //          e.view = false;
                        //          e.delete = false;
                        //          e.modify = false;
                        //          e.print = false;
                        //          e.create = false;
                        //      });
                        //}
                        //else
                        {
                            ousers.permissions.RemoveAll(e => e == null);
                            ousers.permissions.RemoveAll(e => e != null && !LicenseManager.Instance(Context.OrganizationID).Permissions.Exist(e.code));
                        }
                        lst.Add(ousers);
                    }
                }
                if (_users.ValidateExternalLogin && lst.Count == 1)
                {
                    _users.id = lst[0].actualuserid;
                    _users.ValidateExternalLogin = false;
                    lst = Search(_users);
                    _users.ValidateExternalLogin = true;
                    _users.id = 0;
                }

                conn.Close();

            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                if (_users.ValidateLogin) throw exp;
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _users);
            }

            if (lst.Count == 1 && (_users.ValidateSecurity || _users.ValidateLogin))
            {
                if (lst[0].security != null && (lst[0].security.globaluser || (
                    lst[0].permissions.Exists(e => e.code == "SECURITY_GLOBALUSER") &&
                    lst[0].permissions.Find(e => e.code == "SECURITY_GLOBALUSER").view)))
                {
                    if(Services.ConfigManager.Instance(Context.OrganizationID).Config.system_locationrestriction)
                        lst[0].systemrole = "ALL";
                    return lst;
                }
                bool locationsuccess = false;
                bool locationvalidation = false;
                if (Services.ConfigManager.Instance(Context.OrganizationID).Config.system_iprestriction ||
                    (lst[0].security != null && lst[0].security.iprestriction))
                {
                    locationvalidation = true;
                    if (Services.CacheManager.Instance(Context.OrganizationID).StaticIPList != null
                        && Services.CacheManager.Instance(Context.OrganizationID).StaticIPList.Contains(_users.ipaddress))
                    {
                        locationsuccess = true;
                    }
                }

                if (lst[0].security != null && !string.IsNullOrEmpty(lst[0].security.from) &&
                    !string.IsNullOrEmpty(lst[0].security.to))
                {
                    System.Globalization.CultureInfo enUS = new System.Globalization.CultureInfo("en-US");
                    var fromtime = TimeSpan.ParseExact(lst[0].security.from, "hh:MMtt", enUS);
                    var totime = TimeSpan.ParseExact(lst[0].security.to, "hh:MMtt", enUS);
                    if (DateTimeNow.TimeOfDay > totime || DateTimeNow.TimeOfDay < fromtime)
                    {
                        throw new Exception("You can login between " + lst[0].security.from + " " + lst[0].security.to);
                    }
                    else
                    {
                        _users.tr = string.Format("{0},{1}", fromtime.TotalMinutes, totime.TotalMinutes);
                    }
                }

                if ((Services.ConfigManager.Instance(Context.OrganizationID).Config.system_locationrestriction ||
                    (lst[0].security != null && lst[0].security.locationrestriction)) && !locationsuccess)
                {
                    locationvalidation = true;
                    if (_users.locationinfo != null && !string.IsNullOrEmpty(_users.locationinfo.sourcekey))
                    {
                        SecuritykeysDataService securitykeysDataService = new SecuritykeysDataService() { Context = this.Context };
                        if (securitykeysDataService.ValidateLocation(_users, lst[0]))
                        {
                            locationsuccess = true;
                        }
                    }
                }
                if (locationvalidation && !locationsuccess)
                {
                    throw new Exception("Location information not found, can't login from out of office.");
                }
            }
            return lst;
        }

        public List<UserBasic> SearchAll(UsersCreteria _users)
        {
            if (LicenseManager.Instance(Context.OrganizationID).GetLicense().valiate_upto)
            {
                if (LicenseManager.Instance(Context.OrganizationID).GetLicense().validupto < DateTimeNow)
                {
                    throw new Exception("Your license expired, please contact finance team, Store Soft Solution Pvt Ltd.");
                }
            }

            List<UserBasic> lst = new List<UserBasic>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                NpgsqlCommand command = null;
                if (_users.id != 0)
                {
                    command = GetCommand(SQL_Search, conn);
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("users.isactive", true, "=");
                    if(_users.ids != null && _users.ids.Count>0)
                        qb.AddInParamer("users.id",String.Join(",", _users.ids));

                    command = qb.GetCommand(conn);
                }
                command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _users.id;
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        UserBasic ousers = new UserBasic();
                        ousers.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        ousers.username = reader["username"] == DBNull.Value ? "" : reader["username"].ToString();
                        ousers.title = reader["title"] == DBNull.Value ? "" : reader["title"].ToString();
                        if(!string.IsNullOrEmpty(ousers.title))
                            lst.Add(ousers);
                    }
                }
            
                conn.Close();

            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                if (_users.ValidateLogin) throw exp;
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _users);
            }
            return lst;
        }
        internal Users ChangePassword(UsersCreteria item)
        {
            if (!CanRunONSAAS())
            {
                throw new Exception("SAAS Application can't run without organization information");
            }
            var users = Search(new UsersCreteria()
            {
                ValidateLogin = true,
                username = item.username,
                password = item.password
            });

            if (users.Count == 1)
            {
                users[0].changepassword = true;
                users[0].password = item.newpassword;
                return Update(users[0]);
            }
            return null;
        }


        internal string CreatePasscode(long userid, string lastcode)
        {
            if (!CanRunONSAAS())
            {
                throw new Exception("SAAS Application can't run without organization information");
            }
            var newcode = Token.RandomString(6, "0123456789");
            while (lastcode == newcode)
            {
                newcode = Token.RandomString(6, "0123456789");
            }
            var param = new SortedList<string, object>();
            param.Add("passcode", Cryptor.EncryptString(newcode, Cryptor.DefaultToken));
            param.Add("userid", userid);
            param.Add("passcodeon", DateTimeNow);
            ExecuteQuery("Update users set passcode=@passcode,passcodeon=@passcodeon where id=@userid", param);
            return newcode;
        }

        public Users Insert(Users oUsers)
        {
            if (!CanRunONSAAS())
            {
                throw new Exception("SAAS Application can't run without organization information");
            }
            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);

                    Insert(transaction, conn, oUsers);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oUsers);
            }

            return oUsers;
        }

        public Users Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Users oUsers)
        {

            NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oUsers.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oUsers.version;

            command.Parameters.Add("organizationcode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = "";
            command.Parameters.Add("organizationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oUsers.organizationid;

            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("username", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oUsers.username == null ? "" : oUsers.username;
            command.Parameters.Add("permissions", NpgsqlTypes.NpgsqlDbType.Json).Value = oUsers.permissions_json;
            command.Parameters.Add("password", NpgsqlTypes.NpgsqlDbType.Varchar).Value = Cryptor.EncryptString(oUsers.password == null ? "" : oUsers.password, Cryptor.DefaultToken);
            command.Parameters.Add("role", NpgsqlTypes.NpgsqlDbType.Integer).Value = oUsers.role;
            command.Parameters.Add("landingscreen", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oUsers.landingscreen == null ? "" : oUsers.landingscreen;
            command.Parameters.Add("employeeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oUsers.employeeid;
            command.Parameters.Add("changepassword", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oUsers.changepassword;
            command.Parameters.Add("lastloginon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oUsers.lastloginon;
            command.Parameters.Add("externalprovider", NpgsqlTypes.NpgsqlDbType.Varchar).Value = "";
            command.Parameters.Add("accesskey", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oUsers.accesskey == null ? "" : oUsers.accesskey;
            command.Parameters.Add("actualuserid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oUsers.actualuserid;
            command.Parameters.Add("alloweddiscount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oUsers.alloweddiscount;
            command.Parameters.Add("allowedcredit", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oUsers.allowedcredit;
            command.Parameters.Add("accessiblelocation", NpgsqlTypes.NpgsqlDbType.Json).Value = oUsers.accessiblelocation_json;
            command.Parameters.Add("accessiblecompany", NpgsqlTypes.NpgsqlDbType.Json).Value = oUsers.accessiblecompany_json;
            command.Parameters.Add("accessablesections", NpgsqlTypes.NpgsqlDbType.Json).Value = oUsers.accessablesections_json;
            command.Parameters.Add("security", NpgsqlTypes.NpgsqlDbType.Json).Value = oUsers.security_json;


            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oUsers.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            return oUsers;
        }

        public Users Update(Users oUsers)
        {
            if (!CanRunONSAAS())
            {
                throw new Exception("SAAS Application can't run without organization information");
            }
            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oUsers);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oUsers);
            }

            return oUsers;
        }

        public Users Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Users oUsers)
        {
            NpgsqlCommand command = GetCommand(SQL_Update, conn, transaction);
            if (oUsers.changepassword)
            {
                command.CommandText = "update users set password=@password where id=@id;" + SQL_Update;
                command.Parameters.Add("password", NpgsqlTypes.NpgsqlDbType.Varchar).Value = Cryptor.EncryptString(oUsers.password == null ? "" : oUsers.password, Cryptor.DefaultToken);
            }
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oUsers.id;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oUsers.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oUsers.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oUsers.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oUsers.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("username", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oUsers.username == null ? "" : oUsers.username;
            command.Parameters.Add("permissions", NpgsqlTypes.NpgsqlDbType.Json).Value = oUsers.permissions_json;
            //command.Parameters.Add("password", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oUsers.password == null ? "" : oUsers.password;
            command.Parameters.Add("role", NpgsqlTypes.NpgsqlDbType.Integer).Value = oUsers.role;
            command.Parameters.Add("landingscreen", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oUsers.landingscreen == null ? "" : oUsers.landingscreen;
            command.Parameters.Add("employeeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oUsers.employeeid;
            command.Parameters.Add("alloweddiscount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oUsers.alloweddiscount;
            command.Parameters.Add("allowedcredit", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oUsers.allowedcredit;
            //command.Parameters.Add("changepassword", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oUsers.changepassword;
            command.Parameters.Add("accessiblelocation", NpgsqlTypes.NpgsqlDbType.Json).Value = oUsers.accessiblelocation_json;
            command.Parameters.Add("accessiblecompany", NpgsqlTypes.NpgsqlDbType.Json).Value = oUsers.accessiblecompany_json;
            command.Parameters.Add("accesskey", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oUsers.accesskey == null ? "" : oUsers.accesskey;
            command.Parameters.Add("accessablesections", NpgsqlTypes.NpgsqlDbType.Json).Value = oUsers.accessablesections_json;
            command.Parameters.Add("security", NpgsqlTypes.NpgsqlDbType.Json).Value = oUsers.security_json;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oUsers.version = Convert.ToInt32(reader[0]);
                }
            }


            return oUsers;
        }


        public bool Delete(Users oUsers)
        {
            if (!CanRunONSAAS())
            {
                throw new Exception("SAAS Application can't run without organization information");
            }
            NpgsqlTransaction transaction = null;
            int successCount = 0;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    Delete(transaction, conn, oUsers);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oUsers);


            }
            return successCount > 0;

        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Users oUsers)
        {

            NpgsqlCommand command = GetCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oUsers.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oUsers.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oUsers.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

        internal UserCustomer GetMyCustomer(string userid)
        {
            string query = @"select id,accessiblecustomer,username from users where id=@id";
            long uid = 0;
            UserCustomer userCustomer = new UserCustomer();
            if (long.TryParse(userid,out uid))
            {
                var param = new SortedList<string, object>();
                param.Add("id", uid);
                using (var reader = ExecuteReader(query, param))
                {
                    if(reader.Read())
                    {
                        userCustomer.id = reader[0] == DBNull.Value ? 0 : Convert.ToInt64(reader[0]);
                        string customerlist = reader[1] == DBNull.Value ? "" : reader[1].ToString();
                        userCustomer.username = reader[2] == DBNull.Value ? "" : Convert.ToString(reader[2]);
                        userCustomer.accessiblecustomer = new List<long>();
                        if (!string.IsNullOrEmpty(customerlist))
                        {
                            userCustomer.accessiblecustomer = Newtonsoft.Json.JsonConvert.DeserializeObject<List<long>>(customerlist).Distinct().ToList();
                        }
                    };
                }
            }
            return userCustomer;
        }

        internal bool SetGlobalPermission(UsersCreteria item)
        {
            if (item == null || item.ids == null || item.ids.Count == 0) throw new Exception("Please pass the user information");
            try
            {
                var users = Search(new UsersCreteria()
                {
                    ids = item.ids
                });
                if (item.ids.Count != users.Count)
                {
                    throw new Exception("Please pass the user information");
                }
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();

                    
                    NpgsqlTransaction transaction = conn.BeginTransaction();
                    foreach (var itm in users)
                    {
                        if (itm.security == null) itm.security = new Users.UserSecurity();
                        itm.security.globaluser = item.globalpermission;
                        if (itm.permissions.Exists(x => x.code == "SECURITY_GLOBALUSER"))
                        {
                            itm.permissions.Find(x => x.code == "SECURITY_GLOBALUSER").view = item.globalpermission;
                        }
                        else
                        {
                            itm.permissions.Add(new Permissions()
                            {
                                code = "SECURITY_GLOBALUSER",
                                view = item.globalpermission
                            });
                        }
                        NpgsqlCommand command = GetCommand(@"update users set permissions=@permissions,security=@security where id=@id", conn,transaction);
                        command.Parameters.Add("permissions", NpgsqlTypes.NpgsqlDbType.Json).Value = Newtonsoft.Json.JsonConvert.SerializeObject(itm.permissions);
                        command.Parameters.Add("security", NpgsqlTypes.NpgsqlDbType.Json).Value = Newtonsoft.Json.JsonConvert.SerializeObject(itm.security);
                        command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = itm.id;
                        command.ExecuteNonQuery();
                    }
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, item);
                return false;
            }
            return true;
        }

        internal bool SetMyCustomer(UserCustomer item)
        {
            

            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    NpgsqlCommand command = GetCommand(@"update users set accessiblecustomer=@accessiblecustomer where id=@id", conn);
                    command.Parameters.Add("accessiblecustomer", NpgsqlTypes.NpgsqlDbType.Json).Value = Newtonsoft.Json.JsonConvert.SerializeObject(item.accessiblecustomer);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.id;
                    command.ExecuteNonQuery();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, item);
                return false;
            }
            return true;
        }

        internal void SetMyCustomer(Customer oCustomer, NpgsqlConnection conn, NpgsqlTransaction transaction)
        {
            if (Context.UserID == 0) return;
            var users = GetMyCustomer(Context.UserID.ToString());
            if (users.accessiblecustomer == null) users.accessiblecustomer = new List<long>();
            if (!users.accessiblecustomer.Exists(e => e == oCustomer.id))
            {
                users.accessiblecustomer.Add(oCustomer.id);
                NpgsqlCommand command = GetCommand(@"update users set accessiblecustomer=@accessiblecustomer where id=@id", conn, transaction);
                command.Parameters.Add("accessiblecustomer", NpgsqlTypes.NpgsqlDbType.Json).Value = Newtonsoft.Json.JsonConvert.SerializeObject(users.accessiblecustomer);
                command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                command.ExecuteNonQuery();
            }
        }
    }
}
