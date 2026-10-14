using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
namespace InfyPOS.Data

{
    public class StockUANDataService : SAASData
    {
        const string SQL_Search = @"select serialno, barcode, price, invoiceitemid, modifiedon, createdon, version, modifiedby, createdby, isactive, id, discount, additionaldiscount, qty, transfered, stockid, buyingprice,retaillocationid from stockuan where id=@id";

        const string SQL_Insert = @"INSERT INTO public.stockuan(serialno, barcode, price, invoiceitemid, modifiedon, createdon, version, modifiedby, createdby, isactive, discount, additionaldiscount, qty, transfered, stockid, buyingprice,retaillocationid) 
        VALUES(@serialno, @barcode, @price, @invoiceitemid, @modifiedon, @createdon, @version, @modifiedby, @createdby, @isactive, @discount, @additionaldiscount, @qty, @transfered, @stockid, @buyingprice,@retaillocationid) RETURNING id";

        const string SQL_Update = @"UPDATE public.stockuan SET serialno=@serialno, barcode=@barcode, price=@price, invoiceitemid=@invoiceitemid, modifiedon=@modifiedon, createdon=@createdon, version=@version+1, modifiedby=@modifiedby, createdby=@createdby, 
        isactive=@isactive, discount=@discount, additionaldiscount=@additionaldiscount, qty=@qty, transfered=@transfered, stockid=@stockid, buyingprice=@buyingprice where id=@id and version=@version RETURNING version";

        const string SQL_Delete = @"update public.stockuan set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";

        public List<Stockuan> Search(StockuanCreteria _Stockuan)
        {
            List<Stockuan> lst = new List<Stockuan>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_Stockuan.id != 0)
                {
                    command = GetCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _Stockuan.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("isactive", true, "=");

                    if (_Stockuan.stockid > 0)
                        qb.AddParameter("stockid", _Stockuan.stockid, "=");
                    if (_Stockuan.invoiceitemid > 0)
                        qb.AddParameter("invoiceitemid", _Stockuan.invoiceitemid, "=");
                    if (!string.IsNullOrEmpty(_Stockuan.barcode))
                        qb.AddParameter("barcode", _Stockuan.barcode, "=");
                    if (!string.IsNullOrEmpty(_Stockuan.serialno))
                        qb.AddParameter("barcode", _Stockuan.serialno, "=");
                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Stockuan oStockuan = new Stockuan();
                        oStockuan.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        oStockuan.retaillocationid = reader["retaillocationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["retaillocationid"]);
                        oStockuan.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        oStockuan.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        oStockuan.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        oStockuan.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        oStockuan.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        oStockuan.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        oStockuan.price = reader["price"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["price"]);
                        oStockuan.barcode = reader["barcode"] == DBNull.Value ? "" : reader["barcode"].ToString();
                        oStockuan.serialno = reader["serialno"] == DBNull.Value ? "" : reader["serialno"].ToString();
                        oStockuan.invoiceitemid = reader["invoiceitemid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["invoiceitemid"]);
                        oStockuan.discount = reader["discount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discount"]);
                        oStockuan.additionaldiscount = reader["additionaldiscount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["additionaldiscount"]);
                        oStockuan.qty = reader["qty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["qty"]);
                        oStockuan.transfered = reader["transfered"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["transfered"]);
                        oStockuan.buyingprice = reader["buyingprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["buyingprice"]);
                        oStockuan.stockid = reader["stockid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["stockid"]);
                        lst.Add(oStockuan);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _Stockuan);
            }
            return lst;
        }

        public Stockuan Insert(Stockuan oStockuan)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);

                    Insert(transaction, conn, oStockuan);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oStockuan);
                throw exp;
            }

            return oStockuan;
        }

        public Stockuan Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Stockuan oStockuan)
        {

            NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oStockuan.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oStockuan.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oStockuan.modifiedby;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oStockuan.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oStockuan.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oStockuan.modifiedon;
            command.Parameters.Add("price", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oStockuan.price;
            command.Parameters.Add("barcode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oStockuan.barcode == null ? "" : oStockuan.barcode;
            command.Parameters.Add("serialno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oStockuan.serialno == null ? "" : oStockuan.serialno;
            command.Parameters.Add("invoiceitemid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oStockuan.invoiceitemid;
            command.Parameters.Add("discount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oStockuan.discount;
            command.Parameters.Add("additionaldiscount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oStockuan.additionaldiscount;
            command.Parameters.Add("qty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oStockuan.qty;
            command.Parameters.Add("transfered", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oStockuan.transfered;
            command.Parameters.Add("buyingprice", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oStockuan.buyingprice;
            command.Parameters.Add("stockid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oStockuan.stockid;
            command.Parameters.Add("retaillocationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oStockuan.retaillocationid;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oStockuan.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            return oStockuan;
        }

        public Stockuan Update(Stockuan oStockuan)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oStockuan);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oStockuan);
                throw exp;
            }

            return oStockuan;
        }

        public Stockuan Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Stockuan oStockuan)
        {

            NpgsqlCommand command = GetCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oStockuan.id;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oStockuan.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oStockuan.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oStockuan.modifiedby;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oStockuan.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oStockuan.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oStockuan.modifiedon;
            command.Parameters.Add("price", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oStockuan.price;
            command.Parameters.Add("barcode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oStockuan.barcode == null ? "" : oStockuan.barcode;
            command.Parameters.Add("invoiceitemid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oStockuan.invoiceitemid;
            command.Parameters.Add("discount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oStockuan.discount;
            command.Parameters.Add("additionaldiscount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oStockuan.additionaldiscount;
            command.Parameters.Add("qty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oStockuan.qty;
            command.Parameters.Add("transfered", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oStockuan.transfered;
            command.Parameters.Add("buyingprice", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oStockuan.buyingprice;
            command.Parameters.Add("stockid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oStockuan.stockid;
            command.Parameters.Add("serialno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oStockuan.serialno == null ? "" : oStockuan.serialno;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oStockuan.version = Convert.ToInt32(reader[0].ToString());
                }
            }

            return oStockuan;
        }


        public bool Delete(Stockuan oStockuan)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oStockuan);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oStockuan);

                throw exp;
            }
            return hasSuccessCount;

        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Stockuan oStockuan)
        {

            NpgsqlCommand command = GetCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oStockuan.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oStockuan.modifiedby;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oStockuan.modifiedon;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oStockuan.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oStockuan.id;
            int successCount = command.ExecuteNonQuery();
            return successCount > 0;
        }

        internal bool Transfer2Retail(NpgsqlTransaction transaction, NpgsqlConnection conn, Lrinvoiceitemsbarcode item, Stock stock, decimal qty)
        {
            NpgsqlCommand command = GetCommand("Update stockuan set retaillocationid=@retaillocationid,transfered=transfered+@transfered where invoiceitemid=@invoiceitemid", conn, transaction);
            command.Parameters.Add("invoiceitemid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.id;
            command.Parameters.Add("transfered", NpgsqlTypes.NpgsqlDbType.Numeric).Value = qty;
            command.Parameters.Add("retaillocationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = stock.retaillocationid;
            return command.ExecuteNonQuery() > 0;
        }
    }

}
