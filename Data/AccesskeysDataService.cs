using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;

namespace InfyPOS.Data
{
    public class AccesskeysDataService : BaseData
    {
        const string SQL_Search = @"select sourceid, createdon, modifiedon, createdby, modifiedby, version, status, accesslevel, id, sourcetype, statusmessage, accesskey from accesskeys where id=@id";
        const string SQL_Insert = @"INSERT INTO public.accesskeys(sourceid, createdon, modifiedon, createdby, modifiedby, version, status, accesslevel, sourcetype, statusmessage, accesskey) VALUES(@sourceid, @createdon, @modifiedon, @createdby, @modifiedby, @version, @status, @accesslevel, @sourcetype, @statusmessage, @accesskey) RETURNING id";
        const string SQL_Update = @"UPDATE public.accesskeys SET sourceid=@sourceid, modifiedon=@modifiedon, modifiedby=@modifiedby, version=@version+1, status=@status, accesslevel=@accesslevel, sourcetype=@sourcetype, statusmessage=@statusmessage, accesskey=@accesskey where id=@id and version=@version RETURNING version";
        const string SQL_Delete = @"update public.accesskeys set status=-1,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";
        public List<Accesskeys> Search(AccesskeysCreteria _accesskeys)
        {
            List<Accesskeys> lst = new List<Accesskeys>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_accesskeys.id != 0)
                {
                    command = new NpgsqlCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _accesskeys.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    if (!string.IsNullOrEmpty(_accesskeys.accesskey))
                    {
                        qb.AddParameter("accesskey", _accesskeys.accesskey, "=");
                    }
                    else
                    {
                        qb.AddParameter("sourcetype", _accesskeys.sourcetype, "=");
                        qb.AddParameter("sourceid", _accesskeys.sourceid, "=");
                    }
                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Accesskeys oaccesskeys = new Accesskeys();
                        oaccesskeys.sourceid = reader["sourceid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sourceid"]);
                        oaccesskeys.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        oaccesskeys.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        oaccesskeys.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        oaccesskeys.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        oaccesskeys.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        oaccesskeys.status = reader["status"] == DBNull.Value ? 0 : Convert.ToInt32(reader["status"]);
                        oaccesskeys.accesslevel = reader["accesslevel"] == DBNull.Value ? 0 : Convert.ToInt32(reader["accesslevel"]);
                        oaccesskeys.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        oaccesskeys.sourcetype = reader["sourcetype"] == DBNull.Value ? "" : reader["sourcetype"].ToString();
                        oaccesskeys.statusmessage = reader["statusmessage"] == DBNull.Value ? "" : reader["statusmessage"].ToString();
                        oaccesskeys.accesskey = reader["accesskey"] == DBNull.Value ? "" : reader["accesskey"].ToString();
                        //if(oaccesskeys.status <0)
                        //{
                        //    throw new Exception("Access disabled - "+oaccesskeys.statusmessage);
                        //}
                        lst.Add(oaccesskeys);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _accesskeys);
            }
            return lst;
        }

        public Accesskeys Insert(Accesskeys oAccesskeys)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    Insert(transaction, conn, oAccesskeys);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oAccesskeys);
            }

            return oAccesskeys;
        }

        public Accesskeys Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Accesskeys oAccesskeys)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("sourceid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oAccesskeys.sourceid;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oAccesskeys.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oAccesskeys.modifiedon;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oAccesskeys.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oAccesskeys.modifiedby;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oAccesskeys.version;
            command.Parameters.Add("status", NpgsqlTypes.NpgsqlDbType.Integer).Value = oAccesskeys.status;
            command.Parameters.Add("accesslevel", NpgsqlTypes.NpgsqlDbType.Integer).Value = oAccesskeys.accesslevel;
            command.Parameters.Add("sourcetype", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oAccesskeys.sourcetype == null ? "" : oAccesskeys.sourcetype;
            command.Parameters.Add("statusmessage", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oAccesskeys.statusmessage == null ? "" : oAccesskeys.statusmessage;
            command.Parameters.Add("accesskey", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oAccesskeys.accesskey == null ? "" : oAccesskeys.accesskey;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oAccesskeys.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            return oAccesskeys;
        }

        public Accesskeys Update(Accesskeys oAccesskeys)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oAccesskeys);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oAccesskeys);
            }

            return oAccesskeys;
        }

        public Accesskeys Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Accesskeys oAccesskeys)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("sourceid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oAccesskeys.sourceid;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oAccesskeys.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oAccesskeys.modifiedon;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oAccesskeys.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oAccesskeys.modifiedby;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oAccesskeys.version;
            command.Parameters.Add("status", NpgsqlTypes.NpgsqlDbType.Integer).Value = oAccesskeys.status;
            command.Parameters.Add("accesslevel", NpgsqlTypes.NpgsqlDbType.Integer).Value = oAccesskeys.accesslevel;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oAccesskeys.id;
            command.Parameters.Add("sourcetype", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oAccesskeys.sourcetype == null ? "" : oAccesskeys.sourcetype;
            command.Parameters.Add("statusmessage", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oAccesskeys.statusmessage == null ? "" : oAccesskeys.statusmessage;
            command.Parameters.Add("accesskey", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oAccesskeys.accesskey == null ? "" : oAccesskeys.accesskey;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oAccesskeys.version = Convert.ToInt32(reader[0].ToString());
                }
            }

            return oAccesskeys;
        }


        public bool Delete(Accesskeys oAccesskeys)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oAccesskeys);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oAccesskeys);
            }
            return hasSuccessCount;

        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Accesskeys oAccesskeys)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("status", NpgsqlTypes.NpgsqlDbType.Integer).Value = oAccesskeys.status;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oAccesskeys.modifiedby;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.TimestampTZ).Value = oAccesskeys.modifiedon;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oAccesskeys.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oAccesskeys.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

        internal Accesskeys Create(Accesskeys accesskeysCreteria)
        {
            accesskeysCreteria.accesskey = WebAPI.Data.Token.RandomString(24);
            accesskeysCreteria.version = 1;
            accesskeysCreteria.createdby = Context.UserID;
            accesskeysCreteria.modifiedby = Context.UserID;
            accesskeysCreteria.createdon = DateTimeNow;
            accesskeysCreteria.modifiedon = DateTimeNow;
            accesskeysCreteria.status = 1;
            accesskeysCreteria.accesslevel = 1;
            Insert(accesskeysCreteria);
            return accesskeysCreteria;
        }

        internal Accesskeys RevokeAccessKey(Accesskeys item)
        {
            item.status = -1;
            item.modifiedby = Context.UserID;
            item.modifiedon = DateTimeNow;
            Update(item);
            return item;
        }

        public class Accesskeys : BaseEntity
        {
            public Accesskeys lastAccess { get; set; }
            public long sourceid { get; set; }
            public DateTime createdon { get; set; }
            public DateTime modifiedon { get; set; }
            public long createdby { get; set; }
            public long modifiedby { get; set; }
            public int version { get; set; }
            public int status { get; set; }
            public int accesslevel { get; set; }
            public long id { get; set; }
            public string sourcetype { get; set; }
            public string statusmessage { get; set; }
            public string accesskey { get; set; }
        }
        public class AccesskeysCreteria : Accesskeys
        {
        }


    }

}
