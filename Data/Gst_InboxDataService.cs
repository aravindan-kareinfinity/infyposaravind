using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
using Newtonsoft.Json;
using InfyPOS.Models.GST;
using System.Data;
using InfyPOS.Services;

namespace InfyPOS.Data
{
    public class Gst_InboxDataService : BaseData
    {
        const string GSTR2B_Filed = @"select gstinboxid,0 difftax,null as filedmonth,null as fileddate,suppliercompany.name suppliername,tinanddate as gstno,invoiceno,
invoicedate,lrinvoice.isinterstatesale,amounton as taxablevalue,
lrinvoicetax.percentage taxpercentage,lrinvoicetax.taxamount tax,lrinvoice.id lrinvoiceid from lrinvoice
inner join lrinvoicetax on lrinvoicetax.invoiceid = lrinvoice.id and lrinvoicetax.isactive=true
inner join suppliercompany on suppliercompany.id = lrinvoice.suppliercompanyid
where invoicedate>=@gstfromdate and  invoicedate<@gsttodate and lrinvoice.isactive = true and lrinvoice.companyid=@companyid
and gstinboxid != 0 and lrinvoicetax.type<2";

        const string GSTR2B_NoFiled = @"select 0 gstinboxid,0 difftax,null as filedmonth,null as fileddate,suppliercompany.name suppliername,tinanddate as gstno,invoiceno,
invoicedate,lrinvoice.isinterstatesale,amounton as taxablevalue,
lrinvoicetax.percentage taxpercentage,lrinvoicetax.taxamount tax,lrinvoice.id lrinvoiceid from lrinvoice
inner join lrinvoicetax on lrinvoicetax.invoiceid = lrinvoice.id and lrinvoicetax.isactive=true
inner join suppliercompany on suppliercompany.id = lrinvoice.suppliercompanyid
where invoicedate>=@gstfromdate and  invoicedate<@gsttodate and lrinvoice.isactive = true and lrinvoice.companyid=@companyid
and gstinboxid = 0 and lrinvoicetax.type<2";

        const string GSTR1_Filed = @"with GSTR1 as 
(
select Gst_Inbox.id gstinboxid,
	Gst_Inbox.companyid,
b2blist->>'ctin' ctin,
b2blist->>'flprdr1' flprdr1,
b2blist->>'cfs3b' cfs3b, 
b2blist->>'cfs' cfs,
b2blist->>'dtcancel' dtcancel,
b2blist->>'fldtr1' fldtr1,
b2blist->>'cfs' cfs,
invlist->>'aspd' aspd,
invlist->>'astyp' astyp,
invlist->>'inv_typ' inv_typ,
invlist->>'rchrg' rchrg,
TO_DATE(invlist->>'idt','DD-MM-YYYY') idt,
invlist->>'pos' pos,
invlist->>'inum' inum,
invlist->>'cfs' cfs,
invlist->>'val' val,
taxlist->>'num' num,
cast(taxlist#>>'{itm_det,rt}' as decimal) rt,
cast(taxlist#>>'{itm_det,csamt}' as decimal) csamt,
cast(taxlist#>>'{itm_det,samt}' as decimal) samt,
cast(taxlist#>>'{itm_det,txval}' as decimal) txval,
cast(taxlist#>>'{itm_det,iamt}' as decimal) iamt,
cast(taxlist#>>'{itm_det,camt}' as decimal) camt
from Gst_Inbox
LEFT JOIN LATERAL
json_array_elements (gstadata#>'{b2b}') b2blist ON true
LEFT JOIN LATERAL
json_array_elements (b2blist->'inv') invlist ON true
LEFT JOIN LATERAL
json_array_elements (invlist->'itms') taxlist ON true
where companyid=@companyid and fromdate>=@gstfromdate and fromdate<@gsttodate and gsttype ilike 'GSTR1A-%'
), Filed as (
select ctin gstno,idt billdate,inum billno,pos,companyid,
	case when iamt is not null and iamt>0 then true else false end isinterstatesale,
	sum(cast(val as numeric)) receivable,sum(cast(num as numeric)) qty,sum(txval) taxablevalue,
	sum(cast(val as decimal)-txval) tax,sum(csamt) cessamt,
	sum(samt) sgst,
	sum(camt) cgst,
	sum(iamt) igst from GSTR1
group by ctin,idt,inum,pos,companyid,case when iamt is not null and iamt>0 then true else false end)

select bill.billtype,left(customer.gstno,2) pos, bill.companyid,customer.name customer, bill.billno,
bill.billdate,bill.qty,bill.isinterstatesale,bill.tax,customer.gstno,
bill.receivable,Filed.tax filedtax,filed.receivable filedreceivable from bill
left outer join Filed on Filed.billno = bill.billno
inner join customer on customer.id = bill.customerid
where bill.companyid = @companyid and bill.billdate>=@gstfromdate and bill.billdate<@gsttodate 
and customer.gstno != '' and bill.billtype != 9";

        /*select 
        cast(voucher.attributes->>'invoiceno' as varchar) invoiceno,
        cast(voucher.attributes->>'invoicedate' as varchar) invoicedate,
        sum(case when taxmode=0 then voucheritems.amount else 0 end) basevalue,
        sum(case when taxmode=3 then voucheritems.amount else 0 end) igst,
        sum(case when taxmode=4 then voucheritems.amount else 0 end) sgst,
        sum(case when taxmode=5 then voucheritems.amount else 0 end) tgst from voucher
        inner join voucheritems on voucheritems.voucherid = voucher.id
        inner join ledger on ledger.id = voucheritems.ledgerid
        where vouchertype in (1,3,6)
        and voucher.id = 9758
        and cast(voucher.attributes->>'hastds' as bool)=true
        and (taxmode in (3,4,5,0))
        group by voucher.id,cast(voucher.attributes->>'invoiceno' as varchar),
        cast(voucher.attributes->>'invoicedate' as varchar),
        ledger.taxpercentage*/



        const string GSTR2B_REconcilation_MView = @"
        CREATE MATERIALIZED VIEW IF NOT EXISTS GSTINBOX_GSTR2B_{0}
        AS
        select Gst_Inbox.id gstinboxid,
        b2blist->>'ctin' ctin,
        b2blist->>'flprdr1' flprdr1,
        b2blist->>'cfs3b' cfs3b, 
        b2blist->>'cfs' cfs,
        suppliercompany.name trdnm,
        b2blist->>'dtcancel' dtcancel,
        b2blist->>'fldtr1' fldtr1,
        invlist->>'aspd' aspd,
        invlist->>'astyp' astyp,
        invlist->>'inv_typ' inv_typ,
        invlist->>'rchrg' rchrg,
        TO_DATE(invlist->>'idt','DD-MM-YYYY') idt,
        invlist->>'pos' pos,
        invlist->>'inum' inum,
        invlist->>'cfs' invcfs,
        invlist->>'val' val,
        taxlist->>'num' num,
        cast(taxlist#>>'{itm_det,rt}' as decimal) rt,
        cast(taxlist#>>'{itm_det,csamt}' as decimal) csamt,
        cast(taxlist#>>'{itm_det,samt}' as decimal) samt,
        cast(taxlist#>>'{itm_det,txval}' as decimal) txval,
        cast(taxlist#>>'{itm_det,iamt}' as decimal) iamt,
        cast(taxlist#>>'{itm_det,camt}' as decimal) camt
        from Gst_Inbox
        LEFT JOIN LATERAL
        json_array_elements (gstadata#>'{b2b}') b2blist ON true
        LEFT JOIN LATERAL
        json_array_elements (b2blist->'inv') invlist ON true
        LEFT JOIN LATERAL
        json_array_elements (invlist->'itms') taxlist ON true
        left outer join uniquesuppliercompanyforGSTInbox suppliercompany on suppliercompany.tinanddate = b2blist->>'ctin' and suppliercompany.ranknumber=1
        where Gst_Inbox.id = {0}";

        const string GSTR2A_REconcilation_MView = @"
        CREATE MATERIALIZED VIEW IF NOT EXISTS GSTINBOX_GSTR2A_{0}
        AS
        select Gst_Inbox.id gstinboxid,
        b2blist->>'ctin' ctin,
        b2blist->>'flprdr1' flprdr1,
        b2blist->>'cfs3b' cfs3b, 
        b2blist->>'cfs' cfs,
        suppliercompany.name trdnm,
        b2blist->>'dtcancel' dtcancel,
        b2blist->>'fldtr1' fldtr1,
        invlist->>'aspd' aspd,
        invlist->>'astyp' astyp,
        invlist->>'inv_typ' inv_typ,
        invlist->>'rchrg' rchrg,
        TO_DATE(invlist->>'idt','DD-MM-YYYY') idt,
        invlist->>'pos' pos,
        invlist->>'inum' inum,
        invlist->>'cfs' invcfs,
        invlist->>'val' val,
        taxlist->>'num' num,
        cast(taxlist#>>'{itm_det,rt}' as decimal) rt,
        cast(taxlist#>>'{itm_det,csamt}' as decimal) csamt,
        cast(taxlist#>>'{itm_det,samt}' as decimal) samt,
        cast(taxlist#>>'{itm_det,txval}' as decimal) txval,
        cast(taxlist#>>'{itm_det,iamt}' as decimal) iamt,
        cast(taxlist#>>'{itm_det,camt}' as decimal) camt
        from Gst_Inbox
        LEFT JOIN LATERAL
        json_array_elements (gstadata#>'{b2b}') b2blist ON true
        LEFT JOIN LATERAL
        json_array_elements (b2blist->'inv') invlist ON true
        LEFT JOIN LATERAL
        json_array_elements (invlist->'itms') taxlist ON true
        left outer join uniquesuppliercompanyforGSTInbox suppliercompany on suppliercompany.tinanddate = b2blist->>'ctin' and suppliercompany.ranknumber=1
        where Gst_Inbox.id = {0}";


        const string GSTR2B_REconcilation = @"with gstr2b_b2b
as
(
    select * from GSTINBOX_GSTR2B_{0}
),
InventoryPurchase as (
	 select'PURCHASE' trtype, suppliercompany.name,tinanddate as gstno,invoiceno,invoicedate,baseamount,grossamount,totalamount,lrinvoice.isinterstatesale,
amounton,netamount,lrinvoicetax.percentage,lrinvoicetax.taxamount,lrinvoicetax.type,lrinvoice.id lrinvoiceid,invoiceentryno entryno,entrydate from lrinvoice
inner join lrinvoicetax on lrinvoicetax.invoiceid = lrinvoice.id and lrinvoicetax.isactive=true
inner join suppliercompany on suppliercompany.id = lrinvoice.suppliercompanyid
where invoicedate>=@invfromdate and invoicedate<@invtodate 
and lrinvoice.isactive = true and lrinvoice.companyid=@companyid),
 QuantoInvoice
as
(

select trtype, name,gstno,invoiceno,invoicedate,baseamount,grossamount,totalamount,isinterstatesale,
	sum(amounton) amounton,sum(netamount) netamount,percentage,sum(taxamount) taxamount,type,lrinvoiceid,entryno,entrydate
	from InventoryPurchase
    group by trtype, name,gstno,invoiceno,invoicedate,baseamount,grossamount,totalamount,isinterstatesale,
	percentage,type,lrinvoiceid,entryno,entrydate

union all

select 'PURCHASE RETURN' trtype,suppliercompany.name,tinanddate as gstno,
lrinvoicereturn.returnno invoiceno,
lrinvoicereturn.returndate invoicedate,baseamount,grossamount,
grossamount totalamount,lrinvoicereturn.isinterstatesale,
grossamount baseamount, grossamount netamount,taxpercentage,
taxamount,0 ""type"",lrinvoicereturn.id lrinvoiceid,lrinvoicereturn.returnno entryno,
lrinvoicereturn.returndate entrydate
from lrinvoicereturn
inner join (
select taxpercentage,sum(tax) taxamount,sum(tax+amount) grossamount,sum(amount) baseamount,lrinvoicereturnid 
	from lrinvoicereturnitems
group by lrinvoicereturnid,taxpercentage) as returnitems on returnitems.lrinvoicereturnid = lrinvoicereturn.id
inner join suppliercompany on suppliercompany.id = lrinvoicereturn.suppliercompanyid
where returndate>=@invfromdate and returndate<@invtodate 
and lrinvoicereturn.isactive = true and lrinvoicereturn.companyid=@companyid

UNION ALL

select 'VOUCHER' trrtype,vledger.displayname supplier,vledger.gstno,
cast(voucher.attributes->>'invoiceno' as varchar) invoiceno,
cast(voucher.attributes->>'invoicedate' as timestamp) invoicedate,

sum(case when ledger.taxmode=0 then voucheritems.amount else 0 end) baseamount,
sum(case when ledger.taxmode>=0 and ledger.taxmode<6 then voucheritems.amount else 0 end) grossamount,
sum(case when ledger.taxmode>=0 and ledger.taxmode<6 then voucheritems.amount else 0 end) totalamount,
vledger.isinterstate,
sum(case when ledger.taxmode=0 then voucheritems.amount else 0 end) amounton,
sum(case when ledger.taxmode>=0 and ledger.taxmode<6 then voucheritems.amount else 0 end) netamount,
sum(ledger.taxpercentage) percentage,
sum(case when ledger.taxmode>2 and ledger.taxmode<6 then voucheritems.amount else 0 end) taxamount,
0 ""type"",voucher.id,voucherno entryno,voucherdate entrydate from voucher
inner join voucheritems on voucheritems.voucherid = voucher.id
inner join ledger vledger on vledger.id = voucher.adjustmentledgerid
inner join ledger on ledger.id = voucheritems.ledgerid
where vouchertype = 3 
and voucher.isactive = true and voucher.companyid=@companyid
and (ledger.taxmode in (3,4,5,0))
--and cast(voucher.attributes->>'hasgst' as bool)=true and voucherdate>=@invfromdate and voucherdate<@invtodate 
group by voucher.id,cast(voucher.attributes->>'invoiceno' as varchar),
cast(voucher.attributes->>'invoicedate' as timestamp),vledger.displayname,
vledger.gstno,vledger.isinterstate,voucherno,voucherdate

), firstmatch as
(
    select gstinboxid,flprdr1 filedmonth,fldtr1 fileddate,ctin gstno,trdnm suppliername,val totalamount,inum invoiceno,idt invoicedate,
    rt taxpercentage,txval taxablevalue,
    case when iamt>0 then iamt else camt+samt end as tax,
    QuantoInvoice.lrinvoiceid,QuantoInvoice.isinterstatesale,QuantoInvoice.percentage,QuantoInvoice.taxamount,
    (case when iamt>0 then iamt else camt+samt end)-QuantoInvoice.taxamount difftax,
    gstr2b_b2b.inum ptinvoiceno,QuantoInvoice.invoiceno qtinvoiceno,QuantoInvoice.entrydate,QuantoInvoice.entryno,
    case when iamt>0 then 'I' else 'L' end taxfrm,rchrg
    from gstr2b_b2b
    left outer join QuantoInvoice on QuantoInvoice.gstno = ctin
    and (QuantoInvoice.invoiceno = inum and (QuantoInvoice.percentage = rt)) and type <2
),
secondmatched as
(
    select firstmatch.gstinboxid,filedmonth,fileddate,firstmatch.gstno,firstmatch.suppliername,firstmatch.totalamount,
	case when QuantoInvoice.invoiceno is null then 
	firstmatch.invoiceno
	else firstmatch.invoiceno||'->'||QuantoInvoice.invoiceno end invoiceno,firstmatch.invoicedate,firstmatch.taxpercentage,firstmatch.taxablevalue,firstmatch.tax,
	QuantoInvoice.lrinvoiceid,QuantoInvoice.isinterstatesale,QuantoInvoice.percentage,QuantoInvoice.taxamount,
    (firstmatch.tax)-QuantoInvoice.taxamount difftax,
    firstmatch.invoiceno ptinvoiceno,QuantoInvoice.invoiceno qtinvoiceno,QuantoInvoice.entrydate,QuantoInvoice.entryno,
    taxfrm,rchrg
	from firstmatch
    left outer join QuantoInvoice on QuantoInvoice.gstno = firstmatch.gstno
    and (QuantoInvoice.percentage = taxpercentage and firstmatch.invoicedate=QuantoInvoice.invoicedate and abs(tax-QuantoInvoice.taxamount)<5) and type <2
    where firstmatch.lrinvoiceid is null
)";



        //        const string GSTR2B_Reconcilation = @"with gstr2b_b2b
        //as
        //(
        //select Gst_Inbox.id gstinboxid,
        //b2blist->>'ctin' ctin,
        //b2blist->>'supfildt' supfildt,
        //b2blist->>'trdnm' trdnm, 
        //b2blist->>'supprd' supprd,
        //invlist->>'rsn' rsn,
        //cast(invlist->>'val' as numeric) val,
        //invlist->>'rev' rev,
        //invlist->>'pos' pos,
        //invlist->>'itcavl' itcavl,
        //invlist->>'inum' inum,
        //invlist->>'diffprcnt' diffprcnt,
        //cast(invlist->>'dt' as date) dt,
        //invlist->>'typ' typ,
        //cast(taxlist->>'rt' as numeric) rt,
        //cast(taxlist->>'txval' as numeric) txval,
        //taxlist->>'num' num,
        //cast(taxlist->>'igst' as numeric) igst,
        //cast(taxlist->>'sgst' as numeric) sgst,
        //cast(taxlist->>'cess' as numeric) cess,
        //cast(taxlist->>'cgst' as numeric) cgst

        //from Gst_Inbox
        //LEFT JOIN LATERAL
        //json_array_elements (gstadata#>'{docdata,b2b}') b2blist ON true
        //LEFT JOIN LATERAL
        //json_array_elements (b2blist->'inv') invlist ON true
        //LEFT JOIN LATERAL
        //json_array_elements (invlist->'items') taxlist ON true
        //where gsttype = 'GSTR2B' and Gst_Inbox.isactive = true
        //and fromdate>=@gstfromdate  and fromdate<@gsttodate and Gst_Inbox.companyid=@companyid
        //),
        // QuantoInvoice
        //as
        //(
        //    select'PURCHASE' trtype, suppliercompany.name,tinanddate as gstno,invoiceno,invoicedate,baseamount,grossamount,totalamount,lrinvoice.isinterstatesale,
        //    amounton,netamount,lrinvoicetax.percentage,lrinvoicetax.taxamount,lrinvoicetax.type,lrinvoice.id lrinvoiceid,invoiceentryno entryno,entrydate from lrinvoice
        //    inner join lrinvoicetax on lrinvoicetax.invoiceid = lrinvoice.id and lrinvoicetax.isactive=true
        //    inner join suppliercompany on suppliercompany.id = lrinvoice.suppliercompanyid
        //    where invoicedate>=@invfromdate and invoicedate<@invtodate 
        //    and lrinvoice.isactive = true and lrinvoice.companyid=@companyid
        //    union all

        //    select 'PURCHASE RETURN' trtype,suppliercompany.name,tinanddate as gstno,
        //    lrinvoicereturn.returnno invoiceno,
        //    lrinvoicereturn.returndate invoicedate,baseamount,grossamount,
        //    grossamount totalamount,lrinvoicereturn.isinterstatesale,
        //    grossamount baseamount, grossamount netamount,taxpercentage,
        //    taxamount,0 ""type"",lrinvoicereturn.id lrinvoiceid,lrinvoicereturn.returnno entryno,
        //    lrinvoicereturn.returndate entrydate
        //    from lrinvoicereturn
        //    inner join (
        //    select taxpercentage,sum(tax) taxamount,sum(tax+amount) grossamount,sum(amount) baseamount,lrinvoicereturnid 
        //	from lrinvoicereturnitems
        //    group by lrinvoicereturnid,taxpercentage) as returnitems on returnitems.lrinvoicereturnid = lrinvoicereturn.id
        //    inner join suppliercompany on suppliercompany.id = lrinvoicereturn.suppliercompanyid
        //    where returndate>=@invfromdate and returndate<@invtodate 
        //    and lrinvoicereturn.isactive = true and lrinvoicereturn.companyid=@companyid

        //    UNION ALL

        //    select 'VOUCHER' trrtype,vledger.displayname supplier,vledger.gstno,
        //    cast(voucher.attributes->>'invoiceno' as varchar) invoiceno,
        //    cast(voucher.attributes->>'invoicedate' as timestamp) invoicedate,

        //    sum(case when ledger.taxmode=0 then voucheritems.amount else 0 end) baseamount,
        //    sum(case when ledger.taxmode>=0 and ledger.taxmode<6 then voucheritems.amount else 0 end) grossamount,
        //    sum(case when ledger.taxmode>=0 and ledger.taxmode<6 then voucheritems.amount else 0 end) totalamount,
        //    vledger.isinterstate,
        //    sum(case when ledger.taxmode=0 then voucheritems.amount else 0 end) amounton,
        //    sum(case when ledger.taxmode>=0 and ledger.taxmode<6 then voucheritems.amount else 0 end) netamount,
        //    max(ledger.taxpercentage) taxpercentage,
        //    sum(case when ledger.taxmode>2 and ledger.taxmode<6 then voucheritems.amount else 0 end) taxamount,
        //    0 ""type"",voucher.id,voucherno entryno,voucherdate entrydate from voucher
        //    inner join voucheritems on voucheritems.voucherid = voucher.id
        //    inner join ledger vledger on vledger.id = voucher.adjustmentledgerid
        //    inner join ledger on ledger.id = voucheritems.ledgerid
        //    where vouchertype = 3 
        //    and voucher.isactive = true and voucher.companyid=@companyid
        //    and (ledger.taxmode in (3,4,5,0))
        //    --and cast(voucher.attributes->>'hasgst' as bool)=true and voucherdate>=@invfromdate and voucherdate<@invtodate 
        //    group by voucher.id,cast(voucher.attributes->>'invoiceno' as varchar),
        //    cast(voucher.attributes->>'invoicedate' as timestamp),vledger.displayname,
        //    vledger.gstno,vledger.isinterstate,voucherno,voucherdate

        //), firstmatch as
        //(
        //    select gstinboxid,supfildt fileddate,ctin gstno,trdnm suppliername,val totalamount,inum invoiceno,dt invoicedate,
        //    rt taxpercentage,txval taxablevalue,
        //    case when igst>0 then igst else cgst+sgst end as tax,
        //    QuantoInvoice.lrinvoiceid,QuantoInvoice.isinterstatesale,QuantoInvoice.percentage,QuantoInvoice.taxamount,
        //    (case when igst>0 then igst else cgst+sgst end)-QuantoInvoice.taxamount as difftax,
        //    gstr2b_b2b.inum ptinvoiceno,QuantoInvoice.invoiceno qtinvoiceno,QuantoInvoice.entrydate,QuantoInvoice.entryno,
        //    case when iamt>0 then 'I' else 'L' end taxfrm,rchrg
        //    from gstr2b_b2b
        //    left outer join QuantoInvoice on QuantoInvoice.gstno = ctin
        //    and (QuantoInvoice.invoiceno = inum and (QuantoInvoice.percentage = rt)) and type <2
        //),
        //secondmatched as
        //(
        //    select firstmatch.gstinboxid,fileddate,firstmatch.gstno,firstmatch.suppliername,firstmatch.totalamount,
        //    case when QuantoInvoice.invoiceno is null then 
        //	firstmatch.invoiceno
        //	else firstmatch.invoiceno||'->'||QuantoInvoice.invoiceno end invoiceno,firstmatch.invoicedate,firstmatch.taxpercentage,firstmatch.taxablevalue,firstmatch.tax,
        //    QuantoInvoice.lrinvoiceid,QuantoInvoice.isinterstatesale,QuantoInvoice.percentage,QuantoInvoice.taxamount,
        //    (firstmatch.tax)-QuantoInvoice.taxamount as difftax,
        //    firstmatch.invoiceno ptinvoiceno,QuantoInvoice.invoiceno qtinvoiceno,QuantoInvoice.entrydate,QuantoInvoice.entryno,
        //    taxfrm,rchrg
        //    from firstmatch
        //    left outer join QuantoInvoice on QuantoInvoice.gstno = firstmatch.gstno
        //    and (QuantoInvoice.percentage = taxpercentage and firstmatch.invoicedate=QuantoInvoice.invoicedate and abs(tax-QuantoInvoice.taxamount)<5) and type <2
        //    where firstmatch.lrinvoiceid is null
        //)";


        /*
with gstr2b_b2b
as
(
select Gst_Inbox.id,
b2blist->>'ctin' ctin,
b2blist->>'supfildt' supfildt,
b2blist->>'trdnm' trdnm, 
b2blist->>'supprd' supprd,
invlist->>'rsn' rsn,
cast(invlist->>'val' as numeric) val,
invlist->>'rev' rev,
invlist->>'pos' pos,
invlist->>'itcavl' itcavl,
invlist->>'inum' inum,
invlist->>'diffprcnt' diffprcnt,
cast(invlist->>'dt' as date) dt,
invlist->>'typ' typ,
cast(taxlist->>'rt' as numeric) rt,
cast(taxlist->>'txval' as numeric) txval,
taxlist->>'num' num,
cast(taxlist->>'igst' as numeric) igst,
cast(taxlist->>'sgst' as numeric) sgst,
cast(taxlist->>'cess' as numeric) cess,
cast(taxlist->>'cgst' as numeric) cgst

from Gst_Inbox
LEFT JOIN LATERAL
json_array_elements (gstadata#>'{docdata,b2b}') b2blist ON true
LEFT JOIN LATERAL
json_array_elements (b2blist->'inv') invlist ON true
LEFT JOIN LATERAL
json_array_elements (invlist->'items') taxlist ON true
where gsttype = 'GSTR2B'
and fromdate>='2021-11-01'  and fromdate<'2021-12-01'
),
QuantoInvoice
as
(
select suppliercompany.name,tinanddate as gstno,invoiceno,invoicedate,baseamount,grossamount,totalamount,lrinvoice.isinterstatesale,
amounton,netamount,lrinvoicetax.percentage,lrinvoicetax.taxamount,lrinvoicetax.type from lrinvoice
inner join lrinvoicetax on lrinvoicetax.invoiceid = lrinvoice.id and lrinvoicetax.isactive=true
inner join suppliercompany on suppliercompany.id = lrinvoice.suppliercompanyid
where invoicedate>='2021-10-01' and  invoicedate<'2021-12-01' and lrinvoice.isactive = true
)

select QuantoInvoice.* from QuantoInvoice
--inner join gstr2b_b2b on gstr2b_b2b.trdnm = QuantoInvoice.name
where gstno = ''




select supfildt fileddate,ctin gstno,trdnm suppliername,val totalamount,inum invoiceno,dt invoicedate,
rt taxpercentage,txval taxablevalue,
case when igst>0 then igst else cgst+sgst end as tax,
QuantoInvoice.isinterstatesale,QuantoInvoice.percentage,QuantoInvoice.taxamount
from gstr2b_b2b
left outer join QuantoInvoice on QuantoInvoice.gstno = ctin
and (QuantoInvoice.invoiceno = inum or (QuantoInvoice.percentage = rt and QuantoInvoice.amounton =txval))
where 
QuantoInvoice.gstno is null
and rt=5
*/

        const string SQL_GST2A_B2B = @"select Gst_Inbox.id,
b2blist->>'ctin' ctin,
b2blist->>'flprdr1' flprdr1,
b2blist->>'cfs3b' cfs3b, 
b2blist->>'cfs' cfs,
b2blist->>'dtcancel' dtcancel,
b2blist->>'fldtr1' fldtr1,
b2blist->>'cfs' cfs,

invlist->>'aspd' aspd,
invlist->>'astyp' astyp,
invlist->>'inv_typ' inv_typ,
invlist->>'rchrg' rchrg,
invlist->>'idt' idt,
invlist->>'pos' pos,
invlist->>'inum' inum,
invlist->>'cfs' cfs,
invlist->>'val' val,
taxlist->>'num' num,
taxlist#>'{itm_det,rt}' rt,
taxlist#>'{itm_det,csamt}' csamt,
taxlist#>'{itm_det,samt}' samt,
taxlist#>'{itm_det,txval}' txval,
taxlist#>'{itm_det,iamt}' iamt,
taxlist#>'{itm_det,camt}' camt
from Gst_Inbox
LEFT JOIN LATERAL
json_array_elements (gstadata#>'{b2b}') b2blist ON true
LEFT JOIN LATERAL
json_array_elements (b2blist->'inv') invlist ON true
LEFT JOIN LATERAL
json_array_elements (invlist->'itms') taxlist ON true";

        const string SQL_GSTR2A_B2BA = @"select Gst_Inbox.id,
b2blist->>'ctin' ctin,
b2blist->>'flprdr1' flprdr1,
b2blist->>'cfs3b' cfs3b, 
b2blist->>'cfs' cfs,
b2blist->>'dtcancel' dtcancel,
b2blist->>'fldtr1' fldtr1,
b2blist->>'cfs' cfs,

invlist->>'aspd' aspd,
invlist->>'astyp' astyp,
invlist->>'inv_typ' inv_typ,
invlist->>'rchrg' rchrg,
invlist->>'idt' idt,
invlist->>'oidt' idt,
invlist->>'pos' pos,
invlist->>'inum' inum,
invlist->>'oinum' inum,
invlist->>'cfs' cfs,
invlist->>'val' val,
taxlist->>'num' num,
taxlist#>'{itm_det,rt}' rt,
taxlist#>'{itm_det,csamt}' csamt,
taxlist#>'{itm_det,samt}' samt,
taxlist#>'{itm_det,txval}' txval,
taxlist#>'{itm_det,iamt}' iamt,
taxlist#>'{itm_det,camt}' camt
from Gst_Inbox
LEFT JOIN LATERAL
json_array_elements (gstadata#>'{b2ba}') b2blist ON true
LEFT JOIN LATERAL
json_array_elements (b2blist->'inv') invlist ON true
LEFT JOIN LATERAL
json_array_elements (invlist->'itms') taxlist ON true";

        const string SQL_GST2A_CDNR = @"select Gst_Inbox.id,
b2blist->>'ctin' ctin,
b2blist->>'flprdr1' flprdr1,
b2blist->>'cfs3b' cfs3b, 
b2blist->>'cfs' cfs,
b2blist->>'dtcancel' dtcancel,
b2blist->>'fldtr1' fldtr1,
b2blist->>'cfs' cfs,
invlist->>'aspd' aspd,
invlist->>'astyp' astyp,
invlist->>'inv_typ' inv_typ,
invlist->>'rchrg' rchrg,
invlist->>'nt_dt' idt,
invlist->>'pos' pos,
invlist->>'nt_num' inum,
invlist->>'cfs' cfs,
invlist->>'val' val,
taxlist->>'num' num,
taxlist#>'{itm_det,rt}' rt,
taxlist#>'{itm_det,csamt}' csamt,
taxlist#>'{itm_det,samt}' samt,
taxlist#>'{itm_det,iamt}' iamt,
taxlist#>'{itm_det,txval}' txval,
taxlist#>'{itm_det,camt}' camt
from Gst_Inbox
LEFT JOIN LATERAL
json_array_elements (gstadata#>'{cdn}') b2blist ON true
LEFT JOIN LATERAL
json_array_elements (b2blist->'nt') invlist ON true
LEFT JOIN LATERAL
json_array_elements (invlist->'itms') taxlist ON true";

        const string SQL_GSTR2B_CDNR = @"
select Gst_Inbox.id,
b2blist->>'ctin' ctin,
b2blist->>'supfildt' supfildt,
b2blist->>'trdnm' trdnm, 
b2blist->>'supprd' supprd,
invlist->>'rsn' rsn,
invlist->>'val' val,
invlist->>'rev' rev,
invlist->>'pos' pos,
invlist->>'itcavl' itcavl,
invlist->>'ntnum' inum,
invlist->>'diffprcnt' diffprcnt,
invlist->>'dt' dt,
invlist->>'suptyp' sub,
invlist->>'typ' typ,
taxlist->>'rt' rt,
taxlist->>'txval' txval,
taxlist->>'num' num,
taxlist->>'igst' igst,
taxlist->>'sgst' sgst,
taxlist->>'cess' cess,
taxlist->>'num' num,
taxlist->>'cgst' cgst
from Gst_Inbox
LEFT JOIN LATERAL
json_array_elements (gstadata#>'{docdata,cdnr}') b2blist ON true
LEFT JOIN LATERAL
json_array_elements (b2blist->'nt') invlist ON true
LEFT JOIN LATERAL
json_array_elements (invlist->'items') taxlist ON true";

        const string SQL_GSTR2B_B2BA = @"
select Gst_Inbox.id,
b2blist->>'ctin' ctin,
b2blist->>'supfildt' supfildt,
b2blist->>'trdnm' trdnm, 
b2blist->>'supprd' supprd,
invlist->>'rsn' rsn,
invlist->>'val' val,
invlist->>'oidt' val,
invlist->>'rev' rev,
invlist->>'pos' pos,
invlist->>'itcavl' itcavl,
invlist->>'oinum' inum,
invlist->>'inum' inum,
invlist->>'diffprcnt' diffprcnt,
invlist->>'dt' dt,
invlist->>'typ' typ,
taxlist->>'rt' rt,
taxlist->>'txval' txval,
taxlist->>'num' num,
taxlist->>'igst' igst,
taxlist->>'sgst' sgst,
taxlist->>'cess' cess,
taxlist->>'num' num,
taxlist->>'cgst' cgst
from Gst_Inbox
LEFT JOIN LATERAL
json_array_elements (gstadata#>'{docdata,b2ba}') b2blist ON true
LEFT JOIN LATERAL
json_array_elements (b2blist->'inv') invlist ON true
LEFT JOIN LATERAL
json_array_elements (invlist->'items') taxlist ON true";

        const string SQL_GSTR2B_B2B = @"
select Gst_Inbox.id,
b2blist->>'ctin' ctin,
b2blist->>'supfildt' supfildt,
b2blist->>'trdnm' trdnm, 
b2blist->>'supprd' supprd,
invlist->>'rsn' rsn,
invlist->>'val' val,
invlist->>'rev' rev,
invlist->>'pos' pos,
invlist->>'itcavl' itcavl,
invlist->>'inum' inum,
invlist->>'diffprcnt' diffprcnt,
invlist->>'dt' dt,
invlist->>'typ' typ,
taxlist->>'rt' rt,
taxlist->>'txval' txval,
taxlist->>'num' num,
taxlist->>'igst' igst,
taxlist->>'sgst' sgst,
taxlist->>'cess' cess,
taxlist->>'num' num,
taxlist->>'cgst' cgst
from Gst_Inbox
LEFT JOIN LATERAL
json_array_elements (gstadata#>'{docdata,b2b}') b2blist ON true
LEFT JOIN LATERAL
json_array_elements (b2blist->'inv') invlist ON true
LEFT JOIN LATERAL
json_array_elements (invlist->'items') taxlist ON true";


        const string SQL_Search = @"select id, companyid, fromdate, todate, gstno, gsttype, apiresponse, gstperiod, gstsession, gstadata, createdby, createdon, modifiedby, modifiedon, isactive, version from gst_inbox where id=@id";

        const string SQL_Insert = @"
        Update gst_inbox set isactive = false where gsttype=@gsttype and gstperiod=@gstperiod and companyid=@companyid;
        INSERT INTO public.gst_inbox(companyid, fromdate, todate, gstno, gsttype,apiresponse, gstperiod, gstsession, gstadata, createdby, createdon, modifiedby, modifiedon, isactive, version) VALUES(@companyid, @fromdate, @todate, @gstno, @gsttype,@apiresponse, @gstperiod, @gstsession, @gstadata, @createdby, @createdon, @modifiedby, @modifiedon, @isactive, @version) RETURNING id";

        const string SQL_Update = @"UPDATE public.gst_inbox SET companyid=@companyid, fromdate=@fromdate, todate=@todate, gstno=@gstno, gsttype=@gsttype,apiresponse=@apiresponse, gstperiod=@gstperiod, gstsession=@gstsession, gstadata=@gstadata, modifiedby=@modifiedby, modifiedon=@modifiedon, version=@version+1 where id=@id and version=@version RETURNING version";

        const string SQL_Delete = @"update public.gst_inbox set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";



        public List<Gst_Inbox> Search(Gst_InboxCreteria _gst_inbox)
        {
            List<Gst_Inbox> lst = new List<Gst_Inbox>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_gst_inbox.id != 0)
                {
                    command = new NpgsqlCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _gst_inbox.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("isactive", true, "=");
                    qb.AddParameter("companyid", _gst_inbox.companyid, "=");
                    qb.AddParameter("gsttype", _gst_inbox.gsttype, "=");
                    qb.AddParameter("gstperiod", _gst_inbox.gstperiod, "=");
                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Gst_Inbox ogst_inbox = new Gst_Inbox();
                        ogst_inbox.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        ogst_inbox.companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"]);
                        ogst_inbox.fromdate = reader["fromdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["fromdate"]);
                        ogst_inbox.todate = reader["todate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["todate"]);
                        ogst_inbox.gstno = reader["gstno"] == DBNull.Value ? "" : reader["gstno"].ToString();
                        ogst_inbox.gsttype = reader["gsttype"] == DBNull.Value ? "" : reader["gsttype"].ToString();
                        ogst_inbox.apiresponse = reader["apiresponse"] == DBNull.Value ? "" : reader["apiresponse"].ToString();
                        ogst_inbox.gstperiod = reader["gstperiod"] == DBNull.Value ? "" : reader["gstperiod"].ToString();
                        ogst_inbox.gstsession_json = reader["gstsession"] == DBNull.Value ? null : reader["gstsession"].ToString();
                        ogst_inbox.gstadata_json = reader["gstadata"] == DBNull.Value ? null : reader["gstadata"].ToString();
                        ogst_inbox.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        ogst_inbox.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        ogst_inbox.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        ogst_inbox.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        ogst_inbox.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        ogst_inbox.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        lst.Add(ogst_inbox);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _gst_inbox);
            }
            return lst;
        }



        public Gst_Inbox Insert(Gst_Inbox oGst_Inbox)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);
                    oGst_Inbox.fromdate = BaseEntity.GetDataFromString("01-" + oGst_Inbox.gstperiod.Substring(0, 2) +
                        "-" + oGst_Inbox.gstperiod.Substring(2, 4));
                    oGst_Inbox.todate = oGst_Inbox.fromdate.AddMonths(1).AddDays(-1);
                    Insert(transaction, conn, oGst_Inbox);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oGst_Inbox);
            }

            return oGst_Inbox;
        }

        public Gst_Inbox Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Gst_Inbox oGst_Inbox)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oGst_Inbox.companyid;
            command.Parameters.Add("fromdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oGst_Inbox.fromdate;
            command.Parameters.Add("todate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oGst_Inbox.todate;
            command.Parameters.Add("gstno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oGst_Inbox.gstno == null ? "" : oGst_Inbox.gstno;
            command.Parameters.Add("gsttype", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oGst_Inbox.gsttype == null ? "" : oGst_Inbox.gsttype;
            command.Parameters.Add("apiresponse", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oGst_Inbox.apiresponse == null ? "" : oGst_Inbox.apiresponse;
            command.Parameters.Add("gstperiod", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oGst_Inbox.gstperiod == null ? "" : oGst_Inbox.gstperiod;
            command.Parameters.Add("gstsession", NpgsqlTypes.NpgsqlDbType.Json).Value = oGst_Inbox.gstsession_json == null ? "null" : oGst_Inbox.gstsession_json;
            command.Parameters.Add("gstadata", NpgsqlTypes.NpgsqlDbType.Json).Value = oGst_Inbox.gstadata_json == null ? "null" : oGst_Inbox.gstadata_json;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oGst_Inbox.createdby;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oGst_Inbox.createdon;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oGst_Inbox.modifiedby;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oGst_Inbox.modifiedon;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oGst_Inbox.isactive;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oGst_Inbox.version;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oGst_Inbox.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            return oGst_Inbox;
        }

        public Gst_Inbox Update(Gst_Inbox oGst_Inbox)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    Update(transaction, conn, oGst_Inbox);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oGst_Inbox);
            }
            return oGst_Inbox;
        }

        public Gst_Inbox Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Gst_Inbox oGst_Inbox)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oGst_Inbox.id;
            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oGst_Inbox.companyid;
            command.Parameters.Add("fromdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oGst_Inbox.fromdate;
            command.Parameters.Add("todate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oGst_Inbox.todate;
            command.Parameters.Add("gstno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oGst_Inbox.gstno == null ? "" : oGst_Inbox.gstno;
            command.Parameters.Add("gsttype", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oGst_Inbox.gsttype == null ? "" : oGst_Inbox.gsttype;
            command.Parameters.Add("gsttype", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oGst_Inbox.gsttype == null ? "" : oGst_Inbox.gsttype;
            command.Parameters.Add("gstperiod", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oGst_Inbox.gstperiod == null ? "" : oGst_Inbox.gstperiod;
            command.Parameters.Add("gstsession", NpgsqlTypes.NpgsqlDbType.Json).Value = oGst_Inbox.gstsession_json == null ? "null" : oGst_Inbox.gstsession_json;
            command.Parameters.Add("gstadata", NpgsqlTypes.NpgsqlDbType.Json).Value = oGst_Inbox.gstadata_json == null ? "null" : oGst_Inbox.gstadata_json;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oGst_Inbox.createdby;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oGst_Inbox.createdon;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oGst_Inbox.modifiedby;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oGst_Inbox.modifiedon;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oGst_Inbox.isactive;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oGst_Inbox.version;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oGst_Inbox.version = Convert.ToInt32(reader[0].ToString());
                }
            }

            return oGst_Inbox;
        }


        public bool Delete(Gst_Inbox oGst_Inbox)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oGst_Inbox);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oGst_Inbox);
            }
            return hasSuccessCount;

        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Gst_Inbox oGst_Inbox)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oGst_Inbox.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oGst_Inbox.modifiedby;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oGst_Inbox.modifiedon;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oGst_Inbox.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oGst_Inbox.id;
            int successCount = command.ExecuteNonQuery();
            return successCount > 0;
        }

        internal List<GST_Reconciliation> ReconciliationGSTR2A(Gst_Inbox item)
        {
            List<GST_Reconciliation> result = new List<GST_Reconciliation>();
            SortedList<string, object> parameter = new SortedList<string, object>();
            var gstfromdate = BaseEntity.GetDataFromString("01-" + item.gstperiod.Substring(0, 2) + "-" + item.gstperiod.Substring(2, 4));
            var gsttodate = gstfromdate.AddMonths(1);
            var invtodate = gstfromdate.AddMonths(1);
            var invfromdate = gstfromdate.AddMonths(-1);

            parameter.Add("gstfromdate", gstfromdate);
            parameter.Add("gsttodate", gsttodate);
            parameter.Add("invtodate", invtodate);
            parameter.Add("invfromdate", invfromdate);
            parameter.Add("companyid", item.id);
            parameter.Add("gsttype", "GSTR2A-B2B");
            long gstinboxid = 0;
            using (var reader = ExecuteReader(@"select id from Gst_Inbox where gsttype = @gsttype and isactive = true and fromdate>=@gstfromdate and fromdate<@gsttodate and Gst_Inbox.companyid = @companyid", parameter))
            {
                if (reader.Read())
                {
                    gstinboxid = reader.GetInt64(0);
                }
            }
            if (gstinboxid == 0)
                throw new Exception("Please download the GSTR2A before doing reconciliation");

            ExecuteQuery(GSTR2A_REconcilation_MView.Replace("{0}", gstinboxid.ToString()), null, 1000000);

            result.AddRange(Parse(false, GSTR2B_REconcilation.Replace("GSTR2B", "GSTR2A").Replace("{0}", gstinboxid.ToString()) + " select * from firstmatch where lrinvoiceid is not null;",
                "Full Match", parameter));

            CompanyDataService companyDataService = new CompanyDataService() { Context = this.Context };

            var excludeList = companyDataService.SearchGSTExclude(gstfromdate, gsttodate,
        result.GroupBy(e => e.gstinboxid).ToList().ConvertAll(e => e.Key));

            result.AddRange(Parse(false, GSTR2B_REconcilation.Replace("GSTR2B", "GSTR2A").Replace("{0}", gstinboxid.ToString()) + " select * from secondmatched where lrinvoiceid is not null;",
    "Amount Match", parameter));

            List<long> ids = new List<long>();
            string query = "select id from gst_inbox where fromdate>=@gstfromdate and todate<=@gsttodate and gsttype='GSTR2B' and companyid=@companyid";
            using (var reader = ExecuteReader(query, parameter))
            {
                while (reader.Read())
                {
                    ids.Add(reader.GetInt64(0) * -1);
                }
            }
            if (ids.Count > 0)
            {
                ExecuteQuery("update lrinvoice set gstinboxid = 0 where companyid =@companyid and gstinboxid in (" + string.Join(",", ids) + ") and gstinboxid=0", parameter);
            }

            foreach (var gstinboxitem in result.Where(e => e.lrinvoiceid > 0).GroupBy(e => e.gstinboxid))
            {
                string invquery = "Update lrinvoice set gstinboxid=" + (gstinboxitem.Key * -1) + " where  companyid =@companyid and id in (" +
                    string.Join(",", gstinboxitem.ToList().ConvertAll(e => e.lrinvoiceid)) + ")";
                ExecuteQuery(invquery, parameter);
            }

            var nonMatches = Parse(false, GSTR2B_REconcilation.Replace("GSTR2B", "GSTR2A").Replace("{0}", gstinboxid.ToString()) + " select * from secondmatched where lrinvoiceid is null;", "Non Match", parameter);
            foreach (var nonmatch in nonMatches)
            {
                var index = excludeList.FindIndex(e => e.gstinboxid == nonmatch.gstinboxid &&
                e.gstno == nonmatch.gstno && e.invoiceno == nonmatch.invoiceno &&
                e.invoicedate == nonmatch.invoicedate);
                if (index >= 0)
                    nonmatch.type = excludeList[index].reason;
            }
            result.AddRange(nonMatches);

            nonMatches = Parse(false, GSTR2B_NoFiled, "Non Filed", parameter);
            foreach (var nonmatch in nonMatches)
            {
                var index = excludeList.FindIndex(e => e.lrinvoiceid == nonmatch.lrinvoiceid &&
                e.gstno == nonmatch.gstno && e.invoiceno == nonmatch.invoiceno &&
                e.invoicedate == nonmatch.invoicedate);
                if (index >= 0)
                    nonmatch.type = excludeList[index].reason;
            }
            result.AddRange(nonMatches);
            foreach (var gitem in result.GroupBy(e => new { e.gstno, e.invoicedate, e.invoiceno }))
            {
                gitem.First().count = 1;
            }
            var missingstn = result.FindAll(e => string.IsNullOrEmpty(e.suppliername)).ConvertAll(e => e.gstno).Distinct();

            try
            {
                var existinggstns = SearchGSTN(item.id, missingstn.ToList());
                foreach (var gitem in result.FindAll(e => string.IsNullOrEmpty(e.suppliername)))
                {
                    if (existinggstns.Exists(e => e.gstno == gitem.gstno))
                    {
                        gitem.suppliername = existinggstns.Find(e => e.gstno == gitem.gstno).name;
                    }
                }
            }
            catch (Exception exp)
            {

            }

            List<string> duplicategst = new List<string>();
            foreach (var grp in result.GroupBy(x => x.gstno))
            {
                if (grp.ToList().GroupBy(x => x.suppliername).Count() > 1)
                {
                    duplicategst.Add(grp.Key);
                }
            }

            if (duplicategst.Count > 0)
            {
                var duplicatesuppiernames = SearchGSTN(item.id, duplicategst);
                foreach (var gstitems in duplicatesuppiernames)
                {
                    var duplicatelist = result.FindAll(x => x.gstno == gstitems.gstno);
                    result.RemoveAll(x => x.gstno == gstitems.gstno);
                    foreach (var ditem in duplicatelist.GroupBy(x => new { x.invoicedate, x.invoiceno, x.taxable, x.tax, x.taxpercentage }))
                    {
                        var newditem = ditem.First();
                        if (duplicatesuppiernames.Exists(e => e.gstno == gstitems.gstno))
                            newditem.suppliername = duplicatesuppiernames.Find(e => e.gstno == gstitems.gstno).name;
                        result.Add(newditem);
                    }
                }
            }
            var missingfiling = result.FindAll(x => string.IsNullOrEmpty(x.filedmonth));
            if (missingfiling.Count > 0)
            {
                var gstnos = missingfiling.ConvertAll(x => x.gstno).Distinct().ToList();
                var filingdetails = companyDataService.SearchGSTFilingInfo(item.fromdate, gstnos, item.showfiling);
            }
            return result;
        }

        public string URL
        {
            get
            {
                return InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.gstserver;
            }
        }

        public InfyGST.GSTPortal GetService(long id)
        {
            string gstno = null;
            string portaluserid = null;
            string gstaccesskey = null;
            var param = new SortedList<string, object>();
            param.Add("id", id);
            using (var reader = ExecuteReader("select tinanddate,gstusername,gstaccesskey from company where id=@id", param))
            {
                if (reader.Read())
                {
                    gstno = reader.IsDBNull(0) ? "" : reader.GetString(0);
                    portaluserid = reader.IsDBNull(1) ? "" : reader.GetString(1);
                    gstaccesskey = reader.IsDBNull(2) ? "" : reader.GetString(2);
                }
            }
            if (string.IsNullOrEmpty(portaluserid)) throw new Exception("Connect the GST using your otp");

            InfyGST.GSTPortal GSTPortal = new InfyGST.GSTPortal();
            GSTPortal.Initalize(URL, id, gstno, portaluserid, gstaccesskey);
            return GSTPortal;
        }

        internal string ResetGSTR1(Gst_Inbox item)
        {
            var GSTPortal = GetService(item.id);
            var result = GSTPortal.ResetGSTR1A(item.gstperiod).Result;
            if (result.IsSuccess)
            {
                throw new Exception(result.TxnOutcome);
            }
            else
            {
                throw new Exception(string.IsNullOrEmpty(result.TxnOutcome) ? "Your session expire, please connect again.." : result.TxnOutcome);
            }
        }
        internal string GSTR1Status(Gst_Inbox item)
        {
            var GSTPortal = GetService(item.id);
            string query = "Select apiresponse from Gst_Inbox where gsttype='GSTR1-UPLOAD' and companyid=@companyid " +
                "and gstperiod=@gstperiod order by id desc limit 1";
            string referenceid = "";
            SortedList<string, object> parameter = new SortedList<string, object>();
            parameter.Add("companyid", item.id);
            parameter.Add("gstperiod", item.gstperiod);
            using (var reader = ExecuteReader(query, parameter))
            {
                if (reader.Read())
                {
                    referenceid = reader.GetString(0);
                }
            }
            if (string.IsNullOrEmpty(referenceid))
            {
                throw new Exception("Status not available , please upload GSTR-1");
            }
            var result = GSTPortal.GetGSTR1AStatus(item.gstperiod, referenceid).Result;
            if (result.IsSuccess)
            {
                throw new Exception(result.TxnOutcome);
            }
            else
            {
                throw new Exception(string.IsNullOrEmpty(result.TxnOutcome) ? "Your session expire, please connect again.." : result.TxnOutcome);
            }
        }

        public List<GST_Info> SearchGSTN(long id, List<string> list)
        {
            var GSTPortal = GetService(id);
            List<GST_Info> result = new List<GST_Info>();
            if (list.Count == 0) return result;
            string query = "Select * from gst_info where gstno in (" + string.Join(",", list.ConvertAll(e => "'" + e + "'")) + ")";
            using (var reader = ExecuteReader(query, null))
            {
                while (reader.Read())
                {
                    var gstno = reader["gstno"].ToString();
                    if (!result.Exists(x => x.gstno == gstno))
                    {
                        result.Add(new GST_Info()
                        {
                            gstno = reader["gstno"].ToString(),
                            name = reader["name"].ToString(),
                            data_json = reader["data"].ToString()
                        });
                    }
                }
            }
            list.RemoveAll(e => result.Exists(ex => ex.gstno == e));
            CompanyDataService companyDataService = new CompanyDataService() { Context = this.Context };
            using (NpgsqlConnection conn = CreateConnection())
            {
                conn.Open();
                foreach (var gstnoinfo in list)
                {
                    var gstinfo = GSTPortal.SearchGSTN(gstnoinfo).Result;
                    var gstdata = companyDataService.InsertGSTN(conn, new GST_Info()
                    {
                        gstno = gstinfo.gstin,
                        name = gstinfo.tradeNam,
                        data = gstinfo
                    });
                    result.Add(gstdata);
                }
                conn.Close();
            }
            return result;
        }

        internal List<GST_Reconciliation> ReconciliationGSTR2B(Gst_Inbox item)
        {
            List<GST_Reconciliation> result = new List<GST_Reconciliation>();
            SortedList<string, object> parameter = new SortedList<string, object>();
            var gstfromdate = BaseEntity.GetDataFromString("01-" + item.gstperiod.Substring(0, 2) + "-" + item.gstperiod.Substring(2, 4));
            var gsttodate = gstfromdate.AddMonths(1);
            var invtodate = gstfromdate.AddMonths(1);
            var invfromdate = gstfromdate.AddMonths(-1);

            parameter.Add("gstfromdate", gstfromdate);
            parameter.Add("gsttodate", gsttodate);
            parameter.Add("invtodate", invtodate);
            parameter.Add("invfromdate", invfromdate);
            parameter.Add("gsttype", "GSTR2B");
            parameter.Add("companyid", item.id);

            long gstinboxid = 0;
            using (var reader = ExecuteReader(@"select id from Gst_Inbox where gsttype = @gsttype and isactive = true and fromdate>=@gstfromdate and fromdate<@gsttodate and Gst_Inbox.companyid = @companyid", parameter))
            {
                if (reader.Read())
                {
                    gstinboxid = reader.GetInt64(0);
                }
            }
            if (gstinboxid == 0)
                throw new Exception("Please download the GSTR2B before doing reconciliation");

            ExecuteQuery(GSTR2B_REconcilation_MView.Replace("{0}", gstinboxid.ToString()), null, 1000000);

            result.AddRange(Parse(true, GSTR2B_REconcilation.Replace("{0}", gstinboxid.ToString()) + " select * from firstmatch where lrinvoiceid is not null;",
                "Full Match", parameter));

            result.AddRange(Parse(true, GSTR2B_REconcilation.Replace("{0}", gstinboxid.ToString()) + " select * from secondmatched where lrinvoiceid is not null;",
                "Amount Match", parameter));

            List<long> ids = new List<long>();
            string query = "select id from gst_inbox where fromdate>=@gstfromdate and todate<=@gsttodate and companyid=@companyid and isactive=true and gsttype ilike 'GSTR2A-%'";
            using (var reader = ExecuteReader(query, parameter))
            {
                while (reader.Read())
                {
                    ids.Add(reader.GetInt64(0));
                }
            }
            if (ids.Count > 0)
            {
                ExecuteQuery("update lrinvoice set gstinboxid = 0 where  companyid=@companyid and gstinboxid in (" + string.Join(",", ids) + ")", parameter);
            }

            foreach (var gstinboxitem in result.Where(e => e.lrinvoiceid > 0).GroupBy(e => e.gstinboxid))
            {
                string invquery = "Update lrinvoice set gstinboxid=" + gstinboxitem.Key + " where id in (" +
                    string.Join(",", gstinboxitem.ToList().ConvertAll(e => e.lrinvoiceid)) + ") and gstinboxid=0 and companyid=@companyid";
                ExecuteQuery(invquery, parameter);
            }

            CompanyDataService companyDataService = new CompanyDataService() { Context = this.Context };
            var excludeList = companyDataService.SearchGSTExclude(gstfromdate, gsttodate, result.GroupBy(e => e.gstinboxid).ToList().ConvertAll(e => e.Key));

            var nonMatches = Parse(true, GSTR2B_REconcilation.Replace("{0}", gstinboxid.ToString()) + " select * from secondmatched where lrinvoiceid is null;", "Non Match", parameter);
            foreach (var nonmatch in nonMatches)
            {
                var index = excludeList.FindIndex(e => e.gstinboxid == nonmatch.gstinboxid &&
                e.gstno == nonmatch.gstno && e.invoiceno == nonmatch.invoiceno &&
                e.invoicedate == nonmatch.invoicedate);
                if (index >= 0)
                    nonmatch.type = excludeList[index].reason;
            }

            result.AddRange(nonMatches);
            nonMatches = Parse(false, GSTR2B_NoFiled, "Non Filed", parameter);
            foreach (var nonmatch in nonMatches)
            {
                var index = excludeList.FindIndex(e => e.lrinvoiceid == nonmatch.lrinvoiceid &&
                e.gstno == nonmatch.gstno && e.invoiceno == nonmatch.invoiceno &&
                e.invoicedate == nonmatch.invoicedate);
                if (index >= 0)
                    nonmatch.type = excludeList[index].reason;
            }
            result.AddRange(nonMatches);
            foreach (var gitem in result.GroupBy(e => new { e.gstno, e.invoicedate, e.invoiceno }))
            {
                gitem.First().count = 1;
            }

            using (var reader = ExecuteReader(@"select distinct gst_exlude.gstno from gst_exlude 
        inner join gst_inbox on gst_inbox.id = gst_exlude.gstinboxid
        where isreserve=true and gst_inbox.companyid=@companyid", parameter))
            {
                while (reader.Read())
                {
                    var gstn = reader.GetString(0);
                    var lst = result.FindAll(e => e.gstno == gstn && string.IsNullOrEmpty(e.type));
                    lst.ForEach(e => e.type = "RESERVED");
                }
            }


            var missingstn = result.FindAll(e => string.IsNullOrEmpty(e.suppliername)).ConvertAll(e => e.gstno).Distinct().ToList();
            try
            {
                var availgstcodes = result.FindAll(x => !string.IsNullOrEmpty(x.suppliername));
                var matchavail = missingstn.FindAll(x => availgstcodes.Exists(y => y.gstno == x));
                if (matchavail.Count > 0)
                {
                    foreach (var gstnupdate in matchavail)
                    {
                        var suppliername = availgstcodes.Find(x => x.gstno == gstnupdate).suppliername;
                        result.FindAll(x => x.gstno == gstnupdate).ForEach(x => x.suppliername = suppliername);
                        missingstn.Remove(gstnupdate);
                    }
                }
                var existinggstns = SearchGSTN(item.id, missingstn.ToList());
                foreach (var gitem in result.FindAll(e => string.IsNullOrEmpty(e.suppliername)))
                {
                    if (existinggstns.Exists(e => e.gstno == gitem.gstno))
                    {
                        gitem.suppliername = existinggstns.Find(e => e.gstno == gitem.gstno).name;
                    }
                }

                var missingfiling = result.FindAll(x => string.IsNullOrEmpty(x.filedmonth));
                var crossupdate = missingfiling.FindAll(x => result.Exists(y => y.gstno == x.gstno && !string.IsNullOrEmpty(y.filedmonth)));
                foreach (var filing in crossupdate.GroupBy(x => x.gstno))
                {
                    var exist = result.Find(y => y.gstno == filing.Key && !string.IsNullOrEmpty(y.filedmonth));
                    filing.ToList().ForEach(x =>
                    {
                        x.fileddate = exist.fileddate;
                        x.filedmonth = exist.filedmonth;
                    });
                }
                missingfiling = result.FindAll(x => string.IsNullOrEmpty(x.filedmonth));
                if (missingfiling.Count > 0)
                {
                    var gstnos = missingfiling.ConvertAll(x => x.gstno).Distinct().ToList();
                    var filingdetails = companyDataService.SearchGSTFilingInfo(gstfromdate, gstnos, item.showfiling);
                    if (item.showfiling && filingdetails.Exists(x => x.update))
                    {
                        using (var connecition = CreateConnection())
                        {
                            connecition.Open();
                            var command = GetCommand("Update suppliercompany set filedmonth=@filedmonth,filedon=filedon where id=@id", connecition);
                            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = 0;
                            command.Parameters.Add("filedmonth", NpgsqlTypes.NpgsqlDbType.Varchar).Value = "";
                            command.Parameters.Add("filedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTime.MinValue;
                            foreach (var supplier in filingdetails.FindAll(x => x.update && x.supplierid > 0))
                            {
                                command.Parameters["id"].Value = supplier.supplierid;
                                command.Parameters["filedmonth"].Value = supplier.filedmonth;
                                command.Parameters["filedon"].Value = supplier.filedon;
                                command.ExecuteNonQuery();
                            }

                            command = GetCommand("Update gst_info set filedmonth=@filedmonth,filedon=filedon where gstno=@gstno", connecition);
                            command.Parameters.Add("gstno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = "";
                            command.Parameters.Add("filedmonth", NpgsqlTypes.NpgsqlDbType.Varchar).Value = "";
                            command.Parameters.Add("filedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTime.MinValue;
                            foreach (var supplier in filingdetails.FindAll(x => x.update && x.supplierid == 0))
                            {
                                command.Parameters["gstno"].Value = supplier.gstno;
                                command.Parameters["filedmonth"].Value = supplier.filedmonth;
                                command.Parameters["filedon"].Value = supplier.filedon;
                                command.ExecuteNonQuery();
                            }
                            connecition.Close();
                        }
                    }
                    filingdetails.ForEach(x =>
                    {
                        result.FindAll(y => y.gstno == x.gstno).ForEach(z =>
                        {
                            z.filedmonth = x.filedmonth;
                            z.fileddate = x.filedon;
                        });
                    });
                }
            }
            catch (Exception exp)
            {

            }


            return result;
        }

        public List<GST_Reconciliation> Parse(bool detail, string query, string type,
            SortedList<string, object> parameter)
        {
            List<GST_Reconciliation> resulst = new List<GST_Reconciliation>();
            using (var reader = ExecuteReader(query, parameter))
            {
                List<string> uid = new List<string>();
                while (reader.Read())
                {
                    //gstinboxid,fileddate,gstno,suppliername,totalamount,invoiceno,invoicedate,taxpercentage,tax,lrinvoiceid,isinterstatesale
                    GST_Reconciliation item = new GST_Reconciliation();
                    item.type = type;
                    item.fileddate = reader["fileddate"] != DBNull.Value ? Convert.ToDateTime(reader["fileddate"]) : DateTime.MinValue;
                    item.filedmonth = reader["filedmonth"] != DBNull.Value ? Convert.ToString(reader["filedmonth"]) : "";
                    item.invoicedate = reader["invoicedate"] != DBNull.Value ? Convert.ToDateTime(reader["invoicedate"]) : DateTime.MinValue;
                    item.gstinboxid = reader["gstinboxid"] != DBNull.Value ? Convert.ToInt64(reader["gstinboxid"]) : 0;
                    item.lrinvoiceid = reader["lrinvoiceid"] != DBNull.Value ? Convert.ToInt64(reader["lrinvoiceid"]) : 0;
                    item.gstno = reader["gstno"] != DBNull.Value ? Convert.ToString(reader["gstno"]) : "";
                    item.suppliername = reader["suppliername"] != DBNull.Value ? Convert.ToString(reader["suppliername"]) : "";
                    item.invoiceno = reader["invoiceno"] != DBNull.Value ? Convert.ToString(reader["invoiceno"]) : "";
                    item.isinterstatesale = reader["isinterstatesale"] != DBNull.Value ? Convert.ToBoolean(reader["isinterstatesale"]) : false;
                    item.tax = reader["tax"] != DBNull.Value ? Convert.ToDecimal(reader["tax"]) : 0;
                    item.taxable = reader["taxablevalue"] != DBNull.Value ? Convert.ToDecimal(reader["taxablevalue"]) : 0;
                    item.taxpercentage = reader["taxpercentage"] != DBNull.Value ? Convert.ToDecimal(reader["taxpercentage"]) : 0;
                    item.difftax = reader["difftax"] != DBNull.Value ? Convert.ToDecimal(reader["difftax"]) : 0;

                    if (detail)
                    {
                        item.ptinvoiceno = reader["ptinvoiceno"] != DBNull.Value ? Convert.ToString(reader["ptinvoiceno"]) : "";
                        item.qtinvoiceno = reader["qtinvoiceno"] != DBNull.Value ? Convert.ToString(reader["qtinvoiceno"]) : "";
                        item.qtentryno = reader["entryno"] != DBNull.Value ? Convert.ToString(reader["entryno"]) : "";
                        item.qtentrydate = reader["entrydate"] != DBNull.Value ? Convert.ToDateTime(reader["entrydate"]) : DateTime.MinValue;
                        item.taxfrm = reader["taxfrm"] != DBNull.Value ? Convert.ToString(reader["taxfrm"]) : "";
                        item.rchrg = reader["rchrg"] != DBNull.Value ? Convert.ToString(reader["rchrg"]) : "";
                    }
                    item.uniqueid = item.lrinvoiceid.GetHashCode() + "-" + item.gstno.GetHashCode() + "-" + item.taxable.GetHashCode();
                    if (uid.Contains(item.uniqueid))
                        continue;
                    uid.Add(item.uniqueid);
                    //if (resulst.Exists(x => x.lrinvoiceid == item.lrinvoiceid &&
                    //        x.gstno == item.gstno &&
                    //        x.taxpercentage == item.taxpercentage && x.taxable == item.taxable))
                    //    continue;
                    resulst.Add(item);
                }
            }
            return resulst;
        }

        internal Gst_Inbox UpdateGSTR1(Gst_Inbox item)
        {
            var GSTR1 = BuildGSTR1(item);

            var GSTPortal = GetService(item.id);

            var result = GSTPortal.UpdateGSTR1AAsync(item.gstperiod,
                Newtonsoft.Json.JsonConvert.SerializeObject(GSTR1)).Result;

            if (result.IsSuccess)
            {
                item.gsttype = "GSTR1-UPLOAD";
                item.createdby = this.Context.UserID;
                item.createdon = DateTimeNow;
                item.companyid = item.id;
                item.gstno = GSTPortal.GSTN;
                item.gstadata_json = result.data;
                item.version = 1;
                item.isactive = true;
                item.apiresponse = result.ReferenceID;
                Insert(item);
                throw new Exception(result.TxnOutcome);
            }
            else
            {
                throw new Exception(string.IsNullOrEmpty(result.TxnOutcome) ? "Your session expire, please connect again.." : result.TxnOutcome);
            }
        }

        internal bool RequestOTP(Gst_InboxCreteria item)
        {
            var GSTPortal = GetService(item.id);
            var result = GSTPortal.GetOTPAsync().Result;
            return result.IsSuccess;
        }

        public SalesTransaction PrepareGSTR1A(Gst_Inbox item)
        {
            string query = @"
with exportbill as
(
    select distinct bill.id from bill 
    left outer join billadditionalcharges on billadditionalcharges.billid = bill.id and chargetype ='EXPORTGST' AND billadditionalcharges.isactive=true
    where billtype!=9 and bill.companyid=@companyid and billdate>=@fromdate and billdate<@todate
    and (billadditionalcharges.id is not null or bill.isexport=true)
)

select bill.billtype, bill.companyid,states.name pos,customer.name customer, bill.billno,
bill.billdate,billitems.qty,bill.isinterstatesale,billitems.taxpercentage,billitems.tax,customer.gstno,
billitems.receivable from bill
inner join billitems on billitems.billid = bill.id
inner join customer on customer.id = bill.customerid
left outer join exportbill on exportbill.id = bill.id
left outer join referencelist states on states.id = customer.stateid
where bill.companyid = @companyid and billdate>=@gstfromdate and billdate<@gsttodate 
and bill.billtype != 9 and exportbill.id is null
and bill.isb2b=true and customer.gstno != ''";

            List<B2BInvoice> b2b = new List<B2BInvoice>();
            List<B2CInvoice> b2c = new List<B2CInvoice>();
            List<B2CLarge> b2clarge = new List<B2CLarge>();
            List<B2CSmall> b2csmall = new List<B2CSmall>();
            List<B2CExport> b2cexport = new List<B2CExport>();
            List<B2BInvoice> b2bsr = new List<B2BInvoice>();
            List<B2BInvoice> b2brt = new List<B2BInvoice>();
            List<HSNCode> hsncode = new List<HSNCode>();
            List<HSNCode> whsncode = new List<HSNCode>();

            SortedList<string, object> parameter = new SortedList<string, object>();
            var gstfromdate = BaseEntity.GetDataFromString("01-" + item.gstperiod.Substring(0, 2) + "-" + item.gstperiod.Substring(2, 4));
            var gsttodate = gstfromdate.AddMonths(1);

            CompanyDataService companyDataService = new CompanyDataService();
            var company = companyDataService.Search(new CompanyCreteria()
            {
                id = item.companyid
            }).First();

            var defaultstatecode = "";
            var defaultstatename = "";
            if (!string.IsNullOrEmpty(company.gstno))
            {
                defaultstatecode = company.gstno.Substring(0, 2);
            }
            ReferencelistDataService referencelistDataService = new ReferencelistDataService();
            if (company.stateid > 0)
            {
                var states = referencelistDataService.SearchList(new ReferencelistCreteria()
                {
                    id = company.stateid
                }).First();
                defaultstatename = states.name;
            }


            parameter.Add("defaultstatecode", defaultstatecode);
            parameter.Add("defaultstatename", defaultstatename);
            parameter.Add("largebillvalue", 250000);
            parameter.Add("gstfromdate", gstfromdate);
            parameter.Add("fromdate", gstfromdate);
            parameter.Add("todate", gsttodate);
            parameter.Add("gsttodate", gsttodate);
            parameter.Add("companyid", item.id);
            using (var reader = ExecuteReader(ApplyB2BCondition(query), parameter))
            {
                while (reader.Read())
                {
                    b2b.Add(new B2BInvoice()
                    {
                        billdate = reader["billdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["billdate"]),
                        billno = reader["billno"] == DBNull.Value ? "" : Convert.ToString(reader["billno"]),
                        gstno = reader["gstno"] == DBNull.Value ? "" : Convert.ToString(reader["gstno"]),
                        customer = reader["customer"] == DBNull.Value ? "" : Convert.ToString(reader["customer"]),
                        billtype = reader["billtype"] == DBNull.Value ? 0 : Convert.ToInt32(reader["billtype"]),
                        isinterstatesale = reader["isinterstatesale"] == DBNull.Value ? false : Convert.ToBoolean(reader["isinterstatesale"]),
                        pos = reader["pos"] == DBNull.Value ? "" : Convert.ToString(reader["pos"]),
                        qty = reader["qty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["qty"]),
                        receivable = reader["receivable"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["receivable"]),
                        tax = reader["tax"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["tax"]),
                        companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"]),
                        taxpercentage = reader["taxpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["taxpercentage"]),
                    });
                }
            }

            query = @"with largebill as
            (
                select bill.id,billdate,billno,customerid,bill.isinterstatesale,isb2b from bill 
                where billtype!=9 and bill.isinterstatesale=true 
                and bill.companyid=@companyid and receivable>@largebillvalue and billdate>=@fromdate and billdate<@todate
            ),
            exportbills as
            (
                select distinct bill.id from bill 
                left outer join billadditionalcharges on billadditionalcharges.billid = bill.id and chargetype ='EXPORTGST' AND billadditionalcharges.isactive=true
                where billtype!=9 and bill.companyid=@companyid  and billdate>=@fromdate and billdate<@todate
                and (billadditionalcharges.id is not null or bill.isexport=true)
            ),solddata as
            (
	            select billid,taxpercentage,sum(billitems.tax) tax,sum(billitems.receivable) receivable,largebill.billdate::date,
                largebill.billno,referencelist.code poscode,referencelist.name posname,
                case when customer.billingname != '' then customer.billingname else customer.name end customername from largebill
	            inner join billitems on billitems.billid  = largebill.id
                left outer join customer on customer.id = largebill.customerid
                left outer join referencelist on referencelist.id = customer.stateid
                left outer join exportbills on exportbills.id = largebill.id
                WHERE billitems.isactive=true 
                and (customer.id is null or customer.gstno is null or customer.gstno = '') and largebill.isb2b=false
                and exportbills.id is null
	            group by billid,taxpercentage,largebill.billdate,largebill.billno,referencelist.code,referencelist.name,
                case when customer.billingname != '' then customer.billingname else customer.name end 
            )
            select solddata.customername,solddata.billno,solddata.billdate,
            solddata.tax,solddata.receivable,solddata.taxpercentage,
            poscode,posname from bill
            inner join solddata on solddata.billid = bill.id
            where bill.billdate>=@fromdate and bill.billdate<@todate and billtype!=9  and bill.isactive=true";

            using (var reader = ExecuteReader(ApplyB2BCondition(query), parameter))
            {
                while (reader.Read())
                {
                    b2clarge.Add(new B2CLarge()
                    {
                        billdate = reader["billdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["billdate"]),
                        billno = reader["billno"] == DBNull.Value ? "" : Convert.ToString(reader["billno"]),
                        customer = reader["customername"] == DBNull.Value ? "" : Convert.ToString(reader["customername"]),
                        pos = reader["poscode"] == DBNull.Value ? "" : Convert.ToString(reader["poscode"]),
                        receivable = reader["receivable"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["receivable"]),
                        tax = reader["tax"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["tax"]),
                        taxpercentage = reader["taxpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["taxpercentage"]),
                    });
                }
            }

            query = @"with largebill as
(
    select bill.id,billdate,billno,customerid,bill.isinterstatesale,counterid,ADDITIONALINFO->>'isonline' isecommerce,
    ADDITIONALINFO->>'formula' ecommercegstno from bill 
    inner join referencelist on referencelist.id = bill.counterid
    left outer join customer on customer.id = customerid
    where billtype!=9 and bill.companyid=@companyid  and billdate>=@fromdate and billdate<@todate
    and (customer.id is null or customer.gstno is null or customer.gstno = '') and bill.isb2b=false
    and (bill.isinterstatesale=false or receivable<=@largebillvalue) and bill.isactive=true
),exportbills as
(
    select distinct bill.id from bill 
    left outer join billadditionalcharges on billadditionalcharges.billid = bill.id and chargetype ='EXPORTGST' AND billadditionalcharges.isactive=true
    where billtype!=9 and bill.companyid=@companyid  and billdate>=@fromdate and billdate<@todate
    and (billadditionalcharges.id is not null or bill.isexport=true)
),solddata as
(
    select taxpercentage,sum(billitems.tax) tax,sum(billitems.receivable) receivable,
	referencelist.code poscode,referencelist.name posname,ecommercegstno,largebill.isinterstatesale from billitems
	inner join largebill on largebill.id = billitems.billid 
    left outer join exportbills on exportbills.id = largebill.id
    left outer join customer on customer.id = largebill.customerid
    left outer join referencelist on referencelist.id = customer.stateid
    WHERE billitems.isactive=true and exportbills.id is null
	group by taxpercentage,	referencelist.code,referencelist.name,ecommercegstno,largebill.isinterstatesale
)

select solddata.taxpercentage,
case when (poscode is null or isinterstatesale=false) then @defaultstatecode else poscode end pos,
sum(solddata.tax) tax,
sum(solddata.receivable) receivable
from solddata
group by solddata.taxpercentage,
case when (poscode is null or isinterstatesale=false) then @defaultstatecode else poscode end ";

            using (var reader = ExecuteReader(ApplyB2BCondition(query), parameter))
            {
                while (reader.Read())
                {
                    b2csmall.Add(new B2CSmall()
                    {
                        pos = reader["pos"] == DBNull.Value ? "" : Convert.ToString(reader["pos"]),
                        receivable = reader["receivable"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["receivable"]),
                        tax = reader["tax"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["tax"]),
                        taxpercentage = reader["taxpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["taxpercentage"]),
                    });
                }
            }


            query = @"with largebill as
(
    select distinct bill.id,billdate,billno,customerid from bill 
    left outer join billadditionalcharges on billadditionalcharges.billid = bill.id and chargetype ='EXPORTGST' AND billadditionalcharges.isactive=true
    where billtype!=9 and bill.companyid=@companyid  and billdate>=@fromdate and billdate<@todate and bill.isactive=true
    and (billadditionalcharges.id is not null or bill.isexport=true) and bill.isb2b=false
),frightcharges as 
(
    select billadditionalcharges.billid,sum(charge) fright from largebill
    inner join billadditionalcharges on billadditionalcharges.billid=largebill.id
    and billadditionalcharges.chargetype ='FRIGHT'
    AND billadditionalcharges.isactive=true
    group by billadditionalcharges.billid
),solddata as
(
    select billitems.billid,sum(billitems.tax) tax,sum(billitems.receivable) receivable,
    taxpercentage,case when customer.billingname != '' then customer.billingname else customer.name end customername,billdate,billno from largebill
	inner join billitems on  billitems.billid  = largebill.id
    left outer join customer on customer.id = customerid
    left outer join referencelist on referencelist.id = customer.stateid
    WHERE (customer.id is null or customer.gstno is null or customer.gstno = '') and billitems.isactive=true
	group by  billitems.billid,taxpercentage,case when customer.billingname != '' then customer.billingname else customer.name end,billdate,billno
)

select solddata.taxpercentage,customername,billdate,billno,
sum(solddata.tax) tax,sum(solddata.receivable) receivable,
frightcharges.fright
from solddata
left outer join frightcharges on frightcharges.billid = solddata.billid
group by solddata.taxpercentage,customername,billdate,billno,frightcharges.fright";


            using (var reader = ExecuteReader(ApplyB2BCondition(query), parameter))
            {
                while (reader.Read())
                {

                    var billno = reader["billno"] == DBNull.Value ? "" : Convert.ToString(reader["billno"]);
                    var fright = reader["fright"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["fright"]);
                    if (!b2cexport.Exists(x => x.billno == billno && x.fright != 0))
                        fright = 0;

                    b2cexport.Add(new B2CExport()
                    {
                        fright = fright,
                        customer = reader["customername"] == DBNull.Value ? "" : Convert.ToString(reader["customername"]),
                        billno = reader["billno"] == DBNull.Value ? "" : Convert.ToString(reader["billno"]),
                        billdate = reader["billdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["billdate"]),
                        receivable = reader["receivable"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["receivable"]),
                        tax = reader["tax"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["tax"]),
                        taxpercentage = reader["taxpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["taxpercentage"]),
                    });
                }
            }

            query = @"with largebill as
(
    select bill.id,billdate,billno,customerid,bill.isinterstatesale,bill.counterid,ADDITIONALINFO->>'isonline' isecommerce,ADDITIONALINFO->>'formula' ecommercegstno from bill 
    inner join referencelist on referencelist.id = bill.counterid
    inner join customer on customer.id = bill.customerid
    where billtype=8 and bill.isactive=true and bill.customerid>0 AND bill.companyid=@companyid  and billdate>=@fromdate and billdate<@todate 
    AND customer.gstno is not null and customer.gstno != '' and bill.isb2b=true
),solddata as
(
	select billid,taxpercentage,sum(billitems.tax) tax,sum(billitems.receivable) receivable,largebill.billdate,
    largebill.billno,
    referencelist.code poscode,
    referencelist.name posname,
    ecommercegstno,isecommerce,customer.gstno,
    case when customer.billingname != '' then customer.billingname else customer.name end customername from largebill
	inner join billitems on billitems.billid  = largebill.id
    left outer join customer on customer.id = customerid
    left outer join referencelist on referencelist.id = customer.stateid
    where billitems.isactive=true
    group by billid,taxpercentage,largebill.billdate,largebill.billno,referencelist.code,ecommercegstno,isecommerce,referencelist.name,
    case when customer.billingname != '' then customer.billingname else customer.name end,customer.gstno
)
select customername,solddata.billno,solddata.billdate,
solddata.tax,solddata.receivable-solddata.tax taxable,solddata.receivable,solddata.taxpercentage,
poscode,posname,gstno,isinterstatesale from bill
inner join solddata on solddata.billid = bill.id
where bill.isactive=true";

            using (var reader = ExecuteReader(ApplyB2BCondition(query), parameter))
            {
                while (reader.Read())
                {
                    b2bsr.Add(new B2BInvoice()
                    {
                        isinterstatesale = reader["isinterstatesale"] == DBNull.Value ? false : Convert.ToBoolean(reader["isinterstatesale"]),
                        billdate = reader["billdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["billdate"]),
                        billno = reader["billno"] == DBNull.Value ? "" : Convert.ToString(reader["billno"]),
                        gstno = reader["gstno"] == DBNull.Value ? "" : Convert.ToString(reader["gstno"]),
                        customer = reader["customername"] == DBNull.Value ? "" : Convert.ToString(reader["customername"]),
                        pos = reader["poscode"] == DBNull.Value ? "" : Convert.ToString(reader["poscode"]),
                        receivable = reader["receivable"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["receivable"]),
                        tax = reader["tax"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["tax"]),
                        taxpercentage = reader["taxpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["taxpercentage"]),
                    });
                }
            }


            query = @"SELECT SUPPLIER.NAME customername,
SUPPLIERCOMPANY.TINANDDATE GSTNO,RETURNDATE,LRINVOICERETURN.RETURNNO INVOICENO,
CASE WHEN LRINVOICERETURN.returntype = 0 THEN 'D' ELSE 'C' END cdnrtype,REFERENCELIST.CODE POS,
LRINVOICERETURNITEMS.TAXPERCENTAGE,
LRINVOICERETURN.isinterstatesale,
sum(LRINVOICERETURNITEMS.QTY) qty,
sum(LRINVOICERETURNITEMS.TAX) tax,
sum(LRINVOICERETURNITEMS.AMOUNT) total
FROM LRINVOICERETURN 
INNER JOIN SUPPLIER ON SUPPLIER.ID = LRINVOICERETURN.SUPPLIERID
INNER JOIN SUPPLIERCOMPANY ON SUPPLIERCOMPANY.SUPPLIERID = LRINVOICERETURN.SUPPLIERID
INNER JOIN REFERENCELIST ON REFERENCELIST.ID = SUPPLIERCOMPANY.STATEID
INNER JOIN LRINVOICERETURNITEMS ON LRINVOICERETURNITEMS.LRINVOICERETURNID = LRINVOICERETURN.ID 
where RETURNDATE>=@fromdate and RETURNDATE<@todate and LRINVOICERETURN.isactive=true AND LRINVOICERETURNITEMS.ISACTIVE=TRUE 
and  LRINVOICERETURN.companyid=@companyid and (LRINVOICERETURN.returntype IN (0,5))
group by SUPPLIER.NAME,LRINVOICERETURN.isinterstatesale,
SUPPLIERCOMPANY.TINANDDATE ,RETURNDATE,REFERENCELIST.CODE ,REFERENCELIST.NAME ,
LRINVOICERETURNITEMS.TAXPERCENTAGE,LRINVOICERETURN.RETURNNO,LRINVOICERETURN.returntype";


            using (var reader = ExecuteReader(query, parameter))
            {
                while (reader.Read())
                {
                    b2brt.Add(new B2BInvoice()
                    {
                        isinterstatesale = reader["isinterstatesale"] == DBNull.Value ? false : Convert.ToBoolean(reader["isinterstatesale"]),
                        billdate = reader["RETURNDATE"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["RETURNDATE"]),
                        billno = reader["INVOICENO"] == DBNull.Value ? "" : Convert.ToString(reader["INVOICENO"]),
                        gstno = reader["gstno"] == DBNull.Value ? "" : Convert.ToString(reader["gstno"]),
                        customer = reader["customername"] == DBNull.Value ? "" : Convert.ToString(reader["customername"]),
                        pos = reader["pos"] == DBNull.Value ? "" : Convert.ToString(reader["pos"]),
                        receivable = reader["total"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["total"]),
                        tax = reader["tax"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["tax"]),
                        taxpercentage = reader["taxpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["taxpercentage"]),
                    });
                }
            }

            /*query = @"select bill.billtype, bill.companyid,states.name pos,customer.name customer, bill.billno,
bill.billdate,billitems.qty,bill.isinterstatesale,billitems.taxpercentage,billitems.tax,customer.gstno,
billitems.receivable from bill
inner join billitems on billitems.billid = bill.id
inner join customer on customer.id = bill.customerid
left outer join referencelist states on states.id = customer.stateid
where bill.companyid = @companyid and billdate>=@gstfromdate and billdate<@gsttodate 
and customer.gstno != '' and bill.billtype != 9";

            using (var reader = ExecuteReader(query, parameter))
            {
                while (reader.Read())
                {
                    b2b.Add(new B2BInvoice()
                    {
                        billdate = reader["billdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["billdate"]),
                        billno = reader["billno"] == DBNull.Value ? "" : Convert.ToString(reader["billno"]),
                        gstno = reader["gstno"] == DBNull.Value ? "" : Convert.ToString(reader["gstno"]),
                        customer = reader["customer"] == DBNull.Value ? "" : Convert.ToString(reader["customer"]),
                        billtype = reader["billtype"] == DBNull.Value ? 0 : Convert.ToInt32(reader["billtype"]),
                        isinterstatesale = reader["isinterstatesale"] == DBNull.Value ? false : Convert.ToBoolean(reader["isinterstatesale"]),
                        pos = reader["pos"] == DBNull.Value ? "" : Convert.ToString(reader["pos"]),
                        qty = reader["qty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["qty"]),
                        receivable = reader["receivable"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["receivable"]),
                        tax = reader["tax"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["tax"]),
                        companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"]),
                        taxpercentage = reader["taxpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["taxpercentage"]),
                    });
                }
            }*/

            query = @"select states.code pos,bill.isinterstatesale,billitems.taxpercentage,sum(billitems.tax) tax,
sum(billitems.receivable) receivable from bill
inner join billitems on billitems.billid = bill.id
left outer join customer on customer.id = bill.customerid
left outer join referencelist states on states.id = customer.stateid
where bill.companyid = @companyid and billdate>=@gstfromdate and billdate<@gsttodate 
and bill.billtype != 9
and coalesce(customer.gstno,'') = '' and bill.isb2b=false 
group by states.code ,bill.isinterstatesale,billitems.taxpercentage";
            using (var reader = ExecuteReader(ApplyB2BCondition(query), parameter))
            {
                while (reader.Read())
                {
                    b2c.Add(new B2CInvoice()
                    {
                        isinterstatesale = reader["isinterstatesale"] == DBNull.Value ? false : Convert.ToBoolean(reader["isinterstatesale"]),
                        pos = reader["pos"] == DBNull.Value ? "" : Convert.ToString(reader["pos"]),
                        receivable = reader["receivable"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["receivable"]),
                        tax = reader["tax"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["tax"]),
                        taxpercentage = reader["taxpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["taxpercentage"]),
                    });
                }
            }




            query = @"select billitems.taxpercentage,bill.isinterstatesale,
case when products.sellingmode = 3 then 'MTR' else 'NOS' end as UQC,
case when stock.hsncode is null or stock.hsncode = '' then products.hsncode else stock.hsncode end as hsncode,
sum(billitems.tax) tax, sum(billitems.qty) qty,
sum(billitems.receivable) receivable from bill
inner join billitems on billitems.billid = bill.id
left outer join stock on stock.id = billitems.stockid
left outer join products on products.id = stock.productid
left outer join customer on customer.id = bill.customerid
left outer join referencelist states on states.id = customer.stateid
where bill.companyid = @companyid and billdate>=@gstfromdate and billdate<@gsttodate 
and bill.billtype != 9
group by billitems.taxpercentage,bill.isinterstatesale,
case when products.sellingmode = 3 then 'MTR' else 'NOS' end,
case when stock.hsncode is null or stock.hsncode = '' then products.hsncode else stock.hsncode end";

            using (var reader = ExecuteReader(query, parameter))
            {
                while (reader.Read())
                {
                    hsncode.Add(new HSNCode()
                    {
                        isinterstatesale = reader["isinterstatesale"] == DBNull.Value ? false : Convert.ToBoolean(reader["isinterstatesale"]),
                        UQC = reader["UQC"] == DBNull.Value ? "" : Convert.ToString(reader["UQC"]),
                        hsncode = reader["hsncode"] == DBNull.Value ? "" : Convert.ToString(reader["hsncode"]),
                        receivable = reader["receivable"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["receivable"]),
                        qty = reader["qty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["qty"]),
                        tax = reader["tax"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["tax"]),
                        taxpercentage = reader["taxpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["taxpercentage"]),
                    });
                }
            }

            /*hsncode.RemoveAll(e => e.qty == 0);
            var invalidhsns = hsncode.FindAll(e => e.hsncode.Length < 6);
            hsncode.RemoveAll(e => e.hsncode.Length < 6);
            foreach (var invalid in invalidhsns)
            {
                var similer = hsncode.FindAll(e => e.isinterstatesale == invalid.isinterstatesale && e.taxpercentage == invalid.taxpercentage && e.hsncode.StartsWith(invalid.hsncode));
                if (similer.Count > 0)
                {
                    var adjust = similer.OrderBy(e => e.hsncode).First();
                    adjust.receivable += invalid.receivable;
                    adjust.qty += invalid.qty;
                    adjust.tax += invalid.tax;
                }
                else
                {
                    whsncode.Add(invalid);
                }
            }
            var negatives = hsncode.FindAll(e => e.qty < 0);
            foreach (var n in negatives)
            {
                string prefix = "";
                if (n.hsncode.Length > 5)
                    prefix = n.hsncode.Substring(0, 5);
                var similer = hsncode.FindAll(e => e.taxpercentage == n.taxpercentage && e.hsncode.StartsWith(prefix));
                if (similer.Count > 0)
                {
                    var adjust = similer.OrderBy(e => e.receivable).Last();
                    if (adjust.receivable > n.receivable * -1)
                    {
                        adjust.receivable += n.receivable;
                        adjust.qty += n.qty;
                        adjust.tax += n.tax;
                        n.receivable = 0;
                        n.tax = 0;
                        n.qty = 0;
                    }
                }
                if (n.receivable < 0)
                {
                    prefix = n.hsncode.Substring(0, 4);
                    similer = hsncode.FindAll(e => e.taxpercentage == n.taxpercentage && e.hsncode.StartsWith(prefix));
                    if (similer.Count > 0)
                    {
                        var adjust = similer.OrderBy(e => e.receivable).Last();
                        if (adjust.receivable > n.receivable * -1)
                        {
                            adjust.receivable += n.receivable;
                            adjust.qty += n.qty;
                            adjust.tax += n.tax;
                            n.receivable = 0;
                            n.tax = 0;
                            n.qty = 0;
                        }
                    }
                }

                if (n.receivable < 0)
                {
                    whsncode.Add(n);
                }
            }
            hsncode.RemoveAll(e => e.qty == 0);*/

            return new SalesTransaction()
            {
                B2B = b2b,
                B2C = b2c,
                B2CLarge = b2clarge,
                B2CExport = b2cexport,
                B2CSmall = b2csmall,
                B2BCN = b2bsr,
                B2BDN = b2brt,
                HSNCodes = hsncode,
                WrongHSN = whsncode
            };
        }
        internal bool ApplyOTP(Gst_InboxCreteria item)
        {
            var GSTPortal = GetService(item.id);

            var result = GSTPortal.ApplyOTPAsync(item.gstotp).Result;
            return result.IsSuccess;
        }
        internal Gst_Inbox DownloadGSTR2B(Gst_Inbox item)
        {
            var GSTPortal = GetService(item.id);

            var downloaddata = GSTPortal.DownloadGSTR2BAsync(item.gstperiod).Result;
            if (downloaddata.IsSuccess)
            {
                item.gsttype = "GSTR2B";
                item.createdby = this.Context.UserID;
                item.companyid = item.id;
                item.createdon = DateTimeNow;
                item.gstno = GSTPortal.GSTN;
                item.gstadata_json = downloaddata.data;
                item.version = 1;
                item.isactive = true;
                item.apiresponse = downloaddata.TxnOutcome;
                return Insert(item);
            }
            throw new Exception(downloaddata.TxnOutcome);
        }


        internal Gst_Inbox ValidateHSN(Gst_Inbox item)
        {
            var GSTPortal = GetService(item.id);

            var result = GSTPortal.ValidateHSN(item.gstperiod).Result;

            if (result.IsSuccess)
            {
                item.gsttype = "GSTR1-HSN";
                item.createdby = this.Context.UserID;
                item.createdon = DateTimeNow;
                item.companyid = item.id;
                item.gstno = GSTPortal.GSTN;
                item.gstadata_json = result.data;
                item.version = 1;
                item.isactive = true;
                item.apiresponse = result.ReferenceID;
                throw new Exception(result.TxnOutcome);
            }
            else
            {
                throw new Exception(string.IsNullOrEmpty(result.TxnOutcome) ? "Your session expire, please connect again.." : result.TxnOutcome);
            }
        }
        internal List<GST_Expense> Import(List<GST_Expense> items)
        {
            CompanyDataService companyDataService = new CompanyDataService() { Context = this.Context };
            using (NpgsqlConnection conn = CreateConnection())
            {
                conn.Open();
                foreach (var gstno in items)
                {
                    var gstdata = companyDataService.InsertImport(conn, gstno);
                }
                conn.Close();
            }
            return items;
        }

        internal List<GST_Exclude> Exculde(List<GST_Exclude> items)
        {
            CompanyDataService companyDataService = new CompanyDataService() { Context = this.Context };
            using (NpgsqlConnection conn = CreateConnection())
            {
                conn.Open();
                foreach (var gstno in items)
                {
                    var gstdata = companyDataService.InsertExclude(conn, gstno);
                }
                conn.Close();
            }
            return items;
        }

        public InfyPOS.Models.GST.GSTR1A BuildGSTR1(Gst_Inbox item)
        {
            var GSTPortal = GetService(item.id);
            var salestransaction = PrepareGSTR1A(item);
            InfyPOS.Models.GST.GSTR1A response = new Models.GST.GSTR1A()
            {
                Gstin = GSTPortal.GSTN,
                Fp = item.gstperiod,
                Version = "GST3.0.4",
                Hash = "hash",
                //DocIssue = new Models.GST.DocIssue() { }
            };
            if (salestransaction.B2B != null && salestransaction.B2B.Count > 0)
            {
                var b2bList = new List<Models.GST.B2B>();
                foreach (var supplier in salestransaction.B2B.GroupBy(e => e.gstno))
                {
                    var b2bGST = new Models.GST.B2B() { Ctin = supplier.Key };
                    var b2bInvoices = new List<Models.GST.B2B.Invoices>();
                    foreach (var invoice in supplier.GroupBy(e => e.billno))
                    {
                        Models.GST.B2B.Invoices invoices = new Models.GST.B2B.Invoices()
                        {
                            Idt = invoice.First().billdate.Date.ToString("dd-MM-yyyy"),
                            Inum = invoice.Key,
                            InvTyp = "R",
                            Pos = supplier.Key.Substring(0, 2),
                            Rchrg = "N",
                            Val = Convert.ToInt64(invoice.Sum(e => e.receivable)),
                        };
                        b2bInvoices.Add(invoices);
                        var taxList = new List<Models.GST.B2B.Itm>();
                        int taxid = 1;
                        foreach (var tax in invoice.GroupBy(e => e.taxpercentage))
                        {
                            Models.GST.B2B.Itm itm = new Models.GST.B2B.Itm() { Num = taxid++ };
                            if (tax.Key == 0)
                            {
                                itm.ItmDet = new Models.GST.B2B.ItmDet()
                                {
                                    Rt = Convert.ToInt32(tax.Key),
                                    Txval = Convert.ToInt64(tax.Sum(e => e.receivable - e.tax)),
                                };
                            }
                            else
                            {
                                var taxPart = Math.Round(tax.Sum(e => e.tax / 2), 2);
                                var taxable = taxPart * 2 / tax.Key * 100;
                                itm.ItmDet = new Models.GST.B2B.ItmDet()
                                {
                                    Rt = Convert.ToInt32(tax.Key),
                                    Txval = Convert.ToInt64(tax.Sum(e => e.receivable - e.tax)),
                                    Camt = !tax.First().isinterstatesale ? Convert.ToDecimal(tax.Sum(e => e.tax / 2)) : 0,
                                    Samt = !tax.First().isinterstatesale ? Convert.ToDecimal(tax.Sum(e => e.tax / 2)) : 0,
                                    Iamt = tax.First().isinterstatesale ? Convert.ToDecimal(tax.Sum(e => e.tax)) : 0,
                                };
                                itm.ItmDet.Txval = Math.Round(taxable, 2);
                                if (tax.First().isinterstatesale)
                                {
                                    itm.ItmDet.Iamt = taxPart * 2;
                                }
                                else
                                {
                                    itm.ItmDet.Camt = taxPart;
                                    itm.ItmDet.Samt = taxPart;
                                }
                            }
                            taxList.Add(itm);
                        }
                        invoices.Itms = taxList.ToArray();
                    }
                    b2bGST.Inv = b2bInvoices.ToArray();
                    b2bList.Add(b2bGST);
                }
                response.B2B = b2bList.ToArray();
            }

            if (salestransaction.B2CExport != null && salestransaction.B2CExport.Count > 0)
            {
                var b2bList = new List<Models.GST.Exp>();
                foreach (var supplier in salestransaction.B2CExport.GroupBy(e => e.type))
                {
                    var b2bGST = new Models.GST.Exp() { exp_typ = supplier.Key };
                    var b2bInvoices = new List<Models.GST.Exp.Invoice>();
                    foreach (var invoice in supplier.GroupBy(e => new { e.billno, e.billdate }))
                    {
                        Models.GST.Exp.Invoice invoices = new Models.GST.Exp.Invoice()
                        {
                            idt = invoice.Key.billdate.Date.ToString("dd-MM-yyyy"),
                            inum = invoice.Key.billno,
                            val = Convert.ToInt64(invoice.Sum(e => e.receivable)),
                        };

                        b2bInvoices.Add(invoices);
                        var taxList = new List<Models.GST.Exp.Invoice.Itm>();
                        foreach (var tax in invoice.GroupBy(e => e.taxpercentage))
                        {
                            Models.GST.Exp.Invoice.Itm itm = new Models.GST.Exp.Invoice.Itm();
                            if (tax.Key > 0)
                            {
                                var taxPart = Math.Round(tax.Sum(e => e.tax / 2), 2);
                                var taxable = taxPart * 2 / tax.Key * 100;
                                itm.txval = Convert.ToInt64(tax.Sum(e => e.receivable - e.tax));
                                itm.rt = Convert.ToInt32(tax.Key);
                            }
                            else
                            {
                                itm.txval = Convert.ToInt64(tax.Sum(e => e.receivable - e.tax));
                                itm.rt = Convert.ToInt32(tax.Key);
                            }
                            taxList.Add(itm);
                        }
                        invoices.itms = taxList.ToArray();
                    }
                    b2bGST.inv = b2bInvoices.ToArray();
                    b2bList.Add(b2bGST);
                }
                response.Exports = b2bList.ToArray();
            }

            if (salestransaction.B2CLarge != null && salestransaction.B2CLarge.Count > 0)
            {
                var b2bList = new List<Models.GST.B2cl>();
                foreach (var supplier in salestransaction.B2CLarge.GroupBy(e => e.pos))
                {
                    var b2bGST = new Models.GST.B2cl() { pos = supplier.Key };
                    var b2bInvoices = new List<Models.GST.B2cl.Invoice>();
                    foreach (var invoice in supplier.GroupBy(e => new { e.billno, e.billdate }))
                    {
                        Models.GST.B2cl.Invoice invoices = new Models.GST.B2cl.Invoice()
                        {
                            idt = invoice.Key.billdate.Date.ToString("dd-MM-yyyy"),
                            inum = invoice.Key.billno,
                            val = Convert.ToInt64(invoice.Sum(e => e.receivable)),
                        };
                        b2bInvoices.Add(invoices);
                        int taxid = 1;
                        var taxList = new List<Models.GST.B2cl.Invoice.Item>();
                        foreach (var tax in invoice.GroupBy(e => e.taxpercentage))
                        {
                            Models.GST.B2cl.Invoice.Item itm = new Models.GST.B2cl.Invoice.Item() { num = taxid++ };

                            decimal taxPart = 0;
                            decimal taxable = 0;
                            if (tax.Key > 0)
                            {
                                taxPart = Math.Round(tax.Sum(e => e.tax / 2), 2);
                                taxable = taxPart * 2 / tax.Key * 100;
                            }
                            itm.itm_det = new B2cl.Invoice.Item.ItmDet()
                            {
                                rt = Convert.ToInt32(tax.Key),
                                txval = Convert.ToInt64(tax.Sum(e => e.receivable - e.tax)),
                                camt = tax.Key > 0 && !tax.First().isinterstatesale ? Convert.ToDecimal(tax.Sum(e => e.tax / 2)) : 0,
                                samt = tax.Key > 0 && !tax.First().isinterstatesale ? Convert.ToDecimal(tax.Sum(e => e.tax / 2)) : 0,
                                iamt = tax.Key > 0 && tax.First().isinterstatesale ? Convert.ToDecimal(tax.Sum(e => e.tax)) : 0,
                            };
                            itm.itm_det.txval = Math.Round((double)taxable, 2);
                            if (tax.First().isinterstatesale)
                            {
                                itm.itm_det.iamt = taxPart * 2;
                            }
                            else
                            {
                                itm.itm_det.camt = taxPart;
                                itm.itm_det.samt = taxPart;
                            }
                            taxList.Add(itm);
                        }
                        invoices.itms = taxList.ToArray();
                    }
                    b2bGST.inv = b2bInvoices.ToArray();
                    b2bList.Add(b2bGST);
                }
                response.B2Cl = b2bList.ToArray();
            }

            if (salestransaction.B2C != null && salestransaction.B2C.Count > 0)
            {
                var b2bList = new List<Models.GST.B2C>();
                foreach (var supplier in salestransaction.B2C.GroupBy(e => e.taxpercentage))
                {
                    var taxPart = Math.Round(supplier.Sum(e => e.tax) / 2, 2);
                    var taxable = taxPart == 0 ? supplier.Sum(x => x.receivable) : Math.Round(taxPart * 2 / supplier.Key * 100, 2);

                    b2bList.Add(new Models.GST.B2C()
                    {
                        Csamt = 0,
                        Pos = GSTPortal.GSTN.Substring(0, 2),
                        SplyTy = "INTRA",
                        Typ = "OE",
                        Rt = Convert.ToInt32(supplier.Key),
                        Txval = taxable,
                        Camt = taxPart,
                        Samt = taxPart
                    });
                }
                response.B2Cs = b2bList.ToArray();
            }
            if (salestransaction.HSNCodes != null && salestransaction.HSNCodes.Count > 0)
            {
                var hsnlist = new List<Models.GST.Hsn.Datum>();
                foreach (var hsn in salestransaction.HSNCodes)
                {
                    var taxPart = Math.Round(hsn.tax / 2, 2);
                    var taxable = taxPart == 0 ? hsn.receivable : taxPart * 2 / hsn.taxpercentage * 100;
                    hsnlist.Add(new Models.GST.Hsn.Datum()
                    {
                        Num = hsnlist.Count + 1,
                        Rt = Convert.ToInt32(hsn.taxpercentage),
                        Uqc = hsn.UQC,
                        Csamt = 0,
                        HsnSc = hsn.hsncode,
                        Qty = Convert.ToInt64(hsn.qty),
                        Txval = Convert.ToInt32(hsn.receivable - hsn.tax),
                        Camt = !hsn.isinterstatesale ? taxPart : 0,
                        Samt = !hsn.isinterstatesale ? taxPart : 0,
                        Iamt = hsn.isinterstatesale ? taxPart * 2 : 0
                    });
                }
                int num = 1;
                response.Hsn = new Models.GST.Hsn()
                {
                    Data = hsnlist.ToList().GroupBy(e => new { e.HsnSc, e.Uqc, e.Rt }).ToList().ConvertAll(e => new Models.GST.Hsn.Datum()
                    {
                        Num = num++,
                        Uqc = e.Key.Uqc,
                        Rt = e.Key.Rt,
                        HsnSc = e.Key.HsnSc,
                        Camt = e.Sum(ex => ex.Camt),
                        Csamt = e.Sum(ex => ex.Csamt),
                        Iamt = e.Sum(ex => ex.Iamt),
                        Qty = e.Sum(ex => ex.Qty),
                        Samt = e.Sum(ex => ex.Samt),
                        Txval = e.Sum(ex => ex.Txval)
                    }).ToArray()
                };
            }
            return response;
        }

        public Models.GST.DocDet CreateDocument(Models.GST.DocDet.Doc document, int number, string type)
        {
            var doc1 = new Models.GST.DocDet()
            {
                DocNum = number,
                DocTyp = type
            };
            doc1.Docs = new Models.GST.DocDet.Doc[1];
            doc1.Docs[0] = document;
            return doc1;
        }

        internal List<GST_Reconciliation> LocalSearch(Gst_InboxLocal item)
        {
            SortedList<string, object> parameter = new SortedList<string, object>();
            parameter.Add("gstfromdate", item.fromdate);
            parameter.Add("gsttodate", item.todate.AddDays(1));
            parameter.Add("companyid", item.id);

            List<GST_Reconciliation> result = null;
            if (item.type == "supplier" && item.mode == "filed")
            {
                result = Parse(true, GSTR2B_Filed + " and lrinvoice.companyid=@companyid", "Filed", parameter);
            }
            else if (item.type == "supplier" && item.mode == "nonfiled")
            {
                result = Parse(true, GSTR2B_NoFiled + " and lrinvoice.companyid=@companyid", "Non Filed", parameter);
            }
            else
            {
                string query = item.mode == "filed" ?
                    (GSTR1_Filed + " and Filed.gstno is not null") :
                    (GSTR1_Filed + " and Filed.gstno is null");
                result = new List<GST_Reconciliation>();
                using (var reader = ExecuteReader(query, parameter))
                {
                    while (reader.Read())
                    {
                        var reconcilation = new GST_Reconciliation()
                        {
                            invoicedate = reader["billdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["billdate"]),
                            invoiceno = reader["billno"] == DBNull.Value ? "" : Convert.ToString(reader["billno"]),
                            gstno = reader["gstno"] == DBNull.Value ? "" : Convert.ToString(reader["gstno"]),
                            suppliername = reader["customer"] == DBNull.Value ? "" : Convert.ToString(reader["customer"]),
                            isinterstatesale = reader["isinterstatesale"] == DBNull.Value ? false : Convert.ToBoolean(reader["isinterstatesale"]),
                            count = reader["qty"] == DBNull.Value ? 0 : Convert.ToInt32(reader["qty"]),
                            taxable = reader["receivable"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["receivable"]),
                            tax = reader["tax"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["tax"]),
                        };

                        if (item.mode == "filed")
                        {
                            reconcilation.taxable = reader["filedreceivable"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["filedreceivable"]);
                            reconcilation.tax = reader["filedtax"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["filedtax"]);
                        }
                        reconcilation.taxable -= reconcilation.tax;
                        result.Add(reconcilation);
                    }
                }

            }
            return result;
        }

        internal Gst_Inbox DownloadGSTR1(Gst_Inbox item)
        {
            var GSTPortal = GetService(item.id);
            var downloaddata = GSTPortal.DownloadGSTR1AAsync(item.gstperiod).Result;
            if (downloaddata.Any(e => e.data != null && e.data.IsSuccess))
            {
                foreach (var kv in downloaddata)
                {
                    item.gsttype = "GSTR1A-" + kv.key;
                    item.createdby = this.Context.UserID;
                    item.createdon = DateTimeNow;
                    item.companyid = item.id;
                    item.gstno = GSTPortal.GSTN;
                    item.gstadata_json = kv.data.data;
                    item.version = 1;
                    if (kv.data.IsSuccess)
                    {
                        item.apiresponse = kv.data.TxnOutcome;
                        item.isactive = true;
                        Insert(item);
                    }
                }
            }
            else
            {
                var kv = downloaddata.First();
                item.gsttype = "GSTR1A-" + kv.key;
                item.createdby = this.Context.UserID;
                item.createdon = DateTimeNow;
                item.companyid = item.id;
                item.gstno = GSTPortal.GSTN;
                item.gstadata_json = kv.data.data;
                item.version = 1;
                item.apiresponse = kv.data.TxnOutcome;
                item.isactive = true;
            }
            return item;
        }

        internal SalesTransaction ShowGSTR1(Gst_Inbox item)
        {
            var gstfromdate = BaseEntity.GetDataFromString("01-" + item.gstperiod.Substring(0, 2) + "-" + item.gstperiod.Substring(2, 4));
            var gsttodate = gstfromdate.AddMonths(1);
            var parameter = new SortedList<string, object>();
            parameter.Add("gstfromdate", gstfromdate);
            parameter.Add("gsttodate", gsttodate);
            parameter.Add("companyid", item.id);
            var response = new SalesTransaction() { B2B = new List<B2BInvoice>() };

            using (var reader = ExecuteReader(GSTR1_Filed, parameter))
            {
                while (reader.Read())
                {
                    response.B2B.Add(new B2BInvoice()
                    {
                        billdate = reader["billdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["billdate"]),
                        billno = reader["billno"] == DBNull.Value ? "" : Convert.ToString(reader["billno"]),
                        gstno = reader["gstno"] == DBNull.Value ? "" : Convert.ToString(reader["gstno"]),
                        customer = reader["customer"] == DBNull.Value ? "" : Convert.ToString(reader["customer"]),
                        isinterstatesale = reader["isinterstatesale"] == DBNull.Value ? false : Convert.ToBoolean(reader["isinterstatesale"]),
                        pos = reader["pos"] == DBNull.Value ? "" : Convert.ToString(reader["pos"]),
                        qty = reader["qty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["qty"]),
                        receivable = reader["receivable"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["receivable"]),
                        tax = reader["tax"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["tax"]),
                        filedtax = reader["filedtax"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["filedtax"]),
                        filedreceivable = reader["filedreceivable"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["filedreceivable"]),
                        companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"])
                    });
                }
            }

            return response;
        }

        internal Gst_Inbox DownloadGSTR2A(Gst_Inbox item)
        {
            var GSTPortal = GetService(item.id);
            var downloaddata = GSTPortal.DownloadGSTR2AAsync(item.gstperiod).Result;
            foreach (var kv in downloaddata)
            {
                item.gsttype = "GSTR2A-" + kv.key;
                item.createdby = this.Context.UserID;
                item.createdon = DateTimeNow;
                item.companyid = item.id;
                item.gstno = GSTPortal.GSTN;
                item.gstadata_json = kv.data.data;
                item.version = 1;
                item.isactive = true;
                item.apiresponse = kv.data.TxnOutcome;
                if (string.IsNullOrEmpty(item.apiresponse))
                    item.apiresponse = "Successfully downloaded";
                Insert(item);
            }
            return item;
        }

        internal InfyGST.GSTPortal.EInvoiceResponse CreateEInvoice4DebitNote(GST_Invoice itemconfig)
        {
            bool issandbox = false;
            string gstno = null;
            string einvoiceusername = null;
            string einvoicepwd = null;
            string gstaccesskey = null;

            InfyPOS.Data.LrinvoicereturnDataService billDataService = new LrinvoicereturnDataService() { Context = this.Context };
            var bill = billDataService.Print(new LrinvoicereturnCreteria()
            {
                id = itemconfig.billid
            });

            var param = new SortedList<string, object>();
            param.Add("id", bill.companyid);
            using (var reader = ExecuteReader("select tinanddate,einvoiceusername,einvoicepwd,gstaccesskey from company where id=@id", param))
            {
                if (reader.Read())
                {
                    gstno = reader.IsDBNull(0) ? "" : reader.GetString(0);
                    einvoiceusername = reader.IsDBNull(1) ? "" : reader.GetString(1);
                    einvoicepwd = reader.IsDBNull(2) ? "" : reader.GetString(2);
                    gstaccesskey = reader.IsDBNull(3) ? "" : reader.GetString(3);
                }
            }

            ProductsDataService productsDataService = new ProductsDataService() { Context = this.Context };
            var productlist = productsDataService.Search(new ProductsCreteria()
            {
                ids = bill.Lrinvoicereturnitems.ConvertAll(e => e.productid).Distinct().ToList()
            });


            InfyPOS.Data.CompanyDataService company = new CompanyDataService() { Context = this.Context };
            bill.company = company.Search(new CompanyCreteria
            {
                id = bill.companyid
            }).First();


            if (itemconfig.transportid > 0)
            {
                InfyPOS.Data.TransportDataService transportDataService = new TransportDataService() { Context = this.Context };
                var transport = transportDataService.Search(new TransportCreteria()
                {
                    id = itemconfig.transportid
                });
                bill.transport = transport[0];
            }

            if (issandbox)
            {
                gstno = "34AACCC1596Q002";
                gstaccesskey = "34AACCC1596Q002";
                bill.company.tinanddate = gstno;
                bill.isinterstatesale = !bill.supplier.gstno.StartsWith("34");
                bill.company.cityname = "PONDICHERRY";
                bill.company.pincode = "605001";
            }


            InfyGST.GSTPortal.EInvoiceRequest reqPlGenIRN = new InfyGST.GSTPortal.EInvoiceRequest();
            reqPlGenIRN.Version = "1.1";
            reqPlGenIRN.TranDtls = new InfyGST.GSTPortal.EInvoiceRequest.TranDetails();
            reqPlGenIRN.TranDtls.TaxSch = "GST";
            reqPlGenIRN.TranDtls.SupTyp = "B2B";
            reqPlGenIRN.DocDtls = new InfyGST.GSTPortal.EInvoiceRequest.DocSetails();

            reqPlGenIRN.DocDtls.Typ = "DBN";
            bill.Lrinvoicereturnitems.ForEach(x =>
            {
                x.amount = Math.Abs(x.amount);
                x.tax = Math.Abs(x.tax);
                x.qty = Math.Abs(x.qty);
            });
            //if (bill.hasFreight)
            //{
            //    bill.additionalchargeslist.ForEach(x =>
            //    {
            //        x.Tax = Math.Abs(x.Tax);
            //        x.Charge = Math.Abs(x.Charge);
            //        x.Amount = Math.Abs(x.Amount);
            //    });

            //}

            int tryno = 0;
            if (!int.TryParse(bill.location.pincode.Replace(" ", ""), out tryno))
                throw new Exception("Pleae enter the correct location pincode");

            if (!int.TryParse(bill.company.pincode.Replace(" ", ""), out tryno))
                throw new Exception("Pleae enter the correct company pincode");

            reqPlGenIRN.DocDtls.No = bill.returnno;
            reqPlGenIRN.DocDtls.Dt = bill.returndate.ToString("dd/MM/yyyy").Replace("-", "/");
            reqPlGenIRN.SellerDtls = new InfyGST.GSTPortal.EInvoiceRequest.SellerDetails();
            reqPlGenIRN.SellerDtls.Gstin = bill.company.gstno;
            reqPlGenIRN.SellerDtls.LglNm = bill.company.name;
            reqPlGenIRN.SellerDtls.TrdNm = null;
            if (bill.location != null &&
                !string.IsNullOrEmpty(bill.location.cityname) &&
                !string.IsNullOrEmpty(bill.location.pincode) &&
                !string.IsNullOrEmpty(bill.location.addressline1) &&
                !string.IsNullOrEmpty(bill.location.addressline2))
            {
                reqPlGenIRN.SellerDtls.Addr1 = bill.location.addressline1;
                reqPlGenIRN.SellerDtls.Addr2 = bill.location.addressline2;
                reqPlGenIRN.SellerDtls.Loc = bill.location.cityname;
                reqPlGenIRN.SellerDtls.Pin = int.Parse("0" + bill.location.pincode.Replace(" ", ""));
                reqPlGenIRN.SellerDtls.State = bill.location.statename;
                reqPlGenIRN.SellerDtls.Stcd = bill.company.gstno.Substring(0, 2);
                reqPlGenIRN.SellerDtls.Ph = bill.company.contactno.Split(',')[0].Replace("-", "");
                reqPlGenIRN.SellerDtls.Em = bill.company.emailid;
            }
            else
            {
                reqPlGenIRN.SellerDtls.Addr1 = bill.company.addressline1;
                reqPlGenIRN.SellerDtls.Addr2 = bill.company.addressline2;
                reqPlGenIRN.SellerDtls.Loc = bill.company.cityname;
                reqPlGenIRN.SellerDtls.Pin = int.Parse("0" + bill.company.pincode.Replace(" ", ""));
                reqPlGenIRN.SellerDtls.State = bill.company.statename;
                reqPlGenIRN.SellerDtls.Stcd = bill.company.gstno.Substring(0, 2);
                reqPlGenIRN.SellerDtls.Ph = bill.company.contactno.Split(',')[0].Replace("-", "");
                reqPlGenIRN.SellerDtls.Em = bill.company.emailid;
            }

            reqPlGenIRN.BuyerDtls = new InfyGST.GSTPortal.EInvoiceRequest.BuyerDetails();
            reqPlGenIRN.BuyerDtls.Gstin = bill.supplier.gstno;
            reqPlGenIRN.BuyerDtls.LglNm = bill.supplier.name;
            reqPlGenIRN.BuyerDtls.TrdNm = null;
            reqPlGenIRN.BuyerDtls.Pos = bill.supplier.gstno.Substring(0, 2);
            reqPlGenIRN.BuyerDtls.Addr1 = bill.supplier.addressline1;
            reqPlGenIRN.BuyerDtls.Addr2 = string.IsNullOrEmpty(bill.supplier.addressline2) ? null : bill.supplier.addressline2;
            reqPlGenIRN.BuyerDtls.Loc = bill.supplier.cityname;
            reqPlGenIRN.BuyerDtls.Pin = int.Parse("0" + bill.supplier.pincode.Replace(" ", ""));
            reqPlGenIRN.BuyerDtls.State = bill.supplier.statename;
            reqPlGenIRN.BuyerDtls.Stcd = bill.supplier.gstno.Substring(0, 2);
            reqPlGenIRN.BuyerDtls.Ph = bill.supplier.contactnumber.Split(',')[0].Replace("-", "");
            reqPlGenIRN.BuyerDtls.Em = string.IsNullOrEmpty(bill.supplier.companyemailid) ? null : bill.supplier.companyemailid;

            reqPlGenIRN.DispDtls = new InfyGST.GSTPortal.EInvoiceRequest.DispatchedDetails();
            reqPlGenIRN.DispDtls.Nm = bill.company.name;
            reqPlGenIRN.DispDtls.Addr1 = bill.company.addressline1;
            reqPlGenIRN.DispDtls.Addr2 = bill.company.addressline2;
            reqPlGenIRN.DispDtls.Loc = bill.company.cityname;
            reqPlGenIRN.DispDtls.Pin = int.Parse("0" + bill.company.pincode.Replace(" ", ""));
            reqPlGenIRN.DispDtls.Stcd = bill.company.gstno.Substring(0, 2);
            reqPlGenIRN.DispDtls.State = bill.company.statename;

            reqPlGenIRN.ShipDtls = new InfyGST.GSTPortal.EInvoiceRequest.ShippedDetails();
            reqPlGenIRN.ShipDtls.Gstin = bill.supplier.gstno;
            reqPlGenIRN.ShipDtls.LglNm = bill.supplier.name;
            reqPlGenIRN.ShipDtls.TrdNm = null;
            reqPlGenIRN.ShipDtls.Addr1 = bill.supplier.addressline1;
            reqPlGenIRN.ShipDtls.Addr2 = string.IsNullOrEmpty(bill.supplier.addressline2) ? null : bill.supplier.addressline2;
            reqPlGenIRN.ShipDtls.Loc = bill.supplier.cityname;
            reqPlGenIRN.ShipDtls.Pin = int.Parse("0" + bill.supplier.pincode.Replace(" ", ""));
            reqPlGenIRN.ShipDtls.Stcd = bill.supplier.gstno.Substring(0, 2);
            reqPlGenIRN.ShipDtls.State = bill.supplier.statename;
            reqPlGenIRN.ItemList = new List<InfyGST.GSTPortal.EInvoiceRequest.ItmList>();
            int slno = 1;


            foreach (var item in bill.Lrinvoicereturnitems.GroupBy(x => new { x.printingname, x.hsncode, x.taxpercentage, unitprice = (double)Math.Round((x.amount - x.tax) / x.qty, 2) }))
            {
                var tax = Math.Round(item.Sum(x => x.tax), 2);
                var receivable = Math.Round(item.Sum(x => x.taxableamount + x.tax), 2);
                InfyGST.GSTPortal.EInvoiceRequest.ItmList itm = new InfyGST.GSTPortal.EInvoiceRequest.ItmList();
                itm.SlNo = slno.ToString();
                itm.IsServc = "N";
                itm.PrdDesc = string.IsNullOrEmpty(item.Key.printingname) ? item.Key.printingname : item.Key.printingname;
                //itm.PrdDesc = item.productname;
                itm.HsnCd = item.Key.hsncode;
                itm.BchDtls = null;
                itm.Qty = (double)item.Sum(x => x.qty);
                itm.Unit = itm.Qty % 1 == 0 ? "NOS" : "MTR";
                itm.UnitPrice = item.Key.unitprice;
                itm.TotAmt = Math.Round(Convert.ToDouble(receivable - tax), 2);
                itm.Discount = 0.0;
                itm.AssAmt = Math.Round(Convert.ToDouble(receivable - tax), 2);
                itm.GstRt = Math.Round(Convert.ToDouble(item.Key.taxpercentage), 2);
                if (bill.isinterstatesale)
                {
                    itm.IgstAmt = Math.Round(Convert.ToDouble(tax), 2);
                }
                else
                {
                    itm.SgstAmt = Math.Round(Convert.ToDouble(tax / 2), 2);
                    itm.CgstAmt = Math.Round(Convert.ToDouble(tax / 2), 2);
                }
                itm.CesRt = 0.0;
                itm.CesAmt = 0.0;
                itm.CesNonAdvlAmt = 0.0;
                itm.StateCesRt = 0.0;
                itm.StateCesAmt = 0.0;
                itm.StateCesNonAdvlAmt = 0.0;
                itm.OthChrg = 0.0;
                itm.TotItemVal = Math.Round(Convert.ToDouble(receivable), 2);
                itm.AttribDtls = null;
                reqPlGenIRN.ItemList.Add(itm);
                slno++;
            }

            if (bill.hasFreight)
            {
                InfyGST.GSTPortal.EInvoiceRequest.ItmList itm = new InfyGST.GSTPortal.EInvoiceRequest.ItmList();
                itm.SlNo = slno.ToString();
                itm.IsServc = "Y";
                itm.PrdDesc = "FRIGHT CHARGES";
                itm.HsnCd = "9965";
                itm.BchDtls = null;
                itm.Qty = 1;
                itm.Unit = "NOS";
                itm.UnitPrice = (double)Math.Round(bill.freightamt, 2);
                itm.TotAmt = Math.Round(Convert.ToDouble(bill.freightamt), 2);
                itm.Discount = 0.0;
                itm.AssAmt = Math.Round(Convert.ToDouble(bill.freightamt), 2);
                if (bill.freighttaxamt > 0)
                {
                    itm.GstRt = (double)Math.Round(bill.freighttaxamt / bill.freightamt * 100, 0);
                    if (bill.isinterstatesale)
                    {
                        itm.IgstAmt = Math.Round(Convert.ToDouble(bill.freighttaxamt), 2);
                    }
                    else
                    {
                        itm.SgstAmt = Math.Round(Convert.ToDouble(bill.freighttaxamt / 2), 2);
                        itm.CgstAmt = Math.Round(Convert.ToDouble(bill.freighttaxamt / 2), 2);
                    }
                }
                itm.CesRt = 0.0;
                itm.CesAmt = 0.0;
                itm.CesNonAdvlAmt = 0.0;
                itm.StateCesRt = 0.0;
                itm.StateCesAmt = 0.0;
                itm.StateCesNonAdvlAmt = 0.0;
                itm.OthChrg = 0.0;
                itm.TotItemVal = Math.Round(Convert.ToDouble(bill.freightamt + bill.freighttaxamt), 2);
                itm.AttribDtls = null;
                reqPlGenIRN.ItemList.Add(itm);
            }


            //if (bill.additionalchargeslist != null && bill.additionalchargeslist.Count > 0)
            //{
            //    ReferencelistDataService referencelistDataService = new ReferencelistDataService() { Context = this.Context };
            //    var addncharges = referencelistDataService.Search(new ReferencelistCreteria()
            //    {
            //        ids = bill.additionalchargeslist.ConvertAll(x => x.additionalchargestypeid).ToList()
            //    });

            //    foreach (var item in bill.additionalchargeslist)
            //    {
            //        item.Tax = Math.Round(item.Tax, 2);
            //        InfyGST.GSTPortal.EInvoiceRequest.ItmList itm = new InfyGST.GSTPortal.EInvoiceRequest.ItmList();
            //        itm.SlNo = slno.ToString();
            //        itm.IsServc = "Y";
            //        itm.PrdDesc = item.Name;
            //        itm.HsnCd = addncharges.Find(x => x.id == item.additionalchargestypeid).additionalinfo.formula;
            //        itm.BchDtls = null;
            //        itm.Qty = 1;
            //        itm.Unit = "NOS";
            //        itm.UnitPrice = Math.Round(Convert.ToDouble(item.Charge), 2);
            //        itm.TotAmt = Math.Round(Convert.ToDouble(item.Charge), 2);
            //        itm.Discount = 0.0;
            //        itm.AssAmt = Math.Round(Convert.ToDouble(item.Charge), 2);
            //        itm.GstRt = Math.Round(Convert.ToDouble(item.taxpercentage), 2);
            //        if (bill.isinterstatesale)
            //        {
            //            itm.IgstAmt = Math.Round(Convert.ToDouble(item.Tax), 2);
            //        }
            //        else
            //        {
            //            itm.SgstAmt = Math.Round(Convert.ToDouble(item.Tax / 2), 2);
            //            itm.CgstAmt = Math.Round(Convert.ToDouble(item.Tax / 2), 2);
            //        }
            //        itm.CesRt = 0.0;
            //        itm.CesAmt = 0.0;
            //        itm.CesNonAdvlAmt = 0.0;
            //        itm.StateCesRt = 0.0;
            //        itm.StateCesAmt = 0.0;
            //        itm.StateCesNonAdvlAmt = 0.0;
            //        itm.OthChrg = 0.0;
            //        itm.TotItemVal = Math.Round(Convert.ToDouble(item.Amount), 2);
            //        itm.AttribDtls = null;
            //        reqPlGenIRN.ItemList.Add(itm);
            //        slno++;
            //    }
            //}

            reqPlGenIRN.PayDtls = null;
            reqPlGenIRN.RefDtls = null;
            reqPlGenIRN.AddlDocDtls = null;
            reqPlGenIRN.ExpDtls = null;
            if (bill.transport != null && !string.IsNullOrEmpty(itemconfig.vehcicleno))
            {
                reqPlGenIRN.EwbDtls = new InfyGST.GSTPortal.EInvoiceRequest.EwbDetails();
                reqPlGenIRN.EwbDtls.TransDocDt = bill.returndate.ToString("dd/MM/yyyy").Replace("-", "/");
                reqPlGenIRN.EwbDtls.TransDocNo = bill.returnno;
                reqPlGenIRN.EwbDtls.TransId = bill.transport.gst;
                reqPlGenIRN.EwbDtls.Distance = 0;
                reqPlGenIRN.EwbDtls.VehNo = itemconfig.vehcicleno;
                reqPlGenIRN.EwbDtls.VehType = string.IsNullOrEmpty(itemconfig.vehcicleno) ? "O" : "R";
                reqPlGenIRN.EwbDtls.TransMode = itemconfig.modeofdelivery > 0 ? itemconfig.modeofdelivery.ToString("N0") : "1";
                reqPlGenIRN.EwbDtls.TransName = bill.transport.name;
            }

            reqPlGenIRN.ValDtls = new InfyGST.GSTPortal.EInvoiceRequest.ValDetails();
            reqPlGenIRN.ValDtls.AssVal = Math.Round(reqPlGenIRN.ItemList.Sum(e => e.AssAmt), 2);
            reqPlGenIRN.ValDtls.CgstVal = Math.Round(reqPlGenIRN.ItemList.Sum(e => e.CgstAmt), 2);
            reqPlGenIRN.ValDtls.SgstVal = Math.Round(reqPlGenIRN.ItemList.Sum(e => e.SgstAmt), 2);
            reqPlGenIRN.ValDtls.IgstVal = Math.Round(reqPlGenIRN.ItemList.Sum(e => e.IgstAmt), 2);
            reqPlGenIRN.ValDtls.CesVal = 0.0;
            reqPlGenIRN.ValDtls.StCesVal = 0.0;
            var finalreceivable = reqPlGenIRN.ValDtls.AssVal + reqPlGenIRN.ValDtls.CgstVal + reqPlGenIRN.ValDtls.SgstVal + reqPlGenIRN.ValDtls.IgstVal;
            reqPlGenIRN.ValDtls.TotInvVal = Math.Round(Convert.ToDouble(finalreceivable), 0);
            reqPlGenIRN.ValDtls.RndOffAmt = Math.Round(Convert.ToDouble(reqPlGenIRN.ValDtls.TotInvVal) - Math.Round(Convert.ToDouble(finalreceivable), 2), 2);

            var GSTPortal = GetService(bill.companyid);
            var response = GSTPortal.CreateEInvoice(new InfyGST.GSTPortal.InvoiceRequest()
            {
                Data = reqPlGenIRN,
                accesskey = gstaccesskey,
                eInvPwd = einvoicepwd,
                gstin = gstno,
                issandbox = issandbox,
                username = einvoiceusername
            }).Result;

            if (itemconfig.hsn4digit)
            {
                if (response.ErrorDetails != null && response.ErrorDetails.ToList().Exists(e => e.ErrorCode == 2176))
                {
                    var hsnlist = response.ErrorDetails.ToList().Find(e => e.ErrorCode == 2176);
                    var hsncodeList = hsnlist.ErrorMessage.Replace("HSN code(s)-", "").Replace(" is invalid", "").Split(',').ToList();

                    reqPlGenIRN.ItemList.FindAll(x => hsncodeList.Contains(x.HsnCd)).ForEach(e =>
                    {
                        e.HsnCd = e.HsnCd.Substring(0, 4);
                    });

                    response = GSTPortal.CreateEInvoice(new InfyGST.GSTPortal.InvoiceRequest()
                    {
                        Data = reqPlGenIRN,
                        accesskey = gstaccesskey,
                        eInvPwd = einvoicepwd,
                        gstin = gstno,
                        issandbox = issandbox,
                        username = einvoiceusername
                    }).Result;
                }
            }

            return response;
        }

        internal InfyGST.GSTPortal.EInvoiceResponse CreateEInvoice4Voucher(Voucher bill, bool hsn4digit)
        {
            bool issandbox = false;
            string gstno = null;
            string einvoiceusername = null;
            string einvoicepwd = null;
            string gstaccesskey = null;



            var param = new SortedList<string, object>();
            param.Add("id", bill.companyid);
            using (var reader = ExecuteReader("select tinanddate,einvoiceusername,einvoicepwd,gstaccesskey from company where id=@id", param))
            {
                if (reader.Read())
                {
                    gstno = reader.IsDBNull(0) ? "" : reader.GetString(0);
                    einvoiceusername = reader.IsDBNull(1) ? "" : reader.GetString(1);
                    einvoicepwd = reader.IsDBNull(2) ? "" : reader.GetString(2);
                    gstaccesskey = reader.IsDBNull(3) ? "" : reader.GetString(3);
                }
            }

            LedgerDataService productsDataService = new LedgerDataService() { Context = this.Context };
            var productlist = productsDataService.Search(new LedgerCreteria()
            {
                ids = bill.voucheritems.ConvertAll(e => e.ledgerid).Distinct().ToList()
            });

            var customer = productsDataService.Search(new LedgerCreteria()
            {
                id = bill.adjustmentledgerid
            });

            customer.ForEach(x => x.parseaddress());

            ReferencelistDataService referencelistDataService = new ReferencelistDataService() { Context = this.Context };
            var refnames = referencelistDataService.SearchList(new ReferencelistCreteria()
            {
                ids = new List<long>(new long[] { customer[0].cityid, customer[0].stateid })
            });



            InfyPOS.Data.CompanyDataService company = new CompanyDataService() { Context = this.Context };
            var companyinfo = company.Search(new CompanyCreteria
            {
                id = bill.companyid
            });

            if (issandbox)
            {
                gstno = "34AACCC1596Q002";
                gstaccesskey = "34AACCC1596Q002";
                companyinfo[0].tinanddate = gstno;
                companyinfo[0].cityname = "PONDICHERRY";
                companyinfo[0].pincode = "605001";
            }


            InfyGST.GSTPortal.EInvoiceRequest reqPlGenIRN = new InfyGST.GSTPortal.EInvoiceRequest();
            reqPlGenIRN.Version = "1.1";
            reqPlGenIRN.TranDtls = new InfyGST.GSTPortal.EInvoiceRequest.TranDetails();
            reqPlGenIRN.TranDtls.TaxSch = "GST";
            reqPlGenIRN.TranDtls.SupTyp = "B2B";
            reqPlGenIRN.DocDtls = new InfyGST.GSTPortal.EInvoiceRequest.DocSetails();

            reqPlGenIRN.DocDtls.Typ = "INV";

            reqPlGenIRN.DocDtls.No = bill.voucherno;
            reqPlGenIRN.DocDtls.Dt = bill.voucherdate.ToString("dd/MM/yyyy").Replace("-", "/");
            reqPlGenIRN.SellerDtls = new InfyGST.GSTPortal.EInvoiceRequest.SellerDetails();
            reqPlGenIRN.SellerDtls.Gstin = companyinfo[0].gstno;
            reqPlGenIRN.SellerDtls.LglNm = companyinfo[0].name;
            reqPlGenIRN.SellerDtls.TrdNm = null;

            reqPlGenIRN.SellerDtls.Addr1 = companyinfo[0].addressline1;
            reqPlGenIRN.SellerDtls.Addr2 = companyinfo[0].addressline2;
            reqPlGenIRN.SellerDtls.Loc = companyinfo[0].cityname;
            reqPlGenIRN.SellerDtls.Pin = int.Parse("0" + companyinfo[0].pincode.Replace(" ", ""));
            reqPlGenIRN.SellerDtls.State = companyinfo[0].statename;
            reqPlGenIRN.SellerDtls.Stcd = companyinfo[0].gstno.Substring(0, 2);
            reqPlGenIRN.SellerDtls.Ph = companyinfo[0].contactno.Split(',')[0].Replace("-", "");
            reqPlGenIRN.SellerDtls.Em = companyinfo[0].emailid;

            reqPlGenIRN.DispDtls = new InfyGST.GSTPortal.EInvoiceRequest.DispatchedDetails();
            reqPlGenIRN.DispDtls.Nm = companyinfo[0].name;
            reqPlGenIRN.DispDtls.Addr1 = companyinfo[0].addressline1;
            reqPlGenIRN.DispDtls.Addr2 = companyinfo[0].addressline2;
            reqPlGenIRN.DispDtls.Loc = companyinfo[0].cityname;
            reqPlGenIRN.DispDtls.Pin = int.Parse("0" + companyinfo[0].pincode.Replace(" ", ""));
            reqPlGenIRN.DispDtls.Stcd = companyinfo[0].gstno.Substring(0, 2);
            reqPlGenIRN.DispDtls.State = companyinfo[0].statename;


            reqPlGenIRN.BuyerDtls = new InfyGST.GSTPortal.EInvoiceRequest.BuyerDetails();
            reqPlGenIRN.BuyerDtls.Gstin = customer[0].gstno;
            reqPlGenIRN.BuyerDtls.LglNm = string.IsNullOrEmpty(customer[0].billingname) ? customer[0].displayname : customer[0].billingname;
            reqPlGenIRN.BuyerDtls.TrdNm = null;
            reqPlGenIRN.BuyerDtls.Pos = customer[0].gstno.Substring(0, 2);
            reqPlGenIRN.BuyerDtls.Addr1 = customer[0].addressline1;
            reqPlGenIRN.BuyerDtls.Addr2 = string.IsNullOrEmpty(customer[0].addressline2) ? null : customer[0].addressline2;

            if (refnames.Exists(x => x.id == customer[0].cityid))
                reqPlGenIRN.BuyerDtls.Loc = refnames.Find(x => x.id == customer[0].cityid).name;

            reqPlGenIRN.BuyerDtls.Pin = int.Parse("0" + customer[0].pincode.Replace(" ", ""));

            if (refnames.Exists(x => x.id == customer[0].stateid))
                reqPlGenIRN.BuyerDtls.State = refnames.Find(x => x.id == customer[0].stateid).name;

            reqPlGenIRN.BuyerDtls.Stcd = customer[0].gstno.Substring(0, 2);
            reqPlGenIRN.BuyerDtls.Ph = customer[0].contactnumber;
            reqPlGenIRN.BuyerDtls.Em = string.IsNullOrEmpty(customer[0].emailid) ? null : customer[0].emailid;



            reqPlGenIRN.ShipDtls = new InfyGST.GSTPortal.EInvoiceRequest.ShippedDetails();
            reqPlGenIRN.ShipDtls.Gstin = customer[0].gstno;
            reqPlGenIRN.ShipDtls.LglNm = string.IsNullOrEmpty(customer[0].billingname) ? customer[0].displayname : customer[0].billingname;
            reqPlGenIRN.ShipDtls.TrdNm = null;
            reqPlGenIRN.ShipDtls.Addr1 = customer[0].addressline1;
            reqPlGenIRN.ShipDtls.Addr2 = string.IsNullOrEmpty(customer[0].addressline2) ? null : customer[0].addressline2;

            if (refnames.Exists(x => x.id == customer[0].cityid))
                reqPlGenIRN.ShipDtls.Loc = refnames.Find(x => x.id == customer[0].cityid).name;

            reqPlGenIRN.ShipDtls.Pin = int.Parse("0" + customer[0].pincode.Replace(" ", ""));
            reqPlGenIRN.ShipDtls.Stcd = customer[0].gstno.Substring(0, 2);

            if (refnames.Exists(x => x.id == customer[0].stateid))
                reqPlGenIRN.ShipDtls.State = refnames.Find(x => x.id == customer[0].stateid).name;

            reqPlGenIRN.ItemList = new List<InfyGST.GSTPortal.EInvoiceRequest.ItmList>();
            int slno = 1;


            var taxlist = bill.voucheritems.FindAll(x => x.hastax);
            var prdlist = bill.voucheritems.FindAll(x => !x.hastax);

            foreach (var prditem in prdlist)
            {
                var prdledger = productlist.Find(x => x.id == prditem.ledgerid);
                var taxpercentage = prditem.taxpercentage;
                if (taxpercentage == 0)
                {
                    if (reqPlGenIRN.SellerDtls.Stcd != reqPlGenIRN.BuyerDtls.Stcd)
                    {
                        var igstledger = productlist.Find(x => x.id == prdledger.attributes.ledgerigstid);
                        if (igstledger == null)
                            throw new Exception("Please select the igst ledger on the voucher");
                        taxpercentage = igstledger.taxpercentage;
                    }
                    else
                    {
                        var cgstledger = productlist.Find(x => x.id == prdledger.attributes.ledgercgstid);
                        var sgstledger = productlist.Find(x => x.id == prdledger.attributes.ledgersgstid);

                        if (cgstledger == null)
                            throw new Exception("Please select the cgst ledger on the voucher");
                        if (sgstledger == null)
                            throw new Exception("Please select the sgst ledger on the voucher");
                        taxpercentage = cgstledger.taxpercentage + sgstledger.taxpercentage;
                    }
                }
                //if (prditem.taxpercentage == 0 && taxlist.Exists(x=>x.taxable == prditem.amount))
                //{
                //    taxpercentage = taxlist.Sum(x => x.taxable == prditem.amount ? x.taxpercentage : 0);
                //}
                var tax = Math.Round(taxlist.Sum(x => x.amount), 2);
                var prod = prditem.amount;
                InfyGST.GSTPortal.EInvoiceRequest.ItmList itm = new InfyGST.GSTPortal.EInvoiceRequest.ItmList();
                itm.SlNo = slno.ToString();
                itm.IsServc = "N";
                itm.PrdDesc = prdledger.displayname;
                itm.HsnCd = prdledger.hsncode;
                itm.BchDtls = null;
                itm.Unit = (prdledger.attributes != null &&
                               !string.IsNullOrEmpty(prdledger.attributes.unit)) ? prdledger.attributes.unit : "NOS";
                if (prditem.qty == 0)
                {
                    itm.Qty = (double)1;
                    itm.UnitPrice = (double)prditem.amount;
                }
                else
                {
                    itm.Qty = (double)prditem.qty;
                    itm.UnitPrice = (double)prditem.rate;
                }
                itm.TotAmt = Math.Round(Convert.ToDouble(prod), 2);
                itm.Discount = 0.0;
                itm.AssAmt = Math.Round(Convert.ToDouble(prod), 2);
                itm.GstRt = Math.Round(Convert.ToDouble(taxpercentage), 2);
                if (reqPlGenIRN.SellerDtls.Stcd != reqPlGenIRN.BuyerDtls.Stcd)
                {
                    itm.IgstAmt = Math.Round(Convert.ToDouble((double)prod * itm.GstRt / 100), 2);
                }
                else
                {
                    itm.SgstAmt = Math.Round(Convert.ToDouble((double)prod * itm.GstRt / 100) / 2, 2);
                    itm.CgstAmt = Math.Round(Convert.ToDouble((double)prod * itm.GstRt / 100) / 2, 2);
                }
                itm.CesRt = 0.0;
                itm.CesAmt = 0.0;
                itm.CesNonAdvlAmt = 0.0;
                itm.StateCesRt = 0.0;
                itm.StateCesAmt = 0.0;
                itm.StateCesNonAdvlAmt = 0.0;
                itm.OthChrg = 0.0;
                itm.TotItemVal = Math.Round(Convert.ToDouble(itm.AssAmt + itm.IgstAmt + itm.SgstAmt + itm.CgstAmt), 2);
                itm.AttribDtls = null;
                reqPlGenIRN.ItemList.Add(itm);
                slno++;
            }

            reqPlGenIRN.PayDtls = null;
            reqPlGenIRN.RefDtls = null;
            reqPlGenIRN.AddlDocDtls = null;
            reqPlGenIRN.ExpDtls = null;

            reqPlGenIRN.ValDtls = new InfyGST.GSTPortal.EInvoiceRequest.ValDetails();
            reqPlGenIRN.ValDtls.AssVal = Math.Round(reqPlGenIRN.ItemList.Sum(e => e.AssAmt), 2);
            reqPlGenIRN.ValDtls.CgstVal = Math.Round(reqPlGenIRN.ItemList.Sum(e => e.CgstAmt), 2);
            reqPlGenIRN.ValDtls.SgstVal = Math.Round(reqPlGenIRN.ItemList.Sum(e => e.SgstAmt), 2);
            reqPlGenIRN.ValDtls.IgstVal = Math.Round(reqPlGenIRN.ItemList.Sum(e => e.IgstAmt), 2);
            reqPlGenIRN.ValDtls.CesVal = 0.0;
            reqPlGenIRN.ValDtls.StCesVal = 0.0;
            var finalreceivable = reqPlGenIRN.ValDtls.AssVal + reqPlGenIRN.ValDtls.CgstVal + reqPlGenIRN.ValDtls.SgstVal + reqPlGenIRN.ValDtls.IgstVal;
            reqPlGenIRN.ValDtls.TotInvVal = Math.Round(Convert.ToDouble(finalreceivable), 0);
            reqPlGenIRN.ValDtls.RndOffAmt = Math.Round(Convert.ToDouble(reqPlGenIRN.ValDtls.TotInvVal) - Math.Round(Convert.ToDouble(finalreceivable), 2), 2);

            if (string.IsNullOrEmpty(reqPlGenIRN.BuyerDtls.Ph) || (reqPlGenIRN.BuyerDtls.Ph != null && reqPlGenIRN.BuyerDtls.Ph.Length < 6)) reqPlGenIRN.BuyerDtls.Ph = null;
            if (string.IsNullOrEmpty(reqPlGenIRN.SellerDtls.Ph) || (reqPlGenIRN.BuyerDtls.Ph != null && reqPlGenIRN.BuyerDtls.Ph.Length < 6)) reqPlGenIRN.SellerDtls.Ph = null;

            var GSTPortal = GetService(bill.companyid);
            if (GSTPortal.URI == "/InfyGSTService")
            {
                InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Initialize();
            }

            var response = GSTPortal.CreateEInvoice(new InfyGST.GSTPortal.InvoiceRequest()
            {
                Data = reqPlGenIRN,
                accesskey = gstaccesskey,
                eInvPwd = einvoicepwd,
                gstin = gstno,
                issandbox = issandbox,
                username = einvoiceusername
            }).Result;

            if (hsn4digit)
            {
                if (response.ErrorDetails != null && response.ErrorDetails.ToList().Exists(e => e.ErrorCode == 2176))
                {
                    var hsnlist = response.ErrorDetails.ToList().Find(e => e.ErrorCode == 2176);
                    var hsncodeList = hsnlist.ErrorMessage.Replace("HSN code(s)-", "").Replace(" is invalid", "").Split(',').ToList();

                    reqPlGenIRN.ItemList.FindAll(x => hsncodeList.Contains(x.HsnCd)).ForEach(e =>
                    {
                        e.HsnCd = e.HsnCd.Substring(0, 4);
                    });

                    response = GSTPortal.CreateEInvoice(new InfyGST.GSTPortal.InvoiceRequest()
                    {
                        Data = reqPlGenIRN,
                        accesskey = gstaccesskey,
                        eInvPwd = einvoicepwd,
                        gstin = gstno,
                        issandbox = issandbox,
                        username = einvoiceusername
                    }).Result;
                }
            }

            return response;
        }
        internal InfyGST.GSTPortal.EInvoiceResponse CreateEInvoice4Bill(GST_Invoice itemconfig)
        {
            bool issandbox = false;
            string gstno = null;
            string einvoiceusername = null;
            string einvoicepwd = null;
            string gstaccesskey = null;

            InfyPOS.Data.BillDataService billDataService = new BillDataService() { Context = this.Context };
            var bills = billDataService.Search(new BillCreteria()
            {
                id = itemconfig.billid,
                isb2b = true,
                isimport = true
            });

            var param = new SortedList<string, object>();
            param.Add("id", bills[0].companyid);
            using (var reader = ExecuteReader("select tinanddate,einvoiceusername,einvoicepwd,gstaccesskey from company where id=@id", param))
            {
                if (reader.Read())
                {
                    gstno = reader.IsDBNull(0) ? "" : reader.GetString(0);
                    einvoiceusername = reader.IsDBNull(1) ? "" : reader.GetString(1);
                    einvoicepwd = reader.IsDBNull(2) ? "" : reader.GetString(2);
                    gstaccesskey = reader.IsDBNull(3) ? "" : reader.GetString(3);
                }
            }

            ProductsDataService productsDataService = new ProductsDataService() { Context = this.Context };
            var productlist = productsDataService.Search(new ProductsCreteria()
            {
                ids = bills[0].Billitems.ConvertAll(e => e.productid).Distinct().ToList()
            });

            if (itemconfig.hsn8digit)
            {
                var hsnbillitems = bills[0].Billitems.FindAll(x => string.IsNullOrEmpty(x.hsncode) || x.hsncode.Length != 8);
                hsnbillitems.ForEach(x =>
                {
                    if (productlist.Exists(y => y.id == x.productid))
                    {
                        var prd = productlist.Find(y => y.id == x.productid);
                        if (!string.IsNullOrEmpty(prd.hsncode) && prd.hsncode.Length == 8)
                            x.hsncode = prd.hsncode;
                    }
                });
            }

            InfyPOS.Data.CompanyDataService company = new CompanyDataService() { Context = this.Context };
            bills[0].company = company.Search(new CompanyCreteria
            {
                id = bills[0].companyid
            }).First();


            if (itemconfig.transportid > 0)
            {
                InfyPOS.Data.TransportDataService transportDataService = new TransportDataService() { Context = this.Context };
                var transport = transportDataService.Search(new TransportCreteria()
                {
                    id = itemconfig.transportid
                });
                bills[0].transport = transport[0];
            }

            if (issandbox)
            {
                gstno = "34AACCC1596Q002";
                gstaccesskey = "34AACCC1596Q002";
                bills[0].company.tinanddate = gstno;
                bills[0].isinterstatesale = !bills[0].customer.gstno.StartsWith("34");
                bills[0].company.cityname = "PONDICHERRY";
                bills[0].company.pincode = "605001";
            }

            var bill = bills[0];

            InfyGST.GSTPortal.EInvoiceRequest reqPlGenIRN = new InfyGST.GSTPortal.EInvoiceRequest();
            reqPlGenIRN.Version = "1.1";
            reqPlGenIRN.TranDtls = new InfyGST.GSTPortal.EInvoiceRequest.TranDetails();
            reqPlGenIRN.TranDtls.TaxSch = "GST";
            reqPlGenIRN.TranDtls.SupTyp = "B2B";

            if (!string.IsNullOrEmpty(bill.customer.supplytype) &&
                "B2B" != bill.customer.supplytype)
                reqPlGenIRN.TranDtls.SupTyp = bill.customer.supplytype;

            if (string.IsNullOrEmpty(bill.customer.gstno) &&
               reqPlGenIRN.TranDtls.SupTyp == "B2B")
                reqPlGenIRN.TranDtls.SupTyp = "B2C";

            reqPlGenIRN.DocDtls = new InfyGST.GSTPortal.EInvoiceRequest.DocSetails();
            if (bill.billtype == Bill.BillTypes.ReturnBill ||
                bill.receivable < 0)
            {
                reqPlGenIRN.DocDtls.Typ = "CRN";
                bill.Billitems.ForEach(x =>
                {
                    x.receivable = Math.Abs(x.receivable);
                    x.tax = Math.Abs(x.tax);
                    x.qty = Math.Abs(x.qty);
                });
                if (bill.additionalchargeslist != null &&
                    bill.additionalchargeslist.Count > 0)
                {
                    bill.additionalchargeslist.ForEach(x =>
                    {
                        x.Tax = Math.Abs(x.Tax);
                        x.Charge = Math.Abs(x.Charge);
                        x.Amount = Math.Abs(x.Amount);
                    });

                }
            }
            else
            {
                reqPlGenIRN.DocDtls.Typ = "INV";
            }


            //Supply Type Code
            reqPlGenIRN.DocDtls.No = bill.billno;
            reqPlGenIRN.DocDtls.Dt = bill.billdate.ToString("dd/MM/yyyy").Replace("-", "/");
            reqPlGenIRN.SellerDtls = new InfyGST.GSTPortal.EInvoiceRequest.SellerDetails();
            reqPlGenIRN.SellerDtls.Gstin = bill.company.gstno;
            reqPlGenIRN.SellerDtls.LglNm = bill.company.name;
            reqPlGenIRN.SellerDtls.TrdNm = null;

            reqPlGenIRN.SellerDtls.Addr1 = bill.company.addressline1;
            reqPlGenIRN.SellerDtls.Addr2 = bill.company.addressline2;
            reqPlGenIRN.SellerDtls.Loc = bill.company.cityname;
            reqPlGenIRN.SellerDtls.Pin = int.Parse("0" + bill.company.pincode.Replace(" ", ""));
            reqPlGenIRN.SellerDtls.State = bill.company.statename;
            reqPlGenIRN.SellerDtls.Stcd = bill.company.gstno.Substring(0, 2);
            reqPlGenIRN.SellerDtls.Ph = bill.company.contactno.Split(',')[0].Replace("-", "");
            reqPlGenIRN.SellerDtls.Em = bill.company.emailid;

            ReferencelistDataService referencelistDataService = new ReferencelistDataService();
            var location = referencelistDataService.Search(new ReferencelistCreteria()
            {
                id = bill.locationid
            }).First();

            bill.location = new CompanyLocation(location);
            if (bill.location.cityid > 0)
            {
                var citylist = referencelistDataService.Search(new ReferencelistCreteria()
                {
                    id = bill.location.cityid
                });
                if (citylist.Count == 1)
                    bill.location.cityname = citylist[0].name;
            }

            if (bill.location.stateid > 0)
            {
                var statelist = referencelistDataService.Search(new ReferencelistCreteria()
                {
                    id = bill.location.stateid
                });
                if (statelist.Count == 1)
                    bill.location.statename = statelist[0].name;
            }

            if (bill.location != null &&
                !string.IsNullOrEmpty(bill.location.cityname) &&
                !string.IsNullOrEmpty(bill.location.pincode) &&
                !string.IsNullOrEmpty(bill.location.addressline1) &&
                !string.IsNullOrEmpty(bill.location.addressline2))
            {
                reqPlGenIRN.DispDtls = new InfyGST.GSTPortal.EInvoiceRequest.DispatchedDetails();
                reqPlGenIRN.DispDtls.Nm = bill.company.name;
                reqPlGenIRN.DispDtls.Addr1 = bill.location.addressline1;
                reqPlGenIRN.DispDtls.Addr2 = bill.location.addressline2;
                reqPlGenIRN.DispDtls.Loc = bill.location.cityname;
                reqPlGenIRN.DispDtls.Pin = int.Parse("0" + bill.location.pincode.Replace(" ", ""));
                reqPlGenIRN.DispDtls.Stcd = bill.company.gstno.Substring(0, 2);
                reqPlGenIRN.DispDtls.State = bill.company.statename;
            }
            else
            {
                reqPlGenIRN.DispDtls = new InfyGST.GSTPortal.EInvoiceRequest.DispatchedDetails();
                reqPlGenIRN.DispDtls.Nm = bill.company.name;
                reqPlGenIRN.DispDtls.Addr1 = bill.company.addressline1;
                reqPlGenIRN.DispDtls.Addr2 = bill.company.addressline2;
                reqPlGenIRN.DispDtls.Loc = bill.company.cityname;
                reqPlGenIRN.DispDtls.Pin = int.Parse("0" + bill.company.pincode);
                reqPlGenIRN.DispDtls.Stcd = bill.company.gstno.Substring(0, 2);
                reqPlGenIRN.DispDtls.State = bill.company.statename;
            }

            if (string.IsNullOrEmpty(bill.customer.gstno))
            {
                reqPlGenIRN.BuyerDtls = new InfyGST.GSTPortal.EInvoiceRequest.BuyerDetails();
                reqPlGenIRN.BuyerDtls.Gstin = "";
                reqPlGenIRN.BuyerDtls.LglNm = string.IsNullOrEmpty(bill.customer.billingname) ? bill.customer.name : bill.customer.billingname;
                reqPlGenIRN.BuyerDtls.TrdNm = null;
                reqPlGenIRN.BuyerDtls.Pos = bill.customer.gstno.Substring(0, 2);
                reqPlGenIRN.BuyerDtls.Addr1 = bill.customer.addressline1;
                reqPlGenIRN.BuyerDtls.Addr2 = string.IsNullOrEmpty(bill.customer.addressline2) ? null : bill.customer.addressline2;
                reqPlGenIRN.BuyerDtls.Loc = bill.customer.cityname;
                reqPlGenIRN.BuyerDtls.Pin = int.Parse("0" + bill.customer.pincode);
                reqPlGenIRN.BuyerDtls.State = bill.customer.statename;
                reqPlGenIRN.BuyerDtls.Stcd = bill.customer.gstno.Substring(0, 2);
                reqPlGenIRN.BuyerDtls.Ph = bill.customer.mobileno;
                reqPlGenIRN.BuyerDtls.Em = string.IsNullOrEmpty(bill.customer.emailid) ? null : bill.customer.emailid;

                reqPlGenIRN.ShipDtls = new InfyGST.GSTPortal.EInvoiceRequest.ShippedDetails();
                reqPlGenIRN.ShipDtls.Gstin = "";
                reqPlGenIRN.ShipDtls.LglNm = string.IsNullOrEmpty(bill.customer.billingname) ? bill.customer.name : bill.customer.billingname;
                reqPlGenIRN.ShipDtls.TrdNm = null;
                reqPlGenIRN.ShipDtls.Addr1 = bill.customer.addressline1;
                reqPlGenIRN.ShipDtls.Addr2 = string.IsNullOrEmpty(bill.customer.addressline2) ? null : bill.customer.addressline2;
                reqPlGenIRN.ShipDtls.Loc = bill.customer.cityname;
                reqPlGenIRN.ShipDtls.Pin = int.Parse("0" + bill.customer.pincode);
                reqPlGenIRN.ShipDtls.Stcd = bill.customer.gstno.Substring(0, 2);
                reqPlGenIRN.ShipDtls.State = bill.customer.statename;

            }
            else
            {
                reqPlGenIRN.BuyerDtls = new InfyGST.GSTPortal.EInvoiceRequest.BuyerDetails();
                reqPlGenIRN.BuyerDtls.Gstin = bill.customer.gstno;
                reqPlGenIRN.BuyerDtls.LglNm = string.IsNullOrEmpty(bill.customer.billingname) ? bill.customer.name : bill.customer.billingname;
                reqPlGenIRN.BuyerDtls.TrdNm = null;
                reqPlGenIRN.BuyerDtls.Pos = bill.customer.gstno.Substring(0, 2);
                reqPlGenIRN.BuyerDtls.Addr1 = bill.customer.addressline1;
                reqPlGenIRN.BuyerDtls.Addr2 = string.IsNullOrEmpty(bill.customer.addressline2) ? null : bill.customer.addressline2;
                reqPlGenIRN.BuyerDtls.Loc = bill.customer.cityname;
                reqPlGenIRN.BuyerDtls.Pin = int.Parse("0" + bill.customer.pincode);
                reqPlGenIRN.BuyerDtls.State = bill.customer.statename;
                reqPlGenIRN.BuyerDtls.Stcd = bill.customer.gstno.Substring(0, 2);
                reqPlGenIRN.BuyerDtls.Ph = bill.customer.mobileno;
                reqPlGenIRN.BuyerDtls.Em = string.IsNullOrEmpty(bill.customer.emailid) ? null : bill.customer.emailid;

                reqPlGenIRN.ShipDtls = new InfyGST.GSTPortal.EInvoiceRequest.ShippedDetails();
                reqPlGenIRN.ShipDtls.Gstin = bill.customer.gstno;
                reqPlGenIRN.ShipDtls.LglNm = string.IsNullOrEmpty(bill.customer.billingname) ? bill.customer.name : bill.customer.billingname;
                reqPlGenIRN.ShipDtls.TrdNm = null;
                reqPlGenIRN.ShipDtls.Addr1 = bill.customer.addressline1;
                reqPlGenIRN.ShipDtls.Addr2 = string.IsNullOrEmpty(bill.customer.addressline2) ? null : bill.customer.addressline2;
                reqPlGenIRN.ShipDtls.Loc = bill.customer.cityname;
                reqPlGenIRN.ShipDtls.Pin = int.Parse("0" + bill.customer.pincode);
                reqPlGenIRN.ShipDtls.Stcd = bill.customer.gstno.Substring(0, 2);
                reqPlGenIRN.ShipDtls.State = bill.customer.statename;
            }




            if (bill.billattributes != null && bill.billattributes.shiptoid > 0)
            {
                CustomerDataService customerDataService = new CustomerDataService() { Context = this.Context };
                var shipto = customerDataService.Search(new CustomerCreteria()
                {
                    id = bill.billattributes.shiptoid
                });
                reqPlGenIRN.ShipDtls.Gstin = shipto[0].gstno;
                reqPlGenIRN.ShipDtls.LglNm = string.IsNullOrEmpty(shipto[0].billingname) ? shipto[0].name : shipto[0].billingname;
                reqPlGenIRN.ShipDtls.TrdNm = null;
                reqPlGenIRN.ShipDtls.Addr1 = shipto[0].addressline1;
                reqPlGenIRN.ShipDtls.Addr2 = string.IsNullOrEmpty(shipto[0].addressline2) ? null : shipto[0].addressline2;
                reqPlGenIRN.ShipDtls.Loc = shipto[0].cityname;
                reqPlGenIRN.ShipDtls.Pin = int.Parse("0" + shipto[0].pincode);
                if (!string.IsNullOrEmpty(shipto[0].gstno))
                {
                    reqPlGenIRN.ShipDtls.Stcd = shipto[0].gstno.Substring(0, 2);
                }
                else
                {
                    reqPlGenIRN.ShipDtls.Stcd = bill.company.gstno.Substring(0, 2);
                }
                reqPlGenIRN.ShipDtls.State = shipto[0].statename;

                if (string.IsNullOrEmpty(reqPlGenIRN.ShipDtls.Gstin) || reqPlGenIRN.ShipDtls.Pin == 0)
                {
                    throw new Exception(("Ship to Customer address doesn't contain gst information - " + reqPlGenIRN.ShipDtls.LglNm));
                }

            }
            else if (bill.customer.hasdeliveryaddress)
            {
                reqPlGenIRN.ShipDtls.Addr1 = bill.customer.deliveryaddressline1;
                reqPlGenIRN.ShipDtls.Addr2 = string.IsNullOrEmpty(bill.customer.deliveryaddressline2) ? null : bill.customer.deliveryaddressline2;
            }

            reqPlGenIRN.ItemList = new List<InfyGST.GSTPortal.EInvoiceRequest.ItmList>();
            int slno = 1;


            foreach (var item in bill.Billitems.GroupBy(x => new { x.productname, x.printingname, x.hsncode, x.sellingmode, x.taxpercentage, unitprice = (double)Math.Round((x.receivable - x.tax) / x.qty, 2) }))
            {
                var tax = Math.Round(item.Sum(x => x.tax), 2);
                var receivable = Math.Round(item.Sum(x => x.receivable), 2);
                InfyGST.GSTPortal.EInvoiceRequest.ItmList itm = new InfyGST.GSTPortal.EInvoiceRequest.ItmList();
                itm.SlNo = slno.ToString();
                itm.IsServc = "N";
                itm.PrdDesc = string.IsNullOrEmpty(item.Key.productname) ? item.Key.printingname : item.Key.productname;
                //itm.PrdDesc = item.productname;
                itm.HsnCd = item.Key.hsncode;
                itm.BchDtls = null;
                itm.Qty = (double)item.Sum(x => x.qty);
                itm.Unit = item.Key.sellingmode == 3 ? "MTR" : "NOS";
                itm.UnitPrice = item.Key.unitprice;
                itm.TotAmt = Math.Round(Convert.ToDouble(receivable - tax), 2);
                itm.Discount = 0.0;
                itm.AssAmt = Math.Round(Convert.ToDouble(receivable - tax), 2);
                itm.GstRt = Math.Round(Convert.ToDouble(item.Key.taxpercentage), 2);
                if (bill.isinterstatesale)
                {
                    itm.IgstAmt = Math.Round(Convert.ToDouble(tax), 2);
                }
                else
                {
                    itm.SgstAmt = Math.Round(Convert.ToDouble(tax / 2), 2);
                    itm.CgstAmt = Math.Round(Convert.ToDouble(tax / 2), 2);
                }
                itm.CesRt = 0.0;
                itm.CesAmt = 0.0;
                itm.CesNonAdvlAmt = 0.0;
                itm.StateCesRt = 0.0;
                itm.StateCesAmt = 0.0;
                itm.StateCesNonAdvlAmt = 0.0;
                itm.OthChrg = 0.0;
                itm.TotItemVal = Math.Round(Convert.ToDouble(receivable), 2);
                itm.AttribDtls = null;
                reqPlGenIRN.ItemList.Add(itm);
                slno++;
            }

            /*foreach (var item in bill.Billitems)
            {
                item.tax = Math.Round(item.tax, 2);
                InfyGST.GSTPortal.EInvoiceRequest.ItmList itm = new InfyGST.GSTPortal.EInvoiceRequest.ItmList();
                itm.SlNo = slno.ToString();
                itm.IsServc = "N";
                itm.PrdDesc = string.IsNullOrEmpty(item.productname) ? item.printingname : item.productname;
                //itm.PrdDesc = item.productname;
                itm.HsnCd = item.hsncode;
                itm.BchDtls = null;
                itm.Qty = (double)item.qty;
                itm.Unit = item.sellingmode == 3 ? "MTR" : "NOS";
                itm.UnitPrice = Math.Round(Convert.ToDouble((item.receivable - item.tax) / item.qty), 2);
                itm.TotAmt = Math.Round(Convert.ToDouble(item.receivable - item.tax), 2);
                itm.Discount = 0.0;
                itm.AssAmt = Math.Round(Convert.ToDouble(item.receivable - item.tax), 2);
                itm.GstRt = Math.Round(Convert.ToDouble(item.taxpercentage), 2);
                if (bill.isinterstatesale)
                {
                    itm.IgstAmt = Math.Round(Convert.ToDouble(item.tax), 2);
                }
                else
                {
                    itm.SgstAmt = Math.Round(Convert.ToDouble(item.tax / 2), 2);
                    itm.CgstAmt = Math.Round(Convert.ToDouble(item.tax / 2), 2);
                }
                itm.CesRt = 0.0;
                itm.CesAmt = 0.0;
                itm.CesNonAdvlAmt = 0.0;
                itm.StateCesRt = 0.0;
                itm.StateCesAmt = 0.0;
                itm.StateCesNonAdvlAmt = 0.0;
                itm.OthChrg = 0.0;
                itm.TotItemVal = Math.Round(Convert.ToDouble(item.receivable), 2);
                itm.AttribDtls = null;
                reqPlGenIRN.ItemList.Add(itm);
                slno++;
            }*/


            if (bill.additionalchargeslist != null && bill.additionalchargeslist.Count > 0)
            {
                var addncharges = referencelistDataService.Search(new ReferencelistCreteria()
                {
                    ids = bill.additionalchargeslist.ConvertAll(x => x.additionalchargestypeid).ToList()
                });

                foreach (var item in bill.additionalchargeslist)
                {
                    item.Tax = Math.Round(item.Tax, 2);
                    InfyGST.GSTPortal.EInvoiceRequest.ItmList itm = new InfyGST.GSTPortal.EInvoiceRequest.ItmList();
                    itm.SlNo = slno.ToString();
                    itm.IsServc = "Y";
                    itm.PrdDesc = item.Name;
                    itm.HsnCd = addncharges.Find(x => x.id == item.additionalchargestypeid).additionalinfo.formula;
                    itm.BchDtls = null;
                    itm.Qty = 1;
                    itm.Unit = "NOS";
                    itm.UnitPrice = Math.Round(Convert.ToDouble(item.Charge), 2);
                    itm.TotAmt = Math.Round(Convert.ToDouble(item.Charge), 2);
                    itm.Discount = 0.0;
                    itm.AssAmt = Math.Round(Convert.ToDouble(item.Charge), 2);
                    itm.GstRt = Math.Round(Convert.ToDouble(item.taxpercentage), 2);
                    if (bill.isinterstatesale)
                    {
                        itm.IgstAmt = Math.Round(Convert.ToDouble(item.Tax), 2);
                    }
                    else
                    {
                        itm.SgstAmt = Math.Round(Convert.ToDouble(item.Tax / 2), 2);
                        itm.CgstAmt = Math.Round(Convert.ToDouble(item.Tax / 2), 2);
                    }
                    itm.CesRt = 0.0;
                    itm.CesAmt = 0.0;
                    itm.CesNonAdvlAmt = 0.0;
                    itm.StateCesRt = 0.0;
                    itm.StateCesAmt = 0.0;
                    itm.StateCesNonAdvlAmt = 0.0;
                    itm.OthChrg = 0.0;
                    itm.TotItemVal = Math.Round(Convert.ToDouble(item.Amount), 2);
                    itm.AttribDtls = null;
                    reqPlGenIRN.ItemList.Add(itm);
                    slno++;
                }
            }
            reqPlGenIRN.PayDtls = null;
            reqPlGenIRN.RefDtls = null;
            reqPlGenIRN.AddlDocDtls = null;
            reqPlGenIRN.ExpDtls = null;
            if (bill.transport != null && !string.IsNullOrEmpty(itemconfig.vehcicleno))
            {
                reqPlGenIRN.EwbDtls = new InfyGST.GSTPortal.EInvoiceRequest.EwbDetails();
                reqPlGenIRN.EwbDtls.TransDocDt = bill.billdate.ToString("dd/MM/yyyy").Replace("-", "/");
                reqPlGenIRN.EwbDtls.TransDocNo = bill.billno;
                reqPlGenIRN.EwbDtls.TransId = bill.transport.gst;
                reqPlGenIRN.EwbDtls.Distance = 0;
                reqPlGenIRN.EwbDtls.VehNo = itemconfig.vehcicleno;
                reqPlGenIRN.EwbDtls.VehType = "R";
                reqPlGenIRN.EwbDtls.TransMode = "1";
                reqPlGenIRN.EwbDtls.TransName = bill.transport.name;
            }
            reqPlGenIRN.ValDtls = new InfyGST.GSTPortal.EInvoiceRequest.ValDetails();
            reqPlGenIRN.ValDtls.AssVal = Math.Round(reqPlGenIRN.ItemList.Sum(e => e.AssAmt), 2);
            reqPlGenIRN.ValDtls.CgstVal = Math.Round(reqPlGenIRN.ItemList.Sum(e => e.CgstAmt), 2);
            reqPlGenIRN.ValDtls.SgstVal = Math.Round(reqPlGenIRN.ItemList.Sum(e => e.SgstAmt), 2);
            reqPlGenIRN.ValDtls.IgstVal = Math.Round(reqPlGenIRN.ItemList.Sum(e => e.IgstAmt), 2);
            reqPlGenIRN.ValDtls.CesVal = 0.0;
            reqPlGenIRN.ValDtls.StCesVal = 0.0;
            var finalreceivable = reqPlGenIRN.ValDtls.AssVal + reqPlGenIRN.ValDtls.CgstVal + reqPlGenIRN.ValDtls.SgstVal + reqPlGenIRN.ValDtls.IgstVal;
            reqPlGenIRN.ValDtls.TotInvVal = Math.Round(Convert.ToDouble(finalreceivable), 0);
            reqPlGenIRN.ValDtls.RndOffAmt = Math.Round(Convert.ToDouble(reqPlGenIRN.ValDtls.TotInvVal) - Math.Round(Convert.ToDouble(finalreceivable), 2), 2);

            if (string.IsNullOrEmpty(reqPlGenIRN.BuyerDtls.Ph) || (reqPlGenIRN.BuyerDtls.Ph != null && reqPlGenIRN.BuyerDtls.Ph.Length < 6)) reqPlGenIRN.BuyerDtls.Ph = null;
            if (string.IsNullOrEmpty(reqPlGenIRN.SellerDtls.Ph) || (reqPlGenIRN.BuyerDtls.Ph != null && reqPlGenIRN.BuyerDtls.Ph.Length < 6)) reqPlGenIRN.SellerDtls.Ph = null;

            var GSTPortal = GetService(bill.companyid);
            var response = GSTPortal.CreateEInvoice(new InfyGST.GSTPortal.InvoiceRequest()
            {
                Data = reqPlGenIRN,
                accesskey = gstaccesskey,
                eInvPwd = einvoicepwd,
                gstin = gstno,
                issandbox = issandbox,
                username = einvoiceusername
            }).Result;

            if (itemconfig.hsn4digit)
            {
                if (response.ErrorDetails != null && response.ErrorDetails.ToList().Exists(e => e.ErrorCode == 2176))
                {
                    var hsnlist = response.ErrorDetails.ToList().Find(e => e.ErrorCode == 2176);
                    var hsncodeList = hsnlist.ErrorMessage.Replace("HSN code(s)-", "").Replace(" is invalid", "").Split(',').ToList();

                    reqPlGenIRN.ItemList.FindAll(x => hsncodeList.Contains(x.HsnCd)).ForEach(e =>
                    {
                        e.HsnCd = e.HsnCd.Substring(0, 4);
                    });

                    response = GSTPortal.CreateEInvoice(new InfyGST.GSTPortal.InvoiceRequest()
                    {
                        Data = reqPlGenIRN,
                        accesskey = gstaccesskey,
                        eInvPwd = einvoicepwd,
                        gstin = gstno,
                        issandbox = issandbox,
                        username = einvoiceusername
                    }).Result;
                }
            }
            response.isb2b = bills[0].isb2b;
            return response;
        }

        internal InfyGST.GSTPortal.EwayFromGRNResponse CancelEinvoice(Voucher voucher)
        {
            bool issandbox = false;
            string gstno = null;
            string einvoiceusername = null;
            string einvoicepwd = null;
            string gstaccesskey = null;


            var param = new SortedList<string, object>();
            param.Add("id", voucher.companyid);
            using (var reader = ExecuteReader("select tinanddate,einvoiceusername,einvoicepwd,gstaccesskey from company where id=@id", param))
            {
                if (reader.Read())
                {
                    gstno = reader.IsDBNull(0) ? "" : reader.GetString(0);
                    einvoiceusername = reader.IsDBNull(1) ? "" : reader.GetString(1);
                    einvoicepwd = reader.IsDBNull(2) ? "" : reader.GetString(2);
                    gstaccesskey = reader.IsDBNull(3) ? "" : reader.GetString(3);
                }
            }


            var GSTPortal = GetService(voucher.companyid);
            InfyGST.GSTPortal.CancellationRequest reqPlGenIRN = new InfyGST.GSTPortal.CancellationRequest();
            reqPlGenIRN.accesskey = gstaccesskey;
            reqPlGenIRN.eInvPwd = einvoicepwd;
            reqPlGenIRN.gstin = gstno;
            reqPlGenIRN.issandbox = issandbox;
            reqPlGenIRN.username = einvoiceusername;
            reqPlGenIRN.irn = voucher.attributes.einv;
            return GSTPortal.CancelEInvoice(reqPlGenIRN).Result;
        }

        internal InfyGST.GSTPortal.EwayFromGRNResponse CancelEinvoice(GST_Invoice itemconfig)
        {
            bool issandbox = false;
            string gstno = null;
            string einvoiceusername = null;
            string einvoicepwd = null;
            string gstaccesskey = null;

            InfyPOS.Data.BillinboxDataService billInboxDataService = new BillinboxDataService() { Context = this.Context };
            var billinboxs = billInboxDataService.Search(new BillinboxCreteria()
            {
                billid = itemconfig.billid
            });

            var billDataService = new BillDataService() { Context = this.Context };
            var bills = billDataService.Search(new BillCreteria()
            {
                id = itemconfig.billid,
                isb2b = true,
                isimport = true
            });


            var param = new SortedList<string, object>();
            param.Add("id", bills[0].companyid);
            using (var reader = ExecuteReader("select tinanddate,einvoiceusername,einvoicepwd,gstaccesskey from company where id=@id", param))
            {
                if (reader.Read())
                {
                    gstno = reader.IsDBNull(0) ? "" : reader.GetString(0);
                    einvoiceusername = reader.IsDBNull(1) ? "" : reader.GetString(1);
                    einvoicepwd = reader.IsDBNull(2) ? "" : reader.GetString(2);
                    gstaccesskey = reader.IsDBNull(3) ? "" : reader.GetString(3);
                }
            }

            TransportDataService transportDataService = new TransportDataService() { Context = this.Context };
            var transport = transportDataService.Search(new TransportCreteria() { id = itemconfig.transportid });

            var GSTPortal = GetService(bills[0].companyid);
            InfyGST.GSTPortal.CancellationRequest reqPlGenIRN = new InfyGST.GSTPortal.CancellationRequest();
            reqPlGenIRN.accesskey = gstaccesskey;
            reqPlGenIRN.eInvPwd = einvoicepwd;
            reqPlGenIRN.gstin = gstno;
            reqPlGenIRN.issandbox = issandbox;
            reqPlGenIRN.username = einvoiceusername;
            reqPlGenIRN.irn = itemconfig.irn;
            return GSTPortal.CancelEInvoice(reqPlGenIRN).Result;
        }
        internal InfyGST.GSTPortal.EwayFromGRNResponse CancelEWay(GST_Invoice itemconfig)
        {
            bool issandbox = false;
            string gstno = null;
            string einvoiceusername = null;
            string einvoicepwd = null;
            string gstaccesskey = null;

            InfyPOS.Data.BillinboxDataService billInboxDataService = new BillinboxDataService() { Context = this.Context };
            var billinboxs = billInboxDataService.Search(new BillinboxCreteria()
            {
                billid = itemconfig.billid
            });

            var billDataService = new BillDataService() { Context = this.Context };
            var bills = billDataService.Search(new BillCreteria()
            {
                id = itemconfig.billid,
                isb2b = true,
                isimport = true
            });


            var param = new SortedList<string, object>();
            param.Add("id", bills[0].companyid);
            using (var reader = ExecuteReader("select tinanddate,einvoiceusername,einvoicepwd,gstaccesskey from company where id=@id", param))
            {
                if (reader.Read())
                {
                    gstno = reader.IsDBNull(0) ? "" : reader.GetString(0);
                    einvoiceusername = reader.IsDBNull(1) ? "" : reader.GetString(1);
                    einvoicepwd = reader.IsDBNull(2) ? "" : reader.GetString(2);
                    gstaccesskey = reader.IsDBNull(3) ? "" : reader.GetString(3);
                }
            }

            TransportDataService transportDataService = new TransportDataService() { Context = this.Context };
            var transport = transportDataService.Search(new TransportCreteria() { id = itemconfig.transportid });

            var GSTPortal = GetService(bills[0].companyid);
            InfyGST.GSTPortal.CancellationRequest reqPlGenIRN = new InfyGST.GSTPortal.CancellationRequest();
            reqPlGenIRN.accesskey = gstaccesskey;
            reqPlGenIRN.eInvPwd = einvoicepwd;
            reqPlGenIRN.gstin = gstno;
            reqPlGenIRN.issandbox = issandbox;
            reqPlGenIRN.username = einvoiceusername;
            reqPlGenIRN.ewaybillno = itemconfig.ewbNo;
            return GSTPortal.CancelEWay(reqPlGenIRN).Result;
        }
        internal InfyGST.GSTPortal.EwayFromGRNResponse CreateEWay4IRN(GST_Invoice itemconfig)
        {
            bool issandbox = false;
            string gstno = null;
            string einvoiceusername = null;
            string einvoicepwd = null;
            string gstaccesskey = null;

            InfyPOS.Data.BillinboxDataService billInboxDataService = new BillinboxDataService() { Context = this.Context };
            var billinboxs = billInboxDataService.Search(new BillinboxCreteria()
            {
                billid = itemconfig.billid
            });

            var billDataService = new BillDataService() { Context = this.Context };
            var bills = billDataService.Search(new BillCreteria()
            {
                id = itemconfig.billid,
                isb2b = true,
                isimport = true
            });


            var param = new SortedList<string, object>();
            param.Add("id", bills[0].companyid);
            using (var reader = ExecuteReader("select tinanddate,einvoiceusername,einvoicepwd,gstaccesskey from company where id=@id", param))
            {
                if (reader.Read())
                {
                    gstno = reader.IsDBNull(0) ? "" : reader.GetString(0);
                    einvoiceusername = reader.IsDBNull(1) ? "" : reader.GetString(1);
                    einvoicepwd = reader.IsDBNull(2) ? "" : reader.GetString(2);
                    gstaccesskey = reader.IsDBNull(3) ? "" : reader.GetString(3);
                }
            }

            TransportDataService transportDataService = new TransportDataService() { Context = this.Context };
            var transport = transportDataService.Search(new TransportCreteria() { id = itemconfig.transportid });

            var GSTPortal = GetService(bills[0].companyid);
            InfyGST.GSTPortal.EWayFromIRNRequest reqPlGenIRN = new InfyGST.GSTPortal.EWayFromIRNRequest();
            reqPlGenIRN.Data = new InfyGST.GSTPortal.Irn2EwayRequest();
            reqPlGenIRN.accesskey = gstaccesskey;
            reqPlGenIRN.eInvPwd = einvoicepwd;
            reqPlGenIRN.gstin = gstno;
            reqPlGenIRN.issandbox = issandbox;
            reqPlGenIRN.username = einvoiceusername;
            reqPlGenIRN.Data.VehNo = itemconfig.vehcicleno;
            reqPlGenIRN.Data.Irn = billinboxs[0].einvoice;
            reqPlGenIRN.Data.TransMode = itemconfig.modeofdelivery <= 0 ? "1" : itemconfig.modeofdelivery.ToString("F0");

            if (itemconfig.ewaypartb)
            {
                if (billinboxs[0].modeofdelivery == 0) billinboxs[0].modeofdelivery = 1;
                reqPlGenIRN.Data.TransMode = billinboxs[0].modeofdelivery.ToString("F0");
                reqPlGenIRN.Data.TransDocDt = bills[0].billdate.ToString("dd/MM/yyyy").Replace("-", "/");
                reqPlGenIRN.Data.TransDocNo = bills[0].billno;
                reqPlGenIRN.Data.TransName = transport[0].name;
                reqPlGenIRN.Data.Distance = itemconfig.distance;
                reqPlGenIRN.Data.TransId = String.IsNullOrEmpty(transport[0].gst) ? null : transport[0].gst;
                reqPlGenIRN.Data.VehType = string.IsNullOrEmpty(reqPlGenIRN.Data.VehNo) ? "O" : "R";
                reqPlGenIRN.Data.VehNo = String.IsNullOrEmpty(itemconfig.vehcicleno) ? null : itemconfig.vehcicleno;
            }
            else
            {
                if (!string.IsNullOrEmpty(reqPlGenIRN.Data.VehNo))
                {
                    reqPlGenIRN.Data.VehType = string.IsNullOrEmpty(reqPlGenIRN.Data.VehNo) ? "O" : "R";
                    reqPlGenIRN.Data.TransMode = billinboxs[0].modeofdelivery <= 0 ? "1" : billinboxs[0].modeofdelivery.ToString("F0");
                    reqPlGenIRN.Data.VehNo = String.IsNullOrEmpty(itemconfig.vehcicleno) ? null : itemconfig.vehcicleno;
                    reqPlGenIRN.Data.TransDocDt = bills[0].billdate.ToString("dd/MM/yyyy").Replace("-", "/");
                    reqPlGenIRN.Data.TransDocNo = bills[0].billno;
                }
                else
                {
                    reqPlGenIRN.Data.TransMode = null;
                    reqPlGenIRN.Data.VehType = null;
                    reqPlGenIRN.Data.VehNo = null;
                    reqPlGenIRN.Data.TransDocDt = null;
                    reqPlGenIRN.Data.TransDocNo = null;
                }
            }
            reqPlGenIRN.Data.TransName = transport[0].name;
            reqPlGenIRN.Data.TransId = String.IsNullOrEmpty(transport[0].gst) ? null : transport[0].gst;
            reqPlGenIRN.Data.Distance = itemconfig.distance;
            if (string.IsNullOrEmpty(transport[0].gst))
            {
                reqPlGenIRN.Data.TransName = null;
                reqPlGenIRN.Data.TransId = null;
            }
            return GSTPortal.CreateEWay4IRN(reqPlGenIRN).Result;
        }

        internal InfyGST.GSTPortal.EWAYbillResponse CreateEWay4Transfer(GST_Transfer itemconfig)
        {
            bool issandbox = false;
            string gstno = null;
            string einvoiceusername = null;
            string einvoicepwd = null;
            string gstaccesskey = null;

            InfyPOS.Data.BundlesDataService billDataService = new BundlesDataService(true) { Context = this.Context };
            var bills = billDataService.Search(new BundlesCreteria()
            {
                id = itemconfig.bundleid
            });



            ProductsDataService productsDataService = new ProductsDataService() { Context = this.Context };
            var productlist = productsDataService.Search(new ProductsCreteria()
            {
                ids = bills[0].items.ConvertAll(e => e.productid).Distinct().ToList()
            });

            SortedDictionary<long, Tax> taxpercentages = new SortedDictionary<long, Tax>();
            TaxDataService taxDataService = new TaxDataService() { Context = this.Context };
            var taxlist = taxDataService.Search(new TaxCreteria());
            productlist.ForEach(x =>
            {
                //decimal tax = 0;
                var taxitem = taxlist.Find(y => y.id == x.salestaxid);
                if (taxitem != null)
                {
                    taxpercentages.Add(x.id, taxitem);
                    //if (taxitem.taxpercentage == 0 ||
                    //(taxitem.taxsplit != null &&
                    //taxitem.taxsplit.items != null &&
                    //taxitem.taxsplit.items.Count > 0))
                    //{
                    //    tax = taxitem.taxsplit.items.Sum(z => z.igst);
                    //}
                    //else
                    //{
                    //    tax = taxitem.taxpercentage;
                    //}
                }
            });

            InfyPOS.Data.CompanyDataService company = new CompanyDataService() { Context = this.Context };
            bills[0].fromcompany = company.Search(new CompanyCreteria
            {
                id = bills[0].fromcompanyid
            }).First();

            bills[0].tocompany = company.Search(new CompanyCreteria
            {
                id = bills[0].tocompanyid
            }).First();

            ReferencelistDataService referencelist = new ReferencelistDataService() { Context = this.Context };
            var fromlocation = new CompanyLocation(referencelist.Search(new ReferencelistCreteria() { id = bills[0].fromlocationid }).First());
            var tolocation = new CompanyLocation(referencelist.Search(new ReferencelistCreteria() { id = bills[0].tolocationid }).First());

            if (bills[0].tocompanyid == 0 || bills[0].fromcompanyid == 0)
            {
                var companylist = company.Search(new CompanyCreteria
                {
                });
                if (companylist.Count > 1)
                {
                    if (fromlocation.companyid == 0 || tolocation.companyid == 0)
                    {
                        throw new Exception("Please map the company against the transfering location");
                    }
                    bills[0].fromcompany = company.Search(new CompanyCreteria
                    {
                        id = fromlocation.companyid
                    }).First();

                    bills[0].tocompany = company.Search(new CompanyCreteria
                    {
                        id = tolocation.companyid
                    }).First();
                }
            }

            if (fromlocation.cityid > 0)
            {
                var city = referencelist.Search(new ReferencelistCreteria() { id = fromlocation.cityid });
                fromlocation.cityname = city.Count == 1 ? city[0].name : "";
            }
            if (tolocation.cityid > 0)
            {
                var city = referencelist.Search(new ReferencelistCreteria() { id = tolocation.cityid });
                tolocation.cityname = city.Count == 1 ? city[0].name : "";
            }

            var param = new SortedList<string, object>();
            param.Add("id", bills[0].fromcompany.id);
            using (var reader = ExecuteReader("select tinanddate,einvoiceusername,einvoicepwd,gstaccesskey from company where id=@id", param))
            {
                if (reader.Read())
                {
                    gstno = reader.IsDBNull(0) ? "" : reader.GetString(0);
                    einvoiceusername = reader.IsDBNull(1) ? "" : reader.GetString(1);
                    einvoicepwd = reader.IsDBNull(2) ? "" : reader.GetString(2);
                    gstaccesskey = reader.IsDBNull(3) ? "" : reader.GetString(3);
                }
            }

            if (itemconfig.transportid > 0)
            {
                InfyPOS.Data.TransportDataService transportDataService = new TransportDataService() { Context = this.Context };
                var transport = transportDataService.Search(new TransportCreteria()
                {
                    id = itemconfig.transportid
                });
                bills[0].transport = transport[0];
            }

            var bill = bills[0];

            InfyGST.GSTPortal.EWAYBill ewbGen = new InfyGST.GSTPortal.EWAYBill();
            ewbGen.supplyType = "O";
            ewbGen.subSupplyType = string.IsNullOrEmpty(itemconfig.subSupplyType) ? "11": itemconfig.subSupplyType;
            ewbGen.subSupplyDesc = string.IsNullOrEmpty(itemconfig.subSupplyDesc) ? "": itemconfig.subSupplyDesc;
            ewbGen.docType = "CHL";
            ewbGen.docNo = bill.code;
            ewbGen.docDate = bill.packedon.ToString("dd/MM/yyyy").Replace("-", "/");
            ewbGen.transDocDate = bill.packedon.ToString("dd/MM/yyyy").Replace("-", "/");
            ewbGen.fromGstin = bill.fromcompany.gstno;//"07AACCC1596Q1Z4";
            ewbGen.fromTrdName = bill.fromcompany.name;

            ewbGen.toGstin = bill.tocompany.gstno;
            ewbGen.toTrdName = bill.tocompany.name;

            ewbGen.fromStateCode = bill.fromcompany.gstno.Substring(0, 2);
            ewbGen.actFromStateCode = bill.fromcompany.gstno.Substring(0, 2);
            ewbGen.toStateCode = bill.tocompany.gstno.Substring(0, 2);
            ewbGen.actToStateCode = bill.tocompany.gstno.Substring(0, 2);

            if (bills[0].tocompanyid == bills[0].fromcompanyid)
            {
                ewbGen.fromAddr1 = fromlocation.addressline1;
                ewbGen.fromAddr2 = fromlocation.addressline2;
                ewbGen.fromPlace = fromlocation.cityname;
                ewbGen.fromPincode = int.Parse("0"+fromlocation.pincode);

                ewbGen.toAddr1 = tolocation.addressline1;
                ewbGen.toAddr2 = tolocation.addressline2;
                ewbGen.toPlace = tolocation.cityname;
                ewbGen.toPincode = int.Parse("0" + tolocation.pincode);
            }
            else
            {
                ewbGen.fromAddr1 = bill.fromcompany.addressline1;
                ewbGen.fromAddr2 = bill.fromcompany.addressline2;
                ewbGen.fromPlace = bill.fromcompany.cityname;
                ewbGen.fromPincode = int.Parse("0" + bill.fromcompany.pincode);

                ewbGen.toAddr1 = bill.tocompany.addressline1;
                ewbGen.toAddr2 = bill.tocompany.addressline2;
                ewbGen.toPlace = bill.tocompany.cityname;
                ewbGen.toPincode = int.Parse("0" + bill.tocompany.pincode);
            }


            ewbGen.transactionType = 1;
            ewbGen.dispatchFromGSTIN = bill.tocompany.gstno;
            ewbGen.dispatchFromTradeName = bill.tocompany.name;
            ewbGen.shipToGSTIN = bill.tocompany.gstno; //29ALSPR1722R1Z3
            ewbGen.shipToTradeName = bill.tocompany.name;
            ewbGen.cessNonAdvolValue = 0;
            ewbGen.transporterId = bills[0].transport.gst;
            ewbGen.transporterName = bills[0].transport.name;
            ewbGen.transDocNo = "";
            ewbGen.totInvValue = 0;
            if (itemconfig.ewaypartb)
            {
                ewbGen.transMode = itemconfig.modeofdelivery.ToString("F0");
                ewbGen.transDistance = itemconfig.distance != 0 ? itemconfig.distance.ToString("F0") : "0"; /*1200*/
                ewbGen.vehicleNo = itemconfig.vehcicleno;//PVC1234
                ewbGen.vehicleType = "R";//R
                if (string.IsNullOrEmpty(itemconfig.vehcicleno))
                    ewbGen.vehicleType = "O";

            }
            else

            {
                ewbGen.transMode = itemconfig.modeofdelivery <= 0 ? "1" : itemconfig.modeofdelivery.ToString("F0");
                ewbGen.transDistance = itemconfig.distance != 0 ? itemconfig.distance.ToString("F0") : "0"; /*1200*/
                ewbGen.vehicleNo = itemconfig.vehcicleno;//PVC1234
                ewbGen.vehicleType = string.IsNullOrEmpty(itemconfig.vehcicleno) ? "O" : "R";//R
            }

            //itemconfig.modeofdelivery.ToString("F0")

            if (string.IsNullOrEmpty(bills[0].transport.gst))
            {
                ewbGen.transporterName = null;
                ewbGen.transporterId = null;
            }

            ewbGen.itemList = new List<InfyGST.GSTPortal.EWAYBill.ItemList>();

            var zeroprices = bill.items.FindAll(x => x.price == 0);
            zeroprices.ForEach(x => x.price = x.mrp);
            zeroprices = bill.items.FindAll(x => x.price == 0);
            zeroprices.ForEach(x => x.price = 1);

            List<long> stockids = bill.items.ConvertAll(x => x.stockid).Distinct().ToList();
            using (var reader = ExecuteReader("Select id,hsncode from stock where id in (" + string.Join(",", stockids) + ")", null))
            {
                while (reader.Read())
                {
                    long id = Convert.ToInt64(reader["id"]);
                    string hsncode = reader["hsncode"] == DBNull.Value ? "" : reader["hsncode"].ToString();
                    if (!string.IsNullOrEmpty(hsncode))
                        bill.items.FindAll(x => x.stockid == id).ForEach(y => y.hsn = hsncode);
                }
            }

            if (ewbGen.subSupplyType == "5")
            {
                billDataService.UpdateCostPrice(bill);
                foreach (var item in bill.items.GroupBy(x => new { x.productid, x.price }))
                {
                   
                    var receivable = Math.Round(item.Sum(x => x.price * x.iqty), 2);
                    InfyGST.GSTPortal.EWAYBill.ItemList itm = new InfyGST.GSTPortal.EWAYBill.ItemList();
                    itm.productDesc = item.First().product;
                    itm.productName = item.First().product;

                    if (string.IsNullOrEmpty(itm.productName))
                    {
                        if (productlist.Exists(x => x.id == item.Key.productid))
                        {
                            var prd = productlist.Find(x => x.id == item.Key.productid);
                            itm.productName = prd.name;
                            itm.productDesc = prd.name;
                        }
                    }
                    if (itemconfig.hsn8digit &&
                        item.Any(x => !string.IsNullOrEmpty(x.hsn) && x.hsn.Length >= 8))
                    {
                        int hsn;
                        if (int.TryParse(item.Where(x => !string.IsNullOrEmpty(x.hsn) && x.hsn.Length >= 8).First().hsn, out hsn))
                        {
                            itm.hsnCode = hsn;
                        }
                    }
                    else if (itemconfig.hsn6digit &&
                        item.Any(x => !string.IsNullOrEmpty(x.hsn) && x.hsn.Length >= 6))
                    {
                        int hsn;
                        if (int.TryParse(item.Where(x => !string.IsNullOrEmpty(x.hsn) && x.hsn.Length >= 6).First().hsn, out hsn))
                        {
                            itm.hsnCode = hsn;
                        }
                    }
                    if (itm.hsnCode <= 0)
                    {
                        if (item.Any(x => !string.IsNullOrEmpty(x.hsn)))
                        {
                            int hsn;
                            if (int.TryParse(item.Where(x => !string.IsNullOrEmpty(x.hsn)).First().hsn, out hsn))
                            {
                                itm.hsnCode = hsn;
                            }
                        }
                        if (itm.hsnCode <= 0 && productlist.Exists(x => x.id == item.Key.productid))
                            itm.hsnCode = int.Parse("0" + productlist.Find(x => x.id == item.Key.productid).hsncode);
                    }
                    itm.quantity = (decimal)item.Sum(x => x.iqty);
                    itm.qtyUnit = productlist.Find(x => x.id == item.Key.productid).sellingmode == 3 ? "MTR" : "NOS";
                    itm.taxableAmount = Math.Round(Convert.ToDecimal(receivable), 0);
                    ewbGen.itemList.Add(itm);
                }
            }
            else
            {
                foreach (var item in bill.items.GroupBy(x => new { x.productid, x.price }))
                {
                    decimal taxpercentage = 0;
                    if (taxpercentages[item.Key.productid].taxsplit.items.Count > 1)
                    {
                        taxpercentage = taxpercentages[item.Key.productid].taxsplit.Find(item.Key.price).taxpercent;
                    }
                    else
                    {
                        taxpercentage = taxpercentages[item.Key.productid].taxpercentage;
                    }
                    var receivable = Math.Round(item.Sum(x => x.price * x.iqty), 2);
                    receivable = receivable / (taxpercentage + 100) * 100;
                    InfyGST.GSTPortal.EWAYBill.ItemList itm = new InfyGST.GSTPortal.EWAYBill.ItemList();
                    itm.productDesc = item.First().product;
                    itm.productName = item.First().product;

                    if (string.IsNullOrEmpty(itm.productName))
                    {
                        if (productlist.Exists(x => x.id == item.Key.productid))
                        {
                            var prd = productlist.Find(x => x.id == item.Key.productid);
                            itm.productName = prd.name;
                            itm.productDesc = prd.name;
                        }
                    }
                    if (itemconfig.hsn8digit &&
                        item.Any(x => !string.IsNullOrEmpty(x.hsn) && x.hsn.Length >= 8))
                    {
                        int hsn;
                        if (int.TryParse(item.Where(x => !string.IsNullOrEmpty(x.hsn) && x.hsn.Length >= 8).First().hsn, out hsn))
                        {
                            itm.hsnCode = hsn;
                        }
                    }
                    else if (itemconfig.hsn6digit &&
                        item.Any(x => !string.IsNullOrEmpty(x.hsn) && x.hsn.Length >= 6))
                    {
                        int hsn;
                        if (int.TryParse(item.Where(x => !string.IsNullOrEmpty(x.hsn) && x.hsn.Length >= 6).First().hsn, out hsn))
                        {
                            itm.hsnCode = hsn;
                        }
                    }
                    if (itm.hsnCode <= 0)
                    {
                        if (item.Any(x => !string.IsNullOrEmpty(x.hsn)))
                        {
                            int hsn;
                            if (int.TryParse(item.Where(x => !string.IsNullOrEmpty(x.hsn)).First().hsn, out hsn))
                            {
                                itm.hsnCode = hsn;
                            }
                        }
                        if (itm.hsnCode <= 0 && productlist.Exists(x => x.id == item.Key.productid))
                            itm.hsnCode = int.Parse("0" + productlist.Find(x => x.id == item.Key.productid).hsncode);
                    }
                    itm.quantity = (decimal)item.Sum(x => x.iqty);
                    itm.qtyUnit = productlist.Find(x => x.id == item.Key.productid).sellingmode == 3 ? "MTR" : "NOS";
                    itm.taxableAmount = Math.Round(Convert.ToDecimal(receivable), 0);
                    if (bill.fromcompany.gstno.Substring(0, 2) != bill.tocompany.gstno.Substring(0, 2))
                    {
                        itm.igstRate = Math.Round(Convert.ToDecimal(taxpercentage), 2);
                    }
                    else
                    {
                        itm.sgstRate = Math.Round(Convert.ToDecimal(taxpercentage / 2), 2);
                        itm.cgstRate = Math.Round(Convert.ToDecimal(taxpercentage / 2), 2);
                    }
                    ewbGen.itemList.Add(itm);
                }
            }

            ewbGen.otherValue = 0;

            ewbGen.cgstValue = Math.Round(ewbGen.itemList.Sum(x => x.cgstRate * x.taxableAmount / 100), 0);
            ewbGen.sgstValue = Math.Round(ewbGen.itemList.Sum(x => x.sgstRate * x.taxableAmount / 100), 0);
            ewbGen.igstValue = Math.Round(ewbGen.itemList.Sum(x => x.igstRate * x.taxableAmount / 100), 0);
            ewbGen.totalValue = ewbGen.itemList.Sum(x => x.taxableAmount);
            //+ ewbGen.cgstValue + ewbGen.sgstValue + ewbGen.igstValue;
            ewbGen.totalValue = Math.Round(ewbGen.totalValue, 0);
            ewbGen.cessValue = 0;

            var GSTPortal = GetService(bills[0].fromcompany.id);
            var response = GSTPortal.CreateEWay(new InfyGST.GSTPortal.EWayRequest()
            {
                Data = ewbGen,
                accesskey = gstaccesskey,
                ewbpwd = einvoicepwd,
                gstin = gstno,
                issandbox = issandbox,
                username = einvoiceusername
            }).Result;

            return response;
        }
        internal InfyGST.GSTPortal.EWAYbillResponse CreateEWay4Bill(GST_Invoice itemconfig)
        {
            bool issandbox = false;
            string gstno = null;
            string einvoiceusername = null;
            string einvoicepwd = null;
            string gstaccesskey = null;

            InfyPOS.Data.BillDataService billDataService = new BillDataService() { Context = this.Context };
            var bills = billDataService.Search(new BillCreteria()
            {
                id = itemconfig.billid,
                isb2b = true,
                isimport = true
            });

            var param = new SortedList<string, object>();
            param.Add("id", bills[0].companyid);
            using (var reader = ExecuteReader("select tinanddate,einvoiceusername,einvoicepwd,gstaccesskey from company where id=@id", param))
            {
                if (reader.Read())
                {
                    gstno = reader.IsDBNull(0) ? "" : reader.GetString(0);
                    einvoiceusername = reader.IsDBNull(1) ? "" : reader.GetString(1);
                    einvoicepwd = reader.IsDBNull(2) ? "" : reader.GetString(2);
                    gstaccesskey = reader.IsDBNull(3) ? "" : reader.GetString(3);
                }
            }

            ProductsDataService productsDataService = new ProductsDataService() { Context = this.Context };
            var productlist = productsDataService.Search(new ProductsCreteria()
            {
                ids = bills[0].Billitems.ConvertAll(e => e.productid).Distinct().ToList()
            });


            InfyPOS.Data.CompanyDataService company = new CompanyDataService() { Context = this.Context };
            bills[0].company = company.Search(new CompanyCreteria
            {
                id = bills[0].companyid
            }).First();


            if (itemconfig.transportid > 0)
            {
                InfyPOS.Data.TransportDataService transportDataService = new TransportDataService() { Context = this.Context };
                var transport = transportDataService.Search(new TransportCreteria()
                {
                    id = itemconfig.transportid
                });
                bills[0].transport = transport[0];
            }

            if (issandbox)
            {
                gstno = "34AACCC1596Q002";
                gstaccesskey = "34AACCC1596Q002";
                bills[0].company.tinanddate = gstno;
                bills[0].isinterstatesale = !bills[0].customer.gstno.StartsWith("34");
                bills[0].company.cityname = "PONDICHERRY";
                bills[0].company.pincode = "605001";
            }

            var bill = bills[0];


            InfyGST.GSTPortal.EWAYBill ewbGen = new InfyGST.GSTPortal.EWAYBill();
            ewbGen.supplyType = "O";
            ewbGen.subSupplyType = "1";
            ewbGen.subSupplyDesc = "";
            ewbGen.docType = "INV";
            ewbGen.docNo = bill.billno;
            ewbGen.docDate = bill.billdate.ToString("dd/MM/yyyy").Replace("-","/");
            ewbGen.fromGstin = bill.company.gstno;//"07AACCC1596Q1Z4";
            ewbGen.fromTrdName = bill.company.name;
            ewbGen.fromAddr1 = bill.company.addressline1;
            ewbGen.fromAddr2 = bill.company.addressline2;
            ewbGen.fromPlace = bill.company.cityname;
            ewbGen.fromPincode = int.Parse("0" + bill.company.pincode);
            ewbGen.fromStateCode = bill.company.gstno.Substring(0, 2);
            ewbGen.actFromStateCode = bill.company.gstno.Substring(0, 2);

            ewbGen.toTrdName = bill.customer.billingname;
            ewbGen.toAddr1 = bill.customer.addressline1;
            ewbGen.toAddr2 = bill.customer.addressline2;
            ewbGen.toPlace = bill.customer.cityname;
            ewbGen.toPincode = int.Parse("0" + bill.customer.pincode);
            ewbGen.shipToTradeName = bill.customer.billingname;

            if (string.IsNullOrEmpty(bill.customer.gstno))
            {
                ewbGen.toStateCode = ewbGen.fromStateCode;
                ewbGen.actToStateCode = ewbGen.actFromStateCode;
            }
            else
            {
                ewbGen.toGstin = bill.customer.gstno;
                ewbGen.toStateCode = bill.customer.gstno.Substring(0, 2);
                ewbGen.shipToGSTIN = bill.customer.gstno; //29ALSPR1722R1Z3
                ewbGen.actToStateCode = bill.customer.gstno.Substring(0, 2);
            }
            if (bill.billattributes != null && bill.billattributes.shiptoid > 0)
            {
                CustomerDataService customerDataService = new CustomerDataService() { Context = this.Context };
                var shipto = customerDataService.Search(new CustomerCreteria()
                {
                    id = bill.billattributes.shiptoid
                });
                ewbGen.shipToTradeName = string.IsNullOrEmpty(shipto[0].billingname) ? shipto[0].name : shipto[0].billingname;
                ewbGen.shipToGSTIN = shipto[0].gstno;
                ewbGen.toAddr1 = shipto[0].addressline1;
                ewbGen.toAddr2 = string.IsNullOrEmpty(shipto[0].addressline2) ? null : shipto[0].addressline2;
                ewbGen.toPlace = shipto[0].cityname;
                ewbGen.toPincode = int.Parse("0" + shipto[0].pincode);
                ewbGen.toStateCode = shipto[0].gstno.Substring(0, 2);
                ewbGen.actToStateCode = shipto[0].gstno.Substring(0, 2);
            }

            ewbGen.dispatchFromTradeName = bill.customer.billingname;
            ewbGen.transactionType = 1;
            ewbGen.dispatchFromGSTIN = bill.customer.gstno;
            ewbGen.cessNonAdvolValue = 0;
            if (itemconfig.ewaypartb)
            {
                ewbGen.transporterId = bills[0].transport.gst;
                ewbGen.transporterName = bills[0].transport.name;
                ewbGen.transDocNo = "";
                ewbGen.totInvValue = 0;
                ewbGen.transMode = "1";//1
                ewbGen.transDistance = itemconfig.distance != 0 ? itemconfig.distance.ToString("F0") : "0"; /*1200*/
                ewbGen.transDocDate = "";
                ewbGen.vehicleNo = itemconfig.vehcicleno;//PVC1234
                ewbGen.vehicleType = "R";//R
            }
            else
            {
                ewbGen.transMode = itemconfig.modeofdelivery > 0 ? itemconfig.modeofdelivery.ToString("N0") : "1";//1
                ewbGen.transDistance = itemconfig.distance != 0 ? itemconfig.distance.ToString("F0") : "0"; /*1200*/
                ewbGen.transporterName = bills[0].transport.name;
                ewbGen.transporterId = bills[0].transport.gst;
                ewbGen.transDocNo = "";
                ewbGen.transDocDate = "";
                ewbGen.vehicleNo = itemconfig.vehcicleno;//PVC1234
                ewbGen.vehicleType = string.IsNullOrEmpty(itemconfig.vehcicleno) ? "O" : "R";//R
                ewbGen.totInvValue = 0;
            }
            ewbGen.itemList = new List<InfyGST.GSTPortal.EWAYBill.ItemList>();
            foreach (var item in bill.Billitems.GroupBy(x => new { x.productname, x.printingname, x.hsncode, x.sellingmode, x.taxpercentage, unitprice = (double)Math.Round((x.receivable - x.tax) / x.qty, 2) }))
            {
                var tax = Math.Round(item.Sum(x => x.tax), 2);
                var receivable = Math.Round(item.Sum(x => x.receivable), 2);
                InfyGST.GSTPortal.EWAYBill.ItemList itm = new InfyGST.GSTPortal.EWAYBill.ItemList();
                itm.productDesc = string.IsNullOrEmpty(item.Key.productname) ? item.Key.printingname : item.Key.productname;
                itm.productName = string.IsNullOrEmpty(item.Key.productname) ? item.Key.printingname : item.Key.productname;
                itm.hsnCode = int.Parse("0" + item.Key.hsncode);
                itm.quantity = (decimal)item.Sum(x => x.qty);
                itm.qtyUnit = item.Key.sellingmode == 3 ? "MTR" : "NOS";
                itm.taxableAmount = Math.Round(Convert.ToDecimal(receivable - tax), 2);
                if (bill.isinterstatesale)
                {
                    itm.igstRate = Math.Round(Convert.ToDecimal(item.Key.taxpercentage), 2);
                }
                else
                {
                    itm.sgstRate = Math.Round(Convert.ToDecimal(item.Key.taxpercentage / 2), 2);
                    itm.cgstRate = Math.Round(Convert.ToDecimal(item.Key.taxpercentage / 2), 2);
                }
                ewbGen.itemList.Add(itm);
            }

            ewbGen.otherValue = 0;

            ewbGen.cgstValue = Math.Round(ewbGen.itemList.Sum(x => x.cgstRate * x.taxableAmount / 100),2,MidpointRounding.ToEven);
            ewbGen.sgstValue = Math.Round(ewbGen.itemList.Sum(x => x.sgstRate * x.taxableAmount / 100), 2, MidpointRounding.ToEven);
            ewbGen.igstValue = Math.Round(ewbGen.itemList.Sum(x => x.igstRate * x.taxableAmount / 100), 2, MidpointRounding.ToEven);
            ewbGen.totalValue = Math.Round(ewbGen.itemList.Sum(x => x.taxableAmount), 2, MidpointRounding.ToEven);
            //ewbGen.totalValue += ewbGen.cgstValue + ewbGen.sgstValue + ewbGen.igstValue;
            ewbGen.cessValue = 0;



            var GSTPortal = GetService(bill.companyid);
            var response = GSTPortal.CreateEWay(new InfyGST.GSTPortal.EWayRequest()
            {
                Data = ewbGen,
                accesskey = gstaccesskey,
                ewbpwd = einvoicepwd,
                gstin = gstno,
                issandbox = issandbox,
                username = einvoiceusername
            }).Result;

            return response;
        }

        internal InfyGST.GSTPortal.EWAYbillResponse CreateEWay4DebitNote(GST_Invoice itemconfig)
        {
            bool issandbox = false;
            string gstno = null;
            string einvoiceusername = null;
            string einvoicepwd = null;
            string gstaccesskey = null;

            InfyPOS.Data.LrinvoicereturnDataService billDataService = new LrinvoicereturnDataService() { Context = this.Context };
            var bills = billDataService.Print(new  Lrinvoicereturn()
            {
                id = itemconfig.lrinvoicereturnid
            });

            var param = new SortedList<string, object>();
            param.Add("id", bills.companyid);
            using (var reader = ExecuteReader("select tinanddate,einvoiceusername,einvoicepwd,gstaccesskey from company where id=@id", param))
            {
                if (reader.Read())
                {
                    gstno = reader.IsDBNull(0) ? "" : reader.GetString(0);
                    einvoiceusername = reader.IsDBNull(1) ? "" : reader.GetString(1);
                    einvoicepwd = reader.IsDBNull(2) ? "" : reader.GetString(2);
                    gstaccesskey = reader.IsDBNull(3) ? "" : reader.GetString(3);
                }
            }

            ProductsDataService productsDataService = new ProductsDataService() { Context = this.Context };
            var productlist = productsDataService.Search(new ProductsCreteria()
            {
                ids = bills.Lrinvoicereturnitems.ConvertAll(e => e.productid).Distinct().ToList()
            });

            if (bills.company == null)
            {
                InfyPOS.Data.CompanyDataService company = new CompanyDataService() { Context = this.Context };
                bills.company = company.Search(new CompanyCreteria
                {
                    id = bills.companyid
                }).First();
            }

            if (itemconfig.transportid > 0)
            {
                InfyPOS.Data.TransportDataService transportDataService = new TransportDataService() { Context = this.Context };
                var transport = transportDataService.Search(new TransportCreteria()
                {
                    id = itemconfig.transportid
                });
                bills.transport = transport[0];
            }

            if (issandbox)
            {
                gstno = "34AACCC1596Q002";
                gstaccesskey = "34AACCC1596Q002";
                bills.company.tinanddate = gstno;
                bills.isinterstatesale = !bills.supplier.gstno.StartsWith("34");
                bills.company.cityname = "PONDICHERRY";
                bills.company.pincode = "605001";
            }


            var bill = bills;

            int tryno = 0;
            if (!int.TryParse(bill.company.pincode, out tryno))
                throw new Exception("Pleae enter the company pin code");

            if (!int.TryParse(bill.supplier.pincode, out tryno))
                throw new Exception("Pleae enter the supplier pin code");


            InfyGST.GSTPortal.EWAYBill ewbGen = new InfyGST.GSTPortal.EWAYBill();
            ewbGen.supplyType = "O";
            ewbGen.subSupplyType = "9";
            ewbGen.subSupplyDesc = "Purchase Return";
            ewbGen.docType = "INV";
            ewbGen.transactionType = 1;

            ewbGen.docNo = bill.returnno;
            ewbGen.docDate = bill.returndate.ToString("dd/MM/yyyy").Replace("-","/");

            ewbGen.fromGstin = bill.company.gstno;//"07AACCC1596Q1Z4";
            ewbGen.fromTrdName = bill.company.name;
            ewbGen.fromAddr1 = bill.company.addressline1;
            ewbGen.fromAddr2 = bill.company.addressline2;
            ewbGen.fromPlace = bill.company.cityname;
            ewbGen.fromPincode = int.Parse("0" + bill.company.pincode);
            ewbGen.fromStateCode = bill.company.gstno.Substring(0, 2);
            ewbGen.actFromStateCode = bill.company.gstno.Substring(0, 2);


            ewbGen.toGstin = bill.supplier.gstno;
            ewbGen.toTrdName = bill.supplier.name;
            ewbGen.toAddr1 = bill.supplier.addressline1;
            ewbGen.toAddr2 = bill.supplier.addressline2;
            ewbGen.toPlace = bill.supplier.cityname;
            ewbGen.toPincode = int.Parse("0" + bill.supplier.pincode);
            ewbGen.toStateCode = bill.supplier.gstno.Substring(0, 2);
            ewbGen.shipToGSTIN = bill.supplier.gstno; //29ALSPR1722R1Z3
            ewbGen.shipToTradeName = bill.supplier.name;
            ewbGen.actToStateCode = bill.supplier.gstno.Substring(0, 2);

            //if (bill.billattributes != null && bill.billattributes.shiptoid > 0)
            //{
            //    CustomerDataService customerDataService = new CustomerDataService() { Context = this.Context };
            //    var shipto = customerDataService.Search(new CustomerCreteria()
            //    {
            //        id = bill.billattributes.shiptoid
            //    });
            //    ewbGen.shipToTradeName = string.IsNullOrEmpty(shipto[0].billingname) ? shipto[0].name : shipto[0].billingname;
            //    ewbGen.shipToGSTIN = shipto[0].gstno;
            //    ewbGen.toAddr1 = shipto[0].addressline1;
            //    ewbGen.toAddr2 = string.IsNullOrEmpty(shipto[0].addressline2) ? null : shipto[0].addressline2;
            //    ewbGen.toPlace = shipto[0].cityname;
            //    ewbGen.toPincode = int.Parse("0" + shipto[0].pincode);
            //    ewbGen.toStateCode = shipto[0].gstno.Substring(0, 2);
            //    ewbGen.actToStateCode = shipto[0].gstno.Substring(0, 2);
            //}

            ewbGen.dispatchFromTradeName = bill.supplier.name;
            ewbGen.transactionType = 1;
            ewbGen.dispatchFromGSTIN = bill.supplier.gstno;
            ewbGen.cessNonAdvolValue = 0;
            if (itemconfig.ewaypartb)
            {
                ewbGen.transporterId = bills.transport.gst;
                ewbGen.transporterName = bills.transport.name;
                ewbGen.totInvValue = 0;
                ewbGen.transMode = "1";//1
                ewbGen.transDistance = itemconfig.distance != 0 ? itemconfig.distance.ToString("F0") : "0"; /*1200*/
                ewbGen.vehicleNo = itemconfig.vehcicleno;//PVC1234
                ewbGen.vehicleType = "R";//R
                ewbGen.transDocNo = bill.returnno;
                ewbGen.transDocDate = bill.returndate.ToString("dd/MM/yyyy").Replace("-", "/");
            }
            else
            {
                ewbGen.transMode = itemconfig.modeofdelivery > 0 ? itemconfig.modeofdelivery.ToString("N0") : "1";//1
                ewbGen.transDistance = itemconfig.distance != 0 ? itemconfig.distance.ToString("F0") : "0"; /*1200*/
                ewbGen.transporterName = bills.transport.name;
                ewbGen.transporterId = bills.transport.gst;
                ewbGen.vehicleNo = itemconfig.vehcicleno;//PVC1234
                ewbGen.vehicleType = string.IsNullOrEmpty(itemconfig.vehcicleno) ? "O" : "R";//R
                ewbGen.totInvValue = 0;
                if (string.IsNullOrEmpty(itemconfig.vehcicleno))
                {
                    ewbGen.transMode = null;
                    ewbGen.vehicleType = null;
                    ewbGen.vehicleNo = null;
                    ewbGen.transDocDate = null;
                    ewbGen.transDocNo = null;
                }
            }
            ewbGen.itemList = new List<InfyGST.GSTPortal.EWAYBill.ItemList>();

            bill.Lrinvoicereturnitems.ForEach(x =>
            {
                if (productlist.Exists(y => y.id == x.productid))
                {
                    var prod = productlist.Find(y => y.id == x.productid);
                    x.sellingmode = prod.sellingmode;
                    x.productname = prod.name;
                }
            });
            foreach (var item in bill.Lrinvoicereturnitems.GroupBy(x => new { x.productname, x.printingname, x.hsncode, x.sellingmode, x.taxpercentage, unitprice = (double)Math.Round((x.amount) / x.qty, 2) }))
            {
                var tax = Math.Round(item.Sum(x => x.tax), 2);
                var taxable = Math.Round(item.Sum(x => x.amount-x.discount), 2);
                InfyGST.GSTPortal.EWAYBill.ItemList itm = new InfyGST.GSTPortal.EWAYBill.ItemList();
                itm.productDesc =item.Key.productname;
                itm.productName =item.Key.printingname;

                if (!int.TryParse(item.Key.hsncode, out tryno))
                    throw new Exception("Pleae enter the correct hsn code for "+ itm.productName);

                itm.hsnCode = int.Parse("0" + item.Key.hsncode);
                itm.quantity = (decimal)item.Sum(x => x.qty);
                itm.qtyUnit = item.Key.sellingmode ==3 ? "MTR" : "NOS";
                itm.taxableAmount = taxable;
                if (bill.isinterstatesale)
                {
                    itm.igstRate = Math.Round(Convert.ToDecimal(item.Key.taxpercentage), 2);
                }
                else if(item.Key.taxpercentage>0)
                {
                    itm.sgstRate = Math.Round(Convert.ToDecimal(item.Key.taxpercentage / 2), 2);
                    itm.cgstRate = Math.Round(Convert.ToDecimal(item.Key.taxpercentage / 2), 2);
                }
                ewbGen.itemList.Add(itm);
            }

            ewbGen.otherValue = 0;

            ewbGen.cgstValue = ewbGen.itemList.Sum(x => x.cgstRate * x.taxableAmount / 100);
            ewbGen.sgstValue = ewbGen.itemList.Sum(x => x.sgstRate * x.taxableAmount / 100);
            ewbGen.igstValue = ewbGen.itemList.Sum(x => x.igstRate * x.taxableAmount / 100);
            ewbGen.totalValue = ewbGen.itemList.Sum(x => x.taxableAmount);
            //ewbGen.totalValue += ewbGen.cgstValue + ewbGen.sgstValue + ewbGen.igstValue;
            ewbGen.cessValue = 0;

            var GSTPortal = GetService(bill.companyid);
            var response = GSTPortal.CreateEWay(new InfyGST.GSTPortal.EWayRequest()
            {
                Data = ewbGen,
                accesskey = gstaccesskey,
                ewbpwd = einvoicepwd,
                gstin = gstno,
                issandbox = issandbox,
                username = einvoiceusername
            }).Result;

            return response;
        }

        public class GSTR1Row
        {
            public string billingname { get; set; }
            public string billno { get; set; }
            public DateTime billdate { get; set; }
            public string poscode { get; set; }
            public string posname { get; set; }
            public decimal total { get; set; }
            public decimal tax { get; set; }
            public decimal taxpercentage { get; set; }

        }

        const string GSTR1_ProcesConflict = @"
        update bill set isb2b=false where id in (
        select bill.id from bill
        inner join customer on customer.id = bill.customerid
        where isb2b=true and (customer.gstno is null or customer.gstno = '')
        );

WITH CUSTOMERSTATECHANGES AS
(
SELECT CUSTOMER.ID,CUSTOMER.NAME,CUSTOMER.GSTNO,REFERENCELIST.CODE,ST.ID NEWSTID,ST.CODE,ST.NAME FROM CUSTOMER
LEFT OUTER JOIN REFERENCELIST ON REFERENCELIST.ID = CUSTOMER.STATEID
LEFT OUTER JOIN REFERENCELIST ST ON ST.CODE = LEFT(GSTNO,2) AND ST.TYPE ='STATE'
WHERE CUSTOMER.GSTNO IS NOT NULL AND CUSTOMER.GSTNO != '' AND LENGTH(CUSTOMER.GSTNO)>10
AND LEFT(GSTNO,2) != REFERENCELIST.CODE
)
UPDATE CUSTOMER SET STATEID=NEWSTID FROM CUSTOMERSTATECHANGES
WHERE CUSTOMERSTATECHANGES.ID = CUSTOMER.ID;

WITH CUSTOMERINTERSTATE
AS
(
SELECT BILL.ID,BILL.ISINTERSTATESALE,CUSTOMER.GSTNO,COMPANY.TINANDDATE FROM BILL
INNER JOIN COMPANY ON COMPANY.ID = BILL.COMPANYID
INNER JOIN CUSTOMER ON CUSTOMER.ID = BILL.CUSTOMERID
WHERE BILL.ISINTERSTATESALE=TRUE
AND CUSTOMER.GSTNO IS NOT NULL AND LENGTH(CUSTOMER.GSTNO)>10 AND
(LEFT(CUSTOMER.GSTNO,2) = LEFT(COMPANY.TINANDDATE,2))
)

UPDATE BILL SET ISINTERSTATESALE=FALSE FROM CUSTOMERINTERSTATE
WHERE CUSTOMERINTERSTATE.ID = BILL.ID;

WITH CUSTOMERINTERSTATE
AS
(
SELECT BILL.ID,BILL.ISINTERSTATESALE,CUSTOMER.GSTNO,COMPANY.TINANDDATE FROM BILL
INNER JOIN COMPANY ON COMPANY.ID = BILL.COMPANYID
INNER JOIN CUSTOMER ON CUSTOMER.ID = BILL.CUSTOMERID
WHERE BILL.ISINTERSTATESALE=FALSE 
AND CUSTOMER.GSTNO IS NOT NULL AND LENGTH(CUSTOMER.GSTNO)>10 AND
(LEFT(CUSTOMER.GSTNO,2) != LEFT(COMPANY.TINANDDATE,2))
)

UPDATE BILL SET ISINTERSTATESALE=TRUE FROM CUSTOMERINTERSTATE
WHERE CUSTOMERINTERSTATE.ID = BILL.ID;


SELECT BILL.ID,BILL.BILLNO,BILL.ISINTERSTATESALE,customer.id,
CUSTOMER.NAME,CUSTOMER.GSTNO,customer.address,COMPANY.TINANDDATE FROM BILL
INNER JOIN COMPANY ON COMPANY.ID = BILL.COMPANYID
INNER JOIN CUSTOMER ON CUSTOMER.ID = BILL.CUSTOMERID
INNER JOIN REFERENCELIST ON REFERENCELIST.ID = CUSTOMER.STATEID
WHERE BILL.ISINTERSTATESALE=TRUE
AND REFERENCELIST.CODE =  LEFT(COMPANY.TINANDDATE,2);

with newcustomerstate as
(
select bill.id,
(case when customer is not null and customer.stateid>0 and bill.isinterstatesale  then customer.stateid
		   else company.stateid end) customerstateid
from bill
left outer join customer on customerid = bill.customerid 
left outer join company on company.id = bill.companyid
where bill.customerstateid is null
)
update bill set customerstateid=newcustomerstate.customerstateid
from newcustomerstate 
where newcustomerstate.id = bill.id";

        const string GSTR1_B2B_SE_DE = @"
with exportbill as
(
    select distinct bill.id from bill 
    where billdate>=@fromdate and billdate<@todate
    and billtype != 9 and isactive=true
    and bill.isexport=true
),solddata as
(
	select bill.id billid,taxpercentage,sum(billitems.tax) tax,sum(billitems.receivable) receivable,bill.billdate,
    bill.billno from bill
	inner join billitems on billitems.billid = bill.id and billitems.isactive=true
    left outer join exportbill on exportbill.id = bill.id
    where billdate>=@fromdate and billdate<@todate and bill.isactive=true
    and bill.customerid>0 and billtype!=9 and billtype!=8 and bill.companyid=@companyid and exportbill.id is null
	group by bill.id,taxpercentage,bill.billdate,bill.billno
),adddata as
(
    select Billadditionalcharges.billid,
    sum(Billadditionalcharges.tax) tax,
    sum(Billadditionalcharges.charge) taxable,
    sum(Billadditionalcharges.charge + Billadditionalcharges.tax) total,
    Billadditionalcharges.taxpercentage
    from Billadditionalcharges
    inner join bill on bill.id = Billadditionalcharges.billid 
    inner join referencelist on referencelist.id = Billadditionalcharges.additionalchargestypeid
    where 
    billdate>=@fromdate and billdate<@todate 
    and cast(additionalinfo->>'paytype' as character varying(10)) != 'EXPORTGST'
    and Billadditionalcharges.isactive=true
    group by Billadditionalcharges.billid,
    Billadditionalcharges.taxpercentage
)

select 
case when customer.billingname != '' then customer.billingname else customer.name end customername,solddata.billno,solddata.billdate::date,
solddata.tax,solddata.receivable-solddata.tax taxable,solddata.receivable total,solddata.taxpercentage,customer.gstno,ewb eway,einvoice,
referencelist.code poscode,referencelist.name posname from solddata
inner join bill on bill.id = solddata.billid 
inner join customer on customer.id = bill.customerid
left outer join billinbox on billinbox.billid = bill.id and billinbox.isactive=true
left outer join referencelist on referencelist.id = customer.stateid
where bill.billdate>=@fromdate and bill.billdate<@todate 
and billtype!=9 and billtype!=8 
and customer.gstno is not null and customer.gstno != '' and bill.isb2b=true
union all 

select 
case when customer.billingname != '' then customer.billingname else customer.name end customername,solddata.billno,solddata.billdate::date,
adddata.tax,adddata.taxable,adddata.total,adddata.taxpercentage ,customer.gstno,ewb eway,einvoice,
referencelist.code poscode,referencelist.name posname from solddata
inner join adddata on adddata.billid = solddata.billid
inner join bill on bill.id = solddata.billid 
inner join customer on customer.id = bill.customerid
left outer join billinbox on billinbox.billid = bill.id and billinbox.isactive=true
left outer join referencelist on referencelist.id = customer.stateid
where billtype!=9 and billtype!=8 
and customer.gstno is not null and customer.gstno != '' and bill.isb2b=true";

        const string GSTR1_B2C_Large = @"with largebill as
(
    select bill.id,billdate,billno,customerid,bill.isinterstatesale,isb2b from bill where billtype!=9 and bill.isinterstatesale=true 
    and bill.companyid=@companyid and receivable>@largebillvalue and billdate>=@fromdate and billdate<@todate
),
exportbills as
(
    select distinct bill.id from bill 
    where billdate>=@fromdate and billdate<@todate
    and billtype != 9 and isactive=true
    and bill.isexport=true
),solddata as
(
	select billid,taxpercentage,sum(billitems.tax) tax,sum(billitems.receivable) receivable,largebill.billdate::date,
    largebill.billno,referencelist.code poscode,referencelist.name posname,
    case when customer.billingname != '' then customer.billingname else customer.name end customername from largebill
	inner join billitems on billitems.billid = largebill.id
    left outer join customer on customer.id = largebill.customerid
    left outer join referencelist on referencelist.id = customer.stateid
    left outer join exportbills on exportbills.id = largebill.id
    WHERE billitems.isactive=true and exportbills.id is null
    and (customer.id is null or customer.gstno is null or customer.gstno = '' or largebill.isb2b=false)
	group by billid,taxpercentage,largebill.billdate,largebill.billno,referencelist.code,referencelist.name,
    case when customer.billingname != '' then customer.billingname else customer.name end 
),adddata as
(
    select Billadditionalcharges.billid,
    sum(Billadditionalcharges.tax) tax,
    sum(Billadditionalcharges.charge) taxable,
    sum(Billadditionalcharges.charge + Billadditionalcharges.tax) total,
    Billadditionalcharges.taxpercentage
    from Billadditionalcharges
    inner join bill on bill.id = Billadditionalcharges.billid 
    inner join referencelist on referencelist.id = Billadditionalcharges.additionalchargestypeid
    where 
    billdate>=@fromdate and billdate<@todate 
    and cast(additionalinfo->>'paytype' as character varying(10)) != 'EXPORTGST'
    and Billadditionalcharges.isactive=true
    group by Billadditionalcharges.billid,
    Billadditionalcharges.taxpercentage
)
select solddata.customername,solddata.billno,solddata.billdate,
solddata.tax,solddata.receivable-solddata.tax taxable,solddata.receivable total,solddata.taxpercentage,
poscode,posname from bill
inner join solddata on solddata.billid = bill.id
where bill.billdate>=@fromdate and bill.billdate<@todate and billtype!=9  and bill.isactive=true

union all 

select solddata.customername,solddata.billno,solddata.billdate,
adddata.tax,adddata.taxable,adddata.total,adddata.taxpercentage,
poscode,posname from adddata
inner join bill on bill.id =adddata.billid
inner join solddata on solddata.billid = bill.id
where bill.billdate>=@fromdate and bill.billdate<@todate and billtype!=9  and bill.isactive=true";


        //int
        const string GSTR1_B2C_Small = @"with largebill as
(
    select bill.id from bill 
    inner join referencelist on referencelist.id = bill.counterid
    left outer join customer on customer.id = customerid and customer.isactive=true and customer.gstno is not null and customer.gstno != ''
    where billtype!=9 and bill.companyid=@companyid and bill.billdate>=@fromdate and bill.billdate<@todate
    and (customer.id is null or customer.gstno is null or customer.gstno = '' or bill.isb2b=false)
    and (bill.isinterstatesale=false or receivable<=@largebillvalue) and bill.isactive=true
),exportbills as
(
    select distinct bill.id from bill 
    where billdate>=@fromdate and billdate<@todate
    and billtype != 9 and isactive=true
    and bill.isexport=true
),solddata as
(
    select taxpercentage,sum(billitems.tax) tax,sum(billitems.receivable) receivable,
	referencelist.code poscode,referencelist.name posname,bill.isinterstatesale,
    ADDITIONALINFO->>'formula' ecommercegstno
	from largebill
	inner join bill on bill.id = largebill.id
	inner join billitems on billitems.billid =largebill.id and billitems.isactive=true
    left outer join exportbills on exportbills.id = largebill.id
    left outer join customer on customer.id = bill.customerid
    left outer join referencelist on referencelist.id = customer.stateid
    WHERE exportbills.id is null
	group by taxpercentage,	referencelist.code,referencelist.name,
	ADDITIONALINFO->>'formula',bill.isinterstatesale
),adddata as
(
   select 
stateinfo.code poscode,stateinfo.name posname,bill.isinterstatesale,
referencelist.ADDITIONALINFO->>'formula' ecommercegstno,
sum(Billadditionalcharges.tax) tax,
sum(Billadditionalcharges.charge) taxable,
sum(Billadditionalcharges.charge + Billadditionalcharges.tax) total,
Billadditionalcharges.taxpercentage
from Billadditionalcharges
inner join bill on bill.id = Billadditionalcharges.billid 
inner join largebill on largebill.id = Billadditionalcharges.billid 
left outer join exportbills on exportbills.id = largebill.id
inner join referencelist on referencelist.id = Billadditionalcharges.additionalchargestypeid
    left outer join customer on customer.id = bill.customerid
    left outer join referencelist stateinfo on stateinfo.id = customer.stateid
	
where 
bill.billdate>=@fromdate and bill.billdate<@todate
and cast(referencelist.additionalinfo->>'paytype' as character varying(10)) != 'EXPORTGST'
and Billadditionalcharges.isactive=true and exportbills.id is null
group by 
Billadditionalcharges.taxpercentage,
	poscode,posname,bill.isinterstatesale,ecommercegstno


)

select solddata.taxpercentage,coalesce(ecommercegstno,''),
case when poscode is null or solddata.isinterstatesale=false then @defaultstatecode else poscode end poscode,
case when posname is null or solddata.isinterstatesale=false then @defaultstatename else posname end posname,
sum(solddata.tax) tax,sum(solddata.receivable-solddata.tax) taxable,sum(solddata.receivable) total
from solddata
group by solddata.taxpercentage,coalesce(ecommercegstno,''),
case when poscode is null or solddata.isinterstatesale=false then @defaultstatecode else poscode end ,
case when posname is null or solddata.isinterstatesale=false then @defaultstatename else posname end 

union all

select adddata.taxpercentage,coalesce(ecommercegstno,''),
case when poscode is null or adddata.isinterstatesale=false then @defaultstatecode else poscode end poscode,
case when posname is null or adddata.isinterstatesale=false then @defaultstatename else posname end posname,
sum(adddata.tax) tax,sum(adddata.taxable) taxable,sum(adddata.total) total
from adddata
group by adddata.taxpercentage,coalesce(ecommercegstno,''),
case when poscode is null or adddata.isinterstatesale=false then @defaultstatecode else poscode end ,
case when posname is null or adddata.isinterstatesale=false then @defaultstatename else posname end ";

        const string GSTR1_B2C_Export = @"with largebill as
(
    select distinct bill.id,billdate,billno,customerid from bill 
    where billtype!=9 and bill.companyid=@companyid  and billdate>=@fromdate and billdate<@todate and bill.isactive=true
    and bill.isexport=true

),solddata as
(
    select sum(billitems.tax) tax,sum(billitems.receivable) receivable,
    taxpercentage,case when customer.billingname != '' then customer.billingname else customer.name end customername,billdate,billno from largebill
	inner join billitems on  billitems.billid = largebill.id 
    left outer join customer on customer.id = customerid
    left outer join referencelist on referencelist.id = customer.stateid
    WHERE billitems.isactive=true
	group by  taxpercentage,case when customer.billingname != '' then customer.billingname else customer.name end,billdate,billno
),adddata as
(
    select sum(Billadditionalcharges.tax) tax,sum(Billadditionalcharges.charge + Billadditionalcharges.tax) receivable,
    Billadditionalcharges.taxpercentage,
    case when customer.billingname != '' then customer.billingname else customer.name end customername,billdate,billno from largebill
    inner join Billadditionalcharges on Billadditionalcharges.billid = largebill.id
    left outer join customer on customer.id = customerid
    left outer join referencelist on referencelist.id = customer.stateid
    WHERE Billadditionalcharges.isactive=true
	group by Billadditionalcharges.taxpercentage,case when customer.billingname != '' then customer.billingname else customer.name end,billdate,billno
)

select solddata.taxpercentage,customername,billdate,billno,
sum(solddata.tax) tax,sum(solddata.receivable-solddata.tax) taxable,sum(solddata.receivable) total
from solddata
group by solddata.taxpercentage,customername,billdate,billno

union all

select adddata.taxpercentage,customername,billdate,billno,
sum(adddata.tax) tax,sum(adddata.receivable-adddata.tax) taxable,sum(adddata.receivable) total
from adddata
group by adddata.taxpercentage,customername,billdate,billno";

        const string GSTR1_B2B_CN = @"with largebill as
(
    select bill.id,billdate,billno,customerid,bill.isinterstatesale,bill.counterid,ADDITIONALINFO->>'isonline' isecommerce,ADDITIONALINFO->>'formula' ecommercegstno from bill 
    inner join referencelist on referencelist.id = bill.counterid
    inner join customer on customer.id = bill.customerid
    where billtype=8 and bill.isactive=true and bill.customerid>0 AND bill.companyid=@companyid  and billdate>=@fromdate and billdate<@todate 
    AND customer.gstno is not null and customer.gstno != '' and bill.isb2b=true
),solddata as
(
	select billid,taxpercentage,sum(billitems.tax) tax,sum(billitems.receivable) receivable,largebill.billdate,
    largebill.billno,
    referencelist.code poscode,
    referencelist.name posname,
    ecommercegstno,isecommerce,customer.gstno,
    case when customer.billingname != '' then customer.billingname else customer.name end customername from billitems
	inner join largebill on largebill.id = billitems.billid 
    left outer join customer on customer.id = customerid
    left outer join referencelist on referencelist.id = customer.stateid
    where billitems.isactive=true
    group by billid,taxpercentage,largebill.billdate,largebill.billno,referencelist.code,ecommercegstno,isecommerce,referencelist.name,
    case when customer.billingname != '' then customer.billingname else customer.name end,customer.gstno
)
select customername,solddata.billno,solddata.billdate,
solddata.tax,solddata.receivable-solddata.tax taxable,solddata.receivable total,solddata.taxpercentage,
poscode,posname,gstno from bill
inner join solddata on solddata.billid = bill.id
where bill.isactive=true";

        const string GSTR1_DR = @"SELECT SUPPLIER.NAME,
SUPPLIERCOMPANY.TINANDDATE GSTNO,RETURNDATE,LRINVOICERETURN.RETURNNO INVOICENO,
CASE WHEN LRINVOICERETURN.returntype = 0 THEN 'D' ELSE 'C' END cdnrtype,REFERENCELIST.CODE POSCODE,REFERENCELIST.NAME POSNAME,
LRINVOICERETURNITEMS.TAXPERCENTAGE,
sum(LRINVOICERETURNITEMS.QTY) qty,
sum(LRINVOICERETURNITEMS.AMOUNT-LRINVOICERETURNITEMS.TAX) taxable,
sum(case when LRINVOICERETURN.isinterstatesale=true and LRINVOICERETURNITEMS.TAX>0 then LRINVOICERETURNITEMS.TAX else 0 end) igst,
sum(case when LRINVOICERETURN.isinterstatesale=false and LRINVOICERETURNITEMS.TAX>0 then LRINVOICERETURNITEMS.TAX/2 else 0 end) cgst,
sum(case when LRINVOICERETURN.isinterstatesale=false and LRINVOICERETURNITEMS.TAX>0 then LRINVOICERETURNITEMS.TAX/2 else 0 end) sgst,

sum(LRINVOICERETURNITEMS.AMOUNT) total
FROM LRINVOICERETURN 
INNER JOIN SUPPLIER ON SUPPLIER.ID = LRINVOICERETURN.SUPPLIERID
INNER JOIN SUPPLIERCOMPANY ON SUPPLIERCOMPANY.SUPPLIERID = LRINVOICERETURN.SUPPLIERID
INNER JOIN REFERENCELIST ON REFERENCELIST.ID = SUPPLIERCOMPANY.STATEID
INNER JOIN LRINVOICERETURNITEMS ON LRINVOICERETURNITEMS.LRINVOICERETURNID = LRINVOICERETURN.ID 
where RETURNDATE>=@fromdate and RETURNDATE<@todate and LRINVOICERETURN.isactive=true AND LRINVOICERETURNITEMS.ISACTIVE=TRUE 
and  LRINVOICERETURN.companyid=@companyid and (LRINVOICERETURN.returntype IN (0,5))
group by SUPPLIER.NAME,
SUPPLIERCOMPANY.TINANDDATE ,RETURNDATE,REFERENCELIST.CODE ,REFERENCELIST.NAME ,
LRINVOICERETURNITEMS.TAXPERCENTAGE,LRINVOICERETURN.RETURNNO,LRINVOICERETURN.returntype";

        const string GSTR1_HSN = @"select billitems.taxpercentage,
case when products.sellingmode = 3 then 'MTR' else 'NOS' end as UQC,
case when stock.hsncode is null or stock.hsncode = '' then products.hsncode else stock.hsncode end as hsncode,
sum(billitems.qty) qty,sum(billitems.receivable-billitems.tax) taxable,
sum(case when bill.isinterstatesale=true and billitems.tax!=0 then billitems.tax else 0 end) igst,
sum(case when bill.isinterstatesale=false and billitems.tax!=0 then billitems.tax/2 else 0 end) cgst,
sum(case when bill.isinterstatesale=false and billitems.tax!=0 then billitems.tax/2 else 0 end) sgst,
sum(billitems.tax) tax,
sum(billitems.receivable) total from bill
inner join billitems on billitems.billid = bill.id and billitems.isactive=true
left outer join stock on stock.id = billitems.stockid
left outer join products on products.id = stock.productid
--left outer join customer on customer.id = bill.customerid
--left outer join referencelist states on states.id = customer.stateid
where bill.companyid = @companyid and billdate>=@fromdate and billdate<@todate 
and bill.isactive=true and bill.billtype != 9
group by billitems.taxpercentage,
case when products.sellingmode = 3 then 'MTR' else 'NOS' end,
case when stock.hsncode is null or stock.hsncode = '' then products.hsncode else stock.hsncode end
UNION ALL
Select Billadditionalcharges.taxpercentage,'NOS' uqc,
cast(additionalinfo->>'formula' as character varying(10)) as hsncode,1 qty,
sum(Billadditionalcharges.charge) taxable, 
sum(case when bill.isinterstatesale=true and  Billadditionalcharges.tax!=0 then Billadditionalcharges.tax else 0 end) igst,
sum(case when bill.isinterstatesale=false and Billadditionalcharges.tax!=0 then Billadditionalcharges.tax/2 else 0 end) cgst,
sum(case when bill.isinterstatesale=false and Billadditionalcharges.tax!=0 then Billadditionalcharges.tax/2 else 0 end) sgst,
sum(Billadditionalcharges.tax) tax,
sum(Billadditionalcharges.charge + Billadditionalcharges.tax) total
                        from Billadditionalcharges
						inner join bill on bill.id = Billadditionalcharges.billid 
                        inner join referencelist on referencelist.id = Billadditionalcharges.additionalchargestypeid
                        where bill.companyid = @companyid and billdate>=@fromdate and billdate<@todate 
                        and bill.isactive=true and bill.billtype != 9 and Billadditionalcharges.isactive=true
                        and cast(additionalinfo->>'paytype' as character varying(10)) != 'EXPORTGST'
                        --and Billadditionalcharges.taxpercentage>0 and Billadditionalcharges.tax != 0
                        group by cast(additionalinfo->>'formula' as character varying(10)), Billadditionalcharges.taxpercentage
						order by hsncode";

        const string GSTR1_B2BHSN = @"select billitems.taxpercentage,
case when products.sellingmode = 3 then 'MTR' else 'NOS' end as UQC,
case when stock.hsncode is null or stock.hsncode = '' then products.hsncode else stock.hsncode end as hsncode,
products.name productname,
sum(billitems.qty) qty,sum(billitems.receivable-billitems.tax) taxable,
sum(case when bill.isinterstatesale=true and billitems.tax!=0 then billitems.tax else 0 end) igst,
sum(case when bill.isinterstatesale=false and billitems.tax!=0 then billitems.tax/2 else 0 end) cgst,
sum(case when bill.isinterstatesale=false and billitems.tax!=0 then billitems.tax/2 else 0 end) sgst,
sum(billitems.tax) tax,
sum(billitems.receivable) total from bill
inner join billitems on billitems.billid = bill.id and billitems.isactive=true
left outer join stock on stock.id = billitems.stockid
left outer join products on products.id = stock.productid
inner join customer on customer.id = bill.customerid
where bill.companyid = @companyid and billdate>=@fromdate and billdate<@todate 
and bill.isactive=true and bill.billtype != 9 and isb2b=true 
and customer.gstno is not null and customer.gstno != ''
group by billitems.taxpercentage,
case when products.sellingmode = 3 then 'MTR' else 'NOS' end,products.name,
case when stock.hsncode is null or stock.hsncode = '' then products.hsncode else stock.hsncode end
UNION ALL
Select Billadditionalcharges.taxpercentage,'NOS' uqc,
cast(additionalinfo->>'formula' as character varying(10)) as hsncode,referencelist.name productname,1 qty,
sum(Billadditionalcharges.charge) taxable, 
sum(case when bill.isinterstatesale=true and  Billadditionalcharges.tax!=0 then Billadditionalcharges.tax else 0 end) igst,
sum(case when bill.isinterstatesale=false and Billadditionalcharges.tax!=0 then Billadditionalcharges.tax/2 else 0 end) cgst,
sum(case when bill.isinterstatesale=false and Billadditionalcharges.tax!=0 then Billadditionalcharges.tax/2 else 0 end) sgst,
sum(Billadditionalcharges.tax) tax,
sum(Billadditionalcharges.charge + Billadditionalcharges.tax) total
                        from Billadditionalcharges
						inner join bill on bill.id = Billadditionalcharges.billid 
                        inner join referencelist on referencelist.id = Billadditionalcharges.additionalchargestypeid
                        inner join customer on customer.id = bill.customerid
                        where bill.companyid = @companyid and billdate>=@fromdate and billdate<@todate 
                        and bill.isactive=true and bill.billtype != 9 and Billadditionalcharges.isactive=true
                        and cast(additionalinfo->>'paytype' as character varying(10)) != 'EXPORTGST'
                        and isb2b=true and customer.gstno is not null and customer.gstno != ''
                        --and Billadditionalcharges.taxpercentage>0 and Billadditionalcharges.tax != 0
                        group by cast(additionalinfo->>'formula' as character varying(10)), referencelist.name,Billadditionalcharges.taxpercentage
						order by hsncode";

        const string GSTR1_NONB2BHSN = @"select billitems.taxpercentage,
case when products.sellingmode = 3 then 'MTR' else 'NOS' end as UQC,
case when stock.hsncode is null or stock.hsncode = '' then products.hsncode else stock.hsncode end as hsncode,
products.name productname,
sum(billitems.qty) qty,sum(billitems.receivable-billitems.tax) taxable,
sum(case when bill.isinterstatesale=true and billitems.tax!=0 then billitems.tax else 0 end) igst,
sum(case when bill.isinterstatesale=false and billitems.tax!=0 then billitems.tax/2 else 0 end) cgst,
sum(case when bill.isinterstatesale=false and billitems.tax!=0 then billitems.tax/2 else 0 end) sgst,
sum(billitems.tax) tax,
sum(billitems.receivable) total from bill
inner join billitems on billitems.billid = bill.id and billitems.isactive=true
left outer join stock on stock.id = billitems.stockid
left outer join products on products.id = stock.productid
left outer join customer on customer.id = bill.customerid
--left outer join referencelist states on states.id = customer.stateid
where bill.companyid = @companyid and billdate>=@fromdate and billdate<@todate 
and bill.isactive=true and bill.billtype != 9 and (isb2b=false or coalesce(customer.gstno,'') = '')
group by billitems.taxpercentage,products.name,
case when products.sellingmode = 3 then 'MTR' else 'NOS' end,
case when stock.hsncode is null or stock.hsncode = '' then products.hsncode else stock.hsncode end
UNION ALL
Select Billadditionalcharges.taxpercentage,'NOS' uqc,
cast(additionalinfo->>'formula' as character varying(10)) as hsncode,referencelist.name productname,1 qty,
sum(Billadditionalcharges.charge) taxable, 
sum(case when bill.isinterstatesale=true and  Billadditionalcharges.tax!=0 then Billadditionalcharges.tax else 0 end) igst,
sum(case when bill.isinterstatesale=false and Billadditionalcharges.tax!=0 then Billadditionalcharges.tax/2 else 0 end) cgst,
sum(case when bill.isinterstatesale=false and Billadditionalcharges.tax!=0 then Billadditionalcharges.tax/2 else 0 end) sgst,
sum(Billadditionalcharges.tax) tax,
sum(Billadditionalcharges.charge + Billadditionalcharges.tax) total
                        from Billadditionalcharges
						inner join bill on bill.id = Billadditionalcharges.billid 
                        inner join referencelist on referencelist.id = Billadditionalcharges.additionalchargestypeid
                        left outer join customer on customer.id = bill.customerid
                        where bill.companyid = @companyid and billdate>=@fromdate and billdate<@todate 
                        and bill.isactive=true and bill.billtype != 9 and Billadditionalcharges.isactive=true
                        and cast(additionalinfo->>'paytype' as character varying(10)) != 'EXPORTGST'
                        and (isb2b=false or coalesce(customer.gstno,'') = '')
                        --and Billadditionalcharges.taxpercentage>0 and Billadditionalcharges.tax != 0
                        group by cast(additionalinfo->>'formula' as character varying(10)),referencelist.name, Billadditionalcharges.taxpercentage
						order by hsncode";

        const string GSTR1_Doc = @"select billno,id,billtype,isactive from bill where billdate>=@fromdate
and billdate<@todate order by bill.id";

        //        const string ALL_GSTR1 = @"
        //with exportbill as
        //(
        //    select distinct bill.id from bill 
        //    left outer join billadditionalcharges on billadditionalcharges.billid = bill.id and chargetype ='EXPORTGST' AND billadditionalcharges.isactive=true
        //    where billtype!=9 and bill.companyid=@companyid and bill.billdate>=@fromdate and bill.billdate<@todate
        //    and (billadditionalcharges.id is not null or bill.isexport=true)
        //),solddata as
        //(
        //	select billid,billitems.hsncode,taxpercentage,sum(billitems.tax) tax,sum(billitems.receivable) receivable,bill.billdate,
        //    bill.billno from billitems
        //	inner join bill on bill.id = billitems.billid 
        //    left outer join exportbill on exportbill.id = billitems.billid
        //    where billdate>='2024-09-01' and billdate<'2024-10-01' and billitems.isactive=true and bill.isactive=true
        //    and bill.customerid>0 and billtype!=9 and billtype!=8 and bill.companyid=2 and exportbill.id is null
        //	group by billid,taxpercentage,bill.billdate,bill.billno,billitems.hsncode
        //),GSTR1_B2B_SE_DE as (
        //select 
        //sum(solddata.tax) tax,sum(solddata.receivable-solddata.tax) taxable,
        //sum(solddata.receivable) total,
        //hsncode,taxpercentage from bill
        //inner join solddata on solddata.billid = bill.id
        //inner join customer on customer.id = bill.customerid
        //left outer join billinbox on billinbox.billid = bill.id and billinbox.isactive=true
        //left outer join referencelist on referencelist.id = customer.stateid
        //where bill.billdate>='2024-09-01' and bill.billdate<'2024-10-01' and billtype!=9 and billtype!=8 
        //	AND customer.gstno is not null and customer.gstno != ''
        //group by hsncode,taxpercentage), largebill_2 as
        //(
        //    select bill.id,billdate,billno,customerid,bill.isinterstatesale,counterid,ADDITIONALINFO->>'isonline' isecommerce,
        //    ADDITIONALINFO->>'formula' ecommercegstno from bill 
        //    inner join referencelist on referencelist.id = bill.counterid
        //    left outer join customer on customer.id = customerid
        //    where billtype!=9 and bill.companyid=2  and billdate>='2024-09-01' and billdate<'2024-10-01'
        //    and (customer.id is null or customer.gstno is null or customer.gstno = '')
        //    and (bill.isinterstatesale=false or receivable<=250000) and bill.isactive=true
        //),exportbills_2 as
        //(
        //    select distinct bill.id from bill 
        //    left outer join billadditionalcharges on billadditionalcharges.billid = bill.id and chargetype ='EXPORTGST' AND billadditionalcharges.isactive=true
        //    where billtype!=9 and bill.companyid=2  and billdate>='2024-09-01' and billdate<'2024-10-01'
        //    and (billadditionalcharges.id is not null or bill.isexport=true)
        //),solddata_2 as
        //(
        //    select taxpercentage,hsncode,sum(billitems.tax) tax,sum(billitems.receivable) receivable,
        //	referencelist.code poscode,referencelist.name posname,ecommercegstno,largebill.isinterstatesale from billitems
        //	inner join largebill_2 largebill on largebill.id = billitems.billid 
        //    left outer join exportbills_2 exportbills on exportbills.id = largebill.id
        //    left outer join customer on customer.id = largebill.customerid
        //    left outer join referencelist on referencelist.id = customer.stateid
        //    WHERE billitems.isactive=true and exportbills.id is null
        //	group by taxpercentage,hsncode,	referencelist.code,referencelist.name,ecommercegstno,largebill.isinterstatesale
        //),GSTR1_B2C_Small as 
        //(
        //select hsncode,solddata.taxpercentage,
        //sum(solddata.tax) tax,sum(solddata.receivable-solddata.tax) taxable,sum(solddata.receivable) total
        //from solddata_2 solddata
        //group by hsncode,taxpercentage
        //),largebill_3 as
        //(
        //    select bill.id,billdate,billno,customerid,bill.isinterstatesale,bill.counterid,ADDITIONALINFO->>'isonline' isecommerce,ADDITIONALINFO->>'formula' ecommercegstno from bill 
        //    inner join referencelist on referencelist.id = bill.counterid
        //    inner join customer on customer.id = bill.customerid
        //    where billtype=8 and bill.isactive=true and bill.customerid>0 AND bill.companyid=2  and billdate>='2024-09-01' and billdate<'2024-10-01' 
        //    AND customer.gstno is not null and customer.gstno != ''
        //),solddata_3 as
        //(
        //	select billid,taxpercentage,hsncode,sum(billitems.tax) tax,sum(billitems.receivable) receivable,largebill.billdate,
        //    largebill.billno,
        //    referencelist.code poscode,
        //    referencelist.name posname,
        //    ecommercegstno,isecommerce,customer.gstno,
        //    case when customer.billingname != '' then customer.billingname else customer.name end customername from billitems
        //	inner join largebill_3 largebill on largebill.id = billitems.billid 
        //    left outer join customer on customer.id = customerid
        //    left outer join referencelist on referencelist.id = customer.stateid
        //    where billitems.isactive=true
        //    group by billid,taxpercentage,hsncode,largebill.billdate,largebill.billno,referencelist.code,ecommercegstno,isecommerce,referencelist.name,
        //    case when customer.billingname != '' then customer.billingname else customer.name end,customer.gstno
        //),GSTR1_B2B_CN as (
        //select hsncode,
        //sum(solddata.tax) tax,sum(solddata.receivable-solddata.tax) taxable,
        //sum(solddata.receivable) total,
        //solddata.taxpercentage from bill
        //inner join solddata_3 solddata on solddata.billid = bill.id
        //where bill.isactive=true
        //group by hsncode,solddata.taxpercentage),
        //largebill_4 as
        //(
        //    select distinct bill.id,billdate,billno,customerid from bill 
        //    left outer join billadditionalcharges on billadditionalcharges.billid = bill.id and chargetype ='EXPORTGST' AND billadditionalcharges.isactive=true
        //    where billtype!=9 and bill.companyid=2  and billdate>='2024-09-01' and billdate<'2024-10-01' and bill.isactive=true
        //    and (billadditionalcharges.id is not null or bill.isexport=true)
        //),solddata_4 as
        //(
        //    select sum(billitems.tax) tax,sum(billitems.receivable) receivable,hsncode,
        //    taxpercentage,case when customer.billingname != '' then customer.billingname else customer.name end customername,billdate,billno from billitems
        //	inner join largebill_4 largebill on largebill.id = billitems.billid 
        //    left outer join customer on customer.id = customerid
        //    left outer join referencelist on referencelist.id = customer.stateid
        //    WHERE (customer.id is null or customer.gstno is null or customer.gstno = '') and billitems.isactive=true
        //	group by  taxpercentage,hsncode,case when customer.billingname != '' then customer.billingname else customer.name end,billdate,billno
        //),GSTR1_B2C_Export as
        //(
        //select solddata.taxpercentage,hsncode,
        //sum(solddata.tax) tax,sum(solddata.receivable-solddata.tax) taxable,sum(solddata.receivable) total
        //from solddata_4 solddata
        //group by solddata.taxpercentage,hsncode
        //),alltable as
        //(
        //select 'GSTR1_B2C_Export' typ,hsncode,taxpercentage,tax,taxable,total from GSTR1_B2C_Export
        //union all
        //select 'GSTR1_B2B_CN' typ,hsncode,taxpercentage,tax,taxable,total from GSTR1_B2B_CN
        //union all
        //select 'GSTR1_B2C_Small' typ,hsncode,taxpercentage,tax,taxable,total from GSTR1_B2C_Small
        //union all	
        //select 'GSTR1_B2B_SE_DE' typ,hsncode,taxpercentage,tax,taxable,total  from GSTR1_B2B_SE_DE
        //)

        //select * from alltable where taxpercentage=3

        //select hsncode,taxpercentage,sum(tax) tax,sum(taxable) taxable,sum(total) total from alltable
        //group by hsncode,taxpercentage
        //order by hsncode,taxpercentage";


        public class DocumentNos
        {
            public string billno { get; set; }
            public string prefix { get; set; }
            public long id { get; set; }
            public int billtype { get; set; }
            public long billnumber { get; set; }
            public bool isactive { get; set; }
            public bool iscancelled { get; set; }
            public bool iscreditnote { get; set; }
        }

        public DataTable BuildGSTR1Documents(DataSet dataset)
        {
            List<DocumentNos> documentNos = new List<DocumentNos>();
            foreach (DataRow dr in dataset.Tables[0].Rows)
            {
                DocumentNos dn = new DocumentNos();
                dn.billno = Convert.ToString(dr["billno"]);
                dn.id = Convert.ToInt64(dr["id"]);
                dn.billtype = Convert.ToInt32(dr["billtype"]);
                dn.isactive = Convert.ToBoolean(dr["isactive"]);
                dn.iscancelled = dn.billtype == 9 || !dn.isactive;
                dn.iscreditnote = dn.billtype == 8 || !dn.isactive;
                var billparts = dn.billno.Split(new char[] { '-', '/' });
                var anbillno = billparts[billparts.Length - 1];
                long billno = 0;
                if (long.TryParse(anbillno, out billno))
                {
                    dn.billnumber = billno;
                    dn.prefix = dn.billno.Substring(0, dn.billno.LastIndexOf(anbillno));
                }
                documentNos.Add(dn);
            }

            DataTable dt = new DataTable();
            dt.Columns.AddRange(new DataColumn[1] { new DataColumn("Nature of Document", typeof(string)) });
            dt.Columns.AddRange(new DataColumn[1] { new DataColumn("Sr. No. From", typeof(string)) });
            dt.Columns.AddRange(new DataColumn[1] { new DataColumn("Sr. No. To", typeof(string)) });
            dt.Columns.AddRange(new DataColumn[1] { new DataColumn("Total Number", typeof(Int32)) });
            dt.Columns.AddRange(new DataColumn[1] { new DataColumn("Cancelled", typeof(Int32)) });

            var b2b = documentNos.FindAll(x => !x.iscreditnote);
            var cn = documentNos.FindAll(x => x.iscreditnote);
            foreach (var item in b2b.GroupBy(x => x.prefix))
            {
                var rows = item.ToList().OrderBy(x => x.billnumber).ToList();
                dt.Rows.Add(new object[] { "Invoices for outward supply", rows.First().billno, rows.Last().billno, rows.Count, rows.Count(x => x.iscancelled) });
            }
            foreach (var item in cn.GroupBy(x => x.prefix))
            {
                var rows = item.ToList().OrderBy(x => x.billnumber).ToList();
                dt.Rows.Add(new object[] { "Credit Note", rows.First().billno, rows.Last().billno, rows.Count, rows.Count(x => x.iscancelled) });
            }
            return dt;
        }

        public string ApplyB2BCondition(string Query)
        {
            if (!ConfigManager.Instance(Context.OrganizationID).Config.system_disableposgstbill)
            {
                Query = Query.Replace("and bill.isb2b=true", "").Replace("and bill.isb2b=false", "");
                Query = Query.Replace("or bill.isb2b=true", "").Replace("or bill.isb2b=false", "");
            }
            else
            {
                //Query = Query.Replace("AND customer.gstno is not null and customer.gstno != '' and bill.isb2b=true", "and bill.isb2b=true");
                //Query = Query.Replace("and (customer.id is null) and bill.isb2b=false", "and bill.isb2b=false");
                //Query = Query.Replace("and coalesce(customer.gstno,'') = '' and bill.isb2b=false", "and bill.isb2b=false");
                //Query = Query.Replace("and (customer.id is null or customer.gstno is null or customer.gstno = '') and bill.isb2b=false", "and bill.isb2b=false");
                //Query = Query.Replace("and (customer.id is null or customer.gstno is null or customer.gstno = '') and largebill.isb2b=false", "and largebill.isb2b=false");
            }
            return Query;
        }
        public byte[] BuildGSTR1DataSet(Gst_Inbox criteria)
        {
            /*
             
             select 'insterstate bill but customer state is wrong' reason,billno,customer.name,customer.id,customer.mobileno,customer.address,customer.stateid,company.stateid,
             customer.gstno,company.tinanddate,bill.id billid,bill.isinterstatesale from bill 
             inner join company on company.id = companyid
             left outer join customer on customer.id = customerid
             where billdate>='2024-08-01' and billdate<'2024-09-01'
             and bill.isinterstatesale = true
             and (customer.id is null or customer.stateid = company.stateid)

             union all
             select 'non interstate but other state gst no' reason ,billno,customer.name,customer.id,customer.mobileno,customer.address,customer.stateid,company.stateid,
			 customer.gstno,company.tinanddate,bill.id billid,bill.isinterstatesale from bill 
             inner join company on company.id = companyid
             inner join customer on customer.id = customerid
             where billdate>='2024-08-01' and billdate<'2024-09-01'
             and bill.isinterstatesale = false
             and customer.gstno != ''
             and left(customer.gstno,2) != left(company.tinanddate,2)

             union all
             select 'interstate but same state gst no' reason ,billno,customer.name,customer.id,customer.mobileno,customer.address,customer.stateid,company.stateid,
			 customer.gstno,company.tinanddate,bill.id billid,bill.isinterstatesale from bill 
             inner join company on company.id = companyid
             inner join customer on customer.id = customerid
             where billdate>='2024-08-01' and billdate<'2024-09-01'
             and bill.isinterstatesale = true
             and  customer.gstno != ''
             and left(customer.gstno,2) = left(company.tinanddate,2)
		     union all
             select 'interstate but customer dont have gst no' reason ,billno,customer.name,customer.id,customer.mobileno,customer.address,customer.stateid,company.stateid,
			 customer.gstno,company.tinanddate,bill.id billid,bill.isinterstatesale from bill 
             inner join company on company.id = companyid
             inner join customer on customer.id = customerid
             where billdate>='2024-08-01' and billdate<'2024-09-01'
             and bill.isinterstatesale = true
             and customer.gstno = ''
			 and bill.billtype != 9
			 and bill.billtype != 1
             union all 
             select 'interstate but same state' reason ,billno,customer.name,customer.id,customer.mobileno,customer.address,customer.stateid,company.stateid,
			 customer.gstno,company.tinanddate,bill.id billid,bill.isinterstatesale from bill 
             inner join company on company.id = companyid
             inner join customer on customer.id = customerid
             where billdate>='2024-08-01' and billdate<'2024-09-01'
             and bill.isinterstatesale = true
             and customer.stateid  = company.stateid 

             union all 
             select 'non interstate but other state customer' reason ,billno,customer.name,customer.id,
			 customer.mobileno,customer.address,customer.stateid,company.stateid,
			 customer.gstno,company.tinanddate,bill.id billid,bill.isinterstatesale from bill 
             inner join company on company.id = companyid
             inner join customer on customer.id = customerid
             where billdate>='2024-08-01' and billdate<'2024-09-01'
             and bill.isinterstatesale = false
             and  customer.stateid >0 and customer.stateid  != company.stateid
			 and customer.gstno != ''
			 */

            CompanyDataService companyDataService = new CompanyDataService();
            var company = companyDataService.Search(new CompanyCreteria()
            {
                id = criteria.companyid
            }).First();

            var defaultstatecode = "";
            var defaultstatename = "";
            if (!string.IsNullOrEmpty(company.gstno))
            {
                defaultstatecode = company.gstno.Substring(0, 2);
            }
            ReferencelistDataService referencelistDataService = new ReferencelistDataService();
            if (company.stateid > 0)
            {
                var states = referencelistDataService.SearchList(new ReferencelistCreteria()
                {
                    id = company.stateid
                }).First();
                defaultstatename = states.name;
            }

            var param = new SortedList<string, object>();
            param.Add("companyid", criteria.companyid);
            param.Add("fromdate", criteria.fromdate);
            param.Add("defaultstatecode", defaultstatecode);
            param.Add("defaultstatename", defaultstatename);
            param.Add("largebillvalue", 250000);
            param.Add("todate", criteria.todate);

            /*update bill set isexport=true where id in (
            select billid from billadditionalcharges
            inner join bill on bill.id = billid
            where chargetype ='EXPORTGST' AND billadditionalcharges.isactive=true 
            and bill.isexport=false)*/
            /*update billitems set tax=0,taxpercentage=0 where id in (
             select billitems.id from bill
             inner join billitems on billitems.billid = bill.id and billitems.tax!=0 and billitems.isactive=true
             where isexport=true);
             update bill set tax=0  where isexport=true and tax!=0;*/


            DataSet dataSet = new DataSet();
            var dataGSTR1_B2B_SE_DE = ExecuteDataSet(ApplyB2BCondition(GSTR1_B2B_SE_DE), param);
            var dataGSTR1_B2C_Large = ExecuteDataSet(ApplyB2BCondition(GSTR1_B2C_Large), param);
            var dataGSTR1_B2C_Small = ExecuteDataSet(ApplyB2BCondition(GSTR1_B2C_Small), param);
            var dataGSTR1_B2B_CN = ExecuteDataSet(ApplyB2BCondition(GSTR1_B2B_CN), param);
            //var dataGSTR1_DR = ExecuteDataSet(GSTR1_DR, param);
            var dataGSTR1_B2C_Export = ExecuteDataSet(ApplyB2BCondition(GSTR1_B2C_Export), param);
            var dataGSTR1_HSN = ExecuteDataSet(ApplyB2BCondition(GSTR1_HSN), param);

            var dataGSTR1_B2BHSN = ExecuteDataSet(ApplyB2BCondition(GSTR1_B2BHSN), param);
            var dataGSTR1_NONB2BHSN = ExecuteDataSet(ApplyB2BCondition(GSTR1_NONB2BHSN), param);

            var dataGSTR1_Doc = ExecuteDataSet(ApplyB2BCondition(GSTR1_Doc), param);

            var statelist = referencelistDataService.SearchList(new ReferencelistCreteria() { type = "STATE" });
            foreach (DataRow dr in dataGSTR1_B2B_SE_DE.Tables[0].Rows)
            {
                if (dr["poscode"] == DBNull.Value || dr["poscode"] == null || string.IsNullOrEmpty(dr["poscode"].ToString()))
                {
                    var statecode = dr["gstno"].ToString().Substring(0, 2);
                    if (statelist.Exists(x => x.code == statecode))
                    {
                        dr["poscode"] = statecode;
                        dr["posname"] = statelist.Find(x => x.code == statecode).name;
                    }
                }
            }
            dataGSTR1_B2B_SE_DE.Tables[0].AcceptChanges();

            string temppath = System.IO.Path.GetTempFileName() + ".xlsx";
            //using (System.IO.FileStream memoryStream = new System.IO.FileStream(temppath, System.IO.FileMode.Create))
            //{
            //ClosedXML.Excel.XLWorkbook xLWorkbook = new ClosedXML.Excel.XLWorkbook();
            //xLWorkbook.AddWorksheet(dataGSTR1_B2B_SE_DE.Tables[0], "GSTR1_B2B_SE_DE");
            //xLWorkbook.AddWorksheet(dataGSTR1_B2C_Large.Tables[0], "GSTR1_B2C_Large");
            //xLWorkbook.AddWorksheet(dataGSTR1_B2C_Small.Tables[0], "GSTR1_B2C_Small");
            //xLWorkbook.AddWorksheet(dataGSTR1_B2B_CN.Tables[0], "GSTR1_B2B_CN");
            //xLWorkbook.AddWorksheet(dataGSTR1_DR.Tables[0], "GSTR1_DR");
            //xLWorkbook.SaveAs(temppath,false);
            //}


            DataTable dtvalidation = new DataTable();
            dtvalidation.Columns.AddRange(new DataColumn[1] { new DataColumn("Type", typeof(string)) });
            dtvalidation.Columns.AddRange(new DataColumn[1] { new DataColumn("Tax", typeof(decimal)) });
            dtvalidation.Columns.AddRange(new DataColumn[1] { new DataColumn("Taxable", typeof(decimal)) });
            dtvalidation.Columns.AddRange(new DataColumn[1] { new DataColumn("Total", typeof(decimal)) });
            dtvalidation.Rows.Add(new object[] { "B2B",
            SumFieldValue(dataGSTR1_B2B_SE_DE.Tables[0], "tax"),
            SumFieldValue(dataGSTR1_B2B_SE_DE.Tables[0], "taxable"),
            SumFieldValue(dataGSTR1_B2B_SE_DE.Tables[0], "total")});

            dtvalidation.Rows.Add(new object[] { "B2C Large",
            SumFieldValue(dataGSTR1_B2C_Large.Tables[0], "tax"),
            SumFieldValue(dataGSTR1_B2C_Large.Tables[0], "taxable"),
            SumFieldValue(dataGSTR1_B2C_Large.Tables[0], "total")});

            dtvalidation.Rows.Add(new object[] { "B2C Small",
            SumFieldValue(dataGSTR1_B2C_Small.Tables[0], "tax"),
            SumFieldValue(dataGSTR1_B2C_Small.Tables[0], "taxable"),
            SumFieldValue(dataGSTR1_B2C_Small.Tables[0], "total")});

            dtvalidation.Rows.Add(new object[] { "Export",
            SumFieldValue(dataGSTR1_B2C_Export.Tables[0], "tax"),
            SumFieldValue(dataGSTR1_B2C_Export.Tables[0], "taxable"),
            SumFieldValue(dataGSTR1_B2C_Export.Tables[0], "total")});

            var ttax = SumFieldValue(dtvalidation, "Tax");
            var ttaxable = SumFieldValue(dtvalidation, "Taxable");
            var tnet = SumFieldValue(dtvalidation, "Total");

            dtvalidation.Rows.Add(new object[] { "Total",
            SumFieldValue(dtvalidation, "Tax"),
            SumFieldValue(dtvalidation, "Taxable"),
            SumFieldValue(dtvalidation, "Total")});

            var cntax = SumFieldValue(dataGSTR1_B2B_CN.Tables[0], "tax");
            var cntaxable = SumFieldValue(dataGSTR1_B2B_CN.Tables[0], "taxable");
            var cnnet = SumFieldValue(dataGSTR1_B2B_CN.Tables[0], "total");

            dtvalidation.Rows.Add(new object[] { "B2B CN",
            SumFieldValue(dataGSTR1_B2B_CN.Tables[0], "tax"),
            SumFieldValue(dataGSTR1_B2B_CN.Tables[0], "taxable"),
            SumFieldValue(dataGSTR1_B2B_CN.Tables[0], "total")});

            dtvalidation.Rows.Add(new object[] { "Final",
            ttax + cntax,
            ttaxable + cntaxable,
            tnet + cnnet });

            var htax = SumFieldValue(dataGSTR1_HSN.Tables[0], "tax");
            var htaxable = SumFieldValue(dataGSTR1_HSN.Tables[0], "taxable");
            var hnet = SumFieldValue(dataGSTR1_HSN.Tables[0], "total");

            dtvalidation.Rows.Add(new object[] { "HSN",
            SumFieldValue(dataGSTR1_HSN.Tables[0], "tax"),
            SumFieldValue(dataGSTR1_HSN.Tables[0], "taxable"),
            SumFieldValue(dataGSTR1_HSN.Tables[0], "total")});


            dtvalidation.Rows.Add(new object[] { "Different",
            htax - (ttax + cntax),
            htaxable - (ttaxable + cntaxable),
            hnet - (tnet + cnnet) });

            using (System.IO.MemoryStream memoryStream = new System.IO.MemoryStream())
            {
                ClosedXML.Excel.XLWorkbook xLWorkbook = new ClosedXML.Excel.XLWorkbook();
                xLWorkbook.AddWorksheet(dataGSTR1_B2B_SE_DE.Tables[0], "GSTR1_B2B_SE_DE");
                xLWorkbook.AddWorksheet(dataGSTR1_B2C_Large.Tables[0], "GSTR1_B2C_Large");
                xLWorkbook.AddWorksheet(dataGSTR1_B2C_Small.Tables[0], "GSTR1_B2C_Small");
                xLWorkbook.AddWorksheet(dataGSTR1_B2C_Export.Tables[0], "GSTR1_EXP");
                xLWorkbook.AddWorksheet(dataGSTR1_B2B_CN.Tables[0], "GSTR1_B2B_CN");
                //xLWorkbook.AddWorksheet(dataGSTR1_DR.Tables[0], "GSTR1_DR");
                xLWorkbook.AddWorksheet(dataGSTR1_HSN.Tables[0], "GSTR1_HSN");
                xLWorkbook.AddWorksheet(dataGSTR1_B2BHSN.Tables[0], "GSTR1_B2BHSN");
                xLWorkbook.AddWorksheet(dataGSTR1_NONB2BHSN.Tables[0], "GSTR1_NONB2BHSN");
                xLWorkbook.AddWorksheet(BuildGSTR1Documents(dataGSTR1_Doc), "DOCS");

                xLWorkbook.AddWorksheet(dtvalidation, "Summary");

                xLWorkbook.SaveAs(memoryStream);
                return memoryStream.ToArray();
            }

            //return System.IO.File.ReadAllBytes(temppath);
        }

        public decimal SumFieldValue(DataTable dt, string field)
        {
            decimal ret = 0;
            foreach (DataRow dr in dt.Rows)
            {
                ret += dr[field] != DBNull.Value ? Convert.ToDecimal(dr[field]) : 0;
            }
            return ret;
        }

    }

}

