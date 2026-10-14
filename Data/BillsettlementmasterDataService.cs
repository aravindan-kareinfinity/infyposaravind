using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
using InfyPOS.Processors;

namespace InfyPOS.Data

{
    public class BillsettlementmasterDataService : SAASData
    {
        /*  
            update billsettlementmaster set customerid = billsettlement.customerid
            from billsettlement
            where billsettlement.billsettlementmasterid = billsettlementmaster.id
            update billsettlementmaster set paid_refund = (case when billsettlementmaster.paidamount<0 then cast(paymentinfo->>'cash_return' as numeric) else 0 end)
        */

        const string SQL_Search = @"select billsettlementmaster.id, billsettlementmaster.isactive, billsettlementmaster.createdby, billsettlementmaster.modifiedby, billsettlementmaster.version, billsettlementmaster.createdon,
billsettlementmaster.modifiedon, billsettlementmaster.mode, billsettlementmaster.billamount, paymentinfo, customerid, paidamount, billsettlementmaster.code, settlementon, discount, receivable, tax, 
billmastercode,cashierid,counterid,employee.name cashier,referencelist.name counter,billadditionaldiscount,billsettlementmaster.locationid,billsettlementmaster.companyid,
loanproviderid,loan_paid,loan_reference,swipbankid,swipmachineid,intvendor,intmode from billsettlementmaster 
left outer join employee on employee.id = cashierid
left outer join referencelist on referencelist.id =counterid
where billsettlementmaster.id=@id";

        //paid_cash ,paid_card ,paid_credit ,paid_voucher ,paid_debt

        const string SQL_Insert = @"INSERT INTO public.billsettlementmaster(isactive, createdby, modifiedby, version, createdon, modifiedon, mode, billamount, paymentinfo, customerid, paidamount, 
code, settlementon, discount, receivable, tax, billmastercode,cashierid,counterid,billadditionaldiscount,locationid,paid_cash ,paid_card , paid_upi ,paid_credit ,paid_voucher ,paid_debt,paid_refund,paid_discount,companyid,
loanproviderid,loan_paid,loan_reference,swipbankid,swipmachineid,intvendor,intmode,tzoffset) 
VALUES(@isactive, @createdby, @modifiedby, @version, @createdon, @modifiedon, @mode, @billamount, @paymentinfo, @customerid,@paidamount, @code, @settlementon, @discount, @receivable, @tax,
@billmastercode,@cashierid,@counterid,@billadditionaldiscount,@locationid,@paid_cash ,@paid_card, @paid_upi ,@paid_credit ,@paid_voucher ,@paid_debt,@paid_refund,@paid_discount,@companyid,
@loanproviderid,@loan_paid,@loan_reference,@swipbankid,@swipmachineid,@intvendor,@intmode,@tzoffset) RETURNING id";


        const string SQL_Update = @"UPDATE public.billsettlementmaster SET modifiedby=@modifiedby, version=@version+1, modifiedon=@modifiedon, mode=@mode, billamount=@billamount, paymentinfo=@paymentinfo, 
customerid=@customerid, paidamount=@paidamount, code=@code, settlementon=@settlementon, discount=@discount, receivable=@receivable, tax=@tax, billmastercode=@billmastercode, cashierid=@cashierid,
counterid=@counterid, billadditionaldiscount=@billadditionaldiscount,locationid=@locationid,paid_cash=@paid_cash,paid_card=@paid_card,paid_upi=@paid_upi,paid_credit=@paid_credit,paid_voucher=@paid_voucher,
paid_debt=@paid_debt,paid_refund=@paid_refund,paid_discount=@paid_discount,companyid=@companyid,loanproviderid=@loanproviderid,loan_paid=@loan_paid,
loan_reference=@loan_reference,swipbankid=@swipbankid,swipmachineid=@swipmachineid,intvendor=@intvendor,intmode=@intmode where id=@id and version=@version RETURNING version";

        const string SQL_Delete = @"update public.billsettlementmaster set isactive=false,version=version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id";

        public List<Billsettlementmaster> Search(BillsettlementmasterCreteria _billsettlementmaster)
        {
            List<Billsettlementmaster> lst = new List<Billsettlementmaster>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_billsettlementmaster.id != 0)
                {
                    command = GetCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _billsettlementmaster.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("billsettlementmaster.isactive", true, "=");

                    if (!string.IsNullOrEmpty(_billsettlementmaster.code))
                        qb.AddParameter("billsettlementmaster.code", _billsettlementmaster.code, "=");
                    else if (_billsettlementmaster.ids != null)
                    {
                        qb.AddInParamer("billsettlementmaster.id", string.Join<long>(",", _billsettlementmaster.ids));
                    }
                    else if (_billsettlementmaster.billids != null)
                    {
                        qb.AddInParamer("billsettlementmaster.id",
                            "select billsettlementmasterid from billsettlement where isactive = true and billid in (" + string.Join<long>(", ", _billsettlementmaster.billids) + ")");
                    }
                    else if (!string.IsNullOrEmpty(_billsettlementmaster.billnos))
                    {
                        qb.AddInParamer("billsettlementmaster.id",
                            "select billsettlementmasterid from billsettlement where isactive = true and billno in ("
                            + string.Join<string>(", ", _billsettlementmaster.billnos.Split(',').ToList().ConvertAll(y => "'" + y + "'")) + ")");
                    }
                    else
                    {
                        qb.Limit = 100;
                    }
                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Billsettlementmaster obillsettlementmaster = new Billsettlementmaster();
                        obillsettlementmaster.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        obillsettlementmaster.locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                        obillsettlementmaster.casherid = reader["cashierid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["cashierid"]);
                        obillsettlementmaster.counterid = reader["counterid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["counterid"]);
                        obillsettlementmaster.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        obillsettlementmaster.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        obillsettlementmaster.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        obillsettlementmaster.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        obillsettlementmaster.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        obillsettlementmaster.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        obillsettlementmaster.mode = reader["mode"] == DBNull.Value ? 0 : Convert.ToInt32(reader["mode"]);
                        obillsettlementmaster.billamount = reader["billamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["billamount"]);
                        obillsettlementmaster.paymentinfo_json = reader["paymentinfo"] == DBNull.Value ? "null" : reader["paymentinfo"].ToString();
                        obillsettlementmaster.customerid = reader["customerid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["customerid"]);
                        obillsettlementmaster.paidamount = reader["paidamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["paidamount"]);
                        obillsettlementmaster.code = reader["code"] == DBNull.Value ? "" : reader["code"].ToString();
                        obillsettlementmaster.cashiername = reader["cashier"] == DBNull.Value ? "" : reader["cashier"].ToString();
                        obillsettlementmaster.countername = reader["counter"] == DBNull.Value ? "" : reader["counter"].ToString();
                        obillsettlementmaster.settlementon = reader["settlementon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["settlementon"]);
                        obillsettlementmaster.discount = reader["discount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discount"]);
                        obillsettlementmaster.billadditionaldiscount = reader["billadditionaldiscount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["billadditionaldiscount"]);
                        obillsettlementmaster.receivable = reader["receivable"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["receivable"]);
                        obillsettlementmaster.tax = reader["tax"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["tax"]);
                        obillsettlementmaster.billmastercode = reader["billmastercode"] == DBNull.Value ? "" : reader["billmastercode"].ToString();
                        obillsettlementmaster.companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"]);

                        obillsettlementmaster.loanproviderid = reader["loanproviderid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["loanproviderid"]);
                        obillsettlementmaster.swipbankid = reader["swipbankid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["swipbankid"]);
                        obillsettlementmaster.swipmachineid = reader["swipmachineid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["swipmachineid"]);
                        obillsettlementmaster.loan_paid = reader["loan_paid"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["loan_paid"]);
                        obillsettlementmaster.loan_reference = reader["loan_reference"] == DBNull.Value ? "" : reader["loan_reference"].ToString();
                        obillsettlementmaster.intvendor = reader["intvendor"] == DBNull.Value ? 0 : Convert.ToInt32(reader["intvendor"]);
                        obillsettlementmaster.intmode = reader["intmode"] == DBNull.Value ? "" : reader["intmode"].ToString();
                        lst.Add(obillsettlementmaster);
                    }
                }
                if ((_billsettlementmaster.id > 0 || !string.IsNullOrEmpty(_billsettlementmaster.code)
                    || _billsettlementmaster.ids != null || _billsettlementmaster.billnos != null
                    || _billsettlementmaster.billids != null))
                {
                    BillsettlementDataService subBillsettlement = new BillsettlementDataService() { Context = this.Context };
                    subBillsettlement.Context = this.Context;
                    foreach (var item in lst)
                    {
                        item.Billsettlement = subBillsettlement.SearchAll(item.id);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _billsettlementmaster);
            }
            return lst;
        }

        internal Billsettlementmaster Search(SettlementPrint item)
        {
            if (item.id == 0)
            {
                if (!string.IsNullOrEmpty(item.billno))
                {
                    var param = new SortedList<string, object>();
                    param.Add("billno", item.billno);
                    using (var reader = ExecuteReader(@"select billsettlementmasterid from billsettlement where billno=@billno
                                    order by billid desc", param))
                    {
                        if (reader.Read())
                        {
                            item.id = reader.GetInt64(0);
                        }
                    }
                }
                else
                {
                    var param = new SortedList<string, object>();
                    param.Add("billno", item.billmasterno);
                    using (var reader = ExecuteReader(@"select id from billsettlementmaster where code=@billno", param))
                    {
                        if (reader.Read())
                        {
                            item.id = reader.GetInt64(0);
                        }
                    }
                }
            }
            if (item.id != 0)
            {
                var result = Search(new BillsettlementmasterCreteria()
                {
                    id = item.id
                });
                if (result.Count > 0)
                    return result[0];
            }
            return null;
        }

        public Billsettlementmaster InsertDirect(Billsettlementmaster oBillsettlementmaster)
        {
            NpgsqlTransaction transaction = null;
            using (NpgsqlConnection conn = CreateConnection())
            {
                conn.Open();
                transaction = conn.BeginTransaction();
                try
                {
                    InsertDirect(transaction, conn, oBillsettlementmaster);

                    transaction.Commit();
                    conn.Close();
                }
                catch (Exception exp)
                {
                    WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oBillsettlementmaster);
                    oBillsettlementmaster.code = "";
                    oBillsettlementmaster.id = 0;
                }
            }
            return oBillsettlementmaster;
        }
        public Billsettlementmaster InsertDirect(NpgsqlTransaction transaction, NpgsqlConnection conn, Billsettlementmaster oBillsettlementmaster)
        {
            //if (LicenseManager.Instance(Context.OrganizationID).GetLicense().valiate_upto)
            //{
            //    if (LicenseManager.Instance(Context.OrganizationID).GetLicense().validupto < DateTimeNow)
            //    {
            //        throw new Exception("Your license expired, please contact finance team,store soft solution pvt ltd.");
            //    }
            //}
            int customercount = oBillsettlementmaster.Billsettlement.Count(e => e.customerid > 0);
            if (oBillsettlementmaster.customerid == 0 && oBillsettlementmaster.Billsettlement.Exists(e => e.customerid > 0))
            {
                oBillsettlementmaster.customerid = oBillsettlementmaster.Billsettlement.Find(e => e.customerid > 0).customerid;
            }
            if (oBillsettlementmaster.customerid == 0 && oBillsettlementmaster.paymentinfo.creditnote_refund > 0)
            {
                throw new Exception("Customer Registration is must for Credit Note Refund.");
            }
            if (!oBillsettlementmaster.IsSynchronize)
            {

                oBillsettlementmaster.companyid = oBillsettlementmaster.Billsettlement.First().companyid;
                if (!oBillsettlementmaster.paymentinfo.autosettlement)
                {
                    if (oBillsettlementmaster.paymentinfo.cash_paid == 0 &&
                        oBillsettlementmaster.paymentinfo.card_paid == 0 &&
                        oBillsettlementmaster.paymentinfo.upi_paid == 0 &&
                        oBillsettlementmaster.paymentinfo.credit_paid == 0 &&
                        oBillsettlementmaster.paymentinfo.cash_return == 0 &&
                        oBillsettlementmaster.paymentinfo.loan_paid == 0 &&
                        oBillsettlementmaster.billamount == 0 && oBillsettlementmaster.receivable == 0 && oBillsettlementmaster.Billsettlement.Count <= 1)
                    {
                        if (oBillsettlementmaster.Billsettlement.Count == 0 || (oBillsettlementmaster.Billsettlement.Count == 1
                            && oBillsettlementmaster.Billsettlement[0].receivable > 0))
                        {
                            WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(new Exception("Zero value settlement not allowed.")
                                , WebAPI.Common.Logger.LogType.Error, oBillsettlementmaster);
                            return oBillsettlementmaster;
                        }
                    }
                    if (oBillsettlementmaster.paymentinfo.card_paid == 0 &&
                        oBillsettlementmaster.paymentinfo.credit_paid == 0 &&
                        oBillsettlementmaster.paymentinfo.upi_paid == 0 &&
                        oBillsettlementmaster.paymentinfo.discount_paid == 0 &&
                        oBillsettlementmaster.paymentinfo.adjustment_paid == 0 &&
                        oBillsettlementmaster.paymentinfo.loan_paid == 0 &&
                        oBillsettlementmaster.paymentinfo.voucher_paid == 0 &&
                        oBillsettlementmaster.billamount > 0 &&
                        oBillsettlementmaster.receivable > 0 &&
                        oBillsettlementmaster.paymentinfo.cash_paid == oBillsettlementmaster.paymentinfo.cash_return)
                    {
                        WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(new Exception("Cash Paid and Cash Return can't be the same amount")
                            , WebAPI.Common.Logger.LogType.Error, oBillsettlementmaster);
                        return oBillsettlementmaster;
                    }

                    if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig != null &&
                        InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig.ContainsKey(oBillsettlementmaster.locationid) &&
                        InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[oBillsettlementmaster.locationid].settlement_disableadjustment &&
                        oBillsettlementmaster.paymentinfo.adjustment_paid > 0)
                    {
                        WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(new Exception("Settlement Adjustment not allowed.")
                            , WebAPI.Common.Logger.LogType.Error, oBillsettlementmaster);
                        return oBillsettlementmaster;

                    }

                    if (oBillsettlementmaster.paymentinfo.card_paid > 0 &&
                        oBillsettlementmaster.paymentinfo.cash_paid == 0 &&
                        oBillsettlementmaster.paymentinfo.cash_return != 0)
                    {
                        WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(new Exception("Cash Return can't be applicable for card payments")
                            , WebAPI.Common.Logger.LogType.Error, oBillsettlementmaster);
                        return oBillsettlementmaster;
                    }
                    if (oBillsettlementmaster.paymentinfo.upi_paid > 0 &&
                            oBillsettlementmaster.paymentinfo.upiproviderid <= 0)
                    {
                        WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(new Exception("Please select UPI Provider")
                            , WebAPI.Common.Logger.LogType.Error, oBillsettlementmaster);
                        return oBillsettlementmaster;
                    }
                    if (oBillsettlementmaster.paymentinfo.upi_paid > 0 &&
                            oBillsettlementmaster.paymentinfo.cash_paid == 0 &&
                            oBillsettlementmaster.paymentinfo.cash_return != 0)
                    {
                        WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(new Exception("UPI Return can't be applicable for card payments")
                            , WebAPI.Common.Logger.LogType.Error, oBillsettlementmaster);
                        return oBillsettlementmaster;
                    }
                    if (string.IsNullOrEmpty(oBillsettlementmaster.paymentinfo.mode)
                        && oBillsettlementmaster.paymentinfo.cash_paid < 0
                        && oBillsettlementmaster.billamount > 0)
                    {
                        WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(new Exception("Payment mode is Empty but Cash Paid is : " + oBillsettlementmaster.paymentinfo.cash_paid)
                            , WebAPI.Common.Logger.LogType.Error, oBillsettlementmaster);
                        return oBillsettlementmaster;
                    }

                    if (oBillsettlementmaster.paymentinfo.card_paid > 0 &&
                        oBillsettlementmaster.paymentinfo.voucher_paid > 0 &&
                        oBillsettlementmaster.billamount > 0 &&
                        (oBillsettlementmaster.paymentinfo.card_paid + oBillsettlementmaster.paymentinfo.voucher_paid) > oBillsettlementmaster.receivable)
                    {
                        WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(new Exception("Card Payment and Voucher Payment can't be greater than bill amount")
                            , WebAPI.Common.Logger.LogType.Error, oBillsettlementmaster);
                        return oBillsettlementmaster;
                    }

                }

                if (oBillsettlementmaster.paidamount > oBillsettlementmaster.billamount)
                    oBillsettlementmaster.paidamount = oBillsettlementmaster.billamount;

                if (oBillsettlementmaster.rewardunit == 0 && InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig != null &&
                       InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig.ContainsKey(oBillsettlementmaster.locationid))
                    oBillsettlementmaster.rewardunit = InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[oBillsettlementmaster.locationid].sale_rewardpoint;
            }

            var sequence = SequenceManager.Instance(Context.OrganizationID).Get("billsettlementmaster.code",
                oBillsettlementmaster.locationid, 0,
                InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_salefloorwisebillno ? oBillsettlementmaster.floorid : 0);
            {

                var settlementfreegoods = Services.PromotionManager.Instance(Context.OrganizationID).ProcessSettlementFreeProduct(transaction, conn, oBillsettlementmaster);


                int schemerewardpoints = 0;
                if (!oBillsettlementmaster.IsSynchronize && InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig != null &&
                InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig.ContainsKey(oBillsettlementmaster.locationid) &&
                InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[oBillsettlementmaster.locationid].settlement_coupen)
                {
                    var benefitList = Services.PromotionManager.Instance(Context.OrganizationID).ProcessSettlement(transaction, conn, oBillsettlementmaster);
                    if (benefitList != null && benefitList.Count > 0)
                    {
                        if (benefitList.Exists(e => e.benefitmode == Promotions.Benefit.BenefitMode.Coupen))
                        {
                            bool skipforadditionaldiscount = false;
                            if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig != null &&
                                            InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig.ContainsKey(oBillsettlementmaster.locationid) &&
                                            InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[oBillsettlementmaster.locationid].system_disablecoupondisonaddlndisitems)
                            {
                                skipforadditionaldiscount = oBillsettlementmaster.additionaldiscount != 0 ||
                                    oBillsettlementmaster.billadditionaldiscount != 0;
                            }

                            var excludecoponbills =
                                    (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig != null &&
                                            InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig.ContainsKey(oBillsettlementmaster.locationid) &&
                                            InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[oBillsettlementmaster.locationid].system_disablecoupondisoncouponbill) || 
                                    benefitList.FindAll(e => e.benefitmode == Promotions.Benefit.BenefitMode.Coupen).Any(x => x.ex_coupen);
                            if(excludecoponbills)
                            {
                                
                                decimal coupon = 0;
                                string querycoupon = string.Format(@"select sum(cast(billattributes->>'couponamount' as numeric)) from bill
                                                       where id in ({0})", string.Join(",", oBillsettlementmaster.Billsettlement.ConvertAll(y => y.billid)));
                                using (var reader = ExecuteReader(querycoupon, null,conn,transaction))
                                {
                                    if (reader.Read())
                                    {
                                        coupon = reader.IsDBNull(0) ? 0 : reader.GetDecimal(0);
                                    }
                                }
                                excludecoponbills = coupon > 0;
                            }
                            if (!skipforadditionaldiscount && !excludecoponbills)
                            {
                                CustomerCouponsDataService voucherdata = new CustomerCouponsDataService() { Context = this.Context };
                                voucherdata.Context = this.Context;
                                oBillsettlementmaster.paymentinfo.coupons = new List<Billsettlementmaster.voucherinfo>();
                                foreach (var coupon in benefitList.FindAll(e => e.benefitmode == Promotions.Benefit.BenefitMode.Coupen))
                                {
                                    if (coupon.otp && oBillsettlementmaster.customerid == 0) 
                                        continue;

                                    var availamt = coupon.value;
                                    while (availamt > 0 && availamt >= coupon.unit)
                                    {
                                        var voucher = new CustomerCoupons()
                                        {
                                            issuedby = Context.UserID,
                                            issueto = oBillsettlementmaster.customerid,
                                            amount = coupon.unit > 1 ? coupon.unit : coupon.value,
                                            isactive = true,
                                            vouchertype = CustomerCoupons.CouponType.Coupon,
                                            issuedon = DateTimeNow,
                                            validupto = coupon.validity,
                                            minbillvalue = coupon.minbillvalue,
                                            acceptablelocationid = coupon.locationid,
                                            locationid = oBillsettlementmaster.locationid,
                                            counterid = oBillsettlementmaster.counterid,
                                            unlimited = coupon.unlimited,
                                            maxbilldiscountpercentage = coupon.maxdiscountpercentage,
                                            filterschemeid = coupon.filterschemeid,
                                            amountaspercentage = coupon.inpercentage,
                                            otpcustomerid = oBillsettlementmaster.customerid,
                                            otp = coupon.otp,
                                            ex_sdiscount = coupon.ex_scheme,
                                            ex_coupon = coupon.ex_coupen
                                        };

                                        if (coupon.isranged)
                                        {
                                            voucher.isranged = true;
                                            voucher.rangedrule = new List<CustomerCoupons.RangeRule>();
                                            foreach (var benefit in coupon.rangebenefit)
                                            {
                                                voucher.rangedrule.Add(new CustomerCoupons.RangeRule()
                                                {
                                                    fromamount = benefit.pricefrom,
                                                    toamount = benefit.priceto,
                                                    amount = benefit.value
                                                });
                                            }
                                        }
                                        if (coupon.validitydays > 0)
                                        {
                                            if (coupon.validitytomorrow)
                                            {
                                                voucher.validfrom = DateTime.Now.AddDays(1).Date;
                                                voucher.validupto = voucher.validfrom.AddDays(coupon.validitydays);
                                            }
                                            else
                                            {
                                                voucher.validfrom = coupon.validity;
                                                voucher.validupto = coupon.validity.AddDays(coupon.validitydays);
                                            }
                                        }
                                        availamt -= voucher.amount;
                                        voucherdata.Insert(transaction, conn, voucher);
                                        oBillsettlementmaster.paymentinfo.coupons.Add(new Billsettlementmaster.voucherinfo()
                                        {
                                            amount = voucher.amount,
                                            id = voucher.id,
                                            code = voucher.code,
                                            schemename = coupon.schemename,
                                            locationid = coupon.locationid,
                                            validupto = voucher.validupto,
                                            minbillvalue = coupon.minbillvalue,
                                            maxbilldiscountpercentage = coupon.maxdiscountpercentage,
                                            amountaspercentage = coupon.inpercentage
                                        });
                                    }
                                    oBillsettlementmaster.paymentinfo.couponvalue = oBillsettlementmaster.paymentinfo.coupons.Sum(e => e.amount);
                                }
                            }
                        }

                        if (benefitList.Exists(e => e.benefitmode == Promotions.Benefit.BenefitMode.RewardPoints))
                        {
                            schemerewardpoints = (int)benefitList.Sum(e => e.benefitmode == Promotions.Benefit.BenefitMode.RewardPoints ?
                            oBillsettlementmaster.receivable / e.unit * e.value : 0);
                        }
                    }
                }

                if (oBillsettlementmaster.customerid > 0)
                {

                    if (!oBillsettlementmaster.IsSynchronize && InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig != null &&
                InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig.ContainsKey(oBillsettlementmaster.locationid) &&
                InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[oBillsettlementmaster.locationid] != null &&
                InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[oBillsettlementmaster.locationid].settlement_reward &&
                !InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[oBillsettlementmaster.locationid].sales_reward)
                    {
                        if (customercount <= 1)
                        {
                            //CustomerDataService custdata = new CustomerDataService() { Context = this.Context };
                            //var customerlist = custdata.Search(new CustomerCreteria()
                            //{
                            //    id = oBillsettlementmaster.customerid
                            //});
                            bool disableloyalty = false;
                            string loyeltyno = null;
                            var param = new SortedList<string, object>();
                            param.Add("id", oBillsettlementmaster.customerid);
                            using (var reader = ExecuteReader(@"select disableloyalty, loyeltyno from customer where isactive=true and id=@id", param, conn, transaction))
                            {
                                while (reader.Read())
                                {
                                    disableloyalty = reader["disableloyalty"] == DBNull.Value ? false : Convert.ToBoolean(reader["disableloyalty"].ToString());
                                    loyeltyno = reader["loyeltyno"] == DBNull.Value ? "" : reader["loyeltyno"].ToString();
                                }
                            }
                            if (!InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[oBillsettlementmaster.locationid].sale_canclaimloyaltyholderonly
                                || !string.IsNullOrEmpty(loyeltyno) && !disableloyalty)
                            {
                                CustomerrewardsDataService customerrewardsDataService = new CustomerrewardsDataService() { Context = this.Context };
                                customerrewardsDataService.Context = this.Context;
                                customerrewardsDataService.ProcessRewards(schemerewardpoints, transaction, conn, oBillsettlementmaster);
                            }
                        }
                        else
                        {
                            var newsettlementmasterobject = Newtonsoft.Json.JsonConvert.DeserializeObject<Billsettlementmaster>(Newtonsoft.Json.JsonConvert.SerializeObject(oBillsettlementmaster));
                            var customerids = oBillsettlementmaster.Billsettlement.ConvertAll(e => e.customerid).Distinct().ToList();
                            foreach (var _custid in customerids)
                            {
                                newsettlementmasterobject.customerid = _custid;
                                newsettlementmasterobject.Billsettlement = new List<Billsettlement>();
                                foreach (var _item in oBillsettlementmaster.Billsettlement.FindAll(e => e.customerid == _custid).ToList())
                                {
                                    newsettlementmasterobject.Billsettlement.Add(Newtonsoft.Json.JsonConvert.DeserializeObject<Billsettlement>(Newtonsoft.Json.JsonConvert.SerializeObject(_item)));
                                }

                                newsettlementmasterobject.receivable = newsettlementmasterobject.Billsettlement.Sum(e => e.receivable);

                                //CustomerDataService custdata = new CustomerDataService() { Context = this.Context };
                                //var customerlist = custdata.Search(new CustomerCreteria()
                                //{
                                //    id = _custid
                                //});
                                bool disableloyalty = false;
                                string loyeltyno = null;
                                var param = new SortedList<string, object>();
                                param.Add("id", _custid);
                                using (var reader = ExecuteReader(@"select disableloyalty, loyeltyno from customer where isactive=true and id=@id", param, conn, transaction))
                                {
                                    while (reader.Read())
                                    {
                                        disableloyalty = reader["disableloyalty"] == DBNull.Value ? false : Convert.ToBoolean(reader["disableloyalty"].ToString());
                                        loyeltyno = reader["loyeltyno"] == DBNull.Value ? "" : reader["loyeltyno"].ToString();
                                    }
                                }

                                if (!InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[oBillsettlementmaster.locationid].sale_canclaimloyaltyholderonly
                                    || !string.IsNullOrEmpty(loyeltyno) && !disableloyalty)
                                {
                                    CustomerrewardsDataService customerrewardsDataService = new CustomerrewardsDataService() { Context = this.Context };
                                    customerrewardsDataService.Context = this.Context;
                                    customerrewardsDataService.ProcessRewards(schemerewardpoints, transaction, conn, newsettlementmasterobject);
                                }

                            }
                        }
                    }
                }

                if (!oBillsettlementmaster.IsSynchronize && oBillsettlementmaster.paymentinfo.HasDiscount && InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig != null &&
                    Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig.ContainsKey(oBillsettlementmaster.locationid) &&
                    Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[oBillsettlementmaster.locationid].settlement_discountbill)
                {
                    DiscountOnSettlement(transaction, conn, oBillsettlementmaster);
                }


                if (!oBillsettlementmaster.IsSynchronize)
                {
                    oBillsettlementmaster.code = sequence.GetNextNumber(Context, Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[oBillsettlementmaster.locationid].settlement_nodailyreset, oBillsettlementmaster.settlementon.Date, conn, transaction);
                    if (Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig.ContainsKey(oBillsettlementmaster.locationid) &&
                        Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[oBillsettlementmaster.locationid].settlement_nodailyreset
                        && oBillsettlementmaster.settlementon.Date != sequence.modifiedon.Date)
                    {
                        sequence.ResetNumber(conn, transaction, oBillsettlementmaster.settlementon.Date);
                        oBillsettlementmaster.code = sequence.GetNextNumber(Context, Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[oBillsettlementmaster.locationid].settlement_nodailyreset, oBillsettlementmaster.settlementon.Date, conn, transaction);
                    }

                    int loopcount = 0;
                    while (true)
                    {
                        if (LicenseManager.Instance(Context.OrganizationID).GetLicense().isoffline &&
    !string.IsNullOrEmpty(Services.ConfigManager.Instance(Context.OrganizationID).Config.system_offlineprefix))
                            oBillsettlementmaster.code = Services.ConfigManager.Instance(Context.OrganizationID).Config.system_offlineprefix + oBillsettlementmaster.code;
                        else if (!LicenseManager.Instance(Context.OrganizationID).GetLicense().isoffline && !string.IsNullOrEmpty(Services.ConfigManager.Instance(Context.OrganizationID).Config.system_onlineprefix))
                            oBillsettlementmaster.code = Services.ConfigManager.Instance(Context.OrganizationID).Config.system_onlineprefix + oBillsettlementmaster.code;

                        var param = new SortedList<string, object>();
                        param.Add("code", oBillsettlementmaster.code);
                        using (var reader = ExecuteReader("Select id from Billsettlementmaster where code=@code", param))
                        {
                            if (!reader.Read())
                            {
                                break;
                            }
                            oBillsettlementmaster.code = sequence.GetNextNumber(Context, conn, transaction);
                        }
                        if (loopcount > 100) throw new Exception(string.Format("Duplicate Bill settlement Master Code found. Please verify number configuration. Last Code is {0}", oBillsettlementmaster.code));
                        loopcount++;
                    }
                }

                if (!oBillsettlementmaster.IsSynchronize && oBillsettlementmaster.paymentinfo.voucher_paid > 0)
                {
                    if (oBillsettlementmaster.paymentinfo.voucherinformation != null &&
                        Services.ConfigManager.Instance(Context.OrganizationID).Config.system_supportcustcreditnote &&
                        oBillsettlementmaster.paymentinfo.voucherinformation.Sum(e => e.available > e.amount ? e.available - e.amount : 0) > 0)
                    {
                        if (oBillsettlementmaster.customerid == 0)
                        {
                            throw new Exception("Customer Registration is must for Credit Note Refund.");
                        }

                        foreach (var item in oBillsettlementmaster.paymentinfo.voucherinformation)
                        {
                            oBillsettlementmaster.paymentinfo.creditnote_refund += (item.available - item.amount);
                            item.amount = item.available;
                        }
                        // To Restrict new credit note on the balance credit value while using first credit note
                        if (oBillsettlementmaster.paymentinfo.creditnote_refund > 0 &&
                            Services.ConfigManager.Instance(Context.OrganizationID).Config.system_custcreditnoteabove >= oBillsettlementmaster.paymentinfo.creditnote_refund)
                        {
                            oBillsettlementmaster.paymentinfo.cash_return = oBillsettlementmaster.paymentinfo.creditnote_refund;
                            oBillsettlementmaster.paymentinfo.cash_refund = oBillsettlementmaster.paymentinfo.creditnote_refund;
                            oBillsettlementmaster.paymentinfo.creditnote_refund = 0;
                        }
                    }
                }

                Insert(transaction, conn, oBillsettlementmaster);
                BillsettlementDataService subBillsettlement = new BillsettlementDataService() { Context = this.Context };
                subBillsettlement.Context = this.Context;
                decimal available = oBillsettlementmaster.paidamount;
                foreach (var item in oBillsettlementmaster.Billsettlement)
                {
                    item.billsettlementmasterid = oBillsettlementmaster.id;
                    if (!oBillsettlementmaster.IsSynchronize)
                    {
                        if (available > item.receivable)
                        {
                            item.paid = item.receivable;
                            available -= item.paid;
                        }
                        else
                        {
                            item.paid = available;
                            available -= item.paid;
                        }
                    }
                    subBillsettlement.Insert(transaction, conn, item);
                }

                // For Credit Note issue.
                if (!oBillsettlementmaster.IsSynchronize && oBillsettlementmaster.customerid > 0
                    && Services.ConfigManager.Instance(Context.OrganizationID).Config.system_supportcustcreditnote
                    && oBillsettlementmaster.paymentinfo.creditnote_refund > 0)
                {
                    GiftCouponDataService CouponData = new GiftCouponDataService();
                    CouponData.Context = this.Context;

                    GiftVoucher gtvoucher = new GiftVoucher()
                    {
                        isactive = true,
                        createdby = Context.UserID,
                        createdon = DateTimeNow,
                        version = 1,
                        validfrom = DateTimeNow,
                        validto = DateTimeNow.AddDays(90),
                        customerid = oBillsettlementmaster.customerid,
                        counterid = oBillsettlementmaster.counterid,
                        locationid = oBillsettlementmaster.locationid,
                        billsettlementmasterid = oBillsettlementmaster.id,
                        name = "CREDIT NOTE",
                        issuemode = 4,
                        mobileno = oBillsettlementmaster.customermobileno,
                        receivedamt = oBillsettlementmaster.paymentinfo.creditnote_refund,
                        vouchervalue = oBillsettlementmaster.paymentinfo.creditnote_refund,
                        issuedby = Context.EmployeeID,
                        denomination = new List<GiftVoucher.Denomination>()
                    };
                    // Settlement Master ID is the reference in Coupons table for which Credit note Issued.
                    CouponData.Insert(transaction, conn, gtvoucher);

                    // Vouchers table has only Voucher code - voucher type = 7
                    CustomerCouponsDataService voucherservice = new CustomerCouponsDataService() { Context = this.Context };
                    voucherservice.Context = this.Context;
                    gtvoucher.vouchercode = DateTimeNow.Year.ToString().Substring(2, 2) + WebAPI.Data.Token.RandomString(14, "0123456789");
                    voucherservice.Insert(transaction, conn, new CustomerCoupons()
                    {
                        code = gtvoucher.vouchercode,
                        issuedby = gtvoucher.issuedby,
                        issueto = gtvoucher.receivedby,
                        amount = gtvoucher.vouchervalue,
                        isactive = true,
                        issuedon = oBillsettlementmaster.settlementon,
                        validfrom = gtvoucher.validfrom,
                        coupenid = gtvoucher.id,
                        validupto = gtvoucher.validto,
                        vouchertype = CustomerCoupons.CouponType.CreditNote
                    });

                    Billsettlementmaster.voucherinfo creditnote = new Billsettlementmaster.voucherinfo()
                    {
                        code = gtvoucher.vouchercode,
                        amount = gtvoucher.vouchervalue,
                        validupto = gtvoucher.validto,
                        type = 7
                    };
                    if (oBillsettlementmaster.paymentinfo.voucherinformation == null)
                        oBillsettlementmaster.paymentinfo.voucherinformation = new List<Billsettlementmaster.voucherinfo>();
                    oBillsettlementmaster.paymentinfo.voucherinformation.Add(creditnote);
                }

                if (!oBillsettlementmaster.IsSynchronize && oBillsettlementmaster.paymentinfo.voucher_paid > 0)
                {
                    CustomerCouponsDataService voucherdata = new CustomerCouponsDataService() { Context = this.Context };
                    voucherdata.Context = this.Context;
                    using (var reader = ExecuteReader("select code,coupenid from vouchers where code in (" + string.Join(",",
                        oBillsettlementmaster.paymentinfo.voucherinformation.ConvertAll(e => "'" + WebAPI.Data.Utility.SafeSQL(e.code) + "'")) + " )",
                        new SortedList<string, object>(),
                        conn, transaction))
                    {
                        while (reader.Read())
                        {
                            string vouchercode = reader.GetString(0);
                            long couponid = reader.GetInt64(1);
                            if (oBillsettlementmaster.paymentinfo.voucherinformation.Exists(e => e.code == vouchercode && e.type == 0))
                                oBillsettlementmaster.paymentinfo.voucherinformation.Find(e => e.code == vouchercode && e.type == 0).id = couponid;
                        }
                    }

                    // For Lead Management
                    bool _leadmanagement = false;
                    if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID) != null &&
                            InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[oBillsettlementmaster.locationid] != null)
                        _leadmanagement = InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[oBillsettlementmaster.locationid].system_enableleadmgmt;

                    LeadmanagementDataService leadmgmtDataService = new LeadmanagementDataService() { Context = this.Context };
                    leadmgmtDataService.Context = this.Context;

                    // For Voucher Claims - Issueto column has the BillSettlementMaster Id for which the Voucher has claimed.
                    // BillId in Vouchers table has the billid for which the its has claimed.
                    // Voucher type = 4
                    foreach (var item in oBillsettlementmaster.paymentinfo.voucherinformation.FindAll(e => e.type == 0))
                    {
                        voucherdata.Insert(transaction, conn, new CustomerCoupons()
                        {
                            code = item.code,
                            amount = item.amount * -1,
                            issueto = oBillsettlementmaster.id,
                            isactive = true,
                            coupenid = item.id,
                            issuedon = oBillsettlementmaster.settlementon,
                            billid = oBillsettlementmaster.Billsettlement.First().billid,
                            vouchertype = CustomerCoupons.CouponType.GiftClaim
                        });

                        if (_leadmanagement)
                        {
                            leadmgmtDataService.UpdateLeadStatus(transaction, conn, item.code, oBillsettlementmaster.id, false);
                        }
                    }
                }

                CustomerinvoiceDataService customerdata = new CustomerinvoiceDataService() { Context = this.Context };
                customerdata.Context = this.Context;

                if (!oBillsettlementmaster.IsSynchronize && oBillsettlementmaster.paymentinfo.credit_paid != 0)
                {
                    var crpaid = oBillsettlementmaster.paymentinfo.credit_paid;
                    foreach (var item in oBillsettlementmaster.Billsettlement)
                    {
                        if (item.mode != 0 && item.customerid > 0)
                        {
                            var receivable = item.receivable;
                            if (crpaid >= receivable)
                            {
                                crpaid -= receivable;
                            }
                            else
                            {
                                receivable = crpaid;
                                crpaid = 0;
                            }
                            customerdata.Insert(transaction, conn, new Customerinvoice
                            {
                                isactive = true,
                                createdby = Context.UserID,
                                createdon = DateTimeNow,
                                version = 1,
                                type = 1,
                                receivable = receivable,
                                transactiondate = item.receiptdate,
                                billid = item.billid,
                                customerid = item.customerid,
                                billno = item.billno,
                                companyid = item.companyid
                            });
                        }
                    }
                }

                if (!oBillsettlementmaster.IsSynchronize && oBillsettlementmaster.paymentinfo.loan_paid > 0 &&
                    oBillsettlementmaster.paymentinfo.loanproviderid > 0 &&
                    oBillsettlementmaster.customerid > 0)
                {
                    LoansettlementDataService loansettlementDataService = new LoansettlementDataService() { Context = this.Context };
                    loansettlementDataService.Context = this.Context;
                    var amountpercentage = oBillsettlementmaster.paymentinfo.loan_paid / oBillsettlementmaster.receivable * 100;
                    List<Loansettlement> loansettlements = new List<Loansettlement>();
                    var availableloan = oBillsettlementmaster.paymentinfo.loan_paid;
                    for (int i = 0; i < oBillsettlementmaster.Billsettlement.Count; i++)
                    {
                        var item = oBillsettlementmaster.Billsettlement[i];
                        var loanamt = i == oBillsettlementmaster.Billsettlement.Count - 1 ? availableloan : Math.Round(item.receivable * amountpercentage / (decimal)100, 0);
                        availableloan -= loanamt;
                        loansettlements.Add(new Loansettlement
                        {
                            billsettlementmasterid = oBillsettlementmaster.id,
                            companyid = oBillsettlementmaster.companyid,
                            locationid = oBillsettlementmaster.locationid,
                            loanamount = loanamt,
                            loanproviderid = oBillsettlementmaster.paymentinfo.loanproviderid,
                            referenceno = oBillsettlementmaster.paymentinfo.loan_reference,
                            modifiedby = Context.UserID,
                            modifiedon = DateTimeNow,
                            isactive = true,
                            createdby = Context.UserID,
                            createdon = DateTimeNow,
                            version = 1,
                            billdate = item.billdate,
                            billid = item.billid,
                            customerid = item.customerid,
                            billno = item.billno
                        });
                    }
                    loansettlementDataService.Update(transaction, conn, loansettlements, oBillsettlementmaster.id);
                }

                if (!oBillsettlementmaster.IsSynchronize &&
                    !string.IsNullOrEmpty(oBillsettlementmaster.paymentinfo.referral))
                {
                    StoretransactionDataService storetransactionDataService = new StoretransactionDataService() { Context = this.Context };
                    storetransactionDataService.UpdateReferral(oBillsettlementmaster, conn, transaction);
                }

            }

            if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_billlog)
            {
                BillLog.Instance(this.Context.OrganizationID).Log(oBillsettlementmaster, transaction, conn);
            }

            if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.intray_notification_billsettlement)
            {
                if (oBillsettlementmaster.customerid > 0)
                {
                    string query = string.Format(@"select sum(cast(billattributes->>'rewardamount' as numeric)) spentamount,
                    sum(cast(billattributes->>'rewardpoint' as numeric)) spentreward,
                    sum(cast(billattributes->>'earnedreward' as numeric)) earnedreward
                    from bill
                    where id in ({0}) and bill.isactive=true and bill.billtype != 9 and customerid={1}",
                    string.Join(",", oBillsettlementmaster.Billsettlement.ConvertAll(x => x.billid)), oBillsettlementmaster.customerid);
                    using (var reader = ExecuteReader(query, null))
                    {
                        if(reader.Read())
                        {
                            oBillsettlementmaster.paymentinfo.reward_amount = reader["spentamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["spentamount"]);
                            oBillsettlementmaster.paymentinfo.reward_spent = reader["spentreward"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["spentreward"]);
                            oBillsettlementmaster.paymentinfo.reward_earned = reader["earnedreward"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["earnedreward"]);
                        }
                    }
                    Processors.Schedule_Actions.Instance.OnChange(Context.OrganizationID, oBillsettlementmaster, Processors.Schedule_Actions.Actions.Settlement);
                }
            }
            return oBillsettlementmaster;
        }


        internal bool UpdateSettlement(BillInfo item)
        {
            if (item.billsettlementmasterid <= 0)
            {
                throw new Exception("Only silent settled bill can be modified");
            }
            var settlementmaster = Search(new BillsettlementmasterCreteria()
            {
                id = item.billsettlementmasterid
            }).First();

            settlementmaster.casherid = this.Context.EmployeeID;
            settlementmaster.cashiername = this.Context.EmployeeName;
            settlementmaster.counterid = item.settlement.counterid;
            settlementmaster.countername = item.settlement.countername;
            settlementmaster.creditamount = item.settlement.creditamount;
            settlementmaster.floorid = item.settlement.floorid;
            settlementmaster.floorname = item.settlement.floorname;
            settlementmaster.loanprovider = item.settlement.loanprovider;
            settlementmaster.loanproviderid = item.settlement.loanproviderid;
            settlementmaster.loan_paid = item.settlement.loan_paid;
            settlementmaster.loan_reference = item.settlement.loan_reference;
            settlementmaster.locationid = item.settlement.locationid;
            settlementmaster.mode = item.settlement.mode;
            settlementmaster.paidamount = item.settlement.paidamount;
            settlementmaster.paid_discount = item.settlement.paid_discount;
            settlementmaster.paid_refund = item.settlement.paid_refund;
            settlementmaster.reason = item.settlement.reason;
            settlementmaster.returnbillvalue = item.settlement.returnbillvalue;
            settlementmaster.swipbankid = item.settlement.swipbankid;
            settlementmaster.swipingmachineid = item.settlement.swipingmachineid;
            settlementmaster.swipmachineid = item.settlement.swipmachineid;
            settlementmaster.paymentinfo = item.settlement.paymentinfo;

            Update(settlementmaster);
            return true;
        }

        public void DiscountOnSettlement(NpgsqlTransaction transaction, NpgsqlConnection conn, Billsettlementmaster oBillsettlementmaster)
        {
            Data.BillDataService billdataservice = new BillDataService() { Context = this.Context };
            Data.TaxDataService taxDataService = new TaxDataService() { Context = this.Context };
            oBillsettlementmaster.discountedbills = new List<Billsettlementmaster.BillInfo>();
            var taxsplits = taxDataService.RangedTaxProducts(string.Join<long>(",", oBillsettlementmaster.Billsettlement.ConvertAll<long>(e => e.billid)));
            var totaldiscount = oBillsettlementmaster.paymentinfo.discount_paid;
            var billdiscountavailable = totaldiscount;

            if (oBillsettlementmaster.Billsettlement.All(x => x.billamount < 0))
                throw new Exception("Discount on Settlement Can't be applicable Return bills");

            for (int j = 0; j < oBillsettlementmaster.Billsettlement.Count; j++)
            {
                var sitem = oBillsettlementmaster.Billsettlement[j];
                var bills = billdataservice.Search(new BillCreteria()
                {
                    id = sitem.billid
                });

                if (bills[0].Billitems.All(x => x.receivable < 0)) continue;

                var billdiscountpercent = sitem.billamount / oBillsettlementmaster.billamount * 100;
                if (billdiscountpercent > 100) billdiscountpercent = 100;
                var billdiscount = Math.Round((totaldiscount * billdiscountpercent / 100), 2);

                if (Math.Round(billdiscount, 0) > totaldiscount)
                    throw new Exception("Discount on Settlement Can't be applicable On Return Bills, Please Use POS Sales.");

                if (j == oBillsettlementmaster.Billsettlement.Count - 1)
                    billdiscount = billdiscountavailable;
                else
                    billdiscountavailable -= billdiscount;

                sitem.additionaldiscount += billdiscount;
                bills[0].additionaldiscount += billdiscount;
                //var billreceivable = bills[0].receivable;
                //var list = bills[0].Billitems.OrderByDescending(e => e.receivable).ToList();
                bills[0].receivable -= billdiscount;

                sitem.billamount -= billdiscount;
                sitem.receivable -= billdiscount;
                sitem.paid = sitem.receivable;
                sitem.discount = 0;

                var saleitems = bills[0].Billitems.FindAll(x => x.receivable > 0);
                var billreceivable = saleitems.Sum(x => x.receivable);
                decimal thisbilldiscount = billdiscount;
                for (int i = 0; i < saleitems.Count; i++)
                {
                    var itemdiscountpercent = saleitems[i].receivable / billreceivable * 100;
                    var itemdiscount = Math.Round((thisbilldiscount * itemdiscountpercent / 100), 2);
                    if (i == saleitems.Count - 1)
                    {
                        itemdiscount = billdiscount;
                    }
                    else
                    {
                        billdiscount -= itemdiscount;
                    }
                    saleitems[i].additionaldiscount += Math.Round(itemdiscount / saleitems[i].qty, 2);
                    saleitems[i].additionaldiscountpercentage += Math.Round(((saleitems[i].additionaldiscount * saleitems[i].qty) / (saleitems[i].receivable + saleitems[i].discount)) * 100, 2);
                    //saleitems[i].price -= Math.Round(itemdiscount / saleitems[i].qty, 2);
                    var newPrice = saleitems[i].price - saleitems[i].additionaldiscount - (saleitems[i].schemediscount / saleitems[i].qty);
                    if (taxsplits.ContainsKey(saleitems[i].barcode))
                    {
                        var newtax = taxsplits[saleitems[i].barcode].Find(newPrice);
                        if (newtax.taxpercent != saleitems[i].taxpercentage)
                        {
                            saleitems[i].taxpercentage = newtax.taxpercent;
                        }
                    }
                    saleitems[i].tax = Math.Round(saleitems[i].qty * (newPrice / (100 + saleitems[i].taxpercentage) * saleitems[i].taxpercentage), 4);
                    saleitems[i].itemtax = Math.Round(newPrice / (100 + saleitems[i].taxpercentage) * saleitems[i].taxpercentage, 4);
                    saleitems[i].rate = newPrice - saleitems[i].itemtax;
                    saleitems[i].receivable = Math.Round(newPrice * saleitems[i].qty, 2);
                }

                bills[0].tax = bills[0].Billitems.Sum(e => e.tax);
                bills[0].rounding = Math.Round(bills[0].receivable, MidpointRounding.AwayFromZero) - bills[0].receivable;
                bills[0].RecalculateTax();
                billdataservice.Update(transaction, conn, bills[0], true);
                oBillsettlementmaster.discountedbills.Add(new Billsettlementmaster.BillInfo()
                {
                    billid = bills[0].id,
                    billno = bills[0].billno,
                    source = bills[0]
                });
            }
            oBillsettlementmaster.paidamount -= oBillsettlementmaster.paymentinfo.discount_paid;
            oBillsettlementmaster.paymentinfo.discount_adjusted = oBillsettlementmaster.paymentinfo.discount_paid;
            oBillsettlementmaster.paymentinfo.discount_paid = 0;
            oBillsettlementmaster.billamount -= oBillsettlementmaster.paymentinfo.discount_adjusted;
        }

        public List<BillInfo> SearchLastSettlements()
        {
            try
            {
                var result = new List<BillInfo>();
                var param = new SortedList<string, object>();
                param.Add("createdon", DateTimeNow.Date);
                param.Add("createdby", Context.UserID);
                using (NpgsqlConnection connection = CreateConnection())
                {
                    connection.Open();
                    using (var reader = ExecuteReader(@"with lastbill
                    as
                    (
                        select id from billsettlementmaster where createdon>=@createdon and createdby=@createdby order by id desc limit 1
                    )
                    select lastbill.id,billsettlement.billno from lastbill
                    inner join billsettlement on billsettlement.billsettlementmasterid = lastbill.id", param, connection, null))
                    {
                        while (reader.Read())
                        {
                            result.Add(new BillInfo()
                            {
                                id = reader.GetInt64(0),
                                billno = reader.GetString(1)
                            });
                        }
                    }
                    connection.Close();
                }
                return result;
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error,
                    exp.StackTrace, "Search Last Bills");
            }
            return null;
        }
        public void Insert(Processors.OfflineClient.WindowsOfflineResponse response, List<Billsettlementmaster> bills)
        {
            NpgsqlTransaction transaction = null;
            string currentbillno = "";
            try
            {
                BillsettlementDataService subBillsettlement = new BillsettlementDataService() { Context = this.Context };
                subBillsettlement.Context = this.Context;
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    foreach (var bill in bills)
                    {
                        bill.IsSynchronize = true;
                        currentbillno = bill.code;
                        bool exist = false;
                        var param = new SortedList<string, object>();
                        param.Add("code", bill.code);
                        using (var reader = ExecuteReader("Select id from Billsettlementmaster where code=@code", param))
                        {
                            if (reader.Read())
                            {
                                if (string.IsNullOrWhiteSpace(response.errormessage))
                                    response.errormessage = "Already settled :";
                                response.errormessage += bill.code + " , ";
                                response.error = true;
                                exist = true;
                            }
                        }

                        if (!exist)
                        {
                            Insert(transaction, conn, bill);
                            decimal available = bill.paidamount;
                            foreach (var item in bill.Billsettlement)
                            {
                                item.billsettlementmasterid = bill.id;
                                subBillsettlement.Insert(transaction, conn, item);
                            }
                        }
                        if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_billlog)
                        {
                            BillLog.Instance(this.Context.OrganizationID).Log(bill, transaction, conn);
                        }
                    }
                    transaction.Commit();
                    conn.Close();
                }
                lock (response.lockobject)
                {
                    response.completed = true;
                    response.completedon = DateTimeNow;
                }
            }
            catch (Exception exp)
            {
                lock (response.lockobject)
                {
                    response.errormessage = currentbillno + " - " + exp.Message;
                    response.error = true;
                    response.completed = true;
                    response.completedon = DateTimeNow;
                }
            }
        }
        public Billsettlementmaster Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Billsettlementmaster oBillsettlementmaster)
        {
            decimal totalpaid = 0;
            totalpaid += oBillsettlementmaster.paymentinfo == null ? 0 : (oBillsettlementmaster.paymentinfo.cash_paid > 0 ? oBillsettlementmaster.paymentinfo.cash_paid - oBillsettlementmaster.paymentinfo.cash_return : oBillsettlementmaster.paymentinfo.cash_paid);
            totalpaid += oBillsettlementmaster.paymentinfo == null ? 0 : oBillsettlementmaster.paymentinfo.card_paid;
            totalpaid += oBillsettlementmaster.paymentinfo == null ? 0 : oBillsettlementmaster.paymentinfo.upi_paid;
            totalpaid += oBillsettlementmaster.paymentinfo == null ? 0 : oBillsettlementmaster.paymentinfo.credit_paid;
            totalpaid += oBillsettlementmaster.paymentinfo == null ? 0 : oBillsettlementmaster.paymentinfo.voucher_paid;
            totalpaid += oBillsettlementmaster.paymentinfo == null ? 0 : oBillsettlementmaster.paymentinfo.debt_paid;
            totalpaid += oBillsettlementmaster.paymentinfo == null ? 0 : oBillsettlementmaster.paymentinfo.cash_refund;
            totalpaid += oBillsettlementmaster.paymentinfo == null ? 0 : oBillsettlementmaster.paymentinfo.creditnote_refund;
            totalpaid += oBillsettlementmaster.paymentinfo == null ? 0 : oBillsettlementmaster.paymentinfo.discount_paid;
            if (Math.Abs(oBillsettlementmaster.receivable) > 1 && totalpaid == 0)
            {
                decimal adjustmentallowed = (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[oBillsettlementmaster.locationid].settlement_maxadjustment > 0 ?
                    InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[oBillsettlementmaster.locationid].settlement_maxadjustment : 5);
                if (Math.Abs(oBillsettlementmaster.receivable) > adjustmentallowed ||
                    Math.Abs(oBillsettlementmaster.receivable) != oBillsettlementmaster.paymentinfo.adjustment_paid)
                {
                    var exp = new Exception("Can't find the received mode of the payment");
                    WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oBillsettlementmaster);
                    throw exp;
                }
            }

            if (oBillsettlementmaster.paymentinfo != null && oBillsettlementmaster.paymentinfo.cash_paid > 0 && oBillsettlementmaster.paymentinfo.cash_return > 500)
            {
                throw new Exception("Please check the payment, do it again, refund maximum allowed 500");
            }


            if (oBillsettlementmaster.paymentinfo != null && oBillsettlementmaster.paymentinfo.cash_return > 100000)
            {
                throw new Exception("Please check the payment, do it again, refund maximum allowed 100000");
            }

            NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBillsettlementmaster.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillsettlementmaster.locationid;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBillsettlementmaster.version;
            command.Parameters.Add("tzoffset", NpgsqlTypes.NpgsqlDbType.Integer).Value = Context.UTCOffset;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("mode", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBillsettlementmaster.mode;
            command.Parameters.Add("billamount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillsettlementmaster.billamount;
            command.Parameters.Add("paymentinfo", NpgsqlTypes.NpgsqlDbType.Json).Value = oBillsettlementmaster.paymentinfo_json == null ? "null" : oBillsettlementmaster.paymentinfo_json;
            command.Parameters.Add("customerid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillsettlementmaster.Billsettlement.First().customerid;
            command.Parameters.Add("paidamount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillsettlementmaster.paidamount;
            command.Parameters.Add("code", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBillsettlementmaster.code == null ? "" : oBillsettlementmaster.code;
            command.Parameters.Add("settlementon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBillsettlementmaster.settlementon;
            command.Parameters.Add("discount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillsettlementmaster.discount;
            command.Parameters.Add("receivable", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillsettlementmaster.receivable;
            command.Parameters.Add("tax", NpgsqlTypes.NpgsqlDbType.Numeric).Value = 0;
            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillsettlementmaster.companyid;
            command.Parameters.Add("paid_cash", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillsettlementmaster.paymentinfo == null ? 0 : (oBillsettlementmaster.paymentinfo.cash_paid > 0 ? oBillsettlementmaster.paymentinfo.cash_paid - oBillsettlementmaster.paymentinfo.cash_return : 0);
            command.Parameters.Add("paid_card", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillsettlementmaster.paymentinfo == null ? 0 : oBillsettlementmaster.paymentinfo.card_paid;
            command.Parameters.Add("paid_upi", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillsettlementmaster.paymentinfo == null ? 0 : oBillsettlementmaster.paymentinfo.upi_paid;
            command.Parameters.Add("paid_credit", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillsettlementmaster.paymentinfo == null ? 0 : oBillsettlementmaster.paymentinfo.credit_paid;
            command.Parameters.Add("paid_voucher", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillsettlementmaster.paymentinfo == null ? 0 : oBillsettlementmaster.paymentinfo.voucher_paid;
            command.Parameters.Add("paid_debt", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillsettlementmaster.paymentinfo == null ? 0 : oBillsettlementmaster.paymentinfo.debt_paid;
            command.Parameters.Add("paid_refund", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillsettlementmaster.paymentinfo == null ? 0 : oBillsettlementmaster.paymentinfo.cash_refund;
            command.Parameters.Add("paid_discount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillsettlementmaster.paymentinfo == null ? 0 : oBillsettlementmaster.paymentinfo.discount_paid;

            command.Parameters.Add("billmastercode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBillsettlementmaster.billmastercode == null ? "" : oBillsettlementmaster.billmastercode;
            command.Parameters.Add("additionaldiscount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillsettlementmaster.additionaldiscount;
            command.Parameters.Add("billadditionaldiscount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillsettlementmaster.billadditionaldiscount;
            command.Parameters.Add("cashierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.EmployeeID;
            command.Parameters.Add("counterid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillsettlementmaster.counterid;

            command.Parameters.Add("loanproviderid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillsettlementmaster.paymentinfo.loanproviderid;
            command.Parameters.Add("swipbankid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillsettlementmaster.paymentinfo.card_bankid;
            command.Parameters.Add("swipmachineid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillsettlementmaster.paymentinfo.card_machineid;
            command.Parameters.Add("loan_paid", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillsettlementmaster.paymentinfo.loan_paid;
            command.Parameters.Add("loan_reference", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBillsettlementmaster.paymentinfo.loan_reference == null ? "" : oBillsettlementmaster.paymentinfo.loan_reference;

            command.Parameters.Add("intvendor", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBillsettlementmaster.intvendor;
            command.Parameters.Add("intmode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBillsettlementmaster.intmode == null ? "" : oBillsettlementmaster.intmode;

            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oBillsettlementmaster.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            return oBillsettlementmaster;
        }

        public Billsettlementmaster Update(Billsettlementmaster oBillsettlementmaster)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oBillsettlementmaster);
                    BillsettlementDataService subBillsettlement = new BillsettlementDataService() { Context = this.Context };
                    subBillsettlement.Context = this.Context;
                    subBillsettlement.Update(transaction, conn, oBillsettlementmaster.Billsettlement, oBillsettlementmaster.id);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oBillsettlementmaster);


            }

            return oBillsettlementmaster;
        }

        public Billsettlementmaster Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Billsettlementmaster oBillsettlementmaster)
        {

            NpgsqlCommand command = GetCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillsettlementmaster.id;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBillsettlementmaster.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillsettlementmaster.modifiedby;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillsettlementmaster.locationid;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBillsettlementmaster.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("mode", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBillsettlementmaster.mode;
            command.Parameters.Add("billamount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillsettlementmaster.billamount;
            command.Parameters.Add("paymentinfo", NpgsqlTypes.NpgsqlDbType.Json).Value = oBillsettlementmaster.paymentinfo_json == null ? "null" : oBillsettlementmaster.paymentinfo_json;
            command.Parameters.Add("customerid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillsettlementmaster.Billsettlement.First().customerid;
            command.Parameters.Add("paidamount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillsettlementmaster.paidamount;
            command.Parameters.Add("code", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBillsettlementmaster.code == null ? "" : oBillsettlementmaster.code;
            command.Parameters.Add("settlementon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBillsettlementmaster.settlementon;
            command.Parameters.Add("discount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillsettlementmaster.discount;
            command.Parameters.Add("receivable", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillsettlementmaster.receivable;
            command.Parameters.Add("tax", NpgsqlTypes.NpgsqlDbType.Numeric).Value = 0;
            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillsettlementmaster.companyid;
            command.Parameters.Add("paid_cash", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillsettlementmaster.paymentinfo == null ? 0 : (oBillsettlementmaster.paymentinfo.cash_paid > 0 ? oBillsettlementmaster.paymentinfo.cash_paid - oBillsettlementmaster.paymentinfo.cash_return : 0);
            command.Parameters.Add("paid_card", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillsettlementmaster.paymentinfo == null ? 0 : oBillsettlementmaster.paymentinfo.card_paid;
            command.Parameters.Add("paid_upi", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillsettlementmaster.paymentinfo == null ? 0 : oBillsettlementmaster.paymentinfo.upi_paid;
            command.Parameters.Add("paid_credit", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillsettlementmaster.paymentinfo == null ? 0 : oBillsettlementmaster.paymentinfo.credit_paid;
            command.Parameters.Add("paid_voucher", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillsettlementmaster.paymentinfo == null ? 0 : oBillsettlementmaster.paymentinfo.voucher_paid;
            command.Parameters.Add("paid_debt", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillsettlementmaster.paymentinfo == null ? 0 : oBillsettlementmaster.paymentinfo.debt_paid;
            command.Parameters.Add("paid_refund", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillsettlementmaster.paymentinfo == null ? 0 : oBillsettlementmaster.paymentinfo.cash_refund;
            command.Parameters.Add("paid_discount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillsettlementmaster.paymentinfo == null ? 0 : oBillsettlementmaster.paymentinfo.discount_paid;

            command.Parameters.Add("billmastercode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBillsettlementmaster.billmastercode == null ? "" : oBillsettlementmaster.billmastercode;
            command.Parameters.Add("additionaldiscount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillsettlementmaster.additionaldiscount;
            command.Parameters.Add("billadditionaldiscount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillsettlementmaster.billadditionaldiscount;
            command.Parameters.Add("cashierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillsettlementmaster.casherid > 0 ? oBillsettlementmaster.casherid : Context.EmployeeID;
            command.Parameters.Add("counterid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillsettlementmaster.counterid;

            command.Parameters.Add("loanproviderid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillsettlementmaster.loanproviderid;
            command.Parameters.Add("swipbankid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillsettlementmaster.swipbankid;
            command.Parameters.Add("swipmachineid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillsettlementmaster.swipmachineid;
            command.Parameters.Add("loan_paid", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillsettlementmaster.loan_paid;
            command.Parameters.Add("loan_reference", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBillsettlementmaster.loan_reference == null ? "" : oBillsettlementmaster.loan_reference;

            command.Parameters.Add("intvendor", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBillsettlementmaster.intvendor;
            command.Parameters.Add("intmode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBillsettlementmaster.intmode == null ? "" : oBillsettlementmaster.intmode;

            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oBillsettlementmaster.version = Convert.ToInt32(reader[0].ToString());
                }
            }

            return oBillsettlementmaster;
        }


        public bool Delete(Billsettlementmaster oBillsettlementmaster)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oBillsettlementmaster);
                    BillsettlementDataService subBillsettlement = new BillsettlementDataService() { Context = this.Context };
                    subBillsettlement.Context = this.Context;
                    subBillsettlement.DeleteAll(oBillsettlementmaster.id, oBillsettlementmaster.modifiedby, conn, transaction);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oBillsettlementmaster);
            }
            return hasSuccessCount;
        }

        public bool Cancel(Billsettlementmaster oBillsettlementmaster)
        {
            string SQL_billupdate = "UPDATE public.bill set settled=false,settledcancelled=true,modifiedby=@modifiedby,modifiedon=@modifiedon where billno=@billno;" +
                "update customerrewards set billvalue=0,rewards=0,modifiedby=@modifiedby,modifiedon=@modifiedon where billid=@billid; " +
                "update customerinvoice set isactive=false,modifiedby=@modifiedby,modifiedon=@modifiedon where paid=0 and billid=@billid;" +
                "update vouchers set isactive=false,modifiedby=@modifiedby,modifiedon=@modifiedon where vouchertype=4 and billid=@billid;" +
                "update vouchers set isactive=false,modifiedby=@modifiedby,modifiedon=@modifiedon from coupons where coupons.id = vouchers.coupenid and vouchers.vouchertype = 7 and billsettlementmasterid=@billsettlementmasterid;" +
                "update coupons set isactive=false,modifiedby=@modifiedby,modifiedon=@modifiedon where billsettlementmasterid=@billsettlementmasterid;";

            BillsettlementDataService subBillsettlement = new BillsettlementDataService() { Context = this.Context };
            subBillsettlement.Context = this.Context;
            List<Billsettlement> billsettlementlist = subBillsettlement.SearchAll(oBillsettlementmaster.id);

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oBillsettlementmaster);
                    subBillsettlement.DeleteAll(oBillsettlementmaster.id, oBillsettlementmaster.modifiedby, conn, transaction);
                    foreach (var item in billsettlementlist)
                    {
                        NpgsqlCommand command = GetCommand(SQL_billupdate, conn, transaction);
                        command.Parameters.Add("billno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = item.billno;
                        command.Parameters.Add("billid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.billid;
                        command.Parameters.Add("billsettlementmasterid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillsettlementmaster.id;
                        command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = false;
                        command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                        command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
                        int successCount = command.ExecuteNonQuery();
                    }
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oBillsettlementmaster);
            }
            return hasSuccessCount;

        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Billsettlementmaster oBillsettlementmaster)
        {

            NpgsqlCommand command = GetCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = false;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillsettlementmaster.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

        public string CurrentAutoNumber(string locationid, string counterid)
        {

            string result = "";
            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    var sequence = SequenceManager.Instance(Context.OrganizationID).Get("billsettlement.singlewindow", long.Parse(locationid), 0, long.Parse(counterid));
                    result = sequence.GetCurrentNumber(conn, transaction);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, "NextAutoNumber");
                throw exp;
            }
            return result;
        }
        public string NextAutoNumber(string locationid, string counterid)
        {

            string result = "";
            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    var sequence = SequenceManager.Instance(Context.OrganizationID).Get("billsettlement.singlewindow", long.Parse(locationid), 0, long.Parse(counterid));
                    result = sequence.GetNextNumber(Context, conn, transaction);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, "NextAutoNumber");
                throw exp;
            }
            return result;
        }

        internal string HasSettlementNumber(string locationid, string counterid, string uniqueid)
        {
            string query = @"select settlementon,code from billsettlementmaster
where locationid=@locationid and counterid=@counterid and 
paymentinfo->>'paymentuniqueid' = @uniqueid
limit 1";
            var param = new SortedList<string, object>();
            param.Add("locationid", long.Parse(locationid));
            param.Add("counterid", long.Parse(counterid));
            param.Add("uniqueid", uniqueid);
            using (var reader = ExecuteReader(query, param))
            {
                if (reader.Read())
                {
                    return "Already consumed in " + reader["code"].ToString() + " on " +
                        Convert.ToDateTime(reader["settlementon"]).ToString("dd-MM-yyyy hh:mm tt");
                }
            }
            return null;
        }

        internal bool Reconilation(BillsettlementmasterCreteria item)
        {
            string updatereco = "update billsettlementmaster set recon={0} where id in ({1})";

            if (item.reconids != null && item.reconids.Count > 0)
                ExecuteQuery(string.Format(updatereco, "true", string.Join(",", item.reconids)), null);
            if (item.reconmids != null && item.reconmids.Count > 0)
                ExecuteQuery(string.Format(updatereco, "false", string.Join(",", item.reconmids)), null);

            return true;
        }
    }

}
