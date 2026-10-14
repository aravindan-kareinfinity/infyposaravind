using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
namespace InfyPOS.Data

{
    public class BillsettlementDataService : SAASData
    {
        const string SQL_Search = @"select id, isactive, createdby, modifiedby, version, createdon, modifiedon, receiptdate, receiptno, paid, receivable, billno, companyid, billamount, discount, discountapprovedby, mode, billsettlementmasterid, customerid, billid, additionaldiscount, discountreason,billadditionaldiscount from billsettlement where id=@id";
        const string SQL_SearchAll = @"select id, isactive, createdby, modifiedby, version, createdon, modifiedon, receiptdate, receiptno, paid, receivable, billno, companyid, billamount, discount, discountapprovedby, mode, billsettlementmasterid, customerid, billid,billadditionaldiscount from billsettlement where billsettlementmasterid=@billsettlementmasterid";

        const string SQL_Settled = @"UPDATE public.bill set settled = true where id=@billid and settled = false and billtype!=9;";

        const string SQL_Insert = @"INSERT INTO public.billsettlement(isactive, createdby, modifiedby, version, createdon, modifiedon, receiptdate, receiptno, paid, receivable, billno, companyid, billamount, discount, discountapprovedby, mode, billsettlementmasterid, customerid, billid,additionaldiscount,discountreason,billadditionaldiscount) VALUES(@isactive, @createdby, @modifiedby, @version, @createdon, @modifiedon, @receiptdate, @receiptno, @paid, @receivable, @billno, @companyid, @billamount, @discount, @discountapprovedby, @mode, @billsettlementmasterid, @customerid, @billid, @additionaldiscount, @discountreason,@billadditionaldiscount) RETURNING id";

        const string SQL_Update = @"UPDATE public.billsettlement SET modifiedby=@modifiedby, version=@version+1,billadditionaldiscount=@billadditionaldiscount, modifiedon=@modifiedon, receiptdate=@receiptdate, receiptno=@receiptno, paid=@paid, receivable=@receivable, billno=@billno, companyid=@companyid, billamount=@billamount, discount=@discount, discountapprovedby=@discountapprovedby, mode=@mode, billsettlementmasterid=@billsettlementmasterid, customerid=@customerid, billid=@billid where id=@id and version=@version RETURNING version";

        const string SQL_Delete = @"update public.billsettlement set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";
        const string SQL_DeleteAll = @"update public.billsettlement set isactive=false,modifiedby=@modifiedby,modifiedon=@modifiedon,billno=billno||@billno where billsettlementmasterid=@billsettlementmasterid";
        public List<Billsettlement> Search(BillsettlementCreteria _billsettlement)
        {
            List<Billsettlement> lst = new List<Billsettlement>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_billsettlement.id != 0)
                {
                    command = GetCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _billsettlement.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("isactive", true, "=");
                    if(_billsettlement.billids != null && _billsettlement.billids.Count>0)
                        qb.AddInParamer("bill.id", string.Join<long>(",", _billsettlement.billids));

                    if (_billsettlement.billid>0)
                        qb.AddParameter("billid", _billsettlement.billid, "=");
                   
                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Billsettlement obillsettlement = new Billsettlement();
                        obillsettlement.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        obillsettlement.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        obillsettlement.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        obillsettlement.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        obillsettlement.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        obillsettlement.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        obillsettlement.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        obillsettlement.receiptdate = reader["receiptdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["receiptdate"]);
                        obillsettlement.receiptno = reader["receiptno"] == DBNull.Value ? "" : reader["receiptno"].ToString();
                        obillsettlement.paid = reader["paid"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["paid"]);
                        obillsettlement.receivable = reader["receivable"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["receivable"]);
                        obillsettlement.billno = reader["billno"] == DBNull.Value ? "" : reader["billno"].ToString();
                        obillsettlement.companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"]);
                        obillsettlement.billamount = reader["billamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["billamount"]);
                        obillsettlement.discount = reader["discount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discount"]);
                        obillsettlement.billadditionaldiscount = reader["billadditionaldiscount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["billadditionaldiscount"]);
                        obillsettlement.discountapprovedby = reader["discountapprovedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["discountapprovedby"]);
                        obillsettlement.mode = reader["mode"] == DBNull.Value ? 0 : Convert.ToInt32(reader["mode"]);
                        obillsettlement.billsettlementmasterid = reader["billsettlementmasterid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["billsettlementmasterid"]);
                        obillsettlement.customerid = reader["customerid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["customerid"]);
                        obillsettlement.billid = reader["billid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["billid"]);
                        obillsettlement.additionaldiscount = reader["additionaldiscount"] == DBNull.Value ? 0 : Convert.ToInt64(reader["additionaldiscount"]);
                        obillsettlement.discountreason = reader["discountreason"] == DBNull.Value ? "" : reader["discountreason"].ToString();
                        lst.Add(obillsettlement);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _billsettlement);
            }
            return lst;
        }

        public List<Billsettlement> SearchAll(long _billsettlementmasterid)
        {
            List<Billsettlement> lst = new List<Billsettlement>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = GetCommand(SQL_SearchAll, conn);
                command.Parameters.Add("billsettlementmasterid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _billsettlementmasterid;
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Billsettlement obillsettlement = new Billsettlement();
                        obillsettlement.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        obillsettlement.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        obillsettlement.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        obillsettlement.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        obillsettlement.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        obillsettlement.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        obillsettlement.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        obillsettlement.receiptdate = reader["receiptdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["receiptdate"]);
                        obillsettlement.receiptno = reader["receiptno"] == DBNull.Value ? "" : reader["receiptno"].ToString();
                        obillsettlement.billno = reader["billno"] == DBNull.Value ? "" : reader["billno"].ToString();
                        obillsettlement.companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"]);
                        obillsettlement.discountapprovedby = reader["discountapprovedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["discountapprovedby"]);
                        obillsettlement.mode = reader["mode"] == DBNull.Value ? 0 : Convert.ToInt32(reader["mode"]);
                        obillsettlement.billsettlementmasterid = reader["billsettlementmasterid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["billsettlementmasterid"]);
                        obillsettlement.customerid = reader["customerid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["customerid"]);
                        obillsettlement.billid = reader["billid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["billid"]);
                        obillsettlement.paid = reader["paid"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["paid"]);
                        obillsettlement.receivable = reader["receivable"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["receivable"]);
                        obillsettlement.discount = reader["discount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discount"]);
                        obillsettlement.billamount = reader["billamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["billamount"]);
                        obillsettlement.billadditionaldiscount = reader["billadditionaldiscount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["billadditionaldiscount"]);
                        obillsettlement.orderno = lst.Count + 1;
                        lst.Add(obillsettlement);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _billsettlementmasterid);
            }
            return lst;
        }

        public Billsettlement Insert(Billsettlement oBillsettlement)
        {


            NpgsqlTransaction transaction = null;
            try
            {

                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                        transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oBillsettlement,Context.UserID,oBillsettlement.locationid);
            }

            return oBillsettlement;
        }

        public Billsettlement Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Billsettlement oBillsettlement)
        {
            NpgsqlCommand command = GetCommand(SQL_Settled, conn, transaction);
            command.Parameters.Add("billid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillsettlement.billid;
            if (command.ExecuteNonQuery() > 0 || oBillsettlement.IsSynchronize)
            {
                command = GetCommand(SQL_Insert, conn, transaction);
                command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBillsettlement.isactive;
                command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBillsettlement.version;
                command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
                command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
                command.Parameters.Add("receiptdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBillsettlement.receiptdate;
                command.Parameters.Add("receiptno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBillsettlement.receiptno == null ? "" : oBillsettlement.receiptno;
                command.Parameters.Add("paid", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillsettlement.paid;
                command.Parameters.Add("receivable", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillsettlement.receivable;
                command.Parameters.Add("billno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBillsettlement.billno == null ? "" : oBillsettlement.billno;
                command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillsettlement.companyid;
                command.Parameters.Add("billamount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillsettlement.billamount;
                command.Parameters.Add("discount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillsettlement.discount;
                command.Parameters.Add("discountapprovedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillsettlement.discountapprovedby;
                command.Parameters.Add("mode", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBillsettlement.mode;
                command.Parameters.Add("billsettlementmasterid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillsettlement.billsettlementmasterid;
                command.Parameters.Add("customerid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillsettlement.customerid;
                command.Parameters.Add("billid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillsettlement.billid;
                command.Parameters.Add("billadditionaldiscount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillsettlement.billadditionaldiscount;
                command.Parameters.Add("additionaldiscount", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillsettlement.additionaldiscount;
                command.Parameters.Add("discountreason", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBillsettlement.discountreason == null ? "" : oBillsettlement.discountreason;
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        oBillsettlement.id = Convert.ToInt64(reader[0].ToString());
                    }
                }
                return oBillsettlement;
            }
            throw new Exception(oBillsettlement.billno + " either already Settled or Cancelled.");
        }

        public Billsettlement Update(Billsettlement oBillsettlement)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oBillsettlement);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oBillsettlement, Context.UserID, oBillsettlement.locationid);
                
                    
            }

            return oBillsettlement;
        }

        public Billsettlement Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Billsettlement oBillsettlement)
        {

            NpgsqlCommand command = GetCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillsettlement.id;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBillsettlement.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillsettlement.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBillsettlement.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBillsettlement.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("receiptdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBillsettlement.receiptdate;
            command.Parameters.Add("receiptno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBillsettlement.receiptno == null ? "" : oBillsettlement.receiptno;
            command.Parameters.Add("paid", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillsettlement.paid;
            command.Parameters.Add("receivable", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillsettlement.receivable;
            command.Parameters.Add("billno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBillsettlement.billno == null ? "" : oBillsettlement.billno;
            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillsettlement.companyid;
            command.Parameters.Add("billamount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillsettlement.billamount;
            command.Parameters.Add("discount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillsettlement.discount;
            command.Parameters.Add("discountapprovedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillsettlement.discountapprovedby;
            command.Parameters.Add("mode", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBillsettlement.mode;
            command.Parameters.Add("billsettlementmasterid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillsettlement.billsettlementmasterid;
            command.Parameters.Add("customerid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillsettlement.customerid;
            command.Parameters.Add("billadditionaldiscount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillsettlement.billadditionaldiscount;
            command.Parameters.Add("billid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillsettlement.billid;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oBillsettlement.version = Convert.ToInt32(reader[0].ToString());
                }
            }

            return oBillsettlement;
        }

        public bool Update(NpgsqlTransaction transaction, NpgsqlConnection conn, List<Billsettlement> list, long id)
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
                    item.billsettlementmasterid = id;
                    Insert(transaction, conn, item);
                }
            }
            return true;
        }

        public bool Delete(Billsettlement oBillsettlement)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oBillsettlement);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oBillsettlement, Context.UserID, oBillsettlement.locationid);

                
                    
            }
            return hasSuccessCount;

        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Billsettlement oBillsettlement)
        {

            NpgsqlCommand command = GetCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBillsettlement.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBillsettlement.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillsettlement.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

        public bool DeleteAll(long billsettlementmasterid, long userid, NpgsqlConnection conn, NpgsqlTransaction transaction)
        {


            int successCount = 0;
            NpgsqlCommand command = GetCommand(SQL_DeleteAll, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = false;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("billsettlementmasterid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = billsettlementmasterid;
            command.Parameters.Add("billno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = "/" + billsettlementmasterid.ToString();
            successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

    }

}
