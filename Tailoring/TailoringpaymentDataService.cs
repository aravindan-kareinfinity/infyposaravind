using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
namespace InfyPOS.Data

{
    public class TailoringpaymentDataService : BaseData
    {
        const string SQL_Search = @"
        select tailoringpayment.id, tailoringid, paidby, paymentcode, Tailoringitemid, tailoringpayment.tailorid,
        tailoringpayment.customerid, collectedby, tailoringpayment.companyid, tailoringpayment.locationid,
        paymode, bankid, amount, refund, collectedon, tailoringpayment.createdon, paidbycustomer,
        tailoringpayment.version, tailoringpayment.isactive, tailoringpayment.modifiedby, tailoringpayment.createdby,
        tailoringpayment.modifiedon,
        tailor.name tailorname,tailoringorder.receivedon orderdate,
        tailoringorder.entryno, tailoringorder.orderno, tailoringorder.customername,
        Tailoringitems.charge, Tailoringitems.product, measurementname, Tailoringitems.receivedqty,
        Tailoringitems.delaycharge, Tailoringitems.prioritycharge, priorityqty
        from tailoringpayment
        inner join tailor on tailor.id = tailorid
        inner join tailoringorder on tailoringorder.id = tailoringid
        inner join Tailoringitems on Tailoringitems.id = Tailoringitemid
        where tailoringpayment.id=@id";



        const string SQL_Insert = @"INSERT INTO public.tailoringpayment(tailoringid, paidby, paymentcode,Tailoringitemid,tailorid, customerid, collectedby, companyid, locationid, paymode,bankid, amount, refund, collectedon, createdon, paidbycustomer, version, isactive, modifiedby, createdby, modifiedon) 
                                    VALUES(@tailoringid, @paidby, @paymentcode,@Tailoringitemid,@tailorid, @customerid, @collectedby, @companyid, @locationid, @paymode,@bankid, @amount, @refund, @collectedon, @createdon, @paidbycustomer, @version, @isactive, @modifiedby, @createdby, @modifiedon) RETURNING id";

        const string SQL_Update = @"UPDATE public.tailoringpayment SET tailoringid=@tailoringid, paidby=@paidby, paymentcode=@paymentcode, 
                                    Tailoringitemid=@Tailoringitemid,tailorid=@tailorid,customerid=@customerid, collectedby=@collectedby, bankid=@bankid,
                                    companyid=@companyid, locationid=@locationid, paymode=@paymode, amount=@amount, refund=@refund, collectedon=@collectedon, paidbycustomer=@paidbycustomer, version=@version+1, modifiedby=@modifiedby, modifiedon=@modifiedon where id=@id and version=@version RETURNING version";

        const string SQL_Delete = @"update public.tailoringpayment set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";

        public List<TailoringPaymentDetail> Search(TailoringpaymentCreteria _tailoringpayment)
        {
            List<TailoringPaymentDetail> lst = new List<TailoringPaymentDetail>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_tailoringpayment.id != 0)
                {
                    command = new NpgsqlCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _tailoringpayment.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("Tailoringpayment.isactive", true, "=");
                    if (!string.IsNullOrEmpty(_tailoringpayment.paymentcode))
                    {
                        qb.AddParameter("Tailoringpayment.paymentcode", _tailoringpayment.paymentcode, "=");
                    }
                    else
                    {
                        if (_tailoringpayment.fromdate.Year > 1900)
                            qb.AddParameter("Tailoringpayment.collectedon", "fromdate", _tailoringpayment.fromdate, ">=");
                        if (_tailoringpayment.fromdate.Year > 1900)
                            qb.AddParameter("Tailoringpayment.collectedon", "todate", _tailoringpayment.todate.Date.AddDays(1), "<");
                        if (_tailoringpayment.companyid > 0)
                            qb.AddParameter("Tailoringpayment.companyid", _tailoringpayment.companyid, "=");
                        if (_tailoringpayment.locationid > 0)
                            qb.AddParameter("Tailoringpayment.locationid", _tailoringpayment.locationid, "=");
                        if (_tailoringpayment.tailorid > 0)
                            qb.AddParameter("Tailoringpayment.tailorid", _tailoringpayment.tailorid, "=");
                        if (_tailoringpayment.tailoringorderid > 0)
                            qb.AddParameter("Tailoringpayment.tailoringid", _tailoringpayment.tailoringorderid, "=");
                    }
                    //if (!string.IsNullOrEmpty(_tailoringpayment.Code))
                    //    qb.AddParameter("code", _tailoringpayment.Code, "=");

                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        TailoringPaymentDetail otailoringpayment = new TailoringPaymentDetail();
                        otailoringpayment.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        otailoringpayment.tailoringorderid = reader["tailoringid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["tailoringid"]);
                        otailoringpayment.Tailoringitemid = reader["Tailoringitemid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["Tailoringitemid"]);
                        otailoringpayment.tailorid = reader["tailorid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["tailorid"]);
                        otailoringpayment.paidby = reader["paidby"] == DBNull.Value ? "" : reader["paidby"].ToString();
                        otailoringpayment.paymentcode = reader["paymentcode"] == DBNull.Value ? "" : reader["paymentcode"].ToString();
                        otailoringpayment.customerid = reader["customerid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["customerid"]);
                        otailoringpayment.collectedby = reader["collectedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["collectedby"]);
                        otailoringpayment.companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"]);
                        otailoringpayment.locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                        otailoringpayment.paymode = reader["paymode"] == DBNull.Value ? 0 : Convert.ToInt32(reader["paymode"]);
                        otailoringpayment.bankid = reader["bankid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["bankid"]);
                        otailoringpayment.amount = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"]);
                        otailoringpayment.refund = reader["refund"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["refund"]);
                        otailoringpayment.collectedon = reader["collectedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["collectedon"]);
                        otailoringpayment.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        otailoringpayment.paidbycustomer = reader["paidbycustomer"] == DBNull.Value ? false : Convert.ToBoolean(reader["paidbycustomer"]);
                        otailoringpayment.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        otailoringpayment.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        otailoringpayment.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        otailoringpayment.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        otailoringpayment.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);



                        otailoringpayment.orderdate = reader["orderdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["orderdate"]);
                        otailoringpayment.tailorname = reader["tailorname"] == DBNull.Value ? "" : reader["tailorname"].ToString();
                        otailoringpayment.entryno = reader["entryno"] == DBNull.Value ? "" : reader["entryno"].ToString();
                        otailoringpayment.orderno = reader["orderno"] == DBNull.Value ? "" : reader["orderno"].ToString();
                        otailoringpayment.customername = reader["customername"] == DBNull.Value ? "" : reader["customername"].ToString();
                        otailoringpayment.product = reader["product"] == DBNull.Value ? "" : reader["product"].ToString();
                        otailoringpayment.measurementname = reader["measurementname"] == DBNull.Value ? "" : reader["measurementname"].ToString();

                        otailoringpayment.receivedqty = reader["receivedqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["receivedqty"]);
                        otailoringpayment.charge = reader["charge"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["charge"]);
                        otailoringpayment.delaycharge = reader["delaycharge"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["delaycharge"]);
                        otailoringpayment.prioritycharge = reader["prioritycharge"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["prioritycharge"]);
                        otailoringpayment.priorityqty = reader["priorityqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["priorityqty"]);


                        lst.Add(otailoringpayment);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _tailoringpayment);
            }
            return lst;
        }

        public Tailoringpayment Insert(Tailoringpayment oTailoringpayment)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                var sequence = SequenceManager.Instance(Context.OrganizationID).Get("tailoringpayment.paymentcode", oTailoringpayment.locationid);
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);
                    oTailoringpayment.paymentcode = sequence.GetNextNumber(Context, conn, transaction);

                    Insert(transaction, conn, oTailoringpayment);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oTailoringpayment);

            }

            return oTailoringpayment;
        }

        public Tailoringpayment Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Tailoringpayment oTailoringpayment)
        {
            if (oTailoringpayment.companyid == 0)
                throw new Exception("Please select the company");
            if (oTailoringpayment.locationid == 0)
                throw new Exception("Please select the location");
            if(oTailoringpayment.collectedon.Year<DateTimeNow.Year-3)
                throw new Exception("Please select the payment date");
            NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("tailoringid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTailoringpayment.tailoringorderid;
            command.Parameters.Add("Tailoringitemid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTailoringpayment.Tailoringitemid;
            command.Parameters.Add("tailorid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTailoringpayment.tailorid;
            command.Parameters.Add("paidby", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oTailoringpayment.paidby == null ? "" : oTailoringpayment.paidby;
            command.Parameters.Add("paymentcode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oTailoringpayment.paymentcode == null ? "" : oTailoringpayment.paymentcode;
            command.Parameters.Add("customerid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTailoringpayment.customerid;
            command.Parameters.Add("collectedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTailoringpayment.collectedby;
            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTailoringpayment.companyid;
            command.Parameters.Add("bankid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTailoringpayment.bankid;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTailoringpayment.locationid;
            command.Parameters.Add("paymode", NpgsqlTypes.NpgsqlDbType.Integer).Value = oTailoringpayment.paymode;
            command.Parameters.Add("amount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oTailoringpayment.amount;
            command.Parameters.Add("refund", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oTailoringpayment.refund;
            command.Parameters.Add("collectedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oTailoringpayment.collectedon;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oTailoringpayment.createdon;
            command.Parameters.Add("paidbycustomer", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oTailoringpayment.paidbycustomer;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oTailoringpayment.version;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oTailoringpayment.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTailoringpayment.modifiedby;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTailoringpayment.createdby;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oTailoringpayment.modifiedon;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oTailoringpayment.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            return oTailoringpayment;
        }

        public Tailoringpayment Update(Tailoringpayment oTailoringpayment)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oTailoringpayment);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oTailoringpayment);

            }

            return oTailoringpayment;
        }

        public Tailoringpayment Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Tailoringpayment oTailoringpayment)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTailoringpayment.id;
            command.Parameters.Add("tailoringid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTailoringpayment.tailoringorderid;
            command.Parameters.Add("Tailoringitemid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTailoringpayment.Tailoringitemid;
            command.Parameters.Add("tailorid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTailoringpayment.tailorid;
            command.Parameters.Add("paidby", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oTailoringpayment.paidby == null ? "" : oTailoringpayment.paidby;
            command.Parameters.Add("paymentcode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oTailoringpayment.paymentcode == null ? "" : oTailoringpayment.paymentcode;
            command.Parameters.Add("customerid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTailoringpayment.customerid;
            command.Parameters.Add("collectedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTailoringpayment.collectedby;
            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTailoringpayment.companyid;
            command.Parameters.Add("bankid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTailoringpayment.bankid;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTailoringpayment.locationid;
            command.Parameters.Add("paymode", NpgsqlTypes.NpgsqlDbType.Integer).Value = oTailoringpayment.paymode;
            command.Parameters.Add("amount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oTailoringpayment.amount;
            command.Parameters.Add("refund", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oTailoringpayment.refund;
            command.Parameters.Add("collectedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oTailoringpayment.collectedon;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oTailoringpayment.createdon;
            command.Parameters.Add("paidbycustomer", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oTailoringpayment.paidbycustomer;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oTailoringpayment.version;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oTailoringpayment.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTailoringpayment.modifiedby;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTailoringpayment.createdby;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oTailoringpayment.modifiedon;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oTailoringpayment.version = Convert.ToInt32(reader[0].ToString());
                }
            }

            return oTailoringpayment;
        }


        public bool Delete(Tailoringpayment oTailoringpayment)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oTailoringpayment);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oTailoringpayment);

            }
            return hasSuccessCount;

        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Tailoringpayment oTailoringpayment)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oTailoringpayment.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTailoringpayment.modifiedby;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oTailoringpayment.modifiedon;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oTailoringpayment.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTailoringpayment.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

        internal bool Update(NpgsqlTransaction transaction, NpgsqlConnection conn, List<TailoringPaymentDetail> list, long id)
        {
            if (list == null) list = new List<TailoringPaymentDetail>();
            var existingItems = Search(new TailoringpaymentCreteria()
            {
                tailoringorderid = id
            });
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
                    item.tailoringorderid = id;
                    Insert(transaction, conn, item);
                }
            }
            return true;
        }


        internal TailoringPaymentPrint Print(string paymentcode)
        {
            var items = Search(new TailoringpaymentCreteria()
            {
                paymentcode = paymentcode
            });
            if (items.Count == 0) return null;
            var print = new TailoringPaymentPrint()
            {
                collectedon = items[0].collectedon,
                tailorname = items[0].tailorname,
                items = items,
                totalamount = items.Sum(x=>x.amount),
                totaldelaycharge = items.Sum(x=>x.delaycharge),
                totalprioritycharge = items.Sum(x => x.prioritycharge),
                totalpriorityqty = items.Sum(x => x.priorityqty),
                totalqty = items.Sum(x => x.receivedqty),
            };
            CompanyDataService companyDataService = new CompanyDataService() { Context = this.Context };
            print.company = companyDataService.Search(new CompanyCreteria() { id = items[0].companyid }).First();

            ReferencelistDataService referencelistDataService = new ReferencelistDataService() { Context = this.Context };
            var location = referencelistDataService.Search(new ReferencelistCreteria()
            {
                id = items[0].locationid
            }).First();

            print.location = new CompanyLocation(location);
            return print;
        }

        internal bool Cancel(List<Tailoringpayment> items)
        {
            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    var ids = string.Join(",", items.ConvertAll(x => x.id));
                    string reversepayment = string.Format( @"with tailoringpaymentcancels
                    as
                    (
	                    select tailoringitemid,amount from tailoringpayment where id in ({0})
                    )
                    update tailoringitems set paidamount=paidamount-tailoringpaymentcancels.amount
                    from tailoringpaymentcancels 
                    where tailoringitems.id = tailoringpaymentcancels.tailoringitemid",
                    ids);

                    ExecuteQuery(reversepayment, null, conn, transaction);
                    reversepayment = string.Format(@"update tailoringpayment set isactive=false,modifiedby=@modifiedby,modifiedon=@modifiedon where id in ({0})", ids);

                    var param = new SortedList<string, object>();
                    param.Add("modifiedon", DateTimeNow);
                    param.Add("modifiedby", Context.UserID);
                    hasSuccessCount = ExecuteQuery(reversepayment, param, conn, transaction)>0;

                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, items);

            }
            return hasSuccessCount;
        }
    }

}
