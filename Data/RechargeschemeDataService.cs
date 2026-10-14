using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
namespace InfyPOS.Data

{
    public class RechargeschemeDataService : SAASData
    {
        const string SQL_Search = @"select name, isactive, createdby, modifiedby, version, cashbackinpercentage, createdon, modifiedon, id, supplierid, productid, itemid, items from rechargescheme where id=@id";


        const string SQL_Insert = @"INSERT INTO public.rechargescheme(name, isactive, createdby, modifiedby, version, cashbackinpercentage, createdon, modifiedon, supplierid, productid, itemid, items) VALUES(@name, @isactive, @createdby, @modifiedby, @version, @cashbackinpercentage, @createdon, @modifiedon, @supplierid, @productid, @itemid, @items) RETURNING id";


        const string SQL_Update = @"UPDATE public.rechargescheme SET name=@name, modifiedby=@modifiedby, version=@version+1, cashbackinpercentage=@cashbackinpercentage, modifiedon=@modifiedon, supplierid=@supplierid, productid=@productid, itemid=@itemid, items=@items where id=@id and version=@version RETURNING version";

        const string SQL_Delete = @"update public.rechargescheme set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";

        public List<Rechargescheme> Search(RechargeschemeCreteria _rechargescheme)
        {
            List<Rechargescheme> lst = new List<Rechargescheme>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_rechargescheme.id != 0)
                {
                    command = new NpgsqlCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _rechargescheme.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));

                    if (_rechargescheme.hideitems)
                        qb.CurrentQuery = qb.CurrentQuery.Replace(",items", "");

                    qb.AddParameter("isactive", true, "=");

                    if (_rechargescheme.supplierid>0)
                        qb.AddParameter("supplierid", _rechargescheme.supplierid, "=");

                    if (_rechargescheme.productid > 0)
                        qb.AddParameter("productid", _rechargescheme.productid, "=");

                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Rechargescheme orechargescheme = new Rechargescheme();
                        orechargescheme.name = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
                        orechargescheme.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        orechargescheme.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        orechargescheme.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        orechargescheme.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        orechargescheme.cashbackinpercentage = reader["cashbackinpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["cashbackinpercentage"]);
                        orechargescheme.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        orechargescheme.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        orechargescheme.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        orechargescheme.supplierid = reader["supplierid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["supplierid"]);
                        orechargescheme.productid = reader["productid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productid"]);
                        orechargescheme.itemid = reader["itemid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["itemid"]);
                        if (!_rechargescheme.hideitems)
                            orechargescheme.items_json = reader["items"] == DBNull.Value ? null : reader["items"].ToString();
                        lst.Add(orechargescheme);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _rechargescheme);
            }
            return lst;
        }

        public Rechargescheme Insert(Rechargescheme oRechargescheme)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);

                    Insert(transaction, conn, oRechargescheme);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oRechargescheme);
                throw exp;
            }

            return oRechargescheme;
        }

        public Rechargescheme Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Rechargescheme oRechargescheme)
        {
            if (oRechargescheme.items == null) oRechargescheme.items = new List<Rechargescheme.Items>();
            for (int i = 0; i < oRechargescheme.items.Count;i++)
                oRechargescheme.items[i].id = i;

            NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("name", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oRechargescheme.name == null ? "" : oRechargescheme.name;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oRechargescheme.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oRechargescheme.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oRechargescheme.modifiedby;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oRechargescheme.version;
            command.Parameters.Add("cashbackinpercentage", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oRechargescheme.cashbackinpercentage;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oRechargescheme.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oRechargescheme.modifiedon;
            command.Parameters.Add("supplierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oRechargescheme.supplierid;
            command.Parameters.Add("productid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oRechargescheme.productid;
            command.Parameters.Add("itemid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oRechargescheme.itemid;
            command.Parameters.Add("items", NpgsqlTypes.NpgsqlDbType.Json).Value = oRechargescheme.items_json == null ? "null" : oRechargescheme.items_json;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oRechargescheme.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            return oRechargescheme;
        }

        public Rechargescheme Update(Rechargescheme oRechargescheme)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oRechargescheme);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oRechargescheme);
                throw exp;
            }

            return oRechargescheme;
        }

        public Rechargescheme Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Rechargescheme oRechargescheme)
        {
            if (oRechargescheme.items == null) oRechargescheme.items = new List<Rechargescheme.Items>();
            for (int i = 0; i < oRechargescheme.items.Count; i++)
                if(oRechargescheme.items[i].id == 0)
                    oRechargescheme.items[i].id = oRechargescheme.items.Max(e => e.id) + 1;

            NpgsqlCommand command = new NpgsqlCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("name", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oRechargescheme.name == null ? "" : oRechargescheme.name;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oRechargescheme.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oRechargescheme.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oRechargescheme.modifiedby;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oRechargescheme.version;
            command.Parameters.Add("cashbackinpercentage", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oRechargescheme.cashbackinpercentage;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oRechargescheme.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oRechargescheme.modifiedon;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oRechargescheme.id;
            command.Parameters.Add("supplierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oRechargescheme.supplierid;
            command.Parameters.Add("productid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oRechargescheme.productid;
            command.Parameters.Add("itemid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oRechargescheme.itemid;
            command.Parameters.Add("items", NpgsqlTypes.NpgsqlDbType.Json).Value = oRechargescheme.items_json == null ? "null" : oRechargescheme.items_json;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oRechargescheme.version = Convert.ToInt32(reader[0].ToString());
                }
            }

            return oRechargescheme;
        }


        public bool Delete(Rechargescheme oRechargescheme)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oRechargescheme);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oRechargescheme);

                throw exp;
            }
            return hasSuccessCount;

        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Rechargescheme oRechargescheme)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oRechargescheme.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oRechargescheme.modifiedby;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oRechargescheme.modifiedon;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oRechargescheme.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oRechargescheme.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

    }

}
