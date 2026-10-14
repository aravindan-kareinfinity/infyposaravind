using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
namespace InfyPOS.Data

{
    public class DatasecuritygroupDataService : BaseData
    {
        const string SQL_Search = @"select version, isactive, id, userid, createdon, modifiedon, createdby, modifiedby, config, name, username from datasecuritygroup where id=@id";


        const string SQL_Insert = @"INSERT INTO public.datasecuritygroup(version, isactive, userid, createdon, modifiedon, createdby, modifiedby, config, name, username) VALUES(@version, @isactive, @userid, @createdon, @modifiedon, @createdby, @modifiedby, @config, @name, @username) RETURNING id";


        const string SQL_Update = @"UPDATE public.datasecuritygroup SET version=@version+1, userid=@userid, modifiedon=@modifiedon, modifiedby=@modifiedby, config=@config, name=@name, username=@username where id=@id and version=@version RETURNING version";

        const string SQL_Delete = @"update public.datasecuritygroup set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";

        public List<Datasecuritygroup> Search(DatasecuritygroupCreteria _datasecuritygroup)
        {
            List<Datasecuritygroup> lst = new List<Datasecuritygroup>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_datasecuritygroup.id != 0)
                {
                    command = new NpgsqlCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _datasecuritygroup.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("isactive", true, "=");

                    //if (!string.IsNullOrEmpty(_datasecuritygroup.Code))
                    //    qb.AddParameter("code", _datasecuritygroup.Code, "=");

                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Datasecuritygroup odatasecuritygroup = new Datasecuritygroup();
                        odatasecuritygroup.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        odatasecuritygroup.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        odatasecuritygroup.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        odatasecuritygroup.userid = reader["userid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["userid"]);
                        odatasecuritygroup.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        odatasecuritygroup.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        odatasecuritygroup.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        odatasecuritygroup.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        odatasecuritygroup.config_json = reader["config"] == DBNull.Value ? null : reader["config"].ToString();
                        odatasecuritygroup.name = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
                        odatasecuritygroup.username = reader["username"] == DBNull.Value ? "" : reader["username"].ToString();
                        lst.Add(odatasecuritygroup);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _datasecuritygroup);
            }
            return lst;
        }

        public Datasecuritygroup Insert(Datasecuritygroup oDatasecuritygroup)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);

                    Insert(transaction, conn, oDatasecuritygroup);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oDatasecuritygroup);
            }

            return oDatasecuritygroup;
        }

        public Datasecuritygroup Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Datasecuritygroup oDatasecuritygroup)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oDatasecuritygroup.version;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oDatasecuritygroup.isactive;
            command.Parameters.Add("userid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oDatasecuritygroup.userid;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oDatasecuritygroup.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oDatasecuritygroup.modifiedon;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oDatasecuritygroup.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oDatasecuritygroup.modifiedby;
            command.Parameters.Add("config", NpgsqlTypes.NpgsqlDbType.Json).Value = oDatasecuritygroup.config_json == null ? "null" : oDatasecuritygroup.config_json;
            command.Parameters.Add("name", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oDatasecuritygroup.name == null ? "" : oDatasecuritygroup.name;
            command.Parameters.Add("username", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oDatasecuritygroup.username == null ? "" : oDatasecuritygroup.username;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oDatasecuritygroup.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            return oDatasecuritygroup;
        }

        public Datasecuritygroup Update(Datasecuritygroup oDatasecuritygroup)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oDatasecuritygroup);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oDatasecuritygroup);
            }

            return oDatasecuritygroup;
        }

        public Datasecuritygroup Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Datasecuritygroup oDatasecuritygroup)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oDatasecuritygroup.version;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oDatasecuritygroup.isactive;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oDatasecuritygroup.id;
            command.Parameters.Add("userid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oDatasecuritygroup.userid;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oDatasecuritygroup.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oDatasecuritygroup.modifiedon;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oDatasecuritygroup.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oDatasecuritygroup.modifiedby;
            command.Parameters.Add("config", NpgsqlTypes.NpgsqlDbType.Json).Value = oDatasecuritygroup.config_json == null ? "null" : oDatasecuritygroup.config_json;
            command.Parameters.Add("name", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oDatasecuritygroup.name == null ? "" : oDatasecuritygroup.name;
            command.Parameters.Add("username", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oDatasecuritygroup.username == null ? "" : oDatasecuritygroup.username;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oDatasecuritygroup.version = Convert.ToInt32(reader[0].ToString());
                }
            }

            return oDatasecuritygroup;
        }


        public bool Delete(Datasecuritygroup oDatasecuritygroup)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oDatasecuritygroup);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oDatasecuritygroup);

            }
            return hasSuccessCount;

        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Datasecuritygroup oDatasecuritygroup)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oDatasecuritygroup.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oDatasecuritygroup.modifiedby;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.TimestampTZ).Value = oDatasecuritygroup.modifiedon;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oDatasecuritygroup.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oDatasecuritygroup.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

    }

}
