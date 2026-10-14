using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
namespace InfyPOS.Data

{
    public class CustomerinvoiceDataService : SAASData
    {
        const string SQL_Search = @"select customerinvoice.id, customerinvoice.isactive, customerinvoice.createdby, customerinvoice.modifiedby, customerinvoice.version, 
customerinvoice.createdon, customerinvoice.modifiedon, type, customerinvoice.receivable, customerinvoice.paid, customerinvoice.transactiondate, billid, 
customerinvoice.customerid, customerinvoice.paymentid, customerinvoice.receiptno, customerinvoice.billno, approvedby,customer.name customername,
customerinvoice.companyid,customerinvoice.discount,customerinvoice.advance,customerinvoice.paidcustomerinvoiceid,customerinvoice.orderid, bill.billdate 
from customerinvoice 
left outer join bill on bill.id = customerinvoice.billid and bill.billtype != 9 
left outer join customer on customer.id = customerinvoice.customerid
where customerinvoice.isactive = true and customerinvoice.id=@id";

        const string SQL_Reminder = @"select customer.id, customer.mobileno, customer.name, sum(customerinvoice.receivable-customerinvoice.paid) balance
from customerinvoice inner join bill on bill.id = customerinvoice.billid and bill.billtype != 9  
inner join customer on customer.id = customerinvoice.customerid
Where customerinvoice.isactive=true And transactiondate<@fromdate 
And customerinvoice.type in (1,2) and customerinvoice.receivable-customerinvoice.paid>0  
group by customer.id, customer.mobileno, customer.name";

        const string SQL_SearchAll = @"select customerinvoice.id, customerinvoice.isactive, customerinvoice.createdby, 
        customerinvoice.modifiedby, customerinvoice.version, customerinvoice.createdon, customerinvoice.modifiedon, customerinvoice.type,
        customerinvoice.receivable, customerinvoice.paid, customerinvoice.transactiondate, customerinvoice.billid, customerinvoice.customerid, 
        customerinvoice.paymentid, customerinvoice.receiptno, customerinvoice.billno, bill.billdate, 
        case when actualinvoice.id is not null then actualinvoice.receivable-actualinvoice.paid else 0 end balance,
        customerinvoice.approvedby,customerinvoice.companyid,customerinvoice.discount,customerinvoice.advance,
        customerinvoice.paidcustomerinvoiceid from customerinvoice 
        left outer join customerinvoice actualinvoice on actualinvoice.id = customerinvoice.paidcustomerinvoiceid
        left outer join bill on bill.id = customerinvoice.billid
        where customerinvoice.isactive = true and customerinvoice.paymentid=@paymentid";

        const string SQL_Insert = @"INSERT INTO public.customerinvoice(isactive, createdby, modifiedby, version, createdon, modifiedon, type, receivable, paid, transactiondate, billid, customerid, paymentid, receiptno, billno, approvedby,companyid,discount,advance,paidcustomerinvoiceid,orderid) 
                                    VALUES(@isactive, @createdby, @modifiedby, @version, @createdon, @modifiedon, @type, @receivable, @paid, @transactiondate, @billid, @customerid, @paymentid, @receiptno, @billno, @approvedby,@companyid,@discount,@advance,@paidcustomerinvoiceid,@orderid) RETURNING id";

        const string SQL_Update = @"UPDATE public.customerinvoice SET modifiedby=@modifiedby, version=@version+1, modifiedon=@modifiedon, type=@type, receivable=@receivable, paid=@paid,companyid=@companyid,discount=@discount,advance=@advance,
                                    transactiondate=@transactiondate, billid=@billid, customerid=@customerid, paymentid=@paymentid, receiptno=@receiptno, billno=@billno, approvedby=@approvedby, orderid=@orderid where id=@id and version=@version RETURNING version";

        const string SQL_Delete = @"update public.customerinvoice set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version and billid=0";

        const string SQL_DeleteAll = @"
        update customerinvoice set discount=customerinvoice.discount-paidrow.discount,paid=customerinvoice.paid-paidrow.paid from customerinvoice paidrow
        where customerinvoice.id = paidrow.paidcustomerinvoiceid and paidrow.paymentid = @paymentid;

        update public.customerinvoice set isactive=false,modifiedby=@modifiedby,modifiedon=@modifiedon where paymentid=@paymentid";
        public List<Customerinvoice> Search(CustomerinvoiceCreteria _customerinvoice)
        {
            List<Customerinvoice> lst = new List<Customerinvoice>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_customerinvoice.id != 0)
                {
                    command = GetCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _customerinvoice.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("customerinvoice.isactive", true, "=");

                    if (_customerinvoice.orderid > 0)
                        qb.AddParameter("customerinvoice.orderid", _customerinvoice.orderid, "=");
                    if (_customerinvoice.billid > 0)
                        qb.AddParameter("customerinvoice.billid", _customerinvoice.billid, "=");

                    if (_customerinvoice.companyid > 0)
                        qb.AddParameter("customerinvoice.companyid", _customerinvoice.companyid, "=");

                    if (_customerinvoice.customerid > 0)
                        qb.AddParameter("customerinvoice.customerid", _customerinvoice.customerid, "=");

                    if(!string.IsNullOrEmpty(_customerinvoice.billno))
                        qb.AddParameter("customerinvoice.billno", _customerinvoice.billno, "=");

                    if (_customerinvoice.locationid > 0)
                        qb.AddParameter("bill.locationid", _customerinvoice.locationid, "=");

                    if (_customerinvoice.ids != null)
                    {
                        qb.AddInParamer("customerinvoice.id", string.Join<long>(",", _customerinvoice.ids));
                    }

                    if (_customerinvoice.fromdate != DateTime.MinValue && _customerinvoice.todate != DateTime.MinValue)
                    {
                        qb.AddParameter("transactiondate", "datefrom", _customerinvoice.fromdate.Date, ">=");
                        qb.AddParameter("transactiondate", "dateto", _customerinvoice.todate.AddDays(1).Date, "<");
                    }
                    if (_customerinvoice.showpending)
                    {
                        qb.Add2Query("customerinvoice.type in (1,2,5,6)");
                        qb.CurrentQuery += " and (round(customerinvoice.receivable::numeric,0)-(customerinvoice.paid+customerinvoice.discount)::numeric)!=0::numeric ";
                    }
                    else
                    {
                        if (_customerinvoice.orderid == 0)
                            qb.AddParameter("bill.isactive", true, "=");
                    }
                    qb.SortField = "transactiondate ";
                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Customerinvoice ocustomerinvoice = new Customerinvoice();
                        ocustomerinvoice.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        ocustomerinvoice.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        ocustomerinvoice.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        ocustomerinvoice.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        ocustomerinvoice.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        ocustomerinvoice.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        ocustomerinvoice.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        ocustomerinvoice.type = reader["type"] == DBNull.Value ? 0 : Convert.ToInt32(reader["type"]);
                        ocustomerinvoice.receivable = reader["receivable"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["receivable"]);
                        ocustomerinvoice.paid = reader["paid"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["paid"]);
                        ocustomerinvoice.advance = reader["advance"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["advance"]);
                        ocustomerinvoice.discount = reader["discount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discount"]);
                        ocustomerinvoice.transactiondate = reader["transactiondate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["transactiondate"]);
                        ocustomerinvoice.billid = reader["billid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["billid"]);
                        ocustomerinvoice.billdate = reader["billdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["billdate"]);
                        ocustomerinvoice.customerid = reader["customerid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["customerid"]);
                        ocustomerinvoice.paymentid = reader["paymentid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["paymentid"]);
                        ocustomerinvoice.receiptno = reader["receiptno"] == DBNull.Value ? "" : reader["receiptno"].ToString();
                        ocustomerinvoice.billno = reader["billno"] == DBNull.Value ? "" : reader["billno"].ToString();
                        ocustomerinvoice.customername = reader["customername"] == DBNull.Value ? "" : reader["customername"].ToString();
                        ocustomerinvoice.approvedby = reader["approvedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["approvedby"]);
                        ocustomerinvoice.companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"]);
                        ocustomerinvoice.paidcustomerinvoiceid = reader["paidcustomerinvoiceid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["paidcustomerinvoiceid"]);
                        ocustomerinvoice.orderid = reader["orderid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["orderid"]);

                        if(Math.Abs(ocustomerinvoice.receivable-(ocustomerinvoice.paid+ ocustomerinvoice.discount))>0)
                            lst.Add(ocustomerinvoice);
                    }
                }

                if (lst.Count > 0)
                {
                    var query = @"Select sum(tax) tax,billid from billitems 
                where billid in (" + string.Join(",", lst.ConvertAll(x => x.billid)) + ") and isactive=true " +
                    "group by billid";
                    using (var reader = ExecuteReader(query, null))
                    {
                        while (reader.Read())
                        {
                            long billid = reader["billid"] != DBNull.Value ? Convert.ToInt64(reader["billid"]) : 0;
                            lst.Find(x => x.billid == billid).tax = reader["tax"] != DBNull.Value ? Convert.ToDecimal(reader["tax"]) : 0;
                        }
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _customerinvoice);
            }
            return lst;
        }

        public List<Customerinvoice> SearchForReminder(CustomerinvoiceCreteria _customerinvoice)
        {
            List<Customerinvoice> lst = new List<Customerinvoice>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = GetCommand(SQL_Reminder, conn);
                command.Parameters.Add("fromdate", NpgsqlTypes.NpgsqlDbType.Date).Value = _customerinvoice.fromdate;
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Customerinvoice ocustomerinvoice = new Customerinvoice();
                        ocustomerinvoice.customerid = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        ocustomerinvoice.receivable = reader["balance"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["balance"]);
                        ocustomerinvoice.mobileno = reader["mobileno"] == DBNull.Value ? "" : reader["mobileno"].ToString();
                        ocustomerinvoice.customername = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
                        lst.Add(ocustomerinvoice);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _customerinvoice);
            }
            return lst;
        }

        public List<Customerinvoice> SearchAll(long _paymentid)
        {
            List<Customerinvoice> lst = new List<Customerinvoice>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = GetCommand(SQL_SearchAll, conn);
                command.Parameters.Add("paymentid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _paymentid;
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Customerinvoice ocustomerinvoice = new Customerinvoice();
                        ocustomerinvoice.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        ocustomerinvoice.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        ocustomerinvoice.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        ocustomerinvoice.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        ocustomerinvoice.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        ocustomerinvoice.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        ocustomerinvoice.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        ocustomerinvoice.type = reader["type"] == DBNull.Value ? 0 : Convert.ToInt32(reader["type"]);
                        ocustomerinvoice.transactiondate = reader["transactiondate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["transactiondate"]);
                        ocustomerinvoice.billid = reader["billid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["billid"]);
                        ocustomerinvoice.customerid = reader["customerid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["customerid"]);
                        ocustomerinvoice.paymentid = reader["paymentid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["paymentid"]);
                        ocustomerinvoice.receiptno = reader["receiptno"] == DBNull.Value ? "" : reader["receiptno"].ToString();
                        ocustomerinvoice.billno = reader["billno"] == DBNull.Value ? "" : reader["billno"].ToString();
                        ocustomerinvoice.billdate = reader["billdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["billdate"]);
                        ocustomerinvoice.approvedby = reader["approvedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["approvedby"]);
                        ocustomerinvoice.companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"]);
                        ocustomerinvoice.discount = reader["discount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discount"]);
                        ocustomerinvoice.advance = reader["advance"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["advance"]);
                        ocustomerinvoice.receivable = reader["receivable"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["receivable"]);
                        ocustomerinvoice.receivable += reader["balance"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["balance"]);
                        ocustomerinvoice.paid = reader["paid"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["paid"]);
                        ocustomerinvoice.discount = reader["discount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discount"]);
                        ocustomerinvoice.paidcustomerinvoiceid = reader["paidcustomerinvoiceid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["paidcustomerinvoiceid"]);
                        lst.Add(ocustomerinvoice);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _paymentid);
            }
            return lst;
        }



        public Customerinvoice Insert(Customerinvoice oCustomerinvoice)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);

                    Insert(transaction, conn, oCustomerinvoice);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oCustomerinvoice);
            }

            return oCustomerinvoice;
        }

        public Customerinvoice Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Customerinvoice oCustomerinvoice)
        {

            NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oCustomerinvoice.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oCustomerinvoice.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("type", NpgsqlTypes.NpgsqlDbType.Integer).Value = oCustomerinvoice.type;
            command.Parameters.Add("receivable", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oCustomerinvoice.receivable;
            command.Parameters.Add("paid", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oCustomerinvoice.paid;
            command.Parameters.Add("advance", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oCustomerinvoice.advance;
            command.Parameters.Add("discount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oCustomerinvoice.discount;
            command.Parameters.Add("transactiondate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oCustomerinvoice.transactiondate;
            command.Parameters.Add("billid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCustomerinvoice.billid;
            command.Parameters.Add("customerid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCustomerinvoice.customerid;
            command.Parameters.Add("paymentid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCustomerinvoice.paymentid;
            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCustomerinvoice.companyid;
            command.Parameters.Add("receiptno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCustomerinvoice.receiptno == null ? "" : oCustomerinvoice.receiptno;
            command.Parameters.Add("billno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCustomerinvoice.billno == null ? "" : oCustomerinvoice.billno;
            command.Parameters.Add("approvedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCustomerinvoice.approvedby;
            command.Parameters.Add("paidcustomerinvoiceid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCustomerinvoice.paidcustomerinvoiceid;
            command.Parameters.Add("orderid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCustomerinvoice.orderid;

            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oCustomerinvoice.id = Convert.ToInt64(reader[0].ToString());
                }
            }

            /*if(oCustomerinvoice.type == 4)
            {
                command = GetCommand(@"update customerinvoice set paid=paid+@paid,discount=discount+@discount,
                    modifiedby=@modifiedby where id=@paidcustomerinvoiceid and billid=@billid and customerid=@customerid ",
                    conn, transaction);
                command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                command.Parameters.Add("paidcustomerinvoiceid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCustomerinvoice.paidcustomerinvoiceid;
                command.Parameters.Add("billid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCustomerinvoice.billid;
                command.Parameters.Add("customerid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCustomerinvoice.customerid;
                command.Parameters.Add("paid", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oCustomerinvoice.paid;
                command.Parameters.Add("discount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oCustomerinvoice.discount;
                command.ExecuteNonQuery();
            }*/

            return oCustomerinvoice;
        }

        public Customerinvoice Update(Customerinvoice oCustomerinvoice)
        {
            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oCustomerinvoice);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oCustomerinvoice);
            }
            return oCustomerinvoice;
        }
        public bool UpdatePayment(NpgsqlTransaction transaction, NpgsqlConnection conn, Customerinvoice oCustomerinvoice)
        {
            const string SQL_UpdatePayment = @"UPDATE public.customerinvoice SET modifiedby=@modifiedby, version=version+1, modifiedon=@modifiedon,discount=discount+@discount,paid=paid+@paid
                                    where billid=@billid and id=@paidcustomerinvoiceid and customerid=@customerid RETURNING version,paid,receivable";
            NpgsqlCommand command = GetCommand(SQL_UpdatePayment, conn, transaction);
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("paid", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oCustomerinvoice.paid;
            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCustomerinvoice.companyid;
            command.Parameters.Add("discount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oCustomerinvoice.discount;
            command.Parameters.Add("billid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCustomerinvoice.billid;
            command.Parameters.Add("paidcustomerinvoiceid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCustomerinvoice.paidcustomerinvoiceid;
            command.Parameters.Add("customerid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCustomerinvoice.customerid;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    return true;
                }
            }
            return false;
        }
        public Customerinvoice Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Customerinvoice oCustomerinvoice)
        {

            NpgsqlCommand command = GetCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCustomerinvoice.id;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oCustomerinvoice.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCustomerinvoice.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCustomerinvoice.companyid;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oCustomerinvoice.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oCustomerinvoice.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("type", NpgsqlTypes.NpgsqlDbType.Integer).Value = oCustomerinvoice.type;
            command.Parameters.Add("receivable", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oCustomerinvoice.receivable;
            command.Parameters.Add("paid", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oCustomerinvoice.paid;
            command.Parameters.Add("discount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oCustomerinvoice.discount;
            command.Parameters.Add("transactiondate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oCustomerinvoice.transactiondate;
            command.Parameters.Add("billid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCustomerinvoice.billid;
            command.Parameters.Add("customerid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCustomerinvoice.customerid;
            command.Parameters.Add("paymentid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCustomerinvoice.paymentid;
            command.Parameters.Add("receiptno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCustomerinvoice.receiptno == null ? "" : oCustomerinvoice.receiptno;
            command.Parameters.Add("billno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCustomerinvoice.billno == null ? "" : oCustomerinvoice.billno;
            command.Parameters.Add("approvedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCustomerinvoice.approvedby;
            command.Parameters.Add("advance", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oCustomerinvoice.advance;
            command.Parameters.Add("paidcustomerinvoiceid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCustomerinvoice.paidcustomerinvoiceid;
            command.Parameters.Add("orderid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCustomerinvoice.orderid;

            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oCustomerinvoice.version = Convert.ToInt32(reader[0].ToString());
                }
            }

            return oCustomerinvoice;
        }

        public bool Update(NpgsqlTransaction transaction, NpgsqlConnection conn, List<Customerinvoice> list, long id)
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
                    item.paymentid = id;
                    Insert(transaction, conn, item);
                }
            }
            return true;
        }

        public bool Delete(CustomerinvoiceCreteria oCustomerinvoice)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    if (oCustomerinvoice.id > 0)
                    {
                        hasSuccessCount = Delete(transaction, conn, oCustomerinvoice);
                    }
                    else if (oCustomerinvoice.ids != null && oCustomerinvoice.ids.Count > 0)
                    {
                        var list = Search(oCustomerinvoice);
                        foreach (var item in list)
                        {
                            hasSuccessCount = Delete(transaction, conn, item);
                            if (!hasSuccessCount)
                                throw new Exception("Can't delete, some conflict");
                        }
                    }
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oCustomerinvoice);

            }
            return hasSuccessCount;

        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Customerinvoice oCustomerinvoice)
        {

            NpgsqlCommand command = GetCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oCustomerinvoice.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oCustomerinvoice.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCustomerinvoice.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

        public bool DeleteAll(long paymentid, long userid, NpgsqlConnection conn, NpgsqlTransaction transaction)
        {
            int successCount = 0;
            NpgsqlCommand command = GetCommand(SQL_DeleteAll, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = false;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("paymentid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = paymentid;
            successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

        internal Customerinvoice AddAdvance(Voucher oVoucher, NpgsqlConnection conn, NpgsqlTransaction transaction)
        {
            Customerinvoice customerinvoice = new Customerinvoice();
            customerinvoice.isactive = true;
            customerinvoice.version = 1;
            customerinvoice.type = 5;
            customerinvoice.createdby = Context.UserID;
            customerinvoice.modifiedby = Context.UserID;
            customerinvoice.createdon = DateTimeNow;
            customerinvoice.modifiedon = DateTimeNow;
            customerinvoice.transactiondate = oVoucher.voucherdate;
            var voucheritem = oVoucher.voucheritems.Find(e => e.attributes != null && e.attributes.customerid > 0);
            customerinvoice.customerid = voucheritem.attributes.customerid;
            customerinvoice.companyid = oVoucher.companyid;
            customerinvoice.advance = Math.Abs(voucheritem.amount);
            Insert(transaction, conn, customerinvoice);
            return customerinvoice;
        }

        internal Customerinvoice RefundAdvance(Voucher oVoucher, NpgsqlConnection conn, NpgsqlTransaction transaction)
        {
            Customerinvoice customerinvoice = new Customerinvoice();
            customerinvoice.isactive = true;
            customerinvoice.version = 1;
            customerinvoice.type = 5;
            customerinvoice.createdby = Context.UserID;
            customerinvoice.modifiedby = Context.UserID;
            customerinvoice.createdon = DateTimeNow;
            customerinvoice.modifiedon = DateTimeNow;
            customerinvoice.transactiondate = oVoucher.voucherdate;
            var voucheritem = oVoucher.voucheritems.Find(e => e.attributes != null && e.attributes.customerid > 0);
            customerinvoice.customerid = voucheritem.attributes.customerid;
            customerinvoice.companyid = oVoucher.companyid;
            customerinvoice.advance = Math.Abs(voucheritem.amount)*-1;
            Insert(transaction, conn, customerinvoice);
            return customerinvoice;
        }

        internal Customerinvoice AddOpening(Voucheritembillwise item,long companyid, long customerid, NpgsqlConnection conn, NpgsqlTransaction transaction)
        {
            Customerinvoice customerinvoice = new Customerinvoice();
            customerinvoice.isactive = true;
            customerinvoice.version = 1;
            customerinvoice.type = 5;
            customerinvoice.createdby = Context.UserID;
            customerinvoice.modifiedby = Context.UserID;
            customerinvoice.createdon = DateTimeNow;
            customerinvoice.modifiedon = DateTimeNow;
            customerinvoice.companyid = companyid;
            customerinvoice.transactiondate = item.referenceinvoicedate;
            customerinvoice.customerid = customerid;
            if(item.payable > 0)
                customerinvoice.advance = Math.Abs(item.payable);
            else
                customerinvoice.advance = Math.Abs(item.receivable) * -1;
            Insert(transaction, conn, customerinvoice);
            return customerinvoice;
        }

        internal bool RemoveOpening(Voucheritembillwise item, long customerid, NpgsqlConnection conn, NpgsqlTransaction transaction)
        {
            NpgsqlCommand command = GetCommand(SQL_Delete.Replace("and version=@version", ""), conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = false;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.customerinvoiceid;
            return command.ExecuteNonQuery() > 0;

        }
    }

}
