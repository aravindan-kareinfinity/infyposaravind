using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
namespace InfyPOS.Data

{
    public class LeadmanagementDataService : SAASData
    {
        const string SQL_Search = @"with consumption as (select leadmanagementid, count(id) count from leadmanagementitems where billid > 0 or billsettlementmasterid > 0
                    group by leadmanagementid)
                select leadmanagement.id, leadno, customerid, eventtypeid, assignedbyid, assignedid, leadmanagement.companyid, leadmanagement.locationid, leadmanagement.counterid,
                leadmanagement.statusid, leadmanagement.priorityid, leadmanagement.remark, loggedon, eventon, followupon, leadmanagement.version, 
                leadmanagement.isactive, leadmanagement.createdby, leadmanagement.modifiedby, leadmanagement.createdon, billno, billdate,
                leadmanagement.modifiedon, calllog, customer.name as customername, customer.mobileno as mobileno, consumption.count consumed,
                eventtype.name eventtypename, priority.name priorityname, status.name statusname
                from leadmanagement inner join customer on customer.id = leadmanagement.customerid 
                left outer join referencelist eventtype on eventtype.id = eventtypeid
                left outer join referencelist priority on priority.id = priorityid
                left outer join referencelist status on status.id = statusid
                left outer join consumption on consumption.leadmanagementid = leadmanagement.id {0}";


        const string SQL_Insert = @"INSERT INTO public.leadmanagement(leadno, customerid, eventtypeid, assignedbyid, assignedid, companyid, locationid, counterid, statusid, priorityid, remark, loggedon, eventon, followupon, version, isactive, createdby, modifiedby, createdon, modifiedon, calllog, billno, billdate) VALUES(@leadno, @customerid, @eventtypeid, @assignedbyid, @assignedid, @companyid, @locationid, @counterid, @statusid, @priorityid, @remark, @loggedon, @eventon, @followupon, @version, @isactive, @createdby, @modifiedby, @createdon, @modifiedon, @calllog, @billno, @billdate) RETURNING id";


        const string SQL_Update = @"UPDATE public.leadmanagement SET customerid=@customerid, eventtypeid=@eventtypeid, assignedbyid=@assignedbyid, assignedid=@assignedid, companyid=@companyid, locationid=@locationid, counterid=@counterid, statusid=@statusid, priorityid=@priorityid, remark=@remark, loggedon=@loggedon, eventon=@eventon, followupon=@followupon, version=@version+1, isactive=@isactive, modifiedby=@modifiedby, modifiedon=@modifiedon, calllog=@calllog, billno=@billno, billdate=@billdate where id=@id and version=@version RETURNING version";

        const string SQL_Delete = @"update public.leadmanagement set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";

        public List<Leadmanagement> Search(LeadmanagementCreteria _leadmanagement)
        {
            List<Leadmanagement> lst = new List<Leadmanagement>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_leadmanagement.id != 0)
                {
                    command = new NpgsqlCommand(string.Format(SQL_Search, " where leadmanagement.id=@id"), conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _leadmanagement.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = string.Format(SQL_Search, " where leadmanagement.isactive=true ");

                    if (!string.IsNullOrEmpty(_leadmanagement.leadno))
                        qb.AddParameter("leadno", _leadmanagement.leadno, "=");
                    if (_leadmanagement.companyid > 0)
                        qb.AddParameter("leadmanagement.companyid", _leadmanagement.companyid, "=");
                    if (_leadmanagement.statusid != 0)
                        qb.AddParameter("leadmanagement.statusid", _leadmanagement.statusid, "=");
                    else
                        qb.AddParameter("leadmanagement.statusid", -1, "!=");
                    if (_leadmanagement.priorityid > 0)
                        qb.AddParameter("leadmanagement.priorityid", _leadmanagement.priorityid, "=");
                    if (_leadmanagement.eventtypeid > 0)
                        qb.AddParameter("leadmanagement.eventtypeid", _leadmanagement.eventtypeid, "=");
                    if (_leadmanagement.assignedbyid > 0)
                        qb.AddParameter("leadmanagement.assignedbyid", _leadmanagement.assignedbyid, "=");
                    if (_leadmanagement.assignedid > 0)
                        qb.AddParameter("leadmanagement.assignedid", _leadmanagement.assignedid, "=");
                    if (_leadmanagement.fromdate != DateTime.MinValue && _leadmanagement.todate != DateTime.MinValue)
                    {
                        qb.AddParameter("loggedon", "datefrom", _leadmanagement.fromdate.Date, ">=");
                        qb.AddParameter("loggedon", "dateto", _leadmanagement.todate.Date.AddDays(1), "<");
                    }

                    qb.SortField = "leadmanagement.id desc";
                    qb.Limit = 1000;
                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Leadmanagement oLeadmanagement = new Leadmanagement();
                        oLeadmanagement.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        oLeadmanagement.leadno = reader["leadno"] == DBNull.Value ? "" : reader["leadno"].ToString();
                        oLeadmanagement.customername = reader["customername"] == DBNull.Value ? "" : reader["customername"].ToString();
                        oLeadmanagement.contactno = reader["mobileno"] == DBNull.Value ? "" : reader["mobileno"].ToString();
                        oLeadmanagement.customerid = reader["customerid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["customerid"]);
                        oLeadmanagement.eventtypeid = reader["eventtypeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["eventtypeid"]);
                        oLeadmanagement.assignedbyid = reader["assignedbyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["assignedbyid"]);
                        oLeadmanagement.assignedid = reader["assignedid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["assignedid"]);
                        oLeadmanagement.companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"]);
                        oLeadmanagement.locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                        oLeadmanagement.counterid = reader["counterid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["counterid"]);
                        oLeadmanagement.priorityid = reader["priorityid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["priorityid"]);
                        oLeadmanagement.statusid = reader["statusid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["statusid"]);
                        oLeadmanagement.remark = reader["remark"] == DBNull.Value ? "" : reader["remark"].ToString();
                        oLeadmanagement.loggedon = reader["loggedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["loggedon"]);
                        oLeadmanagement.eventon = reader["eventon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["eventon"]);
                        oLeadmanagement.followupon = reader["followupon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["followupon"]);
                        oLeadmanagement.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        oLeadmanagement.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        oLeadmanagement.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        oLeadmanagement.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        oLeadmanagement.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        oLeadmanagement.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        oLeadmanagement.calllog_json = reader["calllog"] == DBNull.Value ? null : reader["calllog"].ToString();
                        oLeadmanagement.isconsumed = (reader["consumed"] == DBNull.Value || Convert.ToInt64(reader["consumed"]) == 0) ? false : true;
                        oLeadmanagement.billno = reader["billno"] == DBNull.Value ? "" : reader["billno"].ToString();
                        oLeadmanagement.billdate = reader["billdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["billdate"]);
                        oLeadmanagement.eventtypename = reader["eventtypename"] == DBNull.Value ? "" : reader["eventtypename"].ToString();
                        oLeadmanagement.priorityname = reader["priorityname"] == DBNull.Value ? "" : reader["priorityname"].ToString();
                        oLeadmanagement.statusname = reader["statusname"] == DBNull.Value ? "" : reader["statusname"].ToString();
                        lst.Add(oLeadmanagement);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _leadmanagement);
            }
            return lst;
        }

        public Leadmanagement Insert(Leadmanagement oLeadmanagement)
        {
            long _leadcouponid = 0;
            long _leadvoucherid = 0;
            if (oLeadmanagement.customerid == 0)
            {
                oLeadmanagement.ErrorMessage = "Please enter valid customer details.";
                return oLeadmanagement;
            }

            if (oLeadmanagement.leadcoupon != null && oLeadmanagement.leadcoupon.rules != null 
                && oLeadmanagement.leadcoupon.rules.Count > 0)
            {
                GiftCouponDataService CouponData = new GiftCouponDataService() { Context = this.Context };
                CouponData.Insert(oLeadmanagement.leadcoupon);
                if (oLeadmanagement.leadcoupon.id == 0 || !string.IsNullOrEmpty(oLeadmanagement.leadcoupon.errormessage))
                {
                    oLeadmanagement.ErrorMessage = oLeadmanagement.leadcoupon.errormessage;
                    return oLeadmanagement;
                }
                _leadcouponid = oLeadmanagement.leadcoupon.id;
            }

            if (oLeadmanagement.leadvoucher != null)
            {
                oLeadmanagement.leadvoucher.customerid = oLeadmanagement.customerid;
                oLeadmanagement.leadvoucher.counterid = oLeadmanagement.counterid;
                oLeadmanagement.leadvoucher.issuedby = oLeadmanagement.assignedbyid;
                GiftCouponDataService CouponData = new GiftCouponDataService() { Context = this.Context };
                CouponData.Insert(oLeadmanagement.leadvoucher);
                if (oLeadmanagement.leadvoucher.id == 0 || !string.IsNullOrEmpty(oLeadmanagement.leadvoucher.errormessage))
                {
                    oLeadmanagement.ErrorMessage = oLeadmanagement.leadvoucher.errormessage;
                    return oLeadmanagement;
                }
                _leadvoucherid = oLeadmanagement.leadvoucher.id;
            }

            NpgsqlTransaction transaction = null;
            try
            {
                var sequence = SequenceManager.Instance(Context.OrganizationID).Get("leadmanagement.leadno", 0);
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);
                    oLeadmanagement.leadno = sequence.GetNextNumber(Context,conn, transaction);

                    Insert(transaction, conn, oLeadmanagement);
                    if (oLeadmanagement.id > 0 && _leadcouponid > 0)
                        InsertLeadItems(transaction, conn, oLeadmanagement, _leadcouponid, true);
                    if (oLeadmanagement.id > 0 && _leadvoucherid > 0)
                        InsertLeadItems(transaction, conn, oLeadmanagement, _leadvoucherid, false);

                    transaction.Commit();
                    conn.Close();

                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oLeadmanagement);
            }

            return oLeadmanagement;
        }

        public Leadmanagement Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Leadmanagement oLeadmanagement)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("leadno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oLeadmanagement.leadno == null ? "" : oLeadmanagement.leadno;
            command.Parameters.Add("customerid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLeadmanagement.customerid;
            command.Parameters.Add("eventtypeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLeadmanagement.eventtypeid;
            command.Parameters.Add("assignedbyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLeadmanagement.assignedbyid;
            command.Parameters.Add("assignedid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLeadmanagement.assignedid;
            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLeadmanagement.companyid;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLeadmanagement.locationid;
            command.Parameters.Add("counterid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLeadmanagement.counterid;
            command.Parameters.Add("priorityid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLeadmanagement.priorityid;
            command.Parameters.Add("statusid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLeadmanagement.statusid;
            command.Parameters.Add("remark", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oLeadmanagement.remark == null ? "" : oLeadmanagement.remark;
            command.Parameters.Add("loggedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oLeadmanagement.loggedon;
            command.Parameters.Add("eventon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oLeadmanagement.eventon;
            command.Parameters.Add("followupon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oLeadmanagement.followupon;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = true;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oLeadmanagement.version;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("calllog", NpgsqlTypes.NpgsqlDbType.Json).Value = oLeadmanagement.calllog_json == null ? "null" : oLeadmanagement.calllog_json;
            command.Parameters.Add("billno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oLeadmanagement.billno == null ? "" : oLeadmanagement.billno;
            command.Parameters.Add("billdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oLeadmanagement.billdate;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oLeadmanagement.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            return oLeadmanagement;
        }

        public Leadmanagement Update(Leadmanagement oLeadmanagement)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oLeadmanagement);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oLeadmanagement);
            }

            return oLeadmanagement;
        }

        public Leadmanagement Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Leadmanagement oLeadmanagement)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLeadmanagement.id;
            command.Parameters.Add("contactno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oLeadmanagement.contactno == null ? "" : oLeadmanagement.contactno;
            command.Parameters.Add("customerid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLeadmanagement.customerid;
            command.Parameters.Add("eventtypeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLeadmanagement.eventtypeid;
            command.Parameters.Add("assignedbyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLeadmanagement.assignedbyid;
            command.Parameters.Add("assignedid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLeadmanagement.assignedid;
            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLeadmanagement.companyid;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLeadmanagement.locationid;
            command.Parameters.Add("counterid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLeadmanagement.counterid;
            command.Parameters.Add("priorityid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLeadmanagement.priorityid;
            command.Parameters.Add("statusid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLeadmanagement.statusid;
            command.Parameters.Add("remark", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oLeadmanagement.remark == null ? "" : oLeadmanagement.remark;
            command.Parameters.Add("loggedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oLeadmanagement.loggedon;
            command.Parameters.Add("eventon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oLeadmanagement.eventon;
            command.Parameters.Add("followupon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oLeadmanagement.followupon;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = true;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oLeadmanagement.version;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("calllog", NpgsqlTypes.NpgsqlDbType.Json).Value = oLeadmanagement.calllog_json == null ? "null" : oLeadmanagement.calllog_json;
            command.Parameters.Add("billno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oLeadmanagement.billno == null ? "" : oLeadmanagement.billno;
            command.Parameters.Add("billdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oLeadmanagement.billdate;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oLeadmanagement.version = Convert.ToInt32(reader[0].ToString());
                }
            }

            return oLeadmanagement;
        }


        public bool Delete(Leadmanagement oLeadmanagement)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oLeadmanagement);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oLeadmanagement);
                
            }
            return hasSuccessCount;

        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Leadmanagement oLeadmanagement)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oLeadmanagement.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oLeadmanagement.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLeadmanagement.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }
        public bool InsertLeadItems(NpgsqlTransaction transaction, NpgsqlConnection conn, Leadmanagement oLeadmanagement,
            long voucherid, bool isCoupon)
        {
            string _InsertSQL = @"insert into leadmanagementitems(isactive, leadmanagementid, vouchertypeid, couponid, voucherid, billid, billsettlementmasterid, vouchercode) 
                                    select true, @leadmanagementid, @vouchertypeid, coupenid, id, 0, 0, code from vouchers where isactive=true and coupenid=@coupenid;";

            NpgsqlCommand command = new NpgsqlCommand(_InsertSQL, conn, transaction);
            command.Parameters.Add("vouchertypeid", NpgsqlTypes.NpgsqlDbType.Integer).Value = (isCoupon ? 1 : 2);
            command.Parameters.Add("leadmanagementid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLeadmanagement.id;
            command.Parameters.Add("coupenid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = voucherid;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }
        public bool UpdateLeadStatus(NpgsqlTransaction transaction, NpgsqlConnection conn, string code, long billid, bool isCoupon)
        {
            string UpdateStatusSQL = @"update public.leadmanagementitems set {0} where vouchercode=@code;";

            NpgsqlCommand command = new NpgsqlCommand(string.Format(UpdateStatusSQL, isCoupon ? "billid=@billid" : "billsettlementmasterid=@billid"), conn, transaction);
            command.Parameters.Add("billid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = billid;
            command.Parameters.Add("code", NpgsqlTypes.NpgsqlDbType.Varchar).Value = code;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;

        }
    }

}
