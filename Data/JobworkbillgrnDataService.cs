using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
namespace InfyPOS.Data
{
    public class JobworkbillgrnDataService : BaseData
    {
        const string SQL_Search = @"select isinterstatesale, invoiceno,entryno,invoicedate, paidamount, otheramount, grossamount, totalamount, id, taxamount, supplierid, materialcost, materialsalevalue, modifiedon, createdon, createdby, settled, locationid, modifiedby, version, suppliercompanyid, companyid, isactive, entrydate,lrinvoiceids,status, qty from jobworkbillgrn where id=@id";
        const string SQL_Insert = @"INSERT INTO public.jobworkbillgrn(isinterstatesale, invoiceno,entryno, invoicedate, paidamount, otheramount, grossamount, totalamount, taxamount, supplierid, materialcost, materialsalevalue, modifiedon, createdon, createdby, settled, locationid, modifiedby, version, suppliercompanyid, companyid, isactive, entrydate,lrinvoiceids,status, qty) VALUES(@isinterstatesale, @invoiceno,@entryno, @invoicedate, @paidamount, @otheramount, @grossamount, @totalamount, @taxamount, @supplierid, @materialcost, @materialsalevalue, @modifiedon, @createdon, @createdby, @settled, @locationid, @modifiedby, @version, @suppliercompanyid, @companyid, @isactive, @entrydate,@lrinvoiceids,@status, @qty) RETURNING id";
        const string SQL_Update = @"UPDATE public.jobworkbillgrn SET isinterstatesale=@isinterstatesale, invoiceno=@invoiceno,entryno=@entryno, invoicedate=@invoicedate, paidamount=@paidamount, otheramount=@otheramount, grossamount=@grossamount, totalamount=@totalamount, taxamount=@taxamount, supplierid=@supplierid, materialcost=@materialcost, materialsalevalue=@materialsalevalue, modifiedon=@modifiedon, settled=@settled, locationid=@locationid, modifiedby=@modifiedby, version=@version+1, suppliercompanyid=@suppliercompanyid, companyid=@companyid, entrydate=@entrydate,lrinvoiceids=@lrinvoiceids,status=@status, qty=@qty where id=@id and version=@version RETURNING version";
        const string SQL_Delete = @"update public.jobworkbillgrn set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";

        public List<Jobworkbillgrn> Search(JobworkbillgrnCreteria _jobworkbillgrn)
        {
            List<Jobworkbillgrn> lst = new List<Jobworkbillgrn>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_jobworkbillgrn.id != 0)
                {
                    command = new NpgsqlCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _jobworkbillgrn.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("isactive", true, "=");

                    if (_jobworkbillgrn.supplierid > 0)
                        qb.AddParameter("supplierid", _jobworkbillgrn.supplierid, "=");

                    if (_jobworkbillgrn.companyid > 0)
                        qb.AddParameter("companyid", _jobworkbillgrn.companyid, "=");

                    if (_jobworkbillgrn.locationid > 0)
                        qb.AddParameter("locationid", _jobworkbillgrn.locationid, "=");

                    if (!string.IsNullOrEmpty(_jobworkbillgrn.invoiceno))
                        qb.AddParameter("invoiceno", _jobworkbillgrn.invoiceno, "=");

                    if (_jobworkbillgrn.fromentrydate.Year > 2000 && _jobworkbillgrn.toentrydate.Year > 2000)
                    {
                        qb.AddParameter("createdon", "fromdate", _jobworkbillgrn.fromentrydate.Date, ">=");
                        qb.AddParameter("createdon", "todate", _jobworkbillgrn.toentrydate.Date.AddDays(1), "<");
                    }

                    if (_jobworkbillgrn.frominvoicedate.Year > 2000 && _jobworkbillgrn.toinvoicedate.Year > 2000)
                    {
                        qb.AddParameter("invoicedate", "frominvdate", _jobworkbillgrn.frominvoicedate.Date, ">=");
                        qb.AddParameter("invoicedate", "toinvdate", _jobworkbillgrn.toinvoicedate.Date.AddDays(1), "<");
                    }

                    qb.SortField = "jobworkbillgrn.id desc";
                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Jobworkbillgrn ojobworkbillgrn = new Jobworkbillgrn();
                        ojobworkbillgrn.isinterstatesale = reader["isinterstatesale"] == DBNull.Value ? false : Convert.ToBoolean(reader["isinterstatesale"]);
                        ojobworkbillgrn.invoiceno = reader["invoiceno"] == DBNull.Value ? "" : reader["invoiceno"].ToString();
                        ojobworkbillgrn.entryno = reader["entryno"] == DBNull.Value ? "" : reader["entryno"].ToString();
                        ojobworkbillgrn.lrinvoiceids = reader["lrinvoiceids"] == DBNull.Value ? "" : reader["lrinvoiceids"].ToString();
                        ojobworkbillgrn.invoicedate = reader["invoicedate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["invoicedate"]);
                        ojobworkbillgrn.paidamount = reader["paidamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["paidamount"]);
                        ojobworkbillgrn.otheramount = reader["otheramount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["otheramount"]);
                        ojobworkbillgrn.grossamount = reader["grossamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["grossamount"]);
                        ojobworkbillgrn.totalamount = reader["totalamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["totalamount"]);
                        ojobworkbillgrn.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        ojobworkbillgrn.taxamount = reader["taxamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["taxamount"]);
                        ojobworkbillgrn.supplierid = reader["supplierid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["supplierid"]);
                        ojobworkbillgrn.materialcost = reader["materialcost"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["materialcost"]);
                        ojobworkbillgrn.materialsalevalue = reader["materialsalevalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["materialsalevalue"]);
                        ojobworkbillgrn.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        ojobworkbillgrn.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        ojobworkbillgrn.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        ojobworkbillgrn.settled = reader["settled"] == DBNull.Value ? false : Convert.ToBoolean(reader["settled"]);
                        ojobworkbillgrn.locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                        ojobworkbillgrn.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        ojobworkbillgrn.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        ojobworkbillgrn.suppliercompanyid = reader["suppliercompanyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["suppliercompanyid"]);
                        ojobworkbillgrn.companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"]);
                        ojobworkbillgrn.status = reader["status"] == DBNull.Value ? 0 : Convert.ToInt32(reader["status"]);
                        ojobworkbillgrn.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        ojobworkbillgrn.entrydate = reader["entrydate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["entrydate"]);
                        ojobworkbillgrn.qty = reader["qty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["qty"]);
                        lst.Add(ojobworkbillgrn);
                    }
                }
                if (_jobworkbillgrn.id > 0 && lst.Count == 1)
                {
                    JobworkbillgrnitemsDataService subJobworkbillgrnitems = new JobworkbillgrnitemsDataService() { Context = this.Context };
                    lst[0].Jobworkbillitems = subJobworkbillgrnitems.SearchAll(_jobworkbillgrn.id);
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _jobworkbillgrn);
            }
            return lst;
        }

        public Jobworkbillgrn Insert(Jobworkbillgrn oJobworkbillgrn)
        {

            if (!InfyPOS.Services.ConfigManager.Instance(this.Context.OrganizationID).Config.system_ismfgapplication &&
                oJobworkbillgrn.Jobworkbillitems.Exists(e => e.invoiceitemid == 0))
            {
                throw new Exception("Barcode reference/invoice id missing.");
            }

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    var sequence = SequenceManager.Instance(Context.OrganizationID).Get("jobworkbillgrn.entryno", oJobworkbillgrn.locationid);
                    conn.Open();

                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);

                    if (oJobworkbillgrn.Jobworkbillitems != null && oJobworkbillgrn.Jobworkbillitems.Exists(e => e.lrinvoiceid > 0))
                        oJobworkbillgrn.lrinvoiceids = string.Join(",", oJobworkbillgrn.Jobworkbillitems.FindAll(e => e.lrinvoiceid > 0).ConvertAll(e => e.lrinvoiceid).Distinct());
                    else
                        oJobworkbillgrn.lrinvoiceids = "";
                    oJobworkbillgrn.createdon = DateTimeNow;
                    oJobworkbillgrn.modifiedon = DateTimeNow;
                    oJobworkbillgrn.createdby = Context.UserID;
                    oJobworkbillgrn.modifiedby = Context.UserID;
                    oJobworkbillgrn.isactive = true;
                    oJobworkbillgrn.version = 1;
                    oJobworkbillgrn.entryno = string.IsNullOrEmpty(oJobworkbillgrn.entryno) ? sequence.GetNextNumber(Context, conn, transaction) : oJobworkbillgrn.entryno;
                    Insert(transaction, conn, oJobworkbillgrn);
                    JobworkbillgrnitemsDataService subJobworkbillgrnitems = new JobworkbillgrnitemsDataService() { Context = this.Context };
                    foreach (var item in oJobworkbillgrn.Jobworkbillitems)
                    {
                        item.jobworkbillgrnid = oJobworkbillgrn.id;
                        item.createdon = DateTimeNow;
                        item.modifiedon = DateTimeNow;
                        item.createdby = Context.UserID;
                        item.modifiedby = Context.UserID;
                        item.isactive = true;
                        item.version = 1;

                        subJobworkbillgrnitems.Insert(transaction, conn, item);
                    }
                    SplitInvoiceItems(transaction, conn, oJobworkbillgrn);
                    if (oJobworkbillgrn.lrinvoiceids == null)
                        oJobworkbillgrn.lrinvoiceids = "";

                    var lrinvoiceids = "";
                    if (oJobworkbillgrn.Jobworkbillitems != null && oJobworkbillgrn.Jobworkbillitems.Exists(e => e.lrinvoiceid > 0))
                        lrinvoiceids = string.Join(",", oJobworkbillgrn.Jobworkbillitems.FindAll(e => e.lrinvoiceid > 0).ConvertAll(e => e.lrinvoiceid).Distinct());

                    if (lrinvoiceids != oJobworkbillgrn.lrinvoiceids)
                    {
                        oJobworkbillgrn.lrinvoiceids = lrinvoiceids;
                        var param = new SortedList<string, object>();
                        param.Add("lrinvoiceids", oJobworkbillgrn.lrinvoiceids);
                        param.Add("id", oJobworkbillgrn.id);
                        ExecuteQuery("update jobworkbillgrn set lrinvoiceids=@lrinvoiceids where id=@id", param, conn, transaction);
                    }
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oJobworkbillgrn);
            }
            return oJobworkbillgrn;
        }

        public Jobworkbillgrn Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Jobworkbillgrn oJobworkbillgrn)
        {
            if (oJobworkbillgrn.Jobworkbillitems != null && oJobworkbillgrn.Jobworkbillitems.Exists(e => e.lrinvoiceid > 0))
                oJobworkbillgrn.lrinvoiceids = string.Join(",", oJobworkbillgrn.Jobworkbillitems.FindAll(e => e.lrinvoiceid > 0).ConvertAll(e => e.lrinvoiceid).Distinct());
            else
                oJobworkbillgrn.lrinvoiceids = "";
            NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("isinterstatesale", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oJobworkbillgrn.isinterstatesale;
            command.Parameters.Add("invoiceno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oJobworkbillgrn.invoiceno == null ? "" : oJobworkbillgrn.invoiceno;
            command.Parameters.Add("entryno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oJobworkbillgrn.entryno == null ? "" : oJobworkbillgrn.entryno;
            command.Parameters.Add("lrinvoiceids", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oJobworkbillgrn.lrinvoiceids == null ? "" : oJobworkbillgrn.lrinvoiceids;
            command.Parameters.Add("invoicedate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oJobworkbillgrn.invoicedate;
            command.Parameters.Add("paidamount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oJobworkbillgrn.paidamount;
            command.Parameters.Add("otheramount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oJobworkbillgrn.otheramount;
            command.Parameters.Add("grossamount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oJobworkbillgrn.grossamount;
            command.Parameters.Add("totalamount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oJobworkbillgrn.totalamount;
            command.Parameters.Add("taxamount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oJobworkbillgrn.taxamount;
            command.Parameters.Add("supplierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oJobworkbillgrn.supplierid;
            command.Parameters.Add("materialcost", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oJobworkbillgrn.materialcost;
            command.Parameters.Add("materialsalevalue", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oJobworkbillgrn.materialsalevalue;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oJobworkbillgrn.modifiedon;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oJobworkbillgrn.createdon;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oJobworkbillgrn.createdby;
            command.Parameters.Add("settled", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oJobworkbillgrn.settled;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oJobworkbillgrn.locationid;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oJobworkbillgrn.modifiedby;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oJobworkbillgrn.version;
            command.Parameters.Add("status", NpgsqlTypes.NpgsqlDbType.Integer).Value = oJobworkbillgrn.status;
            command.Parameters.Add("suppliercompanyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oJobworkbillgrn.suppliercompanyid;
            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oJobworkbillgrn.companyid;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oJobworkbillgrn.isactive;
            command.Parameters.Add("entrydate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oJobworkbillgrn.entrydate;
            command.Parameters.Add("qty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oJobworkbillgrn.qty;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oJobworkbillgrn.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            return oJobworkbillgrn;
        }

        public Jobworkbillgrn Update(Jobworkbillgrn oJobworkbillgrn)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                var current = Search(new JobworkbillgrnCreteria()
                {
                    id = oJobworkbillgrn.id
                });
                if (current.Count != 1)
                    throw new Exception("Unable to find the Job Order.");

                if (current[0].status == 2)
                    throw new Exception("Updation can't be done on Completed Job Work.");

                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    Update(transaction, conn, oJobworkbillgrn);
                    DeleteInvoiceItems(transaction, conn, oJobworkbillgrn, false);

                    JobworkbillgrnitemsDataService subJobworkbillgrnitems = new JobworkbillgrnitemsDataService() { Context = this.Context };
                    subJobworkbillgrnitems.Update(transaction, conn, oJobworkbillgrn.Jobworkbillitems, oJobworkbillgrn.id);
                    SplitInvoiceItems(transaction, conn, oJobworkbillgrn);
                    if (oJobworkbillgrn.lrinvoiceids == null)
                        oJobworkbillgrn.lrinvoiceids = "";

                    var lrinvoiceids = "";
                    if (oJobworkbillgrn.Jobworkbillitems != null && oJobworkbillgrn.Jobworkbillitems.Exists(e => e.lrinvoiceid > 0))
                        lrinvoiceids = string.Join(",", oJobworkbillgrn.Jobworkbillitems.FindAll(e => e.lrinvoiceid > 0).ConvertAll(e => e.lrinvoiceid).Distinct());

                    if (lrinvoiceids != oJobworkbillgrn.lrinvoiceids)
                    {
                        oJobworkbillgrn.lrinvoiceids = lrinvoiceids;
                        var param = new SortedList<string, object>();
                        param.Add("lrinvoiceids", (oJobworkbillgrn.lrinvoiceids.Length > 2000) ? oJobworkbillgrn.lrinvoiceids.Substring(0, 2000) : oJobworkbillgrn.lrinvoiceids);
                        param.Add("id", oJobworkbillgrn.id);
                        ExecuteQuery("update jobworkbillgrn set lrinvoiceids=@lrinvoiceids where id=@id", param, conn, transaction);
                    }
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oJobworkbillgrn);
            }
            return oJobworkbillgrn;
        }

        public Jobworkbillgrn Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Jobworkbillgrn oJobworkbillgrn)
        {
            if (oJobworkbillgrn.Jobworkbillitems != null && oJobworkbillgrn.Jobworkbillitems.Exists(e => e.lrinvoiceid > 0))
                oJobworkbillgrn.lrinvoiceids = string.Join(",", oJobworkbillgrn.Jobworkbillitems.FindAll(e => e.lrinvoiceid > 0).ConvertAll(e => e.lrinvoiceid).Distinct());
            else
                oJobworkbillgrn.lrinvoiceids = "";
            NpgsqlCommand command = new NpgsqlCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("isinterstatesale", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oJobworkbillgrn.isinterstatesale;
            command.Parameters.Add("invoiceno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oJobworkbillgrn.invoiceno == null ? "" : oJobworkbillgrn.invoiceno;
            command.Parameters.Add("entryno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oJobworkbillgrn.entryno == null ? "" : oJobworkbillgrn.entryno;
            command.Parameters.Add("lrinvoiceids", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oJobworkbillgrn.lrinvoiceids == null ? "" : oJobworkbillgrn.lrinvoiceids;
            command.Parameters.Add("invoicedate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oJobworkbillgrn.invoicedate;
            command.Parameters.Add("paidamount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oJobworkbillgrn.paidamount;
            command.Parameters.Add("otheramount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oJobworkbillgrn.otheramount;
            command.Parameters.Add("grossamount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oJobworkbillgrn.grossamount;
            command.Parameters.Add("totalamount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oJobworkbillgrn.totalamount;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oJobworkbillgrn.id;
            command.Parameters.Add("taxamount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oJobworkbillgrn.taxamount;
            command.Parameters.Add("supplierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oJobworkbillgrn.supplierid;
            command.Parameters.Add("materialcost", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oJobworkbillgrn.materialcost;
            command.Parameters.Add("materialsalevalue", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oJobworkbillgrn.materialsalevalue;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oJobworkbillgrn.modifiedon;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oJobworkbillgrn.createdon;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oJobworkbillgrn.createdby;
            command.Parameters.Add("settled", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oJobworkbillgrn.settled;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oJobworkbillgrn.locationid;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oJobworkbillgrn.modifiedby;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oJobworkbillgrn.version;
            command.Parameters.Add("suppliercompanyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oJobworkbillgrn.suppliercompanyid;
            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oJobworkbillgrn.companyid;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oJobworkbillgrn.isactive;
            command.Parameters.Add("entrydate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oJobworkbillgrn.entrydate;
            command.Parameters.Add("qty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oJobworkbillgrn.qty;
            command.Parameters.Add("status", NpgsqlTypes.NpgsqlDbType.Integer).Value = oJobworkbillgrn.status;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oJobworkbillgrn.version = Convert.ToInt32(reader[0].ToString());
                }
            }

            return oJobworkbillgrn;
        }


        public bool Delete(Jobworkbillgrn oJobworkbillgrn)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oJobworkbillgrn);
                    //DeleteInvoiceItems(transaction, conn, oJobworkbillgrn, true);
                    JobworkbillgrnitemsDataService subJobworkbillgrnitems = new JobworkbillgrnitemsDataService() { Context = this.Context };
                    subJobworkbillgrnitems.DeleteAll(oJobworkbillgrn.id, oJobworkbillgrn.modifiedby, conn, transaction);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oJobworkbillgrn);
            }
            return hasSuccessCount;
        }


        public bool DeleteInvoiceItems(NpgsqlTransaction transaction, NpgsqlConnection conn,
            Jobworkbillgrn oJobworkbillgrn, bool deleteall)
        {
            JobworkbillgrnitemsDataService jobworkbillgrnitemsDataService = new JobworkbillgrnitemsDataService() { Context = this.Context };
            jobworkbillgrnitemsDataService.Context = this.Context;

            JobworkbillitemsDataService jobworkbillitemsDataService = new JobworkbillitemsDataService() { Context = this.Context };
            jobworkbillitemsDataService.Context = this.Context;

            var existingitems = jobworkbillgrnitemsDataService.Search(new JobworkbillgrnitemsCreteria()
            {
                jobworkbillgrnid = oJobworkbillgrn.id
            });
            var deleteditems = deleteall ? existingitems : existingitems.FindAll(e => !oJobworkbillgrn.Jobworkbillitems.Exists(x => x.id == e.id));

            LrinvoiceitemsDataService lrinvoiceitems = new LrinvoiceitemsDataService() { Context = this.Context };
            lrinvoiceitems.Context = this.Context;
            ItemsDataService itemservice = new ItemsDataService() { Context = this.Context };

            foreach (var item in deleteditems)
            {
                if (item.newinvoiceitemid == 0) continue;
                var existigInvoiceitem = lrinvoiceitems.Search(conn, transaction, new LrinvoiceitemsCreteria()
                {
                    id = item.invoiceitemid
                }).First();

                var newInvoiceItem = lrinvoiceitems.Search(conn, transaction, new LrinvoiceitemsCreteria()
                {
                    id = item.newinvoiceitemid
                }).First();

                var orderitem = jobworkbillitemsDataService.Search(new JobworkbillitemsCreteria()
                {
                    id = item.jobworkbillitemsid
                }).First();


                var qty = (orderitem.oqty / orderitem.iqty) * item.qty;
                existigInvoiceitem.amount += newInvoiceItem.amount;
                existigInvoiceitem.buyingqty += qty;
                existigInvoiceitem.purchasetotaldiscount += newInvoiceItem.purchasetotaldiscount;
                existigInvoiceitem.purchasetax += newInvoiceItem.purchasetax;
                existigInvoiceitem.modifiedby = Context.UserID;
                existigInvoiceitem.modifiedon = DateTimeNow;

                if (newInvoiceItem.sellableqty > 0)
                {
                    StockDataService stockDataService = new StockDataService() { Context = this.Context };
                    stockDataService.Context = this.Context;
                    var stocklist = stockDataService.SearchForIds(newInvoiceItem.id.ToString(), "");
                    if (stocklist.Exists(e => e.transfered > 0))
                    {
                        SalablegoodsDataService salablegoodsDataService = new SalablegoodsDataService() { Context = this.Context };
                        salablegoodsDataService.Context = this.Context;
                        var salablegoods = salablegoodsDataService.Search(new SalablegoodsCreteria()
                        {
                            stockids = stocklist.ConvertAll(e => e.id)
                        });
                        if (salablegoods.Exists(e => e.oqty > 0))
                        {
                            throw new Exception(salablegoods.Find(e => e.oqty > 0).barcode + " already sold, can't delete");
                        }
                        foreach (var sitem in salablegoods)
                        {
                            salablegoodsDataService.Delete(transaction, conn, sitem);
                        }
                    }
                    foreach (var stitem in stocklist)
                    {
                        stockDataService.Delete(transaction, conn, stitem);
                    }
                }
                lrinvoiceitems.Delete(transaction, conn, newInvoiceItem);
                lrinvoiceitems.Update(transaction, conn, existigInvoiceitem);
            }

            List<Jobworkbillgrnitems> modifieditems = deleteall ? new List<Jobworkbillgrnitems>() :
                existingitems.FindAll(e => oJobworkbillgrn.Jobworkbillitems.Exists(x => x.id == e.id && x.qty != e.qty));

            foreach (var item in modifieditems)
            {
                if (item.newinvoiceitemid == 0) continue;
                var existigInvoiceitem = lrinvoiceitems.Search(conn, transaction, new LrinvoiceitemsCreteria()
                {
                    id = item.invoiceitemid
                }).First();

                var newInvoiceItem = lrinvoiceitems.Search(conn, transaction, new LrinvoiceitemsCreteria()
                {
                    id = item.newinvoiceitemid
                }).First();

                var orderitem = jobworkbillitemsDataService.Search(new JobworkbillitemsCreteria()
                {
                    id = item.jobworkbillitemsid
                }).First();


                if (newInvoiceItem.sellableqty == 0)
                {

                    var qty = (orderitem.iqty / orderitem.oqty) * item.qty;
                    existigInvoiceitem.amount += newInvoiceItem.amount;
                    existigInvoiceitem.buyingqty += qty;
                    existigInvoiceitem.purchasetotaldiscount += newInvoiceItem.purchasetotaldiscount;
                    existigInvoiceitem.purchasetax += newInvoiceItem.purchasetax;
                    existigInvoiceitem.modifiedby = Context.UserID;
                    existigInvoiceitem.modifiedon = DateTimeNow;

                    var newitemid = newInvoiceItem.id;
                    var newitemversion = newInvoiceItem.version;
                    newInvoiceItem = Newtonsoft.Json.JsonConvert.DeserializeObject<LrinvoiceItemDetail>(Newtonsoft.Json.JsonConvert.SerializeObject(existigInvoiceitem));

                    newInvoiceItem.id = newitemid;
                    newInvoiceItem.createdby = Context.UserID;
                    newInvoiceItem.modifiedby = Context.UserID;
                    newInvoiceItem.version = newitemversion;
                    newInvoiceItem.createdon = DateTimeNow;
                    newInvoiceItem.modifiedon = DateTimeNow;
                    newInvoiceItem.sellableqty = 0;
                    newInvoiceItem.designid = item.designid;
                    newInvoiceItem.barcodes = "";
                    newInvoiceItem.POItemInfo = "null";
                    newInvoiceItem.sno = 0;
                    newInvoiceItem.sellingprice = item.finalprice;
                    newInvoiceItem.dealerprice = item.dealerprice;
                    if (newInvoiceItem.Details == null)
                        newInvoiceItem.Details = new Items();

                    newInvoiceItem.Details.styleid = item.styleid;
                    newInvoiceItem.Details.patternid = item.patternid;
                    newInvoiceItem.Details.sizeid = item.sizeid;
                    newInvoiceItem.Details.sleeveid = item.sleeveid;
                    newInvoiceItem.Details.typeid = item.typeid;
                    newInvoiceItem.Details.brandid = item.brandid;
                    newInvoiceItem.Details.productid = item.productid;
                    newInvoiceItem.productid = item.productid;

                    decimal newamount = newInvoiceItem.amount <= 0 ? 0 : newInvoiceItem.amount / newInvoiceItem.buyingqty * orderitem.iqty;
                    decimal newtax = newInvoiceItem.purchasetax <= 0 ? 0 : newInvoiceItem.purchasetax / newInvoiceItem.buyingqty * orderitem.iqty;
                    decimal newpurchasetotaldiscount = newInvoiceItem.purchasetotaldiscount <= 0 ? 0 : newInvoiceItem.purchasetotaldiscount / newInvoiceItem.buyingqty * orderitem.iqty;

                    newInvoiceItem.buyingqty = item.qty;

                    newInvoiceItem.amount = newamount * (item.qty / orderitem.oqty);
                    newInvoiceItem.purchasetotaldiscount = newpurchasetotaldiscount * (item.qty / orderitem.oqty);
                    newInvoiceItem.purchasetax = newtax * (item.qty / orderitem.oqty);
                    newInvoiceItem.purchasediscount = newInvoiceItem.purchasetotaldiscount / newInvoiceItem.buyingqty;
                    newInvoiceItem.buyingprice = newInvoiceItem.amount / newInvoiceItem.buyingqty;
                    newInvoiceItem.basebuyingprice = newInvoiceItem.buyingprice + newInvoiceItem.purchasediscount;

                    existigInvoiceitem.amount -= newInvoiceItem.amount;
                    existigInvoiceitem.buyingqty -= (newInvoiceItem.buyingqty * (orderitem.iqty / orderitem.oqty));
                    existigInvoiceitem.purchasetotaldiscount -= newInvoiceItem.purchasetotaldiscount;
                    existigInvoiceitem.purchasetax -= newInvoiceItem.purchasetax;
                    existigInvoiceitem.modifiedby = Context.UserID;
                    existigInvoiceitem.modifiedon = DateTimeNow;


                    var itemdetails = itemservice.Search(new ItemsCreteria()
                    {
                        brandid = newInvoiceItem.Details.brandid,
                        colourid = newInvoiceItem.Details.colourid,
                        materialid = newInvoiceItem.Details.materialid,
                        patternid = newInvoiceItem.Details.patternid,
                        typeid = newInvoiceItem.Details.typeid,
                        sizeid = newInvoiceItem.Details.sizeid,
                        styleid = newInvoiceItem.Details.styleid,
                        fitid = newInvoiceItem.Details.fitid,
                        sleeveid = newInvoiceItem.Details.sleeveid,
                        productid = newInvoiceItem.Details.productid,
                        productmap = true
                    }, new List<long>(), conn, transaction);
                    if (itemdetails.Count == 1)
                    {
                        newInvoiceItem.itemid = itemdetails[0].id;
                    }
                    else
                    {
                        var newitem = newInvoiceItem.Details;
                        newitem.isactive = true;
                        newitem.salemode = 1;
                        newitem.version = 1;
                        newitem = itemservice.Insert(newitem);
                        newInvoiceItem.itemid = newitem.id;
                    }
                }
                lrinvoiceitems.Update(transaction, conn, newInvoiceItem);
                lrinvoiceitems.Update(transaction, conn, existigInvoiceitem);
            }
            return true;
        }
        public bool SplitInvoiceItems(NpgsqlTransaction transaction, NpgsqlConnection conn, Jobworkbillgrn oJobworkbillgrn)
        {
            if (oJobworkbillgrn.status != 2) return false;
            bool HasMultipleBarcodedSystem = true;
            if (oJobworkbillgrn.Jobworkbillitems.All(e => e.qty == 1))
            {
                string ids = string.Join(",", oJobworkbillgrn.Jobworkbillitems.ConvertAll(e => e.invoiceitemid));
                using (var reader = ExecuteReader("select count(*) from lrinvoiceitems where id in (" + ids + ") and buyingqty>1", null, conn, transaction))
                {
                    if (reader.Read())
                    {
                        HasMultipleBarcodedSystem = !reader.IsDBNull(0) && reader.GetInt32(0) > 0;
                    }
                }
            }

            ReferencelistDataService referencelistDataService = new ReferencelistDataService() { Context = this.Context };
            var receivinglocation = referencelistDataService.Search(new ReferencelistCreteria() { id = oJobworkbillgrn.locationid });
            bool hasrecivingonretail = receivinglocation.Count > 0 && receivinglocation[0].additionalinfo != null
                && !receivinglocation[0].additionalinfo.iswarehouse;
            ProductsDataService productsDataService = new ProductsDataService() { Context = this.Context };
            var sellingmodes = productsDataService.GetSellingModes(oJobworkbillgrn.Jobworkbillitems.ConvertAll(e => e.productid));

            LrinvoiceitemsDataService lrinvoiceitems = new LrinvoiceitemsDataService() { Context = this.Context };
            lrinvoiceitems.Context = this.Context;
            JobworkbillitemsDataService jobworkbillitemsDataService = new JobworkbillitemsDataService() { Context = this.Context };
            jobworkbillitemsDataService.Context = this.Context;
            ItemsDataService itemservice = new ItemsDataService() { Context = this.Context };
            var directstock = oJobworkbillgrn.Jobworkbillitems.FindAll(x => x.invoiceitemid <= 0);

            //if (!Services.ConfigManager.Instance(Context.OrganizationID).Config.system_jobworksamebarcode)
            //{
            //    foreach (var item in oJobworkbillgrn.Jobworkbillitems.FindAll(x=>x.invoiceitemid>0))
            //    {   
            //        var newInvoiceItems = lrinvoiceitems.Search(conn, transaction, new LrinvoiceitemsCreteria()
            //        {
            //            id = item.invoiceitemid
            //        });

            //        if (newInvoiceItems.Count == 0)
            //        {
            //            directstock.Add(item);
            //            continue;
            //        }

            //        var newInvoiceItem = newInvoiceItems.First();
            //        item.lrinvoiceid = newInvoiceItem.lrinvoiceid;
            //        item.lrinvoiceid = newInvoiceItem.lrinvoiceid;
            //        var buyingitemid = newInvoiceItem.itemid;
            //        if (newInvoiceItem.Details == null && newInvoiceItem.itemid > 0)
            //        {
            //            newInvoiceItem.Details = itemservice.Search(new ItemsCreteria()
            //            {
            //                id = newInvoiceItem.itemid
            //            }, new List<long>(), conn, transaction).First();
            //        }

            //        newInvoiceItem.Details.styleid = item.styleid;
            //        newInvoiceItem.Details.patternid = item.patternid;
            //        newInvoiceItem.Details.sizeid = item.sizeid;
            //        newInvoiceItem.Details.typeid = item.typeid;
            //        newInvoiceItem.Details.brandid = item.brandid;
            //        newInvoiceItem.Details.productid = item.productid;
            //        newInvoiceItem.Details.materialid = item.materialid;
            //        newInvoiceItem.Details.colourid = item.colourid;
            //        newInvoiceItem.productid = item.productid;

            //        var itemdetails = itemservice.Search(new ItemsCreteria()
            //        {
            //            brandid = newInvoiceItem.Details.brandid,
            //            colourid = newInvoiceItem.Details.colourid,
            //            materialid = newInvoiceItem.Details.materialid,
            //            patternid = newInvoiceItem.Details.patternid,
            //            typeid = newInvoiceItem.Details.typeid,
            //            sizeid = newInvoiceItem.Details.sizeid,
            //            styleid = newInvoiceItem.Details.styleid,
            //            productid = newInvoiceItem.Details.productid,
            //            fitid = newInvoiceItem.Details.fitid,
            //            sleeveid = newInvoiceItem.Details.sleeveid,
            //            productmap = true
            //        }, new List<long>(), conn, transaction);

            //        if (itemdetails.Count == 1)
            //        {
            //            newInvoiceItem.itemid = itemdetails[0].id;
            //        }
            //        else
            //        {
            //            var newitem = newInvoiceItem.Details;
            //            newitem.isactive = true;
            //            newitem.salemode = 1;
            //            newitem.version = 1;
            //            newitem = itemservice.Insert(newitem);
            //            newInvoiceItem.itemid = newitem.id;
            //        }

            //        var param = new SortedList<string, object>();
            //        param.Add("id", newInvoiceItem.id);
            //        param.Add("jobworkcharge", item.chargerate);
            //        param.Add("jobworktax", item.chargetax);
            //        param.Add("jobworkerid", oJobworkbillgrn.supplierid);
            //        param.Add("itemid", newInvoiceItem.itemid);
            //        param.Add("productid", newInvoiceItem.Details.productid);
            //        param.Add("stockid", item.stockid);
            //        param.Add("price", item.finalprice);
            //        param.Add("buyingitemid", buyingitemid);
            //        param.Add("imageid", string.IsNullOrEmpty(item.imageid) ? newInvoiceItem.imgid : item.imageid);
            //        param.Add("jobworkgrnitemid", item.id);
            //        param.Add("locationid", oJobworkbillgrn.locationid);
            //        newInvoiceItem.barcodeList = new List<string>(new string[] { item.barcode });
            //        lrinvoiceitems.UpdateBarcode(transaction, conn, newInvoiceItem);
            //        param.Add("designid", string.IsNullOrEmpty(item.designid) ? "" : item.designid);
            //        param.Add("buyingdesignid", string.IsNullOrEmpty(newInvoiceItem.designid) ? "" : newInvoiceItem.designid);
            //        ExecuteQuery("update lrinvoiceitems set jobworkcharge=@jobworkcharge,jobworktax=@jobworktax,designid=@designid,productid=@productid,buyingdesignid=@buyingdesignid,buyingitemid=@buyingitemid,itemid=@itemid,jobworkerid=@jobworkerid,jobworkgrnitemid=@jobworkgrnitemid,sellableqty=1,version=100 where id=@id", param, conn, transaction);
            //        ExecuteQuery("update stock set itemid=@itemid,imageid=@imageid,productid=@productid,designid=@designid,buyingprice=buyingprice+@jobworkcharge,ibuyingprice=ibuyingprice+@jobworkcharge+@jobworktax,price=@price,transfered=1,version=100 where id=@stockid", param, conn, transaction);
            //        ExecuteQuery("update salablegoods set costprice=costprice+@jobworkcharge,icostprice=icostprice+@jobworkcharge+@jobworktax,price=@price,iqty=1,version=100 where stockid=@stockid and locationid=@locationid", param, conn, transaction);
            //        ExecuteQuery("update jobworkbillgrnitems set newinvoiceitemid=-1 where id=@jobworkgrnitemid", param, conn, transaction);
            //        item.newinvoiceitemid = -1;
            //        item.barcodes = new List<string>(new string[] { item.barcode });
            //    }
            //}
            //else 

            if (Services.ConfigManager.Instance(Context.OrganizationID).Config.system_jobworksamebarcode)
            {
                foreach (var item in oJobworkbillgrn.Jobworkbillitems)
                {
                    var newInvoiceItems = lrinvoiceitems.Search(conn, transaction, new LrinvoiceitemsCreteria()
                    {
                        id = item.invoiceitemid
                    });
                    if (newInvoiceItems.Count == 0)
                    {
                        directstock.Add(item);
                        continue;
                    }
                    var newInvoiceItem = newInvoiceItems.First();
                    item.lrinvoiceid = newInvoiceItem.lrinvoiceid;
                    var buyingitemid = newInvoiceItem.itemid;
                    if (newInvoiceItem.Details == null && newInvoiceItem.itemid > 0)
                    {
                        newInvoiceItem.Details = itemservice.Search(new ItemsCreteria()
                        {
                            id = newInvoiceItem.itemid
                        }, new List<long>(), conn, transaction).First();
                    }

                    newInvoiceItem.Details.styleid = item.styleid;
                    newInvoiceItem.Details.patternid = item.patternid;
                    newInvoiceItem.Details.sizeid = item.sizeid;
                    newInvoiceItem.Details.sleeveid = item.sleeveid;
                    newInvoiceItem.Details.typeid = item.typeid;
                    newInvoiceItem.Details.brandid = item.brandid;
                    newInvoiceItem.Details.productid = item.productid;
                    newInvoiceItem.Details.materialid = item.materialid;
                    newInvoiceItem.Details.colourid = item.colourid;
                    newInvoiceItem.productid = item.productid;

                    var itemdetails = itemservice.Search(new ItemsCreteria()
                    {
                        brandid = newInvoiceItem.Details.brandid,
                        colourid = newInvoiceItem.Details.colourid,
                        materialid = newInvoiceItem.Details.materialid,
                        patternid = newInvoiceItem.Details.patternid,
                        typeid = newInvoiceItem.Details.typeid,
                        sizeid = newInvoiceItem.Details.sizeid,
                        styleid = newInvoiceItem.Details.styleid,
                        productid = newInvoiceItem.Details.productid,
                        fitid = newInvoiceItem.Details.fitid,
                        sleeveid = newInvoiceItem.Details.sleeveid,
                        productmap = true
                    }, new List<long>(), conn, transaction);

                    if (itemdetails.Count == 1)
                    {
                        newInvoiceItem.itemid = itemdetails[0].id;
                    }
                    else
                    {
                        var newitem = newInvoiceItem.Details;
                        newitem.isactive = true;
                        newitem.salemode = 1;
                        newitem.version = 1;
                        newitem = itemservice.Insert(newitem);
                        newInvoiceItem.itemid = newitem.id;
                    }

                    var param = new SortedList<string, object>();
                    param.Add("id", newInvoiceItem.id);
                    param.Add("jobworkcharge", item.chargerate);
                    param.Add("jobworktax", item.chargetax);
                    param.Add("jobworkerid", oJobworkbillgrn.supplierid);
                    param.Add("itemid", newInvoiceItem.itemid);
                    param.Add("productid", newInvoiceItem.Details.productid);
                    param.Add("stockid", item.stockid);
                    param.Add("price", item.finalprice);
                    param.Add("dealerprice", item.dealerprice);
                    param.Add("buyingitemid", buyingitemid);
                    param.Add("imageid", string.IsNullOrEmpty(item.imageid) ? newInvoiceItem.imgid : item.imageid);
                    param.Add("jobworkgrnitemid", item.id);
                    param.Add("locationid", oJobworkbillgrn.locationid);
                    param.Add("iqty", item.qty);
                    param.Add("salablegoodsid", item.salablegoodsid);
                    newInvoiceItem.barcodeList = new List<string>(new string[] { item.barcode });
                    //lrinvoiceitems.UpdateBarcode(transaction, conn, newInvoiceItem);
                    param.Add("designid", string.IsNullOrEmpty(item.designid) ? "" : item.designid);
                    param.Add("buyingdesignid", string.IsNullOrEmpty(newInvoiceItem.designid) ? "" : newInvoiceItem.designid);
                    ExecuteQuery("update lrinvoiceitems set jobworkcharge=@jobworkcharge,jobworktax=@jobworktax,designid=@designid,productid=@productid,buyingdesignid=@buyingdesignid,buyingitemid=@buyingitemid,itemid=@itemid,jobworkerid=@jobworkerid,jobworkgrnitemid=@jobworkgrnitemid,version=100 where id=@id", param, conn, transaction);
                    ExecuteQuery("update stock set itemid=@itemid,imageid=@imageid,productid=@productid,designid=@designid,buyingprice=buyingprice+@jobworkcharge,ibuyingprice=ibuyingprice+@jobworkcharge+@jobworktax,price=@price,dealerprice=@dealerprice,version=100 where id=@stockid", param, conn, transaction);
                    ExecuteQuery("update salablegoods set costprice=costprice+@jobworkcharge,icostprice=icostprice+@jobworkcharge+@jobworktax,price=@price,version=100 where stockid=@stockid and locationid=@locationid", param, conn, transaction);
                    ExecuteQuery("update jobworkbillgrnitems set newinvoiceitemid=-1 where id=@jobworkgrnitemid", param, conn, transaction);
                    if (item.stockid > 0 && item.salablegoodsid > 0 && oJobworkbillgrn.locationid > 0 && item.qty > 0)
                    {
                        ExecuteQuery("update stock set transfered=transfered+@iqty where id=@stockid", param, conn, transaction);
                        ExecuteQuery("update salablegoods set iqty=iqty+@iqty where id=@salablegoodsid and locationid=@locationid", param, conn, transaction);
                    }
                    item.newinvoiceitemid = -1;
                    newInvoiceItem.version = 100;
                    item.barcodes = new List<string>(new string[] { item.barcode });
                }
            }
            else
            {
                List<string> barcodelist = new List<string>();
                var existinginvoiceprderitems = oJobworkbillgrn.Jobworkbillitems.FindAll(x => x.invoiceitemid > 0);
                if (existinginvoiceprderitems.Count > 0)
                {
                    var existigInvoiceitemsList = lrinvoiceitems.Search(conn, transaction, new LrinvoiceitemsCreteria()
                    {
                        ids = existinginvoiceprderitems.ConvertAll(x=>x.invoiceitemid)
                    });

                    foreach (var item in existinginvoiceprderitems)
                    {
                        var existigInvoiceitems = existigInvoiceitemsList.FindAll(x => x.id == item.invoiceitemid);
                        if (existigInvoiceitems.Count == 0)
                        {
                            directstock.Add(item);
                            continue;
                        }
                        var existigInvoiceitem = existigInvoiceitems.First();
                        item.lrinvoiceid = existigInvoiceitem.lrinvoiceid;
                        item.invoiceitemid = existigInvoiceitem.id;
                        var orderitem = jobworkbillitemsDataService.Search(new JobworkbillitemsCreteria()
                        {
                            id = item.jobworkbillitemsid
                        }).First();

                        var newInvoiceItem = Newtonsoft.Json.JsonConvert.DeserializeObject<LrinvoiceItemDetail>(Newtonsoft.Json.JsonConvert.SerializeObject(existigInvoiceitem));

                        newInvoiceItem.id = 0;
                        newInvoiceItem.createdby = Context.UserID;
                        newInvoiceItem.modifiedby = Context.UserID;
                        newInvoiceItem.version = 1;
                        newInvoiceItem.createdon = DateTimeNow;
                        newInvoiceItem.modifiedon = DateTimeNow;
                        newInvoiceItem.sellableqty = 0;
                        newInvoiceItem.designid = item.designid;
                        newInvoiceItem.barcodes = "";
                        newInvoiceItem.POItemInfo = "null";
                        newInvoiceItem.sno = 0;
                        newInvoiceItem.sellingprice = item.finalprice;
                        newInvoiceItem.dealerprice = item.dealerprice;
                        if (newInvoiceItem.Details == null)
                            newInvoiceItem.Details = new Items();

                        newInvoiceItem.Details.styleid = item.styleid;
                        newInvoiceItem.Details.patternid = item.patternid;
                        newInvoiceItem.Details.sizeid = item.sizeid;
                        newInvoiceItem.Details.sleeveid = item.sleeveid;
                        newInvoiceItem.Details.typeid = item.typeid;
                        newInvoiceItem.Details.brandid = item.brandid;
                        newInvoiceItem.Details.productid = item.productid;
                        newInvoiceItem.Details.materialid = item.materialid;
                        newInvoiceItem.Details.colourid = item.colourid;
                        newInvoiceItem.productid = item.productid;

                        if (newInvoiceItem.buyingqty <= (decimal) 0.01)
                            newInvoiceItem.buyingqty = 1;
                        if (newInvoiceItem.amount <= (decimal)0.01)
                            newInvoiceItem.amount = 100;

                        decimal newamount = newInvoiceItem.amount <= 0 ? 0 : newInvoiceItem.amount / newInvoiceItem.buyingqty * orderitem.iqty;
                        decimal newtax = newInvoiceItem.purchasetax <= 0 ? 0 : newInvoiceItem.purchasetax / newInvoiceItem.buyingqty * orderitem.iqty;
                        decimal newpurchasetotaldiscount = newInvoiceItem.purchasetotaldiscount <= 0 ? 0 : newInvoiceItem.purchasetotaldiscount / newInvoiceItem.buyingqty * orderitem.iqty;

                        if (newamount < 0)
                            newamount = 100;
                        if (newtax < 0)
                            newamount = 5;

                        newInvoiceItem.buyingqty = item.qty;
                        newInvoiceItem.dealerprice = item.dealerprice;
                        newInvoiceItem.sellingprice = item.finalprice;
                        newInvoiceItem.amount = newamount * (item.qty / orderitem.oqty);
                        newInvoiceItem.purchasetotaldiscount = newpurchasetotaldiscount * (item.qty / orderitem.oqty);
                        newInvoiceItem.purchasetax = newtax * (item.qty / orderitem.oqty);
                        newInvoiceItem.purchasediscount = newInvoiceItem.purchasetotaldiscount / newInvoiceItem.buyingqty;
                        newInvoiceItem.buyingprice = newInvoiceItem.amount / newInvoiceItem.buyingqty;
                        newInvoiceItem.basebuyingprice = newInvoiceItem.buyingprice + newInvoiceItem.purchasediscount;

                        existigInvoiceitem.amount -= newInvoiceItem.amount;
                        existigInvoiceitem.buyingqty -= (newInvoiceItem.buyingqty * (orderitem.iqty / orderitem.oqty));
                        existigInvoiceitem.purchasetotaldiscount -= newInvoiceItem.purchasetotaldiscount;
                        existigInvoiceitem.purchasetax -= newInvoiceItem.purchasetax;
                        existigInvoiceitem.modifiedby = Context.UserID;
                        existigInvoiceitem.modifiedon = DateTimeNow;

                        if (item.cost!=0 && InfyPOS.Services.ConfigManager.Instance(this.Context.OrganizationID).Config.system_jobworkcostmanual)
                        {
                            var buyingprice = newInvoiceItem.purchasetaxpercentage > 0 ? (item.cost / (100 + newInvoiceItem.purchasetaxpercentage) * 100) : item.cost;
                            newInvoiceItem.amount = newInvoiceItem.buyingqty*buyingprice;
                            newInvoiceItem.purchasetotaldiscount = 0;
                            newInvoiceItem.purchasediscount = 0;
                            newInvoiceItem.basebuyingprice = buyingprice;
                            newInvoiceItem.buyingprice = buyingprice;
                            newInvoiceItem.purchasetax = (item.cost - buyingprice)*item.qty;
                        }
                        else
                        {
                            newInvoiceItem.buyingprice += item.chargerate;
                            newInvoiceItem.purchasetax += item.chargetax;
                            newInvoiceItem.amount += (item.chargerate * item.qty);
                        }

                        //throw new Exception("buying price " + newInvoiceItem.buyingprice);

                        var itemdetails = itemservice.Search(new ItemsCreteria()
                        {
                            brandid = newInvoiceItem.Details.brandid,
                            colourid = newInvoiceItem.Details.colourid,
                            materialid = newInvoiceItem.Details.materialid,
                            patternid = newInvoiceItem.Details.patternid,
                            typeid = newInvoiceItem.Details.typeid,
                            sizeid = newInvoiceItem.Details.sizeid,
                            styleid = newInvoiceItem.Details.styleid,
                            fitid = newInvoiceItem.Details.fitid,
                            sleeveid = newInvoiceItem.Details.sleeveid,
                            productid = newInvoiceItem.Details.productid,
                            productmap = true
                        }, new List<long>(), conn, transaction);

                        if (itemdetails.Count == 1)
                        {
                            newInvoiceItem.itemid = itemdetails[0].id;
                        }
                        else
                        {
                            var newitem = newInvoiceItem.Details;
                            newitem.isactive = true;
                            newitem.salemode = 1;
                            newitem.version = 1;
                            newitem = itemservice.Insert(newitem);
                            newInvoiceItem.itemid = newitem.id;
                        }
                        lrinvoiceitems.Insert(transaction, conn, newInvoiceItem);
                        item.newinvoiceitemid = newInvoiceItem.id;
                        
                        var param = new SortedList<string, object>();
                        param.Add("id", newInvoiceItem.id);
                        param.Add("jobworkcharge", item.chargerate);
                        param.Add("jobworktax", item.chargetax);
                        param.Add("jobworkerid", oJobworkbillgrn.supplierid);
                        param.Add("itemid", newInvoiceItem.itemid);
                        param.Add("productid", newInvoiceItem.Details.productid);
                        param.Add("stockid", item.stockid);
                        param.Add("price", item.finalprice);
                        param.Add("buyingitemid", existigInvoiceitem.itemid);
                        param.Add("imageid", string.IsNullOrEmpty(item.imageid) ? newInvoiceItem.imgid : item.imageid);
                        param.Add("jobworkgrnitemid", item.id);
                        param.Add("locationid", oJobworkbillgrn.locationid);
                        param.Add("designid", string.IsNullOrEmpty(item.designid) ? "" : item.designid);
                        param.Add("buyingdesignid", string.IsNullOrEmpty(newInvoiceItem.designid) ? "" : newInvoiceItem.designid);
                        ExecuteQuery("update lrinvoiceitems set jobworkcharge=@jobworkcharge,jobworktax=@jobworktax,designid=@designid,productid=@productid,buyingdesignid=@buyingdesignid,buyingitemid=@buyingitemid,itemid=@itemid,jobworkerid=@jobworkerid,jobworkgrnitemid=@jobworkgrnitemid,sellableqty=1,version=100 where id=@id", param, conn, transaction);
                        newInvoiceItem.version = 100;
                        param.Clear();
                        param.Add("jobworkbillitemsid", item.jobworkbillitemsid);
                        param.Add("invoiceitemid", existigInvoiceitem.id);
                        string sourcebarcode = "";

                        decimal jobworkbillitemsoqty = 0;
                        decimal jobworkbillitemsrqty = 0;
                        List<string> oldbarcodelist = new List<string>();
                        string barcodequeryList = @"select barcode,iqty,oqty from jobworkbillitems
		                                        where jobworkbillitems.id = @jobworkbillitemsid;
                                                select barcode from stock where invoiceitemid = @invoiceitemid;";
                        using (var reader = ExecuteReader(barcodequeryList, param, conn, transaction))
                        {
                            if (reader.Read())
                            {
                                sourcebarcode = reader.GetString(0);
                                jobworkbillitemsoqty = reader.IsDBNull(1)? 0 : reader.GetDecimal(1);
                                jobworkbillitemsrqty = reader.IsDBNull(2) ? 0 : reader.GetDecimal(2);
                            }
                            if (reader.NextResult())
                            {
                                while (reader.Read())
                                {
                                    oldbarcodelist.Add(reader.GetString(0));
                                }
                            }
                        }

						if(jobworkbillitemsoqty>0 && jobworkbillitemsrqty>0)
						{
                            decimal warehousestoktoberollbaced = newInvoiceItem.buyingqty * jobworkbillitemsoqty / jobworkbillitemsrqty;
                            param.Clear();
	                        param.Add("id", item.stockid);
	                        param.Add("lessqty", warehousestoktoberollbaced);
	                        ExecuteQuery(@"update stock set qty=qty-@lessqty where id=@id", param, conn, transaction);
						}
                        oldbarcodelist.Remove(sourcebarcode);
                        existigInvoiceitem.barcodeList = oldbarcodelist;
                        lrinvoiceitems.UpdateBarcode(transaction, conn, existigInvoiceitem);

                        param.Clear();
                        param.Add("id", existigInvoiceitem.id);
                        param.Add("amount", existigInvoiceitem.amount);
                        param.Add("buyingqty", existigInvoiceitem.buyingqty);
                        param.Add("purchasetotaldiscount", existigInvoiceitem.purchasetotaldiscount);
                        param.Add("purchasetax", existigInvoiceitem.purchasetax);
                        param.Add("modifiedby", existigInvoiceitem.modifiedby);
                        param.Add("modifiedon", existigInvoiceitem.modifiedon);

                        ExecuteQuery(@"update lrinvoiceitems set amount=@amount,buyingqty=@buyingqty,purchasetotaldiscount=@purchasetotaldiscount,purchasetax=@purchasetax,
                        modifiedby=@modifiedby,modifiedon=@modifiedon,version=version+1 where id=@id", param, conn, transaction);

                        lrinvoiceitems.UpdateBarcode(transaction, conn, newInvoiceItem);

                        ProductsDataService dataService = new ProductsDataService() { Context = this.Context };
                        dataService.Context = this.Context;
                        var products = dataService.Search(new ProductsCreteria()
                        {
                            ids = new List<long>(new long[] { item.productid })
                        });

                        TaxDataService taxService = new TaxDataService() { Context = this.Context };
                        taxService.Context = this.Context;
                        var taxlist = taxService.Search(new TaxCreteria()
                        {
                            ids = new List<long>(new long[] { products.First().salestaxid })
                        });

                        StockDataService stockDataService = new StockDataService() { Context = this.Context };
                        var barcodeitem = new Lrinvoiceitemsbarcode()
                        {
                            actualqty = newInvoiceItem.actualqty,
                            barcodes = newInvoiceItem.barcodes,
                            buyingqty = newInvoiceItem.buyingqty,
                            companyid = oJobworkbillgrn.companyid,
                            damagedqty = newInvoiceItem.damagedqty,
                            Details = newInvoiceItem.Details,
                            hsncode = newInvoiceItem.hsncode,
                            id = newInvoiceItem.id,
                            gln = newInvoiceItem.Details.gln,
                            itemdetail = newInvoiceItem.itemdetail,
                            itemid = newInvoiceItem.itemid,
                            lrid = newInvoiceItem.lrid,
                            printingname = newInvoiceItem.Details.printingname,
                            productid = newInvoiceItem.productid,
                            returnqty = newInvoiceItem.returnqty,
                            salediscount = newInvoiceItem.salediscount,
                            sellableqty = newInvoiceItem.sellableqty,
                            sellingprice = newInvoiceItem.sellingprice,
                            stocklocationid = newInvoiceItem.stocklocationid,
                            version = newInvoiceItem.version,
                            newbarcode = products[0].SellingMode == Products.sellingMode.Piece ? Convert.ToInt32(item.qty) : 1
                        };
                        var newBarcode = stockDataService.CreateBarcode(barcodeitem,
                            products.First(), taxlist.First(), false, conn, transaction);
                        if (newBarcode != null && newBarcode.stock != null && barcodeitem.stock.Count > 0)
                        {
                            item.barcodes = barcodeitem.stock.ConvertAll(e => e.barcode);
                            newInvoiceItem.barcodeList = item.barcodes;
                            lrinvoiceitems.UpdateBarcode(transaction, conn, newInvoiceItem);
                            if (item.salablegoodsid > 0)
                            {
                                SalablegoodsDataService salablegoodsDataService = new SalablegoodsDataService() { Context = this.Context };
                                StocktransactionDataService stocktransactionDataService = new StocktransactionDataService() { Context = this.Context };
                                foreach (var stock in barcodeitem.stock)
                                {
                                    var existbarcode = salablegoodsDataService.Search(new SalablegoodsCreteria()
                                    {
                                        locationid = oJobworkbillgrn.locationid,
                                        barcode = stock.barcode
                                    });
                                    Salablegoods sitem = new Salablegoods();
                                    sitem.inwardon = DateTimeNow;
                                    sitem.barcode = stock.barcode;
                                    sitem.bundleid = -5;
                                    sitem.iqty = stock.qty - stock.returnqty;
                                    sitem.isactive = true;
                                    sitem.itemid = stock.itemid;
                                    sitem.locationid = oJobworkbillgrn.locationid;
                                    sitem.stockid = stock.id;
                                    sitem.version = 1;
                                    sitem.companyid = stock.companyid;
                                    sitem.supplierid = stock.supplierid;
                                    sitem.received = true;
                                    sitem.price = stock.price;
                                    sitem.discount = stock.discount;
                                    sitem.additionaldiscount = stock.additionaldiscount;
                                    sitem.ispurchaselocation = true;
                                    sitem.pqty = stock.qty - stock.returnqty;
                                    sitem.costprice = stock.buyingprice;
                                    sitem.icostprice = stock.ibuyingprice;
                                    if (existbarcode.Count > 0)
                                    {
                                        sitem.id = existbarcode[0].id;
                                        existbarcode[0].iqty += stock.qty - stock.returnqty;
                                        if (existbarcode[0].oqty > existbarcode[0].iqty)
                                            existbarcode[0].iqty = existbarcode[0].oqty;
                                        salablegoodsDataService.Update(transaction, conn, existbarcode[0], existbarcode[0]);
                                    }
                                    else
                                    {
                                        salablegoodsDataService.Insert(transaction, conn, sitem);
                                    }
                                    stocktransactionDataService.IncreaseStock(sitem, transaction, conn);
                                }
                            }
                        }
                    }
                }
                foreach (var item in directstock)
                {
                    StockDataService stockDataService = new StockDataService() { Context = this.Context };
                    var stockitem = stockDataService.Search(new StockCreteria()
                    {
                        id = item.stockid
                    }).First();

                    long itemid = 0;
                    var itemdetails = itemservice.Search(new ItemsCreteria()
                    {
                        brandid = item.brandid,
                        colourid = item.colourid,
                        materialid = item.materialid,
                        patternid = item.patternid,
                        typeid = item.typeid,
                        sizeid = item.sizeid,
                        sleeveid = item.sleeveid,
                        styleid = item.styleid,
                        productid = item.productid,
                        productmap = true
                    }, new List<long>(), conn, transaction);

                    if (itemdetails.Count == 1)
                    {
                        itemid = itemdetails[0].id;
                    }
                    else
                    {
                        var newitem = new Items()
                        {
                            brandid = item.brandid,
                            colourid = item.colourid,
                            materialid = item.materialid,
                            patternid = item.patternid,
                            typeid = item.typeid,
                            sizeid = item.sizeid,
                            sleeveid = item.sleeveid,
                            styleid = item.styleid,
                            productid = item.productid
                        };
                        newitem.isactive = true;
                        newitem.salemode = 1;
                        newitem.version = 1;
                        newitem = itemservice.Insert(newitem);
                        itemid = newitem.id;
                    }

                    ProductsDataService dataService = new ProductsDataService() { Context = this.Context };
                    dataService.Context = this.Context;
                    var products = dataService.Search(new ProductsCreteria()
                    {
                        ids = new List<long>(new long[] { item.productid })
                    });

                    TaxDataService taxService = new TaxDataService() { Context = this.Context };
                    taxService.Context = this.Context;
                    var taxlist = taxService.Search(new TaxCreteria()
                    {
                        ids = new List<long>(new long[] { products.First().salestaxid })
                    });

                    var barcodeConfig = InfyPOS.Processors.BarcodeManager.Instance(Context.OrganizationID).Get(products[0].barcodemode, oJobworkbillgrn.companyid);

                    List<InfyPOS.Processors.BarcodeManager.NewBarcodeID> barcodes = new List<InfyPOS.Processors.BarcodeManager.NewBarcodeID>();
                    var barcodecount = products[0].SellingMode == Products.sellingMode.Piece ? Convert.ToInt32(item.qty) : 1;
                    var missingstock = new MissingBarcode()
                    {
                        productid = products[0].id,
                        locationid = oJobworkbillgrn.locationid,
                        companyid = oJobworkbillgrn.companyid,
                        sellingprice = item.finalprice,
                        sizeid = item.sizeid,
                        qty = item.qty,
                    };

                    if (Services.ConfigManager.Instance(Context.OrganizationID).Config.system_locationbasedbarcodes && oJobworkbillgrn.locationid > 0)
                        barcodes = stockDataService.CreateBarcode(barcodeConfig.Type, barcodecount, oJobworkbillgrn.companyid, conn, transaction,
                            oJobworkbillgrn.locationid, missingstock, null);
                    else
                        barcodes = stockDataService.CreateBarcode(barcodeConfig.Type, barcodecount, oJobworkbillgrn.companyid, conn, transaction, missingstock);



                    if (barcodes != null && barcodes.Count > 0)
                    {
                        item.barcodes = barcodes.ConvertAll(x => x.code);

                        if (item.salablegoodsid > 0)
                        {
                            SalablegoodsDataService salablegoodsDataService = new SalablegoodsDataService() { Context = this.Context };
                            StocktransactionDataService stocktransactionDataService = new StocktransactionDataService() { Context = this.Context };
                            foreach (var stockbarcode in barcodes)
                            {
                                Stock stock = new Stock();
                                stock.barcode = stockbarcode.code;
                                stock.price = item.finalprice;
                                stock.dealerprice = item.dealerprice;
                                stock.itemid = itemid;
                                stock.productid = item.productid;
                                stock.isactive = true;
                                stock.designid = item.designid;
                                stock.buyingqty = item.qty;
                                stock.transfered = hasrecivingonretail ? item.qty : 0;
                                stock.qty = item.qty;
                                stock.buyingprice = stockitem.buyingprice + item.chargegross;
                                stock.ibuyingprice = stockitem.ibuyingprice + item.chargetotal;
                                stock.companyid = oJobworkbillgrn.companyid;
                                stock.supplierid = oJobworkbillgrn.supplierid;
                                if (stock.buyingprice == 0)
                                    stock.buyingprice = item.finalprice;

                                if (stock.ibuyingprice == 0)
                                    stock.ibuyingprice = item.finalprice;
                                var existingstock = stockDataService.Search(new StockCreteria()
                                {
                                    barcode = stockbarcode.code
                                });
                                if (existingstock.Count > 0)
                                {
                                    existingstock[0].buyingqty += stock.buyingqty;
                                    existingstock[0].qty += stock.qty;
                                    existingstock[0].transfered += stock.transfered;
                                    stockDataService.Update(transaction, conn, existingstock[0]);
                                }
                                else
                                {
                                    stockDataService.Insert(transaction, conn, stock);
                                }
                                if (hasrecivingonretail)
                                {
                                    Salablegoods sitem = new Salablegoods();
                                    sitem.inwardon = DateTimeNow;
                                    sitem.barcode = stock.barcode;
                                    sitem.bundleid = -5;
                                    sitem.iqty = stock.qty - stock.returnqty;
                                    sitem.isactive = true;
                                    sitem.itemid = stock.itemid;
                                    sitem.locationid = oJobworkbillgrn.locationid;
                                    sitem.stockid = stock.id;
                                    sitem.version = 1;
                                    sitem.companyid = stock.companyid;
                                    sitem.supplierid = stock.supplierid;
                                    sitem.received = true;
                                    sitem.price = stock.price;
                                    sitem.discount = stock.discount;
                                    sitem.additionaldiscount = stock.additionaldiscount;
                                    sitem.ispurchaselocation = true;
                                    sitem.pqty = stock.qty - stock.returnqty;
                                    sitem.costprice = stock.buyingprice;
                                    sitem.icostprice = stock.ibuyingprice;
                                    salablegoodsDataService.Insert(transaction, conn, sitem);
                                    var existbarcode = salablegoodsDataService.Search(new SalablegoodsCreteria()
                                    {
                                        locationid = oJobworkbillgrn.locationid,
                                        barcode = stock.barcode
                                    });
                                    if (existbarcode.Count > 0)
                                    {
                                        sitem.id = existbarcode[0].id;
                                        existbarcode[0].iqty += stock.qty - stock.returnqty;
                                        if (existbarcode[0].oqty > existbarcode[0].iqty)
                                            existbarcode[0].iqty = existbarcode[0].oqty;
                                        salablegoodsDataService.Update(transaction, conn, existbarcode[0], existbarcode[0]);
                                    }
                                    else
                                    {
                                        salablegoodsDataService.Insert(transaction, conn, sitem);
                                    }
                                    stocktransactionDataService.IncreaseStock(sitem, transaction, conn);
                                }
                            }
                        }
                    }
                }

            }


            //if (directstock.Count > 0)
            //{
            //    StockDataService stockDataService = new StockDataService() { Context = this.Context };
            //    foreach (var item in directstock)
            //    {
            //        var stock = stockDataService.Search(new StockCreteria()
            //        {
            //            id = item.stockid
            //        });

            //        var itemdetails = itemservice.Search(new ItemsCreteria()
            //        {
            //            brandid = item.brandid,
            //            colourid = item.colourid,
            //            materialid = item.materialid,
            //            patternid = item.patternid,
            //            typeid = item.typeid,
            //            sizeid = item.sizeid,
            //            styleid = item.styleid,
            //            productid = item.productid,
            //            productmap = true
            //        }, new List<long>(), conn, transaction);

            //        long newitemid = 0;
            //        if (itemdetails.Count > 0)
            //        {
            //            newitemid = itemdetails[0].id;
            //        }
            //        else
            //        {
            //            var newitem = new Items()
            //            {
            //                brandid = item.brandid,
            //                colourid = item.colourid,
            //                materialid = item.materialid,
            //                patternid = item.patternid,
            //                typeid = item.typeid,
            //                sizeid = item.sizeid,
            //                styleid = item.styleid,
            //                productid = item.productid
            //            };
            //            newitem.isactive = true;
            //            newitem.salemode = 1;
            //            newitem.version = 1;
            //            newitem = itemservice.Insert(newitem);
            //            newitemid = newitem.id;
            //        }

            //        var param = new SortedList<string, object>();
            //        param.Add("id", newInvoiceItem.id);
            //        param.Add("jobworkcharge", item.chargerate);
            //        param.Add("jobworktax", item.chargetax);
            //        param.Add("jobworkerid", oJobworkbillgrn.supplierid);
            //        param.Add("itemid", newInvoiceItem.itemid);
            //        param.Add("productid", newInvoiceItem.Details.productid);
            //        param.Add("stockid", item.stockid);
            //        param.Add("price", item.finalprice);
            //        param.Add("buyingitemid", buyingitemid);
            //        param.Add("imageid", string.IsNullOrEmpty(item.imageid) ? newInvoiceItem.imgid : item.imageid);
            //        param.Add("jobworkgrnitemid", item.id);
            //        param.Add("locationid", oJobworkbillgrn.locationid);
            //        newInvoiceItem.barcodeList = new List<string>(new string[] { item.barcode });
            //        lrinvoiceitems.UpdateBarcode(transaction, conn, newInvoiceItem);
            //        param.Add("designid", string.IsNullOrEmpty(item.designid) ? "" : item.designid);
            //        param.Add("buyingdesignid", string.IsNullOrEmpty(newInvoiceItem.designid) ? "" : newInvoiceItem.designid);
            //        ExecuteQuery("update stock set itemid=@itemid,imageid=@imageid,productid=@productid,designid=@designid,buyingprice=buyingprice+@jobworkcharge,ibuyingprice=ibuyingprice+@jobworkcharge+@jobworktax,price=@price,transfered=1,version=100 where id=@stockid", param, conn, transaction);
            //        ExecuteQuery("update salablegoods set costprice=costprice+@jobworkcharge,icostprice=icostprice+@jobworkcharge+@jobworktax,price=@price,iqty=1,version=100 where stockid=@stockid and locationid=@locationid", param, conn, transaction);
            //        ExecuteQuery("update jobworkbillgrnitems set newinvoiceitemid=-1 where id=@jobworkgrnitemid", param, conn, transaction);
            //        item.newinvoiceitemid = -1;
            //        item.barcodes = new List<string>(new string[] { item.barcode });

            //    }
            //}

            NpgsqlCommand updateJobWorkBillGrnBarcodes = new NpgsqlCommand("update jobworkbillgrnitems set barcodes=@barcodes where id=@id", conn, transaction);
            updateJobWorkBillGrnBarcodes.Parameters.Add("barcodes", NpgsqlTypes.NpgsqlDbType.Json).Value = ""; ;
            updateJobWorkBillGrnBarcodes.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = 0;
            foreach (var item in oJobworkbillgrn.Jobworkbillitems)
            {
                if (item.barcodes != null && item.barcodes.Count > 0)
                {
                    updateJobWorkBillGrnBarcodes.Parameters["barcodes"].Value = Newtonsoft.Json.JsonConvert.SerializeObject(item.barcodes);
                    updateJobWorkBillGrnBarcodes.Parameters["id"].Value = item.id;
                    updateJobWorkBillGrnBarcodes.ExecuteNonQuery();
                }
            }
            var samebarcodes = oJobworkbillgrn.Jobworkbillitems.Sum(e => e.newinvoiceitemid != 0 ? e.qty : 0);
            var regeneratedbarcode = oJobworkbillgrn.Jobworkbillitems.Sum(e => e.newinvoiceitemid == 0 ? e.qty : 0);
            oJobworkbillgrn.ErrorMessage = "Updated Barcodes : " + samebarcodes + ",Regeneration Needed:" + regeneratedbarcode;
            return true;
        }
        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Jobworkbillgrn oJobworkbillgrn)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oJobworkbillgrn.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oJobworkbillgrn.modifiedby;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oJobworkbillgrn.modifiedon;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oJobworkbillgrn.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oJobworkbillgrn.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

    }

}
