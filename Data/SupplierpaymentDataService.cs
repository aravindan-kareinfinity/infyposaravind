using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
using System.Text;
using System.Net.Http;
using WebAPI.Data;

namespace InfyPOS.Data
{
    public class SupplierpaymentDataService : SAASData
    {
        const string SQL_Search = @"select supplierpayment.id, supplierpayment.supplierid, supplierpayment.receiptno, supplierpayment.paymentdate, 
supplierpayment.datatransmitted, supplierpayment.paidamount, supplierpayment.isactive, supplierpayment.createdby,supplierpayment.tdsledgerid, 
supplierpayment.modifiedby, supplierpayment.version, supplierpayment.createdon, supplierpayment.modifiedon, supplierpayment.tdsoninv,
supplierpayment.bankid,supplierpayment.paymentmode,referencedetail,instrumentdate,supplierpayment.companyid,suppliercompanyid,supplierpayment.locationid,supplierpayment.remarks,paymentinfo,
hastds,tdspercentage,tdsamount,rounding, suppliercompany.bankaccountname,suppliercompany.bankaccountno,suppliercompany.bankifci,
supplierbank.name bankname,suppliercompany.bankbranch, agent.name agentname,agntid,agntcomm,agntpaid,commissionagent.name commissionagentname,
supplierpayment.createdon,supplierpayment.modifiedon,createdemployee.name createduser, modifiedemployee.name modifieduser, companyemailid
from supplierpayment 
left outer join suppliercompany on suppliercompany.id = supplierpayment.suppliercompanyid
left outer join agent commissionagent on commissionagent.id = supplierpayment.agntid
left outer join agent agent on agent.id = suppliercompany.agentid
left outer join referencelist paidbank on paidbank.id = supplierpayment.bankid
left outer join referencelist supplierbank on supplierbank.id = suppliercompany.bankid
left outer join users createduser on createduser.id = supplierpayment.createdby
left outer join employee createdemployee on createdemployee.id = createduser.employeeid
left outer join users modifieduser on modifieduser.id = supplierpayment.modifiedby
left outer join employee modifiedemployee on modifiedemployee.id = modifieduser.employeeid
where supplierpayment.id=@id";


        const string SQL_Insert = @"INSERT INTO public.supplierpayment(supplierid, receiptno, paymentdate, datatransmitted, paidamount, isactive, createdby, modifiedby, version, createdon, modifiedon,bankid,paymentmode,referencedetail,instrumentdate,companyid,suppliercompanyid,locationid,remarks,paymentinfo,hastds,tdspercentage,tdsamount,rounding,agntid,agntcomm,agntpaid,tdsoninv,tdsledgerid) 
            VALUES(@supplierid, @receiptno, @paymentdate, @datatransmitted, @paidamount, @isactive, @createdby, @modifiedby, @version, @createdon, @modifiedon,@bankid,@paymentmode,@referencedetail,@instrumentdate,@companyid,@suppliercompanyid,@locationid,@remarks,@paymentinfo,@hastds,@tdspercentage,@tdsamount,@rounding,@agntid,@agntcomm,@agntpaid,@tdsoninv,@tdsledgerid) RETURNING id";

        const string SQL_Update = @"UPDATE public.supplierpayment SET supplierid=@supplierid, receiptno=@receiptno, paymentdate=@paymentdate, datatransmitted=@datatransmitted, paidamount=@paidamount, modifiedby=@modifiedby, version=@version+1, modifiedon=@modifiedon,
        bankid=@bankid,paymentmode=@paymentmode,referencedetail=@referencedetail,instrumentdate=@instrumentdate,locationid=@locationid,hastds=@hastds,tdspercentage=@tdspercentage,tdsamount=@tdsamount,rounding=@rounding,
        companyid=@companyid,suppliercompanyid=@suppliercompanyid,remarks=@remarks,paymentinfo=@paymentinfo,agntid=@agntid,agntcomm=@agntcomm,agntpaid=@agntpaid,tdsoninv=@tdsoninv,tdsledgerid=@tdsledgerid
        where id=@id and version=@version RETURNING version,hascancel";

        const string SQL_Delete = @"update public.supplierpayment set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";

        /*
with allinvoice
as
(
select lrinvoice.id,totalamount,invoiceno,suppliercompany.name,
	invoicedate,
to_char(invoicedate, 'DD.MM.YYYY') invdate from lrinvoice
inner join suppliercompany on suppliercompany.id = suppliercompanyid
)

update lrinvoice set settled=false from allinvoice
inner join supplierpending on 
supplierpending.name = allinvoice.name and
supplierpending.invoiceno = allinvoice.invoiceno and
supplierpending.invoicedate = allinvoice.invdate and
supplierpending.amount = allinvoice.totalamount::numeric
where allinvoice.id = lrinvoice.id
            
with allinvoice
as
(
select lrinvoice.id,totalamount,invoiceno,suppliercompany.name,
	invoicedate,
to_char(invoicedate, 'DD.MM.YYYY') invdate from lrinvoice
inner join suppliercompany on suppliercompany.id = suppliercompanyid
)

select supplierpending.* from supplierpending
left outer join allinvoice on 
supplierpending.name = allinvoice.name and
supplierpending.invoiceno = allinvoice.invoiceno and
supplierpending.invoicedate = allinvoice.invdate and
supplierpending.amount = allinvoice.totalamount::numeric
where allinvoice.id is null
*/

        const string SQL_pendingbills = @"select lrinvoice.id lrinvoiceid, lrinvoice.totalamount as totalamount, lrinvoice.discount as invoicediscount,
lrinvoice.taxamount,lrinvoice.invoiceno,lrentry.agentcommission,lrinvoice.hascommission,lrentry.lrentryno,lrentry.slipno,lrentry.entrylocationid locationid,
lrinvoice.invoicedate,lrinvoice.entrydate,paidinvoice.paidamount,suppliercompany.supplierid,lrinvoice.companyid,intrateagrmnt,intrateagrmntdays,
lrinvoice.suppliercompanyid,plannedpayment,suppliercompany.cityid,lrentry.agentid,Systementitymapper.parentid sectionid,lrentry.duedate,lrinvoice.createdby,
suppliercompany.name suppliername,referencelist.name cityname,purchaseorder.pono,purchaseorder.podate,lrinvoice.baseamount,tdsamount,tdsledgerid,
lrinvoice.presettled
from lrinvoice 
left outer join (select sum(paidamount) paidamount,invoiceid
from supplierpaymentinvoice where islrinvoicereturn = false and isactive = true
group by invoiceid) paidinvoice on paidinvoice.invoiceid = lrinvoice.id
left outer join purchaseorder on purchaseorder.id = lrinvoice.poid
left outer join suppliercompany on suppliercompany.id = lrinvoice.suppliercompanyid
left outer join supplier on supplier.id = suppliercompany.supplierid
left outer join referencelist on referencelist.id = suppliercompany.cityid
left outer join lrentry on lrentry.id = lrinvoice.lrid
left outer join Systementitymapper on Systementitymapper.type = 2 and Systementitymapper.childid = lrentry.supplierid
where lrinvoice.isactive = true and lrinvoice.settled = false";

        const string SQL_pendingjobworkbills = @"select Jobworkbillgrn.id lrinvoiceid, Jobworkbillgrn.totalamount as totalamount,
Jobworkbillgrn.taxamount,Jobworkbillgrn.invoiceno,Jobworkbillgrn.companyid,Jobworkbillgrn.locationid,
Jobworkbillgrn.invoicedate,Jobworkbillgrn.entrydate,paidinvoice.paidamount,suppliercompany.supplierid,
Jobworkbillgrn.suppliercompanyid,plannedpayment,suppliercompany.cityid,Systementitymapper.parentid sectionid
from Jobworkbillgrn 
left outer join (select sum(paidamount) paidamount,invoiceid
from supplierpaymentinvoice where islrinvoicereturn = false and supplierpaymentinvoice.isjobwork = true and isactive = true
group by invoiceid) paidinvoice on paidinvoice.invoiceid = Jobworkbillgrn.id
left outer join suppliercompany on suppliercompany.id = Jobworkbillgrn.suppliercompanyid
left outer join supplier on supplier.id = suppliercompany.supplierid
left outer join Systementitymapper on Systementitymapper.type = 2 and Systementitymapper.childid = suppliercompany.supplierid
where Jobworkbillgrn.isactive = true and Jobworkbillgrn.settled = false";

        string SQL_supplierbills = @"with paidinvoice as (
select sum(paidamount) paidamount,invoiceid
from supplierpaymentinvoice inner join lrinvoice on lrinvoice.id = supplierpaymentinvoice.invoiceid
left outer join lrentry on lrentry.id = lrinvoice.lrid 
where lrinvoice.isactive = true and supplierpaymentinvoice.isactive = true and supplierpaymentinvoice.hascancel=false and islrinvoicereturn = false {0}
group by invoiceid
)
select suppliercompany.name, suppliercompany.msmeno, suppliercompany.cityid, suppliercompany.agentid, lrinvoice.id lrinvoiceid, 
(lrinvoice.totalamount) as totalamount,lrinvoice.invoiceno,lrinvoice.entrydate,lrinvoice.invoicedate,paidinvoice.paidamount, lrentry.duedate,
agent.name agentname, lrentry.lrentryno, bank.name bankname, bankbranch, bankifci, bankaccountname, bankaccountno,lrinvoice.presettled,
tinanddate
from lrinvoice 
left outer join lrentry on lrentry.id = lrinvoice.lrid 
inner join suppliercompany on suppliercompany.id = lrinvoice.suppliercompanyid
left outer join agent on agent.id = lrentry.agentid 
left outer join referencelist bank on bank.id = suppliercompany.bankid
left outer join paidinvoice on paidinvoice.invoiceid = lrinvoice.id 
where lrinvoice.isactive = true and lrinvoice.settled = false {0}";

        string SQL_supplierbebitnote = @"with paidinvoice as (
select sum(paidamount) paidamount,invoiceid
from supplierpaymentinvoice inner join lrinvoicereturn on lrinvoicereturn.id = supplierpaymentinvoice.invoiceid
where lrinvoicereturn.isactive = true and supplierpaymentinvoice.isactive = true and supplierpaymentinvoice.hascancel=false 
and (islrinvoicereturn = true or iscreditnote=true) {0}
group by invoiceid
)
select suppliercompany.name, suppliercompany.msmeno, suppliercompany.cityid, lrinvoicereturn.agentid, lrinvoicereturn.id lrinvoiceid, 
(lrinvoicereturn.total) as totalamount,city.name cityname,agent.name agentname, lrinvoicereturn.locationid,
returnno as invoiceno,returndate as invoicedate,paidinvoice.paidamount, lrinvoicereturn.companyid,
bank.name bankname, bankbranch, bankifci, bankaccountname, bankaccountno,lrinvoicereturn.returntype,
tinanddate
from lrinvoicereturn 
inner join suppliercompany on suppliercompany.id = lrinvoicereturn.suppliercompanyid
left outer join agent on agent.id = lrinvoicereturn.agentid 
left outer join referencelist bank on bank.id = suppliercompany.bankid
left outer join referencelist city on city.id = suppliercompany.cityid
left outer join paidinvoice on paidinvoice.invoiceid = lrinvoicereturn.id
where lrinvoicereturn.isactive = true {0} ";


        string SQL_supplierbillsummary = @"with paidinvoice as (
select sum(paidamount) paidamount,invoiceid
from supplierpaymentinvoice inner join lrinvoice on lrinvoice.id = supplierpaymentinvoice.invoiceid
left outer join lrentry on lrentry.id = lrinvoice.lrid 
where lrinvoice.isactive = true and supplierpaymentinvoice.isactive = true and supplierpaymentinvoice.hascancel=false and islrinvoicereturn = false {0}
group by invoiceid
)
select suppliercompany.id,suppliercompany.name, suppliercompany.msmeno, suppliercompany.cityid, count(lrinvoice.id) invoicecount, 
sum(lrinvoice.totalamount-lrinvoice.presettled) totalamount, sum(paidinvoice.paidamount) paidamount,
bank.name bankname, bankbranch, bankifci, bankaccountname, bankaccountno, tinanddate
from lrinvoice 
left outer join lrentry on lrentry.id = lrinvoice.lrid 
inner join suppliercompany on suppliercompany.id = lrinvoice.suppliercompanyid
left outer join referencelist bank on bank.id = suppliercompany.bankid
left outer join paidinvoice on paidinvoice.invoiceid = lrinvoice.id 
where lrinvoice.isactive = true {0}
group by suppliercompany.id,suppliercompany.name, suppliercompany.msmeno, suppliercompany.cityid, bank.name, bankbranch, bankifci, bankaccountname, bankaccountno, tinanddate ";

        string SQL_supplierbebitnotesummary = @"with paidinvoice as (
select sum(paidamount) paidamount,invoiceid
from supplierpaymentinvoice 
inner join lrinvoicereturn on lrinvoicereturn.id = supplierpaymentinvoice.invoiceid
where lrinvoicereturn.isactive = true and supplierpaymentinvoice.isactive = true and supplierpaymentinvoice.hascancel=false 
and (islrinvoicereturn = true or iscreditnote=true) {0}
group by invoiceid
)
select suppliercompany.id,suppliercompany.name, suppliercompany.msmeno, suppliercompany.cityid, count(lrinvoicereturn.id) invoicecount,
sum(lrinvoicereturn.total) totalamount, sum(paidinvoice.paidamount) paidamount,
bank.name bankname, bankbranch, bankifci, bankaccountname, bankaccountno,lrinvoicereturn.returntype, tinanddate
from lrinvoicereturn inner join suppliercompany on suppliercompany.id = lrinvoicereturn.suppliercompanyid
left outer join referencelist bank on bank.id = suppliercompany.bankid
left outer join paidinvoice on paidinvoice.invoiceid = lrinvoicereturn.id
where lrinvoicereturn.isactive = true {0} 
group by suppliercompany.id,suppliercompany.name, suppliercompany.msmeno, suppliercompany.cityid, bank.name, 
bankbranch, bankifci, bankaccountname, bankaccountno,lrinvoicereturn.returntype, tinanddate ";

        //select id,returnno,returndate,total from lrinvoicereturn
        const string SQL_pendingdebitnotes = @"select lrinvoicereturn.id lrinvoiceid, lrinvoicereturn.locationid,
lrinvoicereturn.total as totalamount,lrinvoicereturn.companyid,lrdate,lrno,transport.name transport,remarks,
returnno as invoiceno,returndate as invoicedate,paidinvoice.paidamount,lrinvoicereturn.supplierid,suppliercompanyid,
plannedpayment,suppliercompany.cityid,Systementitymapper.parentid sectionid, lrinvoicereturn.agentid, lrinvoicereturn.returntype,
suppliercompany.name suppliername,referencelist.name cityname,lrinvoicereturn.createdby from lrinvoicereturn 
left outer join (select sum(supplierpaymentinvoice.paidamount) paidamount,invoiceid
from supplierpaymentinvoice 
inner join supplierpayment on supplierpayment.id = supplierpaymentinvoice.supplierpaymentid  where supplierpaymentinvoice.isactive = true 
and supplierpaymentinvoice.hascancel=false 
and (islrinvoicereturn = true or iscreditnote = true)
group by invoiceid) paidinvoice on paidinvoice.invoiceid = lrinvoicereturn.id
inner join suppliercompany on suppliercompany.id = lrinvoicereturn.suppliercompanyid
left outer join referencelist on referencelist.id = suppliercompany.cityid
left outer join transport on transport.id = transportid
left outer join Systementitymapper on Systementitymapper.type = 2 and Systementitymapper.childid = suppliercompany.supplierid
where lrinvoicereturn.isactive = true and lrinvoicereturn.settled = false";

        const string SQL_SoldValues = @"with purchase as ( 
	            select lrinvoiceitems.lrinvoiceid,
	            sum(lrinvoiceitems.buyingqty-lrinvoiceitems.returnqty) purchaseqty from lrinvoiceitems
	            where lrinvoiceitems.isactive=true and lrinvoiceitems.lrinvoiceid in ({0})
	            group by lrinvoiceitems.lrinvoiceid
            ),
            sales as (
            select lrinvoiceitems.lrinvoiceid,
            sum(billitems.qty) billedqty, sum(billitems.qty*lrinvoiceitems.buyingprice::numeric) soldamount,
                sum(buyingqty) buyingqty,sum(sellableqty) sellableqty,
                sum(lrinvoiceitems.damagedqty-lrinvoiceitems.returnqty) returnqty from lrinvoiceitems
	            inner join stock on stock.invoiceitemid = lrinvoiceitems.id
	            inner join salablegoods on  salablegoods.stockid = stock.id
	            inner join billitems on billitems.salablegoodsid = salablegoods.id and billitems.isactive=true
                inner join bill on bill.id=billitems.billid and bill.isactive = true and bill.billtype != 9
	            where stock.isactive=true and lrinvoiceitems.isactive=true and lrinvoiceitems.lrinvoiceid in ({0})
	            group by lrinvoiceitems.lrinvoiceid
            )
            select purchase.lrinvoiceid, purchase.purchaseqty, sales.billedqty, sales.soldamount,
            buyingqty,sellableqty,returnqty
            from purchase left outer join sales on sales.lrinvoiceid = purchase.lrinvoiceid ";

        const string SQL_pendingsections = @"select distinct lrinvoiceitems.lrinvoiceid
    from lrinvoiceitems
	inner join products on products.id = lrinvoiceitems.productid
	where products.salescounterid = {0} and lrinvoiceitems.lrinvoiceid in ({1})";

        public List<Supplierpayment> SearchSummary(SupplierpaymentCreteria _supplierpayment)
        {
            List<Supplierpayment> lstsummary = new List<Supplierpayment>();
            List<Supplierpayment> lstpayment = Search(_supplierpayment);
            var groupedItems = lstpayment.GroupBy(e => new
            {
                e.supplierid,
                e.paymentinfo.bankbranch,
                e.paymentinfo.bankacno,
                e.paymentinfo.bankifsccode
            });

            foreach (var gitem in groupedItems)
            {
                var newbillitem = Newtonsoft.Json.JsonConvert.DeserializeObject<Supplierpayment>(Newtonsoft.Json.JsonConvert.SerializeObject(gitem.First()));
                newbillitem.paidamount = gitem.Sum(e => e.paidamount);
                lstsummary.Add(newbillitem);
            }
            return lstsummary;
        }

        public List<Supplierpayment> Search(SupplierpaymentCreteria _supplierpayment)
        {
            string SQL_SearchAll = @"
with invoicevalue as (
	select Supplierpaymentid paymentid,sum(Supplierpaymentinvoice.paidamount) paidamount,sum(Supplierpaymentinvoice.discountamount) discountamount 
	from Supplierpayment inner join Supplierpaymentinvoice on Supplierpaymentinvoice.Supplierpaymentid = Supplierpayment.id
where Supplierpayment.isactive = true  and supplierpayment.hascancel=false
and Supplierpaymentinvoice.isactive = true and supplierpaymentinvoice.hascancel=false and islrinvoicereturn = false {0}
group by Supplierpaymentid),
debitnotevalue as (
	select Supplierpaymentid paymentid,sum(Supplierpaymentinvoice.paidamount) paidamount,sum(Supplierpaymentinvoice.discountamount) discountamount 
	from Supplierpayment inner join Supplierpaymentinvoice on Supplierpaymentinvoice.Supplierpaymentid = Supplierpayment.id
where Supplierpayment.isactive = true and Supplierpaymentinvoice.isactive = true 
and supplierpayment.hascancel=false and supplierpaymentinvoice.hascancel=false and 
(islrinvoicereturn = true or iscreditnote=true) {0}
group by Supplierpaymentid
),
invoicedetail as (
SELECT supplierpaymentid paymentid, string_agg(invoiceno, ', ') as invoiceno
FROM Supplierpayment
inner join supplierpaymentinvoice on supplierpaymentid = Supplierpayment.id and supplierpaymentinvoice.isactive = true
where Supplierpayment.isactive=true {0}
group by supplierpaymentid
)
select supplierpayment.id, supplierpayment.supplierid, receiptno, paymentdate, datatransmitted, supplierpayment.paidamount, 
supplierpayment.isactive, supplierpayment.createdby, supplierpayment.modifiedby, supplierpayment.version, supplierpayment.createdon, 
supplierpayment.modifiedon, supplierpayment.bankid, paymentmode, referencedetail,
instrumentdate,supplierpayment.companyid, suppliercompanyid,supplierpayment.tdsoninv,supplierpayment.tdsledgerid,
invoicevalue.paidamount invoicetotal, invoicevalue.discountamount invoicediscount, supplierpayment.locationid,
debitnotevalue.paidamount debitnotetotal, debitnotevalue.discountamount debitnotediscount, supplierpayment.remarks,
suppliercompany.bankaccountname,suppliercompany.bankaccountno,suppliercompany.bankifci,referencelist.name bankname,bankbranch,paymentinfo,tdspercentage,tdsamount,
rounding,hastds,invoicedetail.invoiceno, agent.name agentname,agntid,agntcomm,agntpaid,commissionagent.name commissionagentname,
supplierpayment.createdon,supplierpayment.modifiedon,createdemployee.name createduser, modifiedemployee.name modifieduser, companyemailid,
case when 
company.tinanddate is not null and
length(company.tinanddate)>2 and
suppliercompany.tinanddate  is not null and
length(suppliercompany.tinanddate)>2 and
substring(suppliercompany.tinanddate,1,2) != substring(company.tinanddate,1,2) then true else false
end as isinterstatesale,
case when referencelist.additionalinfo->'bankinfo'->>'ifsc' is not null and
length(referencelist.additionalinfo->'bankinfo'->>'ifsc')>4 and
suppliercompany.bankifci is not null and
length(suppliercompany.bankifci)>4 and
substring(suppliercompany.bankifci,1,4) = substring(referencelist.additionalinfo->'bankinfo'->>'ifsc',1,4) then
true else false end as interbank,suppliercompany.cityid
from supplierpayment 
left outer join company on company.id = supplierpayment.companyid
left outer join invoicevalue on supplierpayment.id = invoicevalue.paymentid
left outer join debitnotevalue on supplierpayment.id = debitnotevalue.paymentid
left outer join suppliercompany on suppliercompany.id = supplierpayment.suppliercompanyid
left outer join agent commissionagent on commissionagent.id = supplierpayment.agntid
left outer join agent agent on agent.id = suppliercompany.agentid
left outer join referencelist on referencelist.id = supplierpayment.bankid
left outer join invoicedetail on invoicedetail.paymentid = supplierpayment.id 
left outer join users createduser on createduser.id = supplierpayment.createdby
left outer join employee createdemployee on createdemployee.id = createduser.employeeid
left outer join users modifieduser on modifieduser.id = supplierpayment.modifiedby
left outer join employee modifiedemployee on modifiedemployee.id = modifieduser.employeeid
";

            List<Supplierpayment> lst = new List<Supplierpayment>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_supplierpayment.id != 0)
                {
                    command = GetCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _supplierpayment.id;
                }
                else
                {

                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_SearchAll;
                    qb.AddParameter("supplierpayment.isactive", true, "=");
                    List<string> conditions = new List<string>();
                    if (_supplierpayment.supplierid > 0)
                    {
                        qb.AddParameter("Supplierpayment.supplierid", _supplierpayment.supplierid, "=");
                        conditions.Add(String.Format("{0}{2}{1}", "Supplierpayment.supplierid", _supplierpayment.supplierid, "="));
                    }

                    if (_supplierpayment.locationid > 0)
                    {
                        qb.AddParameter("Supplierpayment.locationid", _supplierpayment.locationid, "=");
                        conditions.Add(String.Format("{0}{2}{1}", "Supplierpayment.locationid", _supplierpayment.locationid, "="));
                    }

                    if (_supplierpayment.companyid > 0)
                    {
                        qb.AddParameter("Supplierpayment.companyid", _supplierpayment.companyid, "=");
                        conditions.Add(String.Format("{0}{2}{1}", "Supplierpayment.companyid", _supplierpayment.companyid, "="));
                    }

                    if (!string.IsNullOrEmpty(_supplierpayment.receiptno))
                    {
                        qb.AddParameter("Supplierpayment.receiptno", _supplierpayment.receiptno, "=");
                        conditions.Add(String.Format("{0}{2}'{1}'", "Supplierpayment.receiptno", Utility.SafeSQL(_supplierpayment.receiptno), "="));
                    }

                    if (!string.IsNullOrEmpty(_supplierpayment.referencedetail))
                    {
                        qb.AddParameter("SupplierPayment.referencedetail", _supplierpayment.referencedetail, "=");
                        conditions.Add(String.Format("{0}{2}'{1}'", "Supplierpayment.receiptno", Utility.SafeSQL(_supplierpayment.referencedetail), "="));
                    }

                    if (_supplierpayment.fromdate != DateTime.MinValue && _supplierpayment.todate != DateTime.MinValue)
                    {
                        qb.AddParameter("Supplierpayment.paymentdate", "paymentfrom", _supplierpayment.fromdate.Date, ">=");
                        qb.AddParameter("Supplierpayment.paymentdate", "paymentto", _supplierpayment.todate.AddDays(1).Date, "<");
                        conditions.Add(String.Format("{0}{2}'{1}'", "Supplierpayment.paymentdate", _supplierpayment.fromdate.Date.ToString("yyyy-MM-dd"), ">="));
                        conditions.Add(String.Format("{0}{2}'{1}'", "Supplierpayment.paymentdate", _supplierpayment.todate.AddDays(1).Date.ToString("yyyy-MM-dd"), "<"));
                    }

                    if (_supplierpayment.supplierid > 0)
                    {
                        if (!string.IsNullOrEmpty(_supplierpayment.invno) && _supplierpayment.invfromdate.Year > 1900)
                        {
                            qb.AddInParamer("supplierpayment.id", @"select supplierpaymentid from supplierpaymentinvoice
                            where supplierid=@supplierid and invoiceno=@invoiceno and 
                            invoicedate>=@invfromdate and invoicedate<@invtodate");
                        }
                        else if (!string.IsNullOrEmpty(_supplierpayment.invno))
                        {
                            qb.AddInParamer("supplierpayment.id", @"select supplierpaymentid from supplierpaymentinvoice
                            where supplierid=@supplierid and invoiceno=@invoiceno");
                        }
                        else if (_supplierpayment.invfromdate.Year > 1900)
                        {
                            qb.AddInParamer("supplierpayment.id", @"select supplierpaymentid from supplierpaymentinvoice
                            where invoicedate>=@invfromdate and invoicedate<@invtodate");
                        }
                        qb.Parameters.Add("supplierid", _supplierpayment.supplierid);
                        if (!string.IsNullOrEmpty(_supplierpayment.invno))
                            qb.Parameters.Add("invoiceno", _supplierpayment.invno);
                        if (_supplierpayment.invfromdate.Year > 1900)
                            qb.Parameters.Add("invfromdate", _supplierpayment.invfromdate.Date);
                        if (_supplierpayment.invtodate.Year > 1900)
                            qb.Parameters.Add("invtodate", _supplierpayment.invtodate.Date.AddDays(1));
                    }

                    if (_supplierpayment.cityid > 0)
                    {
                        qb.AddParameter("suppliercompany.cityid", "cityid", _supplierpayment.cityid, "=");
                    }
                    else if (_supplierpayment.cityid < 0)
                    {
                        if (_supplierpayment.companyid == 0)
                            throw new Exception("Please the company");
                        CompanyDataService companyDataService = new CompanyDataService() { Context = this.Context };
                        var companystate = companyDataService.Search(new CompanyCreteria()
                        {
                            id = _supplierpayment.companyid
                        }).First().stateid;
                        if (companystate <= 0)
                            throw new Exception("Please select the state in company maaster");
                        qb.AddParameter("suppliercompany.stateid", "stateid", companystate,
                           _supplierpayment.cityid == -1 ? "=" : "!=");
                    }

                    qb.SortField = "Supplierpayment.id desc";
                    if (conditions.Count > 0)
                        qb.CurrentQuery = String.Format(qb.CurrentQuery, " and " + String.Join(" and ", conditions));
                    else
                        qb.CurrentQuery = String.Format(qb.CurrentQuery, "");
                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Supplierpayment osupplierpayment = new Supplierpayment();
                        osupplierpayment.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        osupplierpayment.supplierid = reader["supplierid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["supplierid"]);
                        osupplierpayment.suppliercompanyid = reader["suppliercompanyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["suppliercompanyid"]);
                        osupplierpayment.locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                        osupplierpayment.companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"]);
                        osupplierpayment.receiptno = reader["receiptno"] == DBNull.Value ? "" : reader["receiptno"].ToString();
                        osupplierpayment.paymentdate = reader["paymentdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["paymentdate"]);
                        osupplierpayment.datatransmitted = reader["datatransmitted"] == DBNull.Value ? false : Convert.ToBoolean(reader["datatransmitted"]);
                        osupplierpayment.paidamount = reader["paidamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["paidamount"]);
                        osupplierpayment.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        osupplierpayment.tdsledgerid = reader["tdsledgerid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["tdsledgerid"]);
                        osupplierpayment.tdspercentage = reader["tdspercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["tdspercentage"]);
                        osupplierpayment.tdsamount = reader["tdsamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["tdsamount"]);
                        osupplierpayment.hastds = reader["hastds"] == DBNull.Value ? false : Convert.ToBoolean(reader["hastds"]);
                        osupplierpayment.tdsoninv = reader["tdsoninv"] == DBNull.Value ? false : Convert.ToBoolean(reader["tdsoninv"]);
                        osupplierpayment.rounding = reader["rounding"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["rounding"]);
                        osupplierpayment.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        osupplierpayment.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        osupplierpayment.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        osupplierpayment.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        osupplierpayment.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        osupplierpayment.createduser = reader["createduser"] == DBNull.Value ? "" : reader["createduser"].ToString();
                        osupplierpayment.modifieduser = reader["modifieduser"] == DBNull.Value ? "" : reader["modifieduser"].ToString();
                        //osupplierpayment.suppliercompanyid = reader["suppliercompanyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["suppliercompanyid"]);
                        //osupplierpayment.companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"]);
                        osupplierpayment.bankid = reader["bankid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["bankid"]);
                        osupplierpayment.paymentmode = reader["paymentmode"] == DBNull.Value ? 0 : (Supplierpayment.Paymentmode)Convert.ToInt16(reader["paymentmode"]);
                        osupplierpayment.referencedetail = reader["referencedetail"] == DBNull.Value ? "" : reader["referencedetail"].ToString();
                        osupplierpayment.instrumentdate = reader["instrumentdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["instrumentdate"]);
                        osupplierpayment.remarks = reader["remarks"] == DBNull.Value ? "" : reader["remarks"].ToString();
                        osupplierpayment.suppliermailid = reader["companyemailid"] == DBNull.Value ? "" : reader["companyemailid"].ToString();
                        osupplierpayment.supplierbankname = reader["bankname"] == DBNull.Value ? "" : reader["bankname"].ToString();
                        osupplierpayment.supplierbankacname = reader["bankaccountname"] == DBNull.Value ? "" : reader["bankaccountname"].ToString();
                        osupplierpayment.supplierbankacno = reader["bankaccountno"] == DBNull.Value ? "" : reader["bankaccountno"].ToString();
                        osupplierpayment.supplierbankifsccode = reader["bankifci"] == DBNull.Value ? "" : reader["bankifci"].ToString();
                        osupplierpayment.supplierbankbranch = reader["bankbranch"] == DBNull.Value ? "" : reader["bankbranch"].ToString();


                        osupplierpayment.agentid = reader["agntid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["agntid"]);
                        osupplierpayment.agntcomm = reader["agntcomm"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["agntcomm"]);
                        osupplierpayment.agntpaid = reader["agntpaid"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["agntpaid"]);
                        osupplierpayment.agentname = reader["commissionagentname"] == DBNull.Value ? "" : reader["commissionagentname"].ToString();

                        if (string.IsNullOrEmpty(osupplierpayment.agentname))
                            osupplierpayment.agentname = reader["agentname"] == DBNull.Value ? "" : reader["agentname"].ToString();

                        osupplierpayment.paymentinfo_json = reader["paymentinfo"] == DBNull.Value ? "null" : reader["paymentinfo"].ToString();
                        if (_supplierpayment.id == 0)
                        {
                            osupplierpayment.cityid = reader["cityid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["cityid"]);
                            osupplierpayment.invoicenos = reader["invoiceno"] == DBNull.Value ? "" : reader["invoiceno"].ToString();
                            osupplierpayment.isinterstatesale = reader["isinterstatesale"] == DBNull.Value ? false : Convert.ToBoolean(reader["isinterstatesale"]);
                            osupplierpayment.interbank = reader["interbank"] == DBNull.Value ? false : Convert.ToBoolean(reader["interbank"]);
                            if (osupplierpayment.paymentinfo == null) osupplierpayment.paymentinfo = new Supplierpayment.PaymentInfo();
                            osupplierpayment.invoicetotal = reader["invoicetotal"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["invoicetotal"]);
                            osupplierpayment.invoicediscount = reader["invoicediscount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["invoicediscount"]);
                            osupplierpayment.debitnotetotal = reader["debitnotetotal"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["debitnotetotal"]);
                            osupplierpayment.debitnotediscount = reader["debitnotediscount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["debitnotediscount"]);
                            osupplierpayment.totaldiscount = osupplierpayment.invoicediscount + osupplierpayment.debitnotediscount;
                            if (osupplierpayment.paymentinfo == null || osupplierpayment.paymentinfo.bankname == null || osupplierpayment.paymentinfo.bankname == "")
                            {
                                osupplierpayment.paymentinfo.bankname = reader["bankname"] == DBNull.Value ? "" : reader["bankname"].ToString();
                                osupplierpayment.paymentinfo.bankacname = reader["bankaccountname"] == DBNull.Value ? "" : reader["bankaccountname"].ToString();
                                osupplierpayment.paymentinfo.bankacno = reader["bankaccountno"] == DBNull.Value ? "" : reader["bankaccountno"].ToString();
                                osupplierpayment.paymentinfo.bankifsccode = reader["bankifci"] == DBNull.Value ? "" : reader["bankifci"].ToString();
                                osupplierpayment.paymentinfo.bankbranch = reader["bankbranch"] == DBNull.Value ? "" : reader["bankbranch"].ToString();
                            }
                        }
                        lst.Add(osupplierpayment);
                    }
                }



                conn.Close();
                if (_supplierpayment.id > 0 && lst.Count == 1)
                {
                    SupplierpaymentinvoiceDataService subSupplierpaymentinvoice = new SupplierpaymentinvoiceDataService() { Context = this.Context };
                    lst[0].Supplierpaymentinvoice = subSupplierpaymentinvoice.SearchAll(_supplierpayment.id);

                    if (_supplierpayment.includebalance)
                    {
                        var ids = lst[0].Supplierpaymentinvoice.Where(x => !x.islrinvoicereturn).ToList().ConvertAll(e => e.invoiceid);
                        if (ids.Count > 0)
                        {
                            string query = "select lrinvoice.id lrinvoiceid,lrinvoice.totalamount as totalamount from lrinvoice where id in (" + string.Join(",", ids) + ")";
                            using (var reader = ExecuteReader(query, new SortedList<string, object>()))
                            {
                                while (reader.Read())
                                {
                                    long id = reader.GetInt64(0);
                                    decimal amt = reader.GetDecimal(1);
                                    lst[0].Supplierpaymentinvoice.Find(e => e.invoiceid == id).invoiceamount = amt;
                                }
                            }
                        }

                        ids = lst[0].Supplierpaymentinvoice.Where(x => x.islrinvoicereturn).ToList().ConvertAll(e => e.invoiceid);
                        if (ids.Count > 0)
                        {
                            string query = @"select lrdate,lrno,transport.name transport,remarks,lrinvoicereturn.id from lrinvoicereturn
                                 left outer join transport on transport.id = transportid where lrinvoicereturn.id in (" + string.Join(",", lst[0].Supplierpaymentinvoice.Where(x => x.islrinvoicereturn).ToList().ConvertAll(e => e.invoiceid)) + ")";
                            using (var reader = ExecuteReader(query, new SortedList<string, object>()))
                            {
                                while (reader.Read())
                                {
                                    var lrdate = reader.IsDBNull(0) ? DateTime.MinValue : reader.GetDateTime(0);
                                    string lrno = reader.IsDBNull(1) ? "" : reader.GetString(1);
                                    string transport = reader.IsDBNull(2) ? "" : reader.GetString(2);
                                    string remarks = reader.IsDBNull(3) ? "" : reader.GetString(3);
                                    var id = reader.IsDBNull(4) ? 0 : reader.GetInt64(4);
                                    lst[0].Supplierpaymentinvoice.Find(e => e.invoiceid == id).lrdate = lrdate;
                                    lst[0].Supplierpaymentinvoice.Find(e => e.invoiceid == id).lrno = lrno;
                                    lst[0].Supplierpaymentinvoice.Find(e => e.invoiceid == id).transport = transport;
                                    lst[0].Supplierpaymentinvoice.Find(e => e.invoiceid == id).remarks = remarks;
                                }
                            }
                        }
                    }
                    BankchequetransactionDataService bankchequetransactionDataService = new BankchequetransactionDataService() { Context = this.Context };
                    lst[0].bankcheques = bankchequetransactionDataService.Search(new BankchequetransactionCreteria()
                    {
                        sourceid = _supplierpayment.id,
                        sourcetype = "SPMT"
                    });
                }

            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _supplierpayment);
            }
            return lst;
        }
        public class RangeValue
        {
            public int from { get; set; }
            public int to { get; set; }
            public int value { get; set; }
            public string display
            {
                get
                {
                    return string.Format("{0} - {1}", from, to);
                }
            }
        }

        internal List<PlannedInvoice> SearchPlannedPayment(List<long> suppliercompanyid, long locationid)
        {
            List<PlannedInvoice> plannedInvoices = new List<PlannedInvoice>();
            if (suppliercompanyid == null)
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    try
                    {
                        conn.Open();
                        QueryBuilder qb = new QueryBuilder();
                        qb.CurrentQuery = @"select distinct suppliercompanyid from lrinvoice
                                    where plannedpayment>0";
                        NpgsqlCommand command = qb.GetCommand(conn);
                        suppliercompanyid = new List<long>();
                        using (System.Data.IDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                suppliercompanyid.Add(reader.GetInt64(0));
                            }
                        }
                        conn.Close();
                    }
                    catch (Exception exp)
                    {

                    }
                }
            }
            if (suppliercompanyid.Count > 0)
            {
                var pendinglist = SearchPendingPayment(new SupplierpaymentCreteria()
                {
                    suppliercompanyids = suppliercompanyid,
                    locationid = locationid,
                    showplanned = true
                });
                foreach (var item in pendinglist.GroupBy(e => new { e.supplierid, e.companyid, e.locationid }))
                {
                    PlannedInvoice plannedInvoice = new PlannedInvoice();
                    plannedInvoice.supplierid = item.Key.supplierid;
                    plannedInvoice.companyid = item.Key.companyid;
                    plannedInvoice.locationid = item.Key.locationid;
                    plannedInvoice.suppliercompanyid = item.First().suppliercompanyid;
                    plannedInvoice.amount = item.Sum(e => e.isinvoice ? e.plannedpayment : e.plannedpayment * -1);
                    plannedInvoice.dn = item.Sum(e => !e.isinvoice ? e.plannedpayment : 0);
                    plannedInvoice.balance = item.Sum(e => e.isinvoice ? e.balanceamount : e.balanceamount * -1);
                    plannedInvoice.invoicelist = item.ToList().ConvertAll(e => new PlannedInvoice.invoice()
                    {
                        invoicedate = e.invoicedate,
                        lrinvoiceid = e.lrinvoiceid,
                        amount = e.totalamount,
                        balance = e.balanceamount,
                        invoiceno = e.invoiceno,
                        isinvoice = e.isinvoice
                    });
                    plannedInvoices.Add(plannedInvoice);
                }
            }

            return plannedInvoices;
        }

        public List<PendingLrInvoice> SearchPendingJobWorkPayment(SupplierpaymentCreteria _supplierpayment)
        {
            List<RangeValue> rangeValues = new List<RangeValue>();
            if (!string.IsNullOrEmpty(_supplierpayment.ageranges))
            {
                try
                {
                    var ranges = _supplierpayment.ageranges.Split('-');
                    if (ranges.Length == 0)
                    {
                        ranges = _supplierpayment.ageranges.Split(',');
                        int start = int.Parse(ranges[0]);
                        var current = start;
                        if (start > 0)
                        {
                            rangeValues.Add(new RangeValue()
                            {
                                from = 0,
                                to = start,
                                value = start
                            });
                        }
                        for (int i = 1; i < ranges.Length; i++)
                        {
                            start = int.Parse(ranges[i]);
                            rangeValues.Add(new RangeValue()
                            {
                                from = current,
                                to = start,
                                value = start
                            });
                            current = start;
                        }
                    }
                    else
                    {
                        int start = int.Parse(ranges[0]);
                        int increment = int.Parse(ranges[1]);
                        int end = int.Parse(ranges[2]);
                        var current = start;
                        rangeValues.Add(new RangeValue()
                        {
                            from = 0,
                            to = start,
                            value = start
                        });

                        while (current < end)
                        {
                            rangeValues.Add(new RangeValue()
                            {
                                from = current,
                                to = current + increment,
                                value = current + increment
                            });
                            current += increment;
                        }
                    }
                }
                catch (Exception exp)
                {
                    WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _supplierpayment);
                }
            }
            List<PendingLrInvoice> lst = new List<PendingLrInvoice>();
            using (NpgsqlConnection conn = CreateConnection())
            {
                try
                {
                    conn.Open();

                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_pendingjobworkbills;
                    if (_supplierpayment.suppliercompanyid > 0)
                        qb.AddParameter("Jobworkbillgrn.suppliercompanyid", "suppliercompanyid", _supplierpayment.suppliercompanyid, "=");
                    if (_supplierpayment.suppliercompanyids != null)
                        qb.AddInParamer("Jobworkbillgrn.suppliercompanyid", string.Join(",", _supplierpayment.suppliercompanyids));
                    if (_supplierpayment.companyid > 0)
                        qb.AddParameter("Jobworkbillgrn.companyid", "companyid", _supplierpayment.companyid, "=");
                    if (_supplierpayment.cityid > 0)
                        qb.AddParameter("suppliercompany.cityid", "cityid", _supplierpayment.cityid, "=");
                    if (_supplierpayment.supplierid > 0)
                        qb.AddParameter("Jobworkbillgrn.supplierid", "supplierid", _supplierpayment.supplierid, "=");

                    if (_supplierpayment.entryfromdate > DateTime.MinValue)
                        qb.AddParameter("Jobworkbillgrn.entrydate", "lrdatefrom", _supplierpayment.entryfromdate, ">=");
                    if (_supplierpayment.entrytodate > DateTime.MinValue)
                        qb.AddParameter("Jobworkbillgrn.entrydate", "lrdateto", _supplierpayment.entrytodate.Date.AddDays(1), "<");


                    qb.SortField = "Jobworkbillgrn.invoicedate desc";
                    NpgsqlCommand command = qb.GetCommand(conn);
                    using (System.Data.IDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            PendingLrInvoice oinvoice = new PendingLrInvoice();
                            oinvoice.supplierid = reader["supplierid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["supplierid"]);
                            oinvoice.suppliercompanyid = reader["suppliercompanyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["suppliercompanyid"]);
                            oinvoice.lrinvoiceid = reader["lrinvoiceid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["lrinvoiceid"]);
                            oinvoice.companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"]);
                            oinvoice.invoiceno = reader["invoiceno"] == DBNull.Value ? "" : reader["invoiceno"].ToString();
                            oinvoice.invoicedate = reader["invoicedate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["invoicedate"]);
                            oinvoice.totalamount = reader["totalamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["totalamount"]);

                            oinvoice.taxamount = (reader["taxamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["taxamount"]));
                            oinvoice.paidamount = reader["paidamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["paidamount"]);
                            oinvoice.plannedpayment = reader["plannedpayment"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["plannedpayment"]);
                            oinvoice.balanceamount = Math.Abs(oinvoice.totalamount) - Math.Abs(oinvoice.paidamount);


                            oinvoice.cityid = reader["cityid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["cityid"]);

                            oinvoice.sectionid = reader["sectionid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sectionid"]);
                            if (_supplierpayment.sectionid > 0 && oinvoice.sectionid > 0 && oinvoice.sectionid != _supplierpayment.sectionid) continue;
                            oinvoice.invoicetype = "JobWork";
                            oinvoice.isjobwork = true;
                            var days = (DateTimeNow.Date - oinvoice.invoicedate.Date).TotalDays;
                            if (rangeValues.Count > 0)
                            {
                                oinvoice.days = rangeValues.Exists(e => e.from <= days && e.to >= days) ?
                                    rangeValues.Find(e => e.from <= days && e.to >= days).display : rangeValues.Last().to.ToString("N0") + " >";
                            }
                            else
                            {
                                oinvoice.ndays = Convert.ToInt32(days);
                            }
                            if (oinvoice.balanceamount > 0)
                                lst.Add(oinvoice);
                        }
                    }

                    //qb = new QueryBuilder();
                    //qb.CurrentQuery = SQL_pendingdebitnotes;
                    //if (_supplierpayment.suppliercompanyid > 0)
                    //    qb.AddParameter("lrinvoicereturn.suppliercompanyid", "suppliercompanyid", _supplierpayment.suppliercompanyid, "=");
                    //if (_supplierpayment.suppliercompanyids != null && _supplierpayment.suppliercompanyids.Count > 0)
                    //    qb.AddInParamer("lrinvoicereturn.suppliercompanyid", string.Join(",", _supplierpayment.suppliercompanyids));
                    //if (_supplierpayment.companyid > 0)
                    //    qb.AddParameter("lrinvoicereturn.companyid", "companyid", _supplierpayment.companyid, "=");
                    //if (_supplierpayment.locationid > 0)
                    //    qb.AddParameter("lrinvoicereturn.locationid", "locationid", _supplierpayment.locationid, "=");
                    //command = qb.GetCommand(conn);
                    //using (System.Data.IDataReader reader = command.ExecuteReader())
                    //{
                    //    while (reader.Read())
                    //    {
                    //        PendingLrInvoice oinvoice = new PendingLrInvoice();
                    //        oinvoice.supplierid = reader["supplierid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["supplierid"]);
                    //        oinvoice.suppliercompanyid = reader["suppliercompanyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["suppliercompanyid"]);
                    //        oinvoice.lrinvoiceid = reader["lrinvoiceid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["lrinvoiceid"]);
                    //        oinvoice.invoiceno = reader["invoiceno"] == DBNull.Value ? "" : reader["invoiceno"].ToString();
                    //        oinvoice.invoicedate = reader["invoicedate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["invoicedate"]);
                    //        oinvoice.totalamount = (reader["totalamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["totalamount"]));
                    //        //oinvoice.taxamount = (reader["taxamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["taxamount"]));
                    //        //oinvoice.agentcommission = (reader["agentcommission"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["agentcommission"]));
                    //        oinvoice.plannedpayment = (reader["plannedpayment"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["plannedpayment"]));
                    //        oinvoice.debetnoteamount = (reader["totalamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["totalamount"]));
                    //        oinvoice.paidamount = (reader["paidamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["paidamount"])) * -1;
                    //        oinvoice.isdebitnote = true;
                    //        oinvoice.balanceamount = Math.Abs(oinvoice.totalamount) - Math.Abs(oinvoice.paidamount);
                    //        oinvoice.balanceamount *= -1;
                    //        oinvoice.invoicetype = "Debit Note";
                    //        oinvoice.cityid = reader["cityid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["cityid"]);
                    //        oinvoice.agentid = reader["agentid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["agentid"]);
                    //        oinvoice.sectionid = reader["sectionid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sectionid"]);
                    //        var days = (DateTimeNow.Date - oinvoice.invoicedate.Date).TotalDays;
                    //        if (rangeValues.Count > 0)
                    //        {
                    //            oinvoice.days = rangeValues.Exists(e => e.from <= days && e.to >= days) ? rangeValues.Find(e => e.from <= days && e.to >= days).display : rangeValues.Last().to.ToString("N0") + " >";
                    //        }
                    //        else
                    //        {
                    //            oinvoice.days = days.ToString("N0");
                    //        }
                    //        if (oinvoice.totalamount - oinvoice.paidamount > 0)
                    //        {
                    //            lst.Add(oinvoice);
                    //        }
                    //    }
                    //}
                    conn.Close();
                }
                catch (Exception exp)
                {
                    if (conn != null) conn.Close();
                    WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _supplierpayment);
                }
            }
            return lst;
        }
        public PendingLrInvoiceDetail SearchPendingPaymentDetail(PendingLrInvoice _supplierpayment)
        {
            LrinvoiceDataService lrinvoiceDataService = new LrinvoiceDataService() { Context = this.Context };
            var invoiceinfo = lrinvoiceDataService.Search(new LrinvoiceCreteria()
            {
                id = _supplierpayment.lrinvoiceid,
                showitems = true
            });
            PurchaseorderDataService purchaseorderDataService = new PurchaseorderDataService() { Context = this.Context };
            var poinfo = purchaseorderDataService.Search(new PurchaseorderCreteria()
            {
                pono = _supplierpayment.pono,
                statusid = -1,
                includeitems = true
            });

            string stockstatus = string.Format(@"with transactionlist
            as
            (
                select 'stock' as type,salablegoods.locationid,
                sum(salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty)) qty,
                sum((salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty))*stock.buyingprice) value from salablegoods
                inner join stock on stock.id = stockid
                inner join lrinvoiceitems on lrinvoiceitems.id = invoiceitemid
                where lrinvoiceitems.lrinvoiceid = {0}
                and stock.isactive=true and salablegoods.isactive=true and lrinvoiceitems.isactive=true
                group by salablegoods.locationid
                union all
                select 'sales' as type,bill.locationid, sum(billitems.qty) qty,
                sum(billitems.receivable-billitems.tax) value from billitems
                inner join bill on bill.id = billid
                inner join stock on stock.id = stockid
                inner join lrinvoiceitems on lrinvoiceitems.id = invoiceitemid
                where lrinvoiceitems.lrinvoiceid = {0}
                and bill.isactive=true and billitems.isactive=true and bill.billtype != 9 
                group by bill.locationid
                union all
                select 'return' as type,lrinvoicereturn.locationid,sum(lrinvoicereturnitems.qty) qty,
                sum(lrinvoiceitems.rate*lrinvoicereturnitems.qty) value from lrinvoiceitems
                inner join lrinvoicereturnitems on lrinvoicereturnitems.lrinvoiceitemsid = lrinvoiceitems.id
                inner join lrinvoicereturn on lrinvoicereturn.id = lrinvoiceid and lrinvoicereturn.returntype<10
                where 
                lrinvoiceitems.lrinvoiceid = {0}
                and lrinvoiceitems.isactive=true
                and lrinvoicereturnitems.isactive=true
                and lrinvoicereturn.isactive=true
                group by lrinvoicereturn.locationid
            )
            select referencelist.name locationname,transactionlist.* from transactionlist
            left outer join referencelist on referencelist.id = locationid", _supplierpayment.lrinvoiceid);

            List<PendingLrInvoiceDetail.StockInfo> stocklist = new List<PendingLrInvoiceDetail.StockInfo>();
            using (var reader = lrinvoiceDataService.ExecuteReader(stockstatus, null))
            {
                while (reader.Read())
                {
                    var si = new PendingLrInvoiceDetail.StockInfo();
                    si.name = reader["locationname"] == DBNull.Value ? "Unknown" : Convert.ToString(reader["locationname"]);
                    if (stocklist.Exists(x => x.name == si.name))
                    {
                        si = stocklist.Find(x => x.name == si.name);
                    }
                    else
                    {
                        stocklist.Add(si);
                    }
                    if (reader["type"].ToString() == "stock")
                    {
                        si.stock = reader["value"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["value"]);
                        si.stockqty = reader["qty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["qty"]);
                    }
                    else if (reader["type"].ToString() == "sales")
                    {
                        si.sold = reader["value"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["value"]);
                        si.soldqty = reader["qty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["qty"]);
                    }
                    else
                    {
                        si.preturn = reader["value"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["value"]);
                        si.preturnqty = reader["qty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["qty"]);
                    }
                }
            }

            var response = new PendingLrInvoiceDetail()
            {
                source = _supplierpayment,
                stocklist = stocklist,
                purchaseorder = poinfo.Count > 0 ? poinfo[0] : null,
                invoice = invoiceinfo.Count > 0 ? invoiceinfo[0] : null
            };

            if (response.purchaseorder != null &&
                response.purchaseorder.Purchaseorderitems != null &&
                response.invoice != null &&
                response.invoice.Lrinvoiceitems != null)
            {
                var potaxlist = response.purchaseorder.Purchaseorderitems.GroupBy(x => x.taxpercentage).ToList();
                var invtaxlist = response.invoice.Lrinvoiceitems.GroupBy(x => x.purchasetaxpercentage).ToList();
                List<PendingLrInvoiceDetail.TaxMatch> taxlist = new List<PendingLrInvoiceDetail.TaxMatch>();
                foreach (var itax in potaxlist)
                {
                    taxlist.Add(new PendingLrInvoiceDetail.TaxMatch()
                    {
                        percentage = itax.Key,
                        orderqty = itax.Sum(x => x.oqty),
                        ordervalue = itax.Sum(x => x.price * x.oqty),
                        ordertax = itax.Sum(x => x.tax)
                    });
                }

                foreach (var itax in invtaxlist)
                {
                    PendingLrInvoiceDetail.TaxMatch taxMatch = null;
                    if (taxlist.Exists(x => x.percentage == itax.Key))
                    {
                        taxMatch = taxlist.Find(x => x.percentage == itax.Key);
                    }
                    else
                    {
                        taxlist.Add(new PendingLrInvoiceDetail.TaxMatch()
                        {
                            percentage = itax.Key,
                            invqty = itax.Sum(x => x.buyingqty),
                            invvalue = itax.Sum(x => x.buyingprice * x.buyingqty),
                            invtax = itax.Sum(x => x.purchasetax)
                        });
                    }
                }
                response.taxlist = taxlist;
                response.alertlist = new List<PendingLrInvoiceDetail.AlertItem>();
                var missingpo = response.invoice.Lrinvoiceitems.FindAll(x => x.poinfo == null || x.poinfo.Count == 0);
                if (missingpo.Count > 0)
                {
                    response.alertlist.Add(new PendingLrInvoiceDetail.AlertItem()
                    {
                        reason = "Missing PO",
                        description = String.Join(",", missingpo.ConvertAll(x => x.productname)),
                        qty = missingpo.Sum(x => x.buyingqty),
                        value = missingpo.Sum(x => x.buyingprice * x.buyingqty)
                    });
                }

                var poitemlist = new List<POItemInfo>();

                response.invoice.Lrinvoiceitems.FindAll(x => x.poinfo != null && x.poinfo.Count > 0).ConvertAll(x => x.poinfo).ForEach(x =>
                poitemlist.AddRange(x));

                poitemlist = poitemlist.GroupBy(x => x.purchaseorderitemsid).ToList().ConvertAll(x => new POItemInfo()
                {
                    purchaseorderitemsid = x.Key,
                    pono = x.First().pono,
                    price = x.First().price,
                    aqty = x.Sum(y => y.aqty)
                });

                var diffpoitems = poitemlist.FindAll(x =>
                {
                    return response.purchaseorder.Purchaseorderitems.Exists(y => y.id == x.purchaseorderitemsid);
                });

                if (diffpoitems.Count > 0)
                {
                    response.alertlist.Add(new PendingLrInvoiceDetail.AlertItem()
                    {
                        reason = "From Other PO",
                        description = String.Join(",", diffpoitems.ConvertAll(x => x.pono)),
                        qty = diffpoitems.Sum(x => x.aqty),
                        value = diffpoitems.Sum(x => x.aqty * x.price)
                    });
                }

                var missingpoitems = response.purchaseorder.Purchaseorderitems.FindAll(x =>
                {
                    return (!poitemlist.Exists(y => y.purchaseorderitemsid == x.id));
                });


                if (missingpoitems.Count > 0)
                {
                    response.alertlist.Add(new PendingLrInvoiceDetail.AlertItem()
                    {
                        reason = "Left PO",
                        description = String.Join(",", missingpoitems.ConvertAll(x => x.productname)),
                        qty = missingpoitems.Sum(x => x.oqty),
                        value = missingpoitems.Sum(x => x.oqty * x.price)
                    });
                }

                var diffinqty = response.purchaseorder.Purchaseorderitems.FindAll(x =>
                {
                    if (poitemlist.Exists(y => y.purchaseorderitemsid == x.id))
                    {
                        return x.oqty != poitemlist.Find(y => y.purchaseorderitemsid == x.id).aqty;
                    }
                    return false;
                });

                if (diffinqty.Count > 0)
                {
                    response.alertlist.Add(new PendingLrInvoiceDetail.AlertItem()
                    {
                        reason = "Diff Qty",
                        description = String.Join(",", diffinqty.ConvertAll(x => x.productname)),
                        qty = diffinqty.Sum(x => x.oqty),
                        value = diffinqty.Sum(x => x.oqty * x.price)
                    });
                }


                var diffinprice = response.invoice.Lrinvoiceitems.FindAll(x => x.poinfo != null && x.poinfo.Count > 0).FindAll(y =>
                {
                    return y.buyingprice != y.poinfo.Average(x => x.price);
                });
                if (diffinprice.Count > 0)
                {
                    response.alertlist.Add(new PendingLrInvoiceDetail.AlertItem()
                    {
                        reason = "Diff Qty",
                        description = String.Join(",", diffinprice.ConvertAll(x => x.productname)),
                        qty = diffinprice.Sum(x => x.buyingqty),
                        value = diffinprice.Sum(x => x.buyingqty * x.buyingprice)
                    });
                }
            }
            return response;
        }

        public List<PendingLrInvoice> SearchPendingPayment(SupplierpaymentCreteria _supplierpayment)
        {
            List<RangeValue> rangeValues = new List<RangeValue>();
            if (!string.IsNullOrEmpty(_supplierpayment.ageranges))
            {
                try
                {
                    var ranges = _supplierpayment.ageranges.Split('-');
                    if (ranges.Length == 0)
                    {
                        ranges = _supplierpayment.ageranges.Split(',');
                        int start = int.Parse(ranges[0]);
                        var current = start;
                        if (start > 0)
                        {
                            rangeValues.Add(new RangeValue()
                            {
                                from = 0,
                                to = start,
                                value = start
                            });
                        }
                        for (int i = 1; i < ranges.Length; i++)
                        {
                            start = int.Parse(ranges[i]);
                            rangeValues.Add(new RangeValue()
                            {
                                from = current,
                                to = start,
                                value = start
                            });
                            current = start;
                        }
                    }
                    else
                    {
                        int start = int.Parse(ranges[0]);
                        int increment = int.Parse(ranges[1]);
                        int end = int.Parse(ranges[2]);
                        var current = start;
                        rangeValues.Add(new RangeValue()
                        {
                            from = 0,
                            to = start,
                            value = start
                        });

                        while (current < end)
                        {
                            rangeValues.Add(new RangeValue()
                            {
                                from = current,
                                to = current + increment,
                                value = current + increment
                            });
                            current += increment;
                        }
                    }
                }
                catch (Exception exp)
                {
                    WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _supplierpayment);
                }
            }
            List<PendingLrInvoice> lst = new List<PendingLrInvoice>();
            using (NpgsqlConnection conn = CreateConnection())
            {
                try
                {

                    if (_supplierpayment.supplierid > 0)
                    {
                        SuppliercompanyDataService supplierDataService = new SuppliercompanyDataService() { Context = this.Context };
                        _supplierpayment.suppliercompanyid = supplierDataService.Search(new SuppliercompanyCreteria()
                        {
                            supplierid = _supplierpayment.supplierid
                        }).First().id;
                    }
                    conn.Open();

                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_pendingbills;
                    if (_supplierpayment.suppliercompanyid > 0)
                        qb.AddParameter("lrinvoice.suppliercompanyid", "suppliercompanyid", _supplierpayment.suppliercompanyid, "=");
                    if (_supplierpayment.suppliercompanyids != null)
                        qb.AddInParamer("lrinvoice.suppliercompanyid", string.Join(",", _supplierpayment.suppliercompanyids));
                    if (_supplierpayment.companyid > 0)
                        qb.AddParameter("lrinvoice.companyid", "companyid", _supplierpayment.companyid, "=");
                    if (_supplierpayment.agentid > 0)
                        qb.AddParameter("lrentry.agentid", "agentid", _supplierpayment.agentid, "=");
                    if (_supplierpayment.locationid > 0)
                        qb.AddORParameter("lrentry.entrylocationid", _supplierpayment.locationid, "=",
                            "lrentry.retaillocationid", "retaillocationid", _supplierpayment.locationid, "=");
                    if (_supplierpayment.cityid > 0)
                        qb.AddParameter("suppliercompany.cityid", "cityid", _supplierpayment.cityid, "=");
                    if (_supplierpayment.cityid < 0)
                    {
                        if (_supplierpayment.companyid == 0)
                            throw new Exception("Please the company");
                        CompanyDataService companyDataService = new CompanyDataService() { Context = this.Context };
                        var companystate = companyDataService.Search(new CompanyCreteria()
                        {
                            id = _supplierpayment.companyid
                        }).First().stateid;
                        if (companystate <= 0)
                            throw new Exception("Please select the state in company maaster");
                        qb.AddParameter("suppliercompany.stateid", "stateid", companystate,
                           _supplierpayment.cityid == -1 ? "=" : "!=");
                    }

                    if (_supplierpayment.supplierid > 0)
                        qb.AddParameter("lrentry.supplierid", "supplierid", _supplierpayment.supplierid, "=");

                    if (_supplierpayment.duefromdate > DateTime.MinValue)
                        qb.AddParameter("lrentry.duedate", "duedatefrom", _supplierpayment.duefromdate.Date, ">=");
                    if (_supplierpayment.duetodate > DateTime.MinValue)
                        qb.AddParameter("lrentry.duedate", "duedateto", _supplierpayment.duetodate.Date.AddDays(1), "<");

                    if (_supplierpayment.entryfromdate > DateTime.MinValue)
                        qb.AddParameter("lrinvoice.entrydate", "entrydatefrom", _supplierpayment.entryfromdate.Date, ">=");
                    if (_supplierpayment.entrytodate > DateTime.MinValue)
                        qb.AddParameter("lrinvoice.entrydate", "entrydateto", _supplierpayment.entrytodate.Date.AddDays(1), "<");

                    if (_supplierpayment.invfromdate > DateTime.MinValue)
                        qb.AddParameter("lrinvoice.invoicedate", "invoicedatefrom", _supplierpayment.invfromdate.Date, ">=");
                    if (_supplierpayment.invtodate > DateTime.MinValue)
                        qb.AddParameter("lrinvoice.invoicedate", "invoicedateto", _supplierpayment.invtodate.Date.AddDays(1), "<");

                    if (Services.ConfigManager.Instance(Context.OrganizationID).Config.intray_pur_prt)
                        qb.Add2Query("lrinvoice.hasapprove=true");

                    if (_supplierpayment.showmsme)
                        qb.Add2Query("suppliercompany.msmeno is not null and suppliercompany.msmeno != ''");

                    qb.Add2Query("coalesce(paidinvoice.paidamount,0)<coalesce(lrinvoice.totalamount,0)");

                    //if(_supplierpayment.showplanned)
                    //    qb.Add2Query("coalesce(plannedpayment,0) != 0");

                    qb.SortField = "lrinvoice.invoicedate desc";
                    NpgsqlCommand command = qb.GetCommand(conn);
                    using (System.Data.IDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            PendingLrInvoice oinvoice = new PendingLrInvoice();

                            oinvoice.supplierid = reader["supplierid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["supplierid"]);
                            oinvoice.suppliercompanyid = reader["suppliercompanyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["suppliercompanyid"]);
                            oinvoice.lrinvoiceid = reader["lrinvoiceid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["lrinvoiceid"]);
                            oinvoice.companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"]);
                            oinvoice.invoiceno = reader["invoiceno"] == DBNull.Value ? "" : reader["invoiceno"].ToString();
                            oinvoice.invoicedate = reader["invoicedate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["invoicedate"]);
                            oinvoice.duedate = reader["duedate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["duedate"]);

                            oinvoice.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);

                            oinvoice.pono = reader["pono"] == DBNull.Value ? "" : reader["pono"].ToString();
                            oinvoice.podate = reader["podate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["podate"]);

                            oinvoice.supplier = reader["suppliername"] == DBNull.Value ? "" : reader["suppliername"].ToString();
                            oinvoice.city = reader["cityname"] == DBNull.Value ? "" : reader["cityname"].ToString();

                            oinvoice.lrentryno = reader["lrentryno"] == DBNull.Value ? "" : reader["lrentryno"].ToString();
                            oinvoice.slipno = reader["slipno"] == DBNull.Value ? "" : reader["slipno"].ToString();
                            oinvoice.entrydate = reader["entrydate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["entrydate"]);
                            oinvoice.grossamount = reader["baseamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["baseamount"]);
                            oinvoice.totalamount = reader["totalamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["totalamount"]);
                            oinvoice.totalamount -= reader["presettled"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["presettled"]);
                            oinvoice.invoicediscount = reader["invoicediscount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["invoicediscount"]);
                            oinvoice.taxamount = (reader["taxamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["taxamount"]));
                            oinvoice.paidamount = reader["paidamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["paidamount"]);
                            oinvoice.plannedpayment = reader["plannedpayment"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["plannedpayment"]);
                            oinvoice.cityid = reader["cityid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["cityid"]);
                            oinvoice.locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);

                            oinvoice.tdsledgerid = reader["tdsledgerid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["tdsledgerid"]);
                            oinvoice.tdsamount = (reader["tdsamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["tdsamount"]));
                            oinvoice.balanceamount = oinvoice.totalamount - (oinvoice.paidamount + oinvoice.tdsamount);

                            //if (reader["hascommission"] != DBNull.Value && (bool)reader["hascommission"])
                            {
                                oinvoice.agentid = reader["agentid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["agentid"]);
                                oinvoice.agentcommission = (reader["agentcommission"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["agentcommission"]));
                            }
                            oinvoice.intreceive = 0;
                            oinvoice.intpay = 0;
                            decimal intrateagrmnt = (reader["intrateagrmnt"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["intrateagrmnt"]));
                            decimal intrateagrmntdays = (reader["intrateagrmntdays"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["intrateagrmntdays"]));
                            if (intrateagrmnt > 0 && intrateagrmntdays > 0)
                            {
                                var duedays = Convert.ToInt32(DateTime.Now.Subtract(oinvoice.entrydate).TotalDays);
                                if (intrateagrmntdays > duedays)
                                {
                                    oinvoice.intreceive = Math.Round(((oinvoice.balanceamount * intrateagrmnt / 100) / 365) * (intrateagrmntdays - duedays), 0);
                                }
                                else
                                {
                                    oinvoice.intpay = Math.Round(((oinvoice.balanceamount * intrateagrmnt / 100) / 365) * (duedays - intrateagrmntdays), 0);
                                }
                            }
                            oinvoice.sectionid = reader["sectionid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sectionid"]);
                            if (_supplierpayment.sectionid > 0 && oinvoice.sectionid > 0 && oinvoice.sectionid != _supplierpayment.sectionid) continue;
                            oinvoice.invoicetype = "Invoice";
                            oinvoice.isinvoice = true;
                            var days = (DateTimeNow.Date - oinvoice.invoicedate.Date).TotalDays;
                            if (rangeValues.Count > 0)
                            {
                                oinvoice.days = rangeValues.Exists(e => e.from <= days && e.to >= days) ?
                                    rangeValues.Find(e => e.from <= days && e.to >= days).display : rangeValues.Last().to.ToString("N0") + " >";
                            }
                            else
                            {
                                oinvoice.days = days.ToString("N0");
                                Int32 nday = 0;
                                Int32.TryParse(oinvoice.days, out nday);
                                oinvoice.ndays = nday;
                            }
                            if (oinvoice.balanceamount-oinvoice.agentcomvalue > 1)
                                lst.Add(oinvoice);
                        }
                    }

                    if (lst.Count > 0 && _supplierpayment.sectionid > 0 && lst.All(e => e.sectionid == 0))
                    {
                        List<long> filteredinvoices = new List<long>();
                        command.CommandText = string.Format(SQL_pendingsections, _supplierpayment.sectionid, String.Join<long>(",", lst.ConvertAll<long>(e => e.lrinvoiceid)));
                        using (System.Data.IDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                filteredinvoices.Add(reader["lrinvoiceid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["lrinvoiceid"]));
                            }
                        }
                        lst.RemoveAll(e => !filteredinvoices.Contains(e.lrinvoiceid));
                    }

                    if (lst.Count > 0 && _supplierpayment.suppliercompanyid > 0)
                    {
                        lst.ForEach(x => x.warn = "No Barcode Generated");

                        foreach (var chunk in lst.ConvertAll<long>(e => e.lrinvoiceid).ToList().ChunkBy(500))
                        {
                            command.CommandText = string.Format(SQL_SoldValues, String.Join<long>(",", chunk));
                            using (System.Data.IDataReader reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    long lrinvoiceid = reader["lrinvoiceid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["lrinvoiceid"]);
                                    decimal soldamount = reader["soldamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["soldamount"]);
                                    decimal billedqty = reader["billedqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["billedqty"]);
                                    decimal purchaseqty = reader["purchaseqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["purchaseqty"]);

                                    decimal sellableqty = reader["sellableqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["sellableqty"]);
                                    decimal buyingqty = reader["buyingqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["buyingqty"]);
                                    decimal returnqty = reader["returnqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["returnqty"]);

                                    var item = lst.Find(e => e.lrinvoiceid == lrinvoiceid);
                                    item.soldamount = soldamount;
                                    if (buyingqty == 0 && returnqty > 0)
                                    {
                                        item.warn = "All Goods Returned";
                                    }
                                    else if (sellableqty == 0)
                                    {
                                        item.warn = "Barcode not generated";
                                    }
                                    if (sellableqty < buyingqty - returnqty)
                                    {
                                        item.warn = "Partial barcode";
                                    }
                                    else
                                    {
                                        item.warn = "";
                                    }
                                    //item.pendingpercent = Math.Round((soldamount / item.totalamount * 100), 0);
                                    if (purchaseqty > 0 && billedqty > 0)
                                        item.soldpercent = Math.Round((billedqty / purchaseqty * 100), 0);
                                    else if (purchaseqty > 0 && billedqty == 0)
                                        item.soldpercent = 0;
                                    else
                                        item.intransit = true;
                                }
                            }
                        }
                    }

                    qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_pendingdebitnotes;
                    if (_supplierpayment.suppliercompanyid > 0)
                        qb.AddParameter("lrinvoicereturn.suppliercompanyid", "suppliercompanyid", _supplierpayment.suppliercompanyid, "=");
                    if (_supplierpayment.suppliercompanyids != null && _supplierpayment.suppliercompanyids.Count > 0)
                        qb.AddInParamer("lrinvoicereturn.suppliercompanyid", string.Join(",", _supplierpayment.suppliercompanyids));
                    if (_supplierpayment.companyid > 0)
                        qb.AddParameter("lrinvoicereturn.companyid", "companyid", _supplierpayment.companyid, "=");
                    if (_supplierpayment.locationid > 0)
                        qb.AddParameter("lrinvoicereturn.locationid", "locationid", _supplierpayment.locationid, "=");
                    if (_supplierpayment.agentid > 0)
                        qb.AddParameter("lrinvoicereturn.agentid", "agentid", _supplierpayment.agentid, "=");
                    if (_supplierpayment.cityid > 0)
                        qb.AddParameter("suppliercompany.cityid", "cityid", _supplierpayment.cityid, "=");
                    if (_supplierpayment.entryfromdate > DateTime.MinValue)
                        qb.AddParameter("lrinvoicereturn.returndate", "entrydatefrom", _supplierpayment.entryfromdate.Date, ">=");
                    if (_supplierpayment.entrytodate > DateTime.MinValue)
                        qb.AddParameter("lrinvoicereturn.returndate", "entrydateto", _supplierpayment.entrytodate.Date.AddDays(1), "<");
                    if (Services.ConfigManager.Instance(Context.OrganizationID).Config.intray_pur_prt)
                        qb.Add2Query("lrinvoicereturn.hasapprove=true");
                    if (_supplierpayment.showmsme)
                        qb.Add2Query("suppliercompany.msmeno is not null and suppliercompany.msmeno != ''");
                    if (_supplierpayment.duedebitnotes)
                    {
                        if (_supplierpayment.entryfromdate > DateTime.MinValue)
                            qb.AddParameter("lrinvoicereturn.returndate", "duefromdate", _supplierpayment.duefromdate.Date, ">=");
                        if (_supplierpayment.entrytodate > DateTime.MinValue)
                            qb.AddParameter("lrinvoicereturn.returndate", "duetodate", _supplierpayment.duetodate.Date.AddDays(1), "<");
                    }

                    command = qb.GetCommand(conn);
                    //if (_supplierpayment.duedebitnotes)
                    //{

                    //    command.CommandText = string.Format(@"
                    //    with debitnotelist as
                    //    (
                    //      {0}
                    //    ),duelist as
                    //    (
                    //        select distinct lrinvoicereturnitems.lrinvoicereturnid from debitnotelist
                    //        inner join lrinvoicereturn on lrinvoicereturn.id = debitnotelist.lrinvoiceid
                    //        inner join lrinvoicereturnitems on lrinvoicereturnitems.lrinvoicereturnid = lrinvoicereturn.id
                    //        left outer join lrinvoiceitems on lrinvoiceitems.id = lrinvoiceitemsid
                    //        left outer join lrinvoice on lrinvoice.id = lrinvoiceitems.lrinvoiceid
                    //        left outer join lrentry on lrentry.id = lrid
                    //        where lrentry.id is null or lrentry.duedate<@debitduedateto
                    //    )
                    //    select debitnotelist.* from debitnotelist
                    //    inner join duelist on duelist.lrinvoicereturnid = debitnotelist.lrinvoiceid", command.CommandText);
                    //    command.Parameters.Add("debitduedatefrom", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = _supplierpayment.duefromdate.Date;
                    //    command.Parameters.Add("debitduedateto", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = _supplierpayment.duetodate.AddDays(1).Date;
                    //}

                    var lrinvoiceids = new List<long>();
                    using (System.Data.IDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            PendingLrInvoice oinvoice = new PendingLrInvoice();
                            oinvoice.supplierid = reader["supplierid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["supplierid"]);
                            oinvoice.suppliercompanyid = reader["suppliercompanyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["suppliercompanyid"]);
                            oinvoice.lrinvoiceid = reader["lrinvoiceid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["lrinvoiceid"]);
                            oinvoice.invoiceno = reader["invoiceno"] == DBNull.Value ? "" : reader["invoiceno"].ToString();
                            oinvoice.invoicedate = reader["invoicedate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["invoicedate"]);
                            oinvoice.entrydate = reader["invoicedate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["invoicedate"]);
                            oinvoice.duedate = reader["invoicedate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["invoicedate"]);
                            oinvoice.totalamount = (reader["totalamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["totalamount"]));
                            //oinvoice.taxamount = (reader["taxamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["taxamount"]));
                            //oinvoice.agentcommission = (reader["agentcommission"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["agentcommission"]));
                            oinvoice.plannedpayment = (reader["plannedpayment"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["plannedpayment"]));
                            oinvoice.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                            var returntype = reader["returntype"] == DBNull.Value ? 0 : Convert.ToInt64(reader["returntype"]);
                            if (returntype == 11 || returntype == 12)
                            {
                                oinvoice.debetnoteamount = (reader["totalamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["totalamount"]));
                                oinvoice.paidamount = (reader["paidamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["paidamount"]));
                                oinvoice.iscreditnote = true;
                                oinvoice.balanceamount = Math.Abs(oinvoice.totalamount) - Math.Abs(oinvoice.paidamount);
                                if (returntype == 11)
                                    oinvoice.invoicetype = "Excess Credit Note";
                                else if (returntype == 12)
                                    oinvoice.invoicetype = "Price Credit Note";
                                else
                                    oinvoice.invoicetype = "Credit Note";
                            }
                            else
                            {
                                oinvoice.debetnoteamount = (reader["totalamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["totalamount"]));
                                oinvoice.paidamount = (reader["paidamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["paidamount"])) * -1;
                                oinvoice.isdebitnote = true;
                                oinvoice.balanceamount = Math.Abs(oinvoice.totalamount) - Math.Abs(oinvoice.paidamount);
                                oinvoice.balanceamount *= -1;

                                if (returntype == 5)
                                    oinvoice.invoicetype = "Discount Note";
                                else if (returntype == 6)
                                    oinvoice.invoicetype = "Supplier Credit Note";
                                else
                                    oinvoice.invoicetype = "Debit Note";
                            }

                            oinvoice.lrdate = reader["lrdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["lrdate"]); ;
                            oinvoice.lrno = reader["lrno"] == DBNull.Value ? "" : reader["lrno"].ToString(); ;
                            oinvoice.transport = reader["transport"] == DBNull.Value ? "" : reader["transport"].ToString();
                            oinvoice.remarks = reader["remarks"] == DBNull.Value ? "" : reader["remarks"].ToString();
                            oinvoice.cityid = reader["cityid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["cityid"]);
                            oinvoice.companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"]);
                            oinvoice.locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                            oinvoice.agentid = reader["agentid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["agentid"]);
                            oinvoice.sectionid = reader["sectionid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sectionid"]);
                            oinvoice.supplier = reader["suppliername"] == DBNull.Value ? "" : reader["suppliername"].ToString();
                            oinvoice.city = reader["cityname"] == DBNull.Value ? "" : reader["cityname"].ToString();
                            var days = (DateTimeNow.Date - oinvoice.invoicedate.Date).TotalDays;
                            if (rangeValues.Count > 0)
                            {
                                oinvoice.days = rangeValues.Exists(e => e.from <= days && e.to >= days) ? rangeValues.Find(e => e.from <= days && e.to >= days).display : rangeValues.Last().to.ToString("N0") + " >";
                            }
                            else
                            {
                                oinvoice.days = days.ToString("N0");
                                Int32 nday = 0;
                                Int32.TryParse(oinvoice.days, out nday);
                                oinvoice.ndays = nday;
                            }
                            if (oinvoice.totalamount - oinvoice.paidamount > 0)
                            {
                                lrinvoiceids.Add(oinvoice.lrinvoiceid);
                                lst.Add(oinvoice);
                            }
                        }
                    }


                    if (lrinvoiceids.Count > 0)
                    {
                        foreach (var chunks in lrinvoiceids.ChunkBy(500))
                        {
                            qb.CurrentQuery = @"select distinct lrinvoice.id,lrinvoice.invoiceno,lrinvoice.invoicedate,lrinvoicereturnitems.lrinvoicereturnid from lrinvoicereturnitems
                            inner join lrinvoiceitems on lrinvoiceitems.id = lrinvoicereturnitems.lrinvoiceitemsid
                            inner join lrinvoice on lrinvoice.id = lrinvoiceitems.lrinvoiceid
                            where lrinvoicereturnitems.lrinvoicereturnid in (" + string.Join(",", chunks) + ")"; ;
                            command = qb.GetCommand(conn);
                            using (System.Data.IDataReader reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    var lrinvoiceid = reader["lrinvoicereturnid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["lrinvoicereturnid"]);
                                    var id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                                    PendingLrInvoice oinvoice = lst.Find(e => e.isdebitnote && e.lrinvoiceid == lrinvoiceid);
                                    if (oinvoice == null)
                                    {
                                        oinvoice = lst.Find(e => e.iscreditnote && e.lrinvoiceid == lrinvoiceid);
                                    }
                                    if (!string.IsNullOrEmpty(oinvoice.coreinvoiceno))
                                        oinvoice.coreinvoiceno += ",";
                                    oinvoice.coreinvoiceno += reader["invoiceno"] == DBNull.Value ? "" : reader["invoiceno"].ToString();
                                    oinvoice.coreinvoicedate = reader["invoicedate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["invoicedate"]);
                                    if (oinvoice.coreinvoiceids == null)
                                        oinvoice.coreinvoiceids = new List<long>();
                                    oinvoice.coreinvoiceids.Add(id);

                                }
                            }
                        }
                    }

                    var alldebitnotes = lst.FindAll(x => x.isdebitnote);
                    var allinvoices = lst.FindAll(x => !x.isdebitnote);
                    List<PendingLrInvoice> orderedlst = new List<PendingLrInvoice>();
                    foreach (var invoices in allinvoices.GroupBy(x => new { x.supplier, x.invoicedate, x.lrinvoiceid }))
                    {
                        var ilist = invoices.ToList();
                        var ids = ilist.ConvertAll(x => x.lrinvoiceid);
                        var debitnotes = alldebitnotes.FindAll(x => x.coreinvoiceids != null && x.coreinvoiceids.Exists(y => ids.Contains(y)));
                        orderedlst.AddRange(invoices.ToList());
                        foreach (var dn in debitnotes)
                        {
                            alldebitnotes.Remove(dn);
                            orderedlst.Add(dn);
                        }
                    }
                    orderedlst.AddRange(alldebitnotes);
                    lst = orderedlst;
                    conn.Close();
                }
                catch (Exception exp)
                {
                    if (conn != null) conn.Close();
                    WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _supplierpayment);
                }
            }
            return lst;
        }
        public List<PendingLrInvoice> SearchSupplierPayment(SupplierpaymentCreteria _supplierpayment)
        {
            List<PendingLrInvoice> lst = new List<PendingLrInvoice>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                NpgsqlCommand command = null;

                StringBuilder SQL_Where = new StringBuilder();
                StringBuilder SQL_Where1 = new StringBuilder();
                QueryBuilder qb = new QueryBuilder();

                if (_supplierpayment.fromdate != DateTime.MinValue && _supplierpayment.todate != DateTime.MinValue)
                {
                    SQL_Where.AppendLine("and lrentry.lrentrydate >= '" + _supplierpayment.fromdate.Date.ToString("yyyy-MM-dd")
                        + "' and lrentry.lrentrydate < '" + _supplierpayment.todate.Date.AddDays(1).ToString("yyyy-MM-dd") + "'");
                }

                if (_supplierpayment.entryfromdate != DateTime.MinValue && _supplierpayment.entrytodate != DateTime.MinValue)
                {
                    SQL_Where.AppendLine("and lrinvoice.entrydate >= '" + _supplierpayment.entryfromdate.Date.ToString("yyyy-MM-dd")
                        + "' and lrinvoice.entrydate < '" + _supplierpayment.entrytodate.Date.AddDays(1).ToString("yyyy-MM-dd") + "'");
                    SQL_Where1.AppendLine("and lrinvoicereturn.returndate >= '" + _supplierpayment.entryfromdate.Date.ToString("yyyy-MM-dd")
                        + "' and lrinvoicereturn.returndate < '" + _supplierpayment.entrytodate.Date.AddDays(1).ToString("yyyy-MM-dd") + "'");
                }

                if (_supplierpayment.invfromdate != DateTime.MinValue && _supplierpayment.invtodate != DateTime.MinValue)
                {
                    SQL_Where.AppendLine("and lrinvoice.invoicedate >= '" + _supplierpayment.invfromdate.Date.ToString("yyyy-MM-dd")
                        + "' and lrinvoice.invoicedate < '" + _supplierpayment.invtodate.Date.AddDays(1).ToString("yyyy-MM-dd") + "'");
                }

                if (_supplierpayment.duefromdate != DateTime.MinValue && _supplierpayment.duetodate != DateTime.MinValue)
                {
                    SQL_Where.AppendLine("and lrentry.duedate >= '" + _supplierpayment.duefromdate.Date.ToString("yyyy-MM-dd")
                        + "' and lrentry.duedate < '" + _supplierpayment.duetodate.Date.AddDays(1).ToString("yyyy-MM-dd") + "'");
                }

                if (_supplierpayment.companyid > 0)
                {
                    SQL_Where.AppendLine("and lrinvoice.companyid=" + _supplierpayment.companyid);
                    SQL_Where1.AppendLine("and lrinvoicereturn.companyid=" + _supplierpayment.companyid);
                }
                if (_supplierpayment.suppliercompanyid > 0)
                {
                    SQL_Where.AppendLine("and lrinvoice.suppliercompanyid=" + _supplierpayment.suppliercompanyid);
                    SQL_Where1.AppendLine("and lrinvoicereturn.suppliercompanyid=" + _supplierpayment.suppliercompanyid);
                }

                if (_supplierpayment.locationid > 0)
                {
                    SQL_Where.AppendLine("and lrentry.entrylocationid=" + _supplierpayment.locationid);
                    SQL_Where1.AppendLine("and lrinvoicereturn.locationid=" + _supplierpayment.locationid);
                }

                SQL_supplierbills = string.Format(SQL_supplierbills, SQL_Where.ToString());
                qb.CurrentQuery = SQL_supplierbills;
                qb.SortField = "lrinvoice.invoicedate";
                command = qb.GetCommand(conn);

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        PendingLrInvoice oinvoice = new PendingLrInvoice();
                        oinvoice.supplier = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
                        oinvoice.agentid = reader["agentid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["agentid"]);
                        oinvoice.cityid = reader["cityid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["cityid"]);
                        oinvoice.duedate = reader["duedate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["duedate"]);
                        oinvoice.lrinvoiceid = reader["lrinvoiceid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["lrinvoiceid"]);
                        oinvoice.invoiceno = reader["invoiceno"] == DBNull.Value ? "" : reader["invoiceno"].ToString();
                        oinvoice.invoicedate = reader["invoicedate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["invoicedate"]);
                        oinvoice.invoiceentrydate = reader["entrydate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["entrydate"]);
                        oinvoice.totalamount = reader["totalamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["totalamount"]);
                        oinvoice.totalamount -= reader["presettled"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["presettled"]);
                        oinvoice.paidamount = reader["paidamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["paidamount"]);
                        oinvoice.balanceamount = oinvoice.totalamount - oinvoice.paidamount;

                        oinvoice.msmeno = reader["msmeno"] == DBNull.Value ? "" : reader["msmeno"].ToString();
                        oinvoice.lrentryno = reader["lrentryno"] == DBNull.Value ? "" : reader["lrentryno"].ToString();
                        oinvoice.agentname = reader["agentname"] == DBNull.Value ? "" : reader["agentname"].ToString();
                        oinvoice.bankname = reader["bankname"] == DBNull.Value ? "" : reader["bankname"].ToString();
                        oinvoice.bankbranch = reader["bankbranch"] == DBNull.Value ? "" : reader["bankbranch"].ToString();
                        oinvoice.bankifsccode = reader["bankifci"] == DBNull.Value ? "" : reader["bankifci"].ToString();
                        oinvoice.bankacno = reader["bankaccountno"] == DBNull.Value ? "" : reader["bankaccountno"].ToString();
                        oinvoice.bankacname = reader["bankaccountname"] == DBNull.Value ? "" : reader["bankaccountname"].ToString();
                        oinvoice.gstno = reader["tinanddate"] == DBNull.Value ? "" : reader["tinanddate"].ToString();

                        if (oinvoice.invoicedate > DateTime.MinValue)
                        {
                            System.TimeSpan diffResult = DateTimeNow.Subtract(oinvoice.invoicedate);
                            oinvoice.duedays = diffResult.Days;
                        }
                        else if (oinvoice.invoiceentrydate > DateTime.MinValue)
                        {
                            System.TimeSpan diffResult = DateTimeNow.Subtract(oinvoice.invoiceentrydate);
                            oinvoice.duedays = diffResult.Days;
                        }
                        oinvoice.isinvoice = true;
                        oinvoice.invoicetype = "Invoice";
                        if (oinvoice.totalamount - oinvoice.paidamount > 0)
                            lst.Add(oinvoice);
                    }
                }

                if (lst.Count > 0)
                {
                    command.CommandText = string.Format(SQL_SoldValues, String.Join<long>(",", lst.ConvertAll<long>(e => e.lrinvoiceid)));
                    using (System.Data.IDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            long lrinvoiceid = reader["lrinvoiceid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["lrinvoiceid"]);
                            decimal soldamount = reader["soldamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["soldamount"]);
                            decimal billedqty = reader["billedqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["billedqty"]);
                            decimal purchaseqty = reader["purchaseqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["purchaseqty"]);
                            var item = lst.Find(e => e.lrinvoiceid == lrinvoiceid);
                            item.soldamount = soldamount;
                            //item.pendingpercent = Math.Round((soldamount / item.totalamount * 100), 0);
                            if (purchaseqty > 0 && billedqty > 0)
                                item.soldpercent = Math.Round((billedqty / purchaseqty * 100), 0);
                            else if (purchaseqty > 0 && billedqty == 0)
                                item.soldpercent = 0;
                        }
                    }
                }

                if (_supplierpayment.duefromdate.Date == DateTime.MinValue || _supplierpayment.duetodate.Date == DateTime.MinValue)
                {
                    SQL_supplierbebitnote = string.Format(SQL_supplierbebitnote, SQL_Where1.ToString());
                    qb.CurrentQuery = SQL_supplierbebitnote;
                    qb.SortField = "lrinvoicereturn.returndate";
                    command = qb.GetCommand(conn);
                    List<long> lrinvoiceids = new List<long>();
                    using (System.Data.IDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            PendingLrInvoice oinvoice = new PendingLrInvoice();
                            oinvoice.supplier = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
                            oinvoice.agentid = reader["agentid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["agentid"]);
                            oinvoice.cityid = reader["cityid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["cityid"]);
                            oinvoice.lrinvoiceid = reader["lrinvoiceid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["lrinvoiceid"]);
                            oinvoice.invoiceno = reader["invoiceno"] == DBNull.Value ? "" : reader["invoiceno"].ToString();
                            oinvoice.invoicedate = reader["invoicedate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["invoicedate"]);
                            oinvoice.totalamount = (reader["totalamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["totalamount"]));
                            oinvoice.paidamount = (reader["paidamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["paidamount"])) * -1;
                            oinvoice.agentname = reader["agentname"] == DBNull.Value ? "" : reader["agentname"].ToString();
                            oinvoice.isdebitnote = true;
                            oinvoice.balanceamount = oinvoice.totalamount - oinvoice.paidamount;
                            oinvoice.invoicetype = "Debit Note";
                            if (oinvoice.invoicedate > DateTime.MinValue)
                            {
                                System.TimeSpan diffResult = DateTimeNow.Subtract(oinvoice.invoicedate);
                                oinvoice.duedays = diffResult.Days;
                            }

                            if (oinvoice.totalamount - oinvoice.paidamount > 0)
                            {
                                oinvoice.totalamount = oinvoice.totalamount * -1;
                                oinvoice.paidamount = oinvoice.paidamount * -1;
                                oinvoice.balanceamount = oinvoice.totalamount - oinvoice.paidamount;
                                lrinvoiceids.Add(oinvoice.lrinvoiceid);
                                lst.Add(oinvoice);
                            }

                            oinvoice.msmeno = reader["msmeno"] == DBNull.Value ? "" : reader["msmeno"].ToString();
                            oinvoice.bankname = reader["bankname"] == DBNull.Value ? "" : reader["bankname"].ToString();
                            oinvoice.bankbranch = reader["bankbranch"] == DBNull.Value ? "" : reader["bankbranch"].ToString();
                            oinvoice.bankifsccode = reader["bankifci"] == DBNull.Value ? "" : reader["bankifci"].ToString();
                            oinvoice.bankacno = reader["bankaccountno"] == DBNull.Value ? "" : reader["bankaccountno"].ToString();
                            oinvoice.bankacname = reader["bankaccountname"] == DBNull.Value ? "" : reader["bankaccountname"].ToString();
                            oinvoice.gstno = reader["tinanddate"] == DBNull.Value ? "" : reader["tinanddate"].ToString();
                        }
                    }
                    if (lrinvoiceids.Count > 0)
                    {
                        qb.CurrentQuery = @"select lrinvoice.invoiceno,lrinvoice.invoicedate,lrinvoicereturnitems.lrinvoicereturnid from lrinvoicereturnitems
                        inner join lrinvoiceitems on lrinvoiceitems.id = lrinvoicereturnitems.lrinvoiceitemsid
                        inner join lrinvoice on lrinvoice.id = lrinvoiceitems.lrinvoiceid
                        where lrinvoicereturnitems.lrinvoicereturnid in (" + string.Join(",", lrinvoiceids) + ")";
                        qb.SortField = "lrinvoice.invoicedate";

                        command = qb.GetCommand(conn);
                        using (System.Data.IDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var lrinvoiceid = reader["lrinvoicereturnid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["lrinvoicereturnid"]);
                                PendingLrInvoice oinvoice = lst.Find(e => e.isdebitnote && e.lrinvoiceid == lrinvoiceid);
                                if (!string.IsNullOrEmpty(oinvoice.coreinvoiceno))
                                    oinvoice.coreinvoiceno = ",";
                                oinvoice.coreinvoiceno += reader["invoiceno"] == DBNull.Value ? "" : reader["invoiceno"].ToString();
                                oinvoice.coreinvoicedate = reader["invoicedate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["invoicedate"]);
                            }
                        }
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _supplierpayment);
            }
            return lst;

        }
        public List<PendingLrInvoice> SearchSupplierPaymentSummary(SupplierpaymentCreteria _supplierpayment)
        {
            List<PendingLrInvoice> lst = new List<PendingLrInvoice>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                NpgsqlCommand command = null;

                StringBuilder SQL_Where = new StringBuilder();
                StringBuilder SQL_Where1 = new StringBuilder();
                QueryBuilder qb = new QueryBuilder();

                if (_supplierpayment.fromdate != DateTime.MinValue && _supplierpayment.todate != DateTime.MinValue)
                {
                    SQL_Where.AppendLine("and lrentry.lrentrydate >= '" + _supplierpayment.fromdate.Date.ToString("yyyy-MM-dd")
                        + "' and lrentry.lrentrydate < '" + _supplierpayment.todate.Date.AddDays(1).ToString("yyyy-MM-dd") + "'");
                }
                if (_supplierpayment.entryfromdate != DateTime.MinValue && _supplierpayment.entrytodate != DateTime.MinValue)
                {
                    SQL_Where.AppendLine("and lrinvoice.entrydate >= '" + _supplierpayment.entryfromdate.Date.ToString("yyyy-MM-dd")
                        + "' and lrinvoice.entrydate < '" + _supplierpayment.entrytodate.Date.AddDays(1).ToString("yyyy-MM-dd") + "'");
                    SQL_Where1.AppendLine("and lrinvoicereturn.returndate >= '" + _supplierpayment.entryfromdate.Date.ToString("yyyy-MM-dd")
                        + "' and lrinvoicereturn.returndate < '" + _supplierpayment.entrytodate.Date.AddDays(1).ToString("yyyy-MM-dd") + "'");
                }
                if (_supplierpayment.invfromdate != DateTime.MinValue && _supplierpayment.invtodate != DateTime.MinValue)
                {
                    SQL_Where.AppendLine("and lrinvoice.invoicedate >= '" + _supplierpayment.invfromdate.Date.ToString("yyyy-MM-dd")
                        + "' and lrinvoice.invoicedate < '" + _supplierpayment.invtodate.Date.AddDays(1).ToString("yyyy-MM-dd") + "'");
                }

                if (_supplierpayment.companyid > 0)
                {
                    SQL_Where.AppendLine("and lrinvoice.companyid=" + _supplierpayment.companyid);
                    SQL_Where1.AppendLine("and lrinvoicereturn.companyid=" + _supplierpayment.companyid);
                }
                if (_supplierpayment.suppliercompanyid > 0)
                {
                    SQL_Where.AppendLine("and lrinvoice.suppliercompanyid=" + _supplierpayment.suppliercompanyid);
                    SQL_Where1.AppendLine("and lrinvoicereturn.suppliercompanyid=" + _supplierpayment.suppliercompanyid);
                }

                if (_supplierpayment.locationid > 0)
                {
                    SQL_Where.AppendLine("and lrentry.entrylocationid=" + _supplierpayment.locationid);
                    SQL_Where1.AppendLine("and lrinvoicereturn.locationid=" + _supplierpayment.locationid);
                }


                SQL_supplierbillsummary = string.Format(SQL_supplierbillsummary, SQL_Where.ToString());
                qb.CurrentQuery = SQL_supplierbillsummary;
                command = qb.GetCommand(conn);

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        PendingLrInvoice oinvoice = new PendingLrInvoice();
                        oinvoice.supplierid = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        oinvoice.supplier = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
                        oinvoice.cityid = reader["cityid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["cityid"]);
                        oinvoice.invoicecount = reader["invoicecount"] == DBNull.Value ? 0 : Convert.ToInt32(reader["invoicecount"]);
                        oinvoice.totalamount = reader["totalamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["totalamount"]);
                        oinvoice.paidamount = reader["paidamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["paidamount"]);
                        oinvoice.balanceamount = oinvoice.totalamount - oinvoice.paidamount;
                        oinvoice.isinvoice = true;

                        oinvoice.msmeno = reader["msmeno"] == DBNull.Value ? "" : reader["msmeno"].ToString();
                        oinvoice.bankname = reader["bankname"] == DBNull.Value ? "" : reader["bankname"].ToString();
                        oinvoice.bankbranch = reader["bankbranch"] == DBNull.Value ? "" : reader["bankbranch"].ToString();
                        oinvoice.bankifsccode = reader["bankifci"] == DBNull.Value ? "" : reader["bankifci"].ToString();
                        oinvoice.bankacno = reader["bankaccountno"] == DBNull.Value ? "" : reader["bankaccountno"].ToString();
                        oinvoice.bankacname = reader["bankaccountname"] == DBNull.Value ? "" : reader["bankaccountname"].ToString();
                        oinvoice.gstno = reader["tinanddate"] == DBNull.Value ? "" : reader["tinanddate"].ToString();

                        if (oinvoice.totalamount - oinvoice.paidamount > 0)
                            lst.Add(oinvoice);
                    }
                }

                SQL_supplierbebitnotesummary = string.Format(SQL_supplierbebitnotesummary, SQL_Where1.ToString());
                qb.CurrentQuery = SQL_supplierbebitnotesummary;
                command = qb.GetCommand(conn);

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        long id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        if (lst.Exists(e => e.supplierid == id))
                        {
                            decimal debitadjusted = 0;
                            PendingLrInvoice oinvoice = lst.Find(e => e.supplierid == id);
                            oinvoice.debetnoteamount = (reader["totalamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["totalamount"]));
                            debitadjusted = (reader["paidamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["paidamount"])) * -1;
                            oinvoice.balanceamount = oinvoice.balanceamount - (oinvoice.debetnoteamount - debitadjusted);
                        }
                        else
                        {
                            PendingLrInvoice oinvoice = new PendingLrInvoice();
                            oinvoice.supplierid = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                            oinvoice.supplier = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
                            oinvoice.cityid = reader["cityid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["cityid"]);
                            oinvoice.invoicecount = reader["invoicecount"] == DBNull.Value ? 0 : Convert.ToInt32(reader["invoicecount"]);
                            oinvoice.debetnoteamount = (reader["totalamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["totalamount"]));
                            oinvoice.paidamount = (reader["paidamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["paidamount"])) * -1;
                            oinvoice.isinvoice = false;

                            oinvoice.msmeno = reader["msmeno"] == DBNull.Value ? "" : reader["msmeno"].ToString();
                            oinvoice.bankname = reader["bankname"] == DBNull.Value ? "" : reader["bankname"].ToString();
                            oinvoice.bankbranch = reader["bankbranch"] == DBNull.Value ? "" : reader["bankbranch"].ToString();
                            oinvoice.bankifsccode = reader["bankifci"] == DBNull.Value ? "" : reader["bankifci"].ToString();
                            oinvoice.bankacno = reader["bankaccountno"] == DBNull.Value ? "" : reader["bankaccountno"].ToString();
                            oinvoice.bankacname = reader["bankaccountname"] == DBNull.Value ? "" : reader["bankaccountname"].ToString();
                            oinvoice.gstno = reader["tinanddate"] == DBNull.Value ? "" : reader["tinanddate"].ToString();

                            if (oinvoice.debetnoteamount - oinvoice.paidamount > 0)
                            {
                                oinvoice.debetnoteamount = oinvoice.debetnoteamount * -1;
                                oinvoice.paidamount = oinvoice.paidamount * -1;
                                oinvoice.balanceamount = oinvoice.debetnoteamount - oinvoice.paidamount;
                                lst.Add(oinvoice);
                            }
                        }
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _supplierpayment);
            }
            return lst;

        }


        internal List<PlannedInvoice> SetPlannedPayment(List<PendingLrInvoice> items, long locationid)
        {
            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = GetCommand("Update lrinvoice set plannedpayment=@plannedpayment where id=@id", conn, transaction);

                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = 0;
                    command.Parameters.Add("plannedpayment", NpgsqlTypes.NpgsqlDbType.Numeric).Value = 0;
                    foreach (var item in items.Where(e => e.isinvoice))
                    {
                        command.Parameters["id"].Value = item.lrinvoiceid;
                        command.Parameters["plannedpayment"].Value = item.plannedpayment;
                        command.ExecuteNonQuery();
                    }

                    command = GetCommand("Update Jobworkbillgrn set plannedpayment=@plannedpayment where id=@id", conn, transaction);

                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = 0;
                    command.Parameters.Add("plannedpayment", NpgsqlTypes.NpgsqlDbType.Numeric).Value = 0;
                    foreach (var item in items.Where(e => e.isjobwork))
                    {
                        command.Parameters["id"].Value = item.lrinvoiceid;
                        command.Parameters["plannedpayment"].Value = item.plannedpayment;
                        command.ExecuteNonQuery();
                    }

                    command = GetCommand("Update lrinvoicereturn set plannedpayment=@plannedpayment where id=@id", conn, transaction);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = 0;
                    command.Parameters.Add("plannedpayment", NpgsqlTypes.NpgsqlDbType.Numeric).Value = 0;
                    foreach (var item in items.Where(e => e.isdebitnote))
                    {
                        command.Parameters["id"].Value = item.lrinvoiceid;
                        command.Parameters["plannedpayment"].Value = item.plannedpayment;
                        command.ExecuteNonQuery();
                    }
                    transaction.Commit();
                    conn.Close();
                }
                if (items.Exists(e => e.suppliercompanyid > 0))
                {
                    return SearchPlannedPayment(items.ConvertAll(e => e.suppliercompanyid), locationid);
                }
                return new List<PlannedInvoice>();
            }
            catch (Exception exp)
            {
                return null;
            }
        }

        public Supplierpayment Insert(Supplierpayment oSupplierpayment)
        {
            if (oSupplierpayment.Supplierpaymentinvoice.Exists(e => e.supplierid != oSupplierpayment.supplierid))
                throw new Exception("Supplier mismatch with invoices.");

            NpgsqlTransaction transaction = null;
            try
            {
                if (oSupplierpayment.Supplierpaymentinvoice != null &&
                    oSupplierpayment.Supplierpaymentinvoice.Exists(x => x.agentcommissionvalue > 0))
                {
                    if (oSupplierpayment.Supplierpaymentinvoice.FindAll(x => x.agentid != 0).GroupBy(x => x.agentid).Count() > 1)
                    {
                        throw new Exception("Multiple agent commission can't be distributed in single supplier payment voucher");
                    }
                    else
                    {
                        oSupplierpayment.agntcomm = oSupplierpayment.Supplierpaymentinvoice.Sum(x => x.agentcommissionvalue);
                        var agentid = oSupplierpayment.Supplierpaymentinvoice.Max(x => x.agentid);
                        if (agentid != 0)
                            oSupplierpayment.agentid = agentid;
                    }
                }

                SuppliercompanyDataService suppliercompanyDataService = new SuppliercompanyDataService() { Context = this.Context };
                var supplieraccounts = suppliercompanyDataService.Search(new SuppliercompanyCreteria()
                {
                    id = oSupplierpayment.suppliercompanyid
                });

                if (supplieraccounts.Count == 1)
                {
                    if (oSupplierpayment.paymentinfo == null)
                        oSupplierpayment.paymentinfo = new Supplierpayment.PaymentInfo();
                    oSupplierpayment.paymentinfo.bankacno = supplieraccounts.First().bankaccountno;
                    oSupplierpayment.paymentinfo.bankacname = supplieraccounts.First().bankaccountname;
                    oSupplierpayment.paymentinfo.bankifsccode = supplieraccounts.First().bankifci;
                    oSupplierpayment.paymentinfo.bankname = supplieraccounts.First().bankname;
                    oSupplierpayment.paymentinfo.bankbranch = supplieraccounts.First().bankbranch;

                    foreach (var cheque in oSupplierpayment.bankcheques)
                    {
                        cheque.entrydate = oSupplierpayment.paymentdate;
                        cheque.locationid = oSupplierpayment.locationid;
                        cheque.bankbranch = oSupplierpayment.paymentinfo.bankbranch;
                        cheque.bankaccountno = oSupplierpayment.paymentinfo.bankacno;
                        cheque.bankifsc = oSupplierpayment.paymentinfo.bankifsccode;
                        cheque.referenceno = oSupplierpayment.receiptno;
                        cheque.ledgername = supplieraccounts.First().name;
                        cheque.accountname = oSupplierpayment.paymentinfo.bankacname;
                    }
                }

                if (oSupplierpayment.tdsamount > 0 && !oSupplierpayment.tdsoninv && oSupplierpayment.tdsledgerid == 0)
                {
                    if (supplieraccounts.Count > 0 && supplieraccounts[0].tdsgroupid != 0)
                        oSupplierpayment.tdsledgerid = supplieraccounts[0].tdsgroupid;
                    else
                        oSupplierpayment.tdsledgerid = Services.ConfigManager.Instance(Context.OrganizationID).Config.fin_paytdsledgerid;
                }

                var sequence = SequenceManager.Instance(Context.OrganizationID).Get("supplierpayment.receiptno",
                    Services.ConfigManager.Instance(Context.OrganizationID).Config.system_supportlocationwisesupplierpaymentno ? oSupplierpayment.locationid : 0,
                    InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_supportcompanywiseentryno ?
                    oSupplierpayment.companyid : 0);

                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);
                    oSupplierpayment.receiptno = sequence.GetNextNumber(Context, conn, transaction);

                    Insert(transaction, conn, oSupplierpayment);
                    SupplierpaymentinvoiceDataService subSupplierpaymentinvoice = new SupplierpaymentinvoiceDataService() { Context = this.Context };
                    foreach (var item in oSupplierpayment.Supplierpaymentinvoice)
                    {
                        item.supplierpaymentid = oSupplierpayment.id;
                        item.version = 1;
                        item.isactive = true;
                        item.createdby = this.Context.UserID;
                        item.modifiedby = this.Context.UserID;
                        item.createdon = DateTimeNow;
                        item.modifiedon = DateTimeNow;
                        subSupplierpaymentinvoice.Insert(transaction, conn, item);
                    }

                    BankchequetransactionDataService bankchequetransactionDataService = new BankchequetransactionDataService() { Context = this.Context };
                    if (oSupplierpayment.bankcheques != null)
                    {
                        foreach (var item in oSupplierpayment.bankcheques)
                        {
                            item.sourceid = oSupplierpayment.id;
                            item.sourcetype = "SPMT";
                            item.version = 1;
                            item.isactive = true;
                            item.createdby = this.Context.UserID;
                            item.modifiedby = this.Context.UserID;
                            item.createdon = DateTimeNow;
                            item.modifiedon = DateTimeNow;
                            item.status = 0;
                            bankchequetransactionDataService.Insert(transaction, conn, item);
                        }
                    }
                    transaction.Commit();
                    conn.Close();
                }

                if (Processors.Schedule_Actions.Instance.HasConfig(Context.OrganizationID, Processors.Schedule_Actions.SourceType.SupplierPayment, Processors.Schedule_Actions.Actions.New))
                {
                    string licensekey = LicenseManager.License.Key.Replace("-", "");
                    InfyPOS.Controllers.ImageController.HashDetail hashDetail = new InfyPOS.Controllers.ImageController.HashDetail(Context.OrganizationID, InfyPOS.Controllers.ImageController.HashDetail.GetLongHash(licensekey), oSupplierpayment.id);
                    var url = InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.hostedserver + (InfyPOS.Services.ConfigManager.Instance(new BaseData().Context.OrganizationID).Config.hostedserver.EndsWith("/") ? "" : "/") + "File/SP/" + LicenseManager.License.Key.Replace("-", "") + "/" + System.Web.HttpUtility.UrlEncode(hashDetail.Hash);
                    var pSupplierpayment = Print(oSupplierpayment);
                    pSupplierpayment.url = url;
                    Processors.Schedule_Actions.Instance.OnChange(Context.OrganizationID, pSupplierpayment, Processors.Schedule_Actions.Actions.New);
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oSupplierpayment);
                throw exp;
            }

            return oSupplierpayment;
        }

        public Supplierpayment Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Supplierpayment oSupplierpayment)
        {

            NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("supplierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSupplierpayment.supplierid;
            command.Parameters.Add("receiptno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSupplierpayment.receiptno == null ? "" : oSupplierpayment.receiptno;
            command.Parameters.Add("paymentdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oSupplierpayment.paymentdate;
            command.Parameters.Add("datatransmitted", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oSupplierpayment.datatransmitted;
            command.Parameters.Add("paidamount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oSupplierpayment.paidamount;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oSupplierpayment.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oSupplierpayment.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("suppliercompanyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSupplierpayment.suppliercompanyid;
            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSupplierpayment.companyid;
            command.Parameters.Add("bankid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSupplierpayment.bankid;
            command.Parameters.Add("paymentmode", NpgsqlTypes.NpgsqlDbType.Integer).Value = oSupplierpayment.paymentmode;
            command.Parameters.Add("instrumentdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oSupplierpayment.instrumentdate;
            command.Parameters.Add("referencedetail", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSupplierpayment.referencedetail == null ? "" : oSupplierpayment.referencedetail;
            command.Parameters.Add("remarks", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSupplierpayment.remarks == null ? "" : oSupplierpayment.remarks;
            command.Parameters.Add("paymentinfo", NpgsqlTypes.NpgsqlDbType.Json).Value = oSupplierpayment.paymentinfo == null ? "null" : oSupplierpayment.paymentinfo_json;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSupplierpayment.locationid;
            command.Parameters.Add("hastds", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oSupplierpayment.hastds;
            command.Parameters.Add("tdsoninv", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oSupplierpayment.tdsoninv;
            command.Parameters.Add("tdsledgerid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSupplierpayment.tdsledgerid;
            command.Parameters.Add("tdspercentage", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oSupplierpayment.tdspercentage;
            command.Parameters.Add("tdsamount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oSupplierpayment.tdsamount;
            command.Parameters.Add("rounding", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oSupplierpayment.rounding;
            command.Parameters.Add("agntid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSupplierpayment.agentid;
            command.Parameters.Add("agntcomm", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oSupplierpayment.agntcomm;
            command.Parameters.Add("agntpaid", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oSupplierpayment.agntpaid;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oSupplierpayment.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            return oSupplierpayment;
        }

        public Supplierpayment Update(Supplierpayment oSupplierpayment)
        {
            /*with commsnupdate as
            (
            select sum(agentcommissionvalue) value,supplierpaymentid from Supplierpaymentinvoice 
            where agentcommissionvalue>0 and isactive=true
            group by supplierpaymentid
            )
            update supplierpayment set agntcomm = value from commsnupdate
            where commsnupdate.supplierpaymentid = supplierpayment.id*/

            if (oSupplierpayment.Supplierpaymentinvoice.Exists(e => e.supplierid != oSupplierpayment.supplierid))
                throw new Exception("Supplier mismatch with invoices.");

            NpgsqlTransaction transaction = null;
            try
            {
                oSupplierpayment.agntcomm = 0;
                if (oSupplierpayment.Supplierpaymentinvoice != null &&
                    oSupplierpayment.Supplierpaymentinvoice.Exists(x => x.agentcommissionvalue > 0))
                {
                    if (oSupplierpayment.Supplierpaymentinvoice.FindAll(x => x.agentid != 0).GroupBy(x => x.agentid).Count() > 1)
                    {
                        throw new Exception("Multiple agent commission can't be distributed in single supplier payment voucher");
                    }
                    else
                    {
                        oSupplierpayment.agntcomm = oSupplierpayment.Supplierpaymentinvoice.Sum(x => x.agentcommissionvalue);
                        var agentid = oSupplierpayment.Supplierpaymentinvoice.Max(x => x.agentid);
                        if (agentid != 0)
                            oSupplierpayment.agentid = agentid;
                    }
                }

                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();

                    SuppliercompanyDataService suppliercompanyDataService = new SuppliercompanyDataService() { Context = this.Context };
                    var supplieraccounts = suppliercompanyDataService.Search(new SuppliercompanyCreteria()
                    {
                        id = oSupplierpayment.suppliercompanyid
                    });

                    if (supplieraccounts.Count == 1)
                    {
                        if (oSupplierpayment.paymentinfo == null)
                            oSupplierpayment.paymentinfo = new Supplierpayment.PaymentInfo();
                        oSupplierpayment.paymentinfo.bankacno = supplieraccounts.First().bankaccountno;
                        oSupplierpayment.paymentinfo.bankacname = supplieraccounts.First().bankaccountname;
                        oSupplierpayment.paymentinfo.bankifsccode = supplieraccounts.First().bankifci;
                        oSupplierpayment.paymentinfo.bankname = supplieraccounts.First().bankname;
                        oSupplierpayment.paymentinfo.bankbranch = supplieraccounts.First().bankbranch;

                        foreach (var cheque in oSupplierpayment.bankcheques)
                        {
                            cheque.entrydate = oSupplierpayment.paymentdate;
                            cheque.locationid = oSupplierpayment.locationid;
                            cheque.accountname = oSupplierpayment.paymentinfo.bankacname;
                            cheque.bankbranch = oSupplierpayment.paymentinfo.bankbranch;
                            cheque.bankaccountno = oSupplierpayment.paymentinfo.bankacno;
                            cheque.bankifsc = oSupplierpayment.paymentinfo.bankifsccode;
                        }
                    }

                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oSupplierpayment);
                    SupplierpaymentinvoiceDataService subSupplierpaymentinvoice = new SupplierpaymentinvoiceDataService() { Context = this.Context };
                    subSupplierpaymentinvoice.Update(transaction, conn, oSupplierpayment.Supplierpaymentinvoice, oSupplierpayment.id);
                    BankchequetransactionDataService bankchequetransactionDataService = new BankchequetransactionDataService() { Context = this.Context };
                    bankchequetransactionDataService.Update(transaction, conn, oSupplierpayment.bankcheques, oSupplierpayment.id, "SPMT");
                    transaction.Commit();
                    conn.Close();
                }
                if (Processors.Schedule_Actions.Instance.HasConfig(Context.OrganizationID, Processors.Schedule_Actions.SourceType.SupplierPayment, Processors.Schedule_Actions.Actions.New))
                {
                    string licensekey = LicenseManager.License.Key.Replace("-", "");
                    InfyPOS.Controllers.ImageController.HashDetail hashDetail = new InfyPOS.Controllers.ImageController.HashDetail(Context.OrganizationID, InfyPOS.Controllers.ImageController.HashDetail.GetLongHash(licensekey), oSupplierpayment.id);
                    var url = InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.hostedserver + (InfyPOS.Services.ConfigManager.Instance(new BaseData().Context.OrganizationID).Config.hostedserver.EndsWith("/") ? "" : "/") + "File/SP/" + LicenseManager.License.Key.Replace("-", "") + "/" + System.Web.HttpUtility.UrlEncode(hashDetail.Hash);
                    var pSupplierpayment = Print(oSupplierpayment);
                    pSupplierpayment.url = url;
                    var fileattachment = PrintChequeAdvice(Context.OrganizationID, pSupplierpayment);
                    pSupplierpayment.attachment = new SortedDictionary<string, byte[]>();
                    pSupplierpayment.attachment.Add("Payment-" + pSupplierpayment.receiptno, fileattachment.Content.ReadAsByteArrayAsync().Result);
                    Processors.Schedule_Actions.Instance.OnChange(Context.OrganizationID, pSupplierpayment, Processors.Schedule_Actions.Actions.New);
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oSupplierpayment);
                throw exp;
            }
            return oSupplierpayment;
        }

        public Controllers.FileContentInfo PrintChequeAdvice(long organizationid, SupplierPaymentPrint supplierPaymentPrint)
        {
            InfyPOS.Data.ConfigurationDataService config = new ConfigurationDataService() { Context = new UserClaim() { OrganizationID = organizationid } };
            var lstconfig = config.Search(new ConfigurationCreteria
            {
                key = "CHEQUEADVICE"
            });

            if (lstconfig == null || lstconfig.Count == 0)
                return null;

            if (lstconfig.Count > 1 &&
                lstconfig.Exists(x => x.organizationid == supplierPaymentPrint.company.id))
            {
                lstconfig = lstconfig.FindAll(x => x.organizationid == supplierPaymentPrint.company.id);
            }

            if (lstconfig[0].reportformat.format.Contains("Balance"))
            {
                SupplierpaymentDataService SupplierpaymentData = new SupplierpaymentDataService();
                var pendinglist = SupplierpaymentData.SearchPendingJobWorkPayment(new SupplierpaymentCreteria()
                {
                    suppliercompanyid = supplierPaymentPrint.suppliercompany.id,
                    supplierid = supplierPaymentPrint.supplier.id
                });

                pendinglist.RemoveAll(e => e.isdebitnote);

                pendinglist.AddRange(SupplierpaymentData.SearchPendingPayment(new SupplierpaymentCreteria()
                {
                    suppliercompanyid = supplierPaymentPrint.suppliercompany.id,
                    supplierid = supplierPaymentPrint.supplier.id
                }));

                pendinglist.RemoveAll(e => e.invoicedate > supplierPaymentPrint.paymentdate.Date.AddDays(1));

                pendinglist.ForEach(e =>
                {
                    if (e.isdebitnote)
                        e.totalamount *= -1;
                });

                supplierPaymentPrint.Balance = pendinglist;
                supplierPaymentPrint.hasbalance = pendinglist != null && pendinglist.Count > 0;
                supplierPaymentPrint.BalanceBillValue = pendinglist.Sum(e => e.isinvoice || e.isjobwork ? e.balanceamount : 0);
                supplierPaymentPrint.BalanceDebitnote = Math.Round(pendinglist.Sum(e => e.isinvoice || e.isjobwork ? 0 : e.balanceamount));
                supplierPaymentPrint.BalancePayable = supplierPaymentPrint.BalanceBillValue - supplierPaymentPrint.BalanceDebitnote;
            }

            ReportLibrary.LaserReporter labreport = new ReportLibrary.LaserReporter(organizationid);
            labreport.format = ReportLibrary.DosFormatParser.ParseXML(organizationid, lstconfig[0].reportformat.format);
            labreport.DefaultPageSettings = labreport.format.GetDefaultSetting();
            labreport.Additional = new SortedDictionary<string, object>();
            ReportLibrary.PDFPrinter printer = new ReportLibrary.PDFPrinter();
            printer.Report = labreport;
            printer.EnablePageNumber = lstconfig[0].reportformat.format.Contains("PageCount");
            labreport.DataSource = supplierPaymentPrint;

            List<string> imageids = labreport.format.GetImages();
            var imageData = new ImageData() { Context = new UserClaim() { OrganizationID = organizationid } };
            foreach (var imgid in imageids.FindAll(x => x.Length > 6))
            {
                var file = imageData.GetData(imgid.LongFromBase62());
                if (file != null)
                {
                    var img = imageData.FromBytes(file.data);
                    labreport.Additional.Add("IMG-" + imgid, img);
                }
            }

            if (supplierPaymentPrint.company != null && !string.IsNullOrEmpty(supplierPaymentPrint.company.logo))
            {
                var file = imageData.GetData(supplierPaymentPrint.company.logo.LongFromBase62());
                if (file != null && file.data != null)
                {
                    var img = imageData.FromBytes(file.data);
                    labreport.Additional.Add("LOGO", img);
                }
            }

            using (System.IO.MemoryStream stream = printer.Print(labreport.format.GetDefaultSetting()))
            {
                var fci = new Controllers.FileContentInfo(new ByteArrayContent(stream.ToArray()));
                fci.FileName = "chequeadvice.pdf";
                fci.Extension = "application/pdf";
                return fci;
            }
        }

        public Supplierpayment Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Supplierpayment oSupplierpayment)
        {
            NpgsqlCommand command = GetCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSupplierpayment.id;
            command.Parameters.Add("supplierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSupplierpayment.supplierid;
            command.Parameters.Add("receiptno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSupplierpayment.receiptno == null ? "" : oSupplierpayment.receiptno;
            command.Parameters.Add("paymentdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oSupplierpayment.paymentdate;
            command.Parameters.Add("datatransmitted", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oSupplierpayment.datatransmitted;
            command.Parameters.Add("paidamount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oSupplierpayment.paidamount;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oSupplierpayment.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSupplierpayment.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oSupplierpayment.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oSupplierpayment.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("suppliercompanyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSupplierpayment.suppliercompanyid;
            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSupplierpayment.companyid;
            command.Parameters.Add("bankid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSupplierpayment.bankid;
            command.Parameters.Add("paymentmode", NpgsqlTypes.NpgsqlDbType.Integer).Value = oSupplierpayment.paymentmode;
            command.Parameters.Add("referencedetail", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSupplierpayment.referencedetail == null ? "" : oSupplierpayment.referencedetail;
            command.Parameters.Add("instrumentdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oSupplierpayment.instrumentdate;
            command.Parameters.Add("remarks", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSupplierpayment.remarks == null ? "" : oSupplierpayment.remarks;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSupplierpayment.locationid;
            command.Parameters.Add("paymentinfo", NpgsqlTypes.NpgsqlDbType.Json).Value = oSupplierpayment.paymentinfo == null ? "null" : oSupplierpayment.paymentinfo_json;
            command.Parameters.Add("hastds", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oSupplierpayment.hastds;
            command.Parameters.Add("tdsoninv", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oSupplierpayment.tdsoninv;
            command.Parameters.Add("tdsledgerid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSupplierpayment.tdsledgerid;
            command.Parameters.Add("tdspercentage", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oSupplierpayment.tdspercentage;
            command.Parameters.Add("tdsamount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oSupplierpayment.tdsamount;
            command.Parameters.Add("rounding", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oSupplierpayment.rounding;
            command.Parameters.Add("agntid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSupplierpayment.agentid;
            command.Parameters.Add("agntcomm", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oSupplierpayment.agntcomm;
            command.Parameters.Add("agntpaid", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oSupplierpayment.agntpaid;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oSupplierpayment.version = Convert.ToInt32(reader[0].ToString());
                    if (Convert.ToBoolean(reader[1]))
                    {
                        throw new Exception(oSupplierpayment.receiptno + " Cancelled can't update");
                    }
                }
            }
            return oSupplierpayment;
        }


        public bool Delete(Supplierpayment oSupplierpayment)
        {
            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oSupplierpayment);
                    SupplierpaymentinvoiceDataService subSupplierpaymentinvoice = new SupplierpaymentinvoiceDataService() { Context = this.Context };
                    subSupplierpaymentinvoice.DeleteAll(oSupplierpayment.id, oSupplierpayment.modifiedby, conn, transaction);
                    BankchequetransactionDataService bankchequetransactionDataService = new BankchequetransactionDataService() { Context = this.Context };
                    bankchequetransactionDataService.DeleteAll(oSupplierpayment.id, "SPMT", conn, transaction);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oSupplierpayment);

                throw exp;
            }
            return hasSuccessCount;
        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Supplierpayment oSupplierpayment)
        {

            NpgsqlCommand command = GetCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oSupplierpayment.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oSupplierpayment.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSupplierpayment.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

        internal List<Supplierpayment> SaveAll(Supplierpayment item)
        {
            List<Supplierpayment> response = new List<Supplierpayment>();
            SuppliercompanyDataService suppliercompanyDataService = new SuppliercompanyDataService() { Context = this.Context };
            var supplieraccounts = suppliercompanyDataService.Search(new SuppliercompanyCreteria()
            {
                ids = item.Supplierpaymentinvoice.ConvertAll(e => e.supplierid)
            });
            foreach (var spayment in item.Supplierpaymentinvoice.GroupBy(e => new { e.supplierid, e.companyid, e.agentid }))
            {
                var supplieraccount = supplieraccounts.Find(e => e.supplierid == spayment.Key.supplierid);
                Supplierpayment newpayment = new Supplierpayment();
                newpayment.paymentinfo = new Supplierpayment.PaymentInfo();
                newpayment.paymentinfo.bankacno = supplieraccount.bankaccountno;
                newpayment.paymentinfo.bankacname = supplieraccount.bankaccountname;
                newpayment.paymentinfo.bankifsccode = supplieraccount.bankifci;
                newpayment.paymentinfo.bankname = supplieraccount.bankname;
                newpayment.paymentinfo.bankbranch = supplieraccount.bankbranch;
                newpayment.bankid = item.bankid;
                newpayment.paymentmode = item.paymentmode;
                newpayment.referencedetail = item.referencedetail;
                newpayment.instrumentdate = item.instrumentdate;
                newpayment.remarks = item.remarks;
                newpayment.suppliercompanyid = supplieraccount.id;
                newpayment.supplierid = spayment.Key.supplierid;
                newpayment.companyid = spayment.Key.companyid;
                newpayment.agentid = spayment.Key.agentid;
                newpayment.isactive = true;
                newpayment.locationid = item.locationid;
                newpayment.modifiedby = Context.UserID;
                newpayment.modifiedon = DateTimeNow;
                newpayment.paidamount = spayment.Sum(x => x.paidamount);
                newpayment.paymentdate = item.paymentdate;
                newpayment.remarks = item.remarks;
                newpayment.totaldiscount = spayment.Sum(x => x.discountamount);
                newpayment.debitnotediscount = spayment.Sum(x => x.islrinvoicereturn ? x.discountamount : 0);
                newpayment.debitnotetotal = spayment.Sum(x => x.islrinvoicereturn ? x.paidamount : 0);
                newpayment.Supplierpaymentinvoice = new List<Supplierpaymentinvoice>();
                newpayment.Supplierpaymentinvoice.AddRange(spayment);
                response.Add(Insert(newpayment));
            }
            return response;
        }

        public SupplierPaymentPrint Print(Supplierpayment oSupplierpayment)
        {
            var payment = Search(new SupplierpaymentCreteria()
            {
                id = oSupplierpayment.id,
                includebalance = true
            });
            if (payment.Count != 1) return null;
            SupplierPaymentPrint supplierPaymentPrint = new SupplierPaymentPrint();
            using (var reader = ExecuteReader("select employee.name from users inner join employee on employeeid = employee.id where users.id=" + payment.First().createdby, null))
            {
                if (reader.Read())
                    supplierPaymentPrint.createdby = reader.IsDBNull(0) ? "" : reader.GetString(0);
            }

            payment[0].Supplierpaymentinvoice.Where(item => new long[] { 5, 6, 7, 11, 12, 13 }.Contains(item.returntype)).ToList().ForEach(item => item.islrinvoicereturn = true);

            supplierPaymentPrint.id = payment[0].id;
            supplierPaymentPrint.agentid = payment[0].agentid;
            supplierPaymentPrint.paymentmode = (int)payment[0].paymentmode;
            supplierPaymentPrint.invoicelist = payment[0].Supplierpaymentinvoice;
            supplierPaymentPrint.Bills = supplierPaymentPrint.invoicelist.Where(e => !e.islrinvoicereturn).ToList();
            supplierPaymentPrint.BillPayable = supplierPaymentPrint.Bills.Sum(e => e.invoiceamount);
            supplierPaymentPrint.BillDiscount = supplierPaymentPrint.Bills.Sum(e => e.discountamount);
            supplierPaymentPrint.BillPaid = supplierPaymentPrint.Bills.Sum(e => e.actualpaidamount);
            supplierPaymentPrint.BillGross = supplierPaymentPrint.Bills.Sum(e => e.paidamount);
            supplierPaymentPrint.Returns = supplierPaymentPrint.invoicelist.Where(e => e.islrinvoicereturn).ToList();
            supplierPaymentPrint.ReturnGross = supplierPaymentPrint.Returns.Sum(e => e.paidamount);
            supplierPaymentPrint.ReturnDiscount = supplierPaymentPrint.Returns.Sum(e => e.discountamount);
            supplierPaymentPrint.ReturnAdjusted = supplierPaymentPrint.Returns.Sum(e => e.actualpaidamount);
            supplierPaymentPrint.TotalDiscount = supplierPaymentPrint.BillDiscount + supplierPaymentPrint.ReturnDiscount;
            if (supplierPaymentPrint.TotalDiscount > 0)
            {
                supplierPaymentPrint.DiscountPercentage = Math.Round(supplierPaymentPrint.TotalDiscount / (supplierPaymentPrint.BillGross + supplierPaymentPrint.ReturnGross) * 100, 2);
            }
            supplierPaymentPrint.paymentdate = payment[0].paymentdate;
            supplierPaymentPrint.createdon = payment[0].createdon;
            supplierPaymentPrint.receiptno = payment[0].receiptno;
            supplierPaymentPrint.cheques = payment[0].bankcheques;
            supplierPaymentPrint.remarks = payment[0].remarks;
            supplierPaymentPrint.rounding = payment[0].rounding;
            supplierPaymentPrint.PaidAmount = payment[0].paidamount;

            supplierPaymentPrint.hastds = payment[0].hastds;
            supplierPaymentPrint.tdsoninv = payment[0].tdsoninv;
            supplierPaymentPrint.tdsledgerid = payment[0].tdsledgerid;
            supplierPaymentPrint.tdspercentage = payment[0].tdspercentage;
            supplierPaymentPrint.tdsamount = payment[0].tdsamount;

            supplierPaymentPrint.agentname = payment[0].agentname;

            if (supplierPaymentPrint.cheques != null)
            {
                Data.ReferencelistDataService locationdata = new Data.ReferencelistDataService() { Context = this.Context };
                var bankdata = locationdata.Search(new ReferencelistCreteria()
                {
                    type = "BANK"
                });
                supplierPaymentPrint.cheques.ForEach(e =>
                {
                    if (bankdata.Exists(b => b.id == e.bankid))
                        e.bankname = bankdata.Find(b => b.id == e.bankid).name;
                });
            }
            SupplierDataService _supplierdata = new SupplierDataService() { Context = this.Context };
            var supplier = _supplierdata.Search(new SupplierCreteria()
            {
                id = payment[0].supplierid
            });
            if (supplier != null && supplier.Count > 0)
            {
                supplierPaymentPrint.supplier = supplier[0];
                supplierPaymentPrint.suppliercompany = supplier[0].companylist != null && supplier[0].companylist.Count > 0 ? supplier[0].companylist[0] : null;
            }

            CompanyDataService companyDataService = new CompanyDataService() { Context = this.Context };
            var company = companyDataService.Search(new CompanyCreteria()
            {
                id = payment[0].companyid
            });
            if (company.Count == 1)
            {
                supplierPaymentPrint.company = company.First();
                supplierPaymentPrint.company.parseaddress();
            }

            if (payment[0].locationid > 0)
            {
                ReferencelistDataService referencelistDataService = new ReferencelistDataService();
                var location = referencelistDataService.Search(new ReferencelistCreteria()
                {
                    id = payment[0].locationid
                }).First();

                supplierPaymentPrint.location = new CompanyLocation(location);
                supplierPaymentPrint.location.name = location.name;
                supplierPaymentPrint.location.phonenumber = location.additionalinfo == null || location.additionalinfo.locationconfig == null ? "" : location.additionalinfo.locationconfig.contactno;
                supplierPaymentPrint.location.address = location.additionalinfo == null ? "" : location.additionalinfo.address;
                supplierPaymentPrint.location.pincode = location.additionalinfo == null || location.additionalinfo.locationconfig == null ? "" : location.additionalinfo.locationconfig.pincode;
                supplierPaymentPrint.location.addresslines = new List<CompanyLocation.Address>();
                if (!string.IsNullOrEmpty(supplierPaymentPrint.location.address))
                {
                    var addresslist = supplierPaymentPrint.location.address.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                    addresslist.ToList().ForEach(e =>
                    {
                        supplierPaymentPrint.location.addresslines.Add(new CompanyLocation.Address()
                        {
                            value = e
                        });
                    });
                }
                if (!string.IsNullOrEmpty(supplierPaymentPrint.location.address))
                {
                    var addresslist = supplierPaymentPrint.location.address.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                    addresslist.ToList().ForEach(e =>
                    {
                        supplierPaymentPrint.location.addresslines.Add(new CompanyLocation.Address()
                        {
                            value = e
                        });
                    });
                }

            }
            return supplierPaymentPrint;
        }
        internal PrintableLedger PrintSummary(long supplierid, long locationid, long companyid, int fyear, bool includevouchers)
        {
            DateTime fromdate = BaseEntity.GetDataFromString("01-04-" + fyear.ToString("F0"));
            DateTime todate = BaseEntity.GetDataFromString("31-03-" + (fyear + 1).ToString("F0"));
            return PrintSummary(supplierid, locationid, companyid, fromdate, todate, includevouchers);
        }


        internal PrintableLedger PrintSummary(long supplierid, long location, long companyid,
            DateTime fromdate, DateTime todate, bool includevouchers)
        {
            PrintableLedger printableLedger = new PrintableLedger() { transactions = new List<PrintableLedger.Transaction>() };
            SuppliercompanyDataService suppliercompanyDataService = new SuppliercompanyDataService() { Context = this.Context };
            var suppliercompany = suppliercompanyDataService.Search(new SuppliercompanyCreteria()
            {
                supplierid = supplierid
            });

            decimal pendingamount = 0;
            VoucherDataService voucherDataService = new VoucherDataService() { Context = this.Context };
            if (includevouchers)
            {
                LedgerDataService ledgerDataService = new LedgerDataService()
                {
                    Context = this.Context,
                };

                var supplierledger = ledgerDataService.Search(new LedgerCreteria()
                {
                    supplierid = supplierid
                });
                if (supplierledger.Count == 1 &&
                    supplierledger[0].attributes != null &&
                    supplierledger[0].attributes.issupplier)
                {
                    var supplierledgervouchersummary = voucherDataService.SearchLedger(new VoucherCreteria()
                    {
                        companyid = companyid,
                        fromdate = fromdate,
                        todate = todate,
                        ledgerid = supplierledger[0].id,
                        onvoucherledger = true
                    });

                    var obrecords = supplierledgervouchersummary.Find(x => x.id == -1 && x.no == "OB");
                    //if (includevouchers)
                    //{
                    //    if (obrecords.crdr == "Cr")
                    //        pendingamount = obrecords.amount;
                    //    else
                    //        pendingamount = obrecords.amount * -1;
                    //}
                    supplierledgervouchersummary.Remove(obrecords);

                    foreach (var svs in supplierledgervouchersummary)
                    {
                        printableLedger.transactions.Add(new PrintableLedger.Transaction()
                        {
                            createdby = svs.createdby,
                            modifiedby = svs.modifiedby,
                            modifiedon = svs.modifiedon,
                            createdon = svs.createdon,
                            type = "VOUCHER - " + svs.vouchername,
                            date = svs.date,
                            description = svs.narration,
                            credit = svs.crdr == "Cr" ? svs.amount.ToString("F2") : "",
                            debit = svs.crdr == "Dr" ? svs.amount.ToString("F2") : "",
                            detail = new VoucherDisplayItem()
                            {
                                invoicedate = svs.invoicedate,
                                invoiceno = svs.invoiceno
                            }
                        });
                    }
                }
            }

            string query = @"select sum(lrinvoice.totalamount-coalesce(lrinvoice.presettled,0)) as totalamount,
sum(lrinvoice.tdsamount) tdsamount from lrinvoice 
left outer join lrentry on lrentry.id = lrid
where lrinvoice.isactive = true and lrinvoice.settled=false and entrydate<@fromdate
and suppliercompanyid = @suppliercompanyid and companyid=@companyid
and lrentry.entrylocationid=@locationid;

with invoicevalue as (
    select Supplierpaymentid paymentid,sum(Supplierpaymentinvoice.paidamount) paidamount,
    sum(Supplierpaymentinvoice.discountamount) discountamount,
    sum(Supplierpaymentinvoice.agentcommissionvalue) agentcommissionvalue 
    from Supplierpayment 
    inner join Supplierpaymentinvoice on Supplierpaymentinvoice.Supplierpaymentid = Supplierpayment.id
    where Supplierpayment.isactive = true and Supplierpaymentinvoice.isactive = true 
    and supplierpaymentinvoice.hascancel=false and islrinvoicereturn = false  and supplierpayment.hascancel=false 
    and paymentdate<@fromdate
    and suppliercompanyid = @suppliercompanyid and companyid=@companyid
    and supplierpayment.locationid=@locationid
    group by Supplierpaymentid
),debitnotevalue as (
    select Supplierpaymentid paymentid,sum(Supplierpaymentinvoice.paidamount) paidamount,sum(Supplierpaymentinvoice.discountamount) discountamount 
    from Supplierpayment 
    inner join Supplierpaymentinvoice on Supplierpaymentinvoice.Supplierpaymentid = Supplierpayment.id
    where Supplierpayment.isactive = true  and supplierpayment.hascancel=false and 
    Supplierpaymentinvoice.isactive = true and supplierpaymentinvoice.hascancel=false 
    and (islrinvoicereturn = true or iscreditnote=true)
    and paymentdate<@fromdate 
    and suppliercompanyid = @suppliercompanyid and companyid=@companyid
    and supplierpayment.locationid=@locationid
    group by Supplierpaymentid
)

select sum(supplierpayment.paidamount) paidamount,
sum(debitnotevalue.paidamount) dnpaidamount,
sum(debitnotevalue.discountamount) dndiscount,
sum(invoicevalue.discountamount) invdiscount,
sum(case when tdsoninv = false then supplierpayment.tdsamount else 0 end) tdsamount,
sum(invoicevalue.agentcommissionvalue) agentcommissionvalue
from supplierpayment
left outer join invoicevalue on invoicevalue.paymentid = supplierpayment.id
left outer join debitnotevalue on debitnotevalue.paymentid = supplierpayment.id
where supplierpayment.isactive = true  and supplierpayment.hascancel=false and paymentdate<@fromdate
and suppliercompanyid = @suppliercompanyid and companyid=@companyid
and supplierpayment.locationid=@locationid;

select sum(case when returntype<10 then total else 0 end) dr,
sum(case when returntype>10 then total else 0 end) cr from lrinvoicereturn
where lrinvoicereturn.isactive = true and returndate<@fromdate
and suppliercompanyid = @suppliercompanyid and companyid=@companyid
and lrinvoicereturn.locationid=@locationid;

select sum(Jobworkbillgrn.totalamount) totalamount
from Jobworkbillgrn
where Jobworkbillgrn.entrydate<@fromdate
and isactive=true and suppliercompanyid = @suppliercompanyid 
and companyid=@companyid;

select lrinvoice.entrydate,lrinvoice.invoiceno,lrinvoice.invoicedate,
(lrinvoice.totalamount-coalesce(lrinvoice.presettled,0)) totalamount,
lrinvoice.createdby,lrinvoice.modifiedby,lrinvoice.createdon,lrinvoice.modifiedon,tdsamount from lrinvoice 
left outer join lrentry on lrentry.id = lrid
where lrinvoice.isactive = true and lrinvoice.settled=false and entrydate>=@fromdate and entrydate<@todate
and suppliercompanyid = @suppliercompanyid and companyid=@companyid
and lrentry.entrylocationid=@locationid;

with invoicevalue as 
(
    select Supplierpaymentid paymentid,sum(Supplierpaymentinvoice.paidamount) paidamount,
    sum(Supplierpaymentinvoice.discountamount) discountamount ,
    sum(Supplierpaymentinvoice.agentcommissionvalue) agentcommissionvalue 
    from Supplierpayment 
    inner join Supplierpaymentinvoice on Supplierpaymentinvoice.Supplierpaymentid = Supplierpayment.id
    where Supplierpayment.isactive = true and Supplierpaymentinvoice.isactive = true and supplierpaymentinvoice.hascancel=false and islrinvoicereturn = false
    and paymentdate>=@fromdate and paymentdate<@todate and supplierpayment.hascancel=false 
    and suppliercompanyid = @suppliercompanyid and companyid=@companyid
    and supplierpayment.locationid=@locationid
    group by Supplierpaymentid
),
debitnotevalue as (
    select Supplierpaymentid paymentid,sum(Supplierpaymentinvoice.paidamount) paidamount,sum(Supplierpaymentinvoice.discountamount) discountamount 
    from Supplierpayment inner join Supplierpaymentinvoice on Supplierpaymentinvoice.Supplierpaymentid = Supplierpayment.id
    where Supplierpayment.isactive = true and Supplierpaymentinvoice.isactive = true and supplierpaymentinvoice.hascancel=false 
    and (islrinvoicereturn = true or iscreditnote=true)
    and paymentdate>=@fromdate and paymentdate<@todate and supplierpayment.hascancel=false 
    and suppliercompanyid = @suppliercompanyid and companyid=@companyid
    and supplierpayment.locationid=@locationid
    group by Supplierpaymentid
)

select paymentdate,supplierpayment.paidamount,
debitnotevalue.paidamount dnpaidamount,
debitnotevalue.discountamount dndiscount,
invoicevalue.discountamount invdiscount,
(case when tdsoninv = false then supplierpayment.tdsamount else 0 end) as tdsamount,
invoicevalue.agentcommissionvalue agentcommissionvalue,
receiptno,supplierpayment.id,bank.name bankname,
supplierpayment.paymentmode,instrumentdate chequedate,referencedetail,
supplierpayment.createdby,supplierpayment.modifiedby,
supplierpayment.createdon,supplierpayment.modifiedon
from supplierpayment
left outer join invoicevalue on invoicevalue.paymentid = supplierpayment.id
left outer join debitnotevalue on debitnotevalue.paymentid = supplierpayment.id
left outer join referencelist bank on bank.id = bankid
where supplierpayment.isactive = true and paymentdate>=@fromdate and paymentdate<@todate
and suppliercompanyid = @suppliercompanyid and companyid=@companyid  and supplierpayment.hascancel=false
and supplierpayment.locationid=@locationid;

select supplierpaymentid id,STRING_AGG(invoiceno,',') invoiceno from supplierpaymentinvoice
inner join supplierpayment on supplierpayment.id = supplierpaymentinvoice.supplierpaymentid
where supplierpaymentinvoice.isactive = true and supplierpaymentinvoice.hascancel=false 
and supplierpayment.isactive = true and supplierpayment.paymentdate>=@fromdate 
and supplierpayment.paymentdate<@todate  and supplierpayment.hascancel=false 
and supplierpayment.suppliercompanyid = @suppliercompanyid and supplierpayment.companyid=@companyid
and supplierpayment.locationid=@locationid
group by supplierpaymentinvoice.supplierpaymentid;

select total,returnno,returndate,qty,lrinvoicereturn.id,returntype,
lrinvoicereturn.createdby,lrinvoicereturn.modifiedby,
lrinvoicereturn.createdon,lrinvoicereturn.modifiedon
from lrinvoicereturn 
where lrinvoicereturn.isactive = true and returndate>=@fromdate and returndate<@todate
and lrinvoicereturn.suppliercompanyid = @suppliercompanyid and lrinvoicereturn.companyid=@companyid
and lrinvoicereturn.locationid=@locationid;

select lrinvoicereturnitems.lrinvoicereturnid id,
STRING_AGG((case when lrinvoice.invoiceno is null then 'Direct' else lrinvoice.invoiceno end),',') invoicenos 
from lrinvoicereturnitems
left outer join lrinvoicereturn on lrinvoicereturn.id = lrinvoicereturnid
left outer join lrinvoiceitems on lrinvoiceitems.id = lrinvoiceitemsid
left outer join lrinvoice on lrinvoice.id = lrinvoiceitems.lrinvoiceid
where lrinvoicereturnitems.isactive =true and lrinvoicereturn.isactive = true and lrinvoicereturn.returndate>=@fromdate
and lrinvoicereturn.suppliercompanyid = @suppliercompanyid and lrinvoicereturn.companyid=@companyid
and lrinvoicereturn.locationid=@locationid
group by lrinvoicereturnitems.lrinvoicereturnid;

select Jobworkbillgrn.totalamount,Jobworkbillgrn.invoiceno,id,createdby,createdon,modifiedby,modifiedon,
Jobworkbillgrn.invoicedate,Jobworkbillgrn.entrydate,Jobworkbillgrn.entryno
from Jobworkbillgrn
where Jobworkbillgrn.entrydate>=@fromdate and Jobworkbillgrn.entrydate<@todate
and isactive=true and suppliercompanyid = @suppliercompanyid 
and companyid=@companyid
";


            var param = new SortedList<string, object>();
            param.Add("suppliercompanyid", suppliercompany[0].id);
            param.Add("companyid", companyid);
            param.Add("fromdate", fromdate.Date);
            param.Add("todate", todate.Date.AddDays(1));
            if (location <= 0)
            {
                query = query.Replace("and lrinvoicereturn.locationid=@locationid", "");
                query = query.Replace("and supplierpayment.locationid=@locationid", "");
                query = query.Replace("and lrentry.entrylocationid=@locationid", "");
            }
            else
            {
                param.Add("locationid", location);
            }

            printableLedger.ledgername = suppliercompany[0].name;
            printableLedger.address = suppliercompany[0].address;
            printableLedger.title = fromdate.ToString("dd-MM-yyyy") + " - " + todate.ToString("dd-MM-yyyy");

            CompanyDataService companyDataService = new CompanyDataService() { Context = this.Context };
            var comp = companyDataService.Search(new CompanyCreteria()
            {
                id = companyid
            });

            printableLedger.company = comp.Count > 0 ? comp[0] : new Company();

            using (var reader = ExecuteReader(query, param))
            {
                if (reader.Read() && !reader.IsDBNull(0))
                {
                    pendingamount += !reader.IsDBNull(0) ? reader.GetDecimal(0) : 0;
                    pendingamount -= !reader.IsDBNull(1) ? reader.GetDecimal(1) : 0;
                }
                if (reader.NextResult() && reader.Read())
                {
                    pendingamount -= !reader.IsDBNull(0) ? reader.GetDecimal(0) : 0;
                    //pendingamount -= !reader.IsDBNull(1) ? reader.GetDecimal(1) : 0;
                    //pendingamount += !reader.IsDBNull(2) ? reader.GetDecimal(2) : 0;
                    pendingamount -= !reader.IsDBNull(3) ? reader.GetDecimal(3) : 0;
                    pendingamount -= !reader.IsDBNull(4) ? reader.GetDecimal(4) : 0;
                    pendingamount -= !reader.IsDBNull(5) ? reader.GetDecimal(5) : 0;
                }
                if (reader.NextResult() && reader.Read())
                {
                    if (!reader.IsDBNull(0))
                        pendingamount -= reader.GetDecimal(0);
                    if (!reader.IsDBNull(1))
                        pendingamount += reader.GetDecimal(1);
                }

                if (reader.NextResult() && reader.Read() && !reader.IsDBNull(0))
                {
                    pendingamount += !reader.IsDBNull(0) ? reader.GetDecimal(0) : 0;
                }

                if (pendingamount > 0)
                {
                    printableLedger.obcredit = pendingamount.ToString("F2");
                }
                else
                {
                    printableLedger.obdebit = (pendingamount * -1).ToString("F2");
                }


                if (reader.NextResult())
                {
                    while (reader.Read())
                    {
                        printableLedger.transactions.Add(new PrintableLedger.Transaction()
                        {
                            createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]),
                            modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]),
                            modifiedon = reader["modifiedon"] == DBNull.Value ? Convert.ToDateTime(reader["entrydate"]) : Convert.ToDateTime(reader["modifiedon"]),
                            createdon = reader["createdon"] == DBNull.Value ? Convert.ToDateTime(reader["entrydate"]) : Convert.ToDateTime(reader["createdon"]),
                            type = "Purchase",
                            date = Convert.ToDateTime(reader["entrydate"]),
                            description = String.Format("Inv No :{0}, Inv Date : {1}", reader["invoiceno"].ToString()
                            , Convert.ToDateTime(reader["invoicedate"]).ToString("dd-MM-yyyy")),
                            credit = Convert.ToDecimal(reader["totalamount"]).ToString("F2"),
                            detail = new VoucherDisplayItem()
                            {
                                invoicedate = Convert.ToDateTime(reader["invoicedate"]),
                                invoiceno = reader["invoiceno"].ToString()
                            }
                        });
                    }
                }
                if (reader.NextResult())
                {
                    while (reader.Read())
                    {
                        var paymentmode = reader["paymentmode"] == DBNull.Value ? -1 : Convert.ToInt32(reader["id"]);
                        var chequedate = reader["chequedate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["chequedate"]);
                        var referencedetail = reader["referencedetail"] == DBNull.Value ? "" : Convert.ToString(reader["referencedetail"]);
                        var chequeinfo = "";
                        if (paymentmode != 2)
                        {
                            var bankname = reader["bankname"] == DBNull.Value ? "" : Convert.ToString(reader["bankname"]);
                            switch (paymentmode)
                            {
                                case 0:
                                    chequeinfo = bankname + " NEFT/RTGS ";
                                    break;
                                case 1:
                                    chequeinfo = bankname + " Cheque/DD ";
                                    break;
                            }
                            chequeinfo += " Chq No :" + referencedetail;
                            chequeinfo += " Chq Date :" + chequedate.ToString("dd-MM-yyyy");
                        }

                        printableLedger.transactions.Add(new PrintableLedger.Transaction()
                        {
                            type = "Payment",
                            createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]),
                            modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]),
                            modifiedon = reader["modifiedon"] == DBNull.Value ? Convert.ToDateTime(reader["paymentdate"]) : Convert.ToDateTime(reader["modifiedon"]),
                            createdon = reader["createdon"] == DBNull.Value ? Convert.ToDateTime(reader["paymentdate"]) : Convert.ToDateTime(reader["createdon"]),
                            date = Convert.ToDateTime(reader["paymentdate"]),
                            id = Convert.ToInt64(reader["id"]),
                            description = "Ref No :" + reader["receiptno"].ToString() + " " + chequeinfo,
                            debit = Convert.ToDecimal(reader["paidamount"]).ToString("F2"),
                            detail = new VoucherDisplayItem()
                            {
                                bankdate = chequedate,
                                bankreference = paymentmode == 0 ? ("NEFT/RTGS :" + referencedetail) : ""
                            }
                        });

                        if (reader["invdiscount"] != DBNull.Value &&
                            Convert.ToDecimal(reader["invdiscount"]) > 0)
                        {
                            printableLedger.transactions.Add(new PrintableLedger.Transaction()
                            {
                                createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]),
                                modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]),
                                modifiedon = reader["modifiedon"] == DBNull.Value ? Convert.ToDateTime(reader["paymentdate"]) : Convert.ToDateTime(reader["modifiedon"]),
                                createdon = reader["createdon"] == DBNull.Value ? Convert.ToDateTime(reader["paymentdate"]) : Convert.ToDateTime(reader["createdon"]),
                                type = "Discount",
                                date = Convert.ToDateTime(reader["paymentdate"]),
                                id = Convert.ToInt64(reader["id"]),
                                description = reader["receiptno"].ToString(),
                                debit = Convert.ToDecimal(reader["invdiscount"]).ToString("F2")
                            });
                        }


                        if (reader["agentcommissionvalue"] != DBNull.Value &&
                            Convert.ToDecimal(reader["agentcommissionvalue"]) > 0)
                        {
                            printableLedger.transactions.Add(new PrintableLedger.Transaction()
                            {
                                createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]),
                                modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]),
                                modifiedon = reader["modifiedon"] == DBNull.Value ? Convert.ToDateTime(reader["paymentdate"]) : Convert.ToDateTime(reader["modifiedon"]),
                                createdon = reader["createdon"] == DBNull.Value ? Convert.ToDateTime(reader["paymentdate"]) : Convert.ToDateTime(reader["createdon"]),
                                type = "Agent Commission",
                                date = Convert.ToDateTime(reader["paymentdate"]),
                                id = Convert.ToInt64(reader["id"]),
                                description = reader["receiptno"].ToString(),
                                debit = Convert.ToDecimal(reader["agentcommissionvalue"]).ToString("F2")
                            });
                        }

                        if (reader["tdsamount"] != DBNull.Value &&
                        Convert.ToDecimal(reader["tdsamount"]) > 0)
                        {
                            printableLedger.transactions.Add(new PrintableLedger.Transaction()
                            {
                                createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]),
                                modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]),
                                modifiedon = reader["modifiedon"] == DBNull.Value ? Convert.ToDateTime(reader["paymentdate"]) : Convert.ToDateTime(reader["modifiedon"]),
                                createdon = reader["createdon"] == DBNull.Value ? Convert.ToDateTime(reader["paymentdate"]) : Convert.ToDateTime(reader["createdon"]),
                                type = "TDS On Payment",
                                date = Convert.ToDateTime(reader["paymentdate"]),
                                id = Convert.ToInt64(reader["id"]),
                                description = reader["receiptno"].ToString(),
                                debit = Convert.ToDecimal(reader["tdsamount"]).ToString("F2")
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
                        //if (titem != null) titem.description = titem.description + "(" + reader["invoiceno"].ToString() + ")";
                    }
                }

                if (reader.NextResult())
                {
                    while (reader.Read())
                    {
                        int returntype = Convert.ToInt32(reader["returntype"]);
                        if (returntype < 10)
                        {
                            printableLedger.transactions.Add(new PrintableLedger.Transaction()
                            {
                                createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]),
                                modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]),
                                modifiedon = reader["modifiedon"] == DBNull.Value ? Convert.ToDateTime(reader["returndate"]) : Convert.ToDateTime(reader["modifiedon"]),
                                createdon = reader["createdon"] == DBNull.Value ? Convert.ToDateTime(reader["returndate"]) : Convert.ToDateTime(reader["createdon"]),
                                type = returntype == 7 ? "Advance" : "CR/DR Note",
                                date = Convert.ToDateTime(reader["returndate"]),
                                id = Convert.ToInt64(reader["id"]),
                                description = reader["returnno"].ToString(),
                                debit = Convert.ToDecimal(reader["total"]).ToString("F2")
                            });
                        }
                        else
                        {
                            printableLedger.transactions.Add(new PrintableLedger.Transaction()
                            {
                                createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]),
                                modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]),
                                modifiedon = reader["modifiedon"] == DBNull.Value ? Convert.ToDateTime(reader["returndate"]) : Convert.ToDateTime(reader["modifiedon"]),
                                createdon = reader["createdon"] == DBNull.Value ? Convert.ToDateTime(reader["returndate"]) : Convert.ToDateTime(reader["createdon"]),
                                type = "Excess Note",
                                date = Convert.ToDateTime(reader["returndate"]),
                                id = Convert.ToInt64(reader["id"]),
                                description = reader["returnno"].ToString(),
                                credit = Convert.ToDecimal(reader["total"]).ToString("F2")
                            });
                        }
                    }
                }

                if (reader.NextResult())
                {
                    while (reader.Read())
                    {
                        var id = Convert.ToInt64(reader["id"]);
                        var titem = printableLedger.transactions.Find(x => x.id == id && x.type == "CR/DR Note");
                        if (titem != null) titem.description = titem.description + "(" + reader["invoicenos"].ToString() + ")";
                    }
                }

                if (reader.NextResult())
                {
                    while (reader.Read())
                    {
                        printableLedger.transactions.Add(new PrintableLedger.Transaction()
                        {
                            createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]),
                            modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]),
                            modifiedon = reader["modifiedon"] == DBNull.Value ? Convert.ToDateTime(reader["entrydate"]) : Convert.ToDateTime(reader["modifiedon"]),
                            createdon = reader["createdon"] == DBNull.Value ? Convert.ToDateTime(reader["entrydate"]) : Convert.ToDateTime(reader["createdon"]),
                            type = "JOB Work",
                            date = Convert.ToDateTime(reader["entrydate"]),
                            id = Convert.ToInt64(reader["id"]),
                            description = reader["entryno"].ToString() + " Inv No : " + reader["invoiceno"] +
                            "(" + Convert.ToDateTime(reader["invoicedate"]).ToString("dd-MM-yyyy") + ")",
                            credit = Convert.ToDecimal(reader["totalamount"]).ToString("F2")
                        });
                    }

                }
            }

            var tdsrequest = new FinanceAnalyseCreteria()
            {
                locations = new List<long>(),
                fromdate = fromdate,
                todate = todate,
                ledgerid = supplierid
            };
            if (location > 0)
                tdsrequest.locations.Add(location);
            var invtdslist = voucherDataService.TDSInventoryReport(tdsrequest, false);
            foreach (var invtds in invtdslist)
            {
                printableLedger.transactions.Add(new PrintableLedger.Transaction()
                {
                    type = "TDS",
                    date = invtds.date,
                    id = invtds.id,
                    createdby = invtds.createdby,
                    modifiedby = invtds.modifiedby,
                    modifiedon = invtds.modifiedon,
                    createdon = invtds.createdon,
                    description = invtds.tdsledgername + "@" + invtds.tdspercentage.ToString("F2") + " - " + invtds.no,
                    debit = invtds.tds.ToString("F2")
                });
            }
            printableLedger.transactions = printableLedger.transactions.OrderBy(x => x.date).ToList();
            var emplist = new List<long>();
            emplist.AddRange(printableLedger.transactions.ConvertAll(x => x.createdby));
            emplist.AddRange(printableLedger.transactions.ConvertAll(x => x.modifiedby));
            EmployeeDataService employeeDataService = new EmployeeDataService() { Context = this.Context };
            var empnames = employeeDataService.GetUserName(emplist.Distinct().ToList());
            printableLedger.transactions.ForEach(x =>
            {
                x.cuser = empnames.Exists(y => y.id == x.createdby) ? empnames.Find(y => y.id == x.createdby).name : "";
                x.muser = empnames.Exists(y => y.id == x.modifiedby) ? empnames.Find(y => y.id == x.modifiedby).name : "";
                x.muser = x.modifiedon.ToString("dd-MMM-yyyy") + " @ " + x.muser;
                x.cuser = x.createdon.ToString("dd-MMM-yyyy") + " @ " + x.cuser;
            });
            printableLedger.ProcessTotal();
            return printableLedger;
        }

        internal List<SupplierpaymentReport> SearchCommissionSummary(SupplierpaymentCreteria item)
        {
            var lst = SearchCommmission(item);
            return lst.GroupBy(x => new { x.agentname, x.suppliername }).Select(x => new SupplierpaymentReport
            {
                agentcommissionvalue = x.Sum(y => y.agentcommissionvalue),
                agentname = x.Key.agentname,
                suppliername = x.Key.suppliername,
            }).ToList();
        }

        internal List<SupplierpaymentReport> SearchCommmission(SupplierpaymentCreteria _supplierpayment)
        {
            string query = @"select 
            supplierpayment.paymentdate,
            supplierpayment.receiptno,
            agent.gst gstno,
            agent.name agentname,
            STRING_AGG(supplierpaymentinvoice.invoiceno,',') invoiceno,
            suppliercompany.name suppliername,
            sum(supplierpaymentinvoice.paidamount) paidamount,
            sum(supplierpaymentinvoice.discountamount) discountamount,
            sum(agentcommissionvalue) agentcommissionvalue
            from supplierpaymentinvoice
            inner join supplierpayment on supplierpayment.id = supplierpaymentid
            inner join suppliercompany on suppliercompany.id = supplierpayment.suppliercompanyid
            inner join agent on agent.id = supplierpaymentinvoice.agentid
            where supplierpaymentinvoice.isactive=true and supplierpaymentinvoice.hascancel=false 
            and supplierpayment.isactive=true and supplierpayment.hascancel=false 
            and agentcommissionvalue!=0 {0}
            group by supplierpayment.paymentdate,
            supplierpayment.receiptno,
            agent.gst,
            agent.name ,
            suppliercompany.name";

            List<string> conditions = new List<string>();
            if (_supplierpayment.supplierid > 0)
            {
                conditions.Add(String.Format("{0}{2}{1}", "Supplierpayment.supplierid", _supplierpayment.supplierid, "="));
            }

            if (_supplierpayment.locationid > 0)
            {
                conditions.Add(String.Format("{0}{2}{1}", "Supplierpayment.locationid", _supplierpayment.locationid, "="));
            }


            if (_supplierpayment.agentid > 0)
            {
                conditions.Add(String.Format("{0}{2}{1}", "supplierpaymentinvoice.agentid", _supplierpayment.agentid, "="));
            }

            if (_supplierpayment.companyid > 0)
            {
                conditions.Add(String.Format("{0}{2}{1}", "Supplierpayment.companyid", _supplierpayment.companyid, "="));
            }

            if (!string.IsNullOrEmpty(_supplierpayment.receiptno))
            {
                conditions.Add(String.Format("{0}{2}'{1}'", "Supplierpayment.receiptno", Utility.SafeSQL(_supplierpayment.receiptno), "="));
            }

            if (!string.IsNullOrEmpty(_supplierpayment.referencedetail))
            {
                conditions.Add(String.Format("{0}{2}'{1}'", "Supplierpayment.receiptno", Utility.SafeSQL(_supplierpayment.referencedetail), "="));
            }

            if (_supplierpayment.fromdate != DateTime.MinValue && _supplierpayment.todate != DateTime.MinValue)
            {
                conditions.Add(String.Format("{0}{2}'{1}'", "Supplierpayment.paymentdate", _supplierpayment.fromdate.Date.ToString("yyyy-MM-dd"), ">="));
                conditions.Add(String.Format("{0}{2}'{1}'", "Supplierpayment.paymentdate", _supplierpayment.todate.AddDays(1).Date.ToString("yyyy-MM-dd"), "<"));
            }

            QueryBuilder qb = new QueryBuilder();
            qb.CurrentQuery = query;
            if (conditions.Count > 0)
                qb.CurrentQuery = String.Format(qb.CurrentQuery, " and " + String.Join(" and ", conditions));
            else
                qb.CurrentQuery = String.Format(qb.CurrentQuery, "");
            List<SupplierpaymentReport> lst = new List<SupplierpaymentReport>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                var command = qb.GetCommand(conn);
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        SupplierpaymentReport osupplierpayment = new SupplierpaymentReport();
                        osupplierpayment.receiptno = reader["receiptno"] == DBNull.Value ? "" : reader["receiptno"].ToString();
                        osupplierpayment.paymentdate = reader["paymentdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["paymentdate"]);
                        osupplierpayment.paidamount = reader["paidamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["paidamount"]);
                        osupplierpayment.discountamount = reader["discountamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discountamount"]);
                        osupplierpayment.agentcommissionvalue = reader["agentcommissionvalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["agentcommissionvalue"]);
                        osupplierpayment.agentname = reader["agentname"] == DBNull.Value ? "" : reader["agentname"].ToString();
                        osupplierpayment.suppliername = reader["suppliername"] == DBNull.Value ? "" : reader["suppliername"].ToString();
                        osupplierpayment.invoiceno = reader["invoiceno"] == DBNull.Value ? "" : reader["invoiceno"].ToString();
                        lst.Add(osupplierpayment);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null && conn.State != System.Data.ConnectionState.Closed)
                    conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _supplierpayment);
            }
            return lst;
        }
    }

}
