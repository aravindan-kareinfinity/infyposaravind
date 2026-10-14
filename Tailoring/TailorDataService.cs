using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
namespace InfyPOS.Data

{
    public class TailorDataService : BaseData
    {
        const string SQL_Search = @"select config, cityid, stateid, countryid, isactive, createdby, modifiedby, version, createdon, modifiedon, id, name, contactno, address, bankaccountname, bankifci, bankaccountno, pincode, emailid, gst, bankbranch from tailor where id=@id";


        const string SQL_Insert = @"INSERT INTO public.tailor(config, cityid, stateid, countryid, isactive, createdby, modifiedby, version, createdon, modifiedon, name, contactno, address, bankaccountname, bankifci, bankaccountno, pincode, emailid, gst, bankbranch) VALUES(@config, @cityid, @stateid, @countryid, @isactive, @createdby, @modifiedby, @version, @createdon, @modifiedon, @name, @contactno, @address, @bankaccountname, @bankifci, @bankaccountno, @pincode, @emailid, @gst, @bankbranch) RETURNING id";


        const string SQL_Update = @"UPDATE public.tailor SET config=@config, cityid=@cityid, stateid=@stateid, countryid=@countryid, modifiedby=@modifiedby, version=@version+1, modifiedon=@modifiedon, name=@name, contactno=@contactno, address=@address, bankaccountname=@bankaccountname, bankifci=@bankifci, bankaccountno=@bankaccountno, pincode=@pincode, emailid=@emailid, gst=@gst, bankbranch=@bankbranch where id=@id and version=@version RETURNING version";

        const string SQL_Delete = @"update public.tailor set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";

        public List<Tailor> Search(TailorCreteria _tailor)
        {
            List<Tailor> lst = new List<Tailor>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_tailor.id != 0)
                {
                    command = new NpgsqlCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _tailor.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("isactive", true, "=");

                    //if (!string.IsNullOrEmpty(_tailor.Code))
                    //    qb.AddParameter("code", _tailor.Code, "=");

                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Tailor otailor = new Tailor();
                        otailor.config_json = reader["config"] == DBNull.Value ? null : reader["config"].ToString();
                        otailor.cityid = reader["cityid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["cityid"]);
                        otailor.stateid = reader["stateid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["stateid"]);
                        otailor.countryid = reader["countryid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["countryid"]);
                        otailor.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        otailor.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        otailor.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        otailor.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        otailor.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        otailor.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        otailor.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        otailor.name = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
                        otailor.contactno = reader["contactno"] == DBNull.Value ? "" : reader["contactno"].ToString();
                        otailor.address = reader["address"] == DBNull.Value ? "" : reader["address"].ToString();
                        otailor.bankaccountname = reader["bankaccountname"] == DBNull.Value ? "" : reader["bankaccountname"].ToString();
                        otailor.bankifci = reader["bankifci"] == DBNull.Value ? "" : reader["bankifci"].ToString();
                        otailor.bankaccountno = reader["bankaccountno"] == DBNull.Value ? "" : reader["bankaccountno"].ToString();
                        otailor.pincode = reader["pincode"] == DBNull.Value ? "" : reader["pincode"].ToString();
                        otailor.emailid = reader["emailid"] == DBNull.Value ? "" : reader["emailid"].ToString();
                        otailor.gst = reader["gst"] == DBNull.Value ? "" : reader["gst"].ToString();
                        otailor.bankbranch = reader["bankbranch"] == DBNull.Value ? "" : reader["bankbranch"].ToString();
                        lst.Add(otailor);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _tailor);
            }
            return lst;
        }

        public Tailor Insert(Tailor oTailor)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);

                    Insert(transaction, conn, oTailor);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oTailor);
                if (transaction != null)
                    transaction.Rollback();
            }

            return oTailor;
        }

        public Tailor Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Tailor oTailor)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("config", NpgsqlTypes.NpgsqlDbType.Json).Value = oTailor.config_json == null ? "null" : oTailor.config_json;
            command.Parameters.Add("cityid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTailor.cityid;
            command.Parameters.Add("stateid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTailor.stateid;
            command.Parameters.Add("countryid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTailor.countryid;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oTailor.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTailor.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTailor.modifiedby;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oTailor.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oTailor.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oTailor.modifiedon;
            command.Parameters.Add("name", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oTailor.name == null ? "" : oTailor.name;
            command.Parameters.Add("contactno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oTailor.contactno == null ? "" : oTailor.contactno;
            command.Parameters.Add("address", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oTailor.address == null ? "" : oTailor.address;
            command.Parameters.Add("bankaccountname", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oTailor.bankaccountname == null ? "" : oTailor.bankaccountname;
            command.Parameters.Add("bankifci", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oTailor.bankifci == null ? "" : oTailor.bankifci;
            command.Parameters.Add("bankaccountno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oTailor.bankaccountno == null ? "" : oTailor.bankaccountno;
            command.Parameters.Add("pincode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oTailor.pincode == null ? "" : oTailor.pincode;
            command.Parameters.Add("emailid", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oTailor.emailid == null ? "" : oTailor.emailid;
            command.Parameters.Add("gst", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oTailor.gst == null ? "" : oTailor.gst;
            command.Parameters.Add("bankbranch", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oTailor.bankbranch == null ? "" : oTailor.bankbranch;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oTailor.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            return oTailor;
        }

        public Tailor Update(Tailor oTailor)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oTailor);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oTailor);
                if (transaction != null)
                    transaction.Rollback();
            }

            return oTailor;
        }

        public Tailor Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Tailor oTailor)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("config", NpgsqlTypes.NpgsqlDbType.Json).Value = oTailor.config_json == null ? "null" : oTailor.config_json;
            command.Parameters.Add("cityid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTailor.cityid;
            command.Parameters.Add("stateid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTailor.stateid;
            command.Parameters.Add("countryid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTailor.countryid;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oTailor.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTailor.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTailor.modifiedby;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oTailor.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oTailor.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oTailor.modifiedon;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTailor.id;
            command.Parameters.Add("name", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oTailor.name == null ? "" : oTailor.name;
            command.Parameters.Add("contactno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oTailor.contactno == null ? "" : oTailor.contactno;
            command.Parameters.Add("address", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oTailor.address == null ? "" : oTailor.address;
            command.Parameters.Add("bankaccountname", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oTailor.bankaccountname == null ? "" : oTailor.bankaccountname;
            command.Parameters.Add("bankifci", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oTailor.bankifci == null ? "" : oTailor.bankifci;
            command.Parameters.Add("bankaccountno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oTailor.bankaccountno == null ? "" : oTailor.bankaccountno;
            command.Parameters.Add("pincode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oTailor.pincode == null ? "" : oTailor.pincode;
            command.Parameters.Add("emailid", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oTailor.emailid == null ? "" : oTailor.emailid;
            command.Parameters.Add("gst", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oTailor.gst == null ? "" : oTailor.gst;
            command.Parameters.Add("bankbranch", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oTailor.bankbranch == null ? "" : oTailor.bankbranch;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oTailor.version = Convert.ToInt32(reader[0].ToString());
                }
            }

            return oTailor;
        }


        public bool Delete(Tailor oTailor)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oTailor);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oTailor);

                if (transaction != null)
                    transaction.Rollback();
            }
            return hasSuccessCount;

        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Tailor oTailor)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oTailor.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTailor.modifiedby;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.TimestampTZ).Value = oTailor.modifiedon;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oTailor.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTailor.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

    }

}
