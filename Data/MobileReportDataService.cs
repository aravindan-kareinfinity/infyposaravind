using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
using Newtonsoft.Json;
using System.Text;

namespace InfyPOS.Data{
    public class MobileReportDataService : ReportData
    {

        public MobileReportDataService() : base()
        {
            httpcontext = System.Web.HttpContext.Current;
        }

        const string SQL_SettlementSMachine = @"select cast(paymentinfo->>'card_bankid' as bigint) bankid, machine.name tid, bill.billno,
customer.name customername, bill.billdate, billsettlement.receivable, paid_card
from billsettlementmaster inner join billsettlement on billsettlement.billsettlementmasterid = billsettlementmaster.id
inner join bill on bill.id = billsettlement.billid 
left outer join customer on customer.id = bill.customerid
left outer join referencelist machine on machine.id = swipmachineid
where billsettlementmaster.isactive=true and billsettlementmaster.settlementon>=@fromdate and billsettlementmaster.settlementon<@todate
and case when @locationid=0 then billsettlementmaster.locationid <> 0 else billsettlementmaster.locationid = @locationid end
and case when @companyid=0 then billsettlementmaster.companyid >= 0 else billsettlementmaster.companyid = @companyid end
and paid_card > 0";

        const string SQL_ECSMachine = @"select swipbankid as bankid,machine.name tid,customerphoneno as billno,
        customername,rechargedate as billdate, rate as receivable ,paid_card
        from recharge
         left outer join referencelist machine on machine.id = recharge.swipmachineid 
         where recharge.isactive=true and recharge.rechargedate>=@fromdate and recharge.rechargedate<@todate
         and case when @locationid=0 then recharge.locationid <> 0 else recharge.locationid = @locationid end
         and case when @companyid=0 then recharge.companyid >= 0 else recharge.companyid = @companyid end
         and paid_card > 0";

        const string SQL_EMICustomer = @"select billsettlementmaster.settlementon,billsettlementmaster.locationid,customer.name, customer.gstno, customer.address,
billsettlement.billno,loan_reference,referencelist.name loanprovider, 
billsettlementmaster.receivable, paid_cash cash, paid_upi upi, paid_card card, paid_credit credit,paid_voucher voucher, loan_paid loan,
case when billsettlementmaster.paidamount<0 then cast(paymentinfo->>'cash_return' as numeric) else 0 end cashreturn,
billitems.qty, billitems.barcode,billitems.salablegoodsid, stock.printingname
from billsettlementmaster 
inner join billsettlement on billsettlement.billsettlementmasterid = billsettlementmaster.id
inner join bill on bill.id = billsettlement.billid
inner join billitems on billitems.billid = bill.id and billitems.isactive=true
inner join salablegoods on salablegoods.id = billitems.salablegoodsid
inner join stock on stock.id = salablegoods.stockid
inner join customer on customer.id = billsettlementmaster.customerid
inner join referencelist on referencelist.id = loanproviderid
where loan_paid>0 and settlementon>=@fromdate and settlementon<@todate 
and billsettlementmaster.isactive = true
and case when @locationid=0 then billsettlementmaster.locationid >= 0 else billsettlementmaster.locationid = @locationid end
and case when @companyid=0 then billsettlementmaster.companyid >= 0 else billsettlementmaster.companyid = @companyid end ";

        const string SQL_ECStatement = @"with balance as 
(
	select locationid,sum(recharged) recharged,sum(transferout) transferout,sum(transferin) transferin,
	sum(topup) topup,sum(balance) balance from rechargebalance
	where isactive = true group by locationid
),
ecsale as 
(
	select recharge.locationid,sum(rate) rate,sum(paid_cash+paid_card+paid_upi) totalamount,sum(paid_cash) paid_cash,sum(paid_card) paid_card,sum(paid_upi) paid_upi 
	from recharge where recharge.isactive = true and tranmode = 3
	and rechargedate>=@fromdate and rechargedate<@todate
	group by recharge.locationid
)
select balance.locationid,transferin,topup,transferout,recharged,balance,rate, totalamount, paid_cash,paid_card,paid_upi
from balance left outer join ecsale on ecsale.locationid = balance.locationid";


        const string SQL_BillItemDetails = @"select bill.billdate,bill.locationid,billsettlement.billno, customer.name, 
customer.gstno, customer.address, stock.printingname, billitems.qty, billitems.barcode,
billitems.price,billitems.discount,billitems.additionaldiscount,billitems.receivable,
billsettlementmaster.settlementon,loan_reference,referencelist.name loanprovider, 
bill.receivable billvalue, billsettlementmaster.code, paid_cash cash, paid_upi upi, paid_card card, paid_credit credit,paid_voucher voucher, loan_paid loan,
case when billsettlementmaster.paidamount<0 then cast(paymentinfo->>'cash_return' as numeric) else 0 end cashreturn,
billitems.taxpercentage, billitems.tax, bill.isinterstatesale,(billitems.qty::numeric*stock.buyingprice::numeric) buyingprice
from bill inner join billitems on billitems.billid = bill.id and billitems.isactive=true
inner join salablegoods on salablegoods.id = billitems.salablegoodsid
inner join stock on stock.id = salablegoods.stockid
left outer join customer on customer.id = bill.customerid
left outer join billsettlement on billsettlement.billid = bill.id
left outer join billsettlementmaster on billsettlementmaster.id = billsettlement.billsettlementmasterid
left outer join referencelist on referencelist.id = loanproviderid
where bill.billdate>=@fromdate and bill.billdate<@todate 
and case when @locationid=0 then bill.locationid >= 0 else bill.locationid = @locationid end
and case when @companyid=0 then bill.companyid >= 0 else bill.companyid = @companyid end 
and bill.isactive = true and billitems.isactive=true
order by bill.locationid, bill.billdate, bill.id";

        const string SQL_SchemeBarcodeWise = @"select bill.billno as billno,bill.billdate::date,bill.locationid,
products.name printingname, brand.name brandname, stock.designid, stock.displaymrp, stock.buyingprice, 
(salablegoods.price-salablegoods.discount) saleableprice, stock.price,stock.discount,
billitems.qty, billitems.barcode, stock.serialno, billitems.price salablegoodsprice,
billitems.schemediscount,billitems.receivable billvalue, promotions.name as schemename, tax.id taxid,
(case when billitems.schemeid=@schemeid  or @schemeid=0 then billitems.receivable else 0 end) as discountedbillvalue
from bill inner join billitems on billitems.billid = bill.id and billitems.isactive=true
inner join salablegoods on salablegoods.id = billitems.salablegoodsid
inner join stock on stock.id = salablegoods.stockid
inner join products on products.id = stock.productid
left outer join promotions on billitems.schemeid = promotions.id
left outer join tax on tax.id = products.salestaxid 
left outer join items on stock.itemid = items.id 
left outer join brand brand on brand.id = items.brandid
where bill.billdate>=@fromdate and bill.billdate<@todate and (billitems.schemeid=@schemeid or @schemeid=0)
and case when @locationid=0 then bill.locationid >= 0 else bill.locationid = @locationid end
and case when @companyid=0 then bill.companyid >= 0 else bill.companyid = @companyid end 
and case when @brandid=0 then items.brandid >= 0 else items.brandid = @brandid end
and bill.isactive = true and billitems.isactive=true and bill.billtype != 9 
order by bill.locationid, bill.billdate, bill.id";

        const string SQL_SchemeProductWise = @"select printingname,sum(case when schemeid=@schemeid  or @schemeid=0 then billitems.schemediscount else 0 end) as schemediscount,
sum(billitems.receivable) as billvalue,sum(case when billitems.schemeid=@schemeid  or @schemeid=0 then billitems.qty else 0 end) as qty,
sum(case when billitems.schemeid=@schemeid  or @schemeid=0 then billitems.receivable else 0 end) as discountedbillvalue from bill 
inner join billitems on billitems.billid = bill.id  and billitems.isactive=true
where billdate>=@fromdate and billdate<@todate and case when @locationid=0 then bill.locationid >= 0 else bill.locationid = @locationid end
and case when @companyid=0 then bill.companyid >= 0 else bill.companyid = @companyid end 
and bill.isactive = true and billitems.isactive=true and bill.billtype != 9 and bill.isb2b=false
group by printingname";

        const string SQL_SchemeDateWise = @"with datewise
as 
(select billdate::date as billdate,
sum(case when schemeid=@schemeid or @schemeid=0 then billitems.schemediscount else 0 end) as schemediscount,
sum(billitems.receivable) as billvalue,sum(case when billitems.schemeid=@schemeid  or @schemeid=0 then billitems.qty else 0 end) as qty,
sum(case when billitems.schemeid=@schemeid or @schemeid=0 then billitems.receivable else 0 end) as discountedbillvalue
from bill 	
inner join billitems on billitems.billid = bill.id	and billitems.isactive=true
where billdate>=@fromdate and billdate<@todate and case when @locationid=0 then bill.locationid >= 0 else bill.locationid = @locationid end
and case when @companyid=0 then bill.companyid >= 0 else bill.companyid = @companyid end 
and bill.isactive = true and billitems.isactive=true and bill.billtype != 9 and bill.isb2b=false
group by billdate::date), roundings as 
(
select billdate::date as billdate,
sum(rounding) as rounding
from bill 	
where billdate>=@fromdate and billdate<@todate and case when @locationid=0 then bill.locationid >= 0 else bill.locationid = @locationid end
and case when @companyid=0 then bill.companyid >= 0 else bill.companyid = @companyid end 
and bill.isactive = true and bill.billtype != 9 and bill.isb2b=false
group by billdate::date
)

select roundings.billdate,schemediscount,discountedbillvalue,billvalue,rounding,datewise.qty from roundings
left outer join datewise on datewise.billdate = roundings.billdate
order by roundings.billdate";

        const string SQL_AllSchemeDateWise = @"with datewise
as 
(select 
schemeid,
sum(billitems.schemediscount) as schemediscount,
sum(case when billitems.schemediscount>0 then billitems.receivable else 0 end) as discountedbillvalue,
sum(billitems.receivable) as billvalue,sum(case when billitems.schemeid=@schemeid  or @schemeid=0 then billitems.qty else 0 end) as qty
from bill 	
inner join billitems on billitems.billid = bill.id	and billitems.isactive=true
where billdate>=@fromdate and billdate<@todate 
and case when @locationid=0 then bill.locationid >= 0 else bill.locationid = @locationid end
and case when @companyid=0 then bill.companyid >= 0 else bill.companyid = @companyid end 
and bill.isactive = true and billitems.isactive=true and bill.billtype != 9 and bill.isb2b=false
group by schemeid)

select promotions.name as schemename,datewise.schemediscount,datewise.discountedbillvalue,datewise.billvalue,
0 rounding,datewise.qty from datewise
inner join promotions on datewise.schemeid = promotions.id";
        public List<SettlementReport> SearchSettlementSMachine(RetailReportCreteria _reportcreteria)
        {
            List<SettlementReport> lst = new List<SettlementReport>();
            DateTime today = _reportcreteria.fromdate.Date;
            DateTime nextdate = _reportcreteria.todate.AddDays(1).Date;
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_reportcreteria.showdetail == true)
                {
                    command = GetCommand(SQL_ECSMachine, conn);
                }
                else
                    command = GetCommand(SQL_SettlementSMachine +" union all "+ SQL_ECSMachine, conn);
                command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _reportcreteria.locationid;
                command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _reportcreteria.companyid;
                command.Parameters.Add("fromdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = today;
                command.Parameters.Add("todate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = nextdate;
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        SettlementReport oitem = new SettlementReport();

                        oitem.bankid = reader["bankid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["bankid"]);
                        oitem.counter = reader["billno"] == DBNull.Value ? "" : reader["billno"].ToString();
                        oitem.employeename = reader["tid"] == DBNull.Value ? "" : reader["tid"].ToString();
                        oitem.receiptdate = reader["billdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["billdate"]);
                        oitem.receivable = reader["receivable"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["receivable"]);
                        oitem.paid_card = reader["paid_card"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["paid_card"]);
                        oitem.paid_upi = reader["paid_upi"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["paid_upi"]);
                        oitem.customername = reader["customername"] == DBNull.Value ? "" : reader["customername"].ToString();
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

        public List<EMIStatementReport> SearchEMIStatement(RetailReportCreteria _reportcreteria)
        {
            List<EMIStatementReport> lst = new List<EMIStatementReport>();
            DateTime today = _reportcreteria.fromdate.Date;
            DateTime nextdate = _reportcreteria.todate.AddDays(1).Date;
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                command = GetCommand(SQL_EMICustomer, conn);
                command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _reportcreteria.locationid;
                command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _reportcreteria.companyid;
                command.Parameters.Add("fromdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = today;
                command.Parameters.Add("todate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = nextdate;
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        EMIStatementReport oitem = new EMIStatementReport();
                        oitem.settlementdate = reader["settlementon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["settlementon"]);
                        oitem.locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                        oitem.billno = reader["billno"] == DBNull.Value ? "" : reader["billno"].ToString();
                        oitem.barcode = reader["barcode"] == DBNull.Value ? "" : reader["barcode"].ToString();
                        oitem.customername = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
                        oitem.customergstno = reader["gstno"] == DBNull.Value ? "" : reader["gstno"].ToString();
                        oitem.customeraddress = reader["address"] == DBNull.Value ? "" : reader["address"].ToString();
                        oitem.loanprovider = reader["loanprovider"] == DBNull.Value ? "" : reader["loanprovider"].ToString();
                        oitem.loanreferenceno = reader["loan_reference"] == DBNull.Value ? "" : reader["loan_reference"].ToString();
                        oitem.productname = reader["printingname"] == DBNull.Value ? "" : reader["printingname"].ToString();
                        oitem.qty = reader["qty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["qty"]);
                        oitem.receivable = reader["receivable"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["receivable"]);
                        oitem.paid_cash = reader["cash"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["cash"]);
                        oitem.paid_card = reader["card"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["card"]);
                        oitem.paid_upi = reader["upi"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["upi"]);
                        oitem.paid_credit = reader["credit"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["credit"]);
                        oitem.paid_voucher = reader["voucher"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["voucher"]);
                        oitem.return_cash = reader["cashreturn"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["cashreturn"]);
                        oitem.loanamount = reader["loan"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["loan"]);
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

        public List<ECReport> SearchECStatement(RetailReportCreteria _reportcreteria)
        {
            List<ECReport> lst = new List<ECReport>();
            DateTime today = _reportcreteria.fromdate.Date;
            DateTime nextdate = _reportcreteria.todate.AddDays(1).Date;
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                command = GetCommand(SQL_ECStatement, conn);
                command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _reportcreteria.locationid;
                command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _reportcreteria.companyid;
                command.Parameters.Add("fromdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = today;
                command.Parameters.Add("todate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = nextdate;
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        ECReport oitem = new ECReport();
                        oitem.locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                        oitem.transferin = reader["transferin"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["transferin"]);
                        oitem.transferout = reader["transferout"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["transferout"]);
                        oitem.topup = reader["topup"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["topup"]);
                        oitem.recharged = reader["recharged"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["recharged"]);
                        oitem.balance = reader["balance"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["balance"]);
                        oitem.rate = reader["rate"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["rate"]);
                        oitem.totalamount = reader["totalamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["totalamount"]);
                        oitem.paid_cash = reader["paid_cash"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["paid_cash"]);
                        oitem.paid_card = reader["paid_card"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["paid_card"]);
                        oitem.paid_upi = reader["paid_upi"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["paid_upi"]);
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

        public List<BillItemDetailsReport> SearchBillItemDetails(RetailReportCreteria _reportcreteria)
        {
            List<BillItemDetailsReport> lst = new List<BillItemDetailsReport>();
            DateTime today = _reportcreteria.fromdate.Date;
            DateTime nextdate = _reportcreteria.todate.AddDays(1).Date;
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                command = GetCommand(SQL_BillItemDetails, conn);
                command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _reportcreteria.locationid;
                command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _reportcreteria.companyid;
                command.Parameters.Add("fromdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = today;
                command.Parameters.Add("todate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = nextdate;
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        BillItemDetailsReport oitem = new BillItemDetailsReport();
                        oitem.billdate = reader["billdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["billdate"]);
                        oitem.settlementdate = reader["settlementon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["settlementon"]);
                        oitem.locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                        oitem.billno = reader["billno"] == DBNull.Value ? "" : reader["billno"].ToString();
                        oitem.settlementcode = reader["code"] == DBNull.Value ? "" : reader["code"].ToString();
                        oitem.barcode = reader["barcode"] == DBNull.Value ? "" : reader["barcode"].ToString();
                        oitem.customername = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
                        oitem.customergstno = reader["gstno"] == DBNull.Value ? "" : reader["gstno"].ToString();
                        oitem.customeraddress = reader["address"] == DBNull.Value ? "" : reader["address"].ToString();
                        oitem.loanprovider = reader["loanprovider"] == DBNull.Value ? "" : reader["loanprovider"].ToString();
                        oitem.loanreferenceno = reader["loan_reference"] == DBNull.Value ? "" : reader["loan_reference"].ToString();
                        oitem.productname = reader["printingname"] == DBNull.Value ? "" : reader["printingname"].ToString();
                        oitem.qty = reader["qty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["qty"]);
                        oitem.price = reader["price"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["price"]);
                        oitem.discount = reader["discount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discount"]);
                        oitem.additionaldiscount = reader["additionaldiscount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["additionaldiscount"]);
                        oitem.receivable = reader["receivable"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["receivable"]);
                        oitem.billvalue = reader["billvalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["billvalue"]);
                        oitem.buyingprice = reader["buyingprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["buyingprice"]);

                        oitem.paid_cash = reader["cash"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["cash"]);
                        oitem.paid_card = reader["card"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["card"]);
                        oitem.paid_credit = reader["credit"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["credit"]);
                        oitem.paid_voucher = reader["voucher"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["voucher"]);
                        oitem.return_cash = reader["cashreturn"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["cashreturn"]);
                        oitem.loanamount = reader["loan"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["loan"]);

                        oitem.taxpercentage = reader["taxpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["taxpercentage"]);
                        oitem.tax = reader["tax"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["tax"]);
                        oitem.isinterstatesale = reader["isinterstatesale"] == DBNull.Value ? false : Convert.ToBoolean(reader["isinterstatesale"]);
                        if (oitem.isinterstatesale)
                            oitem.igst = oitem.tax;
                        else
                        {
                            oitem.cgst = Math.Round(oitem.tax / 2, 2);
                            oitem.sgst = Math.Round(oitem.tax / 2, 2);
                        }
                        oitem.taxable = Math.Round(oitem.receivable - oitem.tax, 2);
                        oitem.marginvalue = Math.Round(oitem.taxable - oitem.buyingprice, 2);
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
        public string GetSQL4B2B(string locationfilter, string query)
        {
            if (!string.IsNullOrEmpty(locationfilter))
                query = query.Replace("and bill.isb2b=false", "and bill.isb2b=false " + locationfilter);

            if (InfyPOS.Services.ConfigManager.Instance(this.Context.OrganizationID).Config.system_isb2bapplication ||
                InfyPOS.Services.ConfigManager.Instance(this.Context.OrganizationID).Config.system_ismfgapplication)
                return query.Replace("and bill.isb2b=false", "").Replace("and bill.isb2b=@isb2b", "");
            return query;
        }

        public List<SchemeDetailsReport> SearchSchemeDetails(RetailReportCreteria _reportcreteria)
        {
            List<SchemeDetailsReport> lst = new List<SchemeDetailsReport>();
            DateTime today = _reportcreteria.fromdate.Date;
            DateTime nextdate = _reportcreteria.todate.AddDays(1).Date;
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                QueryBuilder qb = new QueryBuilder();
                if (_reportcreteria.mode == "1")
                {
                    command = GetCommand(GetSQL4B2B("", SQL_SchemeBarcodeWise), conn);
                }
                else if(_reportcreteria.mode=="2")
                {
                    command = GetCommand(GetSQL4B2B("", SQL_SchemeProductWise), conn);
                }
                else if(_reportcreteria.schemeid>0)
                    command = GetCommand(GetSQL4B2B("", SQL_SchemeDateWise), conn);
                else
                    command = GetCommand(GetSQL4B2B("", SQL_AllSchemeDateWise), conn); 
                
                command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _reportcreteria.locationid;
                command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _reportcreteria.companyid;
                command.Parameters.Add("schemeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _reportcreteria.schemeid;
                command.Parameters.Add("brandid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _reportcreteria.brandid;
                command.Parameters.Add("fromdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = today;
                command.Parameters.Add("todate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = nextdate;
                command.Parameters.Add("isb2b", NpgsqlTypes.NpgsqlDbType.Boolean).Value = _reportcreteria.isb2b;
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        SchemeDetailsReport oitem = new SchemeDetailsReport();
                        if (_reportcreteria.mode == "1")
                        {
                            oitem.locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                            oitem.billno = reader["billno"] == DBNull.Value ? "" : reader["billno"].ToString();
                            oitem.billdate = reader["billdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["billdate"]);
                            oitem.barcode = reader["barcode"] == DBNull.Value ? "" : reader["barcode"].ToString();
                            oitem.serialno = reader["serialno"] == DBNull.Value ? "" : reader["serialno"].ToString();
                            oitem.productname = reader["printingname"] == DBNull.Value ? "" : reader["printingname"].ToString();
                            oitem.brandname = reader["brandname"] == DBNull.Value ? "" : reader["brandname"].ToString();
                            oitem.designid = reader["designid"] == DBNull.Value ? "" : reader["designid"].ToString();
                            oitem.schemename = reader["schemename"] == DBNull.Value ? "" : reader["schemename"].ToString();
                            oitem.mrp = reader["displaymrp"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["displaymrp"]);
                            oitem.sellingprice = reader["saleableprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["saleableprice"]);
                            oitem.saletaxid = reader["taxid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["taxid"]);
                            oitem.buyingprice = reader["buyingprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["buyingprice"]);
                            oitem.schemediscount = reader["schemediscount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["schemediscount"]);
                            oitem.billvalue = reader["billvalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["billvalue"]);
                            oitem.qty = reader["qty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["qty"]);
                            oitem.discountedbillvalue = reader["discountedbillvalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discountedbillvalue"]);

                            // for 12 to 18% GST chages - price updated in salablegoods but not updated MRP
                            decimal salablegoodsprice = reader["salablegoodsprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["salablegoodsprice"]);
                            //if (salablegoodsprice > oitem.mrp)
                            //    oitem.mrp = salablegoodsprice;

                            if (oitem.qty < 0)
                            {
                                oitem.sellingprice = oitem.sellingprice * -1;
                                oitem.mrp = oitem.mrp * -1;
                                oitem.schemediscount = oitem.schemediscount * -1;
                            }

                            /// changed from oitem.mrp to sellingprice below 
                            if (oitem.schemediscount > 0 && oitem.sellingprice > 0)
                                oitem.schemediscountpercent = Math.Round(((oitem.schemediscount / oitem.qty) * 100) / oitem.sellingprice, 2);

                            oitem.netamount = Math.Round(oitem.sellingprice - oitem.schemediscount, 2);
                            if (oitem.netamount <= 2625)
                                oitem.nsvtax = (oitem.netamount * Convert.ToDecimal(4.76) / 100);
                            else
                                oitem.nsvtax = (oitem.netamount * Convert.ToDecimal(15.25) / 100);

                            // NSV margin percentage will be considered only if there is any scheme discount
                            if (oitem.schemediscount > 0)
                                oitem.nsvmargin = (oitem.netamount * Convert.ToDecimal((_reportcreteria.discount > 0 ? _reportcreteria.discount : 25) / 100));
                            else
                                oitem.nsvmargin = (oitem.netamount * Convert.ToDecimal((_reportcreteria.additionaldiscount > 0 ? _reportcreteria.additionaldiscount : 25) / 100));

                            if (oitem.sellingprice <= 2625)
                                oitem.mrptax = (oitem.sellingprice * Convert.ToDecimal(4.76) / 100);
                            else
                                oitem.mrptax = (oitem.sellingprice * Convert.ToDecimal(15.25) / 100);

                            oitem.mrpmargin = (oitem.sellingprice * Convert.ToDecimal((_reportcreteria.additionaldiscount > 0 ? _reportcreteria.additionaldiscount : 30) / 100));

                            oitem.buyingprice = (oitem.sellingprice - oitem.mrptax - oitem.mrpmargin);
                        }
                        if (_reportcreteria.mode == "2")
                        {
                            oitem.productname = reader["printingname"] == DBNull.Value ? "" : reader["printingname"].ToString();
                            oitem.schemediscount = reader["schemediscount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["schemediscount"]);
                            oitem.billvalue = reader["billvalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["billvalue"]);
                            oitem.qty = reader["qty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["qty"]);
                            oitem.discountedbillvalue = reader["discountedbillvalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discountedbillvalue"]);
                        }
                        if (_reportcreteria.mode == "3")
                        {
                            oitem.rounding = reader["rounding"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["rounding"]);
                            if(_reportcreteria.schemeid>0)
                                oitem.billdate = reader["billdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["billdate"]);
                            else
                                oitem.schemename = reader["schemename"] == DBNull.Value ? "" : reader["schemename"].ToString();

                            oitem.schemediscount = reader["schemediscount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["schemediscount"]);
                            oitem.billvalue = reader["billvalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["billvalue"]);
                            oitem.qty = reader["qty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["qty"]);
                            oitem.discountedbillvalue = reader["discountedbillvalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discountedbillvalue"]);
                        }

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

            if (_reportcreteria.mode == "1" && lst != null && lst.Count > 0)
            {
                TaxDataService taxdata = new TaxDataService() { Context = this.Context };
                var taxlist = taxdata.Search(new TaxCreteria() { });
                foreach (SchemeDetailsReport stock in lst)
                {
                    if (stock.saletaxid > 0)
                    {
                        if (taxlist.Exists(e => e.id == stock.saletaxid))
                        {
                            var taxitem = taxlist.Find(e => e.id == stock.saletaxid);
                            decimal taxpercentage = 0, taxamount = 0;
                            if (taxitem.type == 1)
                                taxpercentage = taxitem.taxpercentage;
                            else if (taxitem.type == 2 && taxitem.taxsplit != null && taxitem.taxsplit.items.Count > 1)
                            {
                                if (stock.buyingprice > taxitem.taxsplit.items[0].fromamount
                                    && stock.buyingprice <= taxitem.taxsplit.items[0].toamount)
                                    taxpercentage = (taxitem.taxsplit.items[0].cgst * 2);
                                else
                                    taxpercentage = (taxitem.taxsplit.items[1].cgst * 2);
                            }
                            else if (taxitem.type == 2 && taxitem.taxsplit != null && taxitem.taxsplit.items.Count == 1)
                            {
                                taxpercentage = (taxitem.taxsplit.items[0].cgst * 2);
                            }
                            if (taxpercentage > 0 && stock.buyingprice > 0)
                            {
                                taxamount = Math.Round((stock.buyingprice * taxpercentage) / 100, 2);
                                // Inclusive tax
                                //  taxamount = (stock.buyingprice / (100 + taxpercentage) * taxpercentage);
                            }
                            if (stock.buyingprice > 0 && stock.mrp > 0)
                            {
                                stock.margin = Math.Round(stock.mrp - stock.buyingprice, 2);
                                //stock.netmarginpercent = Math.Round(((stock.price - stock.discount - stock.buyingprice - taxamount) / stock.buyingprice) * 100, 2);
                            }

                            stock.tax = taxamount;
                            stock.taxpercentage = taxpercentage;

                            // Bill value
                            stock.billvalue = stock.buyingprice + stock.tax;
                            stock.payble = stock.netamount - stock.nsvtax - stock.nsvmargin + stock.tax;
                            stock.cn = stock.billvalue - stock.payble;
                        }

                    }
                }
            }
            return lst;
        }

    }
    public class EMIStatementReport : BaseEntity
    {
        public long id { get; set; }
        public DateTime settlementdate { get; set; }
        public long locationid { get; set; }
        public string customername { get; set; }
        public string customergstno { get; set; }
        public string customeraddress { get; set; }
        public long companyid { get; set; }
        public string billno { get; set; }
        public string barcode { get; set; }
        public string serialno { get; set; }
        public string loanreferenceno { get; set; }
        public string loanprovider { get; set; }
        public decimal receivable { get; set; }
        public decimal billamount { get; set; }
        public decimal discount { get; set; }
        public decimal additionaldiscount { get; set; }
        public decimal paidamount { get; set; }
        public decimal tax { get; set; }
        public decimal paid_cash { get; set; }
        public decimal paid_card { get; set; }
        public decimal paid_credit { get; set; }
        public decimal paid_voucher { get; set; }
        public decimal loanamount { get; set; }
        public decimal return_cash { get; set; }
        public decimal qty { get; set; }
        public string productname { get; set; }
        public string brandname { get; set; }
        public string designid { get; set; }
        public decimal paid_upi { get;  set; }
    }
    public class ECReport : BaseEntity
    {
        public long locationid { get; set; }
        public decimal transferin { get; set; }
        public decimal transferout { get; set; }
        public decimal topup { get; set; }
        public decimal recharged { get; set; }
        public decimal balance { get; set; }
        public decimal rate { get; set; }
        public decimal paid_cash { get; set; }
        public decimal paid_card { get; set; }
        public decimal paid_upi { get; set; }
        public decimal totalamount { get; set; }
    }

    public class BillItemDetailsReport : EMIStatementReport
    {
        public DateTime billdate { get; set; }
        public decimal billvalue { get; set; }
        public decimal price { get; set; }
        public decimal taxpercentage { get; set; }
        public decimal cgst { get; set; }
        public decimal sgst { get; set; }
        public decimal igst { get; set; }
        public bool isinterstatesale { get; set; }
        public string settlementcode { get; set; }
        public decimal buyingprice { get; set; }
        public decimal taxable { get; set; }
        public decimal marginvalue { get; set; }
    }

    public class SchemeDetailsReport : EMIStatementReport
    {
        public DateTime billdate { get; set; }
        public decimal billvalue { get; set; }
        public decimal mrp { get; set; }
        public decimal sellingprice { get; set; }
        public decimal margin { get; set; }
        public decimal buyingprice { get; set; }
        public long saletaxid { get; set; }
        public decimal taxpercentage { get; set; }
        public long schemeid { get; set; }
        public string schemename { get; set; }
        public decimal schemediscount { get; set; }
        public decimal schemediscountpercent { get; set; }
        public decimal discountedbillvalue { get;  set; }
        public decimal rounding { get; set; }
        public decimal netamount { get; set; }
        public decimal nsvtax { get; set; }
        public decimal nsvmargin { get; set; }
        public decimal mrptax { get; set; }
        public decimal mrpmargin { get; set; }
        public decimal payble { get; set; }
        public decimal cn { get; set; }
    }

}
