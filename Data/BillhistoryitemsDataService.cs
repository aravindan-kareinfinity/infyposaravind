using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
namespace InfyPOS.Data

{
    public class BillhistoryitemsDataService : BaseData
    {
        const string SQL_Search = @"select id, billhistoryid, isactive, createdby, modifiedby, version, createdon, modifiedon, oldbilldata, newbilldata, oldsettlementdata, newsettlementdata, multiplesettlement, billid, billsettlementmasterid, deleted from billhistoryitems where id=@id";
        const string SQL_SearchAll = @"select id, billhistoryid, isactive, createdby, modifiedby, version, createdon, modifiedon, oldbilldata, newbilldata, oldsettlementdata, newsettlementdata, multiplesettlement, billid, billsettlementmasterid, deleted from billhistoryitems where billhistoryid=@billhistoryid";

        const string SQL_Insert = @"INSERT INTO public.billhistoryitems(billhistoryid, isactive, createdby, modifiedby, version, createdon, modifiedon, oldbilldata, newbilldata, oldsettlementdata, newsettlementdata, multiplesettlement, billid, billsettlementmasterid, deleted) VALUES(@billhistoryid, @isactive, @createdby, @modifiedby, @version, @createdon, @modifiedon, @oldbilldata, @newbilldata, @oldsettlementdata, @newsettlementdata, @multiplesettlement, @billid, @billsettlementmasterid, @deleted) RETURNING id";

        const string SQL_Update = @"UPDATE public.billhistoryitems SET billhistoryid=@billhistoryid, modifiedby=@modifiedby, version=@version+1, modifiedon=@modifiedon, oldbilldata=@oldbilldata, newbilldata=@newbilldata, oldsettlementdata=@oldsettlementdata, newsettlementdata=@newsettlementdata, multiplesettlement=@multiplesettlement, billid=@billid, billsettlementmasterid=@billsettlementmasterid, deleted=@deleted where id=@id and version=@version RETURNING version";

        const string SQL_Delete = @"delete from public.billhistoryitems where id=@id and version=@version";

        const string SQL_DeleteAll = @"delete from public.billhistoryitems where billhistoryid=@billhistoryid";
        public List<Billhistoryitems> Search(BillhistoryitemsCreteria _billhistoryitems)
        {
            List<Billhistoryitems> lst = new List<Billhistoryitems>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_billhistoryitems.id != 0)
                {
                    command = new NpgsqlCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _billhistoryitems.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("isactive", true, "=");

                    //if (!string.IsNullOrEmpty(_billhistoryitems.Code))
                    //    qb.AddParameter("code", _billhistoryitems.Code, "=");

                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Billhistoryitems obillhistoryitems = new Billhistoryitems();
                        obillhistoryitems.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        obillhistoryitems.billhistoryid = reader["billhistoryid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["billhistoryid"]);
                        obillhistoryitems.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        obillhistoryitems.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        obillhistoryitems.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        obillhistoryitems.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        obillhistoryitems.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        obillhistoryitems.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        obillhistoryitems.oldbilldata_json = reader["oldbilldata"] == DBNull.Value ? null : reader["oldbilldata"].ToString();
                        obillhistoryitems.newbilldata_json = reader["newbilldata"] == DBNull.Value ? null : reader["newbilldata"].ToString();
                        obillhistoryitems.oldsettlementdata_json = reader["oldsettlementdata"] == DBNull.Value ? null : reader["oldsettlementdata"].ToString();
                        obillhistoryitems.newsettlementdata_json = reader["newsettlementdata"] == DBNull.Value ? null : reader["newsettlementdata"].ToString();
                        obillhistoryitems.multiplesettlement = reader["multiplesettlement"] == DBNull.Value ? false : Convert.ToBoolean(reader["multiplesettlement"]);
                        obillhistoryitems.billid = reader["billid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["billid"]);
                        obillhistoryitems.billsettlementmasterid = reader["billsettlementmasterid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["billsettlementmasterid"]);
                        obillhistoryitems.deleted = reader["deleted"] == DBNull.Value ? false : Convert.ToBoolean(reader["deleted"]);
                        lst.Add(obillhistoryitems);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _billhistoryitems);
            }
            return lst;
        }

        public List<Billhistoryitems> SearchAll(long _billhistoryid)
        {
            List<Billhistoryitems> lst = new List<Billhistoryitems>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = new NpgsqlCommand(SQL_SearchAll, conn);
                command.Parameters.Add("billhistoryid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _billhistoryid;
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Billhistoryitems obillhistoryitems = new Billhistoryitems();
                        obillhistoryitems.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        obillhistoryitems.billhistoryid = reader["billhistoryid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["billhistoryid"]);
                        obillhistoryitems.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        obillhistoryitems.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        obillhistoryitems.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        obillhistoryitems.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        obillhistoryitems.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        obillhistoryitems.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        obillhistoryitems.multiplesettlement = reader["multiplesettlement"] == DBNull.Value ? false : Convert.ToBoolean(reader["multiplesettlement"]);
                        obillhistoryitems.billid = reader["billid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["billid"]);
                        obillhistoryitems.billsettlementmasterid = reader["billsettlementmasterid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["billsettlementmasterid"]);
                        obillhistoryitems.deleted = reader["deleted"] == DBNull.Value ? false : Convert.ToBoolean(reader["deleted"]);

                        obillhistoryitems.oldbilldata_json = reader["oldbilldata"] == DBNull.Value ? null : reader["oldbilldata"].ToString();
                        obillhistoryitems.newbilldata_json = reader["newbilldata"] == DBNull.Value ? null : reader["newbilldata"].ToString();
                        obillhistoryitems.oldsettlementdata_json = reader["oldsettlementdata"] == DBNull.Value ? null : reader["oldsettlementdata"].ToString();
                        obillhistoryitems.newsettlementdata_json = reader["newsettlementdata"] == DBNull.Value ? null : reader["newsettlementdata"].ToString();

                        lst.Add(obillhistoryitems);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _billhistoryid);
            }
            return lst;
        }

        public Billhistoryitems Insert(Billhistoryitems oBillhistoryitems)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);

                    Insert(transaction, conn, oBillhistoryitems);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oBillhistoryitems);
            }

            return oBillhistoryitems;
        }

        public Billhistoryitems Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Billhistoryitems oBillhistoryitems)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("billhistoryid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillhistoryitems.billhistoryid;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBillhistoryitems.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillhistoryitems.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillhistoryitems.modifiedby;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBillhistoryitems.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBillhistoryitems.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBillhistoryitems.modifiedon;
            command.Parameters.Add("oldbilldata", NpgsqlTypes.NpgsqlDbType.Json).Value = oBillhistoryitems.oldbilldata_json == null ? "null" : oBillhistoryitems.oldbilldata_json;
            command.Parameters.Add("newbilldata", NpgsqlTypes.NpgsqlDbType.Json).Value = oBillhistoryitems.newbilldata_json == null ? "null" : oBillhistoryitems.newbilldata_json;
            command.Parameters.Add("oldsettlementdata", NpgsqlTypes.NpgsqlDbType.Json).Value = oBillhistoryitems.oldsettlementdata_json == null ? "null" : oBillhistoryitems.oldsettlementdata_json;
            command.Parameters.Add("newsettlementdata", NpgsqlTypes.NpgsqlDbType.Json).Value = oBillhistoryitems.newsettlementdata_json == null ? "null" : oBillhistoryitems.newsettlementdata_json;
            command.Parameters.Add("multiplesettlement", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBillhistoryitems.multiplesettlement;
            command.Parameters.Add("billid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillhistoryitems.billid;
            command.Parameters.Add("billsettlementmasterid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillhistoryitems.billsettlementmasterid;
            command.Parameters.Add("deleted", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBillhistoryitems.deleted;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oBillhistoryitems.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            return oBillhistoryitems;
        }

        public Billhistoryitems Update(Billhistoryitems oBillhistoryitems)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oBillhistoryitems);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oBillhistoryitems);
            }

            return oBillhistoryitems;
        }

        public Billhistoryitems Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Billhistoryitems oBillhistoryitems)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillhistoryitems.id;
            command.Parameters.Add("billhistoryid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillhistoryitems.billhistoryid;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBillhistoryitems.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillhistoryitems.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillhistoryitems.modifiedby;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBillhistoryitems.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBillhistoryitems.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBillhistoryitems.modifiedon;
            command.Parameters.Add("oldbilldata", NpgsqlTypes.NpgsqlDbType.Json).Value = oBillhistoryitems.oldbilldata_json == null ? "null" : oBillhistoryitems.oldbilldata_json;
            command.Parameters.Add("newbilldata", NpgsqlTypes.NpgsqlDbType.Json).Value = oBillhistoryitems.newbilldata_json == null ? "null" : oBillhistoryitems.newbilldata_json;
            command.Parameters.Add("oldsettlementdata", NpgsqlTypes.NpgsqlDbType.Json).Value = oBillhistoryitems.oldsettlementdata_json == null ? "null" : oBillhistoryitems.oldsettlementdata_json;
            command.Parameters.Add("newsettlementdata", NpgsqlTypes.NpgsqlDbType.Json).Value = oBillhistoryitems.newsettlementdata_json == null ? "null" : oBillhistoryitems.newsettlementdata_json;
            command.Parameters.Add("multiplesettlement", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBillhistoryitems.multiplesettlement;
            command.Parameters.Add("billid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillhistoryitems.billid;
            command.Parameters.Add("billsettlementmasterid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillhistoryitems.billsettlementmasterid;
            command.Parameters.Add("deleted", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBillhistoryitems.deleted;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oBillhistoryitems.version = Convert.ToInt32(reader[0].ToString());
                }
            }

            return oBillhistoryitems;
        }

        public bool Update(NpgsqlTransaction transaction, NpgsqlConnection conn, List<Billhistoryitems> list, long id)
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
                if (item.id > 0)
                {
                    Update(transaction, conn, item);
                }
                else
                {
                    item.billhistoryid = id;
                    Insert(transaction, conn, item);
                }
            }
            return true;
        }

        public bool Delete(Billhistoryitems oBillhistoryitems)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oBillhistoryitems);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oBillhistoryitems);

            }
            return hasSuccessCount;
        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Billhistoryitems oBillhistoryitems)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBillhistoryitems.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillhistoryitems.modifiedby;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBillhistoryitems.modifiedon;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBillhistoryitems.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillhistoryitems.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

        public bool DeleteAll(long billhistoryid, long userid, NpgsqlConnection conn, NpgsqlTransaction transaction)
        {


            int successCount = 0;
            NpgsqlCommand command = new NpgsqlCommand(SQL_DeleteAll, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = false;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = userid;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("billhistoryid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = billhistoryid;
            successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

    }

}
