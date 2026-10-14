using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
namespace InfyPOS.Data

{
    public class LrinvoicetaxDataService : SAASData
    {
        const string SQL_Search = @"select invoiceid, taxid, amounton, percentage, netamount, createdby, modifiedby, createdon, modifiedon, version, isactive, id, discount,type, discountpercentage,fdiscountpercentage,taxamount,isinterstatesale from lrinvoicetax where id=@id";

        const string SQL_SearchAll = @"select invoiceid, taxid, amounton, percentage, netamount, createdby, modifiedby, createdon, modifiedon, version, isactive, id, discount,type, discountpercentage,fdiscountpercentage,taxamount,isinterstatesale from lrinvoicetax where invoiceid=@invoiceid and isactive=true";

        const string SQL_Insert = @"INSERT INTO public.lrinvoicetax(invoiceid, taxid, amounton, percentage, netamount, createdby, modifiedby, createdon, modifiedon, version, isactive, discount, discountpercentage,fdiscountpercentage,taxamount,isinterstatesale,type) 
                                    VALUES(@invoiceid, @taxid, @amounton, @percentage, @netamount, @createdby, @modifiedby, @createdon, @modifiedon, @version, @isactive, @discount, @discountpercentage,@fdiscountpercentage,@taxamount,@isinterstatesale,@type) RETURNING id";

        const string SQL_Update = @"UPDATE public.lrinvoicetax SET invoiceid=@invoiceid, taxid=@taxid,isinterstatesale=@isinterstatesale, amounton=@amounton, percentage=@percentage, netamount=@netamount,type=@type,taxamount=@taxamount, modifiedby=@modifiedby, 
                                    modifiedon=@modifiedon, version=@version+1, discount=@discount, discountpercentage=@discountpercentage,fdiscountpercentage=@fdiscountpercentage where id=@id and version=@version RETURNING version";

        const string SQL_Delete = @"update public.lrinvoicetax set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version RETURNING version";
        const string SQL_DeleteAll = @"update public.lrinvoicetax set isactive=false,modifiedby=@modifiedby,modifiedon=@modifiedon where invoiceid=@invoiceid";
        public List<Lrinvoicetax> Search(LrinvoicetaxCreteria _lrinvoicetax)
        {
            List<Lrinvoicetax> lst = new List<Lrinvoicetax>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_lrinvoicetax.id != 0)
                {
                    command = GetCommand(SQL_Search, conn);
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("isactive", true, "=");

                    //if (!string.IsNullOrEmpty(_lrinvoicetax.Code))
                    //    qb.AddParameter("code", _lrinvoicetax.Code, "=");

                    command = qb.GetCommand(conn);
                }
                command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _lrinvoicetax.id;
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Lrinvoicetax olrinvoicetax = new Lrinvoicetax();
                        olrinvoicetax.invoiceid = reader["invoiceid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["invoiceid"].ToString());
                        olrinvoicetax.taxid = reader["taxid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["taxid"].ToString());
                        olrinvoicetax.amounton = reader["amounton"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amounton"].ToString());
                        olrinvoicetax.netamount = reader["netamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["netamount"].ToString());
                        olrinvoicetax.taxamount = reader["taxamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["taxamount"].ToString());
                        olrinvoicetax.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        olrinvoicetax.type = reader["type"] == DBNull.Value ? Lrinvoicetax.EntryType.Taxable : (Lrinvoicetax.EntryType)Convert.ToInt32(reader["type"]);
                        olrinvoicetax.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        olrinvoicetax.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        olrinvoicetax.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        olrinvoicetax.isinterstatesale = reader["isinterstatesale"] == DBNull.Value ? false : Convert.ToBoolean(reader["isinterstatesale"]);
                        olrinvoicetax.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        olrinvoicetax.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"].ToString());
                        olrinvoicetax.percentage = reader["percentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["percentage"].ToString());
                        olrinvoicetax.discount = reader["discount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discount"].ToString());
                        if(reader["fdiscountpercentage"] != DBNull.Value)
                            olrinvoicetax.discountpercentage = reader["fdiscountpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["fdiscountpercentage"].ToString());
                        else
                            olrinvoicetax.discountpercentage = reader["discountpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discountpercentage"].ToString());
                        olrinvoicetax.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        lst.Add(olrinvoicetax);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _lrinvoicetax);
            }
            return lst;
        }

        public List<Lrinvoicetax> SearchAll(long _invoiceid)
        {
            List<Lrinvoicetax> lst = new List<Lrinvoicetax>();
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
                        Lrinvoicetax olrinvoicetax = new Lrinvoicetax();
                        olrinvoicetax.invoiceid = reader["invoiceid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["invoiceid"].ToString());
                        olrinvoicetax.type = reader["type"] == DBNull.Value ? Lrinvoicetax.EntryType.Taxable : (Lrinvoicetax.EntryType)Convert.ToInt32(reader["type"]);
                        olrinvoicetax.taxid = reader["taxid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["taxid"].ToString());
                        olrinvoicetax.amounton = reader["amounton"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amounton"].ToString());
                        olrinvoicetax.percentage = reader["percentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["percentage"].ToString());
                        olrinvoicetax.discount = reader["discount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discount"].ToString());
                        olrinvoicetax.discountpercentage = reader["discountpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discountpercentage"].ToString());
                        olrinvoicetax.taxamount = reader["taxamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["taxamount"].ToString());
                        olrinvoicetax.netamount = reader["netamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["netamount"].ToString());
                        olrinvoicetax.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        olrinvoicetax.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        olrinvoicetax.isinterstatesale = reader["isinterstatesale"] == DBNull.Value ? false : Convert.ToBoolean(reader["isinterstatesale"]);
                        olrinvoicetax.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        olrinvoicetax.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        olrinvoicetax.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        olrinvoicetax.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"].ToString());
                        olrinvoicetax.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        if (reader["fdiscountpercentage"] != DBNull.Value)
                            olrinvoicetax.discountpercentage = reader["fdiscountpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["fdiscountpercentage"].ToString());
                        else
                            olrinvoicetax.discountpercentage = reader["discountpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discountpercentage"].ToString());
                        lst.Add(olrinvoicetax);
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

        public bool InsertAll(List<Lrinvoicetax> oLrinvoicetaxs)
        {
            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);
                    foreach(var oLrinvoicetax in oLrinvoicetaxs)
                    Insert(transaction, conn, oLrinvoicetax);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oLrinvoicetaxs);
                return false;
            }
            return true;
        }

        public Lrinvoicetax Insert(Lrinvoicetax oLrinvoicetax)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);

                    Insert(transaction, conn, oLrinvoicetax);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oLrinvoicetax);
                
                    
            }

            return oLrinvoicetax;
        }

        public Lrinvoicetax Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Lrinvoicetax oLrinvoicetax)
        {

            NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("invoiceid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrinvoicetax.invoiceid;
            command.Parameters.Add("type", NpgsqlTypes.NpgsqlDbType.Integer).Value = oLrinvoicetax.type;
            command.Parameters.Add("taxid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrinvoicetax.taxid;
            command.Parameters.Add("amounton", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrinvoicetax.amounton;
            command.Parameters.Add("percentage", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrinvoicetax.percentage;
            command.Parameters.Add("discount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrinvoicetax.discount;
            command.Parameters.Add("taxamount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrinvoicetax.taxamount;
            command.Parameters.Add("discountpercentage", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrinvoicetax.discountpercentage;
            command.Parameters.Add("fdiscountpercentage", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrinvoicetax.discountpercentage;
            command.Parameters.Add("netamount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrinvoicetax.netamount;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oLrinvoicetax.version;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oLrinvoicetax.isactive;
            command.Parameters.Add("isinterstatesale", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oLrinvoicetax.isinterstatesale;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oLrinvoicetax.id = Convert.ToInt64(reader[0]);
                }
            }
            return oLrinvoicetax;
        }

        public Lrinvoicetax Update(Lrinvoicetax oLrinvoicetax)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oLrinvoicetax);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oLrinvoicetax);
                
                    
            }

            return oLrinvoicetax;
        }

        public Lrinvoicetax Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Lrinvoicetax oLrinvoicetax)
        {

            NpgsqlCommand command = GetCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("invoiceid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrinvoicetax.invoiceid;
            command.Parameters.Add("type", NpgsqlTypes.NpgsqlDbType.Integer).Value = oLrinvoicetax.type;
            command.Parameters.Add("taxid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrinvoicetax.taxid;
            command.Parameters.Add("amounton", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrinvoicetax.amounton;
            command.Parameters.Add("percentage", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrinvoicetax.percentage;
            command.Parameters.Add("discount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrinvoicetax.discount;
            command.Parameters.Add("discountpercentage", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrinvoicetax.discountpercentage;
            command.Parameters.Add("fdiscountpercentage", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrinvoicetax.discountpercentage;
            command.Parameters.Add("netamount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrinvoicetax.netamount;
            command.Parameters.Add("taxamount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrinvoicetax.taxamount;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrinvoicetax.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oLrinvoicetax.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oLrinvoicetax.version;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oLrinvoicetax.isactive;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrinvoicetax.id;
            command.Parameters.Add("isinterstatesale", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oLrinvoicetax.isinterstatesale;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oLrinvoicetax.version = Convert.ToInt32(reader[0]);
                }
            }

            return oLrinvoicetax;
        }

        public bool Update(NpgsqlTransaction transaction, NpgsqlConnection conn, List<Lrinvoicetax> list, long id)
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

        public bool Delete(Lrinvoicetax oLrinvoicetax)
        {

            NpgsqlTransaction transaction = null;
            int successCount = 0;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    Delete(transaction, conn, oLrinvoicetax);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oLrinvoicetax);

                
                    
            }
            return successCount > 0;

        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Lrinvoicetax oLrinvoicetax)
        {

            NpgsqlCommand command = GetCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oLrinvoicetax.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oLrinvoicetax.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrinvoicetax.id;
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
