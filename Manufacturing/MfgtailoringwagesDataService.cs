using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
namespace InfyPOS.Data

{
    public class MfgtailoringwagesDataService : BaseData
    {
        const string SQL_Search = @"select qty, id, createdby, version, modifiedby, employeeid, modifiedon, createdon, jobcharge, paidcharge, fromdate, todate, isactive, receiptno, paidon, mfgtailoringids from mfgtailoringwages where id=@id";


        const string SQL_Insert = @"INSERT INTO public.mfgtailoringwages(qty, createdby, version, modifiedby, employeeid, modifiedon, createdon, jobcharge, paidcharge, fromdate, todate, isactive, receiptno, paidon, mfgtailoringids) VALUES(@qty, @createdby, @version, @modifiedby, @employeeid, @modifiedon, @createdon, @jobcharge, @paidcharge, @fromdate, @todate, @isactive, @receiptno, @paidon, @mfgtailoringids) RETURNING id";


        const string SQL_Update = @"UPDATE public.mfgtailoringwages SET qty=@qty, version=@version+1, modifiedby=@modifiedby, employeeid=@employeeid, modifiedon=@modifiedon, jobcharge=@jobcharge, paidcharge=@paidcharge, fromdate=@fromdate, todate=@todate, receiptno=@receiptno, paidon=@paidon, mfgtailoringids=@mfgtailoringids where id=@id and version=@version RETURNING version";

        const string SQL_Delete = @"update public.mfgtailoringwages set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";

        public List<Mfgtailoringwages> Search(MfgtailoringwagesCreteria _mfgtailoringwages)
        {
            List<Mfgtailoringwages> lst = new List<Mfgtailoringwages>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_mfgtailoringwages.id != 0)
                {
                    command = new NpgsqlCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _mfgtailoringwages.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("isactive", true, "=");

                    //if (!string.IsNullOrEmpty(_mfgtailoringwages.Code))
                    //    qb.AddParameter("code", _mfgtailoringwages.Code, "=");

                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Mfgtailoringwages omfgtailoringwages = new Mfgtailoringwages();
                        omfgtailoringwages.qty = reader["qty"] == DBNull.Value ? 0 : Convert.ToInt32(reader["qty"]);
                        omfgtailoringwages.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        omfgtailoringwages.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        omfgtailoringwages.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        omfgtailoringwages.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        omfgtailoringwages.employeeid = reader["employeeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["employeeid"]);
                        omfgtailoringwages.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        omfgtailoringwages.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        omfgtailoringwages.jobcharge = reader["jobcharge"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["jobcharge"]);
                        omfgtailoringwages.paidcharge = reader["paidcharge"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["paidcharge"]);
                        omfgtailoringwages.fromdate = reader["fromdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["fromdate"]);
                        omfgtailoringwages.todate = reader["todate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["todate"]);
                        omfgtailoringwages.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        omfgtailoringwages.receiptno = reader["receiptno"] == DBNull.Value ? "" : reader["receiptno"].ToString();
                        omfgtailoringwages.paidon = reader["paidon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["paidon"]);
                        omfgtailoringwages.mfgtailoringids_json = reader["mfgtailoringids"] == DBNull.Value ? null : reader["mfgtailoringids"].ToString();
                        lst.Add(omfgtailoringwages);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _mfgtailoringwages);
            }
            return lst;
        }

        public Mfgtailoringwages Insert(Mfgtailoringwages oMfgtailoringwages)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    Insert(transaction, conn, oMfgtailoringwages);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oMfgtailoringwages);
            }

            return oMfgtailoringwages;
        }

        public Mfgtailoringwages Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Mfgtailoringwages oMfgtailoringwages)
        {
            var sequence = SequenceManager.Instance(Context.OrganizationID).Get("Mfgtailoringwages.receiptno", Context.OrganizationID );
            if (string.IsNullOrEmpty(oMfgtailoringwages.receiptno))
                oMfgtailoringwages.receiptno = sequence.GetNextNumber(Context,conn, transaction);

            NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("qty", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgtailoringwages.qty;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgtailoringwages.createdby;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgtailoringwages.version;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgtailoringwages.modifiedby;
            command.Parameters.Add("employeeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgtailoringwages.employeeid;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgtailoringwages.modifiedon;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgtailoringwages.createdon;
            command.Parameters.Add("jobcharge", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgtailoringwages.jobcharge;
            command.Parameters.Add("paidcharge", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgtailoringwages.paidcharge;
            command.Parameters.Add("fromdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgtailoringwages.fromdate;
            command.Parameters.Add("todate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgtailoringwages.todate;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oMfgtailoringwages.isactive;
            command.Parameters.Add("receiptno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfgtailoringwages.receiptno == null ? "" : oMfgtailoringwages.receiptno;
            command.Parameters.Add("paidon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgtailoringwages.paidon;
            command.Parameters.Add("mfgtailoringids", NpgsqlTypes.NpgsqlDbType.Json).Value = oMfgtailoringwages.mfgtailoringids_json == null ? "null" : oMfgtailoringwages.mfgtailoringids_json;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oMfgtailoringwages.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            return oMfgtailoringwages;
        }

        public Mfgtailoringwages Update(Mfgtailoringwages oMfgtailoringwages)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oMfgtailoringwages);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oMfgtailoringwages);
            }

            return oMfgtailoringwages;
        }

        public Mfgtailoringwages Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Mfgtailoringwages oMfgtailoringwages)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("qty", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgtailoringwages.qty;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgtailoringwages.id;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgtailoringwages.createdby;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgtailoringwages.version;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgtailoringwages.modifiedby;
            command.Parameters.Add("employeeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgtailoringwages.employeeid;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgtailoringwages.modifiedon;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgtailoringwages.createdon;
            command.Parameters.Add("jobcharge", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgtailoringwages.jobcharge;
            command.Parameters.Add("paidcharge", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgtailoringwages.paidcharge;
            command.Parameters.Add("fromdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgtailoringwages.fromdate;
            command.Parameters.Add("todate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgtailoringwages.todate;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oMfgtailoringwages.isactive;
            command.Parameters.Add("receiptno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfgtailoringwages.receiptno == null ? "" : oMfgtailoringwages.receiptno;
            command.Parameters.Add("paidon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgtailoringwages.paidon;
            command.Parameters.Add("mfgtailoringids", NpgsqlTypes.NpgsqlDbType.Json).Value = oMfgtailoringwages.mfgtailoringids_json == null ? "null" : oMfgtailoringwages.mfgtailoringids_json;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oMfgtailoringwages.version = Convert.ToInt32(reader[0].ToString());
                }
            }

            return oMfgtailoringwages;
        }


        public bool Delete(Mfgtailoringwages oMfgtailoringwages)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oMfgtailoringwages);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oMfgtailoringwages);
            }
            return hasSuccessCount;

        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Mfgtailoringwages oMfgtailoringwages)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oMfgtailoringwages.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgtailoringwages.modifiedby;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgtailoringwages.modifiedon;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgtailoringwages.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgtailoringwages.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

    }

}
