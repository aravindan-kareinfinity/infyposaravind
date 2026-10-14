using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
namespace InfyPOS.Data
{
    public class TailoringitemsdeliveredDataService : BaseData
    {
        const string SQL_Search = @"select id, tailoringitemsid, deliveredqty, deliveredby,tailoringorderid, product, deliveredon, createdon, receivedby, receivedbycustomer, version, isactive, modifiedby, createdby, modifiedon from tailoringitemsdelivered where id=@id";
        const string SQL_SearchAll = @"select id, tailoringitemsid, deliveredqty, deliveredby,tailoringorderid,product, deliveredon, createdon, receivedby, receivedbycustomer, version, isactive, modifiedby, createdby, modifiedon from tailoringitemsdelivered where isactive=true and tailoringorderid=@tailoringorderid";
        const string SQL_Insert = @"
UPDATE Tailoringitems SET deliveredqty=deliveredqty+@deliveredqty WHERE id = @tailoringitemsid;
INSERT INTO public.tailoringitemsdelivered(tailoringitemsid, deliveredqty, deliveredby,tailoringorderid,product, deliveredon, createdon, receivedby, receivedbycustomer, version, isactive, modifiedby, createdby, modifiedon) VALUES(@tailoringitemsid, @deliveredqty, @deliveredby,@tailoringorderid,@product, @deliveredon, @createdon, @receivedby, @receivedbycustomer, @version, @isactive, @modifiedby, @createdby, @modifiedon) RETURNING id";

        const string SQL_Update = @"UPDATE public.tailoringitemsdelivered SET tailoringitemsid=@tailoringitemsid, deliveredqty=@deliveredqty, deliveredby=@deliveredby,tailoringorderid=@tailoringorderid,product=@product, deliveredon=@deliveredon, receivedby=@receivedby, receivedbycustomer=@receivedbycustomer, version=@version+1, modifiedby=@modifiedby, modifiedon=@modifiedon where id=@id and version=@version RETURNING version";

        const string SQL_Delete = @"
UPDATE Tailoringitems SET Tailoringitems.deliveredqty=Tailoringitems.deliveredqty-tailoringitemsdelivered.deliveredqty from tailoringitemsdelivered
WHERE tailoringitemsdelivered.id = @id and tailoringitemsdelivered.tailoringitemsid = Tailoringitems.id;

update public.tailoringitemsdelivered set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";
        const string SQL_DeleteAll = @"update public.tailoringitemsdelivered set isactive=false,modifiedby=@modifiedby,modifiedon=@modifiedon where tailoringitemsid=@tailoringitemsid";
        public List<Tailoringitemsdelivered> Search(TailoringitemsdeliveredCreteria _tailoringitemsdelivered)
        {
            List<Tailoringitemsdelivered> lst = new List<Tailoringitemsdelivered>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_tailoringitemsdelivered.id != 0)
                {
                    command = new NpgsqlCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _tailoringitemsdelivered.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("isactive", true, "=");

                    //if (!string.IsNullOrEmpty(_tailoringitemsdelivered.Code))
                    //    qb.AddParameter("code", _tailoringitemsdelivered.Code, "=");

                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Tailoringitemsdelivered otailoringitemsdelivered = new Tailoringitemsdelivered();
                        otailoringitemsdelivered.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        otailoringitemsdelivered.product = reader["product"] == DBNull.Value ? "" : Convert.ToString(reader["product"]);
                        otailoringitemsdelivered.tailoringitemsid = reader["tailoringitemsid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["tailoringitemsid"]);
                        otailoringitemsdelivered.deliveredqty = reader["deliveredqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["deliveredqty"]);
                        otailoringitemsdelivered.deliveredby = reader["deliveredby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["deliveredby"]);
                        otailoringitemsdelivered.tailoringorderid = reader["tailoringorderid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["tailoringorderid"]); 
                        otailoringitemsdelivered.deliveredon = reader["deliveredon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["deliveredon"]);
                        otailoringitemsdelivered.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        otailoringitemsdelivered.receivedby = reader["receivedby"] == DBNull.Value ? "" : reader["receivedby"].ToString();
                        otailoringitemsdelivered.receivedbycustomer = reader["receivedbycustomer"] == DBNull.Value ? false : Convert.ToBoolean(reader["receivedbycustomer"]);
                        otailoringitemsdelivered.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        otailoringitemsdelivered.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        otailoringitemsdelivered.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        otailoringitemsdelivered.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        otailoringitemsdelivered.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        lst.Add(otailoringitemsdelivered);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _tailoringitemsdelivered);
            }
            return lst;
        }

        public List<Tailoringitemsdelivered> SearchAll(long _tailoringorderid)
        {
            List<Tailoringitemsdelivered> lst = new List<Tailoringitemsdelivered>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = new NpgsqlCommand(SQL_SearchAll, conn);
                command.Parameters.Add("tailoringorderid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _tailoringorderid;
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Tailoringitemsdelivered otailoringitemsdelivered = new Tailoringitemsdelivered();
                        otailoringitemsdelivered.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        otailoringitemsdelivered.product = reader["product"] == DBNull.Value ? "" : Convert.ToString(reader["product"]);
                        otailoringitemsdelivered.tailoringitemsid = reader["tailoringitemsid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["tailoringitemsid"]);
                        otailoringitemsdelivered.tailoringorderid = reader["tailoringorderid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["tailoringorderid"]);
                        otailoringitemsdelivered.deliveredqty = reader["deliveredqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["deliveredqty"]);
                        otailoringitemsdelivered.deliveredby = reader["deliveredby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["deliveredby"]);
                        otailoringitemsdelivered.deliveredon = reader["deliveredon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["deliveredon"]);
                        otailoringitemsdelivered.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        otailoringitemsdelivered.receivedby = reader["receivedby"] == DBNull.Value ? "" : reader["receivedby"].ToString();
                        otailoringitemsdelivered.receivedbycustomer = reader["receivedbycustomer"] == DBNull.Value ? false : Convert.ToBoolean(reader["receivedbycustomer"]);
                        otailoringitemsdelivered.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        otailoringitemsdelivered.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        otailoringitemsdelivered.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        otailoringitemsdelivered.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        otailoringitemsdelivered.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        lst.Add(otailoringitemsdelivered);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _tailoringorderid);
            }
            return lst;
        }

        public Tailoringitemsdelivered Insert(Tailoringitemsdelivered oTailoringitemsdelivered)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);

                    Insert(transaction, conn, oTailoringitemsdelivered);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oTailoringitemsdelivered);

            }

            return oTailoringitemsdelivered;
        }

        public Tailoringitemsdelivered Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Tailoringitemsdelivered oTailoringitemsdelivered)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("tailoringitemsid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTailoringitemsdelivered.tailoringitemsid;
            command.Parameters.Add("tailoringorderid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTailoringitemsdelivered.tailoringorderid;
            command.Parameters.Add("deliveredqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oTailoringitemsdelivered.deliveredqty;
            command.Parameters.Add("deliveredby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTailoringitemsdelivered.deliveredby;
            command.Parameters.Add("deliveredon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oTailoringitemsdelivered.deliveredon;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oTailoringitemsdelivered.createdon;
            command.Parameters.Add("receivedby", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oTailoringitemsdelivered.receivedby == null ? "" : oTailoringitemsdelivered.receivedby;
            command.Parameters.Add("product", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oTailoringitemsdelivered.product == null ? "" : oTailoringitemsdelivered.product;
            command.Parameters.Add("receivedbycustomer", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oTailoringitemsdelivered.receivedbycustomer;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oTailoringitemsdelivered.version;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oTailoringitemsdelivered.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTailoringitemsdelivered.modifiedby;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTailoringitemsdelivered.createdby;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oTailoringitemsdelivered.modifiedon;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oTailoringitemsdelivered.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            return oTailoringitemsdelivered;
        }

        public Tailoringitemsdelivered Update(Tailoringitemsdelivered oTailoringitemsdelivered)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oTailoringitemsdelivered);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oTailoringitemsdelivered);

            }

            return oTailoringitemsdelivered;
        }

        public Tailoringitemsdelivered Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Tailoringitemsdelivered oTailoringitemsdelivered)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTailoringitemsdelivered.id;
            command.Parameters.Add("tailoringitemsid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTailoringitemsdelivered.tailoringitemsid;
            command.Parameters.Add("deliveredqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oTailoringitemsdelivered.deliveredqty;
            command.Parameters.Add("product", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oTailoringitemsdelivered.product == null ? "" : oTailoringitemsdelivered.product;
            command.Parameters.Add("deliveredby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTailoringitemsdelivered.deliveredby;
            command.Parameters.Add("deliveredon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oTailoringitemsdelivered.deliveredon;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oTailoringitemsdelivered.createdon;
            command.Parameters.Add("receivedby", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oTailoringitemsdelivered.receivedby == null ? "" : oTailoringitemsdelivered.receivedby;
            command.Parameters.Add("receivedbycustomer", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oTailoringitemsdelivered.receivedbycustomer;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oTailoringitemsdelivered.version;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oTailoringitemsdelivered.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTailoringitemsdelivered.modifiedby;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTailoringitemsdelivered.createdby;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oTailoringitemsdelivered.modifiedon;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oTailoringitemsdelivered.version = Convert.ToInt32(reader[0].ToString());
                }
            }

            return oTailoringitemsdelivered;
        }

        public bool Update(NpgsqlTransaction transaction, NpgsqlConnection conn, List<Tailoringitemsdelivered> list, long id)
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
                    item.tailoringitemsid = id;
                    Insert(transaction, conn, item);
                }
            }
            return true;
        }

        public bool Delete(Tailoringitemsdelivered oTailoringitemsdelivered)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oTailoringitemsdelivered);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oTailoringitemsdelivered);

            }
            return hasSuccessCount;

        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Tailoringitemsdelivered oTailoringitemsdelivered)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oTailoringitemsdelivered.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTailoringitemsdelivered.modifiedby;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oTailoringitemsdelivered.modifiedon;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oTailoringitemsdelivered.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTailoringitemsdelivered.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

        public bool DeleteAll(long tailoringitemsid, long userid, NpgsqlConnection conn, NpgsqlTransaction transaction)
        {


            int successCount = 0;
            NpgsqlCommand command = new NpgsqlCommand(SQL_DeleteAll, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = false;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = userid;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("tailoringitemsid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = tailoringitemsid;
            successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

    }

}
