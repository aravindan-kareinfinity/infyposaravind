using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
using Newtonsoft.Json;
using System.Text;
using WebAPI.Data;


namespace InfyPOS.Data
{
    public class WarehouseReportDataService : ReportData
    {
        const string SQL_LR = @"select lrentry.id lrid,lrentryno, lrno, receiveddate, lrdate, supplier.name as supplier, totalamount, status, lrentry.lrentrydate, noofpieces, noofbundels, noofboxes,
lrentry.modeofdelivery, lrentry.locationid, lrentry.entrylocationid, lrentry.retaillocationid, statusname, lrentry.lrcompanyid,lrentry.transportid, lrentry.agentid,suppliercompany.cityid suppliercompanycityid,
lrentry.invoiceno, lrentry.invoicedate, lrentry.hasfreightcharge, lrentry.hasloadingcharge, lrentry.freightcharge, lrentry.loadingcharge, lrentry.duedate, lrentry.bundlerackid, lrentry.lrsectionid, section.name lrsectionname,
lrentry.slipno, purchaser.name purchasername, transport.gst transportgst
from lrentry inner join supplier on supplier.id=lrentry.supplierid 
inner join inbox on inbox.referenceid=lrentry.id and typeid=1 
left outer join suppliercompany on suppliercompany.supplierid = supplier.id
left outer join transport on transport.id = lrentry.transportid 
left outer join referencelist section on section.id = lrentry.lrsectionid 
left outer join employee purchaser on purchaser.id = lrentry.purchasemanagerid ";

        const string SQL_LRInvoiceSummary = @"with stockvalues as
(
select lrinvoiceitems.lrinvoiceid lrinvoiceid, sum((stock.qty-stock.returnqty)*(stock.price-stock.discount)) stocksalevalue, 
sum((stock.qty-stock.returnqty)*stock.displaymrp) stockmrp, sum(stock.qty-(stock.transfered+stock.journalqty+stock.damagedqty+stock.returnqty)) whstock,
sum( (stock.qty - (stock.transfered+stock.returnqty+stock.damagedqty+stock.journalqty)) * stock.ibuyingprice::numeric) as whstockvalue
from lrentry inner join lrinvoice on lrentry.id=lrinvoice.lrid and lrinvoice.isactive=true
inner join lrinvoiceitems on lrinvoiceitems.lrinvoiceid = lrinvoice.id and lrinvoiceitems.isactive = true 
inner join stock on stock.invoiceitemid = lrinvoiceitems.id and stock.isactive = true
Where lrentry.isactive=true and lrinvoice.isactive=true and lrinvoiceitems.isactive=true {0} {1}
group by lrinvoiceid
),
lrreturns as
(
select lrinvoiceitems.lrinvoiceid, sum(lrinvoicereturnitems.qty) returnqty,sum(lrinvoicereturnitems.tax) tax,sum(lrinvoicereturnitems.amount  - lrinvoicereturnitems.discount) amount,
sum(case when lrinvoicereturn.freightamt>0 and lrinvoicereturn.qty > 0 then 
((lrinvoicereturn.freightamt+(case when lrinvoicereturn.freighttaxamt is null then 0 else lrinvoicereturn.freighttaxamt end))/lrinvoicereturn.qty)
else 0 end) returnfright
from lrinvoicereturnitems
inner join lrinvoicereturn on lrinvoicereturn.id = lrinvoicereturnid and lrinvoicereturn.returntype<14
inner join lrinvoiceitems on lrinvoiceitems.id= lrinvoiceitemsid  
left outer join stockvalues on stockvalues.lrinvoiceid = lrinvoiceitems.lrinvoiceid
where lrinvoicereturn.isactive  = true and lrinvoicereturnitems.isactive=true and lrinvoiceitems.isactive=true
group by lrinvoiceitems.lrinvoiceid),
salevalues as
(
select lrinvoiceitems.lrinvoiceid lrinvoiceid, sum(oqty) soldqty, sum((salablegoods.iqty-(salablegoods.oqty+salablegoods.jqty+salablegoods.dqty))) closingqty,
sum( (salablegoods.iqty-(salablegoods.oqty+salablegoods.jqty+salablegoods.dqty)) * stock.ibuyingprice::numeric) as closingvalue
from lrentry 
inner join lrinvoice on lrentry.id=lrinvoice.lrid and lrinvoice.isactive=true
inner join lrinvoiceitems on lrinvoiceitems.lrinvoiceid = lrinvoice.id and lrinvoiceitems.isactive = true 
inner join stock on stock.invoiceitemid = lrinvoiceitems.id and stock.isactive = true
inner join salablegoods on salablegoods.stockid = stock.id and salablegoods.isactive = true
Where lrentry.isactive=true and lrinvoice.isactive=true and lrinvoiceitems.isactive=true {0} {1}
group by lrinvoiceid
),
returnvalues as
(
select lrinvoiceitems.lrinvoiceid, sum(buyingqty) qty, sum(lrinvoiceitems.returnqty) returnqty, 
sum(lrinvoiceitems.returnqty::numeric * lrinvoiceitems.buyingprice::numeric) returnvalue,
sum(lrinvoiceitems.buyingqty::numeric * lrinvoiceitems.buyingprice::numeric) dealercost,
sum(lrinvoiceitems.buyingqty::numeric * lrinvoiceitems.dealerprice::numeric - (lrinvoiceitems.buyingqty::numeric * lrinvoiceitems.dealerprice::numeric * salestaxpercentage/100)) dealerprice,
avg(marginpercentage::numeric) marginpercentage,
sum( case when lrinvoiceitems.purchasetax>0 and lrinvoiceitems.returnqty>0 
	then lrinvoiceitems.returnqty * (lrinvoiceitems.purchasetax/lrinvoiceitems.buyingqty) else 0 end ) taxvalue
from lrentry 
inner join lrinvoice on lrentry.id=lrinvoice.lrid and lrinvoice.isactive=true
inner join lrinvoiceitems on lrinvoiceitems.lrinvoiceid = lrinvoice.id and lrinvoiceitems.isactive = true 
Where lrentry.isactive=true and lrinvoice.isactive=true and lrinvoiceitems.isactive=true {0} {1}
group by lrinvoiceid
)
select lrentry.id,lrinvoice.id invoiceid, company.name company, suppliercompany.name supplier, suppliercompany.cityid, suppliercompany.bankaccountname,suppliercompany.bankaccountno,suppliercompany.bankifci,
lrentryno, lrinvoice.entrydate entrydate, lrinvoice.invoiceno, lrinvoice.invoicedate, lrinvoice.isinterstatesale, 
lrinvoice.taxamount taxamount, lrinvoice.baseamount taxableamount, lrinvoice.totalamount invoiceamount,lrentry.locationid,lrentry.entrylocationid,lrentry.retaillocationid,
lrreturns.returnqty, (lrreturns.amount+lrreturns.tax+lrreturns.returnfright::numeric) returnvalue,  lrreturns.returnfright, 
returnvalues.qty, returnvalues.marginpercentage,lrinvoice.noofbundle,returnvalues.dealercost,returnvalues.dealerprice,
salevalues.soldqty, salevalues.closingqty, salevalues.closingvalue, stockvalues.whstock, stockvalues.whstockvalue,
stockvalues.stockmrp,stockvalues.stocksalevalue, lrentry.duedate, suppliercompany.tinanddate,
employee.name employeename
from lrentry 
inner join lrinvoice on lrentry.id=lrinvoice.lrid and lrinvoice.isactive=true
inner join suppliercompany on suppliercompany.id=lrinvoice.suppliercompanyid 
inner join company on company.id=lrinvoice.companyid  
left outer join employee on employee.id = lrentry.purchasemanagerid
left outer join returnvalues on returnvalues.lrinvoiceid = lrinvoice.id 
left outer join lrreturns on lrreturns.lrinvoiceid = lrinvoice.id 
left outer join salevalues on salevalues.lrinvoiceid = lrinvoice.id 
left outer join stockvalues on stockvalues.lrinvoiceid = lrinvoice.id 
Where lrentry.isactive=true and lrinvoice.isactive=true {0} {1}
order by lrentry.id ";

        const string SQL_LRInvoice = @"select lrinvoice.companyid, lrinvoice.id, lrinvoice.invoiceno, lrinvoice.invoicedate, lrentryno, baseamount, grossamount, taxamount, discount, lrinvoice.totalamount, otheramount, lrinvoice.entrydate, supplier.name as supplier 
from lrentry inner join supplier on supplier.id=lrentry.supplierid 
inner join lrinvoice on lrentry.id=lrinvoice.lrid";

        const string SQL_Purchase = @"select lrentry.id,lrentry.duedate,lrinvoice.id invoiceid, company.name company, suppliercompany.name supplier, suppliercompany.cityid, suppliercompany.tinanddate,
lrentryno, lrinvoice.entrydate entrydate, lrinvoice.invoiceno, lrinvoice.invoicedate, lrinvoicetax.percentage, lrinvoice.isinterstatesale, 
lrinvoice.otheramount,lrentry.entrylocationid,lrentry.retaillocationid, lrinvoice.totalamount, lrinvoice.discount, lrentry.agentid,
lrentry.receivinglocationid,
lrentry.lrno,msmeno,tdsamount,tdspercentage,
sum(lrinvoicetax.taxamount) taxamount, 
sum(lrinvoicetax.discount) discountbytax,
sum(case when lrinvoicetax.taxamount>0 then amounton-lrinvoicetax.discount else 0 end) taxableamount,
sum(netamount) invoiceamount,
sum(case when (lrinvoicetax.type>3 and lrinvoicetax.type<9) or lrinvoicetax.type=10 then netamount else 0 end) forwardcharge,
sum(case when lrinvoicetax.type=12 then netamount else 0 end) tcsamount 
{0}
from lrentry 
inner join lrinvoice on lrentry.id=lrinvoice.lrid and lrinvoice.isactive=true
left outer join suppliercompany on suppliercompany.id=lrinvoice.suppliercompanyid 
inner join lrinvoicetax on lrinvoice.id=lrinvoicetax.invoiceid and lrinvoicetax.isactive=true
left outer join company on company.id=lrinvoice.companyid 
{1} ";

        const string SQL_PurchaseItemDetails = @"select lrentry.id,lrinvoice.id invoiceid, company.name company, suppliercompany.name supplier, suppliercompany.cityid,
lrentryno, lrinvoice.entrydate entrydate, lrinvoice.invoiceno, lrinvoice.invoicedate, lrinvoice.isinterstatesale,lrinvoiceitems.purchasetaxpercentage,products.name productname, products.salescounterid,
sum(lrinvoiceitems.buyingqty) qty, sum(lrinvoiceitems.buyingqty::numeric*lrinvoiceitems.basebuyingprice::numeric) baseamount, sum(lrinvoiceitems.buyingqty::numeric*lrinvoiceitems.buyingprice::numeric) grossamount,
sum(lrinvoiceitems.purchasetax) tax, sum(lrinvoiceitems.buyingqty::numeric*lrinvoiceitems.purchasediscount::numeric) discount
from lrentry inner join lrinvoice on lrentry.id=lrinvoice.lrid and lrinvoice.isactive=true
inner join suppliercompany on suppliercompany.id=lrinvoice.suppliercompanyid 
inner join lrinvoiceitems on lrinvoiceitems.lrinvoiceid=lrinvoice.id and lrinvoiceitems.isactive = true
inner join products on products.id = lrinvoiceitems.productid
inner join company on company.id=lrinvoice.companyid ";


        const string SQL_PurchaseHSN = @"select lrinvoice.id invoiceid, lrinvoiceitems.hsncode, items.productname, lrinvoice.isinterstatesale, lrinvoice.otheramount, suppliercompany.name supplier, suppliercompany.cityid, lrinvoiceitems.purchasetaxpercentage, 
sum(lrinvoiceitems.buyingprice) buyingprice, sum(lrinvoiceitems.buyingqty) buyingqty, case when products.sellingmode = 3 then 'M' else 'N' end as Measure,
sum(lrinvoiceitems.buyingqty::numeric*lrinvoiceitems.buyingprice::numeric) as value, sum(lrinvoiceitems.purchasetax) tax
from lrentry inner join lrinvoice on lrentry.id=lrinvoice.lrid and lrinvoice.isactive=true
inner join lrinvoiceitems on lrinvoiceitems.lrinvoiceid=lrinvoice.id  and lrinvoiceitems.isactive=true
inner join products on products.id = lrinvoiceitems.productid
inner join suppliercompany on suppliercompany.id=lrinvoice.suppliercompanyid 
left outer join items on items.id = lrinvoiceitems.itemid ";

        const string SQL_PurchaseTax = @"select 
lrinvoice.isinterstatesale, suppliercompany.name supplier, suppliercompany.cityid, 
lrinvoicetax.percentage,
sum(lrinvoicetax.taxamount) taxamount, city.name cityname,
sum(case when lrinvoicetax.taxamount>0 then amounton-lrinvoicetax.discount else 0 end) taxableamount,
sum(netamount) invoiceamount
from lrentry 
inner join lrinvoice on lrentry.id=lrinvoice.lrid and lrinvoice.isactive=true
inner join lrinvoicetax on lrinvoicetax.invoiceid=lrinvoice.id  and lrinvoicetax.isactive=true
inner join suppliercompany on suppliercompany.id=lrinvoice.suppliercompanyid
left outer join referencelist city on city.id = suppliercompany.cityid";

        const string SQL_DebitNotesummary = @"select lrinvoicereturn.id, lrinvoicereturn.locationid, lrinvoice.id invoiceid, supplier.name as supplier, 
suppliercompany.cityid, returnno, returndate, lrinvoice.invoiceno, lrinvoice.invoicedate, suppliercompany.tinanddate,lrinvoicereturn.createdby,lrinvoicereturn.modifiedby,
lrinvoicereturn.isinterstatesale, lrinvoicereturn.qty, lrinvoicereturn.amount,lrinvoicereturn.tax,lrinvoicereturn.discount,lrinvoicereturn.total,lrinvoicereturn.rounding, lrinvoicereturnitems.taxpercentage as taxname, 
freightamt,freighttaxamt,packingcharge,
sum(lrinvoicereturnitems.tax) itemtax, sum(lrinvoicereturnitems.amount-lrinvoicereturnitems.discount) as taxable,
sum((sellingprice - salediscount) * lrinvoicereturnitems.qty) salevalue
from lrinvoicereturn 
inner join lrinvoicereturnitems on lrinvoicereturnitems.lrinvoicereturnid = lrinvoicereturn.id and lrinvoicereturn.returntype<14
inner join supplier on supplier.id=lrinvoicereturn.supplierid
inner join suppliercompany on suppliercompany.supplierid=supplier.id
left outer join lrinvoiceitems on lrinvoiceitems.id = lrinvoiceitemsid and lrinvoiceitems.isactive = true
left outer join lrinvoice on lrinvoice.id = lrinvoiceitems.lrinvoiceid  and lrinvoice.isactive = true ";

        const string SQL_DebitNoteDetail = @"select lrinvoicereturn.id, lrinvoicereturn.locationid, supplier.name as supplier, 
suppliercompany.cityid, returnno, returndate, suppliercompany.tinanddate,lrinvoicereturn.createdby,lrinvoicereturn.modifiedby,
lrinvoice.invoiceno, lrinvoice.invoicedate, lrinvoicereturnitems.taxpercentage as taxname, lrinvoicereturnitems.tax, rate, lrinvoicereturnitems.amount as amount,
lrinvoicereturnitems.productid, lrinvoicereturnitems.qty, lrinvoicereturnitems.barcode, lrinvoicereturnitems.discount, lrinvoiceitems.hsncode,
freightamt,freighttaxamt,packingcharge, (sellingprice-salediscount) * lrinvoicereturnitems.qty salevalue
from lrinvoicereturn 
inner join lrinvoicereturnitems on lrinvoicereturnitems.lrinvoicereturnid = lrinvoicereturn.id and lrinvoicereturn.returntype<14
inner join supplier on supplier.id=lrinvoicereturn.supplierid
inner join suppliercompany on suppliercompany.supplierid=supplier.id
left outer join lrinvoiceitems on lrinvoiceitems.id = lrinvoiceitemsid and lrinvoiceitems.isactive = true
left outer join lrinvoice on lrinvoice.id = lrinvoiceitems.lrinvoiceid  and lrinvoice.isactive = true";


        const string SQL_NewProducts = @"select products.code, products.name as productname, products.isactive,salescounterid, products.createdby, products.createdon,sellingmode, pricingmode, salestaxid, discountmode, discountvalue, purchasetaxid, 
                                     margin, image,  pricerange,keygroup,employee.name as employeename,
									purchaseplanmode,sizegroupid,productgroup,unit,fixedprice from products 
                                     left outer join users on users.id=products.createdby 
                                     left outer join employee on employee.id=users.employeeid  
                                    where products.isactive = true ";

        const string SQL_NewSuppliers = @"select supplier.name, supplier.isactive, supplier.createdby, supplier.createdon,employee.name as employeename 
                                  from supplier left outer join users on users.id=supplier.createdby 
                                     left outer join employee on employee.id=users.employeeid ";

        const string SQL_OutwardwithCost = @"with stockvalue as 
(
        select bundles.id bundlesid, {0}
		sum(stock.buyingprice * cast(itemlist->> 'iqty' as numeric)) buyingprice, 
        sum(stock.ibuyingprice * cast(itemlist->> 'iqty' as numeric)) ibuyingprice, 
		sum(stock.price * cast(itemlist->> 'iqty' as numeric)) price, 
        sum(stock.displaymrp * cast(itemlist->> 'iqty' as numeric)) mrpprice, 
		sum(stock.discount * cast(itemlist->> 'iqty' as numeric)) discount,
        sum(cast(itemlist->> 'iqty' as numeric)) qty, sum(cast(itemlist->> 'aqty' as numeric)) acceptqty
        from bundles INNER JOIN LATERAL json_array_elements (case when bundles.items is null or bundles.items::text ='null' then '[]'::json else bundles.items :: json end) itemlist ON true
        inner join stock on stock.id = cast(itemlist->> 'stockid' as bigint) and stock.isactive = true
        left outer join lrinvoiceitems on lrinvoiceitems.id = invoiceitemid
        where bundles.isactive =true and packedon>=@fromdate and packedon<@todate 
        and bundles.status=0 
        and case when @locationid=0 then tolocationid != 0 else tolocationid=@locationid end        
        and case when @fromlocationid=0 then fromlocationid != 0 else fromlocationid=@fromlocationid end   
        and case when @fromcompanyid=0 then fromcompanyid >= 0 else fromcompanyid=@fromcompanyid end             
        and case when @tocompanyid=0 then tocompanyid >= 0 else tocompanyid=@tocompanyid end   
        and case when @supplierid=0 then supplierid >= 0 else supplierid=@supplierid end                  
        group by bundles.id {1}
)
        select bundles.id,bundles.status,{0} bundles.code, bundles.fromcompanyid, bundles.tocompanyid, bundles.iscompanytransfer,
        referencelistf.name fromlocation, referencelistt.name tolocation, packedon, receivedon, employee.name receivedby,bundles.tolocationid,bundles.fromlocationid,
        stockvalue.qty, stockvalue.acceptqty, stockvalue.buyingprice, stockvalue.ibuyingprice, stockvalue.price, stockvalue.discount, stockvalue.mrpprice
        from bundles 
        inner join stockvalue on stockvalue.bundlesid = bundles.id
        left outer join referencelist referencelistf on referencelistf.type ='LOCATION' and referencelistf.id = fromlocationid
        left outer join referencelist referencelistt on referencelistt.type ='LOCATION' and referencelistt.id = tolocationid
        left outer join employee on employee.id = receivedby 
        where bundles.isactive =true and packedon>=@fromdate and packedon<@todate 
        and bundles.status=0
        and case when @locationid=0 then tolocationid != 0 else tolocationid=@locationid end        
        and case when @fromlocationid=0 then fromlocationid != 0 else fromlocationid=@fromlocationid end        
        and case when @fromcompanyid=0 then fromcompanyid >= 0 else fromcompanyid=@fromcompanyid end             
        and case when @tocompanyid=0 then tocompanyid >= 0 else tocompanyid=@tocompanyid end             
        order by packedon desc";

        const string SQL_Outward = @"select bundles.code, bundles.fromcompanyid, bundles.tocompanyid, bundles.iscompanytransfer,
        referencelistf.name fromlocation, referencelistt.name tolocation, packedon, receivedon, employee.name receivedby,
        sum(cast(itemlist->> 'iqty' as numeric)) qty, sum(cast(itemlist->> 'aqty' as numeric)) acceptqty
        from bundles INNER JOIN LATERAL json_array_elements (case when bundles.items is null or bundles.items::text ='null' then '[]'::json else bundles.items :: json end) itemlist ON true
        left outer join referencelist referencelistf on referencelistf.type ='LOCATION' and referencelistf.id = fromlocationid
        left outer join referencelist referencelistt on referencelistt.type ='LOCATION' and referencelistt.id = tolocationid
        left outer join employee on employee.id = receivedby 
        where bundles.isactive =true and packedon>=@fromdate and packedon<@todate 
        and case when @status=0 then bundles.status=0 else bundles.status!=0  end
        and case when @locationid=0 then tolocationid != 0 else tolocationid=@locationid end        
        group by bundles.code, bundles.fromcompanyid, bundles.tocompanyid, bundles.iscompanytransfer,
        fromlocation,  tolocation, packedon, receivedon, employee.name 
        order by packedon desc";

        const string SQL_OutwardItems = @"
        select bundles.code, bundles.status, supplier.name suppliername, referencelistf.name fromlocation,referencelistt.name tolocation, 
        bundles.fromcompanyid, bundles.tocompanyid,  bundles.iscompanytransfer, packedon, receivedon, employee.name receivedby,
        products.name printingname, cast(itemlist->> 'barcode' as character varying(20)) barcode, 
        cast(itemlist->> 'iqty' as numeric) qty, cast(itemlist->> 'aqty' as numeric) acceptqty,bundles.tolocationid,bundles.fromlocationid,
        purchasetaxpercentage, brand.name brandname, stock.designid, stock.hsncode, lrinvoice.invoiceno, lrinvoice.invoicedate,
        style.name stylename,pattern.name patternname,size.name sizename, colour.name colourname,type.name typename,material.name materialname,
        division.name section, (stock.buyingprice * cast(itemlist->> 'iqty' as numeric)) buyingprice, 
        (stock.ibuyingprice * cast(itemlist->> 'iqty' as numeric)) ibuyingprice, 
        (stock.price * cast(itemlist->> 'iqty' as numeric)) price, (stock.discount * cast(itemlist->> 'iqty' as numeric)) discount,
        (stock.displaymrp * cast(itemlist->> 'iqty' as numeric)) mrpprice,
        stock.buyingprice itembuyingprice, stock.ibuyingprice itemibuyingprice, stock.price itemprice, stock.discount itemdiscount, stock.isbit, stock.displaymrp itemmrp
        from bundles INNER JOIN LATERAL json_array_elements (case when bundles.items is null or bundles.items::text ='null' then '[]'::json else bundles.items :: json end) itemlist ON true
        inner join stock on stock.id = cast(itemlist->> 'stockid' as bigint) and stock.isactive = true
        left outer join lrinvoiceitems on lrinvoiceitems.id = invoiceitemid
        left outer join lrinvoice on lrinvoice.id = lrinvoiceitems.lrinvoiceid
        left outer join supplier on supplier.id = stock.supplierid
        left outer join referencelist referencelistf on referencelistf.type ='LOCATION' and referencelistf.id = fromlocationid
        left outer join referencelist referencelistt on referencelistt.type ='LOCATION' and referencelistt.id = tolocationid
        left outer join employee on employee.id = receivedby 
        left outer join items on stock.itemid=items.id and stock.isactive=true
        left outer join products on products.id = stock.productid
        left outer join brand brand on brand.id = items.brandid
        left outer join referencelist colour on colour.id = colourid
        left outer join referencelist size on size.id = sizeid
        left outer join referencelist style on style.id = styleid
        left outer join referencelist pattern on pattern.id = patternid
        left outer join referencelist material on material.id = materialid
        left outer join referencelist type on type.id = typeid
		left outer join referencelist division on division.id = products.salescounterid
        where bundles.isactive=true {0}
        order by packedon desc";

        const string SQL_OutwardWithLR = @"select DISTINCT lrentry.lrentryno, lrinvoice.invoiceno, bundles.code, referencelistf.name fromlocation,
referencelistt.name tolocation, packedon, receivedon, employee.name receivedby,bundles.iscompanytransfer,bundles.fromcompanyid, bundles.tocompanyid,
cast(itemlist->> 'iqty' as numeric) qty, cast(itemlist->> 'aqty' as numeric) acceptqty
from bundles INNER JOIN LATERAL json_array_elements (case when bundles.items is null or bundles.items::text ='null' then '[]'::json else bundles.items :: json end) itemlist ON true
inner join stock on stock.id = cast(itemlist->> 'stockid' as bigint)
inner join lrinvoiceitems on lrinvoiceitems.id = stock.invoiceitemid and lrinvoiceitems.isactive=true
inner join lrinvoice on lrinvoice.id = lrinvoiceitems.lrinvoiceid  and lrinvoice.isactive = true
inner join lrentry on lrentry.id = lrinvoice.lrid
left outer join referencelist referencelistf on referencelistf.type ='LOCATION' and referencelistf.id = bundles.fromlocationid
left outer join referencelist referencelistt on referencelistt.type ='LOCATION' and referencelistt.id = bundles.tolocationid
left outer join employee on employee.id = receivedby 
where bundles.isactive =true and packedon>=@fromdate and packedon<@todate 
and case when @status=0 then bundles.status=0 else bundles.status!=0  end
and case when @supplier=0 then cast(itemlist->> 'supplierid' as bigint) <> 0 else cast(itemlist->> 'supplierid' as bigint)=@supplier end 
and case when @lrentryno='' then lrentry.lrentryno <> '' else lrentry.lrentryno = @lrentryno end 
order by packedon desc";

        const string SQL_OutwardSupplier = @"select bundles.code,qty, items, referencelistf.name fromlocation,referencelistt.name tolocation, packedon, receivedon, employee.name receivedby 
from bundles INNER JOIN LATERAL json_array_elements (case when bundles.items is null or bundles.items::text ='null' then '[]'::json else bundles.items :: json end) itemlist ON true
left outer join referencelist referencelistf on referencelistf.type ='LOCATION' and referencelistf.id = fromlocationid
left outer join referencelist referencelistt on referencelistt.type ='LOCATION' and referencelistt.id = tolocationid
left outer join employee on employee.id = receivedby
where bundles.isactive =true and cast(itemlist->> 'supplierid' as bigint)=@supplier and packedon>=@fromdate and packedon<@todate 
and case when @status=0 then bundles.status=0 else bundles.status!=0  end
and case when @code='' then bundles.code <> '' else bundles.code = @code end 
order by packedon desc";

        const string SQL_OutwardSupplierSummary = @"select bundles.code, bundles.fromcompanyid, bundles.tocompanyid, bundles.iscompanytransfer,
        referencelistf.name fromlocation, referencelistt.name tolocation, packedon, receivedon, employee.name receivedby,
        sum(cast(itemlist->> 'iqty' as numeric)) qty, sum(cast(itemlist->> 'aqty' as numeric)) acceptqty
        from bundles INNER JOIN LATERAL json_array_elements (case when bundles.items is null or bundles.items::text ='null' then '[]'::json else bundles.items :: json end) itemlist ON true
        left outer join referencelist referencelistf on referencelistf.type ='LOCATION' and referencelistf.id = fromlocationid
        left outer join referencelist referencelistt on referencelistt.type ='LOCATION' and referencelistt.id = tolocationid
        left outer join employee on employee.id = receivedby
        where bundles.isactive =true and cast(itemlist->> 'supplierid' as bigint)=@supplier 
        and packedon>=@fromdate and packedon<@todate 
        and case when @status=0 then bundles.status=0 else bundles.status!=0  end
        and case when @locationid=0 then tolocationid != 0 else tolocationid=@locationid end        
        and case when @code='' then bundles.code <> '' else bundles.code = @code end 
        group by bundles.code, bundles.fromcompanyid, bundles.tocompanyid, bundles.iscompanytransfer,
        fromlocation,  tolocation, packedon, receivedon, employee.name 
                                    order by packedon desc";


        const string SQL_Stock = @"
select stock.id,stock.invoiceitemid, stock.barcode,products.name as printingname, stock.itemid, stock.productid, stock.buyingprice taxable,
stock.ibuyingprice,stock.buyingprice,(stock.ibuyingprice-stock.buyingprice) tax, stock.dealerprice,stock.serialno,
stock.price, stock.discount, stock.qty pqty, stock.qty-stock.transfered-stock.journalqty-stock.damagedqty-stock.returnqty wstock, brand.name brandname,
products.name as productname, stock.designid, stock.hsncode,supplier.name suppliername,products.sellingmode, products.productgroup, products.salescounterid, 
items.sizeid, items.typeid, items.colourid,items.materialid,items.styleid,items.brandid,items.patternid,lrinvoiceitems.mrp, items.sleeveid, items.fitid,
style.name stylename,pattern.name patternname,size.name sizename, colour.name colourname,type.name typename,material.name materialname,sleeve.name sleevename,fit.name fitname, 
pricetag.name pricetagname, EXTRACT(day from @current_date - lrinvoice.entrydate) age, lrentry.stockholdingperiod, EXTRACT(day from @current_date - stock.stockdate) age1,
lrinvoiceitems.itbuscode,stock.isbit
from stock 
left outer join lrinvoiceitems on lrinvoiceitems.id = stock.invoiceitemid and lrinvoiceitems.isactive = true 
left outer join lrinvoice on lrinvoice.id=lrinvoiceitems.lrinvoiceid and lrinvoice.isactive=true
left outer join lrentry on lrentry.id = lrinvoice.lrid
left outer join items on items.id = stock.itemid
left outer join products on products.id = stock.productid
left outer join supplier on supplier.id = lrentry.supplierid
left outer join brand brand on brand.id = items.brandid
                        left outer join referencelist colour on colour.id = colourid
                        left outer join referencelist size on size.id = sizeid
                        left outer join referencelist style on style.id = styleid
                        left outer join referencelist pattern on pattern.id = patternid
                        left outer join referencelist material on material.id = materialid
                        left outer join referencelist sleeve on sleeve.id = sleeveid
                        left outer join referencelist fit on fit.id = fitid
                        left outer join referencelist pricetag on pricetag.id = stock.pricetagid
                        left outer join referencelist type on type.id = typeid";


        const string SQL_PendingTransfer = @"select stock.barcode,stock.printingname, stock.itemid, stock.productid, stock.buyingprice buyingprice, 
stock.price, stock.discount, stock.qty pqty, stock.qty-stock.transfered-stock.journalqty-stock.damagedqty-stock.returnqty wstock, items.brandname, items.patternname, items.stylename, 
items.productname, items.sizename, items.materialname,items.typename, supplier.name as suppliername, lrreference
from stock left outer join items on items.id = stock.itemid
left outer join supplier on supplier.id = stock.supplierid";

        const string SQL_BarcodeDetail = @"select stock.id stockid, stock.barcode,stock.printingname, stock.itemid, stock.productid, stock.buyingprice buyingprice, 
stock.price, stock.discount, stock.qty pqty, stock.qty-stock.transfered-stock.journalqty-stock.damagedqty wstock, items.brandname, items.patternname, items.stylename, 
items.productname, items.sizename, items.materialname,items.typename, lrentry.lrno, lrdate, lrentry.lrentryno, stock.itemid,pricetag.name pricetagname,
lrinvoice.invoiceno, lrinvoice.entrydate, lrinvoice.invoicedate, invoiceentryno, lrinvoiceitems.buyingqty, lrinvoiceitems.buyingprice, lrinvoiceitems.mrp, lrinvoiceitems.sellingprice,
lrinvoiceitems.basebuyingprice, lrinvoiceitems.purchasediscountpercentage, lrinvoiceitems.taxid, lrinvoiceitems.purchasetax, lrinvoiceitems.purchasediscount, lrinvoiceitems.hsncode,
lrinvoiceitems.salediscount, lrinvoiceitems.salediscountpercentage, lrinvoiceitems.marginpercentage, lrinvoiceitems.salestaxpercentage, lrinvoiceitems.purchasetaxpercentage, 
lrinvoiceitems.salestax, lrinvoiceitems.ptaxid, supplier.name supplier, company.name company, section.name sectionname, lrinvoiceitems.designid, lrinvoice.id lrinvoiceid, 
lrinvoiceitems.id lrinvoiceitemid, products.sellingmode, lrinvoiceitems.barcodes, lrinvoice.companyid, lrinvoice.lrid,
stock.companyid stockcompanyid, stock.invoiceitemid
from stock inner join items on items.id = stock.itemid and stock.isactive = true
inner join products on products.id = stock.productid 
left outer join lrinvoiceitems on lrinvoiceitems.id = invoiceitemid  and lrinvoiceitems.isactive = true
left outer join lrinvoice on lrinvoiceitems.lrinvoiceid = lrinvoice.id and lrinvoice.isactive = true
left outer join lrentry on lrentry.id=lrinvoice.lrid and lrentry.isactive = true
left outer join tax on tax.id = lrinvoiceitems.taxid 
left outer join supplier on supplier.id = lrentry.supplierid 
left outer join company on company.id = lrinvoice.companyid 
left outer join referencelist section on section.id = products.salescounterid
left outer join referencelist pricetag on pricetag.id = stock.pricetagid";

        const string SQL_ROLStoreView = @"select lrentry.supplierid,lrentry.lrno,lrentry.id lrid,
lrentry.lrentryno,lrinvoice.invoiceno,
lrinvoice.invoicedate as invoicedate,
lrentrydate,products.name as productname,sum(salablegoods.iqty-(salablegoods.oqty+salablegoods.jqty+salablegoods.dqty)) as stockqty,
EXTRACT(day from @current_date - lrentry.lrentrydate) pendingdays from salablegoods 
inner join stock on stock.id = stockid
inner join lrinvoiceitems on lrinvoiceitems.id = stock.invoiceitemid
inner join lrinvoice on lrinvoice.id = lrinvoiceid
inner join lrentry on lrentry.id = lrid
inner join products on products.id = stock.productid and products.salescounterid=@sectionid
where  salablegoods.locationid = @locationid and salablegoods.hasstock
and EXTRACT(day from @current_date - lrentry.lrentrydate)> COALESCE(lrentry.stockholdingperiod,999)  
group by lrentry.supplierid,lrentry.lrno,lrentry.id,lrentry.lrentryno,lrinvoice.invoiceno,
lrinvoice.invoicedate,lrentrydate,products.name,lrentry.stockholdingperiod";

        const string SQL_ROLStoreViewWarehouse = @"select lrentry.supplierid,lrentry.lrno,lrentry.id lrid,
lrentry.lrentryno,lrinvoice.invoiceno,
lrinvoice.invoicedate as invoicedate,
lrentrydate,products.name as productname,sum(stock.qty - (stock.transfered+stock.journalqty+stock.returnqty+stock.damagedqty)) as stockqty,
EXTRACT(day from @current_date - lrentry.lrentrydate) pendingdays from stock 
inner join lrinvoiceitems on lrinvoiceitems.id = stock.invoiceitemid
inner join lrinvoice on lrinvoice.id = lrinvoiceid
inner join lrentry on lrentry.id = lrid
inner join products on products.id = stock.productid and products.salescounterid=@sectionid
where stock.hasstock and
and EXTRACT(day from @current_date - lrentry.lrentrydate)> COALESCE(lrentry.stockholdingperiod,999)  
group by lrentry.supplierid,lrentry.lrno,lrentry.id,lrentry.lrentryno,lrinvoice.invoiceno,
lrinvoice.invoicedate,lrentrydate,products.name,lrentry.stockholdingperiod";

        const string SQL_ROLStoreViewDetailsWarehouse = @"select stock.barcode, products.name as productname,
stock.qty iqty,(stock.transfered+stock.returnqty+stock.journalqty+stock.damagedqty) as oqty,(stock.qty - (stock.transfered+stock.returnqty+stock.journalqty+stock.damagedqty)) stockqty,
stock.supplierid,stock.id,stock.id stockid,stock.locationid,stock.companyid,
lrreference,stock.itemid
from stock 
inner join lrinvoiceitems on lrinvoiceitems.id = stock.invoiceitemid
inner join lrinvoice on lrinvoice.id = lrinvoiceid
inner join lrentry on lrentry.id = lrid
inner join products on products.id = stock.productid
where  lrentry.id = @lrid
and stock.qty - (stock.transfered+stock.returnqty+stock.journalqty+stock.damagedqty)>0";

        const string SQL_ROLStoreViewDetails = @"select salablegoods.barcode, products.name as productname,
salablegoods.iqty,salablegoods.oqty,(salablegoods.iqty-(salablegoods.oqty+salablegoods.jqty+salablegoods.dqty)) stockqty,
stock.supplierid,salablegoods.id,salablegoods.stockid,salablegoods.locationid,salablegoods.companyid,
lrreference,stock.itemid
from salablegoods 
inner join stock on stock.id = stockid
inner join lrinvoiceitems on lrinvoiceitems.id = stock.invoiceitemid
inner join lrinvoice on lrinvoice.id = lrinvoiceid
inner join lrentry on lrentry.id = lrid
inner join products on products.id = stock.productid
where  lrentry.id = @lrid
and salablegoods.iqty-(salablegoods.oqty+salablegoods.jqty+salablegoods.dqty)>0";

        public List<LRReport> SearchLR(WarehouseReportCreteria _reportcreteria)
        {
            List<LRReport> lst = new List<LRReport>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                NpgsqlCommand command = null;

                QueryBuilder qb = new QueryBuilder();
                qb.CurrentQuery = SQL_LR;

                qb.AddParameter("lrentry.isactive", true, "=");

                if (_reportcreteria.fromdate != DateTime.MinValue && _reportcreteria.todate != DateTime.MinValue)
                {
                    qb.AddParameter("lrentry.lrentrydate", "datefrom", _reportcreteria.fromdate.Date, ">=");
                    qb.AddParameter("lrentry.lrentrydate", "dateto", _reportcreteria.todate.Date.AddDays(1), "<");
                }
                if (_reportcreteria.fromlrdate != DateTime.MinValue && _reportcreteria.tolrdate != DateTime.MinValue)
                {
                    qb.AddParameter("lrentry.lrdate", "lrdatefrom", _reportcreteria.fromlrdate.Date, ">=");
                    qb.AddParameter("lrentry.lrdate", "lrdateto", _reportcreteria.tolrdate.Date.AddDays(1), "<");
                }
                if (_reportcreteria.frominvdate != DateTime.MinValue && _reportcreteria.toinvdate != DateTime.MinValue)
                {
                    qb.AddParameter("lrentry.invoicedate", "frominvdate", _reportcreteria.frominvdate.Date, ">=");
                    qb.AddParameter("lrentry.invoicedate", "toinvdate", _reportcreteria.toinvdate.Date.AddDays(1), "<");
                }

                if (_reportcreteria.transportid > 0)
                    qb.AddParameter("lrentry.transportid", _reportcreteria.transportid, "=");
                if (_reportcreteria.supplierid > 0)
                    qb.AddParameter("lrentry.supplierid", _reportcreteria.supplierid, "=");
                if (_reportcreteria.pendinginward)
                    qb.AddParameter("inbox.statusid", 2, "!=");
                if (_reportcreteria.modeofdelivery > 0)
                    qb.AddParameter("lrentry.modeofdelivery", _reportcreteria.modeofdelivery, "=");
                if (_reportcreteria.companyid > 0)
                    qb.AddParameter("lrentry.lrcompanyid", _reportcreteria.companyid, "=");
                if (_reportcreteria.bundlerackid > 0)
                    qb.AddParameter("lrentry.bundlerackid", _reportcreteria.bundlerackid, "=");
                if (_reportcreteria.purchaserid > 0)
                    qb.AddParameter("lrentry.purchasemanagerid", _reportcreteria.purchaserid, "=");

                qb.SortField = "lrentry.id";
                //qb.Limit = 100;
                command = qb.GetCommand(conn);

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        LRReport oitem = new LRReport();
                        oitem.lrid = reader["lrid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["lrid"]);
                        oitem.lrno = reader["lrno"] == DBNull.Value ? "" : reader["lrno"].ToString();
                        oitem.lrentryno = reader["lrentryno"] == DBNull.Value ? "" : reader["lrentryno"].ToString();
                        oitem.supplier = reader["supplier"] == DBNull.Value ? "" : reader["supplier"].ToString();
                        oitem.purchasername = reader["purchasername"] == DBNull.Value ? "" : reader["purchasername"].ToString();
                        oitem.totalamount = reader["totalamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["totalamount"]);
                        oitem.lrdate = reader["lrdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["lrdate"]);
                        oitem.duedate = reader["duedate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["duedate"]);
                        oitem.entrydate = reader["lrentrydate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["lrentrydate"]);
                        oitem.noofbundels = reader["noofbundels"] == DBNull.Value ? 0 : Convert.ToInt32(reader["noofbundels"]);
                        oitem.noofpieces = reader["noofpieces"] == DBNull.Value ? 0 : Convert.ToInt32(reader["noofpieces"]);
                        oitem.noofboxes = reader["noofboxes"] == DBNull.Value ? 0 : Convert.ToInt32(reader["noofboxes"]);
                        oitem.modeofdelivery = reader["modeofdelivery"] == DBNull.Value ? 0 : Convert.ToInt32(reader["modeofdelivery"]);
                        oitem.retaillocationid = reader["retaillocationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["retaillocationid"]);
                        oitem.entrylocationid = reader["entrylocationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["entrylocationid"]);
                        oitem.statusname = reader["statusname"] == DBNull.Value ? "" : reader["statusname"].ToString();
                        oitem.lrsectionname = reader["lrsectionname"] == DBNull.Value ? "" : reader["lrsectionname"].ToString();
                        oitem.companyid = reader["lrcompanyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["lrcompanyid"]);
                        oitem.transportid = reader["transportid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["transportid"]);
                        oitem.transportgst = reader["transportgst"] == DBNull.Value ? "" : reader["transportgst"].ToString();
                        oitem.agentid = reader["agentid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["agentid"]);
                        oitem.bundlerackid = reader["bundlerackid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["bundlerackid"]);
                        oitem.suppliercompanycityid = reader["suppliercompanycityid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["suppliercompanycityid"]);
                        oitem.invoiceno = reader["invoiceno"] == DBNull.Value ? "" : reader["invoiceno"].ToString();
                        oitem.slipno = reader["slipno"] == DBNull.Value ? "" : reader["slipno"].ToString();
                        oitem.invoicedate = reader["invoicedate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["invoicedate"]);
                        oitem.hasfreightcharge = reader["hasfreightcharge"] == DBNull.Value ? false : Convert.ToBoolean(reader["hasfreightcharge"]);
                        oitem.hasloadingcharge = reader["hasloadingcharge"] == DBNull.Value ? false : Convert.ToBoolean(reader["hasloadingcharge"]);
                        oitem.freightcharge = reader["freightcharge"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["freightcharge"]);
                        oitem.loadingcharge = reader["loadingcharge"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["loadingcharge"]);
                        if (oitem.duedate.Year > 2000 && oitem.duedate.Date < DateTimeNow.Date)
                        {
                            System.TimeSpan diffResult = DateTimeNow.Subtract(oitem.duedate);
                            oitem.duedays = diffResult.Days;
                        }
                        else if (oitem.entrydate.Year > 2000 && oitem.entrydate.Date < DateTimeNow.Date)
                        {
                            System.TimeSpan diffResult = DateTimeNow.Subtract(oitem.entrydate);
                            oitem.duedays = diffResult.Days;
                        }
                        lst.Add(oitem);
                    }
                }

                if (_reportcreteria.showdetail)
                {
                    string datequery = @"select lrid,min(lrinvoiceitems.createdon) pricingstart,
                    max(lrinvoiceitems.modifiedon) pricingend,
                    min(lrinvoice.createdon) invoicestart,
                    max(lrinvoice.modifiedon) invoiceend,
                    min(stock.createdon) barcodestart,
                    max(stock.modifiedon) barcodeend,
                    min(lrinvoiceitems.pentrydate) productstartdate,
                    max(lrinvoiceitems.pentrydate) productenddate
                    from lrentry 
                    inner join lrinvoice on lrinvoice.lrid = lrentry.id
                    inner join lrinvoiceitems on lrinvoiceitems.lrinvoiceid = lrinvoice.id
                    left outer join stock on stock.invoiceitemid = lrinvoiceitems.id
                    where lrentry.id in ({0})
                    group by lrid";

                    foreach (var lrids in lst.ConvertAll(x => x.lrid).ChunkBy<long>(250))
                    {
                        using (var reader = ExecuteReader(String.Format(datequery,
                            String.Join(",", lrids.ToArray())), null))
                        {
                            while (reader.Read())
                            {
                                long lrid = reader["lrid"] != DBNull.Value ? Convert.ToInt64(reader["lrid"]) : 0;
                                DateTime pricingstart = reader["pricingstart"] != DBNull.Value ? Convert.ToDateTime(reader["pricingstart"]) : DateTime.MinValue;
                                DateTime pricingend = reader["pricingend"] != DBNull.Value ? Convert.ToDateTime(reader["pricingend"]) : DateTime.MinValue;
                                DateTime invoicestart = reader["invoicestart"] != DBNull.Value ? Convert.ToDateTime(reader["invoicestart"]) : DateTime.MinValue;
                                DateTime invoiceend = reader["invoiceend"] != DBNull.Value ? Convert.ToDateTime(reader["invoiceend"]) : DateTime.MinValue;
                                DateTime barcodestart = reader["barcodestart"] != DBNull.Value ? Convert.ToDateTime(reader["barcodestart"]) : DateTime.MinValue;
                                DateTime barcodeend = reader["barcodeend"] != DBNull.Value ? Convert.ToDateTime(reader["barcodeend"]) : DateTime.MinValue;
                                DateTime productstartdate = reader["productstartdate"] != DBNull.Value ? Convert.ToDateTime(reader["productstartdate"]) : DateTime.MinValue;
                                DateTime productenddate = reader["productenddate"] != DBNull.Value ? Convert.ToDateTime(reader["productenddate"]) : DateTime.MinValue;

                                var lr = lst.Find(x => x.lrid == lrid);
                                if (lr != null)
                                {
                                    lr.trdates = new LRReport.TransactionDate()
                                    {
                                        pricingstart = pricingstart,
                                        pricingend = pricingend,
                                        invoicestart = invoicestart,
                                        invoiceend = invoiceend,
                                        barcodestart = barcodestart,
                                        barcodeend = barcodeend,
                                        productstartdate = productstartdate,
                                        productenddate = productenddate
                                    };
                                }
                            }
                        }
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

        public List<POEntry> SearchInvoice(WarehouseReportCreteria _reportcreteria)
        {
            List<POEntry> lst = new List<POEntry>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                NpgsqlCommand command = null;
                QueryBuilder qb = new QueryBuilder();
                bool IsCreteriaAdded = false;

                System.Text.StringBuilder WHERE = new System.Text.StringBuilder();
                System.Text.StringBuilder WHERE2 = new System.Text.StringBuilder();


                if (_reportcreteria.multicompany && _reportcreteria.companyid > 0)
                    WHERE.AppendFormat(" and {0}{1}{2} ", "lrinvoice.companyid", "=", _reportcreteria.companyid);

                if (_reportcreteria.fromdate != DateTime.MinValue && _reportcreteria.todate != DateTime.MinValue)
                {
                    WHERE.AppendFormat(" and {0}{1}'{2}' ", "lrentry.lrentrydate", ">=", _reportcreteria.fromdate.Date.ToString("yyyy-MM-dd"));
                    WHERE.AppendFormat(" and {0}{1}'{2}' ", "lrentry.lrentrydate", "<", _reportcreteria.todate.Date.AddDays(1).ToString("yyyy-MM-dd"));
                    IsCreteriaAdded = true;
                }
                if (_reportcreteria.fromlrdate != DateTime.MinValue && _reportcreteria.tolrdate != DateTime.MinValue)
                {
                    WHERE.AppendFormat(" and {0}{1}'{2}' ", "lrinvoice.invoicedate", ">=", _reportcreteria.fromlrdate.Date.ToString("yyyy-MM-dd"));
                    WHERE.AppendFormat(" and {0}{1}'{2}' ", "lrinvoice.invoicedate", "<", _reportcreteria.tolrdate.Date.AddDays(1).ToString("yyyy-MM-dd"));
                    IsCreteriaAdded = true;
                }
                if (_reportcreteria.frompentrydate != DateTime.MinValue && _reportcreteria.topentrydate != DateTime.MinValue)
                {
                    WHERE2.AppendFormat(" and {0}{1}'{2}' ", "lrinvoice.entrydate", ">=", _reportcreteria.frompentrydate.Date.ToString("yyyy-MM-dd"));
                    WHERE2.AppendFormat(" and {0}{1}'{2}' ", "lrinvoice.entrydate", "<", _reportcreteria.topentrydate.Date.AddDays(1).ToString("yyyy-MM-dd"));
                    IsCreteriaAdded = true;
                }

                if (_reportcreteria.locationid > 0)
                    WHERE.AppendFormat(" and {0}{1}{2} ", "lrentry.entrylocationid", "=", _reportcreteria.locationid);

                if (_reportcreteria.purchaserid > 0)
                {
                    WHERE.AppendFormat(" and {0}{1}{2} ", "lrentry.purchasemanagerid", "=", _reportcreteria.purchaserid);
                    IsCreteriaAdded = true;
                }
                if (_reportcreteria.supplierid > 0)
                {
                    WHERE.AppendFormat(" and {0}{1}{2} ", "lrentry.supplierid", "=", _reportcreteria.supplierid);
                    IsCreteriaAdded = true;
                }
                if (!string.IsNullOrEmpty(_reportcreteria.invoiceno))
                {
                    WHERE.AppendFormat(" and {0}{1}'{2}' ", "lrinvoice.invoiceno", "=", _reportcreteria.invoiceno);
                    IsCreteriaAdded = true;
                }
                if (!string.IsNullOrEmpty(_reportcreteria.lrentryno))
                {
                    WHERE.AppendFormat(" and {0}{1}'{2}' ", "lrentry.lrentryno", "=", _reportcreteria.lrentryno);
                    IsCreteriaAdded = true;
                }
                if (!string.IsNullOrEmpty(_reportcreteria.lrno))
                {
                    WHERE.AppendFormat(" and {0}{1}'{2}' ", "lrentry.lrno", "=", _reportcreteria.lrno);
                    IsCreteriaAdded = true;
                }

                qb.CurrentQuery = string.Format(SQL_LRInvoiceSummary, WHERE.ToString(), WHERE2.ToString());

                if (!IsCreteriaAdded)
                    throw new Exception("Please provide minimum search creteria.");

                command = qb.GetCommand(conn);
                command.CommandTimeout = 10000;
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        POEntry oitem = new POEntry();
                        oitem.invoiceid = reader["invoiceid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["invoiceid"]);
                        oitem.company = reader["company"] == DBNull.Value ? "" : reader["company"].ToString();
                        oitem.supplier = reader["supplier"] == DBNull.Value ? "" : reader["supplier"].ToString();
                        oitem.gstno = reader["tinanddate"] == DBNull.Value ? "" : reader["tinanddate"].ToString();
                        oitem.purchaser = reader["employeename"] == DBNull.Value ? "" : reader["employeename"].ToString();
                        oitem.bankname = reader["bankaccountname"] == DBNull.Value ? "" : reader["bankaccountname"].ToString();
                        oitem.accountno = reader["bankaccountno"] == DBNull.Value ? "" : reader["bankaccountno"].ToString();
                        oitem.ifsccode = reader["bankifci"] == DBNull.Value ? "" : reader["bankifci"].ToString();
                        oitem.cityid = reader["cityid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["cityid"]);
                        oitem.isinterstatesale = reader["isinterstatesale"] == DBNull.Value ? false : Convert.ToBoolean(reader["isinterstatesale"]);
                        oitem.retaillocationid = reader["retaillocationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["retaillocationid"]);
                        oitem.entrylocationid = reader["entrylocationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["entrylocationid"]);
                        oitem.lrentryno = reader["lrentryno"] == DBNull.Value ? "" : reader["lrentryno"].ToString();
                        oitem.lrentrydate = reader["entrydate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["entrydate"]);
                        oitem.invoiceno = reader["invoiceno"] == DBNull.Value ? "" : reader["invoiceno"].ToString();
                        oitem.invoicedate = reader["invoicedate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["invoicedate"]);
                        oitem.duedate = reader["duedate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["duedate"]);

                        oitem.invoiceamount = reader["invoiceamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["invoiceamount"]);
                        oitem.taxamount = reader["taxamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["taxamount"]);
                        oitem.taxableamount = reader["taxableamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["taxableamount"]);
                        oitem.noofbundle = reader["noofbundle"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["noofbundle"]);

                        oitem.qty = reader["qty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["qty"]);
                        oitem.returnqty = reader["returnqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["returnqty"]);
                        oitem.marginpercentage = reader["marginpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["marginpercentage"]);

                        oitem.dealerprice = reader["dealerprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["dealerprice"]);
                        oitem.dealercost = reader["dealercost"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["dealercost"]);

                        if (oitem.dealerprice > 0 && oitem.dealercost > 0)
                        {
                            oitem.dealermarginpercentage = Math.Round(oitem.dealercost / oitem.dealerprice * 100, 2);
                        }


                        oitem.returnfright = reader["returnfright"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["returnfright"]);
                        oitem.closingqty = reader["closingqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["closingqty"]);
                        oitem.closingqty += reader["whstock"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["whstock"]);

                        oitem.closingstockvalue = reader["closingvalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["closingvalue"]);
                        oitem.closingstockvalue += reader["whstockvalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["whstockvalue"]);

                        oitem.returnamount = Math.Round(reader["returnvalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["returnvalue"]), 0);
                        oitem.igst = reader["soldqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["soldqty"]);
                        oitem.mrp = reader["stockmrp"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["stockmrp"]);
                        oitem.stocksalevalue = reader["stocksalevalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["stocksalevalue"]);
                        oitem.netamount = oitem.invoiceamount - oitem.returnamount;

                        if (oitem.lrentrydate != DateTime.MinValue)
                            oitem.stockage = (DateTimeNow - oitem.lrentrydate).Days;
                        if ((oitem.qty - oitem.returnqty) > 0 && oitem.igst > 0)
                            oitem.soldpercentage = Math.Round((oitem.igst / (oitem.qty - oitem.returnqty) * 100), 0);

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
            if (_reportcreteria.groupsupplier)
            {
                return lst.GroupBy(x => x.supplier).ToList().ConvertAll<POEntry>(e => new POEntry()
                {
                    supplier = e.Key,
                    gstno = e.First().gstno,
                    cityid = e.First().cityid,
                    bankname = e.First().bankname,
                    accountno = e.First().accountno,
                    ifsccode = e.First().ifsccode,
                    invoiceamount = e.Sum(s => s.invoiceamount),
                    taxamount = e.Sum(s => s.taxamount),
                    taxableamount = e.Sum(s => s.taxableamount),
                    qty = e.Sum(s => s.qty),
                    returnqty = e.Sum(s => s.returnqty),
                    closingqty = e.Sum(s => s.closingqty),
                    closingstockvalue = e.Sum(s => s.closingstockvalue),
                    returnamount = e.Sum(s => s.returnamount),
                    mrp = e.Sum(s => s.mrp),
                    stocksalevalue = e.Sum(s => s.stocksalevalue),
                    igst = e.Sum(s => s.igst),
                    netamount = e.Sum(s => s.netamount),
                    percentage = e.Count(),
                    marginpercentage = e.Average(s => s.marginpercentage)
                });
            }
            return lst;
        }

        public SalesStockReport SearchSalesStockSummary(SalesStockCreteria salesStockCreteria)
        {
            string query = "";
            SalesStockReport salesStockReport = new SalesStockReport();
            salesStockReport.items = new List<SalesStockReport.Item>();
            List<string> fields = new List<string>();
            List<string> criterialist = new List<string>();
            if (salesStockCreteria.showsupplier)
                fields.Add("supplier.name");
            if (salesStockCreteria.showproduct)
                fields.Add("products.name");
            if (salesStockCreteria.showdesign)
                fields.Add("stock.designid");
            if (salesStockCreteria.showbrand)
                fields.Add("brand.name");
            if (salesStockCreteria.showlocation)
                fields.Add("location.name");

            if (salesStockCreteria.companyid > 0)
                criterialist.Add("stock.companyid =@companyid");
            if (salesStockCreteria.productid > 0)
                criterialist.Add("stock.productid =@productid");
            if (salesStockCreteria.supplierid > 0)
                criterialist.Add("lrentry.supplierid =@supplierid");
            if (salesStockCreteria.sectionid > 0)
                criterialist.Add("products.salescounterid =@salescounterid");



            if (salesStockCreteria.locationid == 0)
            {
                string whquery = @"select sum( (stock.qty - (stock.transfered+stock.journalqty+stock.returnqty+stock.damagedqty)) * stock.ibuyingprice::numeric) as ivalue,
                sum(stock.qty-(stock.transfered+stock.journalqty+stock.damagedqty+stock.returnqty)) qty {0}
                from stock 
                left outer join lrinvoiceitems on lrinvoiceitems.id = stock.invoiceitemid 
                left outer join lrinvoice on lrinvoice.id = lrinvoiceitems.lrinvoiceid  
                left outer join lrentry on lrentry.id = lrinvoice.lrid
                left outer join products on products.id = stock.productid 
                left outer join supplier on supplier.id = lrentry.supplierid
                left outer join items on items.id = stock.itemid 
                left outer join brand on brand.id = items.brandid
                left outer join referencelist location on location.id = stock.locationid
                where stock.isactive=true and stock.hasstock=true {2}
                {1}";

                if (fields.Count > 0)
                {
                    whquery = string.Format(whquery, "," + string.Join(",", fields).
                        Replace("supplier.name", "supplier.name suppliername").
                        Replace("brand.name", "brand.name brandname").
                        Replace("location.name", "location.name locationname"),
                        " group by " + string.Join(",", fields),
                        criterialist.Count == 0 ? "" : (" and " + string.Join(" and ", criterialist)));
                }
                else
                {
                    whquery = string.Format(whquery, "", "",
                        criterialist.Count == 0 ? "" : (" and " + string.Join(" and ", criterialist)));
                }

                string retquery = @"select sum( (salablegoods.iqty-(salablegoods.oqty+salablegoods.jqty+salablegoods.dqty)) * stock.ibuyingprice::numeric) as ivalue,  
                sum(salablegoods.iqty-(salablegoods.oqty+salablegoods.jqty+salablegoods.dqty)) qty {0}
                from salablegoods inner join stock on  stock.id = salablegoods.stockid
                left outer join lrinvoiceitems on lrinvoiceitems.id = stock.invoiceitemid 
                left outer join lrinvoice on lrinvoice.id = lrinvoiceitems.lrinvoiceid  
                left outer join lrentry on lrentry.id = lrinvoice.lrid
                left outer join products on products.id = stock.productid 
                left outer join supplier on supplier.id = lrentry.supplierid
                left outer join items on items.id = stock.itemid 
                left outer join brand on brand.id = items.brandid
                left outer join referencelist location on location.id = salablegoods.locationid
                where salablegoods.isactive=true and stock.isactive=true 
                and salablegoods.hasstock=true {2}
                {1}";

                if (salesStockCreteria.locationid > 0)
                    criterialist.Add(" salablegoods.locationid=@locationid ");
                if (fields.Count > 0)
                {
                    retquery = string.Format(retquery, "," + string.Join(",", fields).
                        Replace("supplier.name", "supplier.name suppliername").
                        Replace("brand.name", "brand.name brandname").
                        Replace("location.name", "location.name locationname"),
                        " group by " + string.Join(",", fields),
                        criterialist.Count == 0 ? "" : (" and " + string.Join(" and ", criterialist)));
                }
                else
                {
                    retquery = string.Format(whquery, "", "",
                        criterialist.Count == 0 ? "" : (" and " + string.Join(" and ", criterialist)));
                }
                if (salesStockCreteria.locationid > 0)
                    criterialist.RemoveAt(criterialist.Count - 1);
                query = string.Format("with allstock as ({0} union all {1}) select * from allstock", whquery, retquery);
            }
            else
            {
                string retquery = @"select sum( (salablegoods.iqty-(salablegoods.oqty+salablegoods.jqty+salablegoods.dqty)) * stock.ibuyingprice::numeric) as ivalue,  
                sum(salablegoods.iqty-(salablegoods.oqty+salablegoods.jqty+salablegoods.dqty)) qty {0}
                from salablegoods inner join stock on  stock.id = salablegoods.stockid
                left outer join lrinvoiceitems on lrinvoiceitems.id = stock.invoiceitemid 
                left outer join lrinvoice on lrinvoice.id = lrinvoiceitems.lrinvoiceid  
                left outer join lrentry on lrentry.id = lrinvoice.lrid
                left outer join products on products.id = stock.productid 
                left outer join supplier on supplier.id = lrentry.supplierid
                left outer join items on items.id = stock.itemid 
                left outer join brand on brand.id = items.brandid
                where salablegoods.isactive=true and stock.isactive=true 
                and salablegoods.hasstock=true {2}
                {1}";

                if (salesStockCreteria.locationid > 0)
                    criterialist.Add(" salablegoods.locationid=@locationid ");
                if (fields.Count > 0)
                {
                    retquery = string.Format(retquery, "," + string.Join(",", fields).
                        Replace("supplier.name", "supplier.name suppliername").
                        Replace("brand.name", "brand.name brandname").
                        Replace("location.name", "location.name locationname"),
                        " group by " + string.Join(",", fields),
                        criterialist.Count == 0 ? "" : (" and " + string.Join(" and ", criterialist)));
                }

                if (salesStockCreteria.locationid > 0)
                    criterialist.RemoveAt(criterialist.Count - 1);
                query = retquery;
            }

            if (InfyPOS.Services.ConfigManager.Instance(this.Context.OrganizationID).Config.analyser_stock_withzerostock)
            {
                query = query.Replace("and (salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty))>0", "");
                query = query.Replace("and stock.qty - (stock.transfered+stock.returnqty+stock.damagedqty+stock.journalqty)>0", "");
            }

            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                NpgsqlCommand command = new NpgsqlCommand(query, conn);
                command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = salesStockCreteria.locationid;
                command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = salesStockCreteria.companyid;
                command.Parameters.Add("productid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = salesStockCreteria.productid;
                command.Parameters.Add("supplierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = salesStockCreteria.supplierid;
                command.Parameters.Add("salescounterid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = salesStockCreteria.sectionid;
                command.CommandTimeout = 10000;
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {

                        SalesStockReport.Item item = new SalesStockReport.Item();
                        item.stockqty = reader["qty"] != DBNull.Value ? Convert.ToDecimal(reader["qty"]) : 0;
                        item.stockvalue = reader["ivalue"] != DBNull.Value ? Convert.ToDecimal(reader["ivalue"]) : 0;
                        if (item.stockvalue > 0 && item.stockqty > 0)
                            item.cost = item.stockvalue / item.stockqty;
                        if (salesStockCreteria.showdesign)
                            item.design = reader["designid"] != DBNull.Value ? Convert.ToString(reader["designid"]) : "";
                        if (salesStockCreteria.showproduct)
                            item.productname = reader["name"] != DBNull.Value ? Convert.ToString(reader["name"]) : "";
                        if (salesStockCreteria.showsupplier)
                            item.suppliername = reader["suppliername"] != DBNull.Value ? Convert.ToString(reader["suppliername"]) : "";
                        if (salesStockCreteria.showbrand)
                            item.brandname = reader["brandname"] != DBNull.Value ? Convert.ToString(reader["brandname"]) : "";
                        if (salesStockCreteria.showbrand)
                            item.locationname = reader["locationname"] != DBNull.Value ? Convert.ToString(reader["locationname"]) : "";
                        string key = item.key;
                        if (salesStockReport.items.Exists(e => e.key == key))
                        {
                            salesStockReport.items.Find(e => e.key == key).stockqty += item.stockqty;
                            salesStockReport.items.Find(e => e.key == key).stockvalue += item.stockvalue;
                        }
                        else
                        {
                            salesStockReport.items.Add(item);
                        }
                    }
                }

                string salesquery = @"Select sum(billitems.qty) qty, sum(stock.ibuyingprice::numeric*billitems.qty) productcost, sum(billitems.receivable) ivalue {0}
from bill 
inner join billitems on billitems.billid = bill.id and billitems.isactive=true
inner join salablegoods on salablegoods.id = billitems.salablegoodsid
inner join stock on stock.id = salablegoods.stockid
inner join products on products.id = stock.productid 
left outer join lrinvoiceitems on lrinvoiceitems.id = stock.invoiceitemid
left outer join lrinvoice on lrinvoice.id = lrinvoiceitems.lrinvoiceid
left outer join lrentry on lrentry.id = lrinvoice.lrid 
left outer join supplier on supplier.id = lrentry.supplierid
where bill.isactive=true and bill.billtype!=9 and billdate>=@fromdate and billdate<@todate {2}
{1}";
                if (salesStockCreteria.locationid > 0)
                    criterialist.Add(" bill.locationid=@locationid ");

                if (salesStockCreteria.companyid > 0)
                    criterialist.Add(" bill.companyid=@companyid ");

                if (fields.Count > 0)
                {
                    salesquery = string.Format(salesquery, "," + string.Join(",", fields).Replace("supplier.name", "supplier.name suppliername"),
                        " group by " + string.Join(",", fields),
                        criterialist.Count == 0 ? "" : (" and " + string.Join(" and ", criterialist)));
                }
                else
                {
                    salesquery = string.Format(salesquery, "", "",
                        criterialist.Count == 0 ? "" : (" and " + string.Join(" and ", criterialist)));
                }


                command = new NpgsqlCommand(salesquery, conn);
                command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = salesStockCreteria.locationid;
                command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = salesStockCreteria.companyid;
                command.Parameters.Add("productid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = salesStockCreteria.productid;
                command.Parameters.Add("supplierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = salesStockCreteria.supplierid;
                command.Parameters.Add("fromdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = salesStockCreteria.fromdate.Date;
                command.Parameters.Add("todate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = salesStockCreteria.todate.AddDays(1).Date;
                command.Parameters.Add("salescounterid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = salesStockCreteria.sectionid;
                command.CommandTimeout = 10000;
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        SalesStockReport.Item item = new SalesStockReport.Item();
                        item.salesqty = reader["qty"] != DBNull.Value ? Convert.ToDecimal(reader["qty"]) : 0;
                        item.salesvalue = reader["ivalue"] != DBNull.Value ? Convert.ToDecimal(reader["ivalue"]) : 0;
                        if (item.salesvalue != 0 && item.salesqty != 0)
                            item.salerate = Math.Round(item.salesvalue / item.salesqty, 0);
                        item.cost = reader["productcost"] != DBNull.Value ? Convert.ToDecimal(reader["productcost"]) : 0;
                        if (salesStockCreteria.showdesign)
                            item.design = reader["designid"] != DBNull.Value ? Convert.ToString(reader["designid"]) : "";
                        if (salesStockCreteria.showproduct)
                            item.productname = reader["name"] != DBNull.Value ? Convert.ToString(reader["name"]) : "";
                        if (salesStockCreteria.showsupplier)
                            item.suppliername = reader["suppliername"] != DBNull.Value ? Convert.ToString(reader["suppliername"]) : "";
                        string key = item.key;
                        if (salesStockReport.items.Exists(e => e.key == key))
                        {
                            salesStockReport.items.Find(e => e.key == key).salesqty = item.salesqty;
                            salesStockReport.items.Find(e => e.key == key).salesvalue = item.salesvalue;
                        }
                        else
                        {
                            salesStockReport.items.Add(item);
                        }

                    }
                }


                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null)
                    conn.Close();
            }
            salesStockReport.salesqty = salesStockReport.items.Sum(e => e.salesqty);
            salesStockReport.salesvalue = salesStockReport.items.Sum(e => e.salesvalue);
            salesStockReport.stockqty = salesStockReport.items.Sum(e => e.stockqty);
            salesStockReport.stockvalue = salesStockReport.items.Sum(e => e.stockvalue);
            return salesStockReport;
        }

        public List<POEntry> SearchPOList(WarehouseReportCreteria _reportcreteria)
        {
            List<POEntry> lst = new List<POEntry>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                NpgsqlCommand command = null;
                bool isCreteriaAdded = false;

                QueryBuilder qb = new QueryBuilder();
                qb.CurrentQuery = string.Format(SQL_Purchase, "", "");

                //qb.AddInParamer("lrinvoicetax.type", "0, 1,2");

                if (_reportcreteria.multicompany && _reportcreteria.companyid > 0)
                    qb.AddParameter("lrinvoice.companyid", _reportcreteria.companyid, "=");

                if (_reportcreteria.fromdate != DateTime.MinValue && _reportcreteria.todate != DateTime.MinValue)
                {
                    qb.AddParameter("lrinvoice.entrydate", "datefrom", _reportcreteria.fromdate.Date, ">=");
                    qb.AddParameter("lrinvoice.entrydate", "dateto", _reportcreteria.todate.Date.AddDays(1), "<");
                    isCreteriaAdded = true;
                }
                if (_reportcreteria.fromlrdate != DateTime.MinValue && _reportcreteria.tolrdate != DateTime.MinValue)
                {
                    qb.AddParameter("lrinvoice.invoicedate", "lrdatefrom", _reportcreteria.fromlrdate.Date, ">=");
                    qb.AddParameter("lrinvoice.invoicedate", "lrdateto", _reportcreteria.tolrdate.Date.AddDays(1), "<");
                    isCreteriaAdded = true;
                }
                if (_reportcreteria.locationid > 0)
                    qb.AddParameter("lrentry.entrylocationid", _reportcreteria.locationid, "=");

                if (_reportcreteria.supplierid > 0)
                {
                    qb.AddParameter("lrentry.supplierid", _reportcreteria.supplierid, "=");
                    isCreteriaAdded = true;
                }

                if (_reportcreteria.agentid > 0)
                {
                    qb.AddParameter("lrentry.agentid", _reportcreteria.agentid, "=");
                    isCreteriaAdded = true;
                }
                if (_reportcreteria.suppliercityid > 0)
                {
                    qb.AddParameter("suppliercompany.cityid", _reportcreteria.suppliercityid, "=");
                    isCreteriaAdded = true;
                }
                if (_reportcreteria.receivinglocationid > 0)
                {
                    qb.AddParameter("lrentry.receivinglocationid", _reportcreteria.receivinglocationid, "=");
                    isCreteriaAdded = true;
                }
                if (_reportcreteria.onlymsmeno)
                {
                    qb.Add2Query("suppliercompany.msmeno is not null and suppliercompany.msmeno != ''");
                    isCreteriaAdded = true;
                }
                qb.CurrentQuery += @" group by lrentry.id,lrentry.duedate,lrinvoice.id, company.name, suppliercompany.name, suppliercompany.cityid, suppliercompany.tinanddate, lrentryno, 
                                            lrinvoice.entrydate, lrinvoice.invoiceno, lrinvoice.invoicedate, lrinvoicetax.percentage, lrentry.agentid,
                                    lrinvoice.isinterstatesale, lrinvoice.otheramount, lrinvoicetax.type,lrentry.retaillocationid,lrentry.entrylocationid,lrinvoice.totalamount,lrentry.receivinglocationid,
                                    lrinvoice.discount, lrentry.lrno,msmeno,tdsamount,tdspercentage
                                            order by lrentry.id ";
                command = qb.GetCommand(conn);

                if (!isCreteriaAdded)
                    throw new Exception("Please provide minimum search creteria.");

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        POEntry oitem = new POEntry();
                        oitem.invoiceid = reader["invoiceid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["invoiceid"]);
                        oitem.agentid = reader["agentid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["agentid"]);
                        oitem.company = reader["company"] == DBNull.Value ? "" : reader["company"].ToString();
                        oitem.supplier = reader["supplier"] == DBNull.Value ? "" : reader["supplier"].ToString();
                        oitem.gstno = reader["tinanddate"] == DBNull.Value ? "" : reader["tinanddate"].ToString();
                        oitem.msmeno = reader["msmeno"] == DBNull.Value ? "" : reader["msmeno"].ToString();
                        oitem.retaillocationid = reader["retaillocationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["retaillocationid"]);
                        oitem.entrylocationid = reader["entrylocationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["entrylocationid"]);
                        oitem.receivinglocationid = reader["receivinglocationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["receivinglocationid"]);
                        oitem.cityid = reader["cityid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["cityid"]);
                        oitem.isinterstatesale = reader["isinterstatesale"] == DBNull.Value ? false : Convert.ToBoolean(reader["isinterstatesale"]);
                        oitem.lrentryno = reader["lrentryno"] == DBNull.Value ? "" : reader["lrentryno"].ToString();
                        oitem.lrno = reader["lrno"] == DBNull.Value ? "" : reader["lrno"].ToString();
                        oitem.lrentrydate = reader["entrydate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["entrydate"]);
                        oitem.invoiceno = reader["invoiceno"] == DBNull.Value ? "" : reader["invoiceno"].ToString();
                        oitem.invoicedate = reader["invoicedate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["invoicedate"]);
                        oitem.invoiceamount = reader["invoiceamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["invoiceamount"]);
                        oitem.invoicetotalamount = reader["totalamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["totalamount"]);
                        oitem.percentage = reader["percentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["percentage"]);
                        oitem.otheramount = reader["otheramount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["otheramount"]);
                        oitem.taxamount = reader["taxamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["taxamount"]);
                        oitem.taxableamount = reader["taxableamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["taxableamount"]);
                        oitem.forwardcharge = reader["forwardcharge"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["forwardcharge"]);
                        oitem.discount = reader["discountbytax"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discountbytax"]);
                        oitem.invoiceamount = (oitem.invoiceamount + oitem.discount - oitem.taxamount);
                        oitem.tcsamount = reader["tcsamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["tcsamount"]);
                        oitem.tdspercentage = reader["tdspercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["tdspercentage"]);
                        oitem.tdsamount = reader["tdsamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["tdsamount"]);


                        if (oitem.tdspercentage > 0 && oitem.tdsamount > 0 && oitem.taxableamount > 0)
                        {
                            oitem.tdsamount = oitem.taxableamount * (oitem.tdspercentage / 100);
                        }

                        if (oitem.isinterstatesale)
                        {
                            oitem.igst = oitem.taxamount;
                            oitem.igstpercentage = oitem.percentage;
                        }
                        else
                        {
                            oitem.cgst = Math.Round((oitem.taxamount / 2), 2);
                            oitem.sgst = Math.Round((oitem.taxamount / 2), 2);
                            oitem.cgstpercentage = Math.Round((oitem.percentage / 2), 2);
                            oitem.sgstpercentage = Math.Round((oitem.percentage / 2), 2);
                        }
                        lst.Add(oitem);
                    }
                }
                conn.Close();

                // Merging Discount value if entered in different line in invoice entry 
                foreach (var tcsitem in lst.FindAll(x => x.tcsamount > 0).ToList())
                {
                    lst.Remove(tcsitem);
                    var invoiceitems = lst.Find(x => x.invoiceid == tcsitem.invoiceid);
                    invoiceitems.tcsamount = tcsitem.tcsamount;
                    invoiceitems.tcspercentage = tcsitem.percentage;
                }
                List<long> invoiceids = lst.GroupBy(g => new { g.invoiceid, g.percentage }).Where(grp => grp.Count() > 1).Select(grp => grp.Key.invoiceid).ToList();
                if (invoiceids != null && invoiceids.Count > 0)
                {
                    foreach (long id in invoiceids)
                    {
                        List<POEntry> _out = lst.Where(e => e.invoiceid == id && e.taxableamount == 0 && e.percentage == 0).ToList();

                        if (_out != null && _out.Count > 0)
                        {
                            if (lst.Exists(e => e.invoiceid == id && e.taxableamount > 0 && e.percentage > 0))
                            {
                                lst.RemoveAll(e => e.invoiceid == id && e.taxableamount == 0 && e.percentage == 0);

                                POEntry oitem = lst.First(e => e.invoiceid == id && e.taxableamount > 0 && e.percentage > 0);
                                foreach (POEntry _item in _out)
                                {
                                    oitem.discount += _item.discount;
                                    oitem.taxableamount += _item.taxableamount;
                                    oitem.invoiceamount += _item.invoiceamount;
                                }
                            }
                        }
                    }
                }
                foreach (POEntry item in lst.Where(e => e.taxableamount == 0 && e.taxamount == 0
                && e.percentage == 0 && e.invoiceamount > 0))
                {
                    item.taxableamount = item.invoiceamount;
                }
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _reportcreteria);
            }
            return lst;
        }

        public List<POEntry> SearchWHEntry(WarehouseReportCreteria _reportcreteria)
        {
            string SQL = @"with itementry as
(
	select lrinvoice.id id, sum(buyingqty) entryqty, sum(amount) entrytaxable, 
	sum(purchasetax) entrytax, sum(amount + purchasetax) entrytotalamount
    from lrinvoice inner join lrinvoiceitems on lrinvoiceitems.lrinvoiceid = lrinvoice.id 
	where lrinvoice.isactive=true and lrinvoiceitems.isactive = true
	and lrinvoice.entrydate>=@fromdate And lrinvoice.entrydate<@todate 
	group by lrinvoice.id
)

select 'PURCHASE' as entrytype, lrinvoice.id id, lrentry.retaillocationid locationid, 
suppliercompany.name supplier, suppliercompany.cityid, suppliercompany.tinanddate, 
lrentryno entryno, lrinvoice.entrydate entrydate, lrinvoice.invoiceno, lrinvoice.invoicedate, 
lrinvoice.isinterstatesale, lrinvoice.noofpieces::numeric qty, lrinvoice.grossamount amount, taxamount tax, 
lrinvoice.discount, lrinvoice.totalamount total, lrinvoice.otheramount rounding,
itementry.entryqty, itementry.entrytaxable, itementry.entrytax, itementry.entrytotalamount
from lrentry inner join lrinvoice on lrentry.id=lrinvoice.lrid 
inner join suppliercompany on suppliercompany.id=lrinvoice.suppliercompanyid 
left outer join itementry on itementry.id = lrinvoice.id 
Where lrentry.isactive = true and lrinvoice.isactive = true 
and lrinvoice.entrydate>=@fromdate And lrinvoice.entrydate<@todate 
UNION
select 'RETURN' as entrytype, lrinvoicereturn.id, lrinvoicereturn.locationid, 
supplier.name supplier, suppliercompany.cityid, suppliercompany.tinanddate, 
returnno entryno, returndate entrydate, 'invoiceno' as invoiceno, returndate as invoicedate, 
lrinvoicereturn.isinterstatesale, lrinvoicereturn.qty::numeric, lrinvoicereturn.amount,lrinvoicereturn.tax,
lrinvoicereturn.discount,lrinvoicereturn.total,lrinvoicereturn.rounding,
0 as entryqty, 0 as entrytaxable, 0 as entrytax, 0 as entrytotalamount
from lrinvoicereturn 
inner join supplier on supplier.id=lrinvoicereturn.supplierid
inner join suppliercompany on suppliercompany.supplierid=supplier.id
Where lrinvoicereturn.isactive=True  and lrinvoicereturn.returntype<14
And lrinvoicereturn.returndate>=@fromdate And lrinvoicereturn.returndate<@todate ";

            List<POEntry> lst = new List<POEntry>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                NpgsqlCommand command = null;
                command = GetCommand(SQL, conn);
                command.Parameters.Add("fromdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = _reportcreteria.fromdate.Date;
                command.Parameters.Add("todate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = _reportcreteria.todate.AddDays(1).Date;

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        POEntry oitem = new POEntry();
                        oitem.invoiceid = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        oitem.company = reader["entrytype"] == DBNull.Value ? "" : reader["entrytype"].ToString();
                        oitem.retaillocationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                        oitem.supplier = reader["supplier"] == DBNull.Value ? "" : reader["supplier"].ToString();
                        oitem.cityid = reader["cityid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["cityid"]);
                        oitem.gstno = reader["tinanddate"] == DBNull.Value ? "" : reader["tinanddate"].ToString();
                        oitem.isinterstatesale = reader["isinterstatesale"] == DBNull.Value ? false : Convert.ToBoolean(reader["isinterstatesale"]);
                        oitem.lrentryno = reader["entryno"] == DBNull.Value ? "" : reader["entryno"].ToString();
                        oitem.lrentrydate = reader["entrydate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["entrydate"]);

                        oitem.qty = reader["qty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["qty"]);
                        oitem.invoiceamount = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"]);
                        oitem.taxamount = reader["tax"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["tax"]);
                        oitem.invoicetotalamount = reader["total"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["total"]);
                        oitem.percentage = reader["discount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discount"]);
                        oitem.otheramount = reader["rounding"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["rounding"]);
                        if (oitem.company == "PURCHASE")
                        {
                            oitem.invoiceno = reader["invoiceno"] == DBNull.Value ? "" : reader["invoiceno"].ToString();
                            oitem.invoicedate = reader["invoicedate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["invoicedate"]);
                            oitem.invoiceamount = oitem.invoiceamount - oitem.taxamount + oitem.percentage;

                            oitem.entryqty = reader["entryqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["entryqty"]);
                            oitem.entrytaxable = reader["entrytaxable"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["entrytaxable"]);
                            oitem.entrytax = reader["entrytax"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["entrytax"]);
                            oitem.entrytotalamount = reader["entrytotalamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["entrytotalamount"]);
                            if (oitem.entrytotalamount > 0)
                                oitem.difference = Math.Round(oitem.invoicetotalamount - oitem.entrytotalamount, 2);
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
            return lst;

        }

        public List<POInvoiceSummaryItem> SearchInoicePOSummary(WarehouseReportCreteria _reportcreteria)
        {
            string query = @"
                with affecedinvoices
                as
                (
	                select sum(lrinvoiceitems.buyingqty) as purchasedqty,lrinvoice.id invoiceid,
	                lrinvoice.entrydate,lrinvoice.invoiceno,lrinvoice.totalamount,lrinvoice.invoicedate,
	                lrentry.lrentryno,lrentry.lrno,lrentry.supplierid
	                from lrinvoice
	                inner join lrinvoiceitems on lrinvoiceitems.lrinvoiceid = lrinvoice.id
	                inner join lrentry on lrentry.id = lrid
	                where lrinvoice.isactive=true and lrinvoiceitems.isactive=true
                    {0}
	                group by invoiceid,
	                lrinvoice.entrydate,lrinvoice.invoiceno,lrinvoice.totalamount,lrinvoice.invoicedate,
	                lrentry.lrentryno,lrentry.lrno,lrentry.supplierid
	
                ),affecedpoitems
                as
                (
                 select 
                 purchaseorder.pono,
                 cast(schemelist->>'purchaseorderitemsid' as bigint) purchaseorderitemid,
                 purchaseorder.id poid,
                 lrinvoice.id invoiceid,
                 (cast(schemelist->>'aqty' as numeric)) billedqty
                from lrinvoiceitems
                inner join lrinvoice on lrinvoice.id = lrinvoiceid
                inner join lrentry on lrentry.id = lrid
                LEFT OUTER JOIN LATERAL json_array_elements (case when lrinvoiceitems.poinfo is null 
                or lrinvoiceitems.poinfo::text ='null' then '[]'::json else lrinvoiceitems.poinfo::json end) schemelist ON true
                left outer join purchaseorderitems on purchaseorderitems.id = cast(schemelist->>'purchaseorderitemsid' as bigint)
                left outer join purchaseorder on purchaseorder.id = purchaseorderitems.purchaseorderid
	                where lrinvoice.isactive=true and lrinvoiceitems.isactive=true
	                {0}

                ),polist as
                (
	                select purchaseorderid,sum(affecedpoitems.billedqty) invoicebilled,invoiceid from purchaseorderitems 
	                left outer join affecedpoitems on affecedpoitems.purchaseorderitemid = purchaseorderitems.id
	                where isactive=true 
	                group by purchaseorderid,invoiceid
                ),actualpurchaseorder
                as
                (
                select sum(oqty) orderedpoqty,sum(oqty-(rqty+cqty)) balancepoqty,
	                purchaseorder.id poid,purchaseorder.pono,purchaseorder.podate from purchaseorder
                inner join purchaseorderitems on purchaseorderitems.purchaseorderid = purchaseorder.id
                where purchaseorder.id in (select poid from affecedpoitems)
                group by purchaseorder.id,purchaseorder.pono,purchaseorder.podate
                )


                select actualpurchaseorder.*,
                affecedinvoices.*,polist.*,
                case when orderedpoqty != invoicebilled then (orderedpoqty - (invoicebilled+balancepoqty)) else 0 end
                as otherinvoicebilledqty
                from affecedinvoices
                left outer join polist on polist.invoiceid = affecedinvoices.invoiceid
                left outer join actualpurchaseorder on actualpurchaseorder.poid = polist.purchaseorderid
                order by affecedinvoices.invoiceid";

            List<string> conditions = new List<string>();
            if (_reportcreteria.companyid > 0)
            {
                conditions.Add("lrinvoice.companyid = @companyid");
            }
            if (_reportcreteria.supplierid > 0)
            {
                conditions.Add("lrentry.supplierid = @supplierid");
            }
            if (_reportcreteria.fromdate != DateTime.MinValue)
            {
                conditions.Add("lrinvoice.entrydate >= @fromdate");
                conditions.Add("lrinvoice.entrydate < @todate");
            }

            if (_reportcreteria.frominvdate != DateTime.MinValue)
            {
                conditions.Add("lrinvoice.invoicedate >= @invfrom");
                conditions.Add("lrinvoice.invoicedate < @invto");
            }
            if (_reportcreteria.fromlrdate != DateTime.MinValue)
            {
                conditions.Add("lrinvoiceitems.barcodedon >= @barcodedonfrom");
                conditions.Add("lrinvoiceitems.barcodedon < @barcodedonto");
            }

            NpgsqlConnection conn = CreateConnection();
            List<POInvoiceSummaryItem> list = new List<POInvoiceSummaryItem>();
            try
            {
                conn.Open();
                NpgsqlCommand command = null;
                command = GetCommand(String.Format(query, conditions.Count > 0 ? (" and " + String.Join(" and ", conditions)) : ""), conn);
                command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _reportcreteria.companyid;
                command.Parameters.Add("supplierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _reportcreteria.supplierid;
                command.Parameters.Add("fromdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = _reportcreteria.fromdate.Date;
                command.Parameters.Add("todate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = _reportcreteria.todate.AddDays(1).Date;
                command.Parameters.Add("invfrom", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = _reportcreteria.frominvdate.Date;
                command.Parameters.Add("invto", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = _reportcreteria.toinvdate.AddDays(1).Date;
                command.Parameters.Add("barcodedonfrom", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = _reportcreteria.fromlrdate.Date;
                command.Parameters.Add("barcodedonto", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = _reportcreteria.tolrdate.AddDays(1).Date;

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        POInvoiceSummaryItem pOInvoiceSummaryItem = new POInvoiceSummaryItem();
                        pOInvoiceSummaryItem.supplierid = reader["supplierid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["supplierid"]);
                        pOInvoiceSummaryItem.invoicebilled = reader["invoicebilled"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["invoicebilled"]);
                        pOInvoiceSummaryItem.orderedpoqty = reader["orderedpoqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["orderedpoqty"]);
                        pOInvoiceSummaryItem.otherinvoicebilledqty = reader["otherinvoicebilledqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["otherinvoicebilledqty"]);
                        pOInvoiceSummaryItem.balancepoqty = reader["balancepoqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["balancepoqty"]);


                        pOInvoiceSummaryItem.invoicedate = reader["invoicedate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["invoicedate"]);
                        pOInvoiceSummaryItem.entrydate = reader["entrydate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["entrydate"]);
                        pOInvoiceSummaryItem.podate = reader["podate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["podate"]);

                        pOInvoiceSummaryItem.pono = reader["pono"] == DBNull.Value ? "" : Convert.ToString(reader["pono"]);
                        pOInvoiceSummaryItem.invoiceno = reader["invoiceno"] == DBNull.Value ? "" : Convert.ToString(reader["invoiceno"]);
                        pOInvoiceSummaryItem.lrentryno = reader["lrentryno"] == DBNull.Value ? "" : Convert.ToString(reader["lrentryno"]);
                        pOInvoiceSummaryItem.lrno = reader["lrno"] == DBNull.Value ? "" : Convert.ToString(reader["lrno"]);
                        pOInvoiceSummaryItem.invoicetotal = reader["totalamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["totalamount"]);
                        list.Add(pOInvoiceSummaryItem);

                    }

                }
            }
            catch (Exception exp)
            {

            }
            return list;
        }

        public POReport SearchPO(WarehouseReportCreteria _reportcreteria)
        {
            TaxDataService taxdataservice = new TaxDataService() { Context = this.Context };
            List<Tax> taxlist = taxdataservice.Search(new TaxCreteria() { type = 5 });

            POReport purchaseorder = new POReport();
            List<POReport.taxheader> lsttaxheader = new List<POReport.taxheader>();
            List<POEntry> lst = new List<POEntry>();
            bool hasTCS = taxlist != null && taxlist.Count > 0;
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                NpgsqlCommand command = null;
                bool isCreteriaAdded = false;
                QueryBuilder qb = new QueryBuilder();
                qb.CurrentQuery = string.Format(SQL_Purchase, "", "");

                if (_reportcreteria.showbarcodedon)
                {
                    qb.CurrentQuery = @" with barcodedtable as (
                                    select lrinvoiceid,min(barcodedon) barcodedon from lrinvoiceitems
										 inner join lrinvoice on lrinvoice.id = lrinvoiceid
										 inner join lrentry on lrentry.id = lrinvoice.lrid
	                                    where lrinvoiceitems.isactive=true and supplierid=@supplierid and barcodedon is not null
	                                    group by lrinvoiceid)" +
                                    string.Format(SQL_Purchase, ",max(barcodedon) barcodedon ", " left outer join barcodedtable on barcodedtable.lrinvoiceid=lrinvoice.id");

                    if (_reportcreteria.barcodedfrom != DateTime.MinValue && _reportcreteria.barcodedto != DateTime.MinValue)
                    {
                        qb.CurrentQuery = @" with barcodedtable as (
                                    select lrinvoiceid,min(barcodedon) barcodedon from lrinvoiceitems
										 inner join lrinvoice on lrinvoice.id = lrinvoiceid
										 inner join lrentry on lrentry.id = lrinvoice.lrid
                                    where lrinvoiceitems.isactive=true and barcodedon>=@barcodedfrom 
                                    and barcodedon<@barcodedto and supplierid=@supplierid and barcodedon is not null
                                    group by lrinvoiceid)" +
                                    string.Format(SQL_Purchase, ",max(barcodedon) barcodedon ", " inner join barcodedtable on barcodedtable.lrinvoiceid=lrinvoice.id");
                        isCreteriaAdded = true;
                    }
                }

                if (_reportcreteria.supplierid == 0)
                {
                    qb.CurrentQuery = qb.CurrentQuery.Replace(" and supplierid=@supplierid", "");
                }

                qb.AddParameter("lrinvoice.isactive", true, "=");
                if (_reportcreteria.removemfg)
                {
                    qb.Add2Query("(importreferencetype is null or importreferencetype !=1)");
                }
                //qb.AddInParamer("lrinvoicetax.type", "0, 1,2");

                if (_reportcreteria.multicompany && _reportcreteria.companyid > 0)
                    qb.AddParameter("lrinvoice.companyid", _reportcreteria.companyid, "=");

                if (_reportcreteria.fromdate != DateTime.MinValue && _reportcreteria.todate != DateTime.MinValue)
                {
                    qb.AddParameter("lrinvoice.entrydate", "datefrom", _reportcreteria.fromdate.Date, ">=");
                    qb.AddParameter("lrinvoice.entrydate", "dateto", _reportcreteria.todate.Date.AddDays(1), "<");
                    isCreteriaAdded = true;
                }
                if (_reportcreteria.fromlrdate != DateTime.MinValue && _reportcreteria.tolrdate != DateTime.MinValue)
                {
                    qb.AddParameter("lrinvoice.invoicedate", "lrdatefrom", _reportcreteria.fromlrdate.Date, ">=");
                    qb.AddParameter("lrinvoice.invoicedate", "lrdateto", _reportcreteria.tolrdate.Date.AddDays(1), "<");
                    isCreteriaAdded = true;
                }


                if (_reportcreteria.locationid > 0)
                    qb.AddParameter("lrentry.entrylocationid", _reportcreteria.locationid, "=");

                if (_reportcreteria.supplierid > 0)
                {
                    qb.AddParameter("lrentry.supplierid", _reportcreteria.supplierid, "=");
                    isCreteriaAdded = true;
                }
                if (_reportcreteria.agentid > 0)
                {
                    qb.AddParameter("lrentry.agentid", _reportcreteria.agentid, "=");
                    isCreteriaAdded = true;
                }
                if (_reportcreteria.suppliercityid > 0)
                {
                    qb.AddParameter("suppliercompany.cityid", _reportcreteria.suppliercityid, "=");
                    isCreteriaAdded = true;
                }
                if (_reportcreteria.receivinglocationid > 0)
                {
                    qb.AddParameter("lrentry.receivinglocationid", _reportcreteria.receivinglocationid, "=");
                    isCreteriaAdded = true;
                }
                if (_reportcreteria.onlymsmeno)
                {
                    qb.Add2Query("suppliercompany.msmeno is not null and suppliercompany.msmeno != ''");
                    isCreteriaAdded = true;
                }

                if (_reportcreteria.excludesettled)
                    qb.Add2Query("lrinvoice.settled = false");

                if (!isCreteriaAdded)
                    throw new Exception("Please provide minimum search creteria.");

                qb.CurrentQuery += @" group by lrentry.id,lrentry.duedate,lrinvoice.id, company.name, suppliercompany.name, suppliercompany.cityid, suppliercompany.tinanddate, lrentryno, 
                            lrinvoice.entrydate, lrinvoice.invoiceno, lrinvoice.invoicedate, lrinvoicetax.percentage, lrinvoice.isinterstatesale, 
                            lrinvoice.otheramount, lrentry.retaillocationid,lrentry.entrylocationid, lrinvoice.totalamount,lrinvoice.discount, lrentry.agentid, lrentry.receivinglocationid,
                            lrentry.lrno, msmeno,tdsamount,tdspercentage order by lrentry.id ";


                command = qb.GetCommand(conn);
                command.Parameters.Add("barcodedfrom", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = _reportcreteria.barcodedfrom;
                command.Parameters.Add("supplierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _reportcreteria.supplierid;
                command.Parameters.Add("barcodedto", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = _reportcreteria.barcodedto.Date.AddDays(1);
                List<decimal> taxpercent = new List<decimal>();
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        POEntry oitem = new POEntry();
                        oitem.taxbreakup = new List<POEntry.POTaxValue>();
                        oitem.invoiceid = reader["invoiceid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["invoiceid"]);
                        if (lst.Exists(x => x.invoiceid == oitem.invoiceid))
                        {
                            oitem = lst.First(x => x.invoiceid == oitem.invoiceid);
                        }
                        else
                        {
                            lst.Add(oitem);
                            oitem.agentid = reader["agentid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["agentid"]);
                            oitem.company = reader["company"] == DBNull.Value ? "" : reader["company"].ToString();
                            oitem.supplier = reader["supplier"] == DBNull.Value ? "" : reader["supplier"].ToString();
                            oitem.gstno = reader["tinanddate"] == DBNull.Value ? "" : reader["tinanddate"].ToString();
                            oitem.msmeno = reader["msmeno"] == DBNull.Value ? "" : reader["msmeno"].ToString();
                            oitem.cityid = reader["cityid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["cityid"]);
                            oitem.retaillocationid = reader["retaillocationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["retaillocationid"]);
                            oitem.entrylocationid = reader["entrylocationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["entrylocationid"]);
                            oitem.receivinglocationid = reader["receivinglocationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["receivinglocationid"]);
                            oitem.isinterstatesale = reader["isinterstatesale"] == DBNull.Value ? false : Convert.ToBoolean(reader["isinterstatesale"]);
                            oitem.lrentryno = reader["lrentryno"] == DBNull.Value ? "" : reader["lrentryno"].ToString();
                            oitem.lrno = reader["lrno"] == DBNull.Value ? "" : reader["lrno"].ToString();
                            oitem.lrentrydate = reader["entrydate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["entrydate"]);
                            oitem.invoiceno = reader["invoiceno"] == DBNull.Value ? "" : reader["invoiceno"].ToString();
                            oitem.invoicedate = reader["invoicedate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["invoicedate"]);
                            oitem.forwardcharge = reader["forwardcharge"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["forwardcharge"]);
                            oitem.otheramount = reader["otheramount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["otheramount"]);
                            oitem.duedate = reader["duedate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["duedate"]);
                            oitem.discount = reader["discount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discount"]);
                            oitem.invoicetotalamount = reader["totalamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["totalamount"]);

                            oitem.tdspercentage = reader["tdspercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["tdspercentage"]);
                            oitem.tdsamount = reader["tdsamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["tdsamount"]);
                        }
                        if (_reportcreteria.showbarcodedon)
                        {
                            oitem.barcodedon = reader["barcodedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["barcodedon"]);
                        }

                        oitem.invoiceamount += reader["invoiceamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["invoiceamount"]);

                        if (hasTCS)
                        {
                            if (!taxlist.Exists(e => e.taxpercentage == (reader["percentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["percentage"]))))
                                oitem.taxableamount += reader["taxableamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["taxableamount"]);
                        }
                        else
                            oitem.taxableamount += reader["taxableamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["taxableamount"]);

                        oitem.taxamount += reader["taxamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["taxamount"]);
                        POEntry.POTaxValue otax = new POEntry.POTaxValue();
                        otax.taxpercent = reader["percentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["percentage"]);
                        otax.taxamount = reader["taxamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["taxamount"]);
                        otax.taxableamount = reader["taxableamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["taxableamount"]);
                        oitem.taxbreakup.Add(otax);

                        if (!taxpercent.Contains(otax.taxpercent))
                            taxpercent.Add(otax.taxpercent);
                    }
                }
                conn.Close();

                //List<decimal> taxpercent = lst.Select(x => x.percentage).Distinct().ToList();
                if (taxpercent != null && taxpercent.Count > 0)
                {
                    foreach (decimal taxitem in taxpercent.OrderBy(e => e).ToList())
                    {
                        lsttaxheader.Add(new POReport.taxheader
                        {
                            percentage = taxitem,
                            type = 1,
                            displaytext = taxitem.ToString() + "% VALUE"
                        });
                        lsttaxheader.Add(new POReport.taxheader
                        {
                            percentage = taxitem,
                            type = 2,
                            displaytext = taxitem.ToString() + "% TAX"
                        });
                    }
                }
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _reportcreteria);
            }

            List<long> invoiceids = new List<long>();
            lst.ForEach(e =>
            {
                if (e.invoiceid > 0)
                    invoiceids.Add(e.invoiceid);
            });

            string SQL_Invoiceqty = "Select lrinvoiceid, sum(buyingqty) qty from lrinvoiceitems where isactive=true and lrinvoiceid in ({0}) group by lrinvoiceid";

            if (invoiceids.Count > 0)
            {
                try
                {
                    conn.Open();
                    NpgsqlCommand command1 = GetCommand(string.Format(SQL_Invoiceqty, string.Join(",", invoiceids)), conn); ;
                    using (System.Data.IDataReader reader = command1.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            long id = reader["lrinvoiceid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["lrinvoiceid"]);
                            long qty = reader["qty"] == DBNull.Value ? 0 : Convert.ToInt64(reader["qty"]);
                            if (lst.Exists(e => e.invoiceid == id))
                                lst.Find(e => e.invoiceid == id).qty = qty;
                        }
                    }
                    conn.Close();
                }
                catch (Exception exp)
                {
                    if (conn != null) conn.Close();
                }
            }
            purchaseorder.poentry = lst;
            purchaseorder.taxhead = lsttaxheader;
            return purchaseorder;
        }

        internal List<SalesTaxSummary> SearchPurchaseTaxSummary(WarehouseReportCreteria _reportcreteria)
        {
            string SQL_SalesTaxSummaryModified = @"
                        select lrinvoice.companyid,
lrinvoice.isinterstatesale, 
lrinvoicetax.percentage,
sum(lrinvoicetax.taxamount) taxamount, 
sum(case when lrinvoicetax.taxamount>0 then amounton-lrinvoicetax.discount else 0 end) taxableamount,
sum(netamount) invoiceamount
from lrentry 
inner join lrinvoice on lrentry.id=lrinvoice.lrid and lrinvoice.isactive=true
inner join lrinvoicetax on lrinvoicetax.invoiceid=lrinvoice.id  and lrinvoicetax.isactive=true
where companyid=@companyid and lrinvoice.entrydate>=@fromdate and lrinvoice.entrydate<@todate
group by lrinvoice.companyid,lrinvoice.isinterstatesale, lrinvoicetax.percentage
";

            List<SalesTaxSummary> lst = new List<SalesTaxSummary>();
            DateTime today = _reportcreteria.fromdate.Date;
            DateTime nextdate = _reportcreteria.todate.AddDays(1).Date;
            NpgsqlConnection conn = CreateConnection();

            try
            {
                conn.Open();
                string query = string.Format(SQL_SalesTaxSummaryModified, " and billtype!=9 ");

                NpgsqlCommand command = null;
                command = GetCommand(query, conn);
                command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _reportcreteria.companyid;
                command.Parameters.Add("fromdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = today;
                command.Parameters.Add("todate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = nextdate;
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        SalesTaxSummary item = new SalesTaxSummary();
                        item.companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"]);
                        item.rowmode = "P";
                        if (reader["isinterstatesale"] != DBNull.Value &&
                            Convert.ToBoolean(reader["isinterstatesale"]))
                        {
                            item.igst = reader["taxamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["taxamount"]);
                            item.taxamount = item.igst;
                        }
                        else
                        {
                            item.taxamount = reader["taxamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["taxamount"]);
                            item.cgst = Math.Round((item.taxamount / 2), 2);
                            item.sgst = Math.Round((item.taxamount / 2), 2);
                        }
                        item.amount = (reader["taxableamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["taxableamount"]));
                        item.taxpercentage = reader["percentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["percentage"]);
                        item.total = Math.Round(item.amount + item.taxamount, 0);

                        if (lst.Exists(x => x.taxpercentage == item.taxpercentage && item.companyid == x.companyid))
                        {
                            var eitem = lst.Find(x => x.taxpercentage == item.taxpercentage && item.companyid == x.companyid);
                            eitem.igst += item.igst;
                            eitem.cgst += item.cgst;
                            eitem.sgst += item.sgst;
                            eitem.amount += item.amount;
                            eitem.total += item.total;
                        }
                        else
                        {
                            lst.Add(item);
                        }
                    }
                }

                query = @"select lrinvoicereturn.companyid,
lrinvoicereturn.isinterstatesale, 
lrinvoicereturnitems.taxpercentage,
sum(lrinvoicereturnitems.tax) taxamount, 
sum(lrinvoicereturnitems.amount-lrinvoicereturnitems.discount) taxableamount,
sum(lrinvoicereturnitems.amount-lrinvoicereturnitems.discount+lrinvoicereturnitems.tax) invoiceamount
from lrinvoicereturnitems 
inner join lrinvoicereturn on lrinvoicereturn.id=lrinvoicereturnitems.lrinvoicereturnid 
and lrinvoicereturnitems.isactive=true and lrinvoicereturn.returntype<14 and lrinvoicereturn.isactive=true
where lrinvoicereturn.companyid=@companyid and lrinvoicereturn.returndate>=@fromdate and lrinvoicereturn.returndate<@todate
group by lrinvoicereturn.companyid,lrinvoicereturn.isinterstatesale, lrinvoicereturnitems.taxpercentage
";

                command = GetCommand(query, conn);
                command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _reportcreteria.locationid;
                command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _reportcreteria.companyid;
                command.Parameters.Add("fromdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = today;
                command.Parameters.Add("todate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = nextdate;
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        SalesTaxSummary item = new SalesTaxSummary();
                        item.rowmode = "PR";
                        item.companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"]);

                        if (reader["isinterstatesale"] != DBNull.Value &&
                            Convert.ToBoolean(reader["isinterstatesale"]))
                        {
                            item.igst = reader["taxamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["taxamount"]);
                            item.taxamount = item.igst;
                        }
                        else
                        {
                            item.taxamount = reader["taxamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["taxamount"]);
                            item.cgst = Math.Round((item.taxamount / 2), 2);
                            item.sgst = Math.Round((item.taxamount / 2), 2);
                        }
                        item.amount = (reader["taxableamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["taxableamount"]));
                        item.taxpercentage = reader["taxpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["taxpercentage"]);
                        item.total = Math.Round(item.amount + item.taxamount, 0);


                        item.amount = item.amount * -1;
                        item.igst = item.igst * -1;
                        item.taxamount = item.taxamount * -1;
                        item.cgst = item.cgst * -1;
                        item.sgst = item.sgst * -1;
                        item.total = item.total * -1;

                        if (lst.Exists(x => x.taxpercentage == item.taxpercentage && item.companyid == x.companyid && x.total < 0))
                        {
                            var eitem = lst.Find(x => x.taxpercentage == item.taxpercentage && item.companyid == x.companyid && x.total < 0);
                            eitem.igst += item.igst;
                            eitem.cgst += item.cgst;
                            eitem.sgst += item.sgst;
                            eitem.amount += item.amount;
                            eitem.taxamount += item.taxamount;
                            eitem.total += item.total;
                        }
                        else
                        {
                            lst.Add(item);
                        }
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

        public POReport SearchPurchaseTax(WarehouseReportCreteria _reportcreteria)
        {
            POReport purchaseorder = new POReport();
            List<POReport.taxheader> lsttaxheader = new List<POReport.taxheader>();
            List<POEntry> lst = new List<POEntry>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                NpgsqlCommand command = null;

                QueryBuilder qb = new QueryBuilder();
                qb.CurrentQuery = SQL_PurchaseTax;
                bool isCreteriaAdded = false;

                if (_reportcreteria.multicompany && _reportcreteria.companyid > 0)
                    qb.AddParameter("lrinvoice.companyid", _reportcreteria.companyid, "=");

                if (_reportcreteria.fromdate != DateTime.MinValue && _reportcreteria.todate != DateTime.MinValue)
                {
                    qb.AddParameter("lrinvoice.entrydate", "datefrom", _reportcreteria.fromdate.Date, ">=");
                    qb.AddParameter("lrinvoice.entrydate", "dateto", _reportcreteria.todate.Date.AddDays(1), "<");
                    isCreteriaAdded = true;
                }
                if (_reportcreteria.fromlrdate != DateTime.MinValue && _reportcreteria.tolrdate != DateTime.MinValue)
                {
                    qb.AddParameter("lrinvoice.invoicedate", "lrdatefrom", _reportcreteria.fromlrdate.Date, ">=");
                    qb.AddParameter("lrinvoice.invoicedate", "lrdateto", _reportcreteria.tolrdate.Date.AddDays(1), "<");
                    isCreteriaAdded = true;
                }

                if (_reportcreteria.supplierid > 0)
                {
                    qb.AddParameter("lrentry.supplierid", _reportcreteria.supplierid, "=");
                    isCreteriaAdded = true;
                }

                if (!isCreteriaAdded)
                    throw new Exception("Please provide minimum search creteria.");

                qb.CurrentQuery += @" group by lrinvoice.isinterstatesale, suppliercompany.name, suppliercompany.cityid, 
                                      lrinvoicetax.percentage,city.name";
                command = qb.GetCommand(conn);

                List<decimal> taxpercent = new List<decimal>();
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        POEntry oitem;
                        string suppliername = reader["supplier"] == DBNull.Value ? "" : reader["supplier"].ToString();
                        long cityid = reader["cityid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["cityid"]);
                        if (lst.Exists(e => e.cityid == cityid && e.supplier == suppliername))
                        {
                            oitem = lst.Find(e => e.cityid == cityid && e.supplier == suppliername);
                        }
                        else
                        {
                            oitem = new POEntry();
                            oitem.taxbreakup = new List<POEntry.POTaxValue>();
                            oitem.supplier = reader["supplier"] == DBNull.Value ? "" : reader["supplier"].ToString();
                            oitem.cityid = cityid;
                            oitem.isinterstatesale = reader["isinterstatesale"] == DBNull.Value ? false : Convert.ToBoolean(reader["isinterstatesale"]);
                            lst.Add(oitem);
                        }

                        POEntry.POTaxValue otax = new POEntry.POTaxValue();
                        otax.taxpercent = reader["percentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["percentage"]);
                        otax.taxamount = reader["taxamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["taxamount"]);
                        otax.taxableamount = reader["taxableamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["taxableamount"]);
                        if (oitem.taxbreakup.Exists(e => e.taxpercent == otax.taxpercent))
                        {
                            var taxb = oitem.taxbreakup.Find(e => e.taxpercent == otax.taxpercent);
                            taxb.taxamount += otax.taxamount;
                            taxb.taxableamount += otax.taxableamount;
                        }
                        else
                        {
                            oitem.taxbreakup.Add(otax);
                        }
                        if (!taxpercent.Contains(otax.taxpercent))
                            taxpercent.Add(otax.taxpercent);

                    }
                }
                conn.Close();

                // For Row level Total 
                foreach (POEntry item in lst)
                {
                    POEntry.POTaxValue otax = new POEntry.POTaxValue();
                    otax.taxpercent = 100;
                    otax.taxamount = item.taxbreakup.Sum(e => e.taxamount);
                    otax.qty = item.taxbreakup.Sum(e => e.qty);
                    otax.taxableamount = item.taxbreakup.Sum(e => e.taxableamount);

                    item.taxbreakup.Add(otax);
                }


                var taxable = lst.Sum(e => e.taxbreakup.Sum(x => x.taxableamount));
                //List<decimal> taxpercent = lst.Select(x =>  x.percentage).Distinct().ToList();
                if (taxpercent != null && taxpercent.Count > 0)
                {
                    taxpercent.Sort();
                    foreach (decimal taxitem in taxpercent)
                    {
                        lsttaxheader.Add(new POReport.taxheader
                        {
                            percentage = taxitem,
                            type = 1,
                            displaytext = taxitem.ToString() + "% VALUE",
                            taxalevalue = lst.Sum(e => e.taxbreakup.Sum(x => x.taxpercent == taxitem ? x.taxableamount : 0))
                        });
                        lsttaxheader.Add(new POReport.taxheader
                        {
                            percentage = taxitem,
                            type = 2,
                            displaytext = taxitem.ToString() + "% TAX"
                        });
                    }
                    lsttaxheader.Add(new POReport.taxheader
                    {
                        percentage = 100,
                        type = 1,
                        displaytext = "TOT.VALUE",
                    });
                    lsttaxheader.Add(new POReport.taxheader
                    {
                        percentage = 100,
                        type = 2,
                        displaytext = "TOT.TAX"
                    });
                }

            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _reportcreteria);
            }
            purchaseorder.poentry = lst;
            purchaseorder.taxhead = lsttaxheader;
            return purchaseorder;
        }
        public POReport SearchPurchaseHSN(WarehouseReportCreteria _reportcreteria)
        {
            POReport purchaseorder = new POReport();
            List<POReport.taxheader> lsttaxheader = new List<POReport.taxheader>();
            List<POEntry> lst = new List<POEntry>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                NpgsqlCommand command = null;
                bool isCreteriaAdded = false;

                QueryBuilder qb = new QueryBuilder();
                qb.CurrentQuery = SQL_PurchaseHSN;

                if (_reportcreteria.multicompany && _reportcreteria.companyid > 0)
                    qb.AddParameter("lrinvoice.companyid", _reportcreteria.companyid, "=");

                if (_reportcreteria.fromdate != DateTime.MinValue && _reportcreteria.todate != DateTime.MinValue)
                {
                    qb.AddParameter("lrinvoice.entrydate", "datefrom", _reportcreteria.fromdate.Date, ">=");
                    qb.AddParameter("lrinvoice.entrydate", "dateto", _reportcreteria.todate.Date.AddDays(1), "<");
                    isCreteriaAdded = true;
                }
                if (_reportcreteria.fromlrdate != DateTime.MinValue && _reportcreteria.tolrdate != DateTime.MinValue)
                {
                    qb.AddParameter("lrinvoice.invoicedate", "lrdatefrom", _reportcreteria.fromlrdate.Date, ">=");
                    qb.AddParameter("lrinvoice.invoicedate", "lrdateto", _reportcreteria.tolrdate.Date.AddDays(1), "<");
                    isCreteriaAdded = true;
                }

                if (_reportcreteria.supplierid > 0)
                {
                    qb.AddParameter("lrentry.supplierid", _reportcreteria.supplierid, "=");
                    isCreteriaAdded = true;
                }

                if (!isCreteriaAdded)
                    throw new Exception("Please provide minimum search creteria.");

                qb.CurrentQuery += @" group by lrinvoice.id, lrinvoiceitems.hsncode, items.productname, lrinvoice.isinterstatesale, lrinvoice.otheramount, suppliercompany.name, suppliercompany.cityid, lrinvoiceitems.purchasetaxpercentage,case when products.sellingmode = 3 then 'M' else 'N' end
                                      order by suppliercompany.name, lrinvoiceitems.hsncode ";
                command = qb.GetCommand(conn);

                List<decimal> taxpercent = new List<decimal>();
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        POEntry oitem;
                        string hsncode = reader["hsncode"] == DBNull.Value ? "" : reader["hsncode"].ToString();
                        decimal taxpercentage = reader["purchasetaxpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["purchasetaxpercentage"]);
                        string productname = reader["productname"] == DBNull.Value ? "" : reader["productname"].ToString();
                        string suppliername = reader["supplier"] == DBNull.Value ? "" : reader["supplier"].ToString();
                        if (lst.Exists(e => e.hsncode == hsncode && e.productname == productname && e.supplier == suppliername))
                        {
                            oitem = lst.Find(e => e.hsncode == hsncode && e.productname == productname && e.supplier == suppliername);
                        }
                        else
                        {
                            oitem = new POEntry();
                            oitem.taxbreakup = new List<POEntry.POTaxValue>();
                            oitem.invoiceid = reader["invoiceid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["invoiceid"]);
                            oitem.supplier = reader["supplier"] == DBNull.Value ? "" : reader["supplier"].ToString();
                            oitem.cityid = reader["cityid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["cityid"]);
                            oitem.isinterstatesale = reader["isinterstatesale"] == DBNull.Value ? false : Convert.ToBoolean(reader["isinterstatesale"]);
                            oitem.hsncode = reader["hsncode"] == DBNull.Value ? "" : reader["hsncode"].ToString();
                            oitem.productname = reader["productname"] == DBNull.Value ? "" : reader["productname"].ToString();
                            oitem.percentage = reader["purchasetaxpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["purchasetaxpercentage"]);
                            oitem.measure = reader["Measure"] == DBNull.Value ? "Nos" : (reader["Measure"].ToString() == "N" ? "Nos" : "Mts");
                            lst.Add(oitem);
                        }

                        POEntry.POTaxValue otax = new POEntry.POTaxValue();
                        otax.taxpercent = reader["purchasetaxpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["purchasetaxpercentage"]);
                        otax.taxamount = reader["tax"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["tax"]);
                        otax.qty = reader["buyingqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["buyingqty"]);
                        otax.taxableamount = reader["value"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["value"]);
                        if (oitem.taxbreakup.Exists(e => e.taxpercent == otax.taxpercent))
                        {
                            var taxb = oitem.taxbreakup.Find(e => e.taxpercent == otax.taxpercent);
                            taxb.taxamount += otax.taxamount;
                            taxb.qty += otax.qty;
                            taxb.taxableamount += otax.taxableamount;
                        }
                        else
                        {
                            oitem.taxbreakup.Add(otax);
                        }
                        if (!taxpercent.Contains(otax.taxpercent))
                            taxpercent.Add(otax.taxpercent);

                    }
                }
                conn.Close();

                // For Row level Total 
                foreach (POEntry item in lst)
                {
                    POEntry.POTaxValue otax = new POEntry.POTaxValue();
                    otax.taxpercent = 100;
                    otax.taxamount = item.taxbreakup.Sum(e => e.taxamount);
                    otax.qty = item.taxbreakup.Sum(e => e.qty);
                    otax.taxableamount = item.taxbreakup.Sum(e => e.taxableamount);

                    item.taxbreakup.Add(otax);
                }
                StringBuilder sb = new StringBuilder();
                foreach (var item in lst)
                {
                    if (item.taxbreakup.Exists(e => e.taxpercent == 12))
                    {
                        sb.Append(string.Format("{0}\t{1}\t{2}\r\n", item.hsncode, item.productname, item.taxbreakup.Find(e => e.taxpercent == 12).taxableamount));
                    }
                }

                var taxable = lst.Sum(e => e.taxbreakup.Sum(x => x.taxableamount));
                //List<decimal> taxpercent = lst.Select(x =>  x.percentage).Distinct().ToList();
                if (taxpercent != null && taxpercent.Count > 0)
                {
                    foreach (decimal taxitem in taxpercent)
                    {
                        lsttaxheader.Add(new POReport.taxheader
                        {
                            percentage = taxitem,
                            type = 1,
                            displaytext = taxitem.ToString() + "% VALUE",
                            taxalevalue = lst.Sum(e => e.taxbreakup.Sum(x => x.taxpercent == taxitem ? x.taxableamount : 0))
                        });
                        lsttaxheader.Add(new POReport.taxheader
                        {
                            percentage = taxitem,
                            type = 2,
                            displaytext = taxitem.ToString() + "% TAX"
                        });
                        lsttaxheader.Add(new POReport.taxheader
                        {
                            percentage = taxitem,
                            type = 3,
                            displaytext = taxitem.ToString() + "% QTY"
                        });
                    }
                    lsttaxheader.Add(new POReport.taxheader
                    {
                        percentage = 100,
                        type = 1,
                        displaytext = "TOT.VALUE",
                    });
                    lsttaxheader.Add(new POReport.taxheader
                    {
                        percentage = 100,
                        type = 2,
                        displaytext = "TOT.TAX"
                    });
                    lsttaxheader.Add(new POReport.taxheader
                    {
                        percentage = 100,
                        type = 3,
                        displaytext = "TOT.QTY"
                    });
                }

            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _reportcreteria);
            }
            purchaseorder.poentry = lst;
            purchaseorder.taxhead = lsttaxheader;
            return purchaseorder;
        }
        public PurchaseReturnReport SearchPurchaseReturn(WarehouseReportCreteria _reportcreteria)
        {
            PurchaseReturnReport oReturn = new PurchaseReturnReport();
            List<PurchaseReturnReport.taxheader> lsttaxheader = new List<PurchaseReturnReport.taxheader>();
            List<PurchaseReturnEntry> lst = new List<PurchaseReturnEntry>();

            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                NpgsqlCommand command = null;

                QueryBuilder qb = new QueryBuilder();

                if (_reportcreteria.showdetail)
                {
                    qb.CurrentQuery = SQL_DebitNoteDetail;
                    if (!string.IsNullOrEmpty(_reportcreteria.returnno))
                        qb.AddParameter("returnno", _reportcreteria.returnno, "=");
                }
                else
                    qb.CurrentQuery = SQL_DebitNotesummary;

                if (_reportcreteria.multicompany && _reportcreteria.companyid > 0)
                    qb.AddParameter("lrinvoicereturn.companyid", _reportcreteria.companyid, "=");

                if (_reportcreteria.cancelled)
                {
                    qb.AddParameter("lrinvoicereturn.isactive", false, "=");
                    qb.AddParameter("lrinvoicereturnitems.isactive", false, "=");
                }
                else
                {
                    qb.AddParameter("lrinvoicereturn.isactive", true, "=");
                    qb.AddParameter("lrinvoicereturnitems.isactive", true, "=");
                }
                if (_reportcreteria.fromdate != DateTime.MinValue && _reportcreteria.todate != DateTime.MinValue)
                {
                    qb.AddParameter("lrinvoicereturn.returndate", "returnfrom", _reportcreteria.fromdate.Date, ">=");
                    qb.AddParameter("lrinvoicereturn.returndate", "returnto", _reportcreteria.todate.AddDays(1).Date, "<");
                }
                if (_reportcreteria.supplierid > 0)
                    qb.AddParameter("lrinvoicereturn.supplierid", _reportcreteria.supplierid, "=");

                if (_reportcreteria.returntypeid >= 0)
                    qb.AddParameter("lrinvoicereturn.returntype", _reportcreteria.returntypeid, "=");

                if (!_reportcreteria.showdetail)
                    qb.CurrentQuery += @" group by lrinvoicereturn.id, lrinvoicereturn.locationid, lrinvoice.id, supplier.name, suppliercompany.cityid, suppliercompany.tinanddate, returnno, returndate, lrinvoicereturn.isinterstatesale, lrinvoicereturn.qty, 
                        lrinvoice.invoiceno, lrinvoice.invoicedate, lrinvoicereturnitems.taxpercentage, lrinvoicereturn.amount,lrinvoicereturn.tax,lrinvoicereturn.discount,lrinvoicereturn.total,lrinvoicereturn.rounding,
                        freightamt,freighttaxamt,packingcharge,lrinvoicereturn.createdby,lrinvoicereturn.modifiedby";
                qb.SortField = "lrinvoicereturn.id desc";
                command = qb.GetCommand(conn);

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        PurchaseReturnEntry oitem = new PurchaseReturnEntry();
                        oitem.taxbreakup = new List<POEntry.POTaxValue>();
                        oitem.invoiceid = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        oitem.supplier = reader["supplier"] == DBNull.Value ? "" : reader["supplier"].ToString();

                        oitem.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        oitem.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);

                        oitem.locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                        oitem.cityid = reader["cityid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["cityid"]);
                        oitem.gstno = reader["tinanddate"] == DBNull.Value ? "" : reader["tinanddate"].ToString();
                        oitem.returnno = reader["returnno"] == DBNull.Value ? "" : reader["returnno"].ToString();
                        oitem.returndate = reader["returndate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["returndate"]);
                        oitem.invoiceno = reader["invoiceno"] == DBNull.Value ? "" : reader["invoiceno"].ToString();
                        oitem.invoicedate = reader["invoicedate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["invoicedate"]);
                        oitem.percentage = reader["taxname"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["taxname"]);
                        oitem.isinterstatesale = reader["isinterstatesale"] == DBNull.Value ? false : Convert.ToBoolean(reader["isinterstatesale"]);
                        oitem.qty = reader["qty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["qty"]);
                        oitem.salevalue = reader["salevalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["salevalue"]);

                        oitem.returnamount = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"]);
                        oitem.taxamount = reader["tax"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["tax"]);
                        oitem.discount = reader["discount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discount"]);
                        oitem.netamount = reader["total"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["total"]);
                        oitem.otheramount = reader["rounding"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["rounding"]);
                        oitem.freightamt = reader["freightamt"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["freightamt"]);
                        oitem.freighttaxamt = reader["freighttaxamt"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["freighttaxamt"]);
                        oitem.packingcharge = reader["packingcharge"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["packingcharge"]);
                        oitem.taxableamount = oitem.returnamount - oitem.discount;

                        POEntry.POTaxValue otax = new POEntry.POTaxValue();
                        otax.taxpercent = reader["taxname"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["taxname"]);
                        otax.taxamount = reader["itemtax"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["itemtax"]);
                        otax.taxableamount = reader["taxable"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["taxable"]);
                        oitem.taxbreakup.Add(otax);
                        lst.Add(oitem);
                    }
                }
                conn.Close();

                List<decimal> taxpercent = lst.Select(x => x.percentage).Distinct().ToList();
                if (taxpercent != null && taxpercent.Count > 0)
                {
                    foreach (decimal taxitem in taxpercent)
                    {
                        lsttaxheader.Add(new PurchaseReturnReport.taxheader
                        {
                            percentage = taxitem,
                            type = 1,
                            displaytext = taxitem.ToString() + "% VALUE"
                        });
                        lsttaxheader.Add(new PurchaseReturnReport.taxheader
                        {
                            percentage = taxitem,
                            type = 2,
                            displaytext = taxitem.ToString() + "% TAX"
                        });
                    }
                }
                List<long> ids = lst.GroupBy(g => g.invoiceid).Where(grp => grp.Count() > 1).Select(grp => grp.Key).ToList();
                if (ids != null && ids.Count > 0)
                {
                    foreach (long id in ids)
                    {
                        List<PurchaseReturnEntry> tomerge = lst.Where(x => x.invoiceid == id).ToList();
                        lst.RemoveAll(x => x.invoiceid == id);

                        PurchaseReturnEntry oitem = new PurchaseReturnEntry();
                        oitem.taxbreakup = new List<POEntry.POTaxValue>();
                        int count = 0;
                        foreach (PurchaseReturnEntry newitem in tomerge)
                        {
                            if (count == 0)
                            {
                                oitem.invoiceid = newitem.invoiceid;
                                oitem.invoiceno = newitem.invoiceno;
                                oitem.invoicedate = newitem.invoicedate;
                                oitem.supplier = newitem.supplier;
                                oitem.cityid = newitem.cityid;
                                oitem.gstno = newitem.gstno;
                                oitem.locationid = newitem.locationid;
                                oitem.returnno = newitem.returnno;
                                oitem.returndate = newitem.returndate;
                                oitem.isinterstatesale = newitem.isinterstatesale;
                                oitem.createdby = newitem.createdby;
                                oitem.modifiedby = newitem.modifiedby;
                                oitem.qty = newitem.qty;
                                oitem.returnamount = newitem.returnamount;
                                oitem.taxamount = newitem.taxamount;
                                oitem.taxableamount = newitem.taxableamount;
                                oitem.discount = newitem.discount;
                                oitem.netamount = newitem.netamount;
                                oitem.otheramount = newitem.otheramount;
                            }
                            oitem.salevalue += newitem.salevalue;

                            POEntry.POTaxValue otax = new POEntry.POTaxValue();
                            otax.taxpercent = newitem.taxbreakup[0].taxpercent;
                            otax.taxamount = newitem.taxbreakup[0].taxamount;
                            otax.taxableamount = newitem.taxbreakup[0].taxableamount;
                            oitem.taxbreakup.Add(otax);
                            count++;
                        }
                        List<decimal> disttax = oitem.taxbreakup.GroupBy(g => g.taxpercent).Where(grp => grp.Count() > 1).Select(grp => grp.Key).ToList();
                        if (disttax != null && disttax.Count > 0)
                        {
                            List<POEntry.POTaxValue> existingtax = oitem.taxbreakup;
                            oitem.taxbreakup = new List<POEntry.POTaxValue>();
                            var sumoftaxpercent =
                                            from item in existingtax
                                            group item by item.taxpercent into itemGroup
                                            select new POEntry.POTaxValue
                                            {
                                                taxpercent = itemGroup.Key,
                                                taxamount = itemGroup.Sum(x => x.taxamount),
                                                taxableamount = itemGroup.Sum(x => x.taxableamount)
                                            };
                            oitem.taxbreakup = sumoftaxpercent.ToList();
                        }
                        lst.Add(oitem);
                    }
                }

            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _reportcreteria);
            }
            oReturn.purchasereturn = lst;
            oReturn.taxhead = lsttaxheader;
            List<long> userids = new List<long>();
            userids.AddRange(lst.ConvertAll(x => x.createdby));
            userids.AddRange(lst.ConvertAll(x => x.modifiedby));
            EmployeeDataService employeeDataService = new EmployeeDataService() { Context = this.Context };
            var employeenames = employeeDataService.GetUserName(userids.Distinct().ToList());
            lst.ForEach(x =>
            {
                if (employeenames.Exists(y => y.id == x.createdby))
                {
                    x.createdemployee = employeenames.Find(y => y.id == x.createdby).name;
                }
                if (employeenames.Exists(y => y.id == x.modifiedby))
                {
                    x.modifiedemployee = employeenames.Find(y => y.id == x.modifiedby).name;
                }
            });
            return oReturn;
        }
        public List<OutwardReport> SearchOutwardCost(WarehouseReportCreteria _reportcreteria)
        {
            List<OutwardReport> lst = new List<OutwardReport>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                NpgsqlCommand command = null;
                QueryBuilder qb = new QueryBuilder();
                if (_reportcreteria.fromdate == DateTime.MinValue || _reportcreteria.todate == DateTime.MinValue)
                {
                    _reportcreteria.fromdate = DateTimeNow.AddDays(-365);
                    _reportcreteria.todate = DateTimeNow;
                }
                //if (_reportcreteria.fromlrdate == DateTime.MinValue || _reportcreteria.tolrdate == DateTime.MinValue)
                //{
                //    _reportcreteria.fromlrdate = DateTimeNow.AddDays(-365);
                //    _reportcreteria.tolrdate = DateTimeNow;
                //}
                if (!_reportcreteria.multicompany)
                {
                    _reportcreteria.companyid = 0;
                    _reportcreteria.tocompanyid = 0;
                }
                if (_reportcreteria.showdetail)
                {
                    var query = "";
                    if (string.IsNullOrEmpty(_reportcreteria.packageno))
                    {
                        if (_reportcreteria.fromlrdate == DateTime.MinValue || _reportcreteria.tolrdate == DateTime.MinValue)
                        {
                            query = string.Format(SQL_OutwardItems, @" and case when @locationid=0 then tolocationid != 0 else tolocationid=@locationid end        
        and case when @fromlocationid=0 then fromlocationid != 0 else fromlocationid=@fromlocationid end   
        and case when @fromcompanyid=0 then fromcompanyid >= 0 else fromcompanyid=@fromcompanyid end             
        and case when @tocompanyid=0 then tocompanyid >= 0 else tocompanyid=@tocompanyid end   
        and packedon>=@fromdate and packedon<@todate ");
                        }
                        else
                        {
                            query = string.Format(SQL_OutwardItems, @" and case when @locationid=0 then tolocationid != 0 else tolocationid=@locationid end        
        and case when @fromlocationid=0 then fromlocationid != 0 else fromlocationid=@fromlocationid end   
        and case when @fromcompanyid=0 then fromcompanyid >= 0 else fromcompanyid=@fromcompanyid end             
        and case when @tocompanyid=0 then tocompanyid >= 0 else tocompanyid=@tocompanyid end   
        and packedon>=@fromdate and packedon<@todate 
        and receivedon>=@fromlrdate and receivedon<@tolrdate");
                        }
                    }
                    else
                        query = string.Format(SQL_OutwardItems, " and bundles.code=@code ");

                    command = null;
                    command = GetCommand(query, conn);
                    command.Parameters.Add("status", NpgsqlTypes.NpgsqlDbType.Integer).Value = (_reportcreteria.pendinginward) ? 0 : 1;
                    command.Parameters.Add("code", NpgsqlTypes.NpgsqlDbType.Varchar).Value = string.IsNullOrEmpty(_reportcreteria.packageno) ? "" : _reportcreteria.packageno;
                    command.Parameters.Add("fromlocationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _reportcreteria.fromlocationid;
                    command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _reportcreteria.locationid;
                    command.Parameters.Add("fromcompanyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _reportcreteria.companyid;
                    command.Parameters.Add("tocompanyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _reportcreteria.tocompanyid;
                    command.Parameters.Add("fromdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = _reportcreteria.fromdate.Date;
                    command.Parameters.Add("todate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = _reportcreteria.todate.AddDays(1).Date;
                    command.Parameters.Add("fromlrdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = _reportcreteria.fromlrdate.Date;
                    command.Parameters.Add("tolrdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = _reportcreteria.tolrdate.AddDays(1).Date;
                }
                else
                {
                    command = null;
                    if (_reportcreteria.showsummary)
                        command = GetCommand(string.Format(SQL_OutwardwithCost, "", ""), conn);
                    else
                        command = GetCommand(string.Format(SQL_OutwardwithCost, "purchasetaxpercentage,", ",purchasetaxpercentage"), conn);

                    if (!_reportcreteria.pendinginward)
                        command.CommandText = command.CommandText.Replace("and bundles.status=0", "");

                    command.Parameters.Add("status", NpgsqlTypes.NpgsqlDbType.Integer).Value = (_reportcreteria.pendinginward) ? 0 : 1;
                    command.Parameters.Add("fromlocationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _reportcreteria.fromlocationid;
                    command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _reportcreteria.locationid;
                    command.Parameters.Add("supplierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _reportcreteria.supplierid;
                    command.Parameters.Add("fromcompanyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _reportcreteria.companyid;
                    command.Parameters.Add("tocompanyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _reportcreteria.tocompanyid;
                    command.Parameters.Add("fromdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = _reportcreteria.fromdate.Date;
                    command.Parameters.Add("todate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = _reportcreteria.todate.AddDays(1).Date;
                }
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        OutwardReport oitem = new OutwardReport();
                        oitem.code = reader["code"] == DBNull.Value ? "" : reader["code"].ToString();
                        oitem.status = reader["status"] == DBNull.Value ? 0 : Convert.ToInt32(reader["status"].ToString());
                        oitem.fromcompanyid = reader["fromcompanyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["fromcompanyid"].ToString());
                        oitem.tocompanyid = reader["tocompanyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["tocompanyid"].ToString());
                        oitem.fromlocation = reader["fromlocation"] == DBNull.Value ? "" : reader["fromlocation"].ToString();
                        oitem.tolocation = reader["tolocation"] == DBNull.Value ? "" : reader["tolocation"].ToString();
                        oitem.qty = reader["qty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["qty"]);
                        oitem.aqty = reader["acceptqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["acceptqty"]);
                        oitem.packedon = reader["packedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["packedon"]);
                        oitem.receivedon = reader["receivedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["receivedon"]);
                        oitem.receivedby = reader["receivedby"] == DBNull.Value ? "" : reader["receivedby"].ToString();
                        long fromlocationid = reader["fromlocationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["fromlocationid"].ToString());
                        long tolocationid = reader["tolocationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["tolocationid"].ToString());
                        if (_reportcreteria.showdetail)
                        {
                            oitem.productname = reader["printingname"] == DBNull.Value ? "" : reader["printingname"].ToString();
                            oitem.section = reader["section"] == DBNull.Value ? "" : reader["section"].ToString();
                            oitem.barcode = reader["barcode"] == DBNull.Value ? "" : reader["barcode"].ToString();
                            oitem.suppliername = reader["suppliername"] == DBNull.Value ? "" : reader["suppliername"].ToString();
                            oitem.brandname = reader["brandname"] == DBNull.Value ? "" : reader["brandname"].ToString();
                            oitem.patternname = reader["patternname"] == DBNull.Value ? "" : reader["patternname"].ToString();
                            oitem.stylename = reader["stylename"] == DBNull.Value ? "" : reader["stylename"].ToString();
                            oitem.sizename = reader["sizename"] == DBNull.Value ? "" : reader["sizename"].ToString();
                            oitem.materialname = reader["materialname"] == DBNull.Value ? "" : reader["materialname"].ToString();
                            oitem.typename = reader["typename"] == DBNull.Value ? "" : reader["typename"].ToString();
                            oitem.colourname = reader["colourname"] == DBNull.Value ? "" : reader["colourname"].ToString();
                            oitem.designid = reader["designid"] == DBNull.Value ? "" : reader["designid"].ToString();
                            oitem.hsncode = reader["hsncode"] == DBNull.Value ? "" : reader["hsncode"].ToString();
                            oitem.invoiceno = reader["invoiceno"] == DBNull.Value ? "" : reader["invoiceno"].ToString();
                            oitem.invoicedate = reader["invoicedate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["invoicedate"]);

                            oitem.itembuyingprice = reader["itembuyingprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["itembuyingprice"]);
                            oitem.itemibuyingprice = reader["itemibuyingprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["itemibuyingprice"]);
                            oitem.itemprice = reader["itemprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["itemprice"]);
                            oitem.itemdiscount = reader["itemdiscount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["itemdiscount"]);
                            oitem.itemmrp = reader["itemmrp"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["itemmrp"]);
                            oitem.isbit = reader["isbit"] == DBNull.Value ? false : Convert.ToBoolean(reader["isbit"]);
                        }

                        if (!Context.HasPermission(PermissionSet.showsellingvalue, InfyPOS.Common.Action.View))
                        {
                            oitem.mrp = reader["mrpprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["mrpprice"]);
                            oitem.price = reader["price"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["price"]);
                            oitem.discount = reader["discount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discount"]);
                            oitem.saleprice = oitem.price - oitem.discount;
                        }
                        if (!Context.HasPermission(PermissionSet.showvalue, InfyPOS.Common.Action.View))
                        {
                            oitem.ibuyingprice = reader["ibuyingprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["ibuyingprice"]);
                            oitem.buyingprice = reader["buyingprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["buyingprice"]);
                            oitem.taxamount = oitem.ibuyingprice - oitem.buyingprice;
                            if (!_reportcreteria.showsummary)
                                oitem.taxpercent = reader["purchasetaxpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["purchasetaxpercentage"]);
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
            return lst;
        }
        public List<OutwardReport> SearchOutward(WarehouseReportCreteria _reportcreteria)
        {
            List<OutwardReport> lst = new List<OutwardReport>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                NpgsqlCommand command = null;
                QueryBuilder qb = new QueryBuilder();

                if (_reportcreteria.withlr)
                {
                    _reportcreteria.showdetail = false;
                    command = null;
                    command = GetCommand(SQL_OutwardWithLR, conn);
                    command.Parameters.Add("status", NpgsqlTypes.NpgsqlDbType.Integer).Value = (_reportcreteria.pendinginward) ? 0 : 1;
                    command.Parameters.Add("lrentryno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = _reportcreteria.lrentryno;
                    command.Parameters.Add("supplier", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _reportcreteria.supplierid;
                    command.Parameters.Add("fromdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = _reportcreteria.fromdate.Date;
                    command.Parameters.Add("todate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = _reportcreteria.todate.AddDays(1);
                }
                else if (_reportcreteria.supplierid > 0 && _reportcreteria.showdetail)
                {

                    command = null;
                    command = GetCommand(SQL_OutwardSupplier, conn);
                    command.Parameters.Add("status", NpgsqlTypes.NpgsqlDbType.Integer).Value = (_reportcreteria.pendinginward) ? 0 : 1;
                    command.Parameters.Add("code", NpgsqlTypes.NpgsqlDbType.Varchar).Value = string.IsNullOrEmpty(_reportcreteria.packageno) ? "" : _reportcreteria.packageno;
                    command.Parameters.Add("supplier", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _reportcreteria.supplierid;
                    command.Parameters.Add("fromdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = _reportcreteria.fromdate.Date;
                    command.Parameters.Add("todate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = _reportcreteria.todate.AddDays(1);
                }
                else if (_reportcreteria.supplierid > 0 && !_reportcreteria.showdetail)
                {
                    command = null;
                    command = GetCommand(SQL_OutwardSupplierSummary, conn);
                    command.Parameters.Add("status", NpgsqlTypes.NpgsqlDbType.Integer).Value = (_reportcreteria.pendinginward) ? 0 : 1;
                    command.Parameters.Add("code", NpgsqlTypes.NpgsqlDbType.Varchar).Value = string.IsNullOrEmpty(_reportcreteria.packageno) ? "" : _reportcreteria.packageno;
                    command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _reportcreteria.locationid;
                    command.Parameters.Add("supplier", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _reportcreteria.supplierid;
                    command.Parameters.Add("fromdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = _reportcreteria.fromdate.Date;
                    command.Parameters.Add("todate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = _reportcreteria.todate.AddDays(1);
                }
                else if (_reportcreteria.showdetail)
                {
                    var query = "";
                    if (string.IsNullOrEmpty(_reportcreteria.packageno))
                        query = string.Format(SQL_OutwardItems, " and tolocationid=@locationid and fromlocationid=@fromlocationid and packedon>=@fromdate and packedon<@todate ");
                    else
                        query = string.Format(SQL_OutwardItems, " and bundles.code=@code ");

                    command = null;
                    command = GetCommand(query, conn);
                    command.Parameters.Add("status", NpgsqlTypes.NpgsqlDbType.Integer).Value = (_reportcreteria.pendinginward) ? 0 : 1;
                    command.Parameters.Add("code", NpgsqlTypes.NpgsqlDbType.Varchar).Value = string.IsNullOrEmpty(_reportcreteria.packageno) ? "" : _reportcreteria.packageno;
                }
                else
                {
                    command = null;
                    command = GetCommand(SQL_Outward, conn);
                    command.Parameters.Add("status", NpgsqlTypes.NpgsqlDbType.Integer).Value = (_reportcreteria.pendinginward) ? 0 : 1;
                    command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _reportcreteria.locationid;
                    command.Parameters.Add("fromdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = _reportcreteria.fromdate.Date;
                    command.Parameters.Add("todate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = _reportcreteria.todate.AddDays(1);
                }
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        OutwardReport oitem = new OutwardReport();
                        oitem.code = reader["code"] == DBNull.Value ? "" : reader["code"].ToString();
                        oitem.fromcompanyid = reader["fromcompanyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["fromcompanyid"].ToString());
                        oitem.tocompanyid = reader["tocompanyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["tocompanyid"].ToString());
                        oitem.fromlocation = reader["fromlocation"] == DBNull.Value ? "" : reader["fromlocation"].ToString();
                        oitem.tolocation = reader["tolocation"] == DBNull.Value ? "" : reader["tolocation"].ToString();
                        oitem.qty = reader["qty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["qty"]);
                        oitem.aqty = reader["acceptqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["acceptqty"]);
                        oitem.packedon = reader["packedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["packedon"]);
                        oitem.receivedon = reader["receivedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["receivedon"]);
                        oitem.receivedby = reader["receivedby"] == DBNull.Value ? "" : reader["receivedby"].ToString();

                        if (_reportcreteria.withlr)
                        {
                            oitem.lrentry = reader["lrentryno"] == DBNull.Value ? "" : reader["lrentryno"].ToString();
                            oitem.lrinvoice = reader["invoiceno"] == DBNull.Value ? "" : reader["invoiceno"].ToString();
                        }
                        if (_reportcreteria.showdetail)
                        {
                            oitem.productname = reader["printingname"] == DBNull.Value ? "" : reader["printingname"].ToString();
                            oitem.section = reader["section"] == DBNull.Value ? "" : reader["section"].ToString();
                            oitem.barcode = reader["barcode"] == DBNull.Value ? "" : reader["barcode"].ToString();
                            oitem.brandname = reader["brandname"] == DBNull.Value ? "" : reader["brandname"].ToString();
                            oitem.patternname = reader["patternname"] == DBNull.Value ? "" : reader["patternname"].ToString();
                            oitem.stylename = reader["stylename"] == DBNull.Value ? "" : reader["stylename"].ToString();
                            oitem.sizename = reader["sizename"] == DBNull.Value ? "" : reader["sizename"].ToString();
                            oitem.materialname = reader["materialname"] == DBNull.Value ? "" : reader["materialname"].ToString();
                            oitem.typename = reader["typename"] == DBNull.Value ? "" : reader["typename"].ToString();
                            oitem.colourname = reader["colourname"] == DBNull.Value ? "" : reader["colourname"].ToString();
                            oitem.designid = reader["designid"] == DBNull.Value ? "" : reader["designid"].ToString();
                            oitem.hsncode = reader["hsncode"] == DBNull.Value ? "" : reader["hsncode"].ToString();
                            oitem.invoiceno = reader["invoiceno"] == DBNull.Value ? "" : reader["invoiceno"].ToString();
                            oitem.invoicedate = reader["invoicedate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["invoicedate"]);

                            oitem.itembuyingprice = reader["itembuyingprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["itembuyingprice"]);
                            oitem.itemibuyingprice = reader["itemibuyingprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["itemibuyingprice"]);
                            oitem.itemprice = reader["itemprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["itemprice"]);
                            oitem.itemdiscount = reader["itemdiscount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["itemdiscount"]);
                            oitem.isbit = reader["isbit"] == DBNull.Value ? false : Convert.ToBoolean(reader["isbit"]);
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
            return lst;
        }

        public List<StockReport> SearchStock(StockReportCreteria _reportcreteria)
        {
            List<StockReport> lst = new List<StockReport>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                NpgsqlCommand command = null;

                QueryBuilder qb = new QueryBuilder();
                qb.CurrentQuery = SQL_Stock;

                qb.AddParameter("stock.isactive", true, "=");

                if (_reportcreteria.multicompany && _reportcreteria.companyid > 0)
                    qb.AddParameter("stock.companyid", _reportcreteria.companyid, "=");

                if (!string.IsNullOrEmpty(_reportcreteria.barcode) &&
                    !string.IsNullOrEmpty(InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_barcodeparser))
                {
                    _reportcreteria.barcode = InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.POSParser.UpdateBarcode(_reportcreteria.barcode);
                }

                if (!string.IsNullOrEmpty(_reportcreteria.barcode))
                {
                    if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_fixed_barcodes &&
                        !InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_hyper_uan)
                        qb.AddORParameter("stock.barcode", _reportcreteria.barcode, "=", "stock.serialno = @serialno ");
                    else
                        qb.AddParameter("stock.barcode", _reportcreteria.barcode, "=");
                }
                else if (_reportcreteria.barcodes != null && _reportcreteria.barcodes.Count > 0)
                {
                    var barcodelist = string.Join(",", _reportcreteria.barcodes.ConvertAll(e => "'" + WebAPI.Data.Utility.SafeSQL(e) + "'"));
                    if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_fixed_barcodes &&
                        !InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_hyper_uan)
                        qb.AddInParamer("stock.barcode", barcodelist, "stock.serialno in (" + barcodelist + ")");
                    else
                        qb.AddInParamer("stock.barcode", barcodelist);
                }

                if (_reportcreteria.barcodetype == 2)
                    qb.Add2Query(@"stock.serialno=''");
                else if (_reportcreteria.barcodetype == 3)
                    qb.Add2Query(@"stock.serialno!=''");

                if (_reportcreteria.productid > 0)
                    qb.AddParameter("stock.productid", _reportcreteria.productid, "=");
                if (_reportcreteria.sectionid > 0)
                    qb.AddParameter("products.salescounterid", _reportcreteria.sectionid, "=");
                if (_reportcreteria.productgroupid > 0)
                    qb.AddParameter("products.productgroup", _reportcreteria.productgroupid, "=");
                if (_reportcreteria.brandid > 0)
                    qb.AddParameter("items.brandid", _reportcreteria.brandid, "=");
                if (_reportcreteria.materialid > 0)
                    qb.AddParameter("items.materialid", _reportcreteria.materialid, "=");
                if (_reportcreteria.patternid > 0)
                    qb.AddParameter("items.patternid", _reportcreteria.patternid, "=");
                if (_reportcreteria.styleid > 0)
                    qb.AddParameter("items.styleid", _reportcreteria.styleid, "=");
                if (_reportcreteria.sizeid > 0)
                    qb.AddParameter("items.sizeid", _reportcreteria.sizeid, "=");
                if (_reportcreteria.typeid > 0)
                    qb.AddParameter("items.typeid", _reportcreteria.typeid, "=");
                if (_reportcreteria.colourid > 0)
                    qb.AddParameter("items.colourid", _reportcreteria.colourid, "=");
                if (_reportcreteria.sleeveid > 0)
                    qb.AddParameter("items.sleeveid", _reportcreteria.sleeveid, "=");
                if (_reportcreteria.fitid > 0)
                    qb.AddParameter("items.fitid", _reportcreteria.fitid, "=");
                if (_reportcreteria.supplierid > 0)
                    qb.AddParameter("lrentry.supplierid", _reportcreteria.supplierid, "=");
                if (!string.IsNullOrEmpty(_reportcreteria.designid))
                {
                    if (_reportcreteria.designid.IndexOf(",") > 0)
                    {
                        string designquery = string.Join(" or ", _reportcreteria.designid.Split(',').ToList().ConvertAll(x => "stock.designid ilike '" + WebAPI.Data.Utility.SafeSQL(x) + "'"));
                        qb.Add2Query("(" + designquery + ")");
                    }
                    else
                    {
                        qb.AddParameter("stock.designid", "%" + WebAPI.Data.Utility.SafeSQL(_reportcreteria.designid) + "%", " ilike ");
                    }
                }

                if (_reportcreteria.stockage == 1)
                    qb.Add2Query("EXTRACT(day from @current_date - lrinvoice.entrydate) < COALESCE(lrentry.stockholdingperiod,999)");
                else if (_reportcreteria.stockage == 2)
                    qb.AddParameter("EXTRACT(day from @current_date - lrinvoice.entrydate)", "stockage", 30, "<=");
                else if (_reportcreteria.stockage > 2)
                    qb.AddParameter("EXTRACT(day from @current_date - lrinvoice.entrydate)", "stockage", _reportcreteria.stockage, ">");

                if (_reportcreteria.removezerostock)
                {
                    qb.CurrentQuery += " and stock.qty-(stock.transfered+stock.journalqty+stock.damagedqty+stock.returnqty) > 0 and stock.hasstock=true ";
                }
                else if ((string.IsNullOrEmpty(_reportcreteria.barcode) && string.IsNullOrEmpty(_reportcreteria.designid) &&
                    (_reportcreteria.barcodes == null || _reportcreteria.barcodes.Count == 0)))
                {
                    if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.analyser_stock_withzerostock)
                        qb.CurrentQuery += " and stock.qty-(stock.transfered+stock.journalqty+stock.damagedqty+stock.returnqty) != 0";
                    else
                        qb.CurrentQuery += " and stock.qty-(stock.transfered+stock.journalqty+stock.damagedqty+stock.returnqty) > 0";
                }

                command = qb.GetCommand(conn);
                command.Parameters.AddWithValue("current_date", DateTimeNow.Date);
                if (!string.IsNullOrEmpty(_reportcreteria.barcode))
                    command.Parameters.AddWithValue("serialno", _reportcreteria.barcode);

                command.CommandTimeout = 10000;
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        StockReport oitem = new StockReport();
                        oitem.barcode = reader["barcode"] == DBNull.Value ? "" : reader["barcode"].ToString();
                        oitem.itemname = reader["printingname"] == DBNull.Value ? "" : reader["printingname"].ToString();
                        oitem.productgroupid = reader["productgroup"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productgroup"]);
                        oitem.sectionid = reader["salescounterid"] == DBNull.Value ? 0 : Convert.ToInt32(reader["salescounterid"]);
                        if (!Context.HasPermission(PermissionSet.showsellingvalue, InfyPOS.Common.Action.View))
                        {
                            oitem.price = reader["price"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["price"]);
                            oitem.dealerprice = reader["dealerprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["dealerprice"]);
                            oitem.discount = reader["discount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discount"]);
                            oitem.mrp = oitem.price - oitem.discount;
                        }
                        oitem.qty = reader["wstock"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["wstock"]);
                        oitem.iqty = reader["pqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["pqty"]);
                        if (!Context.HasPermission(PermissionSet.showvalue, InfyPOS.Common.Action.View))
                        {
                            oitem.taxable = reader["taxable"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["taxable"]);
                            oitem.taxvalue = reader["tax"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["tax"]);
                            oitem.buyingprice = reader["buyingprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["buyingprice"]);
                            oitem.ibuyingprice = reader["ibuyingprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["ibuyingprice"]);
                            if (oitem.ibuyingprice < oitem.buyingprice) oitem.ibuyingprice = oitem.buyingprice;
                            oitem.margin = oitem.price - oitem.discount - oitem.ibuyingprice;

                        }
                        oitem.productname = reader["productname"] == DBNull.Value ? "" : reader["productname"].ToString();
                        oitem.brandname = reader["brandname"] == DBNull.Value ? "" : reader["brandname"].ToString();
                        oitem.patternname = reader["patternname"] == DBNull.Value ? "" : reader["patternname"].ToString();
                        oitem.serialno = reader["serialno"] == DBNull.Value ? "" : reader["serialno"].ToString();
                        if (Context.HasPermission(PermissionSet.hidesupplier, InfyPOS.Common.Action.View))
                            oitem.suppliername = "hidden";
                        else
                            oitem.suppliername = reader["suppliername"] == DBNull.Value ? "" : reader["suppliername"].ToString();
                        oitem.stylename = reader["stylename"] == DBNull.Value ? "" : reader["stylename"].ToString();
                        oitem.sizename = reader["sizename"] == DBNull.Value ? "" : reader["sizename"].ToString();
                        oitem.materialname = reader["materialname"] == DBNull.Value ? "" : reader["materialname"].ToString();
                        oitem.typename = reader["typename"] == DBNull.Value ? "" : reader["typename"].ToString();
                        oitem.sleevename = reader["sleevename"] == DBNull.Value ? "" : reader["sleevename"].ToString();
                        oitem.fitname = reader["fitname"] == DBNull.Value ? "" : reader["fitname"].ToString();
                        oitem.pricetagname = reader["pricetagname"] == DBNull.Value ? "" : reader["pricetagname"].ToString();
                        oitem.colourname = reader["colourname"] == DBNull.Value ? "" : reader["colourname"].ToString();
                        oitem.designid = reader["designid"] == DBNull.Value ? "" : reader["designid"].ToString();
                        oitem.stockid = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        oitem.invoiceitemid = reader["invoiceitemid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["invoiceitemid"]);
                        oitem.hsncode = reader["hsncode"] == DBNull.Value ? "" : reader["hsncode"].ToString();
                        oitem.sellingmode = reader["sellingmode"] == DBNull.Value ? 0 : Convert.ToInt32(reader["sellingmode"]);
                        oitem.sizeid = reader["sizeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sizeid"]);
                        oitem.typeid = reader["typeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["typeid"]);
                        oitem.colourid = reader["colourid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["colourid"]);
                        oitem.materialid = reader["materialid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["materialid"]);
                        oitem.styleid = reader["styleid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["styleid"]);
                        oitem.brandid = reader["brandid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["brandid"]);
                        oitem.sleeveid = reader["sleeveid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sleeveid"]);
                        oitem.fitid = reader["fitid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["fitid"]);
                        oitem.patternid = reader["patternid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["patternid"]);
                        oitem.stockage = reader["age"] == DBNull.Value ? 0 : Convert.ToInt32(reader["age"]);
                        oitem.stockvalue = oitem.buyingprice * oitem.qty;
                        oitem.taxablevalue = oitem.taxable * oitem.qty;
                        oitem.salevalue = (oitem.price - oitem.discount) * oitem.qty;
                        oitem.itbuscode = reader["itbuscode"] == DBNull.Value ? "" : reader["itbuscode"].ToString();
                        oitem.isbit = reader["isbit"] == DBNull.Value ? false : Convert.ToBoolean(reader["isbit"]);

                        if (oitem.buyingprice > 0 && (oitem.price - oitem.discount - oitem.buyingprice) > 0)
                            oitem.marginpercentage = Math.Round(((oitem.price - oitem.discount - oitem.buyingprice) / oitem.buyingprice) * 100, 2);

                        if (oitem.stockage > 10000)
                        {
                            oitem.stockage = reader["age1"] == DBNull.Value ? 0 : Convert.ToInt32(reader["age1"]);
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



            if (_reportcreteria.barcodetype == 3 &&
                lst.All(x => !string.IsNullOrEmpty(x.serialno)))
            {
                List<StockReport> lstfilter = new List<StockReport>();
                foreach (var grpbarcode in lst.GroupBy(x => x.serialno))
                {
                    var nbarocde = JsonConvert.DeserializeObject<StockReport>(JsonConvert.SerializeObject(grpbarcode.First()));
                    nbarocde.uanstk = new List<StockReport.UANBarcodes>();
                    nbarocde.uanstk.AddRange(grpbarcode.OrderByDescending(x => x.stockid).ToList().ConvertAll(x => new StockReport.UANBarcodes()
                    {
                        barcode = x.barcode,
                        stockid = x.stockid,
                        salablegoodsid = x.salablegoodsid,
                        iqty = x.iqty,
                        ipurchaseqty = x.ipurchaseqty,
                        returnqty = x.returnqty,
                        poqty = x.poqty,
                        dqty = x.dqty,
                        oqty = x.oqty,
                        jqty = x.jqty,
                        qty = x.qty
                    }));
                    nbarocde.iqty = nbarocde.uanstk.Sum(x => x.iqty);
                    nbarocde.ipurchaseqty = nbarocde.uanstk.Sum(x => x.ipurchaseqty);
                    nbarocde.returnqty = nbarocde.uanstk.Sum(x => x.returnqty);
                    nbarocde.poqty = nbarocde.uanstk.Sum(x => x.poqty);
                    nbarocde.dqty = nbarocde.uanstk.Sum(x => x.dqty);
                    nbarocde.oqty = nbarocde.uanstk.Sum(x => x.oqty);
                    nbarocde.jqty = nbarocde.uanstk.Sum(x => x.jqty);
                    nbarocde.qty = nbarocde.uanstk.Sum(x => x.qty);
                    lstfilter.Add(nbarocde);
                }
                lst = lstfilter;
            }

            return lst;
        }

        public List<StockReport> SearchPurchaseItems(StockReportCreteria _reportcreteria)
        {
            string SQL_Barcodedetails = @"select  lrentry.lrentryno, lrinvoice.invoiceno, lrinvoice.entrydate, lrinvoice.invoicedate,itemdetail->>'serialno' serialno,
supplier.name suppliername, supcity.name suppliercity, agent.name agentname, agentcity.name agentcity, products.name as productname, products.sellingmode, lrinvoiceitems.purchasetax,
products.productgroup, products.salescounterid, lrinvoiceitems.designid, lrinvoiceitems.hsncode, products.salestaxid, lrinvoiceitems.rowexpense, lrinvoiceitems.buyingqty,
lrinvoiceitems.rowexpensevalue, lrinvoiceitems.basebuyingprice, lrinvoiceitems.buyingprice, lrinvoiceitems.purchasediscount, lrinvoiceitems.purchasediscountpercentage,
lrinvoiceitems.marginpercentage, purchasetaxpercentage, brand.name brandname, style.name stylename, pattern.name patternname, lrinvoiceitems.sellingprice,lrinvoiceitems.salediscount,
size.name sizename, colour.name colourname, type.name typename, material.name materialname, sleeve.name sleevename, fit.name fitname,lrinvoiceitems.mrp,lrinvoiceitems.dealerprice,
EXTRACT(day from @current_date - lrinvoice.entrydate) age, lrentry.stockholdingperiod, EXTRACT(day from @current_date - lrinvoice.entrydate) age1
from lrinvoiceitems
left outer join lrinvoice on lrinvoice.id=lrinvoiceitems.lrinvoiceid and lrinvoice.isactive=true
left outer join lrentry on lrentry.id = lrinvoice.lrid
left outer join items on items.id = lrinvoiceitems.itemid
left outer join products on products.id = lrinvoiceitems.productid
left outer join supplier on supplier.id = lrentry.supplierid
left outer join suppliercompany on suppliercompany.supplierid = supplier.id
left outer join agent on agent.id = lrentry.agentid
left outer join brand brand on brand.id = items.brandid
left outer join referencelist colour on colour.id = colourid
left outer join referencelist size on size.id = sizeid
left outer join referencelist style on style.id = styleid
left outer join referencelist pattern on pattern.id = patternid
left outer join referencelist material on material.id = materialid
left outer join referencelist type on type.id = typeid
left outer join referencelist sleeve on sleeve.id = sleeveid
left outer join referencelist fit on fit.id = fitid
left outer join referencelist supcity on supcity.id = suppliercompany.cityid
left outer join referencelist agentcity on agentcity.id = agent.cityid
";



            List<StockReport> lst = new List<StockReport>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                var system_marginonmarkdown = InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_marginonmarkdown;
                conn.Open();
                NpgsqlCommand command = null;

                QueryBuilder qb = new QueryBuilder();


                qb.CurrentQuery = SQL_Barcodedetails;

                // this field used to retrieve only deleted items. (Report name : Warehouse -> Purchase Item Details)
                if (_reportcreteria.isb2b)
                    qb.AddParameter("lrinvoiceitems.isactive", false, "=");
                else
                    qb.AddParameter("lrinvoiceitems.isactive", true, "=");
                if (_reportcreteria.removemfg)
                {
                    qb.Add2Query("(importreferencetype is null or importreferencetype !=1)");
                }
                if (_reportcreteria.removebarcoded)
                {
                    qb.Add2Query("lrinvoiceitems.sellableqty-lrinvoiceitems.returnqty=0");
                }
                if (_reportcreteria.locationid > 0)
                    qb.AddParameter("lrentry.entrylocationid", _reportcreteria.locationid, "=");

                if (_reportcreteria.multicompany && _reportcreteria.companyid > 0)
                    qb.AddParameter("lrinvoice.companyid", _reportcreteria.companyid, "=");

                if (_reportcreteria.productid > 0)
                    qb.AddParameter("lrinvoiceitems.productid", _reportcreteria.productid, "=");
                if (_reportcreteria.sectionid > 0)
                    qb.AddParameter("products.salescounterid", _reportcreteria.sectionid, "=");
                if (_reportcreteria.productgroupid > 0)
                    qb.AddParameter("products.productgroup", _reportcreteria.productgroupid, "=");
                if (_reportcreteria.brandid > 0)
                    qb.AddParameter("items.brandid", _reportcreteria.brandid, "=");
                if (_reportcreteria.materialid > 0)
                    qb.AddParameter("items.materialid", _reportcreteria.materialid, "=");
                if (_reportcreteria.patternid > 0)
                    qb.AddParameter("items.patternid", _reportcreteria.patternid, "=");
                if (_reportcreteria.styleid > 0)
                    qb.AddParameter("items.styleid", _reportcreteria.styleid, "=");
                if (_reportcreteria.sizeid > 0)
                    qb.AddParameter("items.sizeid", _reportcreteria.sizeid, "=");
                if (_reportcreteria.typeid > 0)
                    qb.AddParameter("items.typeid", _reportcreteria.typeid, "=");
                if (_reportcreteria.colourid > 0)
                    qb.AddParameter("items.colourid", _reportcreteria.colourid, "=");
                if (_reportcreteria.sleeveid > 0)
                    qb.AddParameter("items.sleeveid", _reportcreteria.sleeveid, "=");
                if (_reportcreteria.fitid > 0)
                    qb.AddParameter("items.fitid", _reportcreteria.fitid, "=");

                bool hasmin = false;
                if (_reportcreteria.supplierid > 0)
                    qb.AddParameter("lrentry.supplierid", _reportcreteria.supplierid, "=");

                if (!string.IsNullOrEmpty(_reportcreteria.lrentryno))
                {
                    qb.AddParameter("lrentry.lrentryno", _reportcreteria.lrentryno, "=");
                    hasmin = true;
                }

                if (!string.IsNullOrEmpty(_reportcreteria.invoiceno))
                {
                    qb.AddParameter("lrinvoice.invoiceno", _reportcreteria.invoiceno, "=");
                    hasmin = true;
                }

                if (_reportcreteria.frominvdate != DateTime.MinValue && _reportcreteria.toinvdate != DateTime.MinValue)
                {
                    qb.AddParameter("lrinvoice.invoicedate", "datefrom", _reportcreteria.frominvdate.Date, ">=");
                    qb.AddParameter("lrinvoice.invoicedate", "dateto", _reportcreteria.toinvdate.Date.AddDays(1), "<");
                    hasmin = true;
                }

                if (_reportcreteria.fromdate != DateTime.MinValue && _reportcreteria.todate != DateTime.MinValue)
                {
                    qb.AddParameter("lrinvoice.entrydate", "datefrom", _reportcreteria.fromdate.Date, ">=");
                    qb.AddParameter("lrinvoice.entrydate", "dateto", _reportcreteria.todate.Date.AddDays(1), "<");
                    hasmin = true;
                }
                if (!hasmin)
                    throw new Exception("Please provide minumum condition to search barcodewise details like entry date,invoice date,invoice no, lr entry no");

                qb.SortField = "lrinvoice.entrydate";

                command = qb.GetCommand(conn);
                command.Parameters.AddWithValue("current_date", DateTimeNow.Date);


                command.CommandTimeout = 10000;
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {

                        {
                            StockReport oitem = new StockReport();

                            oitem.suppliername = reader["suppliername"] == DBNull.Value ? "" : reader["suppliername"].ToString();
                            oitem.agentname = reader["agentname"] == DBNull.Value ? "" : reader["agentname"].ToString();
                            oitem.lrentryno = reader["lrentryno"] == DBNull.Value ? "" : reader["lrentryno"].ToString();
                            oitem.invoiceno = reader["invoiceno"] == DBNull.Value ? "" : reader["invoiceno"].ToString();
                            oitem.entrydate = reader["entrydate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["entrydate"]);
                            oitem.invoicedate = reader["invoicedate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["invoicedate"]);
                            oitem.itemname = reader["productname"] == DBNull.Value ? "" : reader["productname"].ToString();
                            oitem.productgroupid = reader["productgroup"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productgroup"]);
                            oitem.sectionid = reader["salescounterid"] == DBNull.Value ? 0 : Convert.ToInt32(reader["salescounterid"]);
                            oitem.rowexpense = reader["rowexpense"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["rowexpense"]);
                            oitem.rowexpensevalue = reader["rowexpensevalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["rowexpensevalue"]);
                            if (!Context.HasPermission(PermissionSet.showsellingvalue, InfyPOS.Common.Action.View))
                            {
                                oitem.staxpercent = reader["salestaxid"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["salestaxid"]);
                                oitem.price = reader["sellingprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["sellingprice"]);
                                oitem.dealerprice = reader["dealerprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["dealerprice"]);
                                oitem.discount = reader["salediscount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["salediscount"]);
                                oitem.mrp = reader["mrp"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["mrp"]);
                            }
                            oitem.qty = reader["buyingqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["buyingqty"]);
                            if (!Context.HasPermission(PermissionSet.showvalue, InfyPOS.Common.Action.View) && oitem.qty > 0)
                            {
                                oitem.basebuyingprice = reader["basebuyingprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["basebuyingprice"]);
                                oitem.pdiscout = reader["purchasediscount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["purchasediscount"]);
                                oitem.pdiscoutpercent = reader["purchasediscountpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["purchasediscountpercentage"]);
                                oitem.taxvalue = (reader["purchasetax"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["purchasetax"])) / oitem.qty;
                                oitem.taxable = reader["buyingprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["buyingprice"]);
                                oitem.buyingprice = oitem.taxable;
                                oitem.ibuyingprice = oitem.taxable + oitem.taxvalue;
                                if (oitem.ibuyingprice < oitem.buyingprice) oitem.ibuyingprice = oitem.buyingprice;
                            }
                            oitem.productname = reader["productname"] == DBNull.Value ? "" : reader["productname"].ToString();
                            oitem.brandname = reader["brandname"] == DBNull.Value ? "" : reader["brandname"].ToString();
                            oitem.patternname = reader["patternname"] == DBNull.Value ? "" : reader["patternname"].ToString();
                            oitem.serialno = reader["serialno"] == DBNull.Value ? "" : reader["serialno"].ToString();
                            if (Context.HasPermission(PermissionSet.hidesupplier, InfyPOS.Common.Action.View))
                                oitem.suppliername = "hidden";
                            else
                                oitem.suppliername = reader["suppliername"] == DBNull.Value ? "" : reader["suppliername"].ToString();
                            oitem.suppliercity = reader["suppliercity"] == DBNull.Value ? "" : reader["suppliercity"].ToString();
                            oitem.agentcity = reader["agentcity"] == DBNull.Value ? "" : reader["agentcity"].ToString();
                            oitem.stylename = reader["stylename"] == DBNull.Value ? "" : reader["stylename"].ToString();
                            oitem.sizename = reader["sizename"] == DBNull.Value ? "" : reader["sizename"].ToString();
                            oitem.materialname = reader["materialname"] == DBNull.Value ? "" : reader["materialname"].ToString();
                            oitem.typename = reader["typename"] == DBNull.Value ? "" : reader["typename"].ToString();
                            oitem.sleevename = reader["sleevename"] == DBNull.Value ? "" : reader["sleevename"].ToString();
                            oitem.fitname = reader["fitname"] == DBNull.Value ? "" : reader["fitname"].ToString();
                            oitem.colourname = reader["colourname"] == DBNull.Value ? "" : reader["colourname"].ToString();
                            oitem.designid = reader["designid"] == DBNull.Value ? "" : reader["designid"].ToString();
                            oitem.hsncode = reader["hsncode"] == DBNull.Value ? "" : reader["hsncode"].ToString();
                            oitem.sellingmode = reader["sellingmode"] == DBNull.Value ? 0 : Convert.ToInt32(reader["sellingmode"]);
                            oitem.stockage = reader["age"] == DBNull.Value ? 0 : Convert.ToInt32(reader["age"]);
                            oitem.ptaxpercent = reader["purchasetaxpercentage"] == DBNull.Value ? 0 : Convert.ToInt32(reader["purchasetaxpercentage"]);
                            oitem.stockvalue = oitem.ibuyingprice * oitem.qty;
                            oitem.taxablevalue = oitem.buyingprice * oitem.qty;
                            oitem.salevalue = (oitem.price - oitem.discount) * oitem.qty;

                            if (oitem.ibuyingprice > 0 && (oitem.price - oitem.discount - oitem.ibuyingprice) > 0)
                            {
                                oitem.margin = Math.Round((oitem.price - oitem.discount - oitem.ibuyingprice), 2);
                                if (system_marginonmarkdown)
                                {
                                    oitem.marginpercentage = Math.Round(100 - ((oitem.ibuyingprice / (oitem.price - oitem.discount)) * 100), 2);
                                }
                                else
                                {
                                    oitem.marginpercentage = Math.Round(((oitem.price - oitem.discount - oitem.ibuyingprice) / oitem.ibuyingprice) * 100, 2);
                                }
                            }
                            if (oitem.ibuyingprice > 0 && (oitem.dealerprice - oitem.ibuyingprice) > 0)
                            {
                                oitem.dealermargin = Math.Round((oitem.dealerprice - oitem.ibuyingprice), 2);
                                if (system_marginonmarkdown)
                                {
                                    oitem.dealermarginpercentage = Math.Round(100 - ((oitem.ibuyingprice / (oitem.dealerprice)) * 100), 2);
                                }
                                else
                                {
                                    oitem.dealermarginpercentage = Math.Round(((oitem.dealerprice - oitem.ibuyingprice) / oitem.ibuyingprice) * 100, 2);
                                }
                            }
                            if (oitem.stockage > 10000)
                            {
                                oitem.stockage = reader["age1"] == DBNull.Value ? 0 : Convert.ToInt32(reader["age1"]);
                            }
                            lst.Add(oitem);
                        }
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
        public List<StockReport> SearchBarcodeDetails(StockReportCreteria _reportcreteria)
        {

            string SQL_Purchasedetails = @"select lrinvoiceitems.id,lrinvoiceitems.buyingqty,lrentry.lrentryno, lrentry.stockholdingperiod, lrinvoice.invoiceno, lrinvoice.entrydate, 
lrinvoice.invoicedate,supplier.name suppliername, suppliercompany.tinanddate gstno, supcity.name suppliercity, sourcesupplier.name sourcesuppliername, agent.name agentname, agentcity.name agentcity, 
stock.barcode, stock.serialno, products.name as productname, products.sellingmode, products.productgroup, products.salescounterid, 
stock.designid, stock.hsncode, products.salestaxid, lrinvoiceitems.rowexpense, lrinvoiceitems.rowexpensevalue,
lrinvoiceitems.basebuyingprice, lrinvoiceitems.buyingprice, lrinvoiceitems.purchasediscount, lrinvoiceitems.purchasediscountpercentage,
lrinvoiceitems.marginpercentage, purchasetaxpercentage, stock.id, stock.invoiceitemid, stock.productid,stock.companyid,
stock.buyingprice taxable,stock.ibuyingprice costprice, stock.price, stock.discount, stock.dealerprice, stock.displaymrp,
brand.name brandname, style.name stylename, pattern.name patternname, size.name sizename, colour.name colourname, type.name typename, 
material.name materialname, sleeve.name sleevename, fit.name fitname, pricetag.name pricetagname, 
stock.transfered, stock.returnqty, stock.damagedqty, stock.journalqty, stock.stockdate,
barcodeid.name barcodeidname, entrylocation.name entrylocationname, lrinvoice.isinterstatesale,
stock.itemid,stock.supplierid,stock.qty,
EXTRACT(day from @current_date - lrinvoice.entrydate) age, lrentry.stockholdingperiod, EXTRACT(day from @current_date - stock.stockdate) age1
from stock
left outer join salablegoods on salablegoods.stockid = stock.id and salablegoods.isactive = true
left outer join lrinvoiceitems on lrinvoiceitems.id = stock.invoiceitemid and lrinvoiceitems.isactive = true 
left outer join lrinvoice on lrinvoice.id=lrinvoiceitems.lrinvoiceid and lrinvoice.isactive=true
left outer join lrentry on lrentry.id = lrinvoice.lrid
left outer join items on items.id = stock.itemid
left outer join products on products.id = stock.productid
left outer join supplier on supplier.id = stock.supplierid
left outer join suppliercompany on suppliercompany.supplierid = supplier.id
left outer join supplier sourcesupplier on sourcesupplier.id = stock.srcsupplierid
left outer join agent on agent.id = lrentry.agentid
left outer join brand brand on brand.id = items.brandid
left outer join referencelist colour on colour.id = colourid
left outer join referencelist size on size.id = sizeid
left outer join referencelist style on style.id = styleid
left outer join referencelist pattern on pattern.id = patternid
left outer join referencelist material on material.id = materialid
left outer join referencelist type on type.id = typeid
left outer join referencelist sleeve on sleeve.id = sleeveid
left outer join referencelist fit on fit.id = fitid
left outer join referencelist pricetag on pricetag.id = stock.pricetagid
left outer join referencelist supcity on supcity.id = suppliercompany.cityid
left outer join referencelist agentcity on agentcity.id = agent.cityid
left outer join referencelist barcodeid on barcodeid.id = products.barcodeid
left outer join referencelist entrylocation on entrylocation.id = lrentry.entrylocationid
";
            string SQL_Barcodedetails = @"select  lrentry.lrentryno, lrentry.stockholdingperiod, lrinvoice.invoiceno, lrinvoice.entrydate, 
lrinvoice.invoicedate,
supplier.name suppliername, suppliercompany.tinanddate gstno, supcity.name suppliercity, sourcesupplier.name sourcesuppliername, agent.name agentname, agentcity.name agentcity, 
stock.barcode, stock.serialno, products.name as productname, products.sellingmode, products.productgroup, products.salescounterid, 
stock.designid, stock.hsncode, products.salestaxid, lrinvoiceitems.rowexpense, lrinvoiceitems.rowexpensevalue,
lrinvoiceitems.basebuyingprice, lrinvoiceitems.buyingprice, lrinvoiceitems.purchasediscount, lrinvoiceitems.purchasediscountpercentage,
lrinvoiceitems.marginpercentage, purchasetaxpercentage, stock.id, stock.invoiceitemid, stock.productid,stock.companyid,
stock.buyingprice taxable,stock.ibuyingprice costprice, stock.price, stock.discount, stock.dealerprice, stock.displaymrp,
brand.name brandname, style.name stylename, pattern.name patternname, size.name sizename, colour.name colourname, type.name typename, 
material.name materialname, sleeve.name sleevename, fit.name fitname, pricetag.name pricetagname, 
stock.transfered, stock.returnqty, stock.damagedqty, stock.journalqty, stock.stockdate,
barcodeid.name barcodeidname, entrylocation.name entrylocationname, lrinvoice.isinterstatesale,
stock.qty-(stock.transfered+stock.journalqty+stock.damagedqty+stock.returnqty) wstock,stock.itemid,stock.supplierid,
EXTRACT(day from @current_date - lrinvoice.entrydate) age, lrentry.stockholdingperiod, EXTRACT(day from @current_date - stock.stockdate) age1,
sum(stock.qty) purchaseqty, sum(salablegoods.iqty) ipurchaseqty, sum(stock.returnqty) purchasereturnqty, sum(salablegoods.oqty) saleqty, sum(salablegoods.jqty) jqty, 
sum(salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty)) retailstock
from stock
left outer join salablegoods on salablegoods.stockid = stock.id and salablegoods.isactive = true
left outer join lrinvoiceitems on lrinvoiceitems.id = stock.invoiceitemid and lrinvoiceitems.isactive = true 
left outer join lrinvoice on lrinvoice.id=lrinvoiceitems.lrinvoiceid and lrinvoice.isactive=true
left outer join lrentry on lrentry.id = lrinvoice.lrid
left outer join items on items.id = stock.itemid
left outer join products on products.id = stock.productid
left outer join supplier on supplier.id = stock.supplierid
left outer join suppliercompany on suppliercompany.supplierid = supplier.id
left outer join supplier sourcesupplier on sourcesupplier.id = stock.srcsupplierid
left outer join agent on agent.id = lrentry.agentid
left outer join brand brand on brand.id = items.brandid
left outer join referencelist colour on colour.id = colourid
left outer join referencelist size on size.id = sizeid
left outer join referencelist style on style.id = styleid
left outer join referencelist pattern on pattern.id = patternid
left outer join referencelist material on material.id = materialid
left outer join referencelist type on type.id = typeid
left outer join referencelist sleeve on sleeve.id = sleeveid
left outer join referencelist fit on fit.id = fitid
left outer join referencelist pricetag on pricetag.id = stock.pricetagid
left outer join referencelist supcity on supcity.id = suppliercompany.cityid
left outer join referencelist agentcity on agentcity.id = agent.cityid
left outer join referencelist barcodeid on barcodeid.id = products.barcodeid
left outer join referencelist entrylocation on entrylocation.id = lrentry.entrylocationid
";

            string SQL_BarcodeSummary = @"select  supplier.name suppliername, brand.name brandname, products.salescounterid, products.name as productname, 
stock.designid, size.name sizename, colour.name colourname, stock.ibuyingprice costprice, supcity.name suppliercity, agent.name agentname,
lrinvoiceitems.basebuyingprice, lrinvoiceitems.buyingprice, lrinvoice.entrydate, stock.price,
sum(stock.qty-(stock.transfered+stock.journalqty+stock.damagedqty+stock.returnqty)) wstock, 
sum(stock.qty) purchaseqty, sum(salablegoods.iqty) ipurchaseqty, sum(stock.returnqty) purchasereturnqty, sum(stock.journalqty) journalqty, 
sum(salablegoods.oqty) saleqty, sum(salablegoods.jqty) jqty, 
sum(salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty)) retailstock,
supplier.id supplierid,stock.productid,items.brandid,items.colourid,items.sizeid
from stock
left outer join salablegoods on salablegoods.stockid = stock.id and salablegoods.isactive = true
left outer join lrinvoiceitems on lrinvoiceitems.id = stock.invoiceitemid and lrinvoiceitems.isactive = true 
left outer join lrinvoice on lrinvoice.id=lrinvoiceitems.lrinvoiceid and lrinvoice.isactive=true
left outer join lrentry on lrentry.id = lrinvoice.lrid
left outer join agent on agent.id = lrentry.agentid
left outer join items on items.id = stock.itemid
left outer join products on products.id = stock.productid
left outer join supplier on supplier.id = stock.supplierid
left outer join suppliercompany on suppliercompany.supplierid = supplier.id
left outer join brand brand on brand.id = items.brandid
left outer join referencelist colour on colour.id = colourid
left outer join referencelist size on size.id = sizeid 
left outer join referencelist supcity on supcity.id = suppliercompany.cityid ";



            List<StockReport> lst = new List<StockReport>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                var system_marginonmarkdown = InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_marginonmarkdown;
                conn.Open();
                NpgsqlCommand command = null;

                QueryBuilder qb = new QueryBuilder();


                if (_reportcreteria.mode == 1)
                    qb.CurrentQuery = SQL_BarcodeSummary;
                else if (_reportcreteria.mode == 2)
                    qb.CurrentQuery = SQL_Purchasedetails;
                else
                    qb.CurrentQuery = SQL_Barcodedetails;
                qb.AddParameter("stock.isactive", true, "=");

                if (_reportcreteria.multicompany && _reportcreteria.companyid > 0)
                    qb.AddParameter("stock.companyid", _reportcreteria.companyid, "=");

                if (!string.IsNullOrEmpty(_reportcreteria.barcode) &&
                !string.IsNullOrEmpty(InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_barcodeparser))
                {
                    _reportcreteria.barcode = InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.POSParser.UpdateBarcode(_reportcreteria.barcode);
                }


                if (!string.IsNullOrEmpty(_reportcreteria.barcode))
                {
                    if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_fixed_barcodes &&
                        !InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_hyper_uan)
                        qb.AddORParameter("stock.barcode", _reportcreteria.barcode, "=", "stock.serialno = @serialno ");
                    else
                        qb.AddParameter("stock.barcode", _reportcreteria.barcode, "=");
                }
                else if (_reportcreteria.barcodes != null && _reportcreteria.barcodes.Count > 0)
                {
                    var barcodelist = string.Join(",", _reportcreteria.barcodes.ConvertAll(e => "'" + WebAPI.Data.Utility.SafeSQL(e) + "'"));
                    if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_fixed_barcodes &&
                        !InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_hyper_uan)
                        qb.AddInParamer("stock.barcode", barcodelist, "stock.serialno in (" + barcodelist + ")");
                    else
                        qb.AddInParamer("stock.barcode", barcodelist);
                }
                if (_reportcreteria.productid > 0)
                    qb.AddParameter("stock.productid", _reportcreteria.productid, "=");
                if (_reportcreteria.sectionid > 0)
                    qb.AddParameter("products.salescounterid", _reportcreteria.sectionid, "=");
                if (_reportcreteria.productgroupid > 0)
                    qb.AddParameter("products.productgroup", _reportcreteria.productgroupid, "=");
                if (_reportcreteria.brandid > 0)
                    qb.AddParameter("items.brandid", _reportcreteria.brandid, "=");
                if (_reportcreteria.materialid > 0)
                    qb.AddParameter("items.materialid", _reportcreteria.materialid, "=");
                if (_reportcreteria.patternid > 0)
                    qb.AddParameter("items.patternid", _reportcreteria.patternid, "=");
                if (_reportcreteria.styleid > 0)
                    qb.AddParameter("items.styleid", _reportcreteria.styleid, "=");
                if (_reportcreteria.sizeid > 0)
                    qb.AddParameter("items.sizeid", _reportcreteria.sizeid, "=");
                if (_reportcreteria.typeid > 0)
                    qb.AddParameter("items.typeid", _reportcreteria.typeid, "=");
                if (_reportcreteria.colourid > 0)
                    qb.AddParameter("items.colourid", _reportcreteria.colourid, "=");
                if (_reportcreteria.sleeveid > 0)
                    qb.AddParameter("items.sleeveid", _reportcreteria.sleeveid, "=");
                if (_reportcreteria.fitid > 0)
                    qb.AddParameter("items.fitid", _reportcreteria.fitid, "=");
                if (_reportcreteria.supplierid > 0)
                    qb.AddParameter("lrentry.supplierid", _reportcreteria.supplierid, "=");
                if (_reportcreteria.sourcesupplierid > 0)
                    qb.AddParameter("stock.srcsupplierid", _reportcreteria.sourcesupplierid, "=");
                if (!string.IsNullOrEmpty(_reportcreteria.lrentryno))
                    qb.AddParameter("lrentry.lrentryno", _reportcreteria.lrentryno, "=");
                if (!string.IsNullOrEmpty(_reportcreteria.designid))
                {
                    if (_reportcreteria.designid.IndexOf(",") > 0)
                    {
                        string designquery = string.Join(" or ", _reportcreteria.designid.Split(',').ToList().ConvertAll(x => "stock.designid ilike '" + WebAPI.Data.Utility.SafeSQL(x) + "'"));
                        qb.Add2Query("(" + designquery + ")");
                    }
                    else
                    {
                        qb.AddParameter("stock.designid", "%" + WebAPI.Data.Utility.SafeSQL(_reportcreteria.designid) + "%", " ilike ");
                    }
                }
                if (_reportcreteria.fromdate != DateTime.MinValue && _reportcreteria.todate != DateTime.MinValue)
                {
                    qb.AddParameter("lrinvoice.entrydate", "datefrom", _reportcreteria.fromdate.Date, ">=");
                    qb.AddParameter("lrinvoice.entrydate", "dateto", _reportcreteria.todate.Date.AddDays(1), "<");
                }
                if (_reportcreteria.frominvdate != DateTime.MinValue && _reportcreteria.toinvdate != DateTime.MinValue)
                {
                    qb.AddParameter("lrinvoice.invoicedate", "invdatefrom", _reportcreteria.frominvdate.Date, ">=");
                    qb.AddParameter("lrinvoice.invoicedate", "invdateto", _reportcreteria.toinvdate.Date.AddDays(1), "<");
                }
                if (_reportcreteria.frompricingdate != DateTime.MinValue && _reportcreteria.topricingdate != DateTime.MinValue)
                {
                    qb.AddParameter("stock.stockdate", "pricingdatefrom", _reportcreteria.frompricingdate.Date, ">=");
                    qb.AddParameter("stock.stockdate", "pricingdateto", _reportcreteria.topricingdate.Date.AddDays(1), "<");
                }
                if (_reportcreteria.agentid > 0)
                    qb.AddParameter("lrentry.agentid", _reportcreteria.agentid, "=");
                if (_reportcreteria.suppliercityid > 0)
                    qb.AddParameter("suppliercompany.cityid", _reportcreteria.suppliercityid, "=");

                if (_reportcreteria.stockage == 1)
                    qb.Add2Query("EXTRACT(day from @current_date - lrinvoice.entrydate) < COALESCE(lrentry.stockholdingperiod,999)");
                else if (_reportcreteria.stockage == 2)
                    qb.AddParameter("EXTRACT(day from @current_date - lrinvoice.entrydate)", "stockage", 30, "<=");
                else if (_reportcreteria.stockage > 2)
                    qb.AddParameter("EXTRACT(day from @current_date - lrinvoice.entrydate)", "stockage", _reportcreteria.stockage, ">");

                if (_reportcreteria.mode == 1)
                {
                    qb.CurrentQuery += @" group by supplier.name, brand.name, products.salescounterid, supcity.name, agent.name,
products.name, stock.designid, size.name, colour.name, stock.qty-(stock.transfered+stock.journalqty+stock.damagedqty+stock.returnqty), stock.ibuyingprice,
lrinvoiceitems.basebuyingprice, lrinvoiceitems.buyingprice, lrinvoice.entrydate, stock.price,
supplier.id,stock.productid,items.brandid,items.colourid,items.sizeid";

                    qb.SortField = "lrinvoice.entrydate desc";
                }
                else if (_reportcreteria.mode == 2)
                {
                    qb.SortField = "lrinvoice.entrydate desc";
                }
                else
                {
                    qb.CurrentQuery += @" group by lrentry.lrentryno, lrentry.stockholdingperiod, lrinvoice.invoiceno, lrinvoice.entrydate, lrinvoice.invoicedate,
supplier.name, suppliercompany.tinanddate, supcity.name, sourcesupplier.name, agent.name, agentcity.name, stock.barcode, stock.serialno, products.name, products.sellingmode, products.productgroup, products.salescounterid, 
stock.designid, stock.hsncode, products.salestaxid, lrinvoiceitems.rowexpense, lrinvoiceitems.rowexpensevalue,
lrinvoiceitems.basebuyingprice, lrinvoiceitems.buyingprice, lrinvoiceitems.purchasediscount, lrinvoiceitems.purchasediscountpercentage,
lrinvoiceitems.marginpercentage, purchasetaxpercentage, stock.id, stock.invoiceitemid,stock.itemid,stock.supplierid,
stock.buyingprice,stock.ibuyingprice, stock.price, stock.discount, stock.dealerprice, stock.displaymrp,
brand.name, style.name, pattern.name, size.name, colour.name, type.name, stock.productid,stock.companyid,
material.name, sleeve.name, fit.name, pricetag.name, stock.stockdate, barcodeid.name, entrylocation.name,lrinvoice.isinterstatesale,
stock.qty, stock.transfered, stock.returnqty, stock.damagedqty, stock.journalqty,
stock.qty-(stock.transfered+stock.journalqty+stock.damagedqty+stock.returnqty),
EXTRACT(day from @current_date - lrinvoice.entrydate), lrentry.stockholdingperiod, EXTRACT(day from @current_date - stock.stockdate)
";

                    qb.SortField = "lrinvoice.entrydate";
                }
                command = qb.GetCommand(conn);
                command.Parameters.AddWithValue("current_date", DateTimeNow.Date);

                if (!string.IsNullOrEmpty(_reportcreteria.barcode))
                    command.Parameters.AddWithValue("serialno", _reportcreteria.barcode);

                command.CommandTimeout = 10000;
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (_reportcreteria.mode == 1)
                        {
                            StockReport oitem = null;

                            string suppliername = reader["suppliername"] == DBNull.Value ? "-" : reader["suppliername"].ToString();
                            string productname = reader["productname"] == DBNull.Value ? "-" : reader["productname"].ToString();
                            string brandname = reader["brandname"] == DBNull.Value ? "-" : reader["brandname"].ToString();
                            long sectionid = reader["salescounterid"] == DBNull.Value ? 0 : Convert.ToInt32(reader["salescounterid"]);
                            string colourname = reader["colourname"] == DBNull.Value ? "-" : reader["colourname"].ToString();
                            string designid = reader["designid"] == DBNull.Value ? "-" : reader["designid"].ToString();
                            string sizename = reader["sizename"] == DBNull.Value ? "-" : reader["sizename"].ToString();
                            string suppliercityname = reader["suppliercity"] == DBNull.Value ? "-" : reader["suppliercity"].ToString();
                            string agentname = reader["agentname"] == DBNull.Value ? "-" : reader["agentname"].ToString();

                            if (lst.Exists(e => e.suppliername == suppliername && e.productname == productname
                            && e.brandname == brandname && e.sectionid == sectionid && e.colourname == colourname
                            && e.designid == designid && e.sizename == sizename
                            && e.suppliercity == suppliercityname && e.agentname == agentname))
                            {
                                oitem = lst.Find(e => e.suppliername == suppliername && e.productname == productname
                            && e.brandname == brandname && e.sectionid == sectionid && e.colourname == colourname
                            && e.designid == designid && e.sizename == sizename
                            && e.suppliercity == suppliercityname && e.agentname == agentname);

                                oitem.iqty += reader["purchaseqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["purchaseqty"]);
                                oitem.returnqty += reader["purchasereturnqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["purchasereturnqty"]);
                                oitem.whqty += reader["wstock"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["wstock"]);
                                oitem.oqty += reader["saleqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["saleqty"]);
                                oitem.jqty += reader["jqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["jqty"]);
                                oitem.jqty += reader["journalqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["journalqty"]);
                                oitem.retailqty += reader["retailstock"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["retailstock"]);
                                oitem.qty = oitem.whqty + oitem.retailqty;

                                if (oitem.iqty > 0 && oitem.oqty <= 0)
                                    oitem.stockpercentage = 100;
                                else if (oitem.iqty > 0 && oitem.qty > 0)
                                    oitem.stockpercentage = Math.Round(oitem.qty / oitem.iqty * 100, 2);
                            }
                            else
                            {
                                oitem = new StockReport();
                                oitem.suppliername = suppliername.Trim();
                                oitem.productname = productname.Trim();
                                oitem.brandname = brandname.Trim();
                                oitem.sectionid = sectionid;
                                oitem.colourname = colourname.Trim();
                                oitem.designid = designid.Trim();
                                oitem.sizename = sizename.Trim();
                                oitem.designid = designid.Trim();
                                oitem.sizename = sizename.Trim();
                                oitem.suppliercity = suppliercityname.Trim();
                                oitem.agentname = agentname.Trim();

                                oitem.supplierid = reader["supplierid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["supplierid"]);
                                oitem.productid = reader["productid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productid"]);
                                oitem.brandid = reader["brandid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["brandid"]);
                                oitem.colourid = reader["colourid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["colourid"]);
                                oitem.sizeid = reader["sizeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sizeid"]);

                                oitem.iqty = reader["purchaseqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["purchaseqty"]);
                                oitem.ipurchaseqty = reader["ipurchaseqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["ipurchaseqty"]);
                                if (oitem.ipurchaseqty > 0 && oitem.ipurchaseqty < oitem.iqty)
                                    oitem.iqty = reader["ipurchaseqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["ipurchaseqty"]);

                                oitem.returnqty = reader["purchasereturnqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["purchasereturnqty"]);
                                oitem.whqty = reader["wstock"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["wstock"]);
                                oitem.oqty = reader["saleqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["saleqty"]);
                                oitem.jqty = reader["journalqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["journalqty"]);
                                oitem.jqty += reader["jqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["jqty"]);
                                oitem.retailqty = reader["retailstock"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["retailstock"]);
                                oitem.qty = oitem.whqty + oitem.retailqty;
                                oitem.price = reader["price"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["price"]);
                                if (!Context.HasPermission(PermissionSet.showvalue, InfyPOS.Common.Action.View))
                                {
                                    oitem.ibuyingprice = reader["costprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["costprice"]);
                                    oitem.basebuyingprice = reader["basebuyingprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["basebuyingprice"]);
                                    oitem.buyingprice = reader["buyingprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["buyingprice"]);
                                }

                                if (oitem.iqty > 0 && oitem.oqty <= 0)
                                    oitem.stockpercentage = 100;
                                else if (oitem.iqty > 0 && oitem.qty > 0)
                                    oitem.stockpercentage = Math.Round(oitem.qty / oitem.iqty * 100, 2);

                                lst.Add(oitem);
                            }

                        }
                        else if (_reportcreteria.mode == 2)
                        {
                            StockReport oitem = new StockReport();
                            oitem.suppliername = reader["suppliername"] == DBNull.Value ? "" : reader["suppliername"].ToString();
                            oitem.suppliergstno = reader["gstno"] == DBNull.Value ? "" : reader["gstno"].ToString();
                            oitem.sourcesuppliername = reader["sourcesuppliername"] == DBNull.Value ? "" : reader["sourcesuppliername"].ToString();
                            oitem.agentname = reader["agentname"] == DBNull.Value ? "" : reader["agentname"].ToString();
                            oitem.lrentryno = reader["lrentryno"] == DBNull.Value ? "" : reader["lrentryno"].ToString();
                            oitem.invoiceno = reader["invoiceno"] == DBNull.Value ? "" : reader["invoiceno"].ToString();
                            oitem.entrydate = reader["entrydate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["entrydate"]);
                            oitem.invoicedate = reader["invoicedate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["invoicedate"]);
                            oitem.stockdate = reader["stockdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["stockdate"]);
                            oitem.barcode = reader["barcode"] == DBNull.Value ? "" : reader["barcode"].ToString();
                            oitem.itemname = reader["productname"] == DBNull.Value ? "" : reader["productname"].ToString();
                            oitem.barcodeidname = reader["barcodeidname"] == DBNull.Value ? "" : reader["barcodeidname"].ToString();
                            oitem.entrylocationname = reader["entrylocationname"] == DBNull.Value ? "" : reader["entrylocationname"].ToString();
                            oitem.isinterstatesale = reader["isinterstatesale"] == DBNull.Value ? false : Convert.ToBoolean(reader["isinterstatesale"]);

                            oitem.itemid = reader["itemid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["itemid"]);
                            oitem.supplierid = reader["supplierid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["supplierid"]);
                            oitem.productid = reader["productid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productid"]);
                            oitem.companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"]);

                            oitem.productgroupid = reader["productgroup"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productgroup"]);
                            oitem.sectionid = reader["salescounterid"] == DBNull.Value ? 0 : Convert.ToInt32(reader["salescounterid"]);
                            oitem.rowexpense = reader["rowexpense"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["rowexpense"]);
                            oitem.rowexpensevalue = reader["rowexpensevalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["rowexpensevalue"]);
                            if (!Context.HasPermission(PermissionSet.showsellingvalue, InfyPOS.Common.Action.View))
                            {
                                oitem.staxpercent = reader["salestaxid"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["salestaxid"]);
                                oitem.price = reader["price"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["price"]);
                                oitem.dealerprice = reader["dealerprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["dealerprice"]);
                                oitem.discount = reader["discount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discount"]);
                                oitem.mrp = reader["displaymrp"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["displaymrp"]);
                            }
                            oitem.iqty = reader["buyingqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["buyingqty"]);
                            oitem.qty = reader["qty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["qty"]);
                            if (!Context.HasPermission(PermissionSet.showvalue, InfyPOS.Common.Action.View))
                            {
                                oitem.basebuyingprice = reader["basebuyingprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["basebuyingprice"]);
                                oitem.pdiscout = reader["purchasediscount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["purchasediscount"]);
                                oitem.pdiscoutpercent = reader["purchasediscountpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["purchasediscountpercentage"]);
                                oitem.taxable = reader["taxable"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["taxable"]);
                                oitem.buyingprice = reader["buyingprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["buyingprice"]);
                                oitem.ibuyingprice = reader["costprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["costprice"]);
                                if (oitem.ibuyingprice < oitem.buyingprice) oitem.ibuyingprice = oitem.buyingprice;
                                //oitem.margin = oitem.price - oitem.discount - oitem.ibuyingprice;
                                oitem.taxvalue = oitem.ibuyingprice - oitem.buyingprice;
                            }
                            oitem.productname = reader["productname"] == DBNull.Value ? "" : reader["productname"].ToString();
                            oitem.brandname = reader["brandname"] == DBNull.Value ? "" : reader["brandname"].ToString();
                            oitem.patternname = reader["patternname"] == DBNull.Value ? "" : reader["patternname"].ToString();
                            oitem.serialno = reader["serialno"] == DBNull.Value ? "" : reader["serialno"].ToString();
                            if (Context.HasPermission(PermissionSet.hidesupplier, InfyPOS.Common.Action.View))
                                oitem.suppliername = "hidden";
                            else
                                oitem.suppliername = reader["suppliername"] == DBNull.Value ? "" : reader["suppliername"].ToString();
                            //oitem.suppliercity = reader["suppliercity"] == DBNull.Value ? "" : reader["suppliercity"].ToString();
                            //oitem.agentcity = reader["agentcity"] == DBNull.Value ? "" : reader["agentcity"].ToString();
                            oitem.stylename = reader["stylename"] == DBNull.Value ? "" : reader["stylename"].ToString();
                            oitem.sizename = reader["sizename"] == DBNull.Value ? "" : reader["sizename"].ToString();
                            oitem.materialname = reader["materialname"] == DBNull.Value ? "" : reader["materialname"].ToString();
                            oitem.typename = reader["typename"] == DBNull.Value ? "" : reader["typename"].ToString();
                            oitem.sleevename = reader["sleevename"] == DBNull.Value ? "" : reader["sleevename"].ToString();
                            oitem.fitname = reader["fitname"] == DBNull.Value ? "" : reader["fitname"].ToString();
                            oitem.pricetagname = reader["pricetagname"] == DBNull.Value ? "" : reader["pricetagname"].ToString();
                            oitem.colourname = reader["colourname"] == DBNull.Value ? "" : reader["colourname"].ToString();
                            oitem.designid = reader["designid"] == DBNull.Value ? "" : reader["designid"].ToString();
                            oitem.stockid = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                            oitem.invoiceitemid = reader["invoiceitemid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["invoiceitemid"]);
                            oitem.hsncode = reader["hsncode"] == DBNull.Value ? "" : reader["hsncode"].ToString();
                            //oitem.sellingmode = reader["sellingmode"] == DBNull.Value ? 0 : Convert.ToInt32(reader["sellingmode"]);
                            //oitem.stockage = reader["age"] == DBNull.Value ? 0 : Convert.ToInt32(reader["age"]);
                            oitem.ptaxpercent = reader["purchasetaxpercentage"] == DBNull.Value ? 0 : Convert.ToInt32(reader["purchasetaxpercentage"]);
                            oitem.stockvalue = oitem.ibuyingprice * oitem.qty;
                            oitem.taxablevalue = oitem.buyingprice * oitem.qty;
                            oitem.salevalue = (oitem.price - oitem.discount) * oitem.qty;

                            //if (oitem.ibuyingprice > 0 && (oitem.price - oitem.discount - oitem.ibuyingprice) > 0)
                            //{
                            //    oitem.margin = Math.Round((oitem.price - oitem.discount - oitem.ibuyingprice), 2);
                            //    if (system_marginonmarkdown)
                            //    {
                            //        oitem.marginpercentage = Math.Round(100 - ((oitem.ibuyingprice / (oitem.price - oitem.discount)) * 100), 2);
                            //    }
                            //    else
                            //    {
                            //        oitem.marginpercentage = Math.Round(((oitem.price - oitem.discount - oitem.ibuyingprice) / oitem.ibuyingprice) * 100, 2);
                            //    }
                            //}
                            //if (oitem.ibuyingprice > 0 && (oitem.dealerprice - oitem.ibuyingprice) > 0)
                            //{
                            //    oitem.dealermargin = Math.Round((oitem.dealerprice - oitem.ibuyingprice), 2);
                            //    if (system_marginonmarkdown)
                            //    {
                            //        oitem.dealermarginpercentage = Math.Round(100 - ((oitem.ibuyingprice / (oitem.dealerprice)) * 100), 2);
                            //    }
                            //    else
                            //    {
                            //        oitem.dealermarginpercentage = Math.Round(((oitem.dealerprice - oitem.ibuyingprice) / oitem.ibuyingprice) * 100, 2);
                            //    }
                            //}
                            //if (oitem.stockage > 10000)
                            //{
                            //    oitem.stockage = reader["age1"] == DBNull.Value ? 0 : Convert.ToInt32(reader["age1"]);
                            //}
                            //if (oitem.iqty > 0 && oitem.oqty <= 0)
                            //    oitem.stockpercentage = 100;
                            //else if (oitem.iqty > 0 && oitem.qty > 0)
                            //    oitem.stockpercentage = Math.Round(oitem.qty / oitem.iqty * 100, 2);
                            lst.Add(oitem);
                        }
                        else if (_reportcreteria.mode == 0)
                        {
                            StockReport oitem = new StockReport();

                            oitem.suppliername = reader["suppliername"] == DBNull.Value ? "" : reader["suppliername"].ToString();
                            oitem.suppliergstno = reader["gstno"] == DBNull.Value ? "" : reader["gstno"].ToString();
                            oitem.sourcesuppliername = reader["sourcesuppliername"] == DBNull.Value ? "" : reader["sourcesuppliername"].ToString();
                            oitem.agentname = reader["agentname"] == DBNull.Value ? "" : reader["agentname"].ToString();
                            oitem.lrentryno = reader["lrentryno"] == DBNull.Value ? "" : reader["lrentryno"].ToString();
                            oitem.invoiceno = reader["invoiceno"] == DBNull.Value ? "" : reader["invoiceno"].ToString();
                            oitem.entrydate = reader["entrydate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["entrydate"]);
                            oitem.invoicedate = reader["invoicedate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["invoicedate"]);
                            oitem.stockdate = reader["stockdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["stockdate"]);
                            oitem.barcode = reader["barcode"] == DBNull.Value ? "" : reader["barcode"].ToString();
                            oitem.itemname = reader["productname"] == DBNull.Value ? "" : reader["productname"].ToString();
                            oitem.barcodeidname = reader["barcodeidname"] == DBNull.Value ? "" : reader["barcodeidname"].ToString();
                            oitem.entrylocationname = reader["entrylocationname"] == DBNull.Value ? "" : reader["entrylocationname"].ToString();
                            oitem.isinterstatesale = reader["isinterstatesale"] == DBNull.Value ? false : Convert.ToBoolean(reader["isinterstatesale"]);

                            oitem.itemid = reader["itemid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["itemid"]);
                            oitem.supplierid = reader["supplierid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["supplierid"]);
                            oitem.productid = reader["productid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productid"]);
                            oitem.companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"]);

                            oitem.productgroupid = reader["productgroup"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productgroup"]);
                            oitem.sectionid = reader["salescounterid"] == DBNull.Value ? 0 : Convert.ToInt32(reader["salescounterid"]);
                            oitem.rowexpense = reader["rowexpense"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["rowexpense"]);
                            oitem.rowexpensevalue = reader["rowexpensevalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["rowexpensevalue"]);
                            if (!Context.HasPermission(PermissionSet.showsellingvalue, InfyPOS.Common.Action.View))
                            {
                                oitem.staxpercent = reader["salestaxid"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["salestaxid"]);
                                oitem.price = reader["price"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["price"]);
                                oitem.dealerprice = reader["dealerprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["dealerprice"]);
                                oitem.discount = reader["discount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discount"]);
                                oitem.mrp = reader["displaymrp"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["displaymrp"]);
                                oitem.finalprice = oitem.price - oitem.discount;
                            }
                            oitem.iqty = reader["purchaseqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["purchaseqty"]);
                            oitem.ipurchaseqty = reader["ipurchaseqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["ipurchaseqty"]);
                            if (oitem.ipurchaseqty > 0 && oitem.ipurchaseqty < oitem.iqty)
                                oitem.iqty = reader["ipurchaseqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["ipurchaseqty"]);

                            oitem.returnqty = reader["purchasereturnqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["purchasereturnqty"]);
                            oitem.whqty = reader["wstock"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["wstock"]);
                            oitem.oqty = reader["saleqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["saleqty"]);
                            oitem.retailqty = reader["retailstock"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["retailstock"]);
                            oitem.jqty = reader["journalqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["journalqty"]);
                            oitem.jqty += reader["jqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["jqty"]);
                            oitem.qty = oitem.whqty + oitem.retailqty;
                            if (!Context.HasPermission(PermissionSet.showvalue, InfyPOS.Common.Action.View))
                            {
                                oitem.basebuyingprice = reader["basebuyingprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["basebuyingprice"]);
                                oitem.pdiscout = reader["purchasediscount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["purchasediscount"]);
                                oitem.pdiscoutpercent = reader["purchasediscountpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["purchasediscountpercentage"]);
                                oitem.taxable = reader["taxable"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["taxable"]);
                                oitem.buyingprice = reader["buyingprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["buyingprice"]);
                                oitem.ibuyingprice = reader["costprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["costprice"]);
                                if (oitem.ibuyingprice < oitem.buyingprice) oitem.ibuyingprice = oitem.buyingprice;
                                //oitem.margin = oitem.price - oitem.discount - oitem.ibuyingprice;
                                oitem.taxvalue = oitem.ibuyingprice - oitem.buyingprice;
                            }
                            oitem.productname = reader["productname"] == DBNull.Value ? "" : reader["productname"].ToString();
                            oitem.brandname = reader["brandname"] == DBNull.Value ? "" : reader["brandname"].ToString();
                            oitem.patternname = reader["patternname"] == DBNull.Value ? "" : reader["patternname"].ToString();
                            oitem.serialno = reader["serialno"] == DBNull.Value ? "" : reader["serialno"].ToString();
                            if (Context.HasPermission(PermissionSet.hidesupplier, InfyPOS.Common.Action.View))
                                oitem.suppliername = "hidden";
                            else
                                oitem.suppliername = reader["suppliername"] == DBNull.Value ? "" : reader["suppliername"].ToString();
                            oitem.suppliercity = reader["suppliercity"] == DBNull.Value ? "" : reader["suppliercity"].ToString();
                            oitem.agentcity = reader["agentcity"] == DBNull.Value ? "" : reader["agentcity"].ToString();
                            oitem.stylename = reader["stylename"] == DBNull.Value ? "" : reader["stylename"].ToString();
                            oitem.sizename = reader["sizename"] == DBNull.Value ? "" : reader["sizename"].ToString();
                            oitem.materialname = reader["materialname"] == DBNull.Value ? "" : reader["materialname"].ToString();
                            oitem.typename = reader["typename"] == DBNull.Value ? "" : reader["typename"].ToString();
                            oitem.sleevename = reader["sleevename"] == DBNull.Value ? "" : reader["sleevename"].ToString();
                            oitem.fitname = reader["fitname"] == DBNull.Value ? "" : reader["fitname"].ToString();
                            oitem.pricetagname = reader["pricetagname"] == DBNull.Value ? "" : reader["pricetagname"].ToString();
                            oitem.colourname = reader["colourname"] == DBNull.Value ? "" : reader["colourname"].ToString();
                            oitem.designid = reader["designid"] == DBNull.Value ? "" : reader["designid"].ToString();
                            oitem.stockid = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                            oitem.invoiceitemid = reader["invoiceitemid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["invoiceitemid"]);
                            oitem.hsncode = reader["hsncode"] == DBNull.Value ? "" : reader["hsncode"].ToString();
                            oitem.sellingmode = reader["sellingmode"] == DBNull.Value ? 0 : Convert.ToInt32(reader["sellingmode"]);
                            oitem.stockage = reader["age"] == DBNull.Value ? 0 : Convert.ToInt32(reader["age"]);
                            oitem.ptaxpercent = reader["purchasetaxpercentage"] == DBNull.Value ? 0 : Convert.ToInt32(reader["purchasetaxpercentage"]);
                            oitem.stockvalue = oitem.ibuyingprice * oitem.qty;
                            oitem.taxablevalue = oitem.buyingprice * oitem.qty;
                            oitem.salevalue = (oitem.price - oitem.discount) * oitem.qty;


                            if (oitem.ibuyingprice > 0 && (oitem.price - oitem.discount - oitem.ibuyingprice) > 0)
                            {
                                oitem.margin = Math.Round((oitem.price - oitem.discount - oitem.ibuyingprice), 2);
                                if (system_marginonmarkdown)
                                {
                                    oitem.marginpercentage = Math.Round(100 - ((oitem.ibuyingprice / (oitem.price - oitem.discount)) * 100), 2);
                                }
                                else
                                {
                                    oitem.marginpercentage = Math.Round(((oitem.price - oitem.discount - oitem.ibuyingprice) / oitem.ibuyingprice) * 100, 2);
                                }
                            }
                            if (oitem.ibuyingprice > 0 && (oitem.dealerprice - oitem.ibuyingprice) > 0)
                            {
                                oitem.dealermargin = Math.Round((oitem.dealerprice - oitem.ibuyingprice), 2);
                                if (system_marginonmarkdown)
                                {
                                    oitem.dealermarginpercentage = Math.Round(100 - ((oitem.ibuyingprice / (oitem.dealerprice)) * 100), 2);
                                }
                                else
                                {
                                    oitem.dealermarginpercentage = Math.Round(((oitem.dealerprice - oitem.ibuyingprice) / oitem.ibuyingprice) * 100, 2);
                                }
                            }
                            if (oitem.stockage > 10000)
                            {
                                oitem.stockage = reader["age1"] == DBNull.Value ? 0 : Convert.ToInt32(reader["age1"]);
                            }
                            if (oitem.iqty > 0 && oitem.oqty <= 0)
                                oitem.stockpercentage = 100;
                            else if (oitem.iqty > 0 && oitem.qty > 0)
                                oitem.stockpercentage = Math.Round(oitem.qty / oitem.iqty * 100, 2);
                            lst.Add(oitem);
                        }
                    }
                }
                conn.Close();

                if (_reportcreteria.mode == 2 && lst != null && lst.Count > 0)
                {
                    if (lst.GroupBy(x => x.invoiceitemid).Where(x => x.Count() > 1).Count() > 0)
                    {
                        List<StockReport> lstnew = new List<StockReport>();
                        foreach (var grp in lst.GroupBy(x => x.invoiceitemid))
                        {
                            if (grp.Count() == 1)
                            {
                                lstnew.AddRange(grp);
                            }
                            else
                            {
                                var iqty = grp.First().iqty;
                                var bqty = grp.Sum(x => x.qty);
                                if (iqty == bqty)
                                {
                                    grp.ToList().ForEach(x => x.iqty = x.qty);
                                    lstnew.AddRange(grp);
                                }
                                else if (iqty > bqty)
                                {
                                    grp.ToList().ForEach(x => x.iqty = x.qty);
                                    grp.First().iqty += (iqty - bqty);
                                }
                                else
                                {
                                    grp.ToList().ForEach(x => x.iqty = x.qty);
                                    var lgrp = grp.ToList();
                                    var aqty = bqty - iqty;
                                    for (var i = 0; aqty > 0 && i < lgrp.Count; i++)
                                    {
                                        if (lgrp[i].iqty > aqty)
                                        {
                                            lgrp[i].iqty -= aqty;
                                            aqty = 0;
                                        }
                                        else
                                        {
                                            aqty -= lgrp[i].iqty;
                                            lgrp[i].iqty = 0;
                                        }
                                    }
                                    lstnew.AddRange(lgrp.FindAll(x => x.iqty > 0));
                                }
                            }
                        }
                        lst = lstnew;
                    }
                }

                if (_reportcreteria.mode == 0 && lst != null && lst.Count > 0)
                {
                    TaxDataService taxdata = new TaxDataService() { Context = this.Context };
                    var taxlist = taxdata.Search(new TaxCreteria() { });
                    foreach (StockReport stock in lst)
                    {
                        if (stock.staxpercent > 0)
                        {
                            if (taxlist.Exists(e => e.id == stock.staxpercent))
                            {
                                var taxitem = taxlist.Find(e => e.id == stock.staxpercent);
                                decimal taxpercentage = 0, taxamount = 0;
                                if (taxitem.type == 1)
                                    taxpercentage = taxitem.taxpercentage;
                                else if (taxitem.type == 2 && taxitem.taxsplit != null && taxitem.taxsplit.items.Count > 1)
                                {
                                    if ((stock.price - stock.discount) > taxitem.taxsplit.items[0].fromamount
                                        && (stock.price - stock.discount) <= taxitem.taxsplit.items[0].toamount)
                                        taxpercentage = (taxitem.taxsplit.items[0].cgst * 2);
                                    else
                                        taxpercentage = (taxitem.taxsplit.items[1].cgst * 2);
                                }
                                else if (taxitem.type == 2 && taxitem.taxsplit != null && taxitem.taxsplit.items.Count == 1)
                                {
                                    taxpercentage = (taxitem.taxsplit.items[0].cgst * 2);
                                }
                                if (taxpercentage > 0 && (stock.price - stock.discount) > 0)
                                    taxamount = ((stock.price - stock.discount) / (100 + taxpercentage) * taxpercentage);
                                if (stock.buyingprice > 0)
                                {
                                    stock.margin = Math.Round((stock.price - stock.discount - stock.buyingprice - taxamount), 2);
                                    stock.netmarginpercent = Math.Round(((stock.price - stock.discount - stock.buyingprice - taxamount) / stock.buyingprice) * 100, 2);
                                }
                                if (stock.buyingprice > 0 && stock.dealerprice > 0)
                                {
                                    stock.dealermargin = Math.Round((stock.dealerprice - stock.buyingprice - taxamount), 2);
                                    stock.dealermarginpercentage = Math.Round(((stock.dealerprice - stock.buyingprice - taxamount) / stock.buyingprice) * 100, 2);
                                }

                                stock.salestaxamount = taxamount;
                                stock.salestaxpercent = taxpercentage;
                            }

                        }
                    }
                }
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _reportcreteria);
            }
            return lst;
        }



        public List<ROLStoreView> ROLStoreView(ROLStoreView _reportcreteria)
        {
            List<ROLStoreView> lst = new List<ROLStoreView>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                NpgsqlCommand command = null;
                QueryBuilder qb = new QueryBuilder();

                command = GetCommand(_reportcreteria.iswarehouse ? SQL_ROLStoreViewWarehouse : SQL_ROLStoreView, conn);
                if (_reportcreteria.locationid <= 0)
                    command.CommandText = command.CommandText.Replace("salablegoods.locationid = @locationid and", "");
                if (_reportcreteria.sectionid <= 0)
                    command.CommandText = command.CommandText.Replace("and products.salescounterid=@sectionid", "");

                command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _reportcreteria.locationid;
                command.Parameters.Add("sectionid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _reportcreteria.sectionid;
                command.Parameters.AddWithValue("current_date", DateTimeNow.Date);

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        ROLStoreView oitem = new ROLStoreView();
                        oitem.lrid = reader["lrid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["lrid"]);

                        oitem.supplierid = reader["supplierid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["supplierid"]);
                        oitem.lrno = reader["lrno"] == DBNull.Value ? "" : reader["lrno"].ToString();
                        oitem.lrentryno = reader["lrentryno"] == DBNull.Value ? "" : reader["lrentryno"].ToString();
                        oitem.productname = reader["productname"] == DBNull.Value ? "" : reader["productname"].ToString();

                        oitem.lrentrydate = reader["lrentrydate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["lrentrydate"]);
                        oitem.invoiceno = reader["invoiceno"] == DBNull.Value ? "" : reader["invoiceno"].ToString();
                        oitem.invoicedate = reader["invoicedate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["invoicedate"]);
                        oitem.qty = reader["stockqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["stockqty"]);
                        oitem.pendingdays = reader["pendingdays"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["pendingdays"]);

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

        public List<ROLStoreView> ROLStoreViewDetails(ROLStoreView _reportcreteria)
        {
            List<ROLStoreView> lst = new List<ROLStoreView>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                NpgsqlCommand command = null;
                QueryBuilder qb = new QueryBuilder();

                command = GetCommand(_reportcreteria.iswarehouse ? SQL_ROLStoreViewDetailsWarehouse : SQL_ROLStoreViewDetails, conn);
                command.Parameters.Add("lrid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _reportcreteria.lrid;
                command.Parameters.AddWithValue("current_date", DateTimeNow.Date);
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        ROLStoreView oitem = new ROLStoreView();
                        oitem.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        oitem.stockid = reader["stockid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["stockid"]);
                        oitem.locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                        oitem.companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"]);
                        oitem.itemid = reader["itemid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["itemid"]);
                        oitem.supplierid = reader["supplierid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["supplierid"]);
                        oitem.lrreference = reader["lrreference"] == DBNull.Value ? "" : reader["lrreference"].ToString();
                        oitem.productname = reader["productname"] == DBNull.Value ? "" : reader["productname"].ToString();
                        oitem.barcode = reader["barcode"] == DBNull.Value ? "" : reader["barcode"].ToString();

                        oitem.iqty = reader["iqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["iqty"]);
                        oitem.oqty = reader["oqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["oqty"]);
                        oitem.qty = reader["stockqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["stockqty"]);
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

        public List<StockMovement> SearchStockMovement(StockMovementCreteria _reportcreteria)
        {
            //sum((salablegoods.iqty-(salablegoods.oqty+salablegoods.jqty+salablegoods.dqty))::numeric * (stock.buyingprice+case when lrinvoiceitems.purchasetax>0 
            //and lrinvoiceitems.returnqty>0 and lrinvoiceitems.buyingqty>0 
            //then lrinvoiceitems.returnqty * (lrinvoiceitems.purchasetax/lrinvoiceitems.buyingqty) else 0 end)::numeric)  stockvalue, 
            //sum(oqty::numeric * (salablegoods.price-salablegoods.discount)::numeric) salevalue, sum(pqty::numeric * stock.buyingprice::numeric) purchasevalue,

            string SQL_StockMovement = @"select {0}, sum(salablegoods.iqty-(salablegoods.oqty+salablegoods.jqty+salablegoods.dqty))  stockqty, 
sum(oqty) saleqty, sum(dqty) damagedqty, sum(salablegoods.jqty) auditqty, sum(pqty) purchaseqty,sum(rqty) purchasereturnqty, sum(coalesce(tiqty,0)) transferedin, sum(coalesce(toqty,0)) transferedout,
sum((salablegoods.iqty-(salablegoods.oqty+salablegoods.jqty+salablegoods.dqty))::numeric * stock.buyingprice)  stockvalue, 
sum(oqty::numeric * stock.buyingprice::numeric) salevalue, sum(pqty::numeric * stock.buyingprice::numeric) purchasevalue,
sum(rqty::numeric * stock.buyingprice::numeric) purchasereturnvalue, 
sum(coalesce(tiqty::numeric,0::numeric) * stock.buyingprice::numeric) transferedinvalue, 
sum(coalesce(toqty::numeric,0::numeric) * stock.buyingprice::numeric) transferedoutvalue
from stock
left outer join salablegoods on salablegoods.stockid = stock.id and salablegoods.isactive = true
left outer join lrinvoiceitems on lrinvoiceitems.id = stock.invoiceitemid and lrinvoiceitems.isactive = true 
left outer join lrinvoice on lrinvoice.id=lrinvoiceitems.lrinvoiceid and lrinvoice.isactive=true
left outer join lrentry on lrentry.id = lrinvoice.lrid
left outer join items on items.id = stock.itemid
left outer join products on products.id = stock.productid
left outer join supplier on supplier.id = lrentry.supplierid
left outer join suppliercompany on suppliercompany.supplierid = supplier.id
left outer join agent on agent.id = lrentry.agentid
left outer join brand brand on brand.id = items.brandid
left outer join referencelist colour on colour.id = colourid
left outer join referencelist size on size.id = sizeid
left outer join referencelist style on style.id = styleid
left outer join referencelist pattern on pattern.id = patternid
left outer join referencelist material on material.id = materialid
left outer join referencelist type on type.id = typeid
left outer join referencelist sleeve on sleeve.id = sleeveid
left outer join referencelist fit on fit.id = fitid
left outer join referencelist supcity on supcity.id = suppliercompany.cityid
left outer join referencelist agentcity on agentcity.id = agent.cityid
where lrinvoice.isactive=true {1}
group by {2}
union all
select {3}, sum(stock.qty-(stock.transfered+stock.damagedqty+stock.journalqty+stock.returnqty)) stockqty,
0 as saleqty, sum(stock.damagedqty) damagedqty, sum(stock.journalqty) auditqty, sum(purchaseqty) purchaseqty,sum(purchasereturnqty) purchasereturnqty, sum(coalesce(transferedin,0)) transferedin, sum(coalesce(transferedout,0)) transferedout,
sum((stock.qty-(stock.transfered+stock.journalqty+stock.damagedqty+stock.returnqty))::numeric * stock.buyingprice) stockvalue,
0::numeric as salevalue, sum(purchaseqty::numeric * stock.buyingprice::numeric) purchasevalue,
sum(purchasereturnqty::numeric * stock.buyingprice::numeric) purchasereturnvalue, 
sum(coalesce(transferedin::numeric,0::numeric) * stock.buyingprice::numeric) transferedinvalue, 
sum(coalesce(transferedout::numeric,0::numeric) * stock.buyingprice::numeric) transferedoutvalue
from stock
left outer join lrinvoiceitems on lrinvoiceitems.id = stock.invoiceitemid and lrinvoiceitems.isactive = true 
left outer join lrinvoice on lrinvoice.id=lrinvoiceitems.lrinvoiceid and lrinvoice.isactive=true
left outer join lrentry on lrentry.id = lrinvoice.lrid
left outer join items on items.id = stock.itemid
left outer join products on products.id = stock.productid
left outer join supplier on supplier.id = lrentry.supplierid
left outer join suppliercompany on suppliercompany.supplierid = supplier.id
left outer join agent on agent.id = lrentry.agentid
left outer join brand brand on brand.id = items.brandid
left outer join referencelist colour on colour.id = colourid
left outer join referencelist size on size.id = sizeid
left outer join referencelist style on style.id = styleid
left outer join referencelist pattern on pattern.id = patternid
left outer join referencelist material on material.id = materialid
left outer join referencelist type on type.id = typeid
left outer join referencelist sleeve on sleeve.id = sleeveid
left outer join referencelist fit on fit.id = fitid
left outer join referencelist supcity on supcity.id = suppliercompany.cityid
left outer join referencelist agentcity on agentcity.id = agent.cityid
where stock.isactive = true {4}
group by {5}";
            // and stock.purchaseqty>0
            List<StockMovement> lst = new List<StockMovement>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                ReferencelistDataService refdataservice = new ReferencelistDataService() { Context = this.Context };
                List<ReferenceItem> lstreference = refdataservice.SearchList(new ReferencelistCreteria() { type = "LOCATION" });

                List<string> fields = new List<string>();
                List<string> groupby = new List<string>();
                if (_reportcreteria.hasallfields)
                {
                    fields.Add(@"salablegoods.locationid, lrentry.lrentryno, lrentry.stockholdingperiod, lrinvoice.invoiceno, lrinvoice.entrydate, lrinvoice.invoicedate,
supplier.name suppliername, supcity.name suppliercity, agent.name agentname, agentcity.name agentcity, stock.barcode, stock.serialno, products.name as productname, products.sellingmode, products.productgroup, products.salescounterid,
stock.designid, stock.hsncode, products.salestaxid, lrinvoiceitems.rowexpense, lrinvoiceitems.rowexpensevalue,
lrinvoiceitems.basebuyingprice, lrinvoiceitems.buyingprice, lrinvoiceitems.purchasediscount, lrinvoiceitems.purchasediscountpercentage,
lrinvoiceitems.marginpercentage, purchasetaxpercentage, stock.id, stock.invoiceitemid,
stock.buyingprice taxable, stock.ibuyingprice costprice, salablegoods.price, salablegoods.discount, stock.dealerprice, stock.displaymrp,
brand.name brandname, style.name stylename, pattern.name patternname, size.name sizename, colour.name colourname, type.name typename,
material.name materialname, sleeve.name sleevename, fit.name fitname,
EXTRACT(day from @current_date - lrinvoice.entrydate) age, lrentry.stockholdingperiod, EXTRACT(day from @current_date - stock.stockdate) age1");

                    groupby.Add(@"salablegoods.locationid, lrentry.lrentryno, lrentry.stockholdingperiod, lrinvoice.invoiceno, lrinvoice.entrydate, lrinvoice.invoicedate,
supplier.name, supcity.name, agent.name, agentcity.name, stock.barcode, stock.serialno, products.name, products.sellingmode, products.productgroup, products.salescounterid, 
stock.designid, stock.hsncode, products.salestaxid, lrinvoiceitems.rowexpense, lrinvoiceitems.rowexpensevalue,
lrinvoiceitems.basebuyingprice, lrinvoiceitems.buyingprice, lrinvoiceitems.purchasediscount, lrinvoiceitems.purchasediscountpercentage,
lrinvoiceitems.marginpercentage, purchasetaxpercentage, stock.id, stock.invoiceitemid,
stock.buyingprice,stock.ibuyingprice, salablegoods.price, salablegoods.discount, stock.dealerprice, stock.displaymrp,
brand.name, style.name, pattern.name, size.name, colour.name, type.name, 
material.name, sleeve.name, fit.name, EXTRACT(day from @current_date - lrinvoice.entrydate), lrentry.stockholdingperiod, EXTRACT(day from @current_date - stock.stockdate)");
                }
                else
                {
                    if (_reportcreteria.haslocation) fields.Add("salablegoods.locationid");
                    if (_reportcreteria.hassupplier)
                    {
                        fields.Add("lrentry.supplierid");
                        fields.Add("suppliercompany.cityid");
                    }
                    if (_reportcreteria.haslrentryno) fields.Add("lrentry.lrentryno");
                    if (_reportcreteria.hasinvoiceno) fields.Add("lrinvoice.invoiceno");
                    if (_reportcreteria.hasinventrydate) fields.Add("lrinvoice.entrydate");
                    if (_reportcreteria.hasinvoicedate) fields.Add("lrinvoice.invoicedate");
                    if (_reportcreteria.hasproduct) fields.Add("stock.productid");
                    if (_reportcreteria.hasbarcode) fields.Add("salablegoods.barcode");

                    groupby.AddRange(fields);
                }
                if (fields.Count == 0)
                {
                    _reportcreteria.haslocation = true;
                    fields.Add("salablegoods.locationid");
                    groupby.Add("salablegoods.locationid");
                }

                string salecolumn = string.Join(",", fields);
                string stockcolumn = string.Join(",", fields).Replace("salablegoods.", "stock.");

                string salegroupby = string.Join(",", groupby);
                string stockgroupby = string.Join(",", groupby).Replace("salablegoods.", "stock.");
                bool hasmincondition = false;

                StringBuilder sb = new StringBuilder();
                if (_reportcreteria.fromdate.Year > 2000 && _reportcreteria.todate.Year > 2000)
                {
                    sb.AppendLine("and lrinvoice.entrydate >= @fromdate and lrinvoice.entrydate < @todate ");
                    hasmincondition = true;
                }
                if (_reportcreteria.locationid > 0)
                {
                    sb.AppendLine("and salablegoods.locationid = @locationid ");
                }
                if (_reportcreteria.companyid > 0)
                {
                    sb.AppendLine("and salablegoods.companyid = @companyid ");
                }
                if (_reportcreteria.productid > 0)
                {
                    sb.AppendLine("and stock.productid = @productid ");
                    hasmincondition = true;
                }

                if (_reportcreteria.supplierid > 0)
                {
                    sb.AppendLine("and lrentry.supplierid = @supplierid ");
                    hasmincondition = true;
                }
                if (_reportcreteria.suppliercityid > 0)
                {
                    sb.AppendLine("and suppliercompany.cityid = @suppliercityid ");
                }
                if (!string.IsNullOrEmpty(_reportcreteria.lrentryno))
                {
                    sb.AppendLine("and lrentry.lrentryno = @lrentryno ");
                    hasmincondition = true;
                }
                if (!string.IsNullOrEmpty(_reportcreteria.invoiceno))
                {
                    sb.AppendLine("and lrinvoice.invoiceno = @invoiceno ");
                    hasmincondition = true;
                }

                if (!hasmincondition) throw new Exception("Please provide minimum search conditions");

                string salecondition = sb.ToString();
                string stockcondition = sb.ToString().Replace("salablegoods.locationid", "stock.locationid");
                stockcondition = stockcondition.Replace("salablegoods.companyid", "stock.companyid");

                conn.Open();
                NpgsqlCommand command = null;
                DateTime today = _reportcreteria.fromdate.Date;
                DateTime nextdate = _reportcreteria.todate.Date.AddDays(1);

                SQL_StockMovement = string.Format(SQL_StockMovement, salecolumn, salecondition, salegroupby, stockcolumn, stockcondition, stockgroupby);

                command = GetCommand(SQL_StockMovement, conn);
                command.Parameters.Add("productid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _reportcreteria.productid;
                command.Parameters.Add("supplierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _reportcreteria.supplierid;
                command.Parameters.Add("suppliercityid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _reportcreteria.suppliercityid;
                command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _reportcreteria.locationid;
                command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _reportcreteria.companyid;
                command.Parameters.Add("fromdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = today;
                command.Parameters.Add("todate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = nextdate;
                command.Parameters.Add("lrentryno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = _reportcreteria.lrentryno;
                command.Parameters.Add("invoiceno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = _reportcreteria.invoiceno;
                command.Parameters.Add("current_date", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow.Date;
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        StockMovement oitem = new StockMovement();
                        if (_reportcreteria.haslocation)
                        {
                            oitem.locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                            if (lstreference.Exists(e => e.id == oitem.locationid))
                            {
                                oitem.locationname = lstreference.Find(e => e.id == oitem.locationid).name;
                            }
                            else
                                oitem.locationname = "WAREHOUSE";
                        }
                        if (_reportcreteria.hasallfields)
                        {
                            oitem.locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                            if (lstreference.Exists(e => e.id == oitem.locationid))
                            {
                                oitem.locationname = lstreference.Find(e => e.id == oitem.locationid).name;
                            }
                            else
                                oitem.locationname = "WAREHOUSE";

                            oitem.suppliername = reader["suppliername"] == DBNull.Value ? "" : reader["suppliername"].ToString();
                            oitem.agentname = reader["agentname"] == DBNull.Value ? "" : reader["agentname"].ToString();
                            oitem.lrentryno = reader["lrentryno"] == DBNull.Value ? "" : reader["lrentryno"].ToString();
                            oitem.invoiceno = reader["invoiceno"] == DBNull.Value ? "" : reader["invoiceno"].ToString();
                            oitem.entrydate = reader["entrydate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["entrydate"]);
                            oitem.invoicedate = reader["invoicedate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["invoicedate"]);
                            oitem.barcode = reader["barcode"] == DBNull.Value ? "" : reader["barcode"].ToString();
                            oitem.itemname = reader["productname"] == DBNull.Value ? "" : reader["productname"].ToString();
                            oitem.productgroupid = reader["productgroup"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productgroup"]);
                            oitem.sectionid = reader["salescounterid"] == DBNull.Value ? 0 : Convert.ToInt32(reader["salescounterid"]);
                            oitem.rowexpense = reader["rowexpense"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["rowexpense"]);
                            oitem.rowexpensevalue = reader["rowexpensevalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["rowexpensevalue"]);
                            if (!Context.HasPermission(PermissionSet.showsellingvalue, InfyPOS.Common.Action.View))
                            {
                                oitem.staxpercent = reader["salestaxid"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["salestaxid"]);
                                oitem.price = reader["price"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["price"]);
                                oitem.dealerprice = reader["dealerprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["dealerprice"]);
                                oitem.discount = reader["discount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discount"]);
                                oitem.finalprice = oitem.price - oitem.discount;
                                oitem.mrp = reader["displaymrp"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["displaymrp"]);
                            }
                            if (!Context.HasPermission(PermissionSet.showvalue, InfyPOS.Common.Action.View))
                            {
                                oitem.basebuyingprice = reader["basebuyingprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["basebuyingprice"]);
                                oitem.pdiscout = reader["purchasediscount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["purchasediscount"]);
                                oitem.pdiscoutpercent = reader["purchasediscountpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["purchasediscountpercentage"]);
                                oitem.taxable = reader["taxable"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["taxable"]);
                                oitem.buyingprice = reader["buyingprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["buyingprice"]);
                                oitem.ibuyingprice = reader["costprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["costprice"]);
                                if (oitem.ibuyingprice < oitem.buyingprice) oitem.ibuyingprice = oitem.buyingprice;
                                oitem.taxvalue = oitem.ibuyingprice - oitem.buyingprice;
                            }
                            oitem.productname = reader["productname"] == DBNull.Value ? "" : reader["productname"].ToString();
                            oitem.brandname = reader["brandname"] == DBNull.Value ? "" : reader["brandname"].ToString();
                            oitem.patternname = reader["patternname"] == DBNull.Value ? "" : reader["patternname"].ToString();
                            oitem.serialno = reader["serialno"] == DBNull.Value ? "" : reader["serialno"].ToString();
                            if (Context.HasPermission(PermissionSet.hidesupplier, InfyPOS.Common.Action.View))
                                oitem.suppliername = "hidden";
                            else
                                oitem.suppliername = reader["suppliername"] == DBNull.Value ? "" : reader["suppliername"].ToString();
                            oitem.suppliercity = reader["suppliercity"] == DBNull.Value ? "" : reader["suppliercity"].ToString();
                            oitem.agentcity = reader["agentcity"] == DBNull.Value ? "" : reader["agentcity"].ToString();
                            oitem.stylename = reader["stylename"] == DBNull.Value ? "" : reader["stylename"].ToString();
                            oitem.sizename = reader["sizename"] == DBNull.Value ? "" : reader["sizename"].ToString();
                            oitem.materialname = reader["materialname"] == DBNull.Value ? "" : reader["materialname"].ToString();
                            oitem.typename = reader["typename"] == DBNull.Value ? "" : reader["typename"].ToString();
                            oitem.sleevename = reader["sleevename"] == DBNull.Value ? "" : reader["sleevename"].ToString();
                            oitem.fitname = reader["fitname"] == DBNull.Value ? "" : reader["fitname"].ToString();
                            oitem.colourname = reader["colourname"] == DBNull.Value ? "" : reader["colourname"].ToString();
                            oitem.designid = reader["designid"] == DBNull.Value ? "" : reader["designid"].ToString();
                            oitem.stockid = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                            oitem.invoiceitemid = reader["invoiceitemid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["invoiceitemid"]);
                            oitem.hsncode = reader["hsncode"] == DBNull.Value ? "" : reader["hsncode"].ToString();
                            oitem.sellingmode = reader["sellingmode"] == DBNull.Value ? 0 : Convert.ToInt32(reader["sellingmode"]);
                            oitem.stockage = reader["age"] == DBNull.Value ? 0 : Convert.ToInt32(reader["age"]);
                            oitem.ptaxpercent = reader["purchasetaxpercentage"] == DBNull.Value ? 0 : Convert.ToInt32(reader["purchasetaxpercentage"]);

                            if (oitem.ibuyingprice > 0 && (oitem.price - oitem.discount - oitem.ibuyingprice) > 0)
                            {
                                oitem.margin = Math.Round((oitem.price - oitem.discount - oitem.ibuyingprice), 2);
                                oitem.marginpercentage = Math.Round(((oitem.price - oitem.discount - oitem.ibuyingprice) / oitem.ibuyingprice) * 100, 2);
                            }
                            if (oitem.stockage > 10000)
                            {
                                oitem.stockage = reader["age1"] == DBNull.Value ? 0 : Convert.ToInt32(reader["age1"]);
                            }
                        }
                        else
                        {
                            if (_reportcreteria.hassupplier)
                            {
                                oitem.supplierid = reader["supplierid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["supplierid"]);
                                oitem.suppliercityid = reader["cityid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["cityid"]);
                            }
                            if (_reportcreteria.haslrentryno)
                                oitem.lrentryno = reader["lrentryno"] == DBNull.Value ? "" : reader["lrentryno"].ToString();
                            if (_reportcreteria.hasinvoiceno)
                                oitem.invoiceno = reader["invoiceno"] == DBNull.Value ? "" : reader["invoiceno"].ToString();
                            if (_reportcreteria.hasinvoicedate)
                                oitem.invoicedate = reader["invoicedate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["invoicedate"]);
                            if (_reportcreteria.hasinventrydate)
                                oitem.inventrydate = reader["entrydate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["entrydate"]);
                            if (_reportcreteria.hasproduct)
                                oitem.productid = reader["productid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productid"]);
                            if (_reportcreteria.hasbarcode)
                                oitem.barcode = reader["barcode"] == DBNull.Value ? "" : reader["barcode"].ToString();
                        }

                        oitem.purchaseqty = reader["purchaseqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["purchaseqty"]);
                        oitem.purchaseretqty = reader["purchasereturnqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["purchasereturnqty"]);
                        oitem.saleqty = reader["saleqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["saleqty"]);
                        oitem.inwardqty = reader["transferedin"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["transferedin"]);
                        oitem.outwardqty = reader["transferedout"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["transferedout"]);
                        oitem.stockqty = reader["stockqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["stockqty"]);
                        oitem.auditqty = reader["auditqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["auditqty"]);

                        oitem.purchasevalue = reader["purchasevalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["purchasevalue"]);
                        oitem.purchaseretvalue = reader["purchasereturnvalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["purchasereturnvalue"]);
                        oitem.salevalue = reader["salevalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["salevalue"]);
                        oitem.inwardvalue = reader["transferedinvalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["transferedinvalue"]);
                        oitem.outwardvalue = reader["transferedoutvalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["transferedoutvalue"]);
                        oitem.stockvalue = reader["stockvalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["stockvalue"]);

                        if ((oitem.purchaseqty > 0 || oitem.inwardqty > 0) && (oitem.purchaseretqty > 0 ||
                            oitem.saleqty > 0 || oitem.outwardqty > 0 || oitem.stockqty > 0))
                        {
                            oitem.outpercentage = Math.Round(((oitem.purchaseretqty + oitem.saleqty + oitem.outwardqty) / (oitem.purchaseqty + oitem.inwardqty)) * 100, 2);
                        }
                        if (oitem.purchaseqty == 0 && oitem.inwardqty == 0)
                            oitem.purchaseqty = oitem.saleqty + oitem.stockqty;

                        if (oitem.purchaseqty > 0 || oitem.inwardqty > 0 || oitem.purchaseretqty > 0 ||
                            oitem.saleqty > 0 || oitem.outwardqty > 0 || oitem.stockqty > 0)
                            lst.Add(oitem);
                    }
                }
                conn.Close();

                if (_reportcreteria.hasallfields && lst != null && lst.Count > 0)
                {
                    TaxDataService taxdata = new TaxDataService() { Context = this.Context };
                    var taxlist = taxdata.Search(new TaxCreteria() { });
                    foreach (StockMovement stock in lst)
                    {
                        if (stock.staxpercent > 0)
                        {
                            if (taxlist.Exists(e => e.id == stock.staxpercent))
                            {
                                var taxitem = taxlist.Find(e => e.id == stock.staxpercent);
                                decimal taxpercentage = 0, taxamount = 0;
                                if (taxitem.type == 1)
                                    taxpercentage = taxitem.taxpercentage;
                                else if (taxitem.type == 2 && taxitem.taxsplit != null && taxitem.taxsplit.items.Count > 1)
                                {
                                    if ((stock.price - stock.discount) > taxitem.taxsplit.items[0].fromamount
                                        && (stock.price - stock.discount) <= taxitem.taxsplit.items[0].toamount)
                                        taxpercentage = (taxitem.taxsplit.items[0].cgst * 2);
                                    else
                                        taxpercentage = (taxitem.taxsplit.items[1].cgst * 2);
                                }
                                else if (taxitem.type == 2 && taxitem.taxsplit != null && taxitem.taxsplit.items.Count == 1)
                                {
                                    taxpercentage = (taxitem.taxsplit.items[0].cgst * 2);
                                }
                                if (taxpercentage > 0 && (stock.price - stock.discount) > 0)
                                    taxamount = ((stock.price - stock.discount) / (100 + taxpercentage) * taxpercentage);
                                if (stock.buyingprice > 0)
                                {
                                    stock.margin = Math.Round((stock.price - stock.discount - stock.buyingprice - taxamount), 2);
                                    stock.netmarginpercent = Math.Round(((stock.price - stock.discount - stock.buyingprice - taxamount) / stock.buyingprice) * 100, 2);
                                }
                                stock.salestaxamount = taxamount;
                                stock.salestaxpercent = taxpercentage;
                            }

                        }
                    }
                }
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _reportcreteria);
            }
            return lst;
        }

        public List<Products> SearchNewProducts(ProductsCreteria _reportcreteria)
        {
            List<Products> lst = new List<Products>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                NpgsqlCommand command = null;

                QueryBuilder qb = new QueryBuilder();
                qb.CurrentQuery = SQL_NewProducts;

                if (_reportcreteria.fromdate != DateTime.MinValue && _reportcreteria.todate != DateTime.MinValue)
                {
                    qb.AddParameter("products.createdon", "fromdate", _reportcreteria.fromdate.Date, ">=");
                    qb.AddParameter("products.createdon", "todate", _reportcreteria.todate.Date.AddDays(1), "<");
                }

                command = qb.GetCommand(conn);

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Products oitem = new Products();
                        oitem.code = reader["code"] == DBNull.Value ? "" : reader["code"].ToString();
                        oitem.name = reader["productname"] == DBNull.Value ? "" : reader["productname"].ToString();
                        oitem.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        oitem.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt32(reader["createdby"]);

                        oitem.sellingmode = reader["sellingmode"] == DBNull.Value ? 0 : Convert.ToInt32(reader["sellingmode"]);
                        oitem.pricingmode = reader["pricingmode"] == DBNull.Value ? 0 : Convert.ToInt32(reader["pricingmode"]);
                        oitem.salestaxid = reader["salestaxid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["salestaxid"]);
                        oitem.discountmode = reader["discountmode"] == DBNull.Value ? 0 : Convert.ToInt32(reader["discountmode"]);
                        oitem.discountvalue = reader["discountvalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discountvalue"]);
                        oitem.purchasetaxid = reader["purchasetaxid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["purchasetaxid"]);
                        oitem.margin = reader["margin"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["margin"]);
                        oitem.purchaseplanmode = reader["purchaseplanmode"] == DBNull.Value ? Products.PurchasePlanMode.None : (Products.PurchasePlanMode)Convert.ToInt32(reader["purchaseplanmode"]);
                        oitem.sizegroupid = reader["sizegroupid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sizegroupid"]);
                        oitem.salescounterid = reader["salescounterid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["salescounterid"]);
                        oitem.productgroup = reader["productgroup"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productgroup"]);
                        oitem.employeename = reader["employeename"] == DBNull.Value ? "" : reader["employeename"].ToString();
                        oitem.unit = reader["unit"] == DBNull.Value ? 0 : Convert.ToInt64(reader["unit"]);
                        oitem.fixedprice = reader["fixedprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["fixedprice"]);
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

        public List<Supplier> SearchNewSuppliers(SupplierCreteria _reportcreteria)
        {
            List<Supplier> lst = new List<Supplier>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                NpgsqlCommand command = null;

                QueryBuilder qb = new QueryBuilder();
                qb.CurrentQuery = SQL_NewSuppliers;

                if (_reportcreteria.fromdate != DateTime.MinValue && _reportcreteria.todate != DateTime.MinValue)
                {
                    qb.AddParameter("supplier.createdon", "fromdate", _reportcreteria.fromdate.Date, ">=");
                    qb.AddParameter("supplier.createdon", "todate", _reportcreteria.todate.Date.AddDays(1), "<");
                }

                command = qb.GetCommand(conn);

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Supplier oitem = new Supplier();
                        oitem.name = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
                        oitem.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        oitem.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt32(reader["createdby"]);
                        oitem.employeename = reader["employeename"] == DBNull.Value ? "" : reader["employeename"].ToString();

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



        public List<StockReport> SearchPendingTransfer(StockReportCreteria _reportcreteria)
        {
            List<StockReport> lst = new List<StockReport>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                NpgsqlCommand command = null;

                QueryBuilder qb = new QueryBuilder();
                qb.CurrentQuery = SQL_PendingTransfer;

                qb.AddParameter("stock.isactive", true, "=");

                if (!string.IsNullOrEmpty(_reportcreteria.barcode))
                    qb.AddParameter("stock.barcode", _reportcreteria.barcode, "=");
                else
                {
                    if (_reportcreteria.fromdate != DateTime.MinValue && _reportcreteria.todate != DateTime.MinValue)
                    {
                        qb.AddParameter("stockdate", "stockdatefrom", _reportcreteria.fromdate.Date, ">=");
                        qb.AddParameter("stockdate", "stockdateto", _reportcreteria.todate.AddDays(1).Date, "<");
                    }
                    else
                        qb.AddParameter("stockdate", DateTimeNow.Date, "=");
                }
                if (_reportcreteria.supplierid > 0)
                    qb.AddParameter("supplierid", _reportcreteria.supplierid, "=");
                if (_reportcreteria.productid > 0)
                    qb.AddParameter("stock.productid", _reportcreteria.productid, "=");
                if (_reportcreteria.brandid > 0)
                    qb.AddParameter("items.brandid", _reportcreteria.brandid, "=");
                if (_reportcreteria.materialid > 0)
                    qb.AddParameter("items.materialid", _reportcreteria.materialid, "=");
                if (_reportcreteria.patternid > 0)
                    qb.AddParameter("items.patternid", _reportcreteria.patternid, "=");
                if (_reportcreteria.styleid > 0)
                    qb.AddParameter("items.styleid", _reportcreteria.styleid, "=");

                qb.CurrentQuery += " and qty-transfered-journalqty-damagedqty-returnqty>0";
                qb.Limit = 500;
                command = qb.GetCommand(conn);

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        StockReport oitem = new StockReport();
                        oitem.barcode = reader["barcode"] == DBNull.Value ? "" : reader["barcode"].ToString();
                        oitem.itemname = reader["printingname"] == DBNull.Value ? "" : reader["printingname"].ToString();
                        oitem.price = reader["price"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["price"]);
                        oitem.discount = reader["discount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discount"]);
                        oitem.qty = reader["wstock"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["wstock"]);
                        oitem.iqty = reader["pqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["pqty"]);
                        oitem.buyingprice = reader["buyingprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["buyingprice"]);
                        oitem.productname = reader["productname"] == DBNull.Value ? "" : reader["productname"].ToString();
                        oitem.brandname = reader["brandname"] == DBNull.Value ? "" : reader["brandname"].ToString();
                        oitem.patternname = reader["patternname"] == DBNull.Value ? "" : reader["patternname"].ToString();
                        oitem.stylename = reader["stylename"] == DBNull.Value ? "" : reader["stylename"].ToString();
                        oitem.materialname = reader["materialname"] == DBNull.Value ? "" : reader["materialname"].ToString();
                        oitem.sizename = reader["lrreference"] == DBNull.Value ? "" : reader["lrreference"].ToString();
                        oitem.typename = reader["typename"] == DBNull.Value ? "" : reader["typename"].ToString();

                        if (Context.HasPermission(PermissionSet.hidesupplier, InfyPOS.Common.Action.View))
                            oitem.suppliername = "hidden";
                        else
                            oitem.suppliername = reader["suppliername"] == DBNull.Value ? "" : reader["suppliername"].ToString();
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
        public List<BarcodeReport> SearchBarcodes(StockReportCreteria _reportcreteria)
        {
            List<BarcodeReport> lst = new List<BarcodeReport>();
            foreach (var barcode in _reportcreteria.barcodes)
            {
                var barcodeitem = SearchBarcode(new StockReportCreteria()
                {
                    mode = _reportcreteria.mode,
                    locationid = _reportcreteria.locationid,
                    barcode = barcode
                });

                if (barcodeitem.lrinvoiceitemid > 0) lst.Add(barcodeitem);
            }
            return lst;
        }
        public BarcodeReport SearchBarcode(StockReportCreteria _reportcreteria)
        {
            BarcodeReport oitem = new BarcodeReport();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                NpgsqlCommand command = null;

                if (!string.IsNullOrEmpty(_reportcreteria.barcode) &&
                    !string.IsNullOrEmpty(InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_barcodeparser))
                {
                    _reportcreteria.barcode = InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.POSParser.UpdateBarcode(_reportcreteria.barcode);
                }

                QueryBuilder qb = new QueryBuilder();
                qb.CurrentQuery = SQL_BarcodeDetail;
                qb.AddParameter("stock.barcode", _reportcreteria.barcode, "=");
                command = qb.GetCommand(conn);

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        oitem.stockid = reader["stockid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["stockid"]);
                        oitem.itemid = reader["itemid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["itemid"]);
                        oitem.barcode = reader["barcode"] == DBNull.Value ? "" : reader["barcode"].ToString();
                        oitem.barcodes = reader["barcodes"] == DBNull.Value ? "" : reader["barcodes"].ToString();
                        oitem.itemname = reader["printingname"] == DBNull.Value ? "" : reader["printingname"].ToString();
                        oitem.buyingprice = reader["buyingprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["buyingprice"]);
                        oitem.price = reader["price"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["price"]);
                        oitem.discount = reader["discount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discount"]);
                        oitem.wstock = reader["wstock"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["wstock"]);
                        oitem.pqty = reader["pqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["pqty"]);
                        oitem.buyingqty = reader["buyingqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["buyingqty"]);
                        oitem.productname = reader["productname"] == DBNull.Value ? "" : reader["productname"].ToString();
                        oitem.productid = reader["productid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productid"]);
                        oitem.brandname = reader["brandname"] == DBNull.Value ? "" : reader["brandname"].ToString();
                        oitem.patternname = reader["patternname"] == DBNull.Value ? "" : reader["patternname"].ToString();
                        oitem.stylename = reader["stylename"] == DBNull.Value ? "" : reader["stylename"].ToString();
                        oitem.sizename = reader["sizename"] == DBNull.Value ? "" : reader["sizename"].ToString();
                        oitem.materialname = reader["materialname"] == DBNull.Value ? "" : reader["materialname"].ToString();
                        oitem.typename = reader["typename"] == DBNull.Value ? "" : reader["typename"].ToString();
                        oitem.sectionname = reader["sectionname"] == DBNull.Value ? "" : reader["sectionname"].ToString();
                        oitem.pricetagname = reader["pricetagname"] == DBNull.Value ? "" : reader["pricetagname"].ToString();

                        oitem.invoiceno = reader["invoiceno"] == DBNull.Value ? "" : reader["invoiceno"].ToString();
                        oitem.invoicedate = reader["invoicedate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["invoicedate"]);
                        oitem.entrydate = reader["entrydate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["entrydate"]);

                        oitem.mrp = reader["mrp"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["mrp"]);
                        oitem.sellingprice = reader["sellingprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["sellingprice"]);
                        oitem.basebuyingprice = reader["basebuyingprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["basebuyingprice"]);
                        oitem.pdiscountpercent = reader["purchasediscountpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["purchasediscountpercentage"]);
                        oitem.pdiscount = reader["purchasediscount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["purchasediscount"]);
                        oitem.ptax = reader["purchasetax"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["purchasetax"]);
                        oitem.ptaxpercent = reader["purchasetaxpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["purchasetaxpercentage"]);
                        oitem.salediscount = reader["salediscount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["salediscount"]);
                        oitem.salediscountpercent = reader["salediscountpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["salediscountpercentage"]);
                        oitem.saletax = reader["salestax"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["salestax"]);
                        oitem.saletaxpercent = reader["salestaxpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["salestaxpercentage"]);
                        oitem.hsncode = reader["hsncode"] == DBNull.Value ? "" : reader["hsncode"].ToString();
                        oitem.supplier = reader["supplier"] == DBNull.Value ? "" : reader["supplier"].ToString();
                        oitem.company = reader["company"] == DBNull.Value ? "" : reader["company"].ToString();
                        oitem.designid = reader["designid"] == DBNull.Value ? "" : reader["designid"].ToString();
                        oitem.lrno = reader["lrno"] == DBNull.Value ? "" : reader["lrno"].ToString();
                        oitem.lrentryno = reader["lrentryno"] == DBNull.Value ? "" : reader["lrentryno"].ToString();
                        oitem.lrdate = reader["lrdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["lrdate"]);
                        oitem.lrinvoiceid = reader["lrinvoiceid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["lrinvoiceid"]);
                        oitem.lrinvoiceitemid = reader["lrinvoiceitemid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["lrinvoiceitemid"]);
                        oitem.sellingmode = reader["sellingmode"] == DBNull.Value ? 0 : Convert.ToInt32(reader["sellingmode"]);
                        oitem.lrid = reader["lrid"] == DBNull.Value ? 0 : Convert.ToInt32(reader["lrid"]);
                        oitem.companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt32(reader["companyid"]);
                        if (oitem.companyid == 0)
                            oitem.companyid = reader["stockcompanyid"] == DBNull.Value ? 0 : Convert.ToInt32(reader["stockcompanyid"]);
                    }
                }
                conn.Close();

                if (_reportcreteria.mode == 9)
                {
                    StockDataService stockdate = new StockDataService() { Context = this.Context };
                    stockdate.Context = this.Context;
                    oitem.rstock = stockdate.FindStock(_reportcreteria.barcode).rstock;

                    if (_reportcreteria.locationid > 0)
                    {
                        RetailReportDataService retaildata = new RetailReportDataService() { Context = this.Context };
                        retaildata.Context = this.Context;
                        oitem.retailstock = retaildata.SearchStock(new StockReportCreteria { barcode = _reportcreteria.barcode, locationid = _reportcreteria.locationid });
                    }

                }
                else
                {
                    RetailReportDataService retaildata = new RetailReportDataService() { Context = this.Context };
                    retaildata.Context = this.Context;
                    oitem.retailstock = retaildata.SearchStock(new StockReportCreteria { barcode = _reportcreteria.barcode });
                }
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _reportcreteria);
            }
            return oitem;
        }

        public List<StockReport> SearchStockAnalysis(StockReportCreteria _reportcreteria)
        {
            StringBuilder SQL_Select = new StringBuilder();
            StringBuilder SQL_From = new StringBuilder();
            StringBuilder SQL_Where = new StringBuilder();
            StringBuilder SQL_Groupby = new StringBuilder(@" group by ");
            StringBuilder SQL_Orderby = new StringBuilder(@" order by ");

            if (_reportcreteria.mode == 1)
            {
                SQL_Select.Append(@"select sum( (stock.qty - (stock.transfered+stock.returnqty+stock.journalqty+stock.damagedqty)) * stock.buyingprice::numeric) as value, 
                                            sum( (stock.qty - (stock.transfered+stock.returnqty+stock.journalqty+stock.damagedqty)) * stock.ibuyingprice::numeric) as ivalue,
                                            sum((stock.qty - (stock.transfered+stock.returnqty+stock.journalqty+stock.damagedqty))) qty, 
                                            sum( (stock.qty - (stock.transfered+stock.returnqty+stock.journalqty+stock.damagedqty)) * (stock.price::numeric - stock.discount::numeric)) salevalue");

                SQL_From.Append(@" from stock 
                        left outer join products on products.id = stock.productid
                        left outer join items on items.id = stock.itemid 
                        left outer join lrinvoiceitems on lrinvoiceitems.id = stock.invoiceitemid and lrinvoiceitems.isactive = true
                        left outer join lrinvoice on lrinvoice.id = lrinvoiceitems.lrinvoiceid and lrinvoice.isactive = true
                        left outer join lrentry  on lrentry.id = lrid and lrentry.isactive = true
                        left outer join supplier on supplier.id = lrentry.supplierid 
                        left outer join suppliercompany on suppliercompany.id = lrinvoice.suppliercompanyid 
                        left outer join brand brand on brand.id = items.brandid
                        left outer join referencelist colour on colour.id = colourid
                        left outer join referencelist size on size.id = sizeid
                        left outer join referencelist style on style.id = styleid
                        left outer join referencelist pattern on pattern.id = patternid
                        left outer join referencelist sleeve on sleeve.id = sleeveid
                        left outer join referencelist material on material.id = materialid
                        left outer join referencelist type on type.id = typeid
                        left outer join referencelist section on section.id = products.salescounterid ");

                if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.analyser_stock_withzerostock)
                    SQL_Where.Append(@" where lrentry.isactive = true and stock.qty-(stock.transfered+stock.journalqty+stock.damagedqty+stock.returnqty) != 0");
                else
                    SQL_Where.Append(@" where lrentry.isactive = true and stock.qty-(stock.transfered+stock.journalqty+stock.damagedqty+stock.returnqty)>0");
            }
            else
            {
                SQL_Select.Append(@"select sum( (salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty)) * stock.buyingprice::numeric) as value,  
                                            sum( (salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty)) * stock.ibuyingprice::numeric) as ivalue,  
                                            sum(salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty)) qty, 
                                            sum( (salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty)) * (salablegoods.price::numeric - salablegoods.discount::numeric)) as salevalue");

                SQL_From.Append(@" from salablegoods 
                                inner join stock on stock.id = salablegoods.stockid and stock.isactive = true
                                left outer join products on products.id = stock.productid
                                left outer join items on items.id = stock.itemid 
                                left outer join lrinvoiceitems on lrinvoiceitems.id = stock.invoiceitemid and lrinvoiceitems.isactive = true
                                left outer join lrinvoice on lrinvoice.id = lrinvoiceitems.lrinvoiceid and lrinvoice.isactive = true
                                left outer join lrentry  on lrentry.id = lrid and lrentry.isactive = true
                                left outer join supplier on supplier.id = lrentry.supplierid 
                                left outer join suppliercompany on suppliercompany.id = lrinvoice.suppliercompanyid 
                        left outer join brand brand on brand.id = items.brandid
                        left outer join referencelist colour on colour.id = colourid
                        left outer join referencelist size on size.id = sizeid
                        left outer join referencelist style on style.id = styleid
                        left outer join referencelist pattern on pattern.id = patternid
                        left outer join referencelist material on material.id = materialid
                        left outer join referencelist type on type.id = typeid
                        left outer join referencelist sleeve on sleeve.id = sleeveid
                        left outer join referencelist section on section.id = products.salescounterid ");

                if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.analyser_stock_withzerostock)
                    SQL_Where.Append(@" where salablegoods.isactive=true");
                else
                    SQL_Where.Append(@" where salablegoods.isactive=true and salablegoods.hasstock = true and salablegoods.iqty-(salablegoods.oqty+salablegoods.jqty+salablegoods.dqty)>0");
                if (_reportcreteria.locationid > 0)
                    SQL_Where.Append(" and salablegoods.locationid=" + _reportcreteria.locationid);

            }
            List<StockReport> lst = new List<StockReport>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                NpgsqlCommand command = null;

                QueryBuilder qb = new QueryBuilder();

                if (!_reportcreteria.product && !_reportcreteria.brand && !_reportcreteria.material && !_reportcreteria.pattern
                    && !_reportcreteria.style && !_reportcreteria.size && !_reportcreteria.section && !_reportcreteria.supplier && !_reportcreteria.sourcesupplier)
                {
                    SQL_Select.Append(",items.productname as column1");
                    SQL_Groupby.Append("items.productname,");
                    SQL_Orderby.Append("items.productname,");
                }
                if (_reportcreteria.product)
                {
                    SQL_Select.Append(",products.name as column1");
                    SQL_Groupby.Append("products.name,");
                    SQL_Orderby.Append("products.name,");
                }
                if (_reportcreteria.brand)
                {
                    SQL_Select.Append(",brand.name as column2");
                    SQL_Groupby.Append("brand.name,");
                    SQL_Orderby.Append("brand.name,");
                }
                if (_reportcreteria.material)
                {
                    SQL_Select.Append(",material.name as column3");
                    SQL_Groupby.Append("material.name,");
                    SQL_Orderby.Append("material.name,");
                }
                if (_reportcreteria.pattern)
                {
                    SQL_Select.Append(",pattern.name as column4");
                    SQL_Groupby.Append("pattern.name,");
                    SQL_Orderby.Append("pattern.name,");
                }
                if (_reportcreteria.style)
                {
                    SQL_Select.Append(",style.name as column5");
                    SQL_Groupby.Append("style.name,");
                    SQL_Orderby.Append("style.name,");
                }
                if (_reportcreteria.size)
                {
                    SQL_Select.Append(",size.name as column6");
                    SQL_Groupby.Append("size.name,");
                    SQL_Orderby.Append("size.name,");
                }
                if (_reportcreteria.design)
                {
                    SQL_Select.Append(",stock.designid as column7");
                    SQL_Groupby.Append("stock.designid,");
                    SQL_Orderby.Append("stock.designid,");
                }
                if (_reportcreteria.supplier)
                {
                    SQL_Select.Append(",lrentry.supplierid as column8");
                    SQL_Groupby.Append("lrentry.supplierid,");
                    SQL_Orderby.Append("lrentry.supplierid,");
                }
                if (_reportcreteria.section)
                {
                    SQL_Select.Append(",products.salescounterid as column9");
                    SQL_Groupby.Append("products.salescounterid,");
                    SQL_Orderby.Append("products.salescounterid,");
                }
                if (_reportcreteria.isbarcode)
                {
                    SQL_Select.Append(",stock.barcode as column10");
                    SQL_Groupby.Append("stock.barcode,");
                    SQL_Orderby.Append("stock.barcode,");
                }
                if (_reportcreteria.type)
                {
                    SQL_Select.Append(",items.typename as column11");
                    SQL_Groupby.Append("items.typename,");
                    SQL_Orderby.Append("items.typename,");
                }
                if (_reportcreteria.sourcesupplier)
                {
                    SQL_Select.Append(",lrentry.sourcesupplierid as column12");
                    SQL_Groupby.Append("lrentry.sourcesupplierid,");
                    SQL_Orderby.Append("lrentry.sourcesupplierid,");
                }
                if (_reportcreteria.sleeve)
                {
                    SQL_Select.Append(",sleeve.name as column13");
                    SQL_Groupby.Append("sleeve.name,");
                    SQL_Orderby.Append("sleeve.name,");
                }
                if (_reportcreteria.multicompany && _reportcreteria.companyid > 0)
                {
                    if (_reportcreteria.mode == 1)
                        SQL_Where.Append(" and stock.companyid=" + _reportcreteria.companyid);
                    else
                        SQL_Where.Append(" and salablegoods.companyid=" + _reportcreteria.companyid);
                }
                if (_reportcreteria.productid > 0)
                    SQL_Where.Append(" and items.productid=" + _reportcreteria.productid);
                if (_reportcreteria.brandid > 0)
                    SQL_Where.Append(" and items.brandid=" + _reportcreteria.brandid);
                if (_reportcreteria.materialid > 0)
                    SQL_Where.Append(" and items.materialid=" + _reportcreteria.materialid);
                if (_reportcreteria.typeid > 0)
                    SQL_Where.Append(" and items.typeid=" + _reportcreteria.typeid);
                if (_reportcreteria.patternid > 0)
                    SQL_Where.Append(" and items.patternid=" + _reportcreteria.patternid);
                if (_reportcreteria.styleid > 0)
                    SQL_Where.Append(" and items.styleid=" + _reportcreteria.styleid);
                if (_reportcreteria.sizeid > 0)
                    SQL_Where.Append(" and items.sizeid=" + _reportcreteria.sizeid);
                if (_reportcreteria.sleeveid > 0)
                    SQL_Where.Append(" and items.sleeveid=" + _reportcreteria.sleeveid);
                if (_reportcreteria.supplierid > 0)
                    SQL_Where.Append(" and lrentry.supplierid=" + _reportcreteria.supplierid);
                if (_reportcreteria.sourcesupplierid > 0)
                    SQL_Where.Append(" and lrentry.sourcesupplierid=" + _reportcreteria.sourcesupplierid);
                if (_reportcreteria.sectionid > 0)
                    SQL_Where.Append(" and products.salescounterid=" + _reportcreteria.sectionid);

                if (!string.IsNullOrEmpty(_reportcreteria.designid))
                {
                    if (_reportcreteria.designid.IndexOf(",") > 0)
                    {
                        string designquery = string.Join(" or ", _reportcreteria.designid.Split(',').ToList().ConvertAll(x => "stock.designid ilike '" + WebAPI.Data.Utility.SafeSQL(x) + "'"));
                        qb.Add2Query("(" + designquery + ")");
                    }
                    else
                    {
                        SQL_Where.Append(" and lower(stock.designid)='" + WebAPI.Data.Utility.SafeSQL(_reportcreteria.designid.ToLower()) + "'");
                    }
                }

                if (!string.IsNullOrEmpty(_reportcreteria.barcode))
                {
                    if (_reportcreteria.barcode.IndexOf("%") >= 0)
                    {
                        if (!_reportcreteria.barcode.StartsWith("%") && _reportcreteria.barcode.EndsWith("%") && _reportcreteria.barcode.Split('%').Length == 2)
                            SQL_Where.Append(" and stock.barcode ~* '" + WebAPI.Data.Utility.SafeSQL(_reportcreteria.barcode.Replace("%", "")) + "[0-9]+$'");
                        else
                            SQL_Where.Append(" and stock.barcode like '" + WebAPI.Data.Utility.SafeSQL(_reportcreteria.barcode) + "'");
                    }
                    else
                        SQL_Where.Append(" and lower(stock.barcode)='" + WebAPI.Data.Utility.SafeSQL(_reportcreteria.barcode.ToLower()) + "'");
                }
                qb.CurrentQuery = SQL_Select.ToString() + SQL_From.ToString() + SQL_Where.ToString()
                    + SQL_Groupby.ToString(0, SQL_Groupby.Length - 1) + SQL_Orderby.ToString(0, SQL_Orderby.Length - 1);

                command = qb.GetCommand(conn);
                if (Services.ConfigManager.Instance(Context.OrganizationID).Config.analyser_stock_removeaqty)
                    command.CommandText = command.CommandText.Replace("salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty)", "salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty+salablegoods.aqty)");

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        StockReport oitem = new StockReport();

                        if (!_reportcreteria.product && !_reportcreteria.brand && !_reportcreteria.material && !_reportcreteria.pattern
                            && !_reportcreteria.style && !_reportcreteria.size && !_reportcreteria.section && !_reportcreteria.supplier && !_reportcreteria.sourcesupplier)
                            oitem.productname = reader["column1"] == DBNull.Value ? "" : reader["column1"].ToString();
                        if (_reportcreteria.product)
                            oitem.productname = reader["column1"] == DBNull.Value ? "" : reader["column1"].ToString();
                        if (_reportcreteria.brand)
                            oitem.brandname = reader["column2"] == DBNull.Value ? "" : reader["column2"].ToString();
                        if (_reportcreteria.material)
                            oitem.materialname = reader["column3"] == DBNull.Value ? "" : reader["column3"].ToString();
                        if (_reportcreteria.pattern)
                            oitem.patternname = reader["column4"] == DBNull.Value ? "" : reader["column4"].ToString();
                        if (_reportcreteria.style)
                            oitem.stylename = reader["column5"] == DBNull.Value ? "" : reader["column5"].ToString();
                        if (_reportcreteria.size)
                            oitem.sizename = reader["column6"] == DBNull.Value ? "" : reader["column6"].ToString();
                        if (_reportcreteria.design)
                            oitem.designid = reader["column7"] == DBNull.Value ? "" : reader["column7"].ToString();
                        if (_reportcreteria.supplier)
                            oitem.supplierid = reader["column8"] == DBNull.Value ? 0 : Convert.ToInt64(reader["column8"]);
                        if (_reportcreteria.section)
                            oitem.sectionid = reader["column9"] == DBNull.Value ? 0 : Convert.ToInt64(reader["column9"]);
                        if (_reportcreteria.isbarcode)
                            oitem.barcode = reader["column10"] == DBNull.Value ? "" : reader["column10"].ToString();
                        if (_reportcreteria.type)
                            oitem.typename = reader["column11"] == DBNull.Value ? "" : reader["column11"].ToString();
                        if (_reportcreteria.sourcesupplier)
                            oitem.sourcesupplierid = reader["column12"] == DBNull.Value ? 0 : Convert.ToInt64(reader["column12"]);
                        if (_reportcreteria.sleeve)
                            oitem.sleevename = reader["column13"] == DBNull.Value ? "" : reader["column13"].ToString();

                        oitem.qty = reader["qty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["qty"]);
                        if (!Context.HasPermission(PermissionSet.showvalue, InfyPOS.Common.Action.View))
                        {
                            oitem.price = reader["ivalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["ivalue"]);
                            oitem.buyingprice = reader["value"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["value"]);
                        }
                        if (!Context.HasPermission(PermissionSet.showsellingvalue, InfyPOS.Common.Action.View))
                            oitem.salevalue = reader["salevalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["salevalue"]);
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

        public List<PurhcaseItemDetails> SearchPurchaseItemDetails(WarehouseReportCreteria _reportcreteria)
        {
            List<PurhcaseItemDetails> lst = new List<PurhcaseItemDetails>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                NpgsqlCommand command = null;

                QueryBuilder qb = new QueryBuilder();
                qb.CurrentQuery = SQL_PurchaseItemDetails;

                if (_reportcreteria.multicompany && _reportcreteria.companyid > 0)
                    qb.AddParameter("lrinvoice.companyid", _reportcreteria.companyid, "=");

                if (_reportcreteria.fromdate != DateTime.MinValue && _reportcreteria.todate != DateTime.MinValue)
                {
                    qb.AddParameter("lrinvoice.entrydate", "datefrom", _reportcreteria.fromdate.Date, ">=");
                    qb.AddParameter("lrinvoice.entrydate", "dateto", _reportcreteria.todate.Date.AddDays(1), "<");
                }
                if (_reportcreteria.fromlrdate != DateTime.MinValue && _reportcreteria.tolrdate != DateTime.MinValue)
                {
                    qb.AddParameter("lrinvoice.invoicedate", "lrdatefrom", _reportcreteria.fromlrdate.Date, ">=");
                    qb.AddParameter("lrinvoice.invoicedate", "lrdateto", _reportcreteria.tolrdate.Date.AddDays(1), "<");
                }

                if (_reportcreteria.supplierid > 0)
                    qb.AddParameter("lrentry.supplierid", _reportcreteria.supplierid, "=");

                qb.CurrentQuery += @" group by lrentry.id,lrinvoice.id, company.name, suppliercompany.name, suppliercompany.cityid,
                                            lrentryno, lrinvoice.entrydate, lrinvoice.invoiceno, lrinvoice.invoicedate, lrinvoice.isinterstatesale, lrinvoiceitems.purchasetaxpercentage,
                                            products.name, products.salescounterid
                                            order by lrentry.id, lrinvoice.id ";
                command = qb.GetCommand(conn);
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        PurhcaseItemDetails oitem = new PurhcaseItemDetails();
                        oitem.invoiceid = reader["invoiceid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["invoiceid"]);
                        oitem.company = reader["company"] == DBNull.Value ? "" : reader["company"].ToString();
                        oitem.supplier = reader["supplier"] == DBNull.Value ? "" : reader["supplier"].ToString();
                        oitem.productname = reader["productname"] == DBNull.Value ? "" : reader["productname"].ToString();
                        oitem.sectionid = reader["salescounterid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["salescounterid"]);
                        oitem.cityid = reader["cityid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["cityid"]);
                        oitem.isinterstatesale = reader["isinterstatesale"] == DBNull.Value ? false : Convert.ToBoolean(reader["isinterstatesale"]);
                        oitem.lrentryno = reader["lrentryno"] == DBNull.Value ? "" : reader["lrentryno"].ToString();
                        oitem.lrentrydate = reader["entrydate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["entrydate"]);
                        oitem.invoiceno = reader["invoiceno"] == DBNull.Value ? "" : reader["invoiceno"].ToString();
                        oitem.invoicedate = reader["invoicedate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["invoicedate"]);
                        oitem.percentage = reader["purchasetaxpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["purchasetaxpercentage"]);
                        oitem.qty = reader["qty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["qty"]);
                        oitem.taxableamount = reader["grossamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["grossamount"]);
                        oitem.taxamount = reader["tax"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["tax"]);
                        oitem.discount = reader["discount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discount"]);
                        oitem.basebuyingprice = reader["baseamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["baseamount"]);
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
        public List<PurhcaseItemDetails> SearchPurchaseSectionwise(WarehouseReportCreteria _reportcreteria)
        {
            const string SQL_PurchaseSectionwise = @"select products.salescounterid,
sum(lrinvoiceitems.buyingqty) qty, sum(lrinvoiceitems.buyingqty::numeric*lrinvoiceitems.basebuyingprice::numeric) baseamount, sum(lrinvoiceitems.buyingqty::numeric*lrinvoiceitems.buyingprice::numeric) grossamount,
sum(lrinvoiceitems.purchasetax) tax, sum(lrinvoiceitems.buyingqty::numeric*lrinvoiceitems.purchasediscount::numeric) discount
from lrentry inner join lrinvoice on lrentry.id=lrinvoice.lrid and lrinvoice.isactive=true
inner join suppliercompany on suppliercompany.id=lrinvoice.suppliercompanyid 
inner join lrinvoiceitems on lrinvoiceitems.lrinvoiceid=lrinvoice.id and lrinvoiceitems.isactive = true
inner join products on products.id = lrinvoiceitems.productid
inner join company on company.id=lrinvoice.companyid ";

            const string SQL_PurchaseProductwise = @"select products.name productname, products.salescounterid,
sum(lrinvoiceitems.buyingqty) qty, sum(lrinvoiceitems.buyingqty::numeric*lrinvoiceitems.basebuyingprice::numeric) baseamount, sum(lrinvoiceitems.buyingqty::numeric*lrinvoiceitems.buyingprice::numeric) grossamount,
sum(lrinvoiceitems.purchasetax) tax, sum(lrinvoiceitems.buyingqty::numeric*lrinvoiceitems.purchasediscount::numeric) discount
from lrentry inner join lrinvoice on lrentry.id=lrinvoice.lrid and lrinvoice.isactive=true
inner join suppliercompany on suppliercompany.id=lrinvoice.suppliercompanyid 
inner join lrinvoiceitems on lrinvoiceitems.lrinvoiceid=lrinvoice.id and lrinvoiceitems.isactive = true
inner join products on products.id = lrinvoiceitems.productid
inner join company on company.id=lrinvoice.companyid ";

            List<PurhcaseItemDetails> lst = new List<PurhcaseItemDetails>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                NpgsqlCommand command = null;

                QueryBuilder qb = new QueryBuilder();

                if (_reportcreteria.showdetail)
                    qb.CurrentQuery = SQL_PurchaseProductwise;
                else
                    qb.CurrentQuery = SQL_PurchaseSectionwise;

                if (_reportcreteria.multicompany && _reportcreteria.companyid > 0)
                    qb.AddParameter("lrinvoice.companyid", _reportcreteria.companyid, "=");

                if (_reportcreteria.fromdate != DateTime.MinValue && _reportcreteria.todate != DateTime.MinValue)
                {
                    qb.AddParameter("lrinvoice.entrydate", "datefrom", _reportcreteria.fromdate.Date, ">=");
                    qb.AddParameter("lrinvoice.entrydate", "dateto", _reportcreteria.todate.Date.AddDays(1), "<");
                }
                if (_reportcreteria.fromlrdate != DateTime.MinValue && _reportcreteria.tolrdate != DateTime.MinValue)
                {
                    qb.AddParameter("lrinvoice.invoicedate", "lrdatefrom", _reportcreteria.fromlrdate.Date, ">=");
                    qb.AddParameter("lrinvoice.invoicedate", "lrdateto", _reportcreteria.tolrdate.Date.AddDays(1), "<");
                }

                if (_reportcreteria.supplierid > 0)
                    qb.AddParameter("lrentry.supplierid", _reportcreteria.supplierid, "=");

                if (_reportcreteria.showdetail)
                    qb.CurrentQuery += @" group by products.salescounterid, products.name order by  products.salescounterid, products.name";
                else
                    qb.CurrentQuery += @" group by products.salescounterid order by  products.salescounterid";

                command = qb.GetCommand(conn);
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        PurhcaseItemDetails oitem = new PurhcaseItemDetails();
                        if (_reportcreteria.showdetail)
                            oitem.productname = reader["productname"] == DBNull.Value ? "" : reader["productname"].ToString();
                        oitem.sectionid = reader["salescounterid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["salescounterid"]);
                        oitem.qty = reader["qty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["qty"]);
                        oitem.taxableamount = reader["grossamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["grossamount"]);
                        oitem.taxamount = reader["tax"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["tax"]);
                        oitem.discount = reader["discount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discount"]);
                        oitem.basebuyingprice = reader["baseamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["baseamount"]);
                        oitem.netvalue = oitem.taxableamount + oitem.taxamount;
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

        public List<AsOnStock> SearchAsOnStockV1(StockReportCreteria _reportcreteria)
        {
            const string SQL_whStock = @"select {0}, sum(stock.qty-(stock.transfered+stock.journalqty+stock.damagedqty+stock.returnqty)) qty, 
sum( (stock.qty - (stock.transfered+stock.returnqty+stock.journalqty+stock.damagedqty)) * stock.buyingprice::numeric) as stockvalue,
sum( (stock.qty - (stock.transfered+stock.returnqty+stock.journalqty+stock.damagedqty)) * stock.ibuyingprice::numeric) as istockvalue,
sum( (stock.qty - (stock.transfered+stock.returnqty+stock.journalqty+stock.damagedqty)) * (stock.price-stock.discount)) as salevalue
from lrentry inner join lrinvoice on lrentry.id=lrinvoice.lrid and lrinvoice.isactive=true
inner join lrinvoiceitems on lrinvoiceitems.lrinvoiceid = lrinvoice.id and lrinvoiceitems.isactive = true 
inner join stock on stock.invoiceitemid = lrinvoiceitems.id and stock.isactive = true
inner join supplier on supplier.id = lrentry.supplierid
inner join products on products.id = stock.productid
left outer join referencelist sections on sections.id = products.salescounterid
Where stock.isactive=true 
and stock.hasstock=true and stock.qty-(stock.transfered+stock.journalqty+stock.damagedqty+stock.returnqty)>0 
and lrinvoice.entrydate < @fromenddate and stock.companyid=@companyid {2}
group by {1}
UNION ALL
select {0}, sum(stock.qty-(stock.transfered+stock.journalqty+stock.damagedqty+stock.returnqty)) qty, 
sum( (stock.qty - (stock.transfered+stock.returnqty+stock.journalqty+stock.damagedqty)) * stock.buyingprice::numeric) as stockvalue,
sum( (stock.qty - (stock.transfered+stock.returnqty+stock.journalqty+stock.damagedqty)) * stock.ibuyingprice::numeric) as istockvalue,
sum( (stock.qty - (stock.transfered+stock.returnqty+stock.journalqty+stock.damagedqty)) * (stock.price-stock.discount)) as salevalue
from stock inner join supplier on supplier.id = stock.supplierid
left outer join products on products.id = stock.productid
left outer join referencelist sections on sections.id = products.salescounterid
Where stock.isactive=true 
and stock.hasstock=true and stock.qty-(stock.transfered+stock.journalqty+stock.damagedqty+stock.returnqty)>0 
and stock.createdon < @fromenddate and stock.invoiceitemid=-1 and stock.companyid=@companyid {2}
group by {1}
";

            string SQL_retStock = @"select {0}, sum(salablegoods.iqty-(salablegoods.oqty+salablegoods.jqty+salablegoods.dqty)) qty, 
sum( (salablegoods.iqty-(salablegoods.oqty+salablegoods.jqty+salablegoods.dqty)) * stock.buyingprice::numeric) as stockvalue,
sum( (salablegoods.iqty-(salablegoods.oqty+salablegoods.jqty+salablegoods.dqty)) * stock.ibuyingprice::numeric) as istockvalue,
sum( (salablegoods.iqty-(salablegoods.oqty+salablegoods.jqty+salablegoods.dqty)) * (salablegoods.price-salablegoods.discount)) as salevalue
from lrentry inner join lrinvoice on lrentry.id=lrinvoice.lrid and lrinvoice.isactive=true
inner join lrinvoiceitems on lrinvoiceitems.lrinvoiceid = lrinvoice.id and lrinvoiceitems.isactive = true 
inner join stock on stock.invoiceitemid = lrinvoiceitems.id and stock.isactive = true
inner join salablegoods on salablegoods.stockid = stock.id and salablegoods.isactive = true
inner join supplier on supplier.id = lrentry.supplierid
inner join products on products.id = stock.productid
left outer join referencelist sections on sections.id = products.salescounterid
where salablegoods.isactive=true 
and salablegoods.hasstock=true and salablegoods.iqty-(salablegoods.oqty+salablegoods.jqty+salablegoods.dqty)>0 
and lrinvoice.entrydate < @fromenddate and salablegoods.companyid=@companyid {2}
group by {1}
UNION ALL
select {0}, sum(salablegoods.iqty-(salablegoods.oqty+salablegoods.jqty+salablegoods.dqty)) qty, 
sum( (salablegoods.iqty-(salablegoods.oqty+salablegoods.jqty+salablegoods.dqty)) * stock.buyingprice::numeric) as stockvalue,
sum( (salablegoods.iqty-(salablegoods.oqty+salablegoods.jqty+salablegoods.dqty)) * stock.ibuyingprice::numeric) as istockvalue,
sum( (salablegoods.iqty-(salablegoods.oqty+salablegoods.jqty+salablegoods.dqty)) * (salablegoods.price-salablegoods.discount)) as salevalue
from salablegoods inner join stock on stock.id = salablegoods.stockid and salablegoods.isactive = true
left outer join supplier on supplier.id = stock.supplierid
left outer join products on products.id = stock.productid
left outer join referencelist sections on sections.id = products.salescounterid
where salablegoods.isactive=true 
and salablegoods.hasstock=true and salablegoods.iqty-(salablegoods.oqty+salablegoods.jqty+salablegoods.dqty)>0 
and stock.createdon < @fromenddate and stock.invoiceitemid=-1 and salablegoods.companyid=@companyid {2}
group by {1}
";

            const string SQL_whStock_current = @"select {0}, sum(stock.qty-(stock.transfered+stock.journalqty+stock.damagedqty+stock.returnqty)) qty, 
sum( (stock.qty - (stock.transfered+stock.returnqty+stock.journalqty+stock.damagedqty)) * stock.buyingprice::numeric) as stockvalue,
sum( (stock.qty - (stock.transfered+stock.returnqty+stock.journalqty+stock.damagedqty)) * stock.ibuyingprice::numeric) as istockvalue,
sum( (stock.qty - (stock.transfered+stock.returnqty+stock.journalqty+stock.damagedqty)) * (stock.price-stock.discount)) as salevalue
from stock
left outer join products on products.id = stock.productid
left outer join items on items.id = stock.itemid 
left outer join lrinvoiceitems on lrinvoiceitems.id = stock.invoiceitemid and lrinvoiceitems.isactive = true
left outer join lrinvoice on lrinvoice.id = lrinvoiceitems.lrinvoiceid and lrinvoice.isactive = true
left outer join lrentry  on lrentry.id = lrid and lrentry.isactive = true
left outer join supplier on supplier.id = lrentry.supplierid
left outer join suppliercompany on suppliercompany.id = lrinvoice.suppliercompanyid
left outer join referencelist sections on sections.id = products.salescounterid
Where stock.isactive=true 
and stock.hasstock=true and stock.qty-(stock.transfered+stock.journalqty+stock.damagedqty+stock.returnqty)>0 
and stock.companyid=@companyid {2}
group by {1}";

            string SQL_retStock_current = @"select {0}, sum(salablegoods.iqty-(salablegoods.oqty+salablegoods.jqty+salablegoods.dqty)) qty, 
sum( (salablegoods.iqty-(salablegoods.oqty+salablegoods.jqty+salablegoods.dqty)) * stock.buyingprice::numeric) as stockvalue,
sum( (salablegoods.iqty-(salablegoods.oqty+salablegoods.jqty+salablegoods.dqty)) * stock.ibuyingprice::numeric) as istockvalue,
sum( (salablegoods.iqty-(salablegoods.oqty+salablegoods.jqty+salablegoods.dqty)) * (salablegoods.price-salablegoods.discount)) as salevalue
from salablegoods 
inner join stock on stock.id = salablegoods.stockid and stock.isactive = true
left outer join products on products.id = stock.productid
left outer join items on items.id = stock.itemid 
left outer join lrinvoiceitems on lrinvoiceitems.id = stock.invoiceitemid and lrinvoiceitems.isactive = true
left outer join lrinvoice on lrinvoice.id = lrinvoiceitems.lrinvoiceid and lrinvoice.isactive = true
left outer join lrentry  on lrentry.id = lrid and lrentry.isactive = true
left outer join supplier on supplier.id = lrentry.supplierid 
left outer join suppliercompany on suppliercompany.id = lrinvoice.suppliercompanyid
left outer join referencelist sections on sections.id = products.salescounterid
where salablegoods.isactive = true 
and salablegoods.hasstock=true and salablegoods.iqty-(salablegoods.oqty+salablegoods.jqty+salablegoods.dqty)>0 
and salablegoods.companyid=@companyid {2}
group by {1}";

            const string SQL_autidStock = @"select {0}, sum(salablegoodshistory.auditqty) qty, 
sum( salablegoodshistory.auditqty * stock.buyingprice::numeric) as stockvalue,
sum( salablegoodshistory.auditqty * stock.ibuyingprice::numeric) as istockvalue,
sum( salablegoodshistory.auditqty * (salablegoods.price-salablegoods.discount) ) as salevalue
from lrentry inner join lrinvoice on lrentry.id=lrinvoice.lrid and lrinvoice.isactive=true
inner join lrinvoiceitems on lrinvoiceitems.lrinvoiceid = lrinvoice.id and lrinvoiceitems.isactive = true 
inner join stock on stock.invoiceitemid = lrinvoiceitems.id and stock.isactive = true
inner join salablegoods on salablegoods.stockid = stock.id and salablegoods.isactive = true
inner join salablegoodshistory on salablegoodshistory.salablegoodsid=salablegoods.id
inner join supplier on supplier.id = lrentry.supplierid
inner join products on products.id = stock.productid
left outer join referencelist sections on sections.id = products.salescounterid
where salablegoods.isactive=true 
and salablegoodshistory.createdon>=@fromenddate
and lrinvoice.entrydate < @fromenddate and salablegoods.companyid=@companyid {2}
group by {1}";

            const string SQL_autidwarehouse = @"select {0}, sum(stockhistory.auditqty) qty, 
sum( stockhistory.auditqty * stock.buyingprice::numeric) as stockvalue,
sum( stockhistory.auditqty * stock.ibuyingprice::numeric) as istockvalue,
sum( stockhistory.auditqty * (stock.price-stock.discount)) as salevalue
from lrentry inner join lrinvoice on lrentry.id=lrinvoice.lrid and lrinvoice.isactive=true
inner join lrinvoiceitems on lrinvoiceitems.lrinvoiceid = lrinvoice.id and lrinvoiceitems.isactive = true 
inner join stock on stock.invoiceitemid = lrinvoiceitems.id and stock.isactive = true
inner join stockhistory on stockhistory.stockid=stock.id 
inner join supplier on supplier.id = lrentry.supplierid
inner join products on products.id = stock.productid
left outer join referencelist sections on sections.id = products.salescounterid
where stock.isactive=true 
and stockhistory.createdon>=@fromenddate
and lrinvoice.entrydate < @fromenddate and stock.companyid=@companyid {2}
group by {1}";

            const string SQL_purchaseret = @"select {0}, sum(lrinvoicereturnitems.qty) qty, 
sum(lrinvoicereturnitems.amount) stockvalue,
sum(lrinvoicereturnitems.amount + lrinvoicereturnitems.tax) istockvalue,
sum(case when salablegoods.id is not null then lrinvoicereturnitems.qty else 0 end) retqty, 
sum(case when salablegoods.id is not null then lrinvoicereturnitems.amount else 0 end) retstockvalue,
sum(case when salablegoods.id is not null then (lrinvoicereturnitems.amount + lrinvoicereturnitems.tax) else 0 end) retistockvalue,
sum(lrinvoicereturnitems.qty * (stock.price-stock.discount)) salevalue
from lrinvoicereturn 
inner join lrinvoicereturnitems on lrinvoicereturnitems.lrinvoicereturnid = lrinvoicereturn.id
inner join stock on stock.id = lrinvoicereturnitems.stockid 
inner join lrinvoiceitems on  lrinvoiceitems.id = stock.invoiceitemid
inner join lrinvoice on lrinvoice.id = lrinvoiceitems.lrinvoiceid
inner join lrentry on lrentry.id = lrinvoice.lrid
inner join supplier on supplier.id = lrentry.supplierid
inner join products on products.id = stock.productid
left outer join salablegoods on salablegoods.id = lrinvoicereturnitems.salablegoodsid
left outer join referencelist sections on sections.id = products.salescounterid
where lrinvoicereturn.isactive = true and lrinvoicereturnitems.isactive = true
and lrinvoicereturn.companyid=@companyid 
and lrinvoice.entrydate < @fromenddate and lrinvoicereturn.returndate >= @fromdate 
and lrinvoicereturn.returndate < @todate {2}
group by {1} ";

            const string SQL_sale = @"Select {0}, sum(billitems.qty) qty, sum(billitems.qty * stock.buyingprice::numeric) stockvalue,
sum(billitems.qty * stock.ibuyingprice::numeric) istockvalue,
sum(billitems.qty * (salablegoods.price-salablegoods.discount)) salevalue
from bill inner join billitems on billitems.billid = bill.id and billitems.isactive = true
inner join salablegoods on salablegoods.id = billitems.salablegoodsid 
inner join stock on stock.id = salablegoods.stockid 
left outer join lrinvoiceitems on  lrinvoiceitems.id = stock.invoiceitemid
left outer join lrinvoice on lrinvoice.id = lrinvoiceitems.lrinvoiceid
left outer join lrentry on lrentry.id = lrinvoice.lrid
left outer join supplier on supplier.id = lrentry.supplierid
left outer join products on products.id = stock.productid
left outer join referencelist sections on sections.id = products.salescounterid
where bill.isactive = true and bill.billtype !=9 and bill.companyid=@companyid 
and stock.stockdate < @fromenddate and bill.billdate >= @fromdate and bill.billdate < @todate {2}
group by {1} ";

            List<AsOnStock> lst = new List<AsOnStock>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                NpgsqlCommand command = null;
                DateTime today = _reportcreteria.fromdate.Date == DateTime.MinValue ? DateTimeNow.Date : _reportcreteria.fromdate.Date;
                DateTime nextdate = DateTimeNow.AddDays(1).Date;

                List<string> field1 = new List<string>();
                List<string> field2 = new List<string>();
                string _condition = "";
                if (!_reportcreteria.product && !_reportcreteria.supplier && !_reportcreteria.section)
                    _reportcreteria.product = true;

                if (_reportcreteria.product)
                {
                    field1.Add("products.name as column1");
                    field2.Add("products.name");
                }
                else if (_reportcreteria.supplier)
                {
                    field1.Add("supplier.name as column1");
                    field2.Add("supplier.name");
                }
                else if (_reportcreteria.section)
                {
                    field1.Add("sections.name as column1");
                    field2.Add("sections.name");
                }

                if (_reportcreteria.productid > 0)
                    _condition = "and stock.productid=@productid ";
                if (_reportcreteria.supplierid > 0)
                    _condition = "and stock.supplierid=@supplierid ";
                if (_reportcreteria.sectionid > 0)
                    _condition = "and sections.id=@sectionid ";

                if (_reportcreteria.locationid > 0)
                {
                    _condition += "and salablegoods.locationid=@locationid ";
                }

                if (Services.ConfigManager.Instance(Context.OrganizationID).Config.analyser_stock_removeaqty)
                    SQL_retStock = SQL_retStock.Replace("salablegoods.iqty-(salablegoods.oqty+salablegoods.jqty+salablegoods.dqty)", "salablegoods.iqty-(salablegoods.oqty+salablegoods.jqty+salablegoods.dqty+salablegoods.aqty)");

                command = GetCommand(string.Format(SQL_retStock, string.Join(",", field1), string.Join(",", field2), _condition), conn);
                command.Parameters.Add("supplierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _reportcreteria.supplierid;
                command.Parameters.Add("sectionid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _reportcreteria.sectionid;
                command.Parameters.Add("productid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _reportcreteria.productid;
                command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _reportcreteria.companyid;
                command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _reportcreteria.locationid;
                command.Parameters.Add("fromdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = today;
                command.Parameters.Add("fromenddate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = today.AddDays(1);
                command.Parameters.Add("todate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = nextdate;
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string name = reader["column1"] == DBNull.Value ? "" : reader["column1"].ToString();
                        if (lst.Exists(e => e.productname.ToLower() == name.ToLower()))
                        {
                            AsOnStock oitem = lst.Find(e => e.productname.ToLower() == name.ToLower());
                            oitem.asstockqty += reader["qty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["qty"]);
                            oitem.asstockvalue += reader["stockvalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["stockvalue"]);
                            oitem.iasstockvalue += reader["istockvalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["istockvalue"]);
                            oitem.assalevalue += reader["salevalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["salevalue"]);
                        }
                        else
                        {
                            AsOnStock oitem = new AsOnStock();
                            oitem.productname = reader["column1"] == DBNull.Value ? "" : reader["column1"].ToString();
                            oitem.asstockqty = reader["qty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["qty"]);
                            oitem.asstockvalue = reader["stockvalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["stockvalue"]);
                            oitem.iasstockvalue = reader["istockvalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["istockvalue"]);
                            oitem.assalevalue += reader["salevalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["salevalue"]);
                            lst.Add(oitem);
                        }
                    }
                }

                if (_reportcreteria.locationid <= 0)
                {
                    command = GetCommand(string.Format(SQL_whStock, string.Join(",", field1), string.Join(",", field2), _condition), conn);
                    command.Parameters.Add("supplierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _reportcreteria.supplierid;
                    command.Parameters.Add("sectionid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _reportcreteria.sectionid;
                    command.Parameters.Add("productid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _reportcreteria.productid;
                    command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _reportcreteria.companyid;
                    command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _reportcreteria.locationid;
                    command.Parameters.Add("fromdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = today;
                    command.Parameters.Add("fromenddate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = today.AddDays(1);
                    command.Parameters.Add("todate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = nextdate;
                    using (System.Data.IDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string name = reader["column1"] == DBNull.Value ? "" : reader["column1"].ToString();
                            if (lst.Exists(e => e.productname.ToLower() == name.ToLower()))
                            {
                                AsOnStock oitem = lst.Find(e => e.productname.ToLower() == name.ToLower());
                                oitem.asstockqty += reader["qty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["qty"]);
                                oitem.asstockvalue += reader["stockvalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["stockvalue"]);
                                oitem.iasstockvalue += reader["istockvalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["istockvalue"]);
                                oitem.assalevalue += reader["salevalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["salevalue"]);
                            }
                            else
                            {
                                AsOnStock oitem = new AsOnStock();
                                oitem.productname = name;
                                oitem.asstockqty = reader["qty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["qty"]);
                                oitem.asstockvalue = reader["stockvalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["stockvalue"]);
                                oitem.iasstockvalue = reader["istockvalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["istockvalue"]);
                                oitem.assalevalue += reader["salevalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["salevalue"]);
                                lst.Add(oitem);
                            }
                        }
                    }



                    command = GetCommand(string.Format(SQL_whStock_current, string.Join(",", field1), string.Join(",", field2), _condition), conn);
                    command.Parameters.Add("supplierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _reportcreteria.supplierid;
                    command.Parameters.Add("sectionid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _reportcreteria.sectionid;
                    command.Parameters.Add("productid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _reportcreteria.productid;
                    command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _reportcreteria.companyid;
                    command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _reportcreteria.locationid;
                    command.Parameters.Add("fromdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = today;
                    command.Parameters.Add("fromenddate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = today.AddDays(1);
                    command.Parameters.Add("todate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = nextdate;
                    using (System.Data.IDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string name = reader["column1"] == DBNull.Value ? "" : reader["column1"].ToString();
                            if (lst.Exists(e => e.productname.ToLower() == name.ToLower()))
                            {
                                AsOnStock oitem = lst.Find(e => e.productname.ToLower() == name.ToLower());
                                oitem.currentqty = reader["qty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["qty"]);
                                oitem.currentvalue = reader["stockvalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["stockvalue"]);
                                oitem.icurrentvalue = reader["istockvalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["istockvalue"]);
                                oitem.currentsalevalue += reader["salevalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["salevalue"]);
                            }
                            else
                            {
                                AsOnStock oitem = new AsOnStock();
                                oitem.productname = name;
                                oitem.currentqty = reader["qty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["qty"]);
                                oitem.currentvalue = reader["stockvalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["stockvalue"]);
                                oitem.icurrentvalue = reader["istockvalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["istockvalue"]);
                                oitem.currentsalevalue += reader["salevalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["salevalue"]);
                                lst.Add(oitem);
                            }
                        }
                    }
                }

                if (Services.ConfigManager.Instance(Context.OrganizationID).Config.analyser_stock_removeaqty)
                    SQL_retStock_current = SQL_retStock_current.Replace("salablegoods.iqty-(salablegoods.oqty+salablegoods.jqty+salablegoods.dqty)", "salablegoods.iqty-(salablegoods.oqty+salablegoods.jqty+salablegoods.dqty+salablegoods.aqty)");

                command = GetCommand(string.Format(SQL_retStock_current, string.Join(",", field1), string.Join(",", field2), _condition), conn);
                command.Parameters.Add("supplierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _reportcreteria.supplierid;
                command.Parameters.Add("sectionid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _reportcreteria.sectionid;
                command.Parameters.Add("productid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _reportcreteria.productid;
                command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _reportcreteria.companyid;
                command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _reportcreteria.locationid;
                command.Parameters.Add("fromdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = today;
                command.Parameters.Add("fromenddate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = today.AddDays(1);
                command.Parameters.Add("todate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = nextdate;
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string name = reader["column1"] == DBNull.Value ? "" : reader["column1"].ToString();
                        if (lst.Exists(e => e.productname.ToLower() == name.ToLower()))
                        {
                            AsOnStock oitem = lst.Find(e => e.productname.ToLower() == name.ToLower());
                            oitem.currentqty += reader["qty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["qty"]);
                            oitem.currentvalue += reader["stockvalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["stockvalue"]);
                            oitem.icurrentvalue += reader["istockvalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["istockvalue"]);
                            oitem.currentsalevalue += reader["salevalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["salevalue"]);
                        }
                        else
                        {
                            AsOnStock oitem = new AsOnStock();
                            oitem.productname = name;
                            oitem.currentqty = reader["qty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["qty"]);
                            oitem.currentvalue = reader["stockvalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["stockvalue"]);
                            oitem.icurrentvalue = reader["istockvalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["istockvalue"]);
                            oitem.currentsalevalue += reader["salevalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["salevalue"]);
                            lst.Add(oitem);
                        }
                    }
                }

                command = GetCommand(string.Format(SQL_purchaseret, string.Join(",", field1), string.Join(",", field2), _condition), conn);
                command.Parameters.Add("supplierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _reportcreteria.supplierid;
                command.Parameters.Add("sectionid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _reportcreteria.sectionid;
                command.Parameters.Add("productid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _reportcreteria.productid;
                command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _reportcreteria.companyid;
                command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _reportcreteria.locationid;
                command.Parameters.Add("fromdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = today;
                command.Parameters.Add("fromenddate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = today.AddDays(1);
                command.Parameters.Add("todate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = nextdate;
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string name = reader["column1"] == DBNull.Value ? "" : reader["column1"].ToString();
                        if (lst.Exists(e => e.productname.ToLower() == name.ToLower()))
                        {
                            AsOnStock oitem = lst.Find(e => e.productname.ToLower() == name.ToLower());
                            if (_reportcreteria.locationid > 0)
                            {
                                oitem.purchasereturnqty = reader["retqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["retqty"]);
                                oitem.purchasereturnvalue = reader["retstockvalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["retstockvalue"]);
                                oitem.ipurchasereturnvalue = reader["retistockvalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["retistockvalue"]);
                                oitem.purchasesalevalue += reader["salevalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["salevalue"]);
                            }
                            else
                            {
                                oitem.purchasereturnqty = reader["qty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["qty"]);
                                oitem.purchasereturnvalue = reader["stockvalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["stockvalue"]);
                                oitem.ipurchasereturnvalue = reader["istockvalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["istockvalue"]);
                                oitem.purchasesalevalue += reader["salevalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["salevalue"]);
                            }
                        }
                        else
                        {
                            AsOnStock oitem = new AsOnStock();
                            oitem.productname = name;
                            if (_reportcreteria.locationid > 0)
                            {
                                oitem.purchasereturnqty = reader["retqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["retqty"]);
                                oitem.purchasereturnvalue = reader["retstockvalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["retstockvalue"]);
                                oitem.ipurchasereturnvalue = reader["retistockvalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["retistockvalue"]);
                                oitem.purchaseretsalevalue += reader["salevalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["salevalue"]);
                            }
                            else
                            {
                                oitem.purchasereturnqty = reader["qty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["qty"]);
                                oitem.purchasereturnvalue = reader["stockvalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["stockvalue"]);
                                oitem.ipurchasereturnvalue = reader["istockvalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["istockvalue"]);
                                oitem.purchaseretsalevalue += reader["salevalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["salevalue"]);
                            }
                            lst.Add(oitem);
                        }
                    }
                }

                command = GetCommand(string.Format(SQL_sale, string.Join(",", field1), string.Join(",", field2), _condition), conn);
                command.Parameters.Add("supplierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _reportcreteria.supplierid;
                command.Parameters.Add("sectionid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _reportcreteria.sectionid;
                command.Parameters.Add("productid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _reportcreteria.productid;
                command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _reportcreteria.companyid;
                command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _reportcreteria.locationid;
                command.Parameters.Add("fromdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = today;
                command.Parameters.Add("fromenddate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = today.AddDays(1);
                command.Parameters.Add("todate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = nextdate;
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string name = reader["column1"] == DBNull.Value ? "" : reader["column1"].ToString();
                        if (lst.Exists(e => e.productname.ToLower() == name.ToLower()))
                        {
                            AsOnStock oitem = lst.Find(e => e.productname.ToLower() == name.ToLower());
                            oitem.saleqty = reader["qty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["qty"]);
                            oitem.salevalue = reader["stockvalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["stockvalue"]);
                            oitem.isalevalue = reader["istockvalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["istockvalue"]);
                            oitem.salesalevalue = reader["salevalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["salevalue"]);
                        }
                        else
                        {
                            AsOnStock oitem = new AsOnStock();
                            oitem.productname = name;
                            oitem.saleqty = reader["qty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["qty"]);
                            oitem.salevalue = reader["stockvalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["stockvalue"]);
                            oitem.isalevalue = reader["istockvalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["istockvalue"]);
                            oitem.salesalevalue = reader["salevalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["salevalue"]);
                            lst.Add(oitem);
                        }
                    }
                }

                command = GetCommand(string.Format(SQL_autidStock, string.Join(",", field1), string.Join(",", field2), _condition), conn);
                command.Parameters.Add("supplierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _reportcreteria.supplierid;
                command.Parameters.Add("sectionid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _reportcreteria.sectionid;
                command.Parameters.Add("productid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _reportcreteria.productid;
                command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _reportcreteria.companyid;
                command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _reportcreteria.locationid;
                command.Parameters.Add("fromdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = today;
                command.Parameters.Add("fromenddate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = today.AddDays(1);
                command.Parameters.Add("todate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = nextdate;
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string name = reader["column1"] == DBNull.Value ? "" : reader["column1"].ToString();
                        if (lst.Exists(e => e.productname.ToLower() == name.ToLower()))
                        {
                            AsOnStock oitem = lst.Find(e => e.productname.ToLower() == name.ToLower());
                            oitem.auditqty = reader["qty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["qty"]);
                            oitem.auditvalue = reader["stockvalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["stockvalue"]);
                            oitem.iauditvalue = reader["istockvalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["istockvalue"]);
                            oitem.auditsalevalue = reader["salevalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["salevalue"]);
                        }
                        else
                        {
                            AsOnStock oitem = new AsOnStock();
                            oitem.productname = name;
                            oitem.auditqty = reader["qty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["qty"]);
                            oitem.auditvalue = reader["stockvalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["stockvalue"]);
                            oitem.iauditvalue = reader["istockvalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["istockvalue"]);
                            oitem.auditsalevalue = reader["salevalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["salevalue"]);
                            lst.Add(oitem);
                        }
                    }
                }

                if (_reportcreteria.locationid <= 0)
                {
                    command = GetCommand(string.Format(SQL_autidwarehouse, string.Join(",", field1), string.Join(",", field2), _condition), conn);
                    command.Parameters.Add("supplierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _reportcreteria.supplierid;
                    command.Parameters.Add("sectionid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _reportcreteria.sectionid;
                    command.Parameters.Add("productid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _reportcreteria.productid;
                    command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _reportcreteria.companyid;
                    command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _reportcreteria.locationid;
                    command.Parameters.Add("fromdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = today;
                    command.Parameters.Add("fromenddate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = today.AddDays(1);
                    command.Parameters.Add("todate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = nextdate;
                    using (System.Data.IDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string name = reader["column1"] == DBNull.Value ? "" : reader["column1"].ToString();
                            if (lst.Exists(e => e.productname.ToLower() == name.ToLower()))
                            {
                                AsOnStock oitem = lst.Find(e => e.productname.ToLower() == name.ToLower());
                                oitem.auditqty += reader["qty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["qty"]);
                                oitem.auditvalue += reader["stockvalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["stockvalue"]);
                                oitem.iauditvalue += reader["istockvalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["istockvalue"]);
                                oitem.auditsalevalue = reader["salevalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["salevalue"]);
                            }
                            else
                            {
                                AsOnStock oitem = new AsOnStock();
                                oitem.productname = name;
                                oitem.auditqty += reader["qty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["qty"]);
                                oitem.auditvalue += reader["stockvalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["stockvalue"]);
                                oitem.iauditvalue += reader["istockvalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["istockvalue"]);
                                oitem.auditsalevalue = reader["salevalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["salevalue"]);
                                lst.Add(oitem);
                            }
                        }
                    }
                }

                foreach (AsOnStock item in lst)
                {
                    item.asstockqty = item.asstockqty + item.saleqty + item.purchasereturnqty;
                    item.asstockvalue = item.asstockvalue + item.salevalue + item.purchasereturnvalue;
                    item.iasstockvalue = item.iasstockvalue + item.isalevalue + item.ipurchasereturnvalue;
                    item.assalevalue = item.assalevalue + item.salesalevalue + item.purchaseretsalevalue;
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
        private List<StockTransaction> SearchOpeningStock(StockReportCreteria _reportcreteria)
        {
            string query = @"with currentstock as 
(
	with currentallstock as (
    select stock.productid, products.salescounterid,items.brandid,items.styleid, sum(salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty+salablegoods.aqty)) qty,
    sum((salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty+salablegoods.aqty)) * stock.ibuyingprice) as buyingcost,
    sum((salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty+salablegoods.aqty)) * stock.buyingprice) as finstockcost,
    sum((salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty+salablegoods.aqty)) * (salablegoods.price-salablegoods.discount)) as sellingprice
    from salablegoods 
    inner join stock on stock.id = salablegoods.stockid and stock.isactive = true
    inner join items on items.id = stock.itemid
    left outer join products on products.id = stock.productid
    where salablegoods.isactive = true  {1}
    and salablegoods.hasstock = true and salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty+salablegoods.aqty)>0 
    group by stock.productid, products.salescounterid,items.brandid,items.styleid
    UNION ALL 
    select stock.productid, products.salescounterid,items.brandid,items.styleid,
    sum(stock.qty - (stock.transfered+stock.returnqty+stock.damagedqty+stock.journalqty)) qty,
    sum((stock.qty - (stock.transfered+stock.returnqty+stock.damagedqty+stock.journalqty)) * stock.ibuyingprice::numeric) as buyingcost,
    sum((stock.qty - (stock.transfered+stock.returnqty+stock.damagedqty+stock.journalqty)) * stock.buyingprice::numeric) as finstockcost,
    sum((stock.qty - (stock.transfered+stock.returnqty+stock.damagedqty+stock.journalqty)) * (stock.price-stock.discount)::numeric) as sellingprice
    from stock
    inner join items on items.id = stock.itemid    
    left outer join products on products.id = stock.productid
    where stock.isactive = true  {0}
    and stock.hasstock = true and stock.qty-(stock.transfered+stock.returnqty+stock.damagedqty+stock.journalqty)>0 
    group by stock.productid, products.salescounterid,items.brandid,items.styleid
    )
    select productid, salescounterid,brandid,styleid, sum(qty) qty, sum(buyingcost) buyingcost,sum(finstockcost) finstockcost, sum(sellingprice) sellingprice from currentallstock 
    group by productid, salescounterid,brandid,styleid
), purchase as
(
    with purchaseall as 
    (
    select lrinvoiceitems.productid, products.salescounterid,items.brandid,items.styleid, sum(lrinvoiceitems.buyingqty) qty, 
    sum((lrinvoiceitems.sellableqty * lrinvoiceitems.buyingprice::numeric)+
    (case when lrinvoiceitems.purchasetax= 0 then 0 else lrinvoiceitems.purchasetax/lrinvoiceitems.buyingqty*lrinvoiceitems.sellableqty end)) as purchasevalue,
    sum(lrinvoiceitems.sellableqty * lrinvoiceitems.buyingprice::numeric) as finstockcost,
    sum(lrinvoiceitems.sellableqty * (lrinvoiceitems.sellingprice-lrinvoiceitems.salediscount)) as salevalue
    from lrentry 
	inner join lrinvoice on lrentry.id=lrinvoice.lrid and lrinvoice.isactive=true
    inner join lrinvoiceitems on lrinvoiceitems.lrinvoiceid = lrinvoice.id and lrinvoiceitems.isactive = true 
    inner join items on items.id = lrinvoiceitems.itemid
    inner join products on products.id = lrinvoiceitems.productid
    Where lrinvoice.entrydate>=@fromdate {4}
    group by lrinvoiceitems.productid, products.salescounterid,items.brandid,items.styleid

    UNION ALL
    select stock.productid, products.salescounterid,items.brandid,items.styleid, sum(stock.qty) qty, 
    sum(stock.qty * stock.ibuyingprice::numeric) as purchasevalue,
    sum(stock.qty * stock.buyingprice::numeric) as finstockcost,
    sum(stock.qty * (stock.price-stock.discount)) as salevalue
    from stock 
    inner join items on items.id = stock.itemid    
    inner join supplier on supplier.id = stock.supplierid
    left outer join products on products.id = stock.productid
    Where stock.isactive=true  and stock.invoiceitemid=-1
    and stock.stockdate>=@fromdate {0}
    group by stock.productid, products.salescounterid,items.brandid,items.styleid
    )
    select productid, salescounterid,brandid,styleid, sum(qty) qty,sum(finstockcost) finstockcost, sum(purchasevalue) purchasevalue, sum(salevalue) salevalue from purchaseall
    group by productid, salescounterid,brandid,styleid
), purchasereturn as
(
select lrinvoicereturnitems.productid, products.salescounterid,items.brandid,items.styleid, sum(lrinvoicereturnitems.qty) returnqty,
sum((lrinvoicereturnitems.qty * lrinvoicereturnitems.rate) + lrinvoicereturnitems.tax) as returnvalue,
sum((lrinvoicereturnitems.qty * lrinvoicereturnitems.rate)) as finstockcost,
sum(lrinvoicereturnitems.qty * (stock.price-stock.discount)) as returnsalvalue
from lrinvoicereturn
inner join lrinvoicereturnitems on lrinvoicereturnid = lrinvoicereturn.id
inner join stock on stock.id = lrinvoicereturnitems.stockid
inner join items on items.id = stock.itemid    
left outer join products on products.id = lrinvoicereturnitems.productid
where lrinvoicereturn.isactive = true and lrinvoicereturnitems.isactive = true {3}
and lrinvoicereturn.returntype = 0 and lrinvoicereturn.returndate>=@fromdate 
group by lrinvoicereturnitems.productid, products.salescounterid,items.brandid,items.styleid
), salevalue as
(
select stock.productid, products.salescounterid,items.brandid,items.styleid, sum(billitems.qty) saleqty,
sum(billitems.qty * stock.ibuyingprice::numeric) as salablevalue, 
sum(billitems.qty * stock.buyingprice::numeric) as finstockcost, 
sum(billitems.receivable) as salevalue
from bill 
inner join billitems on billitems.billid = bill.id and billitems.isactive=true
left outer join salablegoods on salablegoods.id = salablegoodsid 
left outer join stock on stock.id = salablegoods.stockid
left outer join items on items.id = stock.itemid    
left outer join products on products.id = stock.productid
where bill.isactive = true and bill.billtype != 9 and bill.billdate>=@fromdate {2}
group by stock.productid, products.salescounterid,items.brandid,items.styleid
),
jobworkoutward as
(
select products.id productid,salescounterid,items.brandid,items.styleid,sum(jobworkbillitems.iqty) jobworkoutward from jobworkbill 
inner join jobworkbillitems on jobworkbillitems.jobworkbillid = jobworkbill.id
inner join stock on stock.id = jobworkbillitems.stockid
inner join items on items.id = stock.itemid    
inner join products on products.id = stock.productid
where entrydate>=@fromdate
and entrydate<@todate 
group by products.id,salescounterid,items.brandid,items.styleid
),
jobworkinward as
(
select products.id productid,salescounterid,items.brandid,items.styleid,sum(Jobworkbillgrnitems.qty) jobworkinward from Jobworkbillgrn 
inner join Jobworkbillgrnitems on Jobworkbillgrnitems.Jobworkbillgrnid = Jobworkbillgrn.id
inner join stock on stock.id = Jobworkbillgrnitems.stockid
inner join items on items.id = stock.itemid    
inner join products on products.id = stock.productid
where entrydate>=@fromdate
and entrydate<@todate 
group by products.id,salescounterid,items.brandid,items.styleid
), 
audit as
(
    with auditstockall as
    (
    select stock.productid,items.brandid,items.styleid, sum(salablegoodshistory.auditqty) auditqty,
    sum( salablegoodshistory.auditqty * stock.ibuyingprice::numeric) as auditcost,
    sum( salablegoodshistory.auditqty * stock.buyingprice::numeric) as finstockcost,
    sum( salablegoodshistory.auditqty * (salablegoods.price-salablegoods.discount)) as auditsalevalue
    from salablegoodshistory inner join salablegoods on salablegoods.id = salablegoodshistory.salablegoodsid
    inner join stock on stock.id = salablegoods.stockid
    inner join items on items.id = stock.itemid    
    where salablegoods.isactive=true and stock.isactive=true {0}
    and salablegoodshistory.modifiedon>=@fromdate 
    group by stock.productid,items.brandid,items.styleid
    UNION ALL
    select stock.productid,items.brandid,items.styleid, sum(stockhistory.auditqty) auditqty,
    sum( stockhistory.auditqty * stock.ibuyingprice::numeric) as auditcost,
    sum( stockhistory.auditqty * stock.ibuyingprice::numeric) as finstockcost,
    sum( stockhistory.auditqty * (stock.price-stock.discount)) as auditsalevalue
    from stockhistory 
    inner join stock on stock.id = stockhistory.stockid
    inner join items on items.id = stock.itemid    
    where stock.isactive=true 
    and stockhistory.modifiedon>=@fromdate {0}
    group by stock.productid,items.brandid,items.styleid
    )
    select productid,brandid,styleid, sum(auditqty) auditqty, sum(auditcost) auditcost, sum(auditsalevalue) auditsalevalue from auditstockall
    group by productid,brandid,styleid
)
select currentstock.productid, currentstock.salescounterid,currentstock.brandid,currentstock.styleid, sum(currentstock.qty) qty, sum(currentstock.buyingcost) buyingcost, sum(currentstock.sellingprice) sellingprice,
sum(purchase.qty) purchaseqty, sum(purchase.purchasevalue) purchasevalue, sum(purchase.salevalue) purchasesalevalue,
sum(purchasereturn.returnqty) returnqty, sum(purchasereturn.returnvalue) returnvalue, sum(purchasereturn.returnsalvalue) returnsalvalue,
sum(salevalue.saleqty) saleqty, sum(salevalue.salablevalue) salablevalue, sum(salevalue.salevalue) salevalue,
sum(audit.auditqty) auditqty, sum(audit.auditcost) auditcost, sum(audit.auditsalevalue) auditsalevalue,
sum(jobworkoutward.jobworkoutward) jobworkoutward,sum(jobworkinward.jobworkinward) jobworkinward,
sum(currentstock.finstockcost) currentstockfinstockcost,sum(purchase.finstockcost) purchasefinstockcost,
sum(salevalue.finstockcost) salevaluefinstockcost,sum(purchasereturn.finstockcost) purchasereturnfinstockcost
from currentstock 
left outer join purchase on purchase.productid = currentstock.productid and purchase.brandid = currentstock.brandid and purchase.styleid = currentstock.styleid
left outer join purchasereturn on purchasereturn.productid = currentstock.productid and purchasereturn.brandid = currentstock.brandid and purchasereturn.styleid = currentstock.styleid
left outer join salevalue on salevalue.productid = currentstock.productid and salevalue.brandid = currentstock.brandid and salevalue.styleid = currentstock.styleid
left outer join audit on audit.productid = currentstock.productid and audit.brandid = currentstock.brandid and audit.styleid = currentstock.styleid
left outer join jobworkinward on jobworkinward.productid = currentstock.productid and jobworkinward.brandid = currentstock.brandid and jobworkinward.styleid = currentstock.styleid
left outer join jobworkoutward on jobworkoutward.productid = currentstock.productid and jobworkoutward.brandid = currentstock.brandid and jobworkoutward.styleid = currentstock.styleid
group by currentstock.productid, currentstock.salescounterid,currentstock.brandid,currentstock.styleid
";

            if (!_reportcreteria.brand && !_reportcreteria.style)
            {
                query = query.Replace("inner join items on items.id = stock.itemid", "")
                    .Replace("inner join items on items.id = lrinvoiceitems.itemid", "");
            }

            if (!_reportcreteria.brand)
            {
                query = query.Replace("and purchase.brandid = currentstock.brandid", "")
                    .Replace("and purchasereturn.brandid = currentstock.brandid", "")
                    .Replace("and salevalue.brandid = currentstock.brandid", "")
                    .Replace("and audit.brandid = currentstock.brandid", "")
                    .Replace("and jobworkinward.brandid = currentstock.brandid", "")
                    .Replace("and jobworkoutward.brandid = currentstock.brandid", "")
                    .Replace(",items.brandid", "").Replace(",currentstock.brandid", "")
                    .Replace(",brandid", "");
            }

            if (!_reportcreteria.style)
            {
                query = query.Replace("and purchase.styleid = currentstock.styleid", "")
                    .Replace("and purchasereturn.styleid = currentstock.styleid", "")
                    .Replace("and salevalue.styleid = currentstock.styleid", "")
                    .Replace("and audit.styleid = currentstock.styleid", "")
                    .Replace("and jobworkinward.styleid = currentstock.styleid", "")
                    .Replace("and jobworkoutward.styleid = currentstock.styleid", "")
                    .Replace(",items.styleid", "").Replace(",currentstock.styleid", "")
                    .Replace(",styleid", "");
            }

            if(!_reportcreteria.product && !_reportcreteria.section && 
                (_reportcreteria.brand || _reportcreteria.style))
            {
                query = query.Replace(@"currentstock.productid, currentstock.salescounterid,", "");
            }
            else
            {
                if (_reportcreteria.section && !_reportcreteria.product)
                {
                    query = query.Replace(@"currentstock.productid,", "");
                }
                else
                {
                    _reportcreteria.product = true;
                    _reportcreteria.section = true;
                }
            }

            List<StockTransaction> lst = new List<StockTransaction>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                NpgsqlCommand command = null;
                DateTime fromdate = _reportcreteria.fromdate.Date;
                DateTime todate = _reportcreteria.todate.Date.AddDays(1);

                if (_reportcreteria.companyid > 0)
                    query = string.Format(query, "and stock.companyid=@companyid ",
                        " and salablegoods.companyid=@companyid ",
                        " and bill.companyid=@companyid",
                        " and lrinvoicereturn.companyid=@companyid",
                        " and lrinvoice.companyid=@companyid");
                else
                    query = string.Format(query, "", "", "", "", "");

                if (!Services.ConfigManager.Instance(Context.OrganizationID).Config.analyser_stock_removeaqty)
                    query = query.Replace("salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty+salablegoods.aqty)", "salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty)");

                if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.analyser_stock_withzerostock)
                {
                    query = query.Replace("and salablegoods.hasstock = true and salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty+salablegoods.aqty)>0", "");
                    query = query.Replace("and salablegoods.hasstock = true and salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty)>0", "");
                    query = query.Replace("and stock.hasstock = true and stock.qty-(stock.transfered+stock.returnqty+stock.damagedqty+stock.journalqty)>0", "");
                }

                command = GetCommand(query, conn);
                command.Parameters.Add("productid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _reportcreteria.productid;
                command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _reportcreteria.companyid;
                command.Parameters.Add("fromdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = fromdate;
                command.Parameters.Add("todate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = todate;
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        StockTransaction oitem = new StockTransaction();
                        if (_reportcreteria.product)
                            oitem.productid = reader["productid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productid"]);
                        if(_reportcreteria.section)
                            oitem.sectionid = reader["salescounterid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["salescounterid"]);

                        if (_reportcreteria.brand)
                            oitem.brandid = reader["brandid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["brandid"]);
                        if (_reportcreteria.style)
                            oitem.styleid = reader["styleid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["styleid"]);

                        oitem.currentqty = reader["qty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["qty"]);
                        oitem.currentstockvalue = reader["buyingcost"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["buyingcost"]);
                        oitem.currentsalevalue = reader["sellingprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["sellingprice"]);

                        oitem.purchaseqty = reader["purchaseqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["purchaseqty"]);
                        oitem.purchasestockvalue = reader["purchasevalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["purchasevalue"]);
                        oitem.purchasesalevalue = reader["purchasesalevalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["purchasesalevalue"]);

                        oitem.purchaseretqty = reader["returnqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["returnqty"]);
                        oitem.purchaseretstockvalue = reader["returnvalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["returnvalue"]);
                        oitem.purchaseretsalevalue = reader["returnsalvalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["returnsalvalue"]);

                        oitem.saleqty = reader["saleqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["saleqty"]);
                        oitem.salestockvalue = reader["salablevalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["salablevalue"]);
                        oitem.salesalevalue = reader["salevalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["salevalue"]);

                        oitem.jobworkoutward = reader["jobworkoutward"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["jobworkoutward"]);
                        oitem.jobworkinward = reader["jobworkinward"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["jobworkinward"]);

                        oitem.auditqty = reader["auditqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["auditqty"]);
                        oitem.auditstockvalue = reader["auditcost"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["auditcost"]);
                        oitem.auditsalevalue = reader["auditsalevalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["auditsalevalue"]);

                        oitem.currentstockfinstockcost = reader["currentstockfinstockcost"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["currentstockfinstockcost"]);
                        oitem.purchasefinstockcost = reader["purchasefinstockcost"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["purchasefinstockcost"]);
                        oitem.salevaluefinstockcost = reader["salevaluefinstockcost"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["salevaluefinstockcost"]);
                        oitem.purchasereturnfinstockcost = reader["purchasereturnfinstockcost"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["purchasereturnfinstockcost"]);
                        
                        lst.Add(oitem);
                    }
                }
                conn.Close();


                foreach (StockTransaction item in lst)
                {
                    item.openingqty = item.currentqty + item.saleqty + item.purchaseretqty - item.purchaseqty;
                    item.openingstockvalue = item.currentstockvalue + item.salestockvalue + item.purchaseretstockvalue - item.purchasestockvalue;
                    item.openingsalevalue = item.currentsalevalue + item.salesalevalue + item.purchaseretsalevalue - item.purchasesalevalue;
                }
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _reportcreteria);
            }
            return lst;
        }


        public List<StockTransaction> SearchAsOnStockOLD(StockReportCreteria _reportcreteria)
        {
            List<StockTransaction> openingstock = SearchOpeningStock(_reportcreteria);

            string query = @"with currentstock as 
(
	with currentallstock as (
    select stock.productid, products.salescounterid, sum(salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty+salablegoods.aqty)) qty,
    sum((salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty+salablegoods.aqty)) * stock.ibuyingprice) as buyingcost,
    sum((salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty+salablegoods.aqty)) * (salablegoods.price-salablegoods.discount)) as sellingprice
    from salablegoods 
    inner join stock on stock.id = salablegoods.stockid and stock.isactive = true
    left outer join products on products.id = stock.productid
    where salablegoods.isactive = true  {1}
    and salablegoods.hasstock = true and salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty+salablegoods.aqty)>0 
    group by stock.productid, products.salescounterid
    UNION ALL 
    select stock.productid, products.salescounterid,
    sum(stock.qty - (stock.transfered+stock.returnqty+stock.damagedqty+stock.journalqty)) qty,
    sum((stock.qty - (stock.transfered+stock.returnqty+stock.damagedqty+stock.journalqty)) * stock.ibuyingprice::numeric) as buyingcost,
    sum((stock.qty - (stock.transfered+stock.returnqty+stock.damagedqty+stock.journalqty)) * (stock.price-stock.discount)::numeric) as sellingprice
    from stock
    left outer join products on products.id = stock.productid
    where stock.isactive = true  {0}
    and stock.hasstock = true and stock.qty-(stock.transfered+stock.returnqty+stock.damagedqty+stock.journalqty)>0 
    group by stock.productid, products.salescounterid
    )
    select productid, salescounterid, sum(qty) qty, sum(buyingcost) buyingcost, sum(sellingprice) sellingprice from currentallstock 
    group by productid, salescounterid
), purchase as
(
    with purchaseall as 
    (
    select stock.productid, products.salescounterid, sum(stock.qty) qty, 
    sum(stock.qty * stock.ibuyingprice::numeric) as purchasevalue,
    sum(stock.qty * (stock.price-stock.discount)) as salevalue
    from lrentry inner join lrinvoice on lrentry.id=lrinvoice.lrid and lrinvoice.isactive=true
    inner join lrinvoiceitems on lrinvoiceitems.lrinvoiceid = lrinvoice.id and lrinvoiceitems.isactive = true 
    inner join stock on stock.invoiceitemid = lrinvoiceitems.id and stock.isactive = true
    inner join products on products.id = stock.productid
    Where stock.isactive=true     {0}
    and stock.stockdate>=@fromdate and stock.stockdate<@todate 
    group by stock.productid, products.salescounterid
    UNION ALL
    select stock.productid, products.salescounterid, sum(stock.qty) qty, 
    sum(stock.qty * stock.ibuyingprice::numeric) as purchasevalue,
    sum(stock.qty * (stock.price-stock.discount)) as salevalue
    from stock inner join supplier on supplier.id = stock.supplierid
    left outer join products on products.id = stock.productid
    Where stock.isactive=true  and stock.invoiceitemid=-1 {0}
    and stock.stockdate>=@fromdate and stock.stockdate<@todate 
    group by stock.productid, products.salescounterid
    )
    select productid, salescounterid, sum(qty) qty, sum(purchasevalue) purchasevalue, sum(salevalue) salevalue from purchaseall
    group by productid, salescounterid
), purchasereturn as
(
select lrinvoicereturnitems.productid, products.salescounterid, sum(lrinvoicereturnitems.qty) returnqty,
sum((lrinvoicereturnitems.qty * lrinvoicereturnitems.rate) + lrinvoicereturnitems.tax) as returnvalue,
sum(lrinvoicereturnitems.qty * (stock.price-stock.discount)) as returnsalvalue
from lrinvoicereturn
inner join lrinvoicereturnitems on lrinvoicereturnid = lrinvoicereturn.id
left outer join stock on stock.id = lrinvoicereturnitems.stockid
left outer join products on products.id = stock.productid
where lrinvoicereturn.isactive = true and lrinvoicereturnitems.isactive = true {0}
and lrinvoicereturn.returntype = 0 and lrinvoicereturn.returndate>=@fromdate and lrinvoicereturn.returndate<@todate
group by lrinvoicereturnitems.productid, products.salescounterid
), salevalue as
(
select stock.productid, products.salescounterid, sum(billitems.qty) saleqty,
sum(billitems.qty * stock.ibuyingprice::numeric) as salablevalue, 
sum(billitems.receivable) as salevalue
from bill 
inner join billitems on billitems.billid = bill.id and billitems.isactive=true
left outer join salablegoods on salablegoods.id = salablegoodsid 
left outer join stock on stock.id = salablegoods.stockid
left outer join products on products.id = stock.productid
where bill.isactive = true and bill.billtype != 9 and bill.billdate>=@fromdate and bill.billdate<@todate {2}
group by stock.productid, products.salescounterid
), 
jobworkoutward as
(
select products.id productid,salescounterid,sum(jobworkbillitems.iqty) jobworkoutward from jobworkbill 
inner join jobworkbillitems on jobworkbillitems.jobworkbillid = jobworkbill.id
inner join stock on stock.id = jobworkbillitems.stockid
inner join products on products.id = stock.productid
where entrydate>=@fromdate
and entrydate<@todate 
group by products.id,salescounterid
),
jobworkinward as
(
select products.id productid,salescounterid,sum(Jobworkbillgrnitems.qty) jobworkinward from Jobworkbillgrn 
inner join Jobworkbillgrnitems on Jobworkbillgrnitems.Jobworkbillgrnid = Jobworkbillgrn.id
inner join stock on stock.id = Jobworkbillgrnitems.stockid
inner join products on products.id = stock.productid
where entrydate>=@fromdate
and entrydate<@todate 
group by products.id,salescounterid
), 
audit as
(
    with auditstockall as
    (
    select stock.productid, sum(salablegoodshistory.auditqty) auditqty,
    sum( salablegoodshistory.auditqty * stock.ibuyingprice::numeric) as auditcost,
    sum( salablegoodshistory.auditqty * (salablegoods.price-salablegoods.discount)) as auditsalevalue
    from salablegoodshistory inner join salablegoods on salablegoods.id = salablegoodshistory.salablegoodsid
    inner join stock on stock.id = salablegoods.stockid
    where salablegoods.isactive=true and stock.isactive=true {1}
    and salablegoodshistory.modifiedon>=@fromdate and salablegoodshistory.modifiedon<@todate  
    group by stock.productid
    UNION ALL
    select stock.productid, sum(stockhistory.auditqty) auditqty,
    sum( stockhistory.auditqty * stock.ibuyingprice::numeric) as auditcost,
    sum( stockhistory.auditqty * (stock.price-stock.discount)) as auditsalevalue
    from stockhistory inner join stock on stock.id = stockhistory.stockid
    where stock.isactive=true {0}
    and stockhistory.modifiedon>=@fromdate and stockhistory.modifiedon<@todate
    group by stock.productid
    )
    select productid, sum(auditqty) auditqty, sum(auditcost) auditcost, sum(auditsalevalue) auditsalevalue from auditstockall
    group by productid
)
select currentstock.productid, currentstock.salescounterid, sum(currentstock.qty) qty, sum(currentstock.buyingcost) buyingcost, sum(currentstock.sellingprice) sellingprice,
sum(purchase.qty) purchaseqty, sum(purchase.purchasevalue) purchasevalue, sum(purchase.salevalue) purchasesalevalue,
sum(purchasereturn.returnqty) returnqty, sum(purchasereturn.returnvalue) returnvalue, sum(purchasereturn.returnsalvalue) returnsalvalue,
sum(salevalue.saleqty) saleqty, sum(salevalue.salablevalue) salablevalue, sum(salevalue.salevalue) salevalue,
sum(audit.auditqty) auditqty, sum(audit.auditcost) auditcost, sum(audit.auditsalevalue) auditsalevalue,
sum(jobworkoutward.jobworkoutward) jobworkoutward,sum(jobworkinward.jobworkinward) jobworkinward
from currentstock left outer join purchase on purchase.productid = currentstock.productid
left outer join purchasereturn on purchasereturn.productid = currentstock.productid
left outer join salevalue on salevalue.productid = currentstock.productid
left outer join audit on audit.productid = currentstock.productid
left outer join jobworkinward on jobworkinward.productid = currentstock.productid
left outer join jobworkoutward on jobworkoutward.productid = currentstock.productid
group by currentstock.productid, currentstock.salescounterid
";

            List<StockTransaction> lst = new List<StockTransaction>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                NpgsqlCommand command = null;
                DateTime fromdate = _reportcreteria.fromdate.Date;
                DateTime todate = _reportcreteria.todate.Date.AddDays(1);

                if (_reportcreteria.companyid > 0)
                    query = string.Format(query, "and stock.companyid=@companyid ", " and salablegoods.companyid=@companyid ", " and bill.companyid=@companyid");
                else
                    query = string.Format(query, "", "", "");

                if (!Services.ConfigManager.Instance(Context.OrganizationID).Config.analyser_stock_removeaqty)
                    query = query.Replace("salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty+salablegoods.aqty)", "salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty)");

                if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.analyser_stock_withzerostock)
                {
                    query = query.Replace("and salablegoods.hasstock = true and salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty+salablegoods.aqty)>0", "");
                    query = query.Replace("and salablegoods.hasstock = true and salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty)>0", "");
                    query = query.Replace("and stock.hasstock = true and stock.qty-(stock.transfered+stock.returnqty+stock.damagedqty+stock.journalqty)>0", "");
                }

                command = GetCommand(query, conn);
                command.Parameters.Add("productid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _reportcreteria.productid;
                command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _reportcreteria.companyid;
                command.Parameters.Add("fromdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = fromdate;
                command.Parameters.Add("todate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = todate;
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        StockTransaction oitem = new StockTransaction();
                        oitem.productid = reader["productid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productid"]);
                        oitem.sectionid = reader["salescounterid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["salescounterid"]);

                        oitem.currentqty = reader["qty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["qty"]);
                        oitem.currentstockvalue = reader["buyingcost"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["buyingcost"]);
                        oitem.currentsalevalue = reader["sellingprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["sellingprice"]);

                        oitem.purchaseqty = reader["purchaseqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["purchaseqty"]);
                        oitem.purchasestockvalue = reader["purchasevalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["purchasevalue"]);
                        oitem.purchasesalevalue = reader["purchasesalevalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["purchasesalevalue"]);

                        oitem.purchaseretqty = reader["returnqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["returnqty"]);
                        oitem.purchaseretstockvalue = reader["returnvalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["returnvalue"]);
                        oitem.purchaseretsalevalue = reader["returnsalvalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["returnsalvalue"]);

                        oitem.saleqty = reader["saleqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["saleqty"]);
                        oitem.salestockvalue = reader["salablevalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["salablevalue"]);
                        oitem.salesalevalue = reader["salevalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["salevalue"]);

                        oitem.jobworkoutward = reader["jobworkoutward"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["jobworkoutward"]);
                        oitem.jobworkinward = reader["jobworkinward"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["jobworkinward"]);

                        oitem.auditqty = reader["auditqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["auditqty"]);
                        oitem.auditstockvalue = reader["auditcost"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["auditcost"]);
                        oitem.auditsalevalue = reader["auditsalevalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["auditsalevalue"]);

                        lst.Add(oitem);
                    }
                }
                conn.Close();

                foreach (StockTransaction item in lst)
                {
                    //item.openingqty = item.currentqty + item.saleqty + item.purchaseretqty - item.purchaseqty;
                    //item.openingstockvalue = item.currentstockvalue + item.salestockvalue + item.purchaseretstockvalue - item.purchasestockvalue;
                    //item.openingsalevalue = item.currentsalevalue + item.salesalevalue + item.purchaseretsalevalue - item.purchasesalevalue;

                    if (openingstock.Exists(e => e.productid == item.productid && e.sectionid == item.sectionid))
                    {
                        var openingitem = openingstock.Find(e => e.productid == item.productid && e.sectionid == item.sectionid);

                        item.openingqty = openingitem.openingqty;
                        item.openingstockvalue = openingitem.openingstockvalue;
                        item.openingsalevalue = openingitem.openingsalevalue;
                    }
                    item.closingqty = item.openingqty + item.purchaseqty - item.purchaseretqty - item.saleqty;
                    item.closingstockvalue = item.openingstockvalue + item.purchasestockvalue - item.purchaseretstockvalue - item.salestockvalue;
                    item.closingsalevalue = item.openingsalevalue + item.purchasesalevalue - item.purchaseretsalevalue - item.salesalevalue;
                }
                lst.RemoveAll(e => e.openingqty == 0 && e.closingqty == 0 && e.currentqty == 0);
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _reportcreteria);
            }
            return lst;
        }


        public List<StockTransaction> SearchAsOnStock(StockReportCreteria _reportcreteria)
        {
            List<StockTransaction> openingstock = SearchOpeningStock(_reportcreteria);

            string query = @"with currentstock as 
(
	with currentallstock as (
    select stock.productid, products.salescounterid,items.brandid,items.styleid, sum(salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty+salablegoods.aqty)) qty,
    sum((salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty+salablegoods.aqty)) * stock.ibuyingprice) as buyingcost,
    sum((salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty+salablegoods.aqty)) * stock.buyingprice) as finstockcost,
    sum((salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty+salablegoods.aqty)) * (salablegoods.price-salablegoods.discount)) as sellingprice
    from salablegoods 
    inner join stock on stock.id = salablegoods.stockid and stock.isactive = true
    inner join items on items.id = stock.itemid    
    left outer join products on products.id = stock.productid
    where salablegoods.isactive = true  {1}
    and salablegoods.hasstock = true and salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty+salablegoods.aqty)>0 
    group by stock.productid, products.salescounterid,items.brandid,items.styleid
    UNION ALL 
    select stock.productid, products.salescounterid,items.brandid,items.styleid,
    sum(stock.qty - (stock.transfered+stock.returnqty+stock.damagedqty+stock.journalqty)) qty,
    sum((stock.qty - (stock.transfered+stock.returnqty+stock.damagedqty+stock.journalqty)) * stock.ibuyingprice::numeric) as buyingcost,
    sum((stock.qty - (stock.transfered+stock.returnqty+stock.damagedqty+stock.journalqty)) * stock.buyingprice::numeric) as finstockcost,
    sum((stock.qty - (stock.transfered+stock.returnqty+stock.damagedqty+stock.journalqty)) * (stock.price-stock.discount)::numeric) as sellingprice
    from stock
    inner join items on items.id = stock.itemid    
    left outer join products on products.id = stock.productid
    where stock.isactive = true  {0}
    and stock.hasstock = true and stock.qty-(stock.transfered+stock.returnqty+stock.damagedqty+stock.journalqty)>0 
    group by stock.productid, products.salescounterid,items.brandid,items.styleid
    )
    select productid, salescounterid,brandid,styleid, sum(qty) qty, sum(buyingcost) buyingcost,sum(finstockcost) finstockcost, sum(sellingprice) sellingprice from currentallstock 
    group by productid, salescounterid,brandid,styleid
), purchase as
(
    with purchaseall as 
    (

    select lrinvoiceitems.productid, products.salescounterid,items.brandid,items.styleid, sum(lrinvoiceitems.sellableqty) qty, 
    sum((lrinvoiceitems.sellableqty * lrinvoiceitems.buyingprice::numeric)+
    (case when lrinvoiceitems.purchasetax= 0 then 0 else lrinvoiceitems.purchasetax/lrinvoiceitems.buyingqty*lrinvoiceitems.sellableqty end)) as purchasevalue,
    sum(lrinvoiceitems.sellableqty * lrinvoiceitems.buyingprice::numeric) as finstockcost,
    sum(lrinvoiceitems.sellableqty * (lrinvoiceitems.sellingprice-lrinvoiceitems.salediscount)) as salevalue
    from lrentry 
	inner join lrinvoice on lrentry.id=lrinvoice.lrid and lrinvoice.isactive=true
    inner join lrinvoiceitems on lrinvoiceitems.lrinvoiceid = lrinvoice.id and lrinvoiceitems.isactive = true 
    inner join items on items.id = lrinvoiceitems.itemid    
    inner join products on products.id = lrinvoiceitems.productid
    Where lrinvoice.entrydate>=@fromdate and lrinvoice.entrydate<@todate {4}
    group by lrinvoiceitems.productid, products.salescounterid,items.brandid,items.styleid
    UNION ALL
    select stock.productid, products.salescounterid,items.brandid,items.styleid, sum(stock.qty) qty, 
    sum(stock.qty * stock.ibuyingprice::numeric) as purchasevalue,
    sum(stock.qty * stock.buyingprice::numeric) as finstockcost,
    sum(stock.qty * (stock.price-stock.discount)) as salevalue
    from stock 
    inner join items on items.id = stock.itemid    
    inner join supplier on supplier.id = stock.supplierid
    left outer join products on products.id = stock.productid
    Where stock.isactive=true  and stock.invoiceitemid=-1 {0}
    and stock.stockdate>=@fromdate and stock.stockdate<@todate 
    group by stock.productid, products.salescounterid,items.brandid,items.styleid
    )
    select productid, salescounterid,brandid,styleid, sum(qty) qty,sum(finstockcost) finstockcost, sum(purchasevalue) purchasevalue, sum(salevalue) salevalue from purchaseall
    group by productid, salescounterid,brandid,styleid
), purchasereturn as
(
select lrinvoicereturnitems.productid, products.salescounterid,items.brandid,items.styleid, sum(lrinvoicereturnitems.qty) returnqty,
sum((lrinvoicereturnitems.qty * lrinvoicereturnitems.rate) + lrinvoicereturnitems.tax) as returnvalue,
sum((lrinvoicereturnitems.qty * lrinvoicereturnitems.rate)) as finstockcost,
sum(lrinvoicereturnitems.qty * (stock.price-stock.discount)) as returnsalvalue
from lrinvoicereturn
inner join lrinvoicereturnitems on lrinvoicereturnid = lrinvoicereturn.id
inner join stock on stock.id = lrinvoicereturnitems.stockid
inner join items on items.id = stock.itemid    
left outer join products on products.id = lrinvoicereturnitems.productid
where lrinvoicereturn.isactive = true and lrinvoicereturnitems.isactive = true {3}
and lrinvoicereturn.returntype = 0 and lrinvoicereturn.returndate>=@fromdate and lrinvoicereturn.returndate<@todate
group by lrinvoicereturnitems.productid, products.salescounterid,items.brandid,items.styleid
), salevalue as
(
select stock.productid, products.salescounterid,items.brandid,items.styleid, sum(billitems.qty) saleqty,
sum(billitems.qty * stock.ibuyingprice::numeric) as salablevalue, 
sum(billitems.qty * stock.buyingprice::numeric) as finstockcost, 
sum(billitems.receivable) as salevalue
from bill 
inner join billitems on billitems.billid = bill.id and billitems.isactive=true
left outer join salablegoods on salablegoods.id = salablegoodsid 
left outer join stock on stock.id = salablegoods.stockid
left outer join items on items.id = stock.itemid    
left outer join products on products.id = stock.productid
where bill.isactive = true and bill.billtype != 9 and bill.billdate>=@fromdate and bill.billdate<@todate {2}
group by stock.productid, products.salescounterid,items.brandid,items.styleid
), 
jobworkoutward as
(
select products.id productid,salescounterid,items.brandid,items.styleid,sum(jobworkbillitems.iqty) jobworkoutward from jobworkbill 
inner join jobworkbillitems on jobworkbillitems.jobworkbillid = jobworkbill.id
inner join stock on stock.id = jobworkbillitems.stockid
inner join items on items.id = stock.itemid    
inner join products on products.id = stock.productid
where entrydate>=@fromdate
and entrydate<@todate 
group by products.id,salescounterid,items.brandid,items.styleid
),
jobworkinward as
(
select products.id productid,salescounterid,items.brandid,items.styleid,sum(Jobworkbillgrnitems.qty) jobworkinward from Jobworkbillgrn 
inner join Jobworkbillgrnitems on Jobworkbillgrnitems.Jobworkbillgrnid = Jobworkbillgrn.id
inner join stock on stock.id = Jobworkbillgrnitems.stockid
inner join products on products.id = stock.productid
inner join items on items.id = stock.itemid    
where entrydate>=@fromdate
and entrydate<@todate 
group by products.id,salescounterid,items.brandid,items.styleid
), 
audit as
(
    with auditstockall as
    (
    select stock.productid,items.brandid,items.styleid, sum(salablegoodshistory.auditqty) auditqty,
    sum( salablegoodshistory.auditqty * stock.ibuyingprice::numeric) as auditcost,
    sum( salablegoodshistory.auditqty * stock.buyingprice::numeric) as finstockcost,
    sum( salablegoodshistory.auditqty * (salablegoods.price-salablegoods.discount)) as auditsalevalue
    from salablegoodshistory inner join salablegoods on salablegoods.id = salablegoodshistory.salablegoodsid
    inner join stock on stock.id = salablegoods.stockid
    inner join items on items.id = stock.itemid    
    where salablegoods.isactive=true and stock.isactive=true {1}
    and salablegoodshistory.modifiedon>=@fromdate and salablegoodshistory.modifiedon<@todate  
    group by stock.productid,items.brandid,items.styleid
    UNION ALL
    select stock.productid,items.brandid,items.styleid, sum(stockhistory.auditqty) auditqty,
    sum( stockhistory.auditqty * stock.ibuyingprice::numeric) as auditcost,
    sum( stockhistory.auditqty * stock.buyingprice::numeric) as finstockcost,
    sum( stockhistory.auditqty * (stock.price-stock.discount)) as auditsalevalue
    from stockhistory 
    inner join stock on stock.id = stockhistory.stockid
    inner join items on items.id = stock.itemid    
    where stock.isactive=true {0}
    and stockhistory.modifiedon>=@fromdate and stockhistory.modifiedon<@todate
    group by stock.productid,items.brandid,items.styleid
    )
    select productid,brandid,styleid, sum(auditqty) auditqty, sum(auditcost) auditcost, sum(auditsalevalue) auditsalevalue from auditstockall
    group by productid,brandid,styleid
)
select currentstock.productid, currentstock.salescounterid,currentstock.brandid,currentstock.styleid, sum(currentstock.qty) qty, 
sum(currentstock.buyingcost) buyingcost, sum(currentstock.sellingprice) sellingprice,
sum(purchase.qty) purchaseqty, sum(purchase.purchasevalue) purchasevalue, sum(purchase.salevalue) purchasesalevalue,
sum(purchasereturn.returnqty) returnqty, sum(purchasereturn.returnvalue) returnvalue, sum(purchasereturn.returnsalvalue) returnsalvalue,
sum(salevalue.saleqty) saleqty, sum(salevalue.salablevalue) salablevalue, sum(salevalue.salevalue) salevalue,
sum(audit.auditqty) auditqty, sum(audit.auditcost) auditcost, sum(audit.auditsalevalue) auditsalevalue,
sum(jobworkoutward.jobworkoutward) jobworkoutward,sum(jobworkinward.jobworkinward) jobworkinward,
sum(currentstock.finstockcost) currentstockfinstockcost,sum(purchase.finstockcost) purchasefinstockcost,
sum(salevalue.finstockcost) salevaluefinstockcost,sum(purchasereturn.finstockcost) purchasereturnfinstockcost
from currentstock 
left outer join purchase on purchase.productid = currentstock.productid and purchase.brandid = currentstock.brandid and purchase.styleid = currentstock.styleid
left outer join purchasereturn on purchasereturn.productid = currentstock.productid and purchasereturn.brandid = currentstock.brandid and purchasereturn.styleid = currentstock.styleid
left outer join salevalue on salevalue.productid = currentstock.productid and salevalue.brandid = currentstock.brandid and salevalue.styleid = currentstock.styleid
left outer join audit on audit.productid = currentstock.productid and audit.brandid = currentstock.brandid and audit.styleid = currentstock.styleid
left outer join jobworkinward on jobworkinward.productid = currentstock.productid and jobworkinward.brandid = currentstock.brandid and jobworkinward.styleid = currentstock.styleid
left outer join jobworkoutward on jobworkoutward.productid = currentstock.productid and jobworkoutward.brandid = currentstock.brandid and jobworkoutward.styleid = currentstock.styleid
group by currentstock.productid, currentstock.salescounterid,currentstock.brandid,currentstock.styleid
";

            if(!_reportcreteria.brand && !_reportcreteria.style)
            {
                query = query.Replace("inner join items on items.id = stock.itemid", "")
                    .Replace("inner join items on items.id = lrinvoiceitems.itemid","");
            }
            if (!_reportcreteria.brand)
            {
                query = query.Replace("and purchase.brandid = currentstock.brandid", "")
                    .Replace("and purchasereturn.brandid = currentstock.brandid", "")
                    .Replace("and salevalue.brandid = currentstock.brandid", "")
                    .Replace("and audit.brandid = currentstock.brandid", "")
                    .Replace("and jobworkinward.brandid = currentstock.brandid", "")
                    .Replace("and jobworkoutward.brandid = currentstock.brandid", "")
                    .Replace(",currentstock.brandid", "")
                    .Replace(",items.brandid", "").Replace(",currentstock.brandid", "").Replace(",brandid", "");
            }

            if (!_reportcreteria.style)
            {
                query = query.Replace("and purchase.styleid = currentstock.styleid", "")
                    .Replace("and purchasereturn.styleid = currentstock.styleid", "")
                    .Replace("and salevalue.styleid = currentstock.styleid", "")
                    .Replace("and audit.styleid = currentstock.styleid", "")
                    .Replace("and jobworkinward.styleid = currentstock.styleid", "")
                    .Replace("and jobworkoutward.styleid = currentstock.styleid", "")
                    .Replace(",currentstock.styleid", "")
                    .Replace(",items.styleid", "").Replace(",currentstock.styleid", "").Replace(",styleid", "");
            }

            if (!_reportcreteria.product && !_reportcreteria.section &&
    (_reportcreteria.brand || _reportcreteria.style))
            {
                query = query.Replace(@"currentstock.productid, currentstock.salescounterid,", "");
            }
            else
            {
                if (_reportcreteria.section && !_reportcreteria.product)
                {
                    query = query.Replace(@"currentstock.productid,", "");
                }
                else
                {
                    _reportcreteria.product = true;
                    _reportcreteria.section = true;
                }
            }

            List<StockTransaction> lst = new List<StockTransaction>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                NpgsqlCommand command = null;
                DateTime fromdate = _reportcreteria.fromdate.Date;
                DateTime todate = _reportcreteria.todate.Date.AddDays(1);

                if (_reportcreteria.companyid > 0)
                    query = string.Format(query, "and stock.companyid=@companyid ",
                        " and salablegoods.companyid=@companyid ",
                        " and bill.companyid=@companyid",
                        " and lrinvoicereturn.companyid=@companyid",
                        " and lrinvoice.companyid=@companyid");
                else
                    query = string.Format(query, "", "", "", "", "");

                if (!Services.ConfigManager.Instance(Context.OrganizationID).Config.analyser_stock_removeaqty)
                    query = query.Replace("salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty+salablegoods.aqty)", "salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty)");

                if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.analyser_stock_withzerostock)
                {
                    query = query.Replace("and salablegoods.hasstock = true and salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty+salablegoods.aqty)>0", "");
                    query = query.Replace("and salablegoods.hasstock = true and salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty)>0", "");
                    query = query.Replace("and stock.hasstock = true and stock.qty-(stock.transfered+stock.returnqty+stock.damagedqty+stock.journalqty)>0", "");
                }

                command = GetCommand(query, conn);
                command.Parameters.Add("productid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _reportcreteria.productid;
                command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _reportcreteria.companyid;
                command.Parameters.Add("fromdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = fromdate;
                command.Parameters.Add("todate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = todate;
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        StockTransaction oitem = new StockTransaction();
                        if(_reportcreteria.product)
                            oitem.productid = reader["productid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productid"]);
                        if(_reportcreteria.section)
                            oitem.sectionid = reader["salescounterid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["salescounterid"]);
                        if (_reportcreteria.brand)
                            oitem.brandid = reader["brandid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["brandid"]);
                        if (_reportcreteria.style)
                            oitem.styleid = reader["styleid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["styleid"]);
                        oitem.currentqty = reader["qty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["qty"]);
                        oitem.currentstockvalue = reader["buyingcost"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["buyingcost"]);
                        oitem.currentsalevalue = reader["sellingprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["sellingprice"]);

                        oitem.purchaseqty = reader["purchaseqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["purchaseqty"]);
                        oitem.purchasestockvalue = reader["purchasevalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["purchasevalue"]);
                        oitem.purchasesalevalue = reader["purchasesalevalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["purchasesalevalue"]);

                        oitem.purchaseretqty = reader["returnqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["returnqty"]);
                        oitem.purchaseretstockvalue = reader["returnvalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["returnvalue"]);
                        oitem.purchaseretsalevalue = reader["returnsalvalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["returnsalvalue"]);

                        oitem.saleqty = reader["saleqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["saleqty"]);
                        oitem.salestockvalue = reader["salablevalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["salablevalue"]);
                        oitem.salesalevalue = reader["salevalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["salevalue"]);

                        oitem.jobworkoutward = reader["jobworkoutward"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["jobworkoutward"]);
                        oitem.jobworkinward = reader["jobworkinward"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["jobworkinward"]);

                        oitem.auditqty = reader["auditqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["auditqty"]);
                        oitem.auditstockvalue = reader["auditcost"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["auditcost"]);
                        oitem.auditsalevalue = reader["auditsalevalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["auditsalevalue"]);

                        oitem.currentstockfinstockcost = reader["currentstockfinstockcost"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["currentstockfinstockcost"]);
                        oitem.purchasefinstockcost = reader["purchasefinstockcost"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["purchasefinstockcost"]);
                        oitem.salevaluefinstockcost = reader["salevaluefinstockcost"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["salevaluefinstockcost"]);
                        oitem.purchasereturnfinstockcost = reader["purchasereturnfinstockcost"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["purchasereturnfinstockcost"]);

                        lst.Add(oitem);
                    }
                }
                conn.Close();

                foreach (StockTransaction item in lst)
                {
                    //item.openingqty = item.currentqty + item.saleqty + item.purchaseretqty - item.purchaseqty;
                    //item.openingstockvalue = item.currentstockvalue + item.salestockvalue + item.purchaseretstockvalue - item.purchasestockvalue;
                    //item.openingsalevalue = item.currentsalevalue + item.salesalevalue + item.purchaseretsalevalue - item.purchasesalevalue;

                    if (openingstock.Exists(e => e.productid == item.productid && e.sectionid == item.sectionid &&
                    e.brandid == item.brandid && e.styleid == item.styleid))
                    {
                        var openingitem = openingstock.Find(e => e.productid == item.productid && e.sectionid == item.sectionid &&
                    e.brandid == item.brandid && e.styleid == item.styleid);

                        item.openingqty = openingitem.openingqty;
                        item.openingstockvalue = openingitem.openingstockvalue;
                        item.openingsalevalue = openingitem.openingsalevalue;
                    }
                    item.closingqty = item.openingqty + item.purchaseqty - item.purchaseretqty - item.saleqty;
                    item.closingstockvalue = item.openingstockvalue + item.purchasestockvalue - item.purchaseretstockvalue - item.salestockvalue;
                    item.closingsalevalue = item.openingsalevalue + item.purchasesalevalue - item.purchaseretsalevalue - item.salesalevalue;
                }
                lst.RemoveAll(e => e.openingqty == 0 && e.closingqty == 0 && e.currentqty == 0);
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _reportcreteria);
            }
            return lst;
        }

        public List<PurhcaseItemDetails> SearchPurchaseReturnSectionwise(WarehouseReportCreteria _reportcreteria)
        {
            const string SQL_ProductwiseDebitNote = @"select products.name productname, products.salescounterid, sum(lrinvoicereturnitems.discount) discount,
sum(lrinvoicereturnitems.tax) tax,sum(lrinvoicereturnitems.amount) amount, sum(lrinvoicereturnitems.qty) qty
from lrinvoicereturn 
inner join lrinvoicereturnitems on lrinvoicereturnitems.lrinvoicereturnid = lrinvoicereturn.id
inner join suppliercompany on suppliercompany.id=lrinvoicereturn.suppliercompanyid 
inner join products on products.id = lrinvoicereturnitems.productid ";

            const string SQL_SectionwiseDebitNote = @"select products.salescounterid, sum(lrinvoicereturnitems.discount) discount,
sum(lrinvoicereturnitems.tax) tax,sum(lrinvoicereturnitems.amount) amount, sum(lrinvoicereturnitems.qty) qty
from lrinvoicereturn 
inner join lrinvoicereturnitems on lrinvoicereturnitems.lrinvoicereturnid = lrinvoicereturn.id
inner join suppliercompany on suppliercompany.id=lrinvoicereturn.suppliercompanyid 
inner join products on products.id = lrinvoicereturnitems.productid ";

            List<PurhcaseItemDetails> lst = new List<PurhcaseItemDetails>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                NpgsqlCommand command = null;

                QueryBuilder qb = new QueryBuilder();
                if (_reportcreteria.showdetail)
                    qb.CurrentQuery = SQL_ProductwiseDebitNote;
                else
                    qb.CurrentQuery = SQL_SectionwiseDebitNote;

                qb.AddParameter("lrinvoicereturn.isactive", true, "=");
                qb.AddParameter("lrinvoicereturnitems.isactive", true, "=");
                qb.AddParameter("lrinvoicereturn.returntype", 14, "<");

                if (_reportcreteria.multicompany && _reportcreteria.companyid > 0)
                    qb.AddParameter("lrinvoicereturn.companyid", _reportcreteria.companyid, "=");

                if (_reportcreteria.fromdate != DateTime.MinValue && _reportcreteria.todate != DateTime.MinValue)
                {
                    qb.AddParameter("lrinvoicereturn.returndate", "datefrom", _reportcreteria.fromdate.Date, ">=");
                    qb.AddParameter("lrinvoicereturn.returndate", "dateto", _reportcreteria.todate.Date.AddDays(1), "<");
                }

                if (_reportcreteria.supplierid > 0)
                    qb.AddParameter("lrinvoicereturn.supplierid", _reportcreteria.supplierid, "=");

                if (_reportcreteria.showdetail)
                    qb.CurrentQuery += @" group by products.name, products.salescounterid order by  products.salescounterid, products.name";
                else
                    qb.CurrentQuery += @" group by products.salescounterid order by  products.salescounterid";

                command = qb.GetCommand(conn);
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        PurhcaseItemDetails oitem = new PurhcaseItemDetails();
                        if (_reportcreteria.showdetail)
                            oitem.productname = reader["productname"] == DBNull.Value ? "" : reader["productname"].ToString();
                        oitem.sectionid = reader["salescounterid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["salescounterid"]);
                        oitem.qty = reader["qty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["qty"]);
                        oitem.taxableamount = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"]);
                        oitem.taxamount = reader["tax"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["tax"]);
                        oitem.discount = reader["discount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discount"]);
                        oitem.invoiceamount = oitem.taxableamount + oitem.discount;
                        oitem.netvalue = oitem.taxableamount - oitem.taxamount;
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

        internal bool ConvertBarcode2PRF(PRFFromPurchaseBarcode prfinfo)
        {
            ProductrequestformDataService productrequestformDataService = new ProductrequestformDataService() { Context = this.Context };
            foreach (var item in prfinfo.items.GroupBy(x => new { x.companyid, x.supplierid }))
            {
                Productrequestform productrequestform = new Productrequestform();
                productrequestform.companyid = item.Key.companyid;
                productrequestform.locationid = prfinfo.locationid;
                productrequestform.requestedby = prfinfo.employeeid;
                productrequestform.requestedon = DateTimeNow;
                productrequestform.isactive = true;
                productrequestform.createdby = Context.UserID;
                productrequestform.modifiedby = Context.UserID;
                productrequestform.createdon = DateTimeNow;
                productrequestform.modifiedon = DateTimeNow;
                productrequestform.Productrequestformitems = new List<Productrequestformitems>();
                foreach (var prfitem in item)
                {
                    string description = "";
                    if (!string.IsNullOrEmpty(prfitem.productname))
                        description += prfitem.productname;
                    if (!string.IsNullOrEmpty(prfitem.brandname))
                        description += "/" + prfitem.brandname;
                    if (!string.IsNullOrEmpty(prfitem.stylename))
                        description += "/" + prfitem.stylename;
                    if (!string.IsNullOrEmpty(prfitem.patternname))
                        description += "/" + prfitem.patternname;
                    if (!string.IsNullOrEmpty(prfitem.typename))
                        description += "/" + prfitem.typename;
                    if (!string.IsNullOrEmpty(prfitem.materialname))
                        description += "/" + prfitem.materialname;
                    if (!string.IsNullOrEmpty(prfitem.colourname))
                        description += "/" + prfitem.colourname;
                    if (!string.IsNullOrEmpty(prfitem.sleevename))
                        description += "/" + prfitem.sleevename;
                    if (!string.IsNullOrEmpty(prfitem.fitname))
                        description += "/" + prfitem.fitname;

                    productrequestform.Productrequestformitems.Add(new Productrequestformitems()
                    {
                        supplierid = item.Key.supplierid,
                        itemid = prfitem.itemid,
                        designid = prfitem.designid,
                        productid = prfitem.productid,
                        qty = prfitem.poqty,
                        pricerange = prfitem.buyingprice.ToString("F2"),
                        isactive = true,
                        createdby = Context.UserID,
                        modifiedby = Context.UserID,
                        createdon = DateTimeNow,
                        modifiedon = DateTimeNow,
                        description = description,
                        detail = new Productrequestformitems.Detail()
                        {
                            brandid = prfitem.brandid,
                            colourid = prfitem.colourid,
                            patternid = prfitem.patternid,
                            typeid = prfitem.typeid,
                            sizeid = prfitem.sizeid,
                            styleid = prfitem.styleid,
                            materialid = prfitem.materialid,
                            sleeveid = prfitem.sleeveid,
                            fitid = prfitem.fitid
                        }
                    });
                }
                productrequestformDataService.Insert(productrequestform);
            }
            return true;
        }

        internal List<StockDepreciation> StockDepreciation(StockDepreciationCreteria item)
        {
            var list = item.config.OrderByDescending(x => x.fromage).ToList();
            string query = "case ";
            int i = 0;
            list.ForEach(x =>
            {
                query = query + " when (current_date - lrinvoice.entrydate::date)>" + (x.fromage - 1).ToString("F0") + " then " + i.ToString("F0");
                x.id = i++;
            });
            query += "when (current_date - lrinvoice.entrydate::date)>0 then -1 else -2 end grpname";

            list.Add(new StockDepreciationCreteria.configuration()
            {
                id = -1,
            });
            list.Add(new StockDepreciationCreteria.configuration()
            {
                id = -2,
            });
            string fullquery = string.Format(@"select {0},
            sum((lrinvoiceitems.amount/lrinvoiceitems.buyingqty)*(iqty-(oqty+dqty))) stockvalue,
            sum((iqty-(oqty+dqty))) stockqty from salablegoods
            inner join stock on stock.id = stockid
            inner join lrinvoiceitems on lrinvoiceitems.id = stock.invoiceitemid
            inner join lrinvoice on lrinvoice.id = lrinvoiceid
            where lrinvoiceitems.isactive=true and lrinvoice.isactive=true
            and salablegoods.isactive=true and salablegoods.hasstock =true
            and lrinvoice.entrydate<@entrydate
            group by 1", query);
            var param = new SortedList<string, object>();
            param.Add("entrydate", item.asondate.AddDays(-1));
            using (var reader = ExecuteReader(fullquery, param))
            {
                while (reader.Read())
                {
                    int id = reader.IsDBNull(1) ? -1 : Convert.ToInt32(reader[0]);
                    decimal value = reader.IsDBNull(1) ? 0 : Convert.ToDecimal(reader[1]);
                    decimal qty = reader.IsDBNull(2) ? 0 : Convert.ToDecimal(reader[2]);
                    list.Find(x => x.id == id).value = value;
                    list.Find(x => x.id == id).qty = qty;
                }
            }

            return list.ConvertAll(x => new Data.StockDepreciation()
            {
                id = x.id,
                fromage = x.fromage,
                toage = x.toage,
                value = x.value,
                depreciation = x.depreciation,
                qty = x.qty,
                dpvalue = Math.Round(x.depreciation * x.value / 100, 0)
            });
        }

        internal List<PurchaseHistoryItem> PurchaseHistory(StockReportCreteria item)
        {
            string purchasehistoryquery = @"
            with purchase as ( 
	            select lrinvoiceitems.id lrinvoiceitemid, lrinvoiceitems.itemid,
				lrinvoiceitems.buyingqty-lrinvoiceitems.returnqty purchaseqty,
				lrinvoice.entrydate,lrinvoice.invoiceno,purchasediscountpercentage,
				buyingprice,designid
				from lrinvoiceitems
				inner join lrinvoice on lrinvoice.id = lrinvoiceid
	            where lrinvoiceitems.isactive=true and lrinvoiceitems.lrinvoiceid = @lrinvoiceid
            ), purchasestock as ( 
	            select lrinvoiceitems.id lrinvoiceitemid,stock.itemid,
				min(salablegoods.price-salablegoods.discount) mnprice,max(salablegoods.price-salablegoods.discount) mxprice,
				sum(salablegoods.iqty-(salablegoods.oqty+salablegoods.jqty+salablegoods.dqty)) stockqty from lrinvoiceitems
	            inner join stock on stock.invoiceitemid = lrinvoiceitems.id
	            inner join salablegoods on  salablegoods.stockid = stock.id
	            where lrinvoiceitems.isactive=true and lrinvoiceitems.lrinvoiceid = @lrinvoiceid
                group by lrinvoiceitems.id,stock.itemid
            ),
            purchasereturn as ( 
	            select lrinvoiceitemsid lrinvoiceitemid,sum(qty) returnqty from lrinvoicereturnitems
                inner join lrinvoiceitems on lrinvoiceitems.id = lrinvoiceitemsid
                where lrinvoiceitems.lrinvoiceid = @lrinvoiceid
                group by lrinvoiceitemsid
            ),
            sales as (
            select lrinvoiceitems.id lrinvoiceitemid,
            sum(billitems.qty) billedqty, sum(billitems.receivable) billedvalue
                from lrinvoiceitems
	            inner join stock on stock.invoiceitemid = lrinvoiceitems.id
	            inner join salablegoods on  salablegoods.stockid = stock.id
	            inner join billitems on billitems.salablegoodsid = salablegoods.id and billitems.isactive=true
                inner join bill on bill.id=billitems.billid and bill.isactive = true and bill.billtype != 9
	            where stock.isactive=true and lrinvoiceitems.isactive=true and lrinvoiceitems.lrinvoiceid = @lrinvoiceid
                group by lrinvoiceitems.id
            )
            select purchase.*, purchasestock.*,sales.*,purchasereturn.*,
			products.name as productname,
			brand.name brandname, style.name stylename, pattern.name patternname, size.name sizename,
			colour.name colourname, type.name typename, products.attributes,
			material.name materialname, sleeve.name sleevename, fit.name fitname
            from purchase 
			left outer join purchasestock on purchasestock.lrinvoiceitemid = purchase.lrinvoiceitemid
            left outer join sales on sales.lrinvoiceitemid = purchase.lrinvoiceitemid
			left outer join purchasereturn on purchasereturn.lrinvoiceitemid = purchase.lrinvoiceitemid
			inner join items on items.id = case when purchasestock.itemid is null then purchase.itemid else purchasestock.itemid end
			left outer join products on products.id = items.productid
			left outer join brand brand on brand.id = items.brandid
			left outer join referencelist colour on colour.id = colourid
			left outer join referencelist size on size.id = sizeid
			left outer join referencelist style on style.id = styleid
			left outer join referencelist pattern on pattern.id = patternid
			left outer join referencelist material on material.id = materialid
			left outer join referencelist type on type.id = typeid
			left outer join referencelist sleeve on sleeve.id = sleeveid
			left outer join referencelist fit on fit.id = fitid";

            List<PurchaseHistoryItem> lst = new List<PurchaseHistoryItem>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                NpgsqlCommand command = new NpgsqlCommand(purchasehistoryquery, conn);
                command.Parameters.Add("lrinvoiceid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.invoiceid;
                command.CommandTimeout = 10000;
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        PurchaseHistoryItem purchaseHistoryItem = new PurchaseHistoryItem();
                        lst.Add(purchaseHistoryItem);
                        Products.ProductAttributes attributes = null;
                        if (reader["attributes"] != DBNull.Value)
                        {
                            attributes = Newtonsoft.Json.JsonConvert.DeserializeObject<Products.ProductAttributes>(Convert.ToString(reader["attributes"]));
                        }
                        purchaseHistoryItem.invoiceno = reader["invoiceno"] == DBNull.Value ? "" : reader["invoiceno"].ToString();
                        purchaseHistoryItem.product = reader["productname"] == DBNull.Value ? "" : reader["productname"].ToString();
                        purchaseHistoryItem.entrydate = reader["entrydate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["entrydate"]);
                        purchaseHistoryItem.cost = reader["buyingprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["buyingprice"]);
                        purchaseHistoryItem.discount = reader["purchasediscountpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["purchasediscountpercentage"]);
                        purchaseHistoryItem.qty = reader["purchaseqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["purchaseqty"]);
                        purchaseHistoryItem.stock = reader["stockqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["stockqty"]);
                        purchaseHistoryItem.rqty = reader["returnqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["returnqty"]);
                        purchaseHistoryItem.bqty = reader["billedqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["billedqty"]);
                        purchaseHistoryItem.bvalue = reader["billedvalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["billedvalue"]);

                        if (purchaseHistoryItem.entrydate != DateTime.MinValue)
                            purchaseHistoryItem.age = DateTimeNow.Subtract(purchaseHistoryItem.entrydate).TotalDays.ToString("F0") + " Days";

                        var mnprice = reader["mnprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["mnprice"]);
                        var mxprice = reader["mxprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["mxprice"]);

                        if (mnprice == mxprice)
                            purchaseHistoryItem.price = mnprice.ToString("F0");
                        else
                            purchaseHistoryItem.price = mnprice.ToString("F0") + "-" + mxprice.ToString("F0");

                        purchaseHistoryItem.margin = Math.Round((mxprice / purchaseHistoryItem.cost * 100) - 100, 2);

                        if (attributes != null)
                        {
                            List<string> stringBuilder = new List<string>();

                            if (attributes.rolbrand &&
                                reader["brandname"] == DBNull.Value &&
                                !string.IsNullOrEmpty(reader["brandname"].ToString()))
                                stringBuilder.Add(reader["brandname"].ToString());

                            if (attributes.roldesignid &&
                                reader["designid"] == DBNull.Value &&
                                !string.IsNullOrEmpty(reader["designid"].ToString()))
                                stringBuilder.Add(reader["designid"].ToString());

                            if (attributes.rolmaterial &&
                                reader["materialname"] == DBNull.Value &&
                                !string.IsNullOrEmpty(reader["materialname"].ToString()))
                                stringBuilder.Add(reader["materialname"].ToString());

                            if (attributes.roltype &&
                                reader["typename"] == DBNull.Value &&
                                !string.IsNullOrEmpty(reader["typename"].ToString()))
                                stringBuilder.Add(reader["typename"].ToString());

                            if (attributes.rolstyle &&
                                reader["stylename"] == DBNull.Value &&
                                !string.IsNullOrEmpty(reader["stylename"].ToString()))
                                stringBuilder.Add(reader["stylename"].ToString());

                            if (attributes.rolpattern &&
                                reader["patternname"] == DBNull.Value &&
                                !string.IsNullOrEmpty(reader["patternname"].ToString()))
                                stringBuilder.Add(reader["patternname"].ToString());


                            if (attributes.rolcolour &&
                                reader["colourname"] == DBNull.Value &&
                                !string.IsNullOrEmpty(reader["colourname"].ToString()))
                                stringBuilder.Add(reader["colourname"].ToString());

                            if (attributes.rolsize &&
                                reader["sizename"] == DBNull.Value &&
                                !string.IsNullOrEmpty(reader["sizename"].ToString()))
                                stringBuilder.Add(reader["sizename"].ToString());

                            if (attributes.rolfit &&
                                reader["fitname"] == DBNull.Value &&
                                !string.IsNullOrEmpty(reader["fitname"].ToString()))
                                stringBuilder.Add(reader["fitname"].ToString());

                            if (attributes.rolsleeve &&
                              reader["sleevename"] == DBNull.Value &&
                              !string.IsNullOrEmpty(reader["sleevename"].ToString()))
                                stringBuilder.Add(reader["sleevename"].ToString());

                            if (stringBuilder.Count > 0)
                                purchaseHistoryItem.detail = string.Join(",", stringBuilder);
                        }
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                conn.Close();
            }
            return lst;
        }

        internal SalesStockReport SearchSaleVsPurchaseVsStock(SalesStockCreteria salesStockCreteria)
        {
            string query = "";
            SalesStockReport salesStockReport = new SalesStockReport();
            salesStockReport.items = new List<SalesStockReport.Item>();
            List<string> fields = new List<string>();
            List<string> criterialist = new List<string>();
            if (salesStockCreteria.showsupplier)
                fields.Add("supplier.name");
            if (salesStockCreteria.showproduct)
                fields.Add("products.name");
            if (salesStockCreteria.showdesign)
                fields.Add("stock.designid");
            if (salesStockCreteria.showbrand)
                fields.Add("brand.name");
            if (salesStockCreteria.showlocation)
                fields.Add("location.name");

            if (salesStockCreteria.companyid > 0)
                criterialist.Add("stock.companyid =@companyid");
            if (salesStockCreteria.productid > 0)
                criterialist.Add("stock.productid =@productid");
            if (salesStockCreteria.supplierid > 0)
                criterialist.Add("lrentry.supplierid =@supplierid");
            if (salesStockCreteria.sectionid > 0)
                criterialist.Add("products.salescounterid =@salescounterid");
            if (salesStockCreteria.productgroupid > 0)
                criterialist.Add("products.productgroup =@productgroupid");
            if (salesStockCreteria.brandid > 0)
                criterialist.Add("items.brandid =@brandid");


            if (salesStockCreteria.locationid == 0)
            {
                string whquery = @"select sum( (stock.qty - (stock.transfered+stock.journalqty+stock.returnqty+stock.damagedqty)) * stock.ibuyingprice::numeric) as ivalue,
                sum(stock.qty-(stock.transfered+stock.journalqty+stock.damagedqty+stock.returnqty)) qty {0}
                from stock left outer join lrinvoiceitems on lrinvoiceitems.id = stock.invoiceitemid 
                left outer join lrinvoice on lrinvoice.id = lrinvoiceitems.lrinvoiceid  
                left outer join lrentry on lrentry.id = lrinvoice.lrid
                left outer join products on products.id = stock.productid 
                left outer join supplier on supplier.id = lrentry.supplierid
                left outer join items on items.id = stock.itemid 
                left outer join brand on brand.id = items.brandid
                left outer join referencelist location on location.id=stock.locationid
                where stock.isactive=true and stock.hasstock=true {2}
                {1}";
                if (salesStockCreteria.companyid > 0)
                    criterialist.Add(" stock.companyid=@companyid ");

                if (fields.Count > 0)
                {
                    whquery = string.Format(whquery, "," + string.Join(",", fields).
                        Replace("supplier.name", "supplier.name suppliername")
                        .Replace("brand.name", "brand.name brandname")
                        .Replace("location.name", "location.name locationname"),
                        " group by " + string.Join(",", fields),
                        criterialist.Count == 0 ? "" : (" and " + string.Join(" and ", criterialist)));
                }
                else
                {
                    whquery = string.Format(whquery, "", "",
                        criterialist.Count == 0 ? "" : (" and " + string.Join(" and ", criterialist)));
                }

                string retquery = @"select sum( (salablegoods.iqty-(salablegoods.oqty+salablegoods.jqty+salablegoods.dqty)) * stock.ibuyingprice::numeric) as ivalue,  
                sum(salablegoods.iqty-(salablegoods.oqty+salablegoods.jqty+salablegoods.dqty)) qty {0}
                from salablegoods inner join stock on  stock.id = salablegoods.stockid
                left outer join lrinvoiceitems on lrinvoiceitems.id = stock.invoiceitemid 
                left outer join lrinvoice on lrinvoice.id = lrinvoiceitems.lrinvoiceid  
                left outer join lrentry on lrentry.id = lrinvoice.lrid
                left outer join products on products.id = stock.productid 
                left outer join supplier on supplier.id = lrentry.supplierid
                left outer join items on items.id = stock.itemid 
                left outer join brand on brand.id = items.brandid
                left outer join referencelist location on location.id=stock.locationid
                where salablegoods.isactive=true and stock.isactive=true 
                and salablegoods.hasstock=true {2}
                {1}";

                if (salesStockCreteria.companyid > 0)
                    criterialist.Add(" salablegoods.companyid=@companyid ");

                if (salesStockCreteria.locationid > 0)
                    criterialist.Add(" salablegoods.locationid=@locationid ");
                if (fields.Count > 0)
                {
                    retquery = string.Format(retquery, "," + string.Join(",", fields).Replace("supplier.name", "supplier.name suppliername")
                        .Replace("brand.name", "brand.name brandname")
                        .Replace("location.name", "location.name locationname"),
                        " group by " + string.Join(",", fields),
                        criterialist.Count == 0 ? "" : (" and " + string.Join(" and ", criterialist)));
                }
                else
                {
                    retquery = string.Format(whquery, "", "",
                        criterialist.Count == 0 ? "" : (" and " + string.Join(" and ", criterialist)));
                }
                if (salesStockCreteria.locationid > 0)
                    criterialist.RemoveAt(criterialist.Count - 1);
                query = string.Format("with allstock as ({0} union all {1}) select * from allstock", whquery, retquery);
            }
            else
            {
                string retquery = @"select sum( (salablegoods.iqty-(salablegoods.oqty+salablegoods.jqty+salablegoods.dqty)) * stock.ibuyingprice::numeric) as ivalue,  
                sum(salablegoods.iqty-(salablegoods.oqty+salablegoods.jqty+salablegoods.dqty)) qty {0}
                from salablegoods inner join stock on  stock.id = salablegoods.stockid
                left outer join lrinvoiceitems on lrinvoiceitems.id = stock.invoiceitemid 
                left outer join lrinvoice on lrinvoice.id = lrinvoiceitems.lrinvoiceid  
                left outer join lrentry on lrentry.id = lrinvoice.lrid
                left outer join products on products.id = stock.productid 
                left outer join supplier on supplier.id = lrentry.supplierid
                left outer join items on items.id = stock.itemid 
                left outer join brand on brand.id = items.brandid
                left outer join referencelist location on location.id=salablegoods.locationid
                where salablegoods.isactive=true and stock.isactive=true 
                and salablegoods.hasstock=true {2}
                {1}";

                if (salesStockCreteria.companyid > 0)
                    criterialist.Add(" salablegoods.companyid=@companyid ");
                if (salesStockCreteria.locationid > 0)
                    criterialist.Add(" salablegoods.locationid=@locationid ");
                if (fields.Count > 0)
                {
                    retquery = string.Format(retquery, "," + string.Join(",", fields)
                        .Replace("supplier.name", "supplier.name suppliername")
                        .Replace("brand.name", "brand.name brandname")
                        .Replace("location.name", "location.name locationname"),
                        " group by " + string.Join(",", fields),
                        criterialist.Count == 0 ? "" : (" and " + string.Join(" and ", criterialist)));
                }

                if (salesStockCreteria.locationid > 0)
                    criterialist.RemoveAt(criterialist.Count - 1);
                query = retquery;
            }

            if (InfyPOS.Services.ConfigManager.Instance(this.Context.OrganizationID).Config.analyser_stock_withzerostock)
            {
                query = query.Replace("and (salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty))>0", "");
                query = query.Replace("and stock.qty - (stock.transfered+stock.returnqty+stock.damagedqty+stock.journalqty)>0", "");
            }

            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                NpgsqlCommand command = new NpgsqlCommand(query, conn);
                command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = salesStockCreteria.locationid;
                command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = salesStockCreteria.companyid;
                command.Parameters.Add("productid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = salesStockCreteria.productid;
                command.Parameters.Add("supplierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = salesStockCreteria.supplierid;
                command.Parameters.Add("salescounterid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = salesStockCreteria.sectionid;
                command.Parameters.Add("productgroupid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = salesStockCreteria.productgroupid;
                command.CommandTimeout = 10000;
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {

                        SalesStockReport.Item item = new SalesStockReport.Item();
                        item.stockqty = reader["qty"] != DBNull.Value ? Convert.ToDecimal(reader["qty"]) : 0;
                        item.stockvalue = reader["ivalue"] != DBNull.Value ? Convert.ToDecimal(reader["ivalue"]) : 0;
                        if (item.stockvalue > 0 && item.stockqty > 0)
                            item.cost = item.stockvalue / item.stockqty;
                        if (salesStockCreteria.showdesign)
                            item.design = reader["designid"] != DBNull.Value ? Convert.ToString(reader["designid"]) : "";
                        if (salesStockCreteria.showproduct)
                            item.productname = reader["name"] != DBNull.Value ? Convert.ToString(reader["name"]) : "";
                        if (salesStockCreteria.showsupplier)
                            item.suppliername = reader["suppliername"] != DBNull.Value ? Convert.ToString(reader["suppliername"]) : "";
                        if (salesStockCreteria.showbrand)
                            item.brandname = reader["brandname"] != DBNull.Value ? Convert.ToString(reader["brandname"]) : "";
                        if (salesStockCreteria.showlocation)
                            item.locationname = reader["locationname"] != DBNull.Value ? Convert.ToString(reader["locationname"]) : "";

                        string key = item.key;
                        if (salesStockReport.items.Exists(e => e.key == key))
                        {
                            salesStockReport.items.Find(e => e.key == key).stockqty += item.stockqty;
                            salesStockReport.items.Find(e => e.key == key).stockvalue += item.stockvalue;
                        }
                        else
                        {
                            salesStockReport.items.Add(item);
                        }
                    }
                }

                string purchasequery = @"select sum( (stock.qty) * stock.ibuyingprice::numeric) as ivalue,
                sum(stock.qty) qty {0}
                from lrentry
                inner join lrinvoice on lrinvoice.lrid = lrentry.id
                inner join lrinvoiceitems on lrinvoiceitems.lrinvoiceid  = lrinvoice.id
                inner join stock on stock.invoiceitemid = lrinvoiceitems.id 
                left outer join products on products.id = stock.productid 
                left outer join supplier on supplier.id = lrentry.supplierid
                left outer join items on items.id = stock.itemid 
                left outer join brand on brand.id = items.brandid
                left outer join referencelist location on location.id=stock.locationid
                where lrinvoiceitems.isactive=true and stock.isactive=true and lrentry.lrentrydate>=@fromdate and lrentry.lrentrydate<@todate {2}
                {1}";


                criterialist.Clear();
                if (salesStockCreteria.companyid > 0)
                    criterialist.Add("stock.companyid =@companyid");
                if (salesStockCreteria.productid > 0)
                    criterialist.Add("stock.productid =@productid");
                if (salesStockCreteria.supplierid > 0)
                    criterialist.Add("lrentry.supplierid =@supplierid");
                if (salesStockCreteria.sectionid > 0)
                    criterialist.Add("products.salescounterid =@salescounterid");
                if (salesStockCreteria.productgroupid > 0)
                    criterialist.Add("products.productgroup =@productgroupid");
                if (salesStockCreteria.brandid > 0)
                    criterialist.Add("items.brandid =@brandid");

                if (salesStockCreteria.locationid > 0)
                    criterialist.Add(" lrentry.entrylocationid=@locationid ");

                if (salesStockCreteria.companyid > 0)
                    criterialist.Add(" lrentry.lrcompanyid=@companyid ");

                if (fields.Count > 0)
                {
                    purchasequery = string.Format(purchasequery, "," + string.Join(",", fields)
                        .Replace("supplier.name", "supplier.name suppliername")
                        .Replace("brand.name", "brand.name brandname")
                        .Replace("location.name", "location.name locationname"),
                        " group by " + string.Join(",", fields),
                        criterialist.Count == 0 ? "" : (" and " + string.Join(" and ", criterialist)));
                }
                else
                {
                    purchasequery = string.Format(purchasequery, "", "",
                        criterialist.Count == 0 ? "" : (" and " + string.Join(" and ", criterialist)));
                }


                command = new NpgsqlCommand(purchasequery, conn);
                command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = salesStockCreteria.locationid;
                command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = salesStockCreteria.companyid;
                command.Parameters.Add("productid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = salesStockCreteria.productid;
                command.Parameters.Add("supplierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = salesStockCreteria.supplierid;
                command.Parameters.Add("fromdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = salesStockCreteria.fromdate.Date;
                command.Parameters.Add("todate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = salesStockCreteria.todate.AddDays(1).Date;
                command.Parameters.Add("salescounterid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = salesStockCreteria.sectionid;
                command.Parameters.Add("productgroupid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = salesStockCreteria.productgroupid;
                command.CommandTimeout = 10000;
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        SalesStockReport.Item item = new SalesStockReport.Item();
                        item.purchaseqty = reader["qty"] != DBNull.Value ? Convert.ToDecimal(reader["qty"]) : 0;
                        item.purchasevalue = reader["ivalue"] != DBNull.Value ? Convert.ToDecimal(reader["ivalue"]) : 0;

                        if (item.purchasevalue > 0 && item.purchaseqty > 0)
                            item.cost = item.purchasevalue / item.purchaseqty;

                        if (salesStockCreteria.showdesign)
                            item.design = reader["designid"] != DBNull.Value ? Convert.ToString(reader["designid"]) : "";
                        if (salesStockCreteria.showproduct)
                            item.productname = reader["name"] != DBNull.Value ? Convert.ToString(reader["name"]) : "";
                        if (salesStockCreteria.showsupplier)
                            item.suppliername = reader["suppliername"] != DBNull.Value ? Convert.ToString(reader["suppliername"]) : "";
                        if (salesStockCreteria.showbrand)
                            item.brandname = reader["brandname"] != DBNull.Value ? Convert.ToString(reader["brandname"]) : "";
                        if (salesStockCreteria.showlocation)
                            item.locationname = reader["locationname"] != DBNull.Value ? Convert.ToString(reader["locationname"]) : "";

                        string key = item.key;
                        if (salesStockReport.items.Exists(e => e.key == key))
                        {
                            salesStockReport.items.Find(e => e.key == key).purchaseqty = item.purchaseqty;
                            salesStockReport.items.Find(e => e.key == key).purchasevalue = item.purchasevalue;
                        }
                        else
                        {
                            salesStockReport.items.Add(item);
                        }

                    }
                }



                string salesquery = @"Select sum(billitems.qty) qty, sum(stock.ibuyingprice::numeric*billitems.qty) productcost, sum(billitems.receivable) ivalue {0}
from bill 
inner join billitems on billitems.billid = bill.id and billitems.isactive=true
inner join salablegoods on salablegoods.id = billitems.salablegoodsid
inner join stock on stock.id = salablegoods.stockid
inner join products on products.id = stock.productid 
left outer join items on items.id = stock.itemid 
left outer join brand on brand.id = items.brandid
left outer join lrinvoiceitems on lrinvoiceitems.id = stock.invoiceitemid
left outer join lrinvoice on lrinvoice.id = lrinvoiceitems.lrinvoiceid
left outer join lrentry on lrentry.id = lrinvoice.lrid 
left outer join supplier on supplier.id = lrentry.supplierid
where bill.isactive=true and bill.billtype!=9 and billdate>=@fromdate and billdate<@todate {2}
{1}";


                criterialist.Clear();
                if (salesStockCreteria.productid > 0)
                    criterialist.Add("stock.productid =@productid");
                if (salesStockCreteria.supplierid > 0)
                    criterialist.Add("lrentry.supplierid =@supplierid");
                if (salesStockCreteria.sectionid > 0)
                    criterialist.Add("products.salescounterid =@salescounterid");
                if (salesStockCreteria.productgroupid > 0)
                    criterialist.Add("products.productgroup =@productgroupid");
                if (salesStockCreteria.locationid > 0)
                    criterialist.Add(" bill.locationid=@locationid ");
                if (salesStockCreteria.companyid > 0)
                    criterialist.Add(" bill.companyid=@companyid ");

                if (fields.Count > 0)
                {
                    salesquery = string.Format(salesquery, "," + string.Join(",", fields)
                        .Replace("supplier.name", "supplier.name suppliername")
                        .Replace("brand.name", "brand.name brandname")
                        .Replace("location.name", "location.name locationname"),
                        " group by " + string.Join(",", fields),
                        criterialist.Count == 0 ? "" : (" and " + string.Join(" and ", criterialist)));
                }
                else
                {
                    salesquery = string.Format(salesquery, "", "",
                        criterialist.Count == 0 ? "" : (" and " + string.Join(" and ", criterialist)));
                }


                command = new NpgsqlCommand(salesquery, conn);
                command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = salesStockCreteria.locationid;
                command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = salesStockCreteria.companyid;
                command.Parameters.Add("productid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = salesStockCreteria.productid;
                command.Parameters.Add("supplierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = salesStockCreteria.supplierid;
                command.Parameters.Add("fromdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = salesStockCreteria.fromdate.Date;
                command.Parameters.Add("todate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = salesStockCreteria.todate.AddDays(1).Date;
                command.Parameters.Add("salescounterid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = salesStockCreteria.sectionid;
                command.Parameters.Add("productgroupid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = salesStockCreteria.productgroupid;
                command.CommandTimeout = 10000;
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        SalesStockReport.Item item = new SalesStockReport.Item();
                        item.salesqty = reader["qty"] != DBNull.Value ? Convert.ToDecimal(reader["qty"]) : 0;
                        item.salesvalue = reader["ivalue"] != DBNull.Value ? Convert.ToDecimal(reader["ivalue"]) : 0;
                        if (item.salesvalue != 0 && item.salesqty != 0)
                            item.salerate = Math.Round(item.salesvalue / item.salesqty, 0);
                        item.cost = reader["productcost"] != DBNull.Value ? Convert.ToDecimal(reader["productcost"]) : 0;

                        if (salesStockCreteria.showdesign)
                            item.design = reader["designid"] != DBNull.Value ? Convert.ToString(reader["designid"]) : "";
                        if (salesStockCreteria.showproduct)
                            item.productname = reader["name"] != DBNull.Value ? Convert.ToString(reader["name"]) : "";
                        if (salesStockCreteria.showsupplier)
                            item.suppliername = reader["suppliername"] != DBNull.Value ? Convert.ToString(reader["suppliername"]) : "";
                        if (salesStockCreteria.showbrand)
                            item.brandname = reader["brandname"] != DBNull.Value ? Convert.ToString(reader["brandname"]) : "";
                        if (salesStockCreteria.showlocation)
                            item.locationname = reader["locationname"] != DBNull.Value ? Convert.ToString(reader["locationname"]) : "";

                        string key = item.key;
                        if (salesStockReport.items.Exists(e => e.key == key))
                        {
                            salesStockReport.items.Find(e => e.key == key).salesqty = item.salesqty;
                            salesStockReport.items.Find(e => e.key == key).salesvalue = item.salesvalue;
                        }
                        else
                        {
                            salesStockReport.items.Add(item);
                        }

                    }
                }




                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null)
                    conn.Close();
            }
            salesStockReport.salesqty = salesStockReport.items.Sum(e => e.salesqty);
            salesStockReport.salesvalue = salesStockReport.items.Sum(e => e.salesvalue);
            salesStockReport.stockqty = salesStockReport.items.Sum(e => e.stockqty);
            salesStockReport.stockvalue = salesStockReport.items.Sum(e => e.stockvalue);
            salesStockReport.purchaseqty = salesStockReport.items.Sum(e => e.purchaseqty);
            salesStockReport.purchasevalue = salesStockReport.items.Sum(e => e.purchasevalue);
            return salesStockReport;
        }
    }

    public class LRReport : BaseEntity
    {
        public string lrsectionname { get; set; }
        public string statusname { get; set; }
        public long companyid { get; set; }
        public long lrid { get; set; }
        public string lrno { get; set; }
        public string lrentryno { get; set; }
        public DateTime lrdate { get; set; }
        public DateTime entrydate { get; set; }
        public DateTime lrreceiveddate { get; set; }
        public DateTime duedate { get; set; }
        public string supplier { get; set; }
        public string purchasername { get; set; }
        public decimal totalamount { get; set; }
        public int noofbundels { get; set; }
        public int noofpieces { get; set; }
        public int noofboxes { get; set; }
        public long entrylocationid { get; set; }
        public long retaillocationid { get; set; }
        public int modeofdelivery { get; set; }
        public string invoiceno { get; set; }
        public string slipno { get; set; }
        public DateTime invoicedate { get; set; }
        public long transportid { get; set; }
        public string transportgst { get; set; }
        public long agentid { get; set; }
        public long suppliercompanycityid { get; set; }
        public int duedays { get; set; }
        public bool hasfreightcharge { get; set; }
        public bool hasloadingcharge { get; set; }
        public decimal freightcharge { get; set; }
        public decimal loadingcharge { get; set; }
        public long bundlerackid { get; set; }
        public TransactionDate trdates { get; set; }
        public class TransactionDate
        {
            public DateTime pricingstart { get; set; }
            public DateTime pricingend { get; set; }
            public DateTime invoicestart { get; set; }
            public DateTime invoiceend { get; set; }
            public DateTime barcodestart { get; set; }
            public DateTime barcodeend { get; set; }
            public DateTime productstartdate { get; set; }
            public DateTime productenddate { get; set; }
        }
    }
    public class AsOnStock : BaseEntity
    {
        public string productname { get; set; }
        public string sectionname { get; set; }
        public string brandname { get; set; }
        public string suppliername { get; set; }
        public decimal asstockqty { get; set; }
        public decimal asstockvalue { get; set; }
        public decimal iasstockvalue { get; set; }
        public decimal assalevalue { get; set; }
        public decimal purchaseqty { get; set; }
        public decimal purchasevalue { get; set; }
        public decimal ipurchasevalue { get; set; }
        public decimal purchasesalevalue { get; set; }
        public decimal auditqty { get; set; }
        public decimal auditvalue { get; set; }
        public decimal iauditvalue { get; set; }
        public decimal auditsalevalue { get; set; }
        public decimal purchasereturnqty { get; set; }
        public decimal purchasereturnvalue { get; set; }
        public decimal ipurchasereturnvalue { get; set; }
        public decimal purchaseretsalevalue { get; set; }
        public decimal saleqty { get; set; }
        public decimal salevalue { get; set; }
        public decimal isalevalue { get; set; }
        public decimal salesalevalue { get; set; }
        public decimal currentqty { get; set; }
        public decimal currentvalue { get; set; }
        public decimal icurrentvalue { get; set; }
        public decimal currentsalevalue { get; set; }
    }
    public class StockTransaction : BaseEntity
    {
        public long productid { get; set; }
        public long sectionid { get; set; }
        public decimal currentqty { get; set; }
        public decimal currentstockvalue { get; set; }
        public decimal currentsalevalue { get; set; }
        public decimal openingqty { get; set; }
        public decimal openingstockvalue { get; set; }
        public decimal openingsalevalue { get; set; }
        public decimal closingqty { get; set; }
        public decimal closingstockvalue { get; set; }
        public decimal closingsalevalue { get; set; }

        public decimal purchaseqty { get; set; }
        public decimal purchasestockvalue { get; set; }
        public decimal purchasesalevalue { get; set; }
        public decimal purchaseretqty { get; set; }
        public decimal purchaseretstockvalue { get; set; }
        public decimal purchaseretsalevalue { get; set; }
        public decimal saleqty { get; set; }
        public decimal salestockvalue { get; set; }
        public decimal salesalevalue { get; set; }
        public decimal auditqty { get; set; }
        public decimal auditstockvalue { get; set; }
        public decimal auditsalevalue { get; set; }
        public decimal jobworkoutward { get; set; }
        public decimal jobworkinward { get; set; }
        public long brandid { get;  set; }
        public long styleid { get;  set; }
        public decimal currentstockfinstockcost { get; set; }
        public decimal purchasefinstockcost { get; set; }
        public decimal salevaluefinstockcost { get; set; }
        public decimal purchasereturnfinstockcost { get; set; }
    }
    public class StockMovement : BaseEntity
    {
        public string barcode { get; set; }
        public long locationid { get; set; }
        public string locationname { get; set; }
        public long supplierid { get; set; }
        public long suppliercityid { get; set; }
        public string lrentryno { get; set; }
        public string invoiceno { get; set; }
        public DateTime invoicedate { get; set; }
        public DateTime inventrydate { get; set; }
        public long productid { get; set; }
        public decimal purchaseqty { get; set; }
        public decimal purchaseretqty { get; set; }
        public decimal saleqty { get; set; }
        public decimal saleretqty { get; set; }
        public decimal inwardqty { get; set; }
        public decimal outwardqty { get; set; }
        public decimal stockqty { get; set; }
        public decimal auditqty { get; set; }
        public decimal purchasevalue { get; set; }
        public decimal purchaseretvalue { get; set; }
        public decimal salevalue { get; set; }
        public decimal saleretvalue { get; set; }
        public decimal inwardvalue { get; set; }
        public decimal outwardvalue { get; set; }
        public decimal stockvalue { get; set; }
        public decimal outpercentage { get; set; }


        public string itemname { get; set; }
        public string serialno { get; set; }
        public string suppliercity { get; set; }
        public string agentcity { get; set; }
        public string brandname { get; set; }
        public string suppliername { get; set; }
        public string agentname { get; set; }
        public DateTime entrydate { get; set; }
        public string patternname { get; set; }
        public string stylename { get; set; }
        public string productname { get; set; }
        public string sizename { get; set; }
        public string materialname { get; set; }
        public string typename { get; set; }
        public string sleevename { get; set; }
        public string fitname { get; set; }
        public string designid { get; set; }
        public string hsncode { get; set; }
        public string colourname { get; set; }
        public string suppliercode { get; set; }
        public int sellingmode { get; set; }
        public string employeename { get; set; }
        public string location { get; set; }
        public DateTime createdon { get; set; }
        public decimal taxablevalue { get; set; }
        public decimal istockvalue { get; set; }
        public decimal grosssalevalue { get; set; }
        public decimal netsalevalue { get; set; }
        public decimal taxvalue { get; set; }
        public decimal taxable { get; set; }
        public decimal margin { get; set; }
        public long markerid { get; set; }
        public long producttype { get; set; }
        public int stockage { get; set; }
        public long productgroupid { get; set; }
        public long sectionid { get; set; }
        public long stockid { get; set; }
        public long invoiceitemid { get; set; }
        public long agentid { get; set; }
        public int stockholdingperiod { get; set; }
        public decimal marginpercentage { get; set; }
        public decimal netmarginpercent { get; set; }
        public decimal markdown { get; set; }
        public decimal salestaxamount { get; set; }
        public decimal salestaxpercent { get; set; }
        public decimal ptaxpercent { get; set; }
        public decimal staxpercent { get; set; }
        public decimal basebuyingprice { get; set; }
        public decimal pdiscoutpercent { get; set; }
        public decimal pdiscout { get; set; }
        public decimal buyingprice { get; set; }
        public decimal ibuyingprice { get; set; }

        public decimal mrp { get; set; }
        public decimal rowexpense { get; set; }
        public decimal rowexpensevalue { get; set; }
        public decimal price { get; set; }
        public decimal dealerprice { get; set; }
        public decimal discount { get; set; }
        public decimal finalprice { get; set; }
    }

    public class POInvoiceSummaryItem : BaseEntity
    {
        public string lrno { get; set; }
        public string lrentryno { get; set; }
        public string invoiceno { get; set; }
        public decimal invoicetotal { get; set; }
        public string pono { get; set; }
        public decimal purchasedqty { get; set; }
        public DateTime entrydate { get; set; }
        public DateTime invoicedate { get; set; }
        public decimal invoicebilled { get; set; }
        public decimal orderedpoqty { get; set; }
        public decimal otherinvoicebilledqty { get; set; }
        public decimal balancepoqty { get; set; }
        public long supplierid { get; set; }
        public DateTime podate { get; set; }

    }
    public class POReport : BaseEntity
    {
        public class taxheader
        {
            public decimal percentage { get; set; }
            public string displaytext { get; set; }
            public int type { get; set; }
            public decimal taxalevalue { get; set; }
        }
        public List<taxheader> taxhead { get; set; }
        public List<POEntry> poentry { get; set; }
    }
    public class POEntry : BaseEntity
    {
        [JsonIgnore]
        public long createdby { get; set; }
        [JsonIgnore]
        public long modifiedby { get; set; }
        public long invoiceid { get; set; }
        public string company { get; set; }
        public string supplier { get; set; }
        public string purchaser { get; set; }
        public string bankname { get; set; }
        public string accountno { get; set; }
        public string ifsccode { get; set; }
        public string gstno { get; set; }
        public string msmeno { get; set; }
        public string hsncode { get; set; }
        public string productname { get; set; }
        public bool isinterstatesale { get; set; }
        public long cityid { get; set; }
        public long agentid { get; set; }
        public long retaillocationid { get; set; }
        public long entrylocationid { get; set; }
        public long receivinglocationid { get; set; }
        public string lrentryno { get; set; }
        public string lrno { get; set; }
        public DateTime lrentrydate { get; set; }
        public DateTime duedate { get; set; }
        public string invoiceno { get; set; }
        public DateTime invoicedate { get; set; }
        public decimal percentage { get; set; }
        public decimal taxamount { get; set; }
        public decimal taxableamount { get; set; }
        public decimal noofbundle { get; set; }
        public decimal invoiceamount { get; set; }
        public decimal invoicetotalamount { get; set; }
        public decimal discount { get; set; }
        public decimal marginpercentage { get; set; }
        public decimal dealercost { get; set; }
        public decimal dealerprice { get; set; }
        public decimal dealermarginpercentage { get; set; }
        public decimal stockage { get; set; }
        public decimal soldpercentage { get; set; }
        public decimal returnfright { get; set; }
        public List<POTaxValue> taxbreakup { get; set; }
        public string measure { get; set; }
        public decimal otheramount { get; set; }
        public decimal cgst { get; set; }
        public decimal sgst { get; set; }
        public decimal igst { get; set; }
        public decimal mrp { get; set; }
        public decimal stocksalevalue { get; set; }
        public decimal igstpercentage { get; set; }
        public decimal cgstpercentage { get; set; }
        public decimal sgstpercentage { get; set; }
        public decimal qty { get; set; }
        public decimal returnqty { get; set; }
        public decimal closingqty { get; set; }
        public decimal taxpercentage { get; set; }
        public decimal closingstockvalue { get; set; }
        public decimal returnamount { get; set; }
        public decimal netamount { get; set; }
        public decimal forwardcharge { get; set; }
        public decimal tcsamount { get; set; }
        public decimal tcspercentage { get; set; }
        public decimal freightamt { get; set; }
        public decimal freighttaxamt { get; set; }
        public decimal packingcharge { get; set; }
        public string customer { get; set; }
        public string billno { get; set; }
        public decimal tdspercentage { get; set; }
        public decimal tdsamount { get; set; }
        public class POTaxValue
        {
            public decimal taxpercent { get; set; }
            public decimal taxamount { get; set; }
            public decimal taxableamount { get; set; }
            public decimal qty { get; set; }
        }

        public decimal entryqty { get; set; }
        public decimal entrytaxable { get; set; }
        public decimal entrytax { get; set; }
        public decimal entrytotalamount { get; set; }
        public decimal difference { get; set; }
        public DateTime barcodedon { get; set; }
    }

    public class PurhcaseItemDetails : POEntry
    {
        public decimal qty { get; set; }
        public long sectionid { get; set; }
        public decimal discount { get; set; }
        public decimal basebuyingprice { get; set; }
        public decimal netvalue { get; set; }
    }

    public class ROLStoreView : BaseEntity
    {
        public long locationid { get; set; }
        public bool iswarehouse { get; set; }
        public string company { get; set; }
        public long supplierid { get; set; }
        public string productname { get; set; }
        public string barcode { get; set; }
        public long lrid { get; set; }
        public decimal pendingdays { get; set; }

        public string lrno { get; set; }
        public DateTime lrdate { get; set; }
        public string lrentryno { get; set; }
        public DateTime lrentrydate { get; set; }
        public string invoiceno { get; set; }
        public DateTime invoicedate { get; set; }
        public decimal qty { get; set; }
        public decimal iqty { get; set; }
        public decimal oqty { get; set; }
        public long id { get; set; }
        public long stockid { get; set; }
        public long itemid { get; set; }
        public long companyid { get; set; }
        public string lrreference { get; set; }
        public long sectionid { get; set; }
    }
    public class PurchaseReturnReport : BaseEntity
    {
        public class taxheader
        {
            public decimal percentage { get; set; }
            public string displaytext { get; set; }
            public int type { get; set; }
        }
        public List<taxheader> taxhead { get; set; }
        public List<PurchaseReturnEntry> purchasereturn { get; set; }
    }
    public class PurchaseReturnEntry : POEntry
    {
        public long locationid { get; set; }
        public string returnno { get; set; }
        public DateTime returndate { get; set; }
        public decimal discount { get; set; }
        public decimal salevalue { get; set; }
        public long productid { get; set; }
        public long itemid { get; set; }
        public long stockid { get; set; }
        public decimal qty { get; set; }
        public string barcode { get; set; }
        public string taxname { get; set; }
        public string createdemployee { get; set; }
        public string modifiedemployee { get; set; }
    }
    public class SwapBarcode : BaseEntity
    {
        public BarcodeReport frombarcode { get; set; }
        public BarcodeReport tobarcode { get; set; }
        public long locationid { get; set; }
    }
    public class SplitBarcode : BarcodeReport
    {
        public int mode { get; set; }
        public long stockmarkerid { get; set; }
        public int actionmode { get; set; }
        public bool assignmarker { get; set; }
    }
    public class BarcodeReport : LRReport
    {
        public long stockid { get; set; }
        public long itemid { get; set; }
        public long locationid { get; set; }
        public long lrinvoiceid { get; set; }
        public long lrinvoiceitemid { get; set; }
        public long productid { get; set; }
        public int sellingmode { get; set; }
        public string company { get; set; }
        public string barcode { get; set; }
        public string barcodes { get; set; }
        public decimal splitqty { get; set; }
        public int splitpiece { get; set; }
        public long salablegoodsid { get; set; }
        public string itemname { get; set; }
        public string brandname { get; set; }
        public string patternname { get; set; }
        public string stylename { get; set; }
        public string productname { get; set; }
        public string sizename { get; set; }
        public string materialname { get; set; }
        public string typename { get; set; }
        public string sectionname { get; set; }
        public string pricetagname { get; set; }
        public bool isbit { get; set; }
        public string designid { get; set; }
        public decimal buyingprice { get; set; }
        public decimal price { get; set; }
        public decimal discount { get; set; }
        public decimal nprice { get; set; }
        public decimal ndiscount { get; set; }
        public decimal npdiscount { get; set; }
        public decimal pqty { get; set; }
        public decimal wstock { get; set; }
        public decimal rstock { get; set; }
        public long companyid { get; set; }
        public decimal buyingqty { get; set; }
        public decimal mrp { get; set; }
        public decimal sellingprice { get; set; }
        public decimal basebuyingprice { get; set; }
        public decimal pdiscountpercent { get; set; }
        public decimal pdiscount { get; set; }
        public decimal ptax { get; set; }
        public decimal ptaxpercent { get; set; }
        public string hsncode { get; set; }
        public decimal salediscount { get; set; }
        public decimal salediscountpercent { get; set; }
        public decimal marginpercent { get; set; }
        public decimal saletaxpercent { get; set; }
        public decimal saletax { get; set; }
        public long changeproductid { get; set; }
        public List<StockReport> retailstock { get; set; }
        public string newbarcodelist { get; set; }
        public string newbarcode { get; set; }
    }
    public class OutwardReport : BaseEntity
    {
        public string lrentry { get; set; }
        public string lrinvoice { get; set; }
        public string code { get; set; }
        public decimal qty { get; set; }
        public decimal aqty { get; set; }
        public long fromcompanyid { get; set; }
        public long tocompanyid { get; set; }
        public bool iscompanytransfer { get; set; }
        public string fromlocation { get; set; }
        public string tolocation { get; set; }
        public DateTime packedon { get; set; }
        public DateTime receivedon { get; set; }
        public string receivedby { get; set; }
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string items_json
        {
            get { return Newtonsoft.Json.JsonConvert.SerializeObject(items); }
            set { items = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Bundleitem>>(value); }
        }
        public List<Bundleitem> items { get; set; }

        public string productname { get; set; }
        public string section { get; set; }
        public string suppliername { get; set; }
        public string barcode { get; set; }
        public decimal buyingprice { get; set; }
        public decimal ibuyingprice { get; set; }
        public decimal price { get; set; }
        public decimal mrp { get; set; }
        public decimal discount { get; set; }
        public decimal saleprice { get; set; }
        public decimal taxamount { get; set; }
        public decimal taxpercent { get; set; }
        public string packedby { get; set; }
        public string tocompany { get; set; }
        public string brandname { get; set; }
        public string patternname { get; set; }
        public string stylename { get; set; }
        public string sizename { get; set; }
        public string materialname { get; set; }
        public string typename { get; set; }
        public string designid { get; set; }
        public string hsncode { get; set; }
        public string colourname { get; set; }
        public string invoiceno { get; set; }
        public DateTime invoicedate { get; set; }
        public string printingname { get { return productname; } }
        public decimal amount
        {
            get
            {
                if (qty > 0) return qty * buyingprice;
                return 0;
            }
        }
        public decimal itembuyingprice { get; set; }
        public decimal itemibuyingprice { get; set; }
        public decimal itemprice { get; set; }
        public decimal itemmrp { get; set; }
        public decimal itemdiscount { get; set; }
        public decimal itemsaleprice { get { return itemprice - itemdiscount; } }
        public bool isbit { get; set; }
        public int status { get; set; }
    }
    public class WarehouseReportCreteria : BaseEntity
    {
        public bool multicompany { get; set; }
        public DateTime frompentrydate { get; set; }
        public DateTime topentrydate { get; set; }
        public DateTime fromdate { get; set; }
        public DateTime todate { get; set; }
        public DateTime fromlrdate { get; set; }
        public DateTime tolrdate { get; set; }
        public DateTime frominvdate { get; set; }
        public DateTime toinvdate { get; set; }
        public long bundlerackid { get; set; }
        public long locationid { get; set; }
        public long fromlocationid { get; set; }
        public long companyid { get; set; }
        public long tocompanyid { get; set; }
        public long supplierid { get; set; }
        public long transportid { get; set; }
        public bool showdetail { get; set; }
        public bool showsummary { get; set; }
        public bool pendinginward { get; set; }
        public bool withlr { get; set; }
        public string packageno { get; set; }
        public string lrno { get; set; }
        public string lrentryno { get; set; }
        public string invoiceno { get; set; }
        public string returnno { get; set; }
        public bool cancelled { get; set; }
        public int modeofdelivery { get; set; }
        public bool groupsupplier { get; set; }
        public long purchaserid { get; set; }
        public long agentid { get; set; }
        public long suppliercityid { get; set; }
        public long receivinglocationid { get; set; }
        public bool removemfg { get; set; }
        public bool excludesettled { get; set; }
        public bool onlymsmeno { get; set; }
        public int returntypeid { get; set; }
        public bool showbarcodedon { get; set; }
        public DateTime barcodedfrom { get; set; }
        public DateTime barcodedto { get; set; }
    }

    public class StockDepreciation : BaseEntity
    {
        public int id { get; set; }
        public long companyid { get; set; }
        public int fromage { get; set; }
        public int toage { get; set; }
        public decimal depreciation { get; set; }
        public decimal dpvalue { get; set; }
        public decimal qty { get; set; }
        public decimal value { get; set; }
        public decimal stvalue
        {
            get
            {
                return value - dpvalue;
            }
        }
        public string display
        {
            get
            {
                if (id == -1) return "Other Ages";
                if (id == -2) return "Unknown Ages";
                return fromage.ToString("F0") + " - " + toage.ToString("F0");
            }
        }

    }
    public class StockDepreciationCreteria : BaseEntity
    {
        public long companyid { get; set; }
        public DateTime asondate { get; set; }
        public List<configuration> config { get; set; }
        public class configuration
        {
            [JsonIgnore]
            public int id { get; set; }
            public int fromage { get; set; }
            public int toage { get; set; }
            public decimal depreciation { get; set; }
            [JsonIgnore]
            public decimal value { get; set; }
            [JsonIgnore]
            public decimal qty { get; set; }
        }
    }
    public class StockMovementCreteria : BaseEntity
    {
        public bool multicompany { get; set; }
        public long locationid { get; set; }
        public long companyid { get; set; }
        public long productid { get; set; }
        public long supplierid { get; set; }
        public long suppliercityid { get; set; }
        public DateTime fromdate { get; set; }
        public DateTime todate { get; set; }
        public string lrentryno { get; set; }
        public string invoiceno { get; set; }
        public bool hasallfields { get; set; }
        public bool haslocation { get; set; }
        public bool hassupplier { get; set; }
        public bool haslrentryno { get; set; }
        public bool hasinvoiceno { get; set; }
        public bool hasinventrydate { get; set; }
        public bool hasinvoicedate { get; set; }
        public bool hasproduct { get; set; }
        public bool hasbarcode { get; set; }
    }
    public class SalesStockCreteria : BaseEntity
    {
        public long companyid { get; set; }
        public long locationid { get; set; }
        public long productid { get; set; }
        public long supplierid { get; set; }
        public long sectionid { get; set; }
        public long brandid { get; set; }
        public long productgroupid { get; set; }
        public string title { get; set; }
        public bool showdesign { get; set; }
        public bool showproduct { get; set; }
        public bool showsupplier { get; set; }
        public bool showbrand { get; set; }
        public bool showlocation { get; set; }
        public string pricerange { get; set; }
        public string costrange { get; set; }
        public DateTime fromdate { get; set; }
        public DateTime todate { get; set; }

    }
    public class SalesStockReport : BaseEntity
    {
        public class Item
        {
            public string productname { get; set; }
            public string suppliername { get; set; }
            public string brandname { get; set; }
            public string locationname { get; set; }
            public string design { get; set; }
            public decimal purchaseqty { get; set; }
            public decimal stockqty { get; set; }
            public decimal salesqty { get; set; }
            public decimal stockvalue { get; set; }
            public decimal salesvalue { get; set; }
            public decimal purchasevalue { get; set; }
            public decimal cost { get; set; }
            public string key
            {
                get
                {
                    return productname + "-" + locationname + "-" + suppliername + "-" + brandname + "-" + design;
                }
            }

            public decimal salerate { get; set; }
        }
        public decimal stockqty { get; set; }
        public decimal salesqty { get; set; }
        public decimal purchaseqty { get; set; }
        public decimal stockvalue { get; set; }
        public decimal salesvalue { get; set; }
        public decimal purchasevalue { get; set; }
        public string title { get; set; }
        public List<Item> items { get; set; }
    }
}
