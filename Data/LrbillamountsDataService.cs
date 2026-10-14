using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
namespace InfyPOS.Data

{
    public class LrbillamountsDataService : SAASData
    {
        const string SQL_Search = @"select lrid, suppliercompanyid, buyingcompanyid, totalamount, version, createdby, modifiedby, createdon, modifiedon, isactive, id from lrbillamounts where id=@id";
        const string SQL_SearchAll = @"select lrid, suppliercompanyid, buyingcompanyid, totalamount, version, createdby, modifiedby, createdon, modifiedon, isactive, id from lrbillamounts where lrid=@lrid";


        const string SQL_Insert = @"INSERT INTO public.lrbillamounts(lrid, suppliercompanyid, buyingcompanyid, totalamount, version, createdby, modifiedby, createdon, modifiedon, isactive) VALUES(@lrid, @suppliercompanyid, @buyingcompanyid, @totalamount, @version, @createdby, @modifiedby, @createdon, @modifiedon, @isactive) RETURNING id";


        const string SQL_Update = @"UPDATE public.lrbillamounts SET lrid=@lrid, suppliercompanyid=@suppliercompanyid, buyingcompanyid=@buyingcompanyid, totalamount=@totalamount, version=@version+1, modifiedby=@modifiedby, modifiedon=@modifiedon where id=@id and version=@version  RETURNING version";

        const string SQL_Delete = @"update public.lrbillamounts set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version  RETURNING version";
        const string SQL_DeleteAll = @"update public.lrbillamounts set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where lrid=@lrid and version=@version";
        public List<Lrbillamounts> Search(LrbillamountsCreteria _lrbillamounts)
        {
            List<Lrbillamounts> lst = new List<Lrbillamounts>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_lrbillamounts.id != 0)
                {
                    command = GetCommand(SQL_Search, conn);
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    //if (!string.IsNullOrEmpty(_lrbillamounts.Code))
                    //    qb.AddParameter("code", _lrbillamounts.Code, "=");
                    command = GetCommand(qb.CurrentQuery, conn);
                    //command.Parameters.Add("code", NpgsqlTypes.NpgsqlDbType.Varchar).Value = _lrbillamounts.Code;
                }
                command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _lrbillamounts.id;
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Lrbillamounts olrbillamounts = new Lrbillamounts();
                        olrbillamounts.lrid = reader["lrid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["lrid"].ToString());
                        olrbillamounts.suppliercompanyid = reader["suppliercompanyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["suppliercompanyid"].ToString());
                        olrbillamounts.buyingcompanyid = reader["buyingcompanyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["buyingcompanyid"].ToString());
                        olrbillamounts.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt64(reader["version"].ToString());
                        olrbillamounts.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        olrbillamounts.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        olrbillamounts.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        olrbillamounts.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        olrbillamounts.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        olrbillamounts.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        lst.Add(olrbillamounts);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);
                throw exp;
            }
            return lst;
        }

        public List<Lrbillamounts> SearchAll(long _lrid)
        {
            List<Lrbillamounts> lst = new List<Lrbillamounts>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = GetCommand(SQL_SearchAll, conn);
                command.Parameters.Add("lrid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _lrid;
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Lrbillamounts olrbillamounts = new Lrbillamounts();
                        olrbillamounts.lrid = reader["lrid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["lrid"].ToString());
                        olrbillamounts.suppliercompanyid = reader["suppliercompanyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["suppliercompanyid"].ToString());
                        olrbillamounts.buyingcompanyid = reader["buyingcompanyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["buyingcompanyid"].ToString());
                        olrbillamounts.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt64(reader["version"].ToString());
                        olrbillamounts.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        olrbillamounts.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        olrbillamounts.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        olrbillamounts.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        olrbillamounts.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        olrbillamounts.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        lst.Add(olrbillamounts);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);
                throw exp;
            }
            return lst;
        }

        public Lrbillamounts Insert(Lrbillamounts oLrbillamounts)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);

                    Insert(transaction, conn, oLrbillamounts);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {

                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);
            }

            return oLrbillamounts;
        }

        public Lrbillamounts Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Lrbillamounts oLrbillamounts)
        {

            NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("lrid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrbillamounts.lrid;
            command.Parameters.Add("suppliercompanyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrbillamounts.suppliercompanyid;
            command.Parameters.Add("buyingcompanyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrbillamounts.buyingcompanyid;
            command.Parameters.Add("totalamount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrbillamounts.totalamount;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrbillamounts.version;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oLrbillamounts.isactive;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oLrbillamounts.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            return oLrbillamounts;
        }

        public Lrbillamounts Update(Lrbillamounts oLrbillamounts)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oLrbillamounts);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {

                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);
            }

            return oLrbillamounts;
        }

        public Lrbillamounts Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Lrbillamounts oLrbillamounts)
        {

            NpgsqlCommand command = GetCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("lrid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrbillamounts.lrid;
            command.Parameters.Add("suppliercompanyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrbillamounts.suppliercompanyid;
            command.Parameters.Add("buyingcompanyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrbillamounts.buyingcompanyid;
            command.Parameters.Add("totalamount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrbillamounts.totalamount;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrbillamounts.version;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrbillamounts.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oLrbillamounts.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oLrbillamounts.isactive;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrbillamounts.id;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oLrbillamounts.version = Convert.ToInt32(reader[0]);
                }
            }

            return oLrbillamounts;
        }

        public bool Delete(Lrbillamounts oLrbillamounts)
        {

            NpgsqlTransaction transaction = null;
            int successCount = 0;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = GetCommand(SQL_Delete, conn, transaction);
                    command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oLrbillamounts.isactive;
                    command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                    command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
                    command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oLrbillamounts.version;
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrbillamounts.id;
                    successCount = command.ExecuteNonQuery();
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);

            }
            return successCount > 0;
        }

        public bool DeleteAll(Lrbillamounts oLrbillamounts)
        {

            NpgsqlTransaction transaction = null;
            int successCount = 0;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = GetCommand(SQL_DeleteAll, conn, transaction);
                    command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oLrbillamounts.isactive;
                    command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                    command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
                    command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oLrbillamounts.version;
                    command.Parameters.Add("lrid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrbillamounts.lrid;
                    successCount = command.ExecuteNonQuery();
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);

            }
            return successCount > 0;
        }

    }

}
