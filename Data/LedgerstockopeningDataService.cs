using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
namespace InfyPOS.Data

{
    public class LedgerstockopeningDataService : BaseData
    {
        const string SQL_Search = @"select modifiedon, isactive, companyid, locationid, ledgerid, id, ondate, value, version, createdby, modifiedby, createdon, reason from ledgerstockopening where id=@id";


        const string SQL_Insert = @"INSERT INTO public.ledgerstockopening(modifiedon, isactive, companyid, locationid, ledgerid, ondate, value, version, createdby, modifiedby, createdon, reason) VALUES(@modifiedon, @isactive, @companyid, @locationid, @ledgerid, @ondate, @value, @version, @createdby, @modifiedby, @createdon, @reason) RETURNING id";


        const string SQL_Update = @"UPDATE public.ledgerstockopening SET modifiedon=@modifiedon, companyid=@companyid, locationid=@locationid, ledgerid=@ledgerid, ondate=@ondate, value=@value, version=@version+1, modifiedby=@modifiedby, reason=@reason where id=@id and version=@version RETURNING version";

        const string SQL_Delete = @"update public.ledgerstockopening set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";

        public List<Ledgerstockopening> Search(LedgerstockopeningCreteria _ledgerstockopening)
        {
            List<Ledgerstockopening> lst = new List<Ledgerstockopening>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_ledgerstockopening.id != 0)
                {
                    command = new NpgsqlCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _ledgerstockopening.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("isactive", true, "=");
                    qb.AddParameter("ledgerid", _ledgerstockopening.ledgerid, "=");
                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Ledgerstockopening oledgerstockopening = new Ledgerstockopening();
                        oledgerstockopening.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        oledgerstockopening.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        oledgerstockopening.companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"]);
                        oledgerstockopening.locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                        oledgerstockopening.ledgerid = reader["ledgerid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["ledgerid"]);
                        oledgerstockopening.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        oledgerstockopening.ondate = reader["ondate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["ondate"]);
                        oledgerstockopening.value = reader["value"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["value"]);
                        oledgerstockopening.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        oledgerstockopening.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        oledgerstockopening.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        oledgerstockopening.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        oledgerstockopening.reason = reader["reason"] == DBNull.Value ? "" : reader["reason"].ToString();
                        lst.Add(oledgerstockopening);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _ledgerstockopening);
            }
            return lst;
        }

        public Ledgerstockopening Insert(Ledgerstockopening oLedgerstockopening)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);

                    Insert(transaction, conn, oLedgerstockopening);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oLedgerstockopening);
            }

            return oLedgerstockopening;
        }

        public Ledgerstockopening Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Ledgerstockopening oLedgerstockopening)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oLedgerstockopening.modifiedon;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oLedgerstockopening.isactive;
            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLedgerstockopening.companyid;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLedgerstockopening.locationid;
            command.Parameters.Add("ledgerid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLedgerstockopening.ledgerid;
            command.Parameters.Add("ondate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oLedgerstockopening.ondate;
            command.Parameters.Add("value", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLedgerstockopening.value;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oLedgerstockopening.version;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLedgerstockopening.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLedgerstockopening.modifiedby;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oLedgerstockopening.createdon;
            command.Parameters.Add("reason", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oLedgerstockopening.reason == null ? "" : oLedgerstockopening.reason;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oLedgerstockopening.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            return oLedgerstockopening;
        }

        public Ledgerstockopening Update(Ledgerstockopening oLedgerstockopening)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oLedgerstockopening);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oLedgerstockopening);
            }

            return oLedgerstockopening;
        }

        public Ledgerstockopening Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Ledgerstockopening oLedgerstockopening)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oLedgerstockopening.modifiedon;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oLedgerstockopening.isactive;
            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLedgerstockopening.companyid;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLedgerstockopening.locationid;
            command.Parameters.Add("ledgerid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLedgerstockopening.ledgerid;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLedgerstockopening.id;
            command.Parameters.Add("ondate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oLedgerstockopening.ondate;
            command.Parameters.Add("value", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLedgerstockopening.value;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oLedgerstockopening.version;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLedgerstockopening.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLedgerstockopening.modifiedby;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oLedgerstockopening.createdon;
            command.Parameters.Add("reason", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oLedgerstockopening.reason == null ? "" : oLedgerstockopening.reason;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oLedgerstockopening.version = Convert.ToInt32(reader[0].ToString());
                }
            }

            return oLedgerstockopening;
        }


        public bool Delete(Ledgerstockopening oLedgerstockopening)
        {
            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oLedgerstockopening);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oLedgerstockopening);
            }
            return hasSuccessCount;
        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Ledgerstockopening oLedgerstockopening)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oLedgerstockopening.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLedgerstockopening.modifiedby;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.TimestampTZ).Value = oLedgerstockopening.modifiedon;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oLedgerstockopening.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLedgerstockopening.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

        internal LedgerstockopeningCreteria SaveAll(LedgerstockopeningCreteria item)
        {
            NpgsqlTransaction transaction = null;
            try
            {
                var existing = Search(new LedgerstockopeningCreteria()
                {
                    ledgerid = item.ledgerid
                });
                if (item.items == null)
                    item.items = new List<Ledgerstockopening>();
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    var deleted = existing.FindAll(x => !item.items.Exists(y => y.id == x.id));
                    deleted.ForEach(x => Delete(transaction, conn, x));
                    item.items.ForEach(x =>
                    {
                        if (x.id <= 0)
                        {
                            Insert(transaction, conn, x);
                        }
                        else
                        {
                            Update(transaction, conn, x);
                        }
                    });
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, item);
            }
            return item;
        }

        public LedgerstockopeningCreteria FindStockOpening(LedgerstockopeningCreteria item)
        {
            string query = @"select ledger.displayname,Ledgerstockopening.* from Ledgerstockopening
                            inner join ledger on ledger.id = ledgerid
                            where Ledgerstockopening.companyid=@companyid
                            and Ledgerstockopening.isactive=true 
                            and (ondate = @fromdate or ondate = @todate)
                            order by Ledgerstockopening.id desc";

            var param = new SortedList<string, object>();
            param.Add("fromdate", item.fromdate);
            param.Add("todate", item.todate);
            param.Add("companyid", item.companyid);
            using (var reader = ExecuteReader(query, param))
            {
                while (reader.Read())
                {
                    Ledgerstockopening oledgerstockopening = new Ledgerstockopening();
                    oledgerstockopening.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                    oledgerstockopening.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                    oledgerstockopening.companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"]);
                    oledgerstockopening.locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                    oledgerstockopening.ledgerid = reader["ledgerid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["ledgerid"]);
                    oledgerstockopening.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                    oledgerstockopening.ondate = reader["ondate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["ondate"]);
                    oledgerstockopening.value = reader["value"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["value"]);
                    oledgerstockopening.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                    oledgerstockopening.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                    oledgerstockopening.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                    oledgerstockopening.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                    oledgerstockopening.reason = reader["reason"] == DBNull.Value ? "" : reader["reason"].ToString();
                    if(oledgerstockopening.ondate == item.fromdate)
                    {
                        item.opening = oledgerstockopening;
                    }
                    else
                    {
                        item.closing = oledgerstockopening;
                    }
                }
            }
            if (item.opening == null)
                throw new Exception(item.fromdate.ToString("dd-MMM-yyyy") + " opening stock not configured");
            if (item.closing == null && item.todate.Date != DateTime.Today.Date)
                throw new Exception(item.todate.ToString("dd-MMM-yyyy") + " closing stock not configured");
            return item;
        }
    }

}
