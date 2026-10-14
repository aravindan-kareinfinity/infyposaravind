using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
namespace InfyPOS.Data
{
    public class BrsstatusDataService : BaseData
    {
        const string SQL_Search = @"select id, transactiontype, bankid, companyid, refid, status, realisationon, isactive, createdby, modifiedby, version, createdon, modifiedon from brsstatus where id=@id";

        const string SQL_Insert = @"INSERT INTO public.brsstatus(transactiontype, bankid, companyid, refid, status, realisationon, isactive, createdby, modifiedby, version, createdon, modifiedon) VALUES(@transactiontype, @bankid, @companyid, @refid, @status, @realisationon, @isactive, @createdby, @modifiedby, @version, @createdon, @modifiedon) RETURNING id";

        const string SQL_Update = @"UPDATE public.brsstatus SET transactiontype=@transactiontype, bankid=@bankid, companyid=@companyid, refid=@refid, status=@status, realisationon=@realisationon, modifiedby=@modifiedby, version=@version+1, modifiedon=@modifiedon where id=@id and version=@version RETURNING version";
        
        const string SQL_UpdateStatus = @"UPDATE public.brsstatus SET  status=@status, realisationon=@realisationon, modifiedby=@modifiedby, version=@version+1, modifiedon=@modifiedon where id=@id RETURNING version";

        const string SQL_Delete = @"UPDATE brsstatus SET ISACTIVE=false where id=@id";

        public List<Brsstatus> Search(BrsstatusCreteria _brsstatus)
        {
            List<Brsstatus> lst = new List<Brsstatus>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_brsstatus.id != 0)
                {
                    command = new NpgsqlCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _brsstatus.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("isactive", true, "=");

                    //if (!string.IsNullOrEmpty(_brsstatus.Code))
                    //    qb.AddParameter("code", _brsstatus.Code, "=");

                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Brsstatus obrsstatus = new Brsstatus();
                        obrsstatus.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        obrsstatus.transactiontype = reader["transactiontype"] == DBNull.Value ? 0 : Convert.ToInt32(reader["transactiontype"]);
                        obrsstatus.bankid = reader["bankid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["bankid"]);
                        obrsstatus.companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"]);
                        obrsstatus.refid = reader["refid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["refid"]);
                        obrsstatus.status = reader["status"] == DBNull.Value ? 0 : Convert.ToInt32(reader["status"]);
                        obrsstatus.realisationon = reader["realisationon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["realisationon"]);
                        obrsstatus.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        obrsstatus.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        obrsstatus.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        obrsstatus.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        obrsstatus.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        obrsstatus.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        lst.Add(obrsstatus);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _brsstatus);
            }
            return lst;
        }

        public Brsstatus Insert(Brsstatus oBrsstatus)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);

                    Insert(transaction, conn, oBrsstatus);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oBrsstatus);
            }

            return oBrsstatus;
        }

        public Brsstatus Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Brsstatus oBrsstatus)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("transactiontype", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBrsstatus.transactiontype;
            command.Parameters.Add("bankid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBrsstatus.bankid;
            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBrsstatus.companyid;
            command.Parameters.Add("refid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBrsstatus.refid;
            command.Parameters.Add("status", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBrsstatus.status;
            command.Parameters.Add("realisationon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBrsstatus.realisationon;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBrsstatus.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBrsstatus.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBrsstatus.modifiedby;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBrsstatus.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBrsstatus.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBrsstatus.modifiedon;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oBrsstatus.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            return oBrsstatus;
        }

        public Brsstatus Update(Brsstatus oBrsstatus)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oBrsstatus);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oBrsstatus);
            }

            return oBrsstatus;
        }

        public Brsstatus UpdateStatus(NpgsqlTransaction transaction, NpgsqlConnection conn, Brsstatus oBrsstatus)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_UpdateStatus, conn, transaction);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBrsstatus.id;
            command.Parameters.Add("transactiontype", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBrsstatus.transactiontype;
            command.Parameters.Add("bankid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBrsstatus.bankid;
            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBrsstatus.companyid;
            command.Parameters.Add("refid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBrsstatus.refid;
            command.Parameters.Add("status", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBrsstatus.status;
            command.Parameters.Add("realisationon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBrsstatus.realisationon;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBrsstatus.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBrsstatus.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBrsstatus.modifiedby;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBrsstatus.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBrsstatus.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBrsstatus.modifiedon;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oBrsstatus.version = Convert.ToInt32(reader[0].ToString());
                }
            }

            return oBrsstatus;
        }
        public Brsstatus Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Brsstatus oBrsstatus)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBrsstatus.id;
            command.Parameters.Add("transactiontype", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBrsstatus.transactiontype;
            command.Parameters.Add("bankid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBrsstatus.bankid;
            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBrsstatus.companyid;
            command.Parameters.Add("refid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBrsstatus.refid;
            command.Parameters.Add("status", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBrsstatus.status;
            command.Parameters.Add("realisationon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBrsstatus.realisationon;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBrsstatus.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBrsstatus.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBrsstatus.modifiedby;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBrsstatus.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBrsstatus.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBrsstatus.modifiedon;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oBrsstatus.version = Convert.ToInt32(reader[0].ToString());
                }
            }

            return oBrsstatus;
        }


        public bool Delete(Brsstatus oBrsstatus)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oBrsstatus);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oBrsstatus);
            }
            return hasSuccessCount;

        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Brsstatus oBrsstatus)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBrsstatus.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBrsstatus.modifiedby;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.TimestampTZ).Value = oBrsstatus.modifiedon;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBrsstatus.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBrsstatus.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

    }

}
