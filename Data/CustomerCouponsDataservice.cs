using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
namespace InfyPOS.Data
{
    public class CustomerCouponsDataService : SAASData
    {
        const string SQL_Search = @"select vouchers.id, vouchers.isactive, vouchers.createdby, vouchers.modifiedby, vouchers.version,
vouchers.createdon, vouchers.modifiedon, vouchers.issuedby, vouchers.issuedon, amountaspercentage,filterschemeid,
validupto, amount, vouchers.status, vouchers.code, issueto,coupenid,billid,minbillvalue,maxbilldiscountpercentage,vouchertype,
acceptablelocationid,unlimited,coupons.mobileno,isranged,rangedrule,coupons.name,ex_discount,ex_adiscount,ex_sdiscount,
coupons.discountpercentage,coupons.discountvalue,issuelocationid,issuecounterid,vouchers.validfrom,
customer.name customername,employee.name employeename,customer.mobileno customermobileno,sourcebillsettlementmasterid,otpcustomerid
from vouchers 
left outer join coupons on coupons.id = vouchers.coupenid 
left outer join customer on customer.id = vouchers.issueto
left outer join employee on employee.id = vouchers.issuedby
where vouchers.id=@id";


        const string SQL_Insert = @"INSERT INTO public.vouchers(isactive, createdby, modifiedby, version, createdon, modifiedon, issuedby, amountaspercentage,filterschemeid,validfrom,
                                    issuedon, validupto, amount, status, code, issueto,coupenid,billid,minbillvalue,maxbilldiscountpercentage,vouchertype,acceptablelocationid,
                                    unlimited,uniquekey,isranged,rangedrule,adjustedrewards,ex_discount,ex_adiscount,ex_sdiscount,sourcebillsettlementmasterid,otpcustomerid,issuelocationid,issuecounterid)
                                    VALUES(@isactive, @createdby, @modifiedby, @version, @createdon, @modifiedon, @issuedby, @amountaspercentage,@filterschemeid,@validfrom,
                                    @issuedon, @validupto, @amount, @status, @code, @issueto,@coupenid,@billid,@minbillvalue,@maxbilldiscountpercentage,@vouchertype,@acceptablelocationid,
                                    @unlimited,@uniquekey,@isranged,@rangedrule,@adjustedrewards,@ex_discount,@ex_adiscount,@ex_sdiscount,@sourcebillsettlementmasterid,@otpcustomerid,@issuelocationid,@issuecounterid) RETURNING id";


        const string SQL_Update = @"UPDATE public.vouchers SET modifiedby=@modifiedby, version=@version+1, modifiedon=@modifiedon, issuedby=@issuedby, filterschemeid=@filterschemeid,
                                    issuedon=@issuedon, validupto=@validupto, amount=@amount, status=@status, code=@code,vouchertype=@vouchertype, amountaspercentage=@amountaspercentage,
                                    issueto=@issueto,coupenid=@coupenid,billid=@billid,minbillvalue=@minbillvalue,acceptablelocationid=@acceptablelocationid,validfrom=@validfrom,
                                    maxbilldiscountpercentage=@maxbilldiscountpercentage,unlimited=@unlimited,isranged=@isranged,rangedrule=@rangedrule,otpcustomerid=@otpcustomerid,
                                    ex_discount=@ex_discount,ex_adiscount=@ex_adiscount,ex_sdiscount=@ex_sdiscount,sourcebillsettlementmasterid=@sourcebillsettlementmasterid
                                    where id=@id and version=@version  RETURNING version";

        const string SQL_Delete = @"update public.vouchers set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version  RETURNING version";
        const string SQL_DeleteAll = @"update public.vouchers set isactive=false,modifiedby=@modifiedby,modifiedon=@modifiedon where coupenid=@coupenid";

        internal CustomerCouponsStatus FindStatus(CustomerCoupons result, bool sendotp, Bill bill, decimal billvalue)
        {
            CustomerCouponsStatus customerCouponsStatus = new CustomerCouponsStatus();
            if (result.validfrom.Date != DateTime.MinValue &&
                result.validfrom.Date > DateTimeNow.Date)
            {
                customerCouponsStatus.reason = "Validity Starts On " + result.validfrom.Date.ToString("dd-MM-yyyy");
            }
            else if (result.issuedon.Date > DateTimeNow.Date)
            {
                customerCouponsStatus.reason = "Validity Starts On " + result.issuedon.ToString("dd-MM-yyyy");
            }
            else if (result.validupto < DateTimeNow.Date)
            {
                customerCouponsStatus.reason = "Expired On " + result.validupto.ToString("dd-MM-yyyy");
            }
            else if (result.vouchertype != CustomerCoupons.CouponType.Coupon
                && result.vouchertype != CustomerCoupons.CouponType.BirthdayCoupon)
            {
                customerCouponsStatus.reason = "Not a discount coupon";
            }
            else if (result.billid != 0 && result.billid != bill.billmasterid && !result.unlimited && billvalue > 0)
            {
                customerCouponsStatus.reason = "Already claimed";
            }
            else
            {
                if (result.filterschemeid > 0)
                {
                    billvalue = Services.PromotionManager.Instance(Context.OrganizationID).SchemedBillValue(result.filterschemeid, new List<Bill>(new Bill[] { bill }));
                    billvalue += bill.adjustedcouponamount;
                    customerCouponsStatus.eligiblebarcodes = bill.Billitems.FindAll(x => x.schemeeligble).ConvertAll(x => x.barcode);
                    if (billvalue < result.amount)
                    {
                        result.amount = billvalue;
                        result.eligiblebarcodes = customerCouponsStatus.eligiblebarcodes;
                        result.hasfilterscheme = true;
                        customerCouponsStatus.hasfilterscheme = true;
                        customerCouponsStatus.reason = "Avilable bill value " + billvalue.ToString("N0");
                    }
                }
                else
                {
                    bill.Billitems.ForEach(x => x.schemeeligble = true);
                    if (result.ex_discount)
                    {
                        bill.Billitems.FindAll(x => (x.discount - x.schemediscount) > 0).ForEach(x => x.schemeeligble = false);
                    }
                    if (result.ex_adiscount)
                    {
                        bill.Billitems.FindAll(x => x.additionaldiscount > 0).ForEach(x => x.schemeeligble = false);
                    }
                    if (result.ex_sdiscount)
                    {
                        bill.Billitems.FindAll(x => x.schemediscount > 0).ForEach(x => x.schemeeligble = false);
                    }
                    //billvalue = bill.Billitems.Sum(x => x.receivable);
                }


                if (result.isranged && result.rangedrule != null && result.rangedrule.Count > 0)
                {
                    if (result.rangedrule.Exists(e => e.fromamount <= billvalue && e.toamount >= billvalue) ||
                        result.rangedrule.Exists(x=>x.toamount<billvalue))
                    {
                        var _newrule = result.rangedrule.Exists(e => e.fromamount <= billvalue && e.toamount >= billvalue) ? (
                            result.rangedrule.FindAll(e => e.fromamount <= billvalue && e.toamount >= billvalue).OrderByDescending(x => x.amount).First()) :
                            (result.rangedrule.OrderByDescending(x => x.amount).First());

                        if (_newrule != null)
                        {
                            result.amountaspercentage = _newrule.amountaspercentage;
                            result.amount = _newrule.amount;
                        }
                        else if (result.rangedrule.Exists(y => y.fromamount < billvalue))
                        {
                            _newrule = result.rangedrule.FindAll(y => y.fromamount < billvalue).OrderBy(x => x.fromamount).Last();
                            result.amountaspercentage = _newrule.amountaspercentage;
                            result.amount = _newrule.amount;
                        }
                        else
                        {
                            result.amount = 0;
                            billvalue = 0;
                            customerCouponsStatus.reason = "bill value should be between " + result.rangedrule[0].fromamount + "  to " + result.rangedrule[0].toamount;
                            return customerCouponsStatus;
                        }
                    }
                    if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[bill.locationid].sale_includeallrangecoupon)
                    {
                        var _newrule = result.rangedrule.OrderBy(x => x.amount).Last();
                        result.amountaspercentage = _newrule.amountaspercentage;
                        result.amount = _newrule.amount;
                    }
                    else
                    {
                        result.amount = 0;
                        billvalue = 0;
                        customerCouponsStatus.reason = "bill value should be between " + result.rangedrule[0].fromamount + "  to " + result.rangedrule[0].toamount;
                        return customerCouponsStatus;
                    }
                }

                customerCouponsStatus.couponid = result.id;
                customerCouponsStatus.available = result.amountaspercentage ? Math.Round(result.amount * billvalue / 100, 0) : result.amount;
                if (result.minbillvalue > 0 && result.minbillvalue > billvalue)
                {
                    customerCouponsStatus.reason = "Minimum bill value " + result.minbillvalue.ToString("N0");
                }
                else if (result.amountaspercentage)
                {
                    customerCouponsStatus.convertable = customerCouponsStatus.available > billvalue ? billvalue : customerCouponsStatus.available;
                }
                else if (result.maxbilldiscountpercentage > 0)
                {
                    var maximum = billvalue * result.maxbilldiscountpercentage / 100;
                    if (maximum < customerCouponsStatus.available)
                    {
                        customerCouponsStatus.reason = string.Format("Maximum {0} or {1}% of bill value", customerCouponsStatus.available.ToString("N2"), result.maxbilldiscountpercentage.ToString("N2"));
                        //customerCouponsStatus.convertable = Math.Floor(maximum);
                        customerCouponsStatus.convertable = Math.Floor(customerCouponsStatus.available);
                    }
                    else
                    {
                        //customerCouponsStatus.convertable = customerCouponsStatus.available;
                        customerCouponsStatus.convertable = Math.Floor(maximum);
                    }
                }
                else
                {
                    customerCouponsStatus.convertable = customerCouponsStatus.available > billvalue ? billvalue : customerCouponsStatus.available;
                }

                if (sendotp && result.otpcustomerid > 0 && result.amount > 0)
                {
                    CustomerDataService customerDataService = new CustomerDataService() { Context = this.Context };
                    var customer = new Customer()
                    {
                        id = result.otpcustomerid,
                        claimamount = result.amount
                    };
                    customerDataService.SendOTP(customer);
                    customerCouponsStatus.otpcustomerid = result.otpcustomerid;
                    customerCouponsStatus.reason += " OTP Sent to " + customer.name + " at *****" + customer.mobileno.Trim().Substring(5);
                }
            }
            return customerCouponsStatus;
        }


        public List<CustomerCoupons> Search(CustomerCouponsCreteria _vouchers)
        {
            List<CustomerCoupons> lst = new List<CustomerCoupons>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_vouchers.id != 0)
                {
                    command = GetCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _vouchers.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("vouchers.isactive", true, "=");

                    if (_vouchers.acceptablelocationid > 0)
                    {
                        qb.AddORParameter("acceptablelocationid", _vouchers.acceptablelocationid, "=", " acceptablelocationid<=0 ");
                    }
                    if (_vouchers.ids != null && _vouchers.ids.Count > 0)
                    {
                        qb.AddInParamer("vouchers.id", string.Join(",", _vouchers.ids));
                    }
                    else if (_vouchers.coupenid > 0)
                    {
                        qb.AddParameter("vouchers.coupenid", _vouchers.coupenid, "=");
                    }
                    else if (_vouchers.coupenid == -9)
                    {
                        qb.AddParameter("vouchers.coupenid", 0, "=");
                        qb.AddParameter("vouchers.vouchertype", 5, "=");
                    }
                    else if (!string.IsNullOrEmpty(_vouchers.code))
                    {
                        qb.AddParameter("vouchers.code", _vouchers.code, "=");
                    }
                    if (_vouchers.issueto < 0)
                        qb.AddParameter("vouchers.issueto", 0, "=");
                    qb.SortField = "vouchers.id";
                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        CustomerCoupons ovouchers = new CustomerCoupons();
                        ovouchers.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        ovouchers.filterschemeid = reader["filterschemeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["filterschemeid"]);
                        ovouchers.type = reader["vouchertype"] == DBNull.Value ? 0 : Convert.ToInt32(reader["vouchertype"]);
                        ovouchers.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);

                        ovouchers.ex_discount = reader["ex_discount"] == DBNull.Value ? false : Convert.ToBoolean(reader["ex_discount"]);
                        ovouchers.ex_adiscount = reader["ex_adiscount"] == DBNull.Value ? false : Convert.ToBoolean(reader["ex_adiscount"]);
                        ovouchers.ex_sdiscount = reader["ex_sdiscount"] == DBNull.Value ? false : Convert.ToBoolean(reader["ex_sdiscount"]);

                        ovouchers.amountaspercentage = reader["amountaspercentage"] == DBNull.Value ? false : Convert.ToBoolean(reader["amountaspercentage"]);
                        ovouchers.unlimited = reader["unlimited"] == DBNull.Value ? false : Convert.ToBoolean(reader["unlimited"]);
                        ovouchers.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        ovouchers.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        ovouchers.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        ovouchers.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        ovouchers.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"].ToString());
                        ovouchers.issuedby = reader["issuedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["issuedby"].ToString());
                        ovouchers.issuedon = reader["issuedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["issuedon"].ToString());
                        ovouchers.validupto = reader["validupto"] == DBNull.Value ? DateTime.MinValue : (DateTime)reader["validupto"];
                        ovouchers.validfrom = reader["validfrom"] == DBNull.Value ? DateTime.MinValue : (DateTime)reader["validfrom"];
                        ovouchers.code = reader["code"] == DBNull.Value ? "" : reader["code"].ToString();
                        ovouchers.name = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
                        ovouchers.issueto = reader["issueto"] == DBNull.Value ? 0 : Convert.ToInt64(reader["issueto"].ToString());
                        ovouchers.coupenid = reader["coupenid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["coupenid"]);
                        ovouchers.billid = reader["billid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["billid"]);
                        ovouchers.acceptablelocationid = reader["acceptablelocationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["acceptablelocationid"]);
                        ovouchers.amount = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"]);
                        ovouchers.minbillvalue = reader["minbillvalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["minbillvalue"]);
                        ovouchers.maxbilldiscountpercentage = reader["maxbilldiscountpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["maxbilldiscountpercentage"]);
                        ovouchers.mobileno = reader["mobileno"] == DBNull.Value ? "" : reader["mobileno"].ToString();
                        ovouchers.status = reader["status"] == DBNull.Value ? 0 : Convert.ToInt32(reader["status"]);
                        if (reader["customermobileno"] != DBNull.Value &&
                            !string.IsNullOrEmpty(reader["customermobileno"].ToString()))
                        {
                            ovouchers.mobileno = reader["customermobileno"] == DBNull.Value ? "" : reader["customermobileno"].ToString();
                        }

                        ovouchers.isranged = reader["isranged"] == DBNull.Value ? false : Convert.ToBoolean(reader["isranged"]);
                        ovouchers.rangedrule_json = reader["rangedrule"] == DBNull.Value ? "null" : reader["rangedrule"].ToString();
                        if(ovouchers.isranged && ovouchers.rangedrule != null && ovouchers.rangedrule.Count>1)
                        {
                            ovouchers.rangedrule = new List<CustomerCoupons.RangeRule>(new CustomerCoupons.RangeRule[] { ovouchers.rangedrule.OrderByDescending(x => x.amount).First() });
                        }

                        ovouchers.sourcebillsettlementmasterid = reader["sourcebillsettlementmasterid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sourcebillsettlementmasterid"]);
                        ovouchers.otpcustomerid = reader["otpcustomerid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["otpcustomerid"]);


                        ovouchers.locationid = reader["issuelocationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["issuelocationid"]);
                        ovouchers.counterid = reader["issuecounterid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["issuecounterid"]);
                        ovouchers.discountvalue = reader["discountvalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discountvalue"]);
                        ovouchers.discountpercentage = reader["discountpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discountpercentage"]);


                        ovouchers.employeename = reader["employeename"] == DBNull.Value ? "" : reader["employeename"].ToString();
                        ovouchers.customername = reader["customername"] == DBNull.Value ? "" : reader["customername"].ToString();

                        lst.Add(ovouchers);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);
                throw exp;
            }

            return lst;
        }

        public List<CustomerCoupons> SearchVouchers(CustomerCoupons oVouchers)
        {
            // vouchertype - 2, 5 are coupons and birthday coupons, those are not considered as vouchers at the settlement
            // vouchertype - 4 are considered as claimed
            const string SQL_SearchVouchers = @"select sum(amount) as amount,code from vouchers 
            where isactive=true and code=@vouchercode and (vouchertype not in (2,5,4)) 
                and COALESCE(validfrom, issuedon)<=@fromdate and validupto>=@todate group by code
            UNION ALL 
            select sum(amount) as amount,code from vouchers 
            where isactive=true and code=@vouchercode and vouchertype = 4 group by code ";

            List<CustomerCoupons> lst = new List<CustomerCoupons>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                command = GetCommand(SQL_SearchVouchers, conn);
                command.Parameters.Add("vouchercode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oVouchers.code;
                command.Parameters.Add("fromdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow.Date.AddDays(1);
                command.Parameters.Add("todate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow.Date;
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string code = reader["code"] == DBNull.Value ? "" : reader["code"].ToString();
                        decimal amount = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"]);
                        if (lst.Exists(e => e.code == code))
                        {
                            var item = lst.Find(e => e.code == code);
                            item.amount += amount;
                        }
                        else
                        {
                            CustomerCoupons oitems = new CustomerCoupons();
                            oitems.code = reader["code"] == DBNull.Value ? "" : reader["code"].ToString();
                            oitems.amount = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"]);
                            lst.Add(oitems);
                        }
                    }
                }
                conn.Close();
                lst.RemoveAll(e => e.amount <= 0);
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oVouchers);
            }
            return lst;
        }

        public CustomerCoupons Insert(CustomerCoupons oVouchers)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    Insert(transaction, conn, oVouchers);
                    if (oVouchers.vouchertype == CustomerCoupons.CouponType.Coupon &&
                        oVouchers.adjustedrewards > 0)
                    {
                        CustomerrewardsDataService customerrewardsDataService = new CustomerrewardsDataService() { Context = this.Context };
                        var availablerewards = customerrewardsDataService.Search(new CustomerrewardsCreteria() { customerid = oVouchers.issueto });
                        long avail = oVouchers.adjustedrewards;
                        foreach (var item in availablerewards)
                        {
                            if (avail > 0)
                            {
                                if ((item.rewards - item.adjusted) > avail)
                                {
                                    var param = new SortedList<string, object>();
                                    param.Add("id", item.id);
                                    param.Add("adjusted", avail);
                                    if (ExecuteQuery("update Customerrewards set adjusted=adjusted+@adjusted where id=@id and (rewards-(adjusted+@adjusted))>=0",
                                        param, conn, transaction) > 0)
                                    {
                                        avail = 0;
                                    }
                                }
                                else if ((item.rewards - item.adjusted) > 0)
                                {
                                    var param = new SortedList<string, object>();
                                    param.Add("id", item.id);
                                    param.Add("adjusted", (item.rewards - item.adjusted));
                                    if (ExecuteQuery("update Customerrewards set adjusted=adjusted+@adjusted where id=@id and (rewards-(adjusted+@adjusted))>=0",
                                        param, conn, transaction) > 0)
                                    {
                                        avail -= Convert.ToInt64(item.rewards - item.adjusted);
                                    }
                                }
                            }
                        }

                        if (avail > 0)
                        {
                            throw new Exception("Customer don't have enough reward point to adjust");
                        }
                        else
                        {
                            var param = new SortedList<string, object>();
                            param.Add("id", oVouchers.issueto);
                            ExecuteQuery("update customer set otp='' where id=@id", param, conn, transaction);
                        }
                    }
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);

            }

            return oVouchers;
        }

        public CustomerCoupons Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, CustomerCoupons oVouchers)
        {
            if (string.IsNullOrEmpty(oVouchers.code))
            {
                while (true)
                {
                    if (oVouchers.vouchertype == CustomerCoupons.CouponType.BirthdayCoupon)
                        oVouchers.code = WebAPI.Data.Token.ShortMonths[DateTimeNow.Month - 1] + DateTimeNow.Year.ToString().Substring(3) + WebAPI.Data.Token.RandomString(7, "ABCDEFGHIJKLMNOPQRSTUVWXYZ23456789");
                    else if (oVouchers.vouchertype != CustomerCoupons.CouponType.Inbound)
                        oVouchers.code = "#" + WebAPI.Data.Token.ShortMonths[DateTimeNow.Month - 1] + DateTimeNow.Year.ToString().Substring(3) + WebAPI.Data.Token.RandomString(7, "ABCDEFGHIJKLMNOPQRSTUVWXYZ23456789");
                    else
                        oVouchers.code = WebAPI.Data.Token.ShortMonths[DateTimeNow.Month - 1] + DateTimeNow.Year.ToString().Substring(3) + WebAPI.Data.Token.RandomString(7, "ABCDEFGHJKLMNPQRSTUVWXY23456789");
                    using (var reader = ExecuteReader("Select vouchers.id from vouchers where vouchers.code='" + WebAPI.Data.Utility.SafeSQL(oVouchers.code) + "' limit 1;",
                         new SortedList<string, object>(),
                         conn, transaction))
                    {
                        if (!reader.Read())
                            break;
                    }
                }
            }


            NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oVouchers.isactive;

            command.Parameters.Add("ex_adiscount", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oVouchers.ex_adiscount;
            command.Parameters.Add("ex_sdiscount", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oVouchers.ex_sdiscount;
            command.Parameters.Add("ex_discount", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oVouchers.ex_discount;

            command.Parameters.Add("amountaspercentage", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oVouchers.amountaspercentage;
            command.Parameters.Add("unlimited", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oVouchers.unlimited;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("filterschemeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oVouchers.filterschemeid;
            command.Parameters.Add("issuelocationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oVouchers.locationid;
            command.Parameters.Add("issuecounterid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oVouchers.counterid;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oVouchers.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("issuedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oVouchers.issuedby;
            command.Parameters.Add("issuedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oVouchers.issuedon;
            command.Parameters.Add("validupto", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oVouchers.validupto;
            command.Parameters.Add("validfrom", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oVouchers.validfrom;
            command.Parameters.Add("amount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oVouchers.amount;
            command.Parameters.Add("status", NpgsqlTypes.NpgsqlDbType.Integer).Value = oVouchers.status;
            command.Parameters.Add("code", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oVouchers.code == null ? "" : oVouchers.code;
            command.Parameters.Add("uniquekey", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oVouchers.uniquekey == null ? "" : oVouchers.uniquekey;
            command.Parameters.Add("adjustedrewards", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oVouchers.adjustedrewards;
            command.Parameters.Add("issueto", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oVouchers.issueto;
            command.Parameters.Add("coupenid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oVouchers.coupenid;
            command.Parameters.Add("billid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oVouchers.billid;
            command.Parameters.Add("sourcebillsettlementmasterid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oVouchers.sourcebillsettlementmasterid;
            command.Parameters.Add("otpcustomerid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oVouchers.otp ? oVouchers.otpcustomerid : 0;

            command.Parameters.Add("acceptablelocationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oVouchers.acceptablelocationid;
            command.Parameters.Add("vouchertype", NpgsqlTypes.NpgsqlDbType.Integer).Value = oVouchers.vouchertype;
            command.Parameters.Add("minbillvalue", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oVouchers.minbillvalue;
            command.Parameters.Add("maxbilldiscountpercentage", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oVouchers.maxbilldiscountpercentage;
            command.Parameters.Add("isranged", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oVouchers.isranged;
            command.Parameters.Add("rangedrule", NpgsqlTypes.NpgsqlDbType.Json).Value = oVouchers.rangedrule == null ? "null" : oVouchers.rangedrule_json;

            int trycount = 0;
            while (trycount < 3)
            {
                try
                {
                    trycount++;
                    using (System.Data.IDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            oVouchers.id = Convert.ToInt64(reader[0].ToString());
                        }
                    }
                    break;
                }
                catch (Exception exp)
                {
                    if (oVouchers.vouchertype == CustomerCoupons.CouponType.BirthdayCoupon)
                        oVouchers.code = WebAPI.Data.Token.ShortMonths[DateTimeNow.Month - 1] + DateTimeNow.Year.ToString().Substring(3) + WebAPI.Data.Token.RandomString(7, "ABCDEFGHIJKLMNOPQRSTUVWXYZ23456789");
                    else if (oVouchers.vouchertype != CustomerCoupons.CouponType.Inbound)
                        oVouchers.code = "#" + WebAPI.Data.Token.ShortMonths[DateTimeNow.Month - 1] + DateTimeNow.Year.ToString().Substring(3) + WebAPI.Data.Token.RandomString(7, "ABCDEFGHIJKLMNOPQRSTUVWXYZ23456789");
                    else
                        oVouchers.code = WebAPI.Data.Token.ShortMonths[DateTimeNow.Month - 1] + DateTimeNow.Year.ToString().Substring(3) + WebAPI.Data.Token.RandomString(7, "ABCDEFGHJKLMNPQRSTUVWXY23456789");
                    command.Parameters["code"].Value = oVouchers.code;
                    command.Parameters["version"].Value = oVouchers.version + trycount;
                }
            }
            return oVouchers;
        }

        internal SortedDictionary<long, string> SearchUnique(string uniquekey)
        {
            if (string.IsNullOrEmpty(uniquekey)) return new SortedDictionary<long, string>();

            SortedDictionary<long, string> lst = new SortedDictionary<long, string>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                NpgsqlCommand command = GetCommand("Select issueto,code from vouchers where uniquekey=@uniquekey", conn);
                command.Parameters.Add("uniquekey", NpgsqlTypes.NpgsqlDbType.Varchar).Value = uniquekey;

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (!reader.IsDBNull(0) && !lst.ContainsKey(reader.GetInt64(0)))
                            lst.Add(reader.GetInt64(0), reader.GetString(1));
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);
            }
            return lst;
        }


        public CustomerCoupons Update(CustomerCoupons oVouchers)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oVouchers);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);

            }

            return oVouchers;
        }

        public CustomerCoupons Update(NpgsqlTransaction transaction, NpgsqlConnection conn, CustomerCoupons oVouchers)
        {

            NpgsqlCommand command = GetCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oVouchers.id;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oVouchers.isactive;
            command.Parameters.Add("ex_adiscount", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oVouchers.ex_adiscount;
            command.Parameters.Add("ex_sdiscount", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oVouchers.ex_sdiscount;
            command.Parameters.Add("ex_discount", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oVouchers.ex_discount;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oVouchers.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oVouchers.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oVouchers.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("issuedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oVouchers.issuedby;
            command.Parameters.Add("issuedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oVouchers.issuedon;
            command.Parameters.Add("validupto", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oVouchers.validupto;
            command.Parameters.Add("validfrom", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oVouchers.validfrom;
            command.Parameters.Add("amount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oVouchers.amount;
            command.Parameters.Add("unlimited", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oVouchers.unlimited;
            command.Parameters.Add("status", NpgsqlTypes.NpgsqlDbType.Integer).Value = oVouchers.status;
            command.Parameters.Add("filterschemeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oVouchers.filterschemeid;
            command.Parameters.Add("code", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oVouchers.code == null ? "" : oVouchers.code;
            command.Parameters.Add("issueto", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oVouchers.issueto;
            command.Parameters.Add("coupenid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oVouchers.coupenid;
            command.Parameters.Add("billid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oVouchers.billid;
            command.Parameters.Add("sourcebillsettlementmasterid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oVouchers.sourcebillsettlementmasterid;
            command.Parameters.Add("otpcustomerid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oVouchers.otpcustomerid;
            command.Parameters.Add("minbillvalue", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oVouchers.minbillvalue;
            command.Parameters.Add("maxbilldiscountpercentage", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oVouchers.maxbilldiscountpercentage;
            command.Parameters.Add("amountaspercentage", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oVouchers.amountaspercentage;
            command.Parameters.Add("vouchertype", NpgsqlTypes.NpgsqlDbType.Integer).Value = oVouchers.vouchertype;
            command.Parameters.Add("acceptablelocationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oVouchers.acceptablelocationid;
            command.Parameters.Add("isranged", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oVouchers.isranged;
            command.Parameters.Add("rangedrule", NpgsqlTypes.NpgsqlDbType.Json).Value = oVouchers.rangedrule == null ? "null" : oVouchers.rangedrule_json;

            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oVouchers.version = Convert.ToInt32(reader[0]);
                }
            }

            return oVouchers;
        }

        public bool Delete(CustomerCoupons oVouchers)
        {

            NpgsqlTransaction transaction = null;
            int successCount = 0;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = GetCommand(SQL_Delete, conn, transaction);
                    command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oVouchers.isactive;
                    command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                    command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
                    command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oVouchers.version;
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oVouchers.id;
                    successCount = command.ExecuteNonQuery();
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);
            }
            return successCount > 0;
        }
        public bool DeleteAll(long coupenid, long userid, NpgsqlConnection conn, NpgsqlTransaction transaction)
        {
            int successCount = 0;
            NpgsqlCommand command = GetCommand(SQL_DeleteAll, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = false;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("coupenid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = coupenid;
            successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

        internal void ApplyCoupon(NpgsqlTransaction transaction, NpgsqlConnection conn,
            string couponcode,
            long couponid,
            decimal couponamount,
            Bill oBill,
            decimal billvalue)
        {
            var items = Search(new CustomerCouponsCreteria()
            {
                id = couponid
            });

            if (items.Count != 1 || items[0].code != couponcode)
                throw new Exception(couponcode + " coupon is not valid");

            var status = FindStatus(items[0], false, oBill, billvalue > 0 ? billvalue : oBill.receivable + oBill.additionaldiscount);

            if (!string.IsNullOrEmpty(status.reason) && status.reason.StartsWith("Minimum"))
                throw new Exception(couponcode + " got exception - " + status.reason);

            if (status.convertable + 1 < couponamount && (oBill.receivable + oBill.additionaldiscount) > couponamount)
            {
                throw new Exception(couponcode + " got exception - Maximum possible amount " + status.convertable);
            }

            const string SQL_Claim = @"update public.vouchers set version=version+1,modifiedby=@modifiedby,
                                            modifiedon=@modifiedon,billid=@billid,billmasterid=@billmasterid where id=@id 
                                            and (billid = 0 or unlimited=true or billmasterid=@billmasterid) 
                                            and (isranged=true or amountaspercentage = true or amount>=@amount) RETURNING version";
            NpgsqlCommand command = GetCommand(SQL_Claim, conn, transaction);
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = this.Context.UserID;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("amount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = couponamount;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = couponid;
            command.Parameters.Add("billid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = (oBill.receivable > 0 || oBill.billtype == Bill.BillTypes.Cash || oBill.billtype == Bill.BillTypes.Card) ? oBill.id : 0;
            command.Parameters.Add("billmasterid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = (oBill.receivable > 0 || oBill.billtype == Bill.BillTypes.Cash || oBill.billtype == Bill.BillTypes.Card) ? oBill.billmasterid : 0;
            if (command.ExecuteNonQuery() != 1)
            {
                throw new Exception(couponcode + " got exception - Can't claim");
            }

            if (!string.IsNullOrEmpty(InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.integration_coupon))
            {
                CouponManager couponManager = new CouponManager(InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.integration_coupon);
                couponManager.Redim(couponcode,oBill.billno);
            }
        }
    }
}
