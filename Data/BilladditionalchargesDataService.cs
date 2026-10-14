using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
namespace InfyPOS.Data

{
    public class BilladditionalchargesDataService : SAASData
    {
        const string SQL_Search = @"
select referencelist.additionalinfo->>'formula' hsncode,billadditionalcharges.id, billadditionalcharges.isactive, 
billadditionalcharges.createdby, billadditionalcharges.modifiedby, billadditionalcharges.version, billadditionalcharges.createdon,
billadditionalcharges.modifiedon, charge, tax, taxid, additionalchargestypeid, billid, amount, 
taxpercentage,billadditionalcharges.name,taxonly,chargetype from billadditionalcharges
left outer join referencelist on referencelist.id = additionalchargestypeid where billadditionalcharges.id=@id";

        const string SQL_SearchAll = @"select referencelist.additionalinfo->>'formula' hsncode,billadditionalcharges.id, billadditionalcharges.isactive, 
billadditionalcharges.createdby, billadditionalcharges.modifiedby, billadditionalcharges.version, billadditionalcharges.createdon,
billadditionalcharges.modifiedon, charge, tax, taxid, additionalchargestypeid, billid, amount, 
taxpercentage,billadditionalcharges.name,taxonly,chargetype from billadditionalcharges
left outer join referencelist on referencelist.id = additionalchargestypeid 
where billid=@billid and billadditionalcharges.isactive = true";

        const string SQL_Insert = @"INSERT INTO public.billadditionalcharges(isactive, createdby, modifiedby, version, createdon, modifiedon, charge, tax, taxid, additionalchargestypeid, billid, amount, taxpercentage,name,taxonly,chargetype) 
        VALUES(@isactive, @createdby, @modifiedby, @version, @createdon, @modifiedon, @charge, @tax, @taxid, @additionalchargestypeid, @billid, @amount, @taxpercentage,@name,@taxonly,@chargetype) RETURNING id";

        const string SQL_Update = @"UPDATE public.billadditionalcharges SET modifiedby=@modifiedby, version=@version+1, modifiedon=@modifiedon, charge=@charge, tax=@tax, taxid=@taxid, additionalchargestypeid=@additionalchargestypeid, 
        billid=@billid, amount=@amount, taxpercentage=@taxpercentage,taxonly=@taxonly,chargetype=@chargetype where id=@id and version=@version RETURNING version";

        const string SQL_Delete = @"update public.billadditionalcharges set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";
        const string SQL_DeleteAll = @"update public.billadditionalcharges set isactive=false,modifiedby=@modifiedby,modifiedon=@modifiedon where billid=@billid";
        public List<Billadditionalcharges> Search(BilladditionalchargesCreteria _billadditionalcharges)
        {
            List<Billadditionalcharges> lst = new List<Billadditionalcharges>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_billadditionalcharges.id != 0)
                {
                    command = new NpgsqlCommand(SQL_Search, conn);
                    command.Parameters.Add("billadditionalcharges.id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _billadditionalcharges.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("billadditionalcharges.isactive", true, "=");

                    //if (!string.IsNullOrEmpty(_billadditionalcharges.Code))
                    //    qb.AddParameter("code", _billadditionalcharges.Code, "=");

                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Billadditionalcharges obilladditionalcharges = new Billadditionalcharges();
                        obilladditionalcharges.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        obilladditionalcharges.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        obilladditionalcharges.taxonly = reader["taxonly"] == DBNull.Value ? false : Convert.ToBoolean(reader["taxonly"]);
                        obilladditionalcharges.chargetype = reader["chargetype"] == DBNull.Value ? "" : reader["chargetype"].ToString();
                        obilladditionalcharges.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        obilladditionalcharges.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        obilladditionalcharges.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        obilladditionalcharges.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        obilladditionalcharges.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        obilladditionalcharges.Charge = reader["charge"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["charge"]);
                        obilladditionalcharges.hsncode = reader["hsncode"] == DBNull.Value ? "" : reader["hsncode"].ToString();
                        obilladditionalcharges.Tax = reader["tax"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["tax"]);
                        obilladditionalcharges.taxid = reader["taxid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["taxid"]);
                        obilladditionalcharges.Name = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
                        obilladditionalcharges.additionalchargestypeid = reader["additionalchargestypeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["additionalchargestypeid"]);
                        obilladditionalcharges.billid = reader["billid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["billid"]);
                        obilladditionalcharges.Amount = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"]);
                        obilladditionalcharges.taxpercentage = reader["taxpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["taxpercentage"]);
                        lst.Add(obilladditionalcharges);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _billadditionalcharges);
            }
            return lst;
        }

        public List<Billadditionalcharges> SearchAll(long _billid)
        {
            List<Billadditionalcharges> lst = new List<Billadditionalcharges>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = new NpgsqlCommand(SQL_SearchAll, conn);
                command.Parameters.Add("billid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _billid;
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Billadditionalcharges obilladditionalcharges = new Billadditionalcharges();
                        obilladditionalcharges.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        obilladditionalcharges.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        obilladditionalcharges.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        obilladditionalcharges.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        obilladditionalcharges.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        obilladditionalcharges.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        obilladditionalcharges.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        obilladditionalcharges.Charge = reader["charge"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["charge"]);
                        obilladditionalcharges.Tax = reader["tax"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["tax"]);
                        obilladditionalcharges.taxid = reader["taxid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["taxid"]);
                        obilladditionalcharges.Name = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
                        obilladditionalcharges.hsncode = reader["hsncode"] == DBNull.Value ? "" : reader["hsncode"].ToString();
                        obilladditionalcharges.chargetype = reader["chargetype"] == DBNull.Value ? "" : reader["chargetype"].ToString();
                        obilladditionalcharges.additionalchargestypeid = reader["additionalchargestypeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["additionalchargestypeid"]);
                        obilladditionalcharges.billid = reader["billid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["billid"]);
                        obilladditionalcharges.Amount = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"]);
                        obilladditionalcharges.taxpercentage = reader["taxpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["taxpercentage"]);
                        obilladditionalcharges.taxonly = reader["taxonly"] == DBNull.Value ? false : Convert.ToBoolean(reader["taxonly"]);
                        lst.Add(obilladditionalcharges);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _billid);
            }
            return lst;
        }

        public Billadditionalcharges Insert(Billadditionalcharges oBilladditionalcharges)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);

                    Insert(transaction, conn, oBilladditionalcharges);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oBilladditionalcharges);
            }

            return oBilladditionalcharges;
        }

        public Billadditionalcharges Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Billadditionalcharges oBilladditionalcharges)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBilladditionalcharges.isactive;
            command.Parameters.Add("taxonly", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBilladditionalcharges.taxonly;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBilladditionalcharges.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBilladditionalcharges.modifiedby;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBilladditionalcharges.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBilladditionalcharges.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBilladditionalcharges.modifiedon;
            command.Parameters.Add("charge", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBilladditionalcharges.Charge;
            command.Parameters.Add("tax", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBilladditionalcharges.Tax;
            command.Parameters.Add("taxid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBilladditionalcharges.taxid;
            command.Parameters.Add("additionalchargestypeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBilladditionalcharges.additionalchargestypeid;
            command.Parameters.Add("billid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBilladditionalcharges.billid;
            command.Parameters.Add("amount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBilladditionalcharges.Amount;
            command.Parameters.Add("taxpercentage", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBilladditionalcharges.taxpercentage;
            command.Parameters.Add("name", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBilladditionalcharges.Name == null ? "" : oBilladditionalcharges.Name;
            command.Parameters.Add("chargetype", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBilladditionalcharges.chargetype == null ? "" : oBilladditionalcharges.chargetype;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oBilladditionalcharges.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            return oBilladditionalcharges;
        }

        public Billadditionalcharges Update(Billadditionalcharges oBilladditionalcharges)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oBilladditionalcharges);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oBilladditionalcharges);
            }

            return oBilladditionalcharges;
        }

        public Billadditionalcharges Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Billadditionalcharges oBilladditionalcharges)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBilladditionalcharges.id;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBilladditionalcharges.isactive;
            command.Parameters.Add("taxonly", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBilladditionalcharges.taxonly;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBilladditionalcharges.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBilladditionalcharges.modifiedby;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBilladditionalcharges.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBilladditionalcharges.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBilladditionalcharges.modifiedon;
            command.Parameters.Add("charge", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBilladditionalcharges.Charge;
            command.Parameters.Add("tax", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBilladditionalcharges.Tax;
            command.Parameters.Add("taxid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBilladditionalcharges.taxid;
            command.Parameters.Add("additionalchargestypeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBilladditionalcharges.additionalchargestypeid;
            command.Parameters.Add("billid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBilladditionalcharges.billid;
            command.Parameters.Add("amount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBilladditionalcharges.Amount;
            command.Parameters.Add("taxpercentage", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBilladditionalcharges.taxpercentage;
            command.Parameters.Add("name", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBilladditionalcharges.Name == null ? "" : oBilladditionalcharges.Name;
            command.Parameters.Add("chargetype", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBilladditionalcharges.chargetype == null ? "" : oBilladditionalcharges.chargetype;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oBilladditionalcharges.version = Convert.ToInt32(reader[0].ToString());
                }
            }

            return oBilladditionalcharges;
        }

        public bool Update(NpgsqlTransaction transaction, NpgsqlConnection conn, List<Billadditionalcharges> list, long id)
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
                    item.billid = id;
                    Insert(transaction, conn, item);
                }
            }
            return true;
        }

        public bool Delete(Billadditionalcharges oBilladditionalcharges)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oBilladditionalcharges);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oBilladditionalcharges);
            }
            return hasSuccessCount;

        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Billadditionalcharges oBilladditionalcharges)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBilladditionalcharges.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBilladditionalcharges.modifiedby;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBilladditionalcharges.modifiedon;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBilladditionalcharges.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBilladditionalcharges.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

        public bool DeleteAll(long billid, long userid, NpgsqlConnection conn, NpgsqlTransaction transaction)
        {


            int successCount = 0;
            NpgsqlCommand command = new NpgsqlCommand(SQL_DeleteAll, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = false;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = userid;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("billid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = billid;
            successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

    }

}
