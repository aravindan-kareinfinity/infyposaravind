using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
using Newtonsoft.Json;

namespace InfyPOS.Data
{
    public class CustomerpaymentDataService : SAASData
    {
        const string SQL_Search = @"select customerpayment.id, customerpayment.isactive, customerpayment.createdby, customerpayment.modifiedby, 
customerpayment.version, customerpayment.createdon, customerpayment.modifiedon, customerpayment.customerid, customerpayment.hastds,customerpayment.tdspercentage,
customerpayment.tdsamount,customerpayment.sourcetype,customerpayment.paidon, customerpayment.receivedby, customerpayment.amount, customerpayment.receiptno,
customerpayment.collectedat,customerpayment.paymentinfo,customerpayment.locationid,customerpayment.counterid,customerpayment.companyid,customerpayment.rounding,
customerpayment.orderid,customerpayment.orderno,customer.name,customerpayment.discount, employee.name receivedbyname,
customerpayment.paymentinfo->>'remarks' remarks,customerpayment.sourcetype,ledgerid
from customerpayment 
inner join customer on customer.id = customerpayment.customerid 
left outer join employee on employee.id = customerpayment.receivedby  where customerpayment.id=@id";

        const string SQL_Insert = @"INSERT INTO public.customerpayment(isactive, createdby, modifiedby, version, createdon, modifiedon, customerid,collectedat, paidon, receivedby, amount,discount, receiptno,paymentinfo,ledgerid,locationid,counterid,companyid,orderid,orderno,rounding,hastds,tdspercentage,tdsamount,sourcetype) 
                                    VALUES(@isactive, @createdby, @modifiedby, @version, @createdon, @modifiedon, @customerid,@collectedat, @paidon, @receivedby, @amount,@discount, @receiptno,@paymentinfo,@ledgerid,@locationid,@counterid,@companyid,@orderid,@orderno,@rounding,@hastds,@tdspercentage,@tdsamount,@sourcetype) RETURNING id";

        const string SQL_Update = @"UPDATE public.customerpayment SET modifiedby=@modifiedby, version=@version+1, paymentinfo=@paymentinfo,ledgerid =@ledgerid, modifiedon=@modifiedon,collectedat=@collectedat, customerid=@customerid, paidon=@paidon,orderid=@orderid,orderno=@orderno,
                                    receivedby=@receivedby, amount=@amount,discount=@discount, receiptno=@receiptno,locationid=@locationid,counterid=@counterid,companyid=@companyid,hastds=@hastds,tdspercentage=@tdspercentage,tdsamount=@tdsamount,sourcetype=@sourcetype 
                                    where id=@id and version=@version RETURNING version";

        const string SQL_Delete = @"update public.customerpayment set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version;
                                    update public.customerinvoice set isactive=false where paymentid=@id;";

        const string SQL_Collection = @"with creditcollection as 
        (
            select customerpayment.id, customer.name, customer.mobileno, customerpayment.amount, customerpayment.discount, 
            customerpayment.createdby, customerpayment.paidon::date,
            cast(customerpayment.paymentinfo->>'paymentmode' as numeric) paymentmode, 
            cast(customerpayment.paymentinfo->>'paidamount' as numeric) paid,ledgerid,
            customerpayment.locationid, customerpayment.counterid, customerpayment.companyid
            from customerpayment
            inner join users on users.id = customerpayment.createdby
            left outer join customer on customer.id = customerpayment.customerid
            where customerpayment.isactive=true and customerpayment.paidon>=@fromdate and customerpayment.paidon<@todate
            and case when @locationid=0 then customerpayment.locationid <> 0 else customerpayment.locationid = @locationid end
            and case when @companyid=0 then customerpayment.companyid >= 0 else customerpayment.companyid = @companyid end
            and case when @counterid=0 then customerpayment.counterid <> 0 else customerpayment.counterid = @counterid end
            and case when @cashierid=0 then (users.employeeid <> 0 or users.employeeid=0 or users.employeeid is null) else users.employeeid = @cashierid end
        ),billnos as (
            select customerinvoice.paymentid,STRING_AGG(customerinvoice.billno,',') billno from customerinvoice 
            inner join creditcollection on creditcollection.id = customerinvoice.paymentid
            where type=4 and customerinvoice.paymentid>0 and customerinvoice.paymentid>0
            group by customerinvoice.paymentid
        ),fullcreditcollection as
        (
            select creditcollection.*,billnos.billno from creditcollection
            left outer join billnos on billnos.paymentid = creditcollection.id
        )
        select location.name as location, counter.name as counter, employee.name employeename, paymentmode,ledgerid,
        creditcollection.companyid, creditcollection.name, creditcollection.mobileno, sum(amount) as amount,sum(paid) paid,
        sum(discount) discount, count(*) noofpayments, STRING_AGG(creditcollection.billno,',') billno
        from fullcreditcollection creditcollection
        inner join referencelist location on location.id = creditcollection.locationid
        inner join referencelist counter on counter.id = creditcollection.counterid
        inner join users on users.id = creditcollection.createdby
        left outer join employee on employeeid = employee.id
        group by location.name, counter.name, employee.name , paymentmode, creditcollection.companyid, 
        creditcollection.name, creditcollection.mobileno,ledgerid
        order by location, counter, employeename";
        public List<Customerpayment> Search(CustomerpaymentCreteria _customerpayment)
        {
            List<Customerpayment> lst = new List<Customerpayment>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_customerpayment.id != 0)
                {
                    command = GetCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _customerpayment.id;
                }

                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("customerpayment.isactive", true, "=");

                    if(!string.IsNullOrEmpty(_customerpayment.receiptno))
                        qb.AddParameter("customerpayment.receiptno", _customerpayment.receiptno, "=");

                    if (_customerpayment.orderid > 0)
                        qb.AddParameter("customerpayment.orderid", _customerpayment.orderid, "=");

                    if (_customerpayment.collectedat > 0)
                        qb.AddParameter("customerpayment.collectedat", _customerpayment.collectedat, "=");

                    if (_customerpayment.customerid > 0)
                        qb.AddParameter("customerpayment.customerid", _customerpayment.customerid, "=");
                    if (_customerpayment.fromdate != DateTime.MinValue && _customerpayment.todate != DateTime.MinValue)
                    {
                        qb.AddParameter("customerpayment.paidon", "datefrom", _customerpayment.fromdate.Date, ">=");
                        qb.AddParameter("customerpayment.paidon", "dateto", _customerpayment.todate.AddDays(1).Date, "<");
                    }
                    qb.SortField = "customerpayment.id desc";
                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Customerpayment ocustomerpayment = new Customerpayment();
                        ocustomerpayment.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        ocustomerpayment.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        ocustomerpayment.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        ocustomerpayment.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        ocustomerpayment.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        ocustomerpayment.sourcetype = reader["sourcetype"] == DBNull.Value ? 0 : Convert.ToInt32(reader["sourcetype"]);
                        ocustomerpayment.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        ocustomerpayment.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        ocustomerpayment.customerid = reader["customerid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["customerid"]);
                        ocustomerpayment.collectedat = reader["collectedat"] == DBNull.Value ? 0 : Convert.ToInt32(reader["collectedat"]);

                        ocustomerpayment.tdspercentage = reader["tdspercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["tdspercentage"]);
                        ocustomerpayment.tdsamount = reader["tdsamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["tdsamount"]);
                        ocustomerpayment.hastds = reader["hastds"] == DBNull.Value ? false : Convert.ToBoolean(reader["hastds"]);

                        ocustomerpayment.paidon = reader["paidon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["paidon"]);
                        ocustomerpayment.receivedby = reader["receivedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["receivedby"]);
                        ocustomerpayment.receivedbyname = reader["receivedbyname"] == DBNull.Value ? "" : reader["receivedbyname"].ToString();
                        ocustomerpayment.amount = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"]);
                        ocustomerpayment.discount = reader["discount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discount"]);
                        ocustomerpayment.rounding = reader["rounding"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["rounding"]);
                        ocustomerpayment.receiptno = reader["receiptno"] == DBNull.Value ? "" : reader["receiptno"].ToString();
                        ocustomerpayment.customername = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
                        ocustomerpayment.remarks = reader["remarks"] == DBNull.Value ? "" : reader["remarks"].ToString();

                        ocustomerpayment.orderno = reader["orderno"] == DBNull.Value ? "" : reader["orderno"].ToString();
                        ocustomerpayment.orderid = reader["orderid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["orderid"]);

                        ocustomerpayment.ledgerid = reader["ledgerid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["ledgerid"]);

                        ocustomerpayment.paymentinfo_json = reader["paymentinfo"] == DBNull.Value ? "null" : reader["paymentinfo"].ToString();
                        ocustomerpayment.companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"]);
                        ocustomerpayment.locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                        ocustomerpayment.counterid = reader["counterid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["counterid"]);

                        ocustomerpayment.paymentmode = (ocustomerpayment.paymentinfo != null ? ocustomerpayment.paymentinfo.mode : "");
                        lst.Add(ocustomerpayment);
                    }
                }
                if ((_customerpayment.id > 0 && lst.Count == 1))
                {
                    CustomerinvoiceDataService subCustomerinvoice = new CustomerinvoiceDataService() { Context = this.Context };
                    foreach (var item in lst)
                    {
                        item.Customerinvoice = subCustomerinvoice.SearchAll(item.id);
                    }
                }
                else if(_customerpayment.orderid > 0)
                {
                    CustomerinvoiceDataService subCustomerinvoice = new CustomerinvoiceDataService() { Context = this.Context };
                    foreach (var item in lst)
                    {
                        item.Customerinvoice = subCustomerinvoice.Search(new CustomerinvoiceCreteria()
                        {
                            orderid = item.orderid
                        });
                    }
                }
                else if (lst.Count > 0 && _customerpayment.showbills)
                {
                    command = GetCommand(string.Format(@"select customerinvoice.paymentid,  customerinvoice.customerid, customerinvoice.billno from customerinvoice 
                                            where type=4 and customerinvoice.paymentid>0 
                                            and customerinvoice.paymentid in ({0})
                                            order by paymentid",string.Join(",",lst.ConvertAll(x=>x.id))), conn);
                    using (System.Data.IDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            long id = reader["paymentid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["paymentid"]);
                            string billno = reader["billno"] == DBNull.Value ? "" : reader["billno"].ToString();
                            var oitem = lst.Find(e => e.id == id);
                            if (oitem != null)
                                oitem.billnos += (string.IsNullOrEmpty(oitem.billnos) ? "" : ",") + billno;
                        }
                    }

                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _customerpayment);
            }
            return lst;
        }

        public Customerpayment Insert(Customerpayment oCustomerpayment)
        {
            if (oCustomerpayment == null || oCustomerpayment.Customerinvoice == null || oCustomerpayment.Customerinvoice.Count == 0)
                throw new Exception("Please select the invoice to be adjusted");
            if(oCustomerpayment.Customerinvoice.Sum(x=>x.paid) != (oCustomerpayment.amount + oCustomerpayment.tdsamount))
            {
                throw new Exception("Adjusted invoice values and paid amount not matching");
            }
            oCustomerpayment.customerid = oCustomerpayment.Customerinvoice[0].customerid;
            oCustomerpayment.companyid = oCustomerpayment.Customerinvoice[0].companyid;
            oCustomerpayment.discount = oCustomerpayment.Customerinvoice.Sum(e => e.discount);
            //oCustomerpayment.paidon = DateTimeNow;
            NpgsqlTransaction transaction = null;
            try
            {
                var sequence = SequenceManager.Instance(Context.OrganizationID).Get("customerpayment.receipt", 0);
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);
                    oCustomerpayment.receiptno = sequence.GetNextNumber(Context,conn, transaction);
                    if (oCustomerpayment.collectedat == 1)
                        oCustomerpayment.receivedby = Context.UserID;
                    Insert(transaction, conn, oCustomerpayment);
                    CustomerinvoiceDataService subCustomerinvoice = new CustomerinvoiceDataService() { Context = this.Context };
                    foreach (var item in oCustomerpayment.Customerinvoice)
                    {
                        item.paymentid = oCustomerpayment.id;
                        item.receiptno = oCustomerpayment.receiptno;
                        item.receivable = item.paid + item.discount;
                        item.createdby = Context.UserID;
                        item.createdon = DateTimeNow;
                        item.transactiondate = DateTimeNow;
                        item.isactive = true;
                        item.version = 1;
                        item.type = 4;
                        subCustomerinvoice.Insert(transaction, conn, item);
                        if (!subCustomerinvoice.UpdatePayment(transaction, conn, item))
                            throw new Exception("Can't update the paid amount on " + item.billno);
                    }
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                oCustomerpayment.id = 0;
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oCustomerpayment);
            }

            return oCustomerpayment;
        }

        public Customerpayment Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Customerpayment oCustomerpayment)
        {

            NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oCustomerpayment.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oCustomerpayment.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("customerid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCustomerpayment.customerid;
            command.Parameters.Add("collectedat", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCustomerpayment.collectedat;
            command.Parameters.Add("sourcetype", NpgsqlTypes.NpgsqlDbType.Integer).Value = oCustomerpayment.sourcetype;
            command.Parameters.Add("paidon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oCustomerpayment.paidon;
            command.Parameters.Add("receivedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCustomerpayment.receivedby;

            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCustomerpayment.companyid;
            command.Parameters.Add("counterid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCustomerpayment.counterid;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCustomerpayment.locationid;
            command.Parameters.Add("ledgerid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCustomerpayment.ledgerid;
            command.Parameters.Add("amount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oCustomerpayment.amount;
            command.Parameters.Add("discount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oCustomerpayment.discount;
            command.Parameters.Add("rounding", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oCustomerpayment.rounding;
            command.Parameters.Add("paymentinfo", NpgsqlTypes.NpgsqlDbType.Json).Value = oCustomerpayment.paymentinfo_json == null ? "null" : oCustomerpayment.paymentinfo_json;
            command.Parameters.Add("receiptno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCustomerpayment.receiptno == null ? "" : oCustomerpayment.receiptno;

            command.Parameters.Add("hastds", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oCustomerpayment.hastds;
            command.Parameters.Add("tdspercentage", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oCustomerpayment.tdspercentage;
            command.Parameters.Add("tdsamount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oCustomerpayment.tdsamount;

            command.Parameters.Add("orderno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCustomerpayment.orderno == null ? "" : oCustomerpayment.orderno;
            command.Parameters.Add("orderid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCustomerpayment.orderid;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oCustomerpayment.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            return oCustomerpayment;
        }

        public Customerpayment Update(Customerpayment oCustomerpayment)
        {
            throw new Exception("Please delete the existing payment and created the new payment.");
            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    oCustomerpayment.discount = oCustomerpayment.Customerinvoice.Sum(e => e.discount);
                    Update(transaction, conn, oCustomerpayment);
                    CustomerinvoiceDataService subCustomerinvoice = new CustomerinvoiceDataService() { Context = this.Context };
                    subCustomerinvoice.Update(transaction, conn, oCustomerpayment.Customerinvoice, oCustomerpayment.id);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oCustomerpayment);
            }

            return oCustomerpayment;
        }

        public Customerpayment Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Customerpayment oCustomerpayment)
        {

            NpgsqlCommand command = GetCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCustomerpayment.id;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oCustomerpayment.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCustomerpayment.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oCustomerpayment.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oCustomerpayment.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("customerid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCustomerpayment.customerid;
            command.Parameters.Add("collectedat", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCustomerpayment.collectedat;
            command.Parameters.Add("paidon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oCustomerpayment.paidon;
            command.Parameters.Add("receivedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCustomerpayment.receivedby;
            command.Parameters.Add("sourcetype", NpgsqlTypes.NpgsqlDbType.Integer).Value = oCustomerpayment.sourcetype;
            command.Parameters.Add("amount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oCustomerpayment.amount;
            command.Parameters.Add("discount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oCustomerpayment.discount;
            command.Parameters.Add("rounding", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oCustomerpayment.rounding;
            command.Parameters.Add("paymentinfo", NpgsqlTypes.NpgsqlDbType.Json).Value = oCustomerpayment.paymentinfo_json == null ? "null" : oCustomerpayment.paymentinfo_json;
            command.Parameters.Add("receiptno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCustomerpayment.receiptno == null ? "" : oCustomerpayment.receiptno;
            command.Parameters.Add("ledgerid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCustomerpayment.ledgerid ;
            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCustomerpayment.companyid;
            command.Parameters.Add("counterid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCustomerpayment.counterid;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCustomerpayment.locationid;
            command.Parameters.Add("orderno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCustomerpayment.orderno == null ? "" : oCustomerpayment.orderno;
            command.Parameters.Add("orderid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCustomerpayment.orderid;
            command.Parameters.Add("hastds", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oCustomerpayment.hastds;
            command.Parameters.Add("tdspercentage", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oCustomerpayment.tdspercentage;
            command.Parameters.Add("tdsamount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oCustomerpayment.tdsamount;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oCustomerpayment.version = Convert.ToInt32(reader[0].ToString());
                }
            }
            return oCustomerpayment;
        }


        public bool Delete(Customerpayment oCustomerpayment)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oCustomerpayment);
                    CustomerinvoiceDataService subCustomerinvoice = new CustomerinvoiceDataService() { Context = this.Context };
                    subCustomerinvoice.DeleteAll(oCustomerpayment.id, oCustomerpayment.modifiedby, conn, transaction);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oCustomerpayment);
            }
            return hasSuccessCount;

        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Customerpayment oCustomerpayment)
        {
            NpgsqlCommand command = GetCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oCustomerpayment.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oCustomerpayment.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCustomerpayment.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

        internal PrintableLedger PrintSummary(bool includevoucher,long customerid, long companyid, DateTime fromdate, DateTime todate)
        {
            if (customerid == 0) throw new Exception("Please select the patient");
            if (companyid == 0) throw new Exception("Please select the company");
            PrintableLedger printableLedger = new PrintableLedger();
            CustomerDataService suppliercompanyDataService = new CustomerDataService() { Context = this.Context };

            var customers = suppliercompanyDataService.Search(new CustomerCreteria()
            {
                id = customerid
            });

            LedgerDataService ledgerDataService = new LedgerDataService() { Context = this.Context };
            var ledgers = ledgerDataService.Search(new LedgerCreteria()
            {
                customerid = customerid
            });

            var ledgerid = ledgers.Count > 0 ? ledgers[0].id : 0;

            string query = @"
select sum(customerinvoice.receivable)
from customerinvoice
where customerinvoice.isactive = true and customerinvoice.type in (1,2)
and customerinvoice.companyid=@companyid
and customerinvoice.customerid = @customerid and transactiondate<@fromdate;

select sum(customerinvoice.paid+customerinvoice.discount)
from customerinvoice
where customerinvoice.isactive = true and customerinvoice.type >2
and customerinvoice.companyid=@companyid
and customerinvoice.customerid = @customerid and transactiondate<@fromdate;

--Voucher Opening Balance

select bill.billno invoiceno,customerinvoice.transactiondate::date entrydate,bill.billdate invoicedate,
customerinvoice.receivable totalamount,
bill.createdon,bill.createdby,bill.modifiedon,bill.modifiedby from customerinvoice 
inner join bill on bill.id = billid
where customerinvoice.isactive=true and customerinvoice.type in (1,2)
and customerinvoice.customerid = @customerid 
and bill.companyid=@companyid
and transactiondate>=@fromdate and transactiondate<@todate;

select amount paidamount,receiptno,discount,paidon paymentdate,tdsamount,
cast(paymentinfo->>'paymentmode' as int) paymentmode,customerpayment.id,
referencelist.name bankname,customerpayment.sourcetype,ledger.displayname ledgername,
customerpayment.createdon,customerpayment.createdby,customerpayment.modifiedon,customerpayment.modifiedby
from customerpayment
left outer join referencelist on referencelist.id = cast(paymentinfo->>'bankid' as bigint)
left outer join ledger on ledger.id = ledgerid
where customerpayment.isactive = true 
and customerpayment.customerid = @customerid 
and customerpayment.companyid = @companyid 
and paidon>=@fromdate and paidon<@todate;

select customerpayment.id,
STRING_AGG(customerinvoice.billno,',') invoiceno
from customerpayment
inner join customerinvoice on customerinvoice.paymentid = customerpayment.id
where customerpayment.isactive = true and customerinvoice.isactive=true 
and customerpayment.customerid = @customerid and paidon>=@fromdate 
and paidon<@todate and customerpayment.companyid = @companyid
group by customerpayment.id;

with b2bsettlement as
(
    select bill.customerid,billsettlementmaster.id
    from billsettlementmaster
    inner join billsettlement on billsettlement.billsettlementmasterid = billsettlementmaster.id
    inner join bill on bill.id =billid
    where billsettlementmaster.isactive = true
    and bill.companyid=@companyid
    and billsettlementmaster.settlementon >= @fromdate
    and billsettlementmaster.settlementon < @todate
    and bill.isb2b=true
	and bill.customerid = @customerid
),b2bsales as
(
	select bill.customerid,billsettlement.billid,billsettlement.billno,billdate,bill.receivable
    from billsettlementmaster
    inner join billsettlement on billsettlement.billsettlementmasterid = billsettlementmaster.id
    inner join bill on bill.id =billid
    where billsettlementmaster.isactive = true
    and bill.companyid=@companyid 
    and billsettlementmaster.settlementon >= @fromdate
    and billsettlementmaster.settlementon < @todate
    and bill.isb2b=true and bill.iscreditbill=false
	and bill.customerid = @customerid
)
select 'P' mode,billsettlementmaster.id,customer.name,
sum(paid_cash) cash,sum(paid_card) card,
sum(paid_upi) upi_paid,sum(paid_credit) paid_credit,
sum(cast(paymentinfo->>'voucher_paid' as numeric)) voucher_paid,
sum(case when paid_cash=0 then cast(paymentinfo->>'cash_return' as numeric) else 0 end) cash_refund,
billsettlementmaster.code billno,
billsettlementmaster.settlementon billdate,
0 receivable
from b2bsettlement
inner join billsettlementmaster on billsettlementmaster.id = b2bsettlement.id
inner join customer on customer.id = b2bsettlement.customerid
group by customer.name,customer.id,billsettlementmaster.id,billsettlementmaster.code,billsettlementmaster.settlementon
union all
select 'S' mode,billid,customer.name,
0 cash,
0 card,
0 upi_paid,
0 paid_credit,
0 voucher_paid,
0 cash_refund,
billno,
billdate,
receivable
from b2bsales
inner join customer on customer.id = b2bsales.customerid;
";

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

            if (includevoucher && ledgerid > 0)
            {

                string ledgeropeningquery = @"with ledgerhistory
                as
                (
                select voucheritems.iscredit iscredit,
                sum(voucheritems.amount) amount from voucher
                inner join voucheritems on voucherid = voucher.id and voucheritems.isactive=true
                inner join ledger on ledger.id = voucher.adjustmentledgerid
                where ledgerid = @ledgerid and voucher.isactive = true 
                and voucheritems.isactive = true and voucher.voucherdate<@fromdate 
                and coalesce(cast(voucher.attributes->>'lrinvoicereturnid' as bigint),0)=0
                and coalesce(cast(voucher.attributes->>'customerinvoiceid' as bigint),0)=0
                and voucher.companyid=@companyid
                group by voucheritems.iscredit
                union all
                select voucher.adjustmentcredit iscredit,sum(voucheritems.amount) amount from voucher
                inner join voucheritems on voucherid = voucher.id and voucheritems.isactive=true
                inner join ledger on ledger.id = voucheritems.ledgerid
                where adjustmentledgerid=@ledgerid and voucher.isactive = true 
                and coalesce(cast(voucher.attributes->>'lrinvoicereturnid' as bigint),0)=0
                and coalesce(cast(voucher.attributes->>'customerinvoiceid' as bigint),0)=0
                and voucher.voucherdate<@fromdate and voucher.companyid=@companyid
                group by voucher.adjustmentcredit
                )
                select * from ledgerhistory;";

                if (companyid == 0)
                {
                    ledgeropeningquery = ledgeropeningquery.Replace("and voucher.companyid=@companyid", "");
                }

                query = query.Replace("--Voucher Opening Balance", ledgeropeningquery);

                query += @" select ledger.displayname,voucheritems.amount,voucherdate,voucherno,voucher.id,
                            case when sourcetype=1 then -15 else voucher.vouchertypeid end vouchertypeid,
                            voucheritems.iscredit,voucher.narration,voucher.createdon,voucher.createdby,
                            voucher.modifiedon,voucher.modifiedby
                            from voucheritems 
                            inner join voucher on voucher.id = voucheritems.voucherid
                            inner join ledger on ledger.id = adjustmentledgerid
                            where ledgerid=@ledgerid and voucherdate>=@fromdate and voucherdate<@todate 
                            and voucher.companyid = @companyid and voucheritems.isactive=true and voucher.isactive=true
                            UNION ALL
                            select ledger.displayname,voucher.amount,voucherdate,voucherno,voucher.id,
                            case when sourcetype=1 then -15 else voucher.vouchertypeid end vouchertypeid,
                            voucher.adjustmentcredit iscredit,voucher.narration,voucher.createdon,voucher.createdby,
                            voucher.modifiedon,voucher.modifiedby
                            from voucheritems 
                            inner join voucher on voucher.id = voucheritems.voucherid
                            inner join ledger on ledger.id = ledgerid
                            where adjustmentledgerid=@ledgerid and voucherdate>=@fromdate and voucherdate<@todate
                            and voucher.companyid = @companyid and voucheritems.isactive=true and voucher.isactive=true;";
            }
            var param = new SortedList<string, object>();
            param.Add("customerid", customers[0].id);
            param.Add("ledgerid", ledgerid);
            param.Add("companyid", companyid);
            param.Add("fromdate", fromdate.Date);
            param.Add("todate", todate.Date.AddDays(1));

            if(companyid == 0)
            {
                query = query.Replace("and voucher.companyid = @companyid", "");
                query = query.Replace("and customerpayment.companyid = @companyid", "");
                query = query.Replace("and customerinvoice.companyid=@companyid", "");
                query = query.Replace("and bill.companyid=@companyid", "");
            }

            printableLedger.ledgername = customers[0].name;
            printableLedger.address = customers[0].address;
            printableLedger.title = fromdate.ToString("dd-MM-yyyy") + " - " + todate.ToString("dd-MM-yyyy");

            CompanyDataService companyDataService = new CompanyDataService() { Context = this.Context };
            printableLedger.company = companyDataService.Search(new CompanyCreteria()
            {
                id = companyid
            }).First();

            using (var reader = ExecuteReader(query, param))
            {
                decimal pendingamount = 0;
                if (reader.Read() && !reader.IsDBNull(0))
                {
                    pendingamount = reader.GetDecimal(0);
                }
                if (reader.NextResult() && reader.Read() && !reader.IsDBNull(0))
                {
                    pendingamount -= reader.GetDecimal(0);
                }

                if(includevoucher && ledgerid>0)
                {
                    if(reader.NextResult())
                    {
                        while (reader.Read())
                        {
                            if (Convert.ToBoolean(reader["iscredit"]))
                                pendingamount -= Convert.ToDecimal(reader["amount"]);
                            else
                                pendingamount += Convert.ToDecimal(reader["amount"]);
                        }
                    }
                }
                
                if (pendingamount > 0)
                {
                    printableLedger.obdebit = pendingamount.ToString("F2");
                }
                else
                {
                    printableLedger.obcredit = pendingamount.ToString("F2");
                }

                printableLedger.transactions = new List<PrintableLedger.Transaction>();
                if (reader.NextResult())
                {
                    while (reader.Read())
                    {
                        printableLedger.transactions.Add(new PrintableLedger.Transaction()
                        {
                            type = Convert.ToDecimal(reader["totalamount"])>0 ?"Sales": "Sales Return",
                            date = Convert.ToDateTime(reader["entrydate"]),
                            description = String.Format("{0}", reader["invoiceno"].ToString(),
                            Convert.ToDateTime(reader["invoicedate"]).ToString("dd-MM-yyyy")),
                            createdby = reader["Createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["Createdby"]),
                            createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]),
                            modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]),
                            modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]),
                            debit = Convert.ToDecimal(reader["totalamount"])>0 ? Convert.ToDecimal(reader["totalamount"]).ToString("F2") : "",
                            credit = Convert.ToDecimal(reader["totalamount"]) > 0 ? "" : (Convert.ToDecimal(reader["totalamount"])*-1).ToString("F2")
                        });
                    }
                }
                if (reader.NextResult())
                {
                    while (reader.Read())
                    {
                        decimal amount = Convert.ToDecimal(reader["paidamount"]);
                        decimal tdsamount = Convert.ToDecimal(reader["tdsamount"]);
                        decimal discount = Convert.ToDecimal(reader["discount"]);
                        DateTime paymentdate = Convert.ToDateTime(reader["paymentdate"]);
                        int paymentmode = Convert.ToInt32(reader["paymentmode"]);
                        int sourcetype = reader["sourcetype"] == DBNull.Value ? 0 : Convert.ToInt32(reader["sourcetype"]);
                        long id = Convert.ToInt32(reader["id"]);
                        string type = "Payment";
                        if (sourcetype == 1)
                            type = "Order Advance";
                        if (amount>0)
                        {
                            string paymentmodedisplay = paymentmode == 2 ?  "Cash": Convert.ToString(reader["bankname"]);
                            printableLedger.transactions.Add(new PrintableLedger.Transaction()
                            {
                                type = type,
                                date = paymentdate,
                                id = id,
                                createdby = reader["Createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["Createdby"]),
                                createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]),
                                modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]),
                                modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]),
                                detail = new VoucherDisplayItem() { narration = "Customer Payment @ "+ reader["receiptno"].ToString() },
                                reference= reader["receiptno"].ToString(),
                                description = paymentmodedisplay +" ("+ reader["receiptno"].ToString()+")",
                                credit = amount.ToString("F2")
                            });
                        }
                        if (discount > 0)
                        {
                            printableLedger.transactions.Add(new PrintableLedger.Transaction()
                            {
                                type = type,
                                date = paymentdate,
                                id = id,
                                createdby = reader["Createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["Createdby"]),
                                createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]),
                                modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]),
                                modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]),
                                reference = reader["receiptno"].ToString(),
                                detail = new VoucherDisplayItem() { narration = "Customer Payment @ " + reader["receiptno"].ToString() },
                                description = "Discount (" + reader["receiptno"].ToString() + ")",
                                credit = discount.ToString("F2")
                            });
                        }
                        if (tdsamount > 0)
                        {
                            printableLedger.transactions.Add(new PrintableLedger.Transaction()
                            {
                                type = type,
                                date = paymentdate,
                                id = id,
                                createdby = reader["Createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["Createdby"]),
                                createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]),
                                modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]),
                                modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]),
                                reference = reader["receiptno"].ToString(),
                                detail = new VoucherDisplayItem() { narration = "Customer Payment @ " + reader["receiptno"].ToString() },
                                description = "TDS (" + reader["receiptno"].ToString() + ")",
                                debit = tdsamount.ToString("F2")
                            });
                        }
                    }
                }


                if (reader.NextResult())
                {
                    while (reader.Read())
                    {
                        var id = Convert.ToInt64(reader["id"]);
                        var titem = printableLedger.transactions.Find(x => x.id == id && x.type == "Payment");
                        if (titem != null)
                        {
                            titem.description = titem.description + "(" + reader["invoiceno"].ToString() + ")";
                            if (titem.detail == null) titem.detail = new VoucherDisplayItem();
                            titem.detail.invoiceno = reader["invoiceno"].ToString();
                        }
                    }
                }

                if (reader.NextResult())
                {
                    while (reader.Read())
                    {
                        string mode= Convert.ToString(reader["mode"]);
                        string vouchername = mode == "S"? "Sales":"Payment";
                        if (vouchername == "Sales")
                        {
                            var billno = reader["billno"].ToString();
                            printableLedger.transactions.Add(new PrintableLedger.Transaction()
                            {
                                type = vouchername+" B2B Settlement",
                                date = Convert.ToDateTime(reader["billdate"]),
                                id = Convert.ToInt64(reader["id"]),
                                reference = reader["billno"].ToString(),
                                detail = new VoucherDisplayItem() { narration = reader["billno"].ToString() },
                                description = reader["billno"].ToString(),
                                debit = Convert.ToDecimal(reader["receivable"]).ToString("F2"),
                            });
                        }
                        else
                        {
                            if (reader["cash"] != null && Convert.ToDecimal(reader["cash"]) > 0)
                            {
                                printableLedger.transactions.Add(new PrintableLedger.Transaction()
                                {
                                    type = vouchername,
                                    date = Convert.ToDateTime(reader["billdate"]),
                                    id = Convert.ToInt64(reader["id"]),
                                    reference = reader["billno"].ToString(),
                                    detail = new VoucherDisplayItem() { narration = "Cash Paid for " + reader["billno"].ToString() },
                                    description = "Cash Paid for "+reader["billno"].ToString(),
                                    credit = Convert.ToDecimal(reader["cash"]).ToString("F2"),
                                });
                            }

                            if (reader["card"] != null && Convert.ToDecimal(reader["card"]) > 0)
                            {
                                printableLedger.transactions.Add(new PrintableLedger.Transaction()
                                {
                                    type = vouchername,
                                    date = Convert.ToDateTime(reader["billdate"]),
                                    id = Convert.ToInt64(reader["id"]),
                                    reference = reader["billno"].ToString(),
                                    detail = new VoucherDisplayItem() { narration = "Card Paid for " + reader["billno"].ToString() },
                                    description = "Card Paid for " + reader["billno"].ToString(),
                                    credit = Convert.ToDecimal(reader["card"]).ToString("F2"),
                                });
                            }

                            if (reader["upi_paid"] != null && Convert.ToDecimal(reader["upi_paid"]) > 0)
                            {
                                printableLedger.transactions.Add(new PrintableLedger.Transaction()
                                {
                                    type = vouchername,
                                    date = Convert.ToDateTime(reader["billdate"]),
                                    id = Convert.ToInt64(reader["id"]),
                                    reference = reader["billno"].ToString(),
                                    detail = new VoucherDisplayItem() { narration = "UPI Paid for " + reader["billno"].ToString() },
                                    description = "UPI Paid for " + reader["billno"].ToString(),
                                    credit = Convert.ToDecimal(reader["upi_paid"]).ToString("F2"),
                                });
                            }

                            if (reader["voucher_paid"] != null && Convert.ToDecimal(reader["voucher_paid"]) > 0)
                            {
                                printableLedger.transactions.Add(new PrintableLedger.Transaction()
                                {
                                    type = vouchername,
                                    date = Convert.ToDateTime(reader["billdate"]),
                                    id = Convert.ToInt64(reader["id"]),
                                    reference = reader["billno"].ToString(),
                                    detail = new VoucherDisplayItem() { narration = "Voucher Paid for " + reader["billno"].ToString() },
                                    description = "Voucher Paid for " + reader["billno"].ToString(),
                                    credit = Convert.ToDecimal(reader["voucher_paid"]).ToString("F2"),
                                });
                            }

                            if (reader["cash_refund"] != null && Convert.ToDecimal(reader["cash_refund"]) > 0)
                            {
                                printableLedger.transactions.Add(new PrintableLedger.Transaction()
                                {
                                    type = vouchername,
                                    date = Convert.ToDateTime(reader["billdate"]),
                                    id = Convert.ToInt64(reader["id"]),
                                    reference = reader["billno"].ToString(),
                                    detail = new VoucherDisplayItem() { narration = "Cash Refund for " + reader["billno"].ToString() },
                                    description = "Cash Refund for " + reader["billno"].ToString(),
                                    debit = Convert.ToDecimal(reader["cash_refund"]).ToString("F2"),
                                });
                            }
                        }
                    }
                }

                if (reader.NextResult())
                {
                    while (reader.Read())
                    {
                        bool iscredit = Convert.ToBoolean(reader["iscredit"]);
                        int vouchertypeid = Convert.ToInt32(reader["vouchertypeid"]);
                        string vouchername = "Advance Payment";
                        if (ledgerTypes.Exists(x => x.id == vouchertypeid))
                        {
                            vouchername = ledgerTypes.Find(x => x.id == vouchertypeid).name;
                        }

                        printableLedger.transactions.Add(new PrintableLedger.Transaction()
                        {
                            type = vouchername,
                            date = Convert.ToDateTime(reader["voucherdate"]),
                            id = Convert.ToInt64(reader["id"]),
                            reference = reader["voucherno"].ToString(),
                            createdby = reader["Createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["Createdby"]),
                            createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]),
                            modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]),
                            modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]),
                            detail = new VoucherDisplayItem() { narration = reader["narration"].ToString() },
                            description = reader["voucherno"].ToString() +" - "+ reader["displayname"].ToString(),
                            debit = !iscredit ? Convert.ToDecimal(reader["amount"]).ToString("F2") : "",
                            credit = iscredit ? Convert.ToDecimal(reader["amount"]).ToString("F2") : "",
                        });
                    }
                }

            }

            printableLedger.transactions = printableLedger.transactions.OrderBy(x => x.date).ToList();
            printableLedger.ProcessTotal();
            return printableLedger;
        }
        public List<SettlementReport> SearchCreditSaleCollection(RetailReportCreteria _reportcreteria)
        {
            List<SettlementReport> lst = new List<SettlementReport>();
            DateTime today = _reportcreteria.fromdate.Date;
            DateTime nextdate = _reportcreteria.todate.AddDays(1).Date;
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                command = GetCommand(SQL_Collection, conn);
                command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _reportcreteria.locationid;
                command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _reportcreteria.companyid;
                command.Parameters.Add("counterid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _reportcreteria.counterid;
                command.Parameters.Add("cashierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _reportcreteria.employeeid;
                command.Parameters.Add("fromdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = today;
                command.Parameters.Add("todate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = nextdate;
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        SettlementReport oitem = new SettlementReport();
                        oitem.mode = reader["paymentmode"] == DBNull.Value ? 0 : Convert.ToInt32(reader["paymentmode"]);
                        oitem.count = reader["noofpayments"] == DBNull.Value ? 0 : Convert.ToInt32(reader["noofpayments"]);
                        oitem.receivable = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"]);
                        oitem.discount = reader["discount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discount"]);
                        if (oitem.mode == 0)
                            oitem.paid_card = reader["paid"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["paid"]);
                        else if (oitem.mode == 1 || oitem.mode == 4)
                            oitem.paid_voucher = reader["paid"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["paid"]);
                        else if (oitem.mode == 2)
                            oitem.paid_cash = reader["paid"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["paid"]);
                        else if (oitem.mode == 3)
                            oitem.paid_upi = reader["paid"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["paid"]);
                        else
                            oitem.adjustment_paid = reader["paid"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["paid"]);
                        oitem.location = reader["location"] == DBNull.Value ? "" : reader["location"].ToString();
                        oitem.counter = reader["counter"] == DBNull.Value ? "" : reader["counter"].ToString();
                        oitem.employeename = reader["employeename"] == DBNull.Value ? "" : reader["employeename"].ToString();
                        oitem.companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"]);
                        oitem.customername = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
                        oitem.customerphoneno = reader["mobileno"] == DBNull.Value ? "" : reader["mobileno"].ToString();
                        oitem.billno = reader["billno"] == DBNull.Value ? "" : reader["billno"].ToString();
                        //oitem.receiptdate = reader["paidon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["paidon"]);
                        lst.Add(oitem);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _reportcreteria);
            }
            return lst;
        }

    }

}
