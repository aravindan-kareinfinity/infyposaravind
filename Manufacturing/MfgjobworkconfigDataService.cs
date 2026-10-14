using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
namespace InfyPOS.Data

{
public class MfgjobworkconfigDataService : SAASData
{
const string SQL_Search = @"select id, name, isactive, createdby, modifiedby, version, createdon, modifiedon, uomid, sourcebomcategoryid, sourcebomid, sourcetypeid, productid, taxid, rate, config from mfgjobworkconfig where id=@id";


const string SQL_Insert = @"INSERT INTO public.mfgjobworkconfig(name, isactive, createdby, modifiedby, version, createdon, modifiedon, uomid, sourcebomcategoryid, sourcebomid, sourcetypeid, productid, taxid, rate, config) VALUES(@name, @isactive, @createdby, @modifiedby, @version, @createdon, @modifiedon, @uomid, @sourcebomcategoryid, @sourcebomid, @sourcetypeid, @productid, @taxid, @rate, @config) RETURNING id";


const string SQL_Update = @"UPDATE public.mfgjobworkconfig SET name=@name, modifiedby=@modifiedby, version=@version+1, modifiedon=@modifiedon, uomid=@uomid, sourcebomcategoryid=@sourcebomcategoryid, sourcebomid=@sourcebomid, sourcetypeid=@sourcetypeid, productid=@productid, taxid=@taxid, rate=@rate, config=@config where id=@id and version=@version RETURNING version";

const string SQL_Delete = @"update public.mfgjobworkconfig set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";

public List<Mfgjobworkconfig> Search(MfgjobworkconfigCreteria _mfgjobworkconfig){
List<Mfgjobworkconfig> lst = new List<Mfgjobworkconfig>();
     NpgsqlConnection conn = CreateConnection();
try {
     conn.Open();

NpgsqlCommand command = null;
if (_mfgjobworkconfig.id != 0){
    command = new NpgsqlCommand(SQL_Search, conn);
         command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _mfgjobworkconfig.id;}
else
{
 QueryBuilder qb = new QueryBuilder();
 qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
 qb.AddParameter("isactive", true, "=");

//if (!string.IsNullOrEmpty(_mfgjobworkconfig.Code))
//    qb.AddParameter("code", _mfgjobworkconfig.Code, "=");

command = qb.GetCommand(conn);
}

         using (System.Data.IDataReader reader = command.ExecuteReader())
         {
             while (reader.Read())
             {
               Mfgjobworkconfig omfgjobworkconfig = new Mfgjobworkconfig();
                        omfgjobworkconfig.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        omfgjobworkconfig.name = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
                        omfgjobworkconfig.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        omfgjobworkconfig.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        omfgjobworkconfig.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        omfgjobworkconfig.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        omfgjobworkconfig.createdon = reader["createdon"] == DBNull.Value ?  DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        omfgjobworkconfig.modifiedon = reader["modifiedon"] == DBNull.Value ?  DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        omfgjobworkconfig.uomid = reader["uomid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["uomid"]);
                        omfgjobworkconfig.sourcebomcategoryid = reader["sourcebomcategoryid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sourcebomcategoryid"]);
                        omfgjobworkconfig.sourcebomid = reader["sourcebomid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sourcebomid"]);
                        omfgjobworkconfig.sourcetypeid = reader["sourcetypeid"] == DBNull.Value ? 0 : Convert.ToInt32(reader["sourcetypeid"]);
                        omfgjobworkconfig.productid = reader["productid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productid"]);
                        omfgjobworkconfig.taxid = reader["taxid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["taxid"]);
                        omfgjobworkconfig.rate = reader["rate"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["rate"]);
                        omfgjobworkconfig.config_json = reader["config"] == DBNull.Value ? null : reader["config"].ToString();
                   lst.Add(omfgjobworkconfig);
                 }
         }
   conn.Close();
}
catch(Exception exp)
{
     if (conn != null) conn.Close();
     WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _mfgjobworkconfig);
}
  return lst;
}

public Mfgjobworkconfig Insert(Mfgjobworkconfig oMfgjobworkconfig){

     NpgsqlTransaction transaction = null;
     try {
         using (NpgsqlConnection conn = CreateConnection())
         {
         conn.Open();
         transaction = conn.BeginTransaction();
         NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);

Insert(transaction, conn, oMfgjobworkconfig);
                     transaction.Commit();
   conn.Close();
      }
}
            catch (Exception exp) {
       WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error,oMfgjobworkconfig);
                throw exp;
            }

  return oMfgjobworkconfig;}

public Mfgjobworkconfig Insert(NpgsqlTransaction transaction,NpgsqlConnection conn, Mfgjobworkconfig oMfgjobworkconfig){

         NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);
command.Parameters.Add("name", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfgjobworkconfig.name == null ? "" : oMfgjobworkconfig.name;
command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oMfgjobworkconfig.isactive;
command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgjobworkconfig.createdby;
command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgjobworkconfig.modifiedby;
command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgjobworkconfig.version;
command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgjobworkconfig.createdon;
command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgjobworkconfig.modifiedon;
command.Parameters.Add("uomid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgjobworkconfig.uomid;
command.Parameters.Add("sourcebomcategoryid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgjobworkconfig.sourcebomcategoryid;
command.Parameters.Add("sourcebomid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgjobworkconfig.sourcebomid;
command.Parameters.Add("sourcetypeid", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgjobworkconfig.sourcetypeid;
command.Parameters.Add("productid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgjobworkconfig.productid;
command.Parameters.Add("taxid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgjobworkconfig.taxid;
command.Parameters.Add("rate", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgjobworkconfig.rate;
command.Parameters.Add("config", NpgsqlTypes.NpgsqlDbType.Json).Value = oMfgjobworkconfig.config_json == null ? "null" : oMfgjobworkconfig.config_json;
                    using (System.Data.IDataReader reader = command.ExecuteReader())
                     {
                         if (reader.Read())
                          {
                            oMfgjobworkconfig.id = Convert.ToInt64(reader[0].ToString());
                             }
                      }
  return oMfgjobworkconfig;}

public Mfgjobworkconfig Update(Mfgjobworkconfig oMfgjobworkconfig){

     NpgsqlTransaction transaction = null;
     try {
         using (NpgsqlConnection conn = CreateConnection())
         {
         conn.Open();
         transaction = conn.BeginTransaction();

Update(transaction, conn, oMfgjobworkconfig);
                     transaction.Commit();
   conn.Close();
      }
}
            catch (Exception exp) {
           WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error,oMfgjobworkconfig);
                throw exp;
            }

  return oMfgjobworkconfig;}

public Mfgjobworkconfig Update(NpgsqlTransaction transaction,NpgsqlConnection conn, Mfgjobworkconfig oMfgjobworkconfig){

         NpgsqlCommand command = new NpgsqlCommand(SQL_Update, conn, transaction);
command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgjobworkconfig.id;
command.Parameters.Add("name", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfgjobworkconfig.name == null ? "" : oMfgjobworkconfig.name;
command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oMfgjobworkconfig.isactive;
command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgjobworkconfig.createdby;
command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgjobworkconfig.modifiedby;
command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgjobworkconfig.version;
command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgjobworkconfig.createdon;
command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgjobworkconfig.modifiedon;
command.Parameters.Add("uomid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgjobworkconfig.uomid;
command.Parameters.Add("sourcebomcategoryid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgjobworkconfig.sourcebomcategoryid;
command.Parameters.Add("sourcebomid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgjobworkconfig.sourcebomid;
command.Parameters.Add("sourcetypeid", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgjobworkconfig.sourcetypeid;
command.Parameters.Add("productid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgjobworkconfig.productid;
command.Parameters.Add("taxid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgjobworkconfig.taxid;
command.Parameters.Add("rate", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgjobworkconfig.rate;
command.Parameters.Add("config", NpgsqlTypes.NpgsqlDbType.Json).Value = oMfgjobworkconfig.config_json == null ? "null" : oMfgjobworkconfig.config_json;
                    using (System.Data.IDataReader reader = command.ExecuteReader())
                     {
                         if (reader.Read())
                          {
                            oMfgjobworkconfig.version = Convert.ToInt32(reader[0].ToString());
                             }
                      }

  return oMfgjobworkconfig;}


public bool Delete(Mfgjobworkconfig oMfgjobworkconfig){

NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
hasSuccessCount = Delete(transaction,conn,oMfgjobworkconfig);
transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error,oMfgjobworkconfig);

                throw exp;
            }
            return hasSuccessCount;
            
}

public bool Delete(NpgsqlTransaction transaction,NpgsqlConnection conn, Mfgjobworkconfig oMfgjobworkconfig){

NpgsqlCommand command = new NpgsqlCommand(SQL_Delete, conn, transaction);
                    command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oMfgjobworkconfig.isactive;
                    command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgjobworkconfig.modifiedby;
                    command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgjobworkconfig.modifiedon;
                    command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgjobworkconfig.version;
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgjobworkconfig.id;
                    int successCount = command.ExecuteNonQuery();
                    
            return successCount>0;
            }

}

}
