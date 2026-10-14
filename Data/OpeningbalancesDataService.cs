using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
namespace InfyPOS.Data

{
    public class OpeningbalancesDataService : SAASData
    {
        const string SQL_Search = @"select isactive, createdon, modifiedon, createdby, version, modifiedby, id, locationid, companyid, ledgertype, ledgerid, financeyear, opening, closing, obsource, ledgername from openingbalances where id=@id";


        const string SQL_Insert = @"INSERT INTO public.openingbalances(isactive, createdon, modifiedon, createdby, version, modifiedby, locationid, companyid, ledgertype, ledgerid, financeyear, opening, closing, obsource, ledgername) VALUES(@isactive, @createdon, @modifiedon, @createdby, @version, @modifiedby, @locationid, @companyid, @ledgertype, @ledgerid, @financeyear, @opening, @closing, @obsource, @ledgername) RETURNING id";


        const string SQL_Update = @"UPDATE public.openingbalances SET modifiedon=@modifiedon, version=@version+1, modifiedby=@modifiedby, locationid=@locationid, companyid=@companyid, ledgertype=@ledgertype, ledgerid=@ledgerid, financeyear=@financeyear, opening=@opening, closing=@closing, obsource=@obsource, ledgername=@ledgername where id=@id and version=@version RETURNING version";

        const string SQL_Delete = @"update public.openingbalances set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";

        public List<Openingbalances> Search(OpeningbalancesCreteria _openingbalances)
        {
            List<Openingbalances> lst = new List<Openingbalances>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_openingbalances.id != 0)
                {
                    command = new NpgsqlCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _openingbalances.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("isactive", true, "=");

                    //if (!string.IsNullOrEmpty(_openingbalances.Code))
                    //    qb.AddParameter("code", _openingbalances.Code, "=");

                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Openingbalances oopeningbalances = new Openingbalances();
                        oopeningbalances.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        oopeningbalances.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        oopeningbalances.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        oopeningbalances.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        oopeningbalances.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        oopeningbalances.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        oopeningbalances.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        oopeningbalances.locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                        oopeningbalances.companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"]);
                        oopeningbalances.ledgertype = reader["ledgertype"] == DBNull.Value ? 0 : Convert.ToInt32(reader["ledgertype"]);
                        oopeningbalances.ledgerid = reader["ledgerid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["ledgerid"]);
                        oopeningbalances.financeyear = reader["financeyear"] == DBNull.Value ? 0 : Convert.ToInt32(reader["financeyear"]);
                        oopeningbalances.opening = reader["opening"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["opening"]);
                        oopeningbalances.closing = reader["closing"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["closing"]);
                        oopeningbalances.obsource = reader["obsource"] == DBNull.Value ? 0 : Convert.ToInt32(reader["obsource"]);
                        oopeningbalances.ledgername = reader["ledgername"] == DBNull.Value ? "" : reader["ledgername"].ToString();
                        lst.Add(oopeningbalances);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _openingbalances);
            }
            return lst;
        }

        public Openingbalances Insert(Openingbalances oOpeningbalances)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);

                    Insert(transaction, conn, oOpeningbalances);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oOpeningbalances);
                throw exp;
            }

            return oOpeningbalances;
        }

        public Openingbalances Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Openingbalances oOpeningbalances)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oOpeningbalances.isactive;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oOpeningbalances.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oOpeningbalances.modifiedon;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oOpeningbalances.createdby;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oOpeningbalances.version;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oOpeningbalances.modifiedby;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oOpeningbalances.locationid;
            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oOpeningbalances.companyid;
            command.Parameters.Add("ledgertype", NpgsqlTypes.NpgsqlDbType.Integer).Value = oOpeningbalances.ledgertype;
            command.Parameters.Add("ledgerid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oOpeningbalances.ledgerid;
            command.Parameters.Add("financeyear", NpgsqlTypes.NpgsqlDbType.Integer).Value = oOpeningbalances.financeyear;
            command.Parameters.Add("opening", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oOpeningbalances.opening;
            command.Parameters.Add("closing", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oOpeningbalances.closing;
            command.Parameters.Add("obsource", NpgsqlTypes.NpgsqlDbType.Integer).Value = oOpeningbalances.obsource;
            command.Parameters.Add("ledgername", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oOpeningbalances.ledgername == null ? "" : oOpeningbalances.ledgername;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oOpeningbalances.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            return oOpeningbalances;
        }

        public Openingbalances Update(Openingbalances oOpeningbalances)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oOpeningbalances);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oOpeningbalances);
                throw exp;
            }

            return oOpeningbalances;
        }

        public Openingbalances Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Openingbalances oOpeningbalances)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oOpeningbalances.isactive;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oOpeningbalances.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oOpeningbalances.modifiedon;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oOpeningbalances.createdby;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oOpeningbalances.version;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oOpeningbalances.modifiedby;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oOpeningbalances.id;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oOpeningbalances.locationid;
            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oOpeningbalances.companyid;
            command.Parameters.Add("ledgertype", NpgsqlTypes.NpgsqlDbType.Integer).Value = oOpeningbalances.ledgertype;
            command.Parameters.Add("ledgerid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oOpeningbalances.ledgerid;
            command.Parameters.Add("financeyear", NpgsqlTypes.NpgsqlDbType.Integer).Value = oOpeningbalances.financeyear;
            command.Parameters.Add("opening", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oOpeningbalances.opening;
            command.Parameters.Add("closing", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oOpeningbalances.closing;
            command.Parameters.Add("obsource", NpgsqlTypes.NpgsqlDbType.Integer).Value = oOpeningbalances.obsource;
            command.Parameters.Add("ledgername", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oOpeningbalances.ledgername == null ? "" : oOpeningbalances.ledgername;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oOpeningbalances.version = Convert.ToInt32(reader[0].ToString());
                }
            }

            return oOpeningbalances;
        }


        public bool Delete(Openingbalances oOpeningbalances)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oOpeningbalances);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oOpeningbalances);

                throw exp;
            }
            return hasSuccessCount;

        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Openingbalances oOpeningbalances)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oOpeningbalances.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oOpeningbalances.modifiedby;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oOpeningbalances.modifiedon;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oOpeningbalances.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oOpeningbalances.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

    }

}
