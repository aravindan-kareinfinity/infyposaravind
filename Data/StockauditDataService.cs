using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
namespace InfyPOS.Data

{
    public class StockauditDataService : SAASData
    {
        const string SQL_Search = @"select stockaudit.modifiedby, stockaudit.modifiedon, stockaudit.createdby, stockaudit.isactive, 
        stockaudit.id, stockaudit.createdon, stockaudit.version,
        condition, matched, missing, additional, recordedon, recordedemployeeid, stockaudit.locationid, 
        producttype, missingcount, additionalcount,employee.name recordedby from stockaudit
        left outer join employee on employee.id = recordedemployeeid
        where id=@id";

        const string SQL_Insert = @"INSERT INTO public.stockaudit(modifiedby, modifiedon, createdby, isactive, createdon, version, condition, matched, missing, additional, recordedon, recordedemployeeid, locationid, producttype, missingcount, additionalcount) VALUES(@modifiedby, @modifiedon, @createdby, @isactive, @createdon, @version, @condition, @matched, @missing, @additional, @recordedon, @recordedemployeeid, @locationid, @producttype, @missingcount, @additionalcount) RETURNING id";

        const string SQL_Update = @"UPDATE public.stockaudit SET modifiedby=@modifiedby, modifiedon=@modifiedon, version=@version+1, condition=@condition, matched=@matched, missing=@missing, additional=@additional, recordedon=@recordedon, recordedemployeeid=@recordedemployeeid, locationid=@locationid, producttype=@producttype, missingcount=@missingcount, additionalcount=@additionalcount where id=@id and version=@version RETURNING version";

        const string SQL_Delete = @"update public.stockaudit set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";

        public List<Stockaudit> Search(StockauditCreteria _stockaudit)
        {
            List<Stockaudit> lst = new List<Stockaudit>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_stockaudit.id != 0)
                {
                    command = new NpgsqlCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _stockaudit.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("isactive", true, "=");

                    //if (!string.IsNullOrEmpty(_stockaudit.Code))
                    //    qb.AddParameter("code", _stockaudit.Code, "=");

                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Stockaudit ostockaudit = new Stockaudit();
                        ostockaudit.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        ostockaudit.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        ostockaudit.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        ostockaudit.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        ostockaudit.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        ostockaudit.recorededby = reader["recordedby"] == DBNull.Value ? "" : Convert.ToString(reader["recordedby"]);
                        ostockaudit.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        ostockaudit.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        ostockaudit.condition_json = reader["condition"] == DBNull.Value ? null : reader["condition"].ToString();
                        ostockaudit.matched_json = reader["matched"] == DBNull.Value ? null : reader["matched"].ToString();
                        ostockaudit.missing_json = reader["missing"] == DBNull.Value ? null : reader["missing"].ToString();
                        ostockaudit.additional_json = reader["additional"] == DBNull.Value ? null : reader["additional"].ToString();
                        ostockaudit.recordedon = reader["recordedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["recordedon"]);
                        ostockaudit.recordedemployeeid = reader["recordedemployeeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["recordedemployeeid"]);
                        ostockaudit.locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                        ostockaudit.producttype = reader["producttype"] == DBNull.Value ? 0 : Convert.ToInt64(reader["producttype"]);
                        ostockaudit.missingcount = reader["missingcount"] == DBNull.Value ? 0 : Convert.ToInt32(reader["missingcount"]);
                        ostockaudit.additionalcount = reader["additionalcount"] == DBNull.Value ? 0 : Convert.ToInt32(reader["additionalcount"]);
                        lst.Add(ostockaudit);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _stockaudit);
            }
            return lst;
        }

        public Stockaudit Insert(Stockaudit oStockaudit)
        {
            oStockaudit.additionalcount = oStockaudit.additional != null ? oStockaudit.additional.Count : 0;
            oStockaudit.missingcount = oStockaudit.missing != null ? oStockaudit.missing.Count : 0;
            oStockaudit.recordedemployeeid = Context.EmployeeID;
            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);

                    Insert(transaction, conn, oStockaudit);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oStockaudit);
                throw exp;
            }

            return oStockaudit;
        }

        public Stockaudit Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Stockaudit oStockaudit)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oStockaudit.modifiedby;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oStockaudit.modifiedon;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oStockaudit.createdby;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oStockaudit.isactive;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oStockaudit.createdon;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oStockaudit.version;
            command.Parameters.Add("condition", NpgsqlTypes.NpgsqlDbType.Json).Value = oStockaudit.condition_json == null ? "null" : oStockaudit.condition_json;
            command.Parameters.Add("matched", NpgsqlTypes.NpgsqlDbType.Json).Value = oStockaudit.matched_json == null ? "null" : oStockaudit.matched_json;
            command.Parameters.Add("missing", NpgsqlTypes.NpgsqlDbType.Json).Value = oStockaudit.missing_json == null ? "null" : oStockaudit.missing_json;
            command.Parameters.Add("additional", NpgsqlTypes.NpgsqlDbType.Json).Value = oStockaudit.additional_json == null ? "null" : oStockaudit.additional_json;
            command.Parameters.Add("recordedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oStockaudit.recordedon;
            command.Parameters.Add("recordedemployeeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oStockaudit.recordedemployeeid;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oStockaudit.locationid;
            command.Parameters.Add("producttype", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oStockaudit.producttype;
            command.Parameters.Add("missingcount", NpgsqlTypes.NpgsqlDbType.Integer).Value = oStockaudit.missingcount;
            command.Parameters.Add("additionalcount", NpgsqlTypes.NpgsqlDbType.Integer).Value = oStockaudit.additionalcount;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oStockaudit.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            return oStockaudit;
        }

        public Stockaudit Update(Stockaudit oStockaudit)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oStockaudit);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oStockaudit);
                throw exp;
            }

            return oStockaudit;
        }

        public Stockaudit Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Stockaudit oStockaudit)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oStockaudit.modifiedby;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oStockaudit.modifiedon;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oStockaudit.createdby;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oStockaudit.isactive;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oStockaudit.id;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oStockaudit.createdon;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oStockaudit.version;
            command.Parameters.Add("condition", NpgsqlTypes.NpgsqlDbType.Json).Value = oStockaudit.condition_json == null ? "null" : oStockaudit.condition_json;
            command.Parameters.Add("matched", NpgsqlTypes.NpgsqlDbType.Json).Value = oStockaudit.matched_json == null ? "null" : oStockaudit.matched_json;
            command.Parameters.Add("missing", NpgsqlTypes.NpgsqlDbType.Json).Value = oStockaudit.missing_json == null ? "null" : oStockaudit.missing_json;
            command.Parameters.Add("additional", NpgsqlTypes.NpgsqlDbType.Json).Value = oStockaudit.additional_json == null ? "null" : oStockaudit.additional_json;
            command.Parameters.Add("recordedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oStockaudit.recordedon;
            command.Parameters.Add("recordedemployeeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oStockaudit.recordedemployeeid;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oStockaudit.locationid;
            command.Parameters.Add("producttype", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oStockaudit.producttype;
            command.Parameters.Add("missingcount", NpgsqlTypes.NpgsqlDbType.Integer).Value = oStockaudit.missingcount;
            command.Parameters.Add("additionalcount", NpgsqlTypes.NpgsqlDbType.Integer).Value = oStockaudit.additionalcount;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oStockaudit.version = Convert.ToInt32(reader[0].ToString());
                }
            }

            return oStockaudit;
        }


        public bool Delete(Stockaudit oStockaudit)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oStockaudit);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oStockaudit);

                throw exp;
            }
            return hasSuccessCount;

        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Stockaudit oStockaudit)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oStockaudit.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oStockaudit.modifiedby;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oStockaudit.modifiedon;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oStockaudit.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oStockaudit.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

    }

}
