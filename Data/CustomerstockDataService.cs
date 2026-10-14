using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
namespace InfyPOS.Data

{
    public class CustomerstockDataService : SAASData
    {
        const string SQL_Search = @"select id, curstomerid, iemployeeid, aemployeeid, remployeeid, issuedon, noofitemsissued, noofitemsreceived, receivedon, isactive, createdby, modifiedby, version, createdon, modifiedon, stockitems, status, code from customerstock where id=@id";


        const string SQL_Insert = @"INSERT INTO public.customerstock(curstomerid, iemployeeid, aemployeeid, remployeeid, issuedon, noofitemsissued, noofitemsreceived, receivedon, isactive, createdby, modifiedby, version, createdon, modifiedon, stockitems, status, code) VALUES(@curstomerid, @iemployeeid, @aemployeeid, @remployeeid, @issuedon, @noofitemsissued, @noofitemsreceived, @receivedon, @isactive, @createdby, @modifiedby, @version, @createdon, @modifiedon, @stockitems, @status, @code) RETURNING id";


        const string SQL_Update = @"UPDATE public.customerstock SET curstomerid=@curstomerid, iemployeeid=@iemployeeid, aemployeeid=@aemployeeid, remployeeid=@remployeeid, issuedon=@issuedon, noofitemsissued=@noofitemsissued, noofitemsreceived=@noofitemsreceived, receivedon=@receivedon, modifiedby=@modifiedby, version=@version+1, modifiedon=@modifiedon, stockitems=@stockitems, status=@status, code=@code where id=@id and version=@version RETURNING version";

        const string SQL_Delete = @"update public.customerstock set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";

        public List<Customerstock> Search(CustomerstockCreteria _customerstock)
        {
            List<Customerstock> lst = new List<Customerstock>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_customerstock.id != 0)
                {
                    command = GetCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _customerstock.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("isactive", true, "=");

                    //if (!string.IsNullOrEmpty(_customerstock.Code))
                    //    qb.AddParameter("code", _customerstock.Code, "=");

                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Customerstock ocustomerstock = new Customerstock();
                        ocustomerstock.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        ocustomerstock.curstomerid = reader["curstomerid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["curstomerid"]);
                        ocustomerstock.iemployeeid = reader["iemployeeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["iemployeeid"]);
                        ocustomerstock.aemployeeid = reader["aemployeeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["aemployeeid"]);
                        ocustomerstock.remployeeid = reader["remployeeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["remployeeid"]);
                        ocustomerstock.issuedon = reader["issuedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["issuedon"]);
                        ocustomerstock.noofitemsissued = reader["noofitemsissued"] == DBNull.Value ? 0 : Convert.ToInt32(reader["noofitemsissued"]);
                        ocustomerstock.noofitemsreceived = reader["noofitemsreceived"] == DBNull.Value ? 0 : Convert.ToInt32(reader["noofitemsreceived"]);
                        ocustomerstock.receivedon = reader["receivedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["receivedon"]);
                        ocustomerstock.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        ocustomerstock.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        ocustomerstock.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        ocustomerstock.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        ocustomerstock.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        ocustomerstock.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        ocustomerstock.stockitems_json = reader["stockitems"] == DBNull.Value ? "null" : reader["stockitems"].ToString();
                        ocustomerstock.status = reader["status"] == DBNull.Value ? 0 : Convert.ToInt32(reader["status"]);
                        ocustomerstock.code = reader["code"] == DBNull.Value ? "" : reader["code"].ToString();
                        lst.Add(ocustomerstock);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _customerstock);
            }
            return lst;
        }

        public Customerstock Insert(Customerstock oCustomerstock)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                var sequence = SequenceManager.Instance(Context.OrganizationID).Get("customerstock.code", 0);
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);
                    oCustomerstock.code = sequence.GetNextNumber(Context,conn, transaction);

                    Insert(transaction, conn, oCustomerstock);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oCustomerstock);


            }

            return oCustomerstock;
        }

        public Customerstock Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Customerstock oCustomerstock)
        {

            NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("curstomerid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCustomerstock.curstomerid;
            command.Parameters.Add("iemployeeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCustomerstock.iemployeeid;
            command.Parameters.Add("aemployeeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCustomerstock.aemployeeid;
            command.Parameters.Add("remployeeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCustomerstock.remployeeid;
            command.Parameters.Add("issuedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oCustomerstock.issuedon;
            command.Parameters.Add("noofitemsissued", NpgsqlTypes.NpgsqlDbType.Integer).Value = oCustomerstock.noofitemsissued;
            command.Parameters.Add("noofitemsreceived", NpgsqlTypes.NpgsqlDbType.Integer).Value = oCustomerstock.noofitemsreceived;
            command.Parameters.Add("receivedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oCustomerstock.receivedon;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oCustomerstock.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oCustomerstock.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("stockitems", NpgsqlTypes.NpgsqlDbType.Json).Value = oCustomerstock.stockitems_json == null ? "null" : oCustomerstock.stockitems_json;
            command.Parameters.Add("status", NpgsqlTypes.NpgsqlDbType.Integer).Value = oCustomerstock.status;
            command.Parameters.Add("code", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCustomerstock.code == null ? "" : oCustomerstock.code;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oCustomerstock.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            return oCustomerstock;
        }

        public Customerstock Update(Customerstock oCustomerstock)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oCustomerstock);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oCustomerstock);


            }

            return oCustomerstock;
        }

        public Customerstock Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Customerstock oCustomerstock)
        {

            NpgsqlCommand command = GetCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCustomerstock.id;
            command.Parameters.Add("curstomerid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCustomerstock.curstomerid;
            command.Parameters.Add("iemployeeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCustomerstock.iemployeeid;
            command.Parameters.Add("aemployeeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCustomerstock.aemployeeid;
            command.Parameters.Add("remployeeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCustomerstock.remployeeid;
            command.Parameters.Add("issuedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oCustomerstock.issuedon;
            command.Parameters.Add("noofitemsissued", NpgsqlTypes.NpgsqlDbType.Integer).Value = oCustomerstock.noofitemsissued;
            command.Parameters.Add("noofitemsreceived", NpgsqlTypes.NpgsqlDbType.Integer).Value = oCustomerstock.noofitemsreceived;
            command.Parameters.Add("receivedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oCustomerstock.receivedon;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oCustomerstock.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCustomerstock.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oCustomerstock.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oCustomerstock.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("stockitems", NpgsqlTypes.NpgsqlDbType.Json).Value = oCustomerstock.stockitems_json == null ? "null" : oCustomerstock.stockitems_json;
            command.Parameters.Add("status", NpgsqlTypes.NpgsqlDbType.Integer).Value = oCustomerstock.status;
            command.Parameters.Add("code", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCustomerstock.code == null ? "" : oCustomerstock.code;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oCustomerstock.version = Convert.ToInt32(reader[0].ToString());
                }
            }

            return oCustomerstock;
        }


        public bool Delete(Customerstock oCustomerstock)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oCustomerstock);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oCustomerstock);



            }
            return hasSuccessCount;

        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Customerstock oCustomerstock)
        {

            NpgsqlCommand command = GetCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oCustomerstock.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oCustomerstock.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCustomerstock.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

    }

}
