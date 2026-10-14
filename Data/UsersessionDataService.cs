using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
using WebAPI.Data;

namespace InfyPOS.Data

{
    public class UsersessionDataService : BaseData
    {
        const string SQL_Search = @"select usersession.id, usersession.isactive, usersession.createdby, usersession.modifiedby, 
usersession.version, usersession.createdon, usersession.modifiedon, token, refreshtoken, users.username sessionkilledby,
tokenexpired, refreshtokenexpired, ""killedBy"", ""loginOn"", ""logoutOn"", refreshtokenon, platform,deviceid,
refereshtokenhash, protectedticket, userid, usersession.employeeid, usersession.username, clientid from usersession
left outer join users on users.id =  ""killedBy"" where usersession.id=@id";

        const string SQL_Insert = @"INSERT INTO public.usersession(isactive, createdby, modifiedby, version, createdon, modifiedon, token, refreshtoken, tokenexpired, refreshtokenexpired, ""killedBy"", ""loginOn"", ""logoutOn"", refreshtokenon, refereshtokenhash, protectedticket, userid, employeeid, username, clientid,platform,deviceid,utcoffset) 
        VALUES(@isactive, @createdby, @modifiedby, @version, @createdon, @modifiedon, @token, @refreshtoken, @tokenexpired, @refreshtokenexpired, @killedBy, @loginOn, @logoutOn, @refreshtokenon, @refereshtokenhash, @protectedticket, @userid, @employeeid, @username, @clientid,@platform,@deviceid,@utcoffset) RETURNING id";

        const string SQL_Update = @"UPDATE public.usersession SET modifiedby=@modifiedby, isactive=@isactive, version=@version+1, modifiedon=@modifiedon, token=@token, refreshtoken=@refreshtoken, tokenexpired=@tokenexpired, refreshtokenexpired=@refreshtokenexpired, refreshtokenon=@refreshtokenon, 
        refereshtokenhash=@refereshtokenhash, protectedticket=@protectedticket where id=@id and isactive=true RETURNING version";

        const string SQL_Delete = @"update public.usersession set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version  RETURNING version";

        internal bool SetMobileToken(UsersToken item)
        {
            item.accessToken = Cryptor.EncryptString(item.accessToken, Cryptor.DefaultToken);
            UsersessionDataService usersDataService = new UsersessionDataService() { Context = this.Context };
            var sessions = usersDataService.Search(new UsersessionCreteria()
            {
                refreshtoken = item.refreshToken
            });
            if(sessions.Count == 1)
            {
                var param = new SortedList<string, object>();
                param.Add("id", sessions[0].id);
                param.Add("accesstoken", item.accessToken);
                return ExecuteQuery("update usersession set accesstoken=@accesstoken where id=@id", param) == 1;
            }
            return false;
        }

        public List<Usersession> Search(UsersessionCreteria _usersession)
        {
            List<Usersession> lst = new List<Usersession>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_usersession.id != 0)
                {
                    command = GetCommand(SQL_Search, conn);
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    
                    if(!string.IsNullOrEmpty(_usersession.refereshtokenhash))
                        qb.AddParameter("refereshtokenhash", _usersession.refereshtokenhash, "=");
                    else if (!string.IsNullOrEmpty(_usersession.refreshtoken))
                        qb.AddParameter("refreshtoken", _usersession.refreshtoken, "=");
                    else if (_usersession.userid>0)
                        qb.AddParameter("userid", _usersession.userid, "=");
                    else
                        qb.AddParameter("usersession.isactive", true, "=");
                    command = qb.GetCommand(conn);
                }
                command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _usersession.id;
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Usersession ousersession = new Usersession();
                        ousersession.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        ousersession.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        ousersession.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        ousersession.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        ousersession.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        ousersession.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        ousersession.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"].ToString());
                        ousersession.token = reader["token"] == DBNull.Value ? "" : reader["token"].ToString();
                        ousersession.refreshtoken = reader["refreshtoken"] == DBNull.Value ? "" : reader["refreshtoken"].ToString();
                        ousersession.tokenexpired = reader["tokenexpired"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["tokenexpired"].ToString());
                        ousersession.tokenexpired = DateTime.SpecifyKind(ousersession.tokenexpired, DateTimeKind.Utc);
                        ousersession.refreshtokenexpired = reader["refreshtokenexpired"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["refreshtokenexpired"].ToString());
                        ousersession.killedBy = reader["killedBy"] == DBNull.Value ? 0 : Convert.ToInt64(reader["killedBy"].ToString());
                        ousersession.loginOn = reader["loginOn"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["loginOn"].ToString());
                        ousersession.logoutOn = reader["logoutOn"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["logoutOn"].ToString());
                        ousersession.refreshtokenon = reader["refreshtokenon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["refreshtokenon"].ToString());
                        ousersession.refereshtokenhash = reader["refereshtokenhash"] == DBNull.Value ? "" : reader["refereshtokenhash"].ToString();
                        ousersession.protectedticket = reader["protectedticket"] == DBNull.Value ? "" : reader["protectedticket"].ToString();
                        ousersession.userid = reader["userid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["userid"].ToString());
                        ousersession.employeeid = reader["employeeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["employeeid"].ToString());
                        ousersession.username = reader["username"] == DBNull.Value ? "" : reader["username"].ToString();
                        ousersession.platform = reader["platform"] == DBNull.Value ? "" : reader["platform"].ToString();
                        ousersession.deviceid = reader["deviceid"] == DBNull.Value ? "" : reader["deviceid"].ToString();
                        ousersession.clientid = reader["clientid"] == DBNull.Value ? "" : reader["clientid"].ToString();
                        ousersession.sessionkilledby = reader["sessionkilledby"] == DBNull.Value ? "" : reader["sessionkilledby"].ToString(); 
                        lst.Add(ousersession);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _usersession);
            }
            return lst;
        }
       
        internal bool KillUser(Usersession item)
        {

            try
            {
                if (string.IsNullOrEmpty(item.platform))
                {
                    if (Context.UserID > 0)
                    {
                        using (NpgsqlConnection conn = CreateConnection())
                        {
                            conn.Open();
                            NpgsqlCommand command = GetCommand(@"update usersession set reason=@reason,""killedBy""=@killedBy,""logoutOn""=@logoutOn,isactive=false where userid=@userid and isactive=true", conn);
                            command.Parameters.Add("killedBy", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                            command.Parameters.Add("userid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.userid;
                            command.Parameters.Add("logoutOn", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
                            command.Parameters.Add("reason", NpgsqlTypes.NpgsqlDbType.Varchar).Value = "KA-"+item.reason;
                            var success = command.ExecuteNonQuery() > 0;
                            conn.Close();
                            return success;
                        }
                    }
                }
                else
                {
                    using (NpgsqlConnection conn = CreateConnection())
                    {
                        conn.Open();
                        NpgsqlCommand command = GetCommand(@"update usersession set reason=@reason,""killedBy""=@killedBy,""logoutOn""=@logoutOn,isactive=false where userid=@userid and isactive=true and platform=@platform", conn);
                        command.Parameters.Add("killedBy", NpgsqlTypes.NpgsqlDbType.Bigint).Value = -2;
                        command.Parameters.Add("userid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.userid;
                        command.Parameters.Add("logoutOn", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
                        command.Parameters.Add("platform", NpgsqlTypes.NpgsqlDbType.Varchar).Value = item.platform;
                        command.Parameters.Add("reason", NpgsqlTypes.NpgsqlDbType.Varchar).Value = "KI-" + item.reason;
                        var success = command.ExecuteNonQuery() > 0;
                        conn.Close();
                        return success;
                    }
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, item);
            }
            return false;
        }


        internal bool Logout(Usersession item)
        {
            try
            {
                if (Context.UserID > 0)
                {
                    using (NpgsqlConnection conn = CreateConnection())
                    {
                        conn.Open();
                        if(Context.SessionID>0)
                        {
                            NpgsqlCommand command = GetCommand(@"update usersession set reason=@reason,""killedBy""=-1,logoutOn=@logoutOn,isactive=false where userid=@userid and id=@id", conn);
                            command.Parameters.Add("userid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                            command.Parameters.Add("logoutOn", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
                            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.SessionID;
                            command.Parameters.Add("reason", NpgsqlTypes.NpgsqlDbType.Varchar).Value ="LOGOUT-SESSION";
                            return command.ExecuteNonQuery() > 0;
                        }
                        else if (item != null && !string.IsNullOrEmpty(item.refreshtoken))
                        {
                            NpgsqlCommand command = GetCommand(@"update usersession set reason=@reason,""killedBy""=-1,logoutOn=@logoutOn,isactive=false where userid=@userid and refreshtoken=@refreshtoken", conn);
                            command.Parameters.Add("userid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                            command.Parameters.Add("logoutOn", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
                            command.Parameters.Add("refreshtoken", NpgsqlTypes.NpgsqlDbType.Varchar).Value = item.refreshtoken;
                            command.Parameters.Add("reason", NpgsqlTypes.NpgsqlDbType.Varchar).Value = "LOGOUT-TOKEN";
                            return command.ExecuteNonQuery() > 0;
                        }
                        else
                        {
                            NpgsqlCommand command = GetCommand(@"update usersession set reason=@reason,""killedBy""=-1,logoutOn=@logoutOn where userid=@userid", conn);
                            command.Parameters.Add("userid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                            command.Parameters.Add("logoutOn", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
                            command.Parameters.Add("reason", NpgsqlTypes.NpgsqlDbType.Varchar).Value = "LOGOUT-USERID";
                            return command.ExecuteNonQuery() > 0;
                        }
                    }
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, item);
            }

            return false;
        }

        public Usersession Insert(Usersession oUsersession)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);
                    Insert(transaction, conn, oUsersession);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oUsersession);
                
                    
            }

            return oUsersession;
        }

        public Usersession Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Usersession oUsersession)
        {

            NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oUsersession.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oUsersession.version;
            command.Parameters.Add("utcoffset", NpgsqlTypes.NpgsqlDbType.Integer).Value = oUsersession.utcoffset;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("token", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oUsersession.token == null ? "" : oUsersession.token;
            command.Parameters.Add("refreshtoken", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oUsersession.refreshtoken == null ? "" : oUsersession.refreshtoken;
            command.Parameters.Add("tokenexpired", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oUsersession.tokenexpired;
            command.Parameters.Add("refreshtokenexpired", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oUsersession.refreshtokenexpired;
            command.Parameters.Add("killedBy", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oUsersession.killedBy;
            command.Parameters.Add("loginOn", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oUsersession.loginOn;
            command.Parameters.Add("logoutOn", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oUsersession.logoutOn;
            command.Parameters.Add("refreshtokenon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oUsersession.refreshtokenon;
            command.Parameters.Add("refereshtokenhash", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oUsersession.refereshtokenhash == null ? "" : oUsersession.refereshtokenhash;
            command.Parameters.Add("protectedticket", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oUsersession.protectedticket == null ? "" : oUsersession.protectedticket;
            command.Parameters.Add("userid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oUsersession.userid;
            command.Parameters.Add("employeeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oUsersession.employeeid;
            command.Parameters.Add("username", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oUsersession.username == null ? "" : oUsersession.username;
            command.Parameters.Add("clientid", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oUsersession.clientid == null ? "" : oUsersession.clientid;
            command.Parameters.Add("platform", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oUsersession.platform == null ? "" : oUsersession.platform;
            command.Parameters.Add("deviceid", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oUsersession.deviceid == null ? "" : oUsersession.deviceid;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oUsersession.id = Convert.ToInt64(reader[0]);
                }
            }

            //if(!string.IsNullOrEmpty(System.Configuration.ConfigurationManager.AppSettings["ConcurrentSession"]) &&
            //   System.Configuration.ConfigurationManager.AppSettings["ConcurrentSession"].ToLower() == "false")
            //{
            //    var existingusers = Search(new UsersessionCreteria()
            //    {
            //        userid = oUsersession.userid,
            //        isactive = true
            //    });
            //    existingusers.RemoveAll(e => e.id == oUsersession.id);
            //    foreach (var usersession in existingusers)
            //    {
            //        usersession.isactive = false;
            //        Update(transaction, conn, usersession);
            //    }
            //}
            return oUsersession;
        }

        internal int ValidateUserCount(string platform)
        {
            if (platform == "WEB")
            {
                if (SAASUserCount > 0)
                {
                    var activeusers = 0;
                    using (var reader = ExecuteReader("select count(*) from usersession where isactive = true and platform = 'WEB'", null))
                    {
                        if (reader.Read())
                            activeusers = reader.GetInt32(0);
                    }
                    if (SAASUserCount < activeusers + 1)
                    {
                        var query = @"with activeusers 
                                as
                                (
                                select id,userid,row_number() OVER (ORDER BY id asc) from usersession where isactive =true and platform = 'WEB'
                                )
delete from usersession where id in (select id from activeusers
where row_number>=@usercount
)";
                        var param = new SortedList<string, object>();
                        param.Add("usercount", SAASUserCount);
                        return ExecuteQuery(query, param);
                    }
                }
            }
            else if(platform =="IOS")
            {
                if (IOSUserCount > 0)
                {
                    var activeusers = 0;
                    using (var reader = ExecuteReader("select count(*) from usersession where isactive = true and platform = 'IOS'", null))
                    {
                        if (reader.Read())
                            activeusers = reader.GetInt32(0);
                    }
                    if (IOSUserCount < activeusers + 1)
                    {
                        var query = @"with activeusers 
                                as
                                (
                                select id,userid,row_number() OVER (ORDER BY id asc) from usersession where isactive =true and platform = 'IOS'
                                )
delete from usersession where id in (select id from activeusers
where row_number>=@usercount
)";
                        var param = new SortedList<string, object>();
                        param.Add("usercount", IOSUserCount);
                        return ExecuteQuery(query, param);
                    }
                }
            }
            else if (platform == "ANDROID")
            {
                if (AndroidUserCount > 0)
                {
                    var activeusers = 0;
                    using (var reader = ExecuteReader("select count(*) from usersession where isactive = true and platform = 'ANDROID'", null))
                    {
                        if (reader.Read())
                            activeusers = reader.GetInt32(0);
                    }
                    if (AndroidUserCount < activeusers + 1)
                    {
                        var query = @"with activeusers 
                                as
                                (
                                select id,userid,row_number() OVER (ORDER BY id asc) from usersession where isactive =true and platform = 'ANDROID'
                                )
delete from usersession where id in (select id from activeusers
where row_number>=@usercount
)";
                        var param = new SortedList<string, object>();
                        param.Add("usercount", AndroidUserCount);
                        return ExecuteQuery(query, param);
                    }
                }
            }
            return 0;
        }

        public Usersession Update(Usersession oUsersession)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oUsersession);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oUsersession);
                
                    
            }

            return oUsersession;
        }

        public Usersession Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Usersession oUsersession)
        {

            NpgsqlCommand command = GetCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oUsersession.id;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oUsersession.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oUsersession.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oUsersession.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oUsersession.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("token", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oUsersession.token == null ? "" : oUsersession.token;
            command.Parameters.Add("refreshtoken", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oUsersession.refreshtoken == null ? "" : oUsersession.refreshtoken;
            command.Parameters.Add("tokenexpired", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oUsersession.tokenexpired;
            command.Parameters.Add("refreshtokenexpired", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oUsersession.refreshtokenexpired;
            command.Parameters.Add("refreshtokenon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oUsersession.refreshtokenon;
            command.Parameters.Add("refereshtokenhash", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oUsersession.refereshtokenhash == null ? "" : oUsersession.refereshtokenhash;
            command.Parameters.Add("protectedticket", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oUsersession.protectedticket == null ? "" : oUsersession.protectedticket;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oUsersession.version = Convert.ToInt32(reader[0]);
                }
            }

            return oUsersession;
        }


        public bool Delete(Usersession oUsersession)
        {

            NpgsqlTransaction transaction = null;
            int successCount = 0;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    Delete(transaction, conn, oUsersession);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oUsersession);

                
                    
            }
            return successCount > 0;

        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Usersession oUsersession)
        {

            NpgsqlCommand command = GetCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oUsersession.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oUsersession.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oUsersession.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

    }

}
