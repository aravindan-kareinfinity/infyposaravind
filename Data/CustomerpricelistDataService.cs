using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
namespace InfyPOS.Data

{
    public class CustomerpricelistDataService : BaseData
    {
        const string SQL_Search = @"select createdon, isactive, id, modifiedby, version, createdby, modifiedon, customercategoryid, customerid, customerpriceid, price from customerpricelist where id=@id";
        const string SQL_SearchAll = @"select createdon, isactive, id, modifiedby, version, createdby, modifiedon, customercategoryid, customerid, customerpriceid, price from customerpricelist where customerpriceid=@customerpriceid";
        const string SQL_Insert = @"INSERT INTO public.customerpricelist(createdon, isactive, modifiedby, version, createdby, modifiedon, customercategoryid, customerid, customerpriceid, price) VALUES(@createdon, @isactive, @modifiedby, @version, @createdby, @modifiedon, @customercategoryid, @customerid, @customerpriceid, @price) RETURNING id";
        const string SQL_Update = @"UPDATE public.customerpricelist SET modifiedby=@modifiedby, version=@version+1, modifiedon=@modifiedon, customercategoryid=@customercategoryid, customerid=@customerid, customerpriceid=@customerpriceid, price=@price where id=@id and version=@version RETURNING version";
        const string SQL_Delete = @"update public.customerpricelist set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";
        const string SQL_DeleteAll = @"update public.customerpricelist set isactive=false,modifiedby=@modifiedby,modifiedon=@modifiedon where customerpriceid=@customerpriceid";
        public List<Customerpricelist> Search(CustomerpricelistCreteria _customerpricelist)
        {
            List<Customerpricelist> lst = new List<Customerpricelist>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_customerpricelist.id != 0)
                {
                    command = new NpgsqlCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _customerpricelist.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("isactive", true, "=");

                    //if (!string.IsNullOrEmpty(_customerpricelist.Code))
                    //    qb.AddParameter("code", _customerpricelist.Code, "=");

                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Customerpricelist ocustomerpricelist = new Customerpricelist();
                        ocustomerpricelist.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        ocustomerpricelist.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        ocustomerpricelist.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        ocustomerpricelist.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        ocustomerpricelist.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        ocustomerpricelist.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        ocustomerpricelist.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        ocustomerpricelist.customercategoryid = reader["customercategoryid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["customercategoryid"]);
                        ocustomerpricelist.customerid = reader["customerid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["customerid"]);
                        ocustomerpricelist.customerpriceid = reader["customerpriceid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["customerpriceid"]);
                        ocustomerpricelist.price = reader["price"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["price"]);
                        lst.Add(ocustomerpricelist);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _customerpricelist);
            }
            return lst;
        }

        public List<Customerpricelist> SearchAll(long _customerpriceid)
        {
            List<Customerpricelist> lst = new List<Customerpricelist>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = new NpgsqlCommand(SQL_SearchAll, conn);
                command.Parameters.Add("customerpriceid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _customerpriceid;
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Customerpricelist ocustomerpricelist = new Customerpricelist();
                        ocustomerpricelist.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        ocustomerpricelist.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        ocustomerpricelist.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        ocustomerpricelist.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        ocustomerpricelist.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        ocustomerpricelist.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        ocustomerpricelist.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        ocustomerpricelist.customercategoryid = reader["customercategoryid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["customercategoryid"]);
                        ocustomerpricelist.customerid = reader["customerid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["customerid"]);
                        ocustomerpricelist.customerpriceid = reader["customerpriceid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["customerpriceid"]);
                        ocustomerpricelist.price = reader["price"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["price"]);
                        lst.Add(ocustomerpricelist);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _customerpriceid);
            }
            return lst;
        }

        public Customerpricelist Insert(Customerpricelist oCustomerpricelist)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);

                    Insert(transaction, conn, oCustomerpricelist);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oCustomerpricelist);
            }

            return oCustomerpricelist;
        }

        public Customerpricelist Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Customerpricelist oCustomerpricelist)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oCustomerpricelist.createdon;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oCustomerpricelist.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCustomerpricelist.modifiedby;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oCustomerpricelist.version;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCustomerpricelist.createdby;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oCustomerpricelist.modifiedon;
            command.Parameters.Add("customercategoryid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCustomerpricelist.customercategoryid;
            command.Parameters.Add("customerid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCustomerpricelist.customerid;
            command.Parameters.Add("customerpriceid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCustomerpricelist.customerpriceid;
            command.Parameters.Add("price", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oCustomerpricelist.price;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oCustomerpricelist.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            return oCustomerpricelist;
        }

        public Customerpricelist Update(Customerpricelist oCustomerpricelist)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oCustomerpricelist);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oCustomerpricelist);
            }

            return oCustomerpricelist;
        }

        public Customerpricelist Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Customerpricelist oCustomerpricelist)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oCustomerpricelist.createdon;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oCustomerpricelist.isactive;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCustomerpricelist.id;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCustomerpricelist.modifiedby;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oCustomerpricelist.version;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCustomerpricelist.createdby;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oCustomerpricelist.modifiedon;
            command.Parameters.Add("customercategoryid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCustomerpricelist.customercategoryid;
            command.Parameters.Add("customerid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCustomerpricelist.customerid;
            command.Parameters.Add("customerpriceid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCustomerpricelist.customerpriceid;
            command.Parameters.Add("price", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oCustomerpricelist.price;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oCustomerpricelist.version = Convert.ToInt32(reader[0].ToString());
                }
            }

            return oCustomerpricelist;
        }

        public bool Update(NpgsqlTransaction transaction, NpgsqlConnection conn, List<Customerpricelist> list, long id)
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
                    item.customerpriceid = id;
                    Insert(transaction, conn, item);
                }
            }
            return true;
        }

        public bool Delete(Customerpricelist oCustomerpricelist)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oCustomerpricelist);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oCustomerpricelist);
            }
            return hasSuccessCount;

        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Customerpricelist oCustomerpricelist)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oCustomerpricelist.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCustomerpricelist.modifiedby;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oCustomerpricelist.modifiedon;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oCustomerpricelist.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCustomerpricelist.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

        public bool DeleteAll(long customerpriceid, long userid, NpgsqlConnection conn, NpgsqlTransaction transaction)
        {


            int successCount = 0;
            NpgsqlCommand command = new NpgsqlCommand(SQL_DeleteAll, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = false;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = userid;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("customerpriceid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = customerpriceid;
            successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

    }

}
