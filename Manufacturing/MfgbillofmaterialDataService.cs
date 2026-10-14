using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
namespace InfyPOS.Data

{
public class MfgbillofmaterialDataService : SAASData
{
const string SQL_Search = @"select id, mfgworkorderid, bomissuecode, supplierid, issuedby, receivedby, isactive, createdby, modifiedby, version, createdon, modifiedon, bomdate from mfgbillofmaterial where id=@id";


const string SQL_Insert = @"INSERT INTO public.mfgbillofmaterial(mfgworkorderid, bomissuecode, supplierid, issuedby, receivedby, isactive, createdby, modifiedby, version, createdon, modifiedon, bomdate) VALUES(@mfgworkorderid, @bomissuecode, @supplierid, @issuedby, @receivedby, @isactive, @createdby, @modifiedby, @version, @createdon, @modifiedon, @bomdate) RETURNING id";


const string SQL_Update = @"UPDATE public.mfgbillofmaterial SET mfgworkorderid=@mfgworkorderid, bomissuecode=@bomissuecode, supplierid=@supplierid, issuedby=@issuedby, receivedby=@receivedby, modifiedby=@modifiedby, version=@version+1, modifiedon=@modifiedon, bomdate=@bomdate where id=@id and version=@version RETURNING version";

const string SQL_Delete = @"update public.mfgbillofmaterial set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";

public List<Mfgbillofmaterial> Search(MfgbillofmaterialCreteria _mfgbillofmaterial){
List<Mfgbillofmaterial> lst = new List<Mfgbillofmaterial>();
     NpgsqlConnection conn = CreateConnection();
try {
     conn.Open();

NpgsqlCommand command = null;
if (_mfgbillofmaterial.id != 0){
    command = new NpgsqlCommand(SQL_Search, conn);
         command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _mfgbillofmaterial.id;}
else
{
 QueryBuilder qb = new QueryBuilder();
 qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
 qb.AddParameter("isactive", true, "=");

//if (!string.IsNullOrEmpty(_mfgbillofmaterial.Code))
//    qb.AddParameter("code", _mfgbillofmaterial.Code, "=");

command = qb.GetCommand(conn);
}

         using (System.Data.IDataReader reader = command.ExecuteReader())
         {
             while (reader.Read())
             {
               Mfgbillofmaterial omfgbillofmaterial = new Mfgbillofmaterial();
                        omfgbillofmaterial.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        omfgbillofmaterial.mfgworkorderid = reader["mfgworkorderid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["mfgworkorderid"]);
                        omfgbillofmaterial.bomissuecode = reader["bomissuecode"] == DBNull.Value ? "" : reader["bomissuecode"].ToString();
                        omfgbillofmaterial.supplierid = reader["supplierid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["supplierid"]);
                        omfgbillofmaterial.issuedby = reader["issuedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["issuedby"]);
                        omfgbillofmaterial.receivedby = reader["receivedby"] == DBNull.Value ? "" : reader["receivedby"].ToString();
                        omfgbillofmaterial.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        omfgbillofmaterial.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        omfgbillofmaterial.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        omfgbillofmaterial.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        omfgbillofmaterial.createdon = reader["createdon"] == DBNull.Value ?  DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        omfgbillofmaterial.modifiedon = reader["modifiedon"] == DBNull.Value ?  DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        omfgbillofmaterial.bomdate = reader["bomdate"] == DBNull.Value ?  DateTime.MinValue : Convert.ToDateTime(reader["bomdate"]);
                   lst.Add(omfgbillofmaterial);
                 }
         }
if (_mfgbillofmaterial.id > 0 && lst.Count == 1){
                        MfgbillofmaterialitemsDataService subMfgbillofmaterialitems = new MfgbillofmaterialitemsDataService() { Context = this.Context };
                        lst[0].Mfgbillofmaterialitems = subMfgbillofmaterialitems.SearchAll(_mfgbillofmaterial.id);
                                }   conn.Close();
}
catch(Exception exp)
{
     if (conn != null) conn.Close();
     WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _mfgbillofmaterial);
}
  return lst;
}

public Mfgbillofmaterial Insert(Mfgbillofmaterial oMfgbillofmaterial){

     NpgsqlTransaction transaction = null;
     try {
         using (NpgsqlConnection conn = CreateConnection())
         {
         conn.Open();
         transaction = conn.BeginTransaction();
         NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);

Insert(transaction, conn, oMfgbillofmaterial);
MfgbillofmaterialitemsDataService subMfgbillofmaterialitems = new MfgbillofmaterialitemsDataService() { Context = this.Context };
                                        foreach (var item in oMfgbillofmaterial.Mfgbillofmaterialitems)
                                        {
                                            item.mfgbillofmaterialid = oMfgbillofmaterial.id;
                                            subMfgbillofmaterialitems.Insert(transaction, conn, item);
                                        }                     transaction.Commit();
   conn.Close();
      }
}
            catch (Exception exp) {
       WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error,oMfgbillofmaterial);
                throw exp;
            }

  return oMfgbillofmaterial;}

public Mfgbillofmaterial Insert(NpgsqlTransaction transaction,NpgsqlConnection conn, Mfgbillofmaterial oMfgbillofmaterial){

         NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);
command.Parameters.Add("mfgworkorderid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgbillofmaterial.mfgworkorderid;
command.Parameters.Add("bomissuecode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfgbillofmaterial.bomissuecode == null ? "" : oMfgbillofmaterial.bomissuecode;
command.Parameters.Add("supplierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgbillofmaterial.supplierid;
command.Parameters.Add("issuedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgbillofmaterial.issuedby;
command.Parameters.Add("receivedby", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfgbillofmaterial.receivedby == null ? "" : oMfgbillofmaterial.receivedby;
command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oMfgbillofmaterial.isactive;
command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgbillofmaterial.createdby;
command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgbillofmaterial.modifiedby;
command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgbillofmaterial.version;
command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgbillofmaterial.createdon;
command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgbillofmaterial.modifiedon;
command.Parameters.Add("bomdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgbillofmaterial.bomdate;
                    using (System.Data.IDataReader reader = command.ExecuteReader())
                     {
                         if (reader.Read())
                          {
                            oMfgbillofmaterial.id = Convert.ToInt64(reader[0].ToString());
                             }
                      }
  return oMfgbillofmaterial;}

public Mfgbillofmaterial Update(Mfgbillofmaterial oMfgbillofmaterial){

     NpgsqlTransaction transaction = null;
     try {
         using (NpgsqlConnection conn = CreateConnection())
         {
         conn.Open();
         transaction = conn.BeginTransaction();

Update(transaction, conn, oMfgbillofmaterial);
MfgbillofmaterialitemsDataService subMfgbillofmaterialitems = new MfgbillofmaterialitemsDataService() { Context = this.Context };
                                        subMfgbillofmaterialitems.Update(transaction, conn, oMfgbillofmaterial.Mfgbillofmaterialitems, oMfgbillofmaterial.id);
                                                             transaction.Commit();
   conn.Close();
      }
}
            catch (Exception exp) {
           WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error,oMfgbillofmaterial);
                throw exp;
            }

  return oMfgbillofmaterial;}

public Mfgbillofmaterial Update(NpgsqlTransaction transaction,NpgsqlConnection conn, Mfgbillofmaterial oMfgbillofmaterial){

         NpgsqlCommand command = new NpgsqlCommand(SQL_Update, conn, transaction);
command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgbillofmaterial.id;
command.Parameters.Add("mfgworkorderid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgbillofmaterial.mfgworkorderid;
command.Parameters.Add("bomissuecode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfgbillofmaterial.bomissuecode == null ? "" : oMfgbillofmaterial.bomissuecode;
command.Parameters.Add("supplierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgbillofmaterial.supplierid;
command.Parameters.Add("issuedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgbillofmaterial.issuedby;
command.Parameters.Add("receivedby", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfgbillofmaterial.receivedby == null ? "" : oMfgbillofmaterial.receivedby;
command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oMfgbillofmaterial.isactive;
command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgbillofmaterial.createdby;
command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgbillofmaterial.modifiedby;
command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgbillofmaterial.version;
command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgbillofmaterial.createdon;
command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgbillofmaterial.modifiedon;
command.Parameters.Add("bomdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgbillofmaterial.bomdate;
                    using (System.Data.IDataReader reader = command.ExecuteReader())
                     {
                         if (reader.Read())
                          {
                            oMfgbillofmaterial.version = Convert.ToInt32(reader[0].ToString());
                             }
                      }

  return oMfgbillofmaterial;}


public bool Delete(Mfgbillofmaterial oMfgbillofmaterial){

NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
hasSuccessCount = Delete(transaction,conn,oMfgbillofmaterial);
MfgbillofmaterialitemsDataService subMfgbillofmaterialitems = new MfgbillofmaterialitemsDataService() { Context = this.Context };
                                  subMfgbillofmaterialitems.DeleteAll(oMfgbillofmaterial.id,oMfgbillofmaterial.modifiedby,conn,transaction);
transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error,oMfgbillofmaterial);

                throw exp;
            }
            return hasSuccessCount;
            
}

public bool Delete(NpgsqlTransaction transaction,NpgsqlConnection conn, Mfgbillofmaterial oMfgbillofmaterial){

NpgsqlCommand command = new NpgsqlCommand(SQL_Delete, conn, transaction);
                    command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oMfgbillofmaterial.isactive;
                    command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgbillofmaterial.modifiedby;
                    command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgbillofmaterial.modifiedon;
                    command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgbillofmaterial.version;
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgbillofmaterial.id;
                    int successCount = command.ExecuteNonQuery();
                    
            return successCount>0;
            }

}

}
