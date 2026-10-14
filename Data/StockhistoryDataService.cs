using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
namespace InfyPOS.Data

{
    public class StockhistoryDataService : SAASData
    {
        const string SQL_Search = @"select id, isactive, createdby, modifiedby, version, createdon, modifiedon, printingname, price, barcode, invoiceitemid, itemid, productid, discount, additionaldiscount, qty, transfered, damagedqty,journalqty, returnqty, auditqty, designid, buyingprice, hsncode, isbit, locationid, stockid, referenceid, actiontype, reason from stockhistory where id=@id";


        const string SQL_Insert = @"INSERT INTO public.stockhistory(isactive, createdby, modifiedby, version, createdon, modifiedon, printingname, price, barcode, invoiceitemid, itemid, productid, discount, additionaldiscount, qty, transfered, damagedqty,journalqty, returnqty, auditqty, designid, buyingprice, hsncode, isbit, locationid, stockid, referenceid, actiontype, reason) VALUES(@isactive, @createdby, @modifiedby, @version, @createdon, @modifiedon, @printingname, @price, @barcode, @invoiceitemid, @itemid, @productid, @discount, @additionaldiscount, @qty, @transfered, @damagedqty,@journalqty, @returnqty, @auditqty, @designid, @buyingprice, @hsncode, @isbit, @locationid, @stockid, @referenceid, @actiontype, @reason) RETURNING id";


        const string SQL_Update = @"UPDATE public.stockhistory SET modifiedby=@modifiedby, version=@version+1, modifiedon=@modifiedon, printingname=@printingname, price=@price, barcode=@barcode, invoiceitemid=@invoiceitemid, itemid=@itemid, productid=@productid, discount=@discount, additionaldiscount=@additionaldiscount, qty=@qty, transfered=@transfered, damagedqty=@damagedqty,journalqty=@journalqty, returnqty=@returnqty, auditqty=@auditqty, designid=@designid, buyingprice=@buyingprice, hsncode=@hsncode, isbit=@isbit, locationid=@locationid, stockid=@stockid, referenceid=@referenceid, actiontype=@actiontype where id=@id and version=@version RETURNING version";

        const string SQL_Delete = @"update public.stockhistory set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";

        public List<Stockhistory> Search(StockhistoryCreteria _stockhistory)
        {
            List<Stockhistory> lst = new List<Stockhistory>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_stockhistory.id != 0)
                {
                    command = GetCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _stockhistory.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("isactive", true, "=");

                    if (_stockhistory.stockid > 0)
                        qb.AddParameter("stockid", _stockhistory.stockid, "=");
                    if (string.IsNullOrEmpty(_stockhistory.barcode))
                        qb.AddParameter("barcode", _stockhistory.barcode, "=");
                    if (_stockhistory.actiontype > 0)
                        qb.AddParameter("actiontype", _stockhistory.actiontype, "=");

                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Stockhistory ostockhistory = new Stockhistory();
                        ostockhistory.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        ostockhistory.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        ostockhistory.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        ostockhistory.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        ostockhistory.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        ostockhistory.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        ostockhistory.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        ostockhistory.printingname = reader["printingname"] == DBNull.Value ? "" : reader["printingname"].ToString();
                        ostockhistory.price = reader["price"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["price"]);
                        ostockhistory.barcode = reader["barcode"] == DBNull.Value ? "" : reader["barcode"].ToString();
                        ostockhistory.invoiceitemid = reader["invoiceitemid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["invoiceitemid"]);
                        ostockhistory.itemid = reader["itemid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["itemid"]);
                        ostockhistory.productid = reader["productid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productid"]);
                        ostockhistory.discount = reader["discount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discount"]);
                        ostockhistory.additionaldiscount = reader["additionaldiscount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["additionaldiscount"]);
                        ostockhistory.qty = reader["qty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["qty"]);
                        ostockhistory.transfered = reader["transfered"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["transfered"]);
                        ostockhistory.damagedqty = reader["damagedqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["damagedqty"]);
                        ostockhistory.journalqty = reader["journalqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["journalqty"]);
                        ostockhistory.returnqty = reader["returnqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["returnqty"]);
                        ostockhistory.auditqty = reader["auditqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["auditqty"]);
                        ostockhistory.designid = reader["designid"] == DBNull.Value ? "" : reader["designid"].ToString();
                        ostockhistory.buyingprice = reader["buyingprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["buyingprice"]);
                        ostockhistory.hsncode = reader["hsncode"] == DBNull.Value ? "" : reader["hsncode"].ToString();
                        ostockhistory.isbit = reader["isbit"] == DBNull.Value ? false : Convert.ToBoolean(reader["isbit"]);
                        ostockhistory.locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                        ostockhistory.stockid = reader["stockid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["stockid"]);
                        ostockhistory.referenceid = reader["referenceid"] == DBNull.Value ? "" : reader["referenceid"].ToString();
                        ostockhistory.actiontype = reader["actiontype"] == DBNull.Value ? 0 : (Stockhistory.ActionType)Convert.ToInt32(reader["actiontype"]);
                        ostockhistory.reason = reader["reason"] == DBNull.Value ? "" : reader["reason"].ToString();
                        lst.Add(ostockhistory);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _stockhistory);
            }
            return lst;
        }

        public Stockhistory Insert(Stockhistory oStockhistory)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);

                    Insert(transaction, conn, oStockhistory);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oStockhistory);
                throw exp;
            }

            return oStockhistory;
        }

        public Stockhistory Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Stockhistory oStockhistory)
        {

            NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oStockhistory.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oStockhistory.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("printingname", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oStockhistory.printingname == null ? "" : oStockhistory.printingname;
            command.Parameters.Add("price", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oStockhistory.price;
            command.Parameters.Add("barcode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oStockhistory.barcode == null ? "" : oStockhistory.barcode;
            command.Parameters.Add("invoiceitemid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oStockhistory.invoiceitemid;
            command.Parameters.Add("itemid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oStockhistory.itemid;
            command.Parameters.Add("productid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oStockhistory.productid;
            command.Parameters.Add("discount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oStockhistory.discount;
            command.Parameters.Add("additionaldiscount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oStockhistory.additionaldiscount;
            command.Parameters.Add("qty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oStockhistory.qty;
            command.Parameters.Add("transfered", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oStockhistory.transfered;
            command.Parameters.Add("damagedqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oStockhistory.damagedqty;
            command.Parameters.Add("journalqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oStockhistory.journalqty;
            command.Parameters.Add("returnqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oStockhistory.returnqty;
            command.Parameters.Add("auditqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oStockhistory.auditqty;
            command.Parameters.Add("designid", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oStockhistory.designid == null ? "" : oStockhistory.designid;
            command.Parameters.Add("buyingprice", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oStockhistory.buyingprice;
            command.Parameters.Add("hsncode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oStockhistory.hsncode == null ? "" : oStockhistory.hsncode;
            command.Parameters.Add("isbit", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oStockhistory.isbit;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oStockhistory.locationid;
            command.Parameters.Add("stockid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oStockhistory.stockid;
            command.Parameters.Add("referenceid", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oStockhistory.referenceid == null ? "" : oStockhistory.referenceid;
            command.Parameters.Add("actiontype", NpgsqlTypes.NpgsqlDbType.Integer).Value = oStockhistory.actiontype;
            command.Parameters.Add("reason", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oStockhistory.reason == null ? "" : oStockhistory.reason;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oStockhistory.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            return oStockhistory;
        }

        public Stockhistory Update(Stockhistory oStockhistory)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oStockhistory);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oStockhistory);
                throw exp;
            }

            return oStockhistory;
        }

        public Stockhistory Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Stockhistory oStockhistory)
        {

            NpgsqlCommand command = GetCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oStockhistory.id;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oStockhistory.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oStockhistory.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oStockhistory.modifiedby;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oStockhistory.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oStockhistory.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oStockhistory.modifiedon;
            command.Parameters.Add("printingname", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oStockhistory.printingname == null ? "" : oStockhistory.printingname;
            command.Parameters.Add("price", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oStockhistory.price;
            command.Parameters.Add("barcode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oStockhistory.barcode == null ? "" : oStockhistory.barcode;
            command.Parameters.Add("invoiceitemid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oStockhistory.invoiceitemid;
            command.Parameters.Add("itemid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oStockhistory.itemid;
            command.Parameters.Add("productid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oStockhistory.productid;
            command.Parameters.Add("discount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oStockhistory.discount;
            command.Parameters.Add("additionaldiscount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oStockhistory.additionaldiscount;
            command.Parameters.Add("qty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oStockhistory.qty;
            command.Parameters.Add("transfered", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oStockhistory.transfered;
            command.Parameters.Add("damagedqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oStockhistory.damagedqty;
            command.Parameters.Add("journalqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oStockhistory.journalqty;
            command.Parameters.Add("returnqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oStockhistory.returnqty;
            command.Parameters.Add("auditqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oStockhistory.auditqty;
            command.Parameters.Add("designid", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oStockhistory.designid == null ? "" : oStockhistory.designid;
            command.Parameters.Add("buyingprice", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oStockhistory.buyingprice;
            command.Parameters.Add("hsncode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oStockhistory.hsncode == null ? "" : oStockhistory.hsncode;
            command.Parameters.Add("isbit", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oStockhistory.isbit;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oStockhistory.locationid;
            command.Parameters.Add("stockid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oStockhistory.stockid;
            command.Parameters.Add("referenceid", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oStockhistory.referenceid == null ? "" : oStockhistory.referenceid;
            command.Parameters.Add("actiontype", NpgsqlTypes.NpgsqlDbType.Integer).Value = oStockhistory.actiontype;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oStockhistory.version = Convert.ToInt32(reader[0].ToString());
                }
            }

            return oStockhistory;
        }


        public bool Delete(Stockhistory oStockhistory)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oStockhistory);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oStockhistory);

                throw exp;
            }
            return hasSuccessCount;

        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Stockhistory oStockhistory)
        {

            NpgsqlCommand command = GetCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oStockhistory.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oStockhistory.modifiedby;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oStockhistory.modifiedon;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oStockhistory.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oStockhistory.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

    }

}
