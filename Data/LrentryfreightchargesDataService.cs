using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
namespace InfyPOS.Data

{
    public class LrentryfreightchargesDataService : SAASData
    {
        const string SQL_Search = @"select lrid, chargetypeid, charge, id, createdby, modifiedby, createdon, modifiedon, version, isactive from lrentryfreightcharges where id=@id";
        const string SQL_SearchAll = @"select lrid, chargetypeid, charge, id, createdby, modifiedby, createdon, modifiedon, version, isactive from lrentryfreightcharges where lrid=@lrid and isactive=true";


        const string SQL_Insert = @"INSERT INTO public.lrentryfreightcharges(lrid, chargetypeid, charge, createdby, modifiedby, createdon, modifiedon, version, isactive) VALUES(@lrid, @chargetypeid, @charge, @createdby, @modifiedby, @createdon, @modifiedon, @version, @isactive) RETURNING id";


        const string SQL_Update = @"UPDATE public.lrentryfreightcharges SET lrid=@lrid, chargetypeid=@chargetypeid, charge=@charge, modifiedby=@modifiedby, modifiedon=@modifiedon, version=@version+1 where id=@id and version=@version  RETURNING version";

        const string SQL_Delete = @"update public.lrentryfreightcharges set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version  RETURNING version";
        const string SQL_DeleteAll = @"update public.lrentryfreightcharges set isactive=false,modifiedby=@modifiedby,modifiedon=@modifiedon where lrid=@lrid";
        public List<Lrentryfreightcharges> Search(LrentryfreightchargesCreteria _lrentryfreightcharges)
        {
            List<Lrentryfreightcharges> lst = new List<Lrentryfreightcharges>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_lrentryfreightcharges.id != 0)
                {
                    command = GetCommand(SQL_Search, conn);
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    //if (!string.IsNullOrEmpty(_lrentryfreightcharges.Code))
                    //    qb.AddParameter("code", _lrentryfreightcharges.Code, "=");
                    command = GetCommand(qb.CurrentQuery, conn);
                    //command.Parameters.Add("code", NpgsqlTypes.NpgsqlDbType.Varchar).Value = _lrentryfreightcharges.Code;
                }
                command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _lrentryfreightcharges.id;
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Lrentryfreightcharges olrentryfreightcharges = new Lrentryfreightcharges();
                        olrentryfreightcharges.lrid = reader["lrid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["lrid"].ToString());
                        olrentryfreightcharges.chargetypeid = reader["chargetypeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["chargetypeid"].ToString());
                        olrentryfreightcharges.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        olrentryfreightcharges.charge = reader["charge"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["charge"]);
                        olrentryfreightcharges.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        olrentryfreightcharges.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        olrentryfreightcharges.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        olrentryfreightcharges.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        olrentryfreightcharges.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"].ToString());
                        olrentryfreightcharges.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        lst.Add(olrentryfreightcharges);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                throw exp;
            }
            return lst;
        }

        public List<Lrentryfreightcharges> SearchAll(long _lrid)
        {
            List<Lrentryfreightcharges> lst = new List<Lrentryfreightcharges>();
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
                        Lrentryfreightcharges olrentryfreightcharges = new Lrentryfreightcharges();
                        olrentryfreightcharges.lrid = reader["lrid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["lrid"].ToString());
                        olrentryfreightcharges.chargetypeid = reader["chargetypeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["chargetypeid"].ToString());
                        olrentryfreightcharges.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        olrentryfreightcharges.charge = reader["charge"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["charge"]);
                        olrentryfreightcharges.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        olrentryfreightcharges.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        olrentryfreightcharges.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        olrentryfreightcharges.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        olrentryfreightcharges.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        olrentryfreightcharges.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"].ToString());
                        lst.Add(olrentryfreightcharges);
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

        public Lrentryfreightcharges Insert(Lrentryfreightcharges oLrentryfreightcharges)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);

                    Insert(transaction, conn, oLrentryfreightcharges);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);

            }

            return oLrentryfreightcharges;
        }

        public Lrentryfreightcharges Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Lrentryfreightcharges oLrentryfreightcharges)
        {

            NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("lrid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrentryfreightcharges.lrid;
            command.Parameters.Add("chargetypeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrentryfreightcharges.chargetypeid;
            command.Parameters.Add("charge", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrentryfreightcharges.charge;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oLrentryfreightcharges.version;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oLrentryfreightcharges.isactive;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oLrentryfreightcharges.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            return oLrentryfreightcharges;
        }

        public Lrentryfreightcharges Update(Lrentryfreightcharges oLrentryfreightcharges)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oLrentryfreightcharges);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);

            }

            return oLrentryfreightcharges;
        }

        public Lrentryfreightcharges Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Lrentryfreightcharges oLrentryfreightcharges)
        {

            NpgsqlCommand command = GetCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("lrid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrentryfreightcharges.lrid;
            command.Parameters.Add("chargetypeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrentryfreightcharges.chargetypeid;
            command.Parameters.Add("charge", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrentryfreightcharges.charge;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrentryfreightcharges.id;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrentryfreightcharges.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oLrentryfreightcharges.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oLrentryfreightcharges.version;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oLrentryfreightcharges.isactive;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oLrentryfreightcharges.version = Convert.ToInt32(reader[0]);
                }
            }

            return oLrentryfreightcharges;
        }

        public bool Update(NpgsqlTransaction transaction, NpgsqlConnection conn, List<Lrentryfreightcharges> list, long id)
        {
            var existingItems = SearchAll(id);
            if (existingItems != null && existingItems.Count > 0)
            {
                var deletedItems = existingItems.FindAll(e => !list.Exists(f => f.id == e.id));
                foreach (var item in deletedItems)
                    Delete(transaction, conn, item);
            }

            foreach (var item in list)
            {
                item.lrid = id;
                if (item.id > 0)
                {
                    Update(transaction, conn, item);
                }
                else
                {
                    Insert(transaction, conn, item);
                }
            }
            return true;
        }

        public bool Delete(Lrentryfreightcharges oLrentryfreightcharges)
        {

            NpgsqlTransaction transaction = null;
            int successCount = 0;
            try
            {
                {
                    using (NpgsqlConnection conn = CreateConnection())
                    {
                        {
                            conn.Open();
                            transaction = conn.BeginTransaction();
                            Delete(transaction, conn, oLrentryfreightcharges);
                            transaction.Commit();
                            conn.Close();
                        }
                    }
                }
            }
            catch (Exception exp)
            {
                {

                    WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);
                }
            }
            return successCount > 0;

        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Lrentryfreightcharges oLrentryfreightcharges)
        {

            NpgsqlCommand command = GetCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oLrentryfreightcharges.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oLrentryfreightcharges.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrentryfreightcharges.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

        public bool DeleteAll(long id, long modifiedby, NpgsqlConnection conn, NpgsqlTransaction transaction)
        {


            int successCount = 0;


            NpgsqlCommand command = GetCommand(SQL_DeleteAll, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = id;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("lrid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = id;
            successCount = command.ExecuteNonQuery();


            return successCount > 0;
        }

    }

}
