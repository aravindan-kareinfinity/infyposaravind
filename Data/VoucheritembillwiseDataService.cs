using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
using static InfyPOS.Data.VoucherDataService;
using System.Web.UI;
using static InfyPOS.Data.B2BDashboard;
using Newtonsoft.Json;

namespace InfyPOS.Data

{
    public class VoucheritembillwiseDataService : BaseData
    {
        const string SQL_Search = @"select voucherid, version, voucheritemsid, ledgerid, createdby, modifiedby, id, transactionon,lrinvoicereturnid,customerinvoiceid,
        receivable, payable, isactive, createdon, modifiedon, referencetype, referenceno,referenceinvoiceno,referenceinvoicedate,obcompanyid,adjusted,sourceitemid,paid,onadvance 
        from voucheritembillwise where id=@id";


        const string SQL_Insert = @"INSERT INTO public.voucheritembillwise(voucherid, version, voucheritemsid, ledgerid, 
        createdby, modifiedby, transactionon, receivable, payable, isactive, createdon, modifiedon, referencetype, referenceno,
        referenceinvoiceno,referenceinvoicedate,obcompanyid,sourceitemid,paid,onadvance,lrinvoicereturnid,customerinvoiceid) 
        VALUES(@voucherid, @version, @voucheritemsid, @ledgerid, @createdby, @modifiedby, @transactionon, @receivable, @payable, 
        @isactive, @createdon, @modifiedon, @referencetype, @referenceno,
        @referenceinvoiceno,@referenceinvoicedate,@obcompanyid,@sourceitemid,@paid,@onadvance,@lrinvoicereturnid,@customerinvoiceid) RETURNING id";


        const string SQL_Update = @"UPDATE public.voucheritembillwise SET voucherid=@voucherid, version=@version+1, 
        voucheritemsid=@voucheritemsid, ledgerid=@ledgerid, modifiedby=@modifiedby, transactionon=@transactionon, 
        receivable=@receivable, payable=@payable, modifiedon=@modifiedon, referencetype=@referencetype, onadvance=@onadvance,
        referenceno=@referenceno,sourceitemid=@sourceitemid,paid=@paid,adjusted=@adjusted,lrinvoicereturnid=@lrinvoicereturnid,customerinvoiceid=@customerinvoiceid,
        referenceinvoiceno=@referenceinvoiceno,referenceinvoicedate=@referenceinvoicedate,obcompanyid=@obcompanyid where id=@id and version=@version RETURNING version";

        const string SQL_Delete = @"update public.voucheritembillwise set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";
        const string SQL_DeleteAll = @"update public.voucheritembillwise set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id in ({0})";

        public List<Voucheritembillwise> Search(VoucheritembillwiseCreteria _voucheritembillwise)
        {
            List<Voucheritembillwise> lst = new List<Voucheritembillwise>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_voucheritembillwise.id != 0)
                {
                    command = new NpgsqlCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _voucheritembillwise.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("isactive", true, "=");

                    if (_voucheritembillwise.showopening)
                    {
                        qb.AddParameter("voucherid", 0, "=");
                        qb.AddParameter("ledgerid", _voucheritembillwise.ledgerid, "=");
                    }
                    else
                    {
                        if (_voucheritembillwise.ledgerid == 0 &&
                            _voucheritembillwise.voucherid == 0 &&
                            (_voucheritembillwise.voucherids == null || _voucheritembillwise.voucherids.Count == 0) &&
                            (_voucheritembillwise.ids == null || _voucheritembillwise.ids.Count == 0) &&
                            _voucheritembillwise.sourceitemid == 0 &&
                            (_voucheritembillwise.sourceitemids == null || _voucheritembillwise.sourceitemids.Count == 0))
                        {
                            throw new Exception("Please provide minimum one condition for searching billwise details");
                        }

                        if (_voucheritembillwise.onlypaid)
                            qb.AddParameter("paid", 0, ">");

                        if (_voucheritembillwise.voucherid != 0)
                            qb.AddParameter("voucherid", _voucheritembillwise.voucherid, "=");

                        if (_voucheritembillwise.voucherids != null && _voucheritembillwise.voucherids.Count > 0)
                            qb.AddInParamer("voucherid", string.Join(",", _voucheritembillwise.voucherids));

                        if (_voucheritembillwise.ledgerid != 0)
                            qb.AddParameter("ledgerid", _voucheritembillwise.ledgerid, "=");

                        if (_voucheritembillwise.ids != null && _voucheritembillwise.ids.Count > 0)
                            qb.AddInParamer("id", string.Join(",", _voucheritembillwise.ids));

                        if (_voucheritembillwise.sourceitemid != 0)
                            qb.AddParameter("sourceitemid", _voucheritembillwise.sourceitemid, "=");

                        if (_voucheritembillwise.sourceitemids != null && _voucheritembillwise.sourceitemids.Count > 0)
                            qb.AddInParamer("sourceitemid", string.Join(",", _voucheritembillwise.sourceitemids));
                    }
                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Voucheritembillwise ovoucheritembillwise = new Voucheritembillwise();
                        ovoucheritembillwise.voucherid = reader["voucherid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["voucherid"]);

                        ovoucheritembillwise.lrinvoicereturnid = reader["lrinvoicereturnid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["lrinvoicereturnid"]);
                        ovoucheritembillwise.customerinvoiceid = reader["customerinvoiceid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["customerinvoiceid"]);

                        ovoucheritembillwise.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        ovoucheritembillwise.voucheritemsid = reader["voucheritemsid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["voucheritemsid"]);
                        ovoucheritembillwise.ledgerid = reader["ledgerid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["ledgerid"]);
                        ovoucheritembillwise.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        ovoucheritembillwise.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        ovoucheritembillwise.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        ovoucheritembillwise.obcompanyid = reader["obcompanyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["obcompanyid"]);
                        ovoucheritembillwise.transactionon = reader["transactionon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["transactionon"]);
                        ovoucheritembillwise.receivable = reader["receivable"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["receivable"]);
                        ovoucheritembillwise.payable = reader["payable"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["payable"]);
                        ovoucheritembillwise.paid = reader["paid"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["paid"]);
                        ovoucheritembillwise.adjusted = reader["adjusted"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["adjusted"]);
                        ovoucheritembillwise.sourceitemid = reader["sourceitemid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sourceitemid"]);
                        ovoucheritembillwise.onadvance = reader["onadvance"] == DBNull.Value ? false : Convert.ToBoolean(reader["onadvance"]);
                        ovoucheritembillwise.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        ovoucheritembillwise.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        ovoucheritembillwise.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        ovoucheritembillwise.referencetype = reader["referencetype"] == DBNull.Value ? "" : reader["referencetype"].ToString();
                        ovoucheritembillwise.referenceno = reader["referenceno"] == DBNull.Value ? "" : reader["referenceno"].ToString();
                        ovoucheritembillwise.referenceinvoiceno = reader["referenceinvoiceno"] == DBNull.Value ? "" : Convert.ToString(reader["referenceinvoiceno"]);
                        ovoucheritembillwise.referenceinvoicedate = reader["referenceinvoicedate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["referenceinvoicedate"]);
                        lst.Add(ovoucheritembillwise);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _voucheritembillwise);
            }
            return lst;
        }

        public bool Insert(List<Voucheritembillwise> oVoucheritembillwise)
        {
            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    foreach (var item in oVoucheritembillwise)
                    {
                        Insert(transaction, conn, item);
                    }
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oVoucheritembillwise);
                return false;
            }
            return true;
        }

        public Voucheritembillwise Insert(Voucheritembillwise oVoucheritembillwise)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);

                    Insert(transaction, conn, oVoucheritembillwise);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oVoucheritembillwise);
            }
            return oVoucheritembillwise;
        }

        public Voucheritembillwise Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Voucheritembillwise oVoucheritembillwise)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("voucherid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oVoucheritembillwise.voucherid;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oVoucheritembillwise.version;
            command.Parameters.Add("voucheritemsid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oVoucheritembillwise.voucheritemsid;
            command.Parameters.Add("ledgerid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oVoucheritembillwise.ledgerid;

            command.Parameters.Add("customerinvoiceid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oVoucheritembillwise.customerinvoiceid;
            command.Parameters.Add("lrinvoicereturnid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oVoucheritembillwise.lrinvoicereturnid;

            command.Parameters.Add("sourceitemid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oVoucheritembillwise.sourceitemid;
            command.Parameters.Add("paid", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oVoucheritembillwise.paid;
            command.Parameters.Add("adjusted", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oVoucheritembillwise.adjusted;
            command.Parameters.Add("obcompanyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oVoucheritembillwise.obcompanyid;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oVoucheritembillwise.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oVoucheritembillwise.modifiedby;
            command.Parameters.Add("transactionon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oVoucheritembillwise.transactionon;
            command.Parameters.Add("receivable", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oVoucheritembillwise.receivable;
            command.Parameters.Add("payable", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oVoucheritembillwise.payable;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oVoucheritembillwise.isactive;
            command.Parameters.Add("onadvance", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oVoucheritembillwise.onadvance;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oVoucheritembillwise.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oVoucheritembillwise.modifiedon;
            command.Parameters.Add("referencetype", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oVoucheritembillwise.referencetype == null ? "" : oVoucheritembillwise.referencetype;
            command.Parameters.Add("referenceno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oVoucheritembillwise.referenceno == null ? "" : oVoucheritembillwise.referenceno;
            command.Parameters.Add("referenceinvoiceno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oVoucheritembillwise.referenceinvoiceno == null ? "" : oVoucheritembillwise.referenceinvoiceno;
            command.Parameters.Add("referenceinvoicedate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oVoucheritembillwise.referenceinvoicedate;

            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oVoucheritembillwise.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            return oVoucheritembillwise;
        }

        public Voucheritembillwise Update(Voucheritembillwise oVoucheritembillwise)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oVoucheritembillwise);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oVoucheritembillwise);
            }

            return oVoucheritembillwise;
        }

        public Voucheritembillwise Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Voucheritembillwise oVoucheritembillwise)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("voucherid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oVoucheritembillwise.voucherid;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oVoucheritembillwise.version;
            command.Parameters.Add("voucheritemsid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oVoucheritembillwise.voucheritemsid;
            command.Parameters.Add("ledgerid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oVoucheritembillwise.ledgerid;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oVoucheritembillwise.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oVoucheritembillwise.modifiedby;
            command.Parameters.Add("obcompanyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oVoucheritembillwise.obcompanyid;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oVoucheritembillwise.id;
            command.Parameters.Add("transactionon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oVoucheritembillwise.transactionon;
            command.Parameters.Add("customerinvoiceid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oVoucheritembillwise.customerinvoiceid;
            command.Parameters.Add("lrinvoicereturnid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oVoucheritembillwise.lrinvoicereturnid;
            command.Parameters.Add("receivable", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oVoucheritembillwise.receivable;
            command.Parameters.Add("payable", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oVoucheritembillwise.payable;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oVoucheritembillwise.isactive;
            command.Parameters.Add("onadvance", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oVoucheritembillwise.onadvance;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oVoucheritembillwise.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oVoucheritembillwise.modifiedon;
            command.Parameters.Add("referencetype", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oVoucheritembillwise.referencetype == null ? "" : oVoucheritembillwise.referencetype;
            command.Parameters.Add("referenceno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oVoucheritembillwise.referenceno == null ? "" : oVoucheritembillwise.referenceno;
            command.Parameters.Add("sourceitemid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oVoucheritembillwise.sourceitemid;
            command.Parameters.Add("paid", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oVoucheritembillwise.paid;
            command.Parameters.Add("adjusted", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oVoucheritembillwise.adjusted;
            command.Parameters.Add("referenceinvoiceno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oVoucheritembillwise.referenceinvoiceno == null ? "" : oVoucheritembillwise.referenceinvoiceno;
            command.Parameters.Add("referenceinvoicedate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oVoucheritembillwise.referenceinvoicedate;

            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oVoucheritembillwise.version = Convert.ToInt32(reader[0].ToString());
                }
            }

            return oVoucheritembillwise;
        }

        public bool DeleteAll(VoucheritembillwiseCreteria oVoucheritembillwise)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = new NpgsqlCommand(string.Format(SQL_DeleteAll, String.Join(",", oVoucheritembillwise.ids), conn, transaction));
                    command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oVoucheritembillwise.isactive;
                    command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                    command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTime.Now;
                    command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oVoucheritembillwise.version;
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oVoucheritembillwise);

            }
            return hasSuccessCount;

        }



        public bool Delete(Voucheritembillwise oVoucheritembillwise)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oVoucheritembillwise);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oVoucheritembillwise);

            }
            return hasSuccessCount;

        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Voucheritembillwise oVoucheritembillwise)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oVoucheritembillwise.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oVoucheritembillwise.modifiedby;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oVoucheritembillwise.modifiedon;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oVoucheritembillwise.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oVoucheritembillwise.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }



        internal List<BillwiseItem> Receivable(VoucherCreteria voucher)
        {
            var lst = new List<BillwiseItem>();

            string query = @"select id,referencetype,referenceno,referenceinvoicedate,referenceinvoiceno,transactionon,receivable,adjusted,onadvance,paid,ledgerid 
            from Voucheritembillwise where Receivable!=adjusted and Voucheritembillwise.isactive=true and onadvance=@onadvance and 
            sourceitemid = 0 and transactionon<@voucherdate and sourceitemid = 0 and (voucherid>0 or obcompanyid=@obcompanyid)";

            if (voucher.onadvance)
            {
                query = query.Replace("and receivable!=adjusted", "and paid!=adjusted");
            }

            var param = new SortedList<string, object>();
            param.Add("ledgerid", voucher.adjustmentledgerid);
            param.Add("companyid", voucher.companyid);
            param.Add("onadvance", voucher.onadvance);
            param.Add("voucherdate", voucher.voucherdate);
            List<long> ledgerids = new List<long>();
            if (voucher.bwise > 0)
                ledgerids.Add(voucher.adjustmentledgerid);
            if (voucher.voucheritems != null)
                ledgerids.AddRange(voucher.voucheritems.FindAll(x => x.bwise > 0).ConvertAll(y => y.ledgerid));
            if (ledgerids.Count == 1)
            {
                query += " and ledgerid=@ledgerid";
                param.Remove("ledgerid");
                param.Add("ledgerid", ledgerids[0]);
            }
            else if (ledgerids.Count > 1)
            {
                query += " and ledgerid in (" + String.Join(",", ledgerids) + ")";
            }

            using (System.Data.IDataReader reader = ExecuteReader(query, param))
            {
                while (reader.Read())
                {
                    BillwiseItem ovoucheritembillwise = new BillwiseItem();
                    ovoucheritembillwise.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                    ovoucheritembillwise.ledgerid = reader["ledgerid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["ledgerid"]);
                    ovoucheritembillwise.transactionon = reader["transactionon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["transactionon"]);
                    ovoucheritembillwise.amount = reader["receivable"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["receivable"]);
                    if (voucher.onadvance)
                    {
                        ovoucheritembillwise.amount = reader["paid"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["paid"]);
                    }
                    ovoucheritembillwise.adjusted = reader["adjusted"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["adjusted"]);
                    ovoucheritembillwise.referencetype = reader["referencetype"] == DBNull.Value ? "" : reader["referencetype"].ToString();
                    ovoucheritembillwise.referenceno = reader["referenceno"] == DBNull.Value ? "" : reader["referenceno"].ToString();
                    ovoucheritembillwise.referenceinvoicedate = reader["referenceinvoicedate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["referenceinvoicedate"]);
                    ovoucheritembillwise.referenceinvoiceno = reader["referenceinvoiceno"] == DBNull.Value ? "" : reader["referenceinvoiceno"].ToString();
                    lst.Add(ovoucheritembillwise);
                }
            }

            if (voucher.id > 0)
            {
                lst = SyncCurrentVoucherBillable(voucher.id, voucher.adjustmentledgerid, lst, voucher.onadvance);
            }
            return lst;
        }

        internal List<BillwiseItem> SyncCurrentVoucherBillable(long voucherid, long ledgerid, List<BillwiseItem> lst, bool isadvance)
        {
            var existing = Search(new VoucheritembillwiseCreteria()
            {
                voucherid = voucherid,
                ledgerid = ledgerid
            });

            var existingadjusted = Search(new VoucheritembillwiseCreteria()
            {
                ids = existing.ConvertAll(x => x.sourceitemid)
            });

            List<Voucheritembillwise> missing = new List<Voucheritembillwise>();
            existing.ForEach(item =>
            {
                if (lst.Exists(x => x.id == item.sourceitemid))
                {
                    var exitem = lst.Find(x => x.id == item.sourceitemid);
                    exitem.adjusted -= item.paid;
                    exitem.paid = item.paid;
                }
                else if (item.sourceitemid > 0)
                {
                    var existmissing = existingadjusted.Find(x => x.id == item.sourceitemid);
                    existmissing.adjusted -= item.paid;
                    lst.Add(new BillwiseItem()
                    {
                        id = existmissing.id,
                        transactionon = existmissing.transactionon,
                        amount = existmissing.payable > 0 ? existmissing.payable : existmissing.receivable,
                        adjusted = existmissing.adjusted,
                        referencetype = existmissing.referencetype,
                        referenceno = existmissing.referenceno,
                        referenceinvoiceno = existmissing.referenceinvoiceno,
                        referenceinvoicedate = existmissing.referenceinvoicedate,
                        paid = item.paid
                    });
                }
                else if (item.adjusted > 0)
                {
                    lst.Add(new BillwiseItem()
                    {
                        id = item.id,
                        transactionon = item.transactionon,
                        adjusted = item.adjusted,
                        referencetype = item.referencetype,
                        referenceno = item.referenceno,
                        referenceinvoicedate = item.referenceinvoicedate,
                        referenceinvoiceno = item.referenceinvoiceno,
                        paid = item.paid,
                        onadvance = true
                    });
                }
            });
            return lst;
        }
        internal List<PayableReceivableLedger> SearchLedgerReceivableDetail(LedgerListCreteria voucher)
        {
            string ledgername = @"[{ 'id': -1, 'typeid': 1, 'name': 'Payment' },
            { 'id': -2, 'typeid': 2, 'name': 'Receipt' },
            { 'id': -3, 'typeid': 3, 'name': 'Purchase' },
            { 'id': -4, 'typeid': 4, 'name': 'Sales' },
            { 'id': -5, 'typeid': 5, 'name': 'Credit Note' },
            { 'id': -6, 'typeid': 6, 'name': 'Debit Note' },
            { 'id': -7, 'typeid': 7, 'name': 'Contra CR' },
            { 'id': -8, 'typeid': 8, 'name': 'Journal CR' },
            { 'id': -9, 'typeid': 9, 'name': 'Cash Deposit' },
            { 'id': -10, 'typeid': 10, 'name': 'Cash Widhrawal' },
            { 'id': -11, 'typeid': 11, 'name': 'Main Cash to Petty Cash' },
            { 'id': -12, 'typeid': 12, 'name': 'Petty Cash to Main Cash' },
            { 'id': -13, 'typeid': 13, 'name': 'Cash Counter Expense' },
            { 'id': -14, 'typeid': 14, 'name': 'Advance Payment' },
            { 'id': -15, 'typeid': 15, 'name': 'Petty Cash Expense' },
            { 'id': -16, 'typeid': 16, 'name': 'Journal DR' },
            { 'id': -17, 'typeid': 17, 'name': 'Contra DR' },
            { 'id': -99, 'typeid': 99, 'name': 'EOD Posting' },
            { 'id': -101, 'typeid': 4, 'name': 'Customer Sales' },
            { 'id': -102, 'typeid': 4, 'name': 'Cusotmer Sales Return' },
            { 'id': -103, 'typeid': 4, 'name': 'Counter Sales' },
            { 'id': -104, 'typeid': 4, 'name': 'Counter Sales Return' },
            { 'id': -105, 'typeid': 3, 'name': 'Purchase Inventory' },
            { 'id': -106, 'typeid': 3, 'name': 'Purchase Return' },
            { 'id': -107, 'typeid': 2, 'name': 'Credit Payment' },
            { 'id': -108, 'typeid': 1, 'name': 'Supplier Payment' }]";

            List<InfyPOS.Data.VoucherDataService.LedgerTypes> ledgerTypes = JsonConvert.DeserializeObject<List<InfyPOS.Data.VoucherDataService.LedgerTypes>>(ledgername);


            var rangeValues = voucher.GetAgeList();
            var lst = new List<PayableReceivableLedger>();
            string query = @"select ledger.displayname,transactionon,receivable,adjusted,
                            referencetype,referenceno,referenceinvoicedate,referenceinvoiceno,
                            employee.name createdby,voucher.vouchertype,voucher.id voucherid
                            from Voucheritembillwise
                            inner join voucher on voucher.id = Voucheritembillwise.voucherid
                            inner join ledger on ledger.id = Voucheritembillwise.ledgerid
                            left outer join users on users.id = voucher.createdby
                            left outer join employee on employee.id = users.employeeid
                            where Voucheritembillwise.isactive=true and receivable-adjusted>0";

            QueryBuilder qb = new QueryBuilder();
            qb.CurrentQuery = query;
            if (voucher.fromdate.Date.Year > 2000)
                qb.AddParameter("transactionon", "fromdate", voucher.fromdate.Date, ">=");
            qb.AddParameter("transactionon", "todate", voucher.todate.Date.AddDays(1), "<");
            if (voucher.ledgerid > 0)
                qb.AddParameter("Voucheritembillwise.ledgerid", "ledgerid", voucher.ledgerid, "=");
            if (voucher.locationid > 0)
                qb.AddParameter("voucher.locationid", "locationid", voucher.locationid, "=");
            if (voucher.companyid > 0)
                qb.AddParameter("voucher.companyid", "companyid", voucher.companyid, "=");

            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                var command = qb.GetCommand(conn);
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        PayableReceivableLedger payableReceivableLedger = new PayableReceivableLedger();
                        payableReceivableLedger.displayname = reader["displayname"] == DBNull.Value ? "" : reader["displayname"].ToString();
                        payableReceivableLedger.referenceno = reader["referenceno"] == DBNull.Value ? "" : reader["referenceno"].ToString();
                        payableReceivableLedger.createdby = reader["createdby"] == DBNull.Value ? "" : reader["createdby"].ToString();
                        payableReceivableLedger.referencetype = reader["referencetype"] == DBNull.Value ? "" : reader["referencetype"].ToString();
                        payableReceivableLedger.voucherid = reader["voucherid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["voucherid"]);
                        if (string.IsNullOrEmpty(payableReceivableLedger.referencetype))
                        {
                            int vouchertype = reader["vouchertype"] == DBNull.Value ? -1 : Convert.ToInt32(reader["vouchertype"]);
                            if (vouchertype != -1 && ledgerTypes.Exists(x => x.typeid == vouchertype))
                            {
                                payableReceivableLedger.referencetype = ledgerTypes.Find(x => x.typeid == vouchertype).name;
                            }
                        }

                        payableReceivableLedger.referenceinvoicedate = reader["referenceinvoicedate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["referenceinvoicedate"]);
                        payableReceivableLedger.referenceinvoiceno = reader["referenceinvoiceno"] == DBNull.Value ? "" : reader["referenceinvoiceno"].ToString();
                        payableReceivableLedger.transactionon = reader["transactionon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["transactionon"]);
                        payableReceivableLedger.amount = reader["receivable"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["receivable"]);
                        payableReceivableLedger.adjusted = reader["adjusted"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["adjusted"]);
                        var days = (DateTimeNow.Date - payableReceivableLedger.transactionon.Date).TotalDays;
                        if (rangeValues.Count > 0)
                        {
                            payableReceivableLedger.age = rangeValues.Exists(e => e.from <= days && e.to >= days) ?
                                    rangeValues.Find(e => e.from <= days && e.to >= days).display : rangeValues.Last().to.ToString("N0") + " >";
                        }
                        lst.Add(payableReceivableLedger);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, voucher);
            }
            return lst;
        }
        internal List<PayableReceivableLedger> SearchReceivable(LedgerListCreteria voucher)
        {
            List<PayableReceivableLedger> payables = new List<PayableReceivableLedger>();
            if (voucher.sourceid == 0 || voucher.sourceid == 2 || voucher.sourceid == 5)
            {
                if (voucher.reporttypeid == 0 || voucher.reporttypeid == 2 || voucher.sourceid == 5)
                {
                    payables.AddRange(SearchLedgerReceivableDetail(voucher));
                }
                else
                {
                    payables.AddRange(SearchLedgerReceivableSummary(voucher));
                }
            }

            if (voucher.sourceid == 1 || voucher.sourceid == 2 || voucher.sourceid == 5)
            {
                payables.AddRange(SearchCustomerReceivable(voucher));
            }
            return payables;
        }
        internal List<PayableReceivableLedger> SearchCustomerReceivable(LedgerListCreteria voucher)
        {
            RetailReportDataService RetailReportData = new RetailReportDataService() { Context = this.Context };
            var list = RetailReportData.SearchCustomerPayment(new RetailReportCreteria()
            {
                fromdate = voucher.fromdate,
                customerid = voucher.ledgerid,
                todate = voucher.todate,
                companyid = voucher.companyid,
                locationid = voucher.locationid,
                showdetail = voucher.reporttypeid == 0 || voucher.reporttypeid == 2,
                showsalesummary = voucher.reporttypeid == 1
            });
            if (voucher.reporttypeid == 0 || voucher.reporttypeid == 1)
            {
                return list.ConvertAll(x => new PayableReceivableLedger()
                {
                    adjusted = x.paid,
                    amount = x.credit - x.discount,
                    age = (DateTime.Now.Date - x.transactiondate).TotalDays.ToString("F0"),
                    city = x.city,
                    displayname = x.customername,
                    referenceinvoicedate = x.transactiondate,
                    referenceinvoiceno = x.billno,
                    locationid = x.locationid,
                    referenceno = x.billno,
                    transactionon = x.transactiondate,
                    createdby = x.createdby,
                    referencetype = "Sales"
                });
            }
            else
            {
                var rangeValues = voucher.GetAgeList();
                if (rangeValues.Count > 0)
                {
                    list.ForEach(x =>
                    {
                        var days = (DateTimeNow.Date - x.transactiondate.Date).TotalDays;
                        if (rangeValues.Count > 0)
                        {
                            x.age = rangeValues.Exists(e => e.from <= days && e.to >= days) ?
                                    rangeValues.Find(e => e.from <= days && e.to >= days).display : rangeValues.Last().to.ToString("N0") + " >";
                        }
                    });
                }
                else
                {
                    list.ForEach(x =>
                    {
                        x.age = (DateTimeNow.Date - x.transactiondate.Date).TotalDays.ToString("F0");
                    });
                }

                if (voucher.reporttypeid == 3)
                {
                    return list.GroupBy(e => new
                    {
                        e.customername,
                        e.city,
                        e.age,
                    }).ToList().ConvertAll(x => new PayableReceivableLedger()
                    {
                        adjusted = x.Sum(y => y.paid),
                        amount = x.Sum(y => y.credit - y.discount),
                        age = x.Key.age,
                        city = x.Key.city,
                        bills = x.Count(),
                        displayname = x.Key.customername,
                    });
                }
                else
                {
                    return list.ConvertAll(x => new PayableReceivableLedger()
                    {
                        adjusted = x.paid,
                        amount = x.credit - x.discount,
                        age = (DateTime.Now.Date - x.transactiondate.Date).TotalDays.ToString("F0"),
                        city = x.city,
                        displayname = x.customername,
                        referenceinvoicedate = x.transactiondate,
                        referenceinvoiceno = x.billno,
                        referenceno = x.billno,
                        createdby = x.createdby,
                        transactionon = x.transactiondate,
                        referencetype = "Sales",
                        locationid = x.locationid
                    });
                }
            }
        }
        internal List<PayableReceivableLedger> SearchPayable(LedgerListCreteria voucher)
        {
            List<PayableReceivableLedger> payables = new List<PayableReceivableLedger>();
            if (voucher.sourceid == 0 || voucher.sourceid == 5)
            {
                if (voucher.reporttypeid == 0 || voucher.reporttypeid == 2)
                {
                    payables.AddRange(SearchLedgerPayableDetail(voucher));
                }
                else
                {
                    payables.AddRange(SearchLedgerPayableSummary(voucher));
                }
            }

            if (voucher.sourceid == 2 || voucher.sourceid == 5)
            {
                payables.AddRange(SearchTransportPayable(voucher));
            }
            if (voucher.sourceid == 1 || voucher.sourceid == 5)
            {
                payables.AddRange(SearchSupplierPayable(voucher));
            }
            return payables;
        }
        internal List<PayableReceivableLedger> SearchTransportPayable(LedgerListCreteria voucher)
        {
            TransportPaymentDataService supplierpaymentDataService = new TransportPaymentDataService() { Context = this.Context };
            var list = supplierpaymentDataService.SearchPendingPayment(new TransportPaymentCreteria()
            {
                entryfromdate = voucher.fromdate,
                entrytodate = voucher.todate,
                ageranges = voucher.ageranges,
                companyid = voucher.companyid,
                locationid = voucher.locationid,
                sourcemode = 0
            });

            list.AddRange(supplierpaymentDataService.SearchPendingPayment(new TransportPaymentCreteria()
            {
                entryfromdate = voucher.fromdate,
                entrytodate = voucher.todate,
                ageranges = voucher.ageranges,
                companyid = voucher.companyid,
                locationid = voucher.locationid,
                sourcemode = 1
            }));
            if (voucher.reporttypeid == 0 || voucher.reporttypeid == 2)
            {
                return list.ConvertAll(x => new PayableReceivableLedger()
                {
                    adjusted = x.paidamount,
                    amount = x.totalamount,
                    age = x.days,
                    city = x.city,
                    displayname = x.supplier,
                    referenceinvoicedate = x.invoicedate,
                    referenceinvoiceno = x.invoiceno,
                    referenceno = x.lrentryno,
                    transactionon = x.entrydate,
                    referencetype = x.source
                });
            }
            else
            {
                return list.GroupBy(e => new { e.supplier, e.city, e.days }).ToList().ConvertAll(x => new PayableReceivableLedger()
                {
                    adjusted = x.Sum(y => y.paidamount),
                    amount = x.Sum(y => y.totalamount),
                    age = x.Key.days,
                    city = x.Key.city,
                    bills = x.Count(),
                    displayname = x.Key.supplier,
                });
            }
        }
        internal List<PayableReceivableLedger> SearchSupplierPayable(LedgerListCreteria voucher)
        {
            SupplierpaymentDataService supplierpaymentDataService = new SupplierpaymentDataService() { Context = this.Context };
            var list = supplierpaymentDataService.SearchPendingPayment(new SupplierpaymentCreteria()
            {
                entryfromdate = voucher.fromdate,
                entrytodate = voucher.todate,
                ageranges = voucher.ageranges,
                companyid = voucher.companyid,
                locationid = voucher.locationid,
            });
            if (voucher.reporttypeid == 0 || voucher.reporttypeid == 2)
            {
                EmployeeDataService usersDataService = new EmployeeDataService() { Context = this.Context };
                var usernames = usersDataService.GetUserName(list.ConvertAll(x => x.createdby).ToList().Distinct().ToList());
                return list.ConvertAll(x => new PayableReceivableLedger()
                {
                    adjusted = x.paidamount,
                    amount = x.totalamount,
                    age = x.days,
                    city = x.city,
                    createdby = usernames.Exists(y => y.id == x.createdby) ? usernames.Find(y => y.id == x.createdby).name : "",
                    displayname = x.supplier,
                    referenceinvoicedate = x.invoicedate,
                    referenceinvoiceno = x.invoiceno,
                    referenceno = x.lrentryno,
                    locationid = x.locationid,
                    transactionon = x.entrydate,
                    referencetype = "Purchase",
                });
            }
            else
            {
                return list.GroupBy(e => new { e.supplier, e.locationid, e.city, e.days }).ToList().ConvertAll(x => new PayableReceivableLedger()
                {
                    adjusted = x.Sum(y => y.paidamount),
                    amount = x.Sum(y => y.totalamount),
                    age = x.Key.days,
                    locationid = x.Key.locationid,
                    city = x.Key.city,
                    bills = x.Count(),
                    displayname = x.Key.supplier,
                });
            }
        }
        internal List<PayableReceivableLedger> SearchLedgerPayableSummary(LedgerListCreteria voucher)
        {
            var lst = new List<PayableReceivableLedger>();
            string query = @"select ledger.displayname,count(*) bills,sum(payable) payable,sum(adjusted) adjusted,referencelist.name city
                            {0}
                            from Voucheritembillwise
                            inner join ledger on ledger.id = Voucheritembillwise.ledgerid   
                            inner join voucher on voucher.id = Voucheritembillwise.voucherid   
                            left outer join referencelist on referencelist.id = cityid
                            where Voucheritembillwise.isactive=true and payable-adjusted>0";

            QueryBuilder qb = new QueryBuilder();
            qb.CurrentQuery = query;
            if (voucher.fromdate.Date.Year > 2000)
                qb.AddParameter("transactionon", "fromdate", voucher.fromdate.Date, ">=");
            if (voucher.todate.Date.Year > 2000)
                qb.AddParameter("transactionon", "todate", voucher.todate.Date.AddDays(1), "<");
            if (voucher.ledgerid > 0)
                qb.AddParameter("ledgerid", "ledgerid", voucher.ledgerid, "=");
            if (voucher.locationid > 0)
                qb.AddParameter("voucher.locationid", "locationid", voucher.locationid, "=");
            if (voucher.companyid > 0)
                qb.AddParameter("voucher.companyid", "companyid", voucher.companyid, "=");
            qb.CurrentQuery = string.Format(qb.CurrentQuery, voucher.GetAgerangesquery()) + " group by ledger.displayname,referencelist.name" + voucher.GetAgerangesquery();
            var hasage = voucher.GetAgeList().Count > 0;
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                var command = qb.GetCommand(conn);
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        PayableReceivableLedger payableReceivableLedger = new PayableReceivableLedger();
                        payableReceivableLedger.displayname = reader["displayname"] == DBNull.Value ? "" : reader["displayname"].ToString();
                        payableReceivableLedger.city = reader["city"] == DBNull.Value ? "" : reader["city"].ToString();
                        payableReceivableLedger.amount = reader["payable"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["payable"]);
                        payableReceivableLedger.bills = reader["bills"] == DBNull.Value ? 0 : Convert.ToInt32(reader["bills"]);
                        payableReceivableLedger.adjusted = reader["adjusted"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["adjusted"]);
                        if (hasage)
                        {
                            payableReceivableLedger.age = reader["age"] == DBNull.Value ? "" : Convert.ToString(reader["age"]);
                        }
                        lst.Add(payableReceivableLedger);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, voucher);
            }
            return lst;
        }
        internal List<PayableReceivableLedger> SearchLedgerReceivableSummary(LedgerListCreteria voucher)
        {
            var lst = new List<PayableReceivableLedger>();
            string query = @"select ledger.displayname,count(*) bills,sum(receivable) receivable,sum(adjusted) adjusted,referencelist.name city
                            {0}
                            from Voucheritembillwise
                            inner join ledger on ledger.id = Voucheritembillwise.ledgerid   
                            inner join voucher on voucher.id = Voucheritembillwise.voucherid   
                            left outer join referencelist on referencelist.id = cityid
                            where Voucheritembillwise.isactive=true and receivable-adjusted>0";

            QueryBuilder qb = new QueryBuilder();
            qb.CurrentQuery = query;
            if (voucher.fromdate.Date.Year > 2000)
                qb.AddParameter("transactionon", "fromdate", voucher.fromdate.Date, ">=");
            if (voucher.todate.Date.Year > 2000)
                qb.AddParameter("transactionon", "todate", voucher.todate.Date.AddDays(1), "<");
            if (voucher.ledgerid > 0)
                qb.AddParameter("ledgerid", "ledgerid", voucher.ledgerid, "=");
            if (voucher.companyid > 0)
                qb.AddParameter("voucher.companyid", "companyid", voucher.companyid, "=");
            if (voucher.locationid > 0)
                qb.AddParameter("voucher.locationid", "locationid", voucher.locationid, "=");
            qb.CurrentQuery = string.Format(qb.CurrentQuery, voucher.GetAgerangesquery()) + " group by ledger.displayname,referencelist.name" + voucher.GetAgerangesquery();
            var hasage = voucher.GetAgeList().Count > 0;
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                var command = qb.GetCommand(conn);
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        PayableReceivableLedger payableReceivableLedger = new PayableReceivableLedger();
                        payableReceivableLedger.displayname = reader["displayname"] == DBNull.Value ? "" : reader["displayname"].ToString();
                        payableReceivableLedger.city = reader["city"] == DBNull.Value ? "" : reader["city"].ToString();
                        payableReceivableLedger.amount = reader["receivable"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["receivable"]);
                        payableReceivableLedger.bills = reader["bills"] == DBNull.Value ? 0 : Convert.ToInt32(reader["bills"]);
                        payableReceivableLedger.adjusted = reader["adjusted"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["adjusted"]);
                        if (hasage)
                        {
                            payableReceivableLedger.age = reader["age"] == DBNull.Value ? "" : Convert.ToString(reader["age"]);
                        }
                        lst.Add(payableReceivableLedger);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, voucher);
            }
            return lst;
        }
        internal List<PayableReceivableLedger> SearchLedgerPayableDetail(LedgerListCreteria voucher)
        {
            var rangeValues = voucher.GetAgeList();
            var lst = new List<PayableReceivableLedger>();
            string query = @"select ledger.displayname,transactionon,payable,adjusted,referencetype,
                            referenceno,referenceinvoicedate,referenceinvoiceno,referencelist.name city,
                            employee.name createdby,voucher.locationid,voucher.id voucherid
                            from Voucheritembillwise
                            inner join voucher on voucher.id = Voucheritembillwise.voucherid
                            inner join ledger on ledger.id = Voucheritembillwise.ledgerid   
                            left outer join referencelist on referencelist.id = cityid
                            left outer join users on users.id = voucher.createdby
                            left outer join employee on employee.id = users.employeeid
                            where Voucheritembillwise.isactive=true and payable-adjusted>0";

            QueryBuilder qb = new QueryBuilder();
            qb.CurrentQuery = query;
            if (voucher.fromdate.Date.Year > 2000)
                qb.AddParameter("Voucheritembillwise.transactionon", "fromdate", voucher.fromdate.Date, ">=");
            if (voucher.todate.Date.Year > 2000)
                qb.AddParameter("Voucheritembillwise.transactionon", "todate", voucher.todate.Date.AddDays(1), "<");
            if (voucher.ledgerid > 0)
                qb.AddParameter("Voucheritembillwise.ledgerid", "ledgerid", voucher.ledgerid, "=");
            if (voucher.locationid > 0)
                qb.AddParameter("voucher.locationid", "locationid", voucher.locationid, "=");
            if (voucher.companyid > 0)
                qb.AddParameter("voucher.companyid", "companyid", voucher.companyid, "=");
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                var command = qb.GetCommand(conn);
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        PayableReceivableLedger payableReceivableLedger = new PayableReceivableLedger();
                        payableReceivableLedger.displayname = reader["displayname"] == DBNull.Value ? "" : reader["displayname"].ToString();
                        payableReceivableLedger.createdby = reader["createdby"] == DBNull.Value ? "" : reader["createdby"].ToString();
                        payableReceivableLedger.city = reader["city"] == DBNull.Value ? "" : reader["city"].ToString();
                        payableReceivableLedger.referenceno = reader["referenceno"] == DBNull.Value ? "" : reader["referenceno"].ToString();
                        payableReceivableLedger.referencetype = reader["referencetype"] == DBNull.Value ? "" : reader["referencetype"].ToString();
                        payableReceivableLedger.referenceinvoicedate = reader["referenceinvoicedate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["referenceinvoicedate"]);
                        payableReceivableLedger.referenceinvoiceno = reader["referenceinvoiceno"] == DBNull.Value ? "" : reader["referenceinvoiceno"].ToString();
                        payableReceivableLedger.transactionon = reader["transactionon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["transactionon"]);
                        payableReceivableLedger.amount = reader["payable"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["payable"]);
                        payableReceivableLedger.adjusted = reader["adjusted"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["adjusted"]);
                        payableReceivableLedger.locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                        payableReceivableLedger.voucherid = reader["voucherid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["voucherid"]);
                        var days = (DateTimeNow.Date - payableReceivableLedger.transactionon.Date).TotalDays;
                        if (rangeValues.Count > 0)
                        {
                            payableReceivableLedger.age = rangeValues.Exists(e => e.from <= days && e.to >= days) ?
                                    rangeValues.Find(e => e.from <= days && e.to >= days).display : rangeValues.Last().to.ToString("N0") + " >";
                        }
                        lst.Add(payableReceivableLedger);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, voucher);
            }
            return lst;
        }
        internal List<BillwiseItem> Payable(VoucherCreteria voucher)
        {
            var lst = new List<BillwiseItem>();
            string query = @"select id,referencetype,referenceno,
            transactionon,payable,adjusted,onadvance,paid,ledgerid,referenceinvoicedate,referenceinvoiceno
            from Voucheritembillwise where onadvance=@onadvance
            and payable>adjusted and Voucheritembillwise.isactive=true 
            and transactionon<@voucherdate and sourceitemid = 0
            and (voucherid>0 or obcompanyid=@obcompanyid)";
            if (voucher.onadvance)
            {
                query = query.Replace("and payable!=adjusted", "and paid!=adjusted");
            }

            var param = new SortedList<string, object>();
            List<long> ledgerids = new List<long>();
            if (voucher.bwise > 0)
                ledgerids.Add(voucher.adjustmentledgerid);
            if (voucher.voucheritems != null)
                ledgerids.AddRange(voucher.voucheritems.FindAll(x => x.bwise > 0).ConvertAll(y => y.ledgerid));
            if (ledgerids.Count == 1)
            {
                query += " and ledgerid=@ledgerid";
                param.Add("ledgerid", ledgerids[0]);
            }
            else if (ledgerids.Count > 1)
            {
                query += " and ledgerid in (" + String.Join(",", ledgerids) + ")";
            }



            param.Add("voucherdate", voucher.voucherdate.Date.AddDays(1));
            param.Add("onadvance", voucher.onadvance);
            param.Add("obcompanyid", voucher.companyid);
            using (System.Data.IDataReader reader = ExecuteReader(query, param))
            {
                while (reader.Read())
                {
                    BillwiseItem ovoucheritembillwise = new BillwiseItem();
                    ovoucheritembillwise.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                    ovoucheritembillwise.ledgerid = reader["ledgerid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["ledgerid"]);
                    ovoucheritembillwise.transactionon = reader["transactionon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["transactionon"]);
                    ovoucheritembillwise.amount = reader["payable"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["payable"]);
                    if (voucher.onadvance)
                    {
                        ovoucheritembillwise.amount = reader["paid"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["paid"]);
                        ovoucheritembillwise.adjusted = reader["adjusted"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["adjusted"]);
                    }
                    else
                    {
                        ovoucheritembillwise.adjusted = reader["adjusted"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["adjusted"]);
                    }
                    //ovoucheritembillwise.amount -= ovoucheritembillwise.adjusted;
                    ovoucheritembillwise.referencetype = reader["referencetype"] == DBNull.Value ? "" : reader["referencetype"].ToString();
                    ovoucheritembillwise.referenceno = reader["referenceno"] == DBNull.Value ? "" : reader["referenceno"].ToString();
                    ovoucheritembillwise.referenceinvoicedate = reader["referenceinvoicedate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["referenceinvoicedate"]);
                    ovoucheritembillwise.referenceinvoiceno = reader["referenceinvoiceno"] == DBNull.Value ? "" : reader["referenceinvoiceno"].ToString();

                    lst.Add(ovoucheritembillwise);
                }
            }

            if (voucher.id > 0)
            {
                lst = SyncCurrentVoucherBillable(voucher.id, voucher.adjustmentledgerid, lst, voucher.onadvance);
            }
            return lst;
        }

        List<vouchertype> VoucherTypes
        {
            get
            {
                List<vouchertype> VoucherTypes = new List<vouchertype>(new vouchertype[] {
               new vouchertype (){ id = -1, name = "Payment Voucher" },
               new vouchertype (){ id=-2, name= "Receipt Voucher" },
               new vouchertype (){ id=-3, name= "Purchase Voucher" },
               new vouchertype (){ id=-4, name= "Sales Voucher" },
               new vouchertype (){ id=-5, name= "Credit Note" },
               new vouchertype (){ id=-6, name= "Debit Note" },
               new vouchertype (){ id=-7, name= "Contra Voucher" },
               new vouchertype (){ id=-8, name= "Journal Voucher" },
               new vouchertype (){ id=-9, name= "Cash Deposit" },
               new vouchertype (){ id=-10, name= "Cash Widhrawal" },
               new vouchertype (){ id=-11, name= "Main Cash to Petty Cash" },
               new vouchertype (){ id=-12, name= "Petty Cash to Main Cash" },
               new vouchertype (){ id=-13, name= "Cash Counter Expense" },
               new vouchertype (){ id=-14, name= "Advance Payment Voucher" }});

                ReferencelistDataService referencelistDataService = new ReferencelistDataService() { Context = this.Context };
                var vouchertypes = referencelistDataService.SearchList(new ReferencelistCreteria() { type = "VOUCHERTYPE" });
                if (vouchertypes != null && vouchertypes.Count > 0)
                {
                    vouchertypes.ForEach(x =>
                    {
                        VoucherTypes.Add(new vouchertype()
                        {
                            id = x.id,
                            name = x.name
                        });
                    });
                }
                return VoucherTypes;
            }
        }

        internal void Sync(bool isupdate, NpgsqlConnection conn, NpgsqlTransaction transaction, Voucher oVoucher)
        {

            if (isupdate)
            {
                List<Voucheritembillwise> olditems = Search(new VoucheritembillwiseCreteria()
                {
                    voucherid = oVoucher.id
                });
                if (oVoucher.billadjustement == null || oVoucher.billadjustement.Count == 0)
                {
                    if (olditems.Count > 0)
                    {
                        UpdateBillwiseItems(conn, transaction, oVoucher, olditems);
                    }
                    else
                    {
                        CreateBillwiseItems(conn, transaction, oVoucher);
                    }
                }
                else
                {
                    var advanceitem = olditems.Find(x => x.onadvance);
                    var onadvancelist = oVoucher.billadjustement.Find(x => x.onadvance);
                    if (advanceitem != null && advanceitem.adjusted != 0)
                    {
                        if (onadvancelist == null || onadvancelist.ledgerid != advanceitem.ledgerid)
                        {
                            var advanceadjusted = Search(new VoucheritembillwiseCreteria()
                            {
                                sourceitemid = advanceitem.id
                            });
                            if (advanceadjusted != null)
                            {
                                throw new Exception("Advance amount already adjusted on " +
                                    advanceadjusted[0].referenceno + "(" + advanceadjusted[0].referencetype + "), can't remove the advance");
                            }
                        }

                        advanceitem.paid = onadvancelist.paid;
                        advanceitem.onadvance = true;
                        Update(transaction, conn, advanceitem);
                        oVoucher.billadjustement.Remove(onadvancelist);
                        olditems.Remove(advanceitem);
                    }

                    foreach (var item in oVoucher.billadjustement.Where(x => x.id > 0))
                    {
                        var aitem = olditems.Find(x => x.sourceitemid == item.id);
                        if (aitem != null)
                        {
                            aitem.paid = item.paid;
                            aitem.modifiedby = Context.UserID;
                            aitem.modifiedon = DateTimeNow;
                            Update(transaction, conn, aitem);
                            olditems.Remove(aitem);
                        }
                        else
                        {
                            Voucheritembillwise voucheritembillwise = new Voucheritembillwise();
                            voucheritembillwise.ledgerid = item.ledgerid;
                            voucheritembillwise.transactionon = oVoucher.voucherdate;
                            voucheritembillwise.createdby = Context.UserID;
                            voucheritembillwise.modifiedby = Context.UserID;
                            voucheritembillwise.createdon = DateTimeNow;
                            voucheritembillwise.onadvance = item.onadvance;
                            voucheritembillwise.modifiedon = DateTimeNow;
                            voucheritembillwise.isactive = true;
                            voucheritembillwise.version = 1;
                            if (oVoucher.attributes != null &&
                               !string.IsNullOrEmpty(oVoucher.attributes.invoiceno))
                            {
                                voucheritembillwise.referenceinvoiceno = oVoucher.attributes.invoiceno;
                                voucheritembillwise.referenceinvoicedate = oVoucher.attributes.invoicedate;
                            }
                            voucheritembillwise.referenceno = oVoucher.voucherno;
                            if (VoucherTypes.Exists(x => x.id == oVoucher.vouchertypeid))
                                voucheritembillwise.referencetype = VoucherTypes.Find(x => x.id == oVoucher.vouchertypeid).name;
                            else if (VoucherTypes.Exists(x => x.id == oVoucher.vouchertype * -1))
                                voucheritembillwise.referencetype = VoucherTypes.Find(x => x.id == oVoucher.vouchertype * -1).name;
                            voucheritembillwise.voucherid = oVoucher.id;
                            voucheritembillwise.paid = item.paid;
                            voucheritembillwise.sourceitemid = item.id;
                            if (item.onadvance)
                            {
                                voucheritembillwise.referencetype = "On Advance";
                            }
                            Insert(transaction, conn, voucheritembillwise);
                            if (!item.onadvance)
                                UpdatePayment(conn, transaction, voucheritembillwise.sourceitemid, voucheritembillwise.paid);
                        }
                    }

                    var onadvance = oVoucher.billadjustement.Find(x => x.id == 0);
                    if (onadvance != null)
                    {
                        Voucheritembillwise voucheritembillwise = new Voucheritembillwise();
                        voucheritembillwise.ledgerid = onadvance.ledgerid;
                        voucheritembillwise.transactionon = oVoucher.voucherdate;
                        voucheritembillwise.createdby = Context.UserID;
                        voucheritembillwise.modifiedby = Context.UserID;
                        voucheritembillwise.createdon = DateTimeNow;
                        voucheritembillwise.modifiedon = DateTimeNow;
                        voucheritembillwise.isactive = true;
                        voucheritembillwise.version = 1;
                        voucheritembillwise.onadvance = true;
                        voucheritembillwise.referenceno = oVoucher.voucherno;
                        voucheritembillwise.referencetype = "On Advance";
                        voucheritembillwise.voucherid = oVoucher.id;
                        voucheritembillwise.paid = onadvance.paid;
                        if (oVoucher.attributes != null &&
                            !string.IsNullOrEmpty(oVoucher.attributes.invoiceno))
                        {
                            voucheritembillwise.referenceinvoiceno = oVoucher.attributes.invoiceno;
                            voucheritembillwise.referenceinvoicedate = oVoucher.attributes.invoicedate;
                        }
                        Insert(transaction, conn, voucheritembillwise);
                    }

                    foreach (var ditem in olditems)
                        Delete(transaction, conn, ditem);
                }
            }
            else if (oVoucher.billadjustement != null
                && oVoucher.billadjustement.Count > 0)
            {
                var olditems = Search(new VoucheritembillwiseCreteria()
                {
                    ids = oVoucher.billadjustement.ConvertAll(x => x.id)
                });
                foreach (var item in oVoucher.billadjustement.Where(x => x.id > 0))
                {
                    var aitem = olditems.Find(x => x.id == item.id);
                    Voucheritembillwise voucheritembillwise = new Voucheritembillwise();
                    voucheritembillwise.ledgerid = aitem.ledgerid;
                    voucheritembillwise.transactionon = oVoucher.voucherdate;
                    voucheritembillwise.createdby = Context.UserID;
                    voucheritembillwise.modifiedby = Context.UserID;
                    voucheritembillwise.createdon = DateTimeNow;
                    voucheritembillwise.modifiedon = DateTimeNow;
                    voucheritembillwise.isactive = true;
                    voucheritembillwise.version = 1;
                    voucheritembillwise.referenceno = oVoucher.voucherno;
                    if (VoucherTypes.Exists(x => x.id == oVoucher.vouchertypeid))
                        voucheritembillwise.referencetype = VoucherTypes.Find(x => x.id == oVoucher.vouchertypeid).name;
                    else if (VoucherTypes.Exists(x => x.id == oVoucher.vouchertype * -1))
                        voucheritembillwise.referencetype = VoucherTypes.Find(x => x.id == oVoucher.vouchertype * -1).name;
                    if (oVoucher.attributes != null &&
                    !string.IsNullOrEmpty(oVoucher.attributes.invoiceno))
                    {
                        voucheritembillwise.referenceinvoiceno = oVoucher.attributes.invoiceno;
                        voucheritembillwise.referenceinvoicedate = oVoucher.attributes.invoicedate;
                    }
                    voucheritembillwise.voucherid = oVoucher.id;
                    voucheritembillwise.paid = item.paid;
                    voucheritembillwise.sourceitemid = item.id;
                    Insert(transaction, conn, voucheritembillwise);

                    UpdatePayment(conn, transaction, voucheritembillwise.sourceitemid, voucheritembillwise.paid);
                }

                var onadvance = oVoucher.billadjustement.Find(x => x.id == 0);
                if (onadvance != null)
                {
                    Voucheritembillwise voucheritembillwise = new Voucheritembillwise();
                    voucheritembillwise.ledgerid = onadvance.ledgerid;
                    voucheritembillwise.transactionon = oVoucher.voucherdate;
                    voucheritembillwise.createdby = Context.UserID;
                    voucheritembillwise.modifiedby = Context.UserID;
                    voucheritembillwise.createdon = DateTimeNow;
                    voucheritembillwise.modifiedon = DateTimeNow;
                    voucheritembillwise.isactive = true;
                    voucheritembillwise.version = 1;
                    voucheritembillwise.referenceno = oVoucher.voucherno;
                    voucheritembillwise.referencetype = "On Advance";
                    voucheritembillwise.onadvance = true;
                    voucheritembillwise.voucherid = oVoucher.id;
                    voucheritembillwise.paid = onadvance.paid;
                    if (oVoucher.attributes != null &&
                       !string.IsNullOrEmpty(oVoucher.attributes.invoiceno))
                    {
                        voucheritembillwise.referenceinvoiceno = oVoucher.attributes.invoiceno;
                        voucheritembillwise.referenceinvoicedate = oVoucher.attributes.invoicedate;
                    }
                    Insert(transaction, conn, voucheritembillwise);
                }
            }
            else
            {
                CreateBillwiseItems(conn, transaction, oVoucher);
            }
        }

        public void UpdateBillwiseItems(NpgsqlConnection conn, NpgsqlTransaction transaction,
            Voucher oVoucher, List<Voucheritembillwise> oldvoucheritembillwise)
        {
            if (oVoucher.bwise != 0)
            {
                if (oldvoucheritembillwise.Exists(x => x.ledgerid == oVoucher.adjustmentledgerid))
                {
                    var oldbillable = oldvoucheritembillwise.Find(x => x.ledgerid == oVoucher.adjustmentledgerid);
                    oldbillable.modifiedby = Context.UserID;
                    oldbillable.modifiedon = DateTimeNow;
                    if (oVoucher.bwise == 11)//customer
                    {
                        oldbillable.receivable = oVoucher.amount;
                    }
                    else if (oVoucher.bwise == 12)
                    {
                        oldbillable.payable = oVoucher.amount;
                    }
                    Update(transaction, conn, oldbillable);
                    oldvoucheritembillwise.RemoveAll(x => x.ledgerid == oVoucher.adjustmentledgerid);
                }
                else
                {
                    Voucheritembillwise voucheritembillwise = new Voucheritembillwise();
                    voucheritembillwise.ledgerid = oVoucher.adjustmentledgerid;
                    voucheritembillwise.transactionon = oVoucher.voucherdate;
                    voucheritembillwise.createdby = Context.UserID;
                    voucheritembillwise.modifiedby = Context.UserID;
                    voucheritembillwise.createdon = DateTimeNow;
                    voucheritembillwise.modifiedon = DateTimeNow;
                    voucheritembillwise.isactive = true;
                    voucheritembillwise.version = 1;
                    voucheritembillwise.referenceno = oVoucher.voucherno;
                    if (VoucherTypes.Exists(x => x.id == oVoucher.vouchertypeid))
                        voucheritembillwise.referencetype = VoucherTypes.Find(x => x.id == oVoucher.vouchertypeid).name;
                    else if (VoucherTypes.Exists(x => x.id == oVoucher.vouchertype * -1))
                        voucheritembillwise.referencetype = VoucherTypes.Find(x => x.id == oVoucher.vouchertype * -1).name;
                    if (oVoucher.attributes != null &&
                       !string.IsNullOrEmpty(oVoucher.attributes.invoiceno))
                    {
                        voucheritembillwise.referenceinvoiceno = oVoucher.attributes.invoiceno;
                        voucheritembillwise.referenceinvoicedate = oVoucher.attributes.invoicedate;
                    }
                    voucheritembillwise.voucherid = oVoucher.id;
                    if (oVoucher.bwise == 11)//customer
                    {
                        voucheritembillwise.receivable = oVoucher.amount;
                    }
                    else if (oVoucher.bwise == 12)
                    {
                        voucheritembillwise.payable = oVoucher.amount;
                    }
                    Insert(transaction, conn, voucheritembillwise);
                }
            }

            if (oVoucher.voucheritems.Exists(x => x.bwise != 0))
            {
                foreach (var item in oVoucher.voucheritems)
                {
                    if (item.bwise != 0)
                    {
                        if (oldvoucheritembillwise.Exists(x => x.ledgerid == item.ledgerid))
                        {
                            var oldbillable = oldvoucheritembillwise.Find(x => x.ledgerid == item.ledgerid);
                            oldbillable.modifiedby = Context.UserID;
                            oldbillable.modifiedon = DateTimeNow;
                            if (item.bwise == 11)//customer
                            {
                                oldbillable.receivable = item.amount;
                            }
                            else if (item.bwise == 12)
                            {
                                oldbillable.payable = item.amount;
                            }
                            Update(transaction, conn, oldbillable);
                            oldvoucheritembillwise.RemoveAll(x => x.ledgerid == item.ledgerid);
                        }
                        else
                        {
                            Voucheritembillwise voucheritembillwise = new Voucheritembillwise();
                            voucheritembillwise.ledgerid = item.ledgerid;
                            voucheritembillwise.transactionon = oVoucher.voucherdate;
                            voucheritembillwise.createdby = Context.UserID;
                            voucheritembillwise.modifiedby = Context.UserID;
                            voucheritembillwise.createdon = DateTimeNow;
                            voucheritembillwise.modifiedon = DateTimeNow;
                            voucheritembillwise.isactive = true;
                            voucheritembillwise.version = 1;
                            voucheritembillwise.referenceno = oVoucher.voucherno;
                            if (VoucherTypes.Exists(x => x.id == oVoucher.vouchertypeid))
                                voucheritembillwise.referencetype = VoucherTypes.Find(x => x.id == oVoucher.vouchertypeid).name;
                            else if (VoucherTypes.Exists(x => x.id == oVoucher.vouchertype * -1))
                                voucheritembillwise.referencetype = VoucherTypes.Find(x => x.id == oVoucher.vouchertype * -1).name;
                            if (oVoucher.attributes != null &&
                               !string.IsNullOrEmpty(oVoucher.attributes.invoiceno))
                            {
                                voucheritembillwise.referenceinvoiceno = oVoucher.attributes.invoiceno;
                                voucheritembillwise.referenceinvoicedate = oVoucher.attributes.invoicedate;
                            }
                            voucheritembillwise.voucherid = oVoucher.id;
                            if (item.bwise == 11)//customer
                            {
                                voucheritembillwise.payable = item.amount;
                            }
                            else if (item.bwise == 12)
                            {
                                voucheritembillwise.receivable = item.amount;
                            }
                            Insert(transaction, conn, voucheritembillwise);
                        }
                    }
                }
            }

            if (oldvoucheritembillwise.Exists(x => x.adjusted != 0))
            {
                var oldadjusted = Search(new VoucheritembillwiseCreteria()
                {
                    sourceitemids = oldvoucheritembillwise.FindAll(x => x.adjusted != 0).ConvertAll(x => x.id)
                });

                LedgerDataService ledgerDataService = new LedgerDataService() { Context = this.Context };
                var ledgername = ledgerDataService.Search(new LedgerCreteria()
                {
                    ids = oldadjusted.ConvertAll(x => x.ledgerid)
                });

                string existingadjusted = "";
                oldadjusted.ForEach(x =>
                {
                    var ledger = ledgername.Find(y => y.id == x.ledgerid);
                    existingadjusted += (existingadjusted != "" ? "\r\n" : "") + ledger.displayname + " Adjusted " + x.adjusted + " on Voucher - " + x.referenceno + "(" + x.referencetype + ")";
                });

                if (existingadjusted != "")
                {
                    throw new Exception("Validation failed \r\n" + existingadjusted);
                }
            }

            foreach (var old in oldvoucheritembillwise)
                Delete(transaction, conn, old);
        }
        public void CreateBillwiseItems(NpgsqlConnection conn, NpgsqlTransaction transaction, Voucher oVoucher)
        {
            if (oVoucher.bwise == 0 && oVoucher.voucheritems.All(x => x.bwise == 0)) return;

            if (oVoucher.bwise != 0)
            {
                Voucheritembillwise voucheritembillwise = new Voucheritembillwise();
                voucheritembillwise.ledgerid = oVoucher.adjustmentledgerid;
                voucheritembillwise.transactionon = oVoucher.voucherdate;
                voucheritembillwise.createdby = Context.UserID;
                voucheritembillwise.modifiedby = Context.UserID;
                voucheritembillwise.createdon = DateTimeNow;
                voucheritembillwise.modifiedon = DateTimeNow;
                voucheritembillwise.isactive = true;
                voucheritembillwise.version = 1;
                voucheritembillwise.referenceno = oVoucher.voucherno;
                if (VoucherTypes.Exists(x => x.id == oVoucher.vouchertypeid))
                    voucheritembillwise.referencetype = VoucherTypes.Find(x => x.id == oVoucher.vouchertypeid).name;
                else if (VoucherTypes.Exists(x => x.id == oVoucher.vouchertype * -1))
                    voucheritembillwise.referencetype = VoucherTypes.Find(x => x.id == oVoucher.vouchertype * -1).name;
                if (oVoucher.attributes != null &&
                    !string.IsNullOrEmpty(oVoucher.attributes.invoiceno))
                {
                    voucheritembillwise.referenceinvoiceno = oVoucher.attributes.invoiceno;
                    voucheritembillwise.referenceinvoicedate = oVoucher.attributes.invoicedate;
                }
                voucheritembillwise.voucherid = oVoucher.id;
                if (oVoucher.bwise == 11)//customer
                {
                    voucheritembillwise.receivable = oVoucher.amount;
                }
                else if (oVoucher.bwise == 12)
                {
                    voucheritembillwise.payable = oVoucher.amount;
                }

                Insert(transaction, conn, voucheritembillwise);
            }

            foreach (var item in oVoucher.voucheritems)
            {
                if (item.bwise != 0)
                {
                    Voucheritembillwise voucheritembillwise = new Voucheritembillwise();
                    voucheritembillwise.ledgerid = item.ledgerid;
                    voucheritembillwise.transactionon = oVoucher.voucherdate;
                    voucheritembillwise.createdby = Context.UserID;
                    voucheritembillwise.modifiedby = Context.UserID;
                    voucheritembillwise.createdon = DateTimeNow;
                    voucheritembillwise.modifiedon = DateTimeNow;
                    voucheritembillwise.isactive = true;
                    voucheritembillwise.version = 1;
                    voucheritembillwise.referenceno = oVoucher.voucherno;
                    if (VoucherTypes.Exists(x => x.id == oVoucher.vouchertypeid))
                        voucheritembillwise.referencetype = VoucherTypes.Find(x => x.id == oVoucher.vouchertypeid).name;
                    else if (VoucherTypes.Exists(x => x.id == oVoucher.vouchertype * -1))
                        voucheritembillwise.referencetype = VoucherTypes.Find(x => x.id == oVoucher.vouchertype * -1).name;
                    if (oVoucher.attributes != null &&
                    !string.IsNullOrEmpty(oVoucher.attributes.invoiceno))
                    {
                        voucheritembillwise.referenceinvoiceno = oVoucher.attributes.invoiceno;
                        voucheritembillwise.referenceinvoicedate = oVoucher.attributes.invoicedate;
                    }
                    voucheritembillwise.voucherid = oVoucher.id;
                    if (item.bwise == 11)//customer
                    {
                        voucheritembillwise.payable = item.amount;
                    }
                    else if (item.bwise == 12)
                    {
                        voucheritembillwise.receivable = item.amount;
                    }
                    Insert(transaction, conn, voucheritembillwise);
                }
            }
        }

        public void DeleteBillwiseItems(NpgsqlConnection conn, NpgsqlTransaction transaction, Voucher oVoucher)
        {
            var oldvoucheritembillwise = Search(new VoucheritembillwiseCreteria() { voucherid = oVoucher.id });
            if (oldvoucheritembillwise.Exists(x => x.adjusted != 0))
            {
                var oldadjusted = Search(new VoucheritembillwiseCreteria()
                {
                    sourceitemids = oldvoucheritembillwise.FindAll(x => x.adjusted != 0).ConvertAll(x => x.id)
                });

                LedgerDataService ledgerDataService = new LedgerDataService() { Context = this.Context };
                var ledgername = ledgerDataService.Search(new LedgerCreteria()
                {
                    ids = oldadjusted.ConvertAll(x => x.ledgerid)
                });

                string existingadjusted = "";
                oldadjusted.ForEach(x =>
                {
                    var ledger = ledgername.Find(y => y.id == x.ledgerid);
                    existingadjusted += (existingadjusted != "" ? "\r\n" : "") + ledger.displayname + " Adjusted " + x.adjusted + " on Voucher - " + x.referenceno + "(" + x.referencetype + ")";
                });

                if (existingadjusted != "")
                {
                    throw new Exception("Validation failed \r\n" + existingadjusted);
                }
            }

            NpgsqlCommand command = new NpgsqlCommand(@"
                Update Voucheritembillwise set adjusted=Voucheritembillwise.adjusted-current.paid from Voucheritembillwise current
                where current.voucherid = @voucherid and Voucheritembillwise.id = current.sourceitemid RETURNING Voucheritembillwise.adjusted", conn, transaction);
            command.Parameters.Add("voucherid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oVoucher.id;
            using (var reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    var newadjusted = reader.IsDBNull(0) ? 0 : reader.GetDecimal(0);
                    if (newadjusted < 0)
                        throw new Exception("Please remove existing billwise adjustment for this current voucher");
                }
            }

            command = new NpgsqlCommand(@"Delete from Voucheritembillwise where voucherid=@voucherid", conn, transaction);
            command.Parameters.Add("voucherid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oVoucher.id;
            var affected = command.ExecuteNonQuery();
        }
        private void UpdatePayment(NpgsqlConnection conn, NpgsqlTransaction transaction, long id, decimal adjusted)
        {
            NpgsqlCommand command = new NpgsqlCommand("Update Voucheritembillwise set adjusted=adjusted+@adjusted where id=@id RETURNING adjusted,payable,receivable,referenceno,referenceinvoiceno", conn, transaction);
            command.Parameters.Add("adjusted", NpgsqlTypes.NpgsqlDbType.Numeric).Value = adjusted;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = id;
            using (var reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    var newadjusted = reader.IsDBNull(0) ? 0 : reader.GetDecimal(0);
                    var payable = reader.IsDBNull(1) ? 0 : reader.GetDecimal(1);
                    var receivable = reader.IsDBNull(2) ? 0 : reader.GetDecimal(2);
                    var referenceno = reader.IsDBNull(3) ? "" : reader.GetString(3);
                    var referenceinvoiceno = reader.IsDBNull(4) ? "" : reader.GetString(4);

                    if (newadjusted > (payable + receivable))
                    {
                        throw new Exception("Already Voucher Number " + referenceno + (!string.IsNullOrEmpty(referenceinvoiceno) ? (", Invoice No -" + referenceinvoiceno) : "")
                            + " settled " + (newadjusted - adjusted));
                    }
                }
            }
        }

        internal bool SaveOpening(LedgerOpening opitem)
        {
            NpgsqlTransaction transaction = null;
            try
            {
                if (opitem.openinglist == null) opitem.openinglist = new List<Voucheritembillwise>();
                var openingledger = Search(new VoucheritembillwiseCreteria()
                {
                    showopening = true,
                    ledgerid = opitem.ledgerid
                });
                LedgerDataService ledgerDataService = new LedgerDataService() { Context = this.Context };
                var leders = ledgerDataService.Search(new LedgerCreteria()
                {
                    id = opitem.ledgerid
                });
                bool issupplier = leders[0].supplierid > 0;
                bool iscustomer = leders[0].customerid > 0;

                if(leders[0].supplierid != opitem.supplierid)
                {
                    throw new Exception("pleaes save the ledger before adding opening balances");
                }
                if (leders[0].customerid != opitem.customerid)
                {
                    throw new Exception("pleaes save the ledger before adding opening balances");
                }

                var delerecords = openingledger.FindAll(x => !opitem.openinglist.Exists(y => y.id == x.id));
                var newrecords = opitem.openinglist.FindAll(x => x.id <= 0);
                if (delerecords.Count > 0 || newrecords.Count > 0)
                {
                    using (NpgsqlConnection conn = CreateConnection())
                    {
                        conn.Open();
                        transaction = conn.BeginTransaction();
                        foreach (var item in newrecords)
                        {
                            if (!InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.finance_tbexclinexp)
                            {
                                if (issupplier)
                                {
                                    if (item.obcompanyid == 0) throw new Exception("Please select the company");
                                    LrinvoicereturnDataService lrinvoicereturnDataService = new LrinvoicereturnDataService() { Context = this.Context };
                                    if (item.receivable > 0 || item.payable > 0)
                                    {
                                        var lrreturn = lrinvoicereturnDataService.AddOpening(item, item.obcompanyid, leders[0].supplierid, conn, transaction);
                                        item.lrinvoicereturnid = lrreturn.id;
                                    }
                                }

                                if (iscustomer)
                                {
                                    if (item.obcompanyid == 0) throw new Exception("Please select the company");
                                    CustomerinvoiceDataService customerinvoiceDataService = new CustomerinvoiceDataService() { Context = this.Context };

                                    if (item.receivable > 0 || item.payable > 0)
                                    {
                                        var lrreturn = customerinvoiceDataService.AddOpening(item, item.obcompanyid, leders[0].customerid, conn, transaction);
                                        item.customerinvoiceid = lrreturn.id;
                                    }
                                }
                            }
                            Insert(transaction, conn, item);
                        }
                        foreach (var item in delerecords)
                        {
                            if (!InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.finance_tbexclinexp)
                            {
                                if (issupplier && item.lrinvoicereturnid > 0)
                                {
                                    LrinvoicereturnDataService lrinvoicereturnDataService = new LrinvoicereturnDataService() { Context = this.Context };
                                    lrinvoicereturnDataService.RemoveOpening(item, leders[0].supplierid, conn, transaction);
                                }

                                if (iscustomer && item.lrinvoicereturnid > 0)
                                {
                                    CustomerinvoiceDataService customerinvoiceDataService = new CustomerinvoiceDataService() { Context = this.Context };
                                    var lrreturn = customerinvoiceDataService.RemoveOpening(item, leders[0].customerid, conn, transaction);
                                }
                            }
                            Delete(transaction, conn, item);
                        }
                        transaction.Commit();
                        conn.Close();
                    }
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, opitem);
                return false;
            }
            return true;
        }
    }

}
