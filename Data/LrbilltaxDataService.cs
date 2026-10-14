using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
namespace InfyPOS.Data

{
    public class LrbilltaxDataService : SAASData
    {
        const string SQL_Search = @"select lrid, chargetypeid, amounton, percentage, netamount, createdby, modifiedby, createdon, modifiedon, version, isactive, id from lrbilltax where id=@id";
        const string SQL_SearchAll = @"select lrid, chargetypeid, amounton, percentage, netamount, createdby, modifiedby, createdon, modifiedon, version, isactive, id from lrbilltax where lrid=@lrid";


        const string SQL_Insert = @"INSERT INTO public.lrbilltax(lrid, chargetypeid, amounton, percentage, netamount, createdby, modifiedby, createdon, modifiedon, version, isactive) VALUES(@lrid, @chargetypeid, @amounton, @percentage, @netamount, @createdby, @modifiedby, @createdon, @modifiedon, @version, @isactive) RETURNING id";


        const string SQL_Update = @"UPDATE public.lrbilltax SET lrid=@lrid, chargetypeid=@chargetypeid, amounton=@amounton, percentage=@percentage, netamount=@netamount, modifiedby=@modifiedby, modifiedon=@modifiedon, version=@version+1 where id=@id and version=@version";

        const string SQL_Delete = @"update public.lrbilltax set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";
        const string SQL_DeleteAll = @"update public.lrbilltax set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where lrid=@lrid and version=@version";
        public List<Lrbilltax> Search(LrbilltaxCreteria _lrbilltax)
        {
            List<Lrbilltax> lst = new List<Lrbilltax>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_lrbilltax.id != 0)
                {
                    command = GetCommand(SQL_Search, conn);
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    //if (!string.IsNullOrEmpty(_lrbilltax.Code))
                    //    qb.AddParameter("code", _lrbilltax.Code, "=");
                    command = GetCommand(qb.CurrentQuery, conn);
                    //command.Parameters.Add("code", NpgsqlTypes.NpgsqlDbType.Varchar).Value = _lrbilltax.Code;
                }
                command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _lrbilltax.id;
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Lrbilltax olrbilltax = new Lrbilltax();
                        olrbilltax.lrid = reader["lrid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["lrid"].ToString());
                        olrbilltax.chargetypeid = reader["chargetypeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["chargetypeid"].ToString());
                        olrbilltax.percentage = reader["percentage"] == DBNull.Value ? "" : reader["percentage"].ToString();
                        olrbilltax.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"].ToString());
                        olrbilltax.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"].ToString());
                        olrbilltax.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"].ToString());
                        olrbilltax.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"].ToString());
                        olrbilltax.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"].ToString());
                        olrbilltax.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"].ToString());
                        lst.Add(olrbilltax);
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

        public List<Lrbilltax> SearchAll(long _lrid)
        {
            List<Lrbilltax> lst = new List<Lrbilltax>();
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
                        Lrbilltax olrbilltax = new Lrbilltax();
                        olrbilltax.lrid = reader["lrid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["lrid"].ToString());
                        olrbilltax.chargetypeid = reader["chargetypeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["chargetypeid"].ToString());
                        olrbilltax.percentage = reader["percentage"] == DBNull.Value ? "" : reader["percentage"].ToString();
                        olrbilltax.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"].ToString());
                        olrbilltax.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"].ToString());
                        olrbilltax.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"].ToString());
                        olrbilltax.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"].ToString());
                        olrbilltax.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"].ToString());
                        olrbilltax.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"].ToString());
                        lst.Add(olrbilltax);
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

        public Lrbilltax Insert(Lrbilltax oLrbilltax)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);

                    Insert(transaction, conn, oLrbilltax);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);

            }

            return oLrbilltax;
        }

        public Lrbilltax Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Lrbilltax oLrbilltax)
        {

            NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("lrid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrbilltax.lrid;
            command.Parameters.Add("chargetypeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrbilltax.chargetypeid;
            command.Parameters.Add("amounton", NpgsqlTypes.NpgsqlDbType.Double).Value = oLrbilltax.amounton;
            command.Parameters.Add("percentage", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oLrbilltax.percentage == null ? "" : oLrbilltax.percentage;
            command.Parameters.Add("netamount", NpgsqlTypes.NpgsqlDbType.Double).Value = oLrbilltax.netamount;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oLrbilltax.version;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oLrbilltax.isactive;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oLrbilltax.id = Convert.ToInt32(reader[0].ToString());
                }
            }
            return oLrbilltax;
        }

        public Lrbilltax Update(Lrbilltax oLrbilltax)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oLrbilltax);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);

            }

            return oLrbilltax;
        }

        public Lrbilltax Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Lrbilltax oLrbilltax)
        {

            NpgsqlCommand command = GetCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("lrid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrbilltax.lrid;
            command.Parameters.Add("chargetypeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrbilltax.chargetypeid;
            command.Parameters.Add("amounton", NpgsqlTypes.NpgsqlDbType.Double).Value = oLrbilltax.amounton;
            command.Parameters.Add("percentage", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oLrbilltax.percentage == null ? "" : oLrbilltax.percentage;
            command.Parameters.Add("netamount", NpgsqlTypes.NpgsqlDbType.Double).Value = oLrbilltax.netamount;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrbilltax.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oLrbilltax.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oLrbilltax.version;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oLrbilltax.isactive;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrbilltax.id;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oLrbilltax.id = Convert.ToInt32(reader[0].ToString());
                }
            }

            return oLrbilltax;
        }

        public bool Delete(Lrbilltax oLrbilltax)
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
                    command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oLrbilltax.isactive;
                    command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                    command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
                    command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oLrbilltax.version;
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrbilltax.id;
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

        public bool DeleteAll(Lrbilltax oLrbilltax)
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
                    command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oLrbilltax.isactive;
                    command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                    command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
                    command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oLrbilltax.version;
                    command.Parameters.Add("lrid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrbilltax.lrid;
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
