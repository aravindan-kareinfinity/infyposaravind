using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
namespace InfyPOS.Data

{
public class MfgbillofmaterialitemsDataService : SAASData
{
const string SQL_Search = @"select id, mfgbillofmaterialid, bomid, bomtypeid, qty, returnqty, taxpercentage, cost, tax, total, isfabric, isactive, createdby, modifiedby, version, createdon, modifiedon from mfgbillofmaterialitems where id=@id";
const string SQL_SearchAll = @"select id, mfgbillofmaterialid, bomid, bomtypeid, qty, returnqty, taxpercentage, cost, tax, total, isfabric, isactive, createdby, modifiedby, version, createdon, modifiedon from mfgbillofmaterialitems where mfgbillofmaterialid=@mfgbillofmaterialid";


const string SQL_Insert = @"INSERT INTO public.mfgbillofmaterialitems(mfgbillofmaterialid, bomid, bomtypeid, qty, returnqty, taxpercentage, cost, tax, total, isfabric, isactive, createdby, modifiedby, version, createdon, modifiedon) VALUES(@mfgbillofmaterialid, @bomid, @bomtypeid, @qty, @returnqty, @taxpercentage, @cost, @tax, @total, @isfabric, @isactive, @createdby, @modifiedby, @version, @createdon, @modifiedon) RETURNING id";


const string SQL_Update = @"UPDATE public.mfgbillofmaterialitems SET mfgbillofmaterialid=@mfgbillofmaterialid, bomid=@bomid, bomtypeid=@bomtypeid, qty=@qty, returnqty=@returnqty, taxpercentage=@taxpercentage, cost=@cost, tax=@tax, total=@total, isfabric=@isfabric, modifiedby=@modifiedby, version=@version+1, modifiedon=@modifiedon where id=@id and version=@version RETURNING version";

const string SQL_Delete = @"update public.mfgbillofmaterialitems set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";
const string SQL_DeleteAll = @"update public.mfgbillofmaterialitems set isactive=false,modifiedby=@modifiedby,modifiedon=@modifiedon where mfgbillofmaterialid=@mfgbillofmaterialid";
public List<Mfgbillofmaterialitems> Search(MfgbillofmaterialitemsCreteria _mfgbillofmaterialitems){
List<Mfgbillofmaterialitems> lst = new List<Mfgbillofmaterialitems>();
     NpgsqlConnection conn = CreateConnection();
try {
     conn.Open();

NpgsqlCommand command = null;
if (_mfgbillofmaterialitems.id != 0){
    command = new NpgsqlCommand(SQL_Search, conn);
         command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _mfgbillofmaterialitems.id;}
else
{
 QueryBuilder qb = new QueryBuilder();
 qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
 qb.AddParameter("isactive", true, "=");

//if (!string.IsNullOrEmpty(_mfgbillofmaterialitems.Code))
//    qb.AddParameter("code", _mfgbillofmaterialitems.Code, "=");

command = qb.GetCommand(conn);
}

         using (System.Data.IDataReader reader = command.ExecuteReader())
         {
             while (reader.Read())
             {
               Mfgbillofmaterialitems omfgbillofmaterialitems = new Mfgbillofmaterialitems();
                        omfgbillofmaterialitems.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        omfgbillofmaterialitems.mfgbillofmaterialid = reader["mfgbillofmaterialid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["mfgbillofmaterialid"]);
                        omfgbillofmaterialitems.bomid = reader["bomid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["bomid"]);
                        omfgbillofmaterialitems.bomtypeid = reader["bomtypeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["bomtypeid"]);
                        omfgbillofmaterialitems.qty = reader["qty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["qty"]);
                        omfgbillofmaterialitems.returnqty = reader["returnqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["returnqty"]);
                        omfgbillofmaterialitems.taxpercentage = reader["taxpercentage"] == DBNull.Value ? 0 : Convert.ToInt64(reader["taxpercentage"]);
                        omfgbillofmaterialitems.cost = reader["cost"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["cost"]);
                        omfgbillofmaterialitems.tax = reader["tax"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["tax"]);
                        omfgbillofmaterialitems.total = reader["total"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["total"]);
                        omfgbillofmaterialitems.isfabric = reader["isfabric"] == DBNull.Value ? false : Convert.ToBoolean(reader["isfabric"]);
                        omfgbillofmaterialitems.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        omfgbillofmaterialitems.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        omfgbillofmaterialitems.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        omfgbillofmaterialitems.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        omfgbillofmaterialitems.createdon = reader["createdon"] == DBNull.Value ?  DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        omfgbillofmaterialitems.modifiedon = reader["modifiedon"] == DBNull.Value ?  DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                   lst.Add(omfgbillofmaterialitems);
                 }
         }
   conn.Close();
}
catch(Exception exp)
{
     if (conn != null) conn.Close();
     WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _mfgbillofmaterialitems);
}
  return lst;
}

public List<Mfgbillofmaterialitems> SearchAll(long _mfgbillofmaterialid){
List<Mfgbillofmaterialitems> lst = new List<Mfgbillofmaterialitems>();
     NpgsqlConnection conn = CreateConnection();
try {
     conn.Open();

     NpgsqlCommand command = new NpgsqlCommand(SQL_SearchAll, conn);
         command.Parameters.Add("mfgbillofmaterialid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _mfgbillofmaterialid;
         using (System.Data.IDataReader reader = command.ExecuteReader())
         {
             while (reader.Read())
             {
               Mfgbillofmaterialitems omfgbillofmaterialitems = new Mfgbillofmaterialitems();
                        omfgbillofmaterialitems.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        omfgbillofmaterialitems.mfgbillofmaterialid = reader["mfgbillofmaterialid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["mfgbillofmaterialid"]);
                        omfgbillofmaterialitems.bomid = reader["bomid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["bomid"]);
                        omfgbillofmaterialitems.bomtypeid = reader["bomtypeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["bomtypeid"]);
                        omfgbillofmaterialitems.qty = reader["qty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["qty"]);
                        omfgbillofmaterialitems.returnqty = reader["returnqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["returnqty"]);
                        omfgbillofmaterialitems.taxpercentage = reader["taxpercentage"] == DBNull.Value ? 0 : Convert.ToInt64(reader["taxpercentage"]);
                        omfgbillofmaterialitems.cost = reader["cost"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["cost"]);
                        omfgbillofmaterialitems.tax = reader["tax"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["tax"]);
                        omfgbillofmaterialitems.total = reader["total"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["total"]);
                        omfgbillofmaterialitems.isfabric = reader["isfabric"] == DBNull.Value ? false : Convert.ToBoolean(reader["isfabric"]);
                        omfgbillofmaterialitems.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        omfgbillofmaterialitems.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        omfgbillofmaterialitems.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        omfgbillofmaterialitems.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        omfgbillofmaterialitems.createdon = reader["createdon"] == DBNull.Value ?  DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        omfgbillofmaterialitems.modifiedon = reader["modifiedon"] == DBNull.Value ?  DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                   lst.Add(omfgbillofmaterialitems);
                 }
         }
   conn.Close();
}
catch(Exception exp)
{
     if (conn != null) conn.Close();
WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _mfgbillofmaterialid);
}
  return lst;
}

public Mfgbillofmaterialitems Insert(Mfgbillofmaterialitems oMfgbillofmaterialitems){

     NpgsqlTransaction transaction = null;
     try {
         using (NpgsqlConnection conn = CreateConnection())
         {
         conn.Open();
         transaction = conn.BeginTransaction();
         NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);

Insert(transaction, conn, oMfgbillofmaterialitems);
                     transaction.Commit();
   conn.Close();
      }
}
            catch (Exception exp) {
       WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error,oMfgbillofmaterialitems);
                throw exp;
            }

  return oMfgbillofmaterialitems;}

public Mfgbillofmaterialitems Insert(NpgsqlTransaction transaction,NpgsqlConnection conn, Mfgbillofmaterialitems oMfgbillofmaterialitems){

         NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);
command.Parameters.Add("mfgbillofmaterialid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgbillofmaterialitems.mfgbillofmaterialid;
command.Parameters.Add("bomid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgbillofmaterialitems.bomid;
command.Parameters.Add("bomtypeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgbillofmaterialitems.bomtypeid;
command.Parameters.Add("qty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgbillofmaterialitems.qty;
command.Parameters.Add("returnqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgbillofmaterialitems.returnqty;
command.Parameters.Add("taxpercentage", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgbillofmaterialitems.taxpercentage;
command.Parameters.Add("cost", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgbillofmaterialitems.cost;
command.Parameters.Add("tax", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgbillofmaterialitems.tax;
command.Parameters.Add("total", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgbillofmaterialitems.total;
command.Parameters.Add("isfabric", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oMfgbillofmaterialitems.isfabric;
command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oMfgbillofmaterialitems.isactive;
command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgbillofmaterialitems.createdby;
command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgbillofmaterialitems.modifiedby;
command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgbillofmaterialitems.version;
command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgbillofmaterialitems.createdon;
command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgbillofmaterialitems.modifiedon;
                    using (System.Data.IDataReader reader = command.ExecuteReader())
                     {
                         if (reader.Read())
                          {
                            oMfgbillofmaterialitems.id = Convert.ToInt64(reader[0].ToString());
                             }
                      }
  return oMfgbillofmaterialitems;}

public Mfgbillofmaterialitems Update(Mfgbillofmaterialitems oMfgbillofmaterialitems){

     NpgsqlTransaction transaction = null;
     try {
         using (NpgsqlConnection conn = CreateConnection())
         {
         conn.Open();
         transaction = conn.BeginTransaction();

Update(transaction, conn, oMfgbillofmaterialitems);
                     transaction.Commit();
   conn.Close();
      }
}
            catch (Exception exp) {
           WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error,oMfgbillofmaterialitems);
                throw exp;
            }

  return oMfgbillofmaterialitems;}

public Mfgbillofmaterialitems Update(NpgsqlTransaction transaction,NpgsqlConnection conn, Mfgbillofmaterialitems oMfgbillofmaterialitems){

         NpgsqlCommand command = new NpgsqlCommand(SQL_Update, conn, transaction);
command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgbillofmaterialitems.id;
command.Parameters.Add("mfgbillofmaterialid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgbillofmaterialitems.mfgbillofmaterialid;
command.Parameters.Add("bomid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgbillofmaterialitems.bomid;
command.Parameters.Add("bomtypeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgbillofmaterialitems.bomtypeid;
command.Parameters.Add("qty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgbillofmaterialitems.qty;
command.Parameters.Add("returnqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgbillofmaterialitems.returnqty;
command.Parameters.Add("taxpercentage", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgbillofmaterialitems.taxpercentage;
command.Parameters.Add("cost", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgbillofmaterialitems.cost;
command.Parameters.Add("tax", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgbillofmaterialitems.tax;
command.Parameters.Add("total", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgbillofmaterialitems.total;
command.Parameters.Add("isfabric", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oMfgbillofmaterialitems.isfabric;
command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oMfgbillofmaterialitems.isactive;
command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgbillofmaterialitems.createdby;
command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgbillofmaterialitems.modifiedby;
command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgbillofmaterialitems.version;
command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgbillofmaterialitems.createdon;
command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgbillofmaterialitems.modifiedon;
                    using (System.Data.IDataReader reader = command.ExecuteReader())
                     {
                         if (reader.Read())
                          {
                            oMfgbillofmaterialitems.version = Convert.ToInt32(reader[0].ToString());
                             }
                      }

  return oMfgbillofmaterialitems;}

public bool Update(NpgsqlTransaction transaction, NpgsqlConnection conn, List<Mfgbillofmaterialitems> list,long id)
        {
            var existingItems = SearchAll(id);
            if (existingItems != null && existingItems.Count > 0)
            {
                var deletedItems = existingItems.FindAll(e => !list.Exists(f => f.id == e.id));
                foreach (var item in deletedItems)
                    Delete(transaction,conn,item);
            }

            foreach (var item in list)
            {
                if (item.id > 0)
                {
                    Update(transaction, conn, item);
                }
                else
                {
item.mfgbillofmaterialid= id;
Insert(transaction,conn, item);
                }
            }
            return true;
        }

public bool Delete(Mfgbillofmaterialitems oMfgbillofmaterialitems){

NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
hasSuccessCount = Delete(transaction,conn,oMfgbillofmaterialitems);
transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error,oMfgbillofmaterialitems);

                throw exp;
            }
            return hasSuccessCount;
            
}

public bool Delete(NpgsqlTransaction transaction,NpgsqlConnection conn, Mfgbillofmaterialitems oMfgbillofmaterialitems){

NpgsqlCommand command = new NpgsqlCommand(SQL_Delete, conn, transaction);
                    command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oMfgbillofmaterialitems.isactive;
                    command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgbillofmaterialitems.modifiedby;
                    command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgbillofmaterialitems.modifiedon;
                    command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgbillofmaterialitems.version;
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgbillofmaterialitems.id;
                    int successCount = command.ExecuteNonQuery();
                    
            return successCount>0;
            }

public bool DeleteAll(long mfgbillofmaterialid,long userid,NpgsqlConnection conn,NpgsqlTransaction transaction){


            int successCount = 0;
                    NpgsqlCommand command = new NpgsqlCommand(SQL_DeleteAll, conn, transaction);
                    command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = false;
                    command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = userid;
                    command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value =DateTimeNow;
command.Parameters.Add("mfgbillofmaterialid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = mfgbillofmaterialid;
 successCount = command.ExecuteNonQuery();

            return successCount>0;
            }

}

}
