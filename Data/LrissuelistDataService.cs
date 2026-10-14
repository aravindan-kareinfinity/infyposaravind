using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
namespace InfyPOS.Data

{
    public class LrissuelistDataService : SAASData
    {
        const string SQL_Search = @"select lrissuelist.id, lrissuelist.isactive, lrissuelist.createdby, lrissuelist.modifiedby, lrentry.lrentryno, lrentry.lrdate,
lrissuelist.version, lrissuelist.createdon, lrissuelist.modifiedon, lrid, lrissuelist.lrno, lrissuelist.supplierid, lrissuelist.noofboxes , lrissuelist.weight,
lrissuelist.noofbundels,lrissuelist.noofpieces, lrissuelist.fromlocationid,lrissuelist.totalamount, lrissuelist.reversed, lrissuelist.reversedid, lrissuelist.lrissueid from lrissuelist
inner join lrentry on lrentry.id = lrissuelist.lrid
where lrissuelist.id=@id";
        const string SQL_SearchAll = @"select id, isactive, createdby, modifiedby, version, createdon, modifiedon, lrid, lrno, supplierid, fromlocationid, totalamount, noofbundels, 
noofpieces, reversed, reversedid, lrissueid, noofboxes, weight from lrissuelist where lrissueid=@lrissueid";


        const string SQL_Insert = @"INSERT INTO public.lrissuelist(isactive, createdby, modifiedby, version, createdon, modifiedon, lrid, lrno, supplierid, fromlocationid, totalamount, noofbundels, noofpieces, reversed, reversedid, lrissueid, noofboxes, weight) 
                            VALUES(@isactive, @createdby, @modifiedby, @version, @createdon, @modifiedon, @lrid, @lrno, @supplierid, @fromlocationid, @totalamount, @noofbundels, @noofpieces, @reversed, @reversedid, @lrissueid, @noofboxes, @weight) RETURNING id";


        const string SQL_Update = @"UPDATE public.lrissuelist SET modifiedby=@modifiedby, version=@version+1, modifiedon=@modifiedon, lrid=@lrid, 
        lrno=@lrno, supplierid=@supplierid, totalamount=@totalamount, frightcharges=@frightcharges, noofbundels=@noofbundels, noofpieces=@noofpieces,
        reversed=@reversed, reversedid=@reversedid, lrissueid=@lrissueid, noofboxes =@noofboxes, weight=@weight where id=@id and version=@version  RETURNING version";

        const string SQL_Delete = @"update public.lrissuelist set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version  RETURNING version";
        const string SQL_DeleteAll = @"update public.lrissuelist set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where lrissueid=@lrissueid and version=@version";
        public List<Lrissuelist> Search(LrissuelistCreteria _lrissuelist)
        {
            List<Lrissuelist> lst = new List<Lrissuelist>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_lrissuelist.id != 0)
                {
                    command = GetCommand(SQL_Search, conn);
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("lrissuelist.isactive", true, "=");

                    if (_lrissuelist.lrissueid > 0)
                        qb.AddParameter("lrissueid", _lrissuelist.lrissueid, "=");

                    if (!string.IsNullOrEmpty(_lrissuelist.lrno))
                        qb.AddParameter("lrno", _lrissuelist.lrno, "=");
                    command = qb.GetCommand(conn);
                }
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Lrissuelist olrissuelist = new Lrissuelist();
                        olrissuelist.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        olrissuelist.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        olrissuelist.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        olrissuelist.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        olrissuelist.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        olrissuelist.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        olrissuelist.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"].ToString());
                        olrissuelist.noofbundels = reader["noofbundels"] == DBNull.Value ? 0 : Convert.ToInt32(reader["noofbundels"].ToString());
                        olrissuelist.noofpieces = reader["noofpieces"] == DBNull.Value ? 0 : Convert.ToInt32(reader["noofpieces"].ToString());
                        olrissuelist.noofboxes = reader["noofboxes"] == DBNull.Value ? 0 : Convert.ToInt32(reader["noofboxes"].ToString());
                        olrissuelist.weight = reader["weight"] == DBNull.Value ? 0 : Convert.ToInt32(reader["weight"].ToString());
                        olrissuelist.lrid = reader["lrid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["lrid"].ToString());
                        olrissuelist.lrno = reader["lrno"] == DBNull.Value ? "" : reader["lrno"].ToString();
                        olrissuelist.lrentryno = reader["lrentryno"] == DBNull.Value ? "" : reader["lrentryno"].ToString();
                        olrissuelist.lrdate = reader["lrdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["lrdate"]);
                        olrissuelist.supplierid = reader["supplierid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["supplierid"].ToString());
                        olrissuelist.fromlocationid = reader["fromlocationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["fromlocationid"].ToString());
                        olrissuelist.totalamount = reader["totalamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["totalamount"].ToString());
                        olrissuelist.reversed = reader["reversed"] == DBNull.Value ? false : Convert.ToBoolean(reader["reversed"].ToString());
                        olrissuelist.reversedid = reader["reversedid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["reversedid"].ToString());
                        olrissuelist.lrissueid = reader["lrissueid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["lrissueid"].ToString());
                        lst.Add(olrissuelist);
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

        public List<Lrissuelist> SearchAll(long _lrissueid)
        {
            List<Lrissuelist> lst = new List<Lrissuelist>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = GetCommand(SQL_SearchAll, conn);
                command.Parameters.Add("lrissueid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _lrissueid;
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Lrissuelist olrissuelist = new Lrissuelist();
                        olrissuelist.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        olrissuelist.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        olrissuelist.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        olrissuelist.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        olrissuelist.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        olrissuelist.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        olrissuelist.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"].ToString());
                        olrissuelist.lrid = reader["lrid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["lrid"].ToString());
                        olrissuelist.lrno = reader["lrno"] == DBNull.Value ? "" : reader["lrno"].ToString();
                        olrissuelist.noofbundels = reader["noofbundels"] == DBNull.Value ? 0 : Convert.ToInt32(reader["noofbundels"].ToString());
                        olrissuelist.noofpieces = reader["noofpieces"] == DBNull.Value ? 0 : Convert.ToInt32(reader["noofpieces"].ToString());
                        olrissuelist.supplierid = reader["supplierid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["supplierid"].ToString());
                        olrissuelist.noofboxes = reader["noofboxes"] == DBNull.Value ? 0 : Convert.ToInt32(reader["noofboxes"].ToString());
                        olrissuelist.weight = reader["weight"] == DBNull.Value ? 0 : Convert.ToInt32(reader["weight"].ToString());
                        olrissuelist.fromlocationid = reader["fromlocationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["fromlocationid"].ToString());
                        olrissuelist.totalamount = reader["totalamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["totalamount"].ToString());
                        olrissuelist.reversed = reader["reversed"] == DBNull.Value ? false : Convert.ToBoolean(reader["reversed"].ToString());
                        olrissuelist.reversedid = reader["reversedid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["reversedid"].ToString());
                        olrissuelist.lrissueid = reader["lrissueid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["lrissueid"].ToString());
                        lst.Add(olrissuelist);
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

        public Lrissuelist Insert(Lrissuelist oLrissuelist)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);

                    Insert(transaction, conn, oLrissuelist);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);

            }

            return oLrissuelist;
        }

        public Lrissuelist Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Lrissuelist oLrissuelist)
        {

            NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oLrissuelist.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oLrissuelist.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("lrid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrissuelist.lrid;
            command.Parameters.Add("lrno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oLrissuelist.lrno == null ? "" : oLrissuelist.lrno;
            command.Parameters.Add("supplierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrissuelist.supplierid;
            command.Parameters.Add("fromlocationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrissuelist.fromlocationid; ;
            command.Parameters.Add("totalamount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrissuelist.totalamount;
            command.Parameters.Add("noofbundels", NpgsqlTypes.NpgsqlDbType.Integer).Value = oLrissuelist.noofbundels;
            command.Parameters.Add("noofpieces", NpgsqlTypes.NpgsqlDbType.Integer).Value = oLrissuelist.noofpieces;
            command.Parameters.Add("noofboxes", NpgsqlTypes.NpgsqlDbType.Integer).Value = oLrissuelist.noofboxes;
            command.Parameters.Add("weight", NpgsqlTypes.NpgsqlDbType.Integer).Value = oLrissuelist.weight;
            command.Parameters.Add("reversed", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oLrissuelist.reversed;
            command.Parameters.Add("reversedid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrissuelist.reversedid;
            command.Parameters.Add("lrissueid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrissuelist.lrissueid;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oLrissuelist.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            return oLrissuelist;
        }

        public bool Update(NpgsqlTransaction transaction, NpgsqlConnection conn, List<Lrissuelist> list, long id)
        {
            var existingItems = SearchAll(id);
            if (existingItems != null && existingItems.Count > 0)
            {
                var deletedItems = existingItems.FindAll(e => !list.Exists(f => f.id == e.id));
                foreach (var item in deletedItems)
                {
                    Delete(transaction, conn, item);
                }
            }

            foreach (var item in list)
            {
                if (item.id > 0)
                {
                    Update(transaction, conn, item);
                }
                else
                {
                    item.lrissueid = id;
                    Insert(transaction, conn, item);
                }
            }
            return true;
        }

        public Lrissuelist Update(Lrissuelist oLrissuelist)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oLrissuelist);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);

            }

            return oLrissuelist;
        }

        public Lrissuelist Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Lrissuelist oLrissuelist)
        {

            NpgsqlCommand command = GetCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrissuelist.id;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oLrissuelist.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrissuelist.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oLrissuelist.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oLrissuelist.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("lrid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrissuelist.lrid;
            command.Parameters.Add("lrno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oLrissuelist.lrno == null ? "" : oLrissuelist.lrno;
            command.Parameters.Add("supplierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrissuelist.supplierid;
            command.Parameters.Add("fromlocationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrissuelist.fromlocationid;
            command.Parameters.Add("noofboxes", NpgsqlTypes.NpgsqlDbType.Integer).Value = oLrissuelist.noofboxes;
            command.Parameters.Add("weight", NpgsqlTypes.NpgsqlDbType.Integer).Value = oLrissuelist.weight;
            command.Parameters.Add("totalamount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrissuelist.totalamount;
            command.Parameters.Add("noofbundels", NpgsqlTypes.NpgsqlDbType.Integer).Value = oLrissuelist.noofbundels;
            command.Parameters.Add("noofpieces", NpgsqlTypes.NpgsqlDbType.Integer).Value = oLrissuelist.noofpieces;
            command.Parameters.Add("reversed", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oLrissuelist.reversed;
            command.Parameters.Add("reversedid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrissuelist.reversedid;
            command.Parameters.Add("lrissueid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrissuelist.lrissueid;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oLrissuelist.version = Convert.ToInt32(reader[0]);
                }
            }

            return oLrissuelist;
        }

        public bool Delete(Lrissuelist oLrissuelist)
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
                    command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oLrissuelist.isactive;
                    command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                    command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
                    command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oLrissuelist.version;
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrissuelist.id;
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

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Lrissuelist lrissue)
        {
            NpgsqlCommand command = GetCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = false;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = lrissue.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = lrissue.id;
            int successCount = command.ExecuteNonQuery();
            return successCount > 0;

        }
        public bool DeleteAll(Lrissuelist oLrissuelist)
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
                    command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oLrissuelist.isactive;
                    command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                    command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
                    command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oLrissuelist.version;
                    command.Parameters.Add("lrissueid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrissuelist.lrissueid;
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
