using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
namespace InfyPOS.Data

{
    public class SupplierpaymentinvoiceDataService : SAASData
    {
        const string SQL_Search = @"select id, supplierid, invoiceid, invoiceno,coreinvoiceno,coreinvoicedate,  paidamount, isactive, createdby, modifiedby, version, createdon, modifiedon, 
        supplierpaymentid,islrinvoicereturn,iscreditnote,invoicedate,discountamountpercent,discountamount,agentid,agentcommission,agentcommissionvalue,intreceive,intpay,isjobwork from supplierpaymentinvoice where id=@id";
        const string SQL_SearchAll = @"select supplierpaymentinvoice.id, supplierpaymentinvoice.supplierid, supplierpaymentinvoice.invoiceid, supplierpaymentinvoice.invoiceno,
coreinvoiceno,coreinvoicedate, supplierpaymentinvoice.paidamount, supplierpaymentinvoice.isactive, supplierpaymentinvoice.createdby, 
supplierpaymentinvoice.modifiedby, supplierpaymentinvoice.version, supplierpaymentinvoice.createdon, supplierpaymentinvoice.modifiedon, 
supplierpaymentid, islrinvoicereturn,iscreditnote,invoicedate,discountamountpercent,discountamount,supplierpaymentinvoice.intpay,supplierpaymentinvoice.intreceive,
supplierpaymentinvoice.agentid,agentcommission,agentcommissionvalue,isjobwork, lrinvoicereturn.returntype 
from supplierpaymentinvoice left outer join lrinvoicereturn on lrinvoicereturn.id = supplierpaymentinvoice.invoiceid
where supplierpaymentid=@supplierpaymentid
order by invoicedate ";

        const string SQL_Insert_Invoice = @"update lrinvoice set plannedpayment=0 where id=@invoiceid;
        INSERT INTO public.supplierpaymentinvoice(supplierid, invoiceid, invoiceno, coreinvoiceno,coreinvoicedate,paidamount, isactive, createdby, modifiedby, version, createdon, modifiedon, supplierpaymentid,islrinvoicereturn,iscreditnote,invoicedate,discountamountpercent,discountamount,agentid,agentcommission,agentcommissionvalue,isjobwork,intreceive,intpay) 
        VALUES(@supplierid, @invoiceid, @invoiceno,@coreinvoiceno,@coreinvoicedate, @paidamount, @isactive, @createdby, @modifiedby, @version, @createdon, @modifiedon, @supplierpaymentid, @islrinvoicereturn,@iscreditnote,@invoicedate,@discountamountpercent,@discountamount,@agentid,@agentcommission,@agentcommissionvalue,@isjobwork,@intreceive,@intpay) RETURNING id";

        const string SQL_Insert_Debitnote = @"update lrinvoicereturn set plannedpayment=0 where id=@invoiceid;
        INSERT INTO public.supplierpaymentinvoice(supplierid, invoiceid, invoiceno,coreinvoiceno,coreinvoicedate, paidamount, isactive, createdby, modifiedby, version, createdon, modifiedon, supplierpaymentid,islrinvoicereturn,iscreditnote,invoicedate,discountamountpercent,discountamount,agentid,agentcommission,agentcommissionvalue,isjobwork) 
        VALUES(@supplierid, @invoiceid, @invoiceno,@coreinvoiceno,@coreinvoicedate, @paidamount, @isactive, @createdby, @modifiedby, @version, @createdon, @modifiedon, @supplierpaymentid, @islrinvoicereturn,@iscreditnote,@invoicedate,@discountamountpercent,@discountamount,@agentid,@agentcommission,@agentcommissionvalue,@isjobwork) RETURNING id";

        const string SQL_Update = @"UPDATE public.supplierpaymentinvoice SET supplierid=@supplierid, invoiceid=@invoiceid, invoiceno=@invoiceno,
        coreinvoiceno=@coreinvoiceno,coreinvoicedate=@coreinvoicedate,paidamount=@paidamount, modifiedby=@modifiedby, version=@version+1, modifiedon=@modifiedon, supplierpaymentid=@supplierpaymentid, 
        islrinvoicereturn=@islrinvoicereturn,iscreditnote=@iscreditnote,invoicedate=@invoicedate,discountamountpercent=@discountamountpercent,agentid=@agentid,isjobwork=@isjobwork,intreceive=@intreceive,intpay=@intpay,
        agentcommission=@agentcommission,agentcommissionvalue=@agentcommissionvalue,discountamount=@discountamount where id=@id and version=@version RETURNING version";

        const string SQL_Delete = @"update public.supplierpaymentinvoice set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";
        const string SQL_DeleteAll = @"update public.supplierpaymentinvoice set isactive=false,modifiedby=@modifiedby,modifiedon=@modifiedon where supplierpaymentid=@supplierpaymentid";
        public List<Supplierpaymentinvoice> Search(SupplierpaymentinvoiceCreteria _supplierpaymentinvoice)
        {
            List<Supplierpaymentinvoice> lst = new List<Supplierpaymentinvoice>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_supplierpaymentinvoice.id != 0)
                {
                    command = GetCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _supplierpaymentinvoice.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("isactive", true, "=");

                    //if (!string.IsNullOrEmpty(_supplierpaymentinvoice.Code))
                    //    qb.AddParameter("code", _supplierpaymentinvoice.Code, "=");

                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Supplierpaymentinvoice osupplierpaymentinvoice = new Supplierpaymentinvoice();
                        osupplierpaymentinvoice.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        osupplierpaymentinvoice.supplierid = reader["supplierid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["supplierid"]);
                        osupplierpaymentinvoice.invoiceid = reader["invoiceid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["invoiceid"]);
                        osupplierpaymentinvoice.invoiceno = reader["invoiceno"] == DBNull.Value ? "" : reader["invoiceno"].ToString();

                        osupplierpaymentinvoice.coreinvoicedate = reader["coreinvoicedate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["coreinvoicedate"]);
                        osupplierpaymentinvoice.coreinvoiceno = reader["coreinvoiceno"] == DBNull.Value ? "" : reader["coreinvoiceno"].ToString();

                        osupplierpaymentinvoice.isjobwork = reader["isjobwork"] == DBNull.Value ? false : Convert.ToBoolean(reader["isjobwork"]);

                        osupplierpaymentinvoice.paidamount = reader["paidamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["paidamount"]);
                        osupplierpaymentinvoice.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        osupplierpaymentinvoice.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        osupplierpaymentinvoice.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        osupplierpaymentinvoice.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        osupplierpaymentinvoice.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        osupplierpaymentinvoice.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        osupplierpaymentinvoice.supplierpaymentid = reader["supplierpaymentid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["supplierpaymentid"]);
                        osupplierpaymentinvoice.islrinvoicereturn = reader["islrinvoicereturn"] == DBNull.Value ? false : Convert.ToBoolean(reader["islrinvoicereturn"]);
                        osupplierpaymentinvoice.iscreditnote = reader["iscreditnote"] == DBNull.Value ? false : Convert.ToBoolean(reader["iscreditnote"]);
                        osupplierpaymentinvoice.paidamount = reader["paidamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["paidamount"]);
                        osupplierpaymentinvoice.invoicedate = reader["invoicedate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["invoicedate"]);
                        osupplierpaymentinvoice.discountamount = reader["discountamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discountamount"]);
                        osupplierpaymentinvoice.discountamountpercent = reader["discountamountpercent"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discountamountpercent"]);

                        osupplierpaymentinvoice.agentid = reader["agentid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["agentid"]);
                        osupplierpaymentinvoice.agentcommission = reader["agentcommission"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["agentcommission"]);
                        osupplierpaymentinvoice.agentcommissionvalue = reader["agentcommissionvalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["agentcommissionvalue"]);

                        osupplierpaymentinvoice.intpay = reader["intpay"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["intpay"]);
                        osupplierpaymentinvoice.intreceive = reader["intreceive"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["intreceive"]);

                        lst.Add(osupplierpaymentinvoice);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _supplierpaymentinvoice);
            }
            return lst;
        }

        public List<Supplierpaymentinvoice> SearchAll(long _supplierpaymentid)
        {
            List<Supplierpaymentinvoice> lst = new List<Supplierpaymentinvoice>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = GetCommand(SQL_SearchAll, conn);
                command.Parameters.Add("supplierpaymentid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _supplierpaymentid;
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Supplierpaymentinvoice osupplierpaymentinvoice = new Supplierpaymentinvoice();
                        osupplierpaymentinvoice.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        osupplierpaymentinvoice.supplierid = reader["supplierid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["supplierid"]);
                        osupplierpaymentinvoice.invoiceid = reader["invoiceid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["invoiceid"]);
                        osupplierpaymentinvoice.invoiceno = reader["invoiceno"] == DBNull.Value ? "" : reader["invoiceno"].ToString();
                        osupplierpaymentinvoice.coreinvoicedate = reader["coreinvoicedate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["coreinvoicedate"]);
                        osupplierpaymentinvoice.coreinvoiceno = reader["coreinvoiceno"] == DBNull.Value ? "" : reader["coreinvoiceno"].ToString();
                        osupplierpaymentinvoice.paidamount = reader["paidamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["paidamount"]);
                        osupplierpaymentinvoice.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        osupplierpaymentinvoice.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        osupplierpaymentinvoice.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        osupplierpaymentinvoice.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        osupplierpaymentinvoice.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        osupplierpaymentinvoice.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        osupplierpaymentinvoice.supplierpaymentid = reader["supplierpaymentid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["supplierpaymentid"]);
                        osupplierpaymentinvoice.islrinvoicereturn = reader["islrinvoicereturn"] == DBNull.Value ? false : Convert.ToBoolean(reader["islrinvoicereturn"]);
                        osupplierpaymentinvoice.iscreditnote = reader["iscreditnote"] == DBNull.Value ? false : Convert.ToBoolean(reader["iscreditnote"]);
                        osupplierpaymentinvoice.paidamount = reader["paidamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["paidamount"]);
                        osupplierpaymentinvoice.invoicedate = reader["invoicedate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["invoicedate"]);
                        osupplierpaymentinvoice.discountamount = reader["discountamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discountamount"]);
                        osupplierpaymentinvoice.discountamountpercent = reader["discountamountpercent"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discountamountpercent"]);
                        osupplierpaymentinvoice.intpay = reader["intpay"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["intpay"]);
                        osupplierpaymentinvoice.intreceive = reader["intreceive"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["intreceive"]);
                        osupplierpaymentinvoice.agentid = reader["agentid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["agentid"]);
                        osupplierpaymentinvoice.agentcommission = reader["agentcommission"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["agentcommission"]);
                        osupplierpaymentinvoice.agentcommissionvalue = reader["agentcommissionvalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["agentcommissionvalue"]);
                        osupplierpaymentinvoice.isjobwork = reader["isjobwork"] == DBNull.Value ? false : Convert.ToBoolean(reader["isjobwork"]);
                        var returntype = reader["returntype"] == DBNull.Value ? -9 : Convert.ToInt64(reader["returntype"]);
                        osupplierpaymentinvoice.returntype = returntype;
                        if (returntype == 5)
                            osupplierpaymentinvoice.invoicetype = "Discount Note";
                        else if (returntype == 6)
                            osupplierpaymentinvoice.invoicetype = "Supplier Credit Note";
                        else if (returntype == 7)
                            osupplierpaymentinvoice.invoicetype = "Supplier Advance";
                        else if (returntype == 11)
                            osupplierpaymentinvoice.invoicetype = "Excess Credit Note";
                        else if (returntype == 12)
                            osupplierpaymentinvoice.invoicetype = "Price Credit Note";
                        else if (returntype == 13)
                            osupplierpaymentinvoice.invoicetype = "Supplier Refund";
                        else if (returntype == -9)
                            osupplierpaymentinvoice.invoicetype = "Invoice";
                        else
                            osupplierpaymentinvoice.invoicetype = "Debit Note";

                        lst.Add(osupplierpaymentinvoice);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _supplierpaymentid);
            }
            return lst;
        }

        public Supplierpaymentinvoice Insert(Supplierpaymentinvoice oSupplierpaymentinvoice)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    Insert(transaction, conn, oSupplierpaymentinvoice);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oSupplierpaymentinvoice);


            }

            return oSupplierpaymentinvoice;
        }

        public Supplierpaymentinvoice Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Supplierpaymentinvoice oSupplierpaymentinvoice)
        {
            var debitnote = oSupplierpaymentinvoice.islrinvoicereturn || oSupplierpaymentinvoice.paidamount < 0;
            NpgsqlCommand command = GetCommand(debitnote ? SQL_Insert_Debitnote : SQL_Insert_Invoice, conn, transaction);
            command.Parameters.Add("supplierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSupplierpaymentinvoice.supplierid;
            command.Parameters.Add("invoiceid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSupplierpaymentinvoice.invoiceid;
            command.Parameters.Add("invoiceno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSupplierpaymentinvoice.invoiceno == null ? "" : oSupplierpaymentinvoice.invoiceno;

            command.Parameters.Add("coreinvoiceno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSupplierpaymentinvoice.coreinvoiceno == null ? "" : oSupplierpaymentinvoice.coreinvoiceno.Substring(0,200);
            command.Parameters.Add("coreinvoicedate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oSupplierpaymentinvoice.coreinvoicedate;

            command.Parameters.Add("paidamount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oSupplierpaymentinvoice.paidamount;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oSupplierpaymentinvoice.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oSupplierpaymentinvoice.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("supplierpaymentid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSupplierpaymentinvoice.supplierpaymentid;
            command.Parameters.Add("islrinvoicereturn", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oSupplierpaymentinvoice.islrinvoicereturn || oSupplierpaymentinvoice.paidamount<0;
            command.Parameters.Add("iscreditnote", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oSupplierpaymentinvoice.iscreditnote;

            command.Parameters.Add("invoicedate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oSupplierpaymentinvoice.invoicedate;
            command.Parameters.Add("discountamount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oSupplierpaymentinvoice.discountamount;
            command.Parameters.Add("discountamountpercent", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oSupplierpaymentinvoice.discountamountpercent;

            command.Parameters.Add("intpay", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oSupplierpaymentinvoice.intpay;
            command.Parameters.Add("intreceive", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oSupplierpaymentinvoice.intreceive;

            command.Parameters.Add("agentid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSupplierpaymentinvoice.agentid;
            command.Parameters.Add("agentcommission", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oSupplierpaymentinvoice.agentcommission;
            command.Parameters.Add("agentcommissionvalue", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oSupplierpaymentinvoice.agentcommissionvalue;
            command.Parameters.Add("isjobwork", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oSupplierpaymentinvoice.isjobwork;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oSupplierpaymentinvoice.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            return oSupplierpaymentinvoice;
        }

        public Supplierpaymentinvoice Update(Supplierpaymentinvoice oSupplierpaymentinvoice)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oSupplierpaymentinvoice);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oSupplierpaymentinvoice);


            }

            return oSupplierpaymentinvoice;
        }

        public Supplierpaymentinvoice Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Supplierpaymentinvoice oSupplierpaymentinvoice)
        {

            NpgsqlCommand command = GetCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSupplierpaymentinvoice.id;
            command.Parameters.Add("supplierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSupplierpaymentinvoice.supplierid;
            command.Parameters.Add("invoiceid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSupplierpaymentinvoice.invoiceid;
            command.Parameters.Add("invoiceno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSupplierpaymentinvoice.invoiceno == null ? "" : oSupplierpaymentinvoice.invoiceno;

            command.Parameters.Add("coreinvoiceno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = string.IsNullOrEmpty(oSupplierpaymentinvoice.coreinvoiceno) ? "" : oSupplierpaymentinvoice.coreinvoiceno.Substring(0, 200);
            command.Parameters.Add("coreinvoicedate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oSupplierpaymentinvoice.coreinvoicedate;

            command.Parameters.Add("intpay", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oSupplierpaymentinvoice.intpay;
            command.Parameters.Add("intreceive", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oSupplierpaymentinvoice.intreceive;

            command.Parameters.Add("paidamount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oSupplierpaymentinvoice.paidamount;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oSupplierpaymentinvoice.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSupplierpaymentinvoice.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oSupplierpaymentinvoice.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oSupplierpaymentinvoice.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("supplierpaymentid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSupplierpaymentinvoice.supplierpaymentid;
            command.Parameters.Add("islrinvoicereturn", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oSupplierpaymentinvoice.islrinvoicereturn || oSupplierpaymentinvoice.paidamount < 0;
            command.Parameters.Add("iscreditnote", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oSupplierpaymentinvoice.iscreditnote;
            command.Parameters.Add("invoicedate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oSupplierpaymentinvoice.invoicedate;
            command.Parameters.Add("discountamount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oSupplierpaymentinvoice.discountamount;
            command.Parameters.Add("discountamountpercent", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oSupplierpaymentinvoice.discountamountpercent;
            command.Parameters.Add("agentid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSupplierpaymentinvoice.agentid;
            command.Parameters.Add("agentcommission", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oSupplierpaymentinvoice.agentcommission;
            command.Parameters.Add("agentcommissionvalue", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oSupplierpaymentinvoice.agentcommissionvalue;
            command.Parameters.Add("isjobwork", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oSupplierpaymentinvoice.isjobwork;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oSupplierpaymentinvoice.version = Convert.ToInt32(reader[0].ToString());
                }
            }

            return oSupplierpaymentinvoice;
        }

        public bool Update(NpgsqlTransaction transaction, NpgsqlConnection conn, List<Supplierpaymentinvoice> list, long id)
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
                    item.supplierpaymentid = id;
                    Insert(transaction, conn, item);
                }
            }
            return true;
        }

        public bool Delete(Supplierpaymentinvoice oSupplierpaymentinvoice)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oSupplierpaymentinvoice);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oSupplierpaymentinvoice);
            }
            return hasSuccessCount;
        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Supplierpaymentinvoice oSupplierpaymentinvoice)
        {

            NpgsqlCommand command = GetCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oSupplierpaymentinvoice.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oSupplierpaymentinvoice.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSupplierpaymentinvoice.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

        public bool DeleteAll(long supplierpaymentid, long userid, NpgsqlConnection conn, NpgsqlTransaction transaction)
        {


            int successCount = 0;
            NpgsqlCommand command = GetCommand(SQL_DeleteAll, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = false;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("supplierpaymentid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = supplierpaymentid;
            successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

    }

}
