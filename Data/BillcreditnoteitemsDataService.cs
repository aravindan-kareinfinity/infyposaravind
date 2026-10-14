using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;


namespace InfyPOS.Data

{
    public class BillcreditnoteitemsDataService : BaseData
    {
        const string SQL_Search = @"select billcreditnoteid, billno, billid,customerinvoiceid, taxpercentage, tax, taxid, amount, totalamount, adjustedtax, adjustedvalue, adjustedtotal, id, isactive, createdby, modifiedby, version, createdon, modifiedon,billdate from billcreditnoteitems where id=@id";
        const string SQL_SearchAll = @"select billcreditnoteid, billno, billid,customerinvoiceid, taxpercentage, tax, taxid, amount, totalamount, adjustedtax, adjustedvalue, adjustedtotal, id, isactive, createdby, modifiedby, version, createdon, modifiedon,billdate from billcreditnoteitems where billcreditnoteid=@billcreditnoteid";
        const string SQL_Insert = @"INSERT INTO public.billcreditnoteitems(billcreditnoteid, billno, billid,customerinvoiceid, taxpercentage, tax, taxid, amount, totalamount, adjustedtax, adjustedvalue, adjustedtotal, isactive, createdby, modifiedby, version, createdon, modifiedon,billdate) VALUES(@billcreditnoteid, @billno, @billid,@customerinvoiceid, @taxpercentage, @tax, @taxid, @amount, @totalamount, @adjustedtax, @adjustedvalue, @adjustedtotal, @isactive, @createdby, @modifiedby, @version, @createdon, @modifiedon,@billdate) RETURNING id";
        const string SQL_Update = @"UPDATE public.billcreditnoteitems SET billcreditnoteid=@billcreditnoteid, billno=@billno, billid=@billid,customerinvoiceid=@customerinvoiceid, taxpercentage=@taxpercentage, tax=@tax, taxid=@taxid, amount=@amount, totalamount=@totalamount, adjustedtax=@adjustedtax, adjustedvalue=@adjustedvalue, adjustedtotal=@adjustedtotal, modifiedby=@modifiedby, version=@version+1, modifiedon=@modifiedon,billdate=@billdate where id=@id and version=@version RETURNING version";

        const string SQL_Delete = @"update public.billcreditnoteitems set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";

        const string SQL_Delete_Invoice = @"update public.customerinvoice set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id";
        const string SQL_DeleteAll = @"update public.billcreditnoteitems set isactive=false,modifiedby=@modifiedby,modifiedon=@modifiedon where billcreditnoteid=@billcreditnoteid";
        public List<Billcreditnoteitems> Search(BillcreditnoteitemsCreteria _billcreditnoteitems)
        {
            List<Billcreditnoteitems> lst = new List<Billcreditnoteitems>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_billcreditnoteitems.id != 0)
                {
                    command = new NpgsqlCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _billcreditnoteitems.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("isactive", true, "=");

                    //if (!string.IsNullOrEmpty(_billcreditnoteitems.Code))
                    //    qb.AddParameter("code", _billcreditnoteitems.Code, "=");

                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Billcreditnoteitems obillcreditnoteitems = new Billcreditnoteitems();
                        obillcreditnoteitems.billcreditnoteid = reader["billcreditnoteid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["billcreditnoteid"]);
                        obillcreditnoteitems.billno = reader["billno"] == DBNull.Value ? "" : reader["billno"].ToString();
                        obillcreditnoteitems.billid = reader["billid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["billid"]);
                        obillcreditnoteitems.customerinvoiceid = reader["customerinvoiceid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["customerinvoiceid"]);
                        
                        obillcreditnoteitems.taxpercentage = reader["taxpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["taxpercentage"]);
                        obillcreditnoteitems.tax = reader["tax"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["tax"]);
                        obillcreditnoteitems.taxid = reader["taxid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["taxid"]);
                        obillcreditnoteitems.amount = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"]);
                        obillcreditnoteitems.totalamount = reader["totalamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["totalamount"]);
                        obillcreditnoteitems.adjustedtax = reader["adjustedtax"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["adjustedtax"]);
                        obillcreditnoteitems.adjustedvalue = reader["adjustedvalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["adjustedvalue"]);
                        obillcreditnoteitems.adjustedtotal = reader["adjustedtotal"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["adjustedtotal"]);
                        obillcreditnoteitems.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        obillcreditnoteitems.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        obillcreditnoteitems.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        obillcreditnoteitems.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        obillcreditnoteitems.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        obillcreditnoteitems.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        obillcreditnoteitems.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        obillcreditnoteitems.billdate = reader["billdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["billdate"]);
                        lst.Add(obillcreditnoteitems);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _billcreditnoteitems);
            }
            return lst;
        }

        public List<Billcreditnoteitems> SearchAll(long _billcreditnoteid)
        {
            List<Billcreditnoteitems> lst = new List<Billcreditnoteitems>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = new NpgsqlCommand(SQL_SearchAll, conn);
                command.Parameters.Add("billcreditnoteid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _billcreditnoteid;
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Billcreditnoteitems obillcreditnoteitems = new Billcreditnoteitems();
                        obillcreditnoteitems.billcreditnoteid = reader["billcreditnoteid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["billcreditnoteid"]);
                        obillcreditnoteitems.billno = reader["billno"] == DBNull.Value ? "" : reader["billno"].ToString();
                        obillcreditnoteitems.billid = reader["billid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["billid"]);
                        obillcreditnoteitems.customerinvoiceid = reader["customerinvoiceid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["customerinvoiceid"]);
                        obillcreditnoteitems.taxpercentage = reader["taxpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["taxpercentage"]);
                        obillcreditnoteitems.tax = reader["tax"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["tax"]);
                        obillcreditnoteitems.taxid = reader["taxid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["taxid"]);
                        obillcreditnoteitems.amount = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"]);
                        obillcreditnoteitems.totalamount = reader["totalamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["totalamount"]);
                        obillcreditnoteitems.adjustedtax = reader["adjustedtax"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["adjustedtax"]);
                        obillcreditnoteitems.adjustedvalue = reader["adjustedvalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["adjustedvalue"]);
                        obillcreditnoteitems.adjustedtotal = reader["adjustedtotal"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["adjustedtotal"]);
                        obillcreditnoteitems.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        obillcreditnoteitems.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        obillcreditnoteitems.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        obillcreditnoteitems.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        obillcreditnoteitems.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        obillcreditnoteitems.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        obillcreditnoteitems.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        obillcreditnoteitems.billdate = reader["billdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["billdate"]);
                        lst.Add(obillcreditnoteitems);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _billcreditnoteid);
            }
            return lst;
        }

        public Billcreditnoteitems Insert(Billcreditnote billcreditnote, Billcreditnoteitems oBillcreditnoteitems)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);

                    Insert(transaction, conn, billcreditnote, oBillcreditnoteitems);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oBillcreditnoteitems);
            }

            return oBillcreditnoteitems;
        }

        public Billcreditnoteitems Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Billcreditnote billcreditnote, Billcreditnoteitems oBillcreditnoteitems)
        {

            CustomerinvoiceDataService customerdata = new CustomerinvoiceDataService() { Context = this.Context };
            oBillcreditnoteitems.customerinvoiceid = customerdata.Insert(transaction, conn, new Customerinvoice
            {
                isactive = true,
                createdby = Context.UserID,
                createdon = DateTimeNow,
                version = 1,
                type = 6,
                receivable = oBillcreditnoteitems.adjustedtotal*-1,
                transactiondate = oBillcreditnoteitems.billdate,
                billid = oBillcreditnoteitems.billid,
                customerid = billcreditnote.customerid,
                billno = oBillcreditnoteitems.billno,
                companyid = billcreditnote.companyid,
            }).id;

            NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("customerinvoiceid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillcreditnoteitems.customerinvoiceid;
            command.Parameters.Add("billcreditnoteid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillcreditnoteitems.billcreditnoteid;
            command.Parameters.Add("billno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBillcreditnoteitems.billno == null ? "" : oBillcreditnoteitems.billno;
            command.Parameters.Add("billid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillcreditnoteitems.billid;
            command.Parameters.Add("taxpercentage", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillcreditnoteitems.taxpercentage;
            command.Parameters.Add("tax", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillcreditnoteitems.tax;
            command.Parameters.Add("taxid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillcreditnoteitems.taxid;
            command.Parameters.Add("amount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillcreditnoteitems.amount;
            command.Parameters.Add("totalamount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillcreditnoteitems.totalamount;
            command.Parameters.Add("adjustedtax", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillcreditnoteitems.adjustedtax;
            command.Parameters.Add("adjustedvalue", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillcreditnoteitems.adjustedvalue;
            command.Parameters.Add("adjustedtotal", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillcreditnoteitems.adjustedtotal;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBillcreditnoteitems.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillcreditnoteitems.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillcreditnoteitems.modifiedby;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBillcreditnoteitems.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBillcreditnoteitems.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBillcreditnoteitems.modifiedon;
            command.Parameters.Add("billdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBillcreditnoteitems.billdate;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oBillcreditnoteitems.id = Convert.ToInt64(reader[0].ToString());
                }
            }

            return oBillcreditnoteitems;
        }

        public Billcreditnoteitems Update(Billcreditnoteitems oBillcreditnoteitems)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oBillcreditnoteitems);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oBillcreditnoteitems);

            }

            return oBillcreditnoteitems;
        }

        public Billcreditnoteitems Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Billcreditnoteitems oBillcreditnoteitems)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("customerinvoiceid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillcreditnoteitems.customerinvoiceid;
            command.Parameters.Add("billcreditnoteid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillcreditnoteitems.billcreditnoteid;
            command.Parameters.Add("billno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBillcreditnoteitems.billno == null ? "" : oBillcreditnoteitems.billno;
            command.Parameters.Add("billid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillcreditnoteitems.billid;
            command.Parameters.Add("taxpercentage", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillcreditnoteitems.taxpercentage;
            command.Parameters.Add("tax", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillcreditnoteitems.tax;
            command.Parameters.Add("taxid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillcreditnoteitems.taxid;
            command.Parameters.Add("amount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillcreditnoteitems.amount;
            command.Parameters.Add("totalamount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillcreditnoteitems.totalamount;
            command.Parameters.Add("adjustedtax", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillcreditnoteitems.adjustedtax;
            command.Parameters.Add("adjustedvalue", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillcreditnoteitems.adjustedvalue;
            command.Parameters.Add("adjustedtotal", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillcreditnoteitems.adjustedtotal;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillcreditnoteitems.id;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBillcreditnoteitems.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillcreditnoteitems.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillcreditnoteitems.modifiedby;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBillcreditnoteitems.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBillcreditnoteitems.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBillcreditnoteitems.modifiedon;
            command.Parameters.Add("billdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBillcreditnoteitems.billdate;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oBillcreditnoteitems.version = Convert.ToInt32(reader[0].ToString());
                }
            }

            return oBillcreditnoteitems;
        }

        public bool Update(NpgsqlTransaction transaction, NpgsqlConnection conn,Billcreditnote billcreditnote, List<Billcreditnoteitems> list, long id)
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
                    item.billcreditnoteid = id;
                    Insert(transaction, conn, billcreditnote,item);
                }
            }
            return true;
        }

        public bool Delete(Billcreditnoteitems oBillcreditnoteitems)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oBillcreditnoteitems);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oBillcreditnoteitems);


            }
            return hasSuccessCount;

        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Billcreditnoteitems oBillcreditnoteitems)
        {
            NpgsqlCommand command = new NpgsqlCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBillcreditnoteitems.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillcreditnoteitems.modifiedby;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBillcreditnoteitems.modifiedon;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBillcreditnoteitems.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillcreditnoteitems.id;
            int successCount = command.ExecuteNonQuery();

            command = new NpgsqlCommand(SQL_Delete_Invoice, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBillcreditnoteitems.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillcreditnoteitems.modifiedby;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBillcreditnoteitems.modifiedon;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBillcreditnoteitems.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillcreditnoteitems.customerinvoiceid;
            successCount = command.ExecuteNonQuery();
            return successCount > 0;
        }

        public bool DeleteAll(long billcreditnoteid, long userid, NpgsqlConnection conn, NpgsqlTransaction transaction)
        {


            int successCount = 0;
            NpgsqlCommand command = new NpgsqlCommand(SQL_DeleteAll, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = false;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = userid;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("billcreditnoteid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = billcreditnoteid;
            successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

    }

}
