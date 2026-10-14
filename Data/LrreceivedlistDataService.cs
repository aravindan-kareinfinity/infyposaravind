using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
namespace InfyPOS.Data
{
    public class LrreceivedlistDataService : SAASData
    {
        const string SQL_Search = @"select id, isactive, createdby, modifiedby, version, createdon, modifiedon, lrid, noofbundelsreceived,noofboxesreceived, lrno, noofbundels, noofpieces, fromlocationid, totalamount,frightcharge, supplierid, isreversed, lrreceivedid,noofboxes,actualweight,chargedweight,loadingcharge,receivedweight,insurance,packing,forward,others from lrreceivedlist where id=@id";
        const string SQL_SearchAll = @"select id, isactive, createdby, modifiedby, version, createdon, modifiedon, lrid, noofbundelsreceived,noofboxesreceived, lrno, noofbundels, noofpieces, fromlocationid , totalamount,frightcharge, supplierid, isreversed, lrreceivedid,noofboxes,actualweight,chargedweight,loadingcharge,receivedweight,insurance,packing,forward,others from lrreceivedlist where lrreceivedid=@lrreceivedid";


        const string SQL_Insert = @"INSERT INTO public.lrreceivedlist(isactive, createdby, modifiedby, version, createdon, modifiedon, lrid, lrno, noofbundels, noofpieces, fromlocationid, totalamount, frightcharge, supplierid, isreversed, lrreceivedid,noofboxes,actualweight,chargedweight,loadingcharge, noofbundelsreceived,noofboxesreceived,receivedweight,insurance,packing,forward,others) 
        VALUES(@isactive, @createdby, @modifiedby, @version, @createdon, @modifiedon, @lrid, @lrno, @noofbundels, @noofpieces, @fromlocationid, @totalamount, @frightcharge, @supplierid, @isreversed, @lrreceivedid,@noofboxes,@actualweight,@chargedweight,@loadingcharge, @noofbundelsreceived,@noofboxesreceived,@receivedweight,@insurance,@packing,@forward,@others) RETURNING id";


        const string SQL_Update = @"UPDATE public.lrreceivedlist SET modifiedby=@modifiedby, version=@version+1, modifiedon=@modifiedon, lrid=@lrid, lrno=@lrno, noofbundels=@noofbundels, noofpieces=@noofpieces, fromlocationid=@fromlocationid, totalamount=@totalamount, frightcharge=@frightcharge,
        supplierid=@supplierid,  isreversed=@isreversed, noofbundelsreceived=@noofbundelsreceived,noofboxesreceived=@noofboxesreceived, lrreceivedid=@lrreceivedid,noofboxes=@noofboxes,actualweight=@actualweight,chargedweight=@chargedweight,loadingcharge=@loadingcharge,receivedweight=@receivedweight,insurance=@insurance,packing=@packing,forward=@forward,others=@others where id=@id and version=@version  RETURNING version";

        const string SQL_Delete = @"update public.lrreceivedlist set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version  RETURNING version";
        const string SQL_DeleteAll = @"update public.lrreceivedlist set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where lrreceivedid=@lrreceivedid and version=@version";
        public List<receiptitem> Search(LrreceivedlistCreteria _lrreceivedlist)
        {
            List<receiptitem> lst = new List<receiptitem>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_lrreceivedlist.id != 0)
                {
                    command = GetCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _lrreceivedlist.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("isactive", true, "=");

                    if (_lrreceivedlist.lrid > 0)
                        qb.AddParameter("lrid", _lrreceivedlist.lrid, "=");
                    
                    if(_lrreceivedlist.onlyreceived)
                        qb.AddParameter("isreversed", false, "=");

                    if (_lrreceivedlist.lrreceivedid > 0)
                        qb.AddParameter("lrreceivedid", _lrreceivedlist.lrreceivedid, "=");
                    command = qb.GetCommand(conn);
                }
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        receiptitem olrreceivedlist = new receiptitem();
                        olrreceivedlist.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        olrreceivedlist.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        olrreceivedlist.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        olrreceivedlist.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        olrreceivedlist.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        olrreceivedlist.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        olrreceivedlist.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"].ToString());
                        olrreceivedlist.noofbundels = reader["noofbundels"] == DBNull.Value ? 0 : Convert.ToInt32(reader["noofbundels"].ToString());
                        olrreceivedlist.noofpieces = reader["noofpieces"] == DBNull.Value ? 0 : Convert.ToInt32(reader["noofpieces"].ToString());
                        olrreceivedlist.fromlocationid = reader["fromlocationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["fromlocationid"].ToString());
                        olrreceivedlist.totalamount = reader["totalamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["totalamount"].ToString());
                        olrreceivedlist.frightcharge = reader["frightcharge"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["frightcharge"].ToString());
                        olrreceivedlist.supplierid = reader["supplierid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["supplierid"].ToString());
                        olrreceivedlist.lrid = reader["lrid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["lrid"].ToString());
                        olrreceivedlist.lrno = reader["lrno"] == DBNull.Value ? "" : reader["lrno"].ToString();
                        olrreceivedlist.isreversed = reader["isreversed"] == DBNull.Value ? false : Convert.ToBoolean(reader["isreversed"].ToString());
                        olrreceivedlist.noofboxes = reader["noofboxes"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["noofboxes"].ToString());
                        olrreceivedlist.actualweight = reader["actualweight"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["actualweight"].ToString());
                        olrreceivedlist.chargedweight = reader["chargedweight"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["chargedweight"].ToString());
                        olrreceivedlist.loadingcharge = reader["loadingcharge"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["loadingcharge"].ToString());
                        olrreceivedlist.lrreceivedid = reader["lrreceivedid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["lrreceivedid"].ToString());

                        olrreceivedlist.noofbundelsreceived = reader["noofbundelsreceived"] == DBNull.Value ? 0 : Convert.ToInt32(reader["noofbundelsreceived"].ToString());
                        olrreceivedlist.noofboxesreceived = reader["noofboxesreceived"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["noofboxesreceived"].ToString());
                        olrreceivedlist.receivedweight = reader["receivedweight"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["receivedweight"].ToString());
                        olrreceivedlist.forward = reader["forward"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["forward"].ToString());
                        olrreceivedlist.packing = reader["packing"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["packing"].ToString());
                        olrreceivedlist.others = reader["others"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["others"].ToString());
                        olrreceivedlist.insurance = reader["insurance"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["insurance"].ToString());

                        lst.Add(olrreceivedlist);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);
                throw exp;
            }
            return lst;
        }

        public List<receiptitem> SearchAll(long _lrreceivedid)
        {
            List<receiptitem> lst = new List<receiptitem>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = GetCommand(SQL_SearchAll, conn);
                command.Parameters.Add("lrreceivedid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _lrreceivedid;
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        receiptitem olrreceivedlist = new receiptitem();
                        olrreceivedlist.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        olrreceivedlist.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        olrreceivedlist.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        olrreceivedlist.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        olrreceivedlist.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        olrreceivedlist.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        olrreceivedlist.noofbundels = reader["noofbundels"] == DBNull.Value ? 0 : Convert.ToInt32(reader["noofbundels"].ToString());
                        olrreceivedlist.noofpieces = reader["noofpieces"] == DBNull.Value ? 0 : Convert.ToInt32(reader["noofpieces"].ToString());
                        olrreceivedlist.fromlocationid = reader["fromlocationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["fromlocationid"].ToString());
                        olrreceivedlist.totalamount = reader["totalamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["totalamount"].ToString());
                        olrreceivedlist.supplierid = reader["supplierid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["supplierid"].ToString());
                        olrreceivedlist.lrid = reader["lrid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["lrid"].ToString());
                        olrreceivedlist.lrno = reader["lrno"] == DBNull.Value ? "" : reader["lrno"].ToString();
                        olrreceivedlist.noofboxes = reader["noofboxes"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["noofboxes"].ToString());
                        olrreceivedlist.actualweight = reader["actualweight"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["actualweight"].ToString());
                        olrreceivedlist.chargedweight = reader["chargedweight"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["chargedweight"].ToString());
                        olrreceivedlist.loadingcharge = reader["loadingcharge"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["loadingcharge"].ToString());
                        olrreceivedlist.isreversed = reader["isreversed"] == DBNull.Value ? false : Convert.ToBoolean(reader["isreversed"].ToString());
                        olrreceivedlist.lrreceivedid = reader["lrreceivedid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["lrreceivedid"].ToString());
                        olrreceivedlist.noofbundelsreceived = reader["noofbundelsreceived"] == DBNull.Value ? 0 : Convert.ToInt32(reader["noofbundelsreceived"].ToString());
                        olrreceivedlist.noofboxesreceived = reader["noofboxesreceived"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["noofboxesreceived"].ToString());
                        olrreceivedlist.frightcharge = reader["frightcharge"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["frightcharge"].ToString());
                        olrreceivedlist.receivedweight = reader["receivedweight"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["receivedweight"].ToString());
                        olrreceivedlist.forward = reader["forward"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["forward"].ToString());
                        olrreceivedlist.packing = reader["packing"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["packing"].ToString());
                        olrreceivedlist.others = reader["others"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["others"].ToString());
                        olrreceivedlist.insurance = reader["insurance"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["insurance"].ToString());

                        lst.Add(olrreceivedlist);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);
                throw exp;
            }
            return lst;
        }

        public receiptitem Insert(receiptitem oLrreceivedlist)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);

                    Insert(transaction, conn, oLrreceivedlist);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {

                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);
            }

            return oLrreceivedlist;
        }

        public receiptitem Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, receiptitem oLrreceivedlist)
        {
            if (oLrreceivedlist.totalamount == 0)
                oLrreceivedlist.totalamount = oLrreceivedlist.frightcharge + oLrreceivedlist.loadingcharge 
                    + oLrreceivedlist.others + oLrreceivedlist.forward + oLrreceivedlist.packing + oLrreceivedlist.insurance;

            NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oLrreceivedlist.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oLrreceivedlist.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("lrid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrreceivedlist.lrid;
            command.Parameters.Add("lrno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oLrreceivedlist.lrno == null ? "" : oLrreceivedlist.lrno;
            command.Parameters.Add("noofbundels", NpgsqlTypes.NpgsqlDbType.Integer).Value = oLrreceivedlist.noofbundels;
            command.Parameters.Add("noofpieces", NpgsqlTypes.NpgsqlDbType.Integer).Value = oLrreceivedlist.noofpieces;
            command.Parameters.Add("fromlocationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrreceivedlist.fromlocationid; ;
            command.Parameters.Add("supplierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrreceivedlist.supplierid;
            command.Parameters.Add("totalamount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrreceivedlist.totalamount;
            command.Parameters.Add("isreversed", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oLrreceivedlist.isreversed;
            command.Parameters.Add("lrreceivedid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrreceivedlist.lrreceivedid;
            command.Parameters.Add("noofboxes", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrreceivedlist.noofboxes;
            command.Parameters.Add("actualweight", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrreceivedlist.actualweight;
            command.Parameters.Add("chargedweight", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrreceivedlist.chargedweight;
            command.Parameters.Add("loadingcharge", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrreceivedlist.loadingcharge;
            command.Parameters.Add("frightcharge", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrreceivedlist.frightcharge;
            command.Parameters.Add("noofbundelsreceived", NpgsqlTypes.NpgsqlDbType.Integer).Value = oLrreceivedlist.noofbundelsreceived;
            command.Parameters.Add("noofboxesreceived", NpgsqlTypes.NpgsqlDbType.Integer).Value = oLrreceivedlist.noofboxesreceived;
            command.Parameters.Add("receivedweight", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrreceivedlist.receivedweight;
            command.Parameters.Add("insurance", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrreceivedlist.insurance;
            command.Parameters.Add("forward", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrreceivedlist.forward;
            command.Parameters.Add("packing", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrreceivedlist.packing;
            command.Parameters.Add("others", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrreceivedlist.others;

            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oLrreceivedlist.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            return oLrreceivedlist;
        }

        public receiptitem Update(receiptitem oLrreceivedlist)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oLrreceivedlist);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);

            }

            return oLrreceivedlist;
        }

        public receiptitem Update(NpgsqlTransaction transaction, NpgsqlConnection conn, receiptitem oLrreceivedlist)
        {

            NpgsqlCommand command = GetCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrreceivedlist.id;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oLrreceivedlist.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrreceivedlist.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oLrreceivedlist.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oLrreceivedlist.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("lrid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrreceivedlist.lrid;
            command.Parameters.Add("lrno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oLrreceivedlist.lrno == null ? "" : oLrreceivedlist.lrno;
            command.Parameters.Add("noofbundels", NpgsqlTypes.NpgsqlDbType.Integer).Value = oLrreceivedlist.noofbundels;
            command.Parameters.Add("noofpieces", NpgsqlTypes.NpgsqlDbType.Integer).Value = oLrreceivedlist.noofpieces;
            command.Parameters.Add("fromlocationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrreceivedlist.fromlocationid; ;
            command.Parameters.Add("totalamount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrreceivedlist.totalamount;
            command.Parameters.Add("supplierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrreceivedlist.supplierid;
            command.Parameters.Add("isreversed", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oLrreceivedlist.isreversed;
            command.Parameters.Add("lrreceivedid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrreceivedlist.lrreceivedid;
            command.Parameters.Add("frightcharge", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrreceivedlist.frightcharge;
            command.Parameters.Add("noofboxes", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrreceivedlist.noofboxes;
            command.Parameters.Add("actualweight", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrreceivedlist.actualweight;
            command.Parameters.Add("chargedweight", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrreceivedlist.chargedweight;
            command.Parameters.Add("loadingcharge", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrreceivedlist.loadingcharge;
            command.Parameters.Add("noofbundelsreceived", NpgsqlTypes.NpgsqlDbType.Integer).Value = oLrreceivedlist.noofbundelsreceived;
            command.Parameters.Add("noofboxesreceived", NpgsqlTypes.NpgsqlDbType.Integer).Value = oLrreceivedlist.noofboxesreceived;
            command.Parameters.Add("receivedweight", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrreceivedlist.receivedweight;
            command.Parameters.Add("insurance", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrreceivedlist.insurance;
            command.Parameters.Add("forward", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrreceivedlist.forward;
            command.Parameters.Add("packing", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrreceivedlist.packing;
            command.Parameters.Add("others", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrreceivedlist.others;

            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oLrreceivedlist.version = Convert.ToInt32(reader[0]);
                }
            }

            return oLrreceivedlist;
        }

        public bool Delete(receiptitem oLrreceivedlist)
        {

            NpgsqlTransaction transaction = null;
            int successCount = 0;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = GetCommand(SQL_Delete, conn, transaction);
                    command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oLrreceivedlist.isactive;
                    command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                    command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
                    command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oLrreceivedlist.version;
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrreceivedlist.id;
                    successCount = command.ExecuteNonQuery();
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);

            }
            return successCount > 0;
        }

        public bool DeleteAll(receiptitem oLrreceivedlist)
        {

            NpgsqlTransaction transaction = null;
            int successCount = 0;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = GetCommand(SQL_DeleteAll, conn, transaction);
                    command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oLrreceivedlist.isactive;
                    command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                    command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
                    command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oLrreceivedlist.version;
                    command.Parameters.Add("lrreceivedid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrreceivedlist.lrreceivedid;
                    successCount = command.ExecuteNonQuery();
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);

            }
            return successCount > 0;
        }

    }

}
