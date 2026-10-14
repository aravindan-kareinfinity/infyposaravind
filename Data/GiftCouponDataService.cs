using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
namespace InfyPOS.Data

{
    public class GiftCouponDataService : SAASData
    {
        const string SQL_Search = @"select id, createdby, modifiedby, createdon, modifiedon, isactive, version, name, mobileno, validfrom, validto, status, customerid, receivedamt, 
        vouchervalue, issuedby, denomination, givento, issuedon,printedon, paymentmode, bankid,ledgerid,paymentreference,locationid,counterid,
        seqcode,issuemode,discountpercentage,discountvalue,companyid from coupons where id=@id";

        const string SQL_SearchVouchersList = @"select name,mobileno,amount,code from coupons inner join vouchers on vouchers.coupenid= coupons.id";

        const string SQL_SearchCouponList = @"select coupons.name, customer.mobileno, customer.name customername, amount, vouchers.issuedon, vouchers.validfrom, validupto, 
                                                coupons.name issuetoname, coupons.mobileno issuetomobileno, vouchers.code,amountaspercentage, billno, billdate,
                                                cast(billattributes->>'couponamount' as numeric) discount,
                                                issuelocation.name issuelocation, issuecounter.name issuecounter, (bill.receivable+bill.rounding) billvalue
                                                from coupons inner join vouchers on vouchers.coupenid = coupons.id
                                                left outer join bill on bill.id = vouchers.billid
                                                left outer join customer on customer.id = bill.customerid 
                                                left outer join referencelist issuelocation on issuelocation.id = vouchers.issuelocationid
                                                left outer join referencelist issuecounter on issuecounter.id = vouchers.issuecounterid ";

        const string SQL_SearchCreditNoteList = @"with billdetail as (
select code, billid, customerid, billno, billdate from vouchers inner join bill on bill.id = vouchers.billid
where vouchers.vouchertype=4
)
select coupons.name, customer.mobileno, customer.name customername, amount, vouchers.issuedon, validupto, 
coupons.name issuetoname, coupons.mobileno issuetomobileno, vouchers.code,amountaspercentage, billno, billdate,vouchers.vouchertype,
counter.name countername, employee.name issuedbyname
from coupons inner join vouchers on vouchers.coupenid = coupons.id
left outer join billdetail on billdetail.code = vouchers.code
left outer join customer on customer.id = billdetail.customerid
left outer join referencelist counter on counter.id = coupons.counterid
left outer join employee on employee.id = coupons.issuedby
";

        const string SQL_SearchBirthdayCouponList = @"select 'BIRTHDAY COUPON - SYSTEM' as name, customer.mobileno, customer.name customername, 
                                                amount, vouchers.code,amountaspercentage, billno, billdate, vouchers.issuedon, vouchers.validfrom, validupto, 
                                                cast(billattributes->>'couponamount' as numeric) discount,
                                                issueto.name issuetoname, issueto.mobileno issuetomobileno,
                                                issuelocation.name issuelocation, issuecounter.name issuecounter, (bill.receivable+bill.rounding) billvalue
                                                from vouchers left outer join bill on bill.id = vouchers.billid
                                                left outer join customer on customer.id = bill.customerid 
                                                left outer join customer issueto on issueto.id = vouchers.issueto 
                                                left outer join referencelist issuelocation on issuelocation.id = vouchers.issuelocationid
                                                left outer join referencelist issuecounter on issuecounter.id = vouchers.issuecounterid
                                                where vouchers.isactive=True 
												And vouchers.vouchertype=5 And vouchers.coupenid=0";

        const string SQL_SearchSettlementSchemeCouponList = @"select 'SETTLEMENT SCHEME - SYSTEM' as name, customer.mobileno, customer.name customername, 
                                                amount, vouchers.code,amountaspercentage, billno, billdate, vouchers.issuedon, vouchers.validfrom, validupto, 
                                                cast(billattributes->>'couponamount' as numeric) discount,
                                                issueto.name issuetoname, issueto.mobileno issuetomobileno,
                                                issuelocation.name issuelocation, issuecounter.name issuecounter, (bill.receivable+bill.rounding) billvalue
                                                from vouchers left outer join bill on bill.id = vouchers.billid
                                                left outer join customer on customer.id = bill.customerid 
                                                left outer join customer issueto on issueto.id = vouchers.issueto 
                                                left outer join referencelist issuelocation on issuelocation.id = vouchers.issuelocationid
                                                left outer join referencelist issuecounter on issuecounter.id = vouchers.issuecounterid
                                                where vouchers.isactive=True 
												And vouchers.vouchertype=2 And vouchers.coupenid=0";

        const string SQL_SearchUnlimittedCouponList = @"with billdetails as (
	select vouchers.id, billno, billdate, customerid, (bill.receivable+bill.rounding) billvalue, 
    cast(billattributes->>'couponamount' as numeric) discount
	from vouchers inner join bill on bill.unlimitedcouponid = vouchers.id
	where bill.isactive = true and billtype != 9 and vouchers.isactive=true
	and vouchers.coupenid = {0}	
)
select coupons.name, customer.mobileno, customer.name customername, amount, vouchers.code,
amountaspercentage, vouchers.issuedon, vouchers.validfrom, validupto, coupons.name issuetoname, coupons.mobileno issuetomobileno, 
billdetails.billno, billdetails.billdate, billdetails.discount, billdetails.billvalue,
issuelocation.name issuelocation, issuecounter.name issuecounter
from coupons inner join vouchers on vouchers.coupenid = coupons.id
left outer join billdetails on billdetails.id = vouchers.id
left outer join customer on customer.id = billdetails.customerid 
left outer join referencelist issuelocation on issuelocation.id = vouchers.issuelocationid
left outer join referencelist issuecounter on issuecounter.id = vouchers.issuecounterid
";

        const string SQL_Insert = @"INSERT INTO public.coupons(createdby, modifiedby, createdon, modifiedon, isactive, version, name, mobileno, validfrom, validto, status, customerid, receivedamt, vouchervalue, issuedby,entrytype,givento,seqcode,issuemode,discountpercentage,discountvalue,paymentmode,bankid,ledgerid,paymentreference,issuedon,printedon,counterid,locationid,companyid,denomination)
        VALUES(@createdby, @modifiedby, @createdon, @modifiedon, @isactive, @version, @name, @mobileno, @validfrom, @validto, @status, @customerid, @receivedamt, @vouchervalue, @issuedby, @entrytype, @givento, @seqcode,@issuemode,@discountpercentage,@discountvalue,@paymentmode,@bankid,@ledgerid,@paymentreference,@issuedon,@printedon,@counterid,@locationid,@companyid,@denomination) RETURNING id";

        const string SQL_InsertVoucher = @"INSERT INTO public.coupons(createdby, modifiedby, createdon, modifiedon, isactive, version, name, mobileno, validfrom, validto, status, customerid, receivedamt, vouchervalue, issuedby,entrytype,denomination,givento,seqcode,issuemode,discountpercentage,discountvalue,counterid,billsettlementmasterid,paymentmode,bankid,ledgerid,paymentreference,issuedon,printedon,locationid,companyid) 
        VALUES(@createdby, @modifiedby, @createdon, @modifiedon, @isactive, @version, @name, @mobileno, @validfrom, @validto, @status, @customerid, @receivedamt, @vouchervalue, @issuedby, @entrytype,@denomination,@givento,@seqcode,@issuemode,@discountpercentage,@discountvalue,@counterid,@billsettlementmasterid,@paymentmode,@bankid,@ledgerid,@paymentreference,@issuedon,@printedon,@locationid,@companyid) RETURNING id";

        const string SQL_Update = @"UPDATE public.coupons SET modifiedby=@modifiedby, modifiedon=@modifiedon, version=@version+1, name=@name, mobileno=@mobileno, validfrom=@validfrom, 
        validto=@validto, status=@status, customerid=@customerid, receivedamt=@receivedamt, vouchervalue=@vouchervalue,paymentmode=@paymentmode,bankid=@bankid,ledgerid=@ledgerid,paymentreference=@paymentreference,
        discountpercentage=@discountpercentage,discountvalue=@discountvalue,issuedby=@issuedby, denomination=@denomination,issuedon=@issuedon,printedon=@printedon,locationid=@locationid,
        companyid=@companyid,counterid=@counterid where id=@id and version=@version RETURNING version";

        const string SQL_Delete = @"update public.coupons set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";

        internal List<Voucher2Print> SearchCodes(List<long> items)
        {
            List<Voucher2Print> lst = new List<Voucher2Print>();
            using (NpgsqlConnection conn = CreateConnection())
            {
                conn.Open();
                NpgsqlCommand command = GetCommand("Select id,code,amount,validfrom,validupto from vouchers where isactive=true and id in (" + string.Join(",", items) + ")", conn);
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Voucher2Print oCoupon = new Voucher2Print();
                        oCoupon.id = reader["id"] == DBNull.Value ? "0" : reader["id"].ToString();
                        oCoupon.code = reader["code"] == DBNull.Value ? "" : reader["code"].ToString();
                        oCoupon.value = reader["amount"] == DBNull.Value ? "0.00" : Convert.ToDecimal(reader["amount"]).ToString("F2");
                        oCoupon.validfrom = reader["validfrom"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["validfrom"]);
                        oCoupon.validto = reader["validupto"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["validupto"]);

                        lst.Add(oCoupon);
                    }
                }
                conn.Close();
            }
            return lst;
        }

        public List<SchemeCouponList> SearchCouponList()
        {
            List<SchemeCouponList> lst = new List<SchemeCouponList>();
            using (NpgsqlConnection conn = CreateConnection())
            {
                conn.Open();
                NpgsqlCommand command = GetCommand("Select id,name from coupons where isactive=true and entrytype=2", conn);
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        SchemeCouponList oCoupon = new SchemeCouponList();
                        oCoupon.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        oCoupon.name = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
                        lst.Add(oCoupon);
                    }
                }
                conn.Close();
            }
            return lst;
        }

        public List<SchemeCoupon> SearchCoupon(GiftVoucherCreteria _Coupon)
        {
            List<SchemeCoupon> lst = new List<SchemeCoupon>();

            if (_Coupon.id == 0 && string.IsNullOrEmpty(_Coupon.name)) return lst;
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_Coupon.id != 0)
                {
                    command = GetCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _Coupon.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("isactive", true, "=");
                    qb.AddParameter("entrytype", 2, "=");
                    if (!string.IsNullOrEmpty(_Coupon.name))
                        qb.AddParameter("name", _Coupon.name, "=");

                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        SchemeCoupon oCoupon = new SchemeCoupon();
                        oCoupon.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        oCoupon.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        oCoupon.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        oCoupon.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        oCoupon.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        oCoupon.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        oCoupon.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        //oCoupon.type = reader["type"] == DBNull.Value ? 0 : Convert.ToInt32(reader["type"]);
                        //oCoupon.cardno = reader["cardno"] == DBNull.Value ? "" : reader["cardno"].ToString();
                        oCoupon.name = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
                        oCoupon.validfrom = reader["validfrom"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["validfrom"]);
                        oCoupon.validto = reader["validto"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["validto"]);
                        //oCoupon.balance = reader["balance"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["balance"]);
                        oCoupon.issuedby = reader["issuedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["issuedby"]);
                        oCoupon.rules_json = reader["denomination"] == DBNull.Value ? null : reader["denomination"].ToString();
                        oCoupon.givento = reader["givento"] == DBNull.Value ? "" : reader["givento"].ToString();
                        oCoupon.seqcode = reader["seqcode"] == DBNull.Value ? "" : reader["seqcode"].ToString();
                        oCoupon.issuemode = reader["issuemode"] == DBNull.Value ? 0 : Convert.ToInt32(reader["issuemode"]);
                        oCoupon.discountpercentage = reader["discountpercentage"] == DBNull.Value ? 0 : Convert.ToInt32(reader["discountpercentage"]);
                        oCoupon.discountvalue = reader["discountvalue"] == DBNull.Value ? 0 : Convert.ToInt32(reader["discountvalue"]);
                        lst.Add(oCoupon);
                    }
                }

                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _Coupon);
            }
            return lst;
        }

        internal List<SchemeCouponList> SearchVoucherList()
        {
            List<SchemeCouponList> lst = new List<SchemeCouponList>();
            using (NpgsqlConnection conn = CreateConnection())
            {
                conn.Open();
                NpgsqlCommand command = GetCommand("Select id,name from coupons where isactive=true and entrytype=1", conn);
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        SchemeCouponList oCoupon = new SchemeCouponList();
                        oCoupon.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        oCoupon.name = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
                        lst.Add(oCoupon);
                    }
                }
                conn.Close();
            }
            return lst;
        }
        public List<SearchVoucherResult> SearchVoucherConsumption(GiftVoucherCreteria _Coupon)
        {
            List<SearchVoucherResult> lst = new List<SearchVoucherResult>();
            List<SearchVoucherResult> lstClaims = new List<SearchVoucherResult>();
            NpgsqlConnection conn = CreateConnection();
            NpgsqlCommand command = null;

            string voucherquery = @"select coupons.id,coupons.entrytype,coupons.vouchervalue, coupons.validfrom, coupons.validto, coupons.mobileno, coupons.name,
            vouchers.id,vouchers.vouchertype,vouchers.billid, vouchers.code, vouchers.amount,vouchers.issuedon,vouchers.issueto,vouchers.validupto,
            counter.name countername, location.name locationname
            from vouchers left outer join coupons on coupons.id = vouchers.coupenid 
            left outer join referencelist counter on counter.id = coupons.counterid
            left outer join referencelist location on location.id = coupons.locationid";

            try
            {
                conn.Open();

                if (!string.IsNullOrEmpty(_Coupon.code))
                    return VoucherHistory(1, "code", _Coupon.code);

                if (string.IsNullOrEmpty(_Coupon.code) && _Coupon.billfromdate == DateTime.MinValue
                    && _Coupon.validfrom == DateTime.MinValue)
                    return lst;

                QueryBuilder qb = new QueryBuilder();
                qb.CurrentQuery = voucherquery;

                qb.AddParameter("coupons.isactive", true, "=");
                qb.AddParameter("vouchers.isactive", true, "=");

                if (_Coupon.vouchertypeid == 0)
                    qb.AddInParamer("vouchers.vouchertype", "1,7,8,4");
                else
                    qb.AddORParameter("vouchers.vouchertype", _Coupon.vouchertypeid, "=", "vouchers.vouchertype=4");

                if (_Coupon.validfrom != DateTime.MinValue && _Coupon.validto != DateTime.MinValue)
                {
                    qb.AddParameter("vouchers.issuedon", "issuedonfrom", _Coupon.validfrom.Date, ">=");
                    qb.AddParameter("vouchers.issuedon", "issuedonto", _Coupon.validto.AddDays(1).Date, "<");
                }
                command = qb.GetCommand(conn);
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        SearchVoucherResult item = new SearchVoucherResult();
                        item.validfrom = reader["validfrom"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["validfrom"]);
                        item.validto = reader["validto"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["validto"]);
                        item.issuedon = reader["issuedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["issuedon"]);
                        item.vouchervalue = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"]);
                        item.issuetono = reader["mobileno"] == DBNull.Value ? "" : reader["mobileno"].ToString();
                        item.vouchercode = reader["code"] == DBNull.Value ? "" : reader["code"].ToString();
                        item.issuecountername = reader["countername"] == DBNull.Value ? "" : reader["countername"].ToString();
                        item.issuelocationname = reader["locationname"] == DBNull.Value ? "" : reader["locationname"].ToString();

                        int vouchertype = reader["vouchertype"] == DBNull.Value ? 0 : Convert.ToInt32(reader["vouchertype"]);

                        if (vouchertype == 7)
                            item.vouchertypename = "CREDIT NOTE";
                        else if (vouchertype == 8)
                            item.vouchertypename = "ADVANCE VOUCHER";
                        else if (vouchertype == 1)
                            item.vouchertypename = "GIFT VOUCHER";

                        if (vouchertype == 4)
                        {
                            item.issueto = reader["issueto"] == DBNull.Value ? 0 : Convert.ToInt64(reader["issueto"]);
                            lstClaims.Add(item);
                        }
                        else
                            lst.Add(item);
                    }
                }

                //if (lstClaims.Count == 0 && _Coupon.billfromdate != DateTime.MinValue)
                //{
                //    lst.RemoveAll(e => e.claim == 0);
                //}
                if (lstClaims.Count > 0)
                {
                    string query = @"select customer.name,customer.mobileno,paymentinfo,billsettlementmaster.id,referencelist.name as location,
                                    billsettlementmaster.code,paid_voucher,billsettlementmaster.settlementon,counter.name as countername
                                    from billsettlementmaster 
                                    left outer join customer on customer.id = billsettlementmaster.customerid
                                    left outer join referencelist on referencelist.id = billsettlementmaster.locationid
                                    left outer join referencelist counter on counter.id = billsettlementmaster.counterid
                                    where billsettlementmaster.isactive=true and {0} 
                                    billsettlementmaster.id in (" + string.Join(",", lstClaims.ConvertAll(e => e.issueto)) + ")";
                    if (_Coupon.billfromdate == DateTime.MinValue || _Coupon.billtodate == DateTime.MinValue)
                        query = string.Format(query, "");
                    else
                        query = string.Format(query, " settlementon>=@fromdate and settlementon<@todate and");
                    SortedList<string, object> param = new SortedList<string, object>();
                    param.Add("fromdate", _Coupon.billfromdate.Date);
                    param.Add("todate", _Coupon.billtodate.AddDays(1).Date);

                    using (var reader = ExecuteReader(query, param))
                    {
                        while (reader.Read())
                        {
                            Billsettlementmaster.Paymentinfo paymentinfo = null;
                            string value = reader["paymentinfo"] == DBNull.Value ? "null" : reader["paymentinfo"].ToString();
                            if (!string.IsNullOrEmpty(value) && value != "null")
                                paymentinfo = Newtonsoft.Json.JsonConvert.DeserializeObject<Billsettlementmaster.Paymentinfo>(value);

                            if (paymentinfo != null && paymentinfo.voucherinformation != null &&
                                paymentinfo.voucherinformation.Count > 0)
                            {
                                foreach (var pvoucher in paymentinfo.voucherinformation.FindAll(e => e.type == 0))
                                {
                                    if (!lst.Exists(e => e.vouchercode == pvoucher.code)) continue;

                                    SearchVoucherResult oCoupon = lst.Find(e => e.vouchercode == pvoucher.code);
                                    oCoupon.name = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
                                    oCoupon.settlementcountername = reader["countername"] == DBNull.Value ? "" : reader["countername"].ToString();
                                    oCoupon.location = reader["location"] == DBNull.Value ? "" : reader["location"].ToString();
                                    oCoupon.mobileno = reader["mobileno"] == DBNull.Value ? "" : reader["mobileno"].ToString();
                                    oCoupon.settlementcode = reader["code"] == DBNull.Value ? "" : reader["code"].ToString();
                                    oCoupon.settlementon = reader["settlementon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["settlementon"]);
                                    oCoupon.claim += pvoucher.amount;
                                }
                            }
                        }
                    }
                    if (_Coupon.validfrom == DateTime.MinValue)
                        lst.RemoveAll(e => e.claim == 0);
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _Coupon.code);
            }
            return lst;

        }
        public List<SearchCouponResult> SearchCouponIssue(GiftVoucherCreteria _Coupon)
        {
            const string SQL = @"select vouchers.id,coupenid,vouchertype,issuedon,vouchers.validfrom,validupto,
vouchers.code,vouchers.amount,amountaspercentage,unlimited,
sourcebillsettlementmasterid,billmasterid,billid,issuelocationid,
issuelocation.name locationname,issuecounter.name countername, employee.name employeename,
customer.name customername, promotions.name schemename
from vouchers 
left outer join customer on customer.id = vouchers.issueto
left outer join promotions on promotions.id = vouchers.filterschemeid
left outer join referencelist issuelocation on issuelocation.id = vouchers.issuelocationid
left outer join referencelist issuecounter on issuecounter.id = vouchers.issuecounterid 
left outer join users on users.id = vouchers.createdby
left outer join employee on employee.id = users.employeeid ";

            List<SearchCouponResult> lst = new List<SearchCouponResult>();
            List<string> vouchercode = new List<string>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                if (_Coupon.validfrom == DateTime.MinValue && _Coupon.validto == DateTime.MinValue)
                    return lst;

                conn.Open();
                NpgsqlCommand command = null;
                QueryBuilder qb = new QueryBuilder();

                qb.CurrentQuery = SQL;
                qb.AddParameter("vouchers.isactive", true, "=");
                qb.AddParameter("vouchers.vouchertype", 2, "=");

                if (_Coupon.locationid > 0)
                    qb.AddParameter("vouchers.issuelocationid", _Coupon.locationid, "=");

                if (_Coupon.validfrom != DateTime.MinValue && _Coupon.validto != DateTime.MinValue)
                {
                    qb.AddParameter("vouchers.issuedon", "issuedonfrom", _Coupon.validfrom.Date, ">=");
                    qb.AddParameter("vouchers.issuedon", "issuedonto", _Coupon.validto.AddDays(1).Date, "<");
                }
                qb.SortField = "vouchers.id desc";
                command = qb.GetCommand(conn);
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        SearchCouponResult oCoupon = new SearchCouponResult();
                        oCoupon.issuedon = reader["issuedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["issuedon"]);
                        if (reader["validfrom"] != DBNull.Value &&
                            Convert.ToDateTime(reader["validfrom"]) != DateTime.MinValue)
                            oCoupon.validfrom = Convert.ToDateTime(reader["validfrom"]);
                        else
                            oCoupon.validfrom = oCoupon.issuedon;
                        oCoupon.validto = reader["validupto"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["validupto"]);

                        oCoupon.vouchercode = reader["code"] == DBNull.Value ? "" : reader["code"].ToString();
                        oCoupon.vouchervalue = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"]);
                        oCoupon.issuedbyname = reader["employeename"] == DBNull.Value ? "" : reader["employeename"].ToString();
                        oCoupon.countername = reader["countername"] == DBNull.Value ? "" : reader["countername"].ToString();
                        oCoupon.issuelocation = reader["locationname"] == DBNull.Value ? "" : reader["locationname"].ToString();
                        oCoupon.customername = reader["customername"] == DBNull.Value ? "" : reader["customername"].ToString();
                        oCoupon.schemename = reader["schemename"] == DBNull.Value ? "" : reader["schemename"].ToString();

                        if (oCoupon.vouchercode.IndexOf('#') >= 0)
                            vouchercode.Add(oCoupon.vouchercode);

                        lst.Add(oCoupon);
                    }

                    if (vouchercode.Count > 0)
                    {
                        string _SQLSetDetails = @"SELECT billsettlementmaster.code setcode, c->>'code' code FROM billsettlementmaster JOIN LATERAL 
json_array_elements(billsettlementmaster.paymentinfo->'coupons') c ON TRUE
WHERE c->>'code' in (" + string.Join(",", vouchercode.ConvertAll(e => "'" + WebAPI.Data.Utility.SafeSQL(e) + "'")) + ")";

                        using (var _reader = ExecuteReader(_SQLSetDetails, null))
                        {
                            while (_reader.Read())
                            {
                                string setcode = _reader["setcode"] == DBNull.Value ? "" : _reader["setcode"].ToString();
                                string code = _reader["code"] == DBNull.Value ? "" : _reader["code"].ToString();
                                if (lst.Exists(e => e.vouchercode == code))
                                    lst.Find(e => e.vouchercode == code).billno = setcode;

                            }
                        }
                    }
                }

                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _Coupon.code);
            }
            return lst;

        }

        public List<SearchCouponResult> SearchSettlementCouponIssue(GiftVoucherCreteria _Coupon)
        {
            const string SQL = @"select delivery.id,delivery.createdon issuedon, delivery.altationno,
cast(voucherinfo->> 'couponcode' as character varying(20)) couponcode,
cast(voucherinfo->> 'amount' as numeric) couponamount,vouchers.issuedon vissuedon,vouchers.validfrom,
cast(voucherinfo->> 'expiry' as timestamp without time zone) expiry,
isgiftcoupon,delivery.amount,location.name locationname,counter.name countername, employee.name employeename,
vouchers.billid, bill.billno, billdate, cast(billattributes->>'couponamount' as numeric) discount
from delivery 
inner join vouchers on vouchers.code = cast(voucherinfo->> 'couponcode' as character varying(20))
left outer join bill on bill.id = vouchers.billid
left outer join referencelist location on location.id=delivery.locationid
left outer join referencelist counter on counter.id=delivery.counterid
left outer join users on users.id = delivery.createdby
left outer join employee on employee.id = users.employeeid ";

            const string SQL_Received = @"select delivery.id,delivery.createdon issuedon, delivery.altationno,
cast(voucherinfo->> 'couponcode' as character varying(20)) couponcode,
cast(voucherinfo->> 'amount' as numeric) couponamount,vouchers.issuedon vissuedon,vouchers.validfrom,
cast(voucherinfo->> 'expiry' as timestamp without time zone) expiry,
isgiftcoupon,delivery.amount,location.name locationname,counter.name countername, employee.name employeename,
vouchers.billid, bill.billno, billdate, cast(billattributes->>'couponamount' as numeric) discount
from bill 
inner join vouchers on  vouchers.billid = bill.id
inner join delivery on vouchers.code = cast(voucherinfo->> 'couponcode' as character varying(20))
left outer join referencelist location on location.id=delivery.locationid
left outer join referencelist counter on counter.id=delivery.counterid
left outer join users on users.id = delivery.createdby
left outer join employee on employee.id = users.employeeid ";

            List<SearchCouponResult> lst = new List<SearchCouponResult>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                if (_Coupon.validfrom == DateTime.MinValue && _Coupon.validto == DateTime.MinValue)
                    return lst;

                conn.Open();
                NpgsqlCommand command = null;
                QueryBuilder qb = new QueryBuilder();

                if (_Coupon.vouchertypeid == 1)
                {
                    qb.CurrentQuery = SQL_Received;
                    qb.AddParameter("delivery.isgiftcoupon", true, "=");
                    qb.AddParameter("vouchers.vouchertype", 2, "=");

                    if (_Coupon.locationid > 0)
                        qb.AddParameter("delivery.locationid", _Coupon.locationid, "=");
                    if (_Coupon.validfrom != DateTime.MinValue && _Coupon.validto != DateTime.MinValue)
                    {
                        qb.AddParameter("bill.billdate", "issuedonfrom", _Coupon.validfrom.Date, ">=");
                        qb.AddParameter("bill.billdate", "issuedonto", _Coupon.validto.AddDays(1).Date, "<");
                    }
                    qb.SortField = "delivery.id desc";
                }
                else
                {
                    qb.CurrentQuery = SQL;
                    if (_Coupon.vouchertypeid == 2)
                        qb.AddParameter("vouchers.billid", 0, "=");

                    qb.AddParameter("delivery.isgiftcoupon", true, "=");
                    qb.AddParameter("vouchers.vouchertype", 2, "=");

                    if (_Coupon.locationid > 0)
                        qb.AddParameter("delivery.locationid", _Coupon.locationid, "=");

                    if (_Coupon.validfrom != DateTime.MinValue && _Coupon.validto != DateTime.MinValue)
                    {
                        qb.AddParameter("delivery.createdon", "issuedonfrom", _Coupon.validfrom.Date, ">=");
                        qb.AddParameter("delivery.createdon", "issuedonto", _Coupon.validto.AddDays(1).Date, "<");
                    }

                    qb.SortField = "delivery.id desc";
                }
                command = qb.GetCommand(conn);
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        SearchCouponResult oCoupon = new SearchCouponResult();
                        oCoupon.name = reader["altationno"] == DBNull.Value ? "" : reader["altationno"].ToString();
                        oCoupon.issuedbyname = reader["employeename"] == DBNull.Value ? "" : reader["employeename"].ToString();
                        oCoupon.countername = reader["countername"] == DBNull.Value ? "" : reader["countername"].ToString();
                        oCoupon.customername = reader["locationname"] == DBNull.Value ? "" : reader["locationname"].ToString();
                        oCoupon.vouchercode = reader["couponcode"] == DBNull.Value ? "" : reader["couponcode"].ToString();
                        oCoupon.vouchervalue = reader["couponamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["couponamount"]);
                        oCoupon.discount = reader["discount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discount"]);
                        oCoupon.validto = reader["expiry"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["expiry"]);
                        oCoupon.billno = reader["billno"] == DBNull.Value ? "" : reader["billno"].ToString();
                        oCoupon.billdate = reader["billdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["billdate"]);
                        oCoupon.issuedon = reader["issuedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["issuedon"]);

                        if (reader["validfrom"] != DBNull.Value &&
                            Convert.ToDateTime(reader["validfrom"]) != DateTime.MinValue)
                            oCoupon.validfrom = Convert.ToDateTime(reader["validfrom"]);
                        else
                            oCoupon.validfrom = oCoupon.issuedon;

                        // Because bill-> coupon discount has sum of multiple coupon code.
                        if (oCoupon.discount > oCoupon.vouchervalue)
                            oCoupon.discount = oCoupon.vouchervalue;

                        lst.Add(oCoupon);
                    }
                }

                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _Coupon.code);
            }
            return lst;

        }
        public List<SearchCouponResult> SearchCouponConsumption(GiftVoucherCreteria _Coupon)
        {
            List<SearchCouponResult> lst = new List<SearchCouponResult>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                long couponid = 0;
                CustomerCouponsDataService customerCouponsDataservice = new CustomerCouponsDataService() { Context = this.Context };
                QueryBuilder qb = new QueryBuilder();

                if (_Coupon.id != 0)
                    return CouponHistory(2, "id", _Coupon.id.ToString(), _Coupon.billfromdate, _Coupon.billtodate);

                if (string.IsNullOrEmpty(_Coupon.code) && _Coupon.billfromdate == DateTime.MinValue
                    && _Coupon.billtodate == DateTime.MinValue)
                    return lst;

                if (!string.IsNullOrEmpty(_Coupon.code))
                {
                    var coupondata = customerCouponsDataservice.Search(new CustomerCouponsCreteria()
                    {
                        code = _Coupon.code
                    });
                    couponid = coupondata.Count > 0 ? coupondata.First().coupenid : 0;
                    if (coupondata.Exists(e => e.unlimited))
                    {
                        qb.CurrentQuery = string.Format(SQL_SearchUnlimittedCouponList, couponid);
                        qb.AddParameter("coupons.isactive", true, "=");
                        qb.AddParameter("vouchers.isactive", true, "=");
                        qb.AddParameter("vouchers.vouchertype", 2, "=");
                        qb.AddParameter("coupons.id", couponid, "=");

                    }
                    else
                    {
                        qb.CurrentQuery = SQL_SearchCouponList;
                        qb.AddParameter("coupons.isactive", true, "=");
                        qb.AddParameter("vouchers.isactive", true, "=");
                        qb.AddParameter("vouchers.vouchertype", 2, "=");
                        qb.AddParameter("coupons.id", couponid, "=");
                        qb.AddParameter("vouchers.code", _Coupon.code, "=");
                    }
                    command = qb.GetCommand(conn);
                }
                else
                {
                    qb.CurrentQuery = @"with billdetails as (
	                        select vouchers.id, billno, billdate, customerid, 
                            cast(billattributes->>'couponamount' as numeric) discount, (bill.receivable+bill.rounding) billvalue
	                        from vouchers inner join bill on bill.unlimitedcouponid = vouchers.id
	                        where bill.isactive = true and billtype != 9 and vouchers.isactive=true
	                        and vouchers.isactive=true and vouchers.unlimited=true and billdate>=@fromdate and billdate<@todate
                        )
                        select coupons.name, customer.mobileno, customer.name customername, amount, vouchers.code,amountaspercentage, vouchers.issuedon, vouchers.validfrom, validupto,
                        billdetails.billno, billdetails.billdate, billdetails.discount, billdetails.billvalue, coupons.name issuetoname, coupons.mobileno issuetomobileno,
                        issuelocation.name issuelocation, issuecounter.name issuecounter 
                        from coupons inner join vouchers on vouchers.coupenid = coupons.id
                        inner join billdetails on billdetails.id = vouchers.id
                        left outer join customer on customer.id = billdetails.customerid  
                        left outer join referencelist issuelocation on issuelocation.id = vouchers.issuelocationid
                        left outer join referencelist issuecounter on issuecounter.id = vouchers.issuecounterid
                        where coupons.isactive=true and vouchers.isactive=true and vouchers.vouchertype=2
                        and vouchers.unlimited=true
                        UNION
                        select coupons.name, customer.mobileno, customer.name customername, amount, vouchers.code,amountaspercentage, 
                        vouchers.issuedon, vouchers.validfrom, validupto, billno, billdate,
                        cast(billattributes->>'couponamount' as numeric) discount,(bill.receivable+bill.rounding) billvalue,
                        coupons.name issuetoname, coupons.mobileno issuetomobileno,
                        issuelocation.name issuelocation, issuecounter.name issuecounter
                        from coupons inner join vouchers on vouchers.coupenid = coupons.id
                        inner join bill on bill.id = vouchers.billid
                        left outer join customer on customer.id = bill.customerid
                        left outer join referencelist issuelocation on issuelocation.id = vouchers.issuelocationid
                        left outer join referencelist issuecounter on issuecounter.id = vouchers.issuecounterid
                        where coupons.isactive=true and vouchers.isactive=true and vouchers.vouchertype=2
                        and bill.isactive = true and billtype != 9 and billdate>=@fromdate and billdate<@todate
                        and vouchers.unlimited=false
                        order by billdate";

                    command = qb.GetCommand(conn);
                    command.Parameters.Add("fromdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = _Coupon.billfromdate.Date;
                    command.Parameters.Add("todate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = _Coupon.billtodate.Date.AddDays(1);

                }
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        SearchCouponResult oCoupon = new SearchCouponResult();
                        oCoupon.name = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
                        oCoupon.customername = reader["customername"] == DBNull.Value ? "" : reader["customername"].ToString();
                        oCoupon.mobileno = reader["mobileno"] == DBNull.Value ? "" : reader["mobileno"].ToString();
                        oCoupon.vouchercode = reader["code"] == DBNull.Value ? "" : reader["code"].ToString();
                        oCoupon.billno = reader["billno"] == DBNull.Value ? "" : reader["billno"].ToString();
                        oCoupon.billdate = reader["billdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["billdate"]);
                        oCoupon.billvalue = reader["billvalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["billvalue"]);
                        oCoupon.vouchervalue = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"]);
                        oCoupon.discount = reader["discount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discount"]);
                        oCoupon.ispercentage = reader["amountaspercentage"] == DBNull.Value ? false : Convert.ToBoolean(reader["amountaspercentage"]);
                        oCoupon.validfrom = reader["issuedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["issuedon"]);
                        oCoupon.validto = reader["validupto"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["validupto"]);
                        oCoupon.issuetocustomername = reader["issuetoname"] == DBNull.Value ? "" : reader["issuetoname"].ToString();
                        oCoupon.issuetomobileno = reader["issuetomobileno"] == DBNull.Value ? "" : reader["issuetomobileno"].ToString();
                        oCoupon.issuelocation = reader["issuelocation"] == DBNull.Value ? "" : reader["issuelocation"].ToString();
                        oCoupon.issuecounter = reader["issuecounter"] == DBNull.Value ? "" : reader["issuecounter"].ToString();

                        DateTime validfrom = reader["validfrom"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["validfrom"]);
                        if (validfrom.Year > 2020)
                            oCoupon.validfrom = validfrom;

                        lst.Add(oCoupon);
                    }
                }

                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _Coupon.code);
            }
            return lst;

        }
        internal List<SearchCouponResult> CouponHistory(int vouchertype, string mode, string id)
        {
            return CouponHistory(vouchertype, mode, id, DateTime.MinValue, DateTime.MinValue);
        }
        internal List<SearchCouponResult> CouponHistory(int vouchertype, string mode, string id, DateTime fromdate, DateTime todate)
        {
            List<SearchCouponResult> lst = new List<SearchCouponResult>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                long couponid = 0;
                CustomerCouponsDataService customerCouponsDataservice = new CustomerCouponsDataService() { Context = this.Context };
                QueryBuilder qb = new QueryBuilder();

                if (mode.ToUpper() == "CODE")
                {
                    var coupons = SearchCoupon(new GiftVoucherCreteria() { name = id });
                    if (coupons != null && coupons.Count > 0)
                    {
                        id = coupons[0].id.ToString();
                    }
                }

                if (long.Parse(id) == -9)
                {
                    qb.CurrentQuery = SQL_SearchBirthdayCouponList;
                    if (fromdate != DateTime.MinValue && todate != DateTime.MinValue)
                    {
                        qb.AddParameter("bill.billdate", "datefrom", fromdate.Date, ">=");
                        qb.AddParameter("bill.billdate", "dateto", todate.Date.AddDays(1), "<");
                    }
                }
                else if (long.Parse(id) == -8)
                {
                    qb.CurrentQuery = SQL_SearchSettlementSchemeCouponList;
                    if (fromdate != DateTime.MinValue && todate != DateTime.MinValue)
                    {
                        qb.AddParameter("bill.billdate", "datefrom", fromdate.Date, ">=");
                        qb.AddParameter("bill.billdate", "dateto", todate.Date.AddDays(1), "<");
                    }
                }
                else
                {
                    var coupondata = customerCouponsDataservice.Search(new CustomerCouponsCreteria()
                    {
                        coupenid = long.Parse(id)
                    });
                    couponid = coupondata.Count > 0 ? coupondata.First().coupenid : 0;
                    if (coupondata.Exists(e => e.unlimited))
                    {
                        System.Text.StringBuilder WHERE = new System.Text.StringBuilder();
                        WHERE.Append(couponid.ToString());

                        if (fromdate != DateTime.MinValue && todate != DateTime.MinValue)
                        {
                            WHERE.AppendFormat(" and {0}{1}'{2}' ", "bill.billdate", ">=", fromdate.Date.ToString("yyyy-MM-dd"));
                            WHERE.AppendFormat(" and {0}{1}'{2}' ", "bill.billdate", "<", todate.Date.AddDays(1).ToString("yyyy-MM-dd"));
                        }
                        qb.CurrentQuery = string.Format(SQL_SearchUnlimittedCouponList, WHERE.ToString());
                        qb.AddParameter("coupons.isactive", true, "=");
                        qb.AddParameter("vouchers.isactive", true, "=");
                        qb.AddParameter("vouchers.vouchertype", vouchertype, "=");
                        qb.AddParameter("coupons.id", couponid, "=");

                    }
                    else
                    {
                        qb.CurrentQuery = SQL_SearchCouponList;
                        qb.AddParameter("coupons.isactive", true, "=");
                        qb.AddParameter("vouchers.isactive", true, "=");
                        qb.AddParameter("vouchers.vouchertype", vouchertype, "=");
                        qb.AddParameter("coupons.id", couponid, "=");
                        if (fromdate != DateTime.MinValue && todate != DateTime.MinValue)
                        {
                            qb.AddParameter("bill.billdate", "datefrom", fromdate.Date, ">=");
                            qb.AddParameter("bill.billdate", "dateto", todate.Date.AddDays(1), "<");
                        }
                    }
                }
                command = qb.GetCommand(conn);

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        SearchCouponResult oCoupon = new SearchCouponResult();
                        oCoupon.name = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
                        oCoupon.customername = reader["customername"] == DBNull.Value ? "" : reader["customername"].ToString();
                        oCoupon.mobileno = reader["mobileno"] == DBNull.Value ? "" : reader["mobileno"].ToString();
                        oCoupon.vouchercode = reader["code"] == DBNull.Value ? "" : reader["code"].ToString();
                        oCoupon.billno = reader["billno"] == DBNull.Value ? "" : reader["billno"].ToString();
                        oCoupon.billdate = reader["billdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["billdate"]);
                        oCoupon.billvalue = reader["billvalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["billvalue"]);
                        oCoupon.vouchervalue = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"]);
                        oCoupon.discount = reader["discount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discount"]);
                        oCoupon.ispercentage = reader["amountaspercentage"] == DBNull.Value ? false : Convert.ToBoolean(reader["amountaspercentage"]);
                        oCoupon.validfrom = reader["issuedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["issuedon"]);
                        oCoupon.validto = reader["validupto"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["validupto"]);
                        oCoupon.issuetocustomername = reader["issuetoname"] == DBNull.Value ? "" : reader["issuetoname"].ToString();
                        oCoupon.issuetomobileno = reader["issuetomobileno"] == DBNull.Value ? "" : reader["issuetomobileno"].ToString();
                        oCoupon.issuelocation = reader["issuelocation"] == DBNull.Value ? "" : reader["issuelocation"].ToString();
                        oCoupon.issuecounter = reader["issuecounter"] == DBNull.Value ? "" : reader["issuecounter"].ToString();

                        DateTime validfrom = reader["validfrom"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["validfrom"]);
                        if (validfrom.Year > 2020)
                            oCoupon.validfrom = validfrom;
                        lst.Add(oCoupon);
                    }
                }

                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, id);
            }
            return lst;
        }

        internal List<SearchVoucherResult> VoucherHistory(int vouchertype, string mode, string id)
        {
            List<SearchVoucherResult> lst = new List<SearchVoucherResult>();
            try
            {

                CustomerCouponsDataService customerCouponsDataservice = new CustomerCouponsDataService() { Context = this.Context };

                if (mode.ToUpper() == "CODE")
                {
                    var coupondata = customerCouponsDataservice.Search(new CustomerCouponsCreteria()
                    {
                        code = id
                    });
                    var billsettlementmasterids = coupondata.Where(e => e.vouchertype == CustomerCoupons.CouponType.GiftClaim).ToList().ConvertAll(e => e.issueto);

                    if (billsettlementmasterids != null && billsettlementmasterids.Count > 0)
                    {
                        string query = @"select customer.name,customer.mobileno,paymentinfo,billsettlementmaster.id,referencelist.name as location,billsettlementmaster.code,paid_voucher,billsettlementmaster.settlementon from billsettlementmaster 
                                    left outer join customer on customer.id = billsettlementmaster.customerid
                                    left outer join referencelist on referencelist.id = billsettlementmaster.locationid
                                    where billsettlementmaster.id in (" + string.Join(",", billsettlementmasterids) + ")";
                        using (var reader = ExecuteReader(query, new SortedList<string, object>()))
                        {
                            while (reader.Read())
                            {
                                Billsettlementmaster.Paymentinfo paymentinfo = null;
                                string value = reader["paymentinfo"] == DBNull.Value ? "null" : reader["paymentinfo"].ToString();
                                if (!string.IsNullOrEmpty(value) && value != "null")
                                    paymentinfo = Newtonsoft.Json.JsonConvert.DeserializeObject<Billsettlementmaster.Paymentinfo>(value);

                                if (paymentinfo != null && paymentinfo.voucherinformation != null &&
                                    paymentinfo.voucherinformation.Count > 0)
                                {
                                    foreach (var pvoucher in paymentinfo.voucherinformation)
                                    {
                                        if (!coupondata.Exists(e => e.code == pvoucher.code)) continue;
                                        SearchVoucherResult oCoupon = new SearchVoucherResult();
                                        oCoupon.name = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
                                        oCoupon.location = reader["location"] == DBNull.Value ? "" : reader["location"].ToString();
                                        oCoupon.mobileno = reader["mobileno"] == DBNull.Value ? "" : reader["mobileno"].ToString();
                                        oCoupon.vouchercode = pvoucher.code;
                                        oCoupon.settlementcode = reader["code"] == DBNull.Value ? "" : reader["code"].ToString();
                                        oCoupon.settlementon = reader["settlementon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["settlementon"]);
                                        oCoupon.claim = pvoucher.amount;
                                        lst.Add(oCoupon);
                                    }
                                }
                            }
                        }
                    }
                    SearchVoucherResult voucher = new SearchVoucherResult();
                    voucher.voucherid = coupondata.First().id;
                    voucher.vouchercode = coupondata.First().code;
                    voucher.vouchervalue = coupondata.First().amount;
                    voucher.validfrom = coupondata.First().issuedon;
                    voucher.validto = coupondata.First().validupto;
                    voucher.issuetono = coupondata.First().mobileno;
                    voucher.unclaim = coupondata.First().amount - lst.Sum(e => e.vouchercode == coupondata.First().code ? e.claim : 0);
                    voucher.isvoucher = true;
                    lst.Add(voucher);
                }
                else
                {
                    var coupondata = customerCouponsDataservice.Search(new CustomerCouponsCreteria()
                    {
                        coupenid = long.Parse(id)
                    });
                    var unclaimed = coupondata.Where(e => e.vouchertype == CustomerCoupons.CouponType.GiftVoucher).ToList();
                    var billsettlementmasterids = coupondata.Where(e => e.vouchertype == CustomerCoupons.CouponType.GiftClaim).ToList().ConvertAll(e => e.issueto);
                    if (billsettlementmasterids != null && billsettlementmasterids.Count > 0)
                    {
                        string query = @"select customer.name,customer.mobileno,paymentinfo,billsettlementmaster.id,referencelist.name as location,billsettlementmaster.code,paid_voucher,billsettlementmaster.settlementon from billsettlementmaster 
                                    left outer join customer on customer.id = billsettlementmaster.customerid
                                    left outer join referencelist on referencelist.id = billsettlementmaster.locationid
                                    where billsettlementmaster.id in (" + string.Join(",", billsettlementmasterids) + ")";
                        using (var reader = ExecuteReader(query, new SortedList<string, object>()))
                        {
                            while (reader.Read())
                            {
                                Billsettlementmaster.Paymentinfo paymentinfo = null;
                                string value = reader["paymentinfo"] == DBNull.Value ? "null" : reader["paymentinfo"].ToString();
                                if (!string.IsNullOrEmpty(value) && value != "null")
                                    paymentinfo = Newtonsoft.Json.JsonConvert.DeserializeObject<Billsettlementmaster.Paymentinfo>(value);

                                if (paymentinfo != null && paymentinfo.voucherinformation != null &&
                                    paymentinfo.voucherinformation.Count > 0)
                                {
                                    foreach (var voucher in paymentinfo.voucherinformation)
                                    {
                                        if (!coupondata.Exists(e => e.code == voucher.code)) continue;
                                        SearchVoucherResult oCoupon = new SearchVoucherResult();
                                        oCoupon.name = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
                                        oCoupon.location = reader["location"] == DBNull.Value ? "" : reader["location"].ToString();
                                        oCoupon.mobileno = reader["mobileno"] == DBNull.Value ? "" : reader["mobileno"].ToString();
                                        oCoupon.vouchercode = voucher.code;
                                        oCoupon.settlementcode = reader["code"] == DBNull.Value ? "" : reader["code"].ToString();
                                        oCoupon.settlementon = reader["settlementon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["settlementon"]);
                                        oCoupon.claim = voucher.amount;
                                        lst.Add(oCoupon);
                                    }
                                }
                            }
                        }
                    }
                    foreach (var unclaim in unclaimed)
                    {
                        if (lst.Exists(e => e.vouchercode == unclaim.code)) continue;
                        SearchVoucherResult oCoupon = new SearchVoucherResult();
                        oCoupon.vouchercode = unclaim.code;
                        oCoupon.vouchervalue = unclaim.amount;
                        oCoupon.unclaim = unclaim.amount - lst.Sum(e => e.vouchercode == unclaim.code ? e.claim : 0);
                        oCoupon.isvoucher = true;
                        lst.Add(oCoupon);
                    }
                }

            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, id);
            }
            lst = lst.OrderBy(e => e.vouchercode).ThenByDescending(e => e.isvoucher).ToList();
            return lst;
        }

        internal List<SearchCouponResult> SearchCreditNote(GiftVoucherCreteria _request)
        {
            string SQL = @"select vouchers.code,vouchers.amount,vouchers.validupto,vouchers.vouchertype
                                from coupons inner join vouchers on vouchers.coupenid = coupons.id
                                where billsettlementmasterid=@billsettlementmasterid and vouchertype=7 ";

            List<SearchCouponResult> lst = new List<SearchCouponResult>();
            using (NpgsqlConnection conn = CreateConnection())
            {
                conn.Open();
                NpgsqlCommand command = GetCommand(SQL, conn);
                command.Parameters.Add("billsettlementmasterid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _request.billsettlementmasterid;
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        SearchCouponResult oCoupon = new SearchCouponResult();
                        oCoupon.vouchercode = reader["code"] == DBNull.Value ? "" : reader["code"].ToString();
                        oCoupon.vouchervalue = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"]);
                        oCoupon.validto = reader["validupto"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["validupto"]);
                        oCoupon.vouchertype = "7";
                        lst.Add(oCoupon);
                    }
                }
                conn.Close();
            }
            return lst;

        }

        internal List<SearchCouponResult> SearchCustomerCreditNote(GiftVoucherCreteria _request)
        {
            List<SearchCouponResult> lst = new List<SearchCouponResult>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                QueryBuilder qb = new QueryBuilder();
                qb.CurrentQuery = SQL_SearchCreditNoteList;

                qb.AddParameter("coupons.isactive", true, "=");
                qb.AddParameter("vouchers.isactive", true, "=");

                if (_request.vouchertypeid == 0)
                    qb.AddInParamer("vouchers.vouchertype", "1,7,8");
                else
                    qb.AddParameter("vouchers.vouchertype", _request.vouchertypeid, "=");

                if (_request.billfromdate != DateTime.MinValue && _request.billtodate != DateTime.MinValue)
                {
                    qb.AddParameter("vouchers.issuedon", "receivedonfrom", _request.billfromdate.Date, ">=");
                    qb.AddParameter("vouchers.issuedon", "receivedonto", _request.billtodate.AddDays(1).Date, "<");
                }
                if (!string.IsNullOrEmpty(_request.mobileno))
                    qb.AddParameter("coupons.mobileno", _request.mobileno, "=");

                command = qb.GetCommand(conn);

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        SearchCouponResult oCoupon = new SearchCouponResult();
                        oCoupon.name = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
                        oCoupon.countername = reader["countername"] == DBNull.Value ? "" : reader["countername"].ToString();
                        oCoupon.issuedbyname = reader["issuedbyname"] == DBNull.Value ? "" : reader["issuedbyname"].ToString();
                        oCoupon.customername = reader["customername"] == DBNull.Value ? "" : reader["customername"].ToString();
                        oCoupon.mobileno = reader["mobileno"] == DBNull.Value ? "" : reader["mobileno"].ToString();
                        oCoupon.vouchercode = reader["code"] == DBNull.Value ? "" : reader["code"].ToString();
                        oCoupon.billno = reader["billno"] == DBNull.Value ? "" : reader["billno"].ToString();
                        oCoupon.billdate = reader["billdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["billdate"]);
                        oCoupon.vouchervalue = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"]);
                        oCoupon.ispercentage = reader["amountaspercentage"] == DBNull.Value ? false : Convert.ToBoolean(reader["amountaspercentage"]);
                        oCoupon.validfrom = reader["issuedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["issuedon"]);
                        oCoupon.validto = reader["validupto"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["validupto"]);
                        oCoupon.issuetocustomername = reader["issuetoname"] == DBNull.Value ? "" : reader["issuetoname"].ToString();
                        oCoupon.issuetomobileno = reader["issuetomobileno"] == DBNull.Value ? "" : reader["issuetomobileno"].ToString();
                        int vouchertype = reader["vouchertype"] == DBNull.Value ? 0 : Convert.ToInt32(reader["vouchertype"]);

                        if (vouchertype == 7)
                            oCoupon.vouchertype = "CREDIT NOTE";
                        else if (vouchertype == 8)
                            oCoupon.vouchertype = "ADVANCE VOUCHER";
                        else if (vouchertype == 1)
                            oCoupon.vouchertype = "GIFT VOUCHER";

                        lst.Add(oCoupon);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _request);
            }
            return lst;
        }


        public List<GiftVoucher> Search(GiftVoucherCreteria _Coupon)
        {
            List<GiftVoucher> lst = new List<GiftVoucher>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;

                QueryBuilder qb = new QueryBuilder();
                qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                if (_Coupon.customerid > 0)

                    if (_Coupon.customerid > 0)
                        qb.AddParameter("customerid", _Coupon.customerid, "=");
                qb.AddParameter("isactive", true, "=");
                qb.AddParameter("coupons.issuedon", "issuedonfrom", _Coupon.issuedfrom.Date, ">=");
                qb.AddParameter("coupons.issuedon", "issuedonto", _Coupon.issuedto.AddDays(1).Date, "<");
                qb.Add2Query("id in (select distinct coupenid from vouchers where vouchertype=1 and isactive=true)");
                command = qb.GetCommand(conn);


                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        GiftVoucher oCoupon = new GiftVoucher();
                        oCoupon.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        oCoupon.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        oCoupon.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        oCoupon.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        oCoupon.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        oCoupon.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        oCoupon.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        //oCoupon.type = reader["type"] == DBNull.Value ? 0 : Convert.ToInt32(reader["type"]);
                        oCoupon.name = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
                        oCoupon.mobileno = reader["mobileno"] == DBNull.Value ? "" : reader["mobileno"].ToString();
                        oCoupon.validfrom = reader["validfrom"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["validfrom"]);
                        oCoupon.validto = reader["validto"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["validto"]);
                        //oCoupon.balance = reader["balance"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["balance"]);
                        oCoupon.status = reader["status"] == DBNull.Value ? 0 : Convert.ToInt32(reader["status"]);
                        oCoupon.customerid = reader["customerid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["customerid"]);
                        oCoupon.receivedamt = reader["receivedamt"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["receivedamt"]);
                        oCoupon.vouchervalue = reader["vouchervalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["vouchervalue"]);
                        oCoupon.issuedby = reader["issuedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["issuedby"]);
                        oCoupon.denomination_json = reader["denomination"] == DBNull.Value ? null : reader["denomination"].ToString();
                        oCoupon.givento = reader["givento"] == DBNull.Value ? "" : reader["givento"].ToString();
                        oCoupon.seqcode = reader["seqcode"] == DBNull.Value ? "" : reader["seqcode"].ToString();
                        oCoupon.issuemode = reader["issuemode"] == DBNull.Value ? 0 : Convert.ToInt32(reader["issuemode"]);
                        oCoupon.discountpercentage = reader["discountpercentage"] == DBNull.Value ? 0 : Convert.ToInt32(reader["discountpercentage"]);
                        oCoupon.discountvalue = reader["discountvalue"] == DBNull.Value ? 0 : Convert.ToInt32(reader["discountvalue"]);
                        oCoupon.issuedon = reader["issuedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["issuedon"]);
                        oCoupon.printedon = reader["printedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["printedon"]);
                        oCoupon.ledgerid = reader["ledgerid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["ledgerid"]);
                        oCoupon.companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"]);
                        oCoupon.locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                        oCoupon.counterid = reader["counterid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["counterid"]);
                        oCoupon.paymentreference = reader["paymentreference"] == DBNull.Value ? "" : reader["paymentreference"].ToString();
                        lst.Add(oCoupon);
                    }
                }

                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _Coupon);
            }
            return lst;
        }


        public List<GiftVoucher> SearchVoucher(GiftVoucherCreteria _Coupon)
        {
            List<GiftVoucher> lst = new List<GiftVoucher>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_Coupon.id != 0)
                {
                    command = GetCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _Coupon.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    if (_Coupon.customerid > 0)

                        if (_Coupon.customerid > 0)
                            qb.AddParameter("customerid", _Coupon.customerid, "=");
                    qb.AddParameter("isactive", true, "=");
                    qb.AddParameter("entrytype", 1, "=");
                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        GiftVoucher oCoupon = new GiftVoucher();
                        oCoupon.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        oCoupon.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        oCoupon.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        oCoupon.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        oCoupon.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        oCoupon.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        oCoupon.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        //oCoupon.type = reader["type"] == DBNull.Value ? 0 : Convert.ToInt32(reader["type"]);
                        //oCoupon.cardno = reader["cardno"] == DBNull.Value ? "" : reader["cardno"].ToString();
                        oCoupon.name = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
                        oCoupon.mobileno = reader["mobileno"] == DBNull.Value ? "" : reader["mobileno"].ToString();
                        oCoupon.validfrom = reader["validfrom"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["validfrom"]);
                        oCoupon.validto = reader["validto"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["validto"]);
                        //oCoupon.balance = reader["balance"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["balance"]);
                        oCoupon.status = reader["status"] == DBNull.Value ? 0 : Convert.ToInt32(reader["status"]);
                        oCoupon.customerid = reader["customerid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["customerid"]);
                        oCoupon.receivedamt = reader["receivedamt"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["receivedamt"]);
                        oCoupon.vouchervalue = reader["vouchervalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["vouchervalue"]);
                        oCoupon.issuedby = reader["issuedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["issuedby"]);
                        oCoupon.denomination_json = reader["denomination"] == DBNull.Value ? null : reader["denomination"].ToString();
                        oCoupon.givento = reader["givento"] == DBNull.Value ? "" : reader["givento"].ToString();
                        oCoupon.seqcode = reader["seqcode"] == DBNull.Value ? "" : reader["seqcode"].ToString();
                        oCoupon.issuemode = reader["issuemode"] == DBNull.Value ? 0 : Convert.ToInt32(reader["issuemode"]);
                        oCoupon.discountpercentage = reader["discountpercentage"] == DBNull.Value ? 0 : Convert.ToInt32(reader["discountpercentage"]);
                        oCoupon.discountvalue = reader["discountvalue"] == DBNull.Value ? 0 : Convert.ToInt32(reader["discountvalue"]);
                        oCoupon.companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"]);
                        oCoupon.locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                        oCoupon.counterid = reader["counterid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["counterid"]);
                        oCoupon.ledgerid = reader["ledgerid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["ledgerid"]);
                        oCoupon.issuedon = reader["issuedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["issuedon"]);
                        oCoupon.printedon = reader["printedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["printedon"]);
                        oCoupon.paymentreference = reader["paymentreference"] == DBNull.Value ? "" : reader["paymentreference"].ToString();
                        lst.Add(oCoupon);
                    }
                }

                if (lst.Count == 1 && _Coupon.id > 0)
                {
                    CustomerCouponsDataService voucherservice = new CustomerCouponsDataService() { Context = this.Context };
                    voucherservice.Context = this.Context;
                    lst[0].coupons = voucherservice.Search(new CustomerCouponsCreteria()
                    {
                        coupenid = _Coupon.id
                    });
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _Coupon);
            }
            return lst;
        }

        //public List<GiftVoucher> SearchVouchersList(GiftVoucherCreteria _Coupon)
        //{
        //    List<GiftVoucher> lst = new List<GiftVoucher>();
        //    NpgsqlConnection conn = CreateConnection();
        //    try
        //    {
        //        conn.Open();

        //        NpgsqlCommand command = null;
        //        if (_Coupon.id != 0)
        //        {
        //            command = GetCommand(SQL_SearchVoucherList, conn);
        //            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _Coupon.id;
        //        }
        //        else
        //        {
        //            QueryBuilder qb = new QueryBuilder();
        //            qb.CurrentQuery = SQL_SearchVoucherList;
        //            qb.AddParameter("coupons.isactive", true, "=");
        //            qb.AddParameter("coupons.customerid", _Coupon.customerid, "=");

        //            //if (!string.IsNullOrEmpty(_Coupon.Code))
        //            //    qb.AddParameter("code", _Coupon.Code, "=");

        //            command = qb.GetCommand(conn);
        //        }

        //        using (System.Data.IDataReader reader = command.ExecuteReader())
        //        {
        //            while (reader.Read())
        //            {
        //                GiftVoucher oCoupon = new GiftVoucher();
        //                oCoupon.name = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
        //                oCoupon.mobileno = reader["mobileno"] == DBNull.Value ? "" : reader["mobileno"].ToString();
        //                oCoupon.vouchercode = reader["code"] == DBNull.Value ? "" : reader["code"].ToString();
        //                oCoupon.vouchervalue = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"].ToString());

        //                lst.Add(oCoupon);
        //            }
        //        }

        //        conn.Close();
        //    }
        //    catch (Exception exp)
        //    {
        //        if (conn != null) conn.Close();
        //        WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _Coupon);
        //    }
        //    return lst;
        //}


        internal SchemeCoupon Update(SchemeCoupon item)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    var existingcoupons = SearchCoupon(new GiftVoucherCreteria()
                    {
                        id = item.id
                    });

                    CustomerCouponsDataService voucherservice = new CustomerCouponsDataService() { Context = this.Context };
                    voucherservice.Context = this.Context;
                    if (existingcoupons.Count == 1)
                    {
                        var deleted = existingcoupons.First().rules.FindAll(e => !item.rules.Exists(c => c.id == e.id));
                        if (deleted.Count > 0)
                        {
                            List<long> ids = new List<long>();
                            deleted.ForEach(e =>
                            {
                                if (e.couponids != null)
                                    ids.AddRange(e.couponids);
                            });

                            var couponlist = voucherservice.Search(new CustomerCouponsCreteria()
                            {
                                ids = ids
                            });
                            if (couponlist.Any(e => e.billid > 0))
                            {
                                item.errormessage = "Some codes [" + string.Join(",", couponlist.Where(e => e.billid > 0).ToList().ConvertAll(e => e.code)) + "] already consumed, can't remove the rule.";
                                return item;
                            }
                            couponlist.ForEach(e =>
                            {
                                voucherservice.Delete(e);
                            });
                        }
                        existingcoupons.First().rules.RemoveAll(e => !item.rules.Exists(c => c.id == e.id));
                        if (existingcoupons.First().rules.Count > 0)
                        {
                            var modified = existingcoupons.First().rules.FindAll(e => item.rules.Exists(c => c.id == e.id && c.amount != e.amount));
                            List<long> ids = new List<long>();
                            modified.ForEach(e =>
                            {
                                if (e.couponids != null)
                                    ids.AddRange(e.couponids);
                            });
                            if (ids.Count == 0)
                            {
                                var couponlist = voucherservice.Search(new CustomerCouponsCreteria()
                                {
                                    coupenid = item.id
                                });
                                if (couponlist.Count > 0)
                                {
                                    if (item.validto.Date != couponlist[0].validupto.Date)
                                    {
                                        couponlist.ForEach(e =>
                                        {
                                            e.validupto = item.validto;
                                            voucherservice.Update(e);
                                        });
                                    }
                                }
                            }
                            else
                            {
                                var couponlist = voucherservice.Search(new CustomerCouponsCreteria()
                                {
                                    ids = ids
                                });
                                couponlist.ForEach(e =>
                                {
                                    if (item.rules != null &&
                                    item.rules.Exists(xe => xe.couponids != null && xe.couponids.Contains(e.id)))
                                    {
                                        e.amount = item.rules.Find(xe => xe.couponids != null && xe.couponids.Contains(e.id)).amount;
                                        voucherservice.Update(e);
                                    }
                                });
                            }
                        }
                    }

                    long ruleid = item.rules.Max(e => e.id) + 1;
                    foreach (var rule in item.rules.Where(e => e.couponids == null || e.couponids.Count == 0 || e.id == 0))
                    {
                        rule.id = ruleid++;
                        rule.couponids = new List<long>();
                        for (int i = 0; i < (rule.unlimitted ? 1 : rule.quantity); i++)
                        {
                            while (true)
                            {
                                try
                                {
                                    var vouchercode = DateTimeNow.Year.ToString().Substring(2, 2) + WebAPI.Data.Token.RandomString(14, "ABCDEFGHJKLMNPQRSTUVWXYZ23456789");
                                    var coupon = new CustomerCoupons()
                                    {
                                        code = vouchercode,
                                        issuedby = item.issuedby,
                                        amount = rule.amount,
                                        isactive = true,
                                        issuedon = item.validfrom,
                                        coupenid = item.id,
                                        validupto = item.validto,
                                        acceptablelocationid = rule.acceptablelocationid,
                                        amountaspercentage = rule.amountaspercentage,
                                        maxbilldiscountpercentage = rule.maxbilldiscountpercentage,
                                        unlimited = rule.unlimitted,
                                        vouchertype = CustomerCoupons.CouponType.Coupon
                                    };
                                    voucherservice.Insert(transaction, conn, coupon);
                                    rule.couponids.Add(coupon.id);
                                    break;
                                }
                                catch (Exception ex)
                                {

                                }
                            }
                        }
                    }
                    Update(transaction, conn, item);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, item);
            }
            return item;
        }


        internal SchemeCoupon Insert(SchemeCoupon item)
        {
            if (item == null || item.rules == null || item.rules.Count == 0)
            {
                item.errormessage = "Invalid Coupon data.";
                return item;
            }
            if (item.validfrom == DateTime.MinValue || item.validto == DateTime.MinValue)
            {
                item.errormessage = "Invalid Coupon validity.";
                return item;
            }

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    item.isactive = true;
                    item.version = 1;
                    conn.Open();
                    transaction = conn.BeginTransaction();


                    Insert(transaction, conn, item);
                    CustomerCouponsDataService voucherservice = new CustomerCouponsDataService() { Context = this.Context };
                    voucherservice.Context = this.Context;

                    long ruleid = 1;
                    foreach (var rule in item.rules)
                    {
                        rule.id = ruleid++;
                        rule.couponids = new List<long>();
                        for (int i = 0; i < (rule.unlimitted ? 1 : rule.quantity); i++)
                        {
                            while (true)
                            {
                                try
                                {
                                    var vouchercode = DateTimeNow.Year.ToString().Substring(2, 2) + WebAPI.Data.Token.RandomString(14, "ABCDEFGHJKLMNPQRSTUVWXYZ23456789");
                                    var coupon = new CustomerCoupons()
                                    {
                                        code = vouchercode,
                                        issuedby = item.issuedby,
                                        amount = rule.amount,
                                        isactive = true,
                                        issuedon = (item.issuedon == DateTime.MinValue ? DateTimeNow : item.issuedon),
                                        validfrom = item.validfrom,
                                        validupto = item.validto,
                                        coupenid = item.id,
                                        locationid = item.locationid,
                                        counterid = item.counterid,
                                        acceptablelocationid = rule.acceptablelocationid,
                                        amountaspercentage = rule.amountaspercentage,
                                        maxbilldiscountpercentage = rule.maxbilldiscountpercentage,
                                        unlimited = rule.unlimitted,
                                        minbillvalue = rule.minbillvalue,
                                        ex_discount = rule.ex_discount,
                                        ex_adiscount = rule.ex_adiscount,
                                        ex_sdiscount = rule.ex_sdiscount,
                                        otp = rule.otprequired,
                                        otpcustomerid = rule.otprequired? rule.otpcustomerid:0,
                                        vouchertype = CustomerCoupons.CouponType.Coupon,
                                        isranged = rule.isranged,
                                        rangedrule = rule.rangedrule
                                    };
                                    voucherservice.Insert(transaction, conn, coupon);
                                    rule.couponids.Add(coupon.id);
                                    break;
                                }
                                catch (Exception ex)
                                {

                                }
                            }
                        }
                    }

                    UpdateCoupons(transaction, conn, item);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, item);
            }

            return item;
        }


        public bool UpdateCoupons(NpgsqlTransaction transaction, NpgsqlConnection conn, SchemeCoupon oCoupon)
        {
            NpgsqlCommand command = GetCommand("Update coupons set denomination=@denomination where id=@id", conn, transaction);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCoupon.id;
            command.Parameters.Add("denomination", NpgsqlTypes.NpgsqlDbType.Json).Value = oCoupon.rules_json == null ? "null" : oCoupon.rules_json;
            return command.ExecuteNonQuery() > 1;
        }

        public SchemeCoupon Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, SchemeCoupon oCoupon)
        {
            if (string.IsNullOrEmpty(oCoupon.seqcode))
            {
                var sequence = SequenceManager.Instance(Context.OrganizationID).Get("giftvoucherseq.code", 0);
                oCoupon.seqcode = sequence.GetNextNumber(Context, conn, transaction);
            }
            NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oCoupon.isactive;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oCoupon.version;
            command.Parameters.Add("entrytype", NpgsqlTypes.NpgsqlDbType.Integer).Value = 2;
            //command.Parameters.Add("cardno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCoupon.cardno == null ? "" : oCoupon.cardno;
            command.Parameters.Add("name", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCoupon.name == null ? "" : oCoupon.name;
            command.Parameters.Add("mobileno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = "";
            command.Parameters.Add("validfrom", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oCoupon.validfrom;
            command.Parameters.Add("validto", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oCoupon.validto;
            //command.Parameters.Add("balance", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oCoupon.balance;
            command.Parameters.Add("status", NpgsqlTypes.NpgsqlDbType.Integer).Value = 0;
            command.Parameters.Add("customerid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = 0;
            command.Parameters.Add("receivedamt", NpgsqlTypes.NpgsqlDbType.Numeric).Value = 0;
            command.Parameters.Add("vouchervalue", NpgsqlTypes.NpgsqlDbType.Numeric).Value = 0;
            command.Parameters.Add("issuedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCoupon.issuedby;
            command.Parameters.Add("givento", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCoupon.givento == null ? "" : oCoupon.givento;
            command.Parameters.Add("seqcode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCoupon.seqcode == null ? "" : oCoupon.seqcode;
            command.Parameters.Add("issuemode", NpgsqlTypes.NpgsqlDbType.Integer).Value = oCoupon.issuemode;
            command.Parameters.Add("discountpercentage", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oCoupon.issuemode;
            command.Parameters.Add("discountvalue", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oCoupon.discountvalue;
            command.Parameters.Add("paymentmode", NpgsqlTypes.NpgsqlDbType.Integer).Value = oCoupon.paymentmode;
            command.Parameters.Add("bankid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCoupon.bankid;
            command.Parameters.Add("ledgerid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCoupon.ledgerid;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCoupon.locationid;
            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCoupon.companyid;
            command.Parameters.Add("counterid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCoupon.counterid;
            command.Parameters.Add("issuedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oCoupon.issuedon;
            command.Parameters.Add("printedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oCoupon.issuemode == 0 ? oCoupon.issuedon : oCoupon.printedon;
            command.Parameters.Add("paymentreference", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCoupon.paymentreference == null ? "" : oCoupon.paymentreference;
            command.Parameters.Add("denomination", NpgsqlTypes.NpgsqlDbType.Json).Value = oCoupon.rules_json == null ? "null" : oCoupon.rules_json;


            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oCoupon.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            return oCoupon;
        }

        public GiftVoucher Insert(GiftVoucher oCoupon)
        {
            if (oCoupon.issuemode == 0)
            {
                if (oCoupon == null || oCoupon.receivedamt != oCoupon.vouchervalue)
                {
                    oCoupon.errormessage = "Invalid amount received.";
                    return oCoupon;
                }
                if (oCoupon.customerid == 0 || string.IsNullOrEmpty(oCoupon.mobileno) || string.IsNullOrEmpty(oCoupon.name))
                {
                    oCoupon.errormessage = "Please enter valid Customer details.";
                    return oCoupon;
                }
                if (oCoupon.validfrom == DateTime.MinValue || oCoupon.validto == DateTime.MinValue)
                {
                    oCoupon.errormessage = "Invalid Coupon validity.";
                    return oCoupon;
                }
                if (oCoupon.counterid == 0)
                {
                    oCoupon.errormessage = "Please Select Counter/Issued By Employee";
                    return oCoupon;
                }
            }

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    var sequence = SequenceManager.Instance(Context.OrganizationID).Get("giftvoucherseq.code", 0);
                    oCoupon.seqcode = string.IsNullOrEmpty(oCoupon.seqcode) ? sequence.GetNextNumber(Context, conn, transaction) : oCoupon.seqcode;

                    Insert(transaction, conn, oCoupon);
                    CustomerCouponsDataService voucherservice = new CustomerCouponsDataService() { Context = this.Context };
                    voucherservice.Context = this.Context;

                    foreach (var item in oCoupon.denomination)
                    {
                        if (item.qty > 0 && item.denomination != -1)
                        {
                            for (int i = 0; i < item.qty; i++)
                            {

                                while (true)
                                {
                                    try
                                    {
                                        oCoupon.vouchercode = DateTimeNow.Year.ToString().Substring(2, 2) + WebAPI.Data.Token.RandomString(14, "0123456789");
                                        voucherservice.Insert(transaction, conn, new CustomerCoupons()
                                        {
                                            code = oCoupon.vouchercode,
                                            issuedby = oCoupon.issuemode == 1 ? 0 : oCoupon.issuedby,
                                            issueto = oCoupon.issuemode == 1 ? 0 : oCoupon.receivedby,
                                            amount = item.denomination,
                                            isactive = true,
                                            issuedon = DateTimeNow,
                                            coupenid = oCoupon.id,
                                            validupto = oCoupon.validto,
                                            validfrom = oCoupon.validfrom,
                                            status = oCoupon.issuemode==1?-1:0,
                                            vouchertype = CustomerCoupons.CouponType.GiftVoucher
                                        });
                                        break;
                                    }
                                    catch (Exception ex)
                                    {

                                    }
                                }
                            }
                        }
                        else if (item.qty > 0 && item.denomination == -1)
                        {
                            while (true)
                            {
                                try
                                {
                                    oCoupon.vouchercode = DateTimeNow.Year.ToString().Substring(2, 2) + WebAPI.Data.Token.RandomString(14, "0123456789");
                                    voucherservice.Insert(transaction, conn, new CustomerCoupons()
                                    {
                                        code = oCoupon.vouchercode,
                                        issuedby = oCoupon.issuedby,
                                        issueto = oCoupon.receivedby,
                                        amount = item.amount,
                                        isactive = true,
                                        issuedon = DateTimeNow,
                                        coupenid = oCoupon.id,
                                        validupto = oCoupon.validto,
                                        validfrom = oCoupon.validfrom,
                                        vouchertype = CustomerCoupons.CouponType.GiftVoucher
                                    });
                                    break;
                                }
                                catch (Exception ex)
                                {

                                }
                            }
                        }
                    }

                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oCoupon);
            }

            return oCoupon;
        }

        public GiftVoucher InsertMigrationData(GiftVoucher gtvoucher)
        {
            NpgsqlTransaction transaction = null;
            using (NpgsqlConnection conn = CreateConnection())
            {
                conn.Open();
                transaction = conn.BeginTransaction();
                try
                {
                    Insert(transaction, conn, gtvoucher);

                    CustomerCouponsDataService voucherservice = new CustomerCouponsDataService() { Context = this.Context };
                    voucherservice.Insert(transaction, conn, new CustomerCoupons()
                    {
                        code = gtvoucher.vouchercode,
                        issuedby = gtvoucher.issuedby,
                        issueto = gtvoucher.receivedby,
                        amount = gtvoucher.vouchervalue,
                        isactive = true,
                        issuedon = DateTime.Now,
                        coupenid = gtvoucher.id,
                        validupto = gtvoucher.validto,
                        vouchertype = CustomerCoupons.CouponType.CreditNote
                    });

                    transaction.Commit();
                    conn.Close();
                }
                catch (Exception exp)
                {
                    WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, gtvoucher);
                }
            }
            return gtvoucher;
        }

        public GiftVoucher Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, GiftVoucher oCoupon)
        {
            if (oCoupon.issuemode == 0 || oCoupon.issuemode == 2 || oCoupon.issuemode == 5)
            {
                if (oCoupon.issuedon == DateTime.MinValue)
                {
                    throw new Exception("Issuedon missing while saving gift coupon");
                }
            }
            if (string.IsNullOrEmpty(oCoupon.seqcode))
            {
                var sequence = SequenceManager.Instance(Context.OrganizationID).Get("giftvoucherseq.code", 0);
                oCoupon.seqcode = sequence.GetNextNumber(Context, conn, transaction);
            }
            NpgsqlCommand command = GetCommand(SQL_InsertVoucher, conn, transaction);
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oCoupon.isactive;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oCoupon.version;
            command.Parameters.Add("entrytype", NpgsqlTypes.NpgsqlDbType.Integer).Value = 1;
            //command.Parameters.Add("cardno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCoupon.cardno == null ? "" : oCoupon.cardno;
            command.Parameters.Add("name", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCoupon.name == null ? "" : oCoupon.name;
            command.Parameters.Add("mobileno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCoupon.mobileno == null ? "" : oCoupon.mobileno;
            command.Parameters.Add("validfrom", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oCoupon.validfrom;
            command.Parameters.Add("validto", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oCoupon.validto;
            //command.Parameters.Add("balance", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oCoupon.balance;
            command.Parameters.Add("status", NpgsqlTypes.NpgsqlDbType.Integer).Value = oCoupon.status;
            command.Parameters.Add("customerid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCoupon.customerid;
            command.Parameters.Add("counterid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCoupon.counterid;
            command.Parameters.Add("receivedamt", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oCoupon.receivedamt;
            command.Parameters.Add("vouchervalue", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oCoupon.vouchervalue;
            command.Parameters.Add("issuedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCoupon.issuedby;
            command.Parameters.Add("denomination", NpgsqlTypes.NpgsqlDbType.Json).Value = oCoupon.denomination_json == null ? "null" : oCoupon.denomination_json;
            command.Parameters.Add("givento", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCoupon.givento == null ? "" : oCoupon.givento;
            command.Parameters.Add("seqcode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCoupon.seqcode == null ? "" : oCoupon.seqcode;
            command.Parameters.Add("billsettlementmasterid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCoupon.billsettlementmasterid;
            command.Parameters.Add("issuemode", NpgsqlTypes.NpgsqlDbType.Integer).Value = oCoupon.issuemode;
            command.Parameters.Add("discountpercentage", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oCoupon.discountpercentage;
            command.Parameters.Add("discountvalue", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oCoupon.discountvalue;
            command.Parameters.Add("bankid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCoupon.bankid;
            command.Parameters.Add("ledgerid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCoupon.ledgerid;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCoupon.locationid;
            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCoupon.companyid;
            command.Parameters.Add("paymentmode", NpgsqlTypes.NpgsqlDbType.Integer).Value = oCoupon.paymentmode;
            command.Parameters.Add("paymentreference", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCoupon.paymentreference == null ? "" : oCoupon.paymentreference;
            command.Parameters.Add("issuedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oCoupon.issuedon;
            command.Parameters.Add("printedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oCoupon.issuemode == 0 ? oCoupon.issuedon : oCoupon.printedon;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oCoupon.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            return oCoupon;
        }
        public GiftVoucher Update(GiftVoucher oCoupon)
        {
            if (oCoupon == null || oCoupon.receivedamt != oCoupon.vouchervalue)
            {
                oCoupon.errormessage = "Invalid amount received.";
                return oCoupon;
            }
            if (oCoupon.customerid == 0 || string.IsNullOrEmpty(oCoupon.mobileno) || string.IsNullOrEmpty(oCoupon.name))
            {
                oCoupon.errormessage = "Please enter valid Customer details.";
                return oCoupon;
            }
            if (oCoupon.validfrom == DateTime.MinValue || oCoupon.validto == DateTime.MinValue)
            {
                oCoupon.errormessage = "Invalid Coupon validity.";
                return oCoupon;
            }

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oCoupon);
                    //CoupondetailDataService subCoupondetail = new CoupondetailDataService() { Context = this.Context };
                    //subCoupondetail.Update(transaction, conn, oCoupon.Coupondetail, oCoupon.id);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oCoupon);
            }

            return oCoupon;
        }

        public GiftVoucher Update(NpgsqlTransaction transaction, NpgsqlConnection conn, GiftVoucher oCoupon)
        {
            NpgsqlCommand command = GetCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCoupon.id;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCoupon.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oCoupon.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oCoupon.isactive;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oCoupon.version;
            //command.Parameters.Add("type", NpgsqlTypes.NpgsqlDbType.Integer).Value = oCoupon.type;
            //command.Parameters.Add("cardno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCoupon.cardno == null ? "" : oCoupon.cardno;
            command.Parameters.Add("name", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCoupon.name == null ? "" : oCoupon.name;
            command.Parameters.Add("mobileno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCoupon.mobileno == null ? "" : oCoupon.mobileno;
            command.Parameters.Add("validfrom", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oCoupon.validfrom;
            command.Parameters.Add("validto", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oCoupon.validto;
            //command.Parameters.Add("balance", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oCoupon.balance;
            command.Parameters.Add("status", NpgsqlTypes.NpgsqlDbType.Integer).Value = oCoupon.status;
            command.Parameters.Add("customerid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCoupon.customerid;
            command.Parameters.Add("receivedamt", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oCoupon.receivedamt;
            command.Parameters.Add("vouchervalue", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oCoupon.vouchervalue;
            command.Parameters.Add("issuedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCoupon.issuedby;
            command.Parameters.Add("discountpercentage", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oCoupon.discountpercentage;
            command.Parameters.Add("discountvalue", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oCoupon.discountvalue;
            command.Parameters.Add("bankid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCoupon.bankid;
            command.Parameters.Add("ledgerid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCoupon.ledgerid;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCoupon.locationid;
            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCoupon.companyid;
            command.Parameters.Add("counterid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCoupon.counterid;
            command.Parameters.Add("paymentmode", NpgsqlTypes.NpgsqlDbType.Integer).Value = oCoupon.paymentmode;
            command.Parameters.Add("issuedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oCoupon.issuedon;
            command.Parameters.Add("printedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oCoupon.issuemode == 0 ? oCoupon.issuedon : oCoupon.printedon;
            command.Parameters.Add("paymentreference", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCoupon.paymentreference == null ? "" : oCoupon.paymentreference;
            command.Parameters.Add("denomination", NpgsqlTypes.NpgsqlDbType.Json).Value = oCoupon.denomination_json == null ? "null" : oCoupon.denomination_json;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oCoupon.version = Convert.ToInt32(reader[0].ToString());
                }
            }

            return oCoupon;
        }


        public SchemeCoupon Update(NpgsqlTransaction transaction, NpgsqlConnection conn, SchemeCoupon oCoupon)
        {
            NpgsqlCommand command = GetCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCoupon.id;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCoupon.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oCoupon.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oCoupon.isactive;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oCoupon.version;
            //command.Parameters.Add("type", NpgsqlTypes.NpgsqlDbType.Integer).Value = oCoupon.type;
            //command.Parameters.Add("cardno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCoupon.cardno == null ? "" : oCoupon.cardno;
            command.Parameters.Add("name", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCoupon.name == null ? "" : oCoupon.name;
            command.Parameters.Add("mobileno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = "";
            command.Parameters.Add("validfrom", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oCoupon.validfrom;
            command.Parameters.Add("validto", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oCoupon.validto;
            //command.Parameters.Add("balance", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oCoupon.balance;
            command.Parameters.Add("status", NpgsqlTypes.NpgsqlDbType.Integer).Value = 0;
            command.Parameters.Add("customerid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = 0;
            command.Parameters.Add("receivedamt", NpgsqlTypes.NpgsqlDbType.Numeric).Value = 0;
            command.Parameters.Add("vouchervalue", NpgsqlTypes.NpgsqlDbType.Numeric).Value = 0;
            command.Parameters.Add("issuedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCoupon.issuedby;
            command.Parameters.Add("discountpercentage", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oCoupon.discountpercentage;
            command.Parameters.Add("discountvalue", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oCoupon.discountvalue;
            command.Parameters.Add("bankid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCoupon.bankid;
            command.Parameters.Add("ledgerid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCoupon.ledgerid;
            command.Parameters.Add("counterid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCoupon.counterid;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCoupon.locationid;
            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCoupon.companyid;
            command.Parameters.Add("paymentmode", NpgsqlTypes.NpgsqlDbType.Integer).Value = oCoupon.paymentmode;
            command.Parameters.Add("issuedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oCoupon.issuedon;
            command.Parameters.Add("printedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oCoupon.issuemode == 0 ? oCoupon.issuedon : oCoupon.printedon;
            command.Parameters.Add("paymentreference", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCoupon.paymentreference == null ? "" : oCoupon.paymentreference;
            command.Parameters.Add("denomination", NpgsqlTypes.NpgsqlDbType.Json).Value = oCoupon.rules_json == null ? "null" : oCoupon.rules_json;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oCoupon.version = Convert.ToInt32(reader[0].ToString());
                }
            }

            return oCoupon;
        }

        public GiftVoucher Delete(GiftVoucher oCoupon)
        {
            CustomerCouponsDataService subCoupondetail = new CustomerCouponsDataService() { Context = this.Context };
            subCoupondetail.Context = this.Context;
            oCoupon.errormessage = "";

            var voucherlist = subCoupondetail.Search(new CustomerCouponsCreteria() { coupenid = oCoupon.id });
            if (voucherlist.Exists(e => e.billid > 0))
            {
                oCoupon.errormessage = "The given coupon is already consumed, Can't delete.";
                return oCoupon;
            }

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oCoupon);
                    if (hasSuccessCount)
                        subCoupondetail.DeleteAll(oCoupon.id, oCoupon.modifiedby, conn, transaction);

                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oCoupon);
            }
            return oCoupon;
        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, GiftVoucher oCoupon)
        {
            NpgsqlCommand command = GetCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oCoupon.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oCoupon.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCoupon.id;
            int successCount = command.ExecuteNonQuery();
            return successCount > 0;
        }

        public GiftVoucher DeleteVoucherCode(GiftVoucher oCoupon)
        {
            NpgsqlTransaction transaction = null;
            string SQL_DeleteVoucher = @"update public.vouchers set isactive=false,version=version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where code=@code ";

            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    NpgsqlCommand command = GetCommand(SQL_DeleteVoucher, conn, transaction);
                    command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oCoupon.isactive;
                    command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                    command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
                    command.Parameters.Add("code", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCoupon.name;
                    int successCount = command.ExecuteNonQuery();
                    if (successCount > 1)
                    {
                        oCoupon.errormessage = "Can't Delete the given Voucher, It may be Consumed.";
                        return oCoupon;
                    }
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oCoupon);
            }
            return oCoupon;
        }

        internal GiftVoucherIssue RemoveIssue(GiftVoucherIssue item)
        {

            if (!Context.HasPermission(PermissionSet.revokegiftvoucher, InfyPOS.Common.Action.View))
            {
                throw new Exception("You don't have permission to revoking the issued voucher");
            }

            try
            {
                CustomerCouponsDataService voucherservice = new CustomerCouponsDataService() { Context = this.Context };
                voucherservice.Context = this.Context;
                var exist = voucherservice.Search(new CustomerCouponsCreteria() { id = item.voucherid }).First();

                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    var transaction = conn.BeginTransaction();
                    exist.id = 0;
                    exist.status = 99;
                    exist.code = '~' + exist.code;
                    exist.isactive = false;
                    exist.modifiedby = Context.UserID;
                    exist.modifiedon = DateTimeNow;
                    voucherservice.Insert(transaction, conn, exist);

                    var query = @"update vouchers set issueto=0,issuedby=0,
                                  issuedon=current_date,
                                  issuelocationid=0,issuecounterid=0,
                                  counteramount=0,status=-1 
                                  where id=@id and status=0 and isactive=true";

                    NpgsqlCommand command = GetCommand(query, conn, transaction);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.voucherid;
                    int successCount = command.ExecuteNonQuery();
                    if (successCount == 0)
                    {
                        throw new Exception("Can't find the relevent voucher to remove issue or may consumed in any bill");
                    }
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, item);
            }
            return item;
        }

        internal GiftVoucherIssue IssueVoucher(GiftVoucherIssue item)
        {
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    var query = @"update vouchers set issueto=@issueto,issuedby=@issuedby,
                                  issuedon=@issuedon,ledgerid=@ledgerid,
                                  issuelocationid=@issuelocationid,issuecounterid=@issuecounterid,
                                  counteramount=@counteramount,status=0,ipaymentmode=@ipaymentmode,
                                  ibankid=@ibankid,ipaymentreference=@ipaymentreference
                                  where id=@id and issueto=0 and status=-1 and isactive=true";

                    NpgsqlCommand command = GetCommand(query, conn);
                    command.Parameters.Add("issueto", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.customerid;
                    command.Parameters.Add("issuedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.issuedby;
                    command.Parameters.Add("issuedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
                    command.Parameters.Add("issuelocationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.locationid;
                    command.Parameters.Add("issuecounterid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.counterid;
                    command.Parameters.Add("counteramount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = item.receivedamt;
                    command.Parameters.Add("ibankid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.bankid;
                    command.Parameters.Add("ledgerid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.ledgerid;
                    command.Parameters.Add("ipaymentmode", NpgsqlTypes.NpgsqlDbType.Integer).Value = item.paymentmode;
                    command.Parameters.Add("ipaymentreference", NpgsqlTypes.NpgsqlDbType.Varchar).Value = item.paymentreference == null ? "" : item.paymentreference;
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.voucherid;

                    int successCount = command.ExecuteNonQuery();
                    if (successCount == 0)
                    {
                        query = @"select customer.name customername,employee.name employeename,issuedon from vouchers 
                                  inner join customer on customer.id = issueto
                                  inner join employee on employee.id = issuedby
                                  where vouchers.id=@id";
                        command = GetCommand(query, conn);
                        command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.voucherid;
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                throw new Exception(string.Format("Already issued to {0} by {1} on {2}",
                                    reader.GetString(0), reader.GetString(1), reader.GetDateTime(2).ToString("dd-MM-yyyy")));
                            }
                        }
                        throw new Exception("Can't find the relevent voucher to issue, please contact support");
                    }
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, item);
            }
            return item;
        }

        internal SearchVoucherResult SearchSettlementCoupon(GiftVoucherCreteria item)
        {
            var results = new SearchVoucherResult();
            if (string.IsNullOrEmpty(item.code)) return null;
            BillsettlementmasterDataService billsettlementmasterDataService = new BillsettlementmasterDataService();
            var settlementlist = billsettlementmasterDataService.Search(new BillsettlementmasterCreteria() { code = item.code });

            if(settlementlist.Count == 0)
            {
                var param = new SortedList<string, object>();
                param.Add("billno", item.code);
                using (var reader = ExecuteReader(@"select billsettlementmaster.code from bill
                inner join billsettlement on billsettlement.billid = bill.id
                inner join billsettlementmaster on billsettlementmaster.id =billsettlement.billsettlementmasterid
                where  bill.billno =@billno
                and bill.billtype != 9", param))
                {
                    if (reader.Read() && !reader.IsDBNull(0))
                    {
                        item.code = reader.GetString(0);
                        settlementlist = billsettlementmasterDataService.Search(new BillsettlementmasterCreteria() { code = item.code });
                    }
                }
            }

            if (settlementlist.Count == 1 &&
                settlementlist[0].paymentinfo != null &&
                settlementlist[0].paymentinfo.coupons != null &&
                settlementlist[0].paymentinfo.coupons.Count > 0)
            {
                if (settlementlist[0].customerid > 0)
                {
                    CustomerDataService customerDataService = new CustomerDataService();
                    var customers = customerDataService.Search(new CustomerCreteria() { id = settlementlist[0].customerid });
                    if(customers.Count>0)
                    {
                        results.customername = customers[0].name;
                        results.mobileno = customers[0].mobileno;
                        results.customeraddress = customers[0].address;
                    }
                }
                results.settlementmasterid = settlementlist[0].id;
                results.voucherids = settlementlist[0].paymentinfo.coupons.ConvertAll(x => x.id);
                results.validto = settlementlist[0].paymentinfo.coupons[0].validupto;
                results.name = settlementlist[0].paymentinfo.coupons[0].schemename;
                results.vouchervalue = settlementlist[0].paymentinfo.coupons.Sum(x => x.amount);
                return results;
            }
            return null;
        }

        public GiftVoucher UpdateVoucherValidDate(GiftVoucher oCoupon)
        {
            NpgsqlTransaction transaction = null;
            string SQL_VoucherDate = @"update public.vouchers set validupto=@validto,version=version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id ";

            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    NpgsqlCommand command = GetCommand(SQL_VoucherDate, conn, transaction);
                    command.Parameters.Add("validto", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oCoupon.validto;
                    command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                    command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCoupon.id;
                    int successCount = command.ExecuteNonQuery();
                    if (successCount > 1)
                    {
                        oCoupon.errormessage = "Can't Update the given Voucher, It may be Consumed.";
                        return oCoupon;
                    }
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oCoupon);
            }
            return oCoupon;
        }

    }

}
