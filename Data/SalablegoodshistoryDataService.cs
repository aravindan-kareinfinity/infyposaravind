using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
namespace InfyPOS.Data
{
    public class SalablegoodshistoryDataService : SAASData
    {
        const string SQL_Search = @"select id, isactive, createdby, modifiedby, version, createdon, modifiedon, iqty, oqty, dqty,jqty, stockid, barcode, locationid, price, discount, additionaldiscount, salablegoodsid, referenceid, actiontype, auditqty, reason from salablegoodshistory where id=@id";

        const string SQL_Insert = @"INSERT INTO public.salablegoodshistory(isactive, createdby, modifiedby, version, createdon, modifiedon, iqty, oqty, dqty,jqty, stockid, barcode, locationid, price, discount, additionaldiscount, salablegoodsid, referenceid, actiontype, auditqty,mbarcode,reason) 
        VALUES(@isactive, @createdby, @modifiedby, @version, @createdon, @modifiedon, @iqty, @oqty, @dqty,@jqty, @stockid, @barcode, @locationid, @price, @discount, @additionaldiscount, @salablegoodsid, @referenceid, @actiontype, @auditqty,@mbarcode,@reason) RETURNING id";

        const string SQL_Update = @"UPDATE public.salablegoodshistory SET modifiedby=@modifiedby, version=@version+1, modifiedon=@modifiedon, iqty=@iqty, oqty=@oqty, dqty=@dqty,jqty=@jqty, stockid=@stockid, barcode=@barcode, locationid=@locationid, price=@price, discount=@discount, additionaldiscount=@additionaldiscount, salablegoodsid=@salablegoodsid, referenceid=@referenceid, actiontype=@actiontype, auditqty=@auditqty where id=@id and version=@version RETURNING version";

        const string SQL_Delete = @"update public.salablegoodshistory set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";

        public List<Salablegoodshistory> Search(SalablegoodshistoryCreteria _salablegoodshistory)
        {
            List<Salablegoodshistory> lst = new List<Salablegoodshistory>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_salablegoodshistory.id != 0)
                {
                    command = GetCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _salablegoodshistory.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("isactive", true, "=");

                    if (_salablegoodshistory.salablegoodsid > 0)
                        qb.AddParameter("salablegoodsid", _salablegoodshistory.salablegoodsid, "=");
                    if (string.IsNullOrEmpty(_salablegoodshistory.barcode))
                        qb.AddParameter("barcode", _salablegoodshistory.barcode, "=");
                    if (_salablegoodshistory.actiontype > 0)
                        qb.AddParameter("actiontype", _salablegoodshistory.actiontype, "=");


                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Salablegoodshistory osalablegoodshistory = new Salablegoodshistory();
                        osalablegoodshistory.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        osalablegoodshistory.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        osalablegoodshistory.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        osalablegoodshistory.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        osalablegoodshistory.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        osalablegoodshistory.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        osalablegoodshistory.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        osalablegoodshistory.iqty = reader["iqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["iqty"]);
                        osalablegoodshistory.oqty = reader["oqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["oqty"]);
                        osalablegoodshistory.dqty = reader["dqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["dqty"]);
                        osalablegoodshistory.jqty = reader["jqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["jqty"]);
                        osalablegoodshistory.auditqty = reader["auditqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["auditqty"]);
                        osalablegoodshistory.stockid = reader["stockid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["stockid"]);
                        osalablegoodshistory.barcode = reader["barcode"] == DBNull.Value ? "" : reader["barcode"].ToString();
                        osalablegoodshistory.locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                        osalablegoodshistory.price = reader["price"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["price"]);
                        osalablegoodshistory.discount = reader["discount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discount"]);
                        osalablegoodshistory.additionaldiscount = reader["additionaldiscount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["additionaldiscount"]);
                        osalablegoodshistory.salablegoodsid = reader["salablegoodsid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["salablegoodsid"]);
                        osalablegoodshistory.referenceid = reader["referenceid"] == DBNull.Value ? "" : reader["referenceid"].ToString();
                        osalablegoodshistory.actiontype = reader["actiontype"] == DBNull.Value ? 0 : (Salablegoodshistory.ActionType)Convert.ToInt32(reader["actiontype"]);
                        osalablegoodshistory.reason = reader["reason"] == DBNull.Value ? "" : reader["reason"].ToString();
                        lst.Add(osalablegoodshistory);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _salablegoodshistory);
            }
            return lst;
        }

        public Salablegoodshistory Insert(Salablegoodshistory oSalablegoodshistory)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);

                    Insert(transaction, conn, oSalablegoodshistory);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oSalablegoodshistory);
                throw exp;
            }

            return oSalablegoodshistory;
        }

        public Salablegoodshistory Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Salablegoodshistory oSalablegoodshistory)
        {

            NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oSalablegoodshistory.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSalablegoodshistory.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSalablegoodshistory.modifiedby;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oSalablegoodshistory.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oSalablegoodshistory.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oSalablegoodshistory.modifiedon;
            command.Parameters.Add("iqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oSalablegoodshistory.iqty;
            command.Parameters.Add("oqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oSalablegoodshistory.oqty;
            command.Parameters.Add("dqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oSalablegoodshistory.dqty;
            command.Parameters.Add("jqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oSalablegoodshistory.jqty;
            command.Parameters.Add("auditqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oSalablegoodshistory.auditqty;
            command.Parameters.Add("stockid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSalablegoodshistory.stockid;
            command.Parameters.Add("barcode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSalablegoodshistory.barcode == null ? "" : oSalablegoodshistory.barcode;
            command.Parameters.Add("mbarcode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSalablegoodshistory.mbarcode == null ? "" : oSalablegoodshistory.mbarcode;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSalablegoodshistory.locationid;
            command.Parameters.Add("price", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oSalablegoodshistory.price;
            command.Parameters.Add("discount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oSalablegoodshistory.discount;
            command.Parameters.Add("additionaldiscount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oSalablegoodshistory.additionaldiscount;
            command.Parameters.Add("salablegoodsid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSalablegoodshistory.salablegoodsid;
            command.Parameters.Add("referenceid", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSalablegoodshistory.referenceid == null ? "" : oSalablegoodshistory.referenceid;
            command.Parameters.Add("actiontype", NpgsqlTypes.NpgsqlDbType.Integer).Value = oSalablegoodshistory.actiontype;
            command.Parameters.Add("reason", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSalablegoodshistory.reason == null ? "" : oSalablegoodshistory.reason;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oSalablegoodshistory.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            return oSalablegoodshistory;
        }

        public Salablegoodshistory Update(Salablegoodshistory oSalablegoodshistory)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oSalablegoodshistory);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oSalablegoodshistory);
                throw exp;
            }

            return oSalablegoodshistory;
        }

        public Salablegoodshistory Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Salablegoodshistory oSalablegoodshistory)
        {

            NpgsqlCommand command = GetCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSalablegoodshistory.id;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oSalablegoodshistory.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSalablegoodshistory.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSalablegoodshistory.modifiedby;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oSalablegoodshistory.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oSalablegoodshistory.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oSalablegoodshistory.modifiedon;
            command.Parameters.Add("iqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oSalablegoodshistory.iqty;
            command.Parameters.Add("oqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oSalablegoodshistory.oqty;
            command.Parameters.Add("dqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oSalablegoodshistory.dqty;
            command.Parameters.Add("jqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oSalablegoodshistory.jqty;
            command.Parameters.Add("auditqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oSalablegoodshistory.auditqty;
            command.Parameters.Add("stockid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSalablegoodshistory.stockid;
            command.Parameters.Add("barcode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSalablegoodshistory.barcode == null ? "" : oSalablegoodshistory.barcode;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSalablegoodshistory.locationid;
            command.Parameters.Add("price", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oSalablegoodshistory.price;
            command.Parameters.Add("discount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oSalablegoodshistory.discount;
            command.Parameters.Add("additionaldiscount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oSalablegoodshistory.additionaldiscount;
            command.Parameters.Add("salablegoodsid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSalablegoodshistory.salablegoodsid;
            command.Parameters.Add("referenceid", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSalablegoodshistory.referenceid == null ? "" : oSalablegoodshistory.referenceid;
            command.Parameters.Add("actiontype", NpgsqlTypes.NpgsqlDbType.Integer).Value = oSalablegoodshistory.actiontype;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oSalablegoodshistory.version = Convert.ToInt32(reader[0].ToString());
                }
            }

            return oSalablegoodshistory;
        }


        public bool Delete(Salablegoodshistory oSalablegoodshistory)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oSalablegoodshistory);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oSalablegoodshistory);

                throw exp;
            }
            return hasSuccessCount;

        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Salablegoodshistory oSalablegoodshistory)
        {

            NpgsqlCommand command = GetCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oSalablegoodshistory.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSalablegoodshistory.modifiedby;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oSalablegoodshistory.modifiedon;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oSalablegoodshistory.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSalablegoodshistory.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

    }

}
