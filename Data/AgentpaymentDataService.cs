using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
namespace InfyPOS.Data

{
    public class AgentpaymentDataService : BaseData
    {
        const string SQL_Search = @"select locationid, tdsamount, rounding, id, companyid, version, paidamount, createdon, modifiedby, modifiedon, 
        datatransmitted, paymentdate, bankid, agentid, createdby, isactive, paymentmode, instrumentdate, sourcemode, hastds, tdspercentage, 
        referencedetail, remarks, receiptno from agentpayment 
        where id=@id";

        const string removecommissionpaid = @"update supplierpaymentinvoice set agentcommissionpaid=agentcommissionpaid-agentpaymentinvoice.paidamount 
                    from agentpaymentinvoice 
                    where agentpaymentinvoice.agentpaymentid = @agentpaymentid and
                    supplierpaymentinvoice.id = agentpaymentinvoice.supplierpaymentinvoiceid";

        const string updatecommissionpaid = @"update supplierpaymentinvoice set agentcommissionpaid=agentcommissionpaid+agentpaymentinvoice.paidamount 
                    from agentpaymentinvoice 
                    where agentpaymentinvoice.agentpaymentid = @agentpaymentid and
                    supplierpaymentinvoice.id = agentpaymentinvoice.supplierpaymentinvoiceid";


        const string SQL_Insert = @"INSERT INTO public.agentpayment(locationid, tdsamount, rounding, companyid, version, paidamount, createdon, modifiedby, modifiedon, datatransmitted, paymentdate, bankid, agentid, createdby, isactive, paymentmode, instrumentdate, sourcemode, hastds, tdspercentage, referencedetail, remarks, receiptno) VALUES(@locationid, @tdsamount, @rounding, @companyid, @version, @paidamount, @createdon, @modifiedby, @modifiedon, @datatransmitted, @paymentdate, @bankid, @agentid, @createdby, @isactive, @paymentmode, @instrumentdate, @sourcemode, @hastds, @tdspercentage, @referencedetail, @remarks, @receiptno) RETURNING id";


        const string SQL_Update = @"UPDATE public.agentpayment SET locationid=@locationid, tdsamount=@tdsamount, rounding=@rounding, companyid=@companyid, version=@version+1, paidamount=@paidamount, modifiedby=@modifiedby, modifiedon=@modifiedon, datatransmitted=@datatransmitted, paymentdate=@paymentdate, bankid=@bankid, agentid=@agentid, paymentmode=@paymentmode, instrumentdate=@instrumentdate, sourcemode=@sourcemode, hastds=@hastds, tdspercentage=@tdspercentage, referencedetail=@referencedetail, remarks=@remarks, receiptno=@receiptno where id=@id and version=@version RETURNING version";

        const string SQL_Delete = @"update public.agentpayment set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";

        public List<Agentpayment> Search(AgentpaymentCreteria _agentpayment)
        {
            List<Agentpayment> lst = new List<Agentpayment>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_agentpayment.id != 0)
                {
                    command = new NpgsqlCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _agentpayment.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("isactive", true, "=");

                    //if (!string.IsNullOrEmpty(_agentpayment.Code))
                    //    qb.AddParameter("code", _agentpayment.Code, "=");

                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Agentpayment oagentpayment = new Agentpayment();
                        oagentpayment.locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                        oagentpayment.tdsamount = reader["tdsamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["tdsamount"]);
                        oagentpayment.rounding = reader["rounding"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["rounding"]);
                        oagentpayment.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        oagentpayment.companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"]);
                        oagentpayment.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        oagentpayment.paidamount = reader["paidamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["paidamount"]);
                        oagentpayment.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        oagentpayment.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        oagentpayment.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        oagentpayment.datatransmitted = reader["datatransmitted"] == DBNull.Value ? false : Convert.ToBoolean(reader["datatransmitted"]);
                        oagentpayment.paymentdate = reader["paymentdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["paymentdate"]);
                        oagentpayment.bankid = reader["bankid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["bankid"]);
                        oagentpayment.agentid = reader["agentid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["agentid"]);
                        oagentpayment.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        oagentpayment.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        oagentpayment.paymentmode = reader["paymentmode"] == DBNull.Value ? 0 : Convert.ToInt32(reader["paymentmode"]);
                        oagentpayment.instrumentdate = reader["instrumentdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["instrumentdate"]);
                        oagentpayment.sourcemode = reader["sourcemode"] == DBNull.Value ? 0 : Convert.ToInt32(reader["sourcemode"]);
                        oagentpayment.hastds = reader["hastds"] == DBNull.Value ? false : Convert.ToBoolean(reader["hastds"]);
                        oagentpayment.tdspercentage = reader["tdspercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["tdspercentage"]);
                        oagentpayment.referencedetail = reader["referencedetail"] == DBNull.Value ? "" : reader["referencedetail"].ToString();
                        oagentpayment.remarks = reader["remarks"] == DBNull.Value ? "" : reader["remarks"].ToString();
                        oagentpayment.receiptno = reader["receiptno"] == DBNull.Value ? "" : reader["receiptno"].ToString();
                        lst.Add(oagentpayment);
                    }
                }
                if (_agentpayment.id > 0 && lst.Count == 1)
                {
                    AgentpaymentinvoiceDataService subAgentpaymentinvoice = new AgentpaymentinvoiceDataService() { Context = this.Context };
                    lst[0].Agentpaymentinvoice = subAgentpaymentinvoice.SearchAll(_agentpayment.id);
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _agentpayment);
            }
            return lst;
        }

        public Agentpayment Insert(Agentpayment oAgentpayment)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                var sequence = SequenceManager.Instance(Context.OrganizationID).Get("agentpayment.receiptno", 0);
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);
                    oAgentpayment.receiptno = sequence.GetNextNumber(Context, conn, transaction);

                    Insert(transaction, conn, oAgentpayment);
                    AgentpaymentinvoiceDataService subAgentpaymentinvoice = new AgentpaymentinvoiceDataService() { Context = this.Context };
                    foreach (var item in oAgentpayment.Agentpaymentinvoice)
                    {
                        item.agentpaymentid = oAgentpayment.id;
                        item.isactive = true;
                        item.createdby = Context.UserID;
                        item.modifiedby = Context.UserID;
                        item.createdon = DateTime.Now;
                        item.modifiedon = DateTime.Now;
                        subAgentpaymentinvoice.Insert(transaction, conn, item);
                    }

                    AgentDataService suppliercompanyDataService = new AgentDataService() { Context = this.Context };
                    var supplieraccounts = suppliercompanyDataService.Search(new AgentCreteria()
                    {
                        id = oAgentpayment.agentid
                    });

                    if (supplieraccounts.Count == 1)
                    {

                        var paymentinfo = new TransportPayment.PaymentInfo();
                        paymentinfo.bankacno = supplieraccounts.First().bankaccountno;
                        paymentinfo.bankacname = supplieraccounts.First().bankaccountname;
                        paymentinfo.bankifsccode = supplieraccounts.First().bankifci;
                        paymentinfo.bankname = supplieraccounts.First().bankname;
                        paymentinfo.bankbranch = supplieraccounts.First().bankbranch;
                        foreach (var cheque in oAgentpayment.bankcheques)
                        {
                            cheque.entrydate = oAgentpayment.paymentdate;
                            cheque.locationid = oAgentpayment.locationid;
                            if(!string.IsNullOrEmpty(paymentinfo.bankacname))
                                cheque.accountname = paymentinfo.bankacname;
                            else if(string.IsNullOrEmpty(cheque.accountname))
                                cheque.accountname = supplieraccounts.First().name;
                            cheque.bankbranch = paymentinfo.bankbranch;
                            cheque.bankaccountno = paymentinfo.bankacno;
                            cheque.bankifsc = paymentinfo.bankifsccode;
                            cheque.referenceno = oAgentpayment.receiptno;
                            cheque.ledgername = supplieraccounts.First().name;
                        }
                    }

                    BankchequetransactionDataService bankchequetransactionDataService = new BankchequetransactionDataService() { Context = this.Context };
                    foreach (var item in oAgentpayment.bankcheques)
                    {
                        item.sourceid = oAgentpayment.id;
                        item.sourcetype = "APMT";
                        item.version = 1;
                        item.isactive = true;
                        item.createdby = this.Context.UserID;
                        item.modifiedby = this.Context.UserID;
                        item.createdon = DateTimeNow;
                        item.modifiedon = DateTimeNow;
                        item.status = 0;
                        bankchequetransactionDataService.Insert(transaction, conn, item);
                    }

                    var param = new SortedList<string, object>();
                    param.Add("agentpaymentid", oAgentpayment.id);
                    ExecuteQuery(updatecommissionpaid, param, conn, transaction);

                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oAgentpayment);

            }

            return oAgentpayment;
        }

        public Agentpayment Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Agentpayment oAgentpayment)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oAgentpayment.locationid;
            command.Parameters.Add("tdsamount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oAgentpayment.tdsamount;
            command.Parameters.Add("rounding", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oAgentpayment.rounding;
            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oAgentpayment.companyid;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oAgentpayment.version;
            command.Parameters.Add("paidamount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oAgentpayment.paidamount;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oAgentpayment.createdon;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oAgentpayment.modifiedby;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oAgentpayment.modifiedon;
            command.Parameters.Add("datatransmitted", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oAgentpayment.datatransmitted;
            command.Parameters.Add("paymentdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oAgentpayment.paymentdate;
            command.Parameters.Add("bankid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oAgentpayment.bankid;
            command.Parameters.Add("agentid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oAgentpayment.agentid;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oAgentpayment.createdby;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oAgentpayment.isactive;
            command.Parameters.Add("paymentmode", NpgsqlTypes.NpgsqlDbType.Integer).Value = oAgentpayment.paymentmode;
            command.Parameters.Add("instrumentdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oAgentpayment.instrumentdate;
            command.Parameters.Add("sourcemode", NpgsqlTypes.NpgsqlDbType.Integer).Value = oAgentpayment.sourcemode;
            command.Parameters.Add("hastds", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oAgentpayment.hastds;
            command.Parameters.Add("tdspercentage", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oAgentpayment.tdspercentage;
            command.Parameters.Add("referencedetail", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oAgentpayment.referencedetail == null ? "" : oAgentpayment.referencedetail;
            command.Parameters.Add("remarks", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oAgentpayment.remarks == null ? "" : oAgentpayment.remarks;
            command.Parameters.Add("receiptno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oAgentpayment.receiptno == null ? "" : oAgentpayment.receiptno;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oAgentpayment.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            return oAgentpayment;
        }

        public Agentpayment Update(Agentpayment oAgentpayment)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oAgentpayment);

                    var param = new SortedList<string, object>();
                    param.Add("agentpaymentid", oAgentpayment.id);
                    ExecuteReader(removecommissionpaid, param, conn, transaction);

                    AgentpaymentinvoiceDataService subAgentpaymentinvoice = new AgentpaymentinvoiceDataService() { Context = this.Context };
                    subAgentpaymentinvoice.Update(transaction, conn, oAgentpayment.Agentpaymentinvoice, oAgentpayment.id);

                    BankchequetransactionDataService bankchequetransactionDataService = new BankchequetransactionDataService() { Context = this.Context };

                    TransportDataService suppliercompanyDataService = new TransportDataService() { Context = this.Context };
                    var supplieraccounts = suppliercompanyDataService.Search(new TransportCreteria()
                    {
                        id = oAgentpayment.agentid
                    });

                    if (supplieraccounts.Count == 1)
                    {

                        var paymentinfo = new TransportPayment.PaymentInfo();
                        paymentinfo.bankacno = supplieraccounts.First().bankaccountno;
                        paymentinfo.bankacname = supplieraccounts.First().bankaccountname;
                        paymentinfo.bankifsccode = supplieraccounts.First().bankifci;
                        paymentinfo.bankname = supplieraccounts.First().bankname;
                        paymentinfo.bankbranch = supplieraccounts.First().bankbranch;
                        foreach (var cheque in oAgentpayment.bankcheques)
                        {
                            cheque.entrydate = oAgentpayment.paymentdate;
                            cheque.locationid = oAgentpayment.locationid;
                            cheque.accountname = oAgentpayment.paymentinfo.bankacname;
                            cheque.bankbranch = oAgentpayment.paymentinfo.bankbranch;
                            cheque.bankaccountno = oAgentpayment.paymentinfo.bankacno;
                            cheque.bankifsc = oAgentpayment.paymentinfo.bankifsccode;
                            cheque.referenceno = oAgentpayment.receiptno;
                            cheque.ledgername = supplieraccounts.First().name;
                            cheque.accountname = oAgentpayment.paymentinfo.bankacname;
                        }
                    }
                    bankchequetransactionDataService.Update(transaction, conn, oAgentpayment.bankcheques, oAgentpayment.id, "APMT");
                    param.Clear();
                    param.Add("agentpaymentid", oAgentpayment.id);
                    ExecuteReader(updatecommissionpaid, param, conn, transaction);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oAgentpayment);

            }

            return oAgentpayment;
        }

        public Agentpayment Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Agentpayment oAgentpayment)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oAgentpayment.locationid;
            command.Parameters.Add("tdsamount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oAgentpayment.tdsamount;
            command.Parameters.Add("rounding", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oAgentpayment.rounding;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oAgentpayment.id;
            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oAgentpayment.companyid;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oAgentpayment.version;
            command.Parameters.Add("paidamount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oAgentpayment.paidamount;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oAgentpayment.createdon;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oAgentpayment.modifiedby;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oAgentpayment.modifiedon;
            command.Parameters.Add("datatransmitted", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oAgentpayment.datatransmitted;
            command.Parameters.Add("paymentdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oAgentpayment.paymentdate;
            command.Parameters.Add("bankid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oAgentpayment.bankid;
            command.Parameters.Add("agentid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oAgentpayment.agentid;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oAgentpayment.createdby;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oAgentpayment.isactive;
            command.Parameters.Add("paymentmode", NpgsqlTypes.NpgsqlDbType.Integer).Value = oAgentpayment.paymentmode;
            command.Parameters.Add("instrumentdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oAgentpayment.instrumentdate;
            command.Parameters.Add("sourcemode", NpgsqlTypes.NpgsqlDbType.Integer).Value = oAgentpayment.sourcemode;
            command.Parameters.Add("hastds", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oAgentpayment.hastds;
            command.Parameters.Add("tdspercentage", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oAgentpayment.tdspercentage;
            command.Parameters.Add("referencedetail", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oAgentpayment.referencedetail == null ? "" : oAgentpayment.referencedetail;
            command.Parameters.Add("remarks", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oAgentpayment.remarks == null ? "" : oAgentpayment.remarks;
            command.Parameters.Add("receiptno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oAgentpayment.receiptno == null ? "" : oAgentpayment.receiptno;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oAgentpayment.version = Convert.ToInt32(reader[0].ToString());
                }
            }

            return oAgentpayment;
        }


        internal List<AgentpaymentinvoiceDetail> SearchPendingPayment(AgentpaymentCreteria _supplierpayment)
        {
            LedgerDataService ledgerDataService = new LedgerDataService() { Context = this.Context };
            List<AgentpaymentinvoiceDetail> lst = new List<AgentpaymentinvoiceDetail>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                QueryBuilder qb = new QueryBuilder();
                qb.CurrentQuery = @"
                select invoiceno,invoiceid,supplierpayment.supplierid,
                invoicedate,agentcommissionvalue,agentcommission,agentcommissionpaid,
                supplierpaymentid,supplierpayment.paymentdate,supplierpayment.receiptno,
                supplierpaymentinvoice.agentid,agent.gst gstno,agent.taxledgerid,
                agent.name agentname,supplierpayment.locationid,
                supplierpaymentinvoice.paidamount,supplierpaymentinvoice.discountamount
                from supplierpaymentinvoice
                inner join supplierpayment on supplierpayment.id = supplierpaymentid
                inner join agent on agent.id = supplierpaymentinvoice.agentid
                where supplierpaymentinvoice.isactive=true and supplierpaymentinvoice.hascancel=false 
                and supplierpayment.isactive=true and supplierpayment.hascancel=false 
                and (agentcommissionvalue-agentcommissionpaid)>0 and 
                supplierpayment.paymentdate >=@fromdate and supplierpayment.paymentdate<@todate and
                supplierpayment.companyid=@companyid";
                if (_supplierpayment.agentid > 0)
                {
                    qb.AddParameter("supplierpaymentinvoice.agentid", _supplierpayment.agentid, "=");
                }
                var command = qb.GetCommand(conn);
                command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _supplierpayment.companyid;
                command.Parameters.Add("fromdate", NpgsqlTypes.NpgsqlDbType.TimestampTZ).Value = _supplierpayment.fromdate.Date;
                command.Parameters.Add("todate", NpgsqlTypes.NpgsqlDbType.TimestampTZ).Value = _supplierpayment.todate.Date.AddDays(1);
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        AgentpaymentinvoiceDetail oinvoice = new AgentpaymentinvoiceDetail();
                        oinvoice.locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                        oinvoice.supplierid = reader["supplierid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["supplierid"]);
                        oinvoice.supplierpaymentinvoiceid = reader["invoiceid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["invoiceid"]);
                        oinvoice.supplierpaymentid = reader["supplierpaymentid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["supplierpaymentid"]);
                        oinvoice.invoiceno = reader["invoiceno"] == DBNull.Value ? "" : reader["invoiceno"].ToString();
                        oinvoice.receiptno = reader["receiptno"] == DBNull.Value ? "" : reader["receiptno"].ToString();
                        oinvoice.agentname = reader["agentname"] == DBNull.Value ? "" : reader["agentname"].ToString();
                        oinvoice.gstno = reader["gstno"] == DBNull.Value ? "" : reader["gstno"].ToString();
                        oinvoice.agentid = reader["agentid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["agentid"]);
                        oinvoice.taxledgerid = reader["taxledgerid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["taxledgerid"]);
                        oinvoice.agentcommissionvalue = reader["agentcommissionvalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["agentcommissionvalue"]);
                        oinvoice.agentcommission = reader["agentcommission"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["agentcommission"]);
                        oinvoice.invpaidamount = reader["paidamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["paidamount"]);
                        oinvoice.invdisount = reader["discountamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discountamount"]);
                        oinvoice.paidamount = reader["agentcommissionpaid"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["agentcommissionpaid"]);
                        oinvoice.invoicedate = reader["invoicedate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["invoicedate"]);
                        oinvoice.supplierpaiddate = reader["paymentdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["paymentdate"]);
                        lst.Add(oinvoice);
                    }
                }

                if (!string.IsNullOrEmpty(_supplierpayment.companygst))
                {
                    var companystatecode = _supplierpayment.companygst.Substring(0, 2);
                    foreach (var grp in lst.GroupBy(x => x.taxledgerid))
                    {
                        if (grp.Key > 0)
                        {
                            var expledger = ledgerDataService.Search(new LedgerCreteria()
                            {
                                id = grp.Key
                            });
                            if (expledger.Count > 0 &&
                                expledger[0].attributes != null &&
                                expledger[0].attributes.hasgst)
                            {
                                List<LedgerDetail> igst = null;
                                List<LedgerDetail> cgst = null;
                                List<LedgerDetail> sgst = null;
                                decimal igsttax = 0;
                                decimal cgsttax = 0;
                                decimal sgsttax = 0;
                                if (expledger[0].attributes.ledgerigstid > 0)
                                {
                                    igst = ledgerDataService.Search(new LedgerCreteria()
                                    {
                                        id = expledger[0].attributes.ledgerigstid
                                    });
                                    if (igst != null && igst.Count > 0)
                                    {
                                        igsttax = igst[0].taxpercentage;
                                    }
                                }
                                if (expledger[0].attributes.ledgersgstid > 0)
                                {
                                    sgst = ledgerDataService.Search(new LedgerCreteria()
                                    {
                                        id = expledger[0].attributes.ledgersgstid
                                    });
                                    if (sgst != null && sgst.Count > 0)
                                    {
                                        sgsttax = sgst[0].taxpercentage;
                                    }
                                }
                                if (expledger[0].attributes.ledgercgstid > 0)
                                {
                                    cgst = ledgerDataService.Search(new LedgerCreteria()
                                    {
                                        id = expledger[0].attributes.ledgercgstid
                                    });
                                    if (cgst != null && cgst.Count > 0)
                                    {
                                        cgsttax = cgst[0].taxpercentage;
                                    }
                                }

                                foreach (var item in grp)
                                {
                                    var balance = item.agentcommissionvalue - item.paidamount;
                                    if (!string.IsNullOrEmpty(item.gstno) &&
                                       item.gstno.Substring(0, 2) != companystatecode)
                                    {
                                        item.taxamount = Math.Round(igsttax * balance / 100, 2);
                                        item.isinterstatesale = true;
                                    }
                                    else
                                    {
                                        item.isinterstatesale = false;
                                        item.taxamount += Math.Round(sgsttax * balance / 100, 2);
                                        item.taxamount += Math.Round(cgsttax * balance / 100, 2);
                                    }
                                }
                            }
                        }
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _supplierpayment);
            }
            return lst;

        }

        //public List<PendingLrInvoice> SearchSupplierPaymentSummary(TransportPaymentCreteria _supplierpayment)
        //{
        //    List<PendingTransportInvoice> lst = SearchPendingPayment(_supplierpayment);
        //    return lst.GroupBy(e => e.transportname).ToList().ConvertAll(e =>
        //    {
        //        return new PendingLrInvoice()
        //        {
        //            transport = e.Key,
        //            invoicecount = e.Count(),
        //            totalamount = e.Sum(x => x.totalamount),
        //            paidamount = e.Sum(x => x.paidamount),
        //            balanceamount = e.Sum(x => x.balanceamount)
        //        };
        //    });
        //}
        public bool Delete(Agentpayment oAgentpayment)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oAgentpayment);

                    var param = new SortedList<string, object>();
                    param.Add("agentpaymentid", oAgentpayment.id);
                    ExecuteReader(removecommissionpaid, param, conn, transaction);
                    AgentpaymentinvoiceDataService subAgentpaymentinvoice = new AgentpaymentinvoiceDataService();
                    subAgentpaymentinvoice.DeleteAll(oAgentpayment.id, oAgentpayment.modifiedby, conn, transaction);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oAgentpayment);
            }
            return hasSuccessCount;

        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Agentpayment oAgentpayment)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oAgentpayment.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oAgentpayment.modifiedby;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.TimestampTZ).Value = oAgentpayment.modifiedon;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oAgentpayment.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oAgentpayment.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

    }

}
