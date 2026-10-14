using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
namespace InfyPOS.Data

{
    public class MfgworkordergrnitemsDataService : SAASData
    {
        const string SQL_Search = @"select id, mfgworkordergrnid,mfgworkorderitemsid, isactive, createdby, modifiedby, version, createdon, modifiedon, received, colourid, sleeveid, size, designid, cost, taxamount, amount, taxpercentage, taxid, damaged,designname,designitemcode,productid from mfgworkordergrnitems where id=@id";
        const string SQL_SearchAll = @"select id, mfgworkordergrnid,mfgworkorderitemsid, isactive, createdby, modifiedby, version, createdon, modifiedon, received, colourid, sleeveid, size, designid, cost, taxamount, amount, taxpercentage, taxid, damaged,designname,designitemcode,productid from mfgworkordergrnitems where mfgworkordergrnid=@mfgworkordergrnid";


        const string SQL_Insert = @"INSERT INTO public.mfgworkordergrnitems(mfgworkordergrnid,mfgworkorderitemsid, isactive, createdby, modifiedby, version, createdon, modifiedon, received, colourid, sleeveid, size, designid, cost, taxamount, amount, taxpercentage, taxid, damaged,designname,designitemcode,productid) 
        VALUES(@mfgworkordergrnid,@mfgworkorderitemsid, @isactive, @createdby, @modifiedby, @version, @createdon, @modifiedon, @received, @colourid, @sleeveid, @size, @designid, @cost, @taxamount, @amount, @taxpercentage, @taxid, @damaged,@designname,@designitemcode,@productid) RETURNING id";


        const string SQL_Update = @"UPDATE public.mfgworkordergrnitems SET mfgworkordergrnid=@mfgworkordergrnid, modifiedby=@modifiedby, version=@version+1, 
        modifiedon=@modifiedon, received=@received, colourid=@colourid, sleeveid=@sleeveid, size=@size, designid=@designid, cost=@cost, taxamount=@taxamount, 
        amount=@amount, taxpercentage=@taxpercentage, taxid=@taxid, damaged=@damaged ,designname=@designname,designitemcode=@designitemcode,
        mfgworkorderitemsid=@mfgworkorderitemsid
        where id=@id and version=@version RETURNING version";

        const string SQL_Delete = @"update public.mfgworkordergrnitems set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";

        const string SQL_DeleteAll = @"update public.mfgworkordergrnitems set isactive=false,modifiedby=@modifiedby,modifiedon=@modifiedon where mfgworkordergrnid=@mfgworkordergrnid;";
        public List<Mfgworkordergrnitems> Search(MfgworkordergrnitemsCreteria _mfgworkordergrnitems)
        {
            List<Mfgworkordergrnitems> lst = new List<Mfgworkordergrnitems>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_mfgworkordergrnitems.id != 0)
                {
                    command = new NpgsqlCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _mfgworkordergrnitems.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("isactive", true, "=");

                    //if (!string.IsNullOrEmpty(_mfgworkordergrnitems.Code))
                    //    qb.AddParameter("code", _mfgworkordergrnitems.Code, "=");

                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Mfgworkordergrnitems omfgworkordergrnitems = new Mfgworkordergrnitems();
                        omfgworkordergrnitems.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        omfgworkordergrnitems.mfgworkordergrnid = reader["mfgworkordergrnid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["mfgworkordergrnid"]);
                        omfgworkordergrnitems.mfgworkorderitemsid = reader["mfgworkorderitemsid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["mfgworkorderitemsid"]);
                        omfgworkordergrnitems.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        omfgworkordergrnitems.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        omfgworkordergrnitems.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        omfgworkordergrnitems.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        omfgworkordergrnitems.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        omfgworkordergrnitems.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        omfgworkordergrnitems.received = reader["received"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["received"]);
                        omfgworkordergrnitems.colourid = reader["colourid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["colourid"]);
                        omfgworkordergrnitems.sleeveid = reader["sleeveid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sleeveid"]);
                        omfgworkordergrnitems.productid = reader["productid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productid"]);
                        omfgworkordergrnitems.size = reader["size"] == DBNull.Value ? "" : reader["size"].ToString();
                        omfgworkordergrnitems.designname = reader["designname"] == DBNull.Value ? "" : reader["designname"].ToString();
                        omfgworkordergrnitems.designitemcode = reader["designitemcode"] == DBNull.Value ? "" : reader["designitemcode"].ToString();
                        omfgworkordergrnitems.designid = reader["designid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["designid"]);
                        omfgworkordergrnitems.cost = reader["cost"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["cost"]);
                        omfgworkordergrnitems.taxamount = reader["taxamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["taxamount"]);
                        omfgworkordergrnitems.amount = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"]);
                        omfgworkordergrnitems.taxpercentage = reader["taxpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["taxpercentage"]);
                        omfgworkordergrnitems.taxid = reader["taxid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["taxid"]);
                        omfgworkordergrnitems.damaged = reader["damaged"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["damaged"]);

                        lst.Add(omfgworkordergrnitems);
                    }
                }
                if (_mfgworkordergrnitems.id > 0 && lst.Count == 1)
                {
                    MfgworkordergrnitemsizesDataService subMfgworkordergrnitemsizes = new MfgworkordergrnitemsizesDataService() { Context = this.Context };
                    lst[0].sizelist = subMfgworkordergrnitemsizes.SearchAll(_mfgworkordergrnitems.id);
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _mfgworkordergrnitems);
            }
            return lst;
        }

        public List<Mfgworkordergrnitems> SearchAll(long _mfgworkordergrnid)
        {
            List<Mfgworkordergrnitems> lst = new List<Mfgworkordergrnitems>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = new NpgsqlCommand(SQL_SearchAll, conn);
                command.Parameters.Add("mfgworkordergrnid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _mfgworkordergrnid;
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Mfgworkordergrnitems omfgworkordergrnitems = new Mfgworkordergrnitems();
                        omfgworkordergrnitems.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        omfgworkordergrnitems.mfgworkordergrnid = reader["mfgworkordergrnid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["mfgworkordergrnid"]);
                        omfgworkordergrnitems.mfgworkorderitemsid = reader["mfgworkorderitemsid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["mfgworkorderitemsid"]);
                        omfgworkordergrnitems.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        omfgworkordergrnitems.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        omfgworkordergrnitems.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        omfgworkordergrnitems.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        omfgworkordergrnitems.productid = reader["productid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productid"]);
                        omfgworkordergrnitems.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        omfgworkordergrnitems.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        omfgworkordergrnitems.received = reader["received"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["received"]);
                        omfgworkordergrnitems.colourid = reader["colourid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["colourid"]);
                        omfgworkordergrnitems.sleeveid = reader["sleeveid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sleeveid"]);
                        omfgworkordergrnitems.designname = reader["designname"] == DBNull.Value ? "" : reader["designname"].ToString();
                        omfgworkordergrnitems.designitemcode = reader["designitemcode"] == DBNull.Value ? "" : reader["designitemcode"].ToString();
                        omfgworkordergrnitems.size = reader["size"] == DBNull.Value ? "" : reader["size"].ToString();
                        omfgworkordergrnitems.designid = reader["designid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["designid"]);
                        omfgworkordergrnitems.cost = reader["cost"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["cost"]);
                        omfgworkordergrnitems.taxamount = reader["taxamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["taxamount"]);
                        omfgworkordergrnitems.amount = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"]);
                        omfgworkordergrnitems.taxpercentage = reader["taxpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["taxpercentage"]);
                        omfgworkordergrnitems.taxid = reader["taxid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["taxid"]);
                        omfgworkordergrnitems.damaged = reader["damaged"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["damaged"]);
                        
                        lst.Add(omfgworkordergrnitems);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _mfgworkordergrnid);
            }
            return lst;
        }

        public Mfgworkordergrnitems Insert(Mfgworkordergrnitems oMfgworkordergrnitems)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);

                    Insert(transaction, conn, oMfgworkordergrnitems);
                   
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oMfgworkordergrnitems);
                throw exp;
            }

            return oMfgworkordergrnitems;
        }

        public Mfgworkordergrnitems Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Mfgworkordergrnitems oMfgworkordergrnitems)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("mfgworkordergrnid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkordergrnitems.mfgworkordergrnid;
            command.Parameters.Add("mfgworkorderitemsid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkordergrnitems.mfgworkorderitemsid;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oMfgworkordergrnitems.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkordergrnitems.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkordergrnitems.modifiedby;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgworkordergrnitems.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("received", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgworkordergrnitems.received;
            command.Parameters.Add("colourid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkordergrnitems.colourid;
            command.Parameters.Add("sleeveid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkordergrnitems.sleeveid;
            command.Parameters.Add("productid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkordergrnitems.productid;
            command.Parameters.Add("size", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfgworkordergrnitems.size == null ? "" : oMfgworkordergrnitems.size;
            command.Parameters.Add("designname", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfgworkordergrnitems.designname == null ? "" : oMfgworkordergrnitems.designname;
            command.Parameters.Add("designitemcode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfgworkordergrnitems.designitemcode == null ? "" : oMfgworkordergrnitems.designitemcode;
            command.Parameters.Add("designid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkordergrnitems.designid;
            command.Parameters.Add("cost", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgworkordergrnitems.cost;
            command.Parameters.Add("taxamount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgworkordergrnitems.taxamount;
            command.Parameters.Add("amount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgworkordergrnitems.amount;
            command.Parameters.Add("taxpercentage", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgworkordergrnitems.taxpercentage;
            command.Parameters.Add("taxid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkordergrnitems.taxid;
            command.Parameters.Add("damaged", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgworkordergrnitems.damaged;

            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oMfgworkordergrnitems.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            MfgworkordergrnitemsizesDataService subMfgworkordergrnitemsizes = new MfgworkordergrnitemsizesDataService() { Context = this.Context };
            foreach (var item in oMfgworkordergrnitems.sizelist)
            {
                item.mfgworkorderitemsgrnid = oMfgworkordergrnitems.id;
                item.mfgworkordergrnid = oMfgworkordergrnitems.mfgworkordergrnid;
                subMfgworkordergrnitemsizes.Insert(transaction, conn, item);
            }
            return oMfgworkordergrnitems;
        }

        public Mfgworkordergrnitems Update(Mfgworkordergrnitems oMfgworkordergrnitems)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oMfgworkordergrnitems);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oMfgworkordergrnitems);
                throw exp;
            }

            return oMfgworkordergrnitems;
        }

        public Mfgworkordergrnitems Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Mfgworkordergrnitems oMfgworkordergrnitems)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkordergrnitems.id;
            command.Parameters.Add("mfgworkordergrnid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkordergrnitems.mfgworkordergrnid;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oMfgworkordergrnitems.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkordergrnitems.createdby;
            command.Parameters.Add("mfgworkorderitemsid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkordergrnitems.mfgworkorderitemsid;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkordergrnitems.modifiedby;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgworkordergrnitems.version;
            command.Parameters.Add("designname", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfgworkordergrnitems.designname == null ? "" : oMfgworkordergrnitems.designname;
            command.Parameters.Add("designitemcode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfgworkordergrnitems.designitemcode == null ? "" : oMfgworkordergrnitems.designitemcode;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgworkordergrnitems.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgworkordergrnitems.modifiedon;
            command.Parameters.Add("received", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgworkordergrnitems.received;
            command.Parameters.Add("colourid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkordergrnitems.colourid;
            command.Parameters.Add("sleeveid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkordergrnitems.sleeveid;
            command.Parameters.Add("size", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfgworkordergrnitems.size == null ? "" : oMfgworkordergrnitems.size;
            command.Parameters.Add("designid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkordergrnitems.designid;
            command.Parameters.Add("cost", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgworkordergrnitems.cost;
            command.Parameters.Add("taxamount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgworkordergrnitems.taxamount;
            command.Parameters.Add("amount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgworkordergrnitems.amount;
            command.Parameters.Add("taxpercentage", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgworkordergrnitems.taxpercentage;
            command.Parameters.Add("taxid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkordergrnitems.taxid;
            command.Parameters.Add("damaged", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgworkordergrnitems.damaged;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oMfgworkordergrnitems.version = Convert.ToInt32(reader[0].ToString());
                }
            }

            oMfgworkordergrnitems.sizelist.ForEach(e => e.mfgworkordergrnid = oMfgworkordergrnitems.mfgworkordergrnid);
            MfgworkordergrnitemsizesDataService subMfgworkordergrnitemsizes = new MfgworkordergrnitemsizesDataService() { Context = this.Context };
            subMfgworkordergrnitemsizes.Update(transaction, conn, oMfgworkordergrnitems.sizelist, oMfgworkordergrnitems.id);
            return oMfgworkordergrnitems;
        }

        public bool Update(NpgsqlTransaction transaction, NpgsqlConnection conn, List<Mfgworkordergrnitems> list, long id)
        {
            var existingItems = SearchAll(id);
            if (existingItems != null && existingItems.Count > 0)
            {
                var deletedItems = existingItems.FindAll(e => !list.Exists(f => f.id == e.id));
                foreach (var item in deletedItems)
                    Delete(transaction, conn, item);
            }

            foreach (var item in list)
            {
                if (item.id > 0)
                {
                    Update(transaction, conn, item);
                }
                else
                {
                    item.mfgworkordergrnid = id;
                    Insert(transaction, conn, item);
                }
            }
            return true;
        }

        public bool Delete(Mfgworkordergrnitems oMfgworkordergrnitems)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oMfgworkordergrnitems);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oMfgworkordergrnitems);

                throw exp;
            }
            return hasSuccessCount;

        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Mfgworkordergrnitems oMfgworkordergrnitems)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oMfgworkordergrnitems.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkordergrnitems.modifiedby;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgworkordergrnitems.modifiedon;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgworkordergrnitems.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkordergrnitems.id;
            int successCount = command.ExecuteNonQuery();
            MfgworkordergrnitemsizesDataService subMfgworkordergrnitemsizes = new MfgworkordergrnitemsizesDataService() { Context = this.Context };
            subMfgworkordergrnitemsizes.DeleteAll(oMfgworkordergrnitems.id, oMfgworkordergrnitems.modifiedby, conn, transaction);

            return successCount > 0;
        }

        public bool DeleteAll(long mfgworkordergrnid, long userid, NpgsqlConnection conn, NpgsqlTransaction transaction)
        {


            int successCount = 0;
            NpgsqlCommand command = new NpgsqlCommand(SQL_DeleteAll, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = false;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = userid;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("mfgworkordergrnid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = mfgworkordergrnid;
            successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

    }

}
