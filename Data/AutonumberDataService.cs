using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
namespace InfyPOS.Data

{
    public class AutonumberDataService : SAASData
    {
        const string SQL_Search = @"select id, isactive, createdby, modifiedby, version, createdon, modifiedon, identityfrom,prefixexpiry,prefixexpiryon,suffixexpiry,suffixexpiryon,
    identigyto, timeout, description, prefix, suffix, currentidentity, filler, nolength, type, preincrement, companyid, locationid,dailyreset,floorid,prefixvalidfrom,suffixvalidfrom from autonumber where id=@id";


        const string SQL_Insert = @"INSERT INTO public.autonumber(isactive, createdby, modifiedby, version, createdon, modifiedon, identityfrom,prefixexpiry,prefixexpiryon,suffixexpiry,suffixexpiryon,
            identigyto, timeout, description, prefix, suffix, currentidentity, filler, nolength, type, preincrement, companyid, locationid,dailyreset,floorid,prefixvalidfrom,suffixvalidfrom)
            VALUES(@isactive, @createdby, @modifiedby, @version, @createdon, @modifiedon, @identityfrom,@prefixexpiry,@prefixexpiryon,@suffixexpiry,@suffixexpiryon,
            @identigyto, @timeout, @description, @prefix, @suffix, @currentidentity, @filler, @nolength, @type, @preincrement, @companyid, @locationid,@dailyreset,@floorid,@prefixvalidfrom,@suffixvalidfrom) RETURNING id";


        const string SQL_Update = @"UPDATE public.autonumber SET modifiedby=@modifiedby, version=@version+1, modifiedon=@modifiedon, identityfrom=@identityfrom, identigyto=@identigyto, timeout=@timeout, description=@description, prefix=@prefix, 
        suffix=@suffix, currentidentity=@currentidentity, filler=@filler, nolength=@nolength, type=@type, preincrement=@preincrement, 
        companyid=@companyid, locationid=@locationid,dailyreset=@dailyreset,floorid=@floorid,prefixexpiry=@prefixexpiry,prefixexpiryon=@prefixexpiryon,
        suffixexpiry=@suffixexpiry,suffixexpiryon=@suffixexpiryon,prefixvalidfrom=@prefixvalidfrom,suffixvalidfrom=@suffixvalidfrom
        where id=@id and version=@version RETURNING version";

        const string SQL_Delete = @"update public.autonumber set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";

        public List<Autonumber> Search(AutonumberCreteria _autonumber)
        {
            List<Autonumber> lst = new List<Autonumber>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_autonumber.id != 0)
                {
                    command = GetCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _autonumber.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("isactive", true, "=");
                    if(_autonumber.locationid>0)
                        qb.AddParameter("locationid", _autonumber.locationid, "=");
                    if (_autonumber.iswarehouse)
                        qb.AddParameter("locationid", 0, "=");
                    if (!string.IsNullOrEmpty(_autonumber.type))
                        qb.AddParameter("type", _autonumber.type, "=");
                    if(_autonumber.HasLastModifiedSearch)
                        qb.AddParameter("modifiedon", _autonumber.modifiedon, ">");
                    if(_autonumber.companyid>0)
                        qb.AddParameter("companyid", _autonumber.companyid, "=");
                    if (_autonumber.floorid > 0)
                        qb.AddParameter("floorid", _autonumber.floorid, "=");
                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Autonumber oautonumber = new Autonumber();
                        oautonumber.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        oautonumber.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        oautonumber.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        oautonumber.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        oautonumber.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        oautonumber.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        oautonumber.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        oautonumber.identityfrom = reader["identityfrom"] == DBNull.Value ? 0 : Convert.ToInt64(reader["identityfrom"]);
                        oautonumber.identigyto = reader["identigyto"] == DBNull.Value ? 0 : Convert.ToInt64(reader["identigyto"]);
                        oautonumber.timeout = reader["timeout"] == DBNull.Value ? 0 : Convert.ToInt64(reader["timeout"]);
                        oautonumber.description = reader["description"] == DBNull.Value ? "" : reader["description"].ToString();
                        oautonumber.prefix = reader["prefix"] == DBNull.Value ? "" : reader["prefix"].ToString();
                        oautonumber.suffix = reader["suffix"] == DBNull.Value ? "" : reader["suffix"].ToString();
                        oautonumber.currentidentity = reader["currentidentity"] == DBNull.Value ? 0 : Convert.ToInt64(reader["currentidentity"]);
                        oautonumber.filler = reader["filler"] == DBNull.Value ? "" : reader["filler"].ToString();
                        oautonumber.nolength = reader["nolength"] == DBNull.Value ? 0 : Convert.ToInt32(reader["nolength"]);
                        oautonumber.type = reader["type"] == DBNull.Value ? "" : reader["type"].ToString();
                        oautonumber.preincrement = reader["preincrement"] == DBNull.Value ? false : Convert.ToBoolean(reader["preincrement"]);
                        oautonumber.companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"]);
                        oautonumber.locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                        oautonumber.dailyreset = reader["dailyreset"] == DBNull.Value ? false : Convert.ToBoolean(reader["dailyreset"]);
                        oautonumber.floorid = reader["floorid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["floorid"]);

                        oautonumber.prefixexpiry = reader["prefixexpiry"] == DBNull.Value ? false : Convert.ToBoolean(reader["prefixexpiry"]);
                        oautonumber.suffixexpiry = reader["suffixexpiry"] == DBNull.Value ? false : Convert.ToBoolean(reader["suffixexpiry"]);
                        oautonumber.prefixexpiryon = reader["prefixexpiryon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["prefixexpiryon"]);
                        oautonumber.suffixexpiryon = reader["suffixexpiryon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["suffixexpiryon"]);
                        oautonumber.prefixvalidfrom = reader["prefixvalidfrom"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["prefixvalidfrom"]);
                        oautonumber.suffixvalidfrom = reader["suffixvalidfrom"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["suffixvalidfrom"]);

                        lst.Add(oautonumber);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _autonumber);
            }
            return lst;
        }

        public Autonumber Insert(Autonumber oAutonumber)
        {
            oAutonumber.identityfrom = 0;
            oAutonumber.identigyto = 100000000000;
            oAutonumber.timeout = 100;
            NpgsqlTransaction transaction = null;
            try
            {
                if (oAutonumber.type == "bill.billno")
                {
                    if (!Services.ConfigManager.Instance(Context.OrganizationID).Config.system_salefloorwisebillno)
                        oAutonumber.floorid = 0;
                    else if (oAutonumber.floorid == 0)
                        throw new Exception("Please select the floor");

                    if (!InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_multicompany)
                        oAutonumber.companyid = 0;
                    else if (oAutonumber.companyid == 0)
                        throw new Exception("Please select the company");

                    if (oAutonumber.locationid <= 0)
                        throw new Exception("Please select the location");

                    var existingbillnos = Search(new AutonumberCreteria()
                    {
                        type = oAutonumber.type
                    });
                    if(existingbillnos.Exists(e=>e.companyid == oAutonumber.companyid && e.locationid == oAutonumber.locationid &&
                    e.floorid == oAutonumber.floorid && e.prefixexpiry == true && (oAutonumber.prefixvalidfrom.Date >= e.prefixvalidfrom && oAutonumber.prefixvalidfrom < e.prefixexpiryon)))
                    {
                        throw new Exception("Can't create duplicate auto number");
                    }
                }

                if (oAutonumber.type == "billreturn.billno")
                {
                    if (!Services.ConfigManager.Instance(Context.OrganizationID).Config.system_srfloorwisebillno)
                        oAutonumber.floorid = 0;
                    else if (oAutonumber.floorid == 0)
                        throw new Exception("Please select the floor");

                    if (!InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_multicompany)
                        oAutonumber.companyid = 0;
                    else if (oAutonumber.companyid == 0)
                        throw new Exception("Please select the company");

                    if (oAutonumber.locationid <= 0)
                        throw new Exception("Please select the location");

                    var existingbillnos = Search(new AutonumberCreteria()
                    {
                        type = oAutonumber.type
                    });
                    if (existingbillnos.Exists(e => e.companyid == oAutonumber.companyid && e.locationid == oAutonumber.locationid &&
                     e.floorid == oAutonumber.floorid && e.prefixexpiry == true && (oAutonumber.prefixvalidfrom.Date >= e.prefixvalidfrom && oAutonumber.prefixvalidfrom < e.prefixexpiryon)))
                    {
                        throw new Exception("Can't create duplicate auto number");
                    }
                }

                if (oAutonumber.type == "bulkbill.billno")
                {
                    if (!Services.ConfigManager.Instance(Context.OrganizationID).Config.system_floorwisebulkbillno)
                        oAutonumber.floorid = 0;
                    else if (oAutonumber.floorid == 0)
                        throw new Exception("Please select the floor");

                    if (!InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_multicompany)
                        oAutonumber.companyid = 0;
                    else if (oAutonumber.companyid == 0)
                        throw new Exception("Please select the company");

                    if (oAutonumber.locationid <= 0)
                        throw new Exception("Please select the location");

                    var existingbillnos = Search(new AutonumberCreteria()
                    {
                        type = oAutonumber.type
                    });
                    if (existingbillnos.Exists(e => e.companyid == oAutonumber.companyid && e.locationid == oAutonumber.locationid &&
                     e.floorid == oAutonumber.floorid && e.prefixexpiry == true && (oAutonumber.prefixvalidfrom.Date >= e.prefixvalidfrom && oAutonumber.prefixvalidfrom < e.prefixexpiryon)))
                    {
                        throw new Exception("Can't create duplicate auto number");
                    }
                }

                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);

                    Insert(transaction, conn, oAutonumber);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oAutonumber);
            }

            return oAutonumber;
        }

        public Autonumber Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Autonumber oAutonumber)
        {

            NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oAutonumber.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oAutonumber.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("identityfrom", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oAutonumber.identityfrom;
            command.Parameters.Add("identigyto", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oAutonumber.identigyto;
            command.Parameters.Add("timeout", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oAutonumber.timeout;
            command.Parameters.Add("description", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oAutonumber.description == null ? "" : oAutonumber.description;
            command.Parameters.Add("prefix", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oAutonumber.prefix == null ? "" : oAutonumber.prefix;
            command.Parameters.Add("suffix", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oAutonumber.suffix == null ? "" : oAutonumber.suffix;
            command.Parameters.Add("currentidentity", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oAutonumber.currentidentity;
            command.Parameters.Add("filler", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oAutonumber.filler == null ? "" : oAutonumber.filler;
            command.Parameters.Add("nolength", NpgsqlTypes.NpgsqlDbType.Integer).Value = oAutonumber.nolength;
            command.Parameters.Add("type", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oAutonumber.type == null ? "" : oAutonumber.type;
            command.Parameters.Add("preincrement", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oAutonumber.preincrement;
            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oAutonumber.companyid;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oAutonumber.locationid;

            command.Parameters.Add("dailyreset", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oAutonumber.dailyreset;
            command.Parameters.Add("floorid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oAutonumber.floorid;

            command.Parameters.Add("prefixexpiry", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oAutonumber.prefixexpiry;
            command.Parameters.Add("suffixexpiry", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oAutonumber.suffixexpiry;
            command.Parameters.Add("prefixexpiryon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oAutonumber.prefixexpiryon;
            command.Parameters.Add("suffixexpiryon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oAutonumber.suffixexpiryon;
            command.Parameters.Add("prefixvalidfrom", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oAutonumber.prefixvalidfrom;
            command.Parameters.Add("suffixvalidfrom", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oAutonumber.suffixvalidfrom;

            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oAutonumber.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            return oAutonumber;
        }

        public Autonumber Update(Autonumber oAutonumber)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                var existing = Search(new AutonumberCreteria()
                {
                    id = oAutonumber.id
                }).First();

                if (existing.type != oAutonumber.type)
                    throw new Exception("Can't change the type, please delete recreate.");

                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log("Auto Number change - Before", WebAPI.Common.Logger.LogType.Error, existing);
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log("Auto Number change - New", WebAPI.Common.Logger.LogType.Error, oAutonumber);

                var existingList = Search(new AutonumberCreteria()
                {
                    companyid = oAutonumber.companyid,
                    floorid = oAutonumber.floorid,
                    locationid = oAutonumber.locationid,
                    type = oAutonumber.type
                });

                if (!InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_autonumberwithexpiry)
                {
                    if (existingList.Count > 1)
                        throw new Exception("Already Exist, can't create duplicate");

                    if (existingList.Count == 1 && existingList[0].id != oAutonumber.id)
                        throw new Exception("Already Exist, can't create duplicate");
                }
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oAutonumber);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oAutonumber);
            }

            return oAutonumber;
        }

        public Autonumber Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Autonumber oAutonumber)
        {

            NpgsqlCommand command = GetCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oAutonumber.id;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oAutonumber.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oAutonumber.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oAutonumber.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oAutonumber.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("identityfrom", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oAutonumber.identityfrom;
            command.Parameters.Add("identigyto", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oAutonumber.identigyto;
            command.Parameters.Add("timeout", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oAutonumber.timeout;
            command.Parameters.Add("description", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oAutonumber.description == null ? "" : oAutonumber.description;
            command.Parameters.Add("prefix", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oAutonumber.prefix == null ? "" : oAutonumber.prefix;
            command.Parameters.Add("suffix", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oAutonumber.suffix == null ? "" : oAutonumber.suffix;
            command.Parameters.Add("currentidentity", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oAutonumber.currentidentity;
            command.Parameters.Add("filler", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oAutonumber.filler == null ? "" : oAutonumber.filler;
            command.Parameters.Add("nolength", NpgsqlTypes.NpgsqlDbType.Integer).Value = oAutonumber.nolength;
            command.Parameters.Add("type", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oAutonumber.type == null ? "" : oAutonumber.type;
            command.Parameters.Add("preincrement", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oAutonumber.preincrement;
            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oAutonumber.companyid;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oAutonumber.locationid;
            command.Parameters.Add("dailyreset", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oAutonumber.dailyreset;
            command.Parameters.Add("floorid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oAutonumber.floorid;
            command.Parameters.Add("prefixexpiry", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oAutonumber.prefixexpiry;
            command.Parameters.Add("suffixexpiry", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oAutonumber.suffixexpiry;
            command.Parameters.Add("prefixexpiryon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oAutonumber.prefixexpiryon;
            command.Parameters.Add("suffixexpiryon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oAutonumber.suffixexpiryon;
            command.Parameters.Add("prefixvalidfrom", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oAutonumber.prefixvalidfrom;
            command.Parameters.Add("suffixvalidfrom", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oAutonumber.suffixvalidfrom;

            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oAutonumber.version = Convert.ToInt32(reader[0].ToString());
                }
            }

            return oAutonumber;
        }


        public bool Delete(Autonumber oAutonumber)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oAutonumber);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oAutonumber);

            }
            return hasSuccessCount;

        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Autonumber oAutonumber)
        {

            NpgsqlCommand command = GetCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oAutonumber.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oAutonumber.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oAutonumber.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

    }

}
