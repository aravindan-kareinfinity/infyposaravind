using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
namespace InfyPOS.Data

{
    public class EmischemesDataService : SAASData
    {
        const string SQL_Search = @"select id, loanproviderid, name, isactive, createdby, modifiedby, version, createdon, modifiedon, rateofinterest, minimumadvancemonth, dbdpercentage, dbdvalue, emicardcharge, documentchargepercentage, documentcharge, validfrom, validto, minimumvalue, maximumvalue,tenure from emischemes where id=@id";

        const string SQL_Insert = @"INSERT INTO public.emischemes(loanproviderid, name, isactive, createdby, modifiedby, version, createdon, modifiedon, rateofinterest, minimumadvancemonth, dbdpercentage, dbdvalue, emicardcharge, documentchargepercentage, documentcharge, validfrom, validto, minimumvalue, maximumvalue,tenure) VALUES(@loanproviderid, @name, @isactive, @createdby, @modifiedby, @version, @createdon, @modifiedon, @rateofinterest, @minimumadvancemonth, @dbdpercentage, @dbdvalue, @emicardcharge, @documentchargepercentage, @documentcharge, @validfrom, @validto, @minimumvalue, @maximumvalue,@tenure) RETURNING id";


        const string SQL_Update = @"UPDATE public.emischemes SET loanproviderid=@loanproviderid, name=@name, modifiedby=@modifiedby,tenure=@tenure, version=@version+1, modifiedon=@modifiedon, rateofinterest=@rateofinterest, minimumadvancemonth=@minimumadvancemonth, dbdpercentage=@dbdpercentage, dbdvalue=@dbdvalue, emicardcharge=@emicardcharge, documentchargepercentage=@documentchargepercentage, documentcharge=@documentcharge, validfrom=@validfrom, validto=@validto, minimumvalue=@minimumvalue, maximumvalue=@maximumvalue where id=@id and version=@version RETURNING version";

        const string SQL_Delete = @"update public.emischemes set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";

        public List<Emischemes> Search(EmischemesCreteria _emischemes)
        {
            List<Emischemes> lst = new List<Emischemes>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_emischemes.id != 0)
                {
                    command = new NpgsqlCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _emischemes.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("isactive", true, "=");

                    if (_emischemes.loanproviderid>0)
                        qb.AddParameter("loanproviderid", _emischemes.loanproviderid, "=");

                    if (_emischemes.validfrom > DateTime.MinValue)
                        qb.AddParameter("fromdate", _emischemes.validfrom, ">=");

                    if (_emischemes.validto > DateTime.MinValue)
                        qb.AddParameter("todate", _emischemes.validto.AddDays(1), "<");

                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Emischemes oemischemes = new Emischemes();
                        oemischemes.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        oemischemes.loanproviderid = reader["loanproviderid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["loanproviderid"]);
                        oemischemes.name = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
                        oemischemes.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        oemischemes.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        oemischemes.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        oemischemes.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        oemischemes.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        oemischemes.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        oemischemes.rateofinterest = reader["rateofinterest"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["rateofinterest"]);
                        oemischemes.minimumadvancemonth = reader["minimumadvancemonth"] == DBNull.Value ? 0 : Convert.ToInt32(reader["minimumadvancemonth"]);
                        oemischemes.tenure = reader["tenure"] == DBNull.Value ? 0 : Convert.ToInt32(reader["tenure"]);
                        oemischemes.dbdpercentage = reader["dbdpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["dbdpercentage"]);
                        oemischemes.dbdvalue = reader["dbdvalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["dbdvalue"]);
                        oemischemes.emicardcharge = reader["emicardcharge"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["emicardcharge"]);
                        oemischemes.documentchargepercentage = reader["documentchargepercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["documentchargepercentage"]);
                        oemischemes.documentcharge = reader["documentcharge"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["documentcharge"]);
                        oemischemes.validfrom = reader["validfrom"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["validfrom"]);
                        oemischemes.validto = reader["validto"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["validto"]);
                        oemischemes.minimumvalue = reader["minimumvalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["minimumvalue"]);
                        oemischemes.maximumvalue = reader["maximumvalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["maximumvalue"]);
                        lst.Add(oemischemes);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _emischemes);
            }
            return lst;
        }

        public Emischemes Insert(Emischemes oEmischemes)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);

                    Insert(transaction, conn, oEmischemes);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oEmischemes);
            }

            return oEmischemes;
        }

        public Emischemes Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Emischemes oEmischemes)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("loanproviderid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oEmischemes.loanproviderid;
            command.Parameters.Add("name", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oEmischemes.name == null ? "" : oEmischemes.name;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oEmischemes.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oEmischemes.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oEmischemes.modifiedby;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oEmischemes.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oEmischemes.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oEmischemes.modifiedon;
            command.Parameters.Add("rateofinterest", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oEmischemes.rateofinterest;
            command.Parameters.Add("minimumadvancemonth", NpgsqlTypes.NpgsqlDbType.Integer).Value = oEmischemes.minimumadvancemonth;
            command.Parameters.Add("tenure", NpgsqlTypes.NpgsqlDbType.Integer).Value = oEmischemes.tenure;
            command.Parameters.Add("dbdpercentage", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oEmischemes.dbdpercentage;
            command.Parameters.Add("dbdvalue", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oEmischemes.dbdvalue;
            command.Parameters.Add("emicardcharge", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oEmischemes.emicardcharge;
            command.Parameters.Add("documentchargepercentage", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oEmischemes.documentchargepercentage;
            command.Parameters.Add("documentcharge", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oEmischemes.documentcharge;
            command.Parameters.Add("validfrom", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oEmischemes.validfrom;
            command.Parameters.Add("validto", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oEmischemes.validto;
            command.Parameters.Add("minimumvalue", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oEmischemes.minimumvalue;
            command.Parameters.Add("maximumvalue", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oEmischemes.maximumvalue;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oEmischemes.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            return oEmischemes;
        }

        public Emischemes Update(Emischemes oEmischemes)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oEmischemes);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oEmischemes);
            }

            return oEmischemes;
        }

        public Emischemes Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Emischemes oEmischemes)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oEmischemes.id;
            command.Parameters.Add("loanproviderid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oEmischemes.loanproviderid;
            command.Parameters.Add("name", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oEmischemes.name == null ? "" : oEmischemes.name;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oEmischemes.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oEmischemes.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oEmischemes.modifiedby;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oEmischemes.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oEmischemes.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oEmischemes.modifiedon;
            command.Parameters.Add("rateofinterest", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oEmischemes.rateofinterest;
            command.Parameters.Add("minimumadvancemonth", NpgsqlTypes.NpgsqlDbType.Integer).Value = oEmischemes.minimumadvancemonth;
            command.Parameters.Add("dbdpercentage", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oEmischemes.dbdpercentage;
            command.Parameters.Add("dbdvalue", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oEmischemes.dbdvalue;
            command.Parameters.Add("emicardcharge", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oEmischemes.emicardcharge;
            command.Parameters.Add("documentchargepercentage", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oEmischemes.documentchargepercentage;
            command.Parameters.Add("documentcharge", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oEmischemes.documentcharge;
            command.Parameters.Add("validfrom", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oEmischemes.validfrom;
            command.Parameters.Add("validto", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oEmischemes.validto;
            command.Parameters.Add("minimumvalue", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oEmischemes.minimumvalue;
            command.Parameters.Add("maximumvalue", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oEmischemes.maximumvalue;
            command.Parameters.Add("tenure", NpgsqlTypes.NpgsqlDbType.Integer).Value = oEmischemes.tenure;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oEmischemes.version = Convert.ToInt32(reader[0].ToString());
                }
            }

            return oEmischemes;
        }


        public bool Delete(Emischemes oEmischemes)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oEmischemes);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oEmischemes);
            }
            return hasSuccessCount;

        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Emischemes oEmischemes)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oEmischemes.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oEmischemes.modifiedby;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oEmischemes.modifiedon;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oEmischemes.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oEmischemes.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

    }

}
