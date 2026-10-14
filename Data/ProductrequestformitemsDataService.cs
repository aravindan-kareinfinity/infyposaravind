using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
namespace InfyPOS.Data

{
    public class ProductrequestformitemsDataService : SAASData
    {
        const string SQL_Search = @"select id, productid, itemid, qty, isactive, createdby, modifiedby, version, productrequestformid, createdon, modifiedon, supplierid,designid,description,pricerange from productrequestformitems where id=@id";
        const string SQL_SearchAll = @"select id, productid, itemid, qty, isactive, createdby, modifiedby, version, productrequestformid, createdon, modifiedon, supplierid,designid,description,pricerange from productrequestformitems where productrequestformid=@productrequestformid";

        const string SQL_Insert = @"INSERT INTO public.productrequestformitems(productid, itemid, qty, isactive, createdby, modifiedby, version, productrequestformid, createdon, modifiedon,rqty,cqty,supplierid,designid,description,pricerange) 
                    VALUES(@productid, @itemid, @qty, @isactive, @createdby, @modifiedby, @version, @productrequestformid, @createdon, @modifiedon,0,0,@supplierid,@designid,@description,@pricerange) RETURNING id";

        const string SQL_Update = @"UPDATE public.productrequestformitems SET productid=@productid, itemid=@itemid, qty=@qty, modifiedby=@modifiedby, version=@version+1,designid=@designid,description=@description, productrequestformid=@productrequestformid, modifiedon=@modifiedon, supplierid=@supplierid,pricerange=@pricerange where id=@id and version=@version RETURNING version";

        const string SQL_Delete = @"update public.productrequestformitems set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";

        const string SQL_DeleteAll = @"update public.productrequestformitems set isactive=false,modifiedby=@modifiedby,modifiedon=@modifiedon where productrequestformid=@productrequestformid";

        const string SQL_CancelOrder = @"update public.productrequestformitems set cqty=@cqty,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";

        public List<Productrequestformitems> Search(ProductrequestformitemsCreteria _productrequestformitems)
        {
            List<Productrequestformitems> lst = new List<Productrequestformitems>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_productrequestformitems.id != 0)
                {
                    command = GetCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _productrequestformitems.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("isactive", true, "=");

                    //if (!string.IsNullOrEmpty(_productrequestformitems.Code))
                    //    qb.AddParameter("code", _productrequestformitems.Code, "=");

                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Productrequestformitems oproductrequestformitems = new Productrequestformitems();
                        oproductrequestformitems.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        oproductrequestformitems.productid = reader["productid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productid"]);
                        oproductrequestformitems.supplierid = reader["supplierid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["supplierid"]);
                        oproductrequestformitems.itemid = reader["itemid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["itemid"]);
                        oproductrequestformitems.designid = reader["designid"] == DBNull.Value ? "" : reader["designid"].ToString();
                        oproductrequestformitems.description = reader["description"] == DBNull.Value ? "" : reader["description"].ToString();
                        oproductrequestformitems.pricerange = reader["pricerange"] == DBNull.Value ? "" : reader["pricerange"].ToString();
                        oproductrequestformitems.qty = reader["qty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["qty"]);
                        oproductrequestformitems.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        oproductrequestformitems.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        oproductrequestformitems.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        oproductrequestformitems.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        oproductrequestformitems.productrequestformid = reader["productrequestformid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productrequestformid"]);
                        oproductrequestformitems.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        oproductrequestformitems.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        lst.Add(oproductrequestformitems);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _productrequestformitems);
            }
            return lst;
        }

        internal List<Productrequestformitems> CancelOrders(List<Productrequestformitems> items)
        {
            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = GetCommand(SQL_CancelOrder, conn, transaction);
                    command.Parameters.Add("cqty", NpgsqlTypes.NpgsqlDbType.Numeric);
                    command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                    command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
                    command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint);
                    foreach (var item in items)
                    {
                        command.Parameters["cqty"].Value = item.cqty;
                        command.Parameters["version"].Value = item.version;
                        command.Parameters["id"].Value = item.id;
                        if(command.ExecuteNonQuery() != 1)
                        {
                            item.cqty = 0;
                        }
                    }
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, items);

                
                    
            }
            return items;
        }

        public List<Productrequestformitems> SearchAll(long _productrequestformid)
        {
            List<Productrequestformitems> lst = new List<Productrequestformitems>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = GetCommand(SQL_SearchAll, conn);
                command.Parameters.Add("productrequestformid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _productrequestformid;
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Productrequestformitems oproductrequestformitems = new Productrequestformitems();
                        oproductrequestformitems.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        oproductrequestformitems.productid = reader["productid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productid"]);
                        oproductrequestformitems.supplierid = reader["supplierid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["supplierid"]);
                        oproductrequestformitems.itemid = reader["itemid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["itemid"]);
                        oproductrequestformitems.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        oproductrequestformitems.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        oproductrequestformitems.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        oproductrequestformitems.designid = reader["designid"] == DBNull.Value ? "" : reader["designid"].ToString();
                        oproductrequestformitems.description = reader["description"] == DBNull.Value ? "" : reader["description"].ToString();
                        oproductrequestformitems.pricerange = reader["pricerange"] == DBNull.Value ? "" : reader["pricerange"].ToString();
                        oproductrequestformitems.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        oproductrequestformitems.productrequestformid = reader["productrequestformid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productrequestformid"]);
                        oproductrequestformitems.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        oproductrequestformitems.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        lst.Add(oproductrequestformitems);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _productrequestformid);
            }
            return lst;
        }

        public Productrequestformitems Insert(Productrequestformitems oProductrequestformitems)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);

                    Insert(transaction, conn, oProductrequestformitems);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oProductrequestformitems);
                
                    
            }

            return oProductrequestformitems;
        }

        public Productrequestformitems Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Productrequestformitems oProductrequestformitems)
        {

            NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("productid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oProductrequestformitems.productid;
            command.Parameters.Add("supplierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oProductrequestformitems.supplierid;
            command.Parameters.Add("itemid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oProductrequestformitems.itemid;
            command.Parameters.Add("qty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oProductrequestformitems.qty;
            command.Parameters.Add("designid", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oProductrequestformitems.designid == null ? "" : oProductrequestformitems.designid;
            command.Parameters.Add("description", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oProductrequestformitems.description == null ? "" : oProductrequestformitems.description;
            command.Parameters.Add("pricerange", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oProductrequestformitems.pricerange == null ? "" : oProductrequestformitems.pricerange;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oProductrequestformitems.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oProductrequestformitems.version;
            command.Parameters.Add("productrequestformid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oProductrequestformitems.productrequestformid;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oProductrequestformitems.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            return oProductrequestformitems;
        }

        public Productrequestformitems Update(Productrequestformitems oProductrequestformitems)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oProductrequestformitems);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oProductrequestformitems);
                
                    
            }

            return oProductrequestformitems;
        }

        public Productrequestformitems Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Productrequestformitems oProductrequestformitems)
        {

            NpgsqlCommand command = GetCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oProductrequestformitems.id;
            command.Parameters.Add("productid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oProductrequestformitems.productid;
            command.Parameters.Add("supplierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oProductrequestformitems.supplierid;
            command.Parameters.Add("itemid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oProductrequestformitems.itemid;
            command.Parameters.Add("qty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oProductrequestformitems.qty;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oProductrequestformitems.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oProductrequestformitems.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("description", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oProductrequestformitems.description == null ? "" : oProductrequestformitems.description;
            command.Parameters.Add("pricerange", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oProductrequestformitems.pricerange == null ? "" : oProductrequestformitems.pricerange;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oProductrequestformitems.version;
            command.Parameters.Add("productrequestformid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oProductrequestformitems.productrequestformid;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oProductrequestformitems.createdon;
            command.Parameters.Add("designid", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oProductrequestformitems.designid == null ? "" : oProductrequestformitems.designid;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oProductrequestformitems.version = Convert.ToInt32(reader[0].ToString());
                }
            }

            return oProductrequestformitems;
        }

        public bool Update(NpgsqlTransaction transaction, NpgsqlConnection conn, List<Productrequestformitems> list, long id)
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
                    item.productrequestformid = id;
                    Insert(transaction, conn, item);
                }
            }
            return true;
        }

        public bool Delete(Productrequestformitems oProductrequestformitems)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oProductrequestformitems);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oProductrequestformitems);

                
                    
            }
            return hasSuccessCount;

        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Productrequestformitems oProductrequestformitems)
        {

            NpgsqlCommand command = GetCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oProductrequestformitems.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oProductrequestformitems.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oProductrequestformitems.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

        public bool DeleteAll(long productrequestformid, long userid, NpgsqlConnection conn, NpgsqlTransaction transaction)
        {
            int successCount = 0;
            NpgsqlCommand command = GetCommand(SQL_DeleteAll, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = false;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("productrequestformid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = productrequestformid;
            successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

    }

}
