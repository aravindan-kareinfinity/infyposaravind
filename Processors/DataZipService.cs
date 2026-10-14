using InfyPOS.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using UnityEngine;

namespace InfyPOS.Processors
{
    public class DataZipService : BaseData
    {
        public void Prepare(DateTime fromdate)
        {
            bool hasavoidmigrationcolumn = false;
            using (var reader = ExecuteReader(@"
            select count(*) from INFORMATION_SCHEMA.TABLES tables
            inner join INFORMATION_SCHEMA.COLUMNS columns on columns.table_name = tables.table_name
            where tables.table_name = 'lrinvoice'
			and column_name ilike '%avoidmigration%'", null))
            {
                hasavoidmigrationcolumn = reader.Read() && !reader.IsDBNull(0) && reader.GetInt32(0) > 0;
            }

            if (!hasavoidmigrationcolumn)
            {
                ExecuteQuery(@"alter table lrinvoice add avoidmigration bool;", null);                
            }

            string query = @"
update lrinvoice set avoidmigration = false;
update lrinvoice set avoidmigration=true where createdon>=@fromdate;
update lrinvoice set avoidmigration=true where entrydate>=@fromdate;
update lrinvoice set avoidmigration=true where invoicedate>=@fromdate;";

            var param = new SortedList<string, object>();
            param.Add("fromdate", fromdate);
            var noofrows = ExecuteQuery(query, param, 1000000);

            query = @"
with stockinvoicelist as
(
select distinct lrinvoiceitems.lrinvoiceid from salablegoods 
inner join stock on stock.id =stockid
inner join lrinvoiceitems on lrinvoiceitems.id = invoiceitemid
where salablegoods.hasstock=true
)
update lrinvoice set avoidmigration=true where id in (select lrinvoiceid from stockinvoicelist);

with billedinvoicelist as
(
select distinct lrinvoiceitems.lrinvoiceid from bill
inner join billitems on billitems.billid = bill.id and billitems.isactive=true
inner join salablegoods on salablegoods.id = billitems.salablegoodsid
inner join stock on stock.id =salablegoods.stockid
inner join lrinvoiceitems on lrinvoiceitems.id = invoiceitemid
where bill.billdate>=@fromdate
)
update lrinvoice set avoidmigration=true where id in (select lrinvoiceid from billedinvoicelist);";
            noofrows = ExecuteQuery(query, param,1000000);
        }

        public void Zip()
        {
            StockTable();
            SalablegoodsTable();
            LrinvoiceitemsTable();
            LrinvoiceTable();
        }

        public void Cleanup(DateTime date)
        {
            string query = @"delete from stock where id in (select id from zip_stock);
delete from salablegoods where id in (select id from zip_salablegoods);
delete from lrinvoice where id in (select id from zip_lrinvoice);
delete from lrinvoiceitems where id in (select id from zip_lrinvoiceitems);
delete from bill where id in (select id from zip_bill);
delete from billitems where id in (select id from zip_billitems);
delete from billsettlementmaster where id in (select id from zip_billsettlementmaster);
delete from billsettlement where id in (select id from zip_billsettlement);";
        }

        public bool HasTable(string tableName)
        {
            var param = new SortedList<string, object>();
            param.Add("tablename", tableName.ToLower());
            using (var reader = ExecuteReader(@"select count(*) from INFORMATION_SCHEMA.TABLES
where table_name = @tablename", param))
            {
                return reader.Read() && !reader.IsDBNull(0) && reader.GetInt32(0) > 0;
            }
            return false;
        }

        public void Cleanup()
        {

        }
        public void StockTable()
        {
            string preparedquery = @"SELECT current_date as zippedon,supplierid, designid, lrreference, buyingprice, stockdate, hsncode, isbit,
minvoicedate, locationid, imageid, serialno, expiry, batch, retaillocationid, 
transferedout, transferedin, purchasereturnqty, purchaseqty, mismatchqty, oldpurchaseqty,
ibuyingprice, id, isactive, createdby, modifiedby, version, createdon, modifiedon, 
printingname, price, barcode, invoiceitemid, itemid, productid, discount, 
additionaldiscount, qty, companyid, transfered, damagedqty, returnqty, runtimeprice, 
hasstock, barcodeid, dealerprice, displaymrp, displayinvoiceno, displayinvoicedate, 
eprice, journalqty, tax_intermediate_dealerprice, srcsupplierid, pricetagid, isuan, 
ismultiprice, sizeinmeter
into Zip_stock
FROM stock limit 0";
            if(!HasTable("Zip_stock"))
                ExecuteQuery(preparedquery, null);

            string processquery = @"insert into Zip_stock(zippedon,supplierid, designid, lrreference, buyingprice, stockdate, hsncode, isbit,
minvoicedate, locationid, imageid, serialno, expiry, batch, retaillocationid, 
transferedout, transferedin, purchasereturnqty, purchaseqty, mismatchqty, oldpurchaseqty,
ibuyingprice, id, isactive, createdby, modifiedby, version, createdon, modifiedon, 
printingname, price, barcode, invoiceitemid, itemid, productid, discount, 
additionaldiscount, qty, companyid, transfered, damagedqty, returnqty, runtimeprice, 
hasstock, barcodeid, dealerprice, displaymrp, displayinvoiceno, displayinvoicedate, 
eprice, journalqty, tax_intermediate_dealerprice, srcsupplierid, pricetagid, isuan, 
ismultiprice, sizeinmeter)

SELECT current_date as zippedon,supplierid, stock.designid, lrreference, 
stock.buyingprice, stockdate, stock.hsncode, isbit,
minvoicedate, stock.locationid, imageid, serialno, expiry, batch, retaillocationid, 
transferedout, transferedin, purchasereturnqty, purchaseqty, mismatchqty, oldpurchaseqty,
ibuyingprice, stock.id, stock.isactive, stock.createdby, stock.modifiedby, 
stock.version, stock.createdon, stock.modifiedon, 
printingname, price, barcode, invoiceitemid, stock.itemid, 
stock.productid, stock.discount, 
additionaldiscount, qty, stock.companyid, transfered, stock.damagedqty, 
stock.returnqty, runtimeprice, 
hasstock, barcodeid, stock.dealerprice, displaymrp, displayinvoiceno, displayinvoicedate, 
eprice, journalqty, tax_intermediate_dealerprice, srcsupplierid, pricetagid, isuan, 
ismultiprice, sizeinmeter
FROM lrinvoice
inner join lrinvoiceitems on lrinvoiceitems.lrinvoiceid = lrinvoice.id
inner join stock on stock.invoiceitemid = lrinvoiceitems.id
where lrinvoice.avoidmigration = false";
            ExecuteQuery(processquery, null, 1000000);
        }


        public void SalablegoodsTable()
        {
            string preparedquery = @"
            SELECT current_date as zippedon,companyid, tiqty, toqty, rqty, ispurchaselocation, 
								pqty, refillboxid, price, version, asalqty, apurretqty, 
								createdon, flag, additionaldiscount, discount, createdby,
								isactive, supplierid, inwardon, icostprice, costprice,
								modifiedby, cstock, modifiedon, locationid, barcode,
								stockid, bundleid, id, salereturnqty, received, dqty,
								oqty, iqty, itemid, hasstock, jqty, aqty, 
								tax_intermediate_price, olqty, cbqty, dmpqty
            into Zip_salablegoods
            FROM salablegoods limit 0;";
            if (!HasTable("Zip_salablegoods"))
                ExecuteQuery(preparedquery, null);

            string processquery = @"INSERT INTO public.Zip_salablegoods(zippedon,companyid, tiqty, toqty, rqty, ispurchaselocation, 
								pqty, refillboxid, price, version, asalqty, apurretqty, 
								createdon, flag, additionaldiscount, discount, createdby,
								isactive, supplierid, inwardon, icostprice, costprice,
								modifiedby, cstock, modifiedon, locationid, barcode,
								stockid, bundleid, id, salereturnqty, received, dqty,
								oqty, iqty, itemid, hasstock, jqty, aqty, 
								tax_intermediate_price, olqty, cbqty, dmpqty)

                                select current_date zippedon,salablegoods.companyid, tiqty, toqty, rqty, ispurchaselocation, pqty, 
                                refillboxid, salablegoods.price, salablegoods.version, asalqty, apurretqty, salablegoods.createdon, flag, 
                                salablegoods.additionaldiscount, 
                                salablegoods.discount, salablegoods.createdby, salablegoods.isactive, salablegoods.supplierid, inwardon, icostprice, 
                                costprice, salablegoods.modifiedby, 
                                cstock, salablegoods.modifiedon, salablegoods.locationid, 
                                salablegoods.barcode, stockid, bundleid, salablegoods.id, salereturnqty, received,
                                dqty, oqty, iqty, salablegoods.itemid, salablegoods.hasstock, jqty, aqty, tax_intermediate_price, olqty, cbqty, 
                                dmpqty
                                FROM lrinvoice
                                inner join lrinvoiceitems on lrinvoiceitems.lrinvoiceid = lrinvoice.id
                                inner join stock on stock.invoiceitemid = lrinvoiceitems.id
                                inner join salablegoods on salablegoods.stockid = stock.id
                                where lrinvoice.avoidmigration = false;";
            ExecuteQuery(processquery, null, 1000000);
        }

        public void LrinvoiceitemsTable()
        {
            string preparedquery = @"SELECT current_date as zippedon,pricingid, buyingqty, buyingprice, mrp, sellingprice, id, isactive, createdby, modifiedby, version, createdon, modifiedon, itemid, 
damagedqty, sellableqty, lrinvoiceid, barcodes, designid, itemdetail, returnqty, basebuyingprice, hsncode, salediscount, purchasediscount, purchasediscountpercentage, 
salediscountpercentage, marginpercentage, purchasetaxpercentage, purchasetax, salestaxpercentage, salestax, sno, taxid, ptaxid, poinfo, actualqty, priceinfo, 
stocklocationid, amount, purchasetotaldiscount, sourceid, productid, wsrate, dealerprice, workingcharge, weight, jobworkerid, jobworkcharge, jobworktax, 
imgid, jobworkgrnitemid, buyingitemid, buyingdesignid, wastage, rowexpense, rowexpensevalue, isseconds, uan, sourcesupplierid, itbuscode, pentrydate, 
cf_bodycolour, freeqty, weightrate, cf_bottomcolour, cf_bodertype, cesstaxid, cesstax, cesspercentage, barcodedon, fstkrmv
into Zip_lrinvoiceitems
FROM lrinvoiceitems limit 0;";
            if (!HasTable("Zip_lrinvoiceitems"))
                ExecuteQuery(preparedquery, null);

            string processquery = @"INSERT INTO public.Zip_lrinvoiceitems(zippedon,pricingid, buyingqty, buyingprice, mrp, sellingprice, id, isactive, createdby, modifiedby, version, createdon, modifiedon, itemid, 
damagedqty, sellableqty, lrinvoiceid, barcodes, designid, itemdetail, returnqty, basebuyingprice, hsncode, salediscount, purchasediscount, purchasediscountpercentage, 
salediscountpercentage, marginpercentage, purchasetaxpercentage, purchasetax, salestaxpercentage, salestax, sno, taxid, ptaxid, poinfo, actualqty, priceinfo, 
stocklocationid, amount, purchasetotaldiscount, sourceid, productid, wsrate, dealerprice, workingcharge, weight, jobworkerid, jobworkcharge, jobworktax, 
imgid, jobworkgrnitemid, buyingitemid, buyingdesignid, wastage, rowexpense, rowexpensevalue, isseconds, uan, sourcesupplierid, itbuscode, pentrydate, 
cf_bodycolour, freeqty, weightrate, cf_bottomcolour, cf_bodertype, cesstaxid, cesstax, cesspercentage, barcodedon, fstkrmv)

select current_date as zippedon,pricingid, buyingqty, buyingprice, mrp, sellingprice, 
lrinvoiceitems.id, lrinvoiceitems.isactive, lrinvoiceitems.createdby, 
lrinvoiceitems.modifiedby, lrinvoiceitems.version, lrinvoiceitems.createdon, 
lrinvoiceitems.modifiedon, lrinvoiceitems.itemid, 
damagedqty, sellableqty, lrinvoiceid, barcodes, designid, itemdetail, returnqty, 
lrinvoiceitems.basebuyingprice, hsncode, salediscount, purchasediscount, purchasediscountpercentage, 
salediscountpercentage, marginpercentage, purchasetaxpercentage, purchasetax, salestaxpercentage, salestax, sno, taxid, ptaxid, poinfo, actualqty, priceinfo, 
stocklocationid, amount, purchasetotaldiscount, sourceid, productid, wsrate, dealerprice, workingcharge, weight, jobworkerid, jobworkcharge, jobworktax, 
imgid, jobworkgrnitemid, buyingitemid, buyingdesignid, wastage, rowexpense, rowexpensevalue, isseconds, uan, sourcesupplierid, 
lrinvoiceitems.itbuscode, pentrydate, 
cf_bodycolour, freeqty, weightrate, cf_bottomcolour, cf_bodertype, cesstaxid, cesstax, cesspercentage, barcodedon, fstkrmv
from lrinvoiceitems
inner join lrinvoice on lrinvoice.id = lrinvoiceitems.lrinvoiceid
where lrinvoice.avoidmigration = false;";
            ExecuteQuery(processquery, null, 1000000);
        }

        public void LrinvoiceTable()
        {
            string preparedquery = @"select current_date as zippedon,entrydate, invoiceentryno, isinclusivetax, isinclusivediscount, isinclusiveall, billvalue, isinterstatesale, basebuyingprice, 
            taxamount, discount, percentage, isactive, suppliercompanyid, noofbundle, version, modifiedby, createdby, createdon, modifiedon, otheramount, id, lrid, totalamount, 
            companyid, invoiceno, invoicedate, noofpieces, grossamount, isgenerated, baseamount, buyerinvoiceid, isvendorbill, barcodegenerationapproval, isdeliverychallan, 
            settled, statusid, isdirectpurchase, lrsupplierid, locationid, expensepercentage, plessdiscount, plessvalue, plannedpayment, expenseperpiece, hascommission, 
            gstinboxid, isimported, isintercompanytransfer, iscreditnote, pono, poid, potypeid, itbuscode, isrcm, hastds, tdsledgerid, tdsamount, tdspercentage, hasapprove, 
            approvedby, approvedon
            into Zip_lrinvoice
            FROM lrinvoice limit 0;";
            if (!HasTable("Zip_lrinvoice"))
                ExecuteQuery(preparedquery, null);

            string processquery = @"INSERT INTO public.Zip_lrinvoice(zippedon,entrydate, invoiceentryno, isinclusivetax, isinclusivediscount, isinclusiveall, billvalue, isinterstatesale, basebuyingprice, 
            taxamount, discount, percentage, isactive, suppliercompanyid, noofbundle, version, modifiedby, createdby, createdon, modifiedon, otheramount, id, lrid, totalamount, 
            companyid, invoiceno, invoicedate, noofpieces, grossamount, isgenerated, baseamount, buyerinvoiceid, isvendorbill, barcodegenerationapproval, isdeliverychallan, 
            settled, statusid, isdirectpurchase, lrsupplierid, locationid, expensepercentage, plessdiscount, plessvalue, plannedpayment, expenseperpiece, hascommission, 
            gstinboxid, isimported, isintercompanytransfer, iscreditnote, pono, poid, potypeid, itbuscode, isrcm, hastds, tdsledgerid, tdsamount, tdspercentage, hasapprove, 
            approvedby, approvedon)

            select current_date as zippedon,entrydate, invoiceentryno, isinclusivetax, isinclusivediscount, isinclusiveall, billvalue, isinterstatesale, basebuyingprice, 
            taxamount, discount, percentage, isactive, suppliercompanyid, noofbundle, version, modifiedby, createdby, createdon, modifiedon, otheramount, id, lrid, totalamount, 
            companyid, invoiceno, invoicedate, noofpieces, grossamount, isgenerated, baseamount, buyerinvoiceid, isvendorbill, barcodegenerationapproval, isdeliverychallan, 
            settled, statusid, isdirectpurchase, lrsupplierid, locationid, expensepercentage, plessdiscount, plessvalue, plannedpayment, expenseperpiece, hascommission, 
            gstinboxid, isimported, isintercompanytransfer, iscreditnote, pono, poid, potypeid, itbuscode, isrcm, hastds, tdsledgerid, tdsamount, tdspercentage, hasapprove, 
            approvedby, approvedon
            from lrinvoice
            where lrinvoice.avoidmigration = false;";
            ExecuteQuery(processquery, null, 1000000);
        }


        public void BillTable(DateTime fromdate)
        {
            var param = new SortedList<string, object>();
            param.Add("@fromdate", fromdate);
            string preparedquery = @"select current_date as zippedon,id, isactive, createdby, modifiedby, version, createdon, modifiedon, billmasterid, 
billno, billdate, companyid, tax, discount, receivable, customerid, additionaldiscount, billtype,
discountstatus, isinterstatesale, qty, locationid, settled, rounding, counterid, returnreasonid, 
returnbillno, haspromotionapplied, salereasonid, areaid, seasonid, billattributes, offline, 
isinterstatetransfer, unsettledcancelled, emidetail, hasemi, floorid, isb2b, additionalcharges, 
isreturn, iscancel, iscreditapproved, iscreditbill, tzoffset, settledcancelled, unlimitedcouponid, 
isexport, istransfer, customerstateid, ecomorderno
            into Zip_bill
            FROM bill limit 0;";
            if (!HasTable("Zip_bill"))
                ExecuteQuery(preparedquery, null);

            string processquery = @"INSERT INTO public.Zip_bill(zippedon,id,isactive, createdby, modifiedby, version, createdon, modifiedon, billmasterid, 
billno, billdate, companyid, tax, discount, receivable, customerid, additionaldiscount, billtype,
discountstatus, isinterstatesale, qty, locationid, settled, rounding, counterid, returnreasonid, 
returnbillno, haspromotionapplied, salereasonid, areaid, seasonid, billattributes, offline, 
isinterstatetransfer, unsettledcancelled, emidetail, hasemi, floorid, isb2b, additionalcharges, 
isreturn, iscancel, iscreditapproved, iscreditbill, tzoffset, settledcancelled, unlimitedcouponid, 
isexport, istransfer, customerstateid, ecomorderno)

            select current_date as zippedon,id, isactive, createdby, modifiedby, version, createdon, modifiedon, billmasterid, 
billno, billdate, companyid, tax, discount, receivable, customerid, additionaldiscount, billtype,
discountstatus, isinterstatesale, qty, locationid, settled, rounding, counterid, returnreasonid, 
returnbillno, haspromotionapplied, salereasonid, areaid, seasonid, billattributes, offline, 
isinterstatetransfer, unsettledcancelled, emidetail, hasemi, floorid, isb2b, additionalcharges, 
isreturn, iscancel, iscreditapproved, iscreditbill, tzoffset, settledcancelled, unlimitedcouponid, 
isexport, istransfer, customerstateid, ecomorderno
            from bill
            where bill.billdate<@fromdate";
            ExecuteQuery(processquery, param, 1000000);
        }

        public void BillSettlementMaster(DateTime fromdate)
        {
            var param = new SortedList<string, object>();
            param.Add("@fromdate", fromdate);
            string preparedquery = @"select current_date as zippedon,id, isactive, createdby, modifiedby, version, createdon, modifiedon, mode, billamount, 
	paymentinfo, customerid, paidamount, code, settlementon, discount, receivable, tax, 
	billmastercode, creditapprovedby, creditreason, cashierid, counterid, billadditionaldiscount, 
	locationid, paid_cash, paid_card, paid_credit, paid_voucher, paid_debt, paid_refund, paid_discount, 
	companyid, swipmachineid, swipbankid, loan_reference, loan_paid, loanproviderid, isb2b, intmode, 
	intvendor, upiproviderid, paid_upi, tzoffset, recon
            into Zip_billsettlementmaster
            FROM billsettlementmaster limit 0;";
            if (!HasTable("Zip_billsettlementmaster"))
                ExecuteQuery(preparedquery, null);

            string processquery = @"INSERT INTO public.Zip_billsettlementmaster(zippedon,id, isactive, createdby, modifiedby, version, createdon, modifiedon, mode, billamount, 
	paymentinfo, customerid, paidamount, code, settlementon, discount, receivable, tax, 
	billmastercode, creditapprovedby, creditreason, cashierid, counterid, billadditionaldiscount, 
	locationid, paid_cash, paid_card, paid_credit, paid_voucher, paid_debt, paid_refund, paid_discount, 
	companyid, swipmachineid, swipbankid, loan_reference, loan_paid, loanproviderid, isb2b, intmode, 
	intvendor, upiproviderid, paid_upi, tzoffset, recon)

            select current_date as zippedon,id, isactive, createdby, modifiedby, version, createdon, modifiedon, mode, billamount, 
	paymentinfo, customerid, paidamount, code, settlementon, discount, receivable, tax, 
	billmastercode, creditapprovedby, creditreason, cashierid, counterid, billadditionaldiscount, 
	locationid, paid_cash, paid_card, paid_credit, paid_voucher, paid_debt, paid_refund, paid_discount, 
	companyid, swipmachineid, swipbankid, loan_reference, loan_paid, loanproviderid, isb2b, intmode, 
	intvendor, upiproviderid, paid_upi, tzoffset, recon
            from billsettlementmaster
            where billsettlementmaster.settlementon<@fromdate";
            ExecuteQuery(processquery, param, 1000000);
        }

        public void BillSettlement(DateTime fromdate)
        {
            var param = new SortedList<string, object>();
            param.Add("@fromdate", fromdate);
            string preparedquery = @"select current_date as zippedon,id, isactive, createdby, modifiedby, version, createdon, modifiedon, receiptdate,
    receiptno, paid, receivable, billno, companyid, billamount, discount, discountapprovedby, mode, billsettlementmasterid,
	customerid, billid, debtpayment, iscreditbill, swipingmachineid, additionaldiscount, discountreason, 
	billadditionaldiscount, locationid, employeesalaryscheduleid
            into Zip_billsettlement
            FROM billsettlement limit 0;";
            if (!HasTable("Zip_billsettlement"))
                ExecuteQuery(preparedquery, null);

            string processquery = @"INSERT INTO public.Zip_billsettlement(zippedon,id, isactive, createdby, modifiedby, version, createdon, modifiedon, receiptdate,
    receiptno, paid, receivable, billno, companyid, billamount, discount, discountapprovedby, mode, billsettlementmasterid,
	customerid, billid, debtpayment, iscreditbill, swipingmachineid, additionaldiscount, discountreason, 
	billadditionaldiscount, locationid, employeesalaryscheduleid)

            select current_date as zippedon,id, isactive, createdby, modifiedby, version, createdon, modifiedon, receiptdate,
    receiptno, paid, receivable, billno, companyid, billamount, discount, discountapprovedby, mode, billsettlementmasterid,
	customerid, billid, debtpayment, iscreditbill, swipingmachineid, additionaldiscount, discountreason, 
	billadditionaldiscount, locationid, employeesalaryscheduleid
            from billsettlement
            where billsettlement.receiptdate<@fromdate";
            ExecuteQuery(processquery, param, 1000000);
        }
        public void BillItemTable(DateTime fromdate)
        {
            var param = new SortedList<string, object>();
            param.Add("@fromdate", fromdate);
            string preparedquery = @"select current_date as zippedon, id, isactive, createdby, modifiedby, version, createdon, modifiedon, barcode, stockid, 
printingname, price, discount, receivable, salesmanid, tax, taxid, companyid, billid, qty, rate, 
salablegoodsid, additionaldiscount, locationid, orderid, piece, journalqty, itemtax, salerate, 
saleamount, taxpercentage, hsncode, discountpercentage, additionaldiscountpercentage, schemediscount, 
schemereward, schemediscountcoupen, schemeid, incentive, attributes, imageid, sellingmode, productid, 
isexchange, etprice, etdiscount, returnbillitemid
            into Zip_billitems
            FROM billitems limit 0;";
            if (!HasTable("Zip_billitems"))
                ExecuteQuery(preparedquery, null);

            string processquery = @"INSERT INTO public.Zip_billitems(zippedon,id, isactive, createdby, modifiedby, version, createdon, modifiedon, barcode, stockid, 
printingname, price, discount, receivable, salesmanid, tax, taxid, companyid, billid, qty, rate, 
salablegoodsid, additionaldiscount, locationid, orderid, piece, journalqty, itemtax, salerate, 
saleamount, taxpercentage, hsncode, discountpercentage, additionaldiscountpercentage, schemediscount, 
schemereward, schemediscountcoupen, schemeid, incentive, attributes, imageid, sellingmode, productid, 
isexchange, etprice, etdiscount, returnbillitemid)

            select current_date as zippedonid, billitems.id, billitems.isactive, billitems.createdby, billitems.modifiedby, 
billitems.version, billitems.createdon, billitems.modifiedon, barcode, stockid, 
printingname, price, billitems.discount, billitems.receivable, salesmanid, 
billitems.tax, taxid, billitems.companyid, billid, billitems.qty, rate, 
salablegoodsid, billitems.additionaldiscount, billitems.locationid, orderid, piece, journalqty, itemtax, salerate, 
saleamount, taxpercentage, hsncode, discountpercentage, additionaldiscountpercentage, schemediscount, 
schemereward, schemediscountcoupen, schemeid, incentive, attributes, imageid, sellingmode, productid, 
isexchange, etprice, etdiscount, returnbillitemid
	FROM bill
	inner join billitems on billitems.billid = bill.id
    where bill.billdate<@fromdate";
            ExecuteQuery(processquery,param, 1000000);
        }
    }
}