using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
using WebAPI.Data;
using Newtonsoft.Json;
namespace InfyPOS.Data
{
    public class SalablegoodsDataService : SAASData
    {
        const string SQL_Search = @"select salablegoods.id, salablegoods.isactive, salablegoods.createdby, salablegoods.modifiedby,salablegoods.companyid, salablegoods.version, 
            salablegoods.createdon, salablegoods.modifiedon, iqty, oqty, dqty, jqty, aqty, tiqty,toqty, stockid, salablegoods.barcode, items.printingname as sellingname,
            costprice,icostprice,items.printingname itemname,stock.serialno,
            salablegoods.locationid, bundleid, salablegoods.itemid,received,
			salablegoods.price,salablegoods.discount,salablegoods.additionaldiscount,
			salablegoods.supplierid,ispurchaselocation,pqty,producttype,products.salestaxid from salablegoods
            inner join stock on stock.id = salablegoods.stockid
            left outer join items on items.id = salablegoods.itemid
            left outer join products on products.id = items.productid
            where salablegoods.id=@id";

        const string SQL_BundleStock = @"select salablegoods.*, items.printingname as sellingname,producttype,products.salestaxid from bundles
                                        LEFT JOIN LATERAL
                                        json_array_elements (case when bundles.items is null or bundles.items::text ='null' then '[]'::json else bundles.items :: json end) schemelist ON true
                                        inner salablegoods stock ON (schemelist ->> 'id') :: BIGINT = stock.id
                                        left outer join items on items.id = salablegoods.itemid
                                        left outer join products on products.id = items.productid
                                        where bundles.id=@bundleid and bundles.isactive=true)";

        public bool ChangeStock()
        {

            string syncoqty = @"
with allsales
as
(
select salablegoodsid,sum(billitems.qty) qty from billitems
inner join bill on bill.id = billid
where billtype != 9
group by salablegoodsid
),changes as
(
select salablegoods.id,salablegoods.barcode,salablegoods.oqty,dqty,jqty,allsales.qty noqty from salablegoods
inner join allsales on allsales.salablegoodsid = salablegoods.id
where salablegoods.oqty != allsales.qty and jqty = 0 and dqty=0
)
update salablegoods set oqty=noqty from changes
where changes.id = salablegoods.id";

            string query = @"

update salablegoods set jqty = salablegoods.iqty,oqty=0 from stock
inner join lrinvoiceitems on lrinvoiceitems.id = stock.invoiceitemid
inner join lrinvoice on lrinvoice.id = lrinvoiceid
inner join lrentry on lrentry.id = lrid
inner join items on items.id = stock.itemid
inner join brand on brand.id = brandid
inner join referencelist size on size.id = sizeid
where stock.id = stockid and salablegoods.locationid != 1 and lrentry.lrentrydate<'2020-02-01';

with allstock
as
(
select salablegoods.id,salablegoods.iqty stock,salablegoods.barcode,brand.name brandname,stock.designid,size.name sizename,referencelist.name locationname from salablegoods
inner join stock on stock.id = stockid
inner join lrinvoiceitems on lrinvoiceitems.id = stock.invoiceitemid
inner join lrinvoice on lrinvoice.id = lrinvoiceid
inner join lrentry on lrentry.id = lrid
inner join items on items.id = stock.itemid
inner join brand on brand.id = brandid
inner join referencelist size on size.id = sizeid
inner join referencelist on referencelist.id = salablegoods.locationid
where salablegoods.locationid != 1 and lrentry.lrentrydate<'2020-02-01'
)

select b2bstock.qty newstock,allstock.barcode,allstock.id,allstock.stock from b2bstock
left outer join allstock on allstock.locationname = b2bstock.location
and  allstock.designid = b2bstock.design
and  allstock.brandname = b2bstock.brand
and  allstock.sizename = b2bstock.size;";


            //WHERE b2bstock.location = 'ST' and b2bstock.brand = 'BINDASS'

            List<ChangeStockItem> changeStockItems = new List<ChangeStockItem>();
            using (var reader = ExecuteReader(query, new SortedList<string, object>()))
            {
                while (reader.Read())
                {
                    if (reader["id"] != DBNull.Value)
                        changeStockItems.Add(new ChangeStockItem()
                        {
                            barcode = reader["barcode"].ToString(),
                            id = Convert.ToInt64(reader["id"]),
                            qty = Convert.ToDecimal(reader["newstock"]),
                            systemstock = Convert.ToDecimal(reader["stock"])
                        });
                }
            }
            foreach (var item in changeStockItems.GroupBy(e => e.barcode))
            {
                var systemstock = item.Sum(e => e.systemstock);
                var physicalstock = item.First().qty;
                foreach (var sl in item.OrderByDescending(e => e.systemstock).ToList())
                {
                    if (sl.systemstock > physicalstock)
                    {
                        var damaged = physicalstock;
                        ExecuteQuery("update salablegoods set oqty=0, jqty=jqty-" + damaged.ToString("F0") + " where id=" + sl.id, null);
                        physicalstock = 0;
                    }
                    else
                    {
                        var journal = sl.systemstock;
                        ExecuteQuery("update salablegoods set oqty=0, jqty=jqty-" + journal.ToString("F0") + " where id=" + sl.id, null);
                        physicalstock -= journal;
                        if (physicalstock <= 0)
                            break;
                    }
                }

            }
            return true;
        }

        private class ChangeStockItem
        {
            public decimal qty { get; set; }
            public string barcode { get; set; }
            public long id { get; set; }
            public decimal systemstock { get; set; }
        }
        const string SQL_Search4Refill = @"select stock.productid, stock.printingname,salablegoods.id,invoiceitemid,
            salablegoods.iqty-(salablegoods.oqty+salablegoods.jqty+salablegoods.dqty+salablegoods.aqty) stock,stock.barcode,refillbox.id as refillboxid
            from stock 
            left outer join items on items.id = stock.itemid
            inner join salablegoods on salablegoods.stockid = stock.id
            left outer join refillbox on refillbox.companyid = stock.companyid and
            refillbox.productid = coalesce(items.productid,refillbox.productid)
            and refillbox.productid = coalesce(items.productid,refillbox.productid)
            and refillbox.brandid = coalesce(items.brandid,refillbox.brandid)
            and refillbox.sizeid = coalesce(items.sizeid,refillbox.sizeid)
            and refillbox.materialid = coalesce(items.sizeid,refillbox.materialid)
            where salablegoods.iqty-(salablegoods.oqty+salablegoods.jqty+salablegoods.dqty+salablegoods.aqty)>0
            and stock.isactive=true";

        const string SQL_Search4RefillPreferred = @"select refillbox.id,items.id as itemid,
            boxno,floor.name as floorname,section.name as sectionname,boxtypes.name as boxtypename from refillbox
            left outer join items on items.productid = coalesce(refillbox.productid,items.productid)
            and refillbox.brandid = coalesce(refillbox.brandid,items.brandid)
            and refillbox.sizeid = coalesce(refillbox.sizeid,items.sizeid)
            and refillbox.materialid = coalesce(refillbox.materialid,items.materialid)
            inner join referencelist floor on floor.id = refillbox.floorid
            inner join referencelist section on section.id = refillbox.sectionid
            inner join referencelist boxtypes on boxtypes.id = refillbox.boxtype
            where items.id in ({0})";


        const string SQL_Productsearch = @"select stock.designid,salablegoods.id, salablegoods.companyid, iqty, oqty, dqty, jqty, stockid, salablegoods.barcode,
            case when COALESCE(producttype,0)<=1 then products.name else stock.printingname end as sellingname, stock.imageid,stock.serialno,stock.runtimeprice,
            salablegoods.itemid,received,salablegoods.price,stock.buyingprice buyingprice,salablegoods.discount,salablegoods.additionaldiscount,stock.productid,salestaxid,stock.hsncode,stock.isbit,stock.qty, products.sellingmode from salablegoods
            LEFT OUTER join items on items.id = salablegoods.itemid
            inner join stock on stock.id = salablegoods.stockid
            inner JOIN products on products.id = stock.productid
            where salablegoods.locationid=@locationid and salablegoods.isactive=true 
            and stock.productid=@productid and iqty>(oqty+jqty+dqty+aqty)";

        const string SQL_brandWsPrice = @"with brandwisedetails as
                                        (
                                            select cast(billitems.attributes->>'margin' as decimal) billmargin,billno,billdate,brand.name brandname,billitems.qty,billitems.receivable,buyingprice,
                                                   (billitems.discountpercentage+billitems.additionaldiscountpercentage) discountpercent,
	                                               row_number() OVER (PARTITION BY brandid ORDER BY billitems.id DESC) as ranknumber,stock.price stockprice,(billitems.price-billitems.itemtax) billprice from bill
                                            inner join billitems on billitems.billid = bill.id and billitems.isactive=true
                                            inner join stock on stock.id = billitems.stockid
                                            inner join items on items.id = stock.itemid
                                            inner join brand on brand.id = items.brandid
                                            where customerid = @customerid and bill.locationid=@locationid
                                        )

                                        select * from brandwisedetails
                                        where ranknumber=1";

        const string SQL_Batchsearch = @"select stock.designid,salablegoods.id, salablegoods.companyid, iqty, oqty, dqty,jqty, aqty, stockid, salablegoods.barcode,
            case when COALESCE(producttype,0)<=1 then products.name else stock.printingname end as sellingname, stock.imageid,stock.serialno,stock.runtimeprice,
            salablegoods.itemid,received,salablegoods.price,stock.buyingprice buyingprice,salablegoods.discount,salablegoods.additionaldiscount,stock.productid,
            salestaxid,stock.hsncode,stock.isbit,stock.qty, products.sellingmode,items.printingname itemname from salablegoods
            LEFT OUTER join items on items.id = salablegoods.itemid
            inner join stock on stock.id = salablegoods.stockid and stock.designid != ''
            inner JOIN products on products.id = stock.productid
            where salablegoods.locationid=@locationid and salablegoods.isactive=true 
            and stock.supplierid=@supplierid and stock.productid=@productid and iqty>(oqty+dqty)";

        const string SQL_Batchsearchbybrandid = @"select stock.designid,salablegoods.id, salablegoods.companyid, iqty, oqty, dqty,jqty, aqty, stockid, salablegoods.barcode,sizelist.name sizename,colourlist.name colourname,
            case when COALESCE(producttype,0)<=1 then products.name else stock.printingname end as sellingname, stock.imageid,stock.serialno,stock.runtimeprice,
            salablegoods.itemid,received,salablegoods.price,COALESCE(basebuyingprice,stock.buyingprice) buyingprice,salablegoods.discount,salablegoods.additionaldiscount,stock.productid,salestaxid,
            stock.hsncode,stock.isbit,stock.qty, products.sellingmode,lrinvoiceitems.purchasediscountpercentage,brand.name brandname,items.printingname itemname from salablegoods
            inner join stock on stock.id = salablegoods.stockid
            LEFT OUTER join items on items.id = stock.itemid
            LEFT OUTER join referencelist sizelist on sizelist.id = items.sizeid
            LEFT OUTER join brand on brand.id = items.brandid
			LEFT OUTER join referencelist colourlist on colourlist.id = items.colourid
            left outer join lrinvoiceitems on lrinvoiceitems.id = stock.invoiceitemid
            inner JOIN products on products.id = stock.productid
            where salablegoods.locationid=@locationid and salablegoods.isactive=true 
            and items.brandid=@brandid and iqty>(oqty+dqty)";

        const string SQL_Batchsoldbybrandid = @"select bill.billno,bill.billdate::date, stock.designid,salablegoods.id, salablegoods.companyid, billitems.qty iqty, 0 as oqty,
            0 as dqty,0 as jqty, salablegoods.stockid, salablegoods.barcode,sizelist.name sizename,colourlist.name colourname,brand.name brandname,
            case when COALESCE(producttype,0)<=1 then products.name else stock.printingname end as sellingname, stock.imageid,stock.serialno,stock.runtimeprice,
            salablegoods.itemid,received,billitems.price-billitems.itemtax price,billitems.price-billitems.itemtax buyingprice,0 discount,0 additionaldiscount,stock.productid,salestaxid,
            stock.hsncode,stock.isbit,stock.qty, products.sellingmode,lrinvoiceitems.purchasediscountpercentage,items.printingname itemname from billitems
			inner join bill on bill.id = billid
			inner join salablegoods on salablegoods.id = billitems.salablegoodsid
            inner join stock on stock.id = salablegoods.stockid
            LEFT OUTER join items on items.id = stock.itemid
            LEFT OUTER join referencelist sizelist on sizelist.id = items.sizeid
            LEFT OUTER join brand on brand.id = items.brandid
			LEFT OUTER join referencelist colourlist on colourlist.id = items.colourid
            left outer join lrinvoiceitems on lrinvoiceitems.id = stock.invoiceitemid
            inner JOIN products on products.id = stock.productid
            where bill.locationid = @locationid and bill.customerid = @customerid 
            and bill.billtype != 8 and bill.billtype != 9 and items.brandid=@brandid 
            and bill.billdate>=@sixmonthdate and billitems.qty>0
            order by bill.id";

        //case when COALESCE(lrinvoiceitems.dealerprice,0)<=0 then stock.dealerprice::numeric else lrinvoiceitems.dealerprice::numeric end as dealerprice,
        const string SQL_Barcodesearch = @"
            select salablegoods.id, salablegoods.companyid, iqty, oqty, dqty, jqty, aqty, stockid, salablegoods.barcode,COALESCE(stock.displaymrp,lrinvoiceitems.mrp) mrp,
            stock.dealerprice,items.printingname itemname,lrinvoiceitems.basebuyingprice,
            case when COALESCE(producttype,0)<=1 then products.name else stock.printingname end as sellingname, stock.imageid,stock.serialno,stock.runtimeprice,
            salablegoods.itemid,received,salablegoods.price,stock.buyingprice buyingprice,stock.ibuyingprice ibuyingprice,salablegoods.discount,items.sizeid,
            salablegoods.additionaldiscount,stock.productid,salestaxid,stock.hsncode,stock.isbit,stock.qty, products.sellingmode, stock.designid, brand.name brandname,
            style.name stylename,pattern.name patternname,size.name sizename, colour.name colourname,type.name typename,material.name materialname,items.colourid,
            cast(attributes->>'batchandexpiry' as bool) hasbatch,salablegoods.jqty,tax_intermediate_price,tax_intermediate_dealerprice,stock.ismultiprice
            from salablegoods
            inner join stock on stock.id = salablegoods.stockid
			left outer join lrinvoiceitems on lrinvoiceitems.id = stock.invoiceitemid and lrinvoiceitems.isactive = true
            inner JOIN products on products.id = stock.productid 
            LEFT OUTER join items on items.id = stock.itemid and items.isactive = true
			left outer join referencelist colour on colour.id = colourid
                                    left outer join referencelist size on size.id = sizeid
                                    left outer join referencelist style on style.id = styleid
                                    left outer join referencelist pattern on pattern.id = patternid
                                    left outer join referencelist material on material.id = materialid
                                    left outer join referencelist type on type.id = typeid
                                    left outer join brand on brand.id = items.brandid
            where salablegoods.barcode=@barcode and salablegoods.locationid=@locationid and salablegoods.isactive=true
            order by salablegoods.stockid";

        const string SQL_Barcodesearch_brandproductprice = @"
            select salablegoods.id, salablegoods.companyid, iqty, oqty, dqty, jqty, aqty, stockid, salablegoods.barcode,COALESCE(stock.displaymrp,lrinvoiceitems.mrp) mrp,
            case when COALESCE(lrinvoiceitems.dealerprice,0)<=0 then stock.dealerprice::numeric else lrinvoiceitems.dealerprice::numeric end as dealerprice,
            case when COALESCE(producttype,0)<=1 then products.name else stock.printingname end as sellingname, stock.imageid,stock.serialno,stock.runtimeprice,
            salablegoods.itemid,received,salablegoods.price,stock.buyingprice buyingprice,stock.ibuyingprice ibuyingprice,salablegoods.discount,items.sizeid,
            salablegoods.additionaldiscount,stock.productid,salestaxid,stock.hsncode,stock.isbit,stock.qty, products.sellingmode, stock.designid, brand.name brandname,
            style.name stylename,pattern.name patternname,size.name sizename, colour.name colourname,type.name typename,material.name materialname,items.colourid,
            cast(attributes->>'batchandexpiry' as bool) hasbatch,salablegoods.jqty,tax_intermediate_price,tax_intermediate_dealerprice,stock.ismultiprice,
            brandmargin->>'margin' brandproductmargin,branddefaultmargin->>'margin' branddefaultmargin,items.printingname itemname
            from salablegoods
            inner join stock on stock.id = salablegoods.stockid
			left outer join lrinvoiceitems on lrinvoiceitems.id = stock.invoiceitemid and lrinvoiceitems.isactive = true
            inner JOIN products on products.id = stock.productid 
            LEFT OUTER join items on items.id = stock.itemid and items.isactive = true
			 left outer join referencelist colour on colour.id = colourid
                                    left outer join referencelist size on size.id = sizeid
                                    left outer join referencelist style on style.id = styleid
                                    left outer join referencelist pattern on pattern.id = patternid
                                    left outer join referencelist material on material.id = materialid
                                    left outer join referencelist type on type.id = typeid
                                    left outer join brand on brand.id = items.brandid
                                    LEFT JOIN LATERAL json_array_elements 
                                                    (case when brand.marginlist is null or brand.marginlist::text ='null' then '[]'::json 
															  else brand.marginlist :: json end) brandmargin ON 
									cast(brandmargin->>'productid' as bigint) = stock.productid 
									LEFT JOIN LATERAL json_array_elements 
                                                    (case when brand.marginlist is null or brand.marginlist::text ='null' then '[]'::json 
															  else brand.marginlist :: json end) branddefaultmargin ON 
									cast(branddefaultmargin->>'productid' as bigint) = -1 
            where salablegoods.barcode=@barcode and salablegoods.locationid=@locationid and salablegoods.isactive=true
            order by salablegoods.stockid";

        const string SQL_SerialSearch = @"select salablegoods.id, salablegoods.companyid, iqty, oqty, dqty, jqty, stockid, salablegoods.barcode,
            case when COALESCE(producttype,0)<=1 then products.name else stock.printingname end as sellingname, stock.imageid,stock.serialno,stock.runtimeprice,
            salablegoods.itemid,received,salablegoods.price,stock.buyingprice buyingprice,stock.ibuyingprice ibuyingprice,salablegoods.discount,salablegoods.additionaldiscount,
            stock.productid,salestaxid,stock.hsncode,stock.isbit,stock.qty, products.sellingmode,items.printingname itemname from salablegoods
            LEFT OUTER join items on items.id = salablegoods.itemid
            inner join stock on stock.id = salablegoods.stockid      
            inner JOIN products on products.id = stock.productid
            where stock.serialno=@barcode and salablegoods.locationid=@locationid and salablegoods.isactive=true order by stock.id";

        const string SQL_SearchAll = @"select salablegoods.id, salablegoods.isactive, salablegoods.createdby, salablegoods.modifiedby,companyid, salablegoods.version, 
            salablegoods.createdon, salablegoods.modifiedon, iqty, oqty, dqty, stockid, barcode, items.printingname  as sellingname,items.printingname itemname,
            locationid, bundleid, itemid,received,ispurchaselocation,pqty from salablegoods
            left outer join items on items.id = salablegoods.itemid
            where bundleid=@bundleid";

        const string SQL_Insert = @"INSERT INTO public.salablegoods(isactive, createdby, modifiedby, version, createdon, modifiedon, iqty, oqty, dqty, stockid, 
            barcode, locationid, bundleid, itemid,companyid,received,supplierid,refillboxid,inwardon,price,discount,additionaldiscount,ispurchaselocation,pqty,tiqty,toqty,rqty,salereturnqty,costprice,icostprice) 
            VALUES(@isactive, @createdby, @modifiedby, @version, @createdon, @modifiedon, @iqty, @oqty, @dqty, @stockid, @barcode, 
            @locationid, @bundleid, @itemid,@companyid,@received,@supplierid,0,@inwardon,@price,@discount,@additionaldiscount,@ispurchaselocation,@pqty,@tiqty,@toqty,@rqty,@salereturnqty,@costprice,@icostprice) RETURNING id";

        const string SQL_UpdateTransferStock = @"UPDATE public.stock SET modifiedby=@modifiedby, version=version+1, modifiedon=@modifiedon, 
                                                        transfered=transfered+@transfered,transferedout=COALESCE(transferedout,0)+@transfered where id=@id RETURNING qty,returnqty,damagedqty,transfered,journalqty";

        const string SQL_ReversTransferStock = @"UPDATE public.stock SET transfered=transfered+@transfered,transferedout=COALESCE(transferedout,0)+@transfered,version=COALESCE(version,0::int)+1 where id=@id;
                                                 UPDATE public.salablegoods set iqty=iqty+@transfered,version=COALESCE(version,0::int)+1 where id=@salablegoodsid;";

        const string SQL_UpdateInwards = @"update salablegoods set received=true where bundleid=@bundleid";

        const string SQL_UpdateInwardItem = @"update salablegoods set received=true,iqty=@iqty,inwardon=@inwardon where id=@id";

        const string SQL_UpdateRefillID = @"update salablegoods set refillboxid=@refillboxid where id=@id";

        const string SQL_Update = @"UPDATE public.salablegoods SET modifiedby=@modifiedby, version=@version+1, modifiedon=@modifiedon,received=@received, iqty=@iqty,costprice=@costprice,icostprice=@icostprice,
                                        oqty=@oqty, dqty=@dqty, stockid=@stockid, barcode=@barcode, locationid=@locationid, bundleid=@bundleid, itemid=@itemid, price=@price,discount=@discount ,
                                        tiqty=@tiqty,toqty=@toqty where id=@id and version=@version RETURNING version";

        const string SQL_Delete = @"update public.salablegoods set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version  RETURNING version";

        const string SQL_Inward2WarehouseReduce = @"Update public.salablegoods set iqty=iqty-@qty,toqty=COALESCE(toqty,0)+@qty,version=COALESCE(version,0::int)+1 where id=@salablegoodsid RETURNING iqty,barcode";

        const string SQL_Inward2WarehouseIncrease = @"Update public.stock set transfered=transfered-@qty,transferedin=COALESCE(transferedin,0)+@qty,version=COALESCE(version,0::int)+1 where id=@stockid";

        const string SQL_SwapStockQTYReduce = @"Update salablegoods set iqty=salablegoods.iqty-@qty,toqty=COALESCE(toqty,0)+@qty,
                                                version=COALESCE(salablegoods.version,0::int)+1 from stock
                                                where salablegoods.id=@fromid
                                                and stock.id = salablegoods.stockid
                                                RETURNING salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty+salablegoods.aqty),
                                                salablegoods.barcode,stock.serialno";

        const string SQL_SwapStockQTYAlter = @"Update public.salablegoods set iqty=iqty+@qty,toqty=COALESCE(toqty,0)-@qty,version=COALESCE(version,0::int)+1 where id=@fromid RETURNING iqty-(oqty+dqty+jqty+aqty),barcode";

        const string SQL_SwapStockQTYIncrease = @"Update public.salablegoods set iqty=iqty+@qty,tiqty=COALESCE(tiqty,0)+@qty,version=COALESCE(version,0::int)+1 where id=@toid RETURNING iqty-(oqty+dqty+jqty+aqty),barcode";

        const string SQL_SwapStockCompany = @"update public.salablegoods set companyid=@companyid,version=COALESCE(version,0::int)+1 where id=@fromid";
        //const string SQL_CanDeleteAll = @"Select count(*) from salablegoods 
        //                                    where bundleid=@bundleid and oqty>0";

        public SalablegoodsDataService()
        {
        }
        public List<Salablegoods> Search(SalablegoodsCreteria _salablegoods)
        {
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                var result = new List<Salablegoods>();
                if (_salablegoods.barcodes != null && _salablegoods.barcodes.Count > 5000)
                {
                    var barcodes = _salablegoods.barcodes.ChunkBy(5000);
                    foreach (var b in barcodes)
                    {
                        _salablegoods.barcodes = b;
                        result.AddRange(Search(_salablegoods, conn, null));
                    }
                }
                else
                {
                    result.AddRange(Search(_salablegoods, conn, null));
                }
                conn.Close();
                return result;

            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _salablegoods);
                return new List<Salablegoods>();
            }
        }
        public List<Salablegoods> Search(SalablegoodsCreteria _salablegoods, NpgsqlConnection conn, Npgsql.NpgsqlTransaction tran)
        {
            List<Salablegoods> lst = new List<Salablegoods>();

            NpgsqlCommand command = null;
            if (_salablegoods.id != 0)
            {
                if (tran != null)
                    command = GetCommand(SQL_Search, conn, tran);
                else
                    command = GetCommand(SQL_Search, conn);
                command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _salablegoods.id;
            }
            else if (_salablegoods.bundleid > 0)
            {


                if (tran != null)
                    command = GetCommand(SQL_BundleStock, conn, tran);
                else
                    command = GetCommand(SQL_BundleStock, conn);

                command.Parameters.Add("salablegoods.bundleid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _salablegoods.bundleid;
            }
            else
            {
                var valid = false;
                QueryBuilder qb = new QueryBuilder();
                qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                qb.AddParameter("salablegoods.isactive", "isactive", true, "=");

                if (!string.IsNullOrEmpty(_salablegoods.barcode))
                {
                    qb.AddParameter("salablegoods.barcode", _salablegoods.barcode, "=");
                    valid = true;
                }

                if (_salablegoods.stockid > 0)
                {
                    qb.AddParameter("salablegoods.stockid", _salablegoods.stockid, "=");
                    valid = true;
                }
                if (_salablegoods.stockids != null && _salablegoods.stockids.Count > 0)
                {
                    //var barecodes = _salablegoods.ids.ConvertAll(e => "'" + e + "'").ToList();
                    qb.AddInParamer("salablegoods.stockid", string.Join<long>(",", _salablegoods.stockids));
                    valid = true;
                }
                if (_salablegoods.ids != null && _salablegoods.ids.Count > 0)
                {
                    //var barecodes = _salablegoods.ids.ConvertAll(e => "'" + e + "'").ToList();
                    qb.AddInParamer("salablegoods.id", string.Join<long>(",", _salablegoods.ids));
                    valid = true;
                }

                if (_salablegoods.barcodes != null && _salablegoods.barcodes.Count > 0)
                {
                    var barecodes = _salablegoods.barcodes.ConvertAll(e => "'" + WebAPI.Data.Utility.SafeSQL(e) + "'").ToList();
                    qb.AddInParamer("salablegoods.barcode", string.Join(",", barecodes));
                    valid = true;
                    if (_salablegoods.barcodes.Count > 20)
                    {
                        WebAPI.Common.Logger.Instance(Context.OrganizationID).Log("SalablegoodsDataService.Search",
                            WebAPI.Common.Logger.LogType.Sync,
                            Environment.StackTrace, _salablegoods);
                    }
                }
                if (_salablegoods.uanbarcodes != null && _salablegoods.uanbarcodes.Count > 0)
                {
                    var barecodes = _salablegoods.uanbarcodes.ConvertAll(e => "'" + WebAPI.Data.Utility.SafeSQL(e) + "'").ToList();
                    qb.AddInParamer("stock.serialno", string.Join(",", barecodes));
                    valid = true;
                    if (_salablegoods.uanbarcodes.Count > 20)
                    {
                        WebAPI.Common.Logger.Instance(Context.OrganizationID).Log("SalablegoodsDataService.Search",
                            WebAPI.Common.Logger.LogType.Sync,
                            Environment.StackTrace, _salablegoods);
                    }
                }

                if (_salablegoods.locationid != 0)
                    qb.AddParameter("salablegoods.locationid", _salablegoods.locationid, "=");

                if (_salablegoods.companyid != 0)
                    qb.AddParameter("salablegoods.companyid", _salablegoods.companyid, "=");

                if (!valid)
                    qb.Limit = 1000;
                command = tran != null ? qb.GetCommand(conn, tran) : qb.GetCommand(conn); ;
            }

            bool showitemname = (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig != null &&
                InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig.ContainsKey(_salablegoods.locationid) &&
                InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[_salablegoods.locationid].sale_showitemname);

        	using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    Salablegoods osalablegoods = new Salablegoods();
                    osalablegoods.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                    osalablegoods.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                    osalablegoods.received = reader["received"] == DBNull.Value ? false : Convert.ToBoolean(reader["received"]);
                    osalablegoods.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                    osalablegoods.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                    osalablegoods.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"].ToString());
                    osalablegoods.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                    osalablegoods.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                    osalablegoods.iqty = reader["iqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["iqty"].ToString());
                    osalablegoods.oqty = reader["oqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["oqty"].ToString());
                    osalablegoods.dqty = reader["dqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["dqty"].ToString());
                    osalablegoods.jqty = reader["jqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["jqty"].ToString());
                    osalablegoods.pqty = reader["pqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["pqty"].ToString());
                    osalablegoods.aqty = reader["aqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["aqty"].ToString());
                    osalablegoods.tiqty = reader["tiqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["tiqty"].ToString());
                    osalablegoods.toqty = reader["toqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["toqty"].ToString());
                    osalablegoods.stockid = reader["stockid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["stockid"].ToString());
                    osalablegoods.companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"].ToString());
                    osalablegoods.barcode = reader["barcode"] == DBNull.Value ? "" : reader["barcode"].ToString();
                    osalablegoods.locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"].ToString());
                    osalablegoods.bundleid = reader["bundleid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["bundleid"].ToString());
                    osalablegoods.itemid = reader["itemid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["itemid"].ToString());
                    osalablegoods.sellingname = reader["sellingname"] == DBNull.Value ? "unknown" : reader["sellingname"].ToString();

                    if (_salablegoods.uanbarcodes != null && _salablegoods.uanbarcodes.Count > 0)
                    {
                        osalablegoods.serialno = reader["serialno"] == DBNull.Value ? "unknown" : reader["serialno"].ToString();
                    }

                    if(showitemname &&
                        reader["itemname"] != DBNull.Value &&
                        reader["itemname"].ToString() != "" )
                        osalablegoods.sellingname = reader["itemname"].ToString();

                    osalablegoods.supplierid = reader["supplierid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["supplierid"]);
                    osalablegoods.price = reader["price"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["price"].ToString());
                    osalablegoods.discount = reader["discount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discount"].ToString());
                    osalablegoods.additionaldiscount = reader["additionaldiscount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["additionaldiscount"].ToString());
                    osalablegoods.ispurchaselocation = reader["ispurchaselocation"] == DBNull.Value ? false : Convert.ToBoolean(reader["ispurchaselocation"]);
                    osalablegoods.producttype = reader["producttype"] == DBNull.Value ? 0 : Convert.ToInt64(reader["producttype"]);
                    osalablegoods.saletaxid = reader["salestaxid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["salestaxid"]);
                    lst.Add(osalablegoods);
                }
            }
            return lst;
        }

        public bool ChangeRefillID(RefillChange change)
        {
            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = GetCommand(SQL_UpdateRefillID, conn, transaction);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = 0;
                    command.Parameters.Add("refillboxid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = 0;
                    foreach (var item in change.Items)
                    {
                        command.Parameters["id"].Value = item.id;
                        command.Parameters["refillboxid"].Value = change.Dispatch ? item.refillboxid : 0;
                    }
                    transaction.Commit();
                    conn.Close();
                }
                return true;
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, change);


            }
            return false;
        }

        public decimal ComputeMargin(SalesorderDataService.MarginRules rule,
            decimal taxpercentage,
            decimal mintaxpercentage,
            decimal margin,
            decimal mrp)
        {
            switch (rule)
            {
                case SalesorderDataService.MarginRules.Markdown:
                    return mrp - (mrp * margin / 100);
                case SalesorderDataService.MarginRules.Markdown_NetOff:
                    var marginvalue = (mrp * margin / 100);
                    var taxvalue = (mrp * (taxpercentage / 100)) / (1 + (taxpercentage / 100));
                    return mrp - marginvalue - taxvalue;
                case SalesorderDataService.MarginRules.Markdown_ExcludeTax:
                    var rrp = mrp / (1 + (taxpercentage / 100));
                    var exmarginvalue = rrp * margin / 100;
                    return rrp - exmarginvalue;
                case SalesorderDataService.MarginRules.Markdown_ExtraMargin:
                    if (taxpercentage > mintaxpercentage)
                    {
                        margin += (taxpercentage - mintaxpercentage);
                    }
                    return mrp - (mrp * margin / 100);
                //TONIQ
                case SalesorderDataService.MarginRules.Markdown_IncludeTax:
                    var toniqrrp = mrp / (1 + (taxpercentage / 100));
                    var wholemargin = mrp * margin / 100;
                    var excludeprice = toniqrrp - wholemargin;
                    var includeprice = excludeprice + (excludeprice * taxpercentage / 100);
                    return includeprice;
            }
            return 0;
        }
        internal void UpdateBarcodePrices(SalesorderDataService.SearchBarcodeData item, List<Barcodedgoods> list)
        {
            if (item.ccmarginmrp > 0)
            {
                SalesorderDataService.MarginRules marginRules = (SalesorderDataService.MarginRules)item.cccomputemode;
                TaxDataService taxDataService = new TaxDataService() { Context = Context };
                var taxlist = taxDataService.TaxProducts(list.ConvertAll(x => x.productid).Distinct().ToList());
                foreach (var sitem in list)
                {
                    if (taxlist.ContainsKey(sitem.productid))
                    {
                        var taxid = taxlist[sitem.productid].id;
                        var taxpercentage = taxlist[sitem.productid].taxsplit.Find(sitem.mrp).taxpercent;
                        var tax = Math.Round((sitem.mrp * 1) * taxpercentage / 100, 2);
                        var mintaxrange = taxlist[sitem.productid].taxsplit.items.OrderBy(x => x.sgst + x.cgst).First();
                        var price = Math.Round(ComputeMargin(marginRules, taxpercentage,
                            mintaxrange.cgst + mintaxrange.sgst,
                            item.ccmarginmrp, sitem.mrp), 2);
                        tax = Math.Round((price * 1) * taxpercentage / 100, 2);
                        sitem.price = price + tax;
                        sitem.priceextax = price;
                        sitem.dealerprice = price + tax;
                        sitem.dealerpriceextax = price;
                        sitem.wsrate = price + tax;
                        sitem.wsrateextax = price;
                    }
                }
            }
        }

        internal void UpdateInwardQty(NpgsqlTransaction transaction, NpgsqlConnection conn, Salablegoods salablegoods)
        {
            var command = GetCommand("Update Salablegoods set iqty=@iqty, pqty=@pqty where id=@id", conn, transaction);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = salablegoods.id;
            command.Parameters.Add("iqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = salablegoods.iqty;
            command.Parameters.Add("pqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = salablegoods.pqty;
            command.ExecuteNonQuery();
        }

        public List<RefillResult> Search4Refill(RefilCreteria request)
        {
            List<RefillResult> lst = new List<RefillResult>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                NpgsqlCommand command = null;
                QueryBuilder qb = new QueryBuilder();
                qb.CurrentQuery = SQL_Search4Refill;
                if (!string.IsNullOrEmpty(request.barcode))
                    qb.AddParameter("stock.barcode", "barcode", request.barcode, "=");
                else
                    qb.AddParameter("stock.companyid", "companyid", request.companyid, "=");
                if (!string.IsNullOrEmpty(request.printingname))
                    qb.AddParameter("item.searchkeys", "searchkeys", request.SearchKeywords2TS(), " @@ ", " to_tsquery('" + WebAPI.Data.Utility.SafeSQL(request.SearchKeywords2TS()) + "') ");
                if (request.productid > 0)
                    qb.AddParameter("items.productid", "productid", request.productid, "=");
                if (request.brandid > 0)
                    qb.AddParameter("items.brandid", "brandid", request.brandid, "=");
                if (request.materialid > 0)
                    qb.AddParameter("items.materialid", "materialid", request.materialid, "=");
                if (request.sizeid > 0)
                    qb.AddParameter("items.sizeid", "sizeid", request.sizeid, "=");
                if (request.supplierid > 0)
                    qb.AddParameter("stock.supplierid", "supplierid", request.supplierid, "=");

                command = qb.GetCommand(conn);

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        RefillResult oRefillResult = new RefillResult();
                        oRefillResult.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        if (!lst.Exists(e => e.id == oRefillResult.id))
                        {
                            oRefillResult.productid = reader["productid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productid"]);
                            oRefillResult.stock = reader["stock"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["stock"].ToString());
                            oRefillResult.invoiceitemid = reader["invoiceitemid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["invoiceitemid"].ToString());
                            oRefillResult.refillboxid = reader["refillboxid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["refillboxid"].ToString());
                            oRefillResult.printingname = reader["printingname"] == DBNull.Value ? "" : reader["printingname"].ToString();
                            oRefillResult.invoiceitemid = reader["invoiceitemid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["invoiceitemid"].ToString());
                            oRefillResult.barcode = reader["barcode"] == DBNull.Value ? "" : reader["barcode"].ToString();
                            lst.Add(oRefillResult);
                        }
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, request);
            }
            return lst;
        }


        internal BarcodedgoodDetail UANSearch(string locationid, string uan, bool includenegative)
        {
            BarcodedgoodDetail barcodedgoods = new BarcodedgoodDetail();
            NpgsqlConnection conn = CreateConnection();
            long locid = 0;
            string actualbarcode = uan;
            try
            {
                locid = long.Parse(locationid);
                if (locid <= 0) return new BarcodedgoodDetail()
                {
                    errormessage = "Please provide location"
                };

                bool showitemname = (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig != null &&
    InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig.ContainsKey(locid) &&
    InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[locid].sale_showitemname);

                bool costincludepd = InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_marginoncost;

                conn.Open();
                NpgsqlCommand command = null;
                int type = 0;
                while (type <= 1)
                {
                    string batchquery = SQL_Barcodesearch;

                    if (type == 0)
                        batchquery = batchquery.Replace("salablegoods.barcode=@barcode", "stock.serialno=@barcode and salablegoods.hasstock = true");
                    else if (type == 1)
                        batchquery = batchquery.Replace("salablegoods.barcode=@barcode", "stock.serialno=@barcode");
                    batchquery += " limit 1";

                    command = GetCommand(batchquery, conn);
                    command.Parameters.Add("barcode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = uan;
                    command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = locid;

                    using (System.Data.IDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            barcodedgoods.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                            barcodedgoods.received = reader["received"] == DBNull.Value ? false : Convert.ToBoolean(reader["received"]);
                            barcodedgoods.iqty = reader["iqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["iqty"].ToString());
                            barcodedgoods.oqty = reader["oqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["oqty"].ToString());
                            barcodedgoods.dqty = reader["dqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["dqty"].ToString());
                            barcodedgoods.jqty = reader["jqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["jqty"].ToString());
                            barcodedgoods.aqty = reader["aqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["aqty"].ToString());
                            barcodedgoods.price = reader["price"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["price"].ToString());
                            barcodedgoods.buyingprice = reader["buyingprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["buyingprice"].ToString());

                            if (costincludepd && reader["basebuyingprice"] != DBNull.Value)
                            {
                                barcodedgoods.buyingprice = reader["basebuyingprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["basebuyingprice"].ToString());
                            }

                            barcodedgoods.mrp = reader["mrp"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["mrp"].ToString());
                            barcodedgoods.dealerprice = reader["dealerprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["dealerprice"].ToString());
                            barcodedgoods.discount = reader["discount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discount"].ToString());
                            barcodedgoods.additionaldiscount = reader["additionaldiscount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["additionaldiscount"].ToString());
                            barcodedgoods.stockid = reader["stockid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["stockid"].ToString());
                            barcodedgoods.companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"].ToString());
                            barcodedgoods.barcode = reader["barcode"] == DBNull.Value ? "" : reader["barcode"].ToString();
                            barcodedgoods.itemid = reader["itemid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["itemid"].ToString());
                            barcodedgoods.taxid = reader["salestaxid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["salestaxid"].ToString());
                            barcodedgoods.sellingname = reader["sellingname"] == DBNull.Value ? "unknown" : reader["sellingname"].ToString();

                            if (showitemname &&
                      reader["itemname"] != DBNull.Value &&
                      reader["itemname"].ToString() != "")
                                barcodedgoods.sellingname = reader["itemname"].ToString();

                            barcodedgoods.serialno = reader["serialno"] == DBNull.Value ? "" : reader["serialno"].ToString();
                            barcodedgoods.imageid = reader["imageid"] == DBNull.Value ? null : reader["imageid"].ToString();
                            barcodedgoods.hsncode = reader["hsncode"] == DBNull.Value ? "" : reader["hsncode"].ToString();
                            barcodedgoods.productid = reader["productid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productid"].ToString());
                            barcodedgoods.stockisbit = reader["isbit"] == DBNull.Value ? false : Convert.ToBoolean(reader["isbit"]);
                            barcodedgoods.runtimeprice = reader["runtimeprice"] == DBNull.Value ? false : Convert.ToBoolean(reader["runtimeprice"]);
                            barcodedgoods.stockqty = reader["qty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["qty"].ToString());
                            barcodedgoods.sellingmode = reader["sellingmode"] == DBNull.Value ? 0 : Convert.ToInt32(reader["sellingmode"]);
                            barcodedgoods.designid = reader["designid"] == DBNull.Value ? "" : reader["designid"].ToString();
                            

                            barcodedgoods.sizename = reader["sizename"] == DBNull.Value ? "" : reader["sizename"].ToString();
                            barcodedgoods.typename = reader["typename"] == DBNull.Value ? "" : reader["typename"].ToString();
                            barcodedgoods.colourname = reader["colourname"] == DBNull.Value ? "" : reader["colourname"].ToString();
                            barcodedgoods.materialname = reader["materialname"] == DBNull.Value ? "" : reader["materialname"].ToString();
                            barcodedgoods.stylename = reader["stylename"] == DBNull.Value ? "" : reader["stylename"].ToString();
                            barcodedgoods.brandname = reader["brandname"] == DBNull.Value ? "" : reader["brandname"].ToString();
                            barcodedgoods.patternname = reader["patternname"] == DBNull.Value ? "" : reader["patternname"].ToString();

                            if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_intermediatetax)
                            {
                                decimal tax_intermediate_price = reader["tax_intermediate_price"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["tax_intermediate_price"].ToString());
                                decimal tax_intermediate_dealerprice = reader["tax_intermediate_dealerprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["tax_intermediate_dealerprice"].ToString());
                                if (tax_intermediate_price > 0)
                                    barcodedgoods.price = tax_intermediate_price;
                                if (tax_intermediate_price > 0)
                                    barcodedgoods.dealerprice = tax_intermediate_dealerprice;
                            }

                            decimal ibuyingprice = reader["ibuyingprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["ibuyingprice"].ToString());
                            if (ibuyingprice == 0 || ibuyingprice < barcodedgoods.buyingprice)
                                ibuyingprice = barcodedgoods.buyingprice;

                            if (ibuyingprice > 0)
                                barcodedgoods.marginpercentage = Math.Round(((barcodedgoods.price - barcodedgoods.discount - ibuyingprice) / ibuyingprice) * 100, 2);
                        }
                    }
                    type++;
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, "barcodedsearch : " + uan);
            }


            if (Services.ConfigManager.Instance(Context.OrganizationID).Config.system_ismfgapplication)
            {
                MfgdesignDataService mfgdesignDataService = new MfgdesignDataService() { Context = this.Context };
                var uanitem = mfgdesignDataService.SearchUANItem(uan, null);
                if (uanitem != null)
                {
                    ProductsDataService productsDataService = new ProductsDataService() { Context = this.Context };
                    var product = productsDataService.Search(new ProductsCreteria()
                    {
                        id = uanitem.productid
                    }).First();

                    barcodedgoods.serialno = uan;
                    barcodedgoods.designid = uanitem.code;
                    barcodedgoods.hsncode = product.hsncode;
                    barcodedgoods.sellingname = product.name;
                    barcodedgoods.sellingmode = product.sellingmode;
                    barcodedgoods.price = uanitem.salerate;
                    barcodedgoods.wsrate = uanitem.wsrate;
                    barcodedgoods.dealerprice = uanitem.dealerprice;
                    barcodedgoods.itemid = uanitem.itemid;
                    barcodedgoods.colourid = uanitem.colourid;
                    barcodedgoods.productid = uanitem.productid;
                    barcodedgoods.styleid = uanitem.styleid;
                    barcodedgoods.sleeveid = uanitem.sleeveid;
                    barcodedgoods.fitid = uanitem.fitid;
                    barcodedgoods.sizeid = uanitem.sizeid;
                    barcodedgoods.patternid = uanitem.patternid;
                    barcodedgoods.materialid = uanitem.materialid;
                    barcodedgoods.mrp = uanitem.mrp;
                    barcodedgoods.buyingprice = uanitem.cost;
                }
            }


            return barcodedgoods;
        }

        public Barcodedgoods BarcodeSearch(string locationid, string barcode, bool includenegative)
        {
            if (!InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_supportautomissingstock)
                return BarcodeSearch(locationid, barcode, 1, includenegative);
            else
            {
                Barcodedgoods returnitem = BarcodeSearch(locationid, barcode, 1, includenegative);
                var parts = barcode.Split(new char[] { '_', '-' });
                if (parts.Length != 2) return returnitem;

                ReferencelistDataService _reference = new ReferencelistDataService() { Context = this.Context };
                List<Referencelist> lst = _reference.Search(new ReferencelistCreteria() { id = long.Parse(locationid) });
                if (lst != null && lst.Count > 0)
                {
                    if (lst[0].additionalinfo == null || lst[0].additionalinfo.parentid == 0
                        || returnitem.companyid == lst[0].additionalinfo.parentid)
                        return returnitem;

                    if (returnitem.companyid != lst[0].additionalinfo.parentid)
                        returnitem.companyid = lst[0].additionalinfo.parentid;
                }
                return returnitem;
            }
        }

        public Barcodedgoods BarcodeSearch(string locationid, string barcode)
        {
            return BarcodeSearch(locationid, barcode, 1, false);
        }

        public List<string> GetQuantoBarcode(string sourcebarcode)
        {

            if (string.IsNullOrEmpty(sourcebarcode)) throw new Exception("Please enter the barcode");
            List<string> result = new List<string>();
            // changed from inner join to outer join add added and stock.isactive=true
            string query = @"select stock.barcode from stock 
                                    left outer join salablegoods on salablegoods.stockid = stock.id
                                    where stock.serialno = @barcode and stock.isactive=true
                                    order by stock.stockdate";

            var param = new SortedList<string, object>();
            param.Add("barcode", sourcebarcode);
            using (var reader = ExecuteReader(query, param))
            {
                while (reader.Read())
                    result.Add(reader.GetString(0));
            }

            string iquery = @"select stock.barcode from Sellablebarcodes 
                                    inner join stock on stock.id = Sellablebarcodes.stockid
                                    inner join salablegoods on salablegoods.stockid = Sellablebarcodes.stockid
                                    where sellablebarcodes.barcode = @barcode 
                                    order by stock.stockdate";
            param = new SortedList<string, object>();
            param.Add("barcode", sourcebarcode);
            using (var reader = ExecuteReader(iquery, param))
            {
                if (reader.Read())
                    result.Add(reader.GetString(0));
            }

            return result;
        }

        public long GetFromSellableBarcodes(string sourcebarcode, long locationid, bool includenegative)
        {
            if (string.IsNullOrEmpty(sourcebarcode)) throw new Exception("Please enter the barcode");
            string iquery = @"select Sellablebarcodes.stockid from Sellablebarcodes 
                                    inner join stock on stock.id = Sellablebarcodes.stockid
                                    inner join salablegoods on salablegoods.stockid = Sellablebarcodes.stockid
                                    where sellablebarcodes.barcode = @barcode 
                                    and salablegoods.locationid=@locationid {0}
                                    order by stock.stockdate
                                    limit 1";
            iquery = string.Format(iquery, includenegative ? "" : "and iqty - (oqty + dqty)>0");

            var param = new SortedList<string, object>();
            param.Add("barcode", sourcebarcode);
            param.Add("locationid", locationid);
            using (var reader = ExecuteReader(iquery, param))
            {
                if (reader.Read())
                    return reader.GetInt64(0);
            }
            return 0;
        }

        public long GetSecondaryBarcode(string sourcebarcode, long locationid, bool includenegative)
        {
            if (string.IsNullOrEmpty(sourcebarcode)) throw new Exception("Please enter the barcode");
            if (Services.ConfigManager.Instance(Context.OrganizationID).Config.system_ismfgapplication)
            {
                var query = @"select stock.id from stock 
                                    inner join salablegoods on salablegoods.stockid = stock.id
                                    where stock.isactive=true and stock.barcode = @barcode and salablegoods.locationid=@locationid {0} 
                                    limit 1";

                query = string.Format(query, includenegative ? "" : "and iqty - (oqty + dqty + jqty + aqty)>0");
                var qparam = new SortedList<string, object>();
                qparam.Add("barcode", sourcebarcode);
                qparam.Add("locationid", locationid);
                using (var reader = ExecuteReader(query, qparam))
                {
                    if (reader.Read())
                        return reader.GetInt64(0);
                }
            }
            else
            {
                var query = @"select stock.id from stock 
                                    inner join salablegoods on salablegoods.stockid = stock.id
                                    where stock.isactive=true and stock.serialno = @barcode and salablegoods.locationid=@locationid {0}
                                    order by stock.stockdate
                                    limit 1";

                query = string.Format(query, includenegative ? "" : "and iqty - (oqty + dqty + jqty + aqty)>0");
                var qparam = new SortedList<string, object>();
                qparam.Add("barcode", sourcebarcode);
                qparam.Add("locationid", locationid);
                using (var reader = ExecuteReader(query, qparam))
                {
                    if (reader.Read())
                        return reader.GetInt64(0);
                }
            }

            string iquery = @"select Sellablebarcodes.stockid from Sellablebarcodes 
                                    inner join stock on stock.id = Sellablebarcodes.stockid
                                    inner join salablegoods on salablegoods.stockid = Sellablebarcodes.stockid
                                    where sellablebarcodes.barcode = @barcode 
                                    and salablegoods.locationid=@locationid {0}
                                    order by stock.stockdate
                                    limit 1";
            iquery = string.Format(iquery, includenegative ? "" : "and iqty - (oqty + dqty + jqty + aqty)>0");

            var param = new SortedList<string, object>();
            param.Add("barcode", sourcebarcode);
            param.Add("locationid", locationid);
            using (var reader = ExecuteReader(iquery, param))
            {
                if (reader.Read())
                    return reader.GetInt64(0);
            }
            return 0;
        }

        internal List<BatchBarcodes> ProductList(string locationid, string productid)
        {
            NpgsqlConnection conn = CreateConnection();
            var result = new List<BatchBarcodes>();
            try
            {
                long locid = 0;
                long prdid = 0;

                long.TryParse(locationid, out locid);
                long.TryParse(productid, out prdid);

                if (locid <= 0 || prdid <= 0) return result;

                bool showitemname = (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig != null &&
    InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig.ContainsKey(locid) &&
    InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[locid].sale_showitemname);

                conn.Open();
                NpgsqlCommand command = null;
                command = GetCommand(SQL_Productsearch, conn);
                command.Parameters.Add("productid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = prdid;
                command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = locid;
                List<Barcodedgoods> barcodelist = new List<Barcodedgoods>();
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Barcodedgoods barcodedgoods = new Barcodedgoods();
                        barcodedgoods.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        barcodedgoods.received = reader["received"] == DBNull.Value ? false : Convert.ToBoolean(reader["received"]);
                        barcodedgoods.iqty = reader["iqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["iqty"].ToString());
                        barcodedgoods.oqty = reader["oqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["oqty"].ToString());
                        barcodedgoods.dqty = reader["dqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["dqty"].ToString());
                        barcodedgoods.jqty = reader["jqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["jqty"].ToString());
                        barcodedgoods.price = reader["price"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["price"].ToString());
                        barcodedgoods.buyingprice = reader["buyingprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["buyingprice"].ToString());
                        barcodedgoods.discount = reader["discount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discount"].ToString());
                        barcodedgoods.additionaldiscount = reader["additionaldiscount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["additionaldiscount"].ToString());
                        barcodedgoods.stockid = reader["stockid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["stockid"].ToString());
                        barcodedgoods.companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"].ToString());
                        barcodedgoods.barcode = reader["barcode"] == DBNull.Value ? "" : reader["barcode"].ToString();
                        barcodedgoods.designid = reader["designid"] == DBNull.Value ? "" : reader["designid"].ToString();
                        barcodedgoods.itemid = reader["itemid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["itemid"].ToString());
                        barcodedgoods.taxid = reader["salestaxid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["salestaxid"].ToString());
                        barcodedgoods.sellingname = reader["sellingname"] == DBNull.Value ? "unknown" : reader["sellingname"].ToString();
                        if (showitemname &&
                     reader["itemname"] != DBNull.Value &&
                     reader["itemname"].ToString() != "")
                            barcodedgoods.sellingname = reader["itemname"].ToString();
                        barcodedgoods.serialno = reader["serialno"] == DBNull.Value ? "" : reader["serialno"].ToString();
                        barcodedgoods.imageid = reader["imageid"] == DBNull.Value ? null : reader["imageid"].ToString();
                        barcodedgoods.hsncode = reader["hsncode"] == DBNull.Value ? "" : reader["hsncode"].ToString();
                        barcodedgoods.productid = reader["productid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productid"].ToString());
                        barcodedgoods.stockisbit = reader["isbit"] == DBNull.Value ? false : Convert.ToBoolean(reader["isbit"]);
                        barcodedgoods.runtimeprice = reader["runtimeprice"] == DBNull.Value ? false : Convert.ToBoolean(reader["runtimeprice"]);
                        barcodedgoods.stockqty = reader["qty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["qty"].ToString());
                        barcodedgoods.sellingmode = reader["sellingmode"] == DBNull.Value ? 0 : Convert.ToInt32(reader["sellingmode"]);
                        barcodelist.Add(barcodedgoods);
                    }
                }
                conn.Close();

                foreach (var grp in barcodelist.GroupBy(e => e.price - e.discount))
                {
                    result.Add(new BatchBarcodes()
                    {
                        id = grp.Key.ToString(),
                        display = grp.Key + " @ " + grp.Sum(e => e.iqty - (e.dqty + e.jqty + e.oqty + e.aqty)),
                        qty = grp.Sum(e => e.iqty - (e.dqty + e.jqty + e.oqty+e.qty)),
                        items = grp.ToList()
                    });
                }
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, "Batch search");
            }
            return result;
        }
        internal List<SellableGoodsBrandPrice> SearchBrandPrice(string scustomerid, string slocationid)
        {
            NpgsqlConnection conn = CreateConnection();
            var result = new List<SellableGoodsBrandPrice>();
            try
            {
                long customerid = 0;
                long locationid = 0;


                if (!long.TryParse(scustomerid, out customerid))
                    return result;


                if (!long.TryParse(slocationid, out locationid))
                    return result;

                conn.Open();
                NpgsqlCommand command = null;
                command = GetCommand(SQL_brandWsPrice, conn);
                command.Parameters.Add("customerid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = customerid;
                command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = locationid;

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        SellableGoodsBrandPrice barcodedgoods = new SellableGoodsBrandPrice();
                        var qty = reader["qty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["qty"].ToString());
                        var receivable = reader["receivable"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["receivable"].ToString());
                        var buyingprice = reader["buyingprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["buyingprice"].ToString());
                        var billmargin = reader["billmargin"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["billmargin"].ToString());
                        var discountpercent = reader["discountpercent"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discountpercent"].ToString());

                        barcodedgoods.discountpercent = discountpercent;
                        if (qty > 0 && buyingprice > 0)
                            barcodedgoods.markup = Math.Round(((receivable / qty) - buyingprice) / buyingprice * 100, 2);
                        if (qty > 0 && receivable > 0)
                            barcodedgoods.markdown = Math.Round(((receivable / qty) - buyingprice) / receivable * 100, 2);
                        barcodedgoods.billedon = reader["billdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["billdate"].ToString());
                        barcodedgoods.brand = reader["brandname"] == DBNull.Value ? "" : reader["brandname"].ToString();
                        barcodedgoods.billno = reader["billno"] == DBNull.Value ? "" : reader["billno"].ToString();
                        if (billmargin == 0)
                        {
                            var stockprice = reader["stockprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["stockprice"].ToString());
                            var billprice = reader["billprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["billprice"].ToString());
                            if (stockprice > 0 && billprice > 0)
                            {
                                billmargin = Math.Round(100 - (billprice / stockprice * 100));
                            }
                        }

                        barcodedgoods.billmargin = billmargin;

                        result.Add(barcodedgoods);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, "Brand Price search");
            }
            return result.OrderBy(e => e.brand).ToList();
        }

        internal List<BatchBarcodes> BatchListByBrand(string locationid, string brandid, string customerid, string salesreturn)
        {
            NpgsqlConnection conn = CreateConnection();
            var result = new List<BatchBarcodes>();
            try
            {
                long locid = 0;
                long brdid = 0;
                long prdid = 0;
                long cusid = 0;
                bool issalesreturn = false;
                long.TryParse(locationid, out locid);
                if (!long.TryParse(brandid, out brdid))
                {
                    long.TryParse(brandid.Split('_')[0], out prdid);
                    long.TryParse(brandid.Split('_')[1], out brdid);
                }
                long.TryParse(customerid, out cusid);
                bool.TryParse(salesreturn, out issalesreturn);
                if (locid <= 0 || brdid <= 0) return result;

                bool showitemname = (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig != null &&
InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig.ContainsKey(locid) &&
InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[locid].sale_showitemname);

                conn.Open();
                NpgsqlCommand command = null;
                var query = issalesreturn ? SQL_Batchsoldbybrandid : SQL_Batchsearchbybrandid;
                if (prdid > 0)
                {
                    query = query.Replace("items.brandid=@brandid", "items.brandid=@brandid and items.productid=@productid");
                }
                command = GetCommand(query, conn);
                command.Parameters.Add("brandid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = brdid;
                command.Parameters.Add("productid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = prdid;
                command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = locid;
                command.Parameters.Add("customerid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = cusid;
                command.Parameters.Add("sixmonthdate", NpgsqlTypes.NpgsqlDbType.Date).Value = DateTimeNow.Date.AddMonths(-12);

                List<Barcodedgoods> barcodelist = new List<Barcodedgoods>();
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Barcodedgoods barcodedgoods = new Barcodedgoods();
                        barcodedgoods.errormessage = issalesreturn ? Convert.ToString(reader["billno"]) : "";
                        barcodedgoods.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        barcodedgoods.received = reader["received"] == DBNull.Value ? false : Convert.ToBoolean(reader["received"]);
                        barcodedgoods.iqty = reader["iqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["iqty"].ToString());
                        barcodedgoods.oqty = reader["oqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["oqty"].ToString());
                        barcodedgoods.dqty = reader["dqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["dqty"].ToString());
                        barcodedgoods.jqty = reader["jqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["jqty"].ToString());
                        barcodedgoods.price = reader["price"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["price"].ToString());
                        barcodedgoods.buyingprice = reader["buyingprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["buyingprice"].ToString());
                        if (issalesreturn)
                        {
                            barcodedgoods.buyingprice = Math.Round(barcodedgoods.buyingprice, 0);
                            barcodedgoods.price = Math.Round(barcodedgoods.price, 0);
                        }
                        barcodedgoods.discount = reader["discount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discount"].ToString());
                        barcodedgoods.additionaldiscount = reader["additionaldiscount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["additionaldiscount"].ToString());
                        barcodedgoods.stockid = reader["stockid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["stockid"].ToString());
                        barcodedgoods.companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"].ToString());
                        barcodedgoods.barcode = reader["barcode"] == DBNull.Value ? "" : reader["barcode"].ToString();
                        barcodedgoods.designid = reader["designid"] == DBNull.Value ? "" : reader["designid"].ToString();
                        barcodedgoods.sizename = reader["sizename"] == DBNull.Value ? "" : reader["sizename"].ToString();
                        barcodedgoods.colourname = reader["colourname"] == DBNull.Value ? "" : reader["colourname"].ToString();
                        barcodedgoods.brandname = reader["brandname"] == DBNull.Value ? "" : reader["brandname"].ToString();
                        barcodedgoods.itemid = reader["itemid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["itemid"].ToString());
                        barcodedgoods.taxid = reader["salestaxid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["salestaxid"].ToString());
                        barcodedgoods.sellingname = reader["sellingname"] == DBNull.Value ? "unknown" : reader["sellingname"].ToString();

                        if (showitemname &&
                     reader["itemname"] != DBNull.Value &&
                     reader["itemname"].ToString() != "")
                            barcodedgoods.sellingname = reader["itemname"].ToString();

                        barcodedgoods.serialno = reader["serialno"] == DBNull.Value ? "" : reader["serialno"].ToString();
                        barcodedgoods.imageid = reader["imageid"] == DBNull.Value ? null : reader["imageid"].ToString();
                        barcodedgoods.hsncode = reader["hsncode"] == DBNull.Value ? "" : reader["hsncode"].ToString();
                        barcodedgoods.productid = reader["productid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productid"].ToString());
                        barcodedgoods.stockisbit = reader["isbit"] == DBNull.Value ? false : Convert.ToBoolean(reader["isbit"]);
                        barcodedgoods.runtimeprice = reader["runtimeprice"] == DBNull.Value ? false : Convert.ToBoolean(reader["runtimeprice"]);
                        barcodedgoods.stockqty = reader["qty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["qty"].ToString());
                        barcodedgoods.pdpercent = reader["purchasediscountpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["purchasediscountpercentage"].ToString());
                        barcodedgoods.sellingmode = reader["sellingmode"] == DBNull.Value ? 0 : Convert.ToInt32(reader["sellingmode"]);
                        if (barcodedgoods.price == 0)
                        {
                            barcodedgoods.buyingprice = Math.Round(barcodedgoods.buyingprice * 115 / 100);
                        }
                        barcodelist.Add(barcodedgoods);
                    }
                }
                conn.Close();

                if (issalesreturn)
                {
                    foreach (var grp in barcodelist.GroupBy(e => new { e.designid, e.sizename, e.colourname, e.price, e.errormessage }))
                    {
                        var row = new BatchBarcodes()
                        {
                            id = string.Format("{0}_{1}_{2}", grp.Key.designid, grp.Key.sizename, grp.Key.colourname, grp.Key.errormessage),
                            display = string.Format("{0} {1} {2} ({3}) ",
                            grp.Key.designid, grp.Key.sizename, grp.Key.colourname, grp.Key.errormessage) + " @ " + grp.First().price.ToString("f0"),
                            qty = grp.Sum(e => e.iqty - (e.dqty + e.oqty + e.jqty+e.aqty)),
                            items = grp.ToList()
                        };
                        row.items.ForEach(e => e.errormessage = "");
                        result.Add(row);
                    }
                }
                else
                {
                    foreach (var grp in barcodelist.GroupBy(e => new { e.designid, e.sizename, e.colourname, e.pdpercent, e.price }))
                    {
                        var row = new BatchBarcodes()
                        {
                            id = string.Format("{0}_{1}_{2}_{3}", grp.Key.designid, grp.Key.sizename, grp.Key.colourname, grp.Key.price),
                            display = string.Format("{0} {1} {2}", grp.Key.designid, grp.Key.sizename, grp.Key.colourname) + " @ " + grp.First().price.ToString("f0"),
                            qty = grp.Sum(e => e.iqty - (e.dqty + e.oqty + e.jqty+e.aqty)),
                            items = grp.ToList()
                        };

                        if (grp.Key.pdpercent > 0)
                            row.display = string.Format("{0} {1} {2} {3}%", grp.Key.designid, grp.Key.sizename, grp.Key.colourname, grp.Key.pdpercent) + " @ " + grp.First().price.ToString("f0");

                        if (result.Exists(e => e.id == row.id))
                        {
                            var existing = result.Find(e => e.id == row.id);
                            existing.items.AddRange(row.items);
                            existing.qty = existing.items.Sum(e => e.iqty - (e.dqty + e.oqty + e.jqty+e.aqty));
                        }
                        else
                        {
                            result.Add(row);
                        }
                    }
                }
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, "Batch search");
            }
            return result;
        }

        public bool IncreseStok(string barcode, long locationid)
        {
            Salablegoods salablegoods = null;
            Stock stock = null;
            if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig != null &&
                InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig.ContainsKey(locationid) &&
                InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[locationid].sale_warehousestock)
            {
                long id = 0;
                var param = new SortedList<string, object>();
                param.Add("barcode", barcode);
                param.Add("locationid", locationid);
                string query = @"select salablegoods.id from salablegoods 
            inner join stock on stock.id = stockid 
            where stock.invoiceitemid<=0 and stock.barcode=@barcode and salablegoods.locationid = @locationid and salablegoods.isactive=true
            order by salablegoods.id desc";
                using (var reader = ExecuteReader(query, param))
                {
                    if (reader.Read())
                        id = reader.GetInt64(0);
                }
                if (id > 0)
                {
                    using (var connection = CreateConnection())
                    {
                        connection.Open();
                        var transaction = connection.BeginTransaction();
                        query = "update salablegoods set dqty=dqty-1 where id=@id";
                        param.Add("id", id);
                        ExecuteQuery(query, param, connection, transaction);
                        salablegoods = new Salablegoods()
                        {
                            iqty = 1,
                            id = id
                        };
                        StocktransactionDataService stocktransactionDataService = new StocktransactionDataService() { Context = this.Context };
                        stocktransactionDataService.IncreaseStock(salablegoods, transaction, connection);
                        transaction.Commit();
                        connection.Close();
                        return true;
                    }
                }
                else
                {
                    MfgdesignDataService mfgdesignDataService = new MfgdesignDataService() { Context = this.Context };
                    var mfgdesigns = mfgdesignDataService.Search(new MfgdesignCreteria()
                    {
                        uancode = barcode,
                        includedetail = true
                    });
                    if (mfgdesigns.Count == 1)
                    {
                        ProductsDataService productsDataService = new ProductsDataService() { Context = this.Context };
                        CompanyDataService companyDataService = new CompanyDataService() { Context = this.Context };
                        var companylist = companyDataService.Search(new CompanyCreteria());
                        var productlist = productsDataService.Search(new ProductsCreteria() { id = mfgdesigns.First().productid });
                        var uanitem = mfgdesigns.First().sizegroup.items.Find(x => x.uid == barcode);
                        SalablegoodsDataService salablegoodsDataService = new SalablegoodsDataService() { Context = this.Context };
                        StockDataService stockDataService = new StockDataService() { Context = this.Context };
                        var existstock = stockDataService.Search(new StockCreteria() { barcode = barcode });
                        long stockid = 0;
                        if (string.IsNullOrEmpty(productlist.First().hsncode)) throw new Exception("HSN Code can't be empty");
                        if (existstock.Count == 0)
                        {
                            stockid = stockDataService.Insert(new Stock()
                            {
                                barcode = barcode,
                                hsncode = productlist.First().hsncode,
                                designid = mfgdesigns.First().code,
                                itemid = uanitem.itemid,
                                productid = mfgdesigns.First().productid,
                                brandid = mfgdesigns.First().brandid,
                                buyingprice = uanitem.wsrate,
                                ibuyingprice = uanitem.wsrate,
                                buyingqty = 1,
                                displaymrp = uanitem.mrp,
                                companyid = companylist.First().id,
                                isactive = true,
                                printingname = productlist.First().name,
                                price = uanitem.wsrate,
                                version = 1000,
                                eprice = uanitem.wsrate,
                                transfered = 1
                            }).id;
                        }
                        else
                        {
                            stockid = existstock[0].id;
                        }

                        try
                        {
                            salablegoods = salablegoodsDataService.Insert(new Salablegoods()
                            {
                                stockid = stockid,
                                isactive = true,
                                version = 1000,
                                iqty = 1,
                                price = uanitem.wsrate,
                                locationid = locationid,
                                barcode = barcode,
                                companyid = companylist.First().id
                            });
                            StocktransactionDataService stocktransactionDataService = new StocktransactionDataService() { Context = this.Context };
                            stocktransactionDataService.IncreaseStock(salablegoods);
                        }
                        catch (Exception exp)
                        {
                            query = "update salablegoods set dqty=dqty-1 where stockid=@id and locationid=@locationid";
                            param.Clear();
                            param.Add("id", stockid);
                            param.Add("locationid", locationid);
                            ExecuteQuery(query, param);
                        }
                        return true;
                    }
                }
            }
            return false;
        }
        public bool IncreseUANStok(string barcode, long locationid)
        {
            if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig != null &&
                InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig.ContainsKey(locationid) &&
                InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[locationid].sale_warehousestock)
            {
                long id = 0;
                var param = new SortedList<string, object>();
                param.Add("serialno", barcode);
                param.Add("locationid", locationid);
                string query = @"select salablegoods.id from salablegoods 
            inner join stock on stock.id = stockid 
            where stock.invoiceitemid<=0 and serialno=@serialno and salablegoods.locationid = @locationid and salablegoods.isactive=true
            order by salablegoods.id desc";
                using (var reader = ExecuteReader(query, param))
                {
                    if (reader.Read())
                        id = reader.GetInt64(0);
                }
                if (id > 0)
                {
                    query = "update salablegoods set dqty=dqty-1 where id=@id";
                    param.Add("id", id);
                    StocktransactionDataService stocktransactionDataService = new StocktransactionDataService() { Context = this.Context };
                    stocktransactionDataService.IncreaseStock(new Salablegoods()
                    {
                        id = id,
                        dqty = 1
                    });
                    ExecuteQuery(query, param);
                    return true;
                }
                else
                {
                    MfgdesignDataService mfgdesignDataService = new MfgdesignDataService() { Context = this.Context };
                    var mfgdesigns = mfgdesignDataService.Search(new MfgdesignCreteria()
                    {
                        uancode = barcode,
                        includedetail = true
                    });
                    if (mfgdesigns.Count == 1)
                    {
                        ProductsDataService productsDataService = new ProductsDataService() { Context = this.Context };
                        CompanyDataService companyDataService = new CompanyDataService() { Context = this.Context };
                        var companylist = companyDataService.Search(new CompanyCreteria());
                        var productlist = productsDataService.Search(new ProductsCreteria() { id = mfgdesigns.First().productid });
                        var uanitem = mfgdesigns.First().sizegroup.items.Find(x => x.uid == barcode);
                        SalablegoodsDataService salablegoodsDataService = new SalablegoodsDataService() { Context = this.Context };
                        StockDataService stockDataService = new StockDataService() { Context = this.Context };
                        var existstock = stockDataService.Search(new StockCreteria() { barcode = barcode.Replace("-", "") + "-" + "AUTO" });
                        long stockid = 0;
                        if (string.IsNullOrEmpty(productlist.First().hsncode)) throw new Exception("HSN Code can't be empty");
                        if (existstock.Count == 0)
                        {
                            stockid = stockDataService.Insert(new Stock()
                            {
                                barcode = barcode.Replace("-", "") + "-" + "AUTO",
                                serialno = barcode,
                                hsncode = productlist.First().hsncode,
                                designid = mfgdesigns.First().code,
                                itemid = uanitem.itemid,
                                displaymrp = uanitem.mrp,
                                productid = mfgdesigns.First().productid,
                                brandid = mfgdesigns.First().brandid,
                                buyingprice = uanitem.wsrate,
                                ibuyingprice = uanitem.wsrate,
                                buyingqty = 1,
                                companyid = companylist.First().id,
                                isactive = true,
                                printingname = productlist.First().name,
                                price = uanitem.wsrate,
                                version = 1,
                                eprice = uanitem.wsrate,
                                transfered = 1
                            }).id;
                        }
                        else
                        {
                            stockid = existstock[0].id;
                        }

                        try
                        {
                            var salablegoods = salablegoodsDataService.Insert(new Salablegoods()
                            {
                                stockid = stockid,
                                isactive = true,
                                version = 1,
                                iqty = 1,
                                price = uanitem.wsrate,
                                locationid = locationid,
                                barcode = barcode.Replace("-", "") + "-" + "AUTO",
                                companyid = companylist.First().id
                            });
                            StocktransactionDataService stocktransactionDataService = new StocktransactionDataService() { Context = this.Context };
                            stocktransactionDataService.IncreaseStock(salablegoods);
                        }
                        catch (Exception exp)
                        {
                            query = "update salablegoods set dqty=dqty-1 where stockid=@id and locationid=@locationid";
                            param.Clear();
                            param.Add("id", stockid);
                            param.Add("locationid", locationid);
                            ExecuteQuery(query, param);
                        }
                        return true;
                    }
                }
            }
            return false;
        }

        public List<BatchBarcodes> BatchSearch(string locationid, string supplierid, string productid)
        {

            NpgsqlConnection conn = CreateConnection();
            var result = new List<BatchBarcodes>();
            try
            {
                long locid = 0;
                long supid = 0;
                long prdid = 0;

                long.TryParse(locationid, out locid);
                long.TryParse(supplierid, out supid);
                long.TryParse(productid, out prdid);

                if (locid <= 0 || supid <= 0 || prdid <= 0) return result;

                bool showitemname = (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig != null &&
InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig.ContainsKey(locid) &&
InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[locid].sale_showitemname);

                conn.Open();
                NpgsqlCommand command = null;
                command = GetCommand(SQL_Batchsearch, conn);
                command.Parameters.Add("productid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = prdid;
                command.Parameters.Add("supplierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = supid;
                command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = locid;
                List<Barcodedgoods> barcodelist = new List<Barcodedgoods>();
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Barcodedgoods barcodedgoods = new Barcodedgoods();
                        barcodedgoods.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        barcodedgoods.received = reader["received"] == DBNull.Value ? false : Convert.ToBoolean(reader["received"]);
                        barcodedgoods.iqty = reader["iqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["iqty"].ToString());
                        barcodedgoods.oqty = reader["oqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["oqty"].ToString());
                        barcodedgoods.dqty = reader["dqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["dqty"].ToString());
                        barcodedgoods.jqty = reader["jqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["jqty"].ToString());
                        barcodedgoods.price = reader["price"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["price"].ToString());
                        barcodedgoods.buyingprice = reader["buyingprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["buyingprice"].ToString());
                        barcodedgoods.discount = reader["discount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discount"].ToString());
                        barcodedgoods.additionaldiscount = reader["additionaldiscount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["additionaldiscount"].ToString());
                        barcodedgoods.stockid = reader["stockid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["stockid"].ToString());
                        barcodedgoods.companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"].ToString());
                        barcodedgoods.barcode = reader["barcode"] == DBNull.Value ? "" : reader["barcode"].ToString();
                        barcodedgoods.designid = reader["designid"] == DBNull.Value ? "" : reader["designid"].ToString();
                        barcodedgoods.itemid = reader["itemid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["itemid"].ToString());
                        barcodedgoods.taxid = reader["salestaxid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["salestaxid"].ToString());
                        barcodedgoods.sellingname = reader["sellingname"] == DBNull.Value ? "unknown" : reader["sellingname"].ToString();

                        if (showitemname &&
                     reader["itemname"] != DBNull.Value &&
                     reader["itemname"].ToString() != "")
                            barcodedgoods.sellingname = reader["itemname"].ToString();

                        barcodedgoods.serialno = reader["serialno"] == DBNull.Value ? "" : reader["serialno"].ToString();
                        barcodedgoods.imageid = reader["imageid"] == DBNull.Value ? null : reader["imageid"].ToString();
                        barcodedgoods.hsncode = reader["hsncode"] == DBNull.Value ? "" : reader["hsncode"].ToString();
                        barcodedgoods.productid = reader["productid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productid"].ToString());
                        barcodedgoods.stockisbit = reader["isbit"] == DBNull.Value ? false : Convert.ToBoolean(reader["isbit"]);
                        barcodedgoods.runtimeprice = reader["runtimeprice"] == DBNull.Value ? false : Convert.ToBoolean(reader["runtimeprice"]);
                        barcodedgoods.stockqty = reader["qty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["qty"].ToString());
                        barcodedgoods.sellingmode = reader["sellingmode"] == DBNull.Value ? 0 : Convert.ToInt32(reader["sellingmode"]);
                        barcodelist.Add(barcodedgoods);
                    }
                }
                conn.Close();

                foreach (var grp in barcodelist.GroupBy(e => e.designid))
                {
                    result.Add(new BatchBarcodes()
                    {
                        id = grp.Key,
                        display = grp.Key + " @ " + grp.First().price.ToString("f0"),
                        qty = grp.Sum(e => e.iqty - (e.dqty + e.oqty + e.jqty+e.aqty)),
                        items = grp.ToList()
                    });
                }
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, "Batch search");
            }
            return result;
        }

        public List<Barcodedgoods> BarcodeSearch(List<long> ids)
        {
            List<Barcodedgoods> retbarcodedgoods = new List<Barcodedgoods>();
            if (ids == null || ids.Count == 0) return retbarcodedgoods;
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                NpgsqlCommand command = null;
                var SQL_Barcodesearch = @"select salablegoods.id, salablegoods.companyid, iqty, oqty, dqty, jqty, aqty, stockid, salablegoods.barcode,lrinvoiceitems.dealerprice,lrinvoiceitems.wsrate,
                COALESCE(stock.displaymrp,lrinvoiceitems.mrp) mrp, case when COALESCE(producttype,0)<=1 then products.name else stock.printingname end as sellingname, stock.imageid,
                stock.serialno,stock.runtimeprice,items.sizeid,items.colourid,tax_intermediate_price,tax_intermediate_dealerprice,
                salablegoods.itemid,received,salablegoods.price,stock.buyingprice buyingprice,sizelist.name sizename,
                stock.ibuyingprice ibuyingprice,salablegoods.discount,salablegoods.additionaldiscount,lrinvoiceitems.basebuyingprice,
                stock.productid,salestaxid,stock.hsncode,stock.isbit,stock.qty, products.sellingmode from salablegoods
                inner join stock on stock.id = salablegoods.stockid   
                LEFT OUTER join items on items.id = stock.itemid
                left outer join lrinvoiceitems on lrinvoiceitems.id = invoiceitemid
                left outer join referencelist sizelist on sizelist.id = items.sizeid
                inner JOIN products on products.id = stock.productid
                where salablegoods.isactive=true and salablegoods.id in (" + string.Join(",", ids) + ")";
                bool costincludepd = InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_marginoncost;
                command = GetCommand(SQL_Barcodesearch, conn);
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Barcodedgoods barcodedgoods = new Barcodedgoods();
                        barcodedgoods.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);

                        barcodedgoods.sizeid = reader["sizeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sizeid"]);
                        barcodedgoods.colourid = reader["colourid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["colourid"]);

                        barcodedgoods.received = reader["received"] == DBNull.Value ? false : Convert.ToBoolean(reader["received"]);
                        barcodedgoods.iqty = reader["iqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["iqty"].ToString());
                        barcodedgoods.oqty = reader["oqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["oqty"].ToString());
                        barcodedgoods.dqty = reader["dqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["dqty"].ToString());
                        barcodedgoods.jqty = reader["jqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["jqty"].ToString());
                        barcodedgoods.aqty = reader["aqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["aqty"].ToString());
                        barcodedgoods.price = reader["price"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["price"].ToString());
                        barcodedgoods.dealerprice = reader["dealerprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["dealerprice"].ToString());
                        barcodedgoods.wsrate = reader["wsrate"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["wsrate"].ToString());
                        barcodedgoods.buyingprice = reader["buyingprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["buyingprice"].ToString());

                        if (costincludepd && reader["basebuyingprice"] != DBNull.Value)
                        {
                            barcodedgoods.buyingprice = reader["basebuyingprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["basebuyingprice"].ToString());
                        }

                        barcodedgoods.mrp = reader["mrp"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["mrp"].ToString());
                        barcodedgoods.discount = reader["discount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discount"].ToString());
                        barcodedgoods.additionaldiscount = reader["additionaldiscount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["additionaldiscount"].ToString());
                        barcodedgoods.stockid = reader["stockid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["stockid"].ToString());
                        barcodedgoods.companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"].ToString());
                        barcodedgoods.barcode = reader["barcode"] == DBNull.Value ? "" : reader["barcode"].ToString();
                        barcodedgoods.itemid = reader["itemid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["itemid"].ToString());
                        barcodedgoods.taxid = reader["salestaxid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["salestaxid"].ToString());
                        barcodedgoods.sellingname = reader["sellingname"] == DBNull.Value ? "unknown" : reader["sellingname"].ToString();
                        barcodedgoods.sizename = reader["sizename"] == DBNull.Value ? "" : reader["sizename"].ToString();
                        barcodedgoods.serialno = reader["serialno"] == DBNull.Value ? "" : reader["serialno"].ToString();
                        barcodedgoods.imageid = reader["imageid"] == DBNull.Value ? null : reader["imageid"].ToString();
                        barcodedgoods.hsncode = reader["hsncode"] == DBNull.Value ? "" : reader["hsncode"].ToString();
                        barcodedgoods.productid = reader["productid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productid"].ToString());
                        barcodedgoods.stockisbit = reader["isbit"] == DBNull.Value ? false : Convert.ToBoolean(reader["isbit"]);
                        barcodedgoods.runtimeprice = reader["runtimeprice"] == DBNull.Value ? false : Convert.ToBoolean(reader["runtimeprice"]);
                        barcodedgoods.stockqty = reader["qty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["qty"].ToString());
                        barcodedgoods.sellingmode = reader["sellingmode"] == DBNull.Value ? 0 : Convert.ToInt32(reader["sellingmode"]);

                        if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_intermediatetax)
                        {
                            decimal tax_intermediate_price = reader["tax_intermediate_price"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["tax_intermediate_price"].ToString());
                            decimal tax_intermediate_dealerprice = reader["tax_intermediate_dealerprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["tax_intermediate_dealerprice"].ToString());
                            if (tax_intermediate_price > 0)
                                barcodedgoods.price = tax_intermediate_price;
                            if (tax_intermediate_price > 0)
                                barcodedgoods.dealerprice = tax_intermediate_dealerprice;
                        }

                        decimal ibuyingprice = reader["ibuyingprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["ibuyingprice"].ToString());
                        if (ibuyingprice == 0 || ibuyingprice < barcodedgoods.buyingprice)
                            ibuyingprice = barcodedgoods.buyingprice;

                        if (ibuyingprice > 0)
                            barcodedgoods.marginpercentage = Math.Round(((barcodedgoods.price - barcodedgoods.discount - ibuyingprice) / ibuyingprice) * 100, 2);



                        retbarcodedgoods.Add(barcodedgoods);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, "");
            }
            return retbarcodedgoods;
        }

        public List<Barcodedgoods> BarcodeSearch(long locid, List<string> barcode,
            List<SalesorderDataService.SearchBarcodeData.itemqty> usedstock,
            bool issalesreturn, bool retry, bool includeretailbarcode)
        {
            var response = BarcodeSearchInternal(locid, barcode, usedstock, issalesreturn,retry, includeretailbarcode);
            if (!issalesreturn)
            {
                barcode.RemoveAll(x => response.Exists(y => y.barcode == x || y.serialno == x));
                if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig != null &&
                InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig.ContainsKey(locid) &&
                !InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[locid].sale_negativestock)
                {
                    response.AddRange(BarcodeSearchInternal(locid, barcode, usedstock, true, retry, includeretailbarcode));
                }
            }
            return response;
        }

        public List<Barcodedgoods> BarcodeSearchInternal(long locid, List<string> barcode,
            List<SalesorderDataService.SearchBarcodeData.itemqty> usedstock,
            bool issalesreturn, bool retry,bool includeretailbarcode)
        {
            List<Barcodedgoods> barcodedgoodList = new List<Barcodedgoods>();
            List<Barcodedgoods> barcodedgoodUANList = new List<Barcodedgoods>();
            NpgsqlConnection conn = CreateConnection();
            try
            {

                if (!string.IsNullOrEmpty(InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[locid].sale_autoprefix))
                {
                    if (!barcode.Exists(e => e.StartsWith(InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[locid].sale_autoprefix)))
                    {
                        var prefix = InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[locid].sale_autoprefix;
                        barcode = barcode.ConvertAll(e =>
                        {
                            return !e.StartsWith(prefix) ? prefix + e : e;
                        });
                    }
                }
                if (WebAPI.Models.HardConfig.SupportAutoCAPS(Context.OrganizationID))
                {
                    barcode = barcode.ConvertAll(e =>
                    {
                        return Char.IsLower(e, 0) ? e.ToUpper() : e;
                    });
                }

                bool showitemname = (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig != null &&
                InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig.ContainsKey(locid) &&
                InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[locid].sale_showitemname);

                bool costincludepd = InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_marginoncost;

                conn.Open();
                NpgsqlCommand command = null;
                var query = SQL_Barcodesearch.Replace("salablegoods.barcode=@barcode",
                "salablegoods.barcode in (" + string.Join(",", barcode.Distinct().ToList().ConvertAll(e => "'" + WebAPI.Data.Utility.SafeSQL(e) + "'")) + ")" + (issalesreturn ? "" : " and salablegoods.hasstock = true"));

                if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_supportbrandprice)
                    query = SQL_Barcodesearch_brandproductprice.Replace("salablegoods.barcode=@barcode",
                "salablegoods.barcode in (" + string.Join(",", barcode.Distinct().ToList().ConvertAll(e => "'" + WebAPI.Data.Utility.SafeSQL(e) + "'")) + ")" + (issalesreturn ? "" : " and salablegoods.hasstock = true"));


                //if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_ismfgapplication)
                //{
                //    query = SQL_Barcodesearch.Replace("salablegoods.barcode=@barcode",
                //                    "stock.serialno in (" + string.Join(",", barcode.Distinct().ToList().ConvertAll(e => "'" + WebAPI.Data.Utility.SafeSQL(e) + "'")) + ")" + (issalesreturn ? "" : " and salablegoods.hasstock = true"));
                //}

                if (!InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_ismfgapplication || includeretailbarcode)
                {
                    command = GetCommand(query, conn);
                    command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = locid;
                    using (System.Data.IDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Barcodedgoods barcodedgoods = new Barcodedgoods();
                            barcodedgoods.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                            barcodedgoods.received = reader["received"] == DBNull.Value ? false : Convert.ToBoolean(reader["received"]);
                            barcodedgoods.iqty = reader["iqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["iqty"].ToString());
                            barcodedgoods.oqty = reader["oqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["oqty"].ToString());
                            barcodedgoods.dqty = reader["dqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["dqty"].ToString());
                            barcodedgoods.jqty = reader["jqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["jqty"].ToString());
                            barcodedgoods.aqty = reader["aqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["aqty"].ToString());
                            barcodedgoods.price = reader["price"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["price"].ToString());
                            barcodedgoods.buyingprice = reader["buyingprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["buyingprice"].ToString());
                            if (costincludepd && reader["basebuyingprice"] != DBNull.Value)
                            {
                                barcodedgoods.buyingprice = reader["basebuyingprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["basebuyingprice"].ToString());
                            }
                            barcodedgoods.mrp = reader["mrp"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["mrp"].ToString());
                            barcodedgoods.discount = reader["discount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discount"].ToString());
                            barcodedgoods.additionaldiscount = reader["additionaldiscount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["additionaldiscount"].ToString());
                            barcodedgoods.stockid = reader["stockid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["stockid"].ToString());

                            barcodedgoods.sizeid = reader["sizeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sizeid"].ToString());
                            barcodedgoods.colourid = reader["colourid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["colourid"].ToString());
                            barcodedgoods.designid = reader["designid"] == DBNull.Value ? "" : reader["designid"].ToString();

                            barcodedgoods.sizename = reader["sizename"] == DBNull.Value ? "" : reader["sizename"].ToString();
                            barcodedgoods.typename = reader["typename"] == DBNull.Value ? "" : reader["typename"].ToString();
                            barcodedgoods.colourname = reader["colourname"] == DBNull.Value ? "" : reader["colourname"].ToString();
                            barcodedgoods.materialname = reader["materialname"] == DBNull.Value ? "" : reader["materialname"].ToString();
                            barcodedgoods.stylename = reader["stylename"] == DBNull.Value ? "" : reader["stylename"].ToString();
                            barcodedgoods.brandname = reader["brandname"] == DBNull.Value ? "" : reader["brandname"].ToString();
                            barcodedgoods.patternname = reader["patternname"] == DBNull.Value ? "" : reader["patternname"].ToString();

                            barcodedgoods.companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"].ToString());
                            barcodedgoods.barcode = reader["barcode"] == DBNull.Value ? "" : reader["barcode"].ToString();
                            barcodedgoods.itemid = reader["itemid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["itemid"].ToString());
                            barcodedgoods.taxid = reader["salestaxid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["salestaxid"].ToString());
                            barcodedgoods.sellingname = reader["sellingname"] == DBNull.Value ? "unknown" : reader["sellingname"].ToString();
                            if (showitemname &&
                         reader["itemname"] != DBNull.Value &&
                         reader["itemname"].ToString() != "")
                                barcodedgoods.sellingname = reader["itemname"].ToString();
                            barcodedgoods.serialno = reader["serialno"] == DBNull.Value ? "" : reader["serialno"].ToString();
                            barcodedgoods.imageid = reader["imageid"] == DBNull.Value ? null : reader["imageid"].ToString();
                            barcodedgoods.hsncode = reader["hsncode"] == DBNull.Value ? "" : reader["hsncode"].ToString();
                            barcodedgoods.productid = reader["productid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productid"].ToString());
                            barcodedgoods.stockisbit = reader["isbit"] == DBNull.Value ? false : Convert.ToBoolean(reader["isbit"]);
                            barcodedgoods.runtimeprice = reader["runtimeprice"] == DBNull.Value ? false : Convert.ToBoolean(reader["runtimeprice"]);
                            barcodedgoods.stockqty = reader["qty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["qty"].ToString());
                            barcodedgoods.sellingmode = reader["sellingmode"] == DBNull.Value ? 0 : Convert.ToInt32(reader["sellingmode"]);
                            barcodedgoods.dealerprice = reader["dealerprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["dealerprice"].ToString());

                            if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_supportbrandprice)
                            {
                                barcodedgoods.brandmargin = reader["branddefaultmargin"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["branddefaultmargin"]);
                                if (barcodedgoods.brandmargin == 0)
                                    barcodedgoods.brandmargin = reader["brandproductmargin"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["brandproductmargin"]);
                            }

                            if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_intermediatetax)
                            {
                                decimal tax_intermediate_price = reader["tax_intermediate_price"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["tax_intermediate_price"].ToString());
                                decimal tax_intermediate_dealerprice = reader["tax_intermediate_dealerprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["tax_intermediate_dealerprice"].ToString());
                                if (tax_intermediate_price > 0)
                                    barcodedgoods.price = tax_intermediate_price;
                                if (tax_intermediate_price > 0)
                                    barcodedgoods.dealerprice = tax_intermediate_dealerprice;
                            }

                            decimal ibuyingprice = reader["ibuyingprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["ibuyingprice"].ToString());
                            if (ibuyingprice == 0 || ibuyingprice < barcodedgoods.buyingprice)
                                ibuyingprice = barcodedgoods.buyingprice;

                            if (ibuyingprice > 0)
                                barcodedgoods.marginpercentage = Math.Round(((barcodedgoods.price - barcodedgoods.discount - ibuyingprice) / ibuyingprice) * 100, 2);

                            barcodedgoods.qty += barcode.Count(e => e == barcodedgoods.barcode);
                            barcodedgoodList.Add(barcodedgoods);
                        }
                    }
                }

                if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_ismfgapplication || 
                    (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_fixed_barcodes &&
                    !InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_hyper_uan &&
                    !barcode.All(e => barcodedgoodList.Exists(x => x.barcode == e))))
                {
                    var uanbarcodes = barcode.FindAll(e => !barcodedgoodList.Exists(x => x.barcode == e));
                    if (uanbarcodes.Count > 0)
                    {
                        command = GetCommand(SQL_Barcodesearch.Replace("salablegoods.barcode=@barcode",
                            "stock.serialno in (" + string.Join(",", uanbarcodes.Distinct().ToList().ConvertAll(e => "'" + WebAPI.Data.Utility.SafeSQL(e) + "'")) + ") " + (issalesreturn ? "" : " and salablegoods.hasstock = true")), conn);

                        command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = locid;
                        using (System.Data.IDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Barcodedgoods barcodedgoods = new Barcodedgoods();
                                barcodedgoods.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                                barcodedgoods.received = reader["received"] == DBNull.Value ? false : Convert.ToBoolean(reader["received"]);
                                barcodedgoods.iqty = reader["iqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["iqty"].ToString());
                                barcodedgoods.oqty = reader["oqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["oqty"].ToString());
                                barcodedgoods.dqty = reader["dqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["dqty"].ToString());
                                barcodedgoods.jqty = reader["jqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["jqty"].ToString());
                                barcodedgoods.aqty = reader["aqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["aqty"].ToString());
                                barcodedgoods.price = reader["price"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["price"].ToString());
                                barcodedgoods.mrp = reader["mrp"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["mrp"].ToString());
                                barcodedgoods.buyingprice = reader["buyingprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["buyingprice"].ToString());
                                if (costincludepd && reader["basebuyingprice"] != DBNull.Value)
                                {
                                    barcodedgoods.buyingprice = reader["basebuyingprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["basebuyingprice"].ToString());
                                }
                                barcodedgoods.discount = reader["discount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discount"].ToString());
                                barcodedgoods.additionaldiscount = reader["additionaldiscount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["additionaldiscount"].ToString());
                                barcodedgoods.stockid = reader["stockid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["stockid"].ToString());
                                barcodedgoods.sizeid = reader["sizeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sizeid"].ToString());
                                barcodedgoods.colourid = reader["colourid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["colourid"].ToString());
                                barcodedgoods.designid = reader["designid"] == DBNull.Value ? "" : reader["designid"].ToString();
                                barcodedgoods.companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"].ToString());
                                barcodedgoods.barcode = reader["barcode"] == DBNull.Value ? "" : reader["barcode"].ToString();

                                barcodedgoods.sizename = reader["sizename"] == DBNull.Value ? "" : reader["sizename"].ToString();
                                barcodedgoods.typename = reader["typename"] == DBNull.Value ? "" : reader["typename"].ToString();
                                barcodedgoods.colourname = reader["colourname"] == DBNull.Value ? "" : reader["colourname"].ToString();
                                barcodedgoods.materialname = reader["materialname"] == DBNull.Value ? "" : reader["materialname"].ToString();
                                barcodedgoods.stylename = reader["stylename"] == DBNull.Value ? "" : reader["stylename"].ToString();
                                barcodedgoods.brandname = reader["brandname"] == DBNull.Value ? "" : reader["brandname"].ToString();
                                barcodedgoods.patternname = reader["patternname"] == DBNull.Value ? "" : reader["patternname"].ToString();

                                barcodedgoods.itemid = reader["itemid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["itemid"].ToString());
                                barcodedgoods.taxid = reader["salestaxid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["salestaxid"].ToString());
                                barcodedgoods.sellingname = reader["sellingname"] == DBNull.Value ? "unknown" : reader["sellingname"].ToString();
                                if (showitemname &&
                                    reader["itemname"] != DBNull.Value &&
                                    reader["itemname"].ToString() != "")
                                    barcodedgoods.sellingname = reader["itemname"].ToString();
                                barcodedgoods.serialno = reader["serialno"] == DBNull.Value ? "" : reader["serialno"].ToString();
                                barcodedgoods.imageid = reader["imageid"] == DBNull.Value ? null : reader["imageid"].ToString();
                                barcodedgoods.hsncode = reader["hsncode"] == DBNull.Value ? "" : reader["hsncode"].ToString();
                                barcodedgoods.productid = reader["productid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productid"].ToString());
                                barcodedgoods.stockisbit = reader["isbit"] == DBNull.Value ? false : Convert.ToBoolean(reader["isbit"]);
                                barcodedgoods.runtimeprice = reader["runtimeprice"] == DBNull.Value ? false : Convert.ToBoolean(reader["runtimeprice"]);
                                barcodedgoods.stockqty = reader["qty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["qty"].ToString());
                                barcodedgoods.sellingmode = reader["sellingmode"] == DBNull.Value ? 0 : Convert.ToInt32(reader["sellingmode"]);
                                barcodedgoods.dealerprice = reader["dealerprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["dealerprice"].ToString());

                                if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_intermediatetax)
                                {
                                    decimal tax_intermediate_price = reader["tax_intermediate_price"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["tax_intermediate_price"].ToString());
                                    decimal tax_intermediate_dealerprice = reader["tax_intermediate_dealerprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["tax_intermediate_dealerprice"].ToString());
                                    if (tax_intermediate_price > 0)
                                        barcodedgoods.price = tax_intermediate_price;
                                    if (tax_intermediate_price > 0)
                                        barcodedgoods.dealerprice = tax_intermediate_dealerprice;
                                }
                                decimal ibuyingprice = reader["ibuyingprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["ibuyingprice"].ToString());
                                if (ibuyingprice == 0 || ibuyingprice < barcodedgoods.buyingprice)
                                    ibuyingprice = barcodedgoods.buyingprice;

                                if (ibuyingprice > 0)
                                    barcodedgoods.marginpercentage = Math.Round(((barcodedgoods.price - barcodedgoods.discount - ibuyingprice) / ibuyingprice) * 100, 2);
                                barcodedgoodUANList.Add(barcodedgoods);
                            }
                        }


                        var uanbarcodefound = new SortedList<string, int>();
                        var missingbarcodefound = new SortedList<string, int>();
                        foreach (var uangroup in uanbarcodes.GroupBy(e => e))
                        {
                            uanbarcodefound.Add(uangroup.Key, uangroup.Count());
                        }
                        foreach (var uan in barcodedgoodUANList.GroupBy(e => e.serialno))
                        {
                            var requiredstock = uan.Count();
                            if (uanbarcodefound.ContainsKey(uan.Key))
                            {
                                requiredstock = uanbarcodefound[uan.Key];
                            }
                            foreach (var uanitem in uan)
                            {
                                if (issalesreturn)
                                {
                                    if (usedstock != null && usedstock.Count > 0 && usedstock.Exists(e => e.id == uanitem.id))
                                    {
                                        uanitem.dqty -= usedstock.Find(e => e.id == uanitem.id).qty;
                                    }
                                    if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_ismfgapplication
                                        //|| uanitem.oqty + (uanitem.dqty) > 0
                                        )
                                    {
                                        int stock = requiredstock;
                                        //if (uanitem.oqty + (uanitem.dqty) > requiredstock)
                                        //{
                                        //    stock = requiredstock;
                                        //}
                                        //else
                                        //{
                                        //    stock = (int)(uanitem.oqty + (uanitem.dqty));
                                        //}
                                        uanitem.qty = stock == 0 ? 1 : stock;
                                        requiredstock -= (stock == 0 ? 1 : stock);
                                        barcodedgoodList.Add(uanitem);
                                        if (requiredstock <= 0) break;
                                    }

                                }
                                else
                                {
                                    if (usedstock != null && usedstock.Count > 0 && usedstock.Exists(e => e.id == uanitem.id))
                                    {
                                        uanitem.dqty += usedstock.Find(e => e.id == uanitem.id).qty;
                                    }
                                    if (uanitem.iqty - (uanitem.oqty + uanitem.dqty + uanitem.jqty + uanitem.aqty) > 0)
                                    {
                                        int stock = 0;
                                        if (uanitem.iqty - (uanitem.oqty + uanitem.dqty + uanitem.jqty + uanitem.aqty) > requiredstock)
                                        {
                                            stock = requiredstock;
                                        }
                                        else
                                        {
                                            stock = (int)(uanitem.iqty - (uanitem.oqty + uanitem.dqty + uanitem.jqty + uanitem.aqty));
                                        }
                                        uanitem.qty = stock;
                                        requiredstock -= stock;
                                        barcodedgoodList.Add(uanitem);
                                        if (requiredstock <= 0) break;
                                    }
                                }
                            }

                            if (requiredstock > 0)
                            {
                                if (issalesreturn &&
                                    InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_ismfgapplication)
                                {
                                    if (barcodedgoodList.Exists(x => x.serialno == uan.Key))
                                    {
                                        barcodedgoodList.Find(x => x.serialno == uan.Key).qty += requiredstock;
                                    }
                                }
                                else
                                {
                                    missingbarcodefound.Add(uan.Key, requiredstock);
                                }
                            }
                        }
                        if (missingbarcodefound.Count > 0)
                        {
                            foreach (var mitem in missingbarcodefound)
                            {

                                barcodedgoodList.Add(new Barcodedgoods()
                                {
                                    serialno = mitem.Key,
                                    rqty = mitem.Value,
                                    qty = uanbarcodefound.ContainsKey(mitem.Key) ? uanbarcodefound[mitem.Key] : 0
                                });
                            }
                        }
                    }
                }

                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, "barcodedsearch : " + barcode);
            }

            if ((InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_hyper_uan ||
                InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_ismfgapplication)
                && retry == true)
            {
                barcode.RemoveAll(x => barcodedgoodList.Exists(y => y.barcode == x));
                if (barcode.Count > 0)
                {
                    SellablebarcodesDataService sellablebarcodesDataService = new SellablebarcodesDataService() { Context = this.Context };
                    var alternatebarcodes = sellablebarcodesDataService.Search(new SellablebarcodesCreteria()
                    {
                        ids = barcode
                    });
                    var missinguans = barcode.FindAll(x => !alternatebarcodes.Exists(y => y.barcode == x));
                    if (alternatebarcodes.Count > 0)
                    {
                        List<string> uanbarcodes = new List<string>();
                        barcode.ForEach(x =>
                        {
                            if (alternatebarcodes.Exists(y => y.barcode == x))
                            {
                                uanbarcodes.Add(alternatebarcodes.Find(y => y.barcode == x).corebarcode);
                            }
                        });
                        var uanbarcodedgoodList = BarcodeSearch(locid, uanbarcodes, usedstock, issalesreturn, false, includeretailbarcode);
                        uanbarcodedgoodList.ForEach(x =>
                        {
                            if (alternatebarcodes.Exists(y => y.corebarcode == x.barcode))
                            {
                                x.obarcode = (!String.IsNullOrEmpty(x.obarcode) ? "," : "") + alternatebarcodes.Find(y => y.corebarcode == x.barcode).barcode;
                            }
                        });
                        missinguans = uanbarcodes.FindAll(x => !alternatebarcodes.Exists(y => y.corebarcode == x));
                        barcodedgoodList.AddRange(uanbarcodedgoodList);
                        if (missinguans.Count > 0)
                        {
                            foreach (var uanbarcode in missinguans)
                                IncreseStok(uanbarcode, locid);
                            uanbarcodedgoodList = BarcodeSearch(locid, missinguans, usedstock, issalesreturn, false, includeretailbarcode);
                            barcodedgoodList.AddRange(uanbarcodedgoodList);
                        }
                    }
                }
            }

            return barcodedgoodList;
        }
        public Barcodedgoods BarcodeSearch(string locationid, string barcode, int retry, bool includenegative)
        {
            Barcodedgoods barcodedgoods = new Barcodedgoods();
            NpgsqlConnection conn = CreateConnection();
            long locid = 0;
            string actualbarcode = barcode;
            try
            {
                locid = long.Parse(locationid);
                if (locid <= 0) return new Barcodedgoods()
                {
                    errormessage = "Please provide location"
                };

                bool showitemname = (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig != null &&
InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig.ContainsKey(locid) &&
InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[locid].sale_showitemname);

                if (retry == 1 &&
                    !string.IsNullOrEmpty(InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_barcodeparser))
                {
                    barcode = InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.POSParser.UpdateBarcode(barcode);
                }

                if (retry == 1 &&
                    InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig.ContainsKey(locid) &&
                    !string.IsNullOrEmpty(InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[locid].sale_autoprefix))
                {
                    if (!barcode.StartsWith(InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[locid].sale_autoprefix))
                        barcode = InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[locid].sale_autoprefix + barcode;
                }

                long sourcestockid = 0;
                if (retry == 2)
                {
                    if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.mfg_mode != 5 &&
                        InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_fixed_barcodes &&
                        !InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_hyper_uan)
                    {
                        sourcestockid = GetSecondaryBarcode(barcode, locid, includenegative ||
                            !(InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig != null &&
                                InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig.ContainsKey(locid) &&
                                InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[locid].sale_negativestock)
                            );
                    }
                    else if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_quanto_barcodes)
                    {
                        sourcestockid = GetFromSellableBarcodes(barcode, locid, includenegative);
                    }
                }
                conn.Open();
                NpgsqlCommand command = null;
                string batchquery = SQL_Barcodesearch;

                if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_supportbrandprice)
                    batchquery = SQL_Barcodesearch_brandproductprice;

                if (sourcestockid > 0)
                {
                    batchquery = batchquery.Replace("salablegoods.barcode=@barcode", "salablegoods.stockid=@stockid");
                }
                else if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_barcodecomloc)
                {
                    var location = InfyPOS.Services.CacheManager.Instance(Context.OrganizationID).Locations.Find(x => x.id == locid);
                    if (location.companyid > 0)
                    {
                        batchquery = batchquery.Replace("salablegoods.barcode=@barcode", "salablegoods.barcode=@barcode and salablegoods.companyid=" + location.companyid);
                    }
                }

                if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.mfg_mode == 5 &&
					InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_ismfgapplication)
                {
                    if(!includenegative)
                        batchquery = batchquery.Replace("salablegoods.barcode=@barcode", "stock.serialno=@barcode and salablegoods.hasstock=true");
                    else
                        batchquery = batchquery.Replace("salablegoods.barcode=@barcode", "stock.serialno=@barcode");
                }
                else if(InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_ismfgapplication && 
                    retry == 2)
                {
                    if (!includenegative)
                        batchquery = batchquery.Replace("salablegoods.barcode=@barcode", "stock.serialno=@barcode and salablegoods.hasstock=true");
                    else
                        batchquery = batchquery.Replace("salablegoods.barcode=@barcode", "stock.serialno=@barcode");
                }

                batchquery += " limit 1";
                command = GetCommand(batchquery, conn);
                command.Parameters.Add("barcode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = barcode;
                command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = locid;
                command.Parameters.Add("stockid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = sourcestockid;
                bool hasbatch = false;
                bool costincludepd = InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_marginoncost;
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        barcodedgoods.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        barcodedgoods.received = reader["received"] == DBNull.Value ? false : Convert.ToBoolean(reader["received"]);
                        barcodedgoods.iqty = reader["iqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["iqty"].ToString());
                        barcodedgoods.oqty = reader["oqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["oqty"].ToString());
                        barcodedgoods.dqty = reader["dqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["dqty"].ToString());
                        barcodedgoods.jqty = reader["jqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["jqty"].ToString());
                        barcodedgoods.aqty = reader["aqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["aqty"].ToString());
                        barcodedgoods.price = reader["price"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["price"].ToString());
                        barcodedgoods.buyingprice = reader["buyingprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["buyingprice"].ToString());
                        if (costincludepd && reader["basebuyingprice"] != DBNull.Value)
                        {
                            barcodedgoods.buyingprice = reader["basebuyingprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["basebuyingprice"].ToString());
                        }
                        barcodedgoods.mrp = reader["mrp"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["mrp"].ToString());
                        barcodedgoods.dealerprice = reader["dealerprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["dealerprice"].ToString());
                        barcodedgoods.discount = reader["discount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discount"].ToString());
                        barcodedgoods.additionaldiscount = reader["additionaldiscount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["additionaldiscount"].ToString());
                        barcodedgoods.stockid = reader["stockid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["stockid"].ToString());
                        barcodedgoods.companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"].ToString());
                        barcodedgoods.barcode = reader["barcode"] == DBNull.Value ? "" : reader["barcode"].ToString();
                        barcodedgoods.itemid = reader["itemid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["itemid"].ToString());
                        barcodedgoods.taxid = reader["salestaxid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["salestaxid"].ToString());
                        barcodedgoods.sellingname = reader["sellingname"] == DBNull.Value ? "unknown" : reader["sellingname"].ToString();
                        if (showitemname &&
                     reader["itemname"] != DBNull.Value &&
                     reader["itemname"].ToString() != "")
                            barcodedgoods.sellingname = reader["itemname"].ToString();
                        barcodedgoods.serialno = reader["serialno"] == DBNull.Value ? "" : reader["serialno"].ToString();
                        barcodedgoods.imageid = reader["imageid"] == DBNull.Value ? null : reader["imageid"].ToString();
                        barcodedgoods.hsncode = reader["hsncode"] == DBNull.Value ? "" : reader["hsncode"].ToString();
                        barcodedgoods.productid = reader["productid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productid"].ToString());
                        barcodedgoods.stockisbit = reader["isbit"] == DBNull.Value ? false : Convert.ToBoolean(reader["isbit"]);
                        barcodedgoods.runtimeprice = reader["runtimeprice"] == DBNull.Value ? false : Convert.ToBoolean(reader["runtimeprice"]);
                        barcodedgoods.stockqty = reader["qty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["qty"].ToString());
                        barcodedgoods.sellingmode = reader["sellingmode"] == DBNull.Value ? 0 : Convert.ToInt32(reader["sellingmode"]);
                        barcodedgoods.designid = reader["designid"] == DBNull.Value ? "" : reader["designid"].ToString();

                        barcodedgoods.sizename = reader["sizename"] == DBNull.Value ? "" : reader["sizename"].ToString();
                        barcodedgoods.typename = reader["typename"] == DBNull.Value ? "" : reader["typename"].ToString();
                        barcodedgoods.colourname = reader["colourname"] == DBNull.Value ? "" : reader["colourname"].ToString();
                        barcodedgoods.materialname = reader["materialname"] == DBNull.Value ? "" : reader["materialname"].ToString();
                        barcodedgoods.stylename = reader["stylename"] == DBNull.Value ? "" : reader["stylename"].ToString();
                        barcodedgoods.brandname = reader["brandname"] == DBNull.Value ? "" : reader["brandname"].ToString();
                        barcodedgoods.patternname = reader["patternname"] == DBNull.Value ? "" : reader["patternname"].ToString();

                        if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_supportbrandprice)
                        {
                            barcodedgoods.brandmargin = reader["branddefaultmargin"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["branddefaultmargin"]);
                            if (barcodedgoods.brandmargin == 0)
                                barcodedgoods.brandmargin = reader["brandproductmargin"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["brandproductmargin"]); 
                        }

                        if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_intermediatetax)
                        {
                            decimal tax_intermediate_price = reader["tax_intermediate_price"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["tax_intermediate_price"].ToString());
                            decimal tax_intermediate_dealerprice = reader["tax_intermediate_dealerprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["tax_intermediate_dealerprice"].ToString());
                            if (tax_intermediate_price > 0)
                                barcodedgoods.price = tax_intermediate_price;
                            if (tax_intermediate_price > 0)
                                barcodedgoods.dealerprice = tax_intermediate_dealerprice;
                        }

                        if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_posmultiprice > 0)
                        {
                            barcodedgoods.ismultiprice = reader["ismultiprice"] == DBNull.Value ? false : Convert.ToBoolean(reader["ismultiprice"]);
                        }
                        decimal ibuyingprice = reader["ibuyingprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["ibuyingprice"].ToString());
                        if (ibuyingprice == 0 || ibuyingprice < barcodedgoods.buyingprice)
                            ibuyingprice = barcodedgoods.buyingprice;

                        if (ibuyingprice > 0)
                            barcodedgoods.marginpercentage = Math.Round(((barcodedgoods.price - barcodedgoods.discount - ibuyingprice) / ibuyingprice) * 100, 2);

                        if (sourcestockid > 0 &&
                            InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_glnbatch)
                            hasbatch = reader["hasbatch"] == DBNull.Value ? false : Convert.ToBoolean(reader["hasbatch"]);
                    }
                }

                if (hasbatch)
                {
                    batchquery = SQL_Barcodesearch.Replace("hasbatch", "hasbatch,stock.batch,stock.expiry");
                    if (includenegative)
                        batchquery = batchquery.Replace("salablegoods.barcode=@barcode", "stock.serialno=@barcode");
                    else
                        batchquery = batchquery.Replace("salablegoods.barcode=@barcode", "stock.serialno=@barcode and salablegoods.hasstock = true");
                    command = GetCommand(batchquery, conn);
                    command.Parameters.Add("barcode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = barcode;
                    command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = locid;
                    barcodedgoods.batchlist = new List<Barcodedbatchgoods>();
                    using (System.Data.IDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Barcodedbatchgoods uanbarcodeitem = new Barcodedbatchgoods();
                            uanbarcodeitem.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                            uanbarcodeitem.received = reader["received"] == DBNull.Value ? false : Convert.ToBoolean(reader["received"]);
                            uanbarcodeitem.iqty = reader["iqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["iqty"].ToString());
                            uanbarcodeitem.oqty = reader["oqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["oqty"].ToString());
                            uanbarcodeitem.dqty = reader["dqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["dqty"].ToString());
                            uanbarcodeitem.jqty = reader["jqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["jqty"].ToString());
                            uanbarcodeitem.aqty = reader["aqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["aqty"].ToString());
                            uanbarcodeitem.price = reader["price"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["price"].ToString());
                            uanbarcodeitem.buyingprice = reader["buyingprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["buyingprice"].ToString());
                            if (costincludepd && reader["basebuyingprice"] != DBNull.Value)
                            {
                                barcodedgoods.buyingprice = reader["basebuyingprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["basebuyingprice"].ToString());
                            }
                            uanbarcodeitem.mrp = reader["mrp"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["mrp"].ToString());
                            uanbarcodeitem.dealerprice = reader["dealerprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["dealerprice"].ToString());
                            uanbarcodeitem.discount = reader["discount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discount"].ToString());
                            uanbarcodeitem.additionaldiscount = reader["additionaldiscount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["additionaldiscount"].ToString());
                            uanbarcodeitem.stockid = reader["stockid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["stockid"].ToString());
                            uanbarcodeitem.companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"].ToString());
                            uanbarcodeitem.barcode = reader["barcode"] == DBNull.Value ? "" : reader["barcode"].ToString();
                            uanbarcodeitem.itemid = reader["itemid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["itemid"].ToString());
                            uanbarcodeitem.taxid = reader["salestaxid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["salestaxid"].ToString());
                            uanbarcodeitem.sellingname = reader["sellingname"] == DBNull.Value ? "unknown" : reader["sellingname"].ToString();
                            if (showitemname &&
                     reader["itemname"] != DBNull.Value &&
                     reader["itemname"].ToString() != "")
                                barcodedgoods.sellingname = reader["itemname"].ToString();
                            uanbarcodeitem.serialno = reader["serialno"] == DBNull.Value ? "" : reader["serialno"].ToString();
                            uanbarcodeitem.batch = reader["batch"] == DBNull.Value ? "" : reader["batch"].ToString();
                            uanbarcodeitem.expiry = reader["expiry"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["expiry"]);
                            uanbarcodeitem.imageid = reader["imageid"] == DBNull.Value ? null : reader["imageid"].ToString();
                            uanbarcodeitem.hsncode = reader["hsncode"] == DBNull.Value ? "" : reader["hsncode"].ToString();
                            uanbarcodeitem.productid = reader["productid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productid"].ToString());
                            uanbarcodeitem.stockisbit = reader["isbit"] == DBNull.Value ? false : Convert.ToBoolean(reader["isbit"]);
                            uanbarcodeitem.runtimeprice = reader["runtimeprice"] == DBNull.Value ? false : Convert.ToBoolean(reader["runtimeprice"]);
                            uanbarcodeitem.stockqty = reader["qty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["qty"].ToString());
                            uanbarcodeitem.sellingmode = reader["sellingmode"] == DBNull.Value ? 0 : Convert.ToInt32(reader["sellingmode"]);
                            uanbarcodeitem.designid = reader["designid"] == DBNull.Value ? "" : reader["designid"].ToString();

                            uanbarcodeitem.sizename = reader["sizename"] == DBNull.Value ? "" : reader["sizename"].ToString();
                            uanbarcodeitem.typename = reader["typename"] == DBNull.Value ? "" : reader["typename"].ToString();
                            uanbarcodeitem.colourname = reader["colourname"] == DBNull.Value ? "" : reader["colourname"].ToString();
                            uanbarcodeitem.materialname = reader["materialname"] == DBNull.Value ? "" : reader["materialname"].ToString();
                            uanbarcodeitem.stylename = reader["stylename"] == DBNull.Value ? "" : reader["stylename"].ToString();
                            uanbarcodeitem.brandname = reader["brandname"] == DBNull.Value ? "" : reader["brandname"].ToString();
                            uanbarcodeitem.patternname = reader["patternname"] == DBNull.Value ? "" : reader["patternname"].ToString();

                            decimal ibuyingprice = reader["ibuyingprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["ibuyingprice"].ToString());
                            if (ibuyingprice == 0 || ibuyingprice < uanbarcodeitem.buyingprice)
                                ibuyingprice = uanbarcodeitem.buyingprice;

                            if (ibuyingprice > 0)
                                uanbarcodeitem.marginpercentage = Math.Round(((uanbarcodeitem.price - uanbarcodeitem.discount - ibuyingprice) / ibuyingprice) * 100, 2);

                            barcodedgoods.batchlist.Add(uanbarcodeitem);
                        }
                    }

                    if (barcodedgoods.batchlist.Count == 1)
                        barcodedgoods.batchlist = null;
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, "barcodedsearch : " + barcode);
            }

            if (retry > 3)
                return barcodedgoods;

            if (barcodedgoods.id == 0 && actualbarcode != barcode && retry == 1)
            {
                barcodedgoods = BarcodeSearch(locationid, actualbarcode, retry + 1, includenegative);
            }

            if (barcodedgoods.id == 0 && retry == 1)
            {
                if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.mfg_mode != 5 &&
                    InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_fixed_barcodes &&
                    !InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_hyper_uan)
                {
                    barcodedgoods = BarcodeSearch(locationid, barcode, retry + 1, includenegative);
                    if (barcodedgoods != null && barcodedgoods.id > 0 && barcodedgoods.ismultiprice == false &&
                        InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_posmultiprice > 0)
                    {
                        string _query = @"select count(id) multiprice from stock where isactive=true and ismultiprice=true and serialno = @serialno";
                        var param = new SortedList<string, object>();
                        param.Add("serialno", barcode);
                        using (var reader = ExecuteReader(_query, param))
                        {
                            if (reader.Read())
                            {
                                long _count = reader["multiprice"] == DBNull.Value ? 0 : Convert.ToInt64(reader["multiprice"].ToString());
                                if ( _count > 0)
                                    barcodedgoods.ismultiprice = true;
                            }
                        }
                    }
                }
                else if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_quanto_barcodes)
                {
                    barcodedgoods = BarcodeSearch(locationid, barcode, retry + 1, includenegative);
                }
            }

            if (barcodedgoods.id == 0)
            {
                if (WebAPI.Models.HardConfig.SupportAutoCAPS(Context.OrganizationID) && Char.IsLower(barcode, 0))
                {
                    barcodedgoods = BarcodeSearch(locationid, barcode.ToUpper(), retry + 1, includenegative);
                }
            }

            if (barcodedgoods.id == 0 &&
                InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig != null &&
                InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig.ContainsKey(locid) &&
                InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[locid].sale_warehousestock)
            {
                if (AutoStock(locid, barcode))
                    barcodedgoods = BarcodeSearch(locationid, barcode, retry + 1, includenegative);
            }

            if (!includenegative && (barcodedgoods.iqty - (barcodedgoods.oqty + barcodedgoods.dqty + barcodedgoods.jqty+ barcodedgoods.aqty)) <= 0 &&
                InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig != null &&
                InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig.ContainsKey(locid) &&
                InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[locid].sale_negativestock
                )
            {
                return new Barcodedgoods()
                {
                    errormessage = "No stock available - " + barcode
                };
            }

            if (!includenegative && (barcodedgoods.iqty - (barcodedgoods.oqty + barcodedgoods.dqty + barcodedgoods.jqty + barcodedgoods.aqty)) <= 0 &&
                    Services.ConfigManager.Instance(Context.OrganizationID).Config.analyser_stock_removeaqty &&
                    InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[locid].sale_negativestock)
            {
                return new Barcodedgoods()
                {
                    errormessage = "No stock available for Sale, Please check Approval stock - " + barcode
                };
            }

            if (includenegative && InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig != null &&
                InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig.ContainsKey(locid) &&
                InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[locid].sale_negativestock
                )
            {
                if (barcodedgoods.oqty <= 0)
                {
                    if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig != null &&
                InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig.ContainsKey(locid) &&
                InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[locid].sale_negativereturnstock)
                    {
                        return new Barcodedgoods()
                        {
                            errormessage = "Return Disabled., No sales available on this barcode - " + barcode
                        };
                    }
                    barcodedgoods.errormessage = "No sales available on this barcode - " + barcode;
                }
            }

            if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig.ContainsKey(locid) &&
                InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[locid].sale_disablepurchasediscountonbill)
            {
                barcodedgoods.discount = 0;
            }

            if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_supportreturnpriceqty &&
                includenegative)
            {
                var param = new SortedList<string, object>();
                string query = @"select billitems.discountpercentage,billitems.additionaldiscountpercentage,
                                 billitems.schemediscount,billitems.additionaldiscount,billitems.discount,
                                 billitems.price,bill.billno,bill.billdate,billitems.etprice,billitems.etdiscount,
                                 billitems.salerate,billitems.qty,customer.name customername
                                 from billitems
                                 inner join bill on bill.id = billitems.billid and billitems.isactive=true
                                 left outer join customer on customer.id = customerid
                                 where bill.isactive = true and billitems.qty>0 and salablegoodsid = @salablegoodsid order by bill.id desc";
                param.Add("salablegoodsid", barcodedgoods.id);
                using (var reader = ExecuteReader(query, param))
                {
                    if (reader.Read())
                    {
                        decimal schemediscount = reader.GetDecimal(2);
                        decimal additionaldiscount = reader.GetDecimal(3);
                        decimal discount = reader.GetDecimal(4);
                        decimal price = reader.IsDBNull(5) ? 0 : reader.GetDecimal(5);
                        decimal etprice = reader.IsDBNull(8) ? 0 : reader.GetDecimal(8);
                        decimal etdiscount = reader.IsDBNull(9) ? 0 : reader.GetDecimal(9);
                        decimal salerate = reader.IsDBNull(10) ? 0 : reader.GetDecimal(10);
                        barcodedgoods.qty = reader.IsDBNull(11) ? 0 : reader.GetDecimal(11);
                        string customername = reader.IsDBNull(12) ? "" : reader.GetString(12);
                        if (etprice > 0)
                        {
                            barcodedgoods.price = etprice;
                            barcodedgoods.discount = etdiscount;
                        }
                        else
                        {
                            //barcodedgoods.price = price;
                            barcodedgoods.price = salerate;
                            barcodedgoods.discount = additionaldiscount + discount;
                        }
                        barcodedgoods.errormessage = (string.IsNullOrEmpty(barcodedgoods.errormessage) ? "" : "\r\n") + "Loaded from bill no " + reader.GetString(6) + " on " +
                            reader.GetDateTime(7).ToString("dd-MM-yyyy");
                        if (!string.IsNullOrEmpty(customername))
                            barcodedgoods.errormessage += " # Customer # " + customername;
                    }
                }
            }

            if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_salepriceonsalereturn &&
                includenegative && barcodedgoods.id > 0 && !barcodedgoods.stockisbit && barcodedgoods.stockqty == 1 &&
                barcodedgoods.sellingmode == 1)
            {
                var param = new SortedList<string, object>();
                string query = @"select billitems.discountpercentage,billitems.additionaldiscountpercentage,
                                 billitems.schemediscount,billitems.additionaldiscount,billitems.discount,
                                 billitems.price,bill.billno,bill.billdate,billitems.etprice,billitems.etdiscount,
                                 billitems.salerate
                                 from billitems
                                 inner join bill on bill.id = billitems.billid and billitems.isactive=true
                                 where bill.isactive = true and billitems.qty>0 and salablegoodsid = @salablegoodsid order by bill.id desc";
                param.Add("salablegoodsid", barcodedgoods.id);
                using (var reader = ExecuteReader(query, param))
                {
                    if (reader.Read())
                    {
                        decimal schemediscount = reader.GetDecimal(2);
                        decimal additionaldiscount = reader.GetDecimal(3);
                        decimal discount = reader.GetDecimal(4);
                        decimal price = reader.IsDBNull(5) ? 0 : reader.GetDecimal(5);
                        decimal etprice = reader.IsDBNull(8) ? 0 : reader.GetDecimal(8);
                        decimal etdiscount = reader.IsDBNull(9) ? 0 : reader.GetDecimal(9);
                        decimal salerate = reader.IsDBNull(10) ? 0 : reader.GetDecimal(10);
                        if (etprice > 0 &&
                            (Services.ConfigManager.Instance(Context.OrganizationID).Config.system_ismfgapplication ||
                            Services.ConfigManager.Instance(Context.OrganizationID).Config.system_isb2bapplication))
                        {
                            barcodedgoods.price = etprice;
                            barcodedgoods.discount = etdiscount;
                        }
                        else
                        {
                            //barcodedgoods.price = price;
                            barcodedgoods.price = salerate;
                            barcodedgoods.discount = additionaldiscount + discount;
                        }

                        barcodedgoods.errormessage = (string.IsNullOrEmpty(barcodedgoods.errormessage) ? "" : "\r\n") + "Loaded from bill no " + reader.GetString(6) + " on " +
                            reader.GetDateTime(7).ToString("dd-MM-yyyy");
                    }
                }
            }

            if (barcodedgoods.id == 0 && InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_supportautomissingstock)
            {
                if (AutoMissingStock(locid, barcode))
                    barcodedgoods = BarcodeSearch(locationid, barcode, retry + 1, includenegative);

            }

            if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_hyper_uan)
            {
                if (barcodedgoods == null || barcodedgoods.id <= 0)
                {
                    SellablebarcodesDataService sellablebarcodesDataService = new SellablebarcodesDataService() { Context = this.Context };
                    var alternatebarcodes = sellablebarcodesDataService.Search(new SellablebarcodesCreteria()
                    {
                        ids = new string[] { barcode }.ToList()
                    });
                    if (alternatebarcodes.Count > 0)
                    {
                        if (barcode != alternatebarcodes[0].corebarcode)
                        {
                            barcodedgoods = BarcodeSearch(locationid, alternatebarcodes[0].corebarcode, retry + 1, includenegative);
                            if (barcodedgoods != null &&
                                barcodedgoods.id > 0)
                            {
                                barcodedgoods.obarcode = barcode;
                                return barcodedgoods;
                            }
                        }
                    }
                }
            }

            return barcodedgoods;
        }

        private bool AutoMissingStock(long locid, string barcode)
        {
            var parts = barcode.Split(new char[] { '_', '-' });
            if (parts.Length != 2) return false;

            ProductsDataService productdata = new ProductsDataService() { Context = this.Context };
            var productlist = productdata.Search(new ProductsCreteria() { code = parts[0], uniqueproduct = true });
            if (productlist == null || productlist.Count == 0)
            {
                throw new Exception("Product details not found for a given barcode");
            }
            if (productlist.Count > 1) throw new Exception("Too many products found");

            if (productlist.Count == 1 && productlist[0].prodcompanyid == 0)
            {
                if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_multicompany)
                {
                    ReferencelistDataService _reference = new ReferencelistDataService() { Context = this.Context };
                    List<Referencelist> lst = _reference.Search(new ReferencelistCreteria() { id = locid });
                    if (lst != null && lst.Count > 0)
                    {
                        if (lst[0].additionalinfo == null || lst[0].additionalinfo.parentid == 0)
                            throw new Exception("Location and Company mapping not found in Location master.");
                        productlist[0].prodcompanyid = lst[0].additionalinfo.parentid;
                    }
                    else
                        throw new Exception("Product and Company mapping missing.");
                }
                else
                {
                    CompanyDataService companydata = new CompanyDataService() { Context = this.Context };
                    var companylist = companydata.Search(new CompanyCreteria());
                    if (companylist == null || companylist.Count == 0)
                        throw new Exception("Company details not found");
                    productlist[0].prodcompanyid = companylist[0].id;
                }
            }

            var items = new List<MissingStock>();
            items.Add(new MissingStock()
            {
                code = parts[0],
                productid = productlist[0].id,
                productname = productlist[0].name,
                hsncode = productlist[0].hsncode,
                price = decimal.Parse(parts[1]),
                dealerprice = decimal.Parse(parts[1]),
                displaymrp = decimal.Parse(parts[1]),
                locationid = locid,
                companyid = productlist[0].prodcompanyid,
                iswarehouse = false,
                barcode = barcode
            });

            StockDataService stockDataService = new StockDataService() { Context = this.Context };
            List<MissingStock> lststock = stockDataService.SaveMissingStock(items);
            if (lststock != null && lststock.Count > 0 && lststock[0].stockid > 0)
                return true;
            else
                return false;
        }

        public bool AutoStock(long locationid, string barcode)
        {
            try
            {
                StockDataService stockdataservice = new StockDataService() { Context = this.Context };
                stockdataservice.Context = this.Context;

                long locationmappedcompanyid = 0;
                ReferencelistDataService _reference = new ReferencelistDataService() { Context = this.Context };
                List<Referencelist> lst = _reference.Search(new ReferencelistCreteria() { id = locationid });
                if (lst != null && lst.Count > 0)
                {
                    if (lst[0].additionalinfo != null && lst[0].additionalinfo.parentid > 0)
                        locationmappedcompanyid = lst[0].additionalinfo.parentid;
                }

                long companyid = 0;
                if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_supportcompanywisebarcode)
                {
                    if (stockdataservice.Context.CompanyID > 0)
                        companyid = stockdataservice.Context.CompanyID;
                }

                var barcodestock = stockdataservice.Search(companyid, barcode);
                if (barcodestock.id <= 0) return false;

                var existingsalableitem = Search(new SalablegoodsCreteria()
                {
                    barcode = barcode,
                    locationid = locationid
                });

                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    var transaction = conn.BeginTransaction();

                    if (existingsalableitem.Count == 0)
                    {
                        Salablegoods sitem = new Salablegoods();
                        sitem.inwardon = DateTimeNow;
                        sitem.barcode = barcode;
                        sitem.bundleid = -2;
                        if (barcodestock.isbit)
                            sitem.iqty = barcodestock.qty;
                        else
                            sitem.iqty = 1;
                        sitem.isactive = true;
                        sitem.itemid = barcodestock.itemid;
                        sitem.locationid = locationid;
                        sitem.stockid = barcodestock.id;
                        sitem.version = 1;
                        sitem.companyid = (locationmappedcompanyid > 0 ? locationmappedcompanyid : barcodestock.companyid);
                        sitem.supplierid = barcodestock.supplierid;
                        sitem.received = true;
                        sitem.price = barcodestock.price;
                        sitem.discount = barcodestock.discount;
                        sitem.additionaldiscount = barcodestock.additionaldiscount;
                        barcodestock.salablegoodsid = sitem.id;
                        Insert(transaction, conn, sitem);
                    }
                    else
                    {
                        barcodestock.salablegoodsid = existingsalableitem[0].id;
                        existingsalableitem[0].iqty += barcodestock.qty;
                        Update(transaction, conn, existingsalableitem[0], existingsalableitem[0]);
                    }
                    StocktransactionDataService stocktransactionDataService = new StocktransactionDataService() { Context = this.Context };
                    stocktransactionDataService.AutoMissingStock(barcodestock, transaction, conn);
                    transaction.Commit();
                    conn.Close();
                }


                return true;
            }
            catch (Exception exp)
            {
                return false;
            }
        }

        char[] smallletters = "abcdefghijklmnopqrstuvwxyz".ToCharArray();
        public List<SalableResult> SearchGoods(SalablegoodsCreteria _salablegoods)
        {
            List<SalableResult> lst = new List<SalableResult>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_salablegoods.id != 0)
                {
                    command = GetCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _salablegoods.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("salablegoods.isactive", "isactive", true, "=");

                    if (!string.IsNullOrEmpty(_salablegoods.barcode))
                    {
                        qb.AddParameter("barcode", _salablegoods.barcode, "=");
                    }

                    if(_salablegoods.locationid>0)
                        qb.AddParameter("salablegoods.locationid", _salablegoods.locationid, "=");

                    if (!string.IsNullOrEmpty(_salablegoods.uan))
                    {
                        qb.AddParameter("serialno", _salablegoods.uan, "=");
                    }

                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        SalableResult osalablegoods = new SalableResult();
                        osalablegoods.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        osalablegoods.received = reader["received"] == DBNull.Value ? false : Convert.ToBoolean(reader["received"]);
                        osalablegoods.iqty = reader["iqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["iqty"].ToString());
                        osalablegoods.oqty = reader["oqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["oqty"].ToString());
                        osalablegoods.dqty = reader["dqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["dqty"].ToString());
                        osalablegoods.jqty = reader["jqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["jqty"].ToString());
                        osalablegoods.stockid = reader["stockid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["stockid"].ToString());
                        osalablegoods.barcode = reader["barcode"] == DBNull.Value ? "" : reader["barcode"].ToString();
                        osalablegoods.sellingname = reader["sellingname"] == DBNull.Value ? "" : reader["sellingname"].ToString();
                        osalablegoods.locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"].ToString());
                        osalablegoods.bundleid = reader["bundleid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["bundleid"].ToString());
                        osalablegoods.itemid = reader["itemid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["itemid"].ToString());
                        lst.Add(osalablegoods);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _salablegoods);
            }

            if (!string.IsNullOrEmpty(_salablegoods.barcode) && lst.Count == 0)
            {
                if (WebAPI.Models.HardConfig.SupportAutoCAPS(Context.OrganizationID) && Char.IsLower(_salablegoods.barcode, 0))
                {
                    _salablegoods.barcode = _salablegoods.barcode.ToUpper();
                    lst = SearchGoods(_salablegoods);
                }
            }
            return lst;
        }

        public List<Salablegoods> SearchAll(long _bundleid)
        {
            List<Salablegoods> lst = new List<Salablegoods>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = GetCommand(SQL_SearchAll, conn);
                command.Parameters.Add("bundleid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _bundleid;
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Salablegoods osalablegoods = new Salablegoods();
                        osalablegoods.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        osalablegoods.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        osalablegoods.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        osalablegoods.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        osalablegoods.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"].ToString());
                        osalablegoods.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        osalablegoods.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        osalablegoods.stockid = reader["stockid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["stockid"].ToString());
                        osalablegoods.barcode = reader["barcode"] == DBNull.Value ? "" : reader["barcode"].ToString();
                        osalablegoods.locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"].ToString());
                        osalablegoods.bundleid = reader["bundleid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["bundleid"].ToString());
                        osalablegoods.iqty = reader["iqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["iqty"]);
                        osalablegoods.oqty = reader["oqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["oqty"]);
                        osalablegoods.itemid = reader["itemid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["itemid"].ToString());
                        osalablegoods.companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"].ToString());
                        osalablegoods.received = reader["received"] == DBNull.Value ? false : Convert.ToBoolean(reader["received"]);
                        osalablegoods.sellingname = reader["sellingname"] == DBNull.Value ? "unknown" : reader["sellingname"].ToString();

                        lst.Add(osalablegoods);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, lst);
            }
            return lst;
        }

        internal void InwardGoods2WareHouse(NpgsqlTransaction transaction,
    NpgsqlConnection conn, List<Salablegoods> salablegoods,
    DateTime receivedon, Bundles bundles)
        {
            InwardGoods2WareHouse(transaction, conn, salablegoods, receivedon, bundles, false);
        }
        internal void InwardGoods2WareHouse(NpgsqlTransaction transaction,
            NpgsqlConnection conn, List<Salablegoods> salablegoods,
            DateTime receivedon, Bundles bundles, bool disableuanvalidation)
        {
            List<Salablegoods> shortageStocks = new List<Salablegoods>();
            SortedDictionary<string, string> errorMessage = new SortedDictionary<string, string>();
            foreach (var item in salablegoods)
            {
                NpgsqlCommand command = GetCommand(SQL_Inward2WarehouseReduce, conn, transaction);
                command.Parameters.Add("stockid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.stockid;
                command.Parameters.Add("salablegoodsid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.id;
                command.Parameters.Add("qty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = item.aqty;
                bool hassuccess = true;
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        var afterreduceiqty = Convert.ToDecimal(reader[0]);
                        if (afterreduceiqty < 0)
                        {
                            if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig.ContainsKey(item.locationid) &&
                            InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[item.locationid].sale_negativestock)
                            {
                                shortageStocks.Add(item);
                                errorMessage.Add(item.barcode, "Expected stock not available in " + Convert.ToString(reader[1]).ToString() +
     " shortage " + Math.Abs(afterreduceiqty).ToString());
                                hassuccess = false;
                            }
                        }
                    }
                }
                if (hassuccess)
                {
                    command = GetCommand(SQL_Inward2WarehouseIncrease, conn, transaction);
                    command.Parameters.Add("stockid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.stockid;
                    command.Parameters.Add("salablegoodsid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.id;
                    command.Parameters.Add("qty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = item.aqty;
                    command.ExecuteNonQuery();
                    bundles.items.Find(e => e.barcode == item.barcode).toid = item.stockid;
                }
                else
                {
                    command = GetCommand(SQL_Inward2WarehouseReduce, conn, transaction);
                    command.Parameters.Add("stockid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.stockid;
                    command.Parameters.Add("salablegoodsid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.id;
                    command.Parameters.Add("qty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = item.aqty * -1;
                    command.ExecuteNonQuery();
                }
            }

            if (shortageStocks.Count > 0)
            {

                if (disableuanvalidation)
                {
                    throw new Exception(String.Join("<br>", errorMessage.Values.ToList()));
                }

                if ((!InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_fixed_barcodes ||
                    InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_hyper_uan))
                {
                    throw new Exception(String.Join("<br>", errorMessage.Values.ToList()));
                }

                List<Tuple<string, string>> uanbarcodes = new List<Tuple<string, string>>();
                string uanquery = "select barcode,serialno from stock where id in (" + string.Join(",", salablegoods.ConvertAll(x => x.stockid)) + ")";
                using (var reader = ExecuteReader(uanquery, null))
                {
                    while (reader.Read())
                    {
                        uanbarcodes.Add(new Tuple<string, string>(reader["barcode"].ToString(), reader["serialno"] == DBNull.Value ? "" : reader["serialno"].ToString()));
                    }
                }
                var missinguannps = uanbarcodes.FindAll(x => string.IsNullOrEmpty(x.Item2));
                if (missinguannps.Count > 0)
                {
                    List<string> displaymessage = new List<string>();
                    foreach (var muan in missinguannps)
                    {
                        if (errorMessage.ContainsKey(muan.Item1))
                            displaymessage.Add(muan.Item2);
                    }
                    throw new Exception(String.Join("<br>", displaymessage));
                }

                foreach (var uanitem in uanbarcodes.GroupBy(x => x.Item2))
                {
                    var barcodes = uanitem.ToList().ConvertAll(x => x.Item1);
                    var available = salablegoods.FindAll(x => barcodes.Contains(x.barcode));
                    var requiredqty = available.Sum(x => x.aqty);
                    var adjusteduans = AdjustUANBarcodes(transaction, conn, uanitem.Key, bundles.fromlocationid, requiredqty);
                    var bundlitems = bundles.items.FindAll(x => barcodes.Contains(x.barcode));

                    bundles.items.RemoveAll(x => barcodes.Contains(x.barcode));

                    bundles.items.AddRange(adjusteduans.ConvertAll(x =>
                    {
                        return new Bundleitem()
                        {
                            id = x.salablegoodsid,
                            stockid = x.stockid,
                            barcode = x.barcode,
                            aqty = x.qty,
                            iqty = x.qty,
                            qty = x.qty,
                            itemid = bundlitems.First().itemid,
                            lrreference = bundlitems.First().lrreference,
                            suppliername = bundlitems.First().suppliername,
                            supplierid = bundlitems.First().supplierid
                        };
                    }));

                    salablegoods.RemoveAll(x => barcodes.Contains(x.barcode));

                    salablegoods.AddRange(adjusteduans.ConvertAll(x =>
                    {
                        return new Salablegoods()
                        {
                            id = x.salablegoodsid,
                            stockid = x.stockid,
                            barcode = x.barcode,
                            aqty = x.qty
                        };
                    }));
                }
                InwardGoods2WareHouse(transaction, conn, salablegoods, receivedon, bundles, true);
            }
        }

        public class ConflictUAN
        {
            public string barcode { get; set; }
            public decimal qty { get; set; }
            public long stockid { get; set; }
            public long salablegoodsid { get; set; }
        }
        private List<ConflictUAN> AdjustUANBarcodes(NpgsqlTransaction transaction, NpgsqlConnection conn, string serialno, long locationid, decimal aaqty)
        {
            if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_fixed_barcodes &&
                !InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_hyper_uan)
            {
                string querystockuan = "select stock.id,salablegoods.id,salablegoods.barcode,iqty-(oqty+jqty+dqty+aqty) from stock " +
                                       "inner join salablegoods on salablegoods.stockid = stock.id " +
                                       "where stock.serialno = @serialno and iqty>(oqty+jqty+dqty+aqty) " +
                                       "and salablegoods.locationid = @locationid " +
                                       "order by iqty-(oqty+jqty+dqty+aqty) desc";
                List<ConflictUAN> listofstock = new List<ConflictUAN>();
                SortedList<string, object> param = new SortedList<string, object>();
                param.Add("@serialno", serialno);
                param.Add("@locationid", locationid);
                using (var reader = ExecuteReader(querystockuan, param, conn, transaction))
                {
                    while (reader.Read())
                    {
                        listofstock.Add(new ConflictUAN()
                        {
                            stockid = reader.GetInt64(0),
                            salablegoodsid = reader.GetInt64(1),
                            barcode = reader.GetString(2),
                            qty = reader.GetDecimal(3)
                        });

                        if (listofstock.Sum(e => e.qty) >= aaqty)
                        {
                            listofstock.Last().qty -= aaqty - listofstock.Sum(e => e.qty);
                            break;
                        }
                    }
                }


                if (listofstock.Sum(e => e.qty) < aaqty &&
                    !InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_supportuannegative)
                {
                    throw new Exception(serialno + " item don't have required stock (" + (aaqty).ToString() + ")");
                }


                if (listofstock.Sum(e => e.qty) < aaqty &&
                   InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_supportuannegative)
                {
                    listofstock.Add(new ConflictUAN()
                    {
                        barcode = listofstock.First().barcode,
                        qty = aaqty - listofstock.Sum(e => e.qty),
                        salablegoodsid = listofstock.First().salablegoodsid,
                        stockid = listofstock.First().stockid
                    });
                }
                return listofstock;
            }
            return new List<ConflictUAN>();

        }
        public class SWAPgoods
        {
            public long fromid { get; set; }
            public long toid { get; set; }
            public decimal qty { get; set; }
            public decimal shortage { get; internal set; }
            public string serialno { get; internal set; }
            public string barcode { get; internal set; }
            public SWAPgoods Parent { get; set; }
        }
        public void SwapGoods(NpgsqlTransaction transaction, NpgsqlConnection conn,
            List<Salablegoods> salablegoods, DateTime receivedon,
            long fromlocationid, long tolocationid, Bundles bundle)
        {
            var fromstock = Search(new SalablegoodsCreteria()
            {
                locationid = fromlocationid,
                ids = salablegoods.ConvertAll<long>(e => e.id)
            }, conn, transaction);


            /*fromstock.ForEach(x =>
            {
                if(!salablegoods.Exists(y=>y.barcode == x.barcode && y.id != x.id))
                {
                    salablegoods.Find(y => y.barcode == x.barcode && y.id != x.id).barcode = x.barcode;
                }
            });*/

            var missingsources = salablegoods.FindAll(x => !fromstock.Exists(y => y.id == x.id));
            if (missingsources.Count > 0)
            {
                throw new Exception("Some of the source barcode is missing or scanned wrong location barcodes");
            }

            var tostock = Search(new SalablegoodsCreteria()
            {
                locationid = tolocationid,
                barcodes = salablegoods.ConvertAll<string>(e => WebAPI.Data.Utility.SafeSQL(e.barcode))
            }, conn, transaction);

            if (tostock.Exists(e => e.aqty > 0 && e.producttype == 6))
            {
                throw new Exception("Mobile can't exceed the stock more than 1 - " + tostock.First(e => e.aqty > 0 && e.producttype == 6).barcode);
            }

            List<SWAPgoods> swap = new List<SWAPgoods>();
            var swaplocation = tostock.FindAll(e => fromstock.Exists(x => x.barcode == e.barcode));
            foreach (var item in swaplocation)
            {
                swap.Add(new SWAPgoods()
                {
                    barcode = item.barcode,
                    fromid = fromstock.Find(e => e.barcode == item.barcode).id,
                    toid = tostock.Find(e => e.barcode == item.barcode).id,
                    qty = salablegoods.Find(e => e.barcode == item.barcode).aqty
                });
                bundle.items.Find(e => e.barcode == item.barcode).toid = swap.Last().toid;
            }
            isMigration = true;
            var newsalablegoods = fromstock.FindAll(e => !tostock.Exists(x => x.barcode == e.barcode));
            foreach (var item in newsalablegoods)
            {
                var citem = item.Clone();
                citem.id = 0;
                citem.createdby = Context.UserID;
                citem.createdon = DateTimeNow;
                citem.bundleid = bundle.id;
                citem.oqty = 0;
                citem.iqty = 0;
                citem.dqty = 0;
                citem.jqty = 0;
                citem.aqty = 0;
                citem.pqty = 0;
                citem.tiqty = 0;
                citem.toqty = 0;
                citem.locationid = tolocationid;

                if (Services.ConfigManager.Instance(Context.OrganizationID).Config.system_invcol_pricetype &&
                    bundle.pricetype > 0 && bundle.items.Exists(x => x.stockid == citem.stockid) &&
                    bundle.items.Find(x => x.stockid == citem.stockid).uprice != 0)
                {
                    citem.price = bundle.items.Find(x => x.stockid == citem.stockid).uprice;
                }

                if (bundle.iscompanytransfer && bundle.tocompanyid > 0)
                    citem.companyid = bundle.tocompanyid;

                if (!Insert(transaction, conn, citem))
                    throw new Exception(citem.barcode + " don't have enough stock to transfer.");

                swap.Add(new SWAPgoods()
                {
                    barcode = citem.barcode,
                    fromid = fromstock.Find(e => e.barcode == citem.barcode).id,
                    toid = citem.id,
                    qty = salablegoods.Find(e => e.barcode == citem.barcode).aqty
                });
                bundle.items.Find(e => e.barcode == citem.barcode).toid = citem.id;
                tostock.Add(citem);
            }

            bool storeswaperror = false;
            List<SWAPgoods> swappending = new List<SWAPgoods>();
            List<SWAPgoods> swapalternate = new List<SWAPgoods>();
            if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_fixed_barcodes &&
                    !InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_hyper_uan)
            {
                storeswaperror = true;
            }

            if(storeswaperror)
            {
                var duplicatebarcodes = bundle.items.GroupBy(x => x.id).Any(y => y.Count() > 1);
                if(duplicatebarcodes)
                {
                    throw new Exception("Duplicate rows can't be managed re-bundle again " +
                        string.Join(",", bundle.items.GroupBy(x => x.id).ToList().FindAll(y => y.Count() > 1).ConvertAll(z => z.Key)));
                }
            }

            foreach (var item in swap)
            {
                if (item.fromid == item.toid)
                {
                    NpgsqlCommand command = GetCommand(SQL_SwapStockCompany, conn, transaction);
                    command.Parameters.Add("fromid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.fromid;
                    command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = bundle.tocompanyid;
                    command.ExecuteNonQuery();
                }
                else
                {
                    NpgsqlCommand command = GetCommand(SQL_SwapStockQTYReduce, conn, transaction);
                    command.Parameters.Add("fromid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.fromid;
                    command.Parameters.Add("toid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.toid;
                    command.Parameters.Add("qty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = item.qty;
                    using (System.Data.IDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var afterreduceiqty = Convert.ToDecimal(reader[0]);
                            if (afterreduceiqty < 0)
                            {
                                string barcode = reader["barcode"] == DBNull.Value ? "" : Convert.ToString(reader["barcode"]);
                                string serialno = reader["serialno"] == DBNull.Value ? "" : Convert.ToString(reader["serialno"]);
                                if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig.ContainsKey(fromlocationid) &&
                                InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[fromlocationid].sale_negativestock)
                                {
                                    if (storeswaperror)
                                    {
                                        command = GetCommand(SQL_SwapStockQTYAlter, conn, transaction);
                                        command.Parameters.Add("fromid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.fromid;
                                        command.Parameters.Add("toid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.toid;
                                        command.Parameters.Add("qty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = afterreduceiqty;
                                        item.qty -= Math.Abs(afterreduceiqty);
                                        item.shortage = Math.Abs(afterreduceiqty);
                                        item.barcode = bundle.items.Find(e => e.id == item.fromid).barcode;
                                        item.serialno = bundle.items.Find(e => e.id == item.fromid).serialno;
                                        bundle.items.Find(e => e.id == item.fromid).qty -= Math.Abs(afterreduceiqty);
                                        bundle.items.Find(e => e.id == item.fromid).aqty -= Math.Abs(afterreduceiqty);
                                        if (string.IsNullOrEmpty(item.serialno))
                                        {
                                            throw new Exception(@"Expected stock not available in " + 
                                                    Convert.ToString(reader[1]).ToString() + " shortage " + Math.Abs(afterreduceiqty).ToString());
                                        }
                                        swappending.Add(item);
                                    }
                                    else
                                    {
                                        throw new Exception("Expected stock not available in " + Convert.ToString(reader[1]).ToString() +
                                             " shortage " + Math.Abs(afterreduceiqty).ToString());
                                    }
                                }
                            }
                        }
                    }
                    command = GetCommand(SQL_SwapStockQTYIncrease, conn, transaction);
                    command.Parameters.Add("fromid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.fromid;
                    command.Parameters.Add("toid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.toid;
                    command.Parameters.Add("qty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = item.qty;
                    using (System.Data.IDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var afterreduceiqty = Convert.ToDecimal(reader[0]);
                            if (afterreduceiqty < 0)
                            {
                                if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig.ContainsKey(fromlocationid) &&
                                InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[fromlocationid].sale_negativestock)
                                    throw new Exception("Expected stock not available in " + Convert.ToString(reader[1]).ToString() +
                                         " shortage " + Math.Abs(afterreduceiqty).ToString());
                            }
                        }
                    }
                }
            }
            if (storeswaperror && swappending.Count > 0)
            {

                foreach (var storestock in swappending)
                {

                    string searchstock = @"select salablegoods.id,salablegoods.barcode,
                                           salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty+salablegoods.aqty) stock 
                                           from salablegoods 
                                           inner join stock on stock.id=salablegoods.stockid
                                           where stock.serialno =@serialno and salablegoods.iqty-(salablegoods.oqty++salablegoods.jqty+salablegoods.dqty+salablegoods.aqty)>0 and
                                           salablegoods.locationid=@fromlocationid and salablegoods.isactive=true";

                    var param = new SortedList<string, object>();
                    param.Add("fromlocationid", bundle.fromlocationid);
                    param.Add("serialno", storestock.serialno);

                    using (var reader = ExecuteReader(searchstock, param, conn, transaction))
                    {
                        while (reader.Read())
                        {
                            if (reader["stock"] == DBNull.Value) continue;
                            decimal stock = Convert.ToDecimal(reader["stock"]);
                            long id = Convert.ToInt64(reader["id"]);

                            if(swapalternate.Exists(y=>y.fromid == id))
                            {
                                stock -= swapalternate.Sum(y => y.fromid == id ? y.qty : 0);
                                if (stock <= 0) continue;
                            }

                            string barcode = Convert.ToString(reader["barcode"]);
                            if (storestock.shortage <= stock)
                            {
                                swapalternate.Add(new SWAPgoods()
                                {
                                    fromid = id,
                                    serialno = storestock.serialno,
                                    barcode = barcode,
                                    qty = storestock.shortage,
                                    Parent = storestock
                                });
                                storestock.shortage = 0;
                                break;
                            }
                            else
                            {
                                swapalternate.Add(new SWAPgoods()
                                {
                                    fromid = id,
                                    serialno = storestock.serialno,
                                    barcode = barcode,
                                    qty = stock,
                                    Parent = storestock,
                                });
                                storestock.shortage -= stock;
                            }
                        }
                    }
                }

                if (swappending.Exists(x => x.shortage != 0))
                {
                    throw new Exception("Stock shortage to transfer - " + string.Join(",", swappending.FindAll(x => x.shortage != 0).
                         ConvertAll(x => x.serialno + " : " + x.shortage)));
                }

                if (swapalternate.Count > 0)
                {
                    fromstock = Search(new SalablegoodsCreteria()
                    {
                        locationid = fromlocationid,
                        barcodes = swapalternate.ConvertAll<string>(e => WebAPI.Data.Utility.SafeSQL(e.barcode))
                    }, conn, transaction);

                    tostock = Search(new SalablegoodsCreteria()
                    {
                        locationid = tolocationid,
                        barcodes = swapalternate.ConvertAll<string>(e => WebAPI.Data.Utility.SafeSQL(e.barcode))
                    }, conn, transaction);

                    newsalablegoods = fromstock.FindAll(e => !tostock.Exists(x => x.barcode == e.barcode));
                    foreach (var item in newsalablegoods)
                    {
                        var citem = item.Clone();
                        citem.id = 0;
                        citem.createdby = Context.UserID;
                        citem.createdon = DateTimeNow;
                        citem.bundleid = bundle.id;
                        citem.oqty = 0;
                        citem.iqty = 0;
                        citem.dqty = 0;
                        citem.jqty = 0;
                        citem.pqty = 0;
                        citem.tiqty = 0;
                        citem.toqty = 0;
                        citem.locationid = tolocationid;

                        if (Services.ConfigManager.Instance(Context.OrganizationID).Config.system_invcol_pricetype &&
                            bundle.pricetype > 0 && bundle.items.Exists(x => x.stockid == citem.stockid) &&
                            bundle.items.Find(x => x.stockid == citem.stockid).uprice != 0)
                        {
                            citem.price = bundle.items.Find(x => x.stockid == citem.stockid).uprice;
                        }
                        if (bundle.iscompanytransfer && bundle.tocompanyid > 0)
                            citem.companyid = bundle.tocompanyid;
                        Insert(transaction, conn, citem);
                        tostock.Add(citem);
                    }

                    swaplocation = tostock.FindAll(e => fromstock.Exists(x => x.barcode == e.barcode));
                    foreach (var item in swaplocation)
                    {
                        var swapitem = swapalternate.Find(x => x.barcode == item.barcode);
                        swapitem.fromid = fromstock.Find(x => x.barcode == item.barcode).id;
                        swapitem.toid = item.id;

                        var newbundleitem = JsonConvert.DeserializeObject<Bundleitem>(JsonConvert.SerializeObject(bundle.items.Find(e => e.barcode == swapitem.Parent.barcode)));
                        newbundleitem.barcode = swapitem.barcode;
                        newbundleitem.qty = swapitem.qty;
                        newbundleitem.aqty = swapitem.qty;
                        newbundleitem.id = swapitem.fromid;
                        newbundleitem.toid = swapitem.toid;
                        newbundleitem.stockid = fromstock.Find(x => x.barcode == item.barcode).stockid;
                        bundle.items.Add(newbundleitem);
                    }

                    isMigration = true;
                    foreach (var item in swapalternate)
                    {
                        NpgsqlCommand command = GetCommand(SQL_SwapStockQTYReduce, conn, transaction);
                        command.Parameters.Add("fromid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.fromid;
                        command.Parameters.Add("toid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.toid;
                        command.Parameters.Add("qty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = item.qty;
                        using (System.Data.IDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var afterreduceiqty = Convert.ToDecimal(reader[0]);
                                string barcode = reader["barcode"] == DBNull.Value ? "" : Convert.ToString(reader["barcode"]);
                                string serialno = reader["serialno"] == DBNull.Value ? "" : Convert.ToString(reader["serialno"]);
                                if (afterreduceiqty < 0)
                                {
                                    if (!string.IsNullOrEmpty(serialno))
                                    {
                                        var uanqty = bundle.items.Sum(x => x.serialno == serialno ? x.aqty : 0);
                                        throw new Exception("Tried with id="+ item.fromid +" - Barcode = "+ barcode+" but failed, Try Again - Expected stock not available in " + serialno+"("+Convert.ToString(reader[1]).ToString()+")" +
                                             " shortage " + Math.Abs(afterreduceiqty).ToString() +" , Total Require stock "+uanqty);
                                    }
                                    else
                                    {
                                        throw new Exception("Try Again - Expected stock not available in " + Convert.ToString(reader[1]).ToString() +
                                             " shortage " + Math.Abs(afterreduceiqty).ToString());
                                    }
                                }
                            }
                        }
                        command = GetCommand(SQL_SwapStockQTYIncrease, conn, transaction);
                        command.Parameters.Add("fromid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.fromid;
                        command.Parameters.Add("toid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.toid;
                        command.Parameters.Add("qty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = item.qty;
                        command.ExecuteNonQuery();
                    }
                }
            }
        }


        internal bool Rollback(List<Bundleitem> items, long modifiedby, NpgsqlConnection conn, NpgsqlTransaction transaction)
        {
            foreach (var item in items)
            {
                NpgsqlCommand command = GetCommand(SQL_Inward2WarehouseReduce, conn, transaction);
                command.Parameters.Add("stockid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.stockid;
                command.Parameters.Add("salablegoodsid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.toid;
                command.Parameters.Add("qty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = item.transfered * -1;
                command.ExecuteNonQuery();

                command = GetCommand(SQL_Inward2WarehouseIncrease, conn, transaction);
                command.Parameters.Add("stockid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.stockid;
                command.Parameters.Add("salablegoodsid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.toid;
                command.Parameters.Add("qty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = item.transfered * -1;
                command.ExecuteNonQuery();
            }
            return true;
        }


        public Salablegoods Insert(Salablegoods oSalablegoods)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);

                    Insert(transaction, conn, oSalablegoods);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oSalablegoods);


            }

            return oSalablegoods;
        }

        public bool Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Salablegoods oSalablegoods)
        {
            return Insert(transaction, conn, oSalablegoods, false);
        }
        public bool Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Salablegoods oSalablegoods, bool validatenegativestock)
        {

            if (oSalablegoods.price == 0)
                throw new Exception("Price Can't be zero value");

            //if (Services.ConfigManager.Instance(Context.OrganizationID).Config.system_multicompany && oSalablegoods.companyid <= 0)
            if (oSalablegoods.companyid <= 0)
            {
                if (isMigration)
                    oSalablegoods.companyid = 1;
                else
                    throw new Exception("Company Information missing while Creating Saleable Stock.");
            }
            if (oSalablegoods.stockid <= 0)
                throw new Exception("Stock(Purchase) Information missing while Creating Saleable Stock.");

            if (oSalablegoods.oqty + oSalablegoods.jqty + oSalablegoods.dqty + oSalablegoods.aqty > (oSalablegoods.iqty))
            {
                if (isMigration)
                {
                    Console.WriteLine("Out qty can't greater than In Qty - " + oSalablegoods.barcode);
                    if (oSalablegoods.iqty < 0)
                        oSalablegoods.iqty = 0;
                    oSalablegoods.iqty += oSalablegoods.oqty - oSalablegoods.iqty - oSalablegoods.jqty - oSalablegoods.dqty- +oSalablegoods.aqty;
                    //oSalablegoods.oqty = oSalablegoods.iqty - oSalablegoods.dqty;
                }
                else
                {
                    WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(new Exception("Out qty can't greater than In Qty - " + oSalablegoods.barcode), WebAPI.Common.Logger.LogType.Error, oSalablegoods);
                    return false;
                }
            }
            oSalablegoods.modifiedon = DateTimeNow;
            NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oSalablegoods.isactive;
            command.Parameters.Add("received", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oSalablegoods.received;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oSalablegoods.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("iqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oSalablegoods.iqty;
            command.Parameters.Add("oqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oSalablegoods.oqty;
            command.Parameters.Add("dqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oSalablegoods.dqty;
            command.Parameters.Add("jqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oSalablegoods.jqty;
            command.Parameters.Add("stockid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSalablegoods.stockid;
            command.Parameters.Add("barcode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSalablegoods.barcode == null ? "" : oSalablegoods.barcode;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSalablegoods.locationid;
            command.Parameters.Add("bundleid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSalablegoods.bundleid;
            command.Parameters.Add("itemid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSalablegoods.itemid;
            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSalablegoods.companyid;
            command.Parameters.Add("supplierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSalablegoods.supplierid;
            command.Parameters.Add("inwardon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oSalablegoods.inwardon;
            command.Parameters.Add("price", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oSalablegoods.price;
            command.Parameters.Add("discount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oSalablegoods.discount;
            command.Parameters.Add("additionaldiscount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oSalablegoods.additionaldiscount;
            command.Parameters.Add("ispurchaselocation", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oSalablegoods.ispurchaselocation;
            command.Parameters.Add("tiqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oSalablegoods.tiqty;
            command.Parameters.Add("pqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oSalablegoods.pqty;
            command.Parameters.Add("toqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oSalablegoods.toqty;
            command.Parameters.Add("rqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oSalablegoods.rqty;
            command.Parameters.Add("salereturnqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oSalablegoods.salereturnqty;
            command.Parameters.Add("costprice", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oSalablegoods.costprice;
            command.Parameters.Add("icostprice", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oSalablegoods.icostprice;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oSalablegoods.id = Convert.ToInt64(reader[0]);
                }
            }

            if ((oSalablegoods.costprice == 0 || oSalablegoods.icostprice == 0) && oSalablegoods.stockid > 0)
            {
                command = GetCommand("select buyingprice,ibuyingprice from stock where id = " + oSalablegoods.stockid, conn, transaction);
                decimal costprice = 0;
                decimal icostprice = 0;
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        costprice = reader.IsDBNull(0) ? (decimal)0 : reader.GetDecimal(0);
                        icostprice = reader.IsDBNull(1) ? (decimal)0 : reader.GetDecimal(1);
                    }
                }
                if (costprice > 0)
                {
                    command = GetCommand("update salablegoods set costprice=" + costprice + ",icostprice=" + icostprice + " where id = " + oSalablegoods.id, conn, transaction);
                    command.ExecuteNonQuery();
                }
            }

            if (!isMigration)
            {
                command = GetCommand(oSalablegoods.ispurchaselocation ? SQL_UpdateTransferStock.Replace(",transferedout=COALESCE(transferedout,0)+@transfered", "") :
                    SQL_UpdateTransferStock, conn, transaction);
                command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
                command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSalablegoods.stockid;
                command.Parameters.Add("transfered", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oSalablegoods.iqty;
                if (validatenegativestock)
                {
                    bool isvalid = true;
                    using (System.Data.IDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var qty = reader.IsDBNull(0) ? 0 : reader.GetDecimal(0);
                            var returnqty = reader.IsDBNull(1) ? 0 : reader.GetDecimal(1);
                            var damageqty = reader.IsDBNull(2) ? 0 : reader.GetDecimal(2);
                            var transfered = reader.IsDBNull(3) ? 0 : reader.GetDecimal(3);
                            var journalqty = reader.IsDBNull(4) ? 0 : reader.GetDecimal(4);
                            if (qty - (returnqty + damageqty + transfered + journalqty) < 0)
                            {
                                isvalid = false;
                            }
                        }
                    }

                    if (!isvalid)
                    {
                        command.CommandText = SQL_ReversTransferStock;
                        command.Parameters["transfered"].Value = oSalablegoods.iqty * -1;
                        command.Parameters.Add("salablegoodsid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSalablegoods.id;
                        command.ExecuteNonQuery();
                        return false;
                    }
                }
                else
                {
                    command.ExecuteNonQuery();
                }
            }
            return true;
        }

        public bool isMigration = false;


        public Salablegoods Update(Salablegoods oSalablegoods, Salablegoods existingSalablegoods)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oSalablegoods, existingSalablegoods);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oSalablegoods);


            }

            return oSalablegoods;
        }

        public bool Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Salablegoods oSalablegoods,
            Salablegoods existingSalablegoods)
        {
            if (oSalablegoods.oqty + oSalablegoods.dqty + oSalablegoods.jqty + oSalablegoods.aqty > (oSalablegoods.iqty))
            {
                if (isMigration)
                {
                    Console.WriteLine("Out qty can't greater than In Qty - " + oSalablegoods.barcode);
                    oSalablegoods.iqty += oSalablegoods.oqty - oSalablegoods.iqty - oSalablegoods.dqty - oSalablegoods.jqty - oSalablegoods.aqty;
                    //oSalablegoods.oqty = oSalablegoods.iqty - oSalablegoods.dqty;
                }
                else
                {
                    WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(new Exception("Out qty can't greater than In Qty - " + oSalablegoods.barcode), WebAPI.Common.Logger.LogType.Error, oSalablegoods);
                    return false;
                }
            }
            NpgsqlCommand command = GetCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSalablegoods.id;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oSalablegoods.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSalablegoods.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oSalablegoods.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oSalablegoods.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("iqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oSalablegoods.iqty;
            command.Parameters.Add("oqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oSalablegoods.oqty;
            command.Parameters.Add("dqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oSalablegoods.dqty;
            command.Parameters.Add("jqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oSalablegoods.jqty;
            command.Parameters.Add("aqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oSalablegoods.aqty;
            command.Parameters.Add("tiqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oSalablegoods.tiqty;
            command.Parameters.Add("toqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oSalablegoods.toqty;
            command.Parameters.Add("stockid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSalablegoods.stockid;
            command.Parameters.Add("barcode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSalablegoods.barcode == null ? "" : oSalablegoods.barcode;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSalablegoods.locationid;
            command.Parameters.Add("bundleid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSalablegoods.bundleid;
            command.Parameters.Add("itemid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSalablegoods.itemid;
            command.Parameters.Add("received", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oSalablegoods.received;
            command.Parameters.Add("price", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oSalablegoods.price;
            command.Parameters.Add("discount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oSalablegoods.discount;
            command.Parameters.Add("additionaldiscount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oSalablegoods.additionaldiscount;
            command.Parameters.Add("costprice", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oSalablegoods.costprice;
            command.Parameters.Add("icostprice", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oSalablegoods.icostprice;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oSalablegoods.version = Convert.ToInt32(reader[0]);
                }
                else
                {
                    return false;
                }
            }

            if (existingSalablegoods != null && existingSalablegoods.iqty != oSalablegoods.iqty)
            {
                command = GetCommand(SQL_UpdateTransferStock, conn, transaction);
                command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
                command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSalablegoods.stockid;
                command.Parameters.Add("transfered", NpgsqlTypes.NpgsqlDbType.Numeric).Value = (oSalablegoods.iqty - existingSalablegoods.iqty);
                bool rollback = false;
                decimal rollbackqty = 0;
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        var qty = reader.IsDBNull(0) ? 0 : reader.GetDecimal(0);
                        var returnqty = reader.IsDBNull(1) ? 0 : reader.GetDecimal(1);
                        var damageqty = reader.IsDBNull(2) ? 0 : reader.GetDecimal(2);
                        var transfered = reader.IsDBNull(3) ? 0 : reader.GetDecimal(3);
                        var journalqty = reader.IsDBNull(4) ? 0 : reader.GetDecimal(4);
                        if (qty - (returnqty + damageqty + transfered + journalqty) < 0)
                        {
                            rollback = true;
                            rollbackqty = (returnqty + damageqty + transfered + journalqty) - qty;
                        }
                    }
                }

                if (rollback)
                {
                    if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_bundledisablerollback)
                    {
                        command.CommandText = SQL_ReversTransferStock;
                        command.Parameters["transfered"].Value = rollbackqty * -1;
                        command.Parameters.Add("salablegoodsid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSalablegoods.id;
                        command.ExecuteNonQuery();
                    }
                    else
                    {
                        command.CommandText = SQL_ReversTransferStock;
                        command.Parameters["transfered"].Value = (oSalablegoods.iqty - existingSalablegoods.iqty) * -1;
                        command.Parameters.Add("salablegoodsid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSalablegoods.id;
                        command.ExecuteNonQuery();
                        return false;
                    }
                }
            }
            return true;
        }

        //public Salablegoods Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Salablegoods oSalablegoods)
        //{
        //    if (oSalablegoods.oqty > oSalablegoods.iqty)
        //    {
        //        //throw new Exception("Out qty can't greater than In Qty");
        //        Console.WriteLine("Out qty can't greater than In Qty");
        //    }
        //    NpgsqlCommand command = GetCommand(SQL_Update, conn, transaction);
        //    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSalablegoods.id;
        //    command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oSalablegoods.isactive;
        //    command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
        //    command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
        //    command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oSalablegoods.version;
        //    command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oSalablegoods.createdon;
        //    command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oSalablegoods.modifiedon;
        //    command.Parameters.Add("iqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oSalablegoods.iqty;
        //    command.Parameters.Add("oqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oSalablegoods.oqty;
        //    command.Parameters.Add("dqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oSalablegoods.dqty;
        //    command.Parameters.Add("stockid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSalablegoods.stockid;
        //    command.Parameters.Add("barcode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSalablegoods.barcode == null ? "" : oSalablegoods.barcode;
        //    command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSalablegoods.locationid;
        //    command.Parameters.Add("bundleid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSalablegoods.bundleid;
        //    command.Parameters.Add("itemid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSalablegoods.itemid;
        //    command.Parameters.Add("received", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oSalablegoods.received;
        //    command.Parameters.Add("price", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oSalablegoods.price;
        //    command.Parameters.Add("discount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oSalablegoods.discount;
        //    command.Parameters.Add("additionaldiscount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oSalablegoods.additionaldiscount;

        //    using (System.Data.IDataReader reader = command.ExecuteReader())
        //    {
        //        if (reader.Read())
        //        {
        //            oSalablegoods.version = Convert.ToInt32(reader[0]);
        //        }
        //    }
        //    return oSalablegoods;
        //}


        //public bool Inward(NpgsqlTransaction transaction, NpgsqlConnection conn, List<Salablegoods> list, DateTime inwardon, long id)
        //{
        //    if (list.All(e => e.iqty == e.aqty))
        //    {
        //        var command = GetCommand(SQL_UpdateInwards, conn, transaction);
        //        command.Parameters.Add("bundleid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = id;
        //        return command.ExecuteNonQuery() > 0;
        //    }
        //    else
        //    {
        //        foreach (var item in list)
        //        {
        //            var command = GetCommand(SQL_UpdateInwardItem, conn, transaction);
        //            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.id;
        //            command.Parameters.Add("iqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = item.aqty;
        //            command.Parameters.Add("inwardon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = inwardon;
        //            if (command.ExecuteNonQuery() > 0)
        //            {
        //                if (item.aqty < item.iqty)
        //                {
        //                    command = GetCommand(SQL_UpdateTransferStock, conn, transaction);
        //                    command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
        //                    command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
        //                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.stockid;
        //                    command.Parameters.Add("transfered", NpgsqlTypes.NpgsqlDbType.Numeric).Value = (item.aqty - item.iqty);
        //                    command.ExecuteNonQuery();
        //                }
        //            }
        //        }
        //        return true;
        //    }
        //}

        public bool Update(NpgsqlTransaction transaction, NpgsqlConnection conn, List<Salablegoods> list, long id)
        {
            var existingItems = SearchAll(id);
            if (existingItems != null && existingItems.Count > 0)
            {
                var deletedItems = existingItems.FindAll(e => !list.Exists(f => f.id == e.id));
                foreach (var item in deletedItems)
                    Delete(transaction, conn, item);
            }

            foreach (var item in list)
            {
                if (item.id > 0)
                {
                    var existingItem = existingItems.Find(e => e.id == item.id);
                    Update(transaction, conn, item, existingItem);
                }
                else
                {
                    item.bundleid = id;
                    Insert(transaction, conn, item);
                }
            }
            return true;
        }

        public bool Delete(Salablegoods oSalablegoods)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oSalablegoods);

                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oSalablegoods);



            }
            return hasSuccessCount;

        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Salablegoods oSalablegoods)
        {

            NpgsqlCommand command = GetCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oSalablegoods.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oSalablegoods.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSalablegoods.id;
            int successCount = command.ExecuteNonQuery();

            command = GetCommand(SQL_UpdateTransferStock, conn, transaction);
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSalablegoods.stockid;
            command.Parameters.Add("transfered", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oSalablegoods.iqty * -1;
            command.ExecuteNonQuery();

            return successCount > 0;
        }

        public decimal UpdateStock(Salablegoods oSalablegoods)
        {
            string SQL_UpdateStock = @"update public.salablegoods set oqty=oqty+@oqty, version=version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where barcode=@barcode and locationid=@locationid RETURNING iqty-(oqty+dqty+jqty+aqty) qty";
            NpgsqlTransaction transaction = null;
            decimal outqty = 0;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    NpgsqlCommand command = GetCommand(SQL_UpdateStock, conn, transaction);
                    command.Parameters.Add("oqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oSalablegoods.oqty;
                    command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = -99;
                    command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
                    command.Parameters.Add("barcode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = WebAPI.Data.Utility.SafeSQL(oSalablegoods.barcode);
                    command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSalablegoods.locationid;
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            outqty = reader["qty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["qty"]);
                        }
                    }
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oSalablegoods);
            }
            return outqty;
        }
        public decimal GetStock(Salablegoods oSalablegoods)
        {
            string SQL_GetStock = @"select iqty-(oqty+dqty+jqty+aqty) qty from salablegoods where barcode=@barcode and locationid=@locationid";
            decimal outqty = 0;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    NpgsqlCommand command = GetCommand(SQL_GetStock, conn);
                    command.Parameters.Add("barcode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = WebAPI.Data.Utility.SafeSQL(oSalablegoods.barcode);
                    command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSalablegoods.locationid;
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            outqty = reader["qty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["qty"]);
                        }
                    }
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oSalablegoods);
            }
            return outqty;

        }

        internal List<BillIntegration.BillItem> FindGLNStock(long locationid, List<BillIntegration.BillItem> glnbillitems)
        {
            List<BillIntegration.BillItem> response = new List<BillIntegration.BillItem>();
            string query = @"select sum(iqty-(oqty+dqty+jqty+aqty)) stock,sum(olstock) olstock,serialno from salablegoods
                             inner join stock on stock.id = salablegoods.stockid
                             where salablegoods.hasstock=true and salablegoods.isactive=true and iqty-(oqty+dqty+jqty+aqty)>0 and
                             salablegoods.locationid =@locationid and stock.serialno in (" + string.Join(",", glnbillitems.ConvertAll(x => "'" + x.gln + "'")) + ")";

            var param = new SortedList<string, object>();
            param.Add("locationid", locationid);
            using (var reader = ExecuteReader(query, param))
            {
                while (reader.Read())
                {
                    var item = new BillIntegration.BillItem();
                    item.gln = reader["serialno"] == DBNull.Value ? "" : reader["serialno"].ToString();
                    item.stockqty = reader["stock"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["stock"]);
                    item.onlinqty = reader["olstock"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["olstock"]);
                    item.soldqty = glnbillitems.Sum(x => x.gln == item.gln ? x.soldqty : 0);
                    response.Add(item);
                }
            }
            return response;
        }

        public List<Barcodedgoods> BatchListBySO(BatchPO item)
        {
            List<Barcodedgoods> barcodelist = new List<Barcodedgoods>();
            var uids = item.items.ConvertAll(x => x.uid).Distinct().ToList();
            string condition = " stock.serialno in (" + String.Join(",", uids.ConvertAll(x => "'" + x + "'")) + ") ";
            bool serialbased = true;
            if (!InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_fixed_barcodes &&
            InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_hyper_uan)
            {
                serialbased = false;
                condition = " stock.barcode in (" + String.Join(",", uids.ConvertAll(x => "'" + x + "'")) + ") ";
            }
            var query = SQL_Batchsearchbybrandid.Replace("items.brandid=@brandid", condition);
            using (NpgsqlConnection conn = CreateConnection())
            {
                conn.Open();
                NpgsqlCommand command = null;
                command = GetCommand(query, conn);
                command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.locationid;
                command.Parameters.Add("sixmonthdate", NpgsqlTypes.NpgsqlDbType.Date).Value = DateTimeNow.Date.AddMonths(-12);
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Barcodedgoods barcodedgoods = new Barcodedgoods();
                        barcodedgoods.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        barcodedgoods.received = reader["received"] == DBNull.Value ? false : Convert.ToBoolean(reader["received"]);
                        barcodedgoods.iqty = reader["iqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["iqty"].ToString());
                        barcodedgoods.oqty = reader["oqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["oqty"].ToString());
                        barcodedgoods.dqty = reader["dqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["dqty"].ToString());
                        barcodedgoods.jqty = reader["jqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["jqty"].ToString());
                        barcodedgoods.qty = barcodedgoods.iqty - (barcodedgoods.oqty + barcodedgoods.dqty + barcodedgoods.jqty);
                        barcodedgoods.price = reader["price"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["price"].ToString());
                        barcodedgoods.buyingprice = reader["buyingprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["buyingprice"].ToString());
                        barcodedgoods.discount = reader["discount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discount"].ToString());
                        barcodedgoods.additionaldiscount = reader["additionaldiscount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["additionaldiscount"].ToString());
                        barcodedgoods.stockid = reader["stockid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["stockid"].ToString());
                        barcodedgoods.companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"].ToString());
                        barcodedgoods.barcode = reader["barcode"] == DBNull.Value ? "" : reader["barcode"].ToString();
                        barcodedgoods.designid = reader["designid"] == DBNull.Value ? "" : reader["designid"].ToString();
                        barcodedgoods.brandname = reader["brandname"] == DBNull.Value ? "" : reader["brandname"].ToString();
                        barcodedgoods.sizename = reader["sizename"] == DBNull.Value ? "" : reader["sizename"].ToString();
                        barcodedgoods.colourname = reader["colourname"] == DBNull.Value ? "" : reader["colourname"].ToString();
                        barcodedgoods.brandname = reader["brandname"] == DBNull.Value ? "" : reader["brandname"].ToString();
                        barcodedgoods.itemid = reader["itemid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["itemid"].ToString());
                        barcodedgoods.taxid = reader["salestaxid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["salestaxid"].ToString());
                        barcodedgoods.sellingname = reader["sellingname"] == DBNull.Value ? "unknown" : reader["sellingname"].ToString();
                        barcodedgoods.serialno = reader["serialno"] == DBNull.Value ? "" : reader["serialno"].ToString();
                        barcodedgoods.imageid = reader["imageid"] == DBNull.Value ? null : reader["imageid"].ToString();
                        barcodedgoods.hsncode = reader["hsncode"] == DBNull.Value ? "" : reader["hsncode"].ToString();
                        barcodedgoods.productid = reader["productid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productid"].ToString());
                        barcodedgoods.stockisbit = reader["isbit"] == DBNull.Value ? false : Convert.ToBoolean(reader["isbit"]);
                        barcodedgoods.runtimeprice = reader["runtimeprice"] == DBNull.Value ? false : Convert.ToBoolean(reader["runtimeprice"]);
                        barcodedgoods.stockqty = reader["qty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["qty"].ToString());
                        barcodedgoods.pdpercent = reader["purchasediscountpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["purchasediscountpercentage"].ToString());
                        barcodedgoods.sellingmode = reader["sellingmode"] == DBNull.Value ? 0 : Convert.ToInt32(reader["sellingmode"]);
                        if (barcodedgoods.price == 0)
                        {
                            barcodedgoods.buyingprice = Math.Round(barcodedgoods.buyingprice * 115 / 100);
                        }
                        barcodelist.Add(barcodedgoods);
                    }
                }
                conn.Close();
            }

            List<Barcodedgoods> allocated = new List<Barcodedgoods>();
            foreach (var barcode in item.items.GroupBy(x => x.uid))
            {
                List<Barcodedgoods> available = null;
                List<Barcodedgoods> newallocated = new List<Barcodedgoods>();
                var neededqty = barcode.Sum(x => x.qty);
                if (serialbased)
                {
                    available = barcodelist.FindAll(x => x.serialno == barcode.Key);
                    barcodelist.RemoveAll(x => x.serialno == barcode.Key);
                }
                if (neededqty <= available.Sum(x => x.qty))
                {
                    for (var i = 0; i < available.Count; i++)
                    {
                        if (available[i].qty >= neededqty)
                        {
                            available[i].qty = neededqty;
                            newallocated.Add(available[i]);
                            break;
                        }
                        else
                        {
                            newallocated.Add(available[i]);
                        }
                    }
                }
                else
                {
                    newallocated = available;
                }
                allocated.AddRange(MapSOItems(barcode.ToList(), newallocated));
            }
            return allocated;
        }

        private List<Barcodedgoods> MapSOItems(List<BatchPO.Item> items,
            List<Barcodedgoods> newallocated)
        {
            List<Barcodedgoods> allocated = new List<Barcodedgoods>();
            foreach (var itm in items)
            {
                decimal qty = itm.qty;
                foreach (var bitm in newallocated.FindAll(x => x.qty > 0).ToList().OrderByDescending(x => x.qty))
                {
                    if (bitm.qty > qty)
                    {
                        Barcodedgoods barcode = JsonConvert.DeserializeObject<Barcodedgoods>(JsonConvert.SerializeObject(bitm));
                        barcode.qty = qty;
                        barcode.soitemid = itm.soitemid;
                        allocated.Add(barcode);
                        break;
                    }
                    else
                    {
                        Barcodedgoods barcode = JsonConvert.DeserializeObject<Barcodedgoods>(JsonConvert.SerializeObject(bitm));
                        barcode.soitemid = itm.soitemid;
                        qty = qty - barcode.qty;
                        allocated.Add(barcode);
                    }
                }
            }
            return allocated;
        }



        internal List<SellableGoodsCustomerHistory> SearchCustomerHistory(string scustomerid, string sproductid, string sdays)
        {
            string SQL_CustomerHistory = @"with brandwisedetails as
                                        (
                                            select billno,billdate,(billitems.receivable-billitems.tax)/billitems.qty billprice,
                                            billitems.qty,billitems.receivable,stock.buyingprice,
											lrinvoice.invoiceno,lrinvoice.invoicedate,suppliercompany.name suppliername,
                                            (billitems.discountpercentage+billitems.additionaldiscountpercentage) discountpercent,
	                                        row_number() OVER (PARTITION BY stock.productid,stock.designid,items.id ORDER BY billitems.id DESC) as ranknumber
                                            from bill
                                            inner join billitems on billitems.billid = bill.id and billitems.isactive=true
                                            inner join stock on stock.id = billitems.stockid
                                            inner join items on items.id = stock.itemid
											left outer join lrinvoiceitems on lrinvoiceitems.id = stock.invoiceitemid
                                            left outer join lrinvoice on lrinvoice.id = lrinvoiceid
											left outer join suppliercompany on suppliercompany.id = lrinvoice.suppliercompanyid
                                            where customerid = @customerid and bill.billdate>@billdate and stock.productid=@productid
                                            and billtype != 9 and billitems.qty>0 and bill.isactive=true and billitems.isactive=true
                                        )
                                        select * from brandwisedetails
                                        where ranknumber<3";

            NpgsqlConnection conn = CreateConnection();
            var result = new List<SellableGoodsCustomerHistory>();
            try
            {
                long customerid = 0;
                long productid = 0;
                int days = 0;


                if (!int.TryParse(sdays, out days))
                    return result;

                if (!long.TryParse(scustomerid, out customerid))
                    return result;


                if (!long.TryParse(sproductid, out productid))
                    return result;

                conn.Open();
                NpgsqlCommand command = null;
                command = GetCommand(SQL_CustomerHistory, conn);
                command.Parameters.Add("customerid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = customerid;
                command.Parameters.Add("productid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = productid;
                command.Parameters.Add("billdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow.AddDays(days * -1);

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        SellableGoodsCustomerHistory barcodedgoods = new SellableGoodsCustomerHistory();
                        barcodedgoods.qty = reader["qty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["qty"].ToString());
                        barcodedgoods.receivable = reader["receivable"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["receivable"].ToString());
                        barcodedgoods.buyingprice = reader["buyingprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["buyingprice"].ToString());
                        barcodedgoods.billprice = reader["billprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["billprice"].ToString());
                        barcodedgoods.receivable = reader["receivable"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["receivable"].ToString());
                        barcodedgoods.discountpercent = reader["discountpercent"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discountpercent"].ToString());
                        //if (qty > 0 && buyingprice > 0)
                        //    barcodedgoods.markup = Math.Round(((receivable / qty) - buyingprice) / buyingprice * 100, 2);
                        //if (qty > 0 && receivable > 0)
                        //    barcodedgoods.markdown = Math.Round(((receivable / qty) - buyingprice) / receivable * 100, 2);
                        barcodedgoods.billedon = reader["billdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["billdate"].ToString());
                        barcodedgoods.billno = reader["billno"] == DBNull.Value ? "" : reader["billno"].ToString();
                        //if (billmargin == 0)
                        //{
                        //    var stockprice = reader["stockprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["stockprice"].ToString());
                        //    var billprice = reader["billprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["billprice"].ToString());
                        //    if (stockprice > 0 && billprice > 0)
                        //    {
                        //        billmargin = Math.Round(100 - (billprice / stockprice * 100));
                        //    }
                        //}
                        //barcodedgoods.billmargin = billmargin;


                        barcodedgoods.invoicedate = reader["invoicedate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["invoicedate"].ToString());
                        barcodedgoods.invoiceno = reader["invoiceno"] == DBNull.Value ? "" : reader["invoiceno"].ToString();
                        barcodedgoods.suppliername = reader["suppliername"] == DBNull.Value ? "" : reader["suppliername"].ToString();



                        result.Add(barcodedgoods);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, "Brand Price search");
            }
            return result.OrderByDescending(e => e.billedon).ToList();
        }


    }
}
