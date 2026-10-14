using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
namespace InfyPOS.Data
{
    public class UniquesequanceDataService : SAASData
    {
        const string SQL_Search = @"select id, isactive, createdby, modifiedby, version, groupid, used, createdon, modifiedon, usedon, sequenceno, sequenceindex, usedreferenceid, usedreferencename from uniquesequance where id=@id";


        const string SQL_Insert = @"INSERT INTO public.uniquesequance(isactive, createdby, modifiedby, version, groupid, used, createdon, modifiedon, usedon, sequenceno, sequenceindex, usedreferenceid, usedreferencename) VALUES(@isactive, @createdby, @modifiedby, @version, @groupid, @used, @createdon, @modifiedon, @usedon, @sequenceno, @sequenceindex, @usedreferenceid, @usedreferencename) RETURNING id";


        const string SQL_Update = @"UPDATE public.uniquesequance SET modifiedby=@modifiedby, version=@version+1, groupid=@groupid, used=@used, modifiedon=@modifiedon, usedon=@usedon, sequenceno=@sequenceno, sequenceindex=@sequenceindex, usedreferenceid=@usedreferenceid, usedreferencename=@usedreferencename where id=@id and version=@version RETURNING version";

        const string SQL_Delete = @"update public.uniquesequance set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";

        public List<Uniquesequance> Search(UniquesequanceCreteria _uniquesequance)
        {
            List<Uniquesequance> lst = new List<Uniquesequance>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_uniquesequance.id != 0)
                {
                    command = new NpgsqlCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _uniquesequance.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("isactive", true, "=");

                    if(!string.IsNullOrEmpty(_uniquesequance.prefix))
                        qb.AddParameter("sequenceindex", "prefix", _uniquesequance.prefix+"%", " like ");
                    if (!string.IsNullOrEmpty(_uniquesequance.suffix))
                        qb.AddParameter("sequenceindex", "suffix", "%"+_uniquesequance.suffix, " like ");

                    qb.AddParameter("sequenceindex", "from", _uniquesequance.fromno, ">=");
                    qb.AddParameter("sequenceindex", "to", _uniquesequance.tono, "<=");
                    qb.SortField = "sequenceindex";
                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Uniquesequance ouniquesequance = new Uniquesequance();
                        ouniquesequance.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        ouniquesequance.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        ouniquesequance.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        ouniquesequance.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        ouniquesequance.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        ouniquesequance.groupid = reader["groupid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["groupid"]);
                        ouniquesequance.used = reader["used"] == DBNull.Value ? false : Convert.ToBoolean(reader["used"]);
                        ouniquesequance.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        ouniquesequance.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        ouniquesequance.usedon = reader["usedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["usedon"]);
                        ouniquesequance.sequenceno = reader["sequenceno"] == DBNull.Value ? "" : reader["sequenceno"].ToString();
                        ouniquesequance.sequenceindex = reader["sequenceindex"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sequenceindex"]);
                        ouniquesequance.usedreferenceid = reader["usedreferenceid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["usedreferenceid"]);
                        ouniquesequance.usedreferencename = reader["usedreferencename"] == DBNull.Value ? "" : reader["usedreferencename"].ToString();
                        lst.Add(ouniquesequance);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _uniquesequance);
            }
            return lst;
        }

        public Uniquesequance Insert(Uniquesequance oUniquesequance)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);

                    Insert(transaction, conn, oUniquesequance);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oUniquesequance);
                throw exp;
            }

            return oUniquesequance;
        }

        public Uniquesequance Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Uniquesequance oUniquesequance)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oUniquesequance.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oUniquesequance.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oUniquesequance.modifiedby;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oUniquesequance.version;
            command.Parameters.Add("groupid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oUniquesequance.groupid;
            command.Parameters.Add("used", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oUniquesequance.used;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oUniquesequance.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oUniquesequance.modifiedon;
            command.Parameters.Add("usedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oUniquesequance.usedon;
            command.Parameters.Add("sequenceno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oUniquesequance.sequenceno == null ? "" : oUniquesequance.sequenceno;
            command.Parameters.Add("sequenceindex", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oUniquesequance.sequenceindex;
            command.Parameters.Add("usedreferenceid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oUniquesequance.usedreferenceid;
            command.Parameters.Add("usedreferencename", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oUniquesequance.usedreferencename == null ? "" : oUniquesequance.usedreferencename;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oUniquesequance.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            return oUniquesequance;
        }

        public Uniquesequance Update(Uniquesequance oUniquesequance)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oUniquesequance);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oUniquesequance);
                throw exp;
            }

            return oUniquesequance;
        }

        public Uniquesequance UpdateUsed(NpgsqlTransaction transaction, NpgsqlConnection conn, Uniquesequance oUniquesequance)
        {
            NpgsqlCommand command = new NpgsqlCommand("UPDATE public.uniquesequance SET version=@version+1,used = @used , usedon=@usedon, usedreferenceid=@usedreferenceid, usedreferencename=@usedreferencename where used=false and id=@id and version=@version RETURNING version", conn, transaction);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oUniquesequance.id;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oUniquesequance.version;
            command.Parameters.Add("used", NpgsqlTypes.NpgsqlDbType.Boolean).Value = true;
            command.Parameters.Add("usedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("usedreferenceid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oUniquesequance.usedreferenceid;
            command.Parameters.Add("usedreferencename", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oUniquesequance.usedreferencename == null ? "" : oUniquesequance.usedreferencename;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oUniquesequance.version = Convert.ToInt32(reader[0].ToString());
                }
                else
                {
                    throw new Exception("Some sequence no used, try again");
                }
            }
            return oUniquesequance;
        }

        public Uniquesequance Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Uniquesequance oUniquesequance)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oUniquesequance.id;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oUniquesequance.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oUniquesequance.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oUniquesequance.modifiedby;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oUniquesequance.version;
            command.Parameters.Add("groupid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oUniquesequance.groupid;
            command.Parameters.Add("used", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oUniquesequance.used;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oUniquesequance.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oUniquesequance.modifiedon;
            command.Parameters.Add("usedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oUniquesequance.usedon;
            command.Parameters.Add("sequenceno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oUniquesequance.sequenceno == null ? "" : oUniquesequance.sequenceno;
            command.Parameters.Add("sequenceindex", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oUniquesequance.sequenceindex;
            command.Parameters.Add("usedreferenceid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oUniquesequance.usedreferenceid;
            command.Parameters.Add("usedreferencename", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oUniquesequance.usedreferencename == null ? "" : oUniquesequance.usedreferencename;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oUniquesequance.version = Convert.ToInt32(reader[0].ToString());
                }
            }

            return oUniquesequance;
        }


        public bool Delete(Uniquesequance oUniquesequance)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oUniquesequance);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oUniquesequance);

                throw exp;
            }
            return hasSuccessCount;

        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Uniquesequance oUniquesequance)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oUniquesequance.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oUniquesequance.modifiedby;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oUniquesequance.modifiedon;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oUniquesequance.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oUniquesequance.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }
        public List<Uniquesequance> InsertAll(List<Uniquesequance> items)
        {
            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);
                    foreach (var item in items.OrderBy(e => e.sequenceindex))
                        Insert(transaction, conn, item);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, items);
                throw exp;
            }
            return items;
        }

        public List<Uniquesequance> Get(int noofean, NpgsqlTransaction transaction, NpgsqlConnection conn)
        {
            List<Uniquesequance> lst = new List<Uniquesequance>();

            string query = "select id, isactive, createdby, modifiedby, version, groupid, used, createdon, modifiedon, " +
           "usedon, sequenceno, sequenceindex, usedreferenceid, usedreferencename from uniquesequance " +
           "where used=false order by createdon,id " +
           "limit " + noofean.ToString("F0");

            QueryBuilder qb = new QueryBuilder();
            qb.CurrentQuery = query;
            NpgsqlCommand command = qb.GetCommand(conn, transaction);

            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    Uniquesequance ouniquesequance = new Uniquesequance();
                    ouniquesequance.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                    ouniquesequance.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                    ouniquesequance.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                    ouniquesequance.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                    ouniquesequance.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                    ouniquesequance.groupid = reader["groupid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["groupid"]);
                    ouniquesequance.used = reader["used"] == DBNull.Value ? false : Convert.ToBoolean(reader["used"]);
                    ouniquesequance.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                    ouniquesequance.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                    ouniquesequance.usedon = reader["usedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["usedon"]);
                    ouniquesequance.sequenceno = reader["sequenceno"] == DBNull.Value ? "" : reader["sequenceno"].ToString();
                    ouniquesequance.sequenceindex = reader["sequenceindex"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sequenceindex"]);
                    ouniquesequance.usedreferenceid = reader["usedreferenceid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["usedreferenceid"]);
                    ouniquesequance.usedreferencename = reader["usedreferencename"] == DBNull.Value ? "" : reader["usedreferencename"].ToString();
                    lst.Add(ouniquesequance);
                }
            }
            return lst;
        }

        public List<Uniquesequance> DeleteAll(List<Uniquesequance> items)
        {
            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);
                    foreach (var item in items.OrderBy(e => e.sequenceindex))
                        Delete(transaction, conn, item);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, items);
                
                throw exp;
            }
            return items;
        }

    }


}
