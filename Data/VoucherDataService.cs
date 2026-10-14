using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
using InfyPOS.Tally;
using Newtonsoft.Json;


namespace InfyPOS.Data
{
    public class VoucherDataService : SAASData
    {
        const string SQL_Search = @"select voucher.id, vouchertype,vouchertypeid,purchasetypeid, voucherdate, voucherno,bwise, narration, amount, 
                                    voucher.isactive, voucher.createdby, voucher.modifiedby,sourcetype, voucher.version, voucher.createdon, 
                                    voucher.modifiedon,adjustmentcredit,adjustmentledgerid,locationid,voucher.employeeid,voucher.companyid,
                                    attributes,muser.employeeid memployeeid,cuser.employeeid cemployeeid,voucher.uniqueuid,voucher.isinterstate
                                    from voucher
                                    left outer join users cuser on cuser.id = voucher.createdby 
                                    left outer join users muser on muser.id = voucher.modifiedby
                                    where voucher.id=@id";

        const string SQL_Insert = @"INSERT INTO public.voucher(vouchertype,vouchertypeid,purchasetypeid, voucherdate, voucherno,bwise, narration, amount, isactive, createdby, modifiedby, version, createdon, modifiedon, adjustmentcredit,adjustmentledgerid,locationid,employeeid,sourcetype,attributes,companyid,uniqueuid,isinterstate)
                                    VALUES(@vouchertype,@vouchertypeid,@purchasetypeid, @voucherdate, @voucherno,@bwise, @narration, @amount, @isactive, @createdby, @modifiedby, @version, @createdon, @modifiedon, @adjustmentcredit, @adjustmentledgerid,@locationid,@employeeid,@sourcetype,@attributes,@companyid,@uniqueuid,@isinterstate) RETURNING id";

        const string SQL_Update = @"UPDATE public.voucher SET vouchertype=@vouchertype,vouchertypeid=@vouchertypeid,purchasetypeid=@purchasetypeid, attributes=@attributes, voucherdate=@voucherdate,locationid=@locationid,employeeid=@employeeid, voucherno=@voucherno,bwise=@bwise,
                                    narration=@narration, amount=@amount, modifiedby=@modifiedby, version=@version+1,modifiedon=@modifiedon, adjustmentcredit=@adjustmentcredit,companyid=@companyid,
                                    adjustmentledgerid=@adjustmentledgerid,sourcetype=@sourcetype,isinterstate=@isinterstate where id=@id and version=@version RETURNING version";

        internal Voucher SaveDayBook(VoucherCreteria item)
        {
            bool createsalevoucher = true;
            bool createdepositvoucher = true;
            if (item.id > 0 || item.secondaryid > 0)
            {

                var list = Search(new VoucherCreteria()
                {
                    id = item.id
                });
                if (list.Count == 1)
                {
                    createsalevoucher = false;
                    if (list[0].amount != item.amount)
                    {
                        list[0].amount = item.amount;
                        list[0].voucheritems[0].amount = item.amount;
                        Update(list[0]);
                    }
                }

                list = Search(new VoucherCreteria()
                {
                    id = item.secondaryid
                });
                if (list.Count == 1)
                {
                    createdepositvoucher = false;
                    list[0].amount = item.voucheritems.Sum(e => e.amount);
                    list[0].voucheritems = item.voucheritems;
                    Update(list[0]);
                }
            }
            //if (item.amount > 0 && createsalevoucher)
            //{
            //    Voucher salesvoucher = new Voucher();
            //    salesvoucher.adjustmentledgerid = InfyPOS.Services.CacheManager.Instance(this.Context.OrganizationID).SalesLedgerId;
            //    salesvoucher.adjustmentcredit = true;
            //    salesvoucher.voucherdate = item.attributes.date1;
            //    salesvoucher.attributes = new Voucher.voucharattributes()
            //    {
            //        date1 = item.voucherdate,
            //    };
            //    salesvoucher.locationid = item.locationid;
            //    salesvoucher.companyid = item.companyid;
            //    salesvoucher.isactive = true;
            //    salesvoucher.version = 1;
            //    salesvoucher.sourcetype = 2;
            //    salesvoucher.amount = item.amount;
            //    salesvoucher.voucheritems = new List<Voucheritems>();
            //    salesvoucher.voucheritems.Add(new Voucheritems()
            //    {
            //        amount = item.amount,
            //        ledgerid = item.adjustmentledgerid,
            //        createdby = Context.UserID,
            //        iscredit = false,
            //        createdon = DateTimeNow,
            //        isactive = true
            //    });
            //    salesvoucher.isactive = true;
            //    salesvoucher.version = 1;
            //    salesvoucher.createdby = Context.UserID;
            //    salesvoucher.createdon = DateTimeNow;
            //    Insert(salesvoucher);
            //}

            //if (item.voucheritems.Sum(e => e.amount) > 0 && createdepositvoucher)
            //{
            //    Voucher salesvoucher = new Voucher();
            //    salesvoucher.adjustmentledgerid = item.adjustmentledgerid;
            //    salesvoucher.adjustmentcredit = true;
            //    salesvoucher.voucherdate = item.voucherdate;
            //    salesvoucher.attributes = new Voucher.voucharattributes()
            //    {
            //        date1 = item.voucherdate,
            //    };
            //    salesvoucher.locationid = item.locationid;
            //    salesvoucher.companyid = item.companyid;
            //    salesvoucher.isactive = true;
            //    salesvoucher.version = 1;
            //    salesvoucher.sourcetype = 3;
            //    salesvoucher.amount = item.voucheritems.Sum(e => e.amount);
            //    salesvoucher.voucheritems = new List<Voucheritems>();
            //    foreach (var vitem in item.voucheritems)
            //    {
            //        salesvoucher.voucheritems.Add(new Voucheritems()
            //        {
            //            amount = vitem.amount,
            //            ledgerid = vitem.ledgerid,
            //            createdby = Context.UserID,
            //            iscredit = false,
            //            createdon = DateTimeNow,
            //            isactive = true
            //        });
            //    }
            //    salesvoucher.isactive = true;
            //    salesvoucher.version = 1;
            //    salesvoucher.createdby = Context.UserID;
            //    salesvoucher.createdon = DateTimeNow;
            //    Insert(salesvoucher);
            //}


            //if (item.id > 0)
            //{
            //    item.modifiedby = Context.UserID;
            //    item.modifiedon = DateTimeNow;
            //    Update(item);
            //}
            //else
            //{
            //    item.isactive = true;
            //    item.version = 1;
            //    item.createdby = Context.UserID;
            //    item.createdon = DateTimeNow;
            //    Insert(item);
            //}
            return item;
        }

        /*with voucheritemamount as
(
select sum(case when iscredit=adjustmentcredit then voucheritems.amount*-1 else voucheritems.amount end) camount,
voucherid from voucheritems
inner join voucher on voucher.id = voucherid
where voucheritems.isactive=true
group by voucherid
)
select voucher.id,voucher.amount,camount from voucher 
inner join voucheritemamount on voucheritemamount.voucherid=voucher.id
where camount!=amount
*/

        internal Tuple<bool, decimal> SearchOpening(VoucherCreteria item, decimal obcredit, decimal obdebit)
        {
            string query = @"with ledgerhistory
as
(
select voucheritems.iscredit iscredit,
sum(voucheritems.amount) amount from voucher
inner join voucheritems on voucherid = voucher.id and voucheritems.isactive=true
inner join ledger on ledger.id = voucher.adjustmentledgerid
where ledgerid = @ledgerid and voucher.locationid=@locationid and voucher.isactive = true 
and voucheritems.isactive = true and voucher.voucherdate<@fromdate 
and coalesce(cast(voucher.attributes->>'lrinvoicereturnid' as bigint),0)=0
and coalesce(cast(voucher.attributes->>'customerinvoiceid' as bigint),0)=0
and voucher.companyid=@companyid
group by voucheritems.iscredit
union all
select voucher.adjustmentcredit iscredit,sum(voucheritems.amount) amount from voucher
inner join voucheritems on voucherid = voucher.id and voucheritems.isactive=true
inner join ledger on ledger.id = voucheritems.ledgerid
where adjustmentledgerid=@ledgerid and voucher.locationid=@locationid and voucher.isactive = true 
and coalesce(cast(voucher.attributes->>'lrinvoicereturnid' as bigint),0)=0
and coalesce(cast(voucher.attributes->>'customerinvoiceid' as bigint),0)=0
and voucher.voucherdate<@fromdate and voucher.companyid=@companyid
group by voucher.adjustmentcredit
)
select * from ledgerhistory";

            if (item.locationid == 0)
            {
                query = query.Replace("and voucher.locationid=@locationid", "");
            }
            if (item.companyid == 0)
            {
                query = query.Replace("and voucher.companyid=@companyid", "");
            }

            if (item.onvoucherledger)
            {
                query = query.Replace("and coalesce(cast(voucher.attributes->>'lrinvoicereturnid' as bigint),0)=0", "");
                query = query.Replace("and coalesce(cast(voucher.attributes->>'customerinvoiceid' as bigint),0)=0", "");
            }

            LedgerDataService ledgerDataService = new LedgerDataService() { Context = this.Context };
            var ledger = ledgerDataService.Search(new LedgerCreteria()
            {
                id = item.ledgerid
            }).First();

            var bankid = ledger.bankid;
            bool issupplier = ledger.typeid == 12 && ledger.attributes != null && ledger.attributes.issupplier && ledger.supplierid > 0;
            bool istransport = ledger.typeid == 12 && ledger.attributes != null && ledger.attributes.istransport && ledger.transportid > 0;
            bool iscustomer = ledger.typeid == 11 && ledger.attributes != null && ledger.attributes.iscustomer && ledger.customerid > 0;

            List<long> ptaxid = null;
            List<long> staxid = null;
            List<long> brnchids = null;
            bool iscash = ledger.typeid == 6 && ledger.eodcashledger;
            decimal taxpercentage = 0;
            bool isigst = ledger.typeid == 16 && ledger.taxmode == 3;

            if (ledger.attributes != null && ledger.typeid == 16)
            {
                ptaxid = ledger.attributes.invptaxids;
                staxid = ledger.attributes.invstaxids;
                taxpercentage = ledger.taxpercentage;
                if (ledger.taxmode == 4 || ledger.taxmode == 5)
                    taxpercentage = taxpercentage * 2;
            }
            if (ledger.typeid == 6)
            {
                brnchids = ledger.attributes.branchids;
            }

            if (!issupplier)
            {
                query = query.Replace("and coalesce(cast(voucher.attributes->>'lrinvoicereturnid' as bigint),0)=0", "");
            }

            if (!iscustomer)
            {
                query = query.Replace("and coalesce(cast(voucher.attributes->>'customerinvoiceid' as bigint),0)=0", "");
            }

            NpgsqlConnection conn = CreateConnection();
            List<Tuple<VoucherDisplayItem, List<VoucherDisplayItem>>> lst = new List<Tuple<VoucherDisplayItem, List<VoucherDisplayItem>>>();
            decimal debit = obdebit;
            decimal credit = obcredit;
            try
            {
                conn.Open();
                var command = GetCommand(query, conn);
                command.Parameters.AddWithValue("fromdate", item.fromdate.Date);
                command.Parameters.AddWithValue("todate", item.todate.Date.AddDays(1));
                command.Parameters.AddWithValue("companyid", item.companyid);
                command.Parameters.AddWithValue("locationid", item.locationid);
                command.Parameters.AddWithValue("ledgerid", item.ledgerid);
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (Convert.ToBoolean(reader["iscredit"]))
                            credit += Convert.ToDecimal(reader["amount"]);
                        else
                            debit += Convert.ToDecimal(reader["amount"]);
                    }
                }

                if (staxid != null && staxid.Count > 0)
                {
                    string salestaxquery = @"select
                    sum(billitems.tax) from billitems
                    inner join bill on bill.id = billid
                    where bill.isactive=true and bill.billtype != 9
                    and bill.billdate<@fromdate and billitems.isactive=true
                    and billitems.taxid in ({0}) and 
                    billitems.taxpercentage=@taxpercentage 
                    and bill.isinterstatesale = @isinterstatesale
                    and bill.locationid=@locationid and bill.companyid=@companyid";
                    if (item.companyid == 0)
                    {
                        salestaxquery = salestaxquery.Replace("and voucher.companyid=@companyid", "");
                    }
                    if (item.locationid == 0)
                    {
                        salestaxquery = salestaxquery.Replace("and voucher.locationid=@locationid", "");
                    }
                    command = GetCommand(string.Format(salestaxquery, String.Join(",", staxid)), conn);
                    command.Parameters.AddWithValue("fromdate", item.fromdate.Date);
                    command.Parameters.AddWithValue("isinterstatesale", isigst);
                    command.Parameters.AddWithValue("taxpercentage", taxpercentage);
                    command.Parameters.AddWithValue("companyid", item.companyid);
                    command.Parameters.AddWithValue("locationid", item.locationid);
                    using (System.Data.IDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var tax = reader[0] == DBNull.Value ? 0 : Convert.ToDecimal(reader[0]);
                            if (!isigst) tax = Math.Round(tax / 2, 2);
                            credit += tax;
                        }
                    }
                }

                if (ptaxid != null && ptaxid.Count > 0)
                {
                    string purchasetaxquery = @"select sum(lrinvoicetax.taxamount) from lrinvoicetax
                    inner join lrinvoice on lrinvoice.id =lrinvoicetax.invoiceid and lrinvoice.settled=false
                    where lrinvoicetax.taxid in ({0}) and lrinvoicetax.percentage=@taxpercentage 
                    and lrinvoice.isactive=true and lrinvoicetax.isactive=true
                    and lrinvoice.entrydate<@fromdate and lrinvoice.isinterstatesale=@isinterstatesale
                    and lrinvoice.companyid=@companyid";
                    if (item.companyid == 0)
                    {
                        purchasetaxquery = purchasetaxquery.Replace("and lrinvoice.companyid=@companyid", "");
                    }
                    command = GetCommand(string.Format(purchasetaxquery, String.Join(",", ptaxid)), conn);
                    command.Parameters.AddWithValue("fromdate", item.fromdate.Date);
                    command.Parameters.AddWithValue("isinterstatesale", isigst);
                    command.Parameters.AddWithValue("taxpercentage", taxpercentage);
                    command.Parameters.AddWithValue("companyid", item.companyid);
                    command.Parameters.AddWithValue("locationid", item.locationid);
                    using (System.Data.IDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var tax = reader[0] == DBNull.Value ? 0 : Convert.ToDecimal(reader[0]);
                            if (!isigst) tax = Math.Round(tax / 2, 2);
                            debit += tax;
                        }
                    }

                    string purchasereturntaxquery = @"
                    select sum(lrinvoicereturnitems.tax) from lrinvoicereturn
                    inner join lrinvoicereturnitems on lrinvoicereturnitems.lrinvoicereturnid = lrinvoicereturn.id
                    where lrinvoicereturn.isactive=true and lrinvoicereturnitems.isactive=true and
                    lrinvoicereturn.returndate<@fromdate 
					and lrinvoicereturnitems.taxid in ({0}) and lrinvoicereturnitems.taxpercentage=@taxpercentage 
                    and lrinvoicereturn.isinterstatesale=@isinterstatesale and lrinvoicereturn.companyid=@companyid";
                    if (item.companyid == 0)
                    {
                        purchasereturntaxquery = purchasereturntaxquery.Replace("and lrinvoicereturn.companyid=@companyid", "");
                    }
                    command = GetCommand(string.Format(purchasereturntaxquery, String.Join(",", ptaxid)), conn);
                    command.Parameters.AddWithValue("fromdate", item.fromdate.Date);
                    command.Parameters.AddWithValue("isinterstatesale", isigst);
                    command.Parameters.AddWithValue("taxpercentage", taxpercentage);
                    command.Parameters.AddWithValue("companyid", item.companyid);
                    command.Parameters.AddWithValue("locationid", item.locationid);
                    using (System.Data.IDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            credit += reader[0] == DBNull.Value ? 0 : Convert.ToDecimal(reader[0]);
                        }
                    }
                }

                if (ledger.taxmode == 6)
                {
                    string purchasetaxquery = @"select sum(supplierpayment.tdsamount)
                    from supplierpayment
                    inner join suppliercompany on suppliercompany.id = supplierpayment.suppliercompanyid
                    where supplierpayment.tdsledgerid=@tdsledgerid and tdsamount>0
					and tdsoninv=false
					and supplierpayment.isactive=true
                    and supplierpayment.companyid=@companyid
                    and supplierpayment.paymentdate<@fromdate";
                    if (item.companyid == 0)
                    {
                        purchasetaxquery = purchasetaxquery.Replace("and supplierpayment.companyid=@companyid", "");
                    }
                    command = GetCommand(purchasetaxquery, conn);
                    command.Parameters.AddWithValue("tdsledgerid", ledger.id);
                    command.Parameters.AddWithValue("companyid", item.companyid);
                    command.Parameters.AddWithValue("locationid", item.locationid);
                    command.Parameters.AddWithValue("fromdate", item.fromdate.Date);
                    command.Parameters.AddWithValue("todate", item.todate.Date.AddDays(1));
                    using (System.Data.IDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            credit += reader[0] == DBNull.Value ? 0 : Convert.ToDecimal(reader[0]);
                        }
                    }

                    purchasetaxquery = @"select sum(lrinvoice.tdsamount)
                    from lrinvoice
                    inner join suppliercompany on suppliercompany.id = lrinvoice.suppliercompanyid
                    where lrinvoice.tdsledgerid=@tdsledgerid and tdsamount>0 and lrinvoice.isactive=true
                    and lrinvoice.companyid=@companyid
                    and lrinvoice.entrydate<@fromdate";
                    if (item.companyid == 0)
                    {
                        purchasetaxquery = purchasetaxquery.Replace("and lrinvoice.companyid=@companyid", "");
                    }
                    command = GetCommand(purchasetaxquery, conn);
                    command.Parameters.AddWithValue("tdsledgerid", ledger.id);
                    command.Parameters.AddWithValue("fromdate", item.fromdate.Date);
                    command.Parameters.AddWithValue("todate", item.todate.Date.AddDays(1));
                    command.Parameters.AddWithValue("companyid", item.companyid);
                    command.Parameters.AddWithValue("locationid", item.locationid);
                    using (System.Data.IDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            credit += reader[0] == DBNull.Value ? 0 : Convert.ToDecimal(reader[0]);
                        }
                    }
                }

                if (iscash && brnchids != null && brnchids.Count > 0)
                {
                    //string cashclosingquery = @"select sum(closingamount) from cash
                    //where paidon<@fromdate and cash.isactive=true and cash.locationid in (" + String.Join(",", brnchids) + ")";
                    //command = GetCommand(cashclosingquery, conn);
                    //command.Parameters.AddWithValue("fromdate", item.fromdate.Date);
                    //command.Parameters.AddWithValue("companyid", item.companyid);
                    //command.Parameters.AddWithValue("locationid", item.locationid);
                    //using (var reader = command.ExecuteReader())
                    //{
                    //    if (reader.Read())
                    //    {
                    //        debit += reader[0] == DBNull.Value ? 0 : Convert.ToDecimal(reader[0]);
                    //    }
                    //}

                    string supplierpaymentquery = @"
                    select sum(paidamount) from supplierpayment
                    inner join supplier on supplier.id = supplierid 
                    where paymentmode = 2 and supplierpayment.companyid=@companyid
                    and paymentdate<@fromdate and supplierpayment.isactive=true  and supplierpayment.hascancel=false 
                    and supplierpayment.locationid in (" + String.Join(",", brnchids) + ")";
                    if (item.companyid == 0)
                    {
                        supplierpaymentquery = supplierpaymentquery.Replace("and supplierpayment.companyid=@companyid", "");
                    }
                    command = GetCommand(supplierpaymentquery, conn);
                    command.Parameters.AddWithValue("fromdate", item.fromdate.Date);
                    command.Parameters.AddWithValue("companyid", item.companyid);
                    command.Parameters.AddWithValue("locationid", item.locationid);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            credit += reader[0] == DBNull.Value ? 0 : Convert.ToDecimal(reader[0]);
                        }
                    }

                    string agentpaymentquery = @"
                    select sum(paidamount) from agentpayment
                    where paymentmode = 2 and agentpayment.companyid=@companyid
                    and paymentdate<@fromdate and agentpayment.isactive=true 
                    and agentpayment.locationid in (" + String.Join(",", brnchids) + ")";
                    if (item.companyid == 0)
                    {
                        agentpaymentquery = agentpaymentquery.Replace("and agentpayment.companyid=@companyid", "");
                    }
                    command = GetCommand(agentpaymentquery, conn);
                    command.Parameters.AddWithValue("fromdate", item.fromdate.Date);
                    command.Parameters.AddWithValue("companyid", item.companyid);
                    command.Parameters.AddWithValue("locationid", item.locationid);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            credit += reader[0] == DBNull.Value ? 0 : Convert.ToDecimal(reader[0]);
                        }
                    }

                    string transportpaymentquery = @"
                    select sum(paidamount) from transportpayment
                    where paymentmode = 2 and transportpayment.companyid=@companyid
                    and paymentdate<@fromdate and transportpayment.isactive=true 
                    and transportpayment.locationid in (" + String.Join(",", brnchids) + ")";
                    if (item.companyid == 0)
                    {
                        transportpaymentquery = transportpaymentquery.Replace("and transportpayment.companyid=@companyid", "");
                    }
                    command = GetCommand(transportpaymentquery, conn);
                    command.Parameters.AddWithValue("fromdate", item.fromdate.Date);
                    command.Parameters.AddWithValue("companyid", item.companyid);
                    command.Parameters.AddWithValue("locationid", item.locationid);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            credit += reader[0] == DBNull.Value ? 0 : Convert.ToDecimal(reader[0]);
                        }
                    }

                    string customerpaymentquery = @"select sum(customerpayment.amount) from customerpayment
                    where customerpayment.isactive=true and customerpayment.paidon<@fromdate 
                    and cast(paymentinfo->>'paymentmode' as integer) = 2 and customerpayment.companyid=@companyid
                    and customerpayment.locationid in (" + String.Join(",", brnchids) + ")";
                    if (item.companyid == 0)
                    {
                        customerpaymentquery = customerpaymentquery.Replace("and customerpayment.companyid=@companyid", "");
                    }
                    command = GetCommand(customerpaymentquery, conn);
                    command.Parameters.AddWithValue("fromdate", item.fromdate.Date);
                    command.Parameters.AddWithValue("companyid", item.companyid);
                    command.Parameters.AddWithValue("locationid", item.locationid);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            debit += reader[0] == DBNull.Value ? 0 : Convert.ToDecimal(reader[0]);
                        }
                    }


                    string customeradvancequery = @"
                    select sum(cast(schemelist->>'amount' as numeric))
                    from Customerorder
					LEFT JOIN LATERAL
					json_array_elements(Customerorder.paymentinfo :: json) schemelist ON true
                    where Customerorder.isactive=true and schemelist->>'mode'='3'
                    and cast(schemelist->>'actionon' as date)<@fromdate and Customerorder.companyid=@companyid
                    and Customerorder.locationid in (" + String.Join(",", brnchids) + ")";
                    if (item.companyid == 0)
                    {
                        customeradvancequery = customeradvancequery.Replace("and Customerorder.companyid=@companyid", "");
                    }
                    command = GetCommand(customeradvancequery, conn);
                    command.Parameters.AddWithValue("fromdate", item.fromdate.Date);
                    command.Parameters.AddWithValue("companyid", item.companyid);
                    command.Parameters.AddWithValue("locationid", item.locationid);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            debit += reader[0] == DBNull.Value ? 0 : Convert.ToDecimal(reader[0]);
                        }
                    }

                    string customervoucher = @"select sum(receivedamt) from coupons
                    where coupons.paymentmode = 3 
                    and coupons.isactive=true and coupons.issuedon<@fromdate
                    and coupons.issuemode = 0 and coupons.companyid=@companyid
                    and coupons.locationid in (" + String.Join(",", brnchids) + ")";
                    if (item.companyid == 0)
                    {
                        customervoucher = customervoucher.Replace("and coupons.companyid=@companyid", "");
                    }
                    command = GetCommand(customervoucher, conn);
                    command.Parameters.AddWithValue("fromdate", item.fromdate.Date);
                    command.Parameters.AddWithValue("companyid", item.companyid);
                    command.Parameters.AddWithValue("locationid", item.locationid);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            debit += reader[0] == DBNull.Value ? 0 : Convert.ToDecimal(reader[0]);
                        }
                    }

                    customervoucher = @"select sum(counteramount) from vouchers
                    inner join coupons on coupons.id = vouchers.coupenid
                    where vouchers.ipaymentmode = 3
                    and vouchers.isactive=true and vouchers.issuedon<@fromdate
                    and vouchers.status >= 0 and coupons.companyid=@companyid
                    and vouchers.issuelocationid in (" + String.Join(",", brnchids) + ")";
                    if (item.companyid == 0)
                    {
                        customervoucher = customervoucher.Replace("and coupons.companyid=@companyid", "");
                    }
                    command = GetCommand(customervoucher, conn);
                    command.Parameters.AddWithValue("fromdate", item.fromdate.Date);
                    command.Parameters.AddWithValue("companyid", item.companyid);
                    command.Parameters.AddWithValue("locationid", item.locationid);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            debit += reader[0] == DBNull.Value ? 0 : Convert.ToDecimal(reader[0]);
                        }
                    }

                }

                if (bankid > 0)
                {
                    string bankopening = @"select sum(paidamount)
                            from supplierpayment
                            where supplierpayment.isactive=true and supplierpayment.companyid=@companyid
                            and supplierpayment.paymentdate<@fromdate and supplierpayment.paidamount>0
                            and supplierpayment.bankid=@bankid and paymentmode != 2  and supplierpayment.hascancel=false ";
                    if (item.companyid == 0)
                    {
                        bankopening = bankopening.Replace("and supplierpayment.companyid=@companyid", "");
                    }
                    command = GetCommand(bankopening, conn);
                    command.Parameters.AddWithValue("fromdate", item.fromdate.Date);
                    command.Parameters.AddWithValue("bankid", bankid);
                    command.Parameters.AddWithValue("companyid", item.companyid);
                    command.Parameters.AddWithValue("locationid", item.locationid);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            credit += reader.IsDBNull(0) ? 0 : reader.GetDecimal(0);
                        }
                    }

                    bankopening = @"select sum(agentpayment.paidamount)
                            from agentpayment
                            where agentpayment.isactive=true and agentpayment.companyid=@companyid
                            and agentpayment.paymentdate<@fromdate and agentpayment.paidamount>0
                            and paymentmode != 2
                            and agentpayment.bankid=@bankid";
                    if (item.companyid == 0)
                    {
                        bankopening = bankopening.Replace("and agentpayment.companyid=@companyid", "");
                    }
                    command = GetCommand(bankopening, conn);
                    command.Parameters.AddWithValue("fromdate", item.fromdate.Date);
                    command.Parameters.AddWithValue("bankid", bankid);
                    command.Parameters.AddWithValue("companyid", item.companyid);
                    command.Parameters.AddWithValue("locationid", item.locationid);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            credit += reader.IsDBNull(0) ? 0 : reader.GetDecimal(0);
                        }
                    }


                    bankopening = @"select sum(transportpayment.paidamount)
                            from transportpayment
                            where transportpayment.isactive=true
                            and transportpayment.paymentdate<@fromdate and transportpayment.paidamount>0
                            and paymentmode != 2 and transportpayment.companyid=@companyid
                            and transportpayment.bankid=@bankid";
                    if (item.companyid == 0)
                    {
                        bankopening = bankopening.Replace("and transportpayment.companyid=@companyid", "");
                    }
                    command = GetCommand(bankopening, conn);
                    command.Parameters.AddWithValue("fromdate", item.fromdate.Date);
                    command.Parameters.AddWithValue("bankid", bankid);
                    command.Parameters.AddWithValue("companyid", item.companyid);
                    command.Parameters.AddWithValue("locationid", item.locationid);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            credit += reader.IsDBNull(0) ? 0 : reader.GetDecimal(0);
                        }
                    }

                    bankopening = @"
                    select sum(customerpayment.amount) 
                    from customerpayment
                    inner join customer on customer.id = customerpayment.customerid
                    where 
                    customerpayment.isactive=true and customerpayment.paidon<@fromdate
                    and cast(paymentinfo->>'paymentmode' as integer) != 2 and customerpayment.companyid=@companyid
                    and cast(paymentinfo->>'bankid' as bigint) = @bankid";
                    if (item.companyid == 0)
                    {
                        bankopening = bankopening.Replace("and customerpayment.companyid=@companyid", "");
                    }
                    command = GetCommand(bankopening, conn);
                    command.Parameters.AddWithValue("fromdate", item.fromdate.Date);
                    command.Parameters.AddWithValue("bankid", bankid);
                    command.Parameters.AddWithValue("companyid", item.companyid);
                    command.Parameters.AddWithValue("locationid", item.locationid);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            debit += reader.IsDBNull(0) ? 0 : reader.GetDecimal(0);
                        }
                    }


                    bankopening = @"
                    select sum(receivedamt) from coupons
                    inner join customer on customer.id = coupons.customerid
                    where  coupons.paymentmode != 3  and coupons.companyid=@companyid
                    and coupons.isactive=true and coupons.issuedon<@fromdate
                    and coupons.issuemode = 0 and coupons.bankid = @bankid";
                    if (item.companyid == 0)
                    {
                        bankopening = bankopening.Replace("and coupons.companyid=@companyid", "");
                    }
                    command = GetCommand(bankopening, conn);
                    command.Parameters.AddWithValue("fromdate", item.fromdate.Date);
                    command.Parameters.AddWithValue("bankid", bankid);
                    command.Parameters.AddWithValue("companyid", item.companyid);
                    command.Parameters.AddWithValue("locationid", item.locationid);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            debit += reader.IsDBNull(0) ? 0 : reader.GetDecimal(0);
                        }
                    }

                    bankopening = @"
                    select sum(counteramount) from vouchers
                    inner join coupons on coupons.id = vouchers.coupenid
                    inner join customer on customer.id = vouchers.issueto
                    where  vouchers.ipaymentmode != 3 and vouchers.ipaymentmode != 0
                    and vouchers.isactive=true and vouchers.issuedon<@fromdate
                    and vouchers.status >= 0 and vouchers.ibankid = @bankid
                    and coupons.companyid=@companyid";
                    if (item.companyid == 0)
                    {
                        bankopening = bankopening.Replace("and coupons.companyid=@companyid", "");
                    }

                    command = GetCommand(bankopening, conn);
                    command.Parameters.AddWithValue("fromdate", item.fromdate.Date);
                    command.Parameters.AddWithValue("bankid", bankid);
                    command.Parameters.AddWithValue("companyid", item.companyid);
                    command.Parameters.AddWithValue("locationid", item.locationid);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            debit += reader.IsDBNull(0) ? 0 : reader.GetDecimal(0);
                        }
                    }

                    bankopening = @"
                    select sum(cast(schemelist->>'amount' as numeric))
                    from Customerorder
					LEFT JOIN LATERAL
					json_array_elements(Customerorder.paymentinfo :: json) schemelist ON true
                    inner join customer on customer.id = Customerorder.customerid
                    where Customerorder.isactive=true and schemelist->>'mode' != '3'
                    and cast(schemelist->>'actionon' as date)<@fromdate and Customerorder.companyid=@companyid
                    and cast(schemelist->>'bankid' as bigint) = @bankid";
                    if (item.companyid == 0)
                    {
                        bankopening = bankopening.Replace("and Customerorder.companyid=@companyid", "");
                    }
                    command = GetCommand(bankopening, conn);
                    command.Parameters.AddWithValue("fromdate", item.fromdate.Date);
                    command.Parameters.AddWithValue("bankid", bankid);
                    command.Parameters.AddWithValue("companyid", item.companyid);
                    command.Parameters.AddWithValue("locationid", item.locationid);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            debit += reader.IsDBNull(0) ? 0 : reader.GetDecimal(0);
                        }
                    }

                    ReferencelistDataService referencelistDataService = new ReferencelistDataService()
                    {
                        Context = this.Context
                    };
                    var banks = referencelistDataService.Search(new ReferencelistCreteria()
                    {
                        id = bankid
                    });


                    if (banks[0].additionalinfo != null &&
                        banks[0].additionalinfo.bankinfo != null &&
                        banks[0].additionalinfo.bankinfo.ob != null)
                    {
                        var oldoblist = banks[0].additionalinfo.bankinfo.ob.FindAll(x => x.date < item.fromdate);
                        if (oldoblist.Count > 0)
                        {
                            debit += oldoblist.OrderBy(x => x.date).Last().value;
                        }
                    }

                    //bankopening = @"
                    //select sum(cast(paymentinfo->>'upi_paid' as numeric)) from billsettlementmaster 
                    //where billsettlementmaster.isactive=true 
                    //and billsettlementmaster.settlementon<@fromdate 
                    //and cast(paymentinfo->>'upi_paid' as numeric)>0
                    //and cast(paymentinfo->>'upiproviderid' as bigint) = @bankid";

                    //command = GetCommand(bankopening, conn);
                    //command.Parameters.AddWithValue("fromdate", item.fromdate.Date);
                    //command.Parameters.AddWithValue("bankid", bankid);
                    //using (var reader = command.ExecuteReader())
                    //{
                    //    if (reader.Read())
                    //    {
                    //        debit += reader.IsDBNull(0) ? 0 : reader.GetDecimal(0);
                    //    }
                    //}

                    //bankopening = @"
                    //select sum(paid_card) from billsettlementmaster 
                    //where billsettlementmaster.isactive=true 
                    //and billsettlementmaster.settlementon<@fromdate 
                    //and paid_card>0
                    //and cast(paymentinfo->>'card_bankid' as bigint) = @bankid";

                    //command = GetCommand(bankopening, conn);
                    //command.Parameters.AddWithValue("fromdate", item.fromdate.Date);
                    //command.Parameters.AddWithValue("bankid", bankid);
                    //using (var reader = command.ExecuteReader())
                    //{
                    //    if (reader.Read())
                    //    {
                    //        debit += reader.IsDBNull(0) ? 0 : reader.GetDecimal(0);
                    //    }
                    //}
                }

                command = GetCommand("select sum(receivable) receivable,sum(payable) payable from Voucheritembillwise where ledgerid=" + 
                    item.ledgerid + " and voucherid = 0 and isactive=true " +
                    (item.companyid>0 ? " and obcompanyid=" +item.companyid : ""), conn);

                bool hasbillwiseopening = false;
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        hasbillwiseopening = reader["payable"] != DBNull.Value || reader["receivable"] != DBNull.Value;
                        if (reader["payable"] != DBNull.Value)
                            credit += Convert.ToDecimal(reader["payable"]);
                        if (reader["receivable"] != DBNull.Value)
                            debit += Convert.ToDecimal(reader["receivable"]);
                    }
                }

                if (!hasbillwiseopening)
                {
                    command = GetCommand("select opening from ledger where id=" + item.ledgerid + " and isactive=true", conn);
                    using (System.Data.IDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            if (reader["opening"] != DBNull.Value)
                            {
                                if (Convert.ToDecimal(reader["opening"]) > 0)
                                    debit += Math.Abs(Convert.ToDecimal(reader["opening"]));
                                else
                                    credit += Math.Abs(Convert.ToDecimal(reader["opening"]));
                            }
                        }
                    }
                }

                //if (hasbillwiseopening)
                //{
                //    if (ledger.opening > 0)
                //        debit += ledger.opening;
                //    else
                //        credit += Math.Abs(ledger.opening);
                //}

                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null && conn.State == System.Data.ConnectionState.Open)
                    conn.Close();
            }

            if (credit > debit)
                return new Tuple<bool, decimal>(true, credit - debit);

            return new Tuple<bool, decimal>(false, debit - credit);
        }
        public class LedgerTypes
        {
            public long id { get; set; }
            public long typeid { get; set; }
            public string name { get; set; }
        }
        internal List<VoucherDisplayItem> SearchLedger(VoucherCreteria item)
        {
            var result = new List<VoucherDisplayItem>();
            string query = @"with ledgerhistory
as
(
select voucher.adjustmentledgerid ledgerid,voucher.id,0 sledgerid,voucher.voucherno,voucher.vouchertype,voucher.narration,
case when sourcetype=1 then -15 else voucher.vouchertypeid end vouchertypeid,
ledger.displayname,ledger.typeid,
voucheritems.iscredit iscredit,
voucher.adjustmentcredit primcrdr,
voucher.voucherdate,voucheritems.amount,
voucher.attributes->>'invoiceno' invoiceno,
voucher.attributes->>'invoicedate' invoicedate,
voucher.attributes->>'banktransfermode' banktransfermode,
voucher.attributes->>'bankreference' bankreference,
voucher.attributes->>'bankdate' bankdate,
false hasgst,	
false hastds,
sledger.displayname sourceledgername,
voucher.createdby,voucher.modifiedby,voucher.createdon,voucher.modifiedon
from voucher
inner join voucheritems on voucherid = voucher.id and voucheritems.isactive=true
inner join ledger on ledger.id = voucher.adjustmentledgerid
left outer join ledger sledger on sledger.id = sledgerid
where ledgerid = @ledgerid and voucher.locationid=@locationid and voucher.companyid=@companyid and voucher.isactive = true 
and coalesce(cast(voucheritems.attributes->>'lrinvoicereturnid' as bigint),0)=0
and voucher.voucherdate>=@fromdate and voucher.voucherdate<@todate
union all
select voucheritems.ledgerid,voucher.id,sledgerid,voucher.voucherno,voucher.vouchertype,voucher.narration,
case when sourcetype=1 then -15 else voucher.vouchertypeid end vouchertypeid,
ledger.displayname,ledger.typeid,
case 
when voucher.adjustmentcredit=voucheritems.iscredit and voucheritems.iscredit = true then false
when voucher.adjustmentcredit=voucheritems.iscredit and voucheritems.iscredit = false then true 
else voucher.adjustmentcredit end iscredit,
voucher.adjustmentcredit primcrdr,
voucher.voucherdate,voucheritems.amount,
voucher.attributes->>'invoiceno' invoiceno,
voucher.attributes->>'invoicedate' invoicedate,
voucher.attributes->>'banktransfermode' banktransfermode,
voucher.attributes->>'bankreference' bankreference,
voucher.attributes->>'bankdate' bankdate,
case when ledger.typeid=16 and ledger.taxmode in (3,4,5) then true else false end hasgst,	
case when ledger.typeid=16 and ledger.taxmode = 6 then true else false end hastds,
sledger.displayname sourceledgername,
voucher.createdby,voucher.modifiedby,voucher.createdon,voucher.modifiedon
from voucher 
inner join voucheritems on voucheritems.voucherid = voucher.id and voucheritems.isactive=true
inner join ledger on ledger.id = voucheritems.ledgerid
left outer join ledger sledger on sledger.id = sledgerid
where adjustmentledgerid=@ledgerid and voucher.locationid=@locationid and voucher.companyid=@companyid and voucher.isactive = true 
and coalesce(cast(voucher.attributes->>'lrinvoicereturnid' as bigint),0)=0
and voucher.voucherdate>=@fromdate and voucher.voucherdate<@todate
{0}
)
select ledgerhistory.*,referencelist.name vouchername from ledgerhistory
left outer join referencelist on referencelist.id = vouchertypeid
order by voucherdate,ledgerhistory.id";

            string tdsledgerquery = @"select voucheritems.ledgerid,voucher.id,sledgerid,voucher.voucherno,voucher.vouchertype,voucher.narration,
case when sourcetype=1 then -15 else voucher.vouchertypeid end vouchertypeid,
ledger.displayname,ledger.typeid,
voucheritems.iscredit,
voucher.adjustmentcredit primcrdr,
voucher.voucherdate,voucheritems.amount,
voucher.attributes->>'invoiceno' invoiceno,
voucher.attributes->>'invoicedate' invoicedate,
voucher.attributes->>'banktransfermode' banktransfermode,
voucher.attributes->>'bankreference' bankreference,
voucher.attributes->>'bankdate' bankdate,
case when ledger.typeid=16 and ledger.taxmode in (3,4,5) then true else false end hasgst,	
case when ledger.typeid=16 and ledger.taxmode = 6 then true else false end hastds,
sledger.displayname sourceledgername,
voucher.createdby,voucher.modifiedby,voucher.createdon,voucher.modifiedon
from voucher 
inner join voucheritems on voucheritems.voucherid = voucher.id
inner join ledger on ledger.id = voucheritems.ledgerid
left outer join ledger sledger on sledger.id = sledgerid
where sledgerid=@ledgerid and voucher.locationid=@locationid and voucher.companyid=@companyid and voucher.isactive = true 
and voucheritems.isactive = true and voucher.voucherdate>=@fromdate and voucher.voucherdate<@todate";


            LedgerDataService ledgerDataService = new LedgerDataService() { Context = this.Context };
            var ledger = ledgerDataService.Search(new LedgerCreteria()
            {
                id = item.ledgerid
            }).First();

            if (ledger.attributes != null &&
                ledger.attributes.hastds)
            {
                query = String.Format(query, " Union all " + tdsledgerquery);
            }
            else
            {
                query = String.Format(query, "");
            }
            if (item.companyid == 0)
            {
                query = query.Replace("and voucher.companyid=@companyid", "");
            }
            if (item.locationid == 0)
            {
                query = query.Replace("and voucher.locationid=@locationid", "");
            }
            if (item.fromdate.Date == DateTime.MinValue || item.todate.Date == DateTime.MinValue)
            {
                query = query.Replace("and voucher.voucherdate>=@fromdate and voucher.voucherdate<@todate", "");
            }

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


            List<LedgerTypes> ledgerTypes = JsonConvert.DeserializeObject<List<LedgerTypes>>(ledgername);



            long bankid = ledger.bankid;
            List<long> ptaxid = null;
            List<long> staxid = null;
            List<long> brnchids = null;
            bool iscash = ledger.typeid == 6 && ledger.eodcashledger;
            decimal taxpercentage = 0;
            bool isjobwork = ledger.tagid == 19;
            bool isigst = ledger.typeid == 16 && ledger.taxmode == 3;
            bool issupplier = ledger.typeid == 12 && ledger.attributes != null && ledger.attributes.issupplier && ledger.supplierid > 0;
            bool istransport = ledger.typeid == 12 && ledger.attributes != null && ledger.attributes.istransport && ledger.transportid > 0;
            bool iscustomer = ledger.typeid == 11 && ledger.attributes != null && ledger.attributes.iscustomer && ledger.customerid > 0;
            if (ledger.attributes != null && ledger.typeid == 16)
            {
                ptaxid = ledger.attributes.invptaxids;
                staxid = ledger.attributes.invstaxids;
                taxpercentage = ledger.taxpercentage;
                if (ledger.taxmode == 4 || ledger.taxmode == 5)
                    taxpercentage = taxpercentage * 2;
            }
            if (ledger.typeid == 6)
            {
                brnchids = ledger.attributes.branchids;
            }

            if (!issupplier)
            {
                query = query.Replace("and coalesce(cast(voucher.attributes->>'lrinvoicereturnid' as bigint),0)=0", "");
            }

            NpgsqlConnection conn = CreateConnection();
            List<Tuple<VoucherDisplayItem, List<VoucherDisplayItem>>> lst = new List<Tuple<VoucherDisplayItem, List<VoucherDisplayItem>>>();

            try
            {
                conn.Open();
                var command = GetCommand(query, conn);
                command.Parameters.AddWithValue("fromdate", item.fromdate.Date);
                command.Parameters.AddWithValue("todate", item.todate.Date.AddDays(1));
                command.Parameters.AddWithValue("locationid", item.locationid);
                command.Parameters.AddWithValue("ledgerid", item.ledgerid);
                command.Parameters.AddWithValue("companyid", item.companyid);
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string vouchername = reader["vouchername"] == DBNull.Value ? "" : reader["vouchername"].ToString();
                        string narration = reader["narration"] == DBNull.Value ? "" : reader["narration"].ToString();

                        if (string.IsNullOrEmpty(vouchername) && reader["vouchertypeid"] != DBNull.Value)
                        {
                            long vouchertypeid = Convert.ToInt32(reader["vouchertypeid"]);
                            if (ledgerTypes.Exists(x => x.id == vouchertypeid))
                            {
                                vouchername = ledgerTypes.Find(x => x.id == vouchertypeid).name;
                            }
                        }
                        if (string.IsNullOrEmpty(vouchername))
                        {
                            long vouchertype = Convert.ToInt32(reader["vouchertype"]) * -1;
                            if (ledgerTypes.Exists(x => x.id == vouchertype))
                            {
                                vouchername = ledgerTypes.Find(x => x.id == vouchertype).name;
                            }
                        }
                        var invoiceno = reader["invoiceno"] == DBNull.Value ? "" : Convert.ToString(reader["invoiceno"]);
                        var invoicedate = reader["invoicedate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["invoicedate"]);
                        var banktransfermode = reader["banktransfermode"] == DBNull.Value ? "" : Convert.ToString(reader["banktransfermode"]);
                        var bankreference = reader["bankreference"] == DBNull.Value ? "" : Convert.ToString(reader["bankreference"]);
                        var bankdate = reader["bankdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["bankdate"]);
                        var sourceledgername = reader["sourceledgername"] == DBNull.Value ? "" : Convert.ToString(reader["sourceledgername"]);
                        var iscredit = reader["iscredit"] == DBNull.Value ? false : Convert.ToBoolean(reader["iscredit"]);
                        var primcrdr = reader["primcrdr"] == DBNull.Value ? false : Convert.ToBoolean(reader["primcrdr"]);

                        var hasgst = reader["hasgst"] == DBNull.Value ? false : Convert.ToBoolean(reader["hasgst"]);
                        var hastds = reader["hastds"] == DBNull.Value ? false : Convert.ToBoolean(reader["hastds"]);

                        var voucheritem = new VoucherDisplayItem()
                        {
                            ledgerid = Convert.ToInt64(reader["ledgerid"]),
                            sledgerid = reader["sledgerid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sledgerid"]),
                            id = Convert.ToInt64(reader["id"]),
                            no = reader["voucherno"].ToString(),
                            createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]),
                            modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]),
                            modifiedon = reader["modifiedon"] == DBNull.Value ? Convert.ToDateTime(reader["voucherdate"]) : Convert.ToDateTime(reader["modifiedon"]),
                            createdon = reader["createdon"] == DBNull.Value ? Convert.ToDateTime(reader["voucherdate"]) : Convert.ToDateTime(reader["createdon"]),
                            adjno = "",
                            hasgst = hasgst,
                            hastds = hastds,
                            date = Convert.ToDateTime(reader["voucherdate"]),
                            amount = Convert.ToDecimal(reader["amount"]),
                            crdr = Convert.ToBoolean(reader["iscredit"]) ? "Cr" : "Dr",
                            primcrdr = Convert.ToBoolean(reader["primcrdr"]) ? "Cr" : "Dr",
                            ledger = (ledger.typeid == 16 && Convert.ToInt32(reader["vouchertype"]) == 1
                            && !string.IsNullOrEmpty(sourceledgername)) ? sourceledgername : Convert.ToString(reader["displayname"]),
                            sourceledgername = sourceledgername,
                            ledgertype = Convert.ToInt32(reader["typeid"]),
                            vouchername = vouchername,
                            vouchertype = Convert.ToInt32(reader["vouchertype"]) * -1,
                            vouchertypeid = reader["vouchertypeid"] == DBNull.Value ? (Convert.ToInt32(reader["vouchertype"]) * -1) : Convert.ToInt64(reader["vouchertypeid"]),
                            isheader = true,
                            invoiceno = invoiceno,
                            invoicedate = invoicedate,
                            banktransfermode = banktransfermode,
                            bankreference = bankreference,
                            bankdate = bankdate,
                            isvoucher = true,
                            narration = narration
                        };

                        //if (iscredit == primcrdr)
                        //    voucheritem.crdr = voucheritem.crdr == "Dr" ? "Cr" : "Dr";
                        //voucheritem.amount = voucheritem.amount * -1;

                        //if (result.Exists(x => x.id == voucheritem.id))
                        //{
                        //    var olditem = result.Find(x => x.id == voucheritem.id);
                        //    if (olditem.amount > voucheritem.amount)
                        //    {
                        //        olditem.amount += voucheritem.amount;
                        //    }
                        //    else
                        //    {
                        //        voucheritem.amount += olditem.amount;
                        //        result.Remove(olditem);
                        //        result.Add(voucheritem);
                        //    }
                        //}
                        //else
                        {
                            result.Add(voucheritem);
                        }
                    }
                }


                if (item.onvoucherledger)
                {
                    var finopening = SearchOpening(item, 0, 0);
                    if (finopening.Item1)
                    {
                        result.Insert(0, new VoucherDisplayItem()
                        {
                            id = -1,
                            no = "OB",
                            date = item.fromdate,
                            amount = finopening.Item2,
                            crdr = "Cr",
                            ledger = "Opening Balance",
                            vouchertype = -1,
                            vouchertypeid = -1,
                            isheader = true
                        });
                    }
                    else
                    {
                        result.Insert(0, new VoucherDisplayItem()
                        {
                            id = -1,
                            no = "OB",
                            date = item.fromdate,
                            amount = finopening.Item2,
                            crdr = "Dr",
                            ledger = "Opening Balance",
                            vouchertype = -1,
                            vouchertypeid = -1,
                            isheader = true
                        });
                    }
                    return result;
                }

                if (bankid != 0 || iscash)
                {
                    var alltds = result.FindAll(x => x.ledgertype == 16 && x.hastds && x.crdr == "Dr" && x.vouchername != "Journal CR");
                    foreach (var tdsledger in alltds)
                    {
                        var sourceledger = result.FindAll(x => x.id == tdsledger.id && x.crdr == "Cr").OrderByDescending(x => x.amount).ToList();
                        if (sourceledger.Count > 0)
                        {
                            sourceledger[0].amount -= tdsledger.amount;
                            result.Remove(tdsledger);
                        }
                    }
                }
                else if (result.Exists(x => x.ledgertype == 16 && x.crdr == x.primcrdr && !x.hasgst))
                {
                    var alltds = result.FindAll(x => x.ledgertype == 16 && x.crdr == x.primcrdr && !x.hasgst && x.hastds && x.vouchername != "Journal CR");
                    foreach (var tds in alltds)
                    {
                        var alternateledgers = result.FindAll(x => x.id == tds.id && x.ledgerid != tds.ledgerid && x.ledgertype != 16);
                        if (alternateledgers.Count == 1 &&
                            alternateledgers[0].primcrdr == tds.primcrdr)
                        {
                            alternateledgers[0].amount -= tds.amount;
                            tds.crdr = tds.crdr == "Dr" ? "Cr" : "Dr";
                        }
                    }
                }

                if (ledger.typeid != 16 &&
                    (result.Exists(x => x.vouchertype == -1) || (ledger.typeid == 11 || ledger.typeid == 12)))
                {
                    VoucheritembillwiseDataService voucheritembillwiseDataService = new VoucheritembillwiseDataService() { Context = this.Context };

                    var paidvouchers = voucheritembillwiseDataService.Search(new VoucheritembillwiseCreteria()
                    {
                        voucherids = result.FindAll(x => x.vouchertype == -1).ConvertAll(x => x.id),
                        ledgerid = (ledger.typeid == 11 || ledger.typeid == 12) ? item.ledgerid : 0
                    });
                    if (paidvouchers.Count > 0)
                    {
                        var paidadjsvouchers = voucheritembillwiseDataService.Search(new VoucheritembillwiseCreteria()
                        {
                            ids = paidvouchers.ConvertAll(x => x.sourceitemid)
                        });

                        foreach (var paidbillwise in paidvouchers.GroupBy(x => new { x.voucherid }))
                        {
                            var sourceids = paidbillwise.ToList().ConvertAll(x => x.sourceitemid);
                            var adjvouchers = paidadjsvouchers.FindAll(x => sourceids.Contains(x.id));
                            if (adjvouchers.Count > 0)
                            {
                                var source = result.FindAll(x => x.id == paidbillwise.Key.voucherid).OrderByDescending(y => y.amount);
                                if (source.Count() > 0)
                                {
                                    source.First().adjno = String.Join(",", adjvouchers.ConvertAll(x => x.referenceno));
                                }
                            }
                        }
                    }
                }

                if (staxid != null && staxid.Count > 0)
                {
                    string salestaxquery = @"select
                    bill.billdate::date,
                    sum(billitems.tax) tax,
                    sum(billitems.receivable) receivable,
                    customer.name,
                    case when customer.id is null then '' else bill.billno end receiptno,
                    bill.createdby,bill.modifiedby,bill.createdon,bill.modifiedon
                    from billitems
                    inner join bill on bill.id = billid
                    left outer join customer on customer.id = bill.customerid
                    inner join tax on tax.id = billitems.taxid
                    where bill.isactive=true and bill.billtype != 9 and bill.locationid=@locationid and bill.companyid=@companyid 
                    and bill.billdate>=@fromdate and bill.billdate<@todate and billitems.isactive=true
                    and billitems.taxid in ({0}) and billitems.taxpercentage=@taxpercentage and bill.isinterstatesale = @isinterstatesale
                    group by bill.billdate::date,
                    customer.name,case when customer.id is null then '' else bill.billno end,
                    bill.createdby,bill.modifiedby,bill.createdon,bill.modifiedon";


                    if (item.locationid == 0)
                    {
                        salestaxquery = salestaxquery.Replace("and bill.locationid=@locationid", "");
                    }
                    if (item.companyid == 0)
                    {
                        salestaxquery = salestaxquery.Replace("and bill.companyid=@companyid", "");
                    }

                    command = GetCommand(string.Format(salestaxquery, String.Join(",", staxid)), conn);
                    command.Parameters.AddWithValue("fromdate", item.fromdate.Date);
                    command.Parameters.AddWithValue("todate", item.todate.Date.AddDays(1));
                    command.Parameters.AddWithValue("isinterstatesale", isigst);
                    command.Parameters.AddWithValue("taxpercentage", taxpercentage);
                    command.Parameters.AddWithValue("companyid", item.companyid);
                    command.Parameters.AddWithValue("locationid", item.locationid);
                    var fromid = result.Count == 0 ? 0 : result.Max(x => x.id);
                    command.CommandTimeout = 1000;
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            fromid++;
                            DateTime paymentdate = reader["billdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["billdate"]);
                            decimal tax = reader["tax"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["tax"]);
                            decimal receivable = reader["receivable"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["receivable"]);
                            string name = reader["name"] == DBNull.Value ? "" : Convert.ToString(reader["name"]);
                            string receiptno = reader["receiptno"] == DBNull.Value ? "" : Convert.ToString(reader["receiptno"]);

                            result.Add(new VoucherDisplayItem()
                            {
                                createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]),
                                modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]),
                                modifiedon = reader["modifiedon"] == DBNull.Value ? Convert.ToDateTime(reader["billdate"]) : Convert.ToDateTime(reader["modifiedon"]),
                                createdon = reader["createdon"] == DBNull.Value ? Convert.ToDateTime(reader["billdate"]) : Convert.ToDateTime(reader["createdon"]),
                                id = fromid,
                                invoiceno = receiptno,
                                date = paymentdate,
                                amount = Math.Abs(!isigst ? Math.Round(tax / 2, 2) : tax),
                                crdr = tax > 0 ? "Cr" : "Dr",
                                ledger = name,
                                ledgertype = 16,
                                narration = !String.IsNullOrEmpty(receiptno) ? "" : (tax > 0 ? "Cash Sales" : "Cash Sales Return"),
                                vouchername = tax > 0 ? "Inventory Sales" : "Inventory Sales Return",
                                vouchertype = -1,
                                vouchertypeid = -1,
                                isheader = true
                            }); ;
                        }
                    }
                }


                if (ptaxid != null && ptaxid.Count > 0)
                {
                    string purchasetaxquery = @"select lrinvoice.isinterstatesale,
                    lrinvoice.invoiceno receiptno,lrinvoice.entrydate billdate,suppliercompany.name,
                    lrinvoice.createdby,lrinvoice.modifiedby,lrinvoice.createdon,lrinvoice.modifiedon,
                    sum(lrinvoicetax.taxamount) tax
                    from lrinvoicetax
                    inner join lrinvoice on lrinvoice.id =lrinvoicetax.invoiceid and lrinvoice.settled=false
                    inner join suppliercompany on suppliercompany.id = lrinvoice.suppliercompanyid
                    where lrinvoicetax.taxid in ({0}) and lrinvoicetax.percentage=@taxpercentage and lrinvoice.isactive=true and lrinvoicetax.isactive=true
                    and lrinvoice.entrydate>=@fromdate and lrinvoice.entrydate<@todate and lrinvoice.companyid=@companyid and lrinvoice.isinterstatesale=@isinterstatesale
                    group by lrinvoice.isinterstatesale,
                    lrinvoice.invoiceno,lrinvoice.entrydate ,suppliercompany.name,
                    lrinvoice.createdby,lrinvoice.modifiedby,lrinvoice.createdon,lrinvoice.modifiedon";

                    if (item.companyid == 0)
                    {
                        purchasetaxquery = purchasetaxquery.Replace("and lrinvoice.companyid=@companyid", "");
                    }

                    command = GetCommand(string.Format(purchasetaxquery, String.Join(",", ptaxid)), conn);
                    command.Parameters.AddWithValue("fromdate", item.fromdate.Date);
                    command.Parameters.AddWithValue("todate", item.todate.Date.AddDays(1));
                    command.Parameters.AddWithValue("isinterstatesale", isigst);
                    command.Parameters.AddWithValue("taxpercentage", taxpercentage);
                    command.Parameters.AddWithValue("companyid", item.companyid);
                    var fromid = result.Count == 0 ? 0 : result.Max(x => x.id);
                    command.CommandTimeout = 1000;
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            fromid++;
                            DateTime paymentdate = reader["billdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["billdate"]);
                            decimal tax = reader["tax"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["tax"]);
                            if (!isigst) tax = Math.Round(tax / 2, 2);
                            string name = reader["name"] == DBNull.Value ? "" : Convert.ToString(reader["name"]);
                            bool isinterstatesale = reader["isinterstatesale"] == DBNull.Value ? false : Convert.ToBoolean(reader["isinterstatesale"]);
                            string receiptno = reader["receiptno"] == DBNull.Value ? "" : Convert.ToString(reader["receiptno"]);
                            result.Add(new VoucherDisplayItem()
                            {
                                createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]),
                                modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]),
                                modifiedon = reader["modifiedon"] == DBNull.Value ? Convert.ToDateTime(reader["billdate"]) : Convert.ToDateTime(reader["modifiedon"]),
                                createdon = reader["createdon"] == DBNull.Value ? Convert.ToDateTime(reader["billdate"]) : Convert.ToDateTime(reader["createdon"]),
                                id = fromid,
                                invoiceno = receiptno,
                                date = paymentdate,
                                amount = tax,
                                crdr = "Dr",
                                ledger = name,
                                ledgertype = 16,
                                vouchername = "Inventory Purchase",
                                vouchertype = -1,
                                vouchertypeid = -1,
                                isheader = true
                            });
                        }
                    }

                    string purchasereturntaxquery = @"
                    select lrinvoicereturn.isinterstatesale,
                    lrinvoicereturn.returnno,lrinvoicereturn.returndate,
                    sum(lrinvoicereturnitems.tax) tax,suppliercompany.name,
                    lrinvoicereturn.createdby,lrinvoicereturn.modifiedby,lrinvoicereturn.createdon,lrinvoicereturn.modifiedon
                    from lrinvoicereturn
                    inner join lrinvoicereturnitems on lrinvoicereturnitems.lrinvoicereturnid = lrinvoicereturn.id
                    inner join suppliercompany on suppliercompany.id = lrinvoicereturn.suppliercompanyid
                    where lrinvoicereturn.isactive=true and lrinvoicereturnitems.isactive=true and
                    lrinvoicereturn.returndate>=@fromdate and lrinvoicereturn.returndate<@todate and lrinvoicereturn.companyid=@companyid 
					and lrinvoicereturnitems.taxid in ({0}) and lrinvoicereturnitems.taxpercentage=@taxpercentage 
                    and lrinvoicereturn.isinterstatesale=@isinterstatesale
                    group by lrinvoicereturn.isinterstatesale,
                    lrinvoicereturn.returnno,lrinvoicereturn.returndate,suppliercompany.name,
                    lrinvoicereturn.createdby,lrinvoicereturn.modifiedby,lrinvoicereturn.createdon,lrinvoicereturn.modifiedon";


                    if (item.companyid == 0)
                    {
                        purchasereturntaxquery = purchasereturntaxquery.Replace("and lrinvoicereturn.companyid=@companyid", "");
                    }


                    command = GetCommand(string.Format(purchasereturntaxquery, String.Join(",", ptaxid)), conn);
                    command.Parameters.AddWithValue("fromdate", item.fromdate.Date);
                    command.Parameters.AddWithValue("todate", item.todate.Date.AddDays(1));
                    command.Parameters.AddWithValue("isinterstatesale", isigst);
                    command.Parameters.AddWithValue("taxpercentage", taxpercentage);
                    command.Parameters.AddWithValue("companyid", item.companyid);
                    command.CommandTimeout = 1000;
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            fromid++;
                            DateTime paymentdate = reader["returndate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["returndate"]);
                            decimal tax = reader["tax"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["tax"]);
                            if (!isigst) tax = Math.Round(tax / 2, 2);
                            string name = reader["name"] == DBNull.Value ? "" : Convert.ToString(reader["name"]);
                            bool isinterstatesale = reader["isinterstatesale"] == DBNull.Value ? false : Convert.ToBoolean(reader["isinterstatesale"]);
                            string receiptno = reader["returnno"] == DBNull.Value ? "" : Convert.ToString(reader["returnno"]);
                            result.Add(new VoucherDisplayItem()
                            {
                                createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]),
                                modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]),
                                modifiedon = reader["modifiedon"] == DBNull.Value ? Convert.ToDateTime(reader["billdate"]) : Convert.ToDateTime(reader["modifiedon"]),
                                createdon = reader["createdon"] == DBNull.Value ? Convert.ToDateTime(reader["billdate"]) : Convert.ToDateTime(reader["createdon"]),
                                id = fromid,
                                invoiceno = receiptno,
                                date = paymentdate,
                                amount = tax,
                                crdr = "Cr",
                                ledger = name,
                                ledgertype = 16,
                                vouchername = "Inventory Debit Note",
                                vouchertype = -1,
                                vouchertypeid = -1,
                                isheader = true
                            }); ;
                        }
                    }
                }

                if (ledger.taxmode == 6)
                {
                    string purchasetaxquery = @"select supplierpayment.tdsamount tax,
                    supplierpayment.receiptno,supplierpayment.paymentdate billdate,suppliercompany.name,
                    supplierpayment.createdby,supplierpayment.modifiedby,supplierpayment.createdon,
					supplierpayment.modifiedon
                    from supplierpayment
                    inner join suppliercompany on suppliercompany.id = supplierpayment.suppliercompanyid
                    where supplierpayment.tdsledgerid=@tdsledgerid and tdsamount>0
					and tdsoninv=false and supplierpayment.companyid=@companyid
					and supplierpayment.isactive=true
                    and supplierpayment.paymentdate>=@fromdate and supplierpayment.paymentdate<@todate";


                    if (item.companyid == 0)
                    {
                        purchasetaxquery = purchasetaxquery.Replace("and supplierpayment.companyid=@companyid", "");
                    }

                    command = GetCommand(purchasetaxquery, conn);
                    command.Parameters.AddWithValue("tdsledgerid", ledger.id);
                    command.Parameters.AddWithValue("fromdate", item.fromdate.Date);
                    command.Parameters.AddWithValue("todate", item.todate.Date.AddDays(1));
                    command.Parameters.AddWithValue("companyid", item.companyid);
                    command.CommandTimeout = 1000;
                    var fromid = result.Count == 0 ? 0 : result.Max(x => x.id);
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            fromid++;
                            DateTime paymentdate = reader["billdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["billdate"]);
                            decimal tax = reader["tax"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["tax"]);
                            string name = reader["name"] == DBNull.Value ? "" : Convert.ToString(reader["name"]);
                            string receiptno = reader["receiptno"] == DBNull.Value ? "" : Convert.ToString(reader["receiptno"]);
                            result.Add(new VoucherDisplayItem()
                            {
                                createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]),
                                modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]),
                                modifiedon = reader["modifiedon"] == DBNull.Value ? Convert.ToDateTime(reader["billdate"]) : Convert.ToDateTime(reader["modifiedon"]),
                                createdon = reader["createdon"] == DBNull.Value ? Convert.ToDateTime(reader["billdate"]) : Convert.ToDateTime(reader["createdon"]),
                                id = fromid,
                                invoiceno = receiptno,
                                date = paymentdate,
                                amount = tax,
                                crdr = "Cr",
                                ledger = name,
                                ledgertype = 16,
                                vouchername = "TDS On Payment",
                                vouchertype = -1,
                                vouchertypeid = -1,
                                isheader = true
                            });
                        }
                    }

                    purchasetaxquery = @"select lrinvoice.tdsamount tax,
                    lrinvoice.invoiceno receiptno,lrinvoice.entrydate billdate,suppliercompany.name,
                    lrinvoice.createdby,lrinvoice.modifiedby,lrinvoice.createdon,lrinvoice.modifiedon
                    from lrinvoice
                    inner join suppliercompany on suppliercompany.id = lrinvoice.suppliercompanyid
                    where lrinvoice.tdsledgerid=@tdsledgerid and tdsamount>0 and lrinvoice.isactive=true
                    and lrinvoice.entrydate>=@fromdate and lrinvoice.entrydate<@todate and lrinvoice.companyid=@companyid";

                    if (item.companyid == 0)
                    {
                        purchasetaxquery = purchasetaxquery.Replace("and lrinvoice.companyid=@companyid", "");
                    }

                    command = GetCommand(purchasetaxquery, conn);
                    command.Parameters.AddWithValue("tdsledgerid", ledger.id);
                    command.Parameters.AddWithValue("fromdate", item.fromdate.Date);
                    command.Parameters.AddWithValue("todate", item.todate.Date.AddDays(1));
                    command.Parameters.AddWithValue("companyid", item.companyid);
                    command.CommandTimeout = 1000;
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            fromid++;
                            DateTime paymentdate = reader["billdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["billdate"]);
                            decimal tax = reader["tax"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["tax"]);
                            string name = reader["name"] == DBNull.Value ? "" : Convert.ToString(reader["name"]);
                            string receiptno = reader["receiptno"] == DBNull.Value ? "" : Convert.ToString(reader["receiptno"]);
                            result.Add(new VoucherDisplayItem()
                            {
                                createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]),
                                modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]),
                                modifiedon = reader["modifiedon"] == DBNull.Value ? Convert.ToDateTime(reader["billdate"]) : Convert.ToDateTime(reader["modifiedon"]),
                                createdon = reader["createdon"] == DBNull.Value ? Convert.ToDateTime(reader["billdate"]) : Convert.ToDateTime(reader["createdon"]),
                                id = fromid,
                                invoiceno = receiptno,
                                date = paymentdate,
                                amount = tax,
                                crdr = "Cr",
                                ledger = name,
                                ledgertype = 16,
                                vouchername = "TDS On Inventory",
                                vouchertype = -1,
                                vouchertypeid = -1,
                                isheader = true
                            });
                        }
                    }

                }
                if (bankid > 0)
                {
                    string supplierpaymentquery = @"
                            select supplierpayment.receiptno,paymentdate,suppliercompany.name,
                            supplierpayment.paidamount,'Supplier Payment' type,
                            supplierpayment.createdby,supplierpayment.modifiedby,supplierpayment.createdon,
                            supplierpayment.modifiedon,supplierpayment.referencedetail chequeno,
                            supplierpayment.instrumentdate
                            from supplierpayment
                            inner join suppliercompany on suppliercompany.id = supplierpayment.suppliercompanyid
                            where supplierpayment.isactive=true and supplierpayment.hascancel=false 
                            and supplierpayment.paymentdate>=@fromdate and supplierpayment.paymentdate<@todate 
                            and supplierpayment.bankid=@bankid and supplierpayment.paidamount>0 and paymentmode !=2
                            and supplierpayment.companyid=@companyid
                            union all 
                            select transportpayment.receiptno,paymentdate,transport.name,
                            transportpayment.paidamount,'Transport Payment' type,
                            transportpayment.createdby,transportpayment.modifiedby,transportpayment.createdon,
                            transportpayment.modifiedon,transportpayment.referencedetail chequeno,
                            transportpayment.instrumentdate
                            from transportpayment
                            inner join transport on transport.id = transportpayment.transportid
                            where transportpayment.isactive=true                            
                            and transportpayment.paymentdate>=@fromdate and transportpayment.paymentdate<@todate 
                            and transportpayment.bankid=@bankid and transportpayment.paidamount>0 and paymentmode !=2
                            and transportpayment.companyid=@companyid
                            union all 
                            select agentpayment.receiptno,paymentdate,agent.name,
                            agentpayment.paidamount,'Agent Payment' type,
                            agentpayment.createdby,agentpayment.modifiedby,agentpayment.createdon,
                            agentpayment.modifiedon,agentpayment.referencedetail chequeno,
                            agentpayment.instrumentdate
                            from agentpayment
                            inner join agent on agent.id = agentpayment.agentid
                            where agentpayment.isactive=true                            
                            and agentpayment.paymentdate>=@fromdate and agentpayment.paymentdate<@todate 
                            and agentpayment.bankid=@bankid and agentpayment.paidamount>0 and paymentmode !=2
                            and agentpayment.companyid=@companyid";

                    if (item.companyid == 0)
                    {
                        supplierpaymentquery = supplierpaymentquery.Replace("and supplierpayment.companyid=@companyid", "");
                        supplierpaymentquery = supplierpaymentquery.Replace("and transportpayment.companyid=@companyid", "");
                        supplierpaymentquery = supplierpaymentquery.Replace("and agentpayment.companyid=@companyid", "");
                    }


                    command = GetCommand(supplierpaymentquery, conn);
                    command.Parameters.AddWithValue("fromdate", item.fromdate.Date);
                    command.Parameters.AddWithValue("todate", item.todate.Date.AddDays(1));
                    command.Parameters.AddWithValue("bankid", bankid);
                    command.Parameters.AddWithValue("companyid", item.companyid);
                    command.CommandTimeout = 1000;
                    var fromid = result.Count == 0 ? 0 : result.Max(x => x.id);
                    using (var reader = command.ExecuteReader())

                    {
                        while (reader.Read())
                        {
                            fromid++;
                            DateTime paymentdate = reader["paymentdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["paymentdate"]);
                            decimal paidamount = reader["paidamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["paidamount"]);
                            string name = reader["name"] == DBNull.Value ? "" : Convert.ToString(reader["name"]);
                            string type = reader["type"] == DBNull.Value ? "" : Convert.ToString(reader["type"]);
                            string receiptno = reader["receiptno"] == DBNull.Value ? "" : Convert.ToString(reader["receiptno"]);
                            result.Add(new VoucherDisplayItem()
                            {
                                createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]),
                                modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]),
                                modifiedon = reader["modifiedon"] == DBNull.Value ? Convert.ToDateTime(reader["paymentdate"]) : Convert.ToDateTime(reader["modifiedon"]),
                                createdon = reader["createdon"] == DBNull.Value ? Convert.ToDateTime(reader["paymentdate"]) : Convert.ToDateTime(reader["createdon"]),
                                id = fromid,
                                no = receiptno,
                                date = paymentdate,
                                amount = paidamount,
                                bankreference = reader["chequeno"] == DBNull.Value ? "" : Convert.ToString(reader["chequeno"]),
                                bankdate = reader["instrumentdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["instrumentdate"]),
                                crdr = "Cr",
                                ledger = name,
                                ledgertype = 12,
                                vouchername = type,
                                vouchertype = -1,
                                vouchertypeid = -1,
                                isheader = true
                            });
                        }
                    }


                    string customerpaymentquery = @"select customer.name,customerpayment.amount,customerpayment.receiptno,
                    customerpayment.paidon, customerpayment.createdby,customerpayment.modifiedby,customerpayment.createdon,
                    customerpayment.modifiedon,paymentinfo->>'remarks' chequeno,paymentinfo->>'chequedate' instrumentdate
                    from customerpayment
                    inner join customer on customer.id = customerpayment.customerid
                    where 
                    customerpayment.isactive=true and customerpayment.paidon>=@fromdate
                    and customerpayment.paidon<@todate and customerpayment.companyid=@companyid
                    and cast(paymentinfo->>'paymentmode' as integer) != 2
                    and cast(paymentinfo->>'bankid' as bigint) = @bankid";


                    if (item.companyid == 0)
                    {
                        customerpaymentquery = customerpaymentquery.Replace("and customerpayment.companyid=@companyid", "");
                    }

                    command = GetCommand(customerpaymentquery, conn);
                    command.Parameters.AddWithValue("fromdate", item.fromdate.Date);
                    command.Parameters.AddWithValue("todate", item.todate.Date.AddDays(1));
                    command.Parameters.AddWithValue("bankid", bankid);
                    command.Parameters.AddWithValue("companyid", item.companyid);
                    command.CommandTimeout = 1000;
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            fromid++;
                            DateTime paymentdate = reader["paidon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["paidon"]);
                            decimal paidamount = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"]);
                            string name = reader["name"] == DBNull.Value ? "" : Convert.ToString(reader["name"]);
                            string receiptno = reader["receiptno"] == DBNull.Value ? "" : Convert.ToString(reader["receiptno"]);
                            result.Add(new VoucherDisplayItem()
                            {
                                createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]),
                                modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]),
                                modifiedon = reader["modifiedon"] == DBNull.Value ? Convert.ToDateTime(reader["paidon"]) : Convert.ToDateTime(reader["modifiedon"]),
                                createdon = reader["createdon"] == DBNull.Value ? Convert.ToDateTime(reader["paidon"]) : Convert.ToDateTime(reader["createdon"]),
                                id = fromid,
                                no = receiptno,
                                date = paymentdate,
                                amount = paidamount,
                                bankreference = reader["chequeno"] == DBNull.Value ? "" : Convert.ToString(reader["chequeno"]),
                                bankdate = reader["instrumentdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["instrumentdate"]),
                                crdr = "Dr",
                                ledger = name,
                                ledgertype = 12,
                                vouchername = "Customer Payment Received",
                                vouchertype = -1,
                                vouchertypeid = -1,
                                isheader = true
                            });
                        }
                    }

                    string customervoucher = @"select receivedamt,issuedon,customer.name,seqcode,
                    coupons.createdby,coupons.modifiedby,coupons.createdon,
                    coupons.modifiedon from coupons
                    inner join customer on customer.id = coupons.customerid
                    where  coupons.paymentmode != 3 and coupons.companyid=@companyid
                    and coupons.isactive=true and coupons.issuedon>=@fromdate
                    and coupons.issuedon<@todate  
                    and coupons.issuemode = 0 and coupons.bankid = @bankid";
                    if (item.companyid == 0)
                    {
                        customervoucher = customervoucher.Replace("and coupons.companyid=@companyid", "");
                    }

                    command = GetCommand(customervoucher, conn);
                    command.Parameters.AddWithValue("fromdate", item.fromdate.Date);
                    command.Parameters.AddWithValue("todate", item.todate.Date.AddDays(1));
                    command.Parameters.AddWithValue("bankid", bankid);
                    command.Parameters.AddWithValue("companyid", item.companyid);
                    command.CommandTimeout = 1000;
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            fromid++;
                            DateTime paymentdate = reader["issuedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["issuedon"]);
                            decimal paidamount = reader["receivedamt"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["receivedamt"]);
                            string name = reader["name"] == DBNull.Value ? "" : Convert.ToString(reader["name"]);
                            string receiptno = reader["seqcode"] == DBNull.Value ? "" : Convert.ToString(reader["seqcode"]);
                            result.Add(new VoucherDisplayItem()
                            {
                                createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]),
                                modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]),
                                modifiedon = reader["modifiedon"] == DBNull.Value ? Convert.ToDateTime(reader["issuedon"]) : Convert.ToDateTime(reader["modifiedon"]),
                                createdon = reader["createdon"] == DBNull.Value ? Convert.ToDateTime(reader["issuedon"]) : Convert.ToDateTime(reader["createdon"]),
                                id = fromid,
                                no = receiptno,
                                date = paymentdate,
                                amount = paidamount,
                                crdr = "Dr",
                                ledger = name,
                                ledgertype = 12,
                                vouchername = "Voucher Sold - Direct",
                                vouchertype = -1,
                                vouchertypeid = -1,
                                isheader = true
                            });
                        }
                    }

                    customervoucher = @"select counteramount,vouchers.issuedon,customer.name,seqcode,
                    vouchers.createdby,vouchers.modifiedby,vouchers.createdon,vouchers.modifiedon
                    from vouchers
                    inner join coupons on coupons.id = vouchers.coupenid
                    inner join customer on customer.id = vouchers.issueto
                    where  vouchers.ipaymentmode != 3 and vouchers.ipaymentmode != 0
                    and vouchers.isactive=true and vouchers.issuedon>=@fromdate
                    and vouchers.issuedon<@todate and coupons.companyid=@companyid
                    and vouchers.status >= 0 and vouchers.ibankid = @bankid";
                    if (item.companyid == 0)
                    {
                        customervoucher = customervoucher.Replace("and coupons.companyid=@companyid", "");
                    }

                    command = GetCommand(customervoucher, conn);
                    command.Parameters.AddWithValue("fromdate", item.fromdate.Date);
                    command.Parameters.AddWithValue("todate", item.todate.Date.AddDays(1));
                    command.Parameters.AddWithValue("bankid", bankid);
                    command.Parameters.AddWithValue("companyid", item.companyid);
                    command.CommandTimeout = 1000;
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            fromid++;
                            DateTime paymentdate = reader["issuedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["issuedon"]);
                            decimal paidamount = reader["counteramount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["counteramount"]);
                            string name = reader["name"] == DBNull.Value ? "" : Convert.ToString(reader["name"]);
                            string receiptno = reader["seqcode"] == DBNull.Value ? "" : Convert.ToString(reader["seqcode"]);
                            result.Add(new VoucherDisplayItem()
                            {
                                createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]),
                                modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]),
                                modifiedon = reader["modifiedon"] == DBNull.Value ? Convert.ToDateTime(reader["issuedon"]) : Convert.ToDateTime(reader["modifiedon"]),
                                createdon = reader["createdon"] == DBNull.Value ? Convert.ToDateTime(reader["issuedon"]) : Convert.ToDateTime(reader["createdon"]),
                                id = fromid,
                                no = receiptno,
                                date = paymentdate,
                                amount = paidamount,
                                crdr = "Dr",
                                ledger = name,
                                ledgertype = 12,
                                vouchername = "Voucher Sold - Issue",
                                vouchertype = -1,
                                vouchertypeid = -1,
                                isheader = true
                            });
                        }
                    }

                    string customeradvancequery = @"
                    select customer.name,
                    cast(schemelist->>'actionon' as date) paidon,
                    cast(schemelist->>'amount' as numeric) amount,
                    Customerorder.createdby,Customerorder.modifiedby,
                    Customerorder.createdon,Customerorder.modifiedon       
                    from Customerorder
					LEFT JOIN LATERAL
					json_array_elements(Customerorder.paymentinfo :: json) schemelist ON true
                    inner join customer on customer.id = Customerorder.customerid
                    where Customerorder.isactive=true and schemelist->>'mode' != '3'
                    and cast(schemelist->>'actionon' as date)>=@fromdate
                    and cast(schemelist->>'actionon' as date)<@todate 
                    and cast(schemelist->>'bankid' as bigint) = @bankid
                    and Customerorder.companyid=@companyid";
                    if (item.companyid == 0)
                    {
                        customeradvancequery = supplierpaymentquery.Replace("and Customerorder.companyid=@companyid", "");
                    }
                    command = GetCommand(customeradvancequery, conn);
                    command.Parameters.AddWithValue("fromdate", item.fromdate.Date);
                    command.Parameters.AddWithValue("todate", item.todate.Date.AddDays(1));
                    command.Parameters.AddWithValue("bankid", bankid);
                    command.Parameters.AddWithValue("companyid", item.companyid);
                    command.CommandTimeout = 1000;
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            fromid++;
                            DateTime paymentdate = reader["paidon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["paidon"]);
                            decimal paidamount = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"]);
                            string name = reader["name"] == DBNull.Value ? "" : Convert.ToString(reader["name"]);
                            result.Add(new VoucherDisplayItem()
                            {
                                createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]),
                                modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]),
                                modifiedon = reader["modifiedon"] == DBNull.Value ? Convert.ToDateTime(reader["paidon"]) : Convert.ToDateTime(reader["modifiedon"]),
                                createdon = reader["createdon"] == DBNull.Value ? Convert.ToDateTime(reader["paidon"]) : Convert.ToDateTime(reader["createdon"]),
                                id = fromid,
                                date = paymentdate,
                                amount = paidamount,
                                crdr = "Dr",
                                ledger = name,
                                ledgertype = 12,
                                vouchername = "Order Advance Received",
                                vouchertype = -1,
                                vouchertypeid = -1,
                                isheader = true
                            });
                        }
                    }

                    /*

                    customerpaymentquery = @"with billsettleupi as 
(
select sum(cast(paymentinfo->>'upi_paid' as numeric)) amount,billsettlementmaster.settlementon::date,billsettlementmaster.locationid, 
billsettlementmaster.counterid 
from billsettlementmaster 
where billsettlementmaster.isactive=true 
and billsettlementmaster.settlementon>=@fromdate 
and billsettlementmaster.settlementon<@todate
and cast(paymentinfo->>'upi_paid' as numeric)>0
and cast(paymentinfo->>'upiproviderid' as bigint) = @bankid
group by billsettlementmaster.settlementon::date,billsettlementmaster.locationid, 
billsettlementmaster.counterid
)

select amount,settlementon,location.name locationname,counter.name countername from billsettleupi
left outer join referencelist location on location.id = billsettleupi.locationid
left outer join referencelist counter on counter.id = billsettleupi.counterid";
                    command = GetCommand(customerpaymentquery, conn);
                    command.Parameters.AddWithValue("fromdate", item.fromdate.Date);
                    command.Parameters.AddWithValue("todate", item.todate.Date.AddDays(1));
                    command.Parameters.AddWithValue("bankid", bankid);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            fromid++;
                            DateTime paymentdate = reader["settlementon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["settlementon"]);
                            decimal paidamount = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"]);
                            string locationname = reader["locationname"] == DBNull.Value ? "" : Convert.ToString(reader["locationname"]);
                            string countername = reader["countername"] == DBNull.Value ? "" : Convert.ToString(reader["countername"]);
                            result.Add(new VoucherDisplayItem()
                            {
                                id = fromid,
                                no = "",
                                date = paymentdate,
                                amount = paidamount,
                                crdr = "Dr",
                                ledger = locationname + "(" + countername + " - UPI )",
                                ledgertype = 19,
                                vouchername = "Payment Received",
                                vouchertype = -1,
                                vouchertypeid = -1,
                                isheader = true
                            });
                        }
                    }


                    customerpaymentquery = @"with billsettlecard as 
(
select sum(paid_card) amount,billsettlementmaster.settlementon::date,billsettlementmaster.locationid, 
billsettlementmaster.counterid 
from billsettlementmaster 
where billsettlementmaster.isactive=true 
and billsettlementmaster.settlementon>=@fromdate 
and billsettlementmaster.settlementon<@todate
and paid_card>0
and cast(paymentinfo->>'card_bankid' as bigint) = @bankid
group by billsettlementmaster.settlementon::date,billsettlementmaster.locationid, 
billsettlementmaster.counterid
)

select amount,settlementon,location.name locationname,counter.name countername from billsettlecard
left outer join referencelist location on location.id = billsettlecard.locationid
left outer join referencelist counter on counter.id = billsettlecard.counterid";
                    command = GetCommand(customerpaymentquery, conn);
                    command.Parameters.AddWithValue("fromdate", item.fromdate.Date);
                    command.Parameters.AddWithValue("todate", item.todate.Date.AddDays(1));
                    command.Parameters.AddWithValue("bankid", bankid);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            fromid++;
                            DateTime paymentdate = reader["settlementon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["settlementon"]);
                            decimal paidamount = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"]);
                            string locationname = reader["locationname"] == DBNull.Value ? "" : Convert.ToString(reader["locationname"]);
                            string countername = reader["countername"] == DBNull.Value ? "" : Convert.ToString(reader["countername"]);
                            result.Add(new VoucherDisplayItem()
                            {
                                id = fromid,
                                no = "",
                                date = paymentdate,
                                amount = paidamount,
                                crdr = "Dr",
                                ledger = locationname + "(" + countername + " - CARD )",
                                ledgertype = 19,
                                vouchername = "Payment Received",
                                vouchertype = -1,
                                vouchertypeid = -1,
                                isheader = true
                            });
                        }
                    }*/


                }


                if (iscash && brnchids != null && brnchids.Count > 0)
                {
                    var fromid = result.Count == 0 ? 0 : result.Max(x => x.id);

                    //string cashclosingquery = @"select paidon,closingamount,counter.name countername,location.name locationname from cash
                    //inner join referencelist counter on counter.id = counterid
                    //inner join referencelist location on location.id = locationid
                    //where paidon>=@fromdate and paidon<@todate and cash.isactive=true and cash.locationid in (" + String.Join(",", brnchids) + ")";

                    //command = GetCommand(cashclosingquery, conn);
                    //command.Parameters.AddWithValue("fromdate", item.fromdate.Date);
                    //command.Parameters.AddWithValue("todate", item.todate.Date.AddDays(1));
                    //using (var reader = command.ExecuteReader())
                    //{
                    //    while (reader.Read())
                    //    {
                    //        fromid++;
                    //        DateTime paymentdate = reader["paidon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["paidon"]);
                    //        decimal closingamount = reader["closingamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["closingamount"]);
                    //        string countername = reader["countername"] == DBNull.Value ? "" : Convert.ToString(reader["countername"]);
                    //        string locationname = reader["locationname"] == DBNull.Value ? "" : Convert.ToString(reader["locationname"]);
                    //        result.Add(new VoucherDisplayItem()
                    //        {
                    //            id = fromid,
                    //            date = paymentdate,
                    //            amount = closingamount,
                    //            crdr = "Dr",
                    //            ledger = countername + " - " + locationname,
                    //            ledgertype = 16,
                    //            vouchername = "Cash Settlement",
                    //            vouchertype = -1,
                    //            vouchertypeid = -1,
                    //            isheader = true
                    //        });
                    //    }
                    //}



                    string supplierpaymentquery = @"
                    select paymentdate,paidamount,supplier.name suppliername,location.name locationname,
                    supplierpayment.createdby,supplierpayment.modifiedby,supplierpayment.createdon,supplierpayment.modifiedon
                    from supplierpayment
                    inner join supplier on supplier.id = supplierid 
                    left outer join referencelist location on location.id = supplierpayment.locationid
                    where paymentmode = 2 and supplierpayment.companyid=@companyid
                    and paymentdate>=@fromdate and paymentdate<@todate and supplierpayment.isactive=true 
                    and supplierpayment.hascancel=false and supplierpayment.locationid in (" + String.Join(",", brnchids) + ")";
                    if (item.companyid == 0)
                    {
                        supplierpaymentquery = supplierpaymentquery.Replace("and supplierpayment.companyid=@companyid", "");
                    }
                    command = GetCommand(supplierpaymentquery, conn);
                    command.Parameters.AddWithValue("fromdate", item.fromdate.Date);
                    command.Parameters.AddWithValue("todate", item.todate.Date.AddDays(1));
                    command.Parameters.AddWithValue("companyid", item.companyid);
                    command.CommandTimeout = 1000;
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            fromid++;
                            DateTime paymentdate = reader["paymentdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["paymentdate"]);
                            decimal paidamount = reader["paidamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["paidamount"]);
                            string suppliername = reader["suppliername"] == DBNull.Value ? "" : Convert.ToString(reader["suppliername"]);
                            string locationname = reader["locationname"] == DBNull.Value ? "" : Convert.ToString(reader["locationname"]);
                            result.Add(new VoucherDisplayItem()
                            {
                                createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]),
                                modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]),
                                modifiedon = reader["modifiedon"] == DBNull.Value ? Convert.ToDateTime(reader["paymentdate"]) : Convert.ToDateTime(reader["modifiedon"]),
                                createdon = reader["createdon"] == DBNull.Value ? Convert.ToDateTime(reader["paymentdate"]) : Convert.ToDateTime(reader["createdon"]),
                                id = fromid,
                                date = paymentdate,
                                amount = paidamount,
                                crdr = "Cr",
                                ledger = suppliername,
                                ledgertype = 16,
                                vouchername = "Suplier Payment " + locationname,
                                vouchertype = -1,
                                vouchertypeid = -1,
                                isheader = true
                            });
                        }
                    }

                    string transportpaymentquery = @"
                    select paymentdate,paidamount,transport.name suppliername,location.name locationname,
                    transportpayment.createdby,transportpayment.modifiedby,transportpayment.createdon,transportpayment.modifiedon
                    from transportpayment
                    inner join transport on transport.id = transportpayment.transportid 
                    left outer join referencelist location on location.id = transportpayment.locationid
                    where paymentmode = 2 and transportpayment.companyid=@companyid
                    and paymentdate>=@fromdate and paymentdate<@todate and transportpayment.isactive=true 
                    and transportpayment.locationid in (" + String.Join(",", brnchids) + ")";
                    if (item.companyid == 0)
                    {
                        transportpaymentquery = query.Replace("and transportpayment.companyid=@companyid", "");
                    }
                    command = GetCommand(transportpaymentquery, conn);
                    command.Parameters.AddWithValue("fromdate", item.fromdate.Date);
                    command.Parameters.AddWithValue("todate", item.todate.Date.AddDays(1));
                    command.Parameters.AddWithValue("companyid", item.companyid);
                    command.CommandTimeout = 1000;
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            fromid++;
                            DateTime paymentdate = reader["paymentdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["paymentdate"]);
                            decimal paidamount = reader["paidamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["paidamount"]);
                            string suppliername = reader["suppliername"] == DBNull.Value ? "" : Convert.ToString(reader["suppliername"]);
                            string locationname = reader["locationname"] == DBNull.Value ? "" : Convert.ToString(reader["locationname"]);
                            result.Add(new VoucherDisplayItem()
                            {
                                createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]),
                                modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]),
                                modifiedon = reader["modifiedon"] == DBNull.Value ? Convert.ToDateTime(reader["paymentdate"]) : Convert.ToDateTime(reader["modifiedon"]),
                                createdon = reader["createdon"] == DBNull.Value ? Convert.ToDateTime(reader["paymentdate"]) : Convert.ToDateTime(reader["createdon"]),
                                id = fromid,
                                date = paymentdate,
                                amount = paidamount,
                                crdr = "Cr",
                                ledger = suppliername,
                                ledgertype = 16,
                                vouchername = "Transport Payment " + locationname,
                                vouchertype = -1,
                                vouchertypeid = -1,
                                isheader = true
                            });
                        }
                    }

                    string agentpaymentquery = @"
                    select paymentdate,paidamount,agent.name agentname,location.name locationname,
                    agentpayment.createdby,agentpayment.modifiedby,agentpayment.createdon,agentpayment.modifiedon
                    from agentpayment
                    inner join agent on agent.id = agentpayment.agentid
                    left outer join referencelist location on location.id = agentpayment.locationid
                    where paymentmode = 2 and agentpayment.companyid=@companyid
                    and paymentdate>=@fromdate and paymentdate<@todate and agentpayment.isactive=true 
                    and agentpayment.locationid in (" + String.Join(",", brnchids) + ")";
                    if (item.companyid == 0)
                    {
                        agentpaymentquery = query.Replace("and agentpayment.companyid=@companyid", "");
                    }
                    command = GetCommand(agentpaymentquery, conn);
                    command.Parameters.AddWithValue("fromdate", item.fromdate.Date);
                    command.Parameters.AddWithValue("todate", item.todate.Date.AddDays(1));
                    command.Parameters.AddWithValue("companyid", item.companyid);
                    command.CommandTimeout = 1000;
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            fromid++;
                            DateTime paymentdate = reader["paymentdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["paymentdate"]);
                            decimal paidamount = reader["paidamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["paidamount"]);
                            string agentname = reader["agentname"] == DBNull.Value ? "" : Convert.ToString(reader["agentname"]);
                            string locationname = reader["locationname"] == DBNull.Value ? "" : Convert.ToString(reader["locationname"]);
                            result.Add(new VoucherDisplayItem()
                            {
                                createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]),
                                modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]),
                                modifiedon = reader["modifiedon"] == DBNull.Value ? Convert.ToDateTime(reader["paymentdate"]) : Convert.ToDateTime(reader["modifiedon"]),
                                createdon = reader["createdon"] == DBNull.Value ? Convert.ToDateTime(reader["paymentdate"]) : Convert.ToDateTime(reader["createdon"]),
                                id = fromid,
                                date = paymentdate,
                                amount = paidamount,
                                crdr = "Cr",
                                ledger = agentname,
                                ledgertype = 16,
                                vouchername = "Agent Payment " + locationname,
                                vouchertype = -1,
                                vouchertypeid = -1,
                                isheader = true
                            });
                        }
                    }

                    string customerpaymentquery = @"select customer.name,customerpayment.amount,customerpayment.receiptno,
                    customerpayment.paidon,location.name locationname,
                    customerpayment.createdby,customerpayment.modifiedby,customerpayment.createdon,customerpayment.modifiedon
                    from customerpayment
                    inner join customer on customer.id = customerpayment.customerid
                    left outer join referencelist location on location.id = customerpayment.locationid
                    where 
                    customerpayment.isactive=true and customerpayment.paidon>=@fromdate 
                    and customerpayment.paidon<@todate and customerpayment.companyid=@companyid
                    and cast(paymentinfo->>'paymentmode' as integer) = 2 
                    and customerpayment.locationid in (" + String.Join(",", brnchids) + ")";
                    if (item.companyid == 0)
                    {
                        customerpaymentquery = query.Replace("and customerpayment.companyid=@companyid", "");
                    }
                    command = GetCommand(customerpaymentquery, conn);
                    command.Parameters.AddWithValue("fromdate", item.fromdate.Date);
                    command.Parameters.AddWithValue("todate", item.todate.Date.AddDays(1));
                    command.Parameters.AddWithValue("companyid", item.companyid);
                    command.CommandTimeout = 1000;
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            fromid++;
                            DateTime paymentdate = reader["paidon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["paidon"]);
                            decimal paidamount = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"]);
                            string name = reader["name"] == DBNull.Value ? "" : Convert.ToString(reader["name"]);
                            string locationname = reader["locationname"] == DBNull.Value ? "" : Convert.ToString(reader["locationname"]);
                            string receiptno = reader["receiptno"] == DBNull.Value ? "" : Convert.ToString(reader["receiptno"]);
                            result.Add(new VoucherDisplayItem()
                            {
                                createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]),
                                modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]),
                                modifiedon = reader["modifiedon"] == DBNull.Value ? Convert.ToDateTime(reader["paymentdate"]) : Convert.ToDateTime(reader["modifiedon"]),
                                createdon = reader["createdon"] == DBNull.Value ? Convert.ToDateTime(reader["paymentdate"]) : Convert.ToDateTime(reader["createdon"]),
                                id = fromid,
                                no = receiptno,
                                date = paymentdate,
                                amount = paidamount,
                                crdr = "Dr",
                                ledger = name,
                                ledgertype = 12,
                                vouchername = "Payment Received " + locationname,
                                vouchertype = -1,
                                vouchertypeid = -1,
                                isheader = true
                            });
                        }
                    }

                    string customeradvancequery = @"
                    select customer.name,location.name locationname,
                    cast(schemelist->>'actionon' as date) paidon,
                    cast(schemelist->>'amount' as numeric) amount,
                    Customerorder.createdby,Customerorder.modifiedby,
                    Customerorder.createdon,Customerorder.modifiedon
                    from Customerorder
					LEFT JOIN LATERAL
					json_array_elements(Customerorder.paymentinfo :: json) schemelist ON true
                    inner join customer on customer.id = Customerorder.customerid
                    left outer join referencelist location on location.id = Customerorder.locationid
                    where Customerorder.isactive=true and schemelist->>'mode'='3'
                    and cast(schemelist->>'actionon' as date)>=@fromdate
                    and cast(schemelist->>'actionon' as date)<@todate 
                    and Customerorder.companyid=@companyid
                    and Customerorder.locationid in (" + String.Join(",", brnchids) + ")";
                    if (item.companyid == 0)
                    {
                        customeradvancequery = query.Replace("and Customerorder.companyid=@companyid", "");
                    }
                    command = GetCommand(customeradvancequery, conn);
                    command.Parameters.AddWithValue("fromdate", item.fromdate.Date);
                    command.Parameters.AddWithValue("todate", item.todate.Date.AddDays(1));
                    command.Parameters.AddWithValue("companyid", item.companyid);
                    command.CommandTimeout = 1000;
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            fromid++;
                            DateTime paymentdate = reader["paidon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["paidon"]);
                            decimal paidamount = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"]);
                            string name = reader["name"] == DBNull.Value ? "" : Convert.ToString(reader["name"]);
                            string locationname = reader["locationname"] == DBNull.Value ? "" : Convert.ToString(reader["locationname"]);
                            result.Add(new VoucherDisplayItem()
                            {
                                createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]),
                                modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]),
                                modifiedon = reader["modifiedon"] == DBNull.Value ? Convert.ToDateTime(reader["paidon"]) : Convert.ToDateTime(reader["modifiedon"]),
                                createdon = reader["createdon"] == DBNull.Value ? Convert.ToDateTime(reader["paidon"]) : Convert.ToDateTime(reader["createdon"]),
                                id = fromid,
                                date = paymentdate,
                                amount = paidamount,
                                crdr = "Dr",
                                ledger = name,
                                ledgertype = 12,
                                vouchername = "Advance Received " + locationname,
                                vouchertype = -1,
                                vouchertypeid = -1,
                                isheader = true
                            });
                        }
                    }

                    string customervoucher = @"select receivedamt,issuedon,customer.name,seqcode,location.name locationname,
                    coupons.createdby,coupons.modifiedby,
                    coupons.createdon,coupons.modifiedon
                    from coupons
                    inner join customer on customer.id = coupons.customerid
                    left outer join referencelist location on location.id = coupons.locationid
                    where  coupons.paymentmode = 3 
                    and coupons.isactive=true and coupons.issuedon>=@fromdate
                    and coupons.issuedon<@todate  
                    and coupons.issuemode = 0 and coupons.companyid=@companyid
                    and coupons.locationid in (" + String.Join(",", brnchids) + ")";
                    if (item.companyid == 0)
                    {
                        customervoucher = query.Replace("and coupons.companyid=@companyid", "");
                    }
                    command = GetCommand(customervoucher, conn);
                    command.Parameters.AddWithValue("fromdate", item.fromdate.Date);
                    command.Parameters.AddWithValue("todate", item.todate.Date.AddDays(1));
                    command.Parameters.AddWithValue("companyid", item.companyid);
                    command.CommandTimeout = 1000;
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            fromid++;
                            DateTime paymentdate = reader["issuedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["issuedon"]);
                            decimal paidamount = reader["receivedamt"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["receivedamt"]);
                            string name = reader["name"] == DBNull.Value ? "" : Convert.ToString(reader["name"]);
                            string receiptno = reader["seqcode"] == DBNull.Value ? "" : Convert.ToString(reader["seqcode"]);
                            string locationname = reader["locationname"] == DBNull.Value ? "" : Convert.ToString(reader["locationname"]);
                            result.Add(new VoucherDisplayItem()
                            {
                                createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]),
                                modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]),
                                modifiedon = reader["modifiedon"] == DBNull.Value ? Convert.ToDateTime(reader["issuedon"]) : Convert.ToDateTime(reader["modifiedon"]),
                                createdon = reader["createdon"] == DBNull.Value ? Convert.ToDateTime(reader["issuedon"]) : Convert.ToDateTime(reader["createdon"]),
                                id = fromid,
                                no = receiptno,
                                date = paymentdate,
                                amount = paidamount,
                                crdr = "Dr",
                                ledger = name,
                                ledgertype = 12,
                                vouchername = "Voucher Sold " + locationname,
                                vouchertype = -1,
                                vouchertypeid = -1,
                                isheader = true
                            });
                        }
                    }

                    customervoucher = @"select counteramount,vouchers.issuedon,customer.name,seqcode,
                    location.name locationname,
                    vouchers.createdby,vouchers.modifiedby,
                    vouchers.createdon,vouchers.modifiedon
                    from vouchers
                    inner join coupons on coupons.id = vouchers.coupenid
                    inner join customer on customer.id = vouchers.issueto
                    left outer join referencelist location on location.id = coupons.locationid
                    where  vouchers.ipaymentmode = 3
                    and vouchers.isactive=true and vouchers.issuedon>=@fromdate
                    and vouchers.issuedon<@todate 
                    and vouchers.status >= 0 and coupons.companyid=@companyid
                    and vouchers.issuelocationid in (" + String.Join(",", brnchids) + ")";
                    if (item.companyid == 0)
                    {
                        customervoucher = query.Replace("and coupons.companyid=@companyid", "");
                    }
                    command = GetCommand(customervoucher, conn);
                    command.Parameters.AddWithValue("fromdate", item.fromdate.Date);
                    command.Parameters.AddWithValue("todate", item.todate.Date.AddDays(1));
                    command.Parameters.AddWithValue("companyid", item.companyid);
                    command.CommandTimeout = 1000;
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            fromid++;
                            DateTime paymentdate = reader["issuedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["issuedon"]);
                            decimal paidamount = reader["counteramount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["counteramount"]);
                            string name = reader["name"] == DBNull.Value ? "" : Convert.ToString(reader["name"]);
                            string locationname = reader["locationname"] == DBNull.Value ? "" : Convert.ToString(reader["locationname"]);
                            string receiptno = reader["seqcode"] == DBNull.Value ? "" : Convert.ToString(reader["seqcode"]);
                            result.Add(new VoucherDisplayItem()
                            {
                                createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]),
                                modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]),
                                modifiedon = reader["modifiedon"] == DBNull.Value ? Convert.ToDateTime(reader["issuedon"]) : Convert.ToDateTime(reader["modifiedon"]),
                                createdon = reader["createdon"] == DBNull.Value ? Convert.ToDateTime(reader["issuedon"]) : Convert.ToDateTime(reader["createdon"]),
                                id = fromid,
                                no = receiptno,
                                date = paymentdate,
                                amount = paidamount,
                                crdr = "Dr",
                                ledger = name,
                                ledgertype = 12,
                                vouchername = "Voucher Sold " + locationname,
                                vouchertype = -1,
                                vouchertypeid = -1,
                                isheader = true
                            });
                        }
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null && conn.State == System.Data.ConnectionState.Open)
                    conn.Close();
            }

            result = result.OrderBy(x => x.date).ThenBy(y => y.id).ToList();


            decimal obcredit = 0;
            decimal obdebit = 0;

            if (isjobwork)
            {
                string jobworkquery = @"
                select sum(Jobworkbillgrn.grossamount)
                from Jobworkbillgrn
                where Jobworkbillgrn.entrydate<@fromdate
                and Jobworkbillgrn.isactive=true 
                and companyid=@companyid;

                select Jobworkbillgrn.grossamount totalamount,Jobworkbillgrn.invoiceno,Jobworkbillgrn.id,
                Jobworkbillgrn.createdby,Jobworkbillgrn.createdon,Jobworkbillgrn.modifiedby,Jobworkbillgrn.modifiedon,
                Jobworkbillgrn.invoicedate,Jobworkbillgrn.entrydate,Jobworkbillgrn.entryno,suppliercompany.name suppliername
                from Jobworkbillgrn
                inner join suppliercompany on suppliercompany.id = suppliercompanyid
                where Jobworkbillgrn.entrydate>=@fromdate and Jobworkbillgrn.entrydate<@todate
                and Jobworkbillgrn.isactive=true 
                and companyid=@companyid";
                var param = new SortedList<string, object>();
                param.Add("fromdate", item.fromdate.Date);
                param.Add("todate", item.todate.Date.AddDays(1));
                param.Add("supplierid", ledger.supplierid);
                param.Add("companyid", item.companyid);
                using (var reader = ExecuteReader(jobworkquery, param))
                {
                    if (reader.Read() && !reader.IsDBNull(0))
                        obcredit = reader.GetDecimal(0);
                    if (reader.NextResult())
                    {
                        while (reader.Read())
                        {
                            result.Add(new VoucherDisplayItem()
                            {
                                createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]),
                                modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]),
                                modifiedon = reader["modifiedon"] == DBNull.Value ? Convert.ToDateTime(reader["entrydate"]) : Convert.ToDateTime(reader["modifiedon"]),
                                createdon = reader["createdon"] == DBNull.Value ? Convert.ToDateTime(reader["entrydate"]) : Convert.ToDateTime(reader["createdon"]),
                                vouchername = "JOB Work",
                                ledger = reader["suppliername"] == DBNull.Value ? "Unknown Jobworker" : reader["suppliername"].ToString(),
                                date = Convert.ToDateTime(reader["entrydate"]),
                                id = Convert.ToInt64(reader["id"]),
                                narration = reader["entryno"].ToString() + " Inv No : " + reader["invoiceno"] +
                                "(" + Convert.ToDateTime(reader["invoicedate"]).ToString("dd-MM-yyyy") + ")",
                                amount = Convert.ToDecimal(reader["totalamount"]),
                                crdr = "Dr"
                            });
                        }
                        result = result.OrderBy(x => x.date).ToList();
                    }
                }

                
            }

            if (issupplier)
            {
                SupplierpaymentDataService SupplierpaymentData = new SupplierpaymentDataService();
                var suppliertransactions = SupplierpaymentData.PrintSummary(ledger.supplierid,
                    item.locationid,
                    item.companyid,
                 item.fromdate,
                 item.todate, false);
                SortedList<long, long> voucherids = new SortedList<long, long>();
                var returnids = suppliertransactions.transactions.FindAll(x => x.type == "Advance" || x.type == "Advance" || x.type == "Excess Note");
                if (returnids.Count > 0)
                {
                    string returnquery = @"select voucher.id,
                        cast(voucheritems.attributes->>'lrinvoicereturnid' as bigint) lrinvoicereturnid from voucher
                        inner join voucheritems on voucheritems.voucherid = voucher.id
                        inner join ledger on ledger.id = ledgerid
                        where voucherdate>=@fromdate
                        and voucherdate<@todate
                        and voucher.companyid = @companyid
                        and cast(voucheritems.attributes->>'lrinvoicereturnid' as bigint)>0
                        and voucheritems.isactive=true 
                        and voucher.isactive=true
                        and ledger.supplierid  = @supplierid";
                    var param = new SortedList<string, object>();
                    param.Add("fromdate", item.fromdate.Date);
                    param.Add("todate", item.todate.Date.AddDays(1));
                    param.Add("supplierid", ledger.supplierid);
                    param.Add("companyid", item.companyid);
                    using (var reader = ExecuteReader(returnquery, param))
                    {
                        while (reader.Read())
                        {
                            voucherids.Add(Convert.ToInt64(reader["lrinvoicereturnid"]), Convert.ToInt64(reader["id"]));
                        }
                    }
                }

                foreach (var transaction in suppliertransactions.transactions)
                {
                    decimal dr = 0;
                    decimal cr = 0;
                    if (!string.IsNullOrEmpty(transaction.credit))
                        cr = decimal.Parse(transaction.credit);
                    if (!string.IsNullOrEmpty(transaction.debit))
                        dr = decimal.Parse(transaction.debit);

                    long id = transaction.id;
                    bool ledgerswitch = false;
                    if (voucherids.ContainsKey(id))
                    {
                        id = voucherids[id];
                        ledgerswitch = true;
                    }

                    result.Insert(0, new VoucherDisplayItem()
                    {
                        createdby = transaction.createdby,
                        modifiedby = transaction.modifiedby,
                        createdon = transaction.createdon,
                        modifiedon = transaction.modifiedon,
                        id = id,
                        no = transaction.detail != null ? transaction.detail.invoiceno : "",
                        date = transaction.date,
                        amount = cr + dr,
                        crdr = cr > 0 ? "Cr" : "Dr",
                        ledger = transaction.type,
                        vouchertype = ledgerswitch ? -99 : -1,
                        vouchertypeid = ledgerswitch ? -99 : -1,
                        isheader = true,
                        vouchername = transaction.type,
                        narration = transaction.description,
                        invoicedate = transaction.detail != null ? transaction.detail.invoicedate : DateTime.MinValue,
                        invoiceno = transaction.detail != null ? transaction.detail.invoiceno : "",
                    });
                }
                decimal d;
                if (decimal.TryParse(suppliertransactions.obcredit, out d))
                    obcredit += d;
                if (decimal.TryParse(suppliertransactions.obdebit, out d))
                    obdebit += d;
            }

            if (istransport)
            {
                TransportPaymentDataService SupplierpaymentData = new TransportPaymentDataService();
                var suppliertransactions = SupplierpaymentData.PrintSummary(ledger.transportid,
                item.companyid,
                item.fromdate, item.todate);

                foreach (var transaction in suppliertransactions.transactions)
                {
                    decimal dr = 0;
                    decimal cr = 0;
                    if (!string.IsNullOrEmpty(transaction.credit))
                        cr = decimal.Parse(transaction.credit);
                    if (!string.IsNullOrEmpty(transaction.debit))
                        dr = decimal.Parse(transaction.debit);
                    result.Insert(0, new VoucherDisplayItem()
                    {
                        createdby = transaction.createdby,
                        modifiedby = transaction.modifiedby,
                        createdon = transaction.createdon,
                        modifiedon = transaction.modifiedon,
                        id = transaction.id,
                        no = transaction.detail != null ? transaction.detail.invoiceno : "",
                        date = transaction.date,
                        amount = cr + dr,
                        crdr = cr > 0 ? "Cr" : "Dr",
                        ledger = transaction.type,
                        vouchertype = -1,
                        vouchertypeid = -1,
                        isheader = true,
                        vouchername = transaction.type,
                        narration = transaction.description,
                        invoicedate = transaction.detail != null ? transaction.detail.invoicedate : DateTime.MinValue,
                        invoiceno = transaction.detail != null ? transaction.detail.invoiceno : "",
                    });
                }
                decimal d;
                if (decimal.TryParse(suppliertransactions.obcredit, out d))
                    obcredit += d;
                if (decimal.TryParse(suppliertransactions.obdebit, out d))
                    obdebit += d;
            }


            if (iscustomer)
            {
                CustomerpaymentDataService SupplierpaymentData = new CustomerpaymentDataService();
                PrintableLedger suppliertransactions = SupplierpaymentData.PrintSummary(false, ledger.customerid, item.companyid,
                    item.fromdate, item.todate);
                foreach (var transaction in suppliertransactions.transactions)
                {
                    decimal dr = 0;
                    decimal cr = 0;
                    if (!string.IsNullOrEmpty(transaction.credit))
                        cr = decimal.Parse(transaction.credit);
                    if (!string.IsNullOrEmpty(transaction.debit))
                        dr = decimal.Parse(transaction.debit);
                    result.Insert(0, new VoucherDisplayItem()
                    {
                        createdby = transaction.createdby,
                        modifiedby = transaction.modifiedby,
                        createdon = transaction.createdon,
                        modifiedon = transaction.modifiedon,
                        id = transaction.id,
                        iscustomer = true,
                        no = transaction.detail != null ? transaction.detail.invoiceno : "",
                        date = transaction.date,
                        amount = cr + dr,
                        crdr = cr > 0 ? "Cr" : "Dr",
                        ledger = transaction.type,
                        vouchertype = -1,
                        vouchertypeid = -1,
                        isheader = true,
                        vouchername = transaction.type,
                        narration = transaction.description,
                        invoicedate = transaction.detail != null ? transaction.detail.invoicedate : DateTime.MinValue,
                        invoiceno = transaction.detail != null ? transaction.detail.invoiceno : "",
                    });
                }
                decimal d;
                if (decimal.TryParse(suppliertransactions.obcredit, out d))
                    obcredit += d;
                if (decimal.TryParse(suppliertransactions.obdebit, out d))
                    obdebit += d;
            }

            var opening = SearchOpening(item, obcredit, obdebit);

            var emplist = new List<long>();
            emplist.AddRange(result.ConvertAll(x => x.createdby));
            emplist.AddRange(result.ConvertAll(x => x.modifiedby));
            EmployeeDataService employeeDataService = new EmployeeDataService() { Context = this.Context };
            var empnames = employeeDataService.GetUserName(emplist.Distinct().ToList());
            result.ForEach(x =>
            {
                x.cuser = empnames.Exists(y => y.id == x.createdby) ? empnames.Find(y => y.id == x.createdby).name : "";
                x.muser = empnames.Exists(y => y.id == x.modifiedby) ? empnames.Find(y => y.id == x.modifiedby).name : "";
                x.muser = x.modifiedon.ToString("dd-MMM-yyyy") + " @ " + x.muser;
                x.cuser = x.createdon.ToString("dd-MMM-yyyy") + " @ " + x.cuser;
            });


            if (opening.Item1)
            {
                result.Insert(0, new VoucherDisplayItem()
                {
                    id = -1,
                    no = "OB",
                    date = item.fromdate,
                    amount = opening.Item2,
                    crdr = "Cr",
                    ledger = "Opening Balance",
                    vouchertype = -1,
                    vouchertypeid = -1,
                    isheader = true
                });
            }
            else
            {
                result.Insert(0, new VoucherDisplayItem()
                {
                    id = -1,
                    no = "OB",
                    date = item.fromdate,
                    amount = opening.Item2,
                    crdr = "Dr",
                    ledger = "Opening Balance",
                    vouchertype = -1,
                    vouchertypeid = -1,
                    isheader = true
                });
            }

            var debit = result.Sum(x => x.crdr == "Dr" ? x.amount : 0);
            var credit = result.Sum(x => x.crdr == "Cr" ? x.amount : 0);
            if (debit < credit)
            {
                result.Add(new VoucherDisplayItem()
                {
                    id = -2,
                    no = "CB",
                    date = item.todate,
                    amount = credit - debit,
                    crdr = "Cr",
                    ledger = "Closing Balance",
                    vouchertype = -1,
                    vouchertypeid = -1,
                    isheader = true
                });
            }
            else
            {
                result.Add(new VoucherDisplayItem()
                {
                    id = -2,
                    no = "CB",
                    date = item.todate,
                    amount = debit - credit,
                    crdr = "Dr",
                    ledger = "Closing Balance",
                    vouchertype = -1,
                    vouchertypeid = -1,
                    isheader = true
                });
            }

            return result;
        }

        internal Voucher SavePettyCash(Voucher item)
        {
            if (item.id > 0)
            {
                item.modifiedby = Context.UserID;
                item.modifiedon = DateTimeNow;
                item.sourcetype = 1;
                Update(item);
            }
            else
            {
                item.isactive = true;
                item.version = 1;
                item.createdby = Context.UserID;
                item.createdon = DateTimeNow;
                item.sourcetype = 1;
                Insert(item);
            }
            return item;
        }

        public enum CashMode
        {
            PettyCash,
            MainCash,
            Both
        }


        public decimal SaleCash(SaleRetailCreteria saleRetailCreteria)
        {
            decimal closing = 0;
            string query = @"select 
            sum(cast(schemelist->>'amount' as decimal)) paid from customerorder
            LEFT JOIN LATERAL
            json_array_elements (customerorder.paymentinfo :: json) schemelist ON true
            where customerorder.isactive=true and cast(schemelist->>'mode' as int) = 3 and cast(schemelist->>'actionon' as date)<@fromdate
            and locationid=@locationid

            select sum(paid_cash-case when billsettlementmaster.paidamount<0 then cast(paymentinfo->>'cash_return' as numeric) else 0 end) cash
            from billsettlementmaster 
            where settlementon<@fromdate
            and locationid=@locationid

            select cast(paymentinfo->>'paidamount' as decimal) from customerpayment where orderid = 0 and cast(paymentinfo->>'paymentmode' as int) = 2
            and paidon<@fromdate and locationid=@locationid";

            NpgsqlConnection conn = CreateConnection();
            try
            {
                var command = GetCommand(query, conn);
                command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = saleRetailCreteria.locationid;
                command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = saleRetailCreteria.companyid;
                command.Parameters.Add("fromdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = saleRetailCreteria.fromdate.Date;
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                        closing += reader.IsDBNull(0) ? 0 : reader.GetDecimal(0);
                    if (reader.NextResult() && reader.Read())
                        closing += reader.IsDBNull(0) ? 0 : reader.GetDecimal(0);
                    if (reader.NextResult() && reader.Read())
                        closing += reader.IsDBNull(0) ? 0 : reader.GetDecimal(0);
                }
                conn.Close();
                return closing;
            }
            catch (Exception exp)
            {
                if (conn != null && conn.State == System.Data.ConnectionState.Open)
                    conn.Close();
            }
            return 0;
        }

        public class CashInfo
        {
            public decimal CashIncome { get; set; }
            public decimal CashExpense { get; set; }
            public decimal CashDeposit { get; set; }
            public decimal CashWithdraw { get; set; }
        }
        public CashInfo CashInOut(SaleRetailCreteria saleRetailCreteria, long ledgerid)
        {
            CashInfo ci = new CashInfo();

            string cashquery = @"
--cash day out
select sum(case when adjustmentcredit=false then voucher.amount*-1 else voucher.amount end) from voucher
where voucher.sourcetype != 1 and voucher.vouchertype=12 and voucher.locationid=@locationid and voucher.companyid=@companyid 
and voucherdate >= @fromdate and voucherdate <@todate and adjustmentledgerid=@ledgerid  and voucher.isactive = true;

--cash day in
select sum(case when voucheritems.iscredit=false then voucheritems.amount else voucheritems.amount*-1 end) from voucher
inner join voucheritems on voucheritems.voucherid = voucher.id and voucheritems.isactive = true
where voucher.sourcetype != 1 and voucher.vouchertype=11 and voucher.locationid=@locationid and voucher.companyid=@companyid 
and voucherdate >= @fromdate and voucherdate <@todate and voucheritems.ledgerid=@ledgerid and voucher.isactive = true;

--cash expense
select sum(case when adjustmentcredit=true then voucher.amount*-1 else voucher.amount end) from voucher
where voucher.sourcetype != 1 and voucher.vouchertype!=12 and voucher.vouchertype!=11 and adjustmentcredit=false and voucher.locationid=@locationid and voucher.companyid=@companyid 
and voucherdate >= @fromdate and voucherdate <@todate and adjustmentledgerid=@ledgerid  and voucher.isactive = true;

--cash income
select sum(case when voucheritems.iscredit=false then voucheritems.amount else voucheritems.amount*-1 end) from voucher
inner join voucheritems on voucheritems.voucherid = voucher.id and voucheritems.isactive = true
where voucher.sourcetype != 1 and voucher.vouchertype!=11 and voucher.vouchertype!=12 and adjustmentcredit=true and voucher.locationid=@locationid and voucher.companyid=@companyid 
and voucherdate >= @fromdate and voucherdate <@todate and adjustmentledgerid=@ledgerid and voucher.isactive = true;
";

            var query = cashquery;

            if (ledgerid <= 0)
            {
                query = query.Replace("and adjustmentledgerid=@ledgerid", "");
                query = query.Replace("and voucheritems.ledgerid=@ledgerid", "");
            }

            if (saleRetailCreteria.companyid <= 0)
            {
                query = query.Replace("and voucher.companyid=@companyid", "");
            }

            if (saleRetailCreteria.locationid <= 0)
            {
                query = query.Replace("and voucher.locationid=@locationid", "");
            }
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                var command = GetCommand(query, conn);
                command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = saleRetailCreteria.locationid;
                command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = saleRetailCreteria.companyid;
                command.Parameters.Add("ledgerid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = ledgerid;
                command.Parameters.Add("fromdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = saleRetailCreteria.fromdate.Date;
                command.Parameters.Add("todate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = saleRetailCreteria.fromdate.Date.AddDays(1);
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                        ci.CashWithdraw = reader.IsDBNull(0) ? 0 : reader.GetDecimal(0);
                    if (reader.NextResult() && reader.Read())
                        ci.CashDeposit = reader.IsDBNull(0) ? 0 : reader.GetDecimal(0);
                    if (reader.NextResult() && reader.Read())
                        ci.CashIncome = reader.IsDBNull(0) ? 0 : reader.GetDecimal(0);
                    if (reader.NextResult() && reader.Read())
                        ci.CashExpense = reader.IsDBNull(0) ? 0 : reader.GetDecimal(0);
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null && conn.State == System.Data.ConnectionState.Open)
                    conn.Close();
            }
            return ci;
        }

        public decimal CashClosing(SaleRetailCreteria saleRetailCreteria, long ledgerid, CashMode mode)
        {


            decimal closing = 0;
            string cashquery = @"--cash in
select sum(case when voucheritems.iscredit=false then voucheritems.amount else voucheritems.amount*-1 end) from voucher
inner join voucheritems on voucheritems.voucherid = voucher.id and voucheritems.isactive = true
inner join ledger dledger on dledger.id = voucheritems.ledgerid 
where dledger.typeid=6 and voucher.locationid=@locationid and voucher.companyid=@companyid 
and voucherdate <@fromdate and voucheritems.ledgerid=@ledgerid and voucher.isactive = true; 

--cash out
select sum(case when adjustmentcredit=false then voucher.amount else voucher.amount*-1 end) from voucher
inner join ledger cledger on cledger.id = adjustmentledgerid
where cledger.typeid=6 and voucher.locationid=@locationid and voucher.companyid=@companyid 
and voucherdate <@fromdate and adjustmentledgerid=@ledgerid  and voucher.isactive = true;

--cash day out
select sum(case when adjustmentcredit=false then voucher.amount else voucher.amount*-1 end) from voucher
inner join ledger cledger on cledger.id = adjustmentledgerid
where cledger.typeid=6 and voucher.locationid=@locationid and voucher.companyid=@companyid 
and voucherdate >@fromdate and voucherdate <@todate and adjustmentledgerid=@ledgerid  and voucher.isactive = true;

--cash day in
select sum(case when voucheritems.iscredit=false then voucheritems.amount else voucheritems.amount*-1 end) from voucher
inner join voucheritems on voucheritems.voucherid = voucher.id and voucheritems.isactive = true
inner join ledger dledger on dledger.id = voucheritems.ledgerid 
where dledger.typeid=6 and voucher.locationid=@locationid and voucher.companyid=@companyid 
and voucherdate >@fromdate and voucherdate <@todate and voucheritems.ledgerid=@ledgerid and voucher.isactive = true;";

            string pettycashquery = @"--petty cash in
select sum(case when voucheritems.iscredit=false then voucheritems.amount else voucheritems.amount*-1 end) from voucher
inner join voucheritems on voucheritems.voucherid = voucher.id and voucheritems.isactive = true
inner join ledger dledger on dledger.id = voucheritems.ledgerid 
where dledger.typeid=13 and voucher.locationid=@locationid and voucher.companyid=@companyid 
and voucherdate <@fromdate and voucheritems.ledgerid=@ledgerid  and voucher.isactive = true;

--petty cash out
select sum(case when adjustmentcredit=false then voucher.amount else voucher.amount*-1 end) from voucher
inner join ledger cledger on cledger.id = adjustmentledgerid
where cledger.typeid=13 and voucher.locationid=@locationid and voucher.companyid=@companyid 
and voucherdate <@fromdate and adjustmentledgerid=@ledgerid  and voucher.isactive = true;";

            var query = cashquery + "\r\n" + pettycashquery;
            if (mode == CashMode.MainCash)
                query = cashquery;
            else if (mode == CashMode.PettyCash)
                query = pettycashquery;

            if (ledgerid <= 0)
            {
                query = query.Replace("and adjustmentledgerid=@ledgerid", "");
                query = query.Replace("and voucheritems.ledgerid=@ledgerid", "");
            }

            if (saleRetailCreteria.companyid <= 0)
            {
                query = query.Replace("and voucher.companyid=@companyid", "");
            }

            if (saleRetailCreteria.locationid <= 0)
            {
                query = query.Replace("and voucher.locationid=@locationid", "");
            }
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                var command = GetCommand(query, conn);
                command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = saleRetailCreteria.locationid;
                command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = saleRetailCreteria.companyid;
                command.Parameters.Add("ledgerid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = ledgerid;
                command.Parameters.Add("fromdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = saleRetailCreteria.fromdate.Date;
                command.Parameters.Add("todate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = saleRetailCreteria.todate.AddDays(1).Date;
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                        closing += reader.IsDBNull(0) ? 0 : reader.GetDecimal(0);
                    if (reader.NextResult() && reader.Read())
                        closing += reader.IsDBNull(0) ? 0 : reader.GetDecimal(0);

                    if (mode == CashMode.Both)
                    {
                        if (reader.NextResult() && reader.Read())
                            closing += reader.IsDBNull(0) ? 0 : reader.GetDecimal(0);
                        if (reader.NextResult() && reader.Read())
                            closing += reader.IsDBNull(0) ? 0 : reader.GetDecimal(0);
                    }
                }
                conn.Close();
                return closing;
            }
            catch (Exception exp)
            {
                if (conn != null && conn.State == System.Data.ConnectionState.Open)
                    conn.Close();
            }
            return 0;
        }
        internal List<VoucherDisplayItem> SearchDaybook(VoucherCreteria item)
        {
            string query = @"select voucher.id,voucher.voucherno,voucher.voucherdate,aledger.displayname pname,
aledger.typeid atype,voucher.amount pamount,vledger.displayname cname,voucheritems.iscredit,voucheritems.amount,
vledger.typeid ctype,voucher.vouchertype,voucher.vouchertypeid,voucher.purchasetypeid,voucher.sourcetype,
adjustmentcredit from voucher
inner join voucheritems on  voucherid = voucher.id 
inner join ledger aledger on aledger.id = voucher.adjustmentledgerid
inner join ledger vledger on vledger.id = voucheritems.ledgerid where voucher.isactive = true and voucheritems.isactive = true";

            QueryBuilder qb = new QueryBuilder();
            qb.CurrentQuery = query;
            if (item.locationid > 0)
                qb.AddParameter("voucher.locationid", item.locationid, "=");
            qb.AddParameter("voucher.voucherdate", "voucherfromdate", item.fromdate.Date, ">=");
            qb.AddParameter("voucher.voucherdate", "vouchertodate", item.todate.AddDays(1).Date, "<");
            NpgsqlConnection conn = CreateConnection();
            List<Tuple<VoucherDisplayItem, List<VoucherDisplayItem>>> lst = new List<Tuple<VoucherDisplayItem, List<VoucherDisplayItem>>>();

            try
            {
                conn.Open();
                var command = qb.GetCommand(conn);
                command.CommandText += " order by voucher.voucherdate,voucher.id,voucheritems.amount desc";
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        long id = Convert.ToInt64(reader["id"]);
                        var index = lst.FindIndex(e => e.Item1.id == id);
                        if (index < 0)
                        {
                            var vouchertype = Convert.ToInt32(reader["vouchertype"]) * -1;
                            if (reader["vouchertypeid"] != DBNull.Value)
                                vouchertype = Convert.ToInt32(reader["vouchertypeid"]);
                            if (reader["sourcetype"] != null &&
                                Convert.ToInt32(reader["sourcetype"]) == 1)
                                vouchertype = -15;


                            var purchasetypeid = reader["purchasetypeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["purchasetypeid"]);

                            VoucherDisplayItem p = new VoucherDisplayItem()
                            {
                                id = id,
                                no = reader["voucherno"].ToString(),
                                amount = Convert.ToDecimal(reader["pamount"]),
                                date = Convert.ToDateTime(reader["voucherdate"]),
                                crdr = Convert.ToBoolean(reader["adjustmentcredit"]) ? "Cr" : "Dr",
                                ledger = Convert.ToString(reader["pname"]),
                                isheader = true,
                                uid = vouchertype == -15 ? "P-" + id : "V-" + id,
                                vouchertype = vouchertype,
                                purchasetypeid = purchasetypeid,
                                ledgertype = Convert.ToInt32(reader["atype"])
                            };
                            lst.Add(new Tuple<VoucherDisplayItem, List<VoucherDisplayItem>>(p, new List<VoucherDisplayItem>()));
                            index = lst.Count - 1;
                        }
                        lst[index].Item2.Add(new VoucherDisplayItem()
                        {
                            id = id,
                            no = reader["voucherno"].ToString(),
                            date = Convert.ToDateTime(reader["voucherdate"]),
                            amount = Convert.ToDecimal(reader["amount"]),
                            crdr = Convert.ToBoolean(reader["iscredit"]) ? "Cr" : "Dr",
                            ledger = Convert.ToString(reader["cname"]),
                            isheader = false,
                            uid = (lst[index].Item1.vouchertype == -15) ? "P-" + id : "V-" + id,
                            vouchertype = lst[index].Item1.vouchertype,
                            ledgertype = Convert.ToInt32(reader["ctype"])
                        });
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null && conn.State == System.Data.ConnectionState.Open)
                    conn.Close();
            }

            if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.int_finance &&
                item.ledgerid <= 0)
            {
                FinanceService financeService = new FinanceService();

                /*  $scope.ItemTypes = [{ 'id': 1, 'name': 'Capital' },
        { 'id': 2, 'name': 'Fixed asset' },
        { 'id': 3, 'name': 'Libalities' },
        { 'id': 4, 'name': 'Direct Income' },
        { 'id': 5, 'name': 'Direct Expense' },
        { 'id': 6, 'name': 'Cash-In-Hand' },
        { 'id': 7, 'name': 'Bank Account' },
        { 'id': 8, 'name': 'Stock-In-Hand' },
        { 'id': 9, 'name': 'Indirect Incomes' },
        { 'id': 10, 'name': 'Indirect Expenses' },
        { 'id': 11, 'name': 'Sundry Debtors' },
        { 'id': 12, 'name': 'Sundry Creditors' },
        { 'id': 13, 'name': 'Petty Cash' },
        { 'id': 14, 'name': 'Sales' },
        { 'id': 15, 'name': 'Purchase' },
        { 'id': 16, 'name': 'Tax & Duties' },
        { 'id': 17, 'name': 'Petty Cash Direct Expense' },
        { 'id': 18, 'name': 'Petty Cash In-Direct Expense' }];


        $scope.refvouchertypelist = [
            { 'id': -1, 'typeid': 1, 'name': 'Payment' },
            { 'id': -2, 'typeid': 2, 'name': 'Receipt' },
            { 'id': -3, 'typeid': 3, 'name': 'Purchase' },
            { 'id': -4, 'typeid': 4, 'name': 'Sales' },
            { 'id': -5, 'typeid': 5, 'name': 'Credit Note' },
            { 'id': -6, 'typeid': 6, 'name': 'Debit Note' },
            { 'id': -7, 'typeid': 7, 'name': 'Contra' },
            { 'id': -8, 'typeid': 8, 'name': 'Journal' },
            { 'id': -9, 'typeid': 9, 'name': 'Cash Deposit' },
            { 'id': -10, 'typeid': 10, 'name': 'Cash Widhrawal' },
            { 'id': -11, 'typeid': 11, 'name': 'Main Cash to Petty Cash' },
            { 'id': -12, 'typeid': 12, 'name': 'Petty Cash to Main Cash' },
            { 'id': -13, 'typeid': 13, 'name': 'Cash Counter Expense' },
            { 'id': -14, 'typeid': 14, 'name': 'Advance Payment' },
            { 'id': -15, 'typeid': 15, 'name': 'Petty Cash Expense' },
            { 'id': -101, 'typeid': 4, 'name': 'Customer Sales' },
            { 'id': -102, 'typeid': 4, 'name': 'Cusotmer Sales Return' },
            { 'id': -103, 'typeid': 4, 'name': 'Counter Sales' },
            { 'id': -104, 'typeid': 4, 'name': 'Counter Sales Return' },
            { 'id': -105, 'typeid': 3, 'name': 'Purchase Inventory' },
            { 'id': -106, 'typeid': 3, 'name': 'Purchase Return' },
            { 'id': -107, 'typeid': 2, 'name': 'Credit Payment' },
            { 'id': -108, 'typeid': 1, 'name': 'Supplier Payment' },
        ];*/


                var quantoledgers = financeService.SearchSalesLedgers(item.companyid, item.fromdate, item.todate);
                if (item.locationid > 0)
                {
                    quantoledgers.CardSalesList.RemoveAll(x => x.locationid != item.locationid);
                    quantoledgers.CashSalesList.RemoveAll(x => x.locationid != item.locationid);
                    quantoledgers.VoucherSalesList.RemoveAll(x => x.locationid != item.locationid);
                    quantoledgers.TaxList.RemoveAll(x => x.LocationID != item.locationid);
                    quantoledgers.BillList.RemoveAll(x => x.LocationID != item.locationid);
                }

                quantoledgers.BillList.Where(x => x.CustomerID > 0 && x.Amount > 0).ToList().ForEach(x =>
                {
                    if (!string.IsNullOrEmpty(x.CustomerName))
                    {
                        VoucherDisplayItem p = new VoucherDisplayItem()
                        {
                            id = 0,
                            no = x.BillNo,
                            uid = "SCR_" + x.Date.ToString("dd-MM-yyyy") + x.BillNo,
                            amount = x.Amount,
                            date = x.Date,
                            crdr = "Dr",
                            locationid = x.LocationID,
                            ledger = x.CustomerName,
                            refid = x.CustomerID,
                            isheader = true,
                            vouchertype = -101,
                            vouchertypeid = 4 * -1,
                            ledgertype = 11
                        };
                        var parent = new Tuple<VoucherDisplayItem, List<VoucherDisplayItem>>(p, new List<VoucherDisplayItem>());
                        lst.Add(parent);

                        var taxledgers = quantoledgers.TaxList.FindAll(y => y.CustomerID == x.CustomerID &&
                        y.Billno == x.BillNo);
                        var tax = taxledgers.Sum(y => y.tax);
                        p = new VoucherDisplayItem()
                        {
                            id = 0,
                            uid = "SCR_" + x.Date.ToString("dd-MM-yyyy") + x.BillNo,
                            date = x.Date,
                            no = x.BillNo,
                            amount = x.Amount - tax,
                            crdr = "Cr",
                            ledger = "Sales",
                            isheader = false,
                            vouchertype = -101,
                            vouchertypeid = -4,
                            ledgertype = 14
                        };
                        parent.Item2.Add(p);
                        taxledgers.ForEach(z =>
                        {

                            if (z.IsInterstate)
                            {
                                p = new VoucherDisplayItem()
                                {
                                    id = 0,
                                    uid = "SCR_" + z.date.ToString("dd-MM-yyyy") + z.Billno,
                                    date = z.date,
                                    no = "Tax & Duties",
                                    amount = z.tax,
                                    crdr = "Cr",
                                    ledger = "IGST @" + z.taxpercentage.ToString("N2"),
                                    isheader = false,
                                    vouchertype = -101,
                                    vouchertypeid = -4,
                                    ledgertype = 16
                                };
                                parent.Item2.Add(p);
                            }
                            else
                            {
                                p = new VoucherDisplayItem()
                                {
                                    id = 0,
                                    uid = "SCR_" + z.date.ToString("dd-MM-yyyy") + z.Billno,
                                    date = z.date,
                                    no = "Tax & Duties",
                                    amount = z.tax / 2,
                                    crdr = "Cr",
                                    ledger = "CGST @" + (z.taxpercentage / 2).ToString("N2"),
                                    isheader = false,
                                    vouchertype = -101,
                                    vouchertypeid = -4,
                                    ledgertype = 16
                                };
                                parent.Item2.Add(p);

                                p = new VoucherDisplayItem()
                                {
                                    id = 0,
                                    date = z.date,
                                    uid = "SCR_" + z.date.ToString("dd-MM-yyyy") + z.Billno,
                                    no = "Tax & Duties",
                                    amount = z.tax / 2,
                                    crdr = "Cr",
                                    ledger = "SGST @" + (z.taxpercentage / 2).ToString("N2"),
                                    isheader = false,
                                    vouchertype = -101,
                                    vouchertypeid = -4,
                                    ledgertype = 16
                                };
                                parent.Item2.Add(p);
                            }
                        });

                        if ((parent.Item1.amount - parent.Item2.Sum(z => z.amount)) > 5)
                        {
                            var diff = 0;
                        }
                    }
                });

                quantoledgers.BillList.Where(x => x.CustomerID > 0 && x.Amount < 0).ToList().ForEach(x =>
                {
                    if (!string.IsNullOrEmpty(x.CustomerName))
                    {
                        VoucherDisplayItem p = new VoucherDisplayItem()
                        {
                            id = 0,
                            no = x.BillNo,
                            uid = "SCR_" + x.Date.ToString("dd-MM-yyyy") + x.BillNo,
                            amount = Math.Abs(x.Amount),
                            date = x.Date,
                            crdr = "Cr",
                            locationid = x.LocationID,
                            ledger = x.CustomerName,
                            refid = x.CustomerID,
                            isheader = true,
                            vouchertype = -102,
                            vouchertypeid = 4 * -1,
                            ledgertype = 11
                        };
                        var parent = new Tuple<VoucherDisplayItem, List<VoucherDisplayItem>>(p, new List<VoucherDisplayItem>());
                        lst.Add(parent);

                        var taxledgers = quantoledgers.TaxList.FindAll(y => y.CustomerID == x.CustomerID &&
                        y.Billno == x.BillNo);
                        var tax = taxledgers.Sum(y => y.tax);
                        p = new VoucherDisplayItem()
                        {
                            id = 0,
                            uid = "SCR_" + x.Date.ToString("dd-MM-yyyy") + x.BillNo,
                            date = x.Date,
                            no = x.BillNo,
                            amount = Math.Abs(x.Amount) - Math.Abs(tax),
                            crdr = "Dr",
                            ledger = "Sales",
                            isheader = false,
                            vouchertype = -102,
                            vouchertypeid = -4,
                            ledgertype = 14
                        };
                        parent.Item2.Add(p);
                        taxledgers.ForEach(z =>
                        {

                            if (z.IsInterstate)
                            {
                                p = new VoucherDisplayItem()
                                {
                                    id = 0,
                                    uid = "SCR_" + z.date.ToString("dd-MM-yyyy") + z.Billno,
                                    date = z.date,
                                    no = "Tax & Duties",
                                    amount = Math.Abs(z.tax),
                                    crdr = "Dr",
                                    ledger = "IGST @" + z.taxpercentage.ToString("N2"),
                                    isheader = false,
                                    vouchertype = -102,
                                    vouchertypeid = -4,
                                    ledgertype = 16
                                };
                                parent.Item2.Add(p);
                            }
                            else
                            {
                                p = new VoucherDisplayItem()
                                {
                                    id = 0,
                                    uid = "SCR_" + z.date.ToString("dd-MM-yyyy") + z.Billno,
                                    date = z.date,
                                    no = "Tax & Duties",
                                    amount = Math.Abs(z.tax / 2),
                                    crdr = "Dr",
                                    ledger = "CGST @" + (z.taxpercentage / 2).ToString("N2"),
                                    isheader = false,
                                    vouchertype = -102,
                                    vouchertypeid = -4,
                                    ledgertype = 16
                                };
                                parent.Item2.Add(p);

                                p = new VoucherDisplayItem()
                                {
                                    id = 0,
                                    date = z.date,
                                    uid = "SCR_" + z.date.ToString("dd-MM-yyyy") + z.Billno,
                                    no = "Tax & Duties",
                                    amount = Math.Abs(z.tax / 2),
                                    crdr = "Dr",
                                    ledger = "SGST @" + (z.taxpercentage / 2).ToString("N2"),
                                    isheader = false,
                                    vouchertype = -102,
                                    vouchertypeid = -4,
                                    ledgertype = 16
                                };
                                parent.Item2.Add(p);
                            }
                        });
                    }
                });

                //CheckCrDr(lst, "Sales B2b Return Rounding", -104);

                foreach (var bitem in quantoledgers.BillList.Where(x => x.CustomerID == 0 && x.Amount > 0).GroupBy(x => new { x.LocationID, x.Date }))
                {
                    var cardsales = quantoledgers.CardSalesList.Where(x => x.locationid == bitem.Key.LocationID &&
                       x.date == bitem.Key.Date).Sum(x => x.amount);
                    var totalsales = bitem.Sum(x => (x.Amount > 0) ? x.Amount : 0); ;
                    var cashsales = totalsales - cardsales;

                    VoucherDisplayItem p = new VoucherDisplayItem()
                    {
                        id = 0,
                        no = "DIRECT SALES - " + bitem.Key.Date.ToString("dd-MM-yyyy") + " - " + bitem.First().Location,
                        uid = "SCS_" + bitem.Key.Date.ToString("dd-MM-yyyy") + bitem.Key.LocationID,
                        amount = totalsales,
                        date = bitem.Key.Date,
                        crdr = "Dr",
                        locationid = bitem.Key.LocationID,
                        ledger = "Direct Sales",
                        isheader = true,
                        vouchertype = -103,
                        vouchertypeid = 4 * -1,
                        ledgertype = 6
                    };
                    var parent = new Tuple<VoucherDisplayItem, List<VoucherDisplayItem>>(p, new List<VoucherDisplayItem>());
                    lst.Add(parent);

                    var taxlist = quantoledgers.TaxList.FindAll(y => y.CustomerID == 0 &&
                    y.LocationID == bitem.Key.LocationID &&
                    y.date == bitem.Key.Date && y.amount > 0);


                    foreach (var taxitem in taxlist.GroupBy(x => new { x.date, x.taxpercentage, x.IsInterstate }))
                    {
                        p = new VoucherDisplayItem()
                        {
                            id = 0,
                            uid = "SCS_" + bitem.Key.Date.ToString("dd-MM-yyyy") + bitem.Key.LocationID,
                            date = bitem.Key.Date,
                            no = "Sales" + bitem.First().Location,
                            amount = taxitem.Sum(x => x.amount - x.tax),
                            crdr = "Cr",
                            ledger = "Sales @" + taxitem.Key.taxpercentage.ToString("N2"),
                            isheader = false,
                            vouchertype = -103,
                            vouchertypeid = -4,
                            ledgertype = 14
                        };
                        parent.Item2.Add(p);


                        if (taxitem.Key.IsInterstate)
                        {
                            p = new VoucherDisplayItem()
                            {
                                id = 0,
                                uid = "SCS_" + taxitem.Key.date.ToString("dd-MM-yyyy") + bitem.Key.LocationID,
                                date = taxitem.Key.date,
                                no = "Tax & Duties " + bitem.First().Location,
                                amount = taxitem.Sum(x => x.tax),
                                crdr = "Cr",
                                ledger = "IGST @" + taxitem.Key.taxpercentage.ToString("N2"),
                                isheader = false,
                                vouchertype = -103,
                                vouchertypeid = -4,
                                ledgertype = 16
                            };
                            parent.Item2.Add(p);
                        }
                        else
                        {
                            p = new VoucherDisplayItem()
                            {
                                id = 0,
                                uid = "SCS_" + taxitem.Key.date.ToString("dd-MM-yyyy") + bitem.Key.LocationID,
                                date = taxitem.Key.date,
                                no = "Tax & Duties " + bitem.First().Location,
                                amount = taxitem.Sum(x => x.tax) / 2,
                                crdr = "Cr",
                                ledger = "CGST @" + (taxitem.Key.taxpercentage / 2).ToString("N2"),
                                isheader = false,
                                vouchertype = -103,
                                vouchertypeid = -4,
                                ledgertype = 16
                            };
                            parent.Item2.Add(p);

                            p = new VoucherDisplayItem()
                            {
                                id = 0,
                                uid = "SCS_" + taxitem.Key.date.ToString("dd-MM-yyyy") + bitem.Key.LocationID,
                                date = taxitem.Key.date,
                                no = "Tax & Duties " + bitem.First().Location,
                                amount = taxitem.Sum(x => x.tax) / 2,
                                crdr = "Cr",
                                ledger = "SGST @" + (taxitem.Key.taxpercentage / 2).ToString("N2"),
                                isheader = false,
                                vouchertype = -103,
                                vouchertypeid = -4,
                                ledgertype = 16
                            };
                            parent.Item2.Add(p);
                        }
                    };

                    //p = new VoucherDisplayItem()
                    //{
                    //    id = 0,
                    //    no = "CARD - " + bitem.Key.Date.ToString("dd-MM-yyyy"),
                    //    uid = "SCS_" + bitem.Key.Date.ToString("dd-MM-yyyy") + bitem.Key.LocationID,
                    //    amount = totalsales,
                    //    date = bitem.Key.Date,
                    //    crdr = "Dr",
                    //    locationid = bitem.Key.LocationID,
                    //    ledger = "Cash",
                    //    isheader = true,
                    //    vouchertype = 4,
                    //    vouchertypeid = 4 * -1,
                    //    ledgertype = 6
                    //};

                    //quantoledgers.CardSalesList.ForEach(x =>
                    //{
                    //    VoucherDisplayItem p = new VoucherDisplayItem()
                    //    {
                    //        id = 0,
                    //        no = "Bank Payment",
                    //        uid = "SCS_" + x.date.ToString("dd-MM-yyyy"),
                    //        amount = x.amount,
                    //        date = x.date,
                    //        crdr = "Dr",
                    //        locationid = x.locationid,
                    //        ledger = String.IsNullOrEmpty(x.bank) ? "Bank" : x.bank,
                    //        isheader = true,
                    //        vouchertype = 4,
                    //        vouchertypeid = 4 * -1,
                    //        ledgertype = 7
                    //    };
                    //    lst.Add(new Tuple<VoucherDisplayItem, List<VoucherDisplayItem>>(p, new List<VoucherDisplayItem>()));

                    //});


                    //if (cashsales != 0)
                    //{
                    //    VoucherDisplayItem cashsaleledger = new VoucherDisplayItem()
                    //    {
                    //        id = 0,
                    //        no = "Direct Sales",
                    //        amount = cashsales,
                    //        date = bitem.Key.Date,
                    //        uid = "SCS_" + bitem.Key.Date.ToString("dd-MM-yyyy"),
                    //        crdr = "Dr",
                    //        ledger = "Cash",
                    //        isheader = true,
                    //        locationid = bitem.Key.LocationID,
                    //        vouchertype = 4,
                    //        vouchertypeid = -4,
                    //        ledgertype = 6
                    //    };
                    //    var parent = new Tuple<VoucherDisplayItem, List<VoucherDisplayItem>>(cashsaleledger, new List<VoucherDisplayItem>());
                    //    lst.Add(parent);
                    //}

                    //cashsales = quantoledgers.BillList.Sum(x => (x.CustomerID == 0 && x.Amount < 0) ? x.Amount : 0);
                    //if (cashsales != 0)
                    //{
                    //    var cashsalereturnledger = new VoucherDisplayItem()
                    //    {
                    //        id = 0,
                    //        no = "Direct Sales return",
                    //        amount = cashsales,
                    //        uid = "SCS_" + bitem.Key.Date.ToString("dd-MM-yyyy"),
                    //        date = bitem.Key.Date,
                    //        crdr = "Dr",
                    //        ledger = "Sales return",
                    //        isheader = false,
                    //        locationid = bitem.Key.LocationID,
                    //        vouchertype = 4,
                    //        vouchertypeid = -4,
                    //        ledgertype = 6
                    //    };
                    //    var parent = new Tuple<VoucherDisplayItem, List<VoucherDisplayItem>>(cashsalereturnledger, new List<VoucherDisplayItem>());
                    //    lst.Add(parent);
                    //}
                }

                //CheckCrDr(lst, "B2B Sales Rounding", -104);


                foreach (var bitem in quantoledgers.BillList.Where(x => x.CustomerID == 0 && x.Amount < 0).GroupBy(x => new { x.LocationID, x.Date }))
                {
                    //var cardsales = quantoledgers.CardSalesList.Where(x => x.locationid == bitem.Key.LocationID &&
                    //   x.date == bitem.Key.Date).Sum(x => x.amount);
                    var totalsales = bitem.Sum(x => (x.Amount < 0) ? x.Amount : 0); ;


                    VoucherDisplayItem p = new VoucherDisplayItem()
                    {
                        id = 0,
                        no = "DIRECT SALES RETURN - " + bitem.Key.Date.ToString("dd-MM-yyyy") + bitem.First().Location,
                        uid = "SDR_" + bitem.Key.Date.ToString("dd-MM-yyyy") + bitem.Key.LocationID,
                        amount = Math.Abs(totalsales),
                        date = bitem.Key.Date,
                        crdr = "Cr",
                        locationid = bitem.Key.LocationID,
                        ledger = "Direct Sales Return",
                        isheader = true,
                        vouchertype = -104,
                        vouchertypeid = 4 * -1,
                        ledgertype = 6
                    };
                    var parent = new Tuple<VoucherDisplayItem, List<VoucherDisplayItem>>(p, new List<VoucherDisplayItem>());
                    lst.Add(parent);

                    var taxlist = quantoledgers.TaxList.FindAll(y => y.CustomerID == 0 &&
                    y.LocationID == bitem.Key.LocationID &&
                    y.date == bitem.Key.Date && y.amount < 0);

                    foreach (var taxitem in taxlist.GroupBy(x => new { x.date, x.IsInterstate, x.taxpercentage }))
                    {
                        p = new VoucherDisplayItem()
                        {
                            id = 0,
                            uid = "SDR_" + bitem.Key.Date.ToString("dd-MM-yyyy") + bitem.Key.LocationID,
                            date = bitem.Key.Date,
                            no = "Sales" + bitem.First().Location,
                            amount = Math.Abs(taxitem.Sum(x => x.amount)) - Math.Abs(taxitem.Sum(x => x.tax)),
                            crdr = "Dr",
                            ledger = "Sales @" + taxitem.Key.taxpercentage.ToString("N2"),
                            isheader = false,
                            vouchertype = -104,
                            vouchertypeid = -4,
                            ledgertype = 14
                        };
                        parent.Item2.Add(p);

                        if (taxitem.Key.IsInterstate)
                        {
                            p = new VoucherDisplayItem()
                            {
                                id = 0,
                                uid = "SDR_" + taxitem.Key.date.ToString("dd-MM-yyyy") + bitem.Key.LocationID,
                                date = taxitem.Key.date,
                                no = "Tax & Duties " + bitem.First().Location,
                                amount = Math.Abs(taxitem.Sum(y => y.Tax)),
                                crdr = "Dr",
                                ledger = "IGST @" + taxitem.Key.taxpercentage.ToString("N2"),
                                isheader = false,
                                vouchertype = -104,
                                vouchertypeid = -4,
                                ledgertype = 16
                            };
                            parent.Item2.Add(p);
                        }
                        else
                        {
                            p = new VoucherDisplayItem()
                            {
                                id = 0,
                                uid = "SDR_" + taxitem.Key.date.ToString("dd-MM-yyyy") + bitem.Key.LocationID,
                                date = taxitem.Key.date,
                                no = "Tax & Duties " + bitem.First().Location,
                                amount = Math.Abs(taxitem.Sum(y => y.tax) / 2),
                                crdr = "Dr",
                                ledger = "CGST @" + (taxitem.Key.taxpercentage / 2).ToString("N2"),
                                isheader = false,
                                vouchertype = -104,
                                vouchertypeid = -4,
                                ledgertype = 16
                            };
                            parent.Item2.Add(p);

                            p = new VoucherDisplayItem()
                            {
                                id = 0,
                                uid = "SDR_" + taxitem.Key.date.ToString("dd-MM-yyyy") + bitem.Key.LocationID,
                                date = taxitem.Key.date,
                                no = "Tax & Duties " + bitem.First().Location,
                                amount = Math.Abs(taxitem.Sum(y => y.tax) / 2),
                                crdr = "Dr",
                                ledger = "SGST @" + (taxitem.Key.taxpercentage / 2).ToString("N2"),
                                isheader = false,
                                vouchertype = -104,
                                vouchertypeid = -4,
                                ledgertype = 16
                            };
                            parent.Item2.Add(p);
                        }
                    };


                    //p = new VoucherDisplayItem()
                    //{
                    //    id = 0,
                    //    no = "CARD - " + bitem.Key.Date.ToString("dd-MM-yyyy"),
                    //    uid = "SCS_" + bitem.Key.Date.ToString("dd-MM-yyyy") + bitem.Key.LocationID,
                    //    amount = totalsales,
                    //    date = bitem.Key.Date,
                    //    crdr = "Dr",
                    //    locationid = bitem.Key.LocationID,
                    //    ledger = "Cash",
                    //    isheader = true,
                    //    vouchertype = 4,
                    //    vouchertypeid = 4 * -1,
                    //    ledgertype = 6
                    //};

                    //quantoledgers.CardSalesList.ForEach(x =>
                    //{
                    //    VoucherDisplayItem p = new VoucherDisplayItem()
                    //    {
                    //        id = 0,
                    //        no = "Bank Payment",
                    //        uid = "SCS_" + x.date.ToString("dd-MM-yyyy"),
                    //        amount = x.amount,
                    //        date = x.date,
                    //        crdr = "Dr",
                    //        locationid = x.locationid,
                    //        ledger = String.IsNullOrEmpty(x.bank) ? "Bank" : x.bank,
                    //        isheader = true,
                    //        vouchertype = 4,
                    //        vouchertypeid = 4 * -1,
                    //        ledgertype = 7
                    //    };
                    //    lst.Add(new Tuple<VoucherDisplayItem, List<VoucherDisplayItem>>(p, new List<VoucherDisplayItem>()));

                    //});


                    //if (cashsales != 0)
                    //{
                    //    VoucherDisplayItem cashsaleledger = new VoucherDisplayItem()
                    //    {
                    //        id = 0,
                    //        no = "Direct Sales",
                    //        amount = cashsales,
                    //        date = bitem.Key.Date,
                    //        uid = "SCS_" + bitem.Key.Date.ToString("dd-MM-yyyy"),
                    //        crdr = "Dr",
                    //        ledger = "Cash",
                    //        isheader = true,
                    //        locationid = bitem.Key.LocationID,
                    //        vouchertype = 4,
                    //        vouchertypeid = -4,
                    //        ledgertype = 6
                    //    };
                    //    var parent = new Tuple<VoucherDisplayItem, List<VoucherDisplayItem>>(cashsaleledger, new List<VoucherDisplayItem>());
                    //    lst.Add(parent);
                    //}

                    //cashsales = quantoledgers.BillList.Sum(x => (x.CustomerID == 0 && x.Amount < 0) ? x.Amount : 0);
                    //if (cashsales != 0)
                    //{
                    //    var cashsalereturnledger = new VoucherDisplayItem()
                    //    {
                    //        id = 0,
                    //        no = "Direct Sales return",
                    //        amount = cashsales,
                    //        uid = "SCS_" + bitem.Key.Date.ToString("dd-MM-yyyy"),
                    //        date = bitem.Key.Date,
                    //        crdr = "Dr",
                    //        ledger = "Sales return",
                    //        isheader = false,
                    //        locationid = bitem.Key.LocationID,
                    //        vouchertype = 4,
                    //        vouchertypeid = -4,
                    //        ledgertype = 6
                    //    };
                    //    var parent = new Tuple<VoucherDisplayItem, List<VoucherDisplayItem>>(cashsalereturnledger, new List<VoucherDisplayItem>());
                    //    lst.Add(parent);
                    //}
                    if ((parent.Item1.amount - parent.Item2.Sum(z => z.amount)) > 5)
                    {
                        var diff = 0;
                    }
                }
                //return ProcessDaybook(lst);

                //CheckCrDr(lst, "Sales Return Rounding", -104);
                //var allcashsales = quantoledgers.CashSalesList.GroupBy(x => new { x.locationid, x.date });
                //foreach (var cashsale in allcashsales)
                //{
                //    VoucherDisplayItem p = new VoucherDisplayItem()
                //    {
                //        id = 0,
                //        no = "DIRECT CASH - " + cashsale.Key.date.ToString("dd-MM-yyyy") + " - " + cashsale.First().locationname,
                //        uid = "SUS_" + cashsale.Key.date.ToString("dd-MM-yyyy") + cashsale.Key.locationid,
                //        amount = cashsale.Sum(x => x.amount),
                //        date = cashsale.Key.date,
                //        crdr = "Cr",
                //        locationid = cashsale.Key.locationid,
                //        ledger = "Direct Settlement",
                //        isheader = true,
                //        vouchertype = -103,
                //        vouchertypeid = 4,
                //        ledgertype = 6
                //    };
                //    var parent = new Tuple<VoucherDisplayItem, List<VoucherDisplayItem>>(p, new List<VoucherDisplayItem>());
                //    lst.Add(parent);
                //    //foreach (var cashcounter in cashsale.GroupBy(x=>x.date))
                //    {
                //        p = new VoucherDisplayItem()
                //        {
                //            id = 0,
                //            uid = "SUS_" + cashsale.Key.date.ToString("dd-MM-yyyy") + cashsale.Key.locationid,
                //            date = cashsale.Key.date,
                //            no = "Sales",
                //            amount = cashsale.Sum(x => x.amount),
                //            crdr = "Dr",
                //            ledger = "COUNTER CASH",
                //            isheader = false,
                //            vouchertype = -103,
                //            vouchertypeid = 4,
                //            ledgertype = 6
                //        };
                //        parent.Item2.Add(p);
                //    }
                //}

                //var allcardsales = quantoledgers.CardSalesList.GroupBy(x => new { x.locationid, x.date });
                //foreach (var cardsale in allcardsales)
                //{
                //    VoucherDisplayItem p = new VoucherDisplayItem()
                //    {
                //        id = 0,
                //        no = "DIRECT BANK - " + cardsale.Key.date.ToString("dd-MM-yyyy") + " - " + cardsale.First().locationname,
                //        uid = "SUS_" + cardsale.Key.date.ToString("dd-MM-yyyy") + cardsale.Key.locationid,
                //        amount = cardsale.Sum(x => x.amount),
                //        date = cardsale.Key.date,
                //        crdr = "Cr",
                //        locationid = cardsale.Key.locationid,
                //        ledger = "Direct Settlement",
                //        isheader = true,
                //        vouchertype = -103,
                //        vouchertypeid = 4,
                //        ledgertype = 6
                //    };
                //    var parent = new Tuple<VoucherDisplayItem, List<VoucherDisplayItem>>(p, new List<VoucherDisplayItem>());
                //    lst.Add(parent);
                //    foreach (var carddetail in cardsale.GroupBy(x => x.bank))
                //    {
                //        p = new VoucherDisplayItem()
                //        {
                //            id = 0,
                //            uid = "SUS_" + cardsale.Key.date.ToString("dd-MM-yyyy") + cardsale.Key.locationid,
                //            date = cardsale.Key.date,
                //            no = "Sales",
                //            amount = carddetail.Sum(x => x.amount),
                //            crdr = "Dr",
                //            ledger = carddetail.Key,
                //            isheader = false,
                //            vouchertype = -103,
                //            vouchertypeid = 4,
                //            ledgertype = 7
                //        };
                //        parent.Item2.Add(p);
                //    }
                //}


                var vouchersales = quantoledgers.VoucherSalesList.GroupBy(x => new { x.locationid, x.date });
                foreach (var cardsale in vouchersales)
                {
                    VoucherDisplayItem p = new VoucherDisplayItem()
                    {
                        id = 0,
                        no = "COUNTER SALE " + cardsale.Key.date.ToString("dd-MM-yyyy") + " - " + cardsale.First().locationname,
                        uid = "SUSV_" + cardsale.Key.date.ToString("dd-MM-yyyy") + cardsale.Key.locationid,
                        amount = cardsale.Sum(x => x.amount),
                        date = cardsale.Key.date,
                        crdr = "Cr",
                        locationid = cardsale.Key.locationid,
                        ledger = "Voucher Settlement",
                        isheader = true,
                        vouchertype = -103,
                        vouchertypeid = 4,
                        ledgertype = 6
                    };
                    var parent = new Tuple<VoucherDisplayItem, List<VoucherDisplayItem>>(p, new List<VoucherDisplayItem>());
                    lst.Add(parent);
                    p = new VoucherDisplayItem()
                    {
                        id = 0,
                        uid = "SUSV_" + cardsale.Key.date.ToString("dd-MM-yyyy") + cardsale.Key.locationid,
                        date = cardsale.Key.date,
                        no = "Sales",
                        amount = cardsale.Sum(x => x.amount),
                        crdr = "Dr",
                        ledger = "VOUCHER SETTLEMENT",
                        isheader = false,
                        vouchertype = -103,
                        vouchertypeid = 4,
                        ledgertype = 7
                    };
                    parent.Item2.Add(p);

                    if ((p.amount - parent.Item2.Sum(z => z.amount)) > 5)
                    {
                        var diff = 0;
                    }

                }


                CheckCrDr(lst, "Sales Voucher Rounding", -104);


                quantoledgers = financeService.SearchPurchaseLedgers(item.companyid, item.fromdate, item.todate, true);

                quantoledgers.PurchaseLegers.ForEach(x =>
                {
                    if (!string.IsNullOrEmpty(x.suppliercompany))
                    {
                        VoucherDisplayItem p = new VoucherDisplayItem()
                        {
                            id = 0,
                            no = x.invoiceno,
                            date = x.entrydate,
                            uid = "PUR_" + x.entrydate.ToString("dd-MM-yyyy") + x.suppliercompanyid,
                            amount = x.totalamount,
                            crdr = "Cr",
                            ledger = x.suppliercompany,
                            isheader = true,
                            vouchertype = -105,
                            vouchertypeid = -3,
                            ledgertype = 12
                        };
                        var parent = new Tuple<VoucherDisplayItem, List<VoucherDisplayItem>>(p, new List<VoucherDisplayItem>());
                        lst.Add(parent);

                        var taxledgers = quantoledgers.TaxLegers.FindAll(y => y.suppliercompanyid == x.suppliercompanyid &&
                           y.date == x.entrydate).ToList();

                        p = new VoucherDisplayItem()
                        {
                            id = 0,
                            no = x.invoiceno,
                            amount = parent.Item1.amount - taxledgers.Sum(z => z.tax),
                            date = x.entrydate,
                            uid = "PUR_" + x.entrydate.ToString("dd-MM-yyyy") + x.suppliercompanyid,
                            crdr = "Dr",
                            ledger = "Purchase",
                            isheader = false,
                            vouchertype = -105,
                            vouchertypeid = -3,
                            ledgertype = 15
                        };
                        parent.Item2.Add(p);

                        taxledgers.GroupBy(z => new { z.isinterstatesale, z.taxname, z.percentage }).ToList().ForEach(y =>
                        {
                            if (y.Key.taxname != "GST")
                            {
                                p = new VoucherDisplayItem()
                                {
                                    id = 0,
                                    no = "Purchase",
                                    date = x.entrydate,
                                    uid = "PUR_" + x.entrydate.ToString("dd-MM-yyyy") + x.suppliercompanyid,
                                    amount = y.Sum(a => a.tax),
                                    crdr = "Dr",
                                    ledger = y.Key.taxname + " @" + y.Key.percentage.ToString("N2"),
                                    isheader = false,
                                    vouchertype = -105,
                                    vouchertypeid = -3,
                                    ledgertype = 16
                                };
                                parent.Item2.Add(p);
                            }
                            else
                            {
                                if (x.isinterstatesale)
                                {
                                    p = new VoucherDisplayItem()
                                    {
                                        id = 0,
                                        no = "Purchase",
                                        date = x.entrydate,
                                        uid = "PUR_" + x.entrydate.ToString("dd-MM-yyyy") + x.suppliercompanyid,
                                        amount = y.Sum(a => a.tax),
                                        crdr = "Dr",
                                        ledger = "IGST @" + y.Key.percentage.ToString("N2"),
                                        isheader = false,
                                        vouchertype = -105,
                                        vouchertypeid = -3,
                                        ledgertype = 16
                                    };
                                    parent.Item2.Add(p);
                                }
                                else
                                {
                                    p = new VoucherDisplayItem()
                                    {
                                        id = 0,
                                        no = "Purchase",
                                        date = x.entrydate,
                                        uid = "PUR_" + x.entrydate.ToString("dd-MM-yyyy") + x.suppliercompanyid,
                                        amount = y.Sum(a => a.tax) / 2,
                                        crdr = "Dr",
                                        ledger = "CGST @" + (y.Key.percentage / 2).ToString("N2"),
                                        isheader = false,
                                        vouchertype = -105,
                                        vouchertypeid = -3,
                                        ledgertype = 16
                                    };
                                    parent.Item2.Add(p);

                                    p = new VoucherDisplayItem()
                                    {
                                        id = 0,
                                        no = "Purchase",
                                        date = x.entrydate,
                                        uid = "PUR_" + x.entrydate.ToString("dd-MM-yyyy") + x.suppliercompanyid,
                                        amount = y.Sum(a => a.tax) / 2,
                                        crdr = "Dr",
                                        ledger = "SGST @" + (y.Key.percentage / 2).ToString("N2"),
                                        isheader = false,
                                        vouchertype = -105,
                                        vouchertypeid = -3,
                                        ledgertype = 16
                                    };
                                    parent.Item2.Add(p);
                                }
                            }
                        });

                        if ((parent.Item1.amount - parent.Item2.Sum(z => z.amount)) > 5)
                        {
                            var diff = 0;
                        }
                    }
                });
                CheckCrDr(lst, "Purchase Rounding", -105);


                quantoledgers = financeService.SearchJobWorkLedgers(item.companyid, item.fromdate, item.todate, true);

                quantoledgers.PurchaseLegers.ForEach(x =>
                {
                    if (!string.IsNullOrEmpty(x.suppliercompany))
                    {
                        VoucherDisplayItem p = new VoucherDisplayItem()
                        {
                            id = 0,
                            no = x.invoiceno,
                            date = x.entrydate,
                            uid = "JW_" + x.entrydate.ToString("dd-MM-yyyy") + x.suppliercompanyid,
                            amount = x.totalamount,
                            crdr = "Cr",
                            ledger = x.suppliercompany,
                            isheader = true,
                            vouchertype = -105,
                            vouchertypeid = -3,
                            ledgertype = 12
                        };
                        var parent = new Tuple<VoucherDisplayItem, List<VoucherDisplayItem>>(p, new List<VoucherDisplayItem>());
                        lst.Add(parent);

                        var taxledgers = x.taxledgersummary.ToList();

                        p = new VoucherDisplayItem()
                        {
                            id = 0,
                            no = x.invoiceno,
                            amount = parent.Item1.amount - taxledgers.Sum(z => z.tax),
                            date = x.entrydate,
                            uid = "JW_" + x.entrydate.ToString("dd-MM-yyyy") + x.suppliercompanyid,
                            crdr = "Dr",
                            ledger = "Job Work",
                            isheader = false,
                            vouchertype = -105,
                            vouchertypeid = -3,
                            ledgertype = 15
                        };
                        parent.Item2.Add(p);

                        taxledgers.GroupBy(z => new { z.isinterstatesale, z.taxname, z.percentage }).ToList().ForEach(y =>
                        {
                            if (y.Key.taxname != "GST")
                            {
                                p = new VoucherDisplayItem()
                                {
                                    id = 0,
                                    no = "Job Work",
                                    date = x.entrydate,
                                    uid = "JW_" + x.entrydate.ToString("dd-MM-yyyy") + x.suppliercompanyid,
                                    amount = y.Sum(a => a.tax),
                                    crdr = "Dr",
                                    ledger = y.Key.taxname + " @" + y.Key.percentage.ToString("N2"),
                                    isheader = false,
                                    vouchertype = -105,
                                    vouchertypeid = -3,
                                    ledgertype = 16
                                };
                                parent.Item2.Add(p);
                            }
                            else
                            {
                                if (x.isinterstatesale)
                                {
                                    p = new VoucherDisplayItem()
                                    {
                                        id = 0,
                                        no = "Job Work",
                                        date = x.entrydate,
                                        uid = "JW_" + x.entrydate.ToString("dd-MM-yyyy") + x.suppliercompanyid,
                                        amount = y.Sum(a => a.tax),
                                        crdr = "Dr",
                                        ledger = "IGST @" + y.Key.percentage.ToString("N2"),
                                        isheader = false,
                                        vouchertype = -105,
                                        vouchertypeid = -3,
                                        ledgertype = 16
                                    };
                                    parent.Item2.Add(p);
                                }
                                else
                                {
                                    p = new VoucherDisplayItem()
                                    {
                                        id = 0,
                                        no = "Job Work",
                                        date = x.entrydate,
                                        uid = "JW_" + x.entrydate.ToString("dd-MM-yyyy") + x.suppliercompanyid,
                                        amount = y.Sum(a => a.tax) / 2,
                                        crdr = "Dr",
                                        ledger = "CGST @" + (y.Key.percentage / 2).ToString("N2"),
                                        isheader = false,
                                        vouchertype = -105,
                                        vouchertypeid = -3,
                                        ledgertype = 16
                                    };
                                    parent.Item2.Add(p);

                                    p = new VoucherDisplayItem()
                                    {
                                        id = 0,
                                        no = "Job Work",
                                        date = x.entrydate,
                                        uid = "JW_" + x.entrydate.ToString("dd-MM-yyyy") + x.suppliercompanyid,
                                        amount = y.Sum(a => a.tax) / 2,
                                        crdr = "Dr",
                                        ledger = "SGST @" + (y.Key.percentage / 2).ToString("N2"),
                                        isheader = false,
                                        vouchertype = -105,
                                        vouchertypeid = -3,
                                        ledgertype = 16
                                    };
                                    parent.Item2.Add(p);
                                }
                            }
                        });

                    }
                });
                CheckCrDr(lst, "Purchase Rounding", -1055);

                quantoledgers = financeService.SupplierReturnLedger(item.companyid, item.fromdate, item.todate, true);
                quantoledgers.PurchaseReturn.ForEach(x =>
                {
                    if (!string.IsNullOrEmpty(x.suppliercompany))
                    {
                        VoucherDisplayItem p = new VoucherDisplayItem()
                        {
                            id = 0,
                            date = x.returndate,
                            uid = "PRR_" + x.returndate.ToString("dd-MM-yyyy") + x.id,
                            no = x.returnno,
                            amount = x.totalamount,
                            crdr = x.returntype > 10 ? "Cr" : "Dr",
                            ledger = x.suppliercompany,
                            isheader = true,
                            vouchertype = -106,
                            vouchertypeid = -3,
                            ledgertype = 12
                        };
                        var parent = new Tuple<VoucherDisplayItem, List<VoucherDisplayItem>>(p, new List<VoucherDisplayItem>());
                        lst.Add(parent);

                        p = new VoucherDisplayItem()
                        {
                            id = 0,
                            no = x.returnno,
                            date = x.returndate,
                            uid = "PRR_" + x.returndate.ToString("dd-MM-yyyy") + x.id,
                            amount = x.totalamount - x.taxsummary.Sum(y => y.tax),
                            crdr = x.returntype > 10 ? "Dr" : "Cr",
                            ledger = x.returntype == 7 ? "Supplier Advance" : (x.returntype == 10 ? "Excess Note" : "Purchase Return"),
                            isheader = false,
                            vouchertype = -106,
                            vouchertypeid = -3,
                            ledgertype = 15
                        };
                        parent.Item2.Add(p);


                        x.taxsummary.ForEach(y =>
                        {
                            if (x.isinterstatesale)
                            {
                                p = new VoucherDisplayItem()
                                {
                                    id = 0,
                                    no = "Purchase Return",
                                    date = x.returndate,
                                    uid = "PRR_" + x.returndate.ToString("dd-MM-yyyy") + x.id,
                                    amount = y.tax,
                                    crdr = x.returntype > 10 ? "Dr" : "Cr",
                                    ledger = "IGST @" + y.percentage.ToString("N2"),
                                    isheader = false,
                                    vouchertype = -106,
                                    vouchertypeid = -3,
                                    ledgertype = 16
                                };
                                parent.Item2.Add(p);
                            }
                            else
                            {
                                p = new VoucherDisplayItem()
                                {
                                    id = 0,
                                    no = "Purchase Return",
                                    date = y.date,
                                    uid = "PRR_" + y.date.ToString("dd-MM-yyyy") + x.id,
                                    amount = y.tax / 2,
                                    crdr = x.returntype > 10 ? "Dr" : "Cr",
                                    ledger = "CGST @" + (y.percentage / 2).ToString("N2"),
                                    isheader = false,
                                    vouchertype = -106,
                                    vouchertypeid = -3,
                                    ledgertype = 16
                                };
                                parent.Item2.Add(p);
                                p = new VoucherDisplayItem()
                                {
                                    id = 0,
                                    no = "Purchase Return",
                                    date = y.date,
                                    uid = "PRR_" + y.date.ToString("dd-MM-yyyy") + x.id,
                                    amount = y.tax / 2,
                                    crdr = x.returntype > 10 ? "Dr" : "Cr",
                                    ledger = "SGST @" + (y.percentage / 2).ToString("N2"),
                                    isheader = false,
                                    vouchertype = -106,
                                    vouchertypeid = -3,
                                    ledgertype = 16
                                };
                                parent.Item2.Add(p);
                            }
                        });

                        if ((parent.Item1.amount - parent.Item2.Sum(z => z.amount)) > 5)
                        {
                            var diff = 0;
                        }
                    }

                });

                quantoledgers = financeService.SearchCustomerOrderAndVouchers(item.companyid, item.locationid, item.fromdate, item.todate, true);
                quantoledgers.CustomerPayment.ForEach(x =>
                {

                    VoucherDisplayItem p = new VoucherDisplayItem()
                    {
                        id = 0,
                        no = x.orderno + "/" + x.vouchertype,
                        uid = "SCP_" + x.customerid + x.date.ToString("dd-MM-yyyy"),
                        date = x.date,
                        amount = x.amount,
                        crdr = "Cr",
                        ledger = x.customername,
                        isheader = true,
                        vouchertype = -107,
                        vouchertypeid = -1,
                        ledgertype = 11
                    };
                    var parent = new Tuple<VoucherDisplayItem, List<VoucherDisplayItem>>(p, new List<VoucherDisplayItem>());
                    lst.Add(parent);

                    if (x.iscash)
                    {
                        p = new VoucherDisplayItem()
                        {
                            id = 0,
                            no = x.orderno + "/" + "Credit Payment",
                            uid = "SCP_" + x.customerid + x.date.ToString("dd-MM-yyyy"),
                            date = x.date,
                            amount = x.amount,
                            crdr = "Dr",
                            ledger = "Cash",
                            isheader = false,
                            vouchertype = -107,
                            vouchertypeid = -1,
                            ledgertype = 6
                        };
                        parent.Item2.Add(p);
                    }
                    else
                    {
                        p = new VoucherDisplayItem()
                        {
                            id = 0,
                            no = x.orderno + "/" + "Credit Payment",
                            uid = "SCP_" + x.customerid + x.date.ToString("dd-MM-yyyy"),
                            date = x.date,
                            amount = x.amount,
                            crdr = "Dr",
                            ledger = String.IsNullOrEmpty(x.bankname) ? "Bank" : x.bankname,
                            isheader = false,
                            vouchertype = -107,
                            vouchertypeid = -1,
                            ledgertype = 7
                        };
                        parent.Item2.Add(p);
                    }

                    if ((parent.Item1.amount - parent.Item2.Sum(z => z.amount)) > 5)
                    {
                        var diff = 0;
                    }
                });


                quantoledgers = financeService.CreateCreditPaymentVouchers(item.companyid, item.locationid, item.fromdate, item.todate, true);
                quantoledgers.CustomerPayment.ForEach(x =>
                {
                    //if (x.allocations.Sum(y => y.paid) > 0)
                    //{
                    VoucherDisplayItem p = new VoucherDisplayItem()
                    {
                        id = 0,
                        no = x.orderno,
                        uid = "SCP_" + x.customerid + x.date.ToString("dd-MM-yyyy"),
                        date = x.date,
                        amount = x.allocations.Sum(y => y.paid + y.discount),
                        crdr = "Cr",
                        ledger = x.customername,
                        isheader = true,
                        vouchertype = -107,
                        vouchertypeid = -1,
                        ledgertype = 11
                    };
                    var parent = new Tuple<VoucherDisplayItem, List<VoucherDisplayItem>>(p, new List<VoucherDisplayItem>());
                    lst.Add(parent);
                    //}

                    if (x.iscash)
                    {
                        p = new VoucherDisplayItem()
                        {
                            id = 0,
                            no = x.orderno,
                            uid = "SCP_" + x.customerid + x.date.ToString("dd-MM-yyyy"),
                            date = x.date,
                            amount = x.allocations.Sum(y => y.paid),
                            crdr = "Dr",
                            ledger = "Cash",
                            isheader = false,
                            vouchertype = -107,
                            vouchertypeid = -1,
                            ledgertype = 6
                        };
                        parent.Item2.Add(p);
                    }
                    else
                    {
                        p = new VoucherDisplayItem()
                        {
                            id = 0,
                            no = x.orderno,
                            uid = "SCP_" + x.customerid + x.date.ToString("dd-MM-yyyy"),
                            date = x.date,
                            amount = x.allocations.Sum(y => y.paid),
                            crdr = "Dr",
                            ledger = String.IsNullOrEmpty(x.bankname) ? "Bank" : x.bankname,
                            isheader = false,
                            vouchertype = -107,
                            vouchertypeid = -1,
                            ledgertype = 7
                        };
                        parent.Item2.Add(p);
                    }

                    if (x.allocations.Sum(y => y.discount) > 0)
                    {
                        p = new VoucherDisplayItem()
                        {
                            id = 0,
                            no = x.orderno,
                            uid = "SCP_" + x.customerid + x.date.ToString("dd-MM-yyyy"),
                            date = x.date,
                            amount = x.allocations.Sum(y => y.discount),
                            crdr = "Dr",
                            ledger = "Customer Paymemt Discount",
                            isheader = false,
                            vouchertype = -107,
                            vouchertypeid = -1,
                            ledgertype = 14
                        };
                        parent.Item2.Add(p);
                    }

                    if ((parent.Item1.amount - parent.Item2.Sum(z => z.amount)) > 5)
                    {
                        var diff = 0;
                    }
                });
                CheckCrDr(lst, "Customer Payment Rounding", 1);
                quantoledgers = financeService.SearchSupplierPaymentSummary(item.companyid, item.locationid, item.fromdate, item.todate, true);
                quantoledgers.SupplierPayment.ForEach(x =>
                {
                    var debitnote = x.allocations.Sum(y => y.isdebitnote ? y.paidamount : 0);
                    var payment = x.allocations.Sum(y => !y.isdebitnote ? y.paidamount : 0);
                    var commision = x.allocations.Sum(y => !y.isdebitnote ? y.agentcommission : 0);
                    //if (debitnote != 0)
                    //{
                    //    VoucherDisplayItem p = new VoucherDisplayItem()
                    //    {
                    //        id = 0,
                    //        no = "Supplier Adjustment",
                    //        date = x.paymentdate,
                    //        uid = "PSA_" + x.paymentdate.ToString("dd-MM-yyyy") + x.id,
                    //        amount = Math.Abs(debitnote),
                    //        crdr = "Dr",
                    //        ledger = x.suppliercompany,
                    //        isheader = true,
                    //        vouchertype = -108,
                    //        vouchertypeid = -1,
                    //        ledgertype = 12
                    //    };
                    //    var parent = new Tuple<VoucherDisplayItem, List<VoucherDisplayItem>>(p, new List<VoucherDisplayItem>());
                    //    lst.Add(parent);
                    //    x.allocations.FindAll(z => z.isdebitnote).ForEach(y =>
                    //    {
                    //        p = new VoucherDisplayItem()
                    //        {
                    //            id = 0,
                    //            no = "Debit Note",
                    //            date = x.paymentdate,
                    //            uid = "PSD_" + x.paymentdate.ToString("dd-MM-yyyy") + x.id,
                    //            amount = Math.Abs(y.paidamount),
                    //            crdr = "Cr",
                    //            ledger = "Purchase Return",
                    //            isheader = false,
                    //            vouchertype = -108,
                    //            vouchertypeid = -1,
                    //            ledgertype = 6
                    //        };
                    //        parent.Item2.Add(p);
                    //    });
                    //    payment -= Math.Abs(debitnote);
                    //}
                    if (payment > 0)
                    {
                        VoucherDisplayItem p = new VoucherDisplayItem()
                        {
                            id = 0,
                            no = x.receiptno,
                            date = x.paymentdate,
                            uid = "PSP_" + x.paymentdate.ToString("dd-MM-yyyy") + x.id,
                            amount = x.tdsamount + x.rounding + x.allocations.Sum(y => y.isdebitnote ? 0 : y.paidamount + y.discountamount),
                            crdr = "Dr",
                            ledger = x.suppliercompany,
                            isheader = true,
                            vouchertype = -108,
                            vouchertypeid = -1,
                            ledgertype = 12
                        };
                        var parent = new Tuple<VoucherDisplayItem, List<VoucherDisplayItem>>(p, new List<VoucherDisplayItem>());
                        lst.Add(parent);
                        x.allocations.FindAll(z => !z.isdebitnote).ForEach(y =>
                        {
                            if (x.iscash)
                            {
                                p = new VoucherDisplayItem()
                                {
                                    id = 0,
                                    no = x.receiptno,
                                    date = x.paymentdate,
                                    uid = "PSP_" + x.paymentdate.ToString("dd-MM-yyyy") + x.id,
                                    amount = y.paidamount,
                                    crdr = "Cr",
                                    ledger = "Cash",
                                    isheader = false,
                                    vouchertype = -108,
                                    vouchertypeid = -1,
                                    ledgertype = 6
                                };
                                parent.Item2.Add(p);
                            }
                            else
                            {
                                p = new VoucherDisplayItem()
                                {
                                    id = 0,
                                    no = x.receiptno,
                                    date = x.paymentdate,
                                    uid = "PSP_" + x.paymentdate.ToString("dd-MM-yyyy") + x.id,
                                    amount = y.paidamount,
                                    crdr = "Cr",
                                    ledger = String.IsNullOrEmpty(x.bankname) ? "Bank" : x.bankname,
                                    isheader = false,
                                    vouchertype = -108,
                                    vouchertypeid = -1,
                                    ledgertype = 7
                                };
                                parent.Item2.Add(p);
                            }
                            if (y.discountamount != 0)
                            {
                                p = new VoucherDisplayItem()
                                {
                                    id = 0,
                                    no = x.receiptno,
                                    date = x.paymentdate,
                                    uid = "PSP_" + x.paymentdate.ToString("dd-MM-yyyy") + x.id,
                                    amount = y.discountamount,
                                    crdr = "Cr",
                                    ledger = "Supplier Discount",
                                    isheader = false,
                                    vouchertype = -108,
                                    vouchertypeid = -1,
                                    ledgertype = 15
                                };
                                parent.Item2.Add(p);
                            }

                        });
                        if (x.tdsamount != 0)
                        {
                            p = new VoucherDisplayItem()
                            {
                                id = 0,
                                no = x.receiptno,
                                date = x.paymentdate,
                                uid = "PSP_" + x.paymentdate.ToString("dd-MM-yyyy") + x.id,
                                amount = x.tdsamount,
                                crdr = "Cr",
                                ledger = "TDS",
                                isheader = false,
                                vouchertype = -108,
                                vouchertypeid = -1,
                                ledgertype = 15
                            };
                            parent.Item2.Add(p);
                        }
                        if (commision != 0)
                        {
                            p = new VoucherDisplayItem()
                            {
                                id = 0,
                                no = x.receiptno,
                                date = x.paymentdate,
                                uid = "PSA_" + x.paymentdate.ToString("dd-MM-yyyy") + x.id,
                                amount = commision,
                                crdr = "Cr",
                                ledger = "Agent Commission",
                                isheader = false,
                                vouchertype = -108,
                                vouchertypeid = -1,
                                ledgertype = 5
                            };
                            parent.Item2.Add(p);
                        }
                    }

                });
                CheckCrDr(lst, "Supplier Rounding", 1);

                quantoledgers = financeService.SearchTransportLedgers(item.companyid, item.locationid, item.fromdate, item.todate, true);
                quantoledgers.PurchaseLegers.ForEach(x =>
                {
                    if (!string.IsNullOrEmpty(x.suppliercompany))
                    {
                        VoucherDisplayItem p = new VoucherDisplayItem()
                        {
                            id = 0,
                            no = "LR Transport",
                            date = x.entrydate,
                            uid = "LR_" + x.entrydate.ToString("dd-MM-yyyy") + x.suppliercompanyid,
                            amount = x.totalamount,
                            crdr = "Cr",
                            ledger = x.suppliercompany,
                            isheader = true,
                            vouchertype = -105,
                            vouchertypeid = -3,
                            ledgertype = 12
                        };
                        var parent = new Tuple<VoucherDisplayItem, List<VoucherDisplayItem>>(p, new List<VoucherDisplayItem>());
                        lst.Add(parent);


                        p = new VoucherDisplayItem()
                        {
                            id = 0,
                            no = "Courier",
                            amount = parent.Item1.amount,
                            date = x.entrydate,
                            uid = "CUR_" + x.entrydate.ToString("dd-MM-yyyy") + x.suppliercompanyid,
                            crdr = "Dr",
                            ledger = "Purchase",
                            isheader = false,
                            vouchertype = -105,
                            vouchertypeid = -3,
                            ledgertype = 15
                        };
                        parent.Item2.Add(p);

                    }
                });

                quantoledgers = financeService.SearchTransportPaymentSummary(item.companyid, item.locationid, item.fromdate, item.todate, true);
                quantoledgers.SupplierPayment.ForEach(x =>
                {
                    var payment = x.allocations.Sum(y => !y.isdebitnote ? y.paidamount : 0);
                    if (payment > 0)
                    {
                        VoucherDisplayItem p = new VoucherDisplayItem()
                        {
                            id = 0,
                            no = "Transport Payment",
                            date = x.paymentdate,
                            uid = "TSP_" + x.paymentdate.ToString("dd-MM-yyyy") + x.id,
                            amount = x.tdsamount + x.rounding + x.allocations.Sum(y => y.isdebitnote ? 0 : y.paidamount + y.discountamount),
                            crdr = "Dr",
                            ledger = x.suppliercompany,
                            isheader = true,
                            vouchertype = -108,
                            vouchertypeid = -1,
                            ledgertype = 12
                        };
                        var parent = new Tuple<VoucherDisplayItem, List<VoucherDisplayItem>>(p, new List<VoucherDisplayItem>());
                        lst.Add(parent);
                        x.allocations.FindAll(z => !z.isdebitnote).ForEach(y =>
                        {
                            if (x.iscash)
                            {
                                p = new VoucherDisplayItem()
                                {
                                    id = 0,
                                    no = "Transport Payment",
                                    date = x.paymentdate,
                                    uid = "TSP_" + x.paymentdate.ToString("dd-MM-yyyy") + x.id,
                                    amount = y.paidamount,
                                    crdr = "Cr",
                                    ledger = "Cash",
                                    isheader = false,
                                    vouchertype = -108,
                                    vouchertypeid = -1,
                                    ledgertype = 6
                                };
                                parent.Item2.Add(p);
                            }
                            else
                            {
                                p = new VoucherDisplayItem()
                                {
                                    id = 0,
                                    no = "Transport Payment",
                                    date = x.paymentdate,
                                    uid = "TSP_" + x.paymentdate.ToString("dd-MM-yyyy") + x.id,
                                    amount = y.paidamount,
                                    crdr = "Cr",
                                    ledger = String.IsNullOrEmpty(x.bankname) ? "Bank" : x.bankname,
                                    isheader = false,
                                    vouchertype = -108,
                                    vouchertypeid = -1,
                                    ledgertype = 7
                                };
                                parent.Item2.Add(p);
                            }
                            if (y.discountamount != 0)
                            {
                                p = new VoucherDisplayItem()
                                {
                                    id = 0,
                                    no = "Transport Payment",
                                    date = x.paymentdate,
                                    uid = "TSP_" + x.paymentdate.ToString("dd-MM-yyyy") + x.id,
                                    amount = y.discountamount,
                                    crdr = "Cr",
                                    ledger = String.IsNullOrEmpty(x.bankname) ? "Bank" : x.bankname,
                                    isheader = false,
                                    vouchertype = -108,
                                    vouchertypeid = -1,
                                    ledgertype = 15
                                };
                                parent.Item2.Add(p);
                            }
                        });
                    }
                });
                CheckCrDr(lst, "Transport Rounding", 1);
                quantoledgers = financeService.SearchAgentPaymentSummary(item.companyid, item.locationid, item.fromdate, item.todate, true);
                quantoledgers.SupplierPayment.ForEach(x =>
                {
                    var payment = x.allocations.Sum(y => !y.isdebitnote ? y.paidamount : 0);
                    if (payment > 0)
                    {
                        VoucherDisplayItem p = new VoucherDisplayItem()
                        {
                            id = 0,
                            no = "Agent Payment",
                            date = x.paymentdate,
                            uid = "AGP_" + x.paymentdate.ToString("dd-MM-yyyy") + x.id,
                            amount = x.tdsamount + x.rounding + x.allocations.Sum(y => y.isdebitnote ? 0 : y.paidamount + y.discountamount),
                            crdr = "Dr",
                            ledger = x.suppliercompany,
                            isheader = true,
                            vouchertype = -108,
                            vouchertypeid = -1,
                            ledgertype = 12
                        };
                        var parent = new Tuple<VoucherDisplayItem, List<VoucherDisplayItem>>(p, new List<VoucherDisplayItem>());
                        lst.Add(parent);
                        x.allocations.FindAll(z => !z.isdebitnote).ForEach(y =>
                        {
                            if (x.iscash)
                            {
                                p = new VoucherDisplayItem()
                                {
                                    id = 0,
                                    no = "Agent Payment",
                                    date = x.paymentdate,
                                    uid = "TSP_" + x.paymentdate.ToString("dd-MM-yyyy") + x.id,
                                    amount = y.paidamount,
                                    crdr = "Cr",
                                    ledger = "Cash",
                                    isheader = false,
                                    vouchertype = -108,
                                    vouchertypeid = -1,
                                    ledgertype = 6
                                };
                                parent.Item2.Add(p);
                            }
                            else
                            {
                                p = new VoucherDisplayItem()
                                {
                                    id = 0,
                                    no = "Agent Payment",
                                    date = x.paymentdate,
                                    uid = "TSP_" + x.paymentdate.ToString("dd-MM-yyyy") + x.id,
                                    amount = y.paidamount,
                                    crdr = "Cr",
                                    ledger = String.IsNullOrEmpty(x.bankname) ? "Bank" : x.bankname,
                                    isheader = false,
                                    vouchertype = -108,
                                    vouchertypeid = -1,
                                    ledgertype = 7
                                };
                                parent.Item2.Add(p);
                            }
                            if (y.discountamount != 0)
                            {
                                p = new VoucherDisplayItem()
                                {
                                    id = 0,
                                    no = "Agent Payment",
                                    date = x.paymentdate,
                                    uid = "TSP_" + x.paymentdate.ToString("dd-MM-yyyy") + x.id,
                                    amount = y.discountamount,
                                    crdr = "Cr",
                                    ledger = String.IsNullOrEmpty(x.bankname) ? "Bank" : x.bankname,
                                    isheader = false,
                                    vouchertype = -108,
                                    vouchertypeid = -1,
                                    ledgertype = 15
                                };
                                parent.Item2.Add(p);
                            }
                        });
                    }
                });
                CheckCrDr(lst, "AgentRounding", 1);
            }
            return ProcessDaybook(lst);
        }

        public List<VoucherDisplayItem> ProcessDaybook(List<Tuple<VoucherDisplayItem, List<VoucherDisplayItem>>> lst)
        {
            var result = new List<VoucherDisplayItem>();

            foreach (var litem in lst)
            {
                litem.Item1.index = result.Count;
                result.Add(litem.Item1);
                foreach (var citem in litem.Item2)
                {
                    citem.index = result.Count;
                    result.Add(citem);
                }
            }
            return result;
        }
        public VoucherDisplayItem CheckCrDr(List<Tuple<VoucherDisplayItem, List<VoucherDisplayItem>>> lst,
            string adjustmentledger, int vouchertype)
        {
            List<VoucherDisplayItem> rounding = new List<VoucherDisplayItem>();
            VoucherDisplayItem p = null;
            decimal diffdr = 0;
            decimal diffcr = 0;
            var drtotal = lst.Sum(x => x.Item1.crdr == "Dr" ? x.Item1.amount : 0);
            var crtotal = lst.Sum(x => x.Item1.crdr == "Cr" ? x.Item1.amount : 0);

            drtotal += lst.Sum(x => x.Item2.Sum(y => y.crdr == "Dr" ? y.amount : 0));
            crtotal += lst.Sum(x => x.Item2.Sum(y => y.crdr == "Cr" ? y.amount : 0));



            if ((Math.Abs(drtotal) - Math.Abs(crtotal)) > 5)
            {
                var missmatches = lst.FindAll(x => x.Item1.amount != x.Item2.Sum(y => y.amount));

                var diff = (Math.Abs(drtotal) - Math.Abs(crtotal));
            }

            if (drtotal != Math.Round(drtotal, 0))
            {
                diffdr = Math.Round(drtotal, 0) - drtotal;
                p = new VoucherDisplayItem()
                {
                    id = 0,
                    no = adjustmentledger,
                    amount = diffdr,
                    crdr = "Dr",
                    ledger = adjustmentledger,
                    isheader = true,
                    vouchertype = vouchertype,
                    vouchertypeid = vouchertype * -1
                };
                rounding.Add(p);
                drtotal += diffdr;
            }

            if (crtotal != Math.Round(crtotal, 0))
            {
                diffcr = Math.Round(crtotal, 0) - crtotal;
                p = new VoucherDisplayItem()
                {
                    id = 0,
                    no = adjustmentledger,
                    amount = diffcr,
                    crdr = "Cr",
                    ledger = adjustmentledger,
                    isheader = true,
                    vouchertype = vouchertype,
                    vouchertypeid = vouchertype * -1
                };
                rounding.Add(p);
                crtotal += diffcr;
            }

            if (drtotal > crtotal)
            {
                p = new VoucherDisplayItem()
                {
                    id = 0,
                    no = adjustmentledger,
                    amount = drtotal - crtotal,
                    crdr = "Cr",
                    ledger = adjustmentledger,
                    isheader = true,
                    vouchertype = vouchertype,
                    vouchertypeid = vouchertype * -1
                };
                rounding.Add(p);
            }
            else if (drtotal < crtotal)
            {
                p = new VoucherDisplayItem()
                {
                    id = 0,
                    no = adjustmentledger,
                    amount = crtotal - drtotal,
                    crdr = "Dr",
                    ledger = adjustmentledger,
                    isheader = true,
                    vouchertype = vouchertype,
                    vouchertypeid = vouchertype * -1
                };
                rounding.Add(p);
            }
            drtotal = rounding.Sum(x => x.crdr == "Dr" ? x.amount : 0);
            crtotal = rounding.Sum(x => x.crdr == "Cr" ? x.amount : 0);
            if (drtotal == 0 && crtotal == 0) return new VoucherDisplayItem();
            if (drtotal > crtotal)
            {
                p = new VoucherDisplayItem()
                {
                    id = 0,
                    no = adjustmentledger,
                    amount = drtotal,
                    crdr = "Dr",
                    ledger = adjustmentledger,
                    isheader = true,
                    vouchertype = vouchertype,
                    vouchertypeid = vouchertype * -1
                };
                var parent = new Tuple<VoucherDisplayItem, List<VoucherDisplayItem>>(p, new List<VoucherDisplayItem>());
                lst.Add(parent);
                parent.Item2.Add(new VoucherDisplayItem()
                {
                    id = 0,
                    no = adjustmentledger,
                    amount = crtotal,
                    crdr = "Cr",
                    ledger = adjustmentledger,
                    isheader = false,
                    vouchertype = vouchertype,
                    vouchertypeid = vouchertype * -1
                });
            }
            else
            {
                p = new VoucherDisplayItem()
                {
                    id = 0,
                    no = adjustmentledger,
                    amount = crtotal,
                    crdr = "Cr",
                    ledger = adjustmentledger,
                    isheader = true,
                    vouchertype = vouchertype,
                    vouchertypeid = vouchertype * -1
                };
                var parent = new Tuple<VoucherDisplayItem, List<VoucherDisplayItem>>(p, new List<VoucherDisplayItem>());
                lst.Add(parent);
                parent.Item2.Add(new VoucherDisplayItem()
                {
                    id = 0,
                    no = adjustmentledger,
                    amount = drtotal,
                    crdr = "Dr",
                    ledger = adjustmentledger,
                    isheader = false,
                    vouchertype = vouchertype,
                    vouchertypeid = vouchertype * -1
                });
            }
            //List<VoucherDisplayItem> childs = new List<VoucherDisplayItem>();
            //List<VoucherDisplayItem> newchilds = new List<VoucherDisplayItem>();
            //foreach (var gitem in lst)
            //{
            //    gitem.Item2.ForEach(x => x.ownerlist = gitem.Item2);
            //    childs.AddRange(gitem.Item2);
            //}
            //childs.GroupBy(x => new { x.ledger, x.crdr, x.ledgertype, iscr = x.amount > 0 }).ToList().ForEach(y =>
            //{
            //    if (y.Count() > 1)
            //    {
            //        VoucherDisplayItem nchild = new VoucherDisplayItem()
            //        {
            //            id = 0,
            //            no = y.First().no,
            //            amount = 0,
            //            crdr = y.First().crdr,
            //            ledger = y.First().ledger,
            //            isheader = false,
            //            vouchertype = y.First().vouchertype,
            //            ledgertype = y.First().ledgertype
            //        };
            //        newchilds.Add(nchild);
            //        foreach (var item in y)
            //        {
            //            item.ownerlist.Clear();
            //            nchild.amount += item.amount;
            //        }
            //    }
            //});
            //foreach(var item in newchilds)
            //{
            //    lst.Add(new Tuple<VoucherDisplayItem, List<VoucherDisplayItem>>(item, new List<VoucherDisplayItem>()));
            //}

            return p;
        }


        internal PettycashVoucher SearchPettyCashReport(VoucherCreteria item)
        {
            PettycashVoucher pettycashVoucher = new PettycashVoucher() { items = new List<PettycashVoucher.ReportLine>() };

            QueryBuilder qb = new QueryBuilder();
            if (item.type == "summary")
            {
                qb.CurrentQuery = @"select ledger.displayname,ledger.typeid,
                                sum(case when voucheritems.iscredit then voucheritems.amount else 0 end) credit,
                                sum(case when voucheritems.iscredit then 0 else voucheritems.amount end) debit
                                from voucher 
                                inner join voucheritems on voucheritems.voucherid = voucher.id and voucheritems.isactive=true
                                inner join ledger on ledger.id = voucheritems.ledgerid
                                where voucher.isactive=true";
                qb.AddParameter("voucher.voucherdate", "fromdate", item.fromdate.Date, ">=");
                qb.AddParameter("voucher.voucherdate", "todate", item.todate.Date.AddDays(1), "<");
                if (item.attributes != null &&
                   item.attributes.counterid > 0)
                {
                    qb.AddParameter("cast(voucher.attributes->>'counterid' as bigint)", "counterid",
                        item.attributes.counterid, "=");
                }
                if (item.sourcetype > 0)
                    qb.AddParameter("sourcetype", item.sourcetype, "=");
                if (item.locationid > 0)
                    qb.AddParameter("voucher.locationid", item.locationid, "=");
                if (item.companyid > 0)
                    qb.AddParameter("voucher.companyid", item.companyid, "=");
                qb.AddParameter("voucher.adjustmentledgerid", item.ledgerid, "=");
                qb.CurrentQuery += " group by ledger.displayname,ledger.typeid";
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    var command = qb.GetCommand(conn);
                    using (System.Data.IDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            PettycashVoucher.ReportLine reportLine = new PettycashVoucher.ReportLine();
                            reportLine.typeid = reader["typeid"] == DBNull.Value ? 0 : Convert.ToInt32(reader["typeid"]);
                            reportLine.debit = reader["debit"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["debit"]);
                            reportLine.credit = reader["credit"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["credit"]);
                            reportLine.ledger = reader["displayname"] == DBNull.Value ? "" : reader["displayname"].ToString();
                            pettycashVoucher.items.Add(reportLine);
                        }
                    }
                    conn.Close();
                }
            }
            else
            {
                if (item.fromdate.Date == item.todate.Date)
                {
                    var cashtransaction = SearchCashTransaction(item,
                    item.fromdate,
                    item.todate.AddDays(1));

                    decimal totalSales = 0;
                    if (cashtransaction.cashsales > 0)
                    {
                        pettycashVoucher.items.Add(new PettycashVoucher.ReportLine()
                        {
                            typeid = 14,
                            credit = cashtransaction.cashsales,
                            ledger = "Cash Sales",
                        });
                    }
                    if (cashtransaction.cardsales.Count > 0)
                    {
                        totalSales += cashtransaction.cardsales.Sum(x => x.amount);
                        pettycashVoucher.items.Add(new PettycashVoucher.ReportLine()
                        {
                            typeid = 14,
                            credit = cashtransaction.cardsales.Sum(x => x.amount),
                            ledger = "Card Sales",
                        });
                    }
                    if (cashtransaction.upisales.Count > 0)
                    {
                        totalSales += cashtransaction.upisales.Sum(x => x.amount);
                        pettycashVoucher.items.Add(new PettycashVoucher.ReportLine()
                        {
                            typeid = 14,
                            credit = cashtransaction.upisales.Sum(x => x.amount),
                            ledger = "UPI Sales",
                        });
                    }
                    if (cashtransaction.chequesales.Count > 0)
                    {
                        totalSales += cashtransaction.chequesales.Sum(x => x.amount);
                        pettycashVoucher.items.Add(new PettycashVoucher.ReportLine()
                        {
                            typeid = 14,
                            credit = cashtransaction.chequesales.Sum(x => x.amount),
                            ledger = "UPI Sales",
                        });
                    }
                    if (cashtransaction.creditsales.Count > 0)
                    {
                        totalSales += cashtransaction.creditsales.Sum(x => x.amount);
                        pettycashVoucher.items.Add(new PettycashVoucher.ReportLine()
                        {
                            typeid = 14,
                            credit = cashtransaction.creditsales.Sum(x => x.amount),
                            ledger = "Credit Sales",
                        });
                    }
                    if (cashtransaction.vouchersales.Count > 0)
                    {
                        totalSales += cashtransaction.vouchersales.Sum(x => x.amount);
                        pettycashVoucher.items.Add(new PettycashVoucher.ReportLine()
                        {
                            typeid = 14,
                            credit = cashtransaction.vouchersales.Sum(x => x.amount),
                            ledger = "Voucher Sales",
                        });
                    }

                    if (cashtransaction.cashrefund > 0)
                    {
                        pettycashVoucher.items.Add(new PettycashVoucher.ReportLine()
                        {
                            typeid = 14,
                            debit = cashtransaction.cashrefund,
                            ledger = "CASH REFUND",
                        });
                    }

                    if (totalSales != 0)
                    {
                        pettycashVoucher.items.Add(new PettycashVoucher.ReportLine()
                        {
                            typeid = 14,
                            debit = totalSales,
                            ledger = "TOTAL SALES - CASH SALES",
                        });
                    }

                    if (cashtransaction.customerpayments.Count > 0)
                    {
                        pettycashVoucher.items.Add(new PettycashVoucher.ReportLine()
                        {
                            typeid = 6,
                            credit = cashtransaction.customerpayments.Sum(x => x.amount),
                            ledger = "CUSTOMER PAYMENT",
                        });
                    }

                    if (cashtransaction.vouchersold.Count > 0)
                    {
                        pettycashVoucher.items.Add(new PettycashVoucher.ReportLine()
                        {
                            typeid = 6,
                            credit = cashtransaction.vouchersold.Sum(x => x.amount),
                            ledger = "VOUCHER SOLD",
                        });
                    }
                }
                qb.CurrentQuery = @"select voucherdate,voucheritems.iscredit,voucheritems.amount,ledger.tagid,
                                ledger.displayname,ledger.typeid,voucheritems.attributes->>'narration' narr from voucher 
                                inner join voucheritems on voucheritems.voucherid = voucher.id and voucheritems.isactive=true
                                inner join ledger on ledger.id = voucheritems.ledgerid
                                where voucher.isactive=true";
                qb.AddParameter("voucher.voucherdate", "fromdate", item.fromdate.Date, ">=");
                qb.AddParameter("voucher.voucherdate", "todate", item.todate.Date.AddDays(1), "<");
                if (item.attributes != null &&
                   item.attributes.counterid > 0)
                {
                    qb.AddParameter("cast(voucher.attributes->>'counterid' as bigint)", "counterid",
                        item.attributes.counterid, "=");
                }
                if (item.sourcetype > 0)
                    qb.AddParameter("sourcetype", item.sourcetype, "=");
                if (item.locationid > 0)
                    qb.AddParameter("voucher.locationid", item.locationid, "=");
                if (item.companyid > 0)
                    qb.AddParameter("voucher.companyid", item.companyid, "=");
                qb.AddParameter("voucher.adjustmentledgerid", item.ledgerid, "=");
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    var command = qb.GetCommand(conn);
                    using (System.Data.IDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            PettycashVoucher.ReportLine reportLine = new PettycashVoucher.ReportLine();
                            reportLine.typeid = reader["typeid"] == DBNull.Value ? 0 : Convert.ToInt32(reader["typeid"]);
                            reportLine.tagid = reader["tagid"] == DBNull.Value ? 0 : Convert.ToInt32(reader["tagid"]);
                            if (Convert.ToBoolean(reader["iscredit"]))
                                reportLine.credit = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"]);
                            else
                                reportLine.debit = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"]);
                            reportLine.ledger = reader["displayname"] == DBNull.Value ? "" : reader["displayname"].ToString();
                            reportLine.narr = reader["narr"] == DBNull.Value ? "" : reader["narr"].ToString();
                            reportLine.date = reader["voucherdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["voucherdate"]);

                            pettycashVoucher.items.Add(reportLine);
                        }
                    }
                    conn.Close();
                }
            }

            if (item.fromdate.Date == item.todate.Date)
            {
                pettycashVoucher.items.RemoveAll(x => x.tagid == 12 && x.narr == "CASH SALES");
                pettycashVoucher.items.RemoveAll(x => x.tagid == 18 && x.narr == "VOUCHER SOLD");
                pettycashVoucher.items.RemoveAll(x => x.tagid == 18 && x.narr == "CASH REFUND");
                pettycashVoucher.items.RemoveAll(x => x.tagid == 18 && x.narr == "CUSTOMER PAYMENT");
            }

            pettycashVoucher.CashOpening = SearchCashTransaction(item, DateTime.MinValue, item.fromdate);
            pettycashVoucher.CashClosing = SearchCashTransaction(item, item.fromdate, item.todate.AddDays(1));
            return pettycashVoucher;
        }

        public class CashResult
        {
            public decimal cashsales { get; set; }
            public decimal cashrefund { get; set; }
            public List<BankTrans> cardsales { get; set; }
            public List<BankTrans> chequesales { get; set; }

            public List<BankTrans> upisales { get; set; }
            public List<VoucherTrans> vouchersales { get; set; }
            public List<CreditTrans> creditsales { get; set; }
            public List<PaymentTrans> customerpayments { get; set; }
            public List<PaymentTrans> customerorder { get; set; }
            public List<CreditTrans> vouchersold { get; set; }
            public SortedDictionary<int, Ledger> ledgers { get; set; }

            public class BankTrans
            {
                public string bank { get; set; }
                public decimal amount { get; set; }
            }
            public class CreditTrans
            {
                public string name { get; set; }
                public decimal amount { get; set; }
            }
            public class VoucherTrans
            {
                public string code { get; set; }
                public decimal amount { get; set; }
            }


            public class OrderTrans
            {
                public decimal amount { get; set; }
                public decimal mode { get; set; }
                public string bank { get; set; }
            }
            public class PaymentTrans
            {
                public string name { get; set; }
                public decimal amount { get; set; }
            }

        }
        public CashResult SearchCashTransaction(VoucherCreteria item, DateTime from, DateTime to)
        {

            string query = @"

select sum(billsettlementmaster.paid_cash)
from billsettlementmaster
left outer join users on users.id = billsettlementmaster.createdby
where billsettlementmaster.isactive=true and billsettlementmaster.paidamount>0
and billsettlementmaster.paid_cash>0
and billsettlementmaster.settlementon>=@fromdate 
and billsettlementmaster.settlementon<@todate
and billsettlementmaster.locationid = @locationid;

with billsettle as 
(
	select billsettlementmaster.id, billsettlementmaster.receivable,
billsettlementmaster.paidamount,paid_upi, paid_card,paid_credit, 
billsettlementmaster.locationid, billsettlementmaster.counterid , 
cast(paymentinfo->>'card_bankid' as bigint) bankid, 
cast(paymentinfo->>'upiproviderid' as bigint) upiproviderid
from billsettlementmaster 
left outer join users on users.id = billsettlementmaster.createdby
where billsettlementmaster.isactive=true and paid_card>0
and billsettlementmaster.settlementon>=@fromdate 
and billsettlementmaster.settlementon<@todate
and billsettlementmaster.locationid = @locationid
)
select bank.name as bankname, sum(paid_card) amount from billsettle
left outer join referencelist bank on bank.id = billsettle.bankid
group by billsettle.bankid, bank.name;

with billsettleupi as 
(
select billsettlementmaster.id, billsettlementmaster.receivable,
billsettlementmaster.paidamount,paid_upi, paid_card,paid_credit, 
billsettlementmaster.locationid, billsettlementmaster.counterid , 
cast(paymentinfo->>'card_bankid' as bigint) bankid, 
cast(paymentinfo->>'upiproviderid' as bigint) upiproviderid
from billsettlementmaster 
left outer join users on users.id = billsettlementmaster.createdby
where billsettlementmaster.isactive=true 
and billsettlementmaster.settlementon>=@fromdate 
and billsettlementmaster.settlementon<@todate
and billsettlementmaster.locationid = @locationid
and cast(paymentinfo->>'upi_paid' as numeric)>0
)

select bank.name as bankname, sum(paid_upi) amount from billsettleupi
left outer join referencelist bank on bank.id = billsettleupi.upiproviderid
group by billsettleupi.upiproviderid, bank.name;

select schemelist->>'code' code,cast(schemelist->>'amount' as numeric) amount
from billsettlementmaster 
left outer join users on users.id = billsettlementmaster.createdby
LEFT JOIN LATERAL json_array_elements (paymentinfo->'voucherinformation') schemelist ON true
where billsettlementmaster.isactive=true
AND (cast(paymentinfo->>'voucher_paid' as numeric))>0
and billsettlementmaster.settlementon>=@fromdate 
and billsettlementmaster.settlementon<@todate
and billsettlementmaster.locationid = @locationid;

select
cast(paymentinfo->>'credit_paid' as numeric) amount, customer.name
from billsettlementmaster
left outer join customer on customer.id = billsettlementmaster.customerid
left outer join users on users.id = billsettlementmaster.createdby
where billsettlementmaster.isactive=true and cast(paymentinfo->>'credit_paid' as numeric)>0
and billsettlementmaster.settlementon>=@fromdate 
and billsettlementmaster.settlementon<@todate
and billsettlementmaster.locationid = @locationid;

select billsettlementmaster.id,billsettlementmaster.code code,
case when billsettlementmaster.paidamount<0 then cast(paymentinfo->>'cash_return' as numeric) else 0 end amount
from billsettlementmaster
left outer join users on users.id = billsettlementmaster.createdby
where billsettlementmaster.isactive=true and billsettlementmaster.paidamount<0 and cast(paymentinfo->>'cash_return' as numeric) > 0
and billsettlementmaster.settlementon>=@fromdate 
and billsettlementmaster.settlementon<@todate
and billsettlementmaster.locationid = @locationid;

select customer.name,customerpayment.amount from customerpayment
inner join customer on customer.id = customerpayment.customerid
left outer join referencelist on referencelist.id = cast(paymentinfo->>'upiproviderid' as bigint)
where customerpayment.isactive=true 
and customerpayment.paidon>=@fromdate 
and customerpayment.paidon<@todate
and customerpayment.locationid = @locationid
and cast(paymentinfo->>'paymentmode' as int) = 2;

Select
sum(cast(paymentlist->>'amount' as numeric)) amount,
customer.name
from customerorder 
INNER JOIN LATERAL json_array_elements (customerorder.paymentinfo :: json) paymentlist ON true
left outer join referencelist on referencelist.id = cast(paymentlist->>'bankid' as bigint)
left outer join customer on customer.id = customerorder.customerid
where customerorder.isactive=true and customerorder.status != 3
and cast(paymentlist->>'actionon' as timestamp without time zone)>=@fromdate
and cast(paymentlist->>'actionon' as timestamp without time zone)<@todate
and  cast(paymentlist->>'locationid' as bigint) = @locationid
and (cast(paymentlist->>'mode' as numeric) = 3 or cast(paymentlist->>'mode' as numeric) = 0)
group by customer.name;

select customer.name,receivedamt amount from coupons
inner join customer on customer.id = coupons.customerid
where issuemode = 0 and paymentmode = 3
and issuedon>=@fromdate 
and issuedon<@todate;";

            CashResult cashResult = new CashResult();
            using (NpgsqlConnection conn = CreateConnection())
            {
                conn.Open();

                var command = GetCommand(query, conn);
                command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.locationid;
                command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.companyid;
                command.Parameters.Add("fromdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = from;
                command.Parameters.Add("todate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = to;
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        cashResult.cashsales = reader.IsDBNull(0) ? 0 : reader.GetDecimal(0);
                    }
                    cashResult.chequesales = new List<CashResult.BankTrans>();
                    if (reader.NextResult())
                    {
                        cashResult.cardsales = new List<CashResult.BankTrans>();
                        while (reader.Read())
                        {
                            string bank = reader["bankname"] == DBNull.Value ? "" : Convert.ToString(reader["bankname"]);
                            if (bank.ToUpper().Contains("CHQ") || bank.ToUpper().Contains("CHEQUE"))
                            {
                                cashResult.chequesales.Add(new CashResult.BankTrans()
                                {
                                    bank = bank,
                                    amount = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"])
                                });
                            }
                            else
                            {
                                cashResult.cardsales.Add(new CashResult.BankTrans()
                                {
                                    bank = bank,
                                    amount = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"])
                                });
                            }
                        }
                    }
                    if (reader.NextResult())
                    {
                        cashResult.upisales = new List<CashResult.BankTrans>();
                        while (reader.Read())
                        {
                            string bank = reader["bankname"] == DBNull.Value ? "" : Convert.ToString(reader["bankname"]);
                            if (bank.ToUpper().Contains("CHQ") || bank.ToUpper().Contains("CHEQUE"))
                            {
                                cashResult.chequesales.Add(new CashResult.BankTrans()
                                {
                                    bank = bank,
                                    amount = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"])
                                });
                            }
                            else
                            {
                                cashResult.upisales.Add(new CashResult.BankTrans()
                                {
                                    bank = reader["bankname"] == DBNull.Value ? "" : Convert.ToString(reader["bankname"]),
                                    amount = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"])
                                });
                            }
                        }
                    }

                    if (reader.NextResult())
                    {
                        cashResult.vouchersales = new List<CashResult.VoucherTrans>();
                        while (reader.Read())
                        {
                            cashResult.vouchersales.Add(new CashResult.VoucherTrans()
                            {
                                code = reader["code"] == DBNull.Value ? "" : Convert.ToString(reader["code"]),
                                amount = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"])
                            });
                        }
                    }

                    if (reader.NextResult())
                    {
                        cashResult.creditsales = new List<CashResult.CreditTrans>();
                        while (reader.Read())
                        {
                            cashResult.creditsales.Add(new CashResult.CreditTrans()
                            {
                                name = reader["name"] == DBNull.Value ? "" : Convert.ToString(reader["name"]),
                                amount = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"])
                            });
                        }
                    }

                    if (reader.NextResult())
                    {
                        cashResult.cashrefund = 0;
                        while (reader.Read())
                        {
                            cashResult.cashrefund += reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"]);
                        }
                    }

                    if (reader.NextResult())
                    {
                        cashResult.customerpayments = new List<CashResult.PaymentTrans>();
                        while (reader.Read())
                        {
                            cashResult.customerpayments.Add(new CashResult.PaymentTrans()
                            {
                                name = reader["name"] == DBNull.Value ? "" : Convert.ToString(reader["name"]),
                                amount = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"])
                            });
                        }
                    }

                    if (reader.NextResult())
                    {
                        cashResult.customerorder = new List<CashResult.PaymentTrans>();
                        while (reader.Read())
                        {
                            cashResult.customerorder.Add(new CashResult.PaymentTrans()
                            {
                                name = reader["name"] == DBNull.Value ? "" : Convert.ToString(reader["name"]),
                                amount = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"])
                            });
                        }
                    }

                    if (reader.NextResult())
                    {
                        cashResult.vouchersold = new List<CashResult.CreditTrans>();
                        while (reader.Read())
                        {
                            cashResult.vouchersold.Add(new CashResult.CreditTrans()
                            {
                                name = reader["name"] == DBNull.Value ? "" : Convert.ToString(reader["name"]),
                                amount = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"])
                            });
                        }
                    }

                }
            }

            LedgerDataService ledgerDataService = new LedgerDataService() { Context = this.Context };
            var cashrelated = ledgerDataService.Search(new LedgerCreteria() { filterid = LedgerCreteria.Filter.CashRelated });
            SortedDictionary<int, Ledger> cashrelatedledgers = new SortedDictionary<int, Ledger>();
            if (!cashrelated.Exists(x => x.tagid == 12))
            {
                cashrelatedledgers.Add(12, ledgerDataService.Insert(new Ledger()
                {
                    typeid = 14,
                    tagid = 12,
                    isactive = true,
                    displayname = "Cash Sales"
                }));
            }
            else
            {
                cashrelatedledgers.Add(12, cashrelated.Find(x => x.tagid == 12));
            }
            if (!cashrelated.Exists(x => x.tagid == 13))
            {
                cashrelatedledgers.Add(13, ledgerDataService.Insert(new Ledger()
                {
                    typeid = 14,
                    tagid = 13,
                    isactive = true,
                    displayname = "Card Sales"
                }));
            }
            else
            {
                cashrelatedledgers.Add(13, cashrelated.Find(x => x.tagid == 13));
            }

            if (!cashrelated.Exists(x => x.tagid == 14))
            {
                cashrelatedledgers.Add(14, ledgerDataService.Insert(new Ledger()
                {
                    typeid = 14,
                    tagid = 14,
                    isactive = true,
                    displayname = "UPI Sales"
                }));
            }
            else
            {
                cashrelatedledgers.Add(14, cashrelated.Find(x => x.tagid == 14));
            }

            if (!cashrelated.Exists(x => x.tagid == 15))
            {
                cashrelatedledgers.Add(15, ledgerDataService.Insert(new Ledger()
                {
                    typeid = 14,
                    tagid = 15,
                    isactive = true,
                    displayname = "CHQ Sales"
                }));
            }
            else
            {
                cashrelatedledgers.Add(15, cashrelated.Find(x => x.tagid == 15));
            }

            if (!cashrelated.Exists(x => x.tagid == 16))
            {
                cashrelatedledgers.Add(16, ledgerDataService.Insert(new Ledger()
                {
                    typeid = 14,
                    tagid = 16,
                    isactive = true,
                    displayname = "CREDIT Sales"
                }));
            }
            else
            {
                cashrelatedledgers.Add(16, cashrelated.Find(x => x.tagid == 16));
            }

            if (!cashrelated.Exists(x => x.tagid == 17))
            {
                cashrelatedledgers.Add(17, ledgerDataService.Insert(new Ledger()
                {
                    typeid = 14,
                    tagid = 17,
                    isactive = true,
                    displayname = "VOUCHER Sales"
                }));
            }
            else
            {
                cashrelatedledgers.Add(17, cashrelated.Find(x => x.tagid == 17));
            }


            if (!cashrelated.Exists(x => x.tagid == 18))
            {
                cashrelatedledgers.Add(18, ledgerDataService.Insert(new Ledger()
                {
                    typeid = 11,
                    tagid = 18,
                    isactive = true,
                    displayname = "CUSTOMER Ledger"
                }));
            }
            else
            {
                cashrelatedledgers.Add(18, cashrelated.Find(x => x.tagid == 18));
            }

            if (!cashrelated.Exists(x => x.tagid == 1))
            {
                cashrelatedledgers.Add(1, ledgerDataService.Insert(new Ledger()
                {
                    typeid = 14,
                    tagid = 1,
                    isactive = true,
                    displayname = "Sales Ledger"
                }));
            }
            else
            {
                cashrelatedledgers.Add(1, cashrelated.Find(x => x.tagid == 1));
            }
            cashResult.ledgers = cashrelatedledgers;
            /*
            { 'id': 12, 'name': 'CASH Sales' },
            { 'id': 13, 'name': 'CARD Sales' },
            { 'id': 14, 'name': 'UPI Sales' },
            { 'id': 15, 'name': 'CHQ Sales' },
            { 'id': 16, 'name': 'CREDIT Sales' },
            { 'id': 17, 'name': 'VOUCHER Sales' },
            { 'id': 18, 'name': 'CASH Ledger' },*/
            return cashResult;
        }
        internal Voucher SearchPettyCash(VoucherCreteria item)
        {
            Voucher voucher = null;
            var list = Search(new VoucherCreteria()
            {
                sourcetype = 1,
                ledgerid = item.ledgerid,
                voucherdate = item.voucherdate,
                locationid = item.locationid,
                companyid = item.companyid,
                attributes = item.attributes
            });
            if (list.Count > 0)
            {
                voucher = list.First();
            }
            if (voucher == null)
            {
                voucher = new Voucher()
                {
                    voucherdate = item.voucherdate,
                    locationid = item.locationid,
                    companyid = item.companyid,
                    sourcetype = 1
                };
            }
            return voucher;
        }

        public class CashTransaction
        {
            public decimal amount { get; set; }
            public TransactionType vouchertype { get; set; }
            public LedgerType ledgertype { get; set; }

            public enum LedgerType
            {
                Capital,
                Fixedasset,
                Libalities,
                DirectIncome,
                DirectExpense,
                CashInHand,
                BankAccount,
                StockInHand,
                IndirectIncomes,
                IndirectExpenses,
                SundryDebtors,
                SundryCreditors,
                PettyCash,
                Sales,
                Purchase,
                TaxDuties
            }
            public enum TransactionType
            {
                PettyCash = 0,
                CashDeposit = 9,
                CashWidhrawal = 10,
                MainCashtoPettyCash = 11,
                PettyCashMainCashto = 12,
            }
        }

        const string SQL_Delete = @"update public.voucher set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";

        public List<VoucherDetail> Search(VoucherCreteria _voucher)
        {
            List<VoucherDetail> lst = new List<VoucherDetail>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_voucher.id != 0)
                {
                    command = GetCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _voucher.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("voucher.isactive", true, "=");


                    if (!string.IsNullOrEmpty(_voucher.voucherno))
                    {
                        qb.AddParameter("voucherno", _voucher.voucherno, "=");
                    }
                    else if (_voucher.cashposting)
                    {
                        qb.AddParameter("vouchertype", 4, "=");
                        qb.AddInParamer("sourcetype", "3,4,5");
                        qb.AddParameter("voucherdate", "fromdate", _voucher.voucherdate.Date, ">=");
                        qb.AddParameter("voucherdate", "todate", _voucher.voucherdate.Date.AddDays(1), "<");
                        if (_voucher.locationid > 0)
                            qb.AddParameter("locationid", _voucher.locationid, "=");
                    }
                    else
                    {

                        if (_voucher.fromdate.Year > 2000 && _voucher.todate.Year > 2000)
                        {
                            qb.AddParameter("voucherdate", "fromdate", _voucher.fromdate.Date, ">=");
                            qb.AddParameter("voucherdate", "todate", _voucher.todate.Date.AddDays(1), "<");
                        }
                        else if (_voucher.voucherdate.Year > 2000)
                        {
                            qb.AddParameter("voucherdate", "fromdate", _voucher.voucherdate.Date, ">=");
                            qb.AddParameter("voucherdate", "todate", _voucher.voucherdate.Date.AddDays(1), "<");
                        }

                        //if (_voucher.attributes != null && _voucher.attributes.date1.Year > 2000)
                        //{
                        //    qb.AddParameter("cast(attributes->> 'date1' as timestamp without timezone)", "fromdate", _voucher.attributes.date1.Date, ">=");
                        //    qb.AddParameter("cast(attributes->> 'date1' as timestamp without timezone)", "todate", _voucher.attributes.date1.Date.AddDays(1), "<");
                        //}


                        if (_voucher.vouchertype == 99)
                        {
                            qb.AddParameter("vouchertype", 4, "=");
                            qb.AddInParamer("sourcetype", "3,4,5");
                        }
                        else
                        {
                            if (_voucher.attributes != null &&
                               _voucher.attributes.counterid > 0)
                            {
                                qb.AddParameter("cast(attributes->>'counterid' as bigint)", "counterid",
                                    _voucher.attributes.counterid, "=");
                            }
                            if (_voucher.sourcetype > 0)
                                qb.AddParameter("sourcetype", _voucher.sourcetype, "=");

                            if (_voucher.vouchertype > 0)
                                qb.AddParameter("vouchertype", _voucher.vouchertype, "=");

                            if (_voucher.vouchertypeid > 0)
                                qb.AddParameter("vouchertypeid", _voucher.vouchertypeid, "=");
                            if (_voucher.purchasetypeid > 0)
                                qb.AddParameter("purchasetypeid", _voucher.purchasetypeid, "=");
                        }

                        if (_voucher.locationid > 0)
                            qb.AddParameter("locationid", _voucher.locationid, "=");

                        if (_voucher.employeeid > 0)
                            qb.AddParameter("voucher.employeeid", _voucher.employeeid, "=");

                        if (_voucher.companyid > 0)
                            qb.AddParameter("voucher.companyid", _voucher.companyid, "=");

                        if (_voucher.ledgerid > 0)
                            qb.AddParameter("adjustmentledgerid", _voucher.ledgerid, "=");

                        else if (_voucher.adjustmentledgerid > 0)
                            qb.AddParameter("adjustmentledgerid", _voucher.adjustmentledgerid, "=");


                        if (_voucher.filtertype == 2)
                        {

                            qb.CurrentQuery = @"with duplicatelist as
                            (
                            select id,adjustmentledgerid,amount,attributes->>'invoicedate' invoicedate,
                            row_number() OVER (PARTITION BY adjustmentledgerid,amount,attributes->>'invoicedate' ORDER BY id ASC) rownumber
                            from voucher 
                            where isactive=true
                            and voucherdate>=@filterfrom and voucherdate<@filterto
                            ),filteredduplicate as
                            (
                                select adup.id from duplicatelist dup
                                inner join duplicatelist adup on adup.adjustmentledgerid = dup.adjustmentledgerid and 
                                adup.invoicedate = dup.invoicedate and adup.amount = dup.amount
                                where dup.rownumber>1
                            )" + qb.CurrentQuery;
                            qb.Add2Query(" voucher.id in (select id from filteredduplicate)");

                        }
                        if (_voucher.filtertype == 9)
                        {
                            QueryBuilder qbfilter = new QueryBuilder();
                            var CurrentQuery = "";
                            if (_voucher.filterledgerid > 0)
                            {
                                CurrentQuery = "select id from voucher where isactive=true and adjustmentledgerid=" + _voucher.filterledgerid + " and voucherdate>=@filterfrom and voucherdate<@filterto";
                                if (_voucher.amountfrom > 0)
                                {
                                    CurrentQuery += " and voucher.amount>=@amountfrom";
                                }
                                if (_voucher.amountfrom > 0)
                                {
                                    CurrentQuery += " and voucher.amount<=@amountto";
                                }
                                CurrentQuery += " union all select voucherid from voucheritems inner join voucher on voucher.id = voucheritems.voucherid  and voucherdate>=@filterfrom and voucherdate<@filterto";
                                CurrentQuery += " and voucheritems.ledgerid=" + _voucher.filterledgerid;

                                if (_voucher.amountfrom > 0)
                                {
                                    CurrentQuery += " and voucheritems.amount>=@amountfrom";
                                }
                                if (_voucher.amountfrom > 0)
                                {
                                    CurrentQuery += " and voucheritems.amount<=@amountto";
                                }
                            }
                            else if (_voucher.amountfrom > 0 || _voucher.amountto > 0)
                            {
                                CurrentQuery = "select id from voucher where isactive=true  and voucherdate>=@filterfrom and voucherdate<@filterto";
                                if (_voucher.amountfrom > 0)
                                {
                                    CurrentQuery += " and voucher.amount>=@amountfrom";
                                }
                                if (_voucher.amountfrom > 0)
                                {
                                    CurrentQuery += " and voucher.amount<=@amountto";
                                }
                                qbfilter.CurrentQuery += " union all select voucherid from voucheritems inner join voucher on voucher.id = voucheritems.voucherid  and voucherdate>=@filterfrom and voucherdate<@filterto";
                                if (_voucher.amountfrom > 0)
                                {
                                    CurrentQuery += " and voucheritems.amount>=@amountfrom";
                                }
                                if (_voucher.amountfrom > 0)
                                {
                                    CurrentQuery += " and voucheritems.amount<=@amountto";
                                }
                            }

                            if (CurrentQuery != "")
                            {
                                qb.Add2Query(" voucher.id in (" + CurrentQuery + ")");
                            }
                        }
                    }
                    command = qb.GetCommand(conn);
                    command.Parameters.Add("amountfrom", NpgsqlTypes.NpgsqlDbType.Numeric).Value = _voucher.amountfrom;
                    command.Parameters.Add("amountto", NpgsqlTypes.NpgsqlDbType.Numeric).Value = _voucher.amountto;
                    command.Parameters.Add("filterfrom", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = _voucher.fromdate;
                    command.Parameters.Add("filterto", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = _voucher.todate.AddDays(1);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        VoucherDetail ovoucher = new VoucherDetail();
                        ovoucher.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        ovoucher.adjustmentledgerid = reader["adjustmentledgerid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["adjustmentledgerid"]);
                        ovoucher.adjustmentcredit = reader["adjustmentcredit"] == DBNull.Value ? false : Convert.ToBoolean(reader["adjustmentcredit"]);
                        ovoucher.vouchertype = reader["vouchertype"] == DBNull.Value ? 0 : Convert.ToInt64(reader["vouchertype"]);
                        ovoucher.vouchertypeid = reader["vouchertypeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["vouchertypeid"]);
                        ovoucher.purchasetypeid = reader["purchasetypeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["purchasetypeid"]);
                        ovoucher.voucherdate = reader["voucherdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["voucherdate"]);
                        ovoucher.uniqueuid = reader["uniqueuid"] == DBNull.Value ? "" : reader["uniqueuid"].ToString();
                        ovoucher.attributes_json = reader["attributes"] == DBNull.Value ? "null" : reader["attributes"].ToString();

                        ovoucher.voucherno = reader["voucherno"] == DBNull.Value ? "" : reader["voucherno"].ToString();
                        ovoucher.narration = reader["narration"] == DBNull.Value ? "" : reader["narration"].ToString();
                        ovoucher.amount = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"]);
                        //ovoucher.opening = reader["opening"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["opening"]);
                        ovoucher.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        ovoucher.bwise = reader["bwise"] == DBNull.Value ? 0 : Convert.ToInt32(reader["bwise"]);
                        ovoucher.isinterstate = reader["isinterstate"] == DBNull.Value ? false : Convert.ToBoolean(reader["isinterstate"]);
                        ovoucher.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        ovoucher.locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                        ovoucher.employeeid = reader["employeeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["employeeid"]);
                        ovoucher.companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"]);
                        ovoucher.sourcetype = reader["sourcetype"] == DBNull.Value ? 0 : Convert.ToInt32(reader["sourcetype"]);

                        ovoucher.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        ovoucher.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        ovoucher.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        ovoucher.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);

                        ovoucher.cempid = reader["cemployeeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["cemployeeid"]);
                        ovoucher.mempid = reader["memployeeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["memployeeid"]);

                        lst.Add(ovoucher);
                    }
                }

                if (_voucher.filtertype == 2)
                {
                    lst = lst.OrderBy(x => x.adjustmentledgerid).ThenBy(x => x.voucherdate).ToList();
                }
                else
                {
                    lst = lst.OrderBy(x => x.voucherdate).ToList();
                }

                var empids = lst.ConvertAll(x => x.cempid);
                empids.AddRange(lst.ConvertAll(y => y.mempid));


                EmployeeDataService employeeDataService = new EmployeeDataService() { Context = this.Context };
                var names = employeeDataService.SearchHeader(new EmployeeCreteria()
                {
                    ids = empids.Distinct().ToList()
                });

                lst.ForEach(x =>
                {
                    if (names.Exists(y => y.id == x.cempid))
                    {
                        x.cemp = names.Find(y => y.id == x.cempid).name;
                    }
                    if (names.Exists(y => y.id == x.mempid))
                    {
                        x.memp = names.Find(y => y.id == x.mempid).name;
                    }
                });



                if (_voucher.id > 0 || lst.Count == 1 || _voucher.showdetail)
                {
                    VoucheritemsDataService subVoucheritems = new VoucheritemsDataService() { Context = this.Context };
                    subVoucheritems.Context = Context;
                    foreach (var item in lst)
                    {
                        item.voucheritems = subVoucheritems.SearchAll(item.id);
                        if (item.attributes != null &&
                            item.attributes.hasinv)
                        {
                            item.voucheritems.RemoveAll(x => x.ledgerid == item.attributes.invledgerid);
                            VoucheritemstockDataService subVoucherstockitems = new VoucheritemstockDataService() { Context = this.Context };
                            var invstockitems = subVoucherstockitems.SearchAll(item.id);
                            invstockitems.ForEach(x => x.id = x.id * -1);
                            item.voucheritems.AddRange(invstockitems);
                        }
                    }

                    if (_voucher.id > 0 && lst.Count == 1 &&
                        (lst[0].vouchertype == 3 || lst[0].vouchertype == 4))
                    {
                        VoucheritembillwiseDataService voucheritembillwiseDataService = new VoucheritembillwiseDataService() { Context = this.Context };
                        voucheritembillwiseDataService.Context = Context;
                        var billwise = voucheritembillwiseDataService.Search(new VoucheritembillwiseCreteria()
                        {
                            voucherid = _voucher.id
                        });
                        if (lst[0].attributes == null) lst[0].attributes = new Voucher.voucharattributesExtension();
                        lst[0].attributes.adjustedvouchervalue = billwise.Sum(x => x.adjusted);
                        if (billwise.Count > 0)
                        {
                            lst[0].attributes.adjustedvouchers = string.Join(",", voucheritembillwiseDataService.Search(new VoucheritembillwiseCreteria()
                            {
                                sourceitemids = billwise.ConvertAll(x => x.id)
                            }).ConvertAll(x => x.referenceno));
                        }
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _voucher);
            }
            return lst;
        }




        public VoucherPrint PrintVoucher(long voucherid)
        {
            List<vouchertype> VoucherTypes = new List<vouchertype>(new vouchertype[] {
               new vouchertype (){ id = -1, name = "Payment Voucher" },
               new vouchertype (){ id=-2, name= "Receipt Voucher" },
               new vouchertype (){ id=-3, name= "Purchase Voucher" },
               new vouchertype (){ id=-4, name= "Sales Voucher" },
               new vouchertype (){ id=-5, name= "Credit Note" },
               new vouchertype (){ id=-6, name= "Debit Note" },
               new vouchertype (){ id=-7, name= "Contra CR" },
               new vouchertype (){ id=-8, name= "Journal CR" },
               new vouchertype (){ id=-9, name= "Cash Deposit" },
               new vouchertype (){ id=-10, name= "Cash Widhrawal" },
               new vouchertype (){ id=-11, name= "Main Cash to Petty Cash" },
               new vouchertype (){ id=-12, name= "Petty Cash to Main Cash" },
               new vouchertype (){ id=-13, name= "Cash Counter Expense" },
               new vouchertype (){ id=-14, name= "Advance Payment Voucher" },
               new vouchertype() { id = -15, name = "Petty Cash Expense" },
               new vouchertype() { id = -16, name = "Journal DR" },
               new vouchertype() { id = -17, name = "Contra CR" }});

            ReferencelistDataService referencelistDataService = new ReferencelistDataService() { Context = this.Context };
            var vouchertypes = referencelistDataService.SearchList(new ReferencelistCreteria() { type = "VOUCHERTYPE" });
            if (vouchertypes != null && vouchertypes.Count > 0)
            {
                vouchertypes.ForEach(x =>
                {
                    VoucherTypes.Add(new vouchertype()
                    {
                        id = x.id,
                        name = x.name
                    });
                });
            }

            VoucherPrint ovoucher = new VoucherPrint()
            {
                childledgers = new List<VoucherPrint.VoucherItemPrint>(),
                taxledgers = new List<VoucherPrint.VoucherItemPrint>()
            };
            List<VoucherPrint.VoucherItemPrint> lstvoucheritem = new List<VoucherPrint.VoucherItemPrint>();

            const string SQL_Voucher = @"SELECT voucher.id, vouchertype,vouchertypeid, voucherdate, voucherno, narration, 
                                    voucher.bwise, amount,ledger.typeid typeid,cast(ledger.attributes->>'hasbank' as boolean) hasbank,ledger.bankid bankid,
                                    adjustmentcredit, adjustmentledgerid, ledger.displayname,voucher.locationid,
                                    voucher.companyid,voucher.narration, voucher.createdon, voucher.modifiedon,voucher.isinterstate,
                                    voucher.adjustmentcredit,voucher.locationid,voucher.employeeid,voucher.companyid,
                                    voucher.attributes,voucher.modifiedby,voucher.createdby,referencelist.name purchasetype
                                    FROM voucher 
                                    INNER JOIN ledger ON voucher.adjustmentledgerid = ledger.id 
                                    LEFT OUTER JOIN referencelist on referencelist.id = purchasetypeid
                                    WHERE voucher.isactive = true AND voucher.id = @id";

            const string SQL_Voucheritem = @"SELECT amount, iscredit, ledger.displayname,
                                    cast(voucheritems.attributes->>'hasgst' as boolean) hasgst,
                                    voucheritems.rate,voucheritems.qty,ledger.taxpercentage,
                                    voucheritems.taxable,ledger.taxmode,ledger.typeid,
                                    cast(ledger.attributes->>'issupplier' as boolean) issupplier,ledger.supplierid,
                                    cast(ledger.attributes->>'hasbank' as boolean) hasbank,
                                    ledger.hsncode,ledger.bankid,voucheritems.ledgerid
                                    FROM voucheritems 
                                    INNER JOIN ledger ON voucheritems.ledgerid = ledger.id
                                    WHERE voucherid=@voucherid AND voucheritems.isactive = true";

            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                command = GetCommand(SQL_Voucher, conn);
                command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = voucherid;
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        ovoucher.vouchertype = reader["vouchertype"] == DBNull.Value ? 0 : Convert.ToInt64(reader["vouchertype"]);
                        ovoucher.vouchertypeid = reader["vouchertypeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["vouchertypeid"]);
                        if (ovoucher.vouchertypeid > 0)
                            ovoucher.displaytitle = VoucherTypes.Find(e => e.id == ovoucher.vouchertypeid).name;
                        else
                            ovoucher.displaytitle = VoucherTypes.Find(e => e.id == ovoucher.vouchertype * -1).name;
                        ovoucher.voucherdate = reader["voucherdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["voucherdate"]);
                        ovoucher.voucherno = reader["voucherno"] == DBNull.Value ? "" : reader["voucherno"].ToString();
                        ovoucher.narration = reader["narration"] == DBNull.Value ? "" : reader["narration"].ToString();
                        ovoucher.purchasetype = reader["purchasetype"] == DBNull.Value ? "" : reader["purchasetype"].ToString();
                        ovoucher.amount = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"]);
                        ovoucher.adjustmentledgerid = reader["adjustmentledgerid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["adjustmentledgerid"]);
                        ovoucher.companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"]);
                        ovoucher.locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                        ovoucher.bwise = reader["bwise"] == DBNull.Value ? 0 : Convert.ToInt32(reader["bwise"]);
                        ovoucher.isinterstate = reader["isinterstate"] == DBNull.Value ? false : Convert.ToBoolean(reader["isinterstate"]);

                        ovoucher.attributes_json = reader["attributes"] == DBNull.Value ? "null" : reader["attributes"].ToString();
                        ovoucher.employeeid = reader["employeeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["employeeid"]);
                        ovoucher.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        ovoucher.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        ovoucher.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        ovoucher.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        ovoucher.adjustmentcredit = reader["adjustmentcredit"] == DBNull.Value ? false : Convert.ToBoolean(reader["adjustmentcredit"]);
                        ovoucher.adjustmentledgertypeid = reader["typeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["typeid"]);
                        if (reader["hasbank"] != DBNull.Value && Convert.ToBoolean(reader["hasbank"]))
                        {
                            ovoucher.bankid = reader["bankid"] == DBNull.Value ? 0 : Convert.ToInt32(reader["bankid"]);
                        }

                        VoucherPrint.VoucherItemPrint item = new VoucherPrint.VoucherItemPrint();
                        item.name = reader["displayname"] == DBNull.Value ? "" : reader["displayname"].ToString();
                        bool iscredit = reader["adjustmentcredit"] == DBNull.Value ? false : Convert.ToBoolean(reader["adjustmentcredit"]);
                        if (iscredit)
                            item.credit = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"]);
                        else
                            item.debit = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"]);
                        item.isparty = true;
                        ovoucher.parentledger = item;
                        lstvoucheritem.Add(item);
                    }
                }

                LedgerDataService ledger = new LedgerDataService() { Context = this.Context };
                ReferencelistDataService location = new ReferencelistDataService() { Context = this.Context };
                CompanyDataService company = new CompanyDataService() { Context = this.Context };
                EmployeeDataService employee = new EmployeeDataService() { Context = this.Context };
                ovoucher.ledger = ledger.Search(new LedgerCreteria() { id = ovoucher.adjustmentledgerid }).First();
                ovoucher.ledger.parseaddress();
                if (ovoucher.companyid > 0)
                    ovoucher.company = company.Search(new CompanyCreteria() { id = ovoucher.companyid }).First();
                if (ovoucher.locationid > 0)
                {
                    var locationdata = location.Search(new ReferencelistCreteria() { id = ovoucher.locationid }).First();

                    ovoucher.location = new CompanyLocation(locationdata);
                    if (locationdata.additionalinfo != null && locationdata.additionalinfo.locationconfig != null
                        && locationdata.additionalinfo.locationconfig.cityid > 0)
                    {
                        var cityinfo = referencelistDataService.Search(new ReferencelistCreteria()
                        {
                            id = locationdata.additionalinfo.locationconfig.cityid
                        });
                        if (cityinfo.Count > 0)
                            ovoucher.location.cityname = cityinfo[0].name;
                    }
                    if (locationdata.additionalinfo != null && locationdata.additionalinfo.locationconfig != null
                        && locationdata.additionalinfo.locationconfig.stateid > 0)
                    {
                        var stateinfo = referencelistDataService.Search(new ReferencelistCreteria()
                        {
                            id = locationdata.additionalinfo.locationconfig.stateid
                        });
                        if (stateinfo.Count > 0)
                            ovoucher.location.statename = stateinfo[0].name;
                    }

                }
                if (ovoucher.employeeid > 0)
                    ovoucher.employee = employee.Search(new EmployeeCreteria()
                    {
                        id = ovoucher.employeeid,
                        hidesalaryinfo = true
                    }).First();
                if (ovoucher.ledger.stateid > 0)
                    ovoucher.ledger.State = location.SearchList(new ReferencelistCreteria() { id = ovoucher.ledger.stateid }).First();

                command = GetCommand(SQL_Voucheritem, conn);
                command.Parameters.Add("voucherid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = voucherid;
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        VoucherPrint.VoucherItemPrint item = new VoucherPrint.VoucherItemPrint();
                        item.hasgst = reader["hasgst"] == DBNull.Value ? false : Convert.ToBoolean(reader["hasgst"]);
                        item.taxmode = reader["taxmode"] == DBNull.Value ? 0 : Convert.ToInt32(reader["taxmode"]);
                        item.typeid = reader["typeid"] == DBNull.Value ? 0 : Convert.ToInt32(reader["typeid"]);
                        item.name = reader["displayname"] == DBNull.Value ? "" : reader["displayname"].ToString();
                        item.hsncode = reader["hsncode"] == DBNull.Value ? "" : reader["hsncode"].ToString();

                        if (reader["issupplier"] != DBNull.Value && Convert.ToBoolean(reader["issupplier"]))
                        {
                            item.supplierid = reader["supplierid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["supplierid"]);
                        }

                        if (reader["hasbank"] != DBNull.Value && Convert.ToBoolean(reader["hasbank"]))
                        {
                            item.bankid = reader["bankid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["bankid"]);
                        }

                        item.ledgerid = reader["ledgerid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["ledgerid"]);


                        if (item.typeid == 16 && !item.hasgst)
                            item.hasgst = true;

                        if (item.hasgst)
                        {
                            item.hasgst = (item.typeid == 16 &&
                            item.taxmode >= 3 &&
                            item.taxmode <= 5);
                        }

                        if (item.typeid == 16 && item.taxmode == 6)
                        {
                            item.hastds = true;
                        }

                        if (item.typeid == 16 && item.taxmode == 7)
                        {
                            item.hastcs = true;
                        }

                        item.rate = reader["rate"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["rate"]);
                        item.qty = reader["qty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["qty"]);
                        item.taxpercentage = reader["taxpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["taxpercentage"]);
                        item.taxable = reader["taxable"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["taxable"]);

                        bool iscredit = reader["iscredit"] == DBNull.Value ? false : Convert.ToBoolean(reader["iscredit"]);
                        if (iscredit)
                            item.credit = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"]);
                        else
                            item.debit = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"]);

                        if (item.typeid == 30)
                        {
                            ovoucher.rounding = item.debit + item.credit;
                            if (iscredit == ovoucher.adjustmentcredit)
                                ovoucher.rounding = ovoucher.rounding * -1;
                        }
                        else
                        {
                            lstvoucheritem.Add(item);
                            ovoucher.childledgers.Add(item);
                        }
                    }
                }

                var ledgers = ledger.Search(new LedgerCreteria() { ids = ovoucher.childledgers.ConvertAll(x => x.ledgerid) });
                lstvoucheritem.ForEach(x =>
                {
                    if (ledgers.Exists(y => y.id == x.ledgerid))
                    {
                        x.ledger = ledgers.Find(y => y.id == x.ledgerid);
                        x.ledger.parseaddress();
                    }
                });

                if (ovoucher.attributes != null &&
                    ovoucher.attributes.hasinv)
                {
                    var invledger = lstvoucheritem.Find(x => x.ledgerid == ovoucher.attributes.invledgerid);
                    lstvoucheritem.Remove(invledger);
                    VoucheritemstockDataService subVoucherstockitems = new VoucheritemstockDataService() { Context = this.Context };
                    var invrows = subVoucherstockitems.SearchAll(voucherid);
                    var invledgers = ledger.Search(new LedgerCreteria()
                    {
                        ids = invrows.ConvertAll(x => x.ledgerid)
                    });
                    foreach (var irows in invrows)
                    {
                        var iledger = invledgers.Find(x => x.id == irows.ledgerid);
                        lstvoucheritem.Add(new VoucherPrint.VoucherItemPrint()
                        {
                            ledgerid = irows.ledgerid,
                            rate = irows.rate == 0 ? irows.amount : irows.rate,
                            credit = invledger.credit != 0 ? irows.amount : 0,
                            debit = invledger.debit != 0 ? irows.amount : 0,
                            qty = irows.qty == 0 ? 1 : irows.qty,
                            taxpercentage = irows.taxpercentage,
                            taxable = irows.taxable,
                            name = iledger.displayname,
                            ledger = iledger,
                            hsncode = iledger.hsncode,
                            typeid = iledger.typeid,
                            taxmode = iledger.taxmode
                        });
                    }
                }

                ovoucher.voucheritem = lstvoucheritem;
                ovoucher.taxledgers = ovoucher.childledgers.FindAll(x => x.hasgst);
                ovoucher.taxableledgers = ovoucher.childledgers.FindAll(x => !x.hasgst);
                if (ovoucher.attributes != null && ovoucher.attributes.hasrcm)
                {
                    ovoucher.childledgers.RemoveAll(x => x.hasgst);
                    ovoucher.voucheritem.RemoveAll(x => x.hasgst);
                }

                if (lstvoucheritem.Exists(x => x.hastds))
                {
                    ovoucher.tdsledger = ovoucher.childledgers.First(x => x.hastds);
                    ovoucher.childledgers.RemoveAll(x => x.hastds);
                    ovoucher.voucheritem.RemoveAll(x => x.hastds);
                }

                if (lstvoucheritem.Exists(x => x.hastcs))
                {
                    ovoucher.tcsledger = ovoucher.childledgers.First(x => x.hastcs);
                    ovoucher.childledgers.RemoveAll(x => x.hastcs);
                    ovoucher.voucheritem.RemoveAll(x => x.hastcs);
                }

                ovoucher.transactions = ovoucher.voucheritem.FindAll(x => !x.isparty && !x.hasgst);

                ovoucher.partyledger = ovoucher.ledger;
                if (ovoucher.transactions.Count == 1 &&
                    (ovoucher.adjustmentledgertypeid == 7))
                {
                    ovoucher.isbankpayment = true;
                    ovoucher.partyledger = ovoucher.transactions[0].ledger;
                }

                if (ovoucher.transactions.Count == 1 &&
                    (ovoucher.adjustmentledgertypeid == 6))
                {
                    ovoucher.iscashpayment = true;
                    ovoucher.partyledger = ovoucher.transactions[0].ledger;
                }

                if (ovoucher.createdby > 0)
                {
                    using (var reader = ExecuteReader("select employee.name from users inner join employee on employeeid = employee.id where users.id=" + ovoucher.createdby, null))
                    {
                        if (reader.Read())
                            ovoucher.enteredby = reader.IsDBNull(0) ? "" : reader.GetString(0);
                    }
                }

                if (ovoucher.modifiedby > 0)
                {
                    using (var reader = ExecuteReader("select employee.name from users inner join employee on employeeid = employee.id where users.id=" + ovoucher.modifiedby, null))
                    {
                        if (reader.Read())
                            ovoucher.alteredby = reader.IsDBNull(0) ? "" : reader.GetString(0);
                    }
                }

                ovoucher.gross = ovoucher.transactions.Sum(x => x.amount);
                ovoucher.igst = ovoucher.taxledgers.Sum(x => x.taxmode == 3 ? x.credit + x.debit : 0);
                ovoucher.sgst = ovoucher.taxledgers.Sum(x => x.taxmode == 4 ? x.credit + x.debit : 0);
                ovoucher.cgst = ovoucher.taxledgers.Sum(x => x.taxmode == 5 ? x.credit + x.debit : 0);
                if (ovoucher.tdsledger != null)
                    ovoucher.tds = ovoucher.tdsledger.debit + ovoucher.tdsledger.credit;
                if (ovoucher.tcsledger != null)
                    ovoucher.tcs = ovoucher.tcsledger.debit + ovoucher.tcsledger.credit;


                if (ovoucher.vouchertype == 1 && ovoucher.adjustmentledgertypeid == 7 &&
                    lstvoucheritem.Exists(x => x.typeid == 12 || x.typeid == 11) &&
                    ovoucher.transactions.Count == 1)
                {

                }
                ovoucher.payable = (ovoucher.gross + ovoucher.igst + ovoucher.cgst + ovoucher.sgst + ovoucher.tcs);
                ovoucher.netpayable = (ovoucher.gross + ovoucher.igst + ovoucher.cgst + ovoucher.sgst + ovoucher.tcs) - ovoucher.tds + ovoucher.rounding;
                ovoucher.numberinword = WebAPI.Data.Numberconvertor.rupees(ovoucher.netpayable);
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, voucherid);
            }
            return ovoucher;
        }

        public Voucher Insert(Voucher oVoucher)
        {
            if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.fin_lockdays > 0)
            {
                if (DateTimeNow.Subtract(oVoucher.voucherdate).TotalDays > InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.fin_lockdays)
                {
                    if (!Context.HasPermission(PermissionSet.advfinance, InfyPOS.Common.Action.View))
                    {
                        throw new Exception("You don't have permission to create old vouchers beyond the limit");
                    }
                }
            }

            NpgsqlTransaction transaction = null;
            try
            {
                if (oVoucher.voucheritems.FindAll(x => x.hastax && x.taxtype == "TDS").GroupBy(x => x.ledgerid).ToList().Exists(x => x.Count() > 1))
                {
                    throw new Exception("Single voucher can't have multiple time same TDS ledger transaction");
                }

                if (oVoucher.vouchertype == 4 && (oVoucher.sourcetype == 3 || oVoucher.sourcetype == 4 || oVoucher.sourcetype == 5))
                {
                    var param = new SortedList<string, object>();
                    param.Add("companyid", oVoucher.companyid);
                    param.Add("locationid", oVoucher.locationid);
                    param.Add("sourcetype", oVoucher.sourcetype);
                    param.Add("voucherdate", oVoucher.voucherdate);
                    param.Add("adjustmentledgerid", oVoucher.adjustmentledgerid);
                    using (var reader = ExecuteReader(@"select voucherno,voucherdate from voucher 
                            where voucher.isactive=true and vouchertype=4 and companyid=@companyid 
                                  and adjustmentledgerid=@adjustmentledgerid and voucherdate=@voucherdate
                                  and locationid=@locationid and sourcetype=@sourcetype", param))
                    {
                        if (reader.Read())
                        {
                            throw new Exception("Already this " + (oVoucher.sourcetype == 3 ? "Cash" : (oVoucher.sourcetype == 4 ? "Bank" : "UPI")) + " voucher entered on " + Convert.ToDateTime(reader["voucherdate"]).ToString("dd-MM-yyyy") + ", Voucher No " +
                                Convert.ToString(reader["voucherno"]));
                        }
                    }
                }
                if (oVoucher.vouchertype == 3 &&
                    oVoucher.attributes != null &&
                    oVoucher.attributes.invoicedate != DateTime.MinValue &&
                    !string.IsNullOrEmpty(oVoucher.attributes.invoiceno))
                {
                    var param = new SortedList<string, object>();
                    param.Add("adjustmentledgerid", oVoucher.adjustmentledgerid);
                    param.Add("invoiceno", oVoucher.attributes.invoiceno);
                    param.Add("invoicedate", oVoucher.attributes.invoicedate);

                    using (var reader = ExecuteReader(@"select voucherno,voucherdate from voucher 
                            where voucher.isactive=true and adjustmentledgerid=@adjustmentledgerid and vouchertype=3
                            and attributes->>'invoiceno' = @invoiceno
                            and cast(attributes->>'invoicedate' as timestamp) = @invoicedate", param))
                    {
                        if (reader.Read())
                        {
                            throw new Exception("Already this voucher entered on " + Convert.ToDateTime(reader["voucherdate"]).ToString("dd-MM-yyyy") + ", Voucher No " +
                                Convert.ToString(reader["voucherno"]));
                        }
                    }
                }

                var vouchertype = oVoucher.vouchertype;
                if ((vouchertype >= 9 && vouchertype < 13) || vouchertype == 99)
                    vouchertype = 0;

                if (oVoucher.adjustmentledgerid <= 0)
                    throw new Exception("Please select the party ledger");


                if (oVoucher.voucheritems.Exists(x => x.ledgerid <= 0))
                    throw new Exception("Please enter appropriate ledgers");


                if (oVoucher.attributes != null &&
                    oVoucher.attributes.hasinv &&
                    oVoucher.attributes.invledgerid > 0)
                {
                    var invvoucheritems = oVoucher.voucheritems.FindAll(x => x.attributes != null &&
                        x.attributes.ledgertype == 8);
                    var amt = invvoucheritems.Sum(x => x.amount);
                    var ledgertype = oVoucher.vouchertype == 4 ? 14 : 15;
                    invvoucheritems.ForEach(x => x.isinv = true);
                    oVoucher.voucheritems.Add(new Voucheritems()
                    {
                        attributes = new Voucheritems.voucharattributes() { ledgertype = ledgertype },
                        ledgerid = oVoucher.attributes.invledgerid,
                        amount = invvoucheritems.Sum(x => x.amount),
                        iscredit = invvoucheritems.First().iscredit,
                        isactive = true,
                        createdby = Context.UserID,
                        modifiedby = Context.UserID,
                        createdon = DateTimeNow,
                        modifiedon = DateTimeNow,
                    });
                }
                var sequence = SequenceManager.Instance(Context.OrganizationID).Get("voucher.voucherno",
                    Services.ConfigManager.Instance(Context.OrganizationID).Config.system_voucherno_location ? oVoucher.locationid : 0,
                    Services.ConfigManager.Instance(Context.OrganizationID).Config.system_voucherno_company ? oVoucher.companyid : 0,
                    Services.ConfigManager.Instance(Context.OrganizationID).Config.system_voucherno_type ? vouchertype : 0);
                sequence.Validate(this.Context, "VOUCHER NUMBER", "", true);
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);
                    oVoucher.voucherno = sequence.GetNextNumber(Context, conn, transaction);

                    if (oVoucher.attributes == null)
                        oVoucher.attributes = new Voucher.voucharattributesExtension();
                    if (oVoucher.vouchertype == 4 || oVoucher.vouchertype == 5)
                    {

                        if (string.IsNullOrEmpty(oVoucher.attributes.invoiceno))
                        {
                            oVoucher.attributes.invoiceno = oVoucher.voucherno;
                            oVoucher.attributes.invoicedate = oVoucher.voucherdate;
                        }
                    }

                    if (oVoucher.vouchertype == 14 && oVoucher.voucheritems.Exists(e => e.attributes != null && e.attributes.supplierid > 0))
                    {
                        LrinvoicereturnDataService lrinvoicereturnDataService = new LrinvoicereturnDataService() { Context = this.Context };
                        var lrreturn = lrinvoicereturnDataService.AddAdvance(oVoucher, conn, transaction);
                        oVoucher.attributes.lrinvoicereturnid = lrreturn.id;
                    }

                    if (oVoucher.vouchertype == 14 && oVoucher.voucheritems.Exists(e => e.attributes != null && e.attributes.customerid > 0))
                    {
                        CustomerinvoiceDataService customerinvoiceDataService = new CustomerinvoiceDataService() { Context = this.Context };
                        var lrreturn = customerinvoiceDataService.AddAdvance(oVoucher, conn, transaction);
                        oVoucher.attributes.customerinvoiceid = lrreturn.id;
                    }

                    if (oVoucher.vouchertype == 18 && oVoucher.voucheritems.Exists(e => e.attributes != null && e.attributes.customerid > 0))
                    {
                        CustomerinvoiceDataService customerinvoiceDataService = new CustomerinvoiceDataService() { Context = this.Context };
                        var lrreturn = customerinvoiceDataService.RefundAdvance(oVoucher, conn, transaction);
                        oVoucher.attributes.customerinvoiceid = lrreturn.id;
                    }

                    if (oVoucher.vouchertype == 18 && oVoucher.voucheritems.Exists(e => e.attributes != null && e.attributes.supplierid > 0))
                    {
                        LrinvoicereturnDataService lrinvoicereturnDataService = new LrinvoicereturnDataService() { Context = this.Context };
                        var lrreturn = lrinvoicereturnDataService.AddRefund(oVoucher, conn, transaction);
                        oVoucher.attributes.lrinvoicereturnid = lrreturn.id;
                    }

                    if (oVoucher.vouchertype == 8 && oVoucher.voucheritems.Exists(e => e.attributes != null && e.attributes.supplierid > 0))
                    {
                        LrinvoicereturnDataService lrinvoicereturnDataService = new LrinvoicereturnDataService() { Context = this.Context };
                        var lrreturn = lrinvoicereturnDataService.AddAdvance(oVoucher, conn, transaction);
                        oVoucher.attributes.lrinvoicereturnid = -1;
                    }

                    oVoucher.attributes.hasgst = oVoucher.voucheritems != null &&
                        oVoucher.voucheritems.Count > 0 &&
                        oVoucher.voucheritems.Exists(x => x.hastax && x.attributes != null && x.attributes.hasgst);

                    Insert(transaction, conn, oVoucher);
                    VoucheritemsDataService subVoucheritems = new VoucheritemsDataService() { Context = this.Context };
                    subVoucheritems.Context = Context;

                    //if (oVoucher.voucheritems.Exists(x => x.attributes != null &&
                    //x.attributes.ledgertdsid > 0))
                    //{
                    //    var tdsledgersource = oVoucher.voucheritems.Find(x => x.attributes != null &&
                    //                        x.attributes.ledgertdsid > 0);
                    //    if (oVoucher.voucheritems.Exists(x => x.ledgerid == tdsledgersource.attributes.ledgertdsid))
                    //    {
                    //        oVoucher.voucheritems.Find(x => x.ledgerid == tdsledgersource.attributes.ledgertdsid).sledgerid = tdsledgersource.ledgerid;
                    //    }
                    //}

                    if (oVoucher.attributes.hasinv)
                    {
                        foreach (var item in oVoucher.voucheritems.FindAll(x => !x.isinv))
                        {
                            item.voucherid = oVoucher.id;
                            //item.iscredit = !oVoucher.adjustmentcredit;
                            subVoucheritems.Insert(transaction, conn, oVoucher, item);
                        }
                        if (oVoucher.voucheritems.Exists(x => x.isinv))
                        {
                            foreach (var item in oVoucher.voucheritems.FindAll(x => x.isinv))
                            {
                                item.voucherid = oVoucher.id;
                                VoucheritemstockDataService voucheritemstockDataService = new VoucheritemstockDataService() { Context = this.Context };
                                voucheritemstockDataService.Insert(transaction, conn, oVoucher, item);
                            }
                        }
                    }
                    else
                    {
                        foreach (var item in oVoucher.voucheritems)
                        {
                            item.voucherid = oVoucher.id;
                            //item.iscredit = !oVoucher.adjustmentcredit;
                            subVoucheritems.Insert(transaction, conn, oVoucher, item);
                        }
                    }

                    if (oVoucher.bwise != 0 ||
                       oVoucher.voucheritems.Any(x => x.bwise != 0))
                    {
                        VoucheritembillwiseDataService voucheritembillwiseDataService = new VoucheritembillwiseDataService() { Context = this.Context };
                        voucheritembillwiseDataService.Sync(false, conn, transaction, oVoucher);
                    }

                    if ((oVoucher.vouchertype == 1 || oVoucher.vouchertype == 14 || oVoucher.vouchertype == 7 || oVoucher.vouchertype == 17) &&
                        (oVoucher.attributes != null && oVoucher.attributes.ledgertype == 7) &&
                        (oVoucher.attributes.banktransfermode == 2 || oVoucher.attributes.banktransfermode == 1))
                    {
                        LedgerDataService ledgerDataService = new LedgerDataService() { Context = this.Context };
                        var addledgerids = oVoucher.voucheritems.ConvertAll(x => x.ledgerid);
                        addledgerids.Add(oVoucher.adjustmentledgerid);
                        var ledgers = ledgerDataService.Search(new LedgerCreteria()
                        {
                            ids = addledgerids
                        });
                        List<Bankchequetransaction> bankchequetransactionList = new List<Bankchequetransaction>();
                        var bankledger = ledgers.Find(x => x.id == oVoucher.adjustmentledgerid);
                        var validatetds = (ledgers.Exists(x => x.typeid == 16 && x.taxmode == 6));

                        foreach (var chequeitem in oVoucher.voucheritems)
                        {
                            var ledger = ledgers.Find(x => x.id == chequeitem.ledgerid);
                            var item = new Bankchequetransaction()
                            {
                                accountname = String.IsNullOrEmpty(ledger.billingname) ? ledger.displayname : ledger.billingname,
                                chequedate = oVoucher.attributes.bankdate,
                                chequeno = oVoucher.attributes.bankreference,
                                amount = chequeitem.amount,
                                bankid = bankledger.bankid,
                                entrydate = oVoucher.voucherdate,
                                isactive = true,
                                locationid = oVoucher.locationid,
                                modifiedby = Context.UserID,
                                createdby = Context.UserID,
                                bankname = bankledger.displayname,
                                employeeid = oVoucher.employeeid,
                                companyid = oVoucher.companyid,
                                sourceid = oVoucher.id,
                                sourcesubid = chequeitem.id,
                                referenceno = oVoucher.voucherno,
                                ledgername = ledger.displayname,
                                sourcetype = "VPMT",
                                modifiedon = DateTimeNow,
                                createdon = DateTimeNow
                            };
                            if (validatetds)
                            {
                                if (chequeitem.iscredit == oVoucher.adjustmentcredit)
                                    continue;
                                var adjustamount = oVoucher.voucheritems.Sum(x => x.iscredit == oVoucher.adjustmentcredit ? x.amount : 0);
                                item.amount -= adjustamount;
                            }

                            if (ledger.attributes != null &&
                                    ledger.attributes.bank != null)
                            {
                                if (!string.IsNullOrEmpty(ledger.attributes.bank.accountname))
                                    item.accountname = ledger.attributes.bank.accountname;

                                item.bankbranch = ledger.attributes.bank.branch;
                                item.bankaccountno = ledger.attributes.bank.accountno;
                                item.bankifsc = ledger.attributes.bank.ifsc;
                                if (ledger.supplierid > 0)
                                {
                                    SuppliercompanyDataService suppliercompanyDataService = new SuppliercompanyDataService() { Context = this.Context };
                                    var suppliercompany = suppliercompanyDataService.Search(new SuppliercompanyCreteria() { supplierid = ledger.supplierid });
                                    if (suppliercompany.Count > 0)
                                    {
                                        if (string.IsNullOrEmpty(item.bankbranch))
                                            item.bankbranch = suppliercompany[0].bankbranch;
                                        if (string.IsNullOrEmpty(item.bankaccountno))
                                            item.bankaccountno = suppliercompany[0].bankaccountno;
                                        if (string.IsNullOrEmpty(item.bankifsc))
                                            item.bankifsc = suppliercompany[0].bankifci;
                                    }
                                }
                            }
                            else if (ledger.supplierid > 0)
                            {
                                SuppliercompanyDataService suppliercompanyDataService = new SuppliercompanyDataService() { Context = this.Context };
                                var suppliercompany = suppliercompanyDataService.Search(new SuppliercompanyCreteria() { supplierid = ledger.supplierid });
                                if (suppliercompany.Count > 0)
                                {
                                    if (!string.IsNullOrEmpty(suppliercompany[0].bankaccountname))
                                        item.accountname = suppliercompany[0].bankaccountname;
                                    item.bankbranch = suppliercompany[0].bankbranch;
                                    item.bankaccountno = suppliercompany[0].bankaccountno;
                                    item.bankifsc = suppliercompany[0].bankifci;
                                }
                            }

                            bankchequetransactionList.Add(item);
                        }
                        BankchequetransactionDataService bankchequetransactionDataService = new BankchequetransactionDataService() { Context = this.Context };
                        bankchequetransactionDataService.Update(transaction, conn, bankchequetransactionList, oVoucher.id, "VPMT");
                    }
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oVoucher);

            }

            return oVoucher;
        }

        public Voucher Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Voucher oVoucher)
        {

            NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("uniqueuid", NpgsqlTypes.NpgsqlDbType.Uuid).Value = string.IsNullOrEmpty(oVoucher.uniqueuid) ? Guid.NewGuid().ToString() : oVoucher.uniqueuid;
            command.Parameters.Add("vouchertype", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oVoucher.vouchertype;
            command.Parameters.Add("purchasetypeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oVoucher.purchasetypeid;
            command.Parameters.Add("vouchertypeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oVoucher.vouchertypeid;
            command.Parameters.Add("adjustmentledgerid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oVoucher.adjustmentledgerid;

            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oVoucher.companyid;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oVoucher.locationid;
            command.Parameters.Add("employeeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oVoucher.employeeid;
            command.Parameters.Add("sourcetype", NpgsqlTypes.NpgsqlDbType.Integer).Value = oVoucher.sourcetype;

            command.Parameters.Add("adjustmentcredit", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oVoucher.adjustmentcredit;
            command.Parameters.Add("voucherdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oVoucher.voucherdate;

            command.Parameters.Add("attributes", NpgsqlTypes.NpgsqlDbType.Json).Value = oVoucher.attributes_json;
            command.Parameters.Add("voucherno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oVoucher.voucherno == null ? "" : oVoucher.voucherno;
            command.Parameters.Add("narration", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oVoucher.narration == null ? "" : oVoucher.narration;
            command.Parameters.Add("amount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oVoucher.amount;
            command.Parameters.Add("bwise", NpgsqlTypes.NpgsqlDbType.Integer).Value = oVoucher.bwise;
            //command.Parameters.Add("opening", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oVoucher.opening;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oVoucher.isactive;
            command.Parameters.Add("isinterstate", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oVoucher.isinterstate;

            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oVoucher.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oVoucher.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            return oVoucher;
        }

        public Voucher Update(Voucher oVoucher)
        {
            /*update voucher set isinterstate = true where id in (
select voucher.id  from voucher
inner join ledger on ledger.id = adjustmentledgerid
inner join company on company.id = voucher.companyid
where ledger.gstno is not null 
and ledger.gstno != ''
and substring(ledger.gstno,2) !=  substring(company.tinanddate,2));*/

            NpgsqlTransaction transaction = null;
            try
            {

                if (oVoucher.vouchertype == 3 &&
                    oVoucher.attributes != null &&
                    oVoucher.attributes.invoicedate != DateTime.MinValue &&
                    !string.IsNullOrEmpty(oVoucher.attributes.invoiceno))
                {
                    var param = new SortedList<string, object>();
                    param.Add("adjustmentledgerid", oVoucher.adjustmentledgerid);
                    param.Add("id", oVoucher.id);
                    param.Add("invoiceno", oVoucher.attributes.invoiceno);
                    param.Add("invoicedate", oVoucher.attributes.invoicedate);

                    using (var reader = ExecuteReader(@"select voucherno,voucherdate from voucher 
                            where id != @id and
                            adjustmentledgerid=@adjustmentledgerid and vouchertype=3
                            and attributes->>'invoiceno' = @invoiceno
                            and cast(attributes->>'invoicedate' as timestamp) = @invoicedate", param))
                    {
                        if (reader.Read())
                        {
                            throw new Exception("Already this voucher entered on " + Convert.ToDateTime(reader["voucherdate"]).ToString("dd-MM-yyyy") + ", Voucher No " +
                                Convert.ToString(reader["voucherno"]));
                        }
                    }
                }


                if (oVoucher.attributes != null &&
                    oVoucher.attributes.hasinv &&
                    oVoucher.attributes.invledgerid > 0)
                {
                    var invvoucheritems = oVoucher.voucheritems.FindAll(x => x.attributes != null &&
                        x.attributes.ledgertype == 8);
                    var amt = invvoucheritems.Sum(x => x.amount);
                    var ledgertype = oVoucher.vouchertype == 4 ? 14 : 15;
                    invvoucheritems.ForEach(x =>
                    {
                        x.voucherid = oVoucher.id;
                        x.isinv = true;
                    });
                    oVoucher.voucheritems.Add(new Voucheritems()
                    {
                        attributes = new Voucheritems.voucharattributes() { ledgertype = ledgertype },
                        ledgerid = oVoucher.attributes.invledgerid,
                        amount = invvoucheritems.Sum(x => x.amount),
                        iscredit = invvoucheritems.First().iscredit,
                        isactive = true,
                        createdby = Context.UserID,
                        modifiedby = Context.UserID,
                        createdon = DateTimeNow,
                        modifiedon = DateTimeNow,
                        voucherid = oVoucher.id
                    });
                }

                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    var existings = Search(new VoucherCreteria() { id = oVoucher.id });

                    if (existings[0].attributes != null && (existings[0].attributes.lrinvoicereturnid > 0 || existings[0].attributes.lrinvoicereturnid == -1))
                    {
                        List<long> returnids = new List<long>();
                        returnids.Add(existings[0].attributes.lrinvoicereturnid);
                        var multireturns = existings[0].voucheritems.FindAll(x => x.attributes != null
                        && x.attributes.lrinvoicereturnid != existings[0].attributes.lrinvoicereturnid
                        && x.attributes.lrinvoicereturnid > 0);
                        if (multireturns.Count > 1)
                        {
                            returnids.AddRange(multireturns.ConvertAll(x => x.attributes.lrinvoicereturnid));
                        }

                        LrinvoicereturnDataService lrinvoicereturnDataService = new LrinvoicereturnDataService() { Context = this.Context };
                        foreach (long id in returnids)
                        {
                            decimal paidamount = 0;
                            using (var reader = ExecuteReader("select sum(paidamount) from supplierpaymentinvoice where isactive = true and islrinvoicereturn=true and supplierpaymentinvoice.invoiceid=" +
                                id, new SortedList<string, object>()))
                            {
                                if (reader.Read())
                                {
                                    paidamount = reader.IsDBNull(0) ? 0 : reader.GetDecimal(0);
                                }
                            }
                            if (paidamount != 0)
                            {
                                throw new Exception("This voucher already got settled on supplier payment, can't allow edit");
                            }
                            var lrinvicereturn = lrinvoicereturnDataService.Search(new LrinvoicereturnCreteria()
                            {
                                id = id
                            });
                            if (lrinvicereturn.Count == 1)
                                lrinvoicereturnDataService.Delete(transaction, conn, lrinvicereturn.First());
                        }
                    }

                    if (oVoucher.vouchertype == 14 && oVoucher.voucheritems.Exists(e => e.attributes != null && e.attributes.supplierid > 0))
                    {
                        LrinvoicereturnDataService lrinvoicereturnDataService = new LrinvoicereturnDataService() { Context = this.Context };
                        var lrreturn = lrinvoicereturnDataService.AddAdvance(oVoucher, conn, transaction);
                        if (oVoucher.attributes == null)
                            oVoucher.attributes = new Voucher.voucharattributesExtension();
                        oVoucher.attributes.lrinvoicereturnid = lrreturn.id;
                    }

                    if (oVoucher.vouchertype == 18 && oVoucher.voucheritems.Exists(e => e.attributes != null && e.attributes.supplierid > 0))
                    {
                        LrinvoicereturnDataService lrinvoicereturnDataService = new LrinvoicereturnDataService() { Context = this.Context };
                        var lrreturn = lrinvoicereturnDataService.AddRefund(oVoucher, conn, transaction);
                        oVoucher.attributes.lrinvoicereturnid = lrreturn.id;
                    }

                    if (oVoucher.vouchertype == 8 && oVoucher.voucheritems.Exists(e => e.attributes != null && e.attributes.supplierid > 0))
                    {
                        LrinvoicereturnDataService lrinvoicereturnDataService = new LrinvoicereturnDataService() { Context = this.Context };
                        var lrreturn = lrinvoicereturnDataService.AddAdvance(oVoucher, conn, transaction);
                        oVoucher.attributes.lrinvoicereturnid = -1;
                    }

                    if (oVoucher.attributes != null && oVoucher.attributes.customerinvoiceid > 0)
                    {
                        CustomerinvoiceDataService customerinvoiceDataService = new CustomerinvoiceDataService() { Context = this.Context };
                        decimal paidamount = 0;
                        using (var reader = ExecuteReader("select sum(paidamount) from Customerinvoice where isactive = true and Customerinvoice.id=" +
                            oVoucher.attributes.customerinvoiceid, new SortedList<string, object>()))
                        {
                            if (reader.Read())
                            {
                                paidamount = reader.IsDBNull(0) ? 0 : reader.GetDecimal(0);
                            }
                        }
                        if (paidamount != 0)
                        {
                            throw new Exception("This voucher already got settled on customer payment, can't allow edit");
                        }
                        var customerinvoices = customerinvoiceDataService.Search(new CustomerinvoiceCreteria()
                        {
                            id = oVoucher.attributes.customerinvoiceid
                        });
                        customerinvoiceDataService.Delete(transaction, conn, customerinvoices.First());
                    }

                    if (oVoucher.vouchertype == 14 && oVoucher.voucheritems.Exists(e => e.attributes != null && e.attributes.customerid > 0))
                    {
                        CustomerinvoiceDataService customerinvoiceDataService = new CustomerinvoiceDataService() { Context = this.Context };
                        var lrreturn = customerinvoiceDataService.AddAdvance(oVoucher, conn, transaction);
                        if (oVoucher.attributes == null)
                            oVoucher.attributes = new Voucher.voucharattributesExtension();
                        oVoucher.attributes.customerinvoiceid = lrreturn.id;
                    }

                    if (oVoucher.vouchertype == 18 && oVoucher.voucheritems.Exists(e => e.attributes != null && e.attributes.customerid > 0))
                    {
                        CustomerinvoiceDataService customerinvoiceDataService = new CustomerinvoiceDataService() { Context = this.Context };

                        var lrreturn = customerinvoiceDataService.RefundAdvance(oVoucher, conn, transaction);
                        oVoucher.attributes.customerinvoiceid = lrreturn.id;
                    }

                    if (oVoucher.attributes == null)
                        oVoucher.attributes = new Voucher.voucharattributesExtension();

                    oVoucher.attributes.hasgst = oVoucher.voucheritems != null &&
                    oVoucher.voucheritems.Count > 0 &&
                    oVoucher.voucheritems.Exists(x => x.hastax && x.attributes != null && x.attributes.hasgst);

                    Update(transaction, conn, oVoucher);

                    BankchequetransactionDataService bankchequetransactionDataService = new BankchequetransactionDataService() { Context = this.Context };

                    VoucheritemsDataService subVoucheritems = new VoucheritemsDataService() { Context = this.Context };
                    subVoucheritems.Context = Context;
                    foreach (var item in oVoucher.voucheritems)
                    {
                        //item.iscredit = !oVoucher.adjustmentcredit;
                        item.voucherid = oVoucher.id;
                    }
                    if (oVoucher.voucheritems.Exists(x => x.attributes != null &&
                    x.attributes.ledgertdsid > 0))
                    {
                        var tdsledgersource = oVoucher.voucheritems.Find(x => x.attributes != null &&
                                            x.attributes.ledgertdsid > 0);
                        if (oVoucher.voucheritems.Exists(x => x.ledgerid == tdsledgersource.attributes.ledgertdsid))
                        {
                            oVoucher.voucheritems.Find(x => x.ledgerid == tdsledgersource.attributes.ledgertdsid).sledgerid = tdsledgersource.ledgerid;
                        }
                    }

                    if (oVoucher.attributes.hasinv)
                    {
                        subVoucheritems.Update(transaction, conn, oVoucher.voucheritems.FindAll(x => !x.isinv), oVoucher);
                        VoucheritemstockDataService voucheritemstockDataService = new VoucheritemstockDataService() { Context = this.Context };
                        var invitems = oVoucher.voucheritems.FindAll(x => x.isinv);
                        invitems.ForEach(x => x.id = Math.Abs(x.id));
                        voucheritemstockDataService.Update(transaction, conn, invitems, oVoucher);
                    }
                    else
                    {
                        subVoucheritems.Update(transaction, conn, oVoucher.voucheritems, oVoucher);
                    }

                    VoucheritembillwiseDataService voucheritembillwiseDataService = new VoucheritembillwiseDataService() { Context = this.Context };
                    if (oVoucher.attributes != null && oVoucher.attributes.adjustedvouchervalue > 0)
                    {
                        voucheritembillwiseDataService.Sync(true, conn, transaction, oVoucher);
                    }
                    else
                    {
                        voucheritembillwiseDataService.DeleteBillwiseItems(conn, transaction, oVoucher);
                        if (oVoucher.bwise != 0 ||
                          oVoucher.voucheritems.Any(x => x.bwise != 0))
                        {
                            voucheritembillwiseDataService.Sync(false, conn, transaction, oVoucher);
                        }
                    }

                    bankchequetransactionDataService.DeleteAll(oVoucher.id, "VPMT", conn, transaction);
                    if ((oVoucher.vouchertype == 1 || oVoucher.vouchertype == 14 || oVoucher.vouchertype == 7 || oVoucher.vouchertype == 17) &&
                        (oVoucher.attributes != null && oVoucher.attributes.ledgertype == 7) &&
                        (oVoucher.attributes.banktransfermode == 2 || oVoucher.attributes.banktransfermode == 1))
                    {
                        LedgerDataService ledgerDataService = new LedgerDataService() { Context = this.Context };
                        var addledgerids = oVoucher.voucheritems.ConvertAll(x => x.ledgerid);
                        addledgerids.Add(oVoucher.adjustmentledgerid);
                        var ledgers = ledgerDataService.Search(new LedgerCreteria()
                        {
                            ids = addledgerids
                        });
                        List<Bankchequetransaction> bankchequetransactionList = new List<Bankchequetransaction>();
                        var bankledger = ledgers.Find(x => x.id == oVoucher.adjustmentledgerid);
                        if (bankledger.bankid > 0)
                        {
                            var validatetds = (ledgers.Exists(x => x.typeid == 16 && x.taxmode == 6));

                            foreach (var chequeitem in oVoucher.voucheritems)
                            {
                                var ledger = ledgers.Find(x => x.id == chequeitem.ledgerid);
                                var item = new Bankchequetransaction()
                                {
                                    accountname = String.IsNullOrEmpty(ledger.billingname) ? ledger.displayname : ledger.billingname,
                                    chequedate = oVoucher.attributes.bankdate,
                                    chequeno = oVoucher.attributes.bankreference,
                                    amount = chequeitem.amount,
                                    bankid = bankledger.bankid,
                                    entrydate = oVoucher.voucherdate,
                                    isactive = true,
                                    locationid = oVoucher.locationid,
                                    modifiedby = Context.UserID,
                                    createdby = Context.UserID,
                                    bankname = bankledger.displayname,
                                    employeeid = oVoucher.employeeid,
                                    companyid = oVoucher.companyid,
                                    sourceid = oVoucher.id,
                                    sourcetype = "VPMT",
                                    sourcesubid = chequeitem.id,
                                    referenceno = oVoucher.voucherno,
                                    ledgername = ledger.displayname,
                                    modifiedon = DateTimeNow,
                                    createdon = DateTimeNow
                                };
                                if (validatetds)
                                {
                                    if (chequeitem.iscredit == oVoucher.adjustmentcredit)
                                        continue;
                                    var adjustamount = oVoucher.voucheritems.Sum(x => x.iscredit == oVoucher.adjustmentcredit ? x.amount : 0);
                                    item.amount -= adjustamount;
                                }

                                if (ledger.attributes != null &&
                                ledger.attributes.bank != null)
                                {
                                    if (!string.IsNullOrEmpty(ledger.attributes.bank.accountname))
                                        item.accountname = ledger.attributes.bank.accountname;

                                    item.bankbranch = ledger.attributes.bank.branch;
                                    item.bankaccountno = ledger.attributes.bank.accountno;
                                    item.bankifsc = ledger.attributes.bank.ifsc;
                                    if (ledger.supplierid > 0)
                                    {
                                        SuppliercompanyDataService suppliercompanyDataService = new SuppliercompanyDataService() { Context = this.Context };
                                        var suppliercompany = suppliercompanyDataService.Search(new SuppliercompanyCreteria() { supplierid = ledger.supplierid });
                                        if (suppliercompany.Count > 0)
                                        {
                                            if (string.IsNullOrEmpty(item.bankbranch))
                                                item.bankbranch = suppliercompany[0].bankbranch;
                                            if (string.IsNullOrEmpty(item.bankaccountno))
                                                item.bankaccountno = suppliercompany[0].bankaccountno;
                                            if (string.IsNullOrEmpty(item.bankifsc))
                                                item.bankifsc = suppliercompany[0].bankifci;
                                        }
                                    }
                                }
                                else if (ledger.supplierid > 0)
                                {
                                    SuppliercompanyDataService suppliercompanyDataService = new SuppliercompanyDataService() { Context = this.Context };
                                    var suppliercompany = suppliercompanyDataService.Search(new SuppliercompanyCreteria() { supplierid = ledger.supplierid });
                                    if (suppliercompany.Count > 0)
                                    {
                                        if (!string.IsNullOrEmpty(suppliercompany[0].bankaccountname))
                                            item.accountname = suppliercompany[0].bankaccountname;
                                        item.bankbranch = suppliercompany[0].bankbranch;
                                        item.bankaccountno = suppliercompany[0].bankaccountno;
                                        item.bankifsc = suppliercompany[0].bankifci;
                                    }
                                }
                                bankchequetransactionList.Add(item);
                            }

                            bankchequetransactionDataService.Update(transaction, conn, bankchequetransactionList, oVoucher.id, "VPMT");
                        }
                    }
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oVoucher);
            }
            return oVoucher;
        }

        public Voucher Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Voucher oVoucher)
        {
            if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.fin_lockdays > 0)
            {
                if (DateTimeNow.Subtract(oVoucher.voucherdate).TotalDays > InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.fin_lockdays)
                {
                    if (!Context.HasPermission(PermissionSet.advfinance, InfyPOS.Common.Action.View))
                    {
                        throw new Exception("You don't have permission to update old vouchers");
                    }
                }
            }

            NpgsqlCommand command = GetCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oVoucher.id;
            command.Parameters.Add("vouchertype", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oVoucher.vouchertype;
            command.Parameters.Add("purchasetypeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oVoucher.purchasetypeid;
            command.Parameters.Add("vouchertypeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oVoucher.vouchertypeid;
            command.Parameters.Add("adjustmentledgerid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oVoucher.adjustmentledgerid;
            command.Parameters.Add("adjustmentcredit", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oVoucher.adjustmentcredit;
            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oVoucher.companyid;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oVoucher.locationid;
            command.Parameters.Add("employeeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oVoucher.employeeid;
            command.Parameters.Add("sourcetype", NpgsqlTypes.NpgsqlDbType.Integer).Value = oVoucher.sourcetype;
            //command.Parameters.Add("opening", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oVoucher.opening;
            command.Parameters.Add("voucherdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oVoucher.voucherdate;
            command.Parameters.Add("voucherno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oVoucher.voucherno == null ? "" : oVoucher.voucherno;
            command.Parameters.Add("narration", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oVoucher.narration == null ? "" : oVoucher.narration;
            command.Parameters.Add("attributes", NpgsqlTypes.NpgsqlDbType.Json).Value = oVoucher.attributes_json;
            command.Parameters.Add("bwise", NpgsqlTypes.NpgsqlDbType.Integer).Value = oVoucher.bwise;
            command.Parameters.Add("amount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oVoucher.amount;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oVoucher.isactive;
            command.Parameters.Add("isinterstate", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oVoucher.isinterstate;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oVoucher.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oVoucher.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oVoucher.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oVoucher.version = Convert.ToInt32(reader[0].ToString());
                }
            }
            return oVoucher;
        }


        public bool Delete(Voucher oVoucher)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    if (oVoucher.attributes != null && oVoucher.attributes.lrinvoicereturnid > 0)
                    {
                        LrinvoicereturnDataService lrinvoicereturnDataService = new LrinvoicereturnDataService() { Context = this.Context };
                        decimal paidamount = 0;
                        using (var reader = ExecuteReader("select sum(paidamount) from supplierpaymentinvoice where isactive = true and islrinvoicereturn=true and  supplierpaymentinvoice.invoiceid=" +
                            oVoucher.attributes.lrinvoicereturnid, new SortedList<string, object>()))
                        {
                            if (reader.Read())
                            {
                                paidamount = reader.IsDBNull(0) ? 0 : reader.GetDecimal(0);
                            }
                        }
                        if (paidamount > 0)
                        {
                            throw new Exception("This voucher already got settled on supplier payment, can't allow delete");
                        }
                        var lrinvicereturn = lrinvoicereturnDataService.Search(new LrinvoicereturnCreteria()
                        {
                            id = oVoucher.attributes.lrinvoicereturnid
                        });
                        lrinvoicereturnDataService.Delete(transaction, conn, lrinvicereturn.First());
                    }

                    hasSuccessCount = Delete(transaction, conn, oVoucher);
                    VoucheritemsDataService subVoucheritems = new VoucheritemsDataService() { Context = this.Context };
                    subVoucheritems.DeleteAll(oVoucher.id, oVoucher.modifiedby, conn, transaction);

                    if (oVoucher.attributes != null &&
                        oVoucher.attributes.hasinv)
                    {
                        VoucheritemstockDataService voucheritemstockDataService = new VoucheritemstockDataService() { Context = this.Context };
                        voucheritemstockDataService.DeleteAll(oVoucher.id, oVoucher.modifiedby, conn, transaction);
                    }
                    VoucheritembillwiseDataService voucheritembillwiseDataService = new VoucheritembillwiseDataService() { Context = this.Context };
                    voucheritembillwiseDataService.DeleteBillwiseItems(conn, transaction, oVoucher);


                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oVoucher);
            }
            return hasSuccessCount;

        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Voucher oVoucher)
        {

            NpgsqlCommand command = GetCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oVoucher.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oVoucher.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oVoucher.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

        internal InfyGST.GSTPortal.EwayFromGRNResponse CancelEInvoice(VoucherCreteria oVoucher)
        {
            Gst_InboxDataService gst_InboxDataService = new Gst_InboxDataService() { Context = this.Context };
            var ewayresonse = gst_InboxDataService.CancelEinvoice(oVoucher);
            if (ewayresonse.Status == "1")
            {
                if (oVoucher.attributes == null)
                    oVoucher.attributes = new Voucher.voucharattributesExtension();
                oVoucher.attributes.einvcan = true;
                oVoucher.attributes.einvcandetail = Newtonsoft.Json.JsonConvert.SerializeObject(ewayresonse);
                string query = @"update voucher set attributes=@attributes where id=@id";
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    var transaction = conn.BeginTransaction();
                    NpgsqlCommand command = new NpgsqlCommand(query, conn);
                    command.Parameters.Add("attributes", NpgsqlTypes.NpgsqlDbType.Json).Value = oVoucher.attributes_json;
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oVoucher.id;
                    command.ExecuteNonQuery();
                    transaction.Commit();
                    conn.Close();
                }
            }
            return ewayresonse;
        }

        internal InfyGST.GSTPortal.EInvoiceResponse GenerateEInvoice(VoucherCreteria item)
        {
            Gst_InboxDataService gst_InboxDataService = new Gst_InboxDataService() { Context = this.Context };

            InfyGST.GSTPortal.EInvoiceResponse response = null;

            InfyPOS.Data.VoucherDataService billDataService = new VoucherDataService() { Context = this.Context };
            var bills = billDataService.Search(new VoucherCreteria()
            {
                id = item.id
            });

            response = gst_InboxDataService.CreateEInvoice4Voucher(bills[0], true);

            if (response.ErrorDetails == null || response.ErrorDetails.Count == 0)
            {
                if (!string.IsNullOrEmpty(response.Irn))
                {
                    if (bills[0].attributes == null) bills[0].attributes = new Voucher.voucharattributesExtension();
                    bills[0].attributes.einv = response.Irn;
                    bills[0].attributes.eack = response.AckNo;
                    bills[0].attributes.einvdetail = Newtonsoft.Json.JsonConvert.SerializeObject(response);
                    string query = @"update voucher set attributes=@attributes where id=@id";
                    using (NpgsqlConnection conn = CreateConnection())
                    {
                        conn.Open();
                        var transaction = conn.BeginTransaction();
                        NpgsqlCommand command = new NpgsqlCommand(query, conn);
                        command.Parameters.Add("attributes", NpgsqlTypes.NpgsqlDbType.Json).Value = bills[0].attributes_json;
                        command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = bills[0].id;
                        command.ExecuteNonQuery();
                        transaction.Commit();
                        conn.Close();
                    }
                    response.Voucher = bills[0];
                }
            }
            return response;
        }

        internal FinanceAnalyse Analyser(FinanceAnalyseCreteria item)
        {
            FinanceAnalyse response = new FinanceAnalyse();
            switch (item.modeid)
            {
                case FinanceAnalyseCreteria.Modes.CompanySummary:
                case FinanceAnalyseCreteria.Modes.LocationSummary:
                    response.summary = BuildSummary(item);
                    List<FinanceAnalyse.Summary.Value> allValues = new List<FinanceAnalyse.Summary.Value>();
                    response.summary.ForEach(x => allValues.AddRange(x.values));
                    response.chart = allValues.GroupBy(x => x.lid).ToList().ConvertAll(y => new FinanceAnalyse.Summary.Value()
                    {
                        lid = y.Key,
                        cr = y.Sum(z => z.cr),
                        dr = y.Sum(z => z.dr)
                    }).ToList();
                    break;
                case FinanceAnalyseCreteria.Modes.Compare:
                    response.title1 = item.fromdate.ToString("dd-MM-yyyy") + '-' + item.todate.ToString("dd-MM-yyyy");
                    response.title2 = item.comparefromdate.ToString("dd-MM-yyyy") + '-' + item.comparetodate.ToString("dd-MM-yyyy");
                    response.summary = CompareSummary(item);
                    break;
                case FinanceAnalyseCreteria.Modes.Monthly:
                    response = MonthlySummary(item);
                    break;
                case FinanceAnalyseCreteria.Modes.Detail:
                    response.detail = BuildDetail(item);
                    break;
                case FinanceAnalyseCreteria.Modes.TaxSummary:
                    response = TaxSummary(item);
                    break;
                case FinanceAnalyseCreteria.Modes.TaxDetails:
                    response = TaxDetails(item);
                    break;
                case FinanceAnalyseCreteria.Modes.VoucherSummary:
                case FinanceAnalyseCreteria.Modes.VoucherDetail:
                    var expinctypes = new List<long>(new long[] { 4, 5, 9, 10 });
                    var sundrycredtors = new List<long>(new long[] { 19, 20, 12 });
                    var sundrydebtors = new List<long>(new long[] { 11 });
                    var fixedassets = new List<long>(new long[] { 2 });
                    var taxids = new List<long>(new long[] { 16 });
                    var processtaxable = true;
                    if (item.types != null && item.types.Count > 0 &&
                        item.types.All(x => expinctypes.Contains(x)))
                    {
                        processtaxable = false;
                        response.vsummary = BuildExpInSummary(item, expinctypes, BuildInvoiceDetail(item, expinctypes));
                    }
                    else if (item.types != null && item.types.Count > 0 &&
                        item.types.All(x => sundrycredtors.Contains(x)))
                    {
                        processtaxable = false;
                        response.vsummary = BuildCrediorsSummary(item, sundrycredtors, BuildInvoiceDetail(item, sundrycredtors));
                    }
                    else if (item.types != null && item.types.Count > 0 &&
                        item.types.All(x => fixedassets.Contains(x)))
                    {
                        processtaxable = false;
                        response.vsummary = BuildFixedAssetSummary(item, fixedassets, BuildInvoiceDetail(item, fixedassets));
                    }
                    else if (item.types != null && item.types.Count > 0 &&
                        item.types.All(x => sundrydebtors.Contains(x)))
                    {
                        processtaxable = false;
                        response.vsummary = BuildDebitorsSummary(item, sundrydebtors, BuildInvoiceDetail(item, sundrydebtors));
                    }
                    else if (item.types != null && item.types.Count > 0 &&
                        item.types.All(x => taxids.Contains(x)))
                    {
                        processtaxable = false;
                        response.vsummary = BuildGSTSummary(item, taxids, BuildInvoiceDetail(item, taxids));
                    }
                    else
                    {
                        response.vsummary = BuildSummary(item, BuildInvoiceDetail(item, new List<long>()));
                    }

                    if (processtaxable)
                    {
                        foreach (var taxablelist in response.vsummary.FindAll(x => x.sgst > 0 || x.cgst > 0 || x.igst > 0))
                        {
                            taxablelist.txble = (taxablelist.cr + taxablelist.dr);
                            if (taxablelist.cr > 0)
                            {
                                taxablelist.cr += (taxablelist.igst + taxablelist.sgst + taxablelist.cgst) - (taxablelist.tds);
                            }
                            else
                            {
                                taxablelist.dr += (taxablelist.igst + taxablelist.sgst + taxablelist.cgst) - (taxablelist.tds);
                            }
                        }
                    }

                    if (response.vsummary != null &&
                        response.vsummary.Exists(x => x.crby > 0))
                    {
                        EmployeeDataService usersDataService = new EmployeeDataService() { Context = this.Context };
                        var emplist = usersDataService.GetUserName(response.vsummary.ConvertAll(x => x.crby).Distinct().ToList());
                        response.vsummary.GroupBy(x => x.crby).ToList().ForEach(x =>
                        {
                            if (x.Key > 0 &&
                                emplist.Exists(y => y.id == x.Key))
                            {
                                var ename = emplist.Find(y => y.id == x.Key).name;
                                x.ToList().ForEach(z => z.crbname = ename);
                            }
                        });
                    }
                    break;
            }
            return response;
        }

        public List<FinanceAnalyse.VoucherSummary> BuildGSTSummary(FinanceAnalyseCreteria item, List<long> expinctypes, List<FinanceAnalyse.VoucherDetail> sourcelist)
        {
            //sourcelist = sourcelist.FindAll(x => x.vid == 18288);
            var ledgerids = sourcelist.ConvertAll(x => x.id).Distinct().ToList();
            LedgerDataService ledgerDataService = new LedgerDataService() { Context = this.Context };
            var ledgers = ledgerDataService.Search(new LedgerCreteria()
            {
                ids = ledgerids
            });
            var inexpledgers = ledgers.FindAll(x => expinctypes.Contains(x.typeid)).ConvertAll(x => x.id);

            var taxledgers = ledgers.FindAll(x => x.typeid == 16 && x.taxmode >= 1 && x.taxmode <= 7);

            var dutyledgers = taxledgers.FindAll(x => x.typeid == 16 && x.taxmode == 1).ConvertAll(x => x.id).ToList();
            var cessledgers = taxledgers.FindAll(x => x.typeid == 16 && x.taxmode == 2).ConvertAll(x => x.id).ToList();
            var igstledgers = taxledgers.FindAll(x => x.typeid == 16 && x.taxmode == 3).ConvertAll(x => x.id).ToList();
            var sgstledgers = taxledgers.FindAll(x => x.typeid == 16 && x.taxmode == 4).ConvertAll(x => x.id).ToList();
            var cgstledgers = taxledgers.FindAll(x => x.typeid == 16 && x.taxmode == 5).ConvertAll(x => x.id).ToList();
            var tdsledgers = taxledgers.FindAll(x => x.typeid == 16 && x.taxmode == 6).ConvertAll(x => x.id).ToList();
            var tcsledgers = taxledgers.FindAll(x => x.typeid == 16 && x.taxmode == 7).ConvertAll(x => x.id).ToList();
            var rndledgers = ledgers.FindAll(x => x.typeid == 30).ConvertAll(x => x.id).ToList();
            var gstledgers = taxledgers.FindAll(x => x.typeid == 16 && (x.taxmode >= 3 && x.taxmode <= 5)).ConvertAll(x => x.id).ToList();
            var taxledgerids = ledgers.FindAll(x => x.typeid == 16 && x.taxmode >= 1 && x.taxmode <= 7).ConvertAll(x => x.id).ToList();
            taxledgers.RemoveAll(x => !taxledgerids.Contains(x.id));
            List<FinanceAnalyse.VoucherSummary> result = new List<FinanceAnalyse.VoucherSummary>();
            foreach (var lst in sourcelist.GroupBy(x => x.vid))
            {
                var voucheritems = lst.ToList();
                var taxvoucheritems = voucheritems.FindAll(x => taxledgerids.Contains(x.id));
                var gstvoucheritems = voucheritems.FindAll(x => gstledgers.Contains(x.id));
                if (gstvoucheritems.Count == 0) continue;

                var tdsvoucheritems = voucheritems.FindAll(x => tdsledgers.Contains(x.id));
                var otherledgers = voucheritems.FindAll(x => !taxledgerids.Contains(x.id));

                var partyledgers = otherledgers.FindAll(x => x.vvid == 0);
                var adjustledgers = otherledgers.FindAll(x => x.vvid != 0 && !rndledgers.Contains(x.id)).OrderByDescending(x => x.dr + x.cr).ToList();

                var taxable = adjustledgers.Sum(x => x.cr + x.dr);
                var vsummary = adjustledgers.ConvertAll(x => new FinanceAnalyse.VoucherSummary()
                {
                    vouchid = x.vid,
                    date = x.date,
                    no = x.no,
                    type = x.type,
                    ptype = x.ptype,
                    name = x.name,
                    id = x.id,
                    dr = x.dr,
                    cr = x.cr,
                    txble = taxable,
                    invno = x.invno,
                    invdate = x.invdate,
                    chequeno = x.chequeno,
                    narr = x.narr,
                    aname = partyledgers[0].name,
                    crby = x.crby,
                    tname = string.Join(",", gstvoucheritems.ConvertAll(y => y.name))
                });
                decimal igsttaxpercentage = 0;
                decimal cgsttaxpercentage = 0;
                decimal sgsttaxpercentage = 0;
                gstvoucheritems.ForEach(x =>
                {
                    var taxledger = ledgers.Find(y => y.id == x.id);
                    if (taxledger.taxmode == 3)
                        igsttaxpercentage = taxledger.taxpercentage;
                    if (taxledger.taxmode == 4)
                        sgsttaxpercentage = taxledger.taxpercentage;
                    if (taxledger.taxmode == 5)
                        cgsttaxpercentage = taxledger.taxpercentage;
                });

                vsummary[0].tp = igsttaxpercentage > 0 ? igsttaxpercentage : (sgsttaxpercentage + cgsttaxpercentage);

                if (vsummary.Count > 1)
                {
                    //vsummary[0].dr = vsummary.Sum(x => x.dr);
                    //vsummary[0].cr = vsummary.Sum(x => x.cr);
                    //vsummary[0].name = string.Join(",", vsummary.ConvertAll(x => x.aname));
                    result.AddRange(vsummary);
                }
                else
                {
                    result.Add(vsummary[0]);
                }
                if (gstvoucheritems.Count > 0)
                {
                    vsummary[0].cgst = (cgstledgers.Count == 0) ? 0 : gstvoucheritems.Sum(x => cgstledgers.Contains(x.id) ? x.cr + x.dr : 0);
                    vsummary[0].sgst = (sgstledgers.Count == 0) ? 0 : gstvoucheritems.Sum(x => sgstledgers.Contains(x.id) ? x.cr + x.dr : 0);
                    vsummary[0].igst = (igstledgers.Count == 0) ? 0 : gstvoucheritems.Sum(x => igstledgers.Contains(x.id) ? x.cr + x.dr : 0);
                    vsummary[0].tds = (tdsledgers.Count == 0) ? 0 : tdsvoucheritems.Sum(x => tdsledgers.Contains(x.id) ? x.cr + x.dr : 0);

                    vsummary[0].duty = (dutyledgers.Count == 0) ? 0 : taxvoucheritems.Sum(x => dutyledgers.Contains(x.id) ? x.cr + x.dr : 0);
                    vsummary[0].tcs = (tcsledgers.Count == 0) ? 0 : taxvoucheritems.Sum(x => tcsledgers.Contains(x.id) ? x.cr + x.dr : 0);
                    vsummary[0].cess = (cessledgers.Count == 0) ? 0 : taxvoucheritems.Sum(x => cessledgers.Contains(x.id) ? x.cr + x.dr : 0);
                }
                vsummary[0].rnd = (rndledgers.Count == 0) ? 0 : voucheritems.Sum(x => rndledgers.Contains(x.id) ? x.cr + x.dr : 0);

            }
            return result;
        }

        public List<FinanceAnalyse.VoucherSummary> BuildDebitorsSummary(FinanceAnalyseCreteria item, List<long> expinctypes, List<FinanceAnalyse.VoucherDetail> sourcelist)
        {
            var ledgerids = sourcelist.ConvertAll(x => x.id).Distinct().ToList();
            LedgerDataService ledgerDataService = new LedgerDataService() { Context = this.Context };
            var ledgers = ledgerDataService.Search(new LedgerCreteria()
            {
                ids = ledgerids
            });
            var inexpledgers = ledgers.FindAll(x => expinctypes.Contains(x.typeid)).ConvertAll(x => x.id);

            var taxledgers = ledgers.FindAll(x => x.typeid == 16 && x.taxmode >= 1 && x.taxmode <= 7);
            var dutyledgers = taxledgers.FindAll(x => x.typeid == 16 && x.taxmode == 1).ConvertAll(x => x.id).ToList();
            var cessledgers = taxledgers.FindAll(x => x.typeid == 16 && x.taxmode == 2).ConvertAll(x => x.id).ToList();
            var igstledgers = taxledgers.FindAll(x => x.typeid == 16 && x.taxmode == 3).ConvertAll(x => x.id).ToList();
            var sgstledgers = taxledgers.FindAll(x => x.typeid == 16 && x.taxmode == 4).ConvertAll(x => x.id).ToList();
            var cgstledgers = taxledgers.FindAll(x => x.typeid == 16 && x.taxmode == 5).ConvertAll(x => x.id).ToList();
            var tdsledgers = taxledgers.FindAll(x => x.typeid == 16 && x.taxmode == 6).ConvertAll(x => x.id).ToList();
            var tcsledgers = taxledgers.FindAll(x => x.typeid == 16 && x.taxmode == 7).ConvertAll(x => x.id).ToList();
            var rndledgers = ledgers.FindAll(x => x.typeid == 30).ConvertAll(x => x.id).ToList();

            var taxledgerids = ledgers.FindAll(x => x.typeid == 16 && x.taxmode >= 1 && x.taxmode <= 7).ConvertAll(x => x.id).ToList();
            taxledgers.RemoveAll(x => !taxledgerids.Contains(x.id));

            List<FinanceAnalyse.VoucherSummary> result = new List<FinanceAnalyse.VoucherSummary>();
            foreach (var lst in sourcelist.GroupBy(x => x.vid))
            {
                var voucheritems = lst.ToList();
                var taxvoucheritems = voucheritems.FindAll(x => taxledgerids.Contains(x.id));
                var nontaxvoucheritems = voucheritems.FindAll(x => !rndledgers.Contains(x.id) && !inexpledgers.Contains(x.id) && !taxledgerids.Contains(x.id));
                var expinvoucheritems = voucheritems.FindAll(x => inexpledgers.Contains(x.id));
                if (nontaxvoucheritems.Count == 1 && expinvoucheritems.Count == 1)
                {
                    var vsummary = nontaxvoucheritems.ConvertAll(x => new FinanceAnalyse.VoucherSummary()
                    {
                        vouchid = x.vid,
                        date = x.date,
                        no = x.no,
                        type = x.type,
                        ptype = x.ptype,
                        name = expinvoucheritems[0].name,
                        id = x.id,
                        cr = x.dr,
                        dr = x.cr,
                        invno = x.invno,
                        invdate = x.invdate,
                        chequeno = x.chequeno,
                        narr = x.narr,
                        aname = x.name,
                        crby = x.crby
                    });
                    if (taxvoucheritems.Count > 0)
                    {
                        vsummary[0].duty = (dutyledgers.Count == 0) ? 0 : taxvoucheritems.Sum(x => dutyledgers.Contains(x.id) ? x.cr + x.dr : 0);
                        vsummary[0].tds = (tdsledgers.Count == 0) ? 0 : taxvoucheritems.Sum(x => tdsledgers.Contains(x.id) ? x.cr + x.dr : 0);
                        vsummary[0].cgst = (cgstledgers.Count == 0) ? 0 : taxvoucheritems.Sum(x => cgstledgers.Contains(x.id) ? x.cr + x.dr : 0);
                        vsummary[0].sgst = (sgstledgers.Count == 0) ? 0 : taxvoucheritems.Sum(x => sgstledgers.Contains(x.id) ? x.cr + x.dr : 0);
                        vsummary[0].igst = (igstledgers.Count == 0) ? 0 : taxvoucheritems.Sum(x => igstledgers.Contains(x.id) ? x.cr + x.dr : 0);
                        vsummary[0].tcs = (tcsledgers.Count == 0) ? 0 : taxvoucheritems.Sum(x => tcsledgers.Contains(x.id) ? x.cr + x.dr : 0);
                        vsummary[0].cess = (cessledgers.Count == 0) ? 0 : taxvoucheritems.Sum(x => cessledgers.Contains(x.id) ? x.cr + x.dr : 0);
                    }
                    vsummary[0].rnd = (rndledgers.Count == 0) ? 0 : voucheritems.Sum(x => rndledgers.Contains(x.id) ? x.cr + x.dr : 0);
                    result.AddRange(vsummary);
                }
                else
                {
                    expinvoucheritems.AddRange(nontaxvoucheritems);
                    var pledgers = voucheritems.FindAll(x => x.vvid == 0);
                    var lledgers = expinvoucheritems.FindAll(x => x.vvid > 0);
                    var vsummary = lledgers.ConvertAll(x => new FinanceAnalyse.VoucherSummary()
                    {
                        vouchid = x.vid,
                        date = x.date,
                        no = x.no,
                        type = x.type,
                        name = pledgers.Count > 0 ? pledgers[0].name : "",
                        gstno = pledgers.Count > 0 ? pledgers[0].gstno : "",
                        aname = x.name,
                        id = x.id,
                        cr = x.dr,
                        dr = x.cr,
                        invno = x.invno,
                        invdate = x.invdate,
                        chequeno = x.chequeno,
                        narr = x.narr,
                        crby = x.crby
                    });
                    if (vsummary.Count > 0)
                    {
                        AssignTax(true, vsummary,
                        voucheritems,
                        taxvoucheritems,
                        ledgers,
                        tdsledgers, sgstledgers, cgstledgers, igstledgers,
                        dutyledgers, tcsledgers, cessledgers, rndledgers);
                        result.AddRange(vsummary);
                    }
                }
            }

            if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.int_finance)
            {
                FinanceService financeService = new FinanceService();
                var quantoledgers = financeService.SearchSalesLedgers(item.companyid, item.fromdate, item.todate);
                if (item.locations != null && item.locations.Count > 0)
                {
                    quantoledgers.CardSalesList.RemoveAll(x => !item.locations.Contains(x.locationid));
                    quantoledgers.CashSalesList.RemoveAll(x => !item.locations.Contains(x.locationid));
                    quantoledgers.VoucherSalesList.RemoveAll(x => !item.locations.Contains(x.locationid));
                    quantoledgers.TaxList.RemoveAll(x => !item.locations.Contains(x.LocationID));
                    quantoledgers.BillList.RemoveAll(x => !item.locations.Contains(x.LocationID));
                }

                quantoledgers.BillList.Where(x => x.CustomerID > 0 && x.Amount > 0).ToList().ForEach(x =>
                {
                    if (!string.IsNullOrEmpty(x.CustomerName))
                    {
                        FinanceAnalyse.VoucherSummary voucherSummary = new FinanceAnalyse.VoucherSummary()
                        {
                            id = 0,
                            no = x.BillNo,
                            date = x.Date,
                            name = x.CustomerName,
                            aname = "B2B Sales",
                            invdate = x.Date,
                            invno = x.BillNo,
                            dr = x.Amount,
                            crby = x.CreatedBy,
                            type = 4
                        };
                        result.Add(voucherSummary);
                        var gsttaxledgers = quantoledgers.TaxList.FindAll(y => y.CustomerID == x.CustomerID && y.Billno == x.BillNo);
                        if (x.IsInterstate)
                        {
                            voucherSummary.igst = gsttaxledgers.Sum(y => y.tax);
                        }
                        else
                        {
                            voucherSummary.cgst = gsttaxledgers.Sum(y => y.tax) / 2;
                            voucherSummary.sgst = gsttaxledgers.Sum(y => y.tax) / 2;
                        }
                        voucherSummary.txble = x.Amount - (voucherSummary.cgst + voucherSummary.sgst + voucherSummary.igst);
                    }
                });

                quantoledgers.BillList.Where(x => x.CustomerID > 0 && x.Amount < 0).ToList().ForEach(x =>
                {
                    if (!string.IsNullOrEmpty(x.CustomerName))
                    {
                        FinanceAnalyse.VoucherSummary voucherSummary = new FinanceAnalyse.VoucherSummary()
                        {
                            id = 0,
                            no = x.BillNo,
                            date = x.Date,
                            name = x.CustomerName,
                            aname = "B2B Sales Return",
                            invdate = x.Date,
                            invno = x.BillNo,
                            cr = Math.Abs(x.Amount),
                            crby = x.CreatedBy,
                            type = 5
                        };
                        result.Add(voucherSummary);
                        var gsttaxledgers = quantoledgers.TaxList.FindAll(y => y.CustomerID == x.CustomerID && y.Billno == x.BillNo);
                        if (x.IsInterstate)
                        {
                            voucherSummary.igst = Math.Abs(gsttaxledgers.Sum(y => y.tax));
                        }
                        else
                        {
                            voucherSummary.cgst = Math.Abs(gsttaxledgers.Sum(y => y.tax) / 2);
                            voucherSummary.sgst = Math.Abs(gsttaxledgers.Sum(y => y.tax) / 2);
                        }
                        voucherSummary.txble = Math.Abs(x.Amount) - (voucherSummary.cgst + voucherSummary.sgst + voucherSummary.igst);
                    }
                });
            }
            return result;
        }

        public void AssignTax(
            bool includetax,
            List<FinanceAnalyse.VoucherSummary> vsummary,
            List<FinanceAnalyse.VoucherDetail> voucheritems,
            List<FinanceAnalyse.VoucherDetail> taxvoucheritems,
            List<LedgerDetail> ledgers,
            List<long> tdsledgers, List<long> sgstledgers, List<long> cgstledgers, List<long> igstledgers,
            List<long> dutyledgers, List<long> tcsledgers, List<long> cessledgers, List<long> rndledgers)
        {

            if (taxvoucheritems.Count > 0)
            {
                var gsttaxledgers = taxvoucheritems.FindAll(x => sgstledgers.Contains(x.id));
                if (gsttaxledgers.Count == 0)
                {
                    gsttaxledgers = taxvoucheritems.FindAll(x => cgstledgers.Contains(x.id));
                    if (gsttaxledgers.Count == 0)
                    {
                        gsttaxledgers = taxvoucheritems.FindAll(x => igstledgers.Contains(x.id));
                    }
                }
                bool addtaxvalues = true;
                if (gsttaxledgers.Count > 0)
                {
                    var cgst = (cgstledgers.Count == 0) ? 0 : taxvoucheritems.Sum(x => cgstledgers.Contains(x.id) ? x.cr + x.dr : 0);
                    var sgst = (sgstledgers.Count == 0) ? 0 : taxvoucheritems.Sum(x => sgstledgers.Contains(x.id) ? x.cr + x.dr : 0);
                    var igst = (igstledgers.Count == 0) ? 0 : taxvoucheritems.Sum(x => igstledgers.Contains(x.id) ? x.cr + x.dr : 0);
                    var taxledger = ledgers.Find(x => x.id == gsttaxledgers[0].id);
                    if (taxledger.taxpercentage > 0)
                    {
                        var txble = (gsttaxledgers[0].cr + gsttaxledgers[0].dr) / taxledger.taxpercentage * 100;
                        var txble1 = Math.Floor(txble);
                        var txble2 = Math.Ceiling(txble);
                        if (vsummary.Exists(x => x.cr + x.dr == txble) ||
                           vsummary.Exists(x => (x.cr + x.dr) >= txble1 && (x.cr + x.dr) <= txble2))
                        {
                            var summry = vsummary.FindAll(x => x.cr + x.dr == txble);
                            if (summry.Count == 0)
                                summry = vsummary.FindAll(x => (x.cr + x.dr) >= txble1 && (x.cr + x.dr) <= txble2);
                            if (summry.Count == 1)
                            {
                                addtaxvalues = false;
                                summry[0].cgst = cgst;
                                summry[0].sgst = sgst;
                                summry[0].igst = igst;
                                summry[0].txble = summry[0].cr + summry[0].dr;
                                summry[0].tds = (tdsledgers.Count == 0) ? 0 : taxvoucheritems.Sum(x => tdsledgers.Contains(x.id) ? x.cr + x.dr : 0);
                                summry[0].duty = (dutyledgers.Count == 0) ? 0 : taxvoucheritems.Sum(x => dutyledgers.Contains(x.id) ? x.cr + x.dr : 0);
                                summry[0].tcs = (tcsledgers.Count == 0) ? 0 : taxvoucheritems.Sum(x => tcsledgers.Contains(x.id) ? x.cr + x.dr : 0);
                                summry[0].cess = (cessledgers.Count == 0) ? 0 : taxvoucheritems.Sum(x => cessledgers.Contains(x.id) ? x.cr + x.dr : 0);
                                if (includetax)
                                {
                                    if (summry[0].cr > 0)
                                        summry[0].cr += cgst + sgst + igst - summry[0].tds;
                                    else
                                        summry[0].dr += cgst + sgst + igst - summry[0].tds;
                                }
                            }
                        }
                    }
                }

                if (addtaxvalues)
                {
                    vsummary[0].tds = (tdsledgers.Count == 0) ? 0 : taxvoucheritems.Sum(x => tdsledgers.Contains(x.id) ? x.cr + x.dr : 0);
                    vsummary[0].duty = (dutyledgers.Count == 0) ? 0 : taxvoucheritems.Sum(x => dutyledgers.Contains(x.id) ? x.cr + x.dr : 0);
                    vsummary[0].tcs = (tcsledgers.Count == 0) ? 0 : taxvoucheritems.Sum(x => tcsledgers.Contains(x.id) ? x.cr + x.dr : 0);
                    vsummary[0].cess = (cessledgers.Count == 0) ? 0 : taxvoucheritems.Sum(x => cessledgers.Contains(x.id) ? x.cr + x.dr : 0);
                }
            }
            vsummary[0].rnd = (rndledgers.Count == 0) ? 0 : voucheritems.Sum(x => rndledgers.Contains(x.id) ? x.cr + x.dr : 0);
        }

        public List<FinanceAnalyse.VoucherSummary> BuildFixedAssetSummary(FinanceAnalyseCreteria item, List<long> expinctypes,
    List<FinanceAnalyse.VoucherDetail> sourcelist)
        {

            var ledgerids = sourcelist.ConvertAll(x => x.id).Distinct().ToList();
            LedgerDataService ledgerDataService = new LedgerDataService() { Context = this.Context };
            var ledgers = ledgerDataService.Search(new LedgerCreteria()
            {
                ids = ledgerids
            });

            var inexpledgers = ledgers.FindAll(x => expinctypes.Contains(x.typeid)).ConvertAll(x => x.id);

            var taxledgers = ledgers.FindAll(x => x.typeid == 16 && x.taxmode >= 1 && x.taxmode <= 7);
            var dutyledgers = taxledgers.FindAll(x => x.typeid == 16 && x.taxmode == 1).ConvertAll(x => x.id).ToList();
            var cessledgers = taxledgers.FindAll(x => x.typeid == 16 && x.taxmode == 2).ConvertAll(x => x.id).ToList();
            var igstledgers = taxledgers.FindAll(x => x.typeid == 16 && x.taxmode == 3).ConvertAll(x => x.id).ToList();
            var sgstledgers = taxledgers.FindAll(x => x.typeid == 16 && x.taxmode == 4).ConvertAll(x => x.id).ToList();
            var cgstledgers = taxledgers.FindAll(x => x.typeid == 16 && x.taxmode == 5).ConvertAll(x => x.id).ToList();
            var tdsledgers = taxledgers.FindAll(x => x.typeid == 16 && x.taxmode == 6).ConvertAll(x => x.id).ToList();
            var tcsledgers = taxledgers.FindAll(x => x.typeid == 16 && x.taxmode == 7).ConvertAll(x => x.id).ToList();
            var rndledgers = ledgers.FindAll(x => x.typeid == 30).ConvertAll(x => x.id).ToList();

            var taxledgerids = ledgers.FindAll(x => x.typeid == 16 && x.taxmode >= 1 && x.taxmode <= 7).ConvertAll(x => x.id).ToList();
            taxledgers.RemoveAll(x => !taxledgerids.Contains(x.id));


            List<FinanceAnalyse.VoucherSummary> result = new List<FinanceAnalyse.VoucherSummary>();
            foreach (var lst in sourcelist.GroupBy(x => x.vid))
            {
                var voucheritems = lst.ToList();
                var taxvoucheritems = voucheritems.FindAll(x => taxledgerids.Contains(x.id));
                var nontaxvoucheritems = voucheritems.FindAll(x => !rndledgers.Contains(x.id) && !inexpledgers.Contains(x.id) && !taxledgerids.Contains(x.id));
                var expinvoucheritems = voucheritems.FindAll(x => inexpledgers.Contains(x.id));
                if (nontaxvoucheritems.Count == 1 && expinvoucheritems.Count == 1)
                {
                    var vsummary = expinvoucheritems.ConvertAll(x => new FinanceAnalyse.VoucherSummary()
                    {
                        vouchid = x.vid,
                        date = x.date,
                        no = x.no,
                        type = x.type,
                        ptype = x.ptype,
                        aname = x.name,
                        id = x.id,
                        dr = x.dr,
                        cr = x.cr,
                        invno = x.invno,
                        invdate = x.invdate,
                        chequeno = x.chequeno,
                        narr = x.narr,
                        ltype = expinvoucheritems[0].ltype,
                        name = nontaxvoucheritems[0].name,
                        gstno = nontaxvoucheritems[0].gstno,
                        crby = x.crby
                    });
                    if (taxvoucheritems.Count > 0)
                    {
                        vsummary[0].duty = (dutyledgers.Count == 0) ? 0 : taxvoucheritems.Sum(x => dutyledgers.Contains(x.id) ? x.cr + x.dr : 0);
                        vsummary[0].tds = (tdsledgers.Count == 0) ? 0 : taxvoucheritems.Sum(x => tdsledgers.Contains(x.id) ? x.cr + x.dr : 0);
                        vsummary[0].cgst = (cgstledgers.Count == 0) ? 0 : taxvoucheritems.Sum(x => cgstledgers.Contains(x.id) ? x.cr + x.dr : 0);
                        vsummary[0].sgst = (sgstledgers.Count == 0) ? 0 : taxvoucheritems.Sum(x => sgstledgers.Contains(x.id) ? x.cr + x.dr : 0);
                        vsummary[0].igst = (igstledgers.Count == 0) ? 0 : taxvoucheritems.Sum(x => igstledgers.Contains(x.id) ? x.cr + x.dr : 0);
                        vsummary[0].tcs = (tcsledgers.Count == 0) ? 0 : taxvoucheritems.Sum(x => tcsledgers.Contains(x.id) ? x.cr + x.dr : 0);
                        vsummary[0].cess = (cessledgers.Count == 0) ? 0 : taxvoucheritems.Sum(x => cessledgers.Contains(x.id) ? x.cr + x.dr : 0);
                        vsummary[0].txble = (vsummary[0].cr + vsummary[0].dr);// + vsummary[0].tds - (vsummary[0].cgst + vsummary[0].sgst + vsummary[0].igst);
                    }
                    vsummary[0].rnd = (rndledgers.Count == 0) ? 0 : voucheritems.Sum(x => rndledgers.Contains(x.id) ? x.cr + x.dr : 0);
                    result.AddRange(vsummary);
                }
                else
                {
                    //expinvoucheritems.AddRange(nontaxvoucheritems);
                    var pledgers = voucheritems.FindAll(x => x.vvid == 0);
                    var lledgers = expinvoucheritems.FindAll(x => x.vvid > 0).OrderByDescending(x => x.cr + x.dr).ToList();
                    var vsummary = lledgers.ConvertAll(x => new FinanceAnalyse.VoucherSummary()
                    {
                        vouchid = x.vid,
                        date = x.date,
                        no = x.no,
                        type = x.type,
                        name = pledgers.Count > 0 ? pledgers[0].name : "",
                        gstno = pledgers.Count > 0 ? pledgers[0].gstno : "",
                        aname = x.name,
                        id = x.id,
                        dr = x.dr,
                        cr = x.cr,
                        ltype = x.ltype,
                        invno = x.invno,
                        invdate = x.invdate,
                        chequeno = x.chequeno,
                        narr = x.narr
                    });
                    if (vsummary.Exists(x => expinctypes.Contains(x.ltype)))
                    {
                        vsummary.RemoveAll(x => !expinctypes.Contains(x.ltype));
                    }
                    if (vsummary.Count > 0)
                    {
                        AssignTax(false, vsummary,
                        voucheritems,
                        taxvoucheritems,
                        ledgers,
                        tdsledgers, sgstledgers, cgstledgers, igstledgers,
                        dutyledgers, tcsledgers, cessledgers, rndledgers);
                        result.AddRange(vsummary);
                    }
                }
            }
            return result;
        }

        public List<FinanceAnalyse.VoucherSummary> BuildCrediorsSummary(FinanceAnalyseCreteria item, List<long> expinctypes,
            List<FinanceAnalyse.VoucherDetail> sourcelist)
        {

            var ledgerids = sourcelist.ConvertAll(x => x.id).Distinct().ToList();
            LedgerDataService ledgerDataService = new LedgerDataService() { Context = this.Context };
            var ledgers = ledgerDataService.Search(new LedgerCreteria()
            {
                ids = ledgerids
            });

            var inexpledgers = ledgers.FindAll(x => expinctypes.Contains(x.typeid)).ConvertAll(x => x.id);

            var taxledgers = ledgers.FindAll(x => x.typeid == 16 && x.taxmode >= 1 && x.taxmode <= 7);
            var dutyledgers = taxledgers.FindAll(x => x.typeid == 16 && x.taxmode == 1).ConvertAll(x => x.id).ToList();
            var cessledgers = taxledgers.FindAll(x => x.typeid == 16 && x.taxmode == 2).ConvertAll(x => x.id).ToList();
            var igstledgers = taxledgers.FindAll(x => x.typeid == 16 && x.taxmode == 3).ConvertAll(x => x.id).ToList();
            var sgstledgers = taxledgers.FindAll(x => x.typeid == 16 && x.taxmode == 4).ConvertAll(x => x.id).ToList();
            var cgstledgers = taxledgers.FindAll(x => x.typeid == 16 && x.taxmode == 5).ConvertAll(x => x.id).ToList();
            var tdsledgers = taxledgers.FindAll(x => x.typeid == 16 && x.taxmode == 6).ConvertAll(x => x.id).ToList();
            var tcsledgers = taxledgers.FindAll(x => x.typeid == 16 && x.taxmode == 7).ConvertAll(x => x.id).ToList();
            var rndledgers = ledgers.FindAll(x => x.typeid == 30).ConvertAll(x => x.id).ToList();

            var taxledgerids = ledgers.FindAll(x => x.typeid == 16 && x.taxmode >= 1 && x.taxmode <= 7).ConvertAll(x => x.id).ToList();
            taxledgers.RemoveAll(x => !taxledgerids.Contains(x.id));

            List<FinanceAnalyse.VoucherSummary> result = new List<FinanceAnalyse.VoucherSummary>();
            foreach (var lst in sourcelist.GroupBy(x => x.vid))
            {
                var voucheritems = lst.ToList();
                var taxvoucheritems = voucheritems.FindAll(x => taxledgerids.Contains(x.id));
                var nontaxvoucheritems = voucheritems.FindAll(x => !rndledgers.Contains(x.id) && !inexpledgers.Contains(x.id) && !taxledgerids.Contains(x.id));
                var expinvoucheritems = voucheritems.FindAll(x => inexpledgers.Contains(x.id));
                if (nontaxvoucheritems.Count == 1 && expinvoucheritems.Count == 1)
                {
                    var vsummary = expinvoucheritems.ConvertAll(x => new FinanceAnalyse.VoucherSummary()
                    {
                        vouchid = x.vid,
                        date = x.date,
                        no = x.no,
                        type = x.type,
                        ptype = x.ptype,
                        name = x.name,
                        id = x.id,
                        dr = x.dr,
                        cr = x.cr,
                        invno = x.invno,
                        invdate = x.invdate,
                        chequeno = x.chequeno,
                        narr = x.narr,
                        aname = nontaxvoucheritems[0].name,
                        gstno = nontaxvoucheritems[0].gstno,
                        crby = x.crby
                    });
                    if (taxvoucheritems.Count > 0)
                    {
                        vsummary[0].duty = (dutyledgers.Count == 0) ? 0 : taxvoucheritems.Sum(x => dutyledgers.Contains(x.id) ? x.cr + x.dr : 0);
                        vsummary[0].tds = (tdsledgers.Count == 0) ? 0 : taxvoucheritems.Sum(x => tdsledgers.Contains(x.id) ? x.cr + x.dr : 0);
                        vsummary[0].cgst = (cgstledgers.Count == 0) ? 0 : taxvoucheritems.Sum(x => cgstledgers.Contains(x.id) ? x.cr + x.dr : 0);
                        vsummary[0].sgst = (sgstledgers.Count == 0) ? 0 : taxvoucheritems.Sum(x => sgstledgers.Contains(x.id) ? x.cr + x.dr : 0);
                        vsummary[0].igst = (igstledgers.Count == 0) ? 0 : taxvoucheritems.Sum(x => igstledgers.Contains(x.id) ? x.cr + x.dr : 0);
                        vsummary[0].tcs = (tcsledgers.Count == 0) ? 0 : taxvoucheritems.Sum(x => tcsledgers.Contains(x.id) ? x.cr + x.dr : 0);
                        vsummary[0].cess = (cessledgers.Count == 0) ? 0 : taxvoucheritems.Sum(x => cessledgers.Contains(x.id) ? x.cr + x.dr : 0);
                        vsummary[0].txble = (vsummary[0].cr + vsummary[0].dr) + vsummary[0].tds - (vsummary[0].cgst + vsummary[0].sgst + vsummary[0].igst);
                    }
                    vsummary[0].rnd = (rndledgers.Count == 0) ? 0 : voucheritems.Sum(x => rndledgers.Contains(x.id) ? x.cr + x.dr : 0);
                    result.AddRange(vsummary);
                }
                else
                {
                    expinvoucheritems.AddRange(nontaxvoucheritems);
                    var pledgers = voucheritems.FindAll(x => x.vvid == 0);
                    var lledgers = expinvoucheritems.FindAll(x => x.vvid > 0).OrderByDescending(x => x.cr + x.dr).ToList();
                    var vsummary = lledgers.ConvertAll(x => new FinanceAnalyse.VoucherSummary()
                    {
                        vouchid = x.vid,
                        date = x.date,
                        no = x.no,
                        type = x.type,
                        name = pledgers.Count > 0 ? pledgers[0].name : "",
                        gstno = pledgers.Count > 0 ? pledgers[0].gstno : "",
                        aname = x.name,
                        id = x.id,
                        cr = x.dr,
                        dr = x.cr,
                        invno = x.invno,
                        invdate = x.invdate,
                        chequeno = x.chequeno,
                        narr = x.narr
                    });
                    if (vsummary.Count > 0)
                    {
                        AssignTax(true, vsummary,
                        voucheritems,
                        taxvoucheritems,
                        ledgers,
                        tdsledgers, sgstledgers, cgstledgers, igstledgers,
                        dutyledgers, tcsledgers, cessledgers, rndledgers);
                        result.AddRange(vsummary);
                    }
                }
            }

            if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.int_finance)
            {
                FinanceService financeService = new FinanceService();
                var quantoledgers = financeService.SearchPurchaseLedgers(item.companyid, item.fromdate, item.todate, true);

                List<long> supplierids = new List<long>();
                if (quantoledgers.PurchaseLegers != null &&
                    quantoledgers.PurchaseLegers.Count > 0)
                {
                    supplierids.AddRange(quantoledgers.PurchaseLegers.ConvertAll(x => x.suppliercompanyid).Distinct().ToList());
                }

                if (quantoledgers.PurchaseReturn != null &&
    quantoledgers.PurchaseReturn.Count > 0)
                {
                    supplierids.AddRange(quantoledgers.PurchaseReturn.ConvertAll(x => x.suppliercompanyid).Distinct().ToList());
                }

                if (quantoledgers.SupplierPayment != null &&
quantoledgers.SupplierPayment.Count > 0)
                {
                    supplierids.AddRange(quantoledgers.SupplierPayment.ConvertAll(x => x.suppliercompanyid).Distinct().ToList());

                }

                SuppliercompanyDataService suppliercompanyDataService = new SuppliercompanyDataService() { Context = this.Context };
                var supplierlist = suppliercompanyDataService.SearchHeader(new SuppliercompanyCreteria()
                {
                    ids = supplierids
                });


                quantoledgers.PurchaseLegers.ForEach(x =>
                {
                    if (!string.IsNullOrEmpty(x.suppliercompany))
                    {
                        FinanceAnalyse.VoucherSummary voucherSummary = new FinanceAnalyse.VoucherSummary()
                        {
                            id = 0,
                            no = x.invoiceno,
                            date = x.entrydate,
                            name = x.suppliercompany,
                            aname = "Inventory Purchase",
                            invdate = x.invoicedate,
                            invno = x.invoiceno,
                            gstno = supplierlist.Exists(y => y.id == x.suppliercompanyid) ? supplierlist.Find(y => y.id == x.suppliercompanyid).gst : "",
                            cr = x.totalamount,
                            crby = x.createdby,
                            type = 3
                        };
                        result.Add(voucherSummary);
                        if (x.isinterstatesale)
                        {
                            voucherSummary.igst = x.taxsummary.Sum(y => y.tax);
                        }
                        else
                        {
                            voucherSummary.cgst = x.taxsummary.Sum(y => y.tax) / 2;
                            voucherSummary.sgst = x.taxsummary.Sum(y => y.tax) / 2;
                        }
                        voucherSummary.txble = x.totalamount - (voucherSummary.cgst + voucherSummary.sgst + voucherSummary.igst);
                    }
                });

                quantoledgers = financeService.SupplierReturnLedger(item.companyid, item.fromdate, item.todate, true);
                quantoledgers.PurchaseReturn.ForEach(x =>
                {
                    if (!string.IsNullOrEmpty(x.suppliercompany))
                    {
                        FinanceAnalyse.VoucherSummary voucherSummary = new FinanceAnalyse.VoucherSummary()
                        {
                            id = 0,
                            no = x.returnno,
                            date = x.returndate,
                            name = x.suppliercompany,
                            aname = x.returntype == 7 ? "Supplier Advance" : (x.returntype == 10 ? "Excess Note" : "Inventory Return"),
                            gstno = supplierlist.Exists(y => y.id == x.suppliercompanyid) ? supplierlist.Find(y => y.id == x.suppliercompanyid).gst : "",
                            invdate = x.invoicedate,
                            narr = x.invoiceno,
                            dr = x.returntype < 10 ? x.totalamount : 0,
                            cr = x.returntype == 10 ? x.totalamount : 0,
                            crby = x.createdby,
                            type = 6
                        };
                        result.Add(voucherSummary);
                        if (x.isinterstatesale)
                        {
                            voucherSummary.igst = x.taxsummary.Sum(y => y.tax);
                        }
                        else
                        {
                            voucherSummary.cgst = x.taxsummary.Sum(y => y.tax) / 2;
                            voucherSummary.sgst = x.taxsummary.Sum(y => y.tax) / 2;
                        }
                        voucherSummary.txble = x.totalamount - (voucherSummary.cgst + voucherSummary.sgst + voucherSummary.igst);
                    }
                });

                quantoledgers = financeService.SearchSupplierPaymentSummary(item.companyid, 0, item.fromdate, item.todate, true);

                if (quantoledgers.SupplierPayment != null &&
                    quantoledgers.SupplierPayment.Count > 0)
                {
                    supplierlist = suppliercompanyDataService.SearchHeader(new SuppliercompanyCreteria()
                    {
                        ids = quantoledgers.SupplierPayment.ConvertAll(x => x.suppliercompanyid).Distinct().ToList()
                    });
                }

                quantoledgers.SupplierPayment.ForEach(x =>
                {
                    FinanceAnalyse.VoucherSummary voucherSummary = new FinanceAnalyse.VoucherSummary()
                    {
                        id = 0,
                        no = x.receiptno,
                        date = x.paymentdate,
                        name = x.suppliercompany,
                        gstno = supplierlist.Exists(y => y.id == x.id) ? supplierlist.Find(y => y.id == x.id).gst : "",
                        aname = "Supplier Payment",
                        invno = x.chequeno,
                        dr = x.allocations.Sum(y => y.paidamount),
                        tds = x.tdsamount,
                        narr = !string.IsNullOrEmpty(x.chequeno) ? ("CHQ:" + x.chequeno + ",INV:" + x.invoiceno) : x.invoiceno,
                        crby = x.createdby,
                        type = 1
                    };
                    result.Add(voucherSummary);
                });
            }
            return result;
        }

        /*        { 'id': 1, 'name': 'DUTY' },
        { 'id': 2, 'name': 'CESS' },
        { 'id': 3, 'name': 'IGST' },
        { 'id': 4, 'name': 'SGST' },
        { 'id': 5, 'name': 'CGST' },
        { 'id': 6, 'name': 'TDS' },
        { 'id': 7, 'name': 'TCS' }*/
        public List<FinanceAnalyse.VoucherSummary> BuildExpInSummary(FinanceAnalyseCreteria item, List<long> expinctypes, List<FinanceAnalyse.VoucherDetail> sourcelist)
        {
            var ledgerids = sourcelist.ConvertAll(x => x.id).Distinct().ToList();
            LedgerDataService ledgerDataService = new LedgerDataService() { Context = this.Context };
            var ledgers = ledgerDataService.Search(new LedgerCreteria()
            {
                ids = ledgerids
            });

            var inexpledgers = ledgers.FindAll(x => expinctypes.Contains(x.typeid)).ConvertAll(x => x.id);

            var taxledgers = ledgers.FindAll(x => x.typeid == 16 && x.taxmode >= 1 && x.taxmode <= 7);
            var dutyledgers = taxledgers.FindAll(x => x.typeid == 16 && x.taxmode == 1).ConvertAll(x => x.id).ToList();
            var cessledgers = taxledgers.FindAll(x => x.typeid == 16 && x.taxmode == 2).ConvertAll(x => x.id).ToList();
            var igstledgers = taxledgers.FindAll(x => x.typeid == 16 && x.taxmode == 3).ConvertAll(x => x.id).ToList();
            var sgstledgers = taxledgers.FindAll(x => x.typeid == 16 && x.taxmode == 4).ConvertAll(x => x.id).ToList();
            var cgstledgers = taxledgers.FindAll(x => x.typeid == 16 && x.taxmode == 5).ConvertAll(x => x.id).ToList();
            var tdsledgers = taxledgers.FindAll(x => x.typeid == 16 && x.taxmode == 6).ConvertAll(x => x.id).ToList();
            var tcsledgers = taxledgers.FindAll(x => x.typeid == 16 && x.taxmode == 7).ConvertAll(x => x.id).ToList();
            var rndledgers = ledgers.FindAll(x => x.typeid == 30).ConvertAll(x => x.id).ToList();

            var taxledgerids = ledgers.FindAll(x => x.typeid == 16 && x.taxmode >= 1 && x.taxmode <= 7).ConvertAll(x => x.id).ToList();
            taxledgers.RemoveAll(x => !taxledgerids.Contains(x.id));
            var releventids = new List<long>();
            releventids.AddRange(inexpledgers);
            releventids.AddRange(taxledgers.ConvertAll(x => x.id));
            List<FinanceAnalyse.VoucherSummary> result = new List<FinanceAnalyse.VoucherSummary>();

            foreach (var lst in sourcelist.GroupBy(x => x.vid))
            {
                var voucheritems = lst.ToList();
                var taxvoucheritems = voucheritems.FindAll(x => taxledgerids.Contains(x.id));
                var nontaxvoucheritems = voucheritems.FindAll(x => !rndledgers.Contains(x.id) &&
                    !inexpledgers.Contains(x.id) && !taxledgerids.Contains(x.id));
                var expinvoucheritems = voucheritems.FindAll(x => inexpledgers.Contains(x.id));
                if (nontaxvoucheritems.Count == 1 && expinvoucheritems.Count == 1)
                {
                    var vsummary = expinvoucheritems.ConvertAll(x => new FinanceAnalyse.VoucherSummary()
                    {
                        vouchid = x.vid,
                        date = x.date,
                        no = x.no,
                        type = x.type,
                        ptype = x.ptype,
                        name = x.name,
                        id = x.id,
                        dr = x.dr,
                        cr = x.cr,
                        invno = x.invno,
                        invdate = x.invdate,
                        chequeno = x.chequeno,
                        gstno = nontaxvoucheritems[0].gstno,
                        narr = x.narr,
                        aname = nontaxvoucheritems[0].name
                    });
                    if (taxvoucheritems.Count > 0)
                    {
                        vsummary[0].duty = (dutyledgers.Count == 0) ? 0 : taxvoucheritems.Sum(x => dutyledgers.Contains(x.id) ? x.cr + x.dr : 0);
                        vsummary[0].tds = (tdsledgers.Count == 0) ? 0 : taxvoucheritems.Sum(x => tdsledgers.Contains(x.id) ? x.cr + x.dr : 0);
                        vsummary[0].cgst = (cgstledgers.Count == 0) ? 0 : taxvoucheritems.Sum(x => cgstledgers.Contains(x.id) ? x.cr + x.dr : 0);
                        vsummary[0].sgst = (sgstledgers.Count == 0) ? 0 : taxvoucheritems.Sum(x => sgstledgers.Contains(x.id) ? x.cr + x.dr : 0);
                        vsummary[0].igst = (igstledgers.Count == 0) ? 0 : taxvoucheritems.Sum(x => igstledgers.Contains(x.id) ? x.cr + x.dr : 0);
                        vsummary[0].tcs = (tcsledgers.Count == 0) ? 0 : taxvoucheritems.Sum(x => tcsledgers.Contains(x.id) ? x.cr + x.dr : 0);
                        vsummary[0].cess = (cessledgers.Count == 0) ? 0 : taxvoucheritems.Sum(x => cessledgers.Contains(x.id) ? x.cr + x.dr : 0);
                        vsummary[0].txble = (vsummary[0].cr + vsummary[0].dr);// + vsummary[0].tds - (vsummary[0].cgst + vsummary[0].sgst + vsummary[0].igst);
                    }
                    vsummary[0].rnd = (rndledgers.Count == 0) ? 0 : voucheritems.Sum(x => rndledgers.Contains(x.id) ? x.cr + x.dr : 0);
                    result.AddRange(vsummary);
                }
                //else if(nontaxvoucheritems.Count>0 && expinvoucheritems.Count == 1)
                //{
                //    var vsummary = nontaxvoucheritems.ConvertAll(x => new FinanceAnalyse.VoucherSummary()
                //    {
                //        vouchid = x.vid,
                //        date = x.date,
                //        no = x.no,
                //        type = x.type,
                //        name = x.name,
                //        id = x.id,
                //        dr = x.dr,
                //        cr = x.cr,
                //        invno = x.invno,
                //        invdate = x.invdate,
                //        chequeno = x.chequeno,
                //        narr = x.narr,
                //        aname = expinvoucheritems[0].name
                //    });
                //    if (taxvoucheritems.Count > 0)
                //    {
                //        vsummary[0].duty = (dutyledgers.Count == 0) ? 0 : taxvoucheritems.Sum(x => dutyledgers.Contains(x.id) ? x.cr + x.dr : 0);
                //        vsummary[0].tds = (tdsledgers.Count == 0) ? 0 : taxvoucheritems.Sum(x => tdsledgers.Contains(x.id) ? x.cr + x.dr : 0);
                //        vsummary[0].cgst = (cgstledgers.Count == 0) ? 0 : taxvoucheritems.Sum(x => cgstledgers.Contains(x.id) ? x.cr + x.dr : 0);
                //        vsummary[0].sgst = (sgstledgers.Count == 0) ? 0 : taxvoucheritems.Sum(x => sgstledgers.Contains(x.id) ? x.cr + x.dr : 0);
                //        vsummary[0].igst = (igstledgers.Count == 0) ? 0 : taxvoucheritems.Sum(x => igstledgers.Contains(x.id) ? x.cr + x.dr : 0);
                //        vsummary[0].tcs = (tcsledgers.Count == 0) ? 0 : taxvoucheritems.Sum(x => tcsledgers.Contains(x.id) ? x.cr + x.dr : 0);
                //        vsummary[0].cess = (cessledgers.Count == 0) ? 0 : taxvoucheritems.Sum(x => cessledgers.Contains(x.id) ? x.cr + x.dr : 0);
                //    }
                //    vsummary[0].rnd = (rndledgers.Count == 0) ? 0 : voucheritems.Sum(x => rndledgers.Contains(x.id) ? x.cr + x.dr : 0);
                //    result.AddRange(vsummary);
                //}
                //else if(expinvoucheritems.Count>0 && nontaxvoucheritems.Count == 1)
                //{
                //    var vsummary = expinvoucheritems.ConvertAll(x => new FinanceAnalyse.VoucherSummary()
                //    {
                //        vouchid = x.vid,
                //        date = x.date,
                //        no = x.no,
                //        type = x.type,
                //        name = nontaxvoucheritems[0].name,
                //        aname = x.name,
                //        id = x.id,
                //        dr = x.dr,
                //        cr = x.cr,
                //        invno = x.invno,
                //        invdate = x.invdate,
                //        chequeno = x.chequeno,
                //        narr = x.narr
                //    });
                //    if (taxvoucheritems.Count > 0)
                //    {
                //        vsummary[0].duty = (dutyledgers.Count == 0) ? 0 : taxvoucheritems.Sum(x => dutyledgers.Contains(x.id) ? x.cr + x.dr : 0);
                //        vsummary[0].tds = (tdsledgers.Count == 0) ? 0 : taxvoucheritems.Sum(x => tdsledgers.Contains(x.id) ? x.cr + x.dr : 0);
                //        vsummary[0].cgst = (cgstledgers.Count == 0) ? 0 : taxvoucheritems.Sum(x => cgstledgers.Contains(x.id) ? x.cr + x.dr : 0);
                //        vsummary[0].sgst = (sgstledgers.Count == 0) ? 0 : taxvoucheritems.Sum(x => sgstledgers.Contains(x.id) ? x.cr + x.dr : 0);
                //        vsummary[0].igst = (igstledgers.Count == 0) ? 0 : taxvoucheritems.Sum(x => igstledgers.Contains(x.id) ? x.cr + x.dr : 0);
                //        vsummary[0].tcs = (tcsledgers.Count == 0) ? 0 : taxvoucheritems.Sum(x => tcsledgers.Contains(x.id) ? x.cr + x.dr : 0);
                //        vsummary[0].cess = (cessledgers.Count == 0) ? 0 : taxvoucheritems.Sum(x => cessledgers.Contains(x.id) ? x.cr + x.dr : 0);
                //    }
                //    vsummary[0].rnd = (rndledgers.Count == 0) ? 0 : voucheritems.Sum(x => rndledgers.Contains(x.id) ? x.cr + x.dr : 0);
                //    result.AddRange(vsummary);
                //}
                else
                {
                    expinvoucheritems.AddRange(nontaxvoucheritems);
                    var pledgers = voucheritems.FindAll(x => x.vvid == 0);
                    if (pledgers.Count == 0)
                    {
                        var c = pledgers.Count;
                    }
                    var lledgers = expinvoucheritems.FindAll(x => x.vvid > 0).OrderByDescending(x => x.dr + x.cr).ToList();
                    lledgers.RemoveAll(x => !releventids.Contains(x.id));
                    var vsummary = lledgers.ConvertAll(x => new FinanceAnalyse.VoucherSummary()
                    {
                        vouchid = x.vid,
                        date = x.date,
                        no = x.no,
                        type = x.type,
                        aname = pledgers.Count > 0 ? pledgers[0].name : (voucheritems.Count > 0 ? voucheritems[0].name : ""),
                        name = x.name,
                        id = x.id,
                        dr = x.dr,
                        cr = x.cr,
                        gstno = pledgers.Count > 0 ? pledgers[0].gstno : (voucheritems.Count > 0 ? voucheritems[0].gstno : x.gstno),
                        invno = x.invno,
                        invdate = x.invdate,
                        chequeno = x.chequeno,
                        narr = x.narr
                    });
                    if (vsummary.Count > 0)
                    {
                        AssignTax(false, vsummary,
                        voucheritems,
                        taxvoucheritems,
                        ledgers,
                        tdsledgers, sgstledgers, cgstledgers, igstledgers,
                        dutyledgers, tcsledgers, cessledgers, rndledgers);
                        result.AddRange(vsummary);
                    }
                }
            }
            return result;
        }

        public List<FinanceAnalyse.VoucherSummary> BuildSummary(FinanceAnalyseCreteria item, List<FinanceAnalyse.VoucherDetail> sourcelist)
        {
            var ledgerids = sourcelist.ConvertAll(x => x.id).Distinct().ToList();
            LedgerDataService ledgerDataService = new LedgerDataService() { Context = this.Context };
            var ledgers = ledgerDataService.Search(new LedgerCreteria()
            {
                ids = ledgerids
            });
            var taxledgers = ledgers.FindAll(x => x.typeid == 16 && x.taxmode >= 1 && x.taxmode <= 7);
            var dutyledgers = taxledgers.FindAll(x => x.typeid == 16 && x.taxmode == 1).ConvertAll(x => x.id).ToList();
            var cessledgers = taxledgers.FindAll(x => x.typeid == 16 && x.taxmode == 2).ConvertAll(x => x.id).ToList();
            var igstledgers = taxledgers.FindAll(x => x.typeid == 16 && x.taxmode == 3).ConvertAll(x => x.id).ToList();
            var sgstledgers = taxledgers.FindAll(x => x.typeid == 16 && x.taxmode == 4).ConvertAll(x => x.id).ToList();
            var cgstledgers = taxledgers.FindAll(x => x.typeid == 16 && x.taxmode == 5).ConvertAll(x => x.id).ToList();
            var tdsledgers = taxledgers.FindAll(x => x.typeid == 16 && x.taxmode == 6).ConvertAll(x => x.id).ToList();
            var tcsledgers = taxledgers.FindAll(x => x.typeid == 16 && x.taxmode == 7).ConvertAll(x => x.id).ToList();
            var taxledgerids = ledgers.FindAll(x => x.typeid == 16 && x.taxmode >= 1 && x.taxmode <= 7).ConvertAll(x => x.id).ToList();
            taxledgers.RemoveAll(x => !taxledgerids.Contains(x.id));
            List<FinanceAnalyse.VoucherSummary> result = new List<FinanceAnalyse.VoucherSummary>();
            foreach (var lst in sourcelist.GroupBy(x => x.vid))
            {
                var voucheritems = lst.ToList();
                var taxvoucheritems = voucheritems.FindAll(x => taxledgerids.Contains(x.id));
                var nontaxvoucheritems = voucheritems.FindAll(x => !taxledgerids.Contains(x.id));
                if (nontaxvoucheritems.Count > 0)
                {
                    var vsummary = nontaxvoucheritems.ConvertAll(x => new FinanceAnalyse.VoucherSummary()
                    {
                        vouchid = x.vid,
                        date = x.date,
                        no = x.no,
                        type = x.type,
                        ptype = x.ptype,
                        name = x.name,
                        id = x.id,
                        dr = x.dr,
                        cr = x.cr,
                        invno = x.invno,
                        invdate = x.invdate,
                        chequeno = x.chequeno,
                        narr = x.narr,
                        crby = x.crby
                    });
                    if (taxvoucheritems.Count > 0)
                    {
                        vsummary[0].duty = (dutyledgers.Count == 0) ? 0 : taxvoucheritems.Sum(x => dutyledgers.Contains(x.id) ? x.cr + x.dr : 0);
                        vsummary[0].tds = (tdsledgers.Count == 0) ? 0 : taxvoucheritems.Sum(x => tdsledgers.Contains(x.id) ? x.cr + x.dr : 0);
                        vsummary[0].cgst = (cgstledgers.Count == 0) ? 0 : taxvoucheritems.Sum(x => cgstledgers.Contains(x.id) ? x.cr + x.dr : 0);
                        vsummary[0].sgst = (sgstledgers.Count == 0) ? 0 : taxvoucheritems.Sum(x => sgstledgers.Contains(x.id) ? x.cr + x.dr : 0);
                        vsummary[0].igst = (igstledgers.Count == 0) ? 0 : taxvoucheritems.Sum(x => igstledgers.Contains(x.id) ? x.cr + x.dr : 0);
                        vsummary[0].tcs = (tcsledgers.Count == 0) ? 0 : taxvoucheritems.Sum(x => tcsledgers.Contains(x.id) ? x.cr + x.dr : 0);
                        vsummary[0].cess = (cessledgers.Count == 0) ? 0 : taxvoucheritems.Sum(x => cessledgers.Contains(x.id) ? x.cr + x.dr : 0);
                    }
                    result.AddRange(vsummary);
                }
                else
                {
                    result.AddRange(voucheritems.ConvertAll(x => new FinanceAnalyse.VoucherSummary()
                    {
                        vouchid = x.vid,
                        date = x.date,
                        no = x.no,
                        type = x.type,
                        name = x.name,
                        id = x.id,
                        dr = x.dr,
                        cr = x.cr,
                        invno = x.invno,
                        invdate = x.invdate,
                        chequeno = x.chequeno,
                        narr = x.narr,
                        crby = x.crby
                    }));
                }
            }
            return result;
        }
        public List<FinanceAnalyse.Summary> CompareSummary(FinanceAnalyseCreteria item)
        {
            var first = BuildSummary(item);
            item.fromdate = item.comparefromdate;
            item.todate = item.comparetodate;
            var second = BuildSummary(item);
            second.ForEach(x =>
            {
                if (first.Exists(y => y.id == x.id))
                {
                    var ex = first.Find(y => y.id == x.id);
                    ex.values.AddRange(x.values);
                }
                else
                {
                    var exvalues = x.values.ToList();
                    x.values = new List<FinanceAnalyse.Summary.Value>();
                    x.values.Add(new FinanceAnalyse.Summary.Value());
                    x.values.AddRange(exvalues);
                }
            });

            foreach (var itm in first.FindAll(x => x.values.Count == 1).ToList())
            {
                itm.values.Add(new FinanceAnalyse.Summary.Value());
            }
            return first.OrderByDescending(x => x.values.Sum(y => y.cr + y.dr)).ToList();
        }

        public FinanceAnalyse TaxDetails(FinanceAnalyseCreteria item)
        {
            string query = @"with allledgertransactions
                            as
                            (
                                select altledger.displayname,ledgerid,voucher.voucherno,voucher.voucherdate,iscredit,voucher.vouchertype,voucheritems.amount amount from voucheritems
                                inner join voucher on voucher.id = voucherid
                                inner join ledger on ledger.id  = ledgerid
                                inner join ledger altledger on altledger.id  = adjustmentledgerid
                                where voucher.voucherdate>=@fromdate and voucher.voucherdate<@todate 
                                and voucheritems.isactive=true and voucher.isactive=true
                                and ledger.typeid=16 and ledger.taxmode in (3,4,5) {0} 
                                union all 
                                select ledger.displayname,adjustmentledgerid ledgerid,voucher.voucherno,voucher.voucherdate,adjustmentcredit iscredit,
                                voucher.vouchertype,(voucher.amount) amount from voucher
                                inner join ledger on ledger.id  = adjustmentledgerid
							    where voucher.voucherdate>=@fromdate and voucher.voucherdate<@todate 
							    and voucher.isactive=true 
							    and ledger.typeid=16 and ledger.taxmode in (3,4,5) {0} 
                            )
                            select 
                            ledgerid,allledgertransactions.displayname name,ledger.displayname,taxpercentage,taxmode,vouchertype,
                            (case when iscredit then amount else 0 end) cr,
                            (case when iscredit then 0 else amount end) dr,
                            voucherno,voucherdate
                            from allledgertransactions
                            inner join ledger on ledger.id = ledgerid";


            List<FinanceAnalyse.TaxDetail> summaries = new List<FinanceAnalyse.TaxDetail>();
            List<string> conditions = new List<string>();
            if (item.companyid > 0)
            {
                conditions.Add(" voucher.companyid = @companyid ");
            }

            if (item.locations != null && item.locations.Count > 0)
            {
                conditions.Add(" voucher.locationid in (" + String.Join(",", item.locations) + ")");
            }

            if (conditions.Count > 0)
                query = String.Format(query, " and " + String.Join(" and ", conditions));
            else
                query = String.Format(query, "");

            var param = new SortedList<string, object>();
            param.Add("companyid", item.companyid);
            param.Add("fromdate", item.fromdate);
            param.Add("todate", item.todate.AddDays(1));
            using (var reader = ExecuteReader(query, param, 10000))
            {
                while (reader.Read())
                {
                    FinanceAnalyse.TaxDetail summary = new FinanceAnalyse.TaxDetail();
                    summary.id = Convert.ToInt64(reader["ledgerid"]);
                    summary.name = Convert.ToString(reader["name"]);
                    summary.percentage = Convert.ToDecimal(reader["taxpercentage"]);
                    summary.mode = Convert.ToInt32(reader["taxmode"]);
                    summary.vouchertype = Convert.ToInt32(reader["vouchertype"]);
                    summary.dr = Convert.ToDecimal(reader["dr"]);
                    summary.cr = Convert.ToDecimal(reader["cr"]);
                    summary.referencetype = "Voucher";
                    summary.referencedate = Convert.ToDateTime(reader["voucherdate"]);
                    summary.referenceno = Convert.ToString(reader["voucherno"]);
                    summaries.Add(summary);
                }
            }
            summaries.RemoveAll(x => x.mode == 5);
            summaries = summaries.GroupBy(x => new FinanceAnalyse.TaxDetail()
            {
                percentage = x.mode == 3 ? x.percentage : x.percentage * 2,
                name = x.name,
                mode = x.mode == 3 ? 3 : 4,
                referencedate = x.referencedate,
                referenceno = x.referenceno,
                vouchertype = x.vouchertype,
                referencetype = x.referencetype
            }).ToList().ConvertAll(x => new FinanceAnalyse.TaxDetail()
            {
                percentage = x.Key.percentage,
                name = x.Key.name,
                mode = x.Key.mode,
                referencedate = x.Key.referencedate,
                referenceno = x.Key.referenceno,
                vouchertype = x.Key.vouchertype,
                referencetype = x.Key.referencetype,
                cr = x.Sum(y => y.cr),
                dr = x.Sum(y => y.dr)
            });

            query = @"select 'Sales' Mode,sum(billitems.tax) tax,billitems.taxid,
                    billitems.taxpercentage,bill.isinterstatesale,
                    'B2C Bill No' billno,
                    'B2C Customer' name,
                    'NO GST' gstno,
                    bill.billdate::date billdate from bill
                    inner join billitems on billid = bill.id and billitems.isactive=true
                    where bill.isactive=true and bill.billtype != 9
					and bill.isb2b=false
                    and bill.billdate>=@fromdate and bill.billdate<@todate and bill.companyid=@companyid
					group by bill.billdate::date,billitems.taxid,billitems.taxpercentage,bill.isinterstatesale

                    union all 

                    select 'Sales' Mode,sum(billitems.tax) tax,billitems.taxid,
                    billitems.taxpercentage,bill.isinterstatesale,
                    bill.billno billno,
                    customer.name name,
                    customer.gstno,
                    bill.billdate::date billdate from bill
                    inner join billitems on billid = bill.id and billitems.isactive=true
                    left outer join customer on customer.id = customerid
                    where bill.isactive=true and bill.billtype != 9
					and bill.isb2b=true
                    and bill.billdate>=@fromdate and bill.billdate<@todate and bill.companyid=@companyid
					group by bill.billdate::date,customer.name,bill.billno,customer.gstno,
                    billitems.taxid,billitems.taxpercentage,bill.isinterstatesale

                    union all

                    select 'Purchase' mode,sum(lrinvoicetax.taxamount) tax,lrinvoicetax.taxid,
                    lrinvoicetax.percentage,lrinvoice.isinterstatesale,
                    coalesce(lrentryno,invoiceentryno) billno,
                    suppliercompany.name,
                    suppliercompany.tinanddate gstno,
                    entrydate billdate
                    from lrinvoicetax
                    inner join lrinvoice on lrinvoice.id =lrinvoicetax.invoiceid and lrinvoice.settled=false
                    left outer join suppliercompany on suppliercompany.id = lrinvoice.suppliercompanyid
                    left outer join lrentry on lrentry.id = lrinvoice.lrid
                    where lrinvoice.isactive=true and lrinvoicetax.isactive=true
                    and lrinvoice.entrydate>=@fromdate and lrinvoice.entrydate<@todate
                    and lrinvoice.companyid=@companyid
                    group by lrinvoicetax.taxid,
                    lrinvoicetax.percentage,lrinvoice.isinterstatesale,
                    coalesce(lrentryno,invoiceentryno),
                    suppliercompany.name,suppliercompany.tinanddate,entrydate
                    
                    union all
                    select 'Debit Note - '||returntype mode,sum(lrinvoicereturnitems.tax) tax,lrinvoicereturnitems.taxid,
					lrinvoicereturnitems.taxpercentage,lrinvoicereturn.isinterstatesale,
                    returnno billno,suppliercompany.name,
                    suppliercompany.tinanddate gstno,
                    returndate billdate
					from lrinvoicereturn
                    left outer join suppliercompany on suppliercompany.id = lrinvoicereturn.suppliercompanyid
                    inner join lrinvoicereturnitems on lrinvoicereturnitems.lrinvoicereturnid = lrinvoicereturn.id
                    where lrinvoicereturn.isactive=true and lrinvoicereturnitems.isactive=true and
                    lrinvoicereturn.returndate>=@fromdate and lrinvoicereturn.returndate<@todate and
                    lrinvoicereturn.companyid=@companyid
                    group by lrinvoicereturnitems.taxid,suppliercompany.tinanddate,
					lrinvoicereturnitems.taxpercentage,lrinvoicereturn.isinterstatesale,
                    returnno,suppliercompany.name,returndate,lrinvoicereturn.returntype";

            TaxDataService taxDataService = new TaxDataService() { Context = this.Context };
            var inventorytaxlist = taxDataService.Search(new TaxCreteria());

            LedgerDataService ledgerDataService = new LedgerDataService() { Context = this.Context };
            var alltaxledgers = ledgerDataService.Search(new LedgerCreteria()
            {
                typeid = 16
            });

            List<FinanceAnalyse.TaxInventory> inventtaxes = new List<FinanceAnalyse.TaxInventory>();
            using (var reader = ExecuteReader(query, param, 10000))
            {
                while (reader.Read())
                {
                    FinanceAnalyse.TaxDetail summary = new FinanceAnalyse.TaxDetail();
                    summary.percentage = Convert.ToDecimal(reader["taxpercentage"]);
                    summary.referencetype = Convert.ToString(reader["mode"]);
                    summary.referencedate = Convert.ToDateTime(reader["billdate"]);
                    summary.referenceno = Convert.ToString(reader["billno"]);
                    summary.name = reader["name"] == DBNull.Value ? "Counter Sales" : Convert.ToString(reader["name"]);
                    summary.gstno = reader["gstno"] == DBNull.Value ? "Missing GST" : Convert.ToString(reader["gstno"]);

                    if (summary.referencetype == "Sales")
                    {
                        summary.cr = Convert.ToDecimal(reader["tax"]);
                    }
                    else
                    {
                        if (summary.referencetype.StartsWith("Debit Note"))
                        {
                            var types = summary.referencetype.Split('-');
                            summary.referencetype = types[0];
                            int type = 0;
                            int.TryParse(types[1], out type);
                            switch (types[1].Trim())
                            {
                                case "5":
                                    summary.referencetype += "( Discount Note )";
                                    break;
                                case "6":
                                    summary.referencetype += "( Credit Note )";
                                    break;
                                case "7":
                                    summary.referencetype += "( Advance )";
                                    break;
                                case "11":
                                    summary.referencetype += "( Excess Credit Note )";
                                    break;
                                case "12":
                                    summary.referencetype += "( Price Credit Note )";
                                    break;
                            }
                            if (type <= 10)
                                summary.cr = Convert.ToDecimal(reader["tax"]);
                            else
                                summary.dr = Convert.ToDecimal(reader["tax"]);
                        }
                        else
                        {
                            summary.dr = Convert.ToDecimal(reader["tax"]);
                        }
                    }

                    if (Convert.ToBoolean(reader["isinterstatesale"]))
                    {
                        summary.mode = -3;
                    }
                    else
                    {
                        summary.mode = -4;
                    }
                    summaries.Add(summary);
                }
            }
            return new FinanceAnalyse()
            {
                taxdetails = summaries.OrderBy(x => x.referencedate).ToList(),
                taxvouchertypes = summaries.ConvertAll(x => x.vouchertype).Distinct()
            };
        }

        public FinanceAnalyse TaxSummary(FinanceAnalyseCreteria item)
        {
            string query = @"with allledgertransactions
                            as
                            (
                                select ledgerid,iscredit,voucher.vouchertype,sum(voucheritems.amount) amount from voucheritems
                                inner join voucher on voucher.id = voucherid
                                inner join ledger on ledger.id  = ledgerid
                                where voucher.voucherdate>=@fromdate and voucher.voucherdate<@todate 
                                and voucheritems.isactive=true and voucher.isactive=true
                                and ledger.typeid=16 and taxmode in (3,4,5) {0}
                                group by ledgerid,iscredit,voucher.vouchertype
                                union all 
                                select adjustmentledgerid ledgerid,adjustmentcredit iscredit,
                                voucher.vouchertype,sum(voucher.amount) amount from voucher
                                inner join ledger on ledger.id  = adjustmentledgerid
							    where voucher.voucherdate>=@fromdate and voucher.voucherdate<@todate 
							    and voucher.isactive=true 
							    and ledger.typeid=16 and taxmode in (3,4,5) {0}
							    group by adjustmentledgerid,adjustmentcredit,voucher.vouchertype
                            )
                            select 
                            ledgerid,displayname,taxpercentage,taxmode,vouchertype,
                            sum(case when iscredit then amount else 0 end) cr,
                            sum(case when iscredit then 0 else amount end) dr
                            from allledgertransactions
                            inner join ledger on ledger.id = ledgerid
                            group by ledgerid,displayname,taxpercentage,taxmode,vouchertype";


            List<FinanceAnalyse.TaxSummary> summaries = new List<FinanceAnalyse.TaxSummary>();
            List<string> conditions = new List<string>();
            if (item.companyid > 0)
            {
                conditions.Add(" voucher.companyid = @companyid ");
            }

            if (item.locations != null && item.locations.Count > 0)
            {
                conditions.Add(" voucher.locationid in (" + String.Join(",", item.locations) + ")");
            }

            if (conditions.Count > 0)
                query = String.Format(query, " and " + String.Join(" and ", conditions));
            else
                query = String.Format(query, "");

            var param = new SortedList<string, object>();
            param.Add("companyid", item.companyid);
            param.Add("fromdate", item.fromdate);
            param.Add("todate", item.todate.AddDays(1));
            using (var reader = ExecuteReader(query, param, 10000))
            {
                while (reader.Read())
                {
                    FinanceAnalyse.TaxSummary summary = new FinanceAnalyse.TaxSummary();
                    summary.id = Convert.ToInt64(reader["ledgerid"]);
                    summary.name = Convert.ToString(reader["displayname"]);
                    summary.percentage = Convert.ToDecimal(reader["taxpercentage"]);
                    summary.mode = Convert.ToInt32(reader["taxmode"]);
                    summary.vouchertype = Convert.ToInt32(reader["vouchertype"]);
                    summary.dr = Convert.ToDecimal(reader["dr"]);
                    summary.cr = Convert.ToDecimal(reader["cr"]);
                    summary.source = "Voucher";
                    summaries.Add(summary);
                }
            }

            query = @"select
                    'Sales' Mode,sum(billitems.tax) tax,billitems.taxid,
					billitems.taxpercentage,bill.isinterstatesale from billitems
                    inner join bill on bill.id = billid
                    where bill.isactive=true and bill.billtype != 9
                    and bill.billdate>=@fromdate and bill.billdate<@todate and bill.companyid=@companyid
					group by billitems.taxid,
					billitems.taxpercentage,bill.isinterstatesale
                    union all
                    select 'Purchase' mode,sum(lrinvoicetax.taxamount) tax,lrinvoicetax.taxid,
                    lrinvoicetax.percentage,lrinvoice.isinterstatesale
                    from lrinvoicetax
                    inner join lrinvoice on lrinvoice.id =lrinvoicetax.invoiceid and lrinvoice.settled=false
                    where lrinvoice.isactive=true and lrinvoicetax.isactive=true
                    and lrinvoice.entrydate>=@fromdate and lrinvoice.entrydate<@todate
                    and lrinvoice.companyid=@companyid
                    group by lrinvoicetax.taxid,
                    lrinvoicetax.percentage,lrinvoice.isinterstatesale
                    union all
                    select case when returntype<10 then 'DebitNote' else 'CreditNote' end mode,sum(lrinvoicereturnitems.tax) tax,lrinvoicereturnitems.taxid,
					lrinvoicereturnitems.taxpercentage,lrinvoicereturn.isinterstatesale
					from lrinvoicereturn
                    inner join lrinvoicereturnitems on lrinvoicereturnitems.lrinvoicereturnid = lrinvoicereturn.id
                    where lrinvoicereturn.isactive=true and lrinvoicereturnitems.isactive=true and
                    lrinvoicereturn.returndate>=@fromdate and lrinvoicereturn.returndate<@todate and
                    lrinvoicereturn.companyid=@companyid
					group by lrinvoicereturn.isinterstatesale,case when returntype<10 then 'DebitNote' else 'CreditNote' end,
					lrinvoicereturnitems.taxid,lrinvoicereturnitems.taxpercentage";

            TaxDataService taxDataService = new TaxDataService() { Context = this.Context };
            var inventorytaxlist = taxDataService.Search(new TaxCreteria());

            LedgerDataService ledgerDataService = new LedgerDataService() { Context = this.Context };
            var alltaxledgers = ledgerDataService.Search(new LedgerCreteria()
            {
                typeid = 16
            });

            List<FinanceAnalyse.TaxInventory> inventtaxes = new List<FinanceAnalyse.TaxInventory>();
            using (var reader = ExecuteReader(query, param, 10000))
            {
                while (reader.Read())
                {
                    FinanceAnalyse.TaxInventory summary = new FinanceAnalyse.TaxInventory();
                    summary.taxid = Convert.ToInt64(reader["taxid"]);
                    summary.taxpercentage = Convert.ToDecimal(reader["taxpercentage"]);
                    summary.mode = Convert.ToString(reader["mode"]);
                    summary.tax = Convert.ToDecimal(reader["tax"]);
                    summary.isinterstate = Convert.ToBoolean(reader["isinterstatesale"]);
                    inventtaxes.Add(summary);
                }
            }

            inventtaxes.RemoveAll(x => x.tax == 0);

            var allsalestax = new List<Tuple<long, long>>();
            var allpurchasetax = new List<Tuple<long, long>>();

            alltaxledgers.FindAll(x => x.attributes != null &&
                x.attributes.invstaxids != null).ForEach(x =>
                {
                    allsalestax.AddRange(x.attributes.invstaxids.ConvertAll(y => new Tuple<long, long>(y, x.id)));
                });

            alltaxledgers.FindAll(x => x.attributes != null &&
                x.attributes.invptaxids != null).ForEach(x =>
                {
                    allpurchasetax.AddRange(x.attributes.invstaxids.ConvertAll(y => new Tuple<long, long>(y, x.id)));
                });


            var newTaxList = inventorytaxlist.ToList();
            newTaxList.RemoveAll(x =>
            allsalestax.Exists(y => y.Item1 == x.id) &&
            allpurchasetax.Exists(y => y.Item1 == x.id));


            foreach (var purchasetax in inventtaxes.FindAll(x => x.mode != "Sales"))
            {
                var purchaseledgers = allpurchasetax.FindAll(x => x.Item1 == purchasetax.taxid);
                if (purchasetax.isinterstate)
                {
                    var igsttaxleders = alltaxledgers.FindAll(x => x.taxmode == 3 &&
                    x.taxpercentage == purchasetax.taxpercentage &&
                    allpurchasetax.Exists(y => y.Item2 == x.id));
                    if (igsttaxleders.Any())
                    {
                        FinanceAnalyse.TaxSummary summary = new FinanceAnalyse.TaxSummary();
                        summary.id = igsttaxleders[0].id;
                        summary.name = igsttaxleders[0].displayname;
                        summary.percentage = igsttaxleders[0].taxpercentage;
                        summary.mode = 3;
                        if (purchasetax.mode == "Purchase" || purchasetax.mode == "CreditNote")
                        {
                            summary.source = "Inventory";
                            summary.vouchertype = 3;
                            summary.dr = purchasetax.tax;
                        }
                        else
                        {
                            summary.source = "Inventory";
                            summary.vouchertype = 6;
                            summary.cr = purchasetax.tax;
                        }
                        summaries.Add(summary);
                    }
                    else if (inventorytaxlist.Exists(x => x.id == purchasetax.taxid))
                    {
                        FinanceAnalyse.TaxSummary summary = new FinanceAnalyse.TaxSummary();
                        summary.id = purchasetax.taxid * -1;
                        summary.name = "Inventory " + purchasetax.mode + "Inventory " + purchasetax.mode + " " + inventorytaxlist.Find(x => x.id == purchasetax.taxid).name;
                        summary.percentage = purchasetax.taxpercentage;
                        summary.mode = 3;
                        if (purchasetax.mode == "Purchase" || purchasetax.mode == "CreditNote")
                        {
                            summary.source = "Inventory";
                            summary.vouchertype = 3;
                            summary.dr = purchasetax.tax;
                        }
                        else
                        {
                            summary.source = "Inventory";
                            summary.vouchertype = 6;
                            summary.cr = purchasetax.tax;
                        }
                        summaries.Add(summary);
                    }
                    else
                    {
                        FinanceAnalyse.TaxSummary summary = new FinanceAnalyse.TaxSummary();
                        summary.id = 0;
                        summary.name = "Inventory " + purchasetax.mode + "IGST TaxLedger Missing for " + purchasetax.taxpercentage + "%";
                        summary.percentage = purchasetax.taxpercentage;
                        summary.mode = 3;
                        if (purchasetax.mode == "Purchase" || purchasetax.mode == "CreditNote")
                        {
                            summary.source = "Inventory";
                            summary.vouchertype = 3;
                            summary.dr = purchasetax.tax;
                        }
                        else
                        {
                            summary.source = "Inventory";
                            summary.vouchertype = 6;
                            summary.cr = purchasetax.tax;
                        }
                        summaries.Add(summary);
                    }
                }
                else
                {
                    var taxpercentage = purchasetax.taxpercentage > 0 ? purchasetax.taxpercentage / 2 : 0;
                    var sgsttaxleders = alltaxledgers.FindAll(x => x.taxmode == 4 &&
                    x.taxpercentage == taxpercentage &&
                    allpurchasetax.Exists(y => y.Item2 == x.id));
                    if (sgsttaxleders.Any())
                    {
                        FinanceAnalyse.TaxSummary summary = new FinanceAnalyse.TaxSummary();
                        summary.id = sgsttaxleders[0].id;
                        summary.name = sgsttaxleders[0].displayname;
                        summary.percentage = sgsttaxleders[0].taxpercentage;
                        summary.mode = 4;
                        if (purchasetax.mode == "Purchase" || purchasetax.mode == "CreditNote")
                        {
                            summary.source = "Inventory";
                            summary.vouchertype = 3;
                            summary.dr = purchasetax.tax / 2;
                        }
                        else
                        {
                            summary.source = "Inventory";
                            summary.vouchertype = 6;
                            summary.cr = purchasetax.tax / 2;
                        }
                        summaries.Add(summary);
                    }
                    else if (inventorytaxlist.Exists(x => x.id == purchasetax.taxid))
                    {
                        FinanceAnalyse.TaxSummary summary = new FinanceAnalyse.TaxSummary();
                        summary.id = purchasetax.taxid * -1;
                        summary.name = "Inventory " + purchasetax.mode + " " + inventorytaxlist.Find(x => x.id == purchasetax.taxid).name;
                        summary.percentage = purchasetax.taxpercentage;
                        summary.mode = 4;
                        if (purchasetax.mode == "Purchase" || purchasetax.mode == "CreditNote")
                        {
                            summary.source = "Inventory";
                            summary.vouchertype = 3;
                            summary.dr = purchasetax.tax / 2;
                        }
                        else
                        {
                            summary.source = "Inventory";
                            summary.vouchertype = 6;
                            summary.cr = purchasetax.tax / 2;
                        }
                        summaries.Add(summary);
                    }
                    else
                    {
                        FinanceAnalyse.TaxSummary summary = new FinanceAnalyse.TaxSummary();
                        summary.id = 0;
                        summary.name = "Inventory " + purchasetax.mode + "SGST TaxLedger Missing for " + (purchasetax.taxpercentage) + "%";
                        summary.percentage = purchasetax.taxpercentage;
                        summary.mode = 4;
                        if (purchasetax.mode == "Purchase" || purchasetax.mode == "CreditNote")
                        {
                            summary.source = "Inventory";
                            summary.vouchertype = 3;
                            summary.dr = purchasetax.tax / 2;
                        }
                        else
                        {
                            summary.source = "Inventory";
                            summary.vouchertype = 6;
                            summary.cr = purchasetax.tax / 2;
                        }
                        summaries.Add(summary);
                    }


                    var cgsttaxleders = alltaxledgers.FindAll(x => x.taxmode == 5 &&
                    x.taxpercentage == taxpercentage &&
                    allpurchasetax.Exists(y => y.Item2 == x.id));
                    if (cgsttaxleders.Any())
                    {
                        FinanceAnalyse.TaxSummary summary = new FinanceAnalyse.TaxSummary();
                        summary.id = cgsttaxleders[0].id;
                        summary.name = cgsttaxleders[0].displayname;
                        summary.percentage = cgsttaxleders[0].taxpercentage;
                        summary.mode = 5;
                        if (purchasetax.mode == "Purchase" || purchasetax.mode == "CreditNote")
                        {
                            summary.source = "Inventory";
                            summary.vouchertype = 3;
                            summary.dr = purchasetax.tax / 2;
                        }
                        else
                        {
                            summary.source = "Inventory";
                            summary.vouchertype = 6;
                            summary.cr = purchasetax.tax / 2;
                        }
                        summaries.Add(summary);
                    }
                    else if (inventorytaxlist.Exists(x => x.id == purchasetax.taxid))
                    {
                        FinanceAnalyse.TaxSummary summary = new FinanceAnalyse.TaxSummary();
                        summary.id = purchasetax.taxid * -1;
                        summary.name = "Inventory " + purchasetax.mode + " " + inventorytaxlist.Find(x => x.id == purchasetax.taxid).name;
                        summary.percentage = purchasetax.taxpercentage;
                        summary.mode = 5;
                        if (purchasetax.mode == "Purchase" || purchasetax.mode == "CreditNote")
                        {
                            summary.source = "Inventory";
                            summary.vouchertype = 3;
                            summary.dr = purchasetax.tax / 2;
                        }
                        else
                        {
                            summary.source = "Inventory";
                            summary.vouchertype = 6;
                            summary.cr = purchasetax.tax / 2;
                        }
                        summaries.Add(summary);
                    }
                    else
                    {
                        FinanceAnalyse.TaxSummary summary = new FinanceAnalyse.TaxSummary();
                        summary.id = 0;
                        summary.name = "Inventory " + purchasetax.mode + "CGST TaxLedger Missing for " + (purchasetax.taxpercentage / 2) + "%";
                        summary.percentage = purchasetax.taxpercentage;
                        summary.mode = 5;
                        if (purchasetax.mode == "Purchase" || purchasetax.mode == "CreditNote")
                        {
                            summary.source = "Inventory";
                            summary.vouchertype = 3;
                            summary.dr = purchasetax.tax / 2;
                        }
                        else
                        {
                            summary.source = "Inventory";
                            summary.vouchertype = 6;
                            summary.cr = purchasetax.tax / 2;
                        }
                        summaries.Add(summary);
                    }
                }
            }

            foreach (var saletax in inventtaxes.FindAll(x => x.mode == "Sales"))
            {
                var saleledgers = allsalestax.FindAll(x => x.Item1 == saletax.taxid);
                if (saletax.isinterstate)
                {
                    var igsttaxleders = alltaxledgers.FindAll(x => x.taxmode == 3 &&
                    x.taxpercentage == saletax.taxpercentage &&
                    saleledgers.Exists(y => y.Item2 == x.id));
                    if (igsttaxleders.Any())
                    {
                        FinanceAnalyse.TaxSummary summary = new FinanceAnalyse.TaxSummary();
                        summary.id = igsttaxleders[0].id;
                        summary.name = igsttaxleders[0].displayname;
                        summary.percentage = igsttaxleders[0].taxpercentage;
                        summary.mode = 3;
                        summary.vouchertype = 4;
                        summary.cr = saletax.tax;
                        summary.source = "Inventory";
                        summaries.Add(summary);
                    }
                    else if (inventorytaxlist.Exists(x => x.id == saletax.taxid))
                    {
                        FinanceAnalyse.TaxSummary summary = new FinanceAnalyse.TaxSummary();
                        summary.id = saletax.taxid * -1;
                        summary.name = "Inventory Sales IGST " + inventorytaxlist.Find(x => x.id == saletax.taxid).name;
                        summary.percentage = saletax.taxpercentage;
                        summary.mode = 3;
                        summary.vouchertype = 4;
                        summary.source = "Inventory";
                        summary.cr = saletax.tax;
                        summaries.Add(summary);
                    }
                    else
                    {
                        FinanceAnalyse.TaxSummary summary = new FinanceAnalyse.TaxSummary();
                        summary.id = 0;
                        summary.source = "Inventory";
                        summary.name = "IGST TaxLedger Missing for " + saletax.taxpercentage + "%";
                        summary.percentage = saletax.taxpercentage;
                        summary.mode = 3;
                        summary.vouchertype = 4;
                        summary.cr = saletax.tax;
                        summaries.Add(summary);
                    }
                }
                else
                {
                    var taxpercentage = saletax.taxpercentage > 0 ? saletax.taxpercentage / 2 : 0;
                    var sgsttaxleders = alltaxledgers.FindAll(x => x.taxmode == 4 &&
                    x.taxpercentage == taxpercentage &&
                    saleledgers.Exists(y => y.Item2 == x.id));
                    if (sgsttaxleders.Any())
                    {
                        FinanceAnalyse.TaxSummary summary = new FinanceAnalyse.TaxSummary();
                        summary.id = sgsttaxleders[0].id;
                        summary.name = sgsttaxleders[0].displayname;
                        summary.percentage = sgsttaxleders[0].taxpercentage;
                        summary.mode = 4;
                        summary.vouchertype = 4;
                        summary.cr = saletax.tax / 2;
                        summary.source = "Inventory";
                        summaries.Add(summary);
                    }
                    else if (inventorytaxlist.Exists(x => x.id == saletax.taxid))
                    {
                        FinanceAnalyse.TaxSummary summary = new FinanceAnalyse.TaxSummary();
                        summary.id = saletax.taxid * -1;
                        summary.name = "Inventory Sales SGST " + inventorytaxlist.Find(x => x.id == saletax.taxid).name;
                        summary.percentage = saletax.taxpercentage;
                        summary.mode = 4;
                        summary.vouchertype = 4;
                        summary.cr = saletax.tax / 2;
                        summary.source = "Inventory";
                        summaries.Add(summary);
                    }
                    else
                    {
                        FinanceAnalyse.TaxSummary summary = new FinanceAnalyse.TaxSummary();
                        summary.id = 0;
                        summary.name = "SGST TaxLedger Missing for " + (saletax.taxpercentage) + "%";
                        summary.percentage = saletax.taxpercentage;
                        summary.mode = 4;
                        summary.vouchertype = 4;
                        summary.cr = saletax.tax / 2;
                        summary.source = "Inventory";
                        summaries.Add(summary);
                    }


                    var cgsttaxleders = alltaxledgers.FindAll(x => x.taxmode == 5 &&
                    x.taxpercentage == taxpercentage &&
                    saleledgers.Exists(y => y.Item2 == x.id));
                    if (cgsttaxleders.Any())
                    {
                        FinanceAnalyse.TaxSummary summary = new FinanceAnalyse.TaxSummary();
                        summary.id = cgsttaxleders[0].id;
                        summary.name = cgsttaxleders[0].displayname;
                        summary.percentage = cgsttaxleders[0].taxpercentage;
                        summary.mode = 5;
                        summary.vouchertype = 4;
                        summary.cr = saletax.tax / 2;
                        summary.source = "Inventory";
                        summaries.Add(summary);
                    }
                    else if (inventorytaxlist.Exists(x => x.id == saletax.taxid))
                    {
                        FinanceAnalyse.TaxSummary summary = new FinanceAnalyse.TaxSummary();
                        summary.id = saletax.taxid * -1;
                        summary.name = "Inventory Sales CGST " + inventorytaxlist.Find(x => x.id == saletax.taxid).name;
                        summary.percentage = saletax.taxpercentage;
                        summary.mode = 5;
                        summary.vouchertype = 4;
                        summary.cr = saletax.tax / 2;
                        summary.source = "Inventory";
                        summaries.Add(summary);
                    }
                    else
                    {
                        FinanceAnalyse.TaxSummary summary = new FinanceAnalyse.TaxSummary();
                        summary.id = 0;
                        summary.name = "CGST TaxLedger Missing for " + (saletax.taxpercentage) + "%";
                        summary.percentage = saletax.taxpercentage;
                        summary.mode = 5;
                        summary.vouchertype = 4;
                        summary.cr = saletax.tax / 2;
                        summary.source = "Inventory";
                        summaries.Add(summary);
                    }
                }
            }

            List<FinanceAnalyse.TaxSummaryGroup> tsummaries = new List<FinanceAnalyse.TaxSummaryGroup>();
            foreach (var sm in summaries.GroupBy(x => new { x.percentage }))
            {
                var tsg = new FinanceAnalyse.TaxSummaryGroup()
                {
                    percentage = sm.Key.percentage,
                    items = new List<FinanceAnalyse.TaxSummary>()
                };
                foreach (var sv in sm.GroupBy(y => new { y.vouchertype, y.mode, y.source }))
                {
                    tsg.items.Add(new FinanceAnalyse.TaxSummary()
                    {
                        cr = sv.Sum(x => x.cr),
                        dr = sv.Sum(x => x.dr),
                        mode = sv.Key.mode,
                        source = sv.Key.source,
                        vouchertype = sv.Key.vouchertype
                    });
                }
                tsummaries.Add(tsg);
            }
            return new FinanceAnalyse()
            {
                taxsummary = tsummaries,
                taxvouchertypes = summaries.ConvertAll(x => x.vouchertype).Distinct()
            };
        }

        public FinanceAnalyse MonthlySummary(FinanceAnalyseCreteria item)
        {
            string query = @"with allledgertransactions
                            as
                            (
                            select ledgerid,to_char(voucherdate, 'Mon')||'-'|| to_char(voucherdate, 'YYYY') myear,iscredit,sum(voucheritems.amount) amount from voucheritems
                            inner join voucher on voucher.id = voucherid
                            inner join ledger on ledger.id  = ledgerid
                            where voucher.voucherdate>=@fromdate and voucher.voucherdate<@todate and voucheritems.isactive=true and voucher.isactive=true
                            {0}
                            group by ledgerid,iscredit,to_char(voucherdate, 'Mon')||'-'|| to_char(voucherdate, 'YYYY')
                            union all 
                            select adjustmentledgerid ledgerid,to_char(voucherdate, 'Mon')||'-'|| to_char(voucherdate, 'YYYY')  myear,adjustmentcredit iscredit,sum(voucher.amount) amount from voucher
                            inner join ledger on ledger.id  = adjustmentledgerid
                            where voucher.voucherdate>=@fromdate and voucher.voucherdate<@todate and voucher.isactive=true
                            {0}
                            group by adjustmentledgerid,adjustmentcredit,to_char(voucherdate, 'Mon')||'-'|| to_char(voucherdate, 'YYYY')
                            )

                            select ledgerid,myear,ledger.displayname,sum(case when iscredit then amount else 0 end) cr,
                            sum(case when iscredit then 0 else amount end) dr
                            from allledgertransactions
                            inner join ledger on ledger.id = ledgerid
                            group by ledgerid,myear,ledger.displayname";


            List<FinanceAnalyse.MonthSummary> summaries = new List<FinanceAnalyse.MonthSummary>();
            List<string> conditions = new List<string>();
            if (item.companyid > 0)
            {
                conditions.Add(" voucher.companyid = @companyid ");
            }

            if (item.locations != null && item.locations.Count > 0)
            {
                conditions.Add(" voucher.locationid in (" + String.Join(",", item.locations) + ")");
            }

            if (item.types != null && item.types.Count > 0)
            {
                conditions.Add(" ledger.typeid in (" + String.Join(",", item.types) + ")");
            }

            if (conditions.Count > 0)
                query = String.Format(query, " and " + String.Join(" and ", conditions));
            else
                query = String.Format(query, "");

            var param = new SortedList<string, object>();
            param.Add("companyid", item.companyid);
            param.Add("fromdate", item.fromdate);
            param.Add("todate", item.todate.AddDays(1));
            List<string> monthnames = new List<string>();
            using (var reader = ExecuteReader(query, param, 10000))
            {
                while (reader.Read())
                {
                    var myear = Convert.ToString(reader["myear"]);
                    FinanceAnalyse.MonthSummary summary = new FinanceAnalyse.MonthSummary();
                    summary.id = Convert.ToInt64(reader["ledgerid"]);
                    summary.name = Convert.ToString(reader["displayname"]);
                    var index = summaries.FindIndex(x => x.id == summary.id);
                    if (index == -1)
                    {
                        summary.values = new List<FinanceAnalyse.MonthSummary.Value>();
                        summary.values.Add(new FinanceAnalyse.MonthSummary.Value()
                        {
                            myear = myear,
                            cr = Convert.ToDecimal(reader["cr"]),
                            dr = Convert.ToDecimal(reader["dr"]),
                        });
                        summaries.Add(summary);
                    }
                    else
                    {
                        summary = summaries[index];
                        summary.values.Add(new FinanceAnalyse.MonthSummary.Value()
                        {
                            myear = myear,
                            cr = Convert.ToDecimal(reader["cr"]),
                            dr = Convert.ToDecimal(reader["dr"]),
                        });
                    }
                    if (!monthnames.Contains(myear))
                        monthnames.Add(myear);

                }
            }

            var monthnos = monthnames.ConvertAll(x =>
            {
                return BaseEntity.GetDataFromString("01-" + x, "dd-MMM-yyyy");
            });

            return new FinanceAnalyse()
            {
                msummary = summaries.OrderByDescending(x => x.values.Sum(y => y.cr + y.dr)).ToList(),
                mtitles = monthnos.OrderBy(x => x).ToList().ConvertAll(x => x.ToString("MMM-yyyy"))
            };
        }

        public List<FinanceAnalyse.VoucherDetail> BuildInvoiceDetail(FinanceAnalyseCreteria item, List<long> expinctypes)
        {
            string query = @"
                            select voucher.id voucherid,purchasetypeid,voucherdate,voucherno,vouchertype,ledgerid,iscredit,
                            voucheritems.amount amount,ledger.displayname,Voucher.attributes->>'invoiceno' invno,
                            Voucher.attributes->>'invoicedate' invdate,Voucher.attributes->>'bankreference' chequeno,
                            voucher.narration,voucheritems.id vvid,voucher.createdby,'' gstno,ledger.typeid from voucheritems
                            inner join voucher on voucher.id = voucherid
                            inner join ledger on ledger.id  = ledgerid
                            where voucher.voucherdate>=@fromdate and voucher.voucherdate<@todate and voucheritems.isactive=true and voucher.isactive=true
                            {0} 
                            union all 
                            select voucher.id voucherid,purchasetypeid,voucherdate,voucherno,vouchertype,adjustmentledgerid ledgerid,adjustmentcredit iscredit,
                            voucher.amount amount,ledger.displayname,Voucher.attributes->>'invoiceno' invno,
                            Voucher.attributes->>'invoicedate' invdate,Voucher.attributes->>'bankreference' chequeno,voucher.narration,
                            0 vvid,voucher.createdby,ledger.gstno,ledger.typeid from voucher
                            inner join ledger on ledger.id  = adjustmentledgerid
                            where voucher.voucherdate>=@fromdate and voucher.voucherdate<@todate and voucher.isactive=true
                            {0} ";


            var typecondition = "";
            if (expinctypes.Count > 0 && item.types != null && item.types.Count > 0 &&
                item.types.All(x => expinctypes.Contains(x)))
            {
                typecondition = string.Join(",", item.types);
                query = @"with filterrows as (
                            select voucher.id voucherid from voucheritems
                            inner join voucher on voucher.id = voucherid
                            inner join ledger on ledger.id  = ledgerid and ledger.typeid in ({1})
                            where voucher.voucherdate>=@fromdate and voucher.voucherdate<@todate 
                            and voucheritems.isactive=true and voucher.isactive=true
                            {0} 
                            union all 
                            select voucher.id voucherid from voucher
                            inner join ledger on ledger.id  = adjustmentledgerid and ledger.typeid in ({1})
                            where voucher.voucherdate>=@fromdate and voucher.voucherdate<@todate and voucher.isactive=true
                            {0}
                        )
                        select voucher.id voucherid,purchasetypeid,voucherdate,voucherno,vouchertype,ledgerid,iscredit,
                        voucheritems.amount amount,ledger.displayname,Voucher.attributes->>'invoiceno' invno,
                        Voucher.attributes->>'invoicedate' invdate,Voucher.attributes->>'bankreference' chequeno,
                        voucher.narration,voucheritems.id vvid,voucher.createdby,'' gstno,ledger.typeid from voucheritems
                        inner join voucher on voucher.id = voucherid
                        inner join ledger on ledger.id  = ledgerid
                        where voucher.voucherdate>=@fromdate and voucher.voucherdate<@todate and voucheritems.isactive=true and voucher.isactive=true
                        and voucher.id in (select voucherid from filterrows)
                        union all 
                        select voucher.id voucherid,purchasetypeid,voucherdate,voucherno,vouchertype,adjustmentledgerid ledgerid,adjustmentcredit iscredit,
                        voucher.amount amount,ledger.displayname,Voucher.attributes->>'invoiceno' invno,
                        Voucher.attributes->>'invoicedate' invdate,Voucher.attributes->>'bankreference' chequeno,
                        voucher.narration,0 vvid,voucher.createdby,ledger.gstno,ledger.typeid from voucher
                        inner join ledger on ledger.id  = adjustmentledgerid
                        where voucher.voucherdate>=@fromdate and voucher.voucherdate<@todate and voucher.isactive=true
                        and voucher.id in (select voucherid from filterrows)";
            }

            List<FinanceAnalyse.VoucherDetail> summaries = new List<FinanceAnalyse.VoucherDetail>();
            List<string> conditions = new List<string>();
            if (item.companyid > 0)
            {
                conditions.Add(" voucher.companyid = @companyid ");
            }

            if (item.locations != null && item.locations.Count > 0)
            {
                conditions.Add(" voucher.locationid in (" + String.Join(",", item.locations) + ")");
            }

            if (item.types != null && item.types.Count > 0)
            {
                conditions.Add(" ledger.typeid in (" + String.Join(",", item.types) + ")");
            }

            if (item.vtypes != null && item.vtypes.Count > 0)
            {
                conditions.Add(" voucher.vouchertype in (" + String.Join(",", item.vtypes) + ")");
            }

            if (item.ptypes != null && item.ptypes.Count > 0)
            {
                conditions.Add(" voucher.purchasetypeid in (" + String.Join(",", item.ptypes) + ")");
            }

            if (conditions.Count > 0)
                query = String.Format(query, " and " + String.Join(" and ", conditions), typecondition);
            else
                query = String.Format(query, "", typecondition);

            var param = new SortedList<string, object>();
            param.Add("companyid", item.companyid);
            param.Add("fromdate", item.fromdate);
            param.Add("todate", item.todate.AddDays(1));
            using (var reader = ExecuteReader(query, param, 10000))
            {
                while (reader.Read())
                {
                    FinanceAnalyse.VoucherDetail summary = new FinanceAnalyse.VoucherDetail();
                    summary.vid = Convert.ToInt64(reader["voucherid"]);
                    summary.id = Convert.ToInt64(reader["ledgerid"]);
                    summary.name = Convert.ToString(reader["displayname"]);
                    summary.no = Convert.ToString(reader["voucherno"]);
                    summary.type = Convert.ToInt32(reader["vouchertype"]);
                    summary.ptype = reader["purchasetypeid"] == DBNull.Value ? 0 : Convert.ToInt32(reader["purchasetypeid"]);
                    summary.ltype = reader["typeid"] == DBNull.Value ? 0 : Convert.ToInt32(reader["typeid"]);
                    summary.vvid = reader["vvid"] == DBNull.Value ? 0 : Convert.ToInt32(reader["vvid"]);
                    summary.crby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt32(reader["createdby"]);

                    if (Convert.ToBoolean(reader["iscredit"]))
                        summary.cr = Convert.ToDecimal(reader["amount"]);
                    else
                        summary.dr = Convert.ToDecimal(reader["amount"]);

                    summary.date = Convert.ToDateTime(reader["voucherdate"]);
                    summary.invdate = reader["invdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["invdate"]);
                    summary.invno = reader["invno"] == DBNull.Value ? "" : Convert.ToString(reader["invno"]);
                    summary.chequeno = reader["chequeno"] == DBNull.Value ? "" : Convert.ToString(reader["chequeno"]);
                    summary.gstno = reader["gstno"] == DBNull.Value ? "" : Convert.ToString(reader["gstno"]);
                    summary.narr = reader["narration"] == DBNull.Value ? "" : Convert.ToString(reader["narration"]);
                    summaries.Add(summary);
                }
            }

            List<FinanceAnalyse.VoucherDetail> dsummaries = new List<FinanceAnalyse.VoucherDetail>();
            foreach (var grp in summaries.GroupBy(x => new { x.id, x.vid }))
            {
                if (grp.Count() == 1)
                {
                    dsummaries.AddRange(grp.ToList());
                }
                else
                {
                    var first = grp.OrderByDescending(x => x.cr + x.dr).First();
                    first.cr = grp.Sum(x => x.cr);
                    first.dr = grp.Sum(x => x.dr);
                    if (grp.ToList().Exists(x => !string.IsNullOrEmpty(x.gstno)))
                    {
                        first.gstno = grp.ToList().Find(x => !string.IsNullOrEmpty(x.gstno)).gstno;
                    }
                    dsummaries.Add(first);
                }
            }
            return dsummaries;
        }

        public List<FinanceAnalyse.Detail> BuildDetail(FinanceAnalyseCreteria item)
        {
            string query = @"
                            select voucher.id voucherid,purchasetypeid,voucherdate,voucherno,vouchertype,ledgerid,iscredit,voucheritems.amount amount,ledger.displayname from voucheritems
                            inner join voucher on voucher.id = voucherid
                            inner join ledger on ledger.id  = ledgerid
                            where voucher.voucherdate>=@fromdate and voucher.voucherdate<@todate and voucheritems.isactive=true and voucher.isactive=true
                            {0} 
                            union all 
                            select voucher.id voucherid,purchasetypeid,voucherdate,voucherno,vouchertype,adjustmentledgerid ledgerid,adjustmentcredit iscredit,voucher.amount amount,ledger.displayname from voucher
                            inner join ledger on ledger.id  = adjustmentledgerid
                            where voucher.voucherdate>=@fromdate and voucher.voucherdate<@todate and voucher.isactive=true
                            {0} ";


            List<FinanceAnalyse.Detail> summaries = new List<FinanceAnalyse.Detail>();
            List<string> conditions = new List<string>();
            if (item.companyid > 0)
            {
                conditions.Add(" voucher.companyid = @companyid ");
            }

            if (item.locations != null && item.locations.Count > 0)
            {
                conditions.Add(" voucher.locationid in (" + String.Join(",", item.locations) + ")");
            }

            if (item.types != null && item.types.Count > 0)
            {
                conditions.Add(" ledger.typeid in (" + String.Join(",", item.types) + ")");
            }

            if (item.vtypes != null && item.vtypes.Count > 0)
            {
                conditions.Add(" voucher.vouchertype in (" + String.Join(",", item.vtypes) + ")");
            }

            if (conditions.Count > 0)
                query = String.Format(query, " and " + String.Join(" and ", conditions));
            else
                query = String.Format(query, "");

            var param = new SortedList<string, object>();
            param.Add("companyid", item.companyid);
            param.Add("fromdate", item.fromdate);
            param.Add("todate", item.todate.AddDays(1));
            using (var reader = ExecuteReader(query, param, 10000))
            {
                while (reader.Read())
                {
                    FinanceAnalyse.Detail summary = new FinanceAnalyse.Detail();
                    summary.vid = Convert.ToInt64(reader["voucherid"]);
                    summary.id = Convert.ToInt64(reader["ledgerid"]);
                    summary.name = Convert.ToString(reader["displayname"]);
                    summary.no = Convert.ToString(reader["voucherno"]);
                    summary.type = Convert.ToInt32(reader["vouchertype"]);
                    summary.ptype = reader["purchasetypeid"] == DBNull.Value ? 0 : Convert.ToInt32(reader["purchasetypeid"]);

                    if (Convert.ToBoolean(reader["iscredit"]))
                        summary.cr = Convert.ToDecimal(reader["amount"]);
                    else
                        summary.dr = Convert.ToDecimal(reader["amount"]);
                    summary.date = Convert.ToDateTime(reader["voucherdate"]);
                    summaries.Add(summary);
                }
            }
            return summaries;
        }

        public List<FinanceAnalyse.Summary> BuildSummary(FinanceAnalyseCreteria item)
        {
            string query = @"with allledgertransactions
                            as
                            (
                            select ledgerid,locationid,iscredit,sum(voucheritems.amount) amount from voucheritems
                            inner join voucher on voucher.id = voucherid
                            inner join ledger on ledger.id  = ledgerid
                            where voucher.voucherdate>=@fromdate and voucher.voucherdate<@todate and voucheritems.isactive=true and voucher.isactive=true
                            {0}
                            group by ledgerid,iscredit,locationid
                            union all 
                            select adjustmentledgerid ledgerid,locationid,adjustmentcredit iscredit,sum(voucher.amount) amount from voucher
                            inner join ledger on ledger.id  = adjustmentledgerid
                            where voucher.voucherdate>=@fromdate and voucher.voucherdate<@todate and voucher.isactive=true
                            {0}
                            group by adjustmentledgerid,adjustmentcredit,locationid
                            )

                            select ledgerid,locationid,ledger.displayname,sum(case when iscredit then amount else 0 end) cr,
                            sum(case when iscredit then 0 else amount end) dr
                            from allledgertransactions
                            inner join ledger on ledger.id = ledgerid
                            group by ledgerid,locationid,ledger.displayname";

            if (item.modeid == FinanceAnalyseCreteria.Modes.CompanySummary)
                query = @"with allledgertransactions
                            as
                            (
                            select voucher.companyid,ledgerid,iscredit,sum(voucheritems.amount) amount from voucheritems
                            inner join voucher on voucher.id = voucherid
                            inner join ledger on ledger.id  = ledgerid
                            where voucher.voucherdate>=@fromdate and voucher.voucherdate<@todate and voucheritems.isactive=true and voucher.isactive=true
                            {0}
                            group by ledgerid,iscredit,voucher.companyid
                            union all 
                            select voucher.companyid,adjustmentledgerid ledgerid,adjustmentcredit iscredit,sum(voucher.amount) amount from voucher
                            inner join ledger on ledger.id  = adjustmentledgerid
                            where voucher.voucherdate>=@fromdate and voucher.voucherdate<@todate and voucher.isactive=true
                            {0}
                            group by adjustmentledgerid,adjustmentcredit,voucher.companyid
                            )

                            select allledgertransactions.companyid,ledgerid,ledger.displayname,sum(case when iscredit then amount else 0 end) cr,
                            sum(case when iscredit then 0 else amount end) dr
                            from allledgertransactions
                            inner join ledger on ledger.id = ledgerid
                            group by ledgerid,allledgertransactions.companyid,ledger.displayname";

            List<FinanceAnalyse.Summary> summaries = new List<FinanceAnalyse.Summary>();
            List<string> conditions = new List<string>();
            if (item.companyid > 0)
            {
                conditions.Add(" voucher.companyid = @companyid ");
            }

            if (item.locations != null && item.locations.Count > 0)
            {
                conditions.Add(" voucher.locationid in (" + String.Join(",", item.locations) + ")");
            }

            if (item.types != null && item.types.Count > 0)
            {
                conditions.Add(" ledger.typeid in (" + String.Join(",", item.types) + ")");
            }

            if (conditions.Count > 0)
                query = String.Format(query, " and " + String.Join(" and ", conditions));
            else
                query = String.Format(query, "");

            var param = new SortedList<string, object>();
            param.Add("companyid", item.companyid);
            param.Add("fromdate", item.fromdate);
            param.Add("todate", item.todate.AddDays(1));
            using (var reader = ExecuteReader(query, param, 10000))
            {
                while (reader.Read())
                {
                    FinanceAnalyse.Summary summary = new FinanceAnalyse.Summary();
                    summary.id = Convert.ToInt64(reader["ledgerid"]);
                    summary.name = Convert.ToString(reader["displayname"]);
                    summary.values = new List<FinanceAnalyse.Summary.Value>();

                    if (item.modeid == FinanceAnalyseCreteria.Modes.LocationSummary)
                    {
                        if (summaries.Exists(x => x.id == summary.id))
                        {
                            summary = summaries.Find(x => x.id == summary.id);
                        }
                        else
                        {
                            summaries.Add(summary);
                        }
                        summary.values.Add(new FinanceAnalyse.Summary.Value()
                        {
                            lid = Convert.ToInt64(reader["locationid"]),
                            cr = Convert.ToDecimal(reader["cr"]),
                            dr = Convert.ToDecimal(reader["dr"]),
                        });
                    }
                    if (item.modeid == FinanceAnalyseCreteria.Modes.CompanySummary)
                    {
                        if (summaries.Exists(x => x.id == summary.id))
                        {
                            summary = summaries.Find(x => x.id == summary.id);
                        }
                        else
                        {
                            summaries.Add(summary);
                        }
                        summary.values.Add(new FinanceAnalyse.Summary.Value()
                        {
                            lid = Convert.ToInt64(reader["companyid"]),
                            cr = Convert.ToDecimal(reader["cr"]),
                            dr = Convert.ToDecimal(reader["dr"]),
                        });
                    }
                    else
                    {
                        if (summaries.Exists(x => x.id == summary.id))
                        {
                            summary = summaries.Find(x => x.id == summary.id);
                            summary.values[0].cr += Convert.ToDecimal(reader["cr"]);
                            summary.values[0].dr += Convert.ToDecimal(reader["dr"]);
                        }
                        else
                        {
                            summary.values.Add(new FinanceAnalyse.Summary.Value()
                            {
                                cr = Convert.ToDecimal(reader["cr"]),
                                dr = Convert.ToDecimal(reader["dr"]),
                            });
                            summaries.Add(summary);
                        }
                    }

                }
            }
            return summaries;
        }

        internal List<FinanceAnalyse.GSTSummary> GSTInventoryReport(FinanceAnalyseCreteria item)
        {
            string query = @"select lrinvoice.isinterstatesale,lrinvoicetax.taxamount tax,
            lrinvoice.invoiceno,lrinvoice.entrydate,suppliercompany.name,lrinvoicetax.amounton,
            lrinvoice.createdby,lrinvoice.modifiedby,lrinvoice.createdon,lrinvoice.modifiedon,
            lrinvoicetax.taxid,lrinvoicetax.percentage taxpercentage
            from lrinvoicetax
            inner join lrinvoice on lrinvoice.id =lrinvoicetax.invoiceid and lrinvoice.settled=false
            inner join suppliercompany on suppliercompany.id = lrinvoice.suppliercompanyid
            where 
		    lrinvoice.isactive=true 
		    and lrinvoicetax.isactive=true
            and lrinvoice.entrydate>=@fromdate 
		    and lrinvoice.entrydate<@todate {0}";

            List<FinanceAnalyse.GSTSummary> tDSSummaries = new List<FinanceAnalyse.GSTSummary>();
            List<string> conditions = new List<string>();
            if (item.companyid > 0)
            {
                conditions.Add(" lrinvoice.companyid = @companyid ");
            }

            LedgerDataService ledgerDataService = new LedgerDataService() { Context = this.Context };
            var taxledgers = ledgerDataService.Search(new LedgerCreteria()
            {
                typeid = 16
            });

            var ptax = new List<long>();
            var stax = new List<long>();
            foreach (var taxledger in taxledgers.FindAll(x => item.ledgers.Contains(x.id)))
            {
                if (taxledger.attributes != null &&
                    taxledger.attributes.invptaxids != null)
                    ptax.AddRange(taxledger.attributes.invptaxids);

                if (taxledger.attributes != null &&
                    taxledger.attributes.invstaxids != null)
                    stax.AddRange(taxledger.attributes.invstaxids);
            }

            stax = stax.Distinct().ToList();
            ptax = ptax.Distinct().ToList();

            if (item.locations != null && item.locations.Count > 0)
            {
                conditions.Add(" lrentry.retaillocationid in (" + String.Join(",", item.locations) + ")");
            }

            if (ptax != null && ptax.Count > 0)
            {
                conditions.Add(" lrinvoicetax.taxid in (" + String.Join(",", ptax) + ")");
            }

            if (conditions.Count > 0)
                query = String.Format(query, " and " + String.Join(" and ", conditions));
            else
                query = String.Format(query, "");

            List<FinanceAnalyse.GSTSummary> tdsSummary = new List<FinanceAnalyse.GSTSummary>();
            var param = new SortedList<string, object>();
            param.Add("companyid", item.companyid);
            param.Add("fromdate", item.fromdate);
            param.Add("ledgerid", item.ledgerid);
            param.Add("todate", item.todate.AddDays(1));

            if (ptax.Count > 0 || stax.Count == 0)
            {
                using (var reader = ExecuteReader(query, param, 10000))
                {
                    while (reader.Read())
                    {
                        FinanceAnalyse.GSTSummary summary = new FinanceAnalyse.GSTSummary()
                        {
                            createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]),
                            createdon = reader["createdon"] == DBNull.Value ? Convert.ToDateTime(reader["entrydate"]) : Convert.ToDateTime(reader["createdon"]),
                            modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]),
                            modifiedon = reader["modifiedon"] == DBNull.Value ? Convert.ToDateTime(reader["entrydate"]) : Convert.ToDateTime(reader["modifiedon"]),
                        };
                        summary.name = Convert.ToString(reader["name"]);
                        summary.no = Convert.ToString(reader["invoiceno"]);
                        summary.date = Convert.ToDateTime(reader["entrydate"]);
                        summary.type = 3;
                        summary.dr = Convert.ToDecimal(reader["amounton"]);
                        summary.tp = Convert.ToDecimal(reader["taxpercentage"]);
                        var isinterstatesale = reader["isinterstatesale"] == DBNull.Value ? false : Convert.ToBoolean(reader["isinterstatesale"]);
                        if (isinterstatesale)
                        {
                            summary.igst = Convert.ToDecimal(reader["tax"]);
                        }
                        else
                        {
                            summary.cgst = Convert.ToDecimal(reader["tax"]) / 2;
                            summary.sgst = Convert.ToDecimal(reader["tax"]) / 2;
                        }
                        summary.input = summary.dr > 0 ? Convert.ToDecimal(reader["tax"]) : 0;
                        summary.output = summary.cr > 0 ? Convert.ToDecimal(reader["tax"]) : 0;
                        summary.itaxid = Convert.ToInt64(reader["taxid"]);
                        tdsSummary.Add(summary);
                    }
                }

                query = @"select lrinvoicereturn.isinterstatesale,
                    lrinvoicereturn.returnno,lrinvoicereturn.returndate,
					suppliercompany.name,lrinvoicereturnitems.taxpercentage,
                    lrinvoicereturn.createdby,lrinvoicereturn.modifiedby,
					lrinvoicereturn.createdon,lrinvoicereturn.modifiedon,
                    sum(lrinvoicereturnitems.amount) amount,
                    sum(lrinvoicereturnitems.tax) tax,
                    lrinvoicereturnitems.taxid
                    from lrinvoicereturn
                    inner join lrinvoicereturnitems on lrinvoicereturnitems.lrinvoicereturnid = lrinvoicereturn.id
                    inner join suppliercompany on suppliercompany.id = lrinvoicereturn.suppliercompanyid
                    where lrinvoicereturn.isactive=true and lrinvoicereturnitems.isactive=true and
                    lrinvoicereturn.returndate>=@fromdate and lrinvoicereturn.returndate<@todate {0}
                    group by lrinvoicereturn.isinterstatesale,
                    lrinvoicereturn.returnno,lrinvoicereturn.returndate,
					suppliercompany.name,lrinvoicereturnitems.taxid,
                    lrinvoicereturnitems.taxpercentage,
                    lrinvoicereturn.createdby,lrinvoicereturn.modifiedby,
					lrinvoicereturn.createdon,lrinvoicereturn.modifiedon";

                conditions = new List<string>();
                if (item.companyid > 0)
                {
                    conditions.Add(" lrinvoicereturn.companyid = @companyid ");
                }

                if (ptax != null && ptax.Count > 0)
                {
                    conditions.Add(" lrinvoicereturnitems.taxid in (" + String.Join(",", ptax) + ")");
                }

                if (item.locations != null && item.locations.Count > 0)
                {
                    conditions.Add(" lrinvoicereturn.locationid in (" + String.Join(",", item.locations) + ")");
                }
                if (conditions.Count > 0)
                    query = String.Format(query, " and " + String.Join(" and ", conditions));
                else
                    query = String.Format(query, "");

                using (var reader = ExecuteReader(query, param, 10000))
                {
                    while (reader.Read())
                    {
                        FinanceAnalyse.GSTSummary summary = new FinanceAnalyse.GSTSummary()
                        {
                            createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]),
                            modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]),
                            modifiedon = reader["modifiedon"] == DBNull.Value ? Convert.ToDateTime(reader["entrydate"]) : Convert.ToDateTime(reader["modifiedon"]),
                            createdon = reader["createdon"] == DBNull.Value ? Convert.ToDateTime(reader["entrydate"]) : Convert.ToDateTime(reader["createdon"]),
                        };
                        summary.name = Convert.ToString(reader["name"]);
                        summary.no = Convert.ToString(reader["returnno"]);
                        summary.date = Convert.ToDateTime(reader["returndate"]);
                        summary.type = 6;
                        summary.cr = Convert.ToDecimal(reader["amount"]);
                        summary.tp = Convert.ToDecimal(reader["taxpercentage"]);
                        var isinterstatesale = reader["isinterstatesale"] == DBNull.Value ? false : Convert.ToBoolean(reader["isinterstatesale"]);
                        if (isinterstatesale)
                        {
                            summary.igst = Convert.ToDecimal(reader["tax"]);
                        }
                        else
                        {
                            summary.cgst = Convert.ToDecimal(reader["tax"]) / 2;
                            summary.sgst = Convert.ToDecimal(reader["tax"]) / 2;
                        }
                        summary.input = summary.dr > 0 ? Convert.ToDecimal(reader["tax"]) : 0;
                        summary.output = summary.cr > 0 ? Convert.ToDecimal(reader["tax"]) : 0;
                        summary.otaxid = Convert.ToInt64(reader["taxid"]);
                        tdsSummary.Add(summary);
                    }
                }
            }

            if (ptax.Count == 0 || stax.Count > 0)
            {

                query = @"select
                    bill.billdate::date billdate,
                    billitems.taxpercentage,
                    sum(billitems.tax) tax,
                    sum(billitems.receivable) receivable,
                    bill.isinterstatesale,billitems.taxid,
                    case when billtype != 1 then 'COUNTER SALE' else customer.name end customername,
                    case when billtype != 1 then 'COUNTER SALE' else bill.billno end receiptno
                    from billitems
                    inner join bill on bill.id = billid
                    left outer join customer on customer.id = bill.customerid
                    inner join tax on tax.id = billitems.taxid
                    where bill.isactive = true and bill.billtype != 9
                    and bill.billdate >= @fromdate and bill.billdate<@todate and billitems.isactive = true  {0}
                    group by bill.billdate::date,bill.isinterstatesale,billitems.taxid,billitems.taxpercentage,
                    case when billtype != 1 then 'COUNTER SALE' else customer.name end,
                    case when billtype != 1 then 'COUNTER SALE' else bill.billno end";

                conditions = new List<string>();
                if (item.companyid > 0)
                {
                    conditions.Add(" bill.companyid = @companyid ");
                }
                if (stax != null && stax.Count > 0)
                {
                    conditions.Add(" billitems.taxid in (" + String.Join(",", stax) + ")");
                }
                if (item.locations != null && item.locations.Count > 0)
                {
                    conditions.Add(" bill.locationid in (" + String.Join(",", item.locations) + ")");
                }
                if (conditions.Count > 0)
                    query = String.Format(query, " and " + String.Join(" and ", conditions));
                else
                    query = String.Format(query, "");

                using (var reader = ExecuteReader(query, param, 10000))
                {
                    while (reader.Read())
                    {
                        FinanceAnalyse.GSTSummary summary = new FinanceAnalyse.GSTSummary();
                        summary.name = Convert.ToString(reader["customername"]);
                        summary.no = Convert.ToString(reader["receiptno"]);
                        summary.date = Convert.ToDateTime(reader["billdate"]);
                        summary.type = 4;
                        summary.cr = Convert.ToDecimal(reader["receivable"]);
                        summary.tp = Convert.ToDecimal(reader["taxpercentage"]);
                        if (summary.cr < 0)
                        {
                            summary.dr = Math.Abs(summary.cr);
                            summary.cr = 0;
                        }
                        var isinterstatesale = reader["isinterstatesale"] == DBNull.Value ? false : Convert.ToBoolean(reader["isinterstatesale"]);
                        var tax = Math.Abs(Convert.ToDecimal(reader["tax"]));
                        if (isinterstatesale)
                        {
                            summary.igst = tax;
                        }
                        else
                        {
                            summary.cgst = tax / 2;
                            summary.sgst = tax / 2;
                        }
                        summary.input = summary.dr > 0 ? tax : 0;
                        summary.output = summary.cr > 0 ? tax : 0;
                        summary.otaxid = Convert.ToInt64(reader["taxid"]);
                        tdsSummary.Add(summary);
                    }
                }
            }

            return tdsSummary;
        }

        internal List<FinanceAnalyse.TDSSummary> TDSInventoryReport(FinanceAnalyseCreteria item, bool includetdspayment)
        {
            string query = @"select suppliercompany.name,suppliercompany.pan,lrentry.retaillocationid,lrinvoice.baseamount,
            lrinvoice.invoicedate,lrinvoice.invoiceno,tdsamount,tdsledgerid,lrinvoice.entrydate,lrinvoice.createdby,
            lrinvoice.modifiedby,lrinvoice.modifiedon,lrinvoice.createdon from lrinvoice 
            inner join suppliercompany on suppliercompany.id = suppliercompanyid
            left outer join lrentry on lrentry.id = lrid
            where tdsamount>0 and lrinvoice.entrydate>=@fromdate and lrinvoice.entrydate<@todate
            {0}";

            List<FinanceAnalyse.TDSSummary> tDSSummaries = new List<FinanceAnalyse.TDSSummary>();
            List<string> conditions = new List<string>();
            if (item.companyid > 0)
            {
                conditions.Add(" lrinvoice.companyid = @companyid ");
            }

            if (item.ledgerid > 0)
            {
                conditions.Add(" suppliercompany.supplierid =@ledgerid");
            }

            if (item.locations != null && item.locations.Count > 0)
            {
                conditions.Add(" lrentry.retaillocationid in (" + String.Join(",", item.locations) + ")");
            }
            if (conditions.Count > 0)
                query = String.Format(query, " and " + String.Join(" and ", conditions));
            else
                query = String.Format(query, "");

            LedgerDataService ledgerDataService = new LedgerDataService() { Context = this.Context };
            var taxledgers = ledgerDataService.Search(new LedgerCreteria()
            {
                typeid = 16
            });

            List<FinanceAnalyse.TDSSummary> tdsSummary = new List<FinanceAnalyse.TDSSummary>();
            var param = new SortedList<string, object>();
            param.Add("companyid", item.companyid);
            param.Add("fromdate", item.fromdate);
            param.Add("ledgerid", item.ledgerid);
            param.Add("todate", item.todate.AddDays(1));
            using (var reader = ExecuteReader(query, param, 10000))
            {
                while (reader.Read())
                {
                    var tdsledgerid = Convert.ToInt64(reader["tdsledgerid"]);
                    var tled = taxledgers.Find(x => x.id == tdsledgerid);
                    FinanceAnalyse.TDSSummary summary = new FinanceAnalyse.TDSSummary()
                    {
                        createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]),
                        createdon = reader["createdon"] == DBNull.Value ? Convert.ToDateTime(reader["entrydate"]) : Convert.ToDateTime(reader["createdon"]),
                        modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]),
                        modifiedon = reader["modifiedon"] == DBNull.Value ? Convert.ToDateTime(reader["entrydate"]) : Convert.ToDateTime(reader["modifiedon"]),
                    };
                    if (tled != null)
                    {
                        summary.tdsledgername = tled.displayname;
                        summary.tdspercentage = tled.taxpercentage;
                    }
                    summary.name = Convert.ToString(reader["name"]);
                    summary.no = Convert.ToString(reader["invoiceno"]);
                    summary.date = Convert.ToDateTime(reader["entrydate"]);
                    summary.type = 3;
                    summary.dr = Convert.ToDecimal(reader["baseamount"]);
                    summary.pan = Convert.ToString(reader["pan"]);
                    summary.tdsledgerid = tdsledgerid;
                    summary.tds = Convert.ToDecimal(reader["tdsamount"]);
                    tdsSummary.Add(summary);
                }
            }

            if (includetdspayment)
            {
                query = @"select suppliercompany.name,suppliercompany.pan,supplierpayment.locationid,
            supplierpayment.paymentdate,supplierpayment.receiptno,tdsamount,
            tdsledgerid,supplierpayment.paidamount,supplierpayment.createdby,
            supplierpayment.modifiedby,supplierpayment.modifiedon,
            supplierpayment.createdon from supplierpayment 
            inner join suppliercompany on suppliercompany.id = suppliercompanyid
            where tdsamount>0 and tdsoninv = false and supplierpayment.paymentdate>=@fromdate and supplierpayment.paymentdate<@todate
            {0}";

                conditions = new List<string>();
                if (item.companyid > 0)
                {
                    conditions.Add(" supplierpayment.companyid = @companyid ");
                }
                if (item.ledgerid > 0)
                {
                    conditions.Add(" suppliercompany.supplierid =@ledgerid");
                }

                if (item.locations != null && item.locations.Count > 0)
                {
                    conditions.Add(" supplierpayment.locationid in (" + String.Join(",", item.locations) + ")");
                }
                if (conditions.Count > 0)
                    query = String.Format(query, " and " + String.Join(" and ", conditions));
                else
                    query = String.Format(query, "");

                using (var reader = ExecuteReader(query, param, 10000))
                {
                    while (reader.Read())
                    {
                        var tdsledgerid = reader["tdsledgerid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["tdsledgerid"]);
                        if (tdsledgerid == 0)
                        {
                            if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.fin_purtdsledgerid > 0)
                            {
                                tdsledgerid = InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.fin_purtdsledgerid;
                            }
                            else
                            {
                                continue;
                            }
                        }
                        var tled = taxledgers.Find(x => x.id == tdsledgerid);
                        FinanceAnalyse.TDSSummary summary = new FinanceAnalyse.TDSSummary()
                        {
                            createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]),
                            modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]),
                            modifiedon = reader["modifiedon"] == DBNull.Value ? Convert.ToDateTime(reader["entrydate"]) : Convert.ToDateTime(reader["modifiedon"]),
                            createdon = reader["createdon"] == DBNull.Value ? Convert.ToDateTime(reader["entrydate"]) : Convert.ToDateTime(reader["createdon"]),
                        };
                        summary.tdsledgername = tled.displayname;
                        summary.tdspercentage = tled.taxpercentage;
                        summary.name = Convert.ToString(reader["name"]);
                        summary.no = Convert.ToString(reader["receiptno"]);
                        summary.date = Convert.ToDateTime(reader["paymentdate"]);
                        summary.type = 3;
                        summary.dr = Convert.ToDecimal(reader["paidamount"]);
                        summary.pan = Convert.ToString(reader["pan"]);
                        summary.tdsledgerid = tdsledgerid;
                        summary.tds = Convert.ToDecimal(reader["tdsamount"]);
                        tdsSummary.Add(summary);
                    }
                }
            }
            return tdsSummary;
        }

        internal FinanceAnalyse TDSReport(FinanceAnalyseCreteria item)
        {
            string query = @"
                            with filterrows as (
                            select voucher.id voucherid from voucheritems
                            inner join voucher on voucher.id = voucherid
                            inner join ledger on ledger.id  = ledgerid and ledger.typeid=16 and ledger.taxmode = 6
                            where voucher.voucherdate>=@fromdate and voucher.voucherdate<@todate 
                            and voucheritems.isactive=true and voucher.isactive=true
                            {0} 
                            union all 
                            select voucher.id voucherid from voucher
                            inner join ledger on ledger.id  = adjustmentledgerid and ledger.typeid=16 and ledger.taxmode = 6
                            where voucher.voucherdate>=@fromdate and voucher.voucherdate<@todate and voucher.isactive=true
                            {0})

                            select voucher.id voucherid,purchasetypeid,voucherdate,voucherno,vouchertype,ledgerid,iscredit,voucheritems.amount amount,
                            ledger.displayname,ledger.typeid,ledger.taxmode,voucheritems.id viid from voucheritems
                            inner join voucher on voucher.id = voucherid
                            inner join ledger on ledger.id  = ledgerid 
                            --and ledger.typeid in (1,2,19,3,20,6,7,11,12,16) and (ledger.typeid != 16 or taxmode in (2,6,8))
                            where voucher.voucherdate>=@fromdate and voucher.voucherdate<@todate and voucheritems.isactive=true and voucher.isactive=true
                            and voucher.id in (select voucherid from filterrows)
                            union all 
                            select voucher.id voucherid,purchasetypeid,voucherdate,voucherno,vouchertype,adjustmentledgerid ledgerid,adjustmentcredit iscredit,
                            voucher.amount amount,ledger.displayname,ledger.typeid,ledger.taxmode,0 viid from voucher
                            inner join ledger on ledger.id  = adjustmentledgerid 
                            --and ledger.typeid in (1,2,19,3,20,6,7,11,12,16) and (ledger.typeid != 16 or taxmode in (2,6,8))
                            where voucher.voucherdate>=@fromdate and voucher.voucherdate<@todate and voucher.isactive=true
                            and voucher.id in (select voucherid from filterrows)";


            List<FinanceAnalyse.Detail> summaries = new List<FinanceAnalyse.Detail>();
            List<string> conditions = new List<string>();
            if (item.companyid > 0)
            {
                conditions.Add(" voucher.companyid = @companyid ");
            }

            if (item.locations != null && item.locations.Count > 0)
            {
                conditions.Add(" voucher.locationid in (" + String.Join(",", item.locations) + ")");
            }

            if (item.types != null && item.types.Count > 0)
            {
                conditions.Add(" ledger.typeid in (" + String.Join(",", item.types) + ")");
            }

            if (item.vtypes != null && item.vtypes.Count > 0)
            {
                conditions.Add(" voucher.vouchertype in (" + String.Join(",", item.vtypes) + ")");
            }

            if (conditions.Count > 0)
                query = String.Format(query, " and " + String.Join(" and ", conditions));
            else
                query = String.Format(query, "");

            var param = new SortedList<string, object>();
            param.Add("companyid", item.companyid);
            param.Add("fromdate", item.fromdate);
            param.Add("todate", item.todate.AddDays(1));
            using (var reader = ExecuteReader(query, param, 10000))
            {
                while (reader.Read())
                {
                    FinanceAnalyse.Detail summary = new FinanceAnalyse.Detail();
                    summary.vid = Convert.ToInt64(reader["voucherid"]);
                    summary.id = Convert.ToInt64(reader["ledgerid"]);
                    summary.name = Convert.ToString(reader["displayname"]);
                    summary.no = Convert.ToString(reader["voucherno"]);
                    summary.type = Convert.ToInt32(reader["vouchertype"]);
                    summary.ptype = reader["purchasetypeid"] == DBNull.Value ? 0 : Convert.ToInt32(reader["purchasetypeid"]);
                    if (Convert.ToBoolean(reader["iscredit"]))
                        summary.cr = Convert.ToDecimal(reader["amount"]);
                    else
                        summary.dr = Convert.ToDecimal(reader["amount"]);
                    summary.date = Convert.ToDateTime(reader["voucherdate"]);
                    summary.taxmode = Convert.ToInt32(reader["taxmode"]);
                    summary.typeid = Convert.ToInt32(reader["typeid"]);
                    summary.vvid = Convert.ToInt64(reader["viid"]);
                    summaries.Add(summary);
                }
            }

            LedgerDataService ledgerDataService = new LedgerDataService() { Context = this.Context };

            var ledgerids = summaries.ConvertAll(x => x.id).Distinct().ToList();
            var taxledgers = ledgerDataService.Search(new LedgerCreteria()
            {
                ids = ledgerids
            });

            List<FinanceAnalyse.TDSSummary> tDSSummaries = new List<FinanceAnalyse.TDSSummary>();
            foreach (var voucheritems in summaries.GroupBy(x => x.vid))
            {
                var items = voucheritems.ToList();
                if (item.ledgers != null && item.ledgers.Count > 0)
                {
                    if (!items.Exists(x => item.ledgers.Contains(x.id)))
                        continue;
                }
                var partyledger = items.FindAll(x => x.cr > 0 && x.typeid != 16 && x.typeid != 30);
                var tdsledger = items.FindAll(x => x.typeid == 16 && x.taxmode == 6);
                var scledger = items.FindAll(x => x.typeid == 16 && x.taxmode == 8);
                var cessledger = items.FindAll(x => x.typeid == 16 && x.taxmode == 2);
                if (tdsledger.Count > 0)
                {
                    items.RemoveAll(x => x.typeid == 16 || x.typeid == 30);
                    items.RemoveAll(x => x.cr > 0);
                    if (items.Count == 1)
                    {
                        FinanceAnalyse.TDSSummary tDSSummary = new FinanceAnalyse.TDSSummary()
                        {
                            cr = items[0].cr,
                            date = items[0].date,
                            dr = items[0].dr,
                            id = items[0].id,
                            name = items[0].name,
                            sname = items[0].name,
                            no = items[0].no,
                            ptype = items[0].ptype,
                            taxmode = items[0].taxmode,
                            type = items[0].type,
                            typeid = items[0].typeid,
                            vid = items[0].vid,
                            vvid = items[0].vvid,
                        };
                        tDSSummary.tds = tdsledger.Sum(x => x.cr + x.dr);
                        tDSSummary.cess = cessledger.Sum(x => x.cr + x.dr);
                        tDSSummary.surcharge = scledger.Sum(x => x.cr + x.dr);
                        tDSSummary.tdsledgerid = tdsledger.Count == 0 ? 0 : tdsledger.First().id;
                        if (tDSSummary.tdsledgerid > 0 &&
                            taxledgers.Exists(x => x.id == tDSSummary.tdsledgerid))
                        {
                            var tled = taxledgers.Find(x => x.id == tDSSummary.tdsledgerid);
                            tDSSummary.tdsledgername = tled.displayname;
                            tDSSummary.tdspercentage = tled.taxpercentage;
                        }
                        if (scledger.Count > 0 && taxledgers.Exists(x => x.id == scledger[0].id))
                        {
                            var tled = taxledgers.Find(x => x.id == scledger[0].id);
                            tDSSummary.surchargepercentage = tled.taxpercentage;
                        }
                        if (cessledger.Count > 0 && taxledgers.Exists(x => x.id == cessledger[0].id))
                        {
                            var tled = taxledgers.Find(x => x.id == cessledger[0].id);
                            tDSSummary.cesspercentage = tled.taxpercentage;
                        }
                        if (partyledger.Count == 1 &&
                            taxledgers.Exists(x => x.id == partyledger[0].id))
                        {
                            var pledger = taxledgers.Find(x => x.id == partyledger[0].id);
                            tDSSummary.name = pledger.displayname;
                            if (pledger != null &&
                                pledger.attributes != null &&
                                !string.IsNullOrEmpty(pledger.attributes.panno))
                            {
                                tDSSummary.pan = pledger.attributes.panno;
                            }
                        }
                        else if (taxledgers.Exists(x => x.id == tDSSummary.id))
                        {
                            var pledger = taxledgers.Find(x => x.id == tDSSummary.id);
                            if (pledger != null &&
                                pledger.attributes != null &&
                                !string.IsNullOrEmpty(pledger.attributes.panno))
                            {
                                tDSSummary.pan = pledger.attributes.panno;
                            }
                        }

                        tDSSummaries.Add(tDSSummary);
                    }
                    else if (tdsledger.Count > 1)
                    {

                        foreach (var titem in items)
                        {
                            FinanceAnalyse.TDSSummary tDSSummary = new FinanceAnalyse.TDSSummary()
                            {
                                cr = titem.cr,
                                date = titem.date,
                                dr = titem.dr,
                                id = titem.id,
                                name = titem.name,
                                sname = titem.name,
                                no = titem.no,
                                ptype = titem.ptype,
                                taxmode = titem.taxmode,
                                type = titem.type,
                                typeid = titem.typeid,
                                vid = titem.vid,
                                vvid = titem.vvid,
                            };
                            foreach (var tdsled in tdsledger)
                            {
                                var txld = taxledgers.Find(x => x.id == tdsled.id);
                                if (Math.Abs(tdsled.cr - (titem.dr * txld.taxpercentage / 100)) < 2)
                                {
                                    tDSSummary.tdsledgerid = txld.id;
                                    tDSSummary.tds = tdsled.cr;
                                    break;
                                }
                            }

                            //var percentage = (tDSSummary.cr + tDSSummary.dr) / totalamt;

                            //tDSSummary.cess = totalcess > 0 ? (totalcess * percentage / 100) : 0;
                            //tDSSummary.surcharge = totaltcs > 0 ? (totaltcs * percentage / 100) : 0;
                            //tDSSummary.tdsledgerid = tdsledger.Count == 0 ? 0 : tdsledger.First().id;

                            if (tDSSummary.tdsledgerid > 0 &&
                                                       taxledgers.Exists(x => x.id == tDSSummary.tdsledgerid))
                            {
                                var tled = taxledgers.Find(x => x.id == tDSSummary.tdsledgerid);
                                tDSSummary.tdsledgername = tled.displayname;
                                tDSSummary.tdspercentage = tled.taxpercentage;
                            }
                            if (scledger.Count > 0 && taxledgers.Exists(x => x.id == scledger[0].id))
                            {
                                var tled = taxledgers.Find(x => x.id == scledger[0].id);
                                tDSSummary.surchargepercentage = tled.taxpercentage;
                            }
                            if (cessledger.Count > 0 && taxledgers.Exists(x => x.id == cessledger[0].id))
                            {
                                var tled = taxledgers.Find(x => x.id == cessledger[0].id);
                                tDSSummary.cesspercentage = tled.taxpercentage;
                            }
                            if (partyledger.Count == 1 &&
                                                        taxledgers.Exists(x => x.id == partyledger[0].id))
                            {
                                var pledger = taxledgers.Find(x => x.id == partyledger[0].id);
                                tDSSummary.name = pledger.displayname;
                                if (pledger != null &&
                                    pledger.attributes != null &&
                                    !string.IsNullOrEmpty(pledger.attributes.panno))
                                {
                                    tDSSummary.pan = pledger.attributes.panno;
                                }
                            }
                            else if (taxledgers.Exists(x => x.id == tDSSummary.id))
                            {
                                var pledger = taxledgers.Find(x => x.id == tDSSummary.id);
                                if (pledger != null &&
                                    pledger.attributes != null &&
                                    !string.IsNullOrEmpty(pledger.attributes.panno))
                                {
                                    tDSSummary.pan = pledger.attributes.panno;
                                }
                            }
                            if (tDSSummary.cr > 0) continue;
                            tDSSummaries.Add(tDSSummary);
                        }
                    }
                    else
                    {
                        var totalamt = items.Sum(x => x.cr + x.dr);
                        var totaltds = tdsledger.Sum(x => x.cr + x.dr);
                        var totalcess = cessledger.Sum(x => x.cr + x.dr);
                        var totaltcs = scledger.Sum(x => x.cr + x.dr);

                        foreach (var titem in items)
                        {
                            FinanceAnalyse.TDSSummary tDSSummary = new FinanceAnalyse.TDSSummary()
                            {
                                cr = titem.cr,
                                date = titem.date,
                                dr = titem.dr,
                                id = titem.id,
                                name = titem.name,
                                sname = titem.name,
                                no = titem.no,
                                ptype = titem.ptype,
                                taxmode = titem.taxmode,
                                type = titem.type,
                                typeid = titem.typeid,
                                vid = titem.vid,
                                vvid = titem.vvid,
                            };

                            foreach (var tdsled in tdsledger)
                            {
                                var txld = taxledgers.Find(x => x.id == tdsled.id);
                                if (Math.Abs(tdsled.cr - (titem.dr * txld.taxpercentage / 100)) < 2)
                                {
                                    tDSSummary.tdsledgerid = txld.id;
                                    tDSSummary.tds = tdsled.cr;
                                    break;
                                }
                            }

                            //var percentage = (tDSSummary.cr + tDSSummary.dr) / totalamt;
                            //tDSSummary.tds = totaltds > 0 ? (totaltds * percentage / 100) : 0;
                            //tDSSummary.cess = totalcess > 0 ? (totalcess * percentage / 100) : 0;
                            //tDSSummary.surcharge = totaltcs > 0 ? (totaltcs * percentage / 100) : 0;
                            //tDSSummary.tdsledgerid = tdsledger.Count == 0 ? 0 : tdsledger.First().id;

                            if (tDSSummary.tdsledgerid > 0 &&
                                                       taxledgers.Exists(x => x.id == tDSSummary.tdsledgerid))
                            {
                                var tled = taxledgers.Find(x => x.id == tDSSummary.tdsledgerid);
                                tDSSummary.tdsledgername = tled.displayname;
                                tDSSummary.tdspercentage = tled.taxpercentage;
                            }
                            if (scledger.Count > 0 && taxledgers.Exists(x => x.id == scledger[0].id))
                            {
                                var tled = taxledgers.Find(x => x.id == scledger[0].id);
                                tDSSummary.surchargepercentage = tled.taxpercentage;
                            }
                            if (cessledger.Count > 0 && taxledgers.Exists(x => x.id == cessledger[0].id))
                            {
                                var tled = taxledgers.Find(x => x.id == cessledger[0].id);
                                tDSSummary.cesspercentage = tled.taxpercentage;
                            }
                            if (partyledger.Count == 1 &&
                                                        taxledgers.Exists(x => x.id == partyledger[0].id))
                            {
                                var pledger = taxledgers.Find(x => x.id == partyledger[0].id);
                                tDSSummary.name = pledger.displayname;
                                if (pledger != null &&
                                    pledger.attributes != null &&
                                    !string.IsNullOrEmpty(pledger.attributes.panno))
                                {
                                    tDSSummary.pan = pledger.attributes.panno;
                                }
                            }
                            else if (taxledgers.Exists(x => x.id == tDSSummary.id))
                            {
                                var pledger = taxledgers.Find(x => x.id == tDSSummary.id);
                                if (pledger != null &&
                                    pledger.attributes != null &&
                                    !string.IsNullOrEmpty(pledger.attributes.panno))
                                {
                                    tDSSummary.pan = pledger.attributes.panno;
                                }
                            }
                            if (tDSSummary.cr > 0)
                                continue;
                            tDSSummaries.Add(tDSSummary);
                        }
                    }
                }
            }

            tDSSummaries.AddRange(TDSInventoryReport(item, true));
            return new FinanceAnalyse()
            {
                tsummary = tDSSummaries
            };
        }

        internal FinanceAnalyse GSTReport(FinanceAnalyseCreteria item)
        {
            string query = @"
                            with filterrows as (
                            select voucher.id voucherid from voucheritems
                            inner join voucher on voucher.id = voucherid
                            inner join ledger on ledger.id  = ledgerid 
                            where voucher.voucherdate>=@fromdate and voucher.voucherdate<@todate 
                            and voucheritems.isactive=true and voucher.isactive=true
                            and ledger.typeid = 16 and taxmode >=3 and taxmode<=5
                            {0}
                            union all 
                            select voucher.id voucherid from voucher
                            inner join ledger on ledger.id  = adjustmentledgerid 
                            where voucher.voucherdate>=@fromdate and voucher.voucherdate<@todate and voucher.isactive=true
                            {0})

                            select voucher.id voucherid,purchasetypeid,voucherdate,voucherno,vouchertype,ledgerid,iscredit,voucheritems.amount amount,
                            ledger.displayname,ledger.typeid,ledger.taxmode,voucheritems.id viid,ledger.id taxid from voucheritems
                            inner join voucher on voucher.id = voucherid
                            inner join ledger on ledger.id  = ledgerid 
                            where voucher.voucherdate>=@fromdate and voucher.voucherdate<@todate and voucheritems.isactive=true and voucher.isactive=true
                            and voucher.id in (select voucherid from filterrows)
                            union all 
                            select voucher.id voucherid,purchasetypeid,voucherdate,voucherno,vouchertype,adjustmentledgerid ledgerid,adjustmentcredit iscredit,
                            voucher.amount amount,ledger.displayname,ledger.typeid,ledger.taxmode,0 viid,ledger.id taxid from voucher
                            inner join ledger on ledger.id  = adjustmentledgerid 
                            where voucher.voucherdate>=@fromdate and voucher.voucherdate<@todate and voucher.isactive=true
                            and voucher.id in (select voucherid from filterrows)";


            List<FinanceAnalyse.Detail> summaries = new List<FinanceAnalyse.Detail>();
            List<string> conditions = new List<string>();
            if (item.companyid > 0)
            {
                conditions.Add(" voucher.companyid = @companyid ");
            }

            if (item.locations != null && item.locations.Count > 0)
            {
                conditions.Add(" voucher.locationid in (" + String.Join(",", item.locations) + ")");
            }

            if (item.ledgers != null && item.ledgers.Count > 0)
            {
                conditions.Add(" ledger.id in (" + String.Join(",", item.ledgers) + ")");
            }

            if (item.types != null && item.types.Count > 0)
            {
                conditions.Add(" ledger.typeid in (" + String.Join(",", item.types) + ")");
            }

            if (item.vtypes != null && item.vtypes.Count > 0)
            {
                conditions.Add(" voucher.vouchertype in (" + String.Join(",", item.vtypes) + ")");
            }

            if (conditions.Count > 0)
                query = String.Format(query, " and " + String.Join(" and ", conditions));
            else
                query = String.Format(query, "");

            var param = new SortedList<string, object>();
            param.Add("companyid", item.companyid);
            param.Add("fromdate", item.fromdate);
            param.Add("todate", item.todate.AddDays(1));
            using (var reader = ExecuteReader(query, param, 10000))
            {
                while (reader.Read())
                {
                    FinanceAnalyse.Detail summary = new FinanceAnalyse.Detail();
                    summary.vid = Convert.ToInt64(reader["voucherid"]);
                    summary.id = Convert.ToInt64(reader["ledgerid"]);
                    summary.name = Convert.ToString(reader["displayname"]);
                    summary.no = Convert.ToString(reader["voucherno"]);
                    summary.type = Convert.ToInt32(reader["vouchertype"]);
                    summary.ptype = reader["purchasetypeid"] == DBNull.Value ? 0 : Convert.ToInt32(reader["purchasetypeid"]);
                    if (Convert.ToBoolean(reader["iscredit"]))
                        summary.cr = Convert.ToDecimal(reader["amount"]);
                    else
                        summary.dr = Convert.ToDecimal(reader["amount"]);
                    summary.date = Convert.ToDateTime(reader["voucherdate"]);
                    summary.taxmode = Convert.ToInt32(reader["taxmode"]);
                    summary.typeid = Convert.ToInt32(reader["typeid"]);
                    summary.vvid = Convert.ToInt64(reader["viid"]);
                    summary.taxid = Convert.ToInt64(reader["taxid"]);
                    summaries.Add(summary);
                }
            }

            LedgerDataService ledgerDataService = new LedgerDataService() { Context = this.Context };

            var ledgerids = summaries.ConvertAll(x => x.id).Distinct().ToList();
            var taxledgers = ledgerDataService.Search(new LedgerCreteria()
            {
                ids = ledgerids
            });

            List<FinanceAnalyse.GSTSummary> tDSSummaries = new List<FinanceAnalyse.GSTSummary>();
            foreach (var voucheritems in summaries.GroupBy(x => x.vid))
            {
                var items = voucheritems.ToList();
                if (item.ledgers != null && item.ledgers.Count > 0)
                {
                    if (!items.Exists(x => item.ledgers.Contains(x.id)))
                        continue;
                }
                var partyledger = items.FindAll(x => x.typeid != 16 && x.typeid != 30);
                var igstledger = items.FindAll(x => x.typeid == 16 && x.taxmode == 3);
                var sgstledger = items.FindAll(x => x.typeid == 16 && x.taxmode == 4);
                var cgstledger = items.FindAll(x => x.typeid == 16 && x.taxmode == 5);

                decimal taxpercentage = 0;
                long taxid = 0;
                if (igstledger.Count > 0)
                {
                    taxid = igstledger[0].taxid;
                    taxpercentage = taxledgers.Find(x => x.id == taxid).taxpercentage;
                }
                else if (sgstledger.Count > 0)
                {
                    taxid = sgstledger[0].taxid;
                    taxpercentage = taxledgers.Find(x => x.id == taxid).taxpercentage * 2;
                }
                else if (cgstledger.Count > 0)
                {
                    taxid = cgstledger[0].taxid;
                    taxpercentage = taxledgers.Find(x => x.id == taxid).taxpercentage * 2;
                }

                var titem = partyledger.OrderByDescending(x => x.cr + x.dr).First();
                FinanceAnalyse.GSTSummary tDSSummary = new FinanceAnalyse.GSTSummary()
                {
                    cr = titem.cr,
                    date = titem.date,
                    dr = titem.dr,
                    id = titem.id,
                    name = titem.name,
                    no = titem.no,
                    ptype = titem.ptype,
                    taxmode = titem.taxmode,
                    type = titem.type,
                    typeid = titem.typeid,
                    vid = titem.vid,
                    vvid = titem.vvid,
                    taxid = taxid,
                    tp = taxpercentage
                };
                tDSSummary.cgst = cgstledger.Sum(x => x.cr + x.dr);
                tDSSummary.sgst = sgstledger.Sum(x => x.cr + x.dr);
                tDSSummary.igst = igstledger.Sum(x => x.cr + x.dr);

                tDSSummary.input = tDSSummary.cr > 0 ? (tDSSummary.cgst + tDSSummary.sgst + tDSSummary.igst) : 0;
                tDSSummary.output = tDSSummary.dr > 0 ? (tDSSummary.cgst + tDSSummary.sgst + tDSSummary.igst) : 0;

                if ((tDSSummary.cgst + tDSSummary.sgst + tDSSummary.igst) > 0)
                    tDSSummaries.Add(tDSSummary);
            }
            var inventorytaxes = GSTInventoryReport(item);
            tDSSummaries.AddRange(inventorytaxes);
            return new FinanceAnalyse()
            {
                gsummary = tDSSummaries
            };
        }

        internal LedgerBalance LedgerBalance(VoucherCreteria item)
        {
            string query = @"select adjustmentcredit,sum(voucheritems.amount) amount from voucher
            inner join voucheritems on voucheritems.voucherid = voucher.id
            inner join ledger on ledger.id = voucheritems.ledgerid and ledger.typeid != 16
            where 
            adjustmentledgerid = @ledgerid 
            and voucher.isactive=true
            and voucheritems.isactive=true
            group by adjustmentcredit";

            DateTime from = DateTime.MinValue;
            DateTime to = DateTime.MinValue;
            if (item.voucherdate.Month > 3)
            {
                from = new DateTime(item.voucherdate.Year, 4, 1);
                to = new DateTime(item.voucherdate.Year + 1, 3, 31);
            }
            else
            {
                from = new DateTime(item.voucherdate.Year - 1, 4, 1);
                to = new DateTime(item.voucherdate.Year, 3, 31);
            }
            var param = new SortedList<string, object>();
            param.Add("from", from);
            param.Add("to", to.AddDays(1));
            param.Add("ledgerid", item.ledgerid);
            decimal cramt = 0;
            decimal dramt = 0;
            using (var reader = ExecuteReader(query, param, 10000))
            {
                while (reader.Read())
                {
                    if (reader["adjustmentcredit"] != DBNull.Value)
                    {
                        if ((bool)reader["adjustmentcredit"])
                        {
                            cramt = Convert.ToDecimal(reader["amount"]);
                        }
                        else
                        {
                            dramt = Convert.ToDecimal(reader["amount"]);
                        }
                    }
                }
            }
            return new LedgerBalance()
            {
                credit = cramt,
                debit = dramt
            };
        }
    }

}
