using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
namespace InfyPOS.Data

{
    public class BOMinvoicetaxDataService : SAASData
    {
        const string SQL_Search = @"select invoiceid, taxid, amounton, percentage, netamount, createdby, modifiedby, createdon, modifiedon, version, isactive, id, discount,type, discountpercentage,taxamount,isinterstatesale from BOMinvoicetax where id=@id";

        const string SQL_SearchAll = @"select invoiceid, taxid, amounton, percentage, netamount, createdby, modifiedby, createdon, modifiedon, version, isactive, id, discount,type, discountpercentage,taxamount,isinterstatesale from BOMinvoicetax where invoiceid=@invoiceid and isactive=true";

        const string SQL_Insert = @"INSERT INTO public.BOMinvoicetax(invoiceid, taxid, amounton, percentage, netamount, createdby, modifiedby, createdon, modifiedon, version, isactive, discount, discountpercentage,taxamount,isinterstatesale,type) 
                                    VALUES(@invoiceid, @taxid, @amounton, @percentage, @netamount, @createdby, @modifiedby, @createdon, @modifiedon, @version, @isactive, @discount, @discountpercentage,@taxamount,@isinterstatesale,@type) RETURNING id";

        const string SQL_Update = @"UPDATE public.BOMinvoicetax SET invoiceid=@invoiceid, taxid=@taxid,isinterstatesale=@isinterstatesale, amounton=@amounton, percentage=@percentage, netamount=@netamount,type=@type,taxamount=@taxamount, modifiedby=@modifiedby, 
                                    modifiedon=@modifiedon, version=@version+1, discount=@discount, discountpercentage=@discountpercentage where id=@id and version=@version RETURNING version";

        const string SQL_Delete = @"update public.BOMinvoicetax set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version RETURNING version";
        const string SQL_DeleteAll = @"update public.BOMinvoicetax set isactive=false,modifiedby=@modifiedby,modifiedon=@modifiedon where invoiceid=@invoiceid";
        public List<BOMinvoicetax> Search(BOMinvoicetaxCreteria _BOMinvoicetax)
        {
            List<BOMinvoicetax> lst = new List<BOMinvoicetax>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_BOMinvoicetax.id != 0)
                {
                    command = GetCommand(SQL_Search, conn);
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("isactive", true, "=");

                    //if (!string.IsNullOrEmpty(_BOMinvoicetax.Code))
                    //    qb.AddParameter("code", _BOMinvoicetax.Code, "=");

                    command = qb.GetCommand(conn);
                }
                command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _BOMinvoicetax.id;
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        BOMinvoicetax oBOMinvoicetax = new BOMinvoicetax();
                        oBOMinvoicetax.invoiceid = reader["invoiceid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["invoiceid"].ToString());
                        oBOMinvoicetax.taxid = reader["taxid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["taxid"].ToString());
                        oBOMinvoicetax.amounton = reader["amounton"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amounton"].ToString());
                        oBOMinvoicetax.netamount = reader["netamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["netamount"].ToString());
                        oBOMinvoicetax.taxamount = reader["taxamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["taxamount"].ToString());
                        oBOMinvoicetax.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        oBOMinvoicetax.type = reader["type"] == DBNull.Value ? BOMinvoicetax.EntryType.Taxable : (BOMinvoicetax.EntryType)Convert.ToInt32(reader["type"]);
                        oBOMinvoicetax.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        oBOMinvoicetax.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        oBOMinvoicetax.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        oBOMinvoicetax.isinterstatesale = reader["isinterstatesale"] == DBNull.Value ? false : Convert.ToBoolean(reader["isinterstatesale"]);
                        oBOMinvoicetax.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        oBOMinvoicetax.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"].ToString());
                        oBOMinvoicetax.percentage = reader["percentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["percentage"].ToString());
                        oBOMinvoicetax.discount = reader["discount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discount"].ToString());
                        oBOMinvoicetax.discountpercentage = reader["discountpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discountpercentage"].ToString());
                        oBOMinvoicetax.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        lst.Add(oBOMinvoicetax);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _BOMinvoicetax);
            }
            return lst;
        }

        public List<BOMinvoicetax> SearchAll(long _invoiceid)
        {
            List<BOMinvoicetax> lst = new List<BOMinvoicetax>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = GetCommand(SQL_SearchAll, conn);
                command.Parameters.Add("invoiceid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _invoiceid;
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        BOMinvoicetax oBOMinvoicetax = new BOMinvoicetax();
                        oBOMinvoicetax.invoiceid = reader["invoiceid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["invoiceid"].ToString());
                        oBOMinvoicetax.type = reader["type"] == DBNull.Value ? BOMinvoicetax.EntryType.Taxable : (BOMinvoicetax.EntryType)Convert.ToInt32(reader["type"]);
                        oBOMinvoicetax.taxid = reader["taxid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["taxid"].ToString());
                        oBOMinvoicetax.amounton = reader["amounton"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amounton"].ToString());
                        oBOMinvoicetax.percentage = reader["percentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["percentage"].ToString());
                        oBOMinvoicetax.discount = reader["discount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discount"].ToString());
                        oBOMinvoicetax.discountpercentage = reader["discountpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discountpercentage"].ToString());
                        oBOMinvoicetax.taxamount = reader["taxamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["taxamount"].ToString());
                        oBOMinvoicetax.netamount = reader["netamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["netamount"].ToString());
                        oBOMinvoicetax.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        oBOMinvoicetax.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        oBOMinvoicetax.isinterstatesale = reader["isinterstatesale"] == DBNull.Value ? false : Convert.ToBoolean(reader["isinterstatesale"]);
                        oBOMinvoicetax.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        oBOMinvoicetax.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        oBOMinvoicetax.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        oBOMinvoicetax.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"].ToString());
                        oBOMinvoicetax.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        lst.Add(oBOMinvoicetax);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _invoiceid);
            }
            return lst;
        }

        public BOMinvoicetax Insert(BOMinvoicetax oBOMinvoicetax)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);

                    Insert(transaction, conn, oBOMinvoicetax);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oBOMinvoicetax);
                
                    
            }

            return oBOMinvoicetax;
        }

        public BOMinvoicetax Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, BOMinvoicetax oBOMinvoicetax)
        {

            NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("invoiceid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBOMinvoicetax.invoiceid;
            command.Parameters.Add("type", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBOMinvoicetax.type;
            command.Parameters.Add("taxid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBOMinvoicetax.taxid;
            command.Parameters.Add("amounton", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBOMinvoicetax.amounton;
            command.Parameters.Add("percentage", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBOMinvoicetax.percentage;
            command.Parameters.Add("discount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBOMinvoicetax.discount;
            command.Parameters.Add("taxamount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBOMinvoicetax.taxamount;
            command.Parameters.Add("discountpercentage", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBOMinvoicetax.discountpercentage;
            command.Parameters.Add("netamount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBOMinvoicetax.netamount;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBOMinvoicetax.version;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBOMinvoicetax.isactive;
            command.Parameters.Add("isinterstatesale", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBOMinvoicetax.isinterstatesale;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oBOMinvoicetax.id = Convert.ToInt64(reader[0]);
                }
            }
            return oBOMinvoicetax;
        }

        public BOMinvoicetax Update(BOMinvoicetax oBOMinvoicetax)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oBOMinvoicetax);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oBOMinvoicetax);
                
                    
            }

            return oBOMinvoicetax;
        }

        public BOMinvoicetax Update(NpgsqlTransaction transaction, NpgsqlConnection conn, BOMinvoicetax oBOMinvoicetax)
        {

            NpgsqlCommand command = GetCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("invoiceid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBOMinvoicetax.invoiceid;
            command.Parameters.Add("type", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBOMinvoicetax.type;
            command.Parameters.Add("taxid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBOMinvoicetax.taxid;
            command.Parameters.Add("amounton", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBOMinvoicetax.amounton;
            command.Parameters.Add("percentage", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBOMinvoicetax.percentage;
            command.Parameters.Add("discount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBOMinvoicetax.discount;
            command.Parameters.Add("discountpercentage", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBOMinvoicetax.discountpercentage;
            command.Parameters.Add("netamount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBOMinvoicetax.netamount;
            command.Parameters.Add("taxamount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBOMinvoicetax.taxamount;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBOMinvoicetax.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBOMinvoicetax.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBOMinvoicetax.version;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBOMinvoicetax.isactive;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBOMinvoicetax.id;
            command.Parameters.Add("isinterstatesale", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBOMinvoicetax.isinterstatesale;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oBOMinvoicetax.version = Convert.ToInt32(reader[0]);
                }
            }

            return oBOMinvoicetax;
        }

        public bool Update(NpgsqlTransaction transaction, NpgsqlConnection conn, List<BOMinvoicetax> list, long id)
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
                    item.invoiceid = id;
                    Insert(transaction, conn, item);
                }
            }
            return true;
        }

        public bool Delete(BOMinvoicetax oBOMinvoicetax)
        {

            NpgsqlTransaction transaction = null;
            int successCount = 0;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    Delete(transaction, conn, oBOMinvoicetax);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oBOMinvoicetax);

                
                    
            }
            return successCount > 0;

        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, BOMinvoicetax oBOMinvoicetax)
        {

            NpgsqlCommand command = GetCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBOMinvoicetax.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBOMinvoicetax.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBOMinvoicetax.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

        public bool DeleteAll(long invoiceid, long userid, NpgsqlConnection conn, NpgsqlTransaction transaction)
        {


            int successCount = 0;
            NpgsqlCommand command = GetCommand(SQL_DeleteAll, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = false;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("invoiceid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = invoiceid;
            successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

    }

}
