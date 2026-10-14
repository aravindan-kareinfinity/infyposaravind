using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
namespace InfyPOS.Data

{
    public class BuybackschemeDataService : SAASData
    {
        const string SQL_Search = @"select name, isactive, createdby, modifiedby, version, createdon, modifiedon, id, supplierid,schemediscount, productid, brandid, fromdate, todate, schemedate, items from buybackscheme where id=@id";


        const string SQL_Insert = @"INSERT INTO public.buybackscheme(name, isactive, createdby, modifiedby, version, createdon, modifiedon, supplierid,schemediscount, productid, brandid, fromdate, todate, schemedate, items) VALUES(@name, @isactive, @createdby, @modifiedby, @version, @createdon, @modifiedon, @supplierid,@schemediscount, @productid, @brandid, @fromdate, @todate, @schemedate, @items) RETURNING id";


        const string SQL_Update = @"UPDATE public.buybackscheme SET name=@name, modifiedby=@modifiedby, version=@version+1, modifiedon=@modifiedon, supplierid=@supplierid,schemediscount=@schemediscount, productid=@productid, brandid=@brandid, fromdate=@fromdate, todate=@todate, schemedate=@schemedate, items=@items where id=@id and version=@version RETURNING version";

        const string SQL_Delete = @"update public.buybackscheme set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";

        public List<Buybackscheme> Search(BuybackschemeCreteria _buybackscheme)
        {
            List<Buybackscheme> lst = new List<Buybackscheme>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_buybackscheme.id != 0)
                {
                    command = new NpgsqlCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _buybackscheme.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));

                    if (_buybackscheme.hideitems)
                        qb.CurrentQuery = qb.CurrentQuery.Replace(",items", "");

                    qb.AddParameter("isactive", true, "=");

                    if (_buybackscheme.supplierid > 0)
                        qb.AddParameter("supplierid", _buybackscheme.supplierid, "=");

                    if (_buybackscheme.brandid > 0)
                        qb.AddParameter("brandid", _buybackscheme.brandid, "=");

                    if (_buybackscheme.fromdate > DateTime.MinValue)
                        qb.AddParameter("fromdate", _buybackscheme.fromdate, ">=");

                    if (_buybackscheme.todate > DateTime.MinValue)
                        qb.AddParameter("todate", _buybackscheme.todate.AddDays(1), "<");

                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Buybackscheme obuybackscheme = new Buybackscheme();
                        obuybackscheme.name = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
                        obuybackscheme.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        obuybackscheme.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        obuybackscheme.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        obuybackscheme.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        obuybackscheme.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        obuybackscheme.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        obuybackscheme.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        obuybackscheme.supplierid = reader["supplierid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["supplierid"]);
                        obuybackscheme.schemediscount = reader["schemediscount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["schemediscount"]);
                        obuybackscheme.productid = reader["productid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productid"]);
                        obuybackscheme.brandid = reader["brandid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["brandid"]);
                        obuybackscheme.fromdate = reader["fromdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["fromdate"]);
                        obuybackscheme.todate = reader["todate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["todate"]);
                        obuybackscheme.schemedate = reader["schemedate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["schemedate"]);
                        if (!_buybackscheme.hideitems)
                            obuybackscheme.items_json = reader["items"] == DBNull.Value ? null : reader["items"].ToString();
                        lst.Add(obuybackscheme);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _buybackscheme);
            }
            return lst;
        }

        public Buybackscheme Insert(Buybackscheme oBuybackscheme)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);

                    Insert(transaction, conn, oBuybackscheme);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oBuybackscheme);
            }

            return oBuybackscheme;
        }

        public Buybackscheme Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Buybackscheme oBuybackscheme)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("name", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBuybackscheme.name == null ? "" : oBuybackscheme.name;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBuybackscheme.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBuybackscheme.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBuybackscheme.modifiedby;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBuybackscheme.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBuybackscheme.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBuybackscheme.modifiedon;
            command.Parameters.Add("supplierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBuybackscheme.supplierid;
            command.Parameters.Add("schemediscount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBuybackscheme.schemediscount;
            command.Parameters.Add("productid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBuybackscheme.productid;
            command.Parameters.Add("brandid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBuybackscheme.brandid;
            command.Parameters.Add("fromdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBuybackscheme.fromdate;
            command.Parameters.Add("todate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBuybackscheme.todate;
            command.Parameters.Add("schemedate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBuybackscheme.schemedate;
            command.Parameters.Add("items", NpgsqlTypes.NpgsqlDbType.Json).Value = oBuybackscheme.items_json == null ? "null" : oBuybackscheme.items_json;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oBuybackscheme.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            return oBuybackscheme;
        }

        public Buybackscheme Update(Buybackscheme oBuybackscheme)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oBuybackscheme);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oBuybackscheme);
            }

            return oBuybackscheme;
        }

        public Buybackscheme Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Buybackscheme oBuybackscheme)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("name", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBuybackscheme.name == null ? "" : oBuybackscheme.name;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBuybackscheme.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBuybackscheme.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBuybackscheme.modifiedby;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBuybackscheme.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBuybackscheme.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBuybackscheme.modifiedon;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBuybackscheme.id;
            command.Parameters.Add("supplierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBuybackscheme.supplierid;
            command.Parameters.Add("schemediscount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBuybackscheme.schemediscount;
            command.Parameters.Add("productid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBuybackscheme.productid;
            command.Parameters.Add("brandid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBuybackscheme.brandid;
            command.Parameters.Add("fromdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBuybackscheme.fromdate;
            command.Parameters.Add("todate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBuybackscheme.todate;
            command.Parameters.Add("schemedate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBuybackscheme.schemedate;
            command.Parameters.Add("items", NpgsqlTypes.NpgsqlDbType.Json).Value = oBuybackscheme.items_json == null ? "null" : oBuybackscheme.items_json;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oBuybackscheme.version = Convert.ToInt32(reader[0].ToString());
                }
            }

            return oBuybackscheme;
        }


        public bool Delete(Buybackscheme oBuybackscheme)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oBuybackscheme);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oBuybackscheme);

            }
            return hasSuccessCount;

        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Buybackscheme oBuybackscheme)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBuybackscheme.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBuybackscheme.modifiedby;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBuybackscheme.modifiedon;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBuybackscheme.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBuybackscheme.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

    }

}
