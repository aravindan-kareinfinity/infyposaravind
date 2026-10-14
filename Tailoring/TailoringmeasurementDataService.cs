using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
namespace InfyPOS.Data

{
public class TailoringmeasurementDataService : BaseData
{
const string SQL_Search = @"select id, productid, code, measurementconfig, version, isactive, modifiedby, createdby, modifiedon, createdon from tailoringmeasurement where id=@id";


const string SQL_Insert = @"INSERT INTO public.tailoringmeasurement(productid, code, measurementconfig, version, isactive, modifiedby, createdby, modifiedon, createdon) VALUES(@productid, @code, @measurementconfig, @version, @isactive, @modifiedby, @createdby, @modifiedon, @createdon) RETURNING id";


const string SQL_Update = @"UPDATE public.tailoringmeasurement SET productid=@productid, code=@code, measurementconfig=@measurementconfig, version=@version+1, modifiedby=@modifiedby, modifiedon=@modifiedon where id=@id and version=@version RETURNING version";

const string SQL_Delete = @"update public.tailoringmeasurement set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";

public List<Tailoringmeasurement> Search(TailoringmeasurementCreteria _tailoringmeasurement){
List<Tailoringmeasurement> lst = new List<Tailoringmeasurement>();
     NpgsqlConnection conn = CreateConnection();
try {
     conn.Open();

NpgsqlCommand command = null;
if (_tailoringmeasurement.id != 0){
    command = new NpgsqlCommand(SQL_Search, conn);
         command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _tailoringmeasurement.id;}
else
{
 QueryBuilder qb = new QueryBuilder();
 qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
 qb.AddParameter("isactive", true, "=");

//if (!string.IsNullOrEmpty(_tailoringmeasurement.Code))
//    qb.AddParameter("code", _tailoringmeasurement.Code, "=");

command = qb.GetCommand(conn);
}

         using (System.Data.IDataReader reader = command.ExecuteReader())
         {
             while (reader.Read())
             {
               Tailoringmeasurement otailoringmeasurement = new Tailoringmeasurement();
                        otailoringmeasurement.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        otailoringmeasurement.productid = reader["productid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productid"]);
                        otailoringmeasurement.code = reader["code"] == DBNull.Value ? "" : reader["code"].ToString();
                        otailoringmeasurement.measurementconfig_json = reader["measurementconfig"] == DBNull.Value ? null : reader["measurementconfig"].ToString();
                        otailoringmeasurement.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        otailoringmeasurement.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        otailoringmeasurement.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        otailoringmeasurement.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        otailoringmeasurement.modifiedon = reader["modifiedon"] == DBNull.Value ?  DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        otailoringmeasurement.createdon = reader["createdon"] == DBNull.Value ?  DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                   lst.Add(otailoringmeasurement);
                 }
         }
   conn.Close();
}
catch(Exception exp)
{
     if (conn != null) conn.Close();
     WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _tailoringmeasurement);
}
  return lst;
}

public Tailoringmeasurement Insert(Tailoringmeasurement oTailoringmeasurement){

     NpgsqlTransaction transaction = null;
     try {
var sequence = SequenceManager.Instance(Context.OrganizationID).Get("tailoringmeasurement.code",0);
         using (NpgsqlConnection conn = CreateConnection())
         {
         conn.Open();
         transaction = conn.BeginTransaction();
         NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);
oTailoringmeasurement.code = sequence.GetNextNumber(Context,conn, transaction);

Insert(transaction, conn, oTailoringmeasurement);
                     transaction.Commit();
   conn.Close();
      }
}
            catch (Exception exp) {
       WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error,oTailoringmeasurement);
           
         }

  return oTailoringmeasurement;}

public Tailoringmeasurement Insert(NpgsqlTransaction transaction,NpgsqlConnection conn, Tailoringmeasurement oTailoringmeasurement){

         NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);
command.Parameters.Add("productid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTailoringmeasurement.productid;
command.Parameters.Add("code", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oTailoringmeasurement.code == null ? "" : oTailoringmeasurement.code;
command.Parameters.Add("measurementconfig", NpgsqlTypes.NpgsqlDbType.Json).Value = oTailoringmeasurement.measurementconfig_json == null ? "null" : oTailoringmeasurement.measurementconfig_json;
command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oTailoringmeasurement.version;
command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oTailoringmeasurement.isactive;
command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTailoringmeasurement.modifiedby;
command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTailoringmeasurement.createdby;
command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oTailoringmeasurement.modifiedon;
command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oTailoringmeasurement.createdon;
                    using (System.Data.IDataReader reader = command.ExecuteReader())
                     {
                         if (reader.Read())
                          {
                            oTailoringmeasurement.id = Convert.ToInt64(reader[0].ToString());
                             }
                      }
  return oTailoringmeasurement;}

public Tailoringmeasurement Update(Tailoringmeasurement oTailoringmeasurement){

     NpgsqlTransaction transaction = null;
     try {
         using (NpgsqlConnection conn = CreateConnection())
         {
         conn.Open();
         transaction = conn.BeginTransaction();

Update(transaction, conn, oTailoringmeasurement);
                     transaction.Commit();
   conn.Close();
      }
}
            catch (Exception exp) {
           WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error,oTailoringmeasurement);
            
         }

  return oTailoringmeasurement;}

public Tailoringmeasurement Update(NpgsqlTransaction transaction,NpgsqlConnection conn, Tailoringmeasurement oTailoringmeasurement){

         NpgsqlCommand command = new NpgsqlCommand(SQL_Update, conn, transaction);
command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTailoringmeasurement.id;
command.Parameters.Add("productid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTailoringmeasurement.productid;
command.Parameters.Add("code", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oTailoringmeasurement.code == null ? "" : oTailoringmeasurement.code;
command.Parameters.Add("measurementconfig", NpgsqlTypes.NpgsqlDbType.Json).Value = oTailoringmeasurement.measurementconfig_json == null ? "null" : oTailoringmeasurement.measurementconfig_json;
command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oTailoringmeasurement.version;
command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oTailoringmeasurement.isactive;
command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTailoringmeasurement.modifiedby;
command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTailoringmeasurement.createdby;
command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oTailoringmeasurement.modifiedon;
command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oTailoringmeasurement.createdon;
                    using (System.Data.IDataReader reader = command.ExecuteReader())
                     {
                         if (reader.Read())
                          {
                            oTailoringmeasurement.version = Convert.ToInt32(reader[0].ToString());
                             }
                      }

  return oTailoringmeasurement;}


public bool Delete(Tailoringmeasurement oTailoringmeasurement){

NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
hasSuccessCount = Delete(transaction,conn,oTailoringmeasurement);
transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error,oTailoringmeasurement);
                
            }
            return hasSuccessCount;
            
}

public bool Delete(NpgsqlTransaction transaction,NpgsqlConnection conn, Tailoringmeasurement oTailoringmeasurement){

NpgsqlCommand command = new NpgsqlCommand(SQL_Delete, conn, transaction);
                    command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oTailoringmeasurement.isactive;
                    command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTailoringmeasurement.modifiedby;
                    command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oTailoringmeasurement.modifiedon;
                    command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oTailoringmeasurement.version;
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTailoringmeasurement.id;
                    int successCount = command.ExecuteNonQuery();
                    
            return successCount>0;
            }

}

}
