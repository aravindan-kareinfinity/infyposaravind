using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
namespace InfyPOS.Data

{
    public class SKUMasterDataService : SAASData
    {
        const string SQL_Search = @"select id,code,name, printingname,  productid , brandid ,sleeveid ,fitid,seasonid,
typeid,styleid,uomid, colorid, pack, patternid ,materialid,sizegroup ,
isactive, createdby, modifiedby, version,createdon, 
modifiedon from SKUmaster where SKUMaster.id=@id";


        const string SQL_Insert = @"INSERT INTO public.SKUMaster(name, code,printingname,  productid , brandid ,sleeveid ,fitid,seasonid,
typeid,styleid,uomid, colorid, pack, patternid ,materialid,sizegroup ,
isactive, createdby, modifiedby, version,createdon, 
modifiedon) 
VALUES(@name, @printingname,  @productid , @brandid ,@sleeveid ,@fitid,@seasonid,
@typeid,@styleid,@uomid, @colorid, @pack, @patternid ,@materialid,@sizegroup ,
@isactive, @createdby, @modifiedby, @version,@createdon, 
@modifiedon) RETURNING id";


        const string SQL_Update = @"UPDATE public.SKUMaster SET name=@name,code=@code, printingname=@printingname,  productid=@productid , 
brandid=@brandid ,sleeveid=@sleeveid ,fitid=@fitid,seasonid=@seasonid,
typeid=@typeid,styleid=@styleid, uomid=@uomid, colorid=@colorid, pack=@pack, patternid=@patternid ,materialid=@materialid,sizegroup=@sizegroup,
isactive=@isactive, modifiedby=@modifiedby, version=@version+1, 
modifiedon=@modifiedon where id=@id and version=@version  RETURNING version";

        const string SQL_Delete = @"update public.SKUMaster set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version  RETURNING version";

        public List<SKUMaster> Search(SKUMasterCreteria _SKUMaster)
        {
            List<SKUMaster> lst = new List<SKUMaster>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_SKUMaster.id != 0)
                {
                    command = GetCommand(SQL_Search, conn);
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));

                    qb.AddParameter("isactive", true, "=");
                    command = qb.GetCommand(conn);

                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        SKUMaster oSKUMaster = new SKUMaster();
                        oSKUMaster.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        oSKUMaster.code = reader["code"] == DBNull.Value ? "" : reader["code"].ToString();
                        oSKUMaster.name = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
                        oSKUMaster.printingname = reader["printingname"] == DBNull.Value ? "" : reader["printingname"].ToString();
                        oSKUMaster.productid = reader["productid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productid"]);
                        oSKUMaster.brandid = reader["brandid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["brandid"]);
                        oSKUMaster.fitid = reader["fitid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["fitid"]);
                        oSKUMaster.sleeveid = reader["sleeveid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sleeveid"]);
                        //oSKUMaster.styleid = reader["styleid "] == DBNull.Value ? 0 : Convert.ToInt64(reader["styleid"]);
                        oSKUMaster.typeid = reader["typeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["typeid"]);
                        oSKUMaster.pack = reader["pack"] == DBNull.Value ? "" : reader["pack"].ToString();
                        oSKUMaster.materialid = reader["materialid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["materialid"]);
                        oSKUMaster.uomid = reader["uomid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["uomid"]);
                        oSKUMaster.patternid = reader["patternid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["patternid"]);
                        oSKUMaster.colorid = reader["colorid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["colorid"]);
                        oSKUMaster.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        oSKUMaster.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        oSKUMaster.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        oSKUMaster.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"].ToString());
                        oSKUMaster.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        oSKUMaster.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        oSKUMaster.sizegroup_json = reader["sizegroup"] == DBNull.Value ? "null" : reader["sizegroup"].ToString();

                        lst.Add(oSKUMaster);
                    }
                }

                conn.Close();
            }


            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);
            }
            return lst;
        }

        public SKUMaster Insert(SKUMaster oSKUMaster)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);

                    Insert(transaction, conn, oSKUMaster);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {

                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);
            }

            return oSKUMaster;
        }

        public SKUMaster Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, SKUMaster oSKUMaster)
        {

            NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("code", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSKUMaster.code == null ? "" : oSKUMaster.code;
            command.Parameters.Add("name", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSKUMaster.name == null ? "" : oSKUMaster.name;
            command.Parameters.Add("printingname", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSKUMaster.printingname == null ? "" : oSKUMaster.printingname;
            command.Parameters.Add("productid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSKUMaster.productid;
            command.Parameters.Add("brandid ", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSKUMaster.brandid;
            command.Parameters.Add("sleeveid ", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSKUMaster.sleeveid;
            command.Parameters.Add("fitid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSKUMaster.fitid;
            command.Parameters.Add("seasonid ", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSKUMaster.seasonid;
            command.Parameters.Add("typeid ", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSKUMaster.typeid;
            command.Parameters.Add("styleid  ", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSKUMaster.styleid;
            command.Parameters.Add("patternid  ", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSKUMaster.patternid;
            command.Parameters.Add("materialid  ", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSKUMaster.materialid;
            command.Parameters.Add("uomid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSKUMaster.uomid;
            command.Parameters.Add("colorid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSKUMaster.colorid;
            command.Parameters.Add("pack", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSKUMaster.pack == null ? "" : oSKUMaster.pack;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = (Services.ConfigManager.Instance(Context.OrganizationID).Config == null) ? oSKUMaster.isactive : false;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oSKUMaster.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("sizegroup", NpgsqlTypes.NpgsqlDbType.Json).Value = oSKUMaster.sizegroup_json == null ? "null" : oSKUMaster.sizegroup_json;

            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oSKUMaster.id = Convert.ToInt64(reader[0].ToString());
                }
            }

            return oSKUMaster;
        }

        public SKUMaster Update(SKUMaster oSKUMaster)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oSKUMaster);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {

                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);
            }

            return oSKUMaster;
        }

        public SKUMaster Update(NpgsqlTransaction transaction, NpgsqlConnection conn, SKUMaster oSKUMaster)
        {

            NpgsqlCommand command = GetCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSKUMaster.id;
            command.Parameters.Add("code", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSKUMaster.code == null ? "" : oSKUMaster.code;
            command.Parameters.Add("name", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSKUMaster.name == null ? "" : oSKUMaster.name;
            command.Parameters.Add("printingname", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSKUMaster.printingname == null ? "" : oSKUMaster.printingname;
            command.Parameters.Add("productid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSKUMaster.productid;
            command.Parameters.Add("brandid ", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSKUMaster.brandid;
            command.Parameters.Add("sleeveid ", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSKUMaster.sleeveid;
            command.Parameters.Add("fitid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSKUMaster.fitid;
            command.Parameters.Add("seasonid ", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSKUMaster.seasonid;
            command.Parameters.Add("typeid ", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSKUMaster.typeid;
            command.Parameters.Add("styleid  ", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSKUMaster.styleid;
            command.Parameters.Add("patternid  ", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSKUMaster.patternid;
            command.Parameters.Add("materialid  ", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSKUMaster.materialid;
            command.Parameters.Add("uomid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSKUMaster.uomid;
            command.Parameters.Add("colorid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSKUMaster.colorid;
            command.Parameters.Add("pack", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSKUMaster.pack == null ? "" : oSKUMaster.pack;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = (Services.ConfigManager.Instance(Context.OrganizationID).Config == null) ? oSKUMaster.isactive : false;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oSKUMaster.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("sizegroup", NpgsqlTypes.NpgsqlDbType.Json).Value = oSKUMaster.sizegroup_json == null ? "null" : oSKUMaster.sizegroup_json;

            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oSKUMaster.version = Convert.ToInt32(reader[0]);
                }
            }

            return oSKUMaster;
        }


        public bool Delete(SKUMaster oSKUMaster)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oSKUMaster);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);

            }
            return hasSuccessCount;

        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, SKUMaster oSKUMaster)
        {

            NpgsqlCommand command = GetCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oSKUMaster.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oSKUMaster.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSKUMaster.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

    }

}
