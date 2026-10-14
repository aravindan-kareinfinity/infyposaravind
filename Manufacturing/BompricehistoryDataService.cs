using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
namespace InfyPOS.Data

{
public class BompricehistoryDataService : SAASData
{
const string SQL_Search = @"select id, isactive, createdby, modifiedby, version, createdon, modifiedon, bomid, cost from bompricehistory where id=@id";


const string SQL_Insert = @"INSERT INTO public.bompricehistory(isactive, createdby, modifiedby, version, createdon, modifiedon, bomid, cost) VALUES(@isactive, @createdby, @modifiedby, @version, @createdon, @modifiedon, @bomid, @cost) RETURNING id";


const string SQL_Update = @"UPDATE public.bompricehistory SET modifiedby=@modifiedby, version=@version+1, modifiedon=@modifiedon, bomid=@bomid, cost=@cost where id=@id and version=@version RETURNING version";

const string SQL_Delete = @"update public.bompricehistory set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";

public List<Bompricehistory> Search(BompricehistoryCreteria _bompricehistory){
List<Bompricehistory> lst = new List<Bompricehistory>();
     NpgsqlConnection conn = CreateConnection();
try {
     conn.Open();

NpgsqlCommand command = null;
if (_bompricehistory.id != 0){
    command = new NpgsqlCommand(SQL_Search, conn);
         command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _bompricehistory.id;}
else
{
 QueryBuilder qb = new QueryBuilder();
 qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
 qb.AddParameter("isactive", true, "=");

//if (!string.IsNullOrEmpty(_bompricehistory.Code))
//    qb.AddParameter("code", _bompricehistory.Code, "=");

command = qb.GetCommand(conn);
}

         using (System.Data.IDataReader reader = command.ExecuteReader())
         {
             while (reader.Read())
             {
               Bompricehistory obompricehistory = new Bompricehistory();
                        obompricehistory.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        obompricehistory.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        obompricehistory.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        obompricehistory.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        obompricehistory.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        obompricehistory.createdon = reader["createdon"] == DBNull.Value ?  DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        obompricehistory.modifiedon = reader["modifiedon"] == DBNull.Value ?  DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        obompricehistory.bomid = reader["bomid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["bomid"]);
                        obompricehistory.cost = reader["cost"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["cost"]);
                   lst.Add(obompricehistory);
                 }
         }
   conn.Close();
}
catch(Exception exp)
{
     if (conn != null) conn.Close();
     WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _bompricehistory);
}
  return lst;
}

public Bompricehistory Insert(Bompricehistory oBompricehistory){

     NpgsqlTransaction transaction = null;
     try {
         using (NpgsqlConnection conn = CreateConnection())
         {
         conn.Open();
         transaction = conn.BeginTransaction();
         NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);

Insert(transaction, conn, oBompricehistory);
                     transaction.Commit();
   conn.Close();
      }
}
            catch (Exception exp) {
       WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error,oBompricehistory);
                throw exp;
            }

  return oBompricehistory;}

public Bompricehistory Insert(NpgsqlTransaction transaction,NpgsqlConnection conn, Bompricehistory oBompricehistory){

         NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);
command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBompricehistory.isactive;
command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBompricehistory.createdby;
command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBompricehistory.modifiedby;
command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBompricehistory.version;
command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBompricehistory.createdon;
command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBompricehistory.modifiedon;
command.Parameters.Add("bomid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBompricehistory.bomid;
command.Parameters.Add("cost", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBompricehistory.cost;
                    using (System.Data.IDataReader reader = command.ExecuteReader())
                     {
                         if (reader.Read())
                          {
                            oBompricehistory.id = Convert.ToInt64(reader[0].ToString());
                             }
                      }
  return oBompricehistory;}

public Bompricehistory Update(Bompricehistory oBompricehistory){

     NpgsqlTransaction transaction = null;
     try {
         using (NpgsqlConnection conn = CreateConnection())
         {
         conn.Open();
         transaction = conn.BeginTransaction();

Update(transaction, conn, oBompricehistory);
                     transaction.Commit();
   conn.Close();
      }
}
            catch (Exception exp) {
           WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error,oBompricehistory);
                throw exp;
            }

  return oBompricehistory;}

public Bompricehistory Update(NpgsqlTransaction transaction,NpgsqlConnection conn, Bompricehistory oBompricehistory){

         NpgsqlCommand command = new NpgsqlCommand(SQL_Update, conn, transaction);
command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBompricehistory.id;
command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBompricehistory.isactive;
command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBompricehistory.createdby;
command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBompricehistory.modifiedby;
command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBompricehistory.version;
command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBompricehistory.createdon;
command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBompricehistory.modifiedon;
command.Parameters.Add("bomid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBompricehistory.bomid;
command.Parameters.Add("cost", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBompricehistory.cost;
                    using (System.Data.IDataReader reader = command.ExecuteReader())
                     {
                         if (reader.Read())
                          {
                            oBompricehistory.version = Convert.ToInt32(reader[0].ToString());
                             }
                      }

  return oBompricehistory;}


public bool Delete(Bompricehistory oBompricehistory){

NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
hasSuccessCount = Delete(transaction,conn,oBompricehistory);
transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error,oBompricehistory);

                throw exp;
            }
            return hasSuccessCount;
            
}

public bool Delete(NpgsqlTransaction transaction,NpgsqlConnection conn, Bompricehistory oBompricehistory){

NpgsqlCommand command = new NpgsqlCommand(SQL_Delete, conn, transaction);
                    command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBompricehistory.isactive;
                    command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBompricehistory.modifiedby;
                    command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBompricehistory.modifiedon;
                    command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBompricehistory.version;
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBompricehistory.id;
                    int successCount = command.ExecuteNonQuery();
                    
            return successCount>0;
            }

}

}
