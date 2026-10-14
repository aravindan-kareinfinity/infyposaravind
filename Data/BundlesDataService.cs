using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
using WebAPI.Data;
using System.Text;
using Newtonsoft.Json;
using System.Management;

namespace InfyPOS.Data
{
    public class BundlesDataService : SAASData
    {
        public bool isgoods;
        public BundlesDataService(bool isgoods)
        {
            this.isgoods = isgoods;
        }

        const string SQL_TransferHistory = @"select bundles.id,bundles.code,
fromlocationid,tolocationid,returngoods,packedby,pricetype, packedon,requesteddate, receivedon,receivedby, stock.id stockid,stock.barcode,
(itemlist ->> 'aqty') aqty, (itemlist ->> 'iqty') iqty  
from bundles 
INNER JOIN LATERAL json_array_elements (case when 
bundles.items is null or bundles.items::text ='null' then '[]'::json 
else bundles.items :: json end) itemlist ON true
inner join stock on stock.id = cast(itemlist->> 'stockid' as bigint) and stock.isactive = true
where stock.barcode = @barcode ";

        const string SQL_UpdateMissing = @"
update stock set printingname = products.name
from products 
where products.id = stock.productid
and stock.printingname = 'Missing'";

        const string SQL_Search = @"select bundles.id, bundles.code, fromlocationid, bundles.tolocationid, bundles.packedby,pricetype, receivedby, bundles.packedon,requesteddate, receivedon, notes, approvedby, bundles.isactive, bundles.createdon,bundles.billno,
            bundles.modifiedon, bundles.createdby, bundles.modifiedby, bundles.status, bundles.version, bundles.companyid,items,bundles.qty,inward2warehouse,iscompanytransfer,tocompanyid,fromcompanyid,invoiceid,bundles.billid,
			bundlepacked.name as packedbyemployee,scanmode,bundles.remarks, bundlecreated.name as createdbyemployee,bundles.ewb,bundles.transportid,bundles.modeofdelivery,bundles.vehicleno,
			bundles.ewayjson,requestcode,actionemploy.name as actionemployee,bundles.itemsvalue,
			flocation.additionalinfo->'locationconfig'->>'gst' fromgst,
            tlocation.additionalinfo->'locationconfig'->>'gst' togst,
			flocation.name fromlocation,tlocation tolocation,packslip,bundles.lrentryno,
            bundles.ewayjson->>'lrno' lrno,
            bundles.ewayjson->>'scanned' scanned,
            cast(bundles.ewayjson->>'noofpieces' as numeric) noofpieces,
            cast(bundles.ewayjson->>'noofbundles' as numeric) noofbundles,	
            cast(bundles.ewayjson->>'lrdate' as timestamp without time zone) lrdate,
            billinbox.transportid bitransportid,billinbox.billvalue bibillvalue,
            billinbox.lrdate bilrdate,billinbox.lrno bilrno,billinbox.ewb biewb,
            billinbox.einvoicejosn->>'AckNo' as bieinv
			from bundles 
            left outer join employee bundlepacked on bundlepacked.id=bundles.packedby 
            left outer join users on users.id = bundles.createdby 
            left outer join employee bundlecreated on bundlecreated.id=users.employeeid
            left outer join users actionuser on actionuser.id = bundles.actionby
            left outer join employee actionemploy on actionemploy.id=actionuser.employeeid
			left outer join referencelist flocation on flocation.id = fromlocationid
			left outer join referencelist tlocation on tlocation.id = bundles.tolocationid
            left outer join bill on bill.id = bundles.billid and bill.billtype != 9 and bill.isactive=true
            left outer join billinbox on billinbox.billid  =bundles.billid and billinbox.isactive=true
            where bundles.id=@id";

        const string SQL_Insert = @"INSERT INTO public.bundles( code, fromlocationid, tolocationid, packedby,pricetype, receivedby, packedon,requesteddate, receivedon, notes, approvedby, isactive, createdon, modifiedon, createdby, 
            modifiedby, status, version, companyid,returngoods,qty,inward2warehouse,iscompanytransfer,tocompanyid,fromcompanyid,invoiceid,billid,scanmode,remarks) VALUES( @code, @fromlocationid, @tolocationid, @packedby,@pricetype, @receivedby, @packedon,@requesteddate, @receivedon, @notes, @approvedby, @isactive, @createdon,
            @modifiedon, @createdby, @modifiedby, @status, @version, @companyid,@returngoods,@qty,@inward2warehouse,@iscompanytransfer,@tocompanyid,@fromcompanyid,@invoiceid,@billid,@scanmode,@remarks) RETURNING id";

        const string SQL_Update = @"UPDATE public.bundles SET code=@code, fromlocationid=@fromlocationid,companyid=@companyid, 
        tolocationid=@tolocationid, qty=@qty, packedby=@packedby,pricetype=@pricetype, receivedby=@receivedby, packedon=@packedon,requesteddate=@requesteddate, receivedon=@receivedon,
        notes=@notes, approvedby=@approvedby, modifiedon=@modifiedon, modifiedby=@modifiedby, status=@status, version=@version+1,remarks=@remarks,
        iscompanytransfer=@iscompanytransfer,tocompanyid=@tocompanyid,fromcompanyid=@fromcompanyid,invoiceid=@invoiceid,billid=@billid,scanmode=@scanmode
        where id=@id and version=@version and status = @currentstatus RETURNING version";

        const string SQL_Delete = @"update public.bundles set isactive=false,status=@status,version=@version+1,
                    modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version and isactive=true 
                    and returngoods=@returngoods RETURNING version";

        const string SQL_UpdateStatus = @"update public.bundles set isactive=true,status=@status,version=@version+1,
                    modifiedby=@modifiedby,modifiedon=@modifiedon,receivedby=@receivedby, receivedon=@receivedon where id=@id and version=@version and isactive=true
                    RETURNING version";

        const string SQL_UpdateItems = @"update public.bundles set items=@items where id=@id";

        const string SQL_RejectSalableGoods = @"with outwards as
                                       (
                                        select (schemelist->> 'toid')::BIGINT id,
                                               (schemelist->> 'aqty')::numeric aqty,
                                               salablegoods.iqty,
                                               salablegoods.oqty,
                                               salablegoods.jqty,

                                               salablegoods.dqty from bundles
                                        INNER JOIN LATERAL json_array_elements (case when bundles.items is null or bundles.items::text ='null' then '[]'::json else bundles.items :: json end) schemelist ON true
                                        inner join salablegoods ON salablegoods.id = (schemelist ->> 'toid') :: BIGINT
                                        where bundles.id = @bundleid and bundles.isactive = true
                                       )
                                       select id,aqty,iqty,oqty,dqty,jqty from outwards";

        const string SQL_PrintableItems = @"with outwards as
                                            (
                                            select (schemelist->> 'iqty')::numeric iqty, (schemelist->> 'aqty')::numeric qty,
                                            (schemelist->> 'journalqty')::numeric skippedqty,
                                            (schemelist->> 'qty')::numeric requestedqty,
                                            products.name,
                                            case when lrinvoiceitems.hsncode is not null or lrinvoiceitems.hsncode != '' then lrinvoiceitems.hsncode else products.hsncode end hsncode,
                                            case when lrinvoiceitems.basebuyingprice is not null or lrinvoiceitems.basebuyingprice > 0 then lrinvoiceitems.basebuyingprice else stock.buyingprice end basebuyingprice,
                                            case when lrinvoiceitems.buyingprice is not null or lrinvoiceitems.buyingprice > 0 then lrinvoiceitems.buyingprice else stock.buyingprice end buyingprice,
                                            case when lrinvoiceitems.purchasetaxpercentage is not null or lrinvoiceitems.purchasetaxpercentage > 0 then lrinvoiceitems.purchasetaxpercentage else 0 end purchasetaxpercentage,
                                            referencelist.name sizename,salablegoods.price saleprice,salablegoods.discount salediscount, stock.displaymrp,stock.designid,colourref.name colourname
                                            from bundles
                                            INNER JOIN LATERAL json_array_elements (case when bundles.items is null or bundles.items::text ='null' then '[]'::json else bundles.items :: json end) schemelist ON true
                                            inner join stock ON stock.id = (schemelist ->> 'stockid') :: BIGINT
                                            left outer join items on items.id = stock.itemid
                                            left outer join referencelist on referencelist.id = items.sizeid
                                            left outer join referencelist colourref on colourref.id = items.colourid
                                            left outer join lrinvoiceitems on lrinvoiceitems.id = invoiceitemid
                                            left outer join products on products.id = stock.productid
											left outer join salablegoods on salablegoods.id = (schemelist->> 'id')::bigint
                                            where bundles.id =  @bundleid  and bundles.isactive = true
                                            )
                                            select sum(iqty) iqty, sum(qty) qty,sum(skippedqty) skippedqty,sum(requestedqty) requestedqty,name,sizename,hsncode,colourname,designid,
											basebuyingprice,buyingprice,purchasetaxpercentage,saleprice,salediscount,displaymrp mrp
											from outwards
                                            group by name,sizename,hsncode,basebuyingprice,buyingprice,purchasetaxpercentage,
											saleprice,salediscount,displaymrp,colourname,designid";

        const string SQL_PrintableCost = @"
                                            select stock.barcode barcode,stock.serialno serialno, (schemelist->> 'iqty')::numeric iqty, (schemelist->> 'aqty')::numeric qty,products.name,
                                            case when lrinvoiceitems.hsncode is not null or lrinvoiceitems.hsncode != '' then lrinvoiceitems.hsncode else products.hsncode end hsncode,
                                            case when lrinvoiceitems.basebuyingprice is not null or lrinvoiceitems.basebuyingprice > 0 then lrinvoiceitems.basebuyingprice else stock.buyingprice end basebuyingprice,
                                            case when lrinvoiceitems.buyingprice is not null or lrinvoiceitems.buyingprice > 0 then lrinvoiceitems.buyingprice else stock.buyingprice end buyingprice,
                                            case when lrinvoiceitems.purchasetaxpercentage is not null or lrinvoiceitems.purchasetaxpercentage > 0 then lrinvoiceitems.purchasetaxpercentage else 0 end purchasetaxpercentage,
                                            referencelist.name sizename,salablegoods.price saleprice,salablegoods.discount salediscount, stock.displaymrp,stock.designid,colourref.name colourname
                                            from bundles
                                            INNER JOIN LATERAL json_array_elements (case when bundles.items is null or bundles.items::text ='null' then '[]'::json else bundles.items :: json end) schemelist ON true
                                            inner join stock ON stock.id = (schemelist ->> 'stockid') :: BIGINT
                                            left outer join items on items.id = stock.itemid
                                            left outer join referencelist on referencelist.id = items.sizeid
                                            left outer join referencelist colourref on colourref.id = items.colourid
                                            left outer join lrinvoiceitems on lrinvoiceitems.id = invoiceitemid
                                            left outer join products on products.id = stock.productid
											left outer join salablegoods on salablegoods.id = (schemelist->> 'id')::bigint
                                            where bundles.id =  @bundleid  and bundles.isactive = true
                                           ";

        const string SQL_PrintableUANItems = @"
with outwards as
                                       (
                                        select (schemelist->> 'iqty')::numeric iqty, (schemelist->> 'aqty')::numeric qty,products.name,
                                            (schemelist->> 'journalqty')::numeric skippedqty,
                                            (schemelist->> 'qty')::numeric requestedqty,
											case when lrinvoiceitems.hsncode is not null or lrinvoiceitems.hsncode != '' then lrinvoiceitems.hsncode else products.hsncode end hsncode,
											(schemelist->> 'serialno') serialno,lrinvoiceitems.basebuyingprice,referencelist.name sizename,stock.designid,colourref.name colourname,
											lrinvoiceitems.buyingprice,lrinvoiceitems.purchasetaxpercentage,salablegoods.price saleprice,salablegoods.discount salediscount,stock.displaymrp
											from bundles
                                        INNER JOIN LATERAL json_array_elements (case when bundles.items is null or bundles.items::text ='null' then '[]'::json else bundles.items :: json end) schemelist ON true
                                        left outer join stock ON stock.id = (schemelist ->> 'stockid') :: BIGINT
                                        left outer join items on items.id = (schemelist ->> 'itemid') :: BIGINT
                                        left outer join referencelist on referencelist.id = items.sizeid
                                        left outer join referencelist colourref on colourref.id = items.colourid
										left outer join lrinvoiceitems on lrinvoiceitems.id = invoiceitemid
										left outer join products on products.id  = (schemelist ->> 'productid') :: BIGINT
                                        left outer join salablegoods on salablegoods.id = (schemelist->> 'id')::bigint
                                        where bundles.id = @bundleid and bundles.isactive = true
                                       )

                                       select sum(iqty) iqty, sum(qty) qty,sum(skippedqty) skippedqty,sum(requestedqty) requestedqty,name,hsncode,serialno,sizename,basebuyingprice,buyingprice,
                                       purchasetaxpercentage,saleprice,salediscount,displaymrp mrp,colourname,designid from outwards
									   group by name,hsncode,serialno,basebuyingprice,buyingprice,purchasetaxpercentage,sizename,saleprice,
                                       salediscount,displaymrp,colourname,designid";

        const string SQL_Reject = @"with outwards as
                                       (
                                        select stock.id stockid,stock.barcode,(schemelist ->> 'aqty')::numeric aqty from bundles
                                        LEFT JOIN LATERAL
                                        json_array_elements (case when bundles.items is null or bundles.items::text ='null' then '[]'::json else bundles.items :: json end) schemelist ON true
                                        inner join stock ON (schemelist ->> 'stockid') :: BIGINT = stock.id
                                        where bundles.id=@bundleid and bundles.isactive=true)

                                        update stock set transfered = (case when transfered-outwards.aqty>=0 then transfered-outwards.aqty else 0 end)
                                        from outwards
                                        where outwards.stockid = stock.id;";

        const string SQL_RejectGoods = @"with outwards as
                                       (
                                        select (schemelist ->> 'id'):: BIGINT id,(schemelist ->> 'aqty')::numeric aqty from bundles
                                        LEFT JOIN LATERAL
                                        json_array_elements (case when bundles.items is null or bundles.items::text ='null' then '[]'::json else bundles.items :: json end) schemelist ON true
                                        where bundles.id=@bundleid and bundles.isactive=true)

                                        update salablegoods set iqty = iqty+outwards.aqty
                                        from outwards
                                        where outwards.id = salablegoods.id;";


        internal List<Bundleitem> SearchStock(BundleStockCreteria item)
        {

            List<Bundleitem> lst = new List<Bundleitem>();
            NpgsqlConnection conn = CreateConnection();
            SortedDictionary<string, decimal> packqty = null;
            try
            {
                conn.Open();

                NpgsqlCommand command = null;

                QueryBuilder qb = new QueryBuilder();

                switch (item.Type)
                {
                    case BundleStockCreteria.SearchType.Brand:
                        qb.CurrentQuery = @"select stock.id,stock.barcode,stock.serialno,stock.qty-stock.journalqty-stock.damagedqty-stock.transfered-stock.returnqty as iqty,
                        stock.qty,stock.transfered,stock.damagedqty,stock.itemid,stock.journalqty,stock.buyingprice,
                        stock.productid,stock.supplierid,stock.printingname,supplier.name as suppliername,lrreference,isbit,items.sizename from stock
                        inner join items on items.id = stock.itemid
                        left outer join supplier on supplier.id = stock.supplierid
                        where stock.isactive = true and stock.qty-stock.damagedqty-stock.journalqty-stock.transfered-stock.returnqty>0 
                        and items.brandid = @brandid and stock.companyid=@companyid";
                        qb.Limit = 1000;
                        qb.SortField = "stock.id";
                        break;
                    case BundleStockCreteria.SearchType.Barcode:

                        if (string.IsNullOrEmpty(item.barcode)) throw new Exception("Barcode can't be empty");

                        qb.CurrentQuery = @"select stock.id,stock.barcode,stock.serialno,stock.qty-stock.journalqty-stock.damagedqty-stock.transfered-stock.returnqty as iqty,
                        stock.qty,stock.transfered,stock.damagedqty,stock.itemid,stock.productid,stock.journalqty,stock.buyingprice,
                        stock.supplierid,stock.printingname,supplier.name as suppliername,lrreference,isbit,items.sizename from stock
                        inner join items on items.id = stock.itemid
                        left outer join supplier on supplier.id = stock.supplierid
                        where stock.isactive = true and (stock.barcode = @barcode or stock.serialno = @barcode) and stock.companyid=@companyid";

                        PackbarcodeDataService packdataservice = new PackbarcodeDataService() { Context = this.Context };
                        if (!string.IsNullOrEmpty(item.barcode) && !item.barcode.Contains(",") && item.barcode.EndsWith("$"))
                        {
                            var packs = packdataservice.Search(new PackbarcodeCreteria()
                            {
                                barcode = item.barcode
                            });
                            if (packs.Count > 0)
                            {
                                packqty = new SortedDictionary<string, decimal>();
                                foreach (var pitem in packs.First().barcodes)
                                {
                                    if (!packqty.ContainsKey(pitem.barcode))
                                        packqty.Add(pitem.barcode, pitem.pieces);
                                    else
                                        packqty[pitem.barcode] += pitem.pieces;
                                }
                                item.barcode = string.Join(",", packqty.Keys.ToList());
                            }
                        }

                        if (!string.IsNullOrEmpty(item.barcode) && item.barcode.Contains(","))
                        {
                            qb.CurrentQuery = @"select stock.id,stock.barcode,stock.serialno,stock.qty-stock.journalqty-stock.damagedqty-stock.transfered-stock.returnqty as iqty,
                                stock.qty,stock.transfered,stock.damagedqty,stock.itemid,stock.productid,stock.journalqty,stock.buyingprice,
                                stock.supplierid,stock.printingname,supplier.name as suppliername,lrreference,isbit,items.sizename from stock
                                inner join items on items.id = stock.itemid
                                left outer join supplier on supplier.id = stock.supplierid";
                            if (packqty == null)
                            {
                                packqty = new SortedDictionary<string, decimal>();
                                item.barcode.Split(',').ToList().ForEach(e =>
                                {
                                    if (e.EndsWith("$"))
                                    {
                                        var packs = packdataservice.Search(new PackbarcodeCreteria()
                                        {
                                            barcode = e
                                        });
                                        foreach (var pitem in packs.First().barcodes)
                                        {
                                            if (!packqty.ContainsKey(pitem.barcode))
                                                packqty.Add(pitem.barcode, pitem.pieces);
                                            else
                                                packqty[pitem.barcode] += pitem.pieces;
                                        }
                                    }
                                    else
                                    {
                                        if (packqty.ContainsKey(e))
                                            packqty[e]++;
                                        else
                                            packqty.Add(e, 1);
                                    }
                                });
                            }
                            qb.AddInParamer("stock.barcode", string.Join(",", packqty.Keys.ToList().ConvertAll(e => "'" + WebAPI.Data.Utility.SafeSQL(e) + "'")));
                            qb.AddParameter("stock.companyid", item.companyid, "=");
                        }
                        qb.SortField = "stock.id";
                        //stock.qty-stock.journalqty-stock.damagedqty-stock.transfered-stock.returnqty>0 and 
                        break;
                    case BundleStockCreteria.SearchType.Product:
                        qb.CurrentQuery = @"select stock.id,stock.barcode,stock.serialno,stock.qty-stock.journalqty-stock.damagedqty-stock.transfered-stock.returnqty as iqty,
                        stock.qty,stock.transfered,stock.damagedqty,stock.journalqty,stock.itemid,stock.productid,stock.buyingprice,
                        stock.supplierid,stock.printingname,supplier.name as suppliername,lrreference,isbit,items.sizename from stock
                        inner join items on items.id = stock.itemid
                        left outer join supplier on supplier.id = stock.supplierid
                        where stock.isactive = true and stock.qty-stock.journalqty-stock.damagedqty-stock.transfered-stock.returnqty>0 
                        and stock.productid = @productid and stock.companyid=@companyid";
                        qb.Limit = 1000;
                        qb.SortField = "stock.id";
                        break;
                    case BundleStockCreteria.SearchType.BarcodedOn:
                        qb.CurrentQuery = @"select stock.id,stock.barcode,stock.serialno,stock.qty-stock.journalqty-stock.damagedqty-stock.transfered-stock.returnqty as iqty,
                        stock.qty,stock.transfered,stock.damagedqty,stock.journalqty,stock.itemid,stock.productid,stock.buyingprice,
                        stock.supplierid,stock.printingname,supplier.name as suppliername,lrreference,isbit,items.sizename  from stock
                        inner join items on items.id = stock.itemid
                        left outer join supplier on supplier.id = stock.supplierid
                        where stock.isactive = true and stock.companyid=@companyid and stock.qty-stock.journalqty-stock.damagedqty-stock.transfered-stock.returnqty>0 
                        and stock.createdon >= @barcodedfrom and stock.createdon < @barcodedto";
                        qb.SortField = "stock.id";
                        break;
                    case BundleStockCreteria.SearchType.AllStock:
                    case BundleStockCreteria.SearchType.None:
                        qb.CurrentQuery = @"select stock.id,stock.barcode,stock.serialno,stock.qty-stock.journalqty-stock.damagedqty-stock.transfered-stock.returnqty as iqty,
                        stock.qty,stock.transfered,stock.damagedqty,stock.journalqty,stock.itemid,stock.productid,stock.buyingprice,
                        stock.supplierid,stock.printingname,supplier.name as suppliername,lrreference,isbit,items.sizename  from stock
                        left outer join items on items.id = stock.itemid
                        left outer join supplier on supplier.id = stock.supplierid
                        where stock.isactive = true and stock.companyid=@companyid and stock.qty-stock.journalqty-stock.damagedqty-stock.transfered-stock.returnqty>0 
                        and case when @retaillocationid=0 then stock.retaillocationid>=-1 else stock.retaillocationid=@retaillocationid end";
                        qb.Limit = 1000;
                        qb.SortField = "stock.id";
                        break;
                    case BundleStockCreteria.SearchType.LREntry:
                        qb.CurrentQuery = @"select stock.id,stock.barcode,stock.serialno,stock.qty-stock.journalqty-stock.damagedqty-stock.transfered-stock.returnqty as iqty,
                        stock.qty,stock.transfered,stock.damagedqty,stock.itemid,stock.productid,stock.journalqty,stock.buyingprice,
                        stock.supplierid,stock.printingname,supplier.name as suppliername,lrreference,isbit,items.sizename  from stock
                        inner join lrinvoiceitems on lrinvoiceitems.id = stock.invoiceitemid and lrinvoiceitems.isactive=true
                        inner join lrinvoice on lrinvoice.id = lrinvoiceitems.lrinvoiceid  and lrinvoice.isactive = true
                        inner join lrentry on lrentry.id = lrinvoice.lrid
                        left outer join items on items.id = stock.itemid
                        left outer join supplier on supplier.id = lrentry.supplierid
                        where stock.isactive = true and stock.companyid=@companyid and stock.qty-stock.journalqty-stock.damagedqty-stock.transfered-stock.returnqty >0
                        and lrentry.lrentryno = @lrentryno";
                        qb.SortField = "stock.id";
                        break;
                    case BundleStockCreteria.SearchType.DesignNo:
                        qb.CurrentQuery = @"select stock.id,stock.barcode,stock.serialno,stock.qty-stock.journalqty-stock.damagedqty-stock.transfered-stock.returnqty as iqty,
                        stock.qty,stock.transfered,stock.damagedqty,stock.itemid,stock.productid,stock.journalqty,stock.buyingprice,
                        stock.supplierid,stock.printingname,supplier.name as suppliername,lrreference,isbit,items.sizename  from stock
                        inner join lrinvoiceitems on lrinvoiceitems.id = stock.invoiceitemid and lrinvoiceitems.isactive=true
                        inner join lrinvoice on lrinvoice.id = lrinvoiceitems.lrinvoiceid  and lrinvoice.isactive = true
                        inner join lrentry on lrentry.id = lrinvoice.lrid
                        left outer join items on items.id = stock.itemid
                        left outer join supplier on supplier.id = lrentry.supplierid
                        where stock.isactive = true and stock.companyid=@companyid and stock.qty-stock.journalqty-stock.damagedqty-stock.transfered-stock.returnqty >0
                        and stock.designid = @designid";
                        qb.SortField = "stock.id";
                        break;
                }

                if (item.supplierid > 0)
                {
                    qb.AddParameter("stock.supplierid", item.supplierid, "=");
                }
                if (item.removelimit)
                    qb.Limit = 0;
                command = qb.GetCommand(conn);
                if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_fixed_barcodes &&
                    !InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_hyper_uan &&
                    !string.IsNullOrEmpty(item.barcode) && item.barcode.Contains(","))
                {
                    string newquery = command.CommandText.Replace("Order by stock.id", "");
                    string secondquery = newquery.Replace("stock.barcode in", "stock.serialno in");
                    command.CommandText = newquery + " union all " + secondquery;
                }
                command.Parameters.Add("productid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.productid;
                command.Parameters.Add("barcode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = item.barcode == null ? "" : item.barcode;
                command.Parameters.Add("designid", NpgsqlTypes.NpgsqlDbType.Varchar).Value = string.IsNullOrEmpty(item.designcode) ? "" : item.designcode;
                command.Parameters.Add("brandid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.brandid;
                command.Parameters.Add("barcodedfrom", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = item.date.Date;
                command.Parameters.Add("barcodedto", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = item.date.Date.AddDays(1);
                command.Parameters.Add("lrentryno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = item.lrentryno == null ? "" : item.lrentryno;
                command.Parameters.Add("retaillocationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.retaillocationid;
                command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.companyid;
                //command.Parameters.Add("supplierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.supplierid;

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Bundleitem ostock = new Bundleitem();
                        ostock.stockid = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        ostock.suppliername = reader["suppliername"] == DBNull.Value ? "" : reader["suppliername"].ToString();
                        ostock.lrreference = reader["lrreference"] == DBNull.Value ? "" : reader["lrreference"].ToString();
                        ostock.printingname = reader["printingname"] == DBNull.Value ? "" : reader["printingname"].ToString();
                        ostock.sizename = reader["sizename"] == DBNull.Value ? "" : reader["sizename"].ToString();
                        ostock.barcode = reader["barcode"] == DBNull.Value ? "" : reader["barcode"].ToString();
                        ostock.serialno = reader["serialno"] == DBNull.Value ? "" : reader["serialno"].ToString();
                        ostock.itemid = reader["itemid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["itemid"].ToString());
                        ostock.productid = reader["productid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productid"].ToString());
                        ostock.qty = reader["iqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["iqty"].ToString());
                        ostock.transfered = reader["transfered"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["transfered"].ToString());
                        ostock.damagedqty = reader["damagedqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["damagedqty"].ToString());
                        ostock.journalqty = reader["journalqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["journalqty"].ToString());
                        ostock.iqty = reader["iqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["iqty"].ToString());
                        ostock.supplierid = reader["supplierid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["supplierid"]);
                        ostock.buyingprice = reader["buyingprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["buyingprice"].ToString());
                        if (ostock.qty > 0 && packqty != null && packqty.ContainsKey(ostock.barcode))
                        {
                            if (ostock.qty >= packqty[ostock.barcode])
                                ostock.iqty = ostock.qty = packqty[ostock.barcode];
                        }
                        else if (ostock.qty > 0 && packqty != null && packqty.ContainsKey(ostock.serialno))
                        {
                            if (packqty[ostock.serialno] == 0)
                                continue;

                            if (ostock.qty >= packqty[ostock.serialno])
                            {
                                ostock.iqty = packqty[ostock.serialno];
                                packqty[ostock.serialno] = 0;
                            }
                            else
                            {
                                ostock.iqty = ostock.qty;
                                packqty[ostock.serialno] -= ostock.qty;
                            }
                        }
                        if (reader["isbit"] != DBNull.Value && (bool)reader["isbit"])
                            ostock.pqty = 1;
                        else
                            ostock.pqty = ostock.qty;

                        if (!string.IsNullOrEmpty(item.barcode) &&
                            ostock.serialno == item.barcode)
                        {
                            if (ostock.iqty <= 0)
                                continue;
                        }
                        lst.Add(ostock);
                        //if (!string.IsNullOrEmpty(item.barcode) &&
                        //    !string.IsNullOrEmpty(ostock.serialno) && 
                        //    ostock.serialno == item.barcode)
                        //    break;
                    }
                }
                if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_fixed_barcodes &&
                    !InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_hyper_uan)
                {
                    lst.RemoveAll(e => lst.Exists(x => x.serialno == e.barcode));
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, item);
            }
            return lst;
        }

        internal void UpdateCostPrice(Bundles bundles)
        {
            Data.WarehouseReportDataService warehousedata = new WarehouseReportDataService() { Context = this.Context };
            warehousedata.Context = this.Context;
            var listofcost = warehousedata.SearchOutwardCost(new WarehouseReportCreteria()
            {
                showdetail = true,
                packageno = bundles.code
            });

            bundles.items.ForEach(x =>
            {
                if (listofcost.Exists(y => y.barcode == x.barcode))
                {
                    x.price = listofcost.Find(y => y.barcode == x.barcode).itembuyingprice;
                }
            });

            return;

            //List<BundlesPrint.Printableitem> retlist = new List<BundlesPrint.Printableitem>();
            //var param = new SortedList<string, object>();
            //param.Add("@bundleid", bundles.id);
            //using (var reader = ExecuteReader(SQL_PrintableCost, param))
            //{
            //    while (reader.Read())
            //    {
            //        BundlesPrint.Printableitem printableitem = new BundlesPrint.Printableitem();
            //        printableitem.description = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
            //        printableitem.hsn = reader["hsncode"] == DBNull.Value ? "" : reader["hsncode"].ToString();
            //        printableitem.sizename = reader["sizename"] == DBNull.Value ? "" : reader["sizename"].ToString();
            //        printableitem.colourname = reader["colourname"] == DBNull.Value ? "" : reader["colourname"].ToString();
            //        printableitem.designid = reader["designid"] == DBNull.Value ? "" : reader["designid"].ToString();
            //        printableitem.gln = reader["barcode"] == DBNull.Value ? "" : reader["barcode"].ToString();
            //        printableitem.serialno = reader["serialno"] == DBNull.Value ? "" : reader["serialno"].ToString();
            //        printableitem.qty = reader["iqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["iqty"].ToString());
            //        printableitem.costprice = reader["buyingprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["buyingprice"].ToString());
            //        printableitem.taxpercentage = reader["purchasetaxpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["purchasetaxpercentage"].ToString());
            //        printableitem.taxable = printableitem.qty * printableitem.costprice;
            //        printableitem.igst = Math.Round(printableitem.taxable * printableitem.taxpercentage / 100, 0);
            //        printableitem.amount = printableitem.taxable + printableitem.igst + printableitem.cgst + printableitem.sgst;
            //        retlist.Add(printableitem);
            //    }
            //}
            //return retlist;
        }

        internal BundlesPrint Print(long id, string packingcode)
        {
            var bundles = Search(new BundlesCreteria()
            {
                id = id
            });

            if (bundles == null || bundles.Count == 0)
                return new BundlesPrint();

            var supportuan = false;

            InfyPOS.Data.ConfigurationDataService config = new ConfigurationDataService() { Context = this.Context };
            config.Context = new UserClaim()
            {
                OrganizationID = Context.OrganizationID
            };
            var lstconfig = config.Search(new ConfigurationCreteria
            {
                key = "STOCKTRANSFER"
            });

            if (lstconfig != null && lstconfig.Count > 0)
            {
                supportuan = lstconfig[0].reportformat.format.IndexOf("gln") > 0||
                    lstconfig[0].reportformat.format.IndexOf("serialno") > 0;
            }

            BundlesPrint ret = new BundlesPrint();
            ret.source = bundles[0];
            ret.packslip = bundles[0].packslip;
            ret.packedby = bundles[0].packedby;
            ret.packedbyemployee = bundles[0].packedbyemployee;
            ret.createdbyemployee = bundles[0].createdbyemployee;
            ret.remarks = bundles[0].remarks;
            ret.packedon = bundles[0].packedon;
            ret.requesteddate = bundles[0].requesteddate;

            ret.qty = bundles[0].qty;
            ret.items = bundles[0].items;
            ret.fromlocationid = bundles[0].fromlocationid;
            ret.tolocationid = bundles[0].tolocationid;
            ret.code = bundles[0].code;
            ret.ewb = bundles[0].ewb;
            ret.vehicleno = bundles[0].vehicleno;
            ret.eway = bundles[0].eway;
            ret.transportid = bundles[0].transportid;
            ret.modeofdelivery = bundles[0].modeofdelivery;

            ret.notes = bundles[0].notes;
            ret.approvedby = bundles[0].approvedby;
            ret.status = bundles[0].status;
            ret.companyid = bundles[0].companyid;
            if (ret.transportid > 0)
            {
                TransportDataService transport = new TransportDataService() { Context = this.Context };
                var transports = transport.Search(new TransportCreteria()
                {
                    id = ret.transportid
                });
                if (transports.Count > 0)
                    ret.transport = transports[0];
            }
            Data.WarehouseReportDataService warehousedata = new WarehouseReportDataService() { Context = this.Context };
            warehousedata.Context = this.Context;
            ret.lstoutwarditems = warehousedata.SearchOutwardCost(new WarehouseReportCreteria()
            {
                showdetail = true,
                packageno = bundles[0].code
            });

            Data.CompanyDataService companyservice = new CompanyDataService() { Context = this.Context };
            companyservice.Context = this.Context;
            var companylist = companyservice.Search(new CompanyCreteria()
            {
                id = bundles[0].fromcompanyid
            });

            if (companylist.Count == 1)
            {
                ret.company = companylist[0];
                ret.tocompany = ret.company;
            }

            if (bundles[0].tocompanyid > 0)
            {
                companylist = companyservice.Search(new CompanyCreteria()
                {
                    id = bundles[0].tocompanyid
                });

                if (companylist.Count == 1)
                    ret.tocompany = companylist[0];
            }

            ReferencelistDataService referencelistDataService = new ReferencelistDataService() { Context = this.Context };
            var location = referencelistDataService.Search(new ReferencelistCreteria()
            {
                id = ret.fromlocationid
            }).First();

            ret.fromlocation = new CompanyLocation(location);
            ret.fromlocation.name = location.name;
            ret.fromlocation.phonenumber = location.additionalinfo == null ? "" : location.additionalinfo.formula;
            ret.fromlocation.address = location.additionalinfo == null ? "" : location.additionalinfo.address;
            ret.fromlocation.addresslines = new List<CompanyLocation.Address>();
            if (!string.IsNullOrEmpty(ret.fromlocation.address))
            {
                var addresslist = ret.fromlocation.address.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                addresslist.ToList().ForEach(e =>
                {
                    ret.fromlocation.addresslines.Add(new CompanyLocation.Address()
                    {
                        value = e
                    });
                });
            }

            if (ret.fromlocation.cityid > 0)
            {
                ret.fromlocation.cityname = referencelistDataService.Search(new ReferencelistCreteria()
                {
                    id = ret.fromlocation.cityid
                }).First().name;
            }
            if (ret.fromlocation.stateid > 0)
            {
                ret.fromlocation.statename = referencelistDataService.Search(new ReferencelistCreteria()
                {
                    id = ret.fromlocation.stateid
                }).First().name;
            }

            if (ret.company == null && location.additionalinfo.parentid > 0)
            {
                companylist = companyservice.Search(new CompanyCreteria()
                {
                    id = location.additionalinfo.parentid
                });
                ret.company = companylist[0];
                ret.tocompany = ret.company;
            }

            location = referencelistDataService.Search(new ReferencelistCreteria()
            {
                id = ret.tolocationid
            }).First();

            ret.tolocation = new CompanyLocation(location);
            ret.tolocation.name = location.name;
            ret.tolocation.phonenumber = location.additionalinfo == null ? "" : location.additionalinfo.formula;
            ret.tolocation.address = location.additionalinfo == null ? "" : location.additionalinfo.address;
            ret.tolocation.addresslines = new List<CompanyLocation.Address>();
            if (!string.IsNullOrEmpty(ret.tolocation.address))
            {
                var addresslist = ret.tolocation.address.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                addresslist.ToList().ForEach(e =>
                {
                    ret.tolocation.addresslines.Add(new CompanyLocation.Address()
                    {
                        value = e
                    });
                });
            }
            if (ret.tolocation.cityid > 0)
            {
                ret.tolocation.cityname = referencelistDataService.Search(new ReferencelistCreteria()
                {
                    id = ret.tolocation.cityid
                }).First().name;
            }
            if (ret.tolocation.stateid > 0)
            {
                ret.tolocation.statename = referencelistDataService.Search(new ReferencelistCreteria()
                {
                    id = ret.tolocation.stateid
                }).First().name;
            }

            var param = new SortedList<string, object>();
            param.Add("@bundleid", id);
            bool isigst = (ret.company != null &&
                ret.tocompany != null &&
                ret.company.stateid != ret.tocompany.stateid);
            ret.groupitems = new List<BundlesPrint.Printableitem>();
            using (var reader = ExecuteReader(supportuan ? SQL_PrintableUANItems : SQL_PrintableItems, param))
            {
                while (reader.Read())
                {
                    BundlesPrint.Printableitem printableitem = new BundlesPrint.Printableitem();
                    printableitem.description = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
                    printableitem.hsn = reader["hsncode"] == DBNull.Value ? "" : reader["hsncode"].ToString();
                    printableitem.sizename = reader["sizename"] == DBNull.Value ? "" : reader["sizename"].ToString();
                    printableitem.colourname = reader["colourname"] == DBNull.Value ? "" : reader["colourname"].ToString();
                    printableitem.designid = reader["designid"] == DBNull.Value ? "" : reader["designid"].ToString();
                    if (supportuan)
                    {
                        printableitem.gln = reader["serialno"] == DBNull.Value ? "" : reader["serialno"].ToString();
                        printableitem.serialno = reader["serialno"] == DBNull.Value ? "" : reader["serialno"].ToString();
                    }
                    printableitem.qty = reader["qty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["qty"].ToString());
                    if (printableitem.qty == 0)
                        printableitem.qty = reader["iqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["iqty"].ToString());

                    printableitem.skippedqty = reader["skippedqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["skippedqty"].ToString());
                    printableitem.requestedqty = reader["requestedqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["requestedqty"].ToString());
                    printableitem.issueddqty = reader["iqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["iqty"].ToString());

                    printableitem.costprice = reader["buyingprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["buyingprice"].ToString());
                    printableitem.taxpercentage = reader["purchasetaxpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["purchasetaxpercentage"].ToString());
                    printableitem.taxable = printableitem.qty * printableitem.costprice;
                    if (isigst)
                    {
                        if (printableitem.taxpercentage > 0)
                            printableitem.igst = Math.Round(printableitem.taxable * printableitem.taxpercentage / 100, 0);
                    }
                    else
                    {
                        if (printableitem.taxpercentage > 0)
                            printableitem.cgst = printableitem.sgst = Math.Round(printableitem.taxable * (printableitem.taxpercentage / 2) / 100, 0);
                    }
                    printableitem.amount = printableitem.taxable + printableitem.igst + printableitem.cgst + printableitem.sgst;
                    printableitem.sno = ret.groupitems.Count + 1;
                    printableitem.mrp = reader["mrp"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["mrp"].ToString());
                    printableitem.price = reader["saleprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["saleprice"].ToString());
                    printableitem.discount = reader["salediscount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["salediscount"].ToString());
                    ret.groupitems.Add(printableitem);
                }
            }

            if (!string.IsNullOrEmpty(packingcode) && ret.packslip.Exists(x => x.code == packingcode))
            {
                ret.ispackingslip = true;
                var packslip = ret.packslip.Find(x => x.code == packingcode);
                ret.code = packslip.code;
                ret.totalqty = ret.groupitems.Sum(x => x.qty);
                ret.packedqty = packslip.items.Sum(x => x.qty);
                EmployeeDataService employeeDataService = new EmployeeDataService() { Context = this.Context };
                var users = employeeDataService.GetUserName(new List<long>() { packslip.createdby, packslip.modifiedby });
                ret.packedbyemployee = users.Exists(x => x.id == packslip.modifiedby) ? users.Find(x => x.id == packslip.modifiedby).name : "";
                ret.items.ForEach(x =>
                {
                    x.qty = 0;
                    x.aqty = 0;
                    if (packslip.items != null &&
                       packslip.items.Exists(y => y.barcode == x.serialno || x.barcode == x.barcode))
                    {
                        x.qty = packslip.items.Find(y => y.barcode == x.serialno || x.barcode == x.barcode).qty;
                    }
                });
                ret.items.RemoveAll(x => x.qty == 0);

                ret.groupitems.ForEach(x =>
                {
                    x.qty = 0;
                    if (packslip.items != null &&
                       packslip.items.Exists(y => y.barcode == x.serialno || y.barcode == x.gln))
                    {
                        x.qty = packslip.items.Find(y => y.barcode == x.serialno || y.barcode == x.gln).qty;
                        x.taxable = x.qty * x.costprice;
                    }
                });
                ret.groupitems.RemoveAll(x => x.qty == 0);
            }

            ret.inwords = WebAPI.Data.Numberconvertor.rupees(ret.netamount);
            if (ret.qty == 0 && ret.items != null)
            {
                ret.qty = ret.items.Sum(e => e.iqty);
            }
            return ret;
        }

        public List<BundleItemTransferHistory> SearchHistory(string barcode, List<string> barcodes, long companyid)
        {
            List<BundleItemTransferHistory> list = new List<BundleItemTransferHistory>();
            var param = new SortedList<string, object>();
            param.Add("barcode", barcode);
            string query = SQL_TransferHistory;
            if (companyid > 0)
            {
                query = query.Replace("stock.barcode = @barcode", "stock.barcode = @barcode and stock.companyid=@companyid");
                param.Add("companyid", companyid);
            }

            if (string.IsNullOrEmpty(barcode) && barcodes != null && barcodes.Count > 0)
                query = query.Replace("stock.barcode = @barcode", "stock.barcode in (" + string.Join(",", barcodes.ConvertAll(e => "'" + WebAPI.Data.Utility.SafeSQL(e) + "'")) + ")");

            using (var reader = ExecuteReader(SQL_TransferHistory, param))
            {
                while (reader.Read())
                {
                    BundleItemTransferHistory item = new BundleItemTransferHistory();
                    item.code = reader["code"] != DBNull.Value ? reader["code"].ToString() : "";
                    item.id = reader["id"] != DBNull.Value ? Convert.ToInt64(reader["id"]) : 0;
                    item.fromlocationid = reader["fromlocationid"] != DBNull.Value ? Convert.ToInt64(reader["fromlocationid"]) : 0;
                    item.tolocationid = reader["tolocationid"] != DBNull.Value ? Convert.ToInt64(reader["tolocationid"]) : 0;
                    item.returngoods = reader["returngoods"] != DBNull.Value ? Convert.ToBoolean(reader["returngoods"]) : false;
                    item.packedon = reader["packedon"] != DBNull.Value ? Convert.ToDateTime(reader["packedon"]) : DateTime.MinValue;
                    item.requesteddate = reader["requesteddate"] != DBNull.Value ? Convert.ToDateTime(reader["requesteddate"]) : DateTime.MinValue;

                    item.receivedon = reader["receivedon"] != DBNull.Value ? Convert.ToDateTime(reader["receivedon"]) : DateTime.MinValue;
                    item.qty = reader["aqty"] != DBNull.Value ? Convert.ToDecimal(reader["aqty"]) : 0;
                    item.transferedqty = reader["iqty"] != DBNull.Value ? Convert.ToDecimal(reader["iqty"]) : 0;
                    list.Add(item);
                }
            }
            return list;
        }

        internal BundlesCreteria UpdateUANStock(BundlesCreteria oBundles)
        {
            string query = @"select stock.serialno,
							sum(salablegoods.iqty-(oqty+dqty+jqty)) stk
                            from bundles
                            INNER JOIN LATERAL json_array_elements (case when bundles.items is null or bundles.items::text ='null' then '[]'::json else bundles.items :: json end) schemelist ON true
                            inner join stock ON stock.serialno = (schemelist ->> 'serialno') and stock.isactive=true
							inner join salablegoods on salablegoods.stockid = stock.id
							and salablegoods.locationid = @fromlocationid and salablegoods.isactive=true
							and salablegoods.iqty-(oqty+dqty+jqty)>0 
                            where bundles.id = @bundleid and bundles.isactive = true
                            group by stock.serialno";

            //string query = @"with uidlist
            //                as
            //                (
            //                select schemelist->>'serialno' serialno
            //                                            from bundles
            //                                            LEFT JOIN LATERAL
            //                                            json_array_elements(bundles.items :: json) schemelist ON true
            //                       where bundles.id = @bundleid
            //                )
            //                select uidlist.serialno,
            //                sum(salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty)) stk
            //                from uidlist
            //                inner join stock ON stock.serialno = uidlist.serialno
            //                inner join salablegoods on salablegoods.stockid = stock.id
            //                and salablegoods.locationid = @fromlocationid and salablegoods.isactive=true 
            //                and salablegoods.iqty-(oqty+dqty+jqty)>0";

            var param = new SortedList<string, object>();
            param.Add("bundleid", oBundles.id);
            param.Add("fromlocationid", oBundles.fromlocationid);
            using (var reader = ExecuteReader(query, param))
            {
                while (reader.Read())
                {
                    string serialno = reader["serialno"] == DBNull.Value ? "" : reader["serialno"].ToString();
                    decimal stk = reader["stk"] == DBNull.Value ? 0 : Convert.ToInt64(reader["stk"]);
                    oBundles.items.Find(x => x.serialno == serialno).rqty = stk;
                }
            }
            return oBundles;
        }

        internal BundlesCreteria UpdateUANGoodsStock(BundlesCreteria oBundles)
        {
            //     string query = @"select salablegoods.barcode,salablegoods.id salablegoodsid,stock.id stockid,
            //stock.serialno,
            //salablegoods.iqty-(oqty+dqty+jqty) stk
            //                     from bundles
            //                     LEFT JOIN LATERAL
            //                     json_array_elements(bundles.items :: json) schemelist ON true
            //                     inner join stock ON stock.serialno = (schemelist ->> 'serialno') and stock.isactive=true
            //inner join salablegoods on salablegoods.stockid = stock.id
            //and salablegoods.locationid = @fromlocationid and salablegoods.isactive=true
            //and salablegoods.iqty-(oqty+dqty+jqty)>0 
            //                     where bundles.id = @bundleid and bundles.isactive = true
            //                     order by stock.id";

            string query = @"with uidlist
                            as
                            (
                            select schemelist->>'serialno' serialno
                                                        from bundles
                                                        INNER JOIN LATERAL json_array_elements (case when bundles.items is null or bundles.items::text ='null' then '[]'::json else bundles.items :: json end) schemelist ON true
							                            where bundles.id = @bundleid
                            )
                            select uidlist.serialno,stock.id stockid,
                            salablegoods.id salablegoodsid,salablegoods.barcode,
                            salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty) stk
                            from uidlist
                            inner join stock ON stock.serialno = uidlist.serialno
                            inner join salablegoods on salablegoods.stockid = stock.id
                            and salablegoods.locationid = @fromlocationid and salablegoods.isactive=true 
                            and salablegoods.iqty-(oqty+dqty+jqty)>0";

            var param = new SortedList<string, object>();
            param.Add("bundleid", oBundles.id);
            param.Add("fromlocationid", oBundles.fromlocationid);
            using (var reader = ExecuteReader(query, param))
            {
                while (reader.Read())
                {
                    string serialno = reader["serialno"] == DBNull.Value ? "" : reader["serialno"].ToString();
                    var pendingitems = oBundles.items.FindAll(x => x.serialno == serialno && x.aqty > x.rqty);
                    if (pendingitems.Count > 0)
                    {
                        long salablegoodsid = reader["salablegoodsid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["salablegoodsid"]);
                        long stockid = reader["stockid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["stockid"]);
                        string barcode = reader["barcode"] == DBNull.Value ? "" : reader["barcode"].ToString();
                        decimal stk = reader["stk"] == DBNull.Value ? 0 : Convert.ToInt64(reader["stk"]);
                        if ((pendingitems[0].aqty - pendingitems[0].rqty) <= stk)
                        {
                            pendingitems[0].rqty += (pendingitems[0].aqty - pendingitems[0].rqty);
                            pendingitems[0].stockid = stockid;
                            pendingitems[0].id = salablegoodsid;
                            pendingitems[0].barcode = barcode;
                        }
                        else if (stk > 0)
                        {
                            var newitem = JsonConvert.DeserializeObject<Bundleitem>(JsonConvert.SerializeObject(pendingitems[0]));
                            if (pendingitems[0].qty > stk)
                            {
                                pendingitems[0].qty -= stk;
                                newitem.qty = stk;
                            }
                            if (pendingitems[0].iqty > stk)
                            {
                                pendingitems[0].iqty -= stk;
                                newitem.iqty = stk;
                            }
                            if (pendingitems[0].aqty > stk)
                            {
                                pendingitems[0].aqty -= stk;
                                newitem.aqty = stk;
                            }
                            newitem.rqty = stk;
                            newitem.stockid = stockid;
                            newitem.id = salablegoodsid;
                            newitem.barcode = barcode;
                            oBundles.items.Add(newitem);
                        }
                    }
                }
            }

            var stockmissing = oBundles.items.FindAll(x => x.aqty > x.rqty);
            if (stockmissing.Count > 0)
            {
                throw new Exception(
                    "Stock Validation failed \r\n" +
                    String.Join("\r\n",
                    stockmissing.ConvertAll(x => x.serialno + " " + (x.aqty - x.rqty).ToString())));
            }

            oBundles.Salablegoods = oBundles.items.ConvertAll(e => new Salablegoods()
            {
                id = e.id,
                iqty = e.iqty,
                aqty = e.aqty,
                barcode = e.barcode,
                supplierid = e.supplierid,
                stockid = e.stockid,
                dqty = e.damagedqty,
                itemid = e.itemid,
                lrreference = e.lrreference,
                suppliername = e.suppliername,
                locationid = oBundles.tolocationid
            });
            return oBundles;
        }
        public Bundles InwardGoods(BundlesCreteria oBundles)
        {
            NpgsqlTransaction transaction = null;
            try
            {
                oBundles.items.RemoveAll(e => e.qty == 0);
                if (oBundles.items.Any(e => e.qty < e.aqty))
                    throw new Exception(oBundles.items.First(e => e.qty < e.aqty).barcode + " accepted qty higher than inwarded");

                if (oBundles.items.Any(e => e.aqty < 0))
                    throw new Exception(oBundles.items.First(e => e.aqty < 0).barcode + " Inwarded qty must be non negative.");


                if (oBundles.Salablegoods.All(x => x.id == 0))
                {
                    oBundles = UpdateUANGoodsStock(oBundles);
                }

                int currentbundlestatus = oBundles.status;
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    oBundles.status = oBundles.Salablegoods.All(e => e.iqty == e.aqty) ? 1 : 2;

                    if (Services.ConfigManager.Instance(Context.OrganizationID).Config.intray_bundle && oBundles.status == 2
                        && currentbundlestatus != 8)
                    {
                        ReferencelistDataService referencelistDataService = new ReferencelistDataService() { Context = this.Context };
                        var locations = referencelistDataService.SearchList(new ReferencelistCreteria()
                        {
                            ids = new List<long>(new long[] { oBundles.fromlocationid, oBundles.tolocationid })
                        });

                        if (locations.Exists(x => x.id == oBundles.fromlocationid) &&
                            locations.Exists(x => x.id == oBundles.tolocationid))
                        {

                            Services.IntrayManager.Instance(Context.OrganizationID).Add(
                                Context.OrganizationID,
                                Services.IntrayManager.EntryType.Transfer,
                                oBundles.id, string.Format("TRANSFER REQUEST {3} FROM {0} TO {1} QTY {2}.",
                                locations.Find(x => x.id == oBundles.fromlocationid).name,
                                locations.Find(x => x.id == oBundles.tolocationid).name,
                                oBundles.items.Sum(x => x.iqty), oBundles.code), Context.UserID, 0, transaction, Context);
                        }
                        oBundles.status = -1;
                        oBundles.errormessage = "Partial Bundle Inward Restricted., Please follow Approval process.";

                        var param = new SortedList<string, object>();
                        param.Add("status", -1);
                        param.Add("id", oBundles.id);
                        ExecuteQuery("update Bundles set status=@status where id=@id", param);

                        transaction.Commit();
                        conn.Close();

                        return oBundles;
                    }

                    foreach (var item in oBundles.items)
                    {
                        item.transfered += oBundles.Salablegoods.Find(e => e.barcode == item.barcode).aqty;
                    }
                    if (!Update(transaction, conn, oBundles, currentbundlestatus))
                    {
                        transaction.Rollback();
                        oBundles.errormessage = "Already got inwarded";
                        return oBundles;
                    }
                    SalablegoodsDataService subSalablegoods = new SalablegoodsDataService() { Context = this.Context };
                    subSalablegoods.Context = this.Context;
                    if (oBundles.inward2warehouse)
                    {
                        subSalablegoods.InwardGoods2WareHouse(transaction, conn, oBundles.Salablegoods, oBundles.receivedon, oBundles);
                        if (oBundles.eway == null)
                            oBundles.eway = new Bundles.EwayInfo();
                        oBundles.eway.scanned = oBundles.items.Sum(x => x.transfered > 0 ? 1 : 0);

                        NpgsqlCommand command = GetCommand(@"update public.bundles set items=@items,ewayjson=@ewayjson where id=@id", conn, transaction);
                        command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBundles.id;
                        command.Parameters.Add("items", NpgsqlTypes.NpgsqlDbType.Json).Value = oBundles.items_json;
                        command.Parameters.Add("ewayjson", NpgsqlTypes.NpgsqlDbType.Json).Value = oBundles.eway_json;
                        command.ExecuteNonQuery();

                        StocktransactionDataService stocktransaction = new StocktransactionDataService() { Context = this.Context };
                        stocktransaction.InwardGoods2WareHouse(oBundles, transaction, conn);
                    }
                    else
                    {
                        if (oBundles.Salablegoods.Exists(x => x.locationid != oBundles.fromlocationid))
                        {
                            bool haserror = false;
                            StringBuilder stringBuilder = new StringBuilder("Barcode scanned from wrong location while creating bundles \r\n");
                            var stockids = oBundles.Salablegoods.ConvertAll(x => x.id);
                            using (var reader = ExecuteReader(String.Format(@"select salablegoods.locationid,referencelist.name,stock.barcode,stock.serialno from salablegoods
                                inner join stock on stock.id = salablegoods.stockid
                                inner join referencelist on referencelist.id = salablegoods.locationid
                                where salablegoods.id in ({0})", String.Join(",", stockids), null), null))
                            {
                                while (reader.Read())
                                {
                                    if (Convert.ToInt64(reader["locationid"]) != oBundles.fromlocationid)
                                    {
                                        haserror = true;
                                        if (reader["serialno"] == DBNull.Value)
                                            stringBuilder.AppendFormat("Location {0} Barcode {1}\r\n", reader["name"].ToString(), reader["barcode"].ToString());
                                        else
                                            stringBuilder.AppendFormat("Location {0} UAN {1} Barcode {2}\r\n", reader["name"].ToString(), reader["serialno"].ToString(), reader["barcode"].ToString());
                                    }
                                }
                            }
                            if (haserror)
                                throw new Exception(stringBuilder.ToString() + " remove these item from bundle before inwarding");
                        }

                        subSalablegoods.SwapGoods(transaction, conn, oBundles.Salablegoods, oBundles.receivedon,
                            oBundles.fromlocationid,
                            oBundles.tolocationid, oBundles);

                        if (oBundles.eway == null)
                            oBundles.eway = new Bundles.EwayInfo();
                        oBundles.eway.scanned = oBundles.items.Sum(x => x.transfered > 0 ? 1 : 0);

                        NpgsqlCommand command = GetCommand(@"update public.bundles set items=@items,ewayjson=@ewayjson where id=@id", conn, transaction);
                        command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBundles.id;
                        command.Parameters.Add("items", NpgsqlTypes.NpgsqlDbType.Json).Value = oBundles.items_json;
                        command.Parameters.Add("ewayjson", NpgsqlTypes.NpgsqlDbType.Json).Value = oBundles.eway_json;
                        command.ExecuteNonQuery();

                        StocktransactionDataService stocktransaction = new StocktransactionDataService() { Context = this.Context };
                        stocktransaction.SwapGoods(oBundles, transaction, conn);

                    }

                    if (Services.ConfigManager.Instance(Context.OrganizationID).Config.intray_notification_inward &&
                        oBundles.status == 1)
                    {
                        Services.IntrayManager.Instance(Context.OrganizationID).Add(Context.OrganizationID, Services.IntrayManager.EntryType.Inward,
                                oBundles.id, string.Format("BUNDLE INWARD SUCCESSFULLY WITH REFERENCE NO {0} ", oBundles.code),
                                Context.UserID, oBundles.tolocationid, transaction, Context);
                    }
                    else if (Services.ConfigManager.Instance(Context.OrganizationID).Config.intray_notification_inward &&
                        oBundles.status == 2)
                    {
                        Services.IntrayManager.Instance(Context.OrganizationID).Add(Context.OrganizationID, Services.IntrayManager.EntryType.Inward,
                                oBundles.id, string.Format("PARTIAL BUNDLE INWARD SUCCESSFULLY WITH REFERENCE NO {0} ", oBundles.code),
                                Context.UserID, oBundles.tolocationid, transaction, Context);
                    }

                    if (oBundles.items.Any(e => e.qty < e.aqty))
                        throw new Exception(oBundles.items.First(e => e.qty < e.aqty).barcode + " accepted qty higher than inwarded");

                    if (oBundles.items.Any(e => e.aqty < 0))
                        throw new Exception(oBundles.items.First(e => e.aqty < 0).barcode + " Inwarded qty must be non negative.");

                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                oBundles.id = 0;
                oBundles.ErrorMessage = exp.Message;
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oBundles);
            }

            return oBundles;
        }
        public class UANStockItem
        {
            public string barcode { get; set; }
            public decimal count { get; set; }
            public decimal allocated { get; set; }
        }
        internal List<Bundleitem> SearchUANGoodsStock(BundleStockCreteria item)
        {
            List<Bundleitem> lst = new List<Bundleitem>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                SortedDictionary<string, decimal> packqty = null;
                NpgsqlCommand command = null;

                QueryBuilder qb = new QueryBuilder();
                var blist = item.barcode.Split(',').ToList();
                var uanlist = blist.GroupBy(e => e).ToList().ConvertAll(ex => new UANStockItem() { barcode = ex.Key, count = ex.Count(), allocated = decimal.Zero });
                var barecodes = uanlist.ConvertAll(e => "'" + WebAPI.Data.Utility.SafeSQL(e.barcode) + "'").ToList();

                //if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_fixed_barcodes)
                //{
                //    barecodes = SellablebarcodesDataService.Instance.GetCoreBarcodes(barecodes);
                //}

                qb.CurrentQuery = @"select stock.id stockid,salablegoods.id as id,stock.barcode,salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty) as stock,stock.itemid,stock.productid,
                        stock.supplierid,stock.printingname,referencelist.name as locationname,stock.lrreference,supplier.name as suppliername,products.sellingmode,
                        stock.ibuyingprice buyingprice,lrinvoiceitems.mrp, (salablegoods.price-salablegoods.discount) price,stock.serialno,stock.designid from salablegoods
			            inner join stock on stock.id = salablegoods.stockid
                        left outer join lrinvoiceitems on lrinvoiceitems.id = stock.invoiceitemid
                        inner join referencelist on referencelist.id = salablegoods.locationid
                        left outer join products on products.id = stock.productid
						left outer join supplier on supplier.id = stock.supplierid
                        left outer join items on items.id = salablegoods.itemid
                        where salablegoods.isactive = true and salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty)>0 and stock.serialno in (" + string.Join(",", barecodes) + ")";

                if (item.exclusionlist != null && item.exclusionlist.Count > 0)
                {
                    qb.CurrentQuery = qb.CurrentQuery + " and stock.id not in (" + string.Join(",", item.exclusionlist) + ")";
                }

                //if (item.locationid > 0)
                qb.AddParameter("salablegoods.locationid", "locationid", item.locationid, "=");

                if (item.companyid > 0 && InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_interstatetransfer)
                    qb.AddParameter("salablegoods.companyid", "companyid", item.companyid, "=");

                qb.SortField = "stock.id";
                command = qb.GetCommand(conn);
                command.CommandTimeout = 10000;
                command.Parameters.Add(new NpgsqlParameter()
                {
                    ParameterName = "locationid",
                    NpgsqlDbType = NpgsqlTypes.NpgsqlDbType.Bigint,
                    Value = item.locationid
                });
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Bundleitem ostock = new Bundleitem();
                        ostock.stockid = reader["stockid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["stockid"]);
                        ostock.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        ostock.printingname = reader["printingname"] == DBNull.Value ? "" : reader["printingname"].ToString();
                        ostock.lrreference = reader["lrreference"] == DBNull.Value ? "" : reader["lrreference"].ToString();
                        ostock.suppliername = reader["suppliername"] == DBNull.Value ? "" : reader["suppliername"].ToString();
                        ostock.barcode = reader["barcode"] == DBNull.Value ? "" : reader["barcode"].ToString();
                        ostock.serialno = reader["serialno"] == DBNull.Value ? "" : reader["serialno"].ToString();
                        ostock.designid = reader["designid"] == DBNull.Value ? "" : reader["designid"].ToString();
                        ostock.itemid = reader["itemid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["itemid"].ToString());
                        ostock.productid = reader["productid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productid"].ToString());
                        ostock.qty = reader["stock"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["stock"].ToString());
                        ostock.supplierid = reader["supplierid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["supplierid"]);
                        ostock.ismeter = reader["sellingmode"] == DBNull.Value ? false : Convert.ToInt32(reader["sellingmode"]) == 3;

                        if (!Context.HasPermission(PermissionSet.showvalue, InfyPOS.Common.Action.View))
                            ostock.buyingprice = reader["buyingprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["buyingprice"].ToString());
                        if (!Context.HasPermission(PermissionSet.showsellingvalue, InfyPOS.Common.Action.View))
                            ostock.price = reader["price"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["price"].ToString());

                        ostock.mrp = reader["mrp"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["mrp"].ToString());
                        if (item.locationid == 0)
                            ostock.location = reader["locationname"] == DBNull.Value ? "" : reader["locationname"].ToString();
                        if (packqty != null && packqty.ContainsKey(ostock.barcode))
                            ostock.pqty = packqty[ostock.barcode];

                        if (uanlist.Exists(e => e.barcode == ostock.serialno && e.count - e.allocated > 0))
                        {
                            var uanitem = uanlist.Find(e => e.barcode == ostock.serialno && e.count - e.allocated > 0);
                            if (uanitem.count - uanitem.allocated > ostock.qty)
                            {
                                uanitem.allocated += ostock.qty;
                                ostock.iqty = ostock.qty;
                            }
                            else
                            {
                                ostock.iqty = uanitem.count - uanitem.allocated;
                                uanitem.allocated += uanitem.count;
                            }
                            ostock.pqty = ostock.iqty;
                            lst.Add(ostock);
                        }
                        else
                        {
                            var roundlist = lst.FindAll(e => e.serialno == ostock.serialno);
                            var toaliqty = roundlist.Sum(e => e.iqty);
                            var toalpqty = roundlist.Sum(e => e.pqty);
                        }
                    }
                }
                conn.Close();
                if (Services.ConfigManager.Instance(Context.OrganizationID).Config.system_ismfgapplication)
                {
                    ReferencelistDataService referencelistDataService = new ReferencelistDataService() { Context = this.Context };
                    var sizelist = referencelistDataService.Search(new ReferencelistCreteria() { type = "SIZE" });

                    var designs = lst.FindAll(x => !string.IsNullOrEmpty(x.designid)).ConvertAll(y => y.designid).Distinct().ToList();
                    if (designs.Count > 0)
                    {
                        MfgdesignDataService mfgdesignDataService = new MfgdesignDataService() { Context = this.Context };
                        var designList = mfgdesignDataService.SearchDesignList(new MfgdesignCreteria() { codes = designs.ConvertAll(x => "'" + x + "'") });
                        foreach (var ditem in lst.Where(x => !string.IsNullOrEmpty(x.designid) && !string.IsNullOrEmpty(x.serialno)))
                        {
                            if (designList.Exists(x => x.code == ditem.designid && x.sizegroup.items.Exists(y => y.uid == ditem.serialno)))
                            {
                                var ritem = designList.Find(x => x.code == ditem.designid).sizegroup.items.Find(x => x.uid == ditem.serialno);
                                ditem.designid = ritem.code;
                                if (sizelist.Exists(x => x.id == ritem.id))
                                    ditem.designid += "/" + sizelist.Find(x => x.id == ritem.id).name;
                            }
                        }
                    }
                }
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, item);
            }

            return lst;
        }

        internal List<Bundleitem> SearchGoodsStock(BundleStockCreteria item)
        {
            if (string.IsNullOrEmpty(item.barcode)) return null;

            List<Bundleitem> lst = new List<Bundleitem>();
            PackbarcodeDataService packdataservice = new PackbarcodeDataService() { Context = this.Context };
            List<Packbarcode> packs = null;
            SortedDictionary<string, decimal> packqty = null;

            if (item.searchtype != 6)
            {
                if (!string.IsNullOrEmpty(InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_barcodeparser))
                {
                    item.barcode = InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.POSParser.UpdateBarcode(item.barcode);
                }

                if (item.barcode.EndsWith("$"))
                {
                    packs = packdataservice.Search(new PackbarcodeCreteria()
                    {
                        barcode = item.barcode
                    });
                    if (packs.Count > 0)
                    {
                        packqty = new SortedDictionary<string, decimal>();
                        foreach (var pitem in packs.First().barcodes)
                        {
                            if (!packqty.ContainsKey(pitem.barcode))
                                packqty.Add(pitem.barcode, pitem.pieces);
                            else
                                packqty[pitem.barcode] += pitem.pieces;
                        }
                        item.barcode = string.Join(",", packqty.Keys.ToList());
                    }
                }

                if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_fixed_barcodes &&
                    !InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_hyper_uan)
                {
                    var barecodes = item.barcode.Split(',').ToList();
                    lst.AddRange(SearchUANGoodsStock(item));
                    if (item.exclusionlist != null && item.exclusionlist.Count > 0)
                        return lst;
                    barecodes.RemoveAll(e => lst.Exists(x => x.serialno == e));
                    item.barcode = string.Join(",", barecodes.GroupBy(e => e).ToList().ConvertAll(e => "'" + e.Key + "'"));
                }

                if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_ismfgapplication &&
                    InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_hyper_uan)
                {
                    var barecodes = item.barcode.Split(',').ToList();

                    SellablebarcodesDataService sellablebarcodesDataService = new SellablebarcodesDataService() { Context = this.Context };
                    var sbarcodelist = sellablebarcodesDataService.Search(new SellablebarcodesCreteria()
                    {
                        ids = barecodes
                    });

                    sbarcodelist.ForEach(x =>
                    {
                        var count = barecodes.Count(y => y == x.barcode);
                        barecodes.RemoveAll(y => y == x.barcode);
                        for (var i = 0; i < count; i++)
                            barecodes.Add(x.corebarcode);
                    });

                    if (barecodes.Count > 0)
                        barecodes.AddRange(item.barcode.Split(',').ToList());

                    item.barcode = String.Join(",", barecodes);
                }
            }
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;

                QueryBuilder qb = new QueryBuilder();

                var barecodes = item.barcode.Split(',').ToList().ConvertAll(e => "'" + WebAPI.Data.Utility.SafeSQL(e) + "'").ToList();
                //if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_fixed_barcodes)
                //{
                //    barecodes = SellablebarcodesDataService.Instance.GetCoreBarcodes(barecodes);
                //}

                if (item.searchtype == 6)
                {
                    qb.CurrentQuery = @"select stock.id stockid,salablegoods.id as id,stock.barcode,salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty) as stock,stock.itemid,stock.productid,
                        stock.supplierid,stock.printingname,referencelist.name as locationname,stock.lrreference,supplier.name as suppliername,products.sellingmode,
                        stock.ibuyingprice buyingprice,lrinvoiceitems.mrp, (salablegoods.price-salablegoods.discount) price,stock.serialno,stock.designid 
                        from salablegoods
			            inner join stock on stock.id = salablegoods.stockid
                        inner join lrinvoiceitems on lrinvoiceitems.id = stock.invoiceitemid
                        inner join lrinvoice on lrinvoice.id = lrinvoiceitems.lrinvoiceid
                        inner join lrentry on lrentry.id = lrinvoice.lrid
                        inner join referencelist on referencelist.id = salablegoods.locationid
                        left outer join products on products.id = stock.productid
						left outer join supplier on supplier.id = stock.supplierid
                        left outer join items on items.id = salablegoods.itemid
                        where salablegoods.isactive = true and lrentry.isactive=true and lrinvoice.isactive=true and lrinvoiceitems.isactive=true
                        and stock.isactive=true 
                        and lrentry.lrentryno in (" + string.Join(",", barecodes) + ")";
                }
                else
                {
                    qb.CurrentQuery = @"select stock.id stockid,salablegoods.id as id,stock.barcode,salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty) as stock,stock.itemid,stock.productid,
                        stock.supplierid,stock.printingname,referencelist.name as locationname,stock.lrreference,supplier.name as suppliername,products.sellingmode,
                        stock.ibuyingprice buyingprice,lrinvoiceitems.mrp, (salablegoods.price-salablegoods.discount) price,stock.serialno,stock.designid from salablegoods
			            inner join stock on stock.id = salablegoods.stockid
                        left outer join lrinvoiceitems on lrinvoiceitems.id = stock.invoiceitemid
                        inner join referencelist on referencelist.id = salablegoods.locationid
                        left outer join products on products.id = stock.productid
						left outer join supplier on supplier.id = stock.supplierid
                        left outer join items on items.id = salablegoods.itemid
                        where salablegoods.isactive = true and salablegoods.barcode in (" + string.Join(",", barecodes) + ")";
                }

                if (item.locationid > 0)
                    qb.AddParameter("salablegoods.locationid", "locationid", item.locationid, "=");

                if (item.companyid > 0 && InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_interstatetransfer)
                    qb.AddParameter("salablegoods.companyid", "companyid", item.companyid, "=");

                qb.SortField = "stock.id";
                command = qb.GetCommand(conn);
                command.Parameters.Add(new NpgsqlParameter()
                {
                    ParameterName = "locationid",
                    NpgsqlDbType = NpgsqlTypes.NpgsqlDbType.Bigint,
                    Value = item.locationid
                });
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Bundleitem ostock = new Bundleitem();
                        ostock.stockid = reader["stockid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["stockid"]);
                        ostock.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        ostock.printingname = reader["printingname"] == DBNull.Value ? "" : reader["printingname"].ToString();
                        ostock.lrreference = reader["lrreference"] == DBNull.Value ? "" : reader["lrreference"].ToString();
                        ostock.suppliername = reader["suppliername"] == DBNull.Value ? "" : reader["suppliername"].ToString();
                        ostock.barcode = reader["barcode"] == DBNull.Value ? "" : reader["barcode"].ToString();
                        ostock.serialno = reader["serialno"] == DBNull.Value ? "" : reader["serialno"].ToString();
                        ostock.designid = reader["designid"] == DBNull.Value ? "" : reader["designid"].ToString();
                        ostock.itemid = reader["itemid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["itemid"].ToString());
                        ostock.productid = reader["productid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productid"].ToString());
                        ostock.qty = reader["stock"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["stock"].ToString());
                        ostock.supplierid = reader["supplierid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["supplierid"]);
                        ostock.ismeter = reader["sellingmode"] == DBNull.Value ? false : Convert.ToInt32(reader["sellingmode"]) == 3;

                        if (!Context.HasPermission(PermissionSet.showvalue, InfyPOS.Common.Action.View))
                            ostock.buyingprice = reader["buyingprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["buyingprice"].ToString());
                        if (!Context.HasPermission(PermissionSet.showsellingvalue, InfyPOS.Common.Action.View))
                            ostock.price = reader["price"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["price"].ToString());

                        ostock.mrp = reader["mrp"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["mrp"].ToString());
                        if (item.locationid == 0)
                            ostock.location = reader["locationname"] == DBNull.Value ? "" : reader["locationname"].ToString();
                        if (packqty != null && packqty.ContainsKey(ostock.barcode))
                            ostock.pqty = packqty[ostock.barcode];
                        if (lst.Exists(e => e.stockid == ostock.stockid))
                        {
                            lst.Find(e => e.stockid == ostock.stockid).qty += ostock.qty;
                            lst.Find(e => e.stockid == ostock.stockid).pqty += ostock.pqty;
                        }
                        else
                        {
                            lst.Add(ostock);
                        }
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, item);
            }


            return lst;
        }
        internal List<Bundleitem> SearchDesignStock(BundleStockCreteria item)
        {
            if (item.designcode == null) return null;

            MfgdesignDataService mfgdesignDataService = new MfgdesignDataService() { Context = this.Context };
            string newuid = "";
            var designid = mfgdesignDataService.GetDesignID(item.designcode.Split('/')[0], out newuid);
            if (designid <= 0)
                designid = mfgdesignDataService.GetDesignID(item.designcode, out newuid);

            if (designid <= 0)
                throw new Exception("Cant find the design - " + item.designcode);
            var design = new Mfgdesign() { id = designid };
            ReferencelistDataService referencelistDataService = new ReferencelistDataService() { Context = this.Context };
            var colourlist = referencelistDataService.SearchList(new ReferencelistCreteria()
            {
                type = "COLOUR"
            });
            mfgdesignDataService.UpdateSizeGroup(colourlist, new List<Mfgdesign>(new Mfgdesign[] { design }));


            if (design.sizegroup.items.Exists(x => x.code == item.designcode))
            {
                design.sizegroup.items.RemoveAll(x => x.code != item.designcode);
            }
            var alldesigncodes = design.sizegroup.items.GroupBy(x => x.code).ToList().ConvertAll(x => x.Key);
            if (alldesigncodes.Count > 1)
            {
                throw new Exception("Please enter either one " + string.Join(",", alldesigncodes));
            }
            var alluids = design.sizegroup.items.ConvertAll(x => x.uid);
            List<Bundleitem> lst = new List<Bundleitem>();
            List<string> uanlist = new List<string>();
            if (item.requiredesignqty != null)
            {
                var sb = new StringBuilder();
                foreach (var uqtyitem in item.requiredesignqty)
                {
                    if (design.sizegroup.items.Exists(x => x.sizeid == uqtyitem.id))
                    {
                        var uid = design.sizegroup.items.Find(x => x.sizeid == uqtyitem.id).uid;
                        var output = sb.Insert(sb.Length, uid + ",", (int)uqtyitem.qty).ToString();
                    }
                }
                lst.AddRange(SearchUANGoodsStock(new BundleStockCreteria()
                {
                    barcode = sb.ToString(),
                    locationid = item.locationid
                }));

                lst.ForEach(x =>
                {
                    if (design.sizegroup.items.Exists(y => y.uid == x.serialno))
                    {
                        var uanitem = design.sizegroup.items.Find(y => y.uid == x.serialno);
                        if (item.requiredesignqty.Exists(y => y.id == uanitem.sizeid))
                        {
                            x.sizename = item.requiredesignqty.Find(y => y.id == uanitem.sizeid).name;
                            x.printingname += "/" + item.designcode + "/" + x.sizename;
                            x.mrp = uanitem.mrp;
                        }
                    }
                });
            }

            return lst;
        }

        public List<BundleDetail> Search(BundlesCreteria _bundles)
        {
            List<BundleDetail> lst = new List<BundleDetail>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_bundles.id != 0)
                {
                    command = GetCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _bundles.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();

                    if (_bundles.id > 0 || !string.IsNullOrEmpty(_bundles.code) || _bundles.includeitems || (_bundles.ids != null && _bundles.ids.Count > 0))
                        qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    else
                        qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where")).Replace(",items", "");

                    qb.AddParameter("bundles.isactive", true, "=");
                    //qb.AddParameter("returngoods", isgoods, "=");

                    if (_bundles.bystatusid)
                    {
                        if (_bundles.statusid == 0)
                            qb.AddORParameter("bundles.status", 0, "<=", "bundles.status=3");
                        else if (_bundles.statusid != 10)
                            qb.AddParameter("bundles.status", _bundles.statusid, "=");
                    }
                    else
                    {
                        if (_bundles.ids != null && _bundles.ids.Count > 0)
                        {
                            qb.AddInParamer("bundles.id", string.Join<long>(",", _bundles.ids));
                        }
                        else if (!string.IsNullOrEmpty(_bundles.code))
                        {
                            qb.AddParameter("bundles.code", _bundles.code, "=");
                        }
                        else
                        {
                            if (_bundles.status >= 0)
                                qb.AddParameter("bundles.status", _bundles.status, "=");

                            if (_bundles.fromwarehouseoutward)
                            {
                                qb.AddParameter("bundles.returngoods", false, "=");
                            }

                            if (_bundles.fromshowroom || _bundles.fromwarehousereturn || isgoods)
                            {
                                qb.AddParameter("bundles.returngoods", true, "=");
                            }

                            if (_bundles.fromwarehousereturn)
                            {
                                qb.AddParameter("inward2warehouse", true, "=");
                            }
                        }
                    }
                    if (_bundles.modifiedon.Year > 2000)
                        qb.AddParameter("bundles.modifiedon", _bundles.modifiedon, ">");
                    if (_bundles.fromlocationid > 0)
                        qb.AddParameter("bundles.fromlocationid", _bundles.fromlocationid, "=");
                    if (_bundles.tolocationid > 0)
                        qb.AddParameter("bundles.tolocationid", _bundles.tolocationid, "=");
                    if (_bundles.fromdate.Year > 2000)
                        qb.AddParameter("bundles.packedon", "fromdate", _bundles.fromdate, " >= ");
                    if (_bundles.todate.Year > 2000)
                        qb.AddParameter("bundles.packedon", "todate", _bundles.todate.AddDays(1).Date, " < ");
                    if (_bundles.status == -1)
                        qb.Limit = 100;
                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        BundleDetail obundles = new BundleDetail();
                        obundles.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        obundles.lrentryno = reader["lrentryno"] == DBNull.Value ? "" : reader["lrentryno"].ToString();
                        obundles.billno = reader["billno"] == DBNull.Value ? "" : reader["billno"].ToString();
                        obundles.code = reader["code"] == DBNull.Value ? "" : reader["code"].ToString();
                        obundles.remarks = reader["remarks"] == DBNull.Value ? "" : reader["remarks"].ToString();
                        obundles.scanmode = reader["scanmode"] == DBNull.Value ? false : Convert.ToBoolean(reader["scanmode"]);
                        obundles.actionemployee = reader["actionemployee"] == DBNull.Value ? "" : Convert.ToString(reader["actionemployee"]);
                        obundles.tocompanyid = reader["tocompanyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["tocompanyid"].ToString());
                        obundles.fromcompanyid = reader["fromcompanyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["fromcompanyid"].ToString());
                        obundles.invoiceid = reader["invoiceid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["invoiceid"].ToString());
                        obundles.billid = reader["billid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["billid"].ToString());
                        obundles.iscompanytransfer = reader["iscompanytransfer"] == DBNull.Value ? false : Convert.ToBoolean(reader["iscompanytransfer"]);

                        obundles.fromlocationid = reader["fromlocationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["fromlocationid"].ToString());
                        obundles.tolocationid = reader["tolocationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["tolocationid"].ToString());
                        obundles.packedby = reader["packedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["packedby"].ToString());
                        obundles.pricetype = reader["pricetype"] == DBNull.Value ? 0 : Convert.ToInt64(reader["pricetype"].ToString());
                        obundles.packedbyemployee = reader["packedbyemployee"] == DBNull.Value ? "" : reader["packedbyemployee"].ToString();
                        obundles.createdbyemployee = reader["createdbyemployee"] == DBNull.Value ? "" : reader["createdbyemployee"].ToString();
                        obundles.receivedby = reader["receivedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["receivedby"].ToString());
                        obundles.requesteddate = reader["requesteddate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["requesteddate"].ToString());
                        obundles.packedon = reader["packedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["packedon"].ToString());
                        obundles.receivedon = reader["receivedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["receivedon"].ToString());
                        obundles.notes = reader["notes"] == DBNull.Value ? "" : reader["notes"].ToString();
                        obundles.approvedby = reader["approvedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["approvedby"].ToString());
                        obundles.qty = reader["qty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["qty"].ToString());


                        obundles.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        obundles.inward2warehouse = reader["inward2warehouse"] == DBNull.Value ? false : Convert.ToBoolean(reader["inward2warehouse"]);
                        obundles.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        obundles.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        obundles.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        obundles.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        obundles.companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"]);
                        obundles.status = reader["status"] == DBNull.Value ? 0 : Convert.ToInt32(reader["status"].ToString());
                        obundles.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"].ToString());
                        obundles.vehicleno = reader["vehicleno"] == DBNull.Value ? "" : reader["vehicleno"].ToString();
                        obundles.ewb = reader["ewb"] == DBNull.Value ? "" : reader["ewb"].ToString();
                        obundles.transportid = reader["transportid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["transportid"]);
                        obundles.lrno = reader["lrno"] == DBNull.Value ? "" : reader["lrno"].ToString();
                        obundles.lrdate = reader["lrdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["lrdate"]);
                        obundles.itemsvalue = reader["itemsvalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["itemsvalue"].ToString());
                        obundles.scanned = reader["scanned"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["scanned"].ToString());

                        obundles.noofpieces = reader["noofpieces"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["noofpieces"].ToString());
                        obundles.noofbundles = reader["noofbundles"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["noofbundles"].ToString());

                        if (obundles.billid > 0)
                        {
                            obundles.transportid = reader["bitransportid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["bitransportid"]);
                            obundles.itemsvalue = reader["bibillvalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["bibillvalue"].ToString());
                            obundles.lrno = reader["bilrno"] == DBNull.Value ? "" : reader["bilrno"].ToString();
                            obundles.lrdate = reader["bilrdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["bilrdate"]);
                            obundles.ewb = reader["biewb"] == DBNull.Value ? "" : reader["biewb"].ToString();
                            obundles.einv = reader["bieinv"] == DBNull.Value ? "" : reader["bieinv"].ToString();
                        }

                        if (_bundles.id > 0 || _bundles.includeitems || !string.IsNullOrEmpty(_bundles.code) || (_bundles.ids != null && _bundles.ids.Count > 0))
                        {
                            obundles.items_json = reader["items"] == DBNull.Value ? "null" : reader["items"].ToString();
                            obundles.eway_json = reader["ewayjson"] == DBNull.Value ? "null" : reader["ewayjson"].ToString();
                            obundles.packslip_json = reader["packslip"] == DBNull.Value ? "null" : reader["packslip"].ToString();
                        }
                        lst.Add(obundles);
                    }
                }
                if (lst.Count > 0 && _bundles.validatetolocation && Services.ConfigManager.Instance(Context.OrganizationID).Config.analyser_access_location)
                {
                    lst.RemoveAll(e => !Context.accessiblelocation.Contains(e.tolocationid));
                    //if (lst.Count == 0)
                    //    throw new Exception("Location access restricted. pls check location permission");
                }
                if (lst.Count > 0 && _bundles.validatefromlocation && Services.ConfigManager.Instance(Context.OrganizationID).Config.analyser_access_location)
                {
                    lst.RemoveAll(e => !Context.accessiblelocation.Contains(e.fromlocationid));
                    //if (lst.Count == 0)
                    //    throw new Exception("Location access restricted. pls check location permission");
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _bundles);
            }

            if (lst.Count == 1 && !string.IsNullOrEmpty(_bundles.code) &&
                Services.ConfigManager.Instance(Context.OrganizationID).Config.system_invcol_pricetype)
            {
                string currentpricequery = @"select stock.id,fromsale.price,tosale.price,cast((schemelist ->> 'uprice') as numeric),stock.price from bundles
                                        LEFT JOIN LATERAL
                                        json_array_elements (case when bundles.items is null or bundles.items::text ='null' then '[]'::json else bundles.items :: json end) schemelist ON true
                                        inner join stock ON (schemelist ->> 'stockid') :: BIGINT = stock.id
                                        left outer join salablegoods fromsale ON  fromsale.stockid = stock.id and fromsale.locationid=@fromlocationid
                                        left outer join salablegoods tosale ON  tosale.stockid = stock.id and tosale.locationid=@tolocationid
                                        where bundles.id = @id;";

                var param = new SortedList<string, object>();
                param.Add("id", lst[0].id);
                param.Add("fromlocationid", lst[0].fromlocationid);
                param.Add("tolocationid", lst[0].tolocationid);
                using (var reader = ExecuteReader(currentpricequery, param))
                {
                    while (reader.Read())
                    {
                        var id = reader.GetInt64(0);
                        var fprice = reader.IsDBNull(1) ? 0 : reader.GetDecimal(1);
                        var tprice = reader.IsDBNull(2) ? 0 : reader.GetDecimal(2);
                        if (reader.IsDBNull(1))
                        {
                            lst[0].items.Find(x => x.stockid == id).price = reader.GetDecimal(4);
                            fprice = reader.GetDecimal(4);
                        }
                        else
                        {
                            lst[0].items.Find(x => x.stockid == id).price = fprice;
                        }
                        if (reader.IsDBNull(2))
                            lst[0].items.Find(x => x.stockid == id).uprice = fprice;
                        else
                            lst[0].items.Find(x => x.stockid == id).uprice = tprice;
                        if (!reader.IsDBNull(3))
                            lst[0].items.Find(x => x.stockid == id).uprice = reader.GetDecimal(3);
                    }
                }
            }
            return lst;
        }


        public void FixBillUAN()
        {
            StockDataService stockDataService = new StockDataService() { Context = new UserClaim() };
            using (NpgsqlConnection conn = CreateConnection())
            {
                try
                {
                    List<Stock> stocks = new List<Stock>();
                    conn.Open();
                    using (var reader = ExecuteReader(@"with billqty
as
(
select billitems.barcode, billitems.salablegoodsid,bill.locationid, sum(billitems.qty) qty from billitems
inner join bill on bill.id = billitems.billid
where bill.isactive = true and billitems.isactive=true
group by billitems.barcode, billitems.salablegoodsid,bill.locationid
)

select billqty.*,salablegoods.iqty stockqty,stock.serialno from billqty
inner join salablegoods on salablegoods.id = billqty.salablegoodsid
inner join stock on stock.id =salablegoods.stockid 
where iqty < billqty.qty", null, conn))
                    {
                        while (reader.Read())
                        {
                            stocks.Add(new Stock()
                            {
                                barcode = reader["barcode"].ToString(),
                                id = (Int64)reader["salablegoodsid"],
                                locationid = (Int64)reader["locationid"],
                                buyingqty = (decimal)reader["stockqty"],
                                qty = (decimal)reader["qty"],
                                serialno = reader["serialno"] != DBNull.Value ? reader["serialno"].ToString() : ""
                            });
                        }
                    }

                    List<Billitems> UpdatableList = new List<Billitems>();
                    List<String> problembarcodes = new List<string>();
                    BillitemsDataService billitemsDataService = new BillitemsDataService() { Context = new UserClaim() };
                    SalablegoodsDataService salablegoodsDataService = new SalablegoodsDataService() { Context = new UserClaim() };
                    foreach (var bitem in stocks.GroupBy(e => new { e.serialno, e.locationid }))
                    {
                        string query = @"select billitems.id from billitems 
inner join bill on bill.id = billitems.billid
inner join stock on stock.id = stockid
where bill.isactive = true and billitems.isactive=true and stock.serialno ='" + WebAPI.Data.Utility.SafeSQL(bitem.Key.serialno) + "' and bill.locationid=" + bitem.Key.locationid;

                        List<long> billids = new List<long>();
                        using (var reader = ExecuteReader(query, null, conn))
                        {
                            while (reader.Read())
                                billids.Add(reader.GetInt64(0));
                        }

                        query = @"select salablegoods.id from salablegoods
inner join stock on stock.id = stockid
where stock.serialno ='" + bitem.Key.serialno + "' and salablegoods.locationid=" + bitem.Key.locationid;
                        List<long> stockids = new List<long>();
                        using (var reader = ExecuteReader(query, null, conn))
                        {
                            while (reader.Read())
                                stockids.Add(reader.GetInt64(0));
                        }

                        var billitems = billitemsDataService.Search(new BillitemsCreteria()
                        {
                            ids = billids
                        });

                        var stockitems = salablegoodsDataService.Search(new SalablegoodsCreteria()
                        {
                            ids = stockids
                        });

                        stockitems.ForEach(e =>
                        {
                            if (e.oqty > e.iqty)
                                e.oqty = e.iqty;
                            e.dqty = 0;
                        });

                        foreach (var bgroup in billitems.GroupBy(e => e.barcode))
                        {
                            System.Diagnostics.Debug.WriteLine(bgroup.Key);
                            var possibleqty = stockitems.Find(e => e.barcode == bgroup.Key).iqty;
                            var soldqty = bgroup.Sum(e => e.qty);
                            if (possibleqty < soldqty)
                            {
                                var billitemlist = bgroup.OrderByDescending(e => e.qty).ToList();
                                var avil = stockitems.FindAll(e => e.iqty - (e.oqty) > 0);
                                var allocateqty = soldqty - possibleqty;
                                for (var i = 0; allocateqty > 0 && i < billitemlist.Count; i++)
                                {
                                    if (billitemlist[i].qty < 0) continue;
                                    if (avil.Exists(e => e.iqty - (e.oqty) >= allocateqty))
                                    {
                                        var stockrepl = avil.Find(e => e.iqty - (e.oqty) >= allocateqty);
                                        billitemlist[i].stockid = stockrepl.stockid;
                                        billitemlist[i].salablegoodsid = stockrepl.id;
                                        billitemlist[i].barcode = stockrepl.barcode;
                                        UpdatableList.Add(billitemlist[i]);
                                        allocateqty = 0;
                                    }
                                    else if (avil.Exists(e => e.iqty - (e.oqty) >= billitemlist[i].qty))
                                    {
                                        var stockrepl = avil.Find(e => e.iqty - (e.oqty) >= billitemlist[i].qty);
                                        billitemlist[i].stockid = stockrepl.stockid;
                                        billitemlist[i].salablegoodsid = stockrepl.id;
                                        billitemlist[i].barcode = stockrepl.barcode;
                                        UpdatableList.Add(billitemlist[i]);
                                        allocateqty -= billitemlist[i].qty;
                                    }
                                    else
                                    {
                                        problembarcodes.Add(bgroup.Key);
                                        allocateqty = 0;
                                    }
                                }
                            }
                        }
                    }
                    var transaction = conn.BeginTransaction();
                    billitemsDataService.IsMigration = true;
                    foreach (var bitem in UpdatableList)
                    {
                        // billitemsDataService.Update(transaction, conn, bitem, bitem);
                    }

                    //var barcodelist = stockDataService.CreateBarcode(Processors.BarcodeManager.BarcodeType.Base10,
                    //    stocks.Count, 1, conn, transaction);

                    //var command = GetCommand(@"update stock set barcode=@barcode,serialno=@uan where id=@stockid;
                    //update salablegoods set barcode=@barcode where stockid=@stockid;
                    //update billitems set barcode=@barcode where stockid=@stockid", conn, transaction);
                    //command.Parameters.AddWithValue("barcode", "");
                    //command.Parameters.AddWithValue("uan", "");
                    //command.Parameters.AddWithValue("stockid", 0);
                    //for (int i = 0; i < stocks.Count; i++)
                    //{
                    //    command.Parameters["barcode"].Value = barcodelist[i].code;
                    //    command.Parameters["uan"].Value = stocks[i].barcode;
                    //    command.Parameters["stockid"].Value = stocks[i].id;
                    //    var affected = command.ExecuteNonQuery();
                    //}
                    transaction.Commit();
                    conn.Close();
                }
                catch (Exception exp)
                {

                }
            }
        }

        public void MoveBarcode2UAN()
        {
            StockDataService stockDataService = new StockDataService() { Context = new UserClaim() };
            using (NpgsqlConnection conn = CreateConnection())
            {
                try
                {
                    List<Stock> stocks = new List<Stock>();
                    conn.Open();
                    using (var reader = ExecuteReader("select barcode,serialno,id from stock where invoiceitemid=-1 and barcode not like 'A%'", null, conn))
                    {
                        while (reader.Read())
                        {
                            stocks.Add(new Stock()
                            {
                                barcode = reader["barcode"].ToString(),
                                id = (Int64)reader["id"],
                                serialno = reader["serialno"] != DBNull.Value ? reader["serialno"].ToString() : ""
                            });
                        }
                    }
                    var transaction = conn.BeginTransaction();
                    var barcodelist = stockDataService.CreateBarcode(Processors.BarcodeManager.BarcodeType.Base10,
                        stocks.Count, 1, conn, transaction);

                    var command = GetCommand(@"update stock set barcode=@barcode,serialno=@uan where id=@stockid;
                    update salablegoods set barcode=@barcode where stockid=@stockid;
                    update billitems set barcode=@barcode where stockid=@stockid", conn, transaction);
                    command.Parameters.AddWithValue("barcode", "");
                    command.Parameters.AddWithValue("uan", "");
                    command.Parameters.AddWithValue("stockid", 0);
                    for (int i = 0; i < stocks.Count; i++)
                    {
                        command.Parameters["barcode"].Value = barcodelist[i].code;
                        command.Parameters["uan"].Value = stocks[i].barcode;
                        command.Parameters["stockid"].Value = stocks[i].id;
                        var affected = command.ExecuteNonQuery();
                    }
                    transaction.Commit();
                    conn.Close();
                }
                catch (Exception exp)
                {

                }
            }
        }
        public SortedDictionary<string, decimal> FindMissingTransfers(long locationid, string[] barcodes)
        {
            string query = @"with jsonlist as
(
select value->>'key' barcode,sum(cast(value->>'qty' as numeric)) qty from 
json_array_elements(
'[{0}]':: json) 
	group by value->>'key'
),
stocklist
as
(
	select sum(iqty) qty,stock.serialno from salablegoods
	inner join stock on stock.id = stockid
	where salablegoods.isactive= true and salablegoods.locationid = {1} and stock.isactive=true
	group by stock.serialno
)
select barcode,jsonlist.qty transfered,stocklist.qty stockqty from jsonlist
left outer join stocklist on stocklist.serialno = jsonlist.barcode
where stocklist.serialno is null";
            // or stocklist.qty::numeric<jsonlist.qty
            var allbarcode = string.Join(",", new List<string>(barcodes).ConvertAll(e =>
            {
                var values = e.Split('\t');
                if (values.Length == 1)
                    values = new string[] { e, "1" };
                return @"{""key"":""" + values[0] + @""",""qty"":" + values[1] + "}";
            }));

            query = string.Format(query, allbarcode, locationid);


            SortedDictionary<string, decimal> lst = new SortedDictionary<string, decimal>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = GetCommand(query, conn);
                command.CommandTimeout = 100000;
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string barcode = reader.GetString(0);
                        decimal transfered = Convert.ToDecimal(reader["transfered"]);
                        decimal stockqty = reader["stockqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["stockqty"]);
                        if (transfered - stockqty > 0)
                            lst.Add(barcode, transfered - stockqty);
                    }
                }

                query = @"with jsonlist as
(
select value->>'key' barcode,sum(cast(value->>'qty' as numeric)) qty from 
json_array_elements(
'[{0}]':: json) 
	group by value->>'key'
)
select stock.barcode from jsonlist
inner join stock on stock.barcode = jsonlist.barcode
where stock.barcode like 'A%'";

                query = string.Format(query, allbarcode, locationid);
                command = GetCommand(query, conn);
                command.CommandTimeout = 100000;
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string barcode = reader.GetString(0);
                        if (lst.ContainsKey(barcode))
                            lst.Remove(barcode);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {

            }
            return lst;
        }
        public bool Validate(Bundles bundles)
        {
            try
            {
                // and products.sellingmode != 2
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                string query = string.Format(@"select (schemelist->>'barcode'),bundles.code,packedon,
                                        products.name productname from bundles
                                        INNER JOIN LATERAL json_array_elements (case when bundles.items is null or bundles.items::text ='null' then '[]'::json else bundles.items :: json end) schemelist ON true
                                        inner join products on products.id = cast((schemelist->>'productid') as bigint)
										where bundles.status = 0 and bundles.isactive = true {3} and
										(schemelist->>'barcode') in ({0}) and fromcompanyid={1} and fromlocationid={2}
                                        ",
                                        string.Join(",", bundles.items.ConvertAll(e => "'" + WebAPI.Data.Utility.SafeSQL(e.barcode) + "'")), bundles.fromcompanyid,
                                        bundles.fromlocationid,
                                        (Services.ConfigManager.Instance(Context.OrganizationID).Config.system_disableoutwardduplicate ? "and products.sellingmode != 2" : ""));
                using (var reader = ExecuteReader(query, null))
                {
                    while (reader.Read())
                    {
                        sb.AppendFormat("{0} packed on {1} in {2} pack ({3})\r\n", reader.GetString(0),
                            reader.GetDateTime(2).ToString("dd-MM-yyyy"), reader.GetString(1), reader.GetString(3));
                    }
                }
                if (sb.Length > 0)
                {
                    bundles.errormessage = sb.ToString();
                    return false;
                }
            }
            catch (Exception exp)
            {

            }
            return true;
        }
        public Bundles Insert(Bundles oBundles)
        {
            oBundles.status = 0;
            if (oBundles.items.Count == 0)
            {
                oBundles.errormessage = "No barcode found for transfer";
                return oBundles;
            }
            NpgsqlTransaction transaction = null;
            try
            {
                if (oBundles.fromcompanyid == 0 && oBundles.companyid > 0)
                {
                    oBundles.fromcompanyid = oBundles.companyid;
                    oBundles.iscompanytransfer = (oBundles.tocompanyid > 0 && oBundles.fromcompanyid != oBundles.tocompanyid);
                }

                if (Services.ConfigManager.Instance(Context.OrganizationID).Config.system_interstatetransfer
                    && (oBundles.fromcompanyid == 0 || oBundles.tocompanyid == 0))
                {
                    oBundles.errormessage = "Inter company transfer details missing.";
                    return oBundles;
                }

                if (!Validate(oBundles))
                    return oBundles;

                var sequence = SequenceManager.Instance(Context.OrganizationID).Get("bundles.code",
                    InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_supportlocationwisebundleno ? oBundles.fromlocationid : 0);
                //oBundles.ToJSON();
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);
                    if (Services.ConfigManager.Instance(Context.OrganizationID).Config.system_randombundlecode)
                    {
                        InfyPOS.Processors.RandomManager randomManager = new Processors.RandomManager(6, Processors.RandomManager.Type.AlphaNumeric, "code", "bundles");
                        oBundles.code = randomManager.Generate();
                    }
                    else
                    {
                        oBundles.code = sequence.GetNextNumber(Context, conn, transaction);
                    }
                    oBundles.qty = oBundles.items.Sum(e => e.ismeter ? 1 : e.iqty);


                    //if (Services.ConfigManager.Instance(Context.OrganizationID).Config.intray_bundle)
                    //{
                    //    oBundles.status = -1;
                    //}
                    Insert(transaction, conn, oBundles);
                    //if (!isgoods)
                    //{
                    //    SalablegoodsDataService subSalablegoods = new SalablegoodsDataService(isgoods);
                    //    subSalablegoods.Context = this.Context;
                    //    List<long> stockids = new List<long>();
                    //    foreach(var stock in oBundles.items.GroupBy(e=>e.stockid))
                    //    {
                    //        stockids.Add(stock.Key);
                    //    }
                    //    StockDataService stockdataservice = new StockDataService() { Context = this.Context };
                    //    var priceList = stockdataservice.SearchForPrice(stockids);
                    //    foreach (var item in oBundles.items)
                    //    {
                    //        Salablegoods sitem = new Salablegoods();
                    //        sitem.inwardon = oBundles.receivedon;
                    //        sitem.barcode = item.barcode;
                    //        sitem.bundleid = oBundles.id;
                    //        sitem.iqty = item.iqty;
                    //        sitem.isactive = true;
                    //        sitem.itemid = item.itemid;
                    //        sitem.locationid = oBundles.tolocationid;
                    //        sitem.stockid = item.stockid;
                    //        sitem.version = 1;
                    //        sitem.companyid = oBundles.companyid;
                    //        sitem.supplierid = item.supplierid;
                    //        if(priceList.Exists(e=>e.id == sitem.stockid))
                    //        {
                    //            var price = priceList.Find(e => e.id == sitem.stockid);
                    //            sitem.price = price.price;
                    //            sitem.discount = price.discount;
                    //            sitem.additionaldiscount = price.additionaldiscount;
                    //            oBundles.Salablegoods.Add(sitem);
                    //            var existingsalablegoods = subSalablegoods.Search(new SalablegoodsCreteria()
                    //            {
                    //                locationid = oBundles.tolocationid,
                    //                barcode = item.barcode
                    //            });

                    //            if (existingsalablegoods != null && existingsalablegoods.Count > 0)
                    //            {
                    //                if (existingsalablegoods[0].stockid == item.stockid)
                    //                {
                    //                    existingsalablegoods[0].iqty += item.iqty;
                    //                    //existingsalablegoods[0].price = price.price;
                    //                    //existingsalablegoods[0].discount = price.discount;
                    //                    //existingsalablegoods[0].additionaldiscount = price.additionaldiscount;
                    //                }
                    //            }
                    //            else
                    //            {
                    //                subSalablegoods.Insert(transaction, conn, sitem);
                    //                item.id = sitem.id;
                    //            }
                    //        }
                    //    }
                    //}

                    //if (Services.ConfigManager.Instance(Context.OrganizationID).Config.intray_bundle)
                    //{
                    //    ReferencelistDataService referencelistDataService = new ReferencelistDataService() { Context = this.Context };
                    //    var locations = referencelistDataService.SearchList(new ReferencelistCreteria()
                    //    {
                    //        ids = new List<long>(new long[] { oBundles.fromlocationid, oBundles.tolocationid })
                    //    });

                    //    if (locations.Exists(x => x.id == oBundles.fromlocationid) &&
                    //        locations.Exists(x => x.id == oBundles.tolocationid))
                    //    {

                    //        Services.IntrayManager.Instance(Context.OrganizationID).Add(
                    //            Context.OrganizationID,
                    //            Services.IntrayManager.EntryType.Transfer,
                    //            oBundles.id, string.Format("TRANSFER REQUEST {3} FROM {0} TO {1} QTY {2}.",
                    //            locations.Find(x => x.id == oBundles.fromlocationid).name,
                    //            locations.Find(x => x.id == oBundles.tolocationid).name,
                    //            oBundles.items.Sum(x=>x.qty), oBundles.code), Context.UserID, 0, transaction, Context);
                    //    }
                    //}
                    if (Services.ConfigManager.Instance(Context.OrganizationID).Config.intray_notification_inward)
                    {
                        Services.IntrayManager.Instance(Context.OrganizationID).Add(Context.OrganizationID, Services.IntrayManager.EntryType.Inward,
                                oBundles.id, string.Format("NEW BUNDLE CREATED REFERENCE NO {0} ", oBundles.code),
                                Context.UserID, oBundles.tolocationid, transaction, Context);
                    }
                    if (oBundles.status == 2)
                    {
                        Services.IntrayManager.Instance(Context.OrganizationID).Add(Context.OrganizationID, Services.IntrayManager.EntryType.Inward,
                                oBundles.id, string.Format("PARTIAL BUNDLE INWARD REFERENCE NO {0} ", oBundles.code),
                                Context.UserID, oBundles.tolocationid, transaction, Context);
                    }
                    command = GetCommand(SQL_UpdateItems, conn, transaction);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBundles.id;
                    command.Parameters.Add("items", NpgsqlTypes.NpgsqlDbType.Json).Value = oBundles.items_json;
                    command.ExecuteNonQuery();


                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oBundles);
            }

            return oBundles;
        }

        public Bundles Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Bundles oBundles)
        {
            if (string.IsNullOrEmpty(oBundles.code))
                throw new Exception("Empty bundle code not allowed");

            if (Services.ConfigManager.Instance(Context.OrganizationID).Config.system_interstatetransfer
                        && (oBundles.fromcompanyid == 0 || oBundles.tocompanyid == 0))
            {
                oBundles.errormessage = "Inter company transfer details missing.";
                throw new Exception("Inter company transfer details missing.");
            }

            NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);
            //command.Parameters.Add("items", NpgsqlTypes.NpgsqlDbType.Json).Value = oBundles.items_json;
            command.Parameters.Add("code", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBundles.code == null ? "" : oBundles.code;
            command.Parameters.Add("remarks", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBundles.remarks == null ? "" : oBundles.remarks;
            command.Parameters.Add("fromlocationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBundles.fromlocationid;
            command.Parameters.Add("tolocationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBundles.tolocationid;
            command.Parameters.Add("scanmode", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBundles.scanmode;

            command.Parameters.Add("tocompanyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBundles.tocompanyid;
            command.Parameters.Add("fromcompanyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBundles.fromcompanyid;
            command.Parameters.Add("invoiceid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBundles.invoiceid;
            command.Parameters.Add("billid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBundles.billid;
            command.Parameters.Add("iscompanytransfer", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBundles.iscompanytransfer;

            command.Parameters.Add("packedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBundles.packedby;
            command.Parameters.Add("pricetype", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBundles.pricetype;
            command.Parameters.Add("qty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBundles.qty;
            command.Parameters.Add("receivedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBundles.receivedby;

            command.Parameters.Add("requesteddate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBundles.requesteddate.Date;
            command.Parameters.Add("packedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBundles.packedon.Date;
            command.Parameters.Add("receivedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBundles.receivedon;
            command.Parameters.Add("notes", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBundles.notes == null ? "" : oBundles.notes;
            command.Parameters.Add("approvedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBundles.approvedby;
            command.Parameters.Add("returngoods", NpgsqlTypes.NpgsqlDbType.Boolean).Value = isgoods;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBundles.isactive;
            command.Parameters.Add("inward2warehouse", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBundles.inward2warehouse;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBundles.companyid;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("status", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBundles.status;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBundles.version;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oBundles.id = Convert.ToInt64(reader[0]);
                }
            }

            return oBundles;
        }

        public class PendingStockItem
        {
            public long stockid { get; set; }
            public long itemid { get; set; }
            public long productid { get; set; }
            public long supplierid { get; set; }
            public decimal iqty { get; set; }
            public string barcode { get; set; }
            public long salablegoodsid { get; set; }
            public long fromlocationid { get; set; }
            public long tolocationid { get; set; }
        }
        public bool InwardAllPendingToRetail()
        {
            ReferencelistDataService referencelist = new ReferencelistDataService() { Context = this.Context };
            var alllocations = referencelist.Search(new ReferencelistCreteria() { type = "LOCATION" });
            var retloca = alllocations.FindAll(x => x.additionalinfo == null || !x.additionalinfo.iswarehouse);
            if (alllocations.Count(x => x.additionalinfo == null || !x.additionalinfo.iswarehouse) > 1)
            {
                throw new Exception("Multiple retail location, can't inward");
            }
            var retaillocation = alllocations.Find(x => x.additionalinfo == null || !x.additionalinfo.iswarehouse).id;

            string pendingstockquery = @"select stock.id,stock.barcode,stock.qty-stock.journalqty-stock.damagedqty-stock.transfered-stock.returnqty as iqty,
                        stock.itemid,stock.productid,
                        stock.supplierid from stock
						left outer join salablegoods on salablegoods.stockid=stock.id
                        where stock.isactive = true and stock.qty-stock.journalqty-stock.damagedqty-stock.transfered-stock.returnqty>0 
                        and salablegoods.id is null";

            pendingstockquery = @"select stock.id,stock.barcode,stock.qty-stock.journalqty-stock.damagedqty-stock.transfered-stock.returnqty as iqty,
                        stock.itemid,stock.productid,stock.locationid,
                        stock.supplierid from stock
                        where stock.isactive = true and stock.qty-stock.journalqty-stock.damagedqty-stock.transfered-stock.returnqty>0";

            List<PendingStockItem> pendingStockItems = new List<PendingStockItem>();
            using (var reader = ExecuteReader(pendingstockquery, new SortedList<string, object>()))
            {
                while (reader.Read())
                {
                    PendingStockItem ostock = new PendingStockItem();
                    ostock.stockid = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                    ostock.barcode = reader["barcode"] == DBNull.Value ? "" : reader["barcode"].ToString();
                    ostock.itemid = reader["itemid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["itemid"].ToString());
                    ostock.productid = reader["productid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productid"].ToString());
                    ostock.fromlocationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"].ToString());
                    ostock.iqty = reader["iqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["iqty"].ToString());
                    ostock.supplierid = reader["supplierid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["supplierid"]);
                    ostock.tolocationid = retaillocation;
                    pendingStockItems.Add(ostock);
                }
            }

            CompanyDataService companyDataService = new CompanyDataService() { Context = this.Context };
            var companyid = companyDataService.Search(new CompanyCreteria()).First().id;
            using (NpgsqlConnection conn = CreateConnection())
            {
                conn.Open();
                var transaction = conn.BeginTransaction();

                SalablegoodsDataService subSalablegoods = new SalablegoodsDataService() { Context = this.Context };
                subSalablegoods.Context = this.Context;
                subSalablegoods.isMigration = true;
                StockDataService stockdataservice = new StockDataService() { Context = this.Context };
                stockdataservice.Context = this.Context;
                var priceList = stockdataservice.SearchForPrice(pendingStockItems.ConvertAll(x => x.stockid));
                List<Salablegoods> salablegoodsInvoled = new List<Salablegoods>();
                foreach (var item in pendingStockItems)
                {
                    if (!priceList.Exists(e => e.id == item.stockid)) continue;
                    Salablegoods sitem = new Salablegoods();
                    sitem.inwardon = DateTimeNow;
                    sitem.barcode = item.barcode;
                    sitem.bundleid = 9;
                    sitem.iqty = item.iqty;
                    sitem.tiqty = item.iqty;
                    sitem.isactive = true;
                    sitem.itemid = item.itemid;
                    sitem.locationid = retaillocation;
                    sitem.stockid = item.stockid;
                    sitem.version = 100;
                    sitem.companyid = companyid;
                    sitem.supplierid = item.supplierid;
                    sitem.received = true;
                    var price = priceList.Find(e => e.id == sitem.stockid);
                    sitem.price = price.price;
                    sitem.discount = price.discount;
                    sitem.additionaldiscount = price.additionaldiscount;

                    var existingsalablegoods = subSalablegoods.Search(new SalablegoodsCreteria()
                    {
                        locationid = retaillocation,
                        stockid = item.stockid
                    });

                    if (existingsalablegoods != null && existingsalablegoods.Count > 0)
                    {
                        if (existingsalablegoods[0].stockid == item.stockid)
                        {
                            if (existingsalablegoods[0].producttype == 6 && existingsalablegoods[0].iqty > 0)
                            {
                                throw new Exception("Mobile can't exceed the stock more than 1 - " + existingsalablegoods[0].barcode);
                            }
                            existingsalablegoods[0].iqty += item.iqty;
                            existingsalablegoods[0].tiqty += item.iqty;
                            existingsalablegoods[0].received = true;
                            item.salablegoodsid = existingsalablegoods[0].id;
                            if (!subSalablegoods.Update(transaction, conn, existingsalablegoods[0], new Salablegoods()
                            {
                                iqty = existingsalablegoods[0].iqty - item.iqty
                            }))
                            {
                                item.iqty = 0;
                            }
                            else
                            {
                                salablegoodsInvoled.Add(existingsalablegoods[0]);
                            }
                        }
                    }
                    else
                    {
                        subSalablegoods.Insert(transaction, conn, sitem, false);
                        item.salablegoodsid = sitem.id;
                    }
                    salablegoodsInvoled.Add(sitem);
                    Console.WriteLine(salablegoodsInvoled.Count);
                }

                ExecuteQuery(@"with siqty
as
(
select sum(iqty) iqty,stockid from salablegoods
group by stockid
)

update stock set transfered=iqty from siqty
where siqty.stockid = stock.id and  transfered!=iqty", new SortedList<string, object>(), conn, transaction);

                StocktransactionDataService stocktransaction = new StocktransactionDataService() { Context = this.Context };
                stocktransaction.InwardAllPendingToRetail(pendingStockItems, transaction, conn);
                transaction.Commit();
                conn.Close();
            }
            return true;
        }
        public Bundles InwardFromWarehouse(Bundles oBundles)
        {
            NpgsqlTransaction transaction = null;
            try
            {
                if (oBundles.items.Any(e => e.qty < e.aqty))
                {
                    throw new Exception(oBundles.items.First(e => e.qty < e.aqty).barcode + " accepted qty higher than inwarded");
                }
                //oBundles.ToJSON();
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    if (!CanUpdate(transaction, conn, oBundles, false))
                    {
                        throw new Exception("Given package updated, please refresh and try again!");
                    }

                    SalablegoodsDataService subSalablegoods = new SalablegoodsDataService() { Context = this.Context };
                    subSalablegoods.Context = this.Context;
                    List<long> stockids = new List<long>();
                    foreach (var stock in oBundles.items.GroupBy(e => e.stockid))
                    {
                        stockids.Add(stock.Key);
                    }
                    StockDataService stockdataservice = new StockDataService() { Context = this.Context };
                    stockdataservice.Context = this.Context;
                    var priceList = stockdataservice.SearchForPrice(stockids);
                    List<Salablegoods> salablegoodsInvoled = new List<Salablegoods>();
                    foreach (var item in oBundles.items)
                    {
                        if (item.aqty <= 0 || !priceList.Exists(e => e.id == item.stockid)) continue;

                        Salablegoods sitem = new Salablegoods();
                        sitem.inwardon = oBundles.receivedon;
                        sitem.barcode = item.barcode;
                        sitem.bundleid = oBundles.id;
                        sitem.iqty = item.aqty;
                        sitem.tiqty = item.aqty;
                        sitem.isactive = true;
                        sitem.itemid = item.itemid;
                        sitem.locationid = oBundles.tolocationid;
                        sitem.stockid = item.stockid;
                        sitem.version = 1;
                        //sitem.companyid = (oBundles.iscompanytransfer && oBundles.tocompanyid > 0) ? oBundles.tocompanyid : oBundles.companyid;
                        sitem.companyid = oBundles.tocompanyid > 0 ? oBundles.tocompanyid : oBundles.companyid;
                        sitem.supplierid = item.supplierid;
                        sitem.received = true;

                        var price = priceList.Find(e => e.id == sitem.stockid);
                        sitem.price = price.price;
                        sitem.discount = price.discount;
                        sitem.additionaldiscount = price.additionaldiscount;
                        //oBundles.Salablegoods.Add(sitem);
                        var existingsalablegoods = subSalablegoods.Search(new SalablegoodsCreteria()
                        {
                            locationid = oBundles.tolocationid,
                            stockid = item.stockid
                        }, conn, transaction);

                        if (existingsalablegoods.Count == 0 && salablegoodsInvoled.Exists(x => x.barcode == sitem.barcode))
                        {
                            existingsalablegoods = salablegoodsInvoled.FindAll(x => x.barcode == sitem.barcode);
                        }

                        if (existingsalablegoods != null && existingsalablegoods.Count > 0)
                        {
                            if (existingsalablegoods[0].stockid == item.stockid)
                            {
                                if (existingsalablegoods[0].producttype == 6 && existingsalablegoods[0].iqty > 0)
                                {
                                    throw new Exception("Mobile can't exceed the stock more than 1 - " + existingsalablegoods[0].barcode);
                                }
                                item.toid = existingsalablegoods[0].id;
                                existingsalablegoods[0].iqty += item.aqty;
                                existingsalablegoods[0].tiqty += item.aqty;
                                existingsalablegoods[0].received = true;
                                if (!subSalablegoods.Update(transaction, conn, existingsalablegoods[0], new Salablegoods()
                                {
                                    iqty = existingsalablegoods[0].iqty - item.aqty
                                }))
                                {
                                    item.aqty = 0;
                                }
                                else
                                {
                                    salablegoodsInvoled.Add(existingsalablegoods[0]);
                                }
                            }
                        }
                        else
                        {
                            if (!subSalablegoods.Insert(transaction, conn, sitem, true))
                            {
                                item.aqty = 0;
                            }
                            else
                            {
                                salablegoodsInvoled.Add(sitem);
                            }
                            item.toid = sitem.id;
                        }
                    }

                    oBundles.status = oBundles.items.All(e => e.iqty == e.aqty) ? 1 : 2;
                    if (!Update(transaction, conn, oBundles, 0))
                    {
                        transaction.Rollback();
                        oBundles.errormessage = "Already got inwarded";
                        return oBundles;
                    }
                    else if (oBundles.status == 2)
                    {
                        Services.IntrayManager.Instance(Context.OrganizationID).Add(Context.OrganizationID, Services.IntrayManager.EntryType.Inward,
                                oBundles.id, string.Format("PARTIAL INWARD BUNDLE INWARD REFERENCE NO {0} ",
                                oBundles.code), Context.UserID, oBundles.tolocationid, transaction, Context);
                    }
                    NpgsqlCommand command = GetCommand(SQL_UpdateItems, conn, transaction);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBundles.id;
                    command.Parameters.Add("items", NpgsqlTypes.NpgsqlDbType.Json).Value = oBundles.items_json;
                    command.ExecuteNonQuery();
                    //command = GetCommand(SQL_InwardAll, conn, transaction);
                    //command.Parameters.Add("bundleid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBundles.id;
                    //command.ExecuteNonQuery();

                    StocktransactionDataService stocktransaction = new StocktransactionDataService() { Context = this.Context };
                    stocktransaction.InwardFromWarehouse(oBundles, transaction, conn);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oBundles);
            }
            return oBundles;
        }

        private Bill DoCompanyIntertransfer(Bundles oBundles, NpgsqlConnection conn, NpgsqlTransaction transaction)
        {
            CompanyDataService companyDataService = new CompanyDataService() { Context = this.Context };
            var fromcompany = companyDataService.Search(new CompanyCreteria()
            {
                id = oBundles.fromcompanyid
            }).First();
            var tocompany = companyDataService.Search(new CompanyCreteria()
            {
                id = oBundles.tocompanyid
            }).First();
            BillDataService billDataService = new BillDataService() { Context = this.Context };
            billDataService.Context = this.Context;

            List<MfgDesignItem> mfgDesignItems = null;
            var serialnos = oBundles.items.FindAll(x => !string.IsNullOrEmpty(x.serialno)).ConvertAll(x => x.serialno).Distinct().ToList();
            if (serialnos.Count > 0)
            {
                MfgdesignDataService mfgdesignDataService = new MfgdesignDataService() { Context = this.Context };
                mfgDesignItems = mfgdesignDataService.SearchDesignList(serialnos);
            }

            ReferencelistDataService referencelistDataService = new ReferencelistDataService() { Context = this.Context };
            var locations = referencelistDataService.Search(new ReferencelistCreteria()
            {
                ids = new List<long> { oBundles.fromlocationid, oBundles.tolocationid }
            });

            var fromlocation = locations.Find(x => x.id == oBundles.fromlocationid);
            var tolocation = locations.Find(x => x.id == oBundles.tolocationid);
            if (fromlocation.additionalinfo != null && fromlocation.additionalinfo.parentid > 0)
                fromcompany = companyDataService.Search(new CompanyCreteria()
                {
                    id = fromlocation.additionalinfo.parentid
                }).First();

            if (tolocation.additionalinfo != null && tolocation.additionalinfo.parentid > 0)
                tocompany = companyDataService.Search(new CompanyCreteria()
                {
                    id = tolocation.additionalinfo.parentid
                }).First();

            if (fromcompany == null || string.IsNullOrEmpty(fromcompany.gstno))
                throw new Exception(fromlocation.name + " company either not register or gst number is missing");

            if (tocompany == null || string.IsNullOrEmpty(tocompany.gstno))
                throw new Exception(tolocation.name + " company either not register or gst number is missing");


            ProductsDataService productsDataService = new ProductsDataService() { Context = this.Context };
            var sellingmodes = productsDataService.GetSellingModes(oBundles.items.ConvertAll(x => x.productid).ToList());

            Bill bill = new Bill();
            bill.isb2b = true;
            bill.billtype = Bill.BillTypes.Credit;
            bill.iscreditbill = true;
            bill.createdby = Context.UserID;
            bill.modifiedby = Context.UserID;
            bill.createdon = DateTimeNow;
            bill.modifiedon = DateTimeNow;
            bill.companyid = fromlocation.additionalinfo != null &&
                fromlocation.additionalinfo.parentid > 0 ? fromlocation.additionalinfo.parentid : fromcompany.id;
            bill.customerid = tolocation.additionalinfo != null &&
                tolocation.additionalinfo.customerid > 0 ? tolocation.additionalinfo.customerid : tocompany.company_as_customerid;
            bill.billdate = DateTimeNow.Date;
            //bill.DontChangeStock = true;
            bill.istransfer = true;
            bill.isinterstatesale = fromcompany.gstno.Substring(0, 2) != tocompany.gstno.Substring(0, 2);
            bill.isinterstatetransfer = fromcompany.gstno.Substring(0, 2) != tocompany.gstno.Substring(0, 2);
            bill.isactive = true;
            bill.version = 1;
            bill.locationid = oBundles.fromlocationid;
            bill.Billitems = new List<Billitems>();

            Data.TaxDataService taxDataService = new TaxDataService() { Context = this.Context };
            taxDataService.Context = this.Context;
            var taxsplits = taxDataService.RangedTaxProducts(oBundles.items.ConvertAll(x => x.barcode));
            foreach (var item in oBundles.items)
            {
                Billitems billitems = new Billitems();
                billitems.createdby = Context.UserID;
                billitems.modifiedby = Context.UserID;
                billitems.createdon = DateTimeNow;
                billitems.modifiedon = DateTimeNow;
                billitems.barcode = item.barcode;
                billitems.salablegoodsid = item.id;
                billitems.stockid = item.stockid;
                billitems.companyid = bill.companyid;
                billitems.isactive = true;
                billitems.printingname = item.printingname;
                billitems.productid = item.productid;
                billitems.locationid = oBundles.fromlocationid;
                billitems.barcode = item.barcode;
                billitems.attributes = new BillItemAttributes();
                billitems.qty = item.iqty;
                if (sellingmodes.ContainsKey(billitems.productid))
                {
                    billitems.sellingmode = sellingmodes[billitems.productid];
                }
                if (billitems.qty <= 0) continue;
                bill.Billitems.Add(billitems);
                billitems.piece = billitems.sellingmode == 3 ? 1 : billitems.qty;
                if (!string.IsNullOrEmpty(item.serialno) &&
                    mfgDesignItems != null &&
                    mfgDesignItems.Exists(x => x.uid == item.serialno))
                {
                    var mfgitem = mfgDesignItems.Find(x => x.uid == item.serialno);
                    billitems.attributes.serialno = item.serialno;
                    billitems.attributes.size = mfgitem.size;
                    billitems.attributes.designid = mfgitem.code;
                    billitems.attributes.colourname = mfgitem.colour;
                    billitems.attributes.colourcode = mfgitem.colourcode;
                    billitems.attributes.size = mfgitem.size;
                    billitems.attributes.mrp = mfgitem.mrp;
                    billitems.mrp = mfgitem.mrp;
                    billitems.hsncode = mfgitem.hsn;
                }

                var newPrice = item.price;

                if (Services.ConfigManager.Instance(Context.OrganizationID).Config.system_interstatetransfer &&
                    Services.ConfigManager.Instance(Context.OrganizationID).Config.system_interstatetransfermargin > 0)
                {
                    newPrice = billitems.price + Math.Round(newPrice * Services.ConfigManager.Instance(Context.OrganizationID).Config.system_interstatetransfermargin / 100, 0);
                }

                if (taxsplits.ContainsKey(billitems.barcode))
                {
                    if (taxsplits[billitems.barcode].taxsplit != null)
                    {
                        var _taxid = taxsplits[billitems.barcode].id;
                        var newtax = taxsplits[billitems.barcode].taxsplit.Find(newPrice);
                        if (newtax.taxpercent != billitems.taxpercentage)
                        {
                            billitems.taxpercentage = newtax.taxpercent;
                            billitems.taxid = _taxid;
                        }
                    }
                    else
                    {
                        billitems.taxpercentage = taxsplits[billitems.barcode].taxpercentage;
                        billitems.taxid = taxsplits[billitems.barcode].id;
                    }
                }
                billitems.tax = Math.Round(billitems.qty * newPrice * billitems.taxpercentage / 100, 4);
                billitems.price = newPrice + (billitems.tax / billitems.qty);
                if (taxsplits.ContainsKey(billitems.barcode))
                {
                    if (taxsplits[billitems.barcode].taxsplit != null)
                    {
                        var _taxid = taxsplits[billitems.barcode].id;
                        var newtax = taxsplits[billitems.barcode].taxsplit.Find(newPrice);
                        if (newtax.taxpercent != billitems.taxpercentage)
                        {
                            billitems.taxpercentage = newtax.taxpercent;
                            billitems.taxid = _taxid;
                            billitems.tax = Math.Round(billitems.qty * newPrice * billitems.taxpercentage / 100, 4);
                            billitems.price = newPrice + billitems.tax;
                        }
                    }
                }
                billitems.itemtax = Math.Round(1 * newPrice * billitems.taxpercentage / 100, 4);
                billitems.rate = newPrice;
                billitems.saleamount = billitems.rate;
                billitems.salerate = billitems.saleamount + billitems.itemtax;
                billitems.receivable = Math.Round(billitems.price * billitems.qty, 2);
            }

            bill.qty = bill.Billitems.Sum(e => e.qty);
            bill.tax = bill.Billitems.Sum(e => e.tax);
            bill.totalqty = bill.Billitems.Sum(e => e.qty);
            bill.totalpiece = bill.Billitems.Sum(e => e.piece);
            bill.receivable = bill.Billitems.Sum(e => e.receivable);
            bill.rounding = Math.Round(bill.receivable, MidpointRounding.AwayFromZero) - bill.receivable;

            var sequence = SequenceManager.Instance(Context.OrganizationID).Get("bill.billno", bill.locationid, bill.companyid);
            if (!sequence.Validate(Context, "Bill No", false))
            {
                InfyPOS.Data.ReferencelistDataService reflist = new ReferencelistDataService() { Context = this.Context };
                var location = reflist.Search(new ReferencelistCreteria()
                {
                    id = bill.locationid
                });
                if (location.Count == 1)
                    sequence.Validate(Context, "Bill No for " + location[0].name, true);
            }
            bill.billno = sequence.GetNextNumber(Context, conn, transaction);
            if (bill.billattributes == null)
                bill.billattributes = new BillAttribute();
            bill.billattributes.bundleid = oBundles.id;
            billDataService.Insert(transaction, conn, bill, new Billmaster()
            {
                id = bill.id,
                total = bill.total,
                billattributes = new BillAttribute()
                {
                    bundleid = oBundles.id
                },
            });


            if (bill.Billitems.Exists(x => string.IsNullOrEmpty(x.hsncode)))
            {
                string SQL = "update billitems set hsncode = products.hsncode, version=billitems.version+1, modifiedby = @modifiedby, modifiedon = @modifiedon from products where products.id = billitems.productid and billitems.billid = @billid and billitems.hsncode ='' and billitems.isactive=true;";
                var command = GetCommand(SQL, conn, transaction);
                command.Parameters.Add("billid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = bill.id;
                command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
                int successCount = command.ExecuteNonQuery();
            }

            oBundles.billid = bill.id;
            oBundles.billno = bill.billno;
            return bill;
        }

        public Bundles ImportBills(Bundles oBundles, NpgsqlConnection conn, NpgsqlTransaction transaction)
        {
            if (string.IsNullOrEmpty(oBundles.billno) ||
                oBundles.billid <= 0)
                throw new Exception("Bill no not associated against this transfer, can't import");


            var param = new SortedList<string, object>();
            CompanyDataService companyDataService = new CompanyDataService() { Context = this.Context };
            var fromcompany = companyDataService.Search(new CompanyCreteria()
            {
                id = oBundles.fromcompanyid
            }).First();
            var tocompany = companyDataService.Search(new CompanyCreteria()
            {
                id = oBundles.tocompanyid
            }).First();
            BillDataService billDataService = new BillDataService() { Context = this.Context };
            billDataService.Context = this.Context;
            var bill = billDataService.Search(new BillCreteria()
            {
                id = oBundles.billid
            }).First();

            BillinboxDataService billinboxDataService = new BillinboxDataService() { Context = this.Context };
            var billinboxlist = billinboxDataService.Search(new BillinboxCreteria()
            {
                billid = oBundles.billid
            });
            ReferencelistDataService referencelistDataService = new ReferencelistDataService() { Context = this.Context };
            var locations = referencelistDataService.Search(new ReferencelistCreteria()
            {
                ids = new List<long> { oBundles.fromlocationid, oBundles.tolocationid }
            });

            var fromlocation = locations.Find(x => x.id == oBundles.fromlocationid);
            var tolocation = locations.Find(x => x.id == oBundles.tolocationid);


            // Purhcase entry against company transfer 
            LrentryDataService lrentrydataservice = new LrentryDataService() { Context = this.Context };
            lrentrydataservice.Context = this.Context;
            Lrentry lrentry = new Lrentry();
            lrentry.createdby = Context.UserID;
            lrentry.modifiedby = Context.UserID;
            lrentry.createdon = DateTimeNow;
            lrentry.modifiedon = DateTimeNow;
            lrentry.isactive = true;
            lrentry.version = 1;
            lrentry.invoicedate = DateTimeNow;
            lrentry.invoiceno = bill.billno;
            lrentry.lrdate = billinboxlist[0].lrdate;
            lrentry.lrentrydate = DateTimeNow;
            lrentry.modeofdelivery = 2;
            lrentry.importreferencetype = Lrentry.ImportReferenceTypes.UANTRANSFER;
            lrentry.importreferenceid = oBundles.id;
            lrentry.noofpieces = (int)billinboxlist[0].noofpieces;
            lrentry.noofbundels = (int)billinboxlist[0].noofbundles;
            lrentry.lrcompanyid = tolocation.additionalinfo != null &&
                tolocation.additionalinfo.parentid > 0 ? tolocation.additionalinfo.parentid :
                oBundles.tocompanyid;

            lrentry.lrno = billinboxlist[0].lrno;
            lrentry.transportid = billinboxlist[0].transportid;
            lrentry.supplierid = fromlocation.additionalinfo != null &&
                fromlocation.additionalinfo.supplierid > 0 ? fromlocation.additionalinfo.supplierid :
                fromcompany.company_as_supplierid;

            lrentry.locationid = tolocation.id;
            lrentry.entrylocationid = tolocation.id;
            lrentry.retaillocationid = tolocation.id;

            var lrseqsequence = SequenceManager.Instance(Context.OrganizationID).Get("lrentry.lrentryno",
            InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_showroomentryno ? lrentry.entrylocationid : 0,
            InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_supportcompanywiseentryno ?
            lrentry.lrcompanyid : 0);

            lrentry.lrentryno = lrseqsequence.GetNextNumber(Context, conn, transaction);
            lrentry = lrentrydataservice.Insert(transaction, conn, lrentry);
            InboxManager.Instance(Context.OrganizationID).Update(Context, true, lrentry, transaction, conn);

            LrinvoiceDataService lrinvoiceDataService = new LrinvoiceDataService() { Context = this.Context };
            lrinvoiceDataService.Context = this.Context;
            var invoice = new Lrinvoice();
            invoice.lrid = lrentry.id;
            invoice.createdby = Context.UserID;
            invoice.modifiedby = Context.UserID;
            invoice.createdon = DateTimeNow;
            invoice.modifiedon = DateTimeNow;
            invoice.companyid = tolocation.additionalinfo != null && tolocation.additionalinfo.parentid > 0 ? tolocation.additionalinfo.parentid : oBundles.tocompanyid;
            invoice.entrydate = DateTimeNow.Date;
            invoice.invoicedate = bill.billdate;
            invoice.invoiceno = bill.billno;
            invoice.isactive = true;
            invoice.isinterstatesale = bill.isinterstatesale;
            invoice.Lrinvoiceitems = new List<LrinvoiceItemDetail>();
            invoice.Lrinvoicetaxitems = new List<Lrinvoicetax>();
            invoice.lrsupplierid = fromlocation.additionalinfo != null &&
                fromlocation.additionalinfo.supplierid > 0 ? fromlocation.additionalinfo.supplierid :
                fromcompany.company_as_supplierid;

            invoice.supplierid = fromlocation.additionalinfo != null &&
                fromlocation.additionalinfo.supplierid > 0 ? fromlocation.additionalinfo.supplierid :
                fromcompany.company_as_supplierid;

            if (invoice.supplierid > 0)
            {
                SuppliercompanyDataService suppliercompanyDataService = new SuppliercompanyDataService() { Context = this.Context };
                var suppliercompanies = suppliercompanyDataService.Search(new SuppliercompanyCreteria()
                {
                    supplierid = invoice.supplierid
                });
                if (suppliercompanies.Count == 1)
                {
                    invoice.suppliercompanyid = suppliercompanies.First().id;
                }
            }

            invoice.version = 1;
            invoice.noofpieces = (Int32)bill.Billitems.Sum(e => e.qty);
            invoice.baseamount = bill.Billitems.Sum(e => e.receivable - e.tax);
            invoice.billvalue = bill.receivable;
            invoice.taxamount = bill.tax;
            invoice.totalamount = bill.receivable;
            foreach (var taxgroup in bill.Billitems.GroupBy(e => new { e.taxid, e.taxpercentage }))
            {
                invoice.Lrinvoicetaxitems.Add(new Lrinvoicetax()
                {
                    isactive = true,
                    isinterstatesale = bill.isinterstatesale,
                    createdby = Context.UserID,
                    modifiedby = Context.UserID,
                    createdon = DateTimeNow,
                    modifiedon = DateTimeNow,
                    amounton = taxgroup.Sum(e => e.receivable - e.tax),
                    taxamount = taxgroup.Sum(e => e.tax),
                    taxid = taxgroup.Key.taxid,
                    percentage = taxgroup.Key.taxpercentage,
                    version = 1
                });
            }

            invoice = lrinvoiceDataService.Insert(transaction, conn, invoice);
            LrinvoicetaxDataService lrinvoicetax = new LrinvoicetaxDataService() { Context = this.Context };
            lrinvoicetax.Context = this.Context;
            foreach (var taxitem in invoice.Lrinvoicetaxitems)
            {
                taxitem.invoiceid = invoice.id;
                taxitem.netamount = taxitem.amounton + taxitem.taxamount;
                lrinvoicetax.Insert(transaction, conn, taxitem);
            }

            LrinvoiceitemsDataService subLrinvoiceitems = new LrinvoiceitemsDataService() { Context = this.Context };
            subLrinvoiceitems.Context = this.Context;
           
            
            
            foreach (var bitem in bill.Billitems)
            {
                LrinvoiceItemDetail lrinvoiceItemDetail = new LrinvoiceItemDetail();
                lrinvoiceItemDetail.actualqty = bitem.qty;
                //var list = new List<string>();
                //list.Add(bitem.barcode);
                lrinvoiceItemDetail.lrinvoiceid = invoice.id;
                lrinvoiceItemDetail.createdby = Context.UserID;
                lrinvoiceItemDetail.modifiedby = Context.UserID;
                lrinvoiceItemDetail.createdon = DateTimeNow;
                if(oBundles.items.Exists(x => x.barcode == bitem.barcode))
                    lrinvoiceItemDetail.itemid = oBundles.items.Find(x => x.barcode == bitem.barcode).itemid;
                else if (oBundles.items.Exists(x => bitem.attributes != null &&  x.serialno == bitem.attributes.serialno))
                    lrinvoiceItemDetail.itemid = oBundles.items.Find(x => bitem.attributes != null &&  x.serialno == bitem.attributes.serialno).itemid;
                lrinvoiceItemDetail.modifiedon = DateTimeNow;
                //lrinvoiceItemDetail.barcodes = Newtonsoft.Json.JsonConvert.SerializeObject(list);
                lrinvoiceItemDetail.designid = bitem.attributes.designid;
                lrinvoiceItemDetail.Details = new Items();
                if (bitem.attributes != null)
                {
                    lrinvoiceItemDetail.designid = bitem.attributes.designid;
                    lrinvoiceItemDetail.uan = bitem.attributes.serialno;
                    lrinvoiceItemDetail.Details.serialno = bitem.attributes.serialno;
                }
                lrinvoiceItemDetail.basebuyingprice = bitem.rate;
                lrinvoiceItemDetail.buyingprice = bitem.rate;
                lrinvoiceItemDetail.buyingqty = bitem.qty;
                lrinvoiceItemDetail.amount = bitem.qty * bitem.rate;
                if (oBundles.items.Exists(x => x.barcode == bitem.barcode && x.journalqty != 0))
                {
                    lrinvoiceItemDetail.returnqty = oBundles.items.Find(x => x.barcode == bitem.barcode && x.journalqty != 0).journalqty;
                }
                lrinvoiceItemDetail.hsncode = bitem.hsncode;
                lrinvoiceItemDetail.mrp = bitem.mrpdisplay;
                lrinvoiceItemDetail.sellingprice = bitem.mrpdisplay;
                lrinvoiceItemDetail.productid = bitem.productid;
                lrinvoiceItemDetail.ptaxid = bitem.taxid;
                lrinvoiceItemDetail.purchasetax = bitem.tax;
                lrinvoiceItemDetail.purchasetaxpercentage = bitem.taxpercentage;
                lrinvoiceItemDetail.taxid = bitem.taxid;
                lrinvoiceItemDetail.version = 1;
                lrinvoiceItemDetail.isactive = true;
                subLrinvoiceitems.Insert(transaction, conn, lrinvoiceItemDetail);
                invoice.Lrinvoiceitems.Add(lrinvoiceItemDetail);
            }

            lrinvoiceDataService.AutoBarcodeCreation(conn, transaction, invoice, true);
            var returnitems = invoice.Lrinvoiceitems.FindAll(x => x.returnqty != 0);
            if (returnitems.Count > 0)
            {
                LrinvoicereturnDataService lrinvoicereturnDataService = new LrinvoicereturnDataService() { Context = this.Context };
                Lrinvoicereturn lrinvoicereturn = new Lrinvoicereturn();
                lrinvoicereturn.supplierid = invoice.supplierid;
                lrinvoicereturn.suppliercompanyid = invoice.suppliercompanyid;
                lrinvoicereturn.returndate = invoice.entrydate;
                lrinvoicereturn.companyid = invoice.companyid;
                lrinvoicereturn.ReturnType = Lrinvoicereturn.ReturnTypes.DebitNote;
                SortedDictionary<long, Tuple<long, long>> salablegoodsid = new SortedDictionary<long, Tuple<long, long>>();

                string query = @"select lrinvoiceitems.id,stock.barcode,stockid,salablegoods.id salablegoodsid from lrinvoice
                inner join lrinvoiceitems on lrinvoiceitems.lrinvoiceid = lrinvoice.id
                inner join stock on stock.invoiceitemid = lrinvoiceitems.id
                inner join salablegoods on salablegoods.stockid = stock.id
                and salablegoods.locationid = @locationid
                where lrinvoice.id = @invoiceid";
                param = new SortedList<string, object>();
                param.Add("locationid", oBundles.tolocationid);
                param.Add("invoiceid", invoice.id);
                using (var reader = ExecuteReader(query, param, conn, transaction))
                {
                    while (reader.Read())
                    {
                        long id = Convert.ToInt64(reader["id"]);
                        long stockid = Convert.ToInt64(reader["id"]);
                        long salableid = Convert.ToInt64(reader["salablegoodsid"]);
                        string barcode = Convert.ToString(reader["barcode"]);
                        invoice.Lrinvoiceitems.Find(x => x.id == id).barcode = barcode;
                        salablegoodsid.Add(id, new Tuple<long, long>(stockid, salableid));
                    }
                }

                foreach (var ivitem in returnitems)
                {
                    Lrinvoicereturnitems lrinvoicereturnitems = new Lrinvoicereturnitems();
                    lrinvoicereturnitems.createdby = Context.UserID;
                    lrinvoicereturnitems.createdon = DateTimeNow;
                    lrinvoicereturnitems.designid = ivitem.designid;
                    lrinvoicereturnitems.hsncode = ivitem.hsncode;
                    lrinvoicereturnitems.uan = ivitem.uan;
                    lrinvoicereturnitems.invoicedate = invoice.invoicedate;
                    lrinvoicereturnitems.invoiceno = invoice.invoiceno;
                    lrinvoicereturnitems.isactive = true;
                    lrinvoicereturnitems.itemid = ivitem.itemid;
                    lrinvoicereturnitems.productid = ivitem.productid;
                    lrinvoicereturnitems.taxid = ivitem.taxid;
                    lrinvoicereturnitems.taxpercentage = ivitem.purchasetaxpercentage;
                    lrinvoicereturnitems.version = 1;
                    lrinvoicereturnitems.lrinvoiceitemsid = ivitem.id;
                    lrinvoicereturnitems.barcode = ivitem.barcode;
                    lrinvoicereturnitems.rate = ivitem.buyingprice;
                    lrinvoicereturnitems.qty = ivitem.returnqty;
                    lrinvoicereturnitems.stockid = salablegoodsid[ivitem.id].Item1;
                    lrinvoicereturnitems.salablegoodsid = salablegoodsid[ivitem.id].Item2;
                    if (lrinvoicereturnitems.taxpercentage > 0)
                    {
                        lrinvoicereturnitems.tax = ivitem.purchasetax / ivitem.buyingqty * ivitem.returnqty;
                    }
                    lrinvoicereturnitems.amount = lrinvoicereturnitems.qty * lrinvoicereturnitems.rate;
                }
                lrinvoicereturnDataService.InsertReturn(transaction, conn, lrinvoicereturn);
                StocktransactionDataService stocktransactionDataService = new StocktransactionDataService() { Context = this.Context };
                stocktransactionDataService.PurchaseReturn(lrinvoicereturn, transaction, conn);
            }


            param = new SortedList<string, object>();
            param.Add("id", oBundles.id);
            param.Add("lrentryno", lrentry.lrentryno);
            param.Add("invoiceid", invoice.id);
            ExecuteQuery("update bundles set lrentryno=@lrentryno,invoiceid=@invoiceid where id=@id", param, conn, transaction);
            ExecuteQuery("update lrinvoiceitems set actualqty=buyingqty-returnqty where lrinvoiceid=@invoiceid", param, conn, transaction);
            oBundles.invoiceid = invoice.id;
            oBundles.lrentryno = lrentry.lrentryno;
            lrentry.Status = Inbox.StatusList.Completed;
            InboxManager.Instance(Context.OrganizationID).Update(Context, false, lrentry, transaction, conn);
            return oBundles;
        }


        public Bundles Update(Bundles oBundles)
        {
            throw new Exception("Bundle Updation restricted. please recreate new bundle for stock transfer.");

            NpgsqlTransaction transaction = null;
            try
            {
                //oBundles.ToJSON();
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    if (!Update(transaction, conn, oBundles, 0))
                    {
                        transaction.Rollback();
                        oBundles.errormessage = "Already got inwarded";
                        return oBundles;
                    }

                    NpgsqlCommand command = GetCommand(SQL_UpdateItems, conn, transaction);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBundles.id;
                    command.Parameters.Add("items", NpgsqlTypes.NpgsqlDbType.Json).Value = oBundles.items_json;
                    command.ExecuteNonQuery();

                    SalablegoodsDataService subSalablegoods = new SalablegoodsDataService() { Context = this.Context };
                    subSalablegoods.Context = this.Context;
                    if (!isgoods)
                    {
                        subSalablegoods.Update(transaction, conn, oBundles.Salablegoods, oBundles.id);
                    }
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oBundles);
            }
            return oBundles;
        }

        public bool CanUpdate(NpgsqlTransaction transaction, NpgsqlConnection conn, Bundles oBundles, bool returnitems)
        {
            //NpgsqlCommand command = GetCommand("Update Bundles set version=version+1 where id=@id and isactive = true and version=@version RETURNING version" + (returnitems ? ",items" : ""), conn, transaction);
            NpgsqlCommand command = GetCommand("Select version" + (returnitems ? ",items" : "") + " from Bundles where id=@id and isactive = true and version=@version ", conn, transaction);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBundles.id;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBundles.version;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oBundles.version = Convert.ToInt32(reader[0]);
                    if (returnitems)
                        oBundles.items_json = reader.GetString(1);
                    return true;
                }
            }
            return false;
        }
        public bool Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Bundles oBundles, int status)
        {
            if (string.IsNullOrEmpty(oBundles.code))
                throw new Exception("Empty bundle code not allowed");

            if (Services.ConfigManager.Instance(Context.OrganizationID).Config.system_interstatetransfer
            && (oBundles.fromcompanyid == 0 || oBundles.tocompanyid == 0))
            {
                oBundles.errormessage = "Inter company transfer details missing.";
                throw new Exception("Inter company transfer details missing.");
            }

            NpgsqlCommand command = GetCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBundles.id;
            command.Parameters.Add("scanmode", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBundles.scanmode;
            command.Parameters.Add("remarks", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBundles.remarks == null ? "" : oBundles.remarks;
            command.Parameters.Add("code", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBundles.code == null ? "" : oBundles.code;
            command.Parameters.Add("fromlocationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBundles.fromlocationid;
            command.Parameters.Add("tolocationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBundles.tolocationid;
            command.Parameters.Add("packedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBundles.packedby;
            command.Parameters.Add("pricetype", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBundles.pricetype;
            command.Parameters.Add("receivedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBundles.receivedby;
            command.Parameters.Add("inward2warehouse", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBundles.inward2warehouse;
            command.Parameters.Add("requesteddate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBundles.requesteddate.Date;
            command.Parameters.Add("packedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBundles.packedon;
            command.Parameters.Add("receivedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBundles.receivedon;
            command.Parameters.Add("qty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBundles.qty;
            command.Parameters.Add("notes", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBundles.notes == null ? "" : oBundles.notes;
            command.Parameters.Add("approvedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBundles.approvedby;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBundles.isactive;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("status", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBundles.status;
            command.Parameters.Add("currentstatus", NpgsqlTypes.NpgsqlDbType.Integer).Value = status;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBundles.version;
            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBundles.companyid;
            command.Parameters.Add("tocompanyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBundles.tocompanyid;
            command.Parameters.Add("fromcompanyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBundles.fromcompanyid;
            command.Parameters.Add("invoiceid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBundles.invoiceid;
            command.Parameters.Add("billid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBundles.billid;
            command.Parameters.Add("iscompanytransfer", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBundles.iscompanytransfer;

            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oBundles.version = Convert.ToInt32(reader[0]);
                    return true;
                }
            }

            return false;
        }

        internal bool RejectGoods(Bundles oBundles)
        {
            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    if (!CanUpdate(transaction, conn, oBundles, true))
                        throw new Exception("Given package updated, please refresh and try again!");

                    NpgsqlCommand command = GetCommand(SQL_RejectSalableGoods, conn, transaction);
                    command.Parameters.Add("bundleid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBundles.id;
                    List<Salablegoods> salablegoods = new List<Salablegoods>();
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Salablegoods salablegood = new Salablegoods();
                            salablegood.id = reader.GetInt64(0);
                            salablegood.dqty = reader.GetDecimal(1);
                            salablegood.iqty = reader.GetDecimal(2);
                            salablegood.oqty = reader.GetDecimal(3) + reader.GetDecimal(4);
                            salablegoods.Add(salablegood);
                        }
                    }
                    if (salablegoods.Count == 0)
                    {
                        hasSuccessCount = Delete(transaction, conn, oBundles);
                        transaction.Commit();
                        conn.Close();
                        return true;
                    }
                    else
                    {
                        if (salablegoods.Exists(e => e.dqty + e.jqty > e.iqty - e.oqty))
                        {
                            throw new Exception("Some of the barcode already sold, can't reject the package");
                        }

                        command = GetCommand(SQL_RejectGoods, conn, transaction);
                        command.Parameters.Add("bundleid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBundles.id;
                        int successCount = command.ExecuteNonQuery();
                        oBundles.status = 9;
                        oBundles.isactive = false;
                        //hasSuccessCount = Delete(transaction, conn, oBundles);

                        command = GetCommand("Update salablegoods set iqty=iqty-@dqty where id=@id and (oqty+jqty+dqty)=@oqty and iqty=@iqty RETURNING iqty;", conn, transaction);
                        command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint);
                        command.Parameters.Add("iqty", NpgsqlTypes.NpgsqlDbType.Numeric);
                        command.Parameters.Add("dqty", NpgsqlTypes.NpgsqlDbType.Numeric);
                        command.Parameters.Add("oqty", NpgsqlTypes.NpgsqlDbType.Numeric);
                        foreach (var item in salablegoods)
                        {
                            item.isactive = true;
                            command.Parameters["id"].Value = item.id;
                            command.Parameters["iqty"].Value = item.iqty;
                            command.Parameters["dqty"].Value = item.dqty;
                            command.Parameters["oqty"].Value = item.oqty;
                            using (var reader = command.ExecuteReader())
                            {
                                if (!reader.Read())
                                //{
                                //    if (reader.GetDecimal(0) == 0)
                                //        item.isactive = false;
                                //}
                                //else
                                {
                                    throw new Exception("Some of the barcode already sold, can't reject the package");
                                }
                            }
                        }

                        //List<long> deletable = salablegoods.FindAll(e => e.isactive == false).ConvertAll(e => e.id);
                        //if (deletable.Count > 0)
                        //{
                        //    command = GetCommand("delete from salablegoods where oqty=0 and id in (" + string.Join(",", deletable) + ");", conn, transaction);
                        //    command.ExecuteNonQuery();
                        //}

                        if (!Update(transaction, conn, oBundles, oBundles.status))
                        {
                            transaction.Rollback();
                            oBundles.errormessage = "Already got inwarded";
                            return false;
                        }
                        else
                        {
                            Services.IntrayManager.Instance(Context.OrganizationID).Add(Context.OrganizationID, Services.IntrayManager.EntryType.Inward,
                                    oBundles.id, string.Format("REJECTED BUNDLE INWARD REFERENCE NO {0} ",
                                    oBundles.code), Context.UserID, oBundles.tolocationid, transaction, Context);
                        }

                    }
                    StocktransactionDataService stocktransaction = new StocktransactionDataService() { Context = this.Context };
                    stocktransaction.RejectGoods(oBundles, transaction, conn);
                    transaction.Commit();
                    conn.Close();
                    return true;
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oBundles);
            }
            return hasSuccessCount;
        }
        public bool Reject(Bundles oBundles)
        {
            int currentstatus = oBundles.status;
            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    if (!CanUpdate(transaction, conn, oBundles, true))
                        throw new Exception("Given package updated, please refresh and try again!");

                    NpgsqlCommand command = GetCommand(SQL_RejectSalableGoods, conn, transaction);
                    command.Parameters.Add("bundleid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBundles.id;
                    List<Salablegoods> salablegoods = new List<Salablegoods>();
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Salablegoods salablegood = new Salablegoods();
                            salablegood.id = reader.GetInt64(0);
                            salablegood.dqty = reader.GetDecimal(1);
                            salablegood.iqty = reader.GetDecimal(2);
                            salablegood.oqty = reader.GetDecimal(3) + reader.GetDecimal(4);
                            salablegoods.Add(salablegood);
                        }
                    }
                    if (salablegoods.Count == 0)
                    {
                        oBundles.status = 9;
                        oBundles.isactive = true;
                        hasSuccessCount = UpdateStatus(transaction, conn, oBundles);
                    }
                    else
                    {
                        if (salablegoods.Exists(e => e.dqty > e.iqty - e.oqty - e.jqty))
                        {
                            throw new Exception("Some of the barcode already sold, can't reject the package");
                        }

                        command = GetCommand(SQL_Reject, conn, transaction);
                        command.Parameters.Add("bundleid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBundles.id;
                        int successCount = command.ExecuteNonQuery();
                        oBundles.status = 9;
                        oBundles.isactive = false;
                        //hasSuccessCount = Delete(transaction, conn, oBundles);

                        command = GetCommand("Update salablegoods set iqty=iqty-@dqty where id=@id and (oqty+jqty+dqty)=@oqty and iqty=@iqty RETURNING iqty;", conn, transaction);
                        command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint);
                        command.Parameters.Add("iqty", NpgsqlTypes.NpgsqlDbType.Numeric);
                        command.Parameters.Add("dqty", NpgsqlTypes.NpgsqlDbType.Numeric);
                        command.Parameters.Add("oqty", NpgsqlTypes.NpgsqlDbType.Numeric);
                        foreach (var item in salablegoods)
                        {
                            item.isactive = true;
                            command.Parameters["id"].Value = item.id;
                            command.Parameters["iqty"].Value = item.iqty;
                            command.Parameters["dqty"].Value = item.dqty;
                            command.Parameters["oqty"].Value = item.oqty;
                            using (var reader = command.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    if (reader.GetDecimal(0) == 0)
                                        item.isactive = false;
                                }
                                else
                                {
                                    throw new Exception("Some of the barcode already sold, can't reject the package");
                                }
                            }
                        }

                        List<long> deletable = salablegoods.FindAll(e => e.isactive == false).ConvertAll(e => e.id);
                        if (deletable.Count > 0)
                        {
                            command = GetCommand("delete from salablegoods where oqty=0 and id in (" + string.Join(",", deletable) + ");", conn, transaction);
                            command.ExecuteNonQuery();
                        }
                        if (!Update(transaction, conn, oBundles, currentstatus))
                        {
                            transaction.Rollback();
                            oBundles.errormessage = "Already got inwarded";
                            return false;
                        }
                        else
                        {
                            StocktransactionDataService stocktransaction = new StocktransactionDataService() { Context = this.Context };
                            stocktransaction.RejectGoods(oBundles, transaction, conn);
                            transaction.Commit();
                            Services.IntrayManager.Instance(Context.OrganizationID).Add(Context.OrganizationID, Services.IntrayManager.EntryType.Inward,
                                    oBundles.id, string.Format("REJECTED BUNDLE INWARD REFERENCE NO {0} ",
                                    oBundles.code), Context.UserID, oBundles.tolocationid, transaction, Context);
                        }
                    }

                    transaction.Commit();
                    conn.Close();
                    return true;
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oBundles);
            }
            return hasSuccessCount;

        }


        public bool Delete(Bundles oBundles)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    //if (!isgoods)
                    //{
                    //    NpgsqlCommand command = GetCommand(SQL_Reject, conn, transaction);
                    //    command.Parameters.Add("bundleid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBundles.id;
                    //    int successCount = command.ExecuteNonQuery();
                    //    hasSuccessCount = Delete(transaction, conn, oBundles);
                    //}
                    //else
                    //{
                    hasSuccessCount = Delete(transaction, conn, oBundles);
                    //}

                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oBundles);

            }
            return hasSuccessCount;

        }

        public bool UpdateStatus(NpgsqlTransaction transaction, NpgsqlConnection conn, Bundles oBundles)
        {
            if (oBundles.items != null && oBundles.items.Exists(e => e.itemid > 0 && e.aqty > 0))
            {
                throw new Exception("Given package already transacted, can't delete");
            }

            NpgsqlCommand command = GetCommand(SQL_UpdateStatus, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBundles.isactive;
            command.Parameters.Add("status", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBundles.status;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("returngoods", NpgsqlTypes.NpgsqlDbType.Boolean).Value = isgoods;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBundles.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBundles.id;
            command.Parameters.Add("receivedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBundles.receivedby;
            command.Parameters.Add("receivedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBundles.receivedon;

            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Bundles oBundles)
        {
            if (!CanUpdate(transaction, conn, oBundles, true) || (oBundles.items != null && oBundles.items.Exists(e => e.itemid > 0 && e.aqty > 0)))
            {
                throw new Exception("Given package already transacted, can't delete");
            }

            NpgsqlCommand command = GetCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBundles.isactive;
            command.Parameters.Add("status", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBundles.status;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("returngoods", NpgsqlTypes.NpgsqlDbType.Boolean).Value = isgoods;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBundles.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBundles.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }
        public System.Data.DataTable ExportBundleDetail(long id)
        {
            string query = @"Select supplier.name Suppliername, products.name Product, cast(itemlist->> 'barcode' as character varying(20)) barcode, 
        cast(itemlist->> 'iqty' as numeric) qty, brand.name brand, stock.designid design, stock.hsncode hsncode, 
		lrinvoice.invoiceno, lrinvoice.invoicedate, size.name size, stock.buyingprice Buyingprice, 
		stock.price price, stock.discount discount, stock.isbit as bit
        from bundles 
        INNER JOIN LATERAL json_array_elements (case when bundles.items is null or bundles.items::text ='null' then '[]'::json else bundles.items :: json end) itemlist ON true
        inner join stock on stock.id = cast(itemlist->> 'stockid' as bigint) and stock.isactive = true
        left outer join lrinvoiceitems on lrinvoiceitems.id = invoiceitemid
        left outer join lrinvoice on lrinvoice.id = lrinvoiceitems.lrinvoiceid
        left outer join supplier on supplier.id = stock.supplierid
        left outer join items on stock.itemid=items.id and stock.isactive=true
        left outer join products on products.id = stock.productid
        left outer join brand brand on brand.id = items.brandid
        left outer join referencelist size on size.id = sizeid
        where bundles.isactive=true  and bundles.id=@id 
        order by product desc";

            System.Data.DataSet dset = null;
            NpgsqlConnection conn = CreateConnection();
            try
            {
                var param = new SortedList<string, object>();
                param.Add("id", id);
                conn.Open();
                dset = ExecuteDataSet(query, param);
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);
                throw exp;
            }
            return dset != null ? dset.Tables[0] : null;
        }

        internal List<BundleItemPrife> FindPrice(BundlesCreteria item)
        {
            List<BundleItemPrife> response = new List<BundleItemPrife>();
            if (Services.ConfigManager.Instance(Context.OrganizationID).Config.system_invcol_pricetype)
            {
                string currentpricequery = @"select stock.id,fromsale.price,itemdetail->>'pricelist' from bundles
                                        LEFT JOIN LATERAL
                                        json_array_elements (case when bundles.items is null or bundles.items::text ='null' then '[]'::json else bundles.items :: json end) schemelist ON true
                                        inner join stock ON (schemelist ->> 'stockid') :: BIGINT = stock.id
                                        inner join lrinvoiceitems on lrinvoiceitems.id = invoiceitemid
									    left outer join salablegoods fromsale ON  fromsale.stockid = stock.id and fromsale.locationid=@tolocationid
                                        where bundles.id = @id;";

                var param = new SortedList<string, object>();
                param.Add("id", item.id);
                param.Add("tolocationid", item.tolocationid);
                using (var reader = ExecuteReader(currentpricequery, param))
                {
                    while (reader.Read())
                    {
                        var id = reader.GetInt64(0);
                        var fprice = reader.IsDBNull(1) ? 0 : reader.GetDecimal(1);
                        bool exist = !reader.IsDBNull(1);
                        if (reader.IsDBNull(1) && !reader.IsDBNull(2))
                        {
                            var pricelist = Items.CreatePriceList(reader.GetString(2));
                            if (pricelist.Exists(x => x.id == item.pricetype))
                            {
                                fprice = pricelist.Find(x => x.id == item.pricetype).price;
                            }
                        }
                        response.Add(new BundleItemPrife()
                        {
                            id = id,
                            exist = exist,
                            price = fprice
                        });
                    }
                }
            }
            return response;
        }

        internal BundleTransport GetTransport(long bundleid)
        {
            BundleTransport oBundles = new BundleTransport();
            oBundles.id = bundleid;
            var param = new SortedList<string, object>();
            param.Add("id", bundleid);

            using (var reader = ExecuteReader("Select vehicleno,packedby,packedon,transportid,ewb,modeofdelivery,ewayjson from bundles where id=@id", param))
            {
                if (reader.Read())
                {
                    oBundles.transportid = reader["transportid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["transportid"]);
                    oBundles.modeofdelivery = reader["modeofdelivery"] == DBNull.Value ? 0 : Convert.ToInt32(reader["modeofdelivery"]);
                    oBundles.vehicleno = reader["vehicleno"] == DBNull.Value ? "" : reader["vehicleno"].ToString();
                    oBundles.ewb = reader["ewb"] == DBNull.Value ? "" : reader["ewb"].ToString();
                    oBundles.eway_json = reader["ewayjson"] == DBNull.Value ? "" : reader["ewayjson"].ToString();
                    oBundles.packedby = reader["packedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["packedby"]);
                    oBundles.packedon = reader["packedon"] == DBNull.Value ? DateTimeNow : Convert.ToDateTime(reader["packedon"]);
                }
            }

            if (oBundles.eway != null)
            {
                oBundles.lrdate = oBundles.eway.lrdate;
                oBundles.lrno = oBundles.eway.lrno;
                oBundles.tocityid = oBundles.eway.tocityid;
                oBundles.noofpieces = oBundles.eway.noofpieces;
                oBundles.noofbundles = oBundles.eway.noofbundles;
                oBundles.distance = oBundles.eway.distance;
                oBundles.billimg = oBundles.eway.billimg;
            }
            return oBundles;
        }

        internal BundleTransport SetTransport(BundleTransport oBundles)
        {
            var bundle = Search(new BundlesCreteria()
            {
                id = oBundles.id
            }).First();
            bool updated = false;
            if (oBundles.geneway)
            {
                if (string.IsNullOrEmpty(oBundles.ewb))
                {
                    Gst_InboxDataService gst_InboxDataService = new Gst_InboxDataService() { Context = this.Context };
                    var eway = gst_InboxDataService.CreateEWay4Transfer(new GST_Transfer()
                    {
                        bundleid = oBundles.id,
                        companyid = bundle.fromcompanyid,
                        transportid = oBundles.transportid,
                        vehcicleno = oBundles.vehicleno,
                        modeofdelivery = oBundles.modeofdelivery,
                        subSupplyDesc = oBundles.subSupplyDesc,
                        subSupplyType = oBundles.subSupplyType
                    });


                    if (eway != null &&
                        eway.ewayBillNo > 0)
                    {
                        oBundles.eway_json = eway.raw;
                        oBundles.ewb = eway.ewayBillNo.ToString();
                        oBundles.eway.lrdate = oBundles.lrdate;
                        oBundles.eway.lrno = oBundles.lrno;
                        oBundles.eway.tocityid = oBundles.tocityid;
                        oBundles.eway.noofpieces = oBundles.noofpieces;
                        oBundles.eway.noofbundles = oBundles.noofbundles;
                        oBundles.eway.distance = oBundles.distance;
                        oBundles.eway.billimg = oBundles.billimg;
                        string query = @"update bundles set vehicleno=@vehicleno,transportid=@transportid,
                                         ewb=@ewb,ewayjson=@ewayjson,modeofdelivery=@modeofdelivery,
                                         packedby=@packedby,packedon=@packedon where id=@id";
                        using (NpgsqlConnection conn = CreateConnection())
                        {
                            conn.Open();
                            NpgsqlCommand command = GetCommand(query, conn);
                            command.Parameters.Add("vehicleno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBundles.vehicleno;
                            command.Parameters.Add("transportid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBundles.transportid;
                            command.Parameters.Add("modeofdelivery", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBundles.modeofdelivery;
                            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
                            command.Parameters.Add("ewb", NpgsqlTypes.NpgsqlDbType.Varchar).Value = eway.ewayBillNo.ToString();
                            command.Parameters.Add("packedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBundles.packedby;
                            command.Parameters.Add("packedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBundles.packedon;
                            command.Parameters.Add("ewayjson", NpgsqlTypes.NpgsqlDbType.Json).Value = oBundles.eway_json;
                            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBundles.id;
                            int successCount = command.ExecuteNonQuery();
                            conn.Close();
                        }
                        updated = true;
                    }
                    else
                    {
                        throw new Exception(string.Join("<br>", eway.ErrorDetails.ConvertAll(x => x.ErrorMessage)));
                    }
                }
            }

            if (!updated)
            {
                oBundles.eway = bundle.eway;
                if (oBundles.eway == null)
                {
                    oBundles.eway = new Bundles.EwayInfo();
                }
                oBundles.ewb = oBundles.ewb;
                oBundles.eway.lrdate = oBundles.lrdate;
                oBundles.eway.lrno = oBundles.lrno;
                oBundles.eway.tocityid = oBundles.tocityid;
                oBundles.eway.noofpieces = oBundles.noofpieces;
                oBundles.eway.noofbundles = oBundles.noofbundles;
                oBundles.eway.distance = oBundles.distance;
                oBundles.eway.billimg = oBundles.billimg;
                string query = @"update bundles set vehicleno=@vehicleno,transportid=@transportid,ewb=@ewb,ewayjson=@ewayjson,
                                 packedby=@packedby,packedon=@packedon,modeofdelivery=@modeofdelivery where id=@id";

                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    NpgsqlCommand command = GetCommand(query, conn);
                    command.Parameters.Add("vehicleno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBundles.vehicleno;
                    command.Parameters.Add("transportid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBundles.transportid;
                    command.Parameters.Add("modeofdelivery", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBundles.modeofdelivery;
                    command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                    command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
                    command.Parameters.Add("ewb", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBundles.ewb;
                    command.Parameters.Add("ewayjson", NpgsqlTypes.NpgsqlDbType.Json).Value = oBundles.eway_json;
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBundles.id;
                    command.Parameters.Add("packedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBundles.packedon;
                    command.Parameters.Add("packedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBundles.packedby;
                    int successCount = command.ExecuteNonQuery();
                    conn.Close();
                }
            }
            return oBundles;
        }

        internal Bundles Merge(List<long> ids)
        {
            Bundles oBundles = null;
            NpgsqlTransaction transaction = null;
            try
            {
                var mergingids = Search(new BundlesCreteria()
                {
                    ids = ids
                });
                oBundles = JsonConvert.DeserializeObject<Bundles>(mergingids[0].ToJSON());
                oBundles.items = new List<Bundleitem>();
                mergingids.ForEach(x =>
                {
                    if (x.items != null && x.items.Count > 0)
                    {
                        oBundles.items.AddRange(x.items);
                    }
                });

                var sequence = SequenceManager.Instance(Context.OrganizationID).Get("bundles.code",
                    InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_supportlocationwisebundleno ? mergingids[0].fromlocationid : 0);
                //oBundles.ToJSON();
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);
                    if (Services.ConfigManager.Instance(Context.OrganizationID).Config.system_randombundlecode)
                    {
                        InfyPOS.Processors.RandomManager randomManager = new Processors.RandomManager(6, Processors.RandomManager.Type.AlphaNumeric, "code", "bundles");
                        oBundles.code = randomManager.Generate();
                    }
                    else
                    {
                        oBundles.code = sequence.GetNextNumber(Context, conn, transaction);
                    }
                    oBundles.qty = oBundles.items.Sum(e => e.ismeter ? 1 : e.iqty);
                    Insert(transaction, conn, oBundles);

                    if (Services.ConfigManager.Instance(Context.OrganizationID).Config.intray_notification_inward)
                    {
                        Services.IntrayManager.Instance(Context.OrganizationID).Add(Context.OrganizationID, Services.IntrayManager.EntryType.Inward,
                                oBundles.id, string.Format("NEW BUNDLE CREATED REFERENCE NO {0} ", oBundles.code),
                                Context.UserID, oBundles.tolocationid, transaction, Context);
                    }

                    if (oBundles.status == 2)
                    {
                        Services.IntrayManager.Instance(Context.OrganizationID).Add(Context.OrganizationID, Services.IntrayManager.EntryType.Inward,
                                oBundles.id, string.Format("PARTIAL BUNDLE INWARD REFERENCE NO {0} ", oBundles.code),
                                Context.UserID, oBundles.tolocationid, transaction, Context);
                    }
                    command = GetCommand(SQL_UpdateItems, conn, transaction);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBundles.id;
                    command.Parameters.Add("items", NpgsqlTypes.NpgsqlDbType.Json).Value = oBundles.items_json;
                    command.ExecuteNonQuery();

                    command = GetCommand("Update bundles set status=9,remarks=@remarks,modifiedon=@modifiedon,modifiedby=@modifiedby where id in (" + string.Join(",", ids) + ")", conn, transaction);
                    command.Parameters.Add("remarks", NpgsqlTypes.NpgsqlDbType.Varchar).Value = "Merged Bundles " + oBundles.code;
                    command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
                    command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                    command.ExecuteNonQuery();

                    transaction.Commit();
                    conn.Close();
                }
                oBundles = Search(new BundlesCreteria()
                {
                    id = oBundles.id
                }).First();
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oBundles);
            }
            return oBundles;
        }

        internal List<StockOrderHistory> SaveUANSTO(STOUANUploaded pouploaditem)
        {
            List<StockOrderHistory> history = new List<StockOrderHistory>();
            NpgsqlTransaction transaction = null;
            try
            {
                CompanyDataService companyDataService = new CompanyDataService() { Context = this.Context };
                var allcompany = companyDataService.Search(new CompanyCreteria()
                {
                });
                ReferencelistDataService referencelistDataService = new ReferencelistDataService() { Context = this.Context };
                var alllocations = referencelistDataService.Search(new ReferencelistCreteria()
                {
                    type = "LOCATION"
                });

                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    foreach (var orderitem in pouploaditem.orders)
                    {
                        var company = allcompany.Find(x => x.id == orderitem.fromcompanyid);
                        Bundles oBundles = new Bundles();
                        oBundles.companyid = orderitem.fromcompanyid;
                        oBundles.requesteddate = orderitem.STODate;
                        oBundles.packedon = orderitem.STODate;
                        oBundles.remarks = orderitem.Remark;
                        oBundles.requestcode = orderitem.OrderSplit;
                        oBundles.version = 1;
                        oBundles.status = -1;
                        oBundles.isactive = true;
                        oBundles.scanmode = true;
                        oBundles.fromlocationid = orderitem.fromlocationid;
                        oBundles.fromcompanyid = orderitem.fromcompanyid;
                        oBundles.tolocationid = orderitem.tolocationid;
                        oBundles.tocompanyid = orderitem.tocompanyid;
                        oBundles.notes = orderitem.Remark;
                        oBundles.items = new List<Bundleitem>();

                        var fromlocation = alllocations.Find(x => x.id == oBundles.fromlocationid);
                        var tolocation = alllocations.Find(x => x.id == oBundles.tolocationid);

                        oBundles.inward2warehouse = tolocation.additionalinfo != null && tolocation.additionalinfo.iswarehouse;
                        oBundles.iscompanytransfer = tolocation.additionalinfo.parentid != fromlocation.additionalinfo.parentid;
                        oBundles.returngoods = !tolocation.additionalinfo.iswarehouse;

                        foreach (var oitem in orderitem.items)
                        {
                            Bundleitem oPurchaseorderitemsDetail = new Bundleitem();
                            oPurchaseorderitemsDetail.serialno = oitem.UAN;
                            oPurchaseorderitemsDetail.qty = oitem.Qty;
                            oPurchaseorderitemsDetail.iqty = oitem.Qty;
                            oPurchaseorderitemsDetail.sno = oBundles.items.Count() + 1;
                            oPurchaseorderitemsDetail.sizename = oitem.Size;
                            oPurchaseorderitemsDetail.mrp = oitem.MRP;
                            oPurchaseorderitemsDetail.price = oitem.Cost;
                            oPurchaseorderitemsDetail.buyingprice = oitem.Cost;
                            oPurchaseorderitemsDetail.printingname = oitem.Product;
                            oPurchaseorderitemsDetail.productid = oitem.ProductID;
                            oPurchaseorderitemsDetail.itemid = oitem.ItemID;
                            oPurchaseorderitemsDetail.designid = oitem.Design;
                            oBundles.items.Add(oPurchaseorderitemsDetail);
                        }
                        var sequence = SequenceManager.Instance(Context.OrganizationID).Get("bundles.code", InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_supportlocationwisebundleno ? oBundles.fromlocationid : 0);
                        if (Services.ConfigManager.Instance(Context.OrganizationID).Config.system_randombundlecode)
                        {
                            InfyPOS.Processors.RandomManager randomManager = new Processors.RandomManager(6, Processors.RandomManager.Type.AlphaNumeric, "code", "bundles");
                            oBundles.code = randomManager.Generate();
                        }
                        else
                        {
                            oBundles.code = sequence.GetNextNumber(Context, conn, transaction);
                        }
                        oBundles.qty = oBundles.items.Sum(e => e.ismeter ? 1 : e.iqty);
                        Insert(transaction, conn, oBundles);
                        if (Services.ConfigManager.Instance(Context.OrganizationID).Config.intray_notification_inward)
                        {
                            Services.IntrayManager.Instance(Context.OrganizationID).Add(Context.OrganizationID, Services.IntrayManager.EntryType.Inward,
                                    oBundles.id, string.Format("NEW BUNDLE CREATED REFERENCE NO {0} ", oBundles.code),
                                    Context.UserID, oBundles.tolocationid, transaction, Context);
                        }
                        var command = GetCommand(@"update public.bundles set items=@items,itemsvalue=@itemsvalue,requestcode=@requestcode where id=@id", conn, transaction);
                        command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBundles.id;
                        command.Parameters.Add("requestcode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBundles.requestcode;
                        command.Parameters.Add("items", NpgsqlTypes.NpgsqlDbType.Json).Value = oBundles.items_json;
                        command.Parameters.Add("itemsvalue", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBundles.items.Sum(x => x.qty * x.buyingprice);

                        command.ExecuteNonQuery();
                        history.Add(new StockOrderHistory()
                        {
                            id = oBundles.id,
                            stocode = oBundles.code,
                            remark = oBundles.remarks,
                            stouniquenumber = oBundles.requestcode,
                            amount = oBundles.items.Sum(x => x.qty * x.price),
                            qty = oBundles.items.Sum(x => x.qty)
                        });
                    }
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, pouploaditem);
            }
            return history;
        }

        public Bundles UpdateStatus(Bundles oBundles)
        {
            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = GetCommand("Update bundles set status=@status,actionby=@actionby,actionon=@actionon,packedon=@packedon where id=@id", conn, transaction);
                    if (oBundles.bundleitemchanged)
                    {
                        command = GetCommand("Update bundles set status=@status,actionby=@actionby,actionon=@actionon,items=@items,packedon=@packedon where id=@id", conn, transaction);
                        command.Parameters.Add("items", NpgsqlTypes.NpgsqlDbType.Json).Value = oBundles.items_json;
                    }
                    command.Parameters.Add("status", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBundles.status;
                    command.Parameters.Add("actionby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                    command.Parameters.Add("actionon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
                    command.Parameters.Add("packedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBundles.packedon;
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBundles.id;
                    command.ExecuteNonQuery();
                    transaction.Commit();
                    conn.Close();
                }
                EmployeeDataService usersDataService = new EmployeeDataService() { Context = this.Context };
                var header = usersDataService.SearchHeader(new EmployeeCreteria() { id = Context.EmployeeID });
                oBundles.actionby = Context.UserID;
                oBundles.actionon = DateTime.Now;
                oBundles.actionemployee = header.Count > 0 ? header[0].name : "";
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oBundles);
            }
            return oBundles;
        }

        internal Bundles CreateBill(Bundles item)
        {
            var bundles = Search(new BundlesCreteria() { id = item.id });
            item = bundles[0];
            if (item.billid > 0)
                throw new Exception("Already bill created - " + item.billno);

            string query = @"select stock.serialno,
                                sum(salablegoods.iqty - (salablegoods.oqty+salablegoods.dqty+salablegoods.jqty)) stock 
                                from bundles
                                INNER JOIN LATERAL json_array_elements (case when 
                                bundles.items is null or bundles.items::text ='null' then '[]'::json  else bundles.items :: json end) itemlist ON true
                                inner join stock on stock.serialno = itemlist->>'serialno'
                                inner join salablegoods on salablegoods.stockid = stock.id and 
	                            salablegoods.locationid = bundles.fromlocationid and 
	                            (salablegoods.iqty - (salablegoods.oqty+salablegoods.dqty+salablegoods.jqty))>0 
	                            and salablegoods.isactive=true
                                where bundles.id = " + item.id + " group by stock.serialno";
            SortedDictionary<string, decimal> stocklist = new SortedDictionary<string, decimal>();
            using (var reader = ExecuteReader(query, null))
            {
                while (reader.Read())
                {
                    string serialno = Convert.ToString(reader["serialno"]);
                    decimal stock = Convert.ToDecimal(reader["stock"]);
                    stocklist.Add(serialno, stock);
                }
            }

            List<string> errorMessages = new List<string>();
            foreach (var st in stocklist)
            {
                var eqty = bundles[0].items.Sum(x => x.serialno == st.Key ? x.iqty : 0);
                if (st.Value < eqty)
                {
                    errorMessages.Add(st.Key + " Required " + eqty.ToString("F0") + " Available " + st.Value.ToString("F0"));
                }
            }

            if (errorMessages.Count > 0)
                throw new Exception(" Stock shortage " + String.Join(",", errorMessages));


            query = @"select stock.serialno,stock.barcode,salablegoods.id,
                                (salablegoods.iqty - (salablegoods.oqty+salablegoods.dqty+salablegoods.jqty)) stock 
                                from bundles
                                INNER JOIN LATERAL json_array_elements (case when 
                                bundles.items is null or bundles.items::text ='null' then '[]'::json  else bundles.items :: json end) itemlist ON true
                                inner join stock on stock.serialno = itemlist->>'serialno'
                                inner join salablegoods on salablegoods.stockid = stock.id and 
	                            salablegoods.locationid = bundles.fromlocationid and 
	                            (salablegoods.iqty - (salablegoods.oqty+salablegoods.dqty+salablegoods.jqty))>0 
	                            and salablegoods.isactive=true
                                where bundles.id = " + item.id;

            List<Bundleitem> stocklistbundleitem = new List<Bundleitem>();
            using (var reader = ExecuteReader(query, null))
            {
                while (reader.Read())
                {
                    Bundleitem bi = new Bundleitem();
                    bi.barcode = reader["barcode"].ToString();
                    bi.serialno = reader["serialno"].ToString();
                    bi.qty = Convert.ToDecimal(reader["stock"]);
                    bi.id = Convert.ToInt64(reader["id"]);
                    stocklistbundleitem.Add(bi);
                }
            }

            var existinglist = item.items.ToList();
            item.items = new List<Bundleitem>();
            existinglist.ForEach(x =>
            {
                var stockuanlist = stocklistbundleitem.FindAll(z => z.serialno == x.serialno && z.qty > 0).OrderByDescending(y => y.qty).ToList();
                var required = x.iqty;
                foreach (var suk in stockuanlist)
                {
                    var newbundleitem = JsonConvert.DeserializeObject<Bundleitem>(x.ToJSON());
                    if (suk.qty >= required)
                    {
                        newbundleitem.iqty = required;
                        newbundleitem.qty = required;
                        newbundleitem.barcode = suk.barcode;
                        newbundleitem.id = suk.id;
                        suk.qty -= newbundleitem.qty;
                        item.items.Add(newbundleitem);
                        required -= newbundleitem.qty;
                        break;
                    }
                    else
                    {
                        newbundleitem.iqty = suk.qty;
                        newbundleitem.qty = suk.qty;
                        newbundleitem.barcode = suk.barcode;
                        newbundleitem.id = suk.id;
                        suk.qty -= newbundleitem.qty;
                        required -= newbundleitem.qty;
                        item.items.Add(newbundleitem);
                    }
                }

                if (required > 0)
                    errorMessages.Add(x.serialno + " Required " + x.iqty.ToString("F0") + " Available " + (x.iqty - required).ToString("F0"));
            });


            if (errorMessages.Count > 0)
                throw new Exception(" Stock shortage " + String.Join(",", errorMessages));

            using (NpgsqlConnection conn = CreateConnection())
            {
                conn.Open();
                var transaction = conn.BeginTransaction();
                var bill = DoCompanyIntertransfer(bundles[0], conn, transaction);
                var command = GetCommand(@"update bundles set billno=@billno,billid=@billid,items=@items,status=3,itemsvalue=@itemsvalue where id=@id", conn, transaction);
                command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = bundles[0].id;
                command.Parameters.Add("billid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = bundles[0].billid;
                command.Parameters.Add("billno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = bundles[0].billno;
                command.Parameters.Add("items", NpgsqlTypes.NpgsqlDbType.Json).Value = bundles[0].items_json;
                command.Parameters.Add("itemsvalue", NpgsqlTypes.NpgsqlDbType.Numeric).Value = bill.total;
                command.ExecuteNonQuery();
                transaction.Commit();
                conn.Close();
            }
            return bundles[0];
        }

        public class UANCompanyTransferItem
        {
            public long fromlocationid { get; set; }
            public long tolocationid { get; set; }
            public long stockid { get; set; }
            public long fromsalablegoodsid { get; set; }
            public long tosalablegoodsid { get; set; }
            public long tocompanyid { get; set; }
            public decimal qty { get; set; }
        }
        internal Bundles InwardUANSTOBill(Bundles item)
        {
            if (item == null || item.id == 0) return null;
            if (item.invoiceid > 0) throw new Exception("Bill already created - " + item.billno);

            item = Search(new BundlesCreteria() { id = item.id }).First();
            if (item.status != 3 && item.invoiceid >0 )
            {
                throw new Exception("Already it may be inwarded to this location");
            }

            if (item.invoiceid > 0)
                throw new Exception("This invoice already imported LR Entry no " + item.lrentryno);

            //string query = @"select 
            //                fromlocationid,tolocationid,
            //                stock.id stockid,stock.barcode,
            //                (itemlist ->> 'aqty') aqty,
            //                (itemlist ->> 'id') id,
            //                fromlocation.id fromsalablegoodsid,
            //                tolocation.id tosalablegoodsid,
            //                tocompanyid
            //                from bundles 
            //                INNER JOIN LATERAL json_array_elements (case when 
            //                bundles.items is null or bundles.items::text ='null' then '[]'::json 
            //                else bundles.items :: json end) itemlist ON true
            //                inner join stock on stock.id = cast(itemlist->> 'stockid' as bigint) and stock.isactive = true
            //                left outer join salablegoods fromlocation on fromlocation.stockid = stock.id and fromlocation.locationid = fromlocationid
            //                left outer join salablegoods tolocation on tolocation.stockid = stock.id and tolocation.locationid = tolocationid
            //                where bundles.id = @id";

            //List<UANCompanyTransferItem> items = new List<UANCompanyTransferItem>();
            //var param = new SortedList<string, object>();
            //param.Add("id", item.id);
            //using (var reader = ExecuteReader(query, param))
            //{
            //    while (reader.Read())
            //    {
            //        UANCompanyTransferItem uanitem = new UANCompanyTransferItem();
            //        uanitem.fromsalablegoodsid = reader["fromsalablegoodsid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["fromsalablegoodsid"]);
            //        uanitem.fromlocationid = reader["fromlocationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["fromlocationid"]);
            //        uanitem.stockid = reader["stockid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["stockid"]);
            //        uanitem.tolocationid = reader["tolocationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["tolocationid"]);
            //        uanitem.tocompanyid = reader["tocompanyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["tocompanyid"]);
            //        uanitem.tosalablegoodsid = reader["tosalablegoodsid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["tosalablegoodsid"]);
            //        uanitem.qty = reader["aqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["aqty"]);
            //        items.Add(uanitem);
            //    }
            //}

            //if (item.tocompanyid == 0)
            //{
            //    ReferencelistDataService referencelistDataService = new ReferencelistDataService() { Context = this.Context };
            //    var location = referencelistDataService.Search(new ReferencelistCreteria() { id = item.tolocationid }).First();
            //    items.ForEach(x => x.tocompanyid = location.additionalinfo.parentid);
            //}

            using (NpgsqlConnection conn = CreateConnection())
            {
                conn.Open();
                var transaction = conn.BeginTransaction();

                ImportBills(item, conn, transaction);
                //throw new Exception("can'save");
                //SalablegoodsDataService salablegoodsDataService = new SalablegoodsDataService() { Context = this.Context, isMigration = true };
                //foreach (var salgood in items.FindAll(x => x.tosalablegoodsid == 0))
                //{
                //    var source = salablegoodsDataService.Search(new SalablegoodsCreteria() { id = salgood.fromsalablegoodsid }).First();
                //    source.id = 0;
                //    source.iqty = 0;
                //    source.oqty = 0;
                //    source.dqty = 0;
                //    source.jqty = 0;
                //    source.aqty = 0;
                //    source.locationid = salgood.tolocationid;
                //    source.companyid = salgood.tocompanyid;
                //    salablegoodsDataService.Insert(transaction, conn, source);
                //    salgood.tosalablegoodsid = source.id;
                //}
                //foreach (var salgood in items)
                //{
                //    if (item.items.Exists(x => x.id == salgood.fromsalablegoodsid && x.aqty == salgood.qty))
                //    {
                //        item.items.Find(x => x.id == salgood.fromsalablegoodsid && x.aqty == salgood.qty).toid = salgood.tosalablegoodsid;
                //        param = new SortedList<string, object>();
                //        param.Add("iqty", salgood.qty);
                //        param.Add("id", salgood.tosalablegoodsid);
                //        query = "Update salablegoods set iqty=iqty+@iqty where id=@id";
                //        ExecuteQuery(query, param, conn, transaction);
                //    }
                //}
                NpgsqlCommand command = GetCommand(@"update public.bundles set items=@items,status=1,modifiedon=@modifiedon,modifiedby=@modifiedby,version=version+1 where id=@id", conn, transaction);
                command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.id;
                command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
                command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                command.Parameters.Add("items", NpgsqlTypes.NpgsqlDbType.Json).Value = item.items_json;
                command.ExecuteNonQuery();
                transaction.Commit();
                conn.Close();
                item.status = 1;
            }
            return item;
        }

        internal Bundles UpdatePackingSlip(BundlesCreteria item)
        {
            if (item.packslip.Exists(x => x.code == item.packingcode))
            {
                var pack = item.packslip.Find(x => x.code == item.packingcode);
                if (pack.items == null || pack.items.Sum(x => x.aqty) == 0)
                {
                    if (item.status != 0)
                    {
                        if (pack.createdby == 0)
                        {
                            pack.createdby = Context.UserID;
                            pack.createdon = DateTimeNow;
                        }
                        pack.modifiedby = Context.UserID;
                        pack.modifiedon = DateTimeNow;
                    }
                }
                else if (item.status == 0)
                {
                    pack.inwardby = Context.UserID;
                    pack.inwardon = DateTimeNow;
                }
            }
            using (NpgsqlConnection conn = CreateConnection())
            {
                conn.Open();
                var transaction = conn.BeginTransaction();
                NpgsqlCommand command = GetCommand(@"update public.bundles set packslip=@packslip,modifiedon=@modifiedon,modifiedby=@modifiedby,version=version+1 where id=@id", conn, transaction);
                command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.id;
                command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
                command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                command.Parameters.Add("packslip", NpgsqlTypes.NpgsqlDbType.Json).Value = item.packslip_json;
                command.ExecuteNonQuery();
                transaction.Commit();
                conn.Close();
                item.status = 1;
            }
            return item;
        }
    }

}
