using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
namespace InfyPOS.Data

{
    public class TailoringdesignDataService : BaseData
    {
        const string SQL_Search = @"select id, productid, designcode, designname, specialnodes, imagelist, measurementconfig, version, isactive, modifiedby, createdby, modifiedon, createdon from tailoringdesign where id=@id";


        const string SQL_Insert = @"INSERT INTO public.tailoringdesign(productid, designcode, designname, specialnodes, imagelist, measurementconfig, version, isactive, modifiedby, createdby, modifiedon, createdon) VALUES(@productid, @designcode, @designname, @specialnodes, @imagelist, @measurementconfig, @version, @isactive, @modifiedby, @createdby, @modifiedon, @createdon) RETURNING id";


        const string SQL_Update = @"UPDATE public.tailoringdesign SET productid=@productid, designcode=@designcode, designname=@designname, specialnodes=@specialnodes, imagelist=@imagelist, measurementconfig=@measurementconfig, version=@version+1, modifiedby=@modifiedby, modifiedon=@modifiedon where id=@id and version=@version RETURNING version";

        const string SQL_Delete = @"update public.tailoringdesign set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";

        public List<Tailoringdesign> Search(TailoringdesignCreteria _tailoringdesign)
        {
            List<Tailoringdesign> lst = new List<Tailoringdesign>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_tailoringdesign.id != 0)
                {
                    command = new NpgsqlCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _tailoringdesign.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("isactive", true, "=");

                    //if (!string.IsNullOrEmpty(_tailoringdesign.Code))
                    //    qb.AddParameter("code", _tailoringdesign.Code, "=");

                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Tailoringdesign otailoringdesign = new Tailoringdesign();
                        otailoringdesign.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        otailoringdesign.productid = reader["productid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productid"]);
                        otailoringdesign.designcode = reader["designcode"] == DBNull.Value ? "" : reader["designcode"].ToString();
                        otailoringdesign.designname = reader["designname"] == DBNull.Value ? "" : reader["designname"].ToString();
                        otailoringdesign.specialnodes = reader["specialnodes"] == DBNull.Value ? "" : reader["specialnodes"].ToString();
                        otailoringdesign.imagelist_json = reader["imagelist"] == DBNull.Value ? null : reader["imagelist"].ToString();
                        otailoringdesign.measurementconfig_json = reader["measurementconfig"] == DBNull.Value ? null : reader["measurementconfig"].ToString();
                        otailoringdesign.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        otailoringdesign.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        otailoringdesign.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        otailoringdesign.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        otailoringdesign.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        otailoringdesign.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        lst.Add(otailoringdesign);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _tailoringdesign);
            }
            return lst;
        }

        public Tailoringdesign Insert(Tailoringdesign oTailoringdesign)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                var sequence = SequenceManager.Instance(Context.OrganizationID).Get("tailoringdesign.designcode", 0);
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);
                    oTailoringdesign.designcode = sequence.GetNextNumber(Context, conn, transaction);

                    Insert(transaction, conn, oTailoringdesign);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oTailoringdesign);
            }

            return oTailoringdesign;
        }

        public Tailoringdesign Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Tailoringdesign oTailoringdesign)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("productid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTailoringdesign.productid;
            command.Parameters.Add("designcode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oTailoringdesign.designcode == null ? "" : oTailoringdesign.designcode;
            command.Parameters.Add("designname", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oTailoringdesign.designname == null ? "" : oTailoringdesign.designname;
            command.Parameters.Add("specialnodes", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oTailoringdesign.specialnodes == null ? "" : oTailoringdesign.specialnodes;
            command.Parameters.Add("imagelist", NpgsqlTypes.NpgsqlDbType.Json).Value = oTailoringdesign.imagelist_json == null ? "null" : oTailoringdesign.imagelist_json;
            command.Parameters.Add("measurementconfig", NpgsqlTypes.NpgsqlDbType.Json).Value = oTailoringdesign.measurementconfig_json == null ? "null" : oTailoringdesign.measurementconfig_json;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oTailoringdesign.version;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oTailoringdesign.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTailoringdesign.modifiedby;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTailoringdesign.createdby;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oTailoringdesign.modifiedon;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oTailoringdesign.createdon;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oTailoringdesign.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            return oTailoringdesign;
        }

        public Tailoringdesign Update(Tailoringdesign oTailoringdesign)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oTailoringdesign);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oTailoringdesign);
            }

            return oTailoringdesign;
        }

        public Tailoringdesign Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Tailoringdesign oTailoringdesign)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTailoringdesign.id;
            command.Parameters.Add("productid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTailoringdesign.productid;
            command.Parameters.Add("designcode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oTailoringdesign.designcode == null ? "" : oTailoringdesign.designcode;
            command.Parameters.Add("designname", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oTailoringdesign.designname == null ? "" : oTailoringdesign.designname;
            command.Parameters.Add("specialnodes", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oTailoringdesign.specialnodes == null ? "" : oTailoringdesign.specialnodes;
            command.Parameters.Add("imagelist", NpgsqlTypes.NpgsqlDbType.Json).Value = oTailoringdesign.imagelist_json == null ? "null" : oTailoringdesign.imagelist_json;
            command.Parameters.Add("measurementconfig", NpgsqlTypes.NpgsqlDbType.Json).Value = oTailoringdesign.measurementconfig_json == null ? "null" : oTailoringdesign.measurementconfig_json;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oTailoringdesign.version;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oTailoringdesign.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTailoringdesign.modifiedby;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTailoringdesign.createdby;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oTailoringdesign.modifiedon;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oTailoringdesign.createdon;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oTailoringdesign.version = Convert.ToInt32(reader[0].ToString());
                }
            }

            return oTailoringdesign;
        }


        public bool Delete(Tailoringdesign oTailoringdesign)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oTailoringdesign);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oTailoringdesign);

            }
            return hasSuccessCount;

        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Tailoringdesign oTailoringdesign)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oTailoringdesign.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTailoringdesign.modifiedby;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oTailoringdesign.modifiedon;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oTailoringdesign.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTailoringdesign.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

    }

}
