using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
namespace InfyPOS.Data
{
    public class LoansettlementDataService : SAASData
    {
        const string SQL_Search = @"select loansettlement.id, loansettlement.settlementon, loansettlement.billid, loansettlement.billsettlementmasterid, 
            loanproviderid, loansettlement.discount, loansettlement.customerid, customer.name as customer,customer.area,customer.mobileno,billdate,
            loanamount, paid, receiptno, referenceno,loansettlement. billno, loansettlement.remarks, loansettlement.createdon, loansettlement.modifiedon, 
            loansettlement.createdby, loansettlement.modifiedby, loansettlement.isactive, loansettlement.version, loansettlement.companyid,
            loansettlement.locationid, attributes from loansettlement
            inner join customer on customer.id = customerid
            where loansettlement.id=@id";
        const string SQL_SearchAll = @"select id, settlementon, billid, billsettlementmasterid, loanproviderid, discount, customerid, billdate,loanamount, paid, receiptno, referenceno, billno, remarks, createdon, modifiedon, createdby, modifiedby, isactive, version, companyid, locationid, attributes from loansettlement where billsettlementmasterid=@billsettlementmasterid";


        const string SQL_Insert = @"INSERT INTO public.loansettlement(settlementon, billid, billsettlementmasterid, loanproviderid, discount, customerid, loanamount, paid, receiptno, referenceno, billno, remarks, createdon, modifiedon, createdby, modifiedby, isactive, version, companyid, locationid, attributes,billdate) VALUES(@settlementon, @billid, @billsettlementmasterid, @loanproviderid, @discount, @customerid, @loanamount, @paid, @receiptno, @referenceno, @billno, @remarks, @createdon, @modifiedon, @createdby, @modifiedby, @isactive, @version, @companyid, @locationid, @attributes,@billdate) RETURNING id";

        public LoandSettlementPayment UpdatePayment(LoandSettlementPayment item)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    foreach (var pitem in item.items)
                    {
                        UpdatePayments(transaction, conn, pitem);
                    }
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, item);
                throw exp;
            }

            return item;
        }
        

        const string SQL_Update = @"UPDATE public.loansettlement SET settlementon=@settlementon, billid=@billid, billsettlementmasterid=@billsettlementmasterid, loanproviderid=@loanproviderid, discount=@discount, customerid=@customerid, loanamount=@loanamount, paid=@paid, receiptno=@receiptno, referenceno=@referenceno, billno=@billno, remarks=@remarks, modifiedon=@modifiedon, modifiedby=@modifiedby, version=@version+1, companyid=@companyid, locationid=@locationid, attributes=@attributes,billdate=@billdate where id=@id and version=@version RETURNING version";

        const string SQL_UpdatePayments = @"UPDATE public.loansettlement SET settlementon=@settlementon, discount=@discount, paid=@paid, receiptno=@receiptno, 
            referenceno=@referenceno,remarks=@remarks, modifiedon=@modifiedon, modifiedby=@modifiedby, version=@version+1 where id=@id";

        const string SQL_Delete = @"update public.loansettlement set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";
        const string SQL_DeleteAll = @"update public.loansettlement set isactive=false,modifiedby=@modifiedby,modifiedon=@modifiedon where billsettlementmasterid=@billsettlementmasterid";
        public List<LoansettlementDetail> Search(LoansettlementCreteria _loansettlement)
        {
            List<LoansettlementDetail> lst = new List<LoansettlementDetail>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_loansettlement.id != 0)
                {
                    command = GetCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _loansettlement.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("loansettlement.isactive", true, "=");

                    if(_loansettlement.customerid>0)
                    {
                        qb.AddParameter("loansettlement.customerid", _loansettlement.customerid, "=");
                    }
                    switch((LoansettlementCreteria.Modes)_loansettlement.searchmode)
                    {
                        case LoansettlementCreteria.Modes.FullPaid:
                            qb.Add2Query("paid>=loanamount-discount");
                            break;
                        case LoansettlementCreteria.Modes.OnlyPaid:
                            qb.Add2Query("paid>=0");
                            break;
                        case LoansettlementCreteria.Modes.PartialPaid:
                            qb.Add2Query("paid>0 and (loanamount-discount)-paid>0");
                            break;
                        case LoansettlementCreteria.Modes.Pending:
                            qb.Add2Query("(loanamount-discount)-paid>0");
                            break;
                    }

                    if(_loansettlement.fromdate.Year>2000)
                    {
                        if((LoansettlementCreteria.Modes)_loansettlement.searchmode == LoansettlementCreteria.Modes.OnlyPaid)
                        {
                            qb.AddParameter("settlementon", _loansettlement.fromdate, ">=");
                        }
                        else
                        {
                            qb.AddParameter("billdate", _loansettlement.fromdate, ">=");
                        }
                    }

                    if (_loansettlement.todate.Year > 2000)
                    {
                        if ((LoansettlementCreteria.Modes)_loansettlement.searchmode == LoansettlementCreteria.Modes.OnlyPaid)
                        {
                            qb.AddParameter("settlementon", _loansettlement.todate.AddDays(1), "<");
                        }
                        else
                        {
                            qb.AddParameter("billdate", _loansettlement.todate.AddDays(1), "<");
                        }
                    }

                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        LoansettlementDetail oloansettlement = new LoansettlementDetail();
                        oloansettlement.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        oloansettlement.settlementon = reader["settlementon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["settlementon"]);
                        oloansettlement.billdate = reader["billdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["billdate"]);
                        oloansettlement.billid = reader["billid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["billid"]);
                        oloansettlement.billsettlementmasterid = reader["billsettlementmasterid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["billsettlementmasterid"]);
                        oloansettlement.loanproviderid = reader["loanproviderid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["loanproviderid"]);
                        oloansettlement.discount = reader["discount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discount"]);
                        oloansettlement.customerid = reader["customerid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["customerid"]);
                        oloansettlement.loanamount = reader["loanamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["loanamount"]);
                        oloansettlement.paid = reader["paid"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["paid"]);
                        oloansettlement.receiptno = reader["receiptno"] == DBNull.Value ? "" : reader["receiptno"].ToString();
                        oloansettlement.referenceno = reader["referenceno"] == DBNull.Value ? "" : reader["referenceno"].ToString();
                        oloansettlement.billno = reader["billno"] == DBNull.Value ? "" : reader["billno"].ToString();
                        oloansettlement.remarks = reader["remarks"] == DBNull.Value ? "" : reader["remarks"].ToString();

                        oloansettlement.mobileno = reader["mobileno"] == DBNull.Value ? "" : reader["mobileno"].ToString();
                        oloansettlement.customer = reader["customer"] == DBNull.Value ? "" : reader["customer"].ToString();
                        oloansettlement.area = reader["area"] == DBNull.Value ? "" : reader["area"].ToString();

                        oloansettlement.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        oloansettlement.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        oloansettlement.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        oloansettlement.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        oloansettlement.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        oloansettlement.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        oloansettlement.companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"]);
                        oloansettlement.locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                        oloansettlement.attributes_json = reader["attributes"] == DBNull.Value ? null : reader["attributes"].ToString();
                        lst.Add(oloansettlement);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _loansettlement);
            }
            return lst;
        }

        public List<Loansettlement> SearchAll(long _billsettlementmasterid)
        {
            List<Loansettlement> lst = new List<Loansettlement>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = GetCommand(SQL_SearchAll, conn);
                command.Parameters.Add("billsettlementmasterid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _billsettlementmasterid;
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Loansettlement oloansettlement = new Loansettlement();
                        oloansettlement.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        oloansettlement.settlementon = reader["settlementon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["settlementon"]);
                        oloansettlement.billid = reader["billid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["billid"]);
                        oloansettlement.billsettlementmasterid = reader["billsettlementmasterid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["billsettlementmasterid"]);
                        oloansettlement.loanproviderid = reader["loanproviderid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["loanproviderid"]);
                        oloansettlement.customerid = reader["customerid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["customerid"]);
                        oloansettlement.billdate = reader["billdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["billdate"]);
                        oloansettlement.receiptno = reader["receiptno"] == DBNull.Value ? "" : reader["receiptno"].ToString();
                        oloansettlement.referenceno = reader["referenceno"] == DBNull.Value ? "" : reader["referenceno"].ToString();
                        oloansettlement.billno = reader["billno"] == DBNull.Value ? "" : reader["billno"].ToString();
                        oloansettlement.remarks = reader["remarks"] == DBNull.Value ? "" : reader["remarks"].ToString();
                        oloansettlement.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        oloansettlement.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        oloansettlement.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        oloansettlement.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        oloansettlement.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        oloansettlement.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        oloansettlement.companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"]);
                        oloansettlement.locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                        lst.Add(oloansettlement);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _billsettlementmasterid);
            }
            return lst;
        }

        public Loansettlement Insert(Loansettlement oLoansettlement)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);

                    Insert(transaction, conn, oLoansettlement);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oLoansettlement);
                throw exp;
            }

            return oLoansettlement;
        }

        public Loansettlement Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Loansettlement oLoansettlement)
        {

            NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("settlementon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oLoansettlement.settlementon;
            command.Parameters.Add("billdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oLoansettlement.billdate;
            command.Parameters.Add("billid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLoansettlement.billid;
            command.Parameters.Add("billsettlementmasterid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLoansettlement.billsettlementmasterid;
            command.Parameters.Add("loanproviderid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLoansettlement.loanproviderid;
            command.Parameters.Add("discount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLoansettlement.discount;
            command.Parameters.Add("customerid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLoansettlement.customerid;
            command.Parameters.Add("loanamount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLoansettlement.loanamount;
            command.Parameters.Add("paid", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLoansettlement.paid;
            command.Parameters.Add("receiptno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oLoansettlement.receiptno == null ? "" : oLoansettlement.receiptno;
            command.Parameters.Add("referenceno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oLoansettlement.referenceno == null ? "" : oLoansettlement.referenceno;
            command.Parameters.Add("billno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oLoansettlement.billno == null ? "" : oLoansettlement.billno;
            command.Parameters.Add("remarks", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oLoansettlement.remarks == null ? "" : oLoansettlement.remarks;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oLoansettlement.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oLoansettlement.modifiedon;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLoansettlement.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLoansettlement.modifiedby;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oLoansettlement.isactive;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oLoansettlement.version;
            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLoansettlement.companyid;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLoansettlement.locationid;
            command.Parameters.Add("attributes", NpgsqlTypes.NpgsqlDbType.Json).Value = oLoansettlement.attributes_json == null ? "null" : oLoansettlement.attributes_json;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oLoansettlement.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            return oLoansettlement;
        }

        public Loansettlement Update(Loansettlement oLoansettlement)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oLoansettlement);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oLoansettlement);
                throw exp;
            }

            return oLoansettlement;
        }

        public Loansettlement UpdatePayments(NpgsqlTransaction transaction, NpgsqlConnection conn, Loansettlement oLoansettlement)
        {

            NpgsqlCommand command = GetCommand(SQL_UpdatePayments, conn, transaction);

            command.Parameters.Add("settlementon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oLoansettlement.settlementon;
            command.Parameters.Add("discount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLoansettlement.discount;
            command.Parameters.Add("paid", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLoansettlement.paid;
            command.Parameters.Add("receiptno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oLoansettlement.receiptno == null ? "" : oLoansettlement.receiptno;
            command.Parameters.Add("referenceno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oLoansettlement.referenceno == null ? "" : oLoansettlement.referenceno;
            command.Parameters.Add("remarks", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oLoansettlement.remarks == null ? "" : oLoansettlement.remarks;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oLoansettlement.modifiedon;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLoansettlement.modifiedby;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oLoansettlement.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLoansettlement.id;

            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oLoansettlement.version = Convert.ToInt32(reader[0].ToString());
                }
            }

            return oLoansettlement;
        }

        public Loansettlement Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Loansettlement oLoansettlement)
        {

            NpgsqlCommand command = GetCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLoansettlement.id;
            command.Parameters.Add("billdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oLoansettlement.billdate;
            command.Parameters.Add("settlementon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oLoansettlement.settlementon;
            command.Parameters.Add("billid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLoansettlement.billid;
            command.Parameters.Add("billsettlementmasterid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLoansettlement.billsettlementmasterid;
            command.Parameters.Add("loanproviderid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLoansettlement.loanproviderid;
            command.Parameters.Add("discount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLoansettlement.discount;
            command.Parameters.Add("customerid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLoansettlement.customerid;
            command.Parameters.Add("loanamount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLoansettlement.loanamount;
            command.Parameters.Add("paid", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLoansettlement.paid;
            command.Parameters.Add("receiptno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oLoansettlement.receiptno == null ? "" : oLoansettlement.receiptno;
            command.Parameters.Add("referenceno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oLoansettlement.referenceno == null ? "" : oLoansettlement.referenceno;
            command.Parameters.Add("billno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oLoansettlement.billno == null ? "" : oLoansettlement.billno;
            command.Parameters.Add("remarks", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oLoansettlement.remarks == null ? "" : oLoansettlement.remarks;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oLoansettlement.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oLoansettlement.modifiedon;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLoansettlement.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLoansettlement.modifiedby;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oLoansettlement.isactive;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oLoansettlement.version;
            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLoansettlement.companyid;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLoansettlement.locationid;
            command.Parameters.Add("attributes", NpgsqlTypes.NpgsqlDbType.Json).Value = oLoansettlement.attributes_json == null ? "null" : oLoansettlement.attributes_json;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oLoansettlement.version = Convert.ToInt32(reader[0].ToString());
                }
            }

            return oLoansettlement;
        }

        public bool Update(NpgsqlTransaction transaction, NpgsqlConnection conn, List<Loansettlement> list, long id)
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
                    item.billsettlementmasterid = id;
                    Insert(transaction, conn, item);
                }
            }
            return true;
        }

        public bool Delete(Loansettlement oLoansettlement)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oLoansettlement);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oLoansettlement);

                throw exp;
            }
            return hasSuccessCount;

        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Loansettlement oLoansettlement)
        {

            NpgsqlCommand command = GetCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oLoansettlement.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLoansettlement.modifiedby;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oLoansettlement.modifiedon;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oLoansettlement.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLoansettlement.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

        public bool DeleteAll(long billsettlementmasterid, long userid, NpgsqlConnection conn, NpgsqlTransaction transaction)
        {


            int successCount = 0;
            NpgsqlCommand command = GetCommand(SQL_DeleteAll, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = false;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = userid;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("billsettlementmasterid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = billsettlementmasterid;
            successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

    }

}
