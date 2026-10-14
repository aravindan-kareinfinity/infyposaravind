using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
using static InfyPOS.Tally.FinanceService;

namespace InfyPOS.Data

{
    public class AgentpaymentinvoiceDataService : BaseData
    {
        const string SQL_Search = @"select discountamountpercent, discountamount, paidamount, id, createdon, isactive, supplierpaymentinvoiceid, supplierpaymentid, createdby, modifiedby, version, modifiedon, agentpaymentid,invoiceno,receiptno,agentcommissionvalue,agentcommission,taxamount,taxledgerid,invoicedate,supplierpaiddate,supplierid,isinterstatesale from agentpaymentinvoice where id=@id";
        const string SQL_SearchAll = @"select discountamountpercent, discountamount, paidamount, id, createdon, isactive, supplierpaymentinvoiceid, supplierpaymentid, createdby, modifiedby, version, modifiedon, agentpaymentid,invoiceno,receiptno,agentcommissionvalue,agentcommission,taxamount,taxledgerid,invoicedate,supplierpaiddate,supplierid,isinterstatesale from agentpaymentinvoice where agentpaymentid=@agentpaymentid";

        const string SQL_Insert = @"
        INSERT INTO public.agentpaymentinvoice(discountamountpercent, discountamount, paidamount, createdon, isactive, supplierpaymentinvoiceid, supplierpaymentid, createdby, modifiedby, version, modifiedon, agentpaymentid,invoiceno,receiptno,agentcommissionvalue,agentcommission,taxamount,taxledgerid,invoicedate,supplierpaiddate,supplierid,isinterstatesale)
        VALUES(@discountamountpercent, @discountamount, @paidamount, @createdon, @isactive, @supplierpaymentinvoiceid, @supplierpaymentid, @createdby, @modifiedby, @version, @modifiedon, @agentpaymentid,@invoiceno,@receiptno,@agentcommissionvalue,@agentcommission,@taxamount,@taxledgerid,@invoicedate,@supplierpaiddate,@supplierid,@isinterstatesale) RETURNING id";

        const string SQL_Update = @"UPDATE public.agentpaymentinvoice SET discountamountpercent=@discountamountpercent, discountamount=@discountamount, paidamount=@paidamount, supplierpaymentinvoiceid=@supplierpaymentinvoiceid, 
        supplierpaymentid=@supplierpaymentid, modifiedby=@modifiedby, version=@version+1, modifiedon=@modifiedon, agentpaymentid=@agentpaymentid,
        invoiceno=@invoiceno,receiptno=@receiptno,agentcommissionvalue=@agentcommissionvalue,agentcommission=@agentcommission,taxamount=@taxamount,
        taxledgerid=@taxledgerid,invoicedate=@invoicedate,supplierpaiddate=@supplierpaiddate,supplierid=@supplierid,isinterstatesale=@isinterstatesale
        where id=@id and version=@version RETURNING version";

        const string SQL_Delete = @"update public.agentpaymentinvoice set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";
        const string SQL_DeleteAll = @"update public.agentpaymentinvoice set isactive=false,modifiedby=@modifiedby,modifiedon=@modifiedon where agentpaymentid=@agentpaymentid";
        public List<Agentpaymentinvoice> Search(AgentpaymentinvoiceDetail _agentpaymentinvoice)
        {
            List<Agentpaymentinvoice> lst = new List<Agentpaymentinvoice>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_agentpaymentinvoice.id != 0)
                {
                    command = new NpgsqlCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _agentpaymentinvoice.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("isactive", true, "=");

                    //if (!string.IsNullOrEmpty(_agentpaymentinvoice.Code))
                    //    qb.AddParameter("code", _agentpaymentinvoice.Code, "=");

                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Agentpaymentinvoice oagentpaymentinvoice = new Agentpaymentinvoice();
                        oagentpaymentinvoice.discountamountpercent = reader["discountamountpercent"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discountamountpercent"]);
                        oagentpaymentinvoice.discountamount = reader["discountamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discountamount"]);
                        oagentpaymentinvoice.paidamount = reader["paidamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["paidamount"]);
                        oagentpaymentinvoice.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        oagentpaymentinvoice.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        oagentpaymentinvoice.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        oagentpaymentinvoice.supplierpaymentinvoiceid = reader["supplierpaymentinvoiceid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["supplierpaymentinvoiceid"]);
                        oagentpaymentinvoice.supplierpaymentid = reader["supplierpaymentid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["supplierpaymentid"]);
                        oagentpaymentinvoice.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        oagentpaymentinvoice.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        oagentpaymentinvoice.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        oagentpaymentinvoice.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        oagentpaymentinvoice.agentpaymentid = reader["agentpaymentid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["agentpaymentid"]);

                        oagentpaymentinvoice.supplierid = reader["supplierid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["supplierid"]);

                        oagentpaymentinvoice.isinterstatesale = reader["isinterstatesale"] == DBNull.Value ? false : Convert.ToBoolean(reader["isinterstatesale"]);
                        oagentpaymentinvoice.agentcommissionvalue = reader["agentcommissionvalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["agentcommissionvalue"]);
                        oagentpaymentinvoice.agentcommission = reader["agentcommission"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["agentcommission"]);
                        oagentpaymentinvoice.taxamount = reader["taxamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["taxamount"]);
                        oagentpaymentinvoice.discountamount = reader["discountamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discountamount"]);
                        oagentpaymentinvoice.taxledgerid = reader["taxledgerid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["taxledgerid"]);

                        oagentpaymentinvoice.invpaidamount = reader["paidamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["paidamount"]);
                        oagentpaymentinvoice.invdisount = reader["discountamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discountamount"]);

                        oagentpaymentinvoice.invoiceno = reader["invoiceno"] == DBNull.Value ? "" : Convert.ToString(reader["invoiceno"]);
                        oagentpaymentinvoice.receiptno = reader["receiptno"] == DBNull.Value ? "" : Convert.ToString(reader["receiptno"]);

                        oagentpaymentinvoice.invoicedate = reader["invoicedate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["invoicedate"]);
                        oagentpaymentinvoice.supplierpaiddate = reader["supplierpaiddate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["supplierpaiddate"]);

                        lst.Add(oagentpaymentinvoice);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _agentpaymentinvoice);
            }
            return lst;
        }

        public List<Agentpaymentinvoice> SearchAll(long _agentpaymentid)
        {
            List<Agentpaymentinvoice> lst = new List<Agentpaymentinvoice>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = new NpgsqlCommand(SQL_SearchAll, conn);
                command.Parameters.Add("agentpaymentid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _agentpaymentid;
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Agentpaymentinvoice oagentpaymentinvoice = new Agentpaymentinvoice();
                        oagentpaymentinvoice.discountamountpercent = reader["discountamountpercent"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discountamountpercent"]);
                        oagentpaymentinvoice.discountamount = reader["discountamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discountamount"]);
                        oagentpaymentinvoice.paidamount = reader["paidamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["paidamount"]);
                        oagentpaymentinvoice.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        oagentpaymentinvoice.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        oagentpaymentinvoice.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        oagentpaymentinvoice.supplierpaymentinvoiceid = reader["supplierpaymentinvoiceid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["supplierpaymentinvoiceid"]);
                        oagentpaymentinvoice.supplierpaymentid = reader["supplierpaymentid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["supplierpaymentid"]);
                        oagentpaymentinvoice.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        oagentpaymentinvoice.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        oagentpaymentinvoice.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        oagentpaymentinvoice.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        oagentpaymentinvoice.agentpaymentid = reader["agentpaymentid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["agentpaymentid"]);
                        oagentpaymentinvoice.isinterstatesale = reader["isinterstatesale"] == DBNull.Value ? false : Convert.ToBoolean(reader["isinterstatesale"]);
                        oagentpaymentinvoice.agentcommissionvalue = reader["agentcommissionvalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["agentcommissionvalue"]);
                        oagentpaymentinvoice.agentcommission = reader["agentcommission"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["agentcommission"]);
                        oagentpaymentinvoice.taxamount = reader["taxamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["taxamount"]);
                        oagentpaymentinvoice.taxledgerid = reader["taxledgerid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["taxledgerid"]);
                        oagentpaymentinvoice.invoiceno = reader["invoiceno"] == DBNull.Value ? "" : Convert.ToString(reader["invoiceno"]);
                        oagentpaymentinvoice.receiptno = reader["receiptno"] == DBNull.Value ? "" : Convert.ToString(reader["receiptno"]);
                        oagentpaymentinvoice.invoicedate = reader["invoicedate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["invoicedate"]);
                        oagentpaymentinvoice.supplierpaiddate = reader["supplierpaiddate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["supplierpaiddate"]);
                        oagentpaymentinvoice.supplierid = reader["supplierid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["supplierid"]);
                        oagentpaymentinvoice.invpaidamount = reader["paidamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["paidamount"]);
                        oagentpaymentinvoice.invdisount = reader["discountamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discountamount"]);

                        lst.Add(oagentpaymentinvoice);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _agentpaymentid);
            }
            return lst;
        }

        public Agentpaymentinvoice Insert(Agentpaymentinvoice oAgentpaymentinvoice)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);

                    Insert(transaction, conn, oAgentpaymentinvoice);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oAgentpaymentinvoice);
            }

            return oAgentpaymentinvoice;
        }

        public Agentpaymentinvoice Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Agentpaymentinvoice oAgentpaymentinvoice)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("discountamountpercent", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oAgentpaymentinvoice.discountamountpercent;
            command.Parameters.Add("discountamount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oAgentpaymentinvoice.discountamount;
            command.Parameters.Add("paidamount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oAgentpaymentinvoice.paidamount;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oAgentpaymentinvoice.createdon;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oAgentpaymentinvoice.isactive;
            command.Parameters.Add("supplierpaymentinvoiceid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oAgentpaymentinvoice.supplierpaymentinvoiceid;
            command.Parameters.Add("supplierpaymentid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oAgentpaymentinvoice.supplierpaymentid;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oAgentpaymentinvoice.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oAgentpaymentinvoice.modifiedby;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oAgentpaymentinvoice.version;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oAgentpaymentinvoice.modifiedon;
            command.Parameters.Add("agentpaymentid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oAgentpaymentinvoice.agentpaymentid;

            command.Parameters.Add("invpaidamount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oAgentpaymentinvoice.invpaidamount;
            command.Parameters.Add("invdisount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oAgentpaymentinvoice.invdisount;

            command.Parameters.Add("agentcommissionvalue", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oAgentpaymentinvoice.agentcommissionvalue;
            command.Parameters.Add("agentcommission", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oAgentpaymentinvoice.agentcommission;
            command.Parameters.Add("taxamount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oAgentpaymentinvoice.taxamount;
            command.Parameters.Add("isinterstatesale", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oAgentpaymentinvoice.isinterstatesale;
            command.Parameters.Add("taxledgerid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oAgentpaymentinvoice.taxledgerid;
            command.Parameters.Add("invoiceno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = string.IsNullOrEmpty(oAgentpaymentinvoice.invoiceno) ? "" : oAgentpaymentinvoice.invoiceno;
            command.Parameters.Add("receiptno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = string.IsNullOrEmpty(oAgentpaymentinvoice.receiptno) ? "" : oAgentpaymentinvoice.receiptno;
            command.Parameters.Add("supplierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oAgentpaymentinvoice.supplierid;
            command.Parameters.Add("supplierpaiddate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oAgentpaymentinvoice.supplierpaiddate;
            command.Parameters.Add("invoicedate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oAgentpaymentinvoice.invoicedate;

            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oAgentpaymentinvoice.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            return oAgentpaymentinvoice;
        }

        public Agentpaymentinvoice Update(Agentpaymentinvoice oAgentpaymentinvoice)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oAgentpaymentinvoice);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oAgentpaymentinvoice);
            }

            return oAgentpaymentinvoice;
        }

        public Agentpaymentinvoice Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Agentpaymentinvoice oAgentpaymentinvoice)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("discountamountpercent", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oAgentpaymentinvoice.discountamountpercent;
            command.Parameters.Add("discountamount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oAgentpaymentinvoice.discountamount;
            command.Parameters.Add("paidamount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oAgentpaymentinvoice.paidamount;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oAgentpaymentinvoice.id;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oAgentpaymentinvoice.createdon;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oAgentpaymentinvoice.isactive;
            command.Parameters.Add("supplierpaymentinvoiceid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oAgentpaymentinvoice.supplierpaymentinvoiceid;
            command.Parameters.Add("supplierpaymentid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oAgentpaymentinvoice.supplierpaymentid;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oAgentpaymentinvoice.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oAgentpaymentinvoice.modifiedby;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oAgentpaymentinvoice.version;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oAgentpaymentinvoice.modifiedon;
            command.Parameters.Add("agentpaymentid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oAgentpaymentinvoice.agentpaymentid;
            command.Parameters.Add("agentcommissionvalue", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oAgentpaymentinvoice.agentcommissionvalue;
            command.Parameters.Add("agentcommission", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oAgentpaymentinvoice.agentcommission;
            command.Parameters.Add("taxamount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oAgentpaymentinvoice.taxamount;
            command.Parameters.Add("isinterstatesale", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oAgentpaymentinvoice.isinterstatesale;
            command.Parameters.Add("taxledgerid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oAgentpaymentinvoice.taxledgerid;
            command.Parameters.Add("invoiceno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = string.IsNullOrEmpty(oAgentpaymentinvoice.invoiceno) ? "" : oAgentpaymentinvoice.invoiceno;
            command.Parameters.Add("receiptno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = string.IsNullOrEmpty(oAgentpaymentinvoice.receiptno) ? "" : oAgentpaymentinvoice.receiptno;
            command.Parameters.Add("supplierpaiddate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oAgentpaymentinvoice.supplierpaiddate;
            command.Parameters.Add("invoicedate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oAgentpaymentinvoice.invoicedate;
            command.Parameters.Add("supplierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oAgentpaymentinvoice.supplierid;
            command.Parameters.Add("invpaidamount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oAgentpaymentinvoice.invpaidamount;
            command.Parameters.Add("invdisount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oAgentpaymentinvoice.invdisount;

            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oAgentpaymentinvoice.version = Convert.ToInt32(reader[0].ToString());
                }
            }

            return oAgentpaymentinvoice;
        }

        public bool Update(NpgsqlTransaction transaction, NpgsqlConnection conn, List<Agentpaymentinvoice> list, long id)
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
                    item.modifiedby = Context.UserID;
                    item.modifiedon = DateTime.Now;
                    Update(transaction, conn, item);
                }
                else
                {
                    item.agentpaymentid = id;
                    item.isactive = true;
                    item.createdby = Context.UserID;
                    item.modifiedby = Context.UserID;
                    item.createdon = DateTime.Now;
                    item.modifiedon = DateTime.Now;
                    Insert(transaction, conn, item);
                }
            }
            return true;
        }

        public bool Delete(Agentpaymentinvoice oAgentpaymentinvoice)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oAgentpaymentinvoice);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oAgentpaymentinvoice);
            }
            return hasSuccessCount;

        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Agentpaymentinvoice oAgentpaymentinvoice)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oAgentpaymentinvoice.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oAgentpaymentinvoice.modifiedby;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.TimestampTZ).Value = oAgentpaymentinvoice.modifiedon;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oAgentpaymentinvoice.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oAgentpaymentinvoice.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

        public bool DeleteAll(long agentpaymentid, long userid, NpgsqlConnection conn, NpgsqlTransaction transaction)
        {


            int successCount = 0;
            NpgsqlCommand command = new NpgsqlCommand(SQL_DeleteAll, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = false;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = userid;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.TimestampTZ).Value = DateTime.Now;
            command.Parameters.Add("agentpaymentid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = agentpaymentid;
            successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

    }

}
